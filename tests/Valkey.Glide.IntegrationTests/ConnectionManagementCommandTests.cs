// Copyright Valkey GLIDE Project Contributors - SPDX Identifier: Apache-2.0

using System.Diagnostics;

using Valkey.Glide.Commands.Options;
using Valkey.Glide.TestUtils;

using static Valkey.Glide.TestUtils.Builders;

namespace Valkey.Glide.IntegrationTests;

/// <summary>
/// Tests for connection management commands.
/// </summary>
[Collection(typeof(ConnectionManagementCommandTests))]
[CollectionDefinition(DisableParallelization = true)]
public class ConnectionManagementCommandTests(ServerFixture fixture) : IClassFixture<ServerFixture>
{
    #region ClientInfoAsync

    [Theory]
    [MemberData(nameof(Data.ClusterMode), MemberType = typeof(Data))]
    public async Task ClientInfoAsync_Succeeds(bool clusterMode)
    {
        var name = "CLIENT-NAME";
        await using var client = await fixture.GetServer(clusterMode).CreateClientAsync(name);

        var infos = client is GlideClusterClient clusterClient
            ? (await clusterClient.ClientInfoAsync()).MultiValue.Values.ToArray()
            : [await ((GlideClient)client).ClientInfoAsync()];

        Assert.NotEmpty(infos);
        foreach (var info in infos)
        {
            /// See <see cref="Glide.Test.UnitTests.ConnectionManagementCommandTests"/>
            /// for comprehensive command converter unit tests.
            Assert.Equal(name, info.Name);
        }
    }

    #endregion
    #region ClientKillAsync

    [Theory]
    [MemberData(nameof(Data.ClusterMode), MemberType = typeof(Data))]
    public async Task ClientKillAsync_ByAddress_NonExistentAddress_ReturnsZero(bool clusterMode)
    {
        await using var client = await fixture.GetServer(clusterMode).CreateClientAsync();
        Assert.Equal(0, await client.ClientKillAsync(new ClientFilterOptions().WithAddress("192.0.2.1", 9999)));
    }

    [Theory]
    [MemberData(nameof(Data.ClusterMode), MemberType = typeof(Data))]
    public async Task ClientKillAsync_ByAddress_KillsClient(bool clusterMode)
    {
        await using var client = await fixture.GetServer(clusterMode).CreateClientAsync();

        var info = client is GlideClusterClient clusterClient
            ? (await clusterClient.ClientInfoAsync()).MultiValue.Values.First()
            : await ((GlideClient)client).ClientInfoAsync();
        var (host, port) = info.Address;

        var options = new ClientFilterOptions().WithAddress(host, port).WithSkipMe(false);
        Assert.Equal(1, await client.ClientKillAsync(options));
    }

    // In Valkey, client IDs are only unique per-server. As a result, we only test killing
    // clients by ID for standalone clients, since calls to ClientIdAsync() on cluster clients
    // are routed to all nodes and so could unexpectedly kill other clients.

    [Fact]
    public async Task ClientKillAsync_ById_NonExistentId_ReturnsZero()
    {
        await using var client = await fixture.StandaloneServer.CreateClientAsync();
        Assert.Equal(0, await client.ClientKillAsync(new ClientFilterOptions().WithId(999999999)));
    }

    [Fact]
    public async Task ClientKillAsync_ById_KillsClient()
    {
        await using var client = await fixture.StandaloneServer.CreateClientAsync();

        // TODO #519: ClientIdAsync on standalone is fine
#pragma warning disable CS0618
        var id = await client.ClientIdAsync();
#pragma warning restore CS0618
        var options = new ClientFilterOptions().WithId(id).WithSkipMe(false);
        Assert.Equal(1, await client.ClientKillAsync(options));
    }

    #endregion
    #region ClientListAsync

    [Theory]
    [MemberData(nameof(Data.ClusterMode), MemberType = typeof(Data))]
    public async Task ClientListAsync_ReturnsAllAddresses(bool clusterMode)
    {
        var server = fixture.GetServer(clusterMode);
        await using var client = await server.CreateClientAsync();

        var infos = await GetClientInfos(client);

        foreach (var address in server.Addresses)
        {
            Assert.Contains(infos, c => c.LocalAddress == address);
        }
    }

    [Theory]
    [MemberData(nameof(Data.ClusterMode), MemberType = typeof(Data))]
    public async Task ClientListAsync_WithAddressFilter_ReturnsAddress(bool clusterMode)
    {
        var server = fixture.GetServer(clusterMode);
        await using var client = await server.CreateClientAsync();

        var address = server.Address;
        var options = new ClientFilterOptions().WithLocalAddress(address.Host, address.Port);
        var infos = await GetClientInfos(client, options);

        Assert.NotEmpty(infos);
        foreach (var info in infos)
        {
            Assert.Equal(address, info.LocalAddress);
        }
    }

    #endregion
    #region ClientPauseAsync / ClientUnpauseAsync

    [Theory]
    [MemberData(nameof(Data.ClusterMode), MemberType = typeof(Data))]
    public async Task TestClientPause_ReadsPausedUntilExpires(bool clusterMode)
    {
        // Request timeout must be longer than the pause duration.
        var pauseFor = TimeSpan.FromSeconds(1);
        var requestTimeout = pauseFor + TimeSpan.FromSeconds(1);

        await using BaseClient client = clusterMode
            ? await GlideClusterClient.CreateClient(
                fixture.ClusterServer.CreateConfigBuilder()
                    .WithRequestTimeout(requestTimeout)
                    .Build())
            : await GlideClient.CreateClient(
                fixture.StandaloneServer.CreateConfigBuilder()
                    .WithRequestTimeout(requestTimeout)
                    .Build());

        var key = Guid.NewGuid().ToString();
        await client.SetAsync(key, "value");

        var sw = Stopwatch.StartNew();
        await client.ClientPauseAsync(pauseFor);

        // Verify that read commands are blocked until the pause expires.
        _ = await client.GetAsync(key);
        Assert.True(sw.Elapsed >= pauseFor);

        await client.ClientUnpauseAsync();
    }

    [Theory]
    [MemberData(nameof(Data.ClusterMode), MemberType = typeof(Data))]
    public async Task TestClientPause_WritesPausedUntilExpires(bool clusterMode)
    {
        // Request timeout must be longer than the pause duration.
        var pauseFor = TimeSpan.FromSeconds(1);
        var requestTimeout = pauseFor + TimeSpan.FromSeconds(1);

        await using BaseClient client = clusterMode
            ? await GlideClusterClient.CreateClient(
                fixture.ClusterServer.CreateConfigBuilder()
                    .WithRequestTimeout(requestTimeout)
                    .Build())
            : await GlideClient.CreateClient(
                fixture.StandaloneServer.CreateConfigBuilder()
                    .WithRequestTimeout(requestTimeout)
                    .Build());

        var key = Guid.NewGuid().ToString();
        await client.SetAsync(key, "before");

        var sw = Stopwatch.StartNew();
        await client.ClientPauseAsync(pauseFor);

        // Verify that write commands are blocked until the pause expires.
        await client.SetAsync(key, "after");
        Assert.True(sw.Elapsed >= pauseFor);

        await client.ClientUnpauseAsync();
    }

    [Theory]
    [MemberData(nameof(Data.ClusterMode), MemberType = typeof(Data))]
    public async Task TestClientPauseWrite_ReadsNotPaused(bool clusterMode)
    {
        await using var client = await fixture.GetServer(clusterMode).CreateClientAsync();

        var key = Guid.NewGuid().ToString();
        await client.SetAsync(key, "before");

        var pauseFor = TimeSpan.FromMinutes(1);
        await client.ClientPauseWriteAsync(pauseFor);

        var sw = Stopwatch.StartNew();

        // Verify that read commands are not blocked.
        Assert.Equal("before", await client.GetAsync(key));
        Assert.True(sw.Elapsed < pauseFor);

        await client.ClientUnpauseAsync();
    }

    [Theory]
    [MemberData(nameof(Data.ClusterMode), MemberType = typeof(Data))]
    public async Task TestClientPauseWrite_ThenUnpause(bool clusterMode)
    {
        await using var client = await fixture.GetServer(clusterMode).CreateClientAsync();

        var key = Guid.NewGuid().ToString();
        await client.SetAsync(key, "before");

        var pausedFor = TimeSpan.FromMinutes(1);
        await client.ClientPauseWriteAsync(pausedFor);

        var sw = Stopwatch.StartNew();

        // Verify that write commands are unblocked once unpaused.
        await client.ClientUnpauseAsync();
        await client.SetAsync(key, "after");
        Assert.True(sw.Elapsed < pausedFor);
    }

    #endregion
    #region ClientTrackingInfoAsync

    [Theory]
    [MemberData(nameof(Data.ClusterMode), MemberType = typeof(Data))]
    public async Task ClientTrackingInfo_Off(bool clusterMode)
    {
        await using var client = await fixture.GetServer(clusterMode).CreateClientAsync();

        var info = await client.ClientTrackingInfoAsync();
        AssertTrackingInfoOff(info);
    }

    [Fact]
    public async Task ClientTrackingInfo_Off_WithRoute()
    {
        await using GlideClusterClient client = await fixture.ClusterServer.CreateClusterClientAsync();

        var response = await client.ClientTrackingInfoAsync(Route.AllNodes);

        Assert.NotEmpty(response.MultiValue);
        foreach (var info in response.MultiValue.Values)
        {
            AssertTrackingInfoOff(info);
        }
    }

    [Theory]
    [MemberData(nameof(Data.ClusterMode), MemberType = typeof(Data))]
    public async Task ClientTrackingInfo_On(bool clusterMode)
    {
        var cache = BuildClientSideCacheConfig().WithServerAssisted();

        await using BaseClient client = clusterMode
            ? await GlideClusterClient.CreateClient(
                fixture.ClusterServer.CreateConfigBuilder()
                    .WithClientSideCache(cache)
                    .Build())
            : await GlideClient.CreateClient(
                fixture.StandaloneServer.CreateConfigBuilder()
                    .WithClientSideCache(cache)
                    .Build());

        AssertTrackingInfoOn(await client.ClientTrackingInfoAsync());
    }

    [Fact]
    public async Task ClientTrackingInfo_On_WithRoute()
    {
        var cache = BuildClientSideCacheConfig().WithServerAssisted();

        await using var client = await GlideClusterClient.CreateClient(
            fixture.ClusterServer.CreateConfigBuilder()
                .WithClientSideCache(cache)
                .Build());

        var response = await client.ClientTrackingInfoAsync(Route.AllNodes);

        Assert.NotEmpty(response.MultiValue);
        foreach (var multiInfo in response.MultiValue.Values)
        {
            AssertTrackingInfoOn(multiInfo);
        }
    }

    #endregion
    #region ResetAsync

    [Theory]
    [MemberData(nameof(Data.ClusterMode), MemberType = typeof(Data))]
    public async Task TestReset_ResetsConnectionState(bool clusterMode)
    {
        var cache = BuildClientSideCacheConfig().WithServerAssisted();

        await using BaseClient client = clusterMode
            ? await GlideClusterClient.CreateClient(
                fixture.ClusterServer.CreateConfigBuilder()
                    .WithClientSideCache(cache)
                    .Build())
            : await GlideClient.CreateClient(
                fixture.StandaloneServer.CreateConfigBuilder()
                    .WithClientSideCache(cache)
                    .Build());

        // Verify tracking is enabled.
        var infoBefore = await client.ClientTrackingInfoAsync();
        Assert.Contains("on", infoBefore.Flags);

        await client.ResetAsync();

        // Verify tracking is disabled after reset.
        var infoAfter = await client.ClientTrackingInfoAsync();
        Assert.Contains("off", infoAfter.Flags);
    }

    #endregion
    #region Helpers

    private static void AssertTrackingInfoOff(ClientTrackingInfo info)
    {
        Assert.Equivalent(new HashSet<string> { "off" }, info.Flags);
        Assert.Equal(-1, info.Redirect);
        Assert.Empty(info.Prefixes);
    }

    private static void AssertTrackingInfoOn(ClientTrackingInfo info)
    {
        Assert.Equivalent(new HashSet<string> { "on", "bcast" }, info.Flags);
        Assert.Equal(0, info.Redirect);
        Assert.Equivalent(new HashSet<string> { "" }, info.Prefixes);
    }

    /// <summary>
    /// Returns all client infos for the given client.
    /// </summary>
    private static async Task<ClientInfo[]> GetClientInfos(BaseClient client, ClientFilterOptions? options = null)
    {
        if (client is GlideClusterClient clusterClient)
        {
            var result = options is null
                ? await clusterClient.ClientListAsync()
                : await clusterClient.ClientListAsync(options);
            return [.. result.MultiValue.Values.SelectMany(static c => c)];
        }

        var standaloneClient = (GlideClient)client;
        return options is null
            ? await standaloneClient.ClientListAsync()
            : await standaloneClient.ClientListAsync(options);
    }

    #endregion
}

// Copyright Valkey GLIDE Project Contributors - SPDX Identifier: Apache-2.0

namespace Valkey.Glide.UnitTests;

public class ConnectionManagementCommandTests
{
    #region ClientInfoAsync

    [Fact]
    public void ClientInfo_GetArgs()
        => Assert.Equal(["CLIENTINFO"], Request.ClientInfo().GetArgs());

    [Fact]
    public void ConvertClientInfoResponse_AllFields()
    {
        var info = Request.ConvertClientInfoResponse("id=42 addr=127.0.0.1:6379 laddr=127.0.0.1:6380 fd=5 name=myconn age=10 idle=2 flags=SP db=3 sub=1 psub=2 ssub=3 multi=4 watch=5 qbuf=100 qbuf-free=200 argv-mem=300 multi-mem=400 obl=50 oll=60 omem=70 tot-mem=500 events=rw cmd=get user=admin redir=99 resp=3 lib-name=GlideC# lib-ver=1.0.0 tot-net-in=600 tot-net-out=700 tot-cmds=800 capa=r\n");

        Assert.Equal(("127.0.0.1", (ushort)6379), info.Address);
        Assert.Equal(TimeSpan.FromSeconds(10), info.Age);
        Assert.Equal(300UL, info.ArgvMemory);
        Assert.Equal((ushort)3, info.Database);
        Assert.Equivalent(new[] { FileDescriptorEvent.Readable, FileDescriptorEvent.Writable }, info.Events);
        Assert.Equal(5U, info.FileDescriptor);
        Assert.Equivalent(new[] { ClientFlag.Replica, ClientFlag.PubSub }, info.Flags);
        Assert.Equal(42UL, info.Id);
        Assert.Equal(TimeSpan.FromSeconds(2), info.Idle);
        Assert.Equal("get", info.LastCommand);
        Assert.Equal(("127.0.0.1", (ushort)6380), info.LocalAddress);
        Assert.Equal(400UL, info.MultiMemory);
        Assert.Equal("myconn", info.Name);
        Assert.Equal(50UL, info.OutputBufferLength);
        Assert.Equal(70UL, info.OutputBufferMemory);
        Assert.Equal(60UL, info.OutputListLength);
        Assert.Equal(2U, info.PatternSubscriptionCount);
        Assert.Equal(100UL, info.QueryBuffer);
        Assert.Equal(200UL, info.QueryBufferFree);
        Assert.Equal(99UL, info.Redirect);
        Assert.Equal(3U, info.ShardedSubscriptionCount);
        Assert.Equal(1U, info.SubscriptionCount);
        Assert.Equal(500UL, info.TotalMemory);
        Assert.Equal(4U, info.TransactionCommandLength);
        Assert.Equal("admin", info.User);

        // Since Valkey 7.0
        Assert.Equal(Protocol.Resp3, info.Protocol);

        // Since Valkey 7.2
        Assert.Equal("GlideC#", info.LibraryName);
        Assert.Equal("1.0.0", info.LibraryVersion);

        // Since Valkey 8.0
        Assert.Equal(800UL, info.TotalCommands);
        Assert.Equal(600UL, info.TotalNetInput);
        Assert.Equal(700UL, info.TotalNetOutput);
        Assert.Equal(5U, info.WatchedKeyCount);

        // Since Valkey 8.1
        Assert.NotNull(info.Capabilities);
        Assert.Contains(ClientCapability.Redirect, info.Capabilities);
    }

    [Fact]
    public void ConvertClientInfoResponse_WithoutOptionalFields()
    {
        var response = Request.ConvertClientInfoResponse("id=1 addr=127.0.0.1:6379 laddr=127.0.0.1:6380 fd=1 name= age=0 idle=0 flags=N db=0 sub=0 psub=0 ssub=0 multi=-1 qbuf=0 qbuf-free=0 argv-mem=0 multi-mem=0 obl=0 oll=0 omem=0 tot-mem=0 events=r cmd=client user=default redir=-1\n");

        // Since Valkey 7.0
        Assert.Null(response.Protocol);

        // Since Valkey 7.2
        Assert.Null(response.LibraryName);
        Assert.Null(response.LibraryVersion);

        // Since Valkey 8.0
        Assert.Null(response.TotalCommands);
        Assert.Null(response.TotalNetInput);
        Assert.Null(response.TotalNetOutput);
        Assert.Null(response.WatchedKeyCount);

        // Since Valkey 8.1
        Assert.Null(response.Capabilities);
    }

    #endregion
    #region ClientListAsync

    [Fact]
    /// <seealso cref="ClientFilterOptionsTests"/>
    public void ClientList_GetArgs()
    {
        Assert.Equal(["CLIENTLIST"], Request.ClientList().GetArgs());
        Assert.Equal(["CLIENTLIST", "TYPE", "normal"], Request.ClientList(new Commands.Options.ClientFilterOptions().WithType(ClientType.Normal)).GetArgs());
    }

    [Fact]
    public void ConvertClientListResponse_EmptyClients()
        => Assert.Empty(Request.ConvertClientListResponse(""));

    [Fact]
    /// <seealso cref="ParseClientInfoResponse_AllFields"/>
    /// <seealso cref="ParseClientInfoResponse_WithoutOptionalFields"/>
    public void ConvertClientListResponse_SingleClient()
    {
        var infos = Request.ConvertClientListResponse("id=1 addr=127.0.0.1:6379 laddr=127.0.0.1:6380 fd=5 name=1 age=0 idle=0 flags=N db=0 sub=0 psub=0 ssub=0 multi=-1 qbuf=0 qbuf-free=0 argv-mem=0 multi-mem=0 obl=0 oll=0 omem=0 tot-mem=0 events=r cmd=client user=default redir=-1 resp=2\n");

        var info = Assert.Single(infos);
        Assert.Equal(1UL, info.Id);
        Assert.Equal("1", info.Name);
    }

    [Fact]
    public void ConvertClientListResponse_MultipleClients()
    {
        var infos = Request.ConvertClientListResponse(
            "id=1 addr=127.0.0.1:6379 laddr=127.0.0.1:6380 fd=5 name=1 age=0 idle=0 flags=N db=0 sub=0 psub=0 ssub=0 multi=-1 qbuf=0 qbuf-free=0 argv-mem=0 multi-mem=0 obl=0 oll=0 omem=0 tot-mem=0 events=r cmd=client user=default redir=-1 resp=2\n" +
            "id=2 addr=127.0.0.1:6381 laddr=127.0.0.1:6380 fd=6 name=2 age=5 idle=1 flags=N db=1 sub=0 psub=0 ssub=0 multi=-1 qbuf=0 qbuf-free=0 argv-mem=0 multi-mem=0 obl=0 oll=0 omem=0 tot-mem=0 events=r cmd=ping user=admin redir=-1 resp=3\n");

        Assert.Equal(2, infos.Length);
        Assert.Equal(1UL, infos[0].Id);
        Assert.Equal(2UL, infos[1].Id);
        Assert.Equal("1", infos[0].Name);
        Assert.Equal("2", infos[1].Name);
    }

    #endregion
    #region ParseClientFlags

    [Fact]
    public void ParseClientFlags_Empty()
        => Assert.Empty(Request.ParseClientFlags(""));

    [Theory]
    [InlineData('A', ClientFlag.CloseAsap)]
    [InlineData('b', ClientFlag.Blocked)]
    [InlineData('c', ClientFlag.CloseAfterReply)]
    [InlineData('d', ClientFlag.DirtyExec)]
    [InlineData('e', ClientFlag.NoEvict)]
    [InlineData('I', ClientFlag.ImportSource)]
    [InlineData('M', ClientFlag.Primary)]
    [InlineData('N', ClientFlag.None)]
    [InlineData('O', ClientFlag.Monitor)]
    [InlineData('P', ClientFlag.PubSub)]
    [InlineData('r', ClientFlag.ReadOnly)]
    [InlineData('R', ClientFlag.TrackingTargetInvalid)]
    [InlineData('S', ClientFlag.Replica)]
    [InlineData('t', ClientFlag.Tracking)]
    [InlineData('T', ClientFlag.NoTouch)]
    [InlineData('u', ClientFlag.Unblocked)]
    [InlineData('U', ClientFlag.UnixSocket)]
    [InlineData('x', ClientFlag.Multi)]
    [InlineData('B', ClientFlag.BroadcastTracking)]
    public void ParseClientFlags_SingleValue(char c, ClientFlag expected)
        => Assert.Equal(expected, Assert.Single(Request.ParseClientFlags(c.ToString())));

    [Fact]
    public void ParseClientFlags_MultipleValues()
        => Assert.Equivalent(
            new[] { ClientFlag.Replica, ClientFlag.PubSub, ClientFlag.Multi },
            Request.ParseClientFlags("SPx"));

    [Fact]
    public void ParseClientFlags_UnknownValues_Skipped()
        => Assert.Empty(Request.ParseClientFlags("Z!"));

    #endregion
    #region ParseClientCapabilities

    [Fact]
    public void ParseClientCapabilities_Empty()
        => Assert.Empty(Request.ParseClientCapabilities(""));

    [Theory]
    [InlineData('r', ClientCapability.Redirect)]
    public void ParseClientCapabilities_SingleValue(char c, ClientCapability expected)
        => Assert.Equal(expected, Assert.Single(Request.ParseClientCapabilities(c.ToString())));

    [Fact]
    public void ParseClientCapabilities_UnknownValues_Skipped()
        => Assert.Empty(Request.ParseClientCapabilities("Z!"));

    #endregion
    #region ParseFileDescriptorEvents

    [Fact]
    public void ParseFileDescriptorEvents_Empty()
        => Assert.Empty(Request.ParseFileDescriptorEvents(""));

    [Theory]
    [InlineData('r', FileDescriptorEvent.Readable)]
    [InlineData('w', FileDescriptorEvent.Writable)]
    public void ParseFileDescriptorEvents_SingleValue(char c, FileDescriptorEvent expected)
        => Assert.Equal(expected, Assert.Single(Request.ParseFileDescriptorEvents(c.ToString())));

    [Fact]
    public void ParseFileDescriptorEvents_MultipleValues()
        => Assert.Equivalent(
            new[] { FileDescriptorEvent.Readable, FileDescriptorEvent.Writable },
            Request.ParseFileDescriptorEvents("rw"));

    [Fact]
    public void ParseFileDescriptorEvents_UnknownValues_Skipped()
        => Assert.Empty(Request.ParseFileDescriptorEvents("Z!"));

    #endregion
    #region ParseProtocol

    [Theory]
    [InlineData("2", Protocol.Resp2)]
    [InlineData("3", Protocol.Resp3)]
    public void ParseProtocol_ValidValue(string value, Protocol expected)
        => Assert.Equal(expected, Request.ParseProtocol(value));

    [Theory]
    [InlineData("")]
    [InlineData("1")]
    [InlineData("4")]
    [InlineData("invalid")]
    public void ParseProtocol_InvalidValue(string value)
        => Assert.Null(Request.ParseProtocol(value));

    #endregion
}

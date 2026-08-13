// Copyright Valkey GLIDE Project Contributors - SPDX Identifier: Apache-2.0

using Valkey.Glide.Commands.Options;

namespace Valkey.Glide;

/// ATTENTION: Methods should only be added to this interface if they are implemented
/// by Valkey GLIDE clients but NOT by StackExchange.Redis databases. Methods implemented
/// by both should be added to <see cref="Commands.IConnectionManagementBaseCommands"/> instead.

/// <summary>
/// Connection management commands for Valkey GLIDE cluster client.
/// </summary>
/// <seealso href="https://valkey.io/commands/#connection">Valkey – Connection Management Commands</seealso>
public partial interface IGlideClusterClient
{
    /// <summary>
    /// Gets the name of the current connection.
    /// </summary>
    /// <seealso href="https://valkey.io/commands/client-getname/">Valkey commands – CLIENT GETNAME</seealso>
    /// <param name="route">Specifies the routing configuration for the command.</param>
    /// <returns>A <see cref="ClusterValue{T}"/> containing the connection names.</returns>
    /// <remarks>
    /// <example>
    /// <code>
    /// var name = (await clusterClient.ClientGetNameAsync(Route.Random)).SingleValue;
    /// Console.WriteLine($"Connection name: {name}");
    /// </code>
    /// </example>
    /// </remarks>
    Task<ClusterValue<ValkeyValue>> ClientGetNameAsync(Route route);

    /// <summary>
    /// Gets the connection IDs for the specified route.
    /// </summary>
    /// <seealso href="https://valkey.io/commands/client-id/">Valkey commands – CLIENT ID</seealso>
    /// <param name="route">Specifies the routing configuration for the command.</param>
    /// <returns>A <see cref="ClusterValue{T}"/> containing the connection IDs.</returns>
    /// <remarks>
    /// <example>
    /// <code>
    /// var ids = await clusterClient.ClientIdAsync(Route.AllPrimaries);
    /// foreach (var (node, id) in ids.MultiValue)
    ///     Console.WriteLine($"{node}: {id}");
    /// </code>
    /// </example>
    /// </remarks>
    Task<ClusterValue<long>> ClientIdAsync(Route route);

    /// <summary>
    /// Returns information about the current client connection to the server on all nodes.
    /// </summary>
    /// <seealso href="https://valkey.io/commands/client-info/">Valkey commands – CLIENT INFO</seealso>
    /// <returns>A <see cref="ClusterValue{T}"/> containing the client info per node.</returns>
    /// <remarks>
    /// <example>
    /// <code>
    /// var info = (await clusterClient.ClientInfoAsync()).MultiValue;
    /// foreach (var (node, ci) in info)
    ///     Console.WriteLine($"{node}: {ci.Name}");
    /// </code>
    /// </example>
    /// </remarks>
    Task<ClusterValue<ClientInfo>> ClientInfoAsync();

    /// <summary>
    /// Returns information about all client connections to the server on all nodes.
    /// </summary>
    /// <seealso href="https://valkey.io/commands/client-list/">Valkey commands – CLIENT LIST</seealso>
    /// <returns>A <see cref="ClusterValue{T}"/> containing the connected clients per node.</returns>
    /// <remarks>
    /// <example>
    /// <code>
    /// var clients = (await clusterClient.ClientListAsync()).MultiValue;
    /// foreach (var (node, list) in clients)
    ///     Console.WriteLine($"{node}: {list.Length} clients");
    /// </code>
    /// </example>
    /// </remarks>
    Task<ClusterValue<ClientInfo[]>> ClientListAsync();

    /// <summary>
    /// Returns information about all client connections to the server
    /// on all nodes matching the given filter options.
    /// </summary>
    /// <seealso href="https://valkey.io/commands/client-list/">Valkey commands – CLIENT LIST</seealso>
    /// <param name="options">The filter options specifying which clients to return.</param>
    /// <returns>A <see cref="ClusterValue{T}"/> containing the matching clients per node.</returns>
    /// <remarks>
    /// <example>
    /// <code>
    /// var options = new ClientFilterOptions().WithType(ClientType.Normal);
    /// var clients = (await clusterClient.ClientListAsync(options)).MultiValue;
    /// </code>
    /// </example>
    /// </remarks>
    Task<ClusterValue<ClientInfo[]>> ClientListAsync(ClientFilterOptions options);

    /// <summary>
    /// Returns information about the current client connection's use of the
    /// server-assisted client-side caching feature.
    /// </summary>
    /// <seealso href="https://valkey.io/commands/client-trackinginfo/">Valkey commands – CLIENT TRACKINGINFO</seealso>
    /// <param name="route">Specifies the routing configuration for the command.</param>
    /// <returns>A <see cref="ClusterValue{T}" /> containing tracking states for this connection.</returns>
    /// <remarks>
    /// <example>
    /// <code>
    /// var info = (await clusterClient.ClientTrackingInfoAsync(Route.Random)).SingleValue;
    /// Console.WriteLine($"Flags: {string.Join(", ", info.Flags)}");  // "Flags: off"
    /// </code>
    /// </example>
    /// </remarks>
    Task<ClusterValue<ClientTrackingInfo>> ClientTrackingInfoAsync(Route route);

    /// <summary>
    /// Echoes the given message back from the server.
    /// </summary>
    /// <seealso href="https://valkey.io/commands/echo/">Valkey commands – ECHO</seealso>
    /// <param name="message">The message to echo.</param>
    /// <param name="route">Specifies the routing configuration for the command.</param>
    /// <returns>A <see cref="ClusterValue{T}"/> containing the echoed messages.</returns>
    /// <remarks>
    /// <example>
    /// <code>
    /// var echoed = (await clusterClient.EchoAsync("Hello World", Route.Random)).SingleValue;  // "Hello World"
    /// </code>
    /// </example>
    /// </remarks>
    Task<ClusterValue<ValkeyValue>> EchoAsync(ValkeyValue message, Route route);

    /// <summary>
    /// Pings the server.
    /// </summary>
    /// <seealso href="https://valkey.io/commands/ping/">Valkey commands – PING</seealso>
    /// <param name="route">Specifies the routing configuration for the command.</param>
    /// <returns>The server response (<c>"PONG"</c>).</returns>
    /// <remarks>
    /// <example>
    /// <code>
    /// var response = await clusterClient.PingAsync(Route.AllPrimaries);
    /// Console.WriteLine(response);  // "PONG"
    /// </code>
    /// </example>
    /// </remarks>
    Task<ValkeyValue> PingAsync(Route route);

    /// <summary>
    /// Pings the server with a message.
    /// </summary>
    /// <seealso href="https://valkey.io/commands/ping/">Valkey commands – PING</seealso>
    /// <param name="message">The message to send with the ping.</param>
    /// <param name="route">Specifies the routing configuration for the command.</param>
    /// <returns>The echoed message.</returns>
    /// <remarks>
    /// <example>
    /// <code>
    /// var response = await clusterClient.PingAsync("Hello World", Route.AllPrimaries);
    /// Console.WriteLine(response);  // "Hello World"
    /// </code>
    /// </example>
    /// </remarks>
    Task<ValkeyValue> PingAsync(ValkeyValue message, Route route);
}

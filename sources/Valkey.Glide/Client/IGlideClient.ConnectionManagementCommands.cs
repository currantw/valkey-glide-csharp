// Copyright Valkey GLIDE Project Contributors - SPDX Identifier: Apache-2.0

using Valkey.Glide.Commands.Options;

namespace Valkey.Glide;

/// ATTENTION: Methods should only be added to this interface if they are implemented
/// by Valkey GLIDE clients but NOT by StackExchange.Redis databases. Methods implemented
/// by both should be added to <see cref="Commands.IConnectionManagementBaseCommands"/> instead.

/// <summary>
/// Connection management commands for Valkey GLIDE standalone client.
/// </summary>
/// <seealso href="https://valkey.io/commands/#connection">Valkey – Connection Management Commands</seealso>
public partial interface IGlideClient
{
    /// <summary>
    /// Returns information about the current client connection to the server.
    /// </summary>
    /// <seealso href="https://valkey.io/commands/client-info/">Valkey commands – CLIENT INFO</seealso>
    /// <returns>A <see cref="ClientInfo"/> describing the current connection.</returns>
    /// <remarks>
    /// <example>
    /// <code>
    /// var info = await client.ClientInfoAsync();
    /// Console.WriteLine($"Client Name: {info.Name}");
    /// </code>
    /// </example>
    /// </remarks>
    Task<ClientInfo> ClientInfoAsync();

    /// <summary>
    /// Returns information about all client connections to the server.
    /// </summary>
    /// <seealso href="https://valkey.io/commands/client-list/">Valkey commands – CLIENT LIST</seealso>
    /// <returns>An array of <see cref="ClientInfo"/> describing the connected clients.</returns>
    /// <remarks>
    /// <example>
    /// <code>
    /// var clients = await client.ClientListAsync();
    /// foreach (var c in clients)
    ///     Console.WriteLine($"Client Name: {c.Name}");
    /// </code>
    /// </example>
    /// </remarks>
    Task<ClientInfo[]> ClientListAsync();

    /// <summary>
    /// Returns information about all client connections to the server matching the given filter options.
    /// </summary>
    /// <seealso href="https://valkey.io/commands/client-list/">Valkey commands – CLIENT LIST</seealso>
    /// <param name="options">The filter options specifying which clients to return.</param>
    /// <returns>An array of <see cref="ClientInfo"/> describing the matching clients.</returns>
    /// <remarks>
    /// <example>
    /// <code>
    /// var options = new ClientFilterOptions().WithType(ClientType.Normal);
    /// var clients = await client.ClientListAsync(options);
    /// foreach (var c in clients)
    ///     Console.WriteLine($"Client Name: {c.Name}");
    /// </code>
    /// </example>
    /// </remarks>
    Task<ClientInfo[]> ClientListAsync(ClientFilterOptions options);
}

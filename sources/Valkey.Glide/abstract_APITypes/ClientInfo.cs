// Copyright Valkey GLIDE Project Contributors - SPDX Identifier: Apache-2.0

namespace Valkey.Glide;

/// <summary>
/// Represents the state of an individual client connection to a Valkey server.
/// </summary>
/// <seealso href="https://valkey.io/commands/client-info/" />
/// <seealso href="https://valkey.io/commands/client-list/" />
public sealed record ClientInfo
{
    #region Public Properties

    /// <summary>
    /// The address of the client.
    /// </summary>
    public required (string Host, ushort Port) Address { get; init; }

    /// <summary>
    /// The total duration of the connection.
    /// </summary>
    public required TimeSpan Age { get; init; }

    /// <summary>
    /// The memory used by the command arguments (in bytes).
    /// </summary>
    public required ulong ArgvMemory { get; init; }

    /// <summary>
    /// The current database ID.
    /// </summary>
    public required ushort Database { get; init; }

    /// <summary>
    /// The file descriptor events the client is interested in.
    /// </summary>
    public required IReadOnlySet<FileDescriptorEvent> Events { get; init; }

    /// <summary>
    /// The file descriptor corresponding to the socket.
    /// </summary>
    public required uint FileDescriptor { get; init; }

    /// <summary>
    /// The client connection flags.
    /// </summary>
    public required IReadOnlySet<ClientFlag> Flags { get; init; }

    /// <summary>
    /// The unique client ID.
    /// </summary>
    public required ulong Id { get; init; }

    /// <summary>
    /// The idle time of the connection.
    /// </summary>
    public required TimeSpan Idle { get; init; }

    /// <summary>
    /// The last command played.
    /// </summary>
    public required string? LastCommand { get; init; }

    /// <summary>
    /// The local address the client is connected to.
    /// </summary>
    public required (string Host, ushort Port) LocalAddress { get; init; }

    /// <summary>
    /// The memory used by the transaction buffer (in bytes).
    /// </summary>
    public required ulong MultiMemory { get; init; }

    /// <summary>
    /// The name allocated to this connection,
    /// or <see langword="null"/> if not set.
    /// </summary>
    public required string? Name { get; init; }

    /// <summary>
    /// The output buffer length (in bytes).
    /// </summary>
    public required ulong OutputBufferLength { get; init; }

    /// <summary>
    /// The output list length (replies are queued in this list when the buffer is full).
    /// </summary>
    public required ulong OutputListLength { get; init; }

    /// <summary>
    /// The output buffer memory usage (in bytes).
    /// </summary>
    public required ulong OutputBufferMemory { get; init; }

    /// <summary>
    /// The number of pattern-matching subscriptions.
    /// </summary>
    public required uint PatternSubscriptionCount { get; init; }

    /// <summary>
    /// The query buffer length (in bytes).
    /// </summary>
    public required ulong QueryBuffer { get; init; }

    /// <summary>
    /// The free space of the query buffer (in bytes).
    /// </summary>
    public required ulong QueryBufferFree { get; init; }

    /// <summary>
    /// The client ID of the current client tracking redirection,
    /// or <see langword="null"/> if not redirecting.
    /// </summary>
    public required ulong? Redirect { get; init; }

    /// <summary>
    /// The number of sharded channel subscriptions.
    /// </summary>
    public required uint ShardedSubscriptionCount { get; init; }

    /// <summary>
    /// The number of channel subscriptions.
    /// </summary>
    public required uint SubscriptionCount { get; init; }

    /// <summary>
    /// The total memory consumed by this client (in bytes).
    /// </summary>
    public required ulong TotalMemory { get; init; }

    /// <summary>
    /// The number of commands in a transaction context,
    /// or <see langword="null"/> if not in a transaction.
    /// </summary>
    public required uint? TransactionCommandLength { get; init; }

    /// <summary>
    /// The authenticated username of the client,
    /// or <see langword="null"/> if not populated.
    /// </summary>
    public required string? User { get; init; }

    /// <summary>
    /// The RESP protocol version.
    /// </summary>
    /// <remarks>Since Valkey 7.0.0.</remarks>
    public Protocol? Protocol { get; init; }

    /// <summary>
    /// The client library name.
    /// </summary>
    /// <remarks>Since Valkey 7.2.0.</remarks>
    public string? LibraryName { get; init; }

    /// <summary>
    /// The client library version.
    /// </summary>
    /// <remarks>Since Valkey 7.2.0.</remarks>
    public string? LibraryVersion { get; init; }

    /// <summary>
    /// Total count of commands this client executed.
    /// </summary>
    /// <remarks>Since Valkey 8.0.0.</remarks>
    public ulong? TotalCommands { get; init; }

    /// <summary>
    /// Total network input bytes read from this client.
    /// </summary>
    /// <remarks>Since Valkey 8.0.0.</remarks>
    public ulong? TotalNetInput { get; init; }

    /// <summary>
    /// Total network output bytes sent to this client.
    /// </summary>
    /// <remarks>Since Valkey 8.0.0.</remarks>
    public ulong? TotalNetOutput { get; init; }

    /// <summary>
    /// The number of keys being watched by this client.
    /// </summary>
    /// <remarks>Since Valkey 8.0.0.</remarks>
    public uint? WatchedKeyCount { get; init; }

    /// <summary>
    /// The client capabilities.
    /// </summary>
    /// <remarks>Since Valkey 8.1.0.</remarks>
    public IReadOnlySet<ClientCapability>? Capabilities { get; init; }

    #endregion
    #region Constructors

    internal ClientInfo() { }

    #endregion
}

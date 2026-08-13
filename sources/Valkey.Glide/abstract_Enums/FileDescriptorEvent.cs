// Copyright Valkey GLIDE Project Contributors - SPDX Identifier: Apache-2.0

namespace Valkey.Glide;

/// <summary>
/// File descriptor event for a client connection.
/// </summary>
/// <seealso href="https://valkey.io/commands/client-info/" />
/// <seealso href="https://valkey.io/commands/client-list/" />
public enum FileDescriptorEvent
{
    /// <summary>
    /// The client socket is readable.
    /// </summary>
    Readable = 'r',

    /// <summary>
    /// The client socket is writable.
    /// </summary>
    Writable = 'w',
}

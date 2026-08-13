// Copyright Valkey GLIDE Project Contributors - SPDX Identifier: Apache-2.0

using Valkey.Glide.Commands.Options;
using Valkey.Glide.Internals;

namespace Valkey.Glide;

public partial class GlideClient
{
    /// <inheritdoc cref="IGlideClient.ClientInfoAsync()"/>
    public async Task<ClientInfo> ClientInfoAsync()
        => await Command(Request.ClientInfo());

    /// <inheritdoc cref="IGlideClient.ClientListAsync()"/>
    public async Task<ClientInfo[]> ClientListAsync()
        => await Command(Request.ClientList());

    /// <inheritdoc cref="IGlideClient.ClientListAsync(ClientFilterOptions)"/>
    public async Task<ClientInfo[]> ClientListAsync(ClientFilterOptions options)
        => await Command(Request.ClientList(options));
}

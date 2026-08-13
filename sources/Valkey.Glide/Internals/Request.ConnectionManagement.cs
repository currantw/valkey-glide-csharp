// Copyright Valkey GLIDE Project Contributors - SPDX Identifier: Apache-2.0

using Valkey.Glide.Commands.Options;

using static Valkey.Glide.Internals.FFI;
using static Valkey.Glide.Internals.TimeUtils;

namespace Valkey.Glide.Internals;

internal partial class Request
{
    #region Constants

    private static readonly IReadOnlySet<string> EmptyStringSet = new HashSet<string>();

    #endregion
    #region Command Builders

    public static Cmd<GlideString, ValkeyValue> ClientGetName()
        => ToValkeyValue(RequestType.ClientGetName, [], isNullable: true);

    public static Cmd<long, long> ClientId()
        => Simple<long>(RequestType.ClientId, []);

    public static Cmd<GlideString, ClientInfo> ClientInfo()
        => new(RequestType.ClientInfo, [], false, ConvertClientInfoResponse);

    public static Cmd<long, long> ClientKill(ClientFilterOptions options)
        => Simple<long>(RequestType.ClientKill, options.ToArgs());

    public static Cmd<GlideString, ClientInfo[]> ClientList()
        => new(RequestType.ClientList, [], false, ConvertClientListResponse);

    public static Cmd<GlideString, ClientInfo[]> ClientList(ClientFilterOptions options)
        => new(RequestType.ClientList, options.ToArgs(), false, ConvertClientListResponse);

    public static Cmd<string, ValkeyValue> ClientPause(TimeSpan timeout)
        => Ok(RequestType.ClientPause, [ToULongMs(timeout, nameof(timeout)).ToGlideString()]);

    public static Cmd<string, ValkeyValue> ClientPauseWrite(TimeSpan timeout)
        => Ok(RequestType.ClientPause, [ToULongMs(timeout, nameof(timeout)).ToGlideString(), ValkeyLiterals.WRITE]);

    public static Cmd<string, ValkeyValue> ClientUnpause()
        => Ok(RequestType.ClientUnpause);

    public static Cmd<Dictionary<GlideString, object>, ClientTrackingInfo> ClientTrackingInfo()
        => new(RequestType.ClientTrackingInfo, [], false, ConvertClientTrackingInfoResponse);

    public static Cmd<GlideString, ValkeyValue> Reset()
        => ToValkeyValue(RequestType.Reset, []);

    #endregion
    #region Response Converters

    internal static ClientInfo ConvertClientInfoResponse(GlideString response)
        => ParseClientInfoResponse(response.ToString().Trim());

    internal static ClientInfo[] ConvertClientListResponse(GlideString response)
        => [.. response.ToString().Trim().Split('\n', StringSplitOptions.RemoveEmptyEntries).Select(ParseClientInfoResponse)];

    private static ClientTrackingInfo ConvertClientTrackingInfoResponse(Dictionary<GlideString, object> map)
    {
        IReadOnlySet<string> flags =
            map.TryGetValue("flags", out object? flagsObj) && flagsObj is IEnumerable<object> flagsItems
            ? ToReadOnlyStringSet(flagsItems)
            : EmptyStringSet;

        long redirect = map.TryGetValue("redirect", out object? redirectObj)
            ? Convert.ToInt64(redirectObj)
            : -1;

        IReadOnlySet<string> prefixes =
            map.TryGetValue("prefixes", out object? prefixesObj) && prefixesObj is IEnumerable<object> prefixItems
            ? ToReadOnlyStringSet(prefixItems)
            : EmptyStringSet;

        return new ClientTrackingInfo
        {
            Flags = flags,
            Redirect = redirect,
            Prefixes = prefixes,
        };
    }

    private static ClientInfo ParseClientInfoResponse(string response)
    {
        var map = response
            .Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Select(token => token.Split('=', 2))
            .ToDictionary<string[], GlideString, object>(parts => parts[0], parts => new GlideString(parts[1]));

        return new ClientInfo
        {
            Address = Utils.SplitAddress(GetString(map, "addr")),
            Age = TimeSpan.FromSeconds(GetULong(map, "age")),
            ArgvMemory = GetULong(map, "argv-mem"),
            Database = GetUShort(map, "db"),
            Events = ParseFileDescriptorEvents(GetString(map, "events")),
            FileDescriptor = GetUInt(map, "fd"),
            Flags = ParseClientFlags(GetString(map, "flags")),
            Id = GetULong(map, "id"),
            Idle = TimeSpan.FromSeconds(GetULong(map, "idle")),
            LastCommand = TryGetString(map, "cmd") is { Length: > 0 } cmd ? cmd : null,
            LocalAddress = Utils.SplitAddress(GetString(map, "laddr")),
            MultiMemory = GetULong(map, "multi-mem"),
            Name = TryGetString(map, "name") is { Length: > 0 } name ? name : null,
            OutputBufferLength = GetULong(map, "obl"),
            OutputBufferMemory = GetULong(map, "omem"),
            OutputListLength = GetULong(map, "oll"),
            PatternSubscriptionCount = GetUInt(map, "psub"),
            QueryBuffer = GetULong(map, "qbuf"),
            QueryBufferFree = GetULong(map, "qbuf-free"),
            Redirect = TryGetULong(map, "redir"),
            ShardedSubscriptionCount = GetUInt(map, "ssub"),
            SubscriptionCount = GetUInt(map, "sub"),
            TotalMemory = GetULong(map, "tot-mem"),
            TransactionCommandLength = TryGetUInt(map, "multi"),
            User = TryGetString(map, "user") is { Length: > 0 } user ? user : null,

            // Since Valkey 7.0
            Protocol = TryGetString(map, "resp") is { } resp ? ParseProtocol(resp) : null,

            // Since Valkey 7.2
            LibraryName = TryGetString(map, "lib-name") is { Length: > 0 } libName ? libName : null,
            LibraryVersion = TryGetString(map, "lib-ver") is { Length: > 0 } libVer ? libVer : null,

            // Since Valkey 8.0
            TotalCommands = TryGetULong(map, "tot-cmds"),
            TotalNetInput = TryGetULong(map, "tot-net-in") ?? TryGetULong(map, "net-i"),
            TotalNetOutput = TryGetULong(map, "tot-net-out") ?? TryGetULong(map, "net-o"),
            WatchedKeyCount = TryGetUInt(map, "watch"),

            // Since Valkey 8.1
            Capabilities = TryGetString(map, "capa") is { } capa ? ParseClientCapabilities(capa) : null,
        };
    }

    internal static IReadOnlySet<ClientCapability> ParseClientCapabilities(string response)
    {
        var capabilities = new HashSet<ClientCapability>();
        foreach (char c in response)
        {
            if (Enum.IsDefined((ClientCapability)c))
            {
                _ = capabilities.Add((ClientCapability)c);
            }
            else
            {
                Logger.Log(Level.Warn, "ParseClientCapabilities", $"Unknown client capability: '{c}'");
            }
        }

        return capabilities;
    }

    internal static IReadOnlySet<ClientFlag> ParseClientFlags(string response)
    {
        var flags = new HashSet<ClientFlag>();
        foreach (char c in response)
        {
            if (Enum.IsDefined((ClientFlag)c))
            {
                _ = flags.Add((ClientFlag)c);
            }
            else
            {
                Logger.Log(Level.Warn, "ParseClientFlags", $"Unknown client flag: '{c}'");
            }
        }

        return flags;
    }

    internal static IReadOnlySet<FileDescriptorEvent> ParseFileDescriptorEvents(string response)
    {
        var events = new HashSet<FileDescriptorEvent>();
        foreach (char c in response)
        {
            if (Enum.IsDefined((FileDescriptorEvent)c))
            {
                _ = events.Add((FileDescriptorEvent)c);
            }
            else
            {
                Logger.Log(Level.Warn, "ParseFileDescriptorEvents", $"Unknown file descriptor event: '{c}'");
            }
        }

        return events;
    }

    internal static Protocol? ParseProtocol(string value)
    {
        var result = value switch
        {
            "2" => (Protocol?)Valkey.Glide.Protocol.Resp2,
            "3" => (Protocol?)Valkey.Glide.Protocol.Resp3,
            _ => null,
        };

        if (result is null)
        {
            Logger.Log(Level.Warn, "ParseProtocol", $"Unknown protocol version: '{value}'");
        }

        return result;
    }

    #endregion
}

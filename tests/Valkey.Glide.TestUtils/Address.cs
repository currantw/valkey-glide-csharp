// Copyright Valkey GLIDE Project Contributors - SPDX Identifier: Apache-2.0

namespace Valkey.Glide.TestUtils;

/// <summary>
/// Represents a network address with a host and port.
/// </summary>
public record class Address(string Host, ushort Port)
{
    public override string ToString() => $"{Host}:{Port}";

    /// <summary>
    /// Parses a comma-separated list of hosts (e.g. "localhost:6379,localhost:6380") to addresses.
    /// </summary>
    public static IList<Address> FromHosts(string hosts)
    {
        List<Address> addresses = [];
        foreach (var host in hosts.Split(','))
        {
            var parts = host.Split(':');
            addresses.Add(new Address(parts[0], ushort.Parse(parts[1])));
        }

        return addresses;
    }

    public static bool operator ==((string Host, ushort Port) tuple, Address address)
        => address.Host == tuple.Host && address.Port == tuple.Port;

    public static bool operator !=((string Host, ushort Port) tuple, Address address)
        => !(tuple == address);

    public static bool operator ==(Address address, (string Host, ushort Port) tuple)
        => address.Host == tuple.Host && address.Port == tuple.Port;

    public static bool operator !=(Address address, (string Host, ushort Port) tuple)
        => !(address == tuple);

    public static implicit operator (string Host, ushort Port)(Address address)
        => (address.Host, address.Port);
}

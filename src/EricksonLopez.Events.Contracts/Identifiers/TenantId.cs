// Copyright © Erickson Lopez. MIT License.
using System;

namespace EricksonLopez.Events.Identifiers;

using System.Diagnostics.CodeAnalysis;

/// <summary>
/// Represents an event correlation and routing tenant identifier attached to an event envelope.
/// </summary>
/// <remarks>
/// <para>
/// <b>Architectural Boundary &amp; Semantic Scope:</b>
/// This struct models the tenant identity carried within event envelopes for distributed event routing,
/// partition key generation, and event correlation in multi-tenant event streams.
/// </para>
/// <para>
/// It is <b>intentionally distinct</b> from the tenant resolution and lifecycle identity managed by
/// <c>EricksonLopez.MultiTenancy</c>. Events remain decoupled from tenancy resolution engines.
/// </para>
/// </remarks>
public readonly record struct TenantId :
    IComparable<TenantId>,
    IComparable,
    IEquatable<TenantId>,
    IParsable<TenantId>
{
    /// <summary>
    /// Gets the empty <see cref="TenantId"/> instance.
    /// </summary>
    public static readonly TenantId Empty = new(string.Empty);

    private readonly string? _value;

    /// <summary>
    /// Gets the string representation of the tenant id.
    /// </summary>
    public string Value => _value ?? string.Empty;

    /// <summary>
    /// Initializes a new instance of the <see cref="TenantId"/> struct.
    /// </summary>
    /// <param name="value">The tenant identifier string.</param>
    public TenantId(string value)
    {
        _value = value ?? string.Empty;
    }

    /// <summary>
    /// Creates a <see cref="TenantId"/> from a string value.
    /// </summary>
    /// <param name="value">The string value.</param>
    /// <returns>A new <see cref="TenantId"/> instance.</returns>
    public static TenantId From(string value) => new(value);

    /// <summary>
    /// Creates a <see cref="TenantId"/> from a <see cref="Guid"/>, interoperable with EricksonLopez.MultiTenancy.
    /// </summary>
    /// <param name="value">The tenant GUID.</param>
    /// <returns>A new <see cref="TenantId"/> formatted as a standard 36-character hyphenated UUID.</returns>
    public static TenantId From(Guid value) => new(value.ToString("D"));

    /// <summary>
    /// Converts a <see cref="Guid"/> to a <see cref="TenantId"/> instance.
    /// </summary>
    /// <param name="value">The GUID value to convert.</param>
    /// <returns>A new <see cref="TenantId"/> initialized with the GUID string.</returns>
    public static explicit operator TenantId(Guid value) => From(value);

    /// <summary>
    /// Attempts to parse the underlying value as a <see cref="Guid"/> for interoperability with EricksonLopez.MultiTenancy.
    /// </summary>
    /// <param name="tenantGuid">When this method returns, contains the parsed GUID value if successful; otherwise, <see cref="Guid.Empty"/>.</param>
    /// <returns><see langword="true"/> if the value is a valid GUID; otherwise, <see langword="false"/>.</returns>
    public bool TryToGuid(out Guid tenantGuid) => Guid.TryParse(Value, out tenantGuid);

    /// <summary>
    /// Gets a value indicating whether the tenant id is empty.
    /// </summary>
    public bool IsEmpty => string.IsNullOrWhiteSpace(Value);

    /// <inheritdoc />
    public bool Equals(TenantId other)
    {
        if (IsEmpty && other.IsEmpty) return true;
        return string.Equals(Value, other.Value, StringComparison.OrdinalIgnoreCase);
    }

    /// <inheritdoc />
    public override int GetHashCode() =>
        IsEmpty ? 0 : string.GetHashCode(Value, StringComparison.OrdinalIgnoreCase);

    /// <inheritdoc />
    public int CompareTo(TenantId other)
    {
        if (IsEmpty && other.IsEmpty) return 0;
        if (IsEmpty) return -1;
        if (other.IsEmpty) return 1;
        return string.Compare(Value, other.Value, StringComparison.OrdinalIgnoreCase);
    }

    /// <inheritdoc />
    public int CompareTo(object? obj)
    {
        if (obj is null) return 1;
        if (obj is TenantId other) return CompareTo(other);
        throw new ArgumentException($"Object must be of type {nameof(TenantId)}", nameof(obj));
    }

    /// <summary>
    /// Determines whether a specified <see cref="TenantId"/> is less than another specified <see cref="TenantId"/>.
    /// </summary>
    /// <param name="left">The first tenant identifier to compare.</param>
    /// <param name="right">The second tenant identifier to compare.</param>
    /// <returns><see langword="true"/> if <paramref name="left"/> is less than <paramref name="right"/>; otherwise, <see langword="false"/>.</returns>
    public static bool operator <(TenantId left, TenantId right) => left.CompareTo(right) < 0;

    /// <summary>
    /// Determines whether a specified <see cref="TenantId"/> is less than or equal to another specified <see cref="TenantId"/>.
    /// </summary>
    /// <param name="left">The first tenant identifier to compare.</param>
    /// <param name="right">The second tenant identifier to compare.</param>
    /// <returns><see langword="true"/> if <paramref name="left"/> is less than or equal to <paramref name="right"/>; otherwise, <see langword="false"/>.</returns>
    public static bool operator <=(TenantId left, TenantId right) => left.CompareTo(right) <= 0;

    /// <summary>
    /// Determines whether a specified <see cref="TenantId"/> is greater than another specified <see cref="TenantId"/>.
    /// </summary>
    /// <param name="left">The first tenant identifier to compare.</param>
    /// <param name="right">The second tenant identifier to compare.</param>
    /// <returns><see langword="true"/> if <paramref name="left"/> is greater than <paramref name="right"/>; otherwise, <see langword="false"/>.</returns>
    public static bool operator >(TenantId left, TenantId right) => left.CompareTo(right) > 0;

    /// <summary>
    /// Determines whether a specified <see cref="TenantId"/> is greater than or equal to another specified <see cref="TenantId"/>.
    /// </summary>
    /// <param name="left">The first tenant identifier to compare.</param>
    /// <param name="right">The second tenant identifier to compare.</param>
    /// <returns><see langword="true"/> if <paramref name="left"/> is greater than or equal to <paramref name="right"/>; otherwise, <see langword="false"/>.</returns>
    public static bool operator >=(TenantId left, TenantId right) => left.CompareTo(right) >= 0;

    /// <summary>
    /// Converts a <see cref="TenantId"/> to its underlying string value.
    /// </summary>
    /// <param name="id">The tenant identifier to convert.</param>
    /// <returns>The string representation of the tenant identifier.</returns>
    public static implicit operator string(TenantId id) => id.Value;

    /// <summary>
    /// Converts a string to a <see cref="TenantId"/> instance.
    /// </summary>
    /// <param name="value">The string value to convert.</param>
    /// <returns>A new <see cref="TenantId"/> initialized with the specified value.</returns>
    public static explicit operator TenantId(string value) => new(value);

    /// <inheritdoc />
    public override string ToString() => Value ?? string.Empty;

    /// <inheritdoc />
    public static TenantId Parse(string s, IFormatProvider? provider = null) =>
        new(s ?? throw new ArgumentNullException(nameof(s)));

    /// <inheritdoc />
    public static bool TryParse([NotNullWhen(true)] string? s, IFormatProvider? provider, out TenantId result)
    {
        if (s is not null)
        {
            result = new TenantId(s);
            return true;
        }

        result = Empty;
        return false;
    }
}





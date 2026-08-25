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

    /// <summary>
    /// Gets the string representation of the tenant id.
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="TenantId"/> struct.
    /// </summary>
    /// <param name="value">The tenant identifier string.</param>
    public TenantId(string value)
    {
        Value = value ?? string.Empty;
    }

    /// <summary>
    /// Creates a <see cref="TenantId"/> from a string value.
    /// </summary>
    /// <param name="value">The string value.</param>
    /// <returns>A new <see cref="TenantId"/> instance.</returns>
    public static TenantId From(string value) => new(value);

    /// <summary>
    /// Gets a value indicating whether the tenant id is empty.
    /// </summary>
    public bool IsEmpty => string.IsNullOrEmpty(Value);

    /// <inheritdoc />
    public int CompareTo(TenantId other) =>
        string.Compare(Value, other.Value, StringComparison.OrdinalIgnoreCase);

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
    public override string ToString() => Value;

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





// Copyright © Erickson Lopez. MIT License.
using System;

namespace EricksonLopez.Events.Identifiers;

using System.Diagnostics.CodeAnalysis;

/// <summary>
/// Represents a causation identifier pointing to the immediate direct trigger of an event.
/// </summary>
public readonly record struct CausationId :
    IComparable<CausationId>,
    IComparable,
    IEquatable<CausationId>,
    IParsable<CausationId>
{
    /// <summary>
    /// Gets the empty <see cref="CausationId"/> instance.
    /// </summary>
    public static readonly CausationId Empty = new(string.Empty);

    /// <summary>
    /// Gets the string representation of the causation id.
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="CausationId"/> struct.
    /// </summary>
    /// <param name="value">The causation identifier string.</param>
    public CausationId(string value)
    {
        Value = value ?? string.Empty;
    }

    /// <summary>
    /// Creates a <see cref="CausationId"/> from a string value.
    /// </summary>
    /// <param name="value">The string value.</param>
    /// <returns>A new <see cref="CausationId"/> instance.</returns>
    public static CausationId From(string value) => new(value);

    /// <summary>
    /// Creates a <see cref="CausationId"/> from an <see cref="EventId"/>.
    /// </summary>
    /// <param name="eventId">The parent event id.</param>
    /// <returns>A new <see cref="CausationId"/> instance.</returns>
    public static CausationId From(EventId eventId) => new(eventId.ToString());

    /// <summary>
    /// Creates a <see cref="CausationId"/> from a unique identifier.
    /// </summary>
    /// <param name="identifier">The unique identifier value.</param>
    /// <returns>A new <see cref="CausationId"/> instance.</returns>
    public static CausationId From(Guid identifier) => new(identifier.ToString());

    /// <summary>
    /// Gets a value indicating whether the causation id is empty.
    /// </summary>
    public bool IsEmpty => string.IsNullOrEmpty(Value);

    /// <inheritdoc />
    public int CompareTo(CausationId other) =>
        string.Compare(Value, other.Value, StringComparison.Ordinal);

    /// <inheritdoc />
    public int CompareTo(object? obj)
    {
        if (obj is null) return 1;
        if (obj is CausationId other) return CompareTo(other);
        throw new ArgumentException($"Object must be of type {nameof(CausationId)}", nameof(obj));
    }

    /// <summary>
    /// Determines whether a specified <see cref="CausationId"/> is less than another specified <see cref="CausationId"/>.
    /// </summary>
    /// <param name="left">The first causation identifier to compare.</param>
    /// <param name="right">The second causation identifier to compare.</param>
    /// <returns><see langword="true"/> if <paramref name="left"/> is less than <paramref name="right"/>; otherwise, <see langword="false"/>.</returns>
    public static bool operator <(CausationId left, CausationId right) => left.CompareTo(right) < 0;

    /// <summary>
    /// Determines whether a specified <see cref="CausationId"/> is less than or equal to another specified <see cref="CausationId"/>.
    /// </summary>
    /// <param name="left">The first causation identifier to compare.</param>
    /// <param name="right">The second causation identifier to compare.</param>
    /// <returns><see langword="true"/> if <paramref name="left"/> is less than or equal to <paramref name="right"/>; otherwise, <see langword="false"/>.</returns>
    public static bool operator <=(CausationId left, CausationId right) => left.CompareTo(right) <= 0;

    /// <summary>
    /// Determines whether a specified <see cref="CausationId"/> is greater than another specified <see cref="CausationId"/>.
    /// </summary>
    /// <param name="left">The first causation identifier to compare.</param>
    /// <param name="right">The second causation identifier to compare.</param>
    /// <returns><see langword="true"/> if <paramref name="left"/> is greater than <paramref name="right"/>; otherwise, <see langword="false"/>.</returns>
    public static bool operator >(CausationId left, CausationId right) => left.CompareTo(right) > 0;

    /// <summary>
    /// Determines whether a specified <see cref="CausationId"/> is greater than or equal to another specified <see cref="CausationId"/>.
    /// </summary>
    /// <param name="left">The first causation identifier to compare.</param>
    /// <param name="right">The second causation identifier to compare.</param>
    /// <returns><see langword="true"/> if <paramref name="left"/> is greater than or equal to <paramref name="right"/>; otherwise, <see langword="false"/>.</returns>
    public static bool operator >=(CausationId left, CausationId right) => left.CompareTo(right) >= 0;

    /// <summary>
    /// Converts a <see cref="CausationId"/> to its underlying string value.
    /// </summary>
    /// <param name="id">The causation identifier to convert.</param>
    /// <returns>The string representation of the causation identifier.</returns>
    public static implicit operator string(CausationId id) => id.Value;

    /// <summary>
    /// Converts a string to a <see cref="CausationId"/> instance.
    /// </summary>
    /// <param name="value">The string value to convert.</param>
    /// <returns>A new <see cref="CausationId"/> initialized with the specified value.</returns>
    public static explicit operator CausationId(string value) => new(value);

    /// <inheritdoc />
    public override string ToString() => Value ?? string.Empty;

    /// <inheritdoc />
    public static CausationId Parse(string s, IFormatProvider? provider = null) =>
        new(s ?? throw new ArgumentNullException(nameof(s)));

    /// <inheritdoc />
    public static bool TryParse([NotNullWhen(true)] string? s, IFormatProvider? provider, out CausationId result)
    {
        if (s is not null)
        {
            result = new CausationId(s);
            return true;
        }

        result = Empty;
        return false;
    }
}





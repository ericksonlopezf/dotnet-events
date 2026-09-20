// Copyright © Erickson Lopez. MIT License.
using System;

namespace EricksonLopez.Events.Identifiers;

using System.Diagnostics.CodeAnalysis;

/// <summary>
/// Represents the semantic identity of an event type, independent of CLR type or namespace names.
/// </summary>
public readonly record struct EventType :
    IComparable<EventType>,
    IComparable,
    IEquatable<EventType>,
    IParsable<EventType>
{
    /// <summary>
    /// Gets the string representation of the event type.
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="EventType"/> struct.
    /// </summary>
    /// <param name="value">The event type name.</param>
    /// <exception cref="ArgumentException"><paramref name="value"/> is <see langword="null"/>, empty, or consists only of white-space characters</exception>
    public EventType(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        Value = value.Trim();
    }

    /// <summary>
    /// Creates a new <see cref="EventType"/> from a string value.
    /// </summary>
    /// <param name="value">The event type string.</param>
    /// <returns>A new <see cref="EventType"/> instance.</returns>
    public static EventType From(string value) => new(value);

    /// <summary>
    /// Gets a value indicating whether the event type is empty or uninitialized.
    /// </summary>
    public bool IsEmpty => string.IsNullOrWhiteSpace(Value);

    /// <inheritdoc />
    public bool Equals(EventType other) =>
        string.Equals(Value, other.Value, StringComparison.OrdinalIgnoreCase);

    /// <inheritdoc />
    public override int GetHashCode() =>
        string.GetHashCode(Value ?? string.Empty, StringComparison.OrdinalIgnoreCase);

    /// <inheritdoc />
    public int CompareTo(EventType other) =>
        string.Compare(Value, other.Value, StringComparison.OrdinalIgnoreCase);

    /// <inheritdoc />
    public int CompareTo(object? obj)
    {
        if (obj is null) return 1;
        if (obj is EventType other) return CompareTo(other);
        throw new ArgumentException($"Object must be of type {nameof(EventType)}", nameof(obj));
    }

    /// <summary>
    /// Determines whether a specified <see cref="EventType"/> is less than another specified <see cref="EventType"/>.
    /// </summary>
    /// <param name="left">The first event type to compare.</param>
    /// <param name="right">The second event type to compare.</param>
    /// <returns><see langword="true"/> if <paramref name="left"/> is less than <paramref name="right"/>; otherwise, <see langword="false"/>.</returns>
    public static bool operator <(EventType left, EventType right) => left.CompareTo(right) < 0;

    /// <summary>
    /// Determines whether a specified <see cref="EventType"/> is less than or equal to another specified <see cref="EventType"/>.
    /// </summary>
    /// <param name="left">The first event type to compare.</param>
    /// <param name="right">The second event type to compare.</param>
    /// <returns><see langword="true"/> if <paramref name="left"/> is less than or equal to <paramref name="right"/>; otherwise, <see langword="false"/>.</returns>
    public static bool operator <=(EventType left, EventType right) => left.CompareTo(right) <= 0;

    /// <summary>
    /// Determines whether a specified <see cref="EventType"/> is greater than another specified <see cref="EventType"/>.
    /// </summary>
    /// <param name="left">The first event type to compare.</param>
    /// <param name="right">The second event type to compare.</param>
    /// <returns><see langword="true"/> if <paramref name="left"/> is greater than <paramref name="right"/>; otherwise, <see langword="false"/>.</returns>
    public static bool operator >(EventType left, EventType right) => left.CompareTo(right) > 0;

    /// <summary>
    /// Determines whether a specified <see cref="EventType"/> is greater than or equal to another specified <see cref="EventType"/>.
    /// </summary>
    /// <param name="left">The first event type to compare.</param>
    /// <param name="right">The second event type to compare.</param>
    /// <returns><see langword="true"/> if <paramref name="left"/> is greater than or equal to <paramref name="right"/>; otherwise, <see langword="false"/>.</returns>
    public static bool operator >=(EventType left, EventType right) => left.CompareTo(right) >= 0;

    /// <summary>
    /// Converts an <see cref="EventType"/> to its underlying <see cref="string"/> value.
    /// </summary>
    /// <param name="type">The event type to convert.</param>
    /// <returns>The string representation of the event type.</returns>
    public static implicit operator string(EventType type) => type.Value;

    /// <summary>
    /// Converts a <see cref="string"/> to an <see cref="EventType"/> instance.
    /// </summary>
    /// <param name="value">The string value to convert.</param>
    /// <returns>A new <see cref="EventType"/> initialized with the specified value.</returns>
    public static explicit operator EventType(string value) => new(value);

    /// <inheritdoc />
    public override string ToString() => Value ?? string.Empty;

    /// <inheritdoc />
    public static EventType Parse(string s, IFormatProvider? provider = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(s);
        return new EventType(s);
    }

    /// <inheritdoc />
    public static bool TryParse([NotNullWhen(true)] string? s, IFormatProvider? provider, out EventType result)
    {
        if (!string.IsNullOrWhiteSpace(s))
        {
            result = new EventType(s);
            return true;
        }

        result = default;
        return false;
    }
}





// Copyright © Erickson Lopez. MIT License.
using System;

namespace EricksonLopez.Events.Identifiers;

using System.Diagnostics.CodeAnalysis;
using System.Globalization;

/// <summary>
/// Represents a monotonic version for an event contract schema.
/// </summary>
public readonly record struct EventVersion :
    IComparable<EventVersion>,
    IComparable,
    IEquatable<EventVersion>,
    IParsable<EventVersion>
{
    /// <summary>
    /// Gets the default initial version (V1).
    /// </summary>
    public static readonly EventVersion V1 = new(1);

    private readonly uint _value;

    /// <summary>
    /// Gets the unsigned integer value of the version.
    /// </summary>
    public uint Value => _value == 0 ? 1 : _value;

    /// <summary>
    /// Gets a value indicating whether this version instance is uninitialized (default struct state).
    /// </summary>
    public bool IsUninitialized => _value == 0;

    /// <summary>
    /// Initializes a new instance of the <see cref="EventVersion"/> struct.
    /// </summary>
    /// <param name="value">The version number (must be greater than or equal to 1).</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="value"/> is equal to zero</exception>
    public EventVersion(uint value)
    {
        if (value == 0)
        {
            throw new ArgumentOutOfRangeException(nameof(value), "Event version must be greater than or equal to 1.");
        }

        _value = value;
    }

    /// <inheritdoc />
    public bool Equals(EventVersion other) => Value == other.Value;

    /// <inheritdoc />
    public override int GetHashCode() => Value.GetHashCode();

    /// <summary>
    /// Creates an <see cref="EventVersion"/> from an unsigned integer.
    /// </summary>
    /// <param name="value">The version number.</param>
    /// <returns>A new <see cref="EventVersion"/> instance.</returns>
    public static EventVersion From(uint value) => new(value);

    /// <summary>
    /// Creates an <see cref="EventVersion"/> from a standard signed integer.
    /// </summary>
    /// <param name="value">The version number.</param>
    /// <returns>A new <see cref="EventVersion"/> instance.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="value"/> is less than or equal to zero</exception>
    public static EventVersion From(int value)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(value);
        return new EventVersion((uint)value);
    }

    /// <inheritdoc />
    public int CompareTo(EventVersion other) => Value.CompareTo(other.Value);

    /// <inheritdoc />
    public int CompareTo(object? obj)
    {
        if (obj is null) return 1;
        if (obj is EventVersion other) return CompareTo(other);
        throw new ArgumentException($"Object must be of type {nameof(EventVersion)}", nameof(obj));
    }

    /// <summary>
    /// Determines whether a specified <see cref="EventVersion"/> is less than another specified <see cref="EventVersion"/>.
    /// </summary>
    /// <param name="left">The first event version to compare.</param>
    /// <param name="right">The second event version to compare.</param>
    /// <returns><see langword="true"/> if <paramref name="left"/> is less than <paramref name="right"/>; otherwise, <see langword="false"/>.</returns>
    public static bool operator <(EventVersion left, EventVersion right) => left.Value < right.Value;

    /// <summary>
    /// Determines whether a specified <see cref="EventVersion"/> is less than or equal to another specified <see cref="EventVersion"/>.
    /// </summary>
    /// <param name="left">The first event version to compare.</param>
    /// <param name="right">The second event version to compare.</param>
    /// <returns><see langword="true"/> if <paramref name="left"/> is less than or equal to <paramref name="right"/>; otherwise, <see langword="false"/>.</returns>
    public static bool operator <=(EventVersion left, EventVersion right) => left.Value <= right.Value;

    /// <summary>
    /// Determines whether a specified <see cref="EventVersion"/> is greater than another specified <see cref="EventVersion"/>.
    /// </summary>
    /// <param name="left">The first event version to compare.</param>
    /// <param name="right">The second event version to compare.</param>
    /// <returns><see langword="true"/> if <paramref name="left"/> is greater than <paramref name="right"/>; otherwise, <see langword="false"/>.</returns>
    public static bool operator >(EventVersion left, EventVersion right) => left.Value > right.Value;

    /// <summary>
    /// Determines whether a specified <see cref="EventVersion"/> is greater than or equal to another specified <see cref="EventVersion"/>.
    /// </summary>
    /// <param name="left">The first event version to compare.</param>
    /// <param name="right">The second event version to compare.</param>
    /// <returns><see langword="true"/> if <paramref name="left"/> is greater than or equal to <paramref name="right"/>; otherwise, <see langword="false"/>.</returns>
    public static bool operator >=(EventVersion left, EventVersion right) => left.Value >= right.Value;

    /// <summary>
    /// Converts an <see cref="EventVersion"/> to its underlying <see cref="uint"/> value.
    /// </summary>
    /// <param name="version">The version to convert.</param>
    /// <returns>The underlying numeric version value.</returns>
    public static implicit operator uint(EventVersion version) => version.Value;

    /// <summary>
    /// Converts a <see cref="uint"/> to an <see cref="EventVersion"/>.
    /// </summary>
    /// <param name="value">The unsigned integer value to convert.</param>
    /// <returns>A new <see cref="EventVersion"/> initialized with the specified value.</returns>
    public static explicit operator EventVersion(uint value) => new(value);

    /// <inheritdoc />
    public override string ToString() => Value.ToString(CultureInfo.InvariantCulture);

    /// <inheritdoc />
    public static EventVersion Parse(string s, IFormatProvider? provider)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(s);
        return new EventVersion(uint.Parse(s, provider));
    }

    /// <summary>
    /// Parses a string into an <see cref="EventVersion"/>.
    /// </summary>
    /// <param name="s">The string representation to parse.</param>
    /// <returns>The parsed <see cref="EventVersion"/>.</returns>
    public static EventVersion Parse(string s) => Parse(s, null);

    /// <inheritdoc />
    public static bool TryParse([NotNullWhen(true)] string? s, IFormatProvider? provider, out EventVersion result)
    {
        if (!string.IsNullOrWhiteSpace(s) && uint.TryParse(s, provider, out var val) && val > 0)
        {
            result = new EventVersion(val);
            return true;
        }

        result = default;
        return false;
    }
}





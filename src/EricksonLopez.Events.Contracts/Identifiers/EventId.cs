// Copyright © Erickson Lopez. MIT License.
using System;

namespace EricksonLopez.Events.Identifiers;

using System.Diagnostics.CodeAnalysis;

/// <summary>
/// Represents a strongly typed, unique, and time-sortable identifier for an event based on GUID Version 7.
/// </summary>
public readonly record struct EventId :
    IComparable<EventId>,
    IComparable,
    IEquatable<EventId>,
    ISpanFormattable,
    IUtf8SpanFormattable,
    IParsable<EventId>,
    ISpanParsable<EventId>
{
    /// <summary>
    /// Gets the empty <see cref="EventId"/> instance.
    /// </summary>
    public static readonly EventId Empty = new(Guid.Empty);

    /// <summary>
    /// Gets the underlying <see cref="Guid"/> value.
    /// </summary>
    public Guid Value { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="EventId"/> struct with a specific <see cref="Guid"/>.
    /// </summary>
    /// <param name="value">The underlying unique identifier value.</param>
    public EventId(Guid value)
    {
        Value = value;
    }

    /// <summary>
    /// Creates a new time-sortable <see cref="EventId"/> using native GUID Version 7.
    /// </summary>
    /// <returns>A new <see cref="EventId"/> containing a monotonically ordered Guid v7.</returns>
#if NET9_0_OR_GREATER
    public static EventId New() => new(Guid.CreateVersion7());
#else
    public static EventId New() => new(Guid.NewGuid());
#endif

    /// <summary>
    /// Creates an <see cref="EventId"/> from an existing <see cref="Guid"/>.
    /// </summary>
    /// <param name="value">The GUID value.</param>
    /// <returns>A new <see cref="EventId"/> instance.</returns>
    public static EventId From(Guid value) => new(value);

    /// <summary>
    /// Gets a value indicating whether this instance is empty.
    /// </summary>
    public bool IsEmpty => Value == Guid.Empty;

    /// <inheritdoc />
    public int CompareTo(EventId other) => Value.CompareTo(other.Value);

    /// <inheritdoc />
    public int CompareTo(object? obj)
    {
        if (obj is null) return 1;
        if (obj is EventId other) return CompareTo(other);
        throw new ArgumentException($"Object must be of type {nameof(EventId)}", nameof(obj));
    }

    /// <summary>
    /// Determines whether a specified <see cref="EventId"/> is less than another specified <see cref="EventId"/>.
    /// </summary>
    /// <param name="left">The first event identifier to compare.</param>
    /// <param name="right">The second event identifier to compare.</param>
    /// <returns><see langword="true"/> if <paramref name="left"/> is less than <paramref name="right"/>; otherwise, <see langword="false"/>.</returns>
    public static bool operator <(EventId left, EventId right) => left.CompareTo(right) < 0;

    /// <summary>
    /// Determines whether a specified <see cref="EventId"/> is less than or equal to another specified <see cref="EventId"/>.
    /// </summary>
    /// <param name="left">The first event identifier to compare.</param>
    /// <param name="right">The second event identifier to compare.</param>
    /// <returns><see langword="true"/> if <paramref name="left"/> is less than or equal to <paramref name="right"/>; otherwise, <see langword="false"/>.</returns>
    public static bool operator <=(EventId left, EventId right) => left.CompareTo(right) <= 0;

    /// <summary>
    /// Determines whether a specified <see cref="EventId"/> is greater than another specified <see cref="EventId"/>.
    /// </summary>
    /// <param name="left">The first event identifier to compare.</param>
    /// <param name="right">The second event identifier to compare.</param>
    /// <returns><see langword="true"/> if <paramref name="left"/> is greater than <paramref name="right"/>; otherwise, <see langword="false"/>.</returns>
    public static bool operator >(EventId left, EventId right) => left.CompareTo(right) > 0;

    /// <summary>
    /// Determines whether a specified <see cref="EventId"/> is greater than or equal to another specified <see cref="EventId"/>.
    /// </summary>
    /// <param name="left">The first event identifier to compare.</param>
    /// <param name="right">The second event identifier to compare.</param>
    /// <returns><see langword="true"/> if <paramref name="left"/> is greater than or equal to <paramref name="right"/>; otherwise, <see langword="false"/>.</returns>
    public static bool operator >=(EventId left, EventId right) => left.CompareTo(right) >= 0;

    /// <summary>
    /// Converts an <see cref="EventId"/> to its underlying <see cref="Guid"/>.
    /// </summary>
    /// <param name="id">The event identifier to convert.</param>
    /// <returns>The underlying <see cref="Guid"/> value.</returns>
    public static implicit operator Guid(EventId id) => id.Value;

    /// <summary>
    /// Converts a <see cref="Guid"/> to an <see cref="EventId"/>.
    /// </summary>
    /// <param name="identifier">The unique identifier value.</param>
    /// <returns>A new <see cref="EventId"/> initialized with the specified value.</returns>
    public static explicit operator EventId(Guid identifier) => new(identifier);

    /// <inheritdoc />
    public override string ToString() => Value.ToString();

    /// <inheritdoc />
    public string ToString(string? format, IFormatProvider? formatProvider) =>
        Value.ToString(format, formatProvider);

    /// <inheritdoc />
    public bool TryFormat(
        Span<char> destination,
        out int charsWritten,
        ReadOnlySpan<char> format = default,
        IFormatProvider? provider = null) =>
        Value.TryFormat(destination, out charsWritten, format);

    /// <inheritdoc />
    public bool TryFormat(
        Span<byte> utf8Destination,
        out int bytesWritten,
        ReadOnlySpan<char> format = default,
        IFormatProvider? provider = null) =>
        Value.TryFormat(utf8Destination, out bytesWritten, format);

    /// <inheritdoc />
    public static EventId Parse(string s, IFormatProvider? provider = null)
    {
        ArgumentNullException.ThrowIfNull(s);
        return new EventId(Guid.Parse(s, provider));
    }

    /// <inheritdoc />
    public static bool TryParse([NotNullWhen(true)] string? s, IFormatProvider? provider, out EventId result)
    {
        if (s is not null && Guid.TryParse(s, provider, out var guid))
        {
            result = new EventId(guid);
            return true;
        }

        result = Empty;
        return false;
    }

    /// <inheritdoc />
    public static EventId Parse(ReadOnlySpan<char> s, IFormatProvider? provider = null) =>
        new(Guid.Parse(s, provider));

    /// <inheritdoc />
    public static bool TryParse(ReadOnlySpan<char> s, IFormatProvider? provider, out EventId result)
    {
        if (Guid.TryParse(s, provider, out var guid))
        {
            result = new EventId(guid);
            return true;
        }

        result = Empty;
        return false;
    }
}





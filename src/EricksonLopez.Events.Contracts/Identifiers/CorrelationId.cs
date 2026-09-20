// Copyright © Erickson Lopez. MIT License.
using System;

namespace EricksonLopez.Events.Identifiers;

using System.Diagnostics.CodeAnalysis;

/// <summary>
/// Represents a distributed event correlation identifier that traces a business workflow across asynchronous event streams.
/// </summary>
/// <remarks>
/// <para>
/// <b>Architectural Boundary &amp; Semantic Scope:</b>
/// This struct models the strongly-typed correlation identity carried within event envelopes to correlate
/// events across distributed processes, sagas, and outbox handlers.
/// </para>
/// <para>
/// For in-memory diagnostic error correlation, <c>EricksonLopez.Result.Error</c> carries a lightweight
/// string-based correlation property, keeping the error model decoupled from event contracts.
/// </para>
/// </remarks>
public readonly record struct CorrelationId :
    IComparable<CorrelationId>,
    IComparable,
    IEquatable<CorrelationId>,
    IParsable<CorrelationId>
{
    /// <summary>
    /// Gets the empty <see cref="CorrelationId"/> instance.
    /// </summary>
    public static readonly CorrelationId Empty = new(string.Empty);

    /// <summary>
    /// Gets the string representation of the correlation id.
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="CorrelationId"/> struct.
    /// </summary>
    /// <param name="value">The correlation identifier string.</param>
    public CorrelationId(string value)
    {
        Value = value ?? string.Empty;
    }

    /// <summary>
    /// Generates a new unique <see cref="CorrelationId"/> using native GUID Version 7.
    /// </summary>
    /// <returns>A new <see cref="CorrelationId"/> instance initialized with a unique identifier.</returns>
#if NET9_0_OR_GREATER
    public static CorrelationId New() => new(Guid.CreateVersion7().ToString());
#else
    public static CorrelationId New() => new(Guid.NewGuid().ToString());
#endif

    /// <summary>
    /// Creates a <see cref="CorrelationId"/> from a string value.
    /// </summary>
    /// <param name="value">The string value.</param>
    /// <returns>A new <see cref="CorrelationId"/> instance.</returns>
    public static CorrelationId From(string value) => new(value);

    /// <summary>
    /// Creates a <see cref="CorrelationId"/> from a GUID.
    /// </summary>
    /// <param name="value">The GUID value.</param>
    /// <returns>A new <see cref="CorrelationId"/> instance.</returns>
    public static CorrelationId From(Guid value) => new(value.ToString());

    /// <summary>
    /// Gets a value indicating whether the correlation id is empty.
    /// </summary>
    public bool IsEmpty => string.IsNullOrEmpty(Value);

    /// <inheritdoc />
    public int CompareTo(CorrelationId other) =>
        string.Compare(Value, other.Value, StringComparison.Ordinal);

    /// <inheritdoc />
    public int CompareTo(object? obj)
    {
        if (obj is null) return 1;
        if (obj is CorrelationId other) return CompareTo(other);
        throw new ArgumentException($"Object must be of type {nameof(CorrelationId)}", nameof(obj));
    }

    /// <summary>
    /// Determines whether a specified <see cref="CorrelationId"/> is less than another specified <see cref="CorrelationId"/>.
    /// </summary>
    /// <param name="left">The first correlation identifier to compare.</param>
    /// <param name="right">The second correlation identifier to compare.</param>
    /// <returns><see langword="true"/> if <paramref name="left"/> is less than <paramref name="right"/>; otherwise, <see langword="false"/>.</returns>
    public static bool operator <(CorrelationId left, CorrelationId right) => left.CompareTo(right) < 0;

    /// <summary>
    /// Determines whether a specified <see cref="CorrelationId"/> is less than or equal to another specified <see cref="CorrelationId"/>.
    /// </summary>
    /// <param name="left">The first correlation identifier to compare.</param>
    /// <param name="right">The second correlation identifier to compare.</param>
    /// <returns><see langword="true"/> if <paramref name="left"/> is less than or equal to <paramref name="right"/>; otherwise, <see langword="false"/>.</returns>
    public static bool operator <=(CorrelationId left, CorrelationId right) => left.CompareTo(right) <= 0;

    /// <summary>
    /// Determines whether a specified <see cref="CorrelationId"/> is greater than another specified <see cref="CorrelationId"/>.
    /// </summary>
    /// <param name="left">The first correlation identifier to compare.</param>
    /// <param name="right">The second correlation identifier to compare.</param>
    /// <returns><see langword="true"/> if <paramref name="left"/> is greater than <paramref name="right"/>; otherwise, <see langword="false"/>.</returns>
    public static bool operator >(CorrelationId left, CorrelationId right) => left.CompareTo(right) > 0;

    /// <summary>
    /// Determines whether a specified <see cref="CorrelationId"/> is greater than or equal to another specified <see cref="CorrelationId"/>.
    /// </summary>
    /// <param name="left">The first correlation identifier to compare.</param>
    /// <param name="right">The second correlation identifier to compare.</param>
    /// <returns><see langword="true"/> if <paramref name="left"/> is greater than or equal to <paramref name="right"/>; otherwise, <see langword="false"/>.</returns>
    public static bool operator >=(CorrelationId left, CorrelationId right) => left.CompareTo(right) >= 0;

    /// <summary>
    /// Converts a <see cref="CorrelationId"/> to its underlying string value.
    /// </summary>
    /// <param name="id">The correlation identifier to convert.</param>
    /// <returns>The string representation of the correlation identifier.</returns>
    public static implicit operator string(CorrelationId id) => id.Value;

    /// <summary>
    /// Converts a string to a <see cref="CorrelationId"/> instance.
    /// </summary>
    /// <param name="value">The string value.</param>
    /// <returns>A new <see cref="CorrelationId"/> initialized with the specified value.</returns>
    public static explicit operator CorrelationId(string value) => new(value);

    /// <inheritdoc />
    public override string ToString() => Value ?? string.Empty;

    /// <inheritdoc />
    public static CorrelationId Parse(string s, IFormatProvider? provider = null) =>
        new(s ?? throw new ArgumentNullException(nameof(s)));

    /// <inheritdoc />
    public static bool TryParse([NotNullWhen(true)] string? s, IFormatProvider? provider, out CorrelationId result)
    {
        if (s is not null)
        {
            result = new CorrelationId(s);
            return true;
        }

        result = Empty;
        return false;
    }
}





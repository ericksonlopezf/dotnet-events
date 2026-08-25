// Copyright © Erickson Lopez. MIT License.
using System;

namespace EricksonLopez.Events.Attributes;

/// <summary>
/// Specifies the originating producer source or URI for an event or all events in an assembly.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Assembly, AllowMultiple = false, Inherited = false)]
public sealed class EventSourceAttribute : Attribute
{
    /// <summary>
    /// Gets the originating source name or URI.
    /// </summary>
    public string Source { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="EventSourceAttribute"/> class.
    /// </summary>
    /// <param name="source">The originating source identifier.</param>
    /// <exception cref="ArgumentException"><paramref name="source"/> is <see langword="null"/>, empty, or consists only of white-space characters</exception>
    public EventSourceAttribute(string source)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(source);
        Source = source.Trim();
    }
}



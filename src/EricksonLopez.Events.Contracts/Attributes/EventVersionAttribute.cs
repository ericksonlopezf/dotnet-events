// Copyright © Erickson Lopez. MIT License.
using System;

namespace EricksonLopez.Events.Attributes;

using EricksonLopez.Events.Identifiers;

/// <summary>
/// Specifies the explicit schema version of an event contract.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false, Inherited = false)]
public sealed class EventVersionAttribute : Attribute
{
    /// <summary>
    /// Gets the schema version number.
    /// </summary>
    public uint Version { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="EventVersionAttribute"/> class.
    /// </summary>
    /// <param name="version">The version number (must be greater than or equal to 1).</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="version"/> is equal to zero</exception>
    public EventVersionAttribute(uint version)
    {
        if (version == 0)
        {
            throw new ArgumentOutOfRangeException(nameof(version), "Version must be greater than or equal to 1.");
        }

        Version = version;
    }

    /// <summary>
    /// Converts the attribute version to a strongly typed <see cref="EventVersion"/>.
    /// </summary>
    /// <returns>A new <see cref="EventVersion"/> initialized with the configured version number.</returns>
    public EventVersion AsEventVersion() => new(Version);
}



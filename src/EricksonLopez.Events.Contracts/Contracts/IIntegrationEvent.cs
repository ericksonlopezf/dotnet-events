// Copyright © Erickson Lopez. MIT License.
namespace EricksonLopez.Events.Contracts;

/// <summary>
/// Represents an integration event published across bounded context or network boundaries.
/// Integration events are versioned, public contracts decoupled from internal domain entities.
/// </summary>
/// <remarks>
/// Integration events must be stable, self-contained, and serialization-safe.
/// They must not embed <see cref="IDomainEvent"/> types or internal domain models.
/// </remarks>
/// <seealso cref="IDomainEvent"/>
public interface IIntegrationEvent : IEvent
{
}

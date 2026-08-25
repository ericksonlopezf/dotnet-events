// Copyright © Erickson Lopez. MIT License.
namespace EricksonLopez.Events.Contracts;

/// <summary>
/// Represents a domain event that captures a significant state change within an aggregate root or bounded context,
/// expressed using the Ubiquitous Language of the domain.
/// </summary>
/// <remarks>
/// Domain events are internal to a bounded context and must not be exposed directly across context boundaries.
/// Use <see cref="IIntegrationEvent"/> for cross-context communication.
/// </remarks>
/// <seealso cref="IIntegrationEvent"/>
public interface IDomainEvent : IEvent
{
}

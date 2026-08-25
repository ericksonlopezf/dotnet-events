// Copyright © Erickson Lopez. MIT License.
namespace EricksonLopez.Events.Contracts;

/// <summary>
/// Defines the in-process event bus for publishing events across decoupled application boundaries.
/// </summary>
/// <remarks>
/// Combines <see cref="IEventPublisher"/> into a single entry point for in-process event dispatch.
/// Implementations are expected to dispatch published events to all registered <see cref="IEventHandler{TEvent}"/> instances.
/// </remarks>
/// <seealso cref="IEventPublisher"/>
public interface IEventBus : IEventPublisher
{
}

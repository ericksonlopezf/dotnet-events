// Copyright © Erickson Lopez. MIT License.
using System;
using System.Threading;
using System.Threading.Tasks;
using AwesomeAssertions;
using EricksonLopez.Events.Contracts;
using EricksonLopez.Events.Dispatch;
using EricksonLopez.Events.Identifiers;
using Xunit;

namespace EricksonLopez.Events.UnitTests.Adversarial.Api;

[Trait("Category", "Adversarial")]
public sealed class ApiConstraintsAndStructEventsTests
{
    // Struct event for zero heap allocation
    public readonly record struct StructDomainEvent(EventId Id, DateTimeOffset OccurredAt, long OrderNumber) : IEvent;

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Naming", "CA1711:Identifiers should not have incorrect suffix", Justification = "Implements IEventHandler for tests")]
    public sealed class StructEventHandler : IEventHandler<StructDomainEvent>
    {
        public static long LastHandledOrderNumber { get; set; }

        public ValueTask HandleAsync(StructDomainEvent eventInstance, CancellationToken cancellationToken = default)
        {
            LastHandledOrderNumber = eventInstance.OrderNumber;
            return ValueTask.CompletedTask;
        }
    }

    [Fact]
    public async Task InMemoryEventPublisher_SupportsStructEvents_Directly()
    {
        StructEventHandler.LastHandledOrderNumber = 0;

        var publisher = new InMemoryEventPublisher();
        var handler = new StructEventHandler();
        publisher.Subscribe(handler);

        var evt = new StructDomainEvent(EventId.New(), DateTimeOffset.UtcNow, 424242);
        await publisher.PublishAsync(evt);

        StructEventHandler.LastHandledOrderNumber.Should().Be(424242);
    }
}

// Copyright © Erickson Lopez. MIT License.
using System;
using System.Threading;
using System.Threading.Tasks;

namespace EricksonLopez.Events.UnitTests.Bus;

using EricksonLopez.Events.Contracts;
using EricksonLopez.Events.Identifiers;
using EricksonLopez.Events.UnitTests.Common;
using AwesomeAssertions;
using Xunit;

[Collection("Diagnostics")]
[Xunit.Trait("Category", "Unit")]
public sealed class EventBusPublishTests
{
    public sealed record OrderCreatedEvent(Guid OrderId, decimal Amount) : IEvent
    {
        public EventId Id { get; init; } = EventId.New();
        public DateTimeOffset OccurredAt { get; init; } = DateTimeOffset.UtcNow;
    }

    public sealed class AuditTracker
    {
        public int CallCount { get; set; }
    }

    public sealed class EmailTracker
    {
        public int CallCount { get; set; }
    }

    public sealed class OrderCreatedAuditHandler : IEventHandler<OrderCreatedEvent>
    {
        private readonly AuditTracker _tracker;

        public OrderCreatedAuditHandler(AuditTracker tracker)
        {
            _tracker = tracker;
        }

        public ValueTask HandleAsync(OrderCreatedEvent @event, CancellationToken cancellationToken = default)
        {
            _tracker.CallCount++;
            return ValueTask.CompletedTask;
        }
    }

    public sealed class OrderCreatedEmailHandler : IEventHandler<OrderCreatedEvent>
    {
        private readonly EmailTracker _tracker;

        public OrderCreatedEmailHandler(EmailTracker tracker)
        {
            _tracker = tracker;
        }

        public ValueTask HandleAsync(OrderCreatedEvent @event, CancellationToken cancellationToken = default)
        {
            _tracker.CallCount++;
            return ValueTask.CompletedTask;
        }
    }

    [Fact]
    public async Task EventBus_PublishAsync_WithSingleHandler_ShouldExecuteHandler()
    {
        var auditTracker = new AuditTracker();
        using var fixture = new EventBusTestFixture()
            .WithSingletonService(auditTracker)
            .WithEventHandler<OrderCreatedEvent, OrderCreatedAuditHandler>();

        var bus = fixture.GetBus();

        await bus.PublishAsync(new OrderCreatedEvent(Guid.NewGuid(), 150m));

        auditTracker.CallCount.Should().Be(1);
    }

    [Fact]
    public async Task EventBus_PublishAsync_WithMultipleHandlers_ShouldExecuteAllHandlers()
    {
        var auditTracker = new AuditTracker();
        var emailTracker = new EmailTracker();
        using var fixture = new EventBusTestFixture()
            .WithSingletonService(auditTracker)
            .WithSingletonService(emailTracker)
            .WithEventHandler<OrderCreatedEvent, OrderCreatedAuditHandler>()
            .WithEventHandler<OrderCreatedEvent, OrderCreatedEmailHandler>();

        var bus = fixture.GetBus();

        await bus.PublishAsync(new OrderCreatedEvent(Guid.NewGuid(), 250m));

        auditTracker.CallCount.Should().Be(1);
        emailTracker.CallCount.Should().Be(1);
    }
}





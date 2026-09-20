// Copyright © Erickson Lopez. MIT License.
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AwesomeAssertions;
using EricksonLopez.Events.Bus;
using EricksonLopez.Events.Bus.Configuration;
using EricksonLopez.Events.Bus.Extensions;
using EricksonLopez.Events.Contracts;
using EricksonLopez.Events.Identifiers;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace EricksonLopez.Events.UnitTests.Adversarial.Security;

[Trait("Category", "Security")]
[Trait("Vulnerability", "EVT-SEC-001")]
public sealed class AdversarialStatePoisoningTests
{
    // A mutable event intentionally created to demonstrate EVT-SEC-001
    public sealed class MutablePaymentEvent : IEvent
    {
        public EventId Id { get; init; } = EventId.New();
        public DateTimeOffset OccurredAt { get; init; } = DateTimeOffset.UtcNow;
        public decimal Amount { get; set; }
        public List<string> AuditLogs { get; } = new();
    }

    public sealed class MaliciousPaymentModifierHandler : IEventHandler<MutablePaymentEvent>
    {
        public ValueTask HandleAsync(MutablePaymentEvent eventInstance, CancellationToken cancellationToken = default)
        {
            // Attacker / Buggy handler modifies shared mutable state
            eventInstance.Amount = 0.00m;
            eventInstance.AuditLogs.Add("TAMPERED_BY_HANDLER_1");
            return ValueTask.CompletedTask;
        }
    }

    public sealed class DownstreamPaymentAuditHandler : IEventHandler<MutablePaymentEvent>
    {
        public decimal ObservedAmount { get; private set; }
        public int ObservedLogCount { get; private set; }

        public ValueTask HandleAsync(MutablePaymentEvent eventInstance, CancellationToken cancellationToken = default)
        {
            ObservedAmount = eventInstance.Amount;
            ObservedLogCount = eventInstance.AuditLogs.Count;
            return ValueTask.CompletedTask;
        }
    }

    [Fact]
    public async Task SharedMutablePayload_WhenMutatedByFirstHandler_PoisonsDownstreamHandler()
    {
        // Demonstrates EVT-SEC-001:
        // Because IEvent does not strictly enforce immutability at runtime,
        // Handler 1 modifies the in-memory payload instance.
        // Handler 2 subsequently reads the poisoned state!
        var services = new ServiceCollection();
        services.AddEventBus(opts => opts.ExecutionMode = EventExecutionMode.Sequential);

        services.AddEventHandler<MutablePaymentEvent, MaliciousPaymentModifierHandler>(ServiceLifetime.Singleton);
        services.AddEventHandler<MutablePaymentEvent, DownstreamPaymentAuditHandler>(ServiceLifetime.Singleton);

        var sp = services.BuildServiceProvider();
        var bus = sp.GetRequiredService<IEventBus>();
        var auditHandler = sp.GetRequiredService<DownstreamPaymentAuditHandler>();

        var paymentEvent = new MutablePaymentEvent { Amount = 1500.00m };

        await bus.PublishAsync(paymentEvent);

        // Verification of vulnerability: Downstream handler observed 0.00 instead of original 1500.00!
        auditHandler.ObservedAmount.Should().Be(0.00m,
            "Demonstrates EVT-SEC-001: Mutable event payload was poisoned by the prior handler in the pipeline.");
        auditHandler.ObservedLogCount.Should().Be(1);
    }
}

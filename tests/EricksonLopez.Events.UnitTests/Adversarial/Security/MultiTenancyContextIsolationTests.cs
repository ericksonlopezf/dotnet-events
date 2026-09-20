// Copyright © Erickson Lopez. MIT License.
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using AwesomeAssertions;
using EricksonLopez.Events.Contracts;
using EricksonLopez.Events.Dispatch;
using EricksonLopez.Events.Identifiers;
using Xunit;

namespace EricksonLopez.Events.UnitTests.Adversarial.Security;

[Trait("Category", "Security")]
public sealed class MultiTenancyContextIsolationTests
{
    public sealed record MultiTenantEvent(EventId Id, DateTimeOffset OccurredAt, TenantId TenantId, string Payload) : IEvent;

    public sealed class MultiTenantTrackingHandler : IEventHandler<MultiTenantEvent>
    {
        private static readonly AsyncLocal<TenantId> s_ambientTenant = new();
        public ConcurrentBag<(TenantId ExpectedTenant, TenantId ObservedTenant)> Contaminations { get; } = new();

        public ValueTask HandleAsync(MultiTenantEvent eventInstance, CancellationToken cancellationToken = default)
        {
            s_ambientTenant.Value = eventInstance.TenantId;

            // Simulate asynchronous context hops:
            return ProcessInternalAsync(eventInstance);
        }

        private async ValueTask ProcessInternalAsync(MultiTenantEvent eventInstance)
        {
            await Task.Yield();

            var current = s_ambientTenant.Value;
            if (current != eventInstance.TenantId)
            {
                Contaminations.Add((eventInstance.TenantId, current));
            }
        }
    }

    [Fact]
    public async Task MultiTenant_ConcurrentPublications_NeverCrossContaminateContext()
    {
        var publisher = new InMemoryEventPublisher();
        var handler = new MultiTenantTrackingHandler();
        publisher.Subscribe(handler);

        var tenantA = TenantId.From("tenant-A");
        var tenantB = TenantId.From("tenant-B");

        var tasks = new Task[100];
        for (int i = 0; i < 100; i++)
        {
            var selectedTenant = (i % 2 == 0) ? tenantA : tenantB;
            tasks[i] = Task.Run(async () =>
            {
                var evt = new MultiTenantEvent(EventId.New(), DateTimeOffset.UtcNow, selectedTenant, $"Payload-{Guid.NewGuid()}");
                await publisher.PublishAsync(evt);
            });
        }

        await Task.WhenAll(tasks);

        handler.Contaminations.Should().BeEmpty("No cross-tenant context contamination should ever occur during concurrent dispatch.");
    }
}

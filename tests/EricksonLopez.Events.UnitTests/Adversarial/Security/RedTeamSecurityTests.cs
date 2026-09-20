// Copyright © Erickson Lopez. MIT License.
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AwesomeAssertions;
using EricksonLopez.Events.Bus;
using EricksonLopez.Events.Bus.Configuration;
using EricksonLopez.Events.Bus.Extensions;
using EricksonLopez.Events.Context;
using EricksonLopez.Events.Contracts;
using EricksonLopez.Events.Envelopes;
using EricksonLopez.Events.Identifiers;
using EricksonLopez.Events.Metadata;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace EricksonLopez.Events.UnitTests.Adversarial.Security;

[Trait("Category", "Security")]
public sealed class RedTeamSecurityTests
{
    public sealed record SecureSecurityEvent(EventId Id, DateTimeOffset OccurredAt, string Data) : IEvent;

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Naming", "CA1711:Identifiers should not have incorrect suffix", Justification = "Test consumer")]
    public sealed class TenantAuditConsumer : IEventHandler<SecureSecurityEvent>
    {
        public static readonly ConcurrentBag<(string TenantId, string Data)> Received = new();

        public ValueTask HandleAsync(SecureSecurityEvent @event, CancellationToken cancellationToken = default)
        {
            var ambientTenant = EventContext.TenantId?.Value ?? string.Empty;
            Received.Add((ambientTenant, @event.Data));
            return ValueTask.CompletedTask;
        }

        public static void Reset() => Received.Clear();
    }

    [Fact]
    public async Task CrossTenantSpoofingAttack_ConcurrentTenants_NeverLeakContextBetweenStreams()
    {
        TenantAuditConsumer.Reset();

        var services = new ServiceCollection();
        services.AddEventBus(opts => opts.ExecutionMode = EventExecutionMode.Sequential);
        services.AddEventHandler<SecureSecurityEvent, TenantAuditConsumer>();

        var sp = services.BuildServiceProvider();
        var bus = sp.GetRequiredService<IEventBus>();

        const int iterations = 1000;
        var tasks = new List<Task>();

        for (int i = 0; i < iterations; i++)
        {
            string tenantA = $"tenant-alpha-{i}";
            string tenantB = $"tenant-beta-{i}";

            tasks.Add(Task.Run(async () =>
            {
                var metaA = new EventMetadataBuilder().WithTenantId(tenantA).Build();
                var envA = EventEnvelope.Create(new SecureSecurityEvent(EventId.New(), DateTimeOffset.UtcNow, $"Data-{tenantA}"), metaA);
                await bus.PublishEnvelopeAsync(envA);
            }));

            tasks.Add(Task.Run(async () =>
            {
                var metaB = new EventMetadataBuilder().WithTenantId(tenantB).Build();
                var envB = EventEnvelope.Create(new SecureSecurityEvent(EventId.New(), DateTimeOffset.UtcNow, $"Data-{tenantB}"), metaB);
                await bus.PublishEnvelopeAsync(envB);
            }));
        }

        await Task.WhenAll(tasks);

        TenantAuditConsumer.Received.Count.Should().Be(iterations * 2);

        foreach (var (tenant, data) in TenantAuditConsumer.Received)
        {
            data.Should().Be($"Data-{tenant}", "Ambient TenantId must match the payload's intended tenant without cross-contamination.");
        }
    }

    [Fact]
    public void EventMetadata_IsImmutable_WithHeaderCreatesNewInstanceWithoutMutatingOriginal()
    {
        var original = new EventMetadataBuilder()
            .WithCorrelationId("corr-123")
            .WithHeader("X-Security-Level", "High")
            .Build();

        var modified = original.WithHeader("X-Security-Level", "Compromised");

        original.TryGetHeader("X-Security-Level", out var originalVal).Should().BeTrue();
        originalVal.Should().Be("High", "Original metadata instance must remain immutable.");

        modified.TryGetHeader("X-Security-Level", out var modifiedVal).Should().BeTrue();
        modifiedVal.Should().Be("Compromised");
    }

    [Fact]
    public void EventEnvelope_IsImmutable_RecordCopyDoesNotMutateSource()
    {
        var evt = new SecureSecurityEvent(EventId.New(), DateTimeOffset.UtcNow, "secret-data");
        var env1 = EventEnvelope.Create(evt);
        var env2 = env1 with { OccurredAt = DateTimeOffset.UtcNow.AddHours(1) };

        env1.OccurredAt.Should().Be(evt.OccurredAt);
        env2.OccurredAt.Should().BeAfter(env1.OccurredAt);
    }
}

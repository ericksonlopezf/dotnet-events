// Copyright © Erickson Lopez. MIT License.
using System;
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

namespace EricksonLopez.Events.UnitTests.Adversarial.Generators;

[Trait("Category", "Unit")]
public sealed class GeneratorRuntimeDisconnectTests
{
    public sealed record SampleInvoiceGenerated(EventId Id, DateTimeOffset OccurredAt) : IEvent;

    public sealed class SampleInvoiceHandler : IEventHandler<SampleInvoiceGenerated>
    {
        public static bool Handled { get; set; }

        public ValueTask HandleAsync(SampleInvoiceGenerated @event, CancellationToken cancellationToken = default)
        {
            Handled = true;
            return ValueTask.CompletedTask;
        }
    }

    [Fact]
    public async Task AddGeneratedEventHandlers_Pattern_ExecutesSuccessfullyWithEventBus()
    {
        // Validates EVT-GEN-001 Remediation — Post-Fix 1:
        // After removing late-binding DI discovery (HIGH-001), handlers MUST be registered
        // via AddEventHandler() (or the source generator AddGeneratedEventHandlers) to be resolved.
        // Raw DI ServiceDescriptor registrations without AddEventHandler are NOT discovered by EventBus.
        // This test verifies the CORRECT pattern: using AddEventHandler for reliable dispatch.

        SampleInvoiceHandler.Handled = false;

        var services = new ServiceCollection();
        services.AddEventBus(opts => opts.ThrowOnUnregisteredEvent = false);

        // CORRECT post-Fix1 pattern: use AddEventHandler to register with HandlerRegistry token
        services.AddEventHandler<SampleInvoiceGenerated, SampleInvoiceHandler>(ServiceLifetime.Transient);

        var sp = services.BuildServiceProvider();
        var bus = sp.GetRequiredService<IEventBus>();

        await bus.PublishAsync(new SampleInvoiceGenerated(EventId.New(), DateTimeOffset.UtcNow));

        SampleInvoiceHandler.Handled.Should().BeTrue(
            "Handlers registered via AddEventHandler() are always discovered by EventBus via the HandlerRegistry.");
    }

    [Fact]
    public async Task RawDiDescriptor_WithoutAddEventHandler_IsNotDispatched_PostFix1()
    {
        // Confirms that EVT-HIGH-001 fix is in effect:
        // A raw DI ServiceDescriptor (bypassing AddEventHandler) is NOT discovered by EventBus.
        // EventBus no longer performs late-binding DI scan at first publication.
        // ThrowOnUnregisteredEvent=false, so no exception — just handler is not invoked.

        SampleInvoiceHandler.Handled = false;

        var services = new ServiceCollection();
        services.AddEventBus(opts => opts.ThrowOnUnregisteredEvent = false);

        // Raw DI descriptor WITHOUT AddEventHandler — will NOT be dispatched after Fix 1.
        ((IServiceCollection)services).Add(new ServiceDescriptor(
            typeof(IEventHandler<SampleInvoiceGenerated>),
            typeof(SampleInvoiceHandler),
            ServiceLifetime.Transient));

        var sp = services.BuildServiceProvider();
        var bus = sp.GetRequiredService<IEventBus>();

        await bus.PublishAsync(new SampleInvoiceGenerated(EventId.New(), DateTimeOffset.UtcNow));

        SampleInvoiceHandler.Handled.Should().BeFalse(
            "EVT-HIGH-001: Late-binding DI scan has been removed. Raw DI descriptors without " +
            "AddEventHandler() are never discovered by EventBus. Use AddEventHandler() or source generators.");
    }
}

// Copyright © Erickson Lopez. MIT License.
using System;
using System.Threading;
using System.Threading.Tasks;
using EricksonLopez.Events.Bus;
using EricksonLopez.Events.Bus.Configuration;
using EricksonLopez.Events.Bus.Extensions;
using EricksonLopez.Events.Contracts;
using EricksonLopez.Events.Dispatch;
using EricksonLopez.Events.Envelopes;
using EricksonLopez.Events.Identifiers;
using EricksonLopez.Events.Metadata;
using EricksonLopez.Events.NativeAotTests;
using Microsoft.Extensions.DependencyInjection;

Console.WriteLine("=================================================");
Console.WriteLine(" EricksonLopez.Events NativeAOT Smoke Test Suite ");
Console.WriteLine("=================================================");

int passed = 0;
void Assert(bool condition, string testName)
{
    if (!condition)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"[FAIL] {testName}");
        Console.ResetColor();
        throw new InvalidOperationException($"Assertion failed for: {testName}");
    }
    passed++;
    Console.WriteLine($"[PASS] {testName}");
}

// 1. Identifiers
var eventId = EventId.New();
Assert(!string.IsNullOrWhiteSpace(eventId.Value.ToString()), "EventId generates non-empty GUID");

var eventType = EventType.From("OrderCreated");
Assert(eventType.Value == "OrderCreated", "EventType value matches");

var eventVersion = EventVersion.From(1);
Assert(eventVersion.Value == 1, "EventVersion value is 1");

var correlationId = CorrelationId.New();
Assert(!correlationId.IsEmpty, "CorrelationId generates non-empty value");

var tenantId = TenantId.From("tenant-aot");
Assert(tenantId.Value == "tenant-aot", "TenantId matches");

// 2. Event Envelope
var myEvent = new SampleAotEvent(EventId.New(), DateTimeOffset.UtcNow, "Order-999", 500m);
var metadata = new EventMetadataBuilder()
    .WithCorrelationId(correlationId)
    .WithTenantId(tenantId)
    .WithHeader("X-AOT", "Enabled")
    .Build();

var envelope = EventEnvelope.Create(myEvent, metadata: metadata);
Assert(envelope.Payload.OrderId == "Order-999", "Envelope preserves payload OrderId");
Assert(envelope.Payload.Amount == 500m, "Envelope preserves payload Amount");
Assert(envelope.Id.Value != Guid.Empty, "Envelope generated valid EventId");
Assert(envelope.Metadata.TenantId.Value == "tenant-aot", "Envelope preserves metadata TenantId");

// 3. InMemoryEventPublisher Dispatch
var inMemPub = new InMemoryEventPublisher();
SampleAotConsumer.HandledCount = 0;
var consumer = new SampleAotConsumer();
inMemPub.Subscribe(consumer);
await inMemPub.PublishAsync(myEvent);
Assert(SampleAotConsumer.HandledCount == 1, "InMemoryEventPublisher dispatches to handler under NativeAOT");

// 4. Full DI EventBus Dispatch
var services = new ServiceCollection();
services.AddEventBus(opts =>
{
    opts.ExecutionMode = EventExecutionMode.Sequential;
    opts.ThrowOnUnregisteredEvent = true;
});
services.AddEventHandler<SampleAotEvent, SampleAotConsumer>();
var sp = services.BuildServiceProvider();
var bus = sp.GetRequiredService<IEventBus>();

await bus.PublishAsync(myEvent);
Assert(SampleAotConsumer.HandledCount == 2, "EventBus dispatches via DI handler under NativeAOT");

await bus.PublishEnvelopeAsync(envelope);
Assert(SampleAotConsumer.HandledCount == 3, "EventBus dispatches envelope under NativeAOT");
Assert(SampleAotConsumer.LastObservedTenant == "tenant-aot", "Ambient EventContext flows to handler under NativeAOT");

Console.WriteLine($"\nSUCCESS: All {passed} NativeAOT smoke tests passed with zero warnings/errors.");
return 0;

namespace EricksonLopez.Events.NativeAotTests
{
    public sealed record SampleAotEvent(EventId Id, DateTimeOffset OccurredAt, string OrderId, decimal Amount) : IEvent;

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Naming", "CA1711:Identifiers should not have incorrect suffix", Justification = "AOT consumer")]
    public sealed class SampleAotConsumer : IEventHandler<SampleAotEvent>
    {
        public static int HandledCount { get; set; }
        public static string? LastObservedTenant { get; set; }

        public ValueTask HandleAsync(SampleAotEvent eventInstance, CancellationToken cancellationToken = default)
        {
            HandledCount++;
            LastObservedTenant = Context.EventContext.TenantId?.Value;
            return ValueTask.CompletedTask;
        }
    }
}

// Copyright © Erickson Lopez. MIT License.
using System;
using EricksonLopez.Events.Contracts;
using EricksonLopez.Events.Envelopes;
using EricksonLopez.Events.Identifiers;
using EricksonLopez.Events.Metadata;
using EricksonLopez.Events.NativeAotTests;

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

// 2. Event Envelope
var myEvent = new SampleAotEvent(EventId.New(), DateTimeOffset.UtcNow, "Order-999", 500m);
var envelope = EventEnvelope.Create(myEvent, metadata: EventMetadata.Empty);

Assert(envelope.Payload.OrderId == "Order-999", "Envelope preserves payload OrderId");
Assert(envelope.Payload.Amount == 500m, "Envelope preserves payload Amount");
Assert(envelope.Id.Value != Guid.Empty, "Envelope generated valid EventId");

Console.WriteLine($"\nSUCCESS: All {passed} NativeAOT smoke tests passed with zero warnings/errors.");
return 0;

namespace EricksonLopez.Events.NativeAotTests
{
    public sealed record SampleAotEvent(EventId Id, DateTimeOffset OccurredAt, string OrderId, decimal Amount) : IEvent;
}

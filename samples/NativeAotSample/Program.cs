// Copyright © Erickson Lopez. MIT License.
using System;
using System.Threading;
using System.Threading.Tasks;

namespace NativeAotSample;

using System.Text.Json;
using System.Text.Json.Serialization;
using EricksonLopez.Events.Attributes;
using EricksonLopez.Events.Contracts;
using EricksonLopez.Events.Dispatch;
using EricksonLopez.Events.Envelopes;
using EricksonLopez.Events.Identifiers;
using EricksonLopez.Events.Metadata;
using EricksonLopez.Events.Serialization.SystemTextJson.Converters;

[EventName("sample.user-onboarded")]
[EventVersion(1)]
[EventSource("identity-service")]
public sealed record UserOnboardedIntegrationEvent(
    EventId Id,
    string UserId,
    string Email,
    DateTimeOffset OccurredAt) : IIntegrationEvent;

[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    PropertyNameCaseInsensitive = true,
    WriteIndented = true,
    Converters = [
        typeof(EventIdJsonConverter),
        typeof(EventTypeJsonConverter),
        typeof(EventVersionJsonConverter),
        typeof(CorrelationIdJsonConverter),
        typeof(CausationIdJsonConverter),
        typeof(TenantIdJsonConverter),
        typeof(EventMetadataJsonConverter)
    ])]
[JsonSerializable(typeof(EventId))]
[JsonSerializable(typeof(EventType))]
[JsonSerializable(typeof(EventVersion))]
[JsonSerializable(typeof(CorrelationId))]
[JsonSerializable(typeof(CausationId))]
[JsonSerializable(typeof(TenantId))]
[JsonSerializable(typeof(EventMetadata))]
[JsonSerializable(typeof(UserOnboardedIntegrationEvent))]
[JsonSerializable(typeof(EventEnvelope<UserOnboardedIntegrationEvent>))]
internal sealed partial class AppJsonContext : JsonSerializerContext
{
}

public static class Program
{
    public static async Task<int> Main()
    {
        Console.WriteLine("==========================================================");
        Console.WriteLine(" EricksonLopez.Events - Native AOT & Trimming Smoke Test ");
        Console.WriteLine("==========================================================");

        // 1. Create event with Guid v7 EventId
        var eventId = EventId.New();
        var now = DateTimeOffset.UtcNow;
        var @event = new UserOnboardedIntegrationEvent(eventId, "usr_9981", "developer@ericksonlopez.dev", now);

        Console.WriteLine($"[1] Created EventId: {@event.Id} (Guid v7 Version: {@event.Id.Value.Version})");

        // 2. Build immutable metadata
        var metadata = new EventMetadataBuilder()
            .WithCorrelationId(CorrelationId.New())
            .WithCausationId(CausationId.From("CMD-SIGNUP-01"))
            .WithTenantId(TenantId.From("enterprise-corp"))
            .WithSource("identity-service")
            .WithHeader("X-Environment", "Production-AOT")
            .Build();

        Console.WriteLine($"[2] Built EventMetadata with CorrelationId: {metadata.CorrelationId}");

        // 3. Wrap in typed EventEnvelope
        var envelope = EventEnvelope.Create(@event, metadata);
        Console.WriteLine($"[3] Wrapped in EventEnvelope (Type: '{envelope.Type}', Version: {envelope.Version})");

        // 4. AOT JSON Serialization using AppJsonContext JsonTypeInfo
        var json = JsonSerializer.Serialize(envelope, AppJsonContext.Default.EventEnvelopeUserOnboardedIntegrationEvent);
        Console.WriteLine("\n[4] Serialized EventEnvelope JSON (AOT-Safe):");
        Console.WriteLine(json);

        // 5. AOT JSON Deserialization using AppJsonContext JsonTypeInfo
        var deserialized = JsonSerializer.Deserialize(json, AppJsonContext.Default.EventEnvelopeUserOnboardedIntegrationEvent);
        if (deserialized is null || deserialized.Id != eventId)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("\n[FAIL] Deserialization check failed!");
            Console.ResetColor();
            return 1;
        }

        Console.WriteLine("\n[5] Deserialization successful and validated!");

        // 6. In-Memory Dispatch
        var publisher = new InMemoryEventPublisher();
        var handler = new UserOnboardedHandler();
        publisher.Subscribe(handler);

        Console.WriteLine("[6] Dispatching event in-memory to subscribed handler...");
        await publisher.PublishAsync(@event);

        if (!handler.WasHandled)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("\n[FAIL] Handler did not receive the event!");
            Console.ResetColor();
            return 1;
        }

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("\n[SUCCESS] All Native AOT & Trimming operations completed flawlessly!");
        Console.ResetColor();
        return 0;
    }

    private sealed class UserOnboardedHandler : IEventHandler<UserOnboardedIntegrationEvent>
    {
        public bool WasHandled { get; private set; }

        public ValueTask HandleAsync(UserOnboardedIntegrationEvent eventInstance, CancellationToken cancellationToken = default)
        {
            Console.WriteLine($"    -> Handler executed for user {eventInstance.Email} (EventId: {eventInstance.Id})");
            WasHandled = true;
            return ValueTask.CompletedTask;
        }
    }
}





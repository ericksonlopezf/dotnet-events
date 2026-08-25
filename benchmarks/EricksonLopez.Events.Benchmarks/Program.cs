// Copyright © Erickson Lopez. MIT License.
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace EricksonLopez.Events.Benchmarks;

using System.Text.Json;
using System.Text.Json.Serialization;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using EricksonLopez.Events.Attributes;
using EricksonLopez.Events.Bus;
using EricksonLopez.Events.Bus.Configuration;
using EricksonLopez.Events.Bus.Middleware;
using EricksonLopez.Events.Bus.Registry;
using EricksonLopez.Events.Contracts;
using EricksonLopez.Events.Dispatch;
using EricksonLopez.Events.Envelopes;
using EricksonLopez.Events.Identifiers;
using EricksonLopez.Events.Metadata;
using EricksonLopez.Events.Registry;
using EricksonLopez.Events.Serialization.SystemTextJson;
using Microsoft.Extensions.DependencyInjection;

public static class Program
{
    public static void Main(string[] args)
    {
        BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args);
    }
}

[EventName("benchmark.order-placed")]
[EventVersion(1)]
[EventSource("ordering.service")]
public sealed record BenchmarkOrderPlaced(
    EventId Id,
    Guid OrderId,
    decimal Total,
    DateTimeOffset OccurredAt) : IIntegrationEvent;

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase, PropertyNameCaseInsensitive = true)]
[JsonSerializable(typeof(EventId))]
[JsonSerializable(typeof(EventType))]
[JsonSerializable(typeof(EventVersion))]
[JsonSerializable(typeof(CorrelationId))]
[JsonSerializable(typeof(CausationId))]
[JsonSerializable(typeof(TenantId))]
[JsonSerializable(typeof(EventMetadata))]
[JsonSerializable(typeof(BenchmarkOrderPlaced))]
[JsonSerializable(typeof(EventEnvelope<BenchmarkOrderPlaced>))]
internal sealed partial class BenchmarkJsonContext : JsonSerializerContext
{
}

[MemoryDiagnoser]
[ShortRunJob]
public class EventBenchmarks
{
    private BenchmarkOrderPlaced _event = null!;
    private EventEnvelope<BenchmarkOrderPlaced> _envelope = null!;
    private EventMetadata _metadata = null!;
    private JsonSerializerOptions _options = null!;
    private InMemoryEventPublisher _publisher = null!;
    private string _serializedJson = string.Empty;

    [GlobalSetup]
    public void Setup()
    {
        var id = EventId.New();
        var now = DateTimeOffset.UtcNow;
        _event = new BenchmarkOrderPlaced(id, Guid.NewGuid(), 199.99m, now);

        _metadata = new EventMetadataBuilder()
            .WithCorrelationId(CorrelationId.New())
            .WithCausationId(CausationId.From("CMD-101"))
            .WithTenantId(TenantId.From("tenant-us"))
            .WithSource("benchmark-service")
            .WithHeader("X-Benchmark", "True")
            .Build();

        _envelope = EventEnvelope.Create(_event, _metadata);

        _options = new JsonSerializerOptions
        {
            TypeInfoResolver = BenchmarkJsonContext.Default,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true
        };
        _options.AddEventsConverters();

        _serializedJson = JsonSerializer.Serialize(_envelope, _options);

        _publisher = new InMemoryEventPublisher();
        _publisher.Subscribe(new NoopHandler());
    }

    [Benchmark(Baseline = true)]
    public EventId EventId_New() => EventId.New();

    [Benchmark]
    public bool EventId_TryFormat_ZeroAlloc()
    {
        Span<char> buffer = stackalloc char[36];
        return _event.Id.TryFormat(buffer, out _);
    }

    [Benchmark]
    public EventEnvelope<BenchmarkOrderPlaced> Envelope_Create() =>
        EventEnvelope.Create(_event, _metadata);

    [Benchmark]
    public string Envelope_Serialize_Json() =>
        JsonSerializer.Serialize(_envelope, _options);

    [Benchmark]
    public EventEnvelope<BenchmarkOrderPlaced>? Envelope_Deserialize_Json() =>
        JsonSerializer.Deserialize<EventEnvelope<BenchmarkOrderPlaced>>(_serializedJson, _options);

    [Benchmark]
    public ValueTask Event_Publish_InMemory() =>
        _publisher.PublishAsync(_event);

    private sealed class NoopHandler : IEventHandler<BenchmarkOrderPlaced>
    {
        public ValueTask HandleAsync(BenchmarkOrderPlaced eventInstance, CancellationToken cancellationToken = default) =>
            ValueTask.CompletedTask;
    }
}

[MemoryDiagnoser]
[ShortRunJob]
public class RegistryBenchmarks
{
    private EventTypeRegistry _registryN1 = null!;
    private EventTypeRegistry _registryN10 = null!;
    private EventTypeRegistry _registryN100 = null!;
    private readonly EventType _targetEventType = EventType.From("benchmark.order-placed");
    private readonly Type _targetClrType = typeof(BenchmarkOrderPlaced);

    [GlobalSetup]
    public void Setup()
    {
        var listN1 = new List<EventTypeDescriptor>
        {
            new(typeof(BenchmarkOrderPlaced), _targetEventType, EventVersion.V1, "ordering.service")
        };
        _registryN1 = new EventTypeRegistry(listN1);

        var listN10 = new List<EventTypeDescriptor>(listN1);
        for (int i = 1; i < 10; i++)
        {
            listN10.Add(new(typeof(DummyEvent), EventType.From($"dummy.event.{i}"), EventVersion.V1, "dummy.service"));
        }
        _registryN10 = new EventTypeRegistry(listN10);

        var listN100 = new List<EventTypeDescriptor>(listN1);
        for (int i = 1; i < 100; i++)
        {
            listN100.Add(new(typeof(DummyEvent), EventType.From($"dummy.event.{i}"), EventVersion.V1, "dummy.service"));
        }
        _registryN100 = new EventTypeRegistry(listN100);
    }

    [Benchmark(Baseline = true)]
    public EventTypeDescriptor StaticRegistry_GetDescriptor_Cached() =>
        StaticEventTypeRegistry.GetDescriptor<BenchmarkOrderPlaced>();

    [Benchmark]
    public bool Registry_TryGetDescriptor_ByType_N1() =>
        _registryN1.TryGetDescriptor(_targetClrType, out _);

    [Benchmark]
    public bool Registry_TryGetDescriptor_ByType_N10() =>
        _registryN10.TryGetDescriptor(_targetClrType, out _);

    [Benchmark]
    public bool Registry_TryGetDescriptor_ByType_N100() =>
        _registryN100.TryGetDescriptor(_targetClrType, out _);

    [Benchmark]
    public bool Registry_TryGetDescriptor_ByEventType_N100() =>
        _registryN100.TryGetDescriptor(_targetEventType, out _);

    private sealed record DummyEvent(EventId Id, DateTimeOffset OccurredAt) : IEvent;
}

[MemoryDiagnoser]
[ShortRunJob]
public class DispatchScenariosBenchmarks
{
    private BenchmarkOrderPlaced _event = null!;
    private InMemoryEventPublisher _pub1 = null!;
    private InMemoryEventPublisher _pub5 = null!;
    private InMemoryEventPublisher _pub10 = null!;
    private InMemoryEventPublisher _pub50 = null!;

    [GlobalSetup]
    public void Setup()
    {
        _event = new BenchmarkOrderPlaced(EventId.New(), Guid.NewGuid(), 99.50m, DateTimeOffset.UtcNow);

        _pub1 = CreatePublisherWithHandlers(1);
        _pub5 = CreatePublisherWithHandlers(5);
        _pub10 = CreatePublisherWithHandlers(10);
        _pub50 = CreatePublisherWithHandlers(50);
    }

    private static InMemoryEventPublisher CreatePublisherWithHandlers(int count)
    {
        var pub = new InMemoryEventPublisher();
        for (int i = 0; i < count; i++)
        {
            pub.Subscribe(new FastHandler());
        }
        return pub;
    }

    [Benchmark(Baseline = true)]
    public ValueTask Publish_1Handler() => _pub1.PublishAsync(_event);

    [Benchmark]
    public ValueTask Publish_5Handlers() => _pub5.PublishAsync(_event);

    [Benchmark]
    public ValueTask Publish_10Handlers() => _pub10.PublishAsync(_event);

    [Benchmark]
    public ValueTask Publish_50Handlers() => _pub50.PublishAsync(_event);

    [Benchmark]
    public async ValueTask Publish_100Events_Sequential()
    {
        for (int i = 0; i < 100; i++)
        {
            await _pub1.PublishAsync(_event).ConfigureAwait(false);
        }
    }

    [Benchmark]
    public async ValueTask Publish_1000Events_Sequential()
    {
        for (int i = 0; i < 1000; i++)
        {
            await _pub1.PublishAsync(_event).ConfigureAwait(false);
        }
    }

    private sealed class FastHandler : IEventHandler<BenchmarkOrderPlaced>
    {
        public ValueTask HandleAsync(BenchmarkOrderPlaced eventInstance, CancellationToken cancellationToken = default) =>
            ValueTask.CompletedTask;
    }
}

[MemoryDiagnoser]
[ShortRunJob]
public class EventBusBenchmarks
{
    private BenchmarkOrderPlaced _event = null!;
    private EventBus _busSequential1 = null!;
    private EventBus _busSequential5 = null!;
    private EventBus _busParallel5 = null!;
    private EventBus _busWithMiddleware = null!;

    [GlobalSetup]
    public void Setup()
    {
        _event = new BenchmarkOrderPlaced(EventId.New(), Guid.NewGuid(), 99.50m, DateTimeOffset.UtcNow);

        var services = new ServiceCollection();
        services.AddSingleton<BenchmarkHandler>();
        var sp = services.BuildServiceProvider();

        _busSequential1 = CreateBus(sp, 1, EventExecutionMode.Sequential, false);
        _busSequential5 = CreateBus(sp, 5, EventExecutionMode.Sequential, false);
        _busParallel5 = CreateBus(sp, 5, EventExecutionMode.Parallel, false);
        _busWithMiddleware = CreateBus(sp, 1, EventExecutionMode.Sequential, true);
    }

    private static EventBus CreateBus(IServiceProvider sp, int handlerCount, EventExecutionMode mode, bool withMiddleware)
    {
        var registry = new HandlerRegistry();
        for (int i = 0; i < handlerCount; i++)
        {
            registry.Register(
                typeof(BenchmarkOrderPlaced),
                new HandlerDescriptor(
                    typeof(BenchmarkHandler),
                    typeof(IEventHandler<BenchmarkOrderPlaced>),
                    (inst, evt, ct) => ((BenchmarkHandler)inst).HandleAsync((BenchmarkOrderPlaced)evt, ct)));
        }

        var options = new EventBusOptions
        {
            ExecutionMode = mode,
            ThrowOnUnregisteredEvent = false
        };

        var middlewares = withMiddleware
            ? new IEventMiddleware[] { new BenchmarkMiddleware() }
            : null;

        return new EventBus(registry, sp, options, middlewares);
    }

    [Benchmark(Baseline = true)]
    public ValueTask EventBus_Sequential_1Handler() =>
        _busSequential1.PublishAsync(_event);

    [Benchmark]
    public ValueTask EventBus_Sequential_5Handlers() =>
        _busSequential5.PublishAsync(_event);

    [Benchmark]
    public ValueTask EventBus_Parallel_5Handlers() =>
        _busParallel5.PublishAsync(_event);

    [Benchmark]
    public ValueTask EventBus_WithMiddleware_1Handler() =>
        _busWithMiddleware.PublishAsync(_event);

    private sealed class BenchmarkHandler : IEventHandler<BenchmarkOrderPlaced>
    {
        public ValueTask HandleAsync(BenchmarkOrderPlaced eventInstance, CancellationToken cancellationToken = default) =>
            ValueTask.CompletedTask;
    }

    private sealed class BenchmarkMiddleware : IEventMiddleware
    {
        public ValueTask InvokeAsync<TEvent>(
            TEvent @event,
            EventMiddlewareDelegate<TEvent> next,
            CancellationToken cancellationToken = default) where TEvent : IEvent =>
            next(@event, cancellationToken);
    }
}





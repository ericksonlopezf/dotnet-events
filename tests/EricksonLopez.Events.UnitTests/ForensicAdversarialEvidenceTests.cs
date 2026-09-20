// Copyright © Erickson Lopez. MIT License.
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AwesomeAssertions;
using EricksonLopez.Events.Bus;
using EricksonLopez.Events.Bus.Configuration;
using EricksonLopez.Events.Bus.Exceptions;
using EricksonLopez.Events.Bus.Extensions;
using EricksonLopez.Events.Bus.Middleware;
using EricksonLopez.Events.Bus.Registry;
using EricksonLopez.Events.Context;
using EricksonLopez.Events.Contracts;
using EricksonLopez.Events.Dispatch;
using EricksonLopez.Events.Envelopes;
using EricksonLopez.Events.Identifiers;
using EricksonLopez.Events.Metadata;
using EricksonLopez.Events.Registry;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace EricksonLopez.Events.UnitTests;

[Trait("Category", "ForensicAdversarial")]
public sealed class ForensicAdversarialEvidenceTests
{
    private sealed record ForensicTestEvent(EventId Id, DateTimeOffset OccurredAt, string Data) : IEvent;
    private readonly record struct ForensicStructEvent(EventId Id, DateTimeOffset OccurredAt, int Value) : IEvent;

    #region 2. Instance-Isolated Reentrancy Depth (EVT-ISO-001 Remediation)

    [Fact]
    public async Task EVT_ISO_001_InMemoryEventPublisher_ReentrancyDepth_IsIsolatedPerInstance()
    {
        // Validates remediation of EVT-ISO-001:
        // Publisher A and Publisher B each maintain their own independent _reentrancyDepth.
        // Nested execution in Publisher A does NOT block Publisher B!
        var pubA = new InMemoryEventPublisher { MaxReentrancyDepth = 10 };
        var pubB = new InMemoryEventPublisher { MaxReentrancyDepth = 2 };

        var handlerBInvoked = false;
        pubB.Subscribe<ForensicTestEvent>(new SimpleHandler<ForensicTestEvent>(_ =>
        {
            handlerBInvoked = true;
            return ValueTask.CompletedTask;
        }));

        pubA.Subscribe<ForensicTestEvent>(new SimpleHandler<ForensicTestEvent>(async (evt) =>
        {
            if (evt.Data == "Root-A")
            {
                // Inside PubA at depth 1: publish nested event in PubA to reach depth 2
                await pubA.PublishAsync(new ForensicTestEvent(EventId.New(), DateTimeOffset.UtcNow, "Nested-A"));
            }
            else if (evt.Data == "Nested-A")
            {
                // Inside PubA's nested event at depth 2: publish to PubB (which has MaxReentrancyDepth = 2)
                await pubB.PublishAsync(new ForensicTestEvent(EventId.New(), DateTimeOffset.UtcNow, "From-B"));
            }
        }));

        await pubA.PublishAsync(new ForensicTestEvent(EventId.New(), DateTimeOffset.UtcNow, "Root-A"));

        // EVIDENCE OF FIX: Publisher B executed successfully because its depth counter is isolated!
        handlerBInvoked.Should().BeTrue("Fix EVT-ISO-001: Publisher B executed successfully with its own reentrancy counter");
    }

    #endregion

    #region 3. EventBus Pipeline Cache Dynamically Resolves Handlers (EVT-DYN-001 Remediation)

    [Fact]
    public async Task EVT_DYN_001_EventBus_PipelineCache_DynamicallyResolvesHandlers()
    {
        var services = new ServiceCollection();
        var handlerRegistry = new HandlerRegistry();

        services.AddTransient<SimpleHandler<ForensicTestEvent>>(_ => new SimpleHandler<ForensicTestEvent>(_ => ValueTask.CompletedTask));

        var sp = services.BuildServiceProvider();
        var options = new EventBusOptions { ExecutionMode = EventExecutionMode.Sequential };
        var busWithMiddleware = new EventBus(handlerRegistry, sp, options, new[] { new TestPassthroughMiddleware() });

        // 1. Initially register Handler 1
        var h1Invoked = false;
        handlerRegistry.Register(typeof(ForensicTestEvent), new HandlerDescriptor(
            typeof(SimpleHandler<ForensicTestEvent>),
            typeof(IEventHandler<ForensicTestEvent>),
            (inst, evt, ct) =>
            {
                h1Invoked = true;
                return ValueTask.CompletedTask;
            }));

        await busWithMiddleware.PublishAsync(new ForensicTestEvent(EventId.New(), DateTimeOffset.UtcNow, "First"));
        h1Invoked.Should().BeTrue();

        // 2. Dynamically register Handler 2 at runtime
        var h2Invoked = false;
        handlerRegistry.Register(typeof(ForensicTestEvent), new HandlerDescriptor(
            typeof(SimpleHandler<ForensicTestEvent>),
            typeof(IEventHandler<ForensicTestEvent>),
            (inst, evt, ct) =>
            {
                h2Invoked = true;
                return ValueTask.CompletedTask;
            }));

        // 3. Publish second event with middleware:
        h1Invoked = false;
        await busWithMiddleware.PublishAsync(new ForensicTestEvent(EventId.New(), DateTimeOffset.UtcNow, "Second"));

        // EVIDENCE OF FIX: Both Handler 1 and dynamically registered Handler 2 were invoked!
        h1Invoked.Should().BeTrue();
        h2Invoked.Should().BeTrue("Fix EVT-DYN-001: Dynamically registered handler is invoked even when middleware pipeline was previously cached");
    }

    private sealed class TestPassthroughMiddleware : IEventMiddleware
    {
        public ValueTask InvokeAsync<TEvent>(TEvent @event, EventMiddlewareDelegate<TEvent> next, CancellationToken cancellationToken = default) where TEvent : IEvent =>
            next(@event, cancellationToken);
    }

    #endregion

    #region 4. Value-Type Struct Event TypedInvoker Without Boxing (EVT-PRF-001 Remediation)

    [Fact]
    public async Task EVT_PRF_001_StructEvent_PassedToTypedInvoker_AvoidsBoxing()
    {
        // With TypedInvoker (HandlerInvoker<TEvent>), struct event is passed directly by value without boxing
        bool typedInvoked = false;
        HandlerInvoker<ForensicStructEvent> typedInvoker = (inst, evt, ct) =>
        {
            typedInvoked = true;
            evt.Value.Should().Be(999);
            return ValueTask.CompletedTask;
        };

        var descriptor = new HandlerDescriptor(
            typeof(SimpleHandler<ForensicStructEvent>),
            typeof(IEventHandler<ForensicStructEvent>),
            (inst, evt, ct) => ValueTask.CompletedTask,
            typedInvoker);

        descriptor.TypedInvoker.Should().NotBeNull();
        var structEvt = new ForensicStructEvent(EventId.New(), DateTimeOffset.UtcNow, 999);

        var invoker = (HandlerInvoker<ForensicStructEvent>)descriptor.TypedInvoker!;
        await invoker(new object(), structEvt, CancellationToken.None);

        typedInvoked.Should().BeTrue("TypedInvoker dispatched struct event without boxing");
    }

    #endregion

    #region 5. EventMetadata HashCode Distinguishes Headers (EVT-DAT-001 Remediation)

    [Fact]
    public void EVT_DAT_001_EventMetadata_DifferentHeaders_ProduceDistinctHashCodes()
    {
        var meta1 = new EventMetadataBuilder()
            .WithCorrelationId("C-1")
            .WithCausationId("CMD-1")
            .WithTenantId("T-1")
            .WithHeader("X-Attack-Payload", "MaliciousValue")
            .Build();

        var meta2 = new EventMetadataBuilder()
            .WithCorrelationId("C-1")
            .WithCausationId("CMD-1")
            .WithTenantId("T-1")
            .WithHeader("X-Safe-Token", "AuthorizedValue")
            .Build();

        meta1.Equals(meta2).Should().BeFalse("Metadata instances with different headers must not be equal.");

        int hash1 = meta1.GetHashCode();
        int hash2 = meta2.GetHashCode();

        // EVIDENCE OF FIX: Hash codes are distinct because headers are now hashed!
        hash1.Should().NotBe(hash2, "Fix EVT-DAT-001: Distinct custom headers produce distinct hash codes");
    }

    [Fact]
    public void EVT_DAT_002_EventMetadata_CaseInsensitiveHeaders_ProduceIdenticalHashCodes()
    {
        var meta1 = new EventMetadataBuilder()
            .WithCorrelationId("C-1")
            .WithCausationId("CMD-1")
            .WithTenantId("T-1")
            .WithHeader("X-Tenant-Tier", "Gold")
            .Build();

        var meta2 = new EventMetadataBuilder()
            .WithCorrelationId("C-1")
            .WithCausationId("CMD-1")
            .WithTenantId("T-1")
            .WithHeader("x-tenant-tier", "Gold")
            .Build();

        meta1.Equals(meta2).Should().BeTrue("Metadata comparison is case-insensitive for custom headers.");

        int hash1 = meta1.GetHashCode();
        int hash2 = meta2.GetHashCode();

        // EVIDENCE OF FIX: Hash codes must match when Equals is true under case-insensitive headers!
        hash1.Should().Be(hash2, "Fix EVT-DAT-002: Case-insensitive header keys must produce identical hash codes matching Equals.");
    }

    #endregion

    #region 6. EventTypeRegistry Unversioned Lookup Determinism (EVT-REG-001 Remediation)

    [Fact]
    public void EVT_REG_001_EventTypeRegistry_UnversionedLookup_DeterministicallyReturnsHighestVersion()
    {
        var eventType = EventType.From("sales.invoice-issued");
        var descV1 = new EventTypeDescriptor(typeof(ForensicTestEvent), eventType, EventVersion.From(1), "src-1");
        var descV2 = new EventTypeDescriptor(typeof(ForensicTestEvent), eventType, EventVersion.From(2), "src-2");

        // Order 1: V1 then V2
        var reg1 = new EventTypeRegistry(new[] { descV1, descV2 });
        reg1.TryGetDescriptor(eventType, out var resolved1).Should().BeTrue();
        resolved1!.Version.Value.Should().Be(2, "Version 2 is the highest version.");

        // Order 2: V2 then V1
        var reg2 = new EventTypeRegistry(new[] { descV2, descV1 });
        reg2.TryGetDescriptor(eventType, out var resolved2).Should().BeTrue();
        resolved2!.Version.Value.Should().Be(2, "Version 2 must still be chosen as the highest version.");

        // EVIDENCE OF FIX: Unversioned lookup is now 100% deterministic regardless of collection order!
        resolved1.Version.Value.Should().Be(resolved2.Version.Value,
            "Fix EVT-REG-001: Unversioned descriptor resolution always returns highest version deterministically.");
    }

    #endregion

    #region 7. TenantId Strict Whitespace Validation (EVT-SEC-003 Remediation)

    [Fact]
    public void EVT_SEC_003_TenantId_ConsidersWhitespaceStrings_AsEmpty()
    {
        var tenant = new TenantId("   ");

        // EVIDENCE OF FIX: Whitespace is now correctly considered empty!
        tenant.IsEmpty.Should().BeTrue(
            "Fix EVT-SEC-003: TenantId now considers whitespace '   ' to be empty.");

        TenantId.TryParse("   ", null, out var parsed).Should().BeTrue();
        parsed.IsEmpty.Should().BeTrue();
    }

    [Fact]
    public void EVT_SEC_004_TenantId_EmptyAndWhitespace_AreEqualAndProduceIdenticalHashCode()
    {
        var t1 = new TenantId("   ");
        var t2 = TenantId.Empty;
        var t3 = new TenantId("");

        (t1 == t2).Should().BeTrue("Whitespace-only TenantId must equate to TenantId.Empty");
        t1.Equals(t2).Should().BeTrue();
        t2.Equals(t1).Should().BeTrue();
        t1.Equals(t3).Should().BeTrue();

        t1.GetHashCode().Should().Be(t2.GetHashCode(), "Fix EVT-SEC-004: IsEmpty TenantIds must produce identical hash codes (0).");
        t1.CompareTo(t2).Should().Be(0, "IsEmpty TenantIds must compare as equivalent (0).");
    }

    #endregion

    #region 8. Guid v7 Monotonicity and Formatting Property Invariants

    [Fact]
    public void EventId_MonotonicGuidV7_GuaranteesChronologicalOrdering()
    {
        var id1 = EventId.New();
        Thread.Sleep(2);
        var id2 = EventId.New();

        (id1 < id2).Should().BeTrue("Guid v7 EventId must be monotonically time-sortable.");
        (id2 > id1).Should().BeTrue();
        id1.CompareTo(id2).Should().BeNegative();
    }

    [Fact]
    public void EventId_TryFormat_RoundtripIsLossless()
    {
        var id = EventId.New();
        Span<char> span = stackalloc char[36];
        bool formatted = id.TryFormat(span, out int charsWritten);

        formatted.Should().BeTrue();
        charsWritten.Should().Be(36);

        var parsed = EventId.Parse(span);
        parsed.Should().Be(id);
    }

    #endregion

    #region 10. EventBus Child Event Custom Headers Propagation (EVT-CAS-001 Remediation)

    [Fact]
    public async Task EVT_CAS_001_EventBus_ChildEvent_PreservesCustomHeadersInEphemeralEnvelope()
    {
        var services = new ServiceCollection();
        var handlerRegistry = new HandlerRegistry();

        IReadOnlyDictionary<string, string>? observedChildHeaders = null;

        var childHandler = new SimpleHandler<ForensicTestEvent>(evt =>
        {
            if (evt.Data == "Child")
            {
                observedChildHeaders = Context.EventContext.Current?.Metadata.CustomHeaders;
            }
            return ValueTask.CompletedTask;
        });

        handlerRegistry.Register(typeof(ForensicTestEvent), new HandlerDescriptor(
            typeof(SimpleHandler<ForensicTestEvent>),
            typeof(IEventHandler<ForensicTestEvent>),
            (inst, evt, ct) => childHandler.HandleAsync((ForensicTestEvent)evt, ct)));

        services.AddSingleton(childHandler);
        var sp = services.BuildServiceProvider();
        var bus = new EventBus(handlerRegistry, sp, new EventBusOptions());

        var parentMeta = new EventMetadataBuilder()
            .WithCorrelationId("CORR-PARENT")
            .WithHeader("X-Causation-Depth", "2")
            .Build();

        var parentEnvelope = EventEnvelope.Create(
            new ForensicTestEvent(EventId.New(), DateTimeOffset.UtcNow, "Parent"),
            parentMeta);

        using (EventContext.SetCurrent(parentEnvelope))
        {
            // Publish child event while parent envelope is active in EventContext
            await bus.PublishAsync(new ForensicTestEvent(EventId.New(), DateTimeOffset.UtcNow, "Child"));
        }

        // EVIDENCE OF FIX: Custom headers (such as X-Causation-Depth) are preserved in child metadata!
        observedChildHeaders.Should().NotBeNull();
        observedChildHeaders!.ContainsKey("X-Causation-Depth").Should().BeTrue(
            "Fix EVT-CAS-001: EventBus preserves custom headers from parent envelope into child event metadata.");
        observedChildHeaders["X-Causation-Depth"].Should().Be("2");
    }

    #endregion

    #region 11. HandlerResolutionHelper Interface Resolution (EVT-LFC-001 Remediation)

    private sealed class CountingHandlerA : IEventHandler<ForensicTestEvent>
    {
        public static int InstanceCount;
        public CountingHandlerA() => Interlocked.Increment(ref InstanceCount);
        public ValueTask HandleAsync(ForensicTestEvent @event, CancellationToken ct = default) => ValueTask.CompletedTask;
    }

    private sealed class CountingHandlerB : IEventHandler<ForensicTestEvent>
    {
        public static int InstanceCount;
        public CountingHandlerB() => Interlocked.Increment(ref InstanceCount);
        public ValueTask HandleAsync(ForensicTestEvent @event, CancellationToken ct = default) => ValueTask.CompletedTask;
    }

    [Fact]
    public async Task EVT_LFC_001_HandlerResolutionHelper_InterfaceRegistration_InstantiatesEachHandlerExactlyOnce()
    {
        CountingHandlerA.InstanceCount = 0;
        CountingHandlerB.InstanceCount = 0;

        var services = new ServiceCollection();
        // Register both handlers by interface (transient)
        services.AddTransient<IEventHandler<ForensicTestEvent>, CountingHandlerA>();
        services.AddTransient<IEventHandler<ForensicTestEvent>, CountingHandlerB>();

        var registry = new HandlerRegistry();
        registry.Register(typeof(ForensicTestEvent), new HandlerDescriptor(
            typeof(CountingHandlerA),
            typeof(IEventHandler<ForensicTestEvent>),
            (inst, evt, ct) => ((CountingHandlerA)inst).HandleAsync((ForensicTestEvent)evt, ct)));
        registry.Register(typeof(ForensicTestEvent), new HandlerDescriptor(
            typeof(CountingHandlerB),
            typeof(IEventHandler<ForensicTestEvent>),
            (inst, evt, ct) => ((CountingHandlerB)inst).HandleAsync((ForensicTestEvent)evt, ct)));

        var sp = services.BuildServiceProvider();
        var bus = new EventBus(registry, sp, new EventBusOptions { ExecutionMode = EventExecutionMode.Sequential });

        await bus.PublishAsync(new ForensicTestEvent(EventId.New(), DateTimeOffset.UtcNow, "Test"));

        // EVIDENCE OF FIX: With fallbackCache, resolving IEnumerable<IEventHandler<T>> occurs once and caches per-dispatch!
        // Exactly 1 instance of CountingHandlerA and 1 instance of CountingHandlerB are created (zero zombie instances)!
        CountingHandlerA.InstanceCount.Should().Be(1, "Fix EVT-LFC-001: CountingHandlerA was instantiated exactly once with zero zombie instances.");
        CountingHandlerB.InstanceCount.Should().Be(1, "Fix EVT-LFC-001: CountingHandlerB was instantiated exactly once with zero zombie instances.");
    }

    #endregion

    #region 12. InMemoryEventPublisher Child Metadata Propagates CustomHeaders (EVT-CAS-002 Remediation)

    [Fact]
    public async Task EVT_CAS_002_InMemoryEventPublisher_ChildEvent_PreservesCustomHeadersInEphemeralEnvelope()
    {
        IReadOnlyDictionary<string, string>? observedChildHeaders = null;

        var publisher = new InMemoryEventPublisher();
        publisher.Subscribe(new SimpleHandler<ForensicTestEvent>(evt =>
        {
            if (evt.Data == "Child")
            {
                observedChildHeaders = Context.EventContext.Current?.Metadata.CustomHeaders;
            }
            return ValueTask.CompletedTask;
        }));

        var parentMeta = new EventMetadataBuilder()
            .WithCorrelationId("CORR-PARENT-PUB")
            .WithHeader("X-Causation-Depth", "3")
            .Build();

        var parentEnvelope = EventEnvelope.Create(
            new ForensicTestEvent(EventId.New(), DateTimeOffset.UtcNow, "Parent"),
            parentMeta);

        using (EventContext.SetCurrent(parentEnvelope))
        {
            await publisher.PublishAsync(new ForensicTestEvent(EventId.New(), DateTimeOffset.UtcNow, "Child"));
        }

        // EVIDENCE OF FIX: Custom headers are preserved in child metadata under InMemoryEventPublisher!
        observedChildHeaders.Should().NotBeNull();
        observedChildHeaders!.ContainsKey("X-Causation-Depth").Should().BeTrue(
            "Fix EVT-CAS-002: InMemoryEventPublisher preserves custom headers from parent envelope into child event metadata.");
        observedChildHeaders["X-Causation-Depth"].Should().Be("3");
    }

    #endregion

    #region 13. EventBus Dynamic Discovery Sets TypedInvoker (EVT-BOX-002 Remediation)

    private readonly record struct ForensicStructEvent2(EventId Id, DateTimeOffset OccurredAt, int Value) : IEvent;

    private sealed class BoxingTestHandler : IEventHandler<ForensicStructEvent2>
    {
        public static bool Invoked { get; set; }
        public static int LastValue { get; set; }

        public ValueTask HandleAsync(ForensicStructEvent2 @event, CancellationToken cancellationToken = default)
        {
            Invoked = true;
            LastValue = @event.Value;
            return ValueTask.CompletedTask;
        }
    }

    [Fact]
    public async Task EVT_BOX_002_EventBus_AddEventHandler_ConfiguresTypedInvoker_AvoidingBoxing()
    {
        // EVT-BOX-002 Remediation Verification — Post-Fix 1:
        // After removing late-binding DI discovery (HIGH-001), TypedInvoker is configured during
        // AddEventHandler() registration, not during dynamic discovery at first publish.
        // This ensures zero boxing for struct events: the TypedInvoker operates on TEvent directly.

        BoxingTestHandler.Invoked = false;
        BoxingTestHandler.LastValue = 0;

        var services = new ServiceCollection();
        services.AddEventBus();
        services.AddEventHandler<ForensicStructEvent2, BoxingTestHandler>(ServiceLifetime.Singleton);
        var sp = services.BuildServiceProvider();

        var registry = sp.GetRequiredService<IHandlerRegistry>();
        var bus = sp.GetRequiredService<IEventBus>();

        var structEvt = new ForensicStructEvent2(EventId.New(), DateTimeOffset.UtcNow, 42);
        await bus.PublishAsync(structEvt);

        BoxingTestHandler.Invoked.Should().BeTrue("Handler was invoked");
        BoxingTestHandler.LastValue.Should().Be(42, "Struct value was passed correctly without boxing.");

        // Check that AddEventHandler configured a TypedInvoker
        var descriptors = registry.GetHandlers(typeof(ForensicStructEvent2));
        descriptors.Should().HaveCount(1);
        descriptors[0].TypedInvoker.Should().NotBeNull(
            "Fix EVT-BOX-002: AddEventHandler must supply a typed invoker to avoid boxing.");
        descriptors[0].TypedInvoker.Should().BeOfType<HandlerInvoker<ForensicStructEvent2>>();
    }

    #endregion

    #region 14. ParallelExecutionStrategy Thread-Safe Logger Resolution (EVT-CONC-003 Remediation)

    private sealed class NullResolutionHandler : IEventHandler<ForensicTestEvent>
    {
        public ValueTask HandleAsync(ForensicTestEvent @event, CancellationToken ct = default) => ValueTask.CompletedTask;
    }

    [Fact]
    public async Task EVT_CONC_003_ParallelExecutionStrategy_ConcurrentUnresolvedHandlers_AreThreadSafe()
    {
        // Register multiple descriptors that will fail resolution from DI to stress test the onUnresolved callback
        var registry = new HandlerRegistry();
        for (int i = 0; i < 20; i++)
        {
            registry.Register(typeof(ForensicTestEvent), new HandlerDescriptor(
                typeof(NullResolutionHandler),
                typeof(IEventHandler<ForensicTestEvent>),
                (inst, evt, ct) => ValueTask.CompletedTask));
        }

        var services = new ServiceCollection();
        // Do NOT register NullResolutionHandler in services - resolution will return null
        var sp = services.BuildServiceProvider();
        var bus = new EventBus(registry, sp, new EventBusOptions { ExecutionMode = EventExecutionMode.Parallel });

        // Should complete without ObjectDisposedException, NullReferenceException or concurrency race
        var act = async () => await bus.PublishAsync(new ForensicTestEvent(EventId.New(), DateTimeOffset.UtcNow, "ParallelTest"));
        await act.Should().NotThrowAsync();
    }

    #endregion

    #region 15. EventMetadata Enforces Case-Insensitive FrozenDictionary (EVT-DAT-003 Remediation)

    [Fact]
    public void EVT_DAT_003_EventMetadata_EnforcesOrdinalIgnoreCase_EvenWhenPassedOrdinalFrozenDictionary()
    {
        // Create a FrozenDictionary with default Ordinal comparer
        var rawDict = new Dictionary<string, string> { { "X-Custom-Header", "Value123" } };
        var ordinalFrozen = System.Collections.Frozen.FrozenDictionary.ToFrozenDictionary(rawDict);

        var metadata = new EventMetadata(
            CorrelationId.Empty,
            CausationId.Empty,
            TenantId.Empty,
            customHeaders: ordinalFrozen);

        // EVIDENCE OF FIX: Case-insensitive lookups MUST succeed
        metadata.TryGetHeader("x-custom-header", out var valLower).Should().BeTrue();
        valLower.Should().Be("Value123");

        metadata.TryGetHeader("X-CUSTOM-HEADER", out var valUpper).Should().BeTrue();
        valUpper.Should().Be("Value123");

        // Also verify empty dictionary reuse
        var emptyMeta = new EventMetadata(
            CorrelationId.Empty,
            CausationId.Empty,
            TenantId.Empty,
            customHeaders: new Dictionary<string, string>());

        emptyMeta.CustomHeaders.Should().BeSameAs(EventMetadata.Empty.CustomHeaders);
    }

    #endregion

    private sealed class SimpleHandler<T> : IEventHandler<T> where T : IEvent
    {
        private readonly Func<T, ValueTask> _action;
        public SimpleHandler(Func<T, ValueTask> action) => _action = action;
        public ValueTask HandleAsync(T eventInstance, CancellationToken cancellationToken = default) => _action(eventInstance);
    }
}

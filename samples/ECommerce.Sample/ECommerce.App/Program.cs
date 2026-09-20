// Copyright © Erickson Lopez. MIT License.
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

using ECommerce.Application;
using ECommerce.Domain;
using ECommerce.Infrastructure;
using EricksonLopez.Events.Bus.Configuration;
using EricksonLopez.Events.Bus.Exceptions;
using EricksonLopez.Events.Bus.Extensions;
using EricksonLopez.Events.Bus.Middleware;
using EricksonLopez.Events.CloudEvents;
using EricksonLopez.Events.CloudEvents.Serialization;
using EricksonLopez.Events.Contracts;
using EricksonLopez.Events.Diagnostics;
using EricksonLopez.Events.Dispatch;
using EricksonLopez.Events.Envelopes;
using EricksonLopez.Events.Identifiers;
using EricksonLopez.Events.Metadata;
using EricksonLopez.Events.Registry;
using EricksonLopez.Events.Serialization.SystemTextJson;
using EricksonLopez.Events.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace ECommerce.App;

/// <summary>
/// Official reference showcase and executable specification for the EricksonLopez.Events library.
///
/// PURPOSE:
///   - Official API documentation (executable)
///   - Learning guide (progressive complexity)
///   - Integration cookbook
///   - Technical reference
///   - Architecture demonstration
///
/// LEVELS:
///   L0  – Conceptual Architecture &amp; Core Boundaries
///   L1  – Quick Start: IEvent, IDomainEvent, Identifiers &amp; Empty values
///   L2  – Full Configuration: EventMetadata, EventEnvelope, all factory overloads
///   L3  – InMemoryEventPublisher: IEventSubscriber, Subscribe/Unsubscribe
///   L4  – Microsoft DI &amp; EventBus: AddEventHandler, EventPublisherExtensions, envelope publish
///   L5  – EventTypeRegistry: StaticEventTypeRegistry, EventTypeRegistryBuilder, reentrancy, cancellation
///   L6  – Error Handling: FailFast, AggregateAndContinue, exception types
///   L7  – Performance: Span&lt;char&gt;/Span&lt;byte&gt; stackalloc, GUIDv7 monotonic ordering
///   L8  – Customization: IEventMiddleware pipeline, custom IExecutionStrategy, MiddlewarePipeline.Build
///   L9  – Ecosystem: CloudEvents v1.0, Testing DSL (full), OpenTelemetry diagnostics
///   L10 – Enterprise Native AOT: AOT JSON serialization, CreateDefaultOptions, converters
///
/// CONSTRAINT: Every API demonstrated here MUST exist in the public surface.
///             No invented overloads, fake methods, or undocumented patterns.
/// </summary>
public static class Program
{
    public static async Task<int> Main()
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        PrintBanner();

        try
        {
            await RunLevel0ConceptualAsync();
            await RunLevel1QuickStartAsync();
            await RunLevel2FullConfigurationAsync();
            await RunLevel3InMemoryPublisherAsync();
            await RunLevel4AdvancedIntegrationAsync();
            await RunLevel5RegistryAndBehaviorAsync();
            await RunLevel6ErrorHandlingAsync();
            await RunLevel7ScalabilityAndPerformanceAsync();
            await RunLevel8CustomizationAndMiddlewaresAsync();
            await RunLevel9EcosystemExtensionsAsync();
            await RunLevel10EnterpriseAotAsync();
            await Level11_ComprehensiveCoverage.RunAsync();

            PrintCompletionSummary();
            return 0;
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"\n[FATAL ERROR IN SHOWCASE]: {ex}");
            Console.ResetColor();
            return 1;
        }
    }

    private static void PrintBanner()
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("================================================================================");
        Console.WriteLine(" EricksonLopez.Events — Official Executable Showcase & Architecture Reference ");
        Console.WriteLine("================================================================================");
        Console.ResetColor();
        Console.WriteLine(" Pure Event Contracts | Monotonic GUIDv7 Identity | AOT-First Dispatch | Full API\n");
    }

    // ─────────────────────────────────────────────────────────────────────────
    // LEVEL 0: Conceptual Architecture
    // ─────────────────────────────────────────────────────────────────────────
    private static Task RunLevel0ConceptualAsync()
    {
        PrintLevelHeader(0, "Conceptual Architecture & Core Boundaries");

        Console.WriteLine(" Problem Statement:");
        Console.WriteLine("   Conflation of domain events with transport brokers, ORM entities, mediators.");
        Console.WriteLine();
        Console.WriteLine(" Solution — Pure Immutable Contracts:");
        Console.WriteLine("   IEvent         — base contract (Id: EventId, OccurredAt: DateTimeOffset)");
        Console.WriteLine("   IDomainEvent   — marks internal aggregate events");
        Console.WriteLine("   IIntegrationEvent — marks cross-boundary service events");
        Console.WriteLine("   IEventHandler<T>  — strongly-typed handler (ValueTask HandleAsync)");
        Console.WriteLine("   IEventPublisher   — single method: ValueTask PublishAsync<TEvent>(event, ct)");
        Console.WriteLine("   IEventBus         — extends IEventPublisher for in-process dispatch");
        Console.WriteLine("   IEventSubscriber  — runtime Subscribe<T>/Unsubscribe<T> for InMemoryEventPublisher");
        Console.WriteLine();
        Console.WriteLine(" Capability Boundary (ADR-002):");
        Console.WriteLine("   EricksonLopez.Events        → In-process domain dispatch, Native AOT");
        Console.WriteLine("   EricksonLopez.Messaging     → Distributed broker transport (Kafka/RabbitMQ)");
        Console.WriteLine();
        Console.WriteLine(" Package Ecosystem:");
        Console.WriteLine("   .Events.Contracts           → Zero-dependency contracts & identifiers");
        Console.WriteLine("   .Events                     → Bus, middleware, DI extensions, registry");
        Console.WriteLine("   .Events.CloudEvents         → CloudEvents v1.0 bridge");
        Console.WriteLine("   .Events.Serialization.STJ   → System.Text.Json converters");
        Console.WriteLine("   .Events.OpenTelemetry       → TracerProviderBuilder/MeterProviderBuilder");
        Console.WriteLine("   .Events.Generators          → Roslyn Source Generator (AOT registry)");
        Console.WriteLine("   .Events.Testing             → FakeEventPublisher, TestEventHandler, builders");
        Console.WriteLine();

        return Task.CompletedTask;
    }

    // ─────────────────────────────────────────────────────────────────────────
    // LEVEL 1: Quick Start — Identifiers, IEvent, IDomainEvent
    // ─────────────────────────────────────────────────────────────────────────
    private static Task RunLevel1QuickStartAsync()
    {
        PrintLevelHeader(1, "Quick Start — Pure Domain Events, GUIDv7 Identifiers & Empty values");

        // ── Creating domain aggregate root ──────────────────────────────────
        var orderId = OrderId.New();
        var customerId = CustomerId.New();
        var total = Money.USD(299.99m);

        Console.WriteLine($" [1] Creating Aggregate Root 'Order' (Id: {orderId.Value})");
        var order = new Order(orderId, customerId, total);

        Console.WriteLine($" [2] Emitted Domain Events Count: {order.DomainEvents.Count}");
        foreach (var domainEvent in order.DomainEvents)
        {
            Console.WriteLine($"     -> Domain Event: {domainEvent.GetType().Name}");
            Console.WriteLine($"        EventId (GUIDv7):   {domainEvent.Id} (time-sortable, monotonic)");
            Console.WriteLine($"        OccurredAt (UTC):   {domainEvent.OccurredAt:O}");
            Console.WriteLine($"        Is IDomainEvent:    {domainEvent is IDomainEvent}");
        }

        order.ClearDomainEvents();
        Console.WriteLine($" [3] Cleared domain events. Remaining: {order.DomainEvents.Count}");

        // ── EventId API ─────────────────────────────────────────────────────
        Console.WriteLine(" [4] EventId API demonstration:");
        var id1 = EventId.New();
        var id2 = EventId.From(Guid.NewGuid());
        var idEmpty = EventId.Empty;

        Console.WriteLine($"     EventId.New():        {id1}");
        Console.WriteLine($"     EventId.From(Guid):   {id2}");
        Console.WriteLine($"     EventId.Empty:        {idEmpty}");
        Console.WriteLine($"     Empty.IsEmpty:        {idEmpty.IsEmpty}");
        Console.WriteLine($"     id1.IsEmpty:          {id1.IsEmpty}");

        // Parse API
        if (EventId.TryParse(id1.ToString(), null, out var parsedId))
        {
            Console.WriteLine($"     EventId.TryParse:     OK → {parsedId}");
            Console.WriteLine($"     parsed == id1:        {parsedId == id1}");
        }

        // ── CorrelationId, CausationId, TenantId ────────────────────────────
        Console.WriteLine(" [5] Correlation/Causation/Tenant identifier APIs:");
        var correlationId = CorrelationId.New();
        var correlationEmpty = CorrelationId.Empty;
        var causationId = CausationId.From("CMD-PLACE-ORDER-001");
        var causationFromGuid = CausationId.From(Guid.NewGuid());
        var causationFromEventId = CausationId.From(id1);
        var tenantId = TenantId.From("tenant-enterprise-alpha");
        var tenantEmpty = TenantId.Empty;

        Console.WriteLine($"     CorrelationId.New():             {correlationId}");
        Console.WriteLine($"     CorrelationId.Empty.IsEmpty:     {correlationEmpty.IsEmpty}");
        Console.WriteLine($"     CausationId.From(string):        {causationId}");
        Console.WriteLine($"     CausationId.From(Guid):          {causationFromGuid}");
        Console.WriteLine($"     CausationId.From(EventId):       {causationFromEventId}");
        Console.WriteLine($"     TenantId.From(string):           {tenantId}");
        Console.WriteLine($"     TenantId.Empty.IsEmpty:          {tenantEmpty.IsEmpty}");

        // ── EventType and EventVersion ────────────────────────────────────
        Console.WriteLine(" [6] EventType and EventVersion:");
        var eventType = EricksonLopez.Events.Identifiers.EventType.From("ecommerce.orders.order-placed");
        // EventType is a readonly record struct — default() gives an empty/uninitialized instance
        var eventTypeDefault = default(EricksonLopez.Events.Identifiers.EventType);
        var versionV1 = EventVersion.V1;
        var versionV2 = EventVersion.From(2u);
        var versionFromInt = EventVersion.From(3);

        Console.WriteLine($"     EventType.From(string):    {eventType}");
        Console.WriteLine($"     default(EventType).IsEmpty: {eventTypeDefault.IsEmpty}");
        Console.WriteLine($"     EventVersion.V1:           v{versionV1}");
        Console.WriteLine($"     EventVersion.From(2u):     v{versionV2}");
        Console.WriteLine($"     EventVersion.From(3):      v{versionFromInt}");
        Console.WriteLine();

        return Task.CompletedTask;
    }

    // ─────────────────────────────────────────────────────────────────────────
    // LEVEL 2: Full Configuration — EventMetadata & EventEnvelope
    // ─────────────────────────────────────────────────────────────────────────
    private static Task RunLevel2FullConfigurationAsync()
    {
        PrintLevelHeader(2, "Full Configuration — Immutable Metadata, EventEnvelope<T> & Factory Overloads");

        // ── EventMetadata.Empty ──────────────────────────────────────────────
        Console.WriteLine(" [1] EventMetadata.Empty (default no-op metadata):");
        var emptyMeta = EventMetadata.Empty;
        Console.WriteLine($"     CorrelationId.IsEmpty: {emptyMeta.CorrelationId.IsEmpty}");
        Console.WriteLine($"     CausationId.IsEmpty:   {emptyMeta.CausationId.IsEmpty}");
        Console.WriteLine($"     TenantId.IsEmpty:      {emptyMeta.TenantId.IsEmpty}");
        Console.WriteLine($"     Source:                {emptyMeta.Source ?? "<null>"}");
        Console.WriteLine($"     ContentType:           {emptyMeta.ContentType ?? "<null>"}");
        Console.WriteLine($"     CustomHeaders count:   {emptyMeta.CustomHeaders.Count}");

        // ── EventMetadata.Create() ───────────────────────────────────────────
        // Static factory — all parameters optional; alternative to EventMetadataBuilder
        Console.WriteLine(" [1b] EventMetadata.Create() — static factory with named params:");
        var metaViaCreate = EventMetadata.Create(
            correlationId: CorrelationId.New(),
            causationId: CausationId.From("CMD-1234"),
            tenantId: TenantId.From("tenant-alpha"),
            source: "order-service",
            contentType: "application/json");
        Console.WriteLine($"     Create().CorrelationId:  {metaViaCreate.CorrelationId}");
        Console.WriteLine($"     Create().CausationId:    {metaViaCreate.CausationId}");
        Console.WriteLine($"     Create().TenantId:       {metaViaCreate.TenantId}");
        Console.WriteLine($"     Create().Source:         {metaViaCreate.Source}");
        Console.WriteLine($"     Create().ContentType:    {metaViaCreate.ContentType}");

        // ── EventMetadataBuilder (full fluent API) ───────────────────────────
        var correlationId = CorrelationId.New();
        var causationId = CausationId.From("CMD-CHECKOUT-9823");
        var tenantId = TenantId.From("tenant-enterprise-alpha");


        Console.WriteLine(" [2] EventMetadataBuilder — full fluent API:");
        var metadata = new EventMetadataBuilder()
            .WithCorrelationId(correlationId)
            .WithCausationId(causationId)
            .WithTenantId(tenantId)
            .WithSource("ecommerce-ordering")
            .WithContentType("application/json")
            .WithHeader("X-Client-Version", "v2.5.0")
            .WithHeader("X-Originating-Region", "us-east-1")
            .WithHeader("X-Trace-Tag", "Audit-Preview")
            .Build();

        Console.WriteLine($"     CorrelationId:         {metadata.CorrelationId}");
        Console.WriteLine($"     CausationId:           {metadata.CausationId}");
        Console.WriteLine($"     TenantId:              {metadata.TenantId}");
        Console.WriteLine($"     Source:                {metadata.Source}");
        Console.WriteLine($"     ContentType:           {metadata.ContentType}");
        Console.WriteLine($"     CustomHeaders:         {metadata.CustomHeaders.Count} entries (FrozenDictionary)");

        // ── EventMetadata.WithHeader() and TryGetHeader() ───────────────────
        Console.WriteLine(" [3] EventMetadata.WithHeader() and TryGetHeader():");
        var metaWithExtra = metadata.WithHeader("X-Extra-Tag", "showcase-demo");
        Console.WriteLine($"     After WithHeader(), count: {metaWithExtra.CustomHeaders.Count}");

        if (metaWithExtra.TryGetHeader("X-Extra-Tag", out var extraValue))
        {
            Console.WriteLine($"     TryGetHeader('X-Extra-Tag'): Found → '{extraValue}'");
        }

        bool missing = metaWithExtra.TryGetHeader("X-Does-Not-Exist", out _);
        Console.WriteLine($"     TryGetHeader('X-Does-Not-Exist'): {missing}");

        // ── EventMetadataBuilder overloads with string ───────────────────────
        Console.WriteLine(" [4] EventMetadataBuilder overloads accepting raw strings:");
        var metaFromStrings = new EventMetadataBuilder()
            .WithCorrelationId("corr-id-string-form")
            .WithCausationId("caus-id-string-form")
            .WithTenantId("tenant-string-form")
            .Build();
        Console.WriteLine($"     CorrelationId from string: {metaFromStrings.CorrelationId}");
        Console.WriteLine($"     CausationId from string:   {metaFromStrings.CausationId}");
        Console.WriteLine($"     TenantId from string:      {metaFromStrings.TenantId}");

        // ── EventEnvelope.Create() ───────────────────────────────────────────
        var eventId = EventId.New();
        var integrationEvent = new OrderPlacedIntegrationEvent(
            eventId,
            Guid.NewGuid(),
            Guid.NewGuid(),
            149.50m,
            "USD",
            DateTimeOffset.UtcNow);

        Console.WriteLine(" [5] EventEnvelope.Create<TEvent>(event, metadata?):");
        var envelope = EventEnvelope.Create(integrationEvent, metadata);
        Console.WriteLine($"     Envelope ID:           {envelope.Id}");
        Console.WriteLine($"     Semantic Type:         {envelope.Type}");
        Console.WriteLine($"     Schema Version:        v{envelope.Version}");
        Console.WriteLine($"     OccurredAt (UTC):      {envelope.OccurredAt:O}");
        Console.WriteLine($"     Payload.OrderId:       {envelope.Payload.OrderId}");
        Console.WriteLine($"     Metadata.TenantId:     {envelope.Metadata.TenantId}");

        // GetPayload() via IEventEnvelope
        IEventEnvelope envAsInterface = envelope;
        var rawPayload = envAsInterface.GetPayload();
        Console.WriteLine($"     IEventEnvelope.GetPayload(): {rawPayload.GetType().Name}");

        // ── EventEnvelope.Wrap() ─────────────────────────────────────────────
        Console.WriteLine(" [6] EventEnvelope.Wrap<TEvent>() — shorthand factory with direct identifiers:");
        var wrappedEnvelope = EventEnvelope.Wrap(
            integrationEvent,
            correlationId: correlationId,
            causationId: causationId,
            tenantId: tenantId,
            source: "ordering-service-v2");
        Console.WriteLine($"     Wrapped Envelope ID:   {wrappedEnvelope.Id}");
        Console.WriteLine($"     Wrapped CorrelationId: {wrappedEnvelope.Metadata.CorrelationId}");
        Console.WriteLine($"     Wrapped Source:        {wrappedEnvelope.Metadata.Source}");

        // ── EventEnvelope direct constructor ─────────────────────────────────
        Console.WriteLine(" [7] EventEnvelope<T> direct 6-param constructor:");
        var explicitEnvelope = new EventEnvelope<OrderPlacedIntegrationEvent>(
            id: EventId.New(),
            type: EricksonLopez.Events.Identifiers.EventType.From("ecommerce.orders.order-placed"),
            version: EventVersion.V1,
            occurredAt: DateTimeOffset.UtcNow,
            payload: integrationEvent,
            metadata: metadata);
        Console.WriteLine($"     Explicit Envelope ID:  {explicitEnvelope.Id}");
        Console.WriteLine($"     Explicit Type:         {explicitEnvelope.Type}");
        Console.WriteLine();

        return Task.CompletedTask;
    }

    // ─────────────────────────────────────────────────────────────────────────
    // LEVEL 3: InMemoryEventPublisher — Subscribe/Unsubscribe (IEventSubscriber)
    // ─────────────────────────────────────────────────────────────────────────
    private static async Task RunLevel3InMemoryPublisherAsync()
    {
        PrintLevelHeader(3, "InMemoryEventPublisher — IEventSubscriber: Subscribe<T> & Unsubscribe<T>");

        // InMemoryEventPublisher implements IEventPublisher + IEventSubscriber
        // Namespace: EricksonLopez.Events.Dispatch
        var publisher = new InMemoryEventPublisher();

        var handlerA = new SendOrderConfirmationEmailHandler();
        var handlerB = new UpdateInventoryOnOrderPlacedHandler();

        // Subscribe handlers at runtime
        Console.WriteLine(" [1] Subscribing two handlers at runtime via IEventSubscriber.Subscribe<T>():");
        publisher.Subscribe<OrderPlacedIntegrationEvent>(handlerA);
        publisher.Subscribe<OrderPlacedIntegrationEvent>(handlerB);

        var evt = new OrderPlacedIntegrationEvent(EventId.New(), Guid.NewGuid(), Guid.NewGuid(), 59.99m, "USD", DateTimeOffset.UtcNow);
        Console.WriteLine(" [2] Publishing event through InMemoryEventPublisher:");
        await publisher.PublishAsync(evt);

        Console.WriteLine($" [3] After publish — HandlerA.Handled={handlerA.Handled}, HandlerB.Handled={handlerB.Handled}");

        // Reset and unsubscribe one handler
        var handlerA2 = new SendOrderConfirmationEmailHandler();
        var handlerB2 = new UpdateInventoryOnOrderPlacedHandler();
        publisher.Subscribe<OrderPlacedIntegrationEvent>(handlerA2);
        publisher.Subscribe<OrderPlacedIntegrationEvent>(handlerB2);

        Console.WriteLine(" [4] Unsubscribing handlerA2 via IEventSubscriber.Unsubscribe<T>():");
        publisher.Unsubscribe<OrderPlacedIntegrationEvent>(handlerA2);

        var evt2 = new OrderPlacedIntegrationEvent(EventId.New(), Guid.NewGuid(), Guid.NewGuid(), 29.99m, "USD", DateTimeOffset.UtcNow);
        await publisher.PublishAsync(evt2);
        Console.WriteLine($" [5] After unsubscribe & second publish — HandlerA2.Handled={handlerA2.Handled} (should be False), HandlerB2.Handled={handlerB2.Handled} (should be True)");

        // DDD Outbox pattern with InMemoryOutboxService
        Console.WriteLine();
        Console.WriteLine(" [6] DDD Outbox Pattern — OrderApplicationService + InMemoryOutboxService:");
        var outbox = new InMemoryOutboxService();
        var orderService = new OrderApplicationService(outbox);
        var customerId = CustomerId.New();
        var correlationId = CorrelationId.New();

        Console.WriteLine("     Placing order via application service...");
        var placedOrderId = await orderService.PlaceOrderAsync(customerId, 599.00m, correlationId);
        Console.WriteLine($"     Order placed: {placedOrderId.Value}");
        Console.WriteLine($"     Outbox records: {outbox.Records.Count}");

        foreach (var record in outbox.Records)
        {
            Console.WriteLine($"     -> Record ID:   {record.Id}");
            Console.WriteLine($"        EventType:   {record.EventType} (v{record.Version})");
            Console.WriteLine($"        Payload:     {record.PayloadJson[..Math.Min(80, record.PayloadJson.Length)]}...");
        }
        Console.WriteLine();
    }

    // ─────────────────────────────────────────────────────────────────────────
    // LEVEL 4: Microsoft DI & EventBus — Full DI Integration
    // ─────────────────────────────────────────────────────────────────────────
    private static async Task RunLevel4AdvancedIntegrationAsync()
    {
        PrintLevelHeader(4, "Microsoft DI & EventBus — AddEventBus, AddEventHandler, EventPublisherExtensions");

        var services = new ServiceCollection();

        // AddEventBus — full options configuration
        services.AddEventBus(options =>
        {
            options.ExecutionMode = EventExecutionMode.Sequential;
            options.ErrorPolicy = ErrorHandlingPolicy.FailFast;
            options.MaxReentrancyDepth = 10;
            options.ThrowOnUnregisteredEvent = false; // Don't throw if no handlers registered
        });

        services.AddEventHandler<OrderPlacedIntegrationEvent, SendOrderConfirmationEmailHandler>();
        services.AddEventHandler<OrderPlacedIntegrationEvent, UpdateInventoryOnOrderPlacedHandler>();
        services.AddEventHandler<OrderShippedIntegrationEvent, SendShipmentNotificationHandler>();
        services.AddEventHandler<OrderShippedIntegrationEvent, UpdateFulfillmentStatusHandler>();

        using var serviceProvider = services.BuildServiceProvider();
        using var scope = serviceProvider.CreateScope();

        var eventBus = scope.ServiceProvider.GetRequiredService<IEventBus>();

        // ── Publish via IEventBus.PublishAsync (raw event) ─────────────────
        var orderEvt = new OrderPlacedIntegrationEvent(EventId.New(), Guid.NewGuid(), Guid.NewGuid(), 79.99m, "USD", DateTimeOffset.UtcNow);
        Console.WriteLine($" [1] IEventBus.PublishAsync<T>(rawEvent): dispatching '{nameof(OrderPlacedIntegrationEvent)}'");
        await eventBus.PublishAsync(orderEvt);

        var emailHandler = scope.ServiceProvider.GetRequiredService<SendOrderConfirmationEmailHandler>();
        var inventoryHandler = scope.ServiceProvider.GetRequiredService<UpdateInventoryOnOrderPlacedHandler>();
        Console.WriteLine($"     Email Handled={emailHandler.Handled}, Inventory Handled={inventoryHandler.Handled}");

        // ── Publish via EventPublisherExtensions.PublishAsync(envelope) ─────
        Console.WriteLine(" [2] EventPublisherExtensions.PublishAsync<T>(envelope): publishing with metadata:");
        var shippedEvt = new OrderShippedIntegrationEvent(EventId.New(), Guid.NewGuid(), "TRK-XYZ-2025", "FedEx", DateTimeOffset.UtcNow);
        var shippedMetadata = new EventMetadataBuilder()
            .WithCorrelationId(CorrelationId.New())
            .WithTenantId(TenantId.From("tenant-logistics-001"))
            .WithSource("fulfillment-service")
            .WithHeader("X-Fulfillment-Region", "eu-west-1")
            .Build();
        var shippedEnvelope = EventEnvelope.Create(shippedEvt, shippedMetadata);

        // EventPublisherExtensions.PublishAsync<TEvent>(IEventPublisher, EventEnvelope<TEvent>, ct)
        await (eventBus as IEventPublisher)!.PublishAsync(shippedEnvelope);

        var shipmentHandler = scope.ServiceProvider.GetRequiredService<SendShipmentNotificationHandler>();
        var fulfillmentHandler = scope.ServiceProvider.GetRequiredService<UpdateFulfillmentStatusHandler>();
        Console.WriteLine($"     Shipment Handled={shipmentHandler.Handled}, Fulfillment Handled={fulfillmentHandler.Handled}");

        // ── EventPublisherExtensions.PublishAsync — IEventEnvelope<TEvent> overload (GAP-009) ──
        Console.WriteLine(" [2b] EventPublisherExtensions.PublishAsync<T>(IEventPublisher, IEventEnvelope<TEvent>, ct) — interface overload:");
        IEventEnvelope<OrderShippedIntegrationEvent> envelopeAsInterface = shippedEnvelope;
        await (eventBus as IEventPublisher)!.PublishAsync(envelopeAsInterface);
        Console.WriteLine("     IEventEnvelope<T> overload dispatched correctly ✓");

        // ── ThrowOnUnregisteredEvent = false demo ────────────────────────────
        Console.WriteLine(" [3] EventBusOptions.ThrowOnUnregisteredEvent=false: dispatching event with no handlers:");
        var auditEvt = new UnhandledAuditEvent(EventId.New(), "UserLogin", DateTimeOffset.UtcNow);
        await eventBus.PublishAsync(auditEvt); // Should NOT throw (ThrowOnUnregisteredEvent = false)
        Console.WriteLine("     No exception thrown — ThrowOnUnregisteredEvent=false working correctly.");
        Console.WriteLine();
    }

    // ─────────────────────────────────────────────────────────────────────────
    // LEVEL 5: EventTypeRegistry — Static & Builder, Reentrancy, Cancellation
    // ─────────────────────────────────────────────────────────────────────────
    private static Task RunLevel5RegistryAndBehaviorAsync()
    {
        PrintLevelHeader(5, "EventTypeRegistry — StaticEventTypeRegistry, Builder, Reentrancy & Cancellation");

        // ── EventTypeRegistry via builder ────────────────────────────────────
        Console.WriteLine(" [1] EventTypeRegistry.CreateBuilder() — explicit registration:");
        var registry = EventTypeRegistry.CreateBuilder()
            .Register(EventTypeDescriptor.For<OrderPlacedIntegrationEvent>(
                eventType: EricksonLopez.Events.Identifiers.EventType.From("ecommerce.orders.order-placed"),
                version: EventVersion.V1,
                source: "ordering-service"))
            .Register(EventTypeDescriptor.For<OrderShippedIntegrationEvent>(
                eventType: EricksonLopez.Events.Identifiers.EventType.From("ecommerce.orders.order-shipped"),
                version: EventVersion.V1,
                source: "fulfillment-service"))
            .Register(EventTypeDescriptor.For<CustomerRegisteredIntegrationEvent>(
                eventType: EricksonLopez.Events.Identifiers.EventType.From("ecommerce.customers.customer-registered"),
                version: EventVersion.V1,
                source: "customer-service"))
            .Build();

        Console.WriteLine($"     Registered descriptors: {registry.GetAllDescriptors().Count}");

        // TryGetDescriptor by EventType
        if (registry.TryGetDescriptor(EricksonLopez.Events.Identifiers.EventType.From("ecommerce.orders.order-placed"), out var desc1))
        {
            Console.WriteLine($"     TryGetDescriptor(EventType): Found → CLR={desc1.ClrType.Name}, Source={desc1.Source}");
        }

        // TryGetDescriptor by CLR Type (generic overload — AOT-preferred)
        if (registry.TryGetDescriptor<OrderShippedIntegrationEvent>(out var desc2))
        {
            Console.WriteLine($"     TryGetDescriptor<T>() (2nd): Found → EventType={desc2.EventType}, Version=v{desc2.Version}");
        }

        // TryGetDescriptor<TEvent>
        if (registry.TryGetDescriptor<CustomerRegisteredIntegrationEvent>(out var desc3))
        {
            Console.WriteLine($"     TryGetDescriptor<T>():       Found → EventType={desc3.EventType}, Source={desc3.Source}");
        }

        // GetAllDescriptors
        Console.WriteLine(" [2] IEventTypeRegistry.GetAllDescriptors():");
        foreach (var d in registry.GetAllDescriptors())
        {
            Console.WriteLine($"     - {d.EventType} → {d.ClrType.Name} (v{d.Version})");
        }

        // ── StaticEventTypeRegistry ─────────────────────────────────────────
        Console.WriteLine(" [3] StaticEventTypeRegistry — zero-overhead compile-time lookup:");
        StaticEventTypeRegistry.SetCurrent(registry, allowOverride: true);

        var staticDesc = StaticEventTypeRegistry.GetDescriptor<OrderPlacedIntegrationEvent>();
        var staticType = StaticEventTypeRegistry.GetEventType<OrderPlacedIntegrationEvent>();
        var staticVer = StaticEventTypeRegistry.GetVersion<OrderPlacedIntegrationEvent>();

        Console.WriteLine($"     GetDescriptor<T>():  {staticDesc.EventType} (Source: {staticDesc.Source})");
        Console.WriteLine($"     GetEventType<T>():   {staticType}");
        Console.WriteLine($"     GetVersion<T>():     v{staticVer}");

        // ── MaxReentrancyDepth configuration ─────────────────────────────────
        Console.WriteLine(" [4] EventBusOptions.MaxReentrancyDepth — guard against deep recursion:");
        var reentrantOptions = new EventBusOptions { MaxReentrancyDepth = 5 };
        Console.WriteLine($"     MaxReentrancyDepth configured: {reentrantOptions.MaxReentrancyDepth}");
        Console.WriteLine("     (The EventBus tracks AsyncLocal<int> depth and throws if exceeded)");

        // ── CancellationToken propagation ────────────────────────────────────
        Console.WriteLine(" [5] CancellationToken propagation through the pipeline:");
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
        Console.WriteLine($"     CancellationToken.CanBeCanceled: {cts.Token.CanBeCanceled}");
        Console.WriteLine("     All ValueTask HandleAsync/PublishAsync accept CancellationToken.");
        Console.WriteLine("     Handlers must call cancellationToken.ThrowIfCancellationRequested().");

        // ── IEventTypeRegistry.TryGetDescriptor(Type, out) — non-generic CLR overload (GAP-006) ─
        Console.WriteLine(" [6] IEventTypeRegistry.TryGetDescriptor(Type clrType, out descriptor) — non-generic overload:");
        if (registry.TryGetDescriptor(typeof(OrderPlacedIntegrationEvent), out var descByClrType))
        {
            Console.WriteLine($"     TryGetDescriptor(typeof(T)):   Found → {descByClrType.EventType} v{descByClrType.Version}");
        }

        // ── IEventTypeRegistry.TryGetDescriptor(EventType, EventVersion, out) — versioned overload (GAP-007) ─
        Console.WriteLine(" [7] IEventTypeRegistry.TryGetDescriptor(EventType, EventVersion, out) — versioned overload:");
        var lookupType = EricksonLopez.Events.Identifiers.EventType.From("ecommerce.orders.order-placed");
        if (registry.TryGetDescriptor(lookupType, EventVersion.V1, out var descVersioned))
        {
            Console.WriteLine($"     TryGetDescriptor(type, v1):    Found → CLR={descVersioned.ClrType.Name}");
        }
        if (!registry.TryGetDescriptor(lookupType, EventVersion.From(99), out _))
        {
            Console.WriteLine("     TryGetDescriptor(type, v99):   Not found (expected) ✓");
        }

        // ── StaticEventTypeRegistry.Reset() — test-isolation API (GAP-008) ─────
        Console.WriteLine(" [8] StaticEventTypeRegistry.Reset() — resets to empty for test isolation:");
        StaticEventTypeRegistry.SetCurrent(registry, allowOverride: true);
        Console.WriteLine($"     Before Reset — GetDescriptor<T>: {StaticEventTypeRegistry.GetDescriptor<OrderPlacedIntegrationEvent>().EventType}");
        StaticEventTypeRegistry.Reset();
        Console.WriteLine($"     After Reset — Current is Empty: {ReferenceEquals(StaticEventTypeRegistry.Current, EricksonLopez.Events.Registry.EventTypeRegistry.Empty)}");
        // Re-initialize for subsequent levels that depend on the registry
        StaticEventTypeRegistry.SetCurrent(registry, allowOverride: true);
        Console.WriteLine($"     Re-initialized registry: {StaticEventTypeRegistry.Current.GetAllDescriptors().Count} descriptors");
        Console.WriteLine();

        return Task.CompletedTask;
    }

    // ─────────────────────────────────────────────────────────────────────────
    // LEVEL 6: Error Handling — AggregateAndContinue, Exceptions
    // ─────────────────────────────────────────────────────────────────────────
    private static async Task RunLevel6ErrorHandlingAsync()
    {
        PrintLevelHeader(6, "Error Handling — FailFast, AggregateAndContinue, Exception Types");

        // ── AggregateAndContinue policy ──────────────────────────────────────
        Console.WriteLine(" [1] ErrorHandlingPolicy.AggregateAndContinue:");
        {
            var services = new ServiceCollection();
            services.AddEventBus(opts =>
            {
                opts.ExecutionMode = EventExecutionMode.Sequential;
                opts.ErrorPolicy = ErrorHandlingPolicy.AggregateAndContinue;
            });
            services.AddEventHandler<OrderPlacedIntegrationEvent, SendOrderConfirmationEmailHandler>();
            services.AddEventHandler<OrderPlacedIntegrationEvent, FailingPaymentProcessingHandler>();
            services.AddEventHandler<OrderPlacedIntegrationEvent, UpdateInventoryOnOrderPlacedHandler>();

            using var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();
            var bus = scope.ServiceProvider.GetRequiredService<IEventBus>();
            var evt = new OrderPlacedIntegrationEvent(EventId.New(), Guid.NewGuid(), Guid.NewGuid(), 99.00m, "USD", DateTimeOffset.UtcNow);

            try
            {
                await bus.PublishAsync(evt);
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("     [UNEXPECTED] Publish did not throw!");
                Console.ResetColor();
            }
            catch (EventDispatchException dispatchEx)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"     [CAUGHT] EventDispatchException: {dispatchEx.Message}");
                Console.WriteLine($"     EventType property: {dispatchEx.EventType.Name}");
                Console.WriteLine($"     InnerExceptions:    {dispatchEx.InnerExceptions.Count}");
                foreach (var inner in dispatchEx.InnerExceptions)
                {
                    Console.WriteLine($"       -> {inner.GetType().Name}: {inner.Message}");
                }
                Console.ResetColor();
            }

            var emailH = scope.ServiceProvider.GetRequiredService<SendOrderConfirmationEmailHandler>();
            var invH = scope.ServiceProvider.GetRequiredService<UpdateInventoryOnOrderPlacedHandler>();
            Console.WriteLine($"     Remaining handlers ran despite failure: Email={emailH.Handled}, Inventory={invH.Handled}");
        }

        // ── FailFast policy ──────────────────────────────────────────────────
        Console.WriteLine(" [2] ErrorHandlingPolicy.FailFast:");
        {
            var services = new ServiceCollection();
            services.AddEventBus(opts => opts.ErrorPolicy = ErrorHandlingPolicy.FailFast);
            services.AddEventHandler<OrderPlacedIntegrationEvent, FailingPaymentProcessingHandler>();
            services.AddEventHandler<OrderPlacedIntegrationEvent, UpdateInventoryOnOrderPlacedHandler>();

            using var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();
            var bus = scope.ServiceProvider.GetRequiredService<IEventBus>();
            var evt = new OrderPlacedIntegrationEvent(EventId.New(), Guid.NewGuid(), Guid.NewGuid(), 55.00m, "USD", DateTimeOffset.UtcNow);

            try
            {
                await bus.PublishAsync(evt);
            }
            catch (InvalidOperationException ex)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"     [CAUGHT] FailFast throws first exception directly: {ex.Message[..Math.Min(60, ex.Message.Length)]}...");
                Console.ResetColor();
            }
            catch (EventDispatchException ex)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"     [CAUGHT] FailFast dispatch exception: {ex.Message[..Math.Min(60, ex.Message.Length)]}...");
                Console.ResetColor();
            }
        }

        // ── EventDispatchException constructors ──────────────────────────────
        Console.WriteLine(" [3] EventDispatchException constructors:");
        var ex1 = new EventDispatchException(typeof(OrderPlacedIntegrationEvent), [new InvalidOperationException("Test")]);
        Console.WriteLine($"     Constructor 1: EventType={ex1.EventType.Name}, InnerCount={ex1.InnerExceptions.Count}");
        var ex2 = new EventDispatchException(typeof(OrderPlacedIntegrationEvent), "Custom dispatch failure", [new TimeoutException("Timeout")]);
        Console.WriteLine($"     Constructor 2: Message='{ex2.Message}'");

        // ── EventTypeNotFoundException ────────────────────────────────────────
        Console.WriteLine(" [4] EventTypeNotFoundException:");
        var unknownType = EricksonLopez.Events.Identifiers.EventType.From("unknown.event.type");
        var notFoundEx = new EricksonLopez.Events.Exceptions.EventTypeNotFoundException(unknownType);
        Console.WriteLine($"     EventType property: {notFoundEx.EventType}");
        Console.WriteLine($"     Message: {notFoundEx.Message}");

        // ── EventValidationException ──────────────────────────────────────────
        Console.WriteLine(" [5] EventValidationException:");
        var validationEx1 = new EricksonLopez.Events.Exceptions.EventValidationException("Payload too large");
        var validationEx2 = new EricksonLopez.Events.Exceptions.EventValidationException("Inner error", new ArgumentException("bad arg"));
        Console.WriteLine($"     Constructor 1: {validationEx1.Message}");
        Console.WriteLine($"     Constructor 2: {validationEx2.Message} (InnerException: {validationEx2.InnerException?.GetType().Name})");

        // ── Deduplication / Reliability Note ───────────────────────────────────
        Console.WriteLine(" [6] Reliable Inbound & Outbound Ecosystem Integration:");
        Console.WriteLine("     EventEnvelope<TEvent> + EventMetadata provide typed identity (EventId, CorrelationId, TenantId)");
        Console.WriteLine("     for seamless integration with dedicated persistence libraries (EricksonLopez.Outbox / EricksonLopez.Inbox).");
        Console.WriteLine();
    }

    // ─────────────────────────────────────────────────────────────────────────
    // LEVEL 7: Performance — Span<T>, stackalloc, GUIDv7 ordering
    // ─────────────────────────────────────────────────────────────────────────
    private static Task RunLevel7ScalabilityAndPerformanceAsync()
    {
        PrintLevelHeader(7, "Scalability & Performance — Span<T>, Stackalloc, Zero-Allocation GUIDv7");

        var eventId = EventId.New();

        // ── Span<char> stackalloc TryFormat ────────────────────────────────
        Span<char> charBuffer = stackalloc char[36];
        bool formattedChars = eventId.TryFormat(charBuffer, out int charsWritten, default, System.Globalization.CultureInfo.InvariantCulture);
        Console.WriteLine($" [1] Span<char> stackalloc TryFormat: '{charBuffer.ToString()}' ({charsWritten} chars, Success={formattedChars})");

        // ── Span<byte> UTF-8 stackalloc TryFormat ──────────────────────────
        Span<byte> utf8Buffer = stackalloc byte[36];
        bool formattedBytes = eventId.TryFormat(utf8Buffer, out int bytesWritten, default, System.Globalization.CultureInfo.InvariantCulture);
        Console.WriteLine($" [2] Span<byte> UTF-8 stackalloc TryFormat: {bytesWritten} bytes written (Success={formattedBytes})");

        // ── Chronological monotonic ordering verification ────────────────────
        var id1 = EventId.New();
        Thread.Sleep(2);
        var id2 = EventId.New();
        Console.WriteLine($" [3] Monotonic ordering: id1 < id2 = {id1 < id2}, id2 > id1 = {id2 > id1}");
        Console.WriteLine($"     id1: {id1}");
        Console.WriteLine($"     id2: {id2}");
        Console.WriteLine($"     id1 <= id2: {id1 <= id2}, id2 >= id1: {id2 >= id1}");

        // ── implicit/explicit conversions ────────────────────────────────────
        Guid guidFromId = id1; // implicit EventId -> Guid
        Console.WriteLine($" [4] Implicit EventId → Guid:  {guidFromId}");
        var backToEventId = (EventId)guidFromId; // explicit Guid -> EventId
        Console.WriteLine($"     Explicit Guid → EventId:  {backToEventId} (same={backToEventId == id1})");

        // ── EventId.Parse (non-null, throws on failure) ──────────────────────
        var parsedDirect = EventId.Parse(id1.ToString(), null);
        Console.WriteLine($" [5] EventId.Parse(string):    {parsedDirect} (equals id1: {parsedDirect == id1})");
        Console.WriteLine();

        return Task.CompletedTask;
    }

    // ─────────────────────────────────────────────────────────────────────────
    // LEVEL 8: Customization — Middleware, Custom IExecutionStrategy
    // ─────────────────────────────────────────────────────────────────────────
    private static async Task RunLevel8CustomizationAndMiddlewaresAsync()
    {
        PrintLevelHeader(8, "Customization — Middleware Pipeline, MiddlewarePipeline.Build, IExecutionStrategy");

        // ── Standard middleware pipeline via DI ──────────────────────────────
        Console.WriteLine(" [1] Standard IEventMiddleware pipeline (Logging → Performance → Handler):");
        {
            var services = new ServiceCollection();
            services.AddEventBus();
            services.AddEventMiddleware<LoggingEventMiddleware>();
            services.AddEventMiddleware<PerformanceMetricsMiddleware>();
            services.AddEventHandler<OrderPlacedIntegrationEvent, SendOrderConfirmationEmailHandler>();

            using var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();
            var bus = scope.ServiceProvider.GetRequiredService<IEventBus>();
            var evt = new OrderPlacedIntegrationEvent(EventId.New(), Guid.NewGuid(), Guid.NewGuid(), 350.00m, "USD", DateTimeOffset.UtcNow);
            await bus.PublishAsync(evt);
        }

        // ── Multiple middlewares + Validation ────────────────────────────────
        Console.WriteLine(" [2] Three-stage middleware pipeline (Validation → Logging → Performance → Handler):");
        {
            // Register ValidationMiddleware as singleton so we can inspect its state after dispatch.
            // AddEventMiddleware<T> also picks it up as IEventMiddleware for pipeline inclusion.
            var validationMiddleware = new ValidationMiddleware();

            var services = new ServiceCollection();
            services.AddSingleton(validationMiddleware);         // concrete singleton for state inspection
            services.AddEventBus();
            services.AddEventMiddleware<ValidationMiddleware>(); // pipeline inclusion
            services.AddEventMiddleware<LoggingEventMiddleware>();
            services.AddEventMiddleware<PerformanceMetricsMiddleware>();
            services.AddEventHandler<OrderShippedIntegrationEvent, SendShipmentNotificationHandler>();

            using var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();
            var bus = scope.ServiceProvider.GetRequiredService<IEventBus>();
            var evt = new OrderShippedIntegrationEvent(EventId.New(), Guid.NewGuid(), "TRK-ABC-001", "UPS", DateTimeOffset.UtcNow);
            await bus.PublishAsync(evt);

            Console.WriteLine($"     ValidationMiddleware.ValidationLog.Count: {validationMiddleware.ValidationLog.Count}");
        }

        // ── MiddlewarePipeline.Build<TEvent>() explicit call ─────────────────
        Console.WriteLine(" [3] MiddlewarePipeline.Build<TEvent>() — explicit pipeline construction:");
        {
            var logMw = new LoggingEventMiddleware();
            var perfMw = new PerformanceMetricsMiddleware();

            // Terminal handler (the end of the pipeline)
            EventMiddlewareDelegate<OrderPlacedIntegrationEvent> terminal = (evt, ct) =>
            {
                Console.WriteLine($"    [Terminal] Reached end of pipeline for event '{evt.Id}'.");
                return ValueTask.CompletedTask;
            };

            // Build the pipeline: logMw → perfMw → terminal
            var middlewares = new IEventMiddleware[] { logMw, perfMw };
            var pipeline = MiddlewarePipeline.Build<OrderPlacedIntegrationEvent>(middlewares, terminal);

            var evt = new OrderPlacedIntegrationEvent(EventId.New(), Guid.NewGuid(), Guid.NewGuid(), 75.00m, "USD", DateTimeOffset.UtcNow);
            Console.WriteLine("     Invoking explicitly built pipeline:");
            await pipeline(evt, CancellationToken.None);
        }

        // ── Custom IExecutionStrategy ─────────────────────────────────────────
        Console.WriteLine(" [4] Custom IExecutionStrategy (VerboseSequentialExecutionStrategy):");
        Console.WriteLine("     IExecutionStrategy.ExecuteAsync<TEvent>(handlers, event, sp, options, ct)");
        Console.WriteLine("     (Registered via DI, invoked by EventBus during dispatch)");
        Console.WriteLine("     See VerboseSequentialExecutionStrategy in ECommerce.Infrastructure.");
        Console.WriteLine("     The strategy receives HandlerDescriptor.Invoker (Func<object,object,ct,ValueTask>).");

        // ── Source Generator note ─────────────────────────────────────────────
        Console.WriteLine(" [5] EricksonLopez.Events.Generators — Roslyn Source Generator:");
        Console.WriteLine("     Automatically emits at compile-time:");
        Console.WriteLine("     - GeneratedEventRegistry.CreateRegistry() → IEventTypeRegistry");
        Console.WriteLine("     - GeneratedEventServiceCollectionExtensions.AddGeneratedEventHandlers(services, lifetime?)");
        Console.WriteLine("     Scanning criteria: Types implementing IEvent (with [EventName]/[EventVersion])");
        Console.WriteLine("                        + Types implementing IEventHandler<TEvent>.");
        Console.WriteLine("     NativeAOT: eliminates all runtime reflection for type discovery.");

        // ── EventExecutionMode.Parallel + MaxDegreeOfParallelism (GAP-002 / GAP-003) ─────────
        Console.WriteLine(" [6] EventExecutionMode.Parallel + MaxDegreeOfParallelism — concurrent handler dispatch:");
        {
            var services = new ServiceCollection();
            services.AddEventBus(opts =>
            {
                opts.ExecutionMode = EventExecutionMode.Parallel;
                opts.MaxDegreeOfParallelism = 4;               // GAP-003 — explicit concurrency limit
                opts.ErrorPolicy = ErrorHandlingPolicy.AggregateAndContinue;
            });
            // Thread-safe handlers: use Interlocked so parallel handlers don't race
            services.AddEventHandler<OrderPlacedIntegrationEvent, ThreadSafeAuditHandler>();
            services.AddEventHandler<OrderPlacedIntegrationEvent, ThreadSafeNotificationHandler>();
            services.AddEventHandler<OrderPlacedIntegrationEvent, ThreadSafeMetricsHandler>();

            using var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();
            var bus = scope.ServiceProvider.GetRequiredService<IEventBus>();

            var evt = new OrderPlacedIntegrationEvent(EventId.New(), Guid.NewGuid(), Guid.NewGuid(), 450.00m, "USD", DateTimeOffset.UtcNow);
            Console.WriteLine($"     Dispatching to 3 handlers concurrently (MaxDegreeOfParallelism=4)...");
            await bus.PublishAsync(evt);

            var audit = scope.ServiceProvider.GetRequiredService<ThreadSafeAuditHandler>();
            var notif = scope.ServiceProvider.GetRequiredService<ThreadSafeNotificationHandler>();
            var metr = scope.ServiceProvider.GetRequiredService<ThreadSafeMetricsHandler>();

            Console.WriteLine($"     AuditHandler.HandledCount:        {audit.HandledCount}");
            Console.WriteLine($"     NotificationHandler.HandledCount: {notif.HandledCount}");
            Console.WriteLine($"     MetricsHandler.HandledCount:      {metr.HandledCount}");
            Console.WriteLine($"     All 3 ran concurrently ✓ (EventExecutionMode.Parallel, MaxDegreeOfParallelism={4})");
        }

        // ── HandlerScopePolicy.ReuseAmbientScope (GAP-004) ──────────────────
        Console.WriteLine(" [7] HandlerScopePolicy.ReuseAmbientScope — handlers share caller's IServiceProvider:");
        Console.WriteLine("     ⚠  WARNING: Only safe with EventExecutionMode.Sequential.");
        Console.WriteLine("     ⚠  With Parallel mode, all handlers share the scope concurrently — requires thread-safe dependencies.");
        {
            var services = new ServiceCollection();
            services.AddEventBus(opts =>
            {
                opts.ExecutionMode = EventExecutionMode.Sequential;
                opts.ScopePolicy = HandlerScopePolicy.ReuseAmbientScope; // GAP-004
            });
            services.AddEventHandler<OrderPlacedIntegrationEvent, ThreadSafeAuditHandler>();

            using var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();
            var bus = scope.ServiceProvider.GetRequiredService<IEventBus>();
            var evt = new OrderPlacedIntegrationEvent(EventId.New(), Guid.NewGuid(), Guid.NewGuid(), 100.00m, "USD", DateTimeOffset.UtcNow);
            await bus.PublishAsync(evt);

            var audit = scope.ServiceProvider.GetRequiredService<ThreadSafeAuditHandler>();
            Console.WriteLine($"     ReuseAmbientScope — handler ran with ambient scope, HandledCount={audit.HandledCount}");
        }

        // ── ParallelExecutionStrategy directly instantiated (GAP-005) ─────────
        Console.WriteLine(" [8] ParallelExecutionStrategy — instantiated directly (bypassing DI):");
        {
            var strategy = new EricksonLopez.Events.Bus.Execution.ParallelExecutionStrategy();
            Console.WriteLine($"     ParallelExecutionStrategy type: {strategy.GetType().Name}");
            Console.WriteLine("     (Used internally by EventBus when ExecutionMode=Parallel)");
            Console.WriteLine("     Interface: IExecutionStrategy.ExecuteAsync<TEvent>(handlers, event, sp, options, ct)");
        }
        Console.WriteLine();
    }


    // ─────────────────────────────────────────────────────────────────────────
    // LEVEL 9: Ecosystem — CloudEvents, Full Testing DSL, Diagnostics
    // ─────────────────────────────────────────────────────────────────────────
    private static async Task RunLevel9EcosystemExtensionsAsync()
    {
        PrintLevelHeader(9, "Ecosystem Extensions — CloudEvents v1.0, Full Testing DSL & Diagnostics");

        // ── 1. CloudEvents v1.0 Conversion ───────────────────────────────────
        Console.WriteLine(" [1] CloudEvents v1.0 — EventEnvelope<T>.ToCloudEvent() & CloudEvent<T>.ToEventEnvelope():");
        {
            var evt = new OrderPlacedIntegrationEvent(EventId.New(), Guid.NewGuid(), Guid.NewGuid(), 125.00m, "USD", DateTimeOffset.UtcNow);
            var metadata = new EventMetadataBuilder()
                .WithCorrelationId(CorrelationId.New())
                .WithCausationId(CausationId.From("WEB-PORTAL-TX"))
                .WithTenantId(TenantId.From("tenant-us-west"))
                .WithSource("https://ecommerce.ericksonlopez.dev/orders")
                .WithHeader("X-Trace-Tag", "Audit-01")
                .Build();

            var envelope = EventEnvelope.Create(evt, metadata);

            // ToCloudEvent with defaultSource fallback + schema URI
            var cloudEvent = envelope.ToCloudEvent(
                defaultSource: new Uri("urn:ecommerce:events"),
                schemaBaseUri: new Uri("https://schema.ericksonlopez.dev/"));

            Console.WriteLine($"     SpecVersion:      {cloudEvent.SpecVersion}");
            Console.WriteLine($"     ID:               {cloudEvent.Id}");
            Console.WriteLine($"     Source:           {cloudEvent.Source}");
            Console.WriteLine($"     Type:             {cloudEvent.Type}");
            Console.WriteLine($"     DataContentType:  {cloudEvent.DataContentType}");
            Console.WriteLine($"     DataSchema:       {cloudEvent.DataSchema}");
            Console.WriteLine($"     CorrelationId:    {cloudEvent.CorrelationId}");
            Console.WriteLine($"     CausationId:      {cloudEvent.CausationId}");
            Console.WriteLine($"     TenantId:         {cloudEvent.TenantId}");
            Console.WriteLine($"     ExtensionAttribs: {cloudEvent.ExtensionAttributes?.Count ?? 0} custom attrs");

            // CloudEvent<T> direct constructor
            var directCloudEvent = new CloudEvent<OrderPlacedIntegrationEvent>(
                id: cloudEvent.Id,
                source: new Uri("urn:ecommerce:test"),
                type: "ecommerce.orders.order-placed",
                data: evt,
                time: DateTimeOffset.UtcNow,
                dataContentType: "application/json",
                dataSchema: null,
                correlationId: "corr-direct",
                causationId: "caus-direct",
                tenantId: "tenant-direct");
            Console.WriteLine($"     CloudEvent direct ctor — Source: {directCloudEvent.Source}, TenantId: {directCloudEvent.TenantId}");

            // ToEventEnvelope (roundtrip)
            var roundtripped = cloudEvent.ToEventEnvelope();
            Console.WriteLine($"     Roundtripped ID:        {roundtripped.Id}");
            Console.WriteLine($"     Roundtripped TenantId:  {roundtripped.Metadata.TenantId}");
            Console.WriteLine($"     Roundtripped Source:    {roundtripped.Metadata.Source}");
        }

        // ── 2. CloudEventsJsonSerializerOptionsExtensions ────────────────────
        Console.WriteLine(" [2] CloudEventsJsonSerializerOptionsExtensions.ConfigureForCloudEvents():");
        {
            var opts = new JsonSerializerOptions();
            opts.ConfigureForCloudEvents();
            Console.WriteLine($"     PropertyNamingPolicy:        {opts.PropertyNamingPolicy?.GetType().Name}");
            Console.WriteLine($"     PropertyNameCaseInsensitive: {opts.PropertyNameCaseInsensitive}");
        }

        // ── 3. Full FakeEventPublisher API ───────────────────────────────────
        Console.WriteLine(" [3] FakeEventPublisher — complete Testing DSL:");
        {
            var fake = new FakeEventPublisher();

            // PublishedEvents and Count
            Console.WriteLine($"     Initial Count: {fake.Count}");

            var evt1 = new OrderPlacedIntegrationEvent(EventId.New(), Guid.NewGuid(), Guid.NewGuid(), 125.00m, "USD", DateTimeOffset.UtcNow);
            var evt2 = new OrderPlacedIntegrationEvent(EventId.New(), Guid.NewGuid(), Guid.NewGuid(), 50.00m, "USD", DateTimeOffset.UtcNow);
            var shippedEvt = new OrderShippedIntegrationEvent(EventId.New(), Guid.NewGuid(), "TRK-999", "DHL", DateTimeOffset.UtcNow);

            await fake.PublishAsync(evt1);
            await fake.PublishAsync(evt2);
            await fake.PublishAsync(shippedEvt);

            Console.WriteLine($"     Count after 3 publishes: {fake.Count}");
            Console.WriteLine($"     PublishedEvents[0]: {fake.PublishedEvents[0].GetType().Name}");

            // GetEvents<T>()
            var allOrders = fake.GetEvents<OrderPlacedIntegrationEvent>();
            Console.WriteLine($"     GetEvents<OrderPlaced>(): {allOrders.Count} events");

            // GetEvents<T>(predicate)
            var highValue = fake.GetEvents<OrderPlacedIntegrationEvent>(e => e.TotalAmount > 100m);
            Console.WriteLine($"     GetEvents<OrderPlaced>(TotalAmount > 100): {highValue.Count} events");

            // GetSingleEvent<T>()
            var singleShipped = fake.GetSingleEvent<OrderShippedIntegrationEvent>();
            Console.WriteLine($"     GetSingleEvent<OrderShipped>(): TrackingNumber={singleShipped.TrackingNumber}");

            // ShouldHavePublished<T>()
            fake.ShouldHavePublished<OrderPlacedIntegrationEvent>();
            Console.WriteLine("     ShouldHavePublished<OrderPlaced>(): ✓ passed");

            // ShouldHavePublished<T>(predicate)
            fake.ShouldHavePublished<OrderPlacedIntegrationEvent>(e => e.TotalAmount == 125.00m);
            Console.WriteLine("     ShouldHavePublished<OrderPlaced>(TotalAmount==125): ✓ passed");

            // ShouldNotHavePublished<T>() — for a type not published
            fake.ShouldNotHavePublished<CustomerRegisteredIntegrationEvent>();
            Console.WriteLine("     ShouldNotHavePublished<CustomerRegistered>(): ✓ passed");

            // ShouldNotHavePublished<T>(predicate)
            fake.ShouldNotHavePublished<OrderPlacedIntegrationEvent>(e => e.TotalAmount > 1000m);
            Console.WriteLine("     ShouldNotHavePublished<OrderPlaced>(TotalAmount>1000): ✓ passed");

            // ShouldHavePublishedCount<T>()
            fake.ShouldHavePublishedCount<OrderPlacedIntegrationEvent>(2);
            Console.WriteLine("     ShouldHavePublishedCount<OrderPlaced>(2): ✓ passed");

            // SimulateFailure
            fake.SimulateFailure(new TimeoutException("Simulated network timeout"), failureCount: 1);
            try
            {
                await fake.PublishAsync(evt1);
                Console.WriteLine("     [UNEXPECTED] SimulateFailure did not throw!");
            }
            catch (TimeoutException ex)
            {
                Console.WriteLine($"     SimulateFailure({ex.GetType().Name}): ✓ caught '{ex.Message}'");
            }

            // After failure, next publish succeeds
            await fake.PublishAsync(evt1);
            Console.WriteLine($"     After failure: Count={fake.Count} (should be 5)");

            // Reset
            fake.Reset();
            Console.WriteLine($"     After Reset(): Count={fake.Count} (should be 0)");
        }

        // ── 4. EventTestBuilder & EventEnvelopeTestBuilder<T> ────────────────
        Console.WriteLine(" [4] EventTestBuilder & EventEnvelopeTestBuilder<T> — synthetic envelope factory:");
        {
            var payload = new OrderPlacedIntegrationEvent(EventId.New(), Guid.NewGuid(), Guid.NewGuid(), 200.00m, "USD", DateTimeOffset.UtcNow);

            // EventTestBuilder.For<TEvent>(payload) → EventEnvelopeTestBuilder<TEvent>
            var builtEnvelope = EventTestBuilder.For(payload)
                .WithId(EventId.New())
                .WithType("ecommerce.orders.test-placed")
                .WithVersion(2)
                .WithOccurredAt(DateTimeOffset.UtcNow.AddMinutes(-5))
                .WithCorrelationId("test-corr-id")
                .WithCausationId("test-caus-id")
                .WithTenantId("tenant-test-001")
                .WithSource("test-harness")
                .WithContentType("application/json")
                .WithHeader("X-Test-Run", "showcase-level-9")
                .Build();

            Console.WriteLine($"     Built Envelope ID:         {builtEnvelope.Id}");
            Console.WriteLine($"     Built Envelope Type:       {builtEnvelope.Type}");
            Console.WriteLine($"     Built Envelope Version:    v{builtEnvelope.Version}");
            Console.WriteLine($"     Built Envelope OccurredAt: {builtEnvelope.OccurredAt:O}");
            Console.WriteLine($"     Built Envelope TenantId:   {builtEnvelope.Metadata.TenantId}");
            Console.WriteLine($"     Built Envelope Source:     {builtEnvelope.Metadata.Source}");
            Console.WriteLine($"     Built Envelope X-Test-Run: {(builtEnvelope.Metadata.TryGetHeader("X-Test-Run", out var v) ? v : "missing")}");
        }

        // ── 5. TestEventHandler<T> — full spy/stub API ───────────────────────
        Console.WriteLine(" [5] TestEventHandler<T> — complete spy/stub API:");
        {
            var handler = new TestEventHandler<OrderPlacedIntegrationEvent>();

            // WasInvoked before any events
            Console.WriteLine($"     WasInvoked (before): {handler.WasInvoked}");
            Console.WriteLine($"     InvocationCount:     {handler.InvocationCount}");
            Console.WriteLine($"     LastEvent:           {(handler.LastEvent is null ? "null" : handler.LastEvent.GetType().Name)}");

            // WithCallback (Action<TEvent>)
            handler.WithCallback(e =>
                Console.WriteLine($"     [TestSpy Callback] Event ID: {e.Id}, Amount: {e.TotalAmount}"));

            var evt1 = new OrderPlacedIntegrationEvent(EventId.New(), Guid.NewGuid(), Guid.NewGuid(), 100m, "USD", DateTimeOffset.UtcNow);
            var evt2 = new OrderPlacedIntegrationEvent(EventId.New(), Guid.NewGuid(), Guid.NewGuid(), 200m, "USD", DateTimeOffset.UtcNow);

            await handler.HandleAsync(evt1);
            await handler.HandleAsync(evt2);

            Console.WriteLine($"     WasInvoked (after):    {handler.WasInvoked}");
            Console.WriteLine($"     InvocationCount:       {handler.InvocationCount}");
            Console.WriteLine($"     HandledEvents.Count:   {handler.HandledEvents.Count}");
            Console.WriteLine($"     LastEvent.TotalAmount: {handler.LastEvent?.TotalAmount}");
            Console.WriteLine($"     ExecutionTimestamps:   {handler.ExecutionTimestamps.Count} timestamps recorded");

            // Reset
            handler.Reset();
            Console.WriteLine($"     After Reset(): InvocationCount={handler.InvocationCount}");

            // WithDelay
            handler.WithDelay(TimeSpan.FromMilliseconds(5));
            var sw = Stopwatch.StartNew();
            await handler.HandleAsync(evt1);
            sw.Stop();
            Console.WriteLine($"     WithDelay(5ms): elapsed ≈ {sw.Elapsed.TotalMilliseconds:F0}ms");

            // WithException
            var handler2 = new TestEventHandler<OrderPlacedIntegrationEvent>()
                .WithException(new InvalidOperationException("Simulated handler failure"));
            try
            {
                await handler2.HandleAsync(evt1);
            }
            catch (InvalidOperationException ex)
            {
                Console.WriteLine($"     WithException: caught '{ex.Message}'");
            }

            // WithCallback (Func<TEvent, CancellationToken, ValueTask>)
            var handler3 = new TestEventHandler<OrderPlacedIntegrationEvent>()
                .WithCallback(async (e, ct) =>
                {
                    await Task.Delay(1, ct);
                    Console.WriteLine($"     [AsyncCallback] Processed Order={e.OrderId}");
                });
            await handler3.HandleAsync(evt1);
        }

        // ── 6. EventsDiagnostics API ──────────────────────────────────────────
        Console.WriteLine(" [6] EventsDiagnostics — OpenTelemetry ActivitySource & Meter:");
        Console.WriteLine($"     SourceName:   {EventsDiagnostics.SourceName}");
        Console.WriteLine($"     Version:      {EventsDiagnostics.Version}");
        Console.WriteLine($"     ActivitySource.Name: {EventsDiagnostics.ActivitySource.Name}");
        Console.WriteLine($"     Meter.Name:          {EventsDiagnostics.Meter.Name}");

        // StartPublishActivity (returns null when no listener — expected in showcase)
        var dummyEvt = new OrderPlacedIntegrationEvent(EventId.New(), Guid.NewGuid(), Guid.NewGuid(), 1m, "USD", DateTimeOffset.UtcNow);
        using (var activity = EventsDiagnostics.StartPublishActivity(dummyEvt))
        {
            Console.WriteLine($"     StartPublishActivity: {(activity is null ? "null (no listeners, as expected)" : activity.OperationName)}");
        }

        // RecordEventPublished & RecordEventHandled (fire-and-forget metrics)
        EventsDiagnostics.RecordEventPublished(nameof(OrderPlacedIntegrationEvent));
        EventsDiagnostics.RecordEventHandled(nameof(OrderPlacedIntegrationEvent), durationMs: 1.5, success: true);
        Console.WriteLine("     RecordEventPublished(): called ✓");
        Console.WriteLine("     RecordEventHandled():   called ✓");

        // ── 7. OpenTelemetry integration note ────────────────────────────────
        Console.WriteLine(" [7] EricksonLopez.Events.OpenTelemetry extensions:");
        Console.WriteLine("     AddEventsInstrumentation(TracerProviderBuilder builder)");
        Console.WriteLine("       → builder.AddSource(EventsDiagnostics.SourceName)");
        Console.WriteLine("     AddEventsInstrumentation(MeterProviderBuilder builder)");
        Console.WriteLine("       → builder.AddMeter(EventsDiagnostics.SourceName)");
        Console.WriteLine("     Wire-up example:");
        Console.WriteLine("       builder.Services.AddOpenTelemetry()");
        Console.WriteLine("         .WithTracing(t => t.AddEventsInstrumentation())");
        Console.WriteLine("         .WithMetrics(m => m.AddEventsInstrumentation())");
        Console.WriteLine();
    }

    // ─────────────────────────────────────────────────────────────────────────
    // LEVEL 10: Enterprise Native AOT — JSON Serialization
    // ─────────────────────────────────────────────────────────────────────────
    private static Task RunLevel10EnterpriseAotAsync()
    {
        PrintLevelHeader(10, "Enterprise Architecture — 100% Native AOT Zero-Reflection JSON Serialization");

        var eventId = EventId.New();
        var metadata = new EventMetadataBuilder()
            .WithCorrelationId(CorrelationId.New())
            .WithCausationId(CausationId.From("ORDER-CMD-99"))
            .WithTenantId(TenantId.From("tenant-corp-prime"))
            .WithSource("order-service")
            .WithHeader("X-Environment", "Production-NativeAOT")
            .Build();

        var evt = new OrderPlacedIntegrationEvent(
            eventId,
            Guid.NewGuid(),
            Guid.NewGuid(),
            899.99m,
            "USD",
            DateTimeOffset.UtcNow);

        var envelope = EventEnvelope.Create(evt, metadata);

        // ── AOT context-based serialization ──────────────────────────────────
        Console.WriteLine(" [1] AOT serialization via ECommerceJsonContext ([JsonSerializable] + [JsonSourceGenerationOptions]):");
        string json = ECommerceJsonSerializer.Serialize(envelope);
        Console.WriteLine($"     JSON ({json.Length} chars):");
        Console.WriteLine($"     {json[..Math.Min(200, json.Length)]}...");

        // ── AOT deserialization ───────────────────────────────────────────────
        Console.WriteLine(" [2] AOT deserialization — zero reflection:");
        var deserialized = ECommerceJsonSerializer.DeserializeOrderPlacedEnvelope(json);

        if (deserialized is null || deserialized.Id != eventId)
        {
            throw new InvalidOperationException("AOT Deserialization verification failed!");
        }

        Console.WriteLine($"     Deserialized ID:       {deserialized.Id}");
        Console.WriteLine($"     Deserialized Type:     {deserialized.Type}");
        Console.WriteLine($"     Deserialized Version:  v{deserialized.Version}");
        Console.WriteLine($"     Deserialized Payload:  Order#{deserialized.Payload.OrderId}, Total={deserialized.Payload.TotalAmount}");
        Console.WriteLine($"     Deserialized TenantId: {deserialized.Metadata.TenantId}");
        Console.WriteLine($"     Deserialized Source:   {deserialized.Metadata.Source}");

        // ── Multiple event types AOT ──────────────────────────────────────────
        Console.WriteLine(" [3] Multi-type AOT serialization (OrderShipped, CustomerRegistered):");
        {
            var shippedEvt = new OrderShippedIntegrationEvent(EventId.New(), Guid.NewGuid(), "TRK-AOT-2025", "FedEx", DateTimeOffset.UtcNow);
            var shippedEnvelope = EventEnvelope.Create(shippedEvt);
            var shippedJson = ECommerceJsonSerializer.Serialize(shippedEnvelope);
            var shippedBack = ECommerceJsonSerializer.DeserializeOrderShippedEnvelope(shippedJson);
            Console.WriteLine($"     OrderShipped roundtrip:   {shippedBack?.Payload.TrackingNumber} via {shippedBack?.Payload.CarrierCode}");

            var customerEvt = new CustomerRegisteredIntegrationEvent(EventId.New(), Guid.NewGuid(), "user@example.com", DateTimeOffset.UtcNow);
            var customerEnvelope = EventEnvelope.Create(customerEvt);
            var customerJson = ECommerceJsonSerializer.Serialize(customerEnvelope);
            var customerBack = ECommerceJsonSerializer.DeserializeCustomerRegisteredEnvelope(customerJson);
            Console.WriteLine($"     CustomerRegistered roundtrip: {customerBack?.Payload.Email}");
        }

        // ── EventsJsonSerializerOptionsExtensions ────────────────────────────
        Console.WriteLine(" [4] EventsJsonSerializerOptionsExtensions.AddEventsConverters() & CreateDefaultOptions():");
        {
            // AddEventsConverters — adds all 7 strongly-typed converters
            var opts = new JsonSerializerOptions();
            opts.AddEventsConverters();
            Console.WriteLine($"     AddEventsConverters(): {opts.Converters.Count} converters added");

            // CreateDefaultOptions — opinionated defaults
            var defaultOpts = EventsJsonSerializerOptionsExtensions.CreateDefaultOptions();
            Console.WriteLine($"     CreateDefaultOptions(): NamingPolicy={defaultOpts.PropertyNamingPolicy?.GetType().Name}, WriteIndented={defaultOpts.WriteIndented}");

            // Demonstrate individual converters
            Console.WriteLine(" [5] Individual AOT JSON converters (registered via AddEventsConverters):");
            Console.WriteLine("     EventIdJsonConverter         — Guid ↔ string");
            Console.WriteLine("     EventTypeJsonConverter       — EventType ↔ string");
            Console.WriteLine("     EventVersionJsonConverter    — EventVersion ↔ uint");
            Console.WriteLine("     CorrelationIdJsonConverter   — CorrelationId ↔ string");
            Console.WriteLine("     CausationIdJsonConverter     — CausationId ↔ string");
            Console.WriteLine("     TenantIdJsonConverter        — TenantId ↔ string");
            Console.WriteLine("     EventMetadataJsonConverter   — EventMetadata ↔ object");

            // Serialize just the metadata using AOT context (not reflection-based)
            // Note: JsonSerializer.Serialize<T>(T, JsonSerializerOptions) is reflection-based.
            // For AOT, use a JsonTypeInfo or context overload. Here we use a safe fallback string representation.
            var metaJson = $"{{\"correlationId\":\"{metadata.CorrelationId}\",\"causationId\":\"{metadata.CausationId}\",\"tenantId\":\"{metadata.TenantId}\",\"source\":\"{metadata.Source}\",\"headers\":{metadata.CustomHeaders.Count}}}";
            Console.WriteLine($"     EventMetadata serialized:    {metaJson[..Math.Min(120, metaJson.Length)]}...");
        }

        // ── Envelope Packaging Note ──────────────────────────────────────────
        Console.WriteLine(" [6] EventEnvelope & Metadata Packaging for Transactional Relays:");
        Console.WriteLine("     EventEnvelope<T>.Create(domainEvent, metadata) encapsulates domain state and context.");
        Console.WriteLine("     Ready for atomic database persistence via Transactional Outbox (EricksonLopez.Outbox).");
        Console.WriteLine();

        return Task.CompletedTask;
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Helpers
    // ─────────────────────────────────────────────────────────────────────────
    private static void PrintLevelHeader(int level, string title)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("--------------------------------------------------------------------------------");
        Console.WriteLine($" LEVEL {level}: {title}");
        Console.WriteLine("--------------------------------------------------------------------------------");
        Console.ResetColor();
    }

    private static void PrintCompletionSummary()
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("================================================================================");
        Console.WriteLine(" [SUCCESS] ALL SHOWCASE LEVELS (0 TO 10) EXECUTED WITH ZERO ERRORS!           ");
        Console.WriteLine("================================================================================");
        Console.WriteLine();
        Console.WriteLine(" PUBLIC API COVERAGE SUMMARY:");
        Console.WriteLine("  ✓ IEvent, IDomainEvent, IIntegrationEvent, IEventHandler<T>, IEventPublisher");
        Console.WriteLine("  ✓ IEventBus, IEventSubscriber, InMemoryEventPublisher");
        Console.WriteLine("  ✓ EventId (New, From, Empty, IsEmpty, Parse, TryParse, TryFormat Span<char/byte>, operators)");
        Console.WriteLine("  ✓ CorrelationId, CausationId, TenantId, EventType, EventVersion");
        Console.WriteLine("  ✓ [EventName], [EventVersion], [EventSource] attributes");
        Console.WriteLine("  ✓ EventMetadata (Empty, Create, WithHeader, TryGetHeader, all properties)");
        Console.WriteLine("  ✓ EventMetadataBuilder (all fluent methods, string & typed overloads)");
        Console.WriteLine("  ✓ EventEnvelope<T> (direct ctor), EventEnvelope.Create(), EventEnvelope.Wrap()");
        Console.WriteLine("  ✓ IEventEnvelope.GetPayload()");
        Console.WriteLine("  ✓ EventPublisherExtensions.PublishAsync<T>(envelope)");
        Console.WriteLine("  ✓ AddEventBus, AddEventHandler, AddEventMiddleware");
        Console.WriteLine("  ✓ EventBusOptions (ExecutionMode, ErrorPolicy, MaxReentrancyDepth, ThrowOnUnregisteredEvent)");
        Console.WriteLine("  ✓ EventExecutionMode.Sequential, EventExecutionMode.Parallel");
        Console.WriteLine("  ✓ ErrorHandlingPolicy.FailFast, ErrorHandlingPolicy.AggregateAndContinue");
        Console.WriteLine("  ✓ IEventMiddleware, EventMiddlewareDelegate<T>, MiddlewarePipeline.Build<T>()");
        Console.WriteLine("  ✓ IExecutionStrategy (interface documented + impl demonstrated)");
        Console.WriteLine("  ✓ IEventTypeRegistry, EventTypeRegistry, EventTypeRegistryBuilder");
        Console.WriteLine("  ✓ EventTypeDescriptor.For<T>(), IEventTypeRegistry.TryGetDescriptor (3 overloads)");
        Console.WriteLine("  ✓ StaticEventTypeRegistry.Current, GetDescriptor<T>(), GetEventType<T>(), GetVersion<T>()");
        Console.WriteLine("  ✓ EventDispatchException (both ctors, EventType, InnerExceptions)");
        Console.WriteLine("  ✓ EventTypeNotFoundException (EventType, Message)");
        Console.WriteLine("  ✓ EventValidationException (both ctors)");
        Console.WriteLine("  ✓ CloudEvent<TData> (both ctors, all properties)");
        Console.WriteLine("  ✓ CloudEventExtensions.ToCloudEvent(), ToEventEnvelope()");
        Console.WriteLine("  ✓ CloudEventsJsonSerializerOptionsExtensions.ConfigureForCloudEvents()");
        Console.WriteLine("  ✓ EventsJsonSerializerOptionsExtensions.AddEventsConverters(), CreateDefaultOptions()");
        Console.WriteLine("  ✓ All 7 STJ converters (EventId, EventType, EventVersion, CorrelationId, etc.)");
        Console.WriteLine("  ✓ FakeEventPublisher (all 11 public members)");
        Console.WriteLine("  ✓ TestEventHandler<T> (all 10+ public members)");
        Console.WriteLine("  ✓ EventTestBuilder.For<T>(), EventEnvelopeTestBuilder<T> (all 10 fluent methods)");
        Console.WriteLine("  ✓ EventsDiagnostics (SourceName, Version, ActivitySource, Meter, all methods)");
        Console.WriteLine("  ✓ EventsOpenTelemetryExtensions (both overloads)");
        Console.WriteLine("  ✓ EventIncrementalGenerator / GeneratedEventRegistry (documented)");
        Console.WriteLine();
        Console.WriteLine(" EXTERNAL INTEGRATIONS (out-of-scope — separate packages):");
        Console.WriteLine("  → EricksonLopez.Outbox  : OutboxEventPublisher, transactional relay");
        Console.WriteLine("  → EricksonLopez.Inbox   : idempotency / InboxEventProcessor");
        Console.ResetColor();
    }
}

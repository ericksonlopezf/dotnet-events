# ADR-030: Distribution of Test Doubles and Testing Utilities as a First-Class Public Package

## Status
Accepted

## Date
2026-08-20

* **Status:** Accepted
* **Date:** 2026-08-20
* **Deciders:** Architecture Team, Erickson Lopez

## Context

Consuming applications (services, bounded contexts, microservices) that integrate `EricksonLopez.Events` need to write unit and integration tests verifying:
1. Event publication (`IEventPublisher` interactions) without configuring real infrastructure or mocking complex internal dispatch pipelines.
2. In-memory event handling and verification of dispatched events.
3. Fluid building and mutation of test envelopes and metadata.

Traditionally, libraries leave testing utilities either:
- Trapped inside internal test assemblies (requiring consumers to reinvent mocks/fakes).
- Substituted with dynamic mock frameworks (e.g., Moq, NSubstitute) which often lead to brittle interaction tests over-coupled to implementation details.

## Decision

1. **Publish `EricksonLopez.Events.Testing` as a First-Class NuGet Package:** Package and ship testing utilities (`FakeEventPublisher`, `TestEventHandler<T>`, `EventTestBuilder`) alongside the production libraries.
2. **Standardize Test Primitives:**
   - `FakeEventPublisher`: An in-memory, thread-safe, recordable event publisher supporting filtering, failure simulation, reset, and fluent assertions.
   - `TestEventHandler<T>`: An in-memory event handler capturing received events with execution counts, delays, and callbacks.
   - `EventTestBuilder`: A fluent builder for test instances of `EventEnvelope<T>` and `EventMetadata`.
3. **Use the Testing Package for Internal Tests:** The internal testing suite must dogfood `EricksonLopez.Events.Testing` to validate its API ergonomics and contract stability.

## Consequences

### Positive
- **Superior Developer Experience (DX):** Consuming applications can write concise, readable unit tests in seconds without mock boilerplate:
  ```csharp
  var publisher = new FakeEventPublisher();
  await sut.ProcessOrderAsync(order);
  publisher.PublishedEvents.Should().ContainSingle(e => e is OrderPlacedEvent);
  ```
- **Test Behavior Over Implementation:** Consumers verify published events via state-based assertions rather than verifying internal method call setups on dynamic mocks.
- **Dogfooding Quality:** Internal test suites continuously validate the public testing abstractions.

### Negative / Trade-offs
- The testing package introduces an additional shipping artifact with public API maintenance and semver compatibility obligations.

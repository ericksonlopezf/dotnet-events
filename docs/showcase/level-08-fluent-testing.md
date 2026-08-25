# Level 08 — Enterprise Testing & Test Doubles

In Level 08, we write isolated unit and integration tests using `EricksonLopez.Events.Testing`.

---

## 1. Using `FakeEventPublisher`

```csharp
using EricksonLopez.Events.Testing;
using Xunit;

public class OrderServiceTests
{
    [Fact]
    public async Task PlaceOrder_ShouldPublish_OrderPlacedDomainEvent()
    {
        // Arrange
        var fakePublisher = new FakeEventPublisher();
        var sut = new OrderService(fakePublisher);

        // Act
        await sut.PlaceOrderAsync(orderId: Guid.NewGuid(), amount: 100m);

        // Assert
        fakePublisher.ShouldHavePublished<OrderPlacedDomainEvent>()
            .With(e => e.TotalAmount == 100m);
    }
}
```

---

## 2. Invariant Assertions

- `fakePublisher.PublishedEnvelopes`: Read-only snapshot of all published envelopes.
- `fakePublisher.Clear()`: Resets captured events between test runs.

// Copyright © Erickson Lopez. MIT License.
using System;
using System.Collections.Generic;

namespace ECommerce.Domain;

using EricksonLopez.Events.Contracts;
using EricksonLopez.Events.Identifiers;

public readonly record struct OrderId(Guid Value)
{
    public static OrderId New() => new(Guid.CreateVersion7());
}

public readonly record struct CustomerId(Guid Value)
{
    public static CustomerId New() => new(Guid.CreateVersion7());
}

public readonly record struct Money(decimal Amount, string Currency)
{
    public static Money USD(decimal amount) => new(amount, "USD");
}

public sealed record OrderPlacedDomainEvent(
    EventId Id,
    OrderId OrderId,
    CustomerId CustomerId,
    Money TotalAmount,
    DateTimeOffset OccurredAt) : IDomainEvent;

public sealed class Order
{
    private readonly List<IDomainEvent> _domainEvents = new();

    public OrderId Id { get; }
    public CustomerId CustomerId { get; }
    public Money Total { get; private set; }
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    public Order(OrderId id, CustomerId customerId, Money total)
    {
        Id = id;
        CustomerId = customerId;
        Total = total;

        _domainEvents.Add(new OrderPlacedDomainEvent(
            EventId.New(),
            id,
            customerId,
            total,
            DateTimeOffset.UtcNow));
    }

    public void ClearDomainEvents() => _domainEvents.Clear();
}



// Copyright © Erickson Lopez. MIT License.
using System;

namespace EricksonLopez.Events.UnitTests.AttributesAndExceptions;

using AwesomeAssertions;
using EricksonLopez.Events.Attributes;
using EricksonLopez.Events.Exceptions;
using EricksonLopez.Events.Identifiers;
using Xunit;

[Xunit.Trait("Category", "Unit")]
public sealed class AttributesAndExceptionsTests
{
    [Fact]
    public void EventNameAttribute_ValidName_ShouldInitializeAndConvert()
    {
        var attr = new EventNameAttribute("  orders.order-created  ");

        attr.Name.Should().Be("orders.order-created");
        attr.AsEventType().Should().Be(EventType.From("orders.order-created"));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void EventNameAttribute_InvalidName_ShouldThrow(string? invalid)
    {
        Action act = () => new EventNameAttribute(invalid!);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void EventVersionAttribute_ValidVersion_ShouldInitializeAndConvert()
    {
        var attr = new EventVersionAttribute(2u);

        attr.Version.Should().Be(2u);
        attr.AsEventVersion().Should().Be(EventVersion.From(2u));
    }

    [Fact]
    public void EventVersionAttribute_ZeroVersion_ShouldThrow()
    {
        Action act = () => new EventVersionAttribute(0u);
        act.Should().Throw<ArgumentOutOfRangeException>()
            .WithMessage("*Version must be greater than or equal to 1.*");
    }

    [Fact]
    public void EventSourceAttribute_ValidSource_ShouldInitialize()
    {
        var attr = new EventSourceAttribute("  identity-service  ");

        attr.Source.Should().Be("identity-service");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void EventSourceAttribute_InvalidSource_ShouldThrow(string? invalid)
    {
        Action act = () => new EventSourceAttribute(invalid!);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void EventValidationException_ShouldInitializeWithMessageAndInner()
    {
        var ex1 = new EventValidationException("Invalid event payload");
        ex1.Message.Should().Be("Invalid event payload");

        var inner = new InvalidOperationException("Root cause");
        var ex2 = new EventValidationException("Invalid event with inner", inner);
        ex2.Message.Should().Be("Invalid event with inner");
        ex2.InnerException.Should().BeSameAs(inner);
    }

    [Fact]
    public void EventTypeNotFoundException_ShouldInitializeWithEventType()
    {
        var eventType = EventType.From("orders.missing");
        var ex = new EventTypeNotFoundException(eventType);

        ex.EventType.Should().Be(eventType);
        ex.Message.Should().Contain("orders.missing");
    }
}




// Copyright © Erickson Lopez. MIT License.
using System;
using System.Collections.Generic;
using System.Threading;

namespace EricksonLopez.Events.UnitTests.Bus;

using AwesomeAssertions;
using EricksonLopez.Events.Bus.Exceptions;
using Xunit;

[Xunit.Trait("Category", "Unit")]
public sealed class EventDispatchExceptionTests
{
    private sealed record TestSampleEvent;

    [Fact]
    public void Constructor_WithDefaultMessage_ShouldInitializePropertiesCorrectly()
    {
        var innerExceptions = new List<Exception>
        {
            new InvalidOperationException("First error"),
            new ArgumentException("Second error")
        };

        var ex = new EventDispatchException(typeof(TestSampleEvent), innerExceptions);

        ex.EventType.Should().Be(typeof(TestSampleEvent));
        ex.Message.Should().StartWith($"One or more handlers failed while dispatching event '{nameof(TestSampleEvent)}'.");
        ex.InnerExceptions.Should().HaveCount(2);
        ex.InnerExceptions[0].Should().BeOfType<InvalidOperationException>();
        ex.InnerExceptions[1].Should().BeOfType<ArgumentException>();
        ex.Should().BeAssignableTo<AggregateException>();
    }

    [Fact]
    public void Constructor_WithCustomMessage_ShouldInitializePropertiesCorrectly()
    {
        var innerExceptions = new List<Exception>
        {
            new TimeoutException("Timeout occurred")
        };
        const string customMessage = "Custom dispatch failure message";

        var ex = new EventDispatchException(typeof(TestSampleEvent), customMessage, innerExceptions);

        ex.EventType.Should().Be(typeof(TestSampleEvent));
        ex.Message.Should().StartWith(customMessage);
        ex.InnerExceptions.Should().HaveCount(1);
        ex.InnerExceptions[0].Should().BeOfType<TimeoutException>();
        ex.Should().BeAssignableTo<AggregateException>();
    }
}





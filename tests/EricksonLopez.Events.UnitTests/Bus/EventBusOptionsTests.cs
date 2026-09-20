// Copyright © Erickson Lopez. MIT License.
using System;
using System.Threading;
using System.Threading.Tasks;

namespace EricksonLopez.Events.UnitTests.Bus;

using AwesomeAssertions;
using EricksonLopez.Events.Bus.Configuration;
using Xunit;

[Xunit.Trait("Category", "Unit")]
public sealed class EventBusOptionsTests
{
    [Fact]
    public void EventBusOptions_DefaultValues_ShouldBeCorrect()
    {
        var options = new EventBusOptions();

        options.ExecutionMode.Should().Be(EventExecutionMode.Sequential);
        options.ErrorPolicy.Should().Be(ErrorHandlingPolicy.FailFast);
        options.ThrowOnUnregisteredEvent.Should().BeFalse();
        options.MaxReentrancyDepth.Should().Be(10);
    }

    [Fact]
    public void EventBusOptions_CustomValues_ShouldSetCorrectly()
    {
        var options = new EventBusOptions
        {
            ExecutionMode = EventExecutionMode.Parallel,
            ErrorPolicy = ErrorHandlingPolicy.AggregateAndContinue,
            ThrowOnUnregisteredEvent = true,
            MaxReentrancyDepth = 42
        };

        options.ExecutionMode.Should().Be(EventExecutionMode.Parallel);
        options.ErrorPolicy.Should().Be(ErrorHandlingPolicy.AggregateAndContinue);
        options.ThrowOnUnregisteredEvent.Should().BeTrue();
        options.MaxReentrancyDepth.Should().Be(42);
    }

    [Fact]
    public void ErrorHandlingPolicy_EnumValues_ShouldMatchContract()
    {
        ((int)ErrorHandlingPolicy.FailFast).Should().Be(0);
        ((int)ErrorHandlingPolicy.AggregateAndContinue).Should().Be(1);
    }

    [Fact]
    public void EventExecutionMode_EnumValues_ShouldMatchContract()
    {
        ((int)EventExecutionMode.Sequential).Should().Be(0);
        ((int)EventExecutionMode.Parallel).Should().Be(1);
    }

    [Fact]
    public void Validate_WhenDefaultOptions_ShouldNotThrow()
    {
        var options = new EventBusOptions();
        var act = () => options.Validate();
        act.Should().NotThrow();
    }

    [Fact]
    public void Validate_WhenMaxDegreeOfParallelismNegative_ShouldThrowInvalidOperationException()
    {
        var options = new EventBusOptions
        {
            MaxDegreeOfParallelism = -1
        };

        var act = () => options.Validate();
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*cannot be negative*");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Validate_WhenMaxReentrancyDepthZeroOrNegative_ShouldThrowInvalidOperationException(int invalidDepth)
    {
        var options = new EventBusOptions
        {
            MaxReentrancyDepth = invalidDepth
        };

        var act = () => options.Validate();
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*must be greater than zero*");
    }
}





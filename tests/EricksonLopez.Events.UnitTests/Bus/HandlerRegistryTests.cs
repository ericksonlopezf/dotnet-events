// Copyright © Erickson Lopez. MIT License.
using System;
using System.Threading;
using System.Threading.Tasks;

namespace EricksonLopez.Events.UnitTests.Bus;

using EricksonLopez.Events.Bus.Registry;
using AwesomeAssertions;
using Xunit;

[Xunit.Trait("Category", "Unit")]
public sealed class HandlerRegistryTests
{
    private sealed record TestEventA;
    private sealed record TestEventB;
    private sealed class HandlerA;
    private sealed class HandlerB;

    [Theory]
    [InlineData(0, "handlerType")]
    [InlineData(1, "serviceType")]
    [InlineData(2, "invoker")]
    public void HandlerDescriptor_Constructor_WithNullArguments_ShouldThrowArgumentNullException(int nullArgIndex, string expectedParamName)
    {
        var invoker = new Func<object, object, CancellationToken, ValueTask>((_, _, _) => ValueTask.CompletedTask);

        Action act = nullArgIndex switch
        {
            0 => () => { _ = new HandlerDescriptor(null!, typeof(object), invoker); },
            1 => () => { _ = new HandlerDescriptor(typeof(HandlerA), null!, invoker); },
            _ => () => { _ = new HandlerDescriptor(typeof(HandlerA), typeof(object), null!); }
        };

        act.Should().Throw<ArgumentNullException>().WithParameterName(expectedParamName);
    }

    [Fact]
    public async Task HandlerDescriptor_Constructor_WithValidArguments_ShouldSetProperties()
    {
        bool invoked = false;
        var invoker = new Func<object, object, CancellationToken, ValueTask>((_, _, _) =>
        {
            invoked = true;
            return ValueTask.CompletedTask;
        });

        var descriptor = new HandlerDescriptor(typeof(HandlerA), typeof(object), invoker);

        descriptor.HandlerType.Should().Be(typeof(HandlerA));
        descriptor.ServiceType.Should().Be(typeof(object));
        descriptor.Invoker.Should().NotBeNull();

        await descriptor.Invoker(new HandlerA(), new TestEventA(), CancellationToken.None);
        invoked.Should().BeTrue();
    }

    [Theory]
    [InlineData(0, "eventType")]
    [InlineData(1, "descriptor")]
    public void HandlerRegistry_Register_WithNullArguments_ShouldThrowArgumentNullException(int nullArgIndex, string expectedParamName)
    {
        var registry = new HandlerRegistry();
        var descriptor = new HandlerDescriptor(typeof(HandlerA), typeof(object), (_, _, _) => ValueTask.CompletedTask);

        Action act = nullArgIndex switch
        {
            0 => () => registry.Register(null!, descriptor),
            _ => () => registry.Register(typeof(TestEventA), null!)
        };

        act.Should().Throw<ArgumentNullException>().WithParameterName(expectedParamName);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void HandlerRegistry_GetHandlersAndHasHandlers_WithNull_ShouldThrowArgumentNullException(bool isGetHandlers)
    {
        var registry = new HandlerRegistry();

        Action act = isGetHandlers ? () => registry.GetHandlers(null!) : () => registry.HasHandlers(null!);

        act.Should().Throw<ArgumentNullException>().WithParameterName("eventType");
    }

    [Fact]
    public void HandlerRegistry_UnregisteredEventType_ShouldReturnEmptyAndFalse()
    {
        var registry = new HandlerRegistry();

        registry.HasHandlers(typeof(TestEventA)).Should().BeFalse();
        registry.GetHandlers(typeof(TestEventA)).Should().BeEmpty();
    }

    [Fact]
    public void HandlerRegistry_Register_SingleAndMultipleHandlers_ShouldMaintainOrderAndCount()
    {
        var registry = new HandlerRegistry();
        var desc1 = new HandlerDescriptor(typeof(HandlerA), typeof(object), (_, _, _) => ValueTask.CompletedTask);
        var desc2 = new HandlerDescriptor(typeof(HandlerB), typeof(object), (_, _, _) => ValueTask.CompletedTask);

        registry.Register(typeof(TestEventA), desc1);

        registry.HasHandlers(typeof(TestEventA)).Should().BeTrue();
        var handlers1 = registry.GetHandlers(typeof(TestEventA));
        handlers1.Should().HaveCount(1);
        handlers1[0].Should().BeSameAs(desc1);

        registry.Register(typeof(TestEventA), desc2);

        var handlers2 = registry.GetHandlers(typeof(TestEventA));
        handlers2.Should().HaveCount(2);
        handlers2[0].Should().BeSameAs(desc1);
        handlers2[1].Should().BeSameAs(desc2);

        // Another event remains unregistered
        registry.HasHandlers(typeof(TestEventB)).Should().BeFalse();
        registry.GetHandlers(typeof(TestEventB)).Should().BeEmpty();
    }
}





// Copyright © Erickson Lopez. MIT License.
using System;
using System.Threading;
using System.Threading.Tasks;
using AwesomeAssertions;
using EricksonLopez.Events.Bus;
using EricksonLopez.Events.Bus.Configuration;
using EricksonLopez.Events.Bus.Extensions;
using EricksonLopez.Events.Contracts;
using EricksonLopez.Events.Dispatch;
using EricksonLopez.Events.Identifiers;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace EricksonLopez.Events.UnitTests.Adversarial.Reliability;

[Trait("Category", "Reliability")]
public sealed class PublisherReentrancyTests
{
    private sealed record RecursiveEventA(EventId Id, DateTimeOffset OccurredAt) : IEvent;
    private sealed record RecursiveEventB(EventId Id, DateTimeOffset OccurredAt) : IEvent;

    [Fact]
    public async Task EventBus_WhenReentrancyExceedsMaxDepth_ThrowsInvalidOperationException()
    {
        // Verifies that EventBus reentrancy protection works
        var services = new ServiceCollection();
        services.AddEventBus(opts => opts.MaxReentrancyDepth = 3);
        services.AddEventHandler<RecursiveEventA, RecursiveHandlerA>();
        services.AddEventHandler<RecursiveEventB, RecursiveHandlerB>();

        var sp = services.BuildServiceProvider();
        var bus = sp.GetRequiredService<IEventBus>();

        Func<Task> act = async () => await bus.PublishAsync(new RecursiveEventA(EventId.New(), DateTimeOffset.UtcNow));

        var ex = await act.Should().ThrowAsync<InvalidOperationException>();
        ex.WithMessage("*maximum reentrancy depth limit (3) exceeded*");
    }

    private sealed class RecursiveHandlerA : IEventHandler<RecursiveEventA>
    {
        private readonly IEventBus _bus;
        public RecursiveHandlerA(IEventBus bus) => _bus = bus;

        public async ValueTask HandleAsync(RecursiveEventA @event, CancellationToken cancellationToken = default) =>
            await _bus.PublishAsync(new RecursiveEventB(EventId.New(), DateTimeOffset.UtcNow), cancellationToken);
    }

    private sealed class RecursiveHandlerB : IEventHandler<RecursiveEventB>
    {
        private readonly IEventBus _bus;
        public RecursiveHandlerB(IEventBus bus) => _bus = bus;

        public async ValueTask HandleAsync(RecursiveEventB @event, CancellationToken cancellationToken = default) =>
            await _bus.PublishAsync(new RecursiveEventA(EventId.New(), DateTimeOffset.UtcNow), cancellationToken);
    }

    [Fact]
    public async Task InMemoryEventPublisher_WhenReentrancyExceedsMaxDepth_ThrowsInvalidOperationException()
    {
        // Validates EVT-REL-002 Remediation:
        // InMemoryEventPublisher now enforces MaxReentrancyDepth using AsyncLocal<int>,
        // preventing fatal StackOverflowException on cyclic event publishing.
        var publisher = new InMemoryEventPublisher { MaxReentrancyDepth = 3 };
        int executionCount = 0;

        var handler = new SimpleReentrantHandler(publisher, () => executionCount++);
        publisher.Subscribe(handler);

        Func<Task> act = async () => await publisher.PublishAsync(new RecursiveEventA(EventId.New(), DateTimeOffset.UtcNow));

        var ex = await act.Should().ThrowAsync<InvalidOperationException>();
        ex.WithMessage("*maximum reentrancy depth limit (3) exceeded*");
    }

    private sealed class SimpleReentrantHandler : IEventHandler<RecursiveEventA>
    {
        private readonly InMemoryEventPublisher _pub;
        private readonly Action _onExecuted;
        private int _calls;

        public SimpleReentrantHandler(InMemoryEventPublisher pub, Action onExecuted)
        {
            _pub = pub;
            _onExecuted = onExecuted;
        }

        public async ValueTask HandleAsync(RecursiveEventA @event, CancellationToken cancellationToken = default)
        {
            _onExecuted();
            if (++_calls < 5)
            {
                await _pub.PublishAsync(new RecursiveEventA(EventId.New(), DateTimeOffset.UtcNow), cancellationToken);
            }
        }
    }
}

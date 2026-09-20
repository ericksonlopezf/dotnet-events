// Copyright © Erickson Lopez. MIT License.
using System;
using System.Threading;
using System.Threading.Tasks;
using AwesomeAssertions;
using EricksonLopez.Events.Bus;
using EricksonLopez.Events.Bus.Configuration;
using EricksonLopez.Events.Bus.Extensions;
using EricksonLopez.Events.Contracts;
using EricksonLopez.Events.Identifiers;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace EricksonLopez.Events.UnitTests.Adversarial.Reliability;

[Trait("Category", "Adversarial")]
public sealed class ReentrancyLeakOnUnregisteredEventTests
{
    public sealed record ValidEvent(EventId Id, DateTimeOffset OccurredAt) : IEvent;
    public sealed record UnregisteredEvent(EventId Id, DateTimeOffset OccurredAt) : IEvent;

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Naming", "CA1711:Identifiers should not have incorrect suffix", Justification = "Test consumer")]
    public sealed class ValidEventConsumer : IEventHandler<ValidEvent>
    {
        public static bool Handled { get; set; }

        public ValueTask HandleAsync(ValidEvent @event, CancellationToken cancellationToken = default)
        {
            Handled = true;
            return ValueTask.CompletedTask;
        }
    }

    [Fact]
    public async Task WhenUnregisteredEventThrows_ReentrancyDepthMustNotLeakIntoAsyncContext()
    {
        ValidEventConsumer.Handled = false;

        var services = new ServiceCollection();
        services.AddEventBus(opts =>
        {
            opts.ThrowOnUnregisteredEvent = true;
            opts.MaxReentrancyDepth = 3;
        });
        services.AddEventHandler<ValidEvent, ValidEventConsumer>();

        var sp = services.BuildServiceProvider();
        var bus = sp.GetRequiredService<IEventBus>();

        // Publish unregistered event 3 times, catching the expected exception
        for (int i = 0; i < 3; i++)
        {
            try
            {
                await bus.PublishAsync(new UnregisteredEvent(EventId.New(), DateTimeOffset.UtcNow));
            }
            catch (InvalidOperationException)
            {
                // Expected: No handlers registered for event
            }
        }

        // In buggy code:
        // ReentrancyDepth was incremented by 1 on each call and never reset when ThrowOnUnregisteredEvent threw.
        // So currentDepth is now 3.
        // Publishing a valid registered event now throws:
        // "EventBus maximum reentrancy depth limit (3) exceeded while publishing 'ValidEvent'. Possible cyclic event cascade."
        // instead of successfully executing the handler!
        Func<Task> act = async () => await bus.PublishAsync(new ValidEvent(EventId.New(), DateTimeOffset.UtcNow));

        await act.Should().NotThrowAsync("Reentrancy depth must be restored in a finally block even when ThrowOnUnregisteredEvent throws.");
        ValidEventConsumer.Handled.Should().BeTrue("Valid registered handler must execute successfully.");
    }
}

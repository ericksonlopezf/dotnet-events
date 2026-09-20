// Copyright © Erickson Lopez. MIT License.
using System;
using System.Threading;
using System.Threading.Tasks;
using AwesomeAssertions;
using EricksonLopez.Events.Contracts;
using EricksonLopez.Events.Dispatch;
using EricksonLopez.Events.Identifiers;
using Xunit;

namespace EricksonLopez.Events.UnitTests.Adversarial.Api;

[Trait("Category", "Adversarial")]
[Trait("Vulnerability", "EVT-CON-001")]
public sealed class AdversarialApiMisuseTests
{
    public sealed record MisuseTestEvent(EventId Id, DateTimeOffset OccurredAt) : IEvent;

    public sealed class CountingTestHandler : IEventHandler<MisuseTestEvent>
    {
        public int InvocationCount { get; private set; }

        public ValueTask HandleAsync(MisuseTestEvent eventInstance, CancellationToken cancellationToken = default)
        {
            InvocationCount++;
            return ValueTask.CompletedTask;
        }
    }

    [Fact]
    public async Task InMemoryEventPublisher_DuplicateSubscribe_DeduplicatesRegistrations_AndInvokesOnce()
    {
        // Validates EVT-CON-001 Remediation:
        // InMemoryEventPublisher deduplicates identical subscriptions.
        var publisher = new InMemoryEventPublisher();
        var handler = new CountingTestHandler();

        // Accidental duplicate subscription by a misconfigured consumer
        publisher.Subscribe(handler);
        publisher.Subscribe(handler);

        await publisher.PublishAsync(new MisuseTestEvent(EventId.New(), DateTimeOffset.UtcNow));

        // In remediated code: InvocationCount is 1 because duplicate subscriptions are ignored
        handler.InvocationCount.Should().Be(1,
            "EVT-CON-001 Remediation: Duplicate subscription was deduplicated, ensuring exactly-once execution per instance.");
    }

    [Fact]
    public void EventVersion_ZeroOrNegative_ThrowsArgumentOutOfRangeException()
    {
        Action actZero = () => EventVersion.From(0);
        actZero.Should().Throw<ArgumentOutOfRangeException>();

        Action actNegative = () => EventVersion.From(-1);
        actNegative.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void EventType_EmptyOrWhitespace_ThrowsArgumentException()
    {
        Action actNull = () => new EventType(null!);
        actNull.Should().Throw<ArgumentException>();

        Action actEmpty = () => new EventType("");
        actEmpty.Should().Throw<ArgumentException>();

        Action actWhitespace = () => new EventType("   ");
        actWhitespace.Should().Throw<ArgumentException>();
    }
}

// Copyright © Erickson Lopez. MIT License.
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AwesomeAssertions;
using EricksonLopez.Events.Bus;
using EricksonLopez.Events.Bus.Configuration;
using EricksonLopez.Events.Bus.Extensions;
using EricksonLopez.Events.Contracts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;

// Alias to avoid ambiguity with Microsoft.Extensions.Logging.EventId
using DomainEventId = EricksonLopez.Events.Identifiers.EventId;

namespace EricksonLopez.Events.UnitTests.Bus;

[Trait("Category", "Unit")]
public sealed class EventBusExceptionLoggingTests
{
    private sealed record LogTestEvent(DomainEventId Id, DateTimeOffset OccurredAt) : IEvent;

    private sealed class FakeLogger<T> : ILogger<T>
    {
        public string LastLoggedMessage { get; private set; } = string.Empty;

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, Microsoft.Extensions.Logging.EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            LastLoggedMessage = formatter(state, exception);
        }
    }

    private sealed class FailingHandler1 : IEventHandler<LogTestEvent>
    {
        public ValueTask HandleAsync(LogTestEvent eventInstance, CancellationToken cancellationToken = default)
        {
            throw new AggregateException(
                new InvalidOperationException("First handler failed."),
                new ArgumentException("Second handler failed."));
        }
    }

    [Fact]
    public async Task EVT_REL_001_PublishAsync_WhenAggregateExceptionIsThrown_LogsFlattenedInnerExceptions()
    {
        // Verified Remediation for EVT-REL-001:
        // AggregateExceptions (like those from ParallelExecutionStrategy) are flattened before logging,
        // so that the underlying handler exceptions are visible in the logs rather than masked.

        var services = new ServiceCollection();

        var fakeLogger = new FakeLogger<EventBus>();
        services.AddSingleton<ILogger<EventBus>>(fakeLogger);

        services.AddEventBus(options =>
        {
            options.ExecutionMode = EventExecutionMode.Parallel;
        });

        services.AddEventHandler<LogTestEvent, FailingHandler1>(ServiceLifetime.Singleton);

        var sp = services.BuildServiceProvider();
        var bus = sp.GetRequiredService<IEventBus>();

        var evt = new LogTestEvent(DomainEventId.New(), DateTimeOffset.UtcNow);

        // Act
        var act = async () => await bus.PublishAsync(evt);
        var thrownEx = await act.Should().ThrowAsync<AggregateException>();

        // Assert
        // The logger should have been called with a message containing flattened exceptions
        fakeLogger.LastLoggedMessage.Should().Contain("First handler failed.", "EVT-REL-001: Flattened exceptions must be logged");
        fakeLogger.LastLoggedMessage.Should().Contain("Second handler failed.", "EVT-REL-001: Flattened exceptions must be logged");
    }
}

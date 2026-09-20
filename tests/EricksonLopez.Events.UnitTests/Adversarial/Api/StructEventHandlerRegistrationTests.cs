// Copyright © Erickson Lopez. MIT License.
using System;
using System.Threading;
using System.Threading.Tasks;
using AwesomeAssertions;
using EricksonLopez.Events.Bus;
using EricksonLopez.Events.Bus.Extensions;
using EricksonLopez.Events.Contracts;
using EricksonLopez.Events.Identifiers;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace EricksonLopez.Events.UnitTests.Adversarial.Api;

[Trait("Category", "Adversarial")]
public sealed class StructEventHandlerRegistrationTests
{
    public readonly record struct ApiStructEvent(EventId Id, DateTimeOffset OccurredAt, string Code) : IEvent;

    public sealed class ApiStructHandler : IEventHandler<ApiStructEvent>
    {
        public static string? HandledCode { get; set; }

        public ValueTask HandleAsync(ApiStructEvent eventInstance, CancellationToken cancellationToken = default)
        {
            HandledCode = eventInstance.Code;
            return ValueTask.CompletedTask;
        }

        public static void Reset() => HandledCode = null;
    }

    [Fact]
    public async Task AddEventHandler_WithStructEvent_ShouldRegisterAndDispatchSuccessfully()
    {
        // Validates EVT-API-001 Remediation:
        // Removing 'where TEvent : class' allows value-type / struct events to be registered directly with DI.
        ApiStructHandler.Reset();

        var services = new ServiceCollection();
        services.AddEventBus();
        services.AddEventHandler<ApiStructEvent, ApiStructHandler>();

        var sp = services.BuildServiceProvider();
        var bus = sp.GetRequiredService<IEventBus>();

        var evt = new ApiStructEvent(EventId.New(), DateTimeOffset.UtcNow, "STRUCT_API_SUCCESS");
        await bus.PublishAsync(evt);

        ApiStructHandler.HandledCode.Should().Be("STRUCT_API_SUCCESS",
            "Struct events must be registrable via AddEventHandler and invoked correctly.");
    }
}

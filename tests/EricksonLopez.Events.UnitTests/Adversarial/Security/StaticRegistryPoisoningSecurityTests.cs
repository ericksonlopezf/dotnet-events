// Copyright © Erickson Lopez. MIT License.
using System;
using AwesomeAssertions;
using EricksonLopez.Events.Contracts;
using EricksonLopez.Events.Identifiers;
using EricksonLopez.Events.Registry;
using Xunit;

namespace EricksonLopez.Events.UnitTests.Adversarial.Security;

[Collection("DiagnosticsAndStaticRegistry")]
[Trait("Category", "Adversarial")]
public sealed class StaticRegistryPoisoningSecurityTests : IDisposable
{
    private sealed record SecurityTestEvent(EventId Id, DateTimeOffset OccurredAt) : IEvent;

    public StaticRegistryPoisoningSecurityTests()
    {
        StaticEventTypeRegistry.Reset();
    }

    public void Dispose()
    {
        StaticEventTypeRegistry.Reset();
    }

    [Fact]
    public void SetCurrent_WhenCalledOnce_ShouldInitializeSuccessfully()
    {
        var descriptor = new EventTypeDescriptor(
            typeof(SecurityTestEvent),
            EventType.From("security.test.event"),
            EventVersion.V1);

        var registry = new EventTypeRegistry(new[] { descriptor });

        StaticEventTypeRegistry.SetCurrent(registry);

        StaticEventTypeRegistry.Current.Should().BeSameAs(registry);
        StaticEventTypeRegistry.GetEventType<SecurityTestEvent>().Value.Should().Be("security.test.event");
    }

    [Fact]
    public void SetCurrent_WhenCalledTwiceWithoutAllowOverride_ShouldThrowInvalidOperationException()
    {
        var reg1 = new EventTypeRegistry(Array.Empty<EventTypeDescriptor>());
        var reg2 = new EventTypeRegistry(Array.Empty<EventTypeDescriptor>());

        StaticEventTypeRegistry.SetCurrent(reg1);

        Action act = () => StaticEventTypeRegistry.SetCurrent(reg2, allowOverride: false);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*StaticEventTypeRegistry is already initialized*");
    }

    [Fact]
    public void CurrentPropertySetter_WhenRegistryAlreadyInitialized_ShouldThrowInvalidOperationException()
    {
        var reg1 = new EventTypeRegistry(Array.Empty<EventTypeDescriptor>());
        var rogueReg = new EventTypeRegistry(Array.Empty<EventTypeDescriptor>());

        StaticEventTypeRegistry.SetCurrent(reg1);

        Action act = () => StaticEventTypeRegistry.Current = rogueReg;

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*StaticEventTypeRegistry is already initialized*");
    }

    [Fact]
    public void SetCurrent_WithAllowOverrideTrue_ShouldPermitAuthorizedReconfiguration()
    {
        var reg1 = new EventTypeRegistry(Array.Empty<EventTypeDescriptor>());
        var reg2 = new EventTypeRegistry(Array.Empty<EventTypeDescriptor>());

        StaticEventTypeRegistry.SetCurrent(reg1);
        StaticEventTypeRegistry.SetCurrent(reg2, allowOverride: true);

        StaticEventTypeRegistry.Current.Should().BeSameAs(reg2);
    }
}

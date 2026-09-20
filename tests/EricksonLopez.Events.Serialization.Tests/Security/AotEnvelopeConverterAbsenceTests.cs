// Copyright © Erickson Lopez. MIT License.
using System;
using System.Text.Json;
using AwesomeAssertions;
using EricksonLopez.Events.Contracts;
using EricksonLopez.Events.Envelopes;
using EricksonLopez.Events.Identifiers;
using EricksonLopez.Events.Serialization.SystemTextJson;
using EricksonLopez.Events.Serialization.SystemTextJson.Converters;
using Xunit;

namespace EricksonLopez.Events.Serialization.Tests.Security;

[Trait("Category", "Security")]
public sealed class AotEnvelopeConverterAbsenceTests
{
    private sealed record UnregisteredPayload(EventId Id, DateTimeOffset OccurredAt, string Note) : IEvent;

    [Fact]
    public void EVT_AOT_001_AddEventsConverters_IncludesConverterForEventEnvelope()
    {
        // Validates EVT-AOT-001 Remediation:
        // AddEventsConverters now registers EventEnvelopeJsonConverterFactory for EventEnvelope<TEvent>,
        // ensuring Native AOT serialization does not fall back to unsafe reflection converter.

        var options = new JsonSerializerOptions();
        options.AddEventsConverters();

        var converter = options.GetConverter(typeof(EventEnvelope<UnregisteredPayload>));

        converter.Should().NotBeNull();
        converter.Should().BeOfType<EventEnvelopeJsonConverter<UnregisteredPayload>>(
            "EVT-AOT-001: AddEventsConverters now provides EventEnvelopeJsonConverterFactory for EventEnvelope<T>.");
    }
}

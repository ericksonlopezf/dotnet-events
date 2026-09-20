// Copyright © Erickson Lopez. MIT License.
using System;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;
using EricksonLopez.Events.Contracts;
using EricksonLopez.Events.Envelopes;

namespace EricksonLopez.Events.Serialization.SystemTextJson.Converters;

/// <summary>
/// Provides a <see cref="JsonConverterFactory"/> for constructing <see cref="EventEnvelopeJsonConverter{TEvent}"/>
/// instances for generic <see cref="EventEnvelope{TEvent}"/> types.
/// </summary>
public sealed class EventEnvelopeJsonConverterFactory : JsonConverterFactory
{
    /// <inheritdoc />
    public override bool CanConvert(Type typeToConvert)
    {
        ArgumentNullException.ThrowIfNull(typeToConvert);

        if (!typeToConvert.IsGenericType)
        {
            return false;
        }

        var genericTypeDefinition = typeToConvert.GetGenericTypeDefinition();
        if (genericTypeDefinition != typeof(EventEnvelope<>) && genericTypeDefinition != typeof(IEventEnvelope<>))
        {
            return false;
        }

        var eventType = typeToConvert.GetGenericArguments()[0];
        return typeof(IEvent).IsAssignableFrom(eventType);
    }

    /// <inheritdoc />
    [UnconditionalSuppressMessage("Trimming", "IL2026", Justification = "Fallback factory for EventEnvelope<TEvent> when source generation is not used.")]
    [UnconditionalSuppressMessage("AOT", "IL3050", Justification = "Fallback factory for EventEnvelope<TEvent> when source generation is not used.")]
    public override JsonConverter? CreateConverter(
        Type typeToConvert,
        JsonSerializerOptions options)
    {
        ArgumentNullException.ThrowIfNull(typeToConvert);
        ArgumentNullException.ThrowIfNull(options);

        var eventType = typeToConvert.GetGenericArguments()[0];
        var converterType = typeof(EventEnvelopeJsonConverter<>).MakeGenericType(eventType);

        return (JsonConverter?)Activator.CreateInstance(converterType);
    }
}

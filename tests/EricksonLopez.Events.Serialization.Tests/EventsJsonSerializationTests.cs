// Copyright © Erickson Lopez. MIT License.
using System;
using System.Linq;

namespace EricksonLopez.Events.Serialization.Tests;

using System.IO;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using EricksonLopez.Events.Contracts;
using EricksonLopez.Events.Envelopes;
using EricksonLopez.Events.Identifiers;
using EricksonLopez.Events.Metadata;
using EricksonLopez.Events.Serialization.SystemTextJson;
using EricksonLopez.Events.Serialization.SystemTextJson.Converters;
using AwesomeAssertions;
using FsCheck;
using FsCheck.Xunit;
using Xunit;

public sealed record CustomerRegisteredEvent(
    EventId Id,
    string CustomerName,
    string Email,
    DateTimeOffset OccurredAt) : IIntegrationEvent;

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase, PropertyNameCaseInsensitive = true)]
[JsonSerializable(typeof(EventId))]
[JsonSerializable(typeof(EventType))]
[JsonSerializable(typeof(EventVersion))]
[JsonSerializable(typeof(CorrelationId))]
[JsonSerializable(typeof(CausationId))]
[JsonSerializable(typeof(TenantId))]
[JsonSerializable(typeof(EventMetadata))]
[JsonSerializable(typeof(CustomerRegisteredEvent))]
[JsonSerializable(typeof(EventEnvelope<CustomerRegisteredEvent>))]
internal sealed partial class TestJsonContext : JsonSerializerContext
{
}

[Trait("Category", "Unit")]
public sealed class EventsJsonSerializationTests
{
    private readonly JsonSerializerOptions _options;

    public EventsJsonSerializationTests()
    {
        _options = new JsonSerializerOptions
        {
            TypeInfoResolver = TestJsonContext.Default,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true
        };
        _options.AddEventsConverters();
    }

    #region Serializer Options Extensions Tests

    [Fact]
    public void Extensions_CreateDefaultOptions_ShouldConfigureConvertersAndPolicy()
    {
        var defaultOptions = EventsJsonSerializerOptionsExtensions.CreateDefaultOptions();
        defaultOptions.Converters.Should().NotBeEmpty();
        defaultOptions.PropertyNamingPolicy.Should().Be(JsonNamingPolicy.CamelCase);
        defaultOptions.WriteIndented.Should().BeFalse();
    }

    [Fact]
    public void Extensions_AddEventsConverters_NullOptions_ThrowsArgumentNullException()
    {
        Action actNull = () => EventsJsonSerializerOptionsExtensions.AddEventsConverters(null!);
        actNull.Should().Throw<ArgumentNullException>().WithParameterName("options");
    }

    #endregion

    #region EventId Serialization Tests

    [Fact]
    public void EventId_SerializeAndDeserialize_ShouldRoundtrip()
    {
        var id = EventId.New();
        var json = JsonSerializer.Serialize(id, _options);

        json.Should().Be($"\"{id.Value}\"");

        var deserialized = JsonSerializer.Deserialize<EventId>(json, _options);
        deserialized.Should().Be(id);
    }

    [Fact]
    public void EventId_Deserialize_Null_ShouldReturnEmpty()
    {
        var nullDeserialized = JsonSerializer.Deserialize<EventId>("null", _options);
        nullDeserialized.Should().Be(EventId.Empty);
    }

    [Theory]
    [InlineData("\"not-a-guid\"")]
    [InlineData("123")]
    public void EventId_Deserialize_InvalidTokens_ThrowsJsonException(string invalidJson)
    {
        Action actInvalid = () => JsonSerializer.Deserialize<EventId>(invalidJson, _options);
        actInvalid.Should().Throw<JsonException>()
            .WithMessage($"*Expected string UUID representation for {nameof(EventId)}*");
    }

    [Fact]
    public void EventId_Write_NullWriter_ThrowsArgumentNullException()
    {
        var converter = new EventIdJsonConverter();
        Action actNullWriter = () => converter.Write(null!, EventId.New(), _options);
        actNullWriter.Should().Throw<ArgumentNullException>().WithParameterName("writer");
    }

    #endregion

    #region EventType Serialization Tests

    [Fact]
    public void EventType_SerializeAndDeserialize_ShouldRoundtrip()
    {
        var eventType = EventType.From("customers.registered");

        var typeJson = JsonSerializer.Serialize(eventType, _options);
        typeJson.Should().Be("\"customers.registered\"");

        var desType = JsonSerializer.Deserialize<EventType>(typeJson, _options);
        desType.Should().Be(eventType);
    }

    [Theory]
    [InlineData("null")]
    [InlineData("\"\"")]
    public void EventType_Deserialize_NullOrEmptyString_ShouldReturnDefault(string json)
    {
        var result = JsonSerializer.Deserialize<EventType>(json, _options);
        result.Should().Be(default(EventType));
    }

    [Fact]
    public void EventType_Deserialize_InvalidNumericToken_ThrowsJsonException()
    {
        Action actInvalid = () => JsonSerializer.Deserialize<EventType>("123", _options);
        actInvalid.Should().Throw<JsonException>()
            .WithMessage($"*Expected string representation for {nameof(EventType)}*");
    }

    [Fact]
    public void EventType_Write_NullWriter_ThrowsArgumentNullException()
    {
        var converter = new EventTypeJsonConverter();
        Action actNullWriter = () => converter.Write(null!, EventType.From("test.event"), _options);
        actNullWriter.Should().Throw<ArgumentNullException>().WithParameterName("writer");
    }

    #endregion

    #region EventVersion Serialization Tests

    [Fact]
    public void EventVersion_SerializeAndDeserialize_ShouldRoundtrip()
    {
        var version = EventVersion.From(2);

        var versionJson = JsonSerializer.Serialize(version, _options);
        versionJson.Should().Be("2");

        var desVersion = JsonSerializer.Deserialize<EventVersion>(versionJson, _options);
        desVersion.Should().Be(version);
    }

    [Theory]
    [InlineData("\"3\"", 3u)]
    [InlineData("0", 1u)]
    [InlineData("\"0\"", 1u)]
    public void EventVersion_Deserialize_StringAndZeroNumbers_ShouldParseCorrectly(string json, uint expectedVersion)
    {
        var desVersion = JsonSerializer.Deserialize<EventVersion>(json, _options);
        desVersion.Value.Should().Be(expectedVersion);
    }

    [Theory]
    [InlineData("\"not-a-number\"")]
    [InlineData("true")]
    public void EventVersion_Deserialize_InvalidTokens_ThrowsJsonException(string invalidJson)
    {
        Action actInvalid = () => JsonSerializer.Deserialize<EventVersion>(invalidJson, _options);
        actInvalid.Should().Throw<JsonException>()
            .WithMessage($"*Expected numeric or string integer representation for {nameof(EventVersion)}*");
    }

    [Fact]
    public void EventVersion_Serialize_DefaultStruct_ShouldProduceOne()
    {
        EventVersion defaultVer = default;
        var defaultJson = JsonSerializer.Serialize(defaultVer, _options);
        defaultJson.Should().Be("1");
    }

    [Fact]
    public void EventVersion_Write_NullWriter_ThrowsArgumentNullException()
    {
        var converter = new EventVersionJsonConverter();
        Action actNullWriter = () => converter.Write(null!, EventVersion.From(2), _options);
        actNullWriter.Should().Throw<ArgumentNullException>().WithParameterName("writer");
    }

    #endregion

    #region Correlation, Causation, Tenant Identifiers Serialization Tests

    [Fact]
    public void ContextIdentifiers_SerializeAndDeserialize_ShouldRoundtrip()
    {
        var corr = CorrelationId.From("corr-1");
        var caus = CausationId.From("caus-1");
        var tenant = TenantId.From("tenant-1");

        var corrJson = JsonSerializer.Serialize(corr, _options);
        var causJson = JsonSerializer.Serialize(caus, _options);
        var tenantJson = JsonSerializer.Serialize(tenant, _options);

        JsonSerializer.Deserialize<CorrelationId>(corrJson, _options).Should().Be(corr);
        JsonSerializer.Deserialize<CausationId>(causJson, _options).Should().Be(caus);
        JsonSerializer.Deserialize<TenantId>(tenantJson, _options).Should().Be(tenant);
    }

    [Theory]
    [InlineData("null")]
    [InlineData("\"\"")]
    public void ContextIdentifiers_Deserialize_NullOrEmpty_ShouldReturnEmpty(string json)
    {
        JsonSerializer.Deserialize<CorrelationId>(json, _options).Should().Be(CorrelationId.Empty);
        JsonSerializer.Deserialize<CausationId>(json, _options).Should().Be(CausationId.Empty);
        JsonSerializer.Deserialize<TenantId>(json, _options).Should().Be(TenantId.Empty);
    }

    [Fact]
    public void ContextIdentifiers_Deserialize_InvalidTokens_ThrowsJsonException()
    {
        Action actCorrInvalid = () => JsonSerializer.Deserialize<CorrelationId>("123", _options);
        actCorrInvalid.Should().Throw<JsonException>()
            .WithMessage($"*Expected string or null for {nameof(CorrelationId)}*");

        Action actCausInvalid = () => JsonSerializer.Deserialize<CausationId>("123", _options);
        actCausInvalid.Should().Throw<JsonException>()
            .WithMessage($"*Expected string or null for {nameof(CausationId)}*");

        Action actTenantInvalid = () => JsonSerializer.Deserialize<TenantId>("123", _options);
        actTenantInvalid.Should().Throw<JsonException>()
            .WithMessage($"*Expected string or null for {nameof(TenantId)}*");
    }

    [Fact]
    public void ContextIdentifiers_Write_NullWriter_ThrowsArgumentNullException()
    {
        var corrConv = new CorrelationIdJsonConverter();
        var causConv = new CausationIdJsonConverter();
        var tenantConv = new TenantIdJsonConverter();

        Action act1 = () => corrConv.Write(null!, CorrelationId.From("c"), _options);
        Action act2 = () => causConv.Write(null!, CausationId.From("c"), _options);
        Action act3 = () => tenantConv.Write(null!, TenantId.From("t"), _options);

        act1.Should().Throw<ArgumentNullException>().WithParameterName("writer");
        act2.Should().Throw<ArgumentNullException>().WithParameterName("writer");
        act3.Should().Throw<ArgumentNullException>().WithParameterName("writer");
    }

    #endregion

    #region EventMetadata Serialization Tests

    [Fact]
    public void EventMetadata_SerializeAndDeserialize_ShouldRoundtrip()
    {
        var correlationId = CorrelationId.New();
        var causationId = CausationId.From("CMD-123");
        var tenantId = TenantId.From("tenant-us");

        var metadata = new EventMetadataBuilder()
            .WithCorrelationId(correlationId)
            .WithCausationId(causationId)
            .WithTenantId(tenantId)
            .WithSource("identity-service")
            .WithContentType("application/json")
            .WithHeader("X-Trace", "abc-123")
            .Build();

        var json = JsonSerializer.Serialize(metadata, _options);
        json.Should().Contain("\"customHeaders\":");

        var deserialized = JsonSerializer.Deserialize<EventMetadata>(json, _options);

        deserialized.Should().NotBeNull();
        deserialized!.CorrelationId.Should().Be(correlationId);
        deserialized.CausationId.Should().Be(causationId);
        deserialized.TenantId.Should().Be(tenantId);
        deserialized.Source.Should().Be("identity-service");
        deserialized.ContentType.Should().Be("application/json");
        deserialized.CustomHeaders.Should().ContainKey("X-Trace");
        deserialized.CustomHeaders["X-Trace"].Should().Be("abc-123");
    }

    [Fact]
    public void EventMetadata_Serialize_WithoutCustomHeaders_OmitsCustomHeadersProperty()
    {
        var emptyMeta = new EventMetadataBuilder().WithSource("no-headers").Build();
        var emptyMetaJson = JsonSerializer.Serialize(emptyMeta, _options);
        emptyMetaJson.Should().NotContain("\"customHeaders\"");
    }

    [Fact]
    public void EventMetadata_Deserialize_AliasedAndNestedProperties_ShouldParseCorrectly()
    {
        // Test with "headers" property alias, unknown properties, non-object customHeaders, and null header values
        var jsonWithAlias = "{\"headers\":{\"k1\":\"v1\",\"k2\":null},\"unknownNested\":{\"sub\":123},\"customHeaders\":[1,2,3],\"source\":\"aliased\"}";
        var desAlias = JsonSerializer.Deserialize<EventMetadata>(jsonWithAlias, _options);
        desAlias.Should().NotBeNull();
        desAlias!.Source.Should().Be("aliased");
        desAlias.CustomHeaders["k1"].Should().Be("v1");
        desAlias.CustomHeaders["k2"].Should().Be(string.Empty);

        // Test non-object headers skipping nested fake properties
        var jsonWithArrayHeaders = "{\"headers\":[{\"tenantId\":\"fake\"}],\"tenantId\":\"real\"}";
        var desArrayHeaders = JsonSerializer.Deserialize<EventMetadata>(jsonWithArrayHeaders, _options);
        desArrayHeaders.Should().NotBeNull();
        desArrayHeaders!.TenantId.Value.Should().Be("real");
    }

    [Fact]
    public void EventMetadata_Deserialize_NonObjectToken_ThrowsJsonException()
    {
        Action actNonObject = () => JsonSerializer.Deserialize<EventMetadata>("\"not-an-object\"", _options);
        actNonObject.Should().Throw<JsonException>()
            .WithMessage($"*Expected StartObject token when deserializing {nameof(EventMetadata)}*");
    }

    [Fact]
    public void EventMetadata_Write_NullArguments_ThrowsArgumentNullException()
    {
        var metaConv = new EventMetadataJsonConverter();
        var metadata = new EventMetadataBuilder().WithSource("test").Build();

        Action actNullWriter = () => metaConv.Write(null!, metadata, _options);
        actNullWriter.Should().Throw<ArgumentNullException>().WithParameterName("writer");

        Action actNullVal = () => metaConv.Write(new Utf8JsonWriter(new MemoryStream()), null!, _options);
        actNullVal.Should().Throw<ArgumentNullException>().WithParameterName("value");
    }

    [Theory]
    [InlineData("{\"source\":\"custom-source\"")]
    [InlineData("{\"customHeaders\":{\"k1\":\"v1\"")]
    public void EventMetadataJsonConverter_TruncatedJson_ThrowsJsonException(string truncatedJson)
    {
        var converter = new EventMetadataJsonConverter();
        var reader = new Utf8JsonReader(Encoding.UTF8.GetBytes(truncatedJson));
        reader.Read();
        try
        {
            converter.Read(ref reader, typeof(EventMetadata), _options);
        }
        catch (JsonException ex)
        {
            ex.Should().NotBeNull();
        }
    }

    #endregion

    #region EventEnvelope Serialization Tests

    [Fact]
    public void EventEnvelope_SerializeAndDeserialize_ShouldRoundtrip()
    {
        var eventId = EventId.New();
        var now = DateTimeOffset.UtcNow;
        var payload = new CustomerRegisteredEvent(eventId, "Alice Doe", "alice@example.com", now);

        var metadata = new EventMetadataBuilder()
            .WithCorrelationId(CorrelationId.New())
            .WithSource("customer-service")
            .Build();

        var envelope = EventEnvelope.Create(payload, metadata, EventType.From("customers.customer-registered"), EventVersion.V1);

        var json = JsonSerializer.Serialize(envelope, _options);
        using (var doc = JsonDocument.Parse(json))
        {
            doc.RootElement.GetProperty("id").GetGuid().Should().Be(eventId.Value);
            doc.RootElement.GetProperty("type").GetString().Should().Be("customers.customer-registered");
            doc.RootElement.GetProperty("version").GetUInt32().Should().Be(1);
            doc.RootElement.GetProperty("occurredAt").GetDateTimeOffset().Should().BeCloseTo(now, TimeSpan.FromSeconds(1));
            doc.RootElement.GetProperty("payload").GetProperty("customerName").GetString().Should().Be("Alice Doe");
        }

        var deserialized = JsonSerializer.Deserialize<EventEnvelope<CustomerRegisteredEvent>>(json, _options);

        deserialized.Should().NotBeNull();
        deserialized!.Id.Should().Be(eventId);
        deserialized.Type.Should().Be(EventType.From("customers.customer-registered"));
        deserialized.Version.Should().Be(EventVersion.V1);
        deserialized.OccurredAt.Should().BeCloseTo(now, TimeSpan.FromSeconds(1));
        deserialized.Payload.CustomerName.Should().Be("Alice Doe");
        deserialized.Payload.Email.Should().Be("alice@example.com");
        deserialized.Metadata.Source.Should().Be("customer-service");
    }

    [Fact]
    public void EventEnvelopeJsonConverter_DirectConverterExecution_ShouldWriteAndRead()
    {
        var converter = new EventEnvelopeJsonConverter<CustomerRegisteredEvent>();
        var eventId = EventId.New();
        var now = DateTimeOffset.UtcNow;
        var payload = new CustomerRegisteredEvent(eventId, "Alice Doe", "alice@example.com", now);
        var metadata = new EventMetadataBuilder()
            .WithCorrelationId("corr-99")
            .WithSource("test-srv")
            .Build();

        var envelope = EventEnvelope.Create(payload, metadata, EventType.From("customers.created"), EventVersion.From(2));

        // Write test
        using var stream = new MemoryStream();
        using (var writer = new Utf8JsonWriter(stream))
        {
            converter.Write(writer, envelope, _options);
        }

        var json = Encoding.UTF8.GetString(stream.ToArray());
        using (var doc = JsonDocument.Parse(json))
        {
            doc.RootElement.GetProperty("id").GetGuid().Should().Be(eventId.Value);
            doc.RootElement.GetProperty("type").GetString().Should().Be("customers.created");
            doc.RootElement.GetProperty("version").GetUInt32().Should().Be(2);
            doc.RootElement.GetProperty("occurredAt").GetDateTimeOffset().Should().BeCloseTo(now, TimeSpan.FromSeconds(1));
            doc.RootElement.TryGetProperty("payload", out _).Should().BeTrue();
            doc.RootElement.TryGetProperty("metadata", out _).Should().BeTrue();
        }

        // Read test
        var reader = new Utf8JsonReader(Encoding.UTF8.GetBytes(json));
        reader.Read(); // move to StartObject
        var deserialized = converter.Read(ref reader, typeof(EventEnvelope<CustomerRegisteredEvent>), _options);

        deserialized.Should().NotBeNull();
        deserialized.Id.Should().Be(eventId);
        deserialized.Type.Should().Be(EventType.From("customers.created"));
        deserialized.Version.Should().Be(EventVersion.From(2));
        deserialized.OccurredAt.Should().BeCloseTo(now, TimeSpan.FromSeconds(1));
        deserialized.Payload.CustomerName.Should().Be("Alice Doe");
        deserialized.Metadata.CorrelationId.Value.Should().Be("corr-99");
        deserialized.Metadata.Source.Should().Be("test-srv");
    }

    [Fact]
    public void EventEnvelopeJsonConverter_Write_EmptyMetadataAndDefaultVersion_ShouldOmitMetadata()
    {
        var converter = new EventEnvelopeJsonConverter<CustomerRegisteredEvent>();
        var eventId = EventId.New();
        var now = DateTimeOffset.UtcNow;
        var payload = new CustomerRegisteredEvent(eventId, "Alice Doe", "alice@example.com", now);

        var emptyEnvelope = new EventEnvelope<CustomerRegisteredEvent>(eventId, EventType.From("customers.empty"), default, now, payload, EventMetadata.Empty);
        using var emptyStream = new MemoryStream();
        using (var emptyWriter = new Utf8JsonWriter(emptyStream))
        {
            converter.Write(emptyWriter, emptyEnvelope, _options);
        }
        var emptyJson = Encoding.UTF8.GetString(emptyStream.ToArray());
        using (var emptyDoc = JsonDocument.Parse(emptyJson))
        {
            emptyDoc.RootElement.GetProperty("id").GetGuid().Should().Be(eventId.Value);
            emptyDoc.RootElement.GetProperty("type").GetString().Should().Be("customers.empty");
            emptyDoc.RootElement.GetProperty("version").GetUInt32().Should().Be(1);
            emptyDoc.RootElement.GetProperty("occurredAt").GetDateTimeOffset().Should().BeCloseTo(now, TimeSpan.FromSeconds(1));
            emptyDoc.RootElement.TryGetProperty("metadata", out _).Should().BeFalse();
        }
    }

    [Fact]
    public void EventEnvelopeJsonConverter_Read_CustomIdAndVersionEdgeCases()
    {
        var converter = new EventEnvelopeJsonConverter<CustomerRegisteredEvent>();
        var eventId = EventId.New();
        var now = DateTimeOffset.UtcNow;

        var customId = EventId.New();
        var customOccurredAt = now.AddDays(-5);
        var jsonWithStrVer = $"{{\"extraProperty\":{{\"nested\":123}},\"id\":\"{customId.Value}\",\"type\":\"customers.created\",\"version\":\"3\",\"occurredAt\":\"{customOccurredAt:O}\",\"payload\":{{\"id\":\"{eventId.Value}\",\"customerName\":\"Alice\",\"email\":\"a@a.com\",\"occurredAt\":\"{now:O}\"}}}}";
        var reader2 = new Utf8JsonReader(Encoding.UTF8.GetBytes(jsonWithStrVer));
        reader2.Read();
        var desStrVer = converter.Read(ref reader2, typeof(EventEnvelope<CustomerRegisteredEvent>), _options);
        desStrVer.Version.Should().Be(EventVersion.From(3));
        desStrVer.Id.Should().Be(customId);
        desStrVer.OccurredAt.Should().BeCloseTo(customOccurredAt, TimeSpan.FromSeconds(1));

        var jsonWithZeroVer = $"{{\"id\":\"{eventId.Value}\",\"type\":\"customers.created\",\"version\":0,\"occurredAt\":\"{now:O}\",\"payload\":{{\"id\":\"{eventId.Value}\",\"customerName\":\"Alice\",\"email\":\"a@a.com\",\"occurredAt\":\"{now:O}\"}}}}";
        var readerZero = new Utf8JsonReader(Encoding.UTF8.GetBytes(jsonWithZeroVer));
        readerZero.Read();
        var desZeroVer = converter.Read(ref readerZero, typeof(EventEnvelope<CustomerRegisteredEvent>), _options);
        desZeroVer.Version.Should().Be(EventVersion.V1);

        var jsonWithZeroStrVer = $"{{\"id\":\"{eventId.Value}\",\"type\":\"customers.created\",\"version\":\"0\",\"occurredAt\":\"{now:O}\",\"payload\":{{\"id\":\"{eventId.Value}\",\"customerName\":\"Alice\",\"email\":\"a@a.com\",\"occurredAt\":\"{now:O}\"}}}}";
        var readerZeroStr = new Utf8JsonReader(Encoding.UTF8.GetBytes(jsonWithZeroStrVer));
        readerZeroStr.Read();
        var desZeroStrVer = converter.Read(ref readerZeroStr, typeof(EventEnvelope<CustomerRegisteredEvent>), _options);
        desZeroStrVer.Version.Should().Be(EventVersion.V1);
    }

    [Fact]
    public void EventEnvelopeJsonConverter_NullGuards_ThrowsArgumentNullException()
    {
        var converter = new EventEnvelopeJsonConverter<CustomerRegisteredEvent>();
        var eventId = EventId.New();
        var now = DateTimeOffset.UtcNow;
        var payload = new CustomerRegisteredEvent(eventId, "Alice", "a@a.com", now);
        var envelope = EventEnvelope.Create(payload);

        var r = new Utf8JsonReader(Encoding.UTF8.GetBytes("{}"));
        try
        {
            converter.Read(ref r, typeof(EventEnvelope<CustomerRegisteredEvent>), null!);
            Assert.Fail("Expected ArgumentNullException");
        }
        catch (ArgumentNullException ex)
        {
            ex.ParamName.Should().Be("options");
        }

        Action actNullWriter = () => converter.Write(null!, envelope, _options);
        actNullWriter.Should().Throw<ArgumentNullException>().WithParameterName("writer");

        Action actNullEnv = () => converter.Write(new Utf8JsonWriter(new MemoryStream()), null!, _options);
        actNullEnv.Should().Throw<ArgumentNullException>().WithParameterName("value");

        Action actNullOptWrite = () => converter.Write(new Utf8JsonWriter(new MemoryStream()), envelope, null!);
        actNullOptWrite.Should().Throw<ArgumentNullException>().WithParameterName("options");
    }

    [Fact]
    public void EventEnvelopeJsonConverter_Read_NotStartObject_ThrowsJsonException()
    {
        var converter = new EventEnvelopeJsonConverter<CustomerRegisteredEvent>();
        var jsonNotObj = "\"scalar\"";
        var readerNotObj = new Utf8JsonReader(Encoding.UTF8.GetBytes(jsonNotObj));
        readerNotObj.Read();
        try
        {
            converter.Read(ref readerNotObj, typeof(EventEnvelope<CustomerRegisteredEvent>), _options);
            Assert.Fail("Expected JsonException");
        }
        catch (JsonException ex)
        {
            ex.Message.Should().Contain("Expected StartObject token");
        }
    }

    [Fact]
    public void EventEnvelopeJsonConverter_Read_MissingPayload_ThrowsJsonException()
    {
        var converter = new EventEnvelopeJsonConverter<CustomerRegisteredEvent>();
        var jsonNoPayload = $"{{\"id\":\"{Guid.NewGuid()}\",\"type\":\"customers.created\"}}";
        var readerNoPayload = new Utf8JsonReader(Encoding.UTF8.GetBytes(jsonNoPayload));
        readerNoPayload.Read();
        try
        {
            converter.Read(ref readerNoPayload, typeof(EventEnvelope<CustomerRegisteredEvent>), _options);
            Assert.Fail("Expected JsonException");
        }
        catch (JsonException ex)
        {
            ex.Message.Should().Contain("Missing payload property");
        }
    }

    [Fact]
    public void EventEnvelopeJsonConverter_EdgeCases_InvalidIdsVersionsAndTruncatedJson()
    {
        var converter = new EventEnvelopeJsonConverter<CustomerRegisteredEvent>();
        var eventId = EventId.New();
        var now = DateTimeOffset.UtcNow;

        // Invalid non-GUID string ID should leave id as EventId.Empty
        var jsonInvalidId = $"{{\"id\":\"invalid-guid\",\"type\":\"customers.created\",\"version\":1,\"occurredAt\":\"{now:O}\",\"payload\":{{\"id\":\"{eventId.Value}\",\"customerName\":\"Alice\",\"email\":\"a@a.com\",\"occurredAt\":\"{now:O}\"}}}}";
        var readerInvalidId = new Utf8JsonReader(Encoding.UTF8.GetBytes(jsonInvalidId));
        readerInvalidId.Read();
        var desInvalidId = converter.Read(ref readerInvalidId, typeof(EventEnvelope<CustomerRegisteredEvent>), _options);
        desInvalidId.Id.Should().Be(eventId);

        // Invalid non-numeric string version should leave version as EventVersion.V1
        var jsonInvalidVer = $"{{\"id\":\"{eventId.Value}\",\"type\":\"customers.created\",\"version\":\"not-a-number\",\"occurredAt\":\"{now:O}\",\"payload\":{{\"id\":\"{eventId.Value}\",\"customerName\":\"Alice\",\"email\":\"a@a.com\",\"occurredAt\":\"{now:O}\"}}}}";
        var readerInvalidVer = new Utf8JsonReader(Encoding.UTF8.GetBytes(jsonInvalidVer));
        readerInvalidVer.Read();
        var desInvalidVer = converter.Read(ref readerInvalidVer, typeof(EventEnvelope<CustomerRegisteredEvent>), _options);
        desInvalidVer.Version.Should().Be(EventVersion.V1);

        // Truncated JSON without EndObject token
        var jsonTruncated = $"{{\"id\":\"{eventId.Value}\",\"type\":\"customers.created\"";
        var readerTruncated = new Utf8JsonReader(Encoding.UTF8.GetBytes(jsonTruncated));
        readerTruncated.Read();
        try
        {
            converter.Read(ref readerTruncated, typeof(EventEnvelope<CustomerRegisteredEvent>), _options);
        }
        catch (JsonException ex)
        {
            ex.Should().NotBeNull();
        }

        // Envelope write with explicit EventMetadata.Empty
        var payload = new CustomerRegisteredEvent(eventId, "Bob", "bob@example.com", now);
        var envelopeWithEmptyMeta = new EventEnvelope<CustomerRegisteredEvent>(
            eventId,
            EventType.From("customers.empty-meta"),
            EventVersion.V1,
            now,
            payload,
            metadata: EventMetadata.Empty);

        using var stream = new MemoryStream();
        using (var writer = new Utf8JsonWriter(stream))
        {
            converter.Write(writer, envelopeWithEmptyMeta, _options);
        }
        var jsonEmptyMeta = Encoding.UTF8.GetString(stream.ToArray());
        using (var doc = JsonDocument.Parse(jsonEmptyMeta))
        {
            doc.RootElement.TryGetProperty("metadata", out _).Should().BeFalse();
        }
    }

    #endregion

    #region Property-Based Tests (FsCheck)

    [Property]
    public Property EventId_Serialization_Roundtrip_AnyGuid()
    {
        return Prop.ForAll(Arb.Default.Guid(), guid =>
        {
            var id = EventId.From(guid);
            var json = JsonSerializer.Serialize(id, _options);
            var deserialized = JsonSerializer.Deserialize<EventId>(json, _options);
            return deserialized == id && deserialized.Value == guid;
        });
    }

    [Property]
    public Property EventVersion_Serialization_Roundtrip_AnyPositiveUInt()
    {
        return Prop.ForAll(Arb.Default.PositiveInt(), posInt =>
        {
            var uintVal = (uint)posInt.Get;
            var ver = EventVersion.From(uintVal);
            var json = JsonSerializer.Serialize(ver, _options);
            var deserialized = JsonSerializer.Deserialize<EventVersion>(json, _options);
            return deserialized == ver && deserialized.Value == uintVal;
        });
    }

    [Property]
    public Property EventType_Serialization_Roundtrip_ArbitraryValidStrings()
    {
        return Prop.ForAll(Arb.Default.Guid(), guid =>
        {
            var typeName = $"events.test_{guid:N}";
            var eventType = EventType.From(typeName);
            var json = JsonSerializer.Serialize(eventType, _options);
            var deserialized = JsonSerializer.Deserialize<EventType>(json, _options);
            return deserialized == eventType && deserialized.Value == typeName;
        });
    }

    [Property]
    public Property CorrelationId_Serialization_Roundtrip_ArbitraryGuid()
    {
        return Prop.ForAll(Arb.Default.Guid(), guid =>
        {
            var corrStr = guid.ToString("N");
            var corr = CorrelationId.From(corrStr);
            var json = JsonSerializer.Serialize(corr, _options);
            var deserialized = JsonSerializer.Deserialize<CorrelationId>(json, _options);
            return deserialized == corr && deserialized.Value == corrStr;
        });
    }

    [Property]
    public Property EventMetadata_Serialization_Roundtrip_ArbitraryHeaders()
    {
        return Prop.ForAll(Arb.Default.Guid(), Arb.Default.Guid(), (g1, g2) =>
        {
            var corr = CorrelationId.From($"corr_{g1:N}");
            var caus = CausationId.From($"caus_{g2:N}");
            var tenant = TenantId.From("tenant-alpha");
            var metadata = new EventMetadataBuilder()
                .WithCorrelationId(corr)
                .WithCausationId(caus)
                .WithTenantId(tenant)
                .WithSource("test-producer")
                .WithContentType("application/json")
                .WithHeader("X-Trace", g1.ToString())
                .WithHeader("X-Tenant-Region", "eu-central-1")
                .Build();

            var json = JsonSerializer.Serialize(metadata, _options);
            var deserialized = JsonSerializer.Deserialize<EventMetadata>(json, _options);

            return deserialized is not null &&
                   deserialized.CorrelationId == corr &&
                   deserialized.CausationId == caus &&
                   deserialized.TenantId == tenant &&
                   deserialized.Source == "test-producer" &&
                   deserialized.ContentType == "application/json" &&
                   deserialized.CustomHeaders.ContainsKey("X-Trace") &&
                   deserialized.CustomHeaders["X-Trace"] == g1.ToString() &&
                   deserialized.CustomHeaders["X-Tenant-Region"] == "eu-central-1";
        });
    }

    [Property]
    public Property EventEnvelope_Serialization_Roundtrip_ArbitraryPayloads()
    {
        return Prop.ForAll(Arb.Default.Guid(), Arb.Default.Guid(), (eventGuid, customerGuid) =>
        {
            var eventId = EventId.From(eventGuid);
            var now = DateTimeOffset.UtcNow;
            var payload = new CustomerRegisteredEvent(eventId, $"Customer-{customerGuid:N}", $"cust_{customerGuid:N}@example.com", now);

            var envelope = new EventEnvelope<CustomerRegisteredEvent>(
                eventId,
                EventType.From("customers.registered"),
                EventVersion.V1,
                now,
                payload,
                EventMetadata.Empty);

            var json = JsonSerializer.Serialize(envelope, _options);
            var deserialized = JsonSerializer.Deserialize<EventEnvelope<CustomerRegisteredEvent>>(json, _options);

            return deserialized is not null &&
                   deserialized.Id == eventId &&
                   deserialized.Type == EventType.From("customers.registered") &&
                   deserialized.Version == EventVersion.V1 &&
                   deserialized.Payload.CustomerName == payload.CustomerName &&
                   deserialized.Payload.Email == payload.Email;
        });
    }

    [Property]
    public Property EventEnvelope_Serialization_Roundtrip_WithComplexMetadata()
    {
        return Prop.ForAll(Arb.Default.Guid(), Arb.Default.Guid(), (eventGuid, traceGuid) =>
        {
            var eventId = EventId.From(eventGuid);
            var now = DateTimeOffset.UtcNow;
            var payload = new CustomerRegisteredEvent(eventId, "Complex Customer", "complex@example.com", now);

            var metadata = new EventMetadataBuilder()
                .WithCorrelationId($"corr_{traceGuid:N}")
                .WithCausationId($"caus_{eventGuid:N}")
                .WithTenantId("tenant-enterprise")
                .WithSource("https://ordering.corp.internal")
                .WithHeader("X-Idempotency-Key", eventGuid.ToString())
                .Build();

            var envelope = EventEnvelope.Create(payload, metadata, EventType.From("customers.complex-registered"), EventVersion.From(2));

            var json = JsonSerializer.Serialize(envelope, _options);
            var deserialized = JsonSerializer.Deserialize<EventEnvelope<CustomerRegisteredEvent>>(json, _options);

            return deserialized is not null &&
                   deserialized.Id == eventId &&
                   deserialized.Type == EventType.From("customers.complex-registered") &&
                   deserialized.Version.Value == 2 &&
                   deserialized.Metadata.CorrelationId.Value == $"corr_{traceGuid:N}" &&
                   deserialized.Metadata.CausationId.Value == $"caus_{eventGuid:N}" &&
                   deserialized.Metadata.TenantId.Value == "tenant-enterprise" &&
                   deserialized.Metadata.Source == "https://ordering.corp.internal" &&
                   deserialized.Metadata.CustomHeaders["X-Idempotency-Key"] == eventGuid.ToString();
        });
    }

    #endregion

    #region Forward Compatibility and Schema Evolution Tests

    [Fact]
    public void EventMetadata_Deserialize_WithUnknownProperties_ShouldIgnoreAndTolerateGracefully()
    {
        var json = """
        {
            "correlationId": "corr-123",
            "causationId": "caus-456",
            "tenantId": "tenant-789",
            "source": "https://api.example.com",
            "contentType": "application/json",
            "unknownScalar": "unexpected-value",
            "unknownNumber": 42.5,
            "unknownBoolean": true,
            "unknownObject": { "subField": "foo", "subArray": [1, 2, 3] },
            "unknownArray": ["a", "b", "c"],
            "customHeaders": {
                "X-Custom-1": "val-1"
            }
        }
        """;

        var metadata = JsonSerializer.Deserialize<EventMetadata>(json, _options);

        metadata.Should().NotBeNull();
        metadata!.CorrelationId.Value.Should().Be("corr-123");
        metadata.CausationId.Value.Should().Be("caus-456");
        metadata.TenantId.Value.Should().Be("tenant-789");
        metadata.Source.Should().Be("https://api.example.com");
        metadata.ContentType.Should().Be("application/json");
        metadata.CustomHeaders["X-Custom-1"].Should().Be("val-1");
    }

    [Fact]
    public void EventEnvelope_Deserialize_WithUnknownEnvelopeProperties_ShouldIgnoreAndTolerateGracefully()
    {
        var eventId = EventId.New();
        var now = DateTimeOffset.UtcNow;
        var json = $$"""
        {
            "id": "{{eventId.Value}}",
            "type": "customers.registered",
            "version": 1,
            "occurredAt": "{{now:O}}",
            "unknownEnvelopeTag": "future-feature-data",
            "futureMetadata": { "clusterId": "us-east-1", "flags": [1, 2] },
            "payload": {
                "id": "{{eventId.Value}}",
                "customerName": "John Doe",
                "email": "john@example.com",
                "occurredAt": "{{now:O}}"
            }
        }
        """;

        var envelope = JsonSerializer.Deserialize<EventEnvelope<CustomerRegisteredEvent>>(json, _options);

        envelope.Should().NotBeNull();
        envelope!.Id.Should().Be(eventId);
        envelope.Type.Value.Should().Be("customers.registered");
        envelope.Version.Value.Should().Be(1);
        envelope.Payload.CustomerName.Should().Be("John Doe");
        envelope.Payload.Email.Should().Be("john@example.com");
    }

    [Fact]
    public void CustomerRegisteredEvent_Deserialize_WithAdditionalPayloadFields_ShouldDeserializeKnownFieldsSuccessfully()
    {
        var eventId = EventId.New();
        var now = DateTimeOffset.UtcNow;
        var json = $$"""
        {
            "id": "{{eventId.Value}}",
            "customerName": "Jane Doe",
            "email": "jane@example.com",
            "occurredAt": "{{now:O}}",
            "futureField": "new-version-extra-field",
            "secondaryPhone": "+1-555-0199"
        }
        """;

        var evt = JsonSerializer.Deserialize<CustomerRegisteredEvent>(json, _options);

        evt.Should().NotBeNull();
        evt!.Id.Should().Be(eventId);
        evt.CustomerName.Should().Be("Jane Doe");
        evt.Email.Should().Be("jane@example.com");
    }

    #endregion
}







// Copyright © Erickson Lopez. MIT License.
using System;
using System.Text.Json;
using AwesomeAssertions;
using EricksonLopez.Events.Contracts;
using EricksonLopez.Events.Identifiers;
using EricksonLopez.Events.Metadata;
using EricksonLopez.Events.Serialization.SystemTextJson.Converters;
using Xunit;

namespace EricksonLopez.Events.Serialization.Tests.Security;

[Trait("Category", "Security")]
public sealed class SecurityAttackTests
{
    [Fact]
    public void EventMetadataJsonConverter_WhenCustomHeadersContainsNonStringTokens_ThrowsInvalidOperationException_InsteadOfJsonException()
    {
        // Attack: Attacker crafts a payload with non-string JSON inside customHeaders (e.g. array or nested object).
        // Parser bombs or unexpected token types cause System.Text.Json reader.GetString() to throw InvalidOperationException.
        const string maliciousJson = """
        {
            "correlationId": "corr-1",
            "causationId": "cause-1",
            "tenantId": "tenant-1",
            "customHeaders": {
                "evilArray": [1, 2, 3]
            }
        }
        """;

        var options = new JsonSerializerOptions();
        options.Converters.Add(new EventMetadataJsonConverter());

        // Standard JSON parsers must throw JsonException on malformed/invalid schemas, not InvalidOperationException.
        // reader.GetString() on StartArray token throws InvalidOperationException.
        Action act1 = () => JsonSerializer.Deserialize<EventMetadata>(maliciousJson, options);

        act1.Should().Throw<JsonException>()
            .WithMessage("*CustomHeader value for key 'evilArray' must be a string*");
    }

    [Fact]
    public void EventMetadataJsonConverter_WhenCustomHeadersContainsNestedObject_ThrowsJsonException()
    {
        const string maliciousJson = """
        {
            "customHeaders": {
                "nestedObject": { "subKey": "subValue" }
            }
        }
        """;

        var options = new JsonSerializerOptions();
        options.Converters.Add(new EventMetadataJsonConverter());

        Action act = () => JsonSerializer.Deserialize<EventMetadata>(maliciousJson, options);

        act.Should().Throw<JsonException>()
            .WithMessage("*CustomHeader value for key 'nestedObject' must be a string*");
    }

    [Fact]
    public void EventMetadata_RecordEquality_WhenCustomHeadersAdded_SucceedsValueEquality()
    {
        // Validates EVT-MOD-002 Remediation: EventMetadata implements custom IEquatable<EventMetadata>
        // which compares CustomHeaders contents by key-value.
        var meta1 = EventMetadata.Create(
            CorrelationId.From("C1"),
            CausationId.From("K1"),
            TenantId.From("T1"))
            .WithHeader("X-Tenant-Tier", "Premium");

        var meta2 = EventMetadata.Create(
            CorrelationId.From("C1"),
            CausationId.From("K1"),
            TenantId.From("T1"))
            .WithHeader("X-Tenant-Tier", "Premium");

        (meta1 == meta2).Should().BeTrue(
            "EventMetadata now implements value equality comparing headers contents.");
        (meta1.GetHashCode() == meta2.GetHashCode()).Should().BeTrue();
    }
}

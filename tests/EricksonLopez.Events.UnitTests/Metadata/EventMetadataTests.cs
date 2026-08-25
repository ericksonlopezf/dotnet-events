// Copyright © Erickson Lopez. MIT License.
using System;
using System.Collections.Generic;

namespace EricksonLopez.Events.UnitTests.Metadata;

using System.Collections.Frozen;
using EricksonLopez.Events.Identifiers;
using EricksonLopez.Events.Metadata;
using AwesomeAssertions;
using Xunit;

[Xunit.Trait("Category", "Unit")]
public sealed class EventMetadataTests
{
    [Fact]
    public void EventMetadata_Empty_ShouldHaveDefaultValues()
    {
        var meta = EventMetadata.Empty;

        meta.CorrelationId.Should().Be(CorrelationId.Empty);
        meta.CausationId.Should().Be(CausationId.Empty);
        meta.TenantId.Should().Be(TenantId.Empty);
        meta.Source.Should().BeNull();
        meta.ContentType.Should().BeNull();
        meta.CustomHeaders.Should().BeEmpty();
    }

    [Fact]
    public void EventMetadata_Create_WithNulls_ShouldDefaultCorrectly()
    {
        var meta = EventMetadata.Create();

        meta.CorrelationId.Should().Be(CorrelationId.Empty);
        meta.CausationId.Should().Be(CausationId.Empty);
        meta.TenantId.Should().Be(TenantId.Empty);
        meta.Source.Should().BeNull();
        meta.ContentType.Should().BeNull();
        meta.CustomHeaders.Should().BeEmpty();
    }

    [Fact]
    public void EventMetadata_Create_WithValues_ShouldPopulateCorrectly()
    {
        var correlation = CorrelationId.From("corr-1");
        var causation = CausationId.From("caus-1");
        var tenant = TenantId.From("tenant-1");
        var headers = new Dictionary<string, string> { ["k1"] = "v1" };

        var meta = EventMetadata.Create(correlation, causation, tenant, "srv", "text/plain", headers);

        meta.CorrelationId.Should().Be(correlation);
        meta.CausationId.Should().Be(causation);
        meta.TenantId.Should().Be(tenant);
        meta.Source.Should().Be("srv");
        meta.ContentType.Should().Be("text/plain");
        meta.CustomHeaders["k1"].Should().Be("v1");
    }

    [Fact]
    public void EventMetadata_Constructor_WithFrozenDictionaryHeaders_ShouldReuseInstance()
    {
        var frozen = new Dictionary<string, string> { ["k"] = "v" }.ToFrozenDictionary();
        var meta = new EventMetadata(CorrelationId.Empty, CausationId.Empty, TenantId.Empty, customHeaders: frozen);

        meta.CustomHeaders.Should().BeSameAs(frozen);
    }

    [Fact]
    public void EventMetadataBuilder_Build_WithCompleteValues_ShouldConstructCompleteMetadata()
    {
        var correlationId = CorrelationId.New();
        var causationId = CausationId.From("CMD-101");
        var tenantId = TenantId.From("tenant-us");

        var meta = new EventMetadataBuilder()
            .WithCorrelationId(correlationId)
            .WithCausationId(causationId)
            .WithTenantId(tenantId)
            .WithSource("ordering-service")
            .WithContentType("application/json")
            .WithHeader("X-Trace-Id", "12345")
            .WithHeader("X-Custom", "value")
            .Build();

        meta.CorrelationId.Should().Be(correlationId);
        meta.CausationId.Should().Be(causationId);
        meta.TenantId.Should().Be(tenantId);
        meta.Source.Should().Be("ordering-service");
        meta.ContentType.Should().Be("application/json");
        meta.CustomHeaders.Should().HaveCount(2);
        meta.TryGetHeader("X-Trace-Id", out var traceId).Should().BeTrue();
        traceId.Should().Be("12345");
    }

    [Fact]
    public void EventMetadataBuilder_Build_WithStrings_ShouldConstructCorrectly()
    {
        var meta = new EventMetadataBuilder()
            .WithCorrelationId("corr-str")
            .WithCausationId("caus-str")
            .WithTenantId("tenant-str")
            .WithHeader("h1", null!)
            .Build();

        meta.CorrelationId.Value.Should().Be("corr-str");
        meta.CausationId.Value.Should().Be("caus-str");
        meta.TenantId.Value.Should().Be("tenant-str");
        meta.CustomHeaders["h1"].Should().BeEmpty();
    }

    [Fact]
    public void EventMetadataBuilder_Build_WithoutCustomHeaders_ShouldReturnFrozenDictionaryEmptySingleton()
    {
        var meta = new EventMetadataBuilder().Build();
        meta.CustomHeaders.Should().BeSameAs(FrozenDictionary<string, string>.Empty);
    }

    [Fact]
    public void EventMetadataBuilder_WithHeader_WithInvalidHeaderKey_ShouldThrowArgumentException()
    {
        var builder = new EventMetadataBuilder();
        Action act = () => builder.WithHeader("   ", "val");
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void EventMetadata_WithHeader_ValidKeyAndValue_ShouldReturnNewImmutableInstance()
    {
        var original = EventMetadata.Empty;
        var modified = original.WithHeader("Key", "Value");

        original.CustomHeaders.Should().BeEmpty();
        modified.CustomHeaders.Should().ContainKey("Key");
        modified.TryGetHeader("Key", out var val).Should().BeTrue();
        val.Should().Be("Value");

        var modifiedWithNullVal = modified.WithHeader("Key2", null!);
        modifiedWithNullVal.CustomHeaders["Key2"].Should().BeEmpty();

        modified.TryGetHeader("NonExistent", out var nonExistent).Should().BeFalse();
        nonExistent.Should().BeNull();

        Action actInvalid = () => original.WithHeader("", "val");
        actInvalid.Should().Throw<ArgumentException>();
    }
}




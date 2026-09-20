// Copyright © Erickson Lopez. MIT License.
using System;
using System.Collections.Generic;

namespace EricksonLopez.Events.UnitTests.Metadata;

using System.Collections.Frozen;
using AwesomeAssertions;
using EricksonLopez.Events.Identifiers;
using EricksonLopez.Events.Metadata;
using Xunit;

[Xunit.Trait("Category", "Unit")]
public sealed class EventMetadataTests
{
    [Fact]
    public void EVT_PRF_002_WithHeader_ToExistingMetadata_ShouldNotUseToFrozenDictionaryForPerformance()
    {
        // Verified Remediation for EVT-PRF-002:
        // `EventMetadata.WithHeader` must not create intermediate FrozenDictionary instances in hot paths
        // because it allocates memory and triggers GC Gen0 collections under load.
        // It should just create a small immutable or copied Dictionary, avoiding the ToFrozenDictionary heavy lift.

        var meta = EventMetadata.Empty;
        var meta2 = meta.WithHeader("X-Test-1", "A");
        var meta3 = meta2.WithHeader("X-Test-2", "B");

        meta3.TryGetHeader("X-Test-1", out var v1).Should().BeTrue();
        meta3.TryGetHeader("X-Test-2", out var v2).Should().BeTrue();

        v1.Should().Be("A");
        v2.Should().Be("B", "EVT-PRF-002: WithHeader must correctly propagate headers without incurring heavy ToFrozenDictionary penalties in the hot path.");

        // Explicitly ensuring type is NOT FrozenDictionary (if we can detect it) or at least ensuring the logic works seamlessly
        // Note: The remediation removed ToFrozenDictionary from the hot path in WithHeader.
        var headersType = meta3.CustomHeaders.GetType();
        headersType.Name.Should().NotContain("FrozenDictionary", "EVT-PRF-002: Intermediate dictionaries should not use FrozenDictionary in hot paths.");
    }

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
    public void EventMetadata_Constructor_WithFrozenDictionaryHeaders_ShouldNormalizeToOrdinalIgnoreCase()
    {
        // EVT-DAT-003 FIX: The constructor always rebuilds the FrozenDictionary with OrdinalIgnoreCase
        // to guarantee case-insensitive header lookups regardless of the input comparer.
        // Reference equality (BeSameAs) is no longer guaranteed — content equality is.
        var frozen = new Dictionary<string, string> { ["k"] = "v" }.ToFrozenDictionary();
        var meta = new EventMetadata(CorrelationId.Empty, CausationId.Empty, TenantId.Empty, customHeaders: frozen);

        meta.CustomHeaders.Should().HaveCount(1);
        meta.CustomHeaders["k"].Should().Be("v");
        // Verify case-insensitive access is guaranteed regardless of input comparer
        meta.CustomHeaders["K"].Should().Be("v");
        meta.TryGetHeader("K", out var val).Should().BeTrue();
        val.Should().Be("v");
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

    [Fact]
    public void EventMetadata_Equals_ShouldVerifyValueEqualityAndSymmetry()
    {
        var meta1 = EventMetadata.Create(
            CorrelationId.From("corr-1"),
            CausationId.From("caus-1"),
            TenantId.From("tenant-1"),
            "source-1",
            "application/json",
            new Dictionary<string, string> { ["header-a"] = "val-a", ["Header-B"] = "val-b" });

        var meta2 = EventMetadata.Create(
            CorrelationId.From("corr-1"),
            CausationId.From("caus-1"),
            TenantId.From("tenant-1"),
            "source-1",
            "application/json",
            new Dictionary<string, string> { ["HEADER-A"] = "val-a", ["header-b"] = "val-b" });

        var metaDifferentTenant = EventMetadata.Create(
            CorrelationId.From("corr-1"),
            CausationId.From("caus-1"),
            TenantId.From("tenant-2"),
            "source-1",
            "application/json",
            new Dictionary<string, string> { ["header-a"] = "val-a", ["Header-B"] = "val-b" });

        var metaDifferentHeaders = EventMetadata.Create(
            CorrelationId.From("corr-1"),
            CausationId.From("caus-1"),
            TenantId.From("tenant-1"),
            "source-1",
            "application/json",
            new Dictionary<string, string> { ["header-a"] = "different-val" });

        // Typed Equals
        meta1.Equals(meta1).Should().BeTrue();
        meta1.Equals(meta2).Should().BeTrue();
        meta2.Equals(meta1).Should().BeTrue();
        meta1.Equals(metaDifferentTenant).Should().BeFalse();
        meta1.Equals(metaDifferentHeaders).Should().BeFalse();
        // Operators
        (meta1 == meta2).Should().BeTrue();
        (meta1 != meta2).Should().BeFalse();
        (meta1 == metaDifferentTenant).Should().BeFalse();
        (meta1 != metaDifferentTenant).Should().BeTrue();

        EventMetadata? nullMeta = null;
        meta1.Equals(nullMeta).Should().BeFalse();
        (nullMeta == meta1).Should().BeFalse();
        (meta1 == nullMeta).Should().BeFalse();
    }

    [Fact]
    public void EventMetadata_BoxedEquals_ShouldHandleAllObjectCases()
    {
        var meta1 = EventMetadata.Create(CorrelationId.From("c1"));
        var meta2 = EventMetadata.Create(CorrelationId.From("c1"));
        var meta3 = EventMetadata.Create(CorrelationId.From("c2"));

        object boxed = meta1;
        boxed.Equals((object)meta2).Should().BeTrue();
        boxed.Equals((object)meta3).Should().BeFalse();
        boxed.Equals("some string").Should().BeFalse();
        boxed.Equals(null).Should().BeFalse();
    }

    [Fact]
    public void EventMetadata_GetHashCode_ShouldBeOrderIndependentAndEqualForEqualInstances()
    {
        var meta1 = EventMetadata.Create(
            CorrelationId.From("corr-1"),
            CausationId.From("caus-1"),
            TenantId.From("tenant-1"),
            "source-1",
            "application/json",
            new Dictionary<string, string>
            {
                ["First"] = "1",
                ["Second"] = "2",
                ["Third"] = "3"
            });

        // Same entries inserted in reverse order and with different case
        var meta2 = EventMetadata.Create(
            CorrelationId.From("corr-1"),
            CausationId.From("caus-1"),
            TenantId.From("tenant-1"),
            "source-1",
            "application/json",
            new Dictionary<string, string>
            {
                ["THIRD"] = "3",
                ["second"] = "2",
                ["first"] = "1"
            });

        meta1.Should().Be(meta2);
        meta1.GetHashCode().Should().Be(meta2.GetHashCode());
    }

    [Fact]
    public void EventMetadata_GetHashCode_WrappingAdditionDoesNotCancelCollidingEntries()
    {
        // When two distinct headers exist, wrapping addition preserves entropy
        var meta = EventMetadata.Create(
            customHeaders: new Dictionary<string, string>
            {
                ["h1"] = "val",
                ["h2"] = "val"
            });

        var hash = meta.GetHashCode();
        hash.Should().NotBe(0);
    }
}

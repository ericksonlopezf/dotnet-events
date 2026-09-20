// Copyright © Erickson Lopez. MIT License.
using System;
using AwesomeAssertions;
using EricksonLopez.Events.Identifiers;
using Xunit;

namespace EricksonLopez.Events.UnitTests.Adversarial.Identifiers;

[Trait("Category", "Unit")]
public sealed class IdentifiersDefaultInvariantsTests
{
    [Fact]
    public void DefaultTenantId_ToString_ReturnsEmptyString_HonoringNonNullContract()
    {
        // Validates EVT-MOD-001 Remediation:
        TenantId defaultTenant = default;

        string str = defaultTenant.ToString();
        str.Should().NotBeNull();
        str.Should().Be(string.Empty, "default(TenantId).ToString() returns string.Empty instead of null.");
    }

    [Fact]
    public void DefaultCorrelationId_ToString_ReturnsEmptyString_HonoringNonNullContract()
    {
        CorrelationId defaultCorrelation = default;

        string str = defaultCorrelation.ToString();
        str.Should().NotBeNull();
        str.Should().Be(string.Empty, "default(CorrelationId).ToString() returns string.Empty instead of null.");
    }

    [Fact]
    public void DefaultCausationId_ToString_ReturnsEmptyString_HonoringNonNullContract()
    {
        CausationId defaultCausation = default;

        string str = defaultCausation.ToString();
        str.Should().NotBeNull();
        str.Should().Be(string.Empty, "default(CausationId).ToString() returns string.Empty instead of null.");
    }

    [Fact]
    public void DefaultEventVersion_ConsistentEqualityAndStringRepresentation()
    {
        // Validates EVT-MOD-001 Remediation:
        // default(EventVersion) now normalizes to 1, consistent with EventVersion.V1
        EventVersion defaultVersion = default;

        defaultVersion.Value.Should().Be(1u);
        defaultVersion.ToString().Should().Be("1");

        // String representations match:
        (defaultVersion.ToString() == EventVersion.V1.ToString()).Should().BeTrue();

        // Value equality succeeds:
        (defaultVersion == EventVersion.V1).Should().BeTrue();
        (defaultVersion <= EventVersion.V1).Should().BeTrue();
    }
}

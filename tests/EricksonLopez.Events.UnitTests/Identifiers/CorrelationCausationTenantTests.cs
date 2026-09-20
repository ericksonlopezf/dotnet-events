// Copyright © Erickson Lopez. MIT License.
using System;

namespace EricksonLopez.Events.UnitTests.Identifiers;

using AwesomeAssertions;
using EricksonLopez.Events.Identifiers;
using FsCheck;
using FsCheck.Xunit;
using Xunit;

[Xunit.Trait("Category", "Unit")]
public sealed class CorrelationCausationTenantTests
{
    [Fact]
    public void CorrelationId_New_ShouldGenerateGuidV7String()
    {
        var correlationId = CorrelationId.New();

        correlationId.IsEmpty.Should().BeFalse();
        Guid.TryParse(correlationId.Value, out var guid).Should().BeTrue();
        guid.Version.Should().Be(7);
    }

    [Fact]
    public void CorrelationId_FromGuidAndString_ShouldWork()
    {
        var guid = Guid.NewGuid();
        var id1 = CorrelationId.From(guid);
        var id2 = CorrelationId.From(guid.ToString());

        id1.Should().Be(id2);
        ((string)id1).Should().Be(guid.ToString());
        ((CorrelationId)guid.ToString()).Should().Be(id1);
    }

    [Fact]
    public void CorrelationId_EmptyAndNull_ShouldBeEmpty()
    {
        CorrelationId.Empty.IsEmpty.Should().BeTrue();
        CorrelationId.Empty.Value.Should().BeEmpty();
        CorrelationId.Empty.ToString().Should().BeEmpty();

        var fromNull = new CorrelationId(null!);
        fromNull.IsEmpty.Should().BeTrue();
        fromNull.Value.Should().BeEmpty();
    }

    [Fact]
    public void CorrelationId_ComparisonsAndParsing_ShouldWork()
    {
        var id1 = CorrelationId.From("A");
        var id2 = CorrelationId.From("B");
        var id1Copy = CorrelationId.From("A");

        (id1 < id2).Should().BeTrue();
        (id1 <= id2).Should().BeTrue();
        (id2 > id1).Should().BeTrue();
        (id2 >= id1).Should().BeTrue();

        (id1 > id2).Should().BeFalse();
        (id1 >= id2).Should().BeFalse();
        (id2 < id1).Should().BeFalse();
        (id2 <= id1).Should().BeFalse();

        (id1 <= id1Copy).Should().BeTrue();
        (id1 >= id1Copy).Should().BeTrue();
        (id1 < id1Copy).Should().BeFalse();
        (id1 > id1Copy).Should().BeFalse();

        id1.CompareTo(null).Should().Be(1);
        id1.CompareTo((object)id1Copy).Should().Be(0);
        id1.Invoking(x => x.CompareTo(123))
            .Should().Throw<ArgumentException>()
            .WithMessage($"*Object must be of type {nameof(CorrelationId)}*");

        CorrelationId.TryParse("test", null, out var parsed).Should().BeTrue();
        parsed.Value.Should().Be("test");

        CorrelationId.TryParse(null, null, out var failedNull).Should().BeFalse();
        failedNull.Should().Be(CorrelationId.Empty);

        CorrelationId.Parse("parsed").Value.Should().Be("parsed");

        Action actNull = () => CorrelationId.Parse(null!);
        actNull.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void CausationId_FromEventIdAndGuid_ShouldWork()
    {
        var eventId = EventId.New();
        var causation1 = CausationId.From(eventId);
        var causation2 = CausationId.From(eventId.Value);
        var causation3 = CausationId.From(eventId.ToString());

        causation1.Should().Be(causation2);
        causation1.Should().Be(causation3);
        causation1.IsEmpty.Should().BeFalse();
        ((string)causation1).Should().Be(eventId.ToString());
        ((CausationId)eventId.ToString()).Should().Be(causation1);
    }

    [Fact]
    public void CausationId_EmptyAndNull_ShouldBeEmpty()
    {
        CausationId.Empty.IsEmpty.Should().BeTrue();
        CausationId.Empty.Value.Should().BeEmpty();
        CausationId.Empty.ToString().Should().BeEmpty();

        var fromNull = new CausationId(null!);
        fromNull.IsEmpty.Should().BeTrue();
    }

    [Fact]
    public void CausationId_ComparisonsAndParsing_ShouldWork()
    {
        var c1 = CausationId.From("CMD-1");
        var c2 = CausationId.From("CMD-2");
        var c1Copy = CausationId.From("CMD-1");

        (c1 < c2).Should().BeTrue();
        (c1 <= c2).Should().BeTrue();
        (c2 > c1).Should().BeTrue();
        (c2 >= c1).Should().BeTrue();

        (c1 > c2).Should().BeFalse();
        (c1 >= c2).Should().BeFalse();
        (c2 < c1).Should().BeFalse();
        (c2 <= c1).Should().BeFalse();

        (c1 <= c1Copy).Should().BeTrue();
        (c1 >= c1Copy).Should().BeTrue();
        (c1 < c1Copy).Should().BeFalse();
        (c1 > c1Copy).Should().BeFalse();

        c1.CompareTo(null).Should().Be(1);
        c1.CompareTo((object)c1Copy).Should().Be(0);
        c1.Invoking(x => x.CompareTo(456))
            .Should().Throw<ArgumentException>()
            .WithMessage($"*Object must be of type {nameof(CausationId)}*");

        CausationId.TryParse("CMD-99", null, out var parsed).Should().BeTrue();
        parsed.Value.Should().Be("CMD-99");

        CausationId.TryParse(null, null, out var failedNull).Should().BeFalse();
        failedNull.Should().Be(CausationId.Empty);

        CausationId.Parse("CMD-100").Value.Should().Be("CMD-100");

        Action actNull = () => CausationId.Parse(null!);
        actNull.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void TenantId_CreationAndComparisons_ShouldWork()
    {
        var t1 = TenantId.From("tenant-a");
        var t2 = TenantId.From("TENANT-B");
        var t1Upper = TenantId.From("TENANT-A");

        t1.IsEmpty.Should().BeFalse();
        ((string)t1).Should().Be("tenant-a");
        ((TenantId)"tenant-a").Should().Be(t1);

        // Case-insensitive comparison
        t1.CompareTo(t1Upper).Should().Be(0);
        t1.CompareTo((object)t1Upper).Should().Be(0);
        (t1 <= t1Upper).Should().BeTrue();
        (t1 >= t1Upper).Should().BeTrue();
        (t1 < t1Upper).Should().BeFalse();
        (t1 > t1Upper).Should().BeFalse();

        (t1 < t2).Should().BeTrue();
        (t1 <= t2).Should().BeTrue();
        (t2 > t1).Should().BeTrue();
        (t2 >= t1).Should().BeTrue();

        (t1 > t2).Should().BeFalse();
        (t1 >= t2).Should().BeFalse();
        (t2 < t1).Should().BeFalse();
        (t2 <= t1).Should().BeFalse();

        t1.CompareTo(null).Should().Be(1);
        t1.Invoking(x => x.CompareTo(789))
            .Should().Throw<ArgumentException>()
            .WithMessage($"*Object must be of type {nameof(TenantId)}*");

        TenantId.TryParse("tenant-1", null, out var parsed).Should().BeTrue();
        parsed.Value.Should().Be("tenant-1");

        TenantId.TryParse(null, null, out var failedNull).Should().BeFalse();
        failedNull.Should().Be(TenantId.Empty);

        TenantId.Parse("tenant-1").Should().Be(parsed);

        Action actNull = () => TenantId.Parse(null!);
        actNull.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void TenantId_EmptyAndNull_ShouldBeEmpty()
    {
        TenantId.Empty.IsEmpty.Should().BeTrue();
        TenantId.Empty.Value.Should().BeEmpty();
        TenantId.Empty.ToString().Should().BeEmpty();

        var fromNull = new TenantId(null!);
        fromNull.IsEmpty.Should().BeTrue();
    }

    [Property]
    public Property CorrelationId_RoundtripsAnyNonNullString()
    {
        return Prop.ForAll(Arb.Default.NonNull<string>(), str =>
        {
            var id = CorrelationId.From(str.Get);
            return CorrelationId.Parse(id.ToString()) == id && id.Value == str.Get;
        });
    }

    [Property]
    public Property CausationId_RoundtripsAnyNonNullString()
    {
        return Prop.ForAll(Arb.Default.NonNull<string>(), str =>
        {
            var id = CausationId.From(str.Get);
            return CausationId.Parse(id.ToString()) == id && id.Value == str.Get;
        });
    }

    [Property]
    public Property TenantId_RoundtripsAnyNonNullString()
    {
        return Prop.ForAll(Arb.Default.NonNull<string>(), str =>
        {
            var id = TenantId.From(str.Get);
            return TenantId.Parse(id.ToString()) == id && id.Value == str.Get;
        });
    }
}




// Copyright © Erickson Lopez. MIT License.
using System;

namespace EricksonLopez.Events.UnitTests.Identifiers;

using System.Globalization;
using AwesomeAssertions;
using EricksonLopez.Events.Identifiers;
using FsCheck;
using FsCheck.Xunit;
using Xunit;

[Xunit.Trait("Category", "Unit")]
public sealed class EventTypeAndVersionTests
{
    [Fact]
    public void EventType_From_ValidString_ShouldInitializeCorrectly()
    {
        var eventType = EventType.From("orders.order-created");

        eventType.IsEmpty.Should().BeFalse();
        eventType.Value.Should().Be("orders.order-created");
        eventType.ToString().Should().Be("orders.order-created");
        ((string)eventType).Should().Be("orders.order-created");
        ((EventType)"orders.order-created").Should().Be(eventType);
    }

    [Fact]
    public void EventType_DefaultStruct_ShouldBeEmpty()
    {
        EventType defaultType = default;

        defaultType.IsEmpty.Should().BeTrue();
        defaultType.ToString().Should().BeEmpty();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void EventType_InvalidString_ShouldThrowArgumentException(string? invalid)
    {
        Action act = () => new EventType(invalid!);
        act.Should().Throw<ArgumentException>().WithParameterName("value");

        Action actParse = () => EventType.Parse(invalid!);
        actParse.Should().Throw<ArgumentException>().WithParameterName("s");

        Action actParseWithProvider = () => EventType.Parse(invalid!, CultureInfo.InvariantCulture);
        actParseWithProvider.Should().Throw<ArgumentException>().WithParameterName("s");
    }

    [Fact]
    public void EventType_Comparison_ShouldBeCaseInsensitive()
    {
        var type1 = EventType.From("orders.created");
        var type2 = EventType.From("ORDERS.CREATED");
        var type3 = EventType.From("orders.updated");

        type1.Equals(type2).Should().BeTrue(); // Invariant: CompareTo == 0 must imply Equals == true
        type1.GetHashCode().Should().Be(type2.GetHashCode());
        type1.CompareTo(type2).Should().Be(0);
        type1.CompareTo((object)type2).Should().Be(0);
        (type1 <= type2).Should().BeTrue();
        (type1 >= type2).Should().BeTrue();
        (type1 < type2).Should().BeFalse();
        (type1 > type2).Should().BeFalse();

        (type1 < type3).Should().BeTrue();
        (type1 <= type3).Should().BeTrue();
        (type3 > type1).Should().BeTrue();
        (type3 >= type1).Should().BeTrue();
        (type1 > type3).Should().BeFalse();
        (type1 >= type3).Should().BeFalse();
        (type3 < type1).Should().BeFalse();
        (type3 <= type1).Should().BeFalse();

        type1.CompareTo(null).Should().Be(1);
        type1.Invoking(x => x.CompareTo(123))
            .Should().Throw<ArgumentException>()
            .WithMessage($"*Object must be of type {nameof(EventType)}*");
    }

    [Fact]
    public void EventType_Parse_And_TryParse_ShouldWork()
    {
        EventType.TryParse("billing.payment", null, out var parsed).Should().BeTrue();
        parsed.Value.Should().Be("billing.payment");

        EventType.TryParse("   ", null, out var failed).Should().BeFalse();
        failed.Should().Be(default);

        EventType.TryParse(null, null, out var failedNull).Should().BeFalse();
        failedNull.Should().Be(default);

        EventType.Parse("shipping.dispatched").Value.Should().Be("shipping.dispatched");
    }

    [Fact]
    public void EventVersion_DefaultAndFactory_ShouldInitializeCorrectly()
    {
        var v1 = EventVersion.V1;
        v1.Value.Should().Be(1u);
        v1.ToString().Should().Be("1");

        EventVersion defaultVersion = default;
        defaultVersion.ToString().Should().Be("1");

        var v2 = EventVersion.From(2u);
        v2.Value.Should().Be(2u);
        ((uint)v2).Should().Be(2u);
        ((EventVersion)2u).Should().Be(v2);

        var v3 = EventVersion.From(3);
        v3.Value.Should().Be(3u);
    }

    [Theory]
    [InlineData(0u)]
    public void EventVersion_Zero_ShouldThrowArgumentOutOfRangeException(uint zero)
    {
        Action act = () => new EventVersion(zero);
        act.Should().Throw<ArgumentOutOfRangeException>()
            .WithMessage("*Event version must be greater than or equal to 1.*");

        Action actFrom = () => EventVersion.From(zero);
        actFrom.Should().Throw<ArgumentOutOfRangeException>()
            .WithMessage("*Event version must be greater than or equal to 1.*");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void EventVersion_NegativeOrZeroInt_ShouldThrowArgumentOutOfRangeException(int invalid)
    {
        Action act = () => EventVersion.From(invalid);
        act.Should().Throw<ArgumentOutOfRangeException>()
            .WithParameterName("value");
    }

    [Fact]
    public void EventVersion_Comparisons_ShouldWork()
    {
        var v1 = EventVersion.From(1);
        var v2 = EventVersion.From(2);
        var v1Copy = EventVersion.From(1);

        (v1 < v2).Should().BeTrue();
        (v1 <= v2).Should().BeTrue();
        (v2 > v1).Should().BeTrue();
        (v2 >= v1).Should().BeTrue();

        (v1 > v2).Should().BeFalse();
        (v1 >= v2).Should().BeFalse();
        (v2 < v1).Should().BeFalse();
        (v2 <= v1).Should().BeFalse();

        (v1 <= v1Copy).Should().BeTrue();
        (v1 >= v1Copy).Should().BeTrue();
        (v1 < v1Copy).Should().BeFalse();
        (v1 > v1Copy).Should().BeFalse();

        v1.CompareTo(v2).Should().BeNegative();
        v1.CompareTo(null).Should().Be(1);
        v1.CompareTo((object)v1Copy).Should().Be(0);
        v1.Invoking(x => x.CompareTo("invalid"))
            .Should().Throw<ArgumentException>()
            .WithMessage($"*Object must be of type {nameof(EventVersion)}*");
    }

    [Fact]
    public void EventVersion_Parse_And_TryParse_ShouldWork()
    {
        EventVersion.TryParse("5", null, out var parsed).Should().BeTrue();
        parsed.Value.Should().Be(5u);

        EventVersion.TryParse("0", null, out var failedZero).Should().BeFalse();
        failedZero.Should().Be(default);

        EventVersion.TryParse("invalid", null, out var failed).Should().BeFalse();
        failed.Should().Be(default);

        EventVersion.TryParse(null, null, out var failedNull).Should().BeFalse();
        failedNull.Should().Be(default);

        EventVersion.TryParse("   ", null, out var failedWhitespace).Should().BeFalse();
        failedWhitespace.Should().Be(default);

        EventVersion.Parse("10", CultureInfo.InvariantCulture).Value.Should().Be(10u);

        Action actNull = () => EventVersion.Parse(null!);
        actNull.Should().Throw<ArgumentException>();

        Action actWhitespace = () => EventVersion.Parse("   ", CultureInfo.InvariantCulture);
        actWhitespace.Should().Throw<ArgumentException>();

        Action actZero = () => EventVersion.Parse("0");
        actZero.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Property]
    public Property EventVersion_RoundtripsAnyPositiveUInt()
    {
        return Prop.ForAll(Arb.Default.PositiveInt(), posInt =>
        {
            var uintVal = (uint)posInt.Get;
            var ver = EventVersion.From(uintVal);
            var parsed = EventVersion.Parse(ver.ToString());
            return ver == parsed && ver.Value == uintVal;
        });
    }
}




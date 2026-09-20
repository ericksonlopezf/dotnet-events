// Copyright © Erickson Lopez. MIT License.
using System;
using System.Threading;

namespace EricksonLopez.Events.UnitTests.Identifiers;

using System.Globalization;
using System.Text;
using AwesomeAssertions;
using EricksonLopez.Events.Identifiers;
using FsCheck;
using FsCheck.Xunit;
using Xunit;

[Xunit.Trait("Category", "Unit")]
public sealed class EventIdTests
{
    [Fact]
    public void EventId_New_ShouldGenerateNonEmptyGuidV7EventId()
    {
        var id = EventId.New();

        id.IsEmpty.Should().BeFalse();
        id.Value.Should().NotBe(Guid.Empty);
        id.Value.Version.Should().Be(7);
    }

    [Fact]
    public void EventId_MonotonicOrderingAndComparisonOperators_ShouldBehaveCorrectly()
    {
        var id1 = EventId.From(new Guid("018f0000-0000-7000-8000-000000000001"));
        var id2 = EventId.From(new Guid("018f0000-0000-7000-8000-000000000002"));

        (id1 < id2).Should().BeTrue();
        (id1 <= id2).Should().BeTrue();
        (id2 > id1).Should().BeTrue();
        (id2 >= id1).Should().BeTrue();
        (id1 > id2).Should().BeFalse();
        (id1 >= id2).Should().BeFalse();
        (id2 < id1).Should().BeFalse();
        (id2 <= id1).Should().BeFalse();
        id1.CompareTo(id2).Should().BeNegative();
        id2.CompareTo(id1).Should().BePositive();
    }

    [Fact]
    public void EventId_ComparisonOperators_WithEqualValues_ShouldBehaveCorrectly()
    {
        var guid = Guid.NewGuid();
        var id1 = new EventId(guid);
        var id2 = new EventId(guid);

        (id1 <= id2).Should().BeTrue();
        (id1 >= id2).Should().BeTrue();
        (id1 < id2).Should().BeFalse();
        (id1 > id2).Should().BeFalse();
        (id1 == id2).Should().BeTrue();
        (id1 != id2).Should().BeFalse();
        id1.CompareTo(id2).Should().Be(0);
    }

    [Fact]
    public void EventId_Empty_ShouldReturnEmptyGuid()
    {
        var id = EventId.Empty;

        id.IsEmpty.Should().BeTrue();
        id.Value.Should().Be(Guid.Empty);
    }

    [Fact]
    public void EventId_From_WithGuid_ShouldWrapCorrectly()
    {
        var guid = Guid.NewGuid();
        var id = EventId.From(guid);

        id.Value.Should().Be(guid);
        ((Guid)id).Should().Be(guid);
        ((EventId)guid).Should().Be(id);
    }

    [Fact]
    public void EventId_TryFormatAndParse_ValidStrings_ShouldRoundtripCorrectly()
    {
        var original = EventId.New();
        var str = original.ToString();
        var formattedWithProvider = original.ToString("D", CultureInfo.InvariantCulture);

        str.Should().Be(formattedWithProvider);

        EventId.TryParse(str, null, out var parsed).Should().BeTrue();
        parsed.Should().Be(original);

        Span<char> buffer = stackalloc char[36];
        original.TryFormat(buffer, out int charsWritten).Should().BeTrue();
        charsWritten.Should().Be(36);
        new string(buffer).Should().Be(str);

        EventId.Parse(str, CultureInfo.InvariantCulture).Should().Be(original);
        EventId.Parse(str.AsSpan(), CultureInfo.InvariantCulture).Should().Be(original);
    }

    [Fact]
    public void EventId_TryFormat_Utf8Destination_ShouldFormatCorrectly()
    {
        var original = EventId.New();
        Span<byte> utf8Buffer = stackalloc byte[36];

        original.TryFormat(utf8Buffer, out int bytesWritten, default, CultureInfo.InvariantCulture).Should().BeTrue();
        bytesWritten.Should().Be(36);

        var utf8String = Encoding.UTF8.GetString(utf8Buffer);
        utf8String.Should().Be(original.ToString());
    }

    [Fact]
    public void EventId_Parse_NullOrInvalid_ShouldThrowException()
    {
        Action actNull = () => EventId.Parse((string)null!);
        actNull.Should().Throw<ArgumentNullException>().WithParameterName("s");

        Action actNullWithProvider = () => EventId.Parse(null!, CultureInfo.InvariantCulture);
        actNullWithProvider.Should().Throw<ArgumentNullException>().WithParameterName("s");

        Action actInvalid = () => EventId.Parse("invalid-guid");
        actInvalid.Should().Throw<FormatException>();

        Action actInvalidSpan = () => EventId.Parse("invalid-guid".AsSpan());
        actInvalidSpan.Should().Throw<FormatException>();
    }

    [Fact]
    public void EventId_TryParse_InvalidStringOrSpan_ShouldReturnFalse()
    {
        EventId.TryParse("not-a-guid", null, out var result).Should().BeFalse();
        result.Should().Be(EventId.Empty);

        EventId.TryParse(null, null, out var resultNull).Should().BeFalse();
        resultNull.Should().Be(EventId.Empty);

        EventId.TryParse("not-a-guid".AsSpan(), null, out var resultSpan).Should().BeFalse();
        resultSpan.Should().Be(EventId.Empty);

        var validGuid = Guid.NewGuid().ToString();
        EventId.TryParse(validGuid.AsSpan(), null, out var validResultSpan).Should().BeTrue();
        validResultSpan.Value.ToString().Should().Be(validGuid);
    }

    [Fact]
    public void EventId_CompareTo_WithObject_ShouldHandleEdgeCases()
    {
        var id1 = EventId.New();

        id1.CompareTo(null).Should().Be(1);
        id1.CompareTo((object)id1).Should().Be(0);
        id1.Invoking(x => x.CompareTo("not-an-eventid"))
            .Should().Throw<ArgumentException>()
            .WithMessage($"*Object must be of type {nameof(EventId)}*");
    }

    [Property]
    public Property EventId_Roundtrip_AnyGuid_ShouldBeConsistent()
    {
        return Prop.ForAll(Arb.Default.Guid(), guid =>
        {
            var eventId = EventId.From(guid);
            var parsed = EventId.Parse(eventId.ToString());
            return eventId == parsed && eventId.Value == guid;
        });
    }
}







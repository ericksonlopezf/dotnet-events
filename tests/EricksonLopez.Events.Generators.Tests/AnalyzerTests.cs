// Copyright © Erickson Lopez. MIT License.
using System;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using AwesomeAssertions;
using EricksonLopez.Events.Contracts;
using EricksonLopez.Events.Generators.Analyzers;
using EricksonLopez.Events.Identifiers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

namespace EricksonLopez.Events.Generators.Tests;

[Trait("Category", "Unit")]
public sealed class AnalyzerTests
{
    #region EventImmutabilityAnalyzer Tests

    [Fact]
    public async Task EventImmutabilityAnalyzer_WhenPropertyHasMutableSetter_ShouldReportELE001()
    {
        var source = @"
namespace TestNamespace
{
    public class MutableDomainEvent : IDomainEvent
    {
        public EventId Id { get; init; }
        public DateTimeOffset OccurredAt { get; init; }
        public string MutableTitle { get; set; } = string.Empty;
    }
}";

        var diagnostics = await RoslynTestBed.RunAnalyzerAsync(new EventImmutabilityAnalyzer(), source);

        diagnostics.Should().ContainSingle();
        var diag = diagnostics[0];
        diag.Id.Should().Be(EventImmutabilityAnalyzer.DiagnosticId);
        diag.Severity.Should().Be(DiagnosticSeverity.Warning);
        diag.GetMessage().Should().Contain("MutableTitle");
    }

    [Fact]
    public async Task EventImmutabilityAnalyzer_WhenRecordOrInitOnly_ShouldNotReportDiagnostic()
    {
        var source = @"
namespace TestNamespace
{
    public sealed record ImmutableDomainEvent(EventId Id, string Title, DateTimeOffset OccurredAt) : IDomainEvent;

    public sealed class InitOnlyEvent : IEvent
    {
        public EventId Id { get; init; }
        public DateTimeOffset OccurredAt { get; init; }
        public string Title { get; init; } = string.Empty;
        public string Computed => Title.ToUpperInvariant();
        public string GetOnlyTitle { get; } = string.Empty;
    }

    public interface ICustomEventInterface : IEvent
    {
        string Title { get; set; }
    }

    public abstract class AbstractEvent : IEvent
    {
        public abstract EventId Id { get; }
        public abstract DateTimeOffset OccurredAt { get; }
        public string MutableInAbstract { get; set; } = string.Empty;
    }

    public class NonEventClassWithMutableProperty
    {
        public string Name { get; set; } = string.Empty;
    }
}";

        var diagnostics = await RoslynTestBed.RunAnalyzerAsync(new EventImmutabilityAnalyzer(), source);
        diagnostics.Should().BeEmpty();
    }

    #endregion

    #region EventAttributeValidationAnalyzer Tests

    [Fact]
    public async Task EventAttributeValidationAnalyzer_WhenVersionZero_ShouldReportELE002()
    {
        var source = @"
namespace TestNamespace
{
    [EventVersion(0)]
    public sealed record InvalidVersionEvent(EventId Id, DateTimeOffset OccurredAt) : IDomainEvent;
}";

        var diagnostics = await RoslynTestBed.RunAnalyzerAsync(new EventAttributeValidationAnalyzer(), source);

        diagnostics.Should().ContainSingle();
        var diag = diagnostics[0];
        diag.Id.Should().Be(EventAttributeValidationAnalyzer.InvalidVersionDiagnosticId);
        diag.Severity.Should().Be(DiagnosticSeverity.Error);
    }

    [Fact]
    public async Task EventAttributeValidationAnalyzer_WhenEmptyEventName_ShouldReportELE003()
    {
        var source = @"
namespace TestNamespace
{
    [EventName(""   "")]
    public sealed record EmptyNameEvent(EventId Id, DateTimeOffset OccurredAt) : IDomainEvent;
}";

        var diagnostics = await RoslynTestBed.RunAnalyzerAsync(new EventAttributeValidationAnalyzer(), source);

        diagnostics.Should().ContainSingle();
        var diag = diagnostics[0];
        diag.Id.Should().Be(EventAttributeValidationAnalyzer.EmptyEventNameDiagnosticId);
        diag.Severity.Should().Be(DiagnosticSeverity.Warning);
    }

    [Fact]
    public async Task EventAttributeValidationAnalyzer_WhenEmptyEventSource_ShouldReportELE004()
    {
        var source = @"
namespace TestNamespace
{
    [EventSource("""")]
    public sealed record EmptySourceEvent(EventId Id, DateTimeOffset OccurredAt) : IDomainEvent;
}";

        var diagnostics = await RoslynTestBed.RunAnalyzerAsync(new EventAttributeValidationAnalyzer(), source);

        diagnostics.Should().ContainSingle();
        var diag = diagnostics[0];
        diag.Id.Should().Be(EventAttributeValidationAnalyzer.EmptyEventSourceDiagnosticId);
        diag.Severity.Should().Be(DiagnosticSeverity.Warning);
    }

    [Fact]
    public async Task EventAttributeValidationAnalyzer_WhenExplicitAttributeSuffixAndInvalidVersion_ShouldReportELE002()
    {
        var source = @"
namespace TestNamespace
{
    [EventVersionAttribute(0)]
    public sealed record SuffixInvalidVersionEvent(EventId Id, DateTimeOffset OccurredAt) : IDomainEvent;
}";

        var diagnostics = await RoslynTestBed.RunAnalyzerAsync(new EventAttributeValidationAnalyzer(), source);

        diagnostics.Should().ContainSingle();
        var diag = diagnostics[0];
        diag.Id.Should().Be(EventAttributeValidationAnalyzer.InvalidVersionDiagnosticId);
        diag.Severity.Should().Be(DiagnosticSeverity.Error);
    }

    [Fact]
    public async Task EventAttributeValidationAnalyzer_WhenValidAttributesOrOtherNamespace_ShouldNotReportDiagnostics()
    {
        var source = @"
namespace OtherNamespace
{
    [System.AttributeUsage(System.AttributeTargets.Class)]
    public class EventNameAttribute : System.Attribute { public EventNameAttribute(string name) {} }
}

namespace TestNamespace
{
    [EventName(""valid.order-created"")]
    [EventVersion(1)]
    [EventSource(""https://ordering.domain.internal"")]
    public sealed record ValidEvent(EventId Id, DateTimeOffset OccurredAt) : IDomainEvent;

    [OtherNamespace.EventName("""")]
    public sealed record OtherAttributeEvent(EventId Id, DateTimeOffset OccurredAt) : IEvent;
}";

        var diagnostics = await RoslynTestBed.RunAnalyzerAsync(new EventAttributeValidationAnalyzer(), source);
        diagnostics.Should().BeEmpty();
    }

    [Fact]
    public async Task EventAttributeValidationAnalyzer_WhenEmptyConstructorArguments_ShouldNotCrash()
    {
        var source = @"
namespace TestNamespace
{
    [EventName]
    [EventVersion]
    [EventSource]
    public sealed record EmptyArgsEvent(EventId Id, DateTimeOffset OccurredAt) : IDomainEvent;
}";

        var diagnostics = await RoslynTestBed.RunAnalyzerAsync(new EventAttributeValidationAnalyzer(), source);
        diagnostics.Should().BeEmpty();
    }

    #endregion

    #region DomainEventLeakAnalyzer Tests

    [Fact]
    public async Task DomainEventLeakAnalyzer_WhenIntegrationEventContainsDomainEventProperty_ShouldReportELE005()
    {
        var source = @"
namespace TestNamespace
{
    public sealed record InternalDomainEvent(EventId Id, DateTimeOffset OccurredAt) : IDomainEvent;

    public sealed record LeakyIntegrationEvent(
        EventId Id,
        InternalDomainEvent NestedDomainEvent,
        DateTimeOffset OccurredAt) : IIntegrationEvent;
}";

        var diagnostics = await RoslynTestBed.RunAnalyzerAsync(new DomainEventLeakAnalyzer(), source);

        diagnostics.Should().ContainSingle();
        var diag = diagnostics[0];
        diag.Id.Should().Be(DomainEventLeakAnalyzer.DiagnosticId);
        diag.Severity.Should().Be(DiagnosticSeverity.Warning);
        diag.GetMessage().Should().Contain("NestedDomainEvent");
    }

    [Fact]
    public async Task DomainEventLeakAnalyzer_WhenIntegrationEventHasPureDtoOrIsAbstractOrInterface_ShouldNotReportDiagnostic()
    {
        var source = @"
namespace TestNamespace
{
    public sealed record InternalDomainEvent(EventId Id, DateTimeOffset OccurredAt) : IDomainEvent;
    public sealed record CustomerDto(Guid Id, string Name);

    public sealed record CleanIntegrationEvent(
        EventId Id,
        CustomerDto Customer,
        DateTimeOffset OccurredAt) : IIntegrationEvent
    {
        public void ExecuteAction(InternalDomainEvent domainEvt) {}
        public int CalculateTotal() => 42;
    }

    public interface IIntegrationEventInterface : IIntegrationEvent
    {
        InternalDomainEvent NestedEvent { get; }
    }

    public abstract class AbstractIntegrationEvent : IIntegrationEvent
    {
        public abstract EventId Id { get; }
        public abstract DateTimeOffset OccurredAt { get; }
        public InternalDomainEvent? AbstractNestedEvent { get; init; }
    }

    public class NonIntegrationClass
    {
        public InternalDomainEvent DomainProp { get; set; } = null!;
    }
}";

        var diagnostics = await RoslynTestBed.RunAnalyzerAsync(new DomainEventLeakAnalyzer(), source);
        diagnostics.Should().BeEmpty();
    }

    [Fact]
    public async Task EventAttributeValidationAnalyzer_WhenParameterlessAttributes_ShouldNotReportDiagnostics()
    {
        var source = @"
namespace CustomAttributes
{
    public class EventNameAttribute : System.Attribute { }
    public class EventVersionAttribute : System.Attribute { }
    public class EventSourceAttribute : System.Attribute { }
}

namespace TestNamespace
{
    [CustomAttributes.EventName]
    [CustomAttributes.EventVersion]
    [CustomAttributes.EventSource]
    public sealed record EmptyArgsEvent(EventId Id, DateTimeOffset OccurredAt) : IDomainEvent;
}";

        var diagnostics = await RoslynTestBed.RunAnalyzerAsync(new EventAttributeValidationAnalyzer(), source);
        diagnostics.Should().BeEmpty();
    }

    [Fact]
    public async Task DomainEventLeakAnalyzer_WhenNonFrameworkIntegrationOrDomainEvents_ShouldNotReportDiagnostic()
    {
        var source = @"
namespace ForeignContracts
{
    public interface IIntegrationEvent { }
    public interface IDomainEvent { }
}

namespace TestNamespace
{
    public sealed record ForeignDomainEvent : ForeignContracts.IDomainEvent;

    public sealed record ForeignIntegrationEvent(
        EventId Id,
        ForeignDomainEvent NestedEvent,
        DateTimeOffset OccurredAt) : ForeignContracts.IIntegrationEvent;

    public sealed record MixedEvent(
        EventId Id,
        ForeignDomainEvent NestedEvent,
        DateTimeOffset OccurredAt) : IIntegrationEvent;
}";

        var diagnostics = await RoslynTestBed.RunAnalyzerAsync(new DomainEventLeakAnalyzer(), source);
        diagnostics.Should().BeEmpty();
    }

    [Fact]
    public async Task DomainEventLeakAnalyzer_WhenDomainEventContainsNestedDomainEvent_ShouldNotReportELE005()
    {
        var source = @"
namespace TestNamespace
{
    public sealed record ChildDomainEvent(EventId Id, DateTimeOffset OccurredAt) : IDomainEvent;

    public sealed record ParentDomainEvent(
        EventId Id,
        ChildDomainEvent Child,
        DateTimeOffset OccurredAt) : IDomainEvent;
}";

        var diagnostics = await RoslynTestBed.RunAnalyzerAsync(new DomainEventLeakAnalyzer(), source);
        diagnostics.Should().BeEmpty("domain events containing other domain events cross no bounded context and are not integration events");
    }

    [Fact]
    public async Task DomainEventLeakAnalyzer_WhenIntegrationEventContainsNonDomainFrameworkEventProperty_ShouldNotReportELE005()
    {
        var source = @"
namespace TestNamespace
{
    public sealed record PlainFrameworkEvent(EventId Id, DateTimeOffset OccurredAt) : IEvent;

    public sealed record IntegrationEventWithPlainEvent(
        EventId Id,
        PlainFrameworkEvent EventPayload,
        DateTimeOffset OccurredAt) : IIntegrationEvent;
}";

        var diagnostics = await RoslynTestBed.RunAnalyzerAsync(new DomainEventLeakAnalyzer(), source);
        diagnostics.Should().BeEmpty("plain IEvent is not an IDomainEvent and must not trigger ELE005");
    }

    [Fact]
    public async Task DomainEventLeakAnalyzer_WhenForeignIntegrationEventContainsFrameworkDomainEvent_ShouldNotReportELE005()
    {
        var source = @"
namespace ForeignContracts
{
    public interface IIntegrationEvent { }
}

namespace TestNamespace
{
    public sealed record FrameworkDomainEvent(EventId Id, DateTimeOffset OccurredAt) : IDomainEvent;

    public sealed record ForeignIntegrationEventWithDomain(
        EventId Id,
        FrameworkDomainEvent DomainProp,
        DateTimeOffset OccurredAt) : ForeignContracts.IIntegrationEvent;
}";

        var diagnostics = await RoslynTestBed.RunAnalyzerAsync(new DomainEventLeakAnalyzer(), source);
        diagnostics.Should().BeEmpty("foreign integration events outside EricksonLopez.Events.Contracts must not trigger ELE005");
    }

    [Fact]
    public async Task EventImmutabilityAnalyzer_WhenNonFrameworkEvent_ShouldNotReportDiagnostic()
    {
        var source = @"
namespace ForeignContracts
{
    public interface IEvent { }
}

namespace TestNamespace
{
    public class MutableForeignEvent : ForeignContracts.IEvent
    {
        public string MutableTitle { get; set; } = string.Empty;
    }
}";

        var diagnostics = await RoslynTestBed.RunAnalyzerAsync(new EventImmutabilityAnalyzer(), source);
        diagnostics.Should().BeEmpty();
    }

    #endregion

    [Fact]
    public async Task EventAttributeValidationAnalyzer_WhenAttributeNameDoesNotEndWithAttribute_ShouldNotStripAndShouldReportCorrectly()
    {
        var source2 = @"
namespace EricksonLopez.Events.Attributes
{
    [System.AttributeUsage(System.AttributeTargets.Class)]
    public sealed class EventName : System.Attribute
    {
        public EventName(string name) { }
    }
}

namespace TestNamespace
{
    [EricksonLopez.Events.Attributes.@EventName("""")]
    public sealed record MyTestEvent(EricksonLopez.Events.Identifiers.EventId Id, System.DateTimeOffset OccurredAt) : EricksonLopez.Events.Contracts.IDomainEvent;
}";
        var diagnostics = await RoslynTestBed.RunAnalyzerAsync(new EventAttributeValidationAnalyzer(), source2);
        diagnostics.Should().ContainSingle();
        diagnostics[0].Id.Should().Be(EventAttributeValidationAnalyzer.EmptyEventNameDiagnosticId);
    }
}

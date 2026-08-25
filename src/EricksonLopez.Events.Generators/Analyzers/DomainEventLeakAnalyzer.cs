// Copyright © Erickson Lopez. MIT License.
using System;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace EricksonLopez.Events.Generators.Analyzers;

/// <summary>
/// Provides diagnostic analysis detecting domain events embedded directly in integration event contracts.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class DomainEventLeakAnalyzer : DiagnosticAnalyzer
{
    /// <summary>
    /// Gets the diagnostic identifier for domain event leaks into integration events.
    /// </summary>
    public const string DiagnosticId = "ELE005";

    private const string Category = "DDD.Architecture";

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId,
        "Domain event leaked in integration event contract",
        "Integration event '{0}' contains property '{1}' of domain event type '{2}'. Domain events should not be directly embedded inside integration event public contracts.",
        Category,
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "Integration events cross bounded contexts and should use independent DTO payloads rather than internal domain event types.");

    /// <inheritdoc />
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

    /// <inheritdoc />
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        context.RegisterSymbolAction(AnalyzeNamedType, SymbolKind.NamedType);
    }

    private static void AnalyzeNamedType(SymbolAnalysisContext context)
    {
        var namedType = (INamedTypeSymbol)context.Symbol;

        if (namedType.TypeKind == TypeKind.Interface || namedType.IsAbstract)
        {
            return;
        }

        bool isIntegrationEvent = namedType.AllInterfaces.Any(static i =>
            i.Name == "IIntegrationEvent" &&
            i.ContainingNamespace.ToDisplayString() == "EricksonLopez.Events.Contracts");

        if (!isIntegrationEvent)
        {
            return;
        }

        foreach (var member in namedType.GetMembers())
        {
            if (member is IPropertySymbol property && property.Type is INamedTypeSymbol propertyType)
            {
                bool isDomainEvent = propertyType.AllInterfaces.Any(static i =>
                    i.Name == "IDomainEvent" &&
                    i.ContainingNamespace.ToDisplayString() == "EricksonLopez.Events.Contracts");

                if (isDomainEvent)
                {
                    var location = property.Locations[0];
                    context.ReportDiagnostic(Diagnostic.Create(
                        Rule,
                        location,
                        namedType.Name,
                        property.Name,
                        propertyType.Name));
                }
            }
        }
    }
}

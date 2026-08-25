// Copyright © Erickson Lopez. MIT License.
using System;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace EricksonLopez.Events.Generators.Analyzers;

/// <summary>
/// Provides diagnostic analysis enforcing immutability on types implementing event contracts.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class EventImmutabilityAnalyzer : DiagnosticAnalyzer
{
    /// <summary>
    /// Gets the diagnostic identifier for mutable event properties.
    /// </summary>
    public const string DiagnosticId = "ELE001";

    private const string Category = "DDD.Design";
    private const string IEventFqn = "EricksonLopez.Events.Contracts.IEvent";

    private static readonly LocalizableString Title = "Event types must be immutable";
    private static readonly LocalizableString MessageFormat = "Type '{0}' implements '{1}' but has mutable property '{2}' with a mutable setter. Events must be immutable records or have get/init-only properties.";
    private static readonly LocalizableString Description = "Domain events and integration events represent facts that have happened in the past and must be immutable.";

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId,
        Title,
        MessageFormat,
        Category,
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: Description);

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

        var eventInterface = namedType.AllInterfaces.FirstOrDefault(static i =>
            i.Name == "IEvent" &&
            i.ContainingNamespace.ToDisplayString() == "EricksonLopez.Events.Contracts");

        if (eventInterface == null)
        {
            return;
        }

        foreach (var member in namedType.GetMembers())
        {
            if (member is IPropertySymbol property)
            {
                if (property.SetMethod != null &&
                    !property.SetMethod.IsInitOnly &&
                    property.DeclaredAccessibility == Accessibility.Public)
                {
                    var location = property.Locations[0];
                    var diagnostic = Diagnostic.Create(
                        Rule,
                        location,
                        namedType.Name,
                        eventInterface.Name,
                        property.Name);

                    context.ReportDiagnostic(diagnostic);
                }
            }
        }
    }
}

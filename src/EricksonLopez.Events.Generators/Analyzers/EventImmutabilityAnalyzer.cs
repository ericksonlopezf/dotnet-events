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

    /// <summary>
    /// Gets the diagnostic identifier for mutable collection properties in event types.
    /// </summary>
    public const string MutableCollectionDiagnosticId = "ELE006";

    private const string Category = "DDD.Design";
    private const string IEventFqn = "EricksonLopez.Events.Contracts.IEvent";

    private static readonly LocalizableString Title = "Event types must be immutable";
    private static readonly LocalizableString MessageFormat = "Type '{0}' implements '{1}' but has mutable member '{2}'. Events must be immutable records or have get/init-only properties and readonly fields.";
    private static readonly LocalizableString Description = "Domain events and integration events represent facts that have happened in the past and must be immutable.";

    private static readonly LocalizableString MutableCollectionTitle = "Event properties should use immutable collection types";
    private static readonly LocalizableString MutableCollectionMessageFormat = "Property '{0}' in event '{1}' has mutable collection type '{2}'. Event properties should use immutable collections (e.g., IReadOnlyList<T>, ImmutableArray<T>) to prevent payload tampering across handlers.";
    private static readonly LocalizableString MutableCollectionDescription = "Events are shared in-memory across handlers by reference. Mutable collection types allow handlers to mutate the payload, violating deep immutability.";

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId,
        Title,
        MessageFormat,
        Category,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: Description);

    private static readonly DiagnosticDescriptor MutableCollectionRule = new(
        MutableCollectionDiagnosticId,
        MutableCollectionTitle,
        MutableCollectionMessageFormat,
        Category,
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: MutableCollectionDescription);

    /// <inheritdoc />
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule, MutableCollectionRule);

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
                if (property.DeclaredAccessibility == Accessibility.Public)
                {
                    if (property.SetMethod != null && !property.SetMethod.IsInitOnly)
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

                    if (IsMutableCollection(property.Type))
                    {
                        var location = property.Locations[0];
                        var diagnostic = Diagnostic.Create(
                            MutableCollectionRule,
                            location,
                            property.Name,
                            namedType.Name,
                            property.Type.ToDisplayString());

                        context.ReportDiagnostic(diagnostic);
                    }
                }
            }
            else if (member is IFieldSymbol field)
            {
                if (field.DeclaredAccessibility == Accessibility.Public &&
                    !field.IsReadOnly &&
                    !field.IsConst)
                {
                    var location = field.Locations[0];
                    var diagnostic = Diagnostic.Create(
                        Rule,
                        location,
                        namedType.Name,
                        eventInterface.Name,
                        field.Name);

                    context.ReportDiagnostic(diagnostic);
                }
            }
        }
    }

    private static bool IsMutableCollection(ITypeSymbol type)
    {
        if (type is IArrayTypeSymbol)
        {
            return true;
        }

        if (type is not INamedTypeSymbol namedType)
        {
            return false;
        }

        var ns = namedType.ContainingNamespace?.ToDisplayString();
        if (ns is "System.Collections.Generic" or "System.Collections.ObjectModel")
        {
            return namedType.Name is "List" or "Dictionary" or "HashSet" or "Collection"
                or "Queue" or "Stack" or "SortedSet" or "SortedDictionary" or "LinkedList";
        }

        return false;
    }
}

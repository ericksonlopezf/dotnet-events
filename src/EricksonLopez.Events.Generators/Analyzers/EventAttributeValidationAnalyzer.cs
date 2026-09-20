// Copyright © Erickson Lopez. MIT License.
using System;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace EricksonLopez.Events.Generators.Analyzers;

/// <summary>
/// Provides diagnostic analysis validating event metadata attributes.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class EventAttributeValidationAnalyzer : DiagnosticAnalyzer
{
    /// <summary>
    /// Gets the diagnostic identifier for an invalid event version.
    /// </summary>
    public const string InvalidVersionDiagnosticId = "ELE002";

    /// <summary>
    /// Gets the diagnostic identifier for an empty event name.
    /// </summary>
    public const string EmptyEventNameDiagnosticId = "ELE003";

    /// <summary>
    /// Gets the diagnostic identifier for an empty event source.
    /// </summary>
    public const string EmptyEventSourceDiagnosticId = "ELE004";

    private const string Category = "DDD.Design";

    private static readonly DiagnosticDescriptor InvalidVersionRule = new(
        InvalidVersionDiagnosticId,
        "Invalid event version in [EventVersion]",
        "Event version on type '{0}' must be greater than or equal to 1, but got {1}",
        Category,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "EventVersion is 1-based and cannot be zero.");

    private static readonly DiagnosticDescriptor EmptyEventNameRule = new(
        EmptyEventNameDiagnosticId,
        "Empty event name in [EventName]",
        "[EventName] on type '{0}' must not be null, empty, or whitespace",
        Category,
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "Event types must have meaningful non-empty semantic names.");

    private static readonly DiagnosticDescriptor EmptyEventSourceRule = new(
        EmptyEventSourceDiagnosticId,
        "Empty event source in [EventSource]",
        "[EventSource] on type '{0}' must not be null, empty, or whitespace",
        Category,
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "Event source attribute must contain a non-empty string or URI identifier.");

    /// <inheritdoc />
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(InvalidVersionRule, EmptyEventNameRule, EmptyEventSourceRule);

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

        foreach (var attr in namedType.GetAttributes())
        {
            var attrClass = attr.AttributeClass;
            if (attrClass == null || attrClass.ContainingNamespace.ToDisplayString() != "EricksonLopez.Events.Attributes")
            {
                continue;
            }

            var attrName = attrClass.Name;
            var shortName = attrName.EndsWith("Attribute", StringComparison.Ordinal)
                ? attrName.Substring(0, attrName.Length - "Attribute".Length)
                : attrName;

            var location = attr.ApplicationSyntaxReference!.GetSyntax(context.CancellationToken).GetLocation();

            switch (shortName)
            {
                case "EventVersion":
                    if (attr.ConstructorArguments.Length > 0 &&
                        attr.ConstructorArguments[0].Value is uint ver &&
                        ver == 0)
                    {
                        context.ReportDiagnostic(Diagnostic.Create(InvalidVersionRule, location, namedType.Name, ver));
                    }
                    break;

                case "EventName":
                    if (attr.ConstructorArguments.Length > 0)
                    {
                        var val = attr.ConstructorArguments[0].Value as string;
                        if (string.IsNullOrWhiteSpace(val))
                        {
                            context.ReportDiagnostic(Diagnostic.Create(EmptyEventNameRule, location, namedType.Name));
                        }
                    }
                    break;

                case "EventSource":
                    if (attr.ConstructorArguments.Length > 0)
                    {
                        var val = attr.ConstructorArguments[0].Value as string;
                        if (string.IsNullOrWhiteSpace(val))
                        {
                            context.ReportDiagnostic(Diagnostic.Create(EmptyEventSourceRule, location, namedType.Name));
                        }
                    }
                    break;
            }
        }
    }
}

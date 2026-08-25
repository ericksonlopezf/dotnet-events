// Copyright © Erickson Lopez. MIT License.
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace EricksonLopez.Events.Generators;

/// <summary>
/// Provides incremental source generation for discovering event types, handlers, and emitting compile-time registries.
/// </summary>
[Generator]
public sealed class EventIncrementalGenerator : IIncrementalGenerator
{
    /// <inheritdoc />
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // 1. Discover Event Types
        var eventTypes = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (s, _) => s is TypeDeclarationSyntax tds && tds.Modifiers.Any(SyntaxKind.PublicKeyword),
                transform: static (ctx, cancellationToken) => GetEventModel(ctx, cancellationToken))
            .Where(static m => m is not null);

        var collectedEvents = eventTypes.Collect();

        context.RegisterSourceOutput(collectedEvents, static (spc, events) => ExecuteEvents(spc, events!));

        // 2. Discover Event Handler Types
        var handlerTypes = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (s, _) => s is ClassDeclarationSyntax or RecordDeclarationSyntax,
                transform: static (ctx, cancellationToken) => GetHandlerModel(ctx, cancellationToken))
            .Where(static m => m is not null);

        var collectedHandlers = handlerTypes.Collect();

        context.RegisterSourceOutput(collectedHandlers, static (spc, handlers) => ExecuteHandlers(spc, handlers!));
    }

    private static EventTypeModel? GetEventModel(GeneratorSyntaxContext ctx, CancellationToken cancellationToken)
    {
        var typeSyntax = (TypeDeclarationSyntax)ctx.Node;

        if (ctx.SemanticModel.GetDeclaredSymbol(typeSyntax, cancellationToken) is not INamedTypeSymbol symbol ||
            symbol.IsAbstract)
        {
            return null;
        }

        bool implementsIEvent = symbol.AllInterfaces.Any(static i =>
            i.Name == "IEvent" &&
            i.ContainingNamespace.ToDisplayString() == "EricksonLopez.Events.Contracts");

        string? eventName = null;
        uint version = 1;
        string? source = null;

        foreach (var attr in symbol.GetAttributes())
        {
            var attrName = attr.AttributeClass!.Name;
            var shortName = attrName.EndsWith("Attribute", StringComparison.Ordinal)
                ? attrName.Substring(0, attrName.Length - "Attribute".Length)
                : attrName;

            switch (shortName)
            {
                case "EventName":
                    var nameVal = GetAttributeArgumentValue(attr, cancellationToken);
                    if (!string.IsNullOrEmpty(nameVal))
                    {
                        eventName = nameVal;
                    }
                    break;

                case "EventVersion":
                    var verStr = GetAttributeArgumentValue(attr, cancellationToken);
                    if (uint.TryParse(verStr, out var verVal) && verVal > 0)
                    {
                        version = verVal;
                    }
                    break;

                case "EventSource":
                    var srcVal = GetAttributeArgumentValue(attr, cancellationToken);
                    if (!string.IsNullOrEmpty(srcVal))
                    {
                        source = srcVal;
                    }
                    break;
            }
        }

        if (!implementsIEvent)
        {
            if (eventName is null) return null;
        }

        eventName ??= symbol.Name;
        var fullTypeName = symbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

        return new EventTypeModel(fullTypeName, eventName, version, source);
    }

    private static HandlerTypeModel? GetHandlerModel(GeneratorSyntaxContext ctx, CancellationToken cancellationToken)
    {
        var typeSyntax = (TypeDeclarationSyntax)ctx.Node;

        if (ctx.SemanticModel.GetDeclaredSymbol(typeSyntax, cancellationToken) is not INamedTypeSymbol symbol ||
            symbol.IsAbstract ||
            symbol.IsGenericType)
        {
            return null;
        }

        var handledEventTypes = new List<string>();

        foreach (var iface in symbol.AllInterfaces)
        {
            if (iface.Name == "IEventHandler" &&
                iface.ContainingNamespace.ToDisplayString() == "EricksonLopez.Events.Contracts" &&
                iface.TypeArguments.Length == 1)
            {
                var eventTypeSymbol = iface.TypeArguments[0];
                handledEventTypes.Add(eventTypeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat));
            }
        }

        if (handledEventTypes.Count == 0)
        {
            return null;
        }

        var fullHandlerTypeName = symbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
        return new HandlerTypeModel(fullHandlerTypeName, handledEventTypes.Distinct().ToList());
    }

    private static string? GetAttributeArgumentValue(AttributeData attr, CancellationToken ct)
    {
        if (attr.ConstructorArguments.Length > 0)
        {
            return attr.ConstructorArguments[0].Value?.ToString();
        }

        if (attr.ApplicationSyntaxReference is { } syntaxRef &&
            syntaxRef.GetSyntax(ct) is AttributeSyntax syntax &&
            syntax.ArgumentList is { Arguments.Count: > 0 } argList &&
            argList.Arguments[0].Expression is LiteralExpressionSyntax lit &&
            !lit.IsKind(SyntaxKind.NullLiteralExpression))
        {
            return lit.Token.ValueText;
        }

        return null;
    }

    private static void ExecuteEvents(SourceProductionContext spc, ImmutableArray<EventTypeModel> events)
    {
        if (events.IsDefaultOrEmpty)
        {
            return;
        }

        var distinctEvents = events.Distinct().OrderBy(static e => e.FullTypeName, StringComparer.Ordinal).ToList();

        var sb = new StringBuilder();
        sb.AppendLine("// <auto-generated />");
        sb.AppendLine("#nullable enable");
        sb.AppendLine();
        sb.AppendLine("namespace EricksonLopez.Events.Generated;");
        sb.AppendLine();
        sb.AppendLine("using System;");
        sb.AppendLine("using System.Collections.Generic;");
        sb.AppendLine("using EricksonLopez.Events.Identifiers;");
        sb.AppendLine("using EricksonLopez.Events.Registry;");
        sb.AppendLine();
        sb.AppendLine("/// <summary>");
        sb.AppendLine("/// Compile-time generated event registry containing zero-reflection descriptors.");
        sb.AppendLine("/// </summary>");
        sb.AppendLine("public static class GeneratedEventRegistry");
        sb.AppendLine("{");
        sb.AppendLine("    /// <summary>");
        sb.AppendLine("    /// Creates an immutable <see cref=\"IEventTypeRegistry\"/> with all compile-time discovered event descriptors.");
        sb.AppendLine("    /// </summary>");
        sb.AppendLine("    public static IEventTypeRegistry CreateRegistry()");
        sb.AppendLine("    {");
        sb.AppendLine($"        var descriptors = new List<EventTypeDescriptor>({distinctEvents.Count});");
        sb.AppendLine();

        foreach (var ev in distinctEvents)
        {
            var sourceStr = ev.Source is not null ? $"\"{ev.Source}\"" : "null";
            sb.AppendLine("        descriptors.Add(new EventTypeDescriptor(");
            sb.AppendLine($"            typeof({ev.FullTypeName}),");
            sb.AppendLine($"            new EventType(\"{ev.EventName}\"),");
            sb.AppendLine($"            new EventVersion({ev.Version}u),");
            sb.AppendLine($"            {sourceStr}));");
            sb.AppendLine();
        }

        sb.AppendLine("        return new EventTypeRegistry(descriptors);");
        sb.AppendLine("    }");
        sb.AppendLine("}");

        spc.AddSource("GeneratedEventRegistry.g.cs", SourceText.From(sb.ToString(), Encoding.UTF8));
    }

    private static void ExecuteHandlers(SourceProductionContext spc, ImmutableArray<HandlerTypeModel> handlers)
    {
        if (handlers.IsDefaultOrEmpty)
        {
            return;
        }

        var distinctHandlers = handlers.Distinct().OrderBy(static h => h.FullHandlerTypeName, StringComparer.Ordinal).ToList();

        var sb = new StringBuilder();
        sb.AppendLine("// <auto-generated />");
        sb.AppendLine("#nullable enable");
        sb.AppendLine();
        sb.AppendLine("namespace Microsoft.Extensions.DependencyInjection;");
        sb.AppendLine();
        sb.AppendLine("using System;");
        sb.AppendLine("using EricksonLopez.Events.Contracts;");
        sb.AppendLine("using Microsoft.Extensions.DependencyInjection;");
        sb.AppendLine();
        sb.AppendLine("/// <summary>");
        sb.AppendLine("/// Compile-time generated DI extension methods for discovered event handlers.");
        sb.AppendLine("/// </summary>");
        sb.AppendLine("public static class GeneratedEventServiceCollectionExtensions");
        sb.AppendLine("{");
        sb.AppendLine("    /// <summary>");
        sb.AppendLine("    /// Automatically registers all compile-time discovered event handlers in the service collection.");
        sb.AppendLine("    /// </summary>");
        sb.AppendLine("    /// <param name=\"services\">The service collection.</param>");
        sb.AppendLine("    /// <param name=\"lifetime\">The service lifetime (defaults to Transient).</param>");
        sb.AppendLine("    /// <returns>The service collection for chaining.</returns>");
        sb.AppendLine("    public static IServiceCollection AddGeneratedEventHandlers(");
        sb.AppendLine("        this IServiceCollection services,");
        sb.AppendLine("        ServiceLifetime lifetime = ServiceLifetime.Transient)");
        sb.AppendLine("    {");
        sb.AppendLine("        ArgumentNullException.ThrowIfNull(services);");
        sb.AppendLine();

        foreach (var handler in distinctHandlers)
        {
            foreach (var eventType in handler.HandledEventTypes)
            {
                sb.AppendLine($"        services.Add(new ServiceDescriptor(");
                sb.AppendLine($"            typeof(IEventHandler<{eventType}>),");
                sb.AppendLine($"            typeof({handler.FullHandlerTypeName}),");
                sb.AppendLine("            lifetime));");
                sb.AppendLine();
            }
        }

        sb.AppendLine("        return services;");
        sb.AppendLine("    }");
        sb.AppendLine("}");

        spc.AddSource("GeneratedEventServiceCollectionExtensions.g.cs", SourceText.From(sb.ToString(), Encoding.UTF8));
    }

    private sealed record EventTypeModel(
        string FullTypeName,
        string EventName,
        uint Version,
        string? Source);

    private sealed record HandlerTypeModel(
        string FullHandlerTypeName,
        List<string> HandledEventTypes);
}

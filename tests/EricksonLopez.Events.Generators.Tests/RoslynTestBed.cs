// Copyright © Erickson Lopez. MIT License.
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using EricksonLopez.Events.Contracts;
using EricksonLopez.Events.Identifiers;
using EricksonLopez.Events.Registry;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

namespace EricksonLopez.Events.Generators.Tests;

/// <summary>
/// Centralized test bed and compilation harness for Roslyn Source Generators and DDD Analyzers.
/// </summary>
public static class RoslynTestBed
{
    private static readonly Lazy<MetadataReference[]> CachedReferences = new(() =>
    {
        var assemblies = new[]
        {
            typeof(object).Assembly,
            typeof(IEvent).Assembly,
            typeof(EventId).Assembly,
            typeof(EventTypeRegistry).Assembly,
            typeof(Microsoft.Extensions.DependencyInjection.IServiceCollection).Assembly,
            typeof(Microsoft.Extensions.DependencyInjection.ServiceCollection).Assembly,
            typeof(IServiceProvider).Assembly,
            typeof(ValueTask).Assembly,
            Assembly.Load("System.Runtime"),
            Assembly.Load("System.Collections"),
            Assembly.Load("System.Threading.Tasks"),
            Assembly.Load("System.ComponentModel")
        };

        return assemblies.Select(a => MetadataReference.CreateFromFile(a.Location)).ToArray();
    });

    private static readonly CSharpCompilationOptions DefaultCompilationOptions =
        new(OutputKind.DynamicallyLinkedLibrary);

    /// <summary>
    /// Gets the shared metadata references required for compiling domain event sources.
    /// </summary>
    public static MetadataReference[] GetReferences() => CachedReferences.Value;

    /// <summary>
    /// Ensures common namespaces are imported in the provided C# source code.
    /// </summary>
    public static string EnsureUsings(string source)
    {
        var usings = "";
        if (!source.Contains("using System;")) usings += "using System;\n";
        if (!source.Contains("using EricksonLopez.Events;")) usings += "using EricksonLopez.Events;\n";
        if (!source.Contains("using EricksonLopez.Events.Attributes;")) usings += "using EricksonLopez.Events.Attributes;\n";
        if (!source.Contains("using EricksonLopez.Events.Contracts;")) usings += "using EricksonLopez.Events.Contracts;\n";
        if (!source.Contains("using EricksonLopez.Events.Identifiers;")) usings += "using EricksonLopez.Events.Identifiers;\n";
        return usings.Length > 0 ? usings + source : source;
    }

    /// <summary>
    /// Creates a Roslyn CSharpCompilation for the provided source code.
    /// </summary>
    public static CSharpCompilation CreateCompilation(
        string source,
        string assemblyName = "TestCompilation",
        IEnumerable<MetadataReference>? references = null)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(EnsureUsings(source));
        return CreateCompilation(new[] { syntaxTree }, assemblyName, references);
    }

    /// <summary>
    /// Creates a Roslyn CSharpCompilation for the provided syntax trees.
    /// </summary>
    public static CSharpCompilation CreateCompilation(
        IEnumerable<SyntaxTree> syntaxTrees,
        string assemblyName = "TestCompilation",
        IEnumerable<MetadataReference>? references = null)
    {
        return CSharpCompilation.Create(
            assemblyName,
            syntaxTrees,
            references ?? GetReferences(),
            DefaultCompilationOptions);
    }

    /// <summary>
    /// Runs an incremental generator against the given compilation and returns the driver run result.
    /// </summary>
    public static GeneratorDriverRunResult RunGenerator(
        IIncrementalGenerator generator,
        CSharpCompilation compilation,
        out Compilation outputCompilation,
        out ImmutableArray<Diagnostic> diagnostics)
    {
        GeneratorDriver driver = CSharpGeneratorDriver.Create(generator);
        driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out outputCompilation, out diagnostics);
        return driver.GetRunResult();
    }

    /// <summary>
    /// Executes a Roslyn DiagnosticAnalyzer against the given source string and returns reported diagnostics.
    /// </summary>
    public static async Task<ImmutableArray<Diagnostic>> RunAnalyzerAsync(
        DiagnosticAnalyzer analyzer,
        string source,
        CancellationToken cancellationToken = default)
    {
        var compilation = CreateCompilation(source, "AnalyzerTestCompilation");
        var compilationWithAnalyzers = compilation.WithAnalyzers(ImmutableArray.Create(analyzer));
        return await compilationWithAnalyzers.GetAnalyzerDiagnosticsAsync(cancellationToken);
    }

    /// <summary>
    /// Emits a compilation to an in-memory byte array and loads the dynamic Assembly.
    /// </summary>
    public static Assembly EmitAndLoadAssembly(Compilation compilation)
    {
        using var peStream = new MemoryStream();
        var emitResult = compilation.Emit(peStream);
        if (!emitResult.Success)
        {
            var errors = string.Join("\n", emitResult.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error));
            throw new InvalidOperationException($"Compilation emit failed:\n{errors}");
        }

        return Assembly.Load(peStream.ToArray());
    }

    /// <summary>
    /// Normalizes newline characters and whitespace for deterministic source assertions.
    /// </summary>
    public static string NormalizeCode(string code) =>
        code.Replace("\r\n", "\n").Trim();
}

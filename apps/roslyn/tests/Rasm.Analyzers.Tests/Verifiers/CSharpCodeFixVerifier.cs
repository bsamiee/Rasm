using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing;

namespace Rasm.Analyzers.Tests.Verifiers;

// --- [SERVICES] ------------------------------------------------------------------------

public sealed class CSharpCodeFixTest<TAnalyzer, TCodeFix> : CSharpCodeFixTest<TAnalyzer, TCodeFix, DefaultVerifier>
    where TAnalyzer : DiagnosticAnalyzer, new()
    where TCodeFix : CodeFixProvider, new() {
    public CSharpCodeFixTest() {
        ReferenceAssemblies = AdditionalMetadataReferences.Framework;
        DisabledDiagnostics.AddRange(["CS1701", "CS1702"]);
        TestState.AdditionalReferences.AddRange(AdditionalMetadataReferences.Packages);
        TestState.DocumentationMode = DocumentationMode.Parse;
        CompilerDiagnostics = CompilerDiagnostics.Warnings;
        CodeActionValidationMode = CodeActionValidationMode.Full;
        CodeFixTestBehaviors = new TAnalyzer().SupportedDiagnostics.Any(static rule => rule.CustomTags.Contains(WellKnownDiagnosticTags.CompilationEnd, StringComparer.Ordinal))
            ? CodeFixTestBehaviors.SkipLocalDiagnosticCheck : CodeFixTestBehaviors.None;
        TestBehaviors = TestBehaviors.SkipGeneratedSourcesCheck;
    }

    public IList<DiagnosticAnalyzer> AdditionalAnalyzers { get; } = [];

    protected override CompilationOptions CreateCompilationOptions() =>
        ((CSharpCompilationOptions)base.CreateCompilationOptions()).WithNullableContextOptions(NullableContextOptions.Enable);

    protected override IEnumerable<DiagnosticAnalyzer> GetDiagnosticAnalyzers() => [.. base.GetDiagnosticAnalyzers(), .. AdditionalAnalyzers];

    protected override IEnumerable<Type> GetSourceGenerators() => AdditionalMetadataReferences.Generators;
}

public abstract class CSharpCodeFixVerifier<TAnalyzer, TCodeFix> : CodeFixVerifier<TAnalyzer, TCodeFix, CSharpCodeFixTest<TAnalyzer, TCodeFix>, DefaultVerifier>
    where TAnalyzer : DiagnosticAnalyzer, new()
    where TCodeFix : CodeFixProvider, new();

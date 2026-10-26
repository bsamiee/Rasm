using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Rasm.Analyzers.Rhino;

namespace Rasm.Analyzers.Render;

// --- [COMPOSITION] ---------------------------------------------------------------------
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class RenderAnalyzer : RasmAnalyzer {
    // --- [RULES]
    public static readonly DiagnosticDescriptor Rendering = Rule(
        "RASM2105",
        RhinoAnalyzer.Library,
        "RenderSettings written outside RenderSettingsSources",
        "`{0}` writes `RhinoDoc.RenderSettings` with no undo record. Write through `RenderSettingsSources.Edit`",
        "`RhinoDoc.RenderSettings` assignment outside `RenderSettingsSources`.");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rendering];

    protected override void Start(CompilationStartAnalysisContext context) {
        if (context.Compilation.GetTypeByMetadataName("Rasm.Rhino.Render.Settings.RenderSettingsSources") is { } sources)
            Writes(context, "P:Rhino.RhinoDoc.RenderSettings", Rendering, (_, analysis) => !Within(analysis.ContainingSymbol, sources));
    }
}

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using Rasm.Analyzers.Rhino;

namespace Rasm.Analyzers.Render;

// --- [COMPOSITION] ---------------------------------------------------------------------
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class RenderAnalyzer : RasmAnalyzer {
    // --- [RULES]
    public static readonly DiagnosticDescriptor Override = Rule(
        "RASM2007",
        RhinoAnalyzer.Host,
        "SetDisplayModeOverride result read",
        "`{0}` reads a result Rhino returns as false after storing the override. Discard it with `_ =`",
        "`ObjectAttributes.SetDisplayModeOverride` result consumed by anything but a discard or an expression statement.");

    public static readonly DiagnosticDescriptor Grayscale = Rule(
        "RASM2009",
        RhinoAnalyzer.Host,
        "GrayscaleDib result read without a null check",
        "`{0}` returns null on macOS on every call. Pass it to `Refused.UnlessFound`, `Optional`, or `DisposalOps.Found`",
        "`ZBufferCapture.GrayscaleDib` result consumed outside the first argument of `Refused.UnlessFound` or `Optional` and the acquire of `DisposalOps.Found`.");

    public static readonly DiagnosticDescriptor Mode = Rule(
        "RASM2012",
        RhinoAnalyzer.Host,
        "Owned DisplayModeDescription left undisposed",
        "`{0}` returns a new `DisplayModeDescription` the caller owns, and no release reaches it. Acquire it through `DisposalOps.Found`",
        "`DisplayModeDescription.GetDisplayMode`, `DisplayModeDescription.FindByName`, or `RhinoViewport.DisplayMode` read reaching no `Dispose`, `using`, `DisposalOps` acquire or release, `new Disposal<T>`, or `retain` call.");

    public static readonly DiagnosticDescriptor Toggled = Rule(
        "RASM2104",
        RhinoAnalyzer.Library,
        "Conduit toggled outside Conduits",
        "`{0}` toggles a conduit outside `Conduits`, and a disabled conduit keeps its pixels until a redraw. Enable it through `Conduits.Enable`",
        "`DisplayConduit.Enabled` write outside `Conduits`.");

    public static readonly DiagnosticDescriptor Rendering = Rule(
        "RASM2105",
        RhinoAnalyzer.Library,
        "RenderSettings written outside RenderSettingsSources",
        "`{0}` writes `RhinoDoc.RenderSettings` outside `RenderSettingsSources`, and Rhino records no undo for the write. Write through `RenderSettingsSources.Edit`",
        "`RhinoDoc.RenderSettings` assignment outside `RenderSettingsSources`.");

    private static readonly ImmutableArray<(string Property, string Owner, DiagnosticDescriptor Rule)> Writers = [
        ("P:Rhino.Display.DisplayConduit.Enabled", "Rasm.Rhino.Display.Conduits", Toggled),
        ("P:Rhino.RhinoDoc.RenderSettings", "Rasm.Rhino.Render.Settings.RenderSettingsSources", Rendering),
    ];

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Override, Grayscale, Mode, Toggled, Rendering];

    protected override void Start(CompilationStartAnalysisContext context) {
        Overrides(context);
        Captures(context);
        if (context.Compilation.GetTypeByMetadataName("Rhino.Display.DisplayModeDescription") is { } description && Lifetimes.Of(context.Compilation) is { } lifetimes) Modes(context, lifetimes, description);
        foreach ((string property, string owner, DiagnosticDescriptor rule) in Writers) Writes(context, property, owner, rule);
    }

    // --- [OVERRIDES]
    private static void Overrides(CompilationStartAnalysisContext context) {
        ImmutableArray<ISymbol> setters = Declared(
            context.Compilation,
            "M:Rhino.DocObjects.ObjectAttributes.SetDisplayModeOverride(Rhino.Display.DisplayModeDescription)",
            "M:Rhino.DocObjects.ObjectAttributes.SetDisplayModeOverride(Rhino.Display.DisplayModeDescription,System.Guid)");
        if (setters.IsEmpty) return;
        Report<IInvocationOperation>(context, [OperationKind.Invocation], (invocation, _) =>
            Among(invocation.TargetMethod, setters) && Consumer(invocation) is not (IExpressionStatementOperation or ISimpleAssignmentOperation { Target: IDiscardOperation })
                ? [Diagnostic.Create(Override, invocation.Syntax.GetLocation(), invocation.Syntax)]
                : []);
    }

    // --- [CAPTURES]
    private static void Captures(CompilationStartAnalysisContext context) {
        Compilation compilation = context.Compilation;
        if (Declared(compilation, "M:Rhino.Display.ZBufferCapture.GrayscaleDib").FirstOrDefault() is not { } grayscale) return;
        ImmutableArray<IMethodSymbol> checks = [
            .. Members(compilation.GetTypeByMetadataName("Rasm.Rhino.Refused"), "UnlessFound"), .. Members(compilation.GetTypeByMetadataName("LanguageExt.Prelude"), "Optional"),
        ];
        ImmutableArray<IMethodSymbol> found = Members(compilation.GetTypeByMetadataName("Rasm.Rhino.DisposalOps"), "Found");
        static bool First(IOperation? consumer, ImmutableArray<IMethodSymbol> owners) =>
            consumer is IArgumentOperation { Parameter.Ordinal: 0, Parent: IInvocationOperation call } && Among(call.TargetMethod, owners);
        Report<IInvocationOperation>(context, [OperationKind.Invocation], (invocation, _) =>
            Same(invocation.TargetMethod, grayscale) && Consumer(invocation) is var consumer && consumer is not IExpressionStatementOperation && !First(consumer, checks)
            && !(consumer is IReturnOperation { Parent: IBlockOperation { Parent: IAnonymousFunctionOperation function } } && Consumer(function) is IDelegateCreationOperation creation && First(Consumer(creation), found))
                ? [Diagnostic.Create(Grayscale, invocation.Syntax.GetLocation(), invocation.Syntax)]
                : []);
    }

    // --- [MODES]
    private static void Modes(CompilationStartAnalysisContext context, Lifetimes lifetimes, INamedTypeSymbol description) {
        ImmutableArray<ISymbol> owned = Declared(
            context.Compilation,
            "M:Rhino.Display.DisplayModeDescription.GetDisplayMode(System.Guid)",
            "M:Rhino.Display.DisplayModeDescription.FindByName(System.String)",
            "P:Rhino.Display.RhinoViewport.DisplayMode");
        Report<IOperation>(context, [OperationKind.Invocation, OperationKind.PropertyReference], (operation, analysis) =>
            !Written(operation) && Among(Target(operation), owned) && !Inside<INameOfOperation>(operation) && operation.SemanticModel is { } model
            && !lifetimes.Released(operation, description, new Reading(model, analysis.CancellationToken)).Any()
                ? [Diagnostic.Create(Mode, operation.Syntax.GetLocation(), operation.Syntax)]
                : []);
    }

    // --- [WRITES]
    private static void Writes(CompilationStartAnalysisContext context, string property, string owner, DiagnosticDescriptor rule) {
        if (Declared(context.Compilation, property).FirstOrDefault() is not { } written || context.Compilation.GetTypeByMetadataName(owner) is not { } holder) return;
        Report<IAssignmentOperation>(context, [OperationKind.SimpleAssignment, OperationKind.CompoundAssignment, OperationKind.CoalesceAssignment], (assignment, analysis) =>
            assignment.Target is IPropertyReferenceOperation reference && Same(reference.Property, written) && !Within(analysis.ContainingSymbol, holder)
                ? [Diagnostic.Create(rule, assignment.Syntax.GetLocation(), assignment.Syntax)]
                : []);
    }
}

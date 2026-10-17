using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace Rasm.Analyzers.RhinoCommon;

// --- [COMPOSITION] ---------------------------------------------------------------------
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class UncheckedItemCountAnalyzer : RasmAnalyzer {
    private static readonly DiagnosticDescriptor Count = Rule(
        "RASM2002",
        "RhinoCommon",
        "Unchecked item count",
        "Item count from `{0}` reaches no `==` or `!=` comparison",
        "Compare the call result directly with input sequence count through `==` or `!=`, and fail on a difference with an error naming method and both counts.");

    private static readonly ImmutableArray<(string Owner, string Member)> Counted = [
        ("Rhino.DocObjects.Tables.ObjectTable", "Delete"),
        ("Rhino.DocObjects.Tables.ObjectTable", "DeleteGrips"),
        ("Rhino.DocObjects.Tables.LayerTable", "Delete"),
        ("Rhino.DocObjects.Tables.HatchPatternTable", "Delete"),
        ("Rhino.DocObjects.Tables.MarkupTable", "Delete"),
        ("Rhino.DocObjects.Tables.SectionStyleTable", "Delete"),
        ("Rhino.FileIO.File3dmObjectTable", "Delete"),
        ("Rhino.Geometry.Collections.MeshFaceList", "DeleteFaces"),
        ("Rhino.Geometry.Collections.MeshNgonList", "RemoveNgons"),
        ("Rhino.Geometry.PointCloud", "RemoveRange"),
        ("Rhino.DocObjects.HatchPattern", "SetHatchLines"),
        ("Rhino.DocObjects.Worksession", "Attach"),
    ];

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Count];

    protected override void Start(CompilationStartAnalysisContext context) {
        Compilation compilation = context.Compilation;
        ImmutableArray<IMethodSymbol> counting = [
            .. Counted.SelectMany(row => compilation.GetTypeByMetadataName(row.Owner)?.GetMembers(row.Member) ?? []).OfType<IMethodSymbol>()
                .Where(static method => method.ReturnType.SpecialType is SpecialType.System_Int32
                    && method.Parameters.Any(static parameter => parameter is { RefKind: RefKind.None, Type: var type }
                        && type.AllInterfaces.Append(type).Any(static face => face.OriginalDefinition.SpecialType is SpecialType.System_Collections_Generic_IEnumerable_T))),
        ];
        if (counting.IsEmpty) return;
        context.RegisterOperationAction(
            operation => {
                IInvocationOperation invocation = (IInvocationOperation)operation.Operation;
                if (counting.Any(method => Same(invocation.TargetMethod, method))
                    && Consumer(invocation) is not IBinaryOperation { OperatorKind: BinaryOperatorKind.Equals or BinaryOperatorKind.NotEquals })
                    operation.ReportDiagnostic(Diagnostic.Create(Count, invocation.Syntax.GetLocation(), invocation.TargetMethod.Name));
            },
            OperationKind.Invocation);
    }
}

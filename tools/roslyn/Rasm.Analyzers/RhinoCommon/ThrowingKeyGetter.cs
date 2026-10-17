using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace Rasm.Analyzers.RhinoCommon;

// --- [COMPOSITION] ---------------------------------------------------------------------
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class ThrowingKeyGetterAnalyzer : RasmAnalyzer {
    private static readonly DiagnosticDescriptor Read = Rule(
        "RASM2001",
        "RhinoCommon",
        "ArchivableDictionary indexer read",
        "`{0}` throws `KeyNotFoundException` on a missing key",
        "Read through `TryGetValue(<key>, out <value>)` or a typed `TryGet<Type>(<key>, out <value>)`.");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Read];

    protected override void Start(CompilationStartAnalysisContext context) {
        if (context.Compilation.GetTypeByMetadataName("Rhino.Collections.ArchivableDictionary") is not { } dictionary) return;
        context.RegisterOperationAction(
            operation => {
                IPropertyReferenceOperation reference = (IPropertyReferenceOperation)operation.Operation;
                if (reference.Property.IsIndexer && Same(reference.Property.ContainingType, dictionary)
                    && !(reference.Parent is ISimpleAssignmentOperation assignment && assignment.Target == reference))
                    operation.ReportDiagnostic(Diagnostic.Create(Read, reference.Syntax.GetLocation(), reference.Syntax));
            },
            OperationKind.PropertyReference);
    }
}

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace Rasm.Analyzers.RhinoCommon;

// --- [COMPOSITION] ---------------------------------------------------------------------
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class ActiveDocWithDocumentAnalyzer : RasmAnalyzer {
    private static readonly DiagnosticDescriptor Active = Rule(
        "RASM2004",
        "RhinoCommon",
        "ActiveDoc read with a document in scope",
        "`RhinoDoc.ActiveDoc` is read where `{0}` holds a `RhinoDoc`, and Mac Rhino can activate another document",
        "Use the held document, and read `RhinoDoc.ActiveDoc` only in an `==` or `!=` comparison that tests which document is active.");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Active];

    protected override void Start(CompilationStartAnalysisContext context) {
        if (context.Compilation.GetTypeByMetadataName("Rhino.RhinoDoc") is not { } document
            || document.GetMembers("ActiveDoc").OfType<IPropertySymbol>().FirstOrDefault() is not { } active) return;
        IEnumerable<ISymbol> Callbacks(IOperation operation) =>
            operation.Parent switch {
                null => [],
                IAnonymousFunctionOperation callback => callback.Symbol.Parameters.Concat(Callbacks(callback)),
                ILocalFunctionOperation local => local.Symbol.Parameters.Concat(Callbacks(local)),
                var parent => Callbacks(parent),
            };
        IEnumerable<ISymbol> Members(INamedTypeSymbol? type, bool instance) =>
            type is null ? [] : type.GetMembers()
                .Where(member => member is IFieldSymbol or IPropertySymbol && (instance || member.IsStatic))
                .Concat(instance ? type.InstanceConstructors
                    .Where(static constructor => constructor.DeclaringSyntaxReferences.Any(static reference => reference.GetSyntax() is TypeDeclarationSyntax))
                    .SelectMany(static constructor => constructor.Parameters) : [])
                .Concat(Members(type.ContainingType, instance: false));
        bool Compared(IOperation operation) =>
            operation.Parent switch {
                IBinaryOperation { OperatorKind: BinaryOperatorKind.Equals or BinaryOperatorKind.NotEquals } => true,
                IConversionOperation or IConditionalAccessOperation or IMemberReferenceOperation => Compared(operation.Parent),
                _ => false,
            };
        context.RegisterOperationAction(
            operation => {
                IPropertyReferenceOperation reference = (IPropertyReferenceOperation)operation.Operation;
                if (!Same(reference.Property, active) || InNameOf(reference) || Compared(reference)) return;
                ISymbol owner = operation.ContainingSymbol;
                if ((owner is IMethodSymbol method ? method.Parameters : [])
                    .Concat(Callbacks(reference))
                    .Concat(Members(owner.ContainingType, !owner.IsStatic))
                    .FirstOrDefault(held => !Same(held, owner) && !Same(held, (owner as IMethodSymbol)?.AssociatedSymbol) && Same(held switch { IParameterSymbol parameter => parameter.Type, IFieldSymbol field => field.Type, IPropertySymbol property => property.Type, _ => null }, document))
                    is { } holder)
                    operation.ReportDiagnostic(Diagnostic.Create(Active, reference.Syntax.GetLocation(), holder.Name));
            },
            OperationKind.PropertyReference);
    }
}

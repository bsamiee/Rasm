using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace Rasm.Analyzers.Thinktecture;

// --- [COMPOSITION] ---------------------------------------------------------------------
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class ValidationConversionByHandAnalyzer : RasmAnalyzer {
    private static readonly DiagnosticDescriptor Crossing = Rule(
        "RASM1102",
        "Thinktecture",
        "Thinktecture validation outside Validated",
        "`{0}` validates outside `Validated` and returns no `Fin`",
        "Call `<type>.Validated(<raw>)`, or `ValidationResults.Validated<T, TKey>(<raw>)` in generic code and for a type with more than one object factory, direct calls stay inside a validation hook, object factory, or `ValidationResults`.");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Crossing];

    protected override void Start(CompilationStartAnalysisContext context) {
        Compilation compilation = context.Compilation;
        if (compilation.GetTypeByMetadataName("Thinktecture.IObjectFactory`3") is not { } factory
            || compilation.GetTypeByMetadataName("Thinktecture.ValidationError") is not { } invalid
            || factory.GetMembers().OfType<IMethodSymbol>().FirstOrDefault(static member => member.IsStatic) is not { } validate) return;
        INamedTypeSymbol? results = compilation.GetTypeByMetadataName("Thinktecture.ValidationResults");
        IEnumerable<INamedTypeSymbol> Factories(INamedTypeSymbol owner) =>
            owner.AllInterfaces.Where(face => Same(face, factory) && Same(face.TypeArguments[0], owner) && Same(face.TypeArguments[2], invalid));
        bool Implements(IMethodSymbol method, INamedTypeSymbol owner) =>
            Factories(owner).Any(face => Same(owner.FindImplementationForInterfaceMember(face.GetMembers().Single(member => Same(member, validate))), method));
        INamedTypeSymbol? items = compilation.GetTypeByMetadataName("Thinktecture.ISmartEnum`3");
        bool Lookup(IMethodSymbol method, INamedTypeSymbol owner) =>
            owner.AllInterfaces.Where(face => Same(face, items)).SelectMany(static face => face.GetMembers())
                .Any(member => member.IsStatic && Same(owner.FindImplementationForInterfaceMember(member), method));
        bool Creates(IMethodSymbol method, INamedTypeSymbol owner) =>
            method is { ReturnType.SpecialType: SpecialType.System_Boolean, Parameters.Length: 2 } && !Lookup(method, owner)
            && method.Parameters[1] is { RefKind: RefKind.Out, Type: var made } && Same(made, owner)
            && Factories(owner).Any(face => Same(face.TypeArguments[1], method.Parameters[0].Type));
        bool Crosses(IMethodSymbol method) =>
            Same(method, validate) || (method is { IsStatic: true, ContainingType: { } owner } && (Implements(method, owner) || Creates(method, owner)));
        bool Hooked(ISymbol owner) =>
            owner is IMethodSymbol method
            && ((method.ReturnType is { NullableAnnotation: NullableAnnotation.Annotated } returned && Same(returned, invalid))
                || method.Parameters.Any(parameter => parameter.RefKind is RefKind.Ref && Same(parameter.Type, invalid)));
        context.RegisterOperationAction(
            operation => {
                IInvocationOperation invocation = (IInvocationOperation)operation.Operation;
                if (Crosses(invocation.TargetMethod) && !Hooked(operation.ContainingSymbol) && !(results is not null && Within(operation.ContainingSymbol, results)))
                    operation.ReportDiagnostic(Diagnostic.Create(Crossing, invocation.Syntax.GetLocation(), invocation.Syntax));
            },
            OperationKind.Invocation);
    }
}

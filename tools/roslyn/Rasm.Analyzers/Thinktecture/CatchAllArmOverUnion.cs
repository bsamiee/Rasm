using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace Rasm.Analyzers.Thinktecture;

// --- [COMPOSITION] ---------------------------------------------------------------------
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class CatchAllArmOverUnionAnalyzer : RasmAnalyzer {
    private static readonly DiagnosticDescriptor CatchAll = Rule(
        "RASM1101",
        "Thinktecture",
        "Catch-all arm over smart enum or union",
        "Catch-all arm in `switch` over `{0}` hides cases without an arm",
        "Call the generated exhaustive `Switch` or `Map`.");

    private static readonly ImmutableArray<string> Marks = [
        "Thinktecture.UnionAttribute", "Thinktecture.UnionAttributeBase", "Thinktecture.AdHocUnionAttribute", "Thinktecture.SmartEnumAttribute", "Thinktecture.SmartEnumAttribute`1",
    ];

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [CatchAll];

    protected override void Start(CompilationStartAnalysisContext context) {
        ImmutableArray<INamedTypeSymbol> marks = [.. Marks.Select(context.Compilation.GetTypeByMetadataName).OfType<INamedTypeSymbol>()];
        if (marks.IsEmpty) return;
        INamedTypeSymbol? Subject(ITypeSymbol? type) =>
            type switch {
                INamedTypeSymbol { IsTupleType: true } tuple => tuple.TupleElements.Select(element => Subject(element.Type)).FirstOrDefault(static found => found is not null),
                INamedTypeSymbol { OriginalDefinition.SpecialType: SpecialType.System_Nullable_T } nullable => Subject(nullable.TypeArguments[0]),
                INamedTypeSymbol named when named.GetAttributes().Any(data => marks.Any(mark => Derives(data.AttributeClass, mark))) => named,
                _ => null,
            };
        bool Whole(ITypeSymbol? matched, IPatternOperation pattern) => matched is null or { SpecialType: SpecialType.System_Object } || Same(matched, pattern.InputType);
        bool Open(IPatternOperation pattern) =>
            pattern switch {
                IDiscardPatternOperation => true,
                IDeclarationPatternOperation declaration => Whole(declaration.MatchedType, declaration),
                ITypePatternOperation type => Whole(type.MatchedType, type),
                INegatedPatternOperation { Pattern: IConstantPatternOperation { Value.ConstantValue: { HasValue: true, Value: null } } } => true,
                IRecursivePatternOperation recursive => Whole(recursive.MatchedType, recursive) && recursive.PropertySubpatterns.IsEmpty && recursive.DeconstructionSubpatterns.All(Open),
                IBinaryPatternOperation { OperatorKind: BinaryOperatorKind.Or } either => Open(either.LeftPattern) || Open(either.RightPattern),
                IBinaryPatternOperation { OperatorKind: BinaryOperatorKind.And } both => Open(both.LeftPattern) && Open(both.RightPattern),
                _ => false,
            };
        bool Dispatches(IOperation? arm, INamedTypeSymbol union) =>
            arm?.DescendantsAndSelf().Any(nested => nested switch {
                IPatternOperation { NarrowedType: var narrowed } => !Same(narrowed, union) && Derives(narrowed, union),
                IFieldReferenceOperation { Field: { IsStatic: true, Type: var item } } => Same(item, union),
                _ => false,
            }) is true;
        void Check(OperationAnalysisContext operation, IOperation subject, IEnumerable<(IOperation Site, IPatternOperation? Pattern, IOperation? Guard)> arms) {
            if (Subject(subject.Type) is not { } union) return;
            ImmutableArray<(IOperation Site, IPatternOperation? Pattern, IOperation? Guard)> held = [.. arms];
            bool Catches((IOperation Site, IPatternOperation? Pattern, IOperation? Guard) arm) => arm.Guard is null && (arm.Pattern is null || Open(arm.Pattern));
            if (held.Any(arm => !Catches(arm) && (Dispatches(arm.Pattern, union) || Dispatches(arm.Guard, union))))
                foreach ((IOperation site, _, _) in held.Where(Catches))
                    operation.ReportDiagnostic(Diagnostic.Create(CatchAll, site.Syntax.GetLocation(), union.Name));
        }
        context.RegisterOperationAction(
            operation => {
                if (operation.Operation is ISwitchExpressionOperation dispatch)
                    Check(operation, dispatch.Value, dispatch.Arms.Select(static arm => ((IOperation)arm, (IPatternOperation?)arm.Pattern, arm.Guard)));
            },
            OperationKind.SwitchExpression);
        context.RegisterOperationAction(
            operation => {
                if (operation.Operation is ISwitchOperation dispatch)
                    Check(operation, dispatch.Value, dispatch.Cases.SelectMany(static section => section.Clauses).Select(static clause => clause switch {
                        IPatternCaseClauseOperation patterned => ((IOperation)patterned, (IPatternOperation?)patterned.Pattern, patterned.Guard),
                        IDefaultCaseClauseOperation => (clause, null, null),
                        _ => (clause, null, (IOperation?)clause),
                    }));
            },
            OperationKind.Switch);
    }
}

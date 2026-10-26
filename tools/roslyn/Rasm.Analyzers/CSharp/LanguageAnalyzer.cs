using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace Rasm.Analyzers.CSharp;

// --- [COMPOSITION] ---------------------------------------------------------------------
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class LanguageAnalyzer : RasmAnalyzer {
    // --- [RULES]
    internal const string Domain = "CSharp";

    public static readonly DiagnosticDescriptor Covered = Rule(
        "RASM3002",
        Domain,
        "Catch-all arm over a covered enum",
        "Catch-all arm over `{0}` matches no declared member. Delete the arm",
        "Catch-all arm of a switch expression over a non-`[Flags]` enum whose other arms name every member.");

    public static readonly DiagnosticDescriptor Fallback = Rule(
        "RASM3004",
        Domain,
        "Catch-all arm over an enum",
        "Catch-all arm over `{0}` maps {1} to one result. Replace it with arms naming {1}, joined by `or` per shared result",
        "Catch-all arm of a switch expression over a non-`[Flags]` enum whose other arms leave members unnamed.");

    public static readonly DiagnosticDescriptor Discarded = Rule(
        "RASM3003",
        Domain,
        "Discarded awaitable",
        "`_ =` discards awaitable `{0}`. Await `{0}` or return it to an awaiting caller",
        "Awaitable value assigned to a discard.");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Covered, Fallback, Discarded];

    protected override void Start(CompilationStartAnalysisContext context) {
        if (context.Compilation.GetTypeByMetadataName("System.FlagsAttribute") is { } flags) Enums(context, flags);
        Awaitables(context);
    }

    // --- [ENUMS]
    private static void Enums(CompilationStartAnalysisContext context, INamedTypeSymbol flags) {
        static IEnumerable<object?> Named(IOperation? test) =>
            test switch {
                IConstantPatternOperation { Value.ConstantValue: { HasValue: true } constant } => [constant.Value],
                IBinaryPatternOperation { OperatorKind: BinaryOperatorKind.Or } either => Named(either.LeftPattern).Concat(Named(either.RightPattern)),
                _ => [],
            };
        static bool CatchesRest((IOperation Arm, IOperation? Test, IOperation? Guard) arm) =>
            CatchesAll(arm) || (arm is { Guard: null, Test: INegatedPatternOperation { Pattern: var excluded } } && Named(excluded).Any());
        Report<ISwitchExpressionOperation>(context, [OperationKind.SwitchExpression], (dispatch, _) => {
            if (Dispatch(dispatch) is not (_, var arms) || dispatch.Value.Type is not INamedTypeSymbol { TypeKind: TypeKind.Enum } subject
                || !subject.Locations.Any(static location => location.IsInSource) || subject.GetAttributes().Any(data => Same(data.AttributeClass, flags))) return [];
            ImmutableHashSet<object?> named = [.. arms.Where(static arm => arm.Guard is null).SelectMany(static arm => Named(arm.Test))];
            ImmutableArray<IFieldSymbol> unnamed = [.. subject.GetMembers().OfType<IFieldSymbol>().Where(member => member.HasConstantValue && !named.Contains(member.ConstantValue))];
            return named.IsEmpty ? [] : arms.Where(CatchesRest).Select(arm => Diagnostic.Create(
                unnamed.IsEmpty ? Covered : Fallback,
                arm.Arm.Syntax.GetLocation(),
                subject.Name,
                string.Join(", ", unnamed.Select(member => $"`{subject.Name}.{member.Name}`"))));
        });
    }

    // --- [AWAITABLES]
    private static void Awaitables(CompilationStartAnalysisContext context) {
        static bool Awaitable(ITypeSymbol? type) =>
            type is not null && (type.GetMembers(WellKnownMemberNames.GetAwaiter).Any(static member => member is IMethodSymbol { IsStatic: false, Parameters.IsEmpty: true }) || Awaitable(type.BaseType));
        Report<ISimpleAssignmentOperation>(context, [OperationKind.SimpleAssignment], static (assignment, _) =>
            assignment is { Target: IDiscardOperation, Value: var value } && Stripped(value) is var awaited && Awaitable(awaited.Type)
                ? [Diagnostic.Create(Discarded, assignment.Syntax.GetLocation(), awaited.Syntax)]
                : []);
    }
}

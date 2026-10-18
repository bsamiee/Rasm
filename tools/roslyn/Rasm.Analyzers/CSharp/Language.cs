using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace Rasm.Analyzers.CSharp;

// --- [COMPOSITION] ---------------------------------------------------------------------
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class LanguageAnalyzer : RasmAnalyzer {
    // --- [RULES]
    private const string Domain = "CSharp";

    public static readonly DiagnosticDescriptor Covered = Rule(
        "RASM3002",
        Domain,
        "Catch-all arm over a covered enum",
        "Arms name every `{0}` member, the catch-all arm is unreachable for declared values",
        "Delete the arm, CS8509 fails compilation when a member is added.");

    public static readonly DiagnosticDescriptor Fallback = Rule(
        "RASM3004",
        Domain,
        "Catch-all arm over an enum",
        "Catch-all arm over `{0}` sends every member the arms leave unnamed to one result",
        "Name each member in its own arm, join members that share a result with `or`, and delete the catch-all arm, CS8509 then fails compilation when a member gains no arm.");

    public static readonly DiagnosticDescriptor Discarded = Rule(
        "RASM3003",
        Domain,
        "Discarded awaitable",
        "`{0}` starts work and discards its awaitable, no caller observes its failure or completion",
        "Await the value, return it to an awaiting caller, or pass it to a combinator that observes it.");

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
        context.RegisterOperationAction(
            analysis => {
                if (analysis.Operation is not ISwitchExpressionOperation dispatch || Dispatch(dispatch) is not (_, var arms)
                    || dispatch.Value.Type is not INamedTypeSymbol { TypeKind: TypeKind.Enum } subject || !subject.Locations.Any(static location => location.IsInSource)
                    || subject.GetAttributes().Any(data => Same(data.AttributeClass, flags))) return;
                ImmutableHashSet<object?> named = [.. arms.Where(static arm => arm.Guard is null).SelectMany(static arm => Named(arm.Test))];
                if (named.IsEmpty) return;
                DiagnosticDescriptor rule = subject.GetMembers().OfType<IFieldSymbol>().Where(static member => member.HasConstantValue).All(member => named.Contains(member.ConstantValue)) ? Covered : Fallback;
                foreach ((IOperation arm, _, _) in arms.Where(CatchesAll))
                    analysis.ReportDiagnostic(Diagnostic.Create(rule, arm.Syntax.GetLocation(), subject.Name));
            },
            OperationKind.SwitchExpression);
    }

    // --- [AWAITABLES]
    private static void Awaitables(CompilationStartAnalysisContext context) {
        static bool Awaitable(ITypeSymbol? type) =>
            type is not null && (type.GetMembers(WellKnownMemberNames.GetAwaiter).Any(static member => member is IMethodSymbol { IsStatic: false, Parameters.IsEmpty: true }) || Awaitable(type.BaseType));
        context.RegisterOperationAction(
            static analysis => {
                if (analysis.Operation is ISimpleAssignmentOperation { Target: IDiscardOperation, Value: var value } && Awaitable(Stripped(value).Type))
                    analysis.ReportDiagnostic(Diagnostic.Create(Discarded, analysis.Operation.Syntax.GetLocation(), Stripped(value).Syntax));
            },
            OperationKind.SimpleAssignment);
    }
}

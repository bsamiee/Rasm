using System.Collections.Immutable;
using Analyzer.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.FlowAnalysis;
using Microsoft.CodeAnalysis.Operations;

namespace Rasm.Analyzers;

// --- [COMPOSITION] ---------------------------------------------------------------------

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class DiscardedAwaitableAnalyzer : DiagnosticAnalyzer {

    // --- [RULES]

    private static readonly DiagnosticDescriptor Rule = new(
        "RASM3003",
        "Discarded awaitable, await or return the value",
        "`_ =` discards awaitable `{0}`, await `{0}` or return `{0}` to an awaiting caller",
        Categories.CSharp,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        "Await each awaitable value or return it to an awaiting caller in place of a `_ =` discard, outside `RASM3003.exempt` start requests with a task that only reports completion.",
        Link("RASM3003"));

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    public override void Initialize(AnalysisContext context) {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.RegisterCompilationStartAction(static start => {
            static bool Awaitable(IOperation value, OperationAnalysisContext analysis) {
                if (value.Type is not { } type || analysis.Operation.SemanticModel is not { } source) return false;
                int position = analysis.Operation.Syntax.SpanStart;
                SyntaxAnnotation mark = new();
                ExpressionStatementSyntax statement = SyntaxFactory.ExpressionStatement(
                    SyntaxFactory.AwaitExpression(SyntaxFactory.DefaultExpression(SyntaxFactory.ParseTypeName(type.ToMinimalDisplayString(source, position)))).WithAdditionalAnnotations(mark));
                return source.TryGetSpeculativeSemanticModel(position, statement, out SemanticModel? model)
                    && model.GetAwaitExpressionInfo(model.SyntaxTree.GetRoot(analysis.CancellationToken).GetAnnotatedNodes(mark).OfType<AwaitExpressionSyntax>().Single())
                        is { GetAwaiterMethod: not null, IsCompletedProperty: not null, GetResultMethod: not null };
            }
            ImmutableArray<ISymbol> requests = [.. Option(start.Options, "RASM3003.exempt").SelectMany(id => Declared(start.Compilation, id))];
            Report<ISimpleAssignmentOperation>(start, [OperationKind.SimpleAssignment], (assignment, analysis) =>
                assignment is { Target: IDiscardOperation, Value: var value } && Stripped(value) is var awaited && Awaitable(awaited, analysis)
                && !(awaited is IInvocationOperation { TargetMethod: var requested } && requests.Any(request => Same(request, requested)))
                    ? [Diagnostic.Create(Rule, assignment.Syntax.GetLocation(), awaited.Syntax)]
                    : []);
        });
    }
}

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class TypeParameterTestAnalyzer : DiagnosticAnalyzer {

    // --- [RULES]

    private static readonly DiagnosticDescriptor Rule = new(
        "RASM3007",
        "Type test on a type parameter, declare a static virtual member",
        "`{0}` tests `{1}` for concrete `{2}`, declare behavior as a static virtual member of the `{1}` constraint",
        Categories.CSharp,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        "Static virtual constraint members replace type-parameter tests (pattern, `is`, `as`, `typeof` equality outside external overrides) for concrete project types.",
        Link("RASM3007"));

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    public override void Initialize(AnalysisContext context) {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.RegisterCompilationStartAction(static start => {
            if (WellKnownTypeProvider.GetOrCreate(start.Compilation).GetOrCreateTypeByMetadataName("System.Type") is not { } type) return;
            ImmutableArray<IAssemblySymbol> sources = NamespaceOwnerAnalyzer.Projects(start.Compilation, start.Options);
            IAssemblySymbol own = start.Compilation.Assembly;
            ControlFlow flow = ControlFlow.For(start.Compilation);
            ImmutableArray<IMethodSymbol> runtime = Members(start.Compilation.GetSpecialType(SpecialType.System_Object), nameof(GetType));
            bool Concrete(ITypeSymbol? tested) =>
                tested is INamedTypeSymbol { TypeKind: TypeKind.Class or TypeKind.Struct, IsAbstract: false } named
                && (Same(named.ContainingAssembly, own) || sources.Any(source => Same(named.ContainingAssembly, source)));
            static bool External(ISymbol owner) => Overridden(owner).Skip(1).LastOrDefault() is { } origin && !Sourced(origin);
            ITypeParameterSymbol? Parameter(IOperation source, CancellationToken token) {
                Queue<ControlFlow.Value> pending = new(flow.Evaluations(source, token));
                HashSet<ControlFlow.Value> seen = [];
                ITypeParameterSymbol? origin = null;
                while (pending.Count is > 0) {
                    token.ThrowIfCancellationRequested();
                    ControlFlow.Value current = pending.Dequeue();
                    if (!seen.Add(current)) continue;
                    IOperation value = Stripped(current.Operation);
                    ITypeParameterSymbol? parameter = value switch {
                        ITypeOfOperation { TypeOperand: ITypeParameterSymbol input } => input,
                        IInvocationOperation { Instance: { } instance } call when Among(call.TargetMethod, runtime) => Stripped(instance).Type as ITypeParameterSymbol,
                        _ => null,
                    };
                    if (parameter is not null) {
                        if (origin is not null && !Same(origin, parameter)) return null;
                        origin = parameter;
                        continue;
                    }
                    ImmutableArray<Optional<ControlFlow.Value>> definitions = value is ILocalReferenceOperation or IParameterReferenceOperation or IFlowCaptureReferenceOperation
                        ? flow.Values(current.With(value), token) : [];
                    if (definitions.IsEmpty || definitions.Any(static definition => !definition.HasValue)) return null;
                    foreach (Optional<ControlFlow.Value> definition in definitions) pending.Enqueue(definition.Value);
                }
                return origin;
            }
            static (IOperation Site, ITypeSymbol? Input, ITypeSymbol? Tested)? Test(IOperation operation) =>
                operation switch {
                    IDeclarationPatternOperation { MatchesNull: true } => null,
                    IPatternOperation pattern => (pattern.Parent is IIsPatternOperation test ? test : pattern, pattern.InputType, pattern.NarrowedType),
                    IIsTypeOperation test => (test, test.ValueOperand.Type, test.TypeOperand),
                    IConversionOperation { IsTryCast: true } cast => (cast, cast.Operand.Type, cast.Type),
                    _ => null,
                };
            Report<IOperation>(
                start,
                [OperationKind.DeclarationPattern, OperationKind.TypePattern, OperationKind.RecursivePattern, OperationKind.IsType, OperationKind.Conversion],
                (operation, _) =>
                    Test(operation) is ( { } site, ITypeParameterSymbol input, var tested) && Concrete(tested)
                        ? [Diagnostic.Create(Rule, site.Syntax.GetLocation(), site.Syntax, input.Name, tested!.Name)]
                        : []);
            Report<IBinaryOperation>(start, [OperationKind.Binary], (binary, analysis) =>
                binary is { OperatorKind: BinaryOperatorKind.Equals or BinaryOperatorKind.NotEquals } && Same(binary.LeftOperand.Type, type) && Same(binary.RightOperand.Type, type)
                && !External(Member(analysis.ContainingSymbol))
                && (Stripped(binary.LeftOperand), Stripped(binary.RightOperand)) switch {
                    (ITypeOfOperation typed, var other) when Concrete(typed.TypeOperand) => (Other: other, Typed: typed.TypeOperand),
                    (var other, ITypeOfOperation typed) when Concrete(typed.TypeOperand) => (Other: other, Typed: typed.TypeOperand),
                    _ => default,
                } is { Other: { } compared, Typed: { } concrete }
                && Parameter(compared, analysis.CancellationToken) is { } parameter
                    ? [Diagnostic.Create(Rule, binary.Syntax.GetLocation(), binary.Syntax, parameter.Name, concrete.Name)]
                    : []);
        });
    }
}

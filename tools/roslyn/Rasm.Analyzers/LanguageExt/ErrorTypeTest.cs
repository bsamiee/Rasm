using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace Rasm.Analyzers.LanguageExt;

// --- [COMPOSITION] ---------------------------------------------------------------------
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class ErrorTypeTestAnalyzer : RasmAnalyzer {
    private static readonly DiagnosticDescriptor Test = Rule(
        "RASM1003",
        "LanguageExt",
        "Type test on Error",
        "Type test on `Error` value `{0}` misses `ManyErrors` leaves",
        "Test with `IsType<E>()`, `IsExpected`, or `IsExceptional`, and bind fields through `Filter<E>().Head is E <case>`, type tests stay on a leaf from `Head`, `AsIterable`, `FoldM`, or `ForAllM` or after a `ManyErrors` arm.");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Test];

    protected override void Start(CompilationStartAnalysisContext context) {
        Compilation compilation = context.Compilation;
        if (compilation.GetTypeByMetadataName("LanguageExt.Common.Error") is not { } error
            || compilation.GetTypeByMetadataName("LanguageExt.Common.ManyErrors") is not { } many) return;
        ISymbol? getType = compilation.GetSpecialType(SpecialType.System_Object).GetMembers(nameof(GetType)).FirstOrDefault();
        bool Case(ITypeSymbol? type) => !Same(type, error) && !Derives(type, many) && Derives(type, error);
        bool Tests(IPatternOperation pattern) =>
            pattern.DescendantsAndSelf().OfType<IPatternOperation>().Any(nested => Same(nested.InputType, error) && Case(nested.NarrowedType));
        bool Composite(IPatternOperation pattern) =>
            pattern.DescendantsAndSelf().OfType<IPatternOperation>().Any(nested => Same(nested.InputType, error) && Derives(nested.NarrowedType, many));
        ImmutableArray<ISymbol> leaves = [.. error.GetMembers("Head"), .. error.GetMembers("FoldM"), .. error.GetMembers("ForAllM"), .. error.GetMembers("AsIterable")];
        bool Visits(ISymbol? member) =>
            member is not null && (leaves.Any(leaf => Same(member, leaf)) || Visits((member as IMethodSymbol)?.OverriddenMethod ?? (ISymbol?)(member as IPropertySymbol)?.OverriddenProperty));
        IOperation? Receiver(IInvocationOperation call) =>
            call.Instance ?? (call.TargetMethod.IsExtensionMethod && !call.Arguments.IsEmpty ? call.Arguments[0].Value : null);
        bool Iterated(IOperation? source) =>
            source switch {
                IConversionOperation conversion => Iterated(conversion.Operand),
                IInvocationOperation call => Visits(call.TargetMethod) || Iterated(Receiver(call)),
                _ => false,
            };
        IAnonymousFunctionOperation? Callback(IOperation? operation, ISymbol owner) =>
            operation switch {
                null => null,
                IAnonymousFunctionOperation callback when Same(callback.Symbol, owner) => callback,
                _ => Callback(operation.Parent, owner),
            };
        bool Leaf(IOperation tested) =>
            tested switch {
                IPropertyReferenceOperation reference => Visits(reference.Property),
                IParameterReferenceOperation { Parameter.ContainingSymbol: var owner } reference =>
                    Callback(reference, owner) is { Parent: IDelegateCreationOperation { Parent: IArgumentOperation { Parent: IInvocationOperation call } } }
                    && (Visits(call.TargetMethod) || Iterated(Receiver(call))),
                _ => false,
            };
        void Report(OperationAnalysisContext operation, IOperation site, IOperation tested) {
            if (!Leaf(tested)) operation.ReportDiagnostic(Diagnostic.Create(Test, site.Syntax.GetLocation(), tested.Syntax));
        }
        void Arms(OperationAnalysisContext operation, IOperation tested, IEnumerable<IPatternOperation> patterns) {
            if (Same(tested.Type, error))
                foreach (IPatternOperation pattern in patterns.TakeWhile(pattern => !Composite(pattern)).Where(Tests))
                    Report(operation, pattern, tested);
        }
        context.RegisterOperationAction(
            operation => {
                if (operation.Operation is IIsTypeOperation test && Same(test.ValueOperand.Type, error) && Case(test.TypeOperand)) Report(operation, test, test.ValueOperand);
            },
            OperationKind.IsType);
        context.RegisterOperationAction(
            operation => {
                if (operation.Operation is IIsPatternOperation test && Same(test.Value.Type, error) && Tests(test.Pattern) && !Composite(test.Pattern)) Report(operation, test, test.Value);
            },
            OperationKind.IsPattern);
        context.RegisterOperationAction(
            operation => {
                if (operation.Operation is IConversionOperation { IsImplicit: false } cast && Same(cast.Operand.Type, error) && Case(cast.Type)) Report(operation, cast, cast.Operand);
            },
            OperationKind.Conversion);
        context.RegisterOperationAction(
            operation => {
                if (operation.Operation is IInvocationOperation { Instance: { } instance } call && Same(instance.Type, error) && Same(call.TargetMethod, getType)) Report(operation, call, instance);
            },
            OperationKind.Invocation);
        context.RegisterOperationAction(
            operation => {
                if (operation.Operation is ISwitchExpressionOperation dispatch) Arms(operation, dispatch.Value, dispatch.Arms.Select(static arm => arm.Pattern));
            },
            OperationKind.SwitchExpression);
        context.RegisterOperationAction(
            operation => {
                if (operation.Operation is ISwitchOperation dispatch)
                    Arms(operation, dispatch.Value, dispatch.Cases.SelectMany(static section => section.Clauses).OfType<IPatternCaseClauseOperation>().Select(static clause => clause.Pattern));
            },
            OperationKind.Switch);
    }
}

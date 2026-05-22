using System.Collections.Immutable;
using Foundation.CSharp.Analyzers.Kernel;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace Foundation.CSharp.Analyzers.Rules;

// --- [FLOW_RULES] ------------------------------------------------------------

internal static class FlowRules {
    // --- [CONTROL_RULES] ------------------------------------------------------

    internal static void CheckImperativeConditional(OperationAnalysisContext context, ScopeInfo scope, IConditionalOperation conditional) =>
        AnalyzerState.Report(context.ReportDiagnostic, (scope.IsFunctional, conditional.Syntax is IfStatementSyntax) switch {
            (true, true) => Diagnostic.Create(RuleCatalog.CSP0001, context.Operation.Syntax.GetLocation(), "conditional"),
            _ => null,
        });
    internal static void CheckImperativeLoop(OperationAnalysisContext context, ScopeInfo scope) =>
        AnalyzerState.Report(context.ReportDiagnostic, scope.IsFunctional switch {
            true => Diagnostic.Create(RuleCatalog.CSP0001, context.Operation.Syntax.GetLocation(), "loop"),
            _ => null,
        });
    internal static void CheckExceptionTry(OperationAnalysisContext context, ScopeInfo scope, ITryOperation tryOperation) =>
        AnalyzerState.Report(context.ReportDiagnostic, (scope.IsFunctional, tryOperation.Catches.Length > 0, tryOperation.Finally is not null) switch {
            (true, true, _) => Diagnostic.Create(RuleCatalog.CSP0009, context.Operation.Syntax.GetLocation(), "try/catch"),
            (true, false, true) => Diagnostic.Create(RuleCatalog.CSP0009, context.Operation.Syntax.GetLocation(), "try/finally"),
            _ => null,
        });
    internal static void CheckExceptionThrow(OperationAnalysisContext context, ScopeInfo scope, IThrowOperation throwOperation) =>
        AnalyzerState.Report(context.ReportDiagnostic, (scope.IsFunctional, SymbolFacts.IsUnreachableThrow(throwOperation)) switch {
            (_, true) => null,
            (true, false) => Diagnostic.Create(RuleCatalog.CSP0009, context.Operation.Syntax.GetLocation(), "throw"),
            _ => null,
        });

    // --- [MATCH_RULES] --------------------------------------------------------

    internal static void CheckMatchCollapse(OperationAnalysisContext context, AnalyzerState state, ScopeInfo scope, IInvocationOperation invocation) =>
        AnalyzerState.Report(context.ReportDiagnostic, (scope.IsFunctional, SymbolFacts.IsLanguageExtMatch(invocation, state.LanguageExtNamespace), SymbolFacts.IsBoundaryMatchUsage(invocation), SymbolFacts.IsRegexMatchCall(invocation)) switch {
            (true, true, false, false) => Diagnostic.Create(RuleCatalog.CSP0002, context.Operation.Syntax.GetLocation()),
            _ => null,
        });
    internal static void CheckMatchBoundaryStrict(OperationAnalysisContext context, AnalyzerState state, ScopeInfo scope, IInvocationOperation invocation) =>
        AnalyzerState.Report(context.ReportDiagnostic, (scope.IsBoundary, SymbolFacts.IsLanguageExtMatch(invocation, state.LanguageExtNamespace), SymbolFacts.IsBoundaryMatchUsage(invocation), SymbolFacts.IsRegexMatchCall(invocation)) switch {
            (true, true, false, false) => Diagnostic.Create(RuleCatalog.CSP0705, context.Operation.Syntax.GetLocation()),
            _ => null,
        });
    internal static void CheckRunInTransform(OperationAnalysisContext context, ScopeInfo scope, IInvocationOperation invocation) =>
        AnalyzerState.Report(context.ReportDiagnostic, (scope.IsFunctional, SymbolFacts.IsLanguageExtRunCollapse(invocation), invocation.TargetMethod.Name) switch {
            (true, true, string method) => Diagnostic.Create(RuleCatalog.CSP0303, context.Operation.Syntax.GetLocation(), method),
            _ => null,
        });
    // CSP0728: detect Try.lift(...).Run().MapFail(_ => ...) chain that discards the captured exception.
    // Receiver walk uses SymbolFacts.ExtractReceiver/UnwrapReceiver/UnwrapLambda — see [SymbolFacts.cs] for shared helpers.
    internal static void CheckMapFailDiscardsException(OperationAnalysisContext context, ScopeInfo scope, IInvocationOperation invocation) {
        bool isMapFail = invocation.TargetMethod.Name == "MapFail"
            && (invocation.TargetMethod.ContainingType?.ContainingNamespace?.ToDisplayString() ?? string.Empty)
                .StartsWith(value: Markers.LanguageExtNamespace, comparisonType: StringComparison.Ordinal);
        bool capturedException = isMapFail && ReceiverIsRunOfTryLift(SymbolFacts.UnwrapReceiver(SymbolFacts.ExtractReceiver(invocation)));
        IAnonymousFunctionOperation? lambda = (invocation.Arguments.Length > 0, capturedException) switch {
            (true, true) => SymbolFacts.UnwrapLambda(invocation.Arguments[invocation.Arguments.Length - 1].Value),
            _ => null,
        };
        bool singleDiscard = lambda?.Symbol.Parameters switch {
            { Length: 1 } parameters => parameters[0].Name == "_",
            _ => false,
        };
        AnalyzerState.Report(context.ReportDiagnostic, (scope.IsAnalyzable, capturedException, singleDiscard) switch {
            (true, true, true) => Diagnostic.Create(RuleCatalog.CSP0728, context.Operation.Syntax.GetLocation()),
            _ => null,
        });
    }
    private static bool ReceiverIsRunOfTryLift(IOperation? receiver) =>
        receiver is IInvocationOperation runCall
        && runCall.TargetMethod.Name is "Run" or "RunAsync"
        && (runCall.TargetMethod.ContainingType?.ContainingNamespace?.ToDisplayString() ?? string.Empty)
            .StartsWith(value: Markers.LanguageExtNamespace, comparisonType: StringComparison.Ordinal)
        && SymbolFacts.UnwrapReceiver(SymbolFacts.ExtractReceiver(runCall)) is IInvocationOperation liftCall
        && liftCall.TargetMethod.Name == "lift"
        && (liftCall.TargetMethod.ContainingType?.Name == "Try" || liftCall.TargetMethod.ContainingType?.Name == "TryExtensions");

    // --- [NULL_RULES] ---------------------------------------------------------

    internal static void CheckNullSentinel(OperationAnalysisContext context, ScopeInfo scope, IBinaryOperation binaryOperation) =>
        AnalyzerState.Report(context.ReportDiagnostic, (scope.IsFunctional, SymbolFacts.IsNullComparison(binaryOperation)) switch {
            (true, true) => Diagnostic.Create(RuleCatalog.CSP0104, context.Operation.Syntax.GetLocation()),
            _ => null,
        });
    internal static void CheckNullPatternSentinel(OperationAnalysisContext context, ScopeInfo scope, IIsPatternOperation isPatternOperation) {
        bool negated = isPatternOperation.Pattern is INegatedPatternOperation;
        string sentinel = negated ? "is not null" : "is null";
        AnalyzerState.Report(context.ReportDiagnostic, (scope.IsFunctional, SymbolFacts.IsNullPattern(isPatternOperation)) switch {
            (true, true) => Diagnostic.Create(RuleCatalog.CSP0709, context.Operation.Syntax.GetLocation(), sentinel),
            _ => null,
        });
    }
    internal static void CheckReferenceEqualsNull(OperationAnalysisContext context, ScopeInfo scope, IInvocationOperation invocation) =>
        AnalyzerState.Report(context.ReportDiagnostic, (scope.IsFunctional, SymbolFacts.IsReferenceEqualsNull(invocation)) switch {
            (true, true) => Diagnostic.Create(RuleCatalog.CSP0709, context.Operation.Syntax.GetLocation(), "ReferenceEquals"),
            _ => null,
        });
    internal static void CheckEarlyReturnGuardChain(OperationAnalysisContext context, ScopeInfo scope, IConditionalOperation conditional) =>
        AnalyzerState.Report(context.ReportDiagnostic, (scope.IsFunctional, SymbolFacts.IsEarlyReturnGuard(conditional)) switch {
            (true, true) => Diagnostic.Create(RuleCatalog.CSP0706, context.Operation.Syntax.GetLocation()),
            _ => null,
        });

    // --- [PRECEDENCE_RULES] ---------------------------------------------------

    internal static void CheckSwitchExpressionPrecedence(SyntaxNodeAnalysisContext context, ScopeInfo scope) =>
        AnalyzerState.Report(context.ReportDiagnostic, (scope.IsAnalyzable, context.Node) switch {
            (true, SwitchExpressionSyntax @switch) when @switch.Parent is BinaryExpressionSyntax binary
                && ReferenceEquals(objA: binary.Right, objB: @switch)
                && IsArithmeticBinary(binary.Kind())
                => Diagnostic.Create(RuleCatalog.CSP0727, @switch.SwitchKeyword.GetLocation(), binary.OperatorToken.Text),
            _ => null,
        });
    private static bool IsArithmeticBinary(SyntaxKind kind) =>
        kind is SyntaxKind.MultiplyExpression
            or SyntaxKind.DivideExpression
            or SyntaxKind.ModuloExpression
            or SyntaxKind.AddExpression
            or SyntaxKind.SubtractExpression;
    internal static void CheckVariableReassignment(OperationAnalysisContext context, ScopeInfo scope, ISimpleAssignmentOperation assignment) {
        string targetName = assignment.Target is ILocalReferenceOperation local ? local.Local.Name : string.Empty;
        AnalyzerState.Report(context.ReportDiagnostic, (scope.IsFunctional, SymbolFacts.IsSelfReassignment(assignment), targetName.Length > 0) switch {
            (true, true, true) => Diagnostic.Create(RuleCatalog.CSP0707, context.Operation.Syntax.GetLocation(), targetName),
            _ => null,
        });
    }

    // --- [ASYNC_RULES] --------------------------------------------------------

    internal static void CheckAsyncVoid(SymbolAnalysisContext context, ScopeInfo scope, IMethodSymbol method) =>
        AnalyzerState.Report(context.ReportDiagnostic, (scope.IsFunctional, method.IsAsync, method.ReturnsVoid, method.Locations.Length) switch {
            (true, true, true, > 0) => Diagnostic.Create(RuleCatalog.CSP0010, method.Locations[0], method.Name),
            _ => null,
        });
    internal static void CheckAsyncBlocking(OperationAnalysisContext context, ScopeInfo scope, IInvocationOperation invocation) =>
        AnalyzerState.Report(context.ReportDiagnostic, (scope.IsFunctional, SymbolFacts.IsBlockingInvocation(invocation)) switch {
            (true, true) => Diagnostic.Create(RuleCatalog.CSP0006, context.Operation.Syntax.GetLocation(), invocation.TargetMethod.Name),
            _ => null,
        });
    internal static void CheckAsyncBlockingProperty(OperationAnalysisContext context, ScopeInfo scope, IPropertyReferenceOperation propertyReference) =>
        AnalyzerState.Report(context.ReportDiagnostic, (scope.IsFunctional, propertyReference.Property.Name == "Result" && SymbolFacts.IsTaskLikeType(propertyReference.Property.ContainingType)) switch {
            (true, true) => Diagnostic.Create(RuleCatalog.CSP0006, context.Operation.Syntax.GetLocation(), ".Result"),
            _ => null,
        });
    internal static void CheckTaskRunFanOut(OperationAnalysisContext context, ScopeInfo scope, IInvocationOperation invocation) =>
        AnalyzerState.Report(context.ReportDiagnostic, (scope.IsFunctional, SymbolFacts.IsTaskRun(invocation)) switch {
            (true, true) => Diagnostic.Create(RuleCatalog.CSP0014, context.Operation.Syntax.GetLocation()),
            _ => null,
        });
    internal static void CheckFireAndForget(OperationAnalysisContext context, ScopeInfo scope, IInvocationOperation invocation) {
        bool taskLike = SymbolFacts.IsTaskLikeType(invocation.TargetMethod.ReturnType);
        bool unobservedStatement = invocation.Parent is IExpressionStatementOperation or ISimpleAssignmentOperation {
            Target: IDiscardOperation,
            Parent: IExpressionStatementOperation,
        };
        AnalyzerState.Report(context.ReportDiagnostic, (scope.IsFunctional, taskLike, unobservedStatement) switch {
            (true, true, true) => Diagnostic.Create(RuleCatalog.CSP0301, context.Operation.Syntax.GetLocation(), invocation.TargetMethod.Name),
            _ => null,
        });
    }
    internal static void CheckUnboundedWhenAll(OperationAnalysisContext context, ScopeInfo scope, IInvocationOperation invocation) =>
        AnalyzerState.Report(context.ReportDiagnostic, (scope.IsFunctional, SymbolFacts.IsTaskWhenAllUnbounded(invocation)) switch {
            (true, true) => Diagnostic.Create(RuleCatalog.CSP0302, context.Operation.Syntax.GetLocation()),
            _ => null,
        });

    // --- [FUNCTIONAL_RULES] ---------------------------------------------------

    internal static void CheckFilterMapChain(OperationAnalysisContext context, ScopeInfo scope, IInvocationOperation invocation) {
        bool filterMapChain = invocation.TargetMethod.Name == "Map"
            && invocation.Instance is IInvocationOperation receiver
            && receiver.TargetMethod.Name == "Filter"
            && (receiver.TargetMethod.ContainingType?.ContainingNamespace?.ToDisplayString() ?? string.Empty)
                .StartsWith(value: Markers.LanguageExtNamespace, comparisonType: StringComparison.Ordinal);
        AnalyzerState.Report(context.ReportDiagnostic, (scope.IsFunctional, filterMapChain) switch {
            (true, true) => Diagnostic.Create(RuleCatalog.CSP0710, context.Operation.Syntax.GetLocation()),
            _ => null,
        });
    }
    internal static void CheckAsyncAwaitInEff(OperationAnalysisContext context, ScopeInfo scope) {
        bool effReturning = context.ContainingSymbol is IMethodSymbol method
            && method.ReturnType.OriginalDefinition.MetadataName is "Eff`1" or "Eff`2"
            && (method.ReturnType.ContainingNamespace?.ToDisplayString() ?? string.Empty)
                .StartsWith(value: Markers.LanguageExtNamespace, comparisonType: StringComparison.Ordinal);
        string methodName = context.ContainingSymbol?.Name ?? string.Empty;
        AnalyzerState.Report(context.ReportDiagnostic, (scope.IsFunctional, effReturning) switch {
            (true, true) => Diagnostic.Create(RuleCatalog.CSP0711, context.Operation.Syntax.GetLocation(), methodName),
            _ => null,
        });
    }
    internal static void CheckMutableAccumulatorInLoop(OperationAnalysisContext context, ScopeInfo scope, IInvocationOperation invocation) {
        string? typeName = invocation.TargetMethod.ContainingType?.OriginalDefinition?.MetadataName;
        bool mutableAdd = invocation.TargetMethod.Name == "Add"
            && typeName is not null
            && SymbolFacts.MutableCollectionNames.Contains(typeName);
        bool insideLoop = SymbolFacts.IsInsideLoop(context.Operation);
        AnalyzerState.Report(context.ReportDiagnostic, (scope.IsFunctional, mutableAdd, insideLoop) switch {
            (true, true, true) => Diagnostic.Create(RuleCatalog.CSP0718, context.Operation.Syntax.GetLocation(), invocation.TargetMethod.ContainingType?.Name ?? string.Empty),
            _ => null,
        });
    }
    internal static void CheckImperativeAccumulator(OperationAnalysisContext context, ScopeInfo scope, ILoopOperation loopOperation) {
        ImmutableArray<string> outerVariableAssignments = [
            .. loopOperation.Body
                .DescendantsAndSelf()
                .OfType<ISimpleAssignmentOperation>()
                .Select(assignment => SymbolFacts.OuterAccumulatorTarget(loop: loopOperation, assignment: assignment))
                .Where(name => name.Length > 0)
                .Distinct(StringComparer.Ordinal),
        ];
        AnalyzerState.Report(context.ReportDiagnostic, (scope.IsFunctional, outerVariableAssignments.Length) switch {
            (true, > 0) => Diagnostic.Create(RuleCatalog.CSP0725, context.Operation.Syntax.GetLocation(), outerVariableAssignments[0]),
            _ => null,
        });
    }
}

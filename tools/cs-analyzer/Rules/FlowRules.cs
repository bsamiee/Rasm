using System.Collections.Immutable;
using Foundation.CSharp.Analyzers.Kernel;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace Foundation.CSharp.Analyzers.Rules;

// --- [OPERATIONS] ------------------------------------------------------------

internal static class FlowRules {
    // --- [IMPERATIVE_CONTROL_RULES] ------------------------------------------

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
    // --- [EXCEPTION_CONTROL_RULES] --------------------------------------------

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

    // --- [ASYNC_EFFECT_RULES] -------------------------------------------------

    internal static void CheckAsyncVoid(SymbolAnalysisContext context, ScopeInfo scope, IMethodSymbol method) =>
        AnalyzerState.Report(context.ReportDiagnostic, (scope.IsFunctional, method.IsAsync, method.ReturnsVoid, method.Locations.Length) switch {
            (true, true, true, > 0) => Diagnostic.Create(RuleCatalog.CSP0010, method.Locations[0], method.Name),
            _ => null,
        });
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

    // --- [SEQUENCE_PIPELINE_RULES] --------------------------------------------

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
    internal static void CheckTraverseFusion(OperationAnalysisContext context, ScopeInfo scope, IInvocationOperation invocation) {
        IOperation? receiver = SymbolFacts.UnwrapReceiver(SymbolFacts.ExtractReceiver(invocation));
        bool mapReceiver = receiver is IInvocationOperation mapCall
            && IsFusibleMapProjection(mapCall: mapCall);
        bool identityTraversal = invocation.Arguments
            .Where(argument => !SymbolEqualityComparer.Default.Equals(argument.Parameter, invocation.TargetMethod.ReducedFrom?.Parameters.FirstOrDefault()))
            .Any(static argument => IsIdentityProjection(argument.Value));
        AnalyzerState.Report(context.ReportDiagnostic, (scope.IsFunctional, SymbolFacts.IsLanguageExtInvocation(invocation, "Traverse", "TraverseM"), mapReceiver, identityTraversal) switch {
            (true, true, true, true) => Diagnostic.Create(RuleCatalog.CSP0735, context.Operation.Syntax.GetLocation(), invocation.TargetMethod.Name),
            _ => null,
        });
    }
    private static bool IsIdentityProjection(IOperation operation) =>
        SymbolFacts.UnwrapLambda(operation) switch {
            IAnonymousFunctionOperation { Symbol.Parameters.Length: 1 } lambda
                => UnwrapOperation(lambda.Body) is IParameterReferenceOperation parameter
                    && SymbolEqualityComparer.Default.Equals(parameter.Parameter, lambda.Symbol.Parameters[0]),
            _ => operation.Syntax.ToString() is "identity" or "Prelude.identity",
        };
    private static bool IsFusibleMapProjection(IInvocationOperation mapCall) {
        bool languageExtMap = mapCall.TargetMethod.Name == "Map"
            && (mapCall.TargetMethod.ContainingType?.ContainingNamespace?.ToDisplayString() ?? string.Empty)
                .StartsWith(value: Markers.LanguageExtNamespace, comparisonType: StringComparison.Ordinal);
        IAnonymousFunctionOperation? projection = mapCall.Arguments
            .Select(argument => SymbolFacts.UnwrapLambda(argument.Value))
            .FirstOrDefault(static lambda => lambda is not null);
        return languageExtMap && projection is { Symbol.Parameters.Length: 1 };
    }

    // --- [GENERATED_DISPATCH_RULES] -------------------------------------------

    internal static void CheckStateThreadedDispatch(OperationAnalysisContext context, ScopeInfo scope, IInvocationOperation invocation) {
        ImmutableArray<IAnonymousFunctionOperation> lambdas = [
            .. invocation.Arguments
                .Select(argument => SymbolFacts.UnwrapLambda(argument.Value))
                .Where(static lambda => lambda is not null)
                .Select(static lambda => lambda!),
        ];
        bool generatedDispatch = invocation.TargetMethod.Name is "Switch" or "Map"
            && (SymbolFacts.HasThinktectureGeneratedDispatch(invocation.TargetMethod.ContainingType)
                || SymbolFacts.HasThinktectureGeneratedDispatch(SymbolFacts.UnwrapReceiver(SymbolFacts.ExtractReceiver(invocation))?.Type as INamedTypeSymbol));
        int capturedArmCount = lambdas
            .SelectMany(static lambda => CapturedSymbols(lambda))
            .GroupBy(symbol => symbol, SymbolEqualityComparer.Default)
            .Select(group => group.Count())
            .DefaultIfEmpty(defaultValue: 0)
            .Max();
        AnalyzerState.Report(context.ReportDiagnostic, (scope.IsAnalyzable, generatedDispatch, lambdas.Length, capturedArmCount) switch {
            (true, true, >= 3, >= 3) => Diagnostic.Create(RuleCatalog.CSP0734, context.Operation.Syntax.GetLocation(), invocation.TargetMethod.Name, capturedArmCount),
            _ => null,
        });
    }
    private static ImmutableArray<ISymbol> CapturedSymbols(IAnonymousFunctionOperation lambda) {
        ImmutableHashSet<ISymbol> parameters = lambda.Symbol.Parameters.ToImmutableHashSet<ISymbol>(SymbolEqualityComparer.Default);
        return [
            .. lambda.Descendants()
                .Select(reference => reference switch {
                    IParameterReferenceOperation parameter when !parameters.Contains(parameter.Parameter) => (ISymbol)parameter.Parameter,
                    ILocalReferenceOperation local when !IsDeclaredWithin(operation: lambda, local: local.Local) => local.Local,
                    _ => null,
                })
                .Where(static symbol => symbol is not null)
                .Select(static symbol => symbol!)
                .Distinct(SymbolEqualityComparer.Default),
        ];
    }
    private static bool IsDeclaredWithin(IOperation operation, ILocalSymbol local) =>
        local.DeclaringSyntaxReferences switch {
            { IsDefaultOrEmpty: true } => false,
            ImmutableArray<SyntaxReference> refs => refs.Any(reference => operation.Syntax.Span.Contains(reference.GetSyntax().Span)),
        };

    // --- [FIN_ADMISSION_RULES] -------------------------------------------------

    internal static void CheckEarlyReturnGuardChain(OperationAnalysisContext context, ScopeInfo scope, IConditionalOperation conditional) =>
        AnalyzerState.Report(context.ReportDiagnostic, (scope.IsFunctional, SymbolFacts.IsEarlyReturnGuard(conditional)) switch {
            (true, true) => Diagnostic.Create(RuleCatalog.CSP0706, context.Operation.Syntax.GetLocation()),
            _ => null,
        });
    internal static void CheckGuardableFinConditional(OperationAnalysisContext context, AnalyzerState state, ScopeInfo scope, IConditionalOperation conditional) =>
        AnalyzerState.Report(context.ReportDiagnostic, (scope.IsFunctional, SymbolFacts.IsLanguageExtFinType(conditional.Type), IsGuardableFinGate(conditional, state.LanguageExtCommonErrorType), IsConfirmOwner(context.ContainingSymbol), IsDemandOwner(context.ContainingSymbol), IsManualConfirmGate(conditional)) switch {
            (true, true, true, false, false, false) => Diagnostic.Create(RuleCatalog.CSP0739, context.Operation.Syntax.GetLocation()),
            _ => null,
        });
    internal static void CheckGuardableFinConditional(SyntaxNodeAnalysisContext context, AnalyzerState state, ScopeInfo scope, SwitchExpressionSyntax switchExpression) =>
        AnalyzerState.Report(context.ReportDiagnostic, (
            scope.IsFunctional,
            context.SemanticModel.GetOperation(switchExpression, context.CancellationToken) is ISwitchExpressionOperation switchOperation && IsGuardableFinGate(switchOperation, state.LanguageExtCommonErrorType),
            IsConfirmOwner(context.ContainingSymbol),
            IsDemandOwner(context.ContainingSymbol),
            SymbolFacts.IsManualOpConfirmGate(model: context.SemanticModel, switchExpression: switchExpression)) switch {
                (true, true, false, false, false) => Diagnostic.Create(RuleCatalog.CSP0739, switchExpression.SwitchKeyword.GetLocation()),
                _ => null,
            });
    internal static void CheckManualOpAdmissionGate(SyntaxNodeAnalysisContext context, ScopeInfo scope, ConditionalExpressionSyntax conditional) =>
        AnalyzerState.Report(context.ReportDiagnostic, (scope.IsAnalyzable, SymbolFacts.IsManualOpAdmissionGate(model: context.SemanticModel, conditional: conditional) || SymbolFacts.IsManualOpConfirmGate(model: context.SemanticModel, conditional: conditional), IsConfirmOwner(context.ContainingSymbol), IsDemandOwner(context.ContainingSymbol)) switch {
            (true, true, false, false) => Diagnostic.Create(RuleCatalog.CSP0742, conditional.QuestionToken.GetLocation(), "conditional"),
            _ => null,
        });
    internal static void CheckManualOpAdmissionGate(SyntaxNodeAnalysisContext context, ScopeInfo scope, SwitchExpressionSyntax switchExpression) =>
        AnalyzerState.Report(context.ReportDiagnostic, (scope.IsAnalyzable, SymbolFacts.IsManualOpAdmissionGate(model: context.SemanticModel, switchExpression: switchExpression) || SymbolFacts.IsManualOpConfirmGate(model: context.SemanticModel, switchExpression: switchExpression), IsConfirmOwner(context.ContainingSymbol), IsDemandOwner(context.ContainingSymbol)) switch {
            (true, true, false, false) => Diagnostic.Create(RuleCatalog.CSP0742, switchExpression.SwitchKeyword.GetLocation(), "switch"),
            _ => null,
        });
    private static bool IsGuardableFinGate(IConditionalOperation conditional, INamedTypeSymbol? commonErrorType) {
        IOperation? whenTrue = UnwrapOperation(conditional.WhenTrue);
        IOperation? whenFalse = UnwrapOperation(conditional.WhenFalse);
        return !BindsPatternVariable(conditional.Condition)
            && ((IsFinSuccess(whenTrue, conditional.Type) && IsFinFailure(whenFalse, conditional.Type, commonErrorType))
                || (IsFinFailure(whenTrue, conditional.Type, commonErrorType) && IsFinSuccess(whenFalse, conditional.Type)));
    }
    private static bool IsGuardableFinGate(ISwitchExpressionOperation switchExpression, INamedTypeSymbol? commonErrorType) {
        ImmutableArray<ISwitchExpressionArmOperation> arms = switchExpression.Arms;
        return SymbolFacts.IsLanguageExtFinType(switchExpression.Type)
            && switchExpression.Value.Type?.SpecialType == SpecialType.System_Boolean
            && arms.Length == 2
            && ((IsFinSuccess(UnwrapOperation(arms[0].Value), switchExpression.Type) && IsFinFailure(UnwrapOperation(arms[1].Value), switchExpression.Type, commonErrorType))
                || (IsFinFailure(UnwrapOperation(arms[0].Value), switchExpression.Type, commonErrorType) && IsFinSuccess(UnwrapOperation(arms[1].Value), switchExpression.Type)));
    }
    private static bool IsConfirmOwner(ISymbol? symbol) =>
        symbol is IMethodSymbol { Name: "Confirm", Parameters.Length: 1 } method
        && method.Parameters[0].Type.SpecialType == SpecialType.System_Boolean
        && SymbolFacts.IsLanguageExtFinUnitType(method.ReturnType);
    private static bool IsDemandOwner(ISymbol? symbol) =>
        symbol is IMethodSymbol { Name: "Demand" } method
        && method.Parameters.Any(static parameter =>
            parameter.Name == "condition"
            && parameter.Type.SpecialType == SpecialType.System_Boolean)
        && SymbolFacts.IsLanguageExtFinType(method.ReturnType);
    private static bool IsFinSuccess(IOperation? operation, ITypeSymbol? finType) =>
        operation is IInvocationOperation invocation
        && SymbolFacts.IsLanguageExtFinSuccessInvocation(invocation)
        && IsSameFinType(invocation.Type, finType)
        && !IsValueTypeThis(UnwrapOperation(invocation.Arguments[0].Value))
        && !HasKnownSideEffectConstruction(invocation.Arguments[0].Value);
    private static bool IsFinFailure(IOperation? operation, ITypeSymbol? finType, INamedTypeSymbol? commonErrorType) =>
        operation is IInvocationOperation invocation
        && SymbolFacts.IsLanguageExtFinFailureInvocation(invocation, commonErrorType)
        && IsSameFinType(invocation.Type, finType)
        && !HasCancellationFailure(invocation)
        && !HasRichNativeFailureDetail(invocation);
    private static bool IsManualConfirmGate(IConditionalOperation conditional) {
        IOperation? whenTrue = UnwrapOperation(conditional.WhenTrue);
        IOperation? whenFalse = UnwrapOperation(conditional.WhenFalse);
        return SymbolFacts.IsLanguageExtFinUnitType(conditional.Type)
            && ((IsFinUnitSuccess(whenTrue, conditional.Type) && IsPlainInvalidResultFailure(whenFalse, conditional.Type))
                || (IsPlainInvalidResultFailure(whenTrue, conditional.Type) && IsFinUnitSuccess(whenFalse, conditional.Type)));
    }
    private static bool IsFinUnitSuccess(IOperation? operation, ITypeSymbol? finType) =>
        operation is IInvocationOperation invocation
        && SymbolFacts.IsLanguageExtFinSuccessInvocation(invocation)
        && IsSameFinType(invocation.Type, finType)
        && SymbolFacts.IsLanguageExtUnitValue(UnwrapOperation(invocation.Arguments[0].Value));
    private static bool IsPlainInvalidResultFailure(IOperation? operation, ITypeSymbol? finType) =>
        operation is IInvocationOperation invocation
        && SymbolFacts.IsLanguageExtFinFailureInvocation(invocation)
        && IsSameFinType(invocation.Type, finType)
        && SymbolFacts.ContainsPlainInvalidResult(invocation.Arguments[0].Value);
    private static bool IsSameFinType(ITypeSymbol? candidate, ITypeSymbol? expected) =>
        candidate is not null
        && expected is not null
        && SymbolEqualityComparer.Default.Equals(candidate, expected);
    private static bool HasKnownSideEffectConstruction(IOperation operation) =>
        operation.DescendantsAndSelf().Any(static node =>
            node is IInvocationOperation { TargetMethod.Name: "Bind" or "Run" or "Dispose" or "Side" });
    private static bool HasRichNativeFailureDetail(IInvocationOperation failure) =>
        SymbolFacts.UnwrapValue(failure.Arguments[0].Value) is IInvocationOperation {
            TargetMethod.Name: "InvalidResult",
            Arguments.Length: > 0,
        };
    private static bool HasCancellationFailure(IInvocationOperation failure) =>
        SymbolFacts.UnwrapValue(failure.Arguments[0].Value)?.Type?.Name.Contains(value: "Cancelled", comparisonType: StringComparison.Ordinal) == true;
    private static bool IsValueTypeThis(IOperation? operation) =>
        operation is IInstanceReferenceOperation { Type.IsValueType: true };
    private static bool BindsPatternVariable(IOperation condition) =>
        condition.DescendantsAndSelf().Any(static node =>
            node is IDeclarationPatternOperation or IRecursivePatternOperation);

    // --- [GENERIC_PROJECTION_RULES] -------------------------------------------

    internal static void CheckManualGenericProjectionGate(OperationAnalysisContext context, ScopeInfo scope, IConditionalOperation conditional) =>
        AnalyzerState.Report(context.ReportDiagnostic, (scope.IsAnalyzable, SymbolFacts.IsManualGenericProjectionGate(conditional, context.ContainingSymbol)) switch {
            (true, true) => Diagnostic.Create(RuleCatalog.CSP0743, context.Operation.Syntax.GetLocation()),
            _ => null,
        });
    internal static void CheckManualGenericProjectionGate(OperationAnalysisContext context, ScopeInfo scope, ISwitchExpressionOperation switchExpression) =>
        AnalyzerState.Report(context.ReportDiagnostic, (scope.IsAnalyzable, SymbolFacts.IsManualGenericProjectionGate(switchExpression, context.ContainingSymbol)) switch {
            (true, true) => Diagnostic.Create(RuleCatalog.CSP0743, context.Operation.Syntax.GetLocation()),
            _ => null,
        });

    // --- [MUTATION_ACCUMULATOR_RULES] -----------------------------------------

    internal static void CheckVariableReassignment(OperationAnalysisContext context, ScopeInfo scope, ISimpleAssignmentOperation assignment) {
        string targetName = assignment.Target is ILocalReferenceOperation local ? local.Local.Name : string.Empty;
        AnalyzerState.Report(context.ReportDiagnostic, (scope.IsFunctional, SymbolFacts.IsSelfReassignment(assignment), targetName.Length > 0) switch {
            (true, true, true) => Diagnostic.Create(RuleCatalog.CSP0707, context.Operation.Syntax.GetLocation(), targetName),
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
    internal static void CheckFoldAppendAccumulator(OperationAnalysisContext context, ScopeInfo scope, IInvocationOperation invocation) {
        bool directAccumulatorAppend = invocation.TargetMethod.Name == "Add"
            && IsLanguageExtSeq(type: invocation.Type)
            && SymbolFacts.ExtractReceiver(invocation) is IOperation receiver
            && IsLanguageExtSeq(type: receiver.Type)
            && ReferencesFoldAccumulator(operation: receiver, out _);
        AnalyzerState.Report(context.ReportDiagnostic, (scope.IsAnalyzable, directAccumulatorAppend) switch {
            (true, true) => Diagnostic.Create(RuleCatalog.CSP0736, context.Operation.Syntax.GetLocation(), invocation.TargetMethod.Name),
            _ => null,
        });
    }
    internal static void CheckFoldAppendAccumulator(OperationAnalysisContext context, ScopeInfo scope, IBinaryOperation binary) {
        bool sequenceAppend = binary.OperatorKind == BinaryOperatorKind.Add
            && !SymbolFacts.IsOperationalReceiptType(binary.Type)
            && IsLanguageExtSeq(type: binary.Type)
            && ReferencesFoldAccumulator(operation: binary.LeftOperand, out _);
        AnalyzerState.Report(context.ReportDiagnostic, (scope.IsAnalyzable, sequenceAppend) switch {
            (true, true) => Diagnostic.Create(RuleCatalog.CSP0736, context.Operation.Syntax.GetLocation(), "+"),
            _ => null,
        });
    }
    private static bool IsLanguageExtSeq(ITypeSymbol? type) =>
        type is INamedTypeSymbol { OriginalDefinition.MetadataName: "Seq`1", ContainingNamespace: { } ns }
        && ns.ToDisplayString().StartsWith(value: Markers.LanguageExtNamespace, comparisonType: StringComparison.Ordinal);
    private static bool ReferencesFoldAccumulator(IOperation operation, out string name) {
        name = string.Empty;
        IAnonymousFunctionOperation? lambda = EnclosingLambda(operation);
        IParameterSymbol? accumulator = (IsFoldLambda(lambda), lambda?.Symbol.Parameters) switch {
            (true, { Length: > 0 } parameters) => parameters[0],
            _ => null,
        };
        bool references = SymbolFacts.UnwrapReceiver(operation) switch {
            IParameterReferenceOperation parameter when SymbolEqualityComparer.Default.Equals(parameter.Parameter, accumulator) => true,
            _ => false,
        };
        name = references && accumulator is not null ? accumulator.Name : string.Empty;
        return references;
    }
    private static bool IsFoldLambda(IAnonymousFunctionOperation? lambda) =>
        EnclosingInvocation(lambda) is IInvocationOperation invocation
        && invocation.TargetMethod.Name is "Fold" or "FoldWhile";
    private static IInvocationOperation? EnclosingInvocation(IOperation? operation) {
        IOperation? current = operation?.Parent;
        while (current is not null) {
            switch (current) {
                case IInvocationOperation invocation:
                    return invocation;
                case IArgumentOperation { Parent: IInvocationOperation invocation }:
                    return invocation;
                default:
                    current = current.Parent;
                    break;
            }
        }
        return null;
    }
    private static IAnonymousFunctionOperation? EnclosingLambda(IOperation? operation) =>
        operation switch {
            null => null,
            IAnonymousFunctionOperation lambda => lambda,
            _ => EnclosingLambda(operation.Parent),
        };

    // --- [NORMALIZATION] -------------------------------------------------------

    private static IOperation? UnwrapOperation(IOperation? operation) =>
        operation switch {
            IConversionOperation conversion => UnwrapOperation(conversion.Operand),
            IParenthesizedOperation parenthesized => UnwrapOperation(parenthesized.Operand),
            IBlockOperation { Operations.Length: 1 } block => UnwrapOperation(block.Operations[0]),
            IReturnOperation { ReturnedValue: IOperation returned } => UnwrapOperation(returned),
            _ => operation,
        };
}

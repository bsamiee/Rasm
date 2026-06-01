using System.Collections.Immutable;
using Foundation.CSharp.Analyzers.Kernel;
using Foundation.CSharp.Analyzers.Rules;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace Foundation.CSharp.Analyzers.Dispatch;

// --- [DISPATCH] --------------------------------------------------------------

internal static class AnalyzerDispatcher {

    // --- [SYMBOL_DISPATCH] ----------------------------------------------------
    internal static void Run(SymbolAnalysisContext context, AnalyzerState state) {
        ScopeInfo scope = state.ScopeFor(symbol: context.Symbol);
        switch ((scope.IsAnalyzable, context.Symbol)) {
            case (false, _):
                return;
            case (_, IMethodSymbol method):
                state.TrackPrivateMethod(method: method);
                ShapeRules.CheckSignatures(context, scope, method);
                ShapeRules.CheckEffectReturnPolicy(context, scope, method);
                ShapeRules.CheckExtensionProjectionPolicy(context, scope, method);
                ShapeRules.CheckGeneratedCaseAliasCollapse(context, scope, method);
                FlowRules.CheckAsyncVoid(context, scope, method);
                RuntimeRules.CheckLibraryImport(context, scope, method);
                RuntimeRules.CheckGeneratedRegexCharsetValidation(context, scope, method);
                RuntimeRules.CheckEnumeratorCancellation(context, scope, method);
                break;
            case (_, IPropertySymbol property):
                ShapeRules.CheckSignatures(context, scope, property);
                ShapeRules.CheckMutableAutoProperty(context, scope, property);
                TypeShapeRules.CheckAtomRefAsProperty(context, scope, property);
                break;
            case (_, INamedTypeSymbol namedType):
                state.TrackNamedType(namedType: namedType);
                state.TrackInterfaceImplementations(namedType: namedType);
                ShapeRules.CheckSignatures(context, scope, namedType);
                ShapeRules.CheckOverloadSpam(context, scope, namedType);
                ShapeRules.CheckOverloadAdjacency(context, scope, namedType);
                ShapeRules.CheckApiSurfaceInflationByPrefix(context, scope, namedType);
                ShapeRules.CheckMutableFields(context, scope, namedType);
                ShapeRules.CheckPublicCtorOnValidatedPrimitive(context, scope, namedType);
                ShapeRules.CheckTypeClassStaticAbstractPolicy(context, scope, namedType);
                TypeShapeRules.CheckDomainPrimitiveShape(context, scope, namedType);
                TypeShapeRules.CheckCreateFactoryReturnType(context, scope, namedType);
                ImmutableArray<INamedTypeSymbol> unionCases = TypeShapeRules.CheckDiscriminatedUnionShape(context, scope, namedType);
                TypeShapeRules.CheckUnionOpsQualification(context, scope, namedType);
                TypeShapeRules.CheckSamePayloadUnionCases(context, scope, namedType, unionCases);
                TypeShapeRules.CheckExclusiveOptionalPayloadBag(context, scope, namedType);
                TypeShapeRules.CheckDateTimeFieldInDomain(context, scope, namedType);
                TypeShapeRules.CheckAnemicEntity(context, scope, namedType);
                TypeShapeRules.CheckInitOnlyBypassOnValidated(context, scope, namedType);
                TypeShapeRules.CheckOperationalReceiptFactStream(context, scope, namedType);
                TypeShapeRules.CheckForwardingRequestCaseFamily(context, scope, namedType);
                TypeShapeRules.TrackFlagsEnumDeclaration(context, state, namedType);
                ShapeRules.CheckValidationTypeUsage(context, scope, namedType);
                break;
            default:
                return;
        }
    }

    // --- [OPERATION_DISPATCH] -------------------------------------------------

    internal static void Run(OperationAnalysisContext context, AnalyzerState state) {
        ScopeInfo scope = state.ScopeFor(symbol: context.ContainingSymbol);
        switch ((scope.IsAnalyzable, context.Operation)) {
            case (false, _):
                return;
            case (_, IInvocationOperation invocation):
                state.TrackMethodInvocation(method: invocation.TargetMethod);
                FlowRules.CheckMatchCollapse(context, state, scope, invocation);
                FlowRules.CheckMatchBoundaryStrict(context, state, scope, invocation);
                FlowRules.CheckRunInTransform(context, scope, invocation);
                FlowRules.CheckMapFailDiscardsException(context, scope, invocation);
                FlowRules.CheckReferenceEqualsNull(context, scope, invocation);
                FlowRules.CheckAsyncBlocking(context, scope, invocation);
                FlowRules.CheckTaskRunFanOut(context, scope, invocation);
                FlowRules.CheckFireAndForget(context, scope, invocation);
                FlowRules.CheckUnboundedWhenAll(context, scope, invocation);
                FlowRules.CheckFilterMapChain(context, scope, invocation);
                FlowRules.CheckTraverseFusion(context, scope, invocation);
                FlowRules.CheckStateThreadedDispatch(context, scope, invocation);
                FlowRules.CheckMutableAccumulatorInLoop(context, scope, invocation);
                FlowRules.CheckFoldAppendAccumulator(context, scope, invocation);
                ShapeRules.CheckPositionalArguments(context, scope, invocation);
                ShapeRules.CheckReceiptDocumentWrapper(context, scope, invocation);
                RuntimeRules.CheckHotPathLinq(context, scope, invocation);
                RuntimeRules.CheckFluentValidation(context, scope, invocation);
                RuntimeRules.CheckScrutorScanRegistrationStrategy(context, scope, invocation);
                RuntimeRules.CheckChannelTopology(context, scope, invocation);
                RuntimeRules.CheckRegexStaticMethod(context, scope, invocation);
                RuntimeRules.CheckUnsafeNumericConversion(context, scope, invocation);
                RuntimeRules.CheckRhinoAmbientInvocation(context, scope, invocation);
                return;
            case (_, IPropertyReferenceOperation propertyReference):
                FlowRules.CheckAsyncBlockingProperty(context, scope, propertyReference);
                RuntimeRules.CheckWallClock(context, scope, propertyReference);
                RuntimeRules.CheckRhinoAmbientPropertyAccess(context, scope, propertyReference);
                return;
            case (_, IObjectCreationOperation objectCreation):
                ShapeRules.CheckMutableCollections(context, scope, objectCreation);
                ShapeRules.CheckPositionalRecordConstructor(context, scope, objectCreation);
                ShapeRules.CheckReceiptConstructionOwner(context, scope, objectCreation);
                RuntimeRules.CheckHttpClient(context, scope, objectCreation);
                RuntimeRules.CheckTimerCreation(context, scope, objectCreation);
                RuntimeRules.CheckTelemetryIdentityConstruction(context, scope, objectCreation);
                RuntimeRules.CheckRegexRuntimeConstruction(context, scope, objectCreation);
                return;
            case (_, IAnonymousFunctionOperation anonymousFunction):
                RuntimeRules.CheckClosureCapture(context, scope, anonymousFunction);
                RuntimeRules.CheckHotPathNonStaticLambda(context, scope, anonymousFunction);
                return;
            case (_, IConditionalOperation conditional):
                FlowRules.CheckImperativeConditional(context, scope, conditional);
                FlowRules.CheckEarlyReturnGuardChain(context, scope, conditional);
                FlowRules.CheckGuardableFinConditional(context, scope, conditional);
                return;
            case (_, ILoopOperation loop):
                FlowRules.CheckImperativeLoop(context, scope);
                FlowRules.CheckImperativeAccumulator(context, scope, loop);
                return;
            case (_, ITryOperation tryOperation):
                FlowRules.CheckExceptionTry(context, scope, tryOperation);
                return;
            case (_, IThrowOperation throwOperation):
                FlowRules.CheckExceptionThrow(context, scope, throwOperation);
                return;
            case (_, IBinaryOperation binary):
                FlowRules.CheckNullSentinel(context, scope, binary);
                FlowRules.CheckFoldAppendAccumulator(context, scope, binary);
                ShapeRules.CheckReceiptChainCollapse(context, scope, binary);
                TypeShapeRules.TrackFlagsEnumComposition(context, state, binary);
                return;
            case (_, IIsPatternOperation isPattern):
                FlowRules.CheckNullPatternSentinel(context, scope, isPattern);
                return;
            case (_, ISimpleAssignmentOperation simpleAssignment):
                FlowRules.CheckVariableReassignment(context, scope, simpleAssignment);
                return;
            case (_, ILiteralOperation literal):
                RuntimeRules.CheckHardcodedOtlp(context, scope, literal);
                return;
            case (_, IAwaitOperation):
                FlowRules.CheckAsyncAwaitInEff(context, scope);
                return;
            case (_, IConversionOperation conversion):
                RuntimeRules.CheckUnsafeNarrowingCast(context, scope, conversion);
                return;
            case (_, IWithOperation withOperation):
                TypeShapeRules.CheckWithExpressionBypass(context, scope, withOperation);
                ShapeRules.CheckReceiptConstructionOwner(context, scope, withOperation);
                return;
        }
    }

    // --- [SYNTAX_DISPATCH] ---------------------------------------------------

    internal static void Run(SyntaxNodeAnalysisContext context, AnalyzerState state) {
        ScopeInfo scope = context.ContainingSymbol switch {
            ISymbol symbol => state.ScopeFor(symbol: symbol),
            _ => new ScopeInfo(kind: ScopeKind.Other, namespaceName: string.Empty, filePath: context.Node.SyntaxTree.FilePath),
        };
        switch (context.Node) {
            case VariableDeclarationSyntax or ForEachStatementSyntax or DeclarationExpressionSyntax:
                ShapeRules.CheckVarInference(context, state);
                return;
            case ConditionalExpressionSyntax conditional:
                FlowRules.CheckManualOpAdmissionGate(context, scope, conditional);
                return;
            case SwitchExpressionSyntax:
                FlowRules.CheckSwitchExpressionPrecedence(context, scope);
                FlowRules.CheckGuardableFinConditional(context, scope, (SwitchExpressionSyntax)context.Node);
                FlowRules.CheckManualOpAdmissionGate(context, scope, (SwitchExpressionSyntax)context.Node);
                return;
        }
    }

    // --- [COMPILATION_DISPATCH] ----------------------------------------------

    internal static void Run(CompilationAnalysisContext context, AnalyzerState state) {
        ShapeRules.ReportInterfacePollution(context, state);
        ShapeRules.ReportSingleUseHelpers(context, state);
        TypeShapeRules.ReportFlagsEnumOveruse(context, state);
        TypeShapeRules.ReportManualClosedUnionOverride(context, state);
    }
}

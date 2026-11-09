using System.Collections.Concurrent;
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
public sealed class ResultLiftAnalyzer : DiagnosticAnalyzer {

    // --- [RULES]

    private static readonly DiagnosticDescriptor Rule = new(
        "RASM1001",
        "Explicit result lift, write the value",
        "`{0}` restates implicit conversion of `{1}` to `{2}`, write `{1}`",
        Categories.LanguageExt,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        "Implicit conversions replace `Fin` or `Validation` factory calls, case constructions, and arm casts when overloads and arm types hold.",
        Link("RASM1001"));

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    public override void Initialize(AnalysisContext context) {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.RegisterCompilationStartAction(static start => {
            WellKnownTypeProvider types = WellKnownTypeProvider.GetOrCreate(start.Compilation);
            if (types.GetOrCreateTypeByMetadataName("LanguageExt.Fin`1") is not { } fin || types.GetOrCreateTypeByMetadataName("LanguageExt.Validation`2") is not { } validation) return;
            ImmutableArray<ISymbol> factories = Declared(
                start.Compilation,
                "M:LanguageExt.Fin.Succ``1(``0)",
                "M:LanguageExt.Fin.Fail``1(LanguageExt.Common.Error)",
                "M:LanguageExt.Validation.Success``2(``1)",
                "M:LanguageExt.Validation.Fail``2(``0)",
                "M:LanguageExt.Prelude.FinSucc``1(``0)",
                "M:LanguageExt.Prelude.FinFail``1(LanguageExt.Common.Error)",
                "M:LanguageExt.Prelude.Pure``1(``0)",
                "M:LanguageExt.Prelude.Fail``1(``0)",
                "M:LanguageExt.Prelude.Success``2(``1)",
                "M:LanguageExt.Prelude.Fail``2(``0)");
            ImmutableArray<INamedTypeSymbol> cases = [
                .. fin.GetTypeMembers().Where(nested => Derives(nested, fin)),
                .. validation.GetTypeMembers().Where(nested => Derives(nested, validation)),
            ];
            (IOperation Lift, INamedTypeSymbol Target, IMethodSymbol Conversion, ExpressionSyntax Value)? Candidate(IOperation operation) {
                IParameterSymbol? parameter = operation switch {
                    IInvocationOperation { Arguments: { Length: 1 } arguments } call when Among(call.TargetMethod, factories) => arguments[0].Parameter,
                    IObjectCreationOperation { Arguments: { Length: 1 } arguments, Initializer: null, Constructor: { } made } when Among(made.ContainingType, cases) => arguments[0].Parameter,
                    IConversionOperation { IsImplicit: false, Syntax: CastExpressionSyntax, OperatorMethod.Parameters: { Length: 1 } operands, Parent: IConditionalOperation or ISwitchExpressionArmOperation } => operands[0],
                    _ => null,
                };
                return parameter is not null
                    && (operation.Parent is IConversionOperation { IsImplicit: true } converted ? converted.Type : operation.Type) is INamedTypeSymbol target
                    && (Same(target, fin) || Same(target, validation))
                    && Unlifted(operation.Syntax) is { } value
                    && operation.SemanticModel?.ClassifyConversion(value, target) is { IsImplicit: true, IsUserDefined: true, MethodSymbol: { Parameters: { Length: 1 } sources } conversion }
                    && SymbolEqualityComparer.Default.Equals(sources[0].Type, parameter.Type)
                    ? (operation, target, conversion, value)
                    : null;
            }
            start.RegisterOperationBlockStartAction(block => {
                ConcurrentDictionary<SyntaxNode, ConcurrentBag<(IOperation Lift, INamedTypeSymbol Target, IMethodSymbol Conversion, ExpressionSyntax Value)>> candidates = new();
                block.RegisterOperationAction(analysis => {
                    if (Candidate(analysis.Operation) is { } candidate && !Inferred(candidate.Lift) && Scope(candidate.Lift.Syntax) is { } root)
                        candidates.GetOrAdd(root, static _ => []).Add(candidate);
                }, OperationKind.Invocation, OperationKind.ObjectCreation, OperationKind.Conversion);
                block.RegisterOperationBlockEndAction(end => {
                    foreach (KeyValuePair<SyntaxNode, ConcurrentBag<(IOperation Lift, INamedTypeSymbol Target, IMethodSymbol Conversion, ExpressionSyntax Value)>> scope in candidates) {
                        end.CancellationToken.ThrowIfCancellationRequested();
                        SyntaxNode root = scope.Key;
                        ImmutableArray<(IOperation Lift, INamedTypeSymbol Target, IMethodSymbol Conversion, ExpressionSyntax Value)> lifts = [.. scope.Value.OrderByDescending(static lift => lift.Lift.Syntax.SpanStart).ThenBy(static lift => lift.Lift.Syntax.Span.Length)];
                        ImmutableArray<(SyntaxNode Site, ISymbol? Symbol)> calls = Calls(lifts[0].Lift.SemanticModel!.GetOperation(root, end.CancellationToken));
                        ImmutableArray<(IOperation Lift, INamedTypeSymbol Target, IMethodSymbol Conversion, ExpressionSyntax Value)> removed = Holds(root, calls, lifts, end.CancellationToken) ? lifts : lifts.Length is 1 ? [] : lifts.Aggregate(
                            ImmutableArray<(IOperation Lift, INamedTypeSymbol Target, IMethodSymbol Conversion, ExpressionSyntax Value)>.Empty,
                            (held, next) => held.Add(next) switch { var proposed => Holds(root, calls, proposed, end.CancellationToken) ? proposed : held });
                        foreach ((IOperation lift, INamedTypeSymbol target, _, ExpressionSyntax value) in removed)
                            end.ReportDiagnostic(Diagnostic.Create(Rule, lift.Syntax.GetLocation(), lift.Syntax, value, target.ToDisplayString(Shown)));
                    }
                });
            });
        });
    }

    // --- [LIFTS]

    private static ExpressionSyntax? Unlifted(SyntaxNode lift) =>
        lift switch {
            InvocationExpressionSyntax { ArgumentList.Arguments: { Count: 1 } arguments } => arguments[0].Expression,
            BaseObjectCreationExpressionSyntax { ArgumentList.Arguments: { Count: 1 } arguments, Initializer: null } => arguments[0].Expression,
            CastExpressionSyntax cast => cast.Expression,
            _ => null,
        };

    private static bool Inferred(IOperation lift) {
        IOperation? consumer = Consumer(lift);
        if (consumer is IReturnOperation { Parent: IBlockOperation { Parent: IAnonymousFunctionOperation function } } && Sole(function.Body) == lift)
            consumer = Consumer(function) is IDelegateCreationOperation creation ? Consumer(creation) : null;
        return consumer switch {
            IVariableInitializerOperation { Parent.Syntax: VariableDeclaratorSyntax { Parent: VariableDeclarationSyntax { Type.IsVar: true } } } => true,
            IArgumentOperation { Parent: IInvocationOperation { TargetMethod: { IsGenericMethod: true } method } call, Parameter: { } parameter }
                when call.Syntax is InvocationExpressionSyntax { Expression: not (GenericNameSyntax or MemberAccessExpressionSyntax { Name: GenericNameSyntax }) }
                && (method.ReducedFrom ?? method).OriginalDefinition is { } definition
                && definition.Parameters[parameter.Ordinal + (method.ReducedFrom is null ? 0 : 1)] is var held =>
                definition.TypeParameters.Any(type => Mentions(held.Type, type) && !definition.Parameters.Any(other => !SymbolEqualityComparer.Default.Equals(other, held) && Mentions(other.Type, type))),
            _ => false,
        };
    }

    private static SyntaxNode? Scope(SyntaxNode node) {
        while (node.Ancestors().FirstOrDefault(static held => held is AnonymousFunctionExpressionSyntax or LocalFunctionStatementSyntax or MemberDeclarationSyntax or AttributeSyntax) is AnonymousFunctionExpressionSyntax callback) node = callback;
        return node.Ancestors().FirstOrDefault(static held => held is (StatementSyntax and not BlockSyntax)
                or ArrowExpressionClauseSyntax { Parent: not LocalFunctionStatementSyntax }
                or EqualsValueClauseSyntax { Parent: PropertyDeclarationSyntax or ParameterSyntax or VariableDeclaratorSyntax { Parent.Parent: FieldDeclarationSyntax } }
                or MemberDeclarationSyntax or AttributeSyntax) is { } scope and (StatementSyntax or ArrowExpressionClauseSyntax or EqualsValueClauseSyntax)
                ? scope
                : null;
    }

    private static bool Holds(SyntaxNode scope, ImmutableArray<(SyntaxNode Site, ISymbol? Symbol)> calls, ImmutableArray<(IOperation Lift, INamedTypeSymbol Target, IMethodSymbol Conversion, ExpressionSyntax Value)> removed, CancellationToken token) {
        SemanticModel model = removed[0].Lift.SemanticModel!;
        ImmutableDictionary<SyntaxNode, SyntaxAnnotation> marked = removed.ToImmutableDictionary(static lift => lift.Lift.Syntax, static _ => new SyntaxAnnotation());
        SyntaxAnnotation outer = new();
        SyntaxNode unlifted = scope.ReplaceNodes(marked.Keys, (site, rewritten) => SyntaxFactory.ParenthesizedExpression(Unlifted(rewritten)!.WithoutTrivia()).WithTriviaFrom(rewritten).WithAdditionalAnnotations(marked[site]))
            .WithAdditionalAnnotations(outer);
        SemanticModel? speculative = unlifted switch {
            StatementSyntax statement when model.TryGetSpeculativeSemanticModel(scope.SpanStart, statement, out SemanticModel? held) => held,
            ArrowExpressionClauseSyntax arrow when model.TryGetSpeculativeSemanticModel(scope.SpanStart, arrow, out SemanticModel? held) => held,
            EqualsValueClauseSyntax initializer when model.TryGetSpeculativeSemanticModel(scope.SpanStart, initializer, out SemanticModel? held) => held,
            _ => null,
        };
        if (speculative is null) return false;
        SyntaxNode rebound = speculative.SyntaxTree.GetRoot(token).GetAnnotatedNodes(outer).First();
        return removed.Select(lift => (Lift: lift, Site: (ExpressionSyntax)rebound.GetAnnotatedNodes(marked[lift.Lift.Syntax]).First()))
            .All(held => speculative.GetConversion(held.Site, token) is { IsImplicit: true, IsUserDefined: true, MethodSymbol: var conversion }
                && SymbolEqualityComparer.Default.Equals(conversion, held.Lift.Conversion)
                && SymbolEqualityComparer.Default.Equals(speculative.GetTypeInfo(held.Site, token).ConvertedType, held.Lift.Target))
            && calls.Where(call => !marked.ContainsKey(call.Site)).Select(static call => call.Symbol).SequenceEqual(
                Calls(speculative.GetOperation(rebound, token)).Select(static call => call.Symbol), SymbolEqualityComparer.Default);
    }

    private static ImmutableArray<(SyntaxNode Site, ISymbol? Symbol)> Calls(IOperation? scope) =>
        scope is null ? [] : [.. scope.DescendantsAndSelf().Select(static (SyntaxNode? Site, ISymbol? Symbol) (operation) => operation switch {
            IInvocationOperation { Syntax: InvocationExpressionSyntax site } call => (site, call.TargetMethod),
            IObjectCreationOperation { Syntax: BaseObjectCreationExpressionSyntax site } made => (site, made.Constructor),
            _ => (null, null),
        }).Where(static call => call.Site is not null).Select(static call => (call.Site!, call.Symbol))];
}

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class ErrorTypeTestAnalyzer : DiagnosticAnalyzer {

    // --- [RULES]

    private static readonly DiagnosticDescriptor Rule = new(
        "RASM1003",
        "Type test on `Error`, call `IsType` or `Filter`",
        "{0} misses errors inside `ManyErrors`, call `{1}.IsType<{2}>()` or bind fields through `{1}.Filter<{2}>().Head is {2} <name>`",
        Categories.LanguageExt,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        "Classify `Error` values able to hold `ManyErrors` with `IsType` or `Filter` over casts, type tests, switch arms, or `GetType()`.",
        Link("RASM1003"));

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    public override void Initialize(AnalysisContext context) {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.RegisterCompilationStartAction(static start => {
            WellKnownTypeProvider types = WellKnownTypeProvider.GetOrCreate(start.Compilation);
            if (types.GetOrCreateTypeByMetadataName("LanguageExt.Common.Error") is not { } error || types.GetOrCreateTypeByMetadataName("LanguageExt.Common.ManyErrors") is not { } many) return;
            ImmutableArray<ISymbol> leaves = Declared(
                start.Compilation,
                "P:LanguageExt.Common.Error.Head",
                "M:LanguageExt.Common.Error.AsIterable",
                "M:LanguageExt.Common.Error.FoldM``2(System.Func{LanguageExt.Common.Error,LanguageExt.Traits.K{``0,``1}})",
                "M:LanguageExt.Common.Error.ForAllM``2(System.Func{LanguageExt.Common.Error,LanguageExt.Traits.K{``0,``1}})");
            ISymbol? runtime = start.Compilation.GetSpecialType(SpecialType.System_Object).GetMembers(nameof(GetType)).FirstOrDefault();
            bool Typed(IOperation value) => Same(value.Type, error);
            bool Case(ITypeSymbol? type) => !Same(type, error) && !Derives(type, many) && Derives(type, error);
            bool Narrows(IOperation pattern, Func<ITypeSymbol?, bool> wanted) =>
                pattern.DescendantsAndSelf().OfType<IPatternOperation>().Any(nested => Same(nested.InputType, error) && wanted(nested.NarrowedType));
            ITypeSymbol? Narrowed(IOperation pattern) =>
                pattern.DescendantsAndSelf().OfType<IPatternOperation>().Where(nested => Same(nested.InputType, error)).Select(static nested => nested.NarrowedType).FirstOrDefault(Case);
            bool Visits(ISymbol member) => Overridden(member).Any(held => Among(held, leaves));
            bool Iterated(IOperation? source) {
                while (source is not null && Stripped(source) is IInvocationOperation call) {
                    if (Visits(call.TargetMethod)) return true;
                    source = Receiver(call);
                }
                return false;
            }
            bool Leaf(IOperation tested) =>
                Stripped(tested) switch {
                    IPropertyReferenceOperation reference => Visits(reference.Property),
                    IParameterReferenceOperation { Parameter.ContainingSymbol: IMethodSymbol { MethodKind: MethodKind.AnonymousFunction } callback } reference =>
                        Ancestor<IAnonymousFunctionOperation>(reference, held => Same(held.Symbol, callback)) is { Parent: IDelegateCreationOperation { Parent: IArgumentOperation { Parent: IInvocationOperation call } } }
                        && (Visits(call.TargetMethod) || Iterated(Receiver(call))),
                    ILocalReferenceOperation reference => Ancestor<IForEachLoopOperation>(reference, loop => loop.Locals.Any(local => Same(local, reference.Local))) is { } loop && Iterated(loop.Collection),
                    _ => false,
                };
            IEnumerable<(IOperation Site, IOperation Tested, ITypeSymbol? Type)> Tests(IOperation operation) =>
                operation switch {
                    IIsTypeOperation test when Typed(test.ValueOperand) && Case(test.TypeOperand) => [(test, test.ValueOperand, test.TypeOperand)],
                    IIsPatternOperation test when Typed(test.Value) && Narrowed(test.Pattern) is { } type && !Narrows(test.Pattern, held => Derives(held, many)) => [(test, test.Value, type)],
                    IConversionOperation { IsImplicit: false } cast when Typed(cast.Operand) && Case(cast.Type) => [(cast, cast.Operand, cast.Type)],
                    IInvocationOperation { Instance: { } instance } call when Typed(instance) && Same(call.TargetMethod, runtime) => [(call, instance, null)],
                    _ when Dispatch(operation) is ( { } subject, var arms) && Typed(subject) =>
                        arms.Select(static arm => arm.Test).OfType<IPatternOperation>().TakeWhile(pattern => !Narrows(pattern, held => Derives(held, many)))
                            .Select(pattern => (Site: (IOperation)pattern, Tested: subject, Type: Narrowed(pattern))).Where(static test => test.Type is not null),
                    _ => [],
                };
            Report<IOperation>(
                start,
                [OperationKind.IsType, OperationKind.IsPattern, OperationKind.Conversion, OperationKind.Invocation, OperationKind.SwitchExpression, OperationKind.Switch],
                (operation, _) => Tests(operation).Where(test => !Leaf(test.Tested)).Select(static test => Diagnostic.Create(
                    Rule,
                    test.Site.Syntax.GetLocation(),
                    test.Type is null ? $"`{test.Site.Syntax}`" : $"Narrowing `{test.Tested.Syntax}` to `{test.Type.ToDisplayString(Shown)}`",
                    test.Tested.Syntax,
                    test.Type?.ToDisplayString(Shown) ?? "<case>")));
        });
    }
}

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class ValidationRoundTripAnalyzer : DiagnosticAnalyzer {

    // --- [RULES]

    private static readonly DiagnosticDescriptor Rule = new(
        "RASM1004",
        "`Fin` round trip through `Validation`, delete both conversions",
        "`{0}` round-trips `Fin` through `Validation`, delete each `{1}()` and the closing `{2}()`",
        Categories.LanguageExt,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        "Compose `Fin` operands as `Fin` over a parameterless `Validation` round trip, direct or through `Apply` or `Traverse`.",
        Link("RASM1004"));

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    public override void Initialize(AnalysisContext context) {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.RegisterCompilationStartAction(static start => {
            WellKnownTypeProvider types = WellKnownTypeProvider.GetOrCreate(start.Compilation);
            if (types.GetOrCreateTypeByMetadataName("LanguageExt.Fin`1") is not { } fin || types.GetOrCreateTypeByMetadataName("LanguageExt.Validation`2") is not { } validation
                || types.GetOrCreateTypeByMetadataName("LanguageExt.Traits.K`2") is not { } kind) return;
            ImmutableArray<IMethodSymbol> applies = Members(types.GetOrCreateTypeByMetadataName("LanguageExt.ApplicativeExtensions"), "Apply");
            ImmutableArray<IMethodSymbol> traversals = [.. kind.ContainingNamespace.ContainingNamespace.GetTypeMembers().SelectMany(static type => Members(type, "Traverse"))];
            IOperation? Source(IOperation operation, INamedTypeSymbol from, INamedTypeSymbol to) =>
                operation is IInvocationOperation call && Same(call.TargetMethod.ContainingAssembly, fin.ContainingAssembly) && Parameterless(call)
                && Receiver(call) is { } held && Same(held.Type, from) && Same(call.Type, to) ? Stripped(held) : null;
            ImmutableArray<IOperation> Hops(IOperation operand, CancellationToken token) {
                ImmutableArray<IOperation>.Builder hops = ImmutableArray.CreateBuilder<IOperation>();
                Stack<IOperation> remaining = new([operand]);
                while (remaining.Count is not 0) {
                    token.ThrowIfCancellationRequested();
                    IOperation held = Stripped(remaining.Pop());
                    if (Source(held, fin, validation) is not null) {
                        hops.Add(held);
                        continue;
                    }
                    switch (Source(held, kind, validation)) {
                        case IInvocationOperation apply when Among(apply.TargetMethod, applies) && Receiver(apply) is { } tupled && Stripped(tupled) is ITupleOperation tuple:
                            foreach (IOperation element in tuple.Elements.Reverse()) remaining.Push(element);
                            break;
                        case IInvocationOperation traverse when Among(traverse.TargetMethod, traversals)
                            && traverse.Arguments.FirstOrDefault(static argument => argument.Parameter!.Type.TypeKind is TypeKind.Delegate)?.Value is { } mapped
                            && Lambda(mapped) is { } function && Sole(function.Body) is { } returned:
                            remaining.Push(returned);
                            break;
                        default:
                            return [];
                    }
                }
                return hops.DrainToImmutable();
            }
            Report<IInvocationOperation>(start, [OperationKind.Invocation], (exit, analysis) =>
                Source(exit, validation, fin) is { } inner && Hops(inner, analysis.CancellationToken) is { IsEmpty: false } hops && (Source(inner, kind, validation) ?? inner) is IInvocationOperation named
                    ? [Diagnostic.Create(
                        Rule,
                        Named(exit),
                        [.. hops.Select(static hop => hop.Syntax.GetLocation()), exit.Syntax.GetLocation()],
                        named.TargetMethod.Name,
                        ((IInvocationOperation)hops[0]).TargetMethod.Name,
                        exit.TargetMethod.Name)]
                    : []);
        });
    }
}

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class SeqSpreadAnalyzer : DiagnosticAnalyzer {

    // --- [RULES]

    private static readonly DiagnosticDescriptor Rule = new(
        "RASM1005",
        "`Seq` accumulator copied per step, call `Add`",
        "`{0}` copies `{1}` on every step, write `{1}.Add({2})`",
        Categories.LanguageExt,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        "`Seq` fold accumulators and loop locals append through `Add` in place of `[.. <seq>, <item>]`.",
        Link("RASM1005"));

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    public override void Initialize(AnalysisContext context) {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.RegisterCompilationStartAction(static start => {
            if (WellKnownTypeProvider.GetOrCreate(start.Compilation).GetOrCreateTypeByMetadataName("LanguageExt.Seq`1") is not { } seq) return;
            static bool Stepped(IOperation operation, ISymbol held) =>
                Ancestor<IOperation>(operation, static step => step is IAnonymousFunctionOperation { Symbol.Parameters.Length: > 1 } or ILoopOperation) switch {
                    IAnonymousFunctionOperation { Symbol: { Parameters: var parameters } folder } => Same(parameters[0], held) && SymbolEqualityComparer.Default.Equals(parameters[0].Type, folder.ReturnType),
                    ILoopOperation => held is ILocalSymbol,
                    _ => false,
                };
            Report<ICollectionExpressionOperation>(start, [OperationKind.CollectionExpression], (collection, _) =>
                Same(collection.Type, seq) && collection.Elements is { Length: 2 } elements && elements[0] is ISpreadOperation { Operand: var spread } && elements[1] is not ISpreadOperation
                && SymbolEqualityComparer.Default.Equals(spread.Type, collection.Type) && Variable(Stripped(spread)) is { } held && Stepped(collection, held)
                && (held is IParameterSymbol || (Consumer(collection) is ISimpleAssignmentOperation { Target: ILocalReferenceOperation target } && Same(target.Local, held)))
                    ? [Diagnostic.Create(Rule, collection.Syntax.GetLocation(), collection.Syntax, held.Name, elements[1].Syntax)]
                    : []);
        });
    }
}

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class RedundantNarrowingAnalyzer : DiagnosticAnalyzer {

    // --- [RULES]

    private static readonly DiagnosticDescriptor Rule = new(
        "RASM1007",
        "Redundant `As`, delete the call",
        "`{0}()` narrows `{1}` to itself, delete the call",
        Categories.LanguageExt,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        "Delete parameterless narrowing calls on a `K<F, A>` conversion returning the converted value's type.",
        Link("RASM1007"));

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    public override void Initialize(AnalysisContext context) {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.RegisterCompilationStartAction(static start => {
            if (WellKnownTypeProvider.GetOrCreate(start.Compilation).GetOrCreateTypeByMetadataName("LanguageExt.Traits.K`2") is not { } kind) return;
            ImmutableArray<IMethodSymbol> narrowings = [.. kind.ContainingNamespace.ContainingNamespace.GetTypeMembers().SelectMany(static type => Members(type, "As"))];
            Report<IInvocationOperation>(start, [OperationKind.Invocation], (call, _) =>
                Among(call.TargetMethod, narrowings) && Parameterless(call) && Receiver(call) is IConversionOperation { IsImplicit: true, Type: var trait, Operand.Type: { } held }
                && Same(trait, kind) && SymbolEqualityComparer.Default.Equals(held, call.Type)
                    ? [Diagnostic.Create(Rule, Named(call), call.TargetMethod.Name, held.ToDisplayString(Shown))]
                    : []);
        });
    }
}

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class OptionHandlerAnalyzer : DiagnosticAnalyzer {

    // --- [RULES]

    private static readonly DiagnosticDescriptor Passed = new(
        "RASM1008",
        "`Match` passing one case through, call the one-handler member",
        "`{0}` returns `{1}` value unchanged, write `{2}`",
        Categories.LanguageExt,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        "Call a member taking the non-identity handler alone with `Match` result type over `Match` with an identity handler.",
        Link("RASM1008"));

    private static readonly DiagnosticDescriptor Nullable = new(
        "RASM1009",
        "`Option` to nullable outside its conversion member, call `ValueUnsafe` or `ToNullable` by element kind",
        "`Option` value `{0}` reaches a nullable outside its conversion member, write `{1}`",
        Categories.LanguageExt,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        "`Option` converts to a nullable through `ValueUnsafe` for a reference element and `ToNullable` for a value element. `Match`, `Case` casts, and `IsSome` or `IsNone` conditionals restate the conversion, and `ValueUnsafe` on a value element returns `default` for `None`.",
        Link("RASM1009"));

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Passed, Nullable];

    public override void Initialize(AnalysisContext context) {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.RegisterCompilationStartAction(static start => {
            WellKnownTypeProvider types = WellKnownTypeProvider.GetOrCreate(start.Compilation);
            if (types.GetOrCreateTypeByMetadataName("LanguageExt.Fin`1") is not { } fin || types.GetOrCreateTypeByMetadataName("LanguageExt.Option`1") is not { } option) return;
            ISymbol? identity = Declared(start.Compilation, "M:LanguageExt.Prelude.identity``1(``0)").FirstOrDefault();
            ISymbol? matched = Declared(start.Compilation, "P:LanguageExt.Option`1.Case").FirstOrDefault();
            ISymbol? some = Declared(start.Compilation, "P:LanguageExt.Option`1.IsSome").FirstOrDefault();
            ISymbol? none = Declared(start.Compilation, "P:LanguageExt.Option`1.IsNone").FirstOrDefault();
            IMethodSymbol? toNullable = Members(types.GetOrCreateTypeByMetadataName("LanguageExt.OptionExtensions"), "ToNullable").SingleOrDefault();
            IMethodSymbol? unsafeValue = Members(types.GetOrCreateTypeByMetadataName("LanguageExt.UnsafeValueAccess.UnsafeValueAccessExtensions"), "ValueUnsafe")
                .FirstOrDefault(method => Same(method.Parameters[0].Type, option));
            ImmutableArray<IMethodSymbol> maps = Members(option, "Map");
            ImmutableArray<IMethodSymbol> matches = [.. Members(fin, "Match"), .. Members(option, "Match")];
            ImmutableArray<IMethodSymbol> fallbacks = Members(option, "IfNone");
            ITypeSymbol? Identity(IOperation value) =>
                Stripped(value) switch {
                    IDelegateCreationOperation { Target: IAnonymousFunctionOperation { Symbol.Parameters: { Length: 1 } parameters } function }
                        when Sole(function.Body) is IParameterReferenceOperation reference && Same(reference.Parameter, parameters[0]) => parameters[0].Type,
                    IDelegateCreationOperation { Target: IMethodReferenceOperation { Method: { Parameters: { Length: 1 } parameters } method } } when Same(method, identity) => parameters[0].Type,
                    _ => null,
                };
            static bool Empty(IOperation value) {
                while (Stripped(value) is IDelegateCreationOperation { Target: IAnonymousFunctionOperation { Symbol.Parameters.IsEmpty: true } function }) {
                    if (Sole(function.Body) is not { } returned) return false;
                    value = returned;
                }
                return Stripped(value) is IDefaultValueOperation or { ConstantValue: { HasValue: true, Value: null } };
            }
            bool Nulled(ITypeSymbol element, ITypeSymbol? produced) =>
                element.IsReferenceType ? SymbolEqualityComparer.Default.Equals(produced, element)
                : produced is INamedTypeSymbol { OriginalDefinition.SpecialType: SpecialType.System_Nullable_T } nullable && SymbolEqualityComparer.Default.Equals(nullable.TypeArguments[0], element);
            object[] Read(IOperation holder, ITypeSymbol element) => [holder.Syntax, $"{holder.Syntax}.{(element.IsReferenceType ? unsafeValue : toNullable)!.Name}()"];
            bool Held(IOperation? first, IOperation? second, CancellationToken token) {
                while ((first, second) is (IMemberReferenceOperation one, IMemberReferenceOperation other) && Same(one.Member, other.Member) && one switch {
                    IFieldReferenceOperation field => !field.Field.IsVolatile,
                    IPropertyReferenceOperation property => property.Property.GetMethod is { } getter
                        && (getter.IsImplicitlyDeclared || (getter.DeclaringSyntaxReferences is { IsEmpty: false } declarations
                            && declarations.All(declared => declared.GetSyntax(token) is AccessorDeclarationSyntax { Body: null, ExpressionBody: null })))
                        && property.Property.ContainingType.GetMembers().OfType<IFieldSymbol>().Any(field => Same(field.AssociatedSymbol, property.Property)),
                    _ => false,
                })
                    (first, second) = (one.Instance, other.Instance);
                return (first, second) switch {
                    (ILocalReferenceOperation one, ILocalReferenceOperation other) => Same(one.Local, other.Local),
                    (IParameterReferenceOperation one, IParameterReferenceOperation other) => Same(one.Parameter, other.Parameter),
                    (IInstanceReferenceOperation, IInstanceReferenceOperation) or (null, null) => true,
                    _ => false,
                };
            }
            bool Unwraps(IOperation value, IOperation tested, CancellationToken token) => Stripped(value) is IConversionOperation { IsImplicit: false, Operand: var source } && Held(source, tested, token);
            (DiagnosticDescriptor Rule, object[] Arguments)? Handled(IInvocationOperation call) {
                if (Receiver(call) is not { } receiver || call.TargetMethod.ContainingType is not { } owner || (!Same(owner, fin) && !Same(owner, option))) return null;
                ImmutableArray<IArgumentOperation> handled = [.. call.Arguments.Where(argument => argument.Value != receiver)];
                bool optional = Same(owner, option);
                ITypeSymbol element = owner.TypeArguments[0];
                object[] unwrapped = Read(receiver, element);
                if (optional && Among(call.TargetMethod, fallbacks) && handled.Length is 1 && Empty(handled[0].Value) && Nulled(element, call.Type))
                    return (Nullable, unwrapped);
                if (handled.Length is not 2 || !Among(call.TargetMethod, matches) || !handled.All(static argument => argument.Parameter?.Type.TypeKind is TypeKind.Delegate)) return null;
                IArgumentOperation? kept = handled.FirstOrDefault(argument => Identity(argument.Value) is not null);
                IArgumentOperation other = handled.First(argument => argument != kept);
                return (kept, optional && Empty(other.Value) && Nulled(element, call.Type)) switch {
                    (not null, true) => (Nullable, unwrapped),
                    (null, _) when optional && handled.FirstOrDefault(static argument => !Empty(argument.Value)) is { } mapped && handled.Any(static argument => Empty(argument.Value))
                        && call.Type is { IsReferenceType: true } or INamedTypeSymbol { OriginalDefinition.SpecialType: SpecialType.System_Nullable_T } =>
                        (Nullable, [receiver.Syntax, $"{receiver.Syntax}.{maps[0].Name}<{call.Type.ToDisplayString(Shown)}>({mapped.Value.Syntax}).{unsafeValue!.Name}()"]),
                    ( { Value: var value, Parameter: var parameter }, false) when SymbolEqualityComparer.Default.Equals(Identity(value), call.Type)
                        && owner.GetMembers().OfType<IMethodSymbol>().FirstOrDefault(member => member is { IsStatic: false, Parameters.Length: 1 } && !Same(member, call.TargetMethod)
                            && SymbolEqualityComparer.IncludeNullability.Equals(member.Parameters[0].Type, other.Parameter!.Type)
                            && SymbolEqualityComparer.IncludeNullability.Equals(member.ReturnType, call.Type)) is { } named =>
                        (Passed, [call.TargetMethod.Name, parameter!.Name, $"{receiver.Syntax}.{named.Name}({other.Value.Syntax})"]),
                    _ => null,
                };
            }
            Report<IOperation>(start, [OperationKind.Invocation, OperationKind.Conversion, OperationKind.Conditional], (operation, analysis) =>
                operation switch {
                    IInvocationOperation call when Handled(call) is { } handled => [Diagnostic.Create(handled.Rule, call.Syntax.GetLocation(), handled.Arguments)],
                    IInvocationOperation { TargetMethod.TypeArguments: { Length: 1 } elements } call when Same(call.TargetMethod, unsafeValue) && elements[0].IsValueType
                        && elements[0].OriginalDefinition.SpecialType is not SpecialType.System_Nullable_T && Receiver(call) is { } holder =>
                        [Diagnostic.Create(Nullable, call.Syntax.GetLocation(), Read(holder, elements[0]))],
                    IConversionOperation { IsImplicit: false, Type: { } target } cast when Stripped(cast.Operand) is IPropertyReferenceOperation { Instance: { } holder, Property.ContainingType: var owner } read
                        && Same(read.Property, matched) && Nulled(owner.TypeArguments[0], target) =>
                        [Diagnostic.Create(Nullable, cast.Syntax.GetLocation(), Read(holder, owner.TypeArguments[0]))],
                    IConditionalOperation { Condition: IPropertyReferenceOperation { Instance: { } tested, Property.ContainingType: var owner } test, WhenTrue: var present, WhenFalse: { } absent } conditional
                        when ((Same(test.Property, some) && Unwraps(present, tested, analysis.CancellationToken) && Empty(absent))
                                || (Same(test.Property, none) && Unwraps(absent, tested, analysis.CancellationToken) && Empty(present)))
                            && Nulled(owner.TypeArguments[0], conditional.Type) =>
                        [Diagnostic.Create(Nullable, conditional.Syntax.GetLocation(), Read(tested, owner.TypeArguments[0]))],
                    _ => [],
                });
        });
    }
}

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class RecordLensAnalyzer : DiagnosticAnalyzer {

    // --- [RULES]

    private static readonly DiagnosticDescriptor Rule = new(
        "RASM1010",
        "Record property lens written by hand, write `with` at the edit",
        "`{0}` restates {1} `{2}` of `{3}`, write `{4}` at the edit",
        Categories.LanguageExt,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        "`Lens<A, B>.New` getters and `with` setters over one positional record property path become `with` at the edit.",
        Link("RASM1010"));

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    public override void Initialize(AnalysisContext context) {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.RegisterCompilationStartAction(static start => {
            ImmutableArray<IMethodSymbol> lenses = Members(WellKnownTypeProvider.GetOrCreate(start.Compilation).GetOrCreateTypeByMetadataName("LanguageExt.Lens`2"), "New");
            if (lenses.IsEmpty) return;
            static ImmutableArray<IPropertySymbol>? Read(IOperation value, IParameterSymbol root, CancellationToken token) {
                Stack<IPropertySymbol> path = new();
                while (Stripped(value) is IPropertyReferenceOperation { Instance: { } instance } reference) {
                    token.ThrowIfCancellationRequested();
                    path.Push(reference.Property);
                    value = instance;
                }
                return Stripped(value) is IParameterReferenceOperation parameter && Same(parameter.Parameter, root) ? [.. path] : null;
            }
            static ImmutableArray<IPropertySymbol>? Written(IOperation value, IParameterSymbol root, IParameterSymbol held, CancellationToken token) {
                ImmutableArray<IPropertySymbol>.Builder path = ImmutableArray.CreateBuilder<IPropertySymbol>();
                while (Stripped(value) is IWithOperation { Operand: var operand, Initializer.Initializers: { Length: 1 } sets }
                    && sets[0] is ISimpleAssignmentOperation { Target: IPropertyReferenceOperation target, Value: var next }
                    && Read(operand, root, token) is { } read && Enumerable.SequenceEqual(read, path, SymbolEqualityComparer.Default)) {
                    token.ThrowIfCancellationRequested();
                    path.Add(target.Property);
                    value = next;
                }
                return Stripped(value) is IParameterReferenceOperation parameter && Same(parameter.Parameter, held) ? path.DrainToImmutable() : null;
            }
            Report<IInvocationOperation>(start, [OperationKind.Invocation], (call, analysis) => {
                bool Positional(IPropertySymbol property) =>
                    property.ContainingType is { IsRecord: true } owner && Sourced(owner) && property.DeclaringSyntaxReferences.Any(reference => reference.GetSyntax(analysis.CancellationToken) is ParameterSyntax);
                return !Among(call.TargetMethod, lenses) || Lambda(call.Arguments.Single(static argument => argument.Parameter!.Ordinal is 0).Value) is not { Symbol.Parameters.Length: 1 } getter || Sole(getter.Body) is not { } got || Read(got, getter.Symbol.Parameters[0], analysis.CancellationToken) is not { IsEmpty: false } path
                    || Lambda(call.Arguments.Single(static argument => argument.Parameter!.Ordinal is 1).Value) is not { Symbol.Parameters.Length: 1 } setter || Sole(setter.Body) is not { } inner || Lambda(inner) is not { Symbol.Parameters.Length: 1 } writer
                    || Sole(writer.Body) is not { } written || Written(written, writer.Symbol.Parameters[0], setter.Symbol.Parameters[0], analysis.CancellationToken) is not { } wrote || !Enumerable.SequenceEqual(wrote, path, SymbolEqualityComparer.Default) || !path.All(Positional)
                    ? []
                    : [
                    Diagnostic.Create(
                        Rule,
                        call.Syntax.GetLocation(),
                        $"{call.TargetMethod.ContainingType.ToDisplayString(Shown)}.{call.TargetMethod.Name}",
                        path.Length is 1 ? "property" : "path",
                        string.Join(".", path.Select(static property => property.Name)),
                        getter.Symbol.Parameters[0].Type.ToDisplayString(Shown),
                        written.Syntax),
                    ];
            });
        });
    }
}

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class LiftedContinuationAnalyzer : DiagnosticAnalyzer {

    // --- [RULES]

    private static readonly DiagnosticDescriptor Rule = new(
        "RASM1011",
        "`IO.lift` thunk or `IO.pure` value returned from a continuation, write the value",
        "`{0}` continuation returns `{1}`, write `{2}`",
        Categories.LanguageExt,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        "`IO` `Bind` and `SelectMany` continuations return no `IO.lift` thunk or `IO.pure` value as their whole result, a body with no call or side effect becomes `Map` under `Bind` and `let` in a later `from` clause, a `Fin` or `Either` thunk lifts its value.",
        Link("RASM1011"));

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    public override void Initialize(AnalysisContext context) {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.RegisterCompilationStartAction(static start => {
            WellKnownTypeProvider types = WellKnownTypeProvider.GetOrCreate(start.Compilation);
            if (types.GetOrCreateTypeByMetadataName("LanguageExt.IO") is not { } io || types.GetOrCreateTypeByMetadataName("LanguageExt.IO`1") is not { } effect) return;
            (ImmutableArray<IMethodSymbol> lifts, ImmutableArray<IMethodSymbol> pures, ImmutableArray<IMethodSymbol> binds, ImmutableArray<IMethodSymbol> maps) = (Members(io, "lift"), Members(io, "pure"), Members(effect, "Bind"), Members(effect, "Map"));
            ImmutableArray<IMethodSymbol> continuations = [.. binds, .. Members(effect, "SelectMany")];
            ImmutableArray<IMethodSymbol> narrowings = [.. io.ContainingNamespace.GetTypeMembers().SelectMany(static type => Members(type, "As"))];
            ImmutableArray<ITypeSymbol> values = [.. lifts.Where(static lift => lift.Parameters is { Length: 1 } parameters && parameters[0].Type.TypeKind is not TypeKind.Delegate).Select(static lift => lift.Parameters[0].Type.OriginalDefinition)];
            if (lifts.IsEmpty || maps.IsEmpty) return;
            bool Valued(ITypeSymbol? type) => type is not null && values.Contains(type.OriginalDefinition, SymbolEqualityComparer.Default);
            bool Held(IOperation? receiver, CancellationToken token) {
                IOperation? source = receiver is null ? null : Stripped(receiver);
                if (source is IInvocationOperation narrowed && Among(narrowed.TargetMethod, narrowings) && Parameterless(narrowed)) source = Receiver(narrowed) is { } inner ? Stripped(inner) : null;
                if (source is ILocalReferenceOperation { Local.DeclaringSyntaxReferences: { Length: 1 } declared } local
                    && declared[0].GetSyntax(token) is VariableDeclaratorSyntax { Initializer.Value: var initial }) source = local.SemanticModel?.GetOperation(initial, token);
                return source is IInvocationOperation { Syntax: InvocationExpressionSyntax { Expression: MemberAccessExpressionSyntax { Name: GenericNameSyntax } } } lifted
                    && Among(lifted.TargetMethod, lifts) && Valued(lifted.TargetMethod.TypeArguments[0]);
            }
            Report<IInvocationOperation>(start, [OperationKind.Invocation], (step, analysis) => {
                (IOperation? body, ExpressionSyntax? syntax) = step.Arguments is not { Length: 1 } arguments ? (null, null)
                    : Lambda(arguments[0].Value) is { Symbol.Parameters.IsEmpty: true, Syntax: LambdaExpressionSyntax { ExpressionBody: { } thunked } and not ParenthesizedLambdaExpressionSyntax { ReturnType: not null } } thunk
                        ? (Among(step.TargetMethod, lifts) ? (Sole(thunk.Body), thunked) : (null, null))
                    : Among(step.TargetMethod, pures) && arguments[0].Value.Syntax is ExpressionSyntax given ? (Stripped(arguments[0].Value), given)
                    : (null, null);
                if (body is null || syntax is null
                    || Consumer(step) is not IReturnOperation { Parent: IBlockOperation { Parent: IAnonymousFunctionOperation outer } } || Sole(outer.Body) != step
                    || Consumer(outer) is not IDelegateCreationOperation creation || Consumer(creation) is not IArgumentOperation { Parent: IInvocationOperation bind } || !Among(bind.TargetMethod, continuations))
                    return [];
                ITypeSymbol accepted = step.TargetMethod.OriginalDefinition.Parameters[0].Type;
                ITypeSymbol? produced = accepted is INamedTypeSymbol { DelegateInvokeMethod: { } invoke } ? invoke.ReturnType : accepted;
                IParameterSymbol? parameter = outer.Symbol.Parameters.FirstOrDefault();
                string shown = syntax.ToString();
                bool projected = !body.DescendantsAndSelf().Any(static operation => operation is IInvocationOperation or IDynamicInvocationOperation or IAssignmentOperation or IIncrementOrDecrementOperation
                    or IEventAssignmentOperation or IRaiseEventOperation or IDynamicObjectCreationOperation or ITypeParameterObjectCreationOperation or IObjectCreationOperation { Type.IsRecord: false });
                (string Text, ImmutableDictionary<string, string?> Properties, Location Location)? written = (produced, outer.Syntax, step.Syntax.Parent) switch {
                    (ITypeParameterSymbol, _, FromClauseSyntax { Type: null, Parent: QueryBodySyntax } clause)
                        when projected && !Among(bind.TargetMethod, binds) && SymbolEqualityComparer.Default.Equals(body.Type, step.TargetMethod.TypeArguments[0]) =>
                        ($"let {clause.Identifier.Text} = {shown}", ImmutableDictionary<string, string?>.Empty.Add(nameof(LetClauseSyntax), value: null), step.Syntax.GetLocation()),
                    (ITypeParameterSymbol, LambdaExpressionSyntax { AsyncKeyword.RawKind: 0, ExpressionBody: { } lifted } continuation, _)
                        when projected && continuation is not ParenthesizedLambdaExpressionSyntax { ReturnType: not null } && Among(bind.TargetMethod, binds)
                        && bind.Syntax is InvocationExpressionSyntax { Expression: MemberAccessExpressionSyntax access } && bind.TargetMethod.TypeArguments is { Length: 1 } bound =>
                        (body, SymbolEqualityComparer.Default.Equals(body.Type, bound[0])) switch {
                            (IParameterReferenceOperation reference, true) when Same(reference.Parameter, parameter) =>
                                (access.Expression.ToString(), ImmutableDictionary<string, string?>.Empty.Add(nameof(IParameterReferenceOperation), value: null), Named(bind)),
                            (_, true) =>
                                ($"{maps[0].Name}({continuation.ToString().Substring(0, lifted.SpanStart - continuation.SpanStart)}{shown})",
                                    ImmutableDictionary<string, string?>.Empty.Add(nameof(IMethodSymbol), maps[0].Name), Named(bind)),
                            _ => bound[0].ToMinimalDisplayString(step.SemanticModel!, step.Syntax.SpanStart) switch {
                                var type => ($"{maps[0].Name}<{type}>({continuation.ToString().Substring(0, lifted.SpanStart - continuation.SpanStart)}{shown})",
                                    ImmutableDictionary<string, string?>.Empty.Add(nameof(IMethodSymbol), maps[0].Name).Add(nameof(ITypeSymbol), type), Named(bind)),
                            },
                        },
                    (INamedTypeSymbol, _, _) when Valued(produced) && !Held(Receiver(bind), analysis.CancellationToken) =>
                        (FormattableString.Invariant($"{((InvocationExpressionSyntax)step.Syntax).Expression}({shown})"), ImmutableDictionary<string, string?>.Empty, step.Syntax.GetLocation()),
                    _ => null,
                };
                return written is { } found ? [Diagnostic.Create(Rule, found.Location, found.Properties, bind.TargetMethod.Name, step.Syntax, found.Text)] : [];
            });
        });
    }
}

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class MapFlattenAnalyzer : DiagnosticAnalyzer {

    // --- [RULES]

    private static readonly DiagnosticDescriptor Rule = new(
        "RASM1012",
        "`Map` then `Flatten`, call `Bind`",
        "`{0}` spells `Bind` as `Map` then `Flatten`, call `Bind` with a lambda return type over untyped parameters (`Fin<T> (x) => …`)",
        Categories.LanguageExt,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        "LanguageExt `Map` over a one-parameter function followed by `Flatten` restates `Bind`, a lambda return type over untyped parameters binds the continuation once against the `Bind` overloads.",
        Link("RASM1012"));

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    public override void Initialize(AnalysisContext context) {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.RegisterCompilationStartAction(static start => {
            if (WellKnownTypeProvider.GetOrCreate(start.Compilation).GetOrCreateTypeByMetadataName("LanguageExt.Fin`1")?.ContainingAssembly is not { } owner) return;
            bool Owned(IInvocationOperation call, string name) => string.Equals(call.TargetMethod.Name, name, StringComparison.Ordinal) && Same(call.TargetMethod.ContainingAssembly, owner);
            Report<IInvocationOperation>(start, [OperationKind.Invocation], (flatten, _) =>
                Owned(flatten, "Flatten") && Parameterless(flatten) && Receiver(flatten) is { } receiver && Stripped(receiver) is IInvocationOperation map && Owned(map, "Map")
                && map.Arguments.Where(argument => argument.Value != Receiver(map)).ToImmutableArray() is { Length: 1 } stepped
                && stepped[0].Parameter?.Type is INamedTypeSymbol { DelegateInvokeMethod.Parameters.Length: 1 }
                    ? [Diagnostic.Create(Rule, flatten.Syntax.GetLocation(), flatten.Syntax)]
                    : []);
        });
    }
}

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class CatchPredicateAnalyzer : DiagnosticAnalyzer {

    // --- [RULES]

    private static readonly DiagnosticDescriptor Rule = new(
        "RASM1013",
        "`Catch` predicate bypassed by an evaluated failure, apply it through `@catch`",
        "`Catch` ignores its predicate for an evaluated failure, use `| @catch(...)`",
        Categories.LanguageExt,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        "`IO` failure cases bypass the predicate passed to `Catch`, the catch operator applies the predicate before invoking recovery.",
        Link("RASM1013"));

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    public override void Initialize(AnalysisContext context) {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.RegisterCompilationStartAction(static start => {
            WellKnownTypeProvider types = WellKnownTypeProvider.GetOrCreate(start.Compilation);
            if (types.GetOrCreateTypeByMetadataName("LanguageExt.IO") is not { } module || types.GetOrCreateTypeByMetadataName("LanguageExt.IO`1") is not { } io) return;
            INamedTypeSymbol? kind = types.GetOrCreateTypeByMetadataName("LanguageExt.Traits.K`2");
            ControlFlow flow = ControlFlow.For(start.Compilation);
            ImmutableArray<IParameterSymbol> predicates = [.. Members(io, "Catch").SelectMany(static method => method.Parameters)
                .Where(static parameter => parameter.Type is INamedTypeSymbol { DelegateInvokeMethod.ReturnType.SpecialType: SpecialType.System_Boolean })];
            ImmutableArray<IMethodSymbol> suspended = [
                .. Members(module, "pure", "pureAsync", "pureVAsync"),
                .. Members(module, "lift", "liftAsync", "liftVAsync").Where(Delegated),
                .. Members(io, "Uninterruptible", "Timeout", "ApplyBack"),
            ];
            ImmutableArray<IMethodSymbol> recoveries = Members(io, "Catch", "IfFail", "Match", "Combine");
            ImmutableArray<IMethodSymbol> narrowed = Members(types.GetOrCreateTypeByMetadataName("LanguageExt.IOExtensions"), "As");
            bool Evaluated(IOperation source, CancellationToken token) {
                Stack<ControlFlow.Value> remaining = new([ControlFlow.At(source)]);
                HashSet<ControlFlow.Value> seen = [];
                while (remaining.Count is not 0) {
                    token.ThrowIfCancellationRequested();
                    ControlFlow.Value current = remaining.Pop();
                    if (!seen.Add(current)) continue;
                    IEnumerable<ControlFlow.Value>? next = current.Operation switch {
                        IParenthesizedOperation grouped => [current.With(grouped.Operand)],
                        IConversionOperation { OperatorMethod.ContainingType: { } owner } when Same(owner, io) => [],
                        IConversionOperation { OperatorMethod: null } converted => [current.With(converted.Operand)],
                        IConditionalOperation { WhenFalse: { } absent } conditional => [current.With(conditional.WhenTrue), current.With(absent)],
                        ICoalesceOperation coalesced => [current.With(coalesced.Value), current.With(coalesced.WhenNull)],
                        ISwitchExpressionOperation switched => switched.Arms.Select(arm => current.With(arm.Value)),
                        ILocalReferenceOperation or IFlowCaptureReferenceOperation when flow.Values(current, token) is var values && values.All(static definition => definition.HasValue) => values.Select(static definition => definition.Value),
                        IInvocationOperation call when Among(call.TargetMethod, suspended) => [],
                        IInvocationOperation call when Among(call.TargetMethod, recoveries) => call.Arguments
                            .Where(argument => argument.Parameter?.OriginalDefinition.Type is INamedTypeSymbol parameter &&
                                (Same(parameter, io) || (parameter.DelegateInvokeMethod?.ReturnType is INamedTypeSymbol returned &&
                                    (Same(returned, io) || Same(returned, kind)))))
                            .Select(argument => Lambda(argument.Value) is { } handler && Sole(handler.Body) is { } returned ? ControlFlow.At(returned) : current.With(argument.Value)),
                        IInvocationOperation { Instance: { } receiver } call when Same(call.TargetMethod.ContainingType, io) => [current.With(receiver)],
                        IInvocationOperation { Arguments: { Length: 1 } arguments } call when Among(call.TargetMethod, narrowed) => [current.With(arguments[0].Value)],
                        _ => null,
                    };
                    if (next is null) return true;
                    foreach (ControlFlow.Value child in next) remaining.Push(child);
                }
                return false;
            }
            Report<IInvocationOperation>(start, [OperationKind.Invocation], (call, analysis) =>
                call is { Syntax: InvocationExpressionSyntax { Expression: MemberAccessExpressionSyntax }, Instance: { } source }
                && call.Arguments.FirstOrDefault(argument => Among(argument.Parameter, predicates)) is { } predicate
                && !(Lambda(predicate.Value) is { } function && Sole(function.Body)?.ConstantValue is { HasValue: true, Value: true })
                && Evaluated(source, analysis.CancellationToken)
                    ? [Diagnostic.Create(Rule, call.Syntax.GetLocation())]
                    : []);
        });
    }
}

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class HandPairedExclusionAnalyzer : DiagnosticAnalyzer {

    // --- [RULES]

    private static readonly DiagnosticDescriptor Rule = new(
        "RASM1014",
        "Exclusion paired by hand, run it as one conduit",
        "`{0}` pairs wait and release by hand, run exclusion as one `Conduit` under `Fork` or `AsyncSemaphore.EnterAsync()` under `use`",
        Categories.LanguageExt,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        "`SemaphoreSlim` and `ConcurrentExclusiveSchedulerPair` leave each wait and release to the caller, a LanguageExt `Conduit` under `Fork` serializes the work and an `AsyncSemaphore` scope releases on every path.",
        Link("RASM1014"));

    private static readonly ImmutableArray<string> Pairs = ["System.Threading.SemaphoreSlim", "System.Threading.Tasks.ConcurrentExclusiveSchedulerPair"];

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    public override void Initialize(AnalysisContext context) {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.RegisterCompilationStartAction(static start => {
            WellKnownTypeProvider types = WellKnownTypeProvider.GetOrCreate(start.Compilation);
            if (types.GetOrCreateTypeByMetadataName("LanguageExt.Prelude") is null) return;
            ImmutableArray<INamedTypeSymbol> pairs = [.. Pairs.Select(types.GetOrCreateTypeByMetadataName).OfType<INamedTypeSymbol>()];
            Report<IObjectCreationOperation>(start, [OperationKind.ObjectCreation], (creation, _) =>
                Among(creation.Type, pairs) ? [Diagnostic.Create(Rule, creation.Syntax.GetLocation(), creation.Syntax)] : []);
        });
    }
}

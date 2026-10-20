using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace Rasm.Analyzers;

// --- [COMPOSITION] ---------------------------------------------------------------------
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class LanguageExtAnalyzer : RasmAnalyzer {
    // --- [RULES]
    private const string Domain = "LanguageExt";

    public static readonly DiagnosticDescriptor Lift = Rule(
        "RASM1001",
        Domain,
        "Explicit result lift",
        "`{0}` restates the implicit conversion of `{1}` to `{2}`",
        "Write the value alone where its position fixes the `Fin` or `Validation` type, the implicit conversion builds the same case. Where only a lift fixes the type of sibling arms, the first lift stays and the later ones are reported.");

    public static readonly DiagnosticDescriptor TypeTest = Rule(
        "RASM1003",
        Domain,
        "Type test on Error",
        "Type test on `Error` value `{0}` misses `ManyErrors` leaves",
        "Test with `IsType<E>()`, `IsExpected`, or `IsExceptional`, and bind fields through `Filter<E>().Head is E <case>`. Type tests hold on a leaf from `Head`, `AsIterable`, `FoldM`, or `ForAllM`, and after a `ManyErrors` arm.");

    public static readonly DiagnosticDescriptor RoundTrip = Rule(
        "RASM1004",
        Domain,
        "Fin round trip through Validation",
        "`{0}` round-trips `Fin` through `Validation`",
        "Apply and traverse the `Fin` operands directly, `Fin` accumulates every error.");

    public static readonly DiagnosticDescriptor Spread = Rule(
        "RASM1005",
        Domain,
        "Seq accumulator copied per step",
        "`{0}` copies `{1}` on every step",
        "Append with `Seq.Add`, which shares the accumulated sequence where a spread copies it.");

    public static readonly DiagnosticDescriptor LiftedApply = Rule(
        "RASM1006",
        Domain,
        "IO Apply over lifted Fin values",
        "`Apply` over {0} lifted `Fin` operands keeps one failure",
        "Apply the `Fin` values inside one `IO.lift`, where `Fin` accumulates every failure and `IO` keeps the first.");

    public static readonly DiagnosticDescriptor Narrowed = Rule(
        "RASM1007",
        Domain,
        "Redundant As",
        "`{0}` narrows `{1}` to itself",
        "Delete the call, `As` narrows a `K<F, A>` trait value to its concrete type and its receiver holds that type.");

    public static readonly DiagnosticDescriptor Passed = Rule(
        "RASM1008",
        Domain,
        "Match that passes one case through",
        "`{0}` returns one case unchanged, `{1}` passes it through and handles the other",
        "Call the named member, which takes the handler of the other case alone.");

    public static readonly DiagnosticDescriptor Nullable = Rule(
        "RASM1009",
        Domain,
        "Option converted to a nullable by hand",
        "`{0}` converts an `Option` to a nullable by hand, `{1}` states it",
        "Call `ValueUnsafe()` from `LanguageExt.UnsafeValueAccess` for a reference element, `ToNullable()` for a value element, and `Map<T?>(<some>).ValueUnsafe()` where `Some` maps the value.");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Lift, TypeTest, RoundTrip, Spread, LiftedApply, Narrowed, Passed, Nullable];

    protected override void Start(CompilationStartAnalysisContext context) {
        Compilation compilation = context.Compilation;
        if (compilation.GetTypeByMetadataName("LanguageExt.Fin`1") is not { } fin
            || compilation.GetTypeByMetadataName("LanguageExt.Validation`2") is not { } validation
            || compilation.GetTypeByMetadataName("LanguageExt.Common.Error") is not { } error
            || compilation.GetTypeByMetadataName("LanguageExt.Common.ManyErrors") is not { } many
            || compilation.GetTypeByMetadataName("LanguageExt.IO") is not { } io
            || compilation.GetTypeByMetadataName("LanguageExt.Seq`1") is not { } seq
            || compilation.GetTypeByMetadataName("LanguageExt.Traits.K`2") is not { } kind
            || compilation.GetTypeByMetadataName("LanguageExt.Option`1") is not { } option) return;
        Lifts(context, fin, validation);
        TypeTests(context, error, many);
        RoundTrips(context, fin, validation, kind);
        Spreads(context, seq);
        LiftedApplies(context, io, fin);
        Narrowings(context, kind);
        Matches(context, fin, option);
    }

    // --- [LIFTS]
    public static ExpressionSyntax? Unlifted(SyntaxNode lift) =>
        lift switch {
            InvocationExpressionSyntax { ArgumentList.Arguments: { Count: 1 } arguments } => arguments[0].Expression,
            BaseObjectCreationExpressionSyntax { ArgumentList.Arguments: { Count: 1 } arguments, Initializer: null } => arguments[0].Expression,
            CastExpressionSyntax cast => cast.Expression,
            _ => null,
        };

    private static void Lifts(CompilationStartAnalysisContext context, INamedTypeSymbol fin, INamedTypeSymbol validation) {
        ImmutableArray<ISymbol> factories = Declared(
            context.Compilation,
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
        (IOperation Lift, INamedTypeSymbol Target, IMethodSymbol Conversion)? Candidate(IOperation operation) {
            IParameterSymbol? parameter = operation switch {
                IInvocationOperation { Arguments: { Length: 1 } arguments } call when factories.Any(factory => Same(call.TargetMethod, factory)) => arguments[0].Parameter,
                IObjectCreationOperation { Arguments: { Length: 1 } arguments, Initializer: null, Constructor: { } made } when cases.Any(held => Same(made.ContainingType, held)) => arguments[0].Parameter,
                IConversionOperation { IsImplicit: false, Syntax: CastExpressionSyntax, OperatorMethod.Parameters: { Length: 1 } operands, Parent: IConditionalOperation or ISwitchExpressionArmOperation } => operands[0],
                _ => null,
            };
            return parameter is not null
                && (operation.Parent is IConversionOperation { IsImplicit: true } converted ? converted.Type : operation.Type) is INamedTypeSymbol target
                && (Same(target, fin) || Same(target, validation))
                && Unlifted(operation.Syntax) is { } value
                && operation.SemanticModel?.ClassifyConversion(value, target) is { IsImplicit: true, IsUserDefined: true, MethodSymbol: { Parameters: { Length: 1 } sources } conversion }
                && SymbolEqualityComparer.Default.Equals(sources[0].Type, parameter.Type)
                ? (operation, target, conversion)
                : null;
        }
        context.RegisterOperationBlockAction(block => {
            foreach (IGrouping<SyntaxNode?, (IOperation Lift, INamedTypeSymbol Target, IMethodSymbol Conversion)> scope in block.OperationBlocks
                .SelectMany(static root => root.DescendantsAndSelf())
                .Select(Candidate)
                .OfType<(IOperation Lift, INamedTypeSymbol Target, IMethodSymbol Conversion)>()
                .GroupBy(static lift => Scope(lift.Lift.Syntax)))
                if (scope.Key is { } root)
                    foreach ((IOperation lift, INamedTypeSymbol target, _) in scope.Reverse().Aggregate(
                        ImmutableArray<(IOperation Lift, INamedTypeSymbol Target, IMethodSymbol Conversion)>.Empty,
                        (removed, next) => Holds(root, [.. removed, next], block.CancellationToken) ? removed.Add(next) : removed))
                        block.ReportDiagnostic(Diagnostic.Create(Lift, lift.Syntax.GetLocation(), lift.Syntax, Unlifted(lift.Syntax), target.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat)));
        });
    }

    private static SyntaxNode? Scope(SyntaxNode node) =>
        node.Ancestors().FirstOrDefault(static held => held is AnonymousFunctionExpressionSyntax or LocalFunctionStatementSyntax or MemberDeclarationSyntax or AttributeSyntax) is AnonymousFunctionExpressionSyntax callback
            ? Scope(callback)
            : node.Ancestors().FirstOrDefault(static held => held is (StatementSyntax and not BlockSyntax)
                or ArrowExpressionClauseSyntax { Parent: not LocalFunctionStatementSyntax }
                or EqualsValueClauseSyntax { Parent: PropertyDeclarationSyntax or ParameterSyntax or VariableDeclaratorSyntax { Parent.Parent: FieldDeclarationSyntax } }
                or MemberDeclarationSyntax or AttributeSyntax) is { } scope and (StatementSyntax or ArrowExpressionClauseSyntax or EqualsValueClauseSyntax)
                ? scope
                : null;

    private static bool Holds(SyntaxNode scope, ImmutableArray<(IOperation Lift, INamedTypeSymbol Target, IMethodSymbol Conversion)> removed, CancellationToken token) {
        if (removed[0].Lift.SemanticModel is not { } model) return false;
        ImmutableArray<SyntaxNode> sites = [.. removed.Select(static lift => lift.Lift.Syntax)];
        ImmutableArray<SyntaxAnnotation> marks = [.. removed.Select(static _ => new SyntaxAnnotation())];
        SyntaxAnnotation outer = new();
        SyntaxNode bare = scope.ReplaceNodes(sites, (site, rewritten) => SyntaxFactory.ParenthesizedExpression(Unlifted(rewritten)!.WithoutTrivia()).WithTriviaFrom(rewritten).WithAdditionalAnnotations(marks[sites.IndexOf(site)]))
            .WithAdditionalAnnotations(outer);
        SemanticModel? speculative = bare switch {
            StatementSyntax statement when model.TryGetSpeculativeSemanticModel(scope.SpanStart, statement, out SemanticModel? held) => held,
            ArrowExpressionClauseSyntax arrow when model.TryGetSpeculativeSemanticModel(scope.SpanStart, arrow, out SemanticModel? held) => held,
            EqualsValueClauseSyntax initializer when model.TryGetSpeculativeSemanticModel(scope.SpanStart, initializer, out SemanticModel? held) => held,
            _ => null,
        };
        if (speculative?.SyntaxTree.GetRoot(token).GetAnnotatedNodes(outer).FirstOrDefault() is not { } rebound) return false;
        IEnumerable<ISymbol?> Calls(SemanticModel held, SyntaxNode root, ImmutableArray<SyntaxNode> skipped) =>
            root.DescendantNodesAndSelf().Where(static node => node is InvocationExpressionSyntax or BaseObjectCreationExpressionSyntax).Except(skipped).Select(node => held.GetSymbolInfo(node, token).Symbol);
        return removed.Select((lift, index) => rebound.GetAnnotatedNodes(marks[index]).FirstOrDefault() is ExpressionSyntax site
                && speculative.GetConversion(site, token) is { IsImplicit: true, IsUserDefined: true, MethodSymbol: var conversion }
                && SymbolEqualityComparer.Default.Equals(conversion, lift.Conversion)
                && SymbolEqualityComparer.Default.Equals(speculative.GetTypeInfo(site, token).ConvertedType, lift.Target))
            .All(static held => held)
            && Calls(model, scope, sites).SequenceEqual(Calls(speculative, rebound, []), SymbolEqualityComparer.Default);
    }

    // --- [TYPE_TESTS]
    private static void TypeTests(CompilationStartAnalysisContext context, INamedTypeSymbol error, INamedTypeSymbol many) {
        ImmutableArray<ISymbol> leaves = Declared(
            context.Compilation,
            "P:LanguageExt.Common.Error.Head",
            "M:LanguageExt.Common.Error.AsIterable",
            "M:LanguageExt.Common.Error.FoldM``2(System.Func{LanguageExt.Common.Error,LanguageExt.Traits.K{``0,``1}})",
            "M:LanguageExt.Common.Error.ForAllM``2(System.Func{LanguageExt.Common.Error,LanguageExt.Traits.K{``0,``1}})");
        ISymbol? runtime = context.Compilation.GetSpecialType(SpecialType.System_Object).GetMembers(nameof(GetType)).FirstOrDefault();
        bool Typed(IOperation value) => Same(value.Type, error);
        bool Case(ITypeSymbol? type) => !Same(type, error) && !Derives(type, many) && Derives(type, error);
        bool Narrows(IOperation pattern, Func<ITypeSymbol?, bool> wanted) =>
            pattern.DescendantsAndSelf().OfType<IPatternOperation>().Any(nested => Same(nested.InputType, error) && wanted(nested.NarrowedType));
        bool Visits(ISymbol? member) =>
            member is not null && (leaves.Any(leaf => Same(member, leaf)) || Visits(member switch { IMethodSymbol method => method.OverriddenMethod, IPropertySymbol property => property.OverriddenProperty, _ => null }));
        bool Iterated(IOperation? source) => source is not null && Stripped(source) is IInvocationOperation call && (Visits(call.TargetMethod) || Iterated(Receiver(call)));
        bool Leaf(IOperation tested) =>
            Stripped(tested) switch {
                IPropertyReferenceOperation reference => Visits(reference.Property),
                IParameterReferenceOperation { Parameter.ContainingSymbol: IMethodSymbol { MethodKind: MethodKind.AnonymousFunction } callback } reference =>
                    Ancestor<IAnonymousFunctionOperation>(reference, held => Same(held.Symbol, callback)) is { Parent: IDelegateCreationOperation { Parent: IArgumentOperation { Parent: IInvocationOperation call } } }
                    && (Visits(call.TargetMethod) || Iterated(Receiver(call))),
                ILocalReferenceOperation reference => Ancestor<IForEachLoopOperation>(reference, loop => loop.Locals.Any(local => Same(local, reference.Local))) is { } loop && Iterated(loop.Collection),
                _ => false,
            };
        IEnumerable<(IOperation Site, IOperation Tested)> Tests(IOperation operation) =>
            operation switch {
                IIsTypeOperation test when Typed(test.ValueOperand) && Case(test.TypeOperand) => [(test, test.ValueOperand)],
                IIsPatternOperation test when Typed(test.Value) && Narrows(test.Pattern, Case) && !Narrows(test.Pattern, held => Derives(held, many)) => [(test, test.Value)],
                IConversionOperation { IsImplicit: false } cast when Typed(cast.Operand) && Case(cast.Type) => [(cast, cast.Operand)],
                IInvocationOperation { Instance: { } instance } call when Typed(instance) && Same(call.TargetMethod, runtime) => [(call, instance)],
                _ when Dispatch(operation) is ( { } subject, var arms) && Typed(subject) =>
                    arms.Select(static arm => arm.Test).OfType<IPatternOperation>().TakeWhile(pattern => !Narrows(pattern, held => Derives(held, many)))
                        .Where(pattern => Narrows(pattern, Case)).Select(pattern => ((IOperation)pattern, subject)),
                _ => [],
            };
        context.RegisterOperationAction(
            analysis => {
                foreach ((IOperation site, IOperation tested) in Tests(analysis.Operation).Where(test => !Leaf(test.Tested)))
                    analysis.ReportDiagnostic(Diagnostic.Create(TypeTest, site.Syntax.GetLocation(), tested.Syntax));
            },
            OperationKind.IsType,
            OperationKind.IsPattern,
            OperationKind.Conversion,
            OperationKind.Invocation,
            OperationKind.SwitchExpression,
            OperationKind.Switch);
    }

    // --- [ROUND_TRIPS]
    private static void RoundTrips(CompilationStartAnalysisContext context, INamedTypeSymbol fin, INamedTypeSymbol validation, INamedTypeSymbol kind) {
        static IOperation? Source(IOperation operation, INamedTypeSymbol from, INamedTypeSymbol to) =>
            operation is IInvocationOperation call && Parameterless(call) && Receiver(call) is { } held && Same(held.Type, from) && Same(call.Type, to) ? Stripped(held) : null;
        ImmutableArray<IOperation> Hops(IOperation operand) =>
            Stripped(operand) is var held && Source(held, fin, validation) is not null
                ? [held]
                : Source(held, kind, validation) switch {
                    IInvocationOperation apply when Receiver(apply) is { } tupled && Stripped(tupled) is ITupleOperation tuple
                        && tuple.Elements.Select(Hops).ToImmutableArray() is var parts && parts.All(static part => !part.IsEmpty) => [.. parts.SelectMany(static part => part)],
                    IInvocationOperation traverse when traverse.Arguments.LastOrDefault()?.Value is IDelegateCreationOperation { Target: IAnonymousFunctionOperation { Body.Operations: { Length: 1 } body } } && body[0] is IReturnOperation { ReturnedValue: { } returned } =>
                        Hops(returned),
                    _ => [],
                };
        context.RegisterOperationAction(
            analysis => {
                IInvocationOperation exit = (IInvocationOperation)analysis.Operation;
                if (Source(exit, validation, fin) is { } inner && Hops(inner) is { IsEmpty: false } hops && (Source(inner, kind, validation) ?? inner) is IInvocationOperation named)
                    analysis.ReportDiagnostic(Diagnostic.Create(RoundTrip, Named(exit), [.. hops.Select(static hop => hop.Syntax.GetLocation()), exit.Syntax.GetLocation()], named.TargetMethod.Name));
            },
            OperationKind.Invocation);
    }

    // --- [SPREADS]
    private static void Spreads(CompilationStartAnalysisContext context, INamedTypeSymbol seq) {
        static ISymbol? Held(IOperation operation) =>
            Stripped(operation) switch {
                IParameterReferenceOperation parameter => parameter.Parameter,
                ILocalReferenceOperation local => local.Local,
                _ => null,
            };
        static bool Stepped(IOperation operation, ISymbol held) =>
            operation.Parent switch {
                null => false,
                IAnonymousFunctionOperation { Symbol: { Parameters: { Length: > 1 } parameters } folder } =>
                    Same(parameters[0], held) && SymbolEqualityComparer.Default.Equals(parameters[0].Type, folder.ReturnType),
                ILoopOperation => held is ILocalSymbol,
                var parent => Stepped(parent, held),
            };
        context.RegisterOperationAction(
            analysis => {
                ICollectionExpressionOperation collection = (ICollectionExpressionOperation)analysis.Operation;
                if (Same(collection.Type, seq) && collection.Elements is { Length: 2 } elements && elements[0] is ISpreadOperation { Operand: var spread } && elements[1] is not ISpreadOperation
                    && SymbolEqualityComparer.Default.Equals(spread.Type, collection.Type) && Held(spread) is { } held && Stepped(collection, held)
                    && (held is IParameterSymbol || (Consumer(collection) is ISimpleAssignmentOperation { Target: ILocalReferenceOperation target } && Same(target.Local, held))))
                    analysis.ReportDiagnostic(Diagnostic.Create(Spread, collection.Syntax.GetLocation(), collection.Syntax, held.Name));
            },
            OperationKind.CollectionExpression);
    }

    // --- [LIFTED_APPLIES]
    private static void LiftedApplies(CompilationStartAnalysisContext context, INamedTypeSymbol io, INamedTypeSymbol fin) {
        bool Raised(IOperation operand) =>
            Stripped(operand) is IInvocationOperation { TargetMethod: { Parameters: { Length: 1 } parameters } lift } && Same(lift.ContainingType, io) && parameters[0].Type is var raised
            && (Same(raised, fin) || (raised is INamedTypeSymbol { DelegateInvokeMethod: { Parameters.IsEmpty: true, ReturnType: var returned } } && Same(returned, fin)));
        context.RegisterOperationAction(
            analysis => {
                IInvocationOperation call = (IInvocationOperation)analysis.Operation;
                if (Receiver(call) is { } receiver && Stripped(receiver) is ITupleOperation { Elements.Length: > 1 } tuple && tuple.Elements.All(Raised))
                    analysis.ReportDiagnostic(Diagnostic.Create(LiftedApply, Named(call), tuple.Elements.Length));
            },
            OperationKind.Invocation);
    }

    // --- [NARROWINGS]
    private static void Narrowings(CompilationStartAnalysisContext context, INamedTypeSymbol kind) =>
        context.RegisterOperationAction(
            analysis => {
                IInvocationOperation call = (IInvocationOperation)analysis.Operation;
                if (Parameterless(call) && Receiver(call) is IConversionOperation { IsImplicit: true, Type: var trait, Operand.Type: { } held }
                    && Same(trait, kind) && SymbolEqualityComparer.Default.Equals(held, call.Type))
                    analysis.ReportDiagnostic(Diagnostic.Create(Narrowed, Named(call), call.TargetMethod.Name, held.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat)));
            },
            OperationKind.Invocation);

    // --- [MATCHES]
    private static void Matches(CompilationStartAnalysisContext context, INamedTypeSymbol fin, INamedTypeSymbol option) {
        ISymbol? identity = Declared(context.Compilation, "M:LanguageExt.Prelude.identity``1(``0)").FirstOrDefault();
        ISymbol? matched = Declared(context.Compilation, "P:LanguageExt.Option`1.Case").FirstOrDefault();
        ISymbol? some = Declared(context.Compilation, "P:LanguageExt.Option`1.IsSome").FirstOrDefault();
        ISymbol? none = Declared(context.Compilation, "P:LanguageExt.Option`1.IsNone").FirstOrDefault();
        ITypeSymbol? Identity(IOperation value) =>
            Stripped(value) switch {
                IDelegateCreationOperation { Target: IAnonymousFunctionOperation { Symbol.Parameters: { Length: 1 } parameters, Body.Operations: { Length: 1 } body } }
                    when body[0] is IReturnOperation { ReturnedValue: { } returned } && Stripped(returned) is IParameterReferenceOperation reference && Same(reference.Parameter, parameters[0]) => parameters[0].Type,
                IDelegateCreationOperation { Target: IMethodReferenceOperation { Method: { Parameters: { Length: 1 } parameters } method } } when Same(method, identity) => parameters[0].Type,
                _ => null,
            };
        static bool Empty(IOperation value) =>
            Stripped(value) switch {
                IDefaultValueOperation => true,
                { ConstantValue: { HasValue: true, Value: null } } => true,
                IDelegateCreationOperation { Target: IAnonymousFunctionOperation { Symbol.Parameters.IsEmpty: true, Body.Operations: { Length: 1 } body } } =>
                    body[0] is IReturnOperation { ReturnedValue: { } returned } && Empty(returned),
                _ => false,
            };
        bool Nulled(ITypeSymbol element, ITypeSymbol? produced) =>
            element.IsReferenceType || (produced is INamedTypeSymbol { OriginalDefinition.SpecialType: SpecialType.System_Nullable_T } nullable && SymbolEqualityComparer.Default.Equals(nullable.TypeArguments[0], element));
        static string Read(ITypeSymbol element) => element.IsReferenceType ? "ValueUnsafe()" : "ToNullable()";
        bool Held(IOperation? first, IOperation? second) =>
            (first, second) switch {
                (ILocalReferenceOperation one, ILocalReferenceOperation other) => Same(one.Local, other.Local),
                (IParameterReferenceOperation one, IParameterReferenceOperation other) => Same(one.Parameter, other.Parameter),
                (IFieldReferenceOperation one, IFieldReferenceOperation other) => Same(one.Field, other.Field) && Held(one.Instance, other.Instance),
                (IPropertyReferenceOperation one, IPropertyReferenceOperation other) => Same(one.Property, other.Property) && Held(one.Instance, other.Instance),
                (IInstanceReferenceOperation, IInstanceReferenceOperation) or (null, null) => true,
                _ => false,
            };
        bool Unwraps(IOperation value, IOperation tested) => Stripped(value) is IConversionOperation { IsImplicit: false, Operand: var source } && Held(source, tested);
        (DiagnosticDescriptor Rule, string Replacement)? Handled(IInvocationOperation call) {
            if (Receiver(call) is not { Type: INamedTypeSymbol owner } receiver || !SymbolEqualityComparer.Default.Equals(owner.ContainingAssembly, fin.ContainingAssembly)) return null;
            ImmutableArray<IArgumentOperation> handled = [.. call.Arguments.Where(argument => argument.Value != receiver)];
            bool optional = Same(owner, option);
            ITypeSymbol element = owner.TypeArguments.FirstOrDefault() ?? owner;
            string read = Read(element);
            if (optional && handled.Length is 1 && Empty(handled[0].Value) && SymbolEqualityComparer.Default.Equals(call.Type?.WithNullableAnnotation(NullableAnnotation.NotAnnotated), element) && Nulled(element, call.Type))
                return (Nullable, read);
            if (handled.Length is not 2 || !handled.All(static argument => argument.Parameter?.Type.TypeKind is TypeKind.Delegate)) return null;
            int kept = Identity(handled[0].Value) is not null ? 0 : Identity(handled[1].Value) is not null ? 1 : -1;
            IArgumentOperation other = handled[kept is 0 ? 1 : 0];
            return (kept, optional && Empty(other.Value) && Nulled(element, call.Type)) switch {
                ( >= 0, true) => (Nullable, read),
                (-1, _) when optional && handled.Any(static argument => Empty(argument.Value)) && call.Type is { IsReferenceType: true } or INamedTypeSymbol { OriginalDefinition.SpecialType: SpecialType.System_Nullable_T } => (Nullable, "Map<T?>(<some>).ValueUnsafe()"),
                ( >= 0, false) when SymbolEqualityComparer.Default.Equals(Identity(handled[kept].Value), call.Type)
                    && owner.GetMembers().OfType<IMethodSymbol>().FirstOrDefault(member => member is { IsStatic: false, Parameters.Length: 1 } && !Same(member, call.TargetMethod)
                        && SymbolEqualityComparer.IncludeNullability.Equals(member.Parameters[0].Type, other.Parameter!.Type)
                        && SymbolEqualityComparer.IncludeNullability.Equals(member.ReturnType, call.Type)) is { } named => (Passed, named.Name),
                _ => null,
            };
        }
        context.RegisterOperationAction(
            analysis => {
                (IOperation site, DiagnosticDescriptor rule, string replacement)? found = analysis.Operation switch {
                    IInvocationOperation call when Handled(call) is { } handled => (call, handled.Rule, handled.Replacement),
                    IConversionOperation { IsImplicit: false, Type: { } target } cast when Stripped(cast.Operand) is IPropertyReferenceOperation { Instance.Type: INamedTypeSymbol owner } read
                        && Same(read.Property, matched) && SymbolEqualityComparer.Default.Equals(target.WithNullableAnnotation(NullableAnnotation.NotAnnotated), owner.TypeArguments[0]) =>
                        (cast, Nullable, Read(owner.TypeArguments[0])),
                    IConditionalOperation { Condition: IPropertyReferenceOperation { Instance: { Type: INamedTypeSymbol owner } tested } test, WhenTrue: var present, WhenFalse: { } absent } conditional
                        when (Same(test.Property, some) && Unwraps(present, tested) && Empty(absent)) || (Same(test.Property, none) && Unwraps(absent, tested) && Empty(present)) =>
                        (conditional, Nullable, Read(owner.TypeArguments[0])),
                    _ => null,
                };
                if (found is ( { } site, { } rule, var replacement))
                    analysis.ReportDiagnostic(Diagnostic.Create(rule, site.Syntax.GetLocation(), site.Syntax, replacement));
            },
            OperationKind.Invocation,
            OperationKind.Conversion,
            OperationKind.Conditional);
    }
}

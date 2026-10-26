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
    private const string Category = "LanguageExt";

    public static readonly DiagnosticDescriptor Lift = Rule(
        "RASM1001",
        Category,
        "Explicit result lift",
        "`{0}` restates implicit conversion of `{1}` to `{2}`, write `{1}`",
        "`Fin` or `Validation` factory call, case construction, or arm cast an implicit conversion replaces with same overloads and arm types.");

    public static readonly DiagnosticDescriptor TypeTest = Rule(
        "RASM1003",
        Category,
        "Type test on Error",
        "{0} misses errors inside `ManyErrors`, call `{1}.IsType<{2}>()` or bind fields through `{1}.Filter<{2}>().Head is {2} <name>`",
        "Cast, type test, switch arm, or `GetType()` call on an `Error` value that can hold `ManyErrors`.");

    public static readonly DiagnosticDescriptor RoundTrip = Rule(
        "RASM1004",
        Category,
        "Fin round trip through Validation",
        "`{0}` round-trips `Fin` through `Validation`, delete each `{1}()` and the closing `{2}()`",
        "Parameterless `Validation` to `Fin` call over `Fin` operands converted to `Validation`, directly or through `Apply` or `Traverse`.");

    public static readonly DiagnosticDescriptor Spread = Rule(
        "RASM1005",
        Category,
        "Seq accumulator copied per step",
        "`{0}` copies `{1}` on every step, write `{1}.Add({2})`",
        "Collection expression `[.. <seq>, <item>]` appending to a `Seq` fold accumulator or loop local.");

    public static readonly DiagnosticDescriptor LiftedApply = Rule(
        "RASM1006",
        Category,
        "IO Apply over lifted Fin values",
        "`{0}` over {1} `IO.lift` operands keeps one `Fin` failure, call `{0}` on `Fin` values inside one `IO.lift`",
        "`Apply` over a tuple of `IO.lift` calls each lifting a `Fin` value or `Fin` thunk.");

    public static readonly DiagnosticDescriptor Narrowed = Rule(
        "RASM1007",
        Category,
        "Redundant As",
        "`{0}()` narrows `{1}` to itself, delete the call",
        "Parameterless narrowing call on a `K<F, A>` conversion returning the converted value's own type.");

    public static readonly DiagnosticDescriptor Passed = Rule(
        "RASM1008",
        Category,
        "Match passing one case through",
        "`{0}` returns `{1}` value unchanged, write `{2}`",
        "`Match` with one identity handler where a member taking the other handler alone returns `Match` result type.");

    public static readonly DiagnosticDescriptor Nullable = Rule(
        "RASM1009",
        Category,
        "Option converted to nullable by hand",
        "`Option` value `{0}` converted to nullable by hand, write `{1}`",
        "`Option` converted to nullable through `Match`, `Case` cast, or `IsSome` or `IsNone` conditional.");

    public static readonly DiagnosticDescriptor Focused = Rule(
        "RASM1010",
        Category,
        "Record property lens written by hand",
        "`{0}` restates {1} `{2}` of `{3}`, {4}",
        "`Lens<A, B>.New` whose getter reads a positional property path of a source record and whose setter rewrites the same path through `with`.");

    private static readonly SymbolDisplayFormat Shown = SymbolDisplayFormat.MinimallyQualifiedFormat.AddMiscellaneousOptions(SymbolDisplayMiscellaneousOptions.IncludeNullableReferenceTypeModifier);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Lift, TypeTest, RoundTrip, Spread, LiftedApply, Narrowed, Passed, Nullable, Focused];

    protected override void Start(CompilationStartAnalysisContext context) {
        LanguageExtSymbols symbols = new(context.Compilation);
        if (symbols is not { Fin: { } fin, Validation: { } validation, Error: { } error, ManyErrors: { } many, IO: { } io, Seq: { } seq, K: { } kind, Option: { } option }) return;
        Lifts(context, symbols.Factories, fin, validation);
        TypeTests(context, symbols.Leaves, error, many);
        RoundTrips(context, fin, validation, kind);
        Spreads(context, seq);
        LiftedApplies(context, io, fin);
        Narrowings(context, kind);
        Matches(context, symbols, fin, option);
        if (symbols.Lens is { } lens) Lenses(context, lens);
    }

    // --- [LIFTS]
    public static ExpressionSyntax? Unlifted(SyntaxNode lift) =>
        lift switch {
            InvocationExpressionSyntax { ArgumentList.Arguments: { Count: 1 } arguments } => arguments[0].Expression,
            BaseObjectCreationExpressionSyntax { ArgumentList.Arguments: { Count: 1 } arguments, Initializer: null } => arguments[0].Expression,
            CastExpressionSyntax cast => cast.Expression,
            _ => null,
        };

    private static void Lifts(CompilationStartAnalysisContext context, ImmutableArray<ISymbol> factories, INamedTypeSymbol fin, INamedTypeSymbol validation) {
        ImmutableArray<INamedTypeSymbol> cases = [
            .. fin.GetTypeMembers().Where(nested => Derives(nested, fin)),
            .. validation.GetTypeMembers().Where(nested => Derives(nested, validation)),
        ];
        (IOperation Lift, INamedTypeSymbol Target, IMethodSymbol Conversion)? Candidate(IOperation operation) {
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
                        block.ReportDiagnostic(Diagnostic.Create(Lift, lift.Syntax.GetLocation(), lift.Syntax, Unlifted(lift.Syntax), target.ToDisplayString(Shown)));
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
    private static void TypeTests(CompilationStartAnalysisContext context, ImmutableArray<ISymbol> leaves, INamedTypeSymbol error, INamedTypeSymbol many) {
        ISymbol? runtime = context.Compilation.GetSpecialType(SpecialType.System_Object).GetMembers(nameof(GetType)).FirstOrDefault();
        bool Typed(IOperation value) => Same(value.Type, error);
        bool Case(ITypeSymbol? type) => !Same(type, error) && !Derives(type, many) && Derives(type, error);
        bool Narrows(IOperation pattern, Func<ITypeSymbol?, bool> wanted) =>
            pattern.DescendantsAndSelf().OfType<IPatternOperation>().Any(nested => Same(nested.InputType, error) && wanted(nested.NarrowedType));
        ITypeSymbol? Narrowed(IOperation pattern) =>
            pattern.DescendantsAndSelf().OfType<IPatternOperation>().Where(nested => Same(nested.InputType, error)).Select(static nested => nested.NarrowedType).FirstOrDefault(Case);
        bool Visits(ISymbol member) => Overridden(member).Any(held => Among(held, leaves));
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
            context,
            [OperationKind.IsType, OperationKind.IsPattern, OperationKind.Conversion, OperationKind.Invocation, OperationKind.SwitchExpression, OperationKind.Switch],
            (operation, _) => Tests(operation).Where(test => !Leaf(test.Tested)).Select(static test => Diagnostic.Create(
                TypeTest,
                test.Site.Syntax.GetLocation(),
                test.Type is null ? $"`{test.Site.Syntax}`" : $"Narrowing `{test.Tested.Syntax}` to `{test.Type.ToDisplayString(Shown)}`",
                test.Tested.Syntax,
                test.Type?.ToDisplayString(Shown) ?? "<case>")));
    }

    // --- [ROUND_TRIPS]
    private static bool Parameterless(IInvocationOperation call) => call.Arguments.Length == (call.Instance is null ? 1 : 0);

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
        Report<IInvocationOperation>(context, [OperationKind.Invocation], (exit, _) =>
            Source(exit, validation, fin) is { } inner && Hops(inner) is { IsEmpty: false } hops && (Source(inner, kind, validation) ?? inner) is IInvocationOperation named
                ? [Diagnostic.Create(
                    RoundTrip,
                    Named(exit),
                    [.. hops.Select(static hop => hop.Syntax.GetLocation()), exit.Syntax.GetLocation()],
                    named.TargetMethod.Name,
                    ((IInvocationOperation)hops[0]).TargetMethod.Name,
                    exit.TargetMethod.Name)]
                : []);
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
        Report<ICollectionExpressionOperation>(context, [OperationKind.CollectionExpression], (collection, _) =>
            Same(collection.Type, seq) && collection.Elements is { Length: 2 } elements && elements[0] is ISpreadOperation { Operand: var spread } && elements[1] is not ISpreadOperation
            && SymbolEqualityComparer.Default.Equals(spread.Type, collection.Type) && Held(spread) is { } held && Stepped(collection, held)
            && (held is IParameterSymbol || (Consumer(collection) is ISimpleAssignmentOperation { Target: ILocalReferenceOperation target } && Same(target.Local, held)))
                ? [Diagnostic.Create(Spread, collection.Syntax.GetLocation(), collection.Syntax, held.Name, elements[1].Syntax)]
                : []);
    }

    // --- [LIFTED_APPLIES]
    private static void LiftedApplies(CompilationStartAnalysisContext context, INamedTypeSymbol io, INamedTypeSymbol fin) {
        bool Raised(IOperation operand) =>
            Stripped(operand) is IInvocationOperation { TargetMethod: { Parameters: { Length: 1 } parameters } lift } && Same(lift.ContainingType, io) && parameters[0].Type is var raised
            && (Same(raised, fin) || (raised is INamedTypeSymbol { DelegateInvokeMethod: { Parameters.IsEmpty: true, ReturnType: var returned } } && Same(returned, fin)));
        Report<IInvocationOperation>(context, [OperationKind.Invocation], (call, _) =>
            Receiver(call) is { } receiver && Stripped(receiver) is ITupleOperation { Elements.Length: > 1 } tuple && tuple.Elements.All(Raised)
                ? [Diagnostic.Create(LiftedApply, Named(call), call.TargetMethod.Name, tuple.Elements.Length)]
                : []);
    }

    // --- [NARROWINGS]
    private static void Narrowings(CompilationStartAnalysisContext context, INamedTypeSymbol kind) =>
        Report<IInvocationOperation>(context, [OperationKind.Invocation], (call, _) =>
            Parameterless(call) && Receiver(call) is IConversionOperation { IsImplicit: true, Type: var trait, Operand.Type: { } held }
            && Same(trait, kind) && SymbolEqualityComparer.Default.Equals(held, call.Type)
                ? [Diagnostic.Create(Narrowed, Named(call), call.TargetMethod.Name, held.ToDisplayString(Shown))]
                : []);

    // --- [MATCHES]
    private static void Matches(CompilationStartAnalysisContext context, LanguageExtSymbols symbols, INamedTypeSymbol fin, INamedTypeSymbol option) {
        (ISymbol? identity, ISymbol? matched, ISymbol? some, ISymbol? none) = (symbols.Identity, symbols.Case, symbols.IsSome, symbols.IsNone);
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
        static object[] Read(IOperation holder, ITypeSymbol element) => [holder.Syntax, $"{holder.Syntax}.{(element.IsReferenceType ? "ValueUnsafe" : "ToNullable")}()"];
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
        (DiagnosticDescriptor Rule, object[] Arguments)? Handled(IInvocationOperation call) {
            if (Receiver(call) is not { Type: INamedTypeSymbol owner } receiver || !SymbolEqualityComparer.Default.Equals(owner.ContainingAssembly, fin.ContainingAssembly)) return null;
            ImmutableArray<IArgumentOperation> handled = [.. call.Arguments.Where(argument => argument.Value != receiver)];
            bool optional = Same(owner, option);
            ITypeSymbol element = owner.TypeArguments.FirstOrDefault() ?? owner;
            object[] unwrapped = Read(receiver, element);
            if (optional && handled.Length is 1 && Empty(handled[0].Value) && SymbolEqualityComparer.Default.Equals(call.Type?.WithNullableAnnotation(NullableAnnotation.NotAnnotated), element) && Nulled(element, call.Type))
                return (Nullable, unwrapped);
            if (handled.Length is not 2 || !handled.All(static argument => argument.Parameter?.Type.TypeKind is TypeKind.Delegate)) return null;
            int kept = Identity(handled[0].Value) is not null ? 0 : Identity(handled[1].Value) is not null ? 1 : -1;
            IArgumentOperation other = handled[kept is 0 ? 1 : 0];
            return (kept, optional && Empty(other.Value) && Nulled(element, call.Type)) switch {
                ( >= 0, true) => (Nullable, unwrapped),
                (-1, _) when optional && handled.FirstOrDefault(static argument => !Empty(argument.Value)) is { } mapped && handled.Any(static argument => Empty(argument.Value))
                    && call.Type is { IsReferenceType: true } or INamedTypeSymbol { OriginalDefinition.SpecialType: SpecialType.System_Nullable_T } =>
                    (Nullable, [receiver.Syntax, $"{receiver.Syntax}.Map<{call.Type.ToDisplayString(Shown)}>({mapped.Value.Syntax}).ValueUnsafe()"]),
                ( >= 0, false) when SymbolEqualityComparer.Default.Equals(Identity(handled[kept].Value), call.Type)
                    && owner.GetMembers().OfType<IMethodSymbol>().FirstOrDefault(member => member is { IsStatic: false, Parameters.Length: 1 } && !Same(member, call.TargetMethod)
                        && SymbolEqualityComparer.IncludeNullability.Equals(member.Parameters[0].Type, other.Parameter!.Type)
                        && SymbolEqualityComparer.IncludeNullability.Equals(member.ReturnType, call.Type)) is { } named =>
                    (Passed, [call.TargetMethod.Name, handled[kept].Parameter!.Name, $"{receiver.Syntax}.{named.Name}({other.Value.Syntax})"]),
                _ => null,
            };
        }
        Report<IOperation>(context, [OperationKind.Invocation, OperationKind.Conversion, OperationKind.Conditional], (operation, _) =>
            operation switch {
                IInvocationOperation call when Handled(call) is { } handled => [Diagnostic.Create(handled.Rule, call.Syntax.GetLocation(), handled.Arguments)],
                IConversionOperation { IsImplicit: false, Type: { } target } cast when Stripped(cast.Operand) is IPropertyReferenceOperation { Instance: { Type: INamedTypeSymbol owner } holder } read
                    && Same(read.Property, matched) && SymbolEqualityComparer.Default.Equals(target.WithNullableAnnotation(NullableAnnotation.NotAnnotated), owner.TypeArguments[0]) =>
                    [Diagnostic.Create(Nullable, cast.Syntax.GetLocation(), Read(holder, owner.TypeArguments[0]))],
                IConditionalOperation { Condition: IPropertyReferenceOperation { Instance: { Type: INamedTypeSymbol owner } tested } test, WhenTrue: var present, WhenFalse: { } absent } conditional
                    when (Same(test.Property, some) && Unwraps(present, tested) && Empty(absent)) || (Same(test.Property, none) && Unwraps(absent, tested) && Empty(present)) =>
                    [Diagnostic.Create(Nullable, conditional.Syntax.GetLocation(), Read(tested, owner.TypeArguments[0]))],
                _ => [],
            });
    }

    // --- [LENSES]
    private static void Lenses(CompilationStartAnalysisContext context, INamedTypeSymbol lens) {
        static IAnonymousFunctionOperation? Function(IOperation value) => Stripped(value) is IDelegateCreationOperation { Target: IAnonymousFunctionOperation function } ? function : null;
        static IOperation? Returned(IAnonymousFunctionOperation function) =>
            function.Body.Operations is { Length: 1 } steps && steps[0] is IReturnOperation { ReturnedValue: { } returned } ? Stripped(returned) : null;
        static IParameterSymbol? Parameter(IAnonymousFunctionOperation? function) => function?.Symbol.Parameters is { Length: 1 } parameters ? parameters[0] : null;
        static bool Equal(ImmutableArray<IPropertySymbol> left, ImmutableArray<IPropertySymbol> right) => left.Length == right.Length && left.Zip(right, Same).All(static same => same);
        static ImmutableArray<IPropertySymbol>? Read(IOperation value, IParameterSymbol root) =>
            Stripped(value) switch {
                IParameterReferenceOperation reference when Same(reference.Parameter, root) => ImmutableArray<IPropertySymbol>.Empty,
                IPropertyReferenceOperation { Instance: { } instance } reference => Read(instance, root) is { } head ? head.Add(reference.Property) : null,
                _ => null,
            };
        static ImmutableArray<IPropertySymbol>? Written(IOperation value, IParameterSymbol root, IParameterSymbol held, ImmutableArray<IPropertySymbol> prefix) =>
            Stripped(value) switch {
                IParameterReferenceOperation reference when Same(reference.Parameter, held) => prefix,
                IWithOperation { Operand: var operand, Initializer.Initializers: { Length: 1 } sets }
                    when sets[0] is ISimpleAssignmentOperation { Target: IPropertyReferenceOperation target, Value: var next } && Read(operand, root) is { } read && Equal(read, prefix) =>
                    Written(next, root, held, prefix.Add(target.Property)),
                _ => null,
            };
        Report<IInvocationOperation>(context, [OperationKind.Invocation], (call, analysis) => {
            bool Positional(IPropertySymbol property) =>
                property.ContainingType is { IsRecord: true } owner && Sourced(owner) && property.DeclaringSyntaxReferences.Any(reference => reference.GetSyntax(analysis.CancellationToken) is ParameterSyntax);
            if (call.TargetMethod is not { Name: "New", IsStatic: true, ContainingType: { } owner } || !Same(owner, lens) || call.Arguments.Length is not 2
                || Function(call.Arguments[0].Value) is not { } getter || Parameter(getter) is not { } root || Returned(getter) is not { } got || Read(got, root) is not { IsEmpty: false } path
                || Function(call.Arguments[1].Value) is not { } setter || Parameter(setter) is not { } held || Returned(setter) is not { } inner || Function(inner) is not { } writer
                || Parameter(writer) is not { } target || Returned(writer) is not { } written || Written(written, target, held, []) is not { } wrote || !Equal(wrote, path) || !path.All(Positional))
                return [];
            string joined = string.Join(".", path.Select(static property => property.Name));
            return [
                Diagnostic.Create(
                    Focused,
                    call.Syntax.GetLocation(),
                    $"{owner.ToDisplayString(Shown)}.New",
                    path.Length is 1 ? "property" : "path",
                    joined,
                    root.Type.ToDisplayString(Shown),
                    path.Length is 1 ? "annotate the parameter with `[Field]` under `[StateRecord]` and read its generated lens" : "compose the generated leaf lenses through `lens(…)`"),
            ];
        });
    }
}

using System.Collections.Concurrent;
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
        "Explicit result lift, write the value",
        "`{0}` restates implicit conversion of `{1}` to `{2}`, write `{1}`",
        "Implicit conversions replace `Fin` or `Validation` factory calls, case constructions, and arm casts when overloads and arm types hold.",
        WellKnownDiagnosticTags.CompilationEnd);

    public static readonly DiagnosticDescriptor TypeTest = Rule(
        "RASM1003",
        Category,
        "Type test on `Error`, call `IsType` or `Filter`",
        "{0} misses errors inside `ManyErrors`, call `{1}.IsType<{2}>()` or bind fields through `{1}.Filter<{2}>().Head is {2} <name>`",
        "Classify `Error` values able to hold `ManyErrors` with `IsType` or `Filter` over casts, type tests, switch arms, or `GetType()`.");

    public static readonly DiagnosticDescriptor RoundTrip = Rule(
        "RASM1004",
        Category,
        "`Fin` round trip through `Validation`, delete both conversions",
        "`{0}` round-trips `Fin` through `Validation`, delete each `{1}()` and the closing `{2}()`",
        "Compose `Fin` operands as `Fin` over a parameterless `Validation` round trip, direct or through `Apply` or `Traverse`.");

    public static readonly DiagnosticDescriptor Spread = Rule(
        "RASM1005",
        Category,
        "`Seq` accumulator copied per step, call `Add`",
        "`{0}` copies `{1}` on every step, write `{1}.Add({2})`",
        "`Seq` fold accumulators and loop locals append through `Add` in place of `[.. <seq>, <item>]`.");

    public static readonly DiagnosticDescriptor Narrowed = Rule(
        "RASM1007",
        Category,
        "Redundant `As`, delete the call",
        "`{0}()` narrows `{1}` to itself, delete the call",
        "Delete parameterless narrowing calls on a `K<F, A>` conversion returning the converted value's type.");

    public static readonly DiagnosticDescriptor Passed = Rule(
        "RASM1008",
        Category,
        "`Match` passing one case through, call the one-handler member",
        "`{0}` returns `{1}` value unchanged, write `{2}`",
        "Call a member taking the non-identity handler alone with `Match` result type over `Match` with an identity handler.");

    public static readonly DiagnosticDescriptor Nullable = Rule(
        "RASM1009",
        Category,
        "`Option` converted to nullable by hand, call `ValueUnsafe` or `ToNullable`",
        "`Option` value `{0}` converted to nullable by hand, write `{1}`",
        "`Option` to nullable conversions call `ValueUnsafe` or `ToNullable` over `Match`, `Case` casts, or `IsSome` and `IsNone` conditionals.");

    public static readonly DiagnosticDescriptor Focused = Rule(
        "RASM1010",
        Category,
        "Record property lens written by hand, write `with` at the edit",
        "`{0}` restates {1} `{2}` of `{3}`, write `{4}` at the edit",
        "`Lens<A, B>.New` getters and `with` setters over one positional record property path become `with` at the edit.");

    public static readonly DiagnosticDescriptor Continued = Rule(
        "RASM1011",
        Category,
        "`IO.lift` thunk or `IO.pure` value returned from a continuation, write the value",
        "`{0}` continuation returns `{1}`, write `{2}`",
        "`IO` `Bind` and `SelectMany` continuations return no `IO.lift` thunk or `IO.pure` value as their whole result, a body with no call or side effect becomes `Map` under `Bind` and `let` in a later `from` clause, a `Fin` or `Either` thunk lifts its value.");

    public static readonly DiagnosticDescriptor Flattened = Rule(
        "RASM1012",
        Category,
        "`Map` then `Flatten`, call `Bind`",
        "`{0}` spells `Bind` as `Map` then `Flatten`, call `Bind` with a lambda return type over untyped parameters (`Fin<T> (x) => …`)",
        "LanguageExt `Map` over a one-parameter function followed by `Flatten` restates `Bind`, a lambda return type over untyped parameters binds the continuation once against the `Bind` overloads.");

    private static readonly SymbolDisplayFormat Shown = SymbolDisplayFormat.MinimallyQualifiedFormat.AddMiscellaneousOptions(SymbolDisplayMiscellaneousOptions.IncludeNullableReferenceTypeModifier);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Lift, TypeTest, RoundTrip, Spread, Narrowed, Passed, Nullable, Focused, Continued, Flattened];

    protected override void Start(CompilationStartAnalysisContext context) {
        LanguageExtSymbols symbols = new(context.Compilation);
        if (symbols is not { Fin: { } fin, Validation: { } validation, Error: { } error, ManyErrors: { } many, Seq: { } seq, K: { } kind, Option: { } option }) return;
        ImmutableArray<IMethodSymbol> narrowings = [.. kind.ContainingNamespace.ContainingNamespace.GetTypeMembers().SelectMany(static type => Members(type, "As"))];
        Lifts(context, symbols.Factories, fin, validation);
        TypeTests(context, symbols.Leaves, error, many);
        RoundTrips(context, symbols.Applies, fin, validation, kind);
        Spreads(context, seq);
        Narrowings(context, narrowings, kind);
        Matches(context, symbols, fin, option);
        Lenses(context, symbols.Lenses);
        if (symbols is { IO: { } io, IO1: { } effect }) Continuations(context, io, effect);
        Flattens(context, fin.ContainingAssembly);
    }

    // --- [FLATTENS]

    private static void Flattens(CompilationStartAnalysisContext context, IAssemblySymbol owner) {
        bool Owned(IInvocationOperation call, string name) => string.Equals(call.TargetMethod.Name, name, StringComparison.Ordinal) && Same(call.TargetMethod.ContainingAssembly, owner);
        Report<IInvocationOperation>(context, [OperationKind.Invocation], (flatten, _) =>
            Owned(flatten, "Flatten") && Parameterless(flatten) && Receiver(flatten) is { } receiver && Stripped(receiver) is IInvocationOperation map && Owned(map, "Map")
            && map.Arguments.Where(argument => argument.Value != Receiver(map)).ToImmutableArray() is { Length: 1 } stepped
            && stepped[0].Parameter?.Type is INamedTypeSymbol { DelegateInvokeMethod.Parameters.Length: 1 }
                ? [Diagnostic.Create(Flattened, flatten.Syntax.GetLocation(), flatten.Syntax)]
                : []);
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
        static bool Inferred(IOperation lift) {
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
        ConcurrentDictionary<SyntaxNode, ConcurrentBag<(IOperation Lift, INamedTypeSymbol Target, IMethodSymbol Conversion)>> candidates = new();
        context.RegisterOperationAction(analysis => {
            if (!analysis.IsGeneratedCode && Candidate(analysis.Operation) is { } candidate && !Inferred(candidate.Lift) && Scope(candidate.Lift.Syntax) is { } root)
                candidates.GetOrAdd(root, static _ => []).Add(candidate);
        }, OperationKind.Invocation, OperationKind.ObjectCreation, OperationKind.Conversion);
        context.RegisterCompilationEndAction(end => {
            foreach (KeyValuePair<SyntaxNode, ConcurrentBag<(IOperation Lift, INamedTypeSymbol Target, IMethodSymbol Conversion)>> scope in candidates) {
                end.CancellationToken.ThrowIfCancellationRequested();
                SyntaxNode root = scope.Key;
                ImmutableArray<(IOperation Lift, INamedTypeSymbol Target, IMethodSymbol Conversion)> lifts = [.. scope.Value.OrderByDescending(static lift => lift.Lift.Syntax.SpanStart).ThenBy(static lift => lift.Lift.Syntax.Span.Length)];
                ImmutableArray<(SyntaxNode Site, ISymbol? Symbol)> calls = Calls(lifts[0].Lift.SemanticModel!.GetOperation(root, end.CancellationToken));
                ImmutableArray<(IOperation Lift, INamedTypeSymbol Target, IMethodSymbol Conversion)> removed = Holds(root, calls, lifts, end.CancellationToken) ? lifts : lifts.Length is 1 ? [] : lifts.Aggregate(
                    ImmutableArray<(IOperation Lift, INamedTypeSymbol Target, IMethodSymbol Conversion)>.Empty,
                    (held, next) => held.Add(next) switch { var proposed => Holds(root, calls, proposed, end.CancellationToken) ? proposed : held });
                foreach ((IOperation lift, INamedTypeSymbol target, _) in removed)
                    end.ReportDiagnostic(Diagnostic.Create(Lift, lift.Syntax.GetLocation(), lift.Syntax, Unlifted(lift.Syntax), target.ToDisplayString(Shown)));
            }
        });
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

    private static bool Holds(SyntaxNode scope, ImmutableArray<(SyntaxNode Site, ISymbol? Symbol)> calls, ImmutableArray<(IOperation Lift, INamedTypeSymbol Target, IMethodSymbol Conversion)> removed, CancellationToken token) {
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

    private static bool Parameterless(IInvocationOperation call) => (call.Instance is not null || call.TargetMethod.IsExtensionMethod) && call.Arguments.Length == (call.Instance is null ? 1 : 0);

    private static void RoundTrips(CompilationStartAnalysisContext context, ImmutableArray<IMethodSymbol> applies, INamedTypeSymbol fin, INamedTypeSymbol validation, INamedTypeSymbol kind) {
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
        Report<IInvocationOperation>(context, [OperationKind.Invocation], (exit, analysis) =>
            Source(exit, validation, fin) is { } inner && Hops(inner, analysis.CancellationToken) is { IsEmpty: false } hops && (Source(inner, kind, validation) ?? inner) is IInvocationOperation named
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
        static bool Stepped(IOperation operation, ISymbol held) =>
            Ancestor<IOperation>(operation, static step => step is IAnonymousFunctionOperation { Symbol.Parameters.Length: > 1 } or ILoopOperation) switch {
                IAnonymousFunctionOperation { Symbol: { Parameters: var parameters } folder } => Same(parameters[0], held) && SymbolEqualityComparer.Default.Equals(parameters[0].Type, folder.ReturnType),
                ILoopOperation => held is ILocalSymbol,
                _ => false,
            };
        Report<ICollectionExpressionOperation>(context, [OperationKind.CollectionExpression], (collection, _) =>
            Same(collection.Type, seq) && collection.Elements is { Length: 2 } elements && elements[0] is ISpreadOperation { Operand: var spread } && elements[1] is not ISpreadOperation
            && SymbolEqualityComparer.Default.Equals(spread.Type, collection.Type) && Variable(Stripped(spread)) is { } held && Stepped(collection, held)
            && (held is IParameterSymbol || (Consumer(collection) is ISimpleAssignmentOperation { Target: ILocalReferenceOperation target } && Same(target.Local, held)))
                ? [Diagnostic.Create(Spread, collection.Syntax.GetLocation(), collection.Syntax, held.Name, elements[1].Syntax)]
                : []);
    }

    // --- [NARROWINGS]

    private static void Narrowings(CompilationStartAnalysisContext context, ImmutableArray<IMethodSymbol> narrowings, INamedTypeSymbol kind) =>
        Report<IInvocationOperation>(context, [OperationKind.Invocation], (call, _) =>
            Among(call.TargetMethod, narrowings) && Parameterless(call) && Receiver(call) is IConversionOperation { IsImplicit: true, Type: var trait, Operand.Type: { } held }
            && Same(trait, kind) && SymbolEqualityComparer.Default.Equals(held, call.Type)
                ? [Diagnostic.Create(Narrowed, Named(call), call.TargetMethod.Name, held.ToDisplayString(Shown))]
                : []);

    // --- [MATCHES]

    private static void Matches(CompilationStartAnalysisContext context, LanguageExtSymbols symbols, INamedTypeSymbol fin, INamedTypeSymbol option) {
        (ISymbol? identity, ISymbol? matched, ISymbol? some, ISymbol? none) = (symbols.Identity, symbols.Case, symbols.IsSome, symbols.IsNone);
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
        object[] Read(IOperation holder, ITypeSymbol element) => [holder.Syntax, $"{holder.Syntax}.{(element.IsReferenceType ? symbols.Unsafe : symbols.Nullable)!.Name}()"];
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
            if (handled.Length is not 2 || !Among(call.TargetMethod, symbols.Matches) || !handled.All(static argument => argument.Parameter?.Type.TypeKind is TypeKind.Delegate)) return null;
            IArgumentOperation? kept = handled.FirstOrDefault(argument => Identity(argument.Value) is not null);
            IArgumentOperation other = handled.First(argument => argument != kept);
            return (kept, optional && Empty(other.Value) && Nulled(element, call.Type)) switch {
                (not null, true) => (Nullable, unwrapped),
                (null, _) when optional && handled.FirstOrDefault(static argument => !Empty(argument.Value)) is { } mapped && handled.Any(static argument => Empty(argument.Value))
                    && call.Type is { IsReferenceType: true } or INamedTypeSymbol { OriginalDefinition.SpecialType: SpecialType.System_Nullable_T } =>
                    (Nullable, [receiver.Syntax, $"{receiver.Syntax}.{symbols.Maps[0].Name}<{call.Type.ToDisplayString(Shown)}>({mapped.Value.Syntax}).{symbols.Unsafe!.Name}()"]),
                ( { Value: var value, Parameter: var parameter }, false) when SymbolEqualityComparer.Default.Equals(Identity(value), call.Type)
                    && owner.GetMembers().OfType<IMethodSymbol>().FirstOrDefault(member => member is { IsStatic: false, Parameters.Length: 1 } && !Same(member, call.TargetMethod)
                        && SymbolEqualityComparer.IncludeNullability.Equals(member.Parameters[0].Type, other.Parameter!.Type)
                        && SymbolEqualityComparer.IncludeNullability.Equals(member.ReturnType, call.Type)) is { } named =>
                    (Passed, [call.TargetMethod.Name, parameter!.Name, $"{receiver.Syntax}.{named.Name}({other.Value.Syntax})"]),
                _ => null,
            };
        }
        Report<IOperation>(context, [OperationKind.Invocation, OperationKind.Conversion, OperationKind.Conditional], (operation, analysis) =>
            operation switch {
                IInvocationOperation call when Handled(call) is { } handled => [Diagnostic.Create(handled.Rule, call.Syntax.GetLocation(), handled.Arguments)],
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
    }

    // --- [LENSES]

    private static void Lenses(CompilationStartAnalysisContext context, ImmutableArray<IMethodSymbol> lenses) {
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
        Report<IInvocationOperation>(context, [OperationKind.Invocation], (call, analysis) => {
            bool Positional(IPropertySymbol property) =>
                property.ContainingType is { IsRecord: true } owner && Sourced(owner) && property.DeclaringSyntaxReferences.Any(reference => reference.GetSyntax(analysis.CancellationToken) is ParameterSyntax);
            if (!Among(call.TargetMethod, lenses)) return [];
            if (Lambda(call.Arguments.Single(static argument => argument.Parameter!.Ordinal is 0).Value) is not { Symbol.Parameters.Length: 1 } getter || Sole(getter.Body) is not { } got || Read(got, getter.Symbol.Parameters[0], analysis.CancellationToken) is not { IsEmpty: false } path
                || Lambda(call.Arguments.Single(static argument => argument.Parameter!.Ordinal is 1).Value) is not { Symbol.Parameters.Length: 1 } setter || Sole(setter.Body) is not { } inner || Lambda(inner) is not { Symbol.Parameters.Length: 1 } writer
                || Sole(writer.Body) is not { } written || Written(written, writer.Symbol.Parameters[0], setter.Symbol.Parameters[0], analysis.CancellationToken) is not { } wrote || !Enumerable.SequenceEqual(wrote, path, SymbolEqualityComparer.Default) || !path.All(Positional))
                return [];
            string joined = string.Join(".", path.Select(static property => property.Name));
            return [
                Diagnostic.Create(
                    Focused,
                    call.Syntax.GetLocation(),
                    $"{call.TargetMethod.ContainingType.ToDisplayString(Shown)}.{call.TargetMethod.Name}",
                    path.Length is 1 ? "property" : "path",
                    joined,
                    getter.Symbol.Parameters[0].Type.ToDisplayString(Shown),
                    written.Syntax),
            ];
        });
    }

    // --- [CONTINUATIONS]

    private static void Continuations(CompilationStartAnalysisContext context, INamedTypeSymbol io, INamedTypeSymbol effect) {
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
        Report<IInvocationOperation>(context, [OperationKind.Invocation], (step, analysis) => {
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
            return written is { } found ? [Diagnostic.Create(Continued, found.Location, found.Properties, bind.TargetMethod.Name, step.Syntax, found.Text)] : [];
        });
    }
}

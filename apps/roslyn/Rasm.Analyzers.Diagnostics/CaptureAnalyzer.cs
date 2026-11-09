using System.Collections.Concurrent;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.FlowAnalysis;
using Microsoft.CodeAnalysis.Operations;
using Microsoft.CodeAnalysis.Text;

namespace Rasm.Analyzers;

// --- [COMPOSITION] ---------------------------------------------------------------------

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class CaptureAnalyzer : RasmAnalyzer {

    // --- [RULES]

    private const string Category = "CSharp";

    private static readonly DiagnosticDescriptor Captured = Rule(
        "RASM3104",
        Category,
        "Callback value passed out through a captured write, return it in the result",
        "`{0}` passes a value out of a callback through a write outside its result",
        "Return callback values in the call's result instead of assigning captured locals or mutating captured collections and reading them afterward.");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Captured];

    // --- [SYMBOLS]

    private static readonly ImmutableArray<string> Atoms = ["Atom", "AtomHashMap", "AtomSeq", "AtomQue", "Ref", "VersionHashMap"];

    private static readonly ImmutableArray<Type> Collections = [
        typeof(List<>), typeof(Dictionary<,>), typeof(HashSet<>), typeof(SortedSet<>), typeof(SortedDictionary<,>),
        typeof(SortedList<,>), typeof(Queue<>), typeof(Stack<>), typeof(LinkedList<>),
        typeof(ICollection<>), typeof(IList<>), typeof(IDictionary<,>), typeof(ISet<>), typeof(System.Collections.IList), typeof(System.Collections.IDictionary),
        typeof(ImmutableArray<>.Builder), typeof(ImmutableList<>.Builder), typeof(ImmutableHashSet<>.Builder), typeof(ImmutableSortedSet<>.Builder),
        typeof(ImmutableDictionary<,>.Builder), typeof(ImmutableSortedDictionary<,>.Builder),
    ];

    // --- [ANALYSIS]

    protected override void Start(CompilationStartAnalysisContext context) {
        Compilation compilation = context.Compilation;
        ControlFlow flow = ControlFlow.For(compilation);
        INamespaceSymbol? languageExt = compilation.GetTypeByMetadataName("LanguageExt.Atom`1")?.ContainingNamespace;
        ImmutableArray<INamedTypeSymbol> atoms = [.. Atoms.SelectMany(name => languageExt?.GetTypeMembers(name) ?? [])];
        ImmutableArray<INamedTypeSymbol> collections = [.. Collections.Select(type => compilation.GetTypeByMetadataName(type.FullName!)).OfType<INamedTypeSymbol>()];
        INamedTypeSymbol? prelude = compilation.GetTypeByMetadataName("LanguageExt.Prelude");
        ImmutableHashSet<ISymbol> mutations = ImmutableHashSet.CreateRange<ISymbol>(SymbolEqualityComparer.Default, [
            .. atoms.Concat(collections).SelectMany(static owner => Members(owner,
                "Swap", "SwapIO", "SwapMaybe", "SwapMaybeIO", "SwapKey", "Add", "AddOrUpdate", "AddOrUpdateRange", "AddRange", "Clear",
                "FilterInPlace", "MapInPlace", "BindInPlace", "FindOrAdd", "FindOrMaybeAdd", "Insert", "InsertRange", "Push", "Pop",
                "Remove", "RemoveAll", "RemoveAt", "RemoveRange", "SetItem", "SetItems", "TryAdd", "TryAddRange", "TrySetItem", "TrySetItems",
                "Enqueue", "Dequeue", "DequeueUnsafe", "TryDequeue", "TryPop", "Update", "Commute")),
            .. Members(prelude, "swap", "commute"),
            .. collections.SelectMany(static owner => owner.GetMembers()).OfType<IPropertySymbol>().Where(static property => property is { IsIndexer: true, SetMethod: not null }),
        ]);
        ImmutableHashSet<IMethodSymbol> suspended = ImmutableHashSet.CreateRange<IMethodSymbol>(SymbolEqualityComparer.Default, [
            .. Members(compilation.GetTypeByMetadataName("LanguageExt.IO"), "lift", "liftAsync"),
            .. Members(prelude, "atomic", "atomicIO", "snapshot", "serial"),
        ]);
        static IMethodSymbol Definition(IMethodSymbol method) => (method.ReducedFrom ?? method).OriginalDefinition;
        ConcurrentDictionary<ISymbol, bool> mutating = new(SymbolEqualityComparer.Default);
        bool Mutates(ISymbol member) => mutating.GetOrAdd(member is IMethodSymbol method ? Definition(method) : member.OriginalDefinition, held => mutations.Contains(held)
            || held.ContainingType.AllInterfaces.SelectMany(static face => face.GetMembers())
                .Any(contract => mutations.Contains(contract.OriginalDefinition) && Same(held.ContainingType.FindImplementationForInterfaceMember(contract), held)));
        ConcurrentDictionary<INamedTypeSymbol, (ImmutableHashSet<IMethodSymbol> Calls, ImmutableHashSet<IMethodSymbol> Switches)> combinators = new(SymbolEqualityComparer.Default);
        bool Combinator(IInvocationOperation call) {
            (ImmutableHashSet<IMethodSymbol> calls, ImmutableHashSet<IMethodSymbol> switches) = combinators.GetOrAdd(call.TargetMethod.ContainingType.OriginalDefinition, static owner => (
                Members(owner, "Iter", "IfSome", "IfSucc", "IfFail", "IfNone", "Do", "ForEach", "Traverse", "TraverseM", "Map", "Select", "Bind", "SelectMany", "Filter", "Where", "Choose", "Fold", "Match").ToImmutableHashSet<IMethodSymbol>(SymbolEqualityComparer.Default),
                Members(owner, "Switch").ToImmutableHashSet<IMethodSymbol>(SymbolEqualityComparer.Default)));
            IMethodSymbol method = Definition(call.TargetMethod);
            return switches.Contains(method) || (calls.Contains(method) && !call.Arguments.Where(static argument => !argument.IsImplicit).Skip(2).Any());
        }
        context.RegisterOperationBlockAction(block => {
            if (block.IsGeneratedCode || block.OwningSymbol is IMethodSymbol { MethodKind: MethodKind.AnonymousFunction or MethodKind.LocalFunction }) return;
            CancellationToken token = block.CancellationToken;
            ImmutableArray<IOperation> nodes = [.. block.OperationBlocks.SelectMany(static root => root.DescendantsAndSelf())
                .Where(static operation => operation is IAnonymousFunctionOperation or ILocalFunctionOperation)];
            if (nodes.IsEmpty) return;
            Dictionary<ISymbol, IBlockOperation> functions = nodes.OfType<IAnonymousFunctionOperation>().Select(static function => (function.Symbol, function.Body))
                .Concat(nodes.OfType<ILocalFunctionOperation>().Where(static function => function.Body is not null).Select(static function => (function.Symbol, Body: function.Body!)))
                .ToDictionary<(IMethodSymbol Symbol, IBlockOperation Body), ISymbol, IBlockOperation>(static function => function.Symbol.OriginalDefinition, static function => function.Body, SymbolEqualityComparer.Default);
            SemanticModel model = block.OperationBlocks[0].SemanticModel!;
            HashSet<IRangeVariableSymbol> ranges = [with(SymbolEqualityComparer.Default)];
            Dictionary<IRangeVariableSymbol, IOperation> aliases = [with(SymbolEqualityComparer.Default)];
            List<(ISymbol Symbol, IOperation Value, ImmutableArray<IInvocationOperation> Calls)> bindings = [];
            List<(IOperation Value, ImmutableArray<IInvocationOperation> Calls)> reads = [];
            List<(IOperation Source, IOperation Value, ImmutableArray<IInvocationOperation> Calls)> writes = [];
            ISymbol? Symbol(IOperation operation) => operation switch {
                { Syntax: IdentifierNameSyntax name } when name.FirstAncestorOrSelf<QueryExpressionSyntax>() is not null && model.GetSymbolInfo(name, token).Symbol is IRangeVariableSymbol range => range,
                IFieldReferenceOperation { Field.IsReadOnly: true, Instance: null or IInstanceReferenceOperation } field => field.Field,
                _ => Variable(operation),
            };
            IOperation? Mutated(IOperation operation) => operation switch {
                IInvocationOperation call when Mutates(call.TargetMethod) => Receiver(call),
                ISimpleAssignmentOperation { Target: IArrayElementReferenceOperation element } => element.ArrayReference,
                ISimpleAssignmentOperation { Target: IPropertyReferenceOperation { Instance: { } receiver, Property: { IsIndexer: true } property } } when Mutates(property) => receiver,
                _ => null,
            };
            Stack<(IOperation Value, ImmutableArray<IInvocationOperation> Calls, ImmutableHashSet<IMethodSymbol> Seen)> pending = new(
                block.OperationBlocks.Select(static root => (root, ImmutableArray<IInvocationOperation>.Empty, ImmutableHashSet.Create<IMethodSymbol>(SymbolEqualityComparer.Default))));
            while (pending.Count is not 0) {
                token.ThrowIfCancellationRequested();
                (IOperation value, ImmutableArray<IInvocationOperation> calls, ImmutableHashSet<IMethodSymbol> seen) = pending.Pop();
                if (value is IAnonymousFunctionOperation or ILocalFunctionOperation) continue;
                if (value is IArgumentOperation { Parameter.Type: { } parameter, Parent: IInvocationOperation call } argument
                    && (parameter.TypeKind is TypeKind.Delegate || argument.Value.DescendantsAndSelf().Any(static held => held is IAnonymousFunctionOperation))
                    && (Combinator(call) || suspended.Contains(Definition(call.TargetMethod)))) {
                    ImmutableArray<IInvocationOperation> nested = Combinator(call) ? calls.Add(call) : calls;
                    foreach ((IMethodSymbol target, IBlockOperation body) in
                        from target in flow.Targets(ControlFlow.At(argument.Value), token).OfType<IMethodSymbol>()
                        let member = target.OriginalDefinition
                        where !seen.Contains(member)
                        let body = functions.TryGetValue(member, out IBlockOperation? found) ? found : null
                        where body is not null
                        select (member, body)) pending.Push((body, nested, seen.Add(target)));
                }
                if (value is IVariableDeclaratorOperation { Initializer.Value: { } initializer } variable) bindings.Add((variable.Symbol, initializer, calls));
                if (value is ISimpleAssignmentOperation { Target: ILocalReferenceOperation local } assigned) bindings.Add((local.Local, assigned.Value, calls));
                ISymbol? symbol = Symbol(value);
                if (symbol is not null) {
                    if (Written(value)) writes.Add((value.Parent!, value, calls));
                    else reads.Add((value, calls));
                }
                if (symbol is IRangeVariableSymbol range && ranges.Add(range) && range.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax(token) is { } declaration
                    && (declaration switch { FromClauseSyntax from => from.Expression, LetClauseSyntax let => let.Expression, _ => null }) is { } expression
                    && model.GetOperation(expression, token) is { } source) {
                    if (declaration is LetClauseSyntax && Symbol(Stripped(source)) is not null) aliases.Add(range, source);
                    QueryExpressionSyntax query = declaration.FirstAncestorOrSelf<QueryExpressionSyntax>()!;
                    bindings.Add((range, source, [.. calls.TakeWhile(held => !query.Span.Contains(held.Syntax.Span))]));
                }
                if (Mutated(value) is { } receiver) writes.Add((value, Stripped(receiver), calls));
                foreach (IOperation child in value.ChildOperations.Reverse()) pending.Push((child, calls, seen));
            }
            if (writes.TrueForAll(static write => write.Calls.IsEmpty)) return;
            Dictionary<(IOperation Value, ImmutableArray<IInvocationOperation> Calls), ImmutableHashSet<(SyntaxTree Tree, TextSpan Span)>> origins = [];
            ImmutableHashSet<(SyntaxTree Tree, TextSpan Span)> Origins(IOperation source, ImmutableArray<IInvocationOperation> calls) {
                if (origins.TryGetValue((source, calls), out ImmutableHashSet<(SyntaxTree Tree, TextSpan Span)>? cached)) return cached;
                ImmutableHashSet<(SyntaxTree Tree, TextSpan Span)>.Builder result = ImmutableHashSet.CreateBuilder<(SyntaxTree Tree, TextSpan Span)>();
                Stack<ControlFlow.Value> held = new([ControlFlow.At(source)]);
                HashSet<ControlFlow.Value> visited = [];
                while (held.Count is not 0) {
                    token.ThrowIfCancellationRequested();
                    ControlFlow.Value current = held.Pop();
                    if (!visited.Add(current)) continue;
                    IOperation value = Stripped(current.Operation);
                    IOperation? receiver = value switch {
                        IConversionOperation { OperatorMethod: null, Conversion: { IsIdentity: true } or { IsReference: true } } conversion => conversion.Operand,
                        IConditionalAccessInstanceOperation conditional => Ancestor<IConditionalAccessOperation>(conditional, access => access.Operation.Syntax == conditional.Syntax)?.Operation,
                        _ => null,
                    };
                    if (receiver is not null) {
                        held.Push(current.With(receiver));
                        continue;
                    }
                    ISymbol? symbol = Symbol(value);
                    if (symbol is IFieldSymbol field) {
                        result.UnionWith(field.DeclaringSyntaxReferences.Select(static declaration => (declaration.SyntaxTree, declaration.Span)));
                        continue;
                    }
                    if (symbol is IRangeVariableSymbol range) {
                        IOperation? projected = aliases.TryGetValue(range, out IOperation? aliased) ? aliased : null;
                        if (projected is not null) held.Push(ControlFlow.At(projected));
                        result.UnionWith((projected is null ? range.DeclaringSyntaxReferences : []).Select(static declaration => (declaration.SyntaxTree, declaration.Span)));
                        continue;
                    }
                    if (value is ILocalReferenceOperation or IParameterReferenceOperation or IFlowCaptureReferenceOperation) {
                        ImmutableArray<Optional<ControlFlow.Value>> definitions = flow.Values(current.With(value), token);
                        for (int index = calls.Length - 1; index >= 0 && symbol is not null && definitions.Any(static definition => !definition.HasValue); index--)
                            definitions = [.. definitions.Where(static definition => definition.HasValue), .. flow.Values(symbol, calls[index], token)];
                        foreach (Optional<ControlFlow.Value> definition in definitions.Where(static definition => definition.HasValue)) held.Push(definition.Value);
                        result.UnionWith((definitions.Any(static definition => !definition.HasValue) && symbol is not null ? symbol.DeclaringSyntaxReferences : [])
                            .Select(static declaration => (declaration.SyntaxTree, declaration.Span)));
                        continue;
                    }
                    _ = result.Add((value.Syntax.SyntaxTree, value.Syntax.Span));
                }
                return origins[(source, calls)] = result.ToImmutable();
            }
            static bool Captures(ImmutableArray<IInvocationOperation> binding, ImmutableArray<IInvocationOperation> callback) =>
                binding.Length < callback.Length && binding.SequenceEqual(callback.Take(binding.Length));
            static IOperation Position(IOperation value, ImmutableArray<IInvocationOperation> calls, ImmutableArray<IInvocationOperation> scope) => calls.Length > scope.Length ? calls[scope.Length] : value;
            HashSet<ISymbol> reported = [with(SymbolEqualityComparer.Default)];
            foreach ((ISymbol symbol, IOperation value, ImmutableArray<IInvocationOperation> scope) in bindings) {
                token.ThrowIfCancellationRequested();
                IOperation within = symbol is IRangeVariableSymbol ranged
                    ? model.GetOperation(ranged.DeclaringSyntaxReferences[0].GetSyntax(token).FirstAncestorOrSelf<QueryExpressionSyntax>()!, token)! : value;
                ImmutableHashSet<(SyntaxTree Tree, TextSpan Span)> values = symbol is IRangeVariableSymbol range && !aliases.ContainsKey(range)
                    ? [.. range.DeclaringSyntaxReferences.Select(static declaration => (declaration.SyntaxTree, declaration.Span))] : Origins(value, scope);
                ImmutableArray<(IOperation Source, IOperation Value, ImmutableArray<IInvocationOperation> Calls)> captured = [.. writes.Where(write => Captures(scope, write.Calls)
                    && flow.Precedes(value, Position(write.Source, write.Calls, scope), within, token)
                    && (Written(write.Value) ? Same(Symbol(write.Value), symbol) : values.Overlaps(Origins(write.Value, write.Calls))))];
                ImmutableHashSet<IInvocationOperation> writers = [.. captured.SelectMany(write => write.Calls.Skip(scope.Length))];
                bool escapes = captured.Any(static write => Written(write.Value)) || reads.Exists(read => !read.Calls.Any(writers.Contains)
                    && !captured.Any(write => write.Value == read.Value) && values.Overlaps(Origins(read.Value, read.Calls))
                    && captured.Any(write => flow.Precedes(Position(write.Source, write.Calls, scope), Position(read.Value, read.Calls, scope), within, token)));
                if (escapes && reported.Add(symbol)) block.ReportDiagnostic(Diagnostic.Create(Captured, symbol.Locations[0], symbol.Name));
            }
        });
    }
}

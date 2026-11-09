using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.FlowAnalysis;
using Microsoft.CodeAnalysis.Operations;
using Microsoft.CodeAnalysis.Text;

namespace Rasm.Analyzers;

// --- [SERVICES] ------------------------------------------------------------------------

internal sealed class ControlFlow {
    private static readonly ConditionalWeakTable<Compilation, ControlFlow> Compilations = new();
    private readonly ConcurrentDictionary<SyntaxNode, Optional<ControlFlowGraph>> graphs = [];
    private readonly ConcurrentDictionary<ControlFlowGraph, Scope> scopes = [];

    internal static ControlFlow For(Compilation compilation) => Compilations.GetValue(compilation, static _ => new());

    internal static Value At(IOperation operation) => new(operation, default);

    internal ImmutableArray<Value> Evaluations(IOperation operation, CancellationToken token) => Find(operation, token) is { } scope
        ? [.. scope.Positions[operation.Syntax.Span].SelectMany(position => scope.StatesByBlock[position.Block])
            .Distinct().Select(state => new Value(operation, (scope, state)))]
        : [At(operation)];

    internal IEnumerable<(Value Reference, Optional<Value> Definition)> Readings(IOperation read, CancellationToken token) =>
        Evaluations(read, token).SelectMany(reference => Values(reference, token).Select(definition => (reference, definition)));

    internal ImmutableArray<Optional<Value>> Values(Value read, CancellationToken token) {
        Scope? scope = read.Context is { } context ? context.Scope : Find(read.Operation, token);
        int? state = read.Context?.State;
        ISymbol? symbol = Variable(read.Operation);
        return scope is null ? [default] : scope.Values.GetOrAdd((read.Operation, state), held => Reached(scope, held.Operation, held.State,
            current => current is IFlowCaptureOperation capture && read.Operation is IFlowCaptureReferenceOperation reference && capture.Id.Equals(reference.Id)
                ? new Optional<IOperation>(capture.Value) : Definition(current, symbol), token));
    }

    internal ImmutableArray<Optional<Value>> Values(ISymbol symbol, IOperation at, CancellationToken token) => Find(at, token) is { } scope
        ? scope.Symbols.GetOrAdd(symbol, static _ => []).GetOrAdd(at, operation => Reached(scope, operation, default, current => Definition(current, symbol), token))
        : [default];

    internal bool Precedes(IOperation first, IOperation last, IOperation within, CancellationToken token) {
        Scope? scope = Find(within, token);
        if (scope is null) return false;
        IEnumerable<(int Block, int Before)> Positions(IOperation operation) {
            if (scope == Find(operation, token)) return scope.Positions[operation.Syntax.Span]
                .Where(point => scope.Events[point.Block][point.Before].Kind == operation.Kind);
            HashSet<IMethodSymbol> owners = [with(SymbolEqualityComparer.Default)];
            for (IOperation? held = operation; held is not null; held = held.Parent)
                if (held is IAnonymousFunctionOperation function) owners.Add(function.Symbol);
            return scope.Events.SelectMany(static (events, block) => events.Select((value, before) => (Value: value, Block: block, Before: before)))
                .Where(point => point.Value is IFlowAnonymousFunctionOperation function && owners.Contains(function.Symbol))
                .Select(static point => (point.Block, point.Before));
        }
        ImmutableArray<(int Block, int Before)> origins = [.. Positions(first)];
        return scope.Reaches(Positions(last).SelectMany(position => scope.StatesByBlock[position.Block].Select(state => (state, position.Before))),
            (state, before) => origins.Any(origin => origin.Block == scope.States[state].Block && origin.Before < before), token);
    }

    internal static IEnumerable<IOperation> Operations(IOperation root, CancellationToken token) {
        Stack<(IOperation Value, bool Expanded)> pending = new([(root, false)]);
        while (pending.Count > 0) {
            token.ThrowIfCancellationRequested();
            (IOperation value, bool expanded) = pending.Pop();
            if (expanded) {
                yield return value;
                continue;
            }
            pending.Push((value, true));
            if (value == root || value is not (IFlowAnonymousFunctionOperation or IAnonymousFunctionOperation or ILocalFunctionOperation))
                foreach (IOperation child in value.ChildOperations.Reverse()) pending.Push((child, false));
        }
    }

    internal ImmutableArray<ISymbol> Targets(Value value, CancellationToken token) {
        HashSet<ISymbol> result = [with(SymbolEqualityComparer.Default)];
        HashSet<Value> visited = [];
        Queue<Value> pending = new([value]);
        while (pending.Count > 0) {
            token.ThrowIfCancellationRequested();
            Value current = pending.Dequeue();
            if (!visited.Add(current)) continue;
            IMethodSymbol? target = current.Operation switch {
                IAnonymousFunctionOperation function => function.Symbol,
                IFlowAnonymousFunctionOperation function => function.Symbol,
                IMethodReferenceOperation method => method.Method,
                _ => null,
            };
            if (target is not null) result.Add(target);
            ImmutableArray<Optional<Value>> definitions = current.Operation is ILocalReferenceOperation or IParameterReferenceOperation or IFlowCaptureReferenceOperation
                ? Values(current, token) : [];
            if (current.Operation is IParameterReferenceOperation parameter && definitions.Any(static reached => !reached.HasValue)) result.Add(parameter.Parameter.OriginalDefinition);
            IEnumerable<Value> next = current.Operation switch {
                IConversionOperation { OperatorMethod: null } conversion => [current.With(conversion.Operand)],
                IDelegateCreationOperation creation => [current.With(creation.Target)],
                IParenthesizedOperation parenthesized => [current.With(parenthesized.Operand)],
                ISimpleAssignmentOperation assignment => [current.With(assignment.Value)],
                IConditionalOperation conditional => conditional.WhenFalse is { } otherwise
                    ? [current.With(conditional.WhenTrue), current.With(otherwise)] : [current.With(conditional.WhenTrue)],
                ICoalesceOperation coalesce => [current.With(coalesce.Value), current.With(coalesce.WhenNull)],
                ISwitchExpressionOperation expression => expression.Arms.Select(arm => current.With(arm.Value)),
                ILocalReferenceOperation or IParameterReferenceOperation or IFlowCaptureReferenceOperation =>
                    definitions.Where(static reached => reached.HasValue).Select(static reached => reached.Value),
                _ => [],
            };
            foreach (Value child in next) pending.Enqueue(child);
        }
        return [.. result];
    }

    internal bool Invoked(IParameterSymbol supplied, Func<IMethodSymbol, IEnumerable<IOperation>> bodies, Func<IParameterSymbol, bool> native, CancellationToken token) {
        IParameterSymbol source = supplied.OriginalDefinition;
        if (native(source)) return true;
        ImmutableHashSet<IParameterSymbol> empty = ImmutableHashSet.Create<IParameterSymbol>(SymbolEqualityComparer.Default);
        Dictionary<IParameterSymbol, ImmutableArray<ImmutableHashSet<IParameterSymbol>>> equations = [with(SymbolEqualityComparer.Default)];
        Queue<IParameterSymbol> parameters = new([source]);
        while (parameters.Count is > 0) {
            token.ThrowIfCancellationRequested();
            IParameterSymbol parameter = parameters.Dequeue();
            if (equations.ContainsKey(parameter)) continue;
            List<ImmutableHashSet<IParameterSymbol>> routes = [];
            Queue<(IOperation Body, ImmutableHashSet<IParameterSymbol> Required)> pending = new(
                bodies((IMethodSymbol)parameter.ContainingSymbol).Select(body => (body, empty)));
            Dictionary<IOperation, List<ImmutableHashSet<IParameterSymbol>>> seen = [];
            void Require(ImmutableHashSet<IParameterSymbol> required) {
                routes.Add(required);
                foreach (IParameterSymbol dependency in required) parameters.Enqueue(dependency);
            }
            if (native(parameter)) Require(empty);
            while (pending.Count is > 0) {
                token.ThrowIfCancellationRequested();
                (IOperation body, ImmutableHashSet<IParameterSymbol> required) = pending.Dequeue();
                if (!seen.TryGetValue(body, out List<ImmutableHashSet<IParameterSymbol>>? paths)) seen.Add(body, paths = []);
                if (paths.Exists(path => path.SetEquals(required))) continue;
                paths.Add(required);
                void Follow(IOperation value, ImmutableHashSet<IParameterSymbol> needs) {
                    foreach (ISymbol target in Targets(At(value), token))
                        if (SymbolEqualityComparer.Default.Equals(target, parameter)) Require(needs);
                        else if (target is IMethodSymbol { MethodKind: MethodKind.AnonymousFunction or MethodKind.LocalFunction } callback)
                            foreach (IOperation nested in bodies(callback)) pending.Enqueue((nested, needs));
                }
                ImmutableArray<IInvocationOperation> calls = [.. Operations(body, token).OfType<IInvocationOperation>()];
                foreach (IOperation nested in calls.Where(static call => call.TargetMethod.MethodKind is MethodKind.LocalFunction).SelectMany(call => bodies(call.TargetMethod)))
                    pending.Enqueue((nested, required));
                foreach ((IOperation value, ImmutableHashSet<IParameterSymbol> needs) in calls.SelectMany(call => call.Arguments
                    .Where(static argument => argument.Parameter?.Type.TypeKind is TypeKind.Delegate)
                    .Select(argument => (argument.Value, required.Add(argument.Parameter!.OriginalDefinition)))
                    .Concat(call is { TargetMethod.MethodKind: MethodKind.DelegateInvoke, Instance: { } instance } ? [(instance, required)] : []))) Follow(value, needs);
            }
            equations.Add(parameter, [.. routes]);
        }
        HashSet<IParameterSymbol> invoked = [with(SymbolEqualityComparer.Default)];
        bool changed;
        do {
            token.ThrowIfCancellationRequested();
            changed = false;
            foreach (KeyValuePair<IParameterSymbol, ImmutableArray<ImmutableHashSet<IParameterSymbol>>> equation in equations)
                if (!invoked.Contains(equation.Key) && equation.Value.Any(route => route.All(invoked.Contains))) changed |= invoked.Add(equation.Key);
        } while (changed);
        return invoked.Contains(source);
    }

    private Scope? Find(IOperation operation, CancellationToken token) {
        Stack<IOperation> owners = [];
        for (IOperation? held = operation; held is not null; held = held.Parent)
            if (held.Parent is null || held is IAnonymousFunctionOperation or ILocalFunctionOperation) owners.Push(held);
        ControlFlowGraph? parent = null;
        while (owners.Count > 0) {
            token.ThrowIfCancellationRequested();
            IOperation owner = owners.Pop();
            ControlFlowGraph? enclosing = parent;
            Optional<ControlFlowGraph> graph = graphs.GetOrAdd(owner.Syntax, syntax => {
                ControlFlowGraph? made = owner switch {
                    ILocalFunctionOperation function => enclosing!.GetLocalFunctionControlFlowGraph(function.Symbol, token),
                    IAnonymousFunctionOperation function => enclosing!.GetAnonymousFunctionControlFlowGraph(
                        enclosing.Blocks.SelectMany(static block => block.Operations.Concat(block.BranchValue is { } branch ? [branch] : []))
                            .SelectMany(static body => body.DescendantsAndSelf()).OfType<IFlowAnonymousFunctionOperation>()
                            .Single(candidate => SymbolEqualityComparer.Default.Equals(candidate.Symbol, function.Symbol)), token),
                    _ => ControlFlowGraph.Create(syntax, owner.SemanticModel!, token),
                };
                return made is null ? default : new Optional<ControlFlowGraph>(made);
            });
            if (!graph.HasValue) return null;
            parent = graph.Value;
        }
        return parent is null ? null : scopes.GetOrAdd(parent, graph => new Scope(graph, token));
    }

    private static ImmutableArray<Optional<Value>> Reached(Scope scope, IOperation at, int? context, Func<IOperation, Optional<IOperation>> definition, CancellationToken token) {
        HashSet<Value?> result = [];
        Queue<(int State, int Before)> pending = new(scope.Positions[at.Syntax.Span]
            .Where(point => scope.Events[point.Block][point.Before].Kind == at.Kind).SelectMany(position =>
            context is { } selected
                ? position.Block == scope.States[selected].Block ? [(selected, position.Before)] : Enumerable.Empty<(int, int)>()
                : scope.StatesByBlock[position.Block].Select(state => (state, position.Before))));
        HashSet<(int State, int Before)> visited = [];
        while (pending.Count > 0) {
            token.ThrowIfCancellationRequested();
            (int state, int before) = pending.Dequeue();
            if (!visited.Add((state, before))) continue;
            ImmutableArray<IOperation> events = scope.Events[scope.States[state].Block];
            bool assigned = false;
            for (int index = before - 1; index >= 0 && !assigned; index--) {
                Optional<IOperation> value = definition(events[index]);
                if (!value.HasValue) continue;
                result.Add(new Value(value.Value, (scope, state)));
                assigned = true;
            }
            if (assigned) continue;
            if (scope.Predecessors[state].IsEmpty) result.Add(null);
            foreach (int predecessor in scope.Predecessors[state]) pending.Enqueue((predecessor, scope.Events[scope.States[predecessor].Block].Length));
        }
        return [.. result.Select(static value => value is null ? default : new Optional<Value>(value))];
    }

    private static Optional<IOperation> Definition(IOperation current, ISymbol? symbol) => current switch {
        ISimpleAssignmentOperation assignment when Same(assignment.Target, symbol) => new Optional<IOperation>(assignment.Value),
        ICompoundAssignmentOperation assignment when Same(assignment.Target, symbol) => new Optional<IOperation>(assignment),
        IIncrementOrDecrementOperation changed when Same(changed.Target, symbol) => new Optional<IOperation>(changed),
        IDeconstructionAssignmentOperation assignment when assignment.Target.DescendantsAndSelf().Any(target => Same(target, symbol)) => new Optional<IOperation>(assignment),
        IInvocationOperation call when call.Arguments.FirstOrDefault(argument => argument.Parameter?.RefKind is RefKind.Ref or RefKind.Out && Same(argument.Value, symbol)) is { } argument => new Optional<IOperation>(argument),
        IObjectCreationOperation creation when creation.Arguments.FirstOrDefault(argument => argument.Parameter?.RefKind is RefKind.Ref or RefKind.Out && Same(argument.Value, symbol)) is { } argument => new Optional<IOperation>(argument),
        _ => default,
    };

    private static bool Same(IOperation operation, ISymbol? symbol) => symbol is not null && SymbolEqualityComparer.Default.Equals(Variable(operation), symbol);

    internal sealed class Value(IOperation operation, (Scope Scope, int State)? context) : IEquatable<Value> {
        internal IOperation Operation { get; } = operation;

        internal (Scope Scope, int State)? Context { get; } = context;

        internal Value With(IOperation child) => new(child, Context);

        internal bool Originates(Value source, CancellationToken token) =>
            Operation.Syntax.SyntaxTree == source.Operation.Syntax.SyntaxTree && Operation.Syntax.Span == source.Operation.Syntax.Span
            && (source.Context, Context) switch {
                (null, _) => true,
                ( { } origin, { } at) when at.Scope == origin.Scope =>
                    at.Scope.Reaches([(at.State, at.Scope.Events[at.Scope.States[at.State].Block].Length)], (state, _) => state == origin.State, token),
                _ => false,
            };

        public bool Equals(Value? other) => other is not null && Operation == other.Operation && Nullable.Equals(Context, other.Context);

        public override bool Equals(object? obj) => obj is Value value && Equals(value);

        public override int GetHashCode() => Operation.GetHashCode() ^ Context.GetHashCode();
    }

    internal sealed class Scope {
        internal Scope(ControlFlowGraph graph, CancellationToken token) {
            Events = [.. graph.Blocks.Select(block => ImmutableArray.CreateRange(block.Operations.Concat(block.BranchValue is { } branch ? [branch] : []).SelectMany(value => Operations(value, token))))];
            Positions = Events.SelectMany(static (events, block) => events.Select((value, before) => (value.Syntax.Span, Block: block, Before: before)))
                .ToLookup(static point => point.Span, static point => (point.Block, point.Before));
            List<(int Block, int? Continuation)> states = [];
            Dictionary<(int Block, int? Continuation), int> indexes = [];
            List<List<int>> predecessors = [];
            List<(int Block, int? Continuation)> continuations = [];
            Dictionary<(int Block, int? Continuation), int> returns = [];
            HashSet<int> blocks = [];
            Queue<int> pending = [];
            int State((int Block, int? Continuation) value) {
                if (indexes.TryGetValue(value, out int held)) return held;
                int index = states.Count;
                indexes.Add(value, index);
                states.Add(value);
                predecessors.Add([]);
                blocks.Add(value.Block);
                pending.Enqueue(index);
                return index;
            }
            (int Block, int? Continuation)? Next(ControlFlowBranch branch, int? continuation) {
                if (branch.Semantics is ControlFlowBranchSemantics.StructuredExceptionHandling)
                    return continuation is { } resumed ? continuations[resumed] : null;
                if (branch.Destination is not { IsReachable: true } destination) return null;
                (int Block, int? Continuation) next = (destination.Ordinal, continuation);
                for (int index = branch.FinallyRegions.Length - 1; index >= 0; index--) {
                    if (!returns.TryGetValue(next, out int exit)) {
                        exit = continuations.Count;
                        returns.Add(next, exit);
                        continuations.Add(next);
                    }
                    next = (branch.FinallyRegions[index].FirstBlockOrdinal, exit);
                }
                return next;
            }
            void Expand() {
                while (pending.Count > 0) {
                    token.ThrowIfCancellationRequested();
                    int current = pending.Dequeue();
                    (int ordinal, int? continuation) = states[current];
                    BasicBlock block = graph.Blocks[ordinal];
                    foreach (ControlFlowBranch branch in new[] { block.FallThroughSuccessor, block.ConditionalSuccessor }.OfType<ControlFlowBranch>())
                        if (Next(branch, continuation) is { } next) predecessors[State(next)].Add(current);
                }
            }
            _ = State((graph.Blocks[0].Ordinal, default));
            Expand();
            foreach (BasicBlock block in graph.Blocks.Where(static block => block.IsReachable)) {
                if (blocks.Contains(block.Ordinal)) continue;
                _ = State((block.Ordinal, default));
                Expand();
            }
            States = [.. states];
            StatesByBlock = states.Select(static (value, index) => (value.Block, Index: index)).ToLookup(static state => state.Block, static state => state.Index);
            Predecessors = [.. predecessors.Select(static values => ImmutableArray.CreateRange(values))];
        }

        internal ImmutableArray<ImmutableArray<IOperation>> Events { get; }

        internal ILookup<TextSpan, (int Block, int Before)> Positions { get; }

        internal ImmutableArray<(int Block, int? Continuation)> States { get; }

        internal ILookup<int, int> StatesByBlock { get; }

        internal ImmutableArray<ImmutableArray<int>> Predecessors { get; }

        internal ConcurrentDictionary<(IOperation Operation, int? State), ImmutableArray<Optional<Value>>> Values { get; } = [];

        internal ConcurrentDictionary<ISymbol, ConcurrentDictionary<IOperation, ImmutableArray<Optional<Value>>>> Symbols { get; } = new(SymbolEqualityComparer.Default);

        internal bool Reaches(IEnumerable<(int State, int Before)> seeds, Func<int, int, bool> found, CancellationToken token) {
            Queue<(int State, int Before)> pending = new(seeds);
            HashSet<(int State, int Before)> visited = [];
            while (pending.Count is > 0) {
                token.ThrowIfCancellationRequested();
                (int state, int before) = pending.Dequeue();
                if (!visited.Add((state, before))) continue;
                if (found(state, before)) return true;
                foreach (int predecessor in Predecessors[state]) pending.Enqueue((predecessor, Events[States[predecessor].Block].Length));
            }
            return false;
        }
    }
}

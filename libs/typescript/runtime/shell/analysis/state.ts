import { Array, Data, Equal, HashMap, HashSet, Option } from 'effect';
import type { Stmt } from 'sh-syntax';
import type { Invocation } from '#/analysis/invocation';
import { Binding, Origin, Scope } from '#/evaluation/value';
import type { Dialect } from '#/syntax/dialect';
import type { Script } from '#/syntax/script';

// --- [TYPES] ---------------------------------------------------------------------------

type Callee = Data.TaggedEnum<{ Absent: object; Defined: { readonly body: Stmt; readonly script: Script } }>;

// --- [CONSTANTS] -----------------------------------------------------------------------

const Callee = Data.taggedEnum<Callee>();
const unknown = HashSet.make(Binding.Unknown({ origins: HashSet.empty() }));

// --- [MODELS] --------------------------------------------------------------------------

class Variable extends Data.Class<{ readonly values: HashSet.HashSet<Binding>; readonly exported: boolean }> {
    get single(): Binding {
        const [only, ...more] = this.values;
        return only !== undefined && more.length === 0 ? only : Binding.Unknown({ origins: this.origins });
    }

    get origins(): HashSet.HashSet<Origin> {
        return HashSet.reduce(this.values, HashSet.empty<Origin>(), (origins, binding) => HashSet.union(origins, binding._tag === 'Unknown' ? binding.origins : HashSet.make(Origin.Literal())));
    }

    join(other: Variable): Variable {
        return Variable.of(HashSet.union(this.values, other.values), this.exported || other.exported);
    }

    static of(values: HashSet.HashSet<Binding>, exported: boolean): Variable {
        const held = new Variable({ values, exported });
        return HashSet.some(values, Binding.$is('Unknown')) ? new Variable({ values: HashSet.make(Binding.Unknown({ origins: held.origins })), exported }) : held;
    }
}

class State extends Data.Class<{
    readonly variables: HashMap.HashMap<string, Variable>;
    readonly unlisted: Variable;
    readonly positional: Option.Option<readonly Binding[]>;
    readonly name: string;
    readonly functions: HashMap.HashMap<string, HashSet.HashSet<Callee>>;
    readonly callee: HashSet.HashSet<Callee>;
}> {
    // --- [READING]

    variable(name: string): Variable {
        return Option.getOrElse(HashMap.get(this.variables, name), () => this.unlisted);
    }

    callees(name: string): HashSet.HashSet<Callee> {
        return Option.getOrElse(HashMap.get(this.functions, name), () => this.callee);
    }

    scopes(names: ReadonlySet<string>, substitutions: Scope['substitutions'] = HashMap.empty()): readonly Scope[] {
        const chosen = [...HashMap.entries(this.variables)].reduce<readonly HashMap.HashMap<string, Binding>[]>(
            (scopes, [name, held]) => (names.has(name) && HashSet.size(held.values) > 1 ? scopes.flatMap((scope) => [...held.values].map((binding) => HashMap.set(scope, name, binding))) : scopes),
            [HashMap.map(this.variables, (held) => held.single)],
        );
        return chosen.map((variables) => new Scope({ variables, unlisted: this.unlisted.single, positional: this.positional, name: this.name, substitutions }));
    }

    // --- [CHANGING]

    assign(name: string, values: HashSet.HashSet<Binding>, exported?: boolean): State {
        return new State({ ...this, variables: HashMap.set(this.variables, name, Variable.of(values, exported ?? this.variable(name).exported)) });
    }

    bound(name: string, binding: Binding): State {
        return this.assign(name, HashSet.make(binding));
    }

    absorbed(before: Scope, after: Scope): State {
        return new State({ ...this, variables: HashMap.reduce(after.variables, this.variables, (variables, binding, name) => (Equal.equals(Option.some(binding), HashMap.get(before.variables, name)) ? variables : HashMap.set(variables, name, Variable.of(HashSet.make(binding), this.variable(name).exported)))) });
    }

    entered(scope: Scope, dialect: Dialect, positional: Option.Option<readonly Binding[]>, name: string): State {
        const started = State.started(scope, dialect);
        return new State({ ...started, variables: HashMap.reduce(this.variables, started.variables, (variables, held, key) => (held.exported || key === 'PWD' ? HashMap.set(variables, key, held) : variables)), positional, name });
    }

    havoc(): State {
        return new State({ ...this, variables: HashMap.empty(), unlisted: Variable.of(unknown, true), positional: Option.none(), functions: HashMap.empty(), callee: HashSet.make(Callee.Absent()) });
    }

    // --- [JOINING]

    join(other: State): State {
        const names = new Set([...HashMap.keys(this.variables), ...HashMap.keys(other.variables)]);
        const callees = new Set([...HashMap.keys(this.functions), ...HashMap.keys(other.functions)]);
        return new State({
            variables: HashMap.fromIterable(names.values().map((name) => [name, this.variable(name).join(other.variable(name))] as const)),
            unlisted: this.unlisted.join(other.unlisted),
            positional: Option.filter(this.positional, () => Equal.equals(this.positional, other.positional)),
            name: this.name,
            functions: HashMap.fromIterable(callees.values().map((name) => [name, HashSet.union(this.callees(name), other.callees(name))] as const)),
            callee: HashSet.union(this.callee, other.callee),
        });
    }

    widen(next: State, known: ReadonlySet<string>): State {
        const finite = (binding: Binding) => Binding.$match(binding, { Unset: () => true, Unknown: () => true, Known: ({ value }) => known.has(value), Listed: ({ items }) => items.every((item) => known.has(item)) });
        return new State({
            ...next,
            variables: HashMap.map(next.variables, (held, name) => (name === '?' || HashSet.every(HashSet.difference(held.values, this.variable(name).values), finite) ? held : Variable.of(HashSet.make(Binding.Unknown({ origins: held.origins })), held.exported))),
            positional: Option.filter(next.positional, () => Equal.equals(next.positional, this.positional)),
        });
    }

    static started(scope: Scope, dialect: Dialect): State {
        return new State({
            variables: HashMap.map(scope.variables, (binding, name) => Variable.of(HashSet.make(binding), Object.hasOwn(dialect.environment, name))),
            unlisted: Variable.of(HashSet.make(scope.unlisted), false),
            positional: scope.positional,
            name: scope.name,
            functions: HashMap.empty(),
            callee: HashSet.make(Callee.Absent()),
        });
    }

    static union(states: readonly Option.Option<State>[]): Option.Option<State> {
        return Array.match(Array.getSomes(states), { onEmpty: () => Option.none(), onNonEmpty: (found) => Option.some(Array.tailNonEmpty(found).reduce((joined, state) => joined.join(state), Array.headNonEmpty(found))) });
    }
}

class Flow extends Data.Class<{
    readonly zero: Option.Option<State>;
    readonly nonzero: Option.Option<State>;
    readonly breaks: Option.Option<State>;
    readonly continues: Option.Option<State>;
    readonly returns: Option.Option<State>;
    readonly exits: Option.Option<State>;
    readonly found: readonly Invocation[];
}> {
    get next(): Option.Option<State> {
        return State.union([this.zero, this.nonzero]);
    }

    join(other: Flow): Flow {
        return new Flow({
            zero: State.union([this.zero, other.zero]),
            nonzero: State.union([this.nonzero, other.nonzero]),
            breaks: State.union([this.breaks, other.breaks]),
            continues: State.union([this.continues, other.continues]),
            returns: State.union([this.returns, other.returns]),
            exits: State.union([this.exits, other.exits]),
            found: [...this.found, ...other.found],
        });
    }

    followed(ran: Flow): Flow {
        return new Flow({ ...this.join(ran), zero: ran.zero, nonzero: ran.nonzero });
    }

    static readonly none = new Flow({ zero: Option.none(), nonzero: Option.none(), breaks: Option.none(), continues: Option.none(), returns: Option.none(), exits: Option.none(), found: [] });

    static ran(zero: Option.Option<State>, nonzero: Option.Option<State>, found: readonly Invocation[] = []): Flow {
        return new Flow({ ...Flow.none, zero: Option.map(zero, (state) => state.bound('?', Binding.Known({ value: '0' }))), nonzero: Option.map(nonzero, (state) => state.bound('?', Binding.Unknown({ origins: HashSet.empty() }))), found });
    }

    static joined(flows: readonly Flow[]): Flow {
        return flows.reduce((joined, flow) => joined.join(flow), Flow.none);
    }
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { Callee, Flow, State };

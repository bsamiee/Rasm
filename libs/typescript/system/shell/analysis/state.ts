import { Array, Combiner, Data, Effect, Equal, HashMap, HashSet, Option, Record, Struct } from 'effect';
import type { Stmt } from 'sh-syntax';
import type { Invocation } from '#/analysis/invocation';
import { Binding, type Origin, Scope, trace } from '#/evaluation/value';
import type { Dialect } from '#/syntax/dialect';
import type { Script } from '#/syntax/script';

// --- [TYPES] ---------------------------------------------------------------------------

type Callee = Data.TaggedEnum<{ Absent: object; Defined: { readonly body: Stmt; readonly script: Script } }>;
type Channel = keyof typeof channels;

// --- [CONSTANTS] -----------------------------------------------------------------------

const Callee = Data.taggedEnum<Callee>();
const status = '?';

// --- [MODELS] --------------------------------------------------------------------------

class Variable extends Data.Class<{ readonly values: HashSet.HashSet<Binding>; readonly exported: boolean }> {
    get origins(): HashSet.HashSet<Origin> {
        return HashSet.reduce(this.values, HashSet.empty<Origin>(), (origins, binding) => HashSet.union(origins, trace(binding)));
    }

    get binding(): Binding {
        return Option.getOrElse(
            Option.filter(this.values.pipe(Array.fromIterable, Array.head), () => HashSet.size(this.values) === 1),
            () => Binding.Unknown({ origins: this.origins }),
        );
    }

    join(that: Variable): Variable {
        return Variable.of(HashSet.union(this.values, that.values), this.exported || that.exported);
    }

    static of(values: HashSet.HashSet<Binding>, exported: boolean): Variable {
        const held = new Variable({ values, exported });
        return HashSet.some(values, Binding.$is('Unknown')) ? new Variable({ values: HashSet.make(Binding.Unknown({ origins: held.origins })), exported }) : held;
    }
}

class Total<A> extends Data.Class<{ readonly listed: HashMap.HashMap<string, A>; readonly unlisted: A }> {
    get(name: string): A {
        return Option.getOrElse(HashMap.get(this.listed, name), () => this.unlisted);
    }

    set(name: string, value: A): Total<A> {
        return new Total({ listed: HashMap.set(this.listed, name, value), unlisted: this.unlisted });
    }

    join(that: Total<A>, joined: (self: A, that: A) => A): Total<A> {
        return new Total({ listed: HashMap.map(HashMap.union(this.listed, that.listed), (_, name) => joined(this.get(name), that.get(name))), unlisted: joined(this.unlisted, that.unlisted) });
    }

    static of<A>(unlisted: A): Total<A> {
        return new Total({ listed: HashMap.empty(), unlisted });
    }
}

class State extends Data.Class<{ readonly variables: Total<Variable>; readonly functions: Total<HashSet.HashSet<Callee>>; readonly positional: Option.Option<readonly Binding[]>; readonly name: string }> {
    // --- [READING]

    project(substitutions: Scope['substitutions'] = HashMap.empty()): Scope {
        return new Scope({ variables: HashMap.map(this.variables.listed, (held) => held.binding), unlisted: this.variables.unlisted.binding, positional: this.positional, name: this.name, substitutions });
    }

    split(names: Iterable<string>, substitutions?: Scope['substitutions']): readonly Scope[] {
        const several = Array.dedupe(Array.fromIterable(names)).flatMap((name) =>
            Option.toArray(
                Option.filter(
                    Option.map(HashMap.get(this.variables.listed, name), ({ values }) => ({ name, values: Array.fromIterable(values) })),
                    ({ values }) => values.length > 1,
                ),
            ),
        );
        return several.reduce<readonly Scope[]>((scopes, { name, values }) => Array.cartesianWith(scopes, values, (scope, binding) => scope.set(name, binding)), [this.project(substitutions)]);
    }

    // --- [CHANGING]

    assign(name: string, values: HashSet.HashSet<Binding>, exported = this.variables.get(name).exported): State {
        return new State({ ...this, variables: this.variables.set(name, Variable.of(values, exported)) });
    }

    bind(name: string, binding: Binding): State {
        return this.assign(name, HashSet.make(binding));
    }

    absorb(before: Scope, after: Scope): State {
        return HashMap.reduce(after.variables, this as State, (state, binding, name) => (Equal.equals(Option.some(binding), HashMap.get(before.variables, name)) ? state : state.bind(name, binding)));
    }

    define(names: readonly string[], callee: Callee): State {
        return new State({ ...this, functions: names.reduce((functions, name) => functions.set(name, HashSet.make(callee)), this.functions) });
    }

    position(positional: State['positional']): State {
        return new State({ ...this, positional });
    }

    restore(to: State, locals: Iterable<string>): State {
        return new State({ ...this, positional: to.positional, variables: Array.fromIterable(locals).reduce((variables, local) => variables.set(local, to.variables.get(local)), this.variables) });
    }

    enter(scope: Scope, dialect: Dialect, positional: State['positional'], name: string): State {
        const started = State.start(scope, dialect);
        const passed = HashMap.filter(this.variables.listed, (held, key) => held.exported || key === 'PWD');
        return new State({ ...started, variables: new Total({ ...started.variables, listed: HashMap.union(started.variables.listed, passed) }), positional, name });
    }

    havoc(): State {
        return new State({ variables: Total.of(Variable.of(HashSet.make(Binding.Unknown({ origins: HashSet.empty() })), true)), functions: Total.of(HashSet.make(Callee.Absent())), positional: Option.none(), name: this.name });
    }

    // --- [JOINING]

    join(that: State): State {
        return new State({
            variables: this.variables.join(that.variables, (held, other) => held.join(other)),
            functions: this.functions.join(that.functions, HashSet.union),
            positional: Option.filter(this.positional, () => Equal.equals(this.positional, that.positional)),
            name: this.name,
        });
    }

    widen(next: State, thresholds: ReadonlySet<string>): State {
        const finite = Binding.$match({ Unset: () => true, Unknown: () => true, Known: ({ value }) => thresholds.has(value), Listed: ({ items }) => items.every((item) => thresholds.has(item)) });
        const settled = (held: Variable, name: string) => name === status || HashSet.every(HashSet.difference(held.values, this.variables.get(name).values), finite);
        return new State({
            ...next,
            variables: new Total({ ...next.variables, listed: HashMap.map(next.variables.listed, (held, name) => (settled(held, name) ? held : Variable.of(HashSet.make(Binding.Unknown({ origins: held.origins })), held.exported))) }),
            positional: Option.filter(next.positional, () => Equal.equals(next.positional, this.positional)),
        });
    }

    static start(scope: Scope, dialect: Dialect): State {
        return new State({
            variables: new Total({ listed: HashMap.map(scope.variables, (binding, name) => Variable.of(HashSet.make(binding), Object.hasOwn(dialect.environment, name))), unlisted: Variable.of(HashSet.make(scope.unlisted), false) }),
            functions: Total.of(HashSet.make(Callee.Absent())),
            positional: scope.positional,
            name: scope.name,
        });
    }
}

const reached = Option.makeReducer(Combiner.make((self: State, that: State) => self.join(that)));
const channels = Struct.Record(['zero', 'nonzero', 'breaks', 'continues', 'returns', 'exits'], reached);
const flows = Struct.makeReducer({ ...channels, found: Array.getReadonlyReducerConcat<Invocation>() });

class Flow extends Data.Class<Readonly<typeof flows.initialValue>> {
    prepend(found: readonly Invocation[]): Flow {
        return new Flow({ ...this, found: [...found, ...this.found] });
    }

    route<C extends Channel>(sources: Readonly<Record<C, readonly Channel[]>>): Flow {
        return new Flow({ ...this, ...Record.map(sources, (from) => reached.combineAll(from.map((channel) => this[channel]))) });
    }

    map(change: (state: State) => State): Flow {
        return new Flow({ ...this, ...Record.map(channels, (_, channel) => Option.map(this[channel], change)) });
    }

    branch<C extends Channel, E, R>(arms: Readonly<Record<C, (state: State) => Effect.Effect<Flow, E, R>>>): Effect.Effect<Flow, E, R> {
        return Effect.map(
            Effect.forEach(Record.toEntries(arms), ([channel, arm]) => Effect.transposeOption(Option.map(this[channel], arm))),
            (ran) => Flow.join([this.route(Record.map(arms, () => [])), ...Array.getSomes(ran)]),
        );
    }

    contain(outer: State): Flow {
        return Flow.status(Option.as(this.zero, outer), Option.as(reached.combine(this.nonzero, this.exits), outer), this.found);
    }

    conclude(change: (state: State) => State): Flow {
        return this.route({ zero: ['zero', 'returns'], returns: [], breaks: [], continues: [] }).map(change);
    }

    andThen<E, R>(step: (state: State) => Effect.Effect<Flow, E, R>): Effect.Effect<Flow, E, R> {
        return this.route({ zero: ['zero', 'nonzero'], nonzero: [] }).branch({ zero: step });
    }

    static readonly none = new Flow(flows.initialValue);

    static of(channel: Channel, state: State, found: readonly Invocation[] = []): Flow {
        return new Flow({ ...flows.initialValue, [channel]: Option.some(state), found });
    }

    static status(zero: Option.Option<State>, nonzero: Option.Option<State>, found: readonly Invocation[] = []): Flow {
        return new Flow({ ...flows.initialValue, zero: Option.map(zero, (state) => state.bind(status, Binding.Known({ value: '0' }))), nonzero: Option.map(nonzero, (state) => state.bind(status, Binding.Unknown({ origins: HashSet.empty() }))), found });
    }

    static havoc(state: State, found: readonly Invocation[]): Flow {
        return Flow.status(Option.some(state.havoc()), Option.some(state.havoc()), found);
    }

    static join(joining: readonly Flow[]): Flow {
        return new Flow(flows.combineAll(joining));
    }
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { Callee, Flow, State };

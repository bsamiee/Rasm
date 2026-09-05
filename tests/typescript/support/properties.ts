import type { Vitest } from '@effect/vitest';
import { Arbitrary, Data, Effect, Equal, FastCheck, Inspectable, type Order, Schema, type TestServices } from 'effect';

// --- [TYPES] ---------------------------------------------------------------------------

type PropertyArbitraries = { readonly [K in string]: Schema.Schema.Any | FastCheck.Arbitrary<unknown> };
type ArbitraryValues<A extends PropertyArbitraries> = {
    readonly [K in keyof A]: A[K] extends FastCheck.Arbitrary<infer T> ? T : Schema.Schema.Type<A[K]>;
};
type PropertyApi<R> = Vitest.Methods<R> | Vitest.MethodsNonLive<R>;
type Binary<A> = (left: A, right: A) => A;
type Equals<A> = (self: A, that: A) => boolean;

interface Isomorphism<A, B> {
    readonly to: (value: A) => B;
    readonly from: (image: B) => A;
}

interface Counterexample<S, Args> {
    readonly label: string;
    readonly implementation: S;
    readonly args: Args;
}

interface PropertyDefinition<S, A extends PropertyArbitraries, E, R> {
    readonly name: string;
    readonly arbitraries: A;
    readonly predicate: (subject: S, args: ArbitraryValues<A>) => Effect.Effect<boolean, E, R | TestServices.TestServices>;
    readonly counterexample: Counterexample<S, ArbitraryValues<A>>;
}

interface RegisteredProperty<S, R = never> {
    readonly name: string;
    readonly register: (api: PropertyApi<R>, subject: S) => void;
}

interface PropertyOptions<S, Args> {
    readonly name?: string;
    readonly counterexample: Counterexample<S, Args>;
}

interface BinaryOptions<A, Args> extends PropertyOptions<Binary<A>, Args> {
    readonly arb: FastCheck.Arbitrary<A>;
    readonly equals?: Equals<A>;
}

interface Property {
    readonly verifyCounterexample: <S, A extends PropertyArbitraries, E, R>(
        definition: PropertyDefinition<S, A, E, R>,
    ) => Effect.Effect<void, PropertyError, R | TestServices.TestServices>;
    readonly define: <S, const A extends PropertyArbitraries, E = never, R = never>(
        definition: PropertyDefinition<S, A, E, R>,
    ) => RegisteredProperty<S, R>;
    readonly register: <S, R>(api: PropertyApi<R>, subject: S, properties: readonly RegisteredProperty<S, R>[]) => void;
    readonly associative: <A>(options: BinaryOptions<A, { readonly a: A; readonly b: A; readonly c: A }>) => RegisteredProperty<Binary<A>>;
    readonly commutative: <A>(options: BinaryOptions<A, { readonly a: A; readonly b: A }>) => RegisteredProperty<Binary<A>>;
    readonly idempotent: <A>(options: BinaryOptions<A, { readonly a: A }>) => RegisteredProperty<Binary<A>>;
    readonly identity: <A>(options: BinaryOptions<A, { readonly a: A }> & { readonly empty: A }) => RegisteredProperty<Binary<A>>;
    readonly equivalence: <A>(
        options: PropertyOptions<Equals<A>, { readonly a: A; readonly b: A; readonly c: A }> & { readonly arb: FastCheck.Arbitrary<A> },
    ) => RegisteredProperty<Equals<A>>;
    readonly order: <A>(
        options: PropertyOptions<Order.Order<A>, { readonly a: A; readonly b: A; readonly c: A }> & { readonly arb: FastCheck.Arbitrary<A> },
    ) => RegisteredProperty<Order.Order<A>>;
    readonly inverse: <A, B>(
        options: PropertyOptions<Isomorphism<A, B>, { readonly a: A }> & { readonly arb: FastCheck.Arbitrary<A>; readonly equals?: Equals<A> },
    ) => RegisteredProperty<Isomorphism<A, B>>;
    readonly deterministic: <I, A, E, R>(
        options: PropertyOptions<(input: I) => Effect.Effect<A, E, R>, { readonly input: I }> & {
            readonly arb: FastCheck.Arbitrary<I>;
            readonly equals?: Equals<A>;
        },
    ) => RegisteredProperty<(input: I) => Effect.Effect<A, E, R>, R>;
    readonly homomorphic: <A, B>(
        options: PropertyOptions<(value: A) => B, { readonly a: A; readonly b: A }> & {
            readonly arb: FastCheck.Arbitrary<A>;
            readonly combine: Binary<A>;
            readonly combineImage: Binary<B>;
            readonly equals?: Equals<B>;
        },
    ) => RegisteredProperty<(value: A) => B>;
    readonly monotone: <A>(
        options: PropertyOptions<(state: A) => A, { readonly a: A }> & { readonly arb: FastCheck.Arbitrary<A>; readonly order: Order.Order<A> },
    ) => RegisteredProperty<(state: A) => A>;
    readonly total: <I, E, R>(
        options: PropertyOptions<(input: I) => Effect.Effect<unknown, E, R>, { readonly input: I }> & { readonly arb: FastCheck.Arbitrary<I> },
    ) => RegisteredProperty<(input: I) => Effect.Effect<unknown, E, R>, R>;
    readonly roundtrip: <A, I>(
        options: PropertyOptions<Schema.Schema<A, I, never>, { readonly value: A }> & { readonly schema: Schema.Schema<A, I, never> },
    ) => RegisteredProperty<Schema.Schema<A, I, never>>;
    readonly machine: <Model extends object, Real>(
        options: PropertyOptions<() => { readonly model: Model; readonly real: Real }, { readonly run: Iterable<FastCheck.Command<Model, Real>> }> & {
            readonly commands: readonly FastCheck.Arbitrary<FastCheck.Command<Model, Real>>[];
        },
    ) => RegisteredProperty<() => { readonly model: Model; readonly real: Real }>;
    readonly machineAsync: <Model extends object, Real>(
        options: PropertyOptions<
            () => { readonly model: Model; readonly real: Real },
            { readonly run: Iterable<FastCheck.AsyncCommand<Model, Real>> }
        > & { readonly commands: readonly FastCheck.Arbitrary<FastCheck.AsyncCommand<Model, Real>>[] },
    ) => RegisteredProperty<() => { readonly model: Model; readonly real: Real }>;
    readonly interleave: (
        options: PropertyOptions<(schedule: FastCheck.Scheduler) => Promise<boolean>, { readonly schedule: FastCheck.Scheduler }>,
    ) => RegisteredProperty<(schedule: FastCheck.Scheduler) => Promise<boolean>>;
}

// --- [ERRORS] --------------------------------------------------------------------------

class PropertyError extends Data.Error<{
    readonly reason: 'violation' | 'counterexample';
    readonly property: string;
    readonly detail: string;
}> {
    readonly _tag = 'PropertyError' as const;
}

// --- [OPERATIONS] ----------------------------------------------------------------------

const _succeeds = <A, E, R>(work: Effect.Effect<A, E, R>): Effect.Effect<boolean, never, R> =>
    Effect.match(work, { onFailure: () => false, onSuccess: () => true });

const Property: Property = {
    // A predicate that fails on the counterexample rejects it, and one that holds on it is the defect the property must expose
    verifyCounterexample: (definition) =>
        definition.predicate(definition.counterexample.implementation, definition.counterexample.args).pipe(
            Effect.catchAllCause(() => Effect.succeed(false)),
            Effect.filterOrFail(
                (holds) => !holds,
                () => new PropertyError({ reason: 'counterexample', property: definition.name, detail: definition.counterexample.label }),
            ),
            Effect.asVoid,
        ),
    define: (definition) => ({
        name: definition.name,
        register: (api, subject) => {
            api.effect.prop(definition.name, definition.arbitraries, (args) =>
                Effect.asVoid(
                    Effect.filterOrFail(
                        definition.predicate(subject, args),
                        (holds) => holds,
                        () => new PropertyError({ reason: 'violation', property: definition.name, detail: Inspectable.toStringUnknown(args) }),
                    ),
                ),
            );
            api.effect(`${definition.name} rejects ${definition.counterexample.label}`, () => Property.verifyCounterexample(definition));
        },
    }),
    register: (api, subject, properties) => {
        for (const property of properties) {
            property.register(api, subject);
        }
    },

    // --- [BUILT_IN_PROPERTIES] ---------------------------------------------------------
    associative: (options) =>
        Property.define({
            name: options.name ?? 'combine is associative',
            arbitraries: { a: options.arb, b: options.arb, c: options.arb },
            predicate: (combine, { a, b, c }) =>
                Effect.sync(() => (options.equals ?? Equal.equals)(combine(combine(a, b), c), combine(a, combine(b, c)))),
            counterexample: options.counterexample,
        }),
    commutative: (options) =>
        Property.define({
            name: options.name ?? 'combine is commutative',
            arbitraries: { a: options.arb, b: options.arb },
            predicate: (combine, { a, b }) => Effect.sync(() => (options.equals ?? Equal.equals)(combine(a, b), combine(b, a))),
            counterexample: options.counterexample,
        }),
    idempotent: (options) =>
        Property.define({
            name: options.name ?? 'combine is idempotent',
            arbitraries: { a: options.arb },
            predicate: (combine, { a }) => Effect.sync(() => (options.equals ?? Equal.equals)(combine(a, a), a)),
            counterexample: options.counterexample,
        }),
    identity: (options) =>
        Property.define({
            name: options.name ?? 'the identity element is neutral',
            arbitraries: { a: options.arb },
            predicate: (combine, { a }) =>
                Effect.sync(() => {
                    const equals = options.equals ?? Equal.equals;
                    return equals(combine(options.empty, a), a) && equals(combine(a, options.empty), a);
                }),
            counterexample: options.counterexample,
        }),
    equivalence: (options) =>
        Property.define({
            name: options.name ?? 'equivalence is reflexive, symmetric, and transitive',
            arbitraries: { a: options.arb, b: options.arb, c: options.arb },
            predicate: (equals, { a, b, c }) =>
                Effect.sync(() => equals(a, a) && equals(a, b) === equals(b, a) && (!(equals(a, b) && equals(b, c)) || equals(a, c))),
            counterexample: options.counterexample,
        }),
    order: (options) =>
        Property.define({
            name: options.name ?? 'comparison defines a total order',
            arbitraries: { a: options.arb, b: options.arb, c: options.arb },
            predicate: (compare, { a, b, c }) =>
                Effect.sync(
                    () =>
                        compare(a, a) === 0 &&
                        compare(a, b) === -compare(b, a) &&
                        (!(compare(a, b) <= 0 && compare(b, c) <= 0) || compare(a, c) <= 0),
                ),
            counterexample: options.counterexample,
        }),
    inverse: (options) =>
        Property.define({
            name: options.name ?? 'decode recovers each encoded value',
            arbitraries: { a: options.arb },
            predicate: (isomorphism, { a }) => Effect.sync(() => (options.equals ?? Equal.equals)(isomorphism.from(isomorphism.to(a)), a)),
            counterexample: options.counterexample,
        }),
    deterministic: (options) =>
        Property.define({
            name: options.name ?? 'operation is deterministic',
            arbitraries: { input: options.arb },
            predicate: (subject, { input }) =>
                Effect.zipWith(subject(input), subject(input), (first, second) => (options.equals ?? Equal.equals)(first, second)),
            counterexample: options.counterexample,
        }),
    homomorphic: (options) =>
        Property.define({
            name: options.name ?? 'map commutes with combine',
            arbitraries: { a: options.arb, b: options.arb },
            predicate: (to, { a, b }) =>
                Effect.sync(() => (options.equals ?? Equal.equals)(to(options.combine(a, b)), options.combineImage(to(a), to(b)))),
            counterexample: options.counterexample,
        }),
    monotone: (options) =>
        Property.define({
            name: options.name ?? 'step never regresses',
            arbitraries: { a: options.arb },
            predicate: (step, { a }) => Effect.sync(() => options.order(a, step(a)) <= 0),
            counterexample: options.counterexample,
        }),
    total: (options) =>
        Property.define({
            name: options.name ?? 'operation is total',
            arbitraries: { input: options.arb },
            predicate: (subject, { input }) => _succeeds(subject(input)),
            counterexample: options.counterexample,
        }),
    roundtrip: (options) => {
        const equivalence = Schema.equivalence(options.schema);
        return Property.define({
            name: options.name ?? 'codec round-trips',
            arbitraries: { value: Arbitrary.make(options.schema) },
            predicate: (subject, { value }) =>
                Effect.match(Effect.flatMap(Schema.encode(subject)(value), Schema.decode(subject)), {
                    onFailure: () => false,
                    onSuccess: (back) => equivalence(value, back),
                }),
            counterexample: options.counterexample,
        });
    },
    machine: (options) =>
        Property.define({
            name: options.name ?? 'system conforms to its model',
            arbitraries: { run: FastCheck.commands([...options.commands]) },
            predicate: (setup, { run }) => _succeeds(Effect.try(() => FastCheck.modelRun(setup, run))),
            counterexample: options.counterexample,
        }),
    machineAsync: (options) =>
        Property.define({
            name: options.name ?? 'asynchronous system conforms to its model',
            arbitraries: { run: FastCheck.commands([...options.commands]) },
            predicate: (setup, { run }) => _succeeds(Effect.tryPromise(() => FastCheck.asyncModelRun(setup, run))),
            counterexample: options.counterexample,
        }),
    interleave: (options) =>
        Property.define({
            name: options.name ?? 'holds under every interleaving',
            arbitraries: { schedule: FastCheck.scheduler() },
            predicate: (subject, { schedule }) => Effect.promise(() => subject(schedule)),
            counterexample: options.counterexample,
        }),
};

// --- [EXPORTS] -------------------------------------------------------------------------

export {
    type ArbitraryValues,
    type Binary,
    type Equals,
    type Isomorphism,
    Property,
    type PropertyApi,
    type PropertyArbitraries,
    type PropertyDefinition,
    PropertyError,
    type PropertyOptions,
    type RegisteredProperty,
};

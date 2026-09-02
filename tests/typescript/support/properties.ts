import type { Vitest } from '@effect/vitest';
import { Data, Effect, Equal, Exit, type Order, Schema } from 'effect';
import * as Arbitrary from 'effect/Arbitrary';
import * as FastCheck from 'effect/FastCheck';
import type * as TestServices from 'effect/TestServices';

// --- [TYPES] ---------------------------------------------------------------------------

declare namespace Property {
    type Arbitraries = {
        readonly [K in string]: Schema.Schema.Any | FastCheck.Arbitrary<unknown>;
    };
    type ValuesOf<A extends Arbitraries> = {
        readonly [K in keyof A]: A[K] extends FastCheck.Arbitrary<infer T> ? T : Schema.Schema.Type<A[K]>;
    };
    type Api<R> = Vitest.Methods<R> | Vitest.MethodsNonLive<R>;
    type Binary<A> = (left: A, right: A) => A;
    type Isomorphism<A, B> = {
        readonly to: (value: A) => B;
        readonly from: (image: B) => A;
    };
    type Equals<A> = (self: A, that: A) => boolean;
    interface Definition<S, A extends Arbitraries, E, R> {
        readonly name: string;
        readonly arbitraries: A;
        readonly predicate: (subject: S, args: ValuesOf<A>) => Effect.Effect<boolean, E, R | TestServices.TestServices>;
        readonly counterexample: {
            readonly label: string;
            readonly implementation: S;
            readonly args: ValuesOf<A>;
        };
    }
    interface Registered<S, R = never> {
        readonly name: string;
        readonly register: (api: Api<R>, subject: S) => void;
    }
    interface Options<S, Args> {
        readonly name?: string;
        readonly counterexample: {
            readonly label: string;
            readonly implementation: S;
            readonly args: Args;
        };
    }
}

// --- [ERRORS] --------------------------------------------------------------------------

class PropertyViolationError extends Data.TaggedError('PropertyViolationError')<{
    readonly property: string;
    readonly args: unknown;
}> {}

class InvalidPropertyCounterexampleError extends Data.TaggedError('InvalidPropertyCounterexampleError')<{
    readonly property: string;
    readonly counterexample: string;
}> {}

// --- [OPERATIONS] ----------------------------------------------------------------------

const _assertProperty = <E, R>(
    verdict: Effect.Effect<boolean, E, R>,
    evidence: { readonly property: string; readonly args: unknown },
): Effect.Effect<void, E | PropertyViolationError, R> =>
    Effect.flatMap(verdict, (holds) => (holds ? Effect.void : Effect.fail(new PropertyViolationError(evidence))));

const _succeeds = <A, E, R>(work: Effect.Effect<A, E, R>): Effect.Effect<boolean, never, R> =>
    Effect.match(work, { onFailure: () => false, onSuccess: () => true });

const Property = {
    verifyCounterexample: <S, A extends Property.Arbitraries, E, R>(
        definition: Property.Definition<S, A, E, R>,
    ): Effect.Effect<void, InvalidPropertyCounterexampleError, R | TestServices.TestServices> =>
        Effect.flatMap(Effect.exit(definition.predicate(definition.counterexample.implementation, definition.counterexample.args)), (exit) =>
            Exit.match(exit, {
                onFailure: () => Effect.void,
                onSuccess: (holds) =>
                    holds
                        ? Effect.fail(
                              new InvalidPropertyCounterexampleError({
                                  property: definition.name,
                                  counterexample: definition.counterexample.label,
                              }),
                          )
                        : Effect.void,
            }),
        ),
    define: <S, const A extends Property.Arbitraries, E = never, R = never>(
        definition: Property.Definition<S, A, E, R>,
    ): Property.Registered<S, R> => ({
        name: definition.name,
        register: (api, subject) => {
            api.effect.prop(definition.name, definition.arbitraries, (args) =>
                _assertProperty(definition.predicate(subject, args), {
                    property: definition.name,
                    args,
                }),
            );
            api.effect(`${definition.name} rejects ${definition.counterexample.label}`, () => Property.verifyCounterexample(definition));
        },
    }),
    register: <S, R>(api: Property.Api<R>, subject: S, properties: ReadonlyArray<Property.Registered<S, R>>): void => {
        for (const property of properties) {
            property.register(api, subject);
        }
    },

    // --- [BUILT_IN_PROPERTIES]
    associative: <A>(
        options: Property.Options<Property.Binary<A>, { readonly a: A; readonly b: A; readonly c: A }> & {
            readonly arb: FastCheck.Arbitrary<A>;
            readonly equals?: Property.Equals<A>;
        },
    ): Property.Registered<Property.Binary<A>> =>
        Property.define({
            name: options.name ?? 'combine is associative',
            arbitraries: { a: options.arb, b: options.arb, c: options.arb },
            predicate: (combine, { a, b, c }) =>
                Effect.sync(() => (options.equals ?? Equal.equals)(combine(combine(a, b), c), combine(a, combine(b, c)))),
            counterexample: options.counterexample,
        }),
    commutative: <A>(
        options: Property.Options<Property.Binary<A>, { readonly a: A; readonly b: A }> & {
            readonly arb: FastCheck.Arbitrary<A>;
            readonly equals?: Property.Equals<A>;
        },
    ): Property.Registered<Property.Binary<A>> =>
        Property.define({
            name: options.name ?? 'combine is commutative',
            arbitraries: { a: options.arb, b: options.arb },
            predicate: (combine, { a, b }) => Effect.sync(() => (options.equals ?? Equal.equals)(combine(a, b), combine(b, a))),
            counterexample: options.counterexample,
        }),
    idempotent: <A>(
        options: Property.Options<Property.Binary<A>, { readonly a: A }> & {
            readonly arb: FastCheck.Arbitrary<A>;
            readonly equals?: Property.Equals<A>;
        },
    ): Property.Registered<Property.Binary<A>> =>
        Property.define({
            name: options.name ?? 'combine is idempotent',
            arbitraries: { a: options.arb },
            predicate: (combine, { a }) => Effect.sync(() => (options.equals ?? Equal.equals)(combine(a, a), a)),
            counterexample: options.counterexample,
        }),
    identity: <A>(
        options: Property.Options<Property.Binary<A>, { readonly a: A }> & {
            readonly arb: FastCheck.Arbitrary<A>;
            readonly empty: A;
            readonly equals?: Property.Equals<A>;
        },
    ): Property.Registered<Property.Binary<A>> =>
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
    equivalence: <A>(
        options: Property.Options<Property.Equals<A>, { readonly a: A; readonly b: A; readonly c: A }> & {
            readonly arb: FastCheck.Arbitrary<A>;
        },
    ): Property.Registered<Property.Equals<A>> =>
        Property.define({
            name: options.name ?? 'equivalence is reflexive, symmetric, and transitive',
            arbitraries: { a: options.arb, b: options.arb, c: options.arb },
            predicate: (equals, { a, b, c }) =>
                Effect.sync(() => equals(a, a) && equals(a, b) === equals(b, a) && (!(equals(a, b) && equals(b, c)) || equals(a, c))),
            counterexample: options.counterexample,
        }),
    order: <A>(
        options: Property.Options<Order.Order<A>, { readonly a: A; readonly b: A; readonly c: A }> & {
            readonly arb: FastCheck.Arbitrary<A>;
        },
    ): Property.Registered<Order.Order<A>> =>
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
    inverse: <A, B>(
        options: Property.Options<Property.Isomorphism<A, B>, { readonly a: A }> & {
            readonly arb: FastCheck.Arbitrary<A>;
            readonly equals?: Property.Equals<A>;
        },
    ): Property.Registered<Property.Isomorphism<A, B>> =>
        Property.define({
            name: options.name ?? 'decode recovers each encoded value',
            arbitraries: { a: options.arb },
            predicate: (isomorphism, { a }) => Effect.sync(() => (options.equals ?? Equal.equals)(isomorphism.from(isomorphism.to(a)), a)),
            counterexample: options.counterexample,
        }),
    deterministic: <I, A, E, R>(
        options: Property.Options<(input: I) => Effect.Effect<A, E, R>, { readonly input: I }> & {
            readonly arb: FastCheck.Arbitrary<I>;
            readonly equals?: Property.Equals<A>;
        },
    ): Property.Registered<(input: I) => Effect.Effect<A, E, R>, R> =>
        Property.define({
            name: options.name ?? 'operation is deterministic',
            arbitraries: { input: options.arb },
            predicate: (subject, { input }) =>
                Effect.zipWith(subject(input), subject(input), (first, second) => (options.equals ?? Equal.equals)(first, second)),
            counterexample: options.counterexample,
        }),
    homomorphic: <A, B>(
        options: Property.Options<(value: A) => B, { readonly a: A; readonly b: A }> & {
            readonly arb: FastCheck.Arbitrary<A>;
            readonly combine: Property.Binary<A>;
            readonly combineImage: Property.Binary<B>;
            readonly equals?: Property.Equals<B>;
        },
    ): Property.Registered<(value: A) => B> =>
        Property.define({
            name: options.name ?? 'map commutes with combine',
            arbitraries: { a: options.arb, b: options.arb },
            predicate: (to, { a, b }) =>
                Effect.sync(() => (options.equals ?? Equal.equals)(to(options.combine(a, b)), options.combineImage(to(a), to(b)))),
            counterexample: options.counterexample,
        }),
    monotone: <A>(
        options: Property.Options<(state: A) => A, { readonly a: A }> & {
            readonly arb: FastCheck.Arbitrary<A>;
            readonly order: Order.Order<A>;
        },
    ): Property.Registered<(state: A) => A> =>
        Property.define({
            name: options.name ?? 'step never regresses',
            arbitraries: { a: options.arb },
            predicate: (step, { a }) => Effect.sync(() => options.order(a, step(a)) <= 0),
            counterexample: options.counterexample,
        }),
    total: <I, E, R>(
        options: Property.Options<(input: I) => Effect.Effect<unknown, E, R>, { readonly input: I }> & { readonly arb: FastCheck.Arbitrary<I> },
    ): Property.Registered<(input: I) => Effect.Effect<unknown, E, R>, R> =>
        Property.define({
            name: options.name ?? 'operation is total',
            arbitraries: { input: options.arb },
            predicate: (subject, { input }) => _succeeds(subject(input)),
            counterexample: options.counterexample,
        }),
    roundtrip: <A, I>(
        options: Property.Options<Schema.Schema<A, I, never>, { readonly value: A }> & { readonly schema: Schema.Schema<A, I, never> },
    ): Property.Registered<Schema.Schema<A, I, never>> => {
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
    machine: <Model extends object, Real>(
        options: Property.Options<
            () => { readonly model: Model; readonly real: Real },
            { readonly run: Iterable<FastCheck.Command<Model, Real>> }
        > & {
            readonly commands: ReadonlyArray<FastCheck.Arbitrary<FastCheck.Command<Model, Real>>>;
        },
    ): Property.Registered<() => { readonly model: Model; readonly real: Real }> =>
        Property.define({
            name: options.name ?? 'system conforms to its model',
            arbitraries: { run: FastCheck.commands([...options.commands]) },
            predicate: (setup, { run }) => _succeeds(Effect.try(() => FastCheck.modelRun(setup, run))),
            counterexample: options.counterexample,
        }),
    machineAsync: <Model extends object, Real>(
        options: Property.Options<
            () => { readonly model: Model; readonly real: Real },
            { readonly run: Iterable<FastCheck.AsyncCommand<Model, Real>> }
        > & {
            readonly commands: ReadonlyArray<FastCheck.Arbitrary<FastCheck.AsyncCommand<Model, Real>>>;
        },
    ): Property.Registered<() => { readonly model: Model; readonly real: Real }> =>
        Property.define({
            name: options.name ?? 'asynchronous system conforms to its model',
            arbitraries: { run: FastCheck.commands([...options.commands]) },
            predicate: (setup, { run }) => _succeeds(Effect.tryPromise(() => FastCheck.asyncModelRun(setup, run))),
            counterexample: options.counterexample,
        }),
    interleave: (
        options: Property.Options<(schedule: FastCheck.Scheduler) => Promise<boolean>, { readonly schedule: FastCheck.Scheduler }>,
    ): Property.Registered<(schedule: FastCheck.Scheduler) => Promise<boolean>> =>
        Property.define({
            name: options.name ?? 'holds under every interleaving',
            arbitraries: { schedule: FastCheck.scheduler() },
            predicate: (subject, { schedule }) => Effect.promise(() => subject(schedule)),
            counterexample: options.counterexample,
        }),
} as const;

// --- [EXPORTS] -------------------------------------------------------------------------

export { InvalidPropertyCounterexampleError, Property, PropertyViolationError };

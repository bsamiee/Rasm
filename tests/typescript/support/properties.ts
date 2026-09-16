// --- [IMPORTS] -------------------------------------------------------------------------

import type { Vitest } from '@effect/vitest';
import { Data, Effect, type Equivalence, Function, type Order, pipe, Schema } from 'effect';
import type { Arbitrary } from 'effect/unstable/arbitrary';
import fc from 'fast-check';

// --- [TYPES] ---------------------------------------------------------------------------

type Values<A extends Vitest.Arbitraries> = { readonly [K in keyof A]: A[K] extends Schema.Schema<infer T> ? T : A[K] extends Arbitrary.Arbitrary<infer T> ? T : never };
type Binary<A> = (left: A, right: A) => A;
type Registration<S, R = never> = (it: Vitest.MethodsNonLive<R>, subject: S) => void;

interface Law<S, A extends Vitest.Arbitraries, E, R> {
    readonly name: string;
    readonly arbitraries: A;
    readonly predicate: (subject: S, args: Values<A>) => Effect.Effect<boolean, E, R>;
}

interface Counterexample<S, Args> {
    readonly label: string;
    readonly implementation: S;
    readonly args: Args;
}

// --- [ERRORS] --------------------------------------------------------------------------

type PropertyError = Data.TaggedEnum<{
    readonly counterexampleAccepted: { readonly property: string; readonly label: string };
    readonly falsified: { readonly property: string; readonly cause: unknown };
}>;

const PropertyError: Data.TaggedEnum.Constructor<PropertyError> = Data.taggedEnum<PropertyError>();

// --- [OPERATIONS] ----------------------------------------------------------------------

const law =
    <S, const A extends Vitest.Arbitraries, E, R>({ name, arbitraries, predicate }: Law<S, A, E, R>): Registration<S, R> =>
    (it, subject): void =>
        it.effect.prop(name, arbitraries, (args) => predicate(subject, args));

const rejection =
    <S, Args, E, R>(property: string, predicate: (subject: S, args: Args) => Effect.Effect<boolean, E, R>, counterexample: Counterexample<S, Args>): ((it: Vitest.MethodsNonLive<R>) => void) =>
    (it): void =>
        it.effect(`${property} rejects ${counterexample.label}`, () =>
            predicate(counterexample.implementation, counterexample.args).pipe(
                Effect.filterOrFail(Function.identity),
                Effect.flip,
                Effect.mapError(() => PropertyError.counterexampleAccepted({ property, label: counterexample.label })),
            ),
        );

const define =
    <S, const A extends Vitest.Arbitraries, E, R>(property: Law<S, A, E, R>, counterexample: Counterexample<S, Values<A>>): Registration<S, R> =>
    (it, subject): void => {
        law(property)(it, subject);
        rejection(property.name, property.predicate, counterexample)(it);
    };

// --- [LAWS] ----------------------------------------------------------------------------

const associative = <A>(arb: Arbitrary.Arbitrary<A>, equals: Equivalence.Equivalence<A>): Registration<Binary<A>> =>
    law({
        name: 'combine is associative',
        arbitraries: { a: arb, b: arb, c: arb },
        predicate: (combine, { a, b, c }) => Effect.sync(() => equals(combine(combine(a, b), c), combine(a, combine(b, c)))),
    });

const commutative = <A>(arb: Arbitrary.Arbitrary<A>, equals: Equivalence.Equivalence<A>): Registration<Binary<A>> =>
    law({ name: 'combine is commutative', arbitraries: { a: arb, b: arb }, predicate: (combine, { a, b }) => Effect.sync(() => equals(combine(a, b), combine(b, a))) });

const idempotent = <A>(arb: Arbitrary.Arbitrary<A>, equals: Equivalence.Equivalence<A>): Registration<Binary<A>> =>
    law({ name: 'combine is idempotent', arbitraries: { a: arb }, predicate: (combine, { a }) => Effect.sync(() => equals(combine(a, a), a)) });

const identity = <A>(arb: Arbitrary.Arbitrary<A>, equals: Equivalence.Equivalence<A>, empty: A): Registration<Binary<A>> =>
    law({ name: 'empty is the identity', arbitraries: { a: arb }, predicate: (combine, { a }) => Effect.sync(() => equals(combine(empty, a), a) && equals(combine(a, empty), a)) });

const equivalence = <A>(arb: Arbitrary.Arbitrary<A>): Registration<Equivalence.Equivalence<A>> =>
    law({
        name: 'equivalence is reflexive, symmetric, and transitive',
        arbitraries: { a: arb, b: arb, c: arb },
        predicate: (equals, { a, b, c }) => Effect.sync(() => pipe(equals(a, b), (related) => equals(a, a) && related === equals(b, a) && (!(related && equals(b, c)) || equals(a, c)))),
    });

const order = <A>(arb: Arbitrary.Arbitrary<A>): Registration<Order.Order<A>> =>
    law({
        name: 'comparison defines a total order',
        arbitraries: { a: arb, b: arb, c: arb },
        predicate: (compare, { a, b, c }) =>
            Effect.sync(() => pipe(compare(a, b), (ordering) => compare(a, a) === 0 && ordering === -compare(b, a) && (!(ordering <= 0 && compare(b, c) <= 0) || compare(a, c) <= 0))),
    });

const inverse = <A, B>(arb: Arbitrary.Arbitrary<A>, equals: Equivalence.Equivalence<A>): Registration<{ readonly to: (value: A) => B; readonly from: (image: B) => A }> =>
    law({ name: 'decode recovers each encoded value', arbitraries: { a: arb }, predicate: (codec, { a }) => Effect.sync(() => equals(codec.from(codec.to(a)), a)) });

const deterministic = <I, A, E, R>(arb: Arbitrary.Arbitrary<I>, equals: Equivalence.Equivalence<A>): Registration<(input: I) => Effect.Effect<A, E, R>, R> =>
    law({ name: 'operation is deterministic', arbitraries: { input: arb }, predicate: (subject, { input }) => pipe(subject(input), (run) => Effect.zipWith(run, run, equals)) });

const homomorphic = <A, B>(arb: Arbitrary.Arbitrary<A>, combine: Binary<A>, combineImage: Binary<B>, equals: Equivalence.Equivalence<B>): Registration<(value: A) => B> =>
    law({ name: 'map commutes with combine', arbitraries: { a: arb, b: arb }, predicate: (map, { a, b }) => Effect.sync(() => equals(map(combine(a, b)), combineImage(map(a), map(b)))) });

const monotone = <A>(arb: Arbitrary.Arbitrary<A>, compare: Order.Order<A>): Registration<(state: A) => A> =>
    law({ name: 'step is monotone', arbitraries: { a: arb }, predicate: (step, { a }) => Effect.sync(() => compare(a, step(a)) <= 0) });

const total = <I, E, R>(
    arb: Arbitrary.Arbitrary<I>,
    counterexample: Counterexample<(input: I) => Effect.Effect<unknown, E, R>, { readonly input: I }>,
): Registration<(input: I) => Effect.Effect<unknown, E, R>, R> =>
    define({ name: 'operation is total', arbitraries: { input: arb }, predicate: (subject, { input }) => Effect.isSuccess(subject(input)) }, counterexample);

const interleave =
    (counterexample: Counterexample<(schedule: fc.Scheduler) => Promise<boolean>, { readonly schedule: fc.Scheduler }>): Registration<(schedule: fc.Scheduler) => Promise<boolean>> =>
    (it, subject): void => {
        it.effect('holds under every interleaving', () =>
            Effect.tryPromise({ try: () => fc.assert(fc.asyncProperty(fc.scheduler(), subject)), catch: (cause) => PropertyError.falsified({ property: 'holds under every interleaving', cause }) }),
        );
        rejection('holds under every interleaving', (implementation, { schedule }) => Effect.promise(() => implementation(schedule)), counterexample)(it);
    };

const roundtrip = <A, I>(it: Vitest.MethodsNonLive, codec: Schema.Codec<A, I>): void =>
    it.effect.prop('codec round-trips', { value: codec }, ({ value }) =>
        Schema.encodeEffect(codec)(value).pipe(
            Effect.flatMap(Schema.decodeEffect(codec)),
            Effect.map((decoded) => Schema.toEquivalence(codec)(value, decoded)),
        ),
    );

const machine =
    <Model extends object, Real>(commands: readonly fc.Arbitrary<fc.Command<Model, Real>>[]): Registration<() => { readonly model: Model; readonly real: Real }> =>
    (it, setup): void => {
        const property = fc.property(fc.commands([...commands]), (run) => fc.modelRun(setup, run));
        it.effect('system conforms to its model', () => Effect.try({ try: () => fc.assert(property), catch: (cause) => PropertyError.falsified({ property: 'system conforms to its model', cause }) }));
    };

const asyncMachine =
    <Model extends object, Real>(commands: readonly fc.Arbitrary<fc.AsyncCommand<Model, Real>>[]): Registration<() => { readonly model: Model; readonly real: Real }> =>
    (it, setup): void => {
        const property = fc.asyncProperty(fc.commands([...commands]), (run) => fc.asyncModelRun(setup, run));
        it.effect('asynchronous system conforms to its model', () =>
            Effect.tryPromise({ try: () => fc.assert(property), catch: (cause) => PropertyError.falsified({ property: 'asynchronous system conforms to its model', cause }) }),
        );
    };

// --- [EXPORTS] -------------------------------------------------------------------------

export {
    associative,
    asyncMachine,
    type Binary,
    type Counterexample,
    commutative,
    define,
    deterministic,
    equivalence,
    homomorphic,
    idempotent,
    identity,
    interleave,
    inverse,
    type Law,
    law,
    machine,
    monotone,
    order,
    PropertyError,
    type Registration,
    roundtrip,
    total,
    type Values,
};

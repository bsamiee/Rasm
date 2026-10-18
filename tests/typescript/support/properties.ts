import { assert, type Vitest } from '@effect/vitest';
import { Arbitrary, Array, type Combiner, Effect, type Equivalence, flow, type Order, type Reducer, Schema } from 'effect';
import fc from 'fast-check';

// --- [TYPES] ---------------------------------------------------------------------------

type Values<A extends Vitest.Arbitraries> = { readonly [K in keyof A]: Vitest.ArbitraryValue<A[K]> };
type Registration<S, R = never> = (it: Vitest.MethodsNonLive<R>, subject: S) => void;

// --- [MODELS] --------------------------------------------------------------------------

interface Law<S, A extends Vitest.Arbitraries, E, R> {
    readonly name: string;
    readonly arbitraries: A;
    readonly holds: (subject: S, values: Values<A>) => Effect.Effect<boolean, E, R>;
}
interface Counterexample<S, V> {
    readonly label: string;
    readonly implementation: S;
    readonly values: V;
}

// --- [OPERATIONS] ----------------------------------------------------------------------

const rejects = <S, V, E, R>(it: Vitest.MethodsNonLive<R>, name: string, holds: (subject: S, values: V) => Effect.Effect<boolean, E, R>, { label, implementation, values }: Counterexample<S, V>): void =>
    it.effect(`${name} rejects ${label}`, () => Effect.map(holds(implementation, values), (held) => assert.isFalse(held)));

const law =
    <S, const A extends Vitest.Arbitraries, E, R>({ name, arbitraries, holds }: Law<S, A, E, R>): Registration<S, R> =>
    (it, subject) =>
        it.effect.prop(name, arbitraries, (values) => holds(subject, values));

const property =
    <S, const A extends Vitest.Arbitraries, E, R>(spec: Law<S, A, E, R>, counterexample: Counterexample<S, Values<A>>): Registration<S, R> =>
    (it, subject) => {
        law(spec)(it, subject);
        rejects(it, spec.name, spec.holds, counterexample);
    };

const laws = <A>(schema: Schema.Schema<A>) => {
    const equals = Schema.toEquivalence(schema);
    return {
        associative: law({ name: 'combine is associative', arbitraries: { a: schema, b: schema, c: schema }, holds: ({ combine }: Combiner.Combiner<A>, { a, b, c }) => Effect.sync(() => equals(combine(combine(a, b), c), combine(a, combine(b, c)))) }),
        commutative: law({ name: 'combine is commutative', arbitraries: { a: schema, b: schema }, holds: ({ combine }: Combiner.Combiner<A>, { a, b }) => Effect.sync(() => equals(combine(a, b), combine(b, a))) }),
        idempotent: law({ name: 'combine is idempotent', arbitraries: { a: schema }, holds: ({ combine }: Combiner.Combiner<A>, { a }) => Effect.sync(() => equals(combine(a, a), a)) }),
        identity: law({ name: 'initial value is the identity', arbitraries: { a: schema }, holds: ({ combine, initialValue }: Reducer.Reducer<A>, { a }) => Effect.sync(() => equals(combine(initialValue, a), a) && equals(combine(a, initialValue), a)) }),
        equivalence: law({
            name: 'equivalence is reflexive, symmetric, and transitive',
            arbitraries: { a: schema, b: schema, c: schema },
            holds: (relation: Equivalence.Equivalence<A>, { a, b, c }) => Effect.sync(() => relation(a, a) && relation(a, b) === relation(b, a) && (!(relation(a, b) && relation(b, c)) || relation(a, c))),
        }),
        order: law({
            name: 'comparison is a total order',
            arbitraries: { a: schema, b: schema, c: schema },
            holds: (compare: Order.Order<A>, { a, b, c }) => Effect.sync(() => compare(a, a) === 0 && compare(a, b) === -compare(b, a) && (compare(a, b) > 0 || compare(b, c) > 0 || compare(a, c) <= 0)),
        }),
        inverse: <B>(it: Vitest.MethodsNonLive, codec: { readonly to: (a: A) => B; readonly from: (b: B) => A }): void => law({ name: 'from inverts to', arbitraries: { a: schema }, holds: ({ to, from }: typeof codec, { a }) => Effect.sync(() => equals(from(to(a)), a)) })(it, codec),
        monotone: <B>(order: Order.Order<A>, image: Order.Order<B>) => law({ name: 'map preserves order', arbitraries: { a: schema, b: schema }, holds: (map: (a: A) => B, { a, b }) => Effect.sync(() => order(a, b) > 0 || image(map(a), map(b)) <= 0) }),
        homomorphic: <B>(image: Schema.Schema<B>, combiner: Combiner.Combiner<A>, imageCombiner: Combiner.Combiner<B>) =>
            law({ name: 'map commutes with combine', arbitraries: { a: schema, b: schema }, holds: (map: (a: A) => B, { a, b }) => Effect.sync(() => Schema.toEquivalence(image)(map(combiner.combine(a, b)), imageCombiner.combine(map(a), map(b)))) }),
        deterministic: <B, E, R>(output: Schema.Schema<B>) => law({ name: 'operation is deterministic', arbitraries: { a: schema }, holds: (operation: (a: A) => Effect.Effect<B, E, R>, { a }) => Effect.zipWith(operation(a), operation(a), Schema.toEquivalence(output)) }),
        total: <E, R>(counterexample: Counterexample<(a: A) => Effect.Effect<unknown, E, R>, { readonly a: A }>) => property({ name: 'operation is total', arbitraries: { a: schema }, holds: (operation: (a: A) => Effect.Effect<unknown, E, R>, { a }) => Effect.isSuccess(operation(a)) }, counterexample),
        covers:
            <const L extends string>(classify: (a: A) => L | readonly L[], labels: readonly L[], options?: Arbitrary.SampleOptions) =>
            (it: Vitest.MethodsNonLive): void =>
                it.effect('generated values cover every class', () => Effect.map(Arbitrary.sampleEffect(Arbitrary.schema(schema), options), (samples) => assert.deepStrictEqual(Array.difference(labels, samples.flatMap(flow(classify, Array.ensure))), []))),
    };
};

const interleave =
    (counterexample: Counterexample<(schedule: fc.Scheduler) => Promise<boolean>, { readonly schedule: fc.Scheduler }>): Registration<(schedule: fc.Scheduler) => Promise<boolean>> =>
    (it, subject) => {
        it('holds under every interleaving', () => fc.assert(fc.asyncProperty(fc.scheduler(), subject)));
        rejects(it, 'holds under every interleaving', (implementation: typeof subject, { schedule }) => Effect.promise(() => implementation(schedule)), counterexample);
    };

const machine =
    <Model extends object, Real>(commands: readonly fc.Arbitrary<fc.AsyncCommand<Model, Real, boolean>>[]): Registration<() => { readonly model: Model; readonly real: Real }> =>
    (it, setup) =>
        it('system conforms to its model', () => fc.assert(fc.asyncProperty(fc.commands([...commands]), (run) => fc.asyncModelRun(setup, run))));

// --- [EXPORTS] -------------------------------------------------------------------------

export { type Counterexample, interleave, type Law, law, laws, machine, property, type Registration };

import type { Vitest } from '@effect/vitest';
import { Data, Effect, Equal, Exit, Schema } from 'effect';
import * as Arbitrary from 'effect/Arbitrary';
import * as FastCheck from 'effect/FastCheck';
import type * as TestServices from 'effect/TestServices';

// --- [TYPES] -----------------------------------------------------------------------------

declare namespace Law {
    type Arbs = { readonly [K in string]: Schema.Schema.Any | FastCheck.Arbitrary<unknown> };
    type ArgsOf<A extends Arbs> = {
        readonly [K in keyof A]: A[K] extends FastCheck.Arbitrary<infer T> ? T : Schema.Schema.Type<A[K]>;
    };
    type Api<R> = Vitest.Methods<R> | Vitest.MethodsNonLive<R>;
    type Binary<A> = (left: A, right: A) => A;
    type Equals<A> = (self: A, that: A) => boolean;
    interface Shape<S, A extends Arbs, E, R> {
        readonly name: string;
        readonly arbitraries: A;
        readonly predicate: (subject: S, args: ArgsOf<A>) => Effect.Effect<boolean, E, R | TestServices.TestServices>;
        readonly witness: { readonly label: string; readonly foil: S; readonly args: ArgsOf<A> };
    }
    interface Law<S, R = never> {
        readonly name: string;
        readonly register: (api: Api<R>, subject: S) => void;
    }
    interface Stock<S, Args> {
        readonly name?: string;
        readonly witness: { readonly label: string; readonly foil: S; readonly args: Args };
    }
}

// --- [ERRORS] ----------------------------------------------------------------------------

class LawRefuted extends Data.TaggedError('LawRefuted')<{ readonly law: string; readonly args: unknown }> {}

class LawTautology extends Data.TaggedError('LawTautology')<{ readonly law: string; readonly witness: string }> {}

// --- [OPERATIONS] ------------------------------------------------------------------------

const _held = <E, R>(
    verdict: Effect.Effect<boolean, E, R>,
    evidence: { readonly law: string; readonly args: unknown },
): Effect.Effect<void, E | LawRefuted, R> => Effect.flatMap(verdict, (held) => (held ? Effect.void : Effect.fail(new LawRefuted(evidence))));

const _survives = <A, E, R>(work: Effect.Effect<A, E, R>): Effect.Effect<boolean, never, R> =>
    Effect.match(work, { onFailure: () => false, onSuccess: () => true });

const Law = {
    // A witness the predicate survives is a tautology no mutant can violate; auditing it is part of every registration.
    audit: <S, A extends Law.Arbs, E, R>(shape: Law.Shape<S, A, E, R>): Effect.Effect<void, LawTautology, R | TestServices.TestServices> =>
        Effect.flatMap(Effect.exit(shape.predicate(shape.witness.foil, shape.witness.args)), (exit) =>
            Exit.match(exit, {
                onFailure: () => Effect.void,
                onSuccess: (held) => (held ? Effect.fail(new LawTautology({ law: shape.name, witness: shape.witness.label })) : Effect.void),
            }),
        ),
    // The witness is mandatory at construction: a law value cannot exist without its refuting foil.
    make: <S, const A extends Law.Arbs, E = never, R = never>(shape: Law.Shape<S, A, E, R>): Law.Law<S, R> => ({
        name: shape.name,
        register: (api, subject) => {
            api.effect.prop(shape.name, shape.arbitraries, (args) => _held(shape.predicate(subject, args), { law: shape.name, args }));
            api.effect(`${shape.name} refutes ${shape.witness.label}`, () => Law.audit(shape));
        },
    }),
    register: <S, R>(api: Law.Api<R>, subject: S, laws: ReadonlyArray<Law.Law<S, R>>): void => {
        for (const law of laws) {
            law.register(api, subject);
        }
    },

    // --- [STOCK_ROWS]
    // The join-semilattice trio the CRDT merge algebra demands; each row stays witness-mandatory and rides the same audit.
    associative: <A>(
        options: Law.Stock<Law.Binary<A>, { readonly a: A; readonly b: A; readonly c: A }> & {
            readonly arb: FastCheck.Arbitrary<A>;
            readonly equals?: Law.Equals<A>;
        },
    ): Law.Law<Law.Binary<A>> =>
        Law.make({
            name: options.name ?? 'combine is associative',
            arbitraries: { a: options.arb, b: options.arb, c: options.arb },
            predicate: (combine, { a, b, c }) =>
                Effect.sync(() => (options.equals ?? Equal.equals)(combine(combine(a, b), c), combine(a, combine(b, c)))),
            witness: options.witness,
        }),
    commutative: <A>(
        options: Law.Stock<Law.Binary<A>, { readonly a: A; readonly b: A }> & {
            readonly arb: FastCheck.Arbitrary<A>;
            readonly equals?: Law.Equals<A>;
        },
    ): Law.Law<Law.Binary<A>> =>
        Law.make({
            name: options.name ?? 'combine is commutative',
            arbitraries: { a: options.arb, b: options.arb },
            predicate: (combine, { a, b }) => Effect.sync(() => (options.equals ?? Equal.equals)(combine(a, b), combine(b, a))),
            witness: options.witness,
        }),
    idempotent: <A>(
        options: Law.Stock<Law.Binary<A>, { readonly a: A }> & {
            readonly arb: FastCheck.Arbitrary<A>;
            readonly equals?: Law.Equals<A>;
        },
    ): Law.Law<Law.Binary<A>> =>
        Law.make({
            name: options.name ?? 'combine is idempotent',
            arbitraries: { a: options.arb },
            predicate: (combine, { a }) => Effect.sync(() => (options.equals ?? Equal.equals)(combine(a, a), a)),
            witness: options.witness,
        }),
    // Upcast totality: the subject decoder succeeds over the whole generated input space — the event-spine version-fold proof.
    total: <I, E, R>(
        options: Law.Stock<(input: I) => Effect.Effect<unknown, E, R>, { readonly input: I }> & { readonly arb: FastCheck.Arbitrary<I> },
    ): Law.Law<(input: I) => Effect.Effect<unknown, E, R>, R> =>
        Law.make({
            name: options.name ?? 'operation is total',
            arbitraries: { input: options.arb },
            predicate: (subject, { input }) => _survives(subject(input)),
            witness: options.witness,
        }),
    // Schema round-trip: encode then decode reproduces the value under the schema's own equivalence — the wire-boundary proof.
    roundtrip: <A, I>(
        options: Law.Stock<Schema.Schema<A, I, never>, { readonly value: A }> & { readonly schema: Schema.Schema<A, I, never> },
    ): Law.Law<Schema.Schema<A, I, never>> => {
        const alike = Schema.equivalence(options.schema);
        return Law.make({
            name: options.name ?? 'codec round-trips',
            arbitraries: { value: Arbitrary.make(options.schema) },
            predicate: (subject, { value }) =>
                Effect.match(Effect.flatMap(Schema.encode(subject)(value), Schema.decode(subject)), {
                    onFailure: () => false,
                    onSuccess: (back) => alike(value, back),
                }),
            witness: options.witness,
        });
    },
    // Model-based machine law: generated command runs hold the model/system correspondence; a thrown postcondition is the refutation.
    machine: <Model extends object, Real>(
        options: Law.Stock<() => { readonly model: Model; readonly real: Real }, { readonly run: Iterable<FastCheck.Command<Model, Real>> }> & {
            readonly commands: ReadonlyArray<FastCheck.Arbitrary<FastCheck.Command<Model, Real>>>;
        },
    ): Law.Law<() => { readonly model: Model; readonly real: Real }> =>
        Law.make({
            name: options.name ?? 'system honors its model',
            arbitraries: { run: FastCheck.commands([...options.commands]) },
            predicate: (setup, { run }) => _survives(Effect.try(() => FastCheck.modelRun(setup, run))),
            witness: options.witness,
        }),
    // The async twin: the correspondence holds across real await boundaries — the journal/queue model proof over live lanes.
    machineAsync: <Model extends object, Real>(
        options: Law.Stock<() => { readonly model: Model; readonly real: Real }, { readonly run: Iterable<FastCheck.AsyncCommand<Model, Real>> }> & {
            readonly commands: ReadonlyArray<FastCheck.Arbitrary<FastCheck.AsyncCommand<Model, Real>>>;
        },
    ): Law.Law<() => { readonly model: Model; readonly real: Real }> =>
        Law.make({
            name: options.name ?? 'system honors its model across awaits',
            arbitraries: { run: FastCheck.commands([...options.commands]) },
            predicate: (setup, { run }) => _survives(Effect.tryPromise(() => FastCheck.asyncModelRun(setup, run))),
            witness: options.witness,
        }),
    // Interleaving law: the subject holds under every scheduler-driven task ordering; the witness pins one refuting ordering via schedulerFor.
    interleave: (
        options: Law.Stock<(schedule: FastCheck.Scheduler) => Promise<boolean>, { readonly schedule: FastCheck.Scheduler }>,
    ): Law.Law<(schedule: FastCheck.Scheduler) => Promise<boolean>> =>
        Law.make({
            name: options.name ?? 'holds under every interleaving',
            arbitraries: { schedule: FastCheck.scheduler() },
            predicate: (subject, { schedule }) => Effect.promise(() => subject(schedule)),
            witness: options.witness,
        }),
} as const;

// --- [EXPORTS] ---------------------------------------------------------------------------

export { Law, LawRefuted, LawTautology };

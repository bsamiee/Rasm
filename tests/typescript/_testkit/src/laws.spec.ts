import { describe, expect, it } from '@effect/vitest';
import { Effect, Schema } from 'effect';
import * as FastCheck from 'effect/FastCheck';
import { Law, LawTautology } from './laws.ts';

// --- [TYPES] -----------------------------------------------------------------------------

type Combine = (left: number, right: number) => number;
type Tally = { readonly count: number };
type Bumper = { readonly bump: () => number };

// --- [CONSTANTS] -------------------------------------------------------------------------

const _ARGS = { x: FastCheck.integer(), y: FastCheck.integer(), z: FastCheck.integer() } as const;
const _INTS = FastCheck.integer({ min: -1000, max: 1000 });
const _SAME = (left: number, right: number): boolean => left === right;

// Associativity over a foil that subtracts: (1-2)-3 diverges from 1-(2-3), so the witness refutes.
const _associativity = Law.make<Combine, typeof _ARGS>({
    name: 'combine is associative',
    arbitraries: _ARGS,
    predicate: (combine, { x, y, z }) => Effect.succeed(combine(combine(x, y), z) === combine(x, combine(y, z))),
    witness: { label: 'subtraction foil', foil: (left, right) => left - right, args: { x: 1, y: 2, z: 3 } },
});

// --- [MODELS] ----------------------------------------------------------------------------

const _Frame = Schema.Struct({ label: Schema.String, rank: Schema.Int });

// A lossy sibling codec over the same channel: decode truncates the label, so round-trips lose evidence.
const _Clipped = Schema.Struct({
    label: Schema.transform(Schema.String, Schema.String, { strict: true, decode: (raw) => raw.slice(0, 1), encode: (held) => held }),
    rank: Schema.Int,
});

// Model-based command over a monotone counter: the model predicts every observed value.
class Bump implements FastCheck.Command<Tally, Bumper> {
    check(): boolean {
        return true;
    }
    run(model: { count: number }, real: Bumper): void {
        model.count += 1;
        if (real.bump() !== model.count) {
            throw new Error(`drifted from ${model.count}`);
        }
    }
    toString(): string {
        return 'bump';
    }
}

// --- [OPERATIONS] ------------------------------------------------------------------------

const _counter = (step: number) => (): { model: Tally; real: Bumper } => {
    let held = 0;
    return {
        model: { count: 0 },
        real: {
            bump: () => {
                held += step;
                return held;
            },
        },
    };
};

// Interleaving subject: schedule two merges into a cell; a max-merge lands order-free, a last-write register does not.
const _merged =
    (combine: Combine) =>
    async (schedule: FastCheck.Scheduler): Promise<boolean> => {
        let cell = 0;
        const land = (value: number): Promise<void> =>
            schedule.schedule(Promise.resolve(value), `land ${value}`).then((landed) => {
                cell = combine(cell, landed);
            });
        const flights = [land(3), land(7)];
        await schedule.waitAll();
        await Promise.all(flights);
        return cell === combine(combine(0, 3), 7);
    };

describe('law registration', () => {
    Law.register(it, Math.min, [
        _associativity,
        Law.commutative({ arb: _INTS, equals: _SAME, witness: { label: 'subtraction foil', foil: (a, b) => a - b, args: { a: 1, b: 2 } } }),
        Law.associative({ arb: _INTS, equals: _SAME, witness: { label: 'subtraction foil', foil: (a, b) => a - b, args: { a: 1, b: 2, c: 3 } } }),
        Law.idempotent({ arb: _INTS, equals: _SAME, witness: { label: 'addition foil', foil: (a, b) => a + b, args: { a: 1 } } }),
    ]);
});

describe('totality law', () => {
    Law.register(it, (input: number) => Effect.succeed(input), [
        Law.total({
            arb: _INTS,
            witness: {
                label: 'negative-refusing decoder',
                foil: (input: number) => (input < 0 ? Effect.fail('refused' as const) : Effect.succeed(input)),
                args: { input: -1 },
            },
        }),
    ]);
});

describe('roundtrip law', () => {
    Law.register(it, _Frame, [
        Law.roundtrip({
            schema: _Frame,
            witness: {
                label: 'label-clipping codec',
                foil: _Clipped,
                args: { value: { label: '<value-long>', rank: 3 } },
            },
        }),
    ]);
});

describe('machine law', () => {
    Law.register(it, _counter(1), [
        Law.machine({
            commands: [FastCheck.constant(new Bump())],
            witness: { label: 'double-stepping counter', foil: _counter(2), args: { run: [new Bump()] } },
        }),
    ]);
});

describe('interleave law', () => {
    Law.register(it, _merged(Math.max), [
        Law.interleave({
            witness: {
                label: 'last-write register under a reversed ordering',
                foil: _merged((_, right) => right),
                args: { schedule: FastCheck.schedulerFor([2, 1]) },
            },
        }),
    ]);
});

describe('tautology audit', () => {
    it.effect('a genuine witness passes the audit', () =>
        Effect.asVoid(
            Law.audit({
                name: 'combine is associative',
                arbitraries: _ARGS,
                predicate: (combine: Combine, { x, y, z }) => Effect.succeed(combine(combine(x, y), z) === combine(x, combine(y, z))),
                witness: { label: 'subtraction foil', foil: (left, right) => left - right, args: { x: 1, y: 2, z: 3 } },
            }),
        ),
    );

    it.effect('a witness the predicate survives is itself the failure', () =>
        Effect.gen(function* () {
            const fault = yield* Effect.flip(
                Law.audit({
                    name: 'tautological registration',
                    arbitraries: _ARGS,
                    // The foil IS the subject: nothing can refute the predicate, so the registration must fail.
                    predicate: (combine: Combine, { x, y, z }) => Effect.succeed(combine(combine(x, y), z) === combine(x, combine(y, z))),
                    witness: { label: 'inert witness', foil: Math.min, args: { x: 1, y: 2, z: 3 } },
                }),
            );
            expect(fault).toBeInstanceOf(LawTautology);
            expect(fault.law).toBe('tautological registration');
        }),
    );
});

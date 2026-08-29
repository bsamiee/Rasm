import { describe, expect, it } from '@effect/vitest';
import { Effect, Order, Schema } from 'effect';
import * as FastCheck from 'effect/FastCheck';
import { InvalidPropertyCounterexampleError, Property } from './properties.ts';

// --- [TYPES] ---------------------------------------------------------------------------

type Combine = (left: number, right: number) => number;
type Tally = { readonly count: number };
type Bumper = { readonly bump: () => number };
type AsyncBumper = { readonly bump: () => Promise<number> };

// --- [CONSTANTS] -----------------------------------------------------------------------

const _ARGS = {
    x: FastCheck.integer(),
    y: FastCheck.integer(),
    z: FastCheck.integer(),
} as const;
const _INTS = FastCheck.integer({ min: -1000, max: 1000 });
const _SAME = (left: number, right: number): boolean => left === right;

const _associativity = Property.define<Combine, typeof _ARGS>({
    name: 'combine is associative',
    arbitraries: _ARGS,
    predicate: (combine, { x, y, z }) =>
        Effect.succeed(combine(combine(x, y), z) === combine(x, combine(y, z))),
    counterexample: {
        label: 'subtraction counterexample',
        implementation: (left, right) => left - right,
        args: { x: 1, y: 2, z: 3 },
    },
});

// --- [MODELS] --------------------------------------------------------------------------

const _VersionedRecord = Schema.Struct({
    label: Schema.String,
    version: Schema.Int,
});

const _TruncatingLabelSchema = Schema.Struct({
    label: Schema.transform(Schema.String, Schema.String, {
        strict: true,
        decode: (raw) => raw.slice(0, 1),
        encode: (value) => value,
    }),
    version: Schema.Int,
});

class Bump implements FastCheck.Command<Tally, Bumper> {
    check(): boolean {
        return true;
    }
    run(model: { count: number }, real: Bumper): void {
        model.count += 1;
        if (real.bump() !== model.count) {
            throw new Error(`counter value differed from ${model.count}`);
        }
    }
    toString(): string {
        return 'bump';
    }
}

class BumpAsync implements FastCheck.AsyncCommand<Tally, AsyncBumper> {
    check(): boolean {
        return true;
    }
    async run(model: { count: number }, real: AsyncBumper): Promise<void> {
        model.count += 1;
        if ((await real.bump()) !== model.count) {
            throw new Error(`counter value differed from ${model.count}`);
        }
    }
    toString(): string {
        return 'bump';
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

const _counter = (step: number) => (): { model: Tally; real: Bumper } => {
    let current = 0;
    return {
        model: { count: 0 },
        real: {
            bump: () => {
                current += step;
                return current;
            },
        },
    };
};

const _asyncCounter =
    (step: number) => (): { model: Tally; real: AsyncBumper } => {
        let current = 0;
        return {
            model: { count: 0 },
            real: {
                bump: () => {
                    current += step;
                    return Promise.resolve(current);
                },
            },
        };
    };

const _scheduledCombination =
    (combine: Combine) =>
    async (schedule: FastCheck.Scheduler): Promise<boolean> => {
        let result = 0;
        const scheduleValue = (value: number): Promise<void> =>
            schedule
                .schedule(Promise.resolve(value), `combine ${value}`)
                .then((scheduledValue) => {
                    result = combine(result, scheduledValue);
                });
        const tasks = [scheduleValue(3), scheduleValue(7)];
        await schedule.waitAll();
        await Promise.all(tasks);
        return result === combine(combine(0, 3), 7);
    };

describe('property registration', () => {
    Property.register(it, Math.min, [
        _associativity,
        Property.commutative({
            arb: _INTS,
            equals: _SAME,
            counterexample: {
                label: 'subtraction counterexample',
                implementation: (a, b) => a - b,
                args: { a: 1, b: 2 },
            },
        }),
        Property.associative({
            arb: _INTS,
            equals: _SAME,
            counterexample: {
                label: 'subtraction counterexample',
                implementation: (a, b) => a - b,
                args: { a: 1, b: 2, c: 3 },
            },
        }),
        Property.idempotent({
            arb: _INTS,
            equals: _SAME,
            counterexample: {
                label: 'addition counterexample',
                implementation: (a, b) => a + b,
                args: { a: 1 },
            },
        }),
        Property.identity({
            arb: _INTS,
            empty: 1000,
            equals: _SAME,
            counterexample: {
                label: 'subtraction counterexample',
                implementation: (a, b) => a - b,
                args: { a: 1 },
            },
        }),
    ]);
});

describe('equivalence property', () => {
    Property.register(it, (self: number, that: number) => self === that, [
        Property.equivalence({
            arb: _INTS,
            counterexample: {
                label: 'non-transitive tolerance relation',
                implementation: (self, that) => Math.abs(self - that) <= 1,
                args: { a: 0, b: 1, c: 2 },
            },
        }),
    ]);
});

describe('order property', () => {
    Property.register(it, Order.number, [
        Property.order({
            arb: _INTS,
            counterexample: {
                label: 'comparator that rejects equality',
                implementation: (self, that) => (self <= that ? -1 : 1),
                args: { a: 1, b: 1, c: 1 },
            },
        }),
    ]);
});

describe('inverse property', () => {
    Property.register(
        it,
        { to: (value: number) => String(value), from: Number },
        [
            Property.inverse({
                arb: _INTS,
                equals: _SAME,
                counterexample: {
                    label: 'sign-erasing isomorphism',
                    implementation: {
                        to: (value: number) => String(Math.abs(value)),
                        from: Number,
                    },
                    args: { a: -1 },
                },
            }),
        ],
    );
});

describe('deterministic property', () => {
    Property.register(it, (input: number) => Effect.succeed(input * 2), [
        Property.deterministic({
            arb: _INTS,
            equals: _SAME,
            counterexample: {
                label: 'stateful implementation',
                implementation: (() => {
                    let invocationCount = 0;
                    return (input: number) => {
                        invocationCount += 1;
                        return Effect.succeed(input + invocationCount);
                    };
                })(),
                args: { input: 0 },
            },
        }),
    ]);
});

describe('homomorphic property', () => {
    Property.register(it, (value: number) => value * 2, [
        Property.homomorphic({
            arb: _INTS,
            combine: (a, b) => a + b,
            combineImage: (a, b) => a + b,
            equals: _SAME,
            counterexample: {
                label: 'squaring map',
                implementation: (value: number) => value * value,
                args: { a: 1, b: 2 },
            },
        }),
    ]);
});

describe('monotone property', () => {
    Property.register(it, (state: number) => state + 1, [
        Property.monotone({
            arb: _INTS,
            order: Order.number,
            counterexample: {
                label: 'regressing step',
                implementation: (state: number) => state - 1,
                args: { a: 0 },
            },
        }),
    ]);
});

describe('totality property', () => {
    Property.register(it, (input: number) => Effect.succeed(input), [
        Property.total({
            arb: _INTS,
            counterexample: {
                label: 'partial decoder',
                implementation: (input: number) =>
                    input < 0
                        ? Effect.fail('rejected' as const)
                        : Effect.succeed(input),
                args: { input: -1 },
            },
        }),
    ]);
});

describe('roundtrip property', () => {
    Property.register(it, _VersionedRecord, [
        Property.roundtrip({
            schema: _VersionedRecord,
            counterexample: {
                label: 'label-clipping codec',
                implementation: _TruncatingLabelSchema,
                args: { value: { label: '<value-long>', version: 3 } },
            },
        }),
    ]);
});

describe('machine property', () => {
    Property.register(it, _counter(1), [
        Property.machine({
            commands: [FastCheck.constant(new Bump())],
            counterexample: {
                label: 'double-stepping counter',
                implementation: _counter(2),
                args: { run: [new Bump()] },
            },
        }),
    ]);
});

describe('async machine property', () => {
    Property.register(it, _asyncCounter(1), [
        Property.machineAsync({
            commands: [FastCheck.constant(new BumpAsync())],
            counterexample: {
                label: 'double-stepping async counter',
                implementation: _asyncCounter(2),
                args: { run: [new BumpAsync()] },
            },
        }),
    ]);
});

describe('interleave property', () => {
    Property.register(it, _scheduledCombination(Math.max), [
        Property.interleave({
            counterexample: {
                label: 'last-write register under a reversed ordering',
                implementation: _scheduledCombination((_, right) => right),
                args: { schedule: FastCheck.schedulerFor([2, 1]) },
            },
        }),
    ]);
});

describe('counterexample verification', () => {
    it.effect('a known-invalid implementation passes verification', () =>
        Effect.asVoid(
            Property.verifyCounterexample({
                name: 'combine is associative',
                arbitraries: _ARGS,
                predicate: (combine: Combine, { x, y, z }) =>
                    Effect.succeed(
                        combine(combine(x, y), z) === combine(x, combine(y, z)),
                    ),
                counterexample: {
                    label: 'subtraction counterexample',
                    implementation: (left, right) => left - right,
                    args: { x: 1, y: 2, z: 3 },
                },
            }),
        ),
    );

    it.effect(
        'an implementation satisfying the property is rejected as a counterexample',
        () =>
            Effect.gen(function* () {
                const error = yield* Effect.flip(
                    Property.verifyCounterexample({
                        name: 'invalid counterexample registration',
                        arbitraries: _ARGS,
                        predicate: (combine: Combine, { x, y, z }) =>
                            Effect.succeed(
                                combine(combine(x, y), z) ===
                                    combine(x, combine(y, z)),
                            ),
                        counterexample: {
                            label: 'valid implementation',
                            implementation: Math.min,
                            args: { x: 1, y: 2, z: 3 },
                        },
                    }),
                );
                expect(error).toBeInstanceOf(
                    InvalidPropertyCounterexampleError,
                );
                expect(error.property).toBe(
                    'invalid counterexample registration',
                );
            }),
    );
});

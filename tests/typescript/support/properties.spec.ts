import { describe, expect, it } from '@effect/vitest';
import { Effect, FastCheck, Order, Schema } from 'effect';
import { Property, type PropertyDefinition, PropertyError } from './properties.ts';

// --- [TYPES] ---------------------------------------------------------------------------

type Combine = (left: number, right: number) => number;

interface CounterModel {
    count: number;
}
interface Counter {
    readonly increment: () => number;
}
interface AsyncCounter {
    readonly increment: () => Promise<number>;
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const _ARGS = { x: FastCheck.integer(), y: FastCheck.integer(), z: FastCheck.integer() } as const;
const _INTS = FastCheck.integer({ min: -1000, max: 1000 });
const _EMPTY = 1000;
const _FIRST = 3;
const _SECOND = 7;
const _equal = (left: number, right: number): boolean => left === right;

const _associativity: PropertyDefinition<Combine, typeof _ARGS, never, never> = {
    name: 'combine is associative',
    arbitraries: _ARGS,
    predicate: (combine, { x, y, z }) => Effect.succeed(combine(combine(x, y), z) === combine(x, combine(y, z))),
    counterexample: { label: 'subtraction counterexample', implementation: (left, right) => left - right, args: { x: 1, y: 2, z: _FIRST } },
};

// --- [MODELS] --------------------------------------------------------------------------

const _VersionedRecord = Schema.Struct({ label: Schema.String, version: Schema.Int });

const _TruncatingLabelSchema = Schema.Struct({
    label: Schema.transform(Schema.String, Schema.String, { strict: true, decode: (raw) => raw.slice(0, 1), encode: (value) => value }),
    version: Schema.Int,
});

const _increment: FastCheck.Command<CounterModel, Counter> = {
    check: () => true,
    run: (model, real) => {
        model.count += 1;
        if (real.increment() !== model.count) {
            throw new Error(`counter value differed from ${model.count}`);
        }
    },
    toString: () => 'increment',
};

const _incrementAsync: FastCheck.AsyncCommand<CounterModel, AsyncCounter> = {
    check: () => true,
    run: async (model, real) => {
        model.count += 1;
        if ((await real.increment()) !== model.count) {
            throw new Error(`counter value differed from ${model.count}`);
        }
    },
    toString: () => 'increment',
};

// --- [OPERATIONS] ----------------------------------------------------------------------

const _counter = (step: number) => (): { model: CounterModel; real: Counter } => {
    let current = 0;
    return {
        model: { count: 0 },
        real: {
            increment: (): number => {
                current += step;
                return current;
            },
        },
    };
};

const _asyncCounter = (step: number) => (): { model: CounterModel; real: AsyncCounter } => {
    let current = 0;
    return {
        model: { count: 0 },
        real: {
            increment: (): Promise<number> => {
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
            schedule.schedule(Promise.resolve(value), `combine ${value}`).then((scheduledValue) => {
                result = combine(result, scheduledValue);
            });
        const tasks = [scheduleValue(_FIRST), scheduleValue(_SECOND)];
        await schedule.waitAll();
        await Promise.all(tasks);
        return result === combine(combine(0, _FIRST), _SECOND);
    };

describe('property registration', () => {
    Property.register(it, Math.min, [
        Property.define(_associativity),
        Property.commutative({
            arb: _INTS,
            equals: _equal,
            counterexample: { label: 'subtraction counterexample', implementation: (a, b) => a - b, args: { a: 1, b: 2 } },
        }),
        Property.associative({
            arb: _INTS,
            equals: _equal,
            counterexample: { label: 'subtraction counterexample', implementation: (a, b) => a - b, args: { a: 1, b: 2, c: _FIRST } },
        }),
        Property.idempotent({
            arb: _INTS,
            equals: _equal,
            counterexample: { label: 'addition counterexample', implementation: (a, b) => a + b, args: { a: 1 } },
        }),
        Property.identity({
            arb: _INTS,
            empty: _EMPTY,
            equals: _equal,
            counterexample: { label: 'subtraction counterexample', implementation: (a, b) => a - b, args: { a: 1 } },
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
                implementation: (self, that) => Order.number(self, that) || 1,
                args: { a: 1, b: 1, c: 1 },
            },
        }),
    ]);
});

describe('inverse property', () => {
    Property.register(it, { to: (value: number) => String(value), from: Number }, [
        Property.inverse({
            arb: _INTS,
            equals: _equal,
            counterexample: {
                label: 'sign-erasing isomorphism',
                implementation: { to: (value: number) => String(Math.abs(value)), from: Number },
                args: { a: -1 },
            },
        }),
    ]);
});

describe('deterministic property', () => {
    Property.register(it, (input: number) => Effect.succeed(input * 2), [
        Property.deterministic({
            arb: _INTS,
            equals: _equal,
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
            equals: _equal,
            counterexample: { label: 'squaring map', implementation: (value: number) => value * value, args: { a: 1, b: 2 } },
        }),
    ]);
});

describe('monotone property', () => {
    Property.register(it, (state: number) => state + 1, [
        Property.monotone({
            arb: _INTS,
            order: Order.number,
            counterexample: { label: 'regressing step', implementation: (state: number) => state - 1, args: { a: 0 } },
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
                    Effect.filterOrFail(
                        Effect.succeed(input),
                        (value) => value >= 0,
                        () => 'rejected' as const,
                    ),
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
                args: { value: { label: '<value-long>', version: _FIRST } },
            },
        }),
    ]);
});

describe('machine property', () => {
    Property.register(it, _counter(1), [
        Property.machine({
            commands: [FastCheck.constant(_increment)],
            counterexample: { label: 'double-stepping counter', implementation: _counter(2), args: { run: [_increment] } },
        }),
    ]);
});

describe('async machine property', () => {
    Property.register(it, _asyncCounter(1), [
        Property.machineAsync({
            commands: [FastCheck.constant(_incrementAsync)],
            counterexample: { label: 'double-stepping async counter', implementation: _asyncCounter(2), args: { run: [_incrementAsync] } },
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
    it.effect('known-invalid implementations pass verification', () => Property.verifyCounterexample(_associativity));

    it.effect('implementations satisfying the property are rejected as counterexamples', () =>
        Effect.gen(function* () {
            const error = yield* Effect.flip(
                Property.verifyCounterexample({
                    name: 'invalid counterexample registration',
                    arbitraries: _ARGS,
                    predicate: (combine: Combine, { x, y, z }) => Effect.succeed(combine(combine(x, y), z) === combine(x, combine(y, z))),
                    counterexample: { label: 'valid implementation', implementation: Math.min, args: { x: 1, y: 2, z: _FIRST } },
                }),
            );
            expect(error).toBeInstanceOf(PropertyError);
            expect(error.reason).toBe('counterexample');
            expect(error.property).toBe('invalid counterexample registration');
        }),
    );
});

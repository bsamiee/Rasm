import { describe, expect, it } from '@effect/vitest';
import { Array, Effect, Exit, Metric, Option } from 'effect';
import { Telemetry } from './telemetry.ts';

// --- [CONSTANTS] -----------------------------------------------------------------------

// Metric identity is registry-global; every spec here mints its own names so no drift row can
// alias another spec's counters.
const _reuse = Metric.counter('kit_spec_token_reuse');
const _taggedReuse = Metric.tagged(Metric.counter('kit_spec_tagged_reuse'), 'tenant', '<tenant-a>');
const _inert = Metric.counter('kit_spec_inert');
const _refusals = Metric.frequency('kit_spec_refusals');

// --- [OPERATIONS] ----------------------------------------------------------------------

const _driftOf = (drift: ReadonlyArray<{ readonly name: string; readonly value: number; readonly before: number }>, name: string) =>
    Array.findFirst(drift, (row) => row.name === name);

describe('metric drift', () => {
    it.effect('a stepped counter lands as one drift row carrying before and after', () =>
        Effect.gen(function* () {
            yield* Metric.increment(_reuse);
            const observed = yield* Telemetry.observed(Effect.zipRight(Metric.increment(_reuse), Metric.increment(_reuse)));
            const row = yield* _driftOf(observed.drift, 'kit_spec_token_reuse');
            expect(row.value - row.before).toBe(2);
            expect(Exit.isSuccess(observed.exit)).toBe(true);
        }),
    );

    it.effect('an untouched metric never enters the drift — the gauge can stay silent', () =>
        Effect.gen(function* () {
            yield* Metric.increment(_inert);
            const observed = yield* Telemetry.observed(Effect.void);
            expect(Option.isNone(_driftOf(observed.drift, 'kit_spec_inert'))).toBe(true);
        }),
    );

    it.effect('a tagged counter keeps its tag rows on the reading', () =>
        Effect.gen(function* () {
            const observed = yield* Telemetry.observed(Metric.increment(_taggedReuse));
            const row = yield* _driftOf(observed.drift, 'kit_spec_tagged_reuse');
            expect(row).toMatchObject({ kind: 'counter', tags: [['tenant', '<tenant-a>']] });
        }),
    );

    it.effect('a frequency fans one reading per occurrence word', () =>
        Effect.gen(function* () {
            const observed = yield* Telemetry.observed(
                Effect.zipRight(Metric.update(_refusals, '<reason-a>'), Metric.update(_refusals, '<reason-a>')),
            );
            const row = yield* Array.findFirst(
                observed.drift,
                (candidate) =>
                    candidate.name === 'kit_spec_refusals' &&
                    Array.some(candidate.tags, ([key, value]) => key === 'occurrence' && value === '<reason-a>'),
            );
            expect(row.value - row.before).toBe(2);
        }),
    );

    it.effect('a failing flow still yields its drift — the failure-path counter is the audit material', () =>
        Effect.gen(function* () {
            const observed = yield* Telemetry.observed(Effect.zipRight(Metric.increment(_reuse), Effect.fail('refused' as const)));
            expect(Exit.isFailure(observed.exit)).toBe(true);
            expect(Option.isSome(_driftOf(observed.drift, 'kit_spec_token_reuse'))).toBe(true);
        }),
    );
});

describe('span capture', () => {
    it.effect('a successful span records its name, attributes, and success outcome', () =>
        Effect.gen(function* () {
            const observed = yield* Telemetry.observed(
                Effect.withSpan(Effect.annotateCurrentSpan('grade', 3), 'mint', { attributes: { lane: '<lane-a>' } }),
            );
            const span = yield* Array.findFirst(observed.spans, (row) => row.name === 'mint');
            expect(span.outcome).toBe('success');
            expect(span.attributes['lane']).toBe('<lane-a>');
            expect(span.attributes['grade']).toBe(3);
        }),
    );

    it.effect('a failing span records the failure outcome — the capture can refute a green claim', () =>
        Effect.gen(function* () {
            const observed = yield* Telemetry.observed(Effect.withSpan(Effect.fail('refused' as const), 'mint'));
            const span = yield* Array.findFirst(observed.spans, (row) => row.name === 'mint');
            expect(span.outcome).toBe('failure');
        }),
    );

    it.effect('a nested span carries its parent name and a span event lands on its own record', () =>
        Effect.gen(function* () {
            const observed = yield* Telemetry.observed(
                Effect.withSpan(
                    Effect.withSpan(
                        Effect.flatMap(Effect.currentSpan, (span) => Effect.sync(() => span.event('marked', 0n, { grade: 7 }))),
                        'inner',
                    ),
                    'outer',
                ),
            );
            const inner = yield* Array.findFirst(observed.spans, (row) => row.name === 'inner');
            expect(inner.parent).toEqual(Option.some('outer'));
            expect(inner.events).toEqual([{ name: 'marked', attributes: { grade: 7 } }]);
        }),
    );
});

describe('board projection', () => {
    it.effect('the board serves the live reading for a registered metric', () =>
        Effect.gen(function* () {
            yield* Metric.increment(_inert);
            const board = yield* Telemetry.board;
            const row = yield* Array.findFirst(board, (reading) => reading.name === 'kit_spec_inert');
            expect(row.kind).toBe('counter');
            expect(row.value).toBeGreaterThan(0);
        }),
    );
});

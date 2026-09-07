import { describe, expect, it } from '@effect/vitest';
import { Array, Effect, Exit, Metric, Option } from 'effect';
import { type MetricChange, Telemetry } from './telemetry.ts';

// --- [CONSTANTS] -----------------------------------------------------------------------

const _GRADE = 3;
const _reuse = Metric.counter('test_support_token_reuse');
const _taggedReuse = Metric.tagged(Metric.counter('test_support_tagged_reuse'), 'tenant', '<tenant-a>');
const _unchanged = Metric.counter('test_support_unchanged');
const _failureReasons = Metric.frequency('test_support_failure_reasons');

// --- [OPERATIONS] ----------------------------------------------------------------------

// The first row under a name, a metric change, a span, or a snapshot reading
const _named = <A extends { readonly name: string }>(rows: readonly A[], name: string): Option.Option<A> =>
    Array.findFirst(rows, (row) => row.name === name);

// The frequency reading of one recorded value, tagged by occurrence
const _occurrence = (changes: readonly MetricChange[], name: string, value: string): Option.Option<MetricChange> =>
    Array.findFirst(changes, (row) => row.name === name && Array.some(row.tags, ([key, tagged]) => key === 'occurrence' && tagged === value));

// One event on the current span, the inner span's body under the nested-span case
const _mark = Effect.flatMap(Effect.currentSpan, (span) => Effect.sync(() => span.event('marked', 0n, { grade: _GRADE })));

describe('metric changes', () => {
    it.effect('changed counters record their previous and current values', () =>
        Effect.gen(function* () {
            yield* Metric.increment(_reuse);
            const capture = yield* Telemetry.capture(Effect.zipRight(Metric.increment(_reuse), Metric.increment(_reuse)));
            const row = yield* _named(capture.metricChanges, 'test_support_token_reuse');
            expect(row.value - row.before).toBe(2);
            expect(Exit.isSuccess(capture.exit)).toBe(true);
        }),
    );

    it.effect('unchanged metrics are absent from the change set', () =>
        Effect.gen(function* () {
            yield* Metric.increment(_unchanged);
            const capture = yield* Telemetry.capture(Effect.void);
            expect(Option.isNone(_named(capture.metricChanges, 'test_support_unchanged'))).toBe(true);
        }),
    );

    it.effect('tagged counters keep their tag rows on the reading', () =>
        Effect.gen(function* () {
            const capture = yield* Telemetry.capture(Metric.increment(_taggedReuse));
            const row = yield* _named(capture.metricChanges, 'test_support_tagged_reuse');
            expect(row).toMatchObject({ kind: 'counter', tags: [['tenant', '<tenant-a>']] });
        }),
    );

    it.effect('frequency metrics emit one reading per recorded value', () =>
        Effect.gen(function* () {
            const capture = yield* Telemetry.capture(
                Effect.zipRight(Metric.update(_failureReasons, '<reason-a>'), Metric.update(_failureReasons, '<reason-a>')),
            );
            const row = yield* _occurrence(capture.metricChanges, 'test_support_failure_reasons', '<reason-a>');
            expect(row.value - row.before).toBe(2);
        }),
    );

    it.effect('failing effects still record metric changes from their failure path', () =>
        Effect.gen(function* () {
            const capture = yield* Telemetry.capture(Effect.zipRight(Metric.increment(_reuse), Effect.fail('rejected' as const)));
            expect(Exit.isFailure(capture.exit)).toBe(true);
            expect(Option.isSome(_named(capture.metricChanges, 'test_support_token_reuse'))).toBe(true);
        }),
    );
});

describe('span capture', () => {
    it.effect('successful spans record their name, attributes, and success outcome', () =>
        Effect.gen(function* () {
            const capture = yield* Telemetry.capture(
                Effect.withSpan(Effect.annotateCurrentSpan('grade', _GRADE), 'operation', { attributes: { operation: '<operation-a>' } }),
            );
            const span = yield* _named(capture.spans, 'operation');
            expect(span.outcome).toBe('success');
            expect(span.attributes['operation']).toBe('<operation-a>');
            expect(span.attributes['grade']).toBe(_GRADE);
        }),
    );

    it.effect('failing spans record the failure outcome', () =>
        Effect.gen(function* () {
            const capture = yield* Telemetry.capture(Effect.withSpan(Effect.fail('rejected' as const), 'operation'));
            const span = yield* _named(capture.spans, 'operation');
            expect(span.outcome).toBe('failure');
        }),
    );

    it.effect('nested spans record their parent name and each span event as a separate record', () =>
        Effect.gen(function* () {
            const capture = yield* Telemetry.capture(_mark.pipe(Effect.withSpan('inner'), Effect.withSpan('outer')));
            const inner = yield* _named(capture.spans, 'inner');
            expect(inner.parent).toEqual(Option.some('outer'));
            expect(inner.events).toEqual([{ name: 'marked', attributes: { grade: _GRADE } }]);
        }),
    );
});

describe('metric snapshot', () => {
    it.effect('the snapshot includes the current reading for a registered metric', () =>
        Effect.gen(function* () {
            yield* Metric.increment(_unchanged);
            const snapshot = yield* Telemetry.snapshot;
            const row = yield* _named(snapshot, 'test_support_unchanged');
            expect(row.kind).toBe('counter');
            expect(row.value).toBeGreaterThan(0);
        }),
    );
});

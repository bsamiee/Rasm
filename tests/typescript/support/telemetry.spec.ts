import { describe, expect, it } from '@effect/vitest';
import { Array, Effect, Exit, Metric, Option } from 'effect';
import { Telemetry } from './telemetry.ts';

// --- [CONSTANTS] -----------------------------------------------------------------------

const _reuse = Metric.counter('test_support_token_reuse');
const _taggedReuse = Metric.tagged(
    Metric.counter('test_support_tagged_reuse'),
    'tenant',
    '<tenant-a>',
);
const _inert = Metric.counter('test_support_inert');
const _failureReasons = Metric.frequency('test_support_failure_reasons');

// --- [OPERATIONS] ----------------------------------------------------------------------

const _metricChange = (
    changes: ReadonlyArray<{
        readonly name: string;
        readonly value: number;
        readonly before: number;
    }>,
    name: string,
) => Array.findFirst(changes, (row) => row.name === name);

describe('metric changes', () => {
    it.effect('changed counters record their previous and current values', () =>
        Effect.gen(function* () {
            yield* Metric.increment(_reuse);
            const capture = yield* Telemetry.capture(
                Effect.zipRight(
                    Metric.increment(_reuse),
                    Metric.increment(_reuse),
                ),
            );
            const row = yield* _metricChange(
                capture.metricChanges,
                'test_support_token_reuse',
            );
            expect(row.value - row.before).toBe(2);
            expect(Exit.isSuccess(capture.exit)).toBe(true);
        }),
    );

    it.effect('unchanged metrics are absent from the change set', () =>
        Effect.gen(function* () {
            yield* Metric.increment(_inert);
            const capture = yield* Telemetry.capture(Effect.void);
            expect(
                Option.isNone(
                    _metricChange(capture.metricChanges, 'test_support_inert'),
                ),
            ).toBe(true);
        }),
    );

    it.effect('tagged counters keep their tag rows on the reading', () =>
        Effect.gen(function* () {
            const capture = yield* Telemetry.capture(
                Metric.increment(_taggedReuse),
            );
            const row = yield* _metricChange(
                capture.metricChanges,
                'test_support_tagged_reuse',
            );
            expect(row).toMatchObject({
                kind: 'counter',
                tags: [['tenant', '<tenant-a>']],
            });
        }),
    );

    it.effect('frequency metrics emit one reading per recorded value', () =>
        Effect.gen(function* () {
            const capture = yield* Telemetry.capture(
                Effect.zipRight(
                    Metric.update(_failureReasons, '<reason-a>'),
                    Metric.update(_failureReasons, '<reason-a>'),
                ),
            );
            const row = yield* Array.findFirst(
                capture.metricChanges,
                (candidate) =>
                    candidate.name === 'test_support_failure_reasons' &&
                    Array.some(
                        candidate.tags,
                        ([key, value]) =>
                            key === 'occurrence' && value === '<reason-a>',
                    ),
            );
            expect(row.value - row.before).toBe(2);
        }),
    );

    it.effect(
        'failing effects still record metric changes from their failure path',
        () =>
            Effect.gen(function* () {
                const capture = yield* Telemetry.capture(
                    Effect.zipRight(
                        Metric.increment(_reuse),
                        Effect.fail('rejected' as const),
                    ),
                );
                expect(Exit.isFailure(capture.exit)).toBe(true);
                expect(
                    Option.isSome(
                        _metricChange(
                            capture.metricChanges,
                            'test_support_token_reuse',
                        ),
                    ),
                ).toBe(true);
            }),
    );
});

describe('span capture', () => {
    it.effect(
        'successful spans record their name, attributes, and success outcome',
        () =>
            Effect.gen(function* () {
                const capture = yield* Telemetry.capture(
                    Effect.withSpan(
                        Effect.annotateCurrentSpan('grade', 3),
                        'operation',
                        { attributes: { operation: '<operation-a>' } },
                    ),
                );
                const span = yield* Array.findFirst(
                    capture.spans,
                    (row) => row.name === 'operation',
                );
                expect(span.outcome).toBe('success');
                expect(span.attributes['operation']).toBe('<operation-a>');
                expect(span.attributes['grade']).toBe(3);
            }),
    );

    it.effect('failing spans record the failure outcome', () =>
        Effect.gen(function* () {
            const capture = yield* Telemetry.capture(
                Effect.withSpan(Effect.fail('rejected' as const), 'operation'),
            );
            const span = yield* Array.findFirst(
                capture.spans,
                (row) => row.name === 'operation',
            );
            expect(span.outcome).toBe('failure');
        }),
    );

    it.effect(
        'nested spans record their parent name and each span event as a separate record',
        () =>
            Effect.gen(function* () {
                const capture = yield* Telemetry.capture(
                    Effect.withSpan(
                        Effect.withSpan(
                            Effect.flatMap(Effect.currentSpan, (span) =>
                                Effect.sync(() =>
                                    span.event('marked', 0n, { grade: 7 }),
                                ),
                            ),
                            'inner',
                        ),
                        'outer',
                    ),
                );
                const inner = yield* Array.findFirst(
                    capture.spans,
                    (row) => row.name === 'inner',
                );
                expect(inner.parent).toEqual(Option.some('outer'));
                expect(inner.events).toEqual([
                    { name: 'marked', attributes: { grade: 7 } },
                ]);
            }),
    );
});

describe('metric snapshot', () => {
    it.effect(
        'the snapshot includes the current reading for a registered metric',
        () =>
            Effect.gen(function* () {
                yield* Metric.increment(_inert);
                const snapshot = yield* Telemetry.snapshot;
                const row = yield* Array.findFirst(
                    snapshot,
                    (reading) => reading.name === 'test_support_inert',
                );
                expect(row.kind).toBe('counter');
                expect(row.value).toBeGreaterThan(0);
            }),
    );
});

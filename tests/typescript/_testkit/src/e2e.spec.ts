import { describe, expect, it } from '@effect/vitest';
import { Array, Effect, Option, Schema } from 'effect';
import { Hermetic, K6, K6Fault } from './e2e.ts';

// --- [CONSTANTS] -------------------------------------------------------------------------

// Golden summary sample carrying BOTH gate spellings the export has shipped; the ungated metric stays out of `gated`.
const _SUMMARY = JSON.stringify({
    metrics: {
        iteration_duration: { avg: 4.2 },
        probe_ms: { thresholds: { 'p(95)<1000': { ok: true } } },
        vus: { thresholds: { 'value<3': true } },
    },
});

// --- [OPERATIONS] ------------------------------------------------------------------------

const _decoded = Schema.decode(Schema.parseJson(K6.Summary), { errors: 'all' });

describe('hermetic corpus', () => {
    it.effect('every route projects a full secure-context document', () =>
        Effect.sync(() => {
            const documents = Array.getSomes(Array.map(Hermetic.routes, Hermetic.page));
            expect(documents).toHaveLength(Hermetic.routes.length);
            expect(Array.every(documents, (doc) => doc.startsWith('<!doctype html>'))).toBe(true);
            expect(Hermetic.origin.startsWith('https://')).toBe(true);
        }),
    );

    it.effect('a phantom route is typed absence, never a fabricated page', () =>
        Effect.sync(() => {
            expect(Option.isNone(Hermetic.page('/phantom'))).toBe(true);
        }),
    );
});

describe('k6 summary receipt', () => {
    it.effect('both gate spellings decode and gated metrics are exactly the thresholded ones', () =>
        Effect.gen(function* () {
            const summary = yield* _decoded(_SUMMARY);
            expect([...summary.gated].sort()).toEqual(['probe_ms', 'vus']);
        }),
    );

    it.effect('a malformed summary refutes the decode at the seam', () =>
        Effect.gen(function* () {
            const fault = yield* Effect.flip(_decoded('{"metrics":{"probe_ms":{"thresholds":{"p(95)<1000":"yes"}}}}'));
            expect(fault._tag).toBe('ParseError');
        }),
    );

    it.effect('a breached verdict and a passed verdict are distinct arms of one family', () =>
        Effect.gen(function* () {
            const summary = yield* _decoded(_SUMMARY);
            expect(K6.Verdict.$is('Breached')(K6.Verdict.Breached({ summary }))).toBe(true);
            expect(K6.Verdict.$is('Breached')(K6.Verdict.Passed({ summary }))).toBe(false);
            expect(new K6Fault({ reason: 'crashed', detail: 'exit 1' })._tag).toBe('K6Fault');
        }),
    );
});

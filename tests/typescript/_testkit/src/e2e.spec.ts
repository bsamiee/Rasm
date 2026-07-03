import { FileSystem, Path } from '@effect/platform';
import { NodeContext } from '@effect/platform-node';
import { describe, expect, it, layer } from '@effect/vitest';
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

// A k6 stand-in binary: receives the exact argv the runner composes, honors --summary-export, exits with the scripted code —
// so the whole subprocess verdict fold is falsifiable with no k6 on PATH.
const _standIn = (body: string) =>
    Effect.gen(function* () {
        const fs = yield* FileSystem.FileSystem;
        const path = yield* Path.Path;
        const scratch = yield* fs.makeTempDirectoryScoped();
        const binary = path.join(scratch, 'k6-stand-in');
        yield* fs.writeFileString(binary, `#!/bin/sh\n${body}\n`);
        yield* fs.chmod(binary, 0o755);
        return { binary, summary: path.join(scratch, 'summary.json') };
    });

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

layer(NodeContext.layer)('k6 subprocess contract', (it) => {
    it.effect('a threshold-breach exit with a written summary folds to the Breached arm', () =>
        Effect.scoped(
            Effect.gen(function* () {
                const lane = yield* _standIn(
                    `for arg in "$@"; do case "$arg" in --summary-export=*) printf '%s' '${_SUMMARY}' > "\${arg#--summary-export=}";; esac; done\nexit 99`,
                );
                const verdict = yield* K6.run({ script: '/dev/null', summary: lane.summary, binary: lane.binary });
                expect(K6.Verdict.$is('Breached')(verdict)).toBe(true);
            }),
        ),
    );

    it.effect('a crash exit reports the crash, never a masking summary-read fault over the file the crash prevented', () =>
        Effect.scoped(
            Effect.gen(function* () {
                const lane = yield* _standIn('exit 7');
                const fault = yield* Effect.flip(K6.run({ script: '/dev/null', summary: lane.summary, binary: lane.binary }));
                expect(fault).toBeInstanceOf(K6Fault);
                expect(fault.reason).toBe('crashed');
                expect(fault.detail).toBe('exit 7');
            }),
        ),
    );

    it.effect('a clean exit whose summary never landed is a typed summary fault', () =>
        Effect.scoped(
            Effect.gen(function* () {
                const lane = yield* _standIn('exit 0');
                const fault = yield* Effect.flip(K6.run({ script: '/dev/null', summary: lane.summary, binary: lane.binary }));
                expect(fault.reason).toBe('summary');
            }),
        ),
    );
});

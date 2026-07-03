import { fileURLToPath } from 'node:url';
import type { CommandExecutor, FileSystem } from '@effect/platform';
import { NodeContext } from '@effect/platform-node';
import { K6 } from '@rasm/ts-testkit/e2e';
import { Effect } from 'effect';
import { expect, test } from '../fixtures.ts';

// --- [OPERATIONS] ------------------------------------------------------------------------

// The playwright body is the platform-forced promise seam; the k6 driver stays on the rail up to it.
const _run = <A, E>(program: Effect.Effect<A, E, CommandExecutor.CommandExecutor | FileSystem.FileSystem>): Promise<A> =>
    Effect.runPromise(Effect.provide(program, NodeContext.layer));

test.describe('k6 load lane', () => {
    test.beforeEach(async () => {
        test.skip(!(await _run(K6.locate)), 'activation: a k6 binary on PATH');
    });

    test('the pass profile clears its thresholds end to end', async () => {
        const verdict = await _run(
            K6.run({ script: fileURLToPath(new URL('./probe.k6.ts', import.meta.url)), summary: test.info().outputPath('probe-summary.json') }),
        );
        expect(K6.Verdict.$is('Passed')(verdict)).toBe(true);
        expect(K6.Verdict.$match(verdict, { Breached: ({ summary }) => summary.gated, Passed: ({ summary }) => summary.gated })).toContain(
            'probe_ms',
        );
    });

    test('the seeded breach profile is refused with the threshold exit', async () => {
        const verdict = await _run(
            K6.run({ script: fileURLToPath(new URL('./breach.k6.ts', import.meta.url)), summary: test.info().outputPath('breach-summary.json') }),
        );
        expect(K6.Verdict.$is('Breached')(verdict)).toBe(true);
    });
});

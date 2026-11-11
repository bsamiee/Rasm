import { assert } from '@effect/vitest';
import { called, nx } from '@rasm/testing/nx/cli';
import { Effect, Schema } from 'effect';
import type { Subject } from '#/inference/cases';
import { type Methods, placed, present, SLOW } from '#/inference/workspaces';

// --- [MODELS] --------------------------------------------------------------------------

const Json = Schema.fromJsonString(Schema.Unknown);

// --- [OPERATIONS] ----------------------------------------------------------------------

const readiness = (subject: Subject, it: Methods) =>
    it.effect.skipIf(!present(subject) || subject.served === undefined)(
        'runs a task waiting on its continuous target only once the target answers, and stops the target when the run ends',
        Effect.fnUntraced(function* () {
            const [project, target, address] = yield* Effect.fromNullishOr(subject.served);
            const probe = 'waiter/probe.mjs';
            const waiter = yield* Schema.encodeUnknownEffect(Json)({ name: 'waiter', targets: { probe: { command: `node ${probe}`, dependsOn: [{ projects: [project], target, waitFor: 'ready' }] } } });
            const root = yield* placed(subject, { 'waiter/project.json': waiter, [probe]: `process.exitCode = await fetch('${address}').then((reply) => (reply.ok ? 0 : 1), () => 1);\n` });
            const { code, output } = yield* nx(root, ['run', 'waiter:probe', '--output-style=static']);
            const after = yield* called(root, 'node', [probe], {});
            assert.deepStrictEqual([code, after.code], [0, 1], `naxodev/oss@8b43d4a packages/nx-cloudflare-e2e/src/readiness.spec.ts, packages/gonx-e2e/src/readiness.spec.ts, nrwl/nx@2a424d3 e2e/nx/src/run.test.ts continuousServe, packages/nx/src/tasks-runner/readiness/*.spec.ts\n${output}`);
        }),
        SLOW,
    );

// --- [EXPORTS] -------------------------------------------------------------------------

export { readiness };

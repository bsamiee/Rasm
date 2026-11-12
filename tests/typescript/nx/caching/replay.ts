import { assert } from '@effect/vitest';
import { ran } from '@rasm/testing/nx/cli';
import { Effect, Order } from 'effect';
import type { Subject } from '#/inference/cases';
import { type Methods, prepared, present, SLOW } from '#/inference/workspaces';

// --- [OPERATIONS] ----------------------------------------------------------------------

const replay = (subject: Subject, it: Methods) =>
    it.effect.skipIf(!present(subject) || subject.runs === undefined)(
        'reads every cacheable task from the cache on a second run of an unchanged workspace, under the hash of the first run',
        Effect.fnUntraced(function* () {
            const targets = yield* Effect.fromNullishOr(subject.runs);
            const { root, cached } = yield* prepared(subject);
            const run = Effect.map(ran(root, ['run-many', '-t', targets.join(',')]), ({ code, output, tasks }) => ({ code, output, tasks: tasks.filter(({ projectName, target }) => cached(projectName, target)).toSorted(Order.mapInput(Order.String, ({ taskId }) => taskId)) }));
            const first = yield* run;
            const second = yield* run;
            assert.deepStrictEqual([first.code, second.code, first.tasks.length > 0], [0, 0, true], `${first.output}${second.output}`);
            assert.deepStrictEqual(
                second.tasks.map(({ taskId, hash, cacheStatus }) => ({ taskId, hash, cacheStatus })),
                first.tasks.map(({ taskId, hash }) => ({ taskId, hash, cacheStatus: 'local-cache-hit' })),
                'nrwl/nx@2a424d3 astro-docs features/cache-task-results.mdoc, gradle/develocity-build-validation-scripts@f5a59c5 01-validate-incremental-building.sh, onepipeline-ui@2ea90f8 nx-baseline-cache.test.mjs',
            );
        }),
        SLOW,
    );

// --- [EXPORTS] -------------------------------------------------------------------------

export { replay };

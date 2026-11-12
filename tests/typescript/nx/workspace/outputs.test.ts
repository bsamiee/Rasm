import { layer } from '@effect/platform-node/NodeServices';
import { assert, it } from '@effect/vitest';
import { Array, Cause, Effect, Layer, Result } from 'effect';
import { validateOutputs } from 'nx/src/devkit-internals';
import { getTaskOutputs } from 'nx/src/hasher/check-task-files';
import { Composed, SLOW } from '#/workspace/graph';

// --- [OPERATIONS] ----------------------------------------------------------------------

const within = (outer: string, inner: string) => inner === outer || inner.startsWith(`${outer}/`);

const outputs = (task: string, seed: Parameters<typeof getTaskOutputs>[1]) =>
    Effect.map(
        Effect.promise(() => getTaskOutputs(task, seed)),
        ({ resolved, unresolved }) => ({ written: resolved.map((output) => ({ task, output })), dropped: unresolved.map((output) => `${task} ${output}`) }),
    );

// --- [COMPOSITION] ---------------------------------------------------------------------

it.layer(Layer.merge(layer, Composed.layer), { excludeTestServices: true, timeout: SLOW.timeout })('outputs', (methods) => {
    methods.effect(
        'holds only outputs Nx validates: each starts with a workspace or project token and none is a glob at the workspace root',
        Effect.fnUntraced(function* () {
            const { targets } = yield* Composed;
            const [, refused] = yield* Effect.partition(targets, ({ task, configuration }) => Effect.try({ try: () => validateOutputs(configuration.outputs ?? []), catch: (cause) => new Cause.UnknownError(cause, task) }));
            assert.deepStrictEqual(refused, [], 'nrwl/nx@2a424d3 packages/nx/src/native/cache/validate_outputs.rs');
        }),
        SLOW,
    );

    methods.effect(
        'resolves every output token of every target and configuration, so Nx drops no output',
        Effect.fnUntraced(function* () {
            const { targets, seed } = yield* Composed;
            const read = yield* Effect.forEach(
                targets.flatMap(({ tasks }) => tasks),
                (task) => outputs(task, seed),
            );
            assert.deepStrictEqual(
                read.flatMap(({ dropped }) => dropped),
                [],
                'nrwl/nx@2a424d3 packages/nx/src/hasher/check-task-files.ts TaskOutputs.unresolved, check-task-files.spec.ts',
            );
        }),
        SLOW,
    );

    methods.effect(
        'gives no two tasks an output path inside the other',
        Effect.fnUntraced(function* () {
            const { targets, seed } = yield* Composed;
            const read = yield* Effect.forEach(targets, ({ task }) => outputs(task, seed));
            const written = read.flatMap((held) => held.written);
            const shared = Array.filterMap(Array.cartesian(written, written), ([left, right]) => (left.task < right.task && (within(left.output, right.output) || within(right.output, left.output)) ? Result.succeed(`${left.task} ${left.output} and ${right.task} ${right.output}`) : Result.failVoid));
            assert.deepStrictEqual(shared, [], 'nrwl/nx@2a424d3 astro-docs kb/configure-outputs.mdoc:108, reference/project-configuration.mdoc:328, packages/nx/src/native/affected/tasks/dependent_outputs.rs, NexusDI/core@025f234 tools/repo-checks/src/tsc-emit-outside-build-output.test.ts');
        }),
        SLOW,
    );
});

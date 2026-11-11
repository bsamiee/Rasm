import { assert } from '@effect/vitest';
import { hashed } from '@rasm/testing/nx/cli';
import { Array, Effect, Record } from 'effect';
import type { Subject } from '#/inference/cases';
import { amended, type Methods, prepared, present, SLOW } from '#/inference/workspaces';

// --- [OPERATIONS] ----------------------------------------------------------------------

const dependentOutputs = (subject: Subject, it: Methods) =>
    it.effect.skipIf(!present(subject))(
        'gives a new hash to exactly the cacheable tasks Nx hashes dependency outputs into when a file in every declared output folder changes',
        Effect.fnUntraced(function* () {
            const { root, folders } = yield* prepared(subject);
            const probes = folders.map((folder) => `${folder}/probe.txt`);
            const probed = (text: string) =>
                Effect.andThen(
                    Effect.forEach(probes, (file) => amended(root, file, text), { discard: true }),
                    hashed(root),
                );
            const before = yield* probed('probe\n');
            const after = yield* probed('probe changed\n');
            assert.deepStrictEqual(
                Record.keys(Record.filter(before, ({ cache, hash }, task) => cache && hash !== after[task]?.hash)),
                Record.keys(Record.filter(before, ({ cache, depOutputs }) => cache && Array.isReadonlyArrayNonEmpty(depOutputs))),
                'nrwl/nx@2a424d3 packages/nx/src/native/tasks/hashers/hash_task_output.rs, packages/nx/src/native/tasks/dep_outputs.rs, vercel/turborepo@af729ab crates/turborepo-task-hash/src/test/dependency_outputs_test.rs',
            );
        }),
        SLOW,
    );

// --- [EXPORTS] -------------------------------------------------------------------------

export { dependentOutputs };

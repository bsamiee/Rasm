import { assert } from '@effect/vitest';
import { hashed } from '@rasm/testing/nx/cli';
import { Effect, Record } from 'effect';
import type { Subject } from '#/inference/cases';
import { amended, type Methods, placed, present, SLOW } from '#/inference/workspaces';

// --- [OPERATIONS] ----------------------------------------------------------------------

const declaredInputs = (subject: Subject, it: Methods) =>
    it.effect.skipIf(!present(subject))(
        'gives a cacheable task a new hash for a change to each file Nx lists as its input, and the same hash for a change to any other file',
        Effect.fnUntraced(function* () {
            const root = yield* placed(subject);
            const before = yield* hashed(root);
            const read = Object.values(before).flatMap(({ cache, files }) => (cache ? files : []));
            const listed = Record.filter(subject.files, (_, file) => read.includes(file));
            const wrong = yield* Effect.forEach(
                Record.toEntries(listed),
                Effect.fnUntraced(function* ([file, text]) {
                    yield* amended(root, file, `${text}\n`);
                    const after = yield* hashed(root);
                    yield* amended(root, file, text);
                    return Record.collect(
                        Record.filter(before, ({ cache, hash, files }, task) => cache && files.includes(file) === (hash === after[task]?.hash)),
                        (task, { files }) => `${task} ${files.includes(file) ? 'keeps its hash after a change to its input' : 'has a new hash after a change to'} ${file}`,
                    );
                }),
            );
            assert.deepStrictEqual(wrong.flat(), [], 'onepipeline-ui@2ea90f8 .github/tests/nx-cache/nx-probe-hashes.mjs and nx-fixture.mjs probeEdit, nrwl/nx@2a424d3 packages/nx/src/native/tasks/hash_planner.rs');
        }),
        SLOW,
    );

// --- [EXPORTS] -------------------------------------------------------------------------

export { declaredInputs };

import { assert } from '@effect/vitest';
import { answered, called, hashed } from '@rasm/testing/nx/cli';
import { Array, Effect, Record, Schema } from 'effect';
import type { Subject } from '#/inference/cases';
import { amended, type Methods, placed, present, SLOW, sources } from '#/inference/workspaces';

// --- [CONSTANTS] -----------------------------------------------------------------------

const COMMIT = [
    ['init', '--quiet'],
    ['add', '--all'],
    ['-c', 'user.name=suite', '-c', 'user.email=suite@example.com', 'commit', '--quiet', '--message', 'base'],
] as const;

// --- [MODELS] --------------------------------------------------------------------------

const Names = Schema.fromJsonString(Schema.Array(Schema.String));

// --- [OPERATIONS] ----------------------------------------------------------------------

const affected = (subject: Subject, it: Methods) =>
    it.effect.skipIf(!present(subject) || sources(subject).length === 0)(
        'has nx affected select the project of every cacheable task a change to a source file gives a new hash',
        Effect.fnUntraced(function* () {
            const [file, text] = yield* Effect.fromNullishOr(sources(subject)[0]);
            const root = yield* placed(subject);
            yield* Effect.forEach(COMMIT, (args) => called(root, 'git', args, {}), { discard: true });
            const before = yield* hashed(root);
            yield* amended(root, file, `${text}\n`);
            const after = yield* hashed(root);
            const selected = yield* Effect.flatMap(answered(root, ['show', 'projects', '--affected', '--uncommitted', '--json']), Schema.decodeEffect(Names));
            const moved = Array.dedupe(Object.values(Record.filter(before, ({ cache, hash }, task) => cache && hash !== after[task]?.hash)).map(({ project }) => project));
            assert.deepStrictEqual(
                Array.difference(moved, selected),
                [],
                `nrwl/nx@2a424d3 packages/nx/src/daemon/server/handle-select-affected-tasks.spec.ts, packages/nx/src/native/affected/tasks/touched.rs, onepipeline-ui@2ea90f8 .github/tests/nx-cache/nx-affected-inputs.test.mjs: ${file} moved ${moved.join(', ')}, affected ${selected.join(', ')}`,
            );
        }),
        SLOW,
    );

// --- [EXPORTS] -------------------------------------------------------------------------

export { affected };

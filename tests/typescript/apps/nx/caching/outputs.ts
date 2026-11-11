import { Buffer } from 'node:buffer';
import { assert } from '@effect/vitest';
import { ran } from '@rasm/testing/nx/cli';
import { Array, Effect, FileSystem, Path } from 'effect';
import type { Subject } from '#/inference/cases';
import { type Methods, prepared, present, SLOW } from '#/inference/workspaces';

// --- [CONSTANTS] -----------------------------------------------------------------------

const HIDDEN = ['node_modules', '.nx'];

// --- [OPERATIONS] ----------------------------------------------------------------------

const content = Effect.fnUntraced(function* (file: string) {
    const fs = yield* FileSystem.FileSystem;
    const { type } = yield* fs.stat(file);
    return type === 'File' ? Buffer.from(yield* fs.readFile(file)).toString('base64') : type;
});

const restoredOutputs = (subject: Subject, it: Methods) =>
    it.effect.skipIf(!present(subject) || subject.runs === undefined)(
        'writes only inside declared outputs and tool state folders, and restores from the cache every file a run wrote',
        Effect.fnUntraced(function* () {
            const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
            const targets = yield* Effect.fromNullishOr(subject.runs);
            const { root, folders, cached } = yield* prepared(subject);
            const tree = Effect.map(fs.readDirectory(root, { recursive: true }), (files) => files.filter((file) => Array.intersection(file.split(path.sep), [...HIDDEN, ...(subject.state ?? [])]).length === 0).toSorted());
            const run = ran(root, ['run-many', '-t', targets.join(',')]);
            const before = yield* tree;
            const first = yield* run;
            const written = Array.difference(yield* tree, before);
            const contents = Effect.forEach(written, (file) => content(path.join(root, file)));
            const wrote = yield* contents;
            yield* Effect.forEach(folders, (folder) => fs.remove(path.join(root, folder), { recursive: true, force: true }), { discard: true });
            const second = yield* run;
            assert.deepStrictEqual([first.code, second.code], [0, 0], `${first.output}${second.output}`);
            assert.deepStrictEqual(
                written.filter((file) => !folders.some((folder) => file === folder || file.startsWith(`${folder}/`) || folder.startsWith(`${file}/`))),
                [],
                'nrwl/nx@2a424d3 astro-docs kb/sandboxing.mdoc "write only declared outputs"',
            );
            assert.deepStrictEqual(
                [second.tasks.filter(({ projectName, target, cacheStatus }) => cached(projectName, target) && cacheStatus === 'cache-miss').map(({ taskId }) => taskId), Array.difference(yield* tree, before), yield* contents],
                [[], written, wrote],
                'nrwl/nx@2a424d3 packages/nx/src/native/cache/cache.rs, packages/nx/src/native/workspace/outputs_tracking.rs, vercel/turborepo@af729ab crates/turborepo/tests/run_caching.rs',
            );
        }),
        SLOW,
    );

// --- [EXPORTS] -------------------------------------------------------------------------

export { restoredOutputs };

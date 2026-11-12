import { assert } from '@effect/vitest';
import { graph } from '@rasm/testing/nx/cli';
import { Effect, FileSystem, Path, Schema } from 'effect';
import type { Subject } from '#/inference/cases';
import { amended, data, fresh, type Methods, placed, present, row, sources } from '#/inference/workspaces';

// --- [CONSTANTS] -----------------------------------------------------------------------

const NX = 'nrwl/nx@2a424d3 packages';
const SOURCE = {
    evaluated: `${NX}/dotnet/src/analyzer/analyzer-client.spec.ts "rerun the analyzer when an evaluation input changed", ${NX}/docker/src/plugins/plugin.spec.ts "use cached targets when hash matches"`,
    options: `${NX}/nx/src/project-graph/nx-deps-cache.spec.ts "plugin config changes", ${NX}/devkit/src/utils/calculate-hash-for-create-nodes.spec.ts`,
    corrupt: `${NX}/nx/src/utils/plugin-cache-utils.spec.ts "should return empty cache if file is corrupted"`,
    incremental: `${NX}/nx/src/project-graph/nx-deps-cache.spec.ts "should handle cases when files change", ${NX}/nx/src/project-graph/project-graph-builder.spec.ts`,
} as const;
const OUTPUT_PATH = { pluginsConfig: { '@rasm/nx-devkit': { outputPath: '.artifacts' } } };

// --- [MODELS] --------------------------------------------------------------------------

const Json = Schema.fromJsonString(Schema.Unknown);

// --- [OPERATIONS] ----------------------------------------------------------------------

const rereads = (subject: Subject, it: Methods) => {
    const live = it.effect.skipIf(!present(subject));

    it.effect.skipIf(!present(subject) || subject.changed === undefined)(
        'returns after a change to a file its tool evaluates the graph a fresh workspace holding the change returns',
        Effect.fnUntraced(function* () {
            const [file, text] = yield* Effect.fromNullishOr(subject.changed);
            const root = yield* placed(subject);
            const before = yield* graph(root);
            yield* amended(root, file, text);
            const warm = yield* graph(root);
            const cold = yield* Effect.flatMap(placed(subject, { [file]: text }), graph);
            assert.notDeepEqual([data(before), before.dependencies], [data(cold), cold.dependencies], `${file} changes nothing its tool reports`);
            assert.deepStrictEqual([data(warm), warm.dependencies], [data(cold), cold.dependencies], SOURCE.evaluated);
        }),
    );

    live(
        'returns after nx.json gains a pluginsConfig outputPath the graph a fresh workspace holding that nx.json returns',
        Effect.fnUntraced(function* () {
            const configured = { plugins: [row(subject)], ...OUTPUT_PATH };
            const root = yield* placed(subject);
            yield* graph(root);
            yield* Effect.flatMap(Schema.encodeUnknownEffect(Json)(configured), (text) => amended(root, 'nx.json', text));
            const warm = yield* graph(root);
            const cold = yield* Effect.flatMap(placed(subject, {}, configured), graph);
            assert.deepStrictEqual(data(warm), data(cold), SOURCE.options);
        }),
    );

    live(
        'reads a corrupt plugin cache file as empty and returns the graph it returned before',
        Effect.fnUntraced(function* () {
            const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
            const root = yield* placed(subject);
            const stored = path.join(root, '.nx/workspace-data');
            const before = yield* graph(root);
            const caches = (yield* fs.readDirectory(stored)).filter((name) => name.endsWith('.hash'));
            yield* Effect.forEach(caches, (name) => fs.writeFileString(path.join(stored, name), '{"corrupt'), { discard: true });
            yield* fs.remove(path.join(stored, 'project-graph.json'));
            assert.deepStrictEqual(data(yield* graph(root)), data(before), SOURCE.corrupt);
        }),
    );

    live(
        'returns after one source file changes the dependencies a fresh read of the workspace returns',
        Effect.fnUntraced(function* () {
            const root = yield* placed(subject, { 'notes/changed.md': '# notes\n' });
            const before = yield* graph(root);
            yield* Effect.forEach([['notes/changed.md', '# notes\n'] as const, ...sources(subject).slice(0, 1)], ([file, text]) => amended(root, file, `${text}\n`), { discard: true });
            const warm = yield* graph(root);
            const cold = yield* graph(root, fresh(root, 'cold'));
            assert.deepStrictEqual([warm.dependencies, warm.dependencies], [cold.dependencies, before.dependencies], SOURCE.incremental);
        }),
    );
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { rereads };

import { matchesGlob } from 'node:path';
import { assert } from '@effect/vitest';
import type { ProjectGraph } from '@nx/devkit';
import { graph } from '@rasm/testing/nx/cli';
import { entered, graphed } from '@rasm/testing/nx/inference';
import { Effect, Path, Record, Schema } from 'effect';
import type { PluginConfiguration } from 'nx/src/config/nx-json';
import { LoadedNxPlugin } from 'nx/src/devkit-internals';
import type { Subject } from '#/inference/cases';
import { amended, data, fresh, located, type Methods, placed, present, row } from '#/inference/workspaces';

// --- [CONSTANTS] -----------------------------------------------------------------------

const NX = 'nrwl/nx@2a424d3 packages/nx/src/project-graph';
const SOURCE = {
    loaded: `${NX}/plugins/resolve-plugin.spec.ts, ${NX}/plugins/get-plugins.spec.ts`,
    isolated: `${NX}/plugins/isolation/isolated-plugin.spec.ts, ${NX}/error-types.spec.ts "deserialized from a plugin worker"`,
    scoped: `${NX}/utils/retrieve-workspace-files.spec.ts "should glob only the include patterns of a scoped plugin"`,
    ignored: `${NX}/utils/retrieve-workspace-files.spec.ts "should not find files that are listed by .nxignore"`,
    beside: `${NX}/utils/retrieve-workspace-files.spec.ts "multiple plugins should not affect other plugins", ${NX}/utils/project-configuration/project-nodes-manager.spec.ts`,
    named: `${NX}/utils/retrieve-workspace-files.spec.ts "should keep the name inferred by an earlier plugin", ${NX}/utils/project-configuration/name-substitution-manager.spec.ts`,
    copied: `${NX}/utils/project-configuration/project-nodes-manager.spec.ts "should error if multiple roots point to the same project"`,
} as const;
const MANIFESTS = ['project.json', 'package.json'];
const MARKER = { plugin: './tools/marker-plugin.cjs' };
const MARKED = {
    'marked/marker.json': '{}\n',
    'tools/marker-plugin.cjs': "exports.createNodes = ['**/marker.json', (files) => files.map((file) => [file, { projects: { [require('node:path').dirname(file)]: { name: 'marked', targets: { mark: { command: 'node --version', cache: true } } } } }])];\n",
};

// --- [MODELS] --------------------------------------------------------------------------

const Json = Schema.fromJsonString(Schema.Unknown);

// --- [OPERATIONS] ----------------------------------------------------------------------

const printed = (value: unknown) => Effect.flatMap(Schema.encodeUnknownEffect(Json)(value), Schema.decodeEffect(Json));

const loading = (subject: Subject, it: Methods) => {
    const live = it.effect.skipIf(!present(subject));

    live(
        'loads from an nx.json plugins row and returns through the Nx CLI the projects a LoadedNxPlugin built from its exports returns',
        Effect.fnUntraced(function* () {
            const root = yield* placed(subject);
            const listed = yield* graph(root);
            yield* entered(root);
            const direct = yield* graphed(root, [new LoadedNxPlugin(subject.plugin, row(subject))]);
            const targets = (held: ProjectGraph) => Record.fromIterableWith(Object.values(held.nodes), (node) => [node.data.root, node.data.targets]);
            assert.deepStrictEqual(targets(listed), yield* printed(targets(direct)), SOURCE.loaded);
        }),
    );

    live(
        'returns the same projects and dependencies from an isolated plugin worker as from the Nx process',
        Effect.fnUntraced(function* () {
            const root = yield* placed(subject);
            const isolated = yield* graph(root, { NX_ISOLATE_PLUGINS: 'true', ...fresh(root, 'isolated') });
            const hosted = yield* graph(root, { NX_ISOLATE_PLUGINS: 'false', ...fresh(root, 'hosted') });
            assert.deepStrictEqual([isolated.nodes, isolated.dependencies], [hosted.nodes, hosted.dependencies], SOURCE.isolated);
        }),
    );

    it.effect.skipIf(!present(subject) || subject.broken === undefined)(
        'has Nx refuse the graph from an isolated plugin worker by naming the plugin and the config its tool cannot read',
        Effect.fnUntraced(function* () {
            const root = yield* placed(subject);
            const [file, text] = yield* Effect.fromNullishOr(subject.broken);
            yield* amended(root, file, text);
            const refusal = yield* graph(root, { NX_ISOLATE_PLUGINS: 'true' }).pipe(
                Effect.as('Nx returned a graph'),
                Effect.catchTag('NxRefusal', ({ output }) => Effect.succeed(output)),
            );
            assert.deepStrictEqual([refusal.includes(subject.plugin.name), refusal.includes(file)], [true, true], `${SOURCE.isolated}: ${refusal}`);
        }),
    );

    live(
        'reads no file where an include pattern names none or an exclude pattern names all: the graph equals one without its plugins row',
        Effect.fnUntraced(function* () {
            const scoped = (scope: Pick<Exclude<PluginConfiguration, string>, 'include' | 'exclude'>) => Effect.map(Effect.flatMap(placed(subject, {}, { plugins: [{ ...row(subject), ...scope }] }), graph), data);
            const unlisted = yield* Effect.map(Effect.flatMap(placed(subject, {}, { plugins: [] }), graph), data);
            assert.deepStrictEqual([yield* scoped({ include: ['no-such-folder/**'] }), yield* scoped({ exclude: ['**/*'] })], [unlisted, unlisted], SOURCE.scoped);
        }),
    );

    live(
        'reads no file .nxignore lists: the graph equals that of a workspace without those files',
        Effect.fnUntraced(function* () {
            const configs = Record.keys(subject.files).filter((file) => matchesGlob(file, subject.plugin.createNodes[0]));
            const ignoring = { '.nxignore': `${configs.join('\n')}\n` };
            const ignored = yield* Effect.flatMap(placed(subject, ignoring), graph);
            const removed = yield* Effect.flatMap(placed({ ...subject, files: Record.filter(subject.files, (_, file) => !configs.includes(file)) }, ignoring), graph);
            assert.deepStrictEqual(data(ignored), data(removed), SOURCE.ignored);
        }),
    );

    live(
        'leaves the project of another plugin as that plugin alone returns it and keeps its own projects beside it',
        Effect.fnUntraced(function* () {
            const listed = (plugins: readonly PluginConfiguration[]) => Effect.map(Effect.flatMap(placed(subject, MARKED, { plugins: [...plugins] }), graph), data);
            const [both, own, other] = yield* Effect.all([listed([row(subject), MARKER]), listed([row(subject)]), listed([MARKER])]);
            assert.deepStrictEqual([Record.remove(both, 'marked'), Record.get(both, 'marked')], [own, Record.get(other, 'marked')], SOURCE.beside);
        }),
    );

    live(
        'keeps each name and target it gives where a project.json without a name sits in the project folder',
        Effect.fnUntraced(function* () {
            const path = yield* Path.Path;
            const plain = yield* Effect.flatMap(placed(subject), graph);
            const beside = Record.fromIterableWith(
                Object.values(located(plain)).filter((folder) => !MANIFESTS.some((manifest) => path.join(folder, manifest) in subject.files)),
                (folder) => [path.join(folder, 'project.json'), '{}\n'],
            );
            const merged = yield* Effect.flatMap(placed(subject, beside), graph);
            const dropped = Record.map(plain.nodes, (node, name) => Record.keys(node.data.targets ?? {}).filter((target) => merged.nodes[name]?.data.targets?.[target] === undefined));
            assert.deepStrictEqual([located(merged), dropped], [located(plain), Record.map(plain.nodes, () => [])], SOURCE.named);
        }),
    );

    live(
        'keeps a project when a second folder holds a copy of its files, or has Nx refuse the graph naming both folders',
        Effect.fnUntraced(function* () {
            const plain = yield* Effect.flatMap(placed(subject), graph);
            const [folder] = Object.values(located(plain))
                .filter((root) => root !== '.')
                .toSorted();
            const copied = yield* Effect.fromNullishOr(folder);
            const copies = Record.fromIterableWith(
                Record.toEntries(subject.files).filter(([file]) => file.startsWith(`${copied}/`)),
                ([file, text]) => [`${copied}-copy${file.slice(copied.length)}`, text],
            );
            const kept = yield* Effect.flatMap(placed(subject, copies), graph).pipe(
                Effect.map((twice) => Object.values(located(twice)).includes(copied)),
                Effect.catchTag('NxRefusal', ({ output }) => Effect.succeed(output.includes(copied) && output.includes(`${copied}-copy`))),
            );
            assert.isTrue(kept, `a copy of ${copied} at ${copied}-copy removed ${copied} from the graph, or the refusal names neither folder: ${SOURCE.copied}`);
        }),
    );
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { loading };

import { layer } from '@effect/platform-node/NodeServices';
import { assert, it } from '@effect/vitest';
import { workspaceRoot } from '@nx/devkit';
import apple from '@rasm/nx-apple/package.json' with { type: 'json' };
import cargo from '@rasm/nx-cargo/package.json' with { type: 'json' };
import cmake from '@rasm/nx-cmake/package.json' with { type: 'json' };
import distribution from '@rasm/nx-distribution/package.json' with { type: 'json' };
import docker from '@rasm/nx-docker/package.json' with { type: 'json' };
import dotnet from '@rasm/nx-dotnet/package.json' with { type: 'json' };
import pulumi from '@rasm/nx-pulumi/package.json' with { type: 'json' };
import storybook from '@rasm/nx-storybook/package.json' with { type: 'json' };
import typescript from '@rasm/nx-typescript/package.json' with { type: 'json' };
import uv from '@rasm/nx-uv/package.json' with { type: 'json' };
import vite from '@rasm/nx-vite/package.json' with { type: 'json' };
import vitest from '@rasm/nx-vitest/package.json' with { type: 'json' };
import { Consumer, Packed } from '@rasm/testing/nx';
import { Array, Duration, Effect, Layer, Option, Predicate, Record, Schema } from 'effect';
import { minimatch } from 'minimatch';
import { readNxJson } from 'nx/src/config/nx-json';
import { read, type Workspace } from '#/workspace';

// --- [TYPES] ---------------------------------------------------------------------------

interface Row {
    readonly plugin: string;
    readonly options?: unknown;
    readonly include?: readonly string[];
    readonly exclude?: readonly string[];
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const SOURCES = {
    shape: 'nrwl/nx packages/nx/src/project-graph/plugins/public-api.ts NxPlugin: every member is optional, createNodes is a pair of a file pattern and a function, every other member a function',
    contributed: 'nrwl/nx e2e/nx/src/extras.test.ts: should use plugins defined in nx.json',
    manifest: 'nrwl/nx packages/{vite,vitest,playwright,docker,dotnet,storybook,js,cypress,gradle,maven}/package.json: `@nx/devkit` is a dependency, and `nx` is never one',
    matched: 'nrwl/nx packages/nx/src/project-graph/plugins/public-api.ts CreateNodesResultArray: each result names the configuration file it came from, one of the files the pattern matched',
    empty: 'nrwl/nx packages/nx/src/project-graph/plugins/utils.spec.ts createNodesFromFiles: returns one result per file it was given',
    twice: 'nrwl/nx packages/nx/src/project-graph/project-graph.spec.ts: should not throw an error if sane plugins are used and called in parallel; two computations of one tree give one graph',
    refused: 'nrwl/nx packages/nx/src/project-graph/plugins/utils.spec.ts createNodesFromFiles: should throw AggregateCreateNodesError with the file and the error of each failing file, and keep partial results',
} as const;
const INSTALLED = Duration.toMillis(Duration.minutes(10));
const THROWN = "throw new Error('refused');\n";
const MANIFESTS: Record.ReadonlyRecord<string, unknown> = Record.fromIterableWith([apple, cargo, cmake, distribution, docker, dotnet, pulumi, storybook, typescript, uv, vite, vitest], (manifest) => [manifest.name, manifest]);
const rows: readonly Row[] = (readNxJson(workspaceRoot).plugins ?? []).map((row) => (Predicate.isString(row) ? { plugin: row } : row));
const BROKEN: Record.ReadonlyRecord<string, readonly [file: string, text: string]> = {
    '@rasm/nx-apple': ['apps/broken/Package.swift', 'not a manifest\n'],
    '@rasm/nx-cargo': ['crates/broken/Cargo.toml', '[package\n'],
    '@rasm/nx-cmake': ['CMakePresets.json', '{\n'],
    '@rasm/nx-distribution': ['apps/broken/blender_manifest.toml', 'id = \n'],
    '@rasm/nx-docker': ['infra/compose.yaml', 'services: [\n'],
    '@rasm/nx-dotnet': ['libs/broken/Broken.csproj', '<Project\n'],
    '@rasm/nx-pulumi': ['infra/package.json', '{\n'],
    '@rasm/nx-storybook': ['apps/broken/.storybook/main.ts', THROWN],
    '@rasm/nx-typescript': ['libs/broken/tsconfig.json', '{\n'],
    '@rasm/nx-uv': ['libs/broken/pyproject.toml', '[project\n'],
    '@rasm/nx-vite': ['apps/broken/vite.config.ts', THROWN],
    '@rasm/nx-vitest': ['apps/broken/vitest.config.ts', THROWN],
};

// --- [MODELS] --------------------------------------------------------------------------

const Manifest = Schema.Struct({ dependencies: Schema.optionalKey(Schema.Record(Schema.String, Schema.String)) });

// --- [OPERATIONS] ----------------------------------------------------------------------

const loaded = (plugin: string) => Effect.promise((): Promise<Record.ReadonlyRecord<string, unknown>> => import(plugin));

const created = Effect.fnUntraced(function* (plugin: string) {
    const { createNodes } = yield* loaded(plugin);
    const [pattern, create] = Array.isArray(createNodes) ? createNodes : [];
    return Option.all({ pattern: Option.liftPredicate(pattern, Predicate.isString), create: Option.liftPredicate(create, Predicate.isFunction) });
});

const formed = (name: string, member: unknown) => (['createNodes', 'createNodesV2'].includes(name) ? Array.isArray(member) && Predicate.isString(member[0]) && Predicate.isFunction(member[1]) : Predicate.isFunction(member));

const shaped = Effect.fnUntraced(function* (plugin: string) {
    const stated = Record.filter(yield* loaded(plugin), (_, name) => ['createNodes', 'createNodesV2', 'createDependencies', 'createMetadata', 'preTasksExecution', 'postTasksExecution'].includes(name));
    return { stated: Record.keys(stated), misshapen: Record.keys(Record.filter(stated, (member, name) => !formed(name, member))) };
});

const declaring = Effect.fnUntraced(function* (manifest: unknown) {
    const { dependencies = {} } = yield* Schema.decodeUnknownEffect(Manifest)(manifest);
    return { devkit: '@nx/devkit' in dependencies, bundled: 'nx' in dependencies };
});

const valued = (data: unknown, key: string) => key.split('.').reduce<Option.Option<unknown>>((held, part) => Option.map(Option.filter(held, Predicate.isObject), (parent): unknown => Reflect.get(parent, part)), Option.some(data));

const attributed = ({ nodes, sourceMaps }: Workspace, plugin: string) =>
    Object.values(nodes).flatMap(({ data }) =>
        Record.collect(
            Record.filter(sourceMaps[data.root] ?? {}, ([, owner]) => owner === plugin),
            (key, [file]) => ({ root: data.root, key, file, value: valued(data, key) }),
        ),
    );

const stray = (workspace: Workspace, { plugin, include = ['**'], exclude = [] }: Row, pattern: string) =>
    Array.dedupe(
        attributed(workspace, plugin)
            .filter(({ file }) => file === null || !minimatch(file, pattern, { dot: true }) || exclude.some((excluded) => minimatch(file, excluded, { dot: true })) || !include.some((included) => minimatch(file, included, { dot: true })))
            .map(({ root, file }) => `${root} from ${file}`),
    );

const emptied = Effect.fnUntraced(function* ({ plugin, options }: Row) {
    const creating = yield* created(plugin);
    return yield* Effect.promise(() => Promise.all(Option.toArray(creating).map(({ create }) => create([], options, { nxJsonConfiguration: readNxJson(workspaceRoot), workspaceRoot }))));
});

const refusing = (plugin: string, file: string) =>
    Effect.fnUntraced(function* () {
        const { read: graphed } = yield* Consumer;
        const [{ nodes, failures }, creating] = yield* Effect.all([graphed, created(plugin)]);
        const refused = failures.filter(({ name, message }) => name === 'AggregateCreateNodesError' && message.includes(plugin));
        assert.isTrue(
            Option.exists(creating, ({ pattern }) => minimatch(file, pattern, { dot: true })),
            `${file} matches the file pattern of ${plugin}`,
        );
        assert.deepStrictEqual(
            refused.flatMap(({ files }) => files.map((failed) => failed.file)),
            [file],
            SOURCES.refused,
        );
        assert.include(
            Object.values(nodes).map(({ data }) => data.root),
            'libs/kept',
            SOURCES.refused,
        );
    });

// --- [COMPOSITION] ---------------------------------------------------------------------

it.layer(layer, { excludeTestServices: true })('plugin contract', (suite) => {
    suite.effect.each(rows)(
        '$plugin exports at least one member of the plugin type of Nx, each in the form that type states',
        Effect.fnUntraced(function* ({ plugin }) {
            const { stated, misshapen } = yield* shaped(plugin);
            assert.isNotEmpty(stated, SOURCES.shape);
            assert.deepStrictEqual(misshapen, [], SOURCES.shape);
        }),
    );

    suite.effect.each(rows)(
        '$plugin declares @nx/devkit as a dependency and nx outside its dependencies',
        Effect.fnUntraced(function* ({ plugin }) {
            assert.deepStrictEqual(yield* declaring(MANIFESTS[plugin]), { devkit: true, bundled: false }, SOURCES.manifest);
        }),
    );

    suite.effect.each(rows)(
        '$plugin gives project properties exactly where it exports createNodes, each from a file its pattern matches and its row includes',
        Effect.fnUntraced(function* (row) {
            const [workspace, creating] = yield* Effect.all([read('fresh'), created(row.plugin)]);
            assert.strictEqual(Array.isReadonlyArrayNonEmpty(attributed(workspace, row.plugin)), Option.isSome(creating), SOURCES.contributed);
            assert.deepStrictEqual(
                Option.toArray(creating).flatMap(({ pattern }) => stray(workspace, row, pattern)),
                [],
                SOURCES.matched,
            );
        }),
    );

    suite.effect.each(rows)(
        '$plugin returns no project for an empty list of files',
        Effect.fnUntraced(function* (row) {
            assert.deepStrictEqual((yield* emptied(row)).flat(), [], SOURCES.empty);
        }),
    );

    suite.effect.each(rows)(
        '$plugin gives the same project properties in two processes that share no stored state',
        Effect.fnUntraced(function* ({ plugin }) {
            const [fresh, again] = yield* Effect.all([read('fresh'), read('again')]);
            assert.deepStrictEqual(attributed(again, plugin), attributed(fresh, plugin), SOURCES.twice);
        }),
    );
});

it.layer(Layer.provideMerge(Packed.layer(...Record.keys(BROKEN)), layer), { timeout: INSTALLED, excludeTestServices: true })('plugin failures', (suite) => {
    Record.toEntries(BROKEN).forEach(([plugin, [file, text]]) => {
        suite.layer(Consumer.layer({ resolved: import.meta.url, settings: { plugins: [plugin] }, ignored: [], files: { 'libs/kept/package.json': '{ "name": "kept", "version": "0.0.0", "type": "module" }\n', [file]: text } }), { timeout: INSTALLED })(plugin, (workspace) => {
            workspace.effect('names a file it cannot read in an AggregateCreateNodesError, and Nx keeps the project of every other file', refusing(plugin, file), INSTALLED);
        });
    });
});

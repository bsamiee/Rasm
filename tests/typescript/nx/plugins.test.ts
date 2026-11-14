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
import { Array, Effect, Option, Predicate, Record, Schema } from 'effect';
import { minimatch } from 'minimatch';
import { readNxJson } from 'nx/src/config/nx-json';
import { declared, dependents, hosted, read, tracked, type Workspace } from '#/workspace';

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
    contributed:
        'nrwl/nx e2e/nx/src/extras.test.ts: should use plugins defined in nx.json; packages/nx/src/project-graph/plugins/public-api.ts CreateNodes: Nx calls the function with the files its pattern matches, so a row with no matching file gives no property; README.md PRINCIPLES: Plugins present a target that runs a host or needs a platform only on a machine holding it',
    manifest: 'nrwl/nx packages/{vite,vitest,playwright,docker,dotnet,storybook,js,cypress,gradle,maven}/package.json: `@nx/devkit` is a dependency, and `nx` is never one',
    matched: 'nrwl/nx packages/nx/src/project-graph/plugins/public-api.ts CreateNodesResultArray: each result names the configuration file it came from, one of the files the pattern matched',
    empty: 'nrwl/nx packages/nx/src/project-graph/plugins/utils.spec.ts createNodesFromFiles: returns one result per file it was given',
    lent: "README.md PRINCIPLES: Task inputs hold the files a task's tool reads: plugin and executor source are no input; nrwl/nx packages/nx/src/native/tasks/hash_planner.rs external_deps: a task hashes the packages its inputs name and those its project depends on, and none for the executor of a local plugin",
    twice: 'nrwl/nx packages/nx/src/project-graph/project-graph.spec.ts: should not throw an error if sane plugins are used and called in parallel; two computations of one tree give one graph',
} as const;
const MANIFESTS: Record.ReadonlyRecord<string, unknown> = Record.fromIterableWith([apple, cargo, cmake, distribution, docker, dotnet, pulumi, storybook, typescript, uv, vite, vitest], (manifest) => [manifest.name, manifest]);
const rows: readonly Row[] = (readNxJson(workspaceRoot).plugins ?? []).map((row) => (Predicate.isString(row) ? { plugin: row } : row));

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

const valued = (data: unknown, key: string) => key.split('.').reduce<Option.Option<unknown>>((held, part) => Option.map(Option.filter(held, Predicate.isObjectOrArray), (parent): unknown => Reflect.get(parent, part)), Option.some(data));

const attributed = ({ nodes, sourceMaps }: Workspace, plugin: string) =>
    Object.values(nodes).flatMap(({ data }) =>
        Record.collect(
            Record.filter(sourceMaps[data.root] ?? {}, ([, owner]) => owner === plugin),
            (key, [file]) => ({ root: data.root, key, file, value: valued(data, key) }),
        ),
    );

const matching = ({ include = ['**'], exclude = [] }: Row, pattern: string, files: readonly string[]) => files.some((file) => minimatch(file, pattern, { dot: true }) && include.some((included) => minimatch(file, included, { dot: true })) && !exclude.some((excluded) => minimatch(file, excluded, { dot: true })));

const stray = (workspace: Workspace, row: Row, pattern: string) =>
    Array.dedupe(
        attributed(workspace, row.plugin)
            .filter(({ file }) => !matching(row, pattern, Array.fromNullishOr(file)))
            .map(({ root, file }) => `${root} from ${file}`),
    );

const lent = Effect.fnUntraced(function* (workspace: Workspace, plugin: string) {
    const { dependencies = {} } = yield* Schema.decodeUnknownEffect(Manifest)(MANIFESTS[plugin]);
    const allowed = Record.map(
        Record.filter(workspace.externalNodes, ({ data }) => data.packageName in dependencies),
        (_, node) => dependents(workspace, new Set([node])),
    );
    return declared(workspace)
        .filter(({ project, task, target }) => target.cache === true && Record.keys(workspace.hashes[task]?.inputs ?? {}).some((input) => allowed[input]?.has(project) === false))
        .map(({ task }) => task);
});

const emptied = Effect.fnUntraced(function* ({ plugin, options }: Row) {
    const creating = yield* created(plugin);
    return yield* Effect.promise(() => Promise.all(Option.toArray(creating).map(({ create }) => create([], options, { nxJsonConfiguration: readNxJson(workspaceRoot), workspaceRoot }))));
});

// --- [COMPOSITION] ---------------------------------------------------------------------

it.layer(layer, { excludeTestServices: true })((suite) => {
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
        '$plugin gives project properties exactly where this machine runs its tools and its createNodes pattern matches a tracked file its row includes, each from such a file',
        Effect.fnUntraced(function* (row) {
            const [workspace, creating, files] = yield* Effect.all([read('fresh'), created(row.plugin), tracked()]);
            assert.strictEqual(Array.isReadonlyArrayNonEmpty(attributed(workspace, row.plugin)), hosted(row.plugin) && Option.exists(creating, ({ pattern }) => matching(row, pattern, files)), SOURCES.contributed);
            assert.deepStrictEqual(
                Option.toArray(creating).flatMap(({ pattern }) => stray(workspace, row, pattern)),
                [],
                SOURCES.matched,
            );
        }),
    );

    suite.effect.each(rows)(
        '$plugin gives no cached task outside the projects depending on them a package of its own dependencies as input',
        Effect.fnUntraced(function* ({ plugin }) {
            assert.deepStrictEqual(yield* lent(yield* read('fresh'), plugin), [], SOURCES.lent);
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

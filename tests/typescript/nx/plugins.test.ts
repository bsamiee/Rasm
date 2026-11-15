import { findPackageJSON } from 'node:module';
import { layer } from '@effect/platform-node/NodeServices';
import { assert, it } from '@effect/vitest';
import { type ExpandedPluginConfiguration, type TargetConfiguration, workspaceRoot } from '@nx/devkit';
import type { Graph } from '@rasm/testing/nx';
import { Array, Effect, FileSystem, Option, Predicate, Record, Schema } from 'effect';
import { minimatch } from 'minimatch';
import { readNxJson } from 'nx/src/config/nx-json';
import { declared, dependents, plugins, read } from '#/workspace';

// --- [CONSTANTS] -----------------------------------------------------------------------

const SOURCES = {
    matched: 'nrwl/nx packages/nx/src/project-graph/plugins/public-api.ts CreateNodesResultArray: each result names the configuration file it came from, one of the files the pattern matched, and a row with include or exclude passes its plugin only the files they keep',
    empty: 'nrwl/nx packages/nx/src/project-graph/plugins/utils.spec.ts createNodesFromFiles: returns one result per file it was given',
    lent: "README.md PRINCIPLES: Task inputs hold the files a task's tool reads: plugin and executor source are no input; nrwl/nx packages/nx/src/native/tasks/hash_planner.rs external_deps: a task hashes the packages its inputs name and those its project depends on, and none for the executor of a local plugin",
} as const;

// --- [MODELS] --------------------------------------------------------------------------

const Loaded = Schema.Struct({ externalDependencies: Schema.Array(Schema.String) }).annotate({ description: 'An input naming the installed packages a task loads' });
const Manifest = Schema.fromJsonString(Schema.Struct({ dependencies: Schema.optionalKey(Schema.Record(Schema.String, Schema.String)) }));

// --- [OPERATIONS] ----------------------------------------------------------------------

const created = Effect.fnUntraced(function* (plugin: string) {
    const { createNodes } = yield* Effect.promise((): Promise<Record.ReadonlyRecord<string, unknown>> => import(plugin));
    const [pattern, create] = Array.isArray(createNodes) ? createNodes : [];
    return Option.all({ pattern: Option.liftPredicate(pattern, Predicate.isString), create: Option.liftPredicate(create, Predicate.isFunction) });
});

const valued = (data: unknown, key: string) => key.split('.').reduce<Option.Option<unknown>>((held, part) => Option.map(Option.filter(held, Predicate.isObjectOrArray), (parent): unknown => Reflect.get(parent, part)), Option.some(data));

const attributed = ({ nodes, sourceMaps }: Graph, plugin: string) =>
    Object.values(nodes).flatMap(({ data }) =>
        Record.collect(
            Record.filter(sourceMaps[data.root] ?? {}, ([, owner]) => owner === plugin),
            (key, [file]) => ({ root: data.root, key, file, value: valued(data, key) }),
        ),
    );

const matching = ({ include = ['**'], exclude = [] }: ExpandedPluginConfiguration, pattern: string, files: readonly string[]) =>
    files.some((file) => minimatch(file, pattern, { dot: true }) && include.some((included) => minimatch(file, included, { dot: true })) && !exclude.some((excluded) => minimatch(file, excluded, { dot: true })));

const stray = (workspace: Graph, row: ExpandedPluginConfiguration, pattern: string) =>
    Array.dedupe(
        attributed(workspace, row.plugin)
            .filter(({ file }) => !matching(row, pattern, Array.fromNullishOr(file)))
            .map(({ root, file }) => `${root} from ${file}`),
    );

const reached = ({ dependencies }: Pick<Graph, 'dependencies'>, nodes: ReadonlySet<string>): ReadonlySet<string> => {
    const next = nodes.union(new Set([...nodes].flatMap((node) => (dependencies[node] ?? []).map(({ target }) => target))));
    return next.size === nodes.size ? nodes : reached({ dependencies }, next);
};

const named = (workspace: Graph, { inputs = [] }: TargetConfiguration) => {
    const packages = new Set(inputs.filter(Schema.is(Loaded)).flatMap(({ externalDependencies }) => externalDependencies));
    return reached(workspace, new Set(Record.keys(Record.filter(workspace.externalNodes, ({ data }) => packages.has(data.packageName)))));
};

const lent = Effect.fnUntraced(function* (workspace: Graph, plugin: string) {
    const fs = yield* FileSystem.FileSystem;
    const manifest = yield* Effect.fromNullishOr(findPackageJSON(plugin, import.meta.url));
    const { dependencies = {} } = yield* Effect.flatMap(fs.readFileString(manifest), Schema.decodeEffect(Manifest));
    const allowed = Record.map(
        Record.filter(workspace.externalNodes, ({ data }) => data.packageName in dependencies),
        (_, node) => dependents(workspace, new Set([node])),
    );
    return declared(workspace)
        .filter(({ target }) => target.cache === true)
        .map(({ project, task, target }) => ({ task, target, foreign: new Set(Record.keys(workspace.hashes[task]?.inputs ?? {}).filter((input) => allowed[input]?.has(project) === false)) }))
        .filter(({ target, foreign }) => foreign.size > 0 && !foreign.isSubsetOf(named(workspace, target)))
        .map(({ task }) => task);
});

const emptied = Effect.fnUntraced(function* ({ plugin, options }: ExpandedPluginConfiguration) {
    const creating = yield* created(plugin);
    return yield* Effect.promise(() => Promise.all(Option.toArray(creating).map(({ create }) => create([], options, { nxJsonConfiguration: readNxJson(workspaceRoot), workspaceRoot }))));
});

// --- [COMPOSITION] ---------------------------------------------------------------------

it.layer(layer, { excludeTestServices: true })((suite) => {
    suite.effect.each(plugins)(
        '$plugin gives each project property from a file its createNodes pattern matches and its row includes',
        Effect.fnUntraced(function* (row) {
            const [workspace, creating] = yield* Effect.all([read('fresh'), created(row.plugin)]);
            assert.deepStrictEqual(
                Option.toArray(creating).flatMap(({ pattern }) => stray(workspace, row, pattern)),
                [],
                SOURCES.matched,
            );
        }),
    );

    suite.effect.each(plugins)(
        '$plugin gives no cached task outside the projects depending on them a package of its own dependencies as input',
        Effect.fnUntraced(function* ({ plugin }) {
            assert.deepStrictEqual(yield* lent(yield* read('fresh'), plugin), [], SOURCES.lent);
        }),
    );

    suite.effect.each(plugins)(
        '$plugin returns no project for an empty list of files',
        Effect.fnUntraced(function* (row) {
            assert.deepStrictEqual((yield* emptied(row)).flat(), [], SOURCES.empty);
        }),
    );
});

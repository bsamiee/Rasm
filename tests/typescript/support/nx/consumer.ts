import { findPackageJSON } from 'node:module';
import { devNull } from 'node:os';
import { delimiter } from 'node:path';
import { env } from 'node:process';
import { pathToFileURL } from 'node:url';
import { type ProjectGraph, workspaceRoot } from '@nx/devkit';
import { Completed } from '@rasm/process';
import { Array, Context, Effect, FileSystem, Layer, Option, Path, Predicate, Record, Schema } from 'effect';
import { ChildProcess, ChildProcessSpawner } from 'effect/process';
import type { NxJsonConfiguration } from 'nx/src/config/nx-json';

// --- [TYPES] ---------------------------------------------------------------------------

interface Stated {
    readonly resolved: string | URL;
    readonly settings: NxJsonConfiguration;
    readonly packages?: readonly string[];
    readonly ignored: readonly string[];
    readonly files: Record.ReadonlyRecord<string, string>;
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const inherited = Record.filter(env, (value, name): value is string => Predicate.isString(value) && !['NX_', 'VITEST', 'npm_'].some((prefix) => name.startsWith(prefix)) && !['NODE_ENV', 'NO_COLOR', 'VIRTUAL_ENV'].includes(name));

// --- [MODELS] --------------------------------------------------------------------------

const Json = Schema.fromJsonString(Schema.Unknown, { space: 4 });
const Version = Schema.fromJsonString(Schema.Struct({ version: Schema.String }));
const Printed = Schema.fromJsonString(Schema.Struct({ graph: Schema.Struct({ nodes: Schema.Record(Schema.String, Schema.Any), dependencies: Schema.Record(Schema.String, Schema.Any) }) })).annotate({
    description: 'JSON nx graph --print writes: the project graph Nx built, nodes and dependencies in the shape of its ProjectGraph type',
});
const Read = Schema.fromJsonString(
    Schema.Struct({
        nodes: Schema.Record(Schema.String, Schema.Any),
        dependencies: Schema.Record(Schema.String, Schema.Any),
        failures: Schema.Array(Schema.Struct({ name: Schema.String, message: Schema.String, files: Schema.Array(Schema.Struct({ file: Schema.NullOr(Schema.String), message: Schema.String })) })),
    }),
).annotate({ description: 'Project graph createProjectGraphAsync built inside the workspace, the partial graph where a plugin failed, and each failure with the files its plugin named' });
const Planned = Schema.fromJsonString(
    Schema.Struct({
        tasks: Schema.Struct({
            tasks: Schema.Record(Schema.String, Schema.Struct({ cache: Schema.optionalKey(Schema.Boolean), parallelism: Schema.Boolean, outputs: Schema.Array(Schema.String) })),
            dependencies: Schema.Record(Schema.String, Schema.Array(Schema.Struct({ id: Schema.String }))),
        }),
    }),
).annotate({ description: 'JSON nx run-many --graph=<file> writes: each task Nx plans with its cache and parallelism flags and declared outputs, and the tasks each one waits on' });
const Inputs = Schema.fromJsonString(
    Schema.Struct({
        files: Schema.Array(Schema.String),
        runtime: Schema.optionalKey(Schema.Array(Schema.String)),
        environment: Schema.optionalKey(Schema.Array(Schema.String)),
        depOutputs: Schema.optionalKey(Schema.Array(Schema.String)),
        external: Schema.optionalKey(Schema.Array(Schema.String)),
    }),
).annotate({ description: 'JSON nx show target inputs prints: the workspace files Nx hashes for one task, and where the task has any its runtime commands, environment variables, dependency output globs, and external packages' });
const Outputs = Schema.fromJsonString(Schema.Struct({ outputPaths: Schema.optionalKey(Schema.Array(Schema.String)), expandedOutputs: Schema.optionalKey(Schema.Array(Schema.String)) })).annotate({
    description: 'JSON nx show target outputs prints: the paths one task declares where it declares any, and once files exist the paths on disk they expand to',
});

// --- [SERVICES] ------------------------------------------------------------------------

class Packed extends Context.Service<Packed, Record.ReadonlyRecord<string, string>>()('@rasm/testing/nx/consumer/Packed') {
    static readonly layer = (...plugins: readonly string[]) =>
        Layer.effect(
            Packed,
            Effect.gen(function* () {
                const path = yield* Path.Path;
                const nx = (...args: readonly string[]) => Completed.output(ChildProcess.make('nx', args, { cwd: workspaceRoot, stdin: 'ignore', extendEnv: true, env: { NX_TUI: 'false' } }));
                const { nodes, dependencies }: ProjectGraph = (yield* Effect.flatMap(nx('graph', '--print'), Schema.decodeEffect(Printed))).graph;
                const archives = Record.map(nodes, ({ data }) => archived(path, data.targets ?? {}));
                const reached = (names: readonly string[], held: ReadonlySet<string>): ReadonlySet<string> => {
                    const fresh = Array.dedupe(names).filter((name) => !held.has(name) && Array.isReadonlyArrayNonEmpty(archives[name] ?? []));
                    return Array.isReadonlyArrayNonEmpty(fresh)
                        ? reached(
                              Array.intersection(
                                  Record.keys(nodes),
                                  fresh.flatMap((name) => (dependencies[name] ?? []).map(({ target }) => target)),
                              ),
                              held.union(new Set(fresh)),
                          )
                        : held;
                };
                const packages = [...reached(plugins, new Set())];
                yield* nx('run-many', '--targets=pack', `--projects=${packages.join(',')}`);
                return yield* Effect.all(Record.fromIterableWith(packages, (name) => [name, tarball(archives[name] ?? [])]));
            }).pipe(Effect.orDie),
        );
}

class Consumer extends Context.Service<
    Consumer,
    {
        readonly root: string;
        readonly called: (command: string, args: readonly string[], folder?: string) => Effect.Effect<Completed>;
        readonly nx: (...args: readonly string[]) => Effect.Effect<Completed>;
        readonly evaluated: (script: string, args?: readonly string[], folder?: string, variables?: Record.ReadonlyRecord<string, string>) => Effect.Effect<string>;
        readonly read: Effect.Effect<ProjectGraph & Pick<typeof Read.Type, 'failures'>>;
        readonly planned: (...args: readonly string[]) => Effect.Effect<(typeof Planned.Type)['tasks']>;
        readonly inputs: (task: string) => Effect.Effect<typeof Inputs.Type>;
        readonly outputs: (task: string) => Effect.Effect<Required<typeof Outputs.Type>>;
        readonly wrote: (files: Record.ReadonlyRecord<string, string>) => Effect.Effect<void>;
        readonly text: (file: string) => Effect.Effect<string>;
        readonly absent: (files: readonly string[]) => Effect.Effect<readonly string[]>;
        readonly written: <A, E, R>(effect: Effect.Effect<A, E, R>) => Effect.Effect<readonly [value: A, files: readonly string[]], E, R>;
        readonly committed: (message: string) => Effect.Effect<void>;
        readonly cleaned: Effect.Effect<void>;
    }
>()('@rasm/testing/nx/consumer') {
    static readonly layer = ({ resolved, settings, packages = [], ignored, files }: Stated) =>
        Layer.effect(
            Consumer,
            Effect.gen(function* () {
                const [fs, path, spawner, packed] = yield* Effect.all([FileSystem.FileSystem, Path.Path, ChildProcessSpawner.ChildProcessSpawner, Packed]);
                const root = yield* Effect.flatMap(fs.makeTempDirectoryScoped({ prefix: 'nx-consumer-' }), fs.realPath);
                const plugins = (settings.plugins ?? []).map((row) => (Predicate.isString(row) ? row : row.plugin));
                const versions = yield* Effect.all(Record.fromIterableWith(Array.dedupe(['nx', ...packages]), (name) => [name, installed(name, resolved)]));
                const sources = Record.map(packed, (archive) => pathToFileURL(archive).href);
                const variables = {
                    ...inherited,
                    NX_DAEMON: 'false',
                    NX_TUI: 'false',
                    FORCE_COLOR: '0',
                    GIT_CONFIG_GLOBAL: devNull,
                    GIT_CONFIG_NOSYSTEM: '1',
                    GIT_AUTHOR_NAME: 'suite',
                    GIT_AUTHOR_EMAIL: 'suite@example.com',
                    GIT_COMMITTER_NAME: 'suite',
                    GIT_COMMITTER_EMAIL: 'suite@example.com',
                    PATH: [path.join(root, 'node_modules/.bin'), ...Option.toArray(Record.get(inherited, 'PATH'))].join(delimiter),
                };
                const called = (command: string, args: readonly string[], folder = '.') => Completed.run(ChildProcess.make(command, args, { cwd: path.join(root, folder), stdin: 'ignore', extendEnv: false, env: variables })).pipe(Effect.provideService(ChildProcessSpawner.ChildProcessSpawner, spawner), Effect.orDie);
                const done = Effect.fnUntraced(function* (command: string, ...args: readonly string[]) {
                    const { checked } = yield* called(command, args);
                    return yield* checked;
                }, Effect.orDie);
                const evaluated = Effect.fnUntraced(function* (script: string, args: readonly string[] = [], folder = '.', stated: Record.ReadonlyRecord<string, string> = {}) {
                    const { checked } = yield* called('env', [...Record.collect(stated, (name, value) => `${name}=${value}`), 'node', '--input-type=module', '--eval', script, ...args], folder);
                    const printed = yield* checked;
                    return yield* Effect.fromOption(Array.last(printed.trimEnd().split('\n')));
                }, Effect.orDie);
                const others = Effect.map(done('git', 'ls-files', '--others', '--', ':/', ':!node_modules', ':!.nx'), (listed) => listed.split('\n').filter((file) => file !== ''));
                const wrote = (stated: Record.ReadonlyRecord<string, string>) => Effect.orDie(Effect.forEach(Record.toEntries(stated), ([file, text]) => Effect.andThen(fs.makeDirectory(path.join(root, path.dirname(file)), { recursive: true }), fs.writeFileString(path.join(root, file), text)), { discard: true }));
                const committed = (message: string) => Effect.asVoid(Effect.andThen(done('git', 'add', '--all'), done('git', 'commit', '--quiet', '--allow-empty', '--message', message)));
                const [nxJson, manifest, overrides] = yield* Effect.all([
                    Schema.encodeEffect(Json)(settings),
                    Schema.encodeEffect(Json)({ name: 'consumer', private: true, type: 'module', devDependencies: { ...versions, ...Record.filter(sources, (_, name) => plugins.includes(name)) } }),
                    Schema.encodeEffect(Json)({ overrides: sources }),
                ]);
                yield* wrote({ '.gitignore': ['node_modules', '.nx', ...ignored, ''].join('\n'), 'nx.json': nxJson, 'package.json': manifest, 'pnpm-workspace.yaml': overrides, ...files });
                yield* done('pnpm', 'install', '--offline', '--ignore-scripts');
                yield* done('git', 'init', '--quiet', '--initial-branch=main');
                yield* committed('base');
                return {
                    root,
                    called,
                    nx: (...args: readonly string[]) => called('nx', args),
                    evaluated,
                    read: Effect.orDie(
                        Effect.flatMap(
                            evaluated(
                                `import { createProjectGraphAsync } from 'nx/src/devkit-exports';
import { isAggregateCreateNodesError, ProjectGraphError } from 'nx/src/project-graph/error-types';
const read = await createProjectGraphAsync().then(
    ({ nodes, dependencies }) => ({ nodes, dependencies, failures: [] }),
    (cause) => {
        if (!(cause instanceof ProjectGraphError)) throw cause;
        const { nodes, dependencies } = cause.getPartialProjectGraph();
        const failures = cause.getErrors().map((error) => ({ name: error.name, message: error.message, files: isAggregateCreateNodesError(error) ? error.errors.map(([file, { message }]) => ({ file, message })) : [] }));
        return { nodes, dependencies, failures };
    },
);
process.stdout.write(JSON.stringify(read), () => process.exit(0));`,
                                [],
                                '.',
                                { NX_CACHE_PROJECT_GRAPH: 'false' },
                            ),
                            Schema.decodeEffect(Read),
                        ),
                    ),
                    planned: Effect.fnUntraced(
                        function* (...args: readonly string[]) {
                            const file = path.join(yield* fs.makeTempDirectoryScoped({ prefix: 'nx-planned-' }), 'tasks.json');
                            yield* done('nx', ...args, `--graph=${file}`);
                            const { tasks } = yield* Effect.flatMap(fs.readFileString(file), Schema.decodeEffect(Planned));
                            return tasks;
                        },
                        Effect.scoped,
                        Effect.orDie,
                    ),
                    inputs: (task: string) => Effect.orDie(Effect.flatMap(done('nx', 'show', 'target', 'inputs', task, '--json'), Schema.decodeEffect(Inputs))),
                    outputs: Effect.fnUntraced(function* (task: string) {
                        const printed = yield* done('nx', 'show', 'target', 'outputs', task, '--json');
                        const { outputPaths = [], expandedOutputs = [] } = yield* Schema.decodeEffect(Outputs)(printed);
                        return { outputPaths, expandedOutputs };
                    }, Effect.orDie),
                    wrote,
                    text: (file: string) => Effect.orDie(fs.readFileString(path.join(root, file))),
                    absent: (stated: readonly string[]) => Effect.orDie(Effect.filter(stated, (file) => Effect.map(fs.exists(path.join(root, file)), (found) => !found))),
                    written: Effect.fnUntraced(function* <A, E, R>(effect: Effect.Effect<A, E, R>) {
                        const before = yield* others;
                        const value = yield* effect;
                        return [value, Array.difference(yield* others, before)] as const;
                    }),
                    committed,
                    cleaned: Effect.asVoid(done('git', 'clean', '-xdf', '--exclude=node_modules', '--exclude=.nx')),
                };
            }).pipe(Effect.orDie),
        );
}

// --- [OPERATIONS] ----------------------------------------------------------------------

const archived = (path: Path.Path, targets: NonNullable<ProjectGraph['nodes'][string]['data']['targets']>) =>
    Object.values(targets)
        .flatMap(({ outputs = [] }) => outputs)
        .filter((output) => path.extname(output) === '.tgz');

const tarball = Effect.fnUntraced(function* (patterns: readonly string[]) {
    const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
    const pattern = yield* Effect.fromOption(Array.head(patterns));
    const folder = path.dirname(pattern).replace('{workspaceRoot}', workspaceRoot);
    const entries = yield* fs.readDirectory(folder);
    const archive = yield* Effect.fromOption(Array.findFirst(entries, (entry) => path.extname(entry) === '.tgz'));
    return path.join(folder, archive);
});

const installed = Effect.fnUntraced(function* (name: string, resolved: string | URL) {
    const fs = yield* FileSystem.FileSystem;
    const file = yield* Effect.fromNullishOr(findPackageJSON(name, resolved));
    const { version } = yield* Effect.flatMap(fs.readFileString(file), Schema.decodeEffect(Version));
    return version;
});

const fromCache = (task: string) => new RegExp(`${RegExp.escape(task)}.*\\[(local cache|remote cache|existing outputs match)`, 'u');

// --- [EXPORTS] -------------------------------------------------------------------------

export { Consumer, fromCache, inherited, Packed };

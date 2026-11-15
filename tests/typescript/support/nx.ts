import { Buffer } from 'node:buffer';
import { createServer } from 'node:http';
import { constants } from 'node:http2';
import { findPackageJSON } from 'node:module';
import { devNull } from 'node:os';
import { delimiter, join } from 'node:path';
import { env } from 'node:process';
import { fileURLToPath, pathToFileURL } from 'node:url';
import { createProjectGraphAsync, type ExpandedPluginConfiguration, type ProjectGraph, workspaceRoot } from '@nx/devkit';
import { run } from '@rasm/process';
import { Array, Context, Effect, FileSystem, Function, Layer, Match, Option, Path, Predicate, Record, Schema } from 'effect';
import type { ChildProcess, ChildProcessSpawner } from 'effect/process';
import type { NxJsonConfiguration } from 'nx/src/config/nx-json';
import { Reading } from '#/reading';

// --- [TYPES] ---------------------------------------------------------------------------

interface Stated {
    readonly resolved: string | URL;
    readonly settings: NxJsonConfiguration;
    readonly packages?: readonly string[];
    readonly ignored: readonly string[];
    readonly files: Record.ReadonlyRecord<string, string>;
}

interface Asked {
    readonly authorized: boolean;
    readonly held: Buffer | undefined;
}

interface Graph extends Required<Pick<ProjectGraph, 'nodes' | 'dependencies' | 'externalNodes'>> {
    readonly sourceMaps: (typeof Reading.Type)['sourceMaps'];
    readonly hashes: (typeof Reading.Type)['hashes'];
    readonly failures: (typeof Reading.Type)['failures'];
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const inherited = Record.filter(env, (value, name): value is string => Predicate.isString(value) && !['NX_', 'VITEST', 'npm_'].some((prefix) => name.startsWith(prefix)) && !['NODE_ENV', 'NO_COLOR', 'VIRTUAL_ENV', 'GITHUB_ACTIONS'].includes(name));

// --- [MODELS] --------------------------------------------------------------------------

const Json = Schema.fromJsonString(Schema.Unknown, { space: 4 });
const Version = Schema.fromJsonString(Schema.Struct({ version: Schema.String }));

const Planned = Schema.fromJsonString(
    Schema.Struct({
        tasks: Schema.Struct({
            tasks: Schema.Record(Schema.String, Schema.Struct({ target: Schema.Struct({ project: Schema.String, target: Schema.String }), cache: Schema.optionalKey(Schema.Boolean), parallelism: Schema.Boolean, outputs: Schema.Array(Schema.String) })),
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
const Run = Schema.fromJsonString(Schema.Struct({ tasks: Schema.Array(Schema.Struct({ taskId: Schema.String, hash: Schema.String, cacheStatus: Schema.Literals(['local-cache-hit', 'remote-cache-hit', 'cache-miss']), status: Schema.Int })) })).annotate({
    description: 'run.json Nx writes into its cache folder at the end of each task run: the hash, cache status, and exit code of every task the run held',
});

// --- [SERVICES] ------------------------------------------------------------------------

class Packed extends Context.Service<Packed, Record.ReadonlyRecord<string, string>>()('@rasm/testing/nx/Packed') {
    static readonly layer = (...plugins: readonly string[]) =>
        Layer.effect(
            Packed,
            Effect.gen(function* () {
                const path = yield* Path.Path;
                const { nodes, dependencies } = yield* Effect.promise(() => createProjectGraphAsync());
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
                yield* run(['nx', 'run-many', '--targets=pack', `--projects=${packages.join(',')}`], { cwd: workspaceRoot, extendEnv: true, env: { NX_TUI: 'false' } });
                return yield* Effect.all(Record.fromIterableWith(packages, (name) => [name, tarball(name, archives[name] ?? [])]));
            }).pipe(Effect.orDie),
        );
}

class Consumer extends Context.Service<
    Consumer,
    {
        readonly root: string;
        readonly plugins: readonly string[];
        readonly called: (command: string, args: readonly string[], folder?: string) => Effect.Effect<Effect.Success<ReturnType<typeof exited>>>;
        readonly nx: (...args: readonly string[]) => Effect.Effect<Effect.Success<ReturnType<typeof exited>>>;
        readonly ran: (args: readonly string[], set?: Record.ReadonlyRecord<string, string>) => Effect.Effect<Effect.Success<ReturnType<typeof exited>> & { readonly tasks: Record.ReadonlyRecord<string, (typeof Run.Type)['tasks'][number]['cacheStatus']> }>;
        readonly evaluated: (script: string, args?: readonly string[], folder?: string, variables?: Record.ReadonlyRecord<string, string>) => Effect.Effect<string>;
        readonly read: Effect.Effect<Graph>;
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
>()('@rasm/testing/nx/Consumer') {
    static readonly layer = ({ resolved, settings, packages = [], ignored, files }: Stated) =>
        Layer.effect(
            Consumer,
            Effect.gen(function* () {
                const [fs, path, packed, services] = yield* Effect.all([FileSystem.FileSystem, Path.Path, Packed, Effect.context<ChildProcessSpawner.ChildProcessSpawner>()]);
                const root = yield* Effect.flatMap(fs.makeTempDirectoryScoped({ prefix: 'nx-consumer-' }), fs.realPath);
                const plugins = rows(settings).map(({ plugin }) => plugin);
                const versions = yield* Effect.all(Record.fromIterableWith(Array.dedupe(['nx', ...packages]), (name) => [name, installed(name, resolved)]));
                const sources = Record.map(packed, (archive) => pathToFileURL(archive).href);
                const [cache, store] = [path.join(root, '.nx/cache'), yield* Effect.map(run(['pnpm', 'store', 'path'], { cwd: workspaceRoot }), ({ stdout }) => stdout.trim())];
                const variables = environment(root, {
                    NX_CACHE_DIRECTORY: cache,
                    NX_WORKSPACE_DATA_DIRECTORY: path.join(root, '.nx/workspace-data'),
                    NX_NATIVE_FILE_CACHE_DIRECTORY: path.join(root, '.nx/native'),
                    GIT_CONFIG_GLOBAL: devNull,
                    GIT_CONFIG_NOSYSTEM: '1',
                    GIT_AUTHOR_NAME: 'suite',
                    GIT_AUTHOR_EMAIL: 'suite@example.com',
                    GIT_COMMITTER_NAME: 'suite',
                    GIT_COMMITTER_EMAIL: 'suite@example.com',
                });
                const spawned = (argv: Array.NonEmptyReadonlyArray<string>, folder = '.', set: Record.ReadonlyRecord<string, string> = {}) => exited(argv, { cwd: path.join(root, folder), extendEnv: false, env: { ...variables, ...set } }).pipe(Effect.provide(services), Effect.orDie);
                const called = (command: string, args: readonly string[], folder = '.') => spawned([command, ...args], folder);
                const done = (command: string, ...args: readonly string[]) => Effect.orDie(Effect.flatMap(called(command, args), ({ checked }) => checked));
                const evaluated = Effect.fnUntraced(function* (script: string, args: readonly string[] = [], folder = '.', stated: Record.ReadonlyRecord<string, string> = {}) {
                    const { checked } = yield* spawned(['node', '--input-type=module', '--eval', script, ...args], folder, stated);
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
                yield* done('pnpm', 'install', '--prefer-offline', '--ignore-scripts', `--store-dir=${store}`);
                yield* done('git', 'init', '--quiet', '--initial-branch=main');
                yield* committed('base');
                return {
                    root,
                    plugins,
                    called,
                    nx: (...args: readonly string[]) => called('nx', args),
                    ran: Effect.fnUntraced(function* (args: readonly string[], set: Record.ReadonlyRecord<string, string> = {}) {
                        const folder = Option.getOrElse(Record.get(set, 'NX_CACHE_DIRECTORY'), () => cache);
                        yield* Effect.orDie(fs.remove(path.join(folder, 'run.json'), { force: true }));
                        const outcome = yield* spawned(['nx', ...args], '.', set);
                        const tasks = yield* Effect.orDie(Effect.provideService(recorded(folder), FileSystem.FileSystem, fs));
                        return { ...outcome, tasks: Record.fromIterableWith(tasks, ({ taskId, cacheStatus }) => [taskId, cacheStatus]) };
                    }),
                    evaluated,
                    read: Effect.orDie(
                        Effect.flatMap(
                            Effect.flatMap(spawned(['node', fileURLToPath(import.meta.resolve('#/reading'))], '.', { NX_CACHE_PROJECT_GRAPH: 'false' }), ({ checked }) => checked),
                            Schema.decodeEffect(Reading),
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

const kept = Effect.gen(function* () {
    const { nx, written, absent, cleaned } = yield* Consumer;
    yield* cleaned;
    const [built, created] = yield* written(nx('run-many', '--targets=build', '--output-style=static'));
    yield* nx('reset');
    const shown = yield* nx('show', 'projects');
    return { built, shown, created, removed: yield* absent(created) };
});

const exited = (argv: Array.NonEmptyReadonlyArray<string>, options?: ChildProcess.CommandOptions) =>
    run(argv, options).pipe(
        Effect.map(({ stdout, stderr }) => ({ code: 0, stdout, stderr, checked: Effect.succeed(stdout) })),
        Effect.catchTag('ExitError', (error) => Effect.succeed({ code: error.code, stdout: error.stdout, stderr: error.stderr, checked: Effect.fail(error) })),
    );

const environment = (root: string, set: Record.ReadonlyRecord<string, string> = {}) => ({
    ...inherited,
    NX_DAEMON: 'false',
    NX_TUI: 'false',
    FORCE_COLOR: '0',
    PATH: [join(root, 'node_modules/.bin'), ...Option.toArray(Record.get(inherited, 'PATH'))].join(delimiter),
    ...set,
});

const rows = ({ plugins = [] }: Pick<NxJsonConfiguration, 'plugins'>): readonly ExpandedPluginConfiguration[] => plugins.map((row) => (Predicate.isString(row) ? { plugin: row } : row));

const recorded = Effect.fnUntraced(function* (folder: string) {
    const fs = yield* FileSystem.FileSystem;
    const { tasks } = yield* Effect.flatMap(fs.readFileString(join(folder, 'run.json')), Schema.decodeEffect(Run));
    return tasks;
});

const archived = (path: Path.Path, targets: NonNullable<ProjectGraph['nodes'][string]['data']['targets']>) =>
    Object.values(targets)
        .flatMap(({ outputs = [] }) => outputs)
        .filter((output) => path.extname(output) === '.tgz');

const tarball = Effect.fnUntraced(function* (name: string, patterns: readonly string[]) {
    const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
    const pattern = yield* Effect.fromOption(Array.head(patterns));
    const folder = path.dirname(pattern).replace('{workspaceRoot}', workspaceRoot).replaceAll('{projectName}', name);
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

const answered = Match.type<Asked>().pipe(
    Match.when({ authorized: false }, () => constants.HTTP_STATUS_UNAUTHORIZED),
    Match.when({ held: Match.undefined }, () => constants.HTTP_STATUS_NOT_FOUND),
    Match.orElse(() => constants.HTTP_STATUS_OK),
);

const served = (token: string, answer: (asked: Asked) => Option.Option<number>) => {
    const [held, requests] = [new Map<string, Buffer>(), Array.empty<string>()];
    const server = createServer((request, response) => {
        const chunks: Buffer[] = [];
        requests.push(`${request.method} ${request.url}`);
        request.on('data', (chunk: Buffer) => chunks.push(chunk));
        request.on('end', () => {
            const body = request.method === 'PUT' ? held.set(String(request.url), Buffer.concat(chunks)).get(String(request.url)) : held.get(String(request.url));
            Option.match(answer({ authorized: request.headers.authorization === `Bearer ${token}`, held: body }), {
                onNone: Function.constVoid,
                onSome: (status) => response.writeHead(status).end(request.method === 'GET' ? body : undefined),
            });
        });
    });
    return { server, requests };
};

const remote = (token: string, answer: (asked: Asked) => Option.Option<number>) =>
    Effect.map(
        Effect.acquireRelease(
            Effect.callback<ReturnType<typeof served>>((resume) => {
                const listening = served(token, answer);
                listening.server.listen(0, '127.0.0.1', () => resume(Effect.succeed(listening)));
            }),
            ({ server }) =>
                Effect.callback<void>((resume) => {
                    server.closeAllConnections();
                    server.close(() => resume(Effect.void));
                }),
        ),
        ({ server, requests }) => {
            const address = server.address();
            return { address: `http://127.0.0.1:${Predicate.isObject(address) ? address.port : address}`, requests };
        },
    );

// --- [EXPORTS] -------------------------------------------------------------------------

export { answered, Consumer, environment, exited, type Graph, kept, Packed, recorded, remote, rows };

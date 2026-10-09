// biome-ignore-all lint/correctness/noNodejsModules lint/nursery/noRestrictedDependencies: Recovery requires native process APIs and Rimraf's cancellable filtered deletion.

import { homedir, userInfo } from 'node:os';
import { sep } from 'node:path';
import process from 'node:process';
import { NodeRuntime, NodeServices, NodeSocket } from '@effect/platform-node';
import { cacheDir, createProjectGraphAsync, getOutputsForTargetAndConfiguration, workspaceRoot } from '@nx/devkit';
import { Array, Config, Console, Data, Effect, FileSystem, flow, HashMap, identity, Option, Path, type PlatformError, Predicate, Record, Result, Schema, Stream, String, Struct, Tuple } from 'effect';
import { ChildProcess, type ChildProcessSpawner } from 'effect/process';
import { escape, Minimatch } from 'minimatch';
import { rimraf } from 'rimraf';

// --- [CONSTANTS] -----------------------------------------------------------------------

const PS_ROW = /^\s*(?<pid>\d+)\s+(?<ppid>\d+)\s+(?<uid>\d+)\s+(?<state>\S+)\s+(?<terminal>-?\d+)\s+(?<started>\S+ \S+\s+\S+ \S+ \S+)\s+(?<command>.*)$/u;
const JOB_ROW = /^(?<pid>\d+|-)\t\S+\t(?<label>.+)$/u;
const OPEN_PROCESS = /^p(?<pid>\d+)\0\n(?<files>[\s\S]*)$/u;
const OPEN_BLOCK = /(?<=\0\n)(?=p\d+\0)/u;
const SUCCESS_CODES = [0];
const LSOF_CODES = [...SUCCESS_CODES, 1];
const GLOB_OPTIONS = { dot: true, windowsPathsNoEscape: process.platform === 'win32', magicalBraces: true, nocase: false };

// --- [MODELS] --------------------------------------------------------------------------

const Installed = Schema.fromJsonString(Schema.Record(Schema.String, Schema.Array(Schema.Struct({ install_path: Schema.String }))));
const Agents = Schema.fromJsonString(Schema.Struct({ launchd: Schema.Struct({ agents: Schema.Array(Schema.Struct({ loaded: Schema.Boolean, label: Schema.String, path: Schema.String })) }) }));
const Registered = Schema.fromJsonString(Schema.Struct({ connection_uri: Schema.URLFromString }));
const Listed = Schema.Struct({ pid: Schema.NumberFromString, ppid: Schema.NumberFromString, uid: Schema.NumberFromString, state: Schema.String, terminal: Schema.NumberFromString, started: Schema.String, command: Schema.String });
const OpenProcess = Schema.Struct({ pid: Schema.NumberFromString, files: Schema.String });
const OpenFile = Schema.Struct({ descriptor: Schema.String, path: Schema.String });

interface Process extends Schema.Schema.Type<typeof Listed> {
    readonly files: Option.Option<readonly { readonly descriptor: string; readonly path: string }[]>;
}
interface Snapshot {
    readonly processes: HashMap.HashMap<number, Process>;
    readonly jobs: HashMap.HashMap<number, string>;
}
interface Removal {
    readonly root: string;
    readonly included: string[];
    readonly excluded: readonly Minimatch[];
}

// --- [ERRORS] --------------------------------------------------------------------------

class Exited extends Data.TaggedError('Exited')<{ readonly command: ChildProcess.StandardCommand; readonly code: number }> {}
class Unsignaled extends Data.TaggedError('Unsignaled')<{ readonly pid: number; readonly cause: unknown }> {}
class RemovalFailed extends Data.TaggedError('RemovalFailed')<{ readonly patterns: readonly string[]; readonly cause: unknown }> {}
class UnsupportedEndpoint extends Data.TaggedError('UnsupportedEndpoint')<{ readonly uri: URL }> {}

// --- [OPERATIONS] ----------------------------------------------------------------------

// --- [COMMANDS]
const execute = (command: ChildProcess.StandardCommand, accepted: readonly number[]): Effect.Effect<string, Exited | PlatformError.PlatformError, ChildProcessSpawner.ChildProcessSpawner> =>
    Effect.scoped(
        Effect.flatMap(ChildProcess.make(command.command, command.args, { ...command.options, stdin: 'ignore', stderr: 'inherit', env: { ...command.options.env, LC_ALL: 'C' }, extendEnv: true }), (handle) =>
            Effect.flatMap(Effect.all([Stream.mkString(Stream.decodeText(handle.stdout)), handle.exitCode], { concurrency: 'unbounded' }), ([text, code]) => (accepted.includes(code) ? Effect.succeed(text) : Effect.fail(new Exited({ command, code })))),
        ),
    );
const under = (entry: string, roots: readonly string[]): boolean => roots.some((root) => entry === root || entry.startsWith(`${root}${sep}`));

// --- [SNAPSHOT]
const opened = Effect.fn('opened')(function* (text: string) {
    return HashMap.fromIterable(
        yield* Effect.forEach(text.split(OPEN_BLOCK).filter(String.isNonEmpty), (block) =>
            Effect.gen(function* () {
                const { pid, files } = yield* Schema.decodeUnknownEffect(OpenProcess)(OPEN_PROCESS.exec(block)?.groups);
                const named = yield* Schema.decodeUnknownEffect(Schema.Array(OpenFile))([...files.matchAll(/(?:^|\0\n)f(?<descriptor>[^\0]+)\0n(?<path>[^\0]*)/gu)].map(Struct.get('groups')));
                return [pid, named] as const;
            }),
        ),
    );
});
const listed = execute(ChildProcess.make('ps', ['-axww', '-o', 'pid=,ppid=,uid=,stat=,tpgid=,lstart=,comm=']), SUCCESS_CODES).pipe(Effect.flatMap((text) => Effect.forEach(text.split('\n').filter(String.isNonEmpty), (line) => Schema.decodeUnknownEffect(Listed)(PS_ROW.exec(line)?.groups))));
const snapshot = Effect.gen(function* () {
    const [processes, files, jobs] = yield* Effect.all(
        [
            listed,
            execute(ChildProcess.make('lsof', ['-nP', '-F0pfn']), LSOF_CODES).pipe(Effect.flatMap(opened)),
            execute(ChildProcess.make('launchctl', ['list']), SUCCESS_CODES).pipe(Effect.flatMap((text) => Effect.forEach(text.split('\n').slice(1).filter(String.isNonEmpty), (line) => Schema.decodeUnknownEffect(Schema.Struct({ pid: Schema.String, label: Schema.String }))(JOB_ROW.exec(line)?.groups)))),
        ],
        { concurrency: 'unbounded' },
    );
    return { processes: HashMap.fromIterable(processes.map((running) => [running.pid, { ...running, files: HashMap.get(files, running.pid) }] as const)), jobs: HashMap.fromIterable(jobs.filter(({ pid }) => pid !== '-').map(({ pid, label }) => [Number(pid), label] as const)) } satisfies Snapshot;
});
const ancestors = (processes: HashMap.HashMap<number, Process>, pid: number): number[] =>
    Array.unfold(pid, (current) =>
        Option.map(
            Option.filter(HashMap.get(processes, current), (running) => running.ppid !== running.pid),
            (running) => [running.pid, running.ppid],
        ),
    );
const descendants = (processes: HashMap.HashMap<number, Process>, pid: number): Process[] => {
    const children = Array.groupBy(HashMap.values(processes), (running) => `${running.ppid}`);
    const walk = (current: number): Process[] => Option.match(Record.get(children, `${current}`), { onNone: () => [], onSome: (members) => members.flatMap((child) => [child, ...walk(child.pid)]) });
    return walk(pid);
};
const stale = ({ processes, jobs }: Snapshot, roots: readonly string[], installed: readonly string[], tools: readonly string[], self: number, uid: number): Process[] => {
    const running = Array.fromIterable(HashMap.values(processes));
    const kept = new Set([
        ...ancestors(processes, self),
        ...Array.flatMap(Array.fromIterable(HashMap.keys(jobs)), (pid) => [pid, ...descendants(processes, pid).map(Struct.get('pid'))]),
        ...Array.flatMap(
            running.filter((candidate) => candidate.terminal > 0),
            (candidate) => ancestors(processes, candidate.pid),
        ),
    ]);
    return Array.flatMap(
        running.filter(
            (candidate) =>
                candidate.ppid === 1 &&
                Option.exists(
                    candidate.files,
                    (files) => files.some(({ descriptor, path }) => descriptor === 'txt' && under(path, installed)) || (files.some(({ descriptor, path }) => descriptor === 'txt' && under(path, tools)) && files.some(({ descriptor, path }) => descriptor === 'cwd' && under(path, [...roots, ...tools]))),
                ),
        ),
        (candidate) => {
            const subtree = [candidate, ...descendants(processes, candidate.pid)];
            return subtree.some((member) => kept.has(member.pid) || member.uid !== uid) ? [] : subtree.filter((member) => !member.state.startsWith('Z'));
        },
    );
};

// --- [RECOVERY]
const wait = (pids: readonly number[]): Effect.Effect<string, Exited | PlatformError.PlatformError, ChildProcessSpawner.ChildProcessSpawner> => execute(ChildProcess.make('lsof', ['-nP', '-p', pids.join(','), '+r', '1'], { stdout: 'ignore' }), LSOF_CODES);
const terminate = Effect.fn('terminate')(function* (roots: readonly string[], installed: readonly string[], tools: readonly string[], uid: number) {
    const signal = (pid: number, name: 'SIGTERM' | 'SIGKILL'): Effect.Effect<void, Unsignaled> =>
        Effect.try({ try: () => process.kill(pid, name), catch: (cause) => new Unsignaled({ pid, cause }) }).pipe(
            Effect.asVoid,
            Effect.catchIf(
                (error) => error.cause instanceof Error && 'code' in error.cause && error.cause.code === 'ESRCH',
                () => Effect.void,
            ),
        );
    const targets = stale(yield* snapshot, roots, installed, tools, process.pid, uid);
    yield* Effect.validate(targets, (target) => Console.log(`terminate ${target.pid} ${target.command}`).pipe(Effect.andThen(signal(target.pid, 'SIGTERM'))), { discard: true });
    if (targets.length > 0 && Option.isNone(yield* wait(targets.map(Struct.get('pid'))).pipe(Effect.timeoutOption('1 minute')))) {
        const after = yield* listed;
        yield* Effect.validate(
            targets.filter((target) => after.some((running) => running.pid === target.pid && running.started === target.started)),
            (target) => Console.log(`kill ${target.pid} ${target.command}`).pipe(Effect.andThen(signal(target.pid, 'SIGKILL'))),
            { discard: true },
        );
    }
});
const withoutAgents = <A, E, R>(agents: Schema.Schema.Type<typeof Agents>['launchd']['agents'], uid: number, work: Effect.Effect<A, E, R>): Effect.Effect<A, E | Exited | PlatformError.PlatformError, R | ChildProcessSpawner.ChildProcessSpawner> =>
    agents.reduceRight<Effect.Effect<A, E | Exited | PlatformError.PlatformError, R | ChildProcessSpawner.ChildProcessSpawner>>(
        (use, agent) =>
            Effect.acquireUseRelease(
                Console.log(`bootout ${agent.label}`).pipe(Effect.andThen(execute(ChildProcess.make('launchctl', ['bootout', `gui/${uid}/${agent.label}`]), SUCCESS_CODES))),
                () => use,
                () => execute(ChildProcess.make('launchctl', ['bootstrap', `gui/${uid}`, agent.path]), SUCCESS_CODES).pipe(Effect.asVoid),
            ),
        work,
    );
const unreachable = Effect.fn('unreachable')(function* (directory: string) {
    const fs = yield* FileSystem.FileSystem;
    const path = yield* Path.Path;
    return yield* Effect.filter(
        (yield* fs.glob('lldb-mcp-*.json', { root: directory })).map((entry) => path.resolve(directory, entry)),
        (entry) =>
            Effect.gen(function* () {
                const { connection_uri: uri } = yield* fs.readFileString(entry).pipe(Effect.flatMap(Schema.decodeUnknownEffect(Registered)));
                if (!['connection:', 'connect:', 'tcp-connect:'].includes(uri.protocol) || uri.hostname === '' || uri.port === '') {
                    return yield* Effect.fail(new UnsupportedEndpoint({ uri }));
                }
                return yield* Effect.scoped(
                    NodeSocket.makeNet({ host: uri.hostname.replace(/^\[|\]$/gu, ''), port: Number(uri.port) }).pipe(
                        Effect.flatMap(Struct.get('reader')),
                        Effect.as(false),
                        Effect.catchIf(
                            (error) => error.reason._tag === 'SocketOpenError' && error.reason.cause instanceof Error && 'code' in error.reason.cause && error.reason.cause.code === 'ECONNREFUSED',
                            () => Effect.succeed(true),
                        ),
                    ),
                );
            }),
        { concurrency: 'unbounded' },
    );
});

// --- [FILES]
const litter = Effect.fn('litter')(function* (root: string) {
    const path = yield* Path.Path;
    const graph = yield* Effect.tryPromise(() => createProjectGraphAsync({ exitOnError: false }));
    const [direct, hypothesis, pulumi, browsers, data, dotnet] = yield* Effect.all(
        [
            Effect.forEach(['BIOME_LOG_PATH', 'NUGET_HTTP_CACHE_PATH', 'PYTHONPYCACHEPREFIX', 'RUFF_CACHE_DIR', 'SWIFTPM_BUILD_DIR', 'WRANGLER_LOG_PATH', 'MINIFLARE_REGISTRY_PATH', 'PLAYWRIGHT_MCP_OUTPUT_DIR', 'PLAYWRIGHT_HTML_OUTPUT_DIR'], Config.String),
            Config.String('HYPOTHESIS_STORAGE_DIRECTORY'),
            Config.String('PULUMI_HOME'),
            Config.String('PLAYWRIGHT_BROWSERS_PATH'),
            Config.String('NX_WORKSPACE_DATA_DIRECTORY'),
            Config.String('DOTNET_CLI_HOME'),
        ],
        { concurrency: 'unbounded' },
    );
    const patternRoot = escape(root, GLOB_OPTIONS);
    const protectedPaths = [cacheDir, data, browsers, dotnet, path.resolve(root, '.cache/playwright/profile')].map((entry) => escape(path.resolve(root, entry), GLOB_OPTIONS));
    const protectedPatterns = [...protectedPaths, path.join(patternRoot, '**/{node_modules,.venv}')];
    const outputs = Object.values(graph.nodes).flatMap((node) =>
        Object.entries(node.data.targets ?? {}).flatMap(([target, definition]) =>
            definition.outputs === undefined ? [] : [undefined, ...Object.keys(definition.configurations ?? {})].map((configuration) => getOutputsForTargetAndConfiguration({ project: node.name, target, ...(configuration === undefined ? {} : { configuration }) }, {}, node)),
        ),
    );
    const residue = ['.artifacts/{dotnet/binlog,ghidra,ilspy,python/coverage}', '.cache/{ghidra/{cache,tmp,settings/**/*.log,settings/ghidra/*/osgi},mypy,pytest,pmd,swiftlint,vite,vitest,xcode}', '{apps,libs,plugins,tools}/**/{__pycache__,.wrangler/{deploy,registry,tmp}}'];
    return [...outputs, residue, [...direct, ...['constants', 'observed', 'patches', 'tmp', 'unicode_data'].map((entry) => path.join(hypothesis, entry)), path.join(pulumi, 'logs')].map((entry) => escape(path.resolve(root, entry), GLOB_OPTIONS))].map(
        (patterns): Removal => ({
            root,
            included: patterns.filter(Predicate.not(String.startsWith('!'))).map((pattern) => path.resolve(patternRoot, pattern)),
            excluded: [...protectedPatterns, ...patterns.filter(String.startsWith('!')).map((pattern) => path.resolve(patternRoot, pattern.slice(1)))].map((pattern) => new Minimatch(`${pattern}{,/**}`, GLOB_OPTIONS)),
        }),
    );
});
const discard = Effect.fn('discard')(function* (groups: readonly Removal[]) {
    yield* Effect.validate(
        groups,
        ({ root, included, excluded }) => {
            const protectedEntry = Predicate.some(excluded.map((pattern) => pattern.match.bind(pattern)));
            return Console.log(`remove ${included.join(', ')}`).pipe(
                Effect.andThen(
                    Effect.tryPromise({
                        try: (signal) => rimraf(included, { signal, glob: { ...GLOB_OPTIONS, ignore: excluded.map(Struct.get('pattern')) }, filter: (entry) => under(entry, [root]) && !protectedEntry(entry) }),
                        catch: (cause) => new RemovalFailed({ patterns: included, cause }),
                    }),
                ),
            );
        },
        { discard: true },
    );
});
const prune = Effect.validate(
    [
        [ChildProcess.make('mise', ['prune', '--yes'], { stdout: 'inherit' })],
        [ChildProcess.make('mise', ['cache', 'prune'], { stdout: 'inherit' })],
        [ChildProcess.make('uv', ['cache', 'prune'], { stdout: 'inherit' })],
        [ChildProcess.make('pnpm', ['store', 'prune'], { stdout: 'inherit' }), ChildProcess.make('pnpm', ['cache', 'prune'], { stdout: 'inherit' })],
    ],
    (commands) => Effect.forEach(commands, (command) => execute(command, SUCCESS_CODES), { discard: true }),
    { concurrency: 'unbounded', discard: true },
);

// --- [COMPOSITION] ---------------------------------------------------------------------

const cleanup = Effect.fn('cleanup')(function* () {
    const path = yield* Path.Path;
    const root = path.resolve(workspaceRoot);
    const workspace = yield* Effect.result(litter(root));
    const work = <E, R>(removal: Effect.Effect<void, E, R>): Effect.Effect<void, Array.NonEmptyArray<E | Effect.Error<typeof prune>>, R | Effect.Services<typeof prune>> =>
        Effect.validate([prune, removal], identity<Effect.Effect<void, E | Effect.Error<typeof prune>, R | Effect.Services<typeof prune>>>, { concurrency: 'unbounded', discard: true });
    if (process.platform !== 'darwin') {
        return yield* work(Effect.fromResult(workspace).pipe(Effect.flatMap(discard)));
    }
    const { uid } = userInfo();
    const [roots, managed, developer, agents, cache] = yield* Effect.all(
        [
            execute(ChildProcess.make('git', ['-C', root, 'worktree', 'list', '--porcelain', '-z']), SUCCESS_CODES).pipe(
                Effect.map((text) =>
                    text
                        .split('\0')
                        .filter(String.startsWith('worktree '))
                        .map((field) => field.slice('worktree '.length)),
                ),
            ),
            execute(ChildProcess.make('mise', ['ls', '--installed', '--json']), SUCCESS_CODES).pipe(
                Effect.flatMap(Schema.decodeUnknownEffect(Installed)),
                Effect.map((installs) => Array.flatten(Record.values(installs)).map(Struct.get('install_path'))),
            ),
            execute(ChildProcess.make('xcode-select', ['--print-path']), SUCCESS_CODES).pipe(Effect.map(String.trim)),
            execute(ChildProcess.make('mise', ['bootstrap', 'macos', 'launchd-agents', 'status', '--json']), SUCCESS_CODES).pipe(
                Effect.flatMap(Schema.decodeUnknownEffect(Agents)),
                Effect.map((status) => status.launchd.agents.filter(Struct.get('loaded'))),
            ),
            execute(ChildProcess.make('uv', ['cache', 'dir']), SUCCESS_CODES).pipe(Effect.map(String.trim)),
        ],
        { concurrency: 'unbounded' },
    );
    yield* execute(ChildProcess.make('dotnet', ['build-server', 'shutdown'], { cwd: root, stdout: 'inherit' }), SUCCESS_CODES);
    yield* terminate(
        roots,
        roots.flatMap((worktree) => ['node_modules', '.venv', '.cache'].map((entry) => path.join(worktree, entry))),
        [...managed, developer],
        uid,
    );
    const registryRoot = path.join(homedir(), '.lldb');
    const registry = yield* unreachable(registryRoot).pipe(
        Effect.map((entries): Removal[] => [{ root: registryRoot, included: entries.map((entry) => escape(entry, GLOB_OPTIONS)), excluded: [] }]),
        Effect.result,
    );
    const plans: readonly Result.Result<readonly Removal[], Effect.Error<ReturnType<typeof litter>> | Effect.Error<ReturnType<typeof unreachable>>>[] = [workspace, registry];
    const removals = Array.getSomes(plans.map(Result.getSuccess)).flat();
    const state = yield* snapshot;
    const held = Predicate.some([
        (file: string): boolean => under(file, [cache]),
        ...removals.map((group) =>
            Predicate.every([
                (file: string): boolean => under(file, [group.root]),
                Predicate.some(group.included.map((pattern) => new Minimatch(`${pattern}{,/**}`, GLOB_OPTIONS)).map((pattern) => pattern.match.bind(pattern))),
                Predicate.not(Predicate.some(group.excluded.map((pattern) => pattern.match.bind(pattern)))),
            ]),
        ),
    ]);
    const busy = new Set(
        Array.fromIterable(HashMap.entries(state.jobs))
            .filter(([pid]) =>
                Array.getSomes([...Option.toArray(HashMap.get(state.processes, pid)), ...descendants(state.processes, pid)].map(Struct.get('files')))
                    .flat()
                    .some(flow(Struct.get('path'), held)),
            )
            .map(Tuple.get(1)),
    );
    const participating = agents.filter((agent) => busy.has(agent.label));
    const labels = new Set(participating.map(Struct.get('label')));
    const pids = Array.fromIterable(HashMap.entries(state.jobs))
        .filter(([, label]) => labels.has(label))
        .flatMap(([pid]) => [pid, ...descendants(state.processes, pid).map(Struct.get('pid'))]);
    return yield* withoutAgents(participating, uid, (pids.length === 0 ? Effect.void : wait(pids).pipe(Effect.asVoid)).pipe(Effect.andThen(work(Effect.validate(plans, (plan) => Effect.fromResult(plan).pipe(Effect.flatMap(discard)), { discard: true })))));
});

cleanup().pipe(Effect.provide(NodeServices.layer), NodeRuntime.runMain);

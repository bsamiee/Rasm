import { findPackageJSON, isBuiltin } from 'node:module';
import { homedir, userInfo } from 'node:os';
import { sep } from 'node:path';
import process from 'node:process';
import { pathToFileURL } from 'node:url';
import { Lang, parse as parseSource } from '@ast-grep/napi';
import { NodeRuntime, NodeServices, NodeSocket } from '@effect/platform-node';
import { cacheDir, createProjectGraphAsync, getOutputsForTargetAndConfiguration, workspaceRoot } from '@nx/devkit';
import { Array, Config, Console, Data, Effect, FileSystem, flow, HashMap, identity, Option, Path, type PlatformError, Predicate, Record, Result, Schema, Stream, String, Struct, Tuple } from 'effect';
import { ChildProcess, type ChildProcessSpawner } from 'effect/process';
import { parse as parseYaml } from 'yaml';
import { sqlite } from '../../../plugins/function-hooks/observation/sql.ts';

// --- [CONSTANTS] -----------------------------------------------------------------------

const PS_ROW = /^\s*(?<pid>\d+)\s+(?<ppid>\d+)\s+(?<uid>-?\d+)\s+(?<state>\S+)\s+(?:\?\?|(?<terminal>\S+))\s+(?<started>\S+ \S+\s+\S+ \S+ \S+)\s+(?<command>.*)$/u;
const JOB_ROW = /^(?<pid>\d+|-)\t\S+\t(?<label>.+)$/u;
const OPEN_PROCESS = /^p(?<pid>\d+)\0\n(?<files>[\s\S]*)$/u;
const OPEN_BLOCK = /(?<=\0\n)(?=p\d+\0)/u;
const GLOBAL_PACKAGES = /^global-packages: (?<path>.+)$/mu;
const XCODEBUILD_PATH = /(?:^|\s)-(?:derivedDataPath|packageCachePath)\s+(?<path>\S+)/gu;
const ROOTS = ['.cache', '.artifacts'];
const SUCCESS_CODES = [0];
const MATCH_CODES = [...SUCCESS_CODES, 1];

// --- [MODELS] --------------------------------------------------------------------------

const Installed = Schema.fromJsonString(Schema.Record(Schema.String, Schema.Array(Schema.Struct({ install_path: Schema.String }))));
const Agents = Schema.fromJsonString(Schema.Struct({ launchd: Schema.Struct({ agents: Schema.Array(Schema.Struct({ loaded: Schema.Boolean, label: Schema.String, path: Schema.String })) }) }));
const Registered = Schema.fromJsonString(Schema.Struct({ connection_uri: Schema.URLFromString }));
const Plugins = Schema.fromJsonString(Schema.Array(Schema.Struct({ name: Schema.String, kind: Schema.String, version: Schema.String })));
const Provider = Schema.fromJsonString(Schema.Struct({ pulumi: Schema.Struct({ resource: Schema.Literal(true), name: Schema.String }) }));
const Listed = Schema.Struct({ pid: Schema.NumberFromString, ppid: Schema.NumberFromString, uid: Schema.NumberFromString, state: Schema.String, terminal: Schema.OptionFromUndefinedOr(Schema.String), started: Schema.String, command: Schema.String });
const OpenProcess = Schema.Struct({ pid: Schema.NumberFromString, files: Schema.String });
const OpenFile = Schema.Struct({ descriptor: Schema.String, path: Schema.String });
const Located = Schema.Struct({ path: Schema.String });
const Grammars = Schema.Struct({ customLanguages: Schema.Record(Schema.String, Schema.Struct({ libraryPath: Schema.String })) });
const Workspace = Schema.Struct({ storeDir: Schema.String, cacheDir: Schema.String, enableGlobalVirtualStore: Schema.optionalKey(Schema.Boolean) });
const Invocation = Schema.Struct({ cwd: Schema.optionalKey(Schema.String), command: Schema.optionalKey(Schema.String), commands: Schema.optionalKey(Schema.Array(Schema.Union([Schema.String, Schema.Struct({ command: Schema.String })]))) });

interface Process extends Schema.Schema.Type<typeof Listed> {
    readonly files: Option.Option<readonly { readonly descriptor: string; readonly path: string }[]>;
}
interface Snapshot {
    readonly processes: HashMap.HashMap<number, Process>;
    readonly jobs: HashMap.HashMap<number, string>;
}
interface Plan {
    readonly candidates: readonly string[];
    readonly kept: readonly string[];
    readonly dropped: readonly string[];
}

// --- [ERRORS] --------------------------------------------------------------------------

class Exited extends Data.TaggedError('Exited')<{ readonly command: ChildProcess.StandardCommand; readonly code: number }> {}
class Unsignaled extends Data.TaggedError('Unsignaled')<{ readonly pid: number; readonly cause: unknown }> {}
class Unparsed extends Data.TaggedError('Unparsed')<{ readonly source: string; readonly cause: unknown }> {}
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
const lines = (text: string): string[] => text.split('\n').filter(String.isNonEmpty);

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
const listed = execute(ChildProcess.make('ps', ['-axww', '-o', 'pid=,ppid=,uid=,stat=,tty=,lstart=,command=']), SUCCESS_CODES).pipe(Effect.flatMap((text) => Effect.forEach(lines(text), (line) => Schema.decodeUnknownEffect(Listed)(PS_ROW.exec(line)?.groups))));
const snapshot = Effect.gen(function* () {
    const [processes, files, jobs] = yield* Effect.all(
        [
            listed,
            execute(ChildProcess.make('lsof', ['-nP', '-F0pfn']), MATCH_CODES).pipe(Effect.flatMap(opened)),
            execute(ChildProcess.make('launchctl', ['list']), SUCCESS_CODES).pipe(Effect.flatMap((text) => Effect.forEach(lines(text).slice(1), (line) => Schema.decodeUnknownEffect(Schema.Struct({ pid: Schema.String, label: Schema.String }))(JOB_ROW.exec(line)?.groups)))),
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
            running.filter((candidate) => Option.isSome(candidate.terminal)),
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
const zombies = (processes: HashMap.HashMap<number, Process>): string[] =>
    Record.toEntries(
        Array.groupBy(
            Array.fromIterable(HashMap.values(processes)).filter((running) => running.state.startsWith('Z')),
            (zombie) => `${zombie.ppid}`,
        ),
    ).map(([ppid, members]) => [`zombie ${members.map(Struct.get('pid')).join(',')} parent ${ppid}`, ...Option.toArray(Option.map(HashMap.get(processes, Number(ppid)), Struct.get('command')))].join(' '));
const held = ({ processes }: Snapshot, targets: readonly string[], candidates: readonly string[], dirname: (path: string) => string): { readonly directory: string; readonly holder: string }[] =>
    Array.dedupeWith(
        Array.fromIterable(HashMap.values(processes)).flatMap((running) =>
            Array.flatten(Option.toArray(running.files))
                .filter(({ path }) => under(path, targets))
                .map(({ descriptor, path }) => ({ directory: descriptor === 'cwd' || candidates.some((candidate) => under(candidate, [dirname(path)])) ? path : dirname(path), holder: `${running.pid} ${running.command}` })),
        ),
        (left, right) => left.directory === right.directory,
    );

// --- [RECOVERY]
const wait = (pids: readonly number[]): Effect.Effect<string, Exited | PlatformError.PlatformError, ChildProcessSpawner.ChildProcessSpawner> => execute(ChildProcess.make('lsof', ['-nP', '-p', pids.join(','), '+r', '1'], { stdout: 'ignore' }), MATCH_CODES);
const terminate = Effect.fn('terminate')(function* (roots: readonly string[], installed: readonly string[], tools: readonly string[], uid: number) {
    const signal = (pid: number, name: 'SIGTERM' | 'SIGKILL'): Effect.Effect<void, Unsignaled> =>
        Effect.try({ try: () => process.kill(pid, name), catch: (cause) => new Unsignaled({ pid, cause }) }).pipe(
            Effect.asVoid,
            Effect.catchIf(
                (error) => error.cause instanceof Error && 'code' in error.cause && error.cause.code === 'ESRCH',
                () => Effect.void,
            ),
        );
    const state = yield* snapshot;
    yield* Effect.forEach(zombies(state.processes), Console.log, { discard: true });
    const targets = stale(state, roots, installed, tools, process.pid, uid);
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
const plan = Effect.fn('plan')(function* (root: string) {
    const fs = yield* FileSystem.FileSystem;
    const path = yield* Path.Path;
    const read = (file: string): Effect.Effect<unknown, Unparsed | PlatformError.PlatformError> => fs.readFileString(path.join(root, file)).pipe(Effect.flatMap((text) => Effect.try({ try: () => parseYaml(text), catch: (cause) => new Unparsed({ source: file, cause }) })));
    const program = path.join(root, 'infra', 'cli.ts');
    const [graph, { customLanguages }, workspace, store, packages, uv, pulumi, plugins, providers] = yield* Effect.all(
        [
            Effect.tryPromise(() => createProjectGraphAsync({ exitOnError: false })),
            read('sgconfig.yml').pipe(Effect.flatMap(Schema.decodeUnknownEffect(Grammars))),
            read('pnpm-workspace.yaml').pipe(Effect.flatMap(Schema.decodeUnknownEffect(Workspace))),
            execute(ChildProcess.make('pnpm', ['store', 'path'], { cwd: root }), SUCCESS_CODES).pipe(Effect.map(String.trim)),
            execute(ChildProcess.make('dotnet', ['nuget', 'locals', 'global-packages', '--list'], { cwd: root }), SUCCESS_CODES).pipe(Effect.flatMap((text) => Schema.decodeUnknownEffect(Located)(GLOBAL_PACKAGES.exec(text)?.groups))),
            Config.String('UV_CACHE_DIR'),
            Config.String('PULUMI_HOME'),
            execute(ChildProcess.make('pulumi', ['plugin', 'ls', '--json']), SUCCESS_CODES).pipe(Effect.flatMap(Schema.decodeUnknownEffect(Plugins))),
            fs.readFileString(program).pipe(
                Effect.flatMap((text) => Effect.try({ try: () => parseSource(Lang.TypeScript, text).root().findAll({ rule: { kind: 'string_fragment', inside: { kind: 'string', inside: { kind: 'import_statement' } } } }), catch: (cause) => new Unparsed({ source: program, cause }) })),
                Effect.flatMap((sources) =>
                    Effect.forEach(
                        sources.filter((source) => !isBuiltin(source.text())),
                        (source) => Effect.try({ try: () => Option.fromNullishOr(findPackageJSON(source.text(), pathToFileURL(program))), catch: (cause) => new Unparsed({ source: source.text(), cause }) }),
                    ),
                ),
                Effect.flatMap((manifests) => Effect.forEach(Array.dedupe(Array.getSomes(manifests)), (manifest) => Effect.map(fs.readFileString(manifest), flow(Schema.decodeUnknownOption(Provider), Option.map(({ pulumi: { name } }) => name))))),
                Effect.map(Array.getSomes),
            ),
        ],
        { concurrency: 'unbounded' },
    );
    const nodes = Object.values(graph.nodes);
    const derived = nodes.flatMap((node) =>
        Object.values(node.data.targets ?? {}).flatMap(({ options }) =>
            Option.toArray(Schema.decodeUnknownOption(Invocation)(options)).flatMap(({ cwd, command, commands }) =>
                [...Option.toArray(Option.fromNullishOr(command)), ...(commands ?? []).map((entry) => (typeof entry === 'string' ? entry : entry.command))].flatMap((text) => Array.getSomes([...text.matchAll(XCODEBUILD_PATH)].map((match) => Option.fromNullishOr(match.groups?.path))).map((entry) => path.resolve(root, cwd ?? '.', entry.replace('$NX_WORKSPACE_ROOT', root)))),
            ),
        ),
    );
    const roots = ROOTS.map((entry) => path.join(root, entry));
    const outputs = nodes.flatMap((node) =>
        Object.entries(node.data.targets ?? {}).flatMap(([target, definition]) =>
            definition.outputs === undefined ? [] : [undefined, ...Object.keys(definition.configurations ?? {})].flatMap((configuration) => getOutputsForTargetAndConfiguration({ project: node.name, target, ...(configuration === undefined ? {} : { configuration }) }, {}, node)),
        ),
    );
    const wrangler = nodes.filter((node) => (graph.dependencies[node.name] ?? []).some(({ target }) => target === 'npm:@cloudflare/vite-plugin')).flatMap((node) => ['deploy', 'registry', 'tmp'].map((entry) => path.join(root, node.data.root, '.wrangler', entry)));
    return {
        candidates: Array.dedupe([
            ...roots,
            ...outputs
                .filter(Predicate.not(String.startsWith('!')))
                .map((output) => path.resolve(root, output))
                .filter((output) => !under(output, roots)),
            ...wrangler,
        ]),
        kept: Array.dedupe(
            [
                ...Record.values(customLanguages).map(Struct.get('libraryPath')),
                workspace.storeDir,
                workspace.cacheDir,
                packages.path,
                uv,
                cacheDir,
                ...derived,
                path.dirname(Array.lastNonEmpty(sqlite(root))),
                ...plugins.filter(({ name }) => providers.includes(name)).map(({ kind, name, version }) => path.join(pulumi, 'plugins', `${kind}-${name}-v${version}`)),
            ].map((entry) => path.resolve(root, entry)),
        ),
        dropped: workspace.enableGlobalVirtualStore === true ? [] : [path.join(store, 'links')],
    } satisfies Plan;
});
const sweep = Effect.fn('sweep')(function* ({ candidates, kept, dropped }: Plan, spared: readonly string[]) {
    const fs = yield* FileSystem.FileSystem;
    const path = yield* Path.Path;
    const retained = [...kept, ...spared];
    const descend = (entry: string): Effect.Effect<string[], PlatformError.PlatformError> =>
        fs.readDirectory(entry).pipe(
            Effect.catchIf(
                (error) => error.reason._tag === 'NotFound',
                () => Effect.succeed([]),
            ),
            Effect.flatMap((names) => Effect.forEach(names, (name) => visit(path.join(entry, name)), { concurrency: 'unbounded' })),
            Effect.map(Array.flatten),
        );
    const visit = (entry: string): Effect.Effect<string[], PlatformError.PlatformError> => {
        if (under(entry, retained)) {
            return Effect.succeed([]);
        }
        return retained.some((keep) => under(keep, [entry])) ? descend(entry) : Effect.succeed([entry]);
    };
    return [...Array.flatten(yield* Effect.forEach(candidates, visit, { concurrency: 'unbounded' })), ...dropped.filter((entry) => !spared.some((directory) => under(directory, [entry])))];
});
const discard = Effect.fn('discard')(function* (targets: readonly string[]) {
    const fs = yield* FileSystem.FileSystem;
    yield* Effect.validate(targets, (target) => Console.log(`remove ${target}`).pipe(Effect.andThen(fs.remove(target, { recursive: true, force: true }))), { concurrency: 'unbounded', discard: true });
});
const prune = Effect.validate(
    [
        [ChildProcess.make('mise', ['prune', '--yes'], { stdout: 'inherit' }), ChildProcess.make('mise', ['install'], { stdout: 'inherit' }), ChildProcess.make('mise', ['cache', 'prune'], { stdout: 'inherit' })],
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
    const workspace = yield* Effect.result(plan(root));
    const work = <E, R>(removal: Effect.Effect<void, E, R>): Effect.Effect<void, Array.NonEmptyArray<E | Effect.Error<typeof prune>>, R | Effect.Services<typeof prune>> =>
        Effect.validate([prune, removal], identity<Effect.Effect<void, E | Effect.Error<typeof prune>, R | Effect.Services<typeof prune>>>, { concurrency: 'unbounded', discard: true });
    if (process.platform !== 'darwin') {
        return yield* work(
            Effect.fromResult(workspace).pipe(
                Effect.flatMap((planned) => sweep(planned, [])),
                Effect.flatMap(discard),
            ),
        );
    }
    const { uid } = userInfo();
    const [worktrees, managed, developer, agents] = yield* Effect.all(
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
        ],
        { concurrency: 'unbounded' },
    );
    yield* execute(ChildProcess.make('dotnet', ['build-server', 'shutdown'], { cwd: root, stdout: 'inherit' }), SUCCESS_CODES);
    yield* terminate(
        worktrees,
        worktrees.flatMap((worktree) => ['node_modules', '.venv', ...ROOTS].map((entry) => path.join(worktree, entry))),
        [...managed, developer],
        uid,
    );
    const registry = yield* Effect.result(unreachable(path.join(homedir(), '.lldb')));
    const initial = Array.flatten(Option.toArray(Result.getSuccess(yield* Effect.result(Effect.fromResult(workspace).pipe(Effect.flatMap((planned) => sweep(planned, [])))))));
    const state = yield* snapshot;
    const busy = new Set(
        Array.fromIterable(HashMap.entries(state.jobs))
            .filter(([pid]) => Option.exists(Option.flatMap(HashMap.get(state.processes, pid), Struct.get('files')), (files) => files.some(({ descriptor, path: directory }) => descriptor === 'cwd' && under(directory, worktrees))))
            .map(Tuple.get(1)),
    );
    const participating = agents.filter((agent) => busy.has(agent.label));
    const labels = new Set(participating.map(Struct.get('label')));
    const pids = Array.fromIterable(HashMap.entries(state.jobs))
        .filter(([, label]) => labels.has(label))
        .flatMap(([pid]) => [pid, ...descendants(state.processes, pid).map(Struct.get('pid'))]);
    const sparing = Effect.gen(function* () {
        const planned = yield* Effect.fromResult(workspace);
        const holders = held(yield* snapshot, initial, planned.candidates, path.dirname);
        yield* Effect.forEach(holders, ({ directory, holder }) => Console.log(`spare ${directory} held by ${holder}`), { discard: true });
        return yield* discard(yield* sweep(planned, holders.map(Struct.get('directory'))));
    });
    const forgetting = Effect.fromResult(registry).pipe(Effect.flatMap(discard));
    const clearing = Effect.validate([sparing, forgetting], identity<Effect.Effect<void, Effect.Error<typeof sparing> | Effect.Error<typeof forgetting>, Effect.Services<typeof sparing> | Effect.Services<typeof forgetting>>>, { discard: true });
    return yield* withoutAgents(participating, uid, (pids.length === 0 ? Effect.void : wait(pids).pipe(Effect.asVoid)).pipe(Effect.andThen(work(clearing))));
});

cleanup().pipe(Effect.provide(NodeServices.layer), NodeRuntime.runMain);

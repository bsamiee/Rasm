import { homedir, userInfo } from 'node:os';
import process from 'node:process';
import { NodeRuntime, NodeServices } from '@effect/platform-node';
import { Array, Console, Duration, Effect, FileSystem, HashMap, Option, Path, type PlatformError, pipe, Record, Schema, type Scope, Stream, String, Struct } from 'effect';
import { Command } from 'effect/cli';
import { ChildProcess, type ChildProcessSpawner } from 'effect/process';

// --- [TYPES] ---------------------------------------------------------------------------

type Argv = Array.NonEmptyReadonlyArray<string>;

// --- [CONSTANTS] -----------------------------------------------------------------------

const PS_ROW = /^\s*(?<pid>\d+)\s+(?<ppid>\d+)\s+(?<uid>\d+)\s+(?<state>\S+)\s+(?<started>\S+ \S+\s+\S+ \S+ \S+)\s+(?<command>.*)$/u;
const LSOF_BLOCK = /\n(?=p\d)/u;
const LSOF_PID = /^p(?<pid>\d+)/u;
const LSOF_CWD = /^fcwd\nn(?<path>.*)$/mu;
const LSOF_TXT = /^ftxt\nn(?<path>.*)$/gmu;
const JOB_ROW = /^(?<pid>\d+)\t\S+\t(?<label>.+)$/u;
const WORKTREE_ROW = /^worktree (?<path>.+)$/u;

// --- [MODELS] --------------------------------------------------------------------------

const Installed = Schema.Record(Schema.String, Schema.Array(Schema.Struct({ install_path: Schema.String })));
const Agents = Schema.Struct({ launchd: Schema.Struct({ agents: Schema.Array(Schema.Struct({ label: Schema.String })) }) });

interface Process {
    readonly pid: number;
    readonly ppid: number;
    readonly uid: number;
    readonly state: string;
    readonly started: string;
    readonly command: string;
    readonly cwd: Option.Option<string>;
    readonly mapped: readonly string[];
}
interface Snapshot {
    readonly processes: HashMap.HashMap<number, Process>;
    readonly jobs: HashMap.HashMap<number, string>;
}

// --- [ERRORS] --------------------------------------------------------------------------

class Exited extends Schema.TaggedError<Exited>()('Exited', { command: Schema.String, code: Schema.Int }) {}
class Unsignaled extends Schema.TaggedError<Unsignaled>()('Unsignaled', { pid: Schema.Int, cause: Schema.Unknown }) {}

// --- [OPERATIONS] ----------------------------------------------------------------------

// --- [COMMANDS]
const run = (argv: Argv, cwd: string): Effect.Effect<void, Exited | PlatformError.PlatformError, ChildProcessSpawner.ChildProcessSpawner> =>
    Effect.scoped(Effect.flatMap(ChildProcess.make(Array.headNonEmpty(argv), Array.tailNonEmpty(argv), { cwd, stdin: 'ignore', stdout: 'inherit', stderr: 'inherit' }), (handle) => Effect.flatMap(handle.exitCode, (code) => (code === 0 ? Effect.void : Effect.fail(new Exited({ command: argv.join(' '), code }))))));
const read = (argv: Argv, accepted: readonly number[]): Effect.Effect<string, Exited | PlatformError.PlatformError, ChildProcessSpawner.ChildProcessSpawner> =>
    Effect.scoped(
        Effect.flatMap(ChildProcess.make(Array.headNonEmpty(argv), Array.tailNonEmpty(argv), { stdin: 'ignore', stderr: 'ignore' }), (handle) =>
            Effect.flatMap(Effect.all([Stream.mkString(Stream.decodeText(handle.stdout)), handle.exitCode]), ([text, code]) => (accepted.includes(code) ? Effect.succeed(text) : Effect.fail(new Exited({ command: argv.join(' '), code })))),
        ),
    );
const groups = (pattern: RegExp, text: string): Option.Option<Record<string, string | undefined>> => Option.fromNullishOr(pattern.exec(text)?.groups);
const group = (pattern: RegExp, name: string, text: string): Option.Option<string> => Option.flatMap(groups(pattern, text), (found) => Option.fromNullishOr(found[name]));
const lines = (pattern: RegExp, name: string, text: string): string[] => Array.getSomes(Array.map(text.split('\n'), (line) => group(pattern, name, line)));

// --- [SNAPSHOT]
const block = (text: string): Option.Option<readonly [number, Pick<Process, 'cwd' | 'mapped'>]> =>
    Option.map(group(LSOF_PID, 'pid', text), (pid) => [Number(pid), { cwd: group(LSOF_CWD, 'path', text), mapped: Array.getSomes(Array.map([...text.matchAll(LSOF_TXT)], (match) => Option.fromNullishOr(match.groups?.path))) }] as const);
const opened = (lsof: string): HashMap.HashMap<number, Pick<Process, 'cwd' | 'mapped'>> => HashMap.fromIterable(Array.getSomes(Array.map(lsof.split(LSOF_BLOCK), block)));
const listed = (line: string): Option.Option<Omit<Process, 'cwd' | 'mapped'>> =>
    pipe(
        groups(PS_ROW, line),
        Option.flatMap((found) => Option.all({ pid: Option.fromNullishOr(found.pid), ppid: Option.fromNullishOr(found.ppid), uid: Option.fromNullishOr(found.uid), state: Option.fromNullishOr(found.state), started: Option.fromNullishOr(found.started), command: Option.fromNullishOr(found.command) })),
        Option.map(({ pid, ppid, uid, ...rest }) => ({ pid: Number(pid), ppid: Number(ppid), uid: Number(uid), ...rest })),
    );
const loaded = (line: string): Option.Option<readonly [number, string]> => Option.map(Option.all([group(JOB_ROW, 'pid', line), group(JOB_ROW, 'label', line)] as const), ([pid, label]) => [Number(pid), label] as const);
const snapshot: Effect.Effect<Snapshot, Exited | PlatformError.PlatformError, ChildProcessSpawner.ChildProcessSpawner> = Effect.map(
    Effect.all([read(['ps', '-axww', '-o', 'pid=,ppid=,uid=,stat=,lstart=,comm='], [0]), read(['lsof', '-n', '-P', '-a', '-d', 'cwd,txt', '-Fpfn'], [0, 1]), read(['launchctl', 'list'], [0])], { concurrency: 'unbounded' }),
    ([ps, lsof, launchctl]) => {
        const files = opened(lsof);
        return {
            processes: pipe(
                ps.split('\n'),
                Array.map(listed),
                Array.getSomes,
                Array.map((running) => [running.pid, { ...running, ...Option.getOrElse(HashMap.get(files, running.pid), () => ({ cwd: Option.none(), mapped: [] })) }] as const),
                HashMap.fromIterable,
            ),
            jobs: HashMap.fromIterable(Array.getSomes(Array.map(launchctl.split('\n'), loaded))),
        };
    },
);
const ancestors = (processes: HashMap.HashMap<number, Process>, pid: number): number[] =>
    Array.unfold(pid, (current) =>
        Option.map(
            Option.filter(HashMap.get(processes, current), (running) => running.ppid !== running.pid),
            (running) => [running.pid, running.ppid],
        ),
    );
const descendants = (processes: HashMap.HashMap<number, Process>, pid: number): Process[] => {
    const children = Array.groupBy(HashMap.values(processes), (running) => `${running.ppid}`);
    const walk = (current: number): Process[] => (children[`${current}`] ?? []).flatMap((child) => [child, ...walk(child.pid)]);
    return walk(pid);
};
const under = (path: string, roots: readonly string[]): boolean => roots.some((root) => path === root || path.startsWith(`${root}/`));
const executable = (running: Process): Option.Option<string> => Array.head(running.mapped);
const stale = ({ processes, jobs }: Snapshot, roots: readonly string[], installed: readonly string[], tools: readonly string[], self: number): Process[] => {
    const harness = [`${homedir()}/.local/share/claude`];
    const running = Array.fromIterable(HashMap.values(processes));
    const kept = new Set([
        ...ancestors(processes, self),
        ...Array.flatMap(Array.fromIterable(HashMap.keys(jobs)), (pid) => Array.map(descendants(processes, pid), Struct.get('pid'))),
        ...pipe(
            running,
            Array.filter((candidate) => Option.exists(executable(candidate), (exe) => under(exe, harness))),
            Array.flatMap((candidate) => ancestors(processes, candidate.pid)),
        ),
    ]);
    const repository = (candidate: Process): boolean => Option.exists(executable(candidate), (exe) => under(exe, installed) || (under(exe, tools) && Option.exists(candidate.cwd, (cwd) => under(cwd, roots))));
    return pipe(
        running,
        Array.filter((candidate) => candidate.ppid === 1 && !HashMap.has(jobs, candidate.pid) && repository(candidate)),
        Array.map((candidate) => [candidate, ...descendants(processes, candidate.pid)]),
        Array.filter((subtree) => !subtree.some((member) => kept.has(member.pid))),
        Array.flatten,
        Array.dedupeWith((left, right) => left.pid === right.pid),
        Array.filter((target) => target.uid === userInfo().uid && !target.state.startsWith('Z')),
    );
};

// --- [SIGNALS]
const signal = (pid: number, name: 'SIGTERM' | 'SIGKILL'): Effect.Effect<void, Unsignaled> =>
    Effect.try({ try: () => process.kill(pid, name), catch: (cause) => new Unsignaled({ pid, cause }) }).pipe(
        Effect.catchIf(
            (error) => error.cause instanceof Error && 'code' in error.cause && error.cause.code === 'ESRCH',
            () => Effect.void,
        ),
    );
const grace = (targets: readonly Process[]): Duration.Duration => {
    const runtimes: Record<string, Duration.Duration> = { java: Duration.fromInputUnsafe('30 seconds'), pulumi: Duration.fromInputUnsafe('1 minute') };
    return Array.reduce(targets, Duration.fromInputUnsafe('5 seconds'), (longest, target) =>
        pipe(
            executable(target),
            Option.flatMap((exe) => Array.last(exe.split('/'))),
            Option.flatMap((name) => Record.get(runtimes, name)),
            Option.map((input) => Duration.max(longest, input)),
            Option.getOrElse(() => longest),
        ),
    );
};
const terminate = Effect.fn('terminate')(function* (roots: readonly string[], installed: readonly string[], tools: readonly string[]) {
    const targets = stale(yield* snapshot, roots, installed, tools, process.pid);
    yield* Effect.forEach(targets, (target) => Console.log(`terminate ${target.pid} ${target.command}`).pipe(Effect.andThen(signal(target.pid, 'SIGTERM'))), { discard: true });
    yield* pipe(
        Effect.sleep(grace(targets)),
        Effect.andThen(snapshot),
        Effect.flatMap((after) =>
            Effect.forEach(
                Array.filter(targets, (target) => Option.exists(HashMap.get(after.processes, target.pid), (running) => running.started === target.started)),
                (target) => Console.log(`kill ${target.pid} ${target.command}`).pipe(Effect.andThen(signal(target.pid, 'SIGKILL'))),
                { discard: true },
            ),
        ),
        Effect.when(Effect.succeed(Array.isReadonlyArrayNonEmpty(targets))),
    );
});

// --- [AGENTS]
const holders = ({ jobs, processes }: Snapshot, labels: readonly string[], held: readonly string[], lockers: readonly number[]): string[] =>
    pipe(
        HashMap.toEntries(jobs),
        Array.filter(([pid, label]) => labels.includes(label) && descendants(processes, pid).some((member) => lockers.includes(member.pid) || member.mapped.some((path) => under(path, held)))),
        Array.map(([, label]) => label),
    );
const withoutAgents = (root: string, labels: readonly string[]): Effect.Effect<void, Exited | PlatformError.PlatformError, ChildProcessSpawner.ChildProcessSpawner | Scope.Scope> =>
    Effect.addFinalizer(() => run(['mise', 'bootstrap', 'macos', 'launchd-agents', 'apply', '--yes'], root).pipe(Effect.when(Effect.succeed(Array.isReadonlyArrayNonEmpty(labels))), Effect.orDie)).pipe(
        Effect.andThen(Effect.forEach(labels, (label) => Console.log(`bootout ${label}`).pipe(Effect.andThen(run(['launchctl', 'bootout', `gui/${userInfo().uid}/${label}`], root))), { discard: true })),
    );

// --- [FILES]
const litter = Effect.fn('litter')(function* (root: string) {
    const fs = yield* FileSystem.FileSystem;
    const patterns = [
        '.artifacts/adobe',
        '.artifacts/apps',
        '.artifacts/blender/*',
        '.artifacts/blender/session/*',
        '.artifacts/dotnet',
        '.artifacts/ghidra',
        '.artifacts/ilspy',
        '.artifacts/playwright',
        '.artifacts/python/coverage',
        '.artifacts/rhino',
        '.artifacts/typescript/coverage',
        '.cache/biome/logs',
        '.cache/dotnet',
        '.cache/ghidra/cache',
        '.cache/ghidra/tmp',
        '.cache/ghidra/settings/**/*.log',
        '.cache/ghidra/settings/ghidra/*/osgi',
        '.cache/hypothesis/{constants,observed,patches,tmp,unicode_data}',
        '.cache/mypy',
        '.cache/nuget/http-cache',
        '.cache/nx/*',
        '.cache/playwright/*',
        '.cache/playwright-*',
        '.cache/pmd',
        '.cache/pnpm/cache/dlx',
        '.cache/pulumi/logs',
        '.cache/pycache',
        '.cache/pytest',
        '.cache/ruff',
        '.cache/swiftlint',
        '.cache/swiftpm',
        '.cache/typescript',
        '.cache/vite',
        '.cache/vitest',
        '.cache/wrangler/logs',
        '.cache/wrangler/registry',
        '.cache/xcode',
        '{apps,libs,plugins,tools}/**/dist',
        '{apps,libs,plugins,tools}/**/__pycache__',
        '{apps,libs,plugins,tools}/**/.wrangler/{deploy,registry,tmp}',
    ];
    const kept = ['**/node_modules', '**/.venv', '.artifacts/blender/session', '.artifacts/blender/session/*.json', '.cache/nx/cache', '.cache/nx/workspace-data', '.cache/playwright/browsers', '.cache/playwright/profile'];
    return yield* fs.glob(`{${patterns.join(',')}}`, { root, exclude: kept });
});
const discard = (entries: readonly string[]): Effect.Effect<void, PlatformError.PlatformError, FileSystem.FileSystem> =>
    Effect.flatMap(FileSystem.FileSystem, (fs) => Effect.forEach(entries, (entry) => Console.log(`remove ${entry}`).pipe(Effect.andThen(fs.remove(entry, { recursive: true, force: true }))), { concurrency: 'unbounded', discard: true }));

// --- [COMPOSITION] ---------------------------------------------------------------------

const cleanup = Effect.fn('cleanup')(function* () {
    const path = yield* Path.Path;
    const root = path.resolve(import.meta.dirname, '..', '..', '..');
    const [worktrees, tools, agents, cache] = yield* Effect.all(
        [
            Effect.map(read(['git', '-C', root, 'worktree', 'list', '--porcelain'], [0]), (text) => lines(WORKTREE_ROW, 'path', text)),
            Effect.flatMap(read(['mise', 'ls', '--installed', '--json'], [0]), (text) => Effect.map(Schema.decodeUnknownEffect(Schema.fromJsonString(Installed))(text), (installs) => Array.map(Array.flatten(Record.values(installs)), Struct.get('install_path')))),
            Effect.flatMap(read(['mise', 'bootstrap', 'macos', 'launchd-agents', 'status', '--json'], [0]), (text) => Effect.map(Schema.decodeUnknownEffect(Schema.fromJsonString(Agents))(text), (status) => Array.map(status.launchd.agents, Struct.get('label')))),
            Effect.map(read(['uv', 'cache', 'dir'], [0]), String.trim),
        ],
        { concurrency: 'unbounded' },
    );
    const installed = Array.flatMap(worktrees, (worktree) => [path.join(worktree, 'node_modules'), path.join(worktree, '.venv'), path.join(worktree, '.cache')]);
    const commands: readonly (readonly Argv[])[] = [
        [['uv', 'cache', 'prune']],
        [
            ['pnpm', 'store', 'prune'],
            ['pnpm', 'cache', 'delete'],
            ['pnpm', 'cache', 'prune'],
        ],
        [['pulumi', 'plugin', 'rm', '--all', '--yes']],
    ];
    yield* run(['dotnet', 'build-server', 'shutdown'], root);
    yield* terminate(worktrees, installed, tools);
    const entries = Array.map(yield* litter(root), (entry) => path.join(root, entry));
    const lockers = Array.map(Array.filter((yield* read(['lsof', '-t', path.join(cache, '.lock')], [0, 1])).split('\n'), String.isNonEmpty), Number);
    const labels = holders(yield* snapshot, agents, [cache, ...entries], lockers);
    yield* Effect.scoped(withoutAgents(root, labels).pipe(Effect.andThen(Effect.validate(commands, (sequence) => Effect.forEach(sequence, (argv) => run(argv, root), { discard: true }), { concurrency: 'unbounded', discard: true })), Effect.andThen(discard(entries))));
});

Command.make('cleanup', {}, cleanup).pipe(Command.run({ version: '0' }), Effect.provide(NodeServices.layer), NodeRuntime.runMain);

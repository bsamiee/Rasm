import process from 'node:process';
import { runMain } from '@effect/platform-node/NodeRuntime';
import { layer } from '@effect/platform-node/NodeServices';
import { cacheDir, getOutputsForTargetAndConfiguration, readCachedProjectGraph, workspaceRoot } from '@nx/devkit';
import { Array, Cause, Config, Console, Effect, Exit, Fiber, FileSystem, Function, flow, HashMap, identity, Option, Path, type PlatformError, Predicate, Schedule, Schema, String, Struct, Tuple } from 'effect';
import type { ChildProcess } from 'effect/process';
import { parse as parsePlist } from 'plist';
import { parse as parseYaml } from 'yaml';
import { stdout } from '../process.ts';

// --- [TYPES] ---------------------------------------------------------------------------

type Process = typeof Listed.Type & { readonly files: Option.Option<readonly { readonly descriptor: string; readonly device: Option.Option<string>; readonly path: string }[]> };
interface Observed {
    readonly processes: HashMap.HashMap<number, Process>;
    readonly jobs: HashMap.HashMap<number, string>;
    readonly descendants: (pid: number) => Process[];
}
interface Scope {
    readonly sep: string;
    readonly worktrees: readonly string[];
    readonly local: readonly string[];
    readonly toolchains: readonly string[];
    readonly engines: readonly string[];
    readonly self: Process;
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const PS_ROW = /^\s*(?<pid>\d+)\s+(?<ppid>\d+)\s+(?<pgid>\d+)\s+(?<uid>-?\d+)\s+(?<state>\S+)\s+(?:\?\??|(?<terminal>\S+))\s+(?<started>\S+ \S+\s+\S+ \S+ \S+)\s+(?<command>.*)$/u;

// --- [MODELS] --------------------------------------------------------------------------

const Listed = Schema.Struct({ pid: Schema.FiniteFromString, ppid: Schema.FiniteFromString, pgid: Schema.FiniteFromString, uid: Schema.FiniteFromString, state: Schema.String, terminal: Schema.OptionFromUndefinedOr(Schema.String), started: Schema.String, command: Schema.String });
const Launched = Schema.Struct({ WorkingDirectory: Schema.OptionFromOptionalKey(Schema.String), StandardOutPath: Schema.OptionFromOptionalKey(Schema.String), StandardErrorPath: Schema.OptionFromOptionalKey(Schema.String) });

// --- [OPERATIONS] ----------------------------------------------------------------------

// --- [EFFECTS]
const execute = (command: string, args: readonly string[], options?: ChildProcess.CommandOptions) => stdout(command, args, { ...options, stdin: 'ignore', stderr: 'inherit', env: { ...options?.env, LC_ALL: 'C' }, extendEnv: true });
const read = Effect.fn('read')(function* <A>(file: string, parse: (text: string) => A) {
    const text = yield* (yield* FileSystem.FileSystem).readFileString(file);
    return yield* Effect.try({ try: () => parse(text), catch: (cause) => new Cause.UnknownError(cause, `${file} did not parse`) });
});
const entries = Effect.fn('entries')(function* (directory: string) {
    const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
    return (yield* fs.readDirectory(directory).pipe(Effect.catchReason('PlatformError', 'NotFound', () => Effect.succeed([])))).map((name) => ({ name, path: path.join(directory, name) }));
});
const accumulate = <A, E, R>(items: Iterable<A>, f: (item: A) => Effect.Effect<unknown, E, R>): Effect.Effect<void, E, R> => Effect.forEach(items, flow(f, Effect.exit), { concurrency: 'unbounded' }).pipe(Effect.flatMap(Exit.asVoidAll));
const under =
    (sep: string, roots: readonly string[]) =>
    (entry: string): boolean =>
        roots.some((root) => entry === root || entry.startsWith(`${root}${sep}`));
const lineage = (path: Path.Path, entry: string): string[] => Array.unfold(entry, (at) => (at === path.dirname(at) ? Option.none() : Option.some([at, path.dirname(at)] as const)));
const lines = (text: string): string[] => text.split('\n').filter(String.isNonEmpty);

// --- [SNAPSHOT]
const observe = (processes: HashMap.HashMap<number, Process>, jobs: HashMap.HashMap<number, string>): Observed => {
    const children = Map.groupBy(HashMap.values(processes), Struct.get('ppid'));
    const descendants = (pid: number): Process[] => (children.get(pid) ?? []).flatMap((child) => [child, ...descendants(child.pid)]);
    return { processes, jobs, descendants };
};
const listing = execute('ps', ['-axww', '-o', 'pid=,ppid=,pgid=,uid=,stat=,tty=,lstart=,command=']).pipe(Effect.flatMap((text) => Schema.decodeUnknownEffect(Schema.Array(Listed))(lines(text).map((line) => PS_ROW.exec(line)?.groups))));
const snapshot = Effect.fn('snapshot')(function* (darwin: boolean) {
    const [processes, files, jobs] = yield* Effect.all(
        [
            listing,
            execute('lsof', ['-nP', '-F0pfdn']).pipe(
                Effect.map(
                    flow(
                        String.split('\0\n'),
                        Array.filter(String.isNonEmpty),
                        Array.map((record) => Object.fromEntries(record.split('\0').map((field) => [field.slice(0, 1), field.slice(1)]))),
                    ),
                ),
                Effect.flatMap(Schema.decodeUnknownEffect(Schema.Array(Schema.Union([Schema.Struct({ p: Schema.FiniteFromString }), Schema.Struct({ f: Schema.String, d: Schema.OptionFromOptionalKey(Schema.String), n: Schema.String })])))),
                Effect.map((records) => Map.groupBy(Array.getSomes(Array.mapAccum(records, 0, (pid, record) => ('p' in record ? ([record.p, Option.none()] as const) : ([pid, Option.some({ pid, descriptor: record.f, device: record.d, path: record.n })] as const)))[1]), Struct.get('pid'))),
            ),
            darwin
                ? execute('launchctl', ['list']).pipe(
                      Effect.flatMap((text) =>
                          Schema.decodeUnknownEffect(Schema.Array(Schema.Tuple([Schema.Union([Schema.Literal('-'), Schema.FiniteFromString]), Schema.String, Schema.String])))(
                              lines(text)
                                  .slice(1)
                                  .map((line) => line.split('\t')),
                          ),
                      ),
                  )
                : Effect.succeed([]),
        ],
        { concurrency: 'unbounded' },
    );
    return observe(HashMap.fromIterable(processes.map((running) => [running.pid, { ...running, files: Option.fromUndefinedOr(files.get(running.pid)) }] as const)), HashMap.fromIterable(jobs.flatMap(([pid, , label]) => (typeof pid === 'number' ? [[pid, label] as const] : []))));
});
const ancestors = (processes: HashMap.HashMap<number, Process>, pid: number): number[] =>
    Array.unfold(pid, (current) =>
        Option.map(
            Option.filter(HashMap.get(processes, current), (running) => running.ppid !== running.pid),
            (running) => [running.pid, running.ppid],
        ),
    );

// --- [RECOVERY]
const terminate = Effect.fn('terminate')(function* (state: Observed, { sep, worktrees, local, toolchains, engines, self }: Scope) {
    const { processes, jobs, descendants } = state;
    const send = (pid: number, signal: NodeJS.Signals | 0) =>
        Effect.try({ try: () => process.kill(pid, signal), catch: (cause) => new Cause.UnknownError(cause, `cannot signal process ${pid}`) }).pipe(
            Effect.catchIf(
                ({ cause }) => Predicate.hasProperty(cause, 'code') && cause.code === 'ESRCH',
                () => Effect.succeed(false),
            ),
        );
    const connection = Schema.decodeUnknownOption(Schema.TemplateLiteralParser([Schema.String, '->', Schema.String]));
    const running = [...HashMap.values(processes)];
    yield* Effect.forEach(
        Map.groupBy(
            running.filter((candidate) => candidate.state.startsWith('Z')),
            Struct.get('ppid'),
        ),
        ([ppid, members]) => Console.log([`zombie ${members.map(Struct.get('pid')).join(',')} parent ${ppid}`, ...Option.toArray(Option.map(HashMap.get(processes, ppid), Struct.get('command')))].join(' ')),
        { discard: true },
    );
    const kept = new Set([...ancestors(processes, self.pid), ...[...HashMap.keys(jobs)].flatMap((pid) => [pid, ...descendants(pid).map(Struct.get('pid'))]), ...running.filter((candidate) => Option.isSome(candidate.terminal)).flatMap((candidate) => ancestors(processes, candidate.pid))]);
    const owned = ({ files }: Process): boolean =>
        Option.exists(
            files,
            (handles) => handles.some(({ descriptor, path }) => descriptor === 'txt' && under(sep, local)(path)) || (handles.some(({ descriptor, path }) => descriptor === 'txt' && under(sep, toolchains)(path)) && handles.some(({ descriptor, path }) => descriptor === 'cwd' && under(sep, worktrees)(path))),
        );
    const opened = running.flatMap(({ pid, files }) => Option.getOrElse(files, Array.empty).map((file) => ({ ...file, pid })));
    const ends = Array.groupBy(
        opened.flatMap(({ device, path, pid }) => [...Option.toArray(device), path].map((key) => ({ key, pid }))),
        Struct.get('key'),
    );
    const peers = Map.groupBy(
        opened.flatMap(({ path, pid }) =>
            Option.match(connection(path), {
                onNone: () => [],
                onSome: ([source, , remote]) => (ends[source === '' ? remote : `${remote}->${source}`] ?? []).flatMap((end) => [[pid, end.pid] as const, [end.pid, pid] as const]),
            }),
        ),
        Tuple.get(0),
    );
    const targets = running
        .filter((root) => root.ppid === 1 && !(root.pgid === root.pid && owned(root) && !Option.getOrElse(root.files, Array.empty).some(({ descriptor, path }) => descriptor === 'txt' && under(sep, engines)(path))))
        .flatMap((root) => {
            const subtree = [root, ...descendants(root.pid)];
            const reached = new Set(subtree.flatMap(({ pid }) => (peers.get(pid) ?? []).map(Tuple.get(1))));
            return subtree.some(owned) && reached.isSubsetOf(new Set(subtree.map(Struct.get('pid')))) && !subtree.some((member) => kept.has(member.pid) || member.uid !== self.uid) ? subtree.filter((member) => !member.state.startsWith('Z')) : [];
        });
    yield* Effect.forEach(targets, (target) => Console.log(`terminate ${target.pid} ${target.command}`), { discard: true });
    yield* accumulate(targets, (target) => send(target.pid, 'SIGTERM'));
    const pids = targets.map(Struct.get('pid'));
    const settled = yield* Effect.filter(pids, (pid) => send(pid, 0)).pipe(Effect.repeat({ schedule: Schedule.spaced('50 millis'), until: (alive) => alive.length === 0 }), Effect.timeoutOption('5 seconds'));
    const after = Option.isSome(settled) ? HashMap.empty<number, string>() : HashMap.fromIterable((yield* listing).map((listed) => [listed.pid, listed.started] as const));
    yield* accumulate(
        targets.filter((target) => Option.contains(HashMap.get(after, target.pid), target.started)),
        (target) => Console.log(`kill ${target.pid} ${target.command}`).pipe(Effect.andThen(send(target.pid, 'SIGKILL'))),
    );
    return observe(HashMap.removeMany(processes, pids), jobs);
});

// --- [FILES]
const plan = Effect.fn('plan')(function* (root: string) {
    const path = yield* Path.Path;
    const xcode = Schema.decodeUnknownOption(Schema.Struct({ env: Schema.Struct({ XCODE_DERIVED_DATA_PATH: Schema.String }) }));
    const [graph, { customLanguages }] = yield* Effect.all([Effect.try(readCachedProjectGraph), read(path.join(root, 'sgconfig.yml'), parseYaml).pipe(Effect.flatMap(Schema.decodeUnknownEffect(Schema.Struct({ customLanguages: Schema.Record(Schema.String, Schema.Struct({ libraryPath: Schema.String })) }))))], {
        concurrency: 'unbounded',
    });
    const nodes = Object.values(graph.nodes);
    const declared = nodes.flatMap((node) => Object.entries(node.data.targets ?? {}).map(([target, definition]) => ({ node, target, definition })));
    return {
        grammars: Object.values(customLanguages).map(({ libraryPath }) => path.resolve(root, libraryPath)),
        derived: declared.flatMap(({ definition }) => Option.toArray(xcode(definition.options))).map(({ env }) => path.resolve(root, env.XCODE_DERIVED_DATA_PATH)),
        outputs: declared
            .filter(({ definition }) => definition.outputs !== undefined)
            .flatMap(({ node, target, definition }) => [undefined, ...Object.keys(definition.configurations ?? {})].flatMap((configuration) => getOutputsForTargetAndConfiguration({ project: node.name, target, ...(configuration === undefined ? {} : { configuration }) }, {}, node)))
            .filter((output) => !output.startsWith('!'))
            .map((output) => path.resolve(root, output)),
    };
});
const collect = Effect.fn('collect')(function* (candidates: readonly string[], retained: readonly string[], spared: readonly string[]) {
    const path = yield* Path.Path;
    const kept = new Set([...retained, ...spared]);
    const above = new Set([...kept].flatMap((entry) => lineage(path, path.dirname(entry))));
    const visit = (entry: string): Effect.Effect<string[], PlatformError.PlatformError, FileSystem.FileSystem | Path.Path> => {
        if (kept.has(entry)) {
            return Effect.succeed([]);
        }
        return above.has(entry)
            ? entries(entry).pipe(
                  Effect.flatMap((found) => Effect.forEach(found, ({ path: child }) => visit(child), { concurrency: 'unbounded' })),
                  Effect.map((visited) => visited.flat()),
              )
            : Effect.succeed([entry]);
    };
    return (yield* Effect.forEach(candidates, visit, { concurrency: 'unbounded' })).flat();
});
const discard = Effect.fn('discard')(function* (targets: readonly string[]) {
    const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
    yield* Effect.forEach(targets, (target) => Console.log(`remove ${target}`), { discard: true });
    return yield* accumulate(targets, (target) =>
        Effect.acquireUseRelease(
            fs.makeTempDirectory({ directory: path.dirname(target), prefix: `.${path.basename(target)}.` }),
            (detached) => fs.rename(target, path.join(detached, path.basename(target))),
            (detached) => fs.remove(detached, { recursive: true }),
        ).pipe(Effect.catchReason('PlatformError', 'NotFound', () => Effect.void)),
    );
});

// --- [COMPOSITION] ---------------------------------------------------------------------

const cleanup = Effect.fn('cleanup')(function* (self: number, darwin: boolean) {
    const [fs, path, home, ghidra, pulumi] = yield* Effect.all([FileSystem.FileSystem, Path.Path, Config.String('HOME'), Config.String('GHIDRA_PROJECT_DIR'), Config.String('PULUMI_HOME')]);
    const engines = path.join(pulumi, 'plugins');
    const root = path.resolve(workspaceRoot);
    const residues = ['.cache', '.artifacts', '.playwright-cli', '.playwright-mcp'];
    const [drivers, launchAgents] = [path.join(home, '.dylint_drivers'), path.join(home, 'Library', 'LaunchAgents')];
    const shown = { stdout: 'inherit' } as const;
    const provider = Schema.decodeUnknownOption(Schema.fromJsonString(Schema.Struct({ pulumi: Schema.Struct({ resource: Schema.Literal(true), name: Schema.String, version: Schema.String }) })));
    const plugins = Effect.all(
        [
            execute('pulumi', ['plugin', 'ls', '--json']).pipe(Effect.flatMap(Schema.decodeUnknownEffect(Schema.fromJsonString(Schema.Array(Schema.Struct({ name: Schema.String, kind: Schema.String, version: Schema.String })))))),
            fs.readFileString(path.join(root, 'package.json')).pipe(
                Effect.flatMap(Schema.decodeUnknownEffect(Schema.fromJsonString(Schema.Struct({ devDependencies: Schema.Record(Schema.String, Schema.String) })))),
                Effect.flatMap(({ devDependencies }) => Effect.forEach(Object.keys(devDependencies), (name) => Effect.map(fs.readFileString(path.join(root, 'node_modules', name, 'package.json')), provider), { concurrency: 'unbounded' })),
                Effect.map(
                    flow(
                        Array.getSomes,
                        Array.map(({ pulumi: { name, version } }) => ({ kind: 'resource', name, version })),
                    ),
                ),
            ),
        ],
        { concurrency: 'unbounded' },
    ).pipe(Effect.flatMap(([cached, providers]) => accumulate(Array.difference(cached, providers), ({ kind, name, version }) => execute('pulumi', ['plugin', 'rm', kind, name, version, '--yes'], shown))));
    const pruning = yield* Effect.forkChild(
        accumulate([execute('mise', ['cache', 'prune'], shown), execute('mise', ['daemons', 'prune', '--yes'], shown), execute('uv', ['cache', 'prune'], shown), execute('pnpm', ['store', 'prune'], shown).pipe(Effect.andThen(execute('pnpm', ['cache', 'prune'], shown))), plugins], identity),
    );
    const planning = yield* Effect.forkChild(plan(root));
    const worktree = Schema.decodeUnknownOption(Schema.TemplateLiteralParser(['worktree ', Schema.String]));
    const product = Schema.decodeUnknownOption(Schema.TemplateLiteralParser([Schema.String, ' v', Schema.String, ' (', Schema.String, '):']));
    const [worktrees, installed, developer, agents, daemons, rustup, products, members] = yield* Effect.all(
        [
            execute('git', ['worktree', 'list', '--porcelain', '-z']).pipe(Effect.map((text) => Array.getSomes(text.split('\0').map((field) => Option.map(worktree(field), Tuple.get(1)))))),
            execute('mise', ['ls', '--installed', '--json']).pipe(
                Effect.flatMap(Schema.decodeUnknownEffect(Schema.fromJsonString(Schema.Record(Schema.String, Schema.Array(Schema.Struct({ install_path: Schema.String })))))),
                Effect.flatMap((installs) => Effect.forEach(Object.values(installs).flat(), ({ install_path }) => fs.realPath(install_path).pipe(Effect.catchReason('PlatformError', 'NotFound', Function.constant(Effect.succeed(install_path)))), { concurrency: 'unbounded' })),
            ),
            darwin ? execute('xcode-select', ['--print-path']).pipe(Effect.map((text) => [text.trim()])) : Effect.succeed([]),
            darwin
                ? execute('mise', ['bootstrap', 'macos', 'launchd-agents', 'status', '--json']).pipe(
                      Effect.flatMap(Schema.decodeUnknownEffect(Schema.fromJsonString(Schema.Struct({ launchd: Schema.Struct({ agents: Schema.Array(Schema.Struct({ name: Schema.String, loaded: Schema.Boolean, label: Schema.String, path: Schema.String })) }) })))),
                      Effect.map(({ launchd }) => launchd.agents),
                  )
                : Effect.succeed([]),
            execute('mise', ['daemons', 'ls', '--json']).pipe(Effect.flatMap(Schema.decodeUnknownEffect(Schema.fromJsonString(Schema.Array(Schema.Struct({ name: Schema.String, pid: Schema.OptionFromNullOr(Schema.Finite) })))))),
            execute('rustup', ['show', 'home']).pipe(Effect.flatMap((text) => entries(path.join(text.trim(), 'toolchains')))),
            execute('cargo', ['install', '--list']).pipe(Effect.map((text) => Array.getSomes(lines(text).map((line) => product(line))))),
            execute('cargo', ['metadata', '--no-deps', '--format-version', '1']).pipe(
                Effect.flatMap(Schema.decodeUnknownEffect(Schema.fromJsonString(Schema.Struct({ packages: Schema.Array(Schema.Struct({ name: Schema.String })) })))),
                Effect.map(({ packages }) => packages.map(Struct.get('name'))),
            ),
        ],
        { concurrency: 'unbounded' },
    );
    const declared = new Set(agents.map(Struct.get('name')));
    const plist = Schema.decodeUnknownOption(Schema.TemplateLiteralParser(['dev.mise.', Schema.String, '.plist']));
    const [logs, orphaned] = yield* Effect.all(
        [
            Effect.forEach(agents.filter(Struct.get('loaded')), ({ path: file }) => read(file, parsePlist).pipe(Effect.flatMap(Schema.decodeUnknownEffect(Launched))), { concurrency: 'unbounded' }).pipe(
                Effect.map(Array.flatMap(({ WorkingDirectory, StandardOutPath, StandardErrorPath }) => (Option.contains(WorkingDirectory, root) ? [...Option.toArray(StandardOutPath), ...Option.toArray(StandardErrorPath)] : []))),
            ),
            entries(launchAgents).pipe(
                Effect.map(Array.flatMap(({ name, path: file }) => Option.toArray(plist(name)).map(([, agent]) => ({ agent, file })))),
                Effect.map(Array.filter(({ agent }) => !declared.has(agent))),
                Effect.flatMap(Effect.filter(({ file }) => read(file, parsePlist).pipe(Effect.flatMap(Schema.decodeUnknownEffect(Launched)), Effect.map(flow(Struct.get('WorkingDirectory'), Option.contains(root)))), { concurrency: 'unbounded' })),
                Effect.map(Array.map(Struct.get('agent'))),
            ),
        ],
        { concurrency: 'unbounded' },
    );
    yield* accumulate(orphaned, (agent) => execute('mise', ['bootstrap', 'services', 'remove', agent, '--yes'], shown));
    const observed = yield* snapshot(darwin);
    const owner = yield* Effect.fromOption(HashMap.get(observed.processes, self));
    const state = yield* terminate(observed, { sep: path.sep, worktrees, local: Array.cartesianWith(worktrees, ['node_modules', '.venv', ...residues], path.join), toolchains: [...installed, ...developer, ...rustup.map(Struct.get('path'))], engines: [engines], self: owner });
    const serving = new Map(daemons.flatMap(({ name, pid }) => [...Option.toArray(pid), ...Option.toArray(pid).flatMap(state.descendants).map(Struct.get('pid'))].map((member) => [member, name] as const)));
    const derived = Schema.decodeUnknownOption(Schema.Struct({ WorkspacePath: Schema.String }));
    const registration = Schema.decodeUnknownOption(Schema.TemplateLiteralParser(['lldb-mcp-', Schema.FiniteFromString, '.json']));
    const removal = Effect.gen(function* () {
        const planned = yield* Fiber.join(planning);
        const toolchains = rustup.map(Struct.get('name'));
        const strays = yield* Effect.all(
            [
                entries(drivers).pipe(Effect.flatMap(Effect.filter(({ name, path: entry }) => (toolchains.includes(name) ? Effect.succeed(false) : Effect.map(fs.stat(entry), flow(Struct.get('type'), Schema.is(Schema.Literal('Directory'))))), { concurrency: 'unbounded' }))),
                entries(path.join(home, 'Library', 'Developer', 'Xcode', 'DerivedData')).pipe(
                    Effect.flatMap(
                        Effect.filter(({ path: entry }) => read(path.join(entry, 'info.plist'), parsePlist).pipe(Effect.map(flow(derived, Option.exists(flow(Struct.get('WorkspacePath'), under(path.sep, worktrees))))), Effect.catchReason('PlatformError', 'NotFound', Function.constant(Effect.succeed(false)))), {
                            concurrency: 'unbounded',
                        }),
                    ),
                ),
                entries(path.join(home, '.lldb')).pipe(Effect.map(Array.flatMap((entry) => Array.cartesian([entry], Option.toArray(registration(entry.name))))), Effect.map(Array.filter(([, [, pid]]) => !HashMap.has(state.processes, pid))), Effect.map(Array.map(Tuple.get(0)))),
            ],
            { concurrency: 'unbounded' },
        ).pipe(Effect.map((found) => found.flat().map(Struct.get('path'))));
        const retained = Array.dedupe([...logs, ...planned.grammars, cacheDir, ...planned.derived, pulumi]);
        const listed = Array.dedupe([...residues.map((entry) => path.join(root, entry)), ghidra, ...planned.outputs, ...strays]);
        const known = new Set(listed);
        const candidates = listed.filter((candidate) => !under(path.sep, retained)(candidate) && new Set(lineage(path, path.dirname(candidate))).isDisjointFrom(known));
        const holders = Array.dedupeWith(
            [...HashMap.values(state.processes)]
                .flatMap((holder) => Array.cartesian([holder], Option.getOrElse(holder.files, Array.empty)))
                .filter(([, { path: file }]) => under(path.sep, candidates)(file) && !under(path.sep, retained)(file))
                .map(([holder, { descriptor, path: file }]) => ({ directory: descriptor === 'cwd' ? file : path.dirname(file), holder })),
            (left, right) => left.directory === right.directory && left.holder.pid === right.holder.pid,
        );
        const stopped = Array.dedupe(holders.flatMap(({ holder }) => serving.get(holder.pid) ?? []));
        const spared = holders.filter(({ holder }) => !serving.has(holder.pid));
        yield* Effect.forEach(spared, ({ directory, holder }) => Console.log(`spare ${directory} held by ${holder.pid} ${holder.command}`), { discard: true });
        const clearing = discard(yield* collect(candidates, retained, Array.dedupe(spared.map(Struct.get('directory')))));
        return yield* stopped.length === 0
            ? clearing
            : Effect.acquireUseRelease(
                  Console.log(`stop daemons ${stopped.join(' ')}`).pipe(Effect.andThen(execute('mise', ['daemons', 'stop', ...stopped], shown)), Effect.result),
                  (stop) => Effect.fromResult(stop).pipe(Effect.andThen(clearing)),
                  () => Effect.asVoid(execute('mise', ['daemons', 'start', ...stopped], shown)),
              );
    });
    const strayed = Array.dedupe(products.flatMap(([name, , , , source]) => (under(path.sep, worktrees)(source) && !members.includes(name) ? [name] : [])));
    return yield* accumulate([removal, strayed.length === 0 ? Effect.void : execute('cargo', ['uninstall', ...strayed], shown), execute('cargo', ['insta', 'reject', '--workspace', '--quiet'], shown), Fiber.join(pruning)], identity);
});

cleanup(process.pid, process.platform === 'darwin').pipe(Effect.provide(layer), runMain);

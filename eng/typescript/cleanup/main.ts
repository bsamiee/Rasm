import process from 'node:process';
import { NodeRuntime, NodeServices } from '@effect/platform-node';
import { cacheDir, getOutputsForTargetAndConfiguration, readCachedProjectGraph, workspaceRoot } from '@nx/devkit';
import { Array, Config, Console, Effect, Exit, Fiber, FileSystem, Formatter, flow, HashMap, identity, Option, Path, type PlatformError, Predicate, Schedule, Schema, Stream, String, Struct } from 'effect';
import { ChildProcess } from 'effect/process';
import { parse as parsePlist } from 'plist';
import { parse as parseYaml } from 'yaml';

// --- [TYPES] ---------------------------------------------------------------------------

type Process = typeof Listed.Type & { readonly files: Option.Option<readonly { readonly descriptor: string; readonly device: Option.Option<string>; readonly path: string }[]> };
interface Observed {
    readonly processes: HashMap.HashMap<number, Process>;
    readonly jobs: HashMap.HashMap<number, string>;
    readonly descendants: (pid: number) => Process[];
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const PS_ROW = /^\s*(?<pid>\d+)\s+(?<ppid>\d+)\s+(?<pgid>\d+)\s+(?<uid>-?\d+)\s+(?<state>\S+)\s+(?:\?\?|(?<terminal>\S+))\s+(?<started>\S+ \S+\s+\S+ \S+ \S+)\s+(?<command>.*)$/u;

// --- [MODELS] --------------------------------------------------------------------------

const Installed = Schema.fromJsonString(Schema.Record(Schema.String, Schema.Array(Schema.Struct({ install_path: Schema.String }))));
const Agents = Schema.fromJsonString(Schema.Struct({ launchd: Schema.Struct({ agents: Schema.Array(Schema.Struct({ name: Schema.String, loaded: Schema.Boolean, label: Schema.String, path: Schema.String })) }) }));
const Launched = Schema.Struct({ WorkingDirectory: Schema.OptionFromOptionalKey(Schema.String), StandardOutPath: Schema.OptionFromOptionalKey(Schema.String), StandardErrorPath: Schema.OptionFromOptionalKey(Schema.String) });
const Plist = Schema.TemplateLiteralParser(['dev.mise.', Schema.String, '.plist']);
const Derived = Schema.Struct({ WorkspacePath: Schema.String });
const Metadata = Schema.fromJsonString(Schema.Struct({ packages: Schema.Array(Schema.Struct({ name: Schema.String })) }));
const Product = Schema.TemplateLiteralParser([Schema.String, ' v', Schema.String, ' (', Schema.String, '):']);
const Listed = Schema.Struct({ pid: Schema.FiniteFromString, ppid: Schema.FiniteFromString, pgid: Schema.FiniteFromString, uid: Schema.FiniteFromString, state: Schema.String, terminal: Schema.OptionFromUndefinedOr(Schema.String), started: Schema.String, command: Schema.String });
const Opened = Schema.Union([Schema.Struct({ p: Schema.FiniteFromString }), Schema.Struct({ f: Schema.String, d: Schema.OptionFromOptionalKey(Schema.String), n: Schema.String })]);
const Job = Schema.Tuple([Schema.Union([Schema.Literal('-'), Schema.FiniteFromString]), Schema.String, Schema.String]);
const Connection = Schema.TemplateLiteralParser([Schema.String, '->', Schema.String]);
const Worktree = Schema.TemplateLiteralParser(['worktree ', Schema.String]);
const Registration = Schema.TemplateLiteralParser(['lldb-mcp-', Schema.FiniteFromString, '.json']);
const Grammars = Schema.Struct({ customLanguages: Schema.Record(Schema.String, Schema.Struct({ libraryPath: Schema.String })) });
const Xcode = Schema.Struct({ env: Schema.Struct({ XCODE_DERIVED_DATA_PATH: Schema.String }) });

// --- [ERRORS] --------------------------------------------------------------------------

const CleanupReason = Schema.TaggedUnion({
    Exited: { command: Schema.String, args: Schema.Array(Schema.String), code: Schema.Finite },
    Unparsed: { source: Schema.String, cause: Schema.Defect() },
    Ungraphed: { cause: Schema.Defect() },
    Unsignaled: { pid: Schema.Finite, cause: Schema.Defect() },
    Unexited: { pids: Schema.Array(Schema.Finite) },
});
class CleanupError extends Schema.TaggedError<CleanupError, { readonly brand: unique symbol }>()('CleanupError', { reason: CleanupReason }) {
    override get message(): string {
        return CleanupReason.match(this.reason, {
            Exited: ({ command, args, code }) => `${[command, ...args].join(' ')} exited with code ${code}`,
            Unparsed: ({ source, cause }) => `${source} did not parse: ${Formatter.format(cause)}`,
            Ungraphed: ({ cause }) => `the cached Nx project graph is unreadable: ${Formatter.format(cause)}`,
            Unsignaled: ({ pid, cause }) => `cannot signal process ${pid}: ${Formatter.format(cause)}`,
            Unexited: ({ pids }) => `processes ${pids.join(', ')} still run after their launchd jobs were booted out`,
        });
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

// --- [EFFECTS]
const execute = Effect.fn('execute')(function* (command: string, args: readonly string[], options?: ChildProcess.CommandOptions) {
    const handle = yield* ChildProcess.make(command, args, { ...options, stdin: 'ignore', stderr: 'inherit', env: { ...options?.env, LC_ALL: 'C' }, extendEnv: true });
    const [text, code] = yield* Effect.all([Stream.mkString(Stream.decodeText(handle.stdout)), handle.exitCode], { concurrency: 'unbounded' });
    return code === 0 ? text : yield* CleanupError.make({ reason: { _tag: 'Exited', command, args, code } });
}, Effect.scoped);
const read = <A>(file: string, parse: (text: string) => A): Effect.Effect<A, CleanupError | PlatformError.PlatformError, FileSystem.FileSystem> =>
    Effect.flatMap(FileSystem.FileSystem, (fs) => Effect.flatMap(fs.readFileString(file), (text) => Effect.try({ try: () => parse(text), catch: (cause) => CleanupError.make({ reason: { _tag: 'Unparsed', source: file, cause } }) })));
const entries = Effect.fn('entries')(function* (directory: string) {
    const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
    return (yield* fs.readDirectory(directory).pipe(Effect.catchReason('PlatformError', 'NotFound', () => Effect.succeed([])))).map((name) => ({ name, path: path.join(directory, name) }));
});
const accumulate = <A, E, R>(items: Iterable<A>, f: (item: A) => Effect.Effect<unknown, E, R>): Effect.Effect<void, E, R> => Effect.forEach(items, flow(f, Effect.exit), { concurrency: 'unbounded' }).pipe(Effect.flatMap(Exit.asVoidAll));
const send = (pid: number, signal: NodeJS.Signals | 0): Effect.Effect<boolean, CleanupError> =>
    Effect.try({ try: () => process.kill(pid, signal), catch: (cause) => CleanupError.make({ reason: { _tag: 'Unsignaled', pid, cause } }) }).pipe(
        Effect.catchIf(
            ({ reason }) => CleanupReason.guards.Unsignaled(reason) && Predicate.hasProperty(reason.cause, 'code') && reason.cause.code === 'ESRCH',
            () => Effect.succeed(false),
        ),
    );
const exited = (pids: readonly number[]): Effect.Effect<Option.Option<number[]>, CleanupError> => Effect.filter(pids, (pid) => send(pid, 0)).pipe(Effect.repeat({ schedule: Schedule.spaced('50 millis'), until: (alive) => alive.length === 0 }), Effect.timeoutOption('5 seconds'));
const under = (sep: string, entry: string, roots: readonly string[]): boolean => roots.some((root) => entry === root || entry.startsWith(`${root}${sep}`));
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
                Effect.flatMap((text) =>
                    Schema.decodeUnknownEffect(Schema.Array(Opened))(
                        text
                            .split('\0\n')
                            .filter(String.isNonEmpty)
                            .map((record) => Object.fromEntries(record.split('\0').map((field) => [field.slice(0, 1), field.slice(1)]))),
                    ),
                ),
                Effect.map((records) => Map.groupBy(Array.getSomes(Array.mapAccum(records, 0, (pid, record) => ('p' in record ? ([record.p, Option.none()] as const) : ([pid, Option.some({ pid, descriptor: record.f, device: record.d, path: record.n })] as const)))[1]), Struct.get('pid'))),
            ),
            darwin
                ? execute('launchctl', ['list']).pipe(
                      Effect.flatMap((text) =>
                          Schema.decodeUnknownEffect(Schema.Array(Job))(
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
const stale = (sep: string, { processes, jobs, descendants }: Observed, worktrees: readonly string[], local: readonly string[], toolchains: readonly string[], engines: readonly string[], self: Process): Process[] => {
    const running = [...HashMap.values(processes)];
    const kept = new Set([...ancestors(processes, self.pid), ...[...HashMap.keys(jobs)].flatMap((pid) => [pid, ...descendants(pid).map(Struct.get('pid'))]), ...running.filter((candidate) => Option.isSome(candidate.terminal)).flatMap((candidate) => ancestors(processes, candidate.pid))]);
    const owned = ({ files }: Process): boolean =>
        Option.exists(
            files,
            (handles) => handles.some(({ descriptor, path }) => descriptor === 'txt' && under(sep, path, local)) || (handles.some(({ descriptor, path }) => descriptor === 'txt' && under(sep, path, toolchains)) && handles.some(({ descriptor, path }) => descriptor === 'cwd' && under(sep, path, worktrees))),
        );
    const opened = running.flatMap(({ pid, files }) => Option.getOrElse(files, () => []).map((file) => ({ ...file, pid })));
    const ends = Array.groupBy(
        opened.flatMap(({ device, path, pid }) => [...Option.toArray(device), path].map((key) => ({ key, pid }))),
        Struct.get('key'),
    );
    const peers = Map.groupBy(
        opened.flatMap(({ path, pid }) =>
            Option.match(Schema.decodeUnknownOption(Connection)(path), {
                onNone: () => [],
                onSome: ([source, , remote]) => (ends[source === '' ? remote : `${remote}->${source}`] ?? []).flatMap((end) => [[pid, end.pid] as const, [end.pid, pid] as const]),
            }),
        ),
        ([pid]) => pid,
    );
    return running
        .filter((root) => root.ppid === 1 && !(root.pgid === root.pid && owned(root) && !Option.exists(root.files, (handles) => handles.some(({ descriptor, path }) => descriptor === 'txt' && under(sep, path, engines)))))
        .flatMap((root) => {
            const subtree = [root, ...descendants(root.pid)];
            const members = new Set(subtree.map(Struct.get('pid')));
            return subtree.some(owned) && !subtree.some((member) => kept.has(member.pid) || member.uid !== self.uid || (peers.get(member.pid) ?? []).some(([, peer]) => !members.has(peer))) ? subtree.filter((member) => !member.state.startsWith('Z')) : [];
        });
};
const zombies = (processes: HashMap.HashMap<number, Process>): string[] =>
    [
        ...Map.groupBy(
            [...HashMap.values(processes)].filter((running) => running.state.startsWith('Z')),
            Struct.get('ppid'),
        ),
    ].map(([ppid, members]) => [`zombie ${members.map(Struct.get('pid')).join(',')} parent ${ppid}`, ...Option.toArray(Option.map(HashMap.get(processes, ppid), Struct.get('command')))].join(' '));
const held = ({ processes }: Observed, inside: (file: string) => boolean, dirname: (path: string) => string): { readonly directory: string; readonly holder: Process }[] =>
    Array.dedupeWith(
        [...HashMap.values(processes)].flatMap((holder) =>
            Option.getOrElse(holder.files, () => [])
                .filter(({ path }) => inside(path))
                .map(({ descriptor, path }) => ({ directory: descriptor === 'cwd' ? path : dirname(path), holder })),
        ),
        (left, right) => left.directory === right.directory && left.holder.pid === right.holder.pid,
    );

// --- [RECOVERY]
const terminate = Effect.fn('terminate')(function* (sep: string, state: Observed, worktrees: readonly string[], local: readonly string[], toolchains: readonly string[], engines: readonly string[], self: Process) {
    yield* Effect.forEach(zombies(state.processes), Console.log, { discard: true });
    const targets = stale(sep, state, worktrees, local, toolchains, engines, self);
    yield* Effect.forEach(targets, (target) => Console.log(`terminate ${target.pid} ${target.command}`), { discard: true });
    yield* accumulate(targets, (target) => send(target.pid, 'SIGTERM'));
    const pids = targets.map(Struct.get('pid'));
    yield* exited(pids).pipe(
        Effect.flatMap(
            Option.match({
                onSome: () => Effect.void,
                onNone: () =>
                    Effect.flatMap(listing, (after) =>
                        accumulate(
                            targets.filter((target) => after.some((running) => running.pid === target.pid && running.started === target.started)),
                            (target) => Console.log(`kill ${target.pid} ${target.command}`).pipe(Effect.andThen(send(target.pid, 'SIGKILL'))),
                        ),
                    ),
            }),
        ),
    );
    return observe(HashMap.removeMany(state.processes, pids), state.jobs);
});

// --- [FILES]
const plan = Effect.fn('plan')(function* (root: string) {
    const path = yield* Path.Path;
    const [graph, { customLanguages }] = yield* Effect.all([Effect.try({ try: readCachedProjectGraph, catch: (cause) => CleanupError.make({ reason: { _tag: 'Ungraphed', cause } }) }), read(path.join(root, 'sgconfig.yml'), parseYaml).pipe(Effect.flatMap(Schema.decodeUnknownEffect(Grammars)))], {
        concurrency: 'unbounded',
    });
    const nodes = Object.values(graph.nodes);
    return {
        grammars: Object.values(customLanguages).map(({ libraryPath }) => path.resolve(root, libraryPath)),
        derived: nodes
            .flatMap((node) => Object.values(node.data.targets ?? {}))
            .flatMap(({ options }) => Option.toArray(Schema.decodeUnknownOption(Xcode)(options)))
            .map(({ env }) => path.resolve(root, env.XCODE_DERIVED_DATA_PATH)),
        outputs: nodes.flatMap((node) =>
            Object.entries(node.data.targets ?? {}).flatMap(([target, definition]) =>
                definition.outputs === undefined
                    ? []
                    : [undefined, ...Object.keys(definition.configurations ?? {})]
                          .flatMap((configuration) => getOutputsForTargetAndConfiguration({ project: node.name, target, ...(configuration === undefined ? {} : { configuration }) }, {}, node))
                          .filter((output) => !output.startsWith('!'))
                          .map((output) => path.resolve(root, output)),
            ),
        ),
        cloudflare: nodes.filter((node) => node.data.tags?.includes('output:cloudflare') === true).map((node) => path.join(root, node.data.root, '.cloudflare')),
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
    const fs = yield* FileSystem.FileSystem;
    yield* Effect.forEach(targets, (target) => Console.log(`remove ${target}`), { discard: true });
    return yield* accumulate(targets, (target) => fs.remove(target, { recursive: true, force: true }));
});

// --- [COMPOSITION] ---------------------------------------------------------------------

const cleanup = Effect.fn('cleanup')(function* (self: number, darwin: boolean) {
    const [fs, path, home, ghidra, pulumi] = yield* Effect.all([FileSystem.FileSystem, Path.Path, Config.String('HOME'), Config.String('GHIDRA_PROJECT_DIR'), Config.String('PULUMI_HOME')]);
    const engines = path.join(pulumi, 'plugins');
    const root = path.resolve(workspaceRoot);
    const residues = ['.cache', '.artifacts', '.playwright-cli', '.playwright-mcp'];
    const [drivers, launchAgents] = [path.join(home, '.dylint_drivers'), path.join(home, 'Library', 'LaunchAgents')];
    const shown = { stdout: 'inherit' } as const;
    const pruning = yield* Effect.forkChild(accumulate([execute('mise', ['cache', 'prune'], shown), execute('uv', ['cache', 'prune'], shown), execute('pnpm', ['store', 'prune'], shown).pipe(Effect.andThen(execute('pnpm', ['cache', 'prune'], shown)))], identity));
    const planning = yield* Effect.forkChild(plan(root));
    const [worktrees, installed, developer, agents, rustup, products, members] = yield* Effect.all(
        [
            execute('git', ['worktree', 'list', '--porcelain', '-z']).pipe(Effect.map((text) => Array.getSomes(text.split('\0').map((field) => Option.map(Schema.decodeUnknownOption(Worktree)(field), ([, worktree]) => worktree))))),
            execute('mise', ['ls', '--installed', '--json']).pipe(
                Effect.flatMap(Schema.decodeUnknownEffect(Installed)),
                Effect.flatMap((installs) => Effect.forEach(Object.values(installs).flat(), ({ install_path }) => fs.realPath(install_path).pipe(Effect.catchReason('PlatformError', 'NotFound', () => Effect.succeed(install_path))), { concurrency: 'unbounded' })),
            ),
            darwin ? execute('xcode-select', ['--print-path']).pipe(Effect.map((text) => [text.trim()])) : Effect.succeed([]),
            darwin
                ? execute('mise', ['bootstrap', 'macos', 'launchd-agents', 'status', '--json']).pipe(
                      Effect.flatMap(Schema.decodeUnknownEffect(Agents)),
                      Effect.map(({ launchd }) => launchd.agents),
                  )
                : Effect.succeed([]),
            execute('rustup', ['show', 'home']).pipe(Effect.flatMap((text) => entries(path.join(text.trim(), 'toolchains')))),
            execute('cargo', ['install', '--list']).pipe(Effect.map((text) => Array.getSomes(lines(text).map((line) => Schema.decodeUnknownOption(Product)(line))))),
            execute('cargo', ['metadata', '--no-deps', '--format-version', '1']).pipe(
                Effect.flatMap(Schema.decodeUnknownEffect(Metadata)),
                Effect.map(({ packages }) => packages.map(Struct.get('name'))),
            ),
        ],
        { concurrency: 'unbounded' },
    );
    const [loaded, orphaned] = yield* Effect.all(
        [
            Effect.forEach(
                agents.filter(Struct.get('loaded')),
                (agent) =>
                    read(agent.path, parsePlist).pipe(
                        Effect.flatMap(Schema.decodeUnknownEffect(Launched)),
                        Effect.map(({ WorkingDirectory, StandardOutPath, StandardErrorPath }) =>
                            Option.map(
                                Option.filter(WorkingDirectory, (directory) => directory === root),
                                () => ({ ...agent, logs: [...Option.toArray(StandardOutPath), ...Option.toArray(StandardErrorPath)] }),
                            ),
                        ),
                    ),
                { concurrency: 'unbounded' },
            ).pipe(Effect.map(Array.getSomes)),
            entries(launchAgents).pipe(
                Effect.map((found) => Array.getSomes(found.map(({ name, path: plist }) => Option.map(Schema.decodeUnknownOption(Plist)(name), ([, agent]) => ({ agent, plist })))).filter(({ agent }) => !agents.some(({ name }) => name === agent))),
                Effect.flatMap(
                    Effect.filter(
                        ({ plist }) =>
                            read(plist, parsePlist).pipe(
                                Effect.flatMap(Schema.decodeUnknownEffect(Launched)),
                                Effect.map(({ WorkingDirectory }) => Option.exists(WorkingDirectory, (directory) => directory === root)),
                            ),
                        { concurrency: 'unbounded' },
                    ),
                ),
                Effect.map(Array.map(Struct.get('agent'))),
            ),
        ],
        { concurrency: 'unbounded' },
    );
    yield* accumulate(orphaned, (agent) => execute('mise', ['bootstrap', 'services', 'remove', agent, '--yes'], shown));
    const observed = yield* snapshot(darwin);
    const owner = yield* Effect.fromOption(HashMap.get(observed.processes, self));
    const state = yield* terminate(path.sep, observed, worktrees, Array.cartesianWith(worktrees, ['node_modules', '.venv', ...residues], path.join), [...installed, ...developer, ...rustup.map(Struct.get('path'))], [engines], owner);
    const removal = Effect.gen(function* () {
        const planned = yield* Fiber.join(planning);
        const toolchains = rustup.map(Struct.get('name'));
        const strays = yield* Effect.all(
            [
                entries(drivers).pipe(Effect.flatMap(Effect.filter(({ name, path: entry }) => (toolchains.includes(name) ? Effect.succeed(false) : Effect.map(fs.stat(entry), ({ type }) => type === 'Directory')), { concurrency: 'unbounded' }))),
                entries(path.join(home, 'Library', 'Developer', 'Xcode', 'DerivedData')).pipe(
                    Effect.flatMap(
                        Effect.filter(
                            ({ path: entry }) =>
                                read(path.join(entry, 'info.plist'), parsePlist).pipe(
                                    Effect.map(
                                        flow(
                                            Schema.decodeUnknownOption(Derived),
                                            Option.exists(({ WorkspacePath }) => under(path.sep, WorkspacePath, worktrees)),
                                        ),
                                    ),
                                    Effect.catchReason('PlatformError', 'NotFound', () => Effect.succeed(false)),
                                ),
                            { concurrency: 'unbounded' },
                        ),
                    ),
                ),
                entries(path.join(home, '.lldb')).pipe(Effect.map((found) => found.filter(({ name }) => Option.exists(Schema.decodeUnknownOption(Registration)(name), ([, pid]) => !HashMap.has(state.processes, pid))))),
            ],
            { concurrency: 'unbounded' },
        ).pipe(Effect.map((found) => found.flat().map(Struct.get('path'))));
        const retained = Array.dedupe([...loaded.flatMap(Struct.get('logs')), ...planned.grammars, cacheDir, ...planned.derived, pulumi]);
        const listed = Array.dedupe([...residues.map((entry) => path.join(root, entry)), ghidra, ...planned.outputs, ...planned.cloudflare, ...strays]);
        const candidates = listed.filter((candidate) => !(under(path.sep, candidate, retained) || listed.some((other) => other !== candidate && under(path.sep, candidate, [other]))));
        const [kept, targeted] = [new Set(retained), new Set(candidates)];
        const holders = held(
            state,
            (file) =>
                Option.exists(
                    Array.findFirst(lineage(path, file), (at) => kept.has(at) || targeted.has(at)),
                    (at) => targeted.has(at),
                ),
            path.dirname,
        );
        const owned = loaded.flatMap((agent) => {
            const pids = [...HashMap.entries(state.jobs)].flatMap(([pid, label]) => (label === agent.label ? [pid, ...state.descendants(pid).map(Struct.get('pid'))] : []));
            return holders.some(({ holder }) => pids.includes(holder.pid)) ? [{ ...agent, pids }] : [];
        });
        const spared = holders.filter(({ holder }) => !owned.some(({ pids }) => pids.includes(holder.pid)));
        yield* Effect.forEach(spared, ({ directory, holder }) => Console.log(`spare ${directory} held by ${holder.pid} ${holder.command}`), { discard: true });
        const clearing = discard(yield* collect(candidates, retained, Array.dedupe(spared.map(Struct.get('directory')))));
        const serving = owned.flatMap(Struct.get('pids'));
        return yield* owned.length === 0
            ? clearing
            : Effect.acquireUseRelease(
                  Effect.forEach(owned, ({ label }) => Console.log(`bootout ${label}`), { discard: true }).pipe(Effect.andThen(accumulate(owned, ({ label }) => execute('launchctl', ['bootout', `gui/${owner.uid}/${label}`]))), Effect.result),
                  (bootout) =>
                      Effect.fromResult(bootout).pipe(
                          Effect.andThen(exited(serving)),
                          Effect.filterOrElse(Option.isSome, () =>
                              Effect.flatMap(
                                  Effect.filter(serving, (pid) => send(pid, 0)),
                                  (alive) => Effect.fail(CleanupError.make({ reason: { _tag: 'Unexited', pids: alive } })),
                              ),
                          ),
                          Effect.andThen(clearing),
                      ),
                  () => execute('mise', ['bootstrap', 'macos', 'launchd-agents', 'apply', '--yes'], shown),
              );
    });
    const strayed = Array.dedupe(products.flatMap(([name, , , , source]) => (under(path.sep, source, worktrees) && !members.includes(name) ? [name] : [])));
    return yield* accumulate([removal, strayed.length === 0 ? Effect.void : execute('cargo', ['uninstall', ...strayed], shown), execute('cargo', ['insta', 'reject', '--workspace', '--quiet'], shown), Fiber.join(pruning)], identity);
});

cleanup(process.pid, process.platform === 'darwin').pipe(Effect.provide(NodeServices.layer), NodeRuntime.runMain);

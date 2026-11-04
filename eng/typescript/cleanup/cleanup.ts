import process from 'node:process';
import { cacheDir, getOutputsForTargetAndConfiguration, readCachedProjectGraph, workspaceRoot } from '@nx/devkit';
import { Array, Cause, Config, Console, Effect, Equal, Fiber, FileSystem, Function, flow, identity, Match, Option, Path, type PlatformError, Predicate, Record, Result, Schedule, Schema, String, Struct, Trie, Tuple } from 'effect';
import { accumulate } from '#/eng/typescript/accumulate';
import { decodeFile, Package } from '#/eng/typescript/manifest';
import { runCommand } from '#/eng/typescript/run-command';
import { TextFormat } from '#/eng/typescript/text-format';
import manifest from '#/package.json' with { type: 'json' };

// --- [CONSTANTS] -----------------------------------------------------------------------

const PS_ROW = /^\s*(?<pid>\d+)\s+(?<ppid>\d+)\s+(?<uid>-?\d+)\s+(?<state>\S+)\s+(?:\?\??|(?<terminal>\S+))\s+(?<started>\S+ \S+\s+\S+ \S+ \S+)\s+(?<command>.*)$/u;
const RESIDUES = ['.build', '.cache', '.artifacts', '.playwright-cli', '.playwright-mcp'];
const AGENT = 'dev.mise.';

// --- [MODELS] --------------------------------------------------------------------------

const IntFromString = Schema.FiniteFromString.check(Schema.isInt());
const PsRow = TextFormat('ps row', (line) => PS_ROW.exec(line)?.groups).pipe(Schema.decodeTo(Schema.Struct({ pid: IntFromString, ppid: IntFromString, uid: IntFromString, state: Schema.String, terminal: Schema.OptionFromUndefinedOr(Schema.String), started: Schema.String, command: Schema.String })));
const LsofRecord = Schema.Union([
    Schema.Struct({ _tag: Schema.tagDefaultOmit('Process'), pid: IntFromString }).pipe(Schema.encodeKeys({ pid: 'p' })),
    Schema.Struct({ _tag: Schema.tagDefaultOmit('File'), descriptor: Schema.String, device: Schema.OptionFromOptionalKey(Schema.String), path: Schema.String }).pipe(Schema.encodeKeys({ descriptor: 'f', device: 'd', path: 'n' })),
]);
const LsofRecords = Schema.Array(LsofRecord);
const LaunchdPlist = Schema.Struct({ Label: Schema.TemplateLiteralParser([AGENT, Schema.String]), WorkingDirectory: Schema.String, StandardOutPath: Schema.OptionFromOptionalKey(Schema.String), StandardErrorPath: Schema.OptionFromOptionalKey(Schema.String) });
const LaunchdAgents = Schema.fromJsonString(Schema.Struct({ launchd: Schema.Struct({ agents: Schema.Array(Schema.Struct({ name: Schema.String, loaded: Schema.Boolean })) }) }));
const MiseInstalls = Schema.fromJsonString(Schema.Record(Schema.String, Schema.Array(Schema.Struct({ install_path: Schema.String }))));
const MiseDaemons = Schema.fromJsonString(Schema.Array(Schema.Struct({ name: Schema.String, pid: Schema.OptionFromNullOr(Schema.Int) })));
const CargoMetadata = Schema.fromJsonString(Schema.Struct({ packages: Schema.Array(Schema.Struct({ name: Schema.String })), build_directory: Schema.String }));
const SgConfig = TextFormat.Yaml.pipe(Schema.decodeTo(Schema.Struct({ customLanguages: Schema.Record(Schema.String, Schema.Struct({ libraryPath: Schema.String })) })));
const Editor = Schema.fromJsonString(Schema.Struct({ sdk: Schema.String }).pipe(Schema.encodeKeys({ sdk: 'js/ts.tsdk.path' })));
const PulumiPlugins = Schema.fromJsonString(Schema.Array(Schema.Struct({ name: Schema.String, kind: Schema.String, version: Schema.String })));
const PulumiProvider = Schema.fromJsonString(Schema.Struct({ pulumi: Schema.Struct({ resource: Schema.Literal(true), name: Schema.String, version: Schema.String }) }));
const XcodeOptions = Schema.Struct({ env: Schema.Struct({ XCODE_DERIVED_DATA_PATH: Schema.String }) });
const DerivedInfo = Schema.Struct({ WorkspacePath: Schema.String });
const AgentFile = Schema.TemplateLiteral([AGENT, Schema.String, '.plist']);
const Worktree = Schema.TemplateLiteralParser(['worktree ', Schema.String]);
const Product = Schema.TemplateLiteralParser([Schema.String, ' v', Schema.String, ' (', Schema.String, '):']);
const Job = Schema.TemplateLiteralParser([Schema.Finite, '\t', Schema.String]);
const Connection = Schema.TemplateLiteralParser([Schema.String, '->', Schema.String]);
const Registration = Schema.TemplateLiteralParser(['lldb-mcp-', Schema.Finite, '.json']);

// --- [OPERATIONS] ----------------------------------------------------------------------

const entries = Effect.fn('entries')(function* (directory: string) {
    const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
    const names = yield* fs.readDirectory(directory).pipe(Effect.catchReason('PlatformError', 'NotFound', () => Effect.succeed([])));
    return names.map((name) => ({ name, path: path.join(directory, name) }));
});
const metadata = (cwd: string) => runCommand('cargo', ['metadata', '--no-deps', '--format-version', '1'], { cwd }).pipe(Effect.flatMap(Schema.decodeEffect(CargoMetadata)));
const attribute = (pid: number, record: typeof LsofRecord.Type) => Match.valueTags(record, { Process: (owner) => [owner.pid, Option.none()] as const, File: (file) => [pid, Option.some({ ...file, pid })] as const });
const listing = runCommand.records('\n', 'ps', ['-axww', '-o', 'pid=,ppid=,uid=,stat=,tty=,lstart=,command='], { env: { LC_ALL: 'C' }, extendEnv: true }).pipe(Effect.flatMap(Schema.decodeEffect(Schema.Array(PsRow))));
const signal = (pid: number, value: NodeJS.Signals | 0) =>
    Effect.try({ try: () => process.kill(pid, value), catch: (cause) => new Cause.UnknownError(cause, `Signal ${value} to PID ${pid} failed`) }).pipe(
        Effect.catchIf(
            ({ cause }) => Predicate.hasProperty(cause, 'code') && cause.code === 'ESRCH',
            () => Effect.succeed(false),
        ),
    );

const cleanup = Effect.gen(function* () {
    const [fs, path, home, ghidra, pulumi] = yield* Effect.all([FileSystem.FileSystem, Path.Path, Config.String('HOME'), Config.String('GHIDRA_PROJECT_DIR'), Config.String('PULUMI_HOME')]);
    const darwin = process.platform === 'darwin';
    const root = path.resolve(workspaceRoot);
    const under = (roots: readonly string[]) => {
        const prefixes = Trie.fromIterable(roots.map((directory) => [path.join(directory, path.sep), undefined] as const));
        return (entry: string) => Option.isSome(Trie.longestPrefixOf(prefixes, path.join(entry, path.sep)));
    };
    const lineage = (entry: string) => Array.unfold(path.dirname(entry), (at) => (at === path.dirname(at) ? Option.none() : Option.some([at, path.dirname(at)] as const)));
    const pruning = yield* Effect.forkChild(
        accumulate(
            [
                runCommand.shown('mise', ['cache', 'prune']),
                runCommand.shown('mise', ['daemons', 'prune', '--yes']),
                runCommand.shown('uv', ['cache', 'prune']),
                runCommand.shown('pnpm', ['store', 'prune']).pipe(Effect.andThen(runCommand.shown('pnpm', ['cache', 'prune']))),
                runCommand.shown('cargo', ['insta', 'reject', '--workspace', '--quiet']),
                Effect.all(
                    [
                        runCommand('pulumi', ['plugin', 'ls', '--json']).pipe(Effect.flatMap(Schema.decodeEffect(PulumiPlugins)), Effect.map(Array.filter(({ kind }) => kind === 'resource'))),
                        Effect.forEach(Object.keys(manifest.devDependencies), (name) => Effect.map(fs.readFileString(path.join(root, 'node_modules', name, Package.file)), Schema.decodeUnknownOption(PulumiProvider)), { concurrency: 'unbounded' }).pipe(
                            Effect.map(Array.getSomes),
                            Effect.map(Array.map(({ pulumi: { name, version } }) => ({ kind: 'resource', name, version }))),
                        ),
                    ],
                    { concurrency: 'unbounded' },
                ).pipe(
                    Effect.map(([cached, used]) => Array.difference(cached, used)),
                    Effect.flatMap(accumulate(({ kind, name, version }) => runCommand.shown('pulumi', ['plugin', 'rm', kind, name, version, '--yes']), { concurrency: 'unbounded' })),
                ),
            ],
            identity,
            { concurrency: 'unbounded' },
        ),
    );
    const { graph, languages, sdk, worktrees, installed, developer, agents, launched, daemons, toolchains, products, own } = yield* Effect.all(
        {
            graph: Effect.try(readCachedProjectGraph),
            languages: decodeFile(SgConfig, root, 'sgconfig.yml').pipe(Effect.map(flow(Struct.get('customLanguages'), Record.values)), Effect.map(Array.map(({ libraryPath }) => path.resolve(root, libraryPath)))),
            sdk: Effect.map(decodeFile(Editor, root, '.vscode/settings.json'), ({ sdk: directory }) => path.resolve(root, directory)),
            worktrees: runCommand
                .records('\0\0', 'git', ['worktree', 'list', '--porcelain', '-z'])
                .pipe(Effect.map(flow(Array.filter(Predicate.not(String.includes('\0prunable'))), Array.flatMap(String.split('\0')))), Effect.map(Array.filterMap((field) => Schema.decodeUnknownResult(Worktree)(field))), Effect.map(Array.map(Tuple.get(1)))),
            installed: runCommand('mise', ['ls', '--installed', '--json']).pipe(Effect.flatMap(Schema.decodeEffect(MiseInstalls)), Effect.map(flow(Record.values, Array.flatten)), Effect.flatMap(Effect.forEach(flow(Struct.get('install_path'), fs.realPath), { concurrency: 'unbounded' }))),
            developer: darwin ? runCommand('xcode-select', ['--print-path']).pipe(Effect.map((text) => [text.trim()])) : Effect.succeed([]),
            agents: darwin
                ? runCommand('mise', ['bootstrap', 'macos', 'launchd-agents', 'status', '--json']).pipe(Effect.flatMap(Schema.decodeEffect(LaunchdAgents)), Effect.map(flow(Struct.get('launchd'), Struct.get('agents'))), Effect.map(Record.fromIterableWith(({ name, loaded }) => [name, loaded])))
                : Effect.succeed(Record.empty<string, boolean>()),
            launched: entries(path.join(home, 'Library', 'LaunchAgents')).pipe(
                Effect.map(Array.filter(flow(Struct.get('name'), Schema.is(AgentFile)))),
                Effect.flatMap(Effect.forEach(({ path: file }) => Effect.map(decodeFile(TextFormat.Plist, file), Schema.decodeUnknownOption(LaunchdPlist)), { concurrency: 'unbounded' })),
                Effect.map(Array.getSomes),
                Effect.map(Array.filter(({ WorkingDirectory }) => WorkingDirectory === root)),
            ),
            daemons: runCommand('mise', ['daemons', 'ls', '--json']).pipe(Effect.flatMap(Schema.decodeEffect(MiseDaemons))),
            toolchains: runCommand('rustup', ['show', 'home']).pipe(Effect.flatMap((text) => entries(path.join(text.trim(), 'toolchains')))),
            products: runCommand.records('\n', 'cargo', ['install', '--list']).pipe(Effect.map(Array.filterMap((line) => Schema.decodeUnknownResult(Product)(line)))),
            own: metadata(root),
        },
        { concurrency: 'unbounded' },
    );
    const checkouts = yield* Effect.filter(worktrees, (checkout) => fs.exists(path.join(checkout, 'Cargo.toml')), { concurrency: 'unbounded' }).pipe(Effect.flatMap(Effect.forEach(metadata, { concurrency: 'unbounded' })));
    yield* accumulate(
        launched.filter(({ Label: [, name] }) => !Record.has(agents, name)),
        ({ Label: [, name] }) => runCommand.shown('mise', ['bootstrap', 'services', 'remove', name, '--yes']),
        { concurrency: 'unbounded' },
    );
    const [rows, opened, jobs] = yield* Effect.all(
        [
            listing,
            runCommand
                .records('\0\n', 'lsof', ['-nP', '-F0pfdn'])
                .pipe(Effect.map(Array.map(String.split('\0'))), Effect.map(Array.map(Record.fromIterableWith((field) => [field.slice(0, 1), field.slice(1)]))), Effect.flatMap(Schema.decodeUnknownEffect(LsofRecords)), Effect.map(flow(Array.mapAccum(0, attribute), Tuple.get(1), Array.getSomes))),
            darwin ? runCommand.records('\n', 'launchctl', ['list']).pipe(Effect.map(Array.filterMap((line) => Schema.decodeUnknownResult(Job)(line))), Effect.map(Array.map(Tuple.get(0)))) : Effect.succeed([]),
        ],
        { concurrency: 'unbounded' },
    );
    const files = Map.groupBy(opened, Struct.get('pid'));
    const running = rows.map((row) => ({ ...row, opened: files.get(row.pid) ?? [] }));
    const processes = new Map(running.map((row) => [row.pid, row] as const));
    const children = Map.groupBy(running, Struct.get('ppid'));
    const descendants = (pid: number): (typeof running)[number][] => (children.get(pid) ?? []).flatMap((child) => [child, ...descendants(child.pid)]);
    const family = (pid: number) => [pid, ...descendants(pid).map(Struct.get('pid'))];
    const ancestors = (pid: number) =>
        Array.unfold(
            pid,
            flow(
                processes.get.bind(processes),
                Option.fromUndefinedOr,
                Option.filter(({ pid: current, ppid }) => ppid !== current),
                Option.map(({ pid: current, ppid }) => [current, ppid] as const),
            ),
        );
    const holds = (candidate: (typeof running)[number], descriptor: 'txt' | 'cwd', within: Predicate.Predicate<string>) => candidate.opened.some((handle) => handle.descriptor === descriptor && within(handle.path));
    const local = under([...Array.cartesianWith(worktrees, ['node_modules', '.venv', ...RESIDUES], path.join), ...checkouts.map(Struct.get('build_directory'))]);
    const tools = under([...installed, ...developer, ...toolchains.map(Struct.get('path'))]);
    const workspaces = under(worktrees);
    const owned = (candidate: (typeof running)[number]) => holds(candidate, 'txt', local) || (holds(candidate, 'txt', tools) && holds(candidate, 'cwd', workspaces));
    const { uid } = yield* Effect.fromOption(Option.fromUndefinedOr(processes.get(process.pid)), () => new Cause.NoSuchElementError(`ps lists no PID ${process.pid}`));
    const supervised = [...jobs, ...Array.getSomes(daemons.map(Struct.get('pid')))];
    const exempt = new Set([...ancestors(process.pid), ...supervised.flatMap(family), ...running.filter(({ terminal }) => Option.isSome(terminal)).flatMap(({ pid }) => ancestors(pid))]);
    const ends = Array.groupBy(
        opened.flatMap(({ device, path: name, pid }) => Array.cartesian([...Option.toArray(device), name], [pid])),
        Tuple.get(0),
    );
    const links = Array.filterMap(opened, ({ pid, path: file }) => Result.map(Schema.decodeUnknownResult(Connection)(file), Tuple.appendElement(pid))).map(([source, , remote, pid]) => ({ pid, key: source === '' ? remote : `${remote}->${source}` }));
    const peers = Map.groupBy(
        links.flatMap(({ pid, key }) => Array.cartesian([pid], Array.map(ends[key] ?? [], Tuple.get(1)))).flatMap(([left, right]) => [[left, right] as const, [right, left] as const]),
        Tuple.get(0),
    );
    const severable = (group: readonly (typeof running)[number][]) => group.some(owned) && !group.some((member) => exempt.has(member.pid) || member.uid !== uid) && new Set(group.flatMap(({ pid }) => (peers.get(pid) ?? []).map(Tuple.get(1)))).isSubsetOf(new Set(group.map(Struct.get('pid'))));
    const targets = running
        .filter((leader) => leader.ppid === 1)
        .map((leader) => [leader, ...descendants(leader.pid)])
        .filter(severable)
        .flat()
        .filter(({ state }) => !state.startsWith('Z'));
    const zombies = Map.groupBy(
        running.filter(({ state }) => state.startsWith('Z')),
        Struct.get('ppid'),
    );
    yield* Effect.forEach(
        running.filter(({ pid }) => zombies.has(pid)),
        (parent) => Console.log(`zombie ${(zombies.get(parent.pid) ?? []).map(Struct.get('pid')).join(',')} parent ${parent.pid} ${parent.command}`),
        { discard: true },
    );
    yield* accumulate(targets, (target) => Console.log(`terminate ${target.pid} ${target.command}`).pipe(Effect.andThen(signal(target.pid, 'SIGTERM'))), { concurrency: 'unbounded' });
    const settled = yield* Effect.filter(targets.map(Struct.get('pid')), (pid) => signal(pid, 0)).pipe(Effect.repeat({ schedule: Schedule.spaced('50 millis'), until: Array.isArrayEmpty }), Effect.timeoutOption('5 seconds'));
    const relisted = new Map((Option.isSome(settled) ? [] : yield* listing).map(({ pid, started }) => [pid, started] as const));
    yield* accumulate(
        targets.filter(({ pid, started }) => relisted.get(pid) === started),
        (target) => Console.log(`kill ${target.pid} ${target.command}`).pipe(Effect.andThen(signal(target.pid, 'SIGKILL'))),
        { concurrency: 'unbounded' },
    );
    const gone = new Set(targets.map(Struct.get('pid')));
    const alive = running.filter(({ pid }) => !gone.has(pid));
    const serving = new Map(daemons.flatMap(({ name, pid }) => Array.cartesianWith(Array.flatMap(Option.toArray(pid), family), [name], Tuple.make)));
    const workspace = (entry: string) =>
        decodeFile(TextFormat.Plist, entry, 'info.plist').pipe(
            Effect.map(flow(Schema.decodeUnknownOption(DerivedInfo), Option.exists(flow(Struct.get('WorkspacePath'), workspaces)))),
            Effect.catchReason('PlatformError', 'NotFound', () => Effect.succeed(false)),
        );
    const toolchainNames = new Set(toolchains.map(Struct.get('name')));
    const strays = yield* Effect.all(
        [
            entries(path.join(home, '.dylint_drivers')).pipe(Effect.flatMap(Effect.filter(({ name, path: entry }) => (toolchainNames.has(name) ? Effect.succeed(false) : Effect.map(fs.stat(entry), flow(Struct.get('type'), Equal.equals('Directory')))), { concurrency: 'unbounded' }))),
            entries(path.join(home, 'Library', 'Developer', 'Xcode', 'DerivedData')).pipe(Effect.flatMap(Effect.filter(({ path: entry }) => workspace(entry), { concurrency: 'unbounded' }))),
            entries(ghidra),
            entries(path.join(home, '.lldb')).pipe(Effect.map(Array.filter(flow(Struct.get('name'), Schema.decodeUnknownOption(Registration), Option.map(Tuple.get(1)), Option.exists(Predicate.or(Predicate.not(processes.has.bind(processes)), gone.has.bind(gone))))))),
        ],
        { concurrency: 'unbounded' },
    ).pipe(Effect.map(flow(Array.flatten, Array.map(Struct.get('path')))));
    const tasks = Object.values(graph.nodes).flatMap((node) => Record.collect(node.data.targets ?? {}, Tuple.make).map(Tuple.appendElement(node)));
    const outputs = tasks
        .filter(([, definition]) => definition.outputs !== undefined)
        .flatMap(([target, definition, node]) => Array.cartesianWith([undefined, ...Object.keys(definition.configurations ?? {})], [[target, node] as const], Tuple.make))
        .map(([configuration, [target, node]]) => getOutputsForTargetAndConfiguration({ project: node.name, target, ...(configuration === undefined ? {} : { configuration }) }, {}, node))
        .map(Array.filter(Predicate.not(String.startsWith('!'))))
        .map(Array.map((output) => path.resolve(root, output)));
    const libraries = new Set(languages);
    const retained = [
        ...launched.filter(({ Label: [, name] }) => agents[name] === true).flatMap(({ StandardOutPath, StandardErrorPath }) => Array.getSomes([StandardOutPath, StandardErrorPath])),
        ...languages,
        ...outputs.filter(Array.some(libraries.has.bind(libraries))).flat(),
        sdk,
        cacheDir,
        pulumi,
        path.join(root, '.artifacts', 'workflows'),
        ...Array.filterMap(tasks, flow(Tuple.get(1), Struct.get('options'), Schema.decodeUnknownResult(XcodeOptions))).map(({ env }) => path.resolve(root, env.XCODE_DERIVED_DATA_PATH)),
    ];
    const listed = new Set([...RESIDUES.map((entry) => path.join(root, entry)), own.build_directory, ...outputs.flat(), ...strays]);
    const preserving = under(retained);
    const candidates = [...listed].filter((candidate) => !(preserving(candidate) || lineage(candidate).some(listed.has.bind(listed))));
    const reclaimable = Match.type<(typeof opened)[number]>().pipe(
        Match.when({ path: Predicate.and(under(candidates), Predicate.not(preserving)) }, ({ descriptor, path: file }) => ({ directory: descriptor === 'cwd' ? file : path.dirname(file) })),
        Match.result,
    );
    const [stopped, spared] = Array.partition(
        Array.dedupeWith(
            alive.flatMap((holder) => Array.map(Array.filterMap(holder.opened, reclaimable), Struct.assign({ holder }))),
            (left, right) => left.directory === right.directory && left.holder.pid === right.holder.pid,
        ),
        (held) => Result.fromNullishOr(serving.get(held.holder.pid), Function.constant(held)),
    );
    yield* Effect.forEach(spared, ({ directory, holder }) => Console.log(`spare ${directory} held by ${holder.pid} ${holder.command}`), { discard: true });
    const kept = new Set([...retained, ...spared.map(Struct.get('directory'))]);
    const above = new Set([...kept].flatMap(lineage));
    const visit = (entry: string): Effect.Effect<readonly string[], PlatformError.PlatformError, FileSystem.FileSystem | Path.Path> =>
        above.has(entry) && !kept.has(entry) ? entries(entry).pipe(Effect.flatMap(Effect.forEach(flow(Struct.get('path'), visit), { concurrency: 'unbounded' })), Effect.map(Array.flatten)) : Effect.succeed([entry]);
    const clearing = Effect.forEach(candidates, visit, { concurrency: 'unbounded' }).pipe(
        Effect.map(flow(Array.flatten, Array.filter(Predicate.not(kept.has.bind(kept))))),
        Effect.flatMap(accumulate((target) => Console.log(`remove ${target}`).pipe(Effect.andThen(fs.remove(target, { recursive: true, force: true }))), { concurrency: 'unbounded' })),
    );
    const members = new Set(own.packages.map(Struct.get('name')));
    return yield* accumulate(
        [
            Array.match(Array.dedupe(stopped), {
                onEmpty: () => clearing,
                onNonEmpty: (names) => Effect.acquireUseRelease(Console.log(`stop daemons ${names.join(' ')}`), Function.constant(runCommand.shown('mise', ['daemons', 'stop', ...names]).pipe(Effect.andThen(clearing))), Function.constant(Effect.asVoid(runCommand.shown('mise', ['daemons', 'start', ...names])))),
            }),
            Array.match(Array.dedupe(Array.filterMap(products, ([name, , , , source]) => (workspaces(source) && !members.has(name) ? Result.succeed(name) : Result.failVoid))), { onEmpty: () => Effect.void, onNonEmpty: (names) => runCommand.shown('cargo', ['uninstall', ...names]) }),
            Fiber.join(pruning),
        ],
        identity,
        { concurrency: 'unbounded' },
    );
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { cleanup };

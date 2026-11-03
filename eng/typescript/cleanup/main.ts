import process from 'node:process';
import { NodeRuntime, NodeServices } from '@effect/platform-node';
import { cacheDir, getOutputsForTargetAndConfiguration, readCachedProjectGraph, workspaceRoot } from '@nx/devkit';
import { Array, Cause, Config, Console, Effect, Fiber, FileSystem, flow, identity, Match, Option, Path, type PlatformError, Predicate, Record, Result, Schedule, Schema, Struct, Tuple } from 'effect';
import { accumulate } from '#/eng/typescript/accumulate';
import { Package } from '#/eng/typescript/manifest';
import { fromString, Plist, Yaml } from '#/eng/typescript/parsed';
import { records, run, SHOWN } from '#/eng/typescript/process';
import manifest from '#/package.json' with { type: 'json' };

// --- [CONSTANTS] -----------------------------------------------------------------------

const PS_ROW = /^\s*(?<pid>\d+)\s+(?<ppid>\d+)\s+(?<uid>-?\d+)\s+(?<state>\S+)\s+(?:\?\??|(?<terminal>\S+))\s+(?<started>\S+ \S+\s+\S+ \S+ \S+)\s+(?<command>.*)$/u;
const RESIDUES = ['.build', '.cache', '.artifacts', '.playwright-cli', '.playwright-mcp'];
const AGENT = 'dev.mise.';

// --- [MODELS] --------------------------------------------------------------------------

const IntFromString = Schema.FiniteFromString.check(Schema.isInt());
const PsRow = fromString('ps row', (line) => PS_ROW.exec(line)?.groups).pipe(Schema.decodeTo(Schema.Struct({ pid: IntFromString, ppid: IntFromString, uid: IntFromString, state: Schema.String, terminal: Schema.OptionFromUndefinedOr(Schema.String), started: Schema.String, command: Schema.String })));
const LsofRecord = Schema.Union([
    Schema.Struct({ _tag: Schema.tagDefaultOmit('Process'), pid: IntFromString }).pipe(Schema.encodeKeys({ pid: 'p' })),
    Schema.Struct({ _tag: Schema.tagDefaultOmit('File'), descriptor: Schema.String, device: Schema.OptionFromOptionalKey(Schema.String), path: Schema.String }).pipe(Schema.encodeKeys({ descriptor: 'f', device: 'd', path: 'n' })),
]);
const LaunchdPlist = Schema.Struct({ Label: Schema.TemplateLiteralParser([AGENT, Schema.String]), WorkingDirectory: Schema.String, StandardOutPath: Schema.OptionFromOptionalKey(Schema.String), StandardErrorPath: Schema.OptionFromOptionalKey(Schema.String) });
const LaunchdAgents = Schema.fromJsonString(Schema.Struct({ launchd: Schema.Struct({ agents: Schema.Array(Schema.Struct({ name: Schema.String, loaded: Schema.Boolean })) }) }));
const MiseInstalls = Schema.fromJsonString(Schema.Record(Schema.String, Schema.Array(Schema.Struct({ install_path: Schema.String }))));
const MiseDaemons = Schema.fromJsonString(Schema.Array(Schema.Struct({ name: Schema.String, pid: Schema.OptionFromNullOr(Schema.Int) })));
const CargoMetadata = Schema.fromJsonString(Schema.Struct({ packages: Schema.Array(Schema.Struct({ name: Schema.String })), build_directory: Schema.String }));
const SgConfig = Yaml.pipe(Schema.decodeTo(Schema.Struct({ customLanguages: Schema.Record(Schema.String, Schema.Struct({ libraryPath: Schema.String })) })));
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
const plist = (file: string) => Effect.flatMap(FileSystem.FileSystem, (fs) => fs.readFileString(file)).pipe(Effect.flatMap(Schema.decodeEffect(Plist)));
const metadata = (cwd: string) => run('cargo', ['metadata', '--no-deps', '--format-version', '1'], { cwd }).pipe(Effect.flatMap(Schema.decodeEffect(CargoMetadata)));
const listing = records('\n', 'ps', ['-axww', '-o', 'pid=,ppid=,uid=,stat=,tty=,lstart=,command='], { env: { LC_ALL: 'C' }, extendEnv: true }).pipe(Effect.flatMap(Schema.decodeEffect(Schema.Array(PsRow))));
const signal = (pid: number, value: NodeJS.Signals | 0) =>
    Effect.try({ try: () => process.kill(pid, value), catch: (cause) => new Cause.UnknownError(cause, `Signal ${value} to PID ${pid} failed`) }).pipe(
        Effect.catchIf(
            ({ cause }) => Predicate.hasProperty(cause, 'code') && cause.code === 'ESRCH',
            () => Effect.succeed(false),
        ),
    );

// --- [COMPOSITION] ---------------------------------------------------------------------

Effect.gen(function* () {
    const [fs, path, home, ghidra, pulumi] = yield* Effect.all([FileSystem.FileSystem, Path.Path, Config.String('HOME'), Config.String('GHIDRA_PROJECT_DIR'), Config.String('PULUMI_HOME')]);
    const darwin = process.platform === 'darwin';
    const root = path.resolve(workspaceRoot);
    const under = (roots: readonly string[]) => (entry: string) => roots.some((at) => entry === at || entry.startsWith(`${at}${path.sep}`));
    const lineage = (entry: string) => Array.unfold(path.dirname(entry), (at) => (at === path.dirname(at) ? Option.none() : Option.some([at, path.dirname(at)] as const)));
    const pruning = yield* Effect.forkChild(
        accumulate(
            [
                run('mise', ['cache', 'prune'], SHOWN),
                run('mise', ['daemons', 'prune', '--yes'], SHOWN),
                run('uv', ['cache', 'prune'], SHOWN),
                run('pnpm', ['store', 'prune'], SHOWN).pipe(Effect.andThen(run('pnpm', ['cache', 'prune'], SHOWN))),
                run('cargo', ['insta', 'reject', '--workspace', '--quiet'], SHOWN),
                Effect.gen(function* () {
                    const [cached, providers] = yield* Effect.all(
                        [
                            run('pulumi', ['plugin', 'ls', '--json']).pipe(Effect.flatMap(Schema.decodeEffect(PulumiPlugins))),
                            Effect.forEach(Object.keys(manifest.devDependencies), (name) => Effect.map(fs.readFileString(path.join(root, 'node_modules', name, Package.file)), Schema.decodeUnknownOption(PulumiProvider)), { concurrency: 'unbounded' }),
                        ],
                        { concurrency: 'unbounded' },
                    );
                    const [resources, used] = [cached.filter(({ kind }) => kind === 'resource'), Array.getSomes(providers).map(({ pulumi: { name, version } }) => ({ kind: 'resource', name, version }))];
                    return yield* accumulate(Array.difference(resources, used), ({ kind, name, version }) => run('pulumi', ['plugin', 'rm', kind, name, version, '--yes'], SHOWN), { concurrency: 'unbounded' });
                }),
            ],
            identity,
            { concurrency: 'unbounded' },
        ),
    );
    const { graph, languages, worktrees, installed, developer, agents, launched, daemons, toolchains, products, own } = yield* Effect.all(
        {
            graph: Effect.try(readCachedProjectGraph),
            languages: fs.readFileString(path.join(root, 'sgconfig.yml')).pipe(
                Effect.flatMap(Schema.decodeEffect(SgConfig)),
                Effect.map(({ customLanguages }) => Object.values(customLanguages).map(({ libraryPath }) => path.resolve(root, libraryPath))),
            ),
            worktrees: records('\0\0', 'git', ['worktree', 'list', '--porcelain', '-z']).pipe(Effect.map((blocks) => Array.getSomes(blocks.flatMap((block) => (block.includes('\0prunable') ? [] : block.split('\0'))).map((field) => Schema.decodeUnknownOption(Worktree)(field))).map(Tuple.get(1)))),
            installed: run('mise', ['ls', '--installed', '--json']).pipe(
                Effect.flatMap(Schema.decodeEffect(MiseInstalls)),
                Effect.flatMap((installs) => Effect.forEach(Object.values(installs).flat(), ({ install_path }) => fs.realPath(install_path), { concurrency: 'unbounded' })),
            ),
            developer: darwin ? run('xcode-select', ['--print-path']).pipe(Effect.map((text) => [text.trim()])) : Effect.succeed([]),
            agents: darwin
                ? run('mise', ['bootstrap', 'macos', 'launchd-agents', 'status', '--json']).pipe(
                      Effect.flatMap(Schema.decodeEffect(LaunchdAgents)),
                      Effect.map(({ launchd }) => new Map(launchd.agents.map(({ name, loaded }) => [name, loaded] as const))),
                  )
                : Effect.succeed(new Map<string, boolean>()),
            launched: entries(path.join(home, 'Library', 'LaunchAgents')).pipe(
                Effect.flatMap((found) => Effect.forEach(found.filter(flow(Struct.get('name'), Schema.is(AgentFile))), ({ path: file }) => Effect.map(plist(file), Schema.decodeUnknownOption(LaunchdPlist)), { concurrency: 'unbounded' })),
                Effect.map((plists) => Array.getSomes(plists).filter(({ WorkingDirectory }) => WorkingDirectory === root)),
            ),
            daemons: run('mise', ['daemons', 'ls', '--json']).pipe(Effect.flatMap(Schema.decodeEffect(MiseDaemons))),
            toolchains: run('rustup', ['show', 'home']).pipe(Effect.flatMap((text) => entries(path.join(text.trim(), 'toolchains')))),
            products: records('\n', 'cargo', ['install', '--list']).pipe(Effect.map((lines) => Array.getSomes(lines.map((line) => Schema.decodeUnknownOption(Product)(line))))),
            own: metadata(root),
        },
        { concurrency: 'unbounded' },
    );
    const checkouts = yield* Effect.filter(worktrees, (checkout) => fs.exists(path.join(checkout, 'Cargo.toml')), { concurrency: 'unbounded' }).pipe(Effect.flatMap(Effect.forEach(metadata, { concurrency: 'unbounded' })));
    yield* accumulate(
        launched.filter(({ Label: [, name] }) => !agents.has(name)),
        ({ Label: [, name] }) => run('mise', ['bootstrap', 'services', 'remove', name, '--yes'], SHOWN),
        { concurrency: 'unbounded' },
    );
    const [rows, opened, jobs] = yield* Effect.all(
        [
            listing,
            records('\0\n', 'lsof', ['-nP', '-F0pfdn']).pipe(
                Effect.map(Array.map((set) => Object.fromEntries(set.split('\0').map((field) => [field.slice(0, 1), field.slice(1)])))),
                Effect.flatMap(Schema.decodeUnknownEffect(Schema.Array(LsofRecord))),
                Effect.map((sets) => Array.getSomes(Array.mapAccum(sets, 0, (pid, set) => Match.valueTags(set, { Process: (owner) => [owner.pid, Option.none()] as const, File: (file) => [pid, Option.some({ ...file, pid })] as const }))[1])),
            ),
            darwin ? records('\n', 'launchctl', ['list']).pipe(Effect.map((lines) => Array.getSomes(lines.map((line) => Schema.decodeUnknownOption(Job)(line))).map(Tuple.get(0)))) : Effect.succeed([]),
        ],
        { concurrency: 'unbounded' },
    );
    const files = Map.groupBy(opened, Struct.get('pid'));
    const running = rows.map((row) => ({ ...row, opened: files.get(row.pid) ?? [] }));
    const processes = new Map(running.map((row) => [row.pid, row] as const));
    const children = Map.groupBy(running, Struct.get('ppid'));
    const descendants = (pid: number): (typeof running)[number][] => (children.get(pid) ?? []).flatMap((child) => [child, ...descendants(child.pid)]);
    const ancestors = (pid: number) => Array.unfold(pid, (current) => Option.flatMap(Option.fromUndefinedOr(processes.get(current)), ({ ppid }) => (ppid === current ? Option.none() : Option.some([current, ppid] as const))));
    const holds = (candidate: (typeof running)[number], descriptor: 'txt' | 'cwd', roots: readonly string[]) => candidate.opened.some((handle) => handle.descriptor === descriptor && under(roots)(handle.path));
    const local = [...Array.cartesianWith(worktrees, ['node_modules', '.venv', ...RESIDUES], path.join), ...checkouts.map(Struct.get('build_directory'))];
    const tools = [...installed, ...developer, ...toolchains.map(Struct.get('path'))];
    const owned = (candidate: (typeof running)[number]) => holds(candidate, 'txt', local) || (holds(candidate, 'txt', tools) && holds(candidate, 'cwd', worktrees));
    const { uid } = yield* Effect.fromOption(Option.fromUndefinedOr(processes.get(process.pid)), () => new Cause.NoSuchElementError(`ps lists no PID ${process.pid}`));
    const supervised = [...jobs, ...Array.getSomes(daemons.map(Struct.get('pid')))];
    const exempt = new Set([...ancestors(process.pid), ...supervised.flatMap((pid) => [pid, ...descendants(pid).map(Struct.get('pid'))]), ...running.filter(({ terminal }) => Option.isSome(terminal)).flatMap(({ pid }) => ancestors(pid))]);
    const ends = Array.groupBy(
        opened.flatMap(({ device, path: name, pid }) => [...Option.toArray(device), name].map((key) => ({ key, pid }))),
        Struct.get('key'),
    );
    const links = Array.getSomes(opened.map(({ path: name, pid }) => Option.map(Schema.decodeUnknownOption(Connection)(name), ([source, , remote]) => ({ pid, key: source === '' ? remote : `${remote}->${source}` }))));
    const peers = Map.groupBy(
        links.flatMap(({ pid, key }) => (ends[key] ?? []).flatMap((end) => [[pid, end.pid] as const, [end.pid, pid] as const])),
        Tuple.get(0),
    );
    const targets = running
        .filter((leader) => leader.ppid === 1)
        .map((leader) => [leader, ...descendants(leader.pid)])
        .filter((group) => group.some(owned) && new Set(group.flatMap(({ pid }) => (peers.get(pid) ?? []).map(Tuple.get(1)))).isSubsetOf(new Set(group.map(Struct.get('pid')))) && !group.some((member) => exempt.has(member.pid) || member.uid !== uid))
        .flatMap((group) => group.filter(({ state }) => !state.startsWith('Z')));
    yield* Effect.forEach(
        running,
        (parent) =>
            Array.match(
                (children.get(parent.pid) ?? []).filter(({ state }) => state.startsWith('Z')),
                { onEmpty: () => Effect.void, onNonEmpty: (zombies) => Console.log(`zombie ${zombies.map(Struct.get('pid')).join(',')} parent ${parent.pid} ${parent.command}`) },
            ),
        { discard: true },
    );
    yield* accumulate(targets, (target) => Console.log(`terminate ${target.pid} ${target.command}`).pipe(Effect.andThen(signal(target.pid, 'SIGTERM'))), { concurrency: 'unbounded' });
    const settled = yield* Effect.filter(targets.map(Struct.get('pid')), (pid) => signal(pid, 0)).pipe(Effect.repeat({ schedule: Schedule.spaced('50 millis'), until: Array.isArrayEmpty }), Effect.timeoutOption('5 seconds'));
    const relisted = Option.isSome(settled) ? [] : yield* listing;
    yield* accumulate(
        targets.filter((target) => relisted.some(({ pid, started }) => pid === target.pid && started === target.started)),
        (target) => Console.log(`kill ${target.pid} ${target.command}`).pipe(Effect.andThen(signal(target.pid, 'SIGKILL'))),
        { concurrency: 'unbounded' },
    );
    const gone = new Set(targets.map(Struct.get('pid')));
    const alive = running.filter(({ pid }) => !gone.has(pid));
    const serving = new Map(
        daemons.flatMap(({ name, pid }) =>
            Option.toArray(pid)
                .flatMap((leader) => [leader, ...descendants(leader).map(Struct.get('pid'))])
                .map((member) => [member, name] as const),
        ),
    );
    const workspace = (entry: string) =>
        plist(path.join(entry, 'info.plist')).pipe(
            Effect.map(flow(Schema.decodeUnknownOption(DerivedInfo), Option.exists(flow(Struct.get('WorkspacePath'), under(worktrees))))),
            Effect.catchReason('PlatformError', 'NotFound', () => Effect.succeed(false)),
        );
    const strays = yield* Effect.all(
        [
            entries(path.join(home, '.dylint_drivers')).pipe(Effect.flatMap(Effect.filter(({ name, path: entry }) => (toolchains.some((toolchain) => toolchain.name === name) ? Effect.succeed(false) : Effect.map(fs.stat(entry), ({ type }) => type === 'Directory')), { concurrency: 'unbounded' }))),
            entries(path.join(home, 'Library', 'Developer', 'Xcode', 'DerivedData')).pipe(Effect.flatMap(Effect.filter(({ path: entry }) => workspace(entry), { concurrency: 'unbounded' }))),
            entries(path.join(home, '.lldb')).pipe(Effect.map(Array.filter(({ name }) => Option.exists(Schema.decodeUnknownOption(Registration)(name), ([, pid]) => !processes.has(pid) || gone.has(pid))))),
        ],
        { concurrency: 'unbounded' },
    ).pipe(Effect.map(flow(Array.flatten, Array.map(Struct.get('path')))));
    const tasks = Object.values(graph.nodes).flatMap((node) => Record.collect(node.data.targets ?? {}, (target, definition) => ({ node, target, definition })));
    const retained = [
        ...launched.filter(({ Label: [, name] }) => agents.get(name) === true).flatMap(({ StandardOutPath, StandardErrorPath }) => Array.getSomes([StandardOutPath, StandardErrorPath])),
        ...languages,
        cacheDir,
        pulumi,
        path.join(root, '.artifacts', 'workflows'),
        ...Array.getSomes(tasks.map(({ definition }) => Schema.decodeUnknownOption(XcodeOptions)(definition.options))).map(({ env }) => path.resolve(root, env.XCODE_DERIVED_DATA_PATH)),
    ];
    const listed = new Set([
        ...RESIDUES.map((entry) => path.join(root, entry)),
        own.build_directory,
        ghidra,
        ...tasks
            .filter(({ definition }) => definition.outputs !== undefined)
            .flatMap(({ node, target, definition }) => [{ project: node.name, target }, ...Object.keys(definition.configurations ?? {}).map((configuration) => ({ project: node.name, target, configuration }))].flatMap((task) => getOutputsForTargetAndConfiguration(task, {}, node)))
            .filter((output) => !output.startsWith('!'))
            .map((output) => path.resolve(root, output)),
        ...strays,
    ]);
    const candidates = [...listed].filter((candidate) => !(under(retained)(candidate) || lineage(candidate).some((at) => listed.has(at))));
    const [stopped, spared] = Array.partition(
        Array.dedupeWith(
            alive.flatMap((holder) => holder.opened.filter(({ path: file }) => under(candidates)(file) && !under(retained)(file)).map(({ descriptor, path: file }) => ({ directory: descriptor === 'cwd' ? file : path.dirname(file), holder }))),
            (left, right) => left.directory === right.directory && left.holder.pid === right.holder.pid,
        ),
        (held) => Result.fromNullishOr(serving.get(held.holder.pid), () => held),
    );
    yield* Effect.forEach(spared, ({ directory, holder }) => Console.log(`spare ${directory} held by ${holder.pid} ${holder.command}`), { discard: true });
    const kept = new Set([...retained, ...spared.map(Struct.get('directory'))]);
    const above = new Set([...kept].flatMap(lineage));
    const visit = (entry: string): Effect.Effect<readonly string[], PlatformError.PlatformError, FileSystem.FileSystem | Path.Path> =>
        above.has(entry) && !kept.has(entry) ? entries(entry).pipe(Effect.flatMap(Effect.forEach(flow(Struct.get('path'), visit), { concurrency: 'unbounded' })), Effect.map(Array.flatten)) : Effect.succeed([entry]);
    const clearing = Effect.forEach(candidates, visit, { concurrency: 'unbounded' }).pipe(
        Effect.flatMap((found) =>
            accumulate(
                found.flat().filter((target) => !kept.has(target)),
                (target) => Console.log(`remove ${target}`).pipe(Effect.andThen(fs.remove(target, { recursive: true, force: true }))),
                { concurrency: 'unbounded' },
            ),
        ),
    );
    const members = new Set(own.packages.map(Struct.get('name')));
    return yield* accumulate(
        [
            Array.match(Array.dedupe(stopped), {
                onEmpty: () => clearing,
                onNonEmpty: (names) =>
                    Effect.acquireUseRelease(
                        Console.log(`stop daemons ${names.join(' ')}`),
                        () => run('mise', ['daemons', 'stop', ...names], SHOWN).pipe(Effect.andThen(clearing)),
                        () => Effect.asVoid(run('mise', ['daemons', 'start', ...names], SHOWN)),
                    ),
            }),
            Array.match(Array.dedupe(Array.filterMap(products, ([name, , , , source]) => (under(worktrees)(source) && !members.has(name) ? Result.succeed(name) : Result.failVoid))), { onEmpty: () => Effect.void, onNonEmpty: (names) => run('cargo', ['uninstall', ...names], SHOWN) }),
            Fiber.join(pruning),
        ],
        identity,
        { concurrency: 'unbounded' },
    );
}).pipe(Effect.provide(NodeServices.layer), NodeRuntime.runMain);

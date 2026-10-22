import process from 'node:process';
import { runMain } from '@effect/platform-node/NodeRuntime';
import { layer } from '@effect/platform-node/NodeServices';
import { cacheDir, getOutputsForTargetAndConfiguration, readCachedProjectGraph, workspaceRoot } from '@nx/devkit';
import { Array, Cause, Config, Console, Effect, Exit, Fiber, FileSystem, flow, identity, Option, Path, type PlatformError, Predicate, Result, Schedule, Schema, String, Struct, Tuple } from 'effect';
import { parse as parsePlist } from 'plist';
import { parse as parseYaml } from 'yaml';
import { stdout } from '#/eng/typescript/process';

// --- [CONSTANTS] -----------------------------------------------------------------------

const PS_ROW = /^\s*(?<pid>\d+)\s+(?<ppid>\d+)\s+(?<pgid>\d+)\s+(?<uid>-?\d+)\s+(?<state>\S+)\s+(?:\?\??|(?<terminal>\S+))\s+(?<started>\S+ \S+\s+\S+ \S+ \S+)\s+(?<command>.*)$/u;

// --- [OPERATIONS] ----------------------------------------------------------------------

const accumulate = <A, E, R>(items: Iterable<A>, f: (item: A) => Effect.Effect<unknown, E, R>): Effect.Effect<void, E, R> => Effect.forEach(items, flow(f, Effect.exit), { concurrency: 'unbounded' }).pipe(Effect.flatMap(Exit.asVoidAll));

// --- [COMPOSITION] ---------------------------------------------------------------------

const cleanup = Effect.fn('cleanup')(function* (self: number, darwin: boolean) {
    const [fs, path, home, ghidra, pulumi] = yield* Effect.all([FileSystem.FileSystem, Path.Path, Config.String('HOME'), Config.String('GHIDRA_PROJECT_DIR'), Config.String('PULUMI_HOME')]);
    const root = path.resolve(workspaceRoot);
    const shown = { stdout: 'inherit' } as const;
    const residues = ['.build', '.cache', '.artifacts', '.playwright-cli', '.playwright-mcp'];
    const under = (roots: readonly string[]) => (entry: string) => roots.some((at) => entry === at || entry.startsWith(`${at}${path.sep}`));
    const lineage = (entry: string) => Array.unfold(path.dirname(entry), (at) => (at === path.dirname(at) ? Option.none() : Option.some([at, path.dirname(at)] as const)));
    const lines = (text: string) => text.split('\n').filter(String.isNonEmpty);
    const entries = (directory: string) =>
        fs.readDirectory(directory).pipe(
            Effect.catchReason('PlatformError', 'NotFound', () => Effect.succeed([])),
            Effect.map(Array.map((name) => ({ name, path: path.join(directory, name) }))),
        );
    const plist = (file: string) => fs.readFileString(file).pipe(Effect.flatMap((text) => Effect.try({ try: () => parsePlist(text), catch: (cause) => new Cause.UnknownError(cause, `${file} did not parse`) })));
    const listing = stdout('ps', ['-axww', '-o', 'pid=,ppid=,pgid=,uid=,stat=,tty=,lstart=,command='], { env: { LC_ALL: 'C' }, extendEnv: true }).pipe(
        Effect.flatMap((text) =>
            Schema.decodeUnknownEffect(Schema.Array(Schema.Struct({ pid: Schema.FiniteFromString, ppid: Schema.FiniteFromString, pgid: Schema.FiniteFromString, uid: Schema.FiniteFromString, state: Schema.String, terminal: Schema.OptionFromUndefinedOr(Schema.String), started: Schema.String, command: Schema.String })))(
                lines(text).map((line) => PS_ROW.exec(line)?.groups),
            ),
        ),
    );
    const provider = Schema.decodeUnknownOption(Schema.fromJsonString(Schema.Struct({ pulumi: Schema.Struct({ resource: Schema.Literal(true), name: Schema.String, version: Schema.String }) })));
    const pruning = yield* Effect.forkChild(
        accumulate(
            [
                stdout('mise', ['cache', 'prune'], shown),
                stdout('mise', ['daemons', 'prune', '--yes'], shown),
                stdout('uv', ['cache', 'prune'], shown),
                stdout('pnpm', ['store', 'prune'], shown).pipe(Effect.andThen(stdout('pnpm', ['cache', 'prune'], shown))),
                stdout('cargo', ['insta', 'reject', '--workspace', '--quiet'], shown),
                Effect.all(
                    [
                        stdout('pulumi', ['plugin', 'ls', '--json']).pipe(Effect.flatMap(Schema.decodeUnknownEffect(Schema.fromJsonString(Schema.Array(Schema.Struct({ name: Schema.String, kind: Schema.String, version: Schema.String })))))),
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
                ).pipe(Effect.flatMap(([cached, providers]) => accumulate(Array.difference(cached, providers), ({ kind, name, version }) => stdout('pulumi', ['plugin', 'rm', kind, name, version, '--yes'], shown)))),
            ],
            identity,
        ),
    );
    const worktree = Schema.decodeUnknownOption(Schema.TemplateLiteralParser(['worktree ', Schema.String]));
    const product = Schema.decodeUnknownOption(Schema.TemplateLiteralParser([Schema.String, ' v', Schema.String, ' (', Schema.String, '):']));
    const agent = Schema.is(Schema.TemplateLiteral(['dev.mise.', Schema.String, '.plist']));
    const launch = Schema.decodeUnknownOption(Schema.Struct({ Label: Schema.TemplateLiteralParser(['dev.mise.', Schema.String]), WorkingDirectory: Schema.Literal(root), StandardOutPath: Schema.OptionFromOptionalKey(Schema.String), StandardErrorPath: Schema.OptionFromOptionalKey(Schema.String) }));
    const { graph, languages, worktrees, installed, developer, agents, launched, daemons, toolchains, products, members } = yield* Effect.all(
        {
            graph: Effect.try(readCachedProjectGraph),
            languages: fs.readFileString(path.join(root, 'sgconfig.yml')).pipe(
                Effect.flatMap((text) => Effect.try(() => parseYaml(text))),
                Effect.flatMap(Schema.decodeUnknownEffect(Schema.Struct({ customLanguages: Schema.Record(Schema.String, Schema.Struct({ libraryPath: Schema.String })) }))),
                Effect.map(({ customLanguages }) => Object.values(customLanguages).map(({ libraryPath }) => path.resolve(root, libraryPath))),
            ),
            worktrees: stdout('git', ['worktree', 'list', '--porcelain', '-z']).pipe(Effect.map((text) => Array.getSomes(text.split('\0').map((field) => worktree(field))).map(Tuple.get(1)))),
            installed: stdout('mise', ['ls', '--installed', '--json']).pipe(
                Effect.flatMap(Schema.decodeUnknownEffect(Schema.fromJsonString(Schema.Record(Schema.String, Schema.Array(Schema.Struct({ install_path: Schema.String })))))),
                Effect.flatMap((installs) => Effect.forEach(Object.values(installs).flat(), ({ install_path }) => fs.realPath(install_path), { concurrency: 'unbounded' })),
            ),
            developer: darwin ? stdout('xcode-select', ['--print-path']).pipe(Effect.map((text) => [text.trim()])) : Effect.succeed([]),
            agents: darwin
                ? stdout('mise', ['bootstrap', 'macos', 'launchd-agents', 'status', '--json']).pipe(
                      Effect.flatMap(Schema.decodeUnknownEffect(Schema.fromJsonString(Schema.Struct({ launchd: Schema.Struct({ agents: Schema.Array(Schema.Struct({ name: Schema.String, loaded: Schema.Boolean })) }) })))),
                      Effect.map(({ launchd }) => new Map(launchd.agents.map(({ name, loaded }) => [name, loaded] as const))),
                  )
                : Effect.succeed(new Map<string, boolean>()),
            launched: entries(path.join(home, 'Library', 'LaunchAgents')).pipe(
                Effect.flatMap((found) => Effect.forEach(found.filter(flow(Struct.get('name'), agent)), ({ path: file }) => Effect.map(plist(file), launch), { concurrency: 'unbounded' })),
                Effect.map(Array.getSomes),
            ),
            daemons: stdout('mise', ['daemons', 'ls', '--json']).pipe(Effect.flatMap(Schema.decodeUnknownEffect(Schema.fromJsonString(Schema.Array(Schema.Struct({ name: Schema.String, pid: Schema.OptionFromNullOr(Schema.Finite) })))))),
            toolchains: stdout('rustup', ['show', 'home']).pipe(Effect.flatMap((text) => entries(path.join(text.trim(), 'toolchains')))),
            products: stdout('cargo', ['install', '--list']).pipe(Effect.map((text) => Array.getSomes(lines(text).map((line) => product(line))))),
            members: stdout('cargo', ['metadata', '--no-deps', '--format-version', '1']).pipe(
                Effect.flatMap(Schema.decodeUnknownEffect(Schema.fromJsonString(Schema.Struct({ packages: Schema.Array(Schema.Struct({ name: Schema.String })) })))),
                Effect.map(({ packages }) => new Set(packages.map(Struct.get('name')))),
            ),
        },
        { concurrency: 'unbounded' },
    );
    yield* accumulate(
        launched.filter(({ Label: [, name] }) => !agents.has(name)),
        ({ Label: [, name] }) => stdout('mise', ['bootstrap', 'services', 'remove', name, '--yes'], shown),
    );
    const job = Schema.decodeUnknownOption(Schema.TemplateLiteralParser([Schema.Finite, '\t', Schema.String]));
    const connection = Schema.decodeUnknownOption(Schema.TemplateLiteralParser([Schema.String, '->', Schema.String]));
    const [rows, opened, jobs] = yield* Effect.all(
        [
            listing,
            stdout('lsof', ['-nP', '-F0pfdn']).pipe(
                Effect.map(
                    flow(
                        String.split('\0\n'),
                        Array.filter(String.isNonEmpty),
                        Array.map((record) => Object.fromEntries(record.split('\0').map((field) => [field.slice(0, 1), field.slice(1)]))),
                    ),
                ),
                Effect.flatMap(Schema.decodeUnknownEffect(Schema.Array(Schema.Union([Schema.Struct({ p: Schema.FiniteFromString }), Schema.Struct({ f: Schema.String, d: Schema.OptionFromOptionalKey(Schema.String), n: Schema.String })])))),
                Effect.map((records) => Array.getSomes(Array.mapAccum(records, 0, (pid, record) => ('p' in record ? ([record.p, Option.none()] as const) : ([pid, Option.some({ pid, descriptor: record.f, device: record.d, path: record.n })] as const)))[1])),
            ),
            darwin ? stdout('launchctl', ['list']).pipe(Effect.map((text) => Array.getSomes(lines(text).map((line) => job(line))).map(Tuple.get(0)))) : Effect.succeed([]),
        ],
        { concurrency: 'unbounded' },
    );
    const files = Map.groupBy(opened, Struct.get('pid'));
    const running = rows.map((row) => ({ ...row, opened: files.get(row.pid) ?? [] }));
    const processes = new Map(running.map((row) => [row.pid, row] as const));
    const children = Map.groupBy(running, Struct.get('ppid'));
    const descendants = (pid: number): (typeof running)[number][] => (children.get(pid) ?? []).flatMap((child) => [child, ...descendants(child.pid)]);
    const ancestors = (pid: number) => Array.unfold(pid, (current) => Option.flatMap(Option.fromUndefinedOr(processes.get(current)), ({ ppid }) => (ppid === current ? Option.none() : Option.some([current, ppid] as const))));
    const holds = (candidate: (typeof running)[number], descriptor: string, roots: readonly string[]) => candidate.opened.some((handle) => handle.descriptor === descriptor && under(roots)(handle.path));
    const [local, tools] = [Array.cartesianWith(worktrees, ['node_modules', '.venv', ...residues], path.join), [...installed, ...developer, ...toolchains.map(Struct.get('path'))]];
    const owned = (candidate: (typeof running)[number]) => holds(candidate, 'txt', local) || (holds(candidate, 'txt', tools) && holds(candidate, 'cwd', worktrees));
    const { uid } = yield* Effect.fromNullishOr(processes.get(self));
    const exempt = new Set([...ancestors(self), ...jobs.flatMap((pid) => [pid, ...descendants(pid).map(Struct.get('pid'))]), ...running.filter(({ terminal }) => Option.isSome(terminal)).flatMap(({ pid }) => ancestors(pid))]);
    const ends = Array.groupBy(
        opened.flatMap(({ device, path: name, pid }) => [...Option.toArray(device), name].map((key) => ({ key, pid }))),
        Struct.get('key'),
    );
    const peers = Map.groupBy(
        opened.flatMap(({ path: name, pid }) => Option.match(connection(name), { onNone: () => [], onSome: ([source, , remote]) => (ends[source === '' ? remote : `${remote}->${source}`] ?? []).flatMap((end) => [[pid, end.pid] as const, [end.pid, pid] as const]) })),
        Tuple.get(0),
    );
    const targets = running
        .filter((leader) => leader.ppid === 1 && !(leader.pgid === leader.pid && owned(leader) && !holds(leader, 'txt', [path.join(pulumi, 'plugins')])))
        .flatMap((leader) => {
            const subtree = [leader, ...descendants(leader.pid)];
            const reached = new Set(subtree.flatMap(({ pid }) => (peers.get(pid) ?? []).map(Tuple.get(1))));
            return subtree.some(owned) && reached.isSubsetOf(new Set(subtree.map(Struct.get('pid')))) && !subtree.some((member) => exempt.has(member.pid) || member.uid !== uid) ? subtree.filter(({ state }) => !state.startsWith('Z')) : [];
        });
    const signal = (pid: number, value: NodeJS.Signals | 0) =>
        Effect.try({ try: () => process.kill(pid, value), catch: (cause) => new Cause.UnknownError(cause, `cannot signal process ${pid}`) }).pipe(
            Effect.catchIf(
                ({ cause }) => Predicate.hasProperty(cause, 'code') && cause.code === 'ESRCH',
                () => Effect.succeed(false),
            ),
        );
    yield* Effect.forEach(
        running,
        (parent) =>
            Array.match(
                (children.get(parent.pid) ?? []).filter(({ state }) => state.startsWith('Z')),
                { onEmpty: () => Effect.void, onNonEmpty: (zombies) => Console.log(`zombie ${zombies.map(Struct.get('pid')).join(',')} parent ${parent.pid} ${parent.command}`) },
            ),
        { discard: true },
    );
    yield* accumulate(targets, (target) => Console.log(`terminate ${target.pid} ${target.command}`).pipe(Effect.andThen(signal(target.pid, 'SIGTERM'))));
    const settled = yield* Effect.filter(targets.map(Struct.get('pid')), (pid) => signal(pid, 0)).pipe(Effect.repeat({ schedule: Schedule.spaced('50 millis'), until: Array.isArrayEmpty }), Effect.timeoutOption('5 seconds'));
    const relisted = Option.isSome(settled) ? [] : yield* listing;
    yield* accumulate(
        targets.filter((target) => relisted.some(({ pid, started }) => pid === target.pid && started === target.started)),
        (target) => Console.log(`kill ${target.pid} ${target.command}`).pipe(Effect.andThen(signal(target.pid, 'SIGKILL'))),
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
    const derivation = Schema.decodeUnknownOption(Schema.Struct({ WorkspacePath: Schema.String }));
    const registration = Schema.decodeUnknownOption(Schema.TemplateLiteralParser(['lldb-mcp-', Schema.Finite, '.json']));
    const strays = yield* Effect.all(
        [
            entries(path.join(home, '.dylint_drivers')).pipe(Effect.flatMap(Effect.filter(({ name, path: entry }) => (toolchains.some((toolchain) => toolchain.name === name) ? Effect.succeed(false) : Effect.map(fs.stat(entry), ({ type }) => type === 'Directory')), { concurrency: 'unbounded' }))),
            entries(path.join(home, 'Library', 'Developer', 'Xcode', 'DerivedData')).pipe(
                Effect.flatMap(
                    Effect.filter(
                        ({ path: entry }) =>
                            plist(path.join(entry, 'info.plist')).pipe(
                                Effect.map(flow(derivation, Option.exists(flow(Struct.get('WorkspacePath'), under(worktrees))))),
                                Effect.catchReason('PlatformError', 'NotFound', () => Effect.succeed(false)),
                            ),
                        {
                            concurrency: 'unbounded',
                        },
                    ),
                ),
            ),
            entries(path.join(home, '.lldb')).pipe(Effect.map(Array.filter(({ name }) => Option.exists(registration(name), ([, pid]) => !processes.has(pid) || gone.has(pid))))),
        ],
        { concurrency: 'unbounded' },
    ).pipe(Effect.map(flow(Array.flatten, Array.map(Struct.get('path')))));
    const tasks = Object.values(graph.nodes).flatMap((node) => Object.entries(node.data.targets ?? {}).map(([target, definition]) => ({ node, target, definition })));
    const xcode = Schema.decodeUnknownOption(Schema.Struct({ env: Schema.Struct({ XCODE_DERIVED_DATA_PATH: Schema.String }) }));
    const retained = [
        ...launched.filter(({ Label: [, name] }) => agents.get(name) === true).flatMap(({ StandardOutPath, StandardErrorPath }) => Array.getSomes([StandardOutPath, StandardErrorPath])),
        ...languages,
        cacheDir,
        ...tasks.flatMap(({ definition }) => Option.toArray(xcode(definition.options))).map(({ env }) => path.resolve(root, env.XCODE_DERIVED_DATA_PATH)),
        pulumi,
    ];
    const listed = new Set([
        ...residues.map((entry) => path.join(root, entry)),
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
    const visit = (entry: string): Effect.Effect<readonly string[], PlatformError.PlatformError> => {
        if (kept.has(entry)) {
            return Effect.succeed([]);
        }
        return above.has(entry)
            ? entries(entry).pipe(
                  Effect.flatMap((found) => Effect.forEach(found, flow(Struct.get('path'), visit), { concurrency: 'unbounded' })),
                  Effect.map(Array.flatten),
              )
            : Effect.succeed([entry]);
    };
    const clearing = accumulate((yield* Effect.forEach(candidates, visit, { concurrency: 'unbounded' })).flat(), (target) => Console.log(`remove ${target}`).pipe(Effect.andThen(fs.remove(target, { recursive: true, force: true }))));
    return yield* accumulate(
        [
            Array.match(Array.dedupe(stopped), {
                onEmpty: () => clearing,
                onNonEmpty: (names) =>
                    Effect.acquireUseRelease(
                        Console.log(`stop daemons ${names.join(' ')}`).pipe(Effect.andThen(stdout('mise', ['daemons', 'stop', ...names], shown)), Effect.result),
                        (stop) => Effect.fromResult(stop).pipe(Effect.andThen(clearing)),
                        () => Effect.asVoid(stdout('mise', ['daemons', 'start', ...names], shown)),
                    ),
            }),
            Array.match(Array.dedupe(products.flatMap(([name, , , , source]) => (under(worktrees)(source) && !members.has(name) ? [name] : []))), { onEmpty: () => Effect.void, onNonEmpty: (names) => stdout('cargo', ['uninstall', ...names], shown) }),
            Fiber.join(pruning),
        ],
        identity,
    );
});

cleanup(process.pid, process.platform === 'darwin').pipe(Effect.provide(layer), runMain);

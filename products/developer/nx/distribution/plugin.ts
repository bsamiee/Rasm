import { join, posix } from 'node:path';
import process, { platform } from 'node:process';
import { fileURLToPath } from 'node:url';
import {
    AggregateCreateNodesError,
    type CreateNodes,
    type CreateNodesContext,
    detectPackageManager,
    getPackageManagerCommand,
    type PostTasksExecution,
    type PostTasksExecutionContext,
    type PreTasksExecution,
    type ProjectConfiguration,
    readJsonFile,
    type TargetConfiguration,
    type Task,
    type TaskResult,
} from '@nx/devkit';
import { combineGlobPatterns, emitPluginWorkerLog, type PackageJson, quoteShellArg, workspaceDataDirectory } from '@nx/devkit/internal';
import { Array, Cause, Config, DateTime, Duration, Effect, FileSystem, Formatter, Function, Option, Order, Predicate, Record, Result, Schema, String, Struct, UndefinedOr } from 'effect';
import { FetchHttpClient, HttpClient, HttpClientRequest, HttpClientResponse } from 'effect/http';
import which from 'which';
import { type Fields, FOLDERS, HOSTS, type Host, routed } from '#/host-packages';
import { above, aggregated, anchored, Folder, inferring, manifested, namedInput, run, type Stated, tagged, type Verbs } from '#/inference';

// --- [TYPES] ---------------------------------------------------------------------------

interface Declared {
    readonly file: string;
    readonly root: string;
    readonly named: 'production' | 'default';
    readonly fields: Fields;
    readonly lock?: string;
    readonly engines: readonly string[];
    readonly stated: Stated;
    readonly host: Host;
    readonly dependents: readonly string[];
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const PLUGIN = readJsonFile<PackageJson>(fileURLToPath(import.meta.resolve('#/package.json'))).name;
const SERVER = 'NX_SELF_HOSTED_REMOTE_CACHE_SERVER';
const RECORD = { file: 'task-cache.jsonl', bound: 1_048_576, slowest: 5 } as const;
const KINDS: Readonly<Record<TaskResult['status'], 'local' | 'remote' | 'missed' | undefined>> = { 'local-cache': 'local', 'local-cache-kept-existing': 'local', 'remote-cache': 'remote', success: 'missed', failure: 'missed', skipped: undefined, stopped: undefined };
const SHARE = new Intl.NumberFormat('en', { style: 'percent', maximumFractionDigits: 1 });
const WORKFLOWS = '**/.github/workflows/*.{yml,yaml}';
const COMPOSITES = '**/.github/actions/*/action.{yml,yaml}';
const PACKAGE = 'package.json';
const MISE = '.config/mise.toml';
const FILES = combineGlobPatterns([...Array.dedupe(HOSTS.map(({ manifest }) => `**/${posix.basename(manifest)}`)), `**/${PACKAGE}`, WORKFLOWS, COMPOSITES, MISE]);
const WITHHELD = ['archive', 'build', 'operations', 'publish'] as const;
const UPSTREAM = { dependentTasksOutputFiles: '**/*' } as const;
const RECEIVED: NonNullable<TargetConfiguration['inputs']> = [UPSTREAM, { externalDependencies: [] }];
const UNSTATED: NonNullable<TargetConfiguration['inputs']> = ['default', '^default', { externalDependencies: [] }];
const RELEASED: NonNullable<TargetConfiguration['inputs']> = [{ dependentTasksOutputFiles: '**/[!.]*' }, { externalDependencies: [] }];
const ROOT: Verbs = {
    clean: {
        mise: {
            executor: `${PLUGIN}:clean`,
            dependsOn: [{ projects: '*', target: 'clean--!(mise)', params: 'forward' }],
            options: { folders: [['ARTIFACTS_DIR']] },
            configurations: { cache: { commands: [['mise', 'prune', '--yes']], folders: [['CACHE_DIR']] } },
            inputs: UNSTATED,
            metadata: {
                description: `Empties the folder ARTIFACTS_DIR of ${MISE} names once every other clean target ran, and under configuration cache runs mise prune and empties the folder CACHE_DIR names but the entry holding Nx workspace data`,
                help: { command: 'mise prune --help', example: { options: { folders: [['ARTIFACTS_DIR']] } } },
            },
        },
    },
    upgrade: {
        mise: {
            executor: `${PLUGIN}:upgrade`,
            dependsOn: [{ projects: '*', target: 'upgrade--!(mise)', params: 'forward' }],
            options: { cwd: '.', commands: [{ apply: ['mise', 'upgrade'], preview: ['mise', 'upgrade', '--dry-run'] }] },
            inputs: [{ externalDependencies: [] }],
            metadata: { description: `Moves every tool row of ${MISE} to its newest release and rewrites its lock with mise upgrade, once every other upgrade target ran`, help: { command: 'mise upgrade --help', example: { options: { dryRun: true } } } },
        },
    },
};

// --- [MODELS] --------------------------------------------------------------------------

class Options extends Schema.Opaque<Options, unknown>()(
    Schema.Struct({
        outputPath: Folder,
        storeCredentials: Schema.optionalKey(Schema.Record(Schema.String, Schema.String)),
        remoteCacheTimeout: Schema.optionalKey(Schema.DurationFromString),
    }).annotate({
        description:
            'outputPath: workspace folder each host package is written to, {projectName} naming the project, absent writes where the host tool writes in the project. storeCredentials: environment variable holding the store credential per technology, absent naming the variable of the host row (Blender: BLENDER_EXTENSIONS_TOKEN, Yak: YAK_TOKEN, Chrome: CHROME_WEB_STORE_TOKEN, an OAuth access token of scope chromewebstore beside CHROME_WEB_STORE_PUBLISHER naming the publisher), a publish whose variable is unset naming it and succeeding. remoteCacheTimeout: longest wait, as a number and a unit (5 seconds), for the answer to the one read each nx run no task started sends the remote cache server before its tasks, a read answered other than 200 or 404, refused, or unanswered that long printing one warning and emptying NX_SELF_HOSTED_REMOTE_CACHE_SERVER for that run, absent waits without bound',
    }),
) {}

const Requirements = Schema.Record(Schema.String, Schema.String).pipe(Schema.withDecodingDefaultKey(Effect.succeed({})));
const Member = Schema.fromJsonString(Schema.Struct({ name: Schema.String, dependencies: Requirements, devDependencies: Requirements, optionalDependencies: Requirements, peerDependencies: Requirements })).annotate({
    description: 'The name of a workspace package.json and the packages each of its dependency fields requires',
});
const Line = Schema.fromJsonString(Schema.Unknown);

// --- [OPERATIONS] ----------------------------------------------------------------------

const read = Effect.fnUntraced(function* (files: readonly string[], context: CreateNodesContext) {
    const fs = yield* FileSystem.FileSystem;
    const { workspaceRoot } = context;
    const member = anchored(workspaceRoot);
    const [packages, manifests] = Array.partition(files, (file) => Result.liftPredicate(file, (at) => posix.basename(at) === PACKAGE, Function.identity));
    const [members, undecoded] = yield* Effect.partition(
        packages.filter((file) => member(posix.dirname(file))),
        (file) =>
            Effect.mapBoth(Effect.flatMap(fs.readFileString(join(workspaceRoot, file)), Schema.decodeEffect(Member)), {
                onFailure: (error): [string, Error] => [file, error],
                onSuccess: ({ name, ...required }) => ({ root: posix.dirname(file), name, required: Object.values(required).flatMap((held) => Object.keys(held)) }),
            }),
        { concurrency: 'unbounded' },
    );
    const requiring = Array.groupBy(
        members.flatMap(({ name, required }) => required.map((dependency) => ({ dependency, name }))),
        Struct.get('dependency'),
    );
    const named = Record.fromIterableWith(members, ({ root, name }) => [root, name]);
    const [held, unread] = yield* Effect.partition(
        manifests,
        (file) =>
            Effect.mapError(
                Effect.flatMap(fs.readFileString(join(workspaceRoot, file)), (text) => routed(file, text)),
                (error): [string, Error] => [file, error],
            ),
        { concurrency: 'unbounded' },
    );
    const matched = Array.getSomes(held);
    const local = Array.dedupe(matched.map(({ host }) => host)).filter(({ platforms = [platform] }) => platforms.includes(platform));
    const installing = yield* Effect.filter(local, ({ install }) =>
        UndefinedOr.match(install?.executable, {
            onUndefined: () => Effect.succeed(true),
            onDefined: (name) =>
                Effect.map(
                    Effect.promise(() => which(name, { nothrow: true })),
                    Predicate.isNotNull,
                ),
        }),
    );
    const one = Effect.fnUntraced(function* ({ file, host, fields }: (typeof matched)[number]) {
        const root = posix.join(file, posix.relative(host.manifest, '.'));
        const lock = Option.map(yield* Effect.findFirst(Array.cartesianWith(above(root), Array.fromNullishOr(host.build?.lock), posix.join), (at) => fs.exists(join(workspaceRoot, at))), (at) => ({ lock: at }));
        const stamp = local.includes(host) && host.engine !== undefined ? yield* host.engine.stamp(join(workspaceRoot, file)) : Option.none<string>();
        const engined = local.includes(host) && (host.engine === undefined || Option.isSome(stamp));
        const stated = yield* manifested(workspaceRoot, root);
        const withheld: readonly ((typeof WITHHELD)[number] | 'install')[] = [...(engined ? [] : WITHHELD), ...(installing.includes(host) ? [] : (['install'] as const))];
        const dependents = Option.getOrElse(
            Option.flatMap(Record.get(named, root), (name) => Option.map(Record.get(requiring, name), Array.map(Struct.get('name')))),
            Array.empty<string>,
        );
        return { file, root, named: namedInput(root, context), fields, host: Struct.omit(host, withheld), engines: Option.toArray(stamp), stated, dependents, ...Option.getOrUndefined(lock) } satisfies Declared;
    });
    const [declared, refused] = yield* Effect.partition(matched, (row) => Effect.mapError(one(row), (error): [string, Error] => [row.file, error]), { concurrency: 'unbounded' });
    return { declared, failures: [...undecoded, ...unread, ...refused] };
});

const placed = (operation: keyof typeof FOLDERS) => `{options.outputPath}/${FOLDERS[operation]}`;

const projects = (declared: readonly Declared[], settings: Options, exec: string, release: readonly string[]) => {
    const holds = (other: Declared, item: Declared) => (other.fields.folders ?? []).some((folder) => !posix.relative(posix.join(other.root, folder), item.root).startsWith('..')) && !(other.fields.references ?? []).some((reference) => reference.name === item.fields.name && !reference.enabled);
    const hosts = Function.memoize((item: Declared) => (item.fields.folders === undefined ? declared.filter((other) => holds(other, item)) : [item]));
    const uses = Function.memoize((item: Declared) => declared.filter((other) => other !== item && (item.fields.references ?? []).some((reference) => reference.enabled && reference.name === other.fields.name)));
    const users = (item: Declared) => declared.filter((other) => !hosts(item).includes(other) && uses(other).includes(item));
    const ran = `${exec} nx run --help`;
    return declared.map((item) => {
        const loading = Array.dedupe([...hosts(item), ...users(item)]);
        const hosting = loading.filter((other) => other !== item);
        const lone = item.fields.folders === undefined && item.host.standalone === true && !hosting.some((other) => other.host.build !== undefined);
        const offered: Host = lone ? Struct.omit(item.host, WITHHELD) : item.host;
        const { technology, archive, platforms = [], standalone = false, configurations = [], build, operations = {}, install, publish, write, output } = offered;
        const forwarded = Array.match(configurations, { onEmpty: () => ({}), onNonEmpty: (names) => ({ configurations: Record.fromIterableWith(names, (name) => [name, {}] as const) }) });
        const manifest = posix.relative(item.root, item.file);
        const packages = item.fields.packages ?? {};
        const linked = Array.dedupe([...[...declared.filter((other) => other !== item && hosts(other).includes(item)), ...uses(item)].map((other) => other.fields.name), ...Object.values(packages).map(({ project }) => project)]);
        const outputPath = settings.outputPath ?? posix.join('{projectRoot}', output);
        const options = standalone ? { outputPath, descriptor: item.file, hosts: loading.map((other) => other.file), dependencies: uses(item).map((other) => other.file), ...(Record.isEmptyRecord(packages) ? {} : { packages }) } : { outputPath };
        const inputs: NonNullable<TargetConfiguration['inputs']> = [
            item.named,
            `^${item.named}`,
            ...Array.fromNullishOr(build?.pack).map(() => UPSTREAM),
            ...Array.fromNullishOr(item.lock).map((lock) => `{workspaceRoot}/${lock}`),
            ...Array.fromNullishOr(build?.runtime).flatMap((runtimes) => runtimes.map((runtime) => ({ runtime }))),
            ...Array.dedupe(loading.flatMap((other) => other.engines)).map((stamp) => ({ runtime: ['cat', stamp].map(quoteShellArg).join(' ') })),
            { externalDependencies: [] },
        ];
        const engined: NonNullable<TargetConfiguration['inputs']> = [...inputs, ...hosting.map((other) => UndefinedOr.match(other.host.engine, { onUndefined: () => `{workspaceRoot}/${other.file}`, onDefined: ({ fields }) => ({ json: `{workspaceRoot}/${other.file}`, fields: [...fields] }) }))];
        const loaded: NonNullable<TargetConfiguration['inputs']> = [...inputs, ...hosting.map((other) => `{workspaceRoot}/${other.file}`)];
        const required = Array.match(
            uses(item).filter((other) => !hosts(other).includes(item)),
            { onEmpty: () => [], onNonEmpty: (needed) => [{ projects: needed.map((other) => other.fields.name), target: 'install' }] },
        );
        const builder = UndefinedOr.match(build, { onUndefined: () => 'build', onDefined: () => `build--${technology}` });
        const packer = UndefinedOr.match(operations.pack, { onUndefined: () => builder, onDefined: () => `pack--${technology}` });
        const held = Array.match(
            hosts(item).filter((other) => other.host.build !== undefined),
            { onEmpty: () => [], onNonEmpty: (holders) => [{ projects: holders.map((other) => other.fields.name), target: 'build' }] },
        );
        const operated = (verb: keyof NonNullable<Host['operations']>, extra: TargetConfiguration<Readonly<Record<string, unknown>>>) =>
            Option.map(Option.all({ built: Option.fromUndefinedOr(build), operation: Option.fromUndefinedOr(operations[verb]) }), ({ built: { executor }, operation: { command, options: added } }): TargetConfiguration => ({
                executor: `${PLUGIN}:${executor}`,
                parallelism: false,
                ...extra,
                options: { ...options, ...added, ...extra.options },
                metadata: { description: `${String.capitalize(verb)}s the ${technology} project its ${manifest} declares with ${command}`, help: { command: ran, example: { options: added } } },
            }));
        const written = [
            ...UndefinedOr.match(operations.pack, {
                onUndefined: () => [...Array.fromNullishOr(archive).map((extension) => `{options.outputPath}/*${extension}`), ...(build?.outputs?.(item.fields) ?? []).map((at) => posix.join('{projectRoot}', at))],
                onDefined: () => [placed('build')],
            }),
            ...Record.keys(packages).map((folder) => posix.join('{projectRoot}', folder, '*')),
        ];
        const installed = Option.firstSomeOf([
            Option.map(
                Option.filter(Option.fromUndefinedOr(install), ({ platforms: on = [platform] }) => on.includes(platform)),
                ({ command, help = ran, unpacked = false }): TargetConfiguration => ({
                    executor: `${PLUGIN}:install`,
                    dependsOn: [unpacked ? `build--!(${technology})` : builder],
                    inputs: RECEIVED,
                    ...forwarded,
                    metadata: { description: `Installs what build wrote in the run's configuration where its host loads it with ${command}`, help: { command: help, example: { args: [] } } },
                }),
            ),
            Option.flatMap(Option.liftPredicate(users(item), Array.isReadonlyArrayNonEmpty), (loaders) => operated('install', { dependsOn: [builder, ...held], inputs: UNSTATED, options: { users: loaders.map((other) => other.file) } })),
            Option.map(Array.head(held), (host): TargetConfiguration => ({ executor: 'nx:noop', dependsOn: [host], inputs: UNSTATED, metadata: { description: `Builds ${host.projects.join(', ')}, the host the result loads in, with its build target`, help: { command: ran, example: { options: {} } } } })),
        ]);
        const own = Record.map(
            Record.getSomes({
                build: Option.map(Option.fromUndefinedOr(build), ({ executor, command, help = ran, pack }): TargetConfiguration => ({
                    executor: `${PLUGIN}:${executor}`,
                    ...Array.match([...Array.fromNullishOr(pack).map(() => 'build--*'), ...required], { onEmpty: () => ({}), onNonEmpty: (dependsOn) => ({ dependsOn }) }),
                    options,
                    ...(standalone && { parallelism: false }),
                    ...(written.length > 0 && { cache: true }),
                    inputs: engined,
                    outputs: written,
                    ...forwarded,
                    metadata: { description: `Builds the ${technology} project its ${manifest} declares with ${command}`, help: { command: help, example: { options: { outputPath } } } },
                })),
                check: Option.firstSomeOf([
                    operated('check', { cache: true, inputs: engined, ...Array.match(required, { onEmpty: () => ({}), onNonEmpty: (dependsOn) => ({ dependsOn }) }), outputs: [] }),
                    Option.as(
                        Option.filter(Option.fromUndefinedOr(build), () => standalone),
                        { executor: 'nx:noop', dependsOn: [builder], inputs: engined, metadata: { description: `Checks the ${technology} project with its ${builder} compile`, help: { command: ran, example: { options: {} } } } },
                    ),
                ]),
                test: Option.filter(operated('test', { cache: true, inputs: loaded, dependsOn: [`install--${technology}`], outputs: [placed('test')] }), () => Array.isReadonlyArrayNonEmpty(hosting)),
                pack: operated('pack', { cache: true, inputs: engined, dependsOn: [builder], outputs: [placed('pack')] }),
                install: installed,
            }),
            (target): Record<string, TargetConfiguration> => ({ [technology]: target }),
        );
        const removed = Record.collect(own, (verb, tools) => Record.keys(Record.filter(tools, ({ outputs = [] }) => outputs.length > 0)).map((tool) => `${verb}--${tool}`)).flat();
        const verbs: Verbs = {
            ...own,
            clean: Array.match(removed, {
                onEmpty: () => ({}),
                onNonEmpty: (labels) => ({ [technology]: { executor: `${PLUGIN}:clean`, options: { targets: labels }, inputs: UNSTATED, metadata: { description: `Removes the outputs ${labels.join(', ')} declare`, help: { command: ran, example: { options: { targets: labels } } } } } }),
            }),
            attach: {
                ...(archive !== undefined && {
                    gh: {
                        executor: `${PLUGIN}:attach`,
                        dependsOn: [packer, ...Array.match(item.dependents, { onEmpty: () => [], onNonEmpty: (dependents) => [{ projects: [...dependents], target: 'pack--*' }] })],
                        inputs: RELEASED,
                        metadata: {
                            description: `Uploads the files ${packer} wrote, with the visible files the pack targets of each package requiring the project wrote, to the GitHub release of the tag nx release made for the project at the checked-out commit with gh release upload`,
                            help: { command: 'gh release upload --help', example: { options: { dryRun: true } } },
                        },
                    },
                }),
            },
        };
        const released = Option.filter(Option.fromUndefinedOr(write), () => item.fields.version !== undefined);
        const stored = Option.map(Option.fromUndefinedOr(publish), ({ store, command, credential }) => ({ store, command, variable: settings.storeCredentials?.[technology] ?? credential }));
        const configuration: ProjectConfiguration = {
            root: item.root,
            name: item.fields.name,
            metadata: { technologies: [technology] },
            tags: [...(Option.isSome(released) ? release : []), ...platforms.map((os) => `os:${os}`), ...(item.host.hosted === true ? [`host:${technology}`] : [])],
            ...Array.match(standalone ? linked : [], { onEmpty: () => ({}), onNonEmpty: (implicitDependencies) => ({ implicitDependencies }) }),
            ...Option.getOrUndefined(Option.map(released, () => ({ release: { version: { versionActions: `${PLUGIN}/release/version-actions`, versionActionsOptions: { manifest } } } }))),
            targets: {
                ...aggregated(verbs, item.stated, technology),
                ...Record.fromIterableWith(Option.toArray(stored), ({ store, command, variable }): [string, TargetConfiguration] => [
                    'nx-release-publish',
                    {
                        executor: `${PLUGIN}:release-publish`,
                        dependsOn: [packer, '^nx-release-publish'],
                        inputs: RECEIVED,
                        options: { credential: variable },
                        metadata: { technologies: [technology], description: `Publishes the ${archive} packages of the project to ${store} with ${command}, under the credential ${variable} holds`, help: { command: ran, example: { options: { dryRun: true } } } },
                    },
                ]),
            },
        };
        return [item.file, { projects: { [item.root]: configuration } }] as const;
    });
};

const upgrading = (listed: readonly string[]): Verbs => ({
    upgrade: {
        jactionlint: {
            executor: `${PLUGIN}:upgrade`,
            options: { cwd: '{projectRoot}', commands: [{ apply: ['jactionlint', 'update', '--apply', ...listed], preview: ['jactionlint', 'update', '--diff', ...listed] }] },
            inputs: [posix.join('{projectRoot}', '.github', '**', '*'), { externalDependencies: [] }],
            metadata: {
                description: 'Moves every external action and reusable workflow the workflows and composite actions under .github use to its newest release with jactionlint update --apply over those files, keeping commit pins and their version comments',
                help: { command: 'jactionlint update --help', example: { options: { dryRun: true } } },
            },
        },
    },
});

const workflows = (files: readonly string[], workspaceRoot: string) =>
    Effect.forEach(
        Array.dedupeWith(
            files.filter((file) => posix.matchesGlob(file, WORKFLOWS)).map((file) => ({ file, root: posix.join(file, '..', '..', '..') })),
            (left, right) => left.root === right.root,
        ),
        ({ file, root }) => {
            const listed = files.filter((at) => [WORKFLOWS, COMPOSITES].some((glob) => posix.matchesGlob(at, glob)) && posix.relative(root, at).split('/')[0] === '.github').map((at) => posix.relative(root, at));
            return Effect.map(manifested(workspaceRoot, root), (stated) => [file, { projects: { [root]: { root, targets: aggregated(upgrading(listed), stated, 'github-actions') } } }] as const);
        },
    );

const workspace = (files: readonly string[], workspaceRoot: string) =>
    Effect.forEach(
        files.filter((file) => file === MISE),
        (file) => Effect.map(manifested(workspaceRoot, '.'), (stated) => [file, { projects: { '.': { root: '.', targets: aggregated(ROOT, stated, 'mise') } } }] as const),
    );

const recorded = Effect.fnUntraced(function* ({ id, argv, startTime, endTime }: PostTasksExecutionContext, rows: Array.NonEmptyReadonlyArray<{ readonly task: Task; readonly status: TaskResult['status']; readonly kind: 'local' | 'remote' | 'missed' }>) {
    const [fs, summary] = yield* Effect.all([FileSystem.FileSystem, Config.option(Config.String('GITHUB_STEP_SUMMARY'))]);
    const { local = [], remote = [], missed = [] } = Array.groupBy(rows, Struct.get('kind'));
    const hits = local.length + remote.length;
    const rate = hits / rows.length;
    const slowest = Array.sortWith(
        missed.map(({ task, status }) => ({ task: task.id, status, seconds: ((task.endTime ?? 0) - (task.startTime ?? 0)) / 1000 })),
        ({ seconds }) => seconds,
        Order.flip(Order.Number),
    ).slice(0, RECORD.slowest);
    const targets = Record.map(
        Array.groupBy(rows, ({ task }) => task.target.target),
        (held) => ({ hits: Array.countBy(held, ({ kind }) => kind !== 'missed'), misses: Array.countBy(held, ({ kind }) => kind === 'missed') }),
    );
    const line = yield* Schema.encodeEffect(Line)({ id, at: DateTime.formatIso(DateTime.makeUnsafe(endTime)), command: argv.slice(2).join(' '), seconds: (endTime - startTime) / 1000, local: local.length, remote: remote.length, missed: missed.length, rate, targets, slowest });
    const at = join(workspaceDataDirectory, RECORD.file);
    yield* fs.makeDirectory(workspaceDataDirectory, { recursive: true });
    const size = yield* fs.stat(at).pipe(
        Effect.map(({ size: held }) => Number(held)),
        Effect.catchReason('PlatformError', 'NotFound', () => Effect.succeed(0)),
    );
    yield* size + line.length < RECORD.bound ? fs.writeFileString(at, `${line}\n`, { flag: 'a' }) : Effect.flatMap(fs.readFileString(at), (held) => fs.writeFileString(at, `${held.slice(held.indexOf('\n', held.length - RECORD.bound / 2) + 1)}${line}\n`));
    const tally = [`Nx cache: ${hits} of ${rows.length} cacheable tasks hit (${SHARE.format(rate)}: ${local.length} local, ${remote.length} remote)`, ...slowest.slice(0, 1).map(({ task, seconds }) => `slowest miss ${task} at ${Duration.format(Duration.seconds(seconds))}`)].join('; ');
    yield* Effect.forEach(Option.toArray(summary), (file) => fs.writeFileString(file, `${tally}\n`, { flag: 'a' }), { discard: true });
});

// --- [COMPOSITION] ---------------------------------------------------------------------

const createNodes: CreateNodes = [
    FILES,
    inferring(Options, run, function* (files, settings, context) {
        const [{ declared, failures }, actions, root] = yield* Effect.all([read(files, context), workflows(files, context.workspaceRoot), workspace(files, context.workspaceRoot)]);
        const results = [
            ...projects(
                declared.filter(({ fields }) => fields.installed !== true),
                settings,
                getPackageManagerCommand(detectPackageManager(context.workspaceRoot)).exec,
                tagged(context.nxJsonConfiguration),
            ),
            ...actions,
            ...root,
        ];
        return yield* Array.match(failures, { onEmpty: () => Effect.succeed(results), onNonEmpty: (refused) => Effect.fail(new AggregateCreateNodesError(refused, results)) });
    }),
];

const preTasksExecution: PreTasksExecution = (options, { id }) =>
    run(
        Effect.gen(function* () {
            const [{ remoteCacheTimeout = Duration.infinity }, server, nested, token, annotated] = yield* Effect.all([
                Schema.decodeEffect(Options)(options ?? {}),
                Config.option(Config.String(SERVER)),
                Config.option(Config.String('NX_TASK_TARGET_PROJECT')),
                Config.option(Config.Redacted('NX_SELF_HOSTED_REMOTE_CACHE_ACCESS_TOKEN')),
                Config.withDefault(Config.Boolean('GITHUB_ACTIONS'), false),
            ]);
            const authorized = Option.match(token, { onNone: () => Function.identity<HttpClientRequest.HttpClientRequest>, onSome: (held) => HttpClientRequest.bearerToken(held) });
            const [, refused] = yield* Effect.partition(
                Option.toArray(server).filter((url) => String.isNonEmpty(url) && Option.isNone(nested)),
                (url) =>
                    HttpClient.execute(authorized(HttpClientRequest.get(`${url}/v1/cache/${id}`, { accept: 'application/octet-stream' }))).pipe(
                        Effect.flatMap(HttpClientResponse.filterStatus((status) => status === 200 || status === 404)),
                        Effect.timeout(remoteCacheTimeout),
                        Effect.mapError((error) => `Remote cache ${url} serves no read, so this run uses the local cache alone: ${Formatter.format(error)}`),
                    ),
            );
            yield* Effect.forEach(
                refused,
                (text) =>
                    Effect.sync(() => {
                        Object.assign(process.env, { [SERVER]: '' });
                        emitPluginWorkerLog('warn', annotated ? `::warning title=Nx remote cache::${text}` : text);
                    }),
                { discard: true },
            );
        }).pipe(Effect.provide(FetchHttpClient.layer)),
    );

const postTasksExecution: PostTasksExecution = (_, context) =>
    run(
        Effect.catchCause(
            Array.match(
                Array.getSomes(
                    Object.values(context.taskResults).map(({ task, status }) =>
                        Option.map(
                            Option.filter(Option.fromUndefinedOr(KINDS[status]), () => task.cache),
                            (kind) => ({ task, status, kind }),
                        ),
                    ),
                ),
                { onEmpty: () => Effect.void, onNonEmpty: (rows) => recorded(context, rows) },
            ),
            (cause) => Effect.sync(() => emitPluginWorkerLog('warn', `Task cache record of run ${context.id} is not written: ${Cause.pretty(cause)}`)),
        ),
    );

// --- [EXPORTS] -------------------------------------------------------------------------

export { createNodes, postTasksExecution, preTasksExecution };

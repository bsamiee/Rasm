import { join, posix, relative } from 'node:path';
import process from 'node:process';
import { fileURLToPath } from 'node:url';
import { parseArgs } from 'node:util';
import { AggregateCreateNodesError, type CreateNodes, type CreateNodesContext, detectPackageManager, normalizePath, type PostTasksExecution, type PreTasksExecution, type ProjectConfiguration, type TargetConfiguration, type TaskResult } from '@nx/devkit';
import { combineGlobPatterns, emitPluginWorkerLog, quoteShellArg } from '@nx/devkit/internal';
import { above, type Anchor, aggregated, anchored, EXEC, EXECUTORS, inferring, inside, manifested, namedInput, removable, run, tagged, type Verbs } from '@rasm/nx';
import { Array, Config, Duration, Effect, FileSystem, Formatter, Function, Option, Record, Result, Schema, String, Struct, UndefinedOr } from 'effect';
import { FetchHttpClient, HttpClient, HttpClientRequest } from 'effect/http';
import { FOLDERS, HOSTS, type Host, routed } from '#/host-packages';
import plugin from '#/package.json' with { type: 'json' };

// --- [CONSTANTS] -----------------------------------------------------------------------

const SERVER = 'NX_SELF_HOSTED_REMOTE_CACHE_SERVER';
const UNCACHED = { flags: ['skip-nx-cache', 'skipNxCache', 'skip-remote-cache', 'skipRemoteCache'], variables: ['NX_SKIP_NX_CACHE', 'NX_DISABLE_NX_CACHE', 'NX_SKIP_REMOTE_CACHE', 'NX_DISABLE_REMOTE_CACHE'] } as const;
const ANNOTATED = Config.withDefault(Config.Boolean('GITHUB_ACTIONS'), false);
const FAILED: ReadonlySet<TaskResult['status']> = new Set(['failure', 'stopped']);
const PACKAGE = 'package.json';
const MISE = '.config/mise.toml';
const FILES = combineGlobPatterns(Array.dedupe([...HOSTS.map(({ manifest }) => `**/${manifest}`), `**/${PACKAGE}`]));
const UPSTREAM = { dependentTasksOutputFiles: '**/*' } as const;
const RECEIVED: NonNullable<TargetConfiguration['inputs']> = [UPSTREAM, { externalDependencies: [] }];
const UNSTATED: NonNullable<TargetConfiguration['inputs']> = ['default', '^default', { externalDependencies: [] }];
const ROOT: Verbs = {
    clean: {
        mise: {
            executor: EXECUTORS.clean,
            dependsOn: [{ projects: '*', target: 'clean--!(mise)', params: 'forward' }],
            options: { folders: [['ARTIFACTS_DIR']] },
            configurations: {
                cache: {
                    commands: [
                        ['mise', 'prune', '--yes'],
                        ['nx', 'reset', '--only-cache'],
                    ],
                    folders: [['CACHE_DIR']],
                },
            },
            inputs: UNSTATED,
            metadata: {
                description: `Empties the folder ARTIFACTS_DIR of ${MISE} names once every other clean target ran, and under configuration cache runs mise prune, empties the Nx task cache with nx reset --only-cache, and empties the folder CACHE_DIR names but the entry holding Nx workspace data`,
                help: { command: 'mise prune --help', example: { options: { folders: [['ARTIFACTS_DIR']] } } },
            },
        },
    },
    upgrade: {
        mise: {
            executor: EXECUTORS.upgrade,
            dependsOn: [{ projects: '*', target: 'upgrade--!(mise)', params: 'forward' }],
            options: { cwd: '.', commands: [{ apply: ['mise', 'upgrade'], preview: ['mise', 'upgrade', '--dry-run'] }] },
            inputs: [{ externalDependencies: [] }],
            metadata: { description: `Moves every tool row of ${MISE} to its newest release and rewrites its lock with mise upgrade, once every other upgrade target ran`, help: { command: 'mise upgrade --help', example: { options: { dryRun: true } } } },
        },
    },
};
const ACTIONS: Verbs = {
    upgrade: {
        jactionlint: {
            executor: EXECUTORS.upgrade,
            options: { cwd: '.', commands: [{ apply: ['jactionlint', 'update', '--apply'], preview: ['jactionlint', 'update', '--diff'] }] },
            inputs: ['{projectRoot}/.github/**/*', { externalDependencies: [] }],
            metadata: {
                technologies: ['github-actions'],
                description: 'Moves every external action and reusable workflow the workflows and composite actions of the repository use to its newest release with jactionlint update --apply, keeping commit pins and their version comments',
                help: { command: 'jactionlint update --help', example: { options: { dryRun: true } } },
            },
        },
    },
};
const PIXI: Verbs = {
    upgrade: {
        pixi: {
            executor: EXECUTORS.upgrade,
            options: { cwd: '.', commands: [{ apply: ['pixi', 'update'], preview: ['pixi', 'update', '--dry-run'] }] },
            inputs: ['{projectRoot}/pixi.toml', '{projectRoot}/pyproject.toml', '{projectRoot}/pixi.lock', { externalDependencies: [] }],
            metadata: {
                technologies: ['pixi'],
                description: 'Moves every package of the Pixi environments to the newest release its requirement allows and rewrites pixi.lock with pixi update',
                help: { command: 'pixi update --help', example: { options: { dryRun: true } } },
            },
        },
    },
};
const HELD = [
    [MISE, ROOT],
    ['.github/workflows', ACTIONS],
    ['pixi.lock', PIXI],
] as const;

// --- [MODELS] --------------------------------------------------------------------------

class Options extends Schema.Opaque<Options, unknown>()(
    Schema.Struct({
        outputPath: Schema.optionalKey(Schema.String),
        storeCredentials: Schema.optionalKey(Schema.Record(Schema.String, Schema.String)),
        remoteCacheTimeout: Schema.optionalKey(Schema.DurationFromString),
    }).annotate({
        description:
            'outputPath: workspace folder each host package is written to, {projectName} naming the project, absent writes where the host tool writes in the project. storeCredentials: environment variable holding the store credential per technology, absent naming the variable of the host row (Blender: BLENDER_EXTENSIONS_TOKEN, Yak: YAK_TOKEN, Chrome: CHROME_WEB_STORE_TOKEN, an OAuth access token of scope chromewebstore beside CHROME_WEB_STORE_PUBLISHER naming the publisher), a publish whose variable is unset naming it and succeeding. remoteCacheTimeout: longest wait, as a number and a unit (5 seconds), for the answer to the one read each nx run no task started sends the remote cache server before its tasks, where neither --skip-nx-cache, --skip-remote-cache, nor their NX_SKIP_* and NX_DISABLE_* variables keep the run off that cache, a read answered other than 200 or 404, refused, or unanswered that long printing one warning and emptying NX_SELF_HOSTED_REMOTE_CACHE_SERVER for that run, absent waits without bound',
    }),
) {}

const Requirements = Schema.Record(Schema.String, Schema.String).pipe(Schema.withDecodingDefaultKey(Effect.succeed({})));
const Member = Schema.fromJsonString(Schema.Struct({ name: Schema.String, dependencies: Requirements, devDependencies: Requirements, optionalDependencies: Requirements, peerDependencies: Requirements })).annotate({
    description: 'The name of a workspace package.json and the packages each of its dependency fields requires',
});

// --- [OPERATIONS] ----------------------------------------------------------------------

const read = Effect.fnUntraced(function* (files: readonly string[], anchor: Anchor, context: CreateNodesContext) {
    const fs = yield* FileSystem.FileSystem;
    const { workspaceRoot } = context;
    const [packages, manifests] = Array.partition(files, (file) => Result.liftPredicate(file, (at) => posix.basename(at) === PACKAGE, Function.identity));
    const [members, undecoded] = yield* Effect.partition(
        packages.filter((file) => anchor.holds(posix.dirname(file))),
        (file) =>
            fs
                .readFileString(join(workspaceRoot, file))
                .pipe(Effect.flatMap(Schema.decodeEffect(Member)), Effect.mapBoth({ onFailure: (error): [string, Error] => [file, error], onSuccess: ({ name, ...required }) => ({ root: posix.dirname(file), name, required: Object.values(required).flatMap((held) => Object.keys(held)) }) })),
        { concurrency: 'unbounded' },
    );
    const requiring = Array.groupBy(
        members.flatMap(({ name, required }) => required.map((dependency) => ({ dependency, name }))),
        Struct.get('dependency'),
    );
    const dependents = Record.fromIterableWith(members, ({ root, name }) => [root, (requiring[name] ?? []).map(Struct.get('name'))]);
    const [held, unread] = yield* Effect.partition(
        manifests,
        (file) =>
            fs.readFileString(join(workspaceRoot, file)).pipe(
                Effect.flatMap((text) => routed(file, text)),
                Effect.mapError((error): [string, Error] => [file, error]),
            ),
        { concurrency: 'unbounded' },
    );
    const [declared, refused] = yield* Effect.partition(
        Array.getSomes(held),
        Effect.fnUntraced(
            function* ({ file, host, fields }) {
                const root = posix.join(file, posix.relative(host.manifest, '.'));
                const [lock, stated] = yield* Effect.all([Effect.findFirst(Array.cartesianWith(above(root), Array.fromNullishOr(host.build?.lock), posix.join), (at) => fs.exists(join(workspaceRoot, at))), manifested(workspaceRoot, root)]);
                return { file, root, named: namedInput(root, context), fields, host, stated, dependents: dependents[root] ?? [], lock };
            },
            (effect, { file }) => Effect.mapError(effect, (error): [string, Error] => [file, error]),
        ),
        { concurrency: 'unbounded' },
    );
    return { declared, failures: [...undecoded, ...unread, ...refused] };
});

const placed = (operation: keyof typeof FOLDERS) => `{options.outputPath}/${FOLDERS[operation]}`;

const projects = (declared: Effect.Success<ReturnType<typeof read>>['declared'], settings: Options, context: CreateNodesContext, anchor: Anchor) => {
    const identified = ['node', normalizePath(relative(anchor.workspaceRoot, fileURLToPath(import.meta.resolve('#/host-packages'))))];
    const hosts = Function.memoize((item: (typeof declared)[number]) =>
        item.fields.folders === undefined ? declared.filter((other) => (other.fields.folders ?? []).some((folder) => inside(posix.join(other.root, folder))(item.root)) && !(other.fields.references ?? []).some((reference) => reference.name === item.fields.name && !reference.enabled)) : [item],
    );
    const uses = Function.memoize((item: (typeof declared)[number]) => declared.filter((other) => other !== item && (item.fields.references ?? []).some((reference) => reference.enabled && reference.name === other.fields.name)));
    const [ran, release] = [`${EXEC[detectPackageManager(context.workspaceRoot)]} nx run --help`, tagged(context.nxJsonConfiguration)];
    return declared.map((item) => {
        const users = declared.filter((other) => !hosts(item).includes(other) && uses(other).includes(item));
        const loading = Array.dedupe([...hosts(item), ...users]);
        const hosting = loading.filter((other) => other !== item);
        const offered: Host = item.fields.folders === undefined && item.host.standalone === true && hosting.length === 0 ? Struct.omit(item.host, ['archive', 'build', 'operations', 'publish']) : item.host;
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
            ...Option.toArray(item.lock).map((lock) => `{workspaceRoot}/${lock}`),
            ...Array.fromNullishOr(build?.runtime).flatMap((runtimes) => runtimes.map((runtime) => ({ runtime }))),
            ...loading.filter((other) => other.host.engine !== undefined).map((other) => ({ runtime: [...identified, other.file].map(quoteShellArg).join(' ') })),
            { externalDependencies: [] },
        ];
        const engined: NonNullable<TargetConfiguration['inputs']> = [...inputs, ...hosting.map((other) => UndefinedOr.match(other.host.engine, { onUndefined: () => `{workspaceRoot}/${other.file}`, onDefined: ({ fields }) => ({ json: `{workspaceRoot}/${other.file}`, fields: [...fields] }) }))];
        const loaded: NonNullable<TargetConfiguration['inputs']> = [...inputs, ...hosting.map((other) => ({ input: other.named, projects: other.fields.name }))];
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
            Option.zipWith(Option.fromUndefinedOr(build), Option.fromUndefinedOr(operations[verb]), ({ executor }, command): TargetConfiguration => ({
                executor: `${plugin.name}:${executor}`,
                parallelism: false,
                ...extra,
                options: { ...options, operation: verb, ...extra.options },
                metadata: { description: `${String.capitalize(verb)}s the ${technology} project its ${manifest} declares with ${command}`, help: { command: ran, example: { options: { operation: verb } } } },
            }));
        const written = [
            ...UndefinedOr.match(operations.pack, {
                onUndefined: () => [...Array.fromNullishOr(archive).map((extension) => `{options.outputPath}/*${extension}`), ...(build?.outputs?.(item.fields) ?? []).map((at) => posix.join('{projectRoot}', at))],
                onDefined: () => [placed('build')],
            }),
            ...Record.keys(packages).map((folder) => posix.join('{projectRoot}', folder, '*')),
        ];
        const installed = Option.firstSomeOf([
            Option.map(Option.fromUndefinedOr(install), ({ command, help = ran, unpacked = false }): TargetConfiguration => ({
                executor: `${plugin.name}:install`,
                dependsOn: [unpacked ? `build--!(${technology})` : builder],
                inputs: RECEIVED,
                ...forwarded,
                metadata: { description: `Installs what build wrote in the run's configuration where its host loads it with ${command}`, help: { command: help, example: { args: [] } } },
            })),
            Option.filter(operated('install', { dependsOn: [builder, ...held], inputs: UNSTATED, options: { users: users.map((other) => other.file) } }), () => Array.isReadonlyArrayNonEmpty(users)),
            Option.map(Array.head(held), (host): TargetConfiguration => ({ executor: 'nx:noop', dependsOn: [host], inputs: UNSTATED, metadata: { description: `Builds ${host.projects.join(', ')}, the host the result loads in, with its build target`, help: { command: ran, example: { options: {} } } } })),
        ]);
        const own: Record<string, TargetConfiguration> = Record.getSomes({
            build: Option.map(Option.fromUndefinedOr(build), ({ executor, command, help = ran, pack }): TargetConfiguration => ({
                executor: `${plugin.name}:${executor}`,
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
        });
        const work = Record.map(own, (target) => ({ [technology]: target }));
        const removed = removable(work);
        const attached = Option.map(Option.fromUndefinedOr(archive), (): TargetConfiguration => ({
            executor: EXECUTORS.attach,
            dependsOn: [packer, ...Array.match(item.dependents, { onEmpty: () => [], onNonEmpty: (dependents) => [{ projects: [...dependents], target: 'pack--*' }] })],
            inputs: [{ dependentTasksOutputFiles: '**/[!.]*' }, { externalDependencies: [] }],
            metadata: {
                description: `Uploads the files ${packer} wrote, with the visible files the pack targets of each package requiring the project wrote, to the GitHub release of the tag nx release made for the project at the checked-out commit with gh release upload, creating that release with gh release create where none exists`,
                help: { command: 'gh release upload --help', example: { options: { dryRun: true } } },
            },
        }));
        const verbs: Verbs = {
            ...work,
            clean: Array.match(removed, {
                onEmpty: () => ({}),
                onNonEmpty: (labels) => ({ [technology]: { executor: EXECUTORS.clean, options: { targets: labels }, inputs: UNSTATED, metadata: { description: `Removes the outputs ${labels.join(', ')} declare`, help: { command: ran, example: { options: { targets: labels } } } } } }),
            }),
            attach: Record.getSomes({ gh: attached }),
        };
        const released = Option.filter(Option.fromUndefinedOr(write), () => item.fields.version !== undefined);
        const stored = Option.map(Option.fromUndefinedOr(publish), ({ store, command, credential }) => ({ store, command, variable: settings.storeCredentials?.[technology] ?? credential }));
        const published = Option.map(stored, ({ store, command, variable }): TargetConfiguration => ({
            executor: `${plugin.name}:release-publish`,
            dependsOn: [packer, '^nx-release-publish'],
            inputs: RECEIVED,
            options: { credential: variable },
            metadata: { technologies: [technology], description: `Publishes the ${archive} packages of the project to ${store} with ${command}, under the credential ${variable} holds`, help: { command: ran, example: { options: { dryRun: true } } } },
        }));
        const configuration: ProjectConfiguration = {
            root: item.root,
            name: item.fields.name,
            metadata: { technologies: [technology] },
            tags: [...(Option.isSome(released) ? release : []), ...platforms.map((os) => `os:${os}`), ...(item.host.hosted === true ? [`host:${technology}`] : [])],
            ...Array.match(standalone ? linked : [], { onEmpty: () => ({}), onNonEmpty: (implicitDependencies) => ({ implicitDependencies }) }),
            ...Option.getOrUndefined(Option.map(released, () => ({ release: { version: { versionActions: `${plugin.name}/release/version-actions`, versionActionsOptions: { manifest } } } }))),
            targets: aggregated(verbs, item.stated, technology, anchor, Record.getSomes({ 'nx-release-publish': published })),
        };
        return [item.file, { projects: { [item.root]: configuration } }] as const;
    });
};

const workspace = Effect.fnUntraced(function* (files: readonly string[], anchor: Anchor) {
    const fs = yield* FileSystem.FileSystem;
    const held = yield* Effect.filter(HELD, ([at]) => fs.exists(join(anchor.workspaceRoot, at)));
    const verbs = held.reduce<Verbs>((all, [, more]) => Record.union(all, more, Struct.assign<Verbs[string], Verbs[string]>), {});
    return yield* Effect.forEach(Record.isEmptyRecord(verbs) ? [] : files.filter((file) => file === PACKAGE), (file) => Effect.map(manifested(anchor.workspaceRoot, '.'), (stated) => [file, { projects: { '.': { root: '.', targets: aggregated(verbs, stated, 'mise', anchor) } } }] as const));
});

// --- [COMPOSITION] ---------------------------------------------------------------------

const createNodes: CreateNodes = [
    FILES,
    inferring(Options, run, function* (files, settings, context) {
        const anchor = anchored(context.workspaceRoot);
        const [{ declared, failures }, root] = yield* Effect.all([read(files, anchor, context), workspace(files, anchor)]);
        const results = [
            ...projects(
                declared.filter(({ fields }) => fields.installed !== true),
                settings,
                context,
                anchor,
            ),
            ...root,
        ];
        return yield* Array.match(failures, { onEmpty: () => Effect.succeed(results), onNonEmpty: (refused) => Effect.fail(new AggregateCreateNodesError(refused, results)) });
    }),
];

const preTasksExecution: PreTasksExecution = (options, { id, argv }) =>
    run(
        Effect.gen(function* () {
            const [{ remoteCacheTimeout = Duration.infinity }, server, nested, token, annotated, uncached, client] = yield* Effect.all([
                Schema.decodeEffect(Options)(options ?? {}),
                Config.option(Config.String(SERVER)),
                Config.option(Config.String('NX_TASK_TARGET_PROJECT')),
                Config.option(Config.Redacted('NX_SELF_HOSTED_REMOTE_CACHE_ACCESS_TOKEN')),
                ANNOTATED,
                Config.all(UNCACHED.variables.map((name) => Config.withDefault(Config.Boolean(name), false))),
                HttpClient.HttpClient,
            ]);
            const { values } = parseArgs({ args: argv.slice(2), strict: false, allowPositionals: true, allowNegative: true, options: Record.fromIterableWith(UNCACHED.flags, (flag) => [flag, { type: 'boolean' }] as const) });
            const reader = client.pipe(
                HttpClient.mapRequest(Option.match(token, { onNone: () => Function.identity<HttpClientRequest.HttpClientRequest>, onSome: (held) => HttpClientRequest.bearerToken(held) })),
                HttpClient.filterStatus((status) => status === 200 || status === 404),
                HttpClient.transformResponse(Effect.timeout(remoteCacheTimeout)),
            );
            const reached = Option.toArray(server).filter((url) => String.isNonEmpty(url) && Option.isNone(nested) && !uncached.includes(true) && !UNCACHED.flags.some((flag) => values[flag] === true));
            const [, refused] = yield* Effect.partition(reached, (url) => Effect.mapError(reader.get(`${url}/v1/cache/${id}`, { accept: 'application/octet-stream' }), (error) => `Remote cache ${url} serves no read, so this run uses the local cache alone: ${Formatter.format(error)}`));
            yield* Effect.forEach(refused, (text) => Effect.all([Effect.sync(() => Object.assign(process.env, { [SERVER]: '' })), Effect.sync(() => emitPluginWorkerLog('warn', annotated ? `::warning title=Nx remote cache::${text}` : text))]), { discard: true });
        }).pipe(Effect.provide(FetchHttpClient.layer)),
    );

const postTasksExecution: PostTasksExecution = (_, { taskResults }) =>
    run(
        Effect.forEach(
            Object.values(taskResults).filter(({ status }) => FAILED.has(status)),
            ({ task: { id }, status, code }) => Effect.sync(() => emitPluginWorkerLog('log', `::error title=${id.replaceAll(/[%\r\n:,]/gu, encodeURIComponent)}::${`${id} ended in ${status} with exit code ${code}`.replaceAll(/[%\r\n]/gu, encodeURIComponent)}`)),
            { discard: true },
        ).pipe(Effect.when(ANNOTATED), Effect.asVoid),
    );

// --- [EXPORTS] -------------------------------------------------------------------------

export { createNodes, postTasksExecution, preTasksExecution };

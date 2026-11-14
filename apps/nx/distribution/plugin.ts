import { constants } from 'node:http2';
import { basename, join, matchesGlob, relative } from 'node:path';
import process, { platform } from 'node:process';
import { AggregateCreateNodesError, type CreateNodes, type CreateNodesContext, hashArray, type PreTasksExecution, type ProjectConfiguration, type TargetConfiguration } from '@nx/devkit';
import { combineGlobPatterns, mergeTargetConfigurations, quoteShellArg } from '@nx/devkit/internal';
import { Array, Cause, Config, Console, Effect, FileSystem, Function, Option, Predicate, Record, Result, Schema, Struct, UndefinedOr } from 'effect';
import { FetchHttpClient, HttpClient, HttpClientRequest } from 'effect/http';
import { run, spawned } from '#/executor';
import { type Fields, HOSTS, type Host } from '#/host-packages';

// --- [TYPES] ---------------------------------------------------------------------------

interface Declared {
    readonly file: string;
    readonly root: string;
    readonly fields: Fields;
    readonly lock?: string;
    readonly engines: readonly string[];
    readonly stated: Readonly<Record<string, TargetConfiguration>>;
    readonly host: Host;
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const PLUGIN = '@rasm/nx-distribution';
const FILES = combineGlobPatterns(Array.dedupe(HOSTS.map(({ manifest }) => `**/${basename(manifest)}`)));
const WITHHELD = ['archive', 'build', 'operations', 'publish'] as const;
const CACHE = { server: 'NX_SELF_HOSTED_REMOTE_CACHE_SERVER', token: 'NX_SELF_HOSTED_REMOTE_CACHE_ACCESS_TOKEN', wait: '5 seconds', served: new Set<number>([constants.HTTP_STATUS_OK, constants.HTTP_STATUS_NOT_FOUND]) } as const;

// --- [MODELS] --------------------------------------------------------------------------

const Placed = Schema.Struct({ outputPath: Schema.optionalKey(Schema.String) }).annotate({ description: 'outputPath: workspace folder each host package is written under as <outputPath>/<technology>/<project name>, absent writes to dist/<project root>' });
const Input = Schema.declare((input: unknown): input is NonNullable<TargetConfiguration['inputs']>[number] => Predicate.isString(input) || Predicate.isObject(input));
const Overrides = Schema.fromJsonString(Schema.Struct({ nx: Schema.optionalKey(Schema.Struct({ targets: Schema.optionalKey(Schema.Record(Schema.String, Schema.Struct({ inputs: Schema.optionalKey(Schema.mutable(Schema.Array(Input))) }))) })) })).annotate({
    description: "The nx.targets rows of a project's package.json, whose inputs Nx merges over the inferred targets after every plugin",
});

// --- [OPERATIONS] ----------------------------------------------------------------------

const read = Effect.fnUntraced(function* (files: readonly string[], { workspaceRoot }: CreateNodesContext) {
    const fs = yield* FileSystem.FileSystem;
    const matched = Array.filterMap(files, (file) =>
        Array.findFirst(HOSTS, (host) => matchesGlob(file, `**/${host.manifest}`)).pipe(
            Result.fromOption(Function.constVoid),
            Result.map((host) => ({ file, host, root: join(file, relative(host.manifest, '.')) })),
        ),
    );
    const local = Array.dedupe(matched.map(({ host }) => host)).filter(({ platforms = [platform] }) => platforms.includes(platform));
    const installing = yield* Effect.filter(local, ({ install }) => UndefinedOr.match(install?.probe, { onUndefined: () => Effect.succeed(true), onDefined: (probe) => Effect.isSuccess(spawned(probe, { stdout: 'ignore', stderr: 'ignore' })) }));
    const one = Effect.fnUntraced(function* ({ file, host, root }: (typeof matched)[number]) {
        const segments = root.split('/');
        const above = Array.makeBy(segments.length + 1, (up) => join(...segments.slice(0, segments.length - up)));
        const lock = Option.map(yield* Effect.findFirst(Array.cartesianWith(above, Array.fromNullishOr(host.build?.lock), join), (at) => fs.exists(join(workspaceRoot, at))), (at) => ({ lock: at }));
        const stamp = local.includes(host) && host.engine !== undefined ? yield* host.engine(join(workspaceRoot, file)) : Option.none<string>();
        const engined = local.includes(host) && (host.engine === undefined || Option.isSome(stamp));
        const fields = yield* Effect.flatMap(fs.readFileString(join(workspaceRoot, file)), (text) => host.read(text, file));
        const manifest = yield* Effect.option(
            fs.readFileString(join(workspaceRoot, root, 'package.json')).pipe(
                Effect.flatMap(Schema.decodeEffect(Overrides)),
                Effect.map(({ nx }) => ({ ...nx?.targets })),
            ),
        );
        const withheld: readonly ((typeof WITHHELD)[number] | 'install')[] = [...(engined ? [] : WITHHELD), ...(installing.includes(host) ? [] : (['install'] as const))];
        return Option.map(
            fields,
            (stated): Declared => ({
                file,
                root,
                fields: stated,
                host: Struct.omit(host, withheld),
                engines: Option.toArray(stamp),
                stated: Option.getOrElse(manifest, Record.empty),
                ...Option.getOrUndefined(lock),
            }),
        );
    });
    return yield* Effect.partition(matched, (row) => Effect.mapError(one(row), (error): [string, Error] => [row.file, error]), { concurrency: 'unbounded' });
});

const projects = (declared: readonly Declared[], placed: Option.Option<string>) => {
    const holds = (other: Declared, item: Declared) => (other.fields.folders ?? []).some((folder) => !relative(join(other.root, folder), item.root).startsWith('..')) && !(other.fields.references ?? []).some((reference) => reference.name === item.fields.name && !reference.enabled);
    const hosts = Function.memoize((item: Declared) => (item.fields.folders === undefined ? declared.filter((other) => holds(other, item)) : [item]));
    const uses = (item: Declared) => declared.filter((other) => other !== item && (item.fields.references ?? []).some((reference) => reference.enabled && reference.name === other.fields.name));
    const users = (item: Declared) => declared.filter((other) => !hosts(item).includes(other) && uses(other).includes(item));
    return declared.map((item) => {
        const loading = Array.dedupe([...hosts(item), ...users(item)]);
        const lone = item.fields.folders === undefined && item.host.standalone === true && !loading.some((other) => other !== item && other.host.build !== undefined);
        const offered: Host = lone ? Struct.omit(item.host, WITHHELD) : item.host;
        const { technology, archive, platforms = [], standalone = false, build, operations = {}, install, publish, write, manifest } = offered;
        const linked = Array.dedupe([...[...declared.filter((other) => other !== item && hosts(other).includes(item)), ...uses(item)].map((other) => other.fields.name), ...Object.values({ ...item.fields.packages }).map(({ project }) => project)]);
        const outputPath = Option.match(placed, { onNone: () => join('dist', item.root), onSome: (at) => join(at, technology, '{projectName}') });
        const settings = standalone ? { outputPath, descriptor: item.file, hosts: loading.map((other) => other.file), dependencies: uses(item).map((other) => other.file), packages: { ...item.fields.packages } } : { outputPath };
        const inputs: NonNullable<TargetConfiguration['inputs']> = [
            '{projectRoot}/**/*',
            '^default',
            ...Array.fromNullishOr(build?.pack).map(() => ({ dependentTasksOutputFiles: '**/*' })),
            ...Array.fromNullishOr(item.lock).map((lock) => `{workspaceRoot}/${lock}`),
            ...Array.fromNullishOr(build?.runtime).flatMap((runtimes) => runtimes.map((runtime) => ({ runtime }))),
            ...Array.dedupe(loading.flatMap((other) => other.engines)).map((stamp) => ({ runtime: ['cat', stamp].map(quoteShellArg).join(' ') })),
            ...loading.filter((other) => other !== item).map((other) => `{workspaceRoot}/${other.file}`),
            { externalDependencies: [] },
        ];
        const required = Array.match(
            uses(item).filter((other) => !hosts(other).includes(item)),
            { onEmpty: () => [], onNonEmpty: (loaded) => [{ projects: loaded.map((other) => other.fields.name), target: 'install' }] },
        );
        const builder = UndefinedOr.match(build, { onUndefined: () => 'build', onDefined: () => `build--${technology}` });
        const packer = UndefinedOr.match(operations.pack, { onUndefined: () => builder, onDefined: () => `pack--${technology}` });
        const held = Array.match(
            hosts(item).filter((other) => other.host.build !== undefined),
            { onEmpty: () => [], onNonEmpty: (holders) => [{ projects: holders.map((other) => other.fields.name), target: 'build' }] },
        );
        const operated = (verb: keyof NonNullable<Host['operations']>, extra: TargetConfiguration) =>
            Option.flatMap(Option.fromUndefinedOr(build), ({ executor }) =>
                Option.map(
                    Option.fromUndefinedOr(operations[verb]),
                    ({ command, options }): TargetConfiguration => ({
                        executor: `${PLUGIN}:${executor}`,
                        parallelism: false,
                        ...extra,
                        options: { ...settings, ...options, ...extra.options },
                        metadata: { description: `Runs ${command} for the ${technology} project its ${manifest} declares` },
                    }),
                ),
            );
        const written = UndefinedOr.match(operations.pack, {
            onUndefined: () => [...Array.fromNullishOr(archive).map((extension) => `{options.outputPath}/*${extension}`), ...(build?.outputs?.(item.fields) ?? []).map((at) => join('{projectRoot}', at))],
            onDefined: () => ['{options.outputPath}/package'],
        });
        const installed = Option.match(
            Option.filter(Option.fromUndefinedOr(install), ({ platforms: on = [platform] }) => on.includes(platform)),
            {
                onSome: ({ command, unpacked = false }) => Option.some<TargetConfiguration>({ executor: `${PLUGIN}:install`, dependsOn: [unpacked ? `build--!(${technology})` : 'build'], metadata: { description: `Installs what build wrote in the run's configuration where its host loads it, through ${command}` } }),
                onNone: () =>
                    Option.orElse(
                        Option.flatMap(Option.liftPredicate(users(item), Array.isReadonlyArrayNonEmpty), (loaded) => operated('install', { dependsOn: [builder, ...held], options: { users: loaded.map((other) => other.file) } })),
                        () => Option.map(Array.head(held), (host): TargetConfiguration => ({ executor: 'nx:noop', dependsOn: [host], metadata: { description: `Builds ${host.projects.join(', ')}, the host the result loads in` } })),
                    ),
            },
        );
        const own = Record.map(
            Record.getSomes({
                build: Option.map(
                    Option.fromUndefinedOr(build),
                    ({ executor, command, pack }): TargetConfiguration => ({
                        executor: `${PLUGIN}:${executor}`,
                        dependsOn: [...Array.fromNullishOr(pack).map(() => 'build--*'), ...required],
                        options: settings,
                        parallelism: !standalone,
                        cache: written.length > 0,
                        inputs,
                        outputs: written,
                        metadata: { description: `Builds the ${technology} project its ${manifest} declares through ${command}` },
                    }),
                ),
                check: Option.orElse(operated('check', { cache: true, inputs, dependsOn: required }), () =>
                    Option.filter(
                        Option.map(Option.fromUndefinedOr(build), (): TargetConfiguration => ({ executor: 'nx:noop', dependsOn: [builder], inputs, metadata: { description: 'Compiles the project as its check' } })),
                        () => standalone,
                    ),
                ),
                test: Option.flatMap(
                    Option.liftPredicate(loading, (all) => all.some((other) => other !== item)),
                    () => operated('test', { cache: true, inputs, dependsOn: [`install--${technology}`], outputs: ['{options.outputPath}/report'] }),
                ),
                pack: operated('pack', { cache: true, inputs, dependsOn: [builder], outputs: ['{options.outputPath}/archive'] }),
                install: installed,
            }),
            (target): Record<string, TargetConfiguration> => ({ [technology]: { ...target, metadata: { technologies: [technology], ...target.metadata } } }),
        );
        const removed = Record.collect(own, (verb, tools) => Record.keys(Record.filter(tools, ({ outputs = [] }) => outputs.length > 0)).map((tool) => `${verb}--${tool}`)).flat();
        const nested: Record<string, Record<string, TargetConfiguration>> = {
            ...own,
            ...Array.match(removed, { onEmpty: () => ({}), onNonEmpty: (labels) => ({ clean: { [technology]: { executor: `${PLUGIN}:clean`, options: { targets: labels }, metadata: { technologies: [technology], description: `Removes the outputs ${labels.join(', ')} declare` } } } }) }),
            ...(archive !== undefined && {
                attach: {
                    gh: {
                        executor: `${PLUGIN}:attach`,
                        dependsOn: [packer],
                        inputs: [{ dependentTasksOutputFiles: '**/*' }],
                        metadata: { technologies: [technology], description: `Uploads the files ${packer} wrote to the GitHub release of the version the project's manifest states through gh release upload` },
                    },
                },
            }),
        };
        const released = Option.filter(Option.fromUndefinedOr(write), () => item.fields.version !== undefined);
        const configuration: ProjectConfiguration = {
            root: item.root,
            name: item.fields.name,
            metadata: { technologies: [technology] },
            tags: [...Option.toArray(released).map(() => 'release'), ...platforms.map((os) => `os:${os}`)],
            ...Array.match(standalone ? linked : [], { onEmpty: () => ({}), onNonEmpty: (implicitDependencies) => ({ implicitDependencies }) }),
            ...Option.getOrUndefined(Option.map(released, () => ({ release: { version: { versionActions: `${PLUGIN}/release/version-actions`, versionActionsOptions: { manifest: relative(item.root, item.file) } } } }))),
            targets: {
                ...Record.fromEntries(
                    Record.collect(nested, (verb, tools): readonly (readonly [string, TargetConfiguration])[] => [
                        [verb, { dependsOn: [`${verb}--*`], inputs: ['...', ...Array.dedupe(Record.collect(tools, (tool, target) => mergeTargetConfigurations(item.stated[`${verb}--${tool}`] ?? {}, { ...target, inputs: target.inputs ?? ['default', '^default'] }).inputs ?? []).flat())] }],
                        ...Record.collect(tools, (tool, target) => [`${verb}--${tool}`, target] as const),
                    ]).flat(),
                ),
                ...Record.fromIterableWith(Array.fromNullishOr(publish), ({ store, command }): [string, TargetConfiguration] => [
                    'nx-release-publish',
                    { executor: `${PLUGIN}:release-publish`, dependsOn: [packer, '^nx-release-publish'], metadata: { description: `Publishes the ${archive} packages of the project to ${store} through ${command}` } },
                ]),
            },
        };
        return [item.file, { projects: { [item.root]: configuration } }] as const;
    });
};

// --- [COMPOSITION] ---------------------------------------------------------------------

const createNodes: CreateNodes = [
    FILES,
    (files, options, context) =>
        run(
            Effect.gen(function* () {
                const { outputPath } = yield* Schema.decodeEffect(Placed)({ ...context.nxJsonConfiguration.pluginsConfig?.[PLUGIN], ...(Predicate.isObject(options) ? options : {}) });
                const [held, failures] = yield* read(files, context);
                const results = projects(
                    Array.getSomes(held).filter(({ fields }) => fields.installed !== true),
                    Option.fromUndefinedOr(outputPath),
                );
                return yield* Array.match(failures, { onEmpty: () => Effect.succeed(results), onNonEmpty: (refused) => Effect.fail(new AggregateCreateNodesError(refused, results)) });
            }),
        ),
];

const preTasksExecution: PreTasksExecution = (_, { id }) =>
    run(
        Effect.gen(function* () {
            const [server, token, annotated] = yield* Effect.all([Config.option(Config.NonEmptyString(CACHE.server)), Config.option(Config.Redacted(CACHE.token)), Config.withDefault(Config.Boolean('GITHUB_ACTIONS'), false)]);
            const stated = Option.map(server, (url) => HttpClientRequest.accept(HttpClientRequest.get(`${url}/v1/cache/${hashArray([id])}`), 'application/octet-stream'));
            const asked = Option.map(stated, (request) => HttpClient.execute(Option.match(token, { onNone: () => request, onSome: (held) => HttpClientRequest.bearerToken(request, held) })));
            const answered = yield* Effect.transposeOption(
                Option.map(asked, (sent) =>
                    Effect.matchCause(Effect.timeout(sent, CACHE.wait), {
                        onSuccess: ({ status }) => Option.liftPredicate(`answered ${status}`, () => !CACHE.served.has(status)),
                        onFailure: (cause) => Option.some(`did not answer: ${Cause.pretty(cause)}`),
                    }),
                ),
            );
            const warning = Option.map(Option.all([server, Option.flatten(answered)]), ([url, answer]) => `Nx remote cache ${url} ${answer}, this run reads and writes the local cache alone`);
            yield* Effect.transposeOption(
                Option.map(warning, (text) =>
                    Effect.andThen(
                        Effect.sync(() => {
                            const { env } = process;
                            return Reflect.set(env, CACHE.server, '');
                        }),
                        Console.warn(annotated ? `::warning title=Nx remote cache::${text.replaceAll('%', '%25').replaceAll('\r', '%0D').replaceAll('\n', '%0A')}` : text),
                    ),
                ),
            );
        }).pipe(Effect.provide(FetchHttpClient.layer)),
    );

// --- [EXPORTS] -------------------------------------------------------------------------

export { createNodes, preTasksExecution };

import { basename, join, matchesGlob, relative } from 'node:path';
import { platform } from 'node:process';
import { fileURLToPath } from 'node:url';
import { AggregateCreateNodesError, type CreateNodes, type CreateNodesContext, detectPackageManager, getPackageManagerCommand, type ProjectConfiguration, readJsonFile, type TargetConfiguration } from '@nx/devkit';
import { combineGlobPatterns, mergeTargetConfigurations, type PackageJson, quoteShellArg } from '@nx/devkit/internal';
import { Array, Cause, Effect, FileSystem, Function, Option, Predicate, Record, Result, Schema, String, Struct, UndefinedOr } from 'effect';
import { run, spawned } from '#/executor';
import { type Fields, HOSTS, type Host } from '#/host-packages';

// --- [TYPES] ---------------------------------------------------------------------------

type Verbs = Readonly<Record<string, Readonly<Record<string, TargetConfiguration>>>>;

interface Declared {
    readonly file: string;
    readonly root: string;
    readonly fields: Fields;
    readonly lock?: string;
    readonly engines: readonly string[];
    readonly stated: Readonly<Record<string, Pick<TargetConfiguration, 'inputs'>>>;
    readonly host: Host;
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const PLUGIN = readJsonFile<PackageJson>(fileURLToPath(import.meta.resolve('#package'))).name;
const FILES = combineGlobPatterns(Array.dedupe(HOSTS.map(({ manifest }) => `**/${basename(manifest)}`)));
const WITHHELD = ['archive', 'build', 'operations', 'publish'] as const;

// --- [MODELS] --------------------------------------------------------------------------

const Options = Schema.Struct({
    outputPath: Schema.optionalKey(Schema.String),
    releaseTag: Schema.optionalKey(Schema.String),
    platformTagPrefix: Schema.optionalKey(Schema.String),
    storeCredentials: Schema.optionalKey(Schema.Record(Schema.String, Schema.String)),
    unrealPackages: Schema.optionalKey(Schema.Struct({ variable: Schema.String, argument: Schema.String })),
}).annotate({
    description:
        'outputPath: workspace folder each host package is written to, {projectName} naming the project, absent writes where the host tool writes in the project. releaseTag: tag of every project nx release versions. platformTagPrefix: prefix of the tag naming each platform a host runs on. storeCredentials: environment variable holding the store credential per technology, a host with none from here or its own tool publishing nothing. unrealPackages: variable is the environment variable the install target of each .uplugin nx.packages row reads the engine root from and argument the argument it takes its install folder under, absent installing none',
});
const Input = Schema.declare((input: unknown): input is NonNullable<TargetConfiguration['inputs']>[number] => Predicate.isString(input) || Predicate.isObject(input));
const Overrides = Schema.fromJsonString(Schema.Struct({ nx: Schema.optionalKey(Schema.Struct({ targets: Schema.optionalKey(Schema.Record(Schema.String, Schema.Struct({ inputs: Schema.optionalKey(Schema.mutable(Schema.Array(Input))) }))) })) })).annotate({
    description: "The nx.targets rows of a project's package.json, whose inputs Nx merges over the inferred targets after every plugin",
});

// --- [OPERATIONS] ----------------------------------------------------------------------

const manifested = Effect.fnUntraced(function* (workspaceRoot: string, root: string) {
    const fs = yield* FileSystem.FileSystem;
    const manifest = join(workspaceRoot, root, 'package.json');
    return (yield* fs.exists(manifest)) ? ((yield* Effect.flatMap(fs.readFileString(manifest), Schema.decodeEffect(Overrides))).nx?.targets ?? {}) : {};
});

const rendered = (cause: Cause.Cause<unknown>) => Effect.fail(new Cause.UnknownError(cause, Cause.pretty(cause)));

const aggregated = (verbs: Verbs, stated: Readonly<Record<string, Pick<TargetConfiguration, 'inputs'>>>, technology: string): Record<string, TargetConfiguration> =>
    Record.fromEntries(
        Record.collect(Record.filter(verbs, Predicate.not(Record.isEmptyRecord)), (verb, tools): readonly (readonly [string, TargetConfiguration])[] => [
            [verb, { dependsOn: [`${verb}--*`], inputs: ['...', ...Array.dedupe(Record.collect(tools, (tool, target) => mergeTargetConfigurations(stated[`${verb}--${tool}`] ?? {}, { ...target, inputs: target.inputs ?? ['default', '^default'] }).inputs ?? []).flat())] }],
            ...Record.collect(tools, (tool, target): readonly [string, TargetConfiguration] => [`${verb}--${tool}`, { ...target, metadata: { technologies: [technology], ...target.metadata } }]),
        ]).flat(),
    );

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
        const stated = yield* manifested(workspaceRoot, root);
        const withheld: readonly ((typeof WITHHELD)[number] | 'install')[] = [...(engined ? [] : WITHHELD), ...(installing.includes(host) ? [] : (['install'] as const))];
        return Option.map(fields, (held): Declared => ({ file, root, fields: held, host: Struct.omit(host, withheld), engines: Option.toArray(stamp), stated, ...Option.getOrUndefined(lock) }));
    });
    return yield* Effect.partition(matched, (row) => Effect.mapError(Effect.catchCause(one(row), rendered), (error): [string, Error] => [row.file, error]), { concurrency: 'unbounded' });
});

const projects = (declared: readonly Declared[], settings: typeof Options.Type, exec: string) => {
    const holds = (other: Declared, item: Declared) => (other.fields.folders ?? []).some((folder) => !relative(join(other.root, folder), item.root).startsWith('..')) && !(other.fields.references ?? []).some((reference) => reference.name === item.fields.name && !reference.enabled);
    const hosts = Function.memoize((item: Declared) => (item.fields.folders === undefined ? declared.filter((other) => holds(other, item)) : [item]));
    const uses = (item: Declared) => declared.filter((other) => other !== item && (item.fields.references ?? []).some((reference) => reference.enabled && reference.name === other.fields.name));
    const users = (item: Declared) => declared.filter((other) => !hosts(item).includes(other) && uses(other).includes(item));
    const ran = `${exec} nx run --help`;
    return declared.map((item) => {
        const loading = Array.dedupe([...hosts(item), ...users(item)]);
        const lone = item.fields.folders === undefined && item.host.standalone === true && !loading.some((other) => other !== item && other.host.build !== undefined);
        const offered: Host = lone ? Struct.omit(item.host, WITHHELD) : item.host;
        const { technology, archive, platforms = [], standalone = false, build, operations = {}, install, publish, write, manifest, output } = offered;
        const packages = Option.map(Option.all({ folders: Option.fromUndefinedOr(item.fields.packages), installs: Option.fromUndefinedOr(settings.unrealPackages) }), ({ folders, installs }) => ({ folders: { ...folders }, ...installs }));
        const linked = Array.dedupe([...[...declared.filter((other) => other !== item && hosts(other).includes(item)), ...uses(item)].map((other) => other.fields.name), ...Option.toArray(packages).flatMap(({ folders }) => Object.values(folders).map(({ project }) => project))]);
        const outputPath = settings.outputPath ?? join('{projectRoot}', output);
        const options = standalone ? { outputPath, descriptor: item.file, hosts: loading.map((other) => other.file), dependencies: uses(item).map((other) => other.file), ...Option.getOrUndefined(Option.map(packages, (installs) => ({ packages: installs }))) } : { outputPath };
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
                    ({ command, options: added }): TargetConfiguration => ({
                        executor: `${PLUGIN}:${executor}`,
                        parallelism: false,
                        ...extra,
                        options: { ...options, ...added, ...extra.options },
                        metadata: { description: `${String.capitalize(verb)}s the ${technology} project its ${manifest} declares with ${command}`, help: { command: ran, example: { options: added } } },
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
                onSome: ({ command, help = ran, unpacked = false }) =>
                    Option.some<TargetConfiguration>({ executor: `${PLUGIN}:install`, dependsOn: [unpacked ? `build--!(${technology})` : 'build'], metadata: { description: `Installs what build wrote in the run's configuration where its host loads it with ${command}`, help: { command: help, example: { args: [] } } } }),
                onNone: () =>
                    Option.orElse(
                        Option.flatMap(Option.liftPredicate(users(item), Array.isReadonlyArrayNonEmpty), (loaded) => operated('install', { dependsOn: [builder, ...held], options: { users: loaded.map((other) => other.file) } })),
                        () => Option.map(Array.head(held), (host): TargetConfiguration => ({ executor: 'nx:noop', dependsOn: [host], metadata: { description: `Builds ${host.projects.join(', ')}, the host the result loads in, with its build target`, help: { command: ran, example: { options: {} } } } })),
                    ),
            },
        );
        const own = Record.map(
            Record.getSomes({
                build: Option.map(
                    Option.fromUndefinedOr(build),
                    ({ executor, command, help = ran, pack }): TargetConfiguration => ({
                        executor: `${PLUGIN}:${executor}`,
                        dependsOn: [...Array.fromNullishOr(pack).map(() => 'build--*'), ...required],
                        options,
                        parallelism: !standalone,
                        cache: written.length > 0,
                        inputs,
                        outputs: written,
                        metadata: { description: `Builds the ${technology} project its ${manifest} declares with ${command}`, help: { command: help, example: { options: { outputPath } } } },
                    }),
                ),
                check: Option.orElse(operated('check', { cache: true, inputs, dependsOn: required }), () =>
                    Option.filter(
                        Option.map(Option.fromUndefinedOr(build), (): TargetConfiguration => ({ executor: 'nx:noop', dependsOn: [builder], inputs, metadata: { description: `Checks the ${technology} project with its ${builder} compile`, help: { command: ran, example: { options: {} } } } })),
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
            (target): Record<string, TargetConfiguration> => ({ [technology]: target }),
        );
        const removed = Record.collect(own, (verb, tools) => Record.keys(Record.filter(tools, ({ outputs = [] }) => outputs.length > 0)).map((tool) => `${verb}--${tool}`)).flat();
        const verbs: Verbs = {
            ...own,
            clean: Array.match(removed, {
                onEmpty: () => ({}),
                onNonEmpty: (labels) => ({ [technology]: { executor: `${PLUGIN}:clean`, options: { targets: labels }, metadata: { description: `Removes the outputs ${labels.join(', ')} declare`, help: { command: ran, example: { options: { targets: labels } } } } } }),
            }),
            attach: {
                ...(archive !== undefined && {
                    gh: {
                        executor: `${PLUGIN}:attach`,
                        dependsOn: [packer],
                        inputs: [{ dependentTasksOutputFiles: '**/*' }],
                        metadata: { description: `Uploads the files ${packer} wrote to the GitHub release of the version the project's manifest states with gh release upload`, help: { command: 'gh release upload --help', example: { options: { dryRun: true } } } },
                    },
                }),
            },
        };
        const released = Option.filter(Option.fromUndefinedOr(write), () => item.fields.version !== undefined);
        const stored = Option.flatMap(Option.fromUndefinedOr(publish), ({ store, command, credential }) =>
            Option.map(
                Option.orElse(Record.get(settings.storeCredentials ?? {}, technology), () => Option.fromUndefinedOr(credential)),
                (variable) => ({ store, command, variable }),
            ),
        );
        const configuration: ProjectConfiguration = {
            root: item.root,
            name: item.fields.name,
            metadata: { technologies: [technology] },
            tags: [...Option.toArray(Option.flatMap(released, () => Option.fromUndefinedOr(settings.releaseTag))), ...Option.toArray(Option.fromUndefinedOr(settings.platformTagPrefix)).flatMap((prefix) => platforms.map((os) => `${prefix}${os}`))],
            ...Array.match(standalone ? linked : [], { onEmpty: () => ({}), onNonEmpty: (implicitDependencies) => ({ implicitDependencies }) }),
            ...Option.getOrUndefined(Option.map(released, () => ({ release: { version: { versionActions: `${PLUGIN}/release/version-actions`, versionActionsOptions: { manifest: relative(item.root, item.file) } } } }))),
            targets: {
                ...aggregated(verbs, item.stated, technology),
                ...Record.fromIterableWith(Option.toArray(stored), ({ store, command, variable }): [string, TargetConfiguration] => [
                    'nx-release-publish',
                    {
                        executor: `${PLUGIN}:release-publish`,
                        dependsOn: [packer, '^nx-release-publish'],
                        options: { credential: variable },
                        metadata: { technologies: [technology], description: `Publishes the ${archive} packages of the project to ${store} with ${command}, under the credential ${variable} holds`, help: { command: ran, example: { options: { dryRun: true } } } },
                    },
                ]),
            },
        };
        return [item.file, { projects: { [item.root]: configuration } }] as const;
    });
};

// --- [COMPOSITION] ---------------------------------------------------------------------

const createNodes: CreateNodes<typeof Options.Encoded> = [
    FILES,
    (files, options, context) =>
        run(
            Effect.gen(function* () {
                const settings = yield* Schema.decodeEffect(Options)(options ?? {});
                const [held, failures] = yield* read(files, context);
                const results = projects(
                    Array.getSomes(held).filter(({ fields }) => fields.installed !== true),
                    settings,
                    getPackageManagerCommand(detectPackageManager(context.workspaceRoot)).exec,
                );
                return yield* Array.match(failures, { onEmpty: () => Effect.succeed(results), onNonEmpty: (refused) => Effect.fail(new AggregateCreateNodesError(refused, results)) });
            }),
        ),
];

// --- [EXPORTS] -------------------------------------------------------------------------

export { createNodes };

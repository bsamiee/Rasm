import { dirname, isAbsolute, join, matchesGlob, relative } from 'node:path';
import { fileURLToPath } from 'node:url';
import { layer } from '@effect/platform-node/NodeServices';
import {
    AggregateCreateNodesError,
    type CreateDependencies,
    type CreateDependenciesContext,
    type CreateNodes,
    type CreateNodesContext,
    createNodesFromFiles,
    DependencyType,
    getOutputsForTargetAndConfiguration,
    type ProjectConfiguration,
    type RawProjectGraphDependency,
    readJsonFile,
    type TargetConfiguration,
} from '@nx/devkit';
import { getReadyCheckReadyWhen, hashObject, hashWithWorkspaceContext, mergeTargetConfigurations, type PackageJson, PluginCache, quoteShellArg, workspaceDataDirectory } from '@nx/devkit/internal';
import { Array, Cause, Context, Effect, Equal, FileSystem, Layer, ManagedRuntime, Option, Order, Predicate, Record, Schema } from 'effect';
import { Yaml } from 'effect/encoding';
import { spawned } from '#/ready-check';

// --- [TYPES] ---------------------------------------------------------------------------

type Composed = Effect.Success<ReturnType<typeof analyzed>>;
type Verbs = Readonly<Record<string, Readonly<Record<string, TargetConfiguration>>>>;

// --- [CONSTANTS] -----------------------------------------------------------------------

const PLUGIN = readJsonFile<PackageJson>(fileURLToPath(import.meta.resolve('#package'))).name;
const EVERY_PROFILE = { COMPOSE_PROFILES: '*' };
const PRODUCERS = ['build--!(compose|buildx)', '^build'];
const DOWN = ['down', '--rmi', 'local', '--remove-orphans'] as const;

// --- [MODELS] --------------------------------------------------------------------------

const Options = Schema.Struct({}).annotate({ description: 'Takes no option: every target derives from docker compose config' });
const Input = Schema.declare((input: unknown): input is NonNullable<TargetConfiguration['inputs']>[number] => Predicate.isString(input) || Predicate.isObject(input));
const Overrides = Schema.fromJsonString(Schema.Struct({ nx: Schema.optionalKey(Schema.Struct({ targets: Schema.optionalKey(Schema.Record(Schema.String, Schema.Struct({ inputs: Schema.optionalKey(Schema.mutable(Schema.Array(Input))) }))) })) })).annotate({
    description: "The nx.targets rows of a project's package.json, whose inputs Nx merges over the inferred targets after every plugin",
});
const Report = Schema.fromJsonString(
    Schema.Struct({
        name: Schema.String,
        services: Schema.Record(
            Schema.String,
            Schema.Struct({
                profiles: Schema.optionalKey(Schema.Array(Schema.String)),
                env_file: Schema.optionalKey(Schema.Array(Schema.Struct({ path: Schema.String }))),
                build: Schema.optionalKey(Schema.Struct({ context: Schema.String, dockerfile: Schema.optionalKey(Schema.String), additional_contexts: Schema.optionalKey(Schema.Record(Schema.String, Schema.String)) })),
            }),
        ),
    }),
);
const Including = Schema.Struct({ include: Schema.Array(Schema.Union([Schema.String, Schema.Struct({ path: Schema.Union([Schema.String, Schema.Array(Schema.String)]) })])).pipe(Schema.withDecodingDefaultKey(Effect.succeed([]))) }).annotate({
    description: 'The include element of a Compose file, each row a path or a path list relative to the file, which docker compose config merges without naming',
});

// --- [SERVICES] ------------------------------------------------------------------------

class Analyses extends Context.Service<Analyses, Map<string, Composed>>()(`${PLUGIN}/plugin/Analyses`) {
    static readonly layer = Layer.sync(Analyses, () => new Map());
}

// --- [OPERATIONS] ----------------------------------------------------------------------

const rendered = (cause: Cause.Cause<unknown>) => Effect.fail(new Cause.UnknownError(cause, Cause.pretty(cause)));

const manifested = Effect.fnUntraced(function* (workspaceRoot: string, root: string) {
    const fs = yield* FileSystem.FileSystem;
    const manifest = join(workspaceRoot, root, 'package.json');
    return (yield* fs.exists(manifest)) ? ((yield* Effect.flatMap(fs.readFileString(manifest), Schema.decodeEffect(Overrides))).nx?.targets ?? {}) : {};
});

const aggregated = (verbs: Verbs, stated: Readonly<Record<string, Pick<TargetConfiguration, 'inputs'>>>, technology: string): Record<string, TargetConfiguration> =>
    Record.fromEntries(
        Record.collect(Record.filter(verbs, Predicate.not(Record.isEmptyRecord)), (verb, tools): readonly (readonly [string, TargetConfiguration])[] => [
            [verb, { dependsOn: [`${verb}--*`], inputs: ['...', ...Array.dedupe(Record.collect(tools, (tool, target) => mergeTargetConfigurations(stated[`${verb}--${tool}`] ?? {}, { ...target, inputs: target.inputs ?? ['default', '^default'] }).inputs ?? []).flat())] }],
            ...Record.collect(tools, (tool, target): readonly [string, TargetConfiguration] => [`${verb}--${tool}`, { ...target, metadata: { technologies: [technology], ...target.metadata } }]),
        ]).flat(),
    );

const merged = Effect.fnUntraced(function* (workspaceRoot: string, file: string, seen: ReadonlySet<string>): Effect.fn.Return<ReadonlySet<string>, never, FileSystem.FileSystem> {
    const fs = yield* FileSystem.FileSystem;
    const stated = Option.flatMap(yield* Effect.option(fs.readFileString(join(workspaceRoot, file))), (text) => Option.flatMap(Option.liftThrowable(Yaml.parse)(text), Schema.decodeUnknownOption(Including)));
    const rows = Option.match(stated, { onNone: () => [], onSome: ({ include }) => include.flatMap((row) => Array.ensure(Predicate.isString(row) ? row : row.path)) });
    return yield* Effect.reduce(
        rows.map((to) => join(dirname(file), to)),
        (): ReadonlySet<string> => new Set([...seen, file]),
        (held, to) => (held.has(to) ? Effect.succeed(held) : merged(workspaceRoot, to, held)),
    );
});

const analyzed = Effect.fnUntraced(function* (workspaceRoot: string, root: string, files: Array.NonEmptyReadonlyArray<string>) {
    const report = yield* Effect.flatMap(spawned(['docker', 'compose', 'config', '--format', 'json', '--no-interpolate'], { cwd: join(workspaceRoot, root), env: EVERY_PROFILE }), Schema.decodeEffect(Report));
    const sources = yield* Effect.reduce(
        files,
        (): ReadonlySet<string> => new Set(),
        (held, file) => merged(workspaceRoot, file, held),
    );
    const local = (at: string) => Option.liftPredicate(relative(workspaceRoot, at), (to) => isAbsolute(at) && !(to.startsWith('..') || isAbsolute(to)));
    const services = Object.values(report.services);
    const builds = Array.getSomes(services.map(({ build }) => Option.fromUndefinedOr(build)));
    const recipes = Array.getSomes(
        Record.collect(report.services, (service, { build }) =>
            Option.gen(function* () {
                const { context, dockerfile } = yield* Option.fromUndefinedOr(build);
                const file = yield* Option.fromUndefinedOr(dockerfile);
                return { service, recipe: yield* local(isAbsolute(file) ? file : join(context, file)) };
            }),
        ),
    );
    return {
        name: report.name,
        file: Array.min(files, Order.String),
        read: Array.sort(Array.dedupe([...[...sources].filter((file) => !file.startsWith('..')), join(root, '.env'), ...services.flatMap(({ env_file = [] }) => Array.getSomes(env_file.map((file) => local(file.path)))), ...recipes.map(({ recipe }) => recipe)]), Order.String),
        profiles: Array.sort(Array.dedupe(services.flatMap(({ profiles = [] }) => profiles)), Order.String),
        built: builds.length > 0,
        servers: Record.map(
            Array.groupBy(recipes, ({ recipe }) => dirname(recipe)),
            Array.map(({ service }) => service),
        ),
        contexts: Array.dedupe(builds.flatMap(({ context, additional_contexts = {} }) => Array.getSomes([context, ...Object.values(additional_contexts)].map(local)))),
    };
});

const compose = (root: string, args: Array.NonEmptyReadonlyArray<string>, description: string, extra: Partial<TargetConfiguration> = {}): TargetConfiguration => ({
    ...extra,
    command: ['docker', 'compose', ...args].map(quoteShellArg).join(' '),
    options: { cwd: root, ...extra.options },
    metadata: { description, help: { command: `docker compose ${args[0]} --help`, example: { args: ['--dry-run'] } } },
});

const served = (root: string, project: Composed, services: Array.NonEmptyReadonlyArray<string>): Verbs => {
    const up = compose(root, ['up', '--wait', '--no-build', ...services], `Starts ${services.join(', ')} of ${project.name} with the services each depends on from built images and follows their logs with docker compose up, the services staying up after the run`, {
        continuous: true,
        dependsOn: ['build--buildx'],
        readyWhen: getReadyCheckReadyWhen(`${PLUGIN}/ready-check`, undefined, ...services),
        inputs: ['default', '^default', ...project.read.map((file) => `{workspaceRoot}/${file}`), { externalDependencies: [] }],
    });
    return {
        build: { buildx: compose(root, ['build', '--with-dependencies', ...services], `Builds the images of ${services.join(', ')} of ${project.name} and of the services each depends on into the engine with docker compose build`, { dependsOn: PRODUCERS }) },
        serve: { compose: { ...up, command: `${up.command} && ${['docker', 'compose', 'logs', '--follow', ...services].map(quoteShellArg).join(' ')}` } },
        clean: { compose: compose(root, [...DOWN, ...services], `Stops ${services.join(', ')} of ${project.name} and removes their containers and the images Compose built for them with docker compose down`) },
    };
};

const targets = (root: string, project: Composed): Verbs => {
    const config = ['docker', 'compose', '--project-directory', root, '--profile', '*', 'config', '--format', 'json', '--no-interpolate', '--no-env-resolution', '--no-path-resolution'];
    const checked = { cache: true, inputs: [...project.read.map((file) => `{workspaceRoot}/${file}`), { runtime: config.map(quoteShellArg).join(' ') }, { externalDependencies: [] }], options: { env: EVERY_PROFILE } };
    const configurations = Record.fromIterableWith(project.profiles, (profile) => [profile, { env: { COMPOSE_PROFILES: profile } }] as const);
    const built = Option.liftPredicate(project, ({ built: images }) => images);
    return {
        check: {
            compose: compose(root, ['config', '--quiet'], `Validates every Compose service of ${project.name}, interpolation and env files included, with docker compose config`, checked),
            ...Record.getSomes({ buildx: Option.map(built, () => compose(root, ['build', '--check'], `Lints every image ${project.name} builds, failing on any warning, with docker compose build --check`, { ...checked, dependsOn: PRODUCERS })) }),
        },
        build: Record.getSomes({ compose: Option.map(built, () => compose(root, ['build'], `Builds the images of ${project.name} into the engine with docker compose build`, { dependsOn: PRODUCERS, configurations })) }),
        install: { compose: compose(root, ['up', '--wait', '--no-build', '--remove-orphans'], `Starts the services of ${project.name} from built images and waits until each is healthy with docker compose up`, { dependsOn: ['build--*'], configurations }) },
        clean: { compose: compose(root, DOWN, `Stops the services of ${project.name} and removes their containers, networks, and the images Compose built with docker compose down`, { options: { env: EVERY_PROFILE } }) },
    };
};

const inferred = Effect.fnUntraced(function* (cache: PluginCache<{ readonly project: Composed; readonly stamp: string }>, context: CreateNodesContext, root: string, matched: Array.NonEmptyReadonlyArray<string>) {
    const stamped = ({ read }: Composed) => Effect.promise(() => hashWithWorkspaceContext(context.workspaceRoot, [...read]));
    const key = `${yield* Effect.promise(() => hashWithWorkspaceContext(context.workspaceRoot, [...matched]))}${root}`;
    const held = yield* Effect.findFirst(Option.toArray(Option.fromUndefinedOr(cache.get(key))), (stored) => Effect.map(stamped(stored.project), Equal.equals(stored.stamp)));
    const project = yield* Option.match(held, { onSome: (stored) => Effect.succeed(stored.project), onNone: () => analyzed(context.workspaceRoot, root, matched) });
    cache.set(key, { project, stamp: yield* stamped(project) });
    (yield* Analyses).set(join(context.workspaceRoot, root), project);
    const serving = Record.map(project.servers, (services) => served(root, project, services));
    const projects = yield* Effect.forEach(Record.toEntries({ ...serving, [root]: Record.union(serving[root] ?? {}, targets(root, project), (base, stated) => ({ ...base, ...stated })) }), ([at, tools]) =>
        Effect.map(manifested(context.workspaceRoot, at), (stated): readonly [string, ProjectConfiguration] => [
            at,
            {
                root: at,
                ...(at === root && { name: project.name }),
                metadata: { technologies: ['compose'] },
                targets: aggregated(tools, stated, 'compose'),
            },
        ]),
    );
    return { projects: Record.fromEntries(projects) };
});

const edges = (analyses: ReadonlyMap<string, Composed>, { projects, workspaceRoot }: CreateDependenciesContext) => {
    const selections = Record.collect(projects, (name, data) =>
        Record.collect(
            Record.filter(data.targets ?? {}, (stated) => stated.outputs !== undefined),
            (target, { configurations = {} }) => [{}, ...Record.keys(configurations).map((configuration) => ({ configuration }))].map((selected) => ({ name, data, target, selected })),
        ),
    ).flat(2);
    const outputs = selections.flatMap(({ name, data, target, selected }) => getOutputsForTargetAndConfiguration({ project: name, target, ...selected }, {}, { name, type: 'lib', data }).map((output) => ({ name, output })));
    const built = Record.collect(projects, (name, { root }) => Array.fromNullishOr(analyses.get(join(workspaceRoot, root))).flatMap(({ file, contexts }) => contexts.map((at) => ({ name, file, at })))).flat();
    return built.flatMap(({ name, file, at }) => outputs.filter(({ output }) => !output.startsWith('!') && (matchesGlob(at, output) || matchesGlob(at, `${output}/**`))).map((made): RawProjectGraphDependency => ({ source: name, target: made.name, type: DependencyType.static, sourceFile: file })));
};

// --- [COMPOSITION] ---------------------------------------------------------------------

const runtime = ManagedRuntime.make(Layer.merge(Analyses.layer, layer));

const createNodes: CreateNodes<typeof Options.Encoded> = [
    '**/{compose,docker-compose}{,.override}.{yaml,yml}',
    (files, options, context) => {
        const folders = new Map(Record.collect(Array.groupBy(files, dirname), (root, matched) => [Array.min(matched, Order.String), { root, matched }] as const));
        const cache = new PluginCache<{ readonly project: Composed; readonly stamp: string }>(join(workspaceDataDirectory, `docker-${hashObject(options ?? {})}.hash`));
        const read = (file: string) =>
            runtime.runPromise(
                Effect.catchCause(
                    Effect.flatMap(Effect.fromNullishOr(folders.get(file)), ({ root, matched }) => inferred(cache, context, root, matched)),
                    rendered,
                ),
            );
        return runtime.runPromise(
            Effect.ensuring(
                Effect.andThen(
                    Schema.decodeEffect(Options)(options ?? {}),
                    Effect.tryPromise({
                        try: () => createNodesFromFiles(read, [...folders.keys()], options, context),
                        catch: (cause) => (cause instanceof AggregateCreateNodesError ? cause : new AggregateCreateNodesError([[null, new Cause.UnknownError(cause, `${PLUGIN} infers no project`)]], [])),
                    }),
                ),
                Effect.sync(() => cache.writeToDisk()),
            ),
        );
    },
];

const createDependencies: CreateDependencies = (_, context) => runtime.runPromise(Effect.map(Analyses, (analyses) => edges(analyses, context)));

// --- [EXPORTS] -------------------------------------------------------------------------

export { createDependencies, createNodes };

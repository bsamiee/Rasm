import { isAbsolute, join, posix, relative } from 'node:path';
import { fileURLToPath } from 'node:url';
import {
    type CreateDependencies,
    type CreateDependenciesContext,
    type CreateNodes,
    type CreateNodesContext,
    createNodesFromFiles,
    DependencyType,
    getOutputsForTargetAndConfiguration,
    hashArray,
    normalizePath,
    type ProjectConfiguration,
    type RawProjectGraphDependency,
    readJsonFile,
    type TargetConfiguration,
} from '@nx/devkit';
import { getReadyCheckReadyWhen, hashWithWorkspaceContext, type PackageJson, PluginCache, quoteShellArg, workspaceDataDirectory } from '@nx/devkit/internal';
import { aggregated, anchored, fingerprinted, manifested, runner, spawned, type Verbs } from '@rasm/nx-distribution/inference';
import { Array, Context, Effect, Equal, FileSystem, Function, Layer, MutableRef, Option, Order, Predicate, Record, Result, Schema } from 'effect';
import { Yaml } from 'effect/encoding';

// --- [TYPES] ---------------------------------------------------------------------------

type Composed = Effect.Success<ReturnType<typeof analyzed>>;

// --- [CONSTANTS] -----------------------------------------------------------------------

const PLUGIN = readJsonFile<PackageJson>(fileURLToPath(import.meta.resolve('#package'))).name;
const OWN = fingerprinted(import.meta.dirname);
const EVERY_PROFILE = { COMPOSE_PROFILES: '*' };
const PRODUCERS = ['build--!(compose|buildx)', '^build--*'];

// --- [MODELS] --------------------------------------------------------------------------

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
const Including = Schema.Struct({
    include: Schema.Array(Schema.Union([Schema.String, Schema.Struct({ path: Schema.Union([Schema.String, Schema.Array(Schema.String)]) })])).pipe(Schema.withDecodingDefaultKey(Effect.succeed([]))),
    services: Schema.optionalKey(Schema.NullOr(Schema.Record(Schema.String, Schema.Struct({ extends: Schema.optionalKey(Schema.Union([Schema.String, Schema.Struct({ file: Schema.optionalKey(Schema.String) })])) })))),
}).annotate({
    description: 'The include element and service extends.file rows of a Compose file, each a path or a path list relative to the file, which docker compose config merges without naming',
});

// --- [SERVICES] ------------------------------------------------------------------------

class Analyses extends Context.Service<Analyses, Map<string, Composed>>()(`${PLUGIN}/plugin/Analyses`) {
    static readonly layer = Layer.sync(Analyses, () => new Map());
}

// --- [OPERATIONS] ----------------------------------------------------------------------

const merged = Effect.fnUntraced(function* (workspaceRoot: string, file: string, seen: ReadonlySet<string>): Effect.fn.Return<ReadonlySet<string>, never, FileSystem.FileSystem> {
    const fs = yield* FileSystem.FileSystem;
    const stated = Option.flatMap(yield* Effect.option(fs.readFileString(join(workspaceRoot, file))), (text) => Option.flatMap(Option.liftThrowable(Yaml.parse)(text), Schema.decodeUnknownOption(Including)));
    const rows = Option.match(stated, {
        onNone: () => [],
        onSome: ({ include, services }) => [...include.flatMap((row) => Array.ensure(Predicate.isString(row) ? row : row.path)), ...Object.values(services ?? {}).flatMap((service) => (Predicate.isObject(service.extends) ? Array.fromNullishOr(service.extends.file) : []))],
    });
    return yield* Effect.reduce(
        rows.map((to) => posix.join(posix.dirname(file), to)),
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
    const local = (at: string) =>
        Option.map(
            Option.liftPredicate(relative(workspaceRoot, at), (to) => isAbsolute(at) && !(to.startsWith('..') || isAbsolute(to))),
            normalizePath,
        );
    const services = Object.values(report.services);
    const builds = Array.getSomes(services.map(({ build }) => Option.fromUndefinedOr(build)));
    const dockerfiles = Record.collect(report.services, (service, { build }) => (build?.dockerfile === undefined ? [] : [{ service, file: isAbsolute(build.dockerfile) ? build.dockerfile : join(build.context, build.dockerfile) }])).flat();
    const recipes = Array.filterMap(dockerfiles, ({ service, file }) => Result.map(Result.fromOption(local(file), Function.constVoid), (recipe) => ({ service, recipe })));
    return {
        name: report.name,
        file: Array.min(files, Order.String),
        read: Array.sort(Array.dedupe([...[...sources].filter((file) => !file.startsWith('..')), posix.join(root, '.env'), ...services.flatMap(({ env_file = [] }) => Array.getSomes(env_file.map((file) => local(file.path)))), ...recipes.map(({ recipe }) => recipe)]), Order.String),
        profiles: Array.sort(Array.dedupe(services.flatMap(({ profiles = [] }) => profiles)), Order.String),
        built: builds.length > 0,
        servers: Record.map(
            Array.groupBy(recipes, ({ recipe }) => posix.dirname(recipe)),
            Array.map(({ service }) => service),
        ),
        contexts: Array.dedupe(builds.flatMap(({ context, additional_contexts = {} }) => Array.getSomes([context, ...Object.values(additional_contexts)].map(local)))),
    };
});

const compose = (root: string, args: Array.NonEmptyReadonlyArray<string>, description: string, extra: Partial<TargetConfiguration<Readonly<Record<string, unknown>>>> = {}): TargetConfiguration => ({
    ...extra,
    command: ['docker', 'compose', ...args].map(quoteShellArg).join(' '),
    options: { cwd: root, ...extra.options },
    metadata: { description, help: { command: `docker compose ${args[0]} --help`, example: { args: ['--dry-run'] } } },
});

const consumed = ({ read }: Composed): NonNullable<TargetConfiguration['inputs']> => ['default', '^default', ...read.map((file) => `{workspaceRoot}/${file}`), { externalDependencies: [] }];

const served = (root: string, project: Composed, services: Array.NonEmptyReadonlyArray<string>): Verbs => {
    const inputs = consumed(project);
    const up = compose(root, ['up', '--detach', '--wait', '--no-build', ...services], `Starts ${services.join(', ')} of ${project.name} with the services each depends on from built images with docker compose up and follows their logs with docker compose logs, the services staying up after the run`, {
        continuous: true,
        dependsOn: ['build--*'],
        readyWhen: getReadyCheckReadyWhen(`${PLUGIN}/ready-check`, undefined, ...services),
        inputs,
    });
    return {
        build: { buildx: compose(root, ['build', '--with-dependencies', ...services], `Builds the images of ${services.join(', ')} of ${project.name} and of the services each depends on into the engine with docker compose build`, { dependsOn: PRODUCERS, inputs }) },
        serve: { compose: { ...up, command: `${up.command} && ${['docker', 'compose', 'logs', '--follow', ...services].map(quoteShellArg).join(' ')}` } },
    };
};

const targets = (root: string, project: Composed): Verbs => {
    const checked = { cache: true, inputs: [...project.read.map((file) => `{workspaceRoot}/${file}`), { externalDependencies: [] }], options: { env: EVERY_PROFILE } };
    const inputs = consumed(project);
    const profiled = Array.isReadonlyArrayNonEmpty(project.profiles) && { configurations: Record.fromIterableWith(project.profiles, (profile) => [profile, { env: { COMPOSE_PROFILES: profile } }] as const) };
    const built = Option.liftPredicate(project, ({ built: images }) => images);
    return {
        check: {
            compose: compose(root, ['config', '--quiet'], `Validates every Compose service of ${project.name}, interpolation and env files included, with docker compose config`, checked),
            ...Record.getSomes({ buildx: Option.map(built, () => compose(root, ['build', '--check'], `Lints every image ${project.name} builds, failing on any warning, with docker compose build --check`, { ...checked, dependsOn: PRODUCERS })) }),
        },
        build: Record.getSomes({ compose: Option.map(built, () => compose(root, ['build'], `Builds the images of ${project.name} into the engine with docker compose build`, { dependsOn: PRODUCERS, inputs, ...profiled })) }),
        install: { compose: compose(root, ['up', '--detach', '--wait', '--no-build', '--remove-orphans'], `Starts the services of ${project.name} from built images and waits until each is healthy with docker compose up`, { dependsOn: ['build--*'], inputs, ...profiled }) },
    };
};

const inferred = Effect.fnUntraced(function* (cache: PluginCache<{ readonly project: Composed; readonly stamp: string; readonly digest: string }>, stored: MutableRef.MutableRef<boolean>, context: CreateNodesContext, rooted: (folder: string) => boolean, root: string, matched: Array.NonEmptyReadonlyArray<string>) {
    const stamped = ({ read }: Composed) => Effect.promise(() => hashWithWorkspaceContext(context.workspaceRoot, [...read]));
    const digest = hashArray([yield* Effect.promise(() => hashWithWorkspaceContext(context.workspaceRoot, [...matched])), yield* OWN]);
    const held = yield* Effect.findFirst(Option.toArray(Option.filter(Option.fromUndefinedOr(cache.get(root)), (entry) => entry.digest === digest)), (entry) => Effect.map(stamped(entry.project), Equal.equals(entry.stamp)));
    const missed = Effect.fnUntraced(function* () {
        const made = yield* analyzed(context.workspaceRoot, root, matched);
        cache.set(root, { project: made, stamp: yield* stamped(made), digest });
        MutableRef.set(stored, true);
        return made;
    });
    const project = yield* Option.match(held, { onSome: (entry) => Effect.succeed(entry.project), onNone: missed });
    (yield* Analyses).set(join(context.workspaceRoot, root), project);
    const serving = Record.map(
        Record.filter(project.servers, (_, at) => rooted(at)),
        (services) => served(root, project, services),
    );
    const projects = yield* Effect.forEach(Record.toEntries({ ...serving, [root]: Record.union(serving[root] ?? {}, targets(root, project), (_, stated) => stated) }), ([at, tools]) =>
        Effect.map(manifested(context.workspaceRoot, at), (stated): readonly [string, ProjectConfiguration] => [
            at,
            {
                root: at,
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
        ).flat(),
    ).flat();
    const outputs = selections.flatMap(({ name, data, target, selected }) => getOutputsForTargetAndConfiguration({ project: name, target, ...selected }, {}, { name, type: 'lib', data }).map((output) => ({ name, output })));
    const built = Record.collect(projects, (name, { root }) => Array.fromNullishOr(analyses.get(join(workspaceRoot, root))).flatMap(({ file, contexts }) => contexts.map((at) => ({ name, file, at })))).flat();
    return built.flatMap(({ name, file, at }) => outputs.filter(({ output }) => !output.startsWith('!') && (posix.matchesGlob(at, output) || posix.matchesGlob(at, `${output}/**`))).map((made): RawProjectGraphDependency => ({ source: name, target: made.name, type: DependencyType.static, sourceFile: file })));
};

// --- [COMPOSITION] ---------------------------------------------------------------------

const run = runner(Analyses.layer);

const createNodes: CreateNodes = [
    '**/{compose,docker-compose}{,.override}.{yaml,yml}',
    (files, options, context) => {
        const rooted = anchored(context.workspaceRoot);
        const folders = new Map(
            Record.collect(
                Record.filter(Array.groupBy(files, posix.dirname), (_, root) => rooted(root)),
                (root, matched) => [Array.min(matched, Order.String), { root, matched }] as const,
            ),
        );
        const cache = new PluginCache<{ readonly project: Composed; readonly stamp: string; readonly digest: string }>(join(workspaceDataDirectory, 'docker.hash'));
        const stored = MutableRef.make(false);
        const read = (file: string) => run(Effect.flatMap(Effect.fromNullishOr(folders.get(file)), ({ root, matched }) => inferred(cache, stored, context, rooted, root, matched)));
        return run(
            Effect.ensuring(
                Effect.promise(() => createNodesFromFiles(read, folders.keys().toArray(), options, context)),
                Effect.when(
                    Effect.sync(() => cache.writeToDisk()),
                    Effect.sync(() => MutableRef.get(stored)),
                ),
            ),
        );
    },
];

const createDependencies: CreateDependencies = (_, context) => run(Effect.map(Analyses, (analyses) => edges(analyses, context)));

// --- [EXPORTS] -------------------------------------------------------------------------

export { createDependencies, createNodes };

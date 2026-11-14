import { execFile } from 'node:child_process';
import { dirname, isAbsolute, join, matchesGlob, relative } from 'node:path';
import { env } from 'node:process';
import { promisify } from 'node:util';
import { layer } from '@effect/platform-node/NodeServices';
import { type CreateDependencies, type CreateDependenciesContext, type CreateNodes, type CreateNodesContext, createNodesFromFiles, DependencyType, getOutputsForTargetAndConfiguration, type ProjectConfiguration, type RawProjectGraphDependency, type TargetConfiguration } from '@nx/devkit';
import { getReadyCheckReadyWhen, hashObject, hashWithWorkspaceContext, PluginCache, quoteShellArg, workspaceDataDirectory } from '@nx/devkit/internal';
import { Array, Cause, Context, Effect, FileSystem, Layer, ManagedRuntime, Option, Order, Predicate, Record, Schema } from 'effect';
import { Yaml } from 'effect/encoding';

// --- [TYPES] ---------------------------------------------------------------------------

type Composed = Effect.Success<ReturnType<typeof analyzed>>;

// --- [CONSTANTS] -----------------------------------------------------------------------

const EVERY_PROFILE = { COMPOSE_PROFILES: '*' };
const PRODUCERS = ['build--!(compose|buildx)', '^build'];
const DOWN = ['down', '--rmi', 'local', '--remove-orphans'] as const;

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
const Including = Schema.Struct({ include: Schema.Array(Schema.Union([Schema.String, Schema.Struct({ path: Schema.Union([Schema.String, Schema.Array(Schema.String)]) })])).pipe(Schema.withDecodingDefaultKey(Effect.succeed([]))) }).annotate({
    description: 'The include element of a Compose file, each row a path or a path list relative to the file, which docker compose config merges without naming',
});

// --- [SERVICES] ------------------------------------------------------------------------

class Analyses extends Context.Service<Analyses, Map<string, Composed>>()('@rasm/nx-docker/plugin/Analyses') {
    static readonly layer = Layer.sync(Analyses, () => new Map());
}

// --- [OPERATIONS] ----------------------------------------------------------------------

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
    const report = yield* Effect.tryPromise((signal) => promisify(execFile)('docker', ['compose', 'config', '--format', 'json', '--no-interpolate'], { encoding: 'utf8', cwd: join(workspaceRoot, root), env: { ...env, ...EVERY_PROFILE }, maxBuffer: Number.POSITIVE_INFINITY, signal })).pipe(
        Effect.flatMap(({ stdout }) => Schema.decodeEffect(Report)(stdout)),
    );
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
    metadata: { technologies: ['compose'], description, help: { command: `docker compose ${args[0]} --help`, example: {} } },
});

const served = (root: string, project: Composed, services: Array.NonEmptyReadonlyArray<string>): Record<string, TargetConfiguration> => {
    const up = compose(root, ['up', '--wait', '--no-build', ...services], `Starts ${services.join(', ')} of ${project.name} with the services each depends on from built images and follows their logs, the services staying up after the run`, {
        continuous: true,
        dependsOn: ['build--buildx'],
        readyWhen: getReadyCheckReadyWhen('@rasm/nx-docker/ready-check', undefined, ...services),
        inputs: ['default', '^default', ...project.read.map((file) => `{workspaceRoot}/${file}`), { externalDependencies: [] }],
    });
    return {
        'build--buildx': compose(root, ['build', '--with-dependencies', ...services], `Builds the images of ${services.join(', ')} of ${project.name} and of the services each depends on into the engine`, { dependsOn: PRODUCERS }),
        'serve--compose': { ...up, command: `${up.command} && ${['docker', 'compose', 'logs', '--follow', ...services].map(quoteShellArg).join(' ')}` },
        'clean--compose': compose(root, [...DOWN, ...services], `Stops ${services.join(', ')} of ${project.name} and removes their containers and the images Compose built for them with docker compose down`),
    };
};

const targets = (root: string, project: Composed): Record<string, TargetConfiguration> => {
    const config = ['docker', 'compose', '--project-directory', root, '--profile', '*', 'config', '--format', 'json', '--no-interpolate', '--no-env-resolution', '--no-path-resolution'];
    const checked = { cache: true, inputs: [...project.read.map((file) => `{workspaceRoot}/${file}`), { runtime: config.map(quoteShellArg).join(' ') }, { externalDependencies: [] }], options: { env: EVERY_PROFILE } };
    const configurations = Record.fromIterableWith(project.profiles, (profile) => [profile, { env: { COMPOSE_PROFILES: profile } }] as const);
    return {
        'check--compose': compose(root, ['config', '--quiet'], `Validates every Compose service of ${project.name}, interpolation and env files included`, checked),
        ...(project.built && {
            'check--buildx': compose(root, ['build', '--check'], `Lints every image ${project.name} builds through BuildKit, failing on any warning`, { ...checked, dependsOn: PRODUCERS }),
            'build--compose': compose(root, ['build'], `Builds the images of ${project.name} into the engine`, { dependsOn: PRODUCERS, configurations }),
        }),
        'install--compose': compose(root, ['up', '--wait', '--no-build', '--remove-orphans'], `Starts the services of ${project.name} from built images and waits until each is healthy`, { dependsOn: ['build--*'], configurations }),
        'clean--compose': compose(root, DOWN, `Stops the services of ${project.name} and removes their containers, networks, and the images Compose built with docker compose down`, { options: { env: EVERY_PROFILE } }),
    };
};

const aggregated = (tools: Readonly<Record<string, TargetConfiguration>>): Record<string, TargetConfiguration> => ({
    ...tools,
    ...Record.map(
        Array.groupBy(Object.entries(tools), ([label]) => label.slice(0, label.indexOf('--'))),
        (members, verb): TargetConfiguration => ({ dependsOn: [`${verb}--*`], ...Array.match(Array.dedupe(members.flatMap(([, { inputs: hashed = [] }]) => hashed)), { onEmpty: () => ({}), onNonEmpty: (unique) => ({ inputs: ['...', ...unique] }) }) }),
    ),
});

const inferred = Effect.fnUntraced(function* (cache: PluginCache<{ readonly project: Composed; readonly digest: string }>, context: CreateNodesContext, root: string, matched: Array.NonEmptyReadonlyArray<string>) {
    const stamped = (read: readonly string[]) => Effect.promise(() => hashWithWorkspaceContext(context.workspaceRoot, [...matched, ...read]));
    const stored = Option.fromUndefinedOr(cache.get(root));
    const current = yield* stamped(Option.match(stored, { onNone: () => [], onSome: (entry) => entry.project.read }));
    const project = yield* Option.match(
        Option.filter(stored, (entry) => entry.digest === current),
        { onSome: (entry) => Effect.succeed(entry.project), onNone: () => Effect.catchCause(analyzed(context.workspaceRoot, root, matched), (cause) => Effect.fail(new Cause.UnknownError(cause, `${Array.min(matched, Order.String)}: does not load as a Compose project: ${Cause.pretty(cause)}`))) },
    );
    cache.set(root, { project, digest: yield* stamped(project.read) });
    (yield* Analyses).set(join(context.workspaceRoot, root), project);
    const serving = Record.map(project.servers, (services) => served(root, project, services));
    const projects = Record.map(
        { ...serving, [root]: { ...serving[root], ...targets(root, project) } },
        (tools, at): ProjectConfiguration => ({
            root: at,
            ...(at === root && { name: project.name }),
            metadata: { technologies: ['compose'] },
            targets: aggregated(tools),
        }),
    );
    return { projects };
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

const createNodes: CreateNodes = [
    '**/{compose,docker-compose}{,.override}.{yaml,yml}',
    (files, options, context) => {
        const folders = new Map(Record.collect(Array.groupBy(files, dirname), (root, matched) => [Array.min(matched, Order.String), { root, matched }] as const));
        const cache = new PluginCache<{ readonly project: Composed; readonly digest: string }>(join(workspaceDataDirectory, `docker-${hashObject(options ?? {})}.hash`));
        const read = (file: string) => runtime.runPromise(Effect.flatMap(Effect.fromNullishOr(folders.get(file)), ({ root, matched }) => inferred(cache, context, root, matched)));
        return runtime.runPromise(
            Effect.ensuring(
                Effect.promise(() => createNodesFromFiles(read, [...folders.keys()], options, context)),
                Effect.sync(() => cache.writeToDisk()),
            ),
        );
    },
];

const createDependencies: CreateDependencies = (_, context) => runtime.runPromise(Effect.map(Analyses, (analyses) => edges(analyses, context)));

// --- [EXPORTS] -------------------------------------------------------------------------

export { createDependencies, createNodes };

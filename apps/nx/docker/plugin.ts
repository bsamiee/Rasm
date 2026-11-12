import { matchesGlob } from 'node:path';
import { type CreateDependenciesContext, DependencyType, type RawProjectGraphDependency, type TargetConfiguration } from '@nx/devkit';
import { getReadyCheckReadyWhen } from '@nx/devkit/internal';
import { inferred, line, produced, type Request, type Unit } from '@rasm/nx-devkit/plugin';
import { printed } from '@rasm/nx-devkit/process';
import { Array, Effect, Option, Order, Path, Record, Schema } from 'effect';

// --- [TYPES] ---------------------------------------------------------------------------

type Composed = Effect.Success<ReturnType<typeof analyzed>>;
interface Folder extends Unit {
    readonly root: string;
    readonly files: Array.NonEmptyReadonlyArray<string>;
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const EVERY_PROFILE = { COMPOSE_PROFILES: '*' };
const PRODUCERS = ['build--!(compose|buildx)', '^build'];
const ENGINE = [{ runtime: 'docker compose version' }, { runtime: 'docker buildx version' }];

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
const Variables = Schema.fromJsonString(Schema.Record(Schema.String, Schema.Unknown));

// --- [OPERATIONS] ----------------------------------------------------------------------

const analyzed = Effect.fnUntraced(function* (workspaceRoot: string, { root, files }: Folder) {
    const path = yield* Path.Path;
    const cwd = path.join(workspaceRoot, root);
    const [report, variables] = yield* Effect.all(
        [Effect.flatMap(printed('docker', ['compose', 'config', '--format', 'json', '--no-interpolate'], { cwd, env: EVERY_PROFILE }), Schema.decodeEffect(Report)), Effect.flatMap(printed('docker', ['compose', 'config', '--variables', '--format', 'json'], { cwd, env: EVERY_PROFILE }), Schema.decodeEffect(Variables))],
        { concurrency: 'unbounded' },
    );
    const local = (at: string) => Option.liftPredicate(path.relative(workspaceRoot, at), (relative) => path.isAbsolute(at) && !(relative.startsWith('..') || path.isAbsolute(relative)));
    const services = Object.values(report.services);
    const builds = Array.getSomes(services.map(({ build }) => Option.fromUndefinedOr(build)));
    const recipes = Array.getSomes(
        Record.collect(report.services, (service, { build }) =>
            Option.gen(function* () {
                const { context, dockerfile } = yield* Option.fromUndefinedOr(build);
                const file = yield* Option.fromUndefinedOr(dockerfile);
                return { service, recipe: yield* local(path.isAbsolute(file) ? file : path.join(context, file)) };
            }),
        ),
    );
    return {
        name: report.name,
        read: Array.sort(Array.dedupe([...files, path.join(root, '.env'), ...services.flatMap(({ env_file = [] }) => Array.getSomes(env_file.map((file) => local(file.path)))), ...recipes.map(({ recipe }) => recipe)]), Order.String),
        variables: Array.sort(Record.keys(variables), Order.String),
        profiles: Array.sort(Array.dedupe(services.flatMap(({ profiles = [] }) => profiles)), Order.String),
        built: builds.length > 0,
        servers: Record.map(
            Array.groupBy(recipes, ({ recipe }) => path.dirname(recipe)),
            Array.map(({ service }) => service),
        ),
        contexts: Array.dedupe(builds.flatMap(({ context, additional_contexts = {} }) => Array.getSomes([context, ...Object.values(additional_contexts)].map(local)))),
    };
});

const compose = (args: Array.NonEmptyReadonlyArray<string>, description: string, extra: Partial<TargetConfiguration>): TargetConfiguration => ({
    ...extra,
    command: line(['docker', 'compose', ...args]),
    metadata: { description, help: { command: `docker compose ${args[0]} --help`, example: {} } },
});

const served = ({ root }: Folder, project: Composed, services: Array.NonEmptyReadonlyArray<string>, plugin: string) => ({
    build: {
        buildx: compose(['build', '--with-dependencies', ...services], `Builds the images of ${services.join(', ')} of ${project.name} and of the services each depends on into the engine`, { dependsOn: PRODUCERS, options: { cwd: root } }),
    },
    serve: {
        compose: {
            command: `${line(['docker', 'compose', 'up', '--wait', '--no-build', ...services])} && ${line(['docker', 'compose', 'logs', '--follow', ...services])}`,
            continuous: true,
            options: { cwd: root },
            dependsOn: ['build--buildx'],
            readyWhen: getReadyCheckReadyWhen(`${plugin}/ready-check`, undefined, ...services),
            inputs: ['default', '^default', { runtime: line(['docker', 'compose', '--project-directory', root, 'config', '--format', 'json', ...services]) }, ...ENGINE],
            metadata: { description: `Starts ${services.join(', ')} of ${project.name} with the services each depends on from built images and follows their logs, the services staying up after the run`, help: { command: 'docker compose up --help', example: {} } },
        },
    },
});

const targets = ({ root }: Folder, project: Composed) => {
    const inputs = [...project.read.map((file) => `{workspaceRoot}/${file}`), { runtime: line(['docker', 'compose', '--project-directory', root, '--profile', '*', 'config', '--format', 'json', '--no-interpolate', '--no-path-resolution']) }, ...project.variables.map((env) => ({ env })), ...ENGINE];
    const configurations = Record.fromIterableWith(project.profiles, (profile) => [profile, { env: { COMPOSE_PROFILES: profile } }] as const);
    return {
        check: {
            compose: compose(['config', '--quiet'], `Validates every Compose service of ${project.name}, interpolation and env files included`, { cache: true, inputs, options: { env: EVERY_PROFILE } }),
            ...(project.built && { buildx: compose(['build', '--check'], `Lints every image ${project.name} builds through BuildKit, failing on any warning`, { cache: true, inputs, dependsOn: PRODUCERS, options: { env: EVERY_PROFILE } }) }),
        },
        build: project.built ? { compose: compose(['build'], `Builds the images of ${project.name} into the engine`, { dependsOn: PRODUCERS, configurations }) } : {},
        install: { compose: compose(['up', '--wait', '--no-build', '--remove-orphans'], `Starts the services of ${project.name} from built images and waits until each is healthy`, { dependsOn: ['build--*'], configurations }) },
    };
};

const edges = (reports: readonly (readonly [Folder, Composed])[], { projects }: CreateDependenciesContext) => {
    const outputs = produced(projects);
    const builders = (at: string) => outputs.filter(({ output }) => matchesGlob(at, output) || matchesGlob(at, `${output}/**`)).map(({ root }) => root);
    return Effect.succeed(reports.flatMap(([{ root, file }, { contexts }]) => Array.dedupe(contexts.flatMap(builders)).map((target): RawProjectGraphDependency => ({ source: root, target, type: DependencyType.static, sourceFile: file }))));
};

// --- [COMPOSITION] ---------------------------------------------------------------------

const { createNodes, createDependencies } = inferred({
    module: import.meta.url,
    files: ['{compose,docker-compose}{,.override}.{yaml,yml}'],
    options: Schema.Struct({}),
    units: ({ files, path }: Request<unknown>) =>
        Effect.succeed(
            Record.collect(
                Array.groupBy(files, (file) => path.dirname(file)),
                (root, matched): Folder => ({ key: root, root, file: Array.min(matched, Order.String), files: Array.sort(matched, Order.String) }),
            ),
        ),
    read: (missing: Array.NonEmptyReadonlyArray<Folder>, { context }: Request<unknown>) => Effect.forEach(missing, (folder) => Effect.result(Effect.map(analyzed(context.workspaceRoot, folder), (report) => ({ report }))), { concurrency: 'unbounded' }),
    sources: (project: Composed) => ({ tracked: project.read }),
    projects: (project: Composed, folder, { plugin }) => {
        const [own, serving] = [targets(folder, project), Record.map(project.servers, (services) => served(folder, project, services, plugin.name))];
        return [
            { file: folder.file, root: folder.root, name: project.name, technologies: ['compose'], members: { ...own, ...serving[folder.root], build: { ...own.build, ...serving[folder.root]?.build } } },
            ...Record.collect(Record.remove(serving, folder.root), (root, members) => ({ file: folder.file, root, technologies: ['compose'], members })),
        ];
    },
    edges,
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { createDependencies, createNodes };

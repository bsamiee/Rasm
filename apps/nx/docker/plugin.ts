import { matchesGlob } from 'node:path';
import { type CreateDependenciesContext, DependencyType, type RawProjectGraphDependency, type TargetConfiguration } from '@nx/devkit';
import { createProjectRootMappingsFromProjectConfigurations } from '@nx/devkit/internal';
import { ancestors, inferred, produced, type Request, type Unit } from '@rasm/nx-devkit/plugin';
import { printed } from '@rasm/nx-devkit/process';
import { described, type Inputs, line } from '@rasm/nx-devkit/target';
import { Array, Effect, Option, Order, Path, Record, Schema } from 'effect';

// --- [TYPES] ---------------------------------------------------------------------------

type Composed = Effect.Success<ReturnType<typeof analyzed>>;
interface Folder extends Unit {
    readonly root: string;
    readonly files: Array.NonEmptyReadonlyArray<string>;
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const EVERY_PROFILE = { COMPOSE_PROFILES: '*' };
const PRODUCERS = ['build--!(compose)', '^build'];

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
    const recipes = Array.getSomes(builds.map(({ context, dockerfile }) => Option.flatMap(Option.fromUndefinedOr(dockerfile), (file) => local(path.isAbsolute(file) ? file : path.join(context, file)))));
    return {
        name: report.name,
        read: Array.sort(Array.dedupe([...files, path.join(root, '.env'), ...services.flatMap(({ env_file = [] }) => Array.getSomes(env_file.map((file) => local(file.path)))), ...recipes]), Order.String),
        variables: Array.sort(Record.keys(variables), Order.String),
        profiles: Array.sort(Array.dedupe(services.flatMap(({ profiles = [] }) => profiles)), Order.String),
        built: builds.length > 0,
        recipes,
        contexts: Array.dedupe(builds.flatMap(({ context, additional_contexts = {} }) => Array.getSomes([context, ...Object.values(additional_contexts)].map(local)))),
    };
});

const targets = ({ root }: Folder, project: Composed) => {
    const inputs: Inputs = [
        ...project.read.map((file) => `{workspaceRoot}/${file}`),
        { runtime: line(['docker', 'compose', '--project-directory', root, '--profile', '*', 'config', '--format', 'json', '--no-interpolate', '--no-path-resolution']) },
        { runtime: 'docker compose version' },
        { runtime: 'docker buildx version' },
        ...project.variables.map((env) => ({ env })),
        { externalDependencies: [] },
    ];
    const configurations = Record.fromIterableWith(project.profiles, (profile) => [profile, { env: { COMPOSE_PROFILES: profile } }] as const);
    const compose = (args: Array.NonEmptyReadonlyArray<string>, description: string, { options, ...extra }: Partial<TargetConfiguration>): TargetConfiguration => ({
        ...extra,
        command: line(['docker', 'compose', ...args]),
        options: { cwd: root, ...options },
        metadata: described({ description, help: `docker compose ${args[0]} --help` }),
    });
    return {
        check: {
            compose: compose(['config', '--quiet'], `Validates every Compose service of ${project.name}, interpolation and env files included`, { cache: true, inputs, options: { env: EVERY_PROFILE } }),
            ...(project.built && { buildx: compose(['build', '--check'], `Lints every image ${project.name} builds through BuildKit, failing on any warning`, { cache: true, inputs, dependsOn: PRODUCERS, options: { env: EVERY_PROFILE } }) }),
        },
        build: project.built ? { compose: compose(['build'], `Builds the images of ${project.name} into the engine`, { dependsOn: PRODUCERS, configurations }) } : {},
        install: { compose: compose(['up', '--detach', '--wait', '--remove-orphans'], `Starts the services of ${project.name} and waits until each is healthy`, { dependsOn: ['build'], configurations }) },
    };
};

const edges = Effect.fnUntraced(function* (reports: readonly (readonly [Folder, Composed])[], { projects }: CreateDependenciesContext) {
    const path = yield* Path.Path;
    const roots = createProjectRootMappingsFromProjectConfigurations(projects);
    const outputs = produced(projects);
    const builders = (at: string) => outputs.filter(({ output }) => matchesGlob(at, output) || matchesGlob(at, `${output}/**`)).map(({ root }) => root);
    const holder = (file: string) => Array.findFirst(ancestors(path, path.dirname(file)), (folder) => roots.has(folder));
    return reports.flatMap(([{ root, file }, { contexts, recipes }]) => Array.dedupe([...contexts.flatMap(builders), ...Array.getSomes(recipes.map(holder))]).map((target): RawProjectGraphDependency => ({ source: root, target, type: DependencyType.static, sourceFile: file })));
});

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
    projects: (project: Composed, folder) => [{ file: folder.file, root: folder.root, name: project.name, technologies: ['compose'], members: targets(folder, project) }],
    edges,
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { createDependencies, createNodes };

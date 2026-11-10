import { matchesGlob } from 'node:path';
import { type CreateDependencies, type CreateNodes, type CreateNodesContext, type CreateNodesResultArray, DependencyType, getOutputsForTargetAndConfiguration, hashArray, logger, type RawProjectGraphDependency, type TargetConfiguration } from '@nx/devkit';
import { createProjectRootMappingsFromProjectConfigurations, hashObject, hashWithWorkspaceContext } from '@nx/devkit/internal';
import { aggregated, cached, identity, inferred, line, type Members, reported } from '@rasm/nx-devkit/inference';
import { present, printed } from '@rasm/nx-devkit/process';
import { Array, Effect, Option, Order, Path, Record, Result, Schema } from 'effect';

// --- [TYPES] ---------------------------------------------------------------------------

type Project = Effect.Success<ReturnType<typeof analyzed>>;

// --- [CONSTANTS] -----------------------------------------------------------------------

const TECHNOLOGIES = ['compose'];
const EVERY_PROFILE = { COMPOSE_PROFILES: '*' };
const PRODUCERS = ['build--!(compose)', '^build'];
const FILES = '**/{compose,docker-compose}{,.override}.{yaml,yml}';

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

const analyzed = Effect.fnUntraced(function* (workspaceRoot: string, root: string, matched: Array.NonEmptyReadonlyArray<string>, available: boolean) {
    const path = yield* Path.Path;
    const cwd = path.join(workspaceRoot, root);
    const files = Array.sort(matched, Order.String);
    const [report, variables] = available
        ? yield* Effect.all(
              [
                  Effect.flatMap(printed('docker', ['compose', 'config', '--format', 'json', '--no-interpolate'], { cwd, env: EVERY_PROFILE }), Schema.decodeEffect(Report)),
                  Effect.flatMap(printed('docker', ['compose', 'config', '--variables', '--format', 'json'], { cwd, env: EVERY_PROFILE }), Schema.decodeEffect(Variables)),
              ],
              { concurrency: 'unbounded' },
          )
        : [{ name: path.basename(cwd), services: {} }, {}];
    const local = (at: string) => Option.liftPredicate(path.relative(workspaceRoot, at), (relative) => path.isAbsolute(at) && !(relative.startsWith('..') || path.isAbsolute(relative)));
    const services = Object.values(report.services);
    const builds = Array.getSomes(services.map(({ build }) => Option.fromUndefinedOr(build)));
    return {
        root,
        files,
        present: available,
        name: report.name,
        read: Array.sort(
            Array.dedupe([
                ...files,
                path.join(root, '.env'),
                ...services.flatMap(({ env_file = [] }) => Array.getSomes(env_file.map((file) => local(file.path)))),
                ...Array.getSomes(builds.map(({ context, dockerfile }) => Option.flatMap(Option.fromUndefinedOr(dockerfile), (file) => local(path.isAbsolute(file) ? file : path.join(context, file))))),
            ]),
            Order.String,
        ),
        variables: Array.sort(Record.keys(variables), Order.String),
        profiles: Array.sort(Array.dedupe(services.flatMap(({ profiles = [] }) => profiles)), Order.String),
        built: builds.length > 0,
        contexts: Array.dedupe(builds.flatMap(({ context, additional_contexts = {} }) => Array.getSomes([context, ...Object.values(additional_contexts)].map(local)))),
    };
});

const targets = (plugin: string, project: Project) => {
    const inputs = [
        ...project.read.map((file) => `{workspaceRoot}/${file}`),
        { runtime: line(['docker', 'compose', '--project-directory', project.root, '--profile', '*', 'config', '--format', 'json', '--no-interpolate', '--no-path-resolution']) },
        ...project.variables.map((env) => ({ env })),
        { externalDependencies: [] },
    ];
    const configurations = Record.fromIterableWith(project.profiles, (profile) => [profile, { env: { COMPOSE_PROFILES: profile } }] as const);
    const compose = (args: Array.NonEmptyReadonlyArray<string>, description: string, { options, ...extra }: Partial<TargetConfiguration>): TargetConfiguration => ({
        ...extra,
        command: line(['docker', 'compose', ...args]),
        options: { cwd: project.root, ...options },
        metadata: { technologies: TECHNOLOGIES, description: project.present ? description : `${description}, inferred from ${project.files[0]} alone since docker compose is absent on this host`, help: { command: `docker compose ${args[0]} --help`, example: {} } },
    });
    return aggregated(
        {
            check: {
                compose: compose(['config', '--quiet'], `Validates every Compose service of ${project.name}, interpolation and env files included`, { cache: true, inputs, options: { env: EVERY_PROFILE } }),
                ...(project.built ? { buildx: compose(['build', '--check'], `Lints every image ${project.name} builds through BuildKit, failing on any warning`, { cache: true, inputs, dependsOn: PRODUCERS, options: { env: EVERY_PROFILE } }) } : {}),
            },
            build: project.built ? { compose: compose(['build'], `Builds the images of ${project.name} into the engine`, { dependsOn: PRODUCERS, configurations }) } : {},
            install: { compose: compose(['up', '--detach', '--wait', '--remove-orphans'], `Starts the services of ${project.name} and waits until each is healthy`, { dependsOn: ['build', '^install'], configurations }) },
            upgrade: {
                compose: {
                    executor: `${plugin}:upgrade`,
                    options: { files: project.files },
                    outputs: project.files.map((file) => `{workspaceRoot}/${file}`),
                    metadata: { technologies: TECHNOLOGIES, description: `Moves each image tag the Compose files of ${project.name} state to the newest registry tag of the same form`, help: { command: 'crane ls --help', example: { options: { dryRun: true } } } },
                },
            },
        } satisfies Members,
        TECHNOLOGIES,
    );
};

const reports = new Map<string, readonly Project[]>();

const keyed = (workspaceRoot: string, options: unknown) => hashArray([workspaceRoot, hashObject(options ?? {})]);

const read = Effect.fnUntraced(function* (workspaceRoot: string, files: readonly string[]) {
    const path = yield* Path.Path;
    const available = Option.isSome(yield* present('docker', ['compose', 'version']));
    const folders = Record.toEntries(Array.groupBy(files, (file) => path.dirname(file)));
    const results = yield* Effect.forEach(folders, ([root, matched]) => Effect.result(analyzed(workspaceRoot, root, matched, available)), { concurrency: 'unbounded' });
    yield* Effect.sync(() => {
        if (!available) {
            logger.warn(`docker compose is absent on this host, so ${files.join(', ')} take their targets from their folder alone, and their runs report the missing tool`);
        }
    });
    return {
        available,
        projects: Array.getSuccesses(results),
        failures: Array.filterMap(Array.zip(folders, results), ([[, matched], result]) => Result.map(Result.flip(result), (error) => [Array.min(matched, Order.String), error] as const)),
    };
});

const graph = Effect.fnUntraced(function* ({ workspaceRoot }: CreateNodesContext, files: readonly string[], options: unknown) {
    const plugin = yield* identity(import.meta.url, workspaceRoot);
    const digest = (found: readonly Project[]) =>
        Effect.map(
            Effect.promise(() => hashWithWorkspaceContext(workspaceRoot, [...files, ...found.flatMap((project) => project.read)])),
            (content) => hashArray([content, plugin.digest, ...files]),
        );
    const cache = yield* cached<readonly Project[]>(plugin, options);
    const valid = yield* cache.get(FILES, digest);
    const { projects, failures, available } = yield* Option.match(valid, {
        onSome: (held) => Effect.succeed({ projects: held, failures: Array.empty<readonly [string, Error]>(), available: true }),
        onNone: () => read(workspaceRoot, files),
    });
    yield* Option.isNone(valid) && available && failures.length === 0 ? Effect.flatMap(digest(projects), (now) => cache.set(FILES, projects, now)) : Effect.void;
    reports.set(keyed(workspaceRoot, options), projects);
    return yield* reported(
        projects.map((project): CreateNodesResultArray[number] => [project.files[0], { projects: { [project.root]: { name: project.name, metadata: { technologies: TECHNOLOGIES }, targets: targets(plugin.name, project) } } }]),
        failures,
    );
});

// --- [COMPOSITION] ---------------------------------------------------------------------

const createNodes: CreateNodes = [FILES, (files, options, context) => inferred(graph(context, files, options))];

const createDependencies: CreateDependencies = (options, { workspaceRoot, projects }) => {
    const roots = createProjectRootMappingsFromProjectConfigurations(projects);
    const produced = Record.toEntries(projects).flatMap(([name, data]) =>
        Record.keys(Record.filter(data.targets ?? {}, (target) => target.outputs !== undefined)).flatMap((target) =>
            getOutputsForTargetAndConfiguration({ project: name, target }, {}, { name, type: 'lib', data })
                .filter((output) => !output.startsWith('!'))
                .map((output) => [name, output] as const),
        ),
    );
    const owners = (at: string) => produced.filter(([, output]) => matchesGlob(at, output) || matchesGlob(at, `${output}/**`)).map(([name]) => name);
    return (reports.get(keyed(workspaceRoot, options)) ?? []).flatMap(({ root, files, contexts }) =>
        Option.toArray(Option.fromUndefinedOr(roots.get(root))).flatMap((source) =>
            Array.dedupe(contexts.flatMap(owners))
                .filter((target) => target !== source)
                .map((target): RawProjectGraphDependency => ({ source, target, type: DependencyType.static, sourceFile: files[0] })),
        ),
    );
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { createDependencies, createNodes };

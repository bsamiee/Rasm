import { findPackageJSON } from 'node:module';
import { layer } from '@effect/platform-node/NodeServices';
import { AggregateCreateNodesError, type CreateDependencies, type CreateNodes, type CreateNodesResultArray, DependencyType, hashArray, type RawProjectGraphDependency, type TargetConfiguration } from '@nx/devkit';
import { createProjectRootMappingsFromProjectConfigurations, findProjectForPath, hashObject, hashWithWorkspaceContext, PluginCache, workspaceDataDirectory } from '@nx/devkit/internal';
import { Array, Effect, FileSystem, Option, Order, Path, Record, Schema, Stream } from 'effect';
import { ChildProcess } from 'effect/process';

// --- [CONSTANTS] -----------------------------------------------------------------------

const FILES = '**/{compose,docker-compose}{,.override}.{yaml,yml}';
const TECHNOLOGIES = ['compose'];
const EVERY_PROFILE = { COMPOSE_PROFILES: '*' };

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
const Manifest = Schema.fromJsonString(Schema.Struct({ name: Schema.String }));
const Variables = Schema.fromJsonString(Schema.Record(Schema.String, Schema.Unknown));

// --- [ERRORS] --------------------------------------------------------------------------

class ComposeError extends Schema.TaggedError<ComposeError, { readonly brand: unique symbol }>()('ComposeError', { subject: Schema.String, detail: Schema.String }) {
    override get message() {
        return `${this.subject}: ${this.detail}`;
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

const spawned = (args: readonly string[], cwd: string) =>
    Effect.scoped(
        Effect.gen(function* () {
            const handle = yield* ChildProcess.make('docker', ['compose', ...args], { cwd, env: EVERY_PROFILE, extendEnv: true, stdin: 'ignore' });
            const [stdout, stderr, code] = yield* Effect.all([Stream.mkString(Stream.decodeText(handle.stdout)), Stream.mkString(Stream.decodeText(handle.stderr)), handle.exitCode], { concurrency: 'unbounded' });
            return code === 0 ? stdout : yield* ComposeError.make({ subject: ['docker', 'compose', ...args].join(' '), detail: `exited ${code} in ${cwd}: ${stderr.trim()}` });
        }),
    );

const analyzed = Effect.fnUntraced(function* (workspaceRoot: string, root: string, files: Array.NonEmptyReadonlyArray<string>) {
    const path = yield* Path.Path;
    const cwd = path.join(workspaceRoot, root);
    const [report, variables] = yield* Effect.all([Effect.flatMap(spawned(['config', '--format', 'json', '--no-interpolate'], cwd), Schema.decodeEffect(Report)), Effect.flatMap(spawned(['config', '--variables', '--format', 'json'], cwd), Schema.decodeEffect(Variables))], {
        concurrency: 'unbounded',
    });
    const services = Record.values(report.services);
    const builds = services.flatMap(({ build }) => Option.toArray(Option.fromUndefinedOr(build)));
    const local = (at: string) => Option.liftPredicate(path.relative(workspaceRoot, at), (relative) => path.isAbsolute(at) && !(relative.startsWith('..') || path.isAbsolute(relative)));
    return {
        root,
        name: report.name,
        files: Array.sort(files, Order.String),
        read: Array.dedupe([
            ...files,
            path.join(root, '.env'),
            ...services.flatMap(({ env_file = [] }) => env_file.flatMap((held) => Option.toArray(local(held.path)))),
            ...builds.flatMap(({ context, dockerfile }) => Option.toArray(Option.flatMap(Option.fromUndefinedOr(dockerfile), (file) => local(path.isAbsolute(file) ? file : path.join(context, file))))),
        ]).toSorted(),
        variables: Record.keys(variables).toSorted(),
        profiles: Array.dedupe(services.flatMap(({ profiles = [] }) => profiles)).toSorted(),
        built: builds.length > 0,
        contexts: Array.dedupe(builds.flatMap(({ context, additional_contexts = {} }) => [context, ...Record.values(additional_contexts)].flatMap((at) => Option.toArray(local(at))))).toSorted(),
    };
});

type Project = Effect.Success<ReturnType<typeof analyzed>>;

const targets = (project: Project): Record<string, TargetConfiguration> => {
    const inputs = [...project.read.map((file) => `{workspaceRoot}/${file}`), ...project.variables.map((env) => ({ env })), { externalDependencies: [] }];
    const configurations = Record.fromIterableWith(project.profiles, (profile) => [profile, { env: { COMPOSE_PROFILES: profile } }] as const);
    const compose = (args: Array.NonEmptyReadonlyArray<string>, description: string, { options, ...extra }: Partial<TargetConfiguration>): TargetConfiguration => ({
        ...extra,
        command: ['docker', 'compose', ...args].join(' '),
        options: { cwd: project.root, ...options },
        metadata: { technologies: TECHNOLOGIES, description, help: { command: `docker compose ${args[0]} --help`, example: {} } },
    });
    const inferred: Record<string, TargetConfiguration> = {
        'check--compose': compose(['config', '--quiet'], `Validates every Compose service of ${project.name}, interpolation and env files included`, { cache: true, inputs, options: { env: EVERY_PROFILE } }),
        ...(project.built
            ? {
                  'check--buildx': compose(['build', '--check'], `Lints every image ${project.name} builds through BuildKit, failing on any warning`, { cache: true, inputs, options: { env: EVERY_PROFILE } }),
                  'build--compose': compose(['build'], `Builds the images of ${project.name} into the engine`, { cache: false, dependsOn: ['^build'], configurations }),
              }
            : {}),
        install: compose(['up', '--detach', '--wait', '--remove-orphans'], `Starts the services of ${project.name} and waits until each is healthy`, { cache: false, dependsOn: ['build', '^install'], configurations }),
    };
    return {
        ...inferred,
        ...Record.map(
            Array.groupBy(
                Record.toEntries(inferred).filter(([name]) => name.includes('--')),
                ([name]) => name.slice(0, name.indexOf('--')),
            ),
            (members, verb): TargetConfiguration => ({
                executor: 'nx:noop',
                dependsOn: [`${verb}--*`],
                inputs: ['...', ...Array.dedupe(members.flatMap(([, held]) => held.inputs ?? []))],
                metadata: { technologies: TECHNOLOGIES, description: `Runs every ${verb}--* target` },
            }),
        ),
    };
};

const reports = new Map<string, readonly Project[]>();

const scanned = Effect.fnUntraced(function* (workspaceRoot: string, files: readonly string[]) {
    const path = yield* Path.Path;
    const present = yield* Effect.isSuccess(spawned(['version', '--short'], workspaceRoot));
    const [projects, failures] = present ? yield* Effect.partition(Record.toEntries(Array.groupBy(files, (file) => path.dirname(file))), ([root, held]) => Effect.mapError(analyzed(workspaceRoot, root, held), (error): [string, Error] => [root, new Error(error.message)]), { concurrency: 'unbounded' }) : [[], []];
    return { cached: present && failures.length === 0, projects, failures };
});

const keyed = (workspaceRoot: string, options: unknown) => hashArray([workspaceRoot, hashObject(options ?? {})]);

const graph = Effect.fnUntraced(function* (workspaceRoot: string, files: readonly string[], options: unknown) {
    const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
    const located = yield* Effect.fromNullishOr(findPackageJSON(import.meta.url));
    const { name } = yield* Effect.flatMap(fs.readFileString(located), Schema.decodeEffect(Manifest));
    const modules = [located, ...(yield* fs.readDirectory(import.meta.dirname)).toSorted().map((module) => path.join(import.meta.dirname, module))];
    const hash = hashArray([yield* Effect.promise(() => hashWithWorkspaceContext(workspaceRoot, [FILES])), ...(yield* Effect.forEach(modules, (module) => fs.readFileString(module))), ...files.toSorted()]);
    const cache = new PluginCache<readonly Project[]>(path.join(workspaceDataDirectory, `${path.basename(name)}-${hashObject(options ?? {})}.hash`));
    const found = yield* Option.match(Option.fromNullishOr(cache.get(hash)), {
        onSome: (held) => Effect.succeed({ projects: held, failures: Array.empty<[string, Error]>() }),
        onNone: () =>
            Effect.tap(scanned(workspaceRoot, files), ({ cached, projects }) =>
                cached
                    ? Effect.sync(() => {
                          cache.set(hash, projects);
                          cache.writeToDisk();
                      })
                    : Effect.void,
            ),
    });
    reports.set(keyed(workspaceRoot, options), found.projects);
    return found;
});

// --- [COMPOSITION] ---------------------------------------------------------------------

const createNodes: CreateNodes = [
    FILES,
    (files, options, context) =>
        Effect.runPromise(
            Effect.flatMap(
                Effect.mapError(graph(context.workspaceRoot, files, options), (error) => new AggregateCreateNodesError([[null, new Error(error.message)]], [])),
                ({ projects, failures }) => {
                    const nodes: CreateNodesResultArray = projects.map((project) => [project.files[0], { projects: { [project.root]: { name: project.name, metadata: { technologies: TECHNOLOGIES }, targets: targets(project) } } }]);
                    return failures.length === 0 ? Effect.succeed(nodes) : Effect.fail(new AggregateCreateNodesError(failures, nodes));
                },
            ).pipe(Effect.provide(layer)),
        ),
];

const createDependencies: CreateDependencies = (options, { workspaceRoot, projects }) => {
    const roots = createProjectRootMappingsFromProjectConfigurations(projects);
    return (reports.get(keyed(workspaceRoot, options)) ?? []).flatMap(({ root, files, contexts }) =>
        Option.toArray(Option.fromUndefinedOr(roots.get(root))).flatMap((source) =>
            Array.dedupe(contexts.flatMap((at) => Option.toArray(Option.fromNullishOr(findProjectForPath(at, roots)))))
                .filter((target) => target !== source)
                .map((target): RawProjectGraphDependency => ({ source, target, type: DependencyType.static, sourceFile: files[0] })),
        ),
    );
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { createDependencies, createNodes };

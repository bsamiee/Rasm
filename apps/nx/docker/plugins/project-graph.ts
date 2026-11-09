import { findPackageJSON } from 'node:module';
import { posix } from 'node:path';
import { layer } from '@effect/platform-node/NodeServices';
import { AggregateCreateNodesError, type CreateDependencies, type CreateNodes, type CreateNodesResultArray, DependencyType, hashArray, type RawProjectGraphDependency, type TargetConfiguration } from '@nx/devkit';
import { createProjectRootMappingsFromProjectConfigurations, hashObject, hashWithWorkspaceContext, PluginCache, workspaceDataDirectory } from '@nx/devkit/internal';
import { Array, Effect, FileSystem, Option, Path, Record, Result, Schema, Stream } from 'effect';
import { ChildProcess } from 'effect/process';

// --- [CONSTANTS] -----------------------------------------------------------------------

const FILES = '**/{compose,docker-compose}{,.override}.{yaml,yml}';
const TECHNOLOGIES = ['compose'];
const VERBS = ['build', 'check', 'fix', 'upgrade'];

// --- [MODELS] --------------------------------------------------------------------------

const Build = Schema.Struct({ context: Schema.String, dockerfile: Schema.optionalKey(Schema.String), additional_contexts: Schema.optionalKey(Schema.Record(Schema.String, Schema.String)) });
const Report = Schema.fromJsonString(
    Schema.Struct({
        name: Schema.String,
        services: Schema.Record(Schema.String, Schema.Struct({ profiles: Schema.optionalKey(Schema.Array(Schema.String)), build: Schema.optionalKey(Build) })),
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
            const handle = yield* ChildProcess.make('docker', args, { cwd, extendEnv: true, stdin: 'ignore' });
            const [stdout, stderr, code] = yield* Effect.all([Stream.mkString(Stream.decodeText(handle.stdout)), Stream.mkString(Stream.decodeText(handle.stderr)), handle.exitCode], { concurrency: 'unbounded' });
            return code === 0 ? stdout : yield* ComposeError.make({ subject: ['docker', ...args].join(' '), detail: `exited ${code} in ${cwd}: ${stderr.trim()}` });
        }),
    ).pipe(Effect.mapError((error) => (Schema.is(ComposeError)(error) ? error : ComposeError.make({ subject: ['docker', ...args].join(' '), detail: error.message }))));

const analyzed = Effect.fnUntraced(function* (workspaceRoot: string, root: string, files: readonly string[]) {
    const path = yield* Path.Path;
    const cwd = path.join(workspaceRoot, root);
    const [report, variables] = yield* Effect.all(
        [Effect.flatMap(spawned(['compose', '--profile', '*', 'config', '--format', 'json', '--no-interpolate'], cwd), Schema.decodeEffect(Report)), Effect.flatMap(spawned(['compose', '--profile', '*', 'config', '--variables', '--format', 'json'], cwd), Schema.decodeEffect(Variables))],
        { concurrency: 'unbounded' },
    ).pipe(Effect.mapError((error) => (Schema.is(ComposeError)(error) ? error : ComposeError.make({ subject: root, detail: error.message }))));
    const builds = Record.values(report.services).flatMap(({ build }) => Option.toArray(Option.fromUndefinedOr(build)));
    const inside = (at: string) => Option.liftPredicate(path.relative(workspaceRoot, at), (relative) => !(relative.startsWith('..') || path.isAbsolute(relative)));
    return {
        root,
        name: report.name,
        files: files.toSorted(),
        variables: Record.keys(variables).toSorted(),
        profiles: Array.dedupe(Record.values(report.services).flatMap(({ profiles = [] }) => profiles)).toSorted(),
        built: builds.length > 0,
        dockerfiles: Array.dedupe(builds.flatMap(({ context, dockerfile }) => Option.toArray(Option.flatMap(Option.fromUndefinedOr(dockerfile), (file) => inside(path.resolve(context, file)))))).toSorted(),
        contexts: Array.dedupe(builds.flatMap(({ context, additional_contexts = {} }) => [context, ...Record.values(additional_contexts).filter((at) => path.isAbsolute(at) || !at.includes(':'))].flatMap((at) => Option.toArray(inside(path.resolve(context, at)))))).toSorted(),
    };
});

type Project = Effect.Success<ReturnType<typeof analyzed>>;

const compose = (project: Project, help: string, args: readonly string[], description: string, extra: Partial<TargetConfiguration>): TargetConfiguration => ({
    command: ['docker', ...args].join(' '),
    options: { cwd: project.root },
    metadata: { technologies: TECHNOLOGIES, description, help: { command: `docker ${help} --help`, example: {} } },
    ...extra,
});

const targets = (project: Project): Record<string, TargetConfiguration> => {
    const inputs = [...project.files.map((file) => `{workspaceRoot}/${file}`), ...project.variables.map((env) => ({ env })), { externalDependencies: [] }];
    const profiled = (verb: readonly string[]) => Record.fromIterableWith(project.profiles, (profile) => [profile, { command: ['docker', 'compose', '--profile', profile, ...verb].join(' ') }] as const);
    const inferred: Record<string, TargetConfiguration> = {
        'check--compose': compose(project, 'compose config', ['compose', '--profile', "'*'", 'config', '--quiet'], `Validates every Compose service of ${project.name}, interpolation included`, { cache: true, inputs }),
        ...(project.built
            ? {
                  'check--buildx': compose(project, 'buildx bake', ['buildx', 'bake', '--call', 'check'], `Runs BuildKit build checks over every image ${project.name} builds`, { cache: true, inputs: [...inputs, ...project.dockerfiles.map((file) => `{workspaceRoot}/${file}`)] }),
                  'build--compose': compose(project, 'compose build', ['compose', 'build'], `Builds the images of ${project.name} into the engine`, { cache: false, dependsOn: ['^build'], configurations: profiled(['build']) }),
              }
            : {}),
        install: compose(project, 'compose up', ['compose', 'up', '--detach', '--wait', '--remove-orphans'], `Starts the services of ${project.name} and waits until each is healthy`, { cache: false, dependsOn: ['build', '^install'], configurations: profiled(['up', '--detach', '--wait', '--remove-orphans']) }),
    };
    return {
        ...inferred,
        ...Record.fromIterableWith(
            VERBS.filter((verb) => Record.keys(inferred).some((name) => name.startsWith(`${verb}--`))),
            (verb) =>
                [
                    verb,
                    {
                        executor: 'nx:noop',
                        dependsOn: [{ target: `${verb}--*`, params: 'forward' }],
                        inputs: [
                            '...',
                            ...Array.dedupe(
                                Record.collect(
                                    Record.filter(inferred, (_, name) => name.startsWith(`${verb}--`)),
                                    (_, held) => held.inputs ?? [],
                                ).flat(),
                            ),
                        ],
                        metadata: { technologies: TECHNOLOGIES, description: `Runs every ${verb}--* target` },
                    } satisfies TargetConfiguration,
                ] as const,
        ),
    };
};
const reports = new Map<string, readonly Project[]>();

const scanned = Effect.fnUntraced(function* (workspaceRoot: string, folders: Readonly<Record<string, readonly string[]>>) {
    const engine = yield* Effect.result(spawned(['compose', 'version', '--short'], workspaceRoot));
    const results = Result.isFailure(engine) ? [] : yield* Effect.forEach(Record.toEntries(folders), ([root, held]) => Effect.map(Effect.result(analyzed(workspaceRoot, root, held)), (result) => [root, result] as const), { concurrency: 'unbounded' });
    return {
        cached: Result.isSuccess(engine) && results.every(([, result]) => Result.isSuccess(result)),
        projects: results.flatMap(([, result]) => (Result.isSuccess(result) ? [result.success] : [])),
        failures: results.flatMap(([root, result]): [string, Error][] => (Result.isFailure(result) ? [[root, new Error(result.failure.message)]] : [])),
    };
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
        onSome: (held) => Effect.succeed({ cached: true, projects: held, failures: Array.empty<[string, Error]>() }),
        onNone: () =>
            Effect.tap(
                scanned(
                    workspaceRoot,
                    Array.groupBy(files, (file) => path.dirname(file)),
                ),
                ({ cached, projects }) =>
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
                    const nodes: CreateNodesResultArray = projects.map((project) => [project.files[0] ?? project.root, { projects: { [project.root]: { name: project.name, metadata: { technologies: TECHNOLOGIES }, targets: targets(project) } } }]);
                    return failures.length === 0 ? Effect.succeed(nodes) : Effect.fail(new AggregateCreateNodesError(failures, nodes));
                },
            ).pipe(Effect.provide(layer)),
        ),
];

const createDependencies: CreateDependencies = (options, { workspaceRoot, projects }) => {
    const roots = createProjectRootMappingsFromProjectConfigurations(projects);
    const owner = (at: string): Option.Option<string> => Option.orElse(Option.fromUndefinedOr(roots.get(at)), () => (at === '.' ? Option.none() : owner(posix.dirname(at))));
    return (reports.get(keyed(workspaceRoot, options)) ?? []).flatMap((project) =>
        Option.toArray(Option.fromUndefinedOr(roots.get(project.root))).flatMap((source) =>
            Array.dedupe(project.contexts.flatMap((at) => Option.toArray(owner(at))))
                .filter((target) => target !== source)
                .map((target): RawProjectGraphDependency => ({ source, target, type: DependencyType.static, sourceFile: project.files[0] ?? project.root })),
        ),
    );
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { createDependencies, createNodes };

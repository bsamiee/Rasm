import { matchesGlob } from 'node:path';
import { layer } from '@effect/platform-node/NodeServices';
import { type CreateDependencies, type CreateNodes, type CreateNodesResultArray, DependencyType, hashArray, logger, type ProjectGraphProjectNode, type RawProjectGraphDependency, type TargetConfiguration } from '@nx/devkit';
import { createProjectRootMappingsFromProjectConfigurations, findProjectForPath, getOutputs, hashObject, hashWithWorkspaceContext, PluginCache, quoteShellArg, workspaceDataDirectory } from '@nx/devkit/internal';
import { Array, Effect, Formatter, Function, Option, Order, Path, Record, Result, Schema, Stream, String } from 'effect';
import { ChildProcess } from 'effect/process';

// --- [CONSTANTS] -----------------------------------------------------------------------

const TECHNOLOGIES = ['compose'];
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

// --- [ERRORS] --------------------------------------------------------------------------

class ComposeError extends Schema.TaggedError<ComposeError, { readonly brand: unique symbol }>()('ComposeError', { subject: Schema.String, detail: Schema.String, cause: Schema.optionalKey(Schema.Defect()) }) {
    override get message() {
        return [`${this.subject}: ${this.detail}`, ...(this.cause === undefined ? [] : [Formatter.format(this.cause)])].join(': ');
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

const spawned = Effect.fnUntraced(function* (args: readonly string[], cwd: string) {
    const handle = yield* ChildProcess.make('docker', ['compose', ...args], { cwd, env: EVERY_PROFILE, extendEnv: true, stdin: 'ignore' });
    const [stdout, stderr, code] = yield* Effect.all([Stream.mkString(Stream.decodeText(handle.stdout)), Stream.mkString(Stream.decodeText(handle.stderr)), handle.exitCode], { concurrency: 'unbounded' });
    return code === 0 ? stdout : yield* ComposeError.make({ subject: ['docker', 'compose', ...args].join(' '), detail: `exited ${code} in ${cwd}: ${stderr.trim()}` });
}, Effect.scoped);

const analyzed = Effect.fnUntraced(function* (workspaceRoot: string, root: string, matched: Array.NonEmptyReadonlyArray<string>) {
    const path = yield* Path.Path;
    const cwd = path.join(workspaceRoot, root);
    const files = Array.sort(matched, Order.String);
    const read = yield* Effect.result(Effect.all([Effect.flatMap(spawned(['config', '--format', 'json', '--no-interpolate'], cwd), Schema.decodeEffect(Report)), Effect.flatMap(spawned(['config', '--variables', '--format', 'json'], cwd), Schema.decodeEffect(Variables))], { concurrency: 'unbounded' }));
    const local = (at: string) => Option.liftPredicate(path.relative(workspaceRoot, at), (relative) => path.isAbsolute(at) && !(relative.startsWith('..') || path.isAbsolute(relative)));
    const own = [...files, path.join(root, '.env')];
    return {
        root,
        files,
        unread: Result.getFailure(read),
        ...Option.match(Result.getSuccess(read), {
            onNone: () => ({ name: path.basename(cwd), read: Array.sort(own, Order.String), variables: [], profiles: [], built: false, contexts: [] }),
            onSome: ([report, variables]) => {
                const services = Object.values(report.services);
                const builds = Array.getSomes(services.map(({ build }) => Option.fromUndefinedOr(build)));
                return {
                    name: report.name,
                    read: Array.sort(
                        Array.dedupe([
                            ...own,
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
            },
        }),
    };
});

type Project = Effect.Success<ReturnType<typeof analyzed>>;

const targets = (project: Project): Record<string, TargetConfiguration> => {
    const inputs = [
        ...project.read.map((file) => `{workspaceRoot}/${file}`),
        { runtime: ['docker', 'compose', '--project-directory', quoteShellArg(project.root), '--profile', quoteShellArg('*'), 'config', '--format', 'json', '--no-interpolate', '--no-path-resolution'].join(' ') },
        ...project.variables.map((env) => ({ env })),
        { externalDependencies: [] },
    ];
    const configurations = Record.fromIterableWith(project.profiles, (profile) => [profile, { env: { COMPOSE_PROFILES: profile } }] as const);
    const origin = Option.map(project.unread, ({ message }) => `inferred from ${project.files[0]} alone since Compose did not read it: ${Array.headNonEmpty(String.split(message, '\n'))}`);
    const compose = (args: Array.NonEmptyReadonlyArray<string>, description: string, { options, ...extra }: Partial<TargetConfiguration>): TargetConfiguration => ({
        ...extra,
        command: ['docker', 'compose', ...args].join(' '),
        options: { cwd: project.root, ...options },
        metadata: { technologies: TECHNOLOGIES, description: Option.match(origin, { onNone: () => description, onSome: (note) => `${description}, ${note}` }), help: { command: `docker compose ${args[0]} --help`, example: {} } },
    });
    const inferred: Record<string, TargetConfiguration> = {
        'check--compose': compose(['config', '--quiet'], `Validates every Compose service of ${project.name}, interpolation and env files included`, { cache: true, inputs, options: { env: EVERY_PROFILE } }),
        ...(project.built
            ? {
                  'check--buildx': compose(['build', '--check'], `Lints every image ${project.name} builds through BuildKit, failing on any warning`, { cache: true, inputs, dependsOn: PRODUCERS, options: { env: EVERY_PROFILE } }),
                  'build--compose': compose(['build'], `Builds the images of ${project.name} into the engine`, { cache: false, dependsOn: PRODUCERS, configurations }),
              }
            : {}),
        'install--compose': compose(['up', '--detach', '--wait', '--remove-orphans'], `Starts the services of ${project.name} and waits until each is healthy`, { cache: false, dependsOn: ['build', '^install'], configurations }),
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
                inputs: ['...', ...Array.dedupe(members.flatMap(([, member]) => member.inputs ?? []))],
                metadata: { technologies: TECHNOLOGIES, description: `Runs every ${verb}--* target` },
            }),
        ),
    };
};

const reports = new Map<string, readonly Project[]>();

const keyed = (workspaceRoot: string, options: unknown) => hashArray([workspaceRoot, hashObject(options ?? {})]);

const graph = Effect.fnUntraced(function* (workspaceRoot: string, files: readonly string[], options: unknown) {
    const path = yield* Path.Path;
    const digest = (read: readonly string[]) => Effect.promise(() => hashWithWorkspaceContext(workspaceRoot, [...read]));
    const key = hashArray([yield* digest(files), hashObject(options ?? {})]);
    const cache = new PluginCache<{ readonly projects: readonly Omit<Project, 'unread'>[]; readonly digest: string }>(path.join(workspaceDataDirectory, `compose-${hashObject(options ?? {})}.hash`));
    const held = Option.fromNullishOr(cache.get(key));
    const current = yield* Effect.transposeOption(Option.map(held, (entry) => digest(entry.projects.flatMap(({ read }) => read))));
    const projects = yield* Option.match(
        Option.filter(held, (entry) => Option.contains(current, entry.digest)),
        {
            onSome: (entry) => Effect.succeed(entry.projects.map((project): Project => ({ ...project, unread: Option.none() }))),
            onNone: Effect.fnUntraced(function* () {
                const fresh = yield* Effect.forEach(Record.toEntries(Array.groupBy(files, (file) => path.dirname(file))), ([root, matched]) => analyzed(workspaceRoot, root, matched), { concurrency: 'unbounded' });
                const stamp = yield* digest(fresh.flatMap(({ read }) => read));
                const stored = fresh.map(({ unread: _, ...project }) => project);
                yield* Effect.when(
                    Effect.sync(() => {
                        cache.set(key, { projects: stored, digest: stamp });
                        cache.writeToDisk();
                    }),
                    Effect.succeed(fresh.every(({ unread }) => Option.isNone(unread))),
                );
                return fresh;
            }),
        },
    );
    yield* Effect.forEach(Array.getSomes(projects.map(({ files: [file], unread }) => Option.map(unread, ({ message }) => `${file} takes check--compose and install--compose from its folder alone, and their runs report Compose's own failure: ${message}`))), (warning) => Effect.sync(() => logger.warn(warning)));
    reports.set(keyed(workspaceRoot, options), projects);
    return projects;
});

// --- [COMPOSITION] ---------------------------------------------------------------------

const createNodes: CreateNodes = [
    '**/{compose,docker-compose}{,.override}.{yaml,yml}',
    (files, options, context) =>
        graph(context.workspaceRoot, files, options).pipe(
            Effect.map((projects): CreateNodesResultArray => projects.map((project) => [project.files[0], { projects: { [project.root]: { name: project.name, metadata: { technologies: TECHNOLOGIES }, targets: targets(project) } } }])),
            Effect.provide(layer),
            Effect.runPromise,
        ),
];

const createDependencies: CreateDependencies = (options, { workspaceRoot, projects }) => {
    const roots = createProjectRootMappingsFromProjectConfigurations(projects);
    const nodes = Record.fromEntries(Array.getSomes(Object.values(projects).map((data) => Option.map(Option.fromUndefinedOr(data.name), (name): readonly [string, ProjectGraphProjectNode] => [name, { name, type: 'lib', data }]))));
    const produced = Object.values(nodes).flatMap(({ name, data }) =>
        Record.keys(data.targets ?? {}).flatMap((target) =>
            Result.getOrElse(
                Result.try(() => getOutputs(nodes, { project: name, target }, {})),
                () => [],
            )
                .filter((output) => !output.startsWith('!'))
                .map((output) => [name, output] as const),
        ),
    );
    const owners = (at: string) =>
        Array.match(
            produced.filter(([, output]) => matchesGlob(at, output) || matchesGlob(at, `${output}/**`)).map(([name]) => name),
            { onEmpty: () => Option.toArray(Option.fromNullishOr(findProjectForPath(at, roots))), onNonEmpty: Function.identity },
        );
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

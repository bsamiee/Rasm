import { findPackageJSON } from 'node:module';
import { availableParallelism } from 'node:os';
import { isAbsolute, join, posix, relative } from 'node:path';
import process from 'node:process';
import { fileURLToPath, pathToFileURL } from 'node:url';
import {
    type CreateDependencies,
    type CreateDependenciesContext,
    type CreateNodes,
    type CreateNodesContext,
    createNodesFromFiles,
    DependencyType,
    detectPackageManager,
    hashArray,
    normalizePath,
    type ProjectConfiguration,
    type RawProjectGraphDependency,
    readJsonFile,
    type TargetConfiguration,
    type TargetDependencyConfig,
} from '@nx/devkit';
import { combineGlobPatterns, getLockFileName, getReadyCheckReadyWhen, hashWithWorkspaceContext, type PackageJson, PluginCache, quoteShellArg, workspaceDataDirectory } from '@nx/devkit/internal';
import { aggregated, anchored, fingerprinted, manifested, produced, runner, spawned, type Verbs } from '@rasm/nx-distribution/inference';
import distribution from '@rasm/nx-distribution/package.json' with { type: 'json' };
import { Array, Context, Effect, Equal, FileSystem, Function, Layer, MutableRef, Option, Order, Predicate, Record, Result, Schema } from 'effect';
import { Yaml } from 'effect/encoding';
import which from 'which';

// --- [TYPES] ---------------------------------------------------------------------------

type Composed = Effect.Success<ReturnType<typeof analyzed>>;
type Driver = Option.Option.Value<Effect.Success<ReturnType<typeof driven>>>;
type Held = { readonly digest: string } & ({ readonly project: Composed; readonly stamp: string } | { readonly drivers: readonly Driver[] });

// --- [CONSTANTS] -----------------------------------------------------------------------

const PLUGIN = readJsonFile<PackageJson>(fileURLToPath(import.meta.resolve('#/package.json'))).name;
const OWN = fingerprinted(import.meta.dirname);
const EVERY_PROFILE = { COMPOSE_PROFILES: '*' };
const COMPOSE = '**/{compose,docker-compose}{,.override}.{yaml,yml}';
const MANIFEST = 'package.json';
const MISE = '.config/mise.toml';
const ROOT = '.';
const ENGINE: TargetDependencyConfig = { projects: [`directory:${ROOT}`], target: 'install--*' };
const PRODUCERS = ['build--!(compose|buildx)', '^build--*'];
const DRIVEN: Readonly<Record<string, string>> = { testcontainers: 'test--vitest', '@pulumi/docker': 'install--pulumi', [PLUGIN]: 'test--vitest' };

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
const Required = Schema.fromJsonString(
    Schema.Struct({
        dependencies: Schema.Record(Schema.String, Schema.String).pipe(Schema.withDecodingDefaultKey(Effect.succeed({}))),
        devDependencies: Schema.Record(Schema.String, Schema.String).pipe(Schema.withDecodingDefaultKey(Effect.succeed({}))),
    }),
).annotate({ description: 'The dependencies and devDependencies of a package.json, a package requiring a package that drives the engine, itself or through one of its dependencies, running it in the target DRIVEN names' });

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

const served = (root: string, project: Composed, engines: readonly TargetDependencyConfig[], services: Array.NonEmptyReadonlyArray<string>): Verbs => {
    const inputs = consumed(project);
    const up = compose(root, ['up', '--detach', '--wait', '--no-build', ...services], `Starts ${services.join(', ')} of ${project.name} with the services each depends on from built images with docker compose up and follows their logs with docker compose logs, the services staying up after the run`, {
        continuous: true,
        dependsOn: ['build--*', ...engines],
        readyWhen: getReadyCheckReadyWhen(`${PLUGIN}/ready-check`, undefined, ...services),
        inputs,
    });
    return {
        build: { buildx: compose(root, ['build', '--with-dependencies', ...services], `Builds the images of ${services.join(', ')} of ${project.name} and of the services each depends on into the engine with docker compose build`, { dependsOn: [...PRODUCERS, ...engines], inputs }) },
        serve: { compose: { ...up, command: `${up.command} && ${['docker', 'compose', 'logs', '--follow', ...services].map(quoteShellArg).join(' ')}` } },
    };
};

const targets = (root: string, project: Composed, engines: readonly TargetDependencyConfig[]): Verbs => {
    const checked = { cache: true, inputs: [...project.read.map((file) => `{workspaceRoot}/${file}`), { externalDependencies: [] }], options: { env: EVERY_PROFILE } };
    const inputs = consumed(project);
    const profiled = Array.isReadonlyArrayNonEmpty(project.profiles) && { configurations: Record.fromIterableWith(project.profiles, (profile) => [profile, { env: { COMPOSE_PROFILES: profile } }] as const) };
    const built = Option.liftPredicate(project, ({ built: images }) => images);
    return {
        check: {
            compose: compose(root, ['config', '--quiet'], `Validates every Compose service of ${project.name}, interpolation and env files included, with docker compose config`, checked),
            ...Record.getSomes({ buildx: Option.map(built, () => compose(root, ['build', '--check'], `Lints every image ${project.name} builds, failing on any warning, with docker compose build --check`, { ...checked, dependsOn: [...PRODUCERS, ...engines] })) }),
        },
        build: Record.getSomes({ compose: Option.map(built, () => compose(root, ['build'], `Builds the images of ${project.name} into the engine with docker compose build`, { dependsOn: [...PRODUCERS, ...engines], inputs, ...profiled })) }),
        install: { compose: compose(root, ['up', '--detach', '--wait', '--no-build', '--remove-orphans'], `Starts the services of ${project.name} from built images and waits until each is healthy with docker compose up`, { dependsOn: ['build--*', ...engines], inputs, ...profiled }) },
        clean: Record.getSomes({
            compose: Option.map(built, (): TargetConfiguration => ({
                executor: `${distribution.name}:clean`,
                dependsOn: [...engines],
                options: { commands: [['docker', 'compose', 'down', '--rmi', 'local', '--remove-orphans']], cwd: root },
                configurations: { cache: { commands: [['docker', 'builder', 'prune', '--all', '--force']] } },
                inputs,
                metadata: {
                    description: `Stops the services of ${project.name} and removes the images its build wrote with docker compose down --rmi local, keeping volumes, and under configuration cache removes the build cache with docker builder prune`,
                    help: { command: 'docker compose down --help', example: { args: ['--dry-run'] } },
                },
            })),
        }),
    };
};

const inferred = Effect.fnUntraced(function* (cache: PluginCache<Held>, stored: MutableRef.MutableRef<boolean>, context: CreateNodesContext, rooted: (folder: string) => boolean, engines: readonly TargetDependencyConfig[], root: string, matched: Array.NonEmptyReadonlyArray<string>) {
    const stamped = ({ read }: Composed) => Effect.promise(() => hashWithWorkspaceContext(context.workspaceRoot, [...read]));
    const digest = hashArray([yield* Effect.promise(() => hashWithWorkspaceContext(context.workspaceRoot, [...matched])), yield* OWN]);
    const held = yield* Effect.findFirst(
        Option.toArray(
            Option.filter(
                Option.filter(Option.fromUndefinedOr(cache.get(root)), (entry) => entry.digest === digest),
                (entry) => 'project' in entry,
            ),
        ),
        (entry) => Effect.map(stamped(entry.project), Equal.equals(entry.stamp)),
    );
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
        (services) => served(root, project, engines, services),
    );
    const projects = yield* Effect.forEach(Record.toEntries({ ...serving, [root]: Record.union(serving[root] ?? {}, targets(root, project, engines), (_, stated) => stated) }), ([at, tools]) =>
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

const engine = (workspaceRoot: string, file: string, cleaned: boolean) =>
    Effect.map(manifested(workspaceRoot, ROOT), (stated) => {
        const started = ['colima', 'start', '--activate=false', '--ssh-config=false', '--mount', 'none', '--port-forwarder', 'grpc', '--cpus', `${Math.floor(availableParallelism() / 2)}`, '--memory', '8', '--disk', '20'];
        const install: TargetConfiguration = {
            command: `mkdir -p "\${COLIMA_HOME:-$HOME/.colima}" && ${started.map(quoteShellArg).join(' ')}`,
            inputs: [{ externalDependencies: [] }],
            metadata: {
                description:
                    'Starts the Colima VM holding the container engine DOCKER_HOST names on half the cores of this machine with a data disk capped at 20 GiB, in the COLIMA_HOME folder it creates first as Colima reads that variable only once the folder exists, returning once the engine answers and leaving the VM up after the run, with colima start',
                help: { command: 'colima start --help', example: { args: ['--verbose'] } },
            },
        };
        const clean: TargetConfiguration = {
            executor: `${distribution.name}:clean`,
            dependsOn: ['install--colima', ...(cleaned ? [{ projects: '*', target: 'clean--compose', params: 'forward' } satisfies TargetDependencyConfig] : [])],
            options: {
                commands: [
                    ['docker', 'image', 'prune', '--force'],
                    ['docker', 'builder', 'prune', '--force'],
                    ['colima', 'ssh', '--', 'sudo', 'fstrim', '--all', '--verbose'],
                ],
            },
            configurations: { cache: { commands: [['colima', 'delete', '--data', '--force']] } },
            inputs: [{ externalDependencies: [] }],
            metadata: {
                description:
                    'After every Compose project removed its images, removes dangling images and build cache with docker image prune and docker builder prune, then discards the freed blocks of the VM disks with fstrim so their files shrink on this machine, and under configuration cache deletes the VM with its data with colima delete --data',
                help: { command: 'colima delete --help', example: { args: [] } },
            },
        };
        return [file, { projects: { [ROOT]: { root: ROOT, targets: aggregated({ install: { colima: install }, clean: { colima: clean } }, stated, 'colima') } } }] as const;
    });

const driven = Effect.fnUntraced(function* (workspaceRoot: string, file: string) {
    const fs = yield* FileSystem.FileSystem;
    const at = join(workspaceRoot, file);
    const read = (manifest: string) => Effect.flatMap(fs.readFileString(manifest), Schema.decodeEffect(Required));
    const { dependencies, devDependencies } = yield* read(at);
    const named = Record.keys({ ...dependencies, ...devDependencies });
    const located = Array.getSomes(named.map((name) => Option.flatMap(Option.liftThrowable(findPackageJSON)(name, pathToFileURL(at)), Option.fromUndefinedOr)));
    const reached = yield* Effect.forEach(located, (manifest) => Effect.map(read(manifest), (held) => Record.keys(held.dependencies)), { concurrency: 'unbounded' });
    const required = new Set([...named, ...reached.flat()]);
    const root = posix.dirname(file);
    return Option.map(
        Option.liftPredicate(Array.dedupe(Object.values(Record.filter(DRIVEN, (_, driver) => required.has(driver)))), Array.isReadonlyArrayNonEmpty),
        (driving) => [file, { projects: { [root]: { root, targets: Record.fromIterableWith(driving, (target) => [target, { dependsOn: ['...', ENGINE] }]) } } }] as const,
    );
});

const required = Effect.fnUntraced(function* (cache: PluginCache<Held>, stored: MutableRef.MutableRef<boolean>, workspaceRoot: string, manifests: readonly string[]) {
    const digest = hashArray([yield* Effect.promise(() => hashWithWorkspaceContext(workspaceRoot, [...manifests, getLockFileName(detectPackageManager(workspaceRoot))])), yield* OWN]);
    const held = Option.filter(
        Option.filter(Option.fromUndefinedOr(cache.get(MANIFEST)), (entry) => entry.digest === digest),
        (entry) => 'drivers' in entry,
    );
    const drivers = yield* Option.match(held, {
        onSome: (entry) => Effect.succeed(entry.drivers),
        onNone: () =>
            Effect.map(
                Effect.forEach(manifests, (file) => driven(workspaceRoot, file), { concurrency: 'unbounded' }),
                Array.getSomes,
            ),
    });
    yield* Option.isNone(held)
        ? Effect.sync(() => {
              cache.set(MANIFEST, { digest, drivers });
              MutableRef.set(stored, true);
          })
        : Effect.void;
    return drivers;
});

const edges = (analyses: ReadonlyMap<string, Composed>, { projects, workspaceRoot }: CreateDependenciesContext) => {
    const built = Record.collect(projects, (name, { root }) => Array.fromNullishOr(analyses.get(join(workspaceRoot, root))).flatMap(({ file, contexts }) => contexts.map((at) => ({ name, file, at })))).flat();
    const made: Readonly<Record<string, readonly string[]>> = Array.match(
        built.map(({ at }) => at),
        { onEmpty: () => ({}), onNonEmpty: (contexts) => produced(projects, contexts) },
    );
    return built.flatMap(({ name, file, at }) => (made[at] ?? []).map((target): RawProjectGraphDependency => ({ source: name, target, type: DependencyType.static, sourceFile: file })));
};

// --- [COMPOSITION] ---------------------------------------------------------------------

const run = runner(Analyses.layer);

const createNodes: CreateNodes = [
    combineGlobPatterns([COMPOSE, `**/${MANIFEST}`, MISE]),
    (files, options, context) => {
        const rooted = anchored(context.workspaceRoot);
        const manifests = files.filter((file) => posix.basename(file) === MANIFEST && posix.dirname(file) !== ROOT && rooted(posix.dirname(file)));
        const folders = new Map(
            Record.collect(
                Record.filter(
                    Array.groupBy(
                        files.filter((file) => posix.matchesGlob(file, COMPOSE)),
                        posix.dirname,
                    ),
                    (_, root) => rooted(root),
                ),
                (root, matched) => [Array.min(matched, Order.String), { root, matched }] as const,
            ),
        );
        const cache = new PluginCache<Held>(join(workspaceDataDirectory, 'docker.hash'));
        const stored = MutableRef.make(false);
        return run(
            Effect.ensuring(
                Effect.gen(function* () {
                    const hosting = yield* Effect.filter(
                        files.filter((file) => file === MISE && process.platform !== 'linux'),
                        () =>
                            Effect.map(
                                Effect.promise(() => which('colima', { nothrow: true })),
                                Predicate.isNotNull,
                            ),
                    );
                    const engines = Array.match(hosting, { onEmpty: () => [], onNonEmpty: () => [ENGINE] as const });
                    const read = (file: string) => run(Effect.flatMap(Effect.fromNullishOr(folders.get(file)), ({ root, matched }) => inferred(cache, stored, context, rooted, engines, root, matched)));
                    const [composed, drivers] = yield* Effect.all([Effect.promise(() => createNodesFromFiles(read, folders.keys().toArray(), options, context)), Array.isReadonlyArrayNonEmpty(engines) ? required(cache, stored, context.workspaceRoot, manifests) : Effect.succeed(Array.empty<Driver>())]);
                    const analyses = yield* Analyses;
                    const cleaned = folders.values().some(({ root }) => analyses.get(join(context.workspaceRoot, root))?.built === true);
                    const started = yield* Effect.forEach(hosting, (file) => engine(context.workspaceRoot, file, cleaned));
                    return [...composed, ...started, ...drivers];
                }),
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

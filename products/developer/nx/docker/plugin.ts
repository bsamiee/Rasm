import { findPackageJSON } from 'node:module';
import { isAbsolute, join, posix, relative, resolve } from 'node:path';
import process from 'node:process';
import { fileURLToPath, pathToFileURL } from 'node:url';
import {
    type CreateDependencies,
    type CreateDependenciesContext,
    type CreateNodes,
    type CreateNodesResultArray,
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
import { combineGlobPatterns, getLockFileName, getReadyCheckReadyWhen, type PackageJson, PluginCache, quoteShellArg, workspaceDataDirectory } from '@nx/devkit/internal';
import { aggregated, anchored, fingerprinted, manifested, produced, runner, spawned, stamped, type Verbs } from '@rasm/nx-distribution/inference';
import distribution from '@rasm/nx-distribution/package.json' with { type: 'json' };
import { Array, Context, Effect, Equal, FileSystem, Layer, MutableRef, Option, Order, type PlatformError, Predicate, Record, Schema } from 'effect';
import { Yaml } from 'effect/encoding';

// --- [TYPES] ---------------------------------------------------------------------------

type Composed = Effect.Success<ReturnType<typeof analyzed>>;
type Kept = Composed | { readonly sources: readonly string[]; readonly drivers: CreateNodesResultArray };
type Held = Kept & { readonly digest: string; readonly stamp: string };
interface Pass {
    readonly workspaceRoot: string;
    readonly cache: PluginCache<Held>;
    readonly stored: MutableRef.MutableRef<boolean>;
    readonly rooted: (folder: string) => boolean;
    readonly engines: readonly TargetDependencyConfig[];
}

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

const recall = Effect.fnUntraced(function* <A extends Kept, E, R>(pass: Pass, key: string, identity: readonly string[], kind: (entry: Held) => entry is Held & A, made: Effect.Effect<A, E, R>) {
    const stamp = stamped(pass.workspaceRoot);
    const digest = hashArray([yield* OWN, ...identity]);
    const held = yield* Effect.findFirst(
        Array.fromNullishOr(pass.cache.get(key)).filter((entry) => entry.digest === digest),
        (entry) => Effect.map(stamp(entry.sources), Equal.equals(entry.stamp)),
    );
    return yield* Option.match(Option.filter(held, kind), {
        onSome: Effect.succeed,
        onNone: Effect.fnUntraced(function* () {
            const kept = yield* made;
            pass.cache.set(key, { ...kept, digest, stamp: yield* stamp(kept.sources) });
            MutableRef.set(pass.stored, true);
            return kept;
        }),
    });
});

const merged = Effect.fnUntraced(function* (workspaceRoot: string, file: string, seen: ReadonlySet<string>): Effect.fn.Return<ReadonlySet<string>, PlatformError.PlatformError, FileSystem.FileSystem> {
    const text = yield* FileSystem.FileSystem.use((fs) => fs.readFileString(join(workspaceRoot, file))).pipe(Effect.catchReason('PlatformError', 'NotFound', () => Effect.succeed('')));
    const rows = Option.toArray(Option.flatMap(Option.liftThrowable(Yaml.parse)(text), Schema.decodeUnknownOption(Including))).flatMap(({ include, services }) => [
        ...include.flatMap((row) => Array.ensure(Predicate.isString(row) ? row : row.path)),
        ...Object.values(services ?? {}).flatMap((service) => (Predicate.isObject(service.extends) ? Array.fromNullishOr(service.extends.file) : [])),
    ]);
    return yield* Effect.reduce(
        rows.map((to) => posix.join(posix.dirname(file), to)),
        (): ReadonlySet<string> => new Set([...seen, file]),
        (held, to) => (held.has(to) ? Effect.succeed(held) : merged(workspaceRoot, to, held)),
    );
});

const analyzed = Effect.fnUntraced(function* (workspaceRoot: string, root: string, files: Array.NonEmptyReadonlyArray<string>) {
    const report = yield* Effect.flatMap(spawned(['docker', 'compose', 'config', '--format', 'json', '--no-interpolate'], { cwd: join(workspaceRoot, root), env: EVERY_PROFILE }), Schema.decodeEffect(Report));
    const loaded = yield* Effect.reduce(
        files,
        (): ReadonlySet<string> => new Set(),
        (held, file) => merged(workspaceRoot, file, held),
    );
    const local = (at: string, ...inside: readonly string[]) =>
        Option.map(
            Option.liftPredicate(relative(workspaceRoot, resolve(at, ...inside)), (to) => isAbsolute(at) && !(to.startsWith('..') || isAbsolute(to))),
            normalizePath,
        );
    const services = Object.values(report.services);
    const builds = Array.getSomes(services.map(({ build }) => Option.fromUndefinedOr(build)));
    const recipes = Record.collect(report.services, (service, { build }) => (build?.dockerfile === undefined ? [] : Option.toArray(Option.map(local(build.context, build.dockerfile), (recipe) => ({ service, recipe }))))).flat();
    return {
        name: report.name,
        file: Array.min(files, Order.String),
        sources: Array.sort(Array.dedupe([...[...loaded].filter((file) => !file.startsWith('..')), posix.join(root, '.env'), ...services.flatMap(({ env_file = [] }) => Array.getSomes(env_file.map(({ path }) => local(path)))), ...recipes.map(({ recipe }) => recipe)]), Order.String),
        profiles: Array.sort(Array.dedupe(services.flatMap(({ profiles = [] }) => profiles)), Order.String),
        built: builds.length > 0,
        servers: Record.map(
            Array.groupBy(recipes, ({ recipe }) => posix.dirname(recipe)),
            Array.map(({ service }) => service),
        ),
        contexts: Array.dedupe(builds.flatMap(({ context, additional_contexts = {} }) => Array.getSomes([context, ...Object.values(additional_contexts)].map((at) => local(at))))),
    };
});

const compose = (root: string, args: Array.NonEmptyReadonlyArray<string>, description: string, extra: Partial<TargetConfiguration<Readonly<Record<string, unknown>>>>): TargetConfiguration => ({
    ...extra,
    command: ['docker', 'compose', ...args].map(quoteShellArg).join(' '),
    options: { cwd: root, ...extra.options },
    metadata: { description, help: { command: `docker compose ${args[0]} --help`, example: { args: ['--dry-run'] } } },
});

const inferred = Effect.fnUntraced(function* (pass: Pass, root: string, matched: Array.NonEmptyReadonlyArray<string>) {
    const project = yield* recall(pass, root, matched, (entry) => 'name' in entry, analyzed(pass.workspaceRoot, root, matched));
    (yield* Analyses).set(join(pass.workspaceRoot, root), project);
    const read = [...project.sources.map((file) => `{workspaceRoot}/${file}`), { externalDependencies: [] }];
    const checked = { cache: true, inputs: read, options: { env: EVERY_PROFILE } };
    const built = { dependsOn: [...PRODUCERS, ...pass.engines], inputs: ['default', '^default', ...read] };
    const started = { ...built, dependsOn: ['build--*', ...pass.engines] };
    const profiled = Array.isReadonlyArrayNonEmpty(project.profiles) && { configurations: Record.fromIterableWith(project.profiles, (profile) => [profile, { env: { COMPOSE_PROFILES: profile } }] as const) };
    const serving = Record.map(
        Record.filter(project.servers, (_, at) => pass.rooted(at)),
        (services): Verbs => {
            const up = compose(root, ['up', '--detach', '--wait', '--no-build', ...services], `Starts ${services.join(', ')} of ${project.name} with the services each depends on from built images with docker compose up and follows their logs with docker compose logs, the services staying up after the run`, {
                ...started,
                continuous: true,
                readyWhen: getReadyCheckReadyWhen(`${PLUGIN}/ready-check`, undefined, ...services),
            });
            return {
                build: { buildx: compose(root, ['build', '--with-dependencies', ...services], `Builds the images of ${services.join(', ')} of ${project.name} and of the services each depends on into the engine with docker compose build`, built) },
                serve: { compose: { ...up, command: `${up.command} && ${['docker', 'compose', 'logs', '--follow', ...services].map(quoteShellArg).join(' ')}` } },
            };
        },
    );
    const verbs: Readonly<Record<string, Verbs>> = {
        ...serving,
        [root]: {
            ...serving[root],
            check: {
                compose: compose(root, ['config', '--quiet'], `Validates every Compose service of ${project.name}, interpolation and env files included, with docker compose config`, checked),
                ...(project.built && { buildx: compose(root, ['build', '--check'], `Lints every image ${project.name} builds, failing on any warning, with docker compose build --check`, { ...checked, dependsOn: built.dependsOn }) }),
            },
            install: { compose: compose(root, ['up', '--detach', '--wait', '--no-build', '--remove-orphans'], `Starts the services of ${project.name} from built images and waits until each is healthy with docker compose up`, { ...started, ...profiled }) },
            ...(project.built && {
                build: { compose: compose(root, ['build'], `Builds the images of ${project.name} into the engine with docker compose build`, { ...built, ...profiled }) },
                clean: {
                    compose: {
                        executor: `${distribution.name}:clean`,
                        dependsOn: [...pass.engines],
                        options: { commands: [['docker', 'compose', 'down', '--rmi', 'local', '--remove-orphans']], cwd: root },
                        configurations: { cache: { commands: [['docker', 'builder', 'prune', '--all', '--force']] } },
                        inputs: built.inputs,
                        metadata: {
                            description: `Stops the services of ${project.name} and removes the images its build wrote with docker compose down --rmi local, keeping volumes, and under configuration cache removes the build cache with docker builder prune`,
                            help: { command: 'docker compose down --help', example: { args: ['--dry-run'] } },
                        },
                    },
                },
            }),
        },
    };
    return { projects: yield* Effect.all(Record.map(verbs, (tools, at) => Effect.map(manifested(pass.workspaceRoot, at), (stated): ProjectConfiguration => ({ root: at, metadata: { technologies: ['compose'] }, targets: aggregated(tools, stated, 'compose') })))) };
});

const engine = (workspaceRoot: string, file: string, cleaned: boolean) =>
    Effect.map(manifested(workspaceRoot, ROOT), (stated) => {
        const started = ['colima', 'start', '--activate=false', '--ssh-config=false', '--mount', 'none', '--port-forwarder', 'grpc', '--memory', '8', '--disk', '20'];
        const install: TargetConfiguration = {
            command: `mkdir -p "\${COLIMA_HOME:-$HOME/.colima}" && ${started.map(quoteShellArg).join(' ')} --cpus "$(($(getconf _NPROCESSORS_ONLN) / 2))"`,
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

const required = (pass: Pass, files: readonly string[]) => {
    const manifests = files.filter((file) => posix.basename(file) === MANIFEST && posix.dirname(file) !== ROOT && pass.rooted(posix.dirname(file)));
    const read = (manifest: string) => FileSystem.FileSystem.use((fs) => fs.readFileString(resolve(pass.workspaceRoot, manifest))).pipe(Effect.flatMap(Schema.decodeEffect(Required)));
    const driving = Effect.forEach(
        manifests,
        Effect.fnUntraced(function* (file) {
            const { dependencies, devDependencies } = yield* read(file);
            const named = Record.keys({ ...dependencies, ...devDependencies });
            const located = Array.getSomes(named.map((name) => Option.flatMap(Option.liftThrowable(findPackageJSON)(name, pathToFileURL(join(pass.workspaceRoot, file))), Option.fromUndefinedOr)));
            const reached = yield* Effect.forEach(located, read, { concurrency: 'unbounded' });
            const requires = new Set([...named, ...reached.flatMap((held) => Record.keys(held.dependencies))]);
            const root = posix.dirname(file);
            return Option.map(
                Option.liftPredicate(Array.dedupe(Object.values(Record.filter(DRIVEN, (_, name) => requires.has(name)))), Array.isReadonlyArrayNonEmpty),
                (targets) => [file, { projects: { [root]: { root, targets: Record.fromIterableWith(targets, (target) => [target, { dependsOn: ['...', ENGINE] }]) } } }] as const,
            );
        }),
        { concurrency: 'unbounded' },
    );
    return Effect.map(
        recall(
            pass,
            MANIFEST,
            manifests,
            (entry) => 'drivers' in entry,
            Effect.map(driving, (drivers): Extract<Kept, { readonly drivers: unknown }> => ({ sources: [...manifests, getLockFileName(detectPackageManager(pass.workspaceRoot))], drivers: Array.getSomes(drivers) })),
        ),
        ({ drivers }) => drivers,
    );
};

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
        const cache = new PluginCache<Held>(join(workspaceDataDirectory, 'docker.hash'));
        const stored = MutableRef.make(false);
        return run(
            Effect.ensuring(
                Effect.gen(function* () {
                    const rooted = anchored(context.workspaceRoot);
                    const hosting = files.filter((file) => file === MISE && process.platform === 'darwin');
                    const pass: Pass = { workspaceRoot: context.workspaceRoot, cache, stored, rooted, engines: Array.match(hosting, { onEmpty: () => [], onNonEmpty: () => [ENGINE] }) };
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
                    const read = (file: string) => run(Effect.flatMap(Effect.fromNullishOr(folders.get(file)), ({ root, matched }) => inferred(pass, root, matched)));
                    const [composed, drivers] = yield* Effect.all([Effect.promise(() => createNodesFromFiles(read, folders.keys().toArray(), options, context)), Array.isReadonlyArrayNonEmpty(pass.engines) ? required(pass, files) : Effect.succeed([])]);
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

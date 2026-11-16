import { once } from 'node:events';
import { existsSync } from 'node:fs';
import { findPackageJSON } from 'node:module';
import { dirname, join, relative, sep } from 'node:path';
import { fileURLToPath, pathToFileURL } from 'node:url';
import { Worker } from 'node:worker_threads';
import { layer } from '@effect/platform-node/NodeServices';
import { type CreateNodes, type CreateNodesContext, createNodesFromFiles, type CreateNodesResult, detectPackageManager, getPackageManagerCommand, hashArray, normalizePath, type ProjectConfiguration, readJsonFile, type TargetConfiguration } from '@nx/devkit';
import {
    buildPackageJsonPatterns,
    buildPackageJsonWorkspacesMatcher,
    calculateHashesForCreateNodes,
    getLockFileName,
    getNamedInputs,
    getReadyCheckReadyWhen,
    hashFile,
    hashObject,
    hashWithWorkspaceContext,
    mergeTargetConfigurations,
    type PackageJson,
    PluginCache,
    quoteShellArg,
    workspaceDataDirectory,
} from '@nx/devkit/internal';
import { getRootTsConfigFileName } from '@nx/js';
import { walkTsconfigExtendsChain } from '@nx/js/internal';
import web from '@nx/web/package.json' with { type: 'json' };
import distribution from '@rasm/nx-distribution/package.json' with { type: 'json' };
import { Array, Cause, Effect, Equal, FileSystem, flow, identity, MutableRef, Option, Predicate, pipe, Record, Result, Schema } from 'effect';

// --- [TYPES] ---------------------------------------------------------------------------

type Resolved = Effect.Success<ReturnType<typeof read>>;
type Verbs = Readonly<Record<string, Readonly<Record<string, TargetConfiguration>>>>;

// --- [CONSTANTS] -----------------------------------------------------------------------

const WORKER = fileURLToPath(import.meta.resolve('#/worker'));
const CHECK = normalizePath(fileURLToPath(import.meta.resolve('@nx/storybook/ready-check')));
const RUNNER = '@storybook/test-runner';
const OWN = pipe(
    Effect.flatMap(FileSystem.FileSystem, (fs) => fs.readDirectory(import.meta.dirname)),
    Effect.map((names) => hashArray(names.toSorted().flatMap((name) => [name, ...Array.fromNullishOr(hashFile(join(import.meta.dirname, name)))]))),
    Effect.cached,
    Effect.runSync,
);

// --- [MODELS] --------------------------------------------------------------------------

const Options = Schema.Struct({ outputPath: Schema.optionalKey(Schema.String) }).annotate({ description: 'outputPath: workspace folder storybook build writes to, {projectName} naming the project, absent writes storybook-static in the project' });
const Input = Schema.declare((input: unknown): input is NonNullable<TargetConfiguration['inputs']>[number] => Predicate.isString(input) || Predicate.isObject(input));
const Overrides = Schema.fromJsonString(Schema.Struct({ nx: Schema.optionalKey(Schema.Struct({ targets: Schema.optionalKey(Schema.Record(Schema.String, Schema.Struct({ inputs: Schema.optionalKey(Schema.mutable(Schema.Array(Input))) }))) })) })).annotate({
    description: "The nx.targets rows of a project's package.json, whose inputs Nx merges over the inferred targets after every plugin",
});
const Entry = Schema.Union([Schema.String, Schema.Struct({ name: Schema.String })]);
const Main = Schema.Struct({ framework: Schema.optionalKey(Entry), addons: Schema.optionalKey(Schema.Array(Entry)), stories: Schema.Array(Schema.Struct({ directory: Schema.String, files: Schema.String })) }).annotate({
    description: 'The framework and addons of a Storybook main config, each a package specifier or path, alone or as the name of an object holding its options, and each stories folder relative to the workspace root with its file glob',
});
const Loaded = Schema.Struct({ main: Schema.fromJsonString(Main), loaded: Schema.Array(Schema.String) }).annotate({ description: 'The framework, addons, and stories the worker read from a Storybook main config, and the URL of every module its load resolved' });
const Named = Schema.fromJsonString(Schema.Struct({ name: Schema.String })).annotate({ description: 'The name field of a package.json' });

// --- [OPERATIONS] ----------------------------------------------------------------------

const run = <A, E>(effect: Effect.Effect<A, E, FileSystem.FileSystem>) => Effect.runPromise(Effect.provide(effect, layer));

const manifested = Effect.fnUntraced(function* (workspaceRoot: string, root: string) {
    const fs = yield* FileSystem.FileSystem;
    const manifest = join(workspaceRoot, root, 'package.json');
    return (yield* fs.exists(manifest)) ? ((yield* Effect.flatMap(fs.readFileString(manifest), Schema.decodeEffect(Overrides))).nx?.targets ?? {}) : {};
});

const aggregated = (verbs: Verbs, stated: Readonly<Record<string, Pick<TargetConfiguration, 'inputs'>>>, technology: string): Record<string, TargetConfiguration> =>
    Record.fromEntries(
        Record.collect(Record.filter(verbs, Predicate.not(Record.isEmptyRecord)), (verb, tools): readonly (readonly [string, TargetConfiguration])[] => [
            [
                verb,
                {
                    dependsOn: [`${verb}--*`],
                    inputs: [
                        '...',
                        ...Array.dedupe(
                            Record.collect(tools, (tool, target) => mergeTargetConfigurations(stated[`${verb}--${tool}`] ?? {}, { ...target, inputs: target.inputs ?? ['default', '^default'] }).inputs ?? [])
                                .flat()
                                .map((input) => (Predicate.isObject(input) && 'dependentTasksOutputFiles' in input ? { ...input, transitive: true } : input)),
                        ),
                    ],
                },
            ],
            ...Record.collect(tools, (tool, target): readonly [string, TargetConfiguration] => [`${verb}--${tool}`, { ...target, metadata: { technologies: [technology], ...target.metadata } }]),
        ]).flat(),
    );

const extended = (workspaceRoot: string, root: string): readonly string[] => {
    const files: string[] = [];
    const above = Array.unfold(
        Option.some(root),
        Option.map((at: string) => [join(workspaceRoot, at, 'tsconfig.json'), Option.liftPredicate(dirname(at), () => at !== '.')] as const),
    );
    Option.toArray(Array.findFirst(above, existsSync)).forEach((config) =>
        walkTsconfigExtendsChain(config, (at) => {
            files.push(at);
            return 'continue';
        }),
    );
    return files.map((at) => normalizePath(relative(workspaceRoot, at))).filter((at) => !(at.startsWith('..') || at.split('/').includes('node_modules')));
};

const stamped = (workspaceRoot: string, sources: readonly string[]) => Effect.promise(() => hashWithWorkspaceContext(workspaceRoot, [...sources]));

const read = Effect.fnUntraced(function* (workspaceRoot: string, file: string) {
    const fs = yield* FileSystem.FileSystem;
    const [root, main] = [dirname(dirname(file)), join(workspaceRoot, file)];
    const failed = (cause: unknown) => new Cause.UnknownError(cause, `${file} does not load as a Storybook main config`);
    const listened = Effect.acquireUseRelease(
        Effect.sync(() => new Worker(WORKER, { workerData: [main, workspaceRoot] })),
        (worker) => Effect.raceFirst(Effect.tryPromise({ try: (signal) => once(worker, 'message', { signal }), catch: failed }), Effect.flatMap(Effect.tryPromise({ try: (signal) => once(worker, 'exit', { signal }), catch: failed }), flow(failed, Effect.fail))),
        (worker) => Effect.promise(() => worker.terminate()),
    );
    const {
        main: { framework, addons = [], stories },
        loaded,
    } = yield* Effect.flatMap(listened, ([message]) => Schema.decodeUnknownEffect(Loaded)(message));
    const sources = Array.dedupe(loaded.filter((url) => url.startsWith('file:')).map((url) => normalizePath(relative(workspaceRoot, fileURLToPath(url))))).filter((at) => !(at.startsWith('..') || at.startsWith(`${root}/`) || at.split('/').includes('node_modules')));
    const specifiers = Array.dedupe([...Array.fromNullishOr(framework), ...addons].map((entry) => (Predicate.isString(entry) ? entry : entry.name)));
    const found = (specifier: string) => Option.flatMap(Option.liftThrowable(findPackageJSON)(specifier, pathToFileURL(main)), Option.fromUndefinedOr);
    const manifests = yield* Effect.forEach(Array.getSomes(specifiers.map(found)), (at) => fs.realPath(at));
    const named = yield* Effect.forEach(manifests, (at) => Effect.map(Effect.flatMap(fs.readFileString(at), Schema.decodeEffect(Named)), ({ name }) => (at.split(sep).includes('node_modules') ? Result.succeed(name) : Result.fail(name))));
    const [packages, linked] = Array.partition(named, identity);
    const globs = Array.dedupe(stories.map(({ directory, files }) => normalizePath(join(directory, files)))).filter((at) => !at.startsWith('..'));
    return { packages, linked, sources, globs, runner: Option.isSome(found(RUNNER)), stamp: yield* stamped(workspaceRoot, sources) };
});

const targets = (root: string, { packages, linked, sources, globs, runner }: Resolved, chain: readonly string[], production: boolean, output: Option.Option<string>, exec: string): Verbs => {
    const rooted = getRootTsConfigFileName();
    const inputs = [
        ...(production ? ['production', '{workspaceRoot}/{projectRoot}/.storybook/**/*', ...globs.map((at) => join('{workspaceRoot}', at)), '^production'] : ['default', '^default']),
        ...chain.filter((at) => at !== rooted).map((at) => ({ json: join('{workspaceRoot}', at), fields: ['compilerOptions'] })),
        ...sources.map((at) => join('{workspaceRoot}', at)),
        ...Array.match(linked, {
            onEmpty: () => [],
            onNonEmpty: (owners) => [
                { input: 'default', projects: [...owners] },
                { input: 'default', projects: [...owners], dependencies: true as const },
            ],
        }),
        { externalDependencies: Array.dedupe(['storybook', ...packages]) },
    ];
    const written = Option.getOrElse(output, () => join(root, 'storybook-static'));
    const needs = { options: { cwd: root }, dependsOn: ['codegen--*', '^codegen'], inputs };
    const build = {
        storybook: {
            command: ['storybook', 'build', ...Option.match(output, { onNone: () => [], onSome: (at) => ['--output-dir', relative(root, at)] })].map(quoteShellArg).join(' '),
            ...needs,
            cache: true,
            outputs: [...Option.match(output, { onNone: () => ['{projectRoot}/storybook-static'], onSome: (at) => [relative(root, at).startsWith('..') ? join('{workspaceRoot}', at) : join('{projectRoot}', relative(root, at))] }), ...['output-dir', 'outputDir', 'o'].map((option) => `{projectRoot}/{options.${option}}`)],
            metadata: { description: 'Builds the static Storybook of the project stories with storybook build', help: { command: `${exec} storybook build --help`, example: { args: ['--test'] } } },
        },
    } satisfies Record<string, TargetConfiguration>;
    const removed = Record.collect(build, (tool) => `build--${tool}`);
    return {
        build,
        preview: {
            storybook: {
                executor: `${web.name}:file-server`,
                dependsOn: removed,
                continuous: true,
                readyWhen: { signal: true },
                inputs,
                options: { buildTarget: 'build--storybook', staticFilePath: written },
                metadata: { description: `Serves the static Storybook build--storybook writes with ${web.name}:file-server`, help: { command: `${exec} nx run --help`, example: { options: { port: 4400 } } } },
            },
        },
        serve: {
            storybook: {
                command: 'storybook dev',
                ...needs,
                continuous: true,
                readyWhen: getReadyCheckReadyWhen(CHECK),
                metadata: { description: 'Serves the project stories with storybook dev', help: { command: `${exec} storybook dev --help`, example: { args: ['--port=<port>'] } } },
            },
        },
        test: runner
            ? {
                  storybook: {
                      command: 'test-storybook',
                      options: { cwd: root },
                      dependsOn: [{ target: 'serve--storybook', waitFor: 'ready' }],
                      inputs: [{ externalDependencies: ['storybook', RUNNER] }],
                      metadata: { description: 'Runs the stories of the Storybook serve--storybook serves as tests with test-storybook', help: { command: `${exec} test-storybook --help`, example: { args: ['--url', 'http://localhost:6006'] } } },
                  },
              }
            : {},
        clean: {
            storybook: {
                executor: `${distribution.name}:clean`,
                options: { targets: removed },
                inputs: ['default', '^default', { externalDependencies: [] }],
                metadata: { description: `Removes the outputs ${removed.join(', ')} declare`, help: { command: `${exec} nx run --help`, example: { options: { targets: removed } } } },
            },
        },
    };
};

const inferred = Effect.fnUntraced(function* (cache: PluginCache<Resolved>, stored: MutableRef.MutableRef<boolean>, context: CreateNodesContext, output: Option.Option<string>, exec: string, { file, chain, key }: { readonly file: string; readonly chain: readonly string[]; readonly key: string }) {
    const root = dirname(dirname(file));
    const held = yield* Effect.findFirst(Array.fromNullishOr(cache.get(key)), ({ sources, stamp }) => Effect.map(stamped(context.workspaceRoot, sources), Equal.equals(stamp)));
    const resolved = yield* Option.match(held, {
        onSome: Effect.succeed,
        onNone: () =>
            Effect.tap(read(context.workspaceRoot, file), (fresh) =>
                Effect.sync(() => {
                    cache.set(key, fresh);
                    MutableRef.set(stored, true);
                }),
            ),
    });
    const stated = yield* manifested(context.workspaceRoot, root);
    const outside = chain.filter((config) => !config.startsWith(`${root}/`));
    const project: ProjectConfiguration = { root, metadata: { technologies: ['storybook'] }, targets: aggregated(targets(root, resolved, outside, 'production' in getNamedInputs(root, context), output, exec), stated, 'storybook') };
    return { projects: { [root]: project } } satisfies CreateNodesResult;
});

const nodes = Effect.fnUntraced(function* (files: readonly string[], options: typeof Options.Encoded | undefined, context: CreateNodesContext) {
    const settings = yield* Schema.decodeEffect(Options)(options ?? {});
    const member = buildPackageJsonWorkspacesMatcher(buildPackageJsonPatterns(context.workspaceRoot, (file) => readJsonFile<PackageJson>(join(context.workspaceRoot, file))));
    const [mains, roots] = Array.unzip(files.map((file) => [file, dirname(dirname(file))] as const).filter(([, root]) => existsSync(join(context.workspaceRoot, root, 'project.json')) || (member(join(root, 'package.json')) && existsSync(join(context.workspaceRoot, root, 'package.json')))));
    const chains = roots.map((root) => extended(context.workspaceRoot, root));
    const manager = detectPackageManager(context.workspaceRoot);
    const { exec } = getPackageManagerCommand(manager);
    const own = yield* OWN;
    const hashes = yield* Effect.promise(() =>
        calculateHashesForCreateNodes(
            roots,
            settings,
            context,
            chains.map((chain) => [getLockFileName(manager), ...chain]),
        ),
    );
    const cache = new PluginCache<Resolved>(join(workspaceDataDirectory, `storybook-${hashObject(options ?? {})}.hash`));
    const stored = MutableRef.make(false);
    const rows = new Map(Array.zipWith(Array.zip(mains, chains), hashes, ([file, chain], hash) => [file, { file, chain, key: hashArray([hash, own, file]) }] as const));
    const each = (file: string) => run(Effect.flatMap(Effect.fromNullishOr(rows.get(file)), (row) => inferred(cache, stored, context, Option.fromUndefinedOr(settings.outputPath), exec, row)));
    return yield* Effect.ensuring(
        Effect.promise(() => createNodesFromFiles(each, mains, options, context)),
        Effect.when(
            Effect.sync(() => cache.writeToDisk()),
            Effect.sync(() => MutableRef.get(stored)),
        ),
    );
});

// --- [COMPOSITION] ---------------------------------------------------------------------

const createNodes: CreateNodes<typeof Options.Encoded> = ['**/.storybook/main.{js,ts,jsx,tsx,mjs,mts,cjs,cts}', (files, options, context) => run(nodes(files, options, context))];

// --- [EXPORTS] -------------------------------------------------------------------------

export { createNodes };

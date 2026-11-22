import { once } from 'node:events';
import { findPackageJSON } from 'node:module';
import { dirname, join, posix, relative } from 'node:path';
import { fileURLToPath, pathToFileURL } from 'node:url';
import { Worker } from 'node:worker_threads';
import { type CreateNodes, type CreateNodesContext, createNodesFromFiles, type CreateNodesResult, detectPackageManager, getPackageManagerCommand, hashArray, normalizePath, type ProjectConfiguration, type TargetConfiguration } from '@nx/devkit';
import { calculateHashesForCreateNodes, getLockFileName, getReadyCheckReadyWhen, PluginCache, quoteShellArg, workspaceDataDirectory } from '@nx/devkit/internal';
import { getRootTsConfigFileName } from '@nx/js';
import web from '@nx/web/package.json' with { type: 'json' };
import { aggregated, anchored, extended, fingerprinted, Folder, manifested, namedInput, packaged, recalled, run, stamped, tokened, type Verbs, within } from '@rasm/nx-distribution/inference';
import distribution from '@rasm/nx-distribution/package.json' with { type: 'json' };
import { Array, Cause, Effect, flow, MutableRef, Option, Predicate, Record, Schema } from 'effect';

// --- [TYPES] ---------------------------------------------------------------------------

type Resolved = Effect.Success<ReturnType<typeof read>>;

// --- [CONSTANTS] -----------------------------------------------------------------------

const WORKER = fileURLToPath(import.meta.resolve('#/worker.ts'));
const RUNNER = '@storybook/test-runner';
const OWN = fingerprinted(import.meta.dirname);

// --- [MODELS] --------------------------------------------------------------------------

const Options = Schema.Struct({ outputPath: Folder }).annotate({ description: 'outputPath: workspace folder storybook build writes to, {projectName} naming the project, absent writes storybook-static in the project' });
const Entry = Schema.Union([Schema.String, Schema.Struct({ name: Schema.String })]);
const Main = Schema.Struct({ framework: Schema.optionalKey(Entry), addons: Schema.optionalKey(Schema.Array(Entry)), stories: Schema.Array(Schema.Struct({ directory: Schema.String, files: Schema.String })) }).annotate({
    description: 'The framework and addons of a Storybook main config, each a package specifier or path, alone or as the name of an object holding its options, and each stories folder relative to the workspace root with its file glob',
});
const Loaded = Schema.Struct({ main: Schema.fromJsonString(Main), loaded: Schema.Array(Schema.Tuple([Schema.String, Schema.String, Schema.String])) }).annotate({
    description: 'The framework, addons, and stories the worker read from a Storybook main config, and the resolved URL, specifier, and importing module URL of every import its load resolved',
});

// --- [OPERATIONS] ----------------------------------------------------------------------

const read = Effect.fnUntraced(function* (workspaceRoot: string, file: string) {
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
    const local = (url: string) => (url.startsWith('file:') ? Option.some(normalizePath(relative(workspaceRoot, fileURLToPath(url)))) : Option.none());
    const sources = Array.dedupe(Array.getSomes(loaded.map(([url]) => local(url)))).filter((at) => within(at) && !at.startsWith(`${root}/`));
    const specifiers = [...Array.fromNullishOr(framework), ...addons].map((entry): readonly [string, string] => [Predicate.isString(entry) ? entry : entry.name, main]);
    const imported = Array.getSomes(loaded.map(([, specifier, parent]) => Option.as(Option.filter(local(parent), within), [specifier, parent] as const)));
    const { packages, linked } = yield* packaged([...specifiers, ...imported]);
    const globs = Array.dedupe(stories.map(({ directory, files }) => normalizePath(join(directory, files)))).filter((at) => !at.startsWith('..'));
    return { packages, linked, sources, globs, runner: Option.isSome(Option.flatMap(Option.liftThrowable(findPackageJSON)(RUNNER, pathToFileURL(main)), Option.fromUndefinedOr)), stamp: yield* stamped(workspaceRoot)(sources) };
});

const targets = (root: string, { packages, linked, sources, globs, runner }: Resolved, chain: readonly string[], production: boolean, output: Option.Option<string>, exec: string): Verbs => {
    const rooted = getRootTsConfigFileName();
    const inputs = [
        ...(production ? ['production', '{workspaceRoot}/{projectRoot}/.storybook/**/*', '^production'] : ['default', '^default']),
        ...globs.map((at) => posix.join('{workspaceRoot}', at)),
        ...chain.filter((at) => at !== rooted).map((at) => ({ json: posix.join('{workspaceRoot}', at), fields: ['compilerOptions'] })),
        ...sources.map((at) => posix.join('{workspaceRoot}', at)),
        ...Array.match(linked, {
            onEmpty: () => [],
            onNonEmpty: (owners) => [
                { input: 'default', projects: [...owners] },
                { input: 'default', projects: [...owners], dependencies: true as const },
            ],
        }),
        { externalDependencies: Array.dedupe(['storybook', ...packages]) },
    ];
    const written = Option.getOrElse(output, () => posix.join(root, 'storybook-static'));
    const needs = { options: { cwd: root }, dependsOn: ['codegen--*', '^codegen--*'], inputs };
    const build = {
        storybook: {
            command: ['storybook', 'build', ...Option.match(output, { onNone: () => [], onSome: (at) => ['--output-dir', posix.relative(root, at)] })].map(quoteShellArg).join(' '),
            ...needs,
            cache: true,
            outputs: [...Option.match(output, { onNone: () => ['{projectRoot}/storybook-static'], onSome: (at) => [tokened(root)(at)] }), ...['output-dir', 'outputDir', 'o'].map((option) => `{projectRoot}/{options.${option}}`)],
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
                readyWhen: getReadyCheckReadyWhen('@nx/storybook/ready-check'),
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

const inferred = Effect.fnUntraced(function* (
    cache: PluginCache<Resolved & { readonly digest: string }>,
    stored: MutableRef.MutableRef<boolean>,
    context: CreateNodesContext,
    output: Option.Option<string>,
    exec: string,
    { file, chain, digest }: { readonly file: string; readonly chain: readonly string[]; readonly digest: string },
) {
    const root = dirname(dirname(file));
    const held = Option.filter(yield* recalled(cache, file, stamped(context.workspaceRoot)), (cached) => cached.digest === digest);
    const resolved = yield* Option.match(held, {
        onSome: Effect.succeed,
        onNone: () =>
            Effect.tap(read(context.workspaceRoot, file), (fresh) =>
                Effect.sync(() => {
                    cache.set(file, { ...fresh, digest });
                    MutableRef.set(stored, true);
                }),
            ),
    });
    const stated = yield* manifested(context.workspaceRoot, root);
    const outside = chain.filter((config) => !config.startsWith(`${root}/`));
    const project: ProjectConfiguration = { root, metadata: { technologies: ['storybook'] }, targets: aggregated(targets(root, resolved, outside, namedInput(root, context) === 'production', output, exec), stated, 'storybook') };
    return { projects: { [root]: project } } satisfies CreateNodesResult;
});

const nodes = Effect.fnUntraced(function* (files: readonly string[], options: typeof Options.Encoded | undefined, context: CreateNodesContext) {
    const settings = yield* Schema.decodeEffect(Options)(options ?? {});
    const project = anchored(context.workspaceRoot);
    const [mains, roots] = Array.unzip(files.map((file) => [file, dirname(dirname(file))] as const).filter(([, root]) => project(root)));
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
    const cache = new PluginCache<Resolved & { readonly digest: string }>(join(workspaceDataDirectory, 'storybook.hash'));
    const stored = MutableRef.make(false);
    const rows = new Map(Iterator.zip([mains, chains, hashes]).map(([file, chain, hash]) => [file, { file, chain, digest: hashArray([hash, own]) }] as const));
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

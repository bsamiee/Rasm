import { once } from 'node:events';
import { findPackageJSON } from 'node:module';
import { dirname, join, posix, relative } from 'node:path';
import { fileURLToPath } from 'node:url';
import { Worker } from 'node:worker_threads';
import { type CreateNodes, type CreateNodesContext, createNodesFromFiles, detectPackageManager, hashArray, normalizePath } from '@nx/devkit';
import { getLockFileName, PluginCache, quoteShellArg, workspaceDataDirectory } from '@nx/devkit/internal';
import { getRootTsConfigFileName } from '@nx/js';
import web from '@nx/web/package.json' with { type: 'json' };
import { aggregated, anchored, EXEC, extended, fingerprinted, Folder, inferring, manifested, namedInput, packaged, recalled, run, stamped, tokened, type Verbs, within } from '@rasm/nx-distribution/inference';
import distribution from '@rasm/nx-distribution/package.json' with { type: 'json' };
import { Array, Cause, Effect, MutableRef, Option, Schema } from 'effect';

// --- [TYPES] ---------------------------------------------------------------------------

type Resolved = Effect.Success<ReturnType<typeof read>>;

// --- [CONSTANTS] -----------------------------------------------------------------------

const WORKER = fileURLToPath(import.meta.resolve('#/worker'));
const RUNNER = '@storybook/test-runner';
const OWN = fingerprinted(import.meta.dirname);

// --- [MODELS] --------------------------------------------------------------------------

const Options = Schema.Struct({ outputPath: Folder }).annotate({ description: 'outputPath: workspace folder storybook build writes to, {projectName} naming the project, absent writes storybook-static in the project' });
const Loaded = Schema.Struct({ named: Schema.Array(Schema.String), stories: Schema.Array(Schema.Struct({ directory: Schema.String, files: Schema.String })), loaded: Schema.Array(Schema.Tuple([Schema.String, Schema.String, Schema.String])) }).annotate({
    description: 'The framework and addon package names Storybook read from a main config, each stories folder relative to the workspace root with its file glob, and the resolved URL, specifier, and importing module URL of every import its load resolved',
});

// --- [OPERATIONS] ----------------------------------------------------------------------

const braced = (glob: string): string => {
    const flat = glob.replaceAll(/@\((?<choices>[^()]*)\)/gu, (_group, choices: string) => `{${choices.replaceAll('|', ',')}}`);
    return flat === glob ? glob : braced(flat);
};

const read = Effect.fnUntraced(function* (workspaceRoot: string, file: string) {
    const [root, main] = [dirname(dirname(file)), join(workspaceRoot, file)];
    const failed = (cause: unknown) => new Cause.UnknownError(cause, `${file} does not load as a Storybook main config`);
    const { named, stories, loaded } = yield* Effect.acquireDisposable(Effect.sync(() => new Worker(WORKER, { workerData: [main, workspaceRoot] }))).pipe(
        Effect.flatMap((worker) => Effect.raceFirst(Effect.tryPromise({ try: (signal) => once(worker, 'message', { signal }), catch: failed }), Effect.flatMap(Effect.tryPromise({ try: (signal) => once(worker, 'exit', { signal }), catch: failed }), failed))),
        Effect.flatMap(([message]) => Schema.decodeUnknownEffect(Loaded)(message)),
        Effect.scoped,
    );
    const local = (url: string) => (url.startsWith('file:') ? Option.some(normalizePath(relative(workspaceRoot, fileURLToPath(url)))) : Option.none());
    const modules = Array.dedupe(Array.getSomes(loaded.map(([url]) => local(url)))).filter(within);
    const imported = Array.getSomes(loaded.map(([, specifier, parent]) => Option.as(Option.filter(local(parent), within), [specifier, parent] as const)));
    const [{ packages: framed, linked }, { packages: reached }] = yield* Effect.all([packaged(named.map((name) => [name, main] as const)), packaged(imported)]);
    const globs = Array.dedupe(stories.map(({ directory, files }) => posix.join(directory, braced(files)))).filter((at) => !at.startsWith('..'));
    return {
        packages: Array.dedupe([...framed, ...reached]),
        linked,
        loaded: modules.filter((at) => !at.startsWith(`${root}/`)),
        globs,
        runner: Option.isSome(Option.liftThrowable(findPackageJSON)(RUNNER, main)),
        sources: modules,
    };
});

const targets = (root: string, { packages, linked, loaded, globs, runner }: Resolved, chain: readonly string[], production: boolean, output: Option.Option<string>, exec: string): Verbs => {
    const sources = [
        ...(production ? ['production', '{workspaceRoot}/{projectRoot}/.storybook/**/*', '^production'] : ['default', '^default']),
        ...globs.map((at) => posix.join('{workspaceRoot}', at)),
        ...chain.map((at) => ({ json: posix.join('{workspaceRoot}', at), fields: ['compilerOptions'] })),
        ...loaded.map((at) => posix.join('{workspaceRoot}', at)),
        ...Array.match(linked, {
            onEmpty: () => [],
            onNonEmpty: (owners) => [
                { input: 'default', projects: [...owners] },
                { input: 'default', projects: [...owners], dependencies: true as const },
            ],
        }),
    ];
    const inputs = [...sources, { externalDependencies: Array.dedupe(['storybook', ...packages]) }];
    const written = Option.getOrElse(output, () => posix.join(root, 'storybook-static'));
    const needs = { options: { cwd: root }, dependsOn: ['codegen--*', '^codegen--*'], inputs };
    const built = ['build--storybook'];
    return {
        build: {
            storybook: {
                command: ['storybook', 'build', ...Option.match(output, { onNone: () => [], onSome: (at) => ['--output-dir', posix.relative(root, at)] })].map(quoteShellArg).join(' '),
                ...needs,
                cache: true,
                outputs: [tokened(root)(written), ...['output-dir', 'outputDir', 'o'].map((option) => `{projectRoot}/{options.${option}}`)],
                metadata: { description: 'Builds the static Storybook of the project stories with storybook build', help: { command: `${exec} storybook build --help`, example: { args: ['--test'] } } },
            },
        },
        preview: {
            storybook: {
                executor: `${web.name}:file-server`,
                dependsOn: built,
                continuous: true,
                readyWhen: { signal: true },
                inputs,
                options: { staticFilePath: written },
                metadata: { description: `Serves the static Storybook build--storybook writes with ${web.name}:file-server`, help: { command: `${exec} nx run --help`, example: { options: { port: 4400 } } } },
            },
        },
        serve: {
            storybook: {
                command: 'storybook dev',
                ...needs,
                continuous: true,
                readyWhen: { logMatches: 'Storybook ready!' },
                metadata: { description: 'Serves the project stories with storybook dev', help: { command: `${exec} storybook dev --help`, example: { args: ['--port=<port>'] } } },
            },
        },
        test: runner
            ? {
                  storybook: {
                      command: 'test-storybook',
                      options: { cwd: root },
                      dependsOn: [{ target: 'serve--storybook', waitFor: 'ready' }],
                      inputs: [...sources, { externalDependencies: Array.dedupe(['storybook', RUNNER, ...packages]) }],
                      metadata: { description: 'Runs the stories of the Storybook serve--storybook serves as tests with test-storybook, hashing the sources serve--storybook serves', help: { command: `${exec} test-storybook --help`, example: { args: ['--url', 'http://localhost:6006'] } } },
                  },
              }
            : {},
        clean: {
            storybook: {
                executor: `${distribution.name}:clean`,
                options: { targets: built },
                inputs: ['default', '^default', { externalDependencies: [] }],
                metadata: { description: `Removes the outputs ${built.join(', ')} declare`, help: { command: `${exec} nx run --help`, example: { options: { targets: built } } } },
            },
        },
    };
};

const inferred = Effect.fnUntraced(function* (pass: { readonly cache: PluginCache<Resolved & { readonly stamp: string }>; readonly stored: MutableRef.MutableRef<boolean>; readonly stamp: ReturnType<typeof stamped>; readonly exec: string }, context: CreateNodesContext, output: Option.Option<string>, file: string) {
    const root = dirname(dirname(file));
    const resolved = yield* recalled(pass.cache, file, pass.stamp, read(context.workspaceRoot, file), () => MutableRef.set(pass.stored, true));
    const stated = yield* manifested(context.workspaceRoot, root);
    const chain = extended(context.workspaceRoot, root).filter((config) => !config.startsWith(`${root}/`) && config !== getRootTsConfigFileName());
    return { projects: { [root]: { root, metadata: { technologies: ['storybook'] }, targets: aggregated(targets(root, resolved, chain, namedInput(root, context) === 'production', output, pass.exec), stated, 'storybook', context.workspaceRoot) } } };
});

const nodes = inferring(Options, run, function* (files, settings, context) {
    const project = anchored(context.workspaceRoot);
    const mains = files.filter((file) => project(dirname(dirname(file))));
    const manager = detectPackageManager(context.workspaceRoot);
    const [lock, digest] = [getLockFileName(manager), yield* OWN];
    const pass = {
        cache: new PluginCache<Resolved & { readonly stamp: string }>(join(workspaceDataDirectory, 'storybook.hash')),
        stored: MutableRef.make(false),
        stamp: (sources: readonly string[]) => Effect.map(stamped(context.workspaceRoot)([lock, ...sources]), (held) => hashArray([held, digest])),
        exec: EXEC[manager],
    };
    return yield* Effect.ensuring(
        Effect.promise(() => createNodesFromFiles((file) => run(inferred(pass, context, Option.fromUndefinedOr(settings.outputPath), file)), mains, settings, context)),
        Effect.when(
            Effect.sync(() => pass.cache.writeToDisk()),
            Effect.sync(() => MutableRef.get(pass.stored)),
        ),
    );
});

// --- [COMPOSITION] ---------------------------------------------------------------------

const createNodes: CreateNodes = ['**/.storybook/main.{js,ts,jsx,tsx,mjs,mts,mtsx,cjs,cts,ctsx}', nodes];

// --- [EXPORTS] -------------------------------------------------------------------------

export { createNodes };

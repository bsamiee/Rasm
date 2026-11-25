import { once } from 'node:events';
import { findPackageJSON } from 'node:module';
import { dirname, join, posix, relative } from 'node:path';
import { fileURLToPath } from 'node:url';
import { Worker } from 'node:worker_threads';
import { type CreateNodes, type CreateNodesContext, createNodesFromFiles, normalizePath } from '@nx/devkit';
import { quoteShellArg } from '@nx/devkit/internal';
import { getRootTsConfigFileName } from '@nx/js';
import web from '@nx/web/package.json' with { type: 'json' };
import { type Anchor, aggregated, anchored, EXECUTORS, extended, fingerprinted, inferring, manifested, namedInput, packaged, run, tokened, type Verbs, within } from '@rasm/nx';
import { Array, Cause, Effect, Option, Schema } from 'effect';

// --- [TYPES] ---------------------------------------------------------------------------

type Resolved = Effect.Success<ReturnType<typeof read>>;

// --- [CONSTANTS] -----------------------------------------------------------------------

const RUNNER = '@storybook/test-runner';
const OWN = fingerprinted(import.meta.dirname);

// --- [MODELS] --------------------------------------------------------------------------

const Options = Schema.Struct({ outputPath: Schema.OptionFromOptionalKey(Schema.String) }).annotate({ description: 'outputPath: workspace folder storybook build writes to, {projectName} naming the project, absent writes storybook-static in the project' });
const Loaded = Schema.Struct({ named: Schema.Array(Schema.String), stories: Schema.Array(Schema.Struct({ directory: Schema.String, files: Schema.String })), loaded: Schema.Array(Schema.Tuple([Schema.String, Schema.String, Schema.String])) }).annotate({
    description: 'The framework and addon package names Storybook read from a main config, each stories folder relative to the workspace root with its file glob, and the resolved URL, specifier, and importing module URL of every import its load resolved',
});

// --- [OPERATIONS] ----------------------------------------------------------------------

const braced = (glob: string): string => {
    const flat = glob.replaceAll(/@\((?<choices>[^()]*)\)/gu, (_group, choices: string) => `{${choices.replaceAll('|', ',')}}`);
    return flat === glob ? glob : braced(flat);
};

const read = Effect.fnUntraced(function* (workspaceRoot: string, file: string) {
    const main = join(workspaceRoot, file);
    const failed = (cause: unknown) => new Cause.UnknownError(cause, `${file} does not load as a Storybook main config`);
    const { named, stories, loaded } = yield* Effect.acquireDisposable(Effect.sync(() => new Worker(new URL(import.meta.resolve('#/worker')), { workerData: [main, workspaceRoot] }))).pipe(
        Effect.flatMap((worker) => Effect.raceFirst(Effect.tryPromise({ try: (signal) => once(worker, 'message', { signal }), catch: failed }), Effect.flatMap(Effect.tryPromise({ try: (signal) => once(worker, 'exit', { signal }), catch: failed }), failed))),
        Effect.flatMap(([message]) => Schema.decodeUnknownEffect(Loaded)(message)),
        Effect.scoped,
    );
    const local = Option.liftThrowable((url: string) => normalizePath(relative(workspaceRoot, fileURLToPath(url))));
    const imported = Array.getSomes(loaded.map(([, specifier, parent]) => Option.as(Option.filter(local(parent), within), [specifier, parent] as const)));
    const [{ packages: framed, linked }, { packages: reached }] = yield* Effect.all([packaged(named.map((name) => [name, main] as const)), packaged(imported)]);
    return {
        packages: [...framed, ...reached],
        linked,
        globs: Array.dedupe(stories.map(({ directory, files }) => posix.join(directory, braced(files)))).filter((at) => !at.startsWith('..')),
        runner: Option.isSome(Option.liftThrowable(findPackageJSON)(RUNNER, main)),
        sources: Array.dedupe(Array.getSomes(loaded.map(([url]) => local(url)))).filter(within),
    };
});

const targets = (root: string, { packages, linked, globs, runner, sources }: Resolved, chain: readonly string[], production: boolean, output: Option.Option<string>, exec: string): Verbs => {
    const hashed = [
        ...(production ? ['production', '{workspaceRoot}/{projectRoot}/.storybook/**/*', '^production'] : ['default', '^default']),
        ...[...globs, ...sources.filter((at) => !at.startsWith(`${root}/`))].map((at) => posix.join('{workspaceRoot}', at)),
        ...chain.map((at) => ({ json: posix.join('{workspaceRoot}', at), fields: ['compilerOptions'] })),
        ...(Array.isReadonlyArrayNonEmpty(linked)
            ? [
                  { input: 'default', projects: [...linked] },
                  { input: 'default', projects: [...linked], dependencies: true as const },
              ]
            : []),
    ];
    const inputs = [...hashed, { externalDependencies: Array.dedupe(['storybook', ...packages]) }];
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
                      inputs: [...hashed, { externalDependencies: Array.dedupe(['storybook', RUNNER, ...packages]) }],
                      metadata: { description: 'Runs the stories of the Storybook serve--storybook serves as tests with test-storybook, hashing the sources serve--storybook serves', help: { command: `${exec} test-storybook --help`, example: { args: ['--url', 'http://localhost:6006'] } } },
                  },
              }
            : {},
        clean: {
            storybook: {
                executor: EXECUTORS.clean,
                options: { targets: built },
                inputs: ['default', '^default', { externalDependencies: [] }],
                metadata: { description: `Removes the outputs ${built.join(', ')} declare`, help: { command: `${exec} nx run --help`, example: { options: { targets: built } } } },
            },
        },
    };
};

const inferred = Effect.fnUntraced(function* (pass: { readonly recall: <E, R>(key: string, fresh: Effect.Effect<Resolved, E, R>) => Effect.Effect<Resolved, E, R>; readonly exec: string }, anchor: Anchor, context: CreateNodesContext, output: Option.Option<string>, file: string) {
    const root = dirname(dirname(file));
    const resolved = yield* pass.recall(file, read(context.workspaceRoot, file));
    const stated = yield* manifested(context.workspaceRoot, root);
    const chain = extended(context.workspaceRoot, root).filter((config) => !config.startsWith(`${root}/`) && config !== getRootTsConfigFileName());
    return { projects: { [root]: { root, metadata: { technologies: ['storybook'] }, targets: aggregated(targets(root, resolved, chain, namedInput(root, context) === 'production', output, pass.exec), stated, 'storybook', anchor) } } };
});

const nodes = inferring(Options, run, function* (files, settings, context, cached) {
    const anchor = anchored(context.workspaceRoot);
    const mains = files.filter((file) => anchor.holds(dirname(dirname(file))));
    const pass = yield* cached<Resolved>('storybook.hash', yield* OWN);
    return yield* Effect.promise(() => createNodesFromFiles((file) => run(inferred(pass, anchor, context, settings.outputPath, file)), mains, settings, context));
});

// --- [COMPOSITION] ---------------------------------------------------------------------

const createNodes: CreateNodes = ['**/.storybook/main.{js,ts,jsx,tsx,mjs,mts,mtsx,cjs,cts,ctsx}', nodes];

// --- [EXPORTS] -------------------------------------------------------------------------

export { createNodes };

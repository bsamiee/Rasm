import { once } from 'node:events';
import { createRequire, findPackageJSON } from 'node:module';
import { dirname, join, posix, relative, resolve } from 'node:path';
import { fileURLToPath, pathToFileURL } from 'node:url';
import { Worker } from 'node:worker_threads';
import { type CreateNodes, type CreateNodesContext, createNodesFromFiles, hashArray, normalizePath, type ProjectConfiguration, type TargetConfiguration } from '@nx/devkit';
import { getReadyCheckReadyWhen, hashObject, quoteShellArg } from '@nx/devkit/internal';
import { type Anchor, aggregated, anchored, EXECUTORS, extended, fingerprinted, inferring, manifested, namedInput, packaged, parsed, run, tokened, type Verbs, within } from '@rasm/nx';
import { Array, Cause, Effect, FileSystem, Option, Record, Schema } from 'effect';
import type { parseSync } from 'vite';

// --- [TYPES] ---------------------------------------------------------------------------

type Resolved = Effect.Success<ReturnType<typeof read>>;

// --- [CONSTANTS] -----------------------------------------------------------------------

const WORKER = fileURLToPath(import.meta.resolve('#/worker'));
const OWN = fingerprinted(import.meta.dirname);
const CLOUDFLARE = { plugin: '@cloudflare/vite-plugin', output: '@cloudflare/build-output-utils' } as const;

// --- [MODELS] --------------------------------------------------------------------------

const Options = Schema.Struct({ outputPath: Schema.optionalKey(Schema.String) }).annotate({ description: 'outputPath: workspace folder vite build writes to, {projectName} naming the project, absent leaves build.outDir of the config' });
const Resolution = Schema.Struct({
    root: Schema.String,
    library: Schema.Boolean,
    buildable: Schema.Boolean,
    signals: Schema.Boolean,
    served: Schema.Boolean,
    written: Schema.Array(Schema.String),
    env: Schema.Struct({ directory: Schema.NullOr(Schema.String), prefixes: Schema.Array(Schema.String) }),
    dependencies: Schema.Array(Schema.String),
}).annotate({
    description:
        'What the worker read from a Vite config through resolveConfig: the Vite root, whether it builds a library, whether vite build builds anything, whether it loads the Nx ready signal, whether vite serves it, the outDir of each environment vite build builds, the envDir its dotenv files load from with the envPrefix of the variables it exposes, and every file the build load and any serve load read',
});
const Exported = Schema.fromJsonString(Schema.Struct({ exports: Schema.Struct({ '.': Schema.Struct({ import: Schema.String }) }) })).annotate({ description: `The import entry of the root export of ${CLOUDFLARE.output}, the package ${CLOUDFLARE.plugin} writes its Build Output tree through` });
const Emitted = Schema.Struct({ BUILD_OUTPUT_ROOT: Schema.String }).annotate({ description: `The Build Output root ${CLOUDFLARE.output} exports, relative to the Vite root, holding the root config and every Worker and Container ${CLOUDFLARE.plugin} writes beside the environment build folders` });

// --- [OPERATIONS] ----------------------------------------------------------------------

const read = Effect.fnUntraced(function* (workspaceRoot: string, file: string, outDir: Option.Option<string>) {
    const fs = yield* FileSystem.FileSystem;
    const at = join(workspaceRoot, file);
    const local = (to: string) => normalizePath(relative(workspaceRoot, to));
    const failed = (cause: unknown) => new Cause.UnknownError(cause, `${file} does not load for vite build`);
    const resolved = Effect.acquireDisposable(Effect.sync(() => new Worker(WORKER, { workerData: { at, outDir: Option.getOrUndefined(outDir) } }))).pipe(
        Effect.flatMap((worker) => Effect.raceFirst(Effect.tryPromise({ try: (signal) => once(worker, 'message', { signal }), catch: failed }), Effect.flatMap(Effect.tryPromise({ try: (signal) => once(worker, 'exit', { signal }), catch: failed }), failed))),
        Effect.flatMap(([message]) => Schema.decodeUnknownEffect(Resolution)(message)),
        Effect.scoped,
    );
    const [{ root: base, library, buildable, signals, served, written, env, dependencies }, vite] = yield* Effect.all(
        [resolved, Effect.tryPromise({ try: (): Promise<{ readonly parseSync: typeof parseSync }> => import(pathToFileURL(createRequire(at).resolve('vite')).href), catch: (cause) => new Cause.UnknownError(cause, `${file} resolves no vite`) })],
        { concurrency: 'unbounded' },
    );
    const loaded = Array.dedupe([file, ...dependencies.map(local)]).filter(within);
    const { packages } = yield* packaged(yield* parsed(vite.parseSync, workspaceRoot, loaded));
    const emitted = yield* Effect.forEach(
        packages.filter((name) => name === CLOUDFLARE.plugin),
        Effect.fnUntraced(function* (name) {
            const unresolved = (cause: unknown) => new Cause.UnknownError(cause, `${file} resolves no ${CLOUDFLARE.output} through ${name}`);
            const manifest = yield* Effect.reduce(
                [name, CLOUDFLARE.output],
                () => at,
                (from, specifier) => Effect.flatMap(Effect.mapError(Effect.fromOption(Option.flatMap(Option.liftThrowable(findPackageJSON)(specifier, pathToFileURL(from)), Option.fromUndefinedOr)), unresolved), fs.realPath),
            );
            const { exports } = yield* Effect.flatMap(fs.readFileString(manifest), Schema.decodeEffect(Exported));
            const utilities = yield* Effect.tryPromise({ try: (): Promise<unknown> => import(pathToFileURL(join(dirname(manifest), exports['.'].import)).href), catch: unresolved });
            return (yield* Schema.decodeUnknownEffect(Emitted)(utilities)).BUILD_OUTPUT_ROOT;
        }),
    );
    const folders = Array.dedupe([...written, ...emitted].map((to) => local(resolve(base, to))));
    return {
        type: library ? ('library' as const) : ('application' as const),
        signals,
        served,
        loaded: loaded.filter((dependency) => posix.relative(posix.dirname(file), dependency).startsWith('..')),
        packages,
        env: { directory: Option.getOrUndefined(Option.filter(Option.map(Option.fromNullOr(env.directory), local), within)), prefixes: env.prefixes },
        sources: [...loaded, local(join(base, 'index.html')), ...Array.fromNullishOr(findPackageJSON(pathToFileURL(at))).map(local)],
        ...(buildable && { folders: folders.filter((to) => !folders.some((other) => to.startsWith(`${other}/`))) }),
    };
});

const targets = (file: string, { folders, signals, served, loaded, packages, env }: Resolved, chain: readonly string[], named: 'production' | 'default', outDir: Option.Option<string>, exec: string): Verbs => {
    const needs = {
        options: { cwd: posix.dirname(file) },
        dependsOn: ['codegen--*', '^codegen--*'],
        inputs: [
            named,
            `^${named}`,
            ...chain.map((at) => ({ json: posix.join('{workspaceRoot}', at), fields: ['compilerOptions'] })),
            ...loaded.map((at) => posix.join('{workspaceRoot}', at)),
            ...Array.fromNullishOr(env.directory).map((at) => ({ fileset: posix.join('{workspaceRoot}', at, '.env*'), includeIgnored: true })),
            { runtime: ['node', '-p', 'Object.entries(process.env).filter(([name]) => process.argv.slice(1).some((prefix) => name.startsWith(prefix))).sort().join()', '--', ...env.prefixes].map(quoteShellArg).join(' ') },
            { dependentTasksOutputFiles: '**/*', transitive: true },
            { externalDependencies: Array.dedupe(['vite', ...packages]) },
        ],
    };
    const ready = (command: 'serve' | 'preview') => (signals ? { signal: true as const } : getReadyCheckReadyWhen('@nx/vite/ready-check', posix.basename(file), command));
    const server = Option.liftPredicate({ ...needs, continuous: true }, () => served);
    const build = Record.getSomes<string, TargetConfiguration>({
        vite: Option.map(Option.fromUndefinedOr(folders), (written) => ({
            command: ['vite', 'build', ...Option.match(outDir, { onNone: () => [], onSome: (at) => ['--outDir', at, '--emptyOutDir'] })].map(quoteShellArg).join(' '),
            ...needs,
            dependsOn: [...needs.dependsOn, '^build--vite'],
            cache: true,
            outputs: written.map(tokened(posix.dirname(file))),
            metadata: { description: 'Builds every environment of the Vite config with vite build', help: { command: `${exec} vite build --help`, example: { args: ['--mode', 'production'] } } },
        })),
    });
    const removed = Record.collect(build, (tool) => `build--${tool}`);
    return {
        build,
        clean: Array.match(removed, {
            onEmpty: () => ({}),
            onNonEmpty: (labels) => ({
                vite: {
                    executor: EXECUTORS.clean,
                    options: { targets: labels },
                    inputs: ['default', '^default', { externalDependencies: [] }],
                    metadata: { description: `Removes the outputs ${labels.join(', ')} declare`, help: { command: `${exec} nx run --help`, example: { options: { targets: labels } } } },
                },
            }),
        }),
        serve: Record.getSomes<string, TargetConfiguration>({
            vite: Option.map(server, (held) => ({ ...held, readyWhen: ready('serve'), command: 'vite', metadata: { description: 'Serves every environment of the Vite config with vite', help: { command: `${exec} vite --help`, example: { args: ['--port', '<port>'] } } } })),
        }),
        preview: Record.getSomes<string, TargetConfiguration>({
            vite: Option.map(server, (held) => ({
                ...held,
                readyWhen: ready('preview'),
                command: ['vite', 'preview', ...Option.match(outDir, { onNone: () => [], onSome: (at) => ['--outDir', at] })].map(quoteShellArg).join(' '),
                dependsOn: removed,
                metadata: { description: 'Serves the build output of the Vite config with vite preview', help: { command: `${exec} vite preview --help`, example: { args: ['--port', '<port>'] } } },
            })),
        }),
    };
};

const inferred = Effect.fnUntraced(function* (pass: { readonly recall: <E, R>(key: string, fresh: Effect.Effect<Resolved, E, R>) => Effect.Effect<Resolved, E, R>; readonly exec: string }, anchor: Anchor, context: CreateNodesContext, at: Option.Option<string>, file: string) {
    const root = posix.dirname(file);
    const outDir = Option.map(at, (folder) => posix.relative(root, folder));
    const resolved = yield* pass.recall(file, read(context.workspaceRoot, file, outDir));
    const stated = yield* manifested(context.workspaceRoot, root);
    const chain = extended(context.workspaceRoot, root).filter((config) => posix.relative(root, config).startsWith('..'));
    const project: ProjectConfiguration = {
        root,
        ...(resolved.folders !== undefined && { projectType: resolved.type }),
        metadata: { technologies: ['vite'] },
        targets: aggregated(targets(file, resolved, chain, namedInput(root, context), outDir, pass.exec), stated, 'vite', anchor),
    };
    return { projects: { [root]: project } };
});

const nodes = inferring(Options, run, function* (files, settings, context, cached) {
    const anchor = anchored(context.workspaceRoot);
    const configs = files.filter((file) => anchor.holds(posix.dirname(file)));
    const pass = yield* cached<Resolved>('vite.hash', hashArray([hashObject(settings), yield* OWN]));
    return yield* Effect.promise(() => createNodesFromFiles((file) => run(inferred(pass, anchor, context, Option.fromUndefinedOr(settings.outputPath), file)), configs, settings, context));
});

// --- [COMPOSITION] ---------------------------------------------------------------------

const createNodes: CreateNodes = ['**/vite.config.{js,mjs,ts,cjs,mts,cts}', nodes];

// --- [EXPORTS] -------------------------------------------------------------------------

export { createNodes };

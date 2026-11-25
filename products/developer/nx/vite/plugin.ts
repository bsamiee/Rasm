import { once } from 'node:events';
import { createRequire, findPackageJSON } from 'node:module';
import { dirname, join, posix, relative, resolve } from 'node:path';
import { fileURLToPath, pathToFileURL } from 'node:url';
import { Worker } from 'node:worker_threads';
import { type CreateNodes, type CreateNodesContext, createNodesFromFiles, hashArray, normalizePath, type TargetConfiguration } from '@nx/devkit';
import { getReadyCheckReadyWhen, hashObject, quoteShellArg } from '@nx/devkit/internal';
import { type Anchor, aggregated, anchored, below, EXECUTORS, extended, fingerprinted, inferring, inside, manifested, namedInput, packaged, parsed, type Pass, run, tokened, within } from '@rasm/nx';
import { Array, Cause, Effect, FileSystem, Option, Predicate, Record, Schema, UndefinedOr } from 'effect';
import type { parseSync } from 'vite';

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
    env: Schema.Struct({ directory: Schema.OptionFromNullOr(Schema.String), prefixes: Schema.Array(Schema.String) }),
    dependencies: Schema.Array(Schema.String),
}).annotate({
    description:
        'What the worker read from a Vite config through resolveConfig: the Vite root, whether it builds a library, whether vite build builds anything, whether it loads the Nx ready signal, whether vite serves it, the outDir of each environment vite build builds, the envDir its dotenv files load from with the envPrefix of the variables it exposes, and every file the build load and any serve load read',
});
const Exported = Schema.fromJsonString(Schema.Struct({ exports: Schema.Struct({ '.': Schema.Struct({ import: Schema.String }) }) })).annotate({ description: `The import entry of the root export of ${CLOUDFLARE.output}, the package ${CLOUDFLARE.plugin} writes its Build Output tree through` });
const Emitted = Schema.Struct({ BUILD_OUTPUT_ROOT: Schema.String }).annotate({ description: `The Build Output root ${CLOUDFLARE.output} exports, relative to the Vite root, holding the root config and every Worker and Container ${CLOUDFLARE.plugin} writes beside the environment build folders` });

// --- [OPERATIONS] ----------------------------------------------------------------------

const read = Effect.fnUntraced(function* (workspaceRoot: string, file: string, outDir: string | undefined) {
    const fs = yield* FileSystem.FileSystem;
    const at = join(workspaceRoot, file);
    const local = (to: string) => normalizePath(relative(workspaceRoot, to));
    const failed = (cause: unknown) => new Cause.UnknownError(cause, `${file} does not load for vite build`);
    const resolved = Effect.acquireDisposable(Effect.sync(() => new Worker(WORKER, { workerData: { at, outDir } }))).pipe(
        Effect.flatMap((worker) => Effect.raceFirst(Effect.tryPromise({ try: (signal) => once(worker, 'message', { signal }), catch: failed }), Effect.flatMap(Effect.tryPromise({ try: (signal) => once(worker, 'exit', { signal }), catch: failed }), failed))),
        Effect.flatMap(([message]) => Schema.decodeUnknownEffect(Resolution)(message)),
        Effect.scoped,
    );
    const [resolution, vite] = yield* Effect.all([resolved, Effect.tryPromise({ try: (): Promise<{ readonly parseSync: typeof parseSync }> => import(pathToFileURL(createRequire(at).resolve('vite')).href), catch: (cause) => new Cause.UnknownError(cause, `${file} resolves no vite`) })], { concurrency: 'unbounded' });
    const loaded = Array.dedupe([file, ...resolution.dependencies.map(local)]).filter(within);
    const { packages } = yield* packaged(yield* parsed(vite.parseSync, workspaceRoot, loaded));
    const emitted = yield* Effect.forEach(
        packages.filter((name) => name === CLOUDFLARE.plugin),
        Effect.fnUntraced(function* (name) {
            const unresolved = (cause: unknown) => new Cause.UnknownError(cause, `${file} resolves no ${CLOUDFLARE.output} through ${name}`);
            const manifest = yield* Effect.reduce(
                [name, CLOUDFLARE.output],
                () => at,
                (from, specifier) => Option.liftThrowable(findPackageJSON)(specifier, pathToFileURL(from)).pipe(Option.flatMap(Option.fromUndefinedOr), Effect.fromOption, Effect.mapError(unresolved), Effect.flatMap(fs.realPath)),
            );
            const { exports } = yield* Effect.flatMap(fs.readFileString(manifest), Schema.decodeEffect(Exported));
            const utilities = yield* Effect.tryPromise({ try: (): Promise<unknown> => import(pathToFileURL(join(dirname(manifest), exports['.'].import)).href), catch: unresolved });
            return (yield* Schema.decodeUnknownEffect(Emitted)(utilities)).BUILD_OUTPUT_ROOT;
        }),
    );
    const folders = Array.dedupe([...resolution.written, ...emitted].map((to) => local(resolve(resolution.root, to))));
    return {
        type: resolution.library ? ('library' as const) : ('application' as const),
        signals: resolution.signals,
        served: resolution.served,
        loaded: loaded.filter(Predicate.not(inside(posix.dirname(file)))),
        packages,
        env: { directory: resolution.env.directory.pipe(Option.map(local), Option.filter(within), Option.getOrUndefined), prefixes: resolution.env.prefixes },
        sources: [...loaded, local(join(resolution.root, 'index.html')), ...Array.fromNullishOr(findPackageJSON(pathToFileURL(at))).map(local)],
        ...(resolution.buildable && { folders: folders.filter((to) => !folders.some((other) => below(other)(to))) }),
    };
});

const inferred = Effect.fnUntraced(function* (pass: Pass<Effect.Success<ReturnType<typeof read>>>, anchor: Anchor, { outputPath }: typeof Options.Type, context: CreateNodesContext, file: string) {
    const root = posix.dirname(file);
    const outDir = UndefinedOr.map(outputPath, (folder) => posix.relative(root, folder));
    const { type, folders, signals, served, loaded, packages, env } = yield* pass.recall(file, read(context.workspaceRoot, file, outDir));
    const stated = yield* manifested(context.workspaceRoot, root);
    const named = namedInput(root, context);
    const needs = {
        options: { cwd: root },
        dependsOn: ['codegen--*', '^codegen--*'],
        inputs: [
            named,
            `^${named}`,
            ...extended(context.workspaceRoot, root)
                .filter(Predicate.not(inside(root)))
                .map((at) => ({ json: posix.join('{workspaceRoot}', at), fields: ['compilerOptions'] })),
            ...loaded.map((at) => posix.join('{workspaceRoot}', at)),
            ...Array.fromNullishOr(env.directory).map((at) => ({ fileset: posix.join('{workspaceRoot}', at, '.env*'), includeIgnored: true })),
            { runtime: ['node', '-p', 'Object.entries(process.env).filter(([name]) => process.argv.slice(1).some((prefix) => name.startsWith(prefix))).sort().join()', '--', ...env.prefixes].map(quoteShellArg).join(' ') },
            { dependentTasksOutputFiles: '**/*', transitive: true },
            { externalDependencies: Array.dedupe(['vite', ...packages]) },
        ],
    };
    const server = { ...needs, continuous: true };
    const ready = (command: 'serve' | 'preview') => (signals ? { signal: true as const } : getReadyCheckReadyWhen('@nx/vite/ready-check', posix.basename(file), command));
    const build = Record.getSomes<string, TargetConfiguration>({
        vite: Option.map(Option.fromUndefinedOr(folders), (written) => ({
            command: ['vite', 'build', ...Array.fromNullishOr(outDir).flatMap((at) => ['--outDir', at, '--emptyOutDir'])].map(quoteShellArg).join(' '),
            ...needs,
            dependsOn: [...needs.dependsOn, '^build--vite'],
            cache: true,
            outputs: written.map(tokened(root)),
            metadata: { description: 'Builds every environment of the Vite config with vite build', help: { command: `${pass.exec} vite build --help`, example: { args: ['--mode', 'production'] } } },
        })),
    });
    const removed = Record.collect(build, (tool) => `build--${tool}`);
    const verbs = {
        build,
        clean: Record.map(build, () => ({
            executor: EXECUTORS.clean,
            options: { targets: removed },
            inputs: ['default', '^default', { externalDependencies: [] }],
            metadata: { description: `Removes the outputs ${removed.join(', ')} declare`, help: { command: `${pass.exec} nx run --help`, example: { options: { targets: removed } } } },
        })),
        ...(served && {
            serve: { vite: { ...server, readyWhen: ready('serve'), command: 'vite', metadata: { description: 'Serves every environment of the Vite config with vite', help: { command: `${pass.exec} vite --help`, example: { args: ['--port', '<port>'] } } } } },
            preview: {
                vite: {
                    ...server,
                    readyWhen: ready('preview'),
                    command: ['vite', 'preview', ...Array.fromNullishOr(outDir).flatMap((at) => ['--outDir', at])].map(quoteShellArg).join(' '),
                    dependsOn: removed,
                    metadata: { description: 'Serves the build output of the Vite config with vite preview', help: { command: `${pass.exec} vite preview --help`, example: { args: ['--port', '<port>'] } } },
                },
            },
        }),
    };
    return { projects: { [root]: { root, ...(folders !== undefined && { projectType: type }), metadata: { technologies: ['vite'] }, targets: aggregated(verbs, stated, 'vite', anchor) } } };
});

const nodes = inferring(Options, run, function* (files, settings, context, cached) {
    const anchor = anchored(context.workspaceRoot);
    const configs = files.filter((file) => anchor.holds(posix.dirname(file)));
    const pass = yield* cached<Effect.Success<ReturnType<typeof read>>>('vite.hash', hashArray([hashObject(settings), yield* OWN]));
    return yield* Effect.promise(() => createNodesFromFiles((file) => run(inferred(pass, anchor, settings, context, file)), configs, settings, context));
});

// --- [COMPOSITION] ---------------------------------------------------------------------

const createNodes: CreateNodes = ['**/vite.config.{js,mjs,ts,cjs,mts,cts}', nodes];

// --- [EXPORTS] -------------------------------------------------------------------------

export { createNodes };

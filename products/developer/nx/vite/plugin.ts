import { once } from 'node:events';
import { createRequire, findPackageJSON } from 'node:module';
import { dirname, join, posix, relative, resolve } from 'node:path';
import { fileURLToPath, pathToFileURL } from 'node:url';
import { Worker } from 'node:worker_threads';
import { type CreateNodes, type CreateNodesContext, createNodesFromFiles, detectPackageManager, getPackageManagerCommand, hashArray, normalizePath, type ProjectConfiguration, type TargetConfiguration } from '@nx/devkit';
import { getLockFileName, getReadyCheckReadyWhen, hashObject, PluginCache, quoteShellArg, workspaceDataDirectory } from '@nx/devkit/internal';
import { getRootTsConfigFileName } from '@nx/js';
import { aggregated, anchored, extended, fingerprinted, Folder, inferring, manifested, namedInput, packaged, parsed, recalled, run, stamped, tokened, type Verbs, within } from '@rasm/nx-distribution/inference';
import distribution from '@rasm/nx-distribution/package.json' with { type: 'json' };
import { Array, Cause, Effect, FileSystem, flow, MutableRef, Option, Record, Schema } from 'effect';
import type { parseSync } from 'vite';

// --- [TYPES] ---------------------------------------------------------------------------

type Resolved = Effect.Success<ReturnType<typeof read>>;

// --- [CONSTANTS] -----------------------------------------------------------------------

const WORKER = fileURLToPath(import.meta.resolve('#/worker'));
const OWN = fingerprinted(import.meta.dirname);
const CLOUDFLARE = { plugin: '@cloudflare/vite-plugin', output: '@cloudflare/build-output-utils' } as const;

// --- [MODELS] --------------------------------------------------------------------------

const Options = Schema.Struct({ outputPath: Folder }).annotate({ description: 'outputPath: workspace folder vite build writes to, {projectName} naming the project, absent leaves build.outDir of the config' });
const Resolution = Schema.Struct({ root: Schema.String, library: Schema.Boolean, built: Schema.Boolean, signals: Schema.Boolean, stated: Schema.Boolean, written: Schema.Array(Schema.String), dependencies: Schema.Array(Schema.String) }).annotate({
    description:
        'What the worker read from a Vite config through resolveConfig for one command: the Vite root, whether it builds a library, states a builder or an entry vite build builds from, loads the Nx ready signal, and states a server or preview block, the outDir of each environment vite build builds, and every file the config load read',
});
const Exported = Schema.fromJsonString(Schema.Struct({ exports: Schema.Struct({ '.': Schema.Struct({ import: Schema.String }) }) })).annotate({ description: `The import entry of the root export of ${CLOUDFLARE.output}, the package ${CLOUDFLARE.plugin} writes its Build Output tree through` });
const Emitted = Schema.Struct({ BUILD_OUTPUT_ROOT: Schema.String }).annotate({ description: `The Build Output root ${CLOUDFLARE.output} exports, relative to the Vite root, holding the root config and every Worker and Container ${CLOUDFLARE.plugin} writes beside the environment build folders` });

// --- [OPERATIONS] ----------------------------------------------------------------------

const read = Effect.fnUntraced(function* (workspaceRoot: string, file: string, outDir: Option.Option<string>, lock: string) {
    const fs = yield* FileSystem.FileSystem;
    const at = join(workspaceRoot, file);
    const local = (to: string) => normalizePath(relative(workspaceRoot, to));
    const resolved = (command: 'build' | 'serve') => {
        const failed = (cause: unknown) => new Cause.UnknownError(cause, `${file} does not load for vite ${command}`);
        return Effect.flatMap(
            Effect.acquireUseRelease(
                Effect.sync(() => new Worker(WORKER, { workerData: { at, command, outDir: Option.getOrUndefined(outDir) } })),
                (worker) => Effect.raceFirst(Effect.tryPromise({ try: (signal) => once(worker, 'message', { signal }), catch: failed }), Effect.flatMap(Effect.tryPromise({ try: (signal) => once(worker, 'exit', { signal }), catch: failed }), flow(failed, Effect.fail))),
                (worker) => Effect.promise(() => worker.terminate()),
            ),
            ([message]) => Schema.decodeUnknownEffect(Resolution)(message),
        );
    };
    const [{ root: base, library, built, signals, stated, written, dependencies }, vite] = yield* Effect.all(
        [resolved('build'), Effect.tryPromise({ try: (): Promise<{ readonly parseSync: typeof parseSync }> => import(pathToFileURL(createRequire(at).resolve('vite')).href), catch: (cause) => new Cause.UnknownError(cause, `${file} resolves no vite`) })],
        { concurrency: 'unbounded' },
    );
    const page = join(base, 'index.html');
    const entry = yield* fs.exists(page);
    const buildable = library || entry || built;
    const served = buildable && ((!library && (entry || signals)) || stated || Option.exists(yield* Effect.option(resolved('serve')), (serve) => serve.stated));
    const loaded = Array.dedupe([file, ...dependencies.map(local)]).filter(within);
    const { packages, linked, folders: imported } = yield* packaged(yield* parsed(vite.parseSync, workspaceRoot, loaded));
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
    const sources = [lock, ...loaded, ...imported.map((folder) => posix.join(local(folder), '**/*')), local(page), ...Array.fromNullishOr(findPackageJSON(pathToFileURL(at))).map(local)];
    return {
        type: library ? ('library' as const) : ('application' as const),
        signals,
        served,
        loaded: loaded.filter((dependency) => posix.relative(posix.dirname(file), dependency).startsWith('..')),
        packages,
        linked,
        sources,
        stamp: yield* stamped(workspaceRoot)(sources),
        ...(buildable && { folders: folders.filter((to) => !folders.some((other) => to.startsWith(`${other}/`))) }),
    };
});

const targets = (file: string, { folders, signals, served, loaded, packages, linked }: Resolved, chain: readonly string[], named: 'production' | 'default', outDir: Option.Option<string>, exec: string): Verbs => {
    const rooted = getRootTsConfigFileName();
    const needs = {
        options: { cwd: posix.dirname(file) },
        dependsOn: ['codegen--*', '^codegen--*'],
        inputs: [
            named,
            `^${named}`,
            ...chain.filter((at) => at !== rooted).map((at) => ({ json: posix.join('{workspaceRoot}', at), fields: ['compilerOptions'] })),
            ...loaded.map((at) => posix.join('{workspaceRoot}', at)),
            ...Array.match(linked, { onEmpty: () => [], onNonEmpty: (owners) => [{ input: 'default', projects: [...owners] }] }),
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
            dependsOn: [...needs.dependsOn, '^build--vite', '^build--tsc'],
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
                    executor: `${distribution.name}:clean`,
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

const inferred = Effect.fnUntraced(function* (pass: { readonly cache: PluginCache<Resolved & { readonly digest: string }>; readonly stored: MutableRef.MutableRef<boolean>; readonly digest: string; readonly lock: string; readonly exec: string }, context: CreateNodesContext, at: Option.Option<string>, file: string) {
    const root = posix.dirname(file);
    const outDir = Option.map(at, (folder) => posix.relative(root, folder));
    const held = Option.filter(yield* recalled(pass.cache, file, stamped(context.workspaceRoot)), (cached) => cached.digest === pass.digest);
    const resolved = yield* Option.match(held, {
        onSome: Effect.succeed,
        onNone: () =>
            Effect.tap(read(context.workspaceRoot, file, outDir, pass.lock), (fresh) =>
                Effect.sync(() => {
                    pass.cache.set(file, { ...fresh, digest: pass.digest });
                    MutableRef.set(pass.stored, true);
                }),
            ),
    });
    const stated = yield* manifested(context.workspaceRoot, root);
    const chain = extended(context.workspaceRoot, root).filter((config) => posix.relative(root, config).startsWith('..'));
    const project: ProjectConfiguration = {
        root,
        ...(resolved.folders !== undefined && { projectType: resolved.type }),
        metadata: { technologies: ['vite'] },
        targets: aggregated(targets(file, resolved, chain, namedInput(root, context), outDir, pass.exec), stated, 'vite'),
    };
    return { projects: { [root]: project } };
});

const nodes = inferring(Options, run, function* (files, settings, context) {
    const project = anchored(context.workspaceRoot);
    const configs = files.filter((file) => project(posix.dirname(file)));
    const manager = detectPackageManager(context.workspaceRoot);
    const pass = {
        cache: new PluginCache<Resolved & { readonly digest: string }>(join(workspaceDataDirectory, 'vite.hash')),
        stored: MutableRef.make(false),
        digest: hashArray([hashObject(settings), yield* OWN]),
        lock: getLockFileName(manager),
        exec: getPackageManagerCommand(manager).exec,
    };
    return yield* Effect.ensuring(
        Effect.promise(() => createNodesFromFiles((file) => run(inferred(pass, context, Option.fromUndefinedOr(settings.outputPath), file)), configs, settings, context)),
        Effect.when(
            Effect.sync(() => pass.cache.writeToDisk()),
            Effect.sync(() => MutableRef.get(pass.stored)),
        ),
    );
});

// --- [COMPOSITION] ---------------------------------------------------------------------

const createNodes: CreateNodes = ['**/vite.config.{js,mjs,ts,cjs,mts,cts}', nodes];

// --- [EXPORTS] -------------------------------------------------------------------------

export { createNodes };

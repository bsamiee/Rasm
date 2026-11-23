import { createRequire, findPackageJSON } from 'node:module';
import { dirname, join, posix, relative, resolve } from 'node:path';
import { env } from 'node:process';
import { pathToFileURL } from 'node:url';
import { type CreateNodes, type CreateNodesContext, createNodesFromFiles, type CreateNodesResult, detectPackageManager, getPackageManagerCommand, hashArray, normalizePath, type ProjectConfiguration, type TargetConfiguration } from '@nx/devkit';
import { calculateHashesForCreateNodes, getLockFileName, getReadyCheckReadyWhen, PluginCache, quoteShellArg, retryOnRequireEsmRace, workspaceDataDirectory } from '@nx/devkit/internal';
import { getRootTsConfigFileName } from '@nx/js';
import { nxReadySignal } from '@nx/vite/plugins/nx-ready-signal.plugin';
import { aggregated, anchored, extended, fingerprinted, Folder, inferring, manifested, namedInput, packaged, parsed, recalled, run, stamped, tokened, type Verbs, within } from '@rasm/nx-distribution/inference';
import distribution from '@rasm/nx-distribution/package.json' with { type: 'json' };
import { Array, Cause, Effect, FileSystem, MutableRef, Option, Record, Schema, Struct } from 'effect';
import type { InlineConfig, parseSync, Plugin, resolveConfig } from 'vite';

// --- [TYPES] ---------------------------------------------------------------------------

type Resolved = Effect.Success<ReturnType<typeof read>>;

// --- [CONSTANTS] -----------------------------------------------------------------------

const OWN = fingerprinted(import.meta.dirname);
const CLOUDFLARE = { plugin: '@cloudflare/vite-plugin', output: '@cloudflare/build-output-utils' } as const;

// --- [MODELS] --------------------------------------------------------------------------

const Options = Schema.Struct({ outputPath: Folder }).annotate({ description: 'outputPath: workspace folder vite build writes to, {projectName} naming the project, absent leaves build.outDir of the config' });
const Exported = Schema.fromJsonString(Schema.Struct({ exports: Schema.Struct({ '.': Schema.Struct({ import: Schema.String }) }) })).annotate({ description: `The import entry of the root export of ${CLOUDFLARE.output}, the package ${CLOUDFLARE.plugin} writes its Build Output tree through` });
const Emitted = Schema.Struct({ BUILD_OUTPUT_ROOT: Schema.String }).annotate({ description: `The Build Output root ${CLOUDFLARE.output} exports, relative to the Vite root, holding the root config and every Worker and Container ${CLOUDFLARE.plugin} writes beside the environment build folders` });

// --- [OPERATIONS] ----------------------------------------------------------------------

const secluded = <A, E, R>(effect: Effect.Effect<A, E, R>) =>
    Effect.acquireUseRelease(
        Effect.sync(() => ({ ...env })),
        () => effect,
        (before) =>
            Effect.sync(() => {
                Array.difference(Object.keys(env), Object.keys(before)).forEach((name) => {
                    Reflect.deleteProperty(env, name);
                });
                Object.assign(env, before);
            }),
    );

const read = Effect.fnUntraced(function* (workspaceRoot: string, file: string, outDir: Option.Option<string>) {
    const fs = yield* FileSystem.FileSystem;
    const [root, folder, at] = [posix.dirname(file), join(workspaceRoot, posix.dirname(file)), join(workspaceRoot, file)];
    const vite = yield* Effect.tryPromise({
        try: (): Promise<{ readonly resolveConfig: typeof resolveConfig; readonly parseSync: typeof parseSync }> => import(pathToFileURL(createRequire(at).resolve('vite')).href),
        catch: (cause) => new Cause.UnknownError(cause, `${file} resolves no vite`),
    });
    const stated = MutableRef.make(false);
    const capture = { name: 'nx-vite-stated-server', enforce: 'post', config: { order: 'post', handler: (user: { readonly server?: unknown; readonly preview?: unknown }) => MutableRef.update(stated, (held) => held || user.server !== undefined || user.preview !== undefined) } } as const;
    const resolved = (command: 'build' | 'serve', mode: string, inline: Pick<InlineConfig, 'build' | 'logLevel'>) =>
        secluded(
            Effect.tryPromise({
                try: () =>
                    retryOnRequireEsmRace(() =>
                        vite.resolveConfig({ configFile: at, root: folder, plugins: [capture], ...inline }, command, mode, mode, false, undefined, (plugins) => {
                            plugins.splice(0, plugins.length, ...plugins.map(Struct.omit<Plugin, ['configResolved']>(['configResolved'])));
                        }),
                    ),
                catch: (cause) => new Cause.UnknownError(cause, `${file} does not load for vite ${command}`),
            }),
        );
    const config = yield* resolved('build', 'production', Option.match(outDir, { onNone: () => ({}), onSome: (to) => ({ build: { outDir: to } }) }));
    const library = config.build.lib !== false;
    const builds = [config.build, ...Object.values(config.environments).map((environment) => environment.build)];
    const entry = yield* fs.exists(join(config.root, 'index.html'));
    const signals = config.plugins.some(({ name }) => name === nxReadySignal().name);
    const buildable = library || entry || config.builder?.buildApp !== undefined || builds.some((build) => build.rolldownOptions.input !== undefined);
    const served = buildable && ((!library && (entry || signals)) || MutableRef.get(stated) || (yield* Effect.map(Effect.option(resolved('serve', 'development', { logLevel: 'silent' })), () => MutableRef.get(stated))));
    const loaded = Array.dedupe(config.configFileDependencies.map((dependency) => normalizePath(relative(workspaceRoot, dependency)))).filter(within);
    const sources = loaded.filter((dependency) => posix.relative(root, dependency).startsWith('..'));
    const { packages, linked } = yield* packaged(yield* parsed(vite.parseSync, workspaceRoot, loaded));
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
    const written = Array.dedupe([...builds.map((build) => build.outDir), ...emitted].map((to) => normalizePath(relative(workspaceRoot, resolve(config.root, to)))));
    const folders = Option.liftPredicate(
        written.filter((to) => !written.some((other) => to.startsWith(`${other}/`))),
        () => buildable,
    );
    return {
        type: library ? ('library' as const) : ('application' as const),
        signals,
        served,
        sources,
        packages,
        linked,
        stamp: yield* stamped(workspaceRoot)(sources),
        ...Option.getOrUndefined(Option.map(folders, (held) => ({ folders: held }))),
    };
});

const targets = (file: string, { folders, signals, served, sources, packages, linked }: Resolved, chain: readonly string[], named: 'production' | 'default', outDir: Option.Option<string>, exec: string): Verbs => {
    const rooted = getRootTsConfigFileName();
    const needs = {
        options: { cwd: posix.dirname(file) },
        dependsOn: ['codegen--*', '^codegen--*'],
        inputs: [
            named,
            `^${named}`,
            ...chain.filter((at) => at !== rooted).map((at) => ({ json: posix.join('{workspaceRoot}', at), fields: ['compilerOptions'] })),
            ...sources.map((at) => posix.join('{workspaceRoot}', at)),
            ...Array.match(linked, { onEmpty: () => [], onNonEmpty: (owners) => [{ input: 'default', projects: [...owners] }] }),
            { dependentTasksOutputFiles: '**/*', transitive: true },
            { externalDependencies: Array.dedupe(['vite', ...packages]) },
        ],
    };
    const ready = (command: 'serve' | 'preview') => (signals ? { signal: true as const } : getReadyCheckReadyWhen('@nx/vite/ready-check', posix.basename(file), command));
    const built = Option.fromUndefinedOr(folders);
    const server = Option.liftPredicate({ ...needs, continuous: true }, () => served);
    const build = Record.getSomes<string, TargetConfiguration>({
        vite: Option.map(built, (written) => ({
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

const inferred = Effect.fnUntraced(function* (
    cache: PluginCache<Resolved & { readonly digest: string }>,
    stored: MutableRef.MutableRef<boolean>,
    context: CreateNodesContext,
    at: Option.Option<string>,
    exec: string,
    { file, chain, digest }: { readonly file: string; readonly chain: readonly string[]; readonly digest: string },
) {
    const root = posix.dirname(file);
    const outDir = Option.map(at, (folder) => posix.relative(root, folder));
    const held = Option.filter(yield* recalled(cache, file, stamped(context.workspaceRoot)), (cached) => cached.digest === digest);
    const resolved = yield* Option.match(held, {
        onSome: Effect.succeed,
        onNone: () =>
            Effect.tap(read(context.workspaceRoot, file, outDir), (fresh) =>
                Effect.sync(() => {
                    cache.set(file, { ...fresh, digest });
                    MutableRef.set(stored, true);
                }),
            ),
    });
    const stated = yield* manifested(context.workspaceRoot, root);
    const project: ProjectConfiguration = {
        root,
        ...(resolved.folders !== undefined && { projectType: resolved.type }),
        metadata: { technologies: ['vite'] },
        targets: aggregated(
            targets(
                file,
                resolved,
                chain.filter((config) => posix.relative(root, config).startsWith('..')),
                namedInput(root, context),
                outDir,
                exec,
            ),
            stated,
            'vite',
        ),
    };
    return { projects: { [root]: project } } satisfies CreateNodesResult;
});

const nodes = inferring(Options, run, function* (files, settings, context) {
    const project = anchored(context.workspaceRoot);
    const configs = files.filter((file) => project(posix.dirname(file)));
    const chains = configs.map((file) => extended(context.workspaceRoot, posix.dirname(file)));
    const manager = detectPackageManager(context.workspaceRoot);
    const { exec } = getPackageManagerCommand(manager);
    const own = yield* OWN;
    const hashes = yield* Effect.promise(() =>
        calculateHashesForCreateNodes(
            configs.map(posix.dirname),
            settings,
            context,
            chains.map((chain) => [getLockFileName(manager), ...chain]),
        ),
    );
    const cache = new PluginCache<Resolved & { readonly digest: string }>(join(workspaceDataDirectory, 'vite.hash'));
    const stored = MutableRef.make(false);
    const rows = new Map(Iterator.zip([configs, chains, hashes]).map(([file, chain, hash]) => [file, { file, chain, digest: hashArray([hash, own]) }] as const));
    const each = (file: string) => run(Effect.flatMap(Effect.fromNullishOr(rows.get(file)), (row) => inferred(cache, stored, context, Option.fromUndefinedOr(settings.outputPath), exec, row)));
    return yield* Effect.ensuring(
        Effect.promise(() => createNodesFromFiles(each, configs, settings, context)),
        Effect.when(
            Effect.sync(() => cache.writeToDisk()),
            Effect.sync(() => MutableRef.get(stored)),
        ),
    );
});

// --- [COMPOSITION] ---------------------------------------------------------------------

const createNodes: CreateNodes = ['**/vite.config.{js,mjs,ts,cjs,mts,cts}', nodes];

// --- [EXPORTS] -------------------------------------------------------------------------

export { createNodes };

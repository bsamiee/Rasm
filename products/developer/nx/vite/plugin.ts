import { createRequire } from 'node:module';
import { basename, dirname, join, relative, resolve } from 'node:path';
import { env } from 'node:process';
import { fileURLToPath, pathToFileURL } from 'node:url';
import { type CreateNodes, type CreateNodesContext, createNodesFromFiles, type CreateNodesResult, detectPackageManager, getPackageManagerCommand, hashArray, normalizePath, type ProjectConfiguration, type TargetConfiguration } from '@nx/devkit';
import { calculateHashesForCreateNodes, getLockFileName, getReadyCheckReadyWhen, hashObject, PluginCache, quoteShellArg, retryOnRequireEsmRace, workspaceDataDirectory } from '@nx/devkit/internal';
import { getRootTsConfigFileName } from '@nx/js';
import { nxReadySignal } from '@nx/vite/plugins/nx-ready-signal.plugin';
import { aggregated, anchored, extended, fingerprinted, Folder, manifested, namedInput, packaged, parsed, recalled, run, stamped, tokened, type Verbs, within } from '@rasm/nx-distribution/inference';
import distribution from '@rasm/nx-distribution/package.json' with { type: 'json' };
import { Array, Cause, Effect, FileSystem, MutableRef, Option, Record, Schema } from 'effect';
import type { InlineConfig, parseSync, resolveConfig } from 'vite';

// --- [TYPES] ---------------------------------------------------------------------------

type Resolved = Effect.Success<ReturnType<typeof read>>;

// --- [CONSTANTS] -----------------------------------------------------------------------

const CHECK = normalizePath(fileURLToPath(import.meta.resolve('@nx/vite/ready-check')));
const OWN = fingerprinted(import.meta.dirname);

// --- [MODELS] --------------------------------------------------------------------------

const Options = Schema.Struct({ outputPath: Folder }).annotate({ description: 'outputPath: workspace folder vite build writes to, {projectName} naming the project, absent leaves build.outDir of the config' });

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
    const [root, folder, at] = [dirname(file), join(workspaceRoot, dirname(file)), join(workspaceRoot, file)];
    const vite = yield* Effect.tryPromise({ try: (): Promise<{ readonly resolveConfig: typeof resolveConfig; readonly parseSync: typeof parseSync }> => import(pathToFileURL(createRequire(at).resolve('vite')).href), catch: (cause) => new Cause.UnknownError(cause, `${file} resolves no vite`) });
    const stated = MutableRef.make(false);
    const capture = { name: 'nx-vite-stated-server', enforce: 'post', config: { order: 'post', handler: (user: { readonly server?: unknown; readonly preview?: unknown }) => MutableRef.update(stated, (held) => held || user.server !== undefined || user.preview !== undefined) } } as const;
    const resolved = (command: 'build' | 'serve', mode: string, inline: Pick<InlineConfig, 'build' | 'logLevel'>) =>
        secluded(
            Effect.tryPromise({
                try: () => retryOnRequireEsmRace(() => vite.resolveConfig({ configFile: at, root: folder, plugins: [capture], ...inline }, command, mode, mode)),
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
    const written = Array.dedupe(builds.map((build) => normalizePath(relative(workspaceRoot, resolve(config.root, build.outDir)))));
    const folders = Option.liftPredicate(
        written.filter((to) => !written.some((other) => to.startsWith(`${other}/`))),
        () => buildable,
    );
    const loaded = Array.dedupe(config.configFileDependencies.map((dependency) => normalizePath(relative(workspaceRoot, dependency)))).filter(within);
    const sources = loaded.filter((dependency) => relative(root, dependency).startsWith('..'));
    const { packages, linked } = yield* packaged(yield* parsed(vite.parseSync, workspaceRoot, loaded));
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
        options: { cwd: dirname(file) },
        dependsOn: ['codegen--*', '^codegen'],
        inputs: [
            named,
            `^${named}`,
            ...chain.filter((at) => at !== rooted).map((at) => ({ json: join('{workspaceRoot}', at), fields: ['compilerOptions'] })),
            ...sources.map((at) => join('{workspaceRoot}', at)),
            ...Array.match(linked, { onEmpty: () => [], onNonEmpty: (owners) => [{ input: 'default', projects: [...owners] }] }),
            { dependentTasksOutputFiles: '**/*', transitive: true },
            { externalDependencies: Array.dedupe(['vite', ...packages]) },
        ],
    };
    const ready = (command: 'serve' | 'preview') => (signals ? { signal: true as const } : getReadyCheckReadyWhen(CHECK, basename(file), command));
    const built = Option.fromUndefinedOr(folders);
    const server = Option.liftPredicate({ ...needs, continuous: true }, () => served);
    const build = Record.getSomes<string, TargetConfiguration>({
        vite: Option.map(built, (written) => ({
            command: ['vite', 'build', ...Option.match(outDir, { onNone: () => [], onSome: (at) => ['--outDir', at, '--emptyOutDir'] })].map(quoteShellArg).join(' '),
            ...needs,
            dependsOn: [...needs.dependsOn, '^build'],
            cache: true,
            outputs: written.map(tokened(dirname(file))),
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

const inferred = Effect.fnUntraced(function* (cache: PluginCache<Resolved>, stored: MutableRef.MutableRef<boolean>, context: CreateNodesContext, at: Option.Option<string>, exec: string, { file, chain, key }: { readonly file: string; readonly chain: readonly string[]; readonly key: string }) {
    const root = dirname(file);
    const outDir = Option.map(at, (folder) => relative(root, folder));
    const held = yield* recalled(cache, key, stamped(context.workspaceRoot));
    const resolved = yield* Option.match(held, {
        onSome: Effect.succeed,
        onNone: () =>
            Effect.tap(read(context.workspaceRoot, file, outDir), (fresh) =>
                Effect.sync(() => {
                    cache.set(key, fresh);
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
                chain.filter((config) => relative(root, config).startsWith('..')),
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

const nodes = Effect.fnUntraced(function* (files: readonly string[], options: typeof Options.Encoded | undefined, context: CreateNodesContext) {
    const settings = yield* Schema.decodeEffect(Options)(options ?? {});
    const project = anchored(context.workspaceRoot);
    const configs = files.filter((file) => project(dirname(file)));
    const chains = configs.map((file) => extended(context.workspaceRoot, dirname(file)));
    const manager = detectPackageManager(context.workspaceRoot);
    const { exec } = getPackageManagerCommand(manager);
    const own = yield* OWN;
    const hashes = yield* Effect.promise(() =>
        calculateHashesForCreateNodes(
            configs.map(dirname),
            settings,
            context,
            chains.map((chain) => [getLockFileName(manager), ...chain]),
        ),
    );
    const cache = new PluginCache<Resolved>(join(workspaceDataDirectory, `vite-${hashObject(options ?? {})}.hash`));
    const stored = MutableRef.make(false);
    const rows = new Map(Array.zipWith(Array.zip(configs, chains), hashes, ([file, chain], hash) => [file, { file, chain, key: hashArray([hash, own, file]) }] as const));
    const each = (file: string) => run(Effect.flatMap(Effect.fromNullishOr(rows.get(file)), (row) => inferred(cache, stored, context, Option.fromUndefinedOr(settings.outputPath), exec, row)));
    return yield* Effect.ensuring(
        Effect.promise(() => createNodesFromFiles(each, configs, options, context)),
        Effect.when(
            Effect.sync(() => cache.writeToDisk()),
            Effect.sync(() => MutableRef.get(stored)),
        ),
    );
});

// --- [COMPOSITION] ---------------------------------------------------------------------

const createNodes: CreateNodes<typeof Options.Encoded> = ['**/vite.config.{js,mjs,ts,cjs,mts,cts}', (files, options, context) => run(nodes(files, options, context))];

// --- [EXPORTS] -------------------------------------------------------------------------

export { createNodes };

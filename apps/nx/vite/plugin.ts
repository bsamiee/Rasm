import { existsSync } from 'node:fs';
import { createRequire } from 'node:module';
import { dirname, join, relative, resolve } from 'node:path';
import { env } from 'node:process';
import { fileURLToPath, pathToFileURL } from 'node:url';
import { layer } from '@effect/platform-node/NodeServices';
import { AggregateCreateNodesError, type CreateNodes, type CreateNodesContext, createNodesFromFiles, detectPackageManager, getPackageManagerCommand, normalizePath, type ProjectConfiguration, readJsonFile, type TargetConfiguration } from '@nx/devkit';
import { calculateHashesForCreateNodes, getLockFileName, getNamedInputs, hashObject, hashWithWorkspaceContext, mergeTargetConfigurations, type PackageJson, PluginCache, quoteShellArg, retryOnRequireEsmRace, workspaceDataDirectory } from '@nx/devkit/internal';
import { walkTsconfigExtendsChain } from '@nx/js/internal';
import { nxReadySignal } from '@nx/vite/plugins/nx-ready-signal.plugin';
import distribution from '@rasm/nx-distribution/package.json' with { type: 'json' };
import { Array, Cause, Effect, Equal, FileSystem, MutableRef, Option, Predicate, Record, Schema } from 'effect';

// --- [TYPES] ---------------------------------------------------------------------------

type Resolved = Effect.Success<ReturnType<typeof read>>;
type Verbs = Readonly<Record<string, Readonly<Record<string, TargetConfiguration>>>>;

// --- [CONSTANTS] -----------------------------------------------------------------------

const MODE = { NODE_ENV: 'production' } as const;
const PLUGIN = readJsonFile<PackageJson>(fileURLToPath(import.meta.resolve('#package'))).name;

// --- [MODELS] --------------------------------------------------------------------------

const Options = Schema.Struct({ outputPath: Schema.optionalKey(Schema.String) }).annotate({ description: 'outputPath: workspace folder vite build writes to, {projectName} naming the project, absent leaves build.outDir of the config' });
const Input = Schema.declare((input: unknown): input is NonNullable<TargetConfiguration['inputs']>[number] => Predicate.isString(input) || Predicate.isObject(input));
const Overrides = Schema.fromJsonString(Schema.Struct({ nx: Schema.optionalKey(Schema.Struct({ targets: Schema.optionalKey(Schema.Record(Schema.String, Schema.Struct({ inputs: Schema.optionalKey(Schema.mutable(Schema.Array(Input))) }))) })) })).annotate({
    description: "The nx.targets rows of a project's package.json, whose inputs Nx merges over the inferred targets after every plugin",
});

// --- [OPERATIONS] ----------------------------------------------------------------------

const run = <A, E>(effect: Effect.Effect<A, E, FileSystem.FileSystem>) => Effect.runPromise(Effect.provide(effect, layer));

const rendered = (cause: Cause.Cause<unknown>) => Effect.fail(new Cause.UnknownError(cause, Cause.pretty(cause)));

const manifested = Effect.fnUntraced(function* (workspaceRoot: string, root: string) {
    const fs = yield* FileSystem.FileSystem;
    const manifest = join(workspaceRoot, root, 'package.json');
    return (yield* fs.exists(manifest)) ? ((yield* Effect.flatMap(fs.readFileString(manifest), Schema.decodeEffect(Overrides))).nx?.targets ?? {}) : {};
});

const aggregated = (verbs: Verbs, stated: Readonly<Record<string, Pick<TargetConfiguration, 'inputs'>>>, technology: string): Record<string, TargetConfiguration> =>
    Record.fromEntries(
        Record.collect(Record.filter(verbs, Predicate.not(Record.isEmptyRecord)), (verb, tools): readonly (readonly [string, TargetConfiguration])[] => [
            [verb, { dependsOn: [`${verb}--*`], inputs: ['...', ...Array.dedupe(Record.collect(tools, (tool, target) => mergeTargetConfigurations(stated[`${verb}--${tool}`] ?? {}, { ...target, inputs: target.inputs ?? ['default', '^default'] }).inputs ?? []).flat())] }],
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

const read = Effect.fnUntraced(function* (workspaceRoot: string, file: string, outDir: Option.Option<string>) {
    const fs = yield* FileSystem.FileSystem;
    const [root, folder, at] = [dirname(file), join(workspaceRoot, dirname(file)), join(workspaceRoot, file)];
    const { resolveConfig }: typeof import('vite') = yield* Effect.tryPromise({ try: () => import(pathToFileURL(createRequire(at).resolve('vite')).href), catch: (cause) => new Cause.UnknownError(cause, `${file} resolves no vite`) });
    const stated = MutableRef.make(false);
    const capture = { name: 'nx-vite-stated-server', enforce: 'post', config: { order: 'post', handler: (user: { readonly server?: unknown }) => MutableRef.update(stated, (held) => held || user.server !== undefined) } } as const;
    const resolved = (command: 'build' | 'serve', mode: string, build: { readonly outDir?: string }) =>
        Effect.tryPromise({
            try: () => retryOnRequireEsmRace(() => resolveConfig({ configFile: at, root: folder, logLevel: 'silent', build, plugins: [capture] }, command, mode, mode)),
            catch: (cause) => new Cause.UnknownError(cause, `${file} does not load for vite ${command}`),
        });
    const config = yield* resolved('build', 'production', Option.match(outDir, { onNone: () => ({}), onSome: (to) => ({ outDir: to }) }));
    const serving = yield* Effect.option(resolved('serve', 'development', {}));
    const builds = [config.build, ...Object.values(config.environments).map((environment) => environment.build)];
    const buildable = config.build.lib !== false || config.builder?.buildApp !== undefined || builds.some((build) => build.rolldownOptions.input !== undefined) || (yield* fs.exists(join(config.root, 'index.html')));
    const signals = config.plugins.some(({ name }) => name === nxReadySignal().name);
    const ports = Option.map(
        Option.filter(serving, () => signals || MutableRef.get(stated)),
        ({ server, preview }) => ({ serve: server.port, preview: preview.port }),
    );
    const written = Array.dedupe(builds.map((build) => normalizePath(relative(workspaceRoot, resolve(config.root, build.outDir)))));
    const folders = Option.liftPredicate(
        written.filter((to) => !written.some((other) => to.startsWith(`${other}/`))),
        () => buildable,
    );
    const sources = Array.dedupe(config.configFileDependencies.map((dependency) => normalizePath(relative(workspaceRoot, dependency)))).filter((dependency) => !(dependency.startsWith('..') || dependency.startsWith(`${root}/`) || dependency.split('/').includes('node_modules')));
    return {
        type: config.build.lib === false ? ('application' as const) : ('library' as const),
        signals,
        sources,
        stamp: yield* stamped(workspaceRoot, sources),
        ...Option.getOrUndefined(Option.map(ports, (held) => ({ ports: held }))),
        ...Option.getOrUndefined(Option.map(folders, (held) => ({ folders: held }))),
    };
});

const targets = (root: string, { folders, signals, ports, sources }: Resolved, chain: readonly string[], named: boolean, outDir: Option.Option<string>, exec: string): Verbs => {
    const needs = {
        options: { cwd: root },
        dependsOn: ['codegen--*', '^codegen'],
        inputs: [named ? 'production' : 'default', named ? '^production' : '^default', ...[...chain, ...sources].map((at) => join('{workspaceRoot}', at)), { dependentTasksOutputFiles: '**/*', transitive: true }, { externalDependencies: ['vite'] }],
    };
    const built = Option.fromUndefinedOr(folders);
    const served = Option.map(
        Option.fromUndefinedOr(ports),
        Record.map((port) => ({ ...needs, continuous: true, readyWhen: signals ? { signal: true as const } : { port } })),
    );
    const build = Record.getSomes<string, TargetConfiguration>({
        vite: Option.map(built, (written) => ({
            command: ['vite', 'build', ...Option.match(outDir, { onNone: () => [], onSome: (at) => ['--outDir', at, '--emptyOutDir'] })].map(quoteShellArg).join(' '),
            ...needs,
            dependsOn: [...needs.dependsOn, '^build'],
            cache: true,
            outputs: written.map((at) => join('{workspaceRoot}', at)),
            metadata: { description: 'Builds every environment of the Vite config with vite build', help: { command: `${exec} vite build --help`, example: { args: ['--mode', 'production'] } } },
        })),
    });
    const removed = Record.collect(build, (tool) => `build--${tool}`);
    return {
        build,
        clean: Array.match(removed, {
            onEmpty: () => ({}),
            onNonEmpty: (labels) => ({ vite: { executor: `${distribution.name}:clean`, options: { targets: labels }, metadata: { description: `Removes the outputs ${labels.join(', ')} declare`, help: { command: `${exec} nx run --help`, example: { options: { targets: labels } } } } } }),
        }),
        serve: Record.getSomes<string, TargetConfiguration>({
            vite: Option.map(served, ({ serve }) => ({ ...serve, command: 'vite', metadata: { description: 'Serves every environment of the Vite config with vite', help: { command: `${exec} vite --help`, example: { args: ['--port', '<port>'] } } } })),
        }),
        preview: Record.getSomes<string, TargetConfiguration>({
            vite: Option.map(Option.andThen(built, served), ({ preview }) => ({
                ...preview,
                command: ['vite', 'preview', ...Option.match(outDir, { onNone: () => [], onSome: (at) => ['--outDir', at] })].map(quoteShellArg).join(' '),
                dependsOn: removed,
                metadata: { description: 'Serves the build output of the Vite config with vite preview', help: { command: `${exec} vite preview --help`, example: { args: ['--port', '<port>'] } } },
            })),
        }),
    };
};

const inferred = Effect.fnUntraced(function* (cache: PluginCache<Resolved>, context: CreateNodesContext, at: Option.Option<string>, chain: readonly string[], hash: string, file: string) {
    const root = dirname(file);
    const key = `${hash}${file}`;
    const outDir = Option.map(at, (folder) => relative(root, folder));
    const held = yield* Effect.findFirst(Option.toArray(Option.fromUndefinedOr(cache.get(key))), ({ sources, stamp }) => Effect.map(stamped(context.workspaceRoot, sources), Equal.equals(stamp)));
    const loaded = Effect.acquireUseRelease(
        Effect.sync(() => ({ ...env })),
        () =>
            Effect.andThen(
                Effect.sync(() => Object.assign(env, MODE)),
                read(context.workspaceRoot, file, outDir),
            ),
        (before) =>
            Effect.sync(() => {
                Reflect.deleteProperty(env, 'NODE_ENV');
                Object.assign(env, before);
            }),
    );
    const resolved = yield* Option.match(held, { onSome: Effect.succeed, onNone: () => loaded });
    cache.set(key, resolved);
    const stated = yield* manifested(context.workspaceRoot, root);
    const { exec } = getPackageManagerCommand(detectPackageManager(context.workspaceRoot));
    const project: ProjectConfiguration = {
        root,
        ...(resolved.folders !== undefined && { projectType: resolved.type }),
        metadata: { technologies: ['vite'] },
        targets: aggregated(
            targets(
                root,
                resolved,
                chain.filter((config) => !config.startsWith(`${root}/`)),
                'production' in getNamedInputs(root, context),
                outDir,
                exec,
            ),
            stated,
            'vite',
        ),
    };
    return { projects: { [root]: project } };
});

const nodes = Effect.fnUntraced(function* (files: readonly string[], options: typeof Options.Encoded | undefined, context: CreateNodesContext) {
    const settings = yield* Schema.decodeEffect(Options)(options ?? {});
    const configs = files.filter((file) => existsSync(join(context.workspaceRoot, dirname(file), 'package.json')));
    const roots = configs.map(dirname);
    const chains = roots.map((root) => extended(context.workspaceRoot, root));
    const lock = getLockFileName(detectPackageManager(context.workspaceRoot));
    const cache = new PluginCache<Resolved>(join(workspaceDataDirectory, `vite-${hashObject(options ?? {})}.hash`));
    const hashes = yield* Effect.promise(() =>
        calculateHashesForCreateNodes(
            roots,
            settings,
            context,
            chains.map((chain) => [lock, ...chain]),
        ),
    );
    const each = (file: string, index: number) => run(Effect.catchCause(inferred(cache, context, Option.fromUndefinedOr(settings.outputPath), chains[index] ?? [], `${hashes[index]}`, file), rendered));
    return yield* Effect.ensuring(
        Effect.tryPromise({
            try: () => createNodesFromFiles((file, _options, _context, index) => each(file, index), configs, options, context),
            catch: (cause) => (cause instanceof AggregateCreateNodesError ? cause : new AggregateCreateNodesError([[null, new Cause.UnknownError(cause, `${PLUGIN} infers no project`)]], [])),
        }),
        Effect.sync(() => cache.writeToDisk()),
    );
});

// --- [COMPOSITION] ---------------------------------------------------------------------

const createNodes: CreateNodes<typeof Options.Encoded> = ['**/vite.config.{js,mjs,ts,cjs,mts,cts}', (files, options, context) => run(nodes(files, options, context))];

// --- [EXPORTS] -------------------------------------------------------------------------

export { createNodes };

import { existsSync } from 'node:fs';
import { createRequire } from 'node:module';
import { basename, dirname, join, relative, resolve } from 'node:path';
import { env } from 'node:process';
import { fileURLToPath, pathToFileURL } from 'node:url';
import { layer } from '@effect/platform-node/NodeServices';
import { AggregateCreateNodesError, type CreateNodes, type CreateNodesContext, type CreateNodesResult, detectPackageManager, getPackageManagerCommand, hashArray, normalizePath, type ProjectConfiguration, type TargetConfiguration } from '@nx/devkit';
import { calculateHashesForCreateNodes, getLockFileName, getNamedInputs, getReadyCheckReadyWhen, hashFile, hashObject, hashWithWorkspaceContext, mergeTargetConfigurations, PluginCache, quoteShellArg, retryOnRequireEsmRace, workspaceDataDirectory } from '@nx/devkit/internal';
import { getRootTsConfigFileName } from '@nx/js';
import { walkTsconfigExtendsChain } from '@nx/js/internal';
import { nxReadySignal } from '@nx/vite/plugins/nx-ready-signal.plugin';
import distribution from '@rasm/nx-distribution/package.json' with { type: 'json' };
import { Array, Cause, Effect, Equal, FileSystem, MutableRef, Option, Predicate, pipe, Record, Result, Schema } from 'effect';

// --- [TYPES] ---------------------------------------------------------------------------

type Resolved = Effect.Success<ReturnType<typeof read>>;
type Verbs = Readonly<Record<string, Readonly<Record<string, TargetConfiguration>>>>;

// --- [CONSTANTS] -----------------------------------------------------------------------

const CHECK = normalizePath(fileURLToPath(import.meta.resolve('@nx/vite/ready-check')));
const OWN = pipe(
    Effect.flatMap(FileSystem.FileSystem, (fs) => fs.readDirectory(import.meta.dirname)),
    Effect.map((names) => hashArray(names.toSorted().flatMap((name) => [name, ...Array.fromNullishOr(hashFile(join(import.meta.dirname, name)))]))),
    Effect.cached,
    Effect.runSync,
);

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

const secluded = <A, E, R>(effect: Effect.Effect<A, E, R>) =>
    Effect.acquireUseRelease(
        Effect.sync(() => ({ ...env })),
        () => effect,
        (before) =>
            Effect.sync(() => {
                Array.difference(Object.keys(env), Object.keys(before)).forEach((name) => Reflect.deleteProperty(env, name));
                Object.assign(env, before);
            }),
    );

const read = Effect.fnUntraced(function* (workspaceRoot: string, file: string, outDir: Option.Option<string>) {
    const fs = yield* FileSystem.FileSystem;
    const [root, folder, at] = [dirname(file), join(workspaceRoot, dirname(file)), join(workspaceRoot, file)];
    const { resolveConfig }: typeof import('vite') = yield* Effect.tryPromise({ try: () => import(pathToFileURL(createRequire(at).resolve('vite')).href), catch: (cause) => new Cause.UnknownError(cause, `${file} resolves no vite`) });
    const stated = MutableRef.make(false);
    const capture = { name: 'nx-vite-stated-server', enforce: 'post', config: { order: 'post', handler: (user: { readonly server?: unknown }) => MutableRef.update(stated, (held) => held || user.server !== undefined) } } as const;
    const resolved = (command: 'build' | 'serve', mode: string, build: { readonly outDir?: string }) =>
        secluded(
            Effect.tryPromise({
                try: () => retryOnRequireEsmRace(() => resolveConfig({ configFile: at, root: folder, logLevel: 'silent', build, plugins: [capture] }, command, mode, mode)),
                catch: (cause) => new Cause.UnknownError(cause, `${file} does not load for vite ${command}`),
            }),
        );
    const config = yield* resolved('build', 'production', Option.match(outDir, { onNone: () => ({}), onSome: (to) => ({ outDir: to }) }));
    const signals = config.plugins.some(({ name }) => name === nxReadySignal().name);
    const served = signals || MutableRef.get(stated) || (yield* Effect.map(Effect.option(resolved('serve', 'development', {})), () => MutableRef.get(stated)));
    const builds = [config.build, ...Object.values(config.environments).map((environment) => environment.build)];
    const buildable = config.build.lib !== false || config.builder?.buildApp !== undefined || builds.some((build) => build.rolldownOptions.input !== undefined) || (yield* fs.exists(join(config.root, 'index.html')));
    const written = Array.dedupe(builds.map((build) => normalizePath(relative(workspaceRoot, resolve(config.root, build.outDir)))));
    const folders = Option.liftPredicate(
        written.filter((to) => !written.some((other) => to.startsWith(`${other}/`))),
        () => buildable,
    );
    const sources = Array.dedupe(config.configFileDependencies.map((dependency) => normalizePath(relative(workspaceRoot, dependency)))).filter((dependency) => !(dependency.startsWith('..') || dependency.startsWith(`${root}/`) || dependency.split('/').includes('node_modules')));
    return {
        type: config.build.lib === false ? ('application' as const) : ('library' as const),
        signals,
        served,
        sources,
        stamp: yield* stamped(workspaceRoot, sources),
        ...Option.getOrUndefined(Option.map(folders, (held) => ({ folders: held }))),
    };
});

const targets = (file: string, { folders, signals, served, sources }: Resolved, chain: readonly string[], named: boolean, outDir: Option.Option<string>, exec: string): Verbs => {
    const rooted = getRootTsConfigFileName();
    const needs = {
        options: { cwd: dirname(file) },
        dependsOn: ['codegen--*', '^codegen'],
        inputs: [
            named ? 'production' : 'default',
            named ? '^production' : '^default',
            ...chain.map((at) => ({ json: join('{workspaceRoot}', at), fields: ['compilerOptions'], ...(at === rooted && { excludeFields: ['compilerOptions.paths'] }) })),
            ...sources.map((at) => join('{workspaceRoot}', at)),
            { dependentTasksOutputFiles: '**/*', transitive: true },
            { externalDependencies: ['vite'] },
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
            outputs: written.map((at) => join('{workspaceRoot}', at)),
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
            vite: Option.map(Option.andThen(built, server), (held) => ({
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
    const held = yield* Effect.findFirst(Array.fromNullishOr(cache.get(key)), ({ sources, stamp }) => Effect.map(stamped(context.workspaceRoot, sources), Equal.equals(stamp)));
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
                chain.filter((config) => !config.startsWith(`${root}/`)),
                'production' in getNamedInputs(root, context),
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
    const configs = files.filter((file) => existsSync(join(context.workspaceRoot, dirname(file), 'package.json')));
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
    const rows = Array.zipWith(Array.zip(configs, chains), hashes, ([file, chain], hash) => ({ file, chain, key: hashArray([hash, own, file]) }));
    const settled = Array.zip(configs, yield* Effect.forEach(rows, (row) => Effect.result(Effect.catchCause(inferred(cache, stored, context, Option.fromUndefinedOr(settings.outputPath), exec, row), rendered))));
    yield* Effect.when(
        Effect.sync(() => cache.writeToDisk()),
        Effect.sync(() => MutableRef.get(stored)),
    );
    const failures = Array.filterMap(settled, ([file, result]) => Result.map(Result.flip(result), (error): [string, Error] => [file, error]));
    const results = Array.filterMap(settled, ([file, result]) => Result.map(result, (held) => [file, held] as const));
    return yield* Array.match(failures, { onEmpty: () => Effect.succeed(results), onNonEmpty: (failed) => Effect.fail(new AggregateCreateNodesError(failed, results)) });
});

// --- [COMPOSITION] ---------------------------------------------------------------------

const createNodes: CreateNodes<typeof Options.Encoded> = ['**/vite.config.{js,mjs,ts,cjs,mts,cts}', (files, options, context) => run(nodes(files, options, context))];

// --- [EXPORTS] -------------------------------------------------------------------------

export { createNodes };

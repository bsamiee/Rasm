import { existsSync } from 'node:fs';
import { createRequire, findPackageJSON } from 'node:module';
import { dirname, join, relative, resolve } from 'node:path';
import { env } from 'node:process';
import { pathToFileURL } from 'node:url';
import { layer } from '@effect/platform-node/NodeServices';
import { AggregateCreateNodesError, type CreateNodes, type CreateNodesContext, createNodesFromFiles, detectPackageManager, getPackageManagerCommand, hashArray, normalizePath, type ProjectConfiguration, parseJson, type TargetConfiguration } from '@nx/devkit';
import { calculateHashesForCreateNodes, getLockFileName, getNamedInputs, hashFile, hashObject, hashWithWorkspaceContext, mergeTargetConfigurations, PluginCache, quoteShellArg, retryOnRequireEsmRace, workspaceDataDirectory } from '@nx/devkit/internal';
import { walkTsconfigExtendsChain } from '@nx/js/internal';
import { nxReadySignal } from '@nx/vite/plugins/nx-ready-signal.plugin';
import distribution from '@rasm/nx-distribution/package.json' with { type: 'json' };
import { Array, Effect, Equal, FileSystem, Formatter, Match, MutableRef, Option, Predicate, Record, Schema, UndefinedOr } from 'effect';
import type { PackageJson } from 'nx/src/utils/package-json';

// --- [TYPES] ---------------------------------------------------------------------------

type Resolved = Effect.Success<ReturnType<typeof read>>;

// --- [CONSTANTS] -----------------------------------------------------------------------

const MODE = { NODE_ENV: 'production' } as const;
const SHARED = distribution.name;

// --- [MODELS] --------------------------------------------------------------------------

const Placed = Schema.Struct({ outputPath: Schema.optionalKey(Schema.String) }).annotate({ description: 'outputPath: workspace folder vite build writes under as <outputPath>/vite/<project name>, absent leaves build.outDir of the config' });

// --- [ERRORS] --------------------------------------------------------------------------

class ViteError extends Schema.TaggedError<ViteError, { readonly brand: unique symbol }>()('ViteError', {
    reason: Schema.TaggedUnion({ Unresolved: { specifier: Schema.String }, Unloaded: {} }),
    subject: Schema.String,
    cause: Schema.optionalKey(Schema.Defect()),
}) {
    static readonly of = (reason: ViteError['reason'], subject: string) => (cause?: unknown) => ViteError.make({ reason, subject, ...(cause === undefined ? {} : { cause }) });

    override get message() {
        const said = Match.valueTags(this.reason, { Unresolved: ({ specifier }) => `resolves no ${specifier}`, Unloaded: () => 'does not load' });
        return [`${this.subject} ${said}`, ...(this.cause === undefined ? [] : [Formatter.format(this.cause)])].join(': ');
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

const run = <A, E>(effect: Effect.Effect<A, E, FileSystem.FileSystem>) => Effect.runPromise(Effect.provide(effect, layer));

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
    const { resolveConfig }: typeof import('vite') = yield* Effect.tryPromise({ try: () => import(pathToFileURL(createRequire(at).resolve('vite')).href), catch: ViteError.of({ _tag: 'Unresolved', specifier: 'vite' }, file) });
    const stated = MutableRef.make(false);
    const capture = { name: 'nx-vite-stated-server', enforce: 'post', config: { order: 'post', handler: (user: { readonly server?: unknown }) => MutableRef.update(stated, (held) => held || user.server !== undefined) } } as const;
    const resolved = (command: 'build' | 'serve', mode: string, build: { readonly outDir?: string }) =>
        Effect.tryPromise({ try: () => retryOnRequireEsmRace(() => resolveConfig({ configFile: at, root: folder, logLevel: 'silent', build, plugins: [capture] }, command, mode, mode)), catch: ViteError.of({ _tag: 'Unloaded' }, file) });
    const config = yield* resolved('build', 'production', Option.match(outDir, { onNone: () => ({}), onSome: (to) => ({ outDir: to }) }));
    const serving = yield* Effect.option(resolved('serve', 'development', {}));
    const builds = [config.build, ...Object.values(config.environments).map((environment) => environment.build)];
    const buildable =
        config.build.lib !== false || config.builder?.buildApp !== undefined || builds.some((build) => build.rolldownOptions.input !== undefined) || (yield* Effect.mapError(fs.exists(join(config.root, 'index.html')), ViteError.of({ _tag: 'Unloaded' }, normalizePath(relative(workspaceRoot, config.root)))));
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

const targets = (root: string, { folders, signals, ports, sources }: Resolved, chain: readonly string[], named: boolean, outDir: Option.Option<string>, exec: string, stated: Readonly<Record<string, TargetConfiguration>>): Record<string, TargetConfiguration> => {
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
    const builders = Record.keys(build).map((tool) => `build--${tool}`);
    const verbs: Readonly<Record<string, Readonly<Record<string, TargetConfiguration>>>> = {
        build,
        clean: Record.getSomes<string, TargetConfiguration>({ vite: Option.map(Option.liftPredicate(builders, Array.isReadonlyArrayNonEmpty), (written) => ({ executor: `${SHARED}:clean`, options: { targets: written }, metadata: { description: `Removes the outputs ${written.join(', ')} declare` } })) }),
        serve: Record.getSomes<string, TargetConfiguration>({
            vite: Option.map(served, ({ serve }) => ({ ...serve, command: 'vite', metadata: { description: 'Serves every environment of the Vite config with the vite development server', help: { command: `${exec} vite --help`, example: { args: ['--port', '<port>'] } } } })),
        }),
        preview: Record.getSomes<string, TargetConfiguration>({
            vite: Option.map(Option.andThen(built, served), ({ preview }) => ({
                ...preview,
                command: ['vite', 'preview', ...Option.match(outDir, { onNone: () => [], onSome: (at) => ['--outDir', at] })].map(quoteShellArg).join(' '),
                dependsOn: builders,
                metadata: { description: 'Serves the build output of the Vite config with vite preview', help: { command: `${exec} vite preview --help`, example: { args: ['--port', '<port>'] } } },
            })),
        }),
    };
    return Record.fromEntries(
        Record.collect(Record.filter(verbs, Predicate.not(Record.isEmptyRecord)), (verb, tools): readonly (readonly [string, TargetConfiguration])[] => [
            [verb, { dependsOn: [`${verb}--*`], inputs: ['...', ...Array.dedupe(Record.collect(tools, (tool, target) => mergeTargetConfigurations(stated[`${verb}--${tool}`] ?? {}, target).inputs ?? ['default', '^default']).flat())] }],
            ...Record.collect(tools, (tool, target): readonly [string, TargetConfiguration] => [`${verb}--${tool}`, target]),
        ]).flat(),
    );
};

const inferred = Effect.fnUntraced(function* (cache: PluginCache<Resolved>, context: CreateNodesContext, at: Option.Option<string>, chain: readonly string[], hash: string, file: string) {
    const fs = yield* FileSystem.FileSystem;
    const root = dirname(file);
    const key = `${hash}_${file}`;
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
    const manifest = join(root, 'package.json');
    const { nx } = yield* Effect.flatMap(fs.readFileString(join(context.workspaceRoot, manifest)), (text) => Effect.try({ try: () => parseJson<PackageJson>(text), catch: ViteError.of({ _tag: 'Unloaded' }, manifest) }));
    const { exec } = getPackageManagerCommand(detectPackageManager(context.workspaceRoot));
    const project: ProjectConfiguration = {
        root,
        ...(resolved.folders !== undefined && { projectType: resolved.type }),
        metadata: { technologies: ['vite'] },
        targets: targets(
            root,
            resolved,
            chain.filter((config) => !config.startsWith(`${root}/`)),
            'production' in getNamedInputs(root, context),
            outDir,
            exec,
            nx?.targets ?? {},
        ),
    };
    return { projects: { [root]: project } };
});

const nodes = Effect.fnUntraced(function* (files: readonly string[], options: unknown, context: CreateNodesContext) {
    const { outputPath } = yield* Schema.decodeEffect(Placed)({ ...context.nxJsonConfiguration.pluginsConfig?.[SHARED], ...(Predicate.isObject(options) ? options : {}) });
    const configs = files.filter((file) => existsSync(join(context.workspaceRoot, dirname(file), 'package.json')));
    const roots = configs.map(dirname);
    const chains = roots.map((root) => extended(context.workspaceRoot, root));
    const lock = getLockFileName(detectPackageManager(context.workspaceRoot));
    const at = Option.map(Option.fromUndefinedOr(outputPath), (folder) => join(folder, 'vite', '{projectName}'));
    const code = hashArray([...Array.fromNullishOr(UndefinedOr.map(findPackageJSON(import.meta.url), hashFile)), yield* Effect.promise(() => hashWithWorkspaceContext(context.workspaceRoot, [normalizePath(join(relative(context.workspaceRoot, import.meta.dirname), '**'))]))]);
    const cache = new PluginCache<Resolved>(join(workspaceDataDirectory, `vite-${hashObject({ options, at: Option.getOrNull(at) })}.hash`));
    const hashes = yield* Effect.promise(() =>
        calculateHashesForCreateNodes(
            roots,
            { options, at: Option.getOrNull(at), code },
            context,
            chains.map((chain) => [lock, ...chain]),
        ),
    );
    const each = (file: string, index: number) => run(inferred(cache, context, at, chains[index] ?? [], `${hashes[index]}`, file));
    return yield* Effect.ensuring(
        Effect.tryPromise({
            try: () => createNodesFromFiles((file, _options, _context, index) => each(file, index), configs, options, context),
            catch: (cause) => (cause instanceof AggregateCreateNodesError ? cause : new AggregateCreateNodesError([[null, ViteError.of({ _tag: 'Unloaded' }, 'createNodes')(cause)]], [])),
        }),
        Effect.sync(() => cache.writeToDisk()),
    );
});

// --- [COMPOSITION] ---------------------------------------------------------------------

const createNodes: CreateNodes = ['**/vite.config.{js,mjs,ts,cjs,mts,cts}', (files, options, context) => run(nodes(files, options, context))];

// --- [EXPORTS] -------------------------------------------------------------------------

export { createNodes };

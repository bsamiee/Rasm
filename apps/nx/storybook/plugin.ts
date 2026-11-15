import { existsSync } from 'node:fs';
import { createRequire, findPackageJSON } from 'node:module';
import { dirname, isAbsolute, join, relative } from 'node:path';
import { env } from 'node:process';
import { fileURLToPath, pathToFileURL } from 'node:url';
import { layer } from '@effect/platform-node/NodeServices';
import { AggregateCreateNodesError, type CreateNodes, type CreateNodesContext, createNodesFromFiles, detectPackageManager, getPackageManagerCommand, normalizePath, type ProjectConfiguration, readJsonFile, type TargetConfiguration } from '@nx/devkit';
import { calculateHashesForCreateNodes, getLockFileName, getNamedInputs, hashObject, mergeTargetConfigurations, type PackageJson, PluginCache, quoteShellArg, workspaceDataDirectory } from '@nx/devkit/internal';
import { walkTsconfigExtendsChain } from '@nx/js/internal';
import web from '@nx/web/package.json' with { type: 'json' };
import distribution from '@rasm/nx-distribution/package.json' with { type: 'json' };
import { Array, Cause, Effect, FileSystem, Option, Predicate, pipe, Record, Schema } from 'effect';

// --- [TYPES] ---------------------------------------------------------------------------

type Verbs = Readonly<Record<string, Readonly<Record<string, TargetConfiguration>>>>;

// --- [CONSTANTS] -----------------------------------------------------------------------

const MODE = { NODE_ENV: 'production' } as const;
const PLUGIN = readJsonFile<PackageJson>(fileURLToPath(import.meta.resolve('#package'))).name;

// --- [MODELS] --------------------------------------------------------------------------

const Options = Schema.Struct({ outputPath: Schema.optionalKey(Schema.String) }).annotate({ description: 'outputPath: workspace folder storybook build writes to, {projectName} naming the project, absent writes storybook-static in the project' });
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

const secluded = <A, E, R>(effect: Effect.Effect<A, E, R>) =>
    Effect.acquireUseRelease(
        Effect.sync(() => ({ ...env })),
        () =>
            Effect.andThen(
                Effect.sync(() => Object.assign(env, MODE)),
                effect,
            ),
        (before) =>
            Effect.sync(() => {
                Reflect.deleteProperty(env, 'NODE_ENV');
                Object.assign(env, before);
            }),
    );

const read = Effect.fnUntraced(function* (workspaceRoot: string, file: string) {
    const fs = yield* FileSystem.FileSystem;
    const main = join(workspaceRoot, file);
    const { isInNodeModules, loadMainConfig, normalizeAddonName }: typeof import('storybook/internal/common') = yield* Effect.tryPromise({
        try: () => import(pathToFileURL(createRequire(main).resolve('storybook/internal/common')).href),
        catch: (cause) => new Cause.UnknownError(cause, `${file} resolves no storybook/internal/common`),
    });
    const { framework, addons = [] } = yield* Effect.tryPromise({ try: () => loadMainConfig({ configDir: dirname(main), cwd: dirname(dirname(main)), skipCache: true }), catch: (cause) => new Cause.UnknownError(cause, `${file} does not load as a Storybook main config`) });
    const named = Array.getSomes([...Option.toArray(Option.fromUndefinedOr(framework)), ...addons].map((entry) => Option.filter(Option.fromUndefinedOr(normalizeAddonName(entry)), (name) => !isAbsolute(name))));
    const located = yield* Effect.forEach(Array.dedupe(named), (name) =>
        pipe(
            Option.liftThrowable(findPackageJSON)(name, pathToFileURL(main)),
            Option.flatMap(Option.fromUndefinedOr),
            Option.map((at) => Effect.option(fs.realPath(at))),
            Effect.transposeOption,
            Effect.map((manifest) => ({ name, manifest })),
        ),
    );
    return located.filter(({ manifest }) => Option.match(manifest, { onNone: () => true, onSome: Option.exists(isInNodeModules) })).map(({ name }) => name);
});

const targets = (root: string, packages: readonly string[], chain: readonly string[], upstream: string, output: Option.Option<string>, exec: string): Verbs => {
    const inputs = ['default', upstream, ...chain.map((at) => join('{workspaceRoot}', at)), { externalDependencies: Array.dedupe(['storybook', ...packages]) }];
    const written = Option.getOrElse(output, () => join(root, 'storybook-static'));
    const build = {
        storybook: {
            command: ['storybook', 'build', ...Option.match(output, { onNone: () => [], onSome: (at) => ['--output-dir', relative(root, at)] })].map(quoteShellArg).join(' '),
            options: { cwd: root },
            dependsOn: ['codegen--*', '^codegen'],
            cache: true,
            inputs,
            outputs: [join('{workspaceRoot}', written)],
            metadata: { description: 'Builds the static Storybook of the project stories with storybook build', help: { command: `${exec} storybook build --help`, example: { args: ['--test'] } } },
        },
    } satisfies Record<string, TargetConfiguration>;
    const removed = Record.collect(build, (tool) => `build--${tool}`);
    return {
        build,
        preview: {
            storybook: {
                executor: `${web.name}:file-server`,
                dependsOn: removed,
                continuous: true,
                readyWhen: { signal: true },
                inputs,
                options: { buildTarget: 'build--storybook', staticFilePath: written },
                metadata: { description: `Serves the static Storybook build--storybook writes with ${web.name}:file-server`, help: { command: `${exec} nx run --help`, example: { options: { port: 4400 } } } },
            },
        },
        serve: {
            storybook: {
                command: 'storybook dev',
                options: { cwd: root },
                continuous: true,
                readyWhen: { port: '{options.port ?? env.SBCONFIG_PORT ?? env.PORT ?? auto}', host: '{env.SBCONFIG_HOSTNAME ?? options.host ?? localhost}' },
                inputs,
                metadata: { description: 'Serves the project stories with storybook dev', help: { command: `${exec} storybook dev --help`, example: { args: ['--port=<port>'] } } },
            },
        },
        clean: { storybook: { executor: `${distribution.name}:clean`, options: { targets: removed }, metadata: { description: `Removes the outputs ${removed.join(', ')} declare`, help: { command: `${exec} nx run --help`, example: { options: { targets: removed } } } } } },
    };
};

const inferred = Effect.fnUntraced(function* (cache: PluginCache<readonly string[]>, context: CreateNodesContext, output: Option.Option<string>, chain: readonly string[], hash: string, file: string) {
    const root = dirname(dirname(file));
    const key = `${hash}${file}`;
    const packages = cache.get(key) ?? (yield* secluded(read(context.workspaceRoot, file)));
    cache.set(key, packages);
    const stated = yield* manifested(context.workspaceRoot, root);
    const { exec } = getPackageManagerCommand(detectPackageManager(context.workspaceRoot));
    const outside = chain.filter((config) => !config.startsWith(`${root}/`));
    const project: ProjectConfiguration = { root, metadata: { technologies: ['storybook'] }, targets: aggregated(targets(root, packages, outside, 'production' in getNamedInputs(root, context) ? '^production' : '^default', output, exec), stated, 'storybook') };
    return { projects: { [root]: project } };
});

const nodes = Effect.fnUntraced(function* (files: readonly string[], options: typeof Options.Encoded | undefined, context: CreateNodesContext) {
    const settings = yield* Schema.decodeEffect(Options)(options ?? {});
    const mains = files.filter((file) => existsSync(join(context.workspaceRoot, dirname(dirname(file)), 'package.json')));
    const roots = mains.map((file) => dirname(dirname(file)));
    const chains = roots.map((root) => extended(context.workspaceRoot, root));
    const lock = getLockFileName(detectPackageManager(context.workspaceRoot));
    const cache = new PluginCache<readonly string[]>(join(workspaceDataDirectory, `storybook-${hashObject(options ?? {})}.hash`));
    const hashes = yield* Effect.promise(() =>
        calculateHashesForCreateNodes(
            roots,
            settings,
            context,
            chains.map((chain) => [lock, ...chain]),
        ),
    );
    return yield* Effect.ensuring(
        Effect.tryPromise({
            try: () => createNodesFromFiles((file, _options, _context, index) => run(Effect.catchCause(inferred(cache, context, Option.fromUndefinedOr(settings.outputPath), chains[index] ?? [], `${hashes[index]}`, file), rendered)), mains, options, context),
            catch: (cause) => (cause instanceof AggregateCreateNodesError ? cause : new AggregateCreateNodesError([[null, new Cause.UnknownError(cause, `${PLUGIN} infers no project`)]], [])),
        }),
        Effect.sync(() => cache.writeToDisk()),
    );
});

// --- [COMPOSITION] ---------------------------------------------------------------------

const createNodes: CreateNodes<typeof Options.Encoded> = ['**/.storybook/main.{js,ts,jsx,tsx,mjs,mts,cjs,cts}', (files, options, context) => run(nodes(files, options, context))];

// --- [EXPORTS] -------------------------------------------------------------------------

export { createNodes };

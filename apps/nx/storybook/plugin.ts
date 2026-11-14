import { existsSync } from 'node:fs';
import { createRequire, findPackageJSON } from 'node:module';
import { dirname, isAbsolute, join, relative } from 'node:path';
import { env } from 'node:process';
import { pathToFileURL } from 'node:url';
import { layer } from '@effect/platform-node/NodeServices';
import { AggregateCreateNodesError, type CreateNodes, type CreateNodesContext, createNodesFromFiles, detectPackageManager, getPackageManagerCommand, normalizePath, type ProjectConfiguration, type TargetConfiguration } from '@nx/devkit';
import { calculateHashesForCreateNodes, getLockFileName, getNamedInputs, hashObject, PluginCache, quoteShellArg, workspaceDataDirectory } from '@nx/devkit/internal';
import { walkTsconfigExtendsChain } from '@nx/js/internal';
import web from '@nx/web/package.json' with { type: 'json' };
import { Array, Cause, Effect, FileSystem, Option, Predicate, pipe, Schema } from 'effect';

// --- [CONSTANTS] -----------------------------------------------------------------------

const MODE = { NODE_ENV: 'production' } as const;
const SHARED = '@rasm/nx-distribution';

// --- [MODELS] --------------------------------------------------------------------------

const Placed = Schema.Struct({ outputPath: Schema.optionalKey(Schema.String) }).annotate({ description: 'outputPath: workspace folder storybook build writes under as <outputPath>/storybook/<project name>, absent writes storybook-static in the project' });

// --- [OPERATIONS] ----------------------------------------------------------------------

const refused = (subject: string, detail: string) => (cause?: unknown) => new Cause.UnknownError(cause, `${subject}: ${detail}`);

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
        catch: refused(file, 'resolves no storybook package'),
    });
    const { framework, addons = [] } = yield* Effect.tryPromise({ try: () => loadMainConfig({ configDir: dirname(main), cwd: dirname(dirname(main)), skipCache: true }), catch: refused(file, 'does not load as a Storybook main config') });
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

const targets = (root: string, packages: readonly string[], chain: readonly string[], upstream: string, output: Option.Option<string>, exec: string): Record<string, TargetConfiguration> => {
    const inputs = ['default', upstream, ...chain.map((at) => join('{workspaceRoot}', at)), { externalDependencies: Array.dedupe(['storybook', ...packages]) }];
    const written = Option.getOrElse(output, () => join(root, 'storybook-static'));
    return {
        'build--storybook': {
            command: ['storybook', 'build', ...Option.match(output, { onNone: () => [], onSome: (at) => ['--output-dir', relative(root, at)] })].map(quoteShellArg).join(' '),
            options: { cwd: root },
            dependsOn: ['codegen--*', '^codegen'],
            cache: true,
            inputs,
            outputs: [join('{workspaceRoot}', written)],
            metadata: { description: 'Builds the static Storybook of the project stories', help: { command: `${exec} storybook build --help`, example: { args: ['--test'] } } },
        },
        'preview--storybook': {
            executor: `${web.name}:file-server`,
            dependsOn: ['build--storybook'],
            continuous: true,
            readyWhen: { signal: true },
            inputs,
            options: { buildTarget: 'build--storybook', staticFilePath: written, cors: false },
            metadata: { description: `Serves the static Storybook build--storybook writes to its own origin through the ${web.name} file server` },
        },
        'serve--storybook': {
            command: 'storybook dev',
            options: { cwd: root },
            continuous: true,
            readyWhen: { port: '{options.port ?? env.SBCONFIG_PORT ?? env.PORT ?? auto}', host: '{env.SBCONFIG_HOSTNAME ?? options.host ?? localhost}' },
            inputs,
            metadata: { description: 'Serves the project stories from the Storybook development server', help: { command: `${exec} storybook dev --help`, example: { args: ['--port=<port>'] } } },
        },
        'clean--storybook': { executor: `${SHARED}:clean`, options: { targets: ['build--storybook'] }, metadata: { description: 'Removes the outputs build--storybook declares' } },
        build: { dependsOn: ['build--*'] },
        preview: { dependsOn: ['preview--*'] },
        serve: { dependsOn: ['serve--*'] },
        clean: { dependsOn: ['clean--*'] },
    };
};

const inferred = Effect.fnUntraced(function* (cache: PluginCache<readonly string[]>, context: CreateNodesContext, output: Option.Option<string>, chain: readonly string[], hash: string, file: string) {
    const root = dirname(dirname(file));
    const key = `${hash}_${file}`;
    const packages = cache.get(key) ?? (yield* secluded(read(context.workspaceRoot, file)));
    cache.set(key, packages);
    const { exec } = getPackageManagerCommand(detectPackageManager(context.workspaceRoot));
    const outside = chain.filter((config) => !config.startsWith(`${root}/`));
    const project: ProjectConfiguration = { root, metadata: { technologies: ['storybook'] }, targets: targets(root, packages, outside, 'production' in getNamedInputs(root, context) ? '^production' : '^default', output, exec) };
    return { projects: { [root]: project } };
});

const nodes = Effect.fnUntraced(function* (files: readonly string[], options: unknown, context: CreateNodesContext) {
    const { outputPath } = yield* Schema.decodeEffect(Placed)({ ...context.nxJsonConfiguration.pluginsConfig?.[SHARED], ...(Predicate.isObject(options) ? options : {}) });
    const mains = files.filter((file) => existsSync(join(context.workspaceRoot, dirname(dirname(file)), 'package.json')));
    const roots = mains.map((file) => dirname(dirname(file)));
    const chains = roots.map((root) => extended(context.workspaceRoot, root));
    const lock = getLockFileName(detectPackageManager(context.workspaceRoot));
    const output = Option.map(Option.fromUndefinedOr(outputPath), (at) => join(at, 'storybook', '{projectName}'));
    const cache = new PluginCache<readonly string[]>(join(workspaceDataDirectory, `storybook-${hashObject({ options })}.hash`));
    const hashes = yield* Effect.promise(() =>
        calculateHashesForCreateNodes(
            roots,
            { options },
            context,
            chains.map((chain) => [lock, ...chain]),
        ),
    );
    return yield* Effect.ensuring(
        Effect.tryPromise({
            try: () => createNodesFromFiles((file, _options, _context, index) => run(inferred(cache, context, output, chains[index] ?? [], `${hashes[index]}`, file)), mains, options, context),
            catch: (cause) => (cause instanceof AggregateCreateNodesError ? cause : new AggregateCreateNodesError([[null, new Cause.UnknownError(cause, 'createNodes failed outside a file')]], [])),
        }),
        Effect.sync(() => cache.writeToDisk()),
    );
});

// --- [COMPOSITION] ---------------------------------------------------------------------

const createNodes: CreateNodes = ['**/.storybook/main.{js,ts,jsx,tsx,mjs,mts,cjs,cts}', (files, options, context) => run(nodes(files, options, context))];

// --- [EXPORTS] -------------------------------------------------------------------------

export { createNodes };

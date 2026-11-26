import { existsSync } from 'node:fs';
import { createRequire, registerHooks } from 'node:module';
import { dirname, join } from 'node:path';
import process from 'node:process';
import { fileURLToPath, pathToFileURL } from 'node:url';
import { parentPort, workerData } from 'node:worker_threads';
import { nxReadySignal } from '@nx/vite/plugins/nx-ready-signal.plugin';
import { Array, Cause, Effect, Filter, MutableRef, Option, Predicate, Schema, Struct } from 'effect';
import type { Plugin, resolveConfig, resolveEnvPrefix, UserConfig } from 'vite';

// --- [MODELS] --------------------------------------------------------------------------

const Task = Schema.Struct({ at: Schema.String, outDir: Schema.optional(Schema.String) }).annotate({ description: 'The absolute path of a Vite config and the outDir vite build takes on its command line' });

// --- [OPERATIONS] ----------------------------------------------------------------------

const read = Effect.fnUntraced(function* ({ at, outDir }: typeof Task.Type) {
    const vite = yield* Effect.promise((): Promise<{ readonly resolveConfig: typeof resolveConfig; readonly resolveEnvPrefix: typeof resolveEnvPrefix }> => import(pathToFileURL(createRequire(at).resolve('vite')).href));
    const imported = new Set<string>();
    const hooks = {
        resolve: (specifier, context, nextResolve) => {
            const module = nextResolve(specifier, context);
            imported.add(module.url);
            return module;
        },
    } satisfies Parameters<typeof registerHooks>[0];
    yield* Effect.sync(() => registerHooks(hooks));
    yield* Effect.sync(() => Object.assign(process, { cwd: () => dirname(at) }));
    const resolved = (command: Parameters<typeof resolveConfig>[1], inline: UserConfig, mode?: string) => {
        const stated = MutableRef.make(false);
        const capture = {
            name: 'nx-vite-stated-server',
            enforce: 'post',
            config: {
                order: 'post',
                handler: ({ server, preview }) => {
                    MutableRef.set(stated, server !== undefined || preview !== undefined);
                },
            },
        } satisfies Plugin;
        return Effect.tryPromise({
            try: () =>
                vite.resolveConfig({ configFile: at, plugins: [capture], ...inline, ...(outDir !== undefined && { build: { outDir } }) }, command, mode, mode, false, undefined, (plugins) => {
                    plugins.splice(0, plugins.length, ...plugins.map((plugin) => Struct.omit(plugin, ['configResolved'])));
                }),
            catch: (cause) => new Cause.UnknownError(cause, `${at} does not load for vite ${command}`),
        }).pipe(Effect.map((config) => ({ config, stated: MutableRef.get(stated) })));
    };
    const { config, stated } = yield* resolved('build', {}, 'production');
    const built = config.builder === undefined ? Array.fromNullishOr(config.environments[config.build.ssr === false ? 'client' : 'ssr']) : Object.values(config.environments);
    const library = config.build.lib !== false;
    const entry = existsSync(join(config.root, 'index.html'));
    const buildable = library || entry || config.builder !== undefined || built.some(({ input, build }) => input !== undefined || Predicate.isString(build.ssr) || build.rolldownOptions.input !== undefined);
    const signals = config.plugins.some(({ name }) => name === nxReadySignal().name);
    const implied = (!library && (entry || signals)) || stated;
    const serve = yield* Effect.when(resolved('serve', { logLevel: 'silent' }), Effect.succeed(buildable && !implied)).pipe(Effect.orElseSucceed(Option.none));
    return {
        root: config.root,
        library,
        buildable,
        signals,
        served: buildable && (implied || Option.exists(serve, (held) => held.stated)),
        written: built.map(({ build }) => build.outDir),
        env: { directory: config.envDir === false ? null : config.envDir, prefixes: vite.resolveEnvPrefix(Struct.pick(config, ['envPrefix'])) },
        dependencies: [...config.configFileDependencies, ...Option.toArray(serve).flatMap((held) => held.config.configFileDependencies), ...Array.filterMap(imported, Filter.try(fileURLToPath))],
    };
});

// --- [COMPOSITION] ---------------------------------------------------------------------

parentPort?.postMessage(await Effect.runPromise(Effect.flatMap(Schema.decodeUnknownEffect(Task)(workerData), read)));

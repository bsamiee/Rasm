import { createRequire } from 'node:module';
import { dirname } from 'node:path';
import process from 'node:process';
import { pathToFileURL } from 'node:url';
import { parentPort, workerData } from 'node:worker_threads';
import { nxReadySignal } from '@nx/vite/plugins/nx-ready-signal.plugin';
import { Array, Effect, MutableRef, Schema, Struct } from 'effect';
import type { Plugin, resolveConfig, UserConfig } from 'vite';

// --- [MODELS] --------------------------------------------------------------------------

const Task = Schema.Struct({ at: Schema.String, command: Schema.Literals(['build', 'serve']), outDir: Schema.optional(Schema.String) }).annotate({ description: 'The absolute path of a Vite config, the Vite command it resolves for, and the outDir vite build takes on its command line' });

// --- [OPERATIONS] ----------------------------------------------------------------------

const read = Effect.fnUntraced(function* ({ at, command, outDir }: typeof Task.Type) {
    const vite = yield* Effect.promise((): Promise<{ readonly resolveConfig: typeof resolveConfig }> => import(pathToFileURL(createRequire(at).resolve('vite')).href));
    const stated = MutableRef.make(false);
    const capture = {
        name: 'nx-vite-stated-server',
        enforce: 'post',
        config: {
            order: 'post',
            handler: ({ server, preview }: UserConfig) => {
                MutableRef.set(stated, server !== undefined || preview !== undefined);
            },
        },
    } as const;
    const mode = command === 'build' ? 'production' : 'development';
    yield* Effect.sync(() => Object.assign(process, { cwd: () => dirname(at) }));
    const config = yield* Effect.promise(() =>
        vite.resolveConfig({ configFile: at, plugins: [capture], ...(command === 'serve' && { logLevel: 'silent' }), ...(outDir !== undefined && { build: { outDir } }) }, command, mode, mode, false, undefined, (plugins) => {
            plugins.splice(0, plugins.length, ...plugins.map(Struct.omit<Plugin, ['configResolved']>(['configResolved'])));
        }),
    );
    const built = config.builder === undefined ? Array.fromNullishOr(config.environments[config.build.ssr === false ? 'client' : 'ssr']) : Object.values(config.environments);
    return {
        root: config.root,
        library: config.build.lib !== false,
        built: config.builder !== undefined || built.some(({ input, build }) => input !== undefined || typeof build.ssr === 'string' || build.rolldownOptions.input !== undefined),
        signals: config.plugins.some(({ name }) => name === nxReadySignal().name),
        stated: MutableRef.get(stated),
        written: built.map(({ build }) => build.outDir),
        dependencies: config.configFileDependencies,
    };
});

// --- [COMPOSITION] ---------------------------------------------------------------------

parentPort?.postMessage(await Effect.runPromise(Effect.flatMap(Schema.decodeUnknownEffect(Task)(workerData), read)));

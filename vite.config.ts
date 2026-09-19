// --- [IMPORTS] -------------------------------------------------------------------------

import { NodeServices } from '@effect/platform-node';
import { Array, Effect, FileSystem, Path, Schema, String } from 'effect';
import { defaultClientConditions, defaultServerConditions, defaultServerMainFields, type UserConfig, type UserConfigFnPromise } from 'vite';

// --- [CONSTANTS] -----------------------------------------------------------------------

const _HOST_MODULE = /^adobe:/u;

// --- [MODELS] --------------------------------------------------------------------------

const _Package = Schema.fromJsonString(Schema.Struct({ main: Schema.String }));
const _Plugin = Schema.Struct({ plugin: Schema.Struct({ main: Schema.String }) });

// --- [CONFIGURATION] -------------------------------------------------------------------

const _config = Effect.gen(function* () {
    const fs = yield* FileSystem.FileSystem;
    const path = yield* Path.Path;
    const project = path.resolve('.');
    const { main } = yield* Schema.decodeEffect(_Package)(yield* fs.readFileString(path.join(project, 'package.json')));
    const { plugin } = yield* Effect.flatMap(
        Effect.promise((): Promise<unknown> => import(path.join(project, 'uxp.config.ts'))),
        Schema.decodeUnknownEffect(_Plugin),
    );
    return {
        resolve: { conditions: Array.intersection(defaultClientConditions, defaultServerConditions), mainFields: [...defaultServerMainFields] },
        build: {
            outDir: path.join(import.meta.dirname, '.artifacts', path.relative(import.meta.dirname, project)),
            emptyOutDir: true,
            lib: { entry: main, formats: ['cjs'], fileName: (): string => plugin.main },
            minify: false,
            sourcemap: true,
            reportCompressedSize: false,
            rolldownOptions: { platform: 'neutral', external: _HOST_MODULE, output: { paths: String.replace(_HOST_MODULE, ''), dynamicImportInCjs: false } },
        },
    } satisfies UserConfig;
});

const userConfig: UserConfigFnPromise = (): Promise<UserConfig> => Effect.runPromise(_config.pipe(Effect.orDie, Effect.provide(NodeServices.layer)));

// --- [EXPORTS] -------------------------------------------------------------------------

export default userConfig;

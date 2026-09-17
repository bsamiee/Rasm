// --- [IMPORTS] -------------------------------------------------------------------------

import { NodeServices } from '@effect/platform-node';
import { Effect, FileSystem, Path, Schema, String } from 'effect';
import { defineConfig, type UserConfig, type UserConfigFnPromise } from 'vite';

// --- [CONSTANTS] -----------------------------------------------------------------------

const _ROOT = import.meta.dirname;
const _HOST_MODULE = /^adobe:/u;

// --- [MODELS] --------------------------------------------------------------------------

const _Package = Schema.fromJsonString(Schema.Struct({ main: Schema.String }));

// --- [CONFIGURATION] -------------------------------------------------------------------

const _config = Effect.gen(function* () {
    const fs = yield* FileSystem.FileSystem;
    const path = yield* Path.Path;
    const project = path.resolve('.');
    const { main } = yield* Schema.decodeEffect(_Package)(yield* fs.readFileString(path.join(project, 'package.json')));
    return {
        build: {
            outDir: path.join(_ROOT, '.artifacts', path.relative(_ROOT, project)),
            emptyOutDir: true,
            lib: { entry: main, formats: ['cjs'], fileName: (_format, entryName): string => `${entryName}.js` },
            minify: false,
            sourcemap: true,
            reportCompressedSize: false,
            rolldownOptions: { external: _HOST_MODULE, output: { paths: String.replace(_HOST_MODULE, '') } },
        },
    } satisfies UserConfig;
});

const userConfig: UserConfigFnPromise = defineConfig((): Promise<UserConfig> => Effect.runPromise(_config.pipe(Effect.orDie, Effect.provide(NodeServices.layer))));

// --- [EXPORTS] -------------------------------------------------------------------------

export default userConfig;

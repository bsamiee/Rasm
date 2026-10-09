import { NodeServices } from '@effect/platform-node';
import { Array, Effect, FileSystem, Path, Schema, String } from 'effect';
import { defaultClientConditions, defaultServerConditions, defaultServerMainFields, type UserConfig } from 'vite';

// --- [CONSTANTS] -----------------------------------------------------------------------

const _HOST_MODULE = /^adobe:/u;

// --- [COMPOSITION] ---------------------------------------------------------------------

const config = Effect.gen(function* () {
    const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
    const project = path.resolve('.');
    const entry = Schema.Struct({ main: Schema.String });
    const [{ main }, { plugin }] = yield* Effect.all(
        [
            Effect.flatMap(fs.readFileString(path.join(project, 'package.json')), Schema.decodeEffect(Schema.fromJsonString(entry))),
            path.toFileUrl(path.join(project, 'uxp.config.ts')).pipe(
                Effect.flatMap((url) => Effect.promise((): Promise<unknown> => import(url.href))),
                Effect.flatMap(Schema.decodeUnknownEffect(Schema.Struct({ plugin: entry }))),
            ),
        ],
        { concurrency: 'unbounded' },
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
}).pipe(Effect.provide(NodeServices.layer), Effect.runPromise);

// --- [EXPORTS] -------------------------------------------------------------------------

// biome-ignore lint/style/noDefaultExport: Vite loads its configuration from the module default export.
export default config;

import { Buffer } from 'node:buffer';
import { findPackageJSON } from 'node:module';
import { dirname, isAbsolute, join, normalize, relative, resolve } from 'node:path';
import process from 'node:process';
import { buffer } from 'node:stream/consumers';
import { pathToFileURL } from 'node:url';
import { createZipArchive, ZipEntry } from 'node:zlib';
import { run } from '@rasm/process';
import { Cause, Effect, FileSystem, Record, Schema } from 'effect';
import type { ChildProcessSpawner } from 'effect/process';
import { type Plugin, type Rolldown, searchForWorkspaceRoot } from 'vite';

// --- [CONSTANTS] -----------------------------------------------------------------------

const READ = /\bnew\s+URL\s*\(\s*(?:"(?<path>[^"]+)"|'(?<path>[^']+)'|`(?<path>[^`$]+)`)\s*,\s*import\.meta\.url\s*\)|\.join\s*\(\s*import\.meta\.dirname\s*,\s*(?:"(?<path>[^"]+)"|'(?<path>[^']+)'|`(?<path>[^`$]+)`)\s*\)/gu;

// --- [MODELS] --------------------------------------------------------------------------

const Manifest = Schema.fromJsonString(Schema.Struct({ name: Schema.String }));

// --- [OPERATIONS] ----------------------------------------------------------------------

const singleExecutable = Effect.fnUntraced(function* (entry: string, name: string) {
    const [context, fs] = yield* Effect.all([Effect.context<ChildProcessSpawner.ChildProcessSpawner>(), FileSystem.FileSystem]);
    const owner = yield* Effect.fromNullishOr(findPackageJSON(pathToFileURL(entry))).pipe(Effect.flatMap(fs.readFileString), Effect.flatMap(Schema.decodeEffect(Manifest)));
    const root = searchForWorkspaceRoot(dirname(entry));
    return {
        name: 'single-executable',
        config: () => ({
            define: { __dirname: 'import.meta.dirname', __filename: 'import.meta.filename' },
            build: {
                ssr: true,
                emitAssets: true,
                minify: 'oxc',
                sourcemap: 'hidden',
                rolldownOptions: {
                    input: { [name]: entry },
                    output: {
                        entryFileNames: '[name].mjs',
                        codeSplitting: false,
                        minify: { compress: true, mangle: false, codegen: { removeWhitespace: true } },
                        sourcemapExcludeSources: true,
                        sourcemapPathTransform: (source, map) => relative(root, resolve(dirname(map), source)),
                    },
                },
            },
            ssr: { noExternal: true },
        }),
        generateBundle(this: Rolldown.PluginContext, { dir = '' }, output): Promise<void> {
            const files = Object.values(output);
            const reads = Record.fromEntries(
                files
                    .values()
                    .flatMap((file) => (file.type === 'chunk' ? Object.entries(file.modules) : []))
                    .filter(([id]) => isAbsolute(id))
                    .flatMap(([id, { code }]) => (code ?? '').matchAll(READ).map(({ groups: { path = '' } = {} }) => [normalize(path), resolve(dirname(id), path)] as const)),
            );
            const written = files.map((file) => (file.type === 'chunk' ? ([file.fileName, Buffer.from(file.code), 'store'] as const) : ([file.fileName, typeof file.source === 'string' ? Buffer.from(file.source) : file.source, 'zstd'] as const)));
            return Effect.all(Record.map(reads, (file) => fs.readFile(file))).pipe(
                Effect.map((read) => [...written, ...Record.collect(read, (fileName, source) => [fileName, source, 'zstd'] as const)]),
                Effect.flatMap(Effect.forEach(([fileName, source, method]) => Effect.tryPromise({ try: () => ZipEntry.create(fileName, source, { method }), catch: (cause) => new Cause.UnknownError(cause, `${fileName} failed to compress into ${name}.zip`) }), { concurrency: 'unbounded' })),
                Effect.flatMap((entries) => Effect.tryPromise({ try: () => buffer(createZipArchive(entries)), catch: (cause) => new Cause.UnknownError(cause, `${name}.zip failed to serialize its ${entries.length} entries`) })),
                Effect.map((archive) => [
                    { fileName: `${name}.zip`, source: archive },
                    {
                        fileName: 'main.mjs',
                        source: `import { enableCompileCache, flushCompileCache } from 'node:module';\nimport { tmpdir } from 'node:os';\nimport { join } from 'node:path';\nimport { env } from 'node:process';\nenableCompileCache(join(env.NODE_COMPILE_CACHE ?? join(tmpdir(), 'node-compile-cache'), ${JSON.stringify(owner.name)}));\nawait import(${JSON.stringify(`./${name}.mjs`)});\nflushCompileCache();\n`,
                    },
                    { fileName: 'sea.json', source: JSON.stringify({ main: join(dir, 'main.mjs'), mainFormat: 'module', output: join(dir, name), useVfs: true, vfsArchive: join(dir, `${name}.zip`), execArgv: ['--disable-warning=ExperimentalWarning'], execArgvExtension: 'none' }) },
                ]),
                Effect.flatMap(Effect.forEach((asset) => Effect.sync(() => this.emitFile({ type: 'asset', ...asset })), { discard: true })),
                Effect.runPromiseWith(context),
            );
        },
        writeBundle({ dir = '' }): Promise<void> {
            const executable = join(dir, name);
            return Effect.forEach(
                [
                    [process.execPath, '--build-sea', join(dir, 'sea.json')],
                    ['strip', '-x', executable],
                    ['codesign', '--force', '--sign', '-', executable],
                ] as const,
                (argv) => run(argv),
                { discard: true },
            ).pipe(Effect.runPromiseWith(context));
        },
    } satisfies Plugin;
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { singleExecutable };

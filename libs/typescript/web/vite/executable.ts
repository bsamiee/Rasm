import { Buffer } from 'node:buffer';
import { dirname, isAbsolute, join, normalize, resolve } from 'node:path';
import process from 'node:process';
import { buffer } from 'node:stream/consumers';
import { createZipArchive, ZipEntry } from 'node:zlib';
import { run } from '@rasm/process';
import { Cause, Effect, FileSystem, Record } from 'effect';
import type { ChildProcessSpawner } from 'effect/process';
import type { Plugin, Rolldown } from 'vite';

// --- [CONSTANTS] -----------------------------------------------------------------------

const READ = /\bnew\s+URL\s*\(\s*(?:"(?<path>[^"]+)"|'(?<path>[^']+)'|`(?<path>[^`$]+)`)\s*,\s*import\.meta\.url\s*\)|\.join\s*\(\s*import\.meta\.dirname\s*,\s*(?:"(?<path>[^"]+)"|'(?<path>[^']+)'|`(?<path>[^`$]+)`)\s*\)/gu;

// --- [OPERATIONS] ----------------------------------------------------------------------

const singleExecutable = Effect.fnUntraced(function* (entry: string, name: string) {
    const [context, fs] = yield* Effect.all([Effect.context<ChildProcessSpawner.ChildProcessSpawner>(), FileSystem.FileSystem]);
    return {
        name: 'single-executable',
        config: () => ({
            define: { __dirname: 'import.meta.dirname', __filename: 'import.meta.filename' },
            build: { ssr: true, emitAssets: true, rolldownOptions: { input: { [name]: entry }, output: { entryFileNames: '[name].mjs', codeSplitting: false } } },
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
                    { fileName: 'main.mjs', source: `import { enableCompileCache } from 'node:module';\nenableCompileCache();\nawait import(${JSON.stringify(`./${name}.mjs`)});\n` },
                    { fileName: 'sea.json', source: JSON.stringify({ main: join(dir, 'main.mjs'), mainFormat: 'module', output: join(dir, name), useVfs: true, vfsArchive: join(dir, `${name}.zip`), execArgv: ['--disable-warning=ExperimentalWarning'], execArgvExtension: 'none' }) },
                ]),
                Effect.flatMap(Effect.forEach((asset) => Effect.sync(() => this.emitFile({ type: 'asset', ...asset })), { discard: true })),
                Effect.runPromiseWith(context),
            );
        },
        writeBundle({ dir = '' }): Promise<void> {
            return run([process.execPath, '--build-sea', join(dir, 'sea.json')]).pipe(Effect.andThen(run(['codesign', '--force', '--sign', '-', join(dir, name)])), Effect.asVoid, Effect.runPromiseWith(context));
        },
    } satisfies Plugin;
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { singleExecutable };

import { readFileSync } from 'node:fs';
import { dirname, isAbsolute, join, normalize, resolve } from 'node:path';
import process from 'node:process';
import { run } from '@rasm/process';
import { Effect, Record } from 'effect';
import type { ChildProcessSpawner } from 'effect/process';
import type { Plugin, Rolldown } from 'vite';

// --- [CONSTANTS] -----------------------------------------------------------------------

const READ = /\bnew\s+URL\s*\(\s*(?:"(?<path>[^"]+)"|'(?<path>[^']+)'|`(?<path>[^`$]+)`)\s*,\s*import\.meta\.url\s*\)|\.join\s*\(\s*import\.meta\.dirname\s*,\s*(?:"(?<path>[^"]+)"|'(?<path>[^']+)'|`(?<path>[^`$]+)`)\s*\)/gu;

// --- [OPERATIONS] ----------------------------------------------------------------------

const helper = Effect.fnUntraced(function* (entry: string, name: string) {
    const context = yield* Effect.context<ChildProcessSpawner.ChildProcessSpawner>();
    return {
        name: 'helper',
        config: () => ({ define: { __dirname: 'import.meta.dirname', __filename: 'import.meta.filename' }, build: { ssr: true, emitAssets: true, rolldownOptions: { input: { [name]: entry }, output: { entryFileNames: '[name].mjs', codeSplitting: false } } }, ssr: { noExternal: true } }),
        generateBundle(this: Rolldown.PluginContext, { dir = '' }, output): void {
            const files = Object.values(output);
            const modules = files.flatMap((file) => (file.type === 'chunk' ? Object.entries(file.modules) : [])).filter(([id]) => isAbsolute(id));
            const read = Record.fromEntries(modules.values().flatMap(([id, { code }]) => (code ?? '').matchAll(READ).map(({ groups: { path = '' } = {} }) => [normalize(path), resolve(dirname(id), path)] as const)));
            Record.toEntries(read).forEach(([fileName, file]) => {
                this.emitFile({ type: 'asset', fileName, source: readFileSync(file) });
            });
            const assets = Record.fromIterableWith([...Record.keys(read), ...files.flatMap((file) => (file.type === 'asset' ? [file.fileName] : []))], (fileName) => [fileName, join(dir, fileName)]);
            this.emitFile({ type: 'asset', fileName: 'sea.json', source: JSON.stringify({ main: join(dir, `${name}.mjs`), mainFormat: 'module', output: join(dir, name), useVfs: true, disableExperimentalSEAWarning: true, assets }) });
        },
        writeBundle({ dir = '' }): Promise<void> {
            return run([process.execPath, '--build-sea', join(dir, 'sea.json')]).pipe(Effect.andThen(run(['codesign', '--force', '--sign', '-', join(dir, name)])), Effect.asVoid, Effect.runPromiseWith(context));
        },
    } satisfies Plugin;
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { helper };

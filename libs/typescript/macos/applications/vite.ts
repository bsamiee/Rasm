import { readFileSync } from 'node:fs';
import { dirname, isAbsolute, join, normalize, resolve } from 'node:path';
import process from 'node:process';
import { run } from '@rasm/process';
import { Effect, Record } from 'effect';
import type { ChildProcessSpawner } from 'effect/process';
import type { Plugin, Rolldown } from 'vite';

// --- [CONSTANTS] -----------------------------------------------------------------------

const READ = /\bnew\s+URL\s*\(\s*(?<path>"[^"]+"|'[^']+'|`[^`$]+`)\s*,\s*import\.meta\.url\s*\)|\.join\s*\(\s*import\.meta\.dirname\s*,\s*(?<path>"[^"]+"|'[^']+'|`[^`$]+`)\s*\)/gu;

// --- [OPERATIONS] ----------------------------------------------------------------------

const helper = Effect.fnUntraced(function* (entry: string, name: string) {
    const context = yield* Effect.context<ChildProcessSpawner.ChildProcessSpawner>();
    let outDir = '';
    return {
        name: 'helper',
        config: () => ({ define: { __dirname: 'import.meta.dirname', __filename: 'import.meta.filename' }, build: { ssr: true, emitAssets: true, rolldownOptions: { input: { [name]: entry }, output: { entryFileNames: '[name].mjs', codeSplitting: false } } }, ssr: { noExternal: true } }),
        configResolved({ root, build: options }): void {
            outDir = resolve(root, options.outDir);
        },
        generateBundle(this: Rolldown.PluginContext, _options, output): void {
            const read = Record.fromEntries(
                Object.values(output)
                    .flatMap((file) => (file.type === 'chunk' ? Object.entries(file.modules).filter(([id]) => isAbsolute(id)) : []))
                    .flatMap(([id, { code }]) =>
                        (code ?? '')
                            .matchAll(READ)
                            .map(({ groups: { path = '' } = {} }) => path.slice(1, -1))
                            .map((path) => [normalize(path), resolve(dirname(id), path)] as const)
                            .toArray(),
                    ),
            );
            Record.toEntries(read).forEach(([fileName, file]) => {
                this.emitFile({ type: 'asset', fileName, source: readFileSync(file) });
            });
            const assets = Record.fromIterableWith([...Record.keys(read), ...Object.values(output).flatMap((file) => (file.type === 'asset' ? [file.fileName] : []))], (fileName) => [fileName, join(outDir, fileName)]);
            this.emitFile({ type: 'asset', fileName: 'sea.json', source: JSON.stringify({ main: join(outDir, `${name}.mjs`), mainFormat: 'module', output: join(outDir, name), useVfs: true, disableExperimentalSEAWarning: true, assets }) });
        },
        closeBundle(): Promise<void> {
            return Effect.forEach(
                [
                    [process.execPath, '--build-sea', join(outDir, 'sea.json')],
                    ['codesign', '--force', '--sign', '-', join(outDir, name)],
                ] as const,
                (argv) => run(argv),
                { discard: true },
            ).pipe(Effect.runPromiseWith(context));
        },
    } satisfies Plugin;
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { helper };

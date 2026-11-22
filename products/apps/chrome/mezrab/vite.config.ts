import { join } from 'node:path';
import { layer } from '@effect/platform-node/NodeServices';
import { paint } from '@rasm/mark';
import { checks } from '@rasm/vite';
import { Effect, FileSystem, Record } from 'effect';
import { defineConfig } from 'vite';
import manifest from '#/manifest.json' with { type: 'json' };

// --- [CONSTANTS] -----------------------------------------------------------------------

const MARK = { drawings: [join(import.meta.dirname, 'icons/mark.svg'), join(import.meta.dirname, 'icons/mark-16.svg')], stylesheet: join(import.meta.dirname, 'icon.css') } as const;

// --- [OPERATIONS] ----------------------------------------------------------------------

const assets = Effect.fnUntraced(function* (emitted: (fileName: string) => (source: string | Uint8Array) => string) {
    const fs = yield* FileSystem.FileSystem;
    const [locales, style] = yield* Effect.all([fs.readDirectory(join(import.meta.dirname, '_locales')), fs.readFileString(MARK.stylesheet)]);
    yield* Effect.all(
        [
            ...locales.map((locale) => Effect.map(fs.readFile(join(import.meta.dirname, '_locales', locale, 'messages.json')), emitted(`_locales/${locale}/messages.json`))),
            ...Record.collect(manifest.icons, (width, fileName) => Effect.map(paint(MARK.drawings, style, { width: Number(width), padding: width === '128' ? 16 : undefined }), emitted(fileName))),
        ],
        { concurrency: 'unbounded', discard: true },
    );
});

// --- [COMPOSITION] ---------------------------------------------------------------------

export default defineConfig({
    build: { target: `chrome${manifest.minimum_chrome_version}`, assetsInlineLimit: () => true, rolldownOptions: { input: 'service-worker.ts', output: { entryFileNames: manifest.background.service_worker } } },
    plugins: [
        checks(),
        {
            name: 'manifest',
            buildStart() {
                this.emitFile({ type: 'asset', fileName: 'manifest.json', source: JSON.stringify(manifest) });
                return assets((fileName) => (source) => this.emitFile({ type: 'asset', fileName, source })).pipe(Effect.provide(layer), Effect.runPromise);
            },
        },
    ],
});

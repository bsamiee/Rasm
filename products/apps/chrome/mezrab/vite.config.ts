import { join } from 'node:path';
import { layer } from '@effect/platform-node/NodeServices';
import { paint } from '@rasm/mark';
import { checks } from '@rasm/vite';
import { rgb } from 'culori';
import { Cause, Effect, FileSystem, Option, Record, Schema } from 'effect';
import { defineConfig } from 'vite';
import manifest from '#/manifest.json' with { type: 'json' };

// --- [CONSTANTS] -----------------------------------------------------------------------

const DRAWINGS = [join(import.meta.dirname, 'icons/mark.svg'), join(import.meta.dirname, 'icons/mark-16.svg')] as const;

// --- [MODELS] --------------------------------------------------------------------------

const Branded = Schema.fromJsonString(Schema.Struct({ config: Schema.Struct({ brand: Schema.String }) }));

// --- [OPERATIONS] ----------------------------------------------------------------------

const assets = Effect.fnUntraced(function* (emitted: (fileName: string) => (source: string | Uint8Array) => string) {
    const fs = yield* FileSystem.FileSystem;
    const [locales, { config }] = yield* Effect.all([fs.readDirectory(join(import.meta.dirname, '_locales')), Effect.flatMap(fs.readFileString(join(import.meta.dirname, 'package.json')), Schema.decodeEffect(Branded))]);
    const color = yield* Effect.fromOption(Option.fromNullishOr(rgb(config.brand)), () => new Cause.NoSuchElementError(`package.json config.brand ${config.brand} is no CSS color`));
    yield* Effect.all(
        [
            ...locales.map((locale) => Effect.map(fs.readFile(join(import.meta.dirname, '_locales', locale, 'messages.json')), emitted(`_locales/${locale}/messages.json`))),
            ...Record.collect(manifest.icons, (width, fileName) => Effect.map(paint(DRAWINGS, { color }, { width: Number(width), padding: width === '128' ? 16 : undefined }), emitted(fileName))),
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

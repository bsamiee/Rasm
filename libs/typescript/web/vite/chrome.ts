import { join, parse } from 'node:path';
import { layer } from '@effect/platform-node/NodeServices';
import { paint } from '@rasm/mark';
import { rgb } from 'culori';
import { Array, Cause, Effect, FileSystem, Option, Predicate, Record, Schema } from 'effect';
import type { Rolldown, UserConfig, UserConfigFnPromise } from 'vite';
import { checks } from '#/checks';

// --- [CONSTANTS] -----------------------------------------------------------------------

const STORE = { width: 128, padding: 16 } as const;
const GRID = /^mark-\d+\.svg$/u;

// --- [MODELS] --------------------------------------------------------------------------

const Manifest = Schema.fromJsonString(
    Schema.Struct({
        minimum_chrome_version: Schema.String,
        icons: Schema.Record(Schema.String, Schema.String),
        background: Schema.Struct({ service_worker: Schema.String }),
        action: Schema.optionalKey(Schema.Struct({ default_popup: Schema.optionalKey(Schema.String) })),
        side_panel: Schema.optionalKey(Schema.Struct({ default_path: Schema.String })),
        options_ui: Schema.optionalKey(Schema.Struct({ page: Schema.String })),
    }),
).annotate({ description: 'The manifest.json keys of a Manifest V3 extension its build reads: target release, icon files, service worker, and each page Chrome opens' });

const Brand = Schema.fromJsonString(Schema.Struct({ config: Schema.Struct({ brand: Schema.String }) })).annotate({ description: 'The package.json config.brand color an extension paints its icons and promo tiles in' });

// --- [OPERATIONS] ----------------------------------------------------------------------

const emitted = Effect.fnUntraced(function* (directory: string, source: string, icons: Readonly<Record<string, string>>) {
    const fs = yield* FileSystem.FileSystem;
    const [locales, drawn, { config }] = yield* Effect.all([fs.readDirectory(join(directory, '_locales')), fs.readDirectory(join(directory, 'icons')), Effect.flatMap(fs.readFileString(join(directory, 'package.json')), Schema.decodeEffect(Brand))]);
    const color = yield* Effect.fromOption(Option.fromNullishOr(rgb(config.brand)), () => new Cause.NoSuchElementError(`package.json config.brand ${config.brand} is no CSS color`));
    const drawings = Array.prepend(
        drawn.filter((name) => GRID.test(name)).map((name) => join(directory, 'icons', name)),
        join(directory, 'icons', 'mark.svg'),
    );
    return yield* Effect.all(
        [
            Effect.succeed({ fileName: 'manifest.json', source }),
            ...locales.map((locale) => Effect.map(fs.readFile(join(directory, '_locales', locale, 'messages.json')), (file) => ({ fileName: `_locales/${locale}/messages.json`, source: file }))),
            ...Record.collect(icons, (width, fileName) => Effect.map(paint(drawings, { color }, { width: Number(width), padding: Number(width) === STORE.width ? STORE.padding : undefined }), (file) => ({ fileName, source: file }))),
        ],
        { concurrency: 'unbounded' },
    );
});

// --- [COMPOSITION] ---------------------------------------------------------------------

const chrome = (directory: string): UserConfigFnPromise =>
    Effect.fnUntraced(
        function* () {
            const fs = yield* FileSystem.FileSystem;
            const source = yield* fs.readFileString(join(directory, 'manifest.json'));
            const manifest = yield* Schema.decodeEffect(Manifest)(source);
            const context = yield* Effect.context<Effect.Services<ReturnType<typeof emitted>>>();
            const pages = [manifest.action?.default_popup, manifest.side_panel?.default_path, manifest.options_ui?.page].filter(Predicate.isNotUndefined);
            return {
                build: {
                    target: `chrome${manifest.minimum_chrome_version}`,
                    assetsInlineLimit: () => true,
                    modulePreload: { polyfill: false },
                    rolldownOptions: { input: [`${parse(manifest.background.service_worker).name}.ts`, ...pages].map((entry) => join(directory, entry)), output: { entryFileNames: '[name].js' } },
                },
                plugins: [
                    checks(),
                    {
                        name: 'manifest',
                        generateBundle(this: Rolldown.PluginContext): Promise<void> {
                            return emitted(directory, source, manifest.icons).pipe(Effect.flatMap(Effect.forEach((file) => Effect.sync(() => this.emitFile({ type: 'asset', ...file })))), Effect.asVoid, Effect.runPromiseWith(context));
                        },
                    },
                ],
            } satisfies UserConfig;
        },
        Effect.provide(layer),
        Effect.runPromise,
    );

// --- [EXPORTS] -------------------------------------------------------------------------

export { Brand, chrome };

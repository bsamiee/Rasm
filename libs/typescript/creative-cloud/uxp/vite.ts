import { isBuiltin } from 'node:module';
import { join } from 'node:path';
import { fileURLToPath } from 'node:url';
import { layer } from '@effect/platform-node/NodeServices';
import { TextDecoder, TextDecoderStream, TextEncoder, TextEncoderStream } from '@exodus/bytes/encoding-lite.js';
import { paint } from '@rasm/mark';
import { checks } from '@rasm/vite/checks';
import { Array, Cause, Effect, FileSystem, Option, Record, Result, Schema, String, Struct } from 'effect';
import { applyEdits, type JSONPath, modify } from 'jsonc-parser';
import { defaultClientConditions, defaultServerConditions, type Plugin, type UserConfig } from 'vite';
import { ICON_SIZES, THEMES, UxpManifest } from '#/manifest';

// --- [CONSTANTS] -----------------------------------------------------------------------

const HOST_MODULE = /^adobe:/u;
const SCALES = [1, 2];
const TONES = Array.groupBy(Struct.keys(THEMES), (theme): string => THEMES[theme]);

// --- [MODELS] --------------------------------------------------------------------------

const Harness = Schema.Struct({ default: Schema.Struct({ mcpServers: Schema.Record(Schema.String, Schema.Struct({ url: Schema.optionalKey(Schema.URLFromString) })) }) });

// --- [OPERATIONS] ----------------------------------------------------------------------

const drawIcons = (app: UxpManifest['host']['app'], species: keyof typeof ICON_SIZES, stem: string, drawing: string) =>
    Record.collect(TONES, (tone, theme) => ({
        row: { width: ICON_SIZES[species], height: ICON_SIZES[species], path: `icons/${stem}-${tone}.png`, scale: SCALES, theme, species: [species] },
        files: Effect.forEach(SCALES, (factor) => Effect.map(paint([drawing], { color: UxpManifest.fills[app][Array.headNonEmpty(theme)] }, { width: ICON_SIZES[species] * factor }), (source) => ({ fileName: `icons/${stem}-${tone}@${factor}x.png`, source })), { concurrency: 'unbounded' }),
    }));

// --- [COMPOSITION] ---------------------------------------------------------------------

const pluginConfig = (server: { readonly harness: URL; readonly name: string }): UserConfig => {
    const domain = Effect.tryPromise({ try: (): Promise<unknown> => import(server.harness.href, { with: { type: 'json' } }), catch: (cause) => new Cause.UnknownError(cause, `${fileURLToPath(server.harness)} unread`) }).pipe(
        Effect.flatMap(Schema.decodeUnknownEffect(Harness)),
        Effect.flatMap(({ default: { mcpServers } }) => Effect.fromOption(Option.fromNullishOr(mcpServers[server.name]?.url), () => new Cause.NoSuchElementError(`${fileURLToPath(server.harness)} states no url for MCP server ${server.name}`))),
        Effect.map(({ origin }) => origin.replace(/^http/u, 'ws')),
        Effect.cached,
        Effect.runSync,
    );
    return {
        build: {
            ssr: 'main.ts',
            sourcemap: true,
            target: Record.collect(UxpManifest.engine, (browser, version) => `${browser}${version}`),
            rolldownOptions: {
                platform: 'neutral',
                transform: { inject: Record.map({ TextDecoder, TextDecoderStream, TextEncoder, TextEncoderStream }, (_, name): [string, string] => [fileURLToPath(import.meta.resolve('@exodus/bytes/encoding-lite.js')), name]) },
                output: { format: 'cjs', entryFileNames: '[name].js', paths: String.replace(HOST_MODULE, ''), codeSplitting: false, sourcemapExcludeSources: true },
            },
        },
        environments: { ssr: { resolve: { noExternal: true, builtins: [HOST_MODULE], conditions: Array.intersection(defaultServerConditions, defaultClientConditions) } } },
        plugins: [
            checks(),
            {
                name: 'uxp',
                config: () => domain.pipe(Effect.asVoid, Effect.runPromise),
                resolveId: {
                    order: 'pre',
                    handler(id, importer) {
                        return isBuiltin(id) ? this.error(`${importer ?? id} imports ${id}, a Node builtin the UXP host lacks`) : undefined;
                    },
                },
                generateBundle(_, bundle) {
                    return Effect.gen({ self: this }, function* () {
                        const fs = yield* FileSystem.FileSystem;
                        const { root } = this.environment.config;
                        const source = yield* fs.readFileString(join(root, UxpManifest.file));
                        const { host, entrypoints } = yield* Schema.decodeEffect(UxpManifest.json)(source);
                        const entry = yield* Effect.fromNullishOr(Object.values(bundle).find((chunk) => chunk.type === 'chunk' && chunk.isEntry));
                        const plugin = drawIcons(host.app, 'pluginList', 'plugin', fileURLToPath(import.meta.resolve('#/icons/mark.svg')));
                        const panels = Array.filterMap(entrypoints, ({ type, id }, index) => (type === 'panel' ? Result.succeed({ index, icons: drawIcons(host.app, 'toolbar', id, join(root, 'icons', `${id}.svg`)) }) : Result.failVoid));
                        const edits: readonly (readonly [JSONPath, unknown])[] = [
                            [['main'], entry.fileName],
                            [['icons'], Array.map(plugin, Struct.get('row'))],
                            ...Array.map(panels, ({ index, icons }): readonly [JSONPath, unknown] => [['entrypoints', index, 'icons'], Array.map(icons, Struct.get('row'))]),
                            [['host', 'data', 'loadEvent'], 'startup'],
                            [['featureFlags', 'unhandledRejection'], true],
                            [['requiredPermissions', 'network', 'domains'], [yield* domain]],
                        ];
                        const icons = yield* Effect.forEach([...plugin, ...Array.flatMap(panels, Struct.get('icons'))], Struct.get('files'), { concurrency: 'unbounded' });
                        this.emitFile({ type: 'asset', fileName: UxpManifest.file, source: edits.reduce((text, [path, value]) => applyEdits(text, modify(text, path, value, { formattingOptions: { insertSpaces: true, tabSize: 4 } })), source) });
                        Array.flatten(icons).forEach((file) => {
                            this.emitFile({ type: 'asset', ...file });
                        });
                    }).pipe(Effect.provide(layer), Effect.runPromise);
                },
            } satisfies Plugin,
        ],
    };
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { pluginConfig };

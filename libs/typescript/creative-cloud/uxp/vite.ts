import { isBuiltin } from 'node:module';
import { join } from 'node:path';
import { fileURLToPath } from 'node:url';
import { layer } from '@effect/platform-node/NodeServices';
import { paint } from '@rasm/mark';
import { HostError } from '@rasm/object-model';
import { checks } from '@rasm/vite';
import { formatHex } from 'culori/fn';
import { Array, Effect, FileSystem, Record, Schema, SemVer, String, UndefinedOr } from 'effect';
import { applyEdits, modify } from 'jsonc-parser';
import { defaultClientConditions, defaultServerConditions, mergeConfig, type Rolldown, type UserConfig, type UserConfigFnPromise } from 'vite';
import { HOSTS, THEMES, UxpManifest } from '#/manifest.ts';

// --- [TYPES] ---------------------------------------------------------------------------

type Icon = NonNullable<UxpManifest['icons']>[number];

// --- [CONSTANTS] -----------------------------------------------------------------------

const HOSTED = /^adobe:/u;
const RASTER = /\.png$/u;
const GLYPH = 16;

// --- [MODELS] --------------------------------------------------------------------------

const Painted = Schema.NonEmptyArray(UxpManifest.Theme)
    .check(Schema.makeFilter((themes) => Array.dedupe(Array.map(themes, (theme) => THEMES[theme])).length === 1 || `themes of one tone, received ${themes.join(', ')}`))
    .annotate({ description: 'Themes of one tone, the fill a painted icon takes' });

// --- [OPERATIONS] ----------------------------------------------------------------------

const emitted = Effect.fnUntraced(function* (app: UxpManifest['host']['app'], drawing: string, { width, path, scale = [1], theme = [] }: Icon, padding?: number) {
    const [first] = yield* Effect.mapError(Schema.decodeUnknownEffect(Painted)(theme), HostError.refused(`Icon ${path}`));
    const fill = UxpManifest.fills[app][first];
    const style = `svg { color: ${formatHex(fill)}; opacity: ${fill.alpha ?? 1}; }`;
    const files = path.endsWith('.png') ? Array.map(scale, (factor) => ({ fileName: path.replace(RASTER, `@${factor}x.png`), frame: { width: width * factor, padding: UndefinedOr.map(padding, (margin) => margin * factor) } })) : [{ fileName: path, frame: undefined }];
    return yield* Effect.forEach(files, ({ fileName, frame }) => Effect.map(paint([drawing], style, frame), (source) => ({ fileName, source })), { concurrency: 'unbounded' });
});
const pluginBundle = (directory: string, exported: readonly string[], config: UserConfig): UserConfig => {
    const encoding = fileURLToPath(import.meta.resolve('@exodus/bytes/encoding-lite.js'));
    return mergeConfig(
        {
            build: {
                ssr: true,
                target: UxpManifest.target,
                rolldownOptions: {
                    input: join(directory, 'main.ts'),
                    platform: 'neutral',
                    transform: {
                        inject: Record.fromIterableWith(
                            exported.filter((name) => name in globalThis),
                            (name) => [name, [encoding, name] satisfies [string, string]],
                        ),
                    },
                    output: { format: 'cjs', entryFileNames: '[name].js', paths: String.replace(HOSTED, ''), codeSplitting: false },
                },
            },
            environments: { ssr: { resolve: { noExternal: true, builtins: [HOSTED], conditions: Array.intersection(defaultServerConditions, defaultClientConditions) } } },
            plugins: [
                checks(),
                {
                    name: 'hosted',
                    enforce: 'pre',
                    resolveId(this: Rolldown.PluginContext, id: string, importer: string | undefined): undefined {
                        return isBuiltin(id) ? this.error(`${importer ?? id} imports ${id}, a Node builtin the UXP host lacks`) : undefined;
                    },
                },
            ],
        },
        config,
    );
};

// --- [COMPOSITION] ---------------------------------------------------------------------

const uxp = (directory: string, mark: string, config: UserConfig = {}): UserConfigFnPromise =>
    Effect.fnUntraced(
        function* () {
            const fs = yield* FileSystem.FileSystem;
            const source = yield* fs.readFileString(join(directory, UxpManifest.file));
            const manifest = yield* Schema.decodeEffect(UxpManifest.json)(source);
            const [context, lite] = yield* Effect.all([Effect.context<Effect.Services<ReturnType<typeof emitted>>>(), Effect.promise(() => import('@exodus/bytes/encoding-lite.js'))]);
            const domains = manifest.requiredPermissions.network?.domains ?? [];
            const formatting = { formattingOptions: { insertSpaces: true, tabSize: 4 } };
            const floored = applyEdits(source, modify(source, ['host', 'minVersion'], SemVer.format(HOSTS[manifest.host.app].since), formatting));
            const permitted = domains === 'all' ? floored : applyEdits(floored, modify(floored, ['requiredPermissions', 'network', 'domains'], Array.dedupe([...domains, UxpManifest.link.origin]), formatting));
            const icons = [
                ...Array.map(manifest.icons ?? [], (icon) => emitted(manifest.host.app, mark, icon, (icon.width - GLYPH) / 2)),
                ...manifest.entrypoints.flatMap((entry) => (entry.type === 'panel' ? Array.map(entry.icons ?? [], (icon) => emitted(manifest.host.app, join(directory, 'icons', `${entry.id}.svg`), icon)) : [])),
            ];
            return pluginBundle(
                directory,
                Object.keys(lite),
                mergeConfig(
                    {
                        plugins: [
                            {
                                name: 'manifest',
                                generateBundle(this: Rolldown.PluginContext): Promise<void> {
                                    return Effect.all(icons, { concurrency: 'unbounded' }).pipe(
                                        Effect.map((files) => [{ fileName: UxpManifest.file, source: permitted }, ...files.flat()]),
                                        Effect.flatMap(Effect.forEach((file) => Effect.sync(() => this.emitFile({ type: 'asset', ...file })))),
                                        Effect.asVoid,
                                        Effect.runPromiseWith(context),
                                    );
                                },
                            },
                        ],
                    },
                    config,
                ),
            );
        },
        Effect.provide(layer),
        Effect.runPromise,
    );

// --- [EXPORTS] -------------------------------------------------------------------------

export { uxp };

import { isBuiltin } from 'node:module';
import { join, resolve } from 'node:path';
import { fileURLToPath } from 'node:url';
import { layer } from '@effect/platform-node/NodeServices';
import { paint } from '@rasm/mark';
import { checks } from '@rasm/vite';
import { Array, Effect, FileSystem, Record, Result, Schema, SemVer, String, Struct } from 'effect';
import { applyEdits, modify } from 'jsonc-parser';
import { defaultClientConditions, defaultServerConditions, type Plugin, type UserConfig } from 'vite';
import { HOSTS, ICON_SIZES, THEMES, UxpManifest } from '#/manifest';

// --- [CONSTANTS] -----------------------------------------------------------------------

const HOST_MODULE = /^adobe:/u;
const ENTRY = 'main.ts';
const SCALES = [1, 2];
const TONES = Array.groupBy(Struct.keys(THEMES), (theme): string => THEMES[theme]);

// --- [OPERATIONS] ----------------------------------------------------------------------

const drawIcons = (app: UxpManifest['host']['app'], species: keyof typeof ICON_SIZES, stem: string, drawing: string) =>
    Record.collect(TONES, (tone, theme) => ({
        row: { width: ICON_SIZES[species], height: ICON_SIZES[species], path: `icons/${stem}-${tone}.png`, scale: SCALES, theme, species: [species] },
        files: Effect.forEach(SCALES, (factor) => Effect.map(paint([drawing], { color: UxpManifest.fills[app][Array.headNonEmpty(theme)] }, { width: ICON_SIZES[species] * factor }), (source) => ({ fileName: `icons/${stem}-${tone}@${factor}x.png`, source })), { concurrency: 'unbounded' }),
    }));
const writeAssets = Effect.fnUntraced(function* (root: string, mark: string, main: string) {
    const fs = yield* FileSystem.FileSystem;
    const source = yield* fs.readFileString(join(root, UxpManifest.file));
    const { host, entrypoints, requiredPermissions } = yield* Schema.decodeEffect(UxpManifest.json)(source);
    const plugin = drawIcons(host.app, 'pluginList', 'plugin', mark);
    const panels = Array.filterMap(entrypoints, ({ type, id }, index) => (type === 'panel' ? Result.succeed({ index, icons: drawIcons(host.app, 'toolbar', id, join(root, 'icons', `${id}.svg`)) }) : Result.failVoid));
    const domains = requiredPermissions.network?.domains ?? [];
    const edits: readonly (readonly [readonly (string | number)[], unknown])[] = [
        [['main'], main],
        [['host', 'minVersion'], SemVer.format(HOSTS[host.app].since)],
        [['icons'], Array.map(plugin, Struct.get('row'))],
        ...Array.map(panels, ({ index, icons }) => [['entrypoints', index, 'icons'], Array.map(icons, Struct.get('row'))] as const),
        ...(domains === 'all' ? [] : [[['requiredPermissions', 'network', 'domains'], Array.dedupe([...domains, new URL(UxpManifest.link).origin])] as const]),
        [['featureFlags', 'unhandledRejection'], true],
    ];
    const manifest = edits.reduce((text, [path, value]) => applyEdits(text, modify(text, [...path], value, { formattingOptions: { insertSpaces: true, tabSize: 4 } })), source);
    const files = yield* Effect.forEach([...plugin, ...Array.flatMap(panels, Struct.get('icons'))], Struct.get('files'), { concurrency: 'unbounded' });
    return [{ fileName: UxpManifest.file, source: manifest }, ...Array.flatten(files)];
});

// --- [COMPOSITION] ---------------------------------------------------------------------

const pluginConfig = ({ mark = fileURLToPath(import.meta.resolve('#/icons/mark.svg')) }: { readonly mark?: string } = {}): UserConfig => {
    const encoding = fileURLToPath(import.meta.resolve('@exodus/bytes/encoding-lite.js'));
    return {
        build: {
            ssr: true,
            sourcemap: true,
            target: Record.collect(UxpManifest.engine, (browser, version) => `${browser}${version}`),
            rolldownOptions: { platform: 'neutral', output: { format: 'cjs', entryFileNames: '[name].js', paths: String.replace(HOST_MODULE, ''), codeSplitting: false, sourcemapExcludeSources: true } },
        },
        environments: { ssr: { resolve: { noExternal: true, builtins: [HOST_MODULE], conditions: Array.intersection(defaultServerConditions, defaultClientConditions) } } },
        plugins: [
            checks(),
            {
                name: 'uxp',
                config: ({ root = '' }) =>
                    import('@exodus/bytes/encoding-lite.js').then((lite) => ({ build: { rolldownOptions: { input: resolve(root, ENTRY), transform: { inject: Record.filterMap(lite, (_, name) => (name in globalThis ? Result.succeed([encoding, name] satisfies [string, string]) : Result.failVoid)) } } } })),
                resolveId: {
                    order: 'pre',
                    handler(id, importer) {
                        return isBuiltin(id) ? this.error(`${importer ?? id} imports ${id}, a Node builtin the UXP host lacks`) : undefined;
                    },
                },
                generateBundle(_, bundle) {
                    return Effect.fromNullishOr(Object.values(bundle).find((chunk) => chunk.type === 'chunk' && chunk.isEntry)).pipe(
                        Effect.flatMap((entry) => writeAssets(this.environment.config.root, mark, entry.fileName)),
                        Effect.flatMap(Effect.forEach((file) => Effect.sync(() => this.emitFile({ type: 'asset', ...file })), { discard: true })),
                        Effect.provide(layer),
                        Effect.runPromise,
                    );
                },
            } satisfies Plugin,
        ],
    };
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { pluginConfig };

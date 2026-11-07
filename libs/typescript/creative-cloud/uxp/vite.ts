import { createRequire, isBuiltin } from 'node:module';
import { join } from 'node:path';
import { fileURLToPath } from 'node:url';
import { layer } from '@effect/platform-node/NodeServices';
import { bundle, dialed, type Link, release } from '@rasm/automation/vite';
import { paint } from '@rasm/mark';
import { Array, Effect, FileSystem, Record, Schema, String, Struct } from 'effect';
import { defaultClientConditions, defaultServerConditions, mergeConfig, type Rolldown, type UserConfig, type UserConfigFnPromise } from 'vite';
import { UxpManifest } from '#/manifest.ts';

// --- [TYPES] ---------------------------------------------------------------------------

type Fields = Parameters<typeof UxpManifest.make>[0];
type Entry = UxpManifest['entrypoints'][number];
type Entries<K extends Entry['type'], O extends string = never> = Record.ReadonlyRecord<string, Omit<Extract<Entry, { readonly type: K }>, 'type' | 'id' | O>>;

interface Plugin extends Omit<Fields, keyof Effect.Success<ReturnType<typeof release>> | 'manifestVersion' | 'main' | 'icons' | 'entrypoints' | 'requiredPermissions'> {
    readonly requiredPermissions?: Omit<UxpManifest['requiredPermissions'], 'enableAddon'>;
    readonly panels?: Entries<'panel', 'icons'>;
    readonly commands?: Entries<'command'>;
    readonly link?: Link;
    readonly mark?: string;
    readonly config?: UserConfig;
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const HOSTED = /^adobe:/u;
const ENTRY = 'main';
const SPECIES = { toolbar: { size: 23, scale: [1, 2], raster: true }, pluginList: { size: 24, scale: [1], raster: false } } as const;

// --- [OPERATIONS] ----------------------------------------------------------------------

const icons = Effect.fnUntraced(function* (master: string, species: keyof typeof SPECIES, stem: string) {
    const { size, scale, raster } = SPECIES[species];
    const folder = 'icons';
    const fills = Record.toEntries(UxpManifest.fills);
    const files = yield* Effect.forEach(
        Array.cartesian(fills, scale),
        Effect.fnUntraced(function* ([[name, fill], factor]) {
            return { fileName: `${folder}/${stem}-${name}${raster ? `@${factor}x.png` : '.svg'}`, source: yield* paint(master, fill, raster ? size * factor : undefined) };
        }),
        { concurrency: 'unbounded' },
    );
    return { icons: Array.map(fills, ([name, { theme }]) => ({ width: size, height: size, path: `${folder}/${stem}-${name}.${raster ? 'png' : 'svg'}`, scale, theme, species: [species] })), files };
});
const panelEntry = (directory: string) =>
    Effect.fnUntraced(function* ([id, row]: readonly [string, Entries<'panel', 'icons'>[string]]) {
        const [fs, glyph] = [yield* FileSystem.FileSystem, join(directory, `${id}.svg`)];
        const rendered = (yield* fs.exists(glyph)) ? yield* icons(glyph, 'toolbar', id) : { icons: [], files: [] };
        return { entry: { ...row, type: 'panel' as const, id, ...(rendered.icons.length === 0 ? {} : { icons: rendered.icons }) }, files: rendered.files };
    });
const uxpScript = (directory: string, config: UserConfig): UserConfig => {
    const encoding = fileURLToPath(import.meta.resolve('@exodus/bytes/encoding-lite.js'));
    return mergeConfig(
        bundle(directory, {
            build: {
                ssr: join(directory, `${ENTRY}.ts`),
                emptyOutDir: true,
                target: UxpManifest.target,
                rolldownOptions: {
                    platform: 'neutral',
                    transform: {
                        inject: Record.fromIterableWith(
                            Object.keys(createRequire(import.meta.url)(encoding)).filter((name) => name in globalThis),
                            (name) => [name, [encoding, name] satisfies [string, string]],
                        ),
                    },
                    output: { format: 'cjs', entryFileNames: '[name].js', paths: String.replace(HOSTED, '') },
                },
            },
            environments: { ssr: { resolve: { noExternal: true, builtins: [HOSTED], conditions: Array.intersection(defaultServerConditions, defaultClientConditions) } } },
            plugins: [
                {
                    name: 'hosted',
                    enforce: 'pre',
                    resolveId(this: Rolldown.PluginContext, id: string, importer: string | undefined): undefined {
                        return isBuiltin(id) ? this.error(`${importer ?? id} imports ${id}, a Node builtin the UXP host lacks`) : undefined;
                    },
                },
            ],
        }),
        config,
    );
};

// --- [COMPOSITION] ---------------------------------------------------------------------

const uxp = (directory: string, { link, mark, panels = {}, commands = {}, config = {}, host, requiredPermissions = {}, ...manifest }: Plugin): UserConfigFnPromise =>
    Effect.fnUntraced(
        function* () {
            const dial = link === undefined ? undefined : yield* dialed(link);
            const [identity, plugin, panelled] = yield* Effect.all([release(directory), mark === undefined ? Effect.succeed({ icons: [], files: [] }) : icons(mark, 'pluginList', 'plugin'), Effect.forEach(Record.toEntries(panels), panelEntry(directory), { concurrency: 'unbounded' })], { concurrency: 'unbounded' });
            const stated = requiredPermissions.network?.domains ?? [];
            const source = yield* Effect.orDie(
                Schema.encodeEffect(UxpManifest.json)(
                    UxpManifest.make({
                        ...manifest,
                        ...identity,
                        manifestVersion: manifest.addon === undefined ? UxpManifest.version.current : UxpManifest.version.addon,
                        main: `${ENTRY}.js`,
                        host: dial === undefined ? host : { ...host, data: { ...host.data, loadEvent: 'startup' } },
                        ...(plugin.icons.length === 0 ? {} : { icons: plugin.icons }),
                        entrypoints: [...Array.map(panelled, Struct.get('entry')), ...Record.collect(commands, (id, row) => ({ ...row, type: 'command' as const, id }))],
                        requiredPermissions: { ...requiredPermissions, ...(manifest.addon === undefined ? {} : { enableAddon: true }), ...(dial === undefined ? {} : { network: { domains: stated === 'all' ? stated : ([dial.origin, ...stated] as const) } }) },
                    }),
                ),
            );
            const emitted = [...plugin.files, ...Array.flatMap(panelled, Struct.get('files'))];
            return uxpScript(
                directory,
                mergeConfig(
                    {
                        ...(dial === undefined ? {} : { define: dial.define }),
                        plugins: [
                            {
                                name: 'manifest',
                                generateBundle(this: Rolldown.PluginContext): void {
                                    this.emitFile({ type: 'asset', fileName: UxpManifest.file, source });
                                    emitted.forEach((file) => this.emitFile({ type: 'asset', ...file }));
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

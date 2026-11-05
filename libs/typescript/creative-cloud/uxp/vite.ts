import { createRequire, isBuiltin } from 'node:module';
import { basename, join, relative } from 'node:path';
import { fileURLToPath } from 'node:url';
import { layer } from '@effect/platform-node/NodeServices';
import { Version } from '@rasm/automation';
import { Array, Config, Effect, FileSystem, flow, Record, Schema, String, Struct } from 'effect';
import type { HttpRouter } from 'effect/http';
import type { NetAddress } from 'effect/net';
import { ChildProcess, ChildProcessSpawner } from 'effect/process';
import Builder from 'fast-xml-builder';
import { XMLParser } from 'fast-xml-parser';
import { defaultClientConditions, defaultServerConditions, mergeConfig, type Rolldown, searchForWorkspaceRoot, type UserConfig, type UserConfigFnPromise } from 'vite';
import { Peer } from '#/peer';
import { UxpManifest } from '#/uxp-manifest';

// --- [TYPES] ---------------------------------------------------------------------------

type Fields = Parameters<typeof UxpManifest.make>[0];
type Entry = UxpManifest['entrypoints'][number];
type Declared = Exclude<Entry, { readonly type: 'panel' }> | Omit<Extract<Entry, { readonly type: 'panel' }>, 'icons'>;
type Link = Omit<ImportMetaEnv['LINK'], 'endpoint'> & { readonly path: Exclude<HttpRouter.PathInput, '*'>; readonly listen: NetAddress.InetAddress };

// --- [CONSTANTS] -----------------------------------------------------------------------

const HOSTED = /^adobe:/u;
const PRERELEASE = /[-+]/u;
const ENTRY = 'main';
const ICONS = 'icons';
const SVG = { ignoreAttributes: false, preserveOrder: true } as const;
const SPECIES = { toolbar: 23, pluginList: 24 } as const;
const SCALES = [1, 2] as const;
const THEMES = [
    { name: 'light', theme: ['lightest', 'light'], color: '#4B4B4B' },
    { name: 'dark', theme: ['dark', 'darkest'], color: '#E3E3E3' },
] as const;

// --- [MODELS] --------------------------------------------------------------------------

const Described = Schema.fromJsonString(Schema.Struct({ description: Schema.String }));

// --- [OPERATIONS] ----------------------------------------------------------------------

// --- [RELEASE]

const release = Effect.fnUntraced(function* (directory: string) {
    const [fs, spawner] = yield* Effect.all([FileSystem.FileSystem, ChildProcessSpawner.ChildProcessSpawner]);
    const [identifier, prefix, { description }] = yield* Effect.all([Config.String('PRODUCT_BUNDLE_IDENTIFIER'), Config.String('MinVerTagPrefix'), Effect.flatMap(fs.readFileString(join(directory, 'package.json')), Schema.decodeEffect(Described))], { concurrency: 'unbounded' });
    const version = yield* spawner.string(ChildProcess.make('minver', ['-t', prefix, '-v', 'e'], { stdin: 'ignore', stderr: 'inherit' })).pipe(Effect.map(flow(String.trim, String.split(PRERELEASE), Array.headNonEmpty)), Effect.flatMap(Schema.decodeEffect(Version)));
    return { id: `${identifier}.${basename(directory)}`, name: description, version };
});
const dialed = ({ path, redial, listen }: Link) => {
    const origin = Peer.origin(listen.port);
    return { origin, define: { 'import.meta.env.LINK': JSON.stringify({ endpoint: `${origin}${path}`, redial } satisfies ImportMetaEnv['LINK']) } };
};

// --- [ICONS]

const panelFile = Effect.fnUntraced(function* (glyph: string, stylesheet: string, name: string, scale: number) {
    const [fs, spawner] = yield* Effect.all([FileSystem.FileSystem, ChildProcessSpawner.ChildProcessSpawner]);
    const rendered = `${stylesheet}@${scale}x.png`;
    yield* spawner.exitCode(ChildProcess.make('resvg', ['--width', `${SPECIES.toolbar * scale}`, '--stylesheet', stylesheet, glyph, rendered], { stdin: 'ignore', stderr: 'inherit' }));
    return { fileName: `${ICONS}/${name}@${scale}x.png`, source: yield* fs.readFile(rendered) };
});
const panelIcon = Effect.fnUntraced(function* (glyph: string, stem: string, { name, theme, color }: (typeof THEMES)[number]) {
    const fs = yield* FileSystem.FileSystem;
    const stylesheet = join(yield* fs.makeTempDirectoryScoped({ prefix: `uxp-${stem}-` }), `${name}.css`);
    yield* fs.writeFileString(stylesheet, `svg { color: ${color}; fill: ${color}; }`);
    const files = yield* Effect.forEach(SCALES, (scale) => panelFile(glyph, stylesheet, `${stem}-${name}`, scale));
    return { icon: { width: SPECIES.toolbar, height: SPECIES.toolbar, path: `${ICONS}/${stem}-${name}.png`, scale: SCALES, theme, species: ['toolbar'] as const }, files };
});
const entryIcons = Effect.fnUntraced(function* (directory: string, entry: Declared) {
    if (entry.type !== 'panel') {
        return { entry, files: [] };
    }
    const rows = yield* Effect.forEach(THEMES, (theme) => panelIcon(join(directory, `${entry.id}.svg`), entry.id, theme));
    return { entry: { ...entry, icons: Array.map(rows, Struct.get('icon')) }, files: Array.flatMap(rows, Struct.get('files')) };
});
const painted = (master: string, color: string): string =>
    new Builder(SVG).build(
        new XMLParser({
            ...SVG,
            updateTag: (tag, _, attributes) => {
                Object.assign(attributes, { '@_color': color, '@_fill': color }, attributes);
                return tag;
            },
        }).parse(master),
    );
const pluginIcons = Effect.fnUntraced(function* (mark: string) {
    const master = yield* Effect.flatMap(FileSystem.FileSystem, (fs) => fs.readFileString(mark));
    return Array.map(THEMES, ({ name, theme, color }) => {
        const path = `${ICONS}/plugin-${name}.svg`;
        return { icon: { width: SPECIES.pluginList, height: SPECIES.pluginList, path, scale: [1], theme, species: ['pluginList'] as const }, files: [{ fileName: path, source: painted(master, color) }] };
    });
});

// --- [BUNDLE]

const bundle = (directory: string, config: UserConfig): UserConfig => {
    const root = searchForWorkspaceRoot(directory);
    return mergeConfig({ build: { outDir: join(root, '.artifacts', relative(root, directory)), emptyOutDir: false, rolldownOptions: { experimental: { lazyBarrel: true }, output: { codeSplitting: false } } } }, config);
};
const uxpScript = (directory: string, extension: string): UserConfig => {
    const encoding = fileURLToPath(import.meta.resolve('@exodus/bytes/encoding-lite.js'));
    return bundle(directory, {
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
                output: { format: 'cjs', entryFileNames: `[name].${extension}`, paths: String.replace(HOSTED, '') },
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
    });
};

// --- [COMPOSITION] ---------------------------------------------------------------------

const uxp = (
    directory: string,
    manifest: Omit<Fields, keyof Effect.Success<ReturnType<typeof release>> | 'manifestVersion' | 'main' | 'host' | 'icons' | 'entrypoints' | 'requiredPermissions'> & {
        readonly host: Omit<typeof UxpManifest.Host.Type, 'data'>;
        readonly entrypoints?: readonly Declared[];
        readonly requiredPermissions: Omit<UxpManifest['requiredPermissions'], 'network'>;
    },
    link: Link,
    mark: string,
): UserConfigFnPromise =>
    Effect.fnUntraced(
        function* () {
            const { origin, define } = dialed(link);
            const [identity, plugin, entries] = yield* Effect.all([release(directory), pluginIcons(join(searchForWorkspaceRoot(directory), mark)), Effect.forEach(manifest.entrypoints ?? [], (entry) => entryIcons(directory, entry), { concurrency: 'unbounded' })], { concurrency: 'unbounded' });
            const source = yield* Effect.orDie(
                Schema.encodeEffect(UxpManifest.json)(
                    UxpManifest.make({
                        ...manifest,
                        ...identity,
                        manifestVersion: manifest.addon === undefined ? UxpManifest.version.current : UxpManifest.version.addon,
                        main: `${ENTRY}.js`,
                        host: { ...manifest.host, data: { loadEvent: 'startup' } },
                        icons: Array.map(plugin, Struct.get('icon')),
                        entrypoints: Array.map(entries, Struct.get('entry')),
                        requiredPermissions: { ...manifest.requiredPermissions, network: { domains: [origin] } },
                    }),
                ),
            );
            const emitted = [...Array.flatMap(plugin, Struct.get('files')), ...Array.flatMap(entries, Struct.get('files'))];
            return mergeConfig(uxpScript(directory, 'js'), {
                define,
                plugins: [
                    {
                        name: 'manifest',
                        generateBundle(this: Rolldown.PluginContext): void {
                            this.emitFile({ type: 'asset', fileName: UxpManifest.file, source });
                            emitted.forEach((file) => this.emitFile({ type: 'asset', ...file }));
                        },
                    },
                ],
            });
        },
        Effect.scoped,
        Effect.provide(layer),
        Effect.runPromise,
    );

// --- [EXPORTS] -------------------------------------------------------------------------

export { bundle, dialed, type Link, release, uxp, uxpScript };

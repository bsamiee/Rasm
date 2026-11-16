import { isBuiltin } from 'node:module';
import { join } from 'node:path';
import { fileURLToPath } from 'node:url';
import { layer } from '@effect/platform-node/NodeServices';
import { paint } from '@rasm/mark';
import { HostError } from '@rasm/object-model';
import { formatHex } from 'culori/fn';
import { Array, Effect, FileSystem, Option, Record, Schema, String } from 'effect';
import { applyEdits, modify } from 'jsonc-parser';
import { defaultClientConditions, defaultServerConditions, mergeConfig, type Rolldown, type UserConfig, type UserConfigFnPromise } from 'vite';
import { UxpManifest } from '#/manifest';

// --- [TYPES] ---------------------------------------------------------------------------

type Icon = NonNullable<UxpManifest['icons']>[number];

interface Plugin {
    readonly masters?: { readonly plugin?: string; readonly panels?: Record.ReadonlyRecord<string, string> };
    readonly network?: readonly string[];
    readonly config?: UserConfig;
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const HOSTED = /^adobe:/u;
const RASTER = /\.png$/u;

// --- [OPERATIONS] ----------------------------------------------------------------------

const emitted = Effect.fnUntraced(function* (directory: string, { width, path, scale = [1], theme = [] }: Icon, master: Option.Option<string>) {
    const files = path.endsWith('.png') ? Array.map(scale, (factor) => ({ fileName: path.replace(RASTER, `@${factor}x.png`), width: Option.some(width * factor) })) : [{ fileName: path, width: Option.none<number>() }];
    const fs = yield* FileSystem.FileSystem;
    const painting = yield* Effect.transposeOption(
        Option.map(master, (painted) =>
            Array.head(theme).pipe(
                Effect.fromOption(() => HostError.refused(`Icon ${path}`)(`states no theme, painted fills exist for ${Record.keys(UxpManifest.fills).join(', ')}`)),
                Effect.filterOrFail(Schema.is(UxpManifest.Theme), () => HostError.refused(`Icon ${path}`)('states theme all, painted fills exist per theme')),
                Effect.map((tone) => [painted, { color: formatHex(UxpManifest.fills[tone]), opacity: UxpManifest.fills[tone].alpha }] as const),
            ),
        ),
    );
    return yield* Effect.forEach(files, ({ fileName, width: sized }) => Effect.map(Option.match(painting, { onNone: () => fs.readFile(join(directory, fileName)), onSome: ([painted, fill]) => paint(painted, fill, ...Option.toArray(sized)) }), (source) => ({ fileName, source })), { concurrency: 'unbounded' });
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

const uxp = (directory: string, { masters = {}, network = [], config = {} }: Plugin = {}): UserConfigFnPromise =>
    Effect.fnUntraced(
        function* () {
            const source = yield* FileSystem.FileSystem.use((fs) => fs.readFileString(join(directory, UxpManifest.file)));
            const manifest = yield* Schema.decodeEffect(UxpManifest.json)(source);
            const [context, lite] = yield* Effect.all([Effect.context<Effect.Services<ReturnType<typeof emitted>>>(), Effect.promise(() => import('@exodus/bytes/encoding-lite.js'))]);
            const domains = manifest.requiredPermissions.network?.domains ?? [];
            const permitted = domains === 'all' || Array.isReadonlyArrayEmpty(network) ? source : applyEdits(source, modify(source, ['requiredPermissions', 'network', 'domains'], Array.dedupe([...domains, ...network]), { formattingOptions: { insertSpaces: true, tabSize: 4 } }));
            const icons = [...Array.map(manifest.icons ?? [], (icon) => [icon, Option.fromUndefinedOr(masters.plugin)] as const), ...manifest.entrypoints.flatMap((entry) => (entry.type === 'panel' ? Array.map(entry.icons ?? [], (icon) => [icon, Option.fromUndefinedOr(masters.panels?.[entry.id])] as const) : []))];
            return pluginBundle(
                directory,
                Object.keys(lite),
                mergeConfig(
                    {
                        plugins: [
                            {
                                name: 'manifest',
                                generateBundle(this: Rolldown.PluginContext): Promise<void> {
                                    return Effect.forEach(icons, ([icon, master]) => emitted(directory, icon, master), { concurrency: 'unbounded' }).pipe(
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

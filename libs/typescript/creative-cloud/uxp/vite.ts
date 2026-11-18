import { isBuiltin } from 'node:module';
import { join } from 'node:path';
import { fileURLToPath } from 'node:url';
import { layer } from '@effect/platform-node/NodeServices';
import { paint } from '@rasm/mark';
import { HostError } from '@rasm/object-model';
import { formatHex } from 'culori/fn';
import { Array, Effect, FileSystem, Option, Record, Schema, SemVer, String } from 'effect';
import { applyEdits, modify } from 'jsonc-parser';
import { defaultClientConditions, defaultServerConditions, mergeConfig, type Rolldown, type UserConfig, type UserConfigFnPromise } from 'vite';
import { HOSTS, UxpManifest } from '#/manifest';

// --- [TYPES] ---------------------------------------------------------------------------

type Icon = NonNullable<UxpManifest['icons']>[number];

// --- [CONSTANTS] -----------------------------------------------------------------------

const HOSTED = /^adobe:/u;
const RASTER = /\.png$/u;
const MARKS = fileURLToPath(new URL('../../../../products/apps/creative-cloud/multi-app/atelier/icons/', import.meta.url));

// --- [MODELS] --------------------------------------------------------------------------

const Composed = Schema.fromJsonString(Schema.Struct({ groups: Schema.NonEmptyArray(Schema.Struct({ layers: Schema.NonEmptyArray(Schema.Struct({ 'image-name': Schema.String })) })) })).annotate({ description: 'Icon Composer document by the image of its first layer' });

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

const uxp = (directory: string, config: UserConfig = {}): UserConfigFnPromise =>
    Effect.fnUntraced(
        function* () {
            const fs = yield* FileSystem.FileSystem;
            const [source, composed] = yield* Effect.all([fs.readFileString(join(directory, UxpManifest.file)), Effect.flatMap(fs.readFileString(join(MARKS, 'AppIcon.icon', 'icon.json')), Schema.decodeEffect(Composed))]);
            const manifest = yield* Schema.decodeEffect(UxpManifest.json)(source);
            const plugin = join(MARKS, 'AppIcon.icon', 'Assets', composed.groups[0].layers[0]['image-name']);
            const marks = new Set(yield* fs.readDirectory(MARKS));
            const marked = (panel: string) => Option.liftPredicate(join(MARKS, `${panel}.svg`), () => marks.has(`${panel}.svg`));
            const [context, lite] = yield* Effect.all([Effect.context<Effect.Services<ReturnType<typeof emitted>>>(), Effect.promise(() => import('@exodus/bytes/encoding-lite.js'))]);
            const domains = manifest.requiredPermissions.network?.domains ?? [];
            const formatting = { formattingOptions: { insertSpaces: true, tabSize: 4 } };
            const floored = applyEdits(source, modify(source, ['host', 'minVersion'], SemVer.format(HOSTS[manifest.host.app].since), formatting));
            const permitted = domains === 'all' ? floored : applyEdits(floored, modify(floored, ['requiredPermissions', 'network', 'domains'], Array.dedupe([...domains, UxpManifest.link.origin]), formatting));
            const icons = [...Array.map(manifest.icons ?? [], (icon) => [icon, Option.some(plugin)] as const), ...manifest.entrypoints.flatMap((entry) => (entry.type === 'panel' ? Array.map(entry.icons ?? [], (icon) => [icon, marked(entry.id)] as const) : []))];
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

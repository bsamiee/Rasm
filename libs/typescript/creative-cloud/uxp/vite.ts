import { createRequire, isBuiltin } from 'node:module';
import { join } from 'node:path';
import { fileURLToPath } from 'node:url';
import { layer } from '@effect/platform-node/NodeServices';
import { HostError } from '@rasm/automation';
import { bundle } from '@rasm/automation/vite';
import { paint } from '@rasm/mark/paint';
import { Array, Effect, FileSystem, Option, Record, Schema, String } from 'effect';
import { defaultClientConditions, defaultServerConditions, mergeConfig, type Rolldown, type UserConfig, type UserConfigFnPromise } from 'vite';
import { UxpManifest } from '#/index';

// --- [TYPES] ---------------------------------------------------------------------------

type Icon = NonNullable<UxpManifest['icons']>[number];

interface Plugin {
    readonly masters?: { readonly plugin?: string; readonly panels?: Record.ReadonlyRecord<string, string> };
    readonly config?: UserConfig;
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const HOSTED = /^adobe:/u;
const ENTRY = 'main';
const RASTER = /\.png$/u;

// --- [OPERATIONS] ----------------------------------------------------------------------

const emitted = Effect.fnUntraced(function* (directory: string, { width, path, scale = [1], theme = [] }: Icon, master: Option.Option<string>) {
    const files = RASTER.test(path) ? Array.map(scale, (factor) => ({ fileName: path.replace(RASTER, `@${factor}x.png`), width: Option.some(width * factor) })) : [{ fileName: path, width: Option.none<number>() }];
    const fs = yield* FileSystem.FileSystem;
    return yield* Option.match(master, {
        onNone: () => Effect.forEach(files, ({ fileName }) => Effect.map(fs.readFile(join(directory, fileName)), (source) => ({ fileName, source }))),
        onSome: (painted) =>
            Option.match(Array.head(theme), {
                onNone: () => Effect.fail(HostError.refused(`Icon ${path}`)(`states no theme, painted fills exist for ${Record.keys(UxpManifest.themes).join(', ')}`)),
                onSome: (first) =>
                    first === 'all'
                        ? Effect.fail(HostError.refused(`Icon ${path}`)('states theme all, painted fills exist per theme'))
                        : Effect.forEach(files, ({ fileName, width: sized }) => Effect.map(paint(painted, UxpManifest.fills[UxpManifest.themes[first]], ...Option.toArray(sized)), (source) => ({ fileName, source })), { concurrency: 'unbounded' }),
            }),
    });
});
const uxpScript = (directory: string, config: UserConfig): UserConfig => {
    const encoding = fileURLToPath(import.meta.resolve('@exodus/bytes/encoding-lite.js'));
    return mergeConfig(
        bundle(directory, {
            build: {
                ssr: true,
                target: UxpManifest.target,
                rolldownOptions: {
                    input: join(directory, `${ENTRY}.ts`),
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

const uxp = (directory: string, { masters = {}, config = {} }: Plugin = {}): UserConfigFnPromise =>
    Effect.fnUntraced(
        function* () {
            const source = yield* FileSystem.FileSystem.use((fs) => fs.readFileString(join(directory, UxpManifest.file)));
            const manifest = yield* Schema.decodeEffect(UxpManifest.json)(source);
            const context = yield* Effect.context<Effect.Services<ReturnType<typeof emitted>>>();
            const icons = [...Array.map(manifest.icons ?? [], (icon) => [icon, Option.fromUndefinedOr(masters.plugin)] as const), ...manifest.entrypoints.flatMap((entry) => (entry.type === 'panel' ? Array.map(entry.icons ?? [], (icon) => [icon, Option.fromUndefinedOr(masters.panels?.[entry.id])] as const) : []))];
            return uxpScript(
                directory,
                mergeConfig(
                    {
                        plugins: [
                            {
                                name: 'manifest',
                                generateBundle(this: Rolldown.PluginContext): Promise<void> {
                                    return Effect.forEach(icons, ([icon, master]) => emitted(directory, icon, master), { concurrency: 'unbounded' }).pipe(
                                        Effect.map((files) => [{ fileName: UxpManifest.file, source }, ...files.flat()]),
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

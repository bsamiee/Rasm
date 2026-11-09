import { createRequire, isBuiltin } from 'node:module';
import { join } from 'node:path';
import { fileURLToPath } from 'node:url';
import { layer } from '@effect/platform-node/NodeServices';
import { HostError } from '@rasm/automation';
import { bundle, dialed, type Link } from '@rasm/automation/vite';
import { paint } from '@rasm/mark';
import { Array, Effect, FileSystem, Option, Record, Schema, String } from 'effect';
import { defaultClientConditions, defaultServerConditions, mergeConfig, type Rolldown, type UserConfig, type UserConfigFnPromise } from 'vite';
import { UxpManifest } from '#/index';

// --- [TYPES] ---------------------------------------------------------------------------

type Icon = NonNullable<UxpManifest['icons']>[number];

interface Plugin {
    readonly link?: Link;
    readonly mark?: string;
    readonly config?: UserConfig;
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const HOSTED = /^adobe:/u;
const ENTRY = 'main';
const RASTER = /\.png$/u;

// --- [OPERATIONS] ----------------------------------------------------------------------

const painted = Effect.fnUntraced(function* (master: string, { width, path, scale = [1], theme = [] }: Icon) {
    const fill = yield* Option.match(Array.head(theme), {
        onNone: () => Effect.fail(HostError.refused(`Icon ${path}`)(`states no theme, painted fills exist for ${Record.keys(UxpManifest.themes).join(', ')}`)),
        onSome: (first) => (first === 'all' ? Effect.fail(HostError.refused(`Icon ${path}`)('states theme all, painted fills exist per theme')) : Effect.succeed(UxpManifest.fills[UxpManifest.themes[first]])),
    });
    return RASTER.test(path)
        ? yield* Effect.forEach(
              scale,
              Effect.fnUntraced(function* (factor) {
                  return { fileName: path.replace(RASTER, `@${factor}x.png`), source: yield* paint(master, fill, width * factor) };
              }),
              { concurrency: 'unbounded' },
          )
        : [{ fileName: path, source: yield* paint(master, fill) }];
});
const uxpScript = (directory: string, config: UserConfig): UserConfig => {
    const encoding = fileURLToPath(import.meta.resolve('@exodus/bytes/encoding-lite.js'));
    return mergeConfig(
        bundle(directory, {
            build: {
                ssr: true,
                emptyOutDir: true,
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

const uxp = (directory: string, { link, mark, config = {} }: Plugin): UserConfigFnPromise =>
    Effect.fnUntraced(
        function* () {
            const source = yield* FileSystem.FileSystem.use((fs) => fs.readFileString(join(directory, UxpManifest.file)));
            const manifest = yield* Schema.decodeEffect(UxpManifest.json)(source);
            const context = yield* Effect.context<Effect.Services<ReturnType<typeof painted>>>();
            const masters = [
                ...(manifest.icons ?? []).flatMap((icon) => Option.toArray(Option.map(Option.fromUndefinedOr(mark), (master) => [master, icon] as const))),
                ...manifest.entrypoints.flatMap((entry) => (entry.type === 'panel' ? (entry.icons ?? []).map((icon) => [join(directory, `${entry.id}.svg`), icon] as const) : [])),
            ];
            return uxpScript(
                directory,
                mergeConfig(
                    {
                        ...(link === undefined ? {} : { define: yield* dialed(link) }),
                        plugins: [
                            {
                                name: 'manifest',
                                generateBundle(this: Rolldown.PluginContext): Promise<void> {
                                    return Effect.forEach(masters, ([master, icon]) => painted(master, icon), { concurrency: 'unbounded' }).pipe(
                                        Effect.map((files) => [{ type: 'asset' as const, fileName: UxpManifest.file, source }, ...files.flat().map((file) => ({ type: 'asset' as const, ...file }))]),
                                        Effect.flatMap(Effect.forEach((file: Rolldown.EmittedAsset) => Effect.sync(() => this.emitFile(file)))),
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

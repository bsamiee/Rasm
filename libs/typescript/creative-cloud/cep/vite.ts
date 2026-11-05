import { createRequire } from 'node:module';
import { join } from 'node:path';
import { fileURLToPath } from 'node:url';
import { layer } from '@effect/platform-node/NodeServices';
import { HostError } from '@rasm/automation';
import { bundle, dialed, type Link, release } from '@rasm/automation/vite';
import { paint } from '@rasm/mark';
import type { JscTarget } from '@swc/wasm';
import { load } from 'cheerio';
import { Array, Effect, FileSystem, flow, Record } from 'effect';
import { build, type ConfigEnv, mergeConfig, type Rolldown, type UserConfig, type UserConfigFnPromise } from 'vite';
import { ExtensionManifest } from '#/extension-manifest';
import { lower } from '#/lower';

// --- [TYPES] ---------------------------------------------------------------------------

type Face = Omit<NonNullable<ExtensionManifest['face']>, 'icons'>;

interface Extension {
    readonly host: ExtensionManifest['host'];
    readonly script?: string;
    readonly parameters?: ExtensionManifest['parameters'];
    readonly config?: UserConfig;
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const ENTRY = 'main';
const ICONS = 'icons';
const SIZE = 23;
const SCALES = [
    { factor: 1, suffix: '' },
    { factor: 2, suffix: '@2X' },
] as const;

// --- [OPERATIONS] ----------------------------------------------------------------------

const extendscript = (target: JscTarget) => ({ name: 'extendscript', renderChunk: (code) => Effect.runPromise(lower(code, target)) }) satisfies Rolldown.Plugin;
const script = Effect.fn('script')(function* (entry: string, { name, target }: { readonly name: string; readonly target: JscTarget }) {
    const refused = HostError.refused(`Build of ${entry}`);
    const built = yield* Effect.tryPromise({
        try: () =>
            build({
                configFile: false,
                logLevel: 'error',
                build: {
                    target: 'esnext',
                    minify: false,
                    write: false,
                    sourcemap: 'hidden',
                    rolldownOptions: { input: entry, platform: 'neutral', preserveEntrySignatures: 'exports-only', output: { format: 'iife', name, exports: 'named', esModule: false, strict: false, generatedCode: { symbols: false }, minify: false, codeSplitting: false, plugins: extendscript(target) } },
                },
            }),
        catch: refused,
    });
    return 'output' in built ? { code: built.output[0].code, map: built.output[0].map?.toString() } : yield* refused('build returned no single output');
});
const faceIcons = Effect.fnUntraced(function* (glyph: string) {
    const files = yield* Effect.forEach(
        Array.cartesian(Record.toEntries(ExtensionManifest.inks), SCALES),
        Effect.fnUntraced(function* ([[name, ink], { factor, suffix }]) {
            return { fileName: `${ICONS}/${name}${suffix}.png`, source: yield* paint(glyph, ink, SIZE * factor) };
        }),
        { concurrency: 'unbounded' },
    );
    return { icons: Record.map(ExtensionManifest.states, (name) => `${ICONS}/${name}.png`), files };
});

// --- [COMPOSITION] ---------------------------------------------------------------------

const cep = (directory: string, { host, script: hostScript, parameters, config = {}, link, face }: Extension & ({ readonly link: Link; readonly face?: Face } | { readonly link?: Link; readonly face: Face })): UserConfigFnPromise =>
    flow(
        Effect.fnUntraced(function* ({ mode }: ConfigEnv) {
            const fs = yield* FileSystem.FileSystem;
            const glyph = join(directory, `${ENTRY}.svg`);
            const rendered = face !== undefined && (yield* fs.exists(glyph)) ? yield* faceIcons(glyph) : undefined;
            const manifest = ExtensionManifest.make({
                host,
                ...(yield* release(directory)),
                ...(face === undefined ? {} : { face: { ...face, ...(rendered === undefined ? {} : { icons: rendered.icons }) } }),
                ...(parameters === undefined ? {} : { parameters }),
                ...(hostScript === undefined ? {} : { scripted: true }),
                ...(link === undefined ? {} : { atLaunch: true }),
            });
            const required = createRequire(join(directory, 'package.json'));
            const panel = {
                name: 'panel',
                generateBundle(this: Rolldown.PluginContext, _options, output): void {
                    const page = load('<!doctype html><meta charset="utf-8">');
                    page('body').append(
                        ...Object.values(output)
                            .filter((file) => file.type === 'chunk' && file.isEntry)
                            .map((file) => page('<script>').attr('src', file.fileName)),
                    );
                    this.emitFile({ type: 'asset', fileName: ExtensionManifest.panel, source: page.html() });
                    this.emitFile({ type: 'asset', fileName: ExtensionManifest.file, source: manifest.xml });
                    (rendered?.files ?? []).forEach((file) => this.emitFile({ type: 'asset', ...file }));
                    if (mode === 'development') {
                        this.emitFile({ type: 'asset', fileName: ExtensionManifest.debugFile, source: manifest.debug(link === undefined ? ExtensionManifest.debugPort : link.listen.port + 1) });
                    }
                },
            } satisfies Rolldown.Plugin;
            return mergeConfig(
                bundle({
                    builder: { sharedConfigBuild: true },
                    define: link === undefined ? {} : (yield* dialed(link)).define,
                    build: { rolldownOptions: { preserveEntrySignatures: 'exports-only', output: { format: 'iife' } } },
                    environments: {
                        client: { build: { target: ExtensionManifest.chromium, emptyOutDir: true, rolldownOptions: { input: face === undefined ? fileURLToPath(new URL('invisible.ts', import.meta.url)) : join(directory, `${ENTRY}.ts`), output: { entryFileNames: '[name].js', plugins: panel } } } },
                        ...(hostScript === undefined
                            ? {}
                            : {
                                  host: {
                                      consumer: 'client',
                                      build: {
                                          target: 'esnext',
                                          minify: false,
                                          rolldownOptions: { input: required.resolve(hostScript), platform: 'neutral', output: { name: ExtensionManifest.global, exports: 'default', entryFileNames: ExtensionManifest.script, minify: false, plugins: extendscript(ExtensionManifest.target) } },
                                      },
                                  },
                              }),
                    },
                }),
                config,
            );
        }, Effect.provide(layer)),
        Effect.runPromise,
    );

// --- [EXPORTS] -------------------------------------------------------------------------

export { cep, script };

import { createRequire } from 'node:module';
import { join } from 'node:path';
import { fileURLToPath } from 'node:url';
import { layer } from '@effect/platform-node/NodeServices';
import { HostError } from '@rasm/automation';
import { bundle, dialed, type Link } from '@rasm/automation/vite';
import type { JscTarget } from '@swc/wasm';
import { load } from 'cheerio';
import { Array, Effect, FileSystem, flow, Schema } from 'effect';
import { build, type ConfigEnv, mergeConfig, type Rolldown, type UserConfig, type UserConfigFnPromise } from 'vite';
import { ExtensionManifest } from '#/manifest';
import { lower } from '#/node';

// --- [TYPES] ---------------------------------------------------------------------------

interface Extension {
    readonly script?: string;
    readonly panel?: string;
    readonly config?: UserConfig;
    readonly link?: Link;
}

// --- [MODELS] --------------------------------------------------------------------------

const Built = Schema.Struct({ output: Schema.NonEmptyArray(Schema.Struct({ code: Schema.String })) }).annotate({ description: 'Single-environment Vite build output, entry chunk first' });

// --- [OPERATIONS] ----------------------------------------------------------------------

const extendscript = (target: JscTarget) => ({ name: 'extendscript', renderChunk: (code) => Effect.runPromise(lower(code, target)) }) satisfies Rolldown.Plugin;
const script = Effect.fn('script')(function* (entry: string, { name, target }: { readonly name: string; readonly target: JscTarget }) {
    const refused = HostError.refused(`Build of ${entry}`);
    const built = yield* Effect.tryPromise({
        try: () =>
            build({
                configFile: false,
                logLevel: 'warn',
                build: {
                    write: false,
                    rolldownOptions: {
                        input: entry,
                        platform: 'neutral',
                        preserveEntrySignatures: 'exports-only',
                        output: { format: 'iife', name, exports: 'named', esModule: false, strict: false, generatedCode: { symbols: false }, codeSplitting: false, minify: false, plugins: extendscript(target) },
                    },
                },
            }),
        catch: refused,
    }).pipe(Effect.flatMap(flow(Schema.decodeUnknownEffect(Built), Effect.mapError(refused))));
    return Array.headNonEmpty(built.output).code;
});

// --- [COMPOSITION] ---------------------------------------------------------------------

const cep = (directory: string, { script: hostScript, panel = fileURLToPath(new URL('invisible.ts', import.meta.url)), config = {}, link }: Extension): UserConfigFnPromise =>
    flow(
        Effect.fnUntraced(function* ({ mode }: ConfigEnv) {
            const source = yield* FileSystem.FileSystem.use((fs) => fs.readFileString(join(directory, ExtensionManifest.file)));
            const manifest = yield* Schema.decodeEffect(ExtensionManifest.fromXml)(source);
            const required = createRequire(join(directory, 'package.json'));
            const page = {
                name: 'panel',
                generateBundle(this: Rolldown.PluginContext, _options, output): void {
                    const document = load('<!doctype html><meta charset="utf-8">');
                    document('body').append(
                        ...Object.values(output)
                            .filter((file) => file.type === 'chunk' && file.isEntry)
                            .map((file) => document('<script>').attr('src', file.fileName)),
                    );
                    [{ fileName: ExtensionManifest.panel, source: document.html() }, { fileName: ExtensionManifest.file, source }, ...(mode === 'development' ? [{ fileName: ExtensionManifest.debugFile, source: manifest.debug(link === undefined ? ExtensionManifest.debugPort : link.listen.port + 1) }] : [])].forEach(
                        (file) => this.emitFile({ type: 'asset', ...file }),
                    );
                },
            } satisfies Rolldown.Plugin;
            return mergeConfig(
                bundle(directory, {
                    builder: { sharedConfigBuild: true },
                    define: link === undefined ? {} : yield* dialed(link),
                    build: { rolldownOptions: { preserveEntrySignatures: 'exports-only', output: { format: 'iife' } } },
                    environments: {
                        client: { build: { target: ExtensionManifest.chromium, emptyOutDir: true, rolldownOptions: { input: panel, output: { entryFileNames: '[name].js', plugins: page } } } },
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

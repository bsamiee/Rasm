import { createRequire } from 'node:module';
import { join } from 'node:path';
import { layer } from '@effect/platform-node/NodeServices';
import { HostError } from '@rasm/automation';
import { bundle } from '@rasm/automation/vite';
import type { JscTarget } from '@swc/wasm';
import { load } from 'cheerio';
import { Array, Effect, FileSystem, flow, Schema } from 'effect';
import { build, type ConfigEnv, mergeConfig, type Plugin, type Rolldown, type UserConfig, type UserConfigFnPromise } from 'vite';
import { ExtensionManifest } from '#/manifest';
import { lower } from '#/node';

// --- [TYPES] ---------------------------------------------------------------------------

interface Extension {
    readonly script?: string;
    readonly panel: string;
    readonly config?: UserConfig;
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const VIRTUAL = /^\0?virtual:extendscript:/u;

// --- [MODELS] --------------------------------------------------------------------------

const Built = Schema.Struct({ output: Schema.NonEmptyArray(Schema.Struct({ code: Schema.String })) }).annotate({ description: 'Single-environment Vite build output, entry chunk first' });

// --- [OPERATIONS] ----------------------------------------------------------------------

const extendscript = (target: JscTarget) => ({ name: 'extendscript', renderChunk: (code) => Effect.runPromise(lower(code, target)) }) satisfies Rolldown.Plugin;
const script = (entry: string, { name, global }: { readonly name: string; readonly global: string }) => {
    const id = `virtual:extendscript:${global}`;
    const resolved = `\0${id}`;
    const refused = HostError.refused(`Build of ${entry}`);
    const compiled = Effect.tryPromise({
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
                        output: { format: 'iife', name, exports: 'named', esModule: false, strict: false, generatedCode: { symbols: false }, codeSplitting: false, minify: false, plugins: extendscript(ExtensionManifest.target) },
                    },
                },
            }),
        catch: refused,
    }).pipe(
        Effect.flatMap(flow(Schema.decodeUnknownEffect(Built), Effect.mapError(refused))),
        Effect.flatMap(({ output }) => Effect.orDie(Schema.encodeEffect(Schema.fromJsonString(Schema.String))(Array.headNonEmpty(output).code))),
        Effect.map((code) => `export default ${code}`),
    );
    return {
        name: 'script',
        config: () => ({ build: { rolldownOptions: { transform: { inject: { [global]: [id, 'default'] } } } } }),
        resolveId: { filter: { id: VIRTUAL }, handler: (source: string) => (source === id ? resolved : undefined) },
        load: { filter: { id: VIRTUAL }, handler: (loaded: string) => (loaded === resolved ? Effect.runPromise(compiled) : undefined) },
    } satisfies Plugin;
};

// --- [COMPOSITION] ---------------------------------------------------------------------

const cep = (directory: string, { script: hostScript, panel, config = {} }: Extension): UserConfigFnPromise =>
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
                    [{ fileName: ExtensionManifest.panel, source: document.html() }, { fileName: ExtensionManifest.file, source }, ...(mode === 'development' ? [{ fileName: ExtensionManifest.debugFile, source: manifest.debug }] : [])].forEach((file) => this.emitFile({ type: 'asset', ...file }));
                },
            } satisfies Rolldown.Plugin;
            return mergeConfig(
                bundle(directory, {
                    builder: { sharedConfigBuild: true },
                    build: { rolldownOptions: { preserveEntrySignatures: 'exports-only', output: { format: 'iife' } } },
                    environments: {
                        client: { build: { target: ExtensionManifest.chromium, rolldownOptions: { input: required.resolve(panel), output: { entryFileNames: '[name].js', plugins: page } } } },
                        ...(hostScript === undefined
                            ? {}
                            : {
                                  host: {
                                      consumer: 'client',
                                      build: {
                                          target: 'esnext',
                                          emptyOutDir: false,
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

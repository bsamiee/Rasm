import { createRequire } from 'node:module';
import { join } from 'node:path';
import { layer } from '@effect/platform-node/NodeServices';
import { HostError } from '@rasm/automation';
import { bundle } from '@rasm/automation/vite';
import type { JscTarget } from '@swc/wasm';
import { load } from 'cheerio';
import { Array, Effect, FileSystem, flow, Option, Record, Schema } from 'effect';
import { build, type ConfigEnv, mergeConfig, type Plugin, type Rolldown, type UserConfig, type UserConfigFnPromise } from 'vite';
import { ExtensionManifest } from '#/index';
import { lower } from '#/node';

// --- [TYPES] ---------------------------------------------------------------------------

interface Extension {
    readonly script?: string;
    readonly panel: string;
    readonly config?: UserConfig;
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const INLINE = /\?inline$/u;

// --- [MODELS] --------------------------------------------------------------------------

const Built = Schema.Struct({ output: Schema.NonEmptyArray(Schema.Struct({ code: Schema.String })) }).annotate({ description: 'Single-environment Vite build output, entry chunk first' });
const Quoted = Schema.fromJsonString(Schema.String);

// --- [OPERATIONS] ----------------------------------------------------------------------

const extendscript = (target: JscTarget) => ({ name: 'extendscript', renderChunk: (code) => Effect.runPromise(lower(code, target)) }) satisfies Rolldown.Plugin;
const script = (entry: string, name: string) => {
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
        Effect.flatMap(({ output }) => Effect.orDie(Schema.encodeEffect(Quoted)(Array.headNonEmpty(output).code))),
        Effect.map((code) => `export default ${code}`),
    );
    return {
        name: 'script',
        load: { filter: { id: INLINE }, handler: (id: string) => (id === `${entry}?inline` ? Effect.runPromise(compiled) : undefined) },
    } satisfies Plugin;
};

// --- [COMPOSITION] ---------------------------------------------------------------------

const cep = (directory: string, { script: hostScript, panel, config = {} }: Extension): UserConfigFnPromise =>
    flow(
        Effect.fnUntraced(function* ({ mode }: ConfigEnv) {
            const source = yield* FileSystem.FileSystem.use((fs) => fs.readFileString(join(directory, ExtensionManifest.file)));
            const manifest = yield* Schema.decodeEffect(ExtensionManifest.fromXml)(source);
            const required = createRequire(join(directory, 'package.json'));
            const host = yield* Effect.transposeOption(
                Option.map(Option.fromUndefinedOr(hostScript), (entry) =>
                    Effect.map(
                        Effect.fromOption(manifest.script, () => HostError.refused(`${ExtensionManifest.file} of ${manifest.id}`)(`names no ScriptPath, host script ${entry} builds one`)),
                        (entryFileNames) => ({
                            consumer: 'client',
                            build: {
                                target: 'esnext',
                                emptyOutDir: false,
                                minify: false,
                                rolldownOptions: { input: required.resolve(entry), platform: 'neutral', output: { name: ExtensionManifest.global, exports: 'default', entryFileNames, minify: false, plugins: extendscript(ExtensionManifest.target) } },
                            },
                        }),
                    ),
                ),
            );
            const page = {
                name: 'panel',
                generateBundle(this: Rolldown.PluginContext, _options, output): void {
                    const document = load('<!doctype html><meta charset="utf-8">');
                    document('body').append(
                        ...Object.values(output)
                            .filter((file) => file.type === 'chunk' && file.isEntry)
                            .map((file) => document('<script>').attr('src', file.fileName)),
                    );
                    [{ fileName: manifest.panel, source: document.html() }, { fileName: ExtensionManifest.file, source }, ...(mode === 'development' ? [{ fileName: ExtensionManifest.debugFile, source: manifest.debug }] : [])].forEach((file) => this.emitFile({ type: 'asset', ...file }));
                },
            } satisfies Rolldown.Plugin;
            return mergeConfig(
                bundle({
                    builder: { sharedConfigBuild: true },
                    build: { rolldownOptions: { preserveEntrySignatures: 'exports-only', output: { format: 'iife' } } },
                    environments: {
                        client: { build: { target: ExtensionManifest.chromium, rolldownOptions: { input: required.resolve(panel), output: { entryFileNames: '[name].js', plugins: page } } } },
                        ...Record.getSomes({ host }),
                    },
                }),
                config,
            );
        }, Effect.provide(layer)),
        Effect.runPromise,
    );

// --- [EXPORTS] -------------------------------------------------------------------------

export { cep, script };

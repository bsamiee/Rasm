import { createRequire } from 'node:module';
import { join } from 'node:path';
import { layer } from '@effect/platform-node/NodeServices';
import { extendscript } from '@rasm/extendscript/vite';
import { HostError } from '@rasm/object-model';
import { load } from 'cheerio';
import { Effect, FileSystem, flow, Option, Record, Schema } from 'effect';
import { type ConfigEnv, mergeConfig, type Rolldown, type UserConfig, type UserConfigFnPromise } from 'vite';
import { ExtensionManifest } from '#/manifest';

// --- [TYPES] ---------------------------------------------------------------------------

interface Extension {
    readonly script?: string;
    readonly panel: string;
    readonly config?: UserConfig;
}

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
                                rolldownOptions: { input: required.resolve(entry), platform: 'neutral', output: { name: ExtensionManifest.global, exports: 'default', entryFileNames, minify: false, plugins: extendscript() } },
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
                    [{ fileName: manifest.panel, source: document.html() }, { fileName: ExtensionManifest.file, source }, ...(mode === 'development' ? [{ fileName: ExtensionManifest.debugFile, source: manifest.debug }] : [])].forEach((file) => {
                        this.emitFile({ type: 'asset', ...file });
                    });
                },
            } satisfies Rolldown.Plugin;
            return mergeConfig(
                {
                    builder: { sharedConfigBuild: true },
                    build: { rolldownOptions: { preserveEntrySignatures: 'exports-only', output: { format: 'iife', codeSplitting: false } } },
                    environments: {
                        client: { build: { target: ExtensionManifest.chromium, rolldownOptions: { input: required.resolve(panel), output: { entryFileNames: '[name].js', plugins: page } } } },
                        ...Record.getSomes({ host }),
                    },
                },
                config,
            );
        }, Effect.provide(layer)),
        Effect.runPromise,
    );

// --- [EXPORTS] -------------------------------------------------------------------------

export { cep };

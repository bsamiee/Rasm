import { createRequire } from 'node:module';
import { join } from 'node:path';
import { layer } from '@effect/platform-node/NodeServices';
import { bundle, dialed, type Link, release } from '@rasm/uxp/vite';
import { transformSync } from '@swc/core';
import { load } from 'cheerio';
import { Effect, flow } from 'effect';
import type { ConfigEnv, Rolldown, UserConfigFnPromise } from 'vite';
import { ExtensionManifest } from '#/extension-manifest';

// --- [TYPES] ---------------------------------------------------------------------------

type App = ExtensionManifest['host']['app'];

// --- [COMPOSITION] ---------------------------------------------------------------------

const cep = <A extends App>(directory: string, contract: { readonly host: ExtensionManifest['host'] & { readonly app: A }; readonly script: string }, link: Link & { readonly debug: { readonly [K in A]: number } }): UserConfigFnPromise =>
    flow(
        Effect.fnUntraced(function* ({ mode }: ConfigEnv) {
            const manifest = ExtensionManifest.make({ host: contract.host, ...(yield* release(directory)) });
            const required = createRequire(join(directory, 'package.json'));
            const extendscript = { name: 'extendscript', renderChunk: (code) => transformSync(code, { jsc: { target: ExtensionManifest.target, parser: { syntax: 'ecmascript' }, assumptions: { iterableIsArray: true } } }).code } satisfies Rolldown.Plugin;
            const panel = {
                name: 'panel',
                generateBundle(this: Rolldown.PluginContext, _options, output): void {
                    const page = load('<!doctype html><meta charset="utf-8">');
                    page('head').append(
                        ...Object.values(output)
                            .filter((file) => file.type === 'chunk' && file.isEntry)
                            .map((file) => page('<script>').attr('src', file.fileName)),
                    );
                    this.emitFile({ type: 'asset', fileName: ExtensionManifest.panel, source: page.html() });
                    this.emitFile({ type: 'asset', fileName: ExtensionManifest.file, source: manifest.xml });
                    if (mode === 'development') {
                        this.emitFile({ type: 'asset', fileName: ExtensionManifest.debugFile, source: manifest.debug(link.debug[contract.host.app]) });
                    }
                },
            } satisfies Rolldown.Plugin;
            return bundle(directory, {
                builder: { sharedConfigBuild: true },
                define: dialed(link).define,
                build: { rolldownOptions: { preserveEntrySignatures: 'exports-only', output: { format: 'iife' } } },
                environments: {
                    client: { build: { target: ExtensionManifest.chromium, emptyOutDir: true, rolldownOptions: { input: required.resolve('@rasm/cep/panel'), output: { entryFileNames: '[name].js', plugins: panel } } } },
                    host: {
                        consumer: 'client',
                        build: { target: 'esnext', minify: false, rolldownOptions: { input: required.resolve(contract.script), platform: 'neutral', output: { name: ExtensionManifest.global, exports: 'default', entryFileNames: ExtensionManifest.script, minify: false, plugins: extendscript } } },
                    },
                },
            });
        }, Effect.provide(layer)),
        Effect.runPromise,
    );

// --- [EXPORTS] -------------------------------------------------------------------------

export { cep };

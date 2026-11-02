import { execFileSync } from 'node:child_process';
import { readFileSync } from 'node:fs';
import { createRequire } from 'node:module';
import { basename, join, relative } from 'node:path';
import { cloudflare } from '@cloudflare/vite-plugin';
import { Manifest as Extension } from '@rasm/cep/manifest';
import { Manifest } from '@rasm/uxp/manifest';
import { transformSync } from '@swc/core';
import compiler from '@vitejs/plugin-react';
import { Array, Config, Effect, Option, Record, Schema, String } from 'effect';
import { parse } from 'smol-toml';
import { defaultClientConditions, defaultServerConditions, type Plugin, type PluginOption, type Rolldown, type UserConfig, type UserConfigFnObject } from 'vite';

// --- [CONSTANTS] -----------------------------------------------------------------------

const _ROOT = import.meta.dirname;
const _HOSTED = /^adobe:/u;
const _EXPERIMENTAL = { lazyBarrel: true };
const _ENTRY = 'main.ts';
const _CORE = /^\d+\.\d+\.\d+/mu;

// --- [OPERATIONS] ----------------------------------------------------------------------

const react = (): PluginOption => compiler({ compiler: { panicThreshold: 'all_errors' } });
const worker =
    (directory: string, plugins: PluginOption, hosting: PluginOption): UserConfigFnObject =>
    ({ mode }) => ({
        build: { target: 'esnext', rolldownOptions: { experimental: _EXPERIMENTAL } },
        plugins: [
            plugins,
            Effect.runSync(Effect.map(Effect.all([Config.withDefault(Config.Boolean('STORYBOOK'), false), Config.withDefault(Config.Boolean('VITEST'), false)]), Array.some(Boolean)))
                ? []
                : [hosting, cloudflare({ types: { generate: false }, ...(mode === 'test' ? { persistState: false, inspectorPort: false } : { persistState: { path: join(_ROOT, '.cache', 'cloudflare', relative(_ROOT, directory)) } }) })],
        ],
    });

const bundled = (directory: string, asset: { readonly fileName: string; readonly source: string }): UserConfig => {
    const load = createRequire(import.meta.url);
    const encoding = load.resolve('@exodus/bytes/encoding-lite.js');
    return {
        build: {
            ssr: true,
            outDir: join(_ROOT, '.artifacts', relative(_ROOT, directory)),
            emptyOutDir: true,
            rolldownOptions: {
                input: { main: join(directory, _ENTRY) },
                platform: 'neutral',
                experimental: _EXPERIMENTAL,
                transform: {
                    inject: Record.fromIterableWith(
                        Array.filter(Object.keys(load(encoding)), (name) => name in globalThis),
                        (name) => [name, [encoding, name] satisfies [string, string]],
                    ),
                },
                output: { format: 'cjs', entryFileNames: '[name].js', codeSplitting: false, paths: String.replace(_HOSTED, '') },
            },
        },
        environments: { ssr: { resolve: { noExternal: true, builtins: [_HOSTED], conditions: Array.intersection(defaultServerConditions, defaultClientConditions) } } },
        plugins: [
            {
                name: 'entry',
                generateBundle(this: Rolldown.PluginContext): void {
                    this.emitFile({ type: 'asset', ...asset });
                },
            },
        ],
    };
};

const origin = (daemon: string) =>
    Schema.decodeUnknownEffect(Schema.Struct({ daemons: Schema.Record(Schema.String, Schema.Struct({ port: Schema.optionalKey(Schema.Int) })) }))(parse(readFileSync(join(_ROOT, 'mise.toml'), 'utf8'))).pipe(
        Effect.flatMap(({ daemons }) => Effect.fromOption(Option.flatMap(Record.get(daemons, daemon), ({ port }) => Option.fromUndefinedOr(port)))),
        Effect.map(Manifest.loopback),
        Effect.runSync,
    );
const released = (directory: string, { description }: { readonly description: string }) =>
    Effect.all({
        id: Effect.map(Config.String('PRODUCT_BUNDLE_IDENTIFIER'), (bundle) => `${bundle}.${basename(directory)}`),
        name: Effect.succeed(description),
        version: Config.String('MinVerTagPrefix').pipe(
            Effect.flatMap((prefix) => Effect.try(() => execFileSync('dotnet', ['dnx', 'minver-cli', '--prerelease', '-y', '--', '-t', prefix, '-v', 'e'], { encoding: 'utf8' }))),
            Effect.flatMap((text) => Effect.fromOption(Option.map(String.match(_CORE)(text), ([core]) => core))),
        ),
    }).pipe(Effect.runSync);
const uxp = (directory: string, { npm, manifest, daemon }: { readonly npm: { readonly description: string }; readonly manifest: Omit<Parameters<typeof Manifest.make>[0], 'id' | 'name' | 'version'>; readonly daemon: string }): UserConfig =>
    bundled(directory, { fileName: 'manifest.json', source: Effect.runSync(Schema.encodeEffect(Manifest.json)(Manifest.make({ ...manifest, ...released(directory, npm), requiredPermissions: { ...manifest.requiredPermissions, network: Option.some({ domains: [origin(daemon)] }) } }))) });
const cep = (directory: string, { npm, host, daemon, manifest: fields }: { readonly npm: { readonly description: string }; readonly host: string; readonly daemon: string; readonly manifest: Omit<Parameters<typeof Extension.make>[0], 'id' | 'name' | 'version'> }): UserConfig => {
    const manifest = Extension.make({ ...fields, ...released(directory, npm) });
    const outDir = join(_ROOT, '.artifacts', relative(_ROOT, directory));
    const bundle = (input: Record<string, string>, platform: 'browser' | 'neutral', output: Rolldown.OutputOptions) =>
        ({ consumer: 'client', build: { outDir, emptyOutDir: false, minify: platform === 'browser', rolldownOptions: { input, platform, preserveEntrySignatures: 'exports-only', experimental: _EXPERIMENTAL, output: { format: 'iife', codeSplitting: false, ...output } } } }) as const;
    return {
        builder: {},
        define: { 'import.meta.env.LINK_ORIGIN': JSON.stringify(origin(daemon)) },
        environments: { client: bundle({ main: join(directory, _ENTRY) }, 'browser', { entryFileNames: '[name].js' }), host: bundle({ host }, 'neutral', { name: 'handle', exports: 'default', entryFileNames: '[name].jsx' }) },
        plugins: [
            {
                name: 'extendscript',
                applyToEnvironment: ({ name }) => name === 'host',
                renderChunk: (code) => transformSync(code, { jsc: { target: Extension.target, parser: { syntax: 'ecmascript' }, assumptions: { iterableIsArray: true } } }).code,
            } satisfies Plugin,
            {
                name: 'panel',
                applyToEnvironment: ({ name }) => name === 'client',
                generateBundle(this: Rolldown.PluginContext): void {
                    this.emitFile({ type: 'asset', fileName: 'index.html', source: '<!doctype html><meta charset="utf-8"><script src="main.js"></script>\n' });
                    this.emitFile({ type: 'asset', fileName: 'CSXS/manifest.xml', source: manifest.xml({ main: 'index.html', script: 'host.jsx' }) });
                },
            } satisfies Plugin,
        ],
    };
};
const script = (directory: string, { extension }: { readonly extension: string }): UserConfig => bundled(directory, { fileName: `main.${extension}`, source: "require('./main.js');\n" });

// --- [EXPORTS] -------------------------------------------------------------------------

export { cep, react, script, uxp, worker };

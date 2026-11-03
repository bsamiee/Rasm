import { execFileSync } from 'node:child_process';
import { readdirSync, readFileSync } from 'node:fs';
import { createRequire } from 'node:module';
import { basename, dirname, isAbsolute, join, relative } from 'node:path';
import process from 'node:process';
import type { Manifest as Extension } from '@rasm/cep/manifest';
import type { Manifest } from '@rasm/uxp/manifest';
import compiler from '@vitejs/plugin-react';
import { Array, Config, Effect, Option, Record, Schema, String } from 'effect';
import { parse } from 'smol-toml';
import { defaultClientConditions, defaultServerConditions, type Plugin, type PluginOption, type Rolldown, type UserConfig, type UserConfigFnPromise } from 'vite';

// --- [CONSTANTS] -----------------------------------------------------------------------

const _ROOT = import.meta.dirname;
const _HOSTED = /^adobe:/u;
const _EXPERIMENTAL = { lazyBarrel: true };
const _ENTRY = 'main.ts';
const _MAIN = 'main.js';
const _CORE = /^\d+\.\d+\.\d+/mu;

// --- [OPERATIONS] ----------------------------------------------------------------------

const react = (): PluginOption => compiler({ compiler: { panicThreshold: 'all_errors' } });
const worker =
    (directory: string, plugins: PluginOption, hosting: PluginOption): UserConfigFnPromise =>
    ({ mode }) =>
        Effect.gen(function* () {
            const tooling = Array.some(yield* Effect.all([Config.withDefault(Config.Boolean('STORYBOOK'), false), Config.withDefault(Config.Boolean('VITEST'), false)]), Boolean);
            const hosted = tooling
                ? []
                : [hosting, (yield* Effect.promise(() => import('@cloudflare/vite-plugin'))).cloudflare({ types: { generate: false }, ...(mode === 'test' ? { persistState: false, inspectorPort: false } : { persistState: { path: join(_ROOT, '.cache', 'cloudflare', relative(_ROOT, directory)) } }) })];
            return { build: { target: 'esnext', rolldownOptions: { experimental: _EXPERIMENTAL } }, plugins: [plugins, hosted] };
        }).pipe(Effect.runPromise);

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

const port = (daemon: string) =>
    Schema.decodeUnknownEffect(Schema.Struct({ daemons: Schema.Record(Schema.String, Schema.Struct({ port: Schema.optionalKey(Schema.Int) })) }))(parse(readFileSync(join(_ROOT, 'mise.toml'), 'utf8'))).pipe(
        Effect.flatMap(({ daemons }) => Effect.fromOption(Option.flatMap(Record.get(daemons, daemon), ({ port: held }) => Option.fromUndefinedOr(held)))),
        Effect.runSync,
    );
const released = (directory: string, { description }: { readonly description: string }) =>
    Effect.all({
        id: Effect.map(Config.String('PRODUCT_BUNDLE_IDENTIFIER'), (bundle) => `${bundle}.${basename(directory)}`),
        name: Effect.succeed(description),
        version: Config.String('MinVerTagPrefix').pipe(
            Effect.flatMap((prefix) => Effect.try(() => execFileSync('minver', ['-t', prefix, '-v', 'e'], { encoding: 'utf8' }))),
            Effect.flatMap((text) => Effect.fromOption(Option.map(String.match(_CORE)(text), ([core]) => core))),
        ),
    });
const uxp =
    (directory: string, { npm, manifest, daemon }: { readonly npm: { readonly description: string }; readonly manifest: Omit<Parameters<typeof Manifest.make>[0], 'id' | 'name' | 'version'>; readonly daemon: string }): UserConfigFnPromise =>
    () =>
        Effect.all([Effect.promise(() => import('@rasm/uxp/manifest')), released(directory, npm)], { concurrency: 'unbounded' }).pipe(
            Effect.flatMap(([{ Manifest: Uxp }, identity]) => Schema.encodeEffect(Uxp.json)(Uxp.make({ ...manifest, ...identity, main: Option.some(_MAIN), requiredPermissions: { ...manifest.requiredPermissions, network: Option.some({ domains: [Uxp.loopback(port(daemon))] }) } }))),
            Effect.map((source) => bundled(directory, { fileName: 'manifest.json', source })),
            Effect.runPromise,
        );
const cep =
    (directory: string, { npm, host, daemon, manifest: fields }: { readonly npm: { readonly description: string }; readonly host: string; readonly daemon: string; readonly manifest: Omit<Parameters<typeof Extension.make>[0], 'id' | 'name' | 'version'> }): UserConfigFnPromise =>
    () =>
        Effect.all([Effect.promise(() => import('@rasm/cep/manifest')), Effect.promise(() => import('@rasm/uxp/manifest')), Effect.promise(() => import('@swc/core')), released(directory, npm)], { concurrency: 'unbounded' }).pipe(
            Effect.map(([{ Manifest: Cep }, { Manifest: Uxp }, { transformSync }, identity]): UserConfig => {
                const manifest = Cep.make({ ...fields, ...identity });
                const outDir = join(_ROOT, '.artifacts', relative(_ROOT, directory));
                const bundle = (input: Record<string, string>, platform: 'browser' | 'neutral', output: Rolldown.OutputOptions) =>
                    ({ consumer: 'client', build: { outDir, emptyOutDir: false, minify: platform === 'browser', rolldownOptions: { input, platform, preserveEntrySignatures: 'exports-only', experimental: _EXPERIMENTAL, output: { format: 'iife', codeSplitting: false, ...output } } } }) as const;
                return {
                    builder: {},
                    define: { 'import.meta.env.LINK_ORIGIN': JSON.stringify(Uxp.loopback(port(daemon))) },
                    environments: { client: bundle({ main: join(directory, _ENTRY) }, 'browser', { entryFileNames: '[name].js' }), host: bundle({ host }, 'neutral', { name: 'handle', exports: 'default', entryFileNames: '[name].jsx' }) },
                    plugins: [
                        {
                            name: 'extendscript',
                            applyToEnvironment: ({ name }) => name === 'host',
                            renderChunk: (code) => transformSync(code, { jsc: { target: Cep.target, parser: { syntax: 'ecmascript' }, assumptions: { iterableIsArray: true } } }).code,
                        } satisfies Plugin,
                        {
                            name: 'panel',
                            applyToEnvironment: ({ name }) => name === 'client',
                            generateBundle(this: Rolldown.PluginContext): void {
                                this.emitFile({ type: 'asset', fileName: 'index.html', source: `<!doctype html><meta charset="utf-8"><script src="${_MAIN}"></script>\n` });
                                this.emitFile({ type: 'asset', fileName: 'CSXS/manifest.xml', source: manifest.xml({ main: 'index.html', script: 'host.jsx' }) });
                            },
                        } satisfies Plugin,
                    ],
                };
            }),
            Effect.runPromise,
        );
const script = (directory: string, { extension }: { readonly extension: string }): UserConfig => bundled(directory, { fileName: `main.${extension}`, source: `require('./${_MAIN}');\n` });
const sea = (directory: string, { main, daemon, host }: { readonly main: string; readonly daemon: string; readonly host: string }): UserConfig => {
    const outDir = join(_ROOT, '.artifacts', relative(_ROOT, directory));
    const load = createRequire(join(directory, 'package.json'));
    const addon = `@swc/core-${process.platform}-${process.arch}`;
    const binding = createRequire(load.resolve('@swc/core')).resolve(addon);
    const { bundle, executable } = Effect.all({ bundle: Config.String('PRODUCT_BUNDLE_IDENTIFIER'), executable: Config.String(host) }).pipe(Effect.runSync);
    const label = `${bundle}.server`;
    const environment = { MCP_LISTEN: `127.0.0.1:${port(daemon)}`, [host]: executable, SWC_BINARY_PATH: join(dirname(dirname(executable)), 'Frameworks', basename(binding)) };
    const plist = [
        '<?xml version="1.0" encoding="UTF-8"?>',
        '<!DOCTYPE plist PUBLIC "-//Apple//DTD PLIST 1.0//EN" "http://www.apple.com/DTDs/PropertyList-1.0.dtd">',
        '<plist version="1.0">',
        '<dict>',
        `    <key>Label</key><string>${label}</string>`,
        `    <key>BundleProgram</key><string>Contents/Helpers/${daemon}</string>`,
        `    <key>AssociatedBundleIdentifiers</key><array><string>${bundle}</string></array>`,
        '    <key>KeepAlive</key><true/>',
        `    <key>EnvironmentVariables</key><dict>${Record.collect(environment, (key, value) => `<key>${key}</key><string>${value}</string>`).join('')}</dict>`,
        '</dict>',
        '</plist>',
        '',
    ].join('\n');
    return {
        build: {
            ssr: true,
            outDir,
            emptyOutDir: false,
            rolldownOptions: { input: { [daemon]: join(directory, main) }, platform: 'node', external: (id) => id.startsWith('@swc/core-') && id !== addon, experimental: _EXPERIMENTAL, output: { format: 'esm', entryFileNames: '[name].mjs', codeSplitting: false } },
        },
        environments: { ssr: { resolve: { noExternal: true } } },
        plugins: [
            {
                name: 'sea',
                enforce: 'pre',
                resolveId: (id) => (id === addon ? `\0${addon}` : undefined),
                load: (id) => (id === `\0${addon}` ? 'module.exports = require(process.env.SWC_BINARY_PATH);' : undefined),
                generateBundle(this: Rolldown.PluginContext, _options, output): void {
                    const chunks = Object.values(output).filter((file) => file.type === 'chunk' && file.isEntry);
                    const assets = Record.fromIterableWith(
                        Array.flatMap(Array.dedupe(chunks.flatMap((chunk) => (chunk.type === 'chunk' ? chunk.moduleIds.filter((id) => isAbsolute(id)).map((id) => dirname(id)) : []))), (folder) =>
                            readdirSync(folder, { withFileTypes: true })
                                .filter((entry) => entry.isFile() && entry.name.endsWith('.wasm'))
                                .map((entry) => join(folder, entry.name)),
                        ),
                        (file) => [basename(file), file],
                    );
                    this.emitFile({ type: 'asset', fileName: 'sea.json', source: `${JSON.stringify({ main: join(outDir, `${daemon}.mjs`), mainFormat: 'module', output: join(outDir, daemon), useVfs: true, disableExperimentalSEAWarning: true, assets }, null, 4)}\n` });
                    this.emitFile({ type: 'asset', fileName: join('LaunchAgents', `${label}.plist`), source: plist });
                    this.emitFile({ type: 'asset', fileName: basename(binding), source: readFileSync(binding) });
                },
            } satisfies Plugin,
        ],
    };
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { cep, react, script, sea, uxp, worker };

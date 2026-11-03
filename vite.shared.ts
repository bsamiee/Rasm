import { execFileSync } from 'node:child_process';
import { readdirSync, readFileSync } from 'node:fs';
import { createRequire } from 'node:module';
import { basename, dirname, isAbsolute, join, relative } from 'node:path';
import process from 'node:process';
import { pathToFileURL } from 'node:url';
import type { Manifest } from '@rasm/uxp/manifest';
import compiler from '@vitejs/plugin-react';
import { Array, Config, Effect, Option, Record, Schema, String, Struct } from 'effect';
import { parse } from 'smol-toml';
import { defaultClientConditions, defaultServerConditions, type Plugin, type PluginOption, type Rolldown, type UserConfig, type UserConfigFnPromise } from 'vite';

// --- [TYPES] ---------------------------------------------------------------------------

type Released = keyof Effect.Success<ReturnType<typeof released>>;

// --- [CONSTANTS] -----------------------------------------------------------------------

const testMode = 'test';
const _ROOT = import.meta.dirname;
const _HOSTED = /^adobe:/u;
const _EXPERIMENTAL = { lazyBarrel: true };
const _ENTRY = 'main.ts';
const _MAIN = 'main.js';
const _CORE = /^\d+\.\d+\.\d+/mu;
const _BUNDLE = Config.String('PRODUCT_BUNDLE_IDENTIFIER');

// --- [OPERATIONS] ----------------------------------------------------------------------

const artifacts = (directory: string) => join(_ROOT, '.artifacts', relative(_ROOT, directory));
const react = (): PluginOption => compiler({ compiler: { panicThreshold: 'all_errors' } });
const worker =
    (directory: string, plugins: PluginOption, hosting: PluginOption): UserConfigFnPromise =>
    ({ mode }) =>
        Effect.gen(function* () {
            const tooling = Array.some(yield* Effect.all([Config.withDefault(Config.Boolean('STORYBOOK'), false), Config.withDefault(Config.Boolean('VITEST'), false)]), Boolean);
            const hosted = tooling
                ? []
                : [hosting, (yield* Effect.promise(() => import('@cloudflare/vite-plugin'))).cloudflare({ types: { generate: false }, ...(mode === testMode ? { persistState: false, inspectorPort: false } : { persistState: { path: join(_ROOT, '.cache', 'cloudflare', relative(_ROOT, directory)) } }) })];
            return { build: { target: 'esnext', rolldownOptions: { experimental: _EXPERIMENTAL } }, plugins: [plugins, hosted] };
        }).pipe(Effect.runPromise);

const bundled = (directory: string, asset: Omit<Rolldown.EmittedAsset, 'type'>): UserConfig => {
    const load = createRequire(import.meta.url);
    const encoding = load.resolve('@exodus/bytes/encoding-lite.js');
    return {
        build: {
            ssr: true,
            outDir: artifacts(directory),
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

const daemon = (directory: string) =>
    Schema.decodeUnknownEffect(Schema.Struct({ daemons: Schema.Record(Schema.String, Schema.Struct({ port: Schema.optionalKey(Schema.Int) })) }))(parse(readFileSync(join(_ROOT, 'mise.toml'), 'utf8'))).pipe(
        Effect.flatMap(({ daemons }) =>
            Effect.fromOption(
                Array.findFirst(
                    Array.unfold(directory, (folder) => (folder === dirname(folder) ? Option.none() : Option.some([folder, dirname(folder)] as const))),
                    (folder) => Option.flatMap(Record.get(daemons, basename(folder)), ({ port }) => Option.map(Option.fromUndefinedOr(port), (held) => ({ name: basename(folder), port: held }))),
                ),
            ),
        ),
        Effect.runSync,
    );
const released = (directory: string) =>
    Effect.all({
        id: Effect.map(_BUNDLE, (bundle) => `${bundle}.${basename(directory)}`),
        name: Effect.map(Schema.decodeUnknownEffect(Schema.fromJsonString(Schema.Struct({ description: Schema.String })))(readFileSync(join(directory, 'package.json'), 'utf8')), Struct.get('description')),
        version: Config.String('MinVerTagPrefix').pipe(
            Effect.flatMap((prefix) => Effect.try(() => execFileSync('minver', ['-t', prefix, '-v', 'e'], { encoding: 'utf8' }))),
            Effect.flatMap((text) => Effect.fromOption(Option.map(String.match(_CORE)(text), ([core]) => core))),
        ),
    });
const uxp =
    (directory: string, manifest: Omit<Parameters<typeof Manifest.make>[0], Released | 'main' | 'requiredPermissions'>): UserConfigFnPromise =>
    () =>
        Effect.all([Effect.promise(() => import('@rasm/uxp/manifest')), released(directory)], { concurrency: 'unbounded' }).pipe(
            Effect.flatMap(([{ Manifest: Uxp }, identity]) =>
                Schema.encodeEffect(Uxp.json)(
                    Uxp.make({
                        ...manifest,
                        ...identity,
                        main: Option.some(_MAIN),
                        host: { ...manifest.host, data: Option.some({ loadEvent: 'startup' }) },
                        requiredPermissions: { localFileSystem: Option.some('fullAccess'), network: Option.some({ domains: [Uxp.loopback(daemon(directory).port)] }), allowCodeGenerationFromStrings: Option.some(true) },
                    }),
                ),
            ),
            Effect.map((source) => bundled(directory, { fileName: Uxp.file, source })),
            Effect.runPromise,
        );
const cep =
    (directory: string, library: string): UserConfigFnPromise =>
    () =>
        Effect.gen(function* () {
            const [{ Manifest: Cep }, { Manifest: Uxp }, declared, { transformSync }, identity] = yield* Effect.all(
                [
                    Effect.promise(() => import('@rasm/cep/manifest')),
                    Effect.promise(() => import('@rasm/uxp/manifest')),
                    Effect.promise((): Promise<unknown> => import(pathToFileURL(createRequire(join(directory, 'package.json')).resolve(`${library}/manifest`)).href)),
                    Effect.promise(() => import('@swc/core')),
                    released(directory),
                ],
                { concurrency: 'unbounded' },
            );
            const { host } = yield* Schema.decodeUnknownEffect(Schema.Struct({ host: Cep.fields.host }))(declared);
            const manifest = Cep.make({ host, ...identity });
            const outDir = artifacts(directory);
            const files = { main: 'index.html', script: 'host.jsx' } as const;
            const panel = '\0panel';
            const bundle = (input: Record<string, string>, platform: 'browser' | 'neutral', output: Rolldown.OutputOptions) =>
                ({ consumer: 'client', build: { outDir, emptyOutDir: false, minify: platform === 'browser', rolldownOptions: { input, platform, preserveEntrySignatures: 'exports-only', experimental: _EXPERIMENTAL, output: { format: 'iife', codeSplitting: false, ...output } } } }) as const;
            return {
                builder: {},
                define: { 'import.meta.env.LINK_ORIGIN': JSON.stringify(Uxp.loopback(daemon(directory).port)) },
                environments: { client: bundle({ main: panel }, 'browser', { entryFileNames: '[name].js' }), host: bundle({ host: `${library}/host` }, 'neutral', { name: 'handle', exports: 'default', entryFileNames: files.script }) },
                plugins: [
                    {
                        name: 'extendscript',
                        applyToEnvironment: ({ name }) => name === 'host',
                        renderChunk: (code) => transformSync(code, { jsc: { target: Cep.target, parser: { syntax: 'ecmascript' }, assumptions: { iterableIsArray: true } } }).code,
                    } satisfies Plugin,
                    {
                        name: 'panel',
                        enforce: 'pre',
                        applyToEnvironment: ({ name }) => name === 'client',
                        resolveId: (id) => (id === panel ? id : undefined),
                        load: (id) => (id === panel ? "import { extension } from '@rasm/cep/entrypoints';\nextension();\n" : undefined),
                        generateBundle(this: Rolldown.PluginContext): void {
                            this.emitFile({ type: 'asset', fileName: files.main, source: `<!doctype html><meta charset="utf-8"><script src="${_MAIN}"></script>\n` });
                            this.emitFile({ type: 'asset', fileName: 'CSXS/manifest.xml', source: manifest.xml(files) });
                        },
                    } satisfies Plugin,
                ],
            } satisfies UserConfig;
        }).pipe(Effect.runPromise);
const script = (directory: string, { extension }: { readonly extension: string }): UserConfig => bundled(directory, { fileName: `main.${extension}`, source: `require('./${_MAIN}');\n` });
const sea = (directory: string, { host }: { readonly host: string }): UserConfig => {
    const { name, port } = daemon(directory);
    const outDir = artifacts(directory);
    const load = createRequire(join(directory, 'package.json'));
    const addon = `@swc/core-${process.platform}-${process.arch}`;
    const binding = createRequire(load.resolve('@swc/core')).resolve(addon);
    const { bundle, executable } = Effect.all({ bundle: _BUNDLE, executable: Config.String(host) }).pipe(Effect.runSync);
    const label = `${bundle}.server`;
    const environment = { MCP_LISTEN: `127.0.0.1:${port}`, [host]: executable, SWC_BINARY_PATH: join(dirname(dirname(executable)), 'Frameworks', basename(binding)) };
    const plist = [
        '<?xml version="1.0" encoding="UTF-8"?>',
        '<!DOCTYPE plist PUBLIC "-//Apple//DTD PLIST 1.0//EN" "http://www.apple.com/DTDs/PropertyList-1.0.dtd">',
        '<plist version="1.0">',
        '<dict>',
        `    <key>Label</key><string>${label}</string>`,
        `    <key>BundleProgram</key><string>Contents/Helpers/${name}</string>`,
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
            rolldownOptions: { input: { [name]: join(directory, _ENTRY) }, platform: 'node', external: (id) => id.startsWith('@swc/core-') && id !== addon, experimental: _EXPERIMENTAL, output: { format: 'esm', entryFileNames: '[name].mjs', codeSplitting: false } },
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
                    this.emitFile({ type: 'asset', fileName: 'sea.json', source: `${JSON.stringify({ main: join(outDir, `${name}.mjs`), mainFormat: 'module', output: join(outDir, name), useVfs: true, disableExperimentalSEAWarning: true, assets }, null, 4)}\n` });
                    this.emitFile({ type: 'asset', fileName: join('LaunchAgents', `${label}.plist`), source: plist });
                    this.emitFile({ type: 'asset', fileName: basename(binding), source: readFileSync(binding) });
                },
            } satisfies Plugin,
        ],
    };
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { cep, react, script, sea, testMode, uxp, worker };

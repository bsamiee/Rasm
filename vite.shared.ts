import { globSync, readFileSync } from 'node:fs';
import { createRequire } from 'node:module';
import { basename, dirname, isAbsolute, join, relative, sep } from 'node:path';
import process from 'node:process';
import { layer } from '@effect/platform-node/NodeServices';
import { ExtensionManifest } from '@rasm/cep';
import { UxpManifest } from '@rasm/uxp';
import viteReact from '@vitejs/plugin-react';
import { Array, Cause, Config, Effect, Function, flow, Option, Predicate, Record, Result, Schema, String } from 'effect';
import type { HttpRouter } from 'effect/http';
import { NetAddress } from 'effect/net';
import { build } from 'plist';
import { type ConfigEnv, defaultClientConditions, defaultServerConditions, mergeConfig, type Plugin, type PluginOption, type Rolldown, type UserConfig, type UserConfigFnPromise } from 'vite';
import { Mise, Package } from '#/eng/typescript/manifest';
import { runCommand } from '#/eng/typescript/run-command';

// --- [TYPES] ---------------------------------------------------------------------------

type Fields = Parameters<typeof UxpManifest.make>[0];
type Linking = Omit<ImportMetaEnv['LINK'], 'endpoint'> & { readonly path: Exclude<HttpRouter.PathInput, '*'> };

// --- [CONSTANTS] -----------------------------------------------------------------------

const testMode = 'test';
const ROOT = import.meta.dirname;
const HOSTED = /^adobe:/u;
const EXPERIMENTAL = { lazyBarrel: true };
const ENTRY = 'main';
const PRERELEASE = /[-+]/u;
const IDENTIFIER = Config.String('PRODUCT_BUNDLE_IDENTIFIER');

// --- [MODELS] --------------------------------------------------------------------------

const JsonText = Schema.fromJsonString(Schema.Unknown);

// --- [OPERATIONS] ----------------------------------------------------------------------

// --- [PROJECT]

const located = (folder: string, directory: string) => join(ROOT, folder, relative(ROOT, directory));
const daemon = Effect.fnUntraced(function* (directory: string) {
    const { daemons } = yield* Mise.read(ROOT);
    const ports = Record.filterMap(daemons, (row) => (Predicate.isString(row) ? Result.failVoid : Result.fromOption(row.port, Function.constVoid)));
    const found = Array.findLast(relative(ROOT, directory).split(sep), (name) => Option.all({ name: Option.some(name), port: Record.get(ports, name) }));
    return yield* Effect.fromOption(found, () => new Cause.NoSuchElementError(`No mise daemon with a port names a folder of ${directory}`));
});
const dialed = Effect.fnUntraced(function* (directory: string, { path, redial }: Linking) {
    const [{ port }, { Peer }] = yield* Effect.all([daemon(directory), Effect.promise(() => import('@rasm/uxp/peer'))], { concurrency: 'unbounded' });
    const origin = Peer.origin(port);
    const link = yield* Effect.orDie(Schema.encodeEffect(JsonText)({ endpoint: `${origin}${path}`, redial } satisfies ImportMetaEnv['LINK']));
    return { origin, define: { 'import.meta.env.LINK': link } };
});
const identity = Effect.fnUntraced(function* (directory: string) {
    const [identifier, { description }, prefix] = yield* Effect.all([IDENTIFIER, Package.read(directory), Config.String('MinVerTagPrefix')], { concurrency: 'unbounded' });
    const release = yield* runCommand('minver', ['-t', prefix, '-v', 'e']);
    const version = yield* Schema.decodeEffect(UxpManifest.Version)(Array.headNonEmpty(String.split(String.trim(release), PRERELEASE)));
    return { id: `${identifier}.${basename(directory)}`, name: description, version };
});

// --- [BUNDLE]

const bundle = (directory: string, config: UserConfig): UserConfig => mergeConfig({ build: { outDir: located('.artifacts', directory), emptyOutDir: false, rolldownOptions: { experimental: EXPERIMENTAL, output: { codeSplitting: false } } } }, config);
const bundled = (directory: string, asset: Required<Pick<Rolldown.EmittedAsset, 'fileName' | 'source'>>): UserConfig => {
    const load = createRequire(import.meta.url);
    const encoding = load.resolve('@exodus/bytes/encoding-lite.js');
    return bundle(directory, {
        build: {
            ssr: join(directory, `${ENTRY}.ts`),
            emptyOutDir: true,
            target: UxpManifest.target,
            rolldownOptions: {
                platform: 'neutral',
                transform: {
                    inject: Record.fromIterableWith(
                        Object.keys(load(encoding)).filter((name) => name in globalThis),
                        (name) => [name, [encoding, name] satisfies [string, string]],
                    ),
                },
                output: {
                    format: 'cjs',
                    entryFileNames: '[name].js',
                    paths: String.replace(HOSTED, ''),
                    plugins: {
                        name: 'asset',
                        generateBundle(this: Rolldown.PluginContext): void {
                            this.emitFile({ type: 'asset', ...asset });
                        },
                    },
                },
            },
        },
        environments: { ssr: { resolve: { noExternal: true, builtins: [HOSTED], conditions: Array.intersection(defaultServerConditions, defaultClientConditions) } } },
    });
};

// --- [COMPOSITION] ---------------------------------------------------------------------

const react = () => viteReact({ compiler: { panicThreshold: 'all_errors' } });
const cloudflareWorker = (directory: string, { plugins, hosting }: { readonly plugins: PluginOption; readonly hosting: PluginOption }): UserConfigFnPromise =>
    flow(
        Effect.fnUntraced(function* ({ mode }: ConfigEnv) {
            const [storybook, vitest] = yield* Effect.all([Config.withDefault(Config.Boolean('STORYBOOK'), false), Config.withDefault(Config.Boolean('VITEST'), false)]);
            const hosted =
                storybook || vitest ? [] : [hosting, (yield* Effect.promise(() => import('@cloudflare/vite-plugin'))).cloudflare({ types: { generate: false }, ...(mode === testMode ? { persistState: false, inspectorPort: false } : { persistState: { path: located(join('.cache', 'cloudflare'), directory) } }) })];
            return { build: { target: 'esnext', rolldownOptions: { experimental: EXPERIMENTAL } }, plugins: [plugins, hosted] };
        }),
        Effect.runPromise,
    );
const uxp = (directory: string, manifest: Omit<Fields, keyof Effect.Success<ReturnType<typeof identity>> | 'main' | 'host' | 'requiredPermissions'> & { readonly host: Omit<Fields['host'], 'data'>; readonly requiredPermissions: Omit<Fields['requiredPermissions'], 'network'> }, link: Linking): UserConfigFnPromise =>
    Effect.fnUntraced(
        function* () {
            const [{ origin, define }, release] = yield* Effect.all([dialed(directory, link), identity(directory)], { concurrency: 'unbounded' });
            const source = yield* Effect.orDie(Schema.encodeEffect(UxpManifest.json)(UxpManifest.make({ ...manifest, ...release, main: `${ENTRY}.js`, host: { ...manifest.host, data: { loadEvent: 'startup' } }, requiredPermissions: { ...manifest.requiredPermissions, network: { domains: [origin] } } })));
            return mergeConfig(bundled(directory, { fileName: UxpManifest.file, source }), { define });
        },
        Effect.provide(layer),
        Effect.runPromise,
    );
const cep = (directory: string, contract: { readonly host: ExtensionManifest['host']; readonly script: string }, link: Linking): UserConfigFnPromise =>
    Effect.fnUntraced(
        function* () {
            const [{ define }, release, { transformSync }] = yield* Effect.all([dialed(directory, link), identity(directory), Effect.promise(() => import('@swc/core'))], { concurrency: 'unbounded' });
            const load = createRequire(join(directory, Package.file));
            const extendscript = { name: 'extendscript', renderChunk: (code) => transformSync(code, { jsc: { target: ExtensionManifest.target, parser: { syntax: 'ecmascript' }, assumptions: { iterableIsArray: true } } }).code } satisfies Rolldown.Plugin;
            const panel = {
                name: 'panel',
                generateBundle(this: Rolldown.PluginContext): void {
                    this.emitFile({ type: 'asset', fileName: ExtensionManifest.panel, source: `<!doctype html><meta charset="utf-8"><script src="${ENTRY}.js"></script>\n` });
                    this.emitFile({ type: 'asset', fileName: ExtensionManifest.file, source: ExtensionManifest.make({ host: contract.host, ...release }).xml });
                },
            } satisfies Rolldown.Plugin;
            return bundle(directory, {
                builder: { sharedConfigBuild: true },
                define,
                build: { rolldownOptions: { preserveEntrySignatures: 'exports-only', output: { format: 'iife' } } },
                environments: {
                    client: { build: { target: ExtensionManifest.chromium, rolldownOptions: { input: { [ENTRY]: load.resolve('@rasm/cep/panel') }, output: { entryFileNames: '[name].js', plugins: panel } } } },
                    host: {
                        consumer: 'client',
                        build: { target: 'esnext', minify: false, rolldownOptions: { input: load.resolve(contract.script), platform: 'neutral', output: { name: ExtensionManifest.global, exports: 'default', entryFileNames: ExtensionManifest.script, plugins: extendscript } } },
                    },
                },
            });
        },
        Effect.provide(layer),
        Effect.runPromise,
    );
const uxpScript = (directory: string, extension: string): UserConfig => bundled(directory, { fileName: `${ENTRY}.${extension}`, source: `require('./${ENTRY}.js');\n` });
const sea = (directory: string, { executable, listen }: { readonly executable: string; readonly listen: string }): UserConfigFnPromise =>
    Effect.fnUntraced(
        function* () {
            const [{ name, port }, identifier, program] = yield* Effect.all([daemon(directory), IDENTIFIER, Config.String(executable)]);
            const addon = `@swc/core-${process.platform}-${process.arch}`;
            const binding = createRequire(createRequire(join(directory, Package.file)).resolve('@swc/core')).resolve(addon);
            const label = `${identifier}.server`;
            const agent = build({
                Label: label,
                BundleProgram: `Contents/Helpers/${name}`,
                AssociatedBundleIdentifiers: [identifier],
                KeepAlive: true,
                EnvironmentVariables: { [listen]: NetAddress.formatInet(yield* Effect.fromResult(NetAddress.inetAddress(NetAddress.ipv4Loopback, port))), [executable]: program, SWC_BINARY_PATH: join(dirname(dirname(program)), 'Frameworks', basename(binding)) },
            });
            return bundle(directory, {
                build: { ssr: true, rolldownOptions: { input: { [name]: join(directory, `${ENTRY}.ts`) }, external: (id) => id.startsWith('@swc/core-') && id !== addon, output: { entryFileNames: '[name].mjs' } } },
                ssr: { noExternal: true },
                plugins: [
                    {
                        name: 'sea',
                        enforce: 'pre',
                        resolveId: (id) => (id === addon ? `\0${addon}` : undefined),
                        load: (id) => (id === `\0${addon}` ? 'module.exports = require(process.env.SWC_BINARY_PATH);' : undefined),
                        generateBundle: {
                            order: 'post',
                            handler(this: Rolldown.PluginContext, _options, output): void {
                                const { outDir } = this.environment.config.build;
                                const folders = Array.dedupe(Object.values(output).flatMap((file) => (file.type === 'chunk' ? file.moduleIds.filter(isAbsolute).map(dirname) : [])));
                                const assets = Record.fromIterableWith(
                                    Array.flatMap(folders, (folder) => globSync('*.wasm', { cwd: folder, withFileTypes: true })),
                                    ({ name: file, parentPath }) => [file, join(parentPath, file)],
                                );
                                this.emitFile({ type: 'asset', fileName: 'sea.json', source: `${JSON.stringify({ main: join(outDir, `${name}.mjs`), mainFormat: 'module', output: join(outDir, name), useVfs: true, disableExperimentalSEAWarning: true, assets }, null, 4)}\n` });
                                this.emitFile({ type: 'asset', fileName: join('LaunchAgents', `${label}.plist`), source: agent });
                                this.emitFile({ type: 'asset', fileName: basename(binding), source: readFileSync(binding) });
                            },
                        },
                    } satisfies Plugin,
                ],
            });
        },
        Effect.provide(layer),
        Effect.runPromise,
    );

// --- [EXPORTS] -------------------------------------------------------------------------

export { cep, cloudflareWorker, located, react, sea, testMode, uxp, uxpScript };

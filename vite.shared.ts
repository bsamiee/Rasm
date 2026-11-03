import { globSync, readFileSync } from 'node:fs';
import { createRequire } from 'node:module';
import { basename, dirname, isAbsolute, join, relative, sep } from 'node:path';
import process from 'node:process';
import { NodeServices } from '@effect/platform-node';
import { ExtensionManifest } from '@rasm/cep/manifest';
import { Manifest } from '@rasm/uxp/manifest';
import viteReact from '@vitejs/plugin-react';
import { Array, Cause, Config, Effect, Function, Option, Predicate, Record, Result, Schema, String } from 'effect';
import type { HttpRouter } from 'effect/http';
import { NetAddress } from 'effect/net';
import { build } from 'plist';
import { defaultClientConditions, defaultServerConditions, mergeConfig, type Plugin, type PluginOption, type Rolldown, type UserConfig, type UserConfigFnPromise } from 'vite';
import { Mise, Package } from '#/eng/typescript/manifest';
import { run } from '#/eng/typescript/process';

// --- [TYPES] ---------------------------------------------------------------------------

type Fields = Parameters<typeof Manifest.make>[0];
type Linking = Omit<ImportMetaEnv['LINK'], 'endpoint'> & { readonly path: Exclude<HttpRouter.PathInput, '*'> };

// --- [CONSTANTS] -----------------------------------------------------------------------

const testMode = 'test';
const ROOT = import.meta.dirname;
const HOSTED = /^adobe:/u;
const EXPERIMENTAL = { lazyBarrel: true };
const ENTRY = 'main';
const PRERELEASE = /[-+]/u;
const IDENTIFIER = Config.String('PRODUCT_BUNDLE_IDENTIFIER');

// --- [OPERATIONS] ----------------------------------------------------------------------

// --- [PROJECT]

const located = (folder: string, directory: string) => join(ROOT, folder, relative(ROOT, directory));
const daemon = Effect.fnUntraced(function* (directory: string) {
    const { daemons } = yield* Mise.read(ROOT);
    const ports = Record.filterMap(daemons, (row) => (Predicate.isString(row) ? Result.failVoid : Result.fromOption(row.port, Function.constVoid)));
    const found = Array.findLast(relative(ROOT, directory).split(sep), (name) => Option.map(Record.get(ports, name), (port) => ({ name, port })));
    return yield* Effect.fromOption(found, () => new Cause.NoSuchElementError(`No mise daemon with a port names a folder of ${directory}`));
});
const dialed = Effect.fnUntraced(function* (directory: string, { path, redial }: Linking) {
    const [{ port }, { Link }] = yield* Effect.all([daemon(directory), Effect.promise(() => import('@rasm/uxp/link'))], { concurrency: 'unbounded' });
    const origin = Link.origin(port);
    return { origin, define: { 'import.meta.env.LINK': JSON.stringify({ endpoint: `${origin}${path}`, redial } satisfies ImportMetaEnv['LINK']) } };
});
const identity = Effect.fnUntraced(function* (directory: string) {
    const [identifier, { description }, prefix] = yield* Effect.all([IDENTIFIER, Package.read(directory), Config.String('MinVerTagPrefix')], { concurrency: 'unbounded' });
    const release = yield* run('minver', ['-t', prefix, '-v', 'e']);
    const version = yield* Schema.decodeEffect(Manifest.Version)(Array.headNonEmpty(String.split(String.trim(release), PRERELEASE)));
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
            target: Manifest.target,
            rolldownOptions: {
                platform: 'neutral',
                transform: { inject: Record.filterMap(load(encoding), (_, name: string) => (name in globalThis ? Result.succeed([encoding, name] satisfies [string, string]) : Result.failVoid)) },
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
const worker =
    (directory: string, { plugins, hosting }: { readonly plugins: PluginOption; readonly hosting: PluginOption }): UserConfigFnPromise =>
    ({ mode }) =>
        Effect.gen(function* () {
            const [storybook, vitest] = yield* Effect.all([Config.withDefault(Config.Boolean('STORYBOOK'), false), Config.withDefault(Config.Boolean('VITEST'), false)]);
            const hosted = storybook || vitest ? [] : [hosting, (yield* Effect.promise(() => import('@cloudflare/vite-plugin'))).cloudflare({ types: { generate: false }, ...(mode === testMode ? { persistState: false, inspectorPort: false } : { persistState: { path: located(join('.cache', 'cloudflare'), directory) } }) })];
            return { build: { target: 'esnext', rolldownOptions: { experimental: EXPERIMENTAL } }, plugins: [plugins, hosted] };
        }).pipe(Effect.runPromise);
const uxp =
    (directory: string, manifest: Omit<Fields, keyof Effect.Success<ReturnType<typeof identity>> | 'main' | 'host' | 'requiredPermissions'> & { readonly host: Omit<Fields['host'], 'data'>; readonly requiredPermissions: Omit<Fields['requiredPermissions'], 'network'> }, link: Linking): UserConfigFnPromise =>
    () =>
        Effect.gen(function* () {
            const [{ origin, define }, release] = yield* Effect.all([dialed(directory, link), identity(directory)], { concurrency: 'unbounded' });
            const source = yield* Effect.orDie(Schema.encodeEffect(Manifest.json)(Manifest.make({ ...manifest, ...release, main: `${ENTRY}.js`, host: { ...manifest.host, data: { loadEvent: 'startup' } }, requiredPermissions: { ...manifest.requiredPermissions, network: { domains: [origin] } } })));
            return mergeConfig(bundled(directory, { fileName: Manifest.file, source }), { define });
        }).pipe(Effect.provide(NodeServices.layer), Effect.runPromise);
const cep =
    (directory: string, contract: { readonly host: ExtensionManifest['host']; readonly script: string }, link: Linking): UserConfigFnPromise =>
    () =>
        Effect.gen(function* () {
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
                    client: { build: { target: ExtensionManifest.chromium, rolldownOptions: { input: { [ENTRY]: load.resolve('@rasm/cep/entrypoints') }, output: { entryFileNames: '[name].js', plugins: panel } } } },
                    host: {
                        consumer: 'client',
                        build: { target: 'esnext', minify: false, rolldownOptions: { input: load.resolve(contract.script), platform: 'neutral', output: { name: ExtensionManifest.global, exports: 'default', entryFileNames: ExtensionManifest.script, plugins: extendscript } } },
                    },
                },
            });
        }).pipe(Effect.provide(NodeServices.layer), Effect.runPromise);
const script = (directory: string, extension: string): UserConfig => bundled(directory, { fileName: `${ENTRY}.${extension}`, source: `require('./${ENTRY}.js');\n` });
const sea =
    (directory: string, { executable, listen }: { readonly executable: string; readonly listen: string }): UserConfigFnPromise =>
    () =>
        Effect.gen(function* () {
            const [{ name, port }, identifier, program] = yield* Effect.all([daemon(directory), IDENTIFIER, Config.String(executable)]);
            const load = createRequire(join(directory, Package.file));
            const addon = `@swc/core-${process.platform}-${process.arch}`;
            const binding = createRequire(load.resolve('@swc/core')).resolve(addon);
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
                                const outDir = located('.artifacts', directory);
                                const folders = Array.dedupe(Object.values(output).flatMap((file) => (file.type === 'chunk' ? file.moduleIds.filter(isAbsolute).map(dirname) : [])));
                                const assets = Record.fromEntries(Array.flatMap(folders, (folder) => globSync('*.wasm', { cwd: folder }).map((file) => [file, join(folder, file)] as const)));
                                this.emitFile({ type: 'asset', fileName: 'sea.json', source: `${JSON.stringify({ main: join(outDir, `${name}.mjs`), mainFormat: 'module', output: join(outDir, name), useVfs: true, disableExperimentalSEAWarning: true, assets }, null, 4)}\n` });
                                this.emitFile({ type: 'asset', fileName: join('LaunchAgents', `${label}.plist`), source: agent });
                                this.emitFile({ type: 'asset', fileName: basename(binding), source: readFileSync(binding) });
                            },
                        },
                    } satisfies Plugin,
                ],
            });
        }).pipe(Effect.provide(NodeServices.layer), Effect.runPromise);

// --- [EXPORTS] -------------------------------------------------------------------------

export { cep, located, react, script, sea, testMode, uxp, worker };

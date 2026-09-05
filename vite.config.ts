// --- [IMPORTS] -------------------------------------------------------------------------

import babel from '@rolldown/plugin-babel';
import tailwindcss from '@tailwindcss/vite';
import react, { reactCompilerPreset } from '@vitejs/plugin-react';
import { Array, Boolean, Config, DateTime, Effect, Match, Option, Record, Schema } from 'effect';
import { visualizer } from 'rollup-plugin-visualizer';
import { type Plugin, type PluginOption, perEnvironmentPlugin, type UserConfig, type ViteBuilder } from 'vite';
import { compression } from 'vite-plugin-compression2';
import csp from 'vite-plugin-csp-guard';
import { ViteImageOptimizer } from 'vite-plugin-image-optimizer';
import Inspect from 'vite-plugin-inspect';
import { VitePWA } from 'vite-plugin-pwa';
import svgr from 'vite-plugin-svgr';
import webfontDownload from 'vite-plugin-webfont-dl';
import type { RuntimeCaching } from 'workbox-build';

// --- [CONSTANTS] -----------------------------------------------------------------------

const _ROOT = import.meta.dirname;
const _PORT = { min: 1024, max: 65_535, default: 3000 };
const _ICON = { small: 192, large: 512 };
const _CACHE = { api: 300, cdn: 604_800, max: 50 };
const _COMPRESSION = { threshold: 10_240, include: /\.(?:js|mjs|json|css|html|svg)$/iu };
const _CHUNK = {
    minSize: 10_240,
    react: /\/node_modules\/(?:\.pnpm\/[^/]+\/node_modules\/)?react(?:-dom)?\//u,
    effect: /\/node_modules\/(?:\.pnpm\/[^/]+\/node_modules\/)?(?:@effect|effect)\//u,
    vendor: /node_modules/u,
};
const _ORIGIN = { cdn: /^https:\/\/cdn\./u, api: /^https:\/\/api\./u };
const _NODE_BUILTIN = /^node:/u;
const _IMAGE = {
    test: /\.(?:jpe?g|png|gif|tiff|webp|svg|avif)$/iu,
    exclude: /^(?:virtual:|node_modules)/u,
    quality: { avif: 70, jpeg: 75, png: 80, webp: 80 },
};
const _ASSETS = ['bin', 'exr', 'fbx', 'glb', 'gltf', 'hdr', 'mtl', 'obj', 'wasm'];
const _ASSET_GLOB = '**/*.{js,css,html,ico,png,svg,wasm,glb,gltf}';
const _EXTENSIONS = ['.mjs', '.js', '.mts', '.ts', '.jsx', '.tsx', '.json'];
const _CSP: Record<string, readonly string[]> = {
    'connect-src': ["'self'", 'https:', 'wss:', 'ws:'],
    'default-src': ["'self'"],
    'font-src': ["'self'", 'https://fonts.gstatic.com', 'data:'],
    'img-src': ["'self'", 'data:', 'blob:', 'https:'],
    'script-src': ["'self'"],
    'style-src': ["'self'", "'unsafe-inline'"],
    'worker-src': ["'self'", 'blob:'],
};
const _SSR = {
    external: ['react', 'react-dom', 'react/jsx-runtime', 'react/compiler-runtime'],
    noExternal: ['@effect/platform', '@effect/platform-browser', '@effect/experimental'],
};
const _IGNORED = ['node_modules', '.git', 'dist', 'build', 'out', '.nx', '.vite', '.cache', '.artifacts', '.history', '__pycache__', '.venv', 'venv'];
const _SVGR = { exportType: 'default', memo: true, ref: true, svgo: true, titleProp: true, typescript: true } as const;
const _TREESHAKE = { moduleSideEffects: 'no-external', propertyReadSideEffects: false, unknownGlobalSideEffects: false } as const;
const _VISUALIZER = {
    brotliSize: true,
    emitFile: true,
    exclude: [{ file: '**/node_modules/react-compiler-runtime/**' }],
    filename: '.vite/stats.html',
    gzipSize: true,
    open: false,
    projectRoot: _ROOT,
    sourcemap: true,
    template: 'treemap',
} as const;

// --- [MODELS] --------------------------------------------------------------------------

// Every optional field with a default holds it in the schema, root and pwa alone stay absent when unset
const _App = Schema.Struct({
    kind: Schema.Literal('app'),
    name: Schema.String,
    root: Schema.optional(Schema.String),
    port: Schema.optionalWith(Schema.Int.pipe(Schema.between(_PORT.min, _PORT.max)), { default: () => _PORT.default }),
    assetExtensions: Schema.optionalWith(Schema.Array(Schema.String), { default: () => _ASSETS }),
    builder: Schema.optionalWith(
        Schema.Struct({
            sharedConfigBuild: Schema.optionalWith(Schema.Boolean, { default: () => true }),
            sharedPlugins: Schema.optionalWith(Schema.Boolean, { default: () => true }),
        }),
        { default: () => ({ sharedConfigBuild: true, sharedPlugins: true }) },
    ),
    compressionThreshold: Schema.optionalWith(Schema.Int.pipe(Schema.positive()), { default: () => _COMPRESSION.threshold }),
    cspPolicy: Schema.optionalWith(Schema.Record({ key: Schema.String, value: Schema.Array(Schema.String) }), { default: () => _CSP }),
    imageQuality: Schema.optionalWith(Schema.Struct({ avif: Schema.Number, jpeg: Schema.Number, png: Schema.Number, webp: Schema.Number }), {
        default: () => _IMAGE.quality,
    }),
    pwa: Schema.optional(Schema.Struct({ description: Schema.String, name: Schema.String, shortName: Schema.String, themeColor: Schema.String })),
    warmup: Schema.optionalWith(Schema.Array(Schema.String), { default: () => [] }),
    webfonts: Schema.optionalWith(Schema.Array(Schema.String), { default: () => [] }),
});

const _Library = Schema.Struct({
    kind: Schema.Literal('library'),
    name: Schema.String,
    entry: Schema.Union(Schema.String, Schema.Record({ key: Schema.String, value: Schema.String })),
    css: Schema.optional(Schema.String),
    external: Schema.optionalWith(Schema.Array(Schema.String), { default: () => [] }),
    react: Schema.optionalWith(Schema.Boolean, { default: () => false }),
});

// Server builds keep bare specifiers external except each bundle entry and its subpaths
const _Server = Schema.Struct({
    kind: Schema.Literal('server'),
    name: Schema.String,
    entry: Schema.String,
    bundle: Schema.optionalWith(Schema.Array(Schema.String), { default: () => [] }),
});

const _ViteConfig = Schema.Union(_App, _Library, _Server);

type AppConfig = typeof _App.Type;
type LibraryConfig = typeof _Library.Type;
type ServerConfig = typeof _Server.Type;

// --- [CONFIGURATION] -------------------------------------------------------------------

// The version falls back to 0.0.0 outside a package script
const _buildEnv = Effect.all({
    production: Config.string('NODE_ENV').pipe(
        Config.withDefault(''),
        Config.map((mode) => mode === 'production'),
    ),
    time: Effect.map(DateTime.now, DateTime.formatIso),
    version: Config.withDefault(Config.string('npm_package_version'), '0.0.0'),
    apiUrl: Config.withDefault(Config.string('VITE_API_URL'), '/api'),
}).pipe(Effect.orDie);

// --- [OPERATIONS] ----------------------------------------------------------------------

const _clientOnly = (plugin: Plugin): Plugin =>
    perEnvironmentPlugin(plugin.name, (environment) => environment.config.consumer === 'client' && plugin);

const _reactPlugins = (): PluginOption[] => [react(), babel({ presets: [reactCompilerPreset()] })];

const _outputFileNames = (prefix: string, entries: string): { assetFileNames: string; chunkFileNames: string; entryFileNames: string } => ({
    assetFileNames: `${prefix}assets/[name]-[hash][extname]`,
    chunkFileNames: `${prefix}chunks/[name]-[hash].js`,
    entryFileNames: `${prefix}${entries}[name]-[hash].js`,
});

const _resolve = (browser: boolean): NonNullable<UserConfig['resolve']> => ({
    conditions: Boolean.match(browser, { onFalse: () => ['import', 'module', 'default'], onTrue: () => ['import', 'module', 'browser', 'default'] }),
    ...Record.getSomes({
        dedupe: Option.liftPredicate(['react', 'react-dom'], () => browser),
        extensions: Option.liftPredicate(_EXTENSIONS, () => browser),
    }),
    tsconfigPaths: true,
});

const _runtimeCache = (handler: 'CacheFirst' | 'NetworkFirst', cacheName: string, maxAgeSeconds: number, urlPattern: RegExp): RuntimeCaching => ({
    handler,
    options: { cacheName, expiration: { maxAgeSeconds, maxEntries: _CACHE.max } },
    urlPattern,
});

const _pwa = (config: AppConfig, pwa: NonNullable<AppConfig['pwa']>): Plugin[] =>
    Array.map(
        VitePWA({
            devOptions: { enabled: false },
            includeAssets: Array.map(config.assetExtensions, (extension) => `**/*.${extension}`),
            manifest: {
                background_color: '#ffffff',
                description: pwa.description,
                display: 'standalone',
                icons: [
                    ...Array.map([_ICON.small, _ICON.large], (size) => ({
                        purpose: 'any',
                        sizes: `${size}x${size}`,
                        src: `/icon-${size}.png`,
                        type: 'image/png',
                    })),
                    { purpose: 'maskable', sizes: `${_ICON.large}x${_ICON.large}`, src: '/icon-maskable.png', type: 'image/png' },
                ],
                name: pwa.name,
                scope: '/',
                short_name: pwa.shortName,
                start_url: '/',
                theme_color: pwa.themeColor,
            },
            registerType: 'autoUpdate',
            workbox: {
                clientsClaim: true,
                globPatterns: [_ASSET_GLOB],
                runtimeCaching: [
                    _runtimeCache('CacheFirst', 'cdn-cache', _CACHE.cdn, _ORIGIN.cdn),
                    _runtimeCache('NetworkFirst', 'api-cache', _CACHE.api, _ORIGIN.api),
                ],
                skipWaiting: true,
            },
        }),
        _clientOnly,
    );

// Compression and Inspect apply to production builds alone, HMR restarts can produce an EEXIST race under Inspect
const _appPlugins = (config: AppConfig, production: boolean): PluginOption[] => [
    ..._reactPlugins(),
    tailwindcss({ optimize: { minify: true } }),
    ...Array.flatMap(Array.fromNullable(config.pwa), (pwa) => _pwa(config, pwa)),
    svgr({ exclude: '', include: '**/*.svg?react', svgrOptions: _SVGR }),
    _clientOnly(
        ViteImageOptimizer({
            avif: { lossless: false, quality: config.imageQuality.avif },
            exclude: _IMAGE.exclude,
            includePublic: true,
            jpeg: { progressive: true, quality: config.imageQuality.jpeg },
            logStats: true,
            png: { quality: config.imageQuality.png },
            test: _IMAGE.test,
            webp: { lossless: false, quality: config.imageQuality.webp },
        }),
    ),
    _clientOnly(webfontDownload(Array.fromIterable(config.webfonts))),
    {
        ...perEnvironmentPlugin(
            'visualizer',
            (environment) => environment.config.consumer === 'client' && visualizer({ ..._VISUALIZER, exclude: [..._VISUALIZER.exclude] }),
        ),
        apply: 'build',
    },
    {
        ..._clientOnly(
            compression({
                algorithms: ['brotliCompress', 'gzip'],
                include: _COMPRESSION.include,
                skipIfLargerOrEqual: true,
                threshold: config.compressionThreshold,
            }),
        ),
        apply: () => production,
    },
    _clientOnly(csp({ algorithm: 'sha256', build: { sri: true }, policy: Record.map(config.cspPolicy, Array.fromIterable) })),
    { ...Inspect({ build: true, dev: false, outputDir: `${config.root ?? _ROOT}/.cache/vite-inspect` }), apply: () => production },
];

// `strictPort` keeps preview on the URL the end-to-end tests expect
const _appServer = (config: AppConfig): Pick<UserConfig, 'preview' | 'server' | 'ssr' | 'worker'> => ({
    preview: { port: config.port, strictPort: true },
    server: {
        cors: true,
        port: config.port,
        warmup: { clientFiles: Array.fromIterable(config.warmup) },
        watch: {
            ignored: [...Array.map(_IGNORED, (directory) => `**/${directory}/**`), '**/*.pyc', '**/*.log', '**/.DS_Store'],
            ignoreInitial: true,
        },
    },
    ssr: {
        external: _SSR.external,
        noExternal: _SSR.noExternal,
        optimizeDeps: { include: ['@effect/platform'] },
        resolve: { conditions: ['node', 'import', 'module', 'default'], externalConditions: ['node'] },
        target: 'node',
    },
    worker: { format: 'es', plugins: _reactPlugins, rolldownOptions: { output: _outputFileNames('workers/', '') } },
});

const _app = (config: AppConfig, env: Effect.Effect.Success<typeof _buildEnv>): UserConfig => ({
    appType: 'spa',
    assetsInclude: Array.map(config.assetExtensions, (extension) => `**/*.${extension}`),
    ...Record.getSomes({ root: Option.fromNullable(config.root) }),
    build: {
        chunkImportMap: true,
        emptyOutDir: true,
        manifest: true,
        reportCompressedSize: false,
        rolldownOptions: {
            output: {
                ..._outputFileNames('', 'entries/'),
                codeSplitting: {
                    groups: [
                        { name: 'vendor-react', priority: 3, test: _CHUNK.react },
                        { name: 'vendor-effect', priority: 2, test: _CHUNK.effect },
                        { name: 'vendor', priority: 1, test: _CHUNK.vendor },
                    ],
                    minSize: _CHUNK.minSize,
                },
            },
            treeshake: _TREESHAKE,
        },
        sourcemap: true,
    },
    builder: {
        buildApp: async ({ build, environments: { client } }: ViteBuilder): Promise<void> => {
            if (client) {
                await build(client);
            }
        },
        sharedConfigBuild: config.builder.sharedConfigBuild,
        sharedPlugins: config.builder.sharedPlugins,
    },
    cacheDir: Option.match(Option.fromNullable(config.root), {
        onNone: () => `${_ROOT}/.cache/vite`,
        onSome: (root) => `${root}/node_modules/.vite`,
    }),
    css: { devSourcemap: true, transformer: 'lightningcss' },
    define: {
        'import.meta.env.APP_VERSION': JSON.stringify(env.version),
        'import.meta.env.BUILD_MODE': JSON.stringify(Boolean.match(env.production, { onFalse: () => 'development', onTrue: () => 'production' })),
        'import.meta.env.BUILD_TIME': JSON.stringify(env.time),
        'import.meta.env.VITE_API_URL': JSON.stringify(env.apiUrl),
    },
    devtools: true,
    future: 'warn',
    optimizeDeps: { exclude: _SSR.noExternal, include: [..._SSR.external, 'react-aria-components', '@floating-ui/react', 'effect'] },
    plugins: _appPlugins(config, env.production),
    resolve: _resolve(true),
    ..._appServer(config),
});

const _library = (config: LibraryConfig): UserConfig => ({
    build: {
        lib: {
            entry: config.entry,
            fileName: (format, name) =>
                Match.value(format).pipe(
                    Match.when('es', () => `${name}.js`),
                    Match.orElse(() => `${name}.${format}.js`),
                ),
            formats: ['es', 'cjs'],
            name: config.name,
        },
        rolldownOptions: { external: [_NODE_BUILTIN, ...config.external], output: { exports: 'named' } },
        sourcemap: true,
        target: 'esnext',
    },
    css: { devSourcemap: config.css !== undefined, transformer: 'lightningcss' },
    future: 'warn',
    plugins: [
        ...Option.liftPredicate(config, (library) => library.react).pipe(Option.map(_reactPlugins), Option.toArray),
        ...Array.map(Array.fromNullable(config.css), () => tailwindcss({ optimize: { minify: true } })),
    ],
    resolve: _resolve(config.react),
    ssr: { target: 'node' },
});

const _server = (config: ServerConfig): UserConfig => ({
    build: {
        lib: { entry: config.entry, fileName: 'main', formats: ['es'], name: config.name },
        rolldownOptions: {
            external: (id: string) =>
                !(
                    id.startsWith('.') ||
                    id.startsWith('/') ||
                    Array.some(config.bundle, (dependency) => id === dependency || id.startsWith(`${dependency}/`))
                ),
            output: { exports: 'named' },
        },
        sourcemap: true,
        target: 'node24',
    },
    future: 'warn',
    plugins: [],
    resolve: _resolve(false),
    ssr: { target: 'node' },
});

// --- [COMPOSITION] ---------------------------------------------------------------------

const createViteConfig = (input: unknown): Effect.Effect<UserConfig> =>
    Effect.map(Effect.all({ config: Effect.orDie(Schema.decodeUnknown(_ViteConfig)(input)), env: _buildEnv }), ({ config, env }) =>
        Match.value(config).pipe(Match.discriminatorsExhaustive('kind')({ app: (app) => _app(app, env), library: _library, server: _server })),
    );

// The root configuration omits the build block, app and library configs alone emit output
const rootConfig: Promise<UserConfig> = Effect.runPromise(
    Effect.map(createViteConfig({ entry: './vite.config.ts', kind: 'library', name: 'workspace' }), ({ build: _, ...options }) => options),
);

// --- [EXPORTS] -------------------------------------------------------------------------

export { createViteConfig };
export default rootConfig;

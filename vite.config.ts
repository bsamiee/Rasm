/// <reference types="vite/client" />
/**
 * Root Vite authority: createConfig parameterizes every build mode (app | library | server) and
 * the default export is the buildless root anchor for dev/typecheck parity — the root emits NO
 * artifact. App and package configs import createConfig from this file and own their outputs.
 */

// --- [RUNTIME_PRELUDE] -----------------------------------------------------------------

import { dirname, resolve as pathResolve } from 'node:path';
import { fileURLToPath } from 'node:url';
import babel from '@rolldown/plugin-babel';
import tailwindcss from '@tailwindcss/vite';
import react, { reactCompilerPreset } from '@vitejs/plugin-react';
import { DateTime, Effect, Match, Option, pipe, Schema as S } from 'effect';
import { visualizer } from 'rollup-plugin-visualizer';
import { defineConfig, type Plugin, type UserConfig, type ViteBuilder } from 'vite';
import { compression } from 'vite-plugin-compression2';
import csp from 'vite-plugin-csp-guard';
import { ViteImageOptimizer } from 'vite-plugin-image-optimizer';
import Inspect from 'vite-plugin-inspect';
import { VitePWA } from 'vite-plugin-pwa';
import svgr from 'vite-plugin-svgr';
import webfontDownload from 'vite-plugin-webfont-dl';

// --- [TYPES] ---------------------------------------------------------------------------

type EnvironmentConsumer = { readonly config: { readonly consumer: 'client' | 'server' } };
type BuildRuntimeEnv = NodeJS.ProcessEnv & {
    readonly NODE_ENV?: string;
    readonly VITE_API_URL?: string;
    readonly npm_package_version?: string;
};
type Cfg = S.Schema.Type<typeof CfgSchema>;
type Mode = Cfg['mode'];

// --- [MODELS] --------------------------------------------------------------------------

const CfgSchema = S.Union(
    S.Struct({
        assetExts: S.optional(S.Array(S.String)),
        builder: S.optional(
            S.Struct({
                sharedConfigBuild: S.optional(S.Boolean),
                sharedPlugins: S.optional(S.Boolean),
            }),
        ),
        compressionThreshold: S.optional(pipe(S.Number, S.int(), S.positive())),
        cspPolicy: S.optional(S.Record({ key: S.String, value: S.Array(S.String) })),
        imageQuality: S.optional(S.Struct({ avif: S.Number, jpeg: S.Number, png: S.Number, webp: S.Number })),
        mode: S.Literal('app'),
        name: S.String,
        port: S.optional(pipe(S.Number, S.int(), S.between(1024, 65535))),
        pwa: S.optional(S.Struct({ description: S.String, name: S.String, shortName: S.String, themeColor: S.String })),
        root: S.optional(S.String),
        warmup: S.optional(S.Array(S.String)),
        webfonts: S.optional(S.Array(S.String)),
    }),
    S.Struct({
        css: S.optional(S.String),
        entry: S.Union(S.String, S.Record({ key: S.String, value: S.String })),
        external: S.optional(S.Array(S.String)),
        mode: S.Literal('library'),
        name: S.String,
        react: S.optional(S.Boolean),
    }),
    S.Struct({
        // Server bundles invert the external default: every bare specifier stays external and `bundle` opts a dependency subtree INTO the artifact — no curated external roster exists.
        bundle: S.optional(S.Array(S.String)),
        entry: S.String,
        mode: S.Literal('server'),
        name: S.String,
    }),
);

// --- [CONSTANTS] -----------------------------------------------------------------------

const ROOT_DIR = dirname(fileURLToPath(import.meta.url));
const _ENV: BuildRuntimeEnv = process.env;
const B = Object.freeze({
    assets: ['bin', 'exr', 'fbx', 'glb', 'gltf', 'hdr', 'mtl', 'obj', 'wasm'],
    builder: { sharedConfigBuild: true, sharedPlugins: true },
    cache: { api: 300, cdn: 604800, max: 50 },
    comp: { f: /\.(js|mjs|json|css|html|svg)$/i, t: 10240 },
    csp: {
        'connect-src': ["'self'", 'https:', 'wss:', 'ws:'],
        'default-src': ["'self'"],
        'font-src': ["'self'", 'https://fonts.gstatic.com', 'data:'],
        'img-src': ["'self'", 'data:', 'blob:', 'https:'],
        'script-src': ["'self'"],
        'style-src': ["'self'", "'unsafe-inline'"],
        'worker-src': ["'self'", 'blob:'],
    },
    exts: ['.mjs', '.js', '.mts', '.ts', '.jsx', '.tsx', '.json'],
    glob: '**/*.{js,css,html,ico,png,svg,wasm,glb,gltf}',
    img: { avif: 70, jpeg: 75, png: 80, webp: 80 },
    port: 3000,
    pwa: {
        bg: '#ffffff',
        desc: 'Universal workspace foundation',
        name: 'Workspace',
        short: 'Workspace',
        theme: '#000000',
    },
    // Declarative Rolldown code-splitting rows: priority replaces the retired weight-sort walk, and
    // every test embeds node_modules so app modules never leak into a vendor chunk.
    split: {
        groups: [
            { name: 'vendor-react', priority: 3, test: /\/node_modules\/(?:\.pnpm\/[^/]+\/node_modules\/)?react(?:-dom)?\// },
            { name: 'vendor-effect', priority: 2, test: /\/node_modules\/(?:\.pnpm\/[^/]+\/node_modules\/)?(?:@effect|effect)\// },
            { name: 'vendor', priority: 1, test: /node_modules/ },
        ],
        minSize: 10240,
    },
    ssr: {
        ext: ['react', 'react-dom', 'react/jsx-runtime', 'react/compiler-runtime'],
        noExt: ['@effect/platform', '@effect/platform-browser', '@effect/experimental'],
    },
    svgr: { exportType: 'default', memo: true, ref: true, svgo: true, titleProp: true, typescript: true },
    treeshake: {
        moduleSideEffects: 'no-external' as const,
        propertyReadSideEffects: false as const,
        unknownGlobalSideEffects: false,
    },
    viz: {
        brotliSize: true,
        emitFile: true,
        exclude: [{ file: '**/node_modules/react-compiler-runtime/**' }],
        filename: '.vite/stats.html',
        gzipSize: true,
        open: false,
        sourcemap: true,
        template: 'treemap' as const,
    },
} as const);

// --- [OPERATIONS] ----------------------------------------------------------------------

const css = (dev = false) => ({
    devSourcemap: dev,
    transformer: 'lightningcss' as const,
});
const cache = <H extends 'CacheFirst' | 'NetworkFirst'>(h: H, n: string, s: number, u: RegExp) => ({
    handler: h,
    options: { cacheName: n, expiration: { maxAgeSeconds: s, maxEntries: B.cache.max } },
    urlPattern: u,
});
const output = (p = '') => ({
    assetFileNames: `${p}assets/[name]-[hash][extname]`,
    chunkFileNames: `${p}chunks/[name]-[hash].js`,
    entryFileNames: `${p}${p ? '' : 'entries/'}[name]-[hash].js`,
});
const resolve = (browser = false) => ({
    conditions: browser ? ['import', 'module', 'browser', 'default'] : ['import', 'module', 'default'],
    ...(browser ? { dedupe: ['react', 'react-dom'], extensions: [...B.exts] } : {}),
    tsconfigPaths: true,
});
const clientOnly = (plugin: Plugin): Plugin => ({
    ...plugin,
    applyToEnvironment: (environment: EnvironmentConsumer) => environment.config.consumer === 'client',
});

// --- [COMPOSITION] ---------------------------------------------------------------------

const buildApp = ({ build, environments: { client } }: ViteBuilder): Promise<void> =>
    pipe(
        Option.fromNullable(client),
        Option.match({
            onNone: () => Promise.resolve(),
            onSome: (env) => build(env).then(() => undefined),
        }),
    );
const plugins = {
    app: (c: Extract<Cfg, { mode: 'app' }>, prod: boolean) => [
        react(),
        babel({ presets: [reactCompilerPreset()] }),
        tailwindcss({ optimize: { minify: true } }),
        ...(c.pwa
            ? VitePWA({
                  devOptions: { enabled: false },
                  includeAssets: (c.assetExts ?? B.assets).map((x) => `**/*.${x}`),
                  manifest: {
                      background_color: B.pwa.bg,
                      description: c.pwa.description,
                      display: 'standalone' as const,
                      icons: [
                          ...[192, 512].map((s) => ({
                              purpose: 'any' as const,
                              sizes: `${s}x${s}`,
                              src: `/icon-${s}.png`,
                              type: 'image/png',
                          })),
                          { purpose: 'maskable' as const, sizes: '512x512', src: '/icon-maskable.png', type: 'image/png' },
                      ],
                      name: c.pwa.name,
                      scope: '/',
                      short_name: c.pwa.shortName,
                      start_url: '/',
                      theme_color: c.pwa.themeColor,
                  },
                  registerType: 'autoUpdate',
                  workbox: {
                      clientsClaim: true,
                      globPatterns: [B.glob],
                      runtimeCaching: [
                          cache('CacheFirst', 'cdn-cache', B.cache.cdn, /^https:\/\/cdn\./),
                          cache('NetworkFirst', 'api-cache', B.cache.api, /^https:\/\/api\./),
                      ],
                      skipWaiting: true,
                  },
              }).map((p) => clientOnly(p))
            : []),
        svgr({ exclude: '', include: '**/*.svg?react', svgrOptions: B.svgr }),
        clientOnly(
            ViteImageOptimizer({
                avif: { lossless: false, quality: (c.imageQuality ?? B.img).avif },
                exclude: /^(?:virtual:|node_modules)/,
                includePublic: true,
                jpeg: { progressive: true, quality: (c.imageQuality ?? B.img).jpeg },
                logStats: true,
                png: { quality: (c.imageQuality ?? B.img).png },
                test: /\.(jpe?g|png|gif|tiff|webp|svg|avif)$/i,
                webp: { lossless: false, quality: (c.imageQuality ?? B.img).webp },
            }),
        ),
        clientOnly(webfontDownload([...(c.webfonts ?? [])])),
        clientOnly({
            ...visualizer({ ...B.viz, exclude: [...B.viz.exclude], projectRoot: process.cwd() }),
            apply: 'build',
        } as unknown as Plugin),
        ...(prod
            ? [
                  clientOnly(
                      compression({
                          algorithms: ['brotliCompress', 'gzip'],
                          include: B.comp.f,
                          skipIfLargerOrEqual: true,
                          threshold: c.compressionThreshold ?? B.comp.t,
                      }),
                  ),
              ]
            : []),
        clientOnly(
            csp({
                algorithm: 'sha256',
                build: { sri: true },
                policy: Object.fromEntries(Object.entries(c.cspPolicy ?? B.csp).map(([k, v]) => [k, [...v]])),
            }),
        ),
        // Inspect disabled in dev due to EEXIST race condition on HMR restart
        ...(prod ? [Inspect({ build: true, dev: false, outputDir: pathResolve(c.root ?? ROOT_DIR, '.cache/vite-inspect') })] : []),
    ],
    library: (c: Extract<Cfg, { mode: 'library' }>) => [
        ...(c.react === true ? [react(), babel({ presets: [reactCompilerPreset()] })] : []),
        ...(c.css === undefined ? [] : [tailwindcss({ optimize: { minify: true } })]),
    ],
    server: () => [],
} as const;
const config: {
    readonly [M in Mode]: (c: Extract<Cfg, { mode: M }>, env: { prod: boolean; time: string; ver: string }) => UserConfig;
} = {
    app: (c, { prod, time, ver }) => ({
        appType: 'spa',
        assetsInclude: (c.assetExts ?? B.assets).map((x) => `**/*.${x}`),
        ...(c.root ? { root: c.root } : {}),
        build: {
            chunkImportMap: true,
            emptyOutDir: true,
            manifest: true,
            reportCompressedSize: false,
            rolldownOptions: {
                output: { ...output(), codeSplitting: { groups: [...B.split.groups], minSize: B.split.minSize } },
                treeshake: B.treeshake,
            },
            sourcemap: true,
        },
        builder: {
            buildApp,
            sharedConfigBuild: c.builder?.sharedConfigBuild ?? B.builder.sharedConfigBuild,
            sharedPlugins: c.builder?.sharedPlugins ?? B.builder.sharedPlugins,
        },
        cacheDir: c.root ? pathResolve(c.root, 'node_modules/.vite') : pathResolve(ROOT_DIR, '.cache/vite'),
        css: css(true),
        define: {
            'import.meta.env.APP_VERSION': JSON.stringify(ver),
            'import.meta.env.BUILD_MODE': JSON.stringify(prod ? 'production' : 'development'),
            'import.meta.env.BUILD_TIME': JSON.stringify(time),
            'import.meta.env.VITE_API_URL': JSON.stringify(_ENV.VITE_API_URL ?? '/api'),
        },
        devtools: true,
        future: 'warn',
        optimizeDeps: {
            exclude: [...B.ssr.noExt],
            include: [...B.ssr.ext, 'react-aria-components', '@floating-ui/react', 'effect'],
        },
        plugins: plugins.app(c, prod),
        // strictPort pins the preview endpoint: an e2e target's webServer row keys its readiness URL
        // on this port, so a silent port drift can never strand the served-product lane.
        preview: { port: c.port ?? B.port, strictPort: true },
        resolve: resolve(true),
        server: {
            cors: true,
            port: c.port ?? B.port,
            ...(c.warmup === undefined ? {} : { warmup: { clientFiles: [...c.warmup] } }),
            watch: {
                ignored: [
                    '**/node_modules/**',
                    '**/.git/**',
                    '**/dist/**',
                    '**/build/**',
                    '**/out/**',
                    '**/.nx/**',
                    '**/.vite/**',
                    '**/.cache/**',
                    '**/.artifacts/**',
                    '**/.history/**',
                    '**/__pycache__/**',
                    '**/.venv/**',
                    '**/venv/**',
                    '**/*.pyc',
                    '**/*.log',
                    '**/.DS_Store',
                ],
                ignoreInitial: true,
            },
        },
        ssr: {
            external: [...B.ssr.ext],
            noExternal: [...B.ssr.noExt],
            optimizeDeps: { include: ['@effect/platform'] },
            resolve: { conditions: ['node', 'import', 'module', 'default'], externalConditions: ['node'] },
            target: 'node',
        },
        worker: {
            format: 'es',
            plugins: () => [react(), babel({ presets: [reactCompilerPreset()] })],
            rolldownOptions: { output: output('workers/') },
        },
    }),
    library: (c) => ({
        build: {
            lib: {
                entry: c.entry,
                fileName: (f: string, n: string) => (f === 'es' ? `${n}.js` : `${n}.${f}.js`),
                formats: ['es', 'cjs'],
                name: c.name,
            },
            rolldownOptions: {
                external: [/^node:/, ...(c.external ?? [])],
                output: { exports: 'named' },
            },
            sourcemap: true,
            target: 'esnext',
        },
        css: css(c.css !== undefined),
        future: 'warn',
        plugins: plugins.library(c),
        resolve: resolve(c.react === true),
        ssr: { target: 'node' as const },
    }),
    server: (c) => ({
        build: {
            lib: { entry: c.entry, fileName: 'main', formats: ['es'] as const, name: c.name },
            rolldownOptions: {
                // Bare specifiers stay external by default — deps resolve from node_modules at run time — and `bundle` roots opt their subtrees into the artifact.
                external: (id: string) =>
                    !id.startsWith('.') && !id.startsWith('/') && !(c.bundle ?? []).some((b) => id === b || id.startsWith(`${b}/`)),
                output: { exports: 'named' as const },
            },
            sourcemap: true,
            target: 'node24',
        },
        future: 'warn',
        plugins: plugins.server(),
        resolve: resolve(),
        ssr: { target: 'node' as const },
    }),
};

const createConfig = (input: unknown): Effect.Effect<UserConfig, never, never> =>
    Effect.map(
        Effect.all({
            c: Effect.orDie(S.decodeUnknown(CfgSchema)(input)),
            env: Effect.map(DateTime.now, (now) => ({
                prod: _ENV.NODE_ENV === 'production',
                time: DateTime.formatIso(now),
                ver: _ENV.npm_package_version ?? '0.0.0',
            })),
        }),
        ({ c, env }) =>
            Match.value(c).pipe(
                Match.discriminatorsExhaustive('mode')({
                    app: (cfg) => config.app(cfg, env),
                    library: (cfg) => config.library(cfg, env),
                    server: (cfg) => config.server(cfg, env),
                }),
            ),
    );

// --- [EXPORTS] -------------------------------------------------------------------------

// The root anchor drops its build block: the root emits no artifact, and the destructure proves the createConfig rail end to end on every config load.
const { build: _build, ...anchor } = Effect.runSync(
    createConfig({
        entry: './vite.config.ts',
        mode: 'library',
        name: 'WorkspaceFoundation',
    }),
);
const rootConfig: UserConfig = defineConfig(anchor);

export { createConfig };
export default rootConfig;

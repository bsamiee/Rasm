/// <reference types="vite/client" />
/**
 * createViteConfig validates shared app, library, and server options and returns the corresponding
 * Vite configuration. The root default omits build output; app and package configs own their output paths.
 */

// --- [IMPORTS] -------------------------------------------------------------------------

import { dirname, resolve as pathResolve } from 'node:path';
import { fileURLToPath } from 'node:url';
import babel from '@rolldown/plugin-babel';
import tailwindcss from '@tailwindcss/vite';
import react, { reactCompilerPreset } from '@vitejs/plugin-react';
import { DateTime, Effect, Match, Option, pipe, Schema as S } from 'effect';
import { visualizer } from 'rollup-plugin-visualizer';
import {
    defineConfig,
    type Plugin,
    type UserConfig,
    type ViteBuilder,
} from 'vite';
import { compression } from 'vite-plugin-compression2';
import csp from 'vite-plugin-csp-guard';
import { ViteImageOptimizer } from 'vite-plugin-image-optimizer';
import Inspect from 'vite-plugin-inspect';
import { VitePWA } from 'vite-plugin-pwa';
import svgr from 'vite-plugin-svgr';
import webfontDownload from 'vite-plugin-webfont-dl';

// --- [TYPES] ---------------------------------------------------------------------------

type EnvironmentConsumer = {
    readonly config: { readonly consumer: 'client' | 'server' };
};
type BuildEnvironment = NodeJS.ProcessEnv & {
    readonly NODE_ENV?: string;
    readonly VITE_API_URL?: string;
    readonly npm_package_version?: string;
};
type ViteConfig = S.Schema.Type<typeof ViteConfigSchema>;
type BuildMode = ViteConfig['mode'];

// --- [MODELS] --------------------------------------------------------------------------

const ViteConfigSchema = S.Union(
    S.Struct({
        assetExtensions: S.optional(S.Array(S.String)),
        builder: S.optional(
            S.Struct({
                sharedConfigBuild: S.optional(S.Boolean),
                sharedPlugins: S.optional(S.Boolean),
            }),
        ),
        compressionThreshold: S.optional(pipe(S.Number, S.int(), S.positive())),
        cspPolicy: S.optional(
            S.Record({ key: S.String, value: S.Array(S.String) }),
        ),
        imageQuality: S.optional(
            S.Struct({
                avif: S.Number,
                jpeg: S.Number,
                png: S.Number,
                webp: S.Number,
            }),
        ),
        mode: S.Literal('app'),
        name: S.String,
        port: S.optional(pipe(S.Number, S.int(), S.between(1024, 65535))),
        pwa: S.optional(
            S.Struct({
                description: S.String,
                name: S.String,
                shortName: S.String,
                themeColor: S.String,
            }),
        ),
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
        // Server builds keep bare specifiers external; bundle includes an explicitly named dependency and its subpaths
        bundle: S.optional(S.Array(S.String)),
        entry: S.String,
        mode: S.Literal('server'),
        name: S.String,
    }),
);

// --- [CONSTANTS] -----------------------------------------------------------------------

const ROOT_DIRECTORY = dirname(fileURLToPath(import.meta.url));
const environment: BuildEnvironment = process.env;
const defaults = Object.freeze({
    assets: ['bin', 'exr', 'fbx', 'glb', 'gltf', 'hdr', 'mtl', 'obj', 'wasm'],
    builder: { sharedConfigBuild: true, sharedPlugins: true },
    cache: { api: 300, cdn: 604800, max: 50 },
    compression: {
        include: /\.(js|mjs|json|css|html|svg)$/i,
        threshold: 10240,
    },
    csp: {
        'connect-src': ["'self'", 'https:', 'wss:', 'ws:'],
        'default-src': ["'self'"],
        'font-src': ["'self'", 'https://fonts.gstatic.com', 'data:'],
        'img-src': ["'self'", 'data:', 'blob:', 'https:'],
        'script-src': ["'self'"],
        'style-src': ["'self'", "'unsafe-inline'"],
        'worker-src': ["'self'", 'blob:'],
    },
    extensions: ['.mjs', '.js', '.mts', '.ts', '.jsx', '.tsx', '.json'],
    assetGlob: '**/*.{js,css,html,ico,png,svg,wasm,glb,gltf}',
    imageQuality: { avif: 70, jpeg: 75, png: 80, webp: 80 },
    port: 3000,
    pwa: {
        backgroundColor: '#ffffff',
    },
    // Rolldown evaluates the specific dependency groups before the catch-all vendor group
    codeSplitting: {
        groups: [
            {
                name: 'vendor-react',
                priority: 3,
                test: /\/node_modules\/(?:\.pnpm\/[^/]+\/node_modules\/)?react(?:-dom)?\//,
            },
            {
                name: 'vendor-effect',
                priority: 2,
                test: /\/node_modules\/(?:\.pnpm\/[^/]+\/node_modules\/)?(?:@effect|effect)\//,
            },
            { name: 'vendor', priority: 1, test: /node_modules/ },
        ],
        minSize: 10240,
    },
    ssr: {
        external: [
            'react',
            'react-dom',
            'react/jsx-runtime',
            'react/compiler-runtime',
        ],
        noExternal: [
            '@effect/platform',
            '@effect/platform-browser',
            '@effect/experimental',
        ],
    },
    svgr: {
        exportType: 'default',
        memo: true,
        ref: true,
        svgo: true,
        titleProp: true,
        typescript: true,
    },
    treeshake: {
        moduleSideEffects: 'no-external' as const,
        propertyReadSideEffects: false as const,
        unknownGlobalSideEffects: false,
    },
    visualizer: {
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

const cssOptions = (development = false) => ({
    devSourcemap: development,
    transformer: 'lightningcss' as const,
});
const runtimeCache = <Handler extends 'CacheFirst' | 'NetworkFirst'>(
    handler: Handler,
    name: string,
    maxAgeSeconds: number,
    urlPattern: RegExp,
) => ({
    handler,
    options: {
        cacheName: name,
        expiration: { maxAgeSeconds, maxEntries: defaults.cache.max },
    },
    urlPattern,
});
const outputNames = (prefix = '') => ({
    assetFileNames: `${prefix}assets/[name]-[hash][extname]`,
    chunkFileNames: `${prefix}chunks/[name]-[hash].js`,
    entryFileNames: `${prefix}${prefix ? '' : 'entries/'}[name]-[hash].js`,
});
const resolveOptions = (browser = false) => ({
    conditions: browser
        ? ['import', 'module', 'browser', 'default']
        : ['import', 'module', 'default'],
    ...(browser
        ? {
              dedupe: ['react', 'react-dom'],
              extensions: [...defaults.extensions],
          }
        : {}),
    tsconfigPaths: true,
});
const clientPlugin = (plugin: Plugin): Plugin => ({
    ...plugin,
    applyToEnvironment: (environment: EnvironmentConsumer) =>
        environment.config.consumer === 'client',
});

// --- [COMPOSITION] ---------------------------------------------------------------------

const buildClient = ({
    build,
    environments: { client },
}: ViteBuilder): Promise<void> =>
    pipe(
        Option.fromNullable(client),
        Option.match({
            onNone: () => Promise.resolve(),
            onSome: (environment) => build(environment).then(() => undefined),
        }),
    );
const pluginSets = {
    app: (
        config: Extract<ViteConfig, { mode: 'app' }>,
        production: boolean,
    ) => [
        react(),
        babel({ presets: [reactCompilerPreset()] }),
        tailwindcss({ optimize: { minify: true } }),
        ...(config.pwa
            ? VitePWA({
                  devOptions: { enabled: false },
                  includeAssets: (
                      config.assetExtensions ?? defaults.assets
                  ).map((extension) => `**/*.${extension}`),
                  manifest: {
                      background_color: defaults.pwa.backgroundColor,
                      description: config.pwa.description,
                      display: 'standalone' as const,
                      icons: [
                          ...[192, 512].map((size) => ({
                              purpose: 'any' as const,
                              sizes: `${size}x${size}`,
                              src: `/icon-${size}.png`,
                              type: 'image/png',
                          })),
                          {
                              purpose: 'maskable' as const,
                              sizes: '512x512',
                              src: '/icon-maskable.png',
                              type: 'image/png',
                          },
                      ],
                      name: config.pwa.name,
                      scope: '/',
                      short_name: config.pwa.shortName,
                      start_url: '/',
                      theme_color: config.pwa.themeColor,
                  },
                  registerType: 'autoUpdate',
                  workbox: {
                      clientsClaim: true,
                      globPatterns: [defaults.assetGlob],
                      runtimeCaching: [
                          runtimeCache(
                              'CacheFirst',
                              'cdn-cache',
                              defaults.cache.cdn,
                              /^https:\/\/cdn\./,
                          ),
                          runtimeCache(
                              'NetworkFirst',
                              'api-cache',
                              defaults.cache.api,
                              /^https:\/\/api\./,
                          ),
                      ],
                      skipWaiting: true,
                  },
              }).map((plugin) => clientPlugin(plugin))
            : []),
        svgr({
            exclude: '',
            include: '**/*.svg?react',
            svgrOptions: defaults.svgr,
        }),
        clientPlugin(
            ViteImageOptimizer({
                avif: {
                    lossless: false,
                    quality: (config.imageQuality ?? defaults.imageQuality)
                        .avif,
                },
                exclude: /^(?:virtual:|node_modules)/,
                includePublic: true,
                jpeg: {
                    progressive: true,
                    quality: (config.imageQuality ?? defaults.imageQuality)
                        .jpeg,
                },
                logStats: true,
                png: {
                    quality: (config.imageQuality ?? defaults.imageQuality).png,
                },
                test: /\.(jpe?g|png|gif|tiff|webp|svg|avif)$/i,
                webp: {
                    lossless: false,
                    quality: (config.imageQuality ?? defaults.imageQuality)
                        .webp,
                },
            }),
        ),
        clientPlugin(webfontDownload([...(config.webfonts ?? [])])),
        clientPlugin({
            ...visualizer({
                ...defaults.visualizer,
                exclude: [...defaults.visualizer.exclude],
                projectRoot: ROOT_DIRECTORY,
            }),
            apply: 'build',
        } as unknown as Plugin),
        ...(production
            ? [
                  clientPlugin(
                      compression({
                          algorithms: ['brotliCompress', 'gzip'],
                          include: defaults.compression.include,
                          skipIfLargerOrEqual: true,
                          threshold:
                              config.compressionThreshold ??
                              defaults.compression.threshold,
                      }),
                  ),
              ]
            : []),
        clientPlugin(
            csp({
                algorithm: 'sha256',
                build: { sri: true },
                policy: Object.fromEntries(
                    Object.entries(config.cspPolicy ?? defaults.csp).map(
                        ([directive, values]) => [directive, [...values]],
                    ),
                ),
            }),
        ),
        // Inspect is disabled during development because HMR restarts can produce an EEXIST race
        ...(production
            ? [
                  Inspect({
                      build: true,
                      dev: false,
                      outputDir: pathResolve(
                          config.root ?? ROOT_DIRECTORY,
                          '.cache/vite-inspect',
                      ),
                  }),
              ]
            : []),
    ],
    library: (config: Extract<ViteConfig, { mode: 'library' }>) => [
        ...(config.react === true
            ? [react(), babel({ presets: [reactCompilerPreset()] })]
            : []),
        ...(config.css === undefined
            ? []
            : [tailwindcss({ optimize: { minify: true } })]),
    ],
    server: () => [],
} as const;
const configByMode: {
    readonly [Mode in BuildMode]: (
        config: Extract<ViteConfig, { mode: Mode }>,
        build: { production: boolean; time: string; version: string },
    ) => UserConfig;
} = {
    app: (config, { production, time, version }) => ({
        appType: 'spa',
        assetsInclude: (config.assetExtensions ?? defaults.assets).map(
            (extension) => `**/*.${extension}`,
        ),
        ...(config.root ? { root: config.root } : {}),
        build: {
            chunkImportMap: true,
            emptyOutDir: true,
            manifest: true,
            reportCompressedSize: false,
            rolldownOptions: {
                output: {
                    ...outputNames(),
                    codeSplitting: {
                        groups: [...defaults.codeSplitting.groups],
                        minSize: defaults.codeSplitting.minSize,
                    },
                },
                treeshake: defaults.treeshake,
            },
            sourcemap: true,
        },
        builder: {
            buildApp: buildClient,
            sharedConfigBuild:
                config.builder?.sharedConfigBuild ??
                defaults.builder.sharedConfigBuild,
            sharedPlugins:
                config.builder?.sharedPlugins ?? defaults.builder.sharedPlugins,
        },
        cacheDir: config.root
            ? pathResolve(config.root, 'node_modules/.vite')
            : pathResolve(ROOT_DIRECTORY, '.cache/vite'),
        css: cssOptions(true),
        define: {
            'import.meta.env.APP_VERSION': JSON.stringify(version),
            'import.meta.env.BUILD_MODE': JSON.stringify(
                production ? 'production' : 'development',
            ),
            'import.meta.env.BUILD_TIME': JSON.stringify(time),
            'import.meta.env.VITE_API_URL': JSON.stringify(
                environment.VITE_API_URL ?? '/api',
            ),
        },
        devtools: true,
        future: 'warn',
        optimizeDeps: {
            exclude: [...defaults.ssr.noExternal],
            include: [
                ...defaults.ssr.external,
                'react-aria-components',
                '@floating-ui/react',
                'effect',
            ],
        },
        plugins: pluginSets.app(config, production),
        // strictPort keeps preview on the URL expected by end-to-end test server configuration
        preview: { port: config.port ?? defaults.port, strictPort: true },
        resolve: resolveOptions(true),
        server: {
            cors: true,
            port: config.port ?? defaults.port,
            ...(config.warmup === undefined
                ? {}
                : { warmup: { clientFiles: [...config.warmup] } }),
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
            external: [...defaults.ssr.external],
            noExternal: [...defaults.ssr.noExternal],
            optimizeDeps: { include: ['@effect/platform'] },
            resolve: {
                conditions: ['node', 'import', 'module', 'default'],
                externalConditions: ['node'],
            },
            target: 'node',
        },
        worker: {
            format: 'es',
            plugins: () => [
                react(),
                babel({ presets: [reactCompilerPreset()] }),
            ],
            rolldownOptions: { output: outputNames('workers/') },
        },
    }),
    library: (config) => ({
        build: {
            lib: {
                entry: config.entry,
                fileName: (format: string, name: string) =>
                    format === 'es' ? `${name}.js` : `${name}.${format}.js`,
                formats: ['es', 'cjs'],
                name: config.name,
            },
            rolldownOptions: {
                external: [/^node:/, ...(config.external ?? [])],
                output: { exports: 'named' },
            },
            sourcemap: true,
            target: 'esnext',
        },
        css: cssOptions(config.css !== undefined),
        future: 'warn',
        plugins: pluginSets.library(config),
        resolve: resolveOptions(config.react === true),
        ssr: { target: 'node' as const },
    }),
    server: (config) => ({
        build: {
            lib: {
                entry: config.entry,
                fileName: 'main',
                formats: ['es'] as const,
                name: config.name,
            },
            rolldownOptions: {
                // Bare specifiers resolve from node_modules at run time; bundle includes each named dependency and its subpaths
                external: (id: string) =>
                    !id.startsWith('.') &&
                    !id.startsWith('/') &&
                    !(config.bundle ?? []).some(
                        (dependency) =>
                            id === dependency ||
                            id.startsWith(`${dependency}/`),
                    ),
                output: { exports: 'named' as const },
            },
            sourcemap: true,
            target: 'node24',
        },
        future: 'warn',
        plugins: pluginSets.server(),
        resolve: resolveOptions(),
        ssr: { target: 'node' as const },
    }),
};
const createViteConfig = (
    input: unknown,
): Effect.Effect<UserConfig, never, never> =>
    Effect.map(
        Effect.all({
            config: Effect.orDie(S.decodeUnknown(ViteConfigSchema)(input)),
            build: Effect.map(DateTime.now, (now) => ({
                production: environment.NODE_ENV === 'production',
                time: DateTime.formatIso(now),
                version: environment.npm_package_version ?? '0.0.0',
            })),
        }),
        ({ config, build }) =>
            Match.value(config).pipe(
                Match.discriminatorsExhaustive('mode')({
                    app: (appConfig) => configByMode.app(appConfig, build),
                    library: (libraryConfig) =>
                        configByMode.library(libraryConfig, build),
                    server: (serverConfig) =>
                        configByMode.server(serverConfig, build),
                }),
            ),
    );

// --- [EXPORTS] -------------------------------------------------------------------------

// Root configuration omits the build block because only app and package configs emit output
const { build: _build, ...rootOptions } = Effect.runSync(
    createViteConfig({
        entry: './vite.config.ts',
        mode: 'library',
        name: 'RasmWorkspace',
    }),
);
const rootConfig: UserConfig = defineConfig(rootOptions);

export { createViteConfig };
export default rootConfig;

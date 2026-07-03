# [API_CATALOGUE] workbox-build

`workbox-build` is the Node build-time API generating and configuring Workbox service workers. `generateSW` writes a complete SW from glob patterns, `injectManifest` injects a precache manifest into a hand-authored SW, and `getManifest` resolves the manifest without writing. `copyWorkboxLibraries`/`getModuleURL` source the runtime libraries locally or from CDN. The option surface composes from shared partials — `BasePartial` (manifest shaping), `GlobPartial` (precache selection), `GeneratePartial` (the full runtime SW behavior: `runtimeCaching`/`navigationPreload`/`navigateFallback`/`clientsClaim`/`skipWaiting`), `InjectPartial` (`injectionPoint`/`swSrc`) — into the three public options aliases. In this pipeline `vite-plugin-pwa` consumes `Partial<GenerateSWOptions>` via its `workbox` field, and `Shell/serviceworker`'s Effect `ServiceWorkerHost` imports `RuntimeCaching`/`StrategyName` type-only for the strategy projection.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `workbox-build`
- package: `workbox-build`
- module: `workbox-build` (`build/index.d.ts`), re-exports `build/types`
- asset: Node build-time service worker generator
- rail: service-worker-build

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: options aliases and result shapes
- rail: service-worker-build

| [INDEX] | [SYMBOL]                  | [TYPE_FAMILY] | [DESCRIPTION]                                                          |
| :-----: | :------------------------ | :------------ | :--------------------------------------------------------------------- |
|  [01]   | `GenerateSWOptions`       | options alias | `Base & Glob & Generate & RequiredSWDest & OptionalGlobDirectory`     |
|  [02]   | `InjectManifestOptions`   | options alias | `Base & Glob & Inject & RequiredSWDest & RequiredGlobDirectory`       |
|  [03]   | `GetManifestOptions`      | options alias | `Base & Glob & RequiredGlobDirectory`                                 |
|  [04]   | `GetManifestResult`       | interface     | `{ count, manifestEntries, size, warnings }`                          |
|  [05]   | `BuildResult`             | type alias    | `Omit<GetManifestResult,'manifestEntries'> & { filePaths }`          |
|  [06]   | `ManifestEntry`           | interface     | `{ integrity?, revision: string\|null, url }`                        |
|  [07]   | `StrategyName`            | string union  | `'CacheFirst'\|'CacheOnly'\|'NetworkFirst'\|'NetworkOnly'\|'StaleWhileRevalidate'` |
|  [08]   | `RuntimeCaching`          | interface     | `{ handler, method?, urlPattern, options? }`; one runtime route       |
|  [09]   | `ManifestTransform`       | function type | `(entries: Array<ManifestEntry & {size}>, compilation?: unknown) => ManifestTransformResult \| Promise<…>` |
|  [10]   | `ManifestTransformResult` | interface     | `{ manifest: Array<ManifestEntry & {size}>, warnings? }`             |
|  [11]   | `BuildType`               | string union  | `'dev' \| 'prod'`; the `getModuleURL` build-type argument            |

[PUBLIC_TYPE_SCOPE]: option partials
- rail: service-worker-build

| [INDEX] | [SYMBOL]                       | [TYPE_FAMILY] | [DESCRIPTION]                                                         |
| :-----: | :----------------------------- | :------------ | :-------------------------------------------------------------------- |
|  [01]   | `BasePartial`                  | interface     | `additionalManifestEntries`, `dontCacheBustURLsMatching`, `manifestTransforms`, `maximumFileSizeToCacheInBytes` (default `2097152`), `modifyURLPrefix` |
|  [02]   | `GlobPartial`                  | interface     | `globPatterns` (default `["**/*.{js,wasm,css,html}"]`), `globIgnores` (default `["**/node_modules/**/*"]`), `globFollow`, `templatedURLs` |
|  [03]   | `GeneratePartial`              | interface     | the full runtime SW behavior surface (field table below)             |
|  [04]   | `InjectPartial`                | interface     | `injectionPoint` (default `self.__WB_MANIFEST`), required `swSrc`     |
|  [05]   | `RequiredSWDestPartial`        | interface     | required `swDest` (must end `.js`)                                    |
|  [06]   | `RequiredGlobDirectoryPartial` | interface     | required `globDirectory`                                              |
|  [07]   | `OptionalGlobDirectoryPartial` | interface     | optional `globDirectory`                                             |
|  [08]   | `WebpackPartial`               | interface     | `chunks`, `excludeChunks`, `include`/`exclude`, `mode` — webpack-only |
|  [09]   | `WebpackGenerateSWPartial`     | interface     | `importScriptsViaChunks`, `swDest` (webpack `generateSW` extras)      |
|  [10]   | `WebpackInjectManifestPartial` | interface     | `compileSrc`, `swDest`, `webpackCompilationPlugins` (webpack inject)  |
|  [11]   | `WebpackGenerateSWOptions`     | options alias | `Base & Webpack & Generate & WebpackGenerateSW` — inert in Vite       |
|  [12]   | `WebpackInjectManifestOptions` | options alias | `Base & Webpack & Inject & WebpackInjectManifest` — inert in Vite     |

[PUBLIC_TYPE_SCOPE]: `GeneratePartial` runtime SW behavior
- rail: service-worker-build

| [INDEX] | [FIELD]                       | [TYPE]                                  | [DEFAULT]        |
| :-----: | :---------------------------- | :-------------------------------------- | :--------------- |
|  [01]   | `runtimeCaching`              | `Array<RuntimeCaching>`                 | —                |
|  [02]   | `navigationPreload`           | `boolean`                               | `false`          |
|  [03]   | `navigateFallback`            | `string \| null`                        | `null`           |
|  [04]   | `navigateFallbackAllowlist`   | `Array<RegExp>`                         | —                |
|  [05]   | `navigateFallbackDenylist`    | `Array<RegExp>`                         | —                |
|  [06]   | `clientsClaim`                | `boolean`                               | `false`          |
|  [07]   | `skipWaiting`                 | `boolean`                               | `false`          |
|  [08]   | `cleanupOutdatedCaches`       | `boolean`                               | `false`          |
|  [09]   | `offlineGoogleAnalytics`      | `boolean \| GoogleAnalyticsInitializeOptions` | `false`    |
|  [10]   | `importScripts`               | `Array<string>`                         | —                |
|  [11]   | `inlineWorkboxRuntime`        | `boolean`                               | `false`          |
|  [12]   | `cacheId`                     | `string \| null`                        | —                |
|  [13]   | `directoryIndex`              | `string \| null`                        | —                |
|  [14]   | `ignoreURLParametersMatching` | `Array<RegExp>`                         | `[/^utm_/, /^fbclid$/]` |
|  [15]   | `babelPresetEnvTargets`       | `Array<string>`                         | `["chrome >= 56"]` |
|  [16]   | `mode`                        | `string \| null`                        | `"production"`   |
|  [17]   | `sourcemap`                   | `boolean`                               | `true`           |
|  [18]   | `disableDevLogs`              | `boolean`                               | `false`          |

[PUBLIC_TYPE_SCOPE]: `RuntimeCaching.options` runtime-cache plugins
- rail: service-worker-build

| [INDEX] | [FIELD]                 | [TYPE]                                             | [OWNING PACKAGE]                |
| :-----: | :---------------------- | :------------------------------------------------ | :------------------------------ |
|  [01]   | `cacheName`             | `string \| null`                                  | workbox-strategies              |
|  [02]   | `expiration`            | `ExpirationPluginOptions` (`maxEntries`/`maxAgeSeconds`) | workbox-expiration        |
|  [03]   | `cacheableResponse`     | `CacheableResponseOptions`                        | workbox-cacheable-response      |
|  [04]   | `networkTimeoutSeconds` | `number` (NetworkFirst/NetworkOnly only)          | workbox-strategies              |
|  [05]   | `backgroundSync`        | `{ name; options?: QueueOptions }`                | workbox-background-sync         |
|  [06]   | `broadcastUpdate`       | `{ channelName?; options: BroadcastCacheUpdateOptions }` | workbox-broadcast-update  |
|  [07]   | `precacheFallback`      | `{ fallbackURL: string }`                         | workbox-precaching              |
|  [08]   | `rangeRequests`         | `boolean`                                         | workbox-range-requests          |
|  [09]   | `plugins`               | `Array<WorkboxPlugin>`                            | workbox-core                    |
|  [10]   | `fetchOptions` / `matchOptions` | `RequestInit` / `CacheQueryOptions`       | native fetch/CacheStorage       |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: build functions
- rail: service-worker-build

| [INDEX] | [SURFACE]                       | [ENTRY_FAMILY]  | [DESCRIPTION]                                                     |
| :-----: | :------------------------------ | :-------------- | :--------------------------------------------------------------- |
|  [01]   | `generateSW(config)`            | sw generator    | writes a ready-to-use SW to `swDest`; returns `Promise<BuildResult>` |
|  [02]   | `injectManifest(config)`        | manifest inject | replaces `injectionPoint` in `swSrc`, writes `swDest`; returns `Promise<BuildResult>` |
|  [03]   | `getManifest(config)`           | manifest read   | resolves the manifest + audit counts without writing; `Promise<GetManifestResult>` |
|  [04]   | `copyWorkboxLibraries(destDir)` | library copy    | copies runtime libraries beside the SW; resolves the new dir name |
|  [05]   | `getModuleURL(name, buildType)` | CDN URL         | resolves a Workbox module URL for `'dev'`/`'prod'` (`BuildType`)  |

```ts contract
// workbox-build (build/index.d.ts, re-exporting build/types)
export declare function generateSW(config: GenerateSWOptions): Promise<BuildResult>
export declare function injectManifest(config: InjectManifestOptions): Promise<BuildResult>
export declare function getManifest(config: GetManifestOptions): Promise<GetManifestResult>
export declare function copyWorkboxLibraries(destDirectory: string): Promise<string>
export declare function getModuleURL(moduleName: string, buildType: BuildType): string

interface RuntimeCaching {
  handler: RouteHandler | StrategyName
  method?: HTTPMethod                       // default 'GET'
  urlPattern: RegExp | string | RouteMatchCallback   // maps to registerRoute's first arg
  options?: {
    cacheName?: string | null
    expiration?: ExpirationPluginOptions    // maxEntries, maxAgeSeconds
    cacheableResponse?: CacheableResponseOptions
    networkTimeoutSeconds?: number
    backgroundSync?: { name: string; options?: QueueOptions }
    broadcastUpdate?: { channelName?: string; options: BroadcastCacheUpdateOptions }
    precacheFallback?: { fallbackURL: string }
    rangeRequests?: boolean
    plugins?: Array<WorkboxPlugin>
    fetchOptions?: RequestInit
    matchOptions?: CacheQueryOptions
  }
}

interface GetManifestResult { count: number; manifestEntries: ManifestEntry[]; size: number; warnings: string[] }
type BuildResult = Omit<GetManifestResult, 'manifestEntries'> & { filePaths: string[] }
interface ManifestEntry { integrity?: string; revision: string | null; url: string }
type StrategyName = 'CacheFirst' | 'CacheOnly' | 'NetworkFirst' | 'NetworkOnly' | 'StaleWhileRevalidate'
```

## [04]-[IMPLEMENTATION_LAW]

[BUILD_TOPOLOGY]:
- `generateSW` requires `swDest` and resolves the precache manifest from `globDirectory` + `globPatterns`; runtime routing is declared in `runtimeCaching`, offline app-shell in `navigateFallback`, and HTML preload race in `navigationPreload`.
- `injectManifest` reads `swSrc`, replaces `injectionPoint` (default `self.__WB_MANIFEST`) with the manifest, and writes `swDest`; it does not compile or bundle `swSrc`.
- `getManifest` returns the resolved manifest and audit counts without writing a file — the pre-flight for a precache-size budget check.
- `ManifestEntry.revision` is `null` when the URL is assumed uniquely versioned (matched by `dontCacheBustURLsMatching`); `manifestTransforms` run sequentially after `modifyURLPrefix` and `dontCacheBustURLsMatching`, each filtering, renaming, or re-revisioning entries.
- `maximumFileSizeToCacheInBytes` defaults to `2097152`, `globPatterns` to `["**/*.{js,wasm,css,html}"]`, `globIgnores` to `["**/node_modules/**/*"]`.

[RUNTIME_CACHING]:
- `RuntimeCaching.handler` is a `StrategyName` string (mapped to a `workbox-strategies` strategy) or a `RouteHandler` callback; `urlPattern` maps to the first argument of `registerRoute` as a `RegExp`, `string`, or `RouteMatchCallback`, and the first matching route wins.
- Each `options` shortcut mounts a Workbox plugin: `expiration` → `ExpirationPlugin` (`maxEntries`/`maxAgeSeconds`), `cacheableResponse` → `CacheableResponsePlugin`, `backgroundSync` → `BackgroundSyncPlugin`, `broadcastUpdate` → `BroadcastUpdatePlugin`, `precacheFallback` → `PrecacheFallbackPlugin`, `rangeRequests` → `RangeRequestsPlugin`; `plugins` admits any `WorkboxPlugin` without a shortcut.
- `networkTimeoutSeconds` applies only to `NetworkFirst`/`NetworkOnly`; `navigationPreload: true` requires a `runtimeCaching` route matching navigation requests to consume the preloaded response.

[LOCAL_ADMISSION]:
- `vite-plugin-pwa` consumes `Partial<GenerateSWOptions>` directly via its `workbox` field, so `Shell/build` never calls `generateSW` itself — direct `workbox-build` use is reserved for SWs generated outside the Vite pipeline.
- `Shell/serviceworker`'s `ServiceWorkerHost` (an `Effect.Service`) imports `RuntimeCaching`/`StrategyName` type-only and projects each `StrategyBehavior` row (`urlPattern`, `cacheName`, `expiration.maxEntries`/`maxAgeSeconds`) into one `RuntimeCaching` entry — the `runtimeCachingManifest` array `vite-plugin-pwa` folds into the emitted worker.
- The offline-first story is `navigationPreload: true` (HTML network-first preload race) plus `navigateFallback` (the precached app-shell `NavigationRoute`) resolved from the precache manifest; `workbox-window` is the browser-side companion registering and driving the lifecycle of the emitted worker.
- Prefer `injectManifest` when the SW carries bespoke push/sync/messaging logic; the `Webpack*` aliases are the webpack plugin surface and are inert in the Vite pipeline.

[RAIL_LAW]:
- Package: `workbox-build`
- Owns: build-time SW generation, manifest injection, and precache-manifest resolution
- Accept: `generateSW`/`injectManifest`/`getManifest` with the shared option partials; the type-only `RuntimeCaching`/`StrategyName` projection consumed by the Effect service
- Reject: hand-rolled precache-manifest assembly; manual SW templating that duplicates `generateSW`; a runtime-cache route authored outside the `StrategyBehavior` → `RuntimeCaching` projection

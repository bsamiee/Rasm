# [API_CATALOGUE] workbox-build

`workbox-build` supplies the Node build-time API for generating and configuring Workbox service workers. `generateSW` writes a complete service worker from glob patterns, `injectManifest` injects a precache manifest into a hand-authored service worker, and `getManifest` resolves the manifest without writing files. `copyWorkboxLibraries` and `getModuleURL` support self-hosted or CDN-sourced runtime libraries. The option surface composes from shared partials (`BasePartial`, `GlobPartial`, `GeneratePartial`, `InjectPartial`) into the three public options aliases.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `workbox-build`
- package: `workbox-build`
- module: `workbox-build` (`build/index.d.ts`), re-exports `build/types`
- asset: Node build-time service worker generator
- rail: service-worker-build

## [2]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: options aliases and result shapes
- rail: service-worker-build

| [INDEX] | [SYMBOL]                  | [TYPE_FAMILY] | [DESCRIPTION]                                                     |
| :-----: | :------------------------ | :------------ | :---------------------------------------------------------------- |
|   [1]   | `GenerateSWOptions`       | options alias | `Base & Glob & Generate & RequiredSWDest & OptionalGlobDirectory` |
|   [2]   | `InjectManifestOptions`   | options alias | `Base & Glob & Inject & RequiredSWDest & RequiredGlobDirectory`   |
|   [3]   | `GetManifestOptions`      | options alias | `Base & Glob & RequiredGlobDirectory`                             |
|   [4]   | `GetManifestResult`       | interface     | `{ count, manifestEntries, size, warnings }`                      |
|   [5]   | `BuildResult`             | type alias    | `Omit<GetManifestResult,'manifestEntries'> & { filePaths }`       |
|   [6]   | `ManifestEntry`           | interface     | `{ integrity?, revision: string\|null, url }`                     |
|   [7]   | `StrategyName`            | string union  | five built-in caching strategy names                              |
|   [8]   | `RuntimeCaching`          | interface     | `{ handler, method?, urlPattern, options? }`                      |
|   [9]   | `ManifestTransform`       | function type | `(entries, compilation?) => ManifestTransformResult \| Promise`   |
|  [10]   | `ManifestTransformResult` | interface     | `{ manifest, warnings? }`                                         |

[PUBLIC_TYPE_SCOPE]: option partials
- rail: service-worker-build

| [INDEX] | [SYMBOL]                       | [TYPE_FAMILY] | [DESCRIPTION]                                       |
| :-----: | :----------------------------- | :------------ | :-------------------------------------------------- |
|   [1]   | `BasePartial`                  | interface     | manifest entries, cache-bust, transforms, size cap  |
|   [2]   | `GlobPartial`                  | interface     | glob follow/ignore/patterns, templated URLs         |
|   [3]   | `GeneratePartial`              | interface     | SW behavior: caching, fallback, claim, skip-waiting |
|   [4]   | `InjectPartial`                | interface     | `injectionPoint`, `swSrc`                           |
|   [5]   | `RequiredSWDestPartial`        | interface     | required `swDest`                                   |
|   [6]   | `RequiredGlobDirectoryPartial` | interface     | required `globDirectory`                            |
|   [7]   | `OptionalGlobDirectoryPartial` | interface     | optional `globDirectory`                            |
|   [8]   | `WebpackPartial`               | interface     | chunks, include/exclude for webpack pipeline        |
|   [9]   | `WebpackGenerateSWOptions`     | options alias | webpack `generateSW` variant                        |
|  [10]   | `WebpackInjectManifestOptions` | options alias | webpack `injectManifest` variant                    |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: build functions
- rail: service-worker-build

| [INDEX] | [SURFACE]                       | [ENTRY_FAMILY]  | [DESCRIPTION]                                         |
| :-----: | :------------------------------ | :-------------- | :---------------------------------------------------- |
|   [1]   | `generateSW(config)`            | sw generator    | writes a ready-to-use SW to `swDest`                  |
|   [2]   | `injectManifest(config)`        | manifest inject | replaces `injectionPoint` in `swSrc`, writes `swDest` |
|   [3]   | `getManifest(config)`           | manifest read   | resolves manifest without writing files               |
|   [4]   | `copyWorkboxLibraries(destDir)` | library copy    | copies runtime libraries beside the SW                |
|   [5]   | `getModuleURL(name, buildType)` | CDN URL         | resolves a Workbox module URL for `dev`/`prod`        |

```ts contract
// workbox-build
export declare function generateSW(config: GenerateSWOptions): Promise<BuildResult>
export declare function injectManifest(config: InjectManifestOptions): Promise<BuildResult>
export declare function getManifest(config: GetManifestOptions): Promise<GetManifestResult>
export declare function copyWorkboxLibraries(destDirectory: string): Promise<string>
export declare function getModuleURL(moduleName: string, buildType: 'dev' | 'prod'): string

interface RuntimeCaching {
  handler: RouteHandler | StrategyName
  method?: HTTPMethod                       // default 'GET'
  urlPattern: RegExp | string | RouteMatchCallback
  options?: {
    backgroundSync?: { name: string; options?: QueueOptions }
    broadcastUpdate?: { channelName?: string; options: BroadcastCacheUpdateOptions }
    cacheableResponse?: CacheableResponseOptions
    cacheName?: string | null
    expiration?: ExpirationPluginOptions
    networkTimeoutSeconds?: number
    plugins?: Array<WorkboxPlugin>
    precacheFallback?: { fallbackURL: string }
    rangeRequests?: boolean
    fetchOptions?: RequestInit
    matchOptions?: CacheQueryOptions
  }
}

interface GetManifestResult { count: number; manifestEntries: ManifestEntry[]; size: number; warnings: string[] }
type BuildResult = Omit<GetManifestResult, 'manifestEntries'> & { filePaths: string[] }
interface ManifestEntry { integrity?: string; revision: string | null; url: string }
type StrategyName = 'CacheFirst' | 'CacheOnly' | 'NetworkFirst' | 'NetworkOnly' | 'StaleWhileRevalidate'
```

## [4]-[IMPLEMENTATION_LAW]

[BUILD_TOPOLOGY]:
- `generateSW` requires `swDest` and resolves the precache manifest from `globDirectory` plus `globPatterns`; runtime routing is declared in `runtimeCaching`.
- `injectManifest` reads `swSrc`, replaces `injectionPoint` (default `self.__WB_MANIFEST`) with the manifest, and writes `swDest`; it does not compile or bundle `swSrc`.
- `getManifest` returns the resolved manifest and audit counts without writing any file.
- `ManifestEntry.revision` is `null` when the URL is assumed uniquely versioned (matched by `dontCacheBustURLsMatching`).
- `maximumFileSizeToCacheInBytes` defaults to `2097152`; `globPatterns` defaults to `["**/*.{js,wasm,css,html}"]`; `globIgnores` defaults to `["**/node_modules/**/*"]`.
- `manifestTransforms` run sequentially after `modifyURLPrefix` and `dontCacheBustURLsMatching`; each transform filters, renames, or re-revisions precache entries.
- `RuntimeCaching.handler` is a `StrategyName` string or a `RouteHandler` callback; `urlPattern` maps to the first argument of `registerRoute` as a `RegExp`, string, or `RouteMatchCallback`.

[LOCAL_ADMISSION]:
- `vite-plugin-pwa` consumes `GenerateSWOptions` directly via its `workbox` option; direct `workbox-build` use is reserved for service workers generated outside the Vite pipeline.
- Prefer `injectManifest` when the service worker carries bespoke push, sync, or messaging logic.
- The `Webpack*` option aliases are for the webpack plugin surface and are inert in the Vite pipeline.

[RAIL_LAW]:
- Package: `workbox-build`
- Owns: build-time service worker generation, manifest injection, and precache manifest resolution
- Accept: `generateSW`/`injectManifest`/`getManifest` with shared option partials
- Reject: hand-rolled precache manifest assembly; manual SW templating that duplicates `generateSW`

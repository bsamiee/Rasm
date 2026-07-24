# [TS_RUNTIME_API_WORKBOX_BUILD]

`workbox-build` runs at app-build in Node: it globs the built assets, hashes each into a precache `ManifestEntry`, and emits the worker — `injectManifest` splices the manifest into an authored SW, `generateSW` writes a complete one, `getManifest` computes entries without writing.

`StrategyName`/`RuntimeCaching` cross into `browser/shell.ts` type-only, so the runtime cache-route rows and the emitted asset share one truth; the value surface never enters the browser bundle.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `workbox-build`
- package: `workbox-build` (MIT)
- module: CommonJS `build/index.js`, types `build/index.d.ts`; the root barrel re-exports the emit functions and the whole `./types` surface
- runtime: Node build-time only — globs the filesystem and writes files, never bundled into the browser app
- rail: `browser/shell` build-time

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the cache-route vocabulary the runtime shares type-only, the precache-entry shapes, and the per-emit option algebras with their receipts

| [INDEX] | [SYMBOL]                  | [TYPE_FAMILY] | [CAPABILITY]                                             |
| :-----: | :------------------------ | :------------ | :------------------------------------------------------- |
|  [01]   | `StrategyName`            | union         | closed cache-strategy vocabulary a handler keys on       |
|  [02]   | `RuntimeCaching`          | interface     | one runtime cache route — handler, pattern, options      |
|  [03]   | `ManifestEntry`           | interface     | one precache entry — url, revision, integrity            |
|  [04]   | `ManifestTransform`       | delegate      | sequential rewrite over the sized manifest array         |
|  [05]   | `ManifestTransformResult` | interface     | transform output — manifest, warnings                    |
|  [06]   | `GenerateSWOptions`       | intersection  | complete-SW emit option algebra                          |
|  [07]   | `InjectManifestOptions`   | intersection  | inject-into-authored-SW option algebra                   |
|  [08]   | `GetManifestOptions`      | intersection  | manifest-only option algebra                             |
|  [09]   | `GetManifestResult`       | interface     | manifest census — count, manifestEntries, size, warnings |
|  [10]   | `BuildResult`             | intersection  | emit receipt — count, size, warnings, filePaths          |
|  [11]   | `BuildType`               | union         | `'dev'\|'prod'` module-URL build selector                |

- `StrategyName`: `"CacheFirst"\|"CacheOnly"\|"NetworkFirst"\|"NetworkOnly"\|"StaleWhileRevalidate"`; `networkTimeoutSeconds` binds only `NetworkFirst`/`NetworkOnly`.
- `RuntimeCaching`: `handler: RouteHandler\|StrategyName`, `urlPattern: RegExp\|string\|RouteMatchCallback`, `method?: HTTPMethod`, `options?: {cacheName, expiration, backgroundSync:{name,options?:QueueOptions}, cacheableResponse, rangeRequests, networkTimeoutSeconds, plugins}`.
- `ManifestEntry.revision: string\|null` — a content-addressed URL sets it `null`, exempting the asset from cache-busting.
- Option algebra: `GenerateSWOptions = BasePartial&GlobPartial&GeneratePartial&RequiredSWDestPartial&OptionalGlobDirectoryPartial`; `InjectManifestOptions` swaps `GeneratePartial`→`InjectPartial` and requires `globDirectory`; `GetManifestOptions = BasePartial&GlobPartial&RequiredGlobDirectoryPartial`, no SW destination.

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: one precache-manifest concern in three output modalities, with the runtime-library copy and CDN-URL helpers

| [INDEX] | [SURFACE]                                                       | [SHAPE] | [CAPABILITY]                                          |
| :-----: | :-------------------------------------------------------------- | :------ | :---------------------------------------------------- |
|  [01]   | `injectManifest(InjectManifestOptions) -> Promise<BuildResult>` | static  | splice the manifest into an authored SW, write swDest |
|  [02]   | `generateSW(GenerateSWOptions) -> Promise<BuildResult>`         | static  | emit a complete SW with runtime routes to swDest      |
|  [03]   | `getManifest(GetManifestOptions) -> Promise<GetManifestResult>` | static  | compute the precache manifest, write no worker        |
|  [04]   | `copyWorkboxLibraries(string) -> Promise<string>`               | static  | copy Workbox runtime libs local, return the dir       |
|  [05]   | `getModuleURL(string, BuildType) -> string`                     | static  | resolve a Workbox module's CDN URL                    |

- `injectManifest`: never compiles or bundles `swSrc`; it replaces the `injectionPoint` placeholder (`self.__WB_MANIFEST` default) with the computed manifest — the Rasm path, over an Effect-authored worker carrying the strategy rows.
- `copyWorkboxLibraries`: an `injectManifest` caller runs it to self-host the runtime rather than the CDN; a `generateSW` caller never needs it.
- `getManifest`: folds the build-time prerendered per-route HTML into the precache without emitting a worker — the client-rendered-plus-prerender SEO posture.

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Each emit function globs `globDirectory` against `globPatterns`, hashes every match into a `ManifestEntry.revision`, and drops the revision to `null` for a URL `dontCacheBustURLsMatching` marks already content-addressed; `maximumFileSizeToCacheInBytes` (2 MiB default) bounds an accidental large match out of the precache, and `manifestTransforms` rewrite the entry array sequentially before emit.
- `injectManifest` replaces the placeholder in `swSrc` and writes `swDest` without compilation; `generateSW` writes a complete worker with `runtimeCaching` translated to route registrations; `getManifest` returns entries and writes nothing.

[STACKING]:
- `workbox-window`(`.api/workbox-window.md`): `RuntimeCaching`/`StrategyName` cross the build/runtime seam type-only, and the asset `injectManifest` emits is the exact script `Workbox(script)` registers through `Workbox.register()` — so the strategy rows are one source of truth, and a `waiting` event drives `Workbox.messageSkipWaiting()` to reload the freshly-emitted worker.
- `effect`(`.api/effect.md`): `Schema.decode` a `RuntimeCaching`/option row at the build boundary so a malformed strategy fails the build, not the runtime; the strategy lookup is a total `Record.map` over the keyed vocabulary rather than a `Match` chain.
- `browser/shell.ts` `Sw`: `Sw.caching(mark) -> ReadonlyArray<RuntimeCaching>` projects the interior `_lanes`/`_STRATEGIES` table — each row's `handler: StrategyName`, `urlPattern`, and `options.expiration`/`backgroundSync`/`rangeRequests`/`networkTimeoutSeconds` — and `Sw.build(spec)` yields the `navigateFallback`/`navigationPreload`/`cacheId`/`cleanupOutdatedCaches`/`clientsClaim` partial the build script's `injectManifest` config spreads; both import `workbox-build` type-only.

[LOCAL_ADMISSION]:
- VALUE-import `workbox-build` only in the Node build script; `browser/shell.ts` imports `RuntimeCaching`/`StrategyName` type-only. `vite-plugin-pwa` (tooling catalog) internalizes `injectManifest`/`generateSW` into the Vite pipeline — the browser catalog owns the type seam, the tooling catalog owns the plugin wiring — under the folder-local `# browser` catalog group.

[RAIL_LAW]:
- Package: `workbox-build`
- Owns: build-time precache-manifest computation and service-worker emission, and the `StrategyName`/`RuntimeCaching` cache-route vocabulary.
- Accept: `injectManifest` over an authored SW; `RuntimeCaching`/`StrategyName` as type-only row data in runtime code; the shared option partials composed per emit function.
- Reject: value-importing `workbox-build` into the browser bundle; per-route imperative caching outside the row projection; `generateSW` when the worker is authored, which `injectManifest` owns.

# [TS_RUNTIME_API_WORKBOX_BUILD]

`workbox-build` is a NODE build-time tool. It runs inside the app build (driven by the Vite PWA plugin, tooling tier), reads a glob config, and writes the precache manifest plus the service-worker asset — it never enters the browser runtime bundle. Its two load-bearing surfaces are the emit functions (`generateSW`/`injectManifest`/`getManifest`) run only in the build script, and the `RuntimeCaching`/`StrategyName` TYPES that `browser/shell.md` composes as runtime-cache route rows through a type-only import. `workbox-window` registers the emitted asset at runtime — distinct altitude, one concern each.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `workbox-build`
- package: `workbox-build` (MIT)
- module: `build/index.js`; types `build/index.d.ts`; a Node build tool — globs the filesystem and writes files, never bundled into the app
- marker: VALUE import restricted to the build script (Node-only); TYPE-only import (`RuntimeCaching`, `StrategyName`) in runtime rows; the standing composition is `vite-plugin-pwa` (tooling catalog), which internalizes `injectManifest`/`generateSW`
- exports: `generateSW`, `injectManifest`, `getManifest`, `copyWorkboxLibraries`, `getModuleURL`, and the full `./types` surface
- bound asset: TSDECL `node_modules/workbox-build/build/{index,generate-sw,inject-manifest,get-manifest,types}.d.ts`
- admission: folder-local `# browser` catalog group; version centralized in `pnpm-workspace.yaml`
- role: `browser/shell.md` (the precache manifest emitted at app build + the strategy-row vocabulary); the SW asset `browser/shell.md` and `workbox-window` consume at runtime
- rail: `browser/shell` (build-time)

## [02]-[EMIT_ENTRYPOINTS]

One concern — precache-manifest generation — in three output modalities. `injectManifest` is the Rasm mode: the SW source is authored (Effect-driven strategy rows) and the manifest is injected at a placeholder, never fully generated.

[SURFACES]: `injectManifest(InjectManifestOptions) -> Promise<BuildResult>` `generateSW(GenerateSWOptions) -> Promise<BuildResult>` `getManifest(GetManifestOptions) -> Promise<GetManifestResult>` `copyWorkboxLibraries(string) -> Promise<string>` `getModuleURL(string,BuildType) -> string`

Consumer note: `injectManifest` is the entry the build script calls — the authored SW carries the strategy rows and the `NavigationRoute` offline shell, `workbox-build` injects the precache manifest. `getManifest` alone feeds the build-time per-route prerendered HTML into the precache (the SEO surface). Reserve `generateSW` for a fully-generated SW; it is the non-goal when the worker is authored.

## [03]-[STRATEGY_VOCABULARY]

`StrategyName` is the closed route-strategy axis; `RuntimeCaching` is the one route-row shape both the build and (type-only) the runtime fold key on. Each cache route is ONE data row projected from a keyed strategy behavior, never a per-route imperative handler.

[STRATEGY_NAME]: `StrategyName = "CacheFirst"|"CacheOnly"|"NetworkFirst"|"NetworkOnly"|"StaleWhileRevalidate"`
[RUNTIME_CACHING]: `RuntimeCaching.handler: RouteHandler|StrategyName` `RuntimeCaching.urlPattern: RegExp|string|RouteMatchCallback` `RuntimeCaching.method: HTTPMethod` `RuntimeCaching.options: {…}`

Consumer note: the parameterized owner is a keyed `StrategyBehavior` row projected through a `Record<CacheStrategy, StrategyName>` lookup — the static `cache-first`, api `network-first`, and geo/glb `stale-while-revalidate` routes are three rows, never three imperative branches. `backgroundSync.options: QueueOptions` is where the window-observed replay queue (§workbox-window) is configured; `networkTimeoutSeconds` pairs with `navigationPreload` to race the network-first HTML route.

## [04]-[MANIFEST_OPTION_ALGEBRA]

The option types compose from shared partials, so `generateSW`/`injectManifest`/`getManifest` differ only by the SW-destination and glob-directory requirements they add.

[GENERATE_SWOPTIONS]: `GenerateSWOptions = BasePartial&GlobPartial&GeneratePartial&RequiredSWDestPartial&OptionalGlobDirectoryPartial`
[INJECT_MANIFEST_OPTIONS]: `InjectManifestOptions = BasePartial&GlobPartial&InjectPartial&RequiredSWDestPartial&RequiredGlobDirectoryPartial`
[GET_MANIFEST_OPTIONS]: `GetManifestOptions = BasePartial&GlobPartial&RequiredGlobDirectoryPartial`
[MANIFEST_ENTRY]: `ManifestEntry.url: string` `ManifestEntry.revision: string|null` `ManifestEntry.integrity: string`
[MANIFEST_TRANSFORM]: `ManifestTransform = (entries:Array<ManifestEntry&{…}>,compilation?:unknown)=>Promise<ManifestTransformResult>|ManifestTransformResult`
[GET_MANIFEST_RESULT]: `GetManifestResult.count: number` `GetManifestResult.manifestEntries: Array<ManifestEntry>` `GetManifestResult.size: number` `GetManifestResult.warnings: Array<string>`
[BUILD_RESULT]: `BuildResult = Omit<GetManifestResult,"manifestEntries">&{filePaths:Array<string>}`

Consumer note: `navigateFallback` + `navigationPreload` arm the SPA offline app-shell and the network-first HTML race; `manifestTransforms` and `ManifestEntry.revision` key the content-addressed precache (a hashed filename sets `revision: null`, exempting it from cache-busting); `maximumFileSizeToCacheInBytes` bounds the precache against an accidental large-asset match.

## [05]-[STACKING]

- sibling `workbox-window`: the same `RuntimeCaching`/`StrategyName` types cross the build/runtime seam (type-only in the runtime fold); the SW asset generated here is the exact script `Workbox` registers, so the strategy rows are one source of truth.
- `vite-plugin-pwa` (tooling): internalizes `injectManifest`/`generateSW` into the Vite pipeline — the browser catalog owns the `workbox-build` surface, the tooling catalog owns the plugin wiring; the runtime rows import types only.
- `effect` `Schema`: validate `RuntimeCaching`/`GenerateSWOptions` at the build boundary so a malformed strategy row fails the build, not the runtime; the `StrategyBehavior` → `RuntimeCaching` projection is a total `Record.map`, matching the keyed-vocabulary lookup over a `Match` chain.
- `core/value/identity`: `ManifestEntry.revision` and `cacheId` derive from the same `ContentKey`/build fingerprint the runtime cache-version uses, so build-time and runtime agree on the precache identity.
- `browser` prerender rows: `getManifest` folds the build-time per-route static HTML into the precache manifest, hydrated by `boot` — the client-rendered-PWA-plus-prerender SEO posture.

## [06]-[RAIL_LAW]

- Owns: BUILD-TIME precache/SW emission and the cache-strategy vocabulary.
- Accept: `injectManifest` over an authored SW; `RuntimeCaching`/`StrategyName` as type-only data rows; the shared option partials.
- Reject: value-importing `workbox-build` into runtime code (Node-only, named defect); per-route imperative caching outside the row projection; `generateSW` when the SW is authored (use `injectManifest`).

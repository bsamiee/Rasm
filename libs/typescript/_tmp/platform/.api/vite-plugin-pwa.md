# [API_CATALOGUE] vite-plugin-pwa

`vite-plugin-pwa` is the build-time PWA pipeline: `VitePWA(userOptions?)` returns a `Plugin[]` (SPREAD, never one element) that emits the service-worker asset and precache manifest via `workbox-build`, injects the web app manifest, and optionally generates icons via the optional `@vite-pwa/assets-generator` peer. Two strategies — `generateSW` (Workbox writes the SW from `workbox` options) and `injectManifest` (a custom SW in `srcDir/filename` gets the precache manifest injected). It exposes a rich programmatic surface (`VitePluginPWAAPI`) and a family of virtual modules (`virtual:pwa-register[/react]`, `virtual:pwa-info`, `virtual:pwa-assets/*`) for runtime registration. In `platform` the surface splits across two pages: `Shell/build.md` `BuildPipeline` calls `VitePWA` at build time and feeds it the `Shell/serviceworker.md` `runtimeCachingManifest` (a `workbox-build` `RuntimeCaching[]`) through `workbox.runtimeCaching`; the RUNTIME lifecycle is owned by `serviceworker.md` `ServiceWorkerHost` over `workbox-window` `Workbox` directly, so `platform` sets `injectRegister: false` and DELIBERATELY bypasses `virtual:pwa-register/react` `useRegisterSW` — a React-state hook that cannot live in the Effect `Layer` graph.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `vite-plugin-pwa`
- package: `vite-plugin-pwa`
- version: `1.3.0`
- license: `MIT`
- module: dual ESM/CJS (`import` → `dist/index.js`/`dist/index.d.ts`, `require` → `dist/index.cjs`/`dist/index.d.cts`), `"type": "module"`, `"sideEffects": false` (tree-shakeable). Subpath exports: `./types` (`RegisterSWOptions`), `./info` (`virtual:pwa-info`), `./pwa-assets` (`virtual:pwa-assets/head`+`/icons`), `./client` (all framework register variants), and per-framework register decls `./react` `./vue` `./svelte` `./solid` `./preact` `./vanillajs`
- asset: Vite plugin (returns `Plugin[]`). Peer floor is the ABI gate — `vite: ^3.1.0 || … || ^8.0.0`, `workbox-build: ^7.4.1`, `workbox-window: ^7.4.1` (both also bundled deps), and OPTIONAL `@vite-pwa/assets-generator: ^1.0.0` (required only when `pwaAssets` is active; its `IconAsset`/`FaviconLink`/`Preset` types back the assets surface). Deps `tinyglobby`, `pretty-bytes`, `debug`
- rail: pwa — SW generation, manifest injection, precache, PWA-asset pipeline
- catalog-verdict: KEEP; the PWA build-emit admitted at `Shell/build.md`; its runtime lifecycle is `Shell/serviceworker.md`'s over `workbox-window`, not this plugin's virtual register

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the plugin options bag + strategy axes
- rail: pwa

| [INDEX] | [SYMBOL]                      | [TYPE_FAMILY]  | [BOUNDARY_NOTE]                                          |
| :-----: | :---------------------------- | :------------- | :------------------------------------------------------- |
|  [01]   | `VitePWAOptions`              | options object | full config; `VitePWA` accepts `Partial<VitePWAOptions>` |
|  [02]   | `Options`                     | alias          | re-export of `VitePWAOptions`                            |
|  [03]   | `CustomInjectManifestOptions` | options object | `injectManifest` extras (below); extends workbox `InjectManifestOptions` |
|  [04]   | `DevOptions`                  | options object | dev-server SW behavior                                   |
|  [05]   | `PWAIntegration`              | hooks object   | `beforeBuildServiceWorker`/`closeBundleOrder`/`configureOptions`/`configureCustomSWViteBuild` |
|  [06]   | `PWAAssetsOptions`            | options object | icon/asset generation (needs the optional peer)          |
|  [07]   | `InjectManifestVitePlugins`   | type alias     | `string[] \| ((ids: string[]) => string[])`             |

[PUBLIC_TYPE_SCOPE]: `VitePWAOptions` key fields
- rail: pwa

| [INDEX] | [FIELD]                | [TYPE]                                                       | [DEFAULT]                 | [BOUNDARY_NOTE]                                  |
| :-----: | :--------------------- | :----------------------------------------------------------- | :------------------------ | :---------------------------------------------- |
|  [01]   | `strategies`           | `'generateSW' \| 'injectManifest'`                           | `'generateSW'`            | picks the SW authoring model                    |
|  [02]   | `registerType`         | `'prompt' \| 'autoUpdate'`                                    | `'prompt'`                | the register mode — NO `'legacy'` value exists  |
|  [03]   | `injectRegister`       | `'inline' \| 'script' \| 'script-defer' \| 'auto' \| null \| false` | `'auto'`            | `false` = register the SW yourself (platform sets this) |
|  [04]   | `workbox`              | `Partial<GenerateSWOptions>` (workbox-build)                 | `{}`                      | receives `serviceworker.md` `runtimeCaching`    |
|  [05]   | `injectManifest`       | `Partial<CustomInjectManifestOptions>`                       | `{}`                      | the custom-SW build config                      |
|  [06]   | `manifest`             | `Partial<ManifestOptions> \| false`                          | `{}`                      | the web app manifest (family below)             |
|  [07]   | `pwaAssets`            | `PWAAssetsOptions`                                           | —                         | icon generation; needs the optional peer        |
|  [08]   | `devOptions`           | `DevOptions`                                                | `{}`                      | dev SW behavior (`enabled` default `false`)     |
|  [09]   | `integration`          | `PWAIntegration`                                            | —                         | framework-integration hooks                     |
|  [10]   | `srcDir` / `outDir`    | `string` / `string`                                         | `'public'` / `'dist'`     | custom-SW source / build output dirs            |
|  [11]   | `filename`             | `string`                                                    | `'sw.js'`                 | the emitted SW filename (`ServiceWorkerHost` registers this) |
|  [12]   | `manifestFilename`     | `string`                                                    | `'manifest.webmanifest'`  | the emitted manifest filename                   |
|  [13]   | `scope` / `base`       | `string` / `string`                                        | vite `base`               | SW registration scope / PWA base override       |
|  [14]   | `includeAssets`        | `string \| string[] \| undefined`                          | —                         | extra `publicDir` files to precache             |
|  [15]   | `includeManifestIcons` | `boolean`                                                   | `true`                    | auto-precache manifest icons                    |
|  [16]   | `minify`               | `boolean`                                                   | `true`                    | minify the generated manifest                   |
|  [17]   | `useCredentials`       | `boolean`                                                   | `false`                   | `crossorigin="use-credentials"` on the manifest link |
|  [18]   | `disable`              | `boolean`                                                   | `false`                   | disable SW gen/registration on build            |
|  [19]   | `selfDestroying`       | `boolean`                                                   | `false`                   | emit an unregistering SW (kill-switch build)    |
|  [20]   | `buildBase`            | `string`                                                   | vite `base`               | SW/manifest path when build dir ≠ base root     |
|  [21]   | `mode`                 | `'development' \| 'production'`                             | `NODE_ENV`                | ignored by `injectManifest` since v0.18         |

[PUBLIC_TYPE_SCOPE]: `CustomInjectManifestOptions` extras (over workbox `InjectManifestOptions`)
- rail: pwa

| [INDEX] | [FIELD]                            | [TYPE]                                              | [BOUNDARY_NOTE]                                    |
| :-----: | :--------------------------------- | :-------------------------------------------------- | :------------------------------------------------- |
|  [01]   | `rollupFormat`                     | `'es' \| 'iife'`                                    | SW bundle format (default `'es'`)                  |
|  [02]   | `target` / `minify` / `sourcemap`  | `BuildOptions['target' \| 'minify' \| 'sourcemap']` | per-SW Vite build overrides                        |
|  [03]   | `buildPlugins`                     | `{ rollup?; vite? }`                                | separate plugin set for the SW build              |
|  [04]   | `rollupOptions`                    | `Omit<RollupOptions, 'plugins' \| 'output'>`        | SW Rollup config                                   |
|  [05]   | `envOptions`                       | `{ envDir?; envPrefix? }`                           | SW env resolution                                  |
|  [06]   | `enableWorkboxModulesLogs`         | `true`                                              | keep workbox logs in the SW                        |

[PUBLIC_TYPE_SCOPE]: web app manifest family
- rail: pwa
- `ManifestOptions` is the full W3C manifest; the unions type each constrained field. The design supplies a `Partial<ManifestOptions>`.

| [INDEX] | [SYMBOL]                 | [SHAPE_OR_VALUES]                                                                                   | [BOUNDARY_NOTE]                          |
| :-----: | :----------------------- | :------------------------------------------------------------------------------------------------- | :--------------------------------------- |
|  [01]   | `ManifestOptions`        | `name; short_name; description; icons: IconResource[]; start_url; scope; id; display; display_override; background_color; theme_color; orientation; categories; screenshots; shortcuts; share_target; file_handlers; protocol_handlers; launch_handler; edge_side_panel; scope_extensions; handle_links; …` | the full manifest |
|  [02]   | `IconResource`           | `{ src; sizes?; type?; purpose?: StringLiteralUnion<IconPurpose> \| IconPurpose[] }`               | one `icons[]` entry                      |
|  [03]   | `Display`                | `'fullscreen' \| 'standalone' \| 'minimal-ui' \| 'browser'`                                         | `display`                                |
|  [04]   | `DisplayOverride`        | `Display \| 'window-controls-overlay'`                                                              | `display_override[]`                     |
|  [05]   | `IconPurpose`            | `'monochrome' \| 'maskable' \| 'any'`                                                               | icon `purpose`                           |
|  [06]   | `LaunchHandlerClientMode`| `'auto' \| 'focus-existing' \| 'navigate-existing' \| 'navigate-new'`                              | `launch_handler.client_mode`             |
|  [07]   | `ShareTargetFiles`       | `{ name; accept: string \| string[] }`                                                             | `share_target.params.files`              |
|  [08]   | `ScopeExtensionsType`    | `'origin'`                                                                                          | `scope_extensions[].type`                |
|  [09]   | `StringLiteralUnion<T>`  | `T \| (string & Nothing)`                                                                           | literal-union with open-string fallback  |

[PUBLIC_TYPE_SCOPE]: programmatic API + resolved shapes
- rail: pwa

| [INDEX] | [SYMBOL]                    | [SHAPE]                                                                                             | [BOUNDARY_NOTE]                              |
| :-----: | :-------------------------- | :------------------------------------------------------------------------------------------------- | :-------------------------------------------- |
|  [01]   | `VitePluginPWAAPI`          | the plugin `api` (methods in ENTRYPOINTS)                                                           | build-integration surface                     |
|  [02]   | `WebManifestData`           | `{ href; useCredentials; toLinkTag(): string }`                                                     | `webManifestData()` return                    |
|  [03]   | `RegisterSWData`            | `{ shouldRegisterSW; mode: 'inline'\|'script'\|'script-defer'; inlinePath; registerPath; scope; type: WorkerType; toScriptTag(): string \| undefined }` | `registerSWData()` return |
|  [04]   | `ExtendManifestEntriesHook` | `(entries: (string \| ManifestEntry)[]) => (string \| ManifestEntry)[] \| undefined`               | `extendManifestEntries` arg                   |
|  [05]   | `ResolvedVitePWAOptions`    | `Required<…>` + `swSrc`/`swDest`/resolved `workbox`/`injectManifest`                                | the fully-resolved config                     |
|  [06]   | `ResolvedServiceWorkerOptions` | `{ format: 'es'\|'iife'; plugins?; rollupOptions }`                                              | resolved SW build inputs                       |
|  [07]   | `ResolvedPWAAssetsOptions`  | resolved `pwaAssets` + `images: string[]`                                                           | resolved assets config                         |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: plugin factory + value exports
- rail: pwa

| [INDEX] | [SURFACE]                          | [ENTRY_FAMILY] | [BOUNDARY_NOTE]                                                    |
| :-----: | :--------------------------------- | :------------- | :---------------------------------------------------------------- |
|  [01]   | `VitePWA(userOptions?)`            | plugin factory | `(userOptions?: Partial<VitePWAOptions>) => Plugin[]`; SPREAD it   |
|  [02]   | `cachePreset`                      | `RuntimeCaching[]` | the default Workbox runtime-cache preset                       |
|  [03]   | `defaultInjectManifestVitePlugins` | `string[]`     | default Vite plugin ids kept for the SW build                     |

[ENTRYPOINT_SCOPE]: `VitePluginPWAAPI` — the build-integration surface (`plugin.api`)
- rail: pwa

| [INDEX] | [SURFACE]                          | [RETURNS]                                    | [BOUNDARY_NOTE]                              |
| :-----: | :--------------------------------- | :------------------------------------------- | :-------------------------------------------- |
|  [01]   | `disabled`                         | `boolean`                                    | is the plugin disabled                        |
|  [02]   | `pwaInDevEnvironment`              | `boolean`                                    | running on the dev server                     |
|  [03]   | `webManifestData()`                | `WebManifestData \| undefined`               | manifest url + `toLinkTag()`                  |
|  [04]   | `registerSWData()`                 | `RegisterSWData \| undefined`                | how the SW is being registered               |
|  [05]   | `extendManifestEntries(fn)`        | `void`                                       | mutate the precache entry list               |
|  [06]   | `generateBundle(bundle?, ctx?)`    | `OutputBundle \| undefined`                  | hook the precache into a bundle              |
|  [07]   | `generateSW()`                     | `Promise<void>`                              | force SW generation                          |
|  [08]   | `pwaAssetsGenerator()`             | `Promise<PWAAssetsGenerator \| undefined>`   | the icon generator (methods below)          |

[ENTRYPOINT_SCOPE]: `PWAAssetsGenerator` (via `pwaAssetsGenerator()`)
- rail: pwa

| [INDEX] | [SURFACE]                    | [RETURNS]                              | [BOUNDARY_NOTE]                              |
| :-----: | :--------------------------- | :------------------------------------- | :-------------------------------------------- |
|  [01]   | `generate()`                 | `Promise<void>`                        | write all generated assets                    |
|  [02]   | `resolveHtmlAssets()`        | `PWAHtmlAssets`                        | `{ links; themeColor? }` for head injection   |
|  [03]   | `transformIndexHtml(html)`   | `string`                               | inject asset links into `index.html`          |
|  [04]   | `injectManifestIcons()`      | `void`                                 | write generated icons into the manifest       |
|  [05]   | `icons()` / `instructions()` | `PWAAssetsIcons` / `ImageAssetsInstructions` | resolved icon buckets / generation plan  |
|  [06]   | `findIconAsset(path)`        | `Promise<ResolvedIconAsset \| undefined>` | one resolved asset                         |

[ENTRYPOINT_SCOPE]: virtual modules — the runtime register + info surface
- rail: pwa
- The default runtime registration path; `platform` bypasses `/react` (uses `workbox-window` via `serviceworker.md`) but `virtual:pwa-info` remains useful for the manifest link tag.

| [INDEX] | [MODULE]                     | [EXPORT]                                                                                            | [BOUNDARY_NOTE]                                    |
| :-----: | :--------------------------- | :------------------------------------------------------------------------------------------------- | :------------------------------------------------- |
|  [01]   | `virtual:pwa-register`       | `registerSW(options?: RegisterSWOptions): (reloadPage?: boolean) => Promise<void>`                 | framework-free register                            |
|  [02]   | `virtual:pwa-register/react` | `useRegisterSW(options?): { needRefresh: [boolean, Dispatch<SetStateAction<boolean>>]; offlineReady: [boolean, …]; updateServiceWorker: (reloadPage?: boolean) => Promise<void> }` | React hook — BYPASSED by platform |
|  [03]   | `virtual:pwa-info`           | `pwaInfo: { pwaInDevEnvironment; webManifest: { href; useCredentials; linkTag } ; registerSW? } \| undefined` | the manifest `<link>` source          |
|  [04]   | `virtual:pwa-assets/head`    | `pwaAssetsHead: { links: PWAAssetHeadLink[]; themeColor? }`                                         | integration-only asset head links                  |
|  [05]   | `virtual:pwa-assets/icons`   | `pwaAssetsIcons: { favicon; transparent; maskable; apple; appleSplashScreen }`                     | integration-only icon buckets                      |
|  [06]   | `RegisterSWOptions`          | `{ immediate?; onNeedRefresh?; onOfflineReady?; onRegisteredSW?(url, reg); onRegisterError?(e); onNeedReload? }` | from `vite-plugin-pwa/types`         |

```ts contract
// vite-plugin-pwa — build wiring in Shell/build.md
import { Plugin } from 'vite'
import { RuntimeCaching } from 'workbox-build'
import { runtimeCachingManifest } from '../serviceworker' // ReadonlyArray<RuntimeCaching>

declare function VitePWA(userOptions?: Partial<VitePWAOptions>): Plugin[]
declare const cachePreset: RuntimeCaching[]
declare const defaultInjectManifestVitePlugins: string[]

const pwa: Plugin[] = VitePWA({
  strategies: 'generateSW',
  registerType: 'autoUpdate',
  injectRegister: false, // platform drives the SW via workbox-window (serviceworker.md)
  workbox: { runtimeCaching: [...runtimeCachingManifest], navigateFallback: '/index.html' },
  manifest: { name: 'Rasm', short_name: 'Rasm', display: 'standalone', theme_color: '#0b0b0b' },
})
// plugins: [react(), tailwindcss(), vitePluginSvgr(), ViteImageOptimizer({}), viteCompression(), ...pwa]
```

## [04]-[IMPLEMENTATION_LAW]

[PWA_TOPOLOGY]:
- `VitePWA` returns `Plugin[]`; spread into the Vite `plugins` array, never a single element, and call it exactly once — the array owns all hooks.
- `strategies: 'generateSW'` lets Workbox write the SW from `workbox: Partial<GenerateSWOptions>`; `'injectManifest'` builds a custom SW in `srcDir/filename` and injects the precache manifest, configured by `injectManifest: Partial<CustomInjectManifestOptions>`.
- `injectRegister` controls how the register snippet is injected; `false` opts the app out so it registers the SW itself.
- `pwaAssets` drives icon generation from one source image (`image`, default `public/favicon.svg`; `preset` default `minimal-2023`) via the optional `@vite-pwa/assets-generator`; auto-discovers a `pwa-assets.config.*` or takes `config`.

[INTEGRATION_LAW]:
- The build/runtime split is the load-bearing seam. `Shell/build.md` `BuildPipeline` owns the BUILD half: it calls `VitePWA({...})` and feeds `workbox.runtimeCaching` the `Shell/serviceworker.md` `runtimeCachingManifest` (`ReadonlyArray<RuntimeCaching>`, authored by the `CacheStrategy`→`RuntimeCaching` projection). So the `RuntimeCaching`/`StrategyName` vocabulary lives once in `serviceworker.md`; `VitePWA`'s `workbox` config is its build-time consumer, never a second authoring site.
- `platform` sets `injectRegister: false` and does NOT import `virtual:pwa-register/react` `useRegisterSW`: that hook returns `[boolean, Dispatch]` React-state tuples that cannot enter the Effect `Layer` graph. Instead `serviceworker.md` `ServiceWorkerHost` registers the emitted `/sw.js` through `workbox-window` `Workbox` under `Effect.acquireRelease`, bridges the `WorkboxLifecycleEventMap` events through `Stream.asyncScoped` into a `SwLifecycle` `SubscriptionRef`, and drives skip-waiting with `messageSkipWaiting()`. `platform` therefore consumes ONLY the build half of this plugin — its virtual register surface is the default this design deliberately overrides (mirroring `nuqs`'s bypassed React hooks and `browserslist`'s build-time-only posture).
- `manifest: Partial<ManifestOptions>` supplies the web app manifest at build; the `VitePluginPWAAPI.webManifestData().toLinkTag()` / `virtual:pwa-info` `pwaInfo.webManifest.linkTag` yield the `<link rel="manifest">` the SPA `index.html` injects — the one place the runtime touches this plugin, and only for the static link tag.
- `pwaAssets` is gated behind the optional peer; when absent the design keeps hand-authored icons in `manifest.icons` and the `virtual:pwa-assets/*` modules stay unused (they are integration-only, per their own warning).
- Build-time only for the emit; the plugin never enters the `platform` `Layer` graph and holds no service tag. The SINGLE runtime dependency (`workbox-window`) is owned by `serviceworker.md`, catalogued separately — this catalog owns the emit + config + programmatic API, not the runtime lifecycle.

[RAIL_LAW]:
- Package: `vite-plugin-pwa`
- Owns: build-time SW generation/injection via `workbox-build`, web app manifest emission, precache manifest, the PWA-asset pipeline, and the `VitePluginPWAAPI`/virtual-module surfaces
- Accept: `Partial<VitePWAOptions>` on `VitePWA()`; `workbox.runtimeCaching` fed from `serviceworker.md`; `manifest` for the web app manifest; `injectRegister: false` to cede runtime registration; `plugin.api` for build integration
- Reject: `registerType: 'legacy'` (nonexistent) and `VitePluginPWAAPI.injectManifest()` (nonexistent — use `generateSW()`/`generateBundle()`); calling `VitePWA` more than once or passing it un-spread; re-authoring the `RuntimeCaching`/`StrategyName` vocabulary here (owned by `serviceworker.md`); importing `virtual:pwa-register/react` in `platform` (the `Layer` graph owns the SW lifecycle via `workbox-window`)

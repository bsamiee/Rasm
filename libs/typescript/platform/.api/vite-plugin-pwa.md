# [API_CATALOGUE] vite-plugin-pwa

`vite-plugin-pwa` supplies service worker generation and injection, web app manifest management, PWA asset generation, and offline caching strategy configuration via Workbox for Vite build pipelines. The single `VitePWA(userOptions?)` entrypoint returns a `Plugin[]` array compatible with the Vite `plugins` array. Two strategies are available: `generateSW` (Workbox-generated SW) and `injectManifest` (custom SW with injected precache manifest).

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `vite-plugin-pwa`
- package: `vite-plugin-pwa`
- module: `vite-plugin-pwa` (ESM default), `vite-plugin-pwa/types`, `vite-plugin-pwa/react`, `vite-plugin-pwa/vue`, `vite-plugin-pwa/svelte`, `vite-plugin-pwa/solid`, `vite-plugin-pwa/vanillajs`, `vite-plugin-pwa/client`, `vite-plugin-pwa/info`, `vite-plugin-pwa/pwa-assets`
- asset: Vite plugin (returns `Plugin[]`)
- rail: pwa

## [2]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: plugin options family
- rail: pwa

| [INDEX] | [SYMBOL]                      | [TYPE_FAMILY]  | [DESCRIPTION]                    |
| :-----: | :---------------------------- | :------------- | :------------------------------- |
|   [1]   | `VitePWAOptions`              | options object | full plugin configuration        |
|   [2]   | `Options`                     | type alias     | re-export of `VitePWAOptions`    |
|   [3]   | `ResolvedVitePWAOptions`      | resolved shape | internal resolved configuration  |
|   [4]   | `ManifestOptions`             | manifest shape | web app manifest fields          |
|   [5]   | `DevOptions`                  | dev config     | dev-server SW behavior           |
|   [6]   | `CustomInjectManifestOptions` | build config   | `injectManifest` strategy extras |
|   [7]   | `PWAAssetsOptions`            | assets config  | icon/asset generation            |
|   [8]   | `PWAIntegration`              | integration    | framework integration hooks      |

[PUBLIC_TYPE_SCOPE]: strategy and register types
- rail: pwa

| [INDEX] | [SYMBOL]                    | [TYPE_FAMILY]  | [DESCRIPTION]                     |
| :-----: | :-------------------------- | :------------- | :-------------------------------- |
|   [1]   | `RegisterSWData`            | virtual module | SW registration data              |
|   [2]   | `VitePluginPWAAPI`          | plugin API     | programmatic plugin API surface   |
|   [3]   | `WebManifestData`           | manifest data  | resolved manifest data            |
|   [4]   | `InjectManifestVitePlugins` | type alias     | `string[] \| ((ids) => string[])` |
|   [5]   | `Display`                   | string union   | web manifest display modes        |
|   [6]   | `DisplayOverride`           | string union   | display override values           |
|   [7]   | `IconPurpose`               | string union   | icon purpose values               |
|   [8]   | `StringLiteralUnion`        | utility type   | string union helper               |

[PUBLIC_TYPE_SCOPE]: PWA assets types
- rail: pwa

| [INDEX] | [SYMBOL]                   | [TYPE_FAMILY]  | [DESCRIPTION]                                           |
| :-----: | :------------------------- | :------------- | :------------------------------------------------------ |
|   [1]   | `PWAAssetsIcons`           | icon catalog   | favicon/transparent/maskable/apple/splashscreen buckets |
|   [2]   | `PWAAssetsGenerator`       | generator API  | asset generation and HTML injection                     |
|   [3]   | `PWAHtmlLink`              | HTML link      | `<link>` element descriptor                             |
|   [4]   | `PWAHtmlAssets`            | HTML assets    | collected links and theme-color                         |
|   [5]   | `ResolvedIconAsset`        | resolved asset | path, mimeType, buffer, age                             |
|   [6]   | `ResolvedPWAAssetsOptions` | resolved opts  | resolved PWA assets configuration                       |
|   [7]   | `ColorSchemeMeta`          | meta element   | `name`/`content` meta descriptor                        |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: plugin factory
- rail: pwa

| [INDEX] | [SURFACE]                          | [ENTRY_FAMILY] | [DESCRIPTION]                         |
| :-----: | :--------------------------------- | :------------- | :------------------------------------ |
|   [1]   | `VitePWA(userOptions?)`            | plugin factory | returns `Plugin[]` for Vite `plugins` |
|   [2]   | `cachePreset`                      | constant       | default Workbox runtime cache preset  |
|   [3]   | `defaultInjectManifestVitePlugins` | constant       | default Vite plugins for SW build     |

[ENTRYPOINT_SCOPE]: VitePWAOptions key fields
- rail: pwa

| [INDEX] | [FIELD]                | [TYPE]                                                      | [DEFAULT]      |
| :-----: | :--------------------- | :---------------------------------------------------------- | :------------- |
|   [1]   | `strategies`           | `'generateSW' \| 'injectManifest'`                          | `'generateSW'` |
|   [2]   | `registerType`         | `'prompt' \| 'autoUpdate' \| 'legacy'`                      | `'prompt'`     |
|   [3]   | `injectRegister`       | `'auto' \| 'inline' \| 'script' \| 'script-defer' \| false` | `'auto'`       |
|   [4]   | `manifest`             | `Partial<ManifestOptions> \| false`                         | `{}`           |
|   [5]   | `workbox`              | `Partial<GenerateSWOptions>`                                | `{}`           |
|   [6]   | `injectManifest`       | `Partial<CustomInjectManifestOptions>`                      | `{}`           |
|   [7]   | `devOptions`           | `DevOptions`                                                | `{}`           |
|   [8]   | `pwaAssets`            | `PWAAssetsOptions`                                          | —              |
|   [9]   | `filename`             | `string`                                                    | `'sw.js'`      |
|  [10]   | `scope`                | `string`                                                    | Vite `base`    |
|  [11]   | `base`                 | `string`                                                    | Vite `base`    |
|  [12]   | `includeAssets`        | `string \| string[] \| RegExp`                              | —              |
|  [13]   | `includeManifestIcons` | `boolean`                                                   | `true`         |
|  [14]   | `integration`          | `PWAIntegration`                                            | —              |

## [4]-[IMPLEMENTATION_LAW]

[PWA_TOPOLOGY]:
- entry: `VitePWA(opts)` returns `Plugin[]`; spread into Vite `plugins` array, not passed as a single element
- strategy `generateSW`: Workbox generates the service worker from `workbox` options; no custom SW source needed
- strategy `injectManifest`: custom SW source in `srcDir/filename`; Workbox injects precache manifest entries at build time
- virtual module `virtual:pwa-register` supplies the SW registration helper; framework-specific variants live under sub-paths (`/react`, `/vue`, etc.)
- `pwaAssets` drives icon generation from a single source image using `@vite-pwa/assets-generator` presets; config file is auto-discovered or set via `pwaAssets.config`
- `PWAAssetsGenerator.transformIndexHtml` and `injectManifestIcons` handle HTML injection of generated asset links

[LOCAL_ADMISSION]:
- Pass `Partial<VitePWAOptions>` to `VitePWA`; resolved options are available via `VitePluginPWAAPI.generateSW()` / `.injectManifest()`.
- Do not call `VitePWA` multiple times; the plugin array manages all hooks internally.
- `devOptions.enabled` defaults to `false`; set to `true` only when SW testing in dev is required.

[RAIL_LAW]:
- Package: `vite-plugin-pwa`
- Owns: service worker generation, web app manifest injection, PWA asset pipeline
- Accept: `Partial<VitePWAOptions>` passed to `VitePWA()`
- Reject: manual Workbox configuration outside `workbox`/`injectManifest` fields; direct manipulation of generated SW files

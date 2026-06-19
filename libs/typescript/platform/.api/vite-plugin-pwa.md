# [API_CATALOGUE] vite-plugin-pwa

`vite-plugin-pwa` supplies service worker generation and injection, web app manifest management, PWA asset generation, and offline caching strategy configuration via Workbox for Vite build pipelines. The single `VitePWA(userOptions?)` entrypoint returns a `Plugin[]` array compatible with the Vite `plugins` array. Two strategies are available: `generateSW` (Workbox-generated SW) and `injectManifest` (custom SW with injected precache manifest).

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `vite-plugin-pwa`
- package: `vite-plugin-pwa`
- module: `vite-plugin-pwa` (ESM default), `vite-plugin-pwa/types`, `vite-plugin-pwa/react`, `vite-plugin-pwa/vue`, `vite-plugin-pwa/svelte`, `vite-plugin-pwa/solid`, `vite-plugin-pwa/vanillajs`, `vite-plugin-pwa/client`, `vite-plugin-pwa/info`, `vite-plugin-pwa/pwa-assets`
- asset: Vite plugin (returns `Plugin[]`)
- rail: pwa

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: plugin options family
- rail: pwa

| [INDEX] | [SYMBOL]                      | [TYPE_FAMILY]  | [DESCRIPTION]                    |
| :-----: | :---------------------------- | :------------- | :------------------------------- |
|  [01]   | `VitePWAOptions`              | options object | full plugin configuration        |
|  [02]   | `Options`                     | type alias     | re-export of `VitePWAOptions`    |
|  [03]   | `ResolvedVitePWAOptions`      | resolved shape | internal resolved configuration  |
|  [04]   | `ManifestOptions`             | manifest shape | web app manifest fields          |
|  [05]   | `DevOptions`                  | dev config     | dev-server SW behavior           |
|  [06]   | `CustomInjectManifestOptions` | build config   | `injectManifest` strategy extras |
|  [07]   | `PWAAssetsOptions`            | assets config  | icon/asset generation            |
|  [08]   | `PWAIntegration`              | integration    | framework integration hooks      |

[PUBLIC_TYPE_SCOPE]: strategy and register types
- rail: pwa

| [INDEX] | [SYMBOL]                    | [TYPE_FAMILY]  | [DESCRIPTION]                     |
| :-----: | :-------------------------- | :------------- | :-------------------------------- |
|  [01]   | `RegisterSWData`            | virtual module | SW registration data              |
|  [02]   | `VitePluginPWAAPI`          | plugin API     | programmatic plugin API surface   |
|  [03]   | `WebManifestData`           | manifest data  | resolved manifest data            |
|  [04]   | `InjectManifestVitePlugins` | type alias     | `string[] \| ((ids) => string[])` |
|  [05]   | `Display`                   | string union   | web manifest display modes        |
|  [06]   | `DisplayOverride`           | string union   | display override values           |
|  [07]   | `IconPurpose`               | string union   | icon purpose values               |
|  [08]   | `StringLiteralUnion`        | utility type   | string union helper               |

[PUBLIC_TYPE_SCOPE]: PWA assets types
- rail: pwa

| [INDEX] | [SYMBOL]                   | [TYPE_FAMILY]  | [DESCRIPTION]                                           |
| :-----: | :------------------------- | :------------- | :------------------------------------------------------ |
|  [01]   | `PWAAssetsIcons`           | icon catalog   | favicon/transparent/maskable/apple/splashscreen buckets |
|  [02]   | `PWAAssetsGenerator`       | generator API  | asset generation and HTML injection                     |
|  [03]   | `PWAHtmlLink`              | HTML link      | `<link>` element descriptor                             |
|  [04]   | `PWAHtmlAssets`            | HTML assets    | collected links and theme-color                         |
|  [05]   | `ResolvedIconAsset`        | resolved asset | path, mimeType, buffer, age                             |
|  [06]   | `ResolvedPWAAssetsOptions` | resolved opts  | resolved PWA assets configuration                       |
|  [07]   | `ColorSchemeMeta`          | meta element   | `name`/`content` meta descriptor                        |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: plugin factory
- rail: pwa

| [INDEX] | [SURFACE]                          | [ENTRY_FAMILY] | [DESCRIPTION]                         |
| :-----: | :--------------------------------- | :------------- | :------------------------------------ |
|  [01]   | `VitePWA(userOptions?)`            | plugin factory | returns `Plugin[]` for Vite `plugins` |
|  [02]   | `cachePreset`                      | constant       | default Workbox runtime cache preset  |
|  [03]   | `defaultInjectManifestVitePlugins` | constant       | default Vite plugins for SW build     |

[ENTRYPOINT_SCOPE]: VitePWAOptions key fields
- rail: pwa

| [INDEX] | [FIELD]                | [TYPE]                                                      | [DEFAULT]      |
| :-----: | :--------------------- | :---------------------------------------------------------- | :------------- |
|  [01]   | `strategies`           | `'generateSW' \| 'injectManifest'`                          | `'generateSW'` |
|  [02]   | `registerType`         | `'prompt' \| 'autoUpdate' \| 'legacy'`                      | `'prompt'`     |
|  [03]   | `injectRegister`       | `'auto' \| 'inline' \| 'script' \| 'script-defer' \| false` | `'auto'`       |
|  [04]   | `manifest`             | `Partial<ManifestOptions> \| false`                         | `{}`           |
|  [05]   | `workbox`              | `Partial<GenerateSWOptions>`                                | `{}`           |
|  [06]   | `injectManifest`       | `Partial<CustomInjectManifestOptions>`                      | `{}`           |
|  [07]   | `devOptions`           | `DevOptions`                                                | `{}`           |
|  [08]   | `pwaAssets`            | `PWAAssetsOptions`                                          | —              |
|  [09]   | `filename`             | `string`                                                    | `'sw.js'`      |
|  [10]   | `scope`                | `string`                                                    | Vite `base`    |
|  [11]   | `base`                 | `string`                                                    | Vite `base`    |
|  [12]   | `includeAssets`        | `string \| string[] \| RegExp`                              | —              |
|  [13]   | `includeManifestIcons` | `boolean`                                                   | `true`         |
|  [14]   | `integration`          | `PWAIntegration`                                            | —              |

## [04]-[IMPLEMENTATION_LAW]

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

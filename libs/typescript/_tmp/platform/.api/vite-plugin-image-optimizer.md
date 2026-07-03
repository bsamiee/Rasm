# [API_CATALOGUE] vite-plugin-image-optimizer

`vite-plugin-image-optimizer` is a build-emit-phase Vite plugin that compresses every bundled raster asset through `sharp` and every SVG through `svgo`, keyed by a single format→engine-options dispatch: one `Options` bag whose seven `sharp`-format keys (`png`/`jpeg`/`jpg`/`tiff`/`gif`/`webp`/`avif`) and one `svg` key are instances of ONE per-format options mechanism, not independent knobs. The named export `ViteImageOptimizer(options?)` returns a single `Plugin` for the `Shell/build.md` `BuildPipeline` plugin set. It runs at Vite build time only, never in the `platform` `Layer` graph: it crosses no `Effect.tryPromise`, holds no service tag, and its content-hash `cache` is an internal build cache distinct from the `interchange` `XxHash128` runtime content key.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `vite-plugin-image-optimizer`
- package: `vite-plugin-image-optimizer`
- version: `2.0.3`
- license: `MIT`
- module: dual ESM/CJS via `exports` map (`import` → `dist/index.js`, `require` → `dist/index.cjs`), types `dist/index.d.ts`; `"type": "module"`; one named export `ViteImageOptimizer` (no default). Deps `ansi-colors`, `pathe`
- asset: Vite plugin (returns `Plugin`). Peer floor is a hard ABI gate — `vite >=5`, `sharp >=0.34.0`, `svgo >=4` — the two engines are PEER, not bundled, so the consumer manifest owns their versions; the sharp `*Options` and svgo `Config` types the option keys accept resolve from those peers
- rail: asset-optimize — build-time raster+SVG compression, content-hash cached
- catalog-verdict: KEEP; the build-emit image-optimization pass admitted at `Shell/build.md`, one plugin row in the `BuildPipeline` set

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the one options bag
- rail: asset-optimize

| [INDEX] | [SYMBOL]  | [TYPE_FAMILY]  | [BOUNDARY_NOTE]                                                                              |
| :-----: | :-------- | :------------- | :------------------------------------------------------------------------------------------- |
|  [01]   | `Options` | options object | the whole config; the only exported type — every field below is one property of this one bag |

[PUBLIC_TYPE_SCOPE]: selection + reporting + cache fields
- rail: asset-optimize

| [INDEX] | [FIELD]         | [TYPE]                         | [DEFAULT] | [BOUNDARY_NOTE]                                                    |
| :-----: | :-------------- | :----------------------------- | :-------- | :---------------------------------------------------------------- |
|  [01]   | `test`          | `RegExp`                       | —         | asset-path match gate; only paths matching `test` reach an engine |
|  [02]   | `include`       | `RegExp \| string \| string[]` | —         | force-include beyond `test`                                       |
|  [03]   | `exclude`       | `RegExp \| string \| string[]` | —         | force-exclude                                                     |
|  [04]   | `includePublic` | `boolean`                      | —         | extend optimization into Vite `publicDir` (copied, not bundled)   |
|  [05]   | `ansiColors`    | `boolean`                      | —         | colorize the log output                                           |
|  [06]   | `logStats`      | `boolean`                      | —         | print per-file compression ratios to the terminal                 |
|  [07]   | `cache`         | `boolean`                      | —         | persist optimized outputs keyed by content hash across builds     |
|  [08]   | `cacheLocation` | `string`                       | —         | cache directory path; the CI-cacheable directory when `cache`     |

[PUBLIC_TYPE_SCOPE]: the format→engine-options axis — one mechanism, one row per output format
- rail: asset-optimize
- Each key routes files of its format to the named engine with the given options; the SVG key targets `svgo`, every raster key targets the matching `sharp` output codec. A new format is one row carrying the peer engine's option type, never a new plugin.

| [INDEX] | [FIELD] | [ENGINE] | [OPTION_TYPE]         | [BOUNDARY_NOTE]                                    |
| :-----: | :------ | :------- | :-------------------- | :------------------------------------------------- |
|  [01]   | `svg`   | `svgo`   | `svgo` `Config`       | full svgo plugin/preset config for SVG minification |
|  [02]   | `png`   | `sharp`  | `sharp` `PngOptions`  | palette, compression level, quality                |
|  [03]   | `jpeg`  | `sharp`  | `sharp` `JpegOptions` | quality, progressive, mozjpeg                      |
|  [04]   | `jpg`   | `sharp`  | `sharp` `JpegOptions` | `.jpg` alias of the JPEG codec                     |
|  [05]   | `tiff`  | `sharp`  | `sharp` `TiffOptions` | quality, compression                               |
|  [06]   | `gif`   | `sharp`  | `sharp` `GifOptions`  | colors, effort, dither                             |
|  [07]   | `webp`  | `sharp`  | `sharp` `WebpOptions` | quality, lossless, effort                          |
|  [08]   | `avif`  | `sharp`  | `sharp` `AvifOptions` | quality, effort, chroma subsampling                |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: plugin factory
- rail: asset-optimize

| [INDEX] | [SURFACE]                      | [ENTRY_FAMILY] | [BOUNDARY_NOTE]                                              |
| :-----: | :----------------------------- | :------------- | :---------------------------------------------------------- |
|  [01]   | `ViteImageOptimizer(options?)` | plugin factory | named export; `(optionsParam?: Options) => Plugin`, one row |

```ts contract
// vite-plugin-image-optimizer
import { Plugin } from 'vite'
import { PngOptions, JpegOptions, TiffOptions, GifOptions, WebpOptions, AvifOptions } from 'sharp'
import { Config as SVGOConfig } from 'svgo'

interface Options {
  test?: RegExp
  include?: RegExp | string | string[]
  exclude?: RegExp | string | string[]
  includePublic?: boolean
  ansiColors?: boolean
  logStats?: boolean
  cache?: boolean
  cacheLocation?: string
  svg?: SVGOConfig
  png?: PngOptions
  jpeg?: JpegOptions
  jpg?: JpegOptions
  tiff?: TiffOptions
  gif?: GifOptions
  webp?: WebpOptions
  avif?: AvifOptions
}
declare function ViteImageOptimizer(optionsParam?: Options): Plugin
export { ViteImageOptimizer }
```

## [04]-[IMPLEMENTATION_LAW]

[OPTIMIZE_TOPOLOGY]:
- Files matching `test`/`include` (minus `exclude`) route by extension to `sharp` (raster) or `svgo` (SVG); the per-format key supplies engine options, an absent key uses engine defaults.
- `cache: true` keys optimized outputs by content hash under `cacheLocation`, so an unchanged asset skips re-compression across builds — the CI-cacheable artifact directory.
- `includePublic` extends optimization to `publicDir` assets that Vite copies verbatim rather than graph-bundles.
- `logStats: true` surfaces per-file before/after ratios; a build-diagnostic signal, not build-critical.

[INTEGRATION_LAW]:
- Stack with `vite.md`: one `PluginOption` row in `vite.md`'s `UserConfig.plugins` host, running at Vite build emit (`apply:"build"`). The canonical `Shell/build.md` order the sibling catalogs assert — `plugins: [tailwindcss(), vitePluginSvgr(), react(), ViteImageOptimizer({...}), VitePWA({...}), viteCompression()]` — resolves `tailwindcss()`/`vitePluginSvgr()` (`enforce: 'pre'`) ahead of the normal-band transforms and lands `viteCompression()` LAST so its `closeBundle` companions cover the final `vite-plugin-pwa` precache emit. Ordering against `vite-plugin-svgr` is contention-free by construction — svgr (`enforce: 'pre'`) consumes only `*.svg?react` query imports as JS component modules, while `ViteImageOptimizer` optimizes static `.svg`/raster assets in the emit phase, so the two partition the SVG space by import shape, never double-process one file.
- Runs BEFORE `vite-plugin-compression`: optimize raster/SVG bytes first, then let compression emit `.gz`/`.br` companions over the already-minimized output; reversing the order compresses un-optimized bytes.
- The `sharp`/`svgo` peers are the engine floor the design composes against by option TYPE only — the plugin owns the invocation, so no `Shell/build.md` code calls `sharp(...)` or `optimize(...)` directly; a raw engine call for build-time asset optimization is the bypass defect.
- Build-time only, never an `effect` rail: it never enters the `platform` `Layer` graph, never lifts through `Effect.tryPromise`, and its content-hash `cache` is a private build cache — distinct from the `interchange` `XxHash128` runtime content key that addresses decoded payloads, never conflated with it.

[RAIL_LAW]:
- Package: `vite-plugin-image-optimizer`
- Owns: build-emit raster compression via `sharp` and SVG minification via `svgo`, with content-hash caching
- Accept: `Options` passed to `ViteImageOptimizer()`; per-format engine options under the matching key; `includePublic` for `publicDir`
- Reject: direct `sharp`/`svgo` calls for build-time asset optimization while this plugin is active; running it after `vite-plugin-compression`; treating its `cache` as the runtime content key

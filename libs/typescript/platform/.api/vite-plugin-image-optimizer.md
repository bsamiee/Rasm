# [API_CATALOGUE] vite-plugin-image-optimizer

`vite-plugin-image-optimizer` compresses raster images (PNG, JPEG, TIFF, GIF, WebP, AVIF) via `sharp` and optimizes SVG files via `svgo` during the Vite build. The named export `ViteImageOptimizer(options?)` returns a `Plugin`.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `vite-plugin-image-optimizer`
- package: `vite-plugin-image-optimizer`
- module: `vite-plugin-image-optimizer`
- asset: Vite plugin (returns `Plugin`)
- rail: asset-optimize

## [2]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: options family
- rail: asset-optimize

| [INDEX] | [SYMBOL]  | [TYPE_FAMILY]  | [DESCRIPTION]             |
| :-----: | :-------- | :------------- | :------------------------ |
|   [1]   | `Options` | options object | full plugin configuration |

[PUBLIC_TYPE_SCOPE]: Options fields
- rail: asset-optimize

| [INDEX] | [FIELD]         | [TYPE]                         | [DESCRIPTION]                                |
| :-----: | :-------------- | :----------------------------- | :------------------------------------------- |
|   [1]   | `test`          | `RegExp`                       | regex to match asset file paths              |
|   [2]   | `include`       | `RegExp \| string \| string[]` | files to include                             |
|   [3]   | `exclude`       | `RegExp \| string \| string[]` | files to exclude                             |
|   [4]   | `includePublic` | `boolean`                      | optimize assets in `public/` directory       |
|   [5]   | `ansiColors`    | `boolean`                      | use ANSI colors in log output                |
|   [6]   | `logStats`      | `boolean`                      | print compression stats to terminal          |
|   [7]   | `svg`           | `SVGOConfig`                   | svgo options for SVG optimization            |
|   [8]   | `png`           | `PngOptions`                   | sharp options for PNG files                  |
|   [9]   | `jpeg`          | `JpegOptions`                  | sharp options for JPEG files                 |
|  [10]   | `jpg`           | `JpegOptions`                  | sharp options for JPG files                  |
|  [11]   | `tiff`          | `TiffOptions`                  | sharp options for TIFF files                 |
|  [12]   | `gif`           | `GifOptions`                   | sharp options for GIF files                  |
|  [13]   | `webp`          | `WebpOptions`                  | sharp options for WebP files                 |
|  [14]   | `avif`          | `AvifOptions`                  | sharp options for AVIF files                 |
|  [15]   | `cache`         | `boolean`                      | persist optimized images in local file cache |
|  [16]   | `cacheLocation` | `string`                       | path to the cache directory                  |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: plugin factory
- rail: asset-optimize

| [INDEX] | [SURFACE]                      | [ENTRY_FAMILY] | [DESCRIPTION]                       |
| :-----: | :----------------------------- | :------------- | :---------------------------------- |
|   [1]   | `ViteImageOptimizer(options?)` | plugin factory | named export; returns Vite `Plugin` |

## [4]-[IMPLEMENTATION_LAW]

[IMAGE_TOPOLOGY]:
- raster formats (PNG, JPEG, TIFF, GIF, WebP, AVIF) are processed by `sharp`; SVG files by `svgo`
- `cache` persists optimized outputs keyed by content hash to avoid redundant re-compression across builds
- `includePublic` extends optimization to assets in the Vite `publicDir`

[LOCAL_ADMISSION]:
- Pass format-specific `sharp` options under the matching key (`png`, `jpeg`, `webp`, etc.) for fine-grained quality control.
- Set `logStats: true` to surface per-file compression ratios during CI builds.
- `cacheLocation` defaults to an internal path; override when CI caching the directory.

[RAIL_LAW]:
- Package: `vite-plugin-image-optimizer`
- Owns: raster and SVG compression at Vite build time
- Accept: `Options` passed to `ViteImageOptimizer()`
- Reject: manual `sharp`/`svgo` invocations for build-time asset optimization when this plugin is active

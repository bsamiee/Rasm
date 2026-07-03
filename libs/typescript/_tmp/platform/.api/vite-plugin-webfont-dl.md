# [API_CATALOGUE] vite-plugin-webfont-dl

`vite-plugin-webfont-dl` downloads Google Fonts (and any remote `@font-face` CSS) at build time, self-hosts the resulting font files as local build assets, and injects the CSS into the HTML output; in dev it proxies font requests through a local middleware. Internally it composes `axios` (CSS + font fetch, and the `proxy` config type), `clean-css` (the `minifyCss` pass), and `flat-cache` (the `cache` persistence). The single factory is exported under five aliases — `viteWebfontDownload` (canonical), `ViteWebfontDownload`, `viteWebfontDl`, `webfontDl`, `webfontDownload` — and returns one Vite `Plugin`.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `vite-plugin-webfont-dl`
- package: `vite-plugin-webfont-dl`
- module: `vite-plugin-webfont-dl` (`dist/index.d.ts`)
- asset: Vite plugin (returns `Plugin`)
- rail: asset-optimize

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: options family
- rail: asset-optimize

| [INDEX] | [SYMBOL]  | [TYPE_FAMILY]  | [DESCRIPTION]                                     |
| :-----: | :-------- | :------------- | :------------------------------------------------ |
|  [01]   | `Options` | options object | full plugin configuration; the sole exported type |

[PUBLIC_TYPE_SCOPE]: `Options` fields
- rail: asset-optimize

| [INDEX] | [FIELD]            | [TYPE]                      | [DESCRIPTION]                                                          |
| :-----: | :----------------- | :-------------------------- | :--------------------------------------------------------------------- |
|  [01]   | `injectAsStyleTag` | `boolean`                   | inject critical CSS as an inline `<style>` tag (default `true`)        |
|  [02]   | `minifyCss`        | `boolean`                   | minify the emitted CSS via `clean-css` (default: Vite `build.minify`)  |
|  [03]   | `embedFonts`       | `boolean`                   | embed base64 fonts into CSS (default `false`); inflates CSS on repeated font refs |
|  [04]   | `async`            | `boolean`                   | load the stylesheet non-render-blocking via `media="print"` (default `true`); applies ONLY when `injectAsStyleTag` is `false` |
|  [05]   | `cache`            | `boolean`                   | persist downloaded CSS/fonts in a local `flat-cache` store (default `true`); `false` deletes the existing cache |
|  [06]   | `proxy`            | `false \| AxiosProxyConfig` | axios proxy config for build-time network requests (default `false`)   |
|  [07]   | `assetsSubfolder`  | `string`                    | subfolder within the assets dir for the self-hosted font files (default `''`) |
|  [08]   | `throwError`       | `boolean`                   | throw and fail the build on download error vs warn-and-continue (default `false`) |
|  [09]   | `subsetsAllowed`   | `string[]`                  | restrict downloaded fonts to the listed Unicode subsets, e.g. `['latin','latin-ext']` (default `[]` = all) |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: plugin factory
- rail: asset-optimize

| [INDEX] | [SURFACE]                                     | [ENTRY_FAMILY] | [DESCRIPTION]                                                     |
| :-----: | :-------------------------------------------- | :------------- | :--------------------------------------------------------------- |
|  [01]   | `viteWebfontDownload(webfontUrls?, options?)` | plugin factory | canonical default export; returns one Vite `Plugin`              |
|  [02]   | `ViteWebfontDownload(webfontUrls?, options?)` | alias          | same factory, PascalCase export                                  |
|  [03]   | `viteWebfontDl(webfontUrls?, options?)`       | alias          | short alias                                                     |
|  [04]   | `webfontDl(webfontUrls?, options?)`           | alias          | shorter alias                                                   |
|  [05]   | `webfontDownload(webfontUrls?, options?)`     | alias          | descriptive alias                                              |

```ts contract
// vite-plugin-webfont-dl (dist/index.d.ts)
import { Plugin } from 'vite'
import { AxiosProxyConfig } from 'axios'

interface Options {
  injectAsStyleTag?: boolean
  minifyCss?: boolean
  embedFonts?: boolean
  async?: boolean                       // only applies when injectAsStyleTag is false
  cache?: boolean
  proxy?: false | AxiosProxyConfig      // { host; port; auth?; protocol? }
  assetsSubfolder?: string
  throwError?: boolean
  subsetsAllowed?: string[]
}

declare function viteWebfontDownload(webfontUrls?: string | string[], options?: Options): Plugin
export {
  viteWebfontDownload as default, viteWebfontDownload,
  viteWebfontDownload as ViteWebfontDownload, viteWebfontDownload as viteWebfontDl,
  viteWebfontDownload as webfontDl, viteWebfontDownload as webfontDownload,
}
```

## [04]-[IMPLEMENTATION_LAW]

[WEBFONT_TOPOLOGY]:
- `webfontUrls` accepts a single URL string or an array; when omitted the plugin scans HTML `<link>` tags for remote font stylesheets.
- build path: `configResolved` → `transformIndexHtml` (collect + rewrite `<link>` tags) → `generateBundle` (download CSS via `axios`, parse font definitions, download fonts, rewrite URLs to local assets, format, `emitFile`, inject).
- dev path: `configResolved` → `configureServer` (middleware at `@webfonts/webfonts.css` for CSS; a per-font middleware for binary assets) → `transformIndexHtml` (strip remote tags, inject local reference).
- `injectAsStyleTag: true` inlines the CSS as a `<style>` element (render-blocking but zero extra request); `injectAsStyleTag: false` emits a linked stylesheet where `async: true` makes it non-render-blocking via the `media="print"` swap.

[LOCAL_ADMISSION]:
- `Shell/build`'s `BuildPipeline` adds one `viteWebfontDownload(urls, opts)` row to the `plugins` array; provide explicit `webfontUrls` when the `<link>` tags are injected dynamically and absent from source HTML.
- The self-hosted `.woff2` assets land under the assets dir, so `vite-plugin-compression` precompresses them and the `Shell/serviceworker` `cache-first` static route (`/\.(?:js|css|woff2|png|svg)$/`) precaches them — self-hosting removes the third-party font-CDN origin from the runtime request graph.
- `injectAsStyleTag: true` emits an inline `<style>`, so `vite-plugin-csp`'s `style-src` must admit the inline hash/nonce; `injectAsStyleTag: false` keeps CSP `style-src` on `'self'`.
- `subsetsAllowed` trims the downloaded font files to the runtime script coverage the `browserslist` target implies, cutting precache and transfer size.
- Set `throwError: true` in CI so a font-fetch failure fails the build rather than silently shipping a fallback; `proxy` passes an `AxiosProxyConfig` when the build host reaches fonts through a corporate proxy.

[RAIL_LAW]:
- Package: `vite-plugin-webfont-dl`
- Owns: remote webfont download, self-hosting, HTML injection, and the dev font-proxy middleware
- Accept: optional `string | string[]` URL list and `Options` passed to `viteWebfontDownload()` as one `plugins` row
- Reject: manual font-download scripts, a runtime third-party font-CDN `<link>`, or a hand-rolled font-proxy when this plugin owns the pipeline

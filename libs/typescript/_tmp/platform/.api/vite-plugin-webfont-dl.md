# [API_CATALOGUE] vite-plugin-webfont-dl

`vite-plugin-webfont-dl` downloads Google Fonts (and any remote `@font-face` CSS) at build time, self-hosts the resulting font files as local assets, and injects the CSS into the HTML output. In dev mode it proxies font requests through a local middleware. The function is exported under five aliases: `viteWebfontDownload` (canonical), `ViteWebfontDownload`, `viteWebfontDl`, `webfontDl`, and `webfontDownload`.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `vite-plugin-webfont-dl`
- package: `vite-plugin-webfont-dl`
- module: `vite-plugin-webfont-dl`
- asset: Vite plugin (returns `Plugin`)
- rail: asset-optimize

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: options family
- rail: asset-optimize

| [INDEX] | [SYMBOL]  | [TYPE_FAMILY]  | [DESCRIPTION]             |
| :-----: | :-------- | :------------- | :------------------------ |
|  [01]   | `Options` | options object | full plugin configuration |

[PUBLIC_TYPE_SCOPE]: Options fields
- rail: asset-optimize

| [INDEX] | [FIELD]            | [TYPE]                      | [DESCRIPTION]                                                |
| :-----: | :----------------- | :-------------------------- | :----------------------------------------------------------- |
|  [01]   | `injectAsStyleTag` | `boolean`                   | inject CSS as `<style>` tag (default `true`)                 |
|  [02]   | `minifyCss`        | `boolean`                   | minify CSS during build (default: Vite `build.minify`)       |
|  [03]   | `embedFonts`       | `boolean`                   | embed base64 fonts into CSS (default `false`)                |
|  [04]   | `async`            | `boolean`                   | load stylesheet async via `media="print"` (default `true`)   |
|  [05]   | `cache`            | `boolean`                   | persist downloaded CSS/fonts in local cache (default `true`) |
|  [06]   | `proxy`            | `false \| AxiosProxyConfig` | proxy config for network requests (default `false`)          |
|  [07]   | `assetsSubfolder`  | `string`                    | subfolder within assets dir for font files (default `''`)    |
|  [08]   | `throwError`       | `boolean`                   | throw on download failure vs warn (default `false`)          |
|  [09]   | `subsetsAllowed`   | `string[]`                  | restrict to specified Unicode subsets (default `[]`)         |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: plugin factory
- rail: asset-optimize

| [INDEX] | [SURFACE]                                     | [ENTRY_FAMILY] | [DESCRIPTION]                         |
| :-----: | :-------------------------------------------- | :------------- | :------------------------------------ |
|  [01]   | `viteWebfontDownload(webfontUrls?, options?)` | plugin factory | default export; returns Vite `Plugin` |
|  [02]   | `ViteWebfontDownload(webfontUrls?, options?)` | alias          | same factory, PascalCase export       |
|  [03]   | `viteWebfontDl(webfontUrls?, options?)`       | alias          | short alias                           |
|  [04]   | `webfontDl(webfontUrls?, options?)`           | alias          | shorter alias                         |
|  [05]   | `webfontDownload(webfontUrls?, options?)`     | alias          | descriptive alias                     |

## [04]-[IMPLEMENTATION_LAW]

[WEBFONT_TOPOLOGY]:
- `webfontUrls` accepts a single URL string or an array; when omitted, the plugin scans HTML `<link>` tags for remote font stylesheets
- build path: `configResolved` → `transformIndexHtml` → `generateBundle` (download CSS, parse font definitions, download fonts, replace URLs, format, save, inject)
- dev path: `configResolved` → `configureServer` (middleware at `@webfonts/webfonts.css` for CSS; per-font middleware for binary assets)
- `async: true` (default) loads the stylesheet non-render-blocking; `injectAsStyleTag: true` inlines CSS as a `<style>` element

[LOCAL_ADMISSION]:
- Provide explicit `webfontUrls` when the font `<link>` tags are injected dynamically and not present in the source HTML.
- Set `throwError: true` in CI to fail the build on font fetch errors rather than silently continuing.
- `subsetsAllowed` restricts downloaded font files to the listed Unicode ranges, reducing asset size.

[RAIL_LAW]:
- Package: `vite-plugin-webfont-dl`
- Owns: remote webfont download, self-hosting, and HTML injection at build and dev time
- Accept: optional `string | string[]` URL list and `Options` passed to `viteWebfontDownload()`
- Reject: manual font download scripts when this plugin is active in the Vite pipeline

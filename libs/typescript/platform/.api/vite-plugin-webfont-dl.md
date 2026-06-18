# [API_CATALOGUE] vite-plugin-webfont-dl

`vite-plugin-webfont-dl` downloads Google Fonts (and any remote `@font-face` CSS) at build time, self-hosts the resulting font files as local assets, and injects the CSS into the HTML output. In dev mode it proxies font requests through a local middleware. The function is exported under five aliases: `viteWebfontDownload` (canonical), `ViteWebfontDownload`, `viteWebfontDl`, `webfontDl`, and `webfontDownload`.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `vite-plugin-webfont-dl`
- package: `vite-plugin-webfont-dl`
- module: `vite-plugin-webfont-dl`
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

| [INDEX] | [FIELD]            | [TYPE]                      | [DESCRIPTION]                                                |
| :-----: | :----------------- | :-------------------------- | :----------------------------------------------------------- |
|   [1]   | `injectAsStyleTag` | `boolean`                   | inject CSS as `<style>` tag (default `true`)                 |
|   [2]   | `minifyCss`        | `boolean`                   | minify CSS during build (default: Vite `build.minify`)       |
|   [3]   | `embedFonts`       | `boolean`                   | embed base64 fonts into CSS (default `false`)                |
|   [4]   | `async`            | `boolean`                   | load stylesheet async via `media="print"` (default `true`)   |
|   [5]   | `cache`            | `boolean`                   | persist downloaded CSS/fonts in local cache (default `true`) |
|   [6]   | `proxy`            | `false \| AxiosProxyConfig` | proxy config for network requests (default `false`)          |
|   [7]   | `assetsSubfolder`  | `string`                    | subfolder within assets dir for font files (default `''`)    |
|   [8]   | `throwError`       | `boolean`                   | throw on download failure vs warn (default `false`)          |
|   [9]   | `subsetsAllowed`   | `string[]`                  | restrict to specified Unicode subsets (default `[]`)         |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: plugin factory
- rail: asset-optimize

| [INDEX] | [SURFACE]                                     | [ENTRY_FAMILY] | [DESCRIPTION]                         |
| :-----: | :-------------------------------------------- | :------------- | :------------------------------------ |
|   [1]   | `viteWebfontDownload(webfontUrls?, options?)` | plugin factory | default export; returns Vite `Plugin` |
|   [2]   | `ViteWebfontDownload(webfontUrls?, options?)` | alias          | same factory, PascalCase export       |
|   [3]   | `viteWebfontDl(webfontUrls?, options?)`       | alias          | short alias                           |
|   [4]   | `webfontDl(webfontUrls?, options?)`           | alias          | shorter alias                         |
|   [5]   | `webfontDownload(webfontUrls?, options?)`     | alias          | descriptive alias                     |

## [4]-[IMPLEMENTATION_LAW]

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

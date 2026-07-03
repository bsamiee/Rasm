# [API_CATALOGUE] vite-plugin-compression

`vite-plugin-compression` pre-compresses Vite build output with Node's `zlib` (gzip/deflate/deflateRaw) or brotli, emitting `.gz`/`.br` companion files at `closeBundle`. The default export is the ONLY export — `Algorithm`, `CompressionOptions`, and `VitePluginCompression` are declared but not exported, so the option shape is reached structurally, never imported by name. `algorithm` is a four-member parameter axis, not four plugins: dual gzip+brotli emit for CDN content-negotiation is the same factory registered twice with different `algorithm`/`ext` rows. It runs `apply:"build"` only, sequenced after `vite-plugin-csp`'s HTML mutation so the compressed bytes match the final assets, and its `filter`/`threshold` scope it to text assets — the image/font siblings own their own already-compressed output.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `vite-plugin-compression`
- package: `vite-plugin-compression`
- version: `0.5.1`
- license: `MIT`
- module: dual `exports["."]` = `{ require: ./dist/index.cjs, import: ./dist/index.mjs, types: ./dist/index.d.ts }`; the default factory is the sole export (`export { default }`)
- asset: build-time Vite plugin returning a single `Plugin` — deps `chalk@^4` (verbose-log color), `debug@^4`, `fs-extra@^10` (companion write); peer dep `vite >=2.0.0`; no `engines` floor. `apply:"build"` + the `closeBundle` hook, so it never runs in dev and emits after the bundle is written
- rail: build/asset — zlib/brotli pre-compression of build output

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: option shape — structural, reached via the factory parameter
- rail: build/asset
- The default factory is the only importable symbol; `VitePluginCompression`, `Algorithm`, and `CompressionOptions` are internal `declare`s, reachable as `Parameters<typeof import("vite-plugin-compression").default>[0]`, never `import { Algorithm }`.

| [INDEX] | [SYMBOL]                | [TYPE_FAMILY]  | [BOUNDARY_NOTE]                                                          |
| :-----: | :---------------------- | :------------- | :----------------------------------------------------------------------- |
|  [01]   | `VitePluginCompression` | options object | the factory parameter shape (structural, not exported)                   |
|  [02]   | `Algorithm`             | string union   | `'gzip' \| 'brotliCompress' \| 'deflate' \| 'deflateRaw'` — the Node `zlib` method-name axis |
|  [03]   | `CompressionOptions`    | type alias     | `Partial<ZlibOptions> \| Partial<BrotliOptions>` — Node `zlib` option pass-through |

[PUBLIC_TYPE_SCOPE]: option fields and defaults
- rail: build/asset

| [INDEX] | [FIELD]              | [TYPE]                                  | [DEFAULT]                          |
| :-----: | :------------------- | :-------------------------------------- | :--------------------------------- |
|  [01]   | `verbose`            | `boolean`                               | `true` (logs ratios via `chalk`)   |
|  [02]   | `threshold`          | `number`                                | `1025` (bytes; smaller files skip) |
|  [03]   | `filter`             | `RegExp \| ((file: string) => boolean)` | `/\.(js\|mjs\|json\|css\|html)$/i`  |
|  [04]   | `disable`            | `boolean`                               | `false`                            |
|  [05]   | `algorithm`          | `Algorithm`                             | `'gzip'`                           |
|  [06]   | `ext`                | `string`                                | `'.gz'`                            |
|  [07]   | `compressionOptions` | `CompressionOptions`                    | — (passed to the `zlib` create fn) |
|  [08]   | `deleteOriginFile`   | `boolean`                               | `false`                            |
|  [09]   | `success`            | `() => void`                            | — (post-completion notification)   |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: plugin factory — the single export
- rail: build/asset

```ts
// dist/index.d.ts — only the default is exported
import { Plugin } from 'vite';
import { ZlibOptions, BrotliOptions } from 'zlib';
declare type Algorithm = 'gzip' | 'brotliCompress' | 'deflate' | 'deflateRaw';
declare type CompressionOptions = Partial<ZlibOptions> | Partial<BrotliOptions>;
declare function export_default(options?: VitePluginCompression): Plugin; // apply:"build", closeBundle
export { export_default as default };
```

## [04]-[IMPLEMENTATION_LAW]

[COMPRESSION_TOPOLOGY]:
- only files whose size exceeds `threshold` bytes AND whose path matches `filter` are compressed; the pass runs once at `closeBundle` (build-only, post-write).
- `algorithm: 'brotliCompress'` requires an explicit `ext: '.br'`; `compressionOptions` passes directly to `zlib.createGzip`/`createBrotliCompress` — brotli quality via `{ params: { [zlib.constants.BROTLI_PARAM_QUALITY]: 11 } }`.
- `deleteOriginFile: true` removes the uncompressed source after emit — only when the CDN serves exclusively pre-compressed responses with no uncompressed fallback.
- `success` fires after all files are compressed; use it for notification only, never build-critical logic.

[INTEGRATION_LAW]:
- Stack with `vite.md`: registered in `UserConfig.plugins`; `apply:"build"` + `closeBundle` make it build-only, running after the bundle is written and emitting companions in place.
- DUAL-ENCODING PARAMETERIZATION (the collapse): `algorithm` is the parameter, not a plugin identity — emit BOTH gzip and brotli by registering the SAME default factory twice, `{ algorithm: 'gzip', ext: '.gz' }` and `{ algorithm: 'brotliCompress', ext: '.br' }`, so the CDN negotiates `Content-Encoding` per request. One plugin, N algorithm rows, never a parallel compression owner.
- Ordering law within `Shell/build.md` `BuildPipeline`: compression must sequence AFTER `vite-plugin-csp` (which mutates HTML at `transformIndexHtml`/`enforce:"post"` by injecting inline-content hashes) and after the `vite-plugin-pwa` precache emit, so the `.gz`/`.br` cover the FINAL asset bytes; the `closeBundle` hook naturally lands after `transformIndexHtml`, keeping the companions consistent.
- Filter boundary with the sibling asset plugins: the default `filter` (js/mjs/json/css/html) scopes to text; already-compressed binary output is the `vite-plugin-image-optimizer` (images) and `vite-plugin-webfont-dl` (woff2 is already-compressed) concern — extend `filter` to add `.svg`/`.wasm`, never double-compress a png/woff2.
- No Effect stacking: a build-time host adapter; the `Content-Encoding: gzip|br` reverse-proxy/CDN serving contract is the runtime consumer, outside the plugin.

[RAIL_LAW]:
- Package: `vite-plugin-compression`
- Owns: zlib/brotli pre-compression of Vite build output into `.gz`/`.br` companion assets at `closeBundle`
- Accept: the option shape to the default factory; `algorithm` as the parameter axis (register twice for dual gzip+brotli emit); `filter`/`threshold` scoping to text assets
- Reject: shell-level gzip post-processing when this plugin owns the pass; a parallel plugin per algorithm instead of the `algorithm`-row pattern; double-compressing image/font output owned by the sibling plugins; importing `Algorithm`/`CompressionOptions`/`VitePluginCompression` as named symbols (the default is the only export)

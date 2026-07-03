# [API_CATALOGUE] vite-plugin-compression

`vite-plugin-compression` compresses Vite build output using gzip, brotli, deflate, or deflateRaw via Node's built-in `zlib`, emitting `.gz` (or custom extension) companion files alongside the originals. The default export is the plugin factory function.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `vite-plugin-compression`
- package: `vite-plugin-compression`
- module: `vite-plugin-compression`
- asset: Vite plugin (returns `Plugin`)
- rail: asset-optimize

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: options and algorithm family
- rail: asset-optimize

| [INDEX] | [SYMBOL]                | [TYPE_FAMILY]  | [DESCRIPTION]                                             |
| :-----: | :---------------------- | :------------- | :-------------------------------------------------------- |
|  [01]   | `VitePluginCompression` | options object | full plugin configuration                                 |
|  [02]   | `Algorithm`             | string union   | `'gzip' \| 'brotliCompress' \| 'deflate' \| 'deflateRaw'` |
|  [03]   | `CompressionOptions`    | type alias     | `Partial<ZlibOptions> \| Partial<BrotliOptions>`          |

[PUBLIC_TYPE_SCOPE]: VitePluginCompression fields
- rail: asset-optimize

| [INDEX] | [FIELD]              | [TYPE]                                  | [DEFAULT]                          |
| :-----: | :------------------- | :-------------------------------------- | :--------------------------------- |
|  [01]   | `verbose`            | `boolean`                               | `true`                             |
|  [02]   | `threshold`          | `number`                                | `1025`                             |
|  [03]   | `filter`             | `RegExp \| ((file: string) => boolean)` | `/\.(js\|mjs\|json\|css\|html)$/i` |
|  [04]   | `disable`            | `boolean`                               | `false`                            |
|  [05]   | `algorithm`          | `Algorithm`                             | `'gzip'`                           |
|  [06]   | `ext`                | `string`                                | `'.gz'`                            |
|  [07]   | `compressionOptions` | `CompressionOptions`                    | —                                  |
|  [08]   | `deleteOriginFile`   | `boolean`                               | `false`                            |
|  [09]   | `success`            | `() => void`                            | —                                  |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: plugin factory
- rail: asset-optimize

| [INDEX] | [SURFACE]           | [ENTRY_FAMILY] | [DESCRIPTION]                         |
| :-----: | :------------------ | :------------- | :------------------------------------ |
|  [01]   | `default(options?)` | plugin factory | default export; returns Vite `Plugin` |

## [04]-[IMPLEMENTATION_LAW]

[COMPRESSION_TOPOLOGY]:
- only files whose size exceeds `threshold` bytes and whose path matches `filter` are compressed
- `algorithm: 'brotliCompress'` emits `.br` files by default; set `ext` explicitly to override
- `deleteOriginFile: true` removes the uncompressed source after emitting the compressed variant
- `compressionOptions` passes directly to `zlib.createGzip` / `zlib.createBrotliCompress` etc.

[LOCAL_ADMISSION]:
- Use `algorithm: 'brotliCompress'` for better compression ratios when the CDN or reverse proxy serves pre-compressed responses.
- Leave `verbose: true` during initial setup to verify compression ratios; disable in CI logs.
- The `success` callback fires after all files are compressed; use it for post-build notifications only, not build-critical logic.

[RAIL_LAW]:
- Package: `vite-plugin-compression`
- Owns: zlib-based pre-compression of Vite build output
- Accept: `VitePluginCompression` passed to the default export
- Reject: shell-level gzip post-processing when this plugin is active in the Vite pipeline

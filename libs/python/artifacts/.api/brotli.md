# [PY_ARTIFACTS_API_BROTLI]

`brotli` supplies the Brotli compression surface for the artifacts compression rail: a one-shot function pair, an incremental compressor, and an incremental decompressor with mode/quality/window tuning for text, generic, and WOFF2 font payloads against the native brotli library. The package owner composes `compress`, `decompress`, `Compressor`, and `Decompressor` into the compression owner; it never re-implements the brotli codec the native core already owns. The codec stacks with the sibling artifacts compression band as a payload-discriminated rail: `brotli` (`MODE_FONT`) is the WOFF2 inverse paired with `fontTools.ttLib.woff2`, `MODE_TEXT`/`MODE_GENERIC` is the HTTP `Content-Encoding: br` transport path, `zstandard` owns archival, `lz4` owns hot-path block compression, and `py7zr`/`stream-zip` own container formats — one `CompressionMode` discriminant selects the codec, never parallel owners.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `brotli`
- package: `brotli`
- import: `brotli`
- owner: `artifacts`
- rail: compression
- version: `1.2.0`
- entry points: none (library only)
- capability: Brotli one-shot and incremental compression/decompression with mode (generic/text/font), quality, and window tuning, plus a bounded-output decompress probe pair (`output_buffer_limit` + `can_accept_more_data`/`is_finished`) for back-pressure-bounded streaming

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: codec types and modes
- rail: compression

| [INDEX] | [SYMBOL]                                   | [PACKAGE_ROLE]           | [CAPABILITY]                                                          |
| :-----: | :----------------------------------------- | :----------------------- | :------------------------------------------------------------------- |
|  [01]   | `Compressor`                               | incremental compressor   | streaming process/flush/finish compression                           |
|  [02]   | `Decompressor`                             | incremental decompressor | streaming process with bounded output, completion/back-pressure probes |
|  [03]   | `error`                                    | codec fault              | a brotli call failed (invalid args or codec failure)                 |
|  [04]   | `MODE_GENERIC` / `MODE_TEXT` / `MODE_FONT` | mode axis (int `0`/`1`/`2`) | input-tuned compression mode (generic / UTF-8 text / WOFF 2.0 font)  |
|  [05]   | `version`                                  | version anchor           | native libbrotli version string                                      |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: one-shot and incremental codec
- rail: compression

`compress`/`decompress` are the one-shot rows; `quality` is the speed/density tradeoff (0..11), `lgwin` the sliding-window log (10..24), `lgblock` the input-block log (0 or 16..24, `0` derives from quality). The incremental `Compressor.process`/`flush`/`finish` triple and the `Decompressor.process`/probe pair are the streaming rows; the decompressor's `output_buffer_limit` bounds a single `process` call's output so a hostile or highly-compressible stream cannot exhaust memory — when the limit is hit, the caller drives further `process(b"")` empty-input calls gated by `can_accept_more_data()` until it returns `True`.

| [INDEX] | [SURFACE]                           | [CALL_SHAPE]                                                                    | [CAPABILITY]                                            |
| :-----: | :---------------------------------- | :------------------------------------------------------------------------------ | :------------------------------------------------------ |
|  [01]   | `compress`                          | `compress(string, mode=MODE_GENERIC, quality=11, lgwin=22, lgblock=0) -> bytes` | one-shot compress                                       |
|  [02]   | `decompress`                        | `decompress(data) -> bytes`                                                     | one-shot decompress                                     |
|  [03]   | `Compressor`                        | `Compressor(mode=MODE_GENERIC, quality=11, lgwin=22, lgblock=0)`                | incremental compressor                                  |
|  [04]   | `Compressor.process`                | `process(data) -> bytes`                                                         | feed a chunk; output may be empty until enough buffered |
|  [05]   | `Compressor.flush`                  | `flush() -> bytes`                                                              | flush all pending input to a flush boundary             |
|  [06]   | `Compressor.finish`                 | `finish() -> bytes`                                                             | finalize the stream (no further `process`/`flush`)      |
|  [07]   | `Decompressor`                      | `Decompressor()`                                                                | incremental decompressor                                |
|  [08]   | `Decompressor.process`              | `process(data, output_buffer_limit=None) -> bytes`                              | feed a compressed chunk; cap per-call output bytes      |
|  [09]   | `Decompressor.is_finished`          | `is_finished() -> bool`                                                         | completion probe (stream fully consumed)                |
|  [10]   | `Decompressor.can_accept_more_data` | `can_accept_more_data() -> bool`                                                | back-pressure probe; gate empty-input drain after a limit hit |

## [04]-[IMPLEMENTATION_LAW]

[COMPRESSION_BROTLI]:
- import: `import brotli` at boundary scope only; module-level import is banned by the manifest import policy.
- modality axis: one-shot (`compress`/`decompress`) and incremental (`Compressor`/`Decompressor`) are rows, never parallel codec owners; the one-shot path is the default and the incremental path is selected only when a payload exceeds the streaming threshold.
- mode axis: `MODE_GENERIC`/`MODE_TEXT`/`MODE_FONT` and the `quality`/`lgwin`/`lgblock` knobs are constructor/call rows tuned per payload, never a per-mode type; the `CompressionMode` discriminant maps `MODE_FONT` to the WOFF2 path, `MODE_TEXT` to UTF-8 transport, and `MODE_GENERIC` to binary.
- bounded-decompress axis: `Decompressor.process(data, output_buffer_limit=cap)` plus the `can_accept_more_data`/`is_finished` probes is the one bounded-memory decode loop; a decompression bomb is contained by the per-call output cap, never by a hand-rolled size guard wrapping an unbounded `decompress`.
- evidence: each codec call captures mode, quality, window log, block log, input/output byte lengths, and (for the bounded path) the output-buffer cap and drain-call count as a compression receipt.
- boundary: brotli owns the web/transport (`Content-Encoding: br`) and WOFF2 (`MODE_FONT`, paired with `fontTools.ttLib.woff2`) codec path; archival routes to `zstandard`; hot-path block compression to `lz4`; archive containers to `py7zr`/`stream-zip`; failures surface as `brotli.error`, never as raw return codes; live UI stays outside this package.

## [05]-[LOCAL_ADMISSION]

[RAIL_LAW]:
- Package: `brotli`
- Owns: Brotli one-shot and incremental compression/decompression with mode/quality/window tuning
- Accept: web-transport and font codec service feeding the compression owner
- Reject: wrapper-renames of `compress`/`decompress`; a per-mode codec type where a call row suffices; identity minting the runtime owns

# [PY_ARTIFACTS_API_BROTLI]

`brotli` supplies the Brotli compression surface for the artifacts compression rail: a one-shot function pair, an incremental compressor, and an incremental decompressor with mode/quality/window tuning for text, generic, and WOFF2 font payloads against the native brotli library. The package owner composes `compress`, `decompress`, `Compressor`, and `Decompressor` into the compression owner; it never re-implements the brotli codec the native core already owns.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `brotli`
- package: `brotli`
- import: `brotli`
- owner: `artifacts`
- rail: compression
- installed: `1.2.0` reflected via `python -c "import brotli"` on cp315
- entry points: none (library only)
- capability: Brotli one-shot and incremental compression/decompression with mode (generic/text/font), quality, and window tuning

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: codec types and modes
- rail: compression

| [INDEX] | [SYMBOL]                                   | [PACKAGE_ROLE]           | [CAPABILITY]                                           |
| :-----: | :----------------------------------------- | :----------------------- | :----------------------------------------------------- |
|  [01]   | `Compressor`                               | incremental compressor   | streaming process/flush/finish compression             |
|  [02]   | `Decompressor`                             | incremental decompressor | streaming process with completion/back-pressure probes |
|  [03]   | `error`                                    | codec fault              | a brotli call failed                                   |
|  [04]   | `MODE_GENERIC` / `MODE_TEXT` / `MODE_FONT` | mode axis                | input-tuned compression mode                           |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: one-shot and incremental codec
- rail: compression

| [INDEX] | [SURFACE]                           | [CALL_SHAPE]                                                                    | [CAPABILITY]             |
| :-----: | :---------------------------------- | :------------------------------------------------------------------------------ | :----------------------- |
|  [01]   | `compress`                          | `compress(string, mode=MODE_GENERIC, quality=11, lgwin=22, lgblock=0) -> bytes` | one-shot compress        |
|  [02]   | `decompress`                        | `decompress(string) -> bytes`                                                   | one-shot decompress      |
|  [03]   | `Compressor`                        | `Compressor(mode=MODE_GENERIC, quality=11, lgwin=22, lgblock=0)`                | incremental compressor   |
|  [04]   | `Compressor.process`                | `process(data) -> bytes`                                                        | feed a chunk             |
|  [05]   | `Compressor.flush`                  | `flush() -> bytes`                                                              | flush buffered output    |
|  [06]   | `Compressor.finish`                 | `finish() -> bytes`                                                             | finalize the stream      |
|  [07]   | `Decompressor`                      | `Decompressor()`                                                                | incremental decompressor |
|  [08]   | `Decompressor.process`              | `process(data) -> bytes`                                                        | feed a compressed chunk  |
|  [09]   | `Decompressor.is_finished`          | `is_finished() -> bool`                                                         | completion probe         |
|  [10]   | `Decompressor.can_accept_more_data` | `can_accept_more_data() -> bool`                                                | back-pressure probe      |

## [04]-[IMPLEMENTATION_LAW]

[COMPRESSION_BROTLI]:
- import: `import brotli` at boundary scope only; module-level import is banned by the manifest import policy.
- modality axis: one-shot (`compress`/`decompress`) and incremental (`Compressor`/`Decompressor`) are rows, never parallel codec owners.
- mode axis: `MODE_GENERIC`/`MODE_TEXT`/`MODE_FONT` and the `quality`/`lgwin` knobs are constructor/call rows tuned per payload, never a per-mode type.
- evidence: each codec call captures mode, quality, window log, and input/output byte lengths as a compression receipt.
- boundary: brotli owns the web/transport and WOFF2 codec path; archival routes to `zstandard`; archive containers route to `py7zr`; live UI stays outside this package.

## [05]-[LOCAL_ADMISSION]

[RAIL_LAW]:
- Package: `brotli`
- Owns: Brotli one-shot and incremental compression/decompression with mode/quality/window tuning
- Accept: web-transport and font codec service feeding the compression owner
- Reject: wrapper-renames of `compress`/`decompress`; a per-mode codec type where a call row suffices; identity minting the runtime owns

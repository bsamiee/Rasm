# [PY_ARTIFACTS_API_BROTLI]

`brotli` supplies the Brotli compression surface for the artifacts compression rail: a one-shot function pair, an incremental compressor, and an incremental decompressor with mode/quality/window/block tuning for text, generic, and WOFF 2.0 font payloads against the native libbrotli encoder/decoder. The package owner composes `compress`, `decompress`, `Compressor`, and `Decompressor` into the compression owner; it never re-implements the brotli codec the native core already owns. The codec stacks with the sibling artifacts compression band as a payload-discriminated rail: `brotli` (`MODE_FONT`) is the WOFF2 transport-table codec paired with `fontTools.ttLib.woff2` (which owns the WOFF2 container/glyph-transform layer while brotli owns only the per-table entropy stream), `MODE_TEXT`/`MODE_GENERIC` is the HTTP `Content-Encoding: br` transport path, `zstandard` owns high-ratio archival, `lz4` owns hot-path block compression, and `py7zr`/`stream-zip`/`stream-unzip` own container formats — one `CompressionMode` discriminant selects the codec, never parallel owners.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `brotli`
- package: `brotli`
- import: `brotli`
- owner: `artifacts`
- rail: compression
- installed: `1.2.0`
- license: MIT (Python bindings + bundled native libbrotli, Brotli Authors)
- target: `cp315-cp315` native wheel (`_brotli` C extension; not pure-Python, not abi3 — one wheel per interpreter minor)
- entry points: none (library only)
- capability: Brotli one-shot and incremental compression/decompression with mode (generic/UTF-8 text/WOFF 2.0 font), quality (0..11), sliding-window (`lgwin` 10..24), and input-block (`lgblock` 0 or 16..24) tuning, plus a bounded-output decompress probe pair (`output_buffer_limit` + `can_accept_more_data`/`is_finished`) for back-pressure-bounded streaming against decompression bombs

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: codec types, modes, identity
- rail: compression

`Compressor` and `Decompressor` are the two incremental roots; all streaming modality is method state on them, never a per-profile codec subclass. `error` is the single fault type for every native call (subclasses `Exception`). The `MODE_*` constants are plain `int` (`0`/`1`/`2`) passed by value to the `mode=` knob — they are an axis on one codec, never three codec types. `version` is the binding/package version string (`'1.2.0'`, the same object as `__version__`), NOT a separately queryable native libbrotli build string — a consuming page must not treat it as a native-library version probe.

| [INDEX] | [SYMBOL]                                   | [PACKAGE_ROLE]              | [CAPABILITY]                                         |
| :-----: | :----------------------------------------- | :-------------------------- | :--------------------------------------------------- |
|  [01]   | `Compressor`                               | incremental compressor      | streaming `process`/`flush`/`finish` compression     |
|  [02]   | `Decompressor`                             | incremental decompressor    | streaming `process`, bounded output + probes         |
|  [03]   | `error`                                    | codec fault                 | brotli call failed; `Exception` subclass             |
|  [04]   | `MODE_GENERIC` / `MODE_TEXT` / `MODE_FONT` | mode axis (int `0`/`1`/`2`) | input-tuned mode (generic / UTF-8 text / WOFF2 font) |
|  [05]   | `version`                                  | identity anchor             | binding version string (`== __version__`)            |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: one-shot codec
- rail: compression — top-level convenience over a transient native codec

`compress`/`decompress` are the one-shot rows. `quality` is the speed/density tradeoff (0..11, 11 default = densest/slowest), `lgwin` the sliding-window log (10..24, 22 default), `lgblock` the input-block log (0 or 16..24; `0` derives the block from `quality`). `decompress` is unbounded and self-describing — it takes only the compressed bytes; the bounded-memory path lives on `Decompressor.process`, so a hostile or bomb payload is decoded through the incremental root, never through one-shot `decompress` wrapped in a hand-rolled size guard.

| [INDEX] | [SURFACE]    | [CALL_SHAPE]                                                                    | [CAPABILITY]                        |
| :-----: | :----------- | :------------------------------------------------------------------------------ | :---------------------------------- |
|  [01]   | `compress`   | `compress(string, mode=MODE_GENERIC, quality=11, lgwin=22, lgblock=0) -> bytes` | one-shot compress (transient codec) |
|  [02]   | `decompress` | `decompress(data) -> bytes`                                                     | one-shot decompress (unbounded)     |

[ENTRYPOINT_SCOPE]: `Compressor` incremental rows
- rail: compression — `Compressor(mode=MODE_GENERIC, quality=11, lgwin=22, lgblock=0)`; same four knobs as `compress`, fixed at construction for the stream lifetime

`process` feeds a chunk and may return empty bytes until enough input is buffered; `flush` drives all pending input to a flush boundary (decodable prefix, stream continues); `finish` finalizes — after it, `process`/`flush` cannot be called again and a new `Compressor` is required. All three return `bytes` that the caller concatenates in call order.

| [INDEX] | [SURFACE]            | [CALL_SHAPE]             | [CAPABILITY]                                                 |
| :-----: | :------------------- | :----------------------- | :----------------------------------------------------------- |
|  [01]   | `Compressor.process` | `process(data) -> bytes` | feed a chunk; output may be empty until enough buffered      |
|  [02]   | `Compressor.flush`   | `flush() -> bytes`       | flush pending input to a flush boundary (stream continues)   |
|  [03]   | `Compressor.finish`  | `finish() -> bytes`      | finalize the stream (terminal; no further `process`/`flush`) |

[ENTRYPOINT_SCOPE]: `Decompressor` incremental + bounded-output rows
- rail: compression — `Decompressor()` (no tuning knobs; the encoder fixed the parameters in the stream header)

`process` feeds compressed bytes and decodes; passing `output_buffer_limit=cap` bounds a single call's output so a highly-compressible or hostile stream cannot exhaust memory in one call. When the limit is hit, the caller MUST drive further `process(b"")` empty-input calls until `can_accept_more_data()` returns `True` again before feeding new compressed input. `is_finished()` reports stream completion; `can_accept_more_data()` is the back-pressure gate (it returns `True` unconditionally if the stream was never driven with a limit). NOTE: the native docstring narrates the legacy `decompress(..., max_length=...)` spelling, but the live bound method is `process(data, output_buffer_limit=...)` — a consuming page binds `output_buffer_limit`, not a phantom `max_length` kwarg.

| [INDEX] | [SURFACE]                           | [CALL_SHAPE]                                       | [CAPABILITY]                                |
| :-----: | :---------------------------------- | :------------------------------------------------- | :------------------------------------------ |
|  [01]   | `Decompressor.process`              | `process(data, output_buffer_limit=None) -> bytes` | feed a chunk; cap per-call output vs a bomb |
|  [02]   | `Decompressor.is_finished`          | `is_finished() -> bool`                            | completion probe                            |
|  [03]   | `Decompressor.can_accept_more_data` | `can_accept_more_data() -> bool`                   | back-pressure drain gate                    |

## [04]-[IMPLEMENTATION_LAW]

[COMPRESSION_BROTLI]:
- import: `import brotli` at boundary scope only; module-level import is banned by the manifest import policy.
- modality axis: one-shot (`compress`/`decompress`) and incremental (`Compressor`/`Decompressor`) are rows on the same codec, never parallel codec owners; the one-shot path is the default and the incremental path is selected only when a payload exceeds the streaming threshold or a bounded-memory decode is required. Pick the modality by the input shape, not by minting a new object.
- mode axis: `MODE_GENERIC`/`MODE_TEXT`/`MODE_FONT` plus the `quality`/`lgwin`/`lgblock` knobs are constructor/call rows tuned per payload, never a per-mode type; the upstream `CompressionMode` discriminant maps `MODE_FONT` to the WOFF2 per-table path (paired with `fontTools.ttLib.woff2`), `MODE_TEXT` to UTF-8 transport, and `MODE_GENERIC` to binary. `lgblock=0` is the standard default (the encoder derives the block size from `quality`).
- bounded-decompress axis: `Decompressor.process(data, output_buffer_limit=cap)` plus the `can_accept_more_data`/`is_finished` probes is the one bounded-memory decode loop; a decompression bomb is contained by the per-call output cap and the empty-input drain gated on `can_accept_more_data()`, never by a hand-rolled size guard wrapping the unbounded one-shot `decompress`. Treat a non-`bytes` / non-contiguous input as a boundary-rejected argument before it reaches the native call.
- integration: at the boundary, encode the canonical payload via `msgspec.msgpack.encode(...)`, compress it with a quality/`lgwin`-tuned mode (`MODE_TEXT` for UTF-8 transport bodies, `MODE_FONT` only for the WOFF2 table stream), wrap the producing boundary call in a `stamina`-retried block, and lift the result onto the universal `expression.Result[bytes, ArtifactError]` rail so a `brotli.error` becomes an `Error` case at the seam rather than a raised exception crossing the owner. Stamp a `structlog` event / `opentelemetry` span carrying `mode`, `quality`, `lgwin`, `lgblock`, input/output byte lengths, and (for the bounded path) `output_buffer_limit` plus the empty-input drain-call count. For a streaming sink, drive `Compressor.process`/`flush`/`finish` so back-pressure stays in the codec state, not in an in-RAM accumulation buffer.
- evidence: each codec call captures mode, quality, window log, block log, input/output byte lengths, and (for the bounded path) the output-buffer cap and drain-call count as a compression receipt contributed through the runtime `ReceiptContributor` port onto the single `ArtifactReceipt` family — never a parallel brotli-only receipt shape.
- boundary: brotli owns the web/transport (`Content-Encoding: br`) and WOFF2 per-table (`MODE_FONT`, paired with `fontTools.ttLib.woff2`) codec path; high-ratio archival routes to `zstandard`; hot-path block compression to `lz4`; archive containers to `py7zr`; bounded-memory streaming zip to `stream-zip`/`stream-unzip`; failures surface as `brotli.error` lifted to the `Result` rail, never as raw native return codes; live UI stays outside this package.

[RAIL_LAW]:
- Package: `brotli`
- Owns: Brotli one-shot and incremental compression/decompression with mode/quality/window/block tuning and bounded-output back-pressured decode
- Accept: web-transport and WOFF2 font codec service feeding the compression owner, selecting modality by input shape on the one-shot pair or the two incremental roots
- Reject: wrapper-renames of `compress`/`decompress`; a per-mode codec type where a `mode=` call row suffices; an unbounded one-shot `decompress` wrapped in a hand-rolled size guard where `Decompressor.process(output_buffer_limit=)` already bounds memory; a phantom `max_length` kwarg where the bound method is `output_buffer_limit`; treating `version` as a native libbrotli build probe; identity minting the runtime owns

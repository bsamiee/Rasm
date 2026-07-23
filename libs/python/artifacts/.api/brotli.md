# [PY_ARTIFACTS_API_BROTLI]

`brotli` owns the Brotli codec on the artifacts compression rail: the `compress`/`decompress` one-shot pair, the `Compressor`/`Decompressor` incremental roots, and `mode`/`quality`/`lgwin`/`lgblock` tuning against native libbrotli. `MODE_TEXT` drives the HTTP `Content-Encoding: br` transport path and `MODE_FONT` the WOFF2 per-table entropy stream, paired with `fontTools.ttLib.woff2` owning the WOFF2 container and glyph-transform layer; the `package/bundle#BUNDLE` `CompressionAlgo` discriminant routes one payload class here.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `brotli`
- package: `brotli` (MIT)
- module: `brotli`
- asset: native wheel; `_brotli` C extension, one wheel per interpreter minor (not abi3)
- rail: compression

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: incremental codec roots and fault

| [INDEX] | [SYMBOL]       | [TYPE_FAMILY] | [CAPABILITY]                                     |
| :-----: | :------------- | :------------ | :----------------------------------------------- |
|  [01]   | `Compressor`   | compressor    | streaming `process`/`flush`/`finish` compression |
|  [02]   | `Decompressor` | decompressor  | streaming decode with bounded output and probes  |
|  [03]   | `error`        | fault         | native call failed; `Exception` subclass         |

[PUBLIC_TYPE_SCOPE]: mode axis and identity
- [MODE]: `MODE_GENERIC` `MODE_TEXT` `MODE_FONT`
- [BINDING_ID]: `brotli.version` `brotli.__version__`

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: one-shot pair — `quality` 0..11 (11 densest, the default), `lgwin` 10..24 (22 default), `lgblock` 0 or 16..24 (0 derives from `quality`)

| [INDEX] | [SURFACE]                                         | [SHAPE] | [CAPABILITY]                        |
| :-----: | :------------------------------------------------ | :------ | :---------------------------------- |
|  [01]   | `compress(string, mode, quality, lgwin, lgblock)` | static  | one-shot compress (transient codec) |
|  [02]   | `decompress(data)`                                | static  | one-shot decompress (unbounded)     |

[ENTRYPOINT_SCOPE]: `Compressor` incremental — the four knobs fix at construction for the stream lifetime; after `finish` the root is terminal and a fresh `Compressor` is required

| [INDEX] | [SURFACE]                  | [SHAPE]  | [CAPABILITY]                                         |
| :-----: | :------------------------- | :------- | :--------------------------------------------------- |
|  [01]   | `Compressor.process(data)` | instance | feed a chunk; output empty until enough buffered     |
|  [02]   | `Compressor.flush()`       | instance | flush pending input to a boundary (stream continues) |
|  [03]   | `Compressor.finish()`      | instance | finalize the stream (terminal)                       |

[ENTRYPOINT_SCOPE]: `Decompressor` incremental and bounded — no knobs; the encoder fixed the parameters in the stream header

| [INDEX] | [SURFACE]                                              | [SHAPE]  | [CAPABILITY]                                |
| :-----: | :----------------------------------------------------- | :------- | :------------------------------------------ |
|  [01]   | `Decompressor.process(data, output_buffer_limit=None)` | instance | feed a chunk; cap per-call output vs a bomb |
|  [02]   | `Decompressor.is_finished()`                           | instance | completion probe                            |
|  [03]   | `Decompressor.can_accept_more_data()`                  | instance | back-pressure drain gate                    |

- `Decompressor.process(data, output_buffer_limit=cap)`: once the cap is hit, drive `process(b"")` until `can_accept_more_data()` returns `True` before feeding new compressed input; the cap is block-granular, so a call may return somewhat past it.

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- One-shot (`compress`/`decompress`) and incremental (`Compressor`/`Decompressor`) are modality rows on one codec, and `MODE_*` with the `quality`/`lgwin`/`lgblock` knobs are call arguments on it, never parallel codec owners or per-mode types; modality follows input shape and mode follows payload class.
- `Decompressor.process(output_buffer_limit=)` with the `can_accept_more_data`/`is_finished` probes is the one bounded-memory decode: the per-call cap and the `process(b"")` drain contain a decompression bomb, and an unfinished stream after input and drain exhaust signals truncation.
- Each codec call records one typed compression receipt carrying `mode`, `quality`, window and block logs, input/output byte lengths, and — on the bounded path — the output cap and drain-call count, onto the single `ArtifactReceipt` family.

[STACKING]:
- `fonttools`(`.api/fonttools.md`): `MODE_FONT` compresses the WOFF2 per-table entropy stream while `fontTools.ttLib.woff2` owns the container and glyph-transform layer.
- `msgspec`(`.api/msgspec.md`): the boundary encodes canonical payload `bytes` via `msgspec.msgpack.encode`, then brotli compresses the encoded buffer.
- `expression`(`.api/expression.md`): the producing boundary call runs `stamina`-retried and its result lifts onto the `expression.Result[bytes, ArtifactError]` rail, a `brotli.error` becoming an `Error` case at the seam rather than a raised exception crossing the owner.
- `structlog`(`.api/structlog.md`) / `opentelemetry`(`.api/opentelemetry-api.md`): each call stamps an event and span carrying the same codec facts the receipt records.
- artifacts compression owner: composes `compress`/`decompress`/`Compressor`/`Decompressor` and contributes the receipt through the runtime `ReceiptContributor` port onto `core/receipt#RECEIPT`; the upstream `BrotliKnobs` carries `mode`/`quality`/`lgwin`/`lgblock` as row data.

[LOCAL_ADMISSION]:
- `import brotli` at boundary scope only; module-level import is banned by the manifest import policy.
- A non-`bytes` or non-contiguous input is a boundary-rejected argument before it reaches the native call.
- Live UI stays outside this package.

[RAIL_LAW]:
- Package: `brotli`
- Owns: Brotli one-shot and incremental compression/decompression with mode/quality/window/block tuning and bounded-output back-pressured decode
- Accept: web-transport and WOFF2 font codec service feeding the compression owner, selecting modality by input shape on the one-shot pair or the two incremental roots
- Reject: wrapper-renames of `compress`/`decompress`; a per-mode codec type where a `mode=` call row suffices; an unbounded one-shot `decompress` wrapped in a hand-rolled size guard where `Decompressor.process(output_buffer_limit=)` already bounds memory; a phantom `max_length` kwarg where the bound method is `output_buffer_limit`; treating `version` as a native libbrotli build probe; identity minting the runtime owns

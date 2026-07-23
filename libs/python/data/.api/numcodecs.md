# [PY_DATA_API_NUMCODECS]

`numcodecs` mints the buffer-codec registry and `Codec` contract zarr binds for chunk filters, compressors, and the extra-compressor family beyond zarr's built-ins. Every codec subclasses `Codec` with one `encode`/`decode` pair and a JSON-round-trippable `get_config`/`from_config` identity keyed by `codec_id`, selected polymorphically through `get_codec`. It owns only the per-chunk byte transform over contiguous `numpy` buffers; `zarr`/`icechunk` own chunk indexing and IO, and the artifacts-rail codecs own transport payloads.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `numcodecs`
- package: `numcodecs`
- module: `import numcodecs`
- namespaces: `numcodecs`, `numcodecs.abc`, `numcodecs.registry`, `numcodecs.compat`, `numcodecs.blosc`, `numcodecs.zstd`, `numcodecs.errors`
- rail: array — chunk-codec supplier for the chunked-array rail
- entry points: codec plugins register through the `numcodecs.codecs` entry-point group (`run_entrypoints()` ingests them into `codec_registry`); the `zfpy`/`pcodec` extras add the `ZFPY`/`PCodec` serializers

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: codec contract, registry, and buffer protocol

| [INDEX] | [SYMBOL]            | [TYPE_FAMILY] | [CAPABILITY]                                                                   |
| :-----: | :------------------ | :------------ | :----------------------------------------------------------------------------- |
|  [01]   | `Codec`             | abc           | polymorphic contract: `encode`/`decode`/`get_config`/`from_config`, `codec_id` |
|  [02]   | `codec_registry`    | dict          | `codec_id -> type[Codec]` table `get_codec` reads                              |
|  [03]   | `UnknownCodecError` | exception     | config `'id'` resolved to no registered codec                                  |
|  [04]   | `NDArrayLike`       | protocol      | runtime-checkable structural numpy-like buffer                                 |

[PUBLIC_TYPE_SCOPE]: lossless compressor codecs (byte-stream)

| [INDEX] | [SYMBOL]                                             | [TYPE_FAMILY] | [CAPABILITY]                                                |
| :-----: | :--------------------------------------------------- | :------------ | :---------------------------------------------------------- |
|  [01]   | `Blosc(cname, clevel, shuffle, blocksize, typesize)` | class         | meta-compressor over c-blosc; `cname` picks the inner codec |
|  [02]   | `Zstd(level, checksum)`                              | class         | libzstd frame                                               |
|  [03]   | `LZ4(acceleration)`                                  | class         | liblz4 block                                                |
|  [04]   | `GZip(level)`                                        | class         | gzip stream                                                 |
|  [05]   | `BZ2(level)`                                         | class         | bzip2 stream                                                |
|  [06]   | `LZMA(format, check, preset, filters)`               | class         | xz/lzma stream                                              |
|  [07]   | `Zlib(level)`                                        | class         | zlib stream                                                 |

[PUBLIC_TYPE_SCOPE]: array filter/transform codecs (reshape before a compressor)

| [INDEX] | [SYMBOL]                                         | [TYPE_FAMILY] | [CAPABILITY]                                    |
| :-----: | :----------------------------------------------- | :------------ | :---------------------------------------------- |
|  [01]   | `Delta(dtype, astype)`                           | class         | per-element difference for monotone sequences   |
|  [02]   | `FixedScaleOffset(offset, scale, dtype, astype)` | class         | linear quantization to a narrower integer dtype |
|  [03]   | `Quantize(digits, dtype, astype)`                | class         | lossy float truncation to `digits` places       |
|  [04]   | `BitRound(keepbits)`                             | class         | IEEE-754 mantissa-bit rounding                  |
|  [05]   | `Shuffle(elementsize)`                           | class         | byte-transpose so like bytes group              |
|  [06]   | `PackBits()`                                     | class         | bit-pack a boolean array                        |
|  [07]   | `AsType(encode_dtype, decode_dtype)`             | class         | dtype reinterpretation across the boundary      |
|  [08]   | `Categorize(labels, dtype, astype)`              | class         | map a fixed label set to small integer codes    |

[PUBLIC_TYPE_SCOPE]: checksum codecs (append/verify a checksum word)

| [INDEX] | [SYMBOL]                          | [TYPE_FAMILY] | [CAPABILITY]                        |
| :-----: | :-------------------------------- | :------------ | :---------------------------------- |
|  [01]   | `CRC32(location)`                 | class         | CRC-32; `decode` raises on mismatch |
|  [02]   | `CRC32C(location)`                | class         | CRC-32C Castagnoli                  |
|  [03]   | `Adler32(location)`               | class         | Adler-32                            |
|  [04]   | `Fletcher32()`                    | class         | Fletcher-32                         |
|  [05]   | `JenkinsLookup3(initval, prefix)` | class         | Jenkins lookup3 hash                |

`location` accepts `'start'`, `'end'`, or `None` and places the checksum word.

[PUBLIC_TYPE_SCOPE]: variable-length and serialization codecs

| [INDEX] | [SYMBOL]                                       | [TYPE_FAMILY] | [CAPABILITY]                           |
| :-----: | :--------------------------------------------- | :------------ | :------------------------------------- |
|  [01]   | `VLenUTF8()`                                   | class         | ragged UTF-8 string array              |
|  [02]   | `VLenBytes()`                                  | class         | ragged bytes array                     |
|  [03]   | `VLenArray(dtype)`                             | class         | ragged sub-array array                 |
|  [04]   | `JSON(encoding, sort_keys, indent, ...)`       | class         | object array via JSON                  |
|  [05]   | `MsgPack(use_single_float, use_bin_type, raw)` | class         | object array via msgpack               |
|  [06]   | `Pickle(protocol)`                             | class         | object array via pickle; boundary-only |
|  [07]   | `Base64()`                                     | class         | base64 byte transcoding                |

Registry ids diverge from class names at `json2`, `msgpack2`, `vlen-utf8`/`vlen-bytes`/`vlen-array`, and `jenkins_lookup3`; every other id lowercases the class name.

[PUBLIC_TYPE_SCOPE]: zarr-v3 codec adapter (`zarr.codecs.numcodecs`) — base family fixes the pipeline slot

| [INDEX] | [SYMBOL]          | [TYPE_FAMILY] | [CAPABILITY]        |
| :-----: | :---------------- | :------------ | :------------------ |
|  [01]   | `BytesBytesCodec` | interface     | `compressors=` slot |
|  [02]   | `ArrayArrayCodec` | interface     | `filters=` slot     |
|  [03]   | `ArrayBytesCodec` | interface     | `serializer=` slot  |

- `[BytesBytesCodec]`: `Blosc` `Zstd` `GZip` `LZ4` `LZMA` `BZ2` `Zlib` `Shuffle` `Adler32` `CRC32` `CRC32C` `Fletcher32` `JenkinsLookup3`
- `[ArrayArrayCodec]`: `Delta` `FixedScaleOffset` `Quantize` `BitRound` `PackBits` `AsType`
- `[ArrayBytesCodec]`: `PCodec` `ZFPY`

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: codec resolution (`numcodecs.registry`, re-exported at `numcodecs`)

| [INDEX] | [SURFACE]                            | [SHAPE] | [CAPABILITY]                                                            |
| :-----: | :----------------------------------- | :------ | :---------------------------------------------------------------------- |
|  [01]   | `get_codec(config) -> Codec`         | factory | instantiate the codec named by `config['id']`, remaining keys as kwargs |
|  [02]   | `register_codec(cls, codec_id=None)` | static  | add a codec class to `codec_registry`                                   |
|  [03]   | `run_entrypoints()`                  | static  | ingest `numcodecs.codecs` entry-point plugins into the registry         |

[ENTRYPOINT_SCOPE]: per-codec contract (every `Codec` subclass)

| [INDEX] | [SURFACE]                                 | [SHAPE]  | [CAPABILITY]                                           |
| :-----: | :---------------------------------------- | :------- | :----------------------------------------------------- |
|  [01]   | `encode(buf) -> bytes\|ndarray`           | instance | forward byte transform                                 |
|  [02]   | `decode(buf, out=None) -> bytes\|ndarray` | instance | inverse; `out=` preallocated destination avoids a copy |
|  [03]   | `get_config() -> dict`                    | instance | JSON-safe config carrying `'id'`                       |
|  [04]   | `from_config(config) -> Codec`            | factory  | classmethod inverse of `get_config`                    |

Chain codecs by feeding one `encode` output into the next codec's `encode`, and decode in reverse.

[ENTRYPOINT_SCOPE]: buffer coercion (`numcodecs.compat`)

| [INDEX] | [SURFACE]                                                                       | [SHAPE] | [CAPABILITY]                   |
| :-----: | :------------------------------------------------------------------------------ | :------ | :----------------------------- |
|  [01]   | `ensure_contiguous_ndarray(buf, max_buffer_size=None, flatten=True) -> ndarray` | static  | canonical codec-input coercion |
|  [02]   | `ensure_ndarray(buf) -> ndarray`                                                | static  | coerce to ndarray              |
|  [03]   | `ensure_bytes(buf) -> bytes`                                                    | static  | coerce to bytes                |
|  [04]   | `ensure_text(s, encoding='utf-8') -> str`                                       | static  | coerce to text                 |
|  [05]   | `ndarray_copy(src, dst) -> ndarray`                                             | static  | copy into a destination        |
|  [06]   | `is_ndarray_like(obj) -> bool`                                                  | static  | structural buffer check        |

`ensure_ndarray_like`/`ensure_contiguous_ndarray_like` are the duck-typed variants accepting any `NDArrayLike` without a numpy copy.

[ENTRYPOINT_SCOPE]: native thread and one-shot control (`numcodecs.blosc`, `numcodecs.zstd`)

| [INDEX] | [SURFACE]                                                                | [SHAPE] | [CAPABILITY]                             |
| :-----: | :----------------------------------------------------------------------- | :------ | :--------------------------------------- |
|  [01]   | `set_nthreads(n) -> int`                                                 | static  | set c-blosc's internal pool size         |
|  [02]   | `get_nthreads() -> int`                                                  | static  | read the pool size                       |
|  [03]   | `list_compressors() -> list[str]`                                        | static  | inner compressors compiled into blosc    |
|  [04]   | `cbuffer_complib(buf) -> str`                                            | static  | which compressor produced a blosc buffer |
|  [05]   | `compress(source, typesize, cname, clevel, shuffle, blocksize) -> bytes` | static  | one-shot path the `Blosc` codec wraps    |
|  [06]   | `decompress(source, dest=None) -> bytes`                                 | static  | one-shot blosc decompress                |

`numcodecs.zstd` mirrors `compress`/`decompress` with `DEFAULT_CLEVEL`/`MAX_CLEVEL`/`VERSION_NUMBER`; blosc exposes the `NOSHUFFLE`/`SHUFFLE`/`BITSHUFFLE`/`AUTOSHUFFLE` shuffle constants.

[ENTRYPOINT_SCOPE]: zarr-v3 adapter methods (`zarr.codecs.numcodecs.<Codec>`)

| [INDEX] | [SURFACE]                                             | [SHAPE]  | [CAPABILITY]                    |
| :-----: | :---------------------------------------------------- | :------- | :------------------------------ |
|  [01]   | `to_dict() -> dict`                                   | instance | emit zarr metadata              |
|  [02]   | `from_dict(d) -> Codec`                               | factory  | rebuild from zarr metadata      |
|  [03]   | `evolve_from_array_spec(array_spec)`                  | instance | bind the codec to an array spec |
|  [04]   | `compute_encoded_size(input_byte_length, chunk_spec)` | instance | predict encoded chunk size      |
|  [05]   | `validate(...)`                                       | instance | validate against array metadata |

Pass these in a zarr array's `filters=`/`serializer=`/`compressors=` slots.

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Every codec folds through one `encode`/`decode`/`get_config`/`from_config` shape keyed by `codec_id`; `get_codec({'id': ...})` selects on the `'id'` discriminant and the config dict is the sole persisted identity.
- A filter reshapes then a compressor compresses: `Delta`/`FixedScaleOffset`/`BitRound`/`Shuffle` precede `Blosc`/`Zstd` in the chain and invert on decode, with `Shuffle`/`BitRound` before `Zstd` the numeric-chunk recipe.
- `Quantize`/`FixedScaleOffset`/`BitRound` discard precision by `digits`/`scale`/`keepbits` irreversibly; the lossy parameter rides the chunk receipt.
- `numcodecs.blosc.set_nthreads(n)` sets c-blosc's pool once at startup; codec instances are immutable and reused across every chunk.
- `encode`/`decode` operate on contiguous buffers coerced by `numcodecs.compat.ensure_contiguous_ndarray`, and the decoder writes into a preallocated `out=` for large chunks.

[STACKING]:
- `zarr`(`.api/zarr.md`): a codec binds to a zarr array through `zarr.codecs.numcodecs.<Codec>`; the base family fixes the slot — `ArrayArrayCodec` into `filters=`, `ArrayBytesCodec` as `serializer=`, `BytesBytesCodec` into `compressors=` — and zarr applies `filters -> serializer -> compressors` on write, inverting on read.
- `icechunk`(`.api/icechunk.md`): supplies the per-chunk byte transform for chunks the versioned store persists; icechunk owns chunk indexing and IO, numcodecs owns only the transform.
- `msgspec`(`libs/python/.api/msgspec.md`)/`pydantic`(`libs/python/.api/pydantic.md`): decode a codec config dict into a discriminated `Codec`-config struct tagged on the `'id'` field, then round-trip the struct back to `get_codec` to instantiate.
- `beartype`(`libs/python/.api/beartype.md`): annotate the chunk-codec rail with `numcodecs.compat.NDArrayLike` so a non-buffer input is rejected at the contract, not deep in the C extension.
- `structlog`(`libs/python/.api/structlog.md`)+`opentelemetry-api`(`libs/python/.api/opentelemetry-api.md`): record `codec_id`, the lossy parameter, input/output byte lengths, and compression ratio as a chunk-codec receipt on the array-write span; `numcodecs.blosc.cbuffer_complib(buf)` recovers a stored frame's inner compressor.
- `anyio`(`libs/python/.api/anyio.md`): fan a many-chunk encode/decode batch across `anyio.to_thread.run_sync`, sizing the outer pool against `numcodecs.blosc.get_nthreads()` since c-blosc threads internally.
- `brotli`(`libs/python/artifacts/.api/brotli.md`)/`zstandard`(`libs/python/artifacts/.api/zstandard.md`): the artifacts-rail codecs own transport and container payloads; array chunks route through numcodecs, file and transport payloads through the artifacts codecs.
- within-lib: `Blosc` fuses the byte-shuffle and inner codec — `cname` selects the inner codec and `typesize`+`shuffle` drive the SIMD shuffle, so `Blosc(cname='zstd', shuffle=Blosc.BITSHUFFLE, typesize=4)` is one fused shuffle+zstd pass in place of a separate `Shuffle` + `Zstd` chain.

[LOCAL_ADMISSION]:
- Select a codec through `get_codec({'id': ..., ...})` and persist it as the config dict.
- Bind a codec to a zarr array through `zarr.codecs.numcodecs.<Codec>` in the slot its base family dictates.
- Coerce buffers through `numcodecs.compat.ensure_contiguous_ndarray` at the boundary and set `set_nthreads` once at process start.

[RAIL_LAW]:
- Package: `numcodecs`
- Owns: the buffer-codec registry and `Codec` contract for the chunked-array rail — lossless compressor, array filter/transform, checksum, and variable-length/serialization codecs, JSON-config resolution, buffer coercion, native thread control, and the zarr-v3 codec adapter
- Accept: codec selection through `get_codec` and config-dict persistence; the filter-then-compressor chain with `Shuffle`/`BitRound` before `Zstd`/`Blosc`; `Blosc` as the fused shuffle+inner-codec meta-compressor; `ensure_contiguous_ndarray` at the boundary; process-global `set_nthreads`; zarr binding through `zarr.codecs.numcodecs.<Codec>`; lossy-parameter and ratio capture on the chunk receipt
- Reject: hand-rolled compression where a codec exists; per-codec method names or parallel constructors where one `get_codec`/`encode`/`decode` shape suffices; the deprecated `numcodecs.zarr3` shim in place of `zarr.codecs.numcodecs`; hand-encoded chunk bytes outside the pipeline; a lossy filter on data that must round-trip bit-exact; `@retry` around a pure `encode`/`decode`; per-chunk thread reconfiguration; discontiguous buffers reaching `encode` uncoerced; a duplicate `Zstd`/`Blosc` owner where the artifacts-rail codecs own transport and container payloads

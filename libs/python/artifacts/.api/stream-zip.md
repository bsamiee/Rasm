# [PY_ARTIFACTS_API_STREAM_ZIP]

`stream-zip` supplies the bounded-memory streaming ZIP CONSTRUCTION surface for the artifacts bundle rail: a generator `stream_zip` that consumes an iterable of `MemberFile` tuples and yields ZIP container `bytes` chunk by chunk, plus a `Method` family (`ZIP_64`, `ZIP_32`, `ZIP_AUTO`, `NO_COMPRESSION_32`, `NO_COMPRESSION_64`) that selects per-member format, ZIP64 upgrade, and compression, and WinZip AES-256 encryption keyed by `password`. The package owner (`package/archive#ARCHIVE` over `package/codec#CODEC`) composes `stream_zip` and its `Method` constants into the `CompressionAlgo.ZIP_STREAM` container arm; it removes the `zipfile.ZipFile` whole-archive buffering leak because every member is encoded as it streams, and it never re-implements the ZIP central-directory, ZIP64 extra-field, or WinZip-AES record layout `stream_zip` already owns. It is the exact pack inverse of `stream-unzip` (the gated extractor).

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `stream-zip`
- package: `stream-zip`
- import: `stream_zip`
- owner: `artifacts`
- rail: bundle
- installed: `0.0.84`
- license: `MIT` (`Classifier: License :: OSI Approved :: MIT License`)
- build-floor: pure-Python wheel (`py.typed`, no compiled extension), `Requires-Python >=3.6.7`; abi-agnostic so it installs on cp315 with no marker/gate. Sole runtime dependency `pycryptodome>=3.10.1` (the WinZip-AES backend).
- entry points: library use is import-only; no console script; no `__all__`/`__version__` export (the module `dir()` IS the public surface).
- capability: bounded-memory streaming ZIP construction yielding `Iterable[bytes]`, per-member ZIP32/ZIP64 format selection with size/offset-driven automatic upgrade, raw-deflate or stored (uncompressed buffered/streamed) compression, the Info-ZIP `UT` extended-timestamp extra field (the only timestamp extra written), WinZip AES-256 encryption (PBKDF2/1000-iter key derivation, AES-CTR encrypt, HMAC-SHA1 authentication via `pycryptodome`) keyed by `password`, streamed-content CRC32/uncompressed-size integrity verification for pre-sized stored members, and a mirrored `async_stream_zip` over async iterables (a loop-bridged thread wrapper, not a native async encoder)

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: member tuples, method family, and integrity rails
- rail: bundle

`MemberFile` is the `(name, modified_at, mode, method, data)` tuple `stream_zip` consumes; `AsyncMemberFile` is its async-data twin. `Method` is the abstract base (`Method(ABC)` with one abstract `_get(offset, default_get_compressobj) -> _MethodTuple`, the 5-element `(format_sentinel, auto_upgrade_sentinel, get_compressobj, uncompressed_size, crc_32)` the encoder dispatches on); its public instances (`ZIP_64`, `ZIP_32`, `ZIP_AUTO`, `NO_COMPRESSION_32`, `NO_COMPRESSION_64`) choose format and compression. `ZIP_AUTO` and the `NO_COMPRESSION_*` instances are ALSO callable to parameterize size/level — calling them mints a fresh anonymous `Method` carrying the bound size/CRC, so the constants are both the buffered-mode method and the streamed-mode factory. The error tree is a single rooted hierarchy: `ZipError` is the root, `ZipValueError(ZipError, ValueError)` is the boundary base, `ZipIntegrityError(ZipValueError)` and `ZipOverflowError(ZipValueError, OverflowError)` are the two failure families, and every concrete failure descends from one of them — one `except ZipError` catches all, `except ZipValueError` catches every recoverable boundary failure.

| [INDEX] | [SYMBOL]            | [TYPE_FAMILY]   | [DEFINITION]                                                               |
| :-----: | :------------------ | :-------------- | :------------------------------------------------------------------------- |
|  [01]   | `MemberFile`        | tuple alias     | `tuple[str, datetime, int, Method, Iterable[bytes]]`                       |
|  [02]   | `AsyncMemberFile`   | tuple alias     | `tuple[str, datetime, int, Method, AsyncIterable[bytes]]`                  |
|  [03]   | `Method`            | abstract base   | `Method(ABC)`; `_get(offset, default_get_compressobj) -> _MethodTuple`     |
|  [04]   | `ZIP_64`            | method instance | force ZIP64 member; deflate from `stream_zip`'s `get_compressobj`          |
|  [05]   | `ZIP_32`            | method instance | force ZIP32 member; deflate from `stream_zip`'s `get_compressobj`          |
|  [06]   | `ZIP_AUTO`          | callable method | size/offset ZIP32->ZIP64 auto-upgrade; binds own deflate at `level`        |
|  [07]   | `NO_COMPRESSION_32` | callable method | stored ZIP32 member (buffered instance, or pre-sized streamed when called) |
|  [08]   | `NO_COMPRESSION_64` | callable method | stored ZIP64 member (buffered instance, or pre-sized streamed when called) |

The error tree descends from `ZipError`; `ZipValueError` is the boundary base, and `ZipIntegrityError`/`ZipOverflowError` are the two failure families.

| [INDEX] | [SYMBOL]                                       | [FAULT]                                                         |
| :-----: | :--------------------------------------------- | :-------------------------------------------------------------- |
|  [01]   | `ZipError`                                     | `Exception` root of every `stream_zip` failure                  |
|  [02]   | `ZipValueError`                                | `ZipError` + `ValueError`; boundary base for integrity+overflow |
|  [03]   | `ZipIntegrityError`                            | `ZipValueError`; declared-content mismatch base                 |
|  [04]   | `CRC32IntegrityError`                          | `ZipIntegrityError`; streamed CRC32 mismatch                    |
|  [05]   | `UncompressedSizeIntegrityError`               | `ZipIntegrityError`; streamed uncompressed-size mismatch        |
|  [06]   | `ZipOverflowError`                             | `ZipValueError` + `OverflowError`; field-overflow base          |
|  [07]   | `UncompressedSizeOverflowError`                | `ZipOverflowError`; uncompressed size exceeds field width       |
|  [08]   | `CompressedSizeOverflowError`                  | `ZipOverflowError`; compressed size exceeds field width         |
|  [09]   | `CentralDirectorySizeOverflowError`            | `ZipOverflowError`; central-directory size exceeds field width  |
|  [10]   | `OffsetOverflowError`                          | `ZipOverflowError`; member offset exceeds ZIP32 field width     |
|  [11]   | `CentralDirectoryNumberOfEntriesOverflowError` | `ZipOverflowError`; entry count exceeds ZIP32 field width       |
|  [12]   | `NameLengthOverflowError`                      | `ZipOverflowError`; member name length exceeds field width      |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: streaming functions
- rail: bundle

`stream_zip` returns a lazy `Iterable[bytes]`; iterating drives `files` member by member so peak memory stays bounded by `chunk_size` and per-member buffering. `password` (a `str`, not `bytes`) switches EVERY member to WinZip AES-256; `get_compressobj` overrides the default raw-deflate object for the `ZIP_64`/`ZIP_32`/stored formats; `get_crypto_random` overrides the encryption salt/IV byte source. `async_stream_zip` mirrors the signature over async iterables. Asymmetry to compose: the `ZIP_AUTO` member IGNORES the function-level `get_compressobj` and builds its own `zlib.compressobj(level=<passed level>, memLevel=8, wbits=-zlib.MAX_WBITS)`, so a uniform deflate codec across every member format is only achieved by setting `stream_zip`'s `get_compressobj` AND passing the matching `level` to each `ZIP_AUTO(size, level)`. Both share `(files, chunk_size=65536, get_compressobj=lambda: zlib.compressobj(wbits=-zlib.MAX_WBITS, level=9), extended_timestamps=True, password=None, get_crypto_random=lambda n: secrets.token_bytes(n))`; `stream_zip` maps `Iterable[MemberFile] -> Iterable[bytes]` and `async_stream_zip` maps `AsyncIterable[AsyncMemberFile] -> AsyncIterable[bytes]`.

| [INDEX] | [SURFACE]          | [CAPABILITY]                                                                                             |
| :-----: | :----------------- | :------------------------------------------------------------------------------------------------------- |
|  [01]   | `stream_zip`       | stream a ZIP archive as ordered byte chunks                                                              |
|  [02]   | `async_stream_zip` | async mirror; runs sync `stream_zip` on a worker thread via `run_in_executor`/`run_coroutine_threadsafe` |

[ENTRYPOINT_SCOPE]: `Method` selection and parameterization
- rail: bundle

`ZIP_64`/`ZIP_32` are used directly as the `method` slot of a `MemberFile`. `ZIP_AUTO` and the `NO_COMPRESSION_*` constants are called first to bind size/level, returning a `Method` to place in the tuple. The auto-upgrade is two-axis: ZIP64 is selected when `uncompressed_size > 4293656841` (the deflate worst-case bound for the default codec) OR `offset > 0xffffffff` (the running archive offset crosses the ZIP32 field) — so a small member late in a large archive still upgrades on offset, and `ZIP_AUTO` also flags the central directory for auto-upgrade (unlike the forced `ZIP_64`/`ZIP_32`).

| [INDEX] | [SURFACE]           | [CALL_SHAPE]                                                       | [CAPABILITY]                                |
| :-----: | :------------------ | :----------------------------------------------------------------- | :------------------------------------------ |
|  [01]   | `ZIP_64`            | used as `method` directly                                          | force ZIP64; deflate from `get_compressobj` |
|  [02]   | `ZIP_32`            | used as `method` directly                                          | force ZIP32; deflate from `get_compressobj` |
|  [03]   | `ZIP_AUTO`          | `ZIP_AUTO(uncompressed_size: int, level: int=9) -> Method`         | size/offset auto-upgrade; own deflate       |
|  [04]   | `NO_COMPRESSION_32` | `NO_COMPRESSION_32(uncompressed_size: int, crc_32: int) -> Method` | stored ZIP32; pre-declared size + CRC32     |
|  [05]   | `NO_COMPRESSION_64` | `NO_COMPRESSION_64(uncompressed_size: int, crc_32: int) -> Method` | stored ZIP64; pre-declared size + CRC32     |

## [04]-[IMPLEMENTATION_LAW]

[BUNDLE_STREAMING_ZIP]:
- import: `from stream_zip import stream_zip, ZIP_64, ZIP_32, ZIP_AUTO, NO_COMPRESSION_32, NO_COMPRESSION_64` at boundary scope only; module-level import is banned by the manifest import policy.
- member axis: one `MemberFile` tuple `(name, modified_at, mode, method, data)` owns each archive entry; `name`/`modified_at`/`mode` are tuple slots, never a per-member builder type; `data` is a lazy `Iterable[bytes]` so member bytes never fully materialize. `modified_at` is a `datetime` written to the DOS field AND (when `extended_timestamps=True`) the `UT` Info-ZIP extra; `mode` is the POSIX mode written to the Unix external attributes.
- format axis: `method` selects the container format per member; `ZIP_AUTO` is the size/offset-driven row that upgrades ZIP32 to ZIP64 past the `4293656841` size bound or the `0xffffffff` offset bound; `ZIP_64`/`ZIP_32` force the format; the `NO_COMPRESSION_*` rows store bytes uncompressed (`(size, crc_32)` for streamed pre-sized members, bare instance for buffered), never a parallel archive type. The owning design resolves a `ZipMethod` (`"auto"`/`"zip64"`/`"zip32"`/`"store32"`/`"store64"`) Literal to a `(builder, ZipArity)` row through one frozen `_ZIP_METHOD` dispatch table, the `ZipArity` selecting `builder` / `builder(size, level)` / `builder(size, crc_32)`, so the serialized profile carries no native `Method` handle.
- compression axis: `get_compressobj` is the single deflate-object override row for the `ZIP_64`/`ZIP_32`/stored formats; the default yields a raw-deflate `zlib.compressobj(wbits=-zlib.MAX_WBITS, level=9)`. `ZIP_AUTO(size, level)` does NOT read `get_compressobj` — it binds its own raw-deflate at the passed `level` with `memLevel=8` — so a uniform codec across all members sets both. Stored mode routes through the `NO_COMPRESSION_*` methods, never a flag on the function. The owning design binds `get_compressobj=lambda: zlib_ng.compressobj(wbits=-zlib_ng.MAX_WBITS, level=level)` (the shared `data:compression` SIMD raw-DEFLATE) so the forced/stored formats and the `ZIP_AUTO` level honour ONE `level` axis on the same `zlib-ng` substrate.
- encryption axis: `password` is the single WinZip AES-256 switch applied to every member; `get_crypto_random` overrides the 16-byte salt + counter byte source; encryption is a function-level row, never a per-member type or a second function. The record is WinZip AE-2/AES-256: PBKDF2 (1000 iterations) derives the AES key + HMAC key + 2-byte password-verification value, AES-CTR (128-bit little-endian counter) encrypts the stream, and an HMAC-SHA1 tag (truncated to 10 bytes) authenticates it — all via the `pycryptodome` `AES`/`HMAC`/`SHA1`/`PBKDF2` primitives, never a hand-rolled cipher.
- integrity axis: streamed `NO_COMPRESSION_*` members carry a declared `(uncompressed_size, crc_32)` that the encoder verifies against the actual streamed bytes, raising `CRC32IntegrityError`/`UncompressedSizeIntegrityError` (both under `ZipIntegrityError` -> `ZipValueError`); ZIP32 field overflows raise the `ZipOverflowError` subtree (under `ZipValueError`) — `UncompressedSizeOverflowError`/`CompressedSizeOverflowError`/`CentralDirectorySizeOverflowError`/`OffsetOverflowError`/`CentralDirectoryNumberOfEntriesOverflowError`/`NameLengthOverflowError`; every failure descends from `ZipError`, so a fault rail catches `ZipValueError` for recoverable boundary faults and `ZipError` for the root.
- evidence: each archive captures member count, per-member name, declared method, ZIP32-vs-ZIP64 resolution, encryption flag, and emitted byte length as the `package/codec#CODEC` `BundleEvidence` projection onto `core/receipt#RECEIPT` `ArtifactReceipt.Bundle` — `entries` the member arity, `verified` the streamed-integrity member count `stream_zip` checks inline.
- boundary: `stream_zip` owns ZIP container framing, ZIP64 local + central extra fields, the `UT` Info-ZIP extended-timestamp extra, and WinZip-AES records (via `pycryptodome` AES-256/HMAC-SHA1/PBKDF2); `async_stream_zip` is the async-iterable boundary over the same encoder (loop-bridged onto a worker thread, not a native async encoder); downstream sinks consume the `bytes` chunks directly to a file, response body, or upload stream; archive extraction routes to `stream-unzip` (the gated inverse), not this package.

[STACKING]:
- The owning design is `package/archive#ARCHIVE` over the `package/codec#CODEC` union, not a freestanding call site: `CompressionAlgo.ZIP_STREAM` is the one `match` arm that binds `stream_zip`/`async_stream_zip`, reading the `zip_stream` `ZipStreamKnobs` `CodecProfile` case (`method`/`level`/`mechanisms`/`password`/`names`/`modified_at`/`mode`). The `ZipMethod` Literal resolves through `_ZIP_METHOD` to a `(builder, ZipArity)` row; `_zip_members` folds each payload into a `(name, modified_at, mode, method, iter((payload,)))` `MemberFile` whose `method` the `ZipArity` `match` selects (`builder`, `builder(size, level)`, or `builder(size, zlib_ng.crc32(payload))`), and `b"".join(stream_zip(members, password=..., get_compressobj=...))` seals the container blob — so a hardcoded `Method` literal or a `.startswith("store")` probe is the deleted form.
- Byte-reproducibility is the content-addressing seam: the design fixes `modified_at` to the `_EPOCH` constant (`1980-01-01`, the ZIP epoch) and the deflate to `zlib_ng` at a fixed `level`, so an unencrypted bundle of identical payloads at an identical profile yields identical bytes run-to-run and dedups by the runtime content key — never the `datetime.now()` stamp that churns every byte. An encrypted bundle is intentionally non-reproducible: `get_crypto_random` defaults to `secrets.token_bytes`, freshly randomizing the AES salt/IV per pack (the same hook accepts a deterministic byte source for reproducible-fixture AES-path tests).
- `ZIP_AUTO(uncompressed_size, level)` reads the per-member size the producer already knows (a `msgspec.msgpack.encode` length, a `pikepdf`/`pymupdf` page-count estimate, a `segno` QR `save(io, kind=...)` buffer length) so the ZIP32-vs-ZIP64 decision is data-driven, not a post-hoc `ZipOverflowError` catch; and because `data` is a lazy `Iterable[bytes]`, a `tomlkit.dumps(...)` document, an `svgelements`-composed SVG, or a `weasyprint`/`pymupdf` PDF stream feeds straight into a member without buffering the whole file — `stream_zip` interleaves encode-and-emit.
- Universal-rail tier: the per-member triples fold into the codec page's `ContentKey` over the shared `xxhash` digest substrate (`xxh3_128`/`XXH3`); the bundle receipt fields cross into `core/receipt#RECEIPT` and ride the shared `structlog` + OpenTelemetry span the rail wraps; `async_stream_zip` is consumed on the shared `anyio` structured-concurrency rail — and because it is already loop-bridged through a thread executor internally, the design offloads the synchronous `stream_zip` body off the event loop onto a bounded thread (it releases no GIL of its own, but the deflate/AES C extensions do), exactly the `package/codec.md` GIL-release offload arm. The decode round-trip closes through `stream-unzip` (`CompressionAlgo.ZIP_STREAM` unpack), whose `allowed_encryption_mechanisms` allow-list refuses any record `stream_zip` does not have written (default `("none", "ae2", "aes256")` admits the page's own AE-2/AES-256 output and plaintext, rejecting legacy `ZIP_CRYPTO` and the weak `ae1`/`aes128`/`aes192` variants).

[RAIL_LAW]:
- Package: `stream-zip`
- Owns: bounded-memory streaming ZIP construction, per-member ZIP32/ZIP64 format selection and size/offset auto-upgrade, raw-deflate/stored compression, the `UT` extended-timestamp extra, WinZip AE-2/AES-256 encryption (PBKDF2/AES-CTR/HMAC-SHA1 via the `pycryptodome` backend), and streamed-content CRC32/uncompressed-size integrity verification
- Accept: streaming ZIP bundle assembly that yields ordered `bytes` chunks for file, response, or upload sinks, consuming lazy member data from the imaging/figure/document/structured-document producers, threaded through the `package/codec#CODEC` `CompressionAlgo.ZIP_STREAM` arm and the `_EPOCH`/`zlib_ng`-at-`level` reproducibility seam
- Reject: wrapper-renames of `stream_zip`/`async_stream_zip`; a `zipfile.ZipFile` whole-archive buffering path where streaming bounds memory; a hand-rolled ZIP64, `UT` extra, or WinZip-AES record layout; a hand-rolled AES where the `pycryptodome` backend already encrypts; a parallel member type per format or compression mode; a second function for the encrypted path the `password` row already owns; a native `Method` handle on the serialized profile where the `ZipMethod`/`_ZIP_METHOD` `(builder, ZipArity)` row resolves it; a `datetime.now()` member stamp where `_EPOCH` keeps the unencrypted container content-addressable; a hand-rolled extractor where `stream-unzip` owns the inverse

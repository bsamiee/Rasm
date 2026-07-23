# [PY_ARTIFACTS_API_STREAM_ZIP]

`stream-zip` mints the bounded-memory streaming ZIP construction surface for the artifacts bundle rail: `stream_zip` consumes an iterable of `MemberFile` tuples and yields the ZIP container as `bytes` chunks, the `Method` family selects per-member format and compression, and `password` switches WinZip AES-256 encryption across every member. `package/archive#ARCHIVE` binds it as the `CompressionAlgo.ZIP_STREAM` arm, encoding each member as it streams so no whole-archive buffer forms and the ZIP central-directory, ZIP64 extra-field, and WinZip-AES record layout stay `stream_zip`-owned.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `stream-zip`
- package: `stream-zip` (`MIT`)
- import: `stream_zip`
- owner: `artifacts`
- rail: bundle
- abi: pure-Python wheel, `py.typed`, no compiled extension
- depends: `pycryptodome` (WinZip-AES backend)
- entry points: import-only, no console script; the module `dir()` is the public surface (no `__all__`/`__version__`)
- capability: bounded-memory streaming ZIP construction yielding `Iterable[bytes]`; per-member ZIP32/ZIP64 selection with size/offset auto-upgrade; raw-deflate or stored compression; the Info-ZIP `UT` extended-timestamp extra; WinZip AES-256 (PBKDF2/AES-CTR/HMAC-SHA1) keyed by `password`; streamed CRC32/uncompressed-size integrity verification for pre-sized stored members; a thread-bridged `async_stream_zip` over async iterables

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: member tuples, method family, and integrity rails

`MemberFile` is the `(name, modified_at, mode, method, data)` tuple `stream_zip` consumes; `AsyncMemberFile` is its async-data twin. `Method` is abstract — its one `_get(offset, default_get_compressobj)` returns the 5-element `_MethodTuple` the encoder dispatches on. `ZIP_AUTO` and the `NO_COMPRESSION_*` instances are also callable: calling one mints a fresh anonymous `Method` binding the size/CRC, so each is both the buffered-mode method and the streamed-mode factory. `except ZipError` catches every failure, `except ZipValueError` every recoverable boundary fault.

| [INDEX] | [SYMBOL]            | [TYPE_FAMILY]   | [CAPABILITY]                                                               |
| :-----: | :------------------ | :-------------- | :------------------------------------------------------------------------- |
|  [01]   | `MemberFile`        | tuple alias     | `tuple[str, datetime, int, Method, Iterable[bytes]]`                       |
|  [02]   | `AsyncMemberFile`   | tuple alias     | `tuple[str, datetime, int, Method, AsyncIterable[bytes]]`                  |
|  [03]   | `Method`            | abstract base   | `Method(ABC)`; `_get(offset, default_get_compressobj) -> _MethodTuple`     |
|  [04]   | `ZIP_64`            | method instance | force ZIP64 member; deflate from `stream_zip`'s `get_compressobj`          |
|  [05]   | `ZIP_32`            | method instance | force ZIP32 member; deflate from `stream_zip`'s `get_compressobj`          |
|  [06]   | `ZIP_AUTO`          | callable method | size/offset ZIP32->ZIP64 auto-upgrade; binds own deflate at `level`        |
|  [07]   | `NO_COMPRESSION_32` | callable method | stored ZIP32 member (buffered instance, or pre-sized streamed when called) |
|  [08]   | `NO_COMPRESSION_64` | callable method | stored ZIP64 member (buffered instance, or pre-sized streamed when called) |

| [INDEX] | [SYMBOL]                                       | [TYPE_FAMILY]                     | [CAPABILITY]                               |
| :-----: | :--------------------------------------------- | :-------------------------------- | :----------------------------------------- |
|  [01]   | `ZipError`                                     | `Exception`                       | root of every `stream_zip` failure         |
|  [02]   | `ZipValueError`                                | `ZipError` + `ValueError`         | boundary base for integrity + overflow     |
|  [03]   | `ZipIntegrityError`                            | `ZipValueError`                   | declared-content mismatch base             |
|  [04]   | `CRC32IntegrityError`                          | `ZipIntegrityError`               | streamed CRC32 mismatch                    |
|  [05]   | `UncompressedSizeIntegrityError`               | `ZipIntegrityError`               | streamed uncompressed-size mismatch        |
|  [06]   | `ZipOverflowError`                             | `ZipValueError` + `OverflowError` | field-overflow base                        |
|  [07]   | `UncompressedSizeOverflowError`                | `ZipOverflowError`                | uncompressed size exceeds field width      |
|  [08]   | `CompressedSizeOverflowError`                  | `ZipOverflowError`                | compressed size exceeds field width        |
|  [09]   | `CentralDirectorySizeOverflowError`            | `ZipOverflowError`                | central-directory size exceeds field width |
|  [10]   | `OffsetOverflowError`                          | `ZipOverflowError`                | member offset exceeds ZIP32 field width    |
|  [11]   | `CentralDirectoryNumberOfEntriesOverflowError` | `ZipOverflowError`                | entry count exceeds ZIP32 field width      |
|  [12]   | `NameLengthOverflowError`                      | `ZipOverflowError`                | member name length exceeds field width     |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: streaming functions

`stream_zip` returns a lazy `Iterable[bytes]`; iterating drives `files` member by member so peak memory stays bounded by `chunk_size` and per-member buffering. Shared knobs `(files, chunk_size=65536, get_compressobj=<raw-deflate level 9>, extended_timestamps=True, password=None, get_crypto_random=<secrets.token_bytes>)`: `password` (a `str`, not `bytes`) switches every member to WinZip AES-256, `get_compressobj` overrides the deflate object for the `ZIP_64`/`ZIP_32`/stored formats, `get_crypto_random` the salt/IV byte source. `async_stream_zip` mirrors it over async iterables, mapping `AsyncIterable[AsyncMemberFile] -> AsyncIterable[bytes]`.

| [INDEX] | [SURFACE]          | [SHAPE] | [CAPABILITY]                                                                                             |
| :-----: | :----------------- | :------ | :------------------------------------------------------------------------------------------------------- |
|  [01]   | `stream_zip`       | fold    | stream a ZIP archive as ordered byte chunks                                                              |
|  [02]   | `async_stream_zip` | fold    | async mirror; runs sync `stream_zip` on a worker thread via `run_in_executor`/`run_coroutine_threadsafe` |

[ENTRYPOINT_SCOPE]: `Method` selection and parameterization

`ZIP_64`/`ZIP_32` drop into the `method` slot directly; `ZIP_AUTO` and the `NO_COMPRESSION_*` constants are called first to bind size/level or size/CRC, returning the `Method` the tuple carries. `ZIP_AUTO` alone flags the central directory for auto-upgrade.

| [INDEX] | [SURFACE]                                                | [SHAPE]  | [CAPABILITY]                                     |
| :-----: | :------------------------------------------------------- | :------- | :----------------------------------------------- |
|  [01]   | `ZIP_64`                                                 | instance | force ZIP64; deflate from `get_compressobj`      |
|  [02]   | `ZIP_32`                                                 | instance | force ZIP32; deflate from `get_compressobj`      |
|  [03]   | `ZIP_AUTO(uncompressed_size, level=9) -> Method`         | factory  | size/offset auto-upgrade; own deflate at `level` |
|  [04]   | `NO_COMPRESSION_32(uncompressed_size, crc_32) -> Method` | factory  | stored ZIP32; pre-declared size + CRC32          |
|  [05]   | `NO_COMPRESSION_64(uncompressed_size, crc_32) -> Method` | factory  | stored ZIP64; pre-declared size + CRC32          |

## [04]-[IMPLEMENTATION_LAW]

[BUNDLE_STREAMING_ZIP]:
- import: `from stream_zip import stream_zip, ZIP_64, ZIP_32, ZIP_AUTO, NO_COMPRESSION_32, NO_COMPRESSION_64` at boundary scope only; module-level import is banned by the manifest import policy.
- member axis: one `MemberFile` tuple `(name, modified_at, mode, method, data)` owns each entry; `name`/`modified_at`/`mode` are tuple slots, never a per-member builder type; `data` is a lazy `Iterable[bytes]` so member bytes never fully materialize. `modified_at` writes the DOS field and (under `extended_timestamps=True`) the `UT` Info-ZIP extra; `mode` writes the POSIX Unix external attributes.
- format axis: `method` selects the container format per member; `ZIP_AUTO` upgrades ZIP32 to ZIP64 past the `4293656841` size bound or the `0xffffffff` offset bound; `ZIP_64`/`ZIP_32` force the format; the `NO_COMPRESSION_*` rows store bytes uncompressed (`(size, crc_32)` streamed pre-sized, bare instance buffered). `package/archive#ARCHIVE` resolves a `ZipMethod` (`"auto"`/`"zip64"`/`"zip32"`/`"store32"`/`"store64"`) Literal through the `_zip_members` total `match` under an `assert_never` tail, so the serialized profile carries no native `Method` handle.
- compression axis: `get_compressobj` is the single deflate-object override for the `ZIP_64`/`ZIP_32`/stored formats, defaulting to raw-deflate `zlib.compressobj(wbits=-zlib.MAX_WBITS, level=9)`; `ZIP_AUTO(size, level)` ignores it and binds its own raw-deflate at the passed `level` with `memLevel=8`, so a uniform codec across every member sets both. `package/archive#ARCHIVE` binds `get_compressobj=lambda: zlib_ng.compressobj(...)` (the shared `data:compression` SIMD raw-DEFLATE) for the forced/stored formats and passes the same `level` into `ZIP_AUTO`, so one `level` axis governs every member while the `ZIP_AUTO` arm deflates on its own stdlib `zlib` object — a per-method codec fact the profile documents.
- encryption axis: `password` is the single WinZip AES-256 switch over every member; `get_crypto_random` overrides the 16-byte salt + counter byte source; encryption is a function-level row, never a per-member type or a second function. PBKDF2 (1000 iterations) derives the AE-2/AES-256 AES key + HMAC key + 2-byte verifier, AES-CTR (128-bit little-endian counter) encrypts, and an HMAC-SHA1 tag truncated to 10 bytes authenticates — all via the `pycryptodome` `AES`/`HMAC`/`SHA1`/`PBKDF2` primitives.
- integrity axis: streamed `NO_COMPRESSION_*` members carry a declared `(uncompressed_size, crc_32)` the encoder verifies against the streamed bytes, raising `CRC32IntegrityError`/`UncompressedSizeIntegrityError`; a ZIP32 field overflow raises the `ZipOverflowError` subtree. A fault rail catches `ZipValueError` for recoverable boundary faults and `ZipError` for the root.
- evidence: each archive captures member count, per-member name, declared method, ZIP32-vs-ZIP64 resolution, encryption flag, and emitted byte length as the `package/codec#CODEC` `BundleEvidence` projection onto `core/receipt#RECEIPT` `ArtifactReceipt.Bundle` — `entries` the member arity, `verified` the streamed-integrity member count.
- boundary: `stream_zip` owns ZIP container framing, ZIP64 local + central extra fields, the `UT` extended-timestamp extra, and WinZip-AES records (via `pycryptodome`); `async_stream_zip` is the async-iterable boundary over the same encoder, loop-bridged onto a worker thread; downstream sinks consume the `bytes` chunks straight to a file, response body, or upload stream; archive extraction routes to `stream-unzip`.

[STACKING]:
- `package/archive#ARCHIVE` over the `package/codec#CODEC` union owns the call site: `CompressionAlgo.ZIP_STREAM` is the one `match` arm binding `stream_zip`/`async_stream_zip`, reading the `zip_stream` `ZipStreamKnobs` `CodecProfile` case (`method`/`level`/`password`/`names`/`modified_at`/`mode`). `_zip_members` folds each payload into a `(name, modified_at, mode, method, iter((payload,)))` `MemberFile` whose `method` a total `match` over the `ZipMethod` Literal selects, and `stream_zip(members, password=..., get_compressobj=...)` yields the ordered chunks the archive owner hands straight to its file, response, or upload sink — the `Iterable[bytes]` is the product, a `b"".join` materialization surviving only inside a bounded test fixture.
- Byte-reproducibility is the content-addressing seam: the design fixes `modified_at` to `_EPOCH` (`1980-01-01`, the ZIP epoch) and the deflate to a fixed `level` on a deterministic codec per method row — `zlib_ng` where the hook is read, `ZIP_AUTO`'s own stdlib raw-deflate on the auto row — so an unencrypted bundle of identical payloads at one profile yields identical bytes run-to-run and dedups by the runtime content key. An encrypted bundle is intentionally non-reproducible: `get_crypto_random` defaults to `secrets.token_bytes`, freshly randomizing the AES salt/IV per pack, and accepts a deterministic byte source for reproducible-fixture AES-path tests.
- `ZIP_AUTO(uncompressed_size, level)` reads the per-member size the producer already knows (a `msgspec.msgpack.encode` length, a `pikepdf`/`pymupdf` page estimate, a `segno` QR buffer length), so the ZIP32-vs-ZIP64 decision is data-driven; and because `data` is a lazy `Iterable[bytes]`, a `tomlkit.dumps(...)` document, an `svgelements`-composed SVG, or a `weasyprint`/`pymupdf` PDF stream feeds straight into a member while `stream_zip` interleaves encode-and-emit.
- Universal-rail tier: the per-member triples fold into the codec page's `ContentKey` over the shared `xxhash` digest (`xxh3_128`/`XXH3`); the bundle receipt fields cross into `core/receipt#RECEIPT` and ride the shared `structlog` + OpenTelemetry span; `async_stream_zip` is consumed on the shared `anyio` structured-concurrency rail, and because it is already loop-bridged through a thread executor the design offloads the synchronous body off the event loop onto a bounded thread (the deflate/AES C extensions release the GIL), the `package/codec.md` GIL-release offload arm. `stream-unzip` closes the decode round-trip, its `allowed_encryption_mechanisms` allow-list deriving from the one password discriminant — plaintext-only when unset, the AE-2/AES-256 pair this encoder writes when set — rejecting legacy `ZIP_CRYPTO` and the weak `ae1`/`aes128`/`aes192` variants structurally.

[RAIL_LAW]:
- Package: `stream-zip`
- Owns: bounded-memory streaming ZIP construction, per-member ZIP32/ZIP64 format selection and size/offset auto-upgrade, raw-deflate/stored compression, the `UT` extended-timestamp extra, WinZip AE-2/AES-256 encryption (PBKDF2/AES-CTR/HMAC-SHA1 via `pycryptodome`), and streamed CRC32/uncompressed-size integrity verification
- Accept: streaming ZIP bundle assembly yielding ordered `bytes` chunks for file, response, or upload sinks, consuming lazy member data from the imaging/figure/document/structured-document producers through the `package/codec#CODEC` `CompressionAlgo.ZIP_STREAM` arm and the `_EPOCH`/`zlib_ng`-at-`level` reproducibility seam
- Reject: a wrapper-rename of `stream_zip`/`async_stream_zip`; a `zipfile.ZipFile` whole-archive buffering path where streaming bounds memory; a hand-rolled ZIP64, `UT` extra, or WinZip-AES record layout; a hand-rolled AES where `pycryptodome` encrypts; a parallel member type per format or compression mode; a second function for the encrypted path the `password` row owns; a native `Method` handle on the serialized profile where the `ZipMethod` Literal and `_zip_members` match resolve it; a `datetime.now()` member stamp where `_EPOCH` keeps the unencrypted container content-addressable; a hand-rolled extractor where `stream-unzip` owns the inverse

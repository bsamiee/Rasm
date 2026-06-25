# [PY_ARTIFACTS_API_STREAM_ZIP]

`stream-zip` supplies the bounded-memory streaming ZIP surface for the artifacts bundle rail: a generator `stream_zip` that consumes an iterable of `MemberFile` tuples and yields ZIP container `bytes` chunk by chunk, plus a `Method` family (`ZIP_64`, `ZIP_32`, `ZIP_AUTO`, `NO_COMPRESSION_32`, `NO_COMPRESSION_64`) that selects per-member format, ZIP64 upgrade, and compression, and AES-256 encryption keyed by `password`. The package owner composes `stream_zip` and its `Method` constants into the bundle assembly path; it removes the `zipfile.ZipFile` whole-archive buffering leak because every member is encoded as it streams, and it never re-implements the ZIP central-directory, ZIP64 extra-field, or WinZip AES record layout `stream_zip` already owns.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `stream-zip`
- package: `stream-zip`
- import: `stream_zip`
- owner: `artifacts`
- rail: bundle
- asset: pure-Python runtime library (no native build; `py3-none-any` wheel); hard runtime dependency `pycryptodome>=3.10.1` (the AES-256/HMAC-SHA1/PBKDF2 WinZip-AES backend imported as `AES`/`HMAC`/`SHA1`/`PBKDF2`)
- installed: `0.0.84` reflected via `importlib.metadata.version('stream-zip')` on cp315
- entry points: library use is import-only; no console script
- capability: bounded-memory streaming ZIP construction yielding `Iterable[bytes]`, per-member ZIP32/ZIP64 format selection with automatic upgrade, raw-deflate or stored (uncompressed buffered/streamed) compression, extended Unix timestamps (`UT`/`ux`/`Info-ZIP NTFS`), WinZip AES-256 encryption keyed by `password`, streamed-content CRC32/size integrity verification, and a mirrored `async_stream_zip` over async iterables

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: member tuples, method family, and integrity rails
- rail: bundle

`MemberFile` is the `(name, modified_at, mode, method, data)` tuple `stream_zip` consumes; `AsyncMemberFile` is its async-data twin. `Method` is the abstract base whose public instances (`ZIP_64`, `ZIP_32`, `ZIP_AUTO`, `NO_COMPRESSION_32`, `NO_COMPRESSION_64`) choose format and compression; `ZIP_AUTO` and the `NO_COMPRESSION_*` instances are also callable to parameterize size/level. The error tree is a single rooted hierarchy: `ZipError` is the root, `ZipValueError(ZipError, ValueError)` is the boundary base, `ZipIntegrityError(ZipValueError)` and `ZipOverflowError(ZipValueError, OverflowError)` are the two failure families, and every concrete failure descends from one of them — one `except ZipError` catches all, `except ZipValueError` catches every recoverable boundary failure.

| [INDEX] | [SYMBOL]                                       | [TYPE_FAMILY]   | [RAIL]                                                            |
| :-----: | :--------------------------------------------- | :-------------- | :--------------------------------------------------------------- |
|  [01]   | `MemberFile`                                   | tuple alias     | `tuple[str, datetime, int, Method, Iterable[bytes]]`             |
|  [02]   | `AsyncMemberFile`                              | tuple alias     | `tuple[str, datetime, int, Method, AsyncIterable[bytes]]`        |
|  [03]   | `Method`                                       | abstract base   | `ABCMeta` per-member format/compression selector contract        |
|  [04]   | `ZIP_64`                                       | method instance | force ZIP64 format member                                        |
|  [05]   | `ZIP_32`                                       | method instance | force ZIP32 format member                                        |
|  [06]   | `ZIP_AUTO`                                     | callable method | size-driven ZIP32/ZIP64 auto-upgrade member                      |
|  [07]   | `NO_COMPRESSION_32`                            | callable method | stored ZIP32 member (buffered or pre-sized streamed)             |
|  [08]   | `NO_COMPRESSION_64`                            | callable method | stored ZIP64 member (buffered or pre-sized streamed)             |
|  [09]   | `ZipError`                                     | error root      | `Exception` base for every `stream_zip` failure                  |
|  [10]   | `ZipValueError`                                | error           | `ZipError` + `ValueError`; boundary base for integrity+overflow  |
|  [11]   | `ZipIntegrityError`                            | error           | `ZipValueError`; declared-content mismatch base                  |
|  [12]   | `CRC32IntegrityError`                          | error           | `ZipIntegrityError`; streamed CRC32 mismatch                     |
|  [13]   | `UncompressedSizeIntegrityError`               | error           | `ZipIntegrityError`; streamed uncompressed-size mismatch         |
|  [14]   | `ZipOverflowError`                             | error           | `ZipValueError` + `OverflowError`; field-overflow base           |
|  [15]   | `UncompressedSizeOverflowError`                | error           | `ZipOverflowError`; uncompressed size exceeds field width        |
|  [16]   | `CompressedSizeOverflowError`                  | error           | `ZipOverflowError`; compressed size exceeds field width          |
|  [17]   | `CentralDirectorySizeOverflowError`            | error           | `ZipOverflowError`; central-directory size exceeds field width   |
|  [18]   | `OffsetOverflowError`                          | error           | `ZipOverflowError`; member offset exceeds ZIP32 field width      |
|  [19]   | `CentralDirectoryNumberOfEntriesOverflowError` | error           | `ZipOverflowError`; entry count exceeds ZIP32 field width        |
|  [20]   | `NameLengthOverflowError`                      | error           | `ZipOverflowError`; member name length exceeds field width       |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: streaming functions
- rail: bundle

`stream_zip` returns a lazy `Iterable[bytes]`; iterating drives `files` member by member so peak memory stays bounded by `chunk_size` and per-member buffering. `password` switches every member to WinZip AES-256; `get_compressobj` overrides the default deflate object and `get_crypto_random` overrides the encryption salt/IV source. `async_stream_zip` mirrors the signature over async iterables.

| [INDEX] | [SURFACE]          | [CALL_SHAPE]                                                                                                                                                                                                                                                                                         | [CAPABILITY]                                |
| :-----: | :----------------- | :--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | :------------------------------------------ |
|  [01]   | `stream_zip`       | `stream_zip(files: Iterable[MemberFile], chunk_size: int=65536, get_compressobj=lambda: zlib.compressobj(wbits=-zlib.MAX_WBITS, level=9), extended_timestamps: bool=True, password: Optional[str]=None, get_crypto_random=lambda n: secrets.token_bytes(n)) -> Iterable[bytes]`                      | stream a ZIP archive as ordered byte chunks |
|  [02]   | `async_stream_zip` | `async_stream_zip(files: AsyncIterable[AsyncMemberFile], chunk_size: int=65536, get_compressobj=lambda: zlib.compressobj(wbits=-zlib.MAX_WBITS, level=9), extended_timestamps: bool=True, password: Optional[str]=None, get_crypto_random=lambda n: secrets.token_bytes(n)) -> AsyncIterable[bytes]` | async mirror over async member data         |

[ENTRYPOINT_SCOPE]: `Method` selection and parameterization
- rail: bundle

`ZIP_64`/`ZIP_32` are used directly as the `method` slot of a `MemberFile`. `ZIP_AUTO` and the `NO_COMPRESSION_*` constants are called first to bind size/level, returning a `Method` to place in the tuple.

| [INDEX] | [SURFACE]           | [CALL_SHAPE]                                                       | [CAPABILITY]                                                    |
| :-----: | :------------------ | :----------------------------------------------------------------- | :-------------------------------------------------------------- |
|  [01]   | `ZIP_64`            | used as `method` directly                                          | force ZIP64 member with default deflate                         |
|  [02]   | `ZIP_32`            | used as `method` directly                                          | force ZIP32 member with default deflate                         |
|  [03]   | `ZIP_AUTO`          | `ZIP_AUTO(uncompressed_size: int, level: int=9) -> Method`         | pick ZIP64 when size/offset exceeds the ZIP32 bound, else ZIP32 |
|  [04]   | `NO_COMPRESSION_32` | `NO_COMPRESSION_32(uncompressed_size: int, crc_32: int) -> Method` | stored ZIP32 member streamed with a pre-declared size and CRC32 |
|  [05]   | `NO_COMPRESSION_64` | `NO_COMPRESSION_64(uncompressed_size: int, crc_32: int) -> Method` | stored ZIP64 member streamed with a pre-declared size and CRC32 |

## [04]-[IMPLEMENTATION_LAW]

[BUNDLE_STREAMING_ZIP]:
- import: `from stream_zip import stream_zip, ZIP_64, ZIP_32, ZIP_AUTO, NO_COMPRESSION_32, NO_COMPRESSION_64` at boundary scope only; module-level import is banned by the manifest import policy.
- member axis: one `MemberFile` tuple `(name, modified_at, mode, method, data)` owns each archive entry; `name`/`modified_at`/`mode` are tuple slots, never a per-member builder type; `data` is a lazy `Iterable[bytes]` so member bytes never fully materialize.
- format axis: `method` selects the container format per member; `ZIP_AUTO` is the size-driven row that upgrades ZIP32 to ZIP64 only past the 0xffffffff bound; `ZIP_64`/`ZIP_32` force the format; the `NO_COMPRESSION_*` rows store bytes uncompressed (`(size, crc_32)` for streamed pre-sized members), never a parallel archive type.
- compression axis: `get_compressobj` is the single deflate-object override row; the default yields a raw-deflate `zlib.compressobj(wbits=-zlib.MAX_WBITS, level=9)`; `ZIP_AUTO(size, level)` binds the level per member; stored mode routes through the `NO_COMPRESSION_*` methods, never a flag on the function.
- encryption axis: `password` is the single AES-256 switch applied to every member; `get_crypto_random` overrides the salt/IV byte source; encryption is a function-level row, never a per-member type or a second function.
- integrity axis: streamed `NO_COMPRESSION_*` members carry a declared `(uncompressed_size, crc_32)` that the encoder verifies, raising `CRC32IntegrityError`/`UncompressedSizeIntegrityError` (both under `ZipIntegrityError` -> `ZipValueError`); ZIP32 field overflows raise the `ZipOverflowError` subtree (under `ZipValueError`); every failure descends from `ZipError`, so a fault rail catches `ZipValueError` for recoverable boundary faults and `ZipError` for the root.
- evidence: each archive captures member count, per-member name, declared method, ZIP32-vs-ZIP64 resolution, encryption flag, and emitted byte length as a bundle receipt.
- boundary: `stream_zip` owns ZIP container framing, ZIP64 extra fields, extended Unix timestamps, and WinZip AES records (via `pycryptodome` AES-256/HMAC-SHA1/PBKDF2); `async_stream_zip` is the async-iterable boundary over the same encoder; downstream sinks consume the `bytes` chunks directly to a file, response body, or upload stream; archive extraction routes to `stream-unzip` (the gated inverse), not this package.

[STACKING]:
- Each `MemberFile.data` is a lazy `Iterable[bytes]`, so a `segno` QR `save(io, kind=...)` output, a `svgelements`-composed SVG, a `tomlkit` `dumps(...)` document, or a `pymupdf`/`weasyprint` PDF stream feeds straight into a member without buffering the whole file — the bundle owner yields `(name, modified_at, mode, ZIP_AUTO(size), member_bytes_iter)` tuples lazily and `stream_zip` interleaves encode-and-emit.
- `ZIP_AUTO(uncompressed_size, level)` reads the per-member size the artifacts producer already knows (e.g. a `msgspec.msgpack.encode` length or a `pikepdf`/`pymupdf` page count estimate) to pick ZIP32 vs ZIP64 deterministically, so the format decision is data-driven, not a post-hoc overflow catch.
- `stream-unzip` (`python_version<'3.15'`, gated) is the streamed inverse: it consumes the same chunked `bytes` an upload or download yields and re-emits `(name, size, data_iter)`, closing the bundle round-trip without a `zipfile` whole-archive buffer.
- `get_crypto_random` accepts a deterministic byte source for reproducible-fixture testing of the AES salt/IV path; production passes the default `secrets.token_bytes`.

[RAIL_LAW]:
- Package: `stream-zip`
- Owns: bounded-memory streaming ZIP construction, per-member ZIP32/ZIP64 format selection and auto-upgrade, raw-deflate/stored compression, extended timestamps, WinZip AES-256 encryption (pycryptodome backend), and streamed-content CRC32/size integrity verification
- Accept: streaming ZIP bundle assembly that yields ordered `bytes` chunks for file, response, or upload sinks, consuming lazy member data from the imaging/figure/document/structured-document producers
- Reject: wrapper-renames of `stream_zip`/`async_stream_zip`; a `zipfile.ZipFile` whole-archive buffering path where streaming bounds memory; a hand-rolled ZIP64 or WinZip AES record layout; a hand-rolled AES where the `pycryptodome` backend already encrypts; a parallel member type per format or compression mode; a second function for the encrypted path the `password` row already owns; a hand-rolled extractor where `stream-unzip` owns the inverse

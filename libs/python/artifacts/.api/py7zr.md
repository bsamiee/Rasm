# [PY_ARTIFACTS_API_PY7ZR]

`py7zr` owns 7z container read/write/test/list for the artifacts compression rail: one `SevenZipFile` root drives a `FILTER_*` codec-chain table with DELTA/BCJ pre-filters, AES256 header+content encryption, and a `WriterFactory`/`Py7zIO` streamed-sink protocol that decodes each entry into a capped, hashing, or discarding sink under a `max_extract_size` bomb bound. One compression owner composes the root, codec table, sink factories, and shutil hooks over the 7z container format `py7zr` already owns and never re-implements.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `py7zr`
- package: `py7zr` (LGPL-2.1-or-later)
- import: `py7zr`
- owner: `artifacts`
- rail: compression
- entry points: console script `py7zr` (list/extract/create/test/append); library use is import-only
- capability: 7z archive read/write/test/list, multi-codec filter chains with DELTA/BCJ pre-filters, AES256 header+content encryption, password protection, streamed per-entry extraction sinks (`WriterFactory`/`Py7zIO`), `max_extract_size` decompression-bomb defense, `ExtractCallback`/`ArchiveCallback` progress mirrors, multiprocessing decode (`mp=True`), shutil archive-format registration

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: archive root, info records, and io protocol
- rail: compression

| [INDEX] | [SYMBOL]                    | [PACKAGE_ROLE] | [CAPABILITY]                                                                              |
| :-----: | :-------------------------- | :------------- | :---------------------------------------------------------------------------------------- |
|  [01]   | `SevenZipFile`              | archive root   | open/read/write/test a 7z archive; context-manager; `reporter` + nested `ParseStatus`     |
|  [02]   | `ArchiveInfo`               | archive view   | `@dataclass` container metadata (`archiveinfo()`); fields below                           |
|  [03]   | `FileInfo`                  | entry view     | `@dataclass` per-entry metadata (`list()` row); fields below                              |
|  [04]   | `WriterFactory`             | sink factory   | ABC: `create(filename) -> Py7zIO`; per-entry streamed sink                                |
|  [05]   | `Py7zIO`                    | streamed sink  | `write`/`read`/`seek`/`size`/`flush`/`close` per-entry IO target                          |
|  [06]   | `callbacks.ExtractCallback` | extract hooks  | `report_{start_preparation,start,update,postprocess,end,warning}`                         |
|  [07]   | `callbacks.ArchiveCallback` | write hooks    | mirror of `ExtractCallback` on write                                                      |
|  [08]   | `io.BytesIOFactory`         | built-in sink  | extract into a capped in-memory buffer `BytesIOFactory(limit)`; `get(name)` after extract |
|  [09]   | `io.HashIOFactory`          | built-in sink  | streaming hash IO (the content-identity seam)                                             |
|  [10]   | `io.NullIOFactory`          | built-in sink  | discard sink                                                                              |

- [02]-[ARCHIVEINFO]: `filename`, `stat`, `header_size`, `method_names`, `solid`, `blocks`, `uncompressed`.
- [03]-[FILEINFO]: `filename`, `compressed`, `uncompressed`, `archivable`, `is_directory`, `is_file`, `is_symlink`, `creationtime`, `crc32`.

[PUBLIC_TYPE_SCOPE]: filter codec, preset, and integrity-check table
- rail: compression

Each row is a `FILTER_*` constant (prefix dropped below) placed in a `filters=[{'id': FILTER_…, 'preset': PRESET_…}]` chain; presets are `PRESET_*`, checks `CHECK_*`.

| [INDEX] | [SYMBOL_FAMILY]                                               | [PACKAGE_ROLE]  | [CAPABILITY]                          |
| :-----: | :------------------------------------------------------------ | :-------------- | :------------------------------------ |
|  [01]   | `LZMA` / `LZMA2`                                              | primary codec   | default LZMA family compression       |
|  [02]   | `BZIP2` / `PPMD` / `ZSTD` / `BROTLI` / `DEFLATE`              | alt codec       | alternate compression codecs          |
|  [03]   | `COPY`                                                        | store           | store-only (no compression)           |
|  [04]   | `DELTA`                                                       | preprocessor    | delta pre-filter for incremental data |
|  [05]   | `X86` / `ARM` / `ARMTHUMB` / `POWERPC` / `SPARC` / `IA64`     | BCJ pre-filter  | branch-convert executable code        |
|  [06]   | `CRYPTO_AES256_SHA256`                                        | crypto filter   | AES256 content/header encryption      |
|  [07]   | `PRESET_DEFAULT` / `PRESET_EXTREME`                           | preset          | LZMA preset level                     |
|  [08]   | `CHECK_CRC32` / `CHECK_CRC64` / `CHECK_SHA256` / `CHECK_NONE` | integrity check | per-chunk integrity-check selector    |

[PUBLIC_TYPE_SCOPE]: faults
- rail: compression

`exceptions.ArchiveError` roots the fault tree; every fault below subclasses it, so a boundary maps the precise subtype over a bare `Exception` and falls back to `ArchiveError`.

| [INDEX] | [SYMBOL]                            | [PACKAGE_ROLE]  | [CAPABILITY]                                                           |
| :-----: | :---------------------------------- | :-------------- | :--------------------------------------------------------------------- |
|  [01]   | `ArchiveError`                      | fault base      | base of every py7zr failure                                            |
|  [02]   | `Bad7zFile`                         | format fault    | corrupt or invalid 7z container                                        |
|  [03]   | `PasswordRequired`                  | auth fault      | encrypted archive opened without a password                            |
|  [04]   | `DecompressionError`                | codec fault     | a decode step failed                                                   |
|  [05]   | `CrcError`                          | integrity fault | a per-entry CRC check mismatched on extract                            |
|  [06]   | `DecompressionBombError`            | safety fault    | extracted size exceeded `max_extract_size` (zip-bomb guard)            |
|  [07]   | `AbsolutePathError`                 | safety fault    | an entry name resolved outside the extract root (path-traversal guard) |
|  [08]   | `UnsupportedCompressionMethodError` | codec fault     | an unimplemented filter was encountered                                |
|  [09]   | `InternalError`                     | invariant fault | a py7zr internal-state invariant broke                                 |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: archive open, read, and write
- rail: compression

`SevenZipFile(file, mode='r', *, filters=None, dereference=False, password=None, header_encryption=False, blocksize=None, mp=False, max_extract_size=None)` opens or creates; the rows below are its methods, and `extract`/`extractall` share the `*, callback=None, factory=None` sink kwargs (the `…` tail). `archiveinfo()` faults on a `BytesIO`-opened archive — sum `FileInfo.uncompressed` there instead.

| [INDEX] | [CALL_SHAPE]                                           | [CAPABILITY]                                           |
| :-----: | :----------------------------------------------------- | :----------------------------------------------------- |
|  [01]   | `SevenZipFile(file, mode='r', …)`                      | open/create an archive; `max_extract_size` caps a bomb |
|  [02]   | `extractall(path=None, …)`                             | extract every entry                                    |
|  [03]   | `extract(path=None, targets=None, recursive=False, …)` | extract selected entries / into a `WriterFactory` sink |
|  [04]   | `writeall(path, arcname=None)`                         | add a tree to the archive                              |
|  [05]   | `write(file, arcname=None)`                            | add one file                                           |
|  [06]   | `writestr(data, arcname)`                              | add in-memory bytes/str                                |
|  [07]   | `writef(bio, arcname)`                                 | add from a file-like object                            |
|  [08]   | `list`/`getnames`/`namelist`/`getinfo`                 | enumerate entries (listing, names, single `FileInfo`)  |
|  [09]   | `test`/`testzip`                                       | verify CRCs                                            |
|  [10]   | `needs_password`/`archiveinfo`/`reset`                 | encryption probe, `ArchiveInfo`, rewind cursor         |

[ENTRYPOINT_SCOPE]: predicate and shutil registration
- rail: compression

| [INDEX] | [CALL_SHAPE]                             | [CAPABILITY]                                                                    |
| :-----: | :--------------------------------------- | :------------------------------------------------------------------------------ |
|  [01]   | `is_7zfile(file)`                        | format predicate over a path/stream                                             |
|  [02]   | `pack_7zarchive(base_name, base_dir, …)` | shutil pack hook: `register_archive_format('7zip', pack_7zarchive)`             |
|  [03]   | `unpack_7zarchive(archive, path, …)`     | shutil unpack hook: `register_unpack_format('7zip', ['.7z'], unpack_7zarchive)` |

## [04]-[IMPLEMENTATION_LAW]

[COMPRESSION_7Z]:
- import: `import py7zr` at boundary scope only; module-level import banned by the manifest import policy.
- archive axis: one `SevenZipFile` owns read/write/test under a `mode` row (`r`/`w`/`a`/`x`); `mp=True` drives multiprocessing decode, `blocksize` sizes the read buffer, `dereference=True` follows symlinks on write — never a codec-per-archive type.
- codec axis: the `FILTER_*` table sources the filter chain, a `list[dict]` (`[{'id': FILTER_LZMA2, 'preset': PRESET_DEFAULT}]`); DELTA and BCJ (`X86`/`ARM`/…) pre-filters prepend the codec stage and `CHECK_*` selects the per-chunk integrity check — never a parallel archive owner per codec.
- crypto axis: `password=` with `header_encryption=True` and a `FILTER_CRYPTO_AES256_SHA256` chain entry encrypt content and header; `set_encrypted_header(True)`/`set_encoded_header_mode(…)` tune header protection — never a separate encryptor.
- streamed-extraction axis: `extract`/`extractall` take a `WriterFactory` whose `create(name) -> Py7zIO` returns a per-entry sink, so a large entry decodes straight into a capped `io.BytesIOFactory(limit)` (`get(name)` retrieves the buffer), a streaming `io.HashIOFactory()` (content-identity seam), a discarding `io.NullIOFactory`, or a custom sink — never a temp-file round-trip; `ExtractCallback` rides the same call for progress and `ArchiveCallback` mirrors it on write.
- safety axis: `max_extract_size=` bounds total decoded size and raises `DecompressionBombError`; a traversal entry name raises `AbsolutePathError`; a CRC mismatch raises `CrcError` — the zip-bomb, path-traversal, and corruption guards on an untrusted upload, mapped at the boundary, never a post-extract re-check.
- evidence: each op captures entry count (`list()` length), the `FileInfo` per-entry fields (`compressed`/`uncompressed`/`crc32`/`is_directory`/`is_symlink`/`creationtime`), the `ArchiveInfo` container fields (`method_names`/`solid`/`blocks`/`header_size`), encryption + header-encryption state, integrity-check kind, and the `max_extract_size` bound as a compression receipt.
- boundary: `py7zr` owns the 7z container; raw single-stream codecs route to `zstandard`/`lz4`/`brotli`, zip/tar to `shutil`/stdlib; entry hashing for content identity rides the `HashIOFactory` sink; live UI stays outside this package.

[STACKING]:
- `stream-zip`(`.api/stream-zip.md`)/`stream-unzip`(`.api/stream-unzip.md`): `SevenZipFile` is the 7z arm of the archive-container rail beside the ZIP-stream arm — the `CompressionAlgo.SEVEN_Z` `match` case against `CompressionAlgo.ZIP_STREAM`, one container row each, never a shared archive class.
- `zstandard`(`.api/zstandard.md`)/`brotli`(`.api/brotli.md`)/`lz4`: the raw single-stream codec split under the same `CompressionAlgo` discriminant — a lone payload routes to a raw codec, a multi-entry container to `py7zr`; `py7zr`'s own `FILTER_ZSTD`/`FILTER_BROTLI` chain entries compress inside the 7z container, distinct from the sibling raw-frame owners.
- within-lib rail: the extract/write body offloads off the event loop through `anyio.to_thread.run_sync` (`mp=True` spanning subprocess decode) onto the shared `expression.Result[…, ArchiveError]` rail, so an `ArchiveError` subtype becomes an `Error` case at the seam.
- within-lib identity: the `HashIOFactory` sink feeds the shared `xxhash.xxh3_128` `ContentIdentity.key` fold — the same 16-byte digest the `stream-unzip` member arm yields, so a 7z entry's content key stays uniform across every codec path.
- within-lib receipt: the entry/`FileInfo`/`ArchiveInfo` evidence with encryption and integrity-check kind folds into the `core/receipt#RECEIPT` `ArtifactReceipt` case over the shared `structlog`/OpenTelemetry span.

[RAIL_LAW]:
- Package: `py7zr`
- Owns: 7z archive read/write/test/list, multi-codec filter chains with DELTA/BCJ pre-filters, AES256 header+content encryption, streamed per-entry extraction sinks, the `exceptions.ArchiveError` fault tree, `max_extract_size` + `AbsolutePathError` bomb/traversal defense, multiprocessing decode, shutil registration
- Accept: the 7z container arm of the archive rail feeding the compression and export-bundle owners; `HashIOFactory` entry digests feeding the content-identity rail; a `max_extract_size`-bounded extract of an untrusted upload mapping `DecompressionBombError`/`AbsolutePathError`/`CrcError` at the boundary
- Reject: wrapper-renames of `extractall`/`writeall`; a codec-per-archive type where a `FILTER_*` row suffices; a temp-file extract round-trip where a `WriterFactory` sink streams; an unbounded extract of untrusted bytes where `max_extract_size` guards; an `ArchiveError` subtype swallowed into a bare `Exception`; identity minting the runtime owns

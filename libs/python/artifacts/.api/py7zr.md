# [PY_ARTIFACTS_API_PY7ZR]

`py7zr` is the 7z archive owner for the artifacts compression rail. It exposes a `SevenZipFile` archive root, `ArchiveInfo`/`FileInfo` records, a `FILTER_*` codec table (LZMA/LZMA2/BZIP2/PPMD/ZSTD/BROTLI/DEFLATE/COPY plus DELTA and BCJ pre-filters), AES256 header/content encryption, an `ExtractCallback` progress protocol, a `WriterFactory`/`Py7zIO` streamed-sink protocol (with built-in `BytesIOFactory`/`HashIOFactory`), and shutil registration hooks that drive read/write/test of 7z containers. The package owner composes `SevenZipFile`, the `FILTER_*` table, the streamed-sink factories, and the shutil hooks into the compression owner; it never re-implements the 7z container format py7zr already owns.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `py7zr`
- package: `py7zr`
- import: `py7zr`
- owner: `artifacts`
- rail: compression
- installed: `1.1.3`
- license: `LGPL-2.1-or-later` (weak copyleft; dynamic import imposes no source obligation on the consumer, but redistribution of a modified `py7zr` must stay LGPL)
- entry points: console script `py7zr` (CLI: list/extract/create/test/append); library use is import-only
- capability: 7z archive read/write/test/list, multi-codec filter chains with DELTA/BCJ pre-filters, AES256 header+content encryption, password protection, streamed per-entry extraction sinks (`WriterFactory`/`Py7zIO`), bounded-size decompression-bomb defense (`max_extract_size`), `ExtractCallback`/`ArchiveCallback` progress mirrors, multiprocessing decode (`mp=True`), shutil archive-format registration

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: archive root, info records, and io protocol
- rail: compression

| [INDEX] | [SYMBOL]            | [PACKAGE_ROLE]   | [CAPABILITY]                                              |
| :-----: | :------------------ | :--------------- | :------------------------------------------------------- |
|  [01]   | `SevenZipFile`      | archive root     | open/read/write/test a 7z archive; context-manager; carries `reporter` and a nested `ParseStatus` |
|  [02]   | `ArchiveInfo`       | archive view     | `@dataclass` container metadata (`archiveinfo()` return): `filename`, `stat`, `header_size`, `method_names`, `solid`, `blocks`, `uncompressed` |
|  [03]   | `FileInfo`          | entry view       | `@dataclass` per-entry metadata (`list()` row): `filename`, `compressed`, `uncompressed`, `archivable`, `is_directory`, `is_file`, `is_symlink`, `creationtime`, `crc32` |
|  [04]   | `WriterFactory`     | sink factory     | ABC: `create(filename) -> Py7zIO`; per-entry streamed sink |
|  [05]   | `Py7zIO`            | streamed sink    | `write`/`read`/`seek`/`size`/`flush`/`close` per-entry IO target |
|  [06]   | `callbacks.ExtractCallback` / `ArchiveCallback` | progress hooks | extract/write progress: `report_start_preparation`/`report_start`/`report_update`/`report_postprocess`/`report_end`/`report_warning` |
|  [07]   | `io.BytesIOFactory` / `HashIOFactory` / `NullIOFactory` | built-in sinks | extract into a capped in-memory buffer (`BytesIOFactory(limit)`) / streaming hash IO (`HashIOFactory()`) / discard sink; `get(name)` retrieves the populated sink after extraction |

[PUBLIC_TYPE_SCOPE]: filter codec, preset, and integrity-check table
- rail: compression

Filter rows are filter-chain vocabulary consumed as `filters=[{ 'id': FILTER_..., 'preset': PRESET_... }]` list values.

| [INDEX] | [SYMBOL_FAMILY]                                               | [PACKAGE_ROLE] | [CAPABILITY]                                  |
| :-----: | :----------------------------------------------------------- | :------------- | :-------------------------------------------- |
|  [01]   | `FILTER_LZMA` / `FILTER_LZMA2`                               | primary codec  | default LZMA family compression               |
|  [02]   | `FILTER_BZIP2` / `FILTER_PPMD` / `FILTER_ZSTD` / `FILTER_BROTLI` / `FILTER_DEFLATE` | alt codec | alternate compression codecs        |
|  [03]   | `FILTER_COPY`                                                | store          | store-only (no compression)                   |
|  [04]   | `FILTER_DELTA`                                               | preprocessor   | delta pre-filter for incremental data         |
|  [05]   | `FILTER_X86` / `FILTER_ARM` / `FILTER_ARMTHUMB` / `FILTER_POWERPC` / `FILTER_SPARC` / `FILTER_IA64` | BCJ pre-filter | branch-convert executable code |
|  [06]   | `FILTER_CRYPTO_AES256_SHA256`                               | crypto filter  | AES256 content/header encryption              |
|  [07]   | `PRESET_DEFAULT` / `PRESET_EXTREME`                         | preset         | LZMA preset level                             |
|  [08]   | `CHECK_CRC32` / `CHECK_CRC64` / `CHECK_SHA256` / `CHECK_NONE` | integrity check | per-chunk integrity-check selector          |

[PUBLIC_TYPE_SCOPE]: faults
- rail: compression

`exceptions.ArchiveError` is the rail base; every fault below subclasses it, so a boundary maps the precise subtype and falls back to `ArchiveError`, never a bare `Exception`.

| [INDEX] | [SYMBOL]                            | [PACKAGE_ROLE] | [CAPABILITY]                                |
| :-----: | :---------------------------------- | :------------- | :------------------------------------------ |
|  [01]   | `ArchiveError`                      | fault base     | base of every py7zr failure                 |
|  [02]   | `Bad7zFile`                         | format fault   | corrupt or invalid 7z container             |
|  [03]   | `PasswordRequired`                  | auth fault     | encrypted archive opened without a password |
|  [04]   | `DecompressionError`                | codec fault    | a decode step failed                        |
|  [05]   | `CrcError`                          | integrity fault| a per-entry CRC check mismatched on extract |
|  [06]   | `DecompressionBombError`            | safety fault   | extracted size exceeded `max_extract_size` (zip-bomb guard) |
|  [07]   | `AbsolutePathError`                 | safety fault   | an entry name resolved outside the extract root (path-traversal guard) |
|  [08]   | `UnsupportedCompressionMethodError` | codec fault    | an unimplemented filter was encountered     |
|  [09]   | `InternalError`                     | invariant fault| a py7zr internal-state invariant broke      |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: archive open, read, and write
- rail: compression

| [INDEX] | [SURFACE]                     | [CALL_SHAPE]                                                                                       | [CAPABILITY]                |
| :-----: | :---------------------------- | :------------------------------------------------------------------------------------------------- | :-------------------------- |
|  [01]   | `SevenZipFile`                | `SevenZipFile(file, mode='r', *, filters=None, dereference=False, password=None, header_encryption=False, blocksize=None, mp=False, max_extract_size=None)` | open/create an archive (`max_extract_size` caps decode to defend against a decompression bomb) |
|  [02]   | `SevenZipFile.extractall`     | `extractall(path=None, *, callback=None, factory=None)`                                            | extract every entry         |
|  [03]   | `SevenZipFile.extract`        | `extract(path=None, targets=None, recursive=False, *, callback=None, factory=None)`                | extract selected entries / into a `WriterFactory` sink |
|  [04]   | `SevenZipFile.writeall`       | `writeall(path, arcname=None)`                                                                     | add a tree to the archive   |
|  [05]   | `SevenZipFile.write`          | `write(file, arcname=None)`                                                                        | add one file                |
|  [06]   | `SevenZipFile.writestr`       | `writestr(data, arcname)`                                                                          | add in-memory bytes/str     |
|  [07]   | `SevenZipFile.writef`         | `writef(bio, arcname)`                                                                             | add from a file-like object |
|  [08]   | `SevenZipFile.list` / `getnames` / `namelist` / `getinfo` | listing / name list / single `FileInfo`                                | enumerate entries           |
|  [09]   | `SevenZipFile.test` / `testzip` | CRC verification query                                                                           | verify CRCs                 |
|  [10]   | `SevenZipFile.needs_password` / `archiveinfo` / `reset` | password-state / `ArchiveInfo` / rewind cursor                              | encryption probe + metadata |

[ENTRYPOINT_SCOPE]: predicate and shutil registration
- rail: compression

| [INDEX] | [SURFACE]          | [CALL_SHAPE]                    | [CAPABILITY]       |
| :-----: | :----------------- | :------------------------------ | :----------------- |
|  [01]   | `is_7zfile`        | `is_7zfile(file)` (path/stream) | format predicate   |
|  [02]   | `pack_7zarchive`   | base name, directory, and hooks | shutil pack hook (`shutil.register_archive_format('7zip', pack_7zarchive)`) |
|  [03]   | `unpack_7zarchive` | archive, target path, and extra | shutil unpack hook (`shutil.register_unpack_format('7zip', ['.7z'], unpack_7zarchive)`) |

## [04]-[IMPLEMENTATION_LAW]

[COMPRESSION_7Z]:
- import: `import py7zr` at boundary scope only; module-level import is banned by the manifest import policy.
- archive axis: one `SevenZipFile` owns read/write/test under a `mode` row (`r`/`w`/`a`/`x`); `mp=True` enables multiprocessing decode, `blocksize` sizes the read buffer, `dereference=True` follows symlinks on write; never a per-codec archive type.
- codec axis: the `FILTER_*` table is the filter-chain row source; a filter chain is a `list[dict]` value (`[{'id': FILTER_LZMA2, 'preset': PRESET_DEFAULT}]`), pre-filters (DELTA, BCJ `X86`/`ARM`/...) prepend the codec stage, and `CHECK_*` selects the per-chunk integrity check; never parallel archive owners per codec.
- crypto axis: `password=...` plus `header_encryption=True` and a `FILTER_CRYPTO_AES256_SHA256` chain entry are the encryption rows on the archive root; `set_encrypted_header(True)`/`set_encoded_header_mode(...)` tune header protection; never a separate encryptor.
- streamed-extraction axis: `extract`/`extractall` accept a `WriterFactory` whose `create(name) -> Py7zIO` returns a per-entry sink, so large entries decode straight into a capped in-memory `BytesIOFactory(limit)` (then `get(name)` retrieves the buffer), a streaming `HashIOFactory()` (content-identity seam), a discarding `NullIOFactory`, or a custom sink, never a temp-file round-trip; `ExtractCallback` rides the same call for progress evidence and `ArchiveCallback` mirrors it on write.
- safety axis: `max_extract_size=...` on the archive root bounds the total decoded size and raises `DecompressionBombError` when exceeded; a traversal entry name raises `AbsolutePathError`; a CRC mismatch raises `CrcError`. These are the zip-bomb / path-traversal / corruption guards on an untrusted upload, mapped at the boundary, never a post-extract size re-check.
- evidence: each archive op captures entry count (`list()` length), the `FileInfo` per-entry fields (`compressed`/`uncompressed`/`crc32`/`is_directory`/`is_symlink`/`creationtime`), the `ArchiveInfo` container fields (`method_names`/`solid`/`blocks`/`header_size`), encryption + header-encryption state, integrity-check kind, and the `max_extract_size` bound as a compression receipt.
- boundary: py7zr owns the 7z container; raw single-stream codecs route to `zstandard`/`lz4`/`brotli`; zip/tar route to `shutil`/stdlib; streamed entry hashing for content identity rides the `HashIOFactory` sink, never a post-extract re-read; live UI stays outside this package.

## [05]-[LOCAL_ADMISSION]

[RAIL_LAW]:
- Package: `py7zr`
- Owns: 7z archive read/write/test/list, multi-codec filter chains with DELTA/BCJ pre-filters, AES256 header+content encryption, streamed per-entry extraction sinks, the `exceptions.ArchiveError` typed fault rail, `max_extract_size` decompression-bomb + `AbsolutePathError` traversal defense, multiprocessing decode, shutil registration
- Accept: 7z container service feeding the compression and export-bundle owners; streamed `HashIOFactory` entry digests feeding the content-identity rail; `max_extract_size`-bounded extract of an untrusted upload mapping `DecompressionBombError`/`AbsolutePathError`/`CrcError` at the boundary
- Reject: wrapper-renames of `extractall`/`writeall`; a per-codec archive type where a `FILTER_*` filter row suffices; a temp-file extract round-trip where a `WriterFactory` sink streams; an unbounded extract of untrusted bytes where `max_extract_size` guards; swallowing an `ArchiveError` subtype into a bare `Exception`; identity minting the runtime owns

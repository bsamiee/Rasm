# [PY_ARTIFACTS_API_PY7ZR]

`py7zr` supplies the 7z archive surface for the artifacts compression rail: an archive root, archive/file info records, a filter codec table, and shutil-registration hooks that drive read/write/test of 7z containers with LZMA2/BZIP2/PPMD/ZSTD/BROTLI codecs and AES256 header encryption. The package owner composes `SevenZipFile`, the `FILTER_*` table, and the shutil hooks into the compression owner; it never re-implements the 7z container format py7zr already owns.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `py7zr`
- package: `py7zr`
- import: `py7zr`
- owner: `artifacts`
- rail: compression
- installed: `1.1.0` reflected via `python -c "import py7zr"` on cp315
- entry points: none (library only)
- capability: 7z archive read/write/test/list, multi-codec filter chains, AES256 header encryption, password protection, shutil archive-format registration

## [2]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: archive root and info records
- rail: compression

| [INDEX] | [SYMBOL] | [PACKAGE_ROLE] | [CAPABILITY] |
| :-----: | :------- | :------------- | :----------- |
| [1] | `SevenZipFile` | archive root | open/read/write/test a 7z archive |
| [2] | `ArchiveInfo` | archive view | container-level metadata |
| [3] | `FileInfo` | entry view | per-entry metadata |
| [4] | `Py7zIO` / `WriterFactory` | io hooks | custom per-entry IO sinks for streamed extraction |

[PUBLIC_TYPE_SCOPE]: filter codec table
- rail: compression

| [INDEX] | [SYMBOL] | [PACKAGE_ROLE] | [CAPABILITY] |
| :-----: | :------- | :------------- | :----------- |
| [1] | `FILTER_LZMA2` / `FILTER_LZMA` | primary codec | default LZMA family compression |
| [2] | `FILTER_ZSTD` / `FILTER_BROTLI` / `FILTER_BZIP2` / `FILTER_PPMD` / `FILTER_DEFLATE` / `FILTER_COPY` | alt codec | alternative compression codecs and the store-only pass-through |
| [3] | `FILTER_CRYPTO_AES256_SHA256` | crypto filter | AES256 content/header encryption |
| [4] | `FILTER_DELTA` / `FILTER_X86` / `FILTER_ARM` / `FILTER_ARMTHUMB` / `FILTER_POWERPC` / `FILTER_SPARC` / `FILTER_IA64` | preprocessor | BCJ/delta pre-filters |
| [5] | `PRESET_DEFAULT` / `PRESET_EXTREME` | preset | LZMA preset level |

[PUBLIC_TYPE_SCOPE]: faults
- rail: compression

| [INDEX] | [SYMBOL] | [PACKAGE_ROLE] | [CAPABILITY] |
| :-----: | :------- | :------------- | :----------- |
| [1] | `Bad7zFile` | format fault | corrupt or invalid 7z container |
| [2] | `PasswordRequired` | auth fault | encrypted archive opened without a password |
| [3] | `DecompressionError` | codec fault | a decode step failed |
| [4] | `UnsupportedCompressionMethodError` | codec fault | an unimplemented filter was encountered |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: archive open, read, and write
- rail: compression

| [INDEX] | [SURFACE] | [CALL_SHAPE] | [CAPABILITY] |
| :-----: | :-------- | :----------- | :----------- |
| [1] | `SevenZipFile` | `SevenZipFile(file: IO[bytes] | str | pathlib.Path, mode: str = 'r', *, filters: list[dict[str, int]] | None = None, dereference=False, password: str | None = None, header_encryption: bool = False, blocksize: int | None = None, mp: bool = False) -> None` | open/create an archive |
| [2] | `SevenZipFile.extractall` | `extractall(path=None, callback=None) -> None` | extract every entry |
| [3] | `SevenZipFile.extract` | `extract(path=None, targets=None, recursive=None, callback=None) -> None` | extract selected entries |
| [4] | `SevenZipFile.writeall` | `writeall(path, arcname=None) -> None` | add a tree to the archive |
| [5] | `SevenZipFile.write` | `write(file, arcname=None) -> None` | add one file |
| [6] | `SevenZipFile.writestr` | `writestr(data, arcname) -> None` | add in-memory bytes |
| [7] | `SevenZipFile.writef` | `writef(bio, arcname) -> None` | add from a file-like object |
| [8] | `SevenZipFile.list` | `list() -> list[FileInfo]` | enumerate entries |
| [9] | `SevenZipFile.test` | `test() -> bool | None` | verify CRCs |
| [10] | `SevenZipFile.needs_password` | `needs_password() -> bool` | encryption probe |

[ENTRYPOINT_SCOPE]: predicate and shutil registration
- rail: compression

| [INDEX] | [SURFACE] | [CALL_SHAPE] | [CAPABILITY] |
| :-----: | :-------- | :----------- | :----------- |
| [1] | `is_7zfile` | `is_7zfile(file: SupportsReadAndSeek | IO[bytes] | str | os.PathLike[str]) -> bool` | format predicate |
| [2] | `pack_7zarchive` | `pack_7zarchive(base_name, base_dir, owner=None, group=None, dry_run=None, logger=None)` | shutil pack hook |
| [3] | `unpack_7zarchive` | `unpack_7zarchive(archive, path, extra=None)` | shutil unpack hook |

## [4]-[IMPLEMENTATION_LAW]

[COMPRESSION_7Z]:
- import: `import py7zr` at boundary scope only; module-level import is banned by the manifest import policy.
- archive axis: one `SevenZipFile` owns read/write/test; `mode` and `filters` are constructor rows, never a per-codec archive type.
- codec axis: the `FILTER_*` table is the filter-chain row source; a filter chain is a `list[dict]` value, never parallel archive owners per codec.
- crypto axis: `header_encryption=True` plus `FILTER_CRYPTO_AES256_SHA256` and `password` are the encryption rows on the archive root, never a separate encryptor.
- evidence: each archive op captures entry count, filter chain, encryption state, and total compressed/uncompressed sizes as a compression receipt.
- boundary: py7zr owns the 7z container; raw stream codecs route to `zstandard`/`lz4`/`brotli`; live UI stays outside this package.

## [5]-[LOCAL_ADMISSION]

[RAIL_LAW]:
- Package: `py7zr`
- Owns: 7z archive read/write/test/list, multi-codec filter chains, AES256 header encryption, shutil registration
- Accept: 7z container service feeding the compression and export-bundle owners
- Reject: wrapper-renames of `extractall`/`writeall`; a per-codec archive type where a filter row suffices; identity minting the runtime owns

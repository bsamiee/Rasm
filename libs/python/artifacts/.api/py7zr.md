# [PY_ARTIFACTS_API_PY7ZR]

`py7zr` supplies the 7z archive surface for the artifacts compression rail: an archive root, archive/file info records, a filter codec table, and shutil-registration hooks that drive read/write/test of 7z containers with LZMA2/BZIP2/PPMD/ZSTD/BROTLI codecs and AES256 header encryption. The package owner composes `SevenZipFile`, the `FILTER_*` table, and the shutil hooks into the compression owner; it never re-implements the 7z container format py7zr already owns.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `py7zr`
- package: `py7zr`
- import: `py7zr`
- owner: `artifacts`
- rail: compression
- installed: `1.1.0` reflected via `python -c "import py7zr"` on cp315
- entry points: none (library only)
- capability: 7z archive read/write/test/list, multi-codec filter chains, AES256 header encryption, password protection, shutil archive-format registration

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: archive root and info records
- rail: compression

| [INDEX] | [SYMBOL]                   | [PACKAGE_ROLE] | [CAPABILITY]                                      |
| :-----: | :------------------------- | :------------- | :------------------------------------------------ |
|  [01]   | `SevenZipFile`             | archive root   | open/read/write/test a 7z archive                 |
|  [02]   | `ArchiveInfo`              | archive view   | container-level metadata                          |
|  [03]   | `FileInfo`                 | entry view     | per-entry metadata                                |
|  [04]   | `Py7zIO` / `WriterFactory` | io hooks       | custom per-entry IO sinks for streamed extraction |

[PUBLIC_TYPE_SCOPE]: filter codec table
- rail: compression

Filter rows are filter-chain vocabulary; exact constants remain in the package namespace and are consumed as `filters` row values.

| [INDEX] | [SYMBOL_FAMILY]               | [PACKAGE_ROLE] | [CAPABILITY]                          |
| :-----: | :---------------------------- | :------------- | :------------------------------------ |
|  [01]   | `FILTER_LZMA*`                | primary codec  | default LZMA family compression       |
|  [02]   | `FILTER_ZSTD` / alternatives  | alt codec      | additional codecs and store-only copy |
|  [03]   | `FILTER_CRYPTO_AES256_SHA256` | crypto filter  | AES256 content/header encryption      |
|  [04]   | `FILTER_DELTA` / BCJ filters  | preprocessor   | BCJ/delta pre-filters                 |
|  [05]   | `PRESET_*`                    | preset         | LZMA preset level                     |

[PUBLIC_TYPE_SCOPE]: faults
- rail: compression

| [INDEX] | [SYMBOL]                            | [PACKAGE_ROLE] | [CAPABILITY]                                |
| :-----: | :---------------------------------- | :------------- | :------------------------------------------ |
|  [01]   | `Bad7zFile`                         | format fault   | corrupt or invalid 7z container             |
|  [02]   | `PasswordRequired`                  | auth fault     | encrypted archive opened without a password |
|  [03]   | `DecompressionError`                | codec fault    | a decode step failed                        |
|  [04]   | `UnsupportedCompressionMethodError` | codec fault    | an unimplemented filter was encountered     |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: archive open, read, and write
- rail: compression

The archive constructor row carries source, mode, filter chain, dereference, password, header-encryption, blocksize, and multiprocessing policy.

| [INDEX] | [SURFACE]                     | [CALL_SHAPE]                  | [CAPABILITY]                |
| :-----: | :---------------------------- | :---------------------------- | :-------------------------- |
|  [01]   | `SevenZipFile`                | archive open/create policy    | open/create an archive      |
|  [02]   | `SevenZipFile.extractall`     | target path plus callback     | extract every entry         |
|  [03]   | `SevenZipFile.extract`        | target path plus selection    | extract selected entries    |
|  [04]   | `SevenZipFile.writeall`       | source tree plus archive name | add a tree to the archive   |
|  [05]   | `SevenZipFile.write`          | source file plus archive name | add one file                |
|  [06]   | `SevenZipFile.writestr`       | bytes plus archive name       | add in-memory bytes         |
|  [07]   | `SevenZipFile.writef`         | file-like object plus name    | add from a file-like object |
|  [08]   | `SevenZipFile.list`           | no-arg listing                | enumerate entries           |
|  [09]   | `SevenZipFile.test`           | CRC verification query        | verify CRCs                 |
|  [10]   | `SevenZipFile.needs_password` | password-state query          | encryption probe            |

[ENTRYPOINT_SCOPE]: predicate and shutil registration
- rail: compression

| [INDEX] | [SURFACE]          | [CALL_SHAPE]                    | [CAPABILITY]       |
| :-----: | :----------------- | :------------------------------ | :----------------- |
|  [01]   | `is_7zfile`        | readable source or path         | format predicate   |
|  [02]   | `pack_7zarchive`   | base name, directory, and hooks | shutil pack hook   |
|  [03]   | `unpack_7zarchive` | archive, target path, and extra | shutil unpack hook |

## [04]-[IMPLEMENTATION_LAW]

[COMPRESSION_7Z]:
- import: `import py7zr` at boundary scope only; module-level import is banned by the manifest import policy.
- archive axis: one `SevenZipFile` owns read/write/test; `mode` and `filters` are constructor rows, never a per-codec archive type.
- codec axis: the `FILTER_*` table is the filter-chain row source; a filter chain is a `list[dict]` value, never parallel archive owners per codec.
- crypto axis: `header_encryption=True` plus `FILTER_CRYPTO_AES256_SHA256` and `password` are the encryption rows on the archive root, never a separate encryptor.
- evidence: each archive op captures entry count, filter chain, encryption state, and total compressed/uncompressed sizes as a compression receipt.
- boundary: py7zr owns the 7z container; raw stream codecs route to `zstandard`/`lz4`/`brotli`; live UI stays outside this package.

## [05]-[LOCAL_ADMISSION]

[RAIL_LAW]:
- Package: `py7zr`
- Owns: 7z archive read/write/test/list, multi-codec filter chains, AES256 header encryption, shutil registration
- Accept: 7z container service feeding the compression and export-bundle owners
- Reject: wrapper-renames of `extractall`/`writeall`; a per-codec archive type where a filter row suffices; identity minting the runtime owns

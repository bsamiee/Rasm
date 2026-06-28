# [PY_ARTIFACTS_API_PYTHON_MAGIC]

`python-magic` supplies the libmagic-backed content-identity surface for the artifacts file-control rail: a configurable `Magic` cookie that wraps the C `magic_open`/`magic_load`/`magic_file`/`magic_buffer`/`magic_descriptor` handle, plus the stateless `from_buffer`/`from_file`/`from_descriptor` module functions that classify bytes, paths, and descriptors into MIME types, human descriptions, charset encodings, or extension lists. The package owner composes one `Magic(mime=..., mime_encoding=..., extension=..., uncompress=..., keep_going=..., raw=...)` cookie per flag-policy plus the module convenience rows into the file-control owner; it removes any extension-table guesser because libmagic does true content sniffing, and it never re-implements the magic-pattern database, the compiled `.mgc` loader, or the per-flag bit math the native library already owns.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `python-magic`
- package: `python-magic`
- import: `magic`
- owner: `artifacts`
- rail: file control
- installed: `0.4.27`
- license: MIT (Adam Hupp) — permissive, no copyleft gate; aligns with the MIT/BSD sibling artifacts owners
- entry points: none (library only)
- capability: libmagic content sniffing from in-memory bytes, filesystem path, or open file descriptor; MIME-type / human-description / MIME-encoding / slash-separated extension-list outputs selected by cookie flags; compressed-container look-through; custom `.mgc` database files; libmagic tuning params (recursion, name-use, regex, byte budget) and version query

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: cookie and fault
- rail: file control

| [INDEX] | [SYMBOL]         | [PACKAGE_ROLE]  | [CAPABILITY]                                                                          |
| :-----: | :--------------- | :-------------- | :------------------------------------------------------------------------------------ |
|  [01]   | `Magic`          | detector cookie | a configured libmagic handle owning the flag set, lock, and lazily-loaded database     |
|  [02]   | `MagicException` | engine fault    | a libmagic call returned NULL/-1; carries `.message` (the `magic_error` C string)      |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: stateless module detection
- rail: file control

The module functions own an internal `_instances`-cached default cookie keyed by the `mime` boolean; they expose only the `mime` switch (description vs MIME-type). Richer flag policy requires a `Magic` cookie.

| [INDEX] | [SURFACE]               | [CALL_SHAPE]                                      | [CAPABILITY]                                            |
| :-----: | :---------------------- | :------------------------------------------------ | :------------------------------------------------------ |
|  [01]   | `from_buffer`           | `from_buffer(buffer, mime=False) -> str`          | detect from in-memory bytes (str input is utf-8 coerced) |
|  [02]   | `from_file`             | `from_file(filename, mime=False) -> str`          | detect from a path (`PathLike` accepted; opens to verify) |
|  [03]   | `from_descriptor`       | `from_descriptor(fd, mime=False) -> str`          | detect from an open file descriptor (int)               |
|  [04]   | `version`               | `version() -> int`                                | libmagic version (e.g. `541`); `NotImplementedError` on old libs |

[ENTRYPOINT_SCOPE]: configured cookie
- rail: file control

`Magic.__init__` is where every detection flag lives; the per-call methods take only the source and return the cooked string under the cookie's flags. One cookie per flag-policy is the canonical owner — the flags are NOT per-call arguments.

| [INDEX] | [SURFACE]              | [CALL_SHAPE]                                                                                  | [CAPABILITY]                                                         |
| :-----: | :--------------------- | :-------------------------------------------------------------------------------------------- | :------------------------------------------------------------------- |
|  [01]   | `Magic`                | `Magic(mime=False, magic_file=None, mime_encoding=False, keep_going=False, uncompress=False, raw=False, extension=False)` | construct a flag-pinned detector cookie over a custom or system DB    |
|  [02]   | `Magic.from_buffer`    | `from_buffer(buf) -> str`                                                                     | detect from bytes under the cookie flags (thread-locked)             |
|  [03]   | `Magic.from_file`      | `from_file(filename) -> str`                                                                  | detect from a path under the cookie flags                            |
|  [04]   | `Magic.from_descriptor`| `from_descriptor(fd) -> str`                                                                  | detect from a descriptor under the cookie flags                      |
|  [05]   | `Magic.setparam`       | `setparam(param, val) -> int`                                                                 | tune a libmagic limit (`MAGIC_PARAM_*`); raises if libmagic lacks it |
|  [06]   | `Magic.getparam`       | `getparam(param) -> int`                                                                      | read a current libmagic limit                                        |

[ENTRYPOINT_SCOPE]: flag and param vocabulary
- rail: file control

The constructor booleans set the underlying `MAGIC_*` bitmask; the `MAGIC_PARAM_*` ordinals address `setparam`/`getparam`. Compose the booleans, never the raw bits.

| [INDEX] | [FLAG_BOOLEAN]   | [MAGIC_BIT]            | [EFFECT]                                                            |
| :-----: | :--------------- | :-------------------- | :----------------------------------------------------------------- |
|  [01]   | `mime`           | `MAGIC_MIME_TYPE`     | return the MIME type (`application/pdf`) instead of a description    |
|  [02]   | `mime_encoding`  | `MAGIC_MIME_ENCODING` | return the charset (`utf-8`, `binary`)                              |
|  [03]   | `extension`      | `MAGIC_EXTENSION`     | return a `/`-separated valid-extension list (libmagic >= 524)       |
|  [04]   | `uncompress`     | `MAGIC_COMPRESS`      | sniff inside gzip/bzip2/xz containers                               |
|  [05]   | `keep_going`     | `MAGIC_CONTINUE`      | return all matches, not just the first                             |
|  [06]   | `raw`            | `MAGIC_RAW`           | do not translate unprintable characters                            |
|  [07]   | `magic_file=`    | (passed to `magic_load`) | load a custom compiled `.mgc` or text magic database               |
|  [08]   | `MAGIC_PARAM_*`  | (param ordinals)      | `INDIR_MAX`/`NAME_MAX`/`REGEX_MAX`/`BYTES_MAX` recursion/budget caps plus `ELF_NOTES_MAX`/`ELF_PHNUM_MAX`/`ELF_SHNUM_MAX` ELF-table caps for `setparam`/`getparam` |

## [04]-[IMPLEMENTATION_LAW]

[CONTENT_IDENTITY]:
- import: `import magic` at boundary scope only; module-level import is banned by the manifest import policy. Wrap the import in the libmagic-absent guard — an `ImportError` at import time means the host has no `libmagic`, which the file-control owner must surface as a provisioning fault, not a content fault.
- detector axis: one `Magic` cookie per flag-policy owns the flags, the `threading.Lock`, and the loaded database; the module-level `from_buffer`/`from_file`/`from_descriptor` are the stateless rows over the `_instances`-cached default cookie and expose ONLY `mime`. A multi-output identity (MIME + encoding + extensions in one pass) requires holding a cookie, since the module functions cannot stack flags.
- source axis: buffer, file, and descriptor are three call rows on the same cookie surface, never parallel detector types; the in-memory-bytes row is the canonical one because admission already holds the payload — file/descriptor rows avoid a re-read only when the payload is on disk.
- output axis: `mime`/`mime_encoding`/`extension` are flag selections on one detection, never separate functions per output; a full identity is one cookie carrying `mime=True, mime_encoding=True` plus a second `extension=True` cookie when extension hints are wanted, because libmagic returns one cooked string per call.
- fault axis: a failed libmagic call raises `MagicException` (carrying `.message`); the cookie's `_handle509Bug` returns `application/octet-stream` for the known null-result MIME bug. Lift `MagicException` to the file-control fault rail once at the boundary; never let it escape as a bare exception into domain logic.
- evidence: each detection captures source kind, MIME type, encoding, detected-extension list, and input byte length as a content-identity receipt; the result is folded into the runtime `ContentIdentity`, never a re-minted identity.
- boundary: python-magic owns content sniffing only. Its `ContentIdentity` is the admission gate the document/PDF/image owners read before dispatch — `python-docx`/`python-pptx`/`openpyxl` consume the OOXML/ODF MIME branch, `pymupdf`/`pypdf` the `application/pdf` branch, `pillow`/`pyvips` the image branches, `msoffcrypto-tool` the encrypted-Office branch. The `compat` shim functions (`detect_from_filename`/`detect_from_content`/`detect_from_fobj`/`open`) are deprecation-wrapped and are NOT admitted — they collide with the upstream file-5.x binding; use the `Magic` cookie. Live UI stays outside this package.

## [05]-[LOCAL_ADMISSION]

[RAIL_LAW]:
- Package: `python-magic`
- Owns: libmagic content sniffing from bytes/path/descriptor with MIME-type / description / MIME-encoding / extension-list outputs, compressed look-through, custom databases, and tuning params
- Accept: content sniffing producing the runtime `ContentIdentity` at the file-control boundary, feeding the per-format reader dispatch
- Reject: wrapper-renames of `from_buffer`/`from_file`; the deprecation-wrapped `compat.*` shim; an extension-only guesser where libmagic content sniffing is admitted; passing detection flags as per-call args where a flag-pinned `Magic` cookie is the owner; identity minting the runtime already owns

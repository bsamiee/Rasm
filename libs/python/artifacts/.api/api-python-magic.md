# [PY_ARTIFACTS_API_PYTHON_MAGIC]

`python-magic` supplies the libmagic-backed content-identity surface for the artifacts file-control rail: module-level detection functions and a configurable `Magic` cookie that classify bytes, file paths, and descriptors into MIME types, human descriptions, and encodings. The package owner composes `from_buffer`, `from_file`, and `Magic` into the file-control owner; it never re-implements the libmagic database the native library already owns.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `python-magic`
- package: `python-magic`
- import: `magic`
- owner: `artifacts`
- rail: file control
- installed: `0.4.27` (Python module present and reflected via source AST on cp315; the native libmagic shared library is absent on this host, so runtime detection is gated until libmagic is provisioned — the Python member surface is captured)
- entry points: none (library only)
- capability: libmagic content-type detection from bytes, path, or descriptor; MIME-type, human-readable description, and encoding outputs; custom magic database files

## [2]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: cookie and fault
- rail: file control

| [INDEX] | [SYMBOL] | [PACKAGE_ROLE] | [CAPABILITY] |
| :-----: | :------- | :------------- | :----------- |
| [1] | `Magic` | detector cookie | a configured libmagic handle (mime/description/encoding flags, custom DB) |
| [2] | `MagicException` | engine fault | a libmagic call failed |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: detection functions and cookie
- rail: file control

| [INDEX] | [SURFACE] | [CALL_SHAPE] | [CAPABILITY] |
| :-----: | :-------- | :----------- | :----------- |
| [1] | `from_buffer` | `from_buffer(buffer, mime=False) -> str` | detect from in-memory bytes |
| [2] | `from_file` | `from_file(filename, mime=False) -> str` | detect from a path |
| [3] | `from_descriptor` | `from_descriptor(fd, mime=False) -> str` | detect from a file descriptor |
| [4] | `Magic` | `Magic(mime=False, magic_file=None, mime_encoding=False, keep_going=False, uncompress=False, raw=False, extension=False)` | configured detector cookie |
| [5] | `Magic.from_buffer` | `from_buffer(buf) -> str` | detect from bytes with the cookie config |
| [6] | `Magic.from_file` | `from_file(filename) -> str` | detect from a path with the cookie config |
| [7] | `version` | `version() -> int` | libmagic version |

## [4]-[IMPLEMENTATION_LAW]

[CONTENT_IDENTITY]:
- import: `import magic` at boundary scope only; module-level import is banned by the manifest import policy.
- detector axis: one `Magic` cookie owns the detection flags (`mime`/`mime_encoding`/`extension`/custom DB); the module-level `from_buffer`/`from_file`/`from_descriptor` are the stateless convenience rows over an internal default cookie.
- source axis: buffer, file, and descriptor are detection rows on the same surface, never parallel detector types.
- output axis: the `mime` flag selects MIME-type vs human-description output; `mime_encoding` selects charset — call rows, never separate functions per output.
- evidence: each detection captures source kind, MIME type, encoding, and input byte length as a content-identity receipt; the result is the runtime `ContentIdentity`, never a re-minted identity.
- boundary: python-magic owns content sniffing; the document/PDF/image owners consume its `ContentIdentity` at admission; live UI stays outside this package.

## [5]-[LOCAL_ADMISSION]

[RAIL_LAW]:
- Package: `python-magic`
- Owns: libmagic content-type detection from bytes/path/descriptor with MIME/description/encoding outputs and custom databases
- Accept: content sniffing producing the runtime `ContentIdentity` at the file-control boundary
- Reject: wrapper-renames of `from_buffer`/`from_file`; an extension-only guesser where libmagic content sniffing is admitted; identity minting the runtime already owns

# [PY_ARTIFACTS_API_STREAM_UNZIP]

`stream-unzip` owns bounded-memory streaming ZIP extraction on the bundle rail: `stream_unzip` yields `(name, size, chunks)` triples from a container's chunked `bytes` over a lazy inner `bytes` generator, decoding each member as it arrives — no central-directory seek, no whole-archive buffer. An `allowed_encryption_mechanisms` allow-list bounds which ZipCrypto and WinZip-AES records decode under a `bytes` `password`. It streams the inverse of `stream-zip`, owning its ZipCrypto keystream while delegating deflate64 to `stream-inflate`, AES to `pycryptodome`, and bzip2/deflate to stdlib.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `stream-unzip`
- package: `stream-unzip` (MIT)
- module: `stream_unzip`
- abi: compiled `stream_unzip._zipcrypto` extension (Rust, maturin-built) owns the ZipCrypto cipher; not pure-Python
- depends: `pycryptodome` (WinZip-AES cipher, HMAC-SHA1, PBKDF2), `stream-inflate` (`stream_inflate64` deflate64 inflate), stdlib `zlib`/`bz2` (deflate/bzip2), stdlib `asyncio` with optional `trio` (async backends)
- role: import-only library; no console script
- rail: bundle

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: encryption-mechanism sentinels and the rooted error tree

Every mechanism sentinel is an opaque `_Encryption` singleton behind a private constructor, so a consumer selects from the seven, never mints an eighth; `_ALL_ENCRYPTIONS` is the default-permitting tuple. `except UnzipError` catches every failure, `except UnzipValueError` every recoverable boundary fault including both `PasswordError` and its sibling `MissingPasswordError`, and `except InvalidOperationError` only the iteration-order guard that sits outside `ValueError`.

| [INDEX] | [SYMBOL]                          | [TYPE_FAMILY]      | [CAPABILITY]                                                                   |
| :-----: | :-------------------------------- | :----------------- | :----------------------------------------------------------------------------- |
|  [01]   | `NO_ENCRYPTION`                   | mechanism sentinel | `_Encryption` singleton; permit unencrypted members                            |
|  [02]   | `ZIP_CRYPTO`                      | mechanism sentinel | `_Encryption` singleton; permit legacy ZipCrypto members                       |
|  [03]   | `AE_1`                            | mechanism sentinel | `_Encryption` singleton; permit WinZip AE-1 (CRC-retained) members             |
|  [04]   | `AE_2`                            | mechanism sentinel | `_Encryption` singleton; permit WinZip AE-2 (CRC-zeroed) members               |
|  [05]   | `AES_128`                         | mechanism sentinel | `_Encryption` singleton; permit AES-128 key-length members                     |
|  [06]   | `AES_192`                         | mechanism sentinel | `_Encryption` singleton; permit AES-192 key-length members                     |
|  [07]   | `AES_256`                         | mechanism sentinel | `_Encryption` singleton; permit AES-256 key-length members                     |
|  [08]   | `_ALL_ENCRYPTIONS`                | sentinel tuple     | default `allowed_encryption_mechanisms` permitting every mechanism above       |
|  [09]   | `UnzipError`                      | error root         | `Exception` base for every `stream_unzip` failure                              |
|  [10]   | `UnzipValueError`                 | error              | `UnzipError` + `ValueError`; boundary base for data + password families        |
|  [11]   | `DataError`                       | error              | `UnzipValueError`; malformed/unsupported container base                        |
|  [12]   | `UnexpectedSignatureError`        | error              | `DataError`; local-file/record signature mismatch                              |
|  [13]   | `TruncatedDataError`              | error              | `DataError`; member data truncated mid-stream                                  |
|  [14]   | `TruncatedExtraError`             | error              | `DataError`; extra-field block truncated                                       |
|  [15]   | `InvalidExtraError`               | error              | `TruncatedExtraError`; malformed extra-field block                             |
|  [16]   | `TruncatedZip64ExtraError`        | error              | `TruncatedExtraError`; ZIP64 extra-field truncated                             |
|  [17]   | `MissingExtraError`               | error              | `DataError`; required extra-field block absent                                 |
|  [18]   | `MissingAESExtraError`            | error              | `MissingExtraError`; WinZip-AES extra-field absent on an AES member            |
|  [19]   | `TruncatedAESExtraError`          | error              | `TruncatedExtraError`; WinZip-AES extra-field truncated                        |
|  [20]   | `InvalidAESKeyLengthError`        | error              | `TruncatedExtraError`; AES key-length byte outside 1/2/3                       |
|  [21]   | `IntegrityError`                  | error              | `DataError`; verified-content mismatch base                                    |
|  [22]   | `CRC32IntegrityError`             | error              | `IntegrityError`; streamed CRC32 mismatch                                      |
|  [23]   | `SizeIntegrityError`              | error              | `IntegrityError`; declared-size mismatch base                                  |
|  [24]   | `CompressedSizeIntegrityError`    | error              | `SizeIntegrityError`; streamed compressed-size mismatch                        |
|  [25]   | `UncompressedSizeIntegrityError`  | error              | `SizeIntegrityError`; streamed uncompressed-size mismatch                      |
|  [26]   | `HMACIntegrityError`              | error              | `IntegrityError`; WinZip-AES HMAC-SHA1 authentication mismatch                 |
|  [27]   | `UncompressError`                 | error              | `UnzipValueError`; decompressor backend failure base                           |
|  [28]   | `DeflateError`                    | error              | `UncompressError`; deflate/deflate64 decompression failure                     |
|  [29]   | `BZ2Error`                        | error              | `UncompressError`; bzip2 decompression failure                                 |
|  [30]   | `UnsupportedFeatureError`         | error              | `DataError`; container uses an unsupported feature base                        |
|  [31]   | `UnsupportedCompressionTypeError` | error              | `UnsupportedFeatureError`; compression method not deflate/deflate64/bz2/store  |
|  [32]   | `UnsupportedFlagsError`           | error              | `UnsupportedFeatureError`; general-purpose flag bits unsupported               |
|  [33]   | `UnsupportedZip64Error`           | error              | `UnsupportedFeatureError`; ZIP64 present while `allow_zip64=False`             |
|  [34]   | `NotStreamUnzippable`             | error              | `UnsupportedFeatureError`; archive cannot be unzipped in a single forward pass |
|  [35]   | `PasswordError`                   | error              | `UnzipValueError`; encryption/password family base                             |
|  [36]   | `MissingPasswordError`            | error              | `UnzipValueError`; encrypted member but `password is None` base                |
|  [37]   | `MissingZipCryptoPasswordError`   | error              | `MissingPasswordError`; ZipCrypto member without a password                    |
|  [38]   | `MissingAESPasswordError`         | error              | `MissingPasswordError`; WinZip-AES member without a password                   |
|  [39]   | `IncorrectPasswordError`          | error              | `PasswordError`; supplied password rejected base                               |
|  [40]   | `IncorrectZipCryptoPasswordError` | error              | `IncorrectPasswordError`; ZipCrypto password-verify byte mismatch              |
|  [41]   | `IncorrectAESPasswordError`       | error              | `IncorrectPasswordError`; WinZip-AES password-verify (PBKDF2) mismatch         |
|  [42]   | `EncryptionMechanismNotAllowed`   | error              | `PasswordError`; member mechanism excluded by the allow-list base              |
|  [43]   | `FileIsNotEncrypted`              | error              | `EncryptionMechanismNotAllowed`; plaintext + `password`, `NO_ENCRYPTION` off   |
|  [44]   | `ZipCryptoNotAllowed`             | error              | `EncryptionMechanismNotAllowed`; ZipCrypto excluded by the allow-list          |
|  [45]   | `AESNotAllowed`                   | error              | `EncryptionMechanismNotAllowed`; WinZip-AES excluded base                      |
|  [46]   | `AE1NotAllowed`                   | error              | `AESNotAllowed`; AE-1 excluded                                                 |
|  [47]   | `AE2NotAllowed`                   | error              | `AESNotAllowed`; AE-2 excluded                                                 |
|  [48]   | `AES128NotAllowed`                | error              | `AESNotAllowed`; AES-128 key length excluded                                   |
|  [49]   | `AES192NotAllowed`                | error              | `AESNotAllowed`; AES-192 key length excluded                                   |
|  [50]   | `AES256NotAllowed`                | error              | `AESNotAllowed`; AES-256 key length excluded                                   |
|  [51]   | `InvalidOperationError`           | error              | `UnzipError` (NOT `ValueError`); API-misuse base                               |
|  [52]   | `UnfinishedIterationError`        | error              | `InvalidOperationError`; advanced to next member before draining prior chunks  |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: streaming extraction
- carry: `password: bytes | None`, `chunk_size: int`, `allow_zip64: bool`, `allowed_encryption_mechanisms: Container[_Encryption]`

Both yield `(name, size, chunks)` triples over the shared carry set. `async_stream_unzip` runs the sync `stream_unzip` on a worker thread — `asyncio.run_in_executor` or `trio.to_thread.run_sync`, auto-detected — bridging both directions through sentinel-terminated hops, so it is the native `anyio` boundary for either backend rather than a consumer-side executor.

| [INDEX] | [SURFACE]                      | [SHAPE]   | [CAPABILITY]                                  |
| :-----: | :----------------------------- | :-------- | :-------------------------------------------- |
|  [01]   | `stream_unzip(zipfile_chunks)` | generator | stream-extract a ZIP archive member by member |
|  [02]   | `async_stream_unzip(chunks)`   | async-gen | async mirror over async container chunks      |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Single forward pass: one `stream_unzip` decodes every member as the container arrives, so peak memory stays bounded by `chunk_size` and one member window, never the whole archive.
- Each member is a `(name, size, chunks)` triple, never a builder type; `name` is raw `bytes` decoded once at the boundary, `size` is `None` exactly when a compressed member defers its length to a trailing data descriptor (a stored member deferring size is `NotStreamUnzippable`), and the inner `chunks` generator MUST fully drain before the outer advances or `UnfinishedIterationError` fires at the next boundary.
- One local-header compression byte (`0` stored / `8` deflate / `9` deflate64 / `12` bzip2) selects the decompressor through a frozen ternary; `password` (`bytes`) is the single decrypt switch and the record's own extra fields select ZipCrypto against WinZip-AES, so no per-scheme function exists.
- `allow_zip64` is the single ZIP64 switch; `allowed_encryption_mechanisms` gates each record before decryption, its AES variant axis (`AE_1`/`AE_2`) and key-length axis (`AES_128`/`AES_192`/`AES_256`) gating orthogonally; the decoder verifies streamed CRC32, compressed and uncompressed size, and the WinZip-AES HMAC-SHA1 tag, with AE-2 zeroing the stored CRC so its HMAC is the sole integrity proof.

[STACKING]:
- `stream-zip`(`.api/stream-zip.md`): `stream_unzip` is the exact streamed inverse, closing the round-trip `_zip_members`/`stream_zip` opens; a download or response body yields `Iterable[bytes]` straight into `stream_unzip`, which `package/archive` `Archive`'s `_zip_drain` re-emits as `(name, size, chunks)` for the `CompressionAlgo.ZIP_STREAM` unpack arm, and `allowed_encryption_mechanisms` derives from the one `ZipStreamKnobs.password` discriminant through `_zip_trust` — `None` admits `{NO_ENCRYPTION}`, a set password admits `{AE_2, AES_256}` — so the allow-list can never reject the archive its own pack leg wrote.
- within-lib: `_zip_drain` folds each member into a `(name, size, digest)` `MemberTriple` over a rolling `xxhash.xxh3_128` digest — the same 16-byte content digest the single-blob and `py7zr` arms yield, so `ContentIdentity.key` stays uniform across codecs — feeding `package/bundle` `BundleManifest.of` and the `core/receipt` `ArtifactReceipt.Bundle.entries` slot on the shared `expression` `Result` rail.
- within-lib: each member's `chunks` streams into its downstream consumer with no temp file — a recovered PDF into `document/lens`, structured text into `ruamel-yaml`/`tomlkit`, a `msgspec.msgpack.decode` over the member bytes.
- `anyio`(`.api/../anyio.md`): `async_stream_unzip` consumes a `universal-pathlib`/`httpx` async byte stream so a remote bundle extracts on the shared structured-concurrency rail, its own asyncio-versus-trio detection carrying the `anyio` backend choice transparently and mirroring the `async_stream_zip` pack side.

[LOCAL_ADMISSION]:
- Import `stream_unzip`, `async_stream_unzip`, and the mechanism and error symbols at boundary scope only, per the manifest import policy, and `password` is the `str.encode()` `bytes` of the profile password; this unpack arm recovers the member roster and never re-authors the container.

[RAIL_LAW]:
- Package: `stream-unzip`
- Owns: bounded-memory streaming ZIP extraction, per-member lazy `bytes` generators, the ordered-consumption guard, ZIP local-header/data-descriptor and ZIP64 extra-field parsing, the compiled ZipCrypto keystream, WinZip-AES record framing over the `pycryptodome` cipher, Deflate/Deflate64/Bzip2/Stored decompression, the encryption-mechanism allow-list, and streamed CRC32/size/HMAC integrity verification
- Accept: streaming unpack/list/test that consumes ordered `bytes` from a download, response, or file source and re-emits `(name, size, chunks)` triples into the `package/archive#Archive` `ZIP_STREAM` inverse and the `exchange/detect` member-roster probe, decoding lazily into the document, structured-text, and wire consumers under the shared `expression`/`anyio`/`structlog` rails
- Reject: a `zipfile.ZipFile` whole-archive seek-and-buffer path where streaming bounds memory; a hand-rolled ZIP64 extra-field, ZipCrypto cipher, WinZip-AES record parser, or AES/HMAC/PBKDF2 backend the package and `pycryptodome` already own; a hand-rolled deflate64 inflate where `stream_inflate64` owns it; a per-scheme decrypt function the `password` row and extra-field detection cover; a hardcoded mechanism `frozenset` where the allow-list resolves trust as data; a consumer-side async executor where the package's thread-pool/trio bridge owns the boundary; archive construction, which routes to `stream-zip`

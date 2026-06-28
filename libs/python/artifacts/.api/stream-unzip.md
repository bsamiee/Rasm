# [PY_ARTIFACTS_API_STREAM_UNZIP]

`stream-unzip` supplies the bounded-memory streaming UNZIP surface for the artifacts bundle rail: a generator `stream_unzip` that consumes the chunked `bytes` of a ZIP container and yields `(name, size, chunks)` per-member triples whose `chunks` is itself a lazy `bytes` generator, plus an encryption-mechanism allow-list vocabulary (`NO_ENCRYPTION`, `ZIP_CRYPTO`, `AE_1`, `AE_2`, `AES_128`, `AES_192`, `AES_256`) that bounds which WinZip-AES/ZipCrypto records are accepted, keyed by `password`. The package owner composes `stream_unzip` and its mechanism sentinels into the bundle unpack/list/test inverse of `stream-zip`; it removes the `zipfile.ZipFile` whole-archive seek-and-buffer path because every member is decoded as the container streams (no central-directory seek, no full download in memory), and it never re-implements the deflate/deflate64/bzip2 decompressor, the ZIP64 extra-field parse, the ZipCrypto keystream, or the WinZip-AES HMAC-SHA1/PBKDF2 record layout `stream-unzip` already owns.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `stream-unzip`
- package: `stream-unzip`
- import: `stream_unzip`
- owner: `artifacts`
- rail: bundle
- installed: `0.0.101`
- license: `MIT` (`Classifier: License :: OSI Approved :: MIT License`)
- entry points: library use is import-only; no console script
- capability: bounded-memory streaming ZIP extraction yielding `(name, size, member_chunks)` triples with a lazy per-member `bytes` generator, streamed Deflate/Deflate64/Bzip2/Stored decompression, ZIP64 size/offset handling under `allow_zip64`, ZipCrypto and WinZip-AES (AE-1/AE-2, AES-128/192/256) decryption keyed by `password`, an `allowed_encryption_mechanisms` allow-list that rejects disallowed records before decryption, streamed CRC32/compressed-size/uncompressed-size/HMAC integrity verification, an `UnfinishedIterationError` guard enforcing ordered member consumption, and a mirrored `async_stream_unzip` over async iterables

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: encryption-mechanism vocabulary and integrity rails
- rail: bundle

The encryption-mechanism allow-list is a closed vocabulary of opaque sentinel objects: `NO_ENCRYPTION`, `ZIP_CRYPTO`, `AE_1`, `AE_2`, `AES_128`, `AES_192`, `AES_256` (each a `NewType('_Encryption', object)` singleton); `_ALL_ENCRYPTIONS` is the default-permitting tuple in that exact order. The error tree is a single rooted hierarchy: `UnzipError` is the root, `UnzipValueError(UnzipError, ValueError)` is the boundary base, and three failure families descend from it — `DataError` (truncation, integrity, unsupported-feature, signature), `PasswordError` (encryption-mechanism and password faults), and the size/integrity leaves — while `InvalidOperationError(UnzipError)` sits OUTSIDE `ValueError` for iteration-order misuse. One `except UnzipError` catches all; `except UnzipValueError` catches every recoverable boundary failure; `except InvalidOperationError` catches only the API-misuse guard.

| [INDEX] | [SYMBOL]                          | [TYPE_FAMILY]      | [RAIL]                                                                       |
| :-----: | :-------------------------------- | :----------------- | :-------------------------------------------------------------------------- |
|  [01]   | `NO_ENCRYPTION`                   | mechanism sentinel | `_Encryption` singleton; permit unencrypted members                         |
|  [02]   | `ZIP_CRYPTO`                      | mechanism sentinel | `_Encryption` singleton; permit legacy ZipCrypto members                    |
|  [03]   | `AE_1`                            | mechanism sentinel | `_Encryption` singleton; permit WinZip AE-1 (CRC-retained) members          |
|  [04]   | `AE_2`                            | mechanism sentinel | `_Encryption` singleton; permit WinZip AE-2 (CRC-zeroed) members            |
|  [05]   | `AES_128`                         | mechanism sentinel | `_Encryption` singleton; permit AES-128 key-length members                  |
|  [06]   | `AES_192`                         | mechanism sentinel | `_Encryption` singleton; permit AES-192 key-length members                  |
|  [07]   | `AES_256`                         | mechanism sentinel | `_Encryption` singleton; permit AES-256 key-length members                  |
|  [08]   | `_ALL_ENCRYPTIONS`                | sentinel tuple     | default `allowed_encryption_mechanisms` permitting every mechanism above    |
|  [09]   | `UnzipError`                      | error root         | `Exception` base for every `stream_unzip` failure                           |
|  [10]   | `UnzipValueError`                 | error              | `UnzipError` + `ValueError`; boundary base for data + password families     |
|  [11]   | `DataError`                       | error              | `UnzipValueError`; malformed/unsupported container base                     |
|  [12]   | `UnexpectedSignatureError`        | error              | `DataError`; local-file/record signature mismatch                           |
|  [13]   | `TruncatedDataError`              | error              | `DataError`; member data truncated mid-stream                               |
|  [14]   | `TruncatedExtraError`             | error              | `DataError`; extra-field block truncated                                    |
|  [15]   | `InvalidExtraError`               | error              | `TruncatedExtraError`; malformed extra-field block                          |
|  [16]   | `TruncatedZip64ExtraError`        | error              | `TruncatedExtraError`; ZIP64 extra-field truncated                          |
|  [17]   | `MissingExtraError`               | error              | `DataError`; required extra-field block absent                              |
|  [18]   | `MissingAESExtraError`            | error              | `MissingExtraError`; WinZip-AES extra-field absent on an AES member         |
|  [19]   | `TruncatedAESExtraError`          | error              | `TruncatedExtraError`; WinZip-AES extra-field truncated                     |
|  [20]   | `InvalidAESKeyLengthError`        | error              | `TruncatedExtraError`; AES key-length byte outside 1/2/3                     |
|  [21]   | `IntegrityError`                  | error              | `DataError`; verified-content mismatch base                                 |
|  [22]   | `CRC32IntegrityError`             | error              | `IntegrityError`; streamed CRC32 mismatch                                    |
|  [23]   | `SizeIntegrityError`              | error              | `IntegrityError`; declared-size mismatch base                               |
|  [24]   | `CompressedSizeIntegrityError`    | error              | `SizeIntegrityError`; streamed compressed-size mismatch                     |
|  [25]   | `UncompressedSizeIntegrityError`  | error              | `SizeIntegrityError`; streamed uncompressed-size mismatch                   |
|  [26]   | `HMACIntegrityError`              | error              | `IntegrityError`; WinZip-AES HMAC-SHA1 authentication mismatch              |
|  [27]   | `UncompressError`                 | error              | `UnzipValueError`; decompressor backend failure base                        |
|  [28]   | `DeflateError`                    | error              | `UncompressError`; deflate/deflate64 decompression failure                  |
|  [29]   | `BZ2Error`                        | error              | `UncompressError`; bzip2 decompression failure                              |
|  [30]   | `UnsupportedFeatureError`         | error              | `DataError`; container uses an unsupported feature base                      |
|  [31]   | `UnsupportedCompressionTypeError` | error              | `UnsupportedFeatureError`; compression method not deflate/deflate64/bz2/store|
|  [32]   | `UnsupportedFlagsError`           | error              | `UnsupportedFeatureError`; general-purpose flag bits unsupported            |
|  [33]   | `UnsupportedZip64Error`           | error              | `UnsupportedFeatureError`; ZIP64 present while `allow_zip64=False`           |
|  [34]   | `NotStreamUnzippable`             | error              | `UnsupportedFeatureError`; archive cannot be unzipped in a single forward pass |
|  [35]   | `PasswordError`                   | error              | `UnzipValueError`; encryption/password family base                          |
|  [36]   | `MissingPasswordError`            | error              | `PasswordError`; encrypted member but `password is None` base               |
|  [37]   | `MissingZipCryptoPasswordError`   | error              | `MissingPasswordError`; ZipCrypto member without a password                 |
|  [38]   | `MissingAESPasswordError`         | error              | `MissingPasswordError`; WinZip-AES member without a password                |
|  [39]   | `IncorrectPasswordError`          | error              | `PasswordError`; supplied password rejected base                            |
|  [40]   | `IncorrectZipCryptoPasswordError` | error              | `IncorrectPasswordError`; ZipCrypto password-verify byte mismatch           |
|  [41]   | `IncorrectAESPasswordError`       | error              | `IncorrectPasswordError`; WinZip-AES password-verify (PBKDF2) mismatch       |
|  [42]   | `EncryptionMechanismNotAllowed`   | error              | `PasswordError`; member mechanism excluded by the allow-list base           |
|  [43]   | `FileIsNotEncrypted`              | error              | `EncryptionMechanismNotAllowed`; `password` given but member is plaintext and `NO_ENCRYPTION` excluded |
|  [44]   | `ZipCryptoNotAllowed`             | error              | `EncryptionMechanismNotAllowed`; ZipCrypto excluded by the allow-list       |
|  [45]   | `AESNotAllowed`                   | error              | `EncryptionMechanismNotAllowed`; WinZip-AES excluded base                    |
|  [46]   | `AE1NotAllowed`                   | error              | `AESNotAllowed`; AE-1 excluded                                              |
|  [47]   | `AE2NotAllowed`                   | error              | `AESNotAllowed`; AE-2 excluded                                              |
|  [48]   | `AES128NotAllowed`                | error              | `AESNotAllowed`; AES-128 key length excluded                                |
|  [49]   | `AES192NotAllowed`                | error              | `AESNotAllowed`; AES-192 key length excluded                                |
|  [50]   | `AES256NotAllowed`                | error              | `AESNotAllowed`; AES-256 key length excluded                                |
|  [51]   | `InvalidOperationError`           | error              | `UnzipError` (NOT `ValueError`); API-misuse base                            |
|  [52]   | `UnfinishedIterationError`        | error              | `InvalidOperationError`; advanced to the next member before draining the prior member's chunks |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: streaming functions
- rail: bundle

`stream_unzip` returns a lazy `Generator` of `(name, size, chunks)` triples; iterating drives the container chunk by chunk so peak memory stays bounded by `chunk_size` and a single member window — never the whole archive. `name` is the raw `bytes` member path and `size` is the declared uncompressed length (`None` when the local header defers it to the streamed data descriptor); `chunks` is a nested `Generator[bytes]` that MUST be fully consumed before advancing, or the outer generator raises `UnfinishedIterationError`. `password` (`bytes`, not `str`) decrypts ZipCrypto and WinZip-AES members; `allow_zip64` permits ZIP64 records; `allowed_encryption_mechanisms` is the accept allow-list (default `_ALL_ENCRYPTIONS`). `async_stream_unzip` mirrors the signature over async iterables, yielding an `AsyncGenerator` of `(name, size, async_chunks)`.

| [INDEX] | [SURFACE]            | [CALL_SHAPE]                                                                                                                                                                                                                               | [CAPABILITY]                                      |
| :-----: | :------------------- | :----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | :------------------------------------------------ |
|  [01]   | `stream_unzip`       | `stream_unzip(zipfile_chunks: Iterable[bytes], password: bytes \| None=None, chunk_size: int=65536, allow_zip64: bool=True, allowed_encryption_mechanisms: Container[_Encryption]=_ALL_ENCRYPTIONS) -> Generator[tuple[bytes, int \| None, Generator[bytes]]]` | stream-extract a ZIP archive member by member     |
|  [02]   | `async_stream_unzip` | `async_stream_unzip(chunks: AsyncIterable[bytes], password: bytes \| None=None, chunk_size: int=65536, allow_zip64: bool=True, allowed_encryption_mechanisms: Container[_Encryption]=_ALL_ENCRYPTIONS) -> AsyncGenerator[tuple[bytes, int \| None, AsyncGenerator[bytes]]]` | async mirror over async container chunks           |

[ENTRYPOINT_SCOPE]: encryption-mechanism allow-list
- rail: bundle

The seven mechanism sentinels are passed (as a `Container`, e.g. a `frozenset` or tuple) to `allowed_encryption_mechanisms` to bound which records decode; a member whose mechanism is absent raises the matching `EncryptionMechanismNotAllowed` leaf BEFORE any decryption, so the allow-list is a policy gate, not a post-hoc check. The default `_ALL_ENCRYPTIONS` permits every mechanism.

| [INDEX] | [SURFACE]         | [CALL_SHAPE]                                  | [CAPABILITY]                                                  |
| :-----: | :---------------- | :-------------------------------------------- | :----------------------------------------------------------- |
|  [01]   | `NO_ENCRYPTION`   | member of `allowed_encryption_mechanisms`     | accept plaintext members (gate `FileIsNotEncrypted`)         |
|  [02]   | `ZIP_CRYPTO`      | member of `allowed_encryption_mechanisms`     | accept legacy ZipCrypto (gate `ZipCryptoNotAllowed`)         |
|  [03]   | `AE_1`            | member of `allowed_encryption_mechanisms`     | accept WinZip AE-1 (gate `AE1NotAllowed`)                    |
|  [04]   | `AE_2`            | member of `allowed_encryption_mechanisms`     | accept WinZip AE-2 (gate `AE2NotAllowed`)                    |
|  [05]   | `AES_128`         | member of `allowed_encryption_mechanisms`     | accept AES-128 key length (gate `AES128NotAllowed`)         |
|  [06]   | `AES_192`         | member of `allowed_encryption_mechanisms`     | accept AES-192 key length (gate `AES192NotAllowed`)         |
|  [07]   | `AES_256`         | member of `allowed_encryption_mechanisms`     | accept AES-256 key length (gate `AES256NotAllowed`)         |

## [04]-[IMPLEMENTATION_LAW]

[BUNDLE_STREAMING_UNZIP]:
- import: `from stream_unzip import stream_unzip, async_stream_unzip, NO_ENCRYPTION, ZIP_CRYPTO, AE_1, AE_2, AES_128, AES_192, AES_256, UnzipError, UnzipValueError, InvalidOperationError` at boundary scope only; module-level import is banned by the manifest import policy.
- member axis: one `stream_unzip` owns extraction; each iteration yields a `(name, size, chunks)` triple, never a per-member builder type; `name` is the raw `bytes` path and `size` the declared uncompressed length (`None` when deferred to the data descriptor); the inner `chunks` generator is the lazy member-bytes window so member content never fully materializes.
- ordering axis: the inner `chunks` generator MUST be fully drained before the outer generator advances; advancing early raises `UnfinishedIterationError` (under `InvalidOperationError`, outside `ValueError`) — the bundle unpack owner consumes each member's chunks to its sink before requesting the next, never buffering all members.
- zip64 axis: `allow_zip64` is the single ZIP64 switch; with it set the encoder reads ZIP64 size/offset extra fields; `allow_zip64=False` raises `UnsupportedZip64Error` when a ZIP64 record appears, never a parallel ZIP64 extractor.
- decryption axis: `password` (`bytes`) is the single decrypt switch covering both ZipCrypto and WinZip-AES; the record's own extra fields select the mechanism, so there is no per-scheme function; a missing password on an encrypted member raises the `MissingPasswordError` subtree and a wrong password raises the `IncorrectPasswordError` subtree.
- mechanism-policy axis: `allowed_encryption_mechanisms` is the accept allow-list over the seven `_Encryption` sentinels; a disallowed mechanism raises the matching `EncryptionMechanismNotAllowed` leaf (`FileIsNotEncrypted`/`ZipCryptoNotAllowed`/`AE1NotAllowed`/`AE2NotAllowed`/`AES{128,192,256}NotAllowed`) before decryption — the policy is a constructor row, never a flag or a second function.
- integrity axis: the encoder verifies streamed CRC32, compressed size, uncompressed size, and (for WinZip-AES) the HMAC-SHA1 tag, raising the `IntegrityError` subtree (`CRC32IntegrityError`/`CompressedSizeIntegrityError`/`UncompressedSizeIntegrityError`/`HMACIntegrityError`, all under `DataError` -> `UnzipValueError`); a fault rail catches `UnzipValueError` for recoverable boundary faults, `InvalidOperationError` for API misuse, and `UnzipError` for the root.
- evidence: each extraction captures member count, per-member name, declared uncompressed size, ZIP64 resolution, detected encryption mechanism, and verified-integrity status as a bundle-manifest receipt row.
- boundary: `stream_unzip` owns ZIP container parsing, ZIP64 extra-field decode, ZipCrypto keystream and WinZip-AES record handling (via `pycryptodome` AES/HMAC-SHA1/PBKDF2), and Deflate/Deflate64/Bzip2/Stored decompression (deflate64 via the `stream-inflate` `stream_inflate64` Cython decompressor, bzip2 via stdlib `bz2`, deflate/stored via stdlib `zlib`); `async_stream_unzip` is the async-iterable boundary over the same decoder; the source chunks come straight from a download/response/file stream and the member chunks go straight to a sink, with no whole-archive buffer; archive CONSTRUCTION routes to `stream-zip`, not this package.

[STACKING]:
- `stream-unzip` is the exact streamed inverse of `stream-zip`: a download or response body yields `Iterable[bytes]` straight into `stream_unzip`, which re-emits `(name, size, chunks)` so the `bundle/bundle` owner's `CompressionAlgo.ZIP_STREAM` unpack/list/test verb closes the round-trip the `stream-zip` `MemberFile`/`async_stream_zip` pack arm opens — without a `zipfile` whole-archive seek-and-buffer.
- the per-member `name`/`size`/mechanism triple is exactly the `BundleManifest` `(name, ContentKey, algo, size)` row material the bundle owner records, so a list/test pass iterates `stream_unzip` and drains each member's chunks through a `ContentIdentity`-keyed hash sink (test) or discards them (list) without materializing the payloads.
- each member's `chunks` generator feeds a downstream artifacts consumer directly — a recovered PDF stream into `documents/lens`, a recovered structured-text member into `ruamel-yaml`/`tomlkit`, or a `msgspec.msgpack.decode` over the member bytes — so the unpacked payload streams to its decoder without a temp file.
- `async_stream_unzip` consumes an `anyio`/`httpx` async byte stream so a remote bundle is extracted on the structured-concurrency rail without blocking, mirroring the `async_stream_zip` pack side.
- `allowed_encryption_mechanisms` set to `frozenset({NO_ENCRYPTION, AES_256})` hardens the unpack policy to refuse legacy ZipCrypto and weak AES key lengths, so the bundle owner's trust policy is a data value, not branching.

[RAIL_LAW]:
- Package: `stream-unzip`
- Owns: bounded-memory streaming ZIP extraction, per-member lazy `bytes` generators, Deflate/Deflate64/Bzip2/Stored streamed decompression, ZIP64 handling, ZipCrypto and WinZip-AES (AE-1/AE-2, AES-128/192/256) decryption, an encryption-mechanism accept allow-list, and streamed CRC32/size/HMAC integrity verification
- Accept: streaming ZIP unpack/list/test that consumes ordered `bytes` chunks from a download, response, or file source and re-emits `(name, size, chunks)` triples into the bundle inverse, decoding lazily into the document/structured-text/wire consumers

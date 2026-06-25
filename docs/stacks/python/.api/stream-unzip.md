# [PY_ARTIFACTS_API_STREAM_UNZIP]

`stream-unzip` supplies the bounded-memory streaming ZIP *extraction* surface for the artifacts bundle rail: a generator `stream_unzip` that consumes an iterable of raw ZIP `bytes` chunks and yields one `(name: bytes, size: int, data: Iterable[bytes])` tuple per member as it is decoded — never loading the whole archive nor any uncompressed member into memory — plus a thread-bridged `async_stream_unzip` mirror that drives the same engine from an async byte source under either asyncio or trio. It is the streamed inverse of `stream-zip`: it owns local-file-header parsing, Deflate / Deflate64 / BZip2 decompression, ZIP64 size resolution, the data-descriptor heuristic, WinZip-AES (AE-1/AE-2, 128/192/256) and legacy ZipCrypto decryption, and CRC32 / size / HMAC integrity verification — surfaces `zipfile` cannot reach (no AES, no Deflate64, no >4GiB Java ZIP64). The package owner composes `stream_unzip`/`async_stream_unzip` and the seven encryption-mechanism sentinels into the bundle ingest path; it removes the `zipfile.ZipFile` whole-archive buffering leak because every member is decoded as it streams, and it never re-implements the ZIP container parser, the ZipCrypto keystream, the WinZip-AES record layout, or the Deflate64 inflater that the native backend already owns.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `stream-unzip`
- package: `stream-unzip`
- import: `stream_unzip`
- owner: `artifacts`
- rail: bundle
- version: `0.0.101`; license `MIT` (`License :: OSI Approved :: MIT License`; `LICENSE` ships in the sdist)
- abi: maturin/PyO3 native build — NOT pure-Python. The package ships a compiled extension module `stream_unzip._zipcrypto` (a PyO3 `cdylib` built from `src/lib.rs` against `pyo3 0.28` + the `crc32-v2` crate) for the ZipCrypto keystream, so wheels are version-specific `cp3X-cp3X` platform wheels (`macosx_11_0_arm64`, `manylinux_2_17`, `musllinux`, `win_amd64`), published `cp310`..`cp314` with NO `cp315` wheel
- gate: no `cp315` wheel published for `0.0.101`, and the hard dependency `stream-inflate` is itself a Rust/maturin native wheel with no `cp315` build; the manifest pins `; python_version<'3.15'` so the package admits on cp314 and below, and the consuming bundle-ingest USAGE card is [BLOCKED] until a cp315 wheel lands for both `stream-unzip` and `stream-inflate`. Reflection over the active cp315 interpreter is therefore blocked (`assay api resolve stream-unzip` yields no PYDIST source); this surface is read from the lock-pinned sdist module (`python/stream_unzip/__init__.py`, `src/lib.rs`) and the official API docs, not live reflection
- requires-python: `>=3.7.7` (upstream floor; the cp315 GATE is wheel/ABI availability, not a source floor)
- depends-on: `pycryptodome>=3.10.1` (the AES-CTR / HMAC-SHA1 / PBKDF2 WinZip-AES backend imported as `AES`/`HMAC`/`SHA1`/`Counter`/`PBKDF2`); `stream-inflate>=0.0.12` (the native streaming Deflate64 inflater imported as `stream_inflate64`); stdlib `bz2`/`zlib` own BZip2 and raw-Deflate; `trio` is an optional runtime import used only when no asyncio loop is running
- entry points: library use is import-only; no console script
- capability: bounded-memory streaming ZIP extraction yielding per-member `(name, size, Iterable[bytes])`; Deflate (compression 8), Deflate64 (9, via `stream-inflate` — a format `zipfile` rejects), BZip2 (12), and stored (0) decompression; ZIP64 size resolution with an `allow_zip64` toggle; the 4-way data-descriptor format heuristic (Mark-Adler-style longest-match) for streamed members whose sizes trail the body; WinZip-AES (AE-1 / AE-2; 128 / 192 / 256-bit) and legacy ZipCrypto/Zip2.0 decryption (the native ZipCrypto path is ~10x faster than `zipfile`); a per-call `allowed_encryption_mechanisms` allowlist that rejects disallowed mechanisms (including *unencrypted*) with a typed fault; streamed CRC32 / compressed-size / uncompressed-size / AES-HMAC integrity verification; and a thread-bridged `async_stream_unzip` mirror over asyncio or trio

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: encryption-mechanism allowlist sentinels
- rail: bundle
- The seven sentinels are opaque `_Encryption` singletons (a private `NewType` over `object()` so callers cannot mint new values). They are the membership vocabulary of the `allowed_encryption_mechanisms` `Container`: a member whose mechanism is absent from the passed container raises the matching `EncryptionMechanismNotAllowed` subclass before any bytes decrypt. `NO_ENCRYPTION` is itself a mechanism — omit it to *reject* plaintext members. AES decryption requires BOTH an `AE_1`/`AE_2` spec sentinel AND at least one `AES_128`/`AES_192`/`AES_256` key-length sentinel in the container. The module-level default `_ALL_ENCRYPTIONS` admits all seven.

| [INDEX] | [SYMBOL]         | [TYPE_FAMILY]      | [RAIL]                                                                       |
| :-----: | :--------------- | :----------------- | :-------------------------------------------------------------------------- |
|  [01]   | `NO_ENCRYPTION`  | mechanism sentinel | allow unencrypted members; omit to reject plaintext                          |
|  [02]   | `ZIP_CRYPTO`     | mechanism sentinel | allow legacy ZipCrypto / Zip2.0 members (insecure; native fast path)         |
|  [03]   | `AE_1`           | mechanism sentinel | allow WinZip AE-1 AES members (pair with a key-length sentinel)              |
|  [04]   | `AE_2`           | mechanism sentinel | allow WinZip AE-2 AES members (CRC suppressed; pair with a key-length sentinel) |
|  [05]   | `AES_128`        | mechanism sentinel | allow 128-bit AES key length (pair with `AE_1`/`AE_2`)                        |
|  [06]   | `AES_192`        | mechanism sentinel | allow 192-bit AES key length (pair with `AE_1`/`AE_2`)                        |
|  [07]   | `AES_256`        | mechanism sentinel | allow 256-bit AES key length (pair with `AE_1`/`AE_2`)                        |

[PUBLIC_TYPE_SCOPE]: failure tree — `InvalidOperationError` arm (programmer fault)
- rail: bundle
- `UnzipError` is the single root of every explicitly-thrown failure. Faults raised by iterating the *source* `zipfile_chunks` pass through unchanged (a `httpx`/`obstore` transport error stays its own type), so a boundary rail catches `UnzipError` for archive-shape faults and the transport error separately. The first arm models caller misuse, not bad data.

| [INDEX] | [SYMBOL]                   | [TYPE_FAMILY] | [RAIL]                                                                      |
| :-----: | :------------------------- | :------------ | :------------------------------------------------------------------------- |
|  [01]   | `UnzipError`               | error root    | `Exception` base for every explicitly-thrown `stream_unzip` failure         |
|  [02]   | `InvalidOperationError`    | error         | `UnzipError`; caller-misuse base                                            |
|  [03]   | `UnfinishedIterationError` | error         | `InvalidOperationError`; advanced to the next member before draining the prior member's chunk iterator |

[PUBLIC_TYPE_SCOPE]: failure tree — `UnzipValueError` arm (data / password fault)
- rail: bundle
- `UnzipValueError(UnzipError, ValueError)` is the recoverable boundary base: every bad-archive, decompression, integrity, and password fault descends from it, so `except UnzipValueError` catches all recoverable input faults and `except ValueError` interops with generic validation. It forks into `DataError` (the bytes are wrong) and `PasswordError`/`MissingPasswordError` (credentials are wrong) — distinguish a corrupt archive from a bad password by which subtree fires.

| [INDEX] | [SYMBOL]                          | [TYPE_FAMILY] | [RAIL]                                                                  |
| :-----: | :-------------------------------- | :------------ | :--------------------------------------------------------------------- |
|  [01]   | `UnzipValueError`                 | error         | `UnzipError` + `ValueError`; recoverable boundary base                  |
|  [02]   | `DataError`                       | error         | `UnzipValueError`; the ZIP bytes themselves are invalid                 |
|  [03]   | `UncompressError`                 | error         | `DataError`; decompression-stage base                                  |
|  [04]   | `DeflateError`                    | error         | `UncompressError`; Deflate/Deflate64 stream could not be decompressed   |
|  [05]   | `BZ2Error`                        | error         | `UncompressError`; BZip2 stream could not be decompressed              |
|  [06]   | `UnsupportedFeatureError`         | error         | `DataError`; a required ZIP feature is unsupported                      |
|  [07]   | `UnsupportedFlagsError`           | error         | `UnsupportedFeatureError`; enhanced-deflate/patched/strong-encryption/masked-header flag set |
|  [08]   | `UnsupportedCompressionTypeError` | error         | `UnsupportedFeatureError`; compression method outside `{0,8,9,12}`      |
|  [09]   | `UnsupportedZip64Error`           | error         | `UnsupportedFeatureError`; ZIP64 member met while `allow_zip64=False`   |
|  [10]   | `NotStreamUnzippable`            | error         | `UnsupportedFeatureError`; stored member with a data-descriptor and no header size — not streamable |
|  [11]   | `TruncatedDataError`              | error         | `DataError`; the source byte stream ended mid-member                    |
|  [12]   | `UnexpectedSignatureError`        | error         | `DataError`; an unexpected ZIP section signature was read               |
|  [13]   | `MissingExtraError`               | error         | `DataError`; a required `extra` field is absent                         |
|  [14]   | `MissingAESExtraError`            | error         | `MissingExtraError`; the WinZip-AES `0x9901` extra is absent            |
|  [15]   | `TruncatedExtraError`             | error         | `DataError`; a required `extra` field is present but too short          |
|  [16]   | `TruncatedZip64ExtraError`        | error         | `TruncatedExtraError`; the ZIP64 extra is shorter than 16 bytes         |
|  [17]   | `TruncatedAESExtraError`          | error         | `TruncatedExtraError`; the AES extra is shorter than 7 bytes            |
|  [18]   | `InvalidExtraError`               | error         | `TruncatedExtraError`; an `extra` field holds an invalid value          |
|  [19]   | `InvalidAESKeyLengthError`        | error         | `TruncatedExtraError`; AES key-length byte is not `1`/`2`/`3`           |
|  [20]   | `IntegrityError`                  | error         | `DataError`; a post-decode integrity check failed                       |
|  [21]   | `HMACIntegrityError`              | error         | `IntegrityError`; the WinZip-AES HMAC-SHA1 authentication tag mismatched |
|  [22]   | `CRC32IntegrityError`             | error         | `IntegrityError`; the streamed CRC32 mismatched the declared value      |
|  [23]   | `SizeIntegrityError`              | error         | `IntegrityError`; a declared size mismatched the streamed length        |
|  [24]   | `UncompressedSizeIntegrityError`  | error         | `SizeIntegrityError`; uncompressed length mismatch                      |
|  [25]   | `CompressedSizeIntegrityError`    | error         | `SizeIntegrityError`; compressed length mismatch                        |
|  [26]   | `PasswordError`                   | error         | `UnzipValueError`; password-stage base                                  |
|  [27]   | `MissingPasswordError`            | error         | `UnzipValueError`; an encrypted member was met but `password=None`      |
|  [28]   | `MissingZipCryptoPasswordError`   | error         | `MissingPasswordError`; ZipCrypto member, no password                   |
|  [29]   | `MissingAESPasswordError`         | error         | `MissingPasswordError`; AES member, no password                         |
|  [30]   | `IncorrectPasswordError`          | error         | `PasswordError`; password-verification byte/word mismatched             |
|  [31]   | `IncorrectZipCryptoPasswordError` | error         | `IncorrectPasswordError`; wrong ZipCrypto password                      |
|  [32]   | `IncorrectAESPasswordError`       | error         | `IncorrectPasswordError`; wrong AES password (PBKDF2 verifier mismatch) |
|  [33]   | `EncryptionMechanismNotAllowed`   | error         | `PasswordError`; member mechanism absent from `allowed_encryption_mechanisms` |
|  [34]   | `FileIsNotEncrypted`              | error         | `EncryptionMechanismNotAllowed`; plaintext member met while `NO_ENCRYPTION` not allowed |
|  [35]   | `ZipCryptoNotAllowed`             | error         | `EncryptionMechanismNotAllowed`; ZipCrypto member while `ZIP_CRYPTO` not allowed |
|  [36]   | `AESNotAllowed`                   | error         | `EncryptionMechanismNotAllowed`; AES base for the four key/spec arms     |
|  [37]   | `AE1NotAllowed`                   | error         | `AESNotAllowed`; AE-1 member while `AE_1` not allowed                    |
|  [38]   | `AE2NotAllowed`                   | error         | `AESNotAllowed`; AE-2 member while `AE_2` not allowed                    |
|  [39]   | `AES128NotAllowed`                | error         | `AESNotAllowed`; 128-bit member while `AES_128` not allowed              |
|  [40]   | `AES192NotAllowed`                | error         | `AESNotAllowed`; 192-bit member while `AES_192` not allowed              |
|  [41]   | `AES256NotAllowed`                | error         | `AESNotAllowed`; 256-bit member while `AES_256` not allowed              |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: streaming extraction functions
- rail: bundle
- `stream_unzip` returns a lazy generator; iterating it drives the source `zipfile_chunks` member by member so peak memory stays bounded by `chunk_size` plus per-member decoder state. Each yielded item is `(name, size, data)` where `name` is the raw **`bytes`** of the member name (the caller decodes — UTF-8 when the ZIP UTF-8 flag is set, else CP437), `size` is the declared uncompressed size as an `int` (or `None` when a compressed member carries only a data descriptor), and `data` is a generator of the member's uncompressed `bytes`. Each member's `data` MUST be fully consumed before advancing to the next member — skipping a member raises `UnfinishedIterationError`; to discard a member, exhaust its iterator with `for _ in data: pass`. `password` (a single `bytes`) applies to every member; `allowed_encryption_mechanisms` constrains which mechanisms decode; `allow_zip64` toggles ZIP64 acceptance. `async_stream_unzip` mirrors the signature over async iterables.

| [INDEX] | [SURFACE]            | [CALL_SHAPE]                                                                                                                                                                                                                                          | [CAPABILITY]                                          |
| :-----: | :------------------- | :---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | :---------------------------------------------------- |
|  [01]   | `stream_unzip`       | `stream_unzip(zipfile_chunks: Iterable[bytes], password: Optional[bytes]=None, chunk_size: int=65536, allow_zip64: bool=True, allowed_encryption_mechanisms: Container=(NO_ENCRYPTION, ZIP_CRYPTO, AE_1, AE_2, AES_128, AES_192, AES_256)) -> Generator[tuple[bytes, int \| None, Generator[bytes]]]` | stream-extract a ZIP, yielding `(name, size, data)` per member |
|  [02]   | `async_stream_unzip` | `async_stream_unzip(chunks: AsyncIterable[bytes], password: Optional[bytes]=None, chunk_size: int=65536, allow_zip64: bool=True, allowed_encryption_mechanisms: Container=(…same defaults…)) -> AsyncGenerator[tuple[bytes, int \| None, AsyncGenerator[bytes]]]` | async mirror over an async byte source (asyncio or trio) |

## [04]-[IMPLEMENTATION_LAW]

[BUNDLE_STREAMING_UNZIP]:
- import: `from stream_unzip import stream_unzip, async_stream_unzip, ZIP_CRYPTO, AE_2, AES_256, NO_ENCRYPTION, UnzipValueError` at boundary scope only; module-level import is banned by the manifest import policy. Importing the package on the cp315 interpreter fails (no native wheel) — the import lives behind the `; python_version<'3.15'` gate.
- source axis: `zipfile_chunks` is any lazy `Iterable[bytes]`, so the archive is consumed straight from a `httpx` response body, an `obstore`/`fsspec` `GET` stream, or a file handle's chunked reads — the ZIP never fully materializes; `chunk_size` bounds the per-pull buffer, never a whole-archive read.
- member axis: each yielded `(name, size, data)` tuple owns one entry; `name` is raw `bytes` (caller decodes by the UTF-8 flag, never assume `str`), `size` is the declared uncompressed `int` or `None` under a compressed data descriptor, and `data` is a lazy `Generator[bytes]` decoded on demand; there is no per-member object — the tuple is the entry.
- consumption-order law: members are a single forward pass and each member's `data` generator MUST be drained before the next member is pulled — the engine raises `UnfinishedIterationError` (under `InvalidOperationError`) on a skipped member; discarding a member is an explicit `for _ in data: pass`, never a silent skip.
- decompression axis: the compression method on the local header selects the decoder — stored (`0`) via a sized passthrough, Deflate (`8`) via `zlib.decompressobj(wbits=-zlib.MAX_WBITS)`, Deflate64 (`9`) via the native `stream_inflate64` (a method `zipfile` rejects), BZip2 (`12`) via `bz2.BZ2Decompressor`; any other method raises `UnsupportedCompressionTypeError`. The decoder is a method-keyed selection, never a parallel per-codec API; a Deflate/BZip2 stream fault surfaces as `DeflateError`/`BZ2Error` under `UncompressError`.
- zip64 axis: `allow_zip64` (default `True`) gates ZIP64; the true compressed/uncompressed sizes are read from the `0x0001` ZIP64 extra when the 32-bit header fields are saturated (`0xFFFFFFFF`); a ZIP64 member under `allow_zip64=False` raises `UnsupportedZip64Error`. This is the path that reads Java `ZipOutputStream` archives >4GiB that libarchive-based readers fail on.
- data-descriptor axis: when a member carries its sizes/CRC in a trailing data descriptor (streamed-produced archives), the engine applies the 4-candidate longest-match heuristic (`(32|64) × (with-sig|without-sig)`, Mark-Adler-style) to recover the descriptor and verify it; a stored member with a data descriptor and no header size is `NotStreamUnzippable` (the one shape this streamer structurally cannot handle).
- encryption axis: `password` (one `bytes`) is the single decryption key for every member; the mechanism is detected per member (general-purpose-flag bit 0 plus compression `99` for AES) — legacy ZipCrypto decrypts through the native `_zipcrypto` PyO3 keystream (~10x `zipfile`), WinZip-AES decrypts through `pycryptodome` AES-CTR with a PBKDF2-derived key whose verifier word gates `IncorrectAESPasswordError`. An encrypted member with `password=None` raises the matching `MissingPasswordError` subclass; decryption is a per-call row, never a second function.
- allowlist axis: `allowed_encryption_mechanisms` is a security policy `Container` of the seven sentinels — a member whose mechanism is absent raises the matching `EncryptionMechanismNotAllowed` subclass *before* bytes decrypt (including `FileIsNotEncrypted` when `NO_ENCRYPTION` is withheld). AES requires both a spec sentinel (`AE_1`/`AE_2`) and a key-length sentinel (`AES_128`/`AES_192`/`AES_256`); this is the policy surface a config layer drives, never a per-mechanism branch at the call site.
- integrity axis: each member's streamed bytes are CRC32-checked against the declared value (`CRC32IntegrityError`), compressed/uncompressed lengths are size-checked (`CompressedSizeIntegrityError`/`UncompressedSizeIntegrityError` under `SizeIntegrityError`), and AES members are HMAC-SHA1 authenticated (`HMACIntegrityError`); AE-2 members suppress the CRC by design and rely on the HMAC. Verification is intrinsic to the decode, not an opt-in pass.
- fault axis: `UnzipError` is the root; `UnzipValueError` is the recoverable boundary base; `DataError` (bad bytes) and `PasswordError`/`MissingPasswordError` (bad credentials) are the two diagnosis forks; `InvalidOperationError` is caller misuse. A boundary rail catches `UnzipValueError` for recoverable input faults and `UnzipError` for the root; faults from the *source* iterable pass through unchanged and are caught as their own transport type.
- evidence: each extraction captures member count, per-member decoded name, declared method, ZIP32-vs-ZIP64 resolution, encryption mechanism, declared-vs-actual size, CRC32/HMAC verification outcome, and streamed byte length as a bundle-ingest receipt.
- boundary: `stream_unzip` owns ZIP local-header parsing, the data-descriptor heuristic, ZIP64 size resolution, the decode of stored/Deflate/Deflate64/BZip2 members, ZipCrypto (native `_zipcrypto`) and WinZip-AES (`pycryptodome`) decryption, and CRC32/size/HMAC integrity; `async_stream_unzip` is the async-iterable boundary over the same synchronous engine (thread-bridged); the producing inverse routes to `stream-zip` (the same author's bundle writer), not this package; container formats with their own engines (`py7zr` for 7z, `pyzipper`-class needs) stay outside.

[STACKING]:
- chunk-source seam: `zipfile_chunks` is a bare `Iterable[bytes]`, so a `httpx` `Response.iter_bytes(chunk_size=65536)` body, an `obstore.get(...).stream()` / `fsspec` `open(...).read`-loop, or a `universal-pathlib` `UPath` read feeds the extractor with zero buffering — the artifacts bundle owner reads a remote ZIP and re-emits members without ever holding the archive; `async_stream_unzip` consumes the async twin (`Response.aiter_bytes`, `obstore` async stream) directly as the official docs show with `httpx.AsyncClient`.
- retry seam: the chunk source is fallible I/O, so the ingest wraps the *download* (not the decode) in a `stamina` `retry_context` / `@retry(on=httpx.TransportError, …)` (`.api/stamina.md`) — because source-iterable exceptions pass through `stream_unzip` unchanged, the retry rail catches the transport error as its own type while `UnzipValueError` stays a terminal decode fault that is never retried; a `RetryDetails`-fed receipt records attempts around the streamed pull.
- decode seam: each member's `data` generator yields `bytes` that flow straight into the structured-decode rail — `msgspec.json.decode` / `msgspec.msgpack.decode` into a typed `Struct` (`.api/msgspec.md`), or a `pydantic` `TypeAdapter(...).validate_json` (`.api/pydantic.md`) — so a ZIP of JSON/MsgPack member files is parsed-not-validated member by member without a temp file; the `bytes` member name is decoded by the UTF-8 flag at this same boundary before keying the parsed model.
- result-rail seam: the whole extract is an `expression` `Result` pipeline (`.api/expression.md`) — `Ok((name, size))` per verified member, `Error(BundleFault(...))` mapped from the `UnzipValueError` subtree — so a corrupt member (`CRC32IntegrityError`), a wrong password (`IncorrectAESPasswordError`), and a disallowed mechanism (`ZipCryptoNotAllowed`) are three distinct typed failure rows on one rail, never raised across the bundle boundary.
- policy seam: `allowed_encryption_mechanisms` is driven from configuration, not hardcoded — a `pydantic-settings` (`.api/pydantic-settings.md`) field resolves the operator's accepted set (e.g. AES-only ingest passes `(AE_2, AES_256)` and *withholds* `NO_ENCRYPTION`+`ZIP_CRYPTO`, so plaintext or legacy members fault as `FileIsNotEncrypted`/`ZipCryptoNotAllowed` at the boundary), making the security envelope a typed config row the rail reads.
- observability seam: the extract is spanned with `structlog`+OpenTelemetry (`.api/structlog.md`, `.api/opentelemetry-api.md`) — one span per archive with member-count/byte-total attributes and a child event per `IntegrityError`/`PasswordError`, so a failed authentication or CRC is a structured log fact, never a bare traceback.
- concurrency seam: `async_stream_unzip` auto-detects the running loop and bridges through `trio.to_thread` under trio or `loop.run_in_executor` under asyncio, so it rides whichever runtime the transport already uses (`.api/anyio.md` / `.api/trio.md` task groups own the surrounding structured concurrency); the underlying engine is synchronous-over-threads, so it is the I/O-bound async edge, not a CPU-parallel decoder — fan-out of *many* archives belongs to the task group, not to one call.
- round-trip seam: `stream-zip` (`.api/stream-zip.md`, the same author's writer) and `stream-unzip` close the bundle loop — `stream_zip` yields `(name: str, …)` container bytes an upload consumes, `stream_unzip` consumes those same chunked bytes and re-emits `(name: bytes, size, data)`; the name-type asymmetry (`str` out, `bytes` in) is the one boundary the round-trip owner reconciles by decoding the extracted name with the encoding the writer used.

[RAIL_LAW]:
- Package: `stream-unzip`
- Owns: bounded-memory streaming ZIP extraction; stored/Deflate/Deflate64/BZip2 decompression; ZIP64 size resolution; the data-descriptor format heuristic; ZipCrypto (native) and WinZip-AES (AE-1/AE-2; 128/192/256) decryption; a per-call encryption-mechanism allowlist; and intrinsic CRC32/size/HMAC integrity verification, with a thread-bridged asyncio/trio async mirror
- Accept: streaming ZIP *ingest* that consumes a lazy `bytes` source (download, object-store stream, or file) and re-emits verified members lazily into the decode/result rail, with the encryption envelope driven from typed config
- Reject: wrapper-renames of `stream_unzip`/`async_stream_unzip`; a `zipfile.ZipFile` whole-archive buffering path where streaming bounds memory; admitting `zipfile`/`pyzipper` for AES or Deflate64 members `stream_unzip` already decodes; a hand-rolled ZipCrypto keystream where the native `_zipcrypto` PyO3 module already decrypts; a hand-rolled WinZip-AES record/HMAC path where the `pycryptodome` backend already authenticates; a hand-rolled Deflate64 inflater where `stream-inflate` already owns it; a parallel member type per compression or encryption mode; a second function for the encrypted/async path the `password` row and `async_stream_unzip` already own; retrying a terminal `UnzipValueError` decode fault; skipping a member's `data` generator instead of draining it; assuming the member `name` is `str`; or a hand-rolled extractor where `stream-zip` owns the producing inverse

# [PY_ARTIFACTS_API_C2PA_PYTHON]

`c2pa-python` supplies the C2PA content-credentials surface for the artifacts provenance rail: a `Builder` that loads a JSON manifest definition and embeds a signed manifest into an asset via `sign`/`sign_file`, a `Reader` that extracts and validates the manifest store from a path or stream and exposes `json`/`get_validation_state`/`get_validation_results`, a `Signer` factory family backed by `C2paSignerInfo` and `C2paSigningAlg`, and a `Settings`/`Context` configuration pair. The package owner composes `Context.from_dict`, `Builder.from_json`/`sign`, `Reader.try_create`/`with_fragment`/`json`/`resource_to_stream`, and `Signer.from_info`/`from_callback` into the provenance path; the native `libc2pa_c` core owns JUMBF/COSE encoding and validation-state computation.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `c2pa-python`
- package: `c2pa-python`
- import: `c2pa`
- owner: `artifacts`
- rail: provenance
- installed: `0.36.0` (native `c2pa-rs` core `sdk_version()` → `0.89.0`)
- marker: pure-Python ctypes binding over a bundled `libc2pa_c` native core (`libs/libc2pa_c.dylib`/`.rlib`); the wheel ships the platform core, so no cp-gate and no compiler — `c2pa-python 0.36.0` resolves a cp315 wheel directly
- license: `MIT OR Apache-2.0` (permissive dual-license; no copyleft obligation)
- entry points: console script `download-artifacts` (native-library fetch helper, `c2pa.build:main`); library use is import-only
- capability: C2PA manifest authoring from a JSON definition, embedded/sidecar manifest signing into the native 57-entry signable set (`Builder.get_supported_mime_types()` — JPEG/PNG/TIFF/GIF/WebP/AVIF/HEIF/HEIC/JXL/DNG/SVG+XHTML+XML image-and-markup formats, BMFF MP4/M4A/M4V/QuickTime-MOV/AVI/fragmented-DASH, WAV/FLAC/MP3 audio, and the `application/c2pa`/`application/x-c2pa-manifest-store` container types), ingredient attachment from stream/archive, whole-builder + per-ingredient archive serialize/rehydrate, manifest-store extraction and parsing (`json`/`detailed_json`/`crjson`), `validation_state` and `validation_results` reporting, callback- and `C2paSignerInfo`-backed signers across ES256/ES384/ES512/PS256/PS384/PS512/ED25519, standalone `format_embeddable`/`ed25519_sign` byte primitives, and per-instance `Settings`/`Context` configuration. The 63-entry `Reader.get_supported_mime_types()` readable set is a strict superset of the signable set; the read-only delta is exactly `{application/pdf, pdf, arw, image/x-sony-arw, nef, image/x-nikon-nef}` — PDF and raw-camera (Sony `arw`/Nikon `nef`) assets are READ-ONLY in this SDK (they appear only in the Reader set, not the Builder set), so PDF provenance-signing routes to the `pyhanko` PAdES rail (`exchange/conformance`), never `Builder.sign`

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: builder, reader, signer, and configuration roots
- rail: provenance

`Builder`, `Reader`, `Signer`, `Settings`, and `Context` are all context managers with `close()`/`is_valid` lifecycle (the native handle is released on `close`); `Builder` is single-sign and closes after `sign`; `Reader` is closed after the `with` block; `Signer` is consumed when attached to a `Context`. `C2paError` is the base of the full typed subclass family — `Assertion`, `AssertionNotFound`, `Decoding`, `Encoding`, `FileNotFound`, `Io`, `Json`, `Manifest`, `ManifestNotFound`, `NotSupported`, `Other`, `RemoteManifest`, `ResourceNotFound`, `Signature`, `Verify` — so a single `except C2paError` rail traps the family and per-subclass arms discriminate codec/signature/verify/not-found faults. `C2paSigningAlg`, `C2paDigitalSourceType`, `C2paBuilderIntent`, and `C2paSeekMode` are `IntEnum` vocabularies driving signing, source provenance, manifest intent, and stream-seek mode. `C2paDigitalSourceType` carries the full 19-member IPTC digital-source vocabulary — `EMPTY` (0, the no-source default `set_intent` supplies) plus the eighteen origins `TRAINED_ALGORITHMIC_DATA`, `DIGITAL_CAPTURE`, `COMPUTATIONAL_CAPTURE`, `NEGATIVE_FILM`, `POSITIVE_FILM`, `PRINT`, `HUMAN_EDITS`, `COMPOSITE_WITH_TRAINED_ALGORITHMIC_MEDIA`, `ALGORITHMICALLY_ENHANCED`, `DIGITAL_CREATION`, `DATA_DRIVEN_MEDIA`, `TRAINED_ALGORITHMIC_MEDIA`, `ALGORITHMIC_MEDIA`, `SCREEN_CAPTURE`, `VIRTUAL_RECORDING`, `COMPOSITE`, `COMPOSITE_CAPTURE`, `COMPOSITE_SYNTHETIC` (ordinals 1–18) — so AI-and-capture provenance origin is a single table-driven source-type row, not a parallel manifest field; the `exchange/credential#CREDENTIAL` `DigitalSource` `StrEnum` mirrors these eighteen non-`EMPTY` names exactly so its `_DIGITAL_SOURCE` row derives by `C2paDigitalSourceType[name]` correspondence. `C2paSeekMode` (`START`/`CURRENT`/`END` = 0/1/2) drives the `Stream` seek-callback bridge and is exported but rarely touched by the owner (the `Stream` adapter consumes it internally). `LifecycleState` (`UNINITIALIZED`/`ACTIVE`/`CLOSED`) is the internal `ManagedResource` state machine — NOT re-exported at package scope and never an owner-facing member.

| [INDEX] | [SYMBOL]                | [TYPE_FAMILY]    | [RAIL]                                                           |
| :-----: | :---------------------- | :--------------- | :--------------------------------------------------------------- |
|  [01]   | `Builder`               | resource         | manifest authoring + single-use signing into an asset            |
|  [02]   | `Reader`                | resource         | manifest-store extraction, parsing, and validation reporting     |
|  [03]   | `Signer`                | resource         | COSE signer from `C2paSignerInfo` or callback                    |
|  [04]   | `Stream`                | resource         | Python stream-to-native `C2paStream` bridge                      |
|  [05]   | `Context`               | resource         | per-instance settings + signer carrier for `Builder`/`Reader`    |
|  [06]   | `ContextBuilder`        | builder          | fluent `with_settings`/`with_signer`/`build` for `Context`       |
|  [07]   | `Settings`              | resource         | dot-path / JSON / dict configuration object                      |
|  [08]   | `ContextProvider`       | protocol         | interface a `Context` implements for `Builder`/`Reader`          |
|  [09]   | `C2paSignerInfo`        | value (ctypes)   | `alg`/`sign_cert`/`private_key`/`ta_url` signer configuration    |
|  [10]   | `C2paSigningAlg`        | enum (`IntEnum`) | ES256/ES384/ES512/PS256/PS384/PS512/ED25519 signing algorithm    |
|  [11]   | `C2paDigitalSourceType` | enum (`IntEnum`) | 19-member IPTC digital-source provenance for `CREATE` intent     |
|  [12]   | `C2paBuilderIntent`     | enum (`IntEnum`) | `CREATE`/`EDIT`/`UPDATE` manifest intent                         |
|  [13]   | `C2paSeekMode`          | enum (`IntEnum`) | `START`/`CURRENT`/`END` stream-seek mode for the `Stream` bridge |
|  [14]   | `C2paError`             | error            | base exception with typed subclass attributes                    |

[EXPORT_TIERS]: the package public surface (`c2pa.__all__`) is the 15-name set `Builder`, `Reader`, `Signer`, `Stream`, `Settings`, `Context`, `ContextBuilder`, `ContextProvider`, `C2paError`, `C2paSignerInfo`, `C2paSigningAlg`, `C2paDigitalSourceType`, `C2paBuilderIntent`, `sdk_version`, `load_settings` — the owner's `from c2pa import (...)` line draws from this set. The inner `c2pa.c2pa.__all__` additionally exposes `C2paSeekMode`, `format_embeddable`, and `version` (the `importlib.metadata` package-version, distinct from `sdk_version` the native-core version); `create_signer`/`create_signer_from_info`/`ed25519_sign` are module functions on `c2pa.c2pa` not in either `__all__`. The owner reaches the inner-only members through `from c2pa.c2pa import format_embeddable` only when the embeddable-bytes primitive is genuinely needed; the canonical signing/reading path never leaves the `c2pa` public set.

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: `Builder` author and sign
- rail: provenance

`Builder` is constructed from a JSON manifest definition (string or dict); `from_json` is the named factory and `from_archive` rehydrates a builder from a written archive stream. `sign` discriminates on its first argument: a `Signer` signs explicitly, a `str` format signs with the `Context`'s signer. `sign` is single-use and closes the builder; omitting `dest` buffers the signed asset into an in-memory `BytesIO`. Below, the members are `Builder` methods with bare param names: `manifest_json`/`ingredient_json`/`action_json` are `str | dict`, `context` is `Optional[ContextProvider]`, `source`/`stream`/`dest` are file-like, `set_intent` defaults `digital_source_type=C2paDigitalSourceType.EMPTY`, and `sign` takes `(signer_or_format, format_or_source=None, source_or_dest=None, dest=None)`.

| [INDEX] | [SURFACE]                     | [CALL_SHAPE]                                              | [CAPABILITY]                               |
| :-----: | :---------------------------- | :-------------------------------------------------------- | :----------------------------------------- |
|  [01]   | `Builder`                     | `Builder(manifest_json, context=None)`                    | construct from a JSON manifest             |
|  [02]   | `from_json`                   | `from_json(manifest_json, context=None) -> Builder`       | named factory from a JSON manifest         |
|  [03]   | `from_archive`                | `from_archive(stream) -> Builder`                         | rehydrate a builder from an archive        |
|  [04]   | `sign`                        | `sign(signer_or_format, ..., dest=None) -> bytes`         | sign source into dest; single-use          |
|  [05]   | `sign_file`                   | `sign_file(source_path, dest_path, signer=None) -> bytes` | sign a file path to an output path         |
|  [06]   | `set_intent`                  | `set_intent(intent, digital_source_type=EMPTY)`           | set manifest intent + digital source       |
|  [07]   | `add_ingredient`              | `add_ingredient(ingredient_json, format, source)`         | attach a parent/component ingredient       |
|  [08]   | `add_ingredient_from_stream`  | `add_ingredient_from_stream(json, format, source)`        | attach an ingredient from an open stream   |
|  [09]   | `add_ingredient_from_archive` | `add_ingredient_from_archive(stream)`                     | rehydrate ingredients from an archive      |
|  [10]   | `write_ingredient_archive`    | `write_ingredient_archive(ingredient_id, stream)`         | serialize an ingredient to an archive      |
|  [11]   | `add_resource`                | `add_resource(uri, stream)`                               | attach a referenced resource by URI        |
|  [12]   | `add_action`                  | `add_action(action_json)`                                 | append a `c2pa.actions` assertion          |
|  [13]   | `set_remote_url`              | `set_remote_url(remote_url)`                              | set the remote manifest URL                |
|  [14]   | `set_no_embed`                | `set_no_embed()`                                          | produce a sidecar (non-embedded) manifest  |
|  [15]   | `to_archive`                  | `to_archive(stream)`                                      | serialize builder state to an archive      |
|  [16]   | `with_archive`                | `with_archive(stream) -> Builder`                         | reload a builder's definition from archive |
|  [17]   | `get_supported_mime_types`    | `get_supported_mime_types() -> list[str]`                 | native signable MIME types                 |

[ENTRYPOINT_SCOPE]: `Reader` extract and validate
- rail: provenance

`Reader(format_or_path)` reads a manifest store: a sole path argument resolves the MIME type and opens the file, a `(format, stream)` pair reads from an open stream, and a `(format, path)` pair reads a named path. `try_create` is the optional-returning factory that returns `None` instead of raising `C2paError.ManifestNotFound` when an asset carries no Content Credentials. `get_validation_state` reads the manifest-store `validation_state` field; `get_validation_results` reads the `validation_results` object. Both `Reader` and `try_create` take `(format_or_path, stream=None, manifest_data=None, context=None)`; the members below are `Reader` methods with bare param names.

| [INDEX] | [SURFACE]                  | [CALL_SHAPE]                                               | [CAPABILITY]                                |
| :-----: | :------------------------- | :--------------------------------------------------------- | :------------------------------------------ |
|  [01]   | `Reader`                   | `Reader(format_or_path, stream=None, ...)`                 | open a manifest store (path/stream)         |
|  [02]   | `try_create`               | `try_create(format_or_path, ...) -> Optional[Reader]`      | open, or `None` when no manifest is present |
|  [03]   | `json`                     | `json() -> str`                                            | manifest store as JSON (cached)             |
|  [04]   | `detailed_json`            | `detailed_json() -> str`                                   | expanded manifest-store JSON                |
|  [05]   | `crjson`                   | `crjson() -> str`                                          | standardized crJSON; `"{}"` when none       |
|  [06]   | `get_validation_state`     | `get_validation_state() -> str \| None`                    | overall `validation_state` field            |
|  [07]   | `get_validation_results`   | `get_validation_results() -> dict \| None`                 | detailed `validation_results` object        |
|  [08]   | `get_active_manifest`      | `get_active_manifest() -> dict \| None`                    | the active manifest dict                    |
|  [09]   | `get_manifest`             | `get_manifest(label) -> dict \| None`                      | a manifest dict by label/ID                 |
|  [10]   | `is_embedded`              | `is_embedded() -> bool`                                    | embedded vs remote manifest flag            |
|  [11]   | `get_remote_url`           | `get_remote_url() -> str \| None`                          | remote manifest URL, or `None` if embedded  |
|  [12]   | `resource_to_stream`       | `resource_to_stream(uri, stream) -> int`                   | write a referenced resource to a stream     |
|  [13]   | `with_fragment`            | `with_fragment(format, stream, fragment_stream) -> Reader` | instance-chain fragmented BMFF              |
|  [14]   | `get_supported_mime_types` | `get_supported_mime_types() -> list[str]`                  | native readable MIME types                  |

[ENTRYPOINT_SCOPE]: signer, settings, and module functions
- rail: provenance

`Signer.from_info` builds a COSE signer from a populated `C2paSignerInfo`; `Signer.from_callback` builds one from an external signing callback plus algorithm and PEM cert chain. `Settings`/`Context` carry per-instance configuration into `Builder`/`Reader`; module-level `load_settings` is the deprecated thread-local path the `Context` per-instance path supersedes. `sdk_version` returns the underlying `c2pa-rs` core semantic version (`0.89.0` on the installed `c2pa-python 0.36.0`), distinct from `version()` which returns the `c2pa-python` distribution version. `format_embeddable` and `ed25519_sign` are standalone byte primitives — the former rewraps a detached manifest into the embeddable wire form for a given format, the latter is the in-process Ed25519 raw-signature primitive a `from_callback` digest-signer composes. `create_signer`/`create_signer_from_info` are the pre-0.11 deprecated function forms of `Signer.from_callback`/`Signer.from_info` (each emits a `DeprecationWarning` and delegates) — the owner uses the `Signer` classmethods, never these.

| [INDEX] | [SURFACE]                 | [CALL_SHAPE]                                                  | [CAPABILITY]                               |
| :-----: | :------------------------ | :------------------------------------------------------------ | :----------------------------------------- |
|  [01]   | `Signer.from_info`        | `from_info(signer_info) -> Signer`                            | signer from cert/key/TSA configuration     |
|  [02]   | `Signer.from_callback`    | `from_callback(callback, alg, certs, tsa_url=None) -> Signer` | signer from an external callback           |
|  [03]   | `Signer.reserve_size`     | `reserve_size() -> int`                                       | byte size reserved for the signature       |
|  [04]   | `C2paSignerInfo`          | `C2paSignerInfo(alg, sign_cert, private_key, ta_url)`         | signer config (`alg` enum/str/bytes)       |
|  [05]   | `Settings.from_json`      | `from_json(json_str) -> Settings`                             | settings from a JSON config string         |
|  [06]   | `Settings.from_dict`      | `from_dict(config) -> Settings`                               | settings from a config dict                |
|  [07]   | `Settings.set`            | `set(path, value) -> Settings`                                | set a dot-notation config value            |
|  [08]   | `Settings.update`         | `update(data) -> Settings`                                    | merge a config dict/JSON into settings     |
|  [09]   | `Context.from_json`       | `from_json(json_str, signer=None) -> Context`                 | context from JSON config + signer          |
|  [10]   | `Context.from_dict`       | `from_dict(config, signer=None) -> Context`                   | context from a config dict + signer        |
|  [11]   | `Context.has_signer`      | `has_signer() -> bool`                                        | whether the context carries a signer       |
|  [12]   | `Context.builder`         | `builder() -> ContextBuilder`                                 | fluent `with_settings`/`with_signer`       |
|  [13]   | `load_settings`           | `load_settings(settings, format="json") -> None`              | deprecated thread-local settings load      |
|  [14]   | `sdk_version`             | `sdk_version() -> str`                                        | `c2pa-rs` core version (`0.89.0`)          |
|  [15]   | `version`                 | `version() -> str`                                            | `c2pa-python` version (`0.36.0`)           |
|  [16]   | `format_embeddable`       | `format_embeddable(format, manifest_bytes) -> tuple`          | rewrap a detached manifest to wire bytes   |
|  [17]   | `ed25519_sign`            | `ed25519_sign(data, private_key) -> bytes`                    | in-process Ed25519 raw-signature (64-byte) |
|  [18]   | `create_signer`           | `create_signer(callback, alg, certs, tsa_url=None)`           | DEPRECATED; use `Signer.from_callback`     |
|  [19]   | `create_signer_from_info` | `create_signer_from_info(signer_info)`                        | DEPRECATED; use `Signer.from_info`         |

## [04]-[IMPLEMENTATION_LAW]

[PROVENANCE_CONTENT_CREDENTIALS]:
- import: `import c2pa` at boundary scope only; module-level import is banned by the manifest import policy.
- author axis: one `Builder.from_json` owns manifest authoring; intent, ingredients, resources, actions, and remote URL are call rows on the builder, never a per-shape builder type; archive rehydration is two rows on the same surface — `from_archive` classmethod-constructs and `with_archive` reloads an existing builder's definition under its bound `Context`; ingredient attachment is the `add_ingredient` (path/source) / `add_ingredient_from_stream` (open source stream) / `add_ingredient_from_archive` (single-arg written-archive stream) row family with `write_ingredient_archive` serializing one attached ingredient for reuse.
- sign axis: `Builder.sign` is the single signing surface keyed by first-argument shape — a `Signer` signs explicitly, a format `str` signs with the `Context` signer; `sign_file` is the path-to-path row; the builder is single-use and closes after signing, so one builder owns one signed asset.
- read axis: `Reader(format_or_path)` is the single extraction surface keyed by argument shape (path, `(format, stream)`, `(format, path)`); `try_create` is the optional-returning row that maps `ManifestNotFound` to `None`; `json`/`detailed_json`/`crjson` are projection rows over the same store, never parallel reader types.
- validation axis: `get_validation_state` reads the `validation_state` string and `get_validation_results` reads the `validation_results` object from the parsed manifest store; validation status is a field read off the store, never a recomputed local verdict.
- signer axis: `Signer.from_info` consumes a `C2paSignerInfo` (`alg`/`sign_cert`/`private_key`/`ta_url`) and `Signer.from_callback` consumes an external callback plus `C2paSigningAlg`; the algorithm is an enum row across ES/PS/ED25519, never a per-algorithm signer type.
- evidence: each operation captures SDK version (`sdk_version`), signing algorithm, embedded-vs-remote flag, manifest label, `validation_state`, and `validation_results` codes as a provenance receipt.
- boundary: `c2pa-python` owns C2PA manifest authoring, signing, extraction, and validation through the native `libc2pa_c` core with Python stream bridging via `Stream`; asset bytes flow in and out as streams or paths supplied by the imaging/document owners; certificate and key material is supplied by the campaign signer configuration, never minted here; live UI stays outside this package.

[INTEGRATION_STACK]:
- `c2pa-python` vs `pyhanko` (PAdES): C2PA Content Credentials and PAdES PDF signatures are orthogonal provenance rails with a hard format split in this SDK — C2PA owns embedded-manifest provenance for the image/BMFF/audio signable set and can only READ a PDF manifest store, while `pyhanko` owns the PDF-native PAdES/CMS signature path; PDF is therefore signed by `pyhanko` and (optionally) carries an additional C2PA manifest that this SDK reads but does not write. Neither re-implements the other's signature container; the campaign provenance owner selects the rail per asset class by the Builder's signable-MIME set, never wraps one in the other.
- `Signer.from_callback` is the seam to the `cryptography`/HSM signer: the external callback returns the raw COSE signature bytes for a digest, so private-key material lives in the `cryptography` keyring or an HSM and `c2pa` never holds it; `C2paSigningAlg` and the PEM `certs` chain are the only crypto config crossing the seam.
- asset stream seam: the in-memory asset buffer is the wire between the imaging/PDF owner and `c2pa` — `Builder.sign(signer, format, source_stream, dest_stream)` reads and writes `BytesIO`, so the imaging codec hands a decoded buffer and receives a signed buffer with no intermediate file; `sign_file` is the path convenience only when a file handle already exists.
- AI-provenance seam: a generated asset sets `Builder.set_intent(C2paBuilderIntent.CREATE, C2paDigitalSourceType.TRAINED_ALGORITHMIC_MEDIA)` so the digital-source type is the table-driven AI-origin signal the verifier reads, never a free-text manifest field.

[TWO_TIER_STACK]: the `exchange/credential#CREDENTIAL` `Provenance` owner stacks this folder-tier package ONTO the shared/universal `libs/python/.api` rails — the catalog members are never called raw; they compose under the substrate combinators into one dense rail.
- `expression` (`libs/python/.api/expression.md`): the `Reader.try_create` `Optional[Reader]` and the `Reader.with_fragment` raised `ManifestNotFound` BOTH fold onto one `Option[Reader]` (`Option.of_optional` for the `None`, the `except C2paError.ManifestNotFound: Nothing` arm for the raise), so a credential-free asset OR fragment projects `CredentialEvidence.unsigned` symmetrically; `Reader.get_remote_url()`'s `Optional[str]` projects through `Option.of_optional(...).default_value("")`. `Provenance` itself is the closed `@tagged_union(frozen=True)` (`tag`/`case`) — `Sign`/`Read`/`ReadFragment`/`Embed`/`ArchiveIngredient` cases keyed by payload shape, every `_run` arm closed by `assert_never`. No `try/except` ladder, no `None`-threading — the SDK's two distinct absence shapes meet one `Option` carrier.
- `msgspec` (`libs/python/.api/msgspec.md`): the `Reader.json()` manifest-STORE string is decoded ONCE through a module-level `json.Decoder(type=_Store)` into a typed `_Store`/`_Manifest`/`_ValidationResults` tree — the `active_manifest` KEY, the `manifests` chain, `validation_state`, the `signature_info` fields, and the `validation_results.{success,informational,failure}[].code` sets all arrive in one decode with `rename=` mapping the camelCase store keys; the heterogeneous per-assertion `data` is held opaque as `msgspec.Raw` and decoded to `_ActionData` only for the `c2pa.actions` label. This collapses the `get_active_manifest`/`get_validation_state`/`get_validation_results`/`get_manifest`-loop accessor family into one typed read — the accessors are the catalog's `[02]`/`[06]`/`[07]`/`[09]` Reader rows the design deliberately does NOT call once it holds the decoded `_Store`.
- `beartype` (`libs/python/.api/beartype.md`): the `SignerSpec._cose(policy)` projector (the `Signer.from_info`/`from_callback` seam) carries `@beartype(conf=FAULT_CONF)` so a malformed signer field faults at the boundary instead of inside ctypes.
- `stamina` (`libs/python/runtime/.api/stamina.md`): `@stamina.retry(on=(C2paError.RemoteManifest, C2paError.Io), attempts=3)` weaves `_run` so only remote-manifest and I/O transients re-attempt.
- retry boundary: RFC-3161 timestamp fetches and remote-manifest fetches retry; `C2paError.Decoding`/`Signature`/`NotSupported` surfaces immediately. The typed subclass family makes selective retry expressible; `except C2paError` over-traps.
- `anyio` (`libs/python/.api/anyio.md`): the native `libc2pa_c` core is GIL-releasing and the `ta_url`/remote transport is blocking I/O, so `Builder.sign`/`Reader.json`/`with_fragment` cross the runtime `LanePolicy.offload(..., modality=Modality.THREAD)` seam — the lane owning the band, boundary, and rail — keeping the work off the event loop. The in-process ctypes binding needs NO `Modality.PROCESS` lane (distinct from the subprocess-crossing `exchange/detect`/`metadata` siblings).
- runtime `content_identity` + `xxhash` (`libs/python/.api/xxhash.md`): `ContentIdentity.key("credential.<tag>", asset_bytes)` mints the `ContentKey` after the thread returns, and the canonical preimage is the complete signed-asset octets — the `bytes` `Builder.sign` returns on the sign arm, the identical octets handed to `Reader` on the read arm — so every producer, the `csharp:Rasm.Persistence` `XxHash128` re-derivation included, hashes one byte sequence and the keys collide by construction.
- `opentelemetry` + `structlog` (`libs/python/.api/opentelemetry-*.md`, `structlog.md`): `validation_state` projects onto `ArtifactReceipt.Credential`; validation code sets and `sdk_version()` remain on the rich `CredentialEvidence` returned by `close`.

[RAIL_LAW]:
- Package: `c2pa-python`
- Owns: C2PA manifest authoring from a JSON definition, embedded/sidecar manifest signing, manifest-store extraction and parsing, `validation_state`/`validation_results` reporting, and COSE signer construction across ES/PS/ED25519
- Accept: signing and validating Content Credentials on assets feeding the provenance, imaging, and document owners
- Reject: wrapper-renames of `Builder.sign`/`Reader`/`get_validation_state`; a hand-rolled JUMBF/COSE manifest codec; a local re-computation of `validation_state` instead of reading the store field; a parallel reader/builder type per asset format or signing algorithm; routing a PDF or raw-camera (`arw`/`nef`) asset to `Builder.sign` (read-only here — PDF signing is the `pyhanko` rail); certificate or key minting the campaign signer config owns

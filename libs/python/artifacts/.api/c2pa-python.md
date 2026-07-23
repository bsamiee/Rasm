# [PY_ARTIFACTS_API_C2PA_PYTHON]

`c2pa-python` owns the C2PA content-credentials rail for `artifacts` provenance: a `Builder` embeds a signed manifest from a JSON definition into an asset, a `Reader` extracts and validates a manifest store, and a `Signer` family mints the COSE signer across the ES/PS/ED25519 algorithms. Native core `libc2pa_c` owns JUMBF/COSE encoding and validation-state computation; asset bytes cross as streams or paths supplied by the imaging and document owners, and certificate material comes from the campaign signer config, never minted here.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `c2pa-python`
- package: `c2pa-python`
- import: `c2pa`
- owner: `artifacts`
- rail: provenance
- license: `MIT OR Apache-2.0` (permissive dual-license, no copyleft obligation)
- marker: pure-Python ctypes binding over a bundled `libc2pa_c` native core (`libs/libc2pa_c.dylib`/`.rlib`); the wheel ships the platform core, so no compiler and no cp-gate
- entry points: console script `download-artifacts` fetches the native library (`c2pa.build:main`); library use is import-only
- capability: manifest authoring from a JSON definition, embedded/sidecar signing into the native signable MIME set, ingredient attachment from stream or archive, whole-builder and per-ingredient archive serialize/rehydrate, manifest-store extraction and parsing (`json`/`detailed_json`/`crjson`), `validation_state`/`validation_results` reporting, callback/`C2paSignerInfo` signers, standalone `format_embeddable`/`ed25519_sign` byte primitives, and per-instance `Settings`/`Context` configuration

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: builder, reader, signer, and configuration roots
- rail: provenance

`Builder`, `Reader`, `Signer`, `Settings`, and `Context` are context managers with a `close()`/`is_valid` handle lifecycle; `Builder` closes after its single `sign`, and a `Signer` is consumed when attached to a `Context`. `C2paError` is the base of the typed subclass family, so one `except C2paError` rail traps every fault and per-subclass arms discriminate codec, signature, verify, and not-found failures.

[C2PA_ERROR_SUBCLASSES]: `Assertion` `AssertionNotFound` `Decoding` `Encoding` `FileNotFound` `Io` `Json` `Manifest` `ManifestNotFound` `NotSupported` `Other` `RemoteManifest` `ResourceNotFound` `Signature` `Verify`

`C2paSigningAlg`, `C2paDigitalSourceType`, `C2paBuilderIntent`, and `C2paSeekMode` are `IntEnum` vocabularies driving signing algorithm, source provenance, manifest intent, and stream-seek mode. `C2paDigitalSourceType.EMPTY` is the no-source default `set_intent` supplies, and AI-or-capture provenance origin is one table-driven source-type row rather than a parallel manifest field. `C2paSeekMode` (`START`/`CURRENT`/`END`) drives the `Stream` seek-callback bridge the `Stream` adapter consumes internally.

[C2PA_DIGITAL_SOURCE_TYPE]: `EMPTY` `TRAINED_ALGORITHMIC_DATA` `DIGITAL_CAPTURE` `COMPUTATIONAL_CAPTURE` `NEGATIVE_FILM` `POSITIVE_FILM` `PRINT` `HUMAN_EDITS` `COMPOSITE_WITH_TRAINED_ALGORITHMIC_MEDIA` `ALGORITHMICALLY_ENHANCED` `DIGITAL_CREATION` `DATA_DRIVEN_MEDIA` `TRAINED_ALGORITHMIC_MEDIA` `ALGORITHMIC_MEDIA` `SCREEN_CAPTURE` `VIRTUAL_RECORDING` `COMPOSITE` `COMPOSITE_CAPTURE` `COMPOSITE_SYNTHETIC`

| [INDEX] | [SYMBOL]                | [TYPE_FAMILY]    | [CAPABILITY]                                                     |
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
|  [11]   | `C2paDigitalSourceType` | enum (`IntEnum`) | IPTC digital-source provenance vocabulary for `set_intent`       |
|  [12]   | `C2paBuilderIntent`     | enum (`IntEnum`) | `CREATE`/`EDIT`/`UPDATE` manifest intent                         |
|  [13]   | `C2paSeekMode`          | enum (`IntEnum`) | `START`/`CURRENT`/`END` stream-seek mode for the `Stream` bridge |
|  [14]   | `C2paError`             | error            | base exception with typed subclass attributes                    |

[EXPORT_TIERS]: the owner imports from `c2pa.__all__` — `Builder` `Reader` `Signer` `Stream` `Settings` `Context` `ContextBuilder` `ContextProvider` `C2paError` `C2paSignerInfo` `C2paSigningAlg` `C2paDigitalSourceType` `C2paBuilderIntent` `sdk_version` `load_settings`.

`c2pa.c2pa` exposes `C2paSeekMode`, `format_embeddable`, and `version` beyond the public set — `version` the `importlib.metadata` distribution version, distinct from `sdk_version` the native-core version; the owner reaches `format_embeddable`/`ed25519_sign` through `from c2pa.c2pa import …` only when the embeddable-bytes primitive is needed.

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: `Builder` author and sign
- rail: provenance

`Builder` constructs from a JSON manifest definition (string or dict); `from_json` names the factory and `from_archive` rehydrates from a written archive stream. `sign` discriminates on its first argument — a `Signer` signs explicitly, a format `str` signs with the `Context` signer — is single-use, closes the builder, and buffers into an in-memory `BytesIO` when `dest` is omitted. Members carry `str | dict` for the `*_json` args, `Optional[ContextProvider]` for `context`, and file-like `source`/`stream`/`dest`.

| [INDEX] | [SURFACE]                                                 | [SHAPE]  | [CAPABILITY]                               |
| :-----: | :-------------------------------------------------------- | :------- | :----------------------------------------- |
|  [01]   | `Builder(manifest_json, context=None)`                    | ctor     | construct from a JSON manifest             |
|  [02]   | `from_json(manifest_json, context=None) -> Builder`       | factory  | named factory from a JSON manifest         |
|  [03]   | `from_archive(stream) -> Builder`                         | factory  | rehydrate a builder from an archive        |
|  [04]   | `sign(signer_or_format, ..., dest=None) -> bytes`         | instance | sign source into dest; single-use          |
|  [05]   | `sign_file(source_path, dest_path, signer=None) -> bytes` | instance | sign a file path to an output path         |
|  [06]   | `set_intent(intent, digital_source_type=EMPTY)`           | instance | set manifest intent + digital source       |
|  [07]   | `add_ingredient(ingredient_json, format, source)`         | instance | attach a parent/component ingredient       |
|  [08]   | `add_ingredient_from_stream(json, format, source)`        | instance | attach an ingredient from an open stream   |
|  [09]   | `add_ingredient_from_archive(stream)`                     | instance | rehydrate ingredients from an archive      |
|  [10]   | `write_ingredient_archive(ingredient_id, stream)`         | instance | serialize an ingredient to an archive      |
|  [11]   | `add_resource(uri, stream)`                               | instance | attach a referenced resource by URI        |
|  [12]   | `add_action(action_json)`                                 | instance | append a `c2pa.actions` assertion          |
|  [13]   | `set_remote_url(remote_url)`                              | instance | set the remote manifest URL                |
|  [14]   | `set_no_embed()`                                          | instance | produce a sidecar (non-embedded) manifest  |
|  [15]   | `to_archive(stream)`                                      | instance | serialize builder state to an archive      |
|  [16]   | `with_archive(stream) -> Builder`                         | instance | reload a builder's definition from archive |
|  [17]   | `get_supported_mime_types() -> list[str]`                 | instance | native signable MIME types                 |

[ENTRYPOINT_SCOPE]: `Reader` extract and validate
- rail: provenance

`Reader(format_or_path)` opens a manifest store keyed by argument shape: a sole path resolves the MIME type and opens the file, a `(format, stream)` pair reads an open stream, a `(format, path)` pair reads a named path. `try_create` returns `None` instead of raising `ManifestNotFound` when an asset carries no Content Credentials. Both `Reader` and `try_create` carry `(format_or_path, stream=None, manifest_data=None, context=None)`.

| [INDEX] | [SURFACE]                                                  | [SHAPE]  | [CAPABILITY]                                |
| :-----: | :--------------------------------------------------------- | :------- | :------------------------------------------ |
|  [01]   | `Reader(format_or_path, stream=None, ...)`                 | ctor     | open a manifest store (path/stream)         |
|  [02]   | `try_create(format_or_path, ...) -> Optional[Reader]`      | factory  | open, or `None` when no manifest is present |
|  [03]   | `json() -> str`                                            | instance | manifest store as JSON (cached)             |
|  [04]   | `detailed_json() -> str`                                   | instance | expanded manifest-store JSON                |
|  [05]   | `crjson() -> str`                                          | instance | standardized crJSON; `"{}"` when none       |
|  [06]   | `get_validation_state() -> str \| None`                    | instance | overall `validation_state` field            |
|  [07]   | `get_validation_results() -> dict \| None`                 | instance | detailed `validation_results` object        |
|  [08]   | `get_active_manifest() -> dict \| None`                    | instance | the active manifest dict                    |
|  [09]   | `get_manifest(label) -> dict \| None`                      | instance | a manifest dict by label/ID                 |
|  [10]   | `is_embedded() -> bool`                                    | instance | embedded vs remote manifest flag            |
|  [11]   | `get_remote_url() -> str \| None`                          | instance | remote manifest URL, or `None` if embedded  |
|  [12]   | `resource_to_stream(uri, stream) -> int`                   | instance | write a referenced resource to a stream     |
|  [13]   | `with_fragment(format, stream, fragment_stream) -> Reader` | instance | instance-chain fragmented BMFF              |
|  [14]   | `get_supported_mime_types() -> list[str]`                  | instance | native readable MIME types                  |

[ENTRYPOINT_SCOPE]: signer, settings, and module functions
- rail: provenance

`Signer.from_info` builds a COSE signer from a populated `C2paSignerInfo`; `Signer.from_callback` builds one from an external signing callback with its algorithm and PEM cert chain. `Settings`/`Context` carry per-instance configuration into `Builder`/`Reader`, and `load_settings` sets process-wide thread-local defaults. `format_embeddable` rewraps a detached manifest into embeddable wire form; `ed25519_sign` is the in-process Ed25519 raw-signature primitive a `from_callback` digest-signer composes.

| [INDEX] | [SURFACE]                                                            | [SHAPE]  | [CAPABILITY]                                 |
| :-----: | :------------------------------------------------------------------- | :------- | :------------------------------------------- |
|  [01]   | `Signer.from_info(signer_info) -> Signer`                            | factory  | signer from cert/key/TSA configuration       |
|  [02]   | `Signer.from_callback(callback, alg, certs, tsa_url=None) -> Signer` | factory  | signer from an external callback             |
|  [03]   | `Signer.reserve_size() -> int`                                       | instance | byte size reserved for the signature         |
|  [04]   | `C2paSignerInfo(alg, sign_cert, private_key, ta_url)`                | ctor     | signer config (`alg` enum/str/bytes)         |
|  [05]   | `Settings.from_json(json_str) -> Settings`                           | factory  | settings from a JSON config string           |
|  [06]   | `Settings.from_dict(config) -> Settings`                             | factory  | settings from a config dict                  |
|  [07]   | `Settings.set(path, value) -> Settings`                              | instance | set a dot-notation config value              |
|  [08]   | `Settings.update(data) -> Settings`                                  | instance | merge a config dict/JSON into settings       |
|  [09]   | `Context.from_json(json_str, signer=None) -> Context`                | factory  | context from JSON config + signer            |
|  [10]   | `Context.from_dict(config, signer=None) -> Context`                  | factory  | context from a config dict + signer          |
|  [11]   | `Context.has_signer() -> bool`                                       | instance | whether the context carries a signer         |
|  [12]   | `Context.builder() -> ContextBuilder`                                | instance | fluent `with_settings`/`with_signer`         |
|  [13]   | `load_settings(settings, format="json") -> None`                     | static   | process-wide thread-local settings load      |
|  [14]   | `sdk_version() -> str`                                               | static   | underlying `c2pa-rs` core version            |
|  [15]   | `version() -> str`                                                   | static   | installed `c2pa-python` distribution version |
|  [16]   | `format_embeddable(format, manifest_bytes) -> tuple`                 | static   | rewrap a detached manifest to wire bytes     |
|  [17]   | `ed25519_sign(data, private_key) -> bytes`                           | static   | in-process Ed25519 raw-signature (64-byte)   |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- import: `import c2pa` at boundary scope only; module-level import is banned by the manifest import policy.
- One `Builder` owns authoring and signing: intent, ingredients, resources, actions, and remote URL are call rows on the builder, never a per-shape builder type; `from_archive` classmethod-constructs and `with_archive` reloads under the bound `Context`; the builder is single-use, so one builder owns one signed asset.
- `Builder.sign` is the one signing surface keyed by first-argument shape (a `Signer` signs explicitly, a format `str` signs with the `Context` signer); `sign_file` is the path-to-path row.
- `Reader(format_or_path)` is the one extraction surface keyed by argument shape; `try_create` maps `ManifestNotFound` to `None`; `json`/`detailed_json`/`crjson` are projection rows over the same store, never parallel reader types.
- `validation_state` and `validation_results` are fields read off the parsed store, never a locally recomputed verdict.
- `Signer.from_info`/`from_callback` are the two signer seams; the algorithm is a `C2paSigningAlg` row across ES/PS/ED25519, never a per-algorithm signer type.
- evidence: each operation captures `sdk_version`, signing algorithm, embedded-vs-remote flag, manifest label, `validation_state`, and `validation_results` codes as a provenance receipt.

[STACKING]:
- `expression`(`libs/python/.api/expression.md`): `Reader.try_create`'s `Optional[Reader]` and `Reader.with_fragment`'s raised `ManifestNotFound` fold onto one `Option[Reader]` (`Option.of_optional` for the `None`, an `except C2paError.ManifestNotFound: Nothing` arm for the raise), so a credential-free asset or fragment projects `CredentialEvidence.unsigned` symmetrically; `Reader.get_remote_url()`'s `Optional[str]` projects through `Option.of_optional(...).default_value("")`. `Provenance` is the closed `@tagged_union(frozen=True)` whose `Sign`/`Read`/`ReadFragment`/`Embed`/`ArchiveIngredient` cases close under `assert_never`.
- `msgspec`(`libs/python/.api/msgspec.md`): `Reader.json()` decodes once through a module-level `json.Decoder(type=_Store)` into a typed `_Store`/`_Manifest`/`_ValidationResults` tree with `rename=` mapping the camelCase store keys; the heterogeneous per-assertion `data` holds opaque as `msgspec.Raw`, decoded to `_ActionData` only for the `c2pa.actions` label. This collapses the `get_active_manifest`/`get_validation_state`/`get_validation_results`/`get_manifest` accessor family into one typed read.
- `beartype`(`libs/python/.api/beartype.md`): the `SignerSpec._cose(policy)` projector over the `Signer.from_info`/`from_callback` seam carries `@beartype(conf=FAULT_CONF)`, so a malformed signer field faults at the boundary instead of inside ctypes.
- `stamina`(`libs/python/runtime/.api/stamina.md`): `@stamina.retry(on=(C2paError.RemoteManifest, C2paError.Io), attempts=3)` weaves `_run` so only remote-manifest and I/O transients re-attempt; the typed subclass family makes `C2paError.Decoding`/`Signature`/`NotSupported` surface immediately, where `except C2paError` over-traps.
- `anyio`(`libs/python/.api/anyio.md`): the native `libc2pa_c` core is GIL-releasing and the `ta_url`/remote transport is blocking I/O, so `Builder.sign`/`Reader.json`/`with_fragment` cross the `LanePolicy.offload(Kernel.of(..., KernelTrait.RELEASING))` seam that owns the band, boundary, and rail; the in-process ctypes binding needs no `HOSTILE` process crossing.
- `xxhash`(`libs/python/.api/xxhash.md`) via runtime `content_identity`: `ContentIdentity.key("credential.<tag>", asset_bytes)` mints the `ContentKey` after the thread returns, its canonical preimage the complete signed-asset octets `Builder.sign` returns and the identical octets handed to `Reader`, so every producer (the `csharp:Rasm.Persistence` `XxHash128` re-derivation included) hashes one byte sequence and the keys collide by construction.
- `opentelemetry`(`libs/python/.api/opentelemetry-api.md`) + `structlog`(`libs/python/.api/structlog.md`): `validation_state` projects onto `ArtifactReceipt.Credential`; validation code sets and `sdk_version()` ride the `CredentialEvidence` returned by `close`.
- within-lib `Signer.from_callback` seam: the external callback returns raw COSE signature bytes for a digest, so private-key material stays in the `cryptography` keyring or an HSM and `c2pa` never holds it; `C2paSigningAlg` and the PEM `certs` chain are the only crypto config crossing the seam.
- within-lib AI-provenance seam: a generated asset sets `Builder.set_intent(C2paBuilderIntent.CREATE, C2paDigitalSourceType.TRAINED_ALGORITHMIC_MEDIA)`, so the digital-source type is the table-driven AI-origin signal the verifier reads rather than a free-text manifest field.

[LOCAL_ADMISSION]:
- Sole C2PA Content-Credentials owner for `artifacts`. `Reader`'s readable MIME set exceeds the `Builder` signable set by PDF and the raw-camera `arw`/`nef` formats, read-only here, so a PDF or raw-camera asset routes to the `pyhanko` PAdES rail (`exchange/conformance`) and never `Builder.sign`. C2PA and PAdES are orthogonal rails the provenance owner selects per asset class by the signable-MIME set; a PDF carries a C2PA manifest this SDK reads but never writes.

[RAIL_LAW]:
- Package: `c2pa-python`
- Owns: C2PA manifest authoring from a JSON definition, embedded/sidecar signing, manifest-store extraction and parsing, `validation_state`/`validation_results` reporting, and COSE signer construction across ES/PS/ED25519.
- Accept: signing and validating Content Credentials on assets feeding the provenance, imaging, and document owners.
- Reject: a wrapper-rename of `Builder.sign`/`Reader`/`get_validation_state`; a hand-rolled JUMBF/COSE manifest codec; a local recomputation of `validation_state`; a parallel reader or builder type per asset format or signing algorithm; a PDF or raw-camera asset on `Builder.sign`; certificate or key material the campaign signer config owns.

# [PY_ARTIFACTS_API_C2PA_PYTHON]

`c2pa-python` supplies the C2PA content-credentials surface for the artifacts provenance rail: a `Builder` that loads a JSON manifest definition and embeds a signed manifest into an asset via `sign`/`sign_file`, a `Reader` that extracts and validates the manifest store from a path or stream and exposes `json`/`get_validation_state`/`get_validation_results`, a `Signer` factory family backed by `C2paSignerInfo` and `C2paSigningAlg`, and a `Settings`/`Context` configuration pair. The package owner composes `Builder.from_json`, `Builder.sign`, `Reader`, and `Reader.get_validation_state` into the PROVENANCE sub-domain path; it removes any hand-rolled JUMBF/COSE manifest codec because the native `libc2pa_c` core owns claim signing and validation, and it never re-implements the C2PA validation-state computation the SDK already returns in the manifest store JSON.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `c2pa-python`
- package: `c2pa-python`
- import: `c2pa`
- owner: `artifacts`
- rail: provenance
- installed: `0.35.1` reflected via `import c2pa; c2pa.__version__` on cp313 (underlying `c2pa-rs` core `0.88.0` via `sdk_version()`)
- license: `MIT OR Apache-2.0`
- wheel: `c2pa_python-0.35.1-py3-none-<platform>.whl` — `py3` ABI tag but NOT pure-Python; each wheel bundles the native `libc2pa_c` core per platform (`macosx_10_9_universal2`/`macosx_10_9_x86_64`/`macosx_11_0_arm64`/`manylinux_2_28_aarch64`/`manylinux_2_28_x86_64`/`win_amd64`/`win_arm64`). No `musllinux` wheel; no abi3 — the bundled core is loaded via `ctypes`, so the platform tag is load-bearing
- marker: `python_requires >=3.10`; runtime deps `cryptography`/`requests` (callback signing and remote manifest fetch)
- entry points: console script `download-artifacts` (native-library fetch); library use is import-only
- capability: C2PA manifest authoring from a JSON definition, embedded/sidecar manifest signing into JPEG/PNG/PDF/BMFF/fragmented-MP4 and other supported MIME types, ingredient attachment from stream/archive, builder archive serialize/rehydrate, manifest-store extraction and parsing (`json`/`detailed_json`/`crjson`), `validation_state` and `validation_results` reporting, callback- and `C2paSignerInfo`-backed signers across ES256/ES384/ES512/PS256/PS384/PS512/ED25519, and per-instance `Settings`/`Context` configuration

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: builder, reader, signer, and configuration roots
- rail: provenance

`Builder`, `Reader`, `Signer`, `Settings`, and `Context` are all context managers with `close()`/`is_valid` lifecycle (the native handle is released on `close`); `Builder` is single-sign and closes after `sign`; `Reader` is closed after the `with` block; `Signer` is consumed when attached to a `Context`. `C2paError` is the base of the full typed subclass family — `Assertion`, `AssertionNotFound`, `Decoding`, `Encoding`, `FileNotFound`, `Io`, `Json`, `Manifest`, `ManifestNotFound`, `NotSupported`, `Other`, `RemoteManifest`, `ResourceNotFound`, `Signature`, `Verify` — so a single `except C2paError` rail traps the family and per-subclass arms discriminate codec/signature/verify/not-found faults. `C2paSigningAlg`, `C2paDigitalSourceType`, and `C2paBuilderIntent` are `IntEnum` vocabularies driving signing, source provenance, and manifest intent; `C2paDigitalSourceType` carries the full IPTC digital-source vocabulary (`DIGITAL_CAPTURE`, `TRAINED_ALGORITHMIC_MEDIA`, `COMPOSITE_SYNTHETIC`, `COMPOSITE_WITH_TRAINED_ALGORITHMIC_MEDIA`, `ALGORITHMICALLY_ENHANCED`, …) so AI-provenance intent is a source-type row, not a parallel manifest field.

| [INDEX] | [SYMBOL]                | [TYPE_FAMILY]    | [RAIL]                                                        |
| :-----: | :---------------------- | :--------------- | :------------------------------------------------------------ |
|  [01]   | `Builder`               | resource         | manifest authoring + single-use signing into an asset         |
|  [02]   | `Reader`                | resource         | manifest-store extraction, parsing, and validation reporting  |
|  [03]   | `Signer`                | resource         | COSE signer from `C2paSignerInfo` or callback                 |
|  [04]   | `Stream`                | resource         | Python stream-to-native `C2paStream` bridge                   |
|  [05]   | `Context`               | resource         | per-instance settings + signer carrier for `Builder`/`Reader` |
|  [06]   | `ContextBuilder`        | builder          | fluent `with_settings`/`with_signer`/`build` for `Context`    |
|  [07]   | `Settings`              | resource         | dot-path / JSON / dict configuration object                   |
|  [08]   | `ContextProvider`       | protocol         | interface a `Context` implements for `Builder`/`Reader`       |
|  [09]   | `C2paSignerInfo`        | value (ctypes)   | `alg`/`sign_cert`/`private_key`/`ta_url` signer configuration |
|  [10]   | `C2paSigningAlg`        | enum (`IntEnum`) | ES256/ES384/ES512/PS256/PS384/PS512/ED25519 signing algorithm |
|  [11]   | `C2paDigitalSourceType` | enum (`IntEnum`) | digital-source provenance for `CREATE` intent                 |
|  [12]   | `C2paBuilderIntent`     | enum (`IntEnum`) | `CREATE`/`EDIT`/`UPDATE` manifest intent                      |
|  [13]   | `C2paError`             | error            | base exception with typed subclass attributes                 |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: `Builder` author and sign
- rail: provenance

`Builder` is constructed from a JSON manifest definition (string or dict); `from_json` is the named factory and `from_archive` rehydrates a builder from a written archive stream. `sign` discriminates on its first argument: a `Signer` signs explicitly, a `str` format signs with the `Context`'s signer. `sign` is single-use and closes the builder; omitting `dest` buffers the signed asset into an in-memory `BytesIO`.

| [INDEX] | [SURFACE]                          | [CALL_SHAPE]                                                                                                                      | [CAPABILITY]                                              |
| :-----: | :--------------------------------- | :-------------------------------------------------------------------------------------------------------------------------------- | :-------------------------------------------------------- |
|  [01]   | `Builder`                          | `Builder(manifest_json: Any, context: Optional[ContextProvider] = None)`                                                          | construct from a JSON manifest definition                 |
|  [02]   | `Builder.from_json`                | `from_json(manifest_json: Any, context: Optional[ContextProvider] = None) -> Builder`                                             | named factory from a JSON manifest                        |
|  [03]   | `Builder.from_archive`             | `from_archive(stream: Any) -> Builder`                                                                                            | rehydrate a builder from an archive stream                |
|  [04]   | `Builder.sign`                     | `sign(signer_or_format: Union[Signer, str], format_or_source: Any = None, source_or_dest: Any = None, dest: Any = None) -> bytes` | sign source into dest, return manifest bytes (single use) |
|  [05]   | `Builder.sign_file`                | `sign_file(source_path: Union[str, Path], dest_path: Union[str, Path], signer: Optional[Signer] = None) -> bytes`                 | sign a file path to an output path, return manifest bytes |
|  [06]   | `Builder.set_intent`               | `set_intent(intent: C2paBuilderIntent, digital_source_type: C2paDigitalSourceType = C2paDigitalSourceType.EMPTY)`                 | set manifest intent and digital source type               |
|  [07]   | `Builder.add_ingredient`           | `add_ingredient(ingredient_json: Union[str, dict], format: str, source: Any)`                                                     | attach a parent/component ingredient from a stream/source |
|  [07a]  | `Builder.add_ingredient_from_stream` | `add_ingredient_from_stream(ingredient_json, format, stream)`                                                                   | ingredient-attach row keyed to an open stream             |
|  [07b]  | `Builder.add_ingredient_from_archive` | `add_ingredient_from_archive(ingredient_json, format, stream)`                                                                 | ingredient-attach row from a written ingredient archive   |
|  [07c]  | `Builder.write_ingredient_archive` | `write_ingredient_archive(ingredient_id: str, stream: Any) -> None`                                                               | serialize an attached ingredient to a reusable archive    |
|  [08]   | `Builder.add_resource`             | `add_resource(uri: str, stream: Any)`                                                                                             | attach a referenced resource by URI                       |
|  [09]   | `Builder.add_action`               | `add_action(action_json: Union[str, dict]) -> None`                                                                               | append a c2pa.actions assertion                           |
|  [10]   | `Builder.set_remote_url`           | `set_remote_url(remote_url: str)`                                                                                                 | set the remote manifest URL                               |
|  [11]   | `Builder.set_no_embed`             | `set_no_embed()`                                                                                                                  | produce a sidecar (non-embedded) manifest                 |
|  [12]   | `Builder.to_archive`               | `to_archive(stream: Any) -> None`                                                                                                 | serialize builder state to an archive stream              |
|  [13]   | `Builder.get_supported_mime_types` | `get_supported_mime_types() -> list[str]`                                                                                         | native list of signable MIME types                        |

[ENTRYPOINT_SCOPE]: `Reader` extract and validate
- rail: provenance

`Reader(format_or_path)` reads a manifest store: a sole path argument resolves the MIME type and opens the file, a `(format, stream)` pair reads from an open stream, and a `(format, path)` pair reads a named path. `try_create` is the optional-returning factory that returns `None` instead of raising `C2paError.ManifestNotFound` when an asset carries no Content Credentials. `get_validation_state` reads the manifest-store `validation_state` field; `get_validation_results` reads the `validation_results` object.

| [INDEX] | [SURFACE]                         | [CALL_SHAPE]                                                                                                                                                                     | [CAPABILITY]                                      |
| :-----: | :-------------------------------- | :------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | :------------------------------------------------ |
|  [01]   | `Reader`                          | `Reader(format_or_path: Union[str, Path], stream: Optional[Any] = None, manifest_data: Optional[Any] = None, context: Optional[ContextProvider] = None)`                         | open a manifest store from a path or stream       |
|  [02]   | `Reader.try_create`               | `try_create(format_or_path: Union[str, Path], stream: Optional[Any] = None, manifest_data: Optional[Any] = None, context: Optional[ContextProvider] = None) -> Optional[Reader]` | open or return `None` when no manifest is present |
|  [03]   | `Reader.json`                     | `json() -> str`                                                                                                                                                                  | manifest store as a JSON string (cached)          |
|  [04]   | `Reader.detailed_json`            | `detailed_json() -> str`                                                                                                                                                         | expanded manifest-store JSON                      |
|  [05]   | `Reader.crjson`                   | `crjson() -> str`                                                                                                                                                                | standardized crJSON; `"{}"` when no credentials   |
|  [06]   | `Reader.get_validation_state`     | `get_validation_state() -> Optional[str]`                                                                                                                                        | overall `validation_state` field of the manifest  |
|  [07]   | `Reader.get_validation_results`   | `get_validation_results() -> Optional[dict]`                                                                                                                                     | detailed `validation_results` object              |
|  [08]   | `Reader.get_active_manifest`      | `get_active_manifest() -> Optional[dict]`                                                                                                                                        | the active manifest dict from the store           |
|  [09]   | `Reader.get_manifest`             | `get_manifest(label: str) -> Optional[dict]`                                                                                                                                     | a manifest dict by label/ID                       |
|  [10]   | `Reader.is_embedded`              | `is_embedded() -> bool`                                                                                                                                                          | embedded vs remote manifest flag                  |
|  [11]   | `Reader.get_remote_url`           | `get_remote_url() -> Optional[str]`                                                                                                                                              | remote manifest URL, or `None` when embedded      |
|  [12]   | `Reader.resource_to_stream`       | `resource_to_stream(uri: str, stream: Any) -> int`                                                                                                                               | write a referenced resource to a stream           |
|  [13]   | `Reader.with_fragment`            | `with_fragment(format: str, stream: Any, fragment_stream: Any) -> Reader`                                                                                                        | read a fragmented-BMFF (DASH/CMAF) init+fragment  |
|  [14]   | `Reader.get_supported_mime_types` | `get_supported_mime_types() -> list[str]`                                                                                                                                        | native list of readable MIME types                |

[ENTRYPOINT_SCOPE]: signer, settings, and module functions
- rail: provenance

`Signer.from_info` builds a COSE signer from a populated `C2paSignerInfo`; `Signer.from_callback` builds one from an external signing callback plus algorithm and PEM cert chain. `Settings`/`Context` carry per-instance configuration into `Builder`/`Reader`; module-level `load_settings` is the deprecated thread-local path. `sdk_version` returns the underlying `c2pa-rs` semantic version.

| [INDEX] | [SURFACE]              | [CALL_SHAPE]                                                                                                                  | [CAPABILITY]                                              |
| :-----: | :--------------------- | :---------------------------------------------------------------------------------------------------------------------------- | :-------------------------------------------------------- |
|  [01]   | `Signer.from_info`     | `from_info(signer_info: C2paSignerInfo) -> Signer`                                                                            | signer from cert/key/TSA configuration                    |
|  [02]   | `Signer.from_callback` | `from_callback(callback: Callable[[bytes], bytes], alg: C2paSigningAlg, certs: str, tsa_url: Optional[str] = None) -> Signer` | signer from an external signing callback                  |
|  [03]   | `Signer.reserve_size`  | `reserve_size() -> int`                                                                                                       | byte size reserved for the signature                      |
|  [04]   | `C2paSignerInfo`       | `C2paSignerInfo(alg, sign_cert, private_key, ta_url)`                                                                         | signer configuration value (`alg` accepts enum/str/bytes) |
|  [05]   | `Settings.from_json`   | `from_json(json_str: str) -> Settings`                                                                                        | settings from a JSON config string                        |
|  [06]   | `Settings.from_dict`   | `from_dict(config: dict) -> Settings`                                                                                         | settings from a config dict                               |
|  [07]   | `Settings.set`         | `set(path: str, value: str) -> Settings`                                                                                      | set a dot-notation config value                           |
|  [07a]  | `Settings.update`      | `update(data: Union[str, dict]) -> Settings`                                                                                  | merge a config dict/JSON string into the settings object  |
|  [08]   | `Context.from_json`    | `from_json(json_str: str, signer: Optional[Signer] = None) -> Context`                                                        | context from JSON config plus optional signer             |
|  [08a]  | `Context.from_dict`    | `from_dict(config: dict, signer: Optional[Signer] = None) -> Context`                                                         | context from a config dict plus optional signer           |
|  [08b]  | `Context.has_signer`   | `has_signer() -> bool`                                                                                                        | whether the context carries a bound signer                |
|  [09]   | `Context.builder`      | `builder() -> ContextBuilder`                                                                                                 | fluent `with_settings`/`with_signer`/`build`              |
|  [10]   | `load_settings`        | `load_settings(settings: Union[str, dict], format: str = "json") -> None`                                                     | deprecated thread-local settings load                     |
|  [11]   | `sdk_version`          | `sdk_version() -> str`                                                                                                        | underlying `c2pa-rs` core semantic version (`0.88.0`)     |

## [04]-[IMPLEMENTATION_LAW]

[PROVENANCE_CONTENT_CREDENTIALS]:
- import: `import c2pa` at boundary scope only; module-level import is banned by the manifest import policy.
- author axis: one `Builder.from_json` owns manifest authoring; intent, ingredients, resources, actions, and remote URL are call rows on the builder, never a per-shape builder type; `from_archive` is the rehydration row.
- sign axis: `Builder.sign` is the single signing surface keyed by first-argument shape — a `Signer` signs explicitly, a format `str` signs with the `Context` signer; `sign_file` is the path-to-path row; the builder is single-use and closes after signing, so one builder owns one signed asset.
- read axis: `Reader(format_or_path)` is the single extraction surface keyed by argument shape (path, `(format, stream)`, `(format, path)`); `try_create` is the optional-returning row that maps `ManifestNotFound` to `None`; `json`/`detailed_json`/`crjson` are projection rows over the same store, never parallel reader types.
- validation axis: `get_validation_state` reads the `validation_state` string and `get_validation_results` reads the `validation_results` object from the parsed manifest store; validation status is a field read off the store, never a recomputed local verdict.
- signer axis: `Signer.from_info` consumes a `C2paSignerInfo` (`alg`/`sign_cert`/`private_key`/`ta_url`) and `Signer.from_callback` consumes an external callback plus `C2paSigningAlg`; the algorithm is an enum row across ES/PS/ED25519, never a per-algorithm signer type.
- evidence: each operation captures SDK version (`sdk_version`), signing algorithm, embedded-vs-remote flag, manifest label, `validation_state`, and `validation_results` codes as a provenance receipt.
- boundary: `c2pa-python` owns C2PA manifest authoring, signing, extraction, and validation through the native `libc2pa_c` core with Python stream bridging via `Stream`; asset bytes flow in and out as streams or paths supplied by the imaging/document owners; certificate and key material is supplied by the campaign signer configuration, never minted here; live UI stays outside this package.

[INTEGRATION_STACK]:
- `c2pa-python` vs `pyhanko` (PAdES): C2PA Content Credentials and PAdES PDF signatures are orthogonal provenance rails — C2PA owns cross-format (JPEG/PNG/PDF/BMFF) embedded-manifest provenance and `pyhanko` owns PDF-native PAdES/CMS signatures; they sign the same PDF on different axes and neither re-implements the other's signature container. The campaign provenance owner selects the rail per asset class, never wraps one in the other.
- `Signer.from_callback` is the seam to the `cryptography`/HSM signer: the external callback returns the raw COSE signature bytes for a digest, so private-key material lives in the `cryptography` keyring or an HSM and `c2pa` never holds it; `C2paSigningAlg` and the PEM `certs` chain are the only crypto config crossing the seam.
- asset stream seam: the in-memory asset buffer is the wire between the imaging/PDF owner and `c2pa` — `Builder.sign(signer, format, source_stream, dest_stream)` reads and writes `BytesIO`, so the imaging codec hands a decoded buffer and receives a signed buffer with no intermediate file; `sign_file` is the path convenience only when a file handle already exists.
- AI-provenance seam: a generated asset sets `Builder.set_intent(C2paBuilderIntent.CREATE, C2paDigitalSourceType.TRAINED_ALGORITHMIC_MEDIA)` so the digital-source type is the table-driven AI-origin signal the verifier reads, never a free-text manifest field.

[RAIL_LAW]:
- Package: `c2pa-python`
- Owns: C2PA manifest authoring from a JSON definition, embedded/sidecar manifest signing, manifest-store extraction and parsing, `validation_state`/`validation_results` reporting, and COSE signer construction across ES/PS/ED25519
- Accept: signing and validating Content Credentials on assets feeding the provenance, imaging, and document owners
- Reject: wrapper-renames of `Builder.sign`/`Reader`/`get_validation_state`; a hand-rolled JUMBF/COSE manifest codec; a local re-computation of `validation_state` instead of reading the store field; a parallel reader/builder type per asset format or signing algorithm; certificate or key minting the campaign signer config owns

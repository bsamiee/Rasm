# [PY_ARTIFACTS_CREDENTIAL]

The C2PA content-credential owner at the exchange boundary. `Provenance` is ONE owner over `c2pa-python` binding a signed tamper-evident manifest into an emitted artifact and reading it back — the closed `tagged_union` owning the dispatch, the async entry, AND every case body directly, discriminating its `Sign`/`Read`/`ReadFragment`/`Embed` cases by payload shape (the `Sign` arm folding the optional sidecar `remote_url` rather than a parallel case, the `Embed` arm rewrapping a captured detached/remote-store manifest into the format's embeddable JUMBF block through `format_embeddable`) — never a one-field wrapper over a separate op union, never a `from_file`/`from_stream`/`per-format` reader family, never a per-signing-algorithm signer type, and never a per-output `sign`/`verify`/`read` method triple. `SignerSpec` is the ONE policy union carrying one typed signer case — a `CertKeySigner` (`alg`, `sign_cert`, `private_key`, `ta_url`) feeding `Signer.from_info` and a `CallbackSigner` (`alg`, `certs`, `sign`, `ta_url`) whose campaign-supplied `sign` digest-callback feeds `Signer.from_callback` — so the `cryptography`/HSM seam is a tagged case carrying the keyring/HSM-bound signer as a typed field, the signing algorithm a `SigningAlg` `StrEnum` whose `C2paSigningAlg` row derives by name correspondence across ES/PS/ED25519, never a parallel signer owner or a free-text alg field; the `CallbackSigner.ed25519` factory binds the in-process `ed25519_sign` primitive as the `sign` callback so the common ED25519 case signs with no HSM/keyring seam. `CredentialEvidence` is the ONE rich provenance receipt a single `Reader.json()` manifest-STORE decode folds — the active-manifest label read off the store's `active_manifest` KEY (never a phantom `label` field inside the manifest dict the SDK never emits), the `manifests` chain count, signer alg, signature `issuer`/`time`/`cert_serial_number`/`revocation_status`, `claim_generator`/`title`/`format`/`instance_id`, embedded flag, remote URL, SDK version, assertion labels, ingredient count, and the `validation_state` plus the `validation_results` failure/success/informational codes, the per-ingredient `ingredientDeltas` failure codes, the per-ingredient lineage (each ingredient's `relationship`, `title`, `format`, and `instance_id`), AND the bound thumbnail `resources` (the claim + per-ingredient thumbnails extracted through `Reader.resource_to_stream`, the forensic evidence a DAM consumer walks) — returned whole as the `(ContentKey, CredentialEvidence)` pair (mirroring the `exchange/conformance#CONFORMANCE` verdict close) the consumer projects onto `core/receipt#RECEIPT` `ArtifactReceipt.Credential` as its four settled scalars, keying the signed buffer through `ContentIdentity.of` so the binding co-identifies the durable artifact with the `csharp:Rasm.Persistence` `XxHash128` content key. The native core signs and reads on an the runtime thread lane worker and drives the RFC-3161 `ta_url` timestamp and remote-manifest `requests` transport there, so the runtime retry class over the transient `C2paError.RemoteManifest`/`Io` subset re-attempts only the network seam — the same crypto-with-network concern the `exchange/conformance#CONFORMANCE` PAdES sibling closes for PDF (PDF/raw-camera are read-only here, gated out by `Builder.get_supported_mime_types`, signed on the `pyhanko` rail); this owner is cross-format content-authenticity provenance over the image/BMFF/audio signable set, owning no artifact production, only the credential close over already-emitted bytes.

## [01]-[INDEX]

- [01]-[CREDENTIAL]: C2PA manifest authoring (the full IPTC `DigitalSource` intent vocabulary, the `Ingredient` stream-or-archive attach modality) + single-use embed/sidecar signing format-gated on `Builder.get_supported_mime_types`, the `format_embeddable` rewrap of a detached/remote-store manifest into the format's embeddable JUMBF block (the `Embed` case), manifest-STORE extraction folding one `Reader.json()` decode through `msgspec.json.Decoder` into the typed `_Store` (the active-manifest label off the `active_manifest` key, the `manifests` chain, `validation_state`/`validation_results`, the claim + ingredient `thumbnail` `ResourceRef`s), the `Reader.resource_to_stream` extraction of the bound thumbnail bytes into the evidence `resources` band, fragmented-BMFF reading over `Reader.with_fragment`, the typed `SignSpec`/`ReadSpec`/`FragmentSpec`/`EmbedSpec` per-case request payloads and the `SignerSpec` cert/callback policy union (with the `CallbackSigner.ed25519` in-process no-HSM digest-signer over `ed25519_sign`), the rich `CredentialEvidence` provenance receipt (signer chain, validation codes, structured `claim_generator_info`, the declared `c2pa.actions` edit/AI-source history, and the extracted thumbnail `resources`) returned as the `(ContentKey, CredentialEvidence)` pair, the `CredentialSettings` `pydantic-settings` env-admission owner (`RASM_CREDENTIAL_` prefix) projecting ONE trust-list-anchored verify `Context` (`Settings.from_dict` trust+verify config → `Context.builder().with_settings().build()`) every Sign/Read/Embed case threads so `validation_state`/`validation_results` anchor against a real trust set, the transient-only the runtime retry class network weave, and the content-key binding the consumer projects onto the four-scalar `core/receipt#RECEIPT` `ArtifactReceipt.Credential` case and the `csharp:Rasm.Persistence` content key.

## [02]-[CREDENTIAL]

- Cases: `Provenance` rows `Sign` (`Manifest.author` → `Builder.sign` into an in-memory `BytesIO`, single-use, the builder closes after `sign`; a present `remote_url` folds `Builder.set_no_embed`/`set_remote_url` so the signed manifest references a remote store rather than embedding — the embed-vs-sidecar modality the `remote_url` value discriminates, never a parallel case nor an `embed: bool` knob) · `Read` (`Reader.try_create` → `Reader.json` STORE decode, the optional-returning extraction mapping `ManifestNotFound`→`None` to the `unsigned` evidence) · `ReadFragment` (`Reader.with_fragment` init+fragment, the fragmented-BMFF read) · `Embed` (`format_embeddable(fmt, manifest)` rewrapping a captured detached/remote-store manifest into the format's embeddable JUMBF block, `_SIGNABLE`-gated and evidence-read through the sidecar `manifest_data` path — the block a downstream writer splices into the asset, this owner producing the credential block, never re-authoring the asset) — each binding the one `Builder`/`Reader` surface keyed by argument shape rather than a parallel type; `SignerSpec` cases `cert_key` (`CertKeySigner`) · `callback` (`CallbackSigner`, with the `CallbackSigner.ed25519(certs, private_key)` factory binding the in-process `ed25519_sign` primitive as the `sign` callback for the ED25519 no-HSM path) — one frozen signer-struct per seam whose fields are the named crypto axes the `match` arm reads directly, the `alg` axis a `SigningAlg` `StrEnum` member, so a per-algorithm signer type and a positional cert tuple are the deleted forms and no private-key material is minted here; `Ingredient` cases `stream` (`ManifestDefinition`-style definition + format + source bytes → `add_ingredient_from_stream`) · `archive` (a `write_ingredient_archive` blob → `add_ingredient_from_archive`) — the attach modality is the case the `author` fold matches, never a `from_archive: bool` knob, so a parent processed once is reused across a campaign's derived assets. The intent axis is one coupled `Manifest.intent` field — an `(Intent, DigitalSource | None)` pair, never two parallel nullable fields where a `DigitalSource` could orphan without an `Intent` to carry it — over `Intent` (`CREATE`/`EDIT`/`UPDATE`) and the full eighteen-member IPTC `DigitalSource` vocabulary riding `Builder.set_intent`, so AI-and-capture provenance origin is the table-driven source-type token the verifier reads, never a free-text manifest field or a raw `C2paDigitalSourceType[name]` member-access.
- Entry: `Provenance` IS the closed `tagged_union` and owns the async entry directly — `emit()` returns the `ArtifactWork` node (PRE-RUN key) and `_emit` resolves `RuntimeRail[ArtifactReceipt.Credential]` ; the `Sign` arm signs and keys the signed buffer, the `Read`/`ReadFragment` arms key the read asset, the `Embed` arm keys the produced embeddable manifest block, never a per-operation entrypoint or a `sign`/`read`/`verify` method triple. The `_emit` core carries one definition-time aspect over a thin body — `@the runtime `RetryClass.OCCT` retry re-attempts ONLY the `C2paError.RemoteManifest`/`Io` network transients (a TSA timestamp or remote-manifest fetch), so a non-transient codec/signature fault surfaces immediately — and offloads `_run` through the runtime thread lane under the module-level `_THREAD_GATE` `CapacityLimiter` (the explicit thread bound, never the per-loop 40-token default) so the `async_boundary` rail never blocks the event loop while the native core signs and drives the `ta_url`/remote `requests` transport; `async_boundary` converts the residual `C2paError`/contract raise into the runtime `BoundaryFault` rail and the spine's own `rejected` line. `_emit` returns the `(ContentKey, CredentialEvidence)` pair — `ContentIdentity.of` over the signed/read bytes plus the rich evidence the read produces — never a four-scalar `ArtifactReceipt` that discards the forensic fields `measure` computes; the consumer mints `ArtifactReceipt.Credential(key, ev.manifest_id, ev.signer, ev.assertions, ev.validation_state)` from the pair exactly as the planner mints `ArtifactReceipt.Verdict` from the conformance pair, so the verification caller sees the full provenance while the receipt stays the thin four-scalar pipeline summary. The asset bytes arrive as an in-memory `BytesIO` from the imaging/document owner and leave as a signed `BytesIO`, so the codec hands a decoded buffer and receives a signed buffer with no intermediate file — `sign_file` is the path convenience only when a file handle already exists, never the default. The in-process `c2pa` package needs no `anyio.to_process` worker lane (distinct from the host-native `exchange/detect#DETECT`/`exchange/metadata#METADATA` siblings that cross the subprocess seam).
- Auto: the build is one ordered fold over `Manifest` then signer — `Manifest.author` seeds `Builder.from_json`, folds `set_intent` from the intent row (the `DigitalSource` projected through `_DIGITAL_SOURCE`, defaulting `C2paDigitalSourceType.EMPTY` when no source is declared), folds `set_no_embed`/`set_remote_url` when a sidecar URL is supplied, folds the `add_action` assertion sequence, folds each `Ingredient` through the `add_ingredient_from_stream`/`add_ingredient_from_archive` arm its case selects, and folds the `Resource` attachments through `add_resource`, while the `SignerSpec.cose` projection (woven with the shared `@beartype(conf=FAULT_CONF)` contract) builds the `Signer.from_info`/`from_callback` row from the matched case (the Signer-free `alg` accessor stays distinct so reading the algorithm mints no native signer handle); the `Sign` arm preflights `fmt` against the module-level `_SIGNABLE` set (`Builder.get_supported_mime_types`) and `raise C2paError.NotSupported` for a read-only asset before authoring, then drives `Builder.sign(signer, format, source_stream, dest_stream)` into a fresh `BytesIO` while capturing the detached manifest bytes the `sign` call returns, and reads the credential back through `_evidence` — the embedded path (no `remote_url`) validates the manifest bound into the signed asset, while the sidecar path (a `remote_url` whose `set_no_embed` asset carries no embedded manifest, the remote store unpublished at sign time) validates the detached bytes through `Reader.try_create(..., manifest_data=...)` so the read never degrades to bogus `unsigned` evidence; the builder closes single-use. The `Embed` arm drives `_embedded` — `_SIGNABLE`-preflighting `fmt`, rewrapping the captured detached/remote-store manifest into the format's embeddable JUMBF block through `format_embeddable(fmt, manifest)`, and reading the credential back over the same `Reader.try_create(fmt, asset, manifest_data=manifest)` sidecar path so the produced block carries a real evidence verdict, the block returned as the keyed artifact rather than a bare byte transform. The `Read`/`ReadFragment` arms open one `Reader.try_create`/`Reader.with_fragment` context whose `Reader.json()` STORE JSON is admitted once through the module-level `_STORE_DECODER` into the typed `_Store` — the active-manifest LABEL is the store's `active_manifest` string KEY and the manifest content is `manifests[active_manifest]` (the manifest dict carries no `label` of its own), so a `.get("label")` over the manifest dict is the phantom always-`""` read the decode replaces, and the same one decode carries `validation_state`, the `validation_results` success/informational/failure code sets, the `ingredientDeltas` per-ingredient failure codes, and the whole `manifests` chain, never per-accessor `get_active_manifest`/`get_validation_state`/`get_validation_results` calls plus a per-label `get_manifest` loop. `CredentialEvidence.measure` folds the active manifest's `label`/`instance_id`/`title`/`format`/`claim_generator`, the `signature_info` `alg`/`issuer`/`time`/`cert_serial_number`/`revocation_status` (with a derived `timestamped` flag off the trusted-time presence), the `Reader.is_embedded`/`get_remote_url` flags, the `sdk_version`, the assertion labels, the per-ingredient lineage (`relationship`/`title`/`format`/`instance_id`) and ingredient count, the `len(manifests)` chain depth, and every validation code set into one row in a single store walk, never a per-arm re-projection or a two-pass count; the `_drawn` fold — on the SAME live-reader borrow — writes the claim thumbnail and every ingredient thumbnail `identifier` through `Reader.resource_to_stream` into the evidence `resources` `frozendict[str, bytes]` band, each extraction wrapped in `expression.extra.result.catch(exception=C2paError)` narrowed to `Nothing` so a missing/broken resource skips by omission rather than faulting the read; the shared `_opened` projector folds the two read-modality absences onto one `Option[Reader]` — `Reader.try_create`'s `None` for a credential-free asset and `Reader.with_fragment`'s raised `ManifestNotFound` for a credential-free fragment — whose `is_none` arm projects `CredentialEvidence.unsigned`, so either read modality admits the foreign absence to the carrier at the seam rather than one path threading `None` inward or degrading to a boundary fault. The verify `Context` threads per case through `Builder.from_json(context=...)` and `Reader.try_create(context=...)` — the `ReadFragment` arm's `Reader.with_fragment` takes none, so its case carries no `Context` slot rather than silently dropping one — so trust-list and verify policy is the case's own settled `Context` value the `CredentialSettings.context()` composition-root owner AUTHORS (the trust anchors read to PEM content + the verify options admitted ONCE through the `RASM_CREDENTIAL_` env, projected via `Settings.from_dict` + `Context.builder().with_settings().build()`), never an opaque unanchored campaign default that leaves `validation_state`/`validation_results` meaningless, and never the deprecated thread-local `load_settings` path.
- Receipt: `_emit` projects the rich `CredentialEvidence` onto the `ArtifactReceipt.Credential` case (`receipt.slot == node.key`); callers read facts off the receipt, never a key+evidence pair. The consumer mints `core/receipt#RECEIPT` `ArtifactReceipt.Credential(key, ev.manifest_id, ev.signer, ev.assertions, ev.validation_state)` from the returned pair — the `resources` bytes and every rich field staying on the evidence value object, never reaching the wire — the four settled scalars (c2pa manifest id the store's `active_manifest` label, signer identity the matched `alg` token, assertion count, `validation_state` string) projected at the coordinator exactly as `ArtifactReceipt.Verdict(key, verdict)` is minted from the conformance pair, so the `credential` case stays flat scalars and `receipt.py` imports no `CredentialEvidence` value object. The `validation_state` plus the valid/invalid evidence the pair carries is the observable provenance the runtime `observability/metrics` `MeterProvider` signal stream reads off the minted receipt fold.
- Packages: `c2pa-python` (installed `0.36.0`, native `c2pa-rs` core `sdk_version() -> 0.89.0`): `Builder.from_json`/`set_intent`/`set_no_embed`/`set_remote_url`/`add_action`/`add_ingredient_from_stream`/`add_ingredient_from_archive`/`add_resource`/`sign`/`get_supported_mime_types` the authoring+signing surface; `Reader.try_create`/`with_fragment`/`json`/`is_embedded`/`get_remote_url`/`resource_to_stream` the extraction surface (the module-level `_STORE_DECODER` reading `json()` once, `resource_to_stream` drawing the bound thumbnail bytes); `Signer.from_info`/`from_callback` the two signer seams; `C2paSignerInfo`/`C2paSigningAlg`/`C2paBuilderIntent`/`C2paDigitalSourceType` the ctypes value + `IntEnum` vocabularies the `_SIGNING_ALG`/`_INTENT`/`_DIGITAL_SOURCE` name-correspondence tables project onto; `C2paError` the typed subclass family the `_TRANSIENT` subset and the resilient `_drawn`/`_opened` traps read; `Context`/`Settings`/`ContextBuilder` the per-instance verify policy each case threads (the `CredentialSettings.context()` owner authoring it through `Settings.from_dict` trust+verify config → `Context.builder().with_settings().build()`); `sdk_version` the core-version evidence scalar; and the `c2pa.c2pa` inner primitives `format_embeddable` (the `Embed` rewrap) + `ed25519_sign` (the `CallbackSigner.ed25519` no-HSM digest-signer). Substrate rails stack ON TOP: `msgspec` (`Struct(frozen=True, gc=False)` the specs + decode leaves + evidence scalar, `Raw` the opaque per-assertion `data`, `json.Decoder(type=...)` the one `_Store`/`_ActionData` decode, `field(default_factory=...)` the nested-struct defaults, `structs.replace` the `Manifest` transitions); `expression` (`tagged_union`/`tag`/`case` the `Provenance`/`Ingredient`/`SignerSpec` unions, `Option`/`Nothing`/`of_optional`/`default_value` the absence carriers, `extra.result.catch` the single-exception substrate trap `_drawn` composes); `beartype` (`beartype(conf=FAULT_CONF)` the contract weave on `SignerSpec.cose`); ` the transient TSA/remote-manifest network weave on `_emit`); `anyio` (`to_thread.run_sync(limiter=_THREAD_GATE)` the bounded thread offload, `CapacityLimiter` the explicit thread band); `functools` (`partial` binding the `ed25519_sign` PEM key); `builtins.frozendict` (the derived `_SIGNING_ALG`/`_INTENT`/`_DIGITAL_SOURCE` tables and the evidence `resources` band); `pydantic-settings` (`BaseSettings`/`SettingsConfigDict` the `CredentialSettings` `RASM_CREDENTIAL_` env-admission owner projecting the trust-list-anchored verify `Context`, mirroring the `exchange/detect#DETECT` `DetectSettings` sibling); `pathlib.Path` (the trust-file locators `context()` reads to PEM content); runtime (`identity.ContentIdentity`/`ContentKey`, `faults.FAULT_CONF`/`RuntimeRail`/`async_boundary`). The in-process ctypes core needs no `anyio.to_process` lane, distinct from the subprocess-crossing `exchange/detect#DETECT`/`exchange/metadata#METADATA` siblings.
- Growth: a new operation is one `Provenance` case with its typed payload and one `match` arm (the `Embed` case over `format_embeddable` is exactly this growth); a new signer seam is one `SignerSpec` case with its typed signer-struct and one `cose`/`alg` arm, and an in-lane signer convenience is one `CallbackSigner` factory binding a concrete digest-signer (the `ed25519` factory over the in-process `ed25519_sign` primitive is exactly this, needing no new case); a new signing algorithm, intent, or digital-source origin is one `SigningAlg`/`Intent`/`DigitalSource` `StrEnum` member whose c2pa row derives by name correspondence, never a hand-enumerated table edit; a new ingredient attach modality is one `Ingredient` case and one `author` arm; a new manifest assertion is one `ActionDefinition` row; a new manifest resource to ATTACH is one `Resource` on the authoring `Manifest.resources` tuple folded through `add_resource`; a new evidence fact is one field on `CredentialEvidence` (the extracted-thumbnail `resources` read band is exactly this, distinct from the authoring `Manifest.resources` tuple — one is resources to embed, the other resources extracted back through `resource_to_stream`), and when contract-settled one scalar on the shared `ArtifactReceipt.Credential` case; a new readable store field is one field on the `_Store`/`_Manifest`/`_ValidationResults`/`_ResourceRef` boundary struct the one decode populates (the claim/ingredient `thumbnail` `_ResourceRef` is exactly this); a new verify/trust deployment knob is one `CredentialSettings` field plus one `Settings.from_dict` key in `context()`; a transient network fault widens the `_TRANSIENT` tuple; zero new surface. A whole-builder archive (`Builder.to_archive`/`from_archive`) is deliberately unbuilt — the frozen `Manifest` value is the reusable authoring template re-authored per asset, so only the processed-ingredient archive (which the JSON re-author cannot cheaply reproduce) earns an `Ingredient` case.
- Boundary: a per-format reader (`PngReader`/`JpegReader`), a per-algorithm signer class, a hand-rolled JUMBF/COSE manifest codec, a `get_active_manifest().get("label")` read where the label is the store's `active_manifest` KEY (the phantom always-`""` field the SDK's manifest dict never carries), a `reader.json()` `store["active_manifest"]`-as-dict mis-read, per-accessor `get_active_manifest`/`get_validation_state`/`get_validation_results` calls plus a per-label `get_manifest` loop where one `_STORE_DECODER.decode(reader.json())` carries the whole store, a local re-computation of `validation_state`, a five-of-eighteen `DigitalSource` subset, a hand-enumerated alg/intent/source `frozendict` where the `StrEnum`-name correspondence derives it, an un-gated `Builder.sign` that lets a PDF/`arw`/`nef` asset reach the native signer where `get_supported_mime_types` preflights it, a no-retry network sign where the `ta_url`/remote seam needs the transient the runtime retry class, a `C2paSignerInfo(ta_url=None)` that raises `TypeError` (the constructor demands `str`/`bytes`) where the no-TSA absence projects to `""` at the `from_info` seam, a bare `Option.of_optional(Reader.with_fragment(...))` whose `is_none` branch is dead — the non-optional reader raises `ManifestNotFound` rather than returning `None` — where the shared `_opened` projector folds that raise to the `unsigned` read, a sidecar evidence read (a `remote_url`-bearing `Sign`) over the `set_no_embed` asset that carries no embedded manifest (degrading to bogus `unsigned` evidence) where the detached manifest bytes `Builder.sign` returns feed `Reader.try_create(..., manifest_data=...)`, a positional cert/key tuple decoded by index, a free-text `alg`/`intent`/`digital_source` field, a separate `Verify` op duplicating the `Read` extraction (the `validation_state`/`validation_results` evidence is the verification result), a separate `Ingredient` op duplicating `Manifest.ingredients`, a one-field `Provenance` wrapper over a separate `ProvenanceOp` union where the `tagged_union` owns dispatch and async entry directly, a `@staticmethod` factory returning the string-quoted `"Provenance"`/`"Ingredient"` where the `@classmethod` returns `Self` (mirroring the conformance sibling's factory convention), a per-instance `context` field the `ReadFragment` arm silently drops where the verify `Context` rides each case that consumes it, an opaque unanchored campaign `Context` treated as a black-box value (leaving `validation_state`/`validation_results` un-anchored) where the `CredentialSettings` `pydantic-settings` owner admits the trust-list + verify policy ONCE at the composition root and projects the configured verify `Context` through `Settings.from_dict` + `Context.builder()`, closing the `exchange/detect#DETECT` `DetectSettings` / `exchange/metadata#METADATA` `MetaSettings` env-admission asymmetry credential alone lacked, a four-scalar `ArtifactReceipt` return discarding the rich `CredentialEvidence` the verification produces where the `(ContentKey, CredentialEvidence)` pair carries it, a `cryptography`-external Ed25519 callback for the common no-HSM case where the `CallbackSigner.ed25519` factory binds the in-process `ed25519_sign` primitive as the `sign` field, a lineage read that decodes the ingredient metadata but drops the bound thumbnail resources where `resource_to_stream` extracts them into the `CredentialEvidence.resources` band, a captured detached manifest discarded after the sidecar sign where `format_embeddable` rewraps it into the embeddable block the `Embed` case produces, a whole `detailed_json`/`crjson` string carried on the frozen evidence where the typed `_Store` decode already holds every fact off `json()`, and a `dict[str, Any]` manifest payload where the `ManifestDefinition`/`ActionDefinition`/`IngredientDefinition` `TypedDict` carries the typed `ReadOnly` extension band are the deleted forms; this owner is content-credential binding over already-emitted artifact bytes and owns no artifact production, mints no certificate or key (the campaign signer config supplies cert/key/TSA, the `CallbackSigner` case keeps private-key material in the `cryptography` keyring or HSM and only `C2paSigningAlg` plus the PEM cert chain cross the seam), and routes a PDF or raw-camera (`arw`/`nef`) asset to the `exchange/conformance#CONFORMANCE` `pyhanko` PAdES rail rather than `Builder.sign` (read-only here, proven by the `_SIGNABLE` set). The `ArtifactReceipt.Credential` case-tuple carrying the flat `(key, manifest_id, signer, assertions, validation_state)` scalars lands on the same-domain `core/receipt#RECEIPT` owner (the `Credential` case beside `Media`); the consumer projects those four scalars from the returned `CredentialEvidence` exactly as it mints `ArtifactReceipt.Verdict` from the conformance verdict, so `receipt.py` imports no producer value object and the `credential` case stays flat scalars. The signed-artifact `ContentKey` is the wire to the `csharp:Rasm.Persistence` store — the binding decodes the `XxHash128` seed the persistence owner re-derives over the same signed bytes, so the C2PA manifest and the content key co-identify one durable artifact across the language boundary, the credential never re-minting the content key the runtime owns.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
from collections.abc import Callable
from enum import StrEnum
from functools import partial
from io import BytesIO
from pathlib import Path
from typing import Final, Literal, NotRequired, ReadOnly, Required, Self, TypedDict, assert_never

from anyio import CapacityLimiter, to_thread
from beartype import beartype
from c2pa import (
    Builder,
    C2paBuilderIntent,
    C2paDigitalSourceType,
    C2paError,
    C2paSignerInfo,
    C2paSigningAlg,
    Context,
    Reader,
    Settings,
    Signer,
    sdk_version,
)
from c2pa.c2pa import ed25519_sign, format_embeddable
from expression import Nothing, Option, case, tag, tagged_union
from expression.collections import Map
from expression.extra.result import catch
from msgspec import Raw, Struct, field, json, structs
from pydantic_settings import BaseSettings, SettingsConfigDict

from rasm.runtime.identity import CANONICAL_POLICY, ContentIdentity, ContentKey
from rasm.runtime.lanes import LanePolicy, Modality
from rasm.runtime.resilience import RetryClass
from rasm.runtime.faults import FAULT_CONF, RuntimeRail, async_boundary


# --- [TYPES] ----------------------------------------------------------------------------
class SigningAlg(StrEnum):
    ES256 = "es256"
    ES384 = "es384"
    ES512 = "es512"
    PS256 = "ps256"
    PS384 = "ps384"
    PS512 = "ps512"
    ED25519 = "ed25519"


class Intent(StrEnum):
    CREATE = "create"
    EDIT = "edit"
    UPDATE = "update"


# the full IPTC digital-source vocabulary `C2paDigitalSourceType` carries (EMPTY is the no-source
# default the `author` fold supplies); member NAMES align with the c2pa enum so the row derives.
class DigitalSource(StrEnum):
    ALGORITHMICALLY_ENHANCED = "algorithmically_enhanced"
    ALGORITHMIC_MEDIA = "algorithmic_media"
    COMPOSITE = "composite"
    COMPOSITE_CAPTURE = "composite_capture"
    COMPOSITE_SYNTHETIC = "composite_synthetic"
    COMPOSITE_WITH_TRAINED_ALGORITHMIC_MEDIA = "composite_with_trained_algorithmic_media"
    COMPUTATIONAL_CAPTURE = "computational_capture"
    DATA_DRIVEN_MEDIA = "data_driven_media"
    DIGITAL_CAPTURE = "digital_capture"
    DIGITAL_CREATION = "digital_creation"
    HUMAN_EDITS = "human_edits"
    NEGATIVE_FILM = "negative_film"
    POSITIVE_FILM = "positive_film"
    PRINT = "print"
    SCREEN_CAPTURE = "screen_capture"
    TRAINED_ALGORITHMIC_DATA = "trained_algorithmic_data"
    TRAINED_ALGORITHMIC_MEDIA = "trained_algorithmic_media"
    VIRTUAL_RECORDING = "virtual_recording"


# the C2PA manifest/action/ingredient authoring JSON the SDK consumes; typed `ReadOnly`
# `extra_items` payloads, dicts at runtime so the foreign API accepts them unchanged.
class ManifestDefinition(TypedDict, extra_items=object):
    claim_generator: NotRequired[ReadOnly[str]]
    claim_generator_info: NotRequired[ReadOnly[list[object]]]
    title: NotRequired[ReadOnly[str]]
    format: NotRequired[ReadOnly[str]]
    vendor: NotRequired[ReadOnly[str]]
    instance_id: NotRequired[ReadOnly[str]]


class ActionDefinition(TypedDict, extra_items=object):
    action: Required[ReadOnly[str]]
    softwareAgent: NotRequired[ReadOnly[str]]
    digitalSourceType: NotRequired[ReadOnly[str]]
    parameters: NotRequired[ReadOnly[dict[str, object]]]
    when: NotRequired[ReadOnly[str]]


class IngredientDefinition(TypedDict, extra_items=object):
    title: NotRequired[ReadOnly[str]]
    format: NotRequired[ReadOnly[str]]
    relationship: NotRequired[ReadOnly[str]]
    document_id: NotRequired[ReadOnly[str]]
    instance_id: NotRequired[ReadOnly[str]]


# --- [CONSTANTS] ------------------------------------------------------------------------
# the native signable MIME set the `Sign` preflight gates on, the network-transient C2PA fault
# subset the runtime retry class re-attempts (TSA timestamp + remote-manifest fetch), and the `c2pa.actions`
# assertion label whose `data` the evidence walk decodes the declared action history from.
_SIGNABLE: Final[frozenset[str]] = frozenset(Builder.get_supported_mime_types())
_TRANSIENT: Final[tuple[type[C2paError], ...]] = (C2paError.RemoteManifest, C2paError.Io)
_ACTIONS_LABEL: Final[str] = "c2pa.actions"

# --- [TABLES] ---------------------------------------------------------------------------
# each row derives from the StrEnum-name <-> c2pa-enum-name correspondence; a new alg/intent/source
# is one StrEnum member, the c2pa member resolved by name — never a hand-enumerated mapping.
_SIGNING_ALG: Final[Map[SigningAlg, C2paSigningAlg]] = Map.of_seq((a, C2paSigningAlg[a.name]) for a in SigningAlg)
_INTENT: Final[Map[Intent, C2paBuilderIntent]] = Map.of_seq((i, C2paBuilderIntent[i.name]) for i in Intent)
_DIGITAL_SOURCE: Final[Map[DigitalSource, C2paDigitalSourceType]] = Map.of_seq((s, C2paDigitalSourceType[s.name]) for s in DigitalSource)

# --- [MODELS] ---------------------------------------------------------------------------


@tagged_union(frozen=True)
class Ingredient:
    tag: Literal["stream", "archive"] = tag()
    stream: tuple[IngredientDefinition, str, bytes] = case()
    archive: bytes = case()

    @classmethod
    def Stream(cls, definition: IngredientDefinition, fmt: str, source: bytes, /) -> Self:
        return cls(stream=(definition, fmt, source))

    @classmethod
    def Archive(cls, archive: bytes, /) -> Self:
        return cls(archive=archive)


class Resource(Struct, frozen=True, gc=False):
    uri: str
    source: bytes


class Manifest(Struct, frozen=True):
    definition: ManifestDefinition
    actions: tuple[ActionDefinition, ...] = ()
    ingredients: tuple[Ingredient, ...] = ()
    resources: tuple[Resource, ...] = ()
    intent: tuple[Intent, DigitalSource | None] | None = None

    def with_ingredient(self, ingredient: Ingredient, /) -> Self:
        return structs.replace(self, ingredients=(*self.ingredients, ingredient))

    def with_resource(self, resource: Resource, /) -> Self:
        return structs.replace(self, resources=(*self.resources, resource))

    def author(self, context: Context | None = None, *, remote_url: str | None = None) -> Builder:
        builder = Builder.from_json(self.definition, context=context)
        if self.intent is not None:
            intent, origin = self.intent
            builder.set_intent(_INTENT[intent], _DIGITAL_SOURCE[origin] if origin is not None else C2paDigitalSourceType.EMPTY)
        if remote_url is not None:
            builder.set_no_embed()
            builder.set_remote_url(remote_url)
        for action in self.actions:
            builder.add_action(action)
        for ingredient in self.ingredients:
            match ingredient:
                case Ingredient(tag="stream", stream=(definition, fmt, source)):
                    builder.add_ingredient_from_stream(definition, fmt, BytesIO(source))
                case Ingredient(tag="archive", archive=archive):
                    builder.add_ingredient_from_archive(BytesIO(archive))
                case _ as unreachable:
                    assert_never(unreachable)
        for resource in self.resources:
            builder.add_resource(resource.uri, BytesIO(resource.source))
        return builder


class CertKeySigner(Struct, frozen=True):
    alg: SigningAlg
    sign_cert: bytes
    private_key: bytes
    ta_url: str | None = None


class CallbackSigner(Struct, frozen=True):
    alg: SigningAlg
    certs: str
    sign: Callable[[bytes], bytes]
    ta_url: str | None = None

    @classmethod
    def ed25519(cls, certs: str, private_key: str, /, *, ta_url: str | None = None) -> Self:
        # the in-process Ed25519 no-HSM digest-signer: the `c2pa.c2pa` `ed25519_sign(data, private_key)`
        # byte primitive binds the PEM `private_key` as the raw COSE-signature callback, so the ED25519 arm
        # signs self-contained without forcing an external keyring/HSM callable for the common case.
        return cls(alg=SigningAlg.ED25519, certs=certs, sign=partial(ed25519_sign, private_key=private_key), ta_url=ta_url)


@tagged_union(frozen=True)
class SignerSpec:
    tag: Literal["cert_key", "callback"] = tag()
    cert_key: CertKeySigner = case()
    callback: CallbackSigner = case()

    @property
    def alg(self) -> SigningAlg:
        match self:
            case SignerSpec(tag="cert_key", cert_key=CertKeySigner(alg=alg)) | SignerSpec(tag="callback", callback=CallbackSigner(alg=alg)):
                return alg
            case _ as unreachable:
                assert_never(unreachable)

    @beartype(conf=FAULT_CONF)
    def cose(self) -> Signer:
        match self:
            case SignerSpec(tag="cert_key", cert_key=CertKeySigner() as s):
                # `C2paSignerInfo` requires `ta_url` as str/bytes and rejects `None`; the no-TSA absence projects to "" at the seam.
                return Signer.from_info(
                    C2paSignerInfo(alg=_SIGNING_ALG[s.alg], sign_cert=s.sign_cert, private_key=s.private_key, ta_url=s.ta_url or "")
                )
            case SignerSpec(tag="callback", callback=CallbackSigner() as s):
                return Signer.from_callback(s.sign, _SIGNING_ALG[s.alg], s.certs, s.ta_url)
            case _ as unreachable:
                assert_never(unreachable)


# the typed projection of the `Reader.json()` manifest STORE, admitted once through one
# `msgspec.json.Decoder`; unknown keys fall away and every field defaults, so a partial or absent
# store never breaks the fold. The active-manifest LABEL is the store's `active_manifest` KEY (NOT a
# field inside the manifest dict), so `manifests[active_manifest]` is the content the walk reads.
class _SignatureInfo(Struct, frozen=True, gc=False):
    alg: str = ""
    issuer: str = ""
    cert_serial_number: str = ""
    time: str = ""
    revocation_status: str = ""


class _ValidationCheck(Struct, frozen=True, gc=False):
    code: str = ""


# the structured `claim_generator_info` row (name+version per signing tool) the modern manifest
# carries beside the legacy flat `claim_generator` string; one value object serves decode and receipt.
class GeneratorRef(Struct, frozen=True, gc=False):
    name: str = ""
    version: str = ""


# one declared `c2pa.actions` entry: the edit/creation verb plus its per-action `digitalSourceType`
# AI-origin token — the forensic provenance the `set_intent`/`add_action` authoring side declares,
# read back symmetrically rather than collapsed to an assertion-label count.
class ActionRef(Struct, frozen=True, gc=False, rename={"source_type": "digitalSourceType"}):
    action: str = ""
    source_type: str = ""


class _ActionData(Struct, frozen=True, gc=False):
    actions: tuple[ActionRef, ...] = ()


# every assertion's `data` is held opaque as `msgspec.Raw` (the assertion-data shapes are
# heterogeneous), decoded to `_ActionData` only for the `c2pa.actions` label; `label` feeds the label
# tuple, `data` the decoded action history.
class _Assertion(Struct, frozen=True, gc=False):
    label: str = ""
    data: Raw = Raw(b"{}")


# the `ResourceRef` decode leaf every thumbnail/resource reference in the store JSON carries — the
# `format` MIME plus the `identifier` JUMBF URI `Reader.resource_to_stream` resolves to the bound bytes.
class _ResourceRef(Struct, frozen=True, gc=False):
    format: str = ""
    identifier: str = ""


# the per-ingredient lineage edge: the decode element `_Manifest.ingredients` admits AND the public
# lineage value `CredentialEvidence` carries, so one value object serves the read and the receipt —
# carrying the cross-asset `document_id`, the bound `thumbnail` `ResourceRef`, and the ingredient's own
# `validation_status` codes.
class IngredientRef(Struct, frozen=True, gc=False):
    relationship: str = ""
    title: str = ""
    format: str = ""
    instance_id: str = ""
    document_id: str = ""
    thumbnail: _ResourceRef = field(default_factory=_ResourceRef)
    validation_status: tuple[_ValidationCheck, ...] = ()


class _Manifest(Struct, frozen=True, gc=False):
    instance_id: str = ""
    title: str = ""
    format: str = ""
    claim_generator: str = ""
    claim_generator_info: tuple[GeneratorRef, ...] = ()
    signature_info: _SignatureInfo = field(default_factory=_SignatureInfo)
    thumbnail: _ResourceRef = field(default_factory=_ResourceRef)
    assertions: tuple[_Assertion, ...] = ()
    ingredients: tuple[IngredientRef, ...] = ()


class _ManifestValidation(Struct, frozen=True, gc=False):
    success: tuple[_ValidationCheck, ...] = ()
    informational: tuple[_ValidationCheck, ...] = ()
    failure: tuple[_ValidationCheck, ...] = ()


class _ValidationDeltas(Struct, frozen=True, gc=False):
    failure: tuple[_ValidationCheck, ...] = ()


class _IngredientDelta(Struct, frozen=True, gc=False, rename={"validation_deltas": "validationDeltas"}):
    validation_deltas: _ValidationDeltas = field(default_factory=_ValidationDeltas)


class _ValidationResults(Struct, frozen=True, gc=False, rename={"active_manifest": "activeManifest", "ingredient_deltas": "ingredientDeltas"}):
    active_manifest: _ManifestValidation = field(default_factory=_ManifestValidation)
    ingredient_deltas: tuple[_IngredientDelta, ...] = ()


class _Store(Struct, frozen=True, gc=False):
    active_manifest: str = ""
    manifests: dict[str, _Manifest] = field(default_factory=dict)
    validation_state: str = ""
    validation_results: _ValidationResults = field(default_factory=_ValidationResults)


_STORE_DECODER: Final = json.Decoder(type=_Store)
_ACTION_DECODER: Final = json.Decoder(type=_ActionData)


class CredentialEvidence(Struct, frozen=True, gc=False):
    manifest_id: str
    signer: str
    assertions: int
    ingredients: int
    validation_state: str
    manifest_count: int = 1
    instance_id: str = ""
    title: str = ""
    format: str = ""
    claim_generator: str = ""
    issuer: str = ""
    signed_at: str = ""
    cert_serial: str = ""
    revocation_status: str = ""
    timestamped: bool = False
    embedded: bool = True
    remote_url: str = ""
    sdk_version: str = ""
    validation_failures: tuple[str, ...] = ()
    validation_successes: tuple[str, ...] = ()
    validation_informationals: tuple[str, ...] = ()
    ingredient_validation_failures: tuple[str, ...] = ()
    assertion_labels: tuple[str, ...] = ()
    generators: tuple[GeneratorRef, ...] = ()
    actions: tuple[ActionRef, ...] = ()
    ingredient_lineage: tuple[IngredientRef, ...] = ()
    # the bound thumbnail resources extracted through `Reader.resource_to_stream` — the claim thumbnail
    # plus every ingredient thumbnail, keyed by JUMBF identifier; the forensic/DAM evidence the pair
    # carries, dropped at the four-scalar receipt projection so the raw bytes never reach the wire.
    resources: frozendict[str, bytes] = frozendict()

    @classmethod
    def measure(cls, store: _Store, *, embedded: bool, remote_url: str, signer: str, resources: frozendict[str, bytes]) -> Self:
        active = store.manifests.get(store.active_manifest, _Manifest())
        info, results = active.signature_info, store.validation_results
        return cls(
            manifest_id=store.active_manifest,
            signer=signer or info.alg,
            assertions=len(active.assertions),
            ingredients=len(active.ingredients),
            validation_state=store.validation_state or "unsigned",
            manifest_count=len(store.manifests),
            instance_id=active.instance_id,
            title=active.title,
            format=active.format,
            claim_generator=active.claim_generator,
            issuer=info.issuer,
            signed_at=info.time,
            cert_serial=info.cert_serial_number,
            revocation_status=info.revocation_status,
            timestamped=bool(info.time),
            embedded=embedded,
            remote_url=remote_url,
            sdk_version=sdk_version(),
            validation_failures=tuple(check.code for check in results.active_manifest.failure),
            validation_successes=tuple(check.code for check in results.active_manifest.success),
            validation_informationals=tuple(check.code for check in results.active_manifest.informational),
            ingredient_validation_failures=tuple(check.code for delta in results.ingredient_deltas for check in delta.validation_deltas.failure),
            assertion_labels=tuple(item.label for item in active.assertions),
            generators=active.claim_generator_info,
            actions=tuple(
                action for item in active.assertions if item.label == _ACTIONS_LABEL for action in _ACTION_DECODER.decode(item.data).actions
            ),
            ingredient_lineage=active.ingredients,
            resources=resources,
        )

    @classmethod
    def unsigned(cls, signer: str, /) -> Self:
        return cls(
            manifest_id="", signer=signer, assertions=0, ingredients=0, validation_state="unsigned", manifest_count=0, sdk_version=sdk_version()
        )


# the per-case request payloads `Provenance` discriminates over — the typed spec the asset bytes pair
# with, mirroring `exchange/conformance#CONFORMANCE`'s `(bytes, Spec)` cases over a naked positional
# tuple; the verify `Context` rides the `SignSpec`/`ReadSpec` that consume it, `FragmentSpec` carries
# none because `Reader.with_fragment` takes no `context`, so a dropped slot is structurally unspellable.
class SignSpec(Struct, frozen=True):
    manifest: Manifest
    fmt: str
    signer: SignerSpec
    remote_url: str | None = None
    context: Context | None = None


class ReadSpec(Struct, frozen=True):
    fmt: str
    context: Context | None = None


class FragmentSpec(Struct, frozen=True):
    fmt: str
    fragment: bytes


# the rewrap payload: `manifest` the captured DETACHED/remote-store manifest bytes (distinct from
# `SignSpec.manifest` the authoring template), `fmt` the target format whose embeddable convention
# `format_embeddable` rewraps into, `context` the verify policy the evidence read threads.
class EmbedSpec(Struct, frozen=True):
    fmt: str
    manifest: bytes
    context: Context | None = None


# the deployment trust-list + verify policy admitted ONCE at the composition root — closing the env-admission
# asymmetry where the `exchange/detect#DETECT` `DetectSettings` and `exchange/metadata#METADATA` `MetaSettings`
# siblings each own a `pydantic-settings` deployment owner and credential ALONE treated its verify `Context` as
# an opaque campaign value. `context()` projects ONE configured verify `Context` — the trust files read to PEM
# content, folded with the verify policy into `Settings.from_dict`, built through
# `Context.builder().with_settings().build()` — threaded into the `SignSpec`/`ReadSpec`/`EmbedSpec.context` every
# arm consumes, so the `validation_state`/`validation_results` the evidence carries anchor against a REAL
# trust-anchor set rather than an unanchored default. `ReadFragment` takes none (`Reader.with_fragment` has no
# `context`), so the asymmetry is structural, never a dropped slot.
def _credential_raise(fault: object) -> tuple[bytes, "CredentialEvidence"]:
    # terminal collapse at the signing boundary: an offload fault reconstructs the raise the node's rail folds.
    raise ValueError(str(fault))


class CredentialSettings(BaseSettings):
    model_config = SettingsConfigDict(env_prefix="RASM_CREDENTIAL_", frozen=True, extra="forbid")
    trust_anchors: Path | None = None  # PEM trust-anchor bundle the verify chains against
    trust_config: Path | None = None  # the allowed-EKU / signing-config the verify enforces
    allowed_list: Path | None = None  # explicit end-entity allow-list
    ta_url: str = ""  # default RFC-3161 TSA the campaign signer stamps against
    verify_trust: bool = True
    verify_timestamp_trust: bool = True
    remote_manifest_fetch: bool = True
    ocsp_fetch: bool = False

    def context(self) -> Context:
        trust = {
            key: path.read_text()
            for key, path in (("trust_anchors", self.trust_anchors), ("trust_config", self.trust_config), ("allowed_list", self.allowed_list))
            if path is not None
        }
        settings = Settings.from_dict({
            "verify": {
                "verify_trust": self.verify_trust,
                "verify_timestamp_trust": self.verify_timestamp_trust,
                "remote_manifest_fetch": self.remote_manifest_fetch,
                "ocsp_fetch": self.ocsp_fetch,
            },
            **({"trust": trust} if trust else {}),
        })
        return Context.builder().with_settings(settings).build()


# `Provenance` IS the closed union: the dispatch, the async entry, and every case body fold onto one
# owner (mirroring `exchange/conformance#CONFORMANCE`), each case a `(bytes, Spec)` pair whose asset
# bytes lead and whose typed spec carries the rest.
@tagged_union(frozen=True)
class Provenance:
    tag: Literal["sign", "read", "read_fragment", "embed"] = tag()
    sign: tuple[bytes, SignSpec] = case()
    read: tuple[bytes, ReadSpec] = case()
    read_fragment: tuple[bytes, FragmentSpec] = case()
    embed: tuple[bytes, EmbedSpec] = case()

    @classmethod
    def Sign(cls, asset: bytes, spec: SignSpec, /) -> Self:
        return cls(sign=(asset, spec))

    @classmethod
    def Read(cls, asset: bytes, spec: ReadSpec, /) -> Self:
        return cls(read=(asset, spec))

    @classmethod
    def ReadFragment(cls, init: bytes, spec: FragmentSpec, /) -> Self:
        return cls(read_fragment=(init, spec))

    @classmethod
    def Embed(cls, asset: bytes, spec: EmbedSpec, /) -> Self:
        return cls(embed=(asset, spec))

    def emit(self, /) -> ArtifactWork:
        return ArtifactWork(key=self._key, work=self._emit, parents=(), admission=Admission(keyed=None), cost=1.0)

    @property
    def _key(self) -> ContentKey:
        # key-over-INPUT: canonical provenance request minted PRE-RUN — the signed asset's own address rides the evidence.
        return ContentIdentity.of(f"credential-{self.tag}", self, policy=CANONICAL_POLICY)

    async def _emit(self) -> RuntimeRail[ArtifactReceipt]:
        # the TSA/remote-manifest transient rides the runtime OCCT retry class; the c2pa sign crosses the thread lane.
        railed = await async_boundary(f"credential.{self.tag}", self._closed)
        return railed.map(
            lambda ev: ArtifactReceipt.Credential(self._key, ev.manifest_id, ev.signer, ev.assertions, ev.validation_state)
        )

    async def _closed(self) -> "CredentialEvidence":
        crossed = await LanePolicy.offload(self._run, modality=Modality.THREAD, retry=RetryClass.OCCT)
        _asset, evidence = crossed.default_with(_credential_raise)
        return evidence

    def _run(self) -> tuple[bytes, CredentialEvidence]:
        match self:
            case Provenance(tag="sign", sign=(asset, spec)):
                return self._signed(asset, spec)
            case Provenance(tag="read", read=(asset, spec)):
                return asset, self._evidence(self._opened(lambda: Reader.try_create(spec.fmt, BytesIO(asset), context=spec.context)), "")
            case Provenance(tag="read_fragment", read_fragment=(init, spec)):
                return init, self._evidence(self._opened(lambda: Reader.with_fragment(spec.fmt, BytesIO(init), BytesIO(spec.fragment))), "")
            case Provenance(tag="embed", embed=(asset, spec)):
                return self._embedded(asset, spec)
            case _ as unreachable:
                assert_never(unreachable)

    def _signed(self, asset: bytes, spec: SignSpec, /) -> tuple[bytes, CredentialEvidence]:
        if spec.fmt not in _SIGNABLE:
            raise C2paError.NotSupported(f"credential.sign: {spec.fmt} is read-only here; route pdf/arw/nef to exchange/conformance")
        sink = BytesIO()
        # `sign` returns the detached manifest bytes AND writes the asset into `sink`; the sidecar path
        # (a `set_no_embed` asset carries no embedded manifest, its remote store unpublished at sign time)
        # reads its evidence through `manifest_data=detached` where an embedded-style read would yield
        # `unsigned`, while the embedded path passes `manifest_data=None` — one `try_create` either way.
        detached = spec.manifest.author(spec.context, remote_url=spec.remote_url).sign(spec.signer.cose(), spec.fmt, BytesIO(asset), sink)
        signed = sink.getvalue()
        reader = self._opened(
            lambda: Reader.try_create(
                spec.fmt, BytesIO(signed), manifest_data=detached if spec.remote_url is not None else None, context=spec.context
            )
        )
        return signed, self._evidence(reader, spec.signer.alg)

    def _embedded(self, asset: bytes, spec: EmbedSpec, /) -> tuple[bytes, CredentialEvidence]:
        # rewrap a captured detached/remote-store manifest into the format's embeddable JUMBF block
        # (`format_embeddable`), the credential block a downstream writer splices into the asset — this
        # owner produces the block and reads its evidence, never re-authoring the asset. The `_SIGNABLE`
        # gate rejects a non-embeddable format, and evidence reads through the same `manifest_data` path the
        # sidecar `Sign` verify uses (the detached bytes bound to the asset), never a degraded `unsigned`.
        if spec.fmt not in _SIGNABLE:
            raise C2paError.NotSupported(f"credential.embed: {spec.fmt} has no embeddable manifest form; route pdf/arw/nef to exchange/conformance")
        _size, block = format_embeddable(spec.fmt, spec.manifest)
        reader = self._opened(lambda: Reader.try_create(spec.fmt, BytesIO(asset), manifest_data=spec.manifest, context=spec.context))
        return block, self._evidence(reader, "")

    @staticmethod
    def _opened(make: Callable[[], Reader | None], /) -> Option[Reader]:
        # `try_create` maps a credential-free asset to `None`; `with_fragment` raises `ManifestNotFound` instead — both project to `unsigned`.
        try:
            return Option.of_optional(make())
        except C2paError.ManifestNotFound:
            return Nothing

    def _evidence(self, reader: Option[Reader], signer: str, /) -> CredentialEvidence:
        if reader.is_none():
            return CredentialEvidence.unsigned(signer)
        with reader.value as live:
            store = _STORE_DECODER.decode(live.json())
            return CredentialEvidence.measure(
                store,
                embedded=live.is_embedded(),
                remote_url=Option.of_optional(live.get_remote_url()).default_value(""),
                signer=signer,
                resources=self._drawn(live, store),
            )

    @staticmethod
    def _drawn(live: Reader, store: _Store, /) -> frozendict[str, bytes]:
        # best-effort forensic capture of the bound thumbnail resources — the claim thumbnail plus every
        # ingredient thumbnail `identifier` — each written through `resource_to_stream` into owned bytes on
        # the same live-reader borrow; a missing/broken resource skips (counted absent by omission through
        # the `catch`-narrowed `Option`), never faulting the credential read.
        active = store.manifests.get(store.active_manifest, _Manifest())
        refs = (active.thumbnail, *(ingredient.thumbnail for ingredient in active.ingredients))

        def drawn(uri: str, /) -> Option[bytes]:
            sink = BytesIO()
            return catch(exception=C2paError)(live.resource_to_stream)(uri, sink).map(lambda _n: sink.getvalue()).to_option()

        return frozendict({ref.identifier: got.value for ref in refs if ref.identifier and (got := drawn(ref.identifier)).is_some()})


# --- [COMPOSITION] ----------------------------------------------------------------------
# every sign/read offload threads this one explicit bound, so N concurrent `close` calls share a fixed
# thread band rather than the per-loop 40-token default — the GIL-releasing native c2pa core and the
# `ta_url`/remote-manifest `requests` transport run off the event loop, bounded at the boundary.
```

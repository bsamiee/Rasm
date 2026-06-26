# [PY_ARTIFACTS_CREDENTIAL]

The C2PA content-credential owner at the exchange boundary. `Provenance` is ONE owner over `c2pa-python` binding a signed tamper-evident manifest into an emitted artifact and reading it back — the closed `tagged_union` owning the dispatch, the async entry, AND every case body directly, discriminating its `Sign`/`Sidecar`/`Read`/`ReadFragment` cases by payload shape — never a one-field wrapper over a separate op union, never a `from_file`/`from_stream`/`per-format` reader family, never a per-signing-algorithm signer type, and never a per-output `sign`/`verify`/`read` method triple. `SignerSpec` is the ONE policy union carrying one typed signer case — a `CertKeySigner` (`alg`, `sign_cert`, `private_key`, `ta_url`) feeding `Signer.from_info` and a `CallbackSigner` (`alg`, `certs`, `sign`, `ta_url`) whose campaign-supplied `sign` digest-callback feeds `Signer.from_callback` — so the `cryptography`/HSM seam is a tagged case carrying the keyring/HSM-bound signer as a typed field, the signing algorithm a `SigningAlg` `StrEnum` whose `C2paSigningAlg` row derives by name correspondence across ES/PS/ED25519, never a parallel signer owner or a free-text alg field. `CredentialEvidence` is the ONE rich provenance receipt a single `Reader.json()` manifest-STORE decode folds — the active-manifest label read off the store's `active_manifest` KEY (never a phantom `label` field inside the manifest dict the SDK never emits), the `manifests` chain count, signer alg, signature `issuer`/`time`/`cert_serial_number`/`revocation_status`, `claim_generator`/`title`/`format`, embedded flag, remote URL, SDK version, assertion labels, ingredient count, and the `validation_state` plus the `validation_results` failure/success/informational codes, the per-ingredient `ingredientDeltas` failure codes, AND the ingredient lineage relationships — returned whole as the `(ContentKey, CredentialEvidence)` pair (mirroring the `exchange/conformance#CONFORMANCE` verdict close) the consumer projects onto `core/receipt#RECEIPT` `ArtifactReceipt.Credential` as its four settled scalars, keying the signed buffer through `ContentIdentity.of` so the binding co-identifies the durable artifact with the `csharp:Rasm.Persistence` `XxHash128` content key. The native core signs and reads on an `anyio.to_thread.run_sync` worker and drives the RFC-3161 `ta_url` timestamp and remote-manifest `requests` transport there, so `stamina.retry` over the transient `C2paError.RemoteManifest`/`Io` subset re-attempts only the network seam — the same crypto-with-network concern the `exchange/conformance#CONFORMANCE` PAdES sibling closes for PDF (PDF/raw-camera are read-only here, gated out by `Builder.get_supported_mime_types`, signed on the `pyhanko` rail); this owner is cross-format content-authenticity provenance over the image/BMFF/audio signable set, owning no artifact production, only the credential close over already-emitted bytes.

## [01]-[INDEX]

- [01]-[CREDENTIAL]: C2PA manifest authoring (the full IPTC `DigitalSource` intent vocabulary, the `Ingredient` stream-or-archive attach modality) + single-use embed/sidecar signing format-gated on `Builder.get_supported_mime_types`, manifest-STORE extraction folding one `Reader.json()` decode through `msgspec.json.Decoder` into the typed `_Store` (the active-manifest label off the `active_manifest` key, the `manifests` chain, `validation_state`/`validation_results`), fragmented-BMFF reading over `Reader.with_fragment`, the per-case `SignerSpec` cert/callback policy union, the rich `CredentialEvidence` provenance receipt returned as the `(ContentKey, CredentialEvidence)` pair, the transient-only `stamina.retry` network weave, and the content-key binding the consumer projects onto the four-scalar `core/receipt#RECEIPT` `ArtifactReceipt.Credential` case and the `csharp:Rasm.Persistence` content key.

## [02]-[CREDENTIAL]

- Owner: `Provenance` the one credential owner AND the closed `tagged_union` discriminating operation over its own typed `(…, context)` payload — `Sign` (manifest + asset bytes + signer + verify `Context`, the embedded close), `Sidecar` (+ remote URL, the `set_no_embed`/`set_remote_url` non-embedded close), `Read` (asset bytes + `Context`, the extraction-and-validation pass), `ReadFragment` (init + fragment bytes, the `Reader.with_fragment` DASH/CMAF pass that takes no `Context`) — the dispatch, the async entry, AND every case body folded onto the one union, matched by one total `match`/`case`, never a one-field `Provenance` wrapper over a separate `ProvenanceOp` union and never a per-operation method family; `Manifest` the frozen authoring value carrying the typed `ManifestDefinition` payload, the `ActionDefinition` assertion sequence, the `Ingredient` closed-union list, the `Resource` list, and the `Intent`/`DigitalSource` intent row, with `with_ingredient`/`with_resource` the immutable attach transitions and `author` the one builder fold; `SignerSpec` the one policy union carrying one typed signer case per signing seam; `CredentialEvidence` the rich typed provenance receipt; the C2PA core binds in-process — the `c2pa` wheel bundles the native `libc2pa_c` per platform loaded via `ctypes` and resolves on the cp315-core process (the `macosx_11_0_arm64` `py3-none` wheel imports clean and is not on the manifest `banned-module-level-imports` list), so module-scope `from c2pa import` binds directly, no gated subprocess band, and no hand-rolled JUMBF/COSE codec.
- Cases: `Provenance` rows `Sign` (`Manifest.author` → `Builder.sign` into an in-memory `BytesIO`, single-use, the builder closes after `sign`) · `Sidecar` (the same author-and-sign fold with `Builder.set_no_embed`/`set_remote_url` folded onto the builder so the signed manifest references a remote store rather than embedding) · `Read` (`Reader.try_create` → `Reader.json` STORE decode, the optional-returning extraction mapping `ManifestNotFound`→`None` to the `unsigned` evidence) · `ReadFragment` (`Reader.with_fragment` init+fragment, the fragmented-BMFF read) — each binding the one `Builder`/`Reader` surface keyed by argument shape rather than a parallel type; `SignerSpec` cases `cert_key` (`CertKeySigner`) · `callback` (`CallbackSigner`) — one frozen signer-struct per seam whose fields are the named crypto axes the `match` arm reads directly, the `alg` axis a `SigningAlg` `StrEnum` member, so a per-algorithm signer type and a positional cert tuple are the deleted forms and no private-key material is minted here; `Ingredient` cases `stream` (`ManifestDefinition`-style definition + format + source bytes → `add_ingredient_from_stream`) · `archive` (a `write_ingredient_archive` blob → `add_ingredient_from_archive`) — the attach modality is the case the `author` fold matches, never a `from_archive: bool` knob, so a parent processed once is reused across a campaign's derived assets. The intent axis is a `Manifest` row not a parallel field: `Intent` (`CREATE`/`EDIT`/`UPDATE`) and the full IPTC `DigitalSource` vocabulary (`digital_capture`, `digital_creation`, `algorithmic_media`, `trained_algorithmic_media`, `trained_algorithmic_data`, `composite_synthetic`, `composite_capture`, `composite_with_trained_algorithmic_media`, `computational_capture`, `data_driven_media`, `human_edits`, `negative_film`, `positive_film`, `print`, `screen_capture`, `virtual_recording`, `algorithmically_enhanced`, `composite`) ride `Builder.set_intent`, so AI-and-capture provenance origin is the table-driven source-type token the verifier reads, never a free-text manifest field or a raw `C2paDigitalSourceType[name]` member-access.
- Entry: `Provenance` IS the closed `tagged_union` and owns the async entry directly — `close` dispatches over the runtime `async_boundary` returning `RuntimeRail[tuple[ContentKey, CredentialEvidence]]`, mirroring the `exchange/conformance#CONFORMANCE` `close` pair rather than a separate `ProvenanceOp` wrapper holding the methods beside a data-only union; the `Sign`/`Sidecar` arms sign and key the signed buffer, the `Read`/`ReadFragment` arms key the read asset, never a per-operation entrypoint or a `sign`/`read`/`verify` method triple. The `_emit` core carries one definition-time aspect over a thin body — `@stamina.retry(on=_TRANSIENT, ...)` re-attempts ONLY the `C2paError.RemoteManifest`/`Io` network transients (a TSA timestamp or remote-manifest fetch), so a non-transient codec/signature fault surfaces immediately — and offloads `_run` through `anyio.to_thread.run_sync` so the `async_boundary` rail never blocks the event loop while the native core signs and drives the `ta_url`/remote `requests` transport; `async_boundary` converts the residual `C2paError`/contract raise into the runtime `BoundaryFault` rail and the spine's own `rejected` line. `_emit` returns the `(ContentKey, CredentialEvidence)` pair — `ContentIdentity.of` over the signed/read bytes plus the rich evidence the read produces — never a four-scalar `ArtifactReceipt` that discards the forensic fields `measure` computes; the consumer mints `ArtifactReceipt.Credential(key, ev.manifest_id, ev.signer, ev.assertions, ev.validation_state)` from the pair exactly as the planner mints `ArtifactReceipt.Verdict` from the conformance pair, so the verification caller sees the full provenance while the receipt stays the thin four-scalar pipeline summary. The asset bytes arrive as an in-memory `BytesIO` from the imaging/document owner and leave as a signed `BytesIO`, so the codec hands a decoded buffer and receives a signed buffer with no intermediate file — `sign_file` is the path convenience only when a file handle already exists, never the default. The in-process `c2pa` wheel needs no `anyio.to_process` gated band (distinct from the host-native `exchange/detect#DETECT`/`exchange/metadata#METADATA` siblings that cross the subprocess seam).
- Auto: the build is one ordered fold over `Manifest` then signer — `Manifest.author` seeds `Builder.from_json`, folds `set_intent` from the intent row (the `DigitalSource` projected through `_DIGITAL_SOURCE`, defaulting `C2paDigitalSourceType.EMPTY` when no source is declared), folds `set_no_embed`/`set_remote_url` when a sidecar URL is supplied, folds the `add_action` assertion sequence, folds each `Ingredient` through the `add_ingredient_from_stream`/`add_ingredient_from_archive` arm its case selects, and folds the `Resource` attachments through `add_resource`, while the `SignerSpec.cose` projection (woven with the shared `@beartype(conf=FAULT_CONF)` contract) builds the `Signer.from_info`/`from_callback` row from the matched case (the Signer-free `alg` accessor stays distinct so reading the algorithm mints no native signer handle); the `Sign`/`Sidecar` arms preflight `fmt` against the module-level `_SIGNABLE` set (`Builder.get_supported_mime_types`) and `raise C2paError.NotSupported` for a read-only asset before authoring, then drive `Builder.sign(signer, format, source_stream, dest_stream)` into a fresh `BytesIO` while capturing the detached manifest bytes the `sign` call returns, and read the credential back through `_evidence` — the embedded `Sign` arm validates the manifest bound into the signed asset, while the `Sidecar` arm (whose `set_no_embed` asset carries no embedded manifest, the remote store unpublished at sign time) validates the detached bytes through `Reader.try_create(..., manifest_data=...)` so the read never degrades to bogus `unsigned` evidence; the builder closes single-use. The `Read`/`ReadFragment` arms open one `Reader.try_create`/`Reader.with_fragment` context whose `Reader.json()` STORE JSON is admitted once through the module-level `_STORE_DECODER` into the typed `_Store` — the active-manifest LABEL is the store's `active_manifest` string KEY and the manifest content is `manifests[active_manifest]` (the manifest dict carries no `label` of its own), so a `.get("label")` over the manifest dict is the phantom always-`""` read the decode replaces, and the same one decode carries `validation_state`, the `validation_results` success/informational/failure code sets, the `ingredientDeltas` per-ingredient failure codes, and the whole `manifests` chain, never per-accessor `get_active_manifest`/`get_validation_state`/`get_validation_results` calls plus a per-label `get_manifest` loop. `CredentialEvidence.measure` folds the active manifest's `label`/`title`/`format`/`claim_generator`, the `signature_info` `alg`/`issuer`/`time`/`cert_serial_number`/`revocation_status` (with a derived `timestamped` flag off the trusted-time presence), the `Reader.is_embedded`/`get_remote_url` flags, the `sdk_version`, the assertion labels and ingredient count, the `len(manifests)` chain depth, and every validation code set into one row in a single store walk, never a per-arm re-projection or a two-pass count; the `Option.of_optional(Reader.try_create(...))` `is_none` arm projects `CredentialEvidence.unsigned` when an asset carries no Content Credentials, the foreign `None` admitted to the carrier at the seam rather than threaded inward. The verify `Context` threads per case through `Builder.from_json(context=...)` and `Reader.try_create(context=...)` — the `ReadFragment` arm's `Reader.with_fragment` takes none, so its case carries no `Context` slot rather than silently dropping one — so trust-list and verify policy is the case's own settled `Context` value, never the deprecated thread-local `load_settings` path.
- Receipt: `close` returns the rich `CredentialEvidence` as the `(ContentKey, CredentialEvidence)` pair, mirroring the `exchange/conformance#CONFORMANCE` `(ContentKey, ConformanceVerdict)` close — so the verification caller receives the full provenance picture (the signer `alg`/`issuer`/signing `time`/`cert_serial_number`/`revocation_status`, the `claim_generator`/`title`/`format`, the manifest-chain depth, the embedded/remote flags, the SDK version, the success/informational/failure validation codes, the per-ingredient delta failures, the assertion labels, and the ingredient lineage relationships) rather than a four-scalar summary that discards the eighteen forensic fields `measure` computes. The consumer mints `core/receipt#RECEIPT` `ArtifactReceipt.Credential(key, ev.manifest_id, ev.signer, ev.assertions, ev.validation_state)` from the returned pair — the four settled scalars (c2pa manifest id the store's `active_manifest` label, signer identity the matched `alg` token, assertion count, `validation_state` string) projected at the coordinator exactly as `ArtifactReceipt.Verdict(key, verdict)` is minted from the conformance pair, so the `credential` case stays flat scalars and `receipt.py` imports no `CredentialEvidence` value object. The `validation_state` plus the valid/invalid evidence the pair carries is the observable provenance the runtime `observability/metrics` `MeterProvider` signal stream reads off the minted receipt fold.
- Packages: `c2pa` (`Builder`; `from_json`/`sign`/`sign_file`/`set_intent`/`add_action`/`add_ingredient_from_stream`/`add_ingredient_from_archive`/`add_resource`/`set_remote_url`/`set_no_embed`/`get_supported_mime_types`; `Reader`; `try_create`/`with_fragment`/`json`/`get_remote_url`/`is_embedded`; `Signer.from_info`/`Signer.from_callback`; `C2paSignerInfo`; `C2paSigningAlg` ES256/ES384/ES512/PS256/PS384/PS512/ED25519; `C2paBuilderIntent` CREATE/EDIT/UPDATE; the full `C2paDigitalSourceType` IPTC vocabulary + `EMPTY`; `C2paError` + nested `RemoteManifest`/`Io`/`NotSupported`; `Context`; `sdk_version`) on the cp315 core; `msgspec` (`Struct(frozen=True)` for the `Manifest`/`CredentialEvidence` owners and the `_Store`/`_Manifest`/`_ValidationResults` boundary read structs, `json.Decoder(type=_Store)` admitting one `Reader.json()` STORE string, `field`/`structs.replace` the immutable transition); `expression` (`tagged_union`/`tag`/`case`, `Option.of_optional`/`is_none`/`value` the `try_create`/remote-url absence projection); `stamina` (`retry` the transient-only network weave over `_TRANSIENT`); `beartype` (`beartype` + the shared `faults.FAULT_CONF` contract weave on `SignerSpec.cose`); `builtins.frozendict` (the name-derived `_SIGNING_ALG`/`_INTENT`/`_DIGITAL_SOURCE` dispatch tables); `anyio` (`to_thread.run_sync`); runtime (`content_identity.ContentIdentity`/`ContentKey`, `faults.RuntimeRail`/`async_boundary`/`FAULT_CONF`).
- Growth: a new operation is one `Provenance` case with its typed payload and one `match` arm; a new signer seam is one `SignerSpec` case with its typed signer-struct and one `cose`/`alg` arm; a new signing algorithm, intent, or digital-source origin is one `SigningAlg`/`Intent`/`DigitalSource` `StrEnum` member whose c2pa row derives by name correspondence, never a hand-enumerated table edit; a new ingredient attach modality is one `Ingredient` case and one `author` arm; a new manifest assertion is one `ActionDefinition` row; a new manifest resource is one `Resource` on the `resources` tuple folded through `add_resource`; a new evidence fact is one field on `CredentialEvidence` (and, when contract-settled, one scalar on the shared `ArtifactReceipt.Credential` case); a new readable store field is one field on the `_Store`/`_Manifest`/`_ValidationResults` boundary struct the one decode populates; a transient network fault widens the `_TRANSIENT` tuple; zero new surface. A whole-builder archive (`Builder.to_archive`/`from_archive`) is deliberately unbuilt — the frozen `Manifest` value is the reusable authoring template re-authored per asset, so only the processed-ingredient archive (which the JSON re-author cannot cheaply reproduce) earns an `Ingredient` case.
- Boundary: a per-format reader (`PngReader`/`JpegReader`), a per-algorithm signer class, a hand-rolled JUMBF/COSE manifest codec, a `get_active_manifest().get("label")` read where the label is the store's `active_manifest` KEY (the phantom always-`""` field the SDK's manifest dict never carries), a `reader.json()` `store["active_manifest"]`-as-dict mis-read, per-accessor `get_active_manifest`/`get_validation_state`/`get_validation_results` calls plus a per-label `get_manifest` loop where one `_STORE_DECODER.decode(reader.json())` carries the whole store, a local re-computation of `validation_state`, a five-of-eighteen `DigitalSource` subset, a hand-enumerated alg/intent/source `frozendict` where the `StrEnum`-name correspondence derives it, an un-gated `Builder.sign` that lets a PDF/`arw`/`nef` asset reach the native signer where `get_supported_mime_types` preflights it, a no-retry network sign where the `ta_url`/remote seam needs the transient `stamina.retry`, a `Sidecar` evidence read over the `set_no_embed` asset that carries no embedded manifest (degrading to bogus `unsigned` evidence) where the detached manifest bytes `Builder.sign` returns feed `Reader.try_create(..., manifest_data=...)`, a positional cert/key tuple decoded by index, a free-text `alg`/`intent`/`digital_source` field, a separate `Verify` op duplicating the `Read` extraction (the `validation_state`/`validation_results` evidence is the verification result), a separate `Ingredient` op duplicating `Manifest.ingredients`, a one-field `Provenance` wrapper over a separate `ProvenanceOp` union where the `tagged_union` owns dispatch and async entry directly, a per-instance `context` field the `ReadFragment` arm silently drops where the verify `Context` rides each case that consumes it, a four-scalar `ArtifactReceipt` return discarding the rich `CredentialEvidence` the verification produces where the `(ContentKey, CredentialEvidence)` pair carries it, and a `dict[str, Any]` manifest payload where the `ManifestDefinition`/`ActionDefinition`/`IngredientDefinition` `TypedDict` carries the typed `ReadOnly` extension band are the deleted forms; this owner is content-credential binding over already-emitted artifact bytes and owns no artifact production, mints no certificate or key (the campaign signer config supplies cert/key/TSA, the `CallbackSigner` case keeps private-key material in the `cryptography` keyring or HSM and only `C2paSigningAlg` plus the PEM cert chain cross the seam), and routes a PDF or raw-camera (`arw`/`nef`) asset to the `exchange/conformance#CONFORMANCE` `pyhanko` PAdES rail rather than `Builder.sign` (read-only here, proven by the `_SIGNABLE` set). The `ArtifactReceipt.Credential` case-tuple carrying the flat `(key, manifest_id, signer, assertions, validation_state)` scalars lands on the same-domain `core/receipt#RECEIPT` owner (the `Credential` case beside `Media`); the consumer projects those four scalars from the returned `CredentialEvidence` exactly as it mints `ArtifactReceipt.Verdict` from the conformance verdict, so `receipt.py` imports no producer value object and the `credential` case stays flat scalars. The signed-artifact `ContentKey` is the wire to the `csharp:Rasm.Persistence` store — the binding decodes the `XxHash128` seed the persistence owner re-derives over the same signed bytes, so the C2PA manifest and the content key co-identify one durable artifact across the language boundary, the credential never re-minting the content key the runtime owns.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
from collections.abc import Callable
from enum import StrEnum
from io import BytesIO
from typing import Final, Literal, NotRequired, ReadOnly, Required, Self, TypedDict, assert_never

import stamina
from anyio import to_thread
from beartype import beartype
from builtins import frozendict
from c2pa import (
    Builder,
    C2paBuilderIntent,
    C2paDigitalSourceType,
    C2paError,
    C2paSignerInfo,
    C2paSigningAlg,
    Context,
    Reader,
    Signer,
    sdk_version,
)
from expression import Option, case, tag, tagged_union
from msgspec import Struct, field, json, structs

from rasm.runtime.content_identity import ContentIdentity, ContentKey
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
# the native signable MIME set the `Sign`/`Sidecar` preflight gates on, and the network-transient
# C2PA fault subset `stamina.retry` re-attempts (TSA timestamp + remote-manifest fetch).
_SIGNABLE: Final[frozenset[str]] = frozenset(Builder.get_supported_mime_types())
_TRANSIENT: Final[tuple[type[C2paError], ...]] = (C2paError.RemoteManifest, C2paError.Io)

# --- [TABLES] ---------------------------------------------------------------------------
# each row derives from the StrEnum-name <-> c2pa-enum-name correspondence; a new alg/intent/source
# is one StrEnum member, the c2pa member resolved by name — never a hand-enumerated mapping.
_SIGNING_ALG: Final[frozendict[SigningAlg, C2paSigningAlg]] = frozendict({a: C2paSigningAlg[a.name] for a in SigningAlg})
_INTENT: Final[frozendict[Intent, C2paBuilderIntent]] = frozendict({i: C2paBuilderIntent[i.name] for i in Intent})
_DIGITAL_SOURCE: Final[frozendict[DigitalSource, C2paDigitalSourceType]] = frozendict({s: C2paDigitalSourceType[s.name] for s in DigitalSource})

# --- [MODELS] ---------------------------------------------------------------------------


@tagged_union(frozen=True)
class Ingredient:
    tag: Literal["stream", "archive"] = tag()
    stream: tuple[IngredientDefinition, str, bytes] = case()
    archive: bytes = case()

    @staticmethod
    def Stream(definition: IngredientDefinition, fmt: str, source: bytes, /) -> "Ingredient":
        return Ingredient(stream=(definition, fmt, source))

    @staticmethod
    def Archive(archive: bytes, /) -> "Ingredient":
        return Ingredient(archive=archive)


class Resource(Struct, frozen=True, gc=False):
    uri: str
    source: bytes


class Manifest(Struct, frozen=True):
    definition: ManifestDefinition
    actions: tuple[ActionDefinition, ...] = ()
    ingredients: tuple[Ingredient, ...] = ()
    resources: tuple[Resource, ...] = ()
    intent: Intent | None = None
    digital_source: DigitalSource | None = None

    def with_ingredient(self, ingredient: Ingredient, /) -> Self:
        return structs.replace(self, ingredients=(*self.ingredients, ingredient))

    def with_resource(self, resource: Resource, /) -> Self:
        return structs.replace(self, resources=(*self.resources, resource))

    def author(self, context: Context | None = None, *, remote_url: str | None = None) -> Builder:
        builder = Builder.from_json(self.definition, context=context)
        if self.intent is not None:
            source = _DIGITAL_SOURCE[self.digital_source] if self.digital_source is not None else C2paDigitalSourceType.EMPTY
            builder.set_intent(_INTENT[self.intent], source)
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
                return Signer.from_info(C2paSignerInfo(alg=_SIGNING_ALG[s.alg], sign_cert=s.sign_cert, private_key=s.private_key, ta_url=s.ta_url))
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


class _Assertion(Struct, frozen=True, gc=False):
    label: str = ""


class _ManifestIngredient(Struct, frozen=True, gc=False):
    relationship: str = ""


class _Manifest(Struct, frozen=True, gc=False):
    title: str = ""
    format: str = ""
    claim_generator: str = ""
    signature_info: _SignatureInfo = field(default_factory=_SignatureInfo)
    assertions: tuple[_Assertion, ...] = ()
    ingredients: tuple[_ManifestIngredient, ...] = ()


class _ValidationCheck(Struct, frozen=True, gc=False):
    code: str = ""


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


class CredentialEvidence(Struct, frozen=True, gc=False):
    manifest_id: str
    signer: str
    assertions: int
    ingredients: int
    validation_state: str
    manifest_count: int = 1
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
    ingredient_relationships: tuple[str, ...] = ()

    @classmethod
    def measure(cls, store: _Store, *, embedded: bool, remote_url: str, signer: str) -> Self:
        active = store.manifests.get(store.active_manifest, _Manifest())
        info, results = active.signature_info, store.validation_results
        return cls(
            manifest_id=store.active_manifest,
            signer=signer or info.alg,
            assertions=len(active.assertions),
            ingredients=len(active.ingredients),
            validation_state=store.validation_state or "unsigned",
            manifest_count=len(store.manifests),
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
            ingredient_relationships=tuple(ingredient.relationship for ingredient in active.ingredients),
        )

    @classmethod
    def unsigned(cls, signer: str, /) -> Self:
        return cls(manifest_id="", signer=signer, assertions=0, ingredients=0, validation_state="unsigned", manifest_count=0, sdk_version=sdk_version())


# `Provenance` IS the closed union: the dispatch, the async entry, and every case body fold onto
# one owner (mirroring `exchange/conformance#CONFORMANCE`), the verify `Context` riding each case
# that consumes it — `read_fragment` carries none because `Reader.with_fragment` takes no `context`.
@tagged_union(frozen=True)
class Provenance:
    tag: Literal["sign", "sidecar", "read", "read_fragment"] = tag()
    sign: tuple[Manifest, str, bytes, SignerSpec, Context | None] = case()
    sidecar: tuple[Manifest, str, bytes, SignerSpec, str, Context | None] = case()
    read: tuple[str, bytes, Context | None] = case()
    read_fragment: tuple[str, bytes, bytes] = case()

    @staticmethod
    def Sign(manifest: Manifest, fmt: str, source: bytes, signer: SignerSpec, context: Context | None = None) -> "Provenance":
        return Provenance(sign=(manifest, fmt, source, signer, context))

    @staticmethod
    def Sidecar(manifest: Manifest, fmt: str, source: bytes, signer: SignerSpec, remote_url: str, context: Context | None = None) -> "Provenance":
        return Provenance(sidecar=(manifest, fmt, source, signer, remote_url, context))

    @staticmethod
    def Read(fmt: str, source: bytes, context: Context | None = None) -> "Provenance":
        return Provenance(read=(fmt, source, context))

    @staticmethod
    def ReadFragment(fmt: str, init: bytes, fragment: bytes) -> "Provenance":
        return Provenance(read_fragment=(fmt, init, fragment))

    async def close(self) -> RuntimeRail[tuple[ContentKey, CredentialEvidence]]:
        return await async_boundary(f"credential.{self.tag}", self._emit)

    # retry re-runs ONLY the `_TRANSIENT` network seam (a TSA timestamp or remote-manifest fetch);
    # a non-transient codec/signature fault surfaces immediately and `async_boundary` rails it once.
    @stamina.retry(on=_TRANSIENT, attempts=4, timeout=30.0)
    async def _emit(self) -> tuple[ContentKey, CredentialEvidence]:
        signed, evidence = await to_thread.run_sync(self._run)
        return ContentIdentity.of(f"credential.{self.tag}", signed), evidence

    def _run(self) -> tuple[bytes, CredentialEvidence]:
        match self:
            case Provenance(tag="sign", sign=(manifest, fmt, source, signer, context)):
                return self._signed(manifest, signer, fmt, source, context)
            case Provenance(tag="sidecar", sidecar=(manifest, fmt, source, signer, remote_url, context)):
                return self._signed(manifest, signer, fmt, source, context, remote_url=remote_url)
            case Provenance(tag="read", read=(fmt, source, context)):
                return source, self._evidence(Option.of_optional(Reader.try_create(fmt, BytesIO(source), context=context)), "")
            case Provenance(tag="read_fragment", read_fragment=(fmt, init, fragment)):
                return init, self._evidence(Option.of_optional(Reader.with_fragment(fmt, BytesIO(init), BytesIO(fragment))), "")
            case _ as unreachable:
                assert_never(unreachable)

    def _signed(self, manifest: Manifest, signer: SignerSpec, fmt: str, source: bytes, context: Context | None, /, *, remote_url: str | None = None) -> tuple[bytes, CredentialEvidence]:
        if fmt not in _SIGNABLE:
            raise C2paError.NotSupported(f"credential.sign: {fmt} is read-only here; route pdf/arw/nef to exchange/conformance")
        sink = BytesIO()
        # `sign` returns the detached manifest bytes AND writes the asset into `sink`; the embedded
        # arm validates the manifest bound into the asset, the sidecar arm (a `set_no_embed` asset
        # carries no embedded manifest and its remote store is unpublished at sign time) validates
        # the detached bytes through `manifest_data=` — an embedded-style read would yield `unsigned`.
        detached = manifest.author(context, remote_url=remote_url).sign(signer.cose(), fmt, BytesIO(source), sink)
        signed = sink.getvalue()
        reader = (
            Reader.try_create(fmt, BytesIO(signed), manifest_data=detached, context=context)
            if remote_url is not None
            else Reader.try_create(fmt, BytesIO(signed), context=context)
        )
        return signed, self._evidence(Option.of_optional(reader), signer.alg)

    def _evidence(self, reader: Option[Reader], signer: str, /) -> CredentialEvidence:
        if reader.is_none():
            return CredentialEvidence.unsigned(signer)
        with reader.value as live:
            store = _STORE_DECODER.decode(live.json())
            return CredentialEvidence.measure(store, embedded=live.is_embedded(), remote_url=Option.of_optional(live.get_remote_url()).default_value(""), signer=signer)
```

## [03]-[RESEARCH]

- [C2PA_SIGN_EMBED_SIDECAR] [RESOLVED]: `Builder.from_json(manifest_json, context=None) -> Builder` (the named factory over a string-or-dict definition), `Builder.sign(signer, format, source, dest) -> bytes` (the single signing surface keyed by first-argument shape, single-use and closing after the call; omitting `dest` buffers into a `BytesIO`), `Builder.sign_file(source_path, dest_path, signer=None) -> bytes` (the path row), `Builder.set_no_embed()` (the sidecar/non-embedded toggle), and `Builder.set_remote_url(remote_url)` (the remote-manifest URL) verify against the installed `c2pa 0.35.1` (`c2pa-rs 0.88.0`) and the folder `c2pa-python` `.api` `[03]` `Builder` table. The `Sign` arm drives one `Builder.sign(signer, format, source_stream, dest_stream)` over `BytesIO`; the `Sidecar` arm folds `set_no_embed`/`set_remote_url` so the genuine non-embedded modality is one case rather than the deleted `embed: bool` knob. `Builder.sign(...) -> bytes` returns the detached manifest store, and `Reader.try_create(format_or_path, stream=None, manifest_data=None, context=None)` carries a `manifest_data` parameter (verified on the installed `Reader`/`try_create` signature), so the `Sidecar` arm reads its evidence through `Reader.try_create(fmt, signed_stream, manifest_data=detached, context=)`: a `set_no_embed` asset carries no embedded manifest and its remote store is unpublished at sign time, so an embedded-style `try_create(fmt, signed_stream)` read would resolve no manifest and degrade to `unsigned` evidence — the captured detached bytes are the only valid evidence source for the sidecar verify. `Builder.get_supported_mime_types() -> list[str]` is a `classmethod` (callable at boundary scope) whose 57-entry native signable set excludes `application/pdf`/`arw`/`nef`/`image/x-nikon-nef`/`image/x-sony-arw` (verified against the 63-entry `Reader.get_supported_mime_types()` readable set), so the module-level `_SIGNABLE` preflight `raise C2paError.NotSupported` fast-fails a read-only asset before authoring, the explicit gate the prior prose-only routing assertion lacked. The in-memory `BytesIO` source/dest pair is the asset-stream seam, no intermediate file.
- [C2PA_READ_AND_STORE_DECODE] [RESOLVED]: `Reader.try_create(format_or_path, stream=None, manifest_data=None, context=None) -> Optional[Reader]` (the optional-returning factory mapping `ManifestNotFound` to `None`), `Reader.with_fragment(format, stream, fragment_stream) -> Reader` (the fragmented-BMFF init+fragment read for DASH/CMAF, no `context` parameter), and `Reader.json() -> str` verify against the installed distribution and the folder `.api` `[03]` `Reader` table. The store walk decodes `Reader.json()` ONCE through the module-level `msgspec.json.Decoder(type=_Store)`: the `c2pa.py` `get_active_manifest` source proves the active manifest is `manifests[data["active_manifest"]]` and the manifest dict carries NO `label` of its own (the label is the store-level `active_manifest` KEY), so the prior `convert(get_active_manifest(), _ActiveManifest).label` was the phantom always-`""` read — `manifest_id` is `store.active_manifest`. The one decode also carries `validation_state`, `validation_results.activeManifest.{success,informational,failure}[].code`, `validation_results.ingredientDeltas[].validationDeltas.failure[].code`, every `signature_info` field, and the whole `manifests` chain (`len(manifests)` the lineage depth), so the separate `get_validation_state`/`get_validation_results` accessors and a per-label `get_manifest` loop collapse into this one decode; a sample-store round trip confirms the rename map, the nested-struct + `dict[str, _Manifest]` decode, and the unknown-field drop. `Reader.is_embedded() -> bool` and `Reader.get_remote_url() -> Optional[str]` stay accessor reads (the embedded flag and remote URL are not store-JSON fields), the latter projected through `Option.of_optional`. The `Read` arm projects `CredentialEvidence.unsigned` on `try_create`'s `None`; `ReadFragment` is the genuine fragmented modality, replacing the deleted `Verify` op whose extraction was identical to `Read`.
- [C2PA_SIGNER_AXIS] [RESOLVED]: `Signer.from_info(signer_info: C2paSignerInfo) -> Signer`, `Signer.from_callback(callback, alg: C2paSigningAlg, certs: str, tsa_url=None) -> Signer`, `C2paSignerInfo(alg, sign_cert, private_key, ta_url)`, and the `C2paSigningAlg` `ES256`/`ES384`/`ES512`/`PS256`/`PS384`/`PS512`/`ED25519` rows verify against the installed distribution. The two `SignerSpec` cases bind one `Signer.from_info`/`Signer.from_callback` row each through the shared `@beartype(conf=FAULT_CONF)` contract weave (matching the `exchange/conformance#CONFORMANCE` `_signer` projector), and the `_SIGNING_ALG` `frozendict` resolves the enum ordinal from the `SigningAlg.name` correspondence at boundary scope so no native `IntEnum` value is hardcoded beside the signer call; `Signer.from_callback` keeps private-key material in the `cryptography`/HSM keyring with only `C2paSigningAlg` and the PEM `certs` chain crossing.
- [C2PA_INTENT_SOURCE] [RESOLVED]: `Builder.set_intent(intent: C2paBuilderIntent, digital_source_type: C2paDigitalSourceType = C2paDigitalSourceType.EMPTY)` and `Builder.add_action(action_json)` verify against the folder `.api` `[03]` `Builder` table; `C2paBuilderIntent` (`CREATE`/`EDIT`/`UPDATE`) and the full `C2paDigitalSourceType` vocabulary verify against the installed enum — eighteen non-`EMPTY` members (`DIGITAL_CAPTURE`, `DIGITAL_CREATION`, `ALGORITHMIC_MEDIA`, `TRAINED_ALGORITHMIC_MEDIA`, `TRAINED_ALGORITHMIC_DATA`, `COMPOSITE_SYNTHETIC`, `COMPOSITE_CAPTURE`, `COMPOSITE_WITH_TRAINED_ALGORITHMIC_MEDIA`, `COMPUTATIONAL_CAPTURE`, `DATA_DRIVEN_MEDIA`, `HUMAN_EDITS`, `NEGATIVE_FILM`, `POSITIVE_FILM`, `PRINT`, `SCREEN_CAPTURE`, `VIRTUAL_RECORDING`, `ALGORITHMICALLY_ENHANCED`, `COMPOSITE`), so the prior five-member `DigitalSource` subset under-modeled the capture-and-synthesis provenance taxonomy the verifier reads. `Intent`/`DigitalSource` are `StrEnum`s whose member names match the c2pa enum names, so `_INTENT`/`_DIGITAL_SOURCE`/`_SIGNING_ALG` derive each row through `C2paX[member.name]` — one name correspondence, never the hand-enumerated table that grew row-by-row; a new IPTC source is one `StrEnum` member.
- [C2PA_INGREDIENT_ARCHIVE] [RESOLVED]: `Builder.add_ingredient_from_stream(ingredient_json, format, source)`, `Builder.add_ingredient_from_archive(stream)` (single-arg, rehydrating from a written ingredient archive), `Builder.write_ingredient_archive(ingredient_id, stream)`, and `Builder.add_resource(uri, stream)` verify against the installed `Builder`. The `Ingredient` closed `tagged_union` carries the attach modality as the case the `author` fold matches — `stream` (definition + format + source bytes → `add_ingredient_from_stream`) and `archive` (a `write_ingredient_archive` blob → `add_ingredient_from_archive`) — so a parent processed once is reused across a campaign's derived assets without re-processing, the modality the value carries rather than a `from_archive: bool` knob the body re-derives. Whole-builder `to_archive`/`from_archive` is deliberately unbuilt: the frozen `Manifest` is the reusable authoring template the per-asset `author` re-runs, so only the processed-ingredient reuse the JSON re-author cannot reproduce earns a case.
- [C2PA_TRANSIENT_RETRY] [RESOLVED]: `C2paError` is the base of the nested typed subclass family — `C2paError.RemoteManifest`, `C2paError.Io`, `C2paError.Signature`, `C2paError.NotSupported`, `C2paError.ManifestNotFound`, `C2paError.Verify`, `C2paError.Decoding`/`Encoding`/`Json`/`ResourceNotFound`/`AssertionNotFound`/`FileNotFound` (verified as `Exception` subclasses accessible as `C2paError.<Name>`, not module-level symbols). The `c2pa` runtime deps `cryptography`/`requests` carry the RFC-3161 `ta_url` timestamp and the remote-manifest fetch over the network on the `anyio.to_thread.run_sync` worker, so this owner shares the `exchange/conformance#CONFORMANCE` "crypto-with-network" concern, not a pure-CPU interior — `stamina.retry(on=(C2paError.RemoteManifest, C2paError.Io), attempts=4, timeout=30.0)` weaves the `_emit` core innermost so only the network transients re-attempt (a non-transient `Decoding`/`Signature`/`NotSupported` fault surfaces immediately per the `stamina` selectivity law), the clean attempt returns the `(key, evidence)` pair once, and `async_boundary` converts the residual `C2paError`/contract raise into the runtime `BoundaryFault`; `stamina.retry` decorating an async method verifies against the installed `stamina 26.1.0`.
- [C2PA_CP315_INPROCESS] [RESOLVED]: the `c2pa` wheel is `py3-none-<platform>` with the native `libc2pa_c` bundled per platform and loaded via `ctypes` (`macosx_11_0_arm64` the osx-arm64 wheel), `python_requires >=3.10`; `c2pa.__version__` `0.35.1` over `c2pa-rs 0.88.0` imports clean on cp315 and `c2pa` is NOT on the manifest `tool.ruff.lint.flake8-tidy-imports.banned-module-level-imports` list (only the seven heavy numeric libs are), so module-scope `from c2pa import (...)` binds directly — no `python_version<'3.15'` gate, no `anyio.to_process` subprocess seam — and the `_SIGNABLE`/`_SIGNING_ALG`/`_INTENT`/`_DIGITAL_SOURCE` module constants resolve eagerly off that import; the synchronous SDK calls fold onto `anyio.to_thread.run_sync` so the `async_boundary` rail never blocks the loop while the native core signs.
- [RECEIPT_AND_PERSISTENCE_SEAM] [RESOLVED]: `Provenance.close` returns `RuntimeRail[tuple[ContentKey, CredentialEvidence]]`, mirroring the `exchange/conformance#CONFORMANCE` `close` that returns `RuntimeRail[tuple[ContentKey, ConformanceVerdict]]` — a content-authenticity verification yields the full evidence value, not a four-scalar summary that discards the issuer, signing time, cert serial, revocation status, validation codes, and ingredient lineage the `measure` walk computes. `_emit` carries `@stamina.retry` alone (no `@receipted`, exactly as the conformance sibling carries none), mints `ContentIdentity.of(f"credential.{self.tag}", signed)` over the signed/read bytes, and returns the `(key, evidence)` pair; the consumer (the `core/plan#PLAN` planner or any coordinator holding both owners) mints `ArtifactReceipt.Credential(key, ev.manifest_id, ev.signer, ev.assertions, ev.validation_state)` from the pair — the settled `tuple[ContentKey, str, str, int, str]` case and its `_KEYS` row `("manifest_id", "signer", "assertions", "validation_state")` on the `core/receipt#RECEIPT` owner, carrying the CORRECT label (the store `active_manifest` key) rather than the phantom empty field — exactly as it mints `ArtifactReceipt.Verdict` from the conformance pair, so `receipt.py` imports no `CredentialEvidence` and the `credential` case stays flat scalars. The persistence owner re-derives the same `XxHash128` seed over the identical signed bytes (`csharp:Rasm.Persistence`), so the C2PA manifest and the content key co-identify one durable artifact; the credential re-mints no canonical key (the runtime `content_identity` owns it), the binding a key thread, never a parallel identity.
- [CALLBACK_SIGNER_SEAM] [RESOLVED]: `Signer.from_callback(callback, alg, certs, tsa_url=None)` is the `cryptography`/HSM seam, and the `CallbackSigner.sign: Callable[[bytes], bytes]` field the `callback` case carries returns the raw COSE signature bytes for a digest so private-key material lives in the keyring/HSM and `c2pa` never holds it; the campaign signer-configuration owner constructs the `CallbackSigner` with the concrete keyring/HSM-bound digest-signer at composition, the same campaign-supplied-credential deferral the `exchange/conformance#CONFORMANCE` `ExternalSig` injected-signature path uses. The `cert_key` case is the direct `C2paSignerInfo` cert/key path needing no external callback.

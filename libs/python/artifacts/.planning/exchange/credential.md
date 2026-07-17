# [PY_ARTIFACTS_CREDENTIAL]

`Provenance` is the C2PA content-credential owner at the exchange boundary — ONE closed `tagged_union` binding a signed tamper-evident manifest into an emitted artifact and reading it back, discriminating `Sign`/`Read`/`ReadFragment`/`Embed`/`ArchiveIngredient` by payload shape and owning the dispatch, the async entry, and every case body directly. `SignerSpec` is the one signer policy union — a `CertKeySigner` feeding `Signer.from_info` and a `CallbackSigner` feeding `Signer.from_callback` whose `ed25519` factory binds the in-process `ed25519_sign` primitive for the common no-HSM case, the signing algorithm a `SigningAlg` `StrEnum` whose `C2paSigningAlg` row derives by name. `CredentialPolicy` is the admitted trust/TSA value every operation carries; each `_run` arm materializes one `Context` inside a deterministic `with` window, and `_signed` brackets the single-use `Builder` and `Signer` together so no native handle enters a spec or crosses the result rail. `CredentialEvidence` is the rich provenance receipt one `Reader.json()` manifest-STORE decode folds — signer chain, validation codes, the declared `c2pa.actions` edit/AI-source history, per-ingredient lineage, and the bound thumbnail `resources` extracted through `Reader.resource_to_stream` (the forensic evidence a DAM consumer walks). `Manifest.with_parents` lowers `core/plan#PLAN` `ArtifactWork.parents` content keys into `parentOf` ingredient rows, so a composed sheet attests the charts and renders it derives from by the same identities the pipeline dedups on.

Native signing and reading cross the caller-threaded `LanePolicy.offload` seam as `KernelTrait.RELEASING` kernels and drive the RFC-3161 `ta_url` timestamp and remote-manifest transport there; the `@stamina.retry(on=_TRANSIENT)` weave on `_run` re-attempts ONLY the `C2paError.RemoteManifest`/`Io` network subset, so a codec/signature fault surfaces immediately — the same crypto-with-network concern the `exchange/conformance#CONFORMANCE` PAdES sibling closes for PDF (PDF/raw-camera route there, gated out of the `_SIGNABLE` set). `close(lane)` is the rich public egress — `RuntimeRail[tuple[ContentKey, bytes, CredentialEvidence]]`, the signed/read bytes keyed by `ContentIdentity.key` over their OWN content so the manifest co-identifies the durable artifact with the `csharp:Rasm.Persistence` `XxHash128` key — and `emit(lane)` is the pipeline node whose `_emit` projects the triple onto `core/receipt#RECEIPT` `ArtifactReceipt.Credential`'s four settled scalars. `CredentialSettings` (`pydantic-settings`, `RASM_CREDENTIAL_` prefix) admits the trust-list + verify policy ONCE into `CredentialPolicy`, so `validation_state`/`validation_results` anchor against a real trust-anchor set without storing a package-native `Context` on an interior request.

## [01]-[INDEX]

- [01]-[CREDENTIAL]: the C2PA content-credential owner that IS the closed `Provenance` union (`Sign`/`Read`/`ReadFragment`/`Embed`/`ArchiveIngredient`) over the `SignerSpec` cert/callback policy and the `CredentialEvidence` receipt one `Reader.json()` STORE decode folds, egressing the `(ContentKey, bytes, CredentialEvidence)` triple through `close` and projecting `ArtifactReceipt.Credential` through `emit`.

## [02]-[CREDENTIAL]

- Owner: `Provenance` IS the closed union owning the dispatch, the async entry, and every case body directly — never a one-field wrapper over a separate op union, never a `from_file`/`from_stream`/per-format reader family, never a per-algorithm signer type, never a `sign`/`verify`/`read` method triple. `SignerSpec` reads its tagged case directly through the `@beartype(conf=FAULT_CONF)`-woven `_cose()`/`alg` arms, never parallel nullable cert fields; no private-key material is minted here — the `CallbackSigner` case keeps it in the `cryptography` keyring/HSM, only `C2paSigningAlg` plus the PEM cert chain crossing the seam. `CredentialPolicy` is the trusted frozen policy projected from `CredentialSettings`; each request stores that value instead of a live `Context`. One coupled `(Intent, DigitalSource | None)` field over the full eighteen-member IPTC vocabulary rides `Builder.set_intent`, never two parallel nullable fields that orphan a `DigitalSource` from its `Intent`.
- Cases: `Provenance` rows — `Sign` (`Manifest._author` → `Builder.sign` into an in-memory `BytesIO`, single-use; a present `remote_url` folds `set_no_embed`/`set_remote_url` so the manifest references a remote store — the embed-vs-sidecar modality the value discriminates, never an `embed: bool` knob) · `Read` (`Reader.try_create` → `Reader.json` STORE decode, `ManifestNotFound`→`None` to `unsigned`) · `ReadFragment` (`Reader.try_create` over the init segment chained through the instance `with_fragment`, the fragmented-BMFF read) · `Embed` (`format_embeddable(fmt, manifest)` rewrapping a captured detached/remote-store manifest into the embeddable JUMBF block, `_SIGNABLE`-gated — this owner produces the block a downstream writer splices, never re-authoring the asset) · `ArchiveIngredient` (`add_ingredient_from_stream` → `write_ingredient_archive` minting the processed-ingredient blob the `Ingredient.Archive` case rehydrates, so a parent processed once is serialized in-system rather than hand-carried from a foreign tool). `SignerSpec` — `cert_key` (`CertKeySigner`) · `callback` (`CallbackSigner`, the `ed25519` factory binding `ed25519_sign` for the no-HSM path). `Ingredient` — `stream` (→ `add_ingredient_from_stream`) · `archive` (a processed-ingredient blob → `add_ingredient_from_archive`), the attach modality the `_author` kernel matches so a parent processed once is reused across a campaign's derived assets; `Manifest.with_parents` derives the plan-lineage `stream` rows from `(ContentKey, fmt, bytes)` parents in one fold.
- Entry: `emit(lane, parents=...)` returns the `ArtifactWork` node under the PRE-RUN `_key` (the msgpack input canon `keyed` admission elides on); `close(lane)` resolves the `(ContentKey, bytes, CredentialEvidence)` triple — `ContentIdentity.key` over the signed/read bytes plus the rich evidence — never a four-scalar receipt that discards the forensic fields or the signed payload; `_emit` composes `close` and mints `ArtifactReceipt.Credential(key, ev.manifest_id, ev.signer, ev.assertions, ev.validation_state)` exactly as the conformance sibling mints `ArtifactReceipt.Verdict` from its triple. Asset bytes arrive as an in-memory `BytesIO` and leave signed with no intermediate file — `sign_file` is the path convenience only. `c2pa`'s in-process ctypes core needs no process worker, distinct from the `HOSTILE`-crossing `exchange/detect#DETECT`/`exchange/metadata#METADATA` siblings, so the kernel rides `KernelTrait.RELEASING` with no worker-death retry row.
- Auto: `Manifest._author` is the foreign mutation kernel — `Builder.from_json`, `set_intent` (the `DigitalSource` through `_DIGITAL_SOURCE`, defaulting `EMPTY`), `set_no_embed`/`set_remote_url` on a sidecar URL, the `add_action` sequence, each `Ingredient` arm, and the `add_resource` attachments mutate one SDK builder because those package methods return `None`. `SignerSpec._cose` builds the signer row while the signer-free `alg` accessor mints no native handle. `Sign` `_SIGNABLE`-preflights `fmt` and raises `C2paError.NotSupported` for a read-only asset, brackets the `Context`/`Builder`/`Signer`, drives `Builder.sign` into a fresh `BytesIO` capturing the detached manifest bytes, and reads evidence back — the sidecar path (a `set_no_embed` asset carrying no embedded manifest) validates the detached bytes through `Reader.try_create(..., manifest_data=...)` so the read never degrades to bogus `unsigned`, the embedded path passing `manifest_data=None`. `Read`/`ReadFragment` admit `Reader.json()` ONCE through `_STORE_DECODER` into the typed `_Store` — the active-manifest LABEL is the store's `active_manifest` KEY and the content is `manifests[active_manifest]` (the manifest dict carries no `label`), so a `.get("label")` is the phantom always-`""` read the decode replaces, and the one decode carries `validation_state`/`validation_results`/`ingredientDeltas`/the whole chain, never per-accessor calls. `CredentialEvidence.measure` folds every fact into one row in a single store walk. `_drawn` writes the claim + ingredient thumbnails through `resource_to_stream` into the evidence `resources` band on the same live-reader borrow, each `catch`-narrowed to `Nothing` so a missing/broken resource skips by omission. `_opened` folds both read-modality absences onto one `Option[Reader]` — `try_create`'s `None` and the raised `ManifestNotFound` — projecting `unsigned`. Each case materializes its verify `Context` from `CredentialPolicy` inside the `_run` lifetime window, `FragmentSpec` included.
- Receipt: `_emit` mints `core/receipt#RECEIPT` `ArtifactReceipt.Credential(key, ev.manifest_id, ev.signer, ev.assertions, ev.validation_state)` from the `close` triple — the `resources` bytes and every rich field staying on the evidence value object, never reaching the wire — so `receipt.py` imports no `CredentialEvidence` and the flat case is a projection of the triple, never the sole egress. `validation_state` plus the valid/invalid evidence is the observable provenance the `observability/metrics` `MeterProvider` stream reads off the minted receipt.
- Packages: `c2pa-python` (native `c2pa-rs` core): `Builder.from_json`/`set_intent`/`set_no_embed`/`set_remote_url`/`add_action`/`add_ingredient_from_stream`/`add_ingredient_from_archive`/`write_ingredient_archive`/`add_resource`/`sign`/`get_supported_mime_types` the authoring+signing surface; `Reader.try_create`/`with_fragment` (an instance method chaining on a constructed reader)/`json`/`is_embedded`/`get_remote_url`/`resource_to_stream` the extraction surface; `Signer.from_info`/`from_callback` the two signer seams; `C2paSignerInfo`/`C2paSigningAlg`/`C2paBuilderIntent`/`C2paDigitalSourceType` the ctypes value + `IntEnum` vocabularies the name-correspondence tables project onto; `C2paError` the typed subclass family the `_TRANSIENT` subset and the `_drawn`/`_opened` traps read; `Context.from_dict` the per-operation verify-policy materialization; `sdk_version` the core-version scalar; the `c2pa.c2pa` inner primitives `format_embeddable` (the `Embed` rewrap) + `ed25519_sign` (the no-HSM signer). Substrate: `msgspec` (`Struct(frozen=True, gc=False)`, `Raw` the opaque assertion `data`, `json.Decoder` the one `_Store`/`_ActionData` decode, `json.encode`/`msgpack.encode` the `_key` canon, `structs.replace` the `Manifest` transitions); `expression` (`tagged_union`, `Option`/`Nothing`/`of_optional`, `extra.result.catch`); `beartype`; `stamina` (the `_TRANSIENT` network-retry weave); `functools.partial` (the `ed25519_sign` key bind and the `emit` work bind); `builtins.frozendict`; `pydantic-settings` (the `CredentialSettings` env owner); `pathlib.Path`; runtime (`identity.ContentIdentity.key`/`ContentKey`, `faults.FAULT_CONF`/`RuntimeRail`, `lanes.LanePolicy`, `workers.Kernel`/`KernelTrait`); core (`plan.ArtifactWork`/`Admission`, `receipt.ArtifactReceipt`).
- Growth: a new operation is one `Provenance` case with its payload and one `match` arm (the `Embed` case is exactly this); a new signer seam one `SignerSpec` case with one `_cose`/`alg` arm, an in-lane convenience one `CallbackSigner` factory (the `ed25519` factory needing no new case); a new signing algorithm, intent, or digital-source origin one `SigningAlg`/`Intent`/`DigitalSource` member the c2pa row derives by name; a new ingredient modality one `Ingredient` case and one `_author` arm; a new lineage source one `with_parents` caller, the fold untouched; a new assertion one `ActionDefinition` row; a new evidence fact one `CredentialEvidence` field (the thumbnail `resources` band is exactly this), and when contract-settled one scalar on the shared receipt case; a new readable store field one field on the `_Store`/`_Manifest`/`_ResourceRef` boundary struct; a new verify/trust knob one field on `CredentialSettings` projected to `CredentialPolicy` plus one `Context.from_dict` key; a transient fault widens `_TRANSIENT`; zero new surface. A whole-builder archive is deliberately unbuilt — the frozen `Manifest` value is the reusable template, so only the processed-ingredient archive (which the JSON re-author cannot cheaply reproduce) earns an `Ingredient` case, and the `ArchiveIngredient` op is its mint half so the blob never arrives hand-carried.
- Boundary: this owner binds content credentials over already-emitted bytes and owns no artifact production, mints no certificate or key (the campaign signer config supplies cert/key/TSA, the `CallbackSigner` case keeping private-key material in the `cryptography` keyring/HSM), and routes a PDF or raw-camera (`arw`/`nef`) asset to the `exchange/conformance#CONFORMANCE` `pyhanko` PAdES rail rather than `Builder.sign` (read-only here, proven by `_SIGNABLE`). Active-manifest labelling is the store's `active_manifest` KEY, never a phantom `label` field the SDK's manifest dict carries. `C2paSignerInfo` demands `ta_url` as `str`/`bytes` and rejects `None`, so the no-TSA absence projects to `""` at the `from_info` seam. `ArtifactReceipt.Credential`'s case-tuple lands on the same-domain `core/receipt#RECEIPT` owner; `_emit` projects the four scalars from the `close` triple so `receipt.py` imports no producer value object. A signed artifact's `ContentKey` is the wire to `csharp:Rasm.Persistence` — the `close`-minted key decodes the `XxHash128` seed the persistence owner re-derives over the same signed bytes, so the manifest and the content key co-identify one durable artifact across the language boundary. `_key`'s input canon covers verb, format, remote URL, the public signer facet (algorithm plus certificate chain), manifest definition + actions + intent, ingredient and resource payloads, and asset bytes; private signer material and `CredentialPolicy` are non-identity (secret/environment) — the produced bytes re-key at `close`.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
from collections.abc import Callable
from enum import StrEnum
from functools import partial
from io import BytesIO
from pathlib import Path
from typing import Final, Literal, NotRequired, ReadOnly, Required, Self, TypedDict, assert_never

import stamina
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
from c2pa.c2pa import ed25519_sign, format_embeddable
from expression import Nothing, Option, case, tag, tagged_union
from expression.collections import Map
from expression.extra.result import catch
from msgspec import Raw, Struct, field, json, msgpack, structs
from pydantic_settings import BaseSettings, SettingsConfigDict

from rasm.artifacts.core.plan import Admission, ArtifactWork
from rasm.artifacts.core.receipt import ArtifactReceipt
from rasm.runtime.faults import FAULT_CONF, RuntimeRail
from rasm.runtime.identity import ContentIdentity, ContentKey
from rasm.runtime.lanes import LanePolicy
from rasm.runtime.workers import Kernel, KernelTrait


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


# full IPTC digital-source vocabulary `C2paDigitalSourceType` carries (EMPTY is the no-source
# default the `_author` kernel supplies); member NAMES align with the c2pa enum so the row derives.
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


# C2PA manifest/action/ingredient authoring JSON the SDK consumes; typed `ReadOnly`
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
# signable MIME set the `Sign` preflight gates on, the network-transient C2PA fault subset the stamina
# weave re-attempts, and the `c2pa.actions` label whose `data` the evidence walk decodes the action history from.
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

    def with_parents(self, *parents: tuple[ContentKey, str, bytes]) -> Self:
        # plan-derived lineage: each `ArtifactWork.parents` key lands as a `parentOf` ingredient carrying the
        # content identity the pipeline dedups on, so a derived artifact attests its sources by key.
        rows = tuple(
            Ingredient.Stream(
                IngredientDefinition(relationship="parentOf", document_id=f"xmp.did:{key.hex}", instance_id=f"xmp.iid:{key.hex}"),
                fmt,
                source,
            )
            for key, fmt, source in parents
        )
        return structs.replace(self, ingredients=(*self.ingredients, *rows))

    def with_resource(self, resource: Resource, /) -> Self:
        return structs.replace(self, resources=(*self.resources, resource))

    def _author(self, context: Context, /, *, remote_url: str | None = None) -> Builder:
        # Exemption: c2pa's `Builder` authoring methods mutate one native handle and return `None`.
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


class CredentialPolicy(Struct, frozen=True):
    trust_anchors: str = ""
    trust_config: str = ""
    allowed_list: str = ""
    ta_url: str = ""
    verify_trust: bool = True
    verify_timestamp_trust: bool = True
    remote_manifest_fetch: bool = True
    ocsp_fetch: bool = False

    def context(self) -> Context:
        trust = {
            key: value
            for key, value in (
                ("trust_anchors", self.trust_anchors),
                ("trust_config", self.trust_config),
                ("allowed_list", self.allowed_list),
            )
            if value
        }
        return Context.from_dict({
            "verify": {
                "verify_trust": self.verify_trust,
                "verify_timestamp_trust": self.verify_timestamp_trust,
                "remote_manifest_fetch": self.remote_manifest_fetch,
                "ocsp_fetch": self.ocsp_fetch,
            },
            **({"trust": trust} if trust else {}),
        })


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
        # in-process Ed25519 no-HSM digest-signer: `ed25519_sign` binds the PEM `private_key` as the raw
        # COSE-signature callback, so the ED25519 arm signs self-contained without an external keyring/HSM.
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
    def _cose(self, policy: CredentialPolicy, /) -> Signer:
        match self:
            case SignerSpec(tag="cert_key", cert_key=CertKeySigner() as s):
                return Signer.from_info(
                    C2paSignerInfo(
                        alg=_SIGNING_ALG[s.alg], sign_cert=s.sign_cert, private_key=s.private_key, ta_url=s.ta_url or policy.ta_url
                    )
                )
            case SignerSpec(tag="callback", callback=CallbackSigner() as s):
                return Signer.from_callback(s.sign, _SIGNING_ALG[s.alg], s.certs, s.ta_url or policy.ta_url or None)
            case _ as unreachable:
                assert_never(unreachable)


# typed projection of the `Reader.json()` STORE, admitted once through one `msgspec.json.Decoder`;
# unknown keys fall away and every field defaults. The active-manifest LABEL is the store's `active_manifest`
# KEY (NOT a field inside the manifest dict), so `manifests[active_manifest]` is the content the walk reads.
class _SignatureInfo(Struct, frozen=True, gc=False):
    alg: str = ""
    issuer: str = ""
    cert_serial_number: str = ""
    time: str = ""
    revocation_status: str = ""


class _ValidationCheck(Struct, frozen=True, gc=False):
    code: str = ""


# structured `claim_generator_info` row (name+version per signing tool) the modern manifest
# carries beside the legacy flat `claim_generator` string; one value object serves decode and receipt.
class GeneratorRef(Struct, frozen=True, gc=False):
    name: str = ""
    version: str = ""


# one declared `c2pa.actions` entry: the edit/creation verb plus its per-action `digitalSourceType`
# AI-origin token, read back symmetrically rather than collapsed to an assertion-label count.
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


# `_ResourceRef` is the decode leaf every thumbnail/resource reference in the store JSON carries — the
# `format` MIME plus the `identifier` JUMBF URI `Reader.resource_to_stream` resolves to the bound bytes.
class _ResourceRef(Struct, frozen=True, gc=False):
    format: str = ""
    identifier: str = ""


# per-ingredient lineage edge: the decode element `_Manifest.ingredients` admits AND the public
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
    # bound thumbnails extracted through `resource_to_stream`, keyed by JUMBF identifier; dropped at the
    # four-scalar receipt projection so the raw bytes never reach the wire.
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


# --- [SERVICES] -------------------------------------------------------------------------
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

    def policy(self) -> CredentialPolicy:
        read = lambda path: path.read_text(encoding="utf-8") if path is not None else ""
        return CredentialPolicy(
            trust_anchors=read(self.trust_anchors),
            trust_config=read(self.trust_config),
            allowed_list=read(self.allowed_list),
            ta_url=self.ta_url,
            verify_trust=self.verify_trust,
            verify_timestamp_trust=self.verify_timestamp_trust,
            remote_manifest_fetch=self.remote_manifest_fetch,
            ocsp_fetch=self.ocsp_fetch,
        )


_POLICY: Final[CredentialPolicy] = CredentialSettings().policy()


# per-case request payloads carry the canonical trust policy, never a package-native `Context` handle.
class SignSpec(Struct, frozen=True):
    manifest: Manifest
    fmt: str
    signer: SignerSpec
    remote_url: str | None = None
    policy: CredentialPolicy = _POLICY


class ReadSpec(Struct, frozen=True):
    fmt: str
    policy: CredentialPolicy = _POLICY


class FragmentSpec(Struct, frozen=True):
    fmt: str
    fragment: bytes
    policy: CredentialPolicy = _POLICY


class EmbedSpec(Struct, frozen=True):
    fmt: str
    manifest: bytes
    policy: CredentialPolicy = _POLICY


# Processed-ingredient mint: `definition` carries the `instance_id`/label `write_ingredient_archive` keys on,
# so the archive blob `Ingredient.Archive` rehydrates is minted in-system, never hand-carried from a foreign tool.
class ArchiveSpec(Struct, frozen=True):
    definition: IngredientDefinition
    fmt: str
    policy: CredentialPolicy = _POLICY


# `Provenance` IS the closed union: dispatch, async entry, and every case body on one owner, each case a
# `(bytes, Spec)` pair whose asset bytes lead and whose typed spec carries the rest.
@tagged_union(frozen=True)
class Provenance:
    tag: Literal["sign", "read", "read_fragment", "embed", "archive_ingredient"] = tag()
    sign: tuple[bytes, SignSpec] = case()
    read: tuple[bytes, ReadSpec] = case()
    read_fragment: tuple[bytes, FragmentSpec] = case()
    embed: tuple[bytes, EmbedSpec] = case()
    archive_ingredient: tuple[bytes, ArchiveSpec] = case()

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

    @classmethod
    def ArchiveIngredient(cls, source: bytes, spec: ArchiveSpec, /) -> Self:
        return cls(archive_ingredient=(source, spec))

    def emit(self, lane: LanePolicy, /, *, parents: tuple[tuple[ContentKey, str, bytes], ...] = ()) -> ArtifactWork:
        # ONE lineage source: the sign arm folds the parent triples through `Manifest.with_parents` so the signed
        # `parentOf` ingredient rows and the plan's DATA edges derive from the same `(key, fmt, bytes)` values —
        # never a free-floating key list beside an unrelated manifest; only the sign arm carries lineage, so a
        # parented non-sign emit refuses before any work item constructs rather than minting orphan DATA edges.
        if parents and self.tag != "sign":
            raise ValueError(f"credential.{self.tag} carries no lineage; parent edges ride the sign arm alone")
        node = self._with_lineage(parents) if parents else self
        return ArtifactWork(
            key=node._key, work=partial(node._emit, lane), parents=tuple(key for key, _, _ in parents), admission=Admission(keyed=None), cost=1.0
        )

    def _with_lineage(self, parents: tuple[tuple[ContentKey, str, bytes], ...], /) -> "Provenance":
        asset, spec = self.sign
        return Provenance.Sign(asset, structs.replace(spec, manifest=spec.manifest.with_parents(*parents)))

    @property
    def _key(self) -> ContentKey:
        # key-over-INPUT minted PRE-RUN so `keyed` admission elides a duplicate; signer key material and the
        # verify policy are non-identity (secret/environment) — every other spec member rides the arm's canon,
        # ingredients and resources included, so two signs differing only in lineage never dedup-elide.
        return ContentIdentity.key(f"credential.{self.tag}", msgpack.encode(self._canon()))

    def _canon(self) -> tuple[object, ...]:
        match self:
            case Provenance(tag="sign", sign=(asset, spec)):
                manifest = spec.manifest
                template = json.encode((dict(manifest.definition), [dict(action) for action in manifest.actions]))
                intent = (manifest.intent[0].value, manifest.intent[1].value if manifest.intent[1] is not None else "") if manifest.intent else ()
                lineage = tuple(ingredient.stream if ingredient.tag == "stream" else ingredient.archive for ingredient in manifest.ingredients)
                bound = tuple((resource.uri, resource.source) for resource in manifest.resources)
                # PUBLIC signer identity — algorithm plus certificate-chain bytes/PEM, never private-key material —
                # so signer-distinct signs never share a keyed admission identity.
                signer = (
                    (spec.signer.alg.value, spec.signer.cert_key.sign_cert)
                    if spec.signer.tag == "cert_key"
                    else (spec.signer.alg.value, spec.signer.callback.certs)
                )
                return (self.tag, spec.fmt, spec.remote_url or "", signer, template, intent, lineage, bound, asset)
            case Provenance(tag="read", read=(asset, spec)):
                return (self.tag, spec.fmt, asset)
            case Provenance(tag="read_fragment", read_fragment=(init, spec)):
                return (self.tag, spec.fmt, init, spec.fragment)
            case Provenance(tag="embed", embed=(asset, spec)):
                return (self.tag, spec.fmt, spec.manifest, asset)
            case Provenance(tag="archive_ingredient", archive_ingredient=(source, spec)):
                return (self.tag, spec.fmt, json.encode(dict(spec.definition)), source)
            case _ as unreachable:
                assert_never(unreachable)

    async def close(self, lane: LanePolicy, /) -> RuntimeRail[tuple[ContentKey, bytes, CredentialEvidence]]:
        # GIL-releasing c2pa core and the TSA/remote-manifest transport cross the THREAD lane; the
        # produced bytes key by their own content so the manifest and the key co-identify one artifact.
        railed = await lane.offload(Kernel.of(self._run, KernelTrait.RELEASING))
        return railed.map(lambda pair: (ContentIdentity.key(f"credential.{self.tag}", pair[0]), pair[0], pair[1]))

    async def _emit(self, lane: LanePolicy, /) -> RuntimeRail[ArtifactReceipt]:
        return (await self.close(lane)).map(
            lambda kbe: ArtifactReceipt.Credential(kbe[0], kbe[2].manifest_id, kbe[2].signer, kbe[2].assertions, kbe[2].validation_state)
        )

    @stamina.retry(on=_TRANSIENT, attempts=3)
    def _run(self) -> tuple[bytes, CredentialEvidence]:
        # Exemption: each c2pa native handle closes inside the operation that consumes it.
        match self:
            case Provenance(tag="sign", sign=(asset, spec)):
                with spec.policy.context() as context:
                    return self._signed(asset, spec, context)
            case Provenance(tag="read", read=(asset, spec)):
                with spec.policy.context() as context:
                    return asset, self._evidence(self._opened(lambda: Reader.try_create(spec.fmt, BytesIO(asset), context=context)), "")
            case Provenance(tag="read_fragment", read_fragment=(init, spec)):
                with spec.policy.context() as context:
                    opened = self._opened(
                        lambda: (made := Reader.try_create(spec.fmt, BytesIO(init), context=context))
                        and made.with_fragment(spec.fmt, BytesIO(init), BytesIO(spec.fragment))
                    )
                    return init, self._evidence(opened, "")
            case Provenance(tag="embed", embed=(asset, spec)):
                with spec.policy.context() as context:
                    return self._embedded(asset, spec, context)
            case Provenance(tag="archive_ingredient", archive_ingredient=(source, spec)):
                with spec.policy.context() as context:
                    return self._archived(source, spec, context)
            case _ as unreachable:
                assert_never(unreachable)

    def _signed(self, asset: bytes, spec: SignSpec, context: Context, /) -> tuple[bytes, CredentialEvidence]:
        if spec.fmt not in _SIGNABLE:
            raise C2paError.NotSupported(f"credential.sign: {spec.fmt} is read-only here; route pdf/arw/nef to exchange/conformance")
        sink = BytesIO()
        # `sign` returns the detached manifest bytes AND writes the asset into `sink`; the sidecar path
        # (a `set_no_embed` asset carries no embedded manifest, its remote store unpublished at sign time)
        # reads its evidence through `manifest_data=detached` where an embedded-style read would yield
        # `unsigned`, while the embedded path passes `manifest_data=None` — one `try_create` either way.
        with spec.manifest._author(context, remote_url=spec.remote_url) as builder, spec.signer._cose(spec.policy) as signer:
            detached = builder.sign(signer, spec.fmt, BytesIO(asset), sink)
        signed = sink.getvalue()
        reader = self._opened(
            lambda: Reader.try_create(spec.fmt, BytesIO(signed), manifest_data=detached if spec.remote_url is not None else None, context=context)
        )
        return signed, self._evidence(reader, spec.signer.alg)

    def _embedded(self, asset: bytes, spec: EmbedSpec, context: Context, /) -> tuple[bytes, CredentialEvidence]:
        # rewrap a captured detached/remote-store manifest into the embeddable JUMBF block; this owner produces the
        # block a downstream writer splices, reading evidence through the sidecar `manifest_data` path.
        if spec.fmt not in _SIGNABLE:
            raise C2paError.NotSupported(f"credential.embed: {spec.fmt} has no embeddable manifest form; route pdf/arw/nef to exchange/conformance")
        _size, block = format_embeddable(spec.fmt, spec.manifest)
        reader = self._opened(lambda: Reader.try_create(spec.fmt, BytesIO(asset), manifest_data=spec.manifest, context=context))
        return block, self._evidence(reader, "")

    def _archived(self, source: bytes, spec: ArchiveSpec, context: Context, /) -> tuple[bytes, CredentialEvidence]:
        # mint the processed-ingredient archive `Ingredient.Archive` rehydrates: one throwaway builder ingests the
        # source and `write_ingredient_archive` serializes it keyed by the definition's `instance_id`/label.
        sink = BytesIO()
        with Builder.from_json({}, context=context) as builder:
            builder.add_ingredient_from_stream(spec.definition, spec.fmt, BytesIO(source))
            builder.write_ingredient_archive(str(spec.definition.get("instance_id") or spec.definition.get("title", "")), sink)
        return sink.getvalue(), CredentialEvidence.unsigned("")

    @staticmethod
    def _opened(make: Callable[[], Reader | None], /) -> Option[Reader]:
        # `try_create` maps a credential-free asset to `None`; a fragment read raises `ManifestNotFound` instead — both project to `unsigned`.
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
        # best-effort forensic capture of the bound thumbnails through `resource_to_stream` on the same
        # live-reader borrow; a missing/broken resource skips via the `catch`-narrowed `Option`, never faulting.
        active = store.manifests.get(store.active_manifest, _Manifest())
        refs = (active.thumbnail, *(ingredient.thumbnail for ingredient in active.ingredients))

        def drawn(uri: str, /) -> Option[bytes]:
            sink = BytesIO()
            return catch(exception=C2paError)(live.resource_to_stream)(uri, sink).map(lambda _n: sink.getvalue()).to_option()

        return frozendict({ref.identifier: got.value for ref in refs if ref.identifier and (got := drawn(ref.identifier)).is_some()})
```

## [03]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)

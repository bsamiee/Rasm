# [PY_ARTIFACTS_CREDENTIAL]

The C2PA content-credential owner at the exchange boundary. `Provenance` is ONE owner over `c2pa-python` binding a signed tamper-evident manifest into an emitted artifact and reading it back — the closed `tagged_union` owning the dispatch, the async entry, AND every case body directly, discriminating its `Sign`/`Read`/`ReadFragment` cases by payload shape (the `Sign` arm folding the optional sidecar `remote_url` rather than a parallel case) — never a one-field wrapper over a separate op union, never a `from_file`/`from_stream`/`per-format` reader family, never a per-signing-algorithm signer type, and never a per-output `sign`/`verify`/`read` method triple. `SignerSpec` is the ONE policy union carrying one typed signer case — a `CertKeySigner` (`alg`, `sign_cert`, `private_key`, `ta_url`) feeding `Signer.from_info` and a `CallbackSigner` (`alg`, `certs`, `sign`, `ta_url`) whose campaign-supplied `sign` digest-callback feeds `Signer.from_callback` — so the `cryptography`/HSM seam is a tagged case carrying the keyring/HSM-bound signer as a typed field, the signing algorithm a `SigningAlg` `StrEnum` whose `C2paSigningAlg` row derives by name correspondence across ES/PS/ED25519, never a parallel signer owner or a free-text alg field. `CredentialEvidence` is the ONE rich provenance receipt a single `Reader.json()` manifest-STORE decode folds — the active-manifest label read off the store's `active_manifest` KEY (never a phantom `label` field inside the manifest dict the SDK never emits), the `manifests` chain count, signer alg, signature `issuer`/`time`/`cert_serial_number`/`revocation_status`, `claim_generator`/`title`/`format`/`instance_id`, embedded flag, remote URL, SDK version, assertion labels, ingredient count, and the `validation_state` plus the `validation_results` failure/success/informational codes, the per-ingredient `ingredientDeltas` failure codes, AND the per-ingredient lineage (each ingredient's `relationship`, `title`, `format`, and `instance_id`) — returned whole as the `(ContentKey, CredentialEvidence)` pair (mirroring the `exchange/conformance#CONFORMANCE` verdict close) the consumer projects onto `core/receipt#RECEIPT` `ArtifactReceipt.Credential` as its four settled scalars, keying the signed buffer through `ContentIdentity.of` so the binding co-identifies the durable artifact with the `csharp:Rasm.Persistence` `XxHash128` content key. The native core signs and reads on an `anyio.to_thread.run_sync` worker and drives the RFC-3161 `ta_url` timestamp and remote-manifest `requests` transport there, so `stamina.retry` over the transient `C2paError.RemoteManifest`/`Io` subset re-attempts only the network seam — the same crypto-with-network concern the `exchange/conformance#CONFORMANCE` PAdES sibling closes for PDF (PDF/raw-camera are read-only here, gated out by `Builder.get_supported_mime_types`, signed on the `pyhanko` rail); this owner is cross-format content-authenticity provenance over the image/BMFF/audio signable set, owning no artifact production, only the credential close over already-emitted bytes.

## [01]-[INDEX]

- [01]-[CREDENTIAL]: C2PA manifest authoring (the full IPTC `DigitalSource` intent vocabulary, the `Ingredient` stream-or-archive attach modality) + single-use embed/sidecar signing format-gated on `Builder.get_supported_mime_types`, manifest-STORE extraction folding one `Reader.json()` decode through `msgspec.json.Decoder` into the typed `_Store` (the active-manifest label off the `active_manifest` key, the `manifests` chain, `validation_state`/`validation_results`), fragmented-BMFF reading over `Reader.with_fragment`, the typed `SignSpec`/`ReadSpec`/`FragmentSpec` per-case request payloads and the `SignerSpec` cert/callback policy union, the rich `CredentialEvidence` provenance receipt (signer chain, validation codes, structured `claim_generator_info`, and the declared `c2pa.actions` edit/AI-source history) returned as the `(ContentKey, CredentialEvidence)` pair, the transient-only `stamina.retry` network weave, and the content-key binding the consumer projects onto the four-scalar `core/receipt#RECEIPT` `ArtifactReceipt.Credential` case and the `csharp:Rasm.Persistence` content key.

## [02]-[CREDENTIAL]

- Cases: `Provenance` rows `Sign` (`Manifest.author` → `Builder.sign` into an in-memory `BytesIO`, single-use, the builder closes after `sign`; a present `remote_url` folds `Builder.set_no_embed`/`set_remote_url` so the signed manifest references a remote store rather than embedding — the embed-vs-sidecar modality the `remote_url` value discriminates, never a parallel case nor an `embed: bool` knob) · `Read` (`Reader.try_create` → `Reader.json` STORE decode, the optional-returning extraction mapping `ManifestNotFound`→`None` to the `unsigned` evidence) · `ReadFragment` (`Reader.with_fragment` init+fragment, the fragmented-BMFF read) — each binding the one `Builder`/`Reader` surface keyed by argument shape rather than a parallel type; `SignerSpec` cases `cert_key` (`CertKeySigner`) · `callback` (`CallbackSigner`) — one frozen signer-struct per seam whose fields are the named crypto axes the `match` arm reads directly, the `alg` axis a `SigningAlg` `StrEnum` member, so a per-algorithm signer type and a positional cert tuple are the deleted forms and no private-key material is minted here; `Ingredient` cases `stream` (`ManifestDefinition`-style definition + format + source bytes → `add_ingredient_from_stream`) · `archive` (a `write_ingredient_archive` blob → `add_ingredient_from_archive`) — the attach modality is the case the `author` fold matches, never a `from_archive: bool` knob, so a parent processed once is reused across a campaign's derived assets. The intent axis is one coupled `Manifest.intent` field — an `(Intent, DigitalSource | None)` pair, never two parallel nullable fields where a `DigitalSource` could orphan without an `Intent` to carry it — over `Intent` (`CREATE`/`EDIT`/`UPDATE`) and the full eighteen-member IPTC `DigitalSource` vocabulary riding `Builder.set_intent`, so AI-and-capture provenance origin is the table-driven source-type token the verifier reads, never a free-text manifest field or a raw `C2paDigitalSourceType[name]` member-access.
- Entry: `Provenance` IS the closed `tagged_union` and owns the async entry directly — `close` dispatches over the runtime `async_boundary` returning `RuntimeRail[tuple[ContentKey, CredentialEvidence]]`, mirroring the `exchange/conformance#CONFORMANCE` `close` pair rather than a separate `ProvenanceOp` wrapper holding the methods beside a data-only union; the `Sign` arm signs and keys the signed buffer, the `Read`/`ReadFragment` arms key the read asset, never a per-operation entrypoint or a `sign`/`read`/`verify` method triple. The `_emit` core carries one definition-time aspect over a thin body — `@stamina.retry(on=_TRANSIENT, ...)` re-attempts ONLY the `C2paError.RemoteManifest`/`Io` network transients (a TSA timestamp or remote-manifest fetch), so a non-transient codec/signature fault surfaces immediately — and offloads `_run` through `anyio.to_thread.run_sync` under the module-level `_THREAD_GATE` `CapacityLimiter` (the explicit thread bound, never the per-loop 40-token default) so the `async_boundary` rail never blocks the event loop while the native core signs and drives the `ta_url`/remote `requests` transport; `async_boundary` converts the residual `C2paError`/contract raise into the runtime `BoundaryFault` rail and the spine's own `rejected` line. `_emit` returns the `(ContentKey, CredentialEvidence)` pair — `ContentIdentity.of` over the signed/read bytes plus the rich evidence the read produces — never a four-scalar `ArtifactReceipt` that discards the forensic fields `measure` computes; the consumer mints `ArtifactReceipt.Credential(key, ev.manifest_id, ev.signer, ev.assertions, ev.validation_state)` from the pair exactly as the planner mints `ArtifactReceipt.Verdict` from the conformance pair, so the verification caller sees the full provenance while the receipt stays the thin four-scalar pipeline summary. The asset bytes arrive as an in-memory `BytesIO` from the imaging/document owner and leave as a signed `BytesIO`, so the codec hands a decoded buffer and receives a signed buffer with no intermediate file — `sign_file` is the path convenience only when a file handle already exists, never the default. The in-process `c2pa` package needs no `anyio.to_process` worker lane (distinct from the host-native `exchange/detect#DETECT`/`exchange/metadata#METADATA` siblings that cross the subprocess seam).
- Auto: the build is one ordered fold over `Manifest` then signer — `Manifest.author` seeds `Builder.from_json`, folds `set_intent` from the intent row (the `DigitalSource` projected through `_DIGITAL_SOURCE`, defaulting `C2paDigitalSourceType.EMPTY` when no source is declared), folds `set_no_embed`/`set_remote_url` when a sidecar URL is supplied, folds the `add_action` assertion sequence, folds each `Ingredient` through the `add_ingredient_from_stream`/`add_ingredient_from_archive` arm its case selects, and folds the `Resource` attachments through `add_resource`, while the `SignerSpec.cose` projection (woven with the shared `@beartype(conf=FAULT_CONF)` contract) builds the `Signer.from_info`/`from_callback` row from the matched case (the Signer-free `alg` accessor stays distinct so reading the algorithm mints no native signer handle); the `Sign` arm preflights `fmt` against the module-level `_SIGNABLE` set (`Builder.get_supported_mime_types`) and `raise C2paError.NotSupported` for a read-only asset before authoring, then drives `Builder.sign(signer, format, source_stream, dest_stream)` into a fresh `BytesIO` while capturing the detached manifest bytes the `sign` call returns, and reads the credential back through `_evidence` — the embedded path (no `remote_url`) validates the manifest bound into the signed asset, while the sidecar path (a `remote_url` whose `set_no_embed` asset carries no embedded manifest, the remote store unpublished at sign time) validates the detached bytes through `Reader.try_create(..., manifest_data=...)` so the read never degrades to bogus `unsigned` evidence; the builder closes single-use. The `Read`/`ReadFragment` arms open one `Reader.try_create`/`Reader.with_fragment` context whose `Reader.json()` STORE JSON is admitted once through the module-level `_STORE_DECODER` into the typed `_Store` — the active-manifest LABEL is the store's `active_manifest` string KEY and the manifest content is `manifests[active_manifest]` (the manifest dict carries no `label` of its own), so a `.get("label")` over the manifest dict is the phantom always-`""` read the decode replaces, and the same one decode carries `validation_state`, the `validation_results` success/informational/failure code sets, the `ingredientDeltas` per-ingredient failure codes, and the whole `manifests` chain, never per-accessor `get_active_manifest`/`get_validation_state`/`get_validation_results` calls plus a per-label `get_manifest` loop. `CredentialEvidence.measure` folds the active manifest's `label`/`instance_id`/`title`/`format`/`claim_generator`, the `signature_info` `alg`/`issuer`/`time`/`cert_serial_number`/`revocation_status` (with a derived `timestamped` flag off the trusted-time presence), the `Reader.is_embedded`/`get_remote_url` flags, the `sdk_version`, the assertion labels, the per-ingredient lineage (`relationship`/`title`/`format`/`instance_id`) and ingredient count, the `len(manifests)` chain depth, and every validation code set into one row in a single store walk, never a per-arm re-projection or a two-pass count; the shared `_opened` projector folds the two read-modality absences onto one `Option[Reader]` — `Reader.try_create`'s `None` for a credential-free asset and `Reader.with_fragment`'s raised `ManifestNotFound` for a credential-free fragment — whose `is_none` arm projects `CredentialEvidence.unsigned`, so either read modality admits the foreign absence to the carrier at the seam rather than one path threading `None` inward or degrading to a boundary fault. The verify `Context` threads per case through `Builder.from_json(context=...)` and `Reader.try_create(context=...)` — the `ReadFragment` arm's `Reader.with_fragment` takes none, so its case carries no `Context` slot rather than silently dropping one — so trust-list and verify policy is the case's own settled `Context` value, never the deprecated thread-local `load_settings` path.
- Receipt: `close` returns the rich `CredentialEvidence` as the `(ContentKey, CredentialEvidence)` pair, mirroring the `exchange/conformance#CONFORMANCE` `(ContentKey, ConformanceVerdict)` close — so the verification caller receives the full provenance picture (the signer `alg`/`issuer`/signing `time`/`cert_serial_number`/`revocation_status`, the `claim_generator`/`title`/`format`, the manifest-chain depth, the embedded/remote flags, the SDK version, the success/informational/failure validation codes, the per-ingredient delta failures, the assertion labels, the asset `instance_id` identity, and the per-ingredient lineage — each ingredient's `relationship`, `title`, `format`, and `instance_id`) rather than a four-scalar summary that discards the full forensic field set `measure` computes. The consumer mints `core/receipt#RECEIPT` `ArtifactReceipt.Credential(key, ev.manifest_id, ev.signer, ev.assertions, ev.validation_state)` from the returned pair — the four settled scalars (c2pa manifest id the store's `active_manifest` label, signer identity the matched `alg` token, assertion count, `validation_state` string) projected at the coordinator exactly as `ArtifactReceipt.Verdict(key, verdict)` is minted from the conformance pair, so the `credential` case stays flat scalars and `receipt.py` imports no `CredentialEvidence` value object. The `validation_state` plus the valid/invalid evidence the pair carries is the observable provenance the runtime `observability/metrics` `MeterProvider` signal stream reads off the minted receipt fold.
- Growth: a new operation is one `Provenance` case with its typed payload and one `match` arm; a new signer seam is one `SignerSpec` case with its typed signer-struct and one `cose`/`alg` arm; a new signing algorithm, intent, or digital-source origin is one `SigningAlg`/`Intent`/`DigitalSource` `StrEnum` member whose c2pa row derives by name correspondence, never a hand-enumerated table edit; a new ingredient attach modality is one `Ingredient` case and one `author` arm; a new manifest assertion is one `ActionDefinition` row; a new manifest resource is one `Resource` on the `resources` tuple folded through `add_resource`; a new evidence fact is one field on `CredentialEvidence` (and, when contract-settled, one scalar on the shared `ArtifactReceipt.Credential` case); a new readable store field is one field on the `_Store`/`_Manifest`/`_ValidationResults` boundary struct the one decode populates; a transient network fault widens the `_TRANSIENT` tuple; zero new surface. A whole-builder archive (`Builder.to_archive`/`from_archive`) is deliberately unbuilt — the frozen `Manifest` value is the reusable authoring template re-authored per asset, so only the processed-ingredient archive (which the JSON re-author cannot cheaply reproduce) earns an `Ingredient` case.
- Boundary: a per-format reader (`PngReader`/`JpegReader`), a per-algorithm signer class, a hand-rolled JUMBF/COSE manifest codec, a `get_active_manifest().get("label")` read where the label is the store's `active_manifest` KEY (the phantom always-`""` field the SDK's manifest dict never carries), a `reader.json()` `store["active_manifest"]`-as-dict mis-read, per-accessor `get_active_manifest`/`get_validation_state`/`get_validation_results` calls plus a per-label `get_manifest` loop where one `_STORE_DECODER.decode(reader.json())` carries the whole store, a local re-computation of `validation_state`, a five-of-eighteen `DigitalSource` subset, a hand-enumerated alg/intent/source `frozendict` where the `StrEnum`-name correspondence derives it, an un-gated `Builder.sign` that lets a PDF/`arw`/`nef` asset reach the native signer where `get_supported_mime_types` preflights it, a no-retry network sign where the `ta_url`/remote seam needs the transient `stamina.retry`, a `C2paSignerInfo(ta_url=None)` that raises `TypeError` (the constructor demands `str`/`bytes`) where the no-TSA absence projects to `""` at the `from_info` seam, a bare `Option.of_optional(Reader.with_fragment(...))` whose `is_none` branch is dead — the non-optional reader raises `ManifestNotFound` rather than returning `None` — where the shared `_opened` projector folds that raise to the `unsigned` read, a sidecar evidence read (a `remote_url`-bearing `Sign`) over the `set_no_embed` asset that carries no embedded manifest (degrading to bogus `unsigned` evidence) where the detached manifest bytes `Builder.sign` returns feed `Reader.try_create(..., manifest_data=...)`, a positional cert/key tuple decoded by index, a free-text `alg`/`intent`/`digital_source` field, a separate `Verify` op duplicating the `Read` extraction (the `validation_state`/`validation_results` evidence is the verification result), a separate `Ingredient` op duplicating `Manifest.ingredients`, a one-field `Provenance` wrapper over a separate `ProvenanceOp` union where the `tagged_union` owns dispatch and async entry directly, a `@staticmethod` factory returning the string-quoted `"Provenance"`/`"Ingredient"` where the `@classmethod` returns `Self` (mirroring the conformance sibling's factory convention), a per-instance `context` field the `ReadFragment` arm silently drops where the verify `Context` rides each case that consumes it, a four-scalar `ArtifactReceipt` return discarding the rich `CredentialEvidence` the verification produces where the `(ContentKey, CredentialEvidence)` pair carries it, and a `dict[str, Any]` manifest payload where the `ManifestDefinition`/`ActionDefinition`/`IngredientDefinition` `TypedDict` carries the typed `ReadOnly` extension band are the deleted forms; this owner is content-credential binding over already-emitted artifact bytes and owns no artifact production, mints no certificate or key (the campaign signer config supplies cert/key/TSA, the `CallbackSigner` case keeps private-key material in the `cryptography` keyring or HSM and only `C2paSigningAlg` plus the PEM cert chain cross the seam), and routes a PDF or raw-camera (`arw`/`nef`) asset to the `exchange/conformance#CONFORMANCE` `pyhanko` PAdES rail rather than `Builder.sign` (read-only here, proven by the `_SIGNABLE` set). The `ArtifactReceipt.Credential` case-tuple carrying the flat `(key, manifest_id, signer, assertions, validation_state)` scalars lands on the same-domain `core/receipt#RECEIPT` owner (the `Credential` case beside `Media`); the consumer projects those four scalars from the returned `CredentialEvidence` exactly as it mints `ArtifactReceipt.Verdict` from the conformance verdict, so `receipt.py` imports no producer value object and the `credential` case stays flat scalars. The signed-artifact `ContentKey` is the wire to the `csharp:Rasm.Persistence` store — the binding decodes the `XxHash128` seed the persistence owner re-derives over the same signed bytes, so the C2PA manifest and the content key co-identify one durable artifact across the language boundary, the credential never re-minting the content key the runtime owns.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
from collections.abc import Callable
from enum import StrEnum
from io import BytesIO
from typing import Final, Literal, NotRequired, ReadOnly, Required, Self, TypedDict, assert_never

import stamina
from anyio import CapacityLimiter, to_thread
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
from expression import Nothing, Option, case, tag, tagged_union
from msgspec import Raw, Struct, field, json, structs

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
# the native signable MIME set the `Sign` preflight gates on, the network-transient C2PA fault
# subset `stamina.retry` re-attempts (TSA timestamp + remote-manifest fetch), and the `c2pa.actions`
# assertion label whose `data` the evidence walk decodes the declared action history from.
_SIGNABLE: Final[frozenset[str]] = frozenset(Builder.get_supported_mime_types())
_TRANSIENT: Final[tuple[type[C2paError], ...]] = (C2paError.RemoteManifest, C2paError.Io)
_ACTIONS_LABEL: Final[str] = "c2pa.actions"

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
                return Signer.from_info(C2paSignerInfo(alg=_SIGNING_ALG[s.alg], sign_cert=s.sign_cert, private_key=s.private_key, ta_url=s.ta_url or ""))
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


# the per-ingredient lineage edge: the decode element `_Manifest.ingredients` admits AND the public
# lineage value `CredentialEvidence` carries, so one value object serves the read and the receipt —
# carrying the cross-asset `document_id` and the ingredient's own `validation_status` codes.
class IngredientRef(Struct, frozen=True, gc=False):
    relationship: str = ""
    title: str = ""
    format: str = ""
    instance_id: str = ""
    document_id: str = ""
    validation_status: tuple[_ValidationCheck, ...] = ()


class _Manifest(Struct, frozen=True, gc=False):
    instance_id: str = ""
    title: str = ""
    format: str = ""
    claim_generator: str = ""
    claim_generator_info: tuple[GeneratorRef, ...] = ()
    signature_info: _SignatureInfo = field(default_factory=_SignatureInfo)
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
            actions=tuple(action for item in active.assertions if item.label == _ACTIONS_LABEL for action in _ACTION_DECODER.decode(item.data).actions),
            ingredient_lineage=active.ingredients,
        )

    @classmethod
    def unsigned(cls, signer: str, /) -> Self:
        return cls(manifest_id="", signer=signer, assertions=0, ingredients=0, validation_state="unsigned", manifest_count=0, sdk_version=sdk_version())


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


# `Provenance` IS the closed union: the dispatch, the async entry, and every case body fold onto one
# owner (mirroring `exchange/conformance#CONFORMANCE`), each case a `(bytes, Spec)` pair whose asset
# bytes lead and whose typed spec carries the rest.
@tagged_union(frozen=True)
class Provenance:
    tag: Literal["sign", "read", "read_fragment"] = tag()
    sign: tuple[bytes, SignSpec] = case()
    read: tuple[bytes, ReadSpec] = case()
    read_fragment: tuple[bytes, FragmentSpec] = case()

    @classmethod
    def Sign(cls, asset: bytes, spec: SignSpec, /) -> Self:
        return cls(sign=(asset, spec))

    @classmethod
    def Read(cls, asset: bytes, spec: ReadSpec, /) -> Self:
        return cls(read=(asset, spec))

    @classmethod
    def ReadFragment(cls, init: bytes, spec: FragmentSpec, /) -> Self:
        return cls(read_fragment=(init, spec))

    async def close(self) -> RuntimeRail[tuple[ContentKey, CredentialEvidence]]:
        return await async_boundary(f"credential.{self.tag}", self._emit)

    # retry re-runs ONLY the `_TRANSIENT` network seam (a TSA timestamp or remote-manifest fetch);
    # a non-transient codec/signature fault surfaces immediately and `async_boundary` rails it once.
    # `ContentIdentity.of` mints the bare `ContentKey` over the produced bytes (the corpus consensus
    # `of -> ContentKey`, never a rail) on the loop after the thread returns, exactly as `exchange/conformance#CONFORMANCE`.
    @stamina.retry(on=_TRANSIENT, attempts=4, timeout=30.0)
    async def _emit(self) -> tuple[ContentKey, CredentialEvidence]:
        asset, evidence = await to_thread.run_sync(self._run, limiter=_THREAD_GATE)
        return ContentIdentity.of(f"credential.{self.tag}", asset), evidence

    def _run(self) -> tuple[bytes, CredentialEvidence]:
        match self:
            case Provenance(tag="sign", sign=(asset, spec)):
                return self._signed(asset, spec)
            case Provenance(tag="read", read=(asset, spec)):
                return asset, self._evidence(self._opened(lambda: Reader.try_create(spec.fmt, BytesIO(asset), context=spec.context)), "")
            case Provenance(tag="read_fragment", read_fragment=(init, spec)):
                return init, self._evidence(self._opened(lambda: Reader.with_fragment(spec.fmt, BytesIO(init), BytesIO(spec.fragment))), "")
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
        reader = self._opened(lambda: Reader.try_create(spec.fmt, BytesIO(signed), manifest_data=detached if spec.remote_url is not None else None, context=spec.context))
        return signed, self._evidence(reader, spec.signer.alg)

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
            return CredentialEvidence.measure(store, embedded=live.is_embedded(), remote_url=Option.of_optional(live.get_remote_url()).default_value(""), signer=signer)


# --- [COMPOSITION] ----------------------------------------------------------------------
# every sign/read offload threads this one explicit bound, so N concurrent `close` calls share a fixed
# thread band rather than the per-loop 40-token default — the GIL-releasing native c2pa core and the
# `ta_url`/remote-manifest `requests` transport run off the event loop, bounded at the boundary.
_THREAD_GATE: Final[CapacityLimiter] = CapacityLimiter(8)
```

## [03]-[RESEARCH]

- [C2PA_SIGN_EMBED_SIDECAR] [RESOLVED]: `Builder.from_json(manifest_json, context=None) -> Builder` (the named factory over a string-or-dict definition), `Builder.sign(signer, format, source, dest) -> bytes` (the single signing surface keyed by first-argument shape, single-use and closing after the call; omitting `dest` buffers into a `BytesIO`), `Builder.sign_file(source_path, dest_path, signer=None) -> bytes` (the path row), `Builder.set_no_embed()` (the sidecar/non-embedded toggle), and `Builder.set_remote_url(remote_url)` (the remote-manifest URL) verify against the installed `c2pa 0.35.1` (`c2pa-rs 0.88.0`) and the folder `c2pa-python` `.api` `[03]` `Builder` table. The `Sign` arm drives one `Builder.sign(signer, format, source_stream, dest_stream)` over `BytesIO`; a present `remote_url` folds `set_no_embed`/`set_remote_url` so the embed-vs-sidecar modality rides one `sign` case discriminated by the `remote_url` value rather than a parallel `Sidecar` case or the deleted `embed: bool` knob. `Builder.sign(...) -> bytes` returns the detached manifest store, and `Reader.try_create(format_or_path, stream=None, manifest_data=None, context=None)` carries a `manifest_data` parameter (verified on the installed `Reader`/`try_create` signature), so the sidecar path reads its evidence through `Reader.try_create(fmt, signed_stream, manifest_data=detached, context=)`: a `set_no_embed` asset carries no embedded manifest and its remote store is unpublished at sign time, so an embedded-style `try_create(fmt, signed_stream)` read would resolve no manifest and degrade to `unsigned` evidence — the captured detached bytes are the only valid evidence source for the sidecar verify. `Builder.get_supported_mime_types() -> list[str]` is a `classmethod` (callable at boundary scope) whose 57-entry native signable set excludes `application/pdf`/`arw`/`nef`/`image/x-nikon-nef`/`image/x-sony-arw` (verified against the 63-entry `Reader.get_supported_mime_types()` readable set), so the module-level `_SIGNABLE` preflight `raise C2paError.NotSupported` fast-fails a read-only asset before authoring, the explicit gate the prior prose-only routing assertion lacked. The in-memory `BytesIO` source/dest pair is the asset-stream seam, no intermediate file.
- [C2PA_READ_AND_STORE_DECODE] [RESOLVED]: `Reader.try_create(format_or_path, stream=None, manifest_data=None, context=None) -> Optional[Reader]` (the optional-returning factory mapping `ManifestNotFound` to `None`), `Reader.with_fragment(format, stream, fragment_stream) -> Reader` (the fragmented-BMFF init+fragment read for DASH/CMAF, no `context` parameter), and `Reader.json() -> str` verify against the installed distribution and the folder `.api` `[03]` `Reader` table. The store walk decodes `Reader.json()` ONCE through the module-level `msgspec.json.Decoder(type=_Store)`: the `c2pa.py` `get_active_manifest` source proves the active manifest is `manifests[data["active_manifest"]]` and the manifest dict carries NO `label` of its own (the label is the store-level `active_manifest` KEY), so the prior `convert(get_active_manifest(), _ActiveManifest).label` was the phantom always-`""` read — `manifest_id` is `store.active_manifest`. The one decode also carries `validation_state`, `validation_results.activeManifest.{success,informational,failure}[].code`, `validation_results.ingredientDeltas[].validationDeltas.failure[].code`, every `signature_info` field, and the whole `manifests` chain (`len(manifests)` the lineage depth), so the separate `get_validation_state`/`get_validation_results` accessors and a per-label `get_manifest` loop collapse into this one decode; a sample-store round trip confirms the rename map, the nested-struct + `dict[str, _Manifest]` decode, and the unknown-field drop. `Reader.is_embedded() -> bool` and `Reader.get_remote_url() -> Optional[str]` stay accessor reads (the embedded flag and remote URL are not store-JSON fields), the latter projected through `Option.of_optional`. `Reader.with_fragment` returns a NON-optional `Reader` and raises `C2paError.ManifestNotFound` for a credential-free fragment (verified against the installed `0.35.1` signature) where `try_create` returns `None`, so wrapping `with_fragment` in a bare `Option.of_optional` is the dead-branch tell that never projects `unsigned`; the shared `_opened` projector folds BOTH absences — the `try_create` `None` and the `with_fragment` raised `ManifestNotFound` — onto one `Option[Reader]`, so a credential-free `Read` OR `ReadFragment` projects `CredentialEvidence.unsigned` symmetrically rather than the fragment path degrading to a boundary fault (the `_resilient`-shaped capture the `exchange/conformance#CONFORMANCE` sibling uses). `ReadFragment` is the genuine fragmented modality, replacing the deleted `Verify` op whose extraction was identical to `Read`. The active manifest's `instance_id` — the asset-instance provenance identity, a snake_case manifest field confirmed against the `c2pa.py` source (`manifests[data["active_manifest"]]` with ingredient matching on `label` or `instance_id`) — joins the one decode as `CredentialEvidence.instance_id`, distinct from `manifest_id` (the claim label off the store key). Each active-manifest ingredient decodes as one `IngredientRef` carrying `relationship`/`title`/`format`/`instance_id` (the snake_case ingredient keys the same store JSON exposes), so `CredentialEvidence.ingredient_lineage` is the genuine per-ingredient derivation evidence a forensic consumer walks rather than the bare relationship-string tuple the prior shape carried.
- [C2PA_SIGNER_AXIS] [RESOLVED]: `Signer.from_info(signer_info: C2paSignerInfo) -> Signer`, `Signer.from_callback(callback, alg: C2paSigningAlg, certs: str, tsa_url=None) -> Signer`, `C2paSignerInfo(alg, sign_cert, private_key, ta_url)`, and the `C2paSigningAlg` `ES256`/`ES384`/`ES512`/`PS256`/`PS384`/`PS512`/`ED25519` rows verify against the installed distribution. The two `SignerSpec` cases bind one `Signer.from_info`/`Signer.from_callback` row each through the shared `@beartype(conf=FAULT_CONF)` contract weave (matching the `exchange/conformance#CONFORMANCE` `_signer` projector), and the `_SIGNING_ALG` `frozendict` resolves the enum ordinal from the `SigningAlg.name` correspondence at boundary scope so no native `IntEnum` value is hardcoded beside the signer call; `Signer.from_callback` keeps private-key material in the `cryptography`/HSM keyring with only `C2paSigningAlg` and the PEM `certs` chain crossing. `C2paSignerInfo.__init__(alg, sign_cert, private_key, ta_url)` requires `ta_url` as `str`/`bytes` and raises `TypeError` on `None` (verified against the installed `0.35.1` constructor, which encodes the value to UTF-8 bytes for the native lib), so the `cert_key` arm projects the no-TSA `ta_url` absence to `""` at the `from_info` seam, while the `callback` arm passes `ta_url` straight through because `Signer.from_callback(callback, alg, certs, tsa_url=None)` admits the `None` default — `sign_cert`/`private_key` cross as `bytes` (the constructor only null-checks them, accepting the PEM byte buffers directly).
- [C2PA_INTENT_SOURCE] [RESOLVED]: `Builder.set_intent(intent: C2paBuilderIntent, digital_source_type: C2paDigitalSourceType = C2paDigitalSourceType.EMPTY)` and `Builder.add_action(action_json)` verify against the folder `.api` `[03]` `Builder` table; `C2paBuilderIntent` (`CREATE`/`EDIT`/`UPDATE`) and the full `C2paDigitalSourceType` vocabulary verify against the installed enum — eighteen non-`EMPTY` members (`DIGITAL_CAPTURE`, `DIGITAL_CREATION`, `ALGORITHMIC_MEDIA`, `TRAINED_ALGORITHMIC_MEDIA`, `TRAINED_ALGORITHMIC_DATA`, `COMPOSITE_SYNTHETIC`, `COMPOSITE_CAPTURE`, `COMPOSITE_WITH_TRAINED_ALGORITHMIC_MEDIA`, `COMPUTATIONAL_CAPTURE`, `DATA_DRIVEN_MEDIA`, `HUMAN_EDITS`, `NEGATIVE_FILM`, `POSITIVE_FILM`, `PRINT`, `SCREEN_CAPTURE`, `VIRTUAL_RECORDING`, `ALGORITHMICALLY_ENHANCED`, `COMPOSITE`), so the prior five-member `DigitalSource` subset under-modeled the capture-and-synthesis provenance taxonomy the verifier reads. `Intent`/`DigitalSource` are `StrEnum`s whose member names match the c2pa enum names, so `_INTENT`/`_DIGITAL_SOURCE`/`_SIGNING_ALG` derive each row through `C2paX[member.name]` — one name correspondence, never the hand-enumerated table that grew row-by-row; a new IPTC source is one `StrEnum` member.
- [C2PA_INGREDIENT_ARCHIVE] [RESOLVED]: `Builder.add_ingredient_from_stream(ingredient_json, format, source)`, `Builder.add_ingredient_from_archive(stream)` (single-arg, rehydrating from a written ingredient archive), `Builder.write_ingredient_archive(ingredient_id, stream)`, and `Builder.add_resource(uri, stream)` verify against the installed `Builder`. The `Ingredient` closed `tagged_union` carries the attach modality as the case the `author` fold matches — `stream` (definition + format + source bytes → `add_ingredient_from_stream`) and `archive` (a `write_ingredient_archive` blob → `add_ingredient_from_archive`) — so a parent processed once is reused across a campaign's derived assets without re-processing, the modality the value carries rather than a `from_archive: bool` knob the body re-derives. Whole-builder `to_archive`/`from_archive` is deliberately unbuilt: the frozen `Manifest` is the reusable authoring template the per-asset `author` re-runs, so only the processed-ingredient reuse the JSON re-author cannot reproduce earns a case.
- [C2PA_TRANSIENT_RETRY] [RESOLVED]: `C2paError` is the base of the nested typed subclass family — `C2paError.RemoteManifest`, `C2paError.Io`, `C2paError.Signature`, `C2paError.NotSupported`, `C2paError.ManifestNotFound`, `C2paError.Verify`, `C2paError.Decoding`/`Encoding`/`Json`/`ResourceNotFound`/`AssertionNotFound`/`FileNotFound` (verified as `Exception` subclasses accessible as `C2paError.<Name>`, not module-level symbols). The `c2pa` runtime deps `cryptography`/`requests` carry the RFC-3161 `ta_url` timestamp and the remote-manifest fetch over the network on the `anyio.to_thread.run_sync` worker, so this owner shares the `exchange/conformance#CONFORMANCE` "crypto-with-network" concern, not a pure-CPU interior — `stamina.retry(on=(C2paError.RemoteManifest, C2paError.Io), attempts=4, timeout=30.0)` weaves the `_emit` core innermost so only the network transients re-attempt (a non-transient `Decoding`/`Signature`/`NotSupported` fault surfaces immediately per the `stamina` selectivity law), the clean attempt returns the `(key, evidence)` pair once, and `async_boundary` converts the residual `C2paError`/contract raise into the runtime `BoundaryFault`; `stamina.retry` decorating an async method verifies against the installed `stamina 26.1.0`.
- [RECEIPT_AND_PERSISTENCE_SEAM] [RESOLVED]: `Provenance.close` returns `RuntimeRail[tuple[ContentKey, CredentialEvidence]]`, mirroring the `exchange/conformance#CONFORMANCE` `close` that returns `RuntimeRail[tuple[ContentKey, ConformanceVerdict]]` — a content-authenticity verification yields the full evidence value, not a four-scalar summary that discards the issuer, signing time, cert serial, revocation status, validation codes, and ingredient lineage the `measure` walk computes. `_emit` carries `@stamina.retry` alone (no `@receipted`, exactly as the conformance sibling carries none), mints `ContentIdentity.of(f"credential.{self.tag}", signed)` over the signed/read bytes, and returns the `(key, evidence)` pair; the consumer (the `core/plan#PLAN` planner or any coordinator holding both owners) mints `ArtifactReceipt.Credential(key, ev.manifest_id, ev.signer, ev.assertions, ev.validation_state)` from the pair — the settled `tuple[ContentKey, str, str, int, str]` case and its `_KEYS` row `("manifest_id", "signer", "assertions", "validation_state")` on the `core/receipt#RECEIPT` owner, carrying the CORRECT label (the store `active_manifest` key) rather than the phantom empty field — exactly as it mints `ArtifactReceipt.Verdict` from the conformance pair, so `receipt.py` imports no `CredentialEvidence` and the `credential` case stays flat scalars. The persistence owner re-derives the same `XxHash128` seed over the identical signed bytes (`csharp:Rasm.Persistence`), so the C2PA manifest and the content key co-identify one durable artifact; the credential re-mints no canonical key (the runtime `content_identity` owns it), the binding a key thread, never a parallel identity.
- [CALLBACK_SIGNER_SEAM] [RESOLVED]: `Signer.from_callback(callback, alg, certs, tsa_url=None)` is the `cryptography`/HSM seam, and the `CallbackSigner.sign: Callable[[bytes], bytes]` field the `callback` case carries returns the raw COSE signature bytes for a digest so private-key material lives in the keyring/HSM and `c2pa` never holds it; the campaign signer-configuration owner constructs the `CallbackSigner` with the concrete keyring/HSM-bound digest-signer at composition, the same campaign-supplied-credential deferral the `exchange/conformance#CONFORMANCE` `ExternalSig` injected-signature path uses. The `cert_key` case is the direct `C2paSignerInfo` cert/key path needing no external callback.

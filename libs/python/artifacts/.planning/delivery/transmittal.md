# [PY_ARTIFACTS_TRANSMITTAL]

`Transmittal` is the ISO 19650 transmittal / issue-for-construction close at the delivery boundary — the assembly orchestrator that folds an issued sheet set into a press-imposed plan-set, a content-addressed transmittal container, a legally PAdES-signed and provenance-credentialed record, and a formal transmittal-record manifest, composing sibling owners at the wire. It IS the closed `expression.tagged_union` — its `Assemble`/`Seal`/`Issue`/`Manifest` cases own the dispatch, the async entry, AND every case body directly, never a one-field wrapper over a separate op union — each discriminating by its own typed `(Deliverable, spec)` payload: `Assemble` composes `composition/imposition#IMPOSE` into a press form, `Seal` composes `package/archive#ARCHIVE` into one content-addressed transmittal container, `Issue` composes `exchange/conformance#CONFORMANCE` PAdES-LTA signing CONCURRENTLY with `exchange/credential#CREDENTIAL` C2PA sheet-lineage signing inside one `anyio` task group, and `Manifest` composes `delivery/register#REGISTER` as the issued index and builds the ISO 19650 transmittal-record XML — Schematron-validated and C14N-canonical — each case folding once into the shared `TransmittalEvidence` the public `close` returns, never a per-stage evidence bag or a per-product transmittal-builder family. Every sibling is composed through the ONE uniform producer contract every artifacts producer exposes — `emit() -> ArtifactWork` whose `work` thunk resolves `RuntimeRail[ArtifactReceipt]` with `receipt.slot == node.key` — so the orchestrator drives `sign.emit().work()` and reads the typed receipt case (`receipt.verdict`, `receipt.credential`, `receipt.slot`), never a phantom `.of()`/`.close()`/`.pack()` sibling surface and never a private-member reach. This owner authors no sheet, drawing, IFC, or QTO, and re-implements no imposition/archive/crypto engine.

`close` runs one shared prologue before the total `match`: the register audit folds ONCE under the purpose-keyed `_PURPOSE_AUDIT` policy row — `Purpose.CONSTRUCTION`/`AS_BUILT` select the register owner's `CONTRACTUAL_AUDIT`, every other purpose `STANDARD_AUDIT` — and `_gated` accumulates every failed gate through `BoundaryFault.combine` before any product renders: a missing legal-header field, an empty sheet set (a zero-sheet collation is a config refusal, never an opaque MuPDF raise off the thread lane), a contractually-issued sheet the issued index never lists (`Deliverable.unregistered`, the sheet-register congruence the transmittal record attests), and a severed contractual register. `TransmittalRecord.of` admits the raw client header through the module-level `_PAYLOAD` `TypeAdapter` EXACTLY ONCE, normalizes issue and response dates, maps its revision through `RevisionCode.parse`, and carries structured `loc` paths on `RecordDefect`; `SheetRef.admitted` similarly binds raw suitability and revision through the register owners, so the interior never re-parses either vocabulary. Owned native seams (`pymupdf` collation and `lxml` record serialization) cross `deliverable.lane.offload(...)` — the instance-lane seam returning `RuntimeRail` directly, no rail-to-raise-to-rail erasure and no second boundary — while transient TSA / remote-manifest retry lives in the composed sign legs. `Transmittal` schedules into `core/plan#PLAN` as ONE aggregate `ArtifactWork` whose `parents` are the constituent sheet keys plus the normalized issued-register key, keyed PRE-RUN through `ContentIdentity.key` over every output-affecting record, sheet, profile, signer-policy, and parent axis. `Issue` and password-bearing `Seal` nodes admit `bare`: fresh legal or secret-dependent bytes are never cache-elided. Each op mints the `core/receipt#RECEIPT` `ArtifactReceipt.Transmittal` case, never a parallel receipt rail.

## [01]-[INDEX]

- [01]-[TRANSMITTAL]: the ISO 19650 transmittal / issue-for-construction orchestrator over the closed `Transmittal` `tagged_union` — `Assemble` the press form, `Seal` the container, `Issue` the concurrent PAdES + C2PA sign, `Manifest` the issued index + record XML — gated by the purpose-keyed register audit, composed through the uniform `emit().work()` producer contract, folded once into the shared `TransmittalEvidence`, and scheduled as one `core/plan#PLAN` `ArtifactWork` minting the `ArtifactReceipt.Transmittal` case.

## [02]-[TRANSMITTAL]

- Owner: `Transmittal` the one delivery-issue owner AND the closed `expression.tagged_union` discriminating over its own typed `(Deliverable, spec)` payload — `Assemble`, `Seal`, `Issue`, `Manifest` — matched by one total `match` in `close`, the `@classmethod` factories returning `Self` and the `_assembled`/`_sealed`/`_issued`/`_manifested` case-body methods all folded onto the union, never a one-field wrapper over a separate op union, a `stage: str` discriminant beside an optional-field bag, or a free module function reconstructing a case body outside the owner. `Deliverable` is the shared payload every case reads — `sheets` the admitted constituent sheets, `issued_register` the normalized `RegisterOp.Index` projection whose key is the aggregate parent, `record: TransmittalRecord` the ISO 19650 header, `lane: LanePolicy` the bounded instance seam the owner's native kernels offload through. `SheetRef` is the one frozen constituent-sheet value carrying distinct container reference, content key, data, suitability, and revision identities; `TransmittalRecord` carries the issue header, routing, response, confidentiality, distribution, and copy-recipient fields admitted once through `_PAYLOAD`. `SealSpec.profile` is the one container policy value selecting algorithm, knobs, names, and optional secret — never parallel `algo`/`profile`/`password` knobs. `pymupdf` owns the deterministic plan-set collation, sheet-index bookmarks, and issue seal; `lxml.etree` the escaped-and-canonical transmittal-record XML plus the `isoschematron.Schematron` oracle; `composition/imposition#IMPOSE` the press form, `package/archive#ARCHIVE` the container, `exchange/conformance#CONFORMANCE` the PAdES close, `exchange/credential#CREDENTIAL` the C2PA lineage.
- Cases: `Transmittal` cases matched by one total `match`, each folding once into the shared `TransmittalEvidence` and returning `RuntimeRail[tuple[ContentKey, TransmittalEvidence]]` — never a per-product transmittal-builder sibling, never a per-stage `_emit`. `Assemble(deliverable, AssembleSpec)` collates the `SheetRef.data` through `_collated`, binds the VALIDATED `ImposeOp.Impose` ingress, then drives `Imposition(op=...).emit().work()` — the press-form key is `receipt.slot` and the press metrics ride `Imposition.planned()`. `Seal(deliverable, SealSpec)` resolves the one `CodecProfile`, rejects a non-container case on the rail, converts `Archive.of` policy refusal through `catch(exception=ValueError)`, then drives `.emit().work()` over the plan-set plus attachments. `Issue(deliverable, IssueSpec)` collates then starts `_signed_pades` CONCURRENTLY with the optional `_signed_cose` inside one `anyio.create_task_group`, each child's rail riding its `TaskHandle.return_value`; `_credential` distinguishes `unsigned`, `failed`, and credential-reported states without a `Result.to_option` erasure. `Manifest(deliverable, ManifestSpec)` drives `issued_register.emit().work()` for the normalized index receipt, then builds the transmittal-record XML with separate container reference and content-key attributes, Schematron failed-assert count, and C14N bytes.
- Entry: `close` is the public orchestrator terminal — the shared prologue folds the register audit once under `_PURPOSE_AUDIT[record.purpose]`, accumulates missing legal-header, empty-sheet-set, unregistered-sheet, and contractual-register faults through the `_gated` `BoundaryFault.combine` fold, then one total `match` dispatches to the async case bodies that thread composed rails through `bind`/`map`. `emit` returns the ONE aggregate `ArtifactWork` directly, its parents the constituent sheet keys plus `issued_register.key`; `_admission` is value-keyed — `Admission(bare=None)` for `Issue` and password-bearing `Seal`, `Admission(keyed=None)` otherwise. `TransmittalRecord.of` narrows malformed payload, purpose, and revision inputs to the closed `RecordDefect` cases and accumulates concurrent purpose/revision defects; `SheetRef.admitted` accumulates the register owner's `RegisterFault` without weakening either vocabulary.
- Auto: `_assembled` binds `_collated` into the validated `ImposeOp.Impose` ingress and composed press node; `_sealed` composes the collation and profile-admission rails before `Archive`; `_issued` opens ONE `anyio.create_task_group`, carries both child rails on `TaskHandle.return_value`, aborts on a failed PAdES leg, and retains C2PA absence versus failure as distinct evidence. `_manifested` binds the normalized index node then `_record`. `_collate` is ONE imperative measured kernel threading ONE owned `pymupdf.Document` across N `insert_pdf` inserts, with every handle bracketed, then authors the TOC and fixed metadata before `tobytes(no_new_id=True)`. `_record_xml` builds dynamic values through `SubElement.text`/`set`, validates against the `@cache`-compiled `_record_schema`, counts `svrl:failed-assert` from `validation_report`, and serializes `c14n2` bytes; both kernels cross `deliverable.lane.offload(Kernel.of(..., KernelTrait.HOSTILE), ...)` onto the warm process pool — the MuPDF collate mutation and the lxml validate/serialize both hold the GIL, the same engine ruling `composition/sheet#SHEET` and `document/emit#DOCUMENT` carry — and thread the returned `RuntimeRail` unchanged.
- Receipt: `close` returns the rich `TransmittalEvidence` as the `(ContentKey, TransmittalEvidence)` pair — the full issue picture (transmittal id / revision / issue date / party / recipient / purpose, issued-sheet and press-signature counts, imposition scheme, container key and member count, folded PAdES level / signer / signing time / validity, C2PA state and lineage depth, the record Schematron conformance evidence-only, the dominant suitability / latest revision, the `register_complete` audit verdict, the unregistered-sheet count, the leaf `ConformanceVerdict` forensic edge as `Option`). `emit` projects it onto `ArtifactReceipt.Transmittal(key, transmittal_id, sheets, suitability, container-or-plan_set, validation_state)` — six settled scalars, so `receipt.py` imports no `TransmittalEvidence` and the case stays flat scalars, the delivery-issue evidence a CASE on the one `ArtifactReceipt` family, never a parallel receipt rail. `validation_state` plus the native-int counts are the observable issue facts the runtime `observability/metrics` `MeterProvider` reads off the minted receipt.
- Packages: `expression` (the `Transmittal`/`RecordDefect` unions, the accumulating `Result`/`Option` admission rails, `.bind`/`.map` composed-rail threading, `Block` the `_gated` refusal fold, and `extra.result.catch` at archive construction); `msgspec` (the frozen value objects, `structs.replace` the profile/evidence projection, `msgpack.Encoder` the canonical key preimage); `pydantic` (`TypeAdapter` the `_PAYLOAD` admission, `ValidationError.errors()` the structured `loc` evidence); `anyio` (`create_task_group` + `TaskHandle.return_value` the concurrent-sign failure boundary and child carrier); `xxhash` (`xxh3_128_digest` the attachment, cover, and public-certificate fingerprints; private key, passphrase, and password bytes never enter a preimage); `pymupdf` (the deterministic navigable, metadata-sealed plan-set collation, `lazy import`-deferred); `lxml.etree` (the escaped C14N record XML, `isoschematron.Schematron`, and `validation_report` SVRL failed-assert count, `lazy`-deferred); `functools.cache` (the compiled-once record Schematron bytes); runtime (`identity.ContentIdentity.key`/`ContentKey`, `lanes.LanePolicy`, `faults.BoundaryFault.combine`/`RuntimeRail`); composed owners `composition/imposition#IMPOSE`, `package/archive#ARCHIVE` with the one `package/bundle#BUNDLE` `CodecProfile`, `exchange/conformance#CONFORMANCE`, `exchange/credential#CREDENTIAL`, `delivery/register#REGISTER` (`RegisterOp.Index`, `Suitability`, `RevisionCode`, audit policies), `core/plan#PLAN`, and `core/receipt#RECEIPT`. No new external library — every engine is composed.
- Growth: a new issue product is one `Transmittal` case with its payload and one `close` arm plus one case-body method; a new imposition scheme rides `Scheme`, a new container algorithm arrives as one composed `CodecProfile` case admitted by `SealSpec.resolved`, a new PAdES level or signer rides `PadesSpec`, and a new C2PA relationship or intent rides `CoseSpec`. A new record-header field lands on `TransmittalPayload`, `TransmittalRecord.canonical`, `_record_xml`, and `TransmittalEvidence`; a new sealed attachment is one `(name, bytes)` row; a new purpose is one `Purpose` member whose audit policy derives in `_PURPOSE_AUDIT`; a new legal-header requirement is one `_REQUIRED_CLOSE` row value; a new issue-gate refusal is one row in the `_gated` fold. Zero new surface — the owner grows by case, composed-owner vocabulary, policy row, and evidence field, never by method family.
- Boundary: no sheet authoring (`composition/sheet#SHEET` owns the title block; the transmittal admits its `SheetRef` projection), no drawing, no imposition placement, no container codec, no crypto engine, no register audit authoring, and no IFC/QTO. Raw purpose, suitability, and revision tokens cross their owner once; container reference and content key remain distinct XML attributes; `SubElement` escapes the record tree; the uniform `emit().work()` producer contract replaces phantom sibling verbs; one in-place `pymupdf` kernel replaces whole-buffer recopy; `no_new_id=True` plus fixed record fields suppress random PDF identity; PDF sheets ride as C2PA `Ingredient` values rather than unsupported per-sheet C2PA signs; `anyio` child handles replace `asyncio.gather`; optional C2PA failure remains soft but evidenced; native rails never collapse and re-raise; legal and secret-dependent output admits `bare`; receipt evidence remains one `ArtifactReceipt.Transmittal` case.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
from datetime import date
from enum import StrEnum
from functools import cache
from typing import Final, Literal, NotRequired, ReadOnly, Required, Self, TypedDict, Unpack, assert_never

import xxhash
from anyio import TaskHandle, create_task_group
from builtins import frozendict
from expression import Error, Nothing, Ok, Option, Result, Some, case, tag, tagged_union
from expression.collections import Block
from expression.extra.result import catch
from msgspec import Struct, msgpack, structs
from pydantic import TypeAdapter, ValidationError

from rasm.artifacts.composition.imposition import Geometry, ImposeOp, ImposedPlan, Imposition, Marks, Scheme
from rasm.artifacts.core.plan import Admission, ArtifactWork
from rasm.artifacts.core.receipt import ArtifactReceipt, ConformanceVerdict
from rasm.artifacts.delivery.register import (
    CONTRACTUAL_AUDIT,
    STANDARD_AUDIT,
    AuditPolicy,
    ContainerState,
    Register,
    RegisterEvidence,
    RegisterFault,
    RegisterOp,
    RevisionCode,
    RevisionKind,
    Suitability,
)
from rasm.artifacts.exchange.conformance import Conformance, SignerSource
from rasm.artifacts.exchange.conformance import SignSpec as PadesSpec  # collides with credential.SignSpec — aliased at the seam
from rasm.artifacts.exchange.credential import (
    ActionDefinition,
    DigitalSource,
    Ingredient,
    IngredientDefinition,
    Intent,
    Manifest,
    ManifestDefinition,
    Provenance,
    SignerSpec,
)
from rasm.artifacts.exchange.credential import SignSpec as CoseSpec  # C2PA sign spec, aliased against PAdES
from rasm.artifacts.package.archive import Archive
from rasm.artifacts.package.bundle import CodecProfile, ZipStreamKnobs
from rasm.runtime.faults import BoundaryFault, RuntimeRail
from rasm.runtime.identity import ContentIdentity, ContentKey
from rasm.runtime.lanes import LanePolicy
from rasm.runtime.workers import Kernel, KernelTrait

lazy import pymupdf  # Plan-set collation engine; cold native MuPDF, deferred
lazy from lxml import etree, isoschematron  # Record XML builder and conformance oracle; cold, deferred to Manifest serialization

# --- [TYPES] ----------------------------------------------------------------------------


class Purpose(StrEnum):  # ISO 19650 purpose-of-issue vocabulary; free-text purpose is unrepresentable
    COORDINATION = "coordination"  # issued for spatial coordination (S1)
    INFORMATION = "information"  # issued for information (S2)
    REVIEW_COMMENT = "review-comment"  # issued for review and comment (S3)
    REVIEW_AUTHORIZE = "review-authorize"  # issued for review and authorization (S4)
    CONSTRUCTION = "construction"  # issued for construction (published, contractual)
    TENDER = "tender"  # issued for tender/bid
    APPROVAL = "approval"  # issued for statutory approval
    AS_BUILT = "as-built"  # as-constructed record issue (published, contractual)


type TransmittalStage = Literal["assemble", "seal", "issue", "manifest"]
type ValidationState = Literal["unsigned", "valid", "invalid"]
type RecordValidationState = Literal["unverified", "valid", "invalid"]


class TransmittalPayload(TypedDict, closed=True):  # Raw client record header, admitted once
    number: Required[ReadOnly[str]]
    issuing_party: Required[ReadOnly[str]]
    recipient: Required[ReadOnly[str]]
    purpose: Required[ReadOnly[str]]
    revision: NotRequired[ReadOnly[str]]
    issued_at: NotRequired[ReadOnly[date]]
    project: NotRequired[ReadOnly[str]]
    project_id: NotRequired[ReadOnly[str]]
    subject: NotRequired[ReadOnly[str]]
    description: NotRequired[ReadOnly[str]]
    response_due: NotRequired[ReadOnly[date]]
    confidentiality: NotRequired[ReadOnly[str]]
    distribution: NotRequired[ReadOnly[str]]
    remarks: NotRequired[ReadOnly[str]]
    copy_recipients: NotRequired[ReadOnly[tuple[str, ...]]]


# --- [CONSTANTS] ------------------------------------------------------------------------
_PAYLOAD: Final = TypeAdapter(TransmittalPayload)
_PURPOSES: Final[frozenset[str]] = frozenset(purpose.value for purpose in Purpose)  # derived membership set, never `_value2member_map_`
# Contractual purposes demand the register owner's `CONTRACTUAL_AUDIT` bar.
_CONTRACTUAL: Final[frozenset[Purpose]] = frozenset({Purpose.CONSTRUCTION, Purpose.AS_BUILT})
_PURPOSE_AUDIT: Final[frozendict[Purpose, AuditPolicy]] = frozendict({
    purpose: CONTRACTUAL_AUDIT if purpose in _CONTRACTUAL else STANDARD_AUDIT for purpose in Purpose
})
_NS: Final[str] = "https://rasm.dev/schema/iso19650/transmittal"
_SCHEMATRON_NS: Final[str] = "http://purl.oclc.org/dsdl/schematron"
_SVRL_NS: Final[str] = "http://purl.oclc.org/dsdl/svrl"
# Mandated record fields each produce one `sch:assert`; a dropped or blank field makes `record_state` invalid.
_REQUIRED_RECORD: Final[tuple[str, ...]] = ("number", "revision", "issuedAt", "issuingParty", "recipient", "purpose", "projectReference")
_REQUIRED_CLOSE: Final[frozendict[TransmittalStage, tuple[str, ...]]] = frozendict({
    "assemble": (),
    "seal": (),
    "issue": ("issued_at", "project_id"),
    "manifest": ("issued_at", "project_id"),
})
_MSGPACK: Final = msgpack.Encoder()  # Canonical key preimage, length/count-framed by the codec
_P01: Final[RevisionCode] = RevisionCode(kind=RevisionKind.PRELIMINARY, revision=1)

# --- [MODELS] ---------------------------------------------------------------------------


@tagged_union(frozen=True)
class RecordDefect:
    # `RecordDefect` carries every offending value on one closed admission rail.
    tag: Literal["invalid_payload", "invalid_purpose", "invalid_revision", "aggregate"] = tag()
    invalid_payload: tuple[str, ...] = case()  # Failing `_PAYLOAD` loc paths
    invalid_purpose: str = case()  # Unrecognized purpose-of-issue token
    invalid_revision: str = case()
    aggregate: tuple["RecordDefect", ...] = case()


class SheetRef(Struct, frozen=True):
    # `SheetRef.data` feeds collation and C2PA lineage; `key` is its `core/plan#PLAN` parent edge.
    key: ContentKey
    data: bytes
    reference: str
    title: str
    discipline: str  # ISO 13567 / NCS discipline code
    suitability: Suitability
    revision: RevisionCode
    fmt: str = "application/pdf"

    @classmethod
    def admitted(
        cls,
        key: ContentKey,
        data: bytes,
        reference: str,
        title: str,
        discipline: str,
        suitability: str,
        revision: str,
        fmt: str = "application/pdf",
        /,
        *,
        documented: frozendict[str, ContainerState] = frozendict(),
    ) -> Result[Self, RegisterFault]:
        admitted = Suitability.parse(suitability, documented=documented)
        revised = RevisionCode.parse(revision)
        match admitted, revised:
            case Result(tag="ok", ok=suit), Result(tag="ok", ok=rev):
                return Ok(cls(key, data, reference, title, discipline, suit, rev, fmt))
            case _:
                severed = RegisterFault.accumulated(admitted.swap().to_option(), revised.swap().to_option())
                return Error(severed.value)

    def canonical(self) -> tuple[object, ...]:
        return (
            self.key.hex,
            self.reference,
            self.title,
            self.discipline,
            self.suitability.tag,
            self.suitability.code,
            self.suitability.state.value,
            self.revision.kind.value,
            self.revision.revision,
            self.revision.version,
            self.fmt,
        )


class TransmittalRecord(Struct, frozen=True):  # ISO 19650 transmittal/issue header, admitted once
    number: str  # Transmittal number/reference
    issuing_party: str  # Sender or issuing appointed party
    recipient: str  # Receiving party
    purpose: Purpose  # Purpose of issue
    revision: RevisionCode = _P01
    issued_at: date | None = None
    project: str = ""
    project_id: str = ""
    subject: str = ""
    description: str = ""
    response_due: date | None = None
    confidentiality: str = ""
    distribution: str = ""
    remarks: str = ""
    copy_recipients: tuple[str, ...] = ()

    @staticmethod
    def of(**payload: Unpack[TransmittalPayload]) -> Result["TransmittalRecord", RecordDefect]:
        # `_PAYLOAD` and the closed purpose/revision parses form the single boundary ingress.
        try:  # Exemption: the pydantic TypeAdapter admission kernel — the one statement seam.
            row = _PAYLOAD.validate_python(payload)
        except ValidationError as fault:
            return Error(RecordDefect(invalid_payload=tuple("/".join(map(str, entry["loc"])) for entry in fault.errors())))
        revision = row.get("revision", "P01")
        purpose = Ok(Purpose(row["purpose"])) if row["purpose"] in _PURPOSES else Error(RecordDefect(invalid_purpose=row["purpose"]))
        revised = RevisionCode.parse(revision).map_error(lambda _fault: RecordDefect(invalid_revision=revision))
        match purpose, revised:
            case Result(tag="ok", ok=admitted), Result(tag="ok", ok=parsed):
                return Ok(
                    TransmittalRecord(
                        number=row["number"],
                        issuing_party=row["issuing_party"],
                        recipient=row["recipient"],
                        purpose=admitted,
                        revision=parsed,
                        issued_at=row.get("issued_at"),
                        project=row.get("project", ""),
                        project_id=row.get("project_id", ""),
                        subject=row.get("subject", ""),
                        description=row.get("description", ""),
                        response_due=row.get("response_due"),
                        confidentiality=row.get("confidentiality", ""),
                        distribution=row.get("distribution", ""),
                        remarks=row.get("remarks", ""),
                        copy_recipients=row.get("copy_recipients", ()),
                    )
                )
            case Result(tag="error", error=left), Result(tag="error", error=right):
                return Error(RecordDefect(aggregate=(left, right)))
            case (Result(tag="error") as err), _:
                return err
            case _, (Result(tag="error") as err):
                return err
            case _ as unreachable:
                assert_never(unreachable)

    def canonical(self) -> tuple[object, ...]:
        return (
            self.number,
            self.issuing_party,
            self.recipient,
            self.purpose.value,
            self.revision.kind.value,
            self.revision.revision,
            self.revision.version,
            self.issued_at.isoformat() if self.issued_at is not None else "",
            self.project,
            self.project_id,
            self.subject,
            self.description,
            self.response_due.isoformat() if self.response_due is not None else "",
            self.confidentiality,
            self.distribution,
            self.remarks,
            self.copy_recipients,
        )


class Deliverable(Struct, frozen=True):  # Common payload every case reads
    # `lane` arrives projected via LanePolicy.of(context) at the composition root — a capacity literal has no owner.
    sheets: tuple[SheetRef, ...]
    register: Register
    record: TransmittalRecord
    lane: LanePolicy

    @property
    def issued_register(self) -> Register:
        match self.register.op:
            case RegisterOp(tag="index"):
                return self.register
            case RegisterOp():
                return structs.replace(self.register, op=RegisterOp.Index())
            case _ as unreachable:
                assert_never(unreachable)

    @property
    def member_keys(self) -> tuple[ContentKey, ...]:
        # Aggregate parents comprise each sheet key plus the issued register's public pre-run key.
        return (*(ref.key for ref in self.sheets), self.issued_register.key)

    @property
    def unregistered(self) -> tuple[str, ...]:
        # Issue-index congruence: constituent sheets the register's revision-latest fold never lists.
        listed = frozenset(container.reference for container in self.issued_register.latest())
        return tuple(sheet.reference for sheet in self.sheets if sheet.reference not in listed)


class AssembleSpec(Struct, frozen=True):
    scheme: Scheme = Scheme.NUP
    geometry: Geometry = Geometry()
    marks: Marks = Marks()


class SealSpec(Struct, frozen=True):
    profile: CodecProfile = CodecProfile(zip_stream=ZipStreamKnobs())
    attachments: tuple[tuple[str, bytes], ...] = ()  # (name, bytes) — register XML / receipts / signed manifests sealed beside the plan-set

    @property
    def sensitive(self) -> bool:
        match self.profile:
            case CodecProfile(tag="seven_z", seven_z=knobs):
                return knobs.password is not None
            case CodecProfile(tag="zip_stream", zip_stream=knobs):
                return knobs.password is not None
            case CodecProfile():
                return False
            case _ as unreachable:
                assert_never(unreachable)

    def resolved(self, /) -> Result[CodecProfile, BoundaryFault]:
        names = ("plan-set.pdf", *(name for name, _ in self.attachments))
        match self.profile:
            case CodecProfile(tag="seven_z", seven_z=knobs):
                return Ok(CodecProfile(seven_z=structs.replace(knobs, names=names)))
            case CodecProfile(tag="zip_stream", zip_stream=knobs):
                return Ok(CodecProfile(zip_stream=structs.replace(knobs, names=names)))
            case CodecProfile(tag=kind):
                return Error(BoundaryFault(config=("transmittal.seal", f"non-container-profile:{kind}")))
            case _ as unreachable:
                assert_never(unreachable)

    def facet(self, /) -> tuple[object, ...]:
        match self.profile:
            case CodecProfile(tag="seven_z", seven_z=knobs):
                keyable = structs.replace(knobs, password=None)
            case CodecProfile(tag="zip_stream", zip_stream=knobs):
                keyable = structs.replace(knobs, password=None)
            case CodecProfile() as profile:
                keyable = profile.keyable
            case _ as unreachable:
                assert_never(unreachable)
        return (
            self.profile.tag,
            keyable,
            tuple(name for name, _ in self.attachments),
            tuple(xxhash.xxh3_128_digest(blob) for _, blob in self.attachments),
        )


class CoverCredential(Struct, frozen=True):
    # `CoverCredential` keeps the optional signer, cover, format, and intent correlated; C2PA lineage rides the cover raster because C2PA cannot sign PDF.
    signer: SignerSpec
    asset: bytes
    fmt: str = "image/png"
    intent: tuple[Intent, DigitalSource | None] = (Intent.CREATE, None)
    title: str = "Transmittal"

    def facet(self, /) -> tuple[object, ...]:
        # `cert_key` identity is fully content-recoverable; only the callback's opaque capability rides `id()` —
        # a process-local foreign-identity axis, sound because the credential rides an `Issue` node whose `bare`
        # admission excludes this facet from cross-process content dedup; durable identity carries only stable components.
        match self.signer:
            case SignerSpec(tag="cert_key", cert_key=signer):
                identity = ("cert_key", signer.alg.value, xxhash.xxh3_128_digest(signer.sign_cert), signer.ta_url)
            case SignerSpec(tag="callback", callback=signer):
                identity = ("callback", signer.alg.value, signer.certs, signer.ta_url, id(signer.sign))
            case _ as unreachable:
                assert_never(unreachable)
        return (identity, xxhash.xxh3_128_digest(self.asset), self.fmt, tuple(item.value if isinstance(item, StrEnum) else item for item in self.intent), self.title)


class IssueSpec(Struct, frozen=True):
    pades: PadesSpec  # Hard legal PAdES-LTA sign policy
    credential: Option[CoverCredential] = Nothing  # Optional soft C2PA leg; `Nothing` selects PAdES-only

    def facet(self, /) -> tuple[object, ...]:
        # Opaque capabilities — the external `sign_digest` callable, `validation_context`, and the foreign
        # placement/appearance payloads — ride process-local `id()`, the foreign-identity memo axis; the `Issue`
        # node always admits `bare`, so this facet never keys cross-process content dedup and only the
        # content-recoverable components (files, chains, vocabulary values) carry identity that must be stable.
        match self.pades.signer:
            case SignerSource(tag="pem", pem=signer):
                identity = ("pem", signer.key_file, signer.cert_file, signer.ca_chain)
            case SignerSource(tag="pkcs12", pkcs12=signer):
                identity = ("pkcs12", signer.pfx_file, signer.ca_chain)
            case SignerSource(tag="external", external=signer):
                identity = ("external", signer.cert_file, signer.ca_chain, signer.bytes_reserved, id(signer.sign_digest))
            case _ as unreachable:
                assert_never(unreachable)
        return (
            identity,
            self.pades.pades_level.value,
            self.pades.field_name,
            (self.pades.placement.tag, id(getattr(self.pades.placement, self.pades.placement.tag))),
            self.pades.certify.value if self.pades.certify is not None else None,
            self.pades.commitment.value if self.pades.commitment is not None else None,
            self.pades.reason,
            self.pades.location,
            self.pades.contact_info,
            self.pades.name,
            self.pades.md_algorithm.value,
            self.pades.tsa_url,
            id(self.pades.validation_context) if self.pades.validation_context is not None else None,
            (self.pades.appearance.tag, id(getattr(self.pades.appearance, self.pades.appearance.tag))),
            tuple(usage.value for usage in self.pades.signer_key_usage),
            self.pades.dss,
            self.pades.source,
            self.credential.map(lambda cover: cover.facet()).to_optional(),
        )


class ManifestSpec(Struct, frozen=True):
    container: Option[ContentKey] = Nothing  # Sealed-container key from a prior `Seal`


class TransmittalEvidence(Struct, frozen=True, gc=False):
    transmittal_id: str
    revision: str
    issued_at: str
    issuing_party: str
    recipient: str
    purpose: str
    project: str
    project_id: str
    subject: str
    description: str
    response_due: str
    confidentiality: str
    distribution: str
    remarks: str
    copy_recipients: tuple[str, ...]
    dominant_suitability: str  # Dominant register suitability projected to the receipt
    stage: TransmittalStage
    sheets: int
    latest_revision: str
    register_complete: bool = True  # Purpose-keyed audit verdict folded once by `close`
    unregistered: int = 0  # Constituent sheets the issued index never lists; a contractual purpose refuses on any
    validation_state: ValidationState = "unsigned"
    container: str = ""  # Sealed container `ContentKey.hex`
    container_members: int = 0
    plan_set: str = ""  # Imposed or signed plan-set `ContentKey.hex`
    scheme: str = ""  # Imposition scheme
    press_sheets: int = 0  # Imposed press-sheet count
    signatures: int = 0  # Bound signature count
    pades_level: str = ""
    signer: str = ""
    signed_at: str = ""
    credential_state: str = ""
    lineage: int = 0  # C2PA ingredient count and sheet-lineage depth
    record_state: RecordValidationState = "unverified"
    record_errors: int = 0  # Failed-assert count when `record_state` is `invalid`
    verdict: Option[ConformanceVerdict] = Nothing  # Leaf forensic edge retained beyond the scalar receipt

    @property
    def signed_valid(self) -> bool:
        return self.validation_state == "valid"

    def facts(self) -> dict[str, object]:
        # Native metrics remain scalar; forensic `verdict` stays on the value.
        return {
            "transmittal_id": self.transmittal_id,
            "revision": self.revision,
            "issued_at": self.issued_at,
            "issuing_party": self.issuing_party,
            "recipient": self.recipient,
            "purpose": self.purpose,
            "project": self.project,
            "project_id": self.project_id,
            "subject": self.subject,
            "description": self.description,
            "response_due": self.response_due,
            "confidentiality": self.confidentiality,
            "distribution": self.distribution,
            "remarks": self.remarks,
            "copy_recipients": self.copy_recipients,
            "dominant_suitability": self.dominant_suitability,
            "stage": self.stage,
            "sheets": self.sheets,
            "latest_revision": self.latest_revision,
            "register_complete": self.register_complete,
            "unregistered": self.unregistered,
            "validation_state": self.validation_state,
            "container": self.container,
            "container_members": self.container_members,
            "plan_set": self.plan_set,
            "scheme": self.scheme,
            "press_sheets": self.press_sheets,
            "signatures": self.signatures,
            "signed_valid": self.signed_valid,
            "pades_level": self.pades_level,
            "signer": self.signer,
            "signed_at": self.signed_at,
            "credential_state": self.credential_state,
            "lineage": self.lineage,
            "record_state": self.record_state,
            "record_errors": self.record_errors,
        }


class RecordBytes(Struct, frozen=True, gc=False):  # C14N record bytes plus Schematron conformance
    data: bytes
    valid: bool
    errors: int


@tagged_union(frozen=True)
class Transmittal:  # Closed issue vocabulary; each owned/composed seam faults through `BoundaryFault`
    tag: Literal["assemble", "seal", "issue", "manifest"] = tag()
    assemble: tuple[Deliverable, AssembleSpec] = case()
    seal: tuple[Deliverable, SealSpec] = case()
    issue: tuple[Deliverable, IssueSpec] = case()
    manifest: tuple[Deliverable, ManifestSpec] = case()

    @classmethod
    def Assemble(cls, deliverable: Deliverable, spec: AssembleSpec = AssembleSpec(), /) -> Self:
        return cls(assemble=(deliverable, spec))

    @classmethod
    def Seal(cls, deliverable: Deliverable, spec: SealSpec = SealSpec(), /) -> Self:
        return cls(seal=(deliverable, spec))

    @classmethod
    def Issue(cls, deliverable: Deliverable, spec: IssueSpec, /) -> Self:
        return cls(issue=(deliverable, spec))

    @classmethod
    def Manifest(cls, deliverable: Deliverable, spec: ManifestSpec = ManifestSpec(), /) -> Self:
        return cls(manifest=(deliverable, spec))

    async def close(self) -> RuntimeRail[tuple[ContentKey, TransmittalEvidence]]:
        # Shared issue gating folds the purpose-keyed register audit once before rendering.
        deliverable = self._deliverable
        audit = deliverable.issued_register.audited(_PURPOSE_AUDIT[deliverable.record.purpose])
        refusal = _gated(self.tag, deliverable, audit)
        if refusal.is_some():
            return Error(refusal.value)
        match self:
            case Transmittal(tag="assemble", assemble=(deliverable, spec)):
                return await self._assembled(deliverable, spec, audit)
            case Transmittal(tag="seal", seal=(deliverable, spec)):
                return await self._sealed(deliverable, spec, audit)
            case Transmittal(tag="issue", issue=(deliverable, spec)):
                return await self._issued(deliverable, spec, audit)
            case Transmittal(tag="manifest", manifest=(deliverable, spec)):
                return await self._manifested(deliverable, spec, audit)
            case _ as unreachable:
                assert_never(unreachable)

    async def _assembled(
        self, deliverable: Deliverable, spec: AssembleSpec, audit: RegisterEvidence, /
    ) -> RuntimeRail[tuple[ContentKey, TransmittalEvidence]]:
        # Validated imposition binds collation once; `receipt.slot` and `planned()` carry its key and press facts.
        routed = (await _collated(deliverable)).bind(
            lambda source: ImposeOp.Impose(source, spec.scheme, spec.geometry, spec.marks).map_error(
                lambda fault: BoundaryFault(config=("transmittal.assemble", fault))
            )
        )
        match routed:
            case Result(tag="error") as err:
                return err
            case Result(tag="ok", ok=op):
                impose = Imposition(op=op, lane=deliverable.lane)
                pressed = await impose.emit().work()
                return pressed.map(lambda receipt: (receipt.slot, _assemble_evidence(deliverable, audit, spec, receipt, impose.planned())))
            case _ as unreachable:
                assert_never(unreachable)

    async def _sealed(
        self, deliverable: Deliverable, spec: SealSpec, audit: RegisterEvidence, /
    ) -> RuntimeRail[tuple[ContentKey, TransmittalEvidence]]:
        collated = await _collated(deliverable)
        profiled = spec.resolved()
        match collated, profiled:
            case (Result(tag="error") as err), _:
                return err
            case _, (Result(tag="error") as err):
                return err
            case Result(tag="ok", ok=plan_set), Result(tag="ok", ok=profile):
                payloads = (plan_set, *(blob for _, blob in spec.attachments))
                constructed = catch(exception=ValueError)(Archive.of)(profile, *payloads, lane=deliverable.lane, parents=deliverable.member_keys).map_error(
                    lambda fault: BoundaryFault(config=("transmittal.seal", str(fault)))
                )
                match constructed:
                    case Result(tag="error") as err:
                        return err
                    case Result(tag="ok", ok=container):
                        sealed = await container.emit().work()
                        return sealed.map(lambda receipt: (receipt.slot, _seal_evidence(deliverable, audit, receipt.slot, len(payloads))))
                    case _ as unreachable:
                        assert_never(unreachable)
            case _ as unreachable:
                assert_never(unreachable)

    async def _issued(
        self, deliverable: Deliverable, spec: IssueSpec, audit: RegisterEvidence, /
    ) -> RuntimeRail[tuple[ContentKey, TransmittalEvidence]]:
        collated = await _collated(deliverable)
        match collated:
            case Result(tag="error") as err:
                return err
            case Result(tag="ok", ok=plan_set):
                # Independent sign rails ride `TaskHandle.return_value` inside one structured task group.
                async with create_task_group() as signs:
                    pades: TaskHandle[RuntimeRail[ArtifactReceipt]] = signs.start_soon(_signed_pades, plan_set, spec.pades)
                    cose: Option[TaskHandle[RuntimeRail[ArtifactReceipt]]] = spec.credential.map(
                        lambda cover: signs.start_soon(_signed_cose, cover, deliverable.sheets)
                    )
                return _folded_signs(deliverable, audit, pades.return_value, cose.map(lambda handle: handle.return_value))
            case _ as unreachable:
                assert_never(unreachable)

    async def _manifested(
        self, deliverable: Deliverable, spec: ManifestSpec, audit: RegisterEvidence, /
    ) -> RuntimeRail[tuple[ContentKey, TransmittalEvidence]]:
        # Issued-register identity is `receipt.slot == node.key` through the uniform producer contract.
        indexed = await deliverable.issued_register.emit().work()
        match indexed:
            case Result(tag="error") as err:
                return err
            case Result(tag="ok", ok=receipt):
                register_key = receipt.slot
                record = await _record(deliverable, audit, register_key.hex, spec.container.map(lambda key: key.hex).default_value(""))
                return record.map(
                    lambda rec: (
                        ContentIdentity.key("transmittal-manifest", rec.data),
                        _manifest_evidence(deliverable, audit, register_key.hex, spec, rec),
                    )
                )
            case _ as unreachable:
                assert_never(unreachable)

    def emit(self, /) -> ArtifactWork:
        return ArtifactWork(
            key=self._key, work=self._emit, parents=self._parents, admission=self._admission, cost=float(len(self._deliverable.sheets) or 1)
        )

    @property
    def _key(self) -> ContentKey:
        # PRE-RUN identity hashes the canonical input preimage, never produced record bytes.
        return ContentIdentity.key(f"transmittal-{self.tag}", _canon(self))

    @property
    def _admission(self) -> Admission:
        return Admission(bare=None) if self._sensitive else Admission(keyed=None)

    @property
    def _sensitive(self) -> bool:
        match self:
            case Transmittal(tag="issue"):
                return True
            case Transmittal(tag="seal", seal=(_, spec)):
                return spec.sensitive
            case Transmittal(tag="assemble") | Transmittal(tag="manifest"):
                return False
            case _ as unreachable:
                assert_never(unreachable)

    @property
    def _deliverable(self) -> Deliverable:
        match self:
            case (
                Transmittal(tag="assemble", assemble=(deliverable, _))
                | Transmittal(tag="seal", seal=(deliverable, _))
                | Transmittal(tag="issue", issue=(deliverable, _))
                | Transmittal(tag="manifest", manifest=(deliverable, _))
            ):
                return deliverable
            case _ as unreachable:
                assert_never(unreachable)

    @property
    def _parents(self) -> tuple[ContentKey, ...]:
        # Member keys are construction-time sheet/register DATA edges.
        return self._deliverable.member_keys

    async def _emit(self) -> RuntimeRail[ArtifactReceipt]:
        # `ArtifactReceipt.Transmittal` keeps the PRE-RUN aggregate key as its slot; produced addresses ride evidence.
        railed = await self.close()
        return railed.map(
            lambda pair: ArtifactReceipt.Transmittal(
                self._key,
                pair[1].transmittal_id,
                pair[1].sheets,
                pair[1].dominant_suitability,
                pair[1].container or pair[1].plan_set,
                pair[1].validation_state,
            )
        )


# --- [OPERATIONS] -----------------------------------------------------------------------


def _gated(stage: TransmittalStage, deliverable: Deliverable, audit: RegisterEvidence, /) -> Option[BoundaryFault]:
    # Shared refusal fold: legal-header, sheet-set, index-congruence, and contractual-audit faults accumulate
    # through the associative `BoundaryFault.combine` so one refusal reports every gate the issue fails.
    record = deliverable.record
    missing = tuple(field for field in _REQUIRED_CLOSE[stage] if not getattr(record, field))
    contractual = record.purpose in _CONTRACTUAL
    held = Block.of_seq((
        Some(BoundaryFault(config=(f"transmittal.{stage}", f"missing:{','.join(missing)}"))) if missing else Nothing,
        Some(BoundaryFault(config=(f"transmittal.{stage}", "empty-sheet-set"))) if not deliverable.sheets else Nothing,
        Some(BoundaryFault(config=(f"transmittal.{stage}", f"unregistered:{','.join(deliverable.unregistered)}")))
        if contractual and deliverable.unregistered
        else Nothing,
        Some(BoundaryFault(config=(f"transmittal.{record.purpose.value}", audit.severed.map(lambda fault: fault.tag).default_value("severed"))))
        if contractual and audit.severed.is_some()
        else Nothing,
    )).choose(lambda fault: fault)
    return Nothing if held.is_empty() else Some(held.reduce(BoundaryFault.combine))


def _canon(op: Transmittal, /) -> bytes:
    # Msgpack frames every field and collection; public policy and foreign-capability identity enter the preimage.
    # Private keys, passphrases, and passwords stay out, and their `Issue`/`Seal` nodes admit `bare` — which is
    # also what licenses the `id()`-keyed opaque components in the `Issue` facets: a bare node never dedups by
    # key across processes, so its preimage needs only within-process distinctness.
    deliverable = op._deliverable
    facet: tuple[object, ...]
    match op:
        case Transmittal(tag="assemble", assemble=(_, spec)):
            facet = (spec,)
        case Transmittal(tag="seal", seal=(_, spec)):
            facet = spec.facet()
        case Transmittal(tag="issue", issue=(_, spec)):
            facet = spec.facet()
        case Transmittal(tag="manifest", manifest=(_, spec)):
            facet = (spec.container.map(lambda key: key.hex).default_value(""),)
        case _ as unreachable:
            assert_never(unreachable)
    return _MSGPACK.encode((op.tag, deliverable.record.canonical(), facet, tuple(sheet.canonical() for sheet in deliverable.sheets), deliverable.issued_register.key.hex))


def _collate(sheets: tuple[SheetRef, ...], record: TransmittalRecord, /) -> bytes:
    # `CAPSULE_OWNER` permits one in-place MuPDF kernel whose nested handles close before deterministic serialization.
    # Fixed record metadata plus `no_new_id=True` excludes wall-clock and random PDF identity.
    with pymupdf.open() as out:
        toc: list[list[object]] = []
        for sheet in sheets:
            toc.append([1, sheet.title or sheet.discipline, out.page_count + 1])  # Bookmark targets this sheet's first page
            with pymupdf.open(stream=sheet.data, filetype="pdf") as src:
                out.insert_pdf(src)
        out.set_toc(toc)
        out.set_metadata({
            "title": record.number,
            "author": record.issuing_party,
            "subject": record.purpose.value,
            "keywords": record.project,
            "creator": "rasm-artifacts",
            "producer": "rasm-artifacts",
        })
        return out.tobytes(garbage=4, deflate=True, use_objstms=1, no_new_id=True)


async def _collated(deliverable: Deliverable, /) -> RuntimeRail[bytes]:
    # GIL-releasing MuPDF work crosses the instance lane once and preserves its returned rail.
    return await deliverable.lane.offload(Kernel.of(_collate, KernelTrait.HOSTILE), deliverable.sheets, deliverable.record)


@cache
def _record_schema() -> bytes:
    # Immutable Schematron bytes derive one `sch:assert` per `_REQUIRED_RECORD` row.
    # Each render compiles its own validator because provider diagnostic state is not thread-re-entrant.
    sch = lambda local: etree.QName(_SCHEMATRON_NS, local)
    schema = etree.Element(sch("schema"), nsmap={"sch": _SCHEMATRON_NS})
    etree.SubElement(schema, sch("ns"), prefix="t", uri=_NS)
    rule = etree.SubElement(etree.SubElement(schema, sch("pattern")), sch("rule"), context="t:Header")
    for local in _REQUIRED_RECORD:
        etree.SubElement(rule, sch("assert"), test=f"t:{local} != ''").text = f"missing mandated transmittal-record {local}"
    return etree.tostring(schema)


def _record_xml(deliverable: Deliverable, audit: RegisterEvidence, register_key: str, container_key: str, /) -> RecordBytes:
    # `SubElement.text`/`set` escape every dynamic value before Schematron validation.
    # `c14n2` serialization makes the validated legal record byte-reproducible.
    record = deliverable.record
    qname = lambda local: etree.QName(_NS, local)
    root = etree.Element(qname("Transmittal"), nsmap={None: _NS})
    header = etree.SubElement(root, qname("Header"))
    for field, value in (
        ("number", record.number),
        ("revision", record.revision.render()),
        ("issuedAt", record.issued_at.isoformat() if record.issued_at is not None else ""),
        ("issuingParty", record.issuing_party),
        ("recipient", record.recipient),
        ("purpose", record.purpose.value),
        ("project", record.project),
        ("projectReference", record.project_id),
        ("subject", record.subject),
        ("description", record.description),
        ("responseDue", record.response_due.isoformat() if record.response_due is not None else ""),
        ("confidentiality", record.confidentiality),
        ("distribution", record.distribution),
        ("remarks", record.remarks),
        ("dominantSuitability", audit.dominant_suitability),
        ("latestRevision", audit.latest_revision),
        ("registerKey", register_key),
        ("containerKey", container_key),
    ):
        etree.SubElement(header, qname(field)).text = value
    copies = etree.SubElement(header, qname("copyRecipients"))
    for recipient in record.copy_recipients:
        etree.SubElement(copies, qname("recipient")).text = recipient
    containers = etree.SubElement(root, qname("Containers"))
    for sheet in deliverable.sheets:
        node = etree.SubElement(
            containers,
            qname("Container"),
            reference=sheet.reference,
            key=sheet.key.hex,
            suitability=sheet.suitability.code,
            revision=sheet.revision.render(),
        )
        for field, value in (("title", sheet.title), ("discipline", sheet.discipline), ("format", sheet.fmt)):
            etree.SubElement(node, qname(field)).text = value
    schema = isoschematron.Schematron(etree.fromstring(_record_schema()), store_report=True)  # per-call: not thread-re-entrant on error_log
    valid = schema.validate(root)
    errors = int(schema.validation_report.xpath("count(//svrl:failed-assert)", namespaces={"svrl": _SVRL_NS}))
    return RecordBytes(etree.tostring(root, method="c14n2"), valid, errors)


async def _record(deliverable: Deliverable, audit: RegisterEvidence, register_key: str, container_key: str, /) -> RuntimeRail[RecordBytes]:
    return await deliverable.lane.offload(Kernel.of(_record_xml, KernelTrait.HOSTILE), deliverable, audit, register_key, container_key)


def _lineage(cover: CoverCredential, sheets: tuple[SheetRef, ...], /) -> Manifest:
    # Each sheet becomes a `componentOf` ingredient carrying its content key as the `xmp.did:`/`xmp.iid:`
    # identity pair — the same spelling `Manifest.with_parents` mints for `parentOf` rows.
    ingredients = tuple(
        Ingredient.Stream(
            IngredientDefinition(
                title=sheet.title,
                format=sheet.fmt,
                relationship="componentOf",
                document_id=f"xmp.did:{sheet.key.hex}",
                instance_id=f"xmp.iid:{sheet.key.hex}",
            ),
            sheet.fmt,
            sheet.data,
        )
        for sheet in sheets
    )
    return Manifest(
        definition=ManifestDefinition(title=cover.title, format=cover.fmt, claim_generator="rasm-artifacts"),
        actions=(ActionDefinition(action="c2pa.published"),),
        ingredients=ingredients,
        intent=cover.intent,
    )


async def _signed_pades(plan_set: bytes, pades: PadesSpec, /) -> RuntimeRail[ArtifactReceipt]:
    # PAdES remains the hard legal child; its producer owns TSA retry and offload.
    return await Conformance.Sign(plan_set, pades).emit().work()


async def _signed_cose(cover: CoverCredential, sheets: tuple[SheetRef, ...], /) -> RuntimeRail[ArtifactReceipt]:
    # C2PA remains the soft provenance child; its producer owns remote-manifest retry.
    return await Provenance.Sign(cover.asset, CoseSpec(manifest=_lineage(cover, sheets), fmt=cover.fmt, signer=cover.signer)).emit().work()


def _folded_signs(
    deliverable: Deliverable, audit: RegisterEvidence, pades: RuntimeRail[ArtifactReceipt], cose: Option[RuntimeRail[ArtifactReceipt]], /
) -> RuntimeRail[tuple[ContentKey, TransmittalEvidence]]:
    # PAdES failure aborts issue; optional C2PA absence, failure, and reported state remain distinguishable.
    match pades:
        case Result(tag="error") as err:
            return err
        case Result(tag="ok", ok=ArtifactReceipt(tag="verdict", verdict=(key, verdict))):
            credential_state, credential_present = _credential(cose)
            return Ok((key, _issue_evidence(deliverable, audit, key, verdict, credential_state, credential_present)))
        case Result(tag="ok", ok=receipt):
            return Error(BoundaryFault(config=("transmittal.issue", f"expected-verdict-receipt:{receipt.tag}")))
        case _ as unreachable:
            assert_never(unreachable)


def _credential(cose: Option[RuntimeRail[ArtifactReceipt]], /) -> tuple[str, bool]:
    match cose:
        case Option(tag="none"):
            return "unsigned", False
        case Option(tag="some", some=Result(tag="error")):
            return "failed", False
        case Option(tag="some", some=Result(tag="ok", ok=ArtifactReceipt(tag="credential", credential=(_, _, _, _, state)))):
            return state, True
        case Option(tag="some", some=Result(tag="ok")):
            return "failed", False
        case _ as unreachable:
            assert_never(unreachable)


def _assemble_evidence(
    deliverable: Deliverable, audit: RegisterEvidence, spec: AssembleSpec, receipt: ArtifactReceipt, plan: Option[ImposedPlan], /
) -> TransmittalEvidence:
    # `planned()` is `Some` for the Impose op; a provider-owned fold (plan sheets None) reads the MEASURED imposed
    # page count off the terminal receipt instead of publishing a fabricated zero press fact.
    measured = receipt.egress[2] if receipt.tag == "egress" else receipt.pdf[2]
    return structs.replace(
        _base_evidence(deliverable, audit, "assemble"),
        plan_set=receipt.slot.hex,
        scheme=spec.scheme.value,
        press_sheets=plan.map(lambda p: p.sheets if p.sheets is not None else measured).default_value(measured),
        signatures=plan.map(lambda p: p.signatures if p.signatures is not None else measured).default_value(measured),
    )


def _seal_evidence(deliverable: Deliverable, audit: RegisterEvidence, key: ContentKey, members: int, /) -> TransmittalEvidence:
    return structs.replace(_base_evidence(deliverable, audit, "seal"), container=key.hex, container_members=members)


def _manifest_evidence(
    deliverable: Deliverable, audit: RegisterEvidence, register_key: str, spec: ManifestSpec, record: RecordBytes, /
) -> TransmittalEvidence:
    return structs.replace(
        _base_evidence(deliverable, audit, "manifest"),
        container=spec.container.map(lambda key: key.hex).default_value(""),
        plan_set=register_key,
        record_state="valid" if record.valid else "invalid",
        record_errors=record.errors,
    )


def _issue_evidence(
    deliverable: Deliverable,
    audit: RegisterEvidence,
    plan_set: ContentKey,
    verdict: ConformanceVerdict,
    credential_state: str,
    credential_present: bool,
    /,
) -> TransmittalEvidence:
    # Credential presence controls lineage depth; PAdES alone controls legal validity.
    return structs.replace(
        _base_evidence(deliverable, audit, "issue"),
        plan_set=plan_set.hex,
        validation_state="valid" if verdict.signature_valid and verdict.trusted else "invalid",
        pades_level=verdict.pades_level,
        signer=verdict.signer_subject,
        signed_at=verdict.signed_at,
        credential_state=credential_state,
        lineage=len(deliverable.sheets) if credential_present else 0,
        verdict=Some(verdict),
    )


def _base_evidence(deliverable: Deliverable, audit: RegisterEvidence, stage: TransmittalStage, /) -> TransmittalEvidence:
    # Every stage reuses the close prologue's single record/index audit fold.
    record = deliverable.record
    return TransmittalEvidence(
        transmittal_id=record.number,
        revision=record.revision.render(),
        issued_at=record.issued_at.isoformat() if record.issued_at is not None else "",
        issuing_party=record.issuing_party,
        recipient=record.recipient,
        purpose=record.purpose.value,
        project=record.project,
        project_id=record.project_id,
        subject=record.subject,
        description=record.description,
        response_due=record.response_due.isoformat() if record.response_due is not None else "",
        confidentiality=record.confidentiality,
        distribution=record.distribution,
        remarks=record.remarks,
        copy_recipients=record.copy_recipients,
        dominant_suitability=audit.dominant_suitability,
        stage=stage,
        sheets=len(deliverable.sheets),
        latest_revision=audit.latest_revision,
        register_complete=audit.complete,
        unregistered=len(deliverable.unregistered),
    )
```

## [03]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)

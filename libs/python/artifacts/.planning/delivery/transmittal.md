# [PY_ARTIFACTS_TRANSMITTAL]

`Transmittal` is the ISO 19650 transmittal / issue-for-construction close at the delivery boundary — the assembly orchestrator that folds an issued sheet set into a press-imposed plan-set, a content-addressed transmittal container, a legally PAdES-signed and provenance-credentialed record, and a formal transmittal-record manifest, composing FOUR sibling owners at the wire. It IS the closed `expression.tagged_union` — its `Assemble`/`Seal`/`Issue`/`Manifest` cases own the dispatch, the async entry, AND every case body directly, never a one-field wrapper over a separate op union — each discriminating by its own typed `(Deliverable, spec)` payload: `Assemble` composes `composition/imposition#IMPOSE` into a press form, `Seal` composes `package/archive#ARCHIVE` `Bundle` into one content-addressed transmittal blob, `Issue` composes `exchange/conformance#CONFORMANCE` PAdES-LTA signing CONCURRENTLY with `exchange/credential#CREDENTIAL` C2PA sheet-lineage signing inside one `anyio` task group, and `Manifest` composes `delivery/register#REGISTER` as the issued index and builds the ISO 19650 transmittal-record XML — Schematron-validated and C14N-canonical — each case folding once into the shared `TransmittalEvidence` the `close` projection returns, never a per-stage evidence bag or a per-product transmittal-builder family. This owner authors no sheet, drawing, IFC, or QTO, and re-implements no imposition/archive/crypto engine — it composes the constituent producers' outputs into the ISO 19650 issue and closes it.

`TransmittalRecord.of` admits the raw client header through the module-level `_PAYLOAD` `TypeAdapter` EXACTLY ONCE, so the interior is total and never re-parses a stringly field. `close` is `async` and mirrors the `exchange/conformance#CONFORMANCE`/`exchange/credential#CREDENTIAL` `close` shape (`RuntimeRail[tuple[ContentKey, TransmittalEvidence]]`) but is the ORCHESTRATOR variant: it THREADS the composed sibling owners' already-railed async results through ROP `bind`/`map` and offloads only its OWN native seams (the `pymupdf` collation, the `lxml` serialize) through the runtime thread lane, so a composed owner's `Result.Error` propagates without a rail-to-raise-to-rail erasure. Transient TSA / remote-manifest retry is NOT re-woven here — it lives in the composed `Conformance`/`Provenance` sign legs the `Issue` arm binds; the transmittal owns only the concurrency (the `anyio` task group over the two independent signs). This owner schedules into `core/plan#PLAN` `ArtifactPipeline` as ONE `ArtifactWork` whose `parents` are the constituent sheet `ContentKey`s, and each op contributes the new `core/receipt#RECEIPT` `ArtifactReceipt.Transmittal` case the `[07]-[SEAM_UNIFICATION]` target admits, never a parallel receipt rail.

## [01]-[INDEX]

- [01]-[TRANSMITTAL]: the ISO 19650 transmittal / issue-for-construction orchestrator over the closed `Transmittal` `tagged_union` — `Assemble` the press form, `Seal` the container, `Issue` the concurrent PAdES + C2PA sign, `Manifest` the issued index + record XML — folded once into the shared `TransmittalEvidence`, closed async and scheduled as one `core/plan#PLAN` `ArtifactWork` minting the new `ArtifactReceipt.Transmittal` case.

## [02]-[TRANSMITTAL]

- Owner: `Transmittal` the one delivery-issue owner AND the closed `expression.tagged_union` discriminating over its own typed `(Deliverable, spec)` payload — `Assemble`, `Seal`, `Issue`, `Manifest` — matched by one total `match`/`case` in `close`, the `@classmethod` factories returning `Self` and the `_assembled`/`_sealed`/`_issued`/`_manifested` case-body methods all folded onto the union, never a one-field wrapper over a separate op union, a `stage: str` discriminant beside an optional-field bag, or a free module function reconstructing a case body outside the owner. `Deliverable` is the shared payload every case reads — `sheets` the constituent issued sheets (bytes for collation and C2PA lineage, keys for the CPM parents), `register: Register` the composed `delivery/register#REGISTER` issued index the transmittal reads `audited()` off for the dominant suitability / latest revision / container count, `record: TransmittalRecord` the ISO 19650 header. `SheetRef` is the one frozen constituent-sheet value, never a bytes tuple decoded by index; `TransmittalRecord` the issue header admitted once through the `_PAYLOAD` `TypeAdapter` at `TransmittalRecord.of`. `pymupdf` owns the deterministic plan-set collation, sheet-index bookmarks, and issue seal; `lxml.etree` the escaped-and-canonical transmittal-record XML plus the `isoschematron.Schematron` oracle; `composition/imposition#IMPOSE` the press form, `package/archive#ARCHIVE` the container, `exchange/conformance#CONFORMANCE` the PAdES close, `exchange/credential#CREDENTIAL` the C2PA lineage — no transmittal engine is admitted, so the ISO 19650 issue algebra is this owner's composition over those engines, never a re-implemented imposer, archiver, or signer.
- Cases: `Transmittal` cases matched by one total `match`, each folding once into the shared `TransmittalEvidence` and returning `RuntimeRail[tuple[ContentKey, TransmittalEvidence]]` — never a per-product transmittal-builder sibling, never a per-stage `_emit`. `Assemble(deliverable, AssembleSpec)` collates the `SheetRef.data` through `_collate` then composes `Imposition.of()` for the imposed press-form key and `.planned()` for the `ImposedPlan` metrics riding the evidence. `Seal(deliverable, SealSpec)` collates then composes `Bundle.of(...).pack()` to fold the plan-set plus `spec.attachments` into one content-addressed `ZIP_STREAM`/`SEVEN_Z` container. `Issue(deliverable, IssueSpec)` collates then drives `Conformance.close()` — the hard PAdES-LTA legal signature — CONCURRENTLY with the optional `Provenance.close()` — the soft C2PA cover-lineage sign carrying each `SheetRef` as an `Ingredient` — inside one `anyio.create_task_group`. `Manifest(deliverable, ManifestSpec)` drives `register.emit()` for the issued-index receipt whose `slot` IS the register key, then builds the transmittal-record XML through `_record_xml`, Schematron-validated (`record_valid`/`record_errors`) and `ContentIdentity.of`-minted over the C14N bytes.
- Entry: `close` is the one total `match` over `self` dispatching to the four async case-body methods that THREAD the composed sibling rails through ROP — no synchronous `_run` offloaded once (the sign-leaf shape), because the composed owners are already async and railed. A self-contained async `emit` thunk is the `core/plan#PLAN` `Work[ArtifactReceipt]` producer the transmittal schedules as ONE `ArtifactWork` leaf (its `parents` the constituent sheet keys), driving `close` and projecting the pair onto the `ArtifactReceipt.Transmittal` case with the PRE-RUN aggregate key as the slot. `TransmittalRecord.of` narrows a bad payload or purpose to `RecordDefect`, and `async_boundary` narrows the own native seams on `_FAULTS` so each fault discriminates into its own `BoundaryFault` case.
- Auto: `_assembled` binds `_collated` (its own `pymupdf` seam offloaded through the runtime lane) then `Imposition.of()` through `map`. `_sealed` binds `_collated` then `Bundle.pack()`. `_issued` binds `_collated`, opens ONE `anyio.create_task_group` starting the PAdES sign always and the C2PA sign only when a `cover` and `credential_signer` are supplied (each child returns its `RuntimeRail` so the `TaskHandle.return_value` carries it — the CHILD_CARRIER law, never a sink list), then folds through `_folded_signs`: a PAdES `Result.Error` aborts the whole issue while an absent-or-failed C2PA sign folds to `Nothing` (soft provenance), never `asyncio.gather`. `_manifested` binds `register.emit()` then `_record`. `_collate` is ONE imperative measured kernel threading ONE owned `pymupdf.Document` mutated in place across N `insert_pdf` inserts (every handle `with`-bracketed), then `set_toc` authoring the sheet-index bookmarks, `set_metadata` sealing the issue header from FIXED record fields (no wall-clock), and `tobytes(no_new_id=True)` suppressing the random `/ID` so the collated plan-set stays byte-reproducible run-to-run. `_record_xml` builds every dynamic value through `SubElement.text`/`set` (never an f-string splice), validates against the owned `_record_schema` (a per-render validator so the bounded `_GATE` threads never race one `error_log`), and serializes `c14n2` bytes.
- Receipt: `close` returns the rich `TransmittalEvidence` as the `(ContentKey, TransmittalEvidence)` pair, mirroring the conformance and credential closes — the full issue picture (transmittal id / revision / issue date / party / recipient / purpose, issued-sheet and press-signature counts, imposition scheme, container key and member count, folded PAdES level / signer / signing time / validity, C2PA state and lineage depth, the record Schematron conformance evidence-only, the dominant suitability / latest revision, the leaf `ConformanceVerdict` forensic edge). `emit` projects it onto `ArtifactReceipt.Transmittal(key, transmittal_id, sheets, suitability, container-or-plan_set, validation_state)` — five settled scalars projected exactly as `ArtifactReceipt.Verdict` is minted from the conformance pair, so `receipt.py` imports no `TransmittalEvidence` and the case stays flat scalars, the delivery-issue evidence a CASE on the one `ArtifactReceipt` family the `[07]-[SEAM_UNIFICATION]` target admits, never a parallel receipt rail. `validation_state` plus the native-int counts are the observable issue facts the runtime `observability/metrics` `MeterProvider` reads off the minted receipt.
- Packages: `expression` (the `Transmittal`/`RecordDefect` unions, the `Result`/`Option` rails and `.bind`/`.map`/`.to_option`/`Option.of_optional` the composed-rail threading and soft-credential fold); `msgspec` (the frozen value objects, `structs.asdict`/`replace` the evidence fill); `pydantic` (`TypeAdapter` the `_PAYLOAD` admission, `ValidationError` mapped to `RecordDefect`); `anyio` (`create_task_group` the concurrent-sign failure boundary, the runtime thread lane the off-loop native seam); `pymupdf` (the deterministic navigable, metadata-sealed plan-set collation, `lazy import`-deferred so an unused path never pays the MuPDF load); `lxml.etree` (the escaped-and-canonical transmittal-record XML plus `isoschematron.Schematron`, `lazy`-deferred); `functools.cache` (the compiled-once record Schematron bytes); runtime (`identity.ContentIdentity`/`ContentKey`, `faults.RuntimeRail`/`async_boundary`); the composed owners `composition/imposition#IMPOSE`, `package/archive#ARCHIVE` + `package/codec#CODEC`, `exchange/conformance#CONFORMANCE` (the `SignSpec` aliased `PadesSpec`, the sign leg owning the transient-TSA retry), `exchange/credential#CREDENTIAL` (the `SignSpec` aliased `CoseSpec`, the sign leg owning the transient-remote-manifest retry), and `delivery/register#REGISTER` (`Register` the issued index); `core/receipt#RECEIPT` (`ArtifactReceipt.Transmittal`). No new external library — every engine is composed.
- Growth: a new issue product is one `Transmittal` case with its payload and one `close` arm plus one case-body method (the `assert_never` tail breaking the match until it exists); a new imposition scheme rides the composed `composition/imposition#IMPOSE` `Scheme`, a new container algorithm the composed `package/archive#ARCHIVE` `CompressionAlgo`, a new PAdES level or signer the composed `exchange/conformance#CONFORMANCE` vocabulary, a new C2PA ingredient relationship or intent the composed `exchange/credential#CREDENTIAL` vocabulary — each for free; a new record-header field is one `TransmittalRecord` field flowing into the `_PAYLOAD` gate, the record XML, and the evidence; a new sealed attachment one `(name, bytes)` row on `SealSpec.attachments`; a new evidence fact one `TransmittalEvidence` field (and, when contract-settled, one scalar on the shared `ArtifactReceipt.Transmittal` case); a new signed-verdict fold rule one arm in `_folded_signs`. Zero new surface — the owner grows by case, composed-owner vocabulary, and evidence field, never by method family.
- Boundary: no sheet authoring (`composition/sheet#SHEET` owns the title block; the transmittal reads the `SheetRef`), no drawing (`drawing/*` owns it), no imposition placement (`composition/imposition#IMPOSE` owns the `Scheme`/`Placement` fold; the transmittal composes `ImposeOp`), no container codec (`package/archive#ARCHIVE`/`package/codec#CODEC` own the `Bundle`), no crypto engine (`exchange/conformance#CONFORMANCE` owns PAdES, `exchange/credential#CREDENTIAL` owns C2PA; the transmittal composes their `close`), no IFC/QTO (`csharp:Rasm.Bim` owns them). A stringly `purpose`/`suitability` where the `_PAYLOAD` gate and composed owners type them, a concatenated-string record XML where `SubElement` escapes the tree, a re-implemented imposer/archiver/signer where the owners are composed, a `pymupdf` fold that recopies the whole buffer per sheet where the ONE in-place kernel threads one handle, a random-`/ID` or wall-clock stamp that churns the content key where `no_new_id=True` and FIXED record fields keep it reproducible, a per-sheet PDF C2PA sign where `c2pa` cannot sign PDFs and each sheet rides as an `Ingredient` instead, an `asyncio.gather` where the `anyio` task group is the failure boundary, a hard-abort C2PA sign where the provenance folds soft, a second retry weave on the CPU fold where the transient retry lives in the composed sign legs, a rail-to-raise-to-rail unwrap where the case body binds the composed `Result`, and a parallel receipt rail are each foreclosed by the correct form above.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
from collections.abc import Iterable
from enum import StrEnum
from functools import cache
from typing import Final, Literal, NotRequired, ReadOnly, Required, Self, TypedDict, Unpack, assert_never

from anyio import create_task_group
from anyio.abc import TaskHandle
from expression import Error, Ok, Option, Result, case, tag, tagged_union
from msgspec import Struct, structs
from pydantic import TypeAdapter, ValidationError

from rasm.runtime.identity import CANONICAL_POLICY, ContentIdentity, ContentKey
from rasm.runtime.lanes import LanePolicy, Modality
from rasm.runtime.resilience import RetryClass
from rasm.runtime.faults import RuntimeRail, async_boundary

from artifacts.composition.imposition import Geometry, ImposeOp, ImposedPlan, Imposition, Marks, Scheme
from artifacts.delivery.register import Register
from artifacts.exchange.conformance import Conformance, ConformanceVerdict
from artifacts.exchange.conformance import SignSpec as PadesSpec  # collides with credential.SignSpec — aliased at the seam, carries the PadesLevel/SignerSource the caller fills
from artifacts.exchange.credential import (
    ActionDefinition,
    CredentialEvidence,
    DigitalSource,
    Ingredient,
    IngredientDefinition,
    Intent,
    Manifest,
    ManifestDefinition,
    Provenance,
    SignerSpec,
)
from artifacts.exchange.credential import SignSpec as CoseSpec  # the C2PA sign spec — aliased against the PAdES one
from artifacts.core.plan import Admission, ArtifactWork
from artifacts.core.receipt import ArtifactReceipt
from artifacts.package.archive import SevenZKnobs, ZipStreamKnobs
from artifacts.package.codec import Bundle, CodecProfile, CompressionAlgo

lazy import pymupdf  # the plan-set collation engine; cold native MuPDF, deferred
lazy from lxml import (
    etree,
    isoschematron,
)  # the transmittal-record XML builder + conformance oracle; cold, deferred to the `Manifest` serialize

# --- [TYPES] ----------------------------------------------------------------------------


class Purpose(StrEnum):  # the ISO 19650 purpose-of-issue vocabulary; a stringly free-text purpose is the deleted form
    COORDINATION = "coordination"  # issued for spatial coordination (S1)
    INFORMATION = "information"  # issued for information (S2)
    REVIEW_COMMENT = "review-comment"  # issued for review and comment (S3)
    REVIEW_AUTHORIZE = "review-authorize"  # issued for review and authorization (S4)
    CONSTRUCTION = "construction"  # issued for construction (published, contractual)
    TENDER = "tender"  # issued for tender/bid
    APPROVAL = "approval"  # issued for statutory approval
    AS_BUILT = "as-built"  # as-constructed record issue


# --- [MODELS] ---------------------------------------------------------------------------


@tagged_union(frozen=True)
class RecordDefect:
    # the closed record-admission fault carrying its offending value, declared first because `TransmittalRecord.of` returns it; never a bare `str`.
    tag: Literal["invalid_payload", "invalid_purpose"] = tag()
    invalid_payload: str = case()  # the failing `_PAYLOAD` field locs
    invalid_purpose: str = case()  # the unrecognized purpose-of-issue token


class SheetRef(Struct, frozen=True):
    # one constituent issued sheet — bytes feed the collation AND the C2PA lineage, the key is the CPM parent edge `core/plan#PLAN` orders the leaf behind.
    key: ContentKey
    data: bytes
    title: str
    discipline: str  # the ISO 13567 / NCS discipline code
    suitability: str  # the ISO 19650 suitability code (`Suitability.of`-parseable at the register)
    revision: str  # the ISO 19650 revision code
    fmt: str = "application/pdf"


class TransmittalPayload(TypedDict, closed=True):  # the raw client record header — strings, admitted once
    number: Required[ReadOnly[str]]
    issuing_party: Required[ReadOnly[str]]
    recipient: Required[ReadOnly[str]]
    purpose: Required[ReadOnly[str]]
    revision: NotRequired[ReadOnly[str]]
    issued_at: NotRequired[ReadOnly[str]]
    project: NotRequired[ReadOnly[str]]
    project_id: NotRequired[ReadOnly[str]]


class TransmittalRecord(Struct, frozen=True):  # the ISO 19650 transmittal/issue header, admitted once
    number: str  # the transmittal number/reference
    issuing_party: str  # the sender (issuing appointed party)
    recipient: str  # the receiving party
    purpose: Purpose  # the purpose of issue
    revision: str = "P01"  # the transmittal revision code
    issued_at: str = ""  # the ISO-8601 issue date
    project: str = ""
    project_id: str = ""

    @staticmethod
    def of(**payload: Unpack[TransmittalPayload]) -> Result["TransmittalRecord", RecordDefect]:
        # the one boundary ingress: the `_PAYLOAD` TypeAdapter shape gate plus the purpose parse, so the interior is total.
        try:
            row = _PAYLOAD.validate_python(payload)
        except ValidationError as fault:
            return Error(RecordDefect(invalid_payload=str([error["loc"] for error in fault.errors()])))
        if row["purpose"] not in _PURPOSES:  # the derived value set — never the `_value2member_map_` private-dunder probe
            return Error(RecordDefect(invalid_purpose=row["purpose"]))
        return Ok(
            TransmittalRecord(
                number=row["number"],
                issuing_party=row["issuing_party"],
                recipient=row["recipient"],
                purpose=Purpose(row["purpose"]),
                revision=row.get("revision", "P01"),
                issued_at=row.get("issued_at", ""),
                project=row.get("project", ""),
                project_id=row.get("project_id", ""),
            )
        )


class Deliverable(Struct, frozen=True):  # the common payload every case reads
    sheets: tuple[SheetRef, ...]
    register: Register
    record: TransmittalRecord

    @property
    def member_keys(self) -> tuple[ContentKey, ...]:
        # the aggregate node's parents: each member sheet's content key plus the register node key
        return (*(ref.key for ref in self.sheets), self.register._key)


class AssembleSpec(Struct, frozen=True):
    scheme: Scheme = Scheme.NUP
    geometry: Geometry = Geometry()
    marks: Marks = Marks()


class SealSpec(Struct, frozen=True):
    algo: CompressionAlgo = CompressionAlgo.ZIP_STREAM  # the byte-reproducible `_EPOCH`-stamped container default
    profile: CodecProfile | None = None  # None -> the archive default profile for `algo`
    attachments: tuple[tuple[str, bytes], ...] = ()  # (name, bytes) — register XML / receipts / signed manifests sealed beside the plan-set
    password: str | None = None  # the container encryption pass (WinZip-AES for ZIP, header-crypt for 7z)


class IssueSpec(Struct, frozen=True):
    pades: PadesSpec  # the hard legal PAdES-LTA sign spec (signer/level/tsa/certify/commitment)
    credential_signer: SignerSpec | None = None  # the optional soft C2PA signer (cert/callback); None -> PAdES-only
    cover: bytes = b""  # the c2pa-signable cover asset (PDF is read-only for c2pa; the lineage rides a cover raster)
    cover_fmt: str = "image/png"
    intent: tuple[Intent, DigitalSource | None] = (Intent.CREATE, None)
    title: str = "Transmittal"


class ManifestSpec(Struct, frozen=True):
    container: str = ""  # the sealed-container `ContentKey.hex` referenced in the record (from a prior `Seal`)


class TransmittalEvidence(Struct, frozen=True, gc=False):
    transmittal_id: str
    revision: str
    issued_at: str
    issuing_party: str
    recipient: str
    suitability: str  # the purpose-of-issue suitability code the receipt carries
    stage: str  # which product this evidence closes (assemble/seal/issue/manifest)
    sheets: int
    dominant_suitability: str
    latest_revision: str
    validation_state: str = "unsigned"
    container: str = ""  # the sealed container `ContentKey.hex` (Seal)
    container_members: int = 0
    plan_set: str = ""  # the imposed / signed plan-set `ContentKey.hex` (Assemble/Issue)
    scheme: str = ""  # the imposition scheme (Assemble)
    press_sheets: int = 0  # the imposed press-sheet count (Assemble)
    signatures: int = 0  # the bound signature count (Assemble)
    signed_valid: bool = False
    pades_level: str = ""
    signer: str = ""
    signed_at: str = ""
    credential_state: str = ""
    lineage: int = 0  # the C2PA ingredient count = sheet-lineage depth (Issue)
    record_valid: bool = True  # the transmittal-record Schematron conformance (Manifest); True where no record is produced
    record_errors: int = 0  # the record's failed-assert count when `record_valid` is False
    verdict: ConformanceVerdict | None = None  # the leaf forensic edge the caller reads, the receipt projects the scalar

    def facts(self) -> dict[str, object]:
        return structs.asdict(self)


class RecordBytes(Struct, frozen=True, gc=False):  # the C14N record bytes plus the Schematron conformance the manifest evidence folds
    data: bytes
    valid: bool
    errors: int


@tagged_union(frozen=True)
class Transmittal:  # the closed issue vocabulary; the fault rail is `BoundaryFault` at each composed and own seam
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

    async def _closed(self) -> RuntimeRail[tuple[ContentKey, TransmittalEvidence]]:
        # thread the composed sibling rails through ROP, offload own native seams; a composed `Result.Error` propagates by `bind`/`map`, never a rail-to-raise unwrap.
        match self:
            case Transmittal(tag="assemble", assemble=(deliverable, spec)):
                return await self._assembled(deliverable, spec)
            case Transmittal(tag="seal", seal=(deliverable, spec)):
                return await self._sealed(deliverable, spec)
            case Transmittal(tag="issue", issue=(deliverable, spec)):
                return await self._issued(deliverable, spec)
            case Transmittal(tag="manifest", manifest=(deliverable, spec)):
                return await self._manifested(deliverable, spec)
            case _ as unreachable:
                assert_never(unreachable)

    async def _assembled(self, deliverable: Deliverable, spec: AssembleSpec, /) -> RuntimeRail[tuple[ContentKey, TransmittalEvidence]]:
        collated = await _collated(deliverable.sheets, deliverable.record)
        match collated:
            case Error() as err:
                return err
            case Ok(source):
                impose = Imposition(ImposeOp.Impose(source, spec.scheme, spec.geometry, spec.marks))
                imposed = await impose.of()
                plan = impose.planned()  # `Some(ImposedPlan)` for the impose op — the press metrics riding the evidence
                return imposed.map(lambda key: (key, _assemble_evidence(deliverable, spec, key, plan)))
            case _ as unreachable:
                assert_never(unreachable)

    async def _sealed(self, deliverable: Deliverable, spec: SealSpec, /) -> RuntimeRail[tuple[ContentKey, TransmittalEvidence]]:
        collated = await _collated(deliverable.sheets, deliverable.record)
        match collated:
            case Error() as err:
                return err
            case Ok(plan_set):
                names = tuple(name for name, _ in spec.attachments)
                payloads = (plan_set, *(blob for _, blob in spec.attachments))
                profile = spec.profile if spec.profile is not None else _seal_profile(spec, names)
                sealed = await Bundle.of(spec.algo, *payloads, profile=profile).pack()
                return sealed.map(lambda pair: (pair[0], _seal_evidence(deliverable, pair[0], len(payloads))))
            case _ as unreachable:
                assert_never(unreachable)

    async def _issued(self, deliverable: Deliverable, spec: IssueSpec, /) -> RuntimeRail[tuple[ContentKey, TransmittalEvidence]]:
        collated = await _collated(deliverable.sheets, deliverable.record)
        match collated:
            case Error() as err:
                return err
            case Ok(plan_set):
                # INDEPENDENT signs over the `anyio` task group, never `asyncio.gather`; each terminal rides the `TaskHandle.return_value` (the `CHILD_CARRIER` law), never a sink list.
                async with create_task_group() as signs:
                    pades: TaskHandle[RuntimeRail[tuple[ContentKey, ConformanceVerdict]]] = signs.start_soon(_sign_pades, plan_set, spec)
                    cose: TaskHandle[RuntimeRail[tuple[ContentKey, CredentialEvidence]]] | None = (
                        signs.start_soon(_sign_cose, spec, deliverable.sheets) if spec.cover and spec.credential_signer is not None else None
                    )
                return _folded_signs(deliverable, pades.return_value, cose.return_value if cose is not None else None)
            case _ as unreachable:
                assert_never(unreachable)

    async def _manifested(self, deliverable: Deliverable, spec: ManifestSpec, /) -> RuntimeRail[tuple[ContentKey, TransmittalEvidence]]:
        # the register's ONE content-keyed entry is `emit`; the issued-index key is the receipt's `slot`, never a `register.of()`.
        registered = await deliverable.register.emit()
        match registered:
            case Error() as err:
                return err
            case Ok(receipt):
                register_key = receipt.slot
                record = await _record(deliverable, register_key.hex, spec.container)
                return record.map(
                    lambda rec: (ContentIdentity.of("transmittal.manifest", rec.data), _manifest_evidence(deliverable, register_key.hex, spec, rec))
                )
            case _ as unreachable:
                assert_never(unreachable)

    def emit(self, /) -> "Iterable[ArtifactWork]":
        # ONE aggregate node whose `parents` are the member keys — a re-issued set re-renders only changed members.
        return (
            ArtifactWork(key=self._key, work=self._emit, parents=self._parents, admission=Admission(keyed=None), cost=1.0),
        )

    @property
    def _key(self) -> ContentKey:
        # key-over-INPUT: canonical (tag ⊕ deliverable ⊕ spec) minted PRE-RUN — never a key over the record bytes.
        return ContentIdentity.of(f"transmittal-{self.tag}", self, policy=CANONICAL_POLICY)

    @property
    def _parents(self) -> tuple[ContentKey, ...]:
        # member keys: the deliverable's sheet/register content keys threaded at construction (DATA edges)
        match self:
            case Transmittal(tag="assemble", assemble=(deliverable, _)) | Transmittal(tag="seal", seal=(deliverable, _)) | Transmittal(
                tag="issue", issue=(deliverable, _)
            ) | Transmittal(tag="manifest", manifest=(deliverable, _)):
                return deliverable.member_keys
            case _:
                return ()

    async def _emit(self) -> RuntimeRail[ArtifactReceipt]:
        # project the evidence onto `ArtifactReceipt.Transmittal` with the PRE-RUN aggregate key as the slot; the produced record's address rides the facts, not the elision key.
        railed = await self._closed()
        return railed.map(
            lambda pair: ArtifactReceipt.Transmittal(
                self._key, pair[1].transmittal_id, pair[1].sheets, pair[1].suitability, pair[1].container or pair[1].plan_set, pair[1].validation_state
            )
        )


# --- [CONSTANTS] ------------------------------------------------------------------------

_PAYLOAD: Final = TypeAdapter(TransmittalPayload)
_PURPOSES: Final[frozenset[str]] = frozenset(
    purpose.value for purpose in Purpose
)  # derived membership set the seam admits a raw purpose against, never `_value2member_map_`
_FAULTS: Final[tuple[type[BaseException], ...]] = (RuntimeError, ValueError, KeyError, OSError)
_NS: Final[str] = "https://rasm.dev/schema/iso19650/transmittal"
_SCHEMATRON_NS: Final[str] = "http://purl.oclc.org/dsdl/schematron"
# the mandated transmittal-record header fields — one `sch:assert` per row; a dropped or blank field fails conformance (folded onto `record_valid`).
_REQUIRED_RECORD: Final[tuple[str, ...]] = ("number", "issuingParty", "recipient", "purpose")

# --- [OPERATIONS] -----------------------------------------------------------------------


def _transmittal_raise(fault: object) -> object:
    # terminal collapse at the native seam: an offload fault reconstructs the raise the rail folds.
    raise ValueError(str(fault))


def _collate(sheets: tuple[SheetRef, ...], record: TransmittalRecord, /) -> bytes:
    # ONE imperative measured kernel threading ONE owned `Document` mutated in place across N inserts (every
    # handle closes on exit); `set_toc` authors the sheet-index bookmarks, `set_metadata` seals the issue header
    # from FIXED record fields (no wall-clock) so `no_new_id=True` keeps the plan-set byte-reproducible run-to-run.
    # Exemption: `boundaries.md CAPSULE_OWNER` binary-kernel in-place PDF assembly over the live MuPDF handle.
    with pymupdf.open() as out:
        toc: list[list[object]] = []
        for sheet in sheets:
            toc.append([1, sheet.title or sheet.discipline, out.page_count + 1])  # the bookmark targets this sheet's first page (0-based count + 1)
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


async def _collated(sheets: tuple[SheetRef, ...], record: TransmittalRecord, /) -> RuntimeRail[bytes]:
    async def kernel() -> bytes:
        return (await LanePolicy.offload(_collate, sheets, record, modality=Modality.THREAD, retry=RetryClass.OCCT)).default_with(_transmittal_raise)

    return await async_boundary("transmittal.collate", kernel, catch=_FAULTS)


@cache
def _record_schema() -> bytes:
    # the transmittal-record Schematron serialized ONCE (immutable, thread-safe) — one `sch:assert` per
    # `_REQUIRED_RECORD` field over the record `Header`, built through the SubElement node tree (never an
    # f-string splice). The wrapping validator compiles per render (not thread-re-entrant).
    sch = lambda local: etree.QName(_SCHEMATRON_NS, local)
    schema = etree.Element(sch("schema"), nsmap={"sch": _SCHEMATRON_NS})
    etree.SubElement(schema, sch("ns"), prefix="t", uri=_NS)
    rule = etree.SubElement(etree.SubElement(schema, sch("pattern")), sch("rule"), context="t:Header")
    for local in _REQUIRED_RECORD:
        etree.SubElement(rule, sch("assert"), test=f"t:{local} != ''").text = f"missing mandated transmittal-record {local}"
    return etree.tostring(schema)


def _record_xml(deliverable: Deliverable, register_key: str, container_key: str, /) -> RecordBytes:
    # the transmittal record as one structured `lxml` node tree — every dynamic value crosses `SubElement.text`/`set`
    # so the serializer escapes a title or purpose carrying `<`/`&`/`"`, never an f-string splice. The built tree is
    # Schematron-validated (`record_valid`), then serialized `c14n2` so the legal record content-addresses run-to-run.
    record, verdict = deliverable.record, deliverable.register.audited()
    qname = lambda local: etree.QName(_NS, local)
    root = etree.Element(qname("Transmittal"), nsmap={None: _NS})
    header = etree.SubElement(root, qname("Header"))
    for field, value in (
        ("number", record.number),
        ("revision", record.revision),
        ("issuedAt", record.issued_at),
        ("issuingParty", record.issuing_party),
        ("recipient", record.recipient),
        ("purpose", record.purpose.value),
        ("project", record.project),
        ("projectReference", record.project_id),
        ("dominantSuitability", verdict.dominant_suitability),
        ("latestRevision", verdict.latest_revision),
        ("registerKey", register_key),
        ("containerKey", container_key),
    ):
        etree.SubElement(header, qname(field)).text = value
    containers = etree.SubElement(root, qname("Containers"))
    for sheet in deliverable.sheets:
        node = etree.SubElement(containers, qname("Container"), reference=sheet.key.hex, suitability=sheet.suitability, revision=sheet.revision)
        for field, value in (("title", sheet.title), ("discipline", sheet.discipline), ("format", sheet.fmt)):
            etree.SubElement(node, qname(field)).text = value
    schema = isoschematron.Schematron(
        etree.fromstring(_record_schema()), store_report=True
    )  # per-call: the ISO engine is not thread-re-entrant on error_log
    valid = schema.validate(root)
    return RecordBytes(etree.tostring(root, method="c14n2"), valid, len(schema.error_log))


async def _record(deliverable: Deliverable, register_key: str, container_key: str, /) -> RuntimeRail[RecordBytes]:
    async def kernel() -> RecordBytes:
        return (await LanePolicy.offload(_record_xml, deliverable, register_key, container_key, modality=Modality.THREAD, retry=RetryClass.OCCT)).default_with(_transmittal_raise)

    return await async_boundary("transmittal.record", kernel, catch=_FAULTS)


def _lineage_manifest(spec: IssueSpec, sheets: tuple[SheetRef, ...], /) -> Manifest:
    # the C2PA sheet-lineage chain: each sheet rides as a `componentOf` `Ingredient` carrying its content-key `instance_id` — tamper-evident provenance of every sheet transmitted.
    ingredients = tuple(
        Ingredient.Stream(
            IngredientDefinition(title=sheet.title, format=sheet.fmt, relationship="componentOf", instance_id=sheet.key.hex), sheet.fmt, sheet.data
        )
        for sheet in sheets
    )
    return Manifest(
        definition=ManifestDefinition(title=spec.title, format=spec.cover_fmt, claim_generator="rasm-artifacts"),
        actions=(ActionDefinition(action="c2pa.published"),),
        ingredients=ingredients,
        intent=spec.intent,
    )


def _seal_profile(spec: SealSpec, names: tuple[str, ...], /) -> CodecProfile:
    # the container profile derived once from the algo, carrying the attachment names and optional pass; never a hardcoded literal beside the call.
    member_names = ("plan-set.pdf", *names)
    match spec.algo:
        case CompressionAlgo.SEVEN_Z:
            return CodecProfile(seven_z=SevenZKnobs(password=spec.password, names=member_names))
        case _:
            return CodecProfile(zip_stream=ZipStreamKnobs(password=spec.password, names=member_names))


async def _sign_pades(plan_set: bytes, spec: IssueSpec, /) -> RuntimeRail[tuple[ContentKey, ConformanceVerdict]]:
    # the hard legal signature task — `Conformance.close` owns the transient-TSA retry weave and the offload; the child returns its rail.
    return await Conformance.Sign(plan_set, spec.pades).close()


async def _sign_cose(spec: IssueSpec, sheets: tuple[SheetRef, ...], /) -> RuntimeRail[tuple[ContentKey, CredentialEvidence]]:
    # the soft provenance task — the cover signed with the sheet-lineage manifest; `Provenance.close` owns the transient-remote-manifest retry weave; the child returns its rail.
    manifest = _lineage_manifest(spec, sheets)
    return await Provenance.Sign(spec.cover, CoseSpec(manifest=manifest, fmt=spec.cover_fmt, signer=spec.credential_signer)).close()


def _assemble_evidence(deliverable: Deliverable, spec: AssembleSpec, key: ContentKey, plan: Option[ImposedPlan], /) -> TransmittalEvidence:
    # `planned()` is `Some` for the Impose op; the defaults guard only the structurally-unreachable `Nothing` a Proof op would carry.
    return structs.replace(
        _base_evidence(deliverable, "assemble"),
        plan_set=key.hex,
        scheme=spec.scheme.value,
        press_sheets=plan.map(lambda p: p.sheets).default_value(0),
        signatures=plan.map(lambda p: p.signatures).default_value(0),
    )


def _seal_evidence(deliverable: Deliverable, key: ContentKey, members: int, /) -> TransmittalEvidence:
    return structs.replace(_base_evidence(deliverable, "seal"), container=key.hex, container_members=members)


def _manifest_evidence(deliverable: Deliverable, register_key: str, spec: ManifestSpec, record: RecordBytes, /) -> TransmittalEvidence:
    return structs.replace(
        _base_evidence(deliverable, "manifest"),
        container=spec.container,
        plan_set=register_key,
        record_valid=record.valid,
        record_errors=record.errors,
    )


def _issue_evidence(
    deliverable: Deliverable, plan_set: ContentKey, verdict: ConformanceVerdict, credential: Option[CredentialEvidence], /
) -> TransmittalEvidence:
    # a present credential records its state and lineage depth, an absent/failed one folds to defaults; the validity is the PAdES bottom line.
    return structs.replace(
        _base_evidence(deliverable, "issue"),
        plan_set=plan_set.hex,
        validation_state="valid" if verdict.signature_valid and verdict.trusted else "invalid",
        signed_valid=verdict.signature_valid and verdict.trusted,
        pades_level=verdict.pades_level,
        signer=verdict.signer_subject,
        signed_at=verdict.signed_at,
        credential_state=credential.map(lambda ev: ev.validation_state).default_value("unsigned"),
        lineage=credential.map(lambda ev: ev.ingredients).default_value(0),
        verdict=verdict,
    )


def _base_evidence(deliverable: Deliverable, stage: str, /) -> TransmittalEvidence:
    # the shared record/index projection every stage fills, the register audit reused, not a second per-sheet re-walk.
    record, verdict = deliverable.record, deliverable.register.audited()
    return TransmittalEvidence(
        transmittal_id=record.number,
        revision=record.revision,
        issued_at=record.issued_at,
        issuing_party=record.issuing_party,
        recipient=record.recipient,
        suitability=verdict.dominant_suitability,
        stage=stage,
        sheets=len(deliverable.sheets),
        dominant_suitability=verdict.dominant_suitability,
        latest_revision=verdict.latest_revision,
    )


def _folded_signs(
    deliverable: Deliverable,
    pades: RuntimeRail[tuple[ContentKey, ConformanceVerdict]],
    cose: RuntimeRail[tuple[ContentKey, CredentialEvidence]] | None,
    /,
) -> RuntimeRail[tuple[ContentKey, TransmittalEvidence]]:
    # the accumulate-vs-abort decision at the boundary — the two signs compose without a shared fault semigroup.
    match pades:
        case Error() as err:
            return err
        case Ok((plan_set, verdict)):
            credential = Option.of_optional(cose).bind(lambda rail: rail.to_option()).map(lambda pair: pair[1])
            return Ok((plan_set, _issue_evidence(deliverable, plan_set, verdict, credential)))
        case _ as unreachable:
            assert_never(unreachable)
```

## [03]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)

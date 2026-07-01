# [PY_ARTIFACTS_TRANSMITTAL]

The ISO 19650 transmittal / issue-for-construction close at the delivery boundary — the assembly orchestrator that folds an issued sheet set into a press-imposed plan-set, a content-addressed transmittal container, a legally PAdES-signed and provenance-credentialed record, and a formal transmittal-record manifest, composing FOUR sibling owners at the wire. `Transmittal` is ONE owner and IS the closed `expression.tagged_union` — its `Assemble`/`Seal`/`Issue`/`Manifest` cases own the dispatch, the async entry, AND every case body directly, never a one-field wrapper over a separate op union — discriminating each case by its own typed `(Deliverable, spec)` payload: `Assemble` composes `composition/imposition#IMPOSE` to lay the constituent sheets into a saddle-stitch / signature / n-up press form, `Seal` composes `package/archive#ARCHIVE` `Bundle` (the `stream-zip` `_EPOCH`-stamped byte-reproducible ZIP or the `py7zr` 7z arm) to container the collated plan-set plus the register / receipts / signed manifests into ONE content-addressed transmittal blob, `Issue` composes `exchange/conformance#CONFORMANCE` (`pyhanko` PAdES-LTA sign + document-timestamp + resilient audit over the plan-set PDF — the legal issue-for-construction signature) CONCURRENTLY with `exchange/credential#CREDENTIAL` (`c2pa-python` sign of a cover asset carrying EACH issued sheet as a component `Ingredient` — the tamper-evident sheet-lineage chain) inside one `anyio` task group as the failure boundary, and `Manifest` composes `delivery/register#REGISTER` as the issued index and builds the ISO 19650 transmittal-record XML through structured `lxml` node authoring — each case folding once into the shared `TransmittalEvidence` value object the `close` projection returns, never a per-stage evidence bag and never a per-product transmittal-builder family.

Raw client transmittal-record headers cross the boundary EXACTLY ONCE: `TransmittalRecord.of(payload)` validates the `TransmittalPayload` `TypedDict` through the module-level `_PAYLOAD` `TypeAdapter`, so the interior is total over an admitted `TransmittalRecord` and never re-parses a stringly field. `close` is `async` and mirrors the `exchange/credential#CREDENTIAL`/`exchange/conformance#CONFORMANCE` `close` shape — returning `RuntimeRail[tuple[ContentKey, TransmittalEvidence]]` — but is the ORCHESTRATOR variant: where the sign leaves wrap a synchronous native core in one `to_thread.run_sync`, the transmittal THREADS the composed sibling owners' already-railed async results through ROP `bind`/`map` and offloads only its OWN native seams (the `pymupdf` plan-set collation, the `lxml` record serialize) through the bounded `_GATE` `CapacityLimiter`, so a composed owner's `Result.Error` propagates as this owner's fault WITHOUT the rail-to-raise-to-rail erasure the `rails-and-effects#EXPRESSION_SPINE` law forbids. The transient TSA / remote-manifest retry is NOT re-woven here: the crypto-with-network `stamina.retry(on=_TRANSIENT)` weave lives in the composed `Conformance`/`Provenance` sign legs the `Issue` arm binds, and a second retry on the CPU assemble / seal fold would be dead ceremony AND a pathological double-retry, so the transmittal owns the concurrency (the `anyio` task group over the two independent signs) while the sign legs own the retry.

Every operation is content-keyed — `Assemble` returns the imposition's imposed plan-set key, `Seal` the archive's container key, `Issue` the PAdES-signed plan-set key, `Manifest` the `ContentIdentity.of` mint over the transmittal-record XML bytes — and the `Transmittal` node schedules into `core/plan#PLAN` `ArtifactPipeline` as ONE `ArtifactWork` producer whose `parents` ARE the constituent sheet `ContentKey`s (the transmittal binding figures is a leaf, its sheets are the CPM dependency edges). Each op contributes the ONE new `core/receipt#RECEIPT` `ArtifactReceipt.Transmittal` case (transmittal id, issued-sheet count, purpose-of-issue suitability, container key, folded signed verdict) the self-contained async `emit` producer thunk mints from the `close` pair exactly as the `exchange/conformance#CONFORMANCE`/`exchange/credential#CREDENTIAL` producers project `ArtifactReceipt.Verdict`/`ArtifactReceipt.Credential` — the delivery-issue evidence the `[07]-[SEAM_UNIFICATION]` target admits as a CASE on the one family, never a parallel receipt rail. This owner authors no sheet, no drawing, no IFC/QTO, and re-implements no imposition / archive / crypto engine — it composes the constituent producers' outputs into the ISO 19650 issue and closes it.

## [01]-[INDEX]

- [01]-[TRANSMITTAL]: the ISO 19650 transmittal / issue-for-construction orchestrator over the closed-payload `TransmittalOp`-shaped `Transmittal` `tagged_union` (`Assemble` the `composition/imposition#IMPOSE` press-form lay, `Seal` the `package/archive#ARCHIVE` `Bundle` content-addressed container, `Issue` the CONCURRENT `exchange/conformance#CONFORMANCE` PAdES + `exchange/credential#CREDENTIAL` C2PA sheet-lineage sign over one `anyio` task group, `Manifest` the `delivery/register#REGISTER` issued index + `lxml` transmittal-record XML) folded once into the shared `TransmittalEvidence`; the `Deliverable` (`sheets`/`register`/`record`) common payload, the `SheetRef` constituent-sheet value (`key`/`data`/`suitability`/`revision`/`title`/`discipline`/`fmt`), the `TransmittalRecord` ISO 19650 header admitted once through the `TransmittalPayload` `_PAYLOAD` `TypeAdapter`, the `AssembleSpec`/`SealSpec`/`IssueSpec`/`ManifestSpec` per-case request payloads; the async `close -> RuntimeRail[tuple[ContentKey, TransmittalEvidence]]` threading the composed sibling rails through `bind`/`map` and offloading its own `pymupdf`/`lxml` seams through the bounded `_GATE`; the concurrent-sign `anyio.create_task_group` failure boundary with the PAdES-hard / C2PA-soft fold; the `_lineage_manifest` sheet-as-`Ingredient` C2PA chain; the `_collate` deterministic `pymupdf` plan-set assembly and the `_record_xml` escaped `lxml` transmittal record; and the self-contained async `emit` `Work[ArtifactReceipt]` producer thunk minting the one new `core/receipt#RECEIPT` `ArtifactReceipt.Transmittal` case from the `close` pair.

## [02]-[TRANSMITTAL]

- Owner: `Transmittal` the one delivery-issue owner AND the closed `expression.tagged_union` discriminating operation over its own typed `(Deliverable, spec)` payload — `Assemble`, `Seal`, `Issue`, `Manifest` — matched by one total `match`/`case` in `close` with the `Assemble`/`Seal`/`Issue`/`Manifest` `@classmethod` factories returning `Self` and the `_assembled`/`_sealed`/`_issued`/`_manifested` case-body methods all folded onto the union, never a one-field wrapper over a separate `TransmittalOp` union, never a `stage: str` discriminant beside a shared optional-field bag, and never a free module function reconstructing a case body outside the owner. `Deliverable` is the shared common payload every case reads — `sheets: tuple[SheetRef, ...]` the constituent issued sheets (bytes for collation and C2PA lineage, keys for the CPM parents), `register: Register` the composed `delivery/register#REGISTER` issued index the transmittal reads `audited()` off for the dominant suitability / latest revision / container count and drives `of()` on for the register key, `record: TransmittalRecord` the ISO 19650 transmittal header — so a case carries `(deliverable, spec)` exactly as the sibling `exchange/credential#CREDENTIAL` cases carry `(bytes, spec)`. `SheetRef` is the one frozen constituent-sheet value (`key`/`data`/`suitability`/`revision`/`title`/`discipline`/`fmt`), never a sheet-bytes tuple decoded by index. `TransmittalRecord` is the ISO 19650 issue header (`number`/`revision`/`issued_at`/`issuing_party`/`recipient`/`purpose`/`project`/`project_id`) admitted once from the raw `TransmittalPayload` `TypedDict` through the `_PAYLOAD` `TypeAdapter` at `TransmittalRecord.of`. `pymupdf` owns the deterministic `open`/`insert_pdf`/`tobytes(no_new_id=True)` plan-set collation; `lxml.etree` owns the `Element`/`SubElement`/`QName`/`tostring` escaped transmittal-record XML; `composition/imposition#IMPOSE` `Imposition`/`ImposeOp`/`Scheme`/`Geometry`/`Marks`/`ImposedPlan` own the press form; `package/archive#ARCHIVE` `Bundle`/`CompressionAlgo`/`CodecProfile`/`ZipStreamKnobs`/`SevenZKnobs` own the container; `exchange/conformance#CONFORMANCE` `Conformance`/`ConformanceVerdict`/`PadesLevel`/`SignerSource` own the PAdES close; `exchange/credential#CREDENTIAL` `Provenance`/`Manifest`/`Ingredient`/`SignerSpec`/`CredentialEvidence` own the C2PA lineage; no transmittal engine is admitted, so the ISO 19650 issue algebra is this owner's composition over those engines, never a re-implemented imposer, archiver, or signer.
- Cases: `Transmittal` cases — `Assemble(deliverable, AssembleSpec)` (collate the `SheetRef.data` sequence into ONE plan-set source through the deterministic `_collate` `pymupdf` kernel, then compose `Imposition(ImposeOp.Impose(source, spec.scheme, spec.geometry, spec.marks)).of()` for the imposed press-form `ContentKey` and `Imposition(ImposeOp.Plan(source, spec.scheme, spec.geometry)).planned()` for the `ImposedPlan` press metrics — the scheme, imposed press-sheet count, signature count, and fold depth riding the evidence — so the transmittal lays the plan-set through the categorical-best imposer rather than a re-implemented n-up) · `Seal(deliverable, SealSpec)` (collate the plan-set, then compose `Bundle.of(spec.algo, plan_set, *(blob for _, blob in spec.attachments), profile=...).pack()` to fold the collated plan-set plus the `spec.attachments` `(name, bytes)` register / receipt / signed-manifest rows into ONE content-addressed `ZIP_STREAM` reproducible or `SEVEN_Z` container, returning the container `ContentKey` and the sealed member count — a byte-reproducible `_EPOCH`-stamped issue archive rather than a loose PDF concat) · `Issue(deliverable, IssueSpec)` (collate the plan-set, then drive `Conformance.Sign(plan_set, spec.pades).close()` — the hard PAdES-LTA legal signature — CONCURRENTLY with the optional `Provenance.Sign(spec.cover, CoseSpec(manifest=_lineage_manifest(spec, sheets), fmt=spec.cover_fmt, signer=spec.credential_signer)).close()` — the soft C2PA sheet-lineage provenance over the cover asset carrying each `SheetRef` as a component `Ingredient` — inside one `anyio.create_task_group`, folding the `ConformanceVerdict` and `Option[CredentialEvidence]` into the signed verdict) · `Manifest(deliverable, ManifestSpec)` (drive `deliverable.register.emit()` for the issued-index receipt whose `slot` IS the register `ContentKey`, then build the ISO 19650 transmittal-record XML through `_record_xml` structured `lxml` node authoring — the record header plus one `Container` node per `SheetRef` carrying reference / suitability / revision / discipline, plus the register and sealed-container key references — returning the `ContentIdentity.of` mint over the pretty-printed UTF-8 record) — matched by one total `match`/`case`, never a per-product transmittal-builder sibling and never a per-stage `_emit` method. Each case folds once into the shared `TransmittalEvidence` and returns `RuntimeRail[tuple[ContentKey, TransmittalEvidence]]`.
- Auto: `close` is the one total `match` over `self`, dispatching to the four async case-body methods that THREAD the composed sibling rails through ROP — no synchronous `_run` offloaded once (the sign-leaf shape), because the composed owners are themselves async and already railed. `_assembled` binds `_collated` (its own `pymupdf` seam boundary-wrapped and `_GATE`-offloaded) then `Imposition.of()` (composed rail) through `map`, the `Imposition.planned()` `ImposedPlan` projected onto the evidence. `_sealed` binds `_collated` then `Bundle.pack()` (composed rail) through `map`, reading the container key off the returned pair and the member count off the sealed payload arity. `_issued` binds `_collated`, opens ONE `anyio.create_task_group` starting the PAdES sign task always and the C2PA sign task only when a `cover` and `credential_signer` are supplied (each child returns its `RuntimeRail` so the `TaskHandle.return_value` the group already owns carries it — the `CHILD_CARRIER` law, never a sink list), then folds through `_folded_signs`: a PAdES `Result.Error` aborts the whole issue (the primary legal signature is a hard failure) while an absent-or-failed C2PA sign folds to `Nothing` (the supplementary provenance is soft, recording `credential_state` without aborting the legally-signed record), the two independent signs running concurrently as the `anyio` task group failure boundary rather than a sequential await or the forbidden `asyncio.gather`. `_manifested` binds `deliverable.register.emit()` (the composed rail whose `ArtifactReceipt.Register.slot` IS the issued-index `ContentKey`) then `_record` (its own `lxml` seam boundary-wrapped) through `map`, minting the record key over the produced XML bytes. `_collate` is ONE imperative measured kernel threading ONE owned `pymupdf.Document` handle mutated in place across N `insert_pdf` inserts — a multi-megabyte multi-sheet plan-set assembled through the platform-forced in-place native-mutation seam, every opened handle bracketed by `with` so it closes deterministically on each exit, then `tobytes(garbage=4, deflate=True, use_objstms=1, no_new_id=True)` suppressing the random `/ID` so the collated plan-set is byte-reproducible and mints one stable `ContentKey` run-to-run. `_record_xml` builds every dynamic value through `SubElement.text`/`set` so the serializer escapes a title or purpose carrying `<`/`&`/`"`, never an f-string splice into markup (the `TEMPLATE_STRUCTURE` injection form).
- Receipt: `close` returns the rich `TransmittalEvidence` as the `(ContentKey, TransmittalEvidence)` pair, mirroring the `exchange/conformance#CONFORMANCE` `(ContentKey, ConformanceVerdict)` and `exchange/credential#CREDENTIAL` `(ContentKey, CredentialEvidence)` closes — so the verification caller receives the full issue picture (the transmittal id / revision / issue date / issuing party / recipient / purpose, the issued-sheet and press-signature counts, the imposition scheme, the container key and member count, the folded PAdES level / signer / signing time / bottom-line validity, the C2PA validation state and sheet-lineage depth, the dominant suitability / latest revision, and the leaf `ConformanceVerdict` forensic edge) rather than a five-scalar summary. The self-contained async `emit` thunk — the `core/plan#PLAN` `Work[ArtifactReceipt]` producer the transmittal schedules as one `ArtifactWork` leaf (its `parents` the constituent sheet keys) — drives `close` and projects the returned pair onto `core/receipt#RECEIPT` `ArtifactReceipt.Transmittal(key, ev.transmittal_id, ev.sheets, ev.suitability, ev.container or ev.plan_set, ev.validation_state)`: the five settled scalars (transmittal id, issued-sheet count, purpose-of-issue suitability code, sealed-container `ContentKey.hex`, folded signed verdict `"valid"`/`"invalid"`/`"unsigned"`) projected exactly as `ArtifactReceipt.Verdict(key, verdict)` is minted from the conformance pair — so the `transmittal` case stays flat scalars, `receipt.py` imports no `TransmittalEvidence` value object, and the delivery-issue evidence lands as a CASE on the one `ArtifactReceipt` family the `[07]-[SEAM_UNIFICATION]` target admits, never a parallel receipt rail. The `validation_state` plus the native-int issued-sheet / member / lineage counts the pair carries are the observable issue facts the runtime `observability/metrics` `MeterProvider` signal stream reads off the minted receipt fold.
- Growth: a new issue product is one `Transmittal` case with its typed payload and one `close` arm plus one case-body method (the `assert_never` tail breaking the match until it exists); a new imposition scheme rides the composed `composition/imposition#IMPOSE` `Scheme` for free; a new container algorithm rides the composed `package/archive#ARCHIVE` `CompressionAlgo` for free; a new PAdES level or signer seam rides the composed `exchange/conformance#CONFORMANCE` `PadesLevel`/`SignerSource` for free; a new C2PA ingredient relationship or intent rides the composed `exchange/credential#CREDENTIAL` vocabulary for free; a new record-header field is one `TransmittalRecord` field flowing into the `_PAYLOAD` gate, the record XML, and the evidence; a new sealed attachment is one `(name, bytes)` row on `SealSpec.attachments`; a new evidence fact is one `TransmittalEvidence` field (and, when contract-settled, one scalar on the shared `ArtifactReceipt.Transmittal` case); a new signed-verdict fold rule is one arm in `_folded_signs`; zero new surface — the owner grows by case, composed-owner vocabulary, and evidence field, never by method family.
- Boundary: no sheet authoring (`composition/sheet#SHEET` owns the title block; the transmittal reads the `SheetRef`), no drawing (`drawing/*` owns it), no imposition placement (`composition/imposition#IMPOSE` owns the `Scheme`/`Placement` fold; the transmittal composes `ImposeOp`), no container codec (`package/archive#ARCHIVE`/`package/codec#CODEC` own the `Bundle`; the transmittal composes `Bundle.pack`), no crypto engine (`exchange/conformance#CONFORMANCE` owns PAdES, `exchange/credential#CREDENTIAL` owns C2PA; the transmittal composes their `close`), no IFC/QTO (`csharp:Rasm.Bim` owns them). The deleted forms are a stringly `purpose: str`/`suitability: str` field where the composed `Suitability` owner and the `_PAYLOAD` gate type them, a per-row `os.getenv`/`dict.get` header re-parse where `TransmittalRecord.of` validates once, a concatenated-string transmittal-record XML where `lxml` `SubElement` escapes the node tree, a re-implemented n-up/booklet imposer where `Imposition` is composed, a re-implemented ZIP/7z writer where `Bundle` is composed, a hand-rolled PAdES/COSE signer where `Conformance`/`Provenance` are composed, a `pymupdf` collation fold that rebinds and recopies the whole buffer per sheet where the ONE imperative in-place kernel threads one handle, a random-`/ID` collation that churns the content key where `no_new_id=True` keeps it reproducible, a per-sheet PDF C2PA sign where `c2pa` cannot sign PDFs (routed to the `pyhanko` PAdES rail) and each sheet rides as an `Ingredient` on the cover manifest instead, an `asyncio.gather` over the two signs where the `anyio` task group is the failure boundary, a hard-abort C2PA sign where the supplementary provenance folds soft, a second `stamina.retry` on the CPU assemble/seal fold where the transient retry lives in the composed sign legs (a double-retry), a rail-to-raise-to-rail unwrap of a composed owner's `Result` inside `async_boundary` where the case body binds it, a per-stage evidence `Struct` where the shared `TransmittalEvidence` folds once, and a parallel `delivery`-transmittal receipt rail where the one new `ArtifactReceipt.Transmittal` case carries the evidence.
- Packages: `expression` (`tagged_union`/`tag`/`case` the `Transmittal` and `RecordDefect` unions; `Result`/`Ok`/`Error`/`Option` and `.bind`/`.map`/`.to_option`/`.default_value`/`Option.of_optional` the composed-rail threading and the soft-credential fold); `msgspec` (`Struct(frozen=True)` the `SheetRef`/`TransmittalRecord`/`Deliverable`/`AssembleSpec`/`SealSpec`/`IssueSpec`/`ManifestSpec` value objects, `Struct(frozen=True, gc=False)` the `TransmittalEvidence` scalar leaf, `structs.asdict` deriving `TransmittalEvidence.facts`, `structs.replace` the per-stage evidence fill); `pydantic` (`TypeAdapter` the module-level `_PAYLOAD` admission over the `TransmittalPayload` `TypedDict`, `ValidationError` mapped to `RecordDefect` at the seam); `anyio` (`create_task_group` the concurrent-sign failure boundary, `CapacityLimiter`/`to_thread.run_sync` the bounded off-loop native seam); `pymupdf` (`open`/`insert_pdf`/`tobytes` the deterministic plan-set collation, deferred through a module-scope `lazy import` so an unused path never pays the MuPDF load); `lxml.etree` (`Element`/`SubElement`/`QName`/`tostring` the transmittal-record XML, deferred through `lazy from` so a non-manifest path never pays the libxml2 load); runtime (`content_identity.ContentIdentity`/`ContentKey` the record key, `faults.RuntimeRail`/`async_boundary` the rail and fault capsule); `composition/imposition#IMPOSE` (`Imposition`/`ImposeOp`/`Scheme`/`Geometry`/`Marks`/`ImposedPlan` the press form); `package/codec#CODEC` (`Bundle`/`CompressionAlgo`/`CodecProfile` the content-addressed bundle entry) plus `package/archive#ARCHIVE` (`ZipStreamKnobs`/`SevenZKnobs` the ZIP/7z container knobs that realize the `ZIP_STREAM`/`SEVEN_Z` cases); `exchange/conformance#CONFORMANCE` (`Conformance`/`ConformanceVerdict`, the `SignSpec` aliased `PadesSpec` carrying the caller's `PadesLevel`/`SignerSource` — the composed sign leg owning the `stamina.retry` transient-TSA weave); `exchange/credential#CREDENTIAL` (`Provenance`/`Manifest`/`Ingredient`/`IngredientDefinition`/`ManifestDefinition`/`ActionDefinition`/`SignerSpec`/`Intent`/`DigitalSource`/`CredentialEvidence`, the `SignSpec` aliased `CoseSpec` — the composed sign leg owning the `stamina.retry` transient-remote-manifest weave); `delivery/register#REGISTER` (`Register`/`Suitability`/`Revision` the issued index and status vocabularies); `core/receipt#RECEIPT` (`ArtifactReceipt.Transmittal` the new contributed case the consumer mints). No new external library — every engine is composed, and `stamina` rides the composed sign legs.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
from enum import StrEnum
from typing import Final, Literal, NotRequired, ReadOnly, Required, Self, TypedDict, Unpack, assert_never

from anyio import CapacityLimiter, TaskHandle, create_task_group, to_thread
from expression import Error, Ok, Option, Result, case, tag, tagged_union
from msgspec import Struct, structs
from pydantic import TypeAdapter, ValidationError

from rasm.runtime.content_identity import ContentIdentity, ContentKey
from rasm.runtime.faults import RuntimeRail, async_boundary

from artifacts.composition.imposition import Geometry, ImposedPlan, ImposeOp, Imposition, Marks, Scheme
from artifacts.delivery.register import Register
from artifacts.exchange.conformance import Conformance, ConformanceVerdict
from artifacts.exchange.conformance import SignSpec as PadesSpec  # collides with credential.SignSpec — aliased at the seam (carries the PadesLevel/SignerSource the caller fills)
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
from artifacts.core.receipt import ArtifactReceipt
from artifacts.package.archive import SevenZKnobs, ZipStreamKnobs
from artifacts.package.codec import Bundle, CodecProfile, CompressionAlgo

lazy import pymupdf  # the plan-set collation engine; cold native MuPDF, deferred to the assemble/seal/issue paths
lazy from lxml import etree  # the ISO 19650 transmittal-record XML builder; cold, deferred to the `Manifest` serialize

# --- [TYPES] ----------------------------------------------------------------------------


class Purpose(StrEnum):  # the ISO 19650 purpose-of-issue vocabulary the record header carries; a stringly free-text purpose is the deleted form
    COORDINATION = "coordination"        # issued for spatial coordination (S1)
    INFORMATION = "information"          # issued for information (S2)
    REVIEW_COMMENT = "review-comment"    # issued for review and comment (S3)
    REVIEW_AUTHORIZE = "review-authorize"  # issued for review and authorization (S4)
    CONSTRUCTION = "construction"        # issued for construction (published, contractual)
    TENDER = "tender"                    # issued for tender/bid
    APPROVAL = "approval"                # issued for statutory approval
    AS_BUILT = "as-built"                # as-constructed record issue

# --- [MODELS] ---------------------------------------------------------------------------


@tagged_union(frozen=True)
class RecordDefect:
    # the closed record-admission fault carrying its offending value — declared first because
    # `TransmittalRecord.of` returns it; the two boundary causes the gate distinguishes, never a bare
    # `str` erasing which admission failed.
    tag: Literal["invalid_payload", "invalid_purpose"] = tag()
    invalid_payload: str = case()   # the failing `_PAYLOAD` field locs
    invalid_purpose: str = case()   # the unrecognized purpose-of-issue token


class SheetRef(Struct, frozen=True):
    # one constituent issued sheet — the bytes feed the plan-set collation AND the C2PA ingredient lineage,
    # the key is the CPM parent edge the `core/plan#PLAN` graph orders the transmittal leaf behind.
    key: ContentKey
    data: bytes
    title: str
    discipline: str            # the ISO 13567 / NCS discipline code
    suitability: str           # the ISO 19650 suitability code (`Suitability.of`-parseable at the register)
    revision: str              # the ISO 19650 revision code
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
    number: str                # the transmittal number/reference
    issuing_party: str         # the sender (issuing appointed party)
    recipient: str             # the receiving party
    purpose: Purpose           # the purpose of issue
    revision: str = "P01"      # the transmittal revision code
    issued_at: str = ""        # the ISO-8601 issue date
    project: str = ""
    project_id: str = ""

    @staticmethod
    def of(**payload: Unpack[TransmittalPayload]) -> Result["TransmittalRecord", RecordDefect]:
        # the one boundary ingress: the shape gate through the `_PAYLOAD` TypeAdapter, the purpose parsed
        # to the closed vocabulary, so the interior is total over an admitted record.
        try:
            row = _PAYLOAD.validate_python(payload)
        except ValidationError as fault:
            return Error(RecordDefect(invalid_payload=str([error["loc"] for error in fault.errors()])))
        if row["purpose"] not in _PURPOSES:  # the derived value set — never the `_value2member_map_` private-dunder probe
            return Error(RecordDefect(invalid_purpose=row["purpose"]))
        return Ok(TransmittalRecord(
            number=row["number"], issuing_party=row["issuing_party"], recipient=row["recipient"],
            purpose=Purpose(row["purpose"]), revision=row.get("revision", "P01"), issued_at=row.get("issued_at", ""),
            project=row.get("project", ""), project_id=row.get("project_id", ""),
        ))


class Deliverable(Struct, frozen=True):  # the common payload every case reads — the sheets, the issued index, the header
    sheets: tuple[SheetRef, ...]
    register: Register
    record: TransmittalRecord


class AssembleSpec(Struct, frozen=True):
    scheme: Scheme = Scheme.NUP
    geometry: Geometry = Geometry()
    marks: Marks = Marks()


class SealSpec(Struct, frozen=True):
    algo: CompressionAlgo = CompressionAlgo.ZIP_STREAM  # the byte-reproducible `_EPOCH`-stamped container default
    profile: CodecProfile | None = None                # None -> the archive default profile for `algo`
    attachments: tuple[tuple[str, bytes], ...] = ()    # (name, bytes) — the register XML / receipts / signed manifests sealed beside the plan-set
    password: str | None = None                        # the container encryption pass (WinZip-AES for ZIP, header-crypt for 7z)


class IssueSpec(Struct, frozen=True):
    pades: PadesSpec                                    # the hard legal PAdES-LTA sign spec (signer/level/tsa/certify/commitment)
    credential_signer: SignerSpec | None = None         # the optional soft C2PA signer (cert/callback); None -> PAdES-only
    cover: bytes = b""                                  # the c2pa-signable cover asset (PDF is read-only for c2pa; the lineage rides a cover raster)
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
    suitability: str          # the purpose-of-issue suitability code the receipt carries
    stage: str                # which product this evidence closes (assemble/seal/issue/manifest)
    sheets: int
    dominant_suitability: str
    latest_revision: str
    validation_state: str = "unsigned"
    container: str = ""       # the sealed container `ContentKey.hex` (Seal)
    container_members: int = 0
    plan_set: str = ""        # the imposed / signed plan-set `ContentKey.hex` (Assemble/Issue)
    scheme: str = ""          # the imposition scheme (Assemble)
    press_sheets: int = 0     # the imposed press-sheet count (Assemble)
    signatures: int = 0       # the bound signature count (Assemble)
    signed_valid: bool = False
    pades_level: str = ""
    signer: str = ""
    signed_at: str = ""
    credential_state: str = ""
    lineage: int = 0          # the C2PA ingredient count = sheet-lineage depth (Issue)
    verdict: ConformanceVerdict | None = None  # the leaf forensic edge the caller reads, the receipt projects the scalar

    def facts(self) -> dict[str, object]:
        return structs.asdict(self)


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

    async def close(self) -> RuntimeRail[tuple[ContentKey, TransmittalEvidence]]:
        # the orchestrator entry: thread the composed sibling rails through ROP, offload own native seams
        # through `_GATE`; a composed `Result.Error` propagates by `bind`/`map`, never a rail-to-raise unwrap.
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
        collated = await _collated(deliverable.sheets)
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
        collated = await _collated(deliverable.sheets)
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
        collated = await _collated(deliverable.sheets)
        match collated:
            case Error() as err:
                return err
            case Ok(plan_set):
                # the two signs are INDEPENDENT — the `anyio` task group is their failure boundary, never
                # `asyncio.gather`; each child rails its own faults and settles `FINISHED`, so its terminal rides
                # the `TaskHandle.return_value` the group already owns (the `CHILD_CARRIER` law), never a sink list.
                async with create_task_group() as signs:
                    pades: TaskHandle[RuntimeRail[tuple[ContentKey, ConformanceVerdict]]] = signs.start_soon(_sign_pades, plan_set, spec)
                    cose: TaskHandle[RuntimeRail[tuple[ContentKey, CredentialEvidence]]] | None = (
                        signs.start_soon(_sign_cose, spec, deliverable.sheets)
                        if spec.cover and spec.credential_signer is not None
                        else None
                    )
                return _folded_signs(deliverable, pades.return_value, cose.return_value if cose is not None else None)
            case _ as unreachable:
                assert_never(unreachable)

    async def _manifested(self, deliverable: Deliverable, spec: ManifestSpec, /) -> RuntimeRail[tuple[ContentKey, TransmittalEvidence]]:
        # the register's ONE content-keyed entry is `emit -> RuntimeRail[ArtifactReceipt]`; the issued-index key
        # is the receipt's `slot` (the register mints its own `ArtifactReceipt.Register`, the transmittal composes
        # only its content key into the record and mints its own `Transmittal` receipt) — never a `register.of()`.
        registered = await deliverable.register.emit()
        match registered:
            case Error() as err:
                return err
            case Ok(receipt):
                register_key = receipt.slot
                record = await _record(deliverable, register_key.hex, spec.container)
                return record.map(lambda xml: (ContentIdentity.of("transmittal.manifest", xml), _manifest_evidence(deliverable, register_key.hex, spec)))
            case _ as unreachable:
                assert_never(unreachable)

    async def emit(self) -> RuntimeRail[ArtifactReceipt]:
        # the `core/plan#PLAN` `Work[ArtifactReceipt]` producer thunk (the transmittal is one scheduled
        # `ArtifactWork` leaf): drive `close`, then project the returned pair onto the new
        # `ArtifactReceipt.Transmittal` case's five settled scalars exactly as the conformance/credential
        # producers project theirs, so `receipt.py` imports no `TransmittalEvidence` value object.
        railed = await self.close()
        return railed.map(lambda pair: ArtifactReceipt.Transmittal(pair[0], pair[1].transmittal_id, pair[1].sheets, pair[1].suitability, pair[1].container or pair[1].plan_set, pair[1].validation_state))

# --- [CONSTANTS] ------------------------------------------------------------------------

_PAYLOAD: Final = TypeAdapter(TransmittalPayload)
_PURPOSES: Final[frozenset[str]] = frozenset(purpose.value for purpose in Purpose)  # derived membership set the seam admits a raw purpose against, never `_value2member_map_`
_FAULTS: Final[tuple[type[BaseException], ...]] = (RuntimeError, ValueError, KeyError, OSError)
_GATE: Final[CapacityLimiter] = CapacityLimiter(4)  # the own-native-seam offload bound (pymupdf collate + lxml record)
_NS: Final[str] = "https://rasm.dev/schema/iso19650/transmittal"

# --- [OPERATIONS] -----------------------------------------------------------------------


def _collate(sheets: tuple[SheetRef, ...], /) -> bytes:
    # ONE imperative measured kernel threading ONE owned `Document` handle mutated in place across N inserts:
    # a multi-megabyte plan-set is assembled through the platform-forced native-mutation seam, never a
    # functional fold that recopies the whole buffer per sheet; every handle closes deterministically on
    # each exit; `no_new_id=True` keeps the collated plan-set byte-reproducible for content-addressing.
    # Exemption: `boundaries.md CAPSULE_OWNER` binary-kernel in-place PDF assembly over the live MuPDF handle.
    with pymupdf.open() as out:
        for sheet in sheets:
            with pymupdf.open(stream=sheet.data, filetype="pdf") as src:
                out.insert_pdf(src)
        return out.tobytes(garbage=4, deflate=True, use_objstms=1, no_new_id=True)


async def _collated(sheets: tuple[SheetRef, ...], /) -> RuntimeRail[bytes]:
    async def kernel() -> bytes:
        return await to_thread.run_sync(_collate, sheets, limiter=_GATE)

    return await async_boundary("transmittal.collate", kernel, catch=_FAULTS)


def _record_xml(deliverable: Deliverable, register_key: str, container_key: str, /) -> bytes:
    # the ISO 19650 transmittal record as one structured `lxml` node tree — every dynamic value crosses
    # `SubElement.text`/`set` so the serializer escapes a title or purpose carrying `<`/`&`/`"`, never an
    # f-string splice into markup (the `TEMPLATE_STRUCTURE` injection form the language page rejects).
    record, verdict = deliverable.record, deliverable.register.audited()
    qname = lambda local: etree.QName(_NS, local)
    root = etree.Element(qname("Transmittal"), nsmap={None: _NS})
    header = etree.SubElement(root, qname("Header"))
    for field, value in (
        ("number", record.number), ("revision", record.revision), ("issuedAt", record.issued_at),
        ("issuingParty", record.issuing_party), ("recipient", record.recipient), ("purpose", record.purpose.value),
        ("project", record.project), ("projectReference", record.project_id),
        ("dominantSuitability", verdict.dominant_suitability), ("latestRevision", verdict.latest_revision),
        ("registerKey", register_key), ("containerKey", container_key),
    ):
        etree.SubElement(header, qname(field)).text = value
    containers = etree.SubElement(root, qname("Containers"))
    for sheet in deliverable.sheets:
        node = etree.SubElement(containers, qname("Container"), reference=sheet.key.hex, suitability=sheet.suitability, revision=sheet.revision)
        for field, value in (("title", sheet.title), ("discipline", sheet.discipline), ("format", sheet.fmt)):
            etree.SubElement(node, qname(field)).text = value
    return etree.tostring(root, xml_declaration=True, encoding="utf-8", pretty_print=True)


async def _record(deliverable: Deliverable, register_key: str, container_key: str, /) -> RuntimeRail[bytes]:
    async def kernel() -> bytes:
        return await to_thread.run_sync(_record_xml, deliverable, register_key, container_key, limiter=_GATE)

    return await async_boundary("transmittal.record", kernel, catch=_FAULTS)


def _lineage_manifest(spec: IssueSpec, sheets: tuple[SheetRef, ...], /) -> Manifest:
    # the C2PA sheet-lineage chain: each issued sheet rides as a `componentOf` `Ingredient` carrying its
    # content-key `instance_id`, so the cover credential is tamper-evident provenance of every sheet it
    # transmits — the forensic lineage a flat issue record ignores.
    ingredients = tuple(
        Ingredient.Stream(
            IngredientDefinition(title=sheet.title, format=sheet.fmt, relationship="componentOf", instance_id=sheet.key.hex),
            sheet.fmt,
            sheet.data,
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
    # the container profile derived once from the algo — the reproducible ZIP or the 7z arm, carrying the
    # attachment names and the optional pass; a hardcoded profile literal beside the call is the deleted form.
    member_names = ("plan-set.pdf", *names)
    match spec.algo:
        case CompressionAlgo.SEVEN_Z:
            return CodecProfile(seven_z=SevenZKnobs(password=spec.password, names=member_names))
        case _:
            return CodecProfile(zip_stream=ZipStreamKnobs(password=spec.password, names=member_names))


async def _sign_pades(plan_set: bytes, spec: IssueSpec, /) -> RuntimeRail[tuple[ContentKey, ConformanceVerdict]]:
    # the hard legal signature task — `Conformance.close` owns the `stamina.retry` transient-TSA weave and the
    # thread offload; the child returns its rail so the `TaskHandle.return_value` carries it, never a sink append.
    return await Conformance.Sign(plan_set, spec.pades).close()


async def _sign_cose(spec: IssueSpec, sheets: tuple[SheetRef, ...], /) -> RuntimeRail[tuple[ContentKey, CredentialEvidence]]:
    # the soft provenance task — the cover asset signed with the sheet-lineage manifest; `Provenance.close`
    # owns the `stamina.retry` transient-remote-manifest weave and the thread offload; the child returns its rail.
    manifest = _lineage_manifest(spec, sheets)
    return await Provenance.Sign(spec.cover, CoseSpec(manifest=manifest, fmt=spec.cover_fmt, signer=spec.credential_signer)).close()


def _assemble_evidence(deliverable: Deliverable, spec: AssembleSpec, key: ContentKey, plan: Option[ImposedPlan], /) -> TransmittalEvidence:
    # `planned()` is `Some` for the Impose op, so the projections read the real press metrics; the `_map`
    # defaults guard only the structurally-unreachable `Nothing` a Proof op would carry.
    return structs.replace(
        _base_evidence(deliverable, "assemble"),
        plan_set=key.hex, scheme=spec.scheme.value,
        press_sheets=plan.map(lambda p: p.sheets).default_value(0),
        signatures=plan.map(lambda p: p.signatures).default_value(0),
    )


def _seal_evidence(deliverable: Deliverable, key: ContentKey, members: int, /) -> TransmittalEvidence:
    return structs.replace(_base_evidence(deliverable, "seal"), container=key.hex, container_members=members)


def _manifest_evidence(deliverable: Deliverable, register_key: str, spec: ManifestSpec, /) -> TransmittalEvidence:
    return structs.replace(_base_evidence(deliverable, "manifest"), container=spec.container, plan_set=register_key)


def _issue_evidence(deliverable: Deliverable, plan_set: ContentKey, verdict: ConformanceVerdict, credential: Option[CredentialEvidence], /) -> TransmittalEvidence:
    # PAdES is the hard legal signature; C2PA is soft provenance — a present credential records its state and
    # sheet-lineage depth, an absent/failed one folds to the empty defaults, the validity the PAdES bottom line.
    return structs.replace(
        _base_evidence(deliverable, "issue"),
        plan_set=plan_set.hex,
        validation_state="valid" if verdict.signature_valid and verdict.trusted else "invalid",
        signed_valid=verdict.signature_valid and verdict.trusted,
        pades_level=verdict.pades_level, signer=verdict.signer_subject, signed_at=verdict.signed_at,
        credential_state=credential.map(lambda ev: ev.validation_state).default_value("unsigned"),
        lineage=credential.map(lambda ev: ev.ingredients).default_value(0),
        verdict=verdict,
    )


def _base_evidence(deliverable: Deliverable, stage: str, /) -> TransmittalEvidence:
    # the shared record/index projection every stage fills, the register audit reused for the dominant
    # suitability / latest revision / container count rather than a second per-sheet re-walk here.
    record, verdict = deliverable.record, deliverable.register.audited()
    return TransmittalEvidence(
        transmittal_id=record.number, revision=record.revision, issued_at=record.issued_at,
        issuing_party=record.issuing_party, recipient=record.recipient, suitability=verdict.dominant_suitability,
        stage=stage, sheets=len(deliverable.sheets), dominant_suitability=verdict.dominant_suitability,
        latest_revision=verdict.latest_revision,
    )


def _folded_signs(
    deliverable: Deliverable,
    pades: RuntimeRail[tuple[ContentKey, ConformanceVerdict]],
    cose: RuntimeRail[tuple[ContentKey, CredentialEvidence]] | None,
    /,
) -> RuntimeRail[tuple[ContentKey, TransmittalEvidence]]:
    # the accumulate-vs-abort decision fixed at the boundary: the PAdES leg is a HARD dependency (its
    # `Error` aborts the whole issue — the primary legal signature), the C2PA leg is SOFT (a failed or
    # absent provenance sign folds to `Nothing`, recording `credential_state` without aborting the
    # legally-signed record), so the two independent signs compose without a shared fault semigroup.
    match pades:
        case Error() as err:
            return err
        case Ok((plan_set, verdict)):
            credential = Option.of_optional(cose).bind(lambda rail: rail.to_option()).map(lambda pair: pair[1])
            return Ok((plan_set, _issue_evidence(deliverable, plan_set, verdict, credential)))
        case _ as unreachable:
            assert_never(unreachable)
```

## [03]-[SIGNALS]

- [RECEIPT_TRANSMITTAL_CASE] [RESOLVED]: the transmittal contributes the `core/receipt#RECEIPT` `ArtifactReceipt.Transmittal` case — `transmittal: tuple[ContentKey, str, int, str, str, str] = case()` (transmittal id, issued-sheet count, purpose-of-issue suitability, sealed-container `ContentKey.hex`, folded signed verdict), the `Transmittal(cls, key, transmittal_id, sheets, suitability, container, validation_state)` `@classmethod` mint returning `Self`, the `_KEYS["transmittal"] = ("transmittal_id", "sheets", "suitability", "container", "validation_state")` row, the `"transmittal"` `ArtifactKind` literal token, and the or-pattern arm in each of `slot` and `_facts`. The consumer mints it from the `close` pair exactly as `ArtifactReceipt.Verdict`/`ArtifactReceipt.Credential` are minted from theirs, so `receipt.py` imports no `TransmittalEvidence` and the case stays flat scalars — the delivery-issue evidence the `[07]-[SEAM_UNIFICATION]` target admits as a CASE on the one family, never a parallel receipt rail. Landed: the `transmittal` case, the `Transmittal` mint, and the `_KEYS`/`slot`/`_facts` arms are present on `core/receipt#RECEIPT`, and the `emit` projection `ArtifactReceipt.Transmittal(pair[0], …validation_state)` binds them positionally.
- [ARCHITECTURE_DELIVERY_TRANSMITTAL] [RESOLVED]: `transmittal.py` is registered in `ARCHITECTURE.md` `[01]-[DOMAIN_MAP]` under the `delivery/` folder ("ISO 19650 transmittal / issue-for-construction orchestrator composing imposition + archive + credential + conformance + register") beside `register.py`, `[02]-[SEAMS]` carries the wire rows (`delivery/transmittal → composition/imposition [IMPOSE]`, `→ package/archive [ARCHIVE]`, `→ exchange/conformance [CONFORMANCE]`, `→ exchange/credential [CREDENTIAL]`, `← delivery/register [REGISTER]`, `→ core/receipt [RECEIPT]`), and `README.md` `[01]-[ROUTER]` `[delivery]` group carries the `[TRANSMITTAL]` row. No new `[02]-[DOMAIN_PACKAGES]` rows — every composed engine (`pdfimpose`/`py7zr`/`stream-zip`/`c2pa-python`/`pyhanko`) is already admitted.
- [PLAN_PRODUCER] [RESOLVED]: `core/plan#PLAN` `ArtifactPipeline` schedules the transmittal as ONE `ArtifactWork` producer — its pre-minted `ContentKey` (from `Transmittal.close`), its `Work[ArtifactReceipt]` `emit` coroutine, and its `parents` the constituent sheet `ContentKey`s (the CPM dependency edges; a transmittal binding sheets is a leaf, its sheets are not) — no `core/plan.md` edit because `ArtifactWork` is generic over producers and the transmittal's `emit` satisfies the `Work[ArtifactReceipt]` contract inherently.

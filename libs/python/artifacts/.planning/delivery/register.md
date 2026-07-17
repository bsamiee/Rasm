# [PY_ARTIFACTS_REGISTER]

`Register` is the ISO 19650 delivery-register / drawing-index owner — the information-container metadata authority at the delivery boundary, ONE owner over the closed `RegisterOp` `expression.tagged_union`: `Index` renders the sheet-index publication table through `visualization/table#TABLE`, `Container` serializes the information-container metadata as a dialect-profiled `lxml` XML tree that is `isoschematron.Schematron`-validated against the profile's mandated-metadata rows and emitted `c14n2`-canonical byte-reproducibly, `Audit` folds the coverage verdict over one closed `AuditRule` policy through the accumulating `RegisterFault.combined`, `Render` streams the native `xlsxwriter` register spreadsheet under one `RenderScope`, and `Delta` renders added, revised, regressed, and withdrawn containers against a prior container set — each arm dispatched by one total `match` and folded ONCE into the `Composed` evidence struct, never a second render. No ISO 19650 register library exists, so this owner composes the delivery algebra over the owned standards vocabularies plus `polars`/`great-tables`/`xlsxwriter`/`openpyxl`/`lxml`, never a re-implemented byte emitter.

Ingress is one accumulating boundary: `Register.admit(*sources)` is the ONE polymorphic entrypoint over every raw shape — a `ContainerPayload` mapping admits one row through the module-level `_PAYLOAD` `TypeAdapter`, a `bytes` source is a client register workbook whose rows ingest through the same gate with per-row coordinates, and a drawn sheet enters as this page's `SheetContext` or the `composition/sheet#SHEET` `SheetSet.registered()` `(container_id, TitleBlock, suitability, revision)` projection, both normalized under the one set-level `naming: NamingContext`; every casualty accumulates through the `RegisterFault.combined` monoid, never a first-failure abort and never a `Result.to_option` erasure. `visualization/table#TABLE` `TablePlan` owns the publication render composed at the `.build()` seam; `specification/classify#CODE` owns the full code tables the lean `Classification` reference points at; `document/model#NODE` `TableNode` lowers from the one `polars` `frame` the `Index` also styles; `Register.key` mints PRE-RUN through `ContentIdentity.key` over the canonical `msgpack` preimage. `Register.emit` schedules as one `core/plan#PLAN` `ArtifactWork` whose parents are the constituent sheet content keys, and `_emit` offloads the render through the owner's `lane: LanePolicy` instance seam, minting the `core/receipt#RECEIPT` `ArtifactReceipt.Register(key, kind, sheets, suitability, revision, classification, validation, bytes)` case — one case on the one family, never a parallel rail. `delivery/transmittal#TRANSMITTAL` composes the `RegisterEvidence` coverage verdict as the issued manifest and imports the `STANDARD_AUDIT`/`CONTRACTUAL_AUDIT` policy instances for its purpose-keyed issue gate.

## [01]-[INDEX]

- [01]-[REGISTER]: the ISO 19650 delivery-register axis — the exact-NA-cardinality suitability/revision/state vocabularies over the one `_SUITABILITY` correspondence, the dialect-profiled container XML, the rule-row audit policy, the `InformationContainer` and `Register` owners with the one polymorphic accumulating `admit` ingress, and the closed `RegisterOp` folded once through the lane-offloaded `_composed` into the `ArtifactReceipt.Register` case.

## [02]-[REGISTER]

- Owner: `Register` the one delivery-register owner discriminating over the closed `RegisterOp` `tagged_union`, every arm folded ONCE into the `Composed` evidence struct `_emit` reads — no second render. Status vocabularies are OWNED closed `StrEnum`+`frozendict` families authored to the EXACT BS EN ISO 19650-2:2018 NA Table NA.1 cardinality: `SuitabilityCode` the `S0..S7` band whose `_SUITABILITY` `frozendict` is the ONE `(state, description, withdrawn, revision_kind)` correspondence every consumer derives from, `_S_VALUES` derived by comprehension, never a `_value2member_map_` probe; `PublishedCode` the `(prefix, ordinal)` value object carrying the OPEN ordinal, never a fabricated closed enum; `Suitability` the closed `shared`/`published`/`project` union whose `project` case is the NA extension band a documented client/record code lands on, never a fabricated `RecordCode` claiming ISO standing; `RevisionCode` the `P{NN}[.{NN}]`/`C{NN}` value object whose `succeeds` orders revisions off the value and whose admission rejects a contractual work-in-progress suffix. `ContainerDialect` keys the `_DIALECT` `frozendict` of `DialectProfile` rows — namespace URI, root/container localnames, the profile's own mandated-metadata tuple, and the localname rename band — so each dialect is a structurally distinct XML contract recoverable from one row, never a decorative root attribute over one fixed vocabulary. `InformationContainer` is the frozen ISO 19650 container owner (BS 1192 naming fields, the §5.1.7 metadata, the `asset_key` co-identification, the aggregated sheet facts); `NamingContext` the set-level originator/functional/spatial/form bundle the sheet ingress reads; `ContainerMeta` the project-level header the `Container` XML roots on; `AuditPolicy.rules` is the one tuple of closed `AuditRule` values — `STANDARD_AUDIT`/`CONTRACTUAL_AUDIT` the named instances, a new coverage rule one member plus one total projection arm, never callable-bearing container/sequence sibling shapes or a boolean flag set the fold re-derives; `RegisterEvidence` the coverage-verdict receipt folded once and read by `Audit`, the receipt, and `delivery/transmittal#TRANSMITTAL`.
- Cases: `RegisterOp` cases matched by one total `match`, each lowering to the one `Composed` fold — never a per-format register builder, never a per-operation `_emit`, never a second render for the receipt. `Index(theme, fmt)` lowers `frame` into `visualization/table#TABLE` via the `_index_ops` `TableOp` sequence and `.build()` (one render, one content key). `Container(dialect)` serializes the profile-namespaced `lxml` metadata tree (each value a `SubElement` the serializer escapes, never an f-string splice), validates it against the `@cache`-compiled per-dialect `_container_schema` (one `sch:assert` per profile `required` localname — the ISO 19650-2 §5.1.7 status/revision/classification set on the ISO profile), folds the `valid`/`invalid:{n}` verdict onto `Composed.validation`, and emits `c14n2` bytes so the container content-addresses byte-reproducibly. `Audit(policy)` folds the one `AuditRule` sequence through the `RegisterFault.combined` monoid into `RegisterEvidence`; the same rule tuple is the capability fingerprint in the register key, so behavior-distinct policies cannot collide. `Render(scope)` streams the `xlsxwriter.Workbook(constant_memory=True)` register with deterministic OOXML creation metadata — sized columns, suitability/state `conditional_format`, owned-vocabulary `data_validation`, per-reference `write_url` asset links, page-of-N running header, issued-register `protect` — over the full history or the `latest()` revision-latest fold per `RenderScope`. `Delta(prior, theme, fmt)` compares every latest reference row against the prior set and uses `RevisionCode.succeeds` to distinguish a valid revision from a regression, rendering the added/revised/regressed/withdrawn issue sheet through the table seam.
- Entry: `Register.emit` returns an `ArtifactWork` whose `work` is `_emit` and whose `parents` are the constituent sheet content keys — the register IS the single content-keyed production entry a `core/plan#PLAN` node wraps, never a per-operation entrypoint. `Register.key` is the public PRE-RUN mint — `ContentIdentity.key` over the canonical `_canon` `msgpack` preimage (op tag, op facet, container rows, meta, documented band, issue fields), length- and count-framed by the codec, never a key over rendered bytes and never the retired rail-returning `ContentIdentity.of`; `parents` and `lane` sit outside the preimage by declaration — plan DATA edges and runtime capacity, never identity. `_emit` crosses `_composed` through `self.lane.offload(Kernel.of(_composed, KernelTrait.RELEASING), self)` — the instance-lane seam, never a class-qualified call — and maps the returned `RuntimeRail[Composed]` straight onto the receipt, no rail-to-raise bridge and no second boundary. One ingress constructor `admit` returns `Result[Register, RegisterFault]` under the accumulating disposition so a batch reports every casualty with its container or `row:{n}` coordinate.
- Auto: `_composed` is the ONE `_GUARD`-contracted total `match` over `register.op`, each arm returning `Composed` with the register-level facts folded once (the schema state `unchecked` for every arm but `Container`) so the body stays one `match`-shaped path, never an inline `try`/`except` ladder beside it. `frame` is the one schema-fixed `polars.DataFrame` shaped natively — `from_dicts` then a derived numeric-ordinal column, `sort` by discipline/ordinal/number in the engine — that remains structurally valid when the register is empty; `latest()` is the revision-latest fold `Render`, `Delta`, and callers share. `audited` flat-maps `AuditPolicy.rules` through one total `AuditRule` projection and reduces every casualty through the associative `RegisterFault.combined` monoid. `InformationContainer.admitted` is the one row boundary: `_PAYLOAD.validate_python` retains `ValidationError.errors()` coordinates, the suitability, revision, and classification rails accumulate through `RegisterFault.accumulated`, and `Classification.admitted` rejects an unknown classification system instead of silently relabeling it. `_ingested` streams client workbook rows through the SAME gate while `closing(load_workbook(...))` holds the read-only handle, and its exact workbook faults become one structurally addressable `malformed` casualty; `_contextual` normalizes both sheet ingress shapes onto `InformationContainer.of_block`.
- Receipt: each operation contributes `core/receipt#RECEIPT` off the one `Composed` fold through the `ArtifactReceipt.Register(key, kind, sheets, suitability, revision, classification, validation, bytes)` case — the register-container evidence plus the `Container`-XML conformance state (`valid`/`invalid:{n}`/`unchecked`) the ISO 19650 primitive requires. `Composed.kind` carries the op tag onto the facts so the arms stay routable by the `MeterProvider` per-kind instrument, and `_emit` mints the single case off the one render exactly as `drawing/schedule#SCHEDULE` mints its `ArtifactReceipt.Schedule` — one case on the shared family, never a per-kind facts `Struct`, a parallel receipt type, or a second render. `core/plan#PLAN` reads the case as the content-keyed evidence its sub-graph elision distinguishes hit from miss on.
- Packages: `expression` (the `Suitability`/`RegisterFault`/`RegisterOp` unions, the `Result`/`Option` admission and coverage rails, `Block`/`Map` the fault, audit, and revision-latest folds); `msgspec` (the frozen value objects, `json.Encoder` the audit-report egress, `msgpack.Encoder` the canonical key preimage); `polars` (the register `frame` shaped in-engine, a settled input the register shapes but never sources); `great-tables` (through `visualization/table#TABLE` `TablePlan`, composed at the `build` seam, never a direct reach); `xlsxwriter` (the native register under `constant_memory` alone — `in_memory` contradicts the streaming bound and `add_table`/post-write `autofit` are its named exclusions); `openpyxl` (the client-register round-trip ingest under `contextlib.closing`); `lxml` (the dialect-profiled container XML, `c14n2` the byte-reproducible egress, `isoschematron.Schematron` the mandated-metadata oracle); `functools.cache` (the per-dialect compiled Schematron bytes); `pydantic` (`TypeAdapter` the `ContainerPayload` gate, `.errors()` the structured `loc` evidence); `beartype` (the `_GUARD` render contract); `collections.Counter`/`itertools.pairwise` (the duplicate multiset and gap detection); runtime (`identity.ContentIdentity.key`, `lanes.LanePolicy` the instance offload, `faults.RuntimeRail`); `composition/sheet#SHEET` (`TitleBlock`); `core/receipt#RECEIPT` (`ArtifactReceipt.Register`).
- Growth: a new project-defined or record suitability code is one `documented` policy row admitting the token onto the `project` case — never a new S-code enum, a fabricated `RecordCode`, or a silent `malformed` drop; a new published family one `PublishedPrefix` member; a new CDE state one `ContainerState` member reaching `_STATE_HEX`; a new container-metadata field one `InformationContainer` slot reaching `row`/`metadata_rows`/`spreadsheet_row` and the `ContainerPayload` band; a new coverage rule one `AuditRule` member plus one `_rule_faults` arm; a new register operation one `RegisterOp` case with its payload and one `_composed` arm; a new XML dialect one `DialectProfile` row — namespace, localnames, and mandated set landing together, the Schematron derived per row; a new classification system one `ClassificationSystem` member; a new publication-index column one `_index_ops` `TableOp`; a new spreadsheet column one `_COLUMNS` entry (width and empty-frame schema derived for free); a new receipt scalar one slot on the shared `ArtifactReceipt.Register` case. Zero new surface — the register grows by case, row, member, and derived projection, never by method.
- Boundary: no artifact production beyond the register/index/manifest close (sheets arrive emitted from `composition/sheet#SHEET`, QTO/schedule frames from `drawing/schedule#SCHEDULE`), no IFC authoring (`csharp:Rasm.Bim` owns the model; the register composes container facts at the wire), no COBie-SpreadsheetML authoring (the COBie `DialectProfile` profiles the metadata XML only), no classification-table authoring (`specification/classify#CODE` owns the code tables), no publication-table render authority (`visualization/table#TABLE` owns the great-tables render; the register composes the `.build()` seam), no data sourcing (rows arrive as payloads, workbooks, or drawn sheets and cross the one gate), no content-key minting the runtime owns, and no second scheduler beside the `core/plan#PLAN` lane. A seven-of-eight `SuitabilityCode` subset that drops `S5`, a fabricated closed `RecordCode`, a `_value2member_map_` membership probe, a bare-`str` suitability the interior re-parses, a `Result.to_option` casualty erasure at the workbook seam, a dialect that renames nothing, a `constant_memory`/`in_memory` contradiction, and a first-failure abort where the coverage verdict accumulates are each foreclosed by the correct form above.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
import re
from collections import Counter
from contextlib import closing
from datetime import datetime
from enum import StrEnum
from functools import cache, reduce
from io import BytesIO
from itertools import pairwise
from typing import Final, Literal, NotRequired, ReadOnly, Required, Self, TypedDict, Unpack, assert_never
from zipfile import BadZipFile

import polars as pl
from beartype import BeartypeConf, beartype
from beartype.roar import BeartypeCallHintViolation
from builtins import frozendict
from expression import Error, Nothing, Ok, Option, Result, Some, case, tag, tagged_union
from expression.collections import Block, Map
from msgspec import Struct, json, msgpack
from pydantic import TypeAdapter, ValidationError

from rasm.artifacts.composition.sheet import TitleBlock
from rasm.artifacts.core.plan import Admission, ArtifactWork
from rasm.artifacts.core.receipt import ArtifactReceipt
from rasm.artifacts.visualization.table import TableFormat, TableOp, TablePlan, Theme
from rasm.runtime.faults import RuntimeRail
from rasm.runtime.identity import ContentIdentity, ContentKey
from rasm.runtime.lanes import LanePolicy
from rasm.runtime.workers import Kernel, KernelTrait

lazy from lxml import etree, isoschematron
lazy import xlsxwriter
lazy from openpyxl import load_workbook

# --- [TYPES] ----------------------------------------------------------------------------


class ContainerState(StrEnum):  # Four ISO 19650 CDE container states
    WIP = "WIP"
    SHARED = "Shared"
    PUBLISHED = "Published"
    ARCHIVE = "Archive"


class SuitabilityCode(StrEnum):  # BS EN ISO 19650-2:2018 NA Table NA.1, exact — S5 withdrawn, kept for cardinality
    S0 = "S0"
    S1 = "S1"
    S2 = "S2"
    S3 = "S3"
    S4 = "S4"
    S5 = "S5"
    S6 = "S6"
    S7 = "S7"


class PublishedPrefix(StrEnum):  # A authorized-and-accepted, B partially-accepted-with-comments
    AUTHORIZED = "A"
    PARTIAL = "B"


class RevisionKind(StrEnum):  # NA.4.3 — P preliminary (WIP/Shared), C contractual (Published)
    PRELIMINARY = "P"
    CONTRACTUAL = "C"


class ClassificationSystem(StrEnum):  # ISO 12006-2 reference; code tables live in the classify owner
    UNICLASS_2015 = "Uniclass 2015"
    ISO_12006_2 = "ISO 12006-2"
    OMNICLASS = "OmniClass"
    MASTERFORMAT = "MasterFormat"
    UNIFORMAT = "UniFormat"


class ContainerDialect(StrEnum):  # keys the _DIALECT profile rows — each member a distinct XML contract
    ISO_19650 = "iso19650"
    BS_1192 = "bs1192"
    COBIE = "cobie"


class RenderScope(StrEnum):  # which container set the native register streams
    FULL = "full"  # every admitted row — the audit-grade history register
    LATEST = "latest"  # revision-latest per reference — the issued drawing register


class DeltaChange(StrEnum):  # Issue-delta classification vocabulary
    ADDED = "added"
    REVISED = "revised"
    REGRESSED = "regressed"
    WITHDRAWN = "withdrawn"


class AuditRule(StrEnum):  # one closed vocabulary spans container and set-level checks
    STATUS_REVISION = "status_revision"
    WITHDRAWN = "withdrawn"
    UNCLASSIFIED = "unclassified"
    CONTRACTUAL = "contractual"
    NON_MONOTONIC = "non_monotonic"
    DUPLICATE = "duplicate"
    GAP = "gap"


# `ContainerPayload` admits naming once; suitability/revision remain raw until their closed parsers bind them.
class ContainerPayload(TypedDict, closed=True):
    project: Required[ReadOnly[str]]
    originator: Required[ReadOnly[str]]
    functional: Required[ReadOnly[str]]
    spatial: Required[ReadOnly[str]]
    form: Required[ReadOnly[str]]
    discipline: Required[ReadOnly[str]]
    number: Required[ReadOnly[str]]
    suitability: Required[ReadOnly[str]]
    revision: Required[ReadOnly[str]]
    title: NotRequired[ReadOnly[str]]
    purpose: NotRequired[ReadOnly[str]]
    classification: NotRequired[ReadOnly[str]]
    classification_system: NotRequired[ReadOnly[str]]
    asset_key: NotRequired[ReadOnly[str]]
    issued: NotRequired[ReadOnly[str]]
    author: NotRequired[ReadOnly[str]]
    checker: NotRequired[ReadOnly[str]]
    approver: NotRequired[ReadOnly[str]]


# --- [CONSTANTS] ------------------------------------------------------------------------
_FIELD_SEP: Final[str] = "-"  # BS 1192 container-reference field separator
_SCHEMATRON_NS: Final[str] = "http://purl.oclc.org/dsdl/schematron"
_SVRL_NS: Final[str] = "http://purl.oclc.org/dsdl/svrl"
_COLUMNS: Final[tuple[str, ...]] = (
    "Reference",
    "Title",
    "Form",
    "Number",
    "Discipline",
    "Suitability",
    "State",
    "Revision",
    "Classification System",
    "Classification",
    "Purpose",
    "Asset Key",
    "Issued",
    "Author",
    "Checker",
    "Approver",
)
_FRAME_FIELDS: Final[tuple[str, ...]] = tuple(column.lower().replace(" ", "_") for column in _COLUMNS)
# `_COL` is the one column-index correspondence every reader derives from `_COLUMNS`.
_COL: Final[frozendict[str, int]] = frozendict({name: at for at, name in enumerate(_COLUMNS)})
_A4_PAPER: Final[int] = 9  # XlsxWriter paper-size code for an ISO 5457 A4 register
_MAX_WIDTH: Final[int] = 48  # `set_column` ceiling under `constant_memory`
_PAD: Final[int] = 2
_CREATED: Final[datetime] = datetime(1980, 1, 1)  # deterministic OOXML package metadata; `Register.issued` carries the legal date
# `_ASSET_URI` composes each co-identified sheet link from its `asset_key`.
_ASSET_URI: Final[str] = "rasm-artifact://"
# Module-level grammars compile once.
_REVISION: Final[re.Pattern[str]] = re.compile(r"^(?P<kind>[PC])(?P<rev>\d{2,})(?:\.(?P<ver>\d{2,}))?$")
_PUBLISHED: Final[re.Pattern[str]] = re.compile(r"^(?P<prefix>[AB])(?P<ordinal>\d+)$")
_ORDINAL: Final[re.Pattern[str]] = re.compile(r"^(?P<stem>.*?)(?P<ordinal>\d+)$")
_ENCODER: Final = json.Encoder()  # Audit-report JSON egress
_MSGPACK: Final = msgpack.Encoder()  # Canonical key preimage, length/count-framed by the codec
# `_GUARD` exposes malformed render input to the offload fault classifier.
_GUARD: Final = beartype(conf=BeartypeConf(violation_type=BeartypeCallHintViolation))

# --- [TABLES] ---------------------------------------------------------------------------


class SuitabilityRow(Struct, frozen=True, gc=False):  # one NA Table NA.1 row — the correspondence the S-band derives from
    state: ContainerState
    description: str
    withdrawn: bool
    revision_kind: RevisionKind


# `_SUITABILITY` is the single code-to-state/description/withdrawn/revision correspondence.
_SUITABILITY: Final[frozendict[SuitabilityCode, SuitabilityRow]] = frozendict({
    SuitabilityCode.S0: SuitabilityRow(ContainerState.WIP, "Initial status", False, RevisionKind.PRELIMINARY),
    SuitabilityCode.S1: SuitabilityRow(ContainerState.SHARED, "Suitable for coordination", False, RevisionKind.PRELIMINARY),
    SuitabilityCode.S2: SuitabilityRow(ContainerState.SHARED, "Suitable for information", False, RevisionKind.PRELIMINARY),
    SuitabilityCode.S3: SuitabilityRow(ContainerState.SHARED, "Suitable for review and comment", False, RevisionKind.PRELIMINARY),
    SuitabilityCode.S4: SuitabilityRow(ContainerState.SHARED, "Suitable for stage approval", False, RevisionKind.PRELIMINARY),
    SuitabilityCode.S5: SuitabilityRow(ContainerState.SHARED, "Withdrawn", True, RevisionKind.PRELIMINARY),
    SuitabilityCode.S6: SuitabilityRow(ContainerState.SHARED, "Suitable for PIM authorization", False, RevisionKind.PRELIMINARY),
    SuitabilityCode.S7: SuitabilityRow(ContainerState.SHARED, "Suitable for AIM authorization", False, RevisionKind.PRELIMINARY),
})
# Admission membership derives from `_SUITABILITY` without a parallel roster.
_S_VALUES: Final[frozenset[str]] = frozenset(code.value for code in SuitabilityCode)
_CLASS_VALUES: Final[frozenset[str]] = frozenset(system.value for system in ClassificationSystem)
# `_STATE_HEX` keys the publication fill band on CDE state.
_STATE_HEX: Final[frozendict[ContainerState, str]] = frozendict({
    ContainerState.WIP: "#FEF3C7",
    ContainerState.SHARED: "#DBEAFE",
    ContainerState.PUBLISHED: "#DCFCE7",
    ContainerState.ARCHIVE: "#E5E7EB",
})
_DELTA_HEX: Final[frozendict[DeltaChange, str]] = frozendict({
    DeltaChange.ADDED: "#DCFCE7",
    DeltaChange.REVISED: "#DBEAFE",
    DeltaChange.REGRESSED: "#FEF3C7",
    DeltaChange.WITHDRAWN: "#FEE2E2",
})
_FRAME_SCHEMA: Final[frozendict[str, pl.DataType]] = frozendict({name: pl.String for name in _FRAME_FIELDS})
_DELTA_SCHEMA: Final[frozendict[str, pl.DataType]] = frozendict({**_FRAME_SCHEMA, "change": pl.String, "supersedes": pl.String})


class DialectProfile(Struct, frozen=True):
    # `DialectProfile` keeps namespace, structure, mandates, and localname renames inseparable.
    uri: str
    root: str
    container: str
    required: tuple[str, ...]
    names: frozendict[str, str] = frozendict()


_DIALECT: Final[frozendict[ContainerDialect, DialectProfile]] = frozendict({
    ContainerDialect.ISO_19650: DialectProfile(
        uri="https://rasm.dev/schema/iso19650/register",
        root="informationContainerSet",
        container="informationContainer",
        # ISO 19650-2 §5.1.7 mandates status, revision, and classification per container.
        required=("suitability", "state", "revision", "classificationSystem", "classificationCode"),
    ),
    ContainerDialect.BS_1192: DialectProfile(
        uri="https://rasm.dev/schema/bs1192",
        root="documentRegister",
        container="document",
        required=("suitability", "revision"),
        names=frozendict({"functionalBreakdown": "volume", "spatialBreakdown": "levelLocation"}),
    ),
    ContainerDialect.COBIE: DialectProfile(
        uri="https://docs.buildingsmart.org/cobie",
        root="Documents",
        container="Document",
        required=("title", "state", "revision"),
        names=frozendict({"project": "Facility", "title": "Name", "issued": "CreatedOn", "purpose": "Category"}),
    ),
})

# --- [ERRORS] ---------------------------------------------------------------------------


# `RegisterFault` carries offending references/coordinates; `aggregate` is its associative audit combination.
# `unwired` names the sheet sources admitted without their DATA parent keys; `undated` carries the conflicting
# container issue dates no single register date resolves.
@tagged_union(frozen=True)
class RegisterFault:
    tag: Literal[
        "malformed", "misstated", "withdrawn_issued", "non_contractual", "non_monotonic", "duplicate", "unclassified", "gap", "unwired",
        "undated", "aggregate"
    ] = tag()
    malformed: frozenset[str] = case()
    misstated: frozenset[str] = case()
    withdrawn_issued: frozenset[str] = case()
    non_contractual: frozenset[str] = case()
    non_monotonic: frozenset[str] = case()
    duplicate: frozenset[str] = case()
    unclassified: frozenset[str] = case()
    gap: frozenset[str] = case()
    unwired: frozenset[str] = case()
    undated: frozenset[str] = case()
    aggregate: tuple["RegisterFault", ...] = case()

    @staticmethod
    def _members(fault: "RegisterFault", /) -> tuple["RegisterFault", ...]:
        return fault.aggregate if fault.tag == "aggregate" else (fault,)

    @staticmethod
    def combined(left: "RegisterFault", right: "RegisterFault", /) -> "RegisterFault":
        return RegisterFault(aggregate=(*RegisterFault._members(left), *RegisterFault._members(right)))

    @staticmethod
    def accumulated(*faults: Option["RegisterFault"]) -> Option["RegisterFault"]:
        held = Block.of_seq(faults).choose(lambda fault: fault)
        return Nothing if held.is_empty() else Some(held.reduce(RegisterFault.combined))


# --- [MODELS] ---------------------------------------------------------------------------


class PublishedCode(Struct, frozen=True):  # Parametric A1..An / B1..Bn published family
    prefix: PublishedPrefix
    ordinal: int

    def render(self) -> str:
        return f"{self.prefix.value}{self.ordinal}"


@tagged_union(frozen=True)
class Suitability:
    # `Suitability` closes standard bands while admitting documented NA extension codes on one project case.
    tag: Literal["shared", "published", "project"] = tag()
    shared: SuitabilityCode = case()
    published: PublishedCode = case()
    project: tuple[str, ContainerState] = case()

    @classmethod
    def parse(cls, code: str, /, *, documented: frozendict[str, ContainerState] = frozendict()) -> Result[Self, RegisterFault]:
        # Admission orders the S-band, A/B grammar, then documented NA §NA.4.2 extensions.
        token = code.strip().upper()
        return (
            Ok(cls(shared=SuitabilityCode(token)))
            if token in _S_VALUES
            else Ok(cls(published=PublishedCode(prefix=PublishedPrefix(found["prefix"]), ordinal=int(found["ordinal"]))))
            if (found := _PUBLISHED.match(token)) is not None and int(found["ordinal"]) > 0
            else Ok(cls(project=(token, state)))
            if (state := documented.get(token)) is not None
            else Error(RegisterFault(malformed=frozenset({code})))
        )

    @property
    def code(self) -> str:
        match self:
            case Suitability(tag="shared", shared=shared):
                return shared.value
            case Suitability(tag="published", published=published):
                return published.render()
            case Suitability(tag="project", project=(token, _)):
                return token
            case _ as unreachable:
                assert_never(unreachable)

    @property
    def state(self) -> ContainerState:
        match self:
            case Suitability(tag="shared", shared=shared):
                return _SUITABILITY[shared].state
            case Suitability(tag="published"):
                return ContainerState.PUBLISHED
            case Suitability(tag="project", project=(_, state)):
                return state
            case _ as unreachable:
                assert_never(unreachable)

    @property
    def contractual(self) -> bool:
        return self.state is ContainerState.PUBLISHED

    @property
    def withdrawn(self) -> bool:
        match self:
            case Suitability(tag="shared", shared=shared):
                return _SUITABILITY[shared].withdrawn
            case _:
                return False


class RevisionCode(Struct, frozen=True):
    # NA.4.3 — P{NN}[.{NN}] preliminary (two-integer WIP version suffix, e.g. P02.05), C{NN} contractual.
    kind: RevisionKind
    revision: int
    version: int | None = None

    def render(self) -> str:
        base = f"{self.kind.value}{self.revision:02d}"
        return f"{base}.{self.version:02d}" if self.version is not None else base

    def succeeds(self, prior: "RevisionCode", /) -> bool:
        # contractual outranks preliminary; within a kind higher (revision, version) wins — not a string compare that mis-sorts C09 before C10.
        rank = lambda code: (code.kind is RevisionKind.CONTRACTUAL, code.revision, code.version or 0)
        return rank(self) > rank(prior)

    @classmethod
    def parse(cls, mark: str, /) -> Result[Self, RegisterFault]:
        return (
            Error(RegisterFault(malformed=frozenset({mark})))
            if (found := _REVISION.match(mark.strip().upper())) is None or (found["kind"] == RevisionKind.CONTRACTUAL.value and found["ver"] is not None)
            else Ok(
                cls(kind=RevisionKind(found["kind"]), revision=int(found["rev"]), version=int(found["ver"]) if found["ver"] is not None else None)
            )
        )


class Classification(Struct, frozen=True):  # Lean ISO 12006-2 reference; code tables live in the classify owner
    system: ClassificationSystem = ClassificationSystem.UNICLASS_2015
    code: str = ""
    title: str = ""

    @classmethod
    def admitted(cls, system: str, code: str, title: str = "", /) -> Result[Self, RegisterFault]:
        return (
            Ok(cls(system=ClassificationSystem(system or ClassificationSystem.UNICLASS_2015.value), code=code, title=title))
            if not system or system in _CLASS_VALUES
            else Error(RegisterFault(malformed=frozenset({f"classification-system:{system}"})))
        )


class NamingContext(Struct, frozen=True):  # Set-level BS 1192 naming fields
    originator: str = ""
    functional: str = ""
    spatial: str = ""
    form: str = ""


class InformationContainer(Struct, frozen=True):
    # `InformationContainer` joins BS 1192 identity, §5.1.7 metadata, asset identity, and title-block facts.
    project: str
    originator: str
    functional: str
    spatial: str
    form: str
    discipline: str
    number: str
    suitability: Suitability
    revision: RevisionCode
    classification: Classification = Classification()
    title: str = ""
    purpose: str = ""
    asset_key: str = ""
    sheet_total: str = ""
    issued: str = ""
    author: str = ""
    checker: str = ""
    approver: str = ""

    @property
    def reference(self) -> str:
        return _FIELD_SEP.join((self.project, self.originator, self.functional, self.spatial, self.form, self.discipline, self.number))

    @classmethod
    def admitted(cls, documented: frozendict[str, ContainerState] = frozendict(), /, **raw: Unpack[ContainerPayload]) -> Result[Self, RegisterFault]:
        # Positional-only `documented` cannot collide with the `Unpack` payload and governs project-code admission.
        try:  # Exemption: the pydantic TypeAdapter admission kernel — the one statement seam, every interior signature past it holding the admitted container.
            payload = _PAYLOAD.validate_python(raw)
        except ValidationError as fault:
            at = str(raw.get("number", "?"))
            return Error(RegisterFault(malformed=frozenset(f"{at}:{'/'.join(map(str, entry['loc']))}" for entry in fault.errors())))
        suitability = Suitability.parse(payload["suitability"], documented=documented)
        revision = RevisionCode.parse(payload["revision"])
        classification = Classification.admitted(payload.get("classification_system", ""), payload.get("classification", ""))
        match suitability, revision, classification:
            case Result(tag="ok", ok=suit), Result(tag="ok", ok=rev), Result(tag="ok", ok=classified):
                return Ok(
                    cls(
                        project=payload["project"],
                        originator=payload["originator"],
                        functional=payload["functional"],
                        spatial=payload["spatial"],
                        form=payload["form"],
                        discipline=payload["discipline"],
                        number=payload["number"],
                        suitability=suit,
                        revision=rev,
                        title=payload.get("title", ""),
                        purpose=payload.get("purpose", ""),
                        asset_key=payload.get("asset_key", ""),
                        issued=payload.get("issued", ""),
                        author=payload.get("author", ""),
                        checker=payload.get("checker", ""),
                        approver=payload.get("approver", ""),
                        classification=classified,
                    )
                )
            case _:
                severed = RegisterFault.accumulated(
                    suitability.swap().to_option(), revision.swap().to_option(), classification.swap().to_option()
                )
                return Error(severed.value)

    @classmethod
    def of_block(
        cls,
        block: TitleBlock,
        naming: NamingContext,
        suitability: Suitability,
        revision: RevisionCode,
        /,
        *,
        reference: str = "",
        classification: Classification = Classification(),
        asset_key: str = "",
    ) -> Self:
        identified = reference.rsplit(_FIELD_SEP, 6)
        project, originator, functional, spatial, form, discipline, number = (
            tuple(identified)
            if len(identified) == 7
            else (
                block.project,
                naming.originator,
                naming.functional,
                naming.spatial,
                naming.form,
                block.discipline,
                reference.removeprefix(f"{block.discipline}{_FIELD_SEP}") or block.sheet_number,
            )
        )
        return cls(
            project=project,
            originator=originator,
            functional=functional,
            spatial=spatial,
            form=form,
            discipline=discipline,
            number=number,
            suitability=suitability,
            revision=revision,
            classification=classification,
            title=block.sheet_title,
            purpose=block.status,
            asset_key=asset_key,
            sheet_total=block.sheet_total,
            issued=block.date,
            author=block.drawn_by,
            checker=block.checked_by,
            approver=block.approved_by,
        )

    def row(self) -> dict[str, object]:  # Register-frame row consumed by Polars and `TableNode`
        return dict(zip(_FRAME_FIELDS, self.spreadsheet_row(), strict=True))

    def spreadsheet_row(self) -> tuple[object, ...]:  # XlsxWriter row ordered by `_COLUMNS`
        return (
            self.reference,
            self.title,
            self.form,
            self.number,
            self.discipline,
            self.suitability.code,
            self.suitability.state.value,
            self.revision.render(),
            self.classification.system.value,
            self.classification.code,
            self.purpose,
            self.asset_key,
            self.issued,
            self.author,
            self.checker,
            self.approver,
        )

    def canonical(self) -> tuple[object, ...]:
        return (
            self.project,
            self.originator,
            self.functional,
            self.spatial,
            self.form,
            self.discipline,
            self.number,
            self.suitability.tag,
            self.suitability.code,
            self.suitability.state.value,
            self.revision.kind.value,
            self.revision.revision,
            self.revision.version,
            self.classification.system.value,
            self.classification.code,
            self.classification.title,
            self.title,
            self.purpose,
            self.asset_key,
            self.sheet_total,
            self.issued,
            self.author,
            self.checker,
            self.approver,
        )

    def metadata_rows(self) -> tuple[tuple[str, str], ...]:  # Single localname/value correspondence renamed by every dialect
        return (
            ("project", self.project),
            ("originator", self.originator),
            ("functionalBreakdown", self.functional),
            ("spatialBreakdown", self.spatial),
            ("form", self.form),
            ("discipline", self.discipline),
            ("number", self.number),
            ("title", self.title),
            ("suitability", self.suitability.code),
            ("state", self.suitability.state.value),
            ("revision", self.revision.render()),
            ("classificationSystem", self.classification.system.value),
            ("classificationCode", self.classification.code),
            ("purpose", self.purpose),
            ("assetKey", self.asset_key),
            ("issued", self.issued),
        )


_PAYLOAD: Final = TypeAdapter(ContainerPayload)


class SheetContext(Struct, frozen=True):  # Sheet aggregation over `TitleBlock` plus ISO naming context
    block: TitleBlock
    naming: NamingContext
    suitability: Suitability
    classification: Classification = Classification()
    asset_key: str = ""


class ContainerMeta(Struct, frozen=True):  # Project-level ISO 19650 XML header
    project: str = ""
    project_id: str = ""
    appointing_party: str = ""
    lead_party: str = ""
    stage: str = ""  # RIBA / ISO 19650 delivery stage
    milestone: str = ""  # Information-delivery milestone satisfied by the issue


def _sequence(container: InformationContainer, /) -> Option[tuple[str, int]]:
    return Option.of_optional(_ORDINAL.match(container.number)).map(
        lambda found: (
            _FIELD_SEP.join(
                (container.project, container.originator, container.functional, container.spatial, container.form, container.discipline, found["stem"])
            ),
            int(found["ordinal"]),
        )
    )


def _regressed(containers: tuple[InformationContainer, ...], /) -> Block[RegisterFault]:
    # `_regressed` flags any later row that does not succeed its prior reference revision.
    def step(acc: tuple[Map[str, RevisionCode], frozenset[str]], container: InformationContainer, /) -> tuple[Map[str, RevisionCode], frozenset[str]]:
        seen, flagged = acc
        prior = seen.try_find(container.reference)
        slipped = prior.is_some() and not container.revision.succeeds(prior.value)
        return seen.add(container.reference, container.revision), (flagged | {container.reference} if slipped else flagged)

    _, flagged = Block.of_seq(containers).fold(step, (Map.empty(), frozenset()))
    return Block.singleton(RegisterFault(non_monotonic=flagged)) if flagged else Block.empty()


def _duplicated(containers: tuple[InformationContainer, ...], /) -> Block[RegisterFault]:
    repeated = frozenset(
        f"{reference}@{revision}"
        for (reference, revision), count in Counter((c.reference, c.revision.render()) for c in containers).items()
        if count > 1
    )
    return Block.singleton(RegisterFault(duplicate=repeated)) if repeated else Block.empty()


def _gapped(containers: tuple[InformationContainer, ...], /) -> Block[RegisterFault]:
    sequences = Block.of_seq(containers).choose(_sequence).fold(
        lambda acc, row: acc.change(row[0], lambda held: Some(held.default_value(frozenset()) | {row[1]})), Map.empty()
    )
    gaps = frozenset(
        f"{series}:{missing}" for series, ordinals in sequences.items() for low, high in pairwise(sorted(ordinals)) for missing in range(low + 1, high)
    )
    return Block.singleton(RegisterFault(gap=gaps)) if gaps else Block.empty()


class AuditPolicy(Struct, frozen=True, gc=False):
    rules: tuple[AuditRule, ...]


STANDARD_AUDIT: Final[AuditPolicy] = AuditPolicy(
    rules=(AuditRule.STATUS_REVISION, AuditRule.WITHDRAWN, AuditRule.UNCLASSIFIED, AuditRule.NON_MONOTONIC, AuditRule.DUPLICATE, AuditRule.GAP)
)
CONTRACTUAL_AUDIT: Final[AuditPolicy] = AuditPolicy(rules=(*STANDARD_AUDIT.rules[:3], AuditRule.CONTRACTUAL, *STANDARD_AUDIT.rules[3:]))


def _rule_faults(rule: AuditRule, containers: tuple[InformationContainer, ...], /) -> Block[RegisterFault]:
    references = lambda predicate: frozenset(container.reference for container in containers if predicate(container))
    match rule:
        case AuditRule.STATUS_REVISION:
            faults = references(lambda c: c.suitability.contractual is not (c.revision.kind is RevisionKind.CONTRACTUAL))
            return Block.singleton(RegisterFault(misstated=faults)) if faults else Block.empty()
        case AuditRule.WITHDRAWN:
            faults = references(lambda c: c.suitability.withdrawn)
            return Block.singleton(RegisterFault(withdrawn_issued=faults)) if faults else Block.empty()
        case AuditRule.UNCLASSIFIED:
            faults = references(lambda c: not c.classification.code)
            return Block.singleton(RegisterFault(unclassified=faults)) if faults else Block.empty()
        case AuditRule.CONTRACTUAL:
            faults = references(lambda c: not c.suitability.contractual)
            return Block.singleton(RegisterFault(non_contractual=faults)) if faults else Block.empty()
        case AuditRule.NON_MONOTONIC:
            return _regressed(containers)
        case AuditRule.DUPLICATE:
            return _duplicated(containers)
        case AuditRule.GAP:
            return _gapped(containers)
        case _ as unreachable:
            assert_never(unreachable)


class RegisterEvidence(Struct, frozen=True, gc=False):
    # `RegisterEvidence` is the single audit/transmittal coverage projection; `checks` counts the rules that ran,
    # so `complete` is earned by executed checks and a zero-rule policy can never mint a vacuous pass.
    containers: int
    wip: int
    shared: int
    published: int
    archive: int
    contractual: int
    withdrawn: int
    duplicates: int
    checks: int
    complete: bool
    dominant_suitability: str
    latest_revision: str
    classification: str
    severed: Option[RegisterFault] = Nothing

    @classmethod
    def of(cls, register: "Register", severed: Option[RegisterFault], checks: int, /) -> Self:
        states = Block.of_seq(register.containers).fold(
            lambda acc, c: acc.change(c.suitability.state, lambda held: Some(held.default_value(0) + 1)), Map.empty()
        )
        counts = Counter((c.reference, c.revision.render()) for c in register.containers)
        return cls(
            containers=len(register.containers),
            wip=states.try_find(ContainerState.WIP).default_value(0),
            shared=states.try_find(ContainerState.SHARED).default_value(0),
            published=states.try_find(ContainerState.PUBLISHED).default_value(0),
            archive=states.try_find(ContainerState.ARCHIVE).default_value(0),
            contractual=sum(1 for c in register.containers if c.suitability.contractual),
            withdrawn=sum(1 for c in register.containers if c.suitability.withdrawn),
            duplicates=sum(1 for count in counts.values() if count > 1),
            checks=checks,
            complete=severed.is_none() and checks > 0,
            dominant_suitability=register.suitability.code,
            latest_revision=register.revision.render(),
            classification=register.classification.system.value,
            severed=severed,
        )

    @property
    def facts(self) -> dict[str, object]:  # native scalars the json.Encoder serializes unstringified, plus the gating cause
        return {
            "containers": self.containers,
            "wip": self.wip,
            "shared": self.shared,
            "published": self.published,
            "archive": self.archive,
            "contractual": self.contractual,
            "withdrawn": self.withdrawn,
            "duplicates": self.duplicates,
            "checks": self.checks,
            "complete": self.complete,
            "dominant_suitability": self.dominant_suitability,
            "latest_revision": self.latest_revision,
            "classification": self.classification,
            "severed": self.severed.map(lambda fault: fault.tag).default_value("ok"),
        }


class Composed(Struct, frozen=True):  # Single evidence struct consumed by `_emit`
    data: bytes
    kind: str
    sheets: int
    suitability: str
    revision: str
    classification: str
    validation: str = "unchecked"  # Container-XML conformance state; non-XML ops remain unchecked


@tagged_union(frozen=True)
class RegisterOp:  # Closed delivery vocabulary lowered once into `Composed`
    tag: Literal["index", "container", "audit", "render", "delta"] = tag()
    index: tuple[Theme, TableFormat] = case()
    container: ContainerDialect = case()
    audit: AuditPolicy = case()
    render: RenderScope = case()
    delta: tuple[tuple[InformationContainer, ...], Theme, TableFormat] = case()

    @staticmethod
    def Index(theme: Theme = Theme(), fmt: TableFormat = TableFormat.HTML) -> "RegisterOp":
        return RegisterOp(index=(theme, fmt))

    @staticmethod
    def Container(dialect: ContainerDialect = ContainerDialect.ISO_19650) -> "RegisterOp":
        return RegisterOp(container=dialect)

    @staticmethod
    def Audit(policy: AuditPolicy = STANDARD_AUDIT) -> "RegisterOp":
        return RegisterOp(audit=policy)

    @staticmethod
    def Render(scope: RenderScope = RenderScope.LATEST) -> "RegisterOp":
        return RegisterOp(render=scope)

    @staticmethod
    def Delta(prior: tuple[InformationContainer, ...], theme: Theme = Theme(), fmt: TableFormat = TableFormat.HTML) -> "RegisterOp":
        return RegisterOp(delta=(prior, theme, fmt))


# --- [SERVICES] -------------------------------------------------------------------------


class Register(Struct, frozen=True):
    # `Register` joins its operation, container set, project header, and issue metadata; `lane` arrives projected via
    # LanePolicy.of(context) at the composition root — a capacity literal has no owner.
    op: RegisterOp
    lane: LanePolicy
    containers: tuple[InformationContainer, ...] = ()
    parents: tuple[ContentKey, ...] = ()  # upstream sheet keys threaded at construction (DATA edges)
    meta: ContainerMeta = ContainerMeta()
    suitability: Suitability = Suitability(shared=SuitabilityCode.S2)
    revision: RevisionCode = RevisionCode(kind=RevisionKind.PRELIMINARY, revision=1)
    classification: Classification = Classification()
    issued: str = ""
    # `documented` governs NA §NA.4.2 project-code admission across every raw-code seam.
    documented: frozendict[str, ContainerState] = frozendict()

    @classmethod
    def admit(
        cls,
        *sources: "ContainerPayload | bytes | bytearray | SheetContext | tuple[str, TitleBlock, str, str]",
        lane: LanePolicy,
        op: RegisterOp = RegisterOp.Index(),
        naming: NamingContext = NamingContext(),
        meta: ContainerMeta = ContainerMeta(),
        documented: frozendict[str, ContainerState] = frozendict(),
        parents: tuple[ContentKey, ...] = (),
        issued: str = "",
        suitability: Suitability | None = None,
        revision: RevisionCode | None = None,
        classification: Classification | None = None,
    ) -> Result["Register", RegisterFault]:
        # One shape-dispatched accumulating gate: payload rows and workbook bytes admit per row, `SheetContext`
        # and `SheetSet.registered()` tuples normalize under the one set-level `naming` policy. `parents` carries
        # caller-owned DATA edges — a sheet-backed admission with no parent keys accumulates `unwired`, because a
        # register over sheets the plan cannot see schedules before its producers. `issued` is the caller's legal
        # register date; omitted, it derives through `_dated` from the admitted containers, exactly as the
        # suitability/revision/classification issue metadata derives through `_issue_meta` unless stated.
        admitted = Block.of_seq(sources).collect(lambda source: _admitted_source(source, naming, documented))
        sheeted = Block.of_seq(sources).choose(_sheet_reference)
        wired = admitted if parents or sheeted.is_empty() else admitted.cons(Error(RegisterFault(unwired=frozenset(sheeted))))
        return _accumulated(wired).bind(
            lambda containers: _dated(containers, issued).map(
                lambda date: cls(
                    op=op,
                    lane=lane,
                    containers=containers,
                    parents=parents,
                    meta=meta,
                    documented=documented,
                    issued=date,
                    **_issue_meta(containers, suitability, revision, classification),
                )
            )
        )

    def emit(self, /) -> ArtifactWork:
        # Re-issued sets retain raw `asset_key` boundary evidence for member-level elision.
        return ArtifactWork(
            key=self.key, work=self._emit, parents=self.parents, admission=Admission(keyed=None), cost=float(len(self.containers) or 1)
        )

    @property
    def key(self) -> ContentKey:
        # PRE-RUN input identity stays public because transmittal names it among aggregate parents;
        # `parents` (plan DATA edges) and `lane` (runtime capacity) are declared derived, outside the preimage.
        return ContentIdentity.key(f"register-{self.op.tag}", _canon(self))

    async def _emit(self) -> RuntimeRail[ArtifactReceipt]:
        # One instance-lane offload preserves the rail and `receipt.slot == node.key`.
        crossed = await self.lane.offload(Kernel.of(_composed, KernelTrait.RELEASING), self)
        return crossed.map(
            lambda composed: ArtifactReceipt.Register(
                self.key,
                composed.kind,
                composed.sheets,
                composed.suitability,
                composed.revision,
                composed.classification,
                composed.validation,
                len(composed.data),
            )
        )

    @property
    def frame(self) -> pl.DataFrame:
        return (
            pl.from_dicts((container.row() for container in self.containers), schema=_FRAME_SCHEMA)
            .with_columns(pl.col("number").str.extract(r"(\d+)").cast(pl.UInt32, strict=False).alias("_ordinal"))
            .sort(["discipline", "_ordinal", "number"], nulls_last=True)
            .drop("_ordinal")
        )

    def latest(self) -> tuple[InformationContainer, ...]:
        # `latest` keeps each reference's highest revision for render, delta, and callers.
        return _latest(self.containers)

    def audited(self, policy: AuditPolicy = STANDARD_AUDIT, /) -> RegisterEvidence:
        casualties = Block.of_seq(policy.rules).collect(lambda rule: _rule_faults(rule, self.containers))
        severed = Nothing if casualties.is_empty() else Some(casualties.reduce(RegisterFault.combined))
        return RegisterEvidence.of(self, severed, len(policy.rules))

    @property
    def evidence(self) -> RegisterEvidence:  # Default-policy coverage verdict composed by transmittal
        return self.audited()


# --- [OPERATIONS] -----------------------------------------------------------------------


def _accumulated(results: Block[Result[InformationContainer, RegisterFault]], /) -> Result[tuple[InformationContainer, ...], RegisterFault]:
    # `_accumulated` returns all rows or the associative combination of every casualty.
    faults = results.choose(lambda outcome: outcome.swap().to_option())
    return Ok(tuple(results.choose(lambda outcome: outcome.to_option()))) if faults.is_empty() else Error(faults.reduce(RegisterFault.combined))


def _admitted_source(
    source: "ContainerPayload | bytes | bytearray | SheetContext | tuple[str, TitleBlock, str, str]",
    naming: NamingContext,
    documented: frozendict[str, ContainerState],
    /,
) -> Block[Result[InformationContainer, RegisterFault]]:
    match source:
        case bytes() | bytearray() as blob:
            return _ingested(bytes(blob), documented)
        case SheetContext() | (str(), TitleBlock(), str(), str()) as sheet:
            return Block.singleton(_contextual(sheet, naming, documented))
        case {**payload}:
            return Block.singleton(InformationContainer.admitted(documented, **payload))
        case _ as unreachable:
            assert_never(unreachable)


def _sheet_reference(
    source: "ContainerPayload | bytes | bytearray | SheetContext | tuple[str, TitleBlock, str, str]", /
) -> Option[str]:
    # Names the sheet-backed sources whose plan DATA edges `admit` requires; payload and workbook rows carry none.
    match source:
        case SheetContext(block=block):
            return Some(block.sheet_number or block.sheet_title)
        case (str() as reference, TitleBlock(), str(), str()):
            return Some(reference)
        case _:
            return Nothing


def _issue_meta(
    containers: tuple[InformationContainer, ...],
    suitability: Suitability | None,
    revision: RevisionCode | None,
    classification: Classification | None,
    /,
) -> dict[str, Suitability | RevisionCode | Classification]:
    # register-level issue metadata derives from the LEADING admitted container — the one whose revision succeeds
    # every sibling — so the index header, the canon preimage, and the emitted receipt attest the admitted set,
    # never the class defaults; an explicit caller argument overrides the derivation per axis, and an empty set
    # keeps the field defaults by omission.
    lead = reduce(lambda held, nxt: nxt if nxt.revision.succeeds(held.revision) else held, containers) if containers else None
    derived: dict[str, Suitability | RevisionCode | Classification] = {} if lead is None else {
        "suitability": lead.suitability, "revision": lead.revision, "classification": lead.classification
    }
    stated = {"suitability": suitability, "revision": revision, "classification": classification}
    return derived | {name: value for name, value in stated.items() if value is not None}


def _dated(containers: tuple[InformationContainer, ...], issued: str, /) -> Result[str, RegisterFault]:
    # One authoritative register issue date: the caller's stated legal date wins, a single container date derives,
    # a dateless set stays the editable draft, and discordant container dates with no stated date refuse as `undated`.
    dated = frozenset(container.issued for container in containers if container.issued)
    match issued, len(dated):
        case (stated, _) if stated:
            return Ok(stated)
        case ("", 0):
            return Ok("")
        case ("", 1):
            return Ok(next(iter(dated)))
        case _:
            return Error(RegisterFault(undated=dated))


def _ingested(data: bytes, documented: frozendict[str, ContainerState], /) -> Block[Result[InformationContainer, RegisterFault]]:
    # Exemption: one provider boundary converts exact workbook-open/read failures into the admission rail.
    try:
        with closing(load_workbook(BytesIO(data), read_only=True, data_only=True)) as book:
            return Block.of_seq(
                InformationContainer.admitted(documented, **_row_payload(row, at))
                for at, row in enumerate(book[book.sheetnames[0]].iter_rows(min_row=2, values_only=True), start=2)
            )
    except (BadZipFile, IndexError, KeyError, OSError, ValueError) as fault:
        return Block.singleton(Error(RegisterFault(malformed=frozenset({f"workbook:{type(fault).__name__}"}))))


def _row_payload(row: tuple[object, ...], at: int, /) -> ContainerPayload:
    # `_row_payload` splits the reference and assigns `row:{n}` when the required number is blank; `asset_key` round-trips
    # through its own value column because `values_only` reads drop the presentation hyperlink that also carries it.
    cells = tuple(str(cell) if cell is not None else "" for cell in row)
    padded = (*cells, *("" for _ in range(len(_COLUMNS) - len(cells))))
    parts = padded[0].split(_FIELD_SEP)
    fields = (*parts, *("" for _ in range(7 - len(parts))))
    return ContainerPayload(
        project=fields[0],
        originator=fields[1],
        functional=fields[2],
        spatial=fields[3],
        form=fields[4],
        discipline=fields[5],
        number=fields[6] or f"row:{at}",
        suitability=padded[_COL["Suitability"]],
        revision=padded[_COL["Revision"]],
        title=padded[_COL["Title"]],
        purpose=padded[_COL["Purpose"]],
        classification=padded[_COL["Classification"]],
        classification_system=padded[_COL["Classification System"]],
        asset_key=padded[_COL["Asset Key"]],
        issued=padded[_COL["Issued"]],
        author=padded[_COL["Author"]],
        checker=padded[_COL["Checker"]],
        approver=padded[_COL["Approver"]],
    )


def _contextual(
    entry: "SheetContext | tuple[str, TitleBlock, str, str]", naming: NamingContext, documented: frozendict[str, ContainerState], /
) -> Result[InformationContainer, RegisterFault]:
    # `_contextual` preserves parsed `SheetContext` values and admits raw `SheetSet.registered()` codes.
    match entry:
        case SheetContext(block=block) as ctx:
            latest = block.revisions[-1].mark if block.revisions else "P01"
            return RevisionCode.parse(latest).map(
                lambda rev: InformationContainer.of_block(
                    block, ctx.naming, ctx.suitability, rev, classification=ctx.classification, asset_key=ctx.asset_key
                )
            )
        case (str() as reference, TitleBlock() as block, str() as suit, str() as rev):
            admitted = Suitability.parse(suit, documented=documented)
            revised = RevisionCode.parse(rev or "P01")
            match admitted, revised:
                case Result(tag="ok", ok=parsed), Result(tag="ok", ok=revision):
                    return Ok(InformationContainer.of_block(block, naming, parsed, revision, reference=reference))
                case _:
                    severed = RegisterFault.accumulated(admitted.swap().to_option(), revised.swap().to_option())
                    return Error(severed.value)
        case _ as unreachable:
            assert_never(unreachable)


def _latest(containers: tuple[InformationContainer, ...], /) -> tuple[InformationContainer, ...]:
    folded = Block.of_seq(containers).fold(
        lambda acc, c: acc.change(
            c.reference, lambda held, incoming=c: Some(incoming if held.is_none() or incoming.revision.succeeds(held.value.revision) else held.value)
        ),
        Map.empty(),
    )
    return tuple(folded.values())


def _theme_facet(theme: Theme, /) -> tuple[object, ...]:
    font = None if theme.font is None else (theme.font.tag, getattr(theme.font, theme.font.tag))
    marks = theme.footnote_marks if isinstance(theme.footnote_marks, tuple) else theme.footnote_marks.value
    return (
        theme.style,
        theme.color,
        theme.striping,
        font,
        theme.header_align,
        theme.all_caps,
        theme.vertical_scale,
        theme.horizontal_scale,
        theme.outline,
        marks,
        theme.css,
    )


def _canon(register: "Register", /) -> bytes:
    # Msgpack length/count framing prevents concatenation ambiguity across the canonical preimage.
    facet: tuple[object, ...]
    match register.op:
        case RegisterOp(tag="index", index=(theme, fmt)):
            facet = (fmt.value, _theme_facet(theme))
        case RegisterOp(tag="container", container=dialect):
            facet = (dialect.value,)
        case RegisterOp(tag="audit", audit=policy):
            facet = tuple(rule.value for rule in policy.rules)
        case RegisterOp(tag="render", render=scope):
            facet = (scope.value,)
        case RegisterOp(tag="delta", delta=(prior, theme, fmt)):
            facet = (fmt.value, _theme_facet(theme), tuple(container.canonical() for container in prior))
        case _ as unreachable:
            assert_never(unreachable)
    return _MSGPACK.encode((
        register.op.tag,
        facet,
        tuple(container.canonical() for container in register.containers),
        register.meta,
        tuple(sorted((token, state.value) for token, state in register.documented.items())),
        register.issued,
        register.suitability.code,
        register.revision.render(),
        register.classification,
    ))


@_GUARD
def _composed(register: "Register", /) -> Composed:  # Single pure render fold
    validation = "unchecked"  # Non-XML ops mint no validatable container
    match register.op:
        case RegisterOp(tag="index", index=(theme, fmt)):
            data = TablePlan(frame=register.frame, ops=_index_ops(register), fmt=fmt, theme=theme).build()
        case RegisterOp(tag="container", container=dialect):
            profile = _DIALECT[dialect]
            tree = _container_document(register, profile)
            # per-call validator: the ISO engine is not thread-re-entrant on error_log
            schema = isoschematron.Schematron(etree.fromstring(_container_schema(dialect)), store_report=True)
            valid = schema.validate(tree)
            failed = int(schema.validation_report.xpath("count(//svrl:failed-assert)", namespaces={"svrl": _SVRL_NS}))
            validation = "valid" if valid else f"invalid:{failed}"
            data = etree.tostring(tree, method="c14n2")  # C14N bytes: the container content-addresses byte-reproducibly run-to-run
        case RegisterOp(tag="audit", audit=policy):
            data = _ENCODER.encode(register.audited(policy).facts)
        case RegisterOp(tag="render", render=scope):
            data = _workbook(register, scope)
        case RegisterOp(tag="delta", delta=(prior, theme, fmt)):
            data = TablePlan(frame=_delta_frame(register, prior), ops=_delta_ops(register), fmt=fmt, theme=theme).build()
        case _ as unreachable:
            assert_never(unreachable)
    return Composed(
        data=data,
        kind=register.op.tag,
        sheets=len(register.containers),
        suitability=register.suitability.code,
        revision=register.revision.render(),
        classification=register.classification.system.value,
        validation=validation,
    )


def _index_ops(register: "Register", /) -> tuple[TableOp, ...]:
    # Publication styling groups on discipline and colors the separate state column.
    return (
        TableOp.Header(f"Drawing Register — {register.meta.project}", subtitle=f"{register.revision.render()} · {register.issued}"),
        TableOp.Stub(rowname="reference", group="discipline"),
        TableOp.Spanner("Identification", columns=["title", "form", "number"]),
        TableOp.Spanner("Status", columns=["suitability", "state", "revision", "classification_system", "classification"]),
        TableOp.Spanner("Responsibility", columns=["author", "checker", "approver"]),
        TableOp.Color(columns=["state"], palette=[_STATE_HEX[state] for state in ContainerState], domain=[state.value for state in ContainerState]),
        TableOp.GrandSummary({"containers": pl.col("reference").count().alias("containers")}),
        TableOp.SourceNote(f"ISO 19650-2 information containers · {register.classification.system.value}"),
    )


def _delta_frame(register: "Register", prior: tuple[InformationContainer, ...], /) -> pl.DataFrame:
    before = frozendict({container.reference: container for container in _latest(prior)})
    now = frozendict({container.reference: container for container in register.latest()})
    rows = (
        *(
            # a reference that arrives already withdrawn is a withdrawal, never an addition — same suitability
            # read the revised arm below applies before revision ordering
            now[ref].row()
            | {"change": (DeltaChange.WITHDRAWN if now[ref].suitability.withdrawn else DeltaChange.ADDED).value, "supersedes": ""}
            for ref in now
            if ref not in before
        ),
        *(
            now[ref].row()
            | {
                # a currently-issued S5/withdrawn container is a withdrawal, read before revision ordering
                "change": (
                    DeltaChange.WITHDRAWN
                    if now[ref].suitability.withdrawn
                    else DeltaChange.REVISED
                    if now[ref].revision.succeeds(before[ref].revision)
                    else DeltaChange.REGRESSED
                ).value,
                "supersedes": before[ref].revision.render(),
            }
            for ref in now
            if ref in before and now[ref] != before[ref]
        ),
        *(before[ref].row() | {"change": DeltaChange.WITHDRAWN.value, "supersedes": ""} for ref in before if ref not in now),
    )
    return pl.from_dicts(rows, schema=_DELTA_SCHEMA)


def _delta_ops(register: "Register", /) -> tuple[TableOp, ...]:
    return (
        TableOp.Header(f"Issue Delta — {register.meta.project}", subtitle=f"{register.revision.render()} · {register.issued}"),
        TableOp.Stub(rowname="reference", group="change"),
        TableOp.Spanner("Status", columns=["suitability", "state", "revision", "classification_system", "classification", "supersedes"]),
        TableOp.Color(columns=["change"], palette=[_DELTA_HEX[change] for change in DeltaChange], domain=[change.value for change in DeltaChange]),
        TableOp.GrandSummary({"containers": pl.col("reference").count().alias("containers")}),
        TableOp.SourceNote("ISO 19650-2 issue delta — added / revised / regressed / withdrawn since the prior issue"),
    )


@cache
def _container_schema(dialect: ContainerDialect, /) -> bytes:
    # Immutable Schematron bytes derive one `sch:assert` per renamed profile requirement.
    # Each render compiles its own validator because provider diagnostic state is not thread-re-entrant.
    profile = _DIALECT[dialect]
    sch = lambda local: etree.QName(_SCHEMATRON_NS, local)
    schema = etree.Element(sch("schema"), nsmap={"sch": _SCHEMATRON_NS})
    etree.SubElement(schema, sch("ns"), prefix="ic", uri=profile.uri)
    rule = etree.SubElement(etree.SubElement(schema, sch("pattern")), sch("rule"), context=f"ic:{profile.container}")
    for local in profile.required:
        named = profile.names.get(local, local)
        etree.SubElement(rule, sch("assert"), test=f"ic:{named} != ''").text = f"missing mandated {dialect.value} {named}"
    return etree.tostring(schema)


def _container_document(register: "Register", profile: DialectProfile, /) -> "etree._Element":
    # Clark-notation `SubElement` nodes keep namespace, structure, and escaping under the dialect profile.
    qname = lambda local: etree.QName(profile.uri, profile.names.get(local, local))
    root = etree.Element(qname(profile.root), nsmap={None: profile.uri})
    header = etree.SubElement(root, qname("project"))
    for local, value in (
        ("name", register.meta.project),
        ("reference", register.meta.project_id),
        ("appointingParty", register.meta.appointing_party),
        ("leadAppointedParty", register.meta.lead_party),
        ("stage", register.meta.stage),
        ("milestone", register.meta.milestone),
        ("revision", register.revision.render()),
        ("issued", register.issued),
    ):
        etree.SubElement(header, qname(local)).text = value
    containers = etree.SubElement(root, qname("containers"))
    for container in register.containers:
        node = etree.SubElement(containers, qname(profile.container))
        node.set("reference", container.reference)
        for local, value in container.metadata_rows():
            etree.SubElement(node, qname(local)).text = value
    return root


def _column_widths(containers: tuple[InformationContainer, ...], /) -> tuple[float, ...]:
    # Column widths derive before `constant_memory` flush because post-write `autofit` cannot recover rows.
    rows = (_COLUMNS, *(tuple(str(cell) for cell in container.spreadsheet_row()) for container in containers))
    return tuple(min(_MAX_WIDTH, max(len(row[col]) for row in rows) + _PAD) for col in range(len(_COLUMNS)))


def _running_header(register: "Register", /) -> tuple[str, str]:
    # Header/footer values escape `&` before XlsxWriter interpolates `&P`/`&N`/`&D` field codes.
    project = register.meta.project.replace("&", "&&")
    return f"&L{project}&RPage &P of &N", f"&L{register.revision.render()}&C&D&R{register.issued}"


def _workbook(register: "Register", scope: RenderScope, /) -> bytes:
    # `constant_memory` streams completed rows to spill and packages the `BytesIO` sink once at close.
    containers = register.latest() if scope is RenderScope.LATEST else register.containers
    header, footer = _running_header(register)
    sink = BytesIO()
    with xlsxwriter.Workbook(sink, {"constant_memory": True}) as book:  # Context exit packages the ZIP once
        sheet = book.add_worksheet("Register")
        head = book.add_format({"bold": True, "bg_color": "#1F2937", "font_color": "#FFFFFF", "border": 1})
        link = book.get_default_url_format()  # Shared hyperlink style
        published = book.add_format({"bg_color": _STATE_HEX[ContainerState.PUBLISHED]})
        archived = book.add_format({"bg_color": _STATE_HEX[ContainerState.ARCHIVE]})
        for col, width in enumerate(_column_widths(containers)):  # set_column before the streamed rows flush
            sheet.set_column(col, col, width)
        sheet.write_row(0, 0, _COLUMNS, head)
        for index, container in enumerate(containers, start=1):
            row = container.spreadsheet_row()
            if container.asset_key:  # Reference links to the co-identified artifact; remaining cells stream from column 1
                sheet.write_url(index, _COL["Reference"], f"{_ASSET_URI}{container.asset_key}", link, string=container.reference)
                sheet.write_row(index, _COL["Reference"] + 1, row[_COL["Reference"] + 1 :])
            else:
                sheet.write_row(index, 0, row)
        last = max(len(containers), 1)
        sheet.conditional_format(
            1,
            _COL["Suitability"],
            last,
            _COL["Suitability"],
            {"type": "text", "criteria": "begins with", "value": PublishedPrefix.AUTHORIZED.value, "format": published},
        )
        sheet.conditional_format(
            1, _COL["State"], last, _COL["State"], {"type": "text", "criteria": "containing", "value": ContainerState.ARCHIVE.value, "format": archived}
        )
        # Excel validation hard-stops state, soft-checks extensible suitability, and guides revision grammar.
        sheet.data_validation(
            1,
            _COL["State"],
            last,
            _COL["State"],
            {
                "validate": "list",
                "source": [state.value for state in ContainerState],
                "error_type": "stop",
                "input_title": "CDE state",
                "input_message": "One of the four ISO 19650 container states.",
            },
        )
        sheet.data_validation(
            1,
            _COL["Suitability"],
            last,
            _COL["Suitability"],
            {
                "validate": "list",
                "source": [code.value for code in SuitabilityCode],
                "error_type": "information",
                "input_title": "Suitability",
                "input_message": "Standard S-band; published A/B and documented project codes extend it.",
            },
        )
        sheet.data_validation(
            1,
            _COL["Revision"],
            last,
            _COL["Revision"],
            {"validate": "any", "input_title": "Revision", "input_message": "Preliminary P{NN}[.{NN}] or contractual C{NN}."},
        )
        sheet.autofilter(0, 0, last, len(_COLUMNS) - 1)
        sheet.freeze_panes(1, 0)
        sheet.set_landscape()
        sheet.set_paper(_A4_PAPER)
        sheet.fit_to_pages(1, 0)
        sheet.repeat_rows(0, 0)
        sheet.set_header(header)
        sheet.set_footer(footer)
        book.set_properties({
            "title": f"Drawing Register — {register.meta.project}",
            "subject": register.revision.render(),
            "author": register.meta.lead_party,
            "created": _CREATED,
        })
        if register.issued:  # an issued register is read-only; a draft stays editable under the validation dropdowns
            sheet.protect(options={"autofilter": True, "sort": True, "select_locked_cells": True})
    return sink.getvalue()
```

## [03]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)

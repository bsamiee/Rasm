# [PY_ARTIFACTS_REGISTER]

`Register` is the ISO 19650 delivery-register / drawing-index owner — the information-container metadata authority at the delivery boundary, ONE owner over the closed `RegisterOp` `expression.tagged_union`: `Index` renders the sheet-index publication table through `visualization/table#TABLE`, `Container` serializes the information-container metadata as an `lxml` COBie/BS 1192 XML tree that is `isoschematron.Schematron`-validated against the owned required-metadata rules and emitted `c14n2`-canonical byte-reproducibly, `Audit` folds the coverage verdict over the owned suitability/revision vocabularies through the accumulating `RegisterFault.combined`, and `Render` streams the native `xlsxwriter` register spreadsheet or round-trips a client register through `openpyxl` — each arm dispatched by one total `match` and folded ONCE into the `Composed` evidence struct `emit` and `_emit` share, never a second render. No ISO 19650 register library exists, so this owner composes the delivery algebra over the owned standards vocabularies plus `polars`/`great-tables`/`xlsxwriter`/`openpyxl`/`lxml`, never a re-implemented byte emitter.

Sheet-index facts aggregate from `composition/sheet#SHEET` `TitleBlock` through `from_title_block`; `visualization/table#TABLE` `TablePlan` owns the publication render composed at the `.build()` seam; `specification/classify#CLASSIFY` owns the full MasterFormat/UniFormat/OmniClass code tables the lean `Classification` reference points at; `document/model#MODEL` `TableNode` lowers from the one `polars` `frame` the `Index` also styles; the runtime mints the content key through `identity.ContentIdentity`. `Register.emit` schedules as one `core/plan#PLAN` `ArtifactWork` whose parents are the constituent `InformationContainer.asset_key` content keys, and the produced receipt IS the new `core/receipt#RECEIPT` `ArtifactReceipt.Register(key, kind, sheets, suitability, revision, classification, validation, bytes)` case the `[07]-[SEAM_UNIFICATION]` target admits — one case on the one family, never a parallel rail. `delivery/transmittal#TRANSMITTAL` composes the `RegisterEvidence` coverage verdict as the issued manifest.

## [01]-[INDEX]

- [01]-[REGISTER]: the ISO 19650 delivery-register axis — the exact-NA-cardinality suitability/revision/state vocabularies over the one `_SUITABILITY` correspondence, the `InformationContainer` and `Register` owners with accumulating `admit`/`of_sheets` ingress, and the closed `RegisterOp` folded once through the `to_thread`-offloaded `_composed` into the new `ArtifactReceipt.Register` case.

## [02]-[REGISTER]

- Owner: `Register` the one delivery-register owner discriminating over the closed `RegisterOp` `tagged_union`, every arm folded ONCE into the `Composed` evidence struct both `emit` and `_emit` read — no second render. Status vocabularies are OWNED closed `StrEnum`+`frozendict` families authored to the EXACT BS EN ISO 19650-2:2018 NA Table NA.1 cardinality: `SuitabilityCode` the `S0..S7` band whose `_SUITABILITY` `frozendict` is the ONE `(state, description, withdrawn, revision_kind)` correspondence every consumer derives from, `_S_VALUES` derived by comprehension, never a `_value2member_map_` probe; `PublishedCode` the `(prefix, ordinal)` value object carrying the OPEN ordinal, never a fabricated closed enum; `Suitability` the closed `shared`/`published`/`project` union whose `project` case is the NA extension band a documented client/record code lands on, never a fabricated `RecordCode` claiming ISO standing; `RevisionCode` the `P{NN}[.{NN}]`/`C{NN}` value object whose `succeeds` orders revisions off the value. `InformationContainer` is the frozen ISO 19650 container owner (BS 1192 naming fields, the metadata triad, the `asset_key` co-identification, the aggregated sheet-index facts); `ContainerMeta` the project-level header the `Container` XML roots on; `RegisterEvidence` the coverage-verdict receipt folded once and read by `Audit`, the receipt, and `delivery/transmittal#TRANSMITTAL`. `polars` owns the `frame`, `great-tables` the publication index (via `visualization/table#TABLE`), `lxml` the container XML, `xlsxwriter` the native register, `openpyxl` the client round-trip; the delivery algebra is this owner's composition over those engines, never a re-implemented emitter.
- Cases: `RegisterOp` cases matched by one total `match`, each lowering to the one `Composed` fold — never a per-format register builder, never a per-operation `_emit`, never a second render for the receipt. `Index(theme, fmt)` lowers `frame` into `visualization/table#TABLE` via the `_index_ops` `TableOp` sequence and `.build()` (one render, one content key). `Container(dialect)` serializes the namespaced `lxml` metadata tree (each value a `SubElement` the serializer escapes, never an f-string splice), validates it against the owned `_container_schema` (one `sch:assert` per `_REQUIRED_META` §5.1.7 localname), folds the `valid`/`invalid:{n}` verdict onto `Composed.validation`, and emits `c14n2` bytes so the container content-addresses byte-reproducibly. `Audit(policy)` folds per-container `_container_faults` plus set-level `_sequence_faults` through the `RegisterFault.combined` monoid into `RegisterEvidence`. `Render(merge)` streams the `xlsxwriter.Workbook(constant_memory=True)` register — sized columns, suitability/state `conditional_format`, owned-vocabulary `data_validation`, per-reference `write_url` asset links, page-of-N running header, issued-register `protect` — and a present `merge` round-trips a client register through `openpyxl` under the revision-latest fold.
- Entry: `Register.emit` returns an `ArtifactWork` whose `work` is `_emit` and whose `parents` are the constituent sheet content keys — the register IS the single content-keyed production entry a `core/plan#PLAN` node wraps, never a per-operation entrypoint. `_emit` offloads `_composed` through the runtime thread lane under the module-level `_GATE` `CapacityLimiter` (the explicit thread bound, never the per-loop default) so the GIL-releasing render never blocks the loop, mints the content key PRE-RUN over `(op ⊕ containers ⊕ meta)` through `ContentIdentity.of`, and returns the `ArtifactReceipt.Register` case directly — no second `contribute` re-renders. Both ingress constructors `admit`/`of_sheets` return `Result[Register, RegisterFault]` under the accumulating disposition so a batch reports every casualty. `async_boundary` narrows on `_FAULTS` so a malformed frame, XML fault, vocabulary miss, bad workbook, or `_GUARD`-rejected container each discriminates into its own `BoundaryFault` case.
- Auto: `_composed` is the ONE `_GUARD`-contracted total `match` over `register.op` both `emit` and `_emit` read, each arm returning `Composed` with the register-level facts folded once (the schema state `unchecked` for every arm but `Container`) so the body stays one `match`-shaped path, never an inline `try`/`except` ladder beside it. `frame` is the one `polars.DataFrame` ordered by discipline then numeric `_sheet_ordinal` (deterministic, not insertion order) that the `Index` `TablePlan` styles AND the `document/model#MODEL` `TableNode` lowers from — the sheet-index authorable into the document tree without a second projection. `audited` flat-maps per-container `_container_faults` through `Block.collect`, appends the set-level `_sequence_faults`, and reduces through the associative `RegisterFault.combined` monoid into `RegisterEvidence` — the accumulating disposition the coverage verdict needs. `InformationContainer.admitted` is the one boundary: the single `try` is the `_PAYLOAD.validate_python` gate, `Suitability.parse`.`map2`(`RevisionCode.parse`) binds the raw codes before construction so the interior never re-parses; `_ingest`/`_merged` compose the same admission through `Block.choose`/`Map.change`, never a mutable accumulator.
- Receipt: each operation contributes `core/receipt#RECEIPT` off the one `Composed` fold through the new `ArtifactReceipt.Register(key, kind, sheets, suitability, revision, classification, validation, bytes)` case — the register-container evidence (sheet count, suitability, revision, classification) plus the `Container`-XML conformance state (`valid`/`invalid:{n}`/`unchecked`) the ISO 19650 primitive requires. `Composed.kind` carries the op tag onto the facts so the arms stay routable by the `MeterProvider` per-kind instrument, and `_emit` mints the single case off the one render exactly as `drawing/schedule#SCHEDULE` mints its `ArtifactReceipt.Schedule` — one case on the shared family, never a per-kind facts `Struct`, a phantom `ArtifactReceipt.of`, a parallel receipt type, or a second render. `core/plan#PLAN` reads the case as the content-keyed evidence its sub-graph elision distinguishes hit from miss on.
- Packages: `expression` (the `Suitability`/`RegisterFault`/`RegisterOp` unions, the `Result`/`Option` admission and coverage rails, `.map2` the applicative code admission, `Block`/`Map` the fault fold and merge); `msgspec` (the frozen value objects, `json.Encoder` the audit-report egress); `polars` (the register `frame` the `TablePlan` styles and the document lowering reads, a settled input the register shapes but never sources); `great-tables` (through `visualization/table#TABLE` `TablePlan`, composed at the `build` seam, never a direct reach); `xlsxwriter` (the native register under `constant_memory`, which forbids only `add_table`/`autofit`); `openpyxl` (the client-register round-trip ingest); `lxml` (the namespaced container XML, `c14n2` the byte-reproducible egress, `isoschematron.Schematron` the required-metadata oracle); `functools.cache` (the compiled-once Schematron bytes); `pydantic` (`TypeAdapter` the `ContainerPayload` gate); `beartype` (the `_GUARD` boundary contract); `anyio` (the `CapacityLimiter` bounded offload); `collections.Counter`/`itertools.pairwise` (the duplicate multiset and gap detection); runtime (`identity.ContentIdentity`, `faults.RuntimeRail`/`async_boundary`); `composition/sheet#SHEET` (`TitleBlock`); `core/receipt#RECEIPT` (`ArtifactReceipt.Register`).
- Growth: a new project-defined or record suitability code is one `documented` policy row (the `frozendict` the raw-row `admit` and the client round-trip both thread through `Suitability.parse`) admitting the token onto the `project` case — never a new S-code enum, a fabricated `RecordCode`, or a silent `malformed` drop; a new published family one `PublishedPrefix` member; a new CDE state one `ContainerState` member reaching `_STATE_HEX`; a new container-metadata field one `InformationContainer` slot reaching `row`/`metadata_rows`/`spreadsheet_row` and the `ContainerPayload` band; a new coverage rule one `RegisterFault` case plus one `_container_faults`/`_sequence_faults` row; a new register operation one `RegisterOp` case with its payload and one `_composed` arm; a new XML profile one `ContainerDialect` member and one `_NSMAP` row; a new classification system one `ClassificationSystem` member; a new publication-index column one `_index_ops` `TableOp`; a new spreadsheet column one `_COLUMNS` entry (width derived for free); a newly-mandated ISO 19650 metadata field one `_REQUIRED_META` localname the Schematron derives one `sch:assert` per; a new receipt scalar one slot on the shared `ArtifactReceipt.Register` case. Zero new surface — the register grows by case, row, member, and derived projection, never by method.
- Boundary: no artifact production beyond the register/index/manifest close (sheets arrive emitted from `composition/sheet#SHEET`, QTO/schedule frames from `drawing/schedule#SCHEDULE`), no IFC authoring (`csharp:Rasm.Bim` owns the model; the register composes container facts at the wire), no COBie-SpreadsheetML authoring (`ContainerDialect` marks the XML profile only), no classification-table authoring (`specification/classify#CLASSIFY` owns the code tables), no publication-table render authority (`visualization/table#TABLE` owns the great-tables render; the register composes the `.build()` seam), no data sourcing (the `polars.DataFrame` arrives settled), no content-key minting the runtime owns, and no second scheduler beside the `core/plan#PLAN` lane. A seven-of-eight `SuitabilityCode` subset that drops `S5`, a fabricated closed `RecordCode`, a `_value2member_map_` membership probe, a bare-`str` suitability the interior re-parses, a double-render `of`-then-`contribute`, and a first-failure abort where the coverage verdict accumulates are each foreclosed by the correct form above.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
import re
from collections import Counter
from collections.abc import Iterable
from enum import StrEnum
from functools import cache
from io import BytesIO
from itertools import pairwise
from typing import Final, Literal, NotRequired, ReadOnly, Required, Self, TypedDict, Unpack, assert_never

import polars as pl
from beartype import BeartypeConf, beartype
from beartype.roar import BeartypeCallHintViolation
from expression import Error, Nothing, Ok, Option, Result, Some, case, tag, tagged_union
from expression.collections import Block, Map
from msgspec import Struct, json
from pydantic import TypeAdapter, ValidationError

from rasm.runtime.identity import CANONICAL_POLICY, ContentIdentity, ContentKey
from rasm.runtime.lanes import LanePolicy, Modality
from rasm.runtime.resilience import RetryClass
from rasm.runtime.faults import RuntimeRail, async_boundary

from artifacts.composition.sheet import TitleBlock
from artifacts.core.plan import Admission, ArtifactWork
from artifacts.core.receipt import ArtifactReceipt
from artifacts.visualization.table import TableFormat, TableOp, TablePlan, Theme

lazy from lxml import etree, isoschematron
lazy from lxml.etree import LxmlError
lazy import xlsxwriter
lazy from xlsxwriter.exceptions import XlsxWriterException
lazy from openpyxl import load_workbook

# --- [TYPES] ----------------------------------------------------------------------------


class ContainerState(StrEnum):  # the four ISO 19650 CDE container states
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


class ClassificationSystem(StrEnum):  # the ISO 12006-2 reference; the code tables live in the classify owner
    UNICLASS_2015 = "Uniclass 2015"
    ISO_12006_2 = "ISO 12006-2"
    OMNICLASS = "OmniClass"
    MASTERFORMAT = "MasterFormat"
    UNIFORMAT = "UniFormat"


class ContainerDialect(StrEnum):  # the Container XML namespace profile
    ISO_19650 = "iso19650"
    BS_1192 = "bs1192"
    COBIE = "cobie"


# the closed naming payload admitted ONCE through _PAYLOAD; suitability/revision arrive as raw strings the parsers bind at the seam.
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


# --- [CONSTANTS] ------------------------------------------------------------------------
_FIELD_SEP: Final[str] = "-"  # the BS 1192 container-reference field separator
_NS_19650: Final[str] = "https://rasm.dev/schema/iso19650/register"
_NSMAP: Final[Map[str | None, str]] = Map.of_seq([
    (None, _NS_19650),
    ("cobie", "https://docs.buildingsmart.org/cobie"),
    ("bs1192", "https://rasm.dev/schema/bs1192"),
])
_SCHEMATRON_NS: Final[str] = "http://purl.oclc.org/dsdl/schematron"
# the ISO 19650-2 §5.1.7 mandated per-container metadata localnames — one `sch:assert` per row; a dropped or blank element fails conformance. VALUE coherence stays `audited`'s.
_REQUIRED_META: Final[tuple[str, ...]] = ("suitability", "state", "revision")
_COLUMNS: Final[tuple[str, ...]] = (
    "Reference",
    "Title",
    "Form",
    "Number",
    "Discipline",
    "Suitability",
    "State",
    "Revision",
    "Classification",
    "Purpose",
    "Issued",
    "Author",
    "Checker",
    "Approver",
)
_SUITABILITY_COL: Final[int] = _COLUMNS.index("Suitability")
_STATE_COL: Final[int] = _COLUMNS.index("State")
_REFERENCE_COL: Final[int] = _COLUMNS.index("Reference")
_REVISION_COL: Final[int] = _COLUMNS.index("Revision")
_A4_PAPER: Final[int] = 9  # the xlsxwriter paper-size code for the ISO 5457 A4 plotted register
_MAX_WIDTH: Final[int] = 48  # the set_column width ceiling (constant_memory forbids the post-write autofit pass)
_PAD: Final[int] = 2
# the content-addressed artifact-link scheme write_url composes `{_ASSET_URI}{asset_key}` per co-identified sheet.
_ASSET_URI: Final[str] = "rasm-artifact://"
# the module-level compiled grammars — never an inline re.compile per parse
_REVISION: Final[re.Pattern[str]] = re.compile(r"^(?P<kind>[PC])(?P<rev>\d{2,})(?:\.(?P<ver>\d{2,}))?$")
_PUBLISHED: Final[re.Pattern[str]] = re.compile(r"^(?P<prefix>[AB])(?P<ordinal>\d+)$")
_ENCODER: Final = json.Encoder()  # the audit-report JSON egress
# the boundary contract: a malformed container inside a well-tagged op raises BeartypeCallHintViolation the _FAULTS-narrowed async_boundary discriminates.
_GUARD: Final = beartype(conf=BeartypeConf(violation_type=BeartypeCallHintViolation))
# the engine raise tuple async_boundary narrows on; naming the lxml/xlsxwriter roots reifies those lazy modules at load — the deliberate cost of a precise fault split.
_FAULTS: Final[tuple[type[BaseException], ...]] = (LxmlError, XlsxWriterException, KeyError, ValueError, BeartypeCallHintViolation)

# --- [TABLES] ---------------------------------------------------------------------------


class SuitabilityRow(Struct, frozen=True, gc=False):  # one NA Table NA.1 row — the correspondence the S-band derives from
    state: ContainerState
    description: str
    withdrawn: bool
    revision_kind: RevisionKind


# the ONE code -> (state, description, withdrawn, revision_kind) correspondence every consumer derives from.
_SUITABILITY: Final[Map[SuitabilityCode, SuitabilityRow]] = Map.of_seq([
    (SuitabilityCode.S0, SuitabilityRow(ContainerState.WIP, "Initial status", False, RevisionKind.PRELIMINARY)),
    (SuitabilityCode.S1, SuitabilityRow(ContainerState.SHARED, "Suitable for coordination", False, RevisionKind.PRELIMINARY)),
    (SuitabilityCode.S2, SuitabilityRow(ContainerState.SHARED, "Suitable for information", False, RevisionKind.PRELIMINARY)),
    (SuitabilityCode.S3, SuitabilityRow(ContainerState.SHARED, "Suitable for review and comment", False, RevisionKind.PRELIMINARY)),
    (SuitabilityCode.S4, SuitabilityRow(ContainerState.SHARED, "Suitable for stage approval", False, RevisionKind.PRELIMINARY)),
    (SuitabilityCode.S5, SuitabilityRow(ContainerState.SHARED, "Withdrawn", True, RevisionKind.PRELIMINARY)),
    (SuitabilityCode.S6, SuitabilityRow(ContainerState.SHARED, "Suitable for PIM authorization", False, RevisionKind.PRELIMINARY)),
    (SuitabilityCode.S7, SuitabilityRow(ContainerState.SHARED, "Suitable for AIM authorization", False, RevisionKind.PRELIMINARY)),
])
# the derived admission-membership vocabulary — comprehension over the one edit site above; shared/withdrawn ride the row, no parallel frozenset.
_S_VALUES: Final[frozenset[str]] = frozenset(c.value for c in SuitabilityCode)
_CLASS_VALUES: Final[frozenset[str]] = frozenset(s.value for s in ClassificationSystem)
# the state-band fill keyed on CDE state (WIP amber, shared blue, published green, archive grey).
_STATE_HEX: Final[Map[ContainerState, str]] = Map.of_seq([
    (ContainerState.WIP, "#FEF3C7"),
    (ContainerState.SHARED, "#DBEAFE"),
    (ContainerState.PUBLISHED, "#DCFCE7"),
    (ContainerState.ARCHIVE, "#E5E7EB"),
])

# --- [ERRORS] ---------------------------------------------------------------------------


# the closed coverage/admission fault vocabulary — each case its offending container-reference set so the root recovers on the cause; aggregate is the associative audit combination.
@tagged_union(frozen=True)
class RegisterFault:
    tag: Literal["malformed", "misstated", "withdrawn_issued", "non_monotonic", "duplicate", "unclassified", "gap", "aggregate"] = tag()
    malformed: frozenset[str] = case()
    misstated: frozenset[str] = case()
    withdrawn_issued: frozenset[str] = case()
    non_monotonic: frozenset[str] = case()
    duplicate: frozenset[str] = case()
    unclassified: frozenset[str] = case()
    gap: frozenset[str] = case()
    aggregate: tuple["RegisterFault", ...] = case()

    @staticmethod
    def _members(fault: "RegisterFault", /) -> tuple["RegisterFault", ...]:
        return fault.aggregate if fault.tag == "aggregate" else (fault,)

    @staticmethod
    def combined(left: "RegisterFault", right: "RegisterFault", /) -> "RegisterFault":
        return RegisterFault(aggregate=(*RegisterFault._members(left), *RegisterFault._members(right)))


# --- [MODELS] ---------------------------------------------------------------------------


class PublishedCode(Struct, frozen=True):  # the A1..An / B1..Bn parametric published family, never a fixed enum
    prefix: PublishedPrefix
    ordinal: int

    def render(self) -> str:
        return f"{self.prefix.value}{self.ordinal}"


@tagged_union(frozen=True)
class Suitability:
    # the closed suitability family: an S-code, a published A/B code, or a documented project/record extension code the NA "codes may be expanded" clause admits.
    tag: Literal["shared", "published", "project"] = tag()
    shared: SuitabilityCode = case()
    published: PublishedCode = case()
    project: tuple[str, ContainerState] = case()

    @classmethod
    def parse(cls, code: str, /, *, documented: frozendict[str, ContainerState] = frozendict()) -> Result[Self, RegisterFault]:
        # the raw-code seam: S-band, then the A/B published grammar, then the project's documented extension band (NA §NA.4.2) carried as a policy value.
        token = code.strip().upper()
        if token in _S_VALUES:
            return Ok(cls(shared=SuitabilityCode(token)))
        if (match := _PUBLISHED.match(token)) is not None:
            return Ok(cls(published=PublishedCode(prefix=PublishedPrefix(match["prefix"]), ordinal=int(match["ordinal"]))))
        if (state := documented.get(token)) is not None:
            return Ok(cls(project=(token, state)))
        return Error(RegisterFault(malformed=frozenset({code})))

    @property
    def code(self) -> str:
        match self:
            case Suitability(tag="shared", shared=s):
                return s.value
            case Suitability(tag="published", published=p):
                return p.render()
            case Suitability(tag="project", project=(token, _)):
                return token
            case _ as unreachable:
                assert_never(unreachable)

    @property
    def state(self) -> ContainerState:
        match self:
            case Suitability(tag="shared", shared=s):
                return _SUITABILITY[s].state
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
            case Suitability(tag="shared", shared=s):
                return _SUITABILITY[s].withdrawn
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
        rank = lambda r: (r.kind is RevisionKind.CONTRACTUAL, r.revision, r.version or 0)
        return rank(self) > rank(prior)

    @classmethod
    def parse(cls, mark: str, /) -> Result[Self, RegisterFault]:
        if (match := _REVISION.match(mark.strip().upper())) is None:
            return Error(RegisterFault(malformed=frozenset({mark})))
        version = int(match["ver"]) if match["ver"] is not None else None
        return Ok(cls(kind=RevisionKind(match["kind"]), revision=int(match["rev"]), version=version))


class Classification(Struct, frozen=True):  # the lean ISO 12006-2 reference; the code tables live in the classify owner
    system: ClassificationSystem = ClassificationSystem.UNICLASS_2015
    code: str = ""
    title: str = ""


class InformationContainer(Struct, frozen=True):
    # the ISO 19650 information container — BS 1192 naming fields, the §5.1.7.c metadata triad, the asset-key co-identification, the aggregated TitleBlock facts.
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
        # documented is positional-only so it never collides with the `Unpack` payload keys — the extension band the suitability seam admits a `CR`/client code through.
        try:  # Exemption: the pydantic TypeAdapter admission kernel — the one statement seam, every interior signature past it holding the admitted container.
            payload = _PAYLOAD.validate_python(raw)
        except ValidationError:
            return Error(RegisterFault(malformed=frozenset({raw.get("number", "?")})))
        system = (
            ClassificationSystem(raw_sys)
            if (raw_sys := payload.get("classification_system", "")) in _CLASS_VALUES
            else ClassificationSystem.UNICLASS_2015
        )
        return Suitability.parse(payload["suitability"], documented=documented).map2(
            RevisionCode.parse(payload["revision"]),
            lambda suit, rev: cls(
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
                classification=Classification(system=system, code=payload.get("classification", "")),
            ),
        )

    @classmethod
    def from_title_block(cls, entry: "SheetEntry", /) -> Result[Self, RegisterFault]:
        # aggregate a drawn title block into a register row — latest revision parsed to a RevisionCode, falling to P01 on a bare status.
        block = entry.block
        latest = block.revisions[-1].mark if block.revisions else "P01"
        return (
            RevisionCode
            .parse(latest)
            .or_else_with(lambda _: RevisionCode.parse("P01"))
            .map(
                lambda rev: cls(
                    project=block.project,
                    originator=entry.originator,
                    functional=entry.functional,
                    spatial=entry.spatial,
                    form=entry.form,
                    discipline=block.discipline,
                    number=block.sheet_number,
                    suitability=entry.suitability,
                    revision=rev,
                    classification=entry.classification,
                    title=block.sheet_title,
                    purpose=block.status,
                    asset_key=entry.asset_key,
                    sheet_total=block.sheet_total,
                    issued=block.date,
                    author=block.drawn_by,
                    checker=block.checked_by,
                    approver=block.approved_by,
                )
            )
        )

    def row(self) -> dict[str, object]:  # the register-frame row polars and the document TableNode read
        return {
            "reference": self.reference,
            "title": self.title,
            "form": self.form,
            "number": self.number,
            "discipline": self.discipline,
            "suitability": self.suitability.code,
            "state": self.suitability.state.value,
            "revision": self.revision.render(),
            "classification": self.classification.code,
            "purpose": self.purpose,
            "issued": self.issued,
            "author": self.author,
            "checker": self.checker,
            "approver": self.approver,
        }

    def spreadsheet_row(self) -> tuple[object, ...]:  # the xlsxwriter row, ordered by _COLUMNS
        return (
            self.reference,
            self.title,
            self.form,
            self.number,
            self.discipline,
            self.suitability.code,
            self.suitability.state.value,
            self.revision.render(),
            self.classification.code,
            self.purpose,
            self.issued,
            self.author,
            self.checker,
            self.approver,
        )

    def metadata_rows(self) -> tuple[tuple[str, str], ...]:  # the ISO 19650 (localname, value) pairs the lxml build reads
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


class SheetEntry(Struct, frozen=True):  # the sheet-aggregation ingress — a TitleBlock plus the ISO naming context
    block: TitleBlock
    originator: str
    functional: str
    spatial: str
    form: str
    suitability: Suitability
    classification: Classification = Classification()
    asset_key: str = ""


class ContainerMeta(Struct, frozen=True):  # the project-level ISO 19650 header the Container XML is rooted on
    project: str = ""
    project_id: str = ""
    appointing_party: str = ""
    lead_party: str = ""
    stage: str = ""  # the RIBA / ISO 19650 delivery stage
    milestone: str = ""  # the information-delivery milestone the issue satisfies


class AuditPolicy(Struct, frozen=True):  # the coverage-audit requirements — POLICY_VALUES, never a per-check flag the body re-derives
    require_contractual: bool = False
    require_classification: bool = True
    require_sequence: bool = True


_DEFAULT_AUDIT: Final[AuditPolicy] = AuditPolicy()


class RegisterEvidence(Struct, frozen=True, gc=False):
    # the coverage-verdict receipt folded once — Audit encodes its facts, delivery/transmittal reads it.
    containers: int
    wip: int
    shared: int
    published: int
    archive: int
    contractual: int
    withdrawn: int
    duplicates: int
    complete: bool
    dominant_suitability: str
    latest_revision: str
    classification: str
    severed: Option[RegisterFault] = Nothing

    @classmethod
    def of(cls, register: "Register", severed: Option[RegisterFault], complete: bool, /) -> Self:
        states = Block.of_seq(register.containers).fold(
            lambda acc, c: acc.change(c.suitability.state, lambda held: Some(held.default_value(0) + 1)), Map.empty()
        )
        counts = Counter(c.reference for c in register.containers)
        return cls(
            containers=len(register.containers),
            wip=states.try_find(ContainerState.WIP).default_value(0),
            shared=states.try_find(ContainerState.SHARED).default_value(0),
            published=states.try_find(ContainerState.PUBLISHED).default_value(0),
            archive=states.try_find(ContainerState.ARCHIVE).default_value(0),
            contractual=sum(1 for c in register.containers if c.suitability.contractual),
            withdrawn=sum(1 for c in register.containers if c.suitability.withdrawn),
            duplicates=sum(1 for count in counts.values() if count > 1),
            complete=complete,
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
            "complete": self.complete,
            "dominant_suitability": self.dominant_suitability,
            "latest_revision": self.latest_revision,
            "classification": self.classification,
            "severed": self.severed.map(lambda fault: fault.tag).default_value("ok"),
        }


class Composed(Struct, frozen=True):  # the one evidence struct emit/_emit read — no second render
    data: bytes
    kind: str
    sheets: int
    suitability: str
    revision: str
    classification: str
    validation: str = "unchecked"  # the Container-XML schema-conformance state; the non-XML ops leave the default


@tagged_union(frozen=True)
class RegisterOp:  # the closed delivery vocabulary lowered once into Composed
    tag: Literal["index", "container", "audit", "render"] = tag()
    index: tuple[Theme, TableFormat] = case()
    container: ContainerDialect = case()
    audit: AuditPolicy = case()
    render: Option[bytes] = case()

    @staticmethod
    def Index(theme: Theme = Theme(), fmt: TableFormat = TableFormat.HTML) -> "RegisterOp":
        return RegisterOp(index=(theme, fmt))

    @staticmethod
    def Container(dialect: ContainerDialect = ContainerDialect.ISO_19650) -> "RegisterOp":
        return RegisterOp(container=dialect)

    @staticmethod
    def Audit(policy: AuditPolicy = _DEFAULT_AUDIT) -> "RegisterOp":
        return RegisterOp(audit=policy)

    @staticmethod
    def Render(merge: Option[bytes] = Nothing) -> "RegisterOp":
        return RegisterOp(render=merge)


# --- [SERVICES] -------------------------------------------------------------------------


class Register(Struct, frozen=True):
    # the register model plus the operation — the container set, the ISO 19650 project header, and the issue metadata.
    op: RegisterOp
    containers: tuple[InformationContainer, ...] = ()
    parents: tuple[ContentKey, ...] = ()  # upstream sheet keys threaded at construction (DATA edges)
    meta: ContainerMeta = ContainerMeta()
    suitability: Suitability = Suitability(shared=SuitabilityCode.S2)
    revision: RevisionCode = RevisionCode(kind=RevisionKind.PRELIMINARY, revision=1)
    classification: Classification = Classification()
    issued: str = ""
    # the project's documented extension codes (NA §NA.4.2) the raw-row admission and the client round-trip both read; empty for a standard-codes-only register.
    documented: frozendict[str, ContainerState] = frozendict()

    @classmethod
    def admit(
        cls,
        *payloads: ContainerPayload,
        op: RegisterOp = RegisterOp.Index(),
        meta: ContainerMeta = ContainerMeta(),
        documented: frozendict[str, ContainerState] = frozendict(),
        parents: tuple[ContentKey, ...] = (),
    ) -> Result["Register", RegisterFault]:
        # raw client-row ingress under `documented`; every casualty accumulated through the monoid, never aborting.
        # `parents` = the caller's DATA-edge sheet keys — the raw `asset_key` never re-parses into a `ContentKey`.
        admitted = Block.of_seq(payloads).map(lambda payload: InformationContainer.admitted(documented, **payload))
        return _accumulated(admitted).map(
            lambda containers: cls(op=op, containers=containers, parents=parents, meta=meta, documented=documented)
        )

    @classmethod
    def of_sheets(
        cls,
        entries: Iterable[SheetEntry],
        /,
        *,
        op: RegisterOp = RegisterOp.Index(),
        meta: ContainerMeta = ContainerMeta(),
        documented: frozendict[str, ContainerState] = frozendict(),
        parents: tuple[ContentKey, ...] = (),
    ) -> Result["Register", RegisterFault]:
        # `from_title_block` reads a pre-built `Suitability`, so admission needs no `documented` (retained for a later `Render` round-trip); `parents` threads the per-entry sheet keys.
        admitted = Block.of_seq(entries).map(InformationContainer.from_title_block)
        return _accumulated(admitted).map(
            lambda containers: cls(op=op, containers=containers, parents=parents, meta=meta, documented=documented)
        )

    def emit(self, /) -> ArtifactWork:
        # a re-issued set re-renders only changed members; the raw `asset_key` band stays boundary evidence.
        return ArtifactWork(key=self._key, work=self._emit, parents=self.parents, admission=Admission(keyed=None), cost=float(len(self.containers) or 1))

    @property
    def _key(self) -> ContentKey:
        # key-over-INPUT: canonical (op ⊕ containers ⊕ meta) PRE-RUN — never a key over the rendered index bytes.
        return ContentIdentity.of(f"register-{self.op.tag}", (self.op, self.containers, self.meta), policy=CANONICAL_POLICY)

    async def _emit(self) -> RuntimeRail[ArtifactReceipt]:
        return await async_boundary(f"register.{self.op.tag}", self._rendered, catch=_FAULTS)

    async def _rendered(self) -> ArtifactReceipt:
        # the _composed render crosses the runtime thread lane off the loop; the terminal receipt threads the PRE-RUN key (receipt.slot == node.key).
        crossed = await LanePolicy.offload(_composed, self, modality=Modality.THREAD, retry=RetryClass.OCCT)
        composed = crossed.default_with(_register_raise)
        key = self._key
        return ArtifactReceipt.Register(
            key,
            composed.kind,
            composed.sheets,
            composed.suitability,
            composed.revision,
            composed.classification,
            composed.validation,
            len(composed.data),
        )

    @property
    def frame(self) -> pl.DataFrame:
        # the ONE register frame the Index TablePlan styles and the document TableNode lowers from, ordered by
        # discipline then numeric `_sheet_ordinal` (deterministic, not insertion order) — the typed ordinal sorts `A-10` after `A-9` where a lexical cast would not.
        if not self.containers:
            return pl.DataFrame()
        ordered = sorted(self.containers, key=lambda c: (c.discipline, _sheet_ordinal(c) or 0, c.number))
        return pl.from_dicts([container.row() for container in ordered])

    def audited(self, policy: AuditPolicy = _DEFAULT_AUDIT, /) -> RegisterEvidence:
        casualties = Block.of_seq(self.containers).collect(lambda c: _container_faults(c, policy)).append(_sequence_faults(self.containers, policy))
        severed = Nothing if casualties.is_empty() else Some(casualties.reduce(RegisterFault.combined))
        return RegisterEvidence.of(self, severed, casualties.is_empty())

    @property
    def evidence(self) -> RegisterEvidence:  # the default-policy coverage verdict delivery/transmittal composes as the issued manifest
        return self.audited()


# --- [OPERATIONS] -----------------------------------------------------------------------


def _accumulated(results: Block[Result[InformationContainer, RegisterFault]], /) -> Result[tuple[InformationContainer, ...], RegisterFault]:
    # the accumulating disposition — combine every fault through the monoid, return the set only when the casualty set is empty; never an abort.
    faults = results.choose(lambda outcome: outcome.swap().to_option())
    return Ok(tuple(results.choose(lambda outcome: outcome.to_option()))) if faults.is_empty() else Error(faults.reduce(RegisterFault.combined))


@_GUARD
def _register_raise(fault: object) -> "Composed":
    # terminal collapse at the render boundary: an offload fault reconstructs the raise _FAULTS folds.
    raise ValueError(str(fault))


def _composed(register: Register) -> Composed:  # the one pure render fold both emit and _emit read
    validation = "unchecked"  # the non-XML ops mint no validatable container; only Container is schema-checked
    match register.op:
        case RegisterOp(tag="index", index=(theme, fmt)):
            data = TablePlan(frame=register.frame, ops=_index_ops(register), fmt=fmt, theme=theme).build()
        case RegisterOp(tag="container", container=dialect):
            tree = _container_document(register, dialect)
            schema = isoschematron.Schematron(
                etree.fromstring(_container_schema()), store_report=True
            )  # per-call: not re-entrant across the bounded _GATE threads
            validation = "valid" if schema.validate(tree) else f"invalid:{len(schema.error_log)}"
            data = etree.tostring(
                tree, method="c14n2"
            )  # C14N bytes: the container content-addresses byte-reproducibly run-to-run
        case RegisterOp(tag="audit", audit=policy):
            data = _ENCODER.encode(register.audited(policy).facts)
        case RegisterOp(tag="render", render=merge):
            data = _workbook(register, merge)
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


def _index_ops(register: Register, /) -> tuple[TableOp, ...]:
    # the publication sheet-index TableOp sequence — grouping and colouring one column would conflict, so state (a body column) colours and discipline groups.
    return (
        TableOp.Header(f"Drawing Register — {register.meta.project}", subtitle=f"{register.revision.render()} · {register.issued}"),
        TableOp.Stub(rowname="reference", group="discipline"),
        TableOp.Spanner("Identification", columns=["title", "form", "number"]),
        TableOp.Spanner("Status", columns=["suitability", "state", "revision", "classification"]),
        TableOp.Spanner("Responsibility", columns=["author", "checker", "approver"]),
        TableOp.Color(columns=["state"], palette=[_STATE_HEX[state] for state in ContainerState], domain=[state.value for state in ContainerState]),
        TableOp.GrandSummary({"containers": pl.col("reference").count().alias("containers")}),
        TableOp.SourceNote(f"ISO 19650-2 information containers · {register.classification.system.value}"),
    )


@cache
def _container_schema() -> bytes:
    # the ISO Schematron serialized ONCE (immutable, thread-safe) — one `sch:assert` per `_REQUIRED_META` localname, built through the SubElement node tree. The wrapping validator compiles per render (not thread-re-entrant on `error_log`).
    sch = lambda local: etree.QName(_SCHEMATRON_NS, local)
    schema = etree.Element(sch("schema"), nsmap={"sch": _SCHEMATRON_NS})
    etree.SubElement(schema, sch("ns"), prefix="ic", uri=_NS_19650)
    rule = etree.SubElement(etree.SubElement(schema, sch("pattern")), sch("rule"), context="ic:informationContainer")
    for local in _REQUIRED_META:
        etree.SubElement(rule, sch("assert"), test=f"ic:{local} != ''").text = f"missing mandated ISO 19650 {local}"
    return etree.tostring(schema)


def _column_widths(containers: tuple["InformationContainer", ...], /) -> tuple[float, ...]:
    # the per-column width from header and widest cell, bounded — `set_column` under `constant_memory`, where post-write `autofit` cannot read the flushed widths.
    rows = (_COLUMNS, *(tuple(str(cell) for cell in c.spreadsheet_row()) for c in containers))
    return tuple(min(_MAX_WIDTH, max(len(row[col]) for row in rows) + _PAD) for col in range(len(_COLUMNS)))


def _running_header(register: Register, /) -> tuple[str, str]:
    # the header/footer field codes xlsxwriter interpolates (`&P`/`&N`/`&D`); dynamic values `&`-escaped so a project `&` stays a literal, not a field code.
    project = register.meta.project.replace("&", "&&")
    return f"&L{project}&RPage &P of &N", f"&L{register.revision.render()}&C&D&R{register.issued}"


def _container_document(register: Register, dialect: ContainerDialect, /) -> "etree._Element":
    # the namespaced container-metadata document — each field a Clark-notation SubElement whose .text carries the value, never an f-string splice.
    qname = lambda local: etree.QName(_NS_19650, local)
    root = etree.Element(qname("informationContainerSet"), nsmap=dict(_NSMAP.items()))
    root.set("dialect", dialect.value)
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
        node = etree.SubElement(containers, qname("informationContainer"))
        node.set("reference", container.reference)
        for local, value in container.metadata_rows():
            etree.SubElement(node, qname(local)).text = value
    return root


def _workbook(register: Register, merge: Option[bytes], /) -> bytes:
    # the native register spreadsheet under constant-memory streaming — conditional bands, validation dropdowns, asset hyperlinks, a page-of-N header, issued-register protection; a present merge round-trips first.
    containers = _merged(register, merge)
    header, footer = _running_header(register)
    sink = BytesIO()
    with xlsxwriter.Workbook(
        sink, {"constant_memory": True, "in_memory": True}
    ) as book:  # the with-exit close packages the zip into sink, deterministic on every exit
        sheet = book.add_worksheet("Register")
        head = book.add_format({"bold": True, "bg_color": "#1F2937", "font_color": "#FFFFFF", "border": 1})
        link = book.get_default_url_format()  # the shared hyperlink style, never a per-cell mint
        published = book.add_format({"bg_color": _STATE_HEX[ContainerState.PUBLISHED]})
        archived = book.add_format({"bg_color": _STATE_HEX[ContainerState.ARCHIVE]})
        for col, width in enumerate(_column_widths(containers)):  # set_column before the streamed rows flush
            sheet.set_column(col, col, width)
        sheet.write_row(0, 0, _COLUMNS, head)
        for index, container in enumerate(containers, start=1):
            row = container.spreadsheet_row()
            if container.asset_key:  # the Reference cell links to the co-identified artifact, the rest streams from column 1
                sheet.write_url(index, _REFERENCE_COL, f"{_ASSET_URI}{container.asset_key}", link, string=container.reference)
                sheet.write_row(index, _REFERENCE_COL + 1, row[_REFERENCE_COL + 1 :])
            else:
                sheet.write_row(index, 0, row)
        last = max(len(containers), 1)
        sheet.conditional_format(
            1,
            _SUITABILITY_COL,
            last,
            _SUITABILITY_COL,
            {"type": "text", "criteria": "begins with", "value": PublishedPrefix.AUTHORIZED.value, "format": published},
        )
        sheet.conditional_format(
            1, _STATE_COL, last, _STATE_COL, {"type": "text", "criteria": "containing", "value": ContainerState.ARCHIVE.value, "format": archived}
        )
        # the owned vocabularies as Excel validation: State hard-stop, Suitability soft (A/B and project codes extend it), Revision the P/C grammar as guidance.
        sheet.data_validation(
            1,
            _STATE_COL,
            last,
            _STATE_COL,
            {
                "validate": "list",
                "source": [s.value for s in ContainerState],
                "error_type": "stop",
                "input_title": "CDE state",
                "input_message": "One of the four ISO 19650 container states.",
            },
        )
        sheet.data_validation(
            1,
            _SUITABILITY_COL,
            last,
            _SUITABILITY_COL,
            {
                "validate": "list",
                "source": [c.value for c in SuitabilityCode],
                "error_type": "information",
                "input_title": "Suitability",
                "input_message": "Standard S-band; published A/B and documented project codes extend it.",
            },
        )
        sheet.data_validation(
            1,
            _REVISION_COL,
            last,
            _REVISION_COL,
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
        })
        if register.issued:  # an issued register is read-only; a draft stays editable under the validation dropdowns
            sheet.protect(options={"autofilter": True, "sort": True, "select_locked_cells": True})
    return sink.getvalue()


def _merged(register: Register, merge: Option[bytes], /) -> tuple[InformationContainer, ...]:
    # the revision-latest merge — each reference kept at its highest revision so a re-issue supersedes; a Nothing merge is the register's own set.
    if merge.is_none():
        return register.containers
    seed: Map[str, InformationContainer] = Map.of_seq((c.reference, c) for c in _ingest(merge.value, register.documented))
    latest = Block.of_seq(register.containers).fold(
        lambda acc, c: acc.change(
            c.reference, lambda held, incoming=c: Some(incoming if held.is_none() or incoming.revision.succeeds(held.value.revision) else held.value)
        ),
        seed,
    )
    return tuple(latest.values())


def _ingest(data: bytes, documented: frozendict[str, ContainerState], /) -> tuple[InformationContainer, ...]:
    # the client-register read — openpyxl values-only rows through the same pydantic gate under the `documented` band; malformed rows dropped.
    with load_workbook(BytesIO(data), read_only=True, data_only=True) as book:  # the read_only handle closes deterministically on with-exit
        rows = tuple(book[book.sheetnames[0]].iter_rows(min_row=2, values_only=True))
    return tuple(Block.of_seq(rows).choose(lambda row: InformationContainer.admitted(documented, **_row_payload(row)).to_option()))


def _row_payload(row: tuple[object, ...], /) -> ContainerPayload:
    # project a _COLUMNS-ordered client row into the ContainerPayload; the naming fields recovered by splitting the reference on _FIELD_SEP.
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
        number=fields[6],
        suitability=padded[_SUITABILITY_COL],
        revision=padded[_COLUMNS.index("Revision")],
        title=padded[1],
        purpose=padded[_COLUMNS.index("Purpose")],
        classification=padded[_COLUMNS.index("Classification")],
    )


def _container_faults(container: InformationContainer, policy: AuditPolicy, /) -> Block[RegisterFault]:
    # one container's coverage faults — a (predicate, RegisterFault) table filtered to the flagged, never a mutable append.
    ref = frozenset({container.reference})
    checks: tuple[tuple[bool, RegisterFault], ...] = (
        (container.suitability.contractual is not (container.revision.kind is RevisionKind.CONTRACTUAL), RegisterFault(misstated=ref)),
        (container.suitability.withdrawn, RegisterFault(withdrawn_issued=ref)),
        (policy.require_classification and not container.classification.code, RegisterFault(unclassified=ref)),
        (policy.require_contractual and not container.suitability.contractual, RegisterFault(misstated=ref)),
    )
    return Block.of_seq(fault for flagged, fault in checks if flagged)


def _sequence_faults(containers: tuple[InformationContainer, ...], policy: AuditPolicy, /) -> Block[RegisterFault]:
    # the set-level integrity faults — a non-monotonic revision on a repeated reference, a Counter-detected duplicate, and a sheet-index gap.
    if not policy.require_sequence:
        return Block.empty()

    def step(acc: tuple[Map[str, RevisionCode], frozenset[str]], container: InformationContainer, /) -> tuple[Map[str, RevisionCode], frozenset[str]]:
        seen, flagged = acc
        prior = seen.try_find(container.reference)
        regressed = prior.is_some() and not container.revision.succeeds(prior.value)
        return seen.add(container.reference, container.revision), (flagged | {container.reference} if regressed else flagged)

    _, regressed = Block.of_seq(containers).fold(step, (Map.empty(), frozenset()))
    duplicated = frozenset(ref for ref, count in Counter(c.reference for c in containers).items() if count > 1)
    ordinals = sorted({ordinal for container in containers if (ordinal := _sheet_ordinal(container)) is not None})
    gaps = frozenset(str(missing) for low, high in pairwise(ordinals) for missing in range(low + 1, high))
    faults: tuple[tuple[bool, RegisterFault], ...] = (
        (bool(regressed), RegisterFault(non_monotonic=regressed)),
        (bool(duplicated), RegisterFault(duplicate=duplicated)),
        (bool(gaps), RegisterFault(gap=gaps)),
    )
    return Block.of_seq(fault for flagged, fault in faults if flagged)


def _sheet_ordinal(container: InformationContainer, /) -> int | None:
    return int(digits) if (digits := "".join(ch for ch in container.number if ch.isdigit())) else None
```

## [03]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)

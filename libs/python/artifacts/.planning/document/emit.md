# [PY_ARTIFACTS_EMIT]

The document-emission and post-processing axis, rebased onto the one `DocumentNode` semantic tree. `DocumentPlan` is ONE frozen owner discriminating document mode over a `frozendict[DocumentMode, Backend]` policy table whose every `Backend` row binds the lowering arm to its runtime `Band`, so the runtime/worker split is a row column rather than a second dispatch surface — every arm is a LOWERING fold FROM the `document/model#NODE` `DocumentNode` tree, never an opaque payload: PDF authoring (reportlab platypus flowables, weasyprint HTML-CSS with the `pdf_variant` PDF/A-PDF/UA archival row plus `base_url`/`output_intent`/`stylesheets`, typst markup typesetting with `pdf_standards` selection over the held font-cached `Compiler` plus `sys.inputs` data-binding and `timestamp` reproducibility), PDF render/raster (pymupdf native `Pixmap.tobytes`, pypdfium2 `to_numpy` — both Pillow-free on the runtime), PDF assembly (pypdf), PDF repair/linearize (pikepdf, worker band), Office (python-docx/python-pptx/openpyxl/xlsxwriter/docxtpl), OpenDocument (odfpy ODT/ODS authoring, the pure-Python OASIS sibling to the OOXML office rows), and structured-text (lxml/ruamel-yaml/tomlkit). Emission lowers FROM the tree and `document/lens#LENS` recovers TO it, so production and extraction are inverses over the one node algebra defined once at `document/model#NODE`. Every per-arm input is one frozen `EmitSpec` `msgspec.Struct` admitted exactly once at `DocumentPlan.of` through the closed `EmitPayload` `TypedDict` and its module-level `TypeAdapter`, the per-mode `_REQUIRED` precondition making admission total over well-formed requests — never a `dict[str, object]` bag, never re-validated in the interior. Every production threads one `EmitFact` typed-evidence carrier onto the frozen owner through `copy.replace`, the `@receipted(_REDACTION)` harvest weave draining `DocumentPlan.contribute` off the stepped owner so the receipt reads `EmitFact` without an in-process re-run; `EmitFault` is the closed `@tagged_union` over the `payload`/`unsatisfied` ADMISSION causes `of` produces, while every arm-level provider raise (`typst.TypstError`, `pypdfium2.PdfiumError`, `lxml.etree.XMLSyntaxError`, `xlsxwriter.XlsxWriterException`) converts to the runtime `BoundaryFault` at the `async_boundary` capsule, exactly as `document/egress#FINISH` routes its provider raises.

## [01]-[INDEX]

- [01]-[DOCUMENT]: document-mode dispatch axis whose `Backend`-per-mode policy rows lower from `DocumentNode` across the one `BACKENDS` table — each row binding its `Band` (`CORE`/`WORKER`) so the gated lxml/pikepdf/python-pptx band is a row column the `_worker_emit` re-resolves rather than a parallel `GATED_ARMS` table — carrying the reportlab flowable author, the weasyprint archival HTML row, the Typst held-`Compiler` compile/data rows and the free-function `query`/`eval` introspection, the pymupdf/pypdfium2 Pillow-free raster rows with the pypdfium2 outline harvest, the pypdf assemble and pikepdf repair rows, the python-docx/python-pptx/docxtpl authoring rows, the xlsxwriter constant-memory streaming policy, the odfpy ODT/ODS OpenDocument authoring rows, and the lxml transform/validate/query rows; `EmitSpec` the one admitted-once typed payload, `EmitFact` the threaded evidence carrier, `EmitFault` the closed admission vocabulary, and the `@receipted` harvest weave over a thin pure `_emit` returning the stepped `Self`.

## [02]-[DOCUMENT]

- Entry: `produced` is the ONE modal-arity entrypoint over `DocumentPlan | Iterable[DocumentPlan]` discriminating on the INPUT SHAPE — a lone plan is `Block.singleton`, an iterable is `Block.of_seq`, normalized once at the head, threaded through the rail with NO `batch`/`mode` knob. `_emitted` constructs ONE held `Compiler` off the batch head OFF the loop through `to_thread.run_sync` under the shared `_OFFLOAD` limiter (the font-discovery scan is blocking native, so the build never stalls the scheduler) yet INSIDE the `async_boundary` capsule (so a `Compiler` construction raise still converts to `BoundaryFault`) when the head mode is a `_HELD_WORLD` Typst row and threads it through every Typst arm so a multi-document Typst render pays font load once; a Typst arm in a non-Typst-headed batch builds its own per-plan world so it never receives a `None` compiler. `_emit` is the thin pure core returning the stepped `Self` (a `ReceiptContributor`) the `@receipted` weave harvests; `_stepped` reads `Backend.band` and NEVER runs a synchronous native render inline on the loop — a `CORE`-band arm crosses `anyio.to_thread.run_sync` (the GIL-releasing native render off the loop) under the shared `_OFFLOAD` `CapacityLimiter`, a `WORKER`-band arm crosses `anyio.to_process.run_sync` onto `_worker_emit` which re-resolves the SAME `BACKENDS` row, so the worker lane carries no second `match` and neither lane stalls the scheduler. `produced` returns a `RuntimeRail[Block[ContentKey]]` keyed over each `EmitFact.data`.
- Auto: every arm receives the `DocumentNode` tree and the held `Compiler` world and lowers the tree — `PDF_AUTHOR` folds it through the `_flowables` platypus story; `PDF_HTML` folds `to_html(node)` through weasyprint `HTML(string, base_url).write_pdf(pdf_variant=, pdf_tags=, pdf_forms=, output_intent=, stylesheets=)`; `PDF_TYPST`/`TYPST_DATA` fold `to_typst_source(node).encode()` through the held `Compiler.compile_with_warnings(input=, sys_inputs=, pdf_standards=, timestamp=)`; `TYPST_QUERY` folds the selector through the free `typst.query(input=source-bytes, selector, field, one)` and `TYPST_EVAL` the source expression through the free `typst.eval(input=source-bytes, expression)`; render folds each page through pymupdf `get_pixmap`→native `tobytes`/`numpy`; raster folds each page through pypdfium2 `render(**render_keywords)`→`to_numpy` into one `numpy` frame stack and harvests the `get_toc(max_depth)`→`PdfBookmark.get_count`/`get_title`/`get_dest`→`PdfDest.get_index` outline as `OutlineRow` triples; assembly runs pypdf `PdfWriter.append`; repair runs pikepdf `Pdf.save`; font-embed registers each `CONFORM`-subsetted face through reportlab `TTFont`+`registerFont` and draws every `RunNode`; `DOCX`/`PPTX` fold the tree through `_docx_block`/the slide loop; `DOCX_TEMPLATE` folds the tree in one `walk`+`match` pass into the role-keyed `render` context reading `get_undeclared_template_variables` onto the fact; XLSX folds the `TableNode` grid through the `SpreadsheetPolicy` row projecting each cell through `CellValue`; `ODT`/`ODS` fold one `_odf_emit` arm flavor-keyed by mode — `ODS` lowers the `TableNode` grid into `Table`/`TableRow`/`TableCell` through `CellValue.odf_cell` typed `(valuetype, value)` cells, `ODT` lowers the block tree through `_odf_block` `H`/`P` paragraphs over `addTextToElement`, each serialized once through `OpenDocument.write`; structured-text folds through ruamel-yaml `YAML().dump`, tomlkit `dumps`, and the four gated lxml rows under the `XMLParser(resolve_entities=False, huge_tree=False, no_network=True)` hardening.
- Receipt: the `@receipted(_REDACTION)` harvest weave stacks over the pure `_emit`, draining `DocumentPlan.contribute` and emitting through `Signals.emit_async`; `contribute` reads the threaded `EmitFact` off `self.fact` (never an in-process re-run of a worker-gated arm), re-mints the content key over `fact.data`, and folds the case off the `Backend.kind` `ReceiptKind` discriminant through the `_RECEIPT` table in one expression — the PDF rows mint `core/receipt#RECEIPT` `ArtifactReceipt.Pdf(key, bytes, pages)`, the Office and structured-text rows mint `ArtifactReceipt.Office(key, bytes)` — the exact `(ContentKey, <scalars>)` arities the receipt owner declares, never a widened tuple and never the typography-rail `Document` case (emit mints only the `Pdf`/`Office` arities). The rich per-arm evidence (render scale, outline count, embedded-face set, `get_undeclared_template_variables` set, queried length, validation verdict, warning/error count) rides the `EmitFact` carrier the consumer reads off `self.fact`; a richer byte-only receipt case is a `core/receipt#RECEIPT` growth concern — the `Document` case is the typography rail's, so emit mints only the `Pdf`/`Office` arities it owns.
- Growth: a new document format is one `DocumentMode` row plus one `Backend` row binding its arm and band, plus a `_REQUIRED` row if it demands an input; a new archival profile is one `PdfVariant` row projecting to both engines; a new typed cell one `CellValue` arm; a new schema dialect one `SchemaKind` row; a new evidence fact one `EmitFact` field; a new admission cause one `EmitFault` case; the held `Compiler` amortizes font load across a batched `produced`; zero new surface.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
import io
import re
from collections.abc import Callable, Iterable, Iterator
from copy import replace
from datetime import date, datetime
from enum import StrEnum
from typing import TYPE_CHECKING, Final, Literal, NotRequired, ReadOnly, Self, TypedDict, Unpack

import msgspec
from anyio import CapacityLimiter, to_process, to_thread
from builtins import frozendict
from expression import Error, Ok, Result, case, tag, tagged_union
from expression.collections import Block, Map
from msgspec import Struct, field, to_builtins
from pydantic import TypeAdapter, ValidationError

from rasm.runtime.content_identity import ContentIdentity, ContentKey
from rasm.runtime.faults import RuntimeRail, async_boundary
from rasm.runtime.receipts import OPEN, Receipt, receipted

from artifacts.core.receipt import ArtifactReceipt
from artifacts.document.model import (
    BlockKind,
    BlockNode,
    DocumentNode,
    FieldNode,
    FigureNode,
    ListKind,
    ListNode,
    PageNode,
    RunNode,
    RunScript,
    SectionNode,
    TableNode,
    TextDirection,
    to_html,
    to_lxml_tree,
    to_typst_source,
    walk,
)

lazy import docx
lazy import numpy as np
lazy import openpyxl
lazy import pikepdf
lazy import pymupdf
lazy import pypdfium2
lazy import tomlkit
lazy import typst
lazy import xlsxwriter
lazy from docx.shared import Pt, RGBColor
lazy from docxtpl import DocxTemplate, InlineImage, Listing, RichText, RichTextParagraph
lazy from lxml import etree
lazy from odf.opendocument import OpenDocumentSpreadsheet, OpenDocumentText
lazy from odf.table import Table as OdfTable, TableCell, TableRow
lazy from odf.teletype import addTextToElement
lazy from odf.text import H, P
lazy from openpyxl.utils import get_column_letter
lazy from pptx import Presentation
lazy from pptx.util import Inches, Pt as SlidePt
lazy from pypdf import PdfReader, PdfWriter
lazy from reportlab.lib.pagesizes import A4
lazy from reportlab.lib.styles import getSampleStyleSheet
lazy from reportlab.pdfbase import pdfmetrics
lazy from reportlab.pdfbase.ttfonts import TTFont
lazy from reportlab.pdfgen.canvas import Canvas
lazy from reportlab.platypus import Image, ListFlowable, ListItem, PageBreak, Paragraph, SimpleDocTemplate, Spacer, Table
lazy from ruamel.yaml import YAML
lazy from typst import Compiler
lazy from weasyprint import CSS, HTML

if TYPE_CHECKING:
    from typst import Compiler

# --- [TYPES] ----------------------------------------------------------------------------


class DocumentMode(StrEnum):
    PDF_AUTHOR = "pdf-author"
    PDF_HTML = "pdf-html"
    PDF_TYPST = "pdf-typst"
    TYPST_QUERY = "typst-query"
    TYPST_EVAL = "typst-eval"
    TYPST_DATA = "typst-data"
    PDF_RENDER = "pdf-render"
    PDF_RASTER = "pdf-raster"
    PDF_ASSEMBLE = "pdf-assemble"
    PDF_REPAIR = "pdf-repair"
    FONT_EMBED = "font-embed"
    DOCX = "docx"
    DOCX_TEMPLATE = "docx-template"
    PPTX = "pptx"
    XLSX = "xlsx"
    ODT = "odt"
    ODS = "ods"
    XML = "xml"
    XML_TRANSFORM = "xml-transform"
    XML_VALIDATE = "xml-validate"
    XML_QUERY = "xml-query"
    YAML = "yaml"
    TOML = "toml"


class Band(StrEnum):
    CORE = "core"
    WORKER = "worker"


class ReceiptKind(StrEnum):
    PDF = "pdf"
    OFFICE = "office"


class SchemaKind(StrEnum):
    XSD = "XMLSchema"
    RELAXNG = "RelaxNG"
    SCHEMATRON = "Schematron"
    DTD = "DTD"


class XlsxRegime(StrEnum):
    IN_MEMORY = "in-memory"
    STREAMED = "streamed"


class PdfVariant(StrEnum):
    NONE = "none"
    A_1B = "a-1b"
    A_2B = "a-2b"
    A_3B = "a-3b"
    A_4 = "a-4"
    UA_1 = "ua-1"

    @property
    def typst(self) -> tuple[str, ...]:
        return _PDF_STANDARD.get(self, ())  # `A_4` (PDF 2.0 archival) is weasyprint-only; typst renders its strongest supported profile

    @property
    def weasyprint(self) -> str | None:
        return _PDF_PROFILE.get(self)

    @property
    def tagged(self) -> bool:
        return self is PdfVariant.UA_1


type Arm = Callable[["DocumentPlan", "Compiler | None"], "EmitFact"]
type SheetWriter = Callable[["DocumentPlan", list[tuple["CellValue", ...]]], "EmitFact"]
type OutlineRow = tuple[int, str, int]  # (sub-item count, title, destination page index)
type CellScalar = str | float | bool | datetime | date

# --- [CONSTANTS] ------------------------------------------------------------------------

# the keep-all redaction the `@receipted` weave rides; `Redaction` is a `Struct(classified, salt)` with
# NO named instances, so `Redaction.STRUCTURAL` was a phantom — the empty `classified` map scrubs nothing.
_REDACTION: Final[Redaction] = Redaction(classified=Map.empty())
_STREAMING_ROW_THRESHOLD: Final = 50_000
_ZIP64_ROW_THRESHOLD: Final = 1_048_576
_OUTLINE_MAX_DEPTH: Final = 15
_RASTER_SCALE: Final = 4.0
_OFFLOAD: Final = CapacityLimiter(8)  # bounds the in-process native-render thread fan-out; a CORE arm never runs inline on the loop
_BOLD_WEIGHT: Final = 700
_PT_PER_HALFPT: Final = 2
_HEADER_WIDTH: Final = 18.0
_NUM_FORMAT: Final = "0.############"
_DATE_FORMAT: Final = "yyyy-mm-dd hh:mm:ss"
_HEADER_RANGE: Final = "_RasmHeader"
_BOOL_CELL: Final[frozendict[str, bool]] = frozendict({"true": True, "false": False, "yes": True, "no": False})
_PDF_STANDARD: Final[frozendict[PdfVariant, tuple[str, ...]]] = frozendict(
    {PdfVariant.A_1B: ("a-1b",), PdfVariant.A_2B: ("a-2b",), PdfVariant.A_3B: ("a-3b",), PdfVariant.UA_1: ("a-3b", "ua-1")}
)
_PDF_PROFILE: Final[frozendict[PdfVariant, str]] = frozendict(
    {PdfVariant.A_1B: "pdf/a-1b", PdfVariant.A_2B: "pdf/a-2b", PdfVariant.A_3B: "pdf/a-3b", PdfVariant.A_4: "pdf/a-4", PdfVariant.UA_1: "pdf/ua-1"}
)
# `add_format({...})` property dicts replace the per-setter `getattr` loop — one row, no stringly setter dispatch.
_FORMATS: Final[frozendict[str, frozendict[str, object]]] = frozendict({
    "header": frozendict({"bold": True, "align": "center"}),
    "number": frozendict({"num_format": _NUM_FORMAT}),
    "datetime": frozendict({"num_format": _DATE_FORMAT}),
})

# --- [ERRORS] ---------------------------------------------------------------------------


@tagged_union(frozen=True)
class EmitFault:
    # the closed ADMISSION vocabulary `of` produces; every arm-level provider raise
    # (`TypstError`/`PdfiumError`/`XMLSyntaxError`/`XlsxWriterException`) converts to the runtime
    # `BoundaryFault` at the `async_boundary` capsule, never into this interior vocabulary.
    tag: Literal["payload", "unsatisfied"] = tag()
    payload: tuple[str, ...] = case()             # the rejected EmitPayload key paths
    unsatisfied: tuple[DocumentMode, str] = case() # a mode whose `_REQUIRED` input field is empty


# --- [MODELS] ---------------------------------------------------------------------------


class CellValue(Struct, frozen=True):
    raw: CellScalar | None

    @staticmethod
    def of(cell: DocumentNode) -> "CellValue":
        if isinstance(cell, FieldNode):
            return CellValue(cell.value)
        return CellValue(_coerce_cell("".join(run.text for run in walk(cell) if isinstance(run, RunNode))))

    def write_xlsxwriter(self, sheet: object, row: int, column: int, formats: frozendict[str, object]) -> None:
        match self.raw:
            case bool() as flag:
                sheet.write_boolean(row, column, flag)
            case int() | float() as number:
                sheet.write_number(row, column, number, formats["number"])
            case datetime() | date() as moment:
                sheet.write_datetime(row, column, moment, formats["datetime"])
            case None | "":
                sheet.write_blank(row, column, None)
            case str() as text:
                sheet.write_string(row, column, text)

    def write_openpyxl(self) -> object:
        return self.raw if self.raw is not None else ""

    def odf_cell(self) -> tuple[str, float | None, str]:
        # (office:value-type, office:value for the numeric row, the `<text:p>` display run) — bool/date keep the
        # readable string form because the odfpy `TableCell` rows only the `valuetype`/`value` pair, not per-type value attrs.
        match self.raw:
            case bool() as flag:
                return ("string", None, "true" if flag else "false")
            case int() | float() as number:
                return ("float", float(number), str(number))
            case datetime() | date() as moment:
                return ("string", None, moment.isoformat())
            case None | "":
                return ("string", None, "")
            case str() as text:
                return ("string", None, text)


class SpreadsheetPolicy(Struct, frozen=True):
    regime: XlsxRegime
    crossover: int
    writer: SheetWriter

    @staticmethod
    def select(plan: "DocumentPlan", rows: int) -> "SpreadsheetPolicy":
        chosen = plan.spec.spreadsheet
        return (
            _SPREADSHEET_POLICIES[chosen]
            if chosen is not None
            else next(policy for policy in _SPREADSHEET_POLICIES.values() if rows < policy.crossover)
        )


class EmitFact(Struct, frozen=True):
    data: bytes
    pages: int = 0
    scale: float = 0.0
    outline_count: int = 0
    faces: tuple[str, ...] = ()
    undeclared: tuple[str, ...] = ()
    template_path: str = ""
    queried: int = 0
    valid: bool = True
    warnings: int = 0
    errors: int = 0
    outline: tuple[OutlineRow, ...] = ()


class EmitSpec(Struct, frozen=True, omit_defaults=True):
    source: bytes = b""
    password: str = ""
    title: str = ""
    author: str = ""
    subject: str = ""
    variant: PdfVariant = PdfVariant.NONE
    forms: bool = False
    output_intent: str = ""                                    # weasyprint PDF/A ICC output-intent profile path
    base_url: str = ""                                         # weasyprint relative-resource resolution root
    stylesheets: tuple[str, ...] = ()                          # weasyprint supplemental CSS paths
    selector: str = ""
    field_name: str = ""
    one: bool = False
    expression: str = ""                                       # typst `eval` source expression evaluated against the document
    sys_inputs: frozendict[str, str] = field(default_factory=frozendict)
    timestamp: int = 0                                         # typst reproducible-byte creation pin (epoch); 0 = unpinned
    image_format: str = "png"                                 # pymupdf native `Pixmap.tobytes` codec
    pages: tuple[int, ...] = ()
    scale: float = _RASTER_SCALE
    rotation: int = 0
    annotations: bool = True
    subset_fonts: frozendict[str, bytes] = field(default_factory=frozendict)
    subfont_index: int = 0
    template: str = ""
    assets: frozendict[str, str] = field(default_factory=frozendict)
    anchors: frozendict[str, str] = field(default_factory=frozendict)
    links: frozendict[str, str] = field(default_factory=frozendict)
    replace: frozendict[str, str] = field(default_factory=frozendict)  # docxtpl pre-render zip-part swaps (name -> path)
    sheet: str = ""
    header: bool = True
    column_width: float = _HEADER_WIDTH
    spreadsheet: XlsxRegime | None = None
    in_memory: bool = True
    stylesheet: bytes = b""
    xslt_params: frozendict[str, str] = field(default_factory=frozendict)
    schema: bytes = b""
    schema_kind: SchemaKind = SchemaKind.XSD
    path: str = ""
    namespaces: frozendict[str, str] = field(default_factory=frozendict)
    pretty: bool = False

    @property
    def render_keywords(self) -> frozendict[str, object]:
        return frozendict({"scale": self.scale, "rotation": self.rotation, "may_draw_forms": self.annotations})


# --- [BOUNDARIES] -----------------------------------------------------------------------


class EmitPayload(TypedDict, closed=True):
    source: NotRequired[ReadOnly[bytes]]
    password: NotRequired[ReadOnly[str]]
    title: NotRequired[ReadOnly[str]]
    author: NotRequired[ReadOnly[str]]
    subject: NotRequired[ReadOnly[str]]
    variant: NotRequired[ReadOnly[PdfVariant]]
    forms: NotRequired[ReadOnly[bool]]
    output_intent: NotRequired[ReadOnly[str]]
    base_url: NotRequired[ReadOnly[str]]
    stylesheets: NotRequired[ReadOnly[tuple[str, ...]]]
    selector: NotRequired[ReadOnly[str]]
    field_name: NotRequired[ReadOnly[str]]
    one: NotRequired[ReadOnly[bool]]
    expression: NotRequired[ReadOnly[str]]
    sys_inputs: NotRequired[ReadOnly[frozendict[str, str]]]
    timestamp: NotRequired[ReadOnly[int]]
    image_format: NotRequired[ReadOnly[str]]
    pages: NotRequired[ReadOnly[tuple[int, ...]]]
    scale: NotRequired[ReadOnly[float]]
    rotation: NotRequired[ReadOnly[int]]
    annotations: NotRequired[ReadOnly[bool]]
    subset_fonts: NotRequired[ReadOnly[frozendict[str, bytes]]]
    subfont_index: NotRequired[ReadOnly[int]]
    template: NotRequired[ReadOnly[str]]
    assets: NotRequired[ReadOnly[frozendict[str, str]]]
    anchors: NotRequired[ReadOnly[frozendict[str, str]]]
    links: NotRequired[ReadOnly[frozendict[str, str]]]
    replace: NotRequired[ReadOnly[frozendict[str, str]]]
    sheet: NotRequired[ReadOnly[str]]
    header: NotRequired[ReadOnly[bool]]
    column_width: NotRequired[ReadOnly[float]]
    spreadsheet: NotRequired[ReadOnly[XlsxRegime]]
    in_memory: NotRequired[ReadOnly[bool]]
    stylesheet: NotRequired[ReadOnly[bytes]]
    xslt_params: NotRequired[ReadOnly[frozendict[str, str]]]
    schema: NotRequired[ReadOnly[bytes]]
    schema_kind: NotRequired[ReadOnly[SchemaKind]]
    path: NotRequired[ReadOnly[str]]
    namespaces: NotRequired[ReadOnly[frozendict[str, str]]]
    pretty: NotRequired[ReadOnly[bool]]


_PAYLOAD: Final = TypeAdapter(EmitPayload)
# the per-mode precondition: a mode's named `EmitSpec` fields must be non-empty so the interior is total.
_REQUIRED: Final[frozendict[DocumentMode, tuple[str, ...]]] = frozendict({
    DocumentMode.PDF_RENDER: ("source",),
    DocumentMode.PDF_RASTER: ("source",),
    DocumentMode.PDF_ASSEMBLE: ("source",),
    DocumentMode.PDF_REPAIR: ("source",),
    DocumentMode.TYPST_QUERY: ("selector",),
    DocumentMode.TYPST_EVAL: ("expression",),
    DocumentMode.FONT_EMBED: ("subset_fonts",),
    DocumentMode.DOCX_TEMPLATE: ("template",),
    DocumentMode.XML_TRANSFORM: ("stylesheet",),
    DocumentMode.XML_VALIDATE: ("schema",),
    DocumentMode.XML_QUERY: ("path",),
})


# --- [SERVICES] -------------------------------------------------------------------------


class Backend(Struct, frozen=True):
    band: Band
    arm: Arm
    kind: ReceiptKind


class DocumentPlan(Struct, frozen=True):
    mode: DocumentMode
    node: DocumentNode
    spec: EmitSpec = field(default_factory=EmitSpec)
    fact: EmitFact | None = None

    def world(self) -> "Compiler":
        # the held world is seeded with the head document bytes and amortizes font load; each Typst arm
        # overrides `input=` per compile, so the cached fonts serve every batched render.
        return Compiler(to_typst_source(self.node).encode(), font_paths=[], sys_inputs=dict(self.spec.sys_inputs))

    @receipted(_REDACTION)
    async def _emit(self, world: "Compiler | None", /) -> Self:
        # returns the stepped owner (a `ReceiptContributor`) the harvest weave drains; the content key
        # is minted by `_emitted` off `self.fact.data`, never inside the pure core.
        return await self._stepped(world)

    async def _stepped(self, world: "Compiler | None", /) -> Self:
        # the synchronous native render NEVER runs inline on the loop: a WORKER arm crosses the GIL-hostile
        # process seam, a CORE arm the GIL-releasing thread seam, each bounded by an explicit limiter.
        backend = BACKENDS[self.mode]
        fact = (
            await to_process.run_sync(_worker_emit, self, limiter=_OFFLOAD)
            if backend.band is Band.WORKER
            else await to_thread.run_sync(backend.arm, self, world, limiter=_OFFLOAD)
        )
        return replace(self, fact=fact)

    def contribute(self) -> Iterable[Receipt]:
        # the canonical `ReceiptContributor.contribute(self)` port — phase is the constant `"emitted"`
        # the `ArtifactReceipt` fixes by construction (KNOB_TEST); rides the stepped owner, never a worker re-run.
        if (fact := self.fact) is None:
            return
        key = ContentIdentity.of(self.mode.value, fact.data)
        yield from _RECEIPT[BACKENDS[self.mode].kind](key, fact).contribute()

    @classmethod
    def of(cls, mode: DocumentMode, node: DocumentNode, /, **raw: Unpack[EmitPayload]) -> Result[Self, EmitFault]:
        try:
            payload = _PAYLOAD.validate_python(raw)
        except ValidationError as fault:
            return Error(EmitFault(payload=tuple(str(error["loc"]) for error in fault.errors())))
        spec = EmitSpec(**payload)
        missing = next((name for name in _REQUIRED.get(mode, ()) if not getattr(spec, name)), None)
        return Error(EmitFault(unsatisfied=(mode, missing))) if missing else Ok(cls(mode=mode, node=node, spec=spec))


# --- [OPERATIONS] -----------------------------------------------------------------------


async def produced(plans: "DocumentPlan | Iterable[DocumentPlan]", /) -> RuntimeRail[Block[ContentKey]]:
    block = Block.singleton(plans) if isinstance(plans, DocumentPlan) else Block.of_seq(plans)
    return await async_boundary("document.emit", lambda: _emitted(block))


async def _emitted(block: "Block[DocumentPlan]", /) -> Block[ContentKey]:
    # the held Typst world is built off the loop (a `Compiler` font-discovery scan is blocking native) under the shared
    # limiter, INSIDE the capsule so the construction never stalls the scheduler yet a build raise still converts to `BoundaryFault`
    held = (
        await to_thread.run_sync(block.head().world, limiter=_OFFLOAD)
        if not block.is_empty() and block.head().mode in _HELD_WORLD
        else None
    )
    stepped = [await plan._emit(held if plan.mode in _HELD_WORLD else None) for plan in block]
    return Block.of_seq([ContentIdentity.of(plan.mode.value, plan.fact.data) for plan in stepped])


# one compiled classifier names the cell type in a single total pass — no per-cell speculative `try` ladder
# decides the shape; the `int`/`float` arms are then regex-guaranteed, the temporal pair the lone fallible parse.
_CELL_SHAPE: Final[re.Pattern[str]] = re.compile(
    r"\A(?:(?P<int>[-+]?\d+)"
    r"|(?P<float>[-+]?(?:\d+\.\d*|\.\d+|\d+)(?:[eE][-+]?\d+)?)"
    r"|(?P<dt>\d{4}-\d{2}-\d{2}[T ]\d{2}:\d{2}(?::\d{2}(?:\.\d+)?)?)"
    r"|(?P<date>\d{4}-\d{2}-\d{2}))\Z"
)


def _temporal(parse: Callable[[str], CellScalar], text: str, /) -> CellScalar:
    try:  # the one boundary catch: a shape-valid but range-invalid stamp (e.g. month 13) falls back to raw text
        return parse(text)
    except ValueError:
        return text


# the matched group names the total cell parser; `int`/`float` are regex-guaranteed, the temporal pair guarded once.
_CELL_TYPED: Final[frozendict[str, Callable[[str], CellScalar]]] = frozendict({
    "int": int,
    "float": float,
    "dt": lambda text: _temporal(datetime.fromisoformat, text),
    "date": lambda text: _temporal(date.fromisoformat, text),
})


def _coerce_cell(text: str) -> CellScalar | None:
    stripped = text.strip()
    if not stripped:
        return None
    if (folded := stripped.casefold()) in _BOOL_CELL:
        return _BOOL_CELL[folded]
    return _CELL_TYPED[matched.lastgroup](stripped) if (matched := _CELL_SHAPE.match(stripped)) is not None else text


def _flowables(node: DocumentNode, styles: object, spec: EmitSpec, /) -> Iterator[object]:
    match node:
        case RunNode():
            yield Paragraph(node.text, styles["Normal"])
        case BlockNode(block=BlockKind.HEADING, level=level, runs=runs):
            yield Paragraph("".join(run.text for run in runs), styles[f"Heading{min(max(level, 1), 6)}"])
        case BlockNode(block=BlockKind.CODE, runs=runs):
            yield Paragraph("".join(run.text for run in runs), styles["Code"])
        case BlockNode(runs=runs, children=kids):
            yield Paragraph("".join(run.text for run in runs), styles["Normal"])
            for kid in kids:
                yield from _flowables(kid, styles, spec)
        case ListNode(list_kind=kind, items=items):
            entries = [ListItem(Paragraph(_text(item), styles["Normal"])) for item in items]
            yield ListFlowable(entries, bulletType="1" if kind is ListKind.ORDERED else "bullet")
        case TableNode(rows=rows):
            yield Table([[_text(cell) for cell in row] for row in rows])
        case FigureNode(asset_key=asset_key) if asset_key.hex in spec.assets:
            yield Image(spec.assets[asset_key.hex])
        case SectionNode(level=level, heading=head, children=kids):
            yield Paragraph("".join(run.text for run in head), styles[f"Heading{min(max(level, 1), 6)}"])
            for kid in kids:
                yield from _flowables(kid, styles, spec)
        case PageNode(children=kids):
            for kid in kids:
                yield from _flowables(kid, styles, spec)
            yield PageBreak()
        case _:
            yield Spacer(1, 0)


def _text(node: DocumentNode) -> str:
    return "".join(run.text for run in walk(node) if isinstance(run, RunNode))


def _reportlab_author(plan: DocumentPlan, _world: "Compiler | None", /) -> EmitFact:
    sink = io.BytesIO()
    doc = SimpleDocTemplate(sink, pagesize=A4, title=plan.spec.title or plan.node.meta.key.hex, author=plan.spec.author, subject=plan.spec.subject)
    doc.build(list(_flowables(plan.node, getSampleStyleSheet(), plan.spec)))
    return EmitFact(data=sink.getvalue(), pages=sum(1 for n in walk(plan.node) if isinstance(n, PageNode)) or 1)


def _weasyprint_html(plan: DocumentPlan, _world: "Compiler | None", /) -> EmitFact:
    data = HTML(string=to_html(plan.node), base_url=plan.spec.base_url or None).write_pdf(
        target=None,
        pdf_variant=plan.spec.variant.weasyprint,
        pdf_tags=plan.spec.variant.tagged,
        pdf_forms=plan.spec.forms,
        output_intent=plan.spec.output_intent or None,
        stylesheets=[CSS(filename=sheet) for sheet in plan.spec.stylesheets] or None,
    )
    return EmitFact(data=data)


def _typst_compile(plan: DocumentPlan, world: "Compiler | None", /) -> EmitFact:
    compiler = world if world is not None else plan.world()  # held world amortizes fonts; a non-Typst-headed batch falls back per-plan
    source = to_typst_source(plan.node)
    if plan.spec.variant is PdfVariant.UA_1:  # PDF/UA hard-errors `missing document title` without it
        title = (plan.spec.title or plan.node.meta.key.hex).replace("\\", "\\\\").replace('"', '\\"')
        source = f'#set document(title: "{title}")\n{source}'
    data, warnings = compiler.compile_with_warnings(
        input=source.encode(), output=None, sys_inputs=dict(plan.spec.sys_inputs),
        pdf_standards=plan.spec.variant.typst, timestamp=plan.spec.timestamp or None,
    )
    return EmitFact(data=data, warnings=len(warnings))


def _typst_query(plan: DocumentPlan, _world: "Compiler | None", /) -> EmitFact:
    # the free function compiles a single-shot world over THIS plan's own document — never the held
    # head world, which would query the wrong document in a batch.
    result = typst.query(
        input=to_typst_source(plan.node).encode(), selector=plan.spec.selector,
        field=plan.spec.field_name or None, one=plan.spec.one,
    ).encode()
    return EmitFact(data=result, queried=len(result))


def _typst_eval(plan: DocumentPlan, _world: "Compiler | None", /) -> EmitFact:
    # the expression-evaluation inverse of `_typst_query`: a single-shot world over THIS plan's own document.
    result = typst.eval(input=to_typst_source(plan.node).encode(), expression=plan.spec.expression).encode()
    return EmitFact(data=result, queried=len(result))


def _pymupdf_render(plan: DocumentPlan, _world: "Compiler | None", /) -> EmitFact:
    doc = pymupdf.open(stream=plan.spec.source, filetype="pdf")
    try:
        pixmaps = [doc[index].get_pixmap(matrix=pymupdf.Matrix(plan.spec.scale, plan.spec.scale)) for index in plan.spec.pages or range(doc.page_count)]
        if len(pixmaps) == 1:
            data = pixmaps[0].tobytes(output=plan.spec.image_format)  # native MuPDF encode, no Pillow
        else:
            frames = [np.frombuffer(pix.samples, dtype=np.uint8).reshape(pix.height, pix.width, pix.n) for pix in pixmaps]
            data = np.ascontiguousarray(np.stack(frames)).tobytes()
        return EmitFact(data=data, pages=doc.page_count, scale=plan.spec.scale)
    finally:
        doc.close()


def _pypdfium2_raster(plan: DocumentPlan, _world: "Compiler | None", /) -> EmitFact:
    pdf = pypdfium2.PdfDocument(plan.spec.source, password=plan.spec.password or None, autoclose=True)
    try:
        frames = [pdf.get_page(index).render(**plan.spec.render_keywords).to_numpy() for index in plan.spec.pages or range(len(pdf))]
        data = np.ascontiguousarray(np.stack(frames)).tobytes()  # Pillow-free; multi-frame container egress is `graphic/raster/io#IO`
        outline = tuple(_outline_rows(pdf, _OUTLINE_MAX_DEPTH))
        return EmitFact(data=data, pages=len(pdf), scale=plan.spec.scale, outline_count=len(outline), outline=outline)
    finally:
        pdf.close()


def _outline_rows(pdf: object, max_depth: int) -> Iterator[OutlineRow]:
    for mark in pdf.get_toc(max_depth=max_depth):
        dest = mark.get_dest()
        yield (mark.get_count(), mark.get_title(), dest.get_index() if dest else -1)


def _pypdf_assemble(plan: DocumentPlan, _world: "Compiler | None", /) -> EmitFact:
    writer = PdfWriter()
    writer.append(PdfReader(io.BytesIO(plan.spec.source)))
    for page in writer.pages:
        page.compress_content_streams()
    writer.add_metadata({"/Title": plan.spec.title or plan.node.meta.key.hex, "/Author": plan.spec.author})
    sink = io.BytesIO()
    writer.write(sink)
    return EmitFact(data=sink.getvalue(), pages=len(writer.pages))


def _pikepdf_repair(plan: DocumentPlan, _world: "Compiler | None", /) -> EmitFact:
    sink = io.BytesIO()
    with pikepdf.open(io.BytesIO(plan.spec.source), password=plan.spec.password) as pdf:  # deterministic close, never GC-reaped
        pdf.save(sink, linearize=True, recompress_flate=True, object_stream_mode=pikepdf.ObjectStreamMode.generate, deterministic_id=True)
        return EmitFact(data=sink.getvalue(), pages=len(pdf.pages))


def _font_embed(plan: DocumentPlan, _world: "Compiler | None", /) -> EmitFact:
    for face, blob in plan.spec.subset_fonts.items():
        pdfmetrics.registerFont(TTFont(face, io.BytesIO(blob), subfontIndex=plan.spec.subfont_index))
    canvas = Canvas(io.BytesIO())
    canvas.setTitle(plan.spec.title or plan.node.meta.key.hex)
    canvas.setAuthor(plan.spec.author)
    canvas.setSubject(plan.spec.subject)
    for page in [page for page in walk(plan.node) if isinstance(page, PageNode)] or [plan.node]:
        for run in (leaf for leaf in walk(page) if isinstance(leaf, RunNode)):
            origin = run.meta.bounds or (0.0, 0.0, 0.0, 0.0)
            canvas.setFont(run.font_key, run.size)
            canvas.setFillColorRGB(run.color[0] / 255, run.color[1] / 255, run.color[2] / 255)
            canvas.drawString(origin[0], origin[1], run.text)
        canvas.showPage()
    canvas.save()
    return EmitFact(data=canvas.getpdfdata(), faces=tuple(plan.spec.subset_fonts))


def _docx_run(run_obj: object, run: RunNode, /) -> None:
    run_obj.bold = run.weight >= _BOLD_WEIGHT
    run_obj.italic = run.italic
    run_obj.font.size = Pt(run.size)
    run_obj.font.name = run.font_key
    run_obj.font.rtl = run.direction is TextDirection.RTL
    if run.color != (0, 0, 0):
        run_obj.font.color.rgb = RGBColor(*run.color)


def _docx_block(document: object, node: DocumentNode, spec: EmitSpec, /) -> None:
    match node:
        case BlockNode(block=BlockKind.HEADING, level=level, runs=runs):
            document.add_heading("".join(run.text for run in runs), level=min(max(level, 1), 9))
        case BlockNode(runs=runs, children=kids, block=block):
            paragraph = document.add_paragraph(style="Quote" if block is BlockKind.QUOTE else None)
            for run in runs:
                _docx_run(paragraph.add_run(run.text), run)
            for kid in kids:
                _docx_block(document, kid, spec)
        case ListNode(list_kind=kind, items=items):
            style = "List Number" if kind is ListKind.ORDERED else "List Bullet"
            for item in items:
                document.add_paragraph(_text(item), style=style)
        case TableNode(rows=rows) if rows:
            table = document.add_table(rows=len(rows), cols=len(rows[0]))
            for ri, row in enumerate(rows):
                for ci, cell in enumerate(row):
                    table.cell(ri, ci).text = _text(cell)
        case FigureNode(asset_key=asset_key, meta=meta) if asset_key.hex in spec.assets:
            bounds = meta.bounds or (0.0, 0.0, 0.0, 0.0)
            document.add_picture(spec.assets[asset_key.hex], width=Pt(bounds[2]) if bounds[2] else None)
        case SectionNode(level=level, heading=head, children=kids):
            document.add_heading("".join(run.text for run in head), level=min(max(level, 1), 9))
            for kid in kids:
                _docx_block(document, kid, spec)
        case PageNode(children=kids):
            for kid in kids:
                _docx_block(document, kid, spec)
            document.add_page_break()
        case _:
            pass


def _docx_emit(plan: DocumentPlan, _world: "Compiler | None", /) -> EmitFact:
    document = docx.Document()
    props = document.core_properties
    props.title, props.author, props.subject = plan.spec.title or plan.node.meta.key.hex, plan.spec.author, plan.spec.subject
    _docx_block(document, plan.node, plan.spec)
    sink = io.BytesIO()
    document.save(sink)
    return EmitFact(data=sink.getvalue(), pages=len(document.sections))


def _docx_run_props(template: object, run: RunNode, spec: EmitSpec) -> dict[str, object]:
    url = spec.links.get(run.meta.role)
    return {
        "bold": run.weight >= _BOLD_WEIGHT,
        "italic": run.italic,
        "superscript": run.script is RunScript.SUPER,
        "subscript": run.script is RunScript.SUB,
        "size": int(run.size * _PT_PER_HALFPT),
        "font": run.font_key,
        "color": "%02X%02X%02X" % run.color if run.color != (0, 0, 0) else None,
        "rtl": run.direction is TextDirection.RTL,
        "lang": run.meta.lang if isinstance(run.meta.lang, str) else None,
        "url_id": template.build_url_id(url) if url else None,
    }


def _docxtpl_emit(plan: DocumentPlan, _world: "Compiler | None", /) -> EmitFact:
    template = DocxTemplate(plan.spec.template)
    for name, dst in plan.spec.replace.items():
        template.replace_zipname(name, dst)
    context: dict[str, object] = {}
    for child in walk(plan.node):
        role = child.meta.role
        match child:
            case RunNode():
                context.setdefault(role, RichText()).add(child.text, **_docx_run_props(template, child, plan.spec))
            case BlockNode(block=BlockKind.CODE, runs=runs):
                context[role] = Listing("".join(run.text for run in runs))
            case BlockNode(runs=runs, block=block):
                paragraph = context.setdefault(role, RichTextParagraph())
                for run in runs:
                    paragraph.add(RichText(run.text, **_docx_run_props(template, run, plan.spec)), parastyle=block.value)
            case FigureNode(asset_key=asset_key, meta=meta):
                bounds = meta.bounds or (0.0, 0.0, 0.0, 0.0)
                context[role] = InlineImage(
                    template, plan.spec.assets[asset_key.hex], width=Pt(bounds[2]) if bounds[2] else None, height=Pt(bounds[3]) if bounds[3] else None, anchor=plan.spec.anchors.get(role)
                )
            case SectionNode():
                context[role] = template.new_subdoc()
    undeclared = tuple(template.get_undeclared_template_variables())
    template.render(context, autoescape=True)
    template.render_footnotes(context)  # footnote-part XML, after `render` per the docxtpl contract
    sink = io.BytesIO()
    template.save(sink)
    return EmitFact(data=sink.getvalue(), undeclared=undeclared, template_path=plan.spec.template)


def _pptx_emit(plan: DocumentPlan, _world: "Compiler | None", /) -> EmitFact:
    presentation = Presentation()
    presentation.core_properties.title = plan.spec.title or plan.node.meta.key.hex
    presentation.core_properties.author = plan.spec.author
    blank = presentation.slide_layouts[6]
    for page in [n for n in walk(plan.node) if isinstance(n, PageNode)] or [plan.node]:
        slide = presentation.slides.add_slide(blank)
        frame = slide.shapes.add_textbox(Inches(0.5), Inches(0.5), presentation.slide_width - Inches(1), presentation.slide_height - Inches(1)).text_frame
        frame.word_wrap = True
        for block in (n for n in walk(page) if isinstance(n, BlockNode)):
            paragraph = frame.add_paragraph()
            for run in block.runs:
                run_obj = paragraph.add_run()
                run_obj.text = run.text
                run_obj.font.bold, run_obj.font.italic, run_obj.font.size = run.weight >= _BOLD_WEIGHT, run.italic, SlidePt(run.size)
        for figure in (n for n in walk(page) if isinstance(n, FigureNode) and n.asset_key.hex in plan.spec.assets):
            slide.shapes.add_picture(plan.spec.assets[figure.asset_key.hex], Inches(0.5), Inches(0.5))
    sink = io.BytesIO()
    presentation.save(sink)
    return EmitFact(data=sink.getvalue(), pages=len(presentation.slides))


def _odf_block(body: object, node: DocumentNode, /) -> None:
    match node:
        case RunNode():
            body.addElement(_odf_text(P(), node.text))
        case BlockNode(block=BlockKind.HEADING, level=level, runs=runs):
            body.addElement(_odf_text(H(outlinelevel=min(max(level, 1), 10)), "".join(run.text for run in runs)))
        case BlockNode(runs=runs, children=kids):
            body.addElement(_odf_text(P(), "".join(run.text for run in runs)))
            for kid in kids:
                _odf_block(body, kid)
        case ListNode(items=items):
            for item in items:
                body.addElement(_odf_text(P(), _text(item)))
        case SectionNode(level=level, heading=head, children=kids):
            body.addElement(_odf_text(H(outlinelevel=min(max(level, 1), 10)), "".join(run.text for run in head)))
            for kid in kids:
                _odf_block(body, kid)
        case PageNode(children=kids):
            for kid in kids:
                _odf_block(body, kid)
        case _:
            pass


def _odf_text[E](element: E, text: str, /) -> E:
    addTextToElement(element, text)  # owns the whitespace-correct `<text:s>`/`tab`/`line-break` split
    return element


def _odf_emit(plan: DocumentPlan, _world: "Compiler | None", /) -> EmitFact:
    if plan.mode is DocumentMode.ODS:
        document = OpenDocumentSpreadsheet()
        sheet = OdfTable(name=plan.spec.sheet or "Sheet1")
        for table in (n for n in walk(plan.node) if isinstance(n, TableNode)):
            for row in table.rows:
                line = TableRow()
                for cell in row:
                    kind, value, text = CellValue.of(cell).odf_cell()
                    typed = TableCell(valuetype=kind, value=value) if value is not None else TableCell(valuetype=kind)
                    typed.addElement(_odf_text(P(), text))
                    line.addElement(typed)
                sheet.addElement(line)
        document.spreadsheet.addElement(sheet)
    else:
        document = OpenDocumentText()
        _odf_block(document.text, plan.node)
    sink = io.BytesIO()
    document.write(sink)  # whole ODF zip (content/styles/meta/manifest) to the binary stream, never a temp file
    return EmitFact(data=sink.getvalue())


def _xlsx_emit(plan: DocumentPlan, _world: "Compiler | None", /) -> EmitFact:
    grid = [tuple(CellValue.of(cell) for cell in row) for table in walk(plan.node) if isinstance(table, TableNode) for row in table.rows]
    return SpreadsheetPolicy.select(plan, len(grid)).writer(plan, grid)


def _xlsx_streamed(plan: DocumentPlan, grid: list[tuple[CellValue, ...]]) -> EmitFact:
    sink = io.BytesIO()
    book = xlsxwriter.Workbook(sink, {
        "constant_memory": True, "in_memory": plan.spec.in_memory, "use_zip64": len(grid) >= _ZIP64_ROW_THRESHOLD,
        "remove_timezone": True, "nan_inf_to_errors": True,  # `_coerce_cell` admits tz-aware datetimes and `float('inf')`
    })
    book.set_properties({"title": plan.spec.title or plan.node.meta.key.hex, "author": plan.spec.author})
    sheet = book.add_worksheet(plan.spec.sheet or None)
    header, headed = book.add_format(dict(_FORMATS["header"])), plan.spec.header
    formats = frozendict({kind: book.add_format(dict(_FORMATS[kind])) for kind in ("number", "datetime")})
    width = max((len(row) for row in grid), default=0)
    if width:
        sheet.set_column(0, width - 1, plan.spec.column_width)
    for index, row in enumerate(grid):
        sheet.set_row(index, None, header if index == 0 and headed else None)  # row format before its cells under constant_memory
        for column, value in enumerate(row):
            value.write_xlsxwriter(sheet, index, column, formats)
    if headed and grid:
        sheet.freeze_panes(1, 0)
        book.define_name(_HEADER_RANGE, f"='{sheet.name}'!$A$1:${get_column_letter(width)}$1")
    book.close()
    return EmitFact(data=sink.getvalue(), pages=len(grid))


def _xlsx_in_memory(plan: DocumentPlan, grid: list[tuple[CellValue, ...]]) -> EmitFact:
    book = openpyxl.Workbook(write_only=True, iso_dates=True)
    sheet = book.create_sheet(plan.spec.sheet or None)
    for row in grid:
        sheet.append([value.write_openpyxl() for value in row])
    if plan.spec.header and grid:
        sheet.freeze_panes = "A2"
    sink = io.BytesIO()
    book.save(sink)
    return EmitFact(data=sink.getvalue(), pages=len(grid))


def _ruamel_emit(plan: DocumentPlan, _world: "Compiler | None", /) -> EmitFact:
    engine, sink = YAML(), io.BytesIO()
    engine.dump(to_builtins(plan.node), sink)
    return EmitFact(data=sink.getvalue())


def _tomlkit_emit(plan: DocumentPlan, _world: "Compiler | None", /) -> EmitFact:
    return EmitFact(data=tomlkit.dumps(to_builtins(plan.node)).encode())


def _hardened_parse(source: bytes) -> object:
    return etree.parse(io.BytesIO(source), etree.XMLParser(resolve_entities=False, huge_tree=False, no_network=True))


def _lxml_emit(plan: DocumentPlan, _world: "Compiler | None", /) -> EmitFact:
    tree = to_lxml_tree(plan.node)
    etree.cleanup_namespaces(tree)
    return EmitFact(data=etree.tostring(tree, xml_declaration=True, encoding="utf-8", pretty_print=plan.spec.pretty))


def _lxml_transform(plan: DocumentPlan, _world: "Compiler | None", /) -> EmitFact:
    transform = etree.XSLT(_hardened_parse(plan.spec.stylesheet))
    quoted = {key: etree.XSLT.strparam(value) for key, value in plan.spec.xslt_params.items()}
    return EmitFact(data=bytes(transform(to_lxml_tree(plan.node), **quoted)))


def _lxml_validate(plan: DocumentPlan, _world: "Compiler | None", /) -> EmitFact:
    validator = getattr(etree, plan.spec.schema_kind.value)(_hardened_parse(plan.spec.schema))
    tree = to_lxml_tree(plan.node)
    valid = validator.validate(tree)  # the boolean verdict + `error_log` count, never `assertValid` raising the verdict away
    return EmitFact(data=etree.tostring(tree, xml_declaration=True, encoding="utf-8"), valid=valid, errors=len(validator.error_log))


def _lxml_query(plan: DocumentPlan, _world: "Compiler | None", /) -> EmitFact:
    query = etree.XPath(plan.spec.path, namespaces=dict(plan.spec.namespaces), smart_strings=False)
    result = query(to_lxml_tree(plan.node))
    projected = [etree.tostring(hit).decode() if isinstance(hit, etree._Element) else hit for hit in result] if isinstance(result, list) else result
    data = msgspec.msgpack.encode(projected)  # the XPath projection is arbitrary leaf data, not a `DocumentNode`; one-shot codec
    return EmitFact(data=data, queried=len(data))


def _worker_emit(plan: DocumentPlan) -> EmitFact:
    return BACKENDS[plan.mode].arm(plan, None)


# --- [COMPOSITION] ----------------------------------------------------------------------

_SPREADSHEET_POLICIES: Final[frozendict[XlsxRegime, SpreadsheetPolicy]] = frozendict({
    XlsxRegime.IN_MEMORY: SpreadsheetPolicy(XlsxRegime.IN_MEMORY, _STREAMING_ROW_THRESHOLD, _xlsx_in_memory),
    XlsxRegime.STREAMED: SpreadsheetPolicy(XlsxRegime.STREAMED, 1 << 62, _xlsx_streamed),
})

_HELD_WORLD: Final[frozenset[DocumentMode]] = frozenset({DocumentMode.PDF_TYPST, DocumentMode.TYPST_DATA})

_RECEIPT: Final[frozendict[ReceiptKind, Callable[[ContentKey, EmitFact], "ArtifactReceipt"]]] = frozendict({
    ReceiptKind.PDF: lambda key, fact: ArtifactReceipt.Pdf(key, len(fact.data), fact.pages),
    ReceiptKind.OFFICE: lambda key, fact: ArtifactReceipt.Office(key, len(fact.data)),
})

BACKENDS: Final[frozendict[DocumentMode, Backend]] = frozendict({
    DocumentMode.PDF_AUTHOR: Backend(Band.CORE, _reportlab_author, ReceiptKind.PDF),
    DocumentMode.PDF_HTML: Backend(Band.CORE, _weasyprint_html, ReceiptKind.PDF),
    DocumentMode.PDF_TYPST: Backend(Band.CORE, _typst_compile, ReceiptKind.PDF),
    DocumentMode.TYPST_QUERY: Backend(Band.CORE, _typst_query, ReceiptKind.OFFICE),
    DocumentMode.TYPST_EVAL: Backend(Band.CORE, _typst_eval, ReceiptKind.OFFICE),
    DocumentMode.TYPST_DATA: Backend(Band.CORE, _typst_compile, ReceiptKind.PDF),
    DocumentMode.PDF_RENDER: Backend(Band.CORE, _pymupdf_render, ReceiptKind.PDF),
    DocumentMode.PDF_RASTER: Backend(Band.CORE, _pypdfium2_raster, ReceiptKind.PDF),
    DocumentMode.PDF_ASSEMBLE: Backend(Band.CORE, _pypdf_assemble, ReceiptKind.PDF),
    DocumentMode.PDF_REPAIR: Backend(Band.WORKER, _pikepdf_repair, ReceiptKind.PDF),
    DocumentMode.FONT_EMBED: Backend(Band.CORE, _font_embed, ReceiptKind.PDF),
    DocumentMode.DOCX: Backend(Band.CORE, _docx_emit, ReceiptKind.OFFICE),
    DocumentMode.DOCX_TEMPLATE: Backend(Band.CORE, _docxtpl_emit, ReceiptKind.OFFICE),
    DocumentMode.PPTX: Backend(Band.WORKER, _pptx_emit, ReceiptKind.OFFICE),  # python-pptx -> lxml
    DocumentMode.XLSX: Backend(Band.CORE, _xlsx_emit, ReceiptKind.OFFICE),
    DocumentMode.ODT: Backend(Band.CORE, _odf_emit, ReceiptKind.OFFICE),  # odfpy is pure-Python (defusedxml), no lxml -> CORE
    DocumentMode.ODS: Backend(Band.CORE, _odf_emit, ReceiptKind.OFFICE),
    DocumentMode.XML: Backend(Band.WORKER, _lxml_emit, ReceiptKind.OFFICE),
    DocumentMode.XML_TRANSFORM: Backend(Band.WORKER, _lxml_transform, ReceiptKind.OFFICE),
    DocumentMode.XML_VALIDATE: Backend(Band.WORKER, _lxml_validate, ReceiptKind.OFFICE),
    DocumentMode.XML_QUERY: Backend(Band.WORKER, _lxml_query, ReceiptKind.OFFICE),
    DocumentMode.YAML: Backend(Band.CORE, _ruamel_emit, ReceiptKind.OFFICE),
    DocumentMode.TOML: Backend(Band.CORE, _tomlkit_emit, ReceiptKind.OFFICE),
})
```

## [03]-[RESEARCH]

- [PHANTOM_ARMS_REALIZED]: the prior fence bound `_reportlab_author`/`_pikepdf_repair`/`_docx_emit`/`_pptx_emit` in `BACKENDS` but DEFINED NONE of them — four modes were a `NameError` at module load, a stub dressed as a complete table. The rebuild realizes each as a real lowering arm: `_reportlab_author` folds the tree through `_flowables` into a `platypus` `SimpleDocTemplate.build` story (`Paragraph`/`Table`/`Image`/`ListFlowable`/`PageBreak` per node kind, the `getSampleStyleSheet` heading/normal/code styles), `_docx_emit` folds it through `_docx_block` (`add_heading`/`add_paragraph`+`add_run`/`add_table`+`cell`/`add_picture`/`add_page_break`) with the full `_docx_run` font projection, `_pptx_emit` folds each `PageNode` into a blank-layout slide's textbox `TextFrame` plus `add_picture` figures, and `_pikepdf_repair` opens and re-saves through `Pdf.save(linearize=True, recompress_flate=True, object_stream_mode=generate, deterministic_id=True)`. The `reportlab.platypus`/`lib.styles`/`lib.pagesizes`, `docx.Document`/`add_*`/`Run.font`/`shared.Pt`/`RGBColor`, `pptx.Presentation`/`slide_layouts`/`slides.add_slide`/`SlideShapes.add_textbox`/`add_picture`/`util.Inches`/`Pt`, and `pikepdf.open`/`Pdf.save`/`ObjectStreamMode`/`Pdf.pages` spellings verify against the `reportlab`/`python-docx`/`python-pptx`/`pikepdf` catalogues.
- [RECEIPT_WEAVE]: the prior fence wrote `@receipted(ArtifactReceipt)` and `_emit -> ContentKey` — both wrong against the runtime `receipts.@receipted` contract, which takes a `Redaction` policy value and harvests `contribute()` off the operation's `ReceiptContributor` return. The rebuild matches the runtime owner and the `document/egress#FINISH`/`document/report#REPORT` siblings: `@receipted(_REDACTION)` over a thin `_emit -> Self` returning the stepped `DocumentPlan` (a `ReceiptContributor` via `contribute`), so the weave drains `self.contribute()` and emits through `Signals.emit_async`, and `_emitted` mints the content key off `plan.fact.data` rather than the pure core. `contribute` reads the threaded `EmitFact` off `self.fact`, folds the `Backend.kind` discriminant through `_RECEIPT`, and mints the real `core/receipt#RECEIPT` `ArtifactReceipt.Pdf(key, bytes, pages)`/`.Office(key, bytes)` arities — never a widened tuple, the typography-rail `Document` case, or an in-process re-run of a worker-gated arm.
- [TYPED_PAYLOAD]: the prior fence carried `extra_items=str` while `of` did `EmitSpec(**payload)` with NO band-fold, so any extension key was either a `TypeError` (unknown kwarg) or a wrong-typed `str` against a non-`str` field — the `extra_items` band the prose claimed was illusory. The rebuild closes the payload (`TypedDict(closed=True)` over the full settable `EmitSpec` surface, per-key `NotRequired[ReadOnly[...]]`), admits once through the module-level `TypeAdapter`, and materializes `EmitSpec(**payload)` over a known-key set; an unknown key faults at `validate_python`. The `_REQUIRED` `frozendict[DocumentMode, tuple[str, ...]]` precondition is the two-tier admission gate making the interior total — a mode whose named input (`source`/`template`/`schema`/`selector`/`subset_fonts`/`stylesheet`/`path`) is empty becomes `EmitFault.unsatisfied` at `of`, so no arm reaches an empty `DocxTemplate("")` or `etree.XSLT(b"")`.
- [FAULT_COLLAPSE]: the prior `EmitFault` carried five cases (`payload`/`source`/`schema`/`content`/`typeset`) but the body CONSTRUCTED only `payload` — four decorative provider-conversion cases the arms never minted (the arms `raise` provider exceptions; nothing built `EmitFault(typeset=...)`). The rebuild collapses to the admission vocabulary `of` actually produces — `payload` (rejected keys) plus `unsatisfied` (mode precondition) — and routes every arm-level provider raise (`TypstError`/`PdfiumError`/`XMLSyntaxError`/`XlsxWriterException`) to the runtime `BoundaryFault` at the `async_boundary` capsule, exactly as `document/egress#FINISH` routes its provider raises rather than re-spelling a per-page provider-fault union the runtime already owns.
- [TYPST_WORLD]: three Typst defects are fixed. (1) `world()` constructed `Compiler(font_paths=[], sys_inputs=...)` with NO `input` — but the `typst` `Compiler` constructor requires `input`; the rebuild seeds it with `to_typst_source(self.node).encode()` (the held world amortizes font load, each arm overriding `input=` per compile). (2) `_typst_compile`/`_typst_query` passed a markup `str` as `input`, which the binding treats as a file PATH (`input` is "source path or bytes"); the rebuild passes `.encode()` bytes. (3) `_typst_query` used the held HEAD world's `Compiler.query`, querying the wrong document in a batch; the rebuild uses the free `typst.query(input=this-plan-source-bytes, selector, field, one)` which compiles a single-shot world over the querying plan's OWN document, so `TYPST_QUERY` leaves `_HELD_WORLD`. The PDF/UA path prepends `#set document(title:)` (the `ua-1` render hard-errors `missing document title` without it), and `timestamp` pins the creation date for byte-deterministic archival output. `Compiler`/`compile_with_warnings(input, sys_inputs, pdf_standards, timestamp)` and the free `query` verify against the `typst` catalogue.
- [OUTLINE_VERIFIED]: the prior `_outline_rows` read `mark.level` — a PHANTOM the `pypdfium2` catalogue does not row (a 5.x `PdfBookmark` exposes `get_title`/`get_count`/`get_dest`, NOT a `.level` attribute), and the prose's claim that it "was corrected to `max_depth - get_count()`" was never applied to the code. The rebuild reads the verified `mark.get_count()` (the signed sub-item count) as the `OutlineRow` head, `mark.get_title()`, and `dest.get_index()` off `mark.get_dest()`, so `outline_count` is the verified bookmark count (`len(outline)`) and the `outline` triples carry the per-bookmark `get_count`/title/dest — never a `.level` depth the `PdfBookmark` does not expose. `get_toc(max_depth)`/`PdfBookmark.get_count`/`get_title`/`get_dest`/`PdfDest.get_index` verify against the `pypdfium2` catalogue rows.
- [RUN_FIDELITY]: the prior `_docx_run_props` mapped only `bold`/`size`/`font`/`rtl`/`url_id`, lowering a `RunNode` that `document/model#NODE` carries as bold-italic-superscript-coloured-language-tagged to plain bold — the exact defect the model page's accessibility rebuild fixed, repeated at emission. The rebuild projects the full `docxtpl` `RichText.add` axis — `italic` from `RunNode.italic`, `superscript`/`subscript` from `RunScript`, `color` from the `Rgb` triple, `lang` from `NodeMeta.lang` — and the `_docx_emit`/`_pptx_emit`/`_font_embed` arms carry the same character appearance (`Run.font.color`, `Font.italic`, `Canvas.setFillColorRGB`). `RichText.add(italic, superscript, subscript, color, rtl, lang)`, `python-docx` `Run.font`, and `reportlab` `setFillColorRGB` verify against the catalogues.
- [DOCX_DENSITY]: the dead `_REPLACE` constant (the `("media", "embedded", "zipname", "pic")` tuple referencing methods the body never called) is gone — the `replace: frozendict[str, str]` `EmitSpec` field now wires the real `DocxTemplate.replace_zipname(name, dst)` pre-render part swap, and `render_footnotes` (after `render`) binds the footnote-part XML the prior fence dropped. The `_FORMAT_SETTERS` `getattr`-setter loop collapses into `_FORMATS` `add_format({...})` property-dict rows. `replace_zipname`/`render_footnotes`/`add_format(properties)` verify against the `docxtpl`/`xlsxwriter` catalogues.
- [WEASY_ARCHIVAL]: the HTML→PDF arm gains the archival-correctness surface its catalogue rows — `base_url` (relative-resource resolution the catalogue mandates over string concatenation), `output_intent` (the ICC profile PDF/A conformance requires), and `stylesheets` (supplemental `CSS`) — folded beside the existing `PdfVariant` `pdf_variant`/`pdf_tags`/`pdf_forms` row, and `PdfVariant` gains the `A_1B` profile both engines support (`pdf/a-1b`/`("a-1b",)`). `PdfVariant` additionally rows `A_4` — the weasyprint catalogue's `pdf/a-4` (the PDF 2.0-based archival standard typst does not yet accept), so the `.typst`/`.weasyprint` projections read through `_PDF_STANDARD.get(self, ())`/`_PDF_PROFILE.get(self)` and each engine renders the strongest profile it supports rather than a `KeyError` on a one-engine row; the prior `NONE`-special-cased branches collapse into the defaulted `.get`. `HTML(string, base_url)`/`write_pdf(output_intent, stylesheets)`/`CSS(filename)` and the `pdf/a-4` row verify against the `weasyprint` catalogue.
- [TYPST_EVAL]: the introspection half rowed `TYPST_QUERY` (element selection) but not the catalogue's distinct `eval` (expression evaluation), so `TYPST_EVAL` lands as one `DocumentMode` row over `_typst_eval` — the free `typst.eval(input=this-plan-source-bytes, expression)` single-shot world over the plan's OWN document, the scripting-introspection sibling of `query` and the same `ReceiptKind.OFFICE` `(ContentKey, bytes)` terminal — with `expression` the one `EmitSpec`/`EmitPayload` field its `_REQUIRED` row demands. `eval(input, expression)` verifies against the `typst` catalogue.
- [CELL_FOLD]: `_coerce_cell` ran a per-cell speculative `try/except ValueError` ladder (three parsers attempted in sequence) — the NO-EXCEPTION-HOTLOOP smell, exception flow deciding the cell type once per cell across a whole table grid. It collapses to one module-level compiled `_CELL_SHAPE` classifier whose named alternatives (`int`/`float`/`dt`/`date`) decide the type in a single total `match` over the trimmed text, then `_CELL_TYPED[matched.lastgroup]` dispatches the named total parser: the `int`/`float` arms are regex-guaranteed so they never raise, and only a genuine ISO-shape string reaches the lone `_temporal` guard whose single boundary `try/except` lets a shape-valid but range-invalid stamp (month 13) fall back to raw text — so the common text/number/bool/empty cell never enters an exception path and the speculative ladder is gone. The same cell algebra admits tz-aware `datetime.fromisoformat` results and `float('inf')`, so the `_xlsx_streamed` workbook options carry the catalogue's `remove_timezone=True` and `nan_inf_to_errors=True` so a tz-aware datetime serializes and a non-finite float maps to an Excel error code rather than raising. The `re.Pattern.match`/`Match.lastgroup` spelling is stdlib and `remove_timezone`/`nan_inf_to_errors` verify against the `xlsxwriter` catalogue.

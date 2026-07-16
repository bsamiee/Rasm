# [PY_ARTIFACTS_EMIT]

The document-emission axis over the one `DocumentNode` semantic tree: `DocumentPlan` discriminates document mode over the `BACKENDS` policy table, and every arm is a lowering fold FROM the `document/model#NODE` tree — PDF authoring, archival HTML-CSS, tagged PDF/UA, Typst typesetting, raster, assembly, repair, OOXML and OpenDocument office, structured text, and the Markdown/LaTeX manuscript egress — never an opaque payload. Emission lowers FROM the tree and `document/lens#LENS` recovers TO it, so production and extraction are inverses over the one node algebra.

Each `Backend` row binds its arm to its runtime `Band`, so the runtime/worker split is a row column rather than a second dispatch surface: a `CORE`-band arm crosses `Modality.THREAD`, a `WORKER`-band arm crosses `Modality.PROCESS` with `retry=RetryClass.OCCT` onto `_worker_emit`, which re-resolves the SAME `BACKENDS` row — the worker lane carries no second `match`, and both ride the runtime-owned offload bound. `EmitSpec` admits exactly once at `DocumentPlan.of` through the closed `EmitPayload` `TypedDict` under the per-mode `_REQUIRED` precondition, so the interior never re-validates; arm-level provider raises convert to the runtime `BoundaryFault` at the `async_boundary` capsule. The `@receipted(OPEN)` weave drains `contribute` off the stepped owner; `core/plan#PLAN` schedules the node set and `core/issue#ISSUE` constructs it; a `template` payload format-IDs through `exchange/detect#DETECT` before any engine trusts its extension.

## [01]-[INDEX]

- [01]-[DOCUMENT]: document-mode dispatch over the band-bound `BACKENDS` policy table, every arm lowering from the `DocumentNode` tree.

## [02]-[DOCUMENT]

- Entry: the key mints PRE-RUN over the canonical `(mode, node, spec)` input — `receipt.slot == node.key`, the produced output content-address riding the receipt FACTS, never the elision key. `bound(node, modes, **payload)` is the one-context-many-formats fan: one admitted payload binds one `DocumentNode` context to N format nodes with per-mode keys, so a re-issued package re-renders only changed formats — never a per-format vocabulary re-spelling `DocumentMode` and never a second scheduling rail. The held Typst `Compiler` world amortizes font discovery across a plan's renders.
- Auto: `_run_markup` carries each `RunNode`'s real weight/italic/colour/baseline/decoration with `<`/`&` escaped once before any tag — a plain `run.text` splice drops run fidelity; the `A_3A` weasyprint variant embeds its PDF/A-3 source files through `attachments`, never only naming them; each `PageNode` opens its own tagged sheet through `new_page_same_size`, so a multi-page UA document never overflows one buffered page; the manuscript rows escape through the model owner's `_MD_ESCAPE`/`_LATEX_ESCAPE` maps so no active character breaks the source.
- Receipt: `contribute` reads the threaded `EmitFact` off `self.fact` — never an in-process re-run of a worker-gated arm — and folds the case off the `Backend.kind` discriminant: emit mints only the `ArtifactReceipt.Pdf`/`.Office` arities the receipt owner declares; the typography-rail `Document` case is not its to mint. Rich per-arm evidence (render scale, outline count, embedded-face set, undeclared-variable set, validation verdict) rides the `EmitFact` carrier, never a widened receipt tuple.
- Growth: a new document format is one `DocumentMode` row plus one `Backend` row binding its arm and band, plus a `_REQUIRED` row when it demands an input; a new typed cell is one `CellValue` arm; a new interactive-field kind is one `FieldKind` member plus one `_ua_field` arm; a new archival profile is one `PdfVariant` row projecting through `_PDF_STANDARD`/`_PDF_PROFILE`/`_ACCESSIBLE` to both engines; a new evidence fact is one `EmitFact` field; a new admission cause is one `EmitFault` case.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
import io
import re
from collections.abc import Callable, Iterable, Iterator
from copy import replace
from datetime import date, datetime
from pathlib import Path
from enum import StrEnum
from typing import TYPE_CHECKING, Final, Literal, NotRequired, ReadOnly, Self, TypedDict, Unpack, assert_never

import msgspec
from expression import Error, Ok, Result, case, tag, tagged_union
from expression.collections import Block, Map
from expression.extra.result import sequence
from msgspec import Struct, field, to_builtins
from pydantic import TypeAdapter, ValidationError

from rasm.runtime.identity import CANONICAL_POLICY, ContentIdentity, ContentKey
from rasm.runtime.faults import RuntimeRail
from rasm.runtime.lanes import LanePolicy, Modality
from rasm.runtime.receipts import OPEN, Receipt, receipted
from rasm.runtime.resilience import RetryClass

from rasm.artifacts.core.plan import Admission, ArtifactWork
from rasm.artifacts.core.receipt import ArtifactReceipt
from rasm.artifacts.exchange.detect import Detect
from rasm.artifacts.document.model import (
    AnnotationNode,
    AnnotKind,
    BlockKind,
    BlockNode,
    DocumentNode,
    FieldKind,
    FieldNode,
    FigureNode,
    ForeignRole,
    ListKind,
    ListNode,
    PageNode,
    RunNode,
    RunScript,
    SectionNode,
    StructureNode,
    TableNode,
    TextDecoration,
    TextDirection,
    Uri,
    role_of,
    standard_for,
    to_html,
    to_latex,
    to_lxml_tree,
    to_markdown,
    to_typst_source,
    walk,
)

lazy import docx
lazy import numpy as np
lazy import pdf_oxide
lazy import pikepdf
lazy import pymupdf
lazy import pypdfium2
lazy import tomlkit
lazy import typst
lazy import xlsxwriter
lazy from docx.enum.section import WD_ORIENTATION
lazy from docx.enum.style import WD_STYLE_TYPE
lazy from docx.enum.table import WD_CELL_VERTICAL_ALIGNMENT
lazy from docx.shared import Cm, Pt, RGBColor
lazy from docxtpl import DocxTemplate, InlineImage, Listing, RichText, RichTextParagraph
lazy from lxml import etree
lazy from odf import dc
lazy from odf.draw import Frame as OdfFrame, Image as OdfImage
lazy from odf.number import DateStyle, Day, Month, Text as NumberText, Year
lazy from odf.opendocument import OpenDocumentSpreadsheet, OpenDocumentText
lazy from odf.style import ParagraphProperties, Style as OdfStyle, TextProperties
lazy from odf.table import CoveredTableCell, Table as OdfTable, TableCell, TableHeaderRows, TableRow
lazy from odf.teletype import addTextToElement
lazy from odf.text import A as OdfLink, H, List as OdfList, ListItem as OdfListItem, Note, NoteBody, NoteCitation, P, Span as OdfSpan
lazy from pdf_oxide import Column as OxideColumn, DocumentBuilder, EmbeddedFont, Pdf, Table as OxideTable
lazy from pptx import Presentation
lazy from pptx.util import Inches, Pt as SlidePt
lazy from pypdf import PdfReader, PdfWriter
lazy from reportlab.lib import colors
lazy from reportlab.lib.pagesizes import A4
lazy from reportlab.lib.styles import ParagraphStyle, getSampleStyleSheet
lazy from reportlab.lib.units import mm
lazy from reportlab.pdfbase import pdfmetrics
lazy from reportlab.pdfbase.ttfonts import TTFont
lazy from reportlab.pdfgen.canvas import Canvas
lazy from reportlab.platypus import (
    BaseDocTemplate,
    Frame,
    Image,
    ListFlowable,
    ListItem,
    PageBreak,
    PageTemplate,
    Paragraph,
    Spacer,
    Table,
    TableStyle,
)
lazy from reportlab.platypus.tableofcontents import TableOfContents
lazy from ruamel.yaml import YAML
lazy from typst import Compiler
lazy from weasyprint import CSS, HTML, Attachment

if TYPE_CHECKING:
    from typst import Compiler

# --- [TYPES] ----------------------------------------------------------------------------


class DocumentMode(StrEnum):
    PDF_AUTHOR = "pdf-author"
    PDF_HTML = "pdf-html"
    PDF_OXIDE = "pdf-oxide"  # commercial-safe (MIT/Apache) HTML->PDF create — pdf_oxide `Pdf.from_html`
    PDF_UA = "pdf-ua"  # native tagged PDF/UA-1 fluent author — pdf_oxide `DocumentBuilder`
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
    MARKDOWN = "markdown"  # CommonMark/GFM plain-text manuscript — `document/model#NODE` `to_markdown` node lowering
    LATEX = "latex"  # journal-submission LaTeX manuscript — `document/model#NODE` `to_latex` node lowering


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
    A_2A = "a-2a"  # PDF/A-2 accessible: archival AND tagged — the both-telos preservation+accessibility union
    A_3A = "a-3a"  # PDF/A-3 accessible: archival + tagged + embedded source files (the AEC deliverable seal)
    A_4 = "a-4"  # PDF/A-4 (PDF 2.0 archival)
    UA_1 = "ua-1"

    @property
    def typst(self) -> tuple[str, ...]:
        return _PDF_STANDARD.try_find(self).default_value(())

    @property
    def weasyprint(self) -> str | None:
        return _PDF_PROFILE.try_find(self).default_value(None)

    @property
    def tagged(self) -> bool:
        return self in _ACCESSIBLE


type Arm = Callable[["DocumentPlan", "Compiler | None"], "EmitFact"]
type OutlineRow = tuple[int, str, int]  # (sub-item count, title, destination page index)
type CellScalar = str | float | bool | datetime | date

# --- [CONSTANTS] ------------------------------------------------------------------------

_STREAMING_ROW_THRESHOLD: Final = 50_000
_ZIP64_ROW_THRESHOLD: Final = 1_048_576
_OUTLINE_MAX_DEPTH: Final = 15
_RASTER_SCALE: Final = 4.0
_BOLD_WEIGHT: Final = 700
_PT_PER_HALFPT: Final = 2
_HEADER_WIDTH: Final = 18.0
_NUM_FORMAT: Final = "0.############"
_DATE_FORMAT: Final = "yyyy-mm-dd hh:mm:ss"
_BOOL_CELL: Final[Map[str, bool]] = Map.of_seq([("true", True), ("false", False), ("yes", True), ("no", False)])
# run text translates ONCE at the boundary before any `<b>`/`<font>`/`<a>` tag is composed, so a run carrying
# `<`/`&`/`"` never breaks the reportlab mini-XML parser.
_MARKUP_ESCAPE: Final = str.maketrans({"&": "&amp;", "<": "&lt;", ">": "&gt;", '"': "&quot;"})
# the typst `PDFStandard` token tuple per variant — the archival token alone for `-b`/`a-4`, the archival
# token PLUS `ua-1` for the accessible `-a` and `ua-1` rows so one render is conformant AND screen-readable.
_PDF_STANDARD: Final[Map[PdfVariant, tuple[str, ...]]] = Map.of_seq([
    (PdfVariant.A_1B, ("a-1b",)),
    (PdfVariant.A_2B, ("a-2b",)),
    (PdfVariant.A_3B, ("a-3b",)),
    (PdfVariant.A_4, ("a-4",)),
    (PdfVariant.A_2A, ("a-2a", "ua-1")),
    (PdfVariant.A_3A, ("a-3a", "ua-1")),
    (PdfVariant.UA_1, ("a-3b", "ua-1")),
])
# the weasyprint `pdf_variant` profile per variant — only `-b`/`a-4`/`ua-1` ship, so the accessible `-a`
# rows project to their `-b` profile and ride `pdf_tags=True` (the `tagged` flag) for the structure tree.
_PDF_PROFILE: Final[Map[PdfVariant, str]] = Map.of_seq([
    (PdfVariant.A_1B, "pdf/a-1b"),
    (PdfVariant.A_2B, "pdf/a-2b"),
    (PdfVariant.A_3B, "pdf/a-3b"),
    (PdfVariant.A_4, "pdf/a-4"),
    (PdfVariant.A_2A, "pdf/a-2b"),
    (PdfVariant.A_3A, "pdf/a-3b"),
    (PdfVariant.UA_1, "pdf/ua-1"),
])
# the tagged (structure-tree-bearing) variants: the `-a` accessible levels and `ua-1` set `pdf_tags=True`.
_ACCESSIBLE: Final[frozenset[PdfVariant]] = frozenset({PdfVariant.A_2A, PdfVariant.A_3A, PdfVariant.UA_1})
# `add_format({...})` property dicts replace the per-setter `getattr` loop — one row, no stringly setter dispatch.
_FORMATS: Final[Map[str, frozendict[str, object]]] = Map.of_seq([
    ("header", frozendict({"bold": True, "align": "center"})),
    ("number", frozendict({"num_format": _NUM_FORMAT})),
    ("datetime", frozendict({"num_format": _DATE_FORMAT})),
])

# --- [ERRORS] ---------------------------------------------------------------------------


@tagged_union(frozen=True)
class EmitFault:
    # the closed ADMISSION vocabulary `of` produces; arm-level provider raises (`TypstError`/`PdfiumError`/`XMLSyntaxError`/
    # `XlsxWriterException`) convert to the runtime `BoundaryFault` at the `async_boundary` capsule, never into this vocabulary.
    tag: Literal["payload", "unsatisfied"] = tag()
    payload: tuple[str, ...] = case()  # the rejected EmitPayload key paths
    unsatisfied: tuple[DocumentMode, str] = case()  # a mode whose `_REQUIRED` input field is empty


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

    def odf_cell(self) -> tuple[str, frozendict[str, object], str]:
        # bool and date key their own `booleanvalue`/`datevalue` attributes rather than collapsing to a `string` cell,
        # so the `date` rows bind the `odf.number` `DateStyle` and the sheet round-trips as typed data, not display text.
        match self.raw:
            case bool() as flag:
                return ("boolean", frozendict({"booleanvalue": "true" if flag else "false"}), "TRUE" if flag else "FALSE")
            case int() | float() as number:
                return ("float", frozendict({"value": float(number)}), str(number))
            case datetime() | date() as moment:
                return ("date", frozendict({"datevalue": moment.isoformat()}), moment.isoformat())
            case None | "":
                return ("string", frozendict(), "")
            case str() as text:
                return ("string", frozendict(), text)


class SpreadsheetPolicy(Struct, frozen=True):
    regime: XlsxRegime
    crossover: int

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
    outline_count: int = 0  # reportlab `addOutlineEntry`/TOC entries or the pypdfium2 bookmark count
    faces: tuple[str, ...] = ()
    undeclared: tuple[str, ...] = ()
    template_path: str = ""
    queried: int = 0
    valid: bool = True
    warnings: int = 0
    errors: int = 0
    outline: tuple[OutlineRow, ...] = ()
    tagged: bool = False  # PDF/UA structure tree emitted (pdf_oxide `tagged_pdf_ua1`, weasyprint `pdf_tags`)
    structure: int = 0  # tagged structure elements or `role_map` foreign->standard rows
    merges: int = 0  # merged/spanned office cells folded from `TableNode.spans`
    footnotes: int = 0  # authored `text:note` footnotes / docx comments


class EmitSpec(Struct, frozen=True, omit_defaults=True):
    parents: tuple[ContentKey, ...] = ()  # upstream content keys (placed figures, authored sections) the node's plan edges ride
    source: bytes = b""
    password: str = ""
    title: str = ""
    author: str = ""
    subject: str = ""
    toc: bool = False  # reportlab multiBuild table-of-contents + PDF outline from heading flowables
    header_text: str = ""  # title-block running header (reportlab onPage sheet furniture, docx/odf section header)
    footer_text: str = ""  # title-block running footer; the page-number field renders per page
    landscape: bool = False  # page orientation for the reportlab/docx/xlsx print layout
    variant: PdfVariant = PdfVariant.NONE
    forms: bool = False
    output_intent: str = ""  # weasyprint PDF/A ICC output-intent profile path
    base_url: str = ""  # weasyprint relative-resource resolution root
    stylesheets: tuple[str, ...] = ()  # weasyprint supplemental CSS paths
    attachments: frozendict[str, str] = field(
        default_factory=frozendict
    )  # PDF/A-3 embedded source files: display-name -> file path, the `/Source`-relationship AEC deliverable seal the A_3A variant requires
    full_fonts: bool = False  # weasyprint embed complete fonts (archival fidelity) instead of subsetting
    optimize_images: bool = False  # weasyprint recompress embedded raster images
    presentational_hints: bool = False  # weasyprint honor legacy HTML presentational attributes
    selector: str = ""
    field_name: str = ""
    one: bool = False
    expression: str = ""  # typst `eval` source expression evaluated against the document
    sys_inputs: frozendict[str, str] = field(default_factory=frozendict)
    timestamp: int = 0  # typst reproducible-byte creation pin (epoch); 0 = unpinned
    image_format: str = "png"  # pymupdf native `Pixmap.tobytes` codec
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
    toc: NotRequired[ReadOnly[bool]]
    header_text: NotRequired[ReadOnly[str]]
    footer_text: NotRequired[ReadOnly[str]]
    landscape: NotRequired[ReadOnly[bool]]
    variant: NotRequired[ReadOnly[PdfVariant]]
    forms: NotRequired[ReadOnly[bool]]
    output_intent: NotRequired[ReadOnly[str]]
    base_url: NotRequired[ReadOnly[str]]
    stylesheets: NotRequired[ReadOnly[tuple[str, ...]]]
    attachments: NotRequired[ReadOnly[frozendict[str, str]]]
    full_fonts: NotRequired[ReadOnly[bool]]
    optimize_images: NotRequired[ReadOnly[bool]]
    presentational_hints: NotRequired[ReadOnly[bool]]
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
_REQUIRED: Final[Map[DocumentMode, tuple[str, ...]]] = Map.of_seq([
    (DocumentMode.PDF_RENDER, ("source",)),
    (DocumentMode.PDF_RASTER, ("source",)),
    (DocumentMode.PDF_ASSEMBLE, ("source",)),
    (DocumentMode.PDF_REPAIR, ("source",)),
    (DocumentMode.TYPST_QUERY, ("selector",)),
    (DocumentMode.TYPST_EVAL, ("expression",)),
    (DocumentMode.FONT_EMBED, ("subset_fonts",)),
    (DocumentMode.DOCX_TEMPLATE, ("template",)),
    (DocumentMode.XML_TRANSFORM, ("stylesheet",)),
    (DocumentMode.XML_VALIDATE, ("schema",)),
    (DocumentMode.XML_QUERY, ("path",)),
])


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
        # each Typst arm overrides `input=` per compile, so the cached fonts serve every same-context render.
        return Compiler(to_typst_source(self.node).encode(), font_paths=[], sys_inputs=dict(self.spec.sys_inputs))

    def emit(self, /) -> ArtifactWork:
        return ArtifactWork(key=self._key, work=self._emit, parents=self.spec.parents, admission=Admission(keyed=None), cost=1.0)

    @property
    def _key(self) -> ContentKey:
        return ContentIdentity.of(self.mode.value, (self.node, self.spec), policy=CANONICAL_POLICY)

    async def _emit(self, /) -> RuntimeRail[ArtifactReceipt]:
        stepped = await self._stepped(self.world() if self.mode in _HELD_WORLD else None)
        return Ok(_RECEIPT[BACKENDS[self.mode].kind](self._key, stepped.fact))

    @receipted(OPEN)  # emit facts carry no classified field, so the runtime keep-all `OPEN` policy rides directly, never a re-minted per-file `Redaction`
    async def _stepped(self, world: "Compiler | None", /) -> Self:
        backend = BACKENDS[self.mode]
        fact = (
            await LanePolicy.offload(_worker_emit, self, modality=Modality.PROCESS, retry=RetryClass.OCCT)
            if backend.band is Band.WORKER
            else await LanePolicy.offload(backend.arm, self, world, modality=Modality.THREAD)
        )
        return replace(self, fact=fact.default_with(_emit_raise))

    def contribute(self) -> Iterable[Receipt]:
        if (fact := self.fact) is None:
            return
        yield from _RECEIPT[BACKENDS[self.mode].kind](self._key, fact).contribute()

    @classmethod
    def of(cls, mode: DocumentMode, node: DocumentNode, /, **raw: Unpack[EmitPayload]) -> Result[Self, EmitFault]:
        try:
            payload = _PAYLOAD.validate_python(raw)
        except ValidationError as fault:
            return Error(EmitFault(payload=tuple(str(error["loc"]) for error in fault.errors())))
        spec = EmitSpec(**payload)
        missing = next((name for name in _REQUIRED.try_find(mode).default_value(()) if not getattr(spec, name)), None)
        return Error(EmitFault(unsatisfied=(mode, missing))) if missing else Ok(cls(mode=mode, node=node, spec=spec))

    @classmethod
    def bound(cls, node: DocumentNode, modes: "Iterable[DocumentMode]", /, **raw: Unpack[EmitPayload]) -> Result[tuple[ArtifactWork, ...], EmitFault]:
        # one plan per mode, per-mode keys, per-member elision.
        return (
            sequence(Block.of_seq(cls.of(mode, node, **raw) for mode in modes))
            .map(lambda plans: tuple(plan.emit() for plan in plans))
        )


# --- [OPERATIONS] -----------------------------------------------------------------------


def _emit_raise(fault: object) -> "EmitFact":
    # terminal collapse at the render boundary: an offload fault reconstructs the raise the drain's
    # fault capsule folds onto the node's rail.
    raise ValueError(str(fault))


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
_CELL_TYPED: Final[Map[str, Callable[[str], CellScalar]]] = Map.of_seq([
    ("int", int),
    ("float", float),
    ("dt", lambda text: _temporal(datetime.fromisoformat, text)),
    ("date", lambda text: _temporal(date.fromisoformat, text)),
])


def _coerce_cell(text: str) -> CellScalar | None:
    stripped = text.strip()
    if not stripped:
        return None
    if (folded := stripped.casefold()) in _BOOL_CELL:
        return _BOOL_CELL[folded]
    return _CELL_TYPED[matched.lastgroup](stripped) if (matched := _CELL_SHAPE.match(stripped)) is not None else text


def _flowables(node: DocumentNode, styles: object, spec: EmitSpec, /) -> Iterator[object]:
    # `QUOTE` and inline `LINK` are their own arms, never Normal.
    match node:
        case RunNode():
            yield Paragraph(_run_markup(node), styles["Normal"])
        case BlockNode(block=BlockKind.HEADING, level=level, runs=runs):
            yield Paragraph(_runs_markup(runs), styles[f"Heading{min(max(level, 1), 6)}"])
        case BlockNode(block=BlockKind.CODE, runs=runs):
            yield Paragraph(_runs_markup(runs), styles["Code"])
        case BlockNode(block=BlockKind.QUOTE, runs=runs, children=kids):
            yield Paragraph(_runs_markup(runs), styles["Italic"])
            for kid in kids:
                yield from _flowables(kid, styles, spec)
        case BlockNode(runs=runs, children=kids):
            yield Paragraph(_runs_markup(runs), styles["Normal"])
            for kid in kids:
                yield from _flowables(kid, styles, spec)
        case ListNode(list_kind=kind, items=items):
            entries = [ListItem(Paragraph(_node_markup(item), styles["Normal"])) for item in items]
            yield ListFlowable(
                entries, bulletType="1" if kind is ListKind.ORDERED else "bullet", start=node.start if kind is ListKind.ORDERED else None
            )
        case TableNode() as table:
            yield _table_flowable(table, styles)
        case FigureNode(asset_key=asset_key, intrinsic=intrinsic, caption=caption) if asset_key.hex in spec.assets:
            width, height = intrinsic or (None, None)  # native size when the model carries it, else reportlab's own fit
            yield Image(spec.assets[asset_key.hex], width=width, height=height)
            if caption:
                yield Paragraph(_runs_markup(caption), styles["Italic"])
        case AnnotationNode(annot=AnnotKind.LINK, contents=text, link=Uri(href=href)):
            yield Paragraph(f'<a href="{href.translate(_MARKUP_ESCAPE)}">{(text or href).translate(_MARKUP_ESCAPE)}</a>', styles["Normal"])
        case SectionNode(level=level, heading=head, children=kids):
            yield Paragraph(_runs_markup(head), styles[f"Heading{min(max(level, 1), 6)}"])
            for kid in kids:
                yield from _flowables(kid, styles, spec)
        case PageNode(children=kids):
            for kid in kids:
                yield from _flowables(kid, styles, spec)
            yield PageBreak()
        case _:
            yield Spacer(1, 0)


def _text(node: DocumentNode) -> str:
    return "".join(
        run.text for run in walk(node) if isinstance(run, RunNode)
    )  # plain text for docx/xlsx/UA-linearize; the reportlab arm uses the markup form below


def _run_markup(run: RunNode, /) -> str:
    # font-face markup is withheld — `PDF_AUTHOR` registers no per-run face, so `<font name>` references an unregistered font,
    # while colour/weight/style ride safely; `text` escapes ONCE.
    body = run.text.translate(_MARKUP_ESCAPE)
    if run.color != (0, 0, 0):
        body = f'<font color="#{run.color[0]:02X}{run.color[1]:02X}{run.color[2]:02X}">{body}</font>'
    if TextDecoration.UNDERLINE in run.decorations:
        body = f"<u>{body}</u>"
    if TextDecoration.STRIKETHROUGH in run.decorations:
        body = f"<strike>{body}</strike>"
    body = f"<super>{body}</super>" if run.script is RunScript.SUPER else f"<sub>{body}</sub>" if run.script is RunScript.SUB else body
    body = f"<i>{body}</i>" if run.italic else body
    return f"<b>{body}</b>" if run.weight >= _BOLD_WEIGHT else body


def _runs_markup(runs: tuple[RunNode, ...], /) -> str:
    return "".join(_run_markup(run) for run in runs)


def _node_markup(node: DocumentNode, /) -> str:
    # a cell/list-item subtree flattened to escaped run-fidelity markup for one `Paragraph`.
    return _runs_markup(tuple(run for run in walk(node) if isinstance(run, RunNode)))


def _span_commands(spans: tuple[tuple[int, int, int, int], ...], /) -> Iterator[tuple[object, ...]]:
    # `TableNode.spans` `(row, col, col_span, row_span)` -> the platypus `('SPAN', (c0, r0), (c1, r1))` command
    for row, col, col_span, row_span in spans:
        yield ("SPAN", (col, row), (col + max(col_span, 1) - 1, row + max(row_span, 1) - 1))


def _table_flowable(node: TableNode, styles: object, /) -> Table:
    # `repeatRows` reprints the header band on every page.
    data = [[Paragraph(_node_markup(cell), styles["BodyText"]) for cell in row] for row in node.rows]
    head, foot, last = node.header_rows, node.footer_rows, len(node.rows) - 1
    commands: list[tuple[object, ...]] = [
        ("GRID", (0, 0), (-1, -1), 0.5, colors.grey),
        ("VALIGN", (0, 0), (-1, -1), "MIDDLE"),
        ("LEFTPADDING", (0, 0), (-1, -1), 4),
        ("RIGHTPADDING", (0, 0), (-1, -1), 4),
        ("TOPPADDING", (0, 0), (-1, -1), 2),
        ("BOTTOMPADDING", (0, 0), (-1, -1), 2),
        *(
            [("BACKGROUND", (0, 0), (-1, head - 1), colors.HexColor("#E8E8E8")), ("FONTNAME", (0, 0), (-1, head - 1), "Helvetica-Bold")]
            if head
            else ()
        ),
        *([("BACKGROUND", (0, last - foot + 1), (-1, -1), colors.HexColor("#F4F4F4"))] if foot else ()),
        *_span_commands(node.spans),
    ]
    return Table(data, repeatRows=head or 0, style=TableStyle(commands))


class _SheetDocTemplate(BaseDocTemplate):
    # `multiBuild` resolves the TOC page numbers and the bookmark tree in one owner — never a hand-computed page
    # number; `outline` counts the emitted entries for the `EmitFact`.
    outline: int = 0
    _seq: int = 0

    def afterFlowable(self, flowable: object, /) -> None:
        style = getattr(getattr(flowable, "style", None), "name", "")
        if isinstance(style, str) and style.startswith("Heading"):
            level, text, key = int(style.removeprefix("Heading") or "1") - 1, flowable.getPlainText(), f"h{self._seq}"
            self._seq += 1
            self.canv.bookmarkPage(key)
            self.canv.addOutlineEntry(text, key, level=min(level, 6), closed=level > 0)
            self.notify("TOCEntry", (level, text, self.page, key))
            self.outline += 1


def _title_block(spec: EmitSpec, node_key: str, /) -> Callable[[object, object], None]:
    # a closure over the spec so the same callback serves first and later pages; furniture paints outside the content frame.
    def paint(canvas: object, doc: object, /) -> None:
        width, height = doc.pagesize
        canvas.saveState()
        canvas.setFont("Helvetica", 8)
        canvas.drawString(20 * mm, height - 12 * mm, spec.header_text or spec.title or node_key)
        canvas.drawString(20 * mm, 10 * mm, spec.footer_text or spec.author)
        canvas.drawRightString(width - 20 * mm, 10 * mm, f"{doc.page}")
        canvas.line(20 * mm, height - 14 * mm, width - 20 * mm, height - 14 * mm)
        canvas.restoreState()

    return paint


def _reportlab_author(plan: DocumentPlan, _world: "Compiler | None", /) -> EmitFact:
    sink, size = io.BytesIO(), (A4[1], A4[0]) if plan.spec.landscape else A4
    styles = getSampleStyleSheet()
    doc = _SheetDocTemplate(sink, pagesize=size, title=plan.spec.title or plan.node.meta.key.hex, author=plan.spec.author, subject=plan.spec.subject)
    frame = Frame(20 * mm, 18 * mm, size[0] - 40 * mm, size[1] - 34 * mm, id="body")
    doc.addPageTemplates([PageTemplate(id="sheet", frames=[frame], onPage=_title_block(plan.spec, plan.node.meta.key.hex))])
    toc = TableOfContents(levelStyles=[ParagraphStyle(name=f"TOC{n}", fontSize=11 - n, leftIndent=12 * n) for n in range(3)])
    story = [toc, PageBreak(), *_flowables(plan.node, styles, plan.spec)] if plan.spec.toc else list(_flowables(plan.node, styles, plan.spec))
    doc.multiBuild(story) if plan.spec.toc else doc.build(story)  # multiBuild resolves the TOC/outline page numbers across passes
    return EmitFact(data=sink.getvalue(), pages=doc.page, outline_count=doc.outline)


def _weasyprint_html(plan: DocumentPlan, _world: "Compiler | None", /) -> EmitFact:
    spec = plan.spec
    # `full_fonts`/`optimize_images`/`presentational_hints` are the archival controls.
    attachments = [Attachment(filename=path, name=name, relationship="Source") for name, path in spec.attachments.items()]
    data = HTML(string=to_html(plan.node), base_url=spec.base_url or None).write_pdf(
        target=None,
        pdf_variant=spec.variant.weasyprint,
        pdf_tags=spec.variant.tagged,
        pdf_forms=spec.forms,
        output_intent=spec.output_intent or None,
        stylesheets=[CSS(filename=sheet) for sheet in spec.stylesheets] or None,
        attachments=attachments or None,
        full_fonts=spec.full_fonts,
        optimize_images=spec.optimize_images,
        presentational_hints=spec.presentational_hints,
    )
    return EmitFact(data=data, tagged=spec.variant.tagged)


def _pdf_oxide_create(plan: DocumentPlan, _world: "Compiler | None", /) -> EmitFact:
    # the commercial-safe (MIT/Apache) HTML->PDF create sibling of the full-CSS weasyprint arm; serializes the `Pdf` value, no open handle.
    pdf = Pdf.from_html(to_html(plan.node), title=plan.spec.title or plan.node.meta.key.hex, author=plan.spec.author)
    return EmitFact(data=pdf.to_bytes(), pages=len(pdf))


def _ua_field(page: object, node: FieldNode, /) -> object:
    # a native AcroForm widget positioned from `NodeMeta.bounds`; the `FluentPageBuilder` owns the full form vocabulary.
    x0, y0, x1, y1 = node.meta.bounds or (20.0, 40.0, 220.0, 60.0)
    box, filled = (x0, y0, x1 - x0, y1 - y0), "" if node.value is None else str(node.value)
    match node.field:
        case FieldKind.TEXT:
            return page.text_field(node.name, *box, default_value=filled)
        case FieldKind.CHECKBOX:
            return page.checkbox(node.name, *box, bool(node.value))
        case FieldKind.CHOICE:
            return page.combo_box(node.name, *box, list(node.options), selected=filled or None)
        case FieldKind.SIGNATURE:
            return page.signature_field(node.name, *box)
        case FieldKind.BUTTON:
            return page.push_button(node.name, *box, filled or node.name)
        case _ as unreachable:
            assert_never(unreachable)


def _ua_block(page: object, node: DocumentNode, spec: EmitSpec, /) -> object:
    # a figure emits an alt-carrying image (the PDF/UA alt-text requirement) and a `TableNode` a NATIVE tagged `Table` —
    # real THead/TR/TD structure elements, never tab-joined text that erases the table's accessibility; the builder
    # returns `self`, so the caller threads the successor page.
    match node:
        case SectionNode(level=level, heading=head):
            return page.heading(min(max(level, 1), 6), "".join(run.text for run in head))
        case BlockNode(block=BlockKind.HEADING, level=level, runs=runs):
            return page.heading(min(max(level, 1), 6), "".join(run.text for run in runs))
        case BlockNode(runs=runs) if runs:
            return page.paragraph("".join(run.text for run in runs))
        case FigureNode(asset_key=asset_key, alt=alt, intrinsic=intrinsic) if asset_key.hex in spec.assets:
            width, height = intrinsic or (200.0, 150.0)
            return page.image_with_alt(Path(spec.assets[asset_key.hex]).read_bytes(), 20.0, 40.0, width, height, alt)
        case FieldNode() as field:
            return _ua_field(page, field)
        case TableNode(rows=rows, header_rows=head_n) if rows:
            # native tagged `Table`: the leading header row keys the `Column` headers (the tagged THead), the
            # remaining rows the TD body; `has_header` stays off since the headers ride the columns.
            columns = [OxideColumn(_text(cell)) for cell in rows[0]] if head_n else [OxideColumn("") for _ in rows[0]]
            body = [[_text(cell) for cell in row] for row in (rows[head_n:] if head_n else rows)]
            return page.table(OxideTable(columns=columns, rows=body, has_header=False))
        case _:
            return page


def _pdf_ua_build(plan: DocumentPlan, _world: "Compiler | None", /) -> EmitFact:
    # the accessibility-first create path the weasyprint `pdf_tags` flag only approximates: `role_map` binds each foreign
    # `StructureNode` role to its canonical standard element, and embedded faces register once.
    builder = DocumentBuilder().title(plan.spec.title or plan.node.meta.key.hex).author(plan.spec.author).tagged_pdf_ua1()
    for face, blob in plan.spec.subset_fonts.items():
        builder = builder.register_embedded_font(face, EmbeddedFont.from_bytes(blob, name=face))
    foreign = tuple(n for n in walk(plan.node) if isinstance(n, StructureNode) and isinstance(n.role, ForeignRole))
    for structure in foreign:
        builder = builder.role_map(role_of(structure), standard_for(structure.role).value)
    sheets = [n for n in walk(plan.node) if isinstance(n, PageNode)] or [plan.node]
    page = builder.a4_page()
    for index, sheet in enumerate(sheets):
        page = page.new_page_same_size() if index else page  # the first sheet is the opened a4_page; each next opens its own
        for block in (n for n in walk(sheet) if isinstance(n, SectionNode | BlockNode | FigureNode | TableNode | FieldNode)):
            page = _ua_block(page, block, plan.spec)
    return EmitFact(data=page.done().build(), tagged=True, structure=len(foreign))


def _typst_compile(plan: DocumentPlan, world: "Compiler | None", /) -> EmitFact:
    compiler = world if world is not None else plan.world()  # a non-Typst-headed batch falls back per-plan
    # a `ua-1` render hard-errors `missing document title`, so tagged variants derive one; `to_typst_source(node, title=)`
    # owns the escaped `#set document(title:)` set-rule, so this seam never hand-rolls the markup escape.
    title = (plan.spec.title or plan.node.meta.key.hex) if plan.spec.variant.tagged else None
    source = to_typst_source(plan.node, title=title)
    data, warnings = compiler.compile_with_warnings(
        input=source.encode(),
        output=None,
        sys_inputs=dict(plan.spec.sys_inputs),
        pdf_standards=plan.spec.variant.typst,
        timestamp=plan.spec.timestamp or None,
    )
    return EmitFact(data=data, warnings=len(warnings))


def _typst_query(plan: DocumentPlan, _world: "Compiler | None", /) -> EmitFact:
    # the free function compiles a single-shot world over THIS plan's own document — never the held
    # head world, which would query the wrong document in a batch.
    result = typst.query(
        input=to_typst_source(plan.node).encode(), selector=plan.spec.selector, field=plan.spec.field_name or None, one=plan.spec.one
    ).encode()
    return EmitFact(data=result, queried=len(result))


def _typst_eval(plan: DocumentPlan, _world: "Compiler | None", /) -> EmitFact:
    # the expression-evaluation inverse of `_typst_query`: a single-shot world over THIS plan's own document.
    result = typst.eval(input=to_typst_source(plan.node).encode(), expression=plan.spec.expression).encode()
    return EmitFact(data=result, queried=len(result))


def _pymupdf_render(plan: DocumentPlan, _world: "Compiler | None", /) -> EmitFact:
    doc = pymupdf.open(stream=plan.spec.source, filetype="pdf")
    try:
        pixmaps = [
            doc[index].get_pixmap(matrix=pymupdf.Matrix(plan.spec.scale, plan.spec.scale)) for index in plan.spec.pages or range(doc.page_count)
        ]
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
        case TableNode(rows=rows, spans=spans, header_rows=head_n) if rows:
            table = document.add_table(rows=len(rows), cols=len(rows[0]), style="Table Grid")
            for ri, row in enumerate(rows):
                for ci, cell in enumerate(row):
                    target = table.cell(ri, ci)
                    target.text, target.vertical_alignment = _text(cell), WD_CELL_VERTICAL_ALIGNMENT.CENTER
                    for run in (r for para in target.paragraphs for r in para.runs):
                        run.bold = ri < head_n
            for row, col, col_span, row_span in spans:  # `_Cell.merge` spans the rectangle; `grid_span` reads the horizontal span back
                table.cell(row, col).merge(table.cell(row + row_span - 1, col + col_span - 1))
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


def _docx_scaffold(document: object, spec: EmitSpec, /) -> None:
    # the caption style registers once, never per-run duplication.
    if "RasmCaption" not in {style.name for style in document.styles}:
        caption = document.styles.add_style("RasmCaption", WD_STYLE_TYPE.PARAGRAPH)
        caption.font.italic, caption.font.size = True, Pt(9)
    section = document.sections[0]
    if spec.landscape:
        section.orientation = WD_ORIENTATION.LANDSCAPE
        section.page_width, section.page_height = section.page_height, section.page_width
    section.left_margin = section.right_margin = Cm(2.0)
    if spec.header_text:
        section.header.paragraphs[0].text = spec.header_text
    if spec.footer_text:
        section.footer.paragraphs[0].text = spec.footer_text


def _docx_emit(plan: DocumentPlan, _world: "Compiler | None", /) -> EmitFact:
    document = docx.Document()
    props = document.core_properties
    props.title, props.author, props.subject = plan.spec.title or plan.node.meta.key.hex, plan.spec.author, plan.spec.subject
    _docx_scaffold(document, plan.spec)
    _docx_block(document, plan.node, plan.spec)
    merges = sum(len(t.spans) for t in walk(plan.node) if isinstance(t, TableNode))
    sink = io.BytesIO()
    document.save(sink)
    return EmitFact(data=sink.getvalue(), pages=len(document.sections), merges=merges)


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
                    template,
                    plan.spec.assets[asset_key.hex],
                    width=Pt(bounds[2]) if bounds[2] else None,
                    height=Pt(bounds[3]) if bounds[3] else None,
                    anchor=plan.spec.anchors.get(role),
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
        frame = slide.shapes.add_textbox(
            Inches(0.5), Inches(0.5), presentation.slide_width - Inches(1), presentation.slide_height - Inches(1)
        ).text_frame
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


def _odf_text[E](element: E, text: str, /) -> E:
    addTextToElement(element, text)  # owns the whitespace-correct `<text:s>`/`tab`/`line-break` split
    return element


def _odf_style(document: object, cache: dict[object, str], run: RunNode, /) -> str:
    # the key is the FULL run appearance including face/size/locale, so identical runs share one row while a face
    # difference keys its own — a face-blind key collapses distinct fonts onto one row; `NodeMeta.lang` (BCP-47)
    # splits into `fo:language`/`fo:country`.
    lang = run.meta.lang if isinstance(run.meta.lang, str) else ""
    language, _, country = lang.partition("-")
    key = (run.weight >= _BOLD_WEIGHT, run.italic, run.script, run.color, run.decorations, run.font_key, run.size, lang)
    if key not in cache:
        attrs: dict[str, object] = {"fontname": run.font_key, "fontsize": f"{run.size}pt"}
        if run.weight >= _BOLD_WEIGHT:
            attrs["fontweight"] = "bold"
        if run.italic:
            attrs["fontstyle"] = "italic"
        if run.script is RunScript.SUPER:
            attrs["textposition"] = "super 58%"
        elif run.script is RunScript.SUB:
            attrs["textposition"] = "sub 58%"
        if run.color != (0, 0, 0):
            attrs["color"] = "#%02X%02X%02X" % run.color
        if TextDecoration.UNDERLINE in run.decorations:
            attrs["textunderlinestyle"] = "solid"
        if TextDecoration.STRIKETHROUGH in run.decorations:
            attrs["textlinethroughstyle"] = "solid"
        if language:
            attrs["language"] = language
        if country:
            attrs["country"] = country
        name = f"R{len(cache)}"
        style = OdfStyle(name=name, family="text")
        style.addElement(TextProperties(**attrs))
        document.automaticstyles.addElement(style)
        cache[key] = name
    return cache[key]


def _odf_runs(paragraph: object, runs: tuple[RunNode, ...], document: object, cache: dict[object, str], /) -> object:
    for run in runs:
        paragraph.addElement(_odf_text(OdfSpan(stylename=_odf_style(document, cache, run)), run.text))
    return paragraph


def _odf_scaffold(document: object, spec: EmitSpec, key: str, /) -> None:
    # shared styles register once; the Dublin-Core seal is the OASIS descriptive-metadata surface.
    date_style = DateStyle(name="RasmDate", automaticorder="true")
    for token in (Year(), NumberText(text="-"), Month(), NumberText(text="-"), Day()):
        date_style.addElement(token)
    document.automaticstyles.addElement(date_style)
    rtl = OdfStyle(name="RasmRTL", family="paragraph")
    rtl.addElement(ParagraphProperties(writingmode="rl-tb"))
    document.automaticstyles.addElement(rtl)
    document.meta.addElement(dc.Title(text=spec.title or key))
    for element in (dc.Creator(text=spec.author) for _ in (spec.author,) if spec.author):
        document.meta.addElement(element)
    for element in (dc.Subject(text=spec.subject) for _ in (spec.subject,) if spec.subject):
        document.meta.addElement(element)


def _odf_sheet(document: object, table: TableNode, name: str, /) -> object:
    # a `CoveredTableCell` placeholder sits under every merged position.
    sheet = OdfTable(name=name)
    spans = {(r, c): (cs, rs) for r, c, cs, rs in table.spans}
    covered = {(r + dr, c + dc) for r, c, cs, rs in table.spans for dr in range(rs) for dc in range(cs) if (dr, dc) != (0, 0)}
    header, body_rows = TableHeaderRows(), []
    for ri, row in enumerate(table.rows):
        line = TableRow()
        for ci, cell in enumerate(row):
            if (ri, ci) in covered:
                line.addElement(CoveredTableCell())
                continue
            value_type, attrs, text = CellValue.of(cell).odf_cell()
            merge = {"numbercolumnsspanned": str(span[0]), "numberrowsspanned": str(span[1])} if (span := spans.get((ri, ci))) else {}
            styled = {"stylename": "RasmDate"} if value_type == "date" else {}
            typed = TableCell(valuetype=value_type, **attrs, **merge, **styled)
            typed.addElement(_odf_text(P(), text))
            line.addElement(typed)
        (header.addElement if ri < table.header_rows else body_rows.append)(line)
    if table.header_rows:
        sheet.addElement(header)
    for line in body_rows:
        sheet.addElement(line)
    return sheet


def _odf_block(document: object, body: object, node: DocumentNode, spec: EmitSpec, cache: dict[object, str], /) -> None:
    match node:
        case RunNode():
            body.addElement(_odf_runs(P(), (node,), document, cache))
        case BlockNode(block=BlockKind.HEADING, level=level, runs=runs):
            body.addElement(_odf_runs(H(outlinelevel=min(max(level, 1), 10)), runs, document, cache))
        case BlockNode(runs=runs, children=kids):
            style = "RasmRTL" if runs and runs[0].direction is TextDirection.RTL else None
            body.addElement(_odf_runs(P(stylename=style), runs, document, cache))
            for kid in kids:
                _odf_block(document, body, kid, spec, cache)
        case ListNode(items=items):
            listing = OdfList()
            for item in items:
                entry = OdfListItem()
                entry.addElement(_odf_runs(P(), tuple(r for r in walk(item) if isinstance(r, RunNode)), document, cache))
                listing.addElement(entry)
            body.addElement(listing)
        case FigureNode(asset_key=asset_key, intrinsic=intrinsic) if asset_key.hex in spec.assets:
            href = document.addPicture(spec.assets[asset_key.hex])
            width, height = intrinsic or (120.0, 90.0)
            frame = OdfFrame(width=f"{width}pt", height=f"{height}pt", anchortype="paragraph")
            frame.addElement(OdfImage(href=href))
            carrier = P()
            carrier.addElement(frame)
            body.addElement(carrier)
        case AnnotationNode(annot=AnnotKind.LINK, contents=text, link=Uri(href=href)):
            anchor = P()
            anchor.addElement(_odf_text(OdfLink(href=href, type="simple"), text))
            body.addElement(anchor)
        case AnnotationNode(annot=AnnotKind.NOTE, contents=text):
            note = Note(noteclass="footnote")
            note.addElement(NoteCitation(text="*"))
            note_body = NoteBody()
            note_body.addElement(_odf_text(P(), text))
            note.addElement(note_body)
            carrier = P()
            carrier.addElement(note)
            body.addElement(carrier)
        case SectionNode(level=level, heading=head, children=kids):
            body.addElement(_odf_runs(H(outlinelevel=min(max(level, 1), 10)), head, document, cache))
            for kid in kids:
                _odf_block(document, body, kid, spec, cache)
        case TableNode() as table:
            # the `table:name` is unique per document (ODF rejects duplicate names), so it keys off the
            # table's own content key rather than a constant every table would collide on.
            body.addElement(_odf_sheet(document, table, f"T{table.meta.key.hex[:8]}"))
        case PageNode(children=kids):
            for kid in kids:
                _odf_block(document, body, kid, spec, cache)
        case _:
            pass


def _odf_emit(plan: DocumentPlan, _world: "Compiler | None", /) -> EmitFact:
    document = OpenDocumentSpreadsheet() if plan.mode is DocumentMode.ODS else OpenDocumentText()
    _odf_scaffold(document, plan.spec, plan.node.meta.key.hex)
    if plan.mode is DocumentMode.ODS:
        # each `table:table-name` must be unique (Calc rejects duplicate sheet names), so a multi-table
        # QTO/schedule workbook indexes the base name per sheet; a lone table keeps the exact bound name.
        tables = [n for n in walk(plan.node) if isinstance(n, TableNode)]
        for index, table in enumerate(tables):
            name = (plan.spec.sheet or "Sheet1") if len(tables) == 1 else f"{plan.spec.sheet or 'Sheet'}{index + 1}"
            document.spreadsheet.addElement(_odf_sheet(document, table, name))
    else:
        _odf_block(document, document.text, plan.node, plan.spec, {})
    sink = io.BytesIO()
    document.write(sink)  # whole ODF zip (content/styles/meta/manifest) to the binary stream, never a temp file
    merges = sum(len(t.spans) for t in walk(plan.node) if isinstance(t, TableNode))
    footnotes = sum(1 for n in walk(plan.node) if isinstance(n, AnnotationNode) and n.annot is AnnotKind.NOTE)
    return EmitFact(data=sink.getvalue(), merges=merges, footnotes=footnotes)


def _xlsx_emit(plan: DocumentPlan, _world: "Compiler | None", /) -> EmitFact:
    grid = [tuple(CellValue.of(cell) for cell in row) for table in walk(plan.node) if isinstance(table, TableNode) for row in table.rows]
    return _xlsx_write(plan, grid, SpreadsheetPolicy.select(plan, len(grid)).regime)


def _xlsx_merge(node: DocumentNode, sheet: object, header_fmt: object, /) -> int:
    # the top-left cell keeps its display value, the covered cells blank — the non-streaming regime only.
    offset, count = 0, 0
    for table in (n for n in walk(node) if isinstance(n, TableNode)):
        for row, col, col_span, row_span in table.spans:
            if col_span > 1 or row_span > 1:
                top = table.rows[row][col] if row < len(table.rows) and col < len(table.rows[row]) else None
                sheet.merge_range(
                    offset + row, col, offset + row + row_span - 1, col + col_span - 1, _text(top) if top is not None else "", header_fmt
                )
                count += 1
        offset += len(table.rows)
    return count


def _xlsx_write(plan: DocumentPlan, grid: list[tuple[CellValue, ...]], regime: XlsxRegime, /) -> EmitFact:
    # the stream and in-memory passes are a policy value, not two writers; the in-memory arm gains the structured
    # features the streaming flush cannot back-patch (`add_table`/`conditional_format`/`merge_range`).
    streamed = regime is XlsxRegime.STREAMED
    sink = io.BytesIO()
    book = xlsxwriter.Workbook(
        sink,
        {
            "constant_memory": streamed,
            "in_memory": plan.spec.in_memory,
            "use_zip64": len(grid) >= _ZIP64_ROW_THRESHOLD,
            "remove_timezone": True,
            "nan_inf_to_errors": True,  # `_coerce_cell` admits tz-aware datetimes and `float('inf')`
        },
    )
    book.set_properties({"title": plan.spec.title or plan.node.meta.key.hex, "author": plan.spec.author})
    sheet = book.add_worksheet(plan.spec.sheet or None)
    header_fmt, headed = book.add_format(dict(_FORMATS["header"])), plan.spec.header
    formats = frozendict({kind: book.add_format(dict(_FORMATS[kind])) for kind in ("number", "datetime")})
    width, last = max((len(row) for row in grid), default=0), len(grid) - 1
    if width:
        sheet.set_column(0, width - 1, plan.spec.column_width)
    if plan.spec.landscape:
        sheet.set_landscape()
    if plan.spec.header_text or plan.spec.footer_text:
        sheet.set_header(f"&L{plan.spec.header_text}&R&P")
        sheet.set_footer(f"&L{plan.spec.footer_text}&R&D")
    for index, row in enumerate(grid):
        sheet.set_row(index, None, header_fmt if index == 0 and headed and streamed else None)  # row format before its cells under constant_memory
        for column, value in enumerate(row):
            value.write_xlsxwriter(sheet, index, column, formats)
    merges = 0
    if not streamed and grid and width:
        merges = _xlsx_merge(plan.node, sheet, header_fmt)
        sheet.autofilter(0, 0, last, width - 1) if merges else sheet.add_table(
            0, 0, last, width - 1, {"header_row": headed, "autofilter": True, "banded_rows": True}
        )
        sheet.conditional_format(1, 0, last, width - 1, {"type": "data_bar"})
    elif headed and grid and width:
        sheet.freeze_panes(1, 0)
        sheet.autofilter(0, 0, last, width - 1)
    book.close()
    return EmitFact(data=sink.getvalue(), pages=len(grid), merges=merges)


def _ruamel_emit(plan: DocumentPlan, _world: "Compiler | None", /) -> EmitFact:
    engine, sink = YAML(), io.BytesIO()
    engine.dump(to_builtins(plan.node), sink)
    return EmitFact(data=sink.getvalue())


def _tomlkit_emit(plan: DocumentPlan, _world: "Compiler | None", /) -> EmitFact:
    return EmitFact(data=tomlkit.dumps(to_builtins(plan.node)).encode())


def _markdown_emit(plan: DocumentPlan, _world: "Compiler | None", /) -> EmitFact:
    return EmitFact(data=to_markdown(plan.node).encode())  # the CommonMark/GFM plain-text manuscript egress of the same bound tree


def _latex_emit(plan: DocumentPlan, _world: "Compiler | None", /) -> EmitFact:
    return EmitFact(data=to_latex(plan.node).encode())  # the journal-submission LaTeX manuscript egress of the same bound tree


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

_SPREADSHEET_POLICIES: Final[Map[XlsxRegime, SpreadsheetPolicy]] = Map.of_seq([
    (XlsxRegime.IN_MEMORY, SpreadsheetPolicy(XlsxRegime.IN_MEMORY, _STREAMING_ROW_THRESHOLD)),
    (XlsxRegime.STREAMED, SpreadsheetPolicy(XlsxRegime.STREAMED, 1 << 62)),
])

_HELD_WORLD: Final[frozenset[DocumentMode]] = frozenset({DocumentMode.PDF_TYPST, DocumentMode.TYPST_DATA})

_RECEIPT: Final[Map[ReceiptKind, Callable[[ContentKey, EmitFact], "ArtifactReceipt"]]] = Map.of_seq([
    (ReceiptKind.PDF, lambda key, fact: ArtifactReceipt.Pdf(key, len(fact.data), fact.pages)),
    (ReceiptKind.OFFICE, lambda key, fact: ArtifactReceipt.Office(key, len(fact.data))),
])

BACKENDS: Final[Map[DocumentMode, Backend]] = Map.of_seq([
    (DocumentMode.PDF_AUTHOR, Backend(Band.CORE, _reportlab_author, ReceiptKind.PDF)),
    (DocumentMode.PDF_HTML, Backend(Band.CORE, _weasyprint_html, ReceiptKind.PDF)),
    (DocumentMode.PDF_OXIDE, Backend(Band.CORE, _pdf_oxide_create, ReceiptKind.PDF)),  # pdf_oxide Rust core is GIL-releasing -> CORE thread lane
    (DocumentMode.PDF_UA, Backend(Band.CORE, _pdf_ua_build, ReceiptKind.PDF)),
    (DocumentMode.PDF_TYPST, Backend(Band.CORE, _typst_compile, ReceiptKind.PDF)),
    (DocumentMode.TYPST_QUERY, Backend(Band.CORE, _typst_query, ReceiptKind.OFFICE)),
    (DocumentMode.TYPST_EVAL, Backend(Band.CORE, _typst_eval, ReceiptKind.OFFICE)),
    (DocumentMode.TYPST_DATA, Backend(Band.CORE, _typst_compile, ReceiptKind.PDF)),
    (DocumentMode.PDF_RENDER, Backend(
        Band.CORE, _pymupdf_render, ReceiptKind.PDF
    )),  # AGPL: native-codec encoded raster; the commercial-safe path rides pdf_oxide `render_page` instead
    (DocumentMode.PDF_RASTER, Backend(
        Band.CORE, _pypdfium2_raster, ReceiptKind.PDF
    )),  # Apache/BSD: the commercial-safe Pillow-free numpy-frame-stack raster axis
    (DocumentMode.PDF_ASSEMBLE, Backend(Band.CORE, _pypdf_assemble, ReceiptKind.PDF)),
    (DocumentMode.PDF_REPAIR, Backend(Band.WORKER, _pikepdf_repair, ReceiptKind.PDF)),
    (DocumentMode.FONT_EMBED, Backend(Band.CORE, _font_embed, ReceiptKind.PDF)),
    (DocumentMode.DOCX, Backend(Band.CORE, _docx_emit, ReceiptKind.OFFICE)),
    (DocumentMode.DOCX_TEMPLATE, Backend(Band.CORE, _docxtpl_emit, ReceiptKind.OFFICE)),
    (DocumentMode.PPTX, Backend(Band.WORKER, _pptx_emit, ReceiptKind.OFFICE)),  # python-pptx -> lxml
    (DocumentMode.XLSX, Backend(Band.CORE, _xlsx_emit, ReceiptKind.OFFICE)),
    (DocumentMode.ODT, Backend(Band.CORE, _odf_emit, ReceiptKind.OFFICE)),  # odfpy is pure-Python (defusedxml), no lxml -> CORE
    (DocumentMode.ODS, Backend(Band.CORE, _odf_emit, ReceiptKind.OFFICE)),
    (DocumentMode.XML, Backend(Band.WORKER, _lxml_emit, ReceiptKind.OFFICE)),
    (DocumentMode.XML_TRANSFORM, Backend(Band.WORKER, _lxml_transform, ReceiptKind.OFFICE)),
    (DocumentMode.XML_VALIDATE, Backend(Band.WORKER, _lxml_validate, ReceiptKind.OFFICE)),
    (DocumentMode.XML_QUERY, Backend(Band.WORKER, _lxml_query, ReceiptKind.OFFICE)),
    (DocumentMode.YAML, Backend(Band.CORE, _ruamel_emit, ReceiptKind.OFFICE)),
    (DocumentMode.TOML, Backend(Band.CORE, _tomlkit_emit, ReceiptKind.OFFICE)),
    (DocumentMode.MARKDOWN, Backend(
        Band.CORE, _markdown_emit, ReceiptKind.OFFICE
    )),  # pure-Python `to_markdown` node fold — no external input, so no `_REQUIRED` row
    (DocumentMode.LATEX, Backend(
        Band.CORE, _latex_emit, ReceiptKind.OFFICE
    )),  # pure-Python `to_latex` node fold — no external input, so no `_REQUIRED` row
])
```

## [03]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

- [WEASYPRINT_LICENSE]-[OPEN]: is weasyprint BSD-3 or AGPL — the permissive-lane framing of the pdf_oxide create sibling depends on it; verify against the installed distribution license metadata.

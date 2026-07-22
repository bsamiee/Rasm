# [PY_ARTIFACTS_EMIT]

`DocumentPlan` owns the document-emission axis over the single `DocumentNode` semantic tree. `DocumentMode` discriminates over the `BACKENDS` policy table, and every arm folds FROM `document/model#NODE` — PDF authoring, archival HTML-CSS, tagged PDF/UA, Typst typesetting, raster, assembly, repair, OOXML and OpenDocument office, structured text, and Markdown/LaTeX manuscript egress — never an opaque payload. Emission lowers FROM the tree and `document/lens#LENS` recovers TO it, so production and extraction are inverses over one node algebra.

Each `Backend` row binds its arm to its runtime `Band`, so the runtime/worker split is a row column rather than a second dispatch surface: a `CORE`-band arm crosses as a `KernelTrait.RELEASING` kernel, a `WORKER`-band arm as `KernelTrait.HOSTILE` onto the warm process pool under its trait-row worker-death retry, and both hand the SAME `_dispatched` row-resolver to the caller-threaded `lane: LanePolicy` offload seam the runtime owns — the worker lane carries no second `match`. `EmitSpec` admits exactly once at `DocumentPlan.of` through the closed `EmitPayload` `TypedDict` under the per-mode `_REQUIRED` precondition AND the per-mode `_SCOPE` admissibility set — a payload key the selected mode cannot observe is the `EmitFault.foreign` refusal, `bound` slices one validated payload per mode so a key rides exactly the modes that observe it and refuses only a key NO selected mode observes, and `_key` hashes only the mode-scoped spec slice, so plan identity is invariant under data no backend reads; arm-level provider raises convert to the runtime `BoundaryFault` at the `async_boundary` capsule. `@receipted(OPEN)` drains `contribute` off the stepped owner; `core/plan#PLAN` schedules the node set and `core/issue#ISSUE` constructs it; `exchange/detect#DETECT` format-identifies a `template` payload before any engine trusts its extension.

## [01]-[INDEX]

- [01]-[DOCUMENT]: document-mode dispatch over the band-bound `BACKENDS` policy table, every arm lowering from the `DocumentNode` tree.

## [02]-[DOCUMENT]

- Entry: the key mints PRE-RUN over the canonical `(mode, node, spec)` input — `receipt.slot == node.key`, the produced output content-address riding the receipt FACTS, never the elision key. `bound(node, modes, **payload)` is the one-context-many-formats fan: one validated payload binds one `DocumentNode` context to N format nodes with per-mode keys and per-mode payload slices, so a re-issued package re-renders only changed formats — never a per-format vocabulary re-spelling `DocumentMode` and never a second scheduling rail. `world(title=)` is the one Typst `Compiler` mint — compile, query, and eval arms share it, so the tree lowers to Typst source exactly once per emission.
- Auto: `_run_markup` carries each `RunNode`'s real weight/italic/colour/baseline/decoration with `<`/`&` escaped once before any tag — a plain `run.text` splice drops run fidelity, and the docx/pptx/odf office arms carry the same per-run fidelity into list items and slide runs rather than flattening to bare text; the `A_3A` weasyprint variant embeds its PDF/A-3 source files through `attachments`, never only naming them; each `PageNode` opens its own tagged sheet through `new_page_same_size`, so a multi-page UA document never overflows one buffered page, `FormulaNode` and footnote `NOTE` annotations land in the UA walk (`page.paragraph` over the `/Alt` equivalent, `page.footnote`), a spanned grid rides the `streaming_table(repeat_header=, max_rowspan=)` row surface, and the UA and slide walks prune below `TableNode` so cell content never doubles as loose paragraphs; the manuscript rows escape through the model owner's `_MD_ESCAPE`/`_LATEX_ESCAPE` maps so no active character breaks the source. A born-archival or born-tagged render self-verifies: `_conformance` re-opens the emitted bytes through the `pdf_oxide` oracle (`validate_pdf_a(level)`/`validate_pdf_ua()`) and folds `valid`/`errors` onto the fact — emit-side evidence of the claimed variant, never the sealed verdict `exchange/conformance#CONFORMANCE` owns. `_pypdfium2_raster` copies each borrowed `to_numpy` view into the `EmitFact.data` byte stream before closing its `PdfBitmap`, then closes the page and document leaf-first; `frame_shapes` preserves heterogeneous page boundaries without `np.stack` shape assumptions.
- Receipt: `contribute` reads the threaded `EmitFact` off `self.fact` — never an in-process re-run of a worker-gated arm — and folds the case off the `Backend.kind` discriminant: emit mints only the `ArtifactReceipt.Pdf`/`.Office` arities the receipt owner declares; the typography-rail `Document` case is not its to mint. Rich per-arm evidence (render scale, outline count, embedded-face set, undeclared-variable set, validation verdict) rides the `EmitFact` carrier, never a widened receipt tuple.
- Growth: a new document format is one `DocumentMode` row plus one `Backend` row binding its arm and band, one `_SCOPE` row naming its observable payload fields, plus a `_REQUIRED` row when it demands an input; a new mode-specific control is one `EmitSpec` field plus its `_SCOPE` membership; a new typed cell is one `CellValue` arm; a new interactive-field payload is one `FieldValue` case plus one `_ua_field` arm; a new archival profile is one `PdfVariant` row projecting through `_PDF_STANDARD`/`_PDF_PROFILE`/`_ACCESSIBLE` to both engines; a new evidence fact is one `EmitFact` field; a new admission cause is one `EmitFault` case.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
import io
import re
from collections.abc import Callable, Iterable, Iterator
from copy import replace
from datetime import date, datetime
from enum import StrEnum
from pathlib import Path
from typing import Annotated, Final, Literal, Never, NotRequired, ReadOnly, Self, TypedDict, Unpack, assert_never

import msgspec
from expression import Error, Ok, Result, case, tag, tagged_union
from expression.collections import Block, Map
from expression.extra.result import sequence
from builtins import frozendict
from msgspec import Struct, field, to_builtins
from pydantic import Field, TypeAdapter, ValidationError

from rasm.runtime.identity import ContentIdentity, ContentKey
from rasm.runtime.faults import BoundaryFault, RuntimeRail, async_boundary
from rasm.runtime.lanes import LanePolicy
from rasm.runtime.receipts import OPEN, Receipt, receipted
from rasm.runtime.workers import Kernel, KernelTrait

from rasm.artifacts.core.plan import Admission, ArtifactWork
from rasm.artifacts.core.receipt import ArtifactReceipt
from rasm.artifacts.exchange.detect import Detect, MediaClass, Source
from rasm.artifacts.document.model import (
    AnnotationNode,
    AnnotKind,
    BlockKind,
    BlockNode,
    ButtonField,
    CheckboxField,
    ComboField,
    DocumentNode,
    FieldNode,
    FigureNode,
    ForeignRole,
    FormulaNode,
    ListField,
    ListKind,
    ListNode,
    PageNode,
    RadioField,
    RunNode,
    RunScript,
    SectionNode,
    SignatureField,
    StructureNode,
    TableNode,
    TextDecoration,
    TextDirection,
    TextField,
    Uri,
    field_text,
    node_digest,
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
lazy import pdf_oxide
lazy import pikepdf
lazy import pymupdf
lazy import pypdfium2
lazy import tomlkit
lazy import xlsxwriter
lazy from docx.enum.section import WD_ORIENTATION
lazy from docx.enum.style import WD_STYLE_TYPE
lazy from docx.enum.table import WD_CELL_VERTICAL_ALIGNMENT
lazy from docx.shared import Cm, Pt, RGBColor
lazy from docxtpl import DocxTemplate, InlineImage, Listing, RichText, RichTextParagraph
lazy from lxml import etree
lazy from lxml import isoschematron
lazy from odf import dc
lazy from odf.draw import Frame as OdfFrame, Image as OdfImage
lazy from odf.number import DateStyle, Day, Month, Text as NumberText, Year
lazy from odf.opendocument import OpenDocumentSpreadsheet, OpenDocumentText
lazy from odf.style import ParagraphProperties, Style as OdfStyle, TextProperties
lazy from odf.table import CoveredTableCell, Table as OdfTable, TableCell, TableHeaderRows, TableRow
lazy from odf.teletype import addTextToElement
lazy from odf.text import A as OdfLink, H, List as OdfList, ListItem as OdfListItem, Note, NoteBody, NoteCitation, P, Span as OdfSpan
lazy from pdf_oxide import Column as OxideColumn, DocumentBuilder, EmbeddedFont, Pdf
lazy from pptx import Presentation
lazy from pptx.dml.color import RGBColor as SlideColor
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

# --- [TYPES] ----------------------------------------------------------------------------


class DocumentMode(StrEnum):
    PDF_AUTHOR = "pdf-author"
    PDF_HTML = "pdf-html"
    PDF_OXIDE = "pdf-oxide"
    PDF_UA = "pdf-ua"
    PDF_TYPST = "pdf-typst"
    TYPST_QUERY = "typst-query"
    TYPST_EVAL = "typst-eval"
    TYPST_DATA = "typst-data"
    PDF_RENDER = "pdf-render"
    PDF_RASTER = "pdf-raster"
    PDF_OXIDE_RENDER = "pdf-oxide-render"
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
    MARKDOWN = "markdown"
    LATEX = "latex"


class Band(StrEnum):
    CORE = "core"
    WORKER = "worker"


class ReceiptKind(StrEnum):
    PDF = "pdf"
    OFFICE = "office"  # workbook/slide/word-processing CONTAINERS alone — the ArtifactReceipt.Office definition
    DOCUMENT = "document"  # structured-data, introspection, and manuscript outputs — the generic non-PDF document rail


class SchemaKind(StrEnum):
    XSD = "xsd"
    RELAXNG = "relaxng"
    SCHEMATRON = "schematron"
    DTD = "dtd"


class RasterFormat(StrEnum):
    PNG = "png"
    JPEG = "jpeg"


class XlsxRegime(StrEnum):
    IN_MEMORY = "in-memory"
    STREAMED = "streamed"


class PdfVariant(StrEnum):
    NONE = "none"
    A_1B = "a-1b"
    A_2B = "a-2b"
    A_3B = "a-3b"
    A_2A = "a-2a"
    A_3A = "a-3a"
    A_4 = "a-4"
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


type Arm = Callable[["DocumentPlan"], "EmitFact"]
type OutlineRow = tuple[int, str, int]
type CellScalar = str | int | float | bool | datetime | date

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
_MARKUP_ESCAPE: Final = str.maketrans({"&": "&amp;", "<": "&lt;", ">": "&gt;", '"': "&quot;"})
_PDF_STANDARD: Final[Map[PdfVariant, tuple[str, ...]]] = Map.of_seq([
    (PdfVariant.A_1B, ("a-1b",)),
    (PdfVariant.A_2B, ("a-2b",)),
    (PdfVariant.A_3B, ("a-3b",)),
    (PdfVariant.A_4, ("a-4",)),
    (PdfVariant.A_2A, ("a-2a", "ua-1")),
    (PdfVariant.A_3A, ("a-3a", "ua-1")),
    (PdfVariant.UA_1, ("ua-1",)),
])
_PDF_PROFILE: Final[Map[PdfVariant, str]] = Map.of_seq([
    (PdfVariant.A_1B, "pdf/a-1b"),
    (PdfVariant.A_2B, "pdf/a-2b"),
    (PdfVariant.A_3B, "pdf/a-3b"),
    (PdfVariant.A_4, "pdf/a-4"),
    (PdfVariant.A_2A, "pdf/a-2b"),
    (PdfVariant.A_3A, "pdf/a-3b"),
    (PdfVariant.UA_1, "pdf/ua-1"),
])
_ACCESSIBLE: Final[frozenset[PdfVariant]] = frozenset({PdfVariant.A_2A, PdfVariant.A_3A, PdfVariant.UA_1})
_ORACLE_LEVEL: Final[Map[PdfVariant, str]] = Map.of_seq([
    (PdfVariant.A_1B, "1b"),
    (PdfVariant.A_2B, "2b"),
    (PdfVariant.A_3B, "3b"),
    (PdfVariant.A_2A, "2a"),
    (PdfVariant.A_3A, "3a"),
    (PdfVariant.A_4, "4"),
])
_PLAN_ENCODER: Final = msgspec.msgpack.Encoder(order="deterministic")
_FORMATS: Final[Map[str, frozendict[str, object]]] = Map.of_seq([
    ("header", frozendict({"bold": True, "align": "center"})),
    ("number", frozendict({"num_format": _NUM_FORMAT})),
    ("datetime", frozendict({"num_format": _DATE_FORMAT})),
])

# --- [ERRORS] ---------------------------------------------------------------------------


@tagged_union(frozen=True)
class EmitFault:
    tag: Literal["payload", "unsatisfied", "foreign"] = tag()
    payload: tuple[str, ...] = case()
    unsatisfied: tuple[DocumentMode, str] = case()
    foreign: tuple[tuple[DocumentMode, ...], tuple[str, ...]] = case()


# --- [MODELS] ---------------------------------------------------------------------------


class CellValue(Struct, frozen=True):
    raw: CellScalar | None

    @staticmethod
    def of(cell: DocumentNode) -> "CellValue":
        match cell:
            case FieldNode(field=TextField(value=value)):
                return CellValue(value)
            case FieldNode(field=CheckboxField(checked=checked)):
                return CellValue(checked)
            case FieldNode(field=RadioField(selected=selected)) | FieldNode(field=ComboField(selected=selected)):
                return CellValue(selected)
            case FieldNode(field=ListField(selected=selected)):
                return CellValue(", ".join(selected))
            case FieldNode(field=SignatureField(signer=signer, reason=reason)):
                return CellValue(signer or reason)
            case FieldNode(field=ButtonField(label=label)):
                return CellValue(label)
            case _:
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


class EmitFact(Struct, frozen=True):
    data: bytes
    frame_shapes: tuple[tuple[int, ...], ...] = ()
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
    tagged: bool = False
    structure: int = 0
    merges: int = 0
    footnotes: int = 0


class EmitSpec(Struct, frozen=True, omit_defaults=True):
    parents: tuple[ContentKey, ...] = ()
    source: bytes = b""
    password: str = ""
    title: str = ""
    author: str = ""
    subject: str = ""
    toc: bool = False
    header_text: str = ""
    footer_text: str = ""
    landscape: bool = False
    variant: PdfVariant = PdfVariant.NONE
    forms: bool = False
    output_intent: str = ""
    base_url: str = ""
    stylesheets: tuple[str, ...] = ()
    attachments: frozendict[str, str] = field(default_factory=frozendict)
    full_fonts: bool = False
    optimize_images: bool = False
    presentational_hints: bool = False
    selector: str = ""
    field_name: str = ""
    one: bool = False
    expression: str = ""
    sys_inputs: frozendict[str, str] = field(default_factory=frozendict)
    timestamp: int = 0
    image_format: RasterFormat = RasterFormat.PNG
    pages: tuple[int, ...] = ()
    scale: float = _RASTER_SCALE
    rotation: int = 0
    subset_fonts: frozendict[str, bytes] = field(default_factory=frozendict)
    subfont_index: int = 0
    template: str = ""
    assets: frozendict[str, str] = field(default_factory=frozendict)
    anchors: frozendict[str, str] = field(default_factory=frozendict)
    links: frozendict[str, str] = field(default_factory=frozendict)
    replace: frozendict[str, str] = field(default_factory=frozendict)
    sheet: str = ""
    header: bool = True
    column_width: float = _HEADER_WIDTH
    spreadsheet: XlsxRegime | None = None
    stylesheet: bytes = b""
    xslt_params: frozendict[str, str] = field(default_factory=frozendict)
    schema: bytes = b""
    schema_kind: SchemaKind = SchemaKind.XSD
    path: str = ""
    namespaces: frozendict[str, str] = field(default_factory=frozendict)
    pretty: bool = False

    @property
    def render_keywords(self) -> frozendict[str, object]:
        return frozendict({"scale": self.scale, "rotation": self.rotation, "may_draw_forms": self.forms})


# --- [BOUNDARIES] -----------------------------------------------------------------------


class EmitPayload(TypedDict, closed=True):
    parents: NotRequired[ReadOnly[tuple[ContentKey, ...]]]
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
    image_format: NotRequired[ReadOnly[RasterFormat]]
    pages: NotRequired[ReadOnly[tuple[Annotated[int, Field(ge=0)], ...]]]
    scale: NotRequired[ReadOnly[Annotated[float, Field(gt=0, allow_inf_nan=False)]]]
    rotation: NotRequired[ReadOnly[int]]
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
    stylesheet: NotRequired[ReadOnly[bytes]]
    xslt_params: NotRequired[ReadOnly[frozendict[str, str]]]
    schema: NotRequired[ReadOnly[bytes]]
    schema_kind: NotRequired[ReadOnly[SchemaKind]]
    path: NotRequired[ReadOnly[str]]
    namespaces: NotRequired[ReadOnly[frozendict[str, str]]]
    pretty: NotRequired[ReadOnly[bool]]


_PAYLOAD: Final = TypeAdapter(EmitPayload)
_REQUIRED: Final[Map[DocumentMode, tuple[str, ...]]] = Map.of_seq([
    (DocumentMode.PDF_RENDER, ("source",)),
    (DocumentMode.PDF_RASTER, ("source",)),
    (DocumentMode.PDF_OXIDE_RENDER, ("source",)),
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
_ENVELOPE: Final[frozenset[str]] = frozenset({"parents"})
_META: Final[frozenset[str]] = frozenset({"title", "author", "subject"})
_AUTHOR_META: Final[frozenset[str]] = frozenset({"title", "author"})
_TITLE_META: Final[frozenset[str]] = frozenset({"title"})
_PAGE_FURNITURE: Final[frozenset[str]] = frozenset({"header_text", "footer_text", "landscape"})
_REPORT_FURNITURE: Final[frozenset[str]] = _PAGE_FURNITURE | {"toc"}
_SOURCE: Final[frozenset[str]] = frozenset({"source", "password"})
_RASTER_KNOBS: Final[frozenset[str]] = frozenset({"pages", "scale", "rotation", "forms"})
_ARCHIVAL: Final[frozenset[str]] = frozenset(
    {"variant", "forms", "output_intent", "base_url", "stylesheets", "attachments", "full_fonts", "optimize_images", "presentational_hints"}
)
_TEMPLATE_KNOBS: Final[frozenset[str]] = frozenset({"template", "assets", "anchors", "links", "replace"})
_GRID: Final[frozenset[str]] = frozenset({"sheet", "header", "column_width", "spreadsheet"})
_SCOPE: Final[Map[DocumentMode, frozenset[str]]] = Map.of_seq([
    (DocumentMode.PDF_AUTHOR, _META | _REPORT_FURNITURE | {"assets"}),
    (DocumentMode.PDF_HTML, _ARCHIVAL | _AUTHOR_META),
    (DocumentMode.PDF_OXIDE, _AUTHOR_META),
    (DocumentMode.PDF_UA, _AUTHOR_META | {"assets", "subset_fonts"}),
    (DocumentMode.PDF_TYPST, _TITLE_META | {"variant", "sys_inputs", "timestamp"}),
    (DocumentMode.TYPST_QUERY, frozenset({"selector", "field_name", "one", "sys_inputs"})),
    (DocumentMode.TYPST_EVAL, frozenset({"expression", "sys_inputs"})),
    (DocumentMode.TYPST_DATA, _TITLE_META | {"variant", "sys_inputs", "timestamp"}),
    (DocumentMode.PDF_RENDER, _SOURCE | frozenset({"image_format", "pages", "scale"})),
    (DocumentMode.PDF_RASTER, _SOURCE | _RASTER_KNOBS),
    (DocumentMode.PDF_OXIDE_RENDER, _SOURCE | frozenset({"image_format", "pages", "scale"})),
    (DocumentMode.PDF_ASSEMBLE, _SOURCE | _META),
    (DocumentMode.PDF_REPAIR, _SOURCE),
    (DocumentMode.FONT_EMBED, _META | {"subset_fonts", "subfont_index"}),
    (DocumentMode.DOCX, _META | _PAGE_FURNITURE | {"assets"}),
    (DocumentMode.DOCX_TEMPLATE, _TEMPLATE_KNOBS),
    (DocumentMode.PPTX, _META | {"assets"}),
    (DocumentMode.XLSX, _GRID | _PAGE_FURNITURE | _META),
    (DocumentMode.ODT, _META | {"assets"}),
    (DocumentMode.ODS, _META | {"sheet"}),
    (DocumentMode.XML, frozenset({"pretty"})),
    (DocumentMode.XML_TRANSFORM, frozenset({"stylesheet", "xslt_params"})),
    (DocumentMode.XML_VALIDATE, frozenset({"schema", "schema_kind"})),
    (DocumentMode.XML_QUERY, frozenset({"path", "namespaces"})),
    (DocumentMode.YAML, frozenset()),
    (DocumentMode.TOML, frozenset()),
    (DocumentMode.MARKDOWN, frozenset()),
    (DocumentMode.LATEX, frozenset()),
])


# --- [SERVICES] -------------------------------------------------------------------------


class Backend(Struct, frozen=True):
    band: Band
    arm: Arm
    kind: ReceiptKind


class DocumentPlan(Struct, frozen=True):
    # `lane` arrives projected via LanePolicy.of(context) at the composition root — a capacity literal has no owner.
    mode: DocumentMode
    node: DocumentNode
    lane: LanePolicy
    spec: EmitSpec = field(default_factory=EmitSpec)
    fact: EmitFact | None = None

    def world(self, *, title: str | None = None) -> "Compiler":
        return Compiler(to_typst_source(self.node, title=title).encode(), font_paths=[], sys_inputs=dict(self.spec.sys_inputs))

    def emit(self, /) -> ArtifactWork:
        return ArtifactWork(key=self._key, work=self._emit, parents=self.spec.parents, admission=Admission(keyed=None), cost=1.0)

    @property
    def _key(self) -> ContentKey:
        # `node_digest` joins the tree onto the preimage through the runtime merkle fold — msgpack cannot
        # integer-encode the live u128 `ContentKey` leaves a `DocumentNode` carries.
        spec = ContentIdentity.key(self.mode.value, _PLAN_ENCODER.encode(self._scoped()))
        return ContentIdentity.key(self.mode.value, (spec, node_digest(self.node)))

    def _scoped(self) -> EmitSpec:
        return EmitSpec(**{name: getattr(self.spec, name) for name in _admissible(self.mode)})

    async def _emit(self, /) -> RuntimeRail[ArtifactReceipt]:
        crossed = await async_boundary(f"emit.{self.mode}", self._stepped)
        return crossed.map(lambda live: _RECEIPT[BACKENDS[self.mode].kind](self._key, live.fact))

    @receipted(OPEN)
    async def _stepped(self, /) -> Self:
        gate: RuntimeRail[Self] = (
            (await Detect(lane=self.lane).of(Source.File(Path(self.spec.template))))
            .bind(
                lambda identity: Ok(self)
                if identity.media_class is MediaClass.WORD
                else Error(BoundaryFault(config=("artifacts.emit.template", identity.mime)))
            )
            if self.mode is DocumentMode.DOCX_TEMPLATE
            else Ok(self)
        )
        admitted = gate.default_with(_emit_raise)
        fact = (
            await admitted.lane.offload(Kernel.of(_dispatched, KernelTrait.HOSTILE), admitted)
            if BACKENDS[admitted.mode].band is Band.WORKER
            else await admitted.lane.offload(Kernel.of(_dispatched, KernelTrait.RELEASING), admitted)
        )
        return replace(admitted, fact=fact.default_with(_emit_raise))

    def contribute(self) -> Iterable[Receipt]:
        if (fact := self.fact) is None:
            return
        yield from _RECEIPT[BACKENDS[self.mode].kind](self._key, fact).contribute()

    @classmethod
    def _planned(cls, mode: DocumentMode, node: DocumentNode, lane: LanePolicy, payload: EmitPayload, /) -> Result[Self, EmitFault]:
        if foreign := tuple(sorted(set(payload) - _admissible(mode))):
            return Error(EmitFault(foreign=((mode,), foreign)))
        spec = EmitSpec(**payload)
        missing = next((name for name in _REQUIRED.try_find(mode).default_value(()) if not getattr(spec, name)), None)
        return Error(EmitFault(unsatisfied=(mode, missing))) if missing else Ok(cls(mode=mode, node=node, lane=lane, spec=spec))

    @classmethod
    def of(cls, mode: DocumentMode, node: DocumentNode, /, *, lane: LanePolicy, **raw: Unpack[EmitPayload]) -> Result[Self, EmitFault]:
        return _validated(raw).bind(lambda payload: cls._planned(mode, node, lane, payload))

    @classmethod
    def bound(
        cls, node: DocumentNode, modes: "Iterable[DocumentMode]", /, *, lane: LanePolicy, **raw: Unpack[EmitPayload]
    ) -> Result[tuple[ArtifactWork, ...], EmitFault]:
        selected = tuple(modes)

        def fanned(payload: EmitPayload, /) -> Result[tuple[ArtifactWork, ...], EmitFault]:
            observable = frozenset(_ENVELOPE).union(*(_admissible(mode) for mode in selected))
            if foreign := tuple(sorted(set(payload) - observable)):
                return Error(EmitFault(foreign=(selected, foreign)))
            sliced = (
                cls._planned(mode, node, lane, {name: value for name, value in payload.items() if name in _admissible(mode)}) for mode in selected
            )
            return sequence(Block.of_seq(sliced)).map(lambda plans: tuple(plan.emit() for plan in plans))

        return _validated(raw).bind(fanned)


# --- [OPERATIONS] -----------------------------------------------------------------------


def _emit_raise(fault: object) -> Never:
    raise ValueError(str(fault))


def _admissible(mode: DocumentMode, /) -> frozenset[str]:
    return _ENVELOPE | _SCOPE.try_find(mode).default_value(frozenset())


def _validated(raw: object, /) -> Result[EmitPayload, EmitFault]:
    try:
        return Ok(_PAYLOAD.validate_python(raw, strict=True))  # the document folder's one admission strictness — egress/lens/report match
    except ValidationError as fault:
        return Error(EmitFault(payload=tuple(str(error["loc"]) for error in fault.errors())))


_CELL_SHAPE: Final[re.Pattern[str]] = re.compile(
    r"\A(?:(?P<int>[-+]?\d+)"
    r"|(?P<float>[-+]?(?:\d+\.\d*|\.\d+|\d+)(?:[eE][-+]?\d+)?)"
    r"|(?P<dt>\d{4}-\d{2}-\d{2}[T ]\d{2}:\d{2}(?::\d{2}(?:\.\d+)?)?)"
    r"|(?P<date>\d{4}-\d{2}-\d{2}))\Z"
)


def _temporal(parse: Callable[[str], CellScalar], text: str, /) -> CellScalar:
    try:
        return parse(text)
    except ValueError:
        return text


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
    match node:
        case RunNode():
            yield Paragraph(_run_markup(node), styles["Normal"])
        case BlockNode(block=BlockKind.HEADING, level=level, runs=runs):
            yield Paragraph(_runs_markup(runs), styles[f"Heading{level}"])
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
            width, height = intrinsic or (None, None)
            yield Image(spec.assets[asset_key.hex], width=width, height=height)
            if caption:
                yield Paragraph(_runs_markup(caption), styles["Italic"])
        case FormulaNode(tex=tex, alt=alt):
            yield Paragraph((alt or tex).translate(_MARKUP_ESCAPE), styles["Italic"])
        case FieldNode(field=field):
            yield Paragraph(field_text(field).translate(_MARKUP_ESCAPE), styles["Normal"])
        case AnnotationNode(annot=AnnotKind.LINK, contents=text, link=Uri(href=href)):
            yield Paragraph(f'<a href="{href.translate(_MARKUP_ESCAPE)}">{(text or href).translate(_MARKUP_ESCAPE)}</a>', styles["Normal"])
        case SectionNode(level=level, heading=head, children=kids):
            yield Paragraph(_runs_markup(head), styles[f"Heading{level}"])
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
        live.text if isinstance(live, RunNode) else field_text(live.field) if isinstance(live, FieldNode) else "" for live in walk(node)
    )


def _run_markup(run: RunNode, /) -> str:
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
    return "".join(
        _run_markup(live) if isinstance(live, RunNode) else field_text(live.field).translate(_MARKUP_ESCAPE) if isinstance(live, FieldNode) else ""
        for live in walk(node)
    )


def _span_commands(spans: tuple[tuple[int, int, int, int], ...], /) -> Iterator[tuple[object, ...]]:
    for row, col, col_span, row_span in spans:
        yield ("SPAN", (col, row), (col + max(col_span, 1) - 1, row + max(row_span, 1) - 1))


def _table_flowable(node: TableNode, styles: object, /) -> Table:
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
    outline: int = 0
    _seq: int = 0
    _last: int = 0

    def afterFlowable(self, flowable: object, /) -> None:
        style = getattr(getattr(flowable, "style", None), "name", "")
        if isinstance(style, str) and style.startswith("Heading"):
            # reportlab rejects an outline level that jumps more than one past the previous entry, so a skipped heading rank clamps.
            level = min(int(style.removeprefix("Heading") or "1") - 1, self._last + 1, 6)
            text, key = flowable.getPlainText(), f"h{self._seq}"
            self._seq += 1
            self._last = level
            self.canv.bookmarkPage(key)
            self.canv.addOutlineEntry(text, key, level=level, closed=level > 0)
            self.notify("TOCEntry", (level, text, self.page, key))
            self.outline += 1


def _title_block(spec: EmitSpec, node_key: str, /) -> Callable[[object, object], None]:
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


def _reportlab_author(plan: DocumentPlan, /) -> EmitFact:
    sink, size = io.BytesIO(), (A4[1], A4[0]) if plan.spec.landscape else A4
    styles = getSampleStyleSheet()
    doc = _SheetDocTemplate(sink, pagesize=size, title=plan.spec.title or plan.node.meta.key.hex, author=plan.spec.author, subject=plan.spec.subject)
    frame = Frame(20 * mm, 18 * mm, size[0] - 40 * mm, size[1] - 34 * mm, id="body")
    doc.addPageTemplates([PageTemplate(id="sheet", frames=[frame], onPage=_title_block(plan.spec, plan.node.meta.key.hex))])
    toc = TableOfContents(levelStyles=[ParagraphStyle(name=f"TOC{n}", fontSize=11 - n, leftIndent=12 * n) for n in range(3)])
    story = [toc, PageBreak(), *_flowables(plan.node, styles, plan.spec)] if plan.spec.toc else list(_flowables(plan.node, styles, plan.spec))
    doc.multiBuild(story) if plan.spec.toc else doc.build(story)
    return EmitFact(data=sink.getvalue(), pages=doc.page, outline_count=doc.outline)


def _weasyprint_html(plan: DocumentPlan, /) -> EmitFact:
    spec = plan.spec
    attachments = [Attachment(filename=path, name=name, relationship="Source") for name, path in spec.attachments.items()]
    title = spec.title or (plan.node.meta.key.hex if spec.variant.tagged else "")
    head = "".join(
        part
        for part in (
            f"<title>{title.translate(_MARKUP_ESCAPE)}</title>" if title else "",
            f'<meta name="author" content="{spec.author.translate(_MARKUP_ESCAPE)}">' if spec.author else "",
        )
        if part
    )
    source = f'<!DOCTYPE html><html><head><meta charset="utf-8">{head}</head><body>{to_html(plan.node)}</body></html>'
    data = HTML(string=source, base_url=spec.base_url or None).write_pdf(
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
    valid, errors = _conformance(data, spec.variant)
    return EmitFact(data=data, pages=_pdf_pages(data), tagged=spec.variant.tagged, valid=valid, errors=errors)


def _pdf_oxide_create(plan: DocumentPlan, /) -> EmitFact:
    pdf = Pdf.from_html(to_html(plan.node), title=plan.spec.title or plan.node.meta.key.hex, author=plan.spec.author)
    return EmitFact(data=pdf.to_bytes(), pages=len(pdf))


def _ua_field(page: object, node: FieldNode, /) -> object:
    x0, y0, x1, y1 = node.meta.bounds or (20.0, 40.0, 220.0, 60.0)
    box = (x0, y0, x1 - x0, y1 - y0)
    match node.field:
        case TextField(value=value):
            return page.text_field(node.name, *box, default_value=value or None)
        case CheckboxField(checked=checked):
            return page.checkbox(node.name, *box, checked)
        case RadioField(selected=selected, options=options):
            height = box[3] / len(options)
            buttons = [(option, box[0], box[1] + index * height, box[2], height) for index, option in enumerate(options)]
            return page.radio_group(node.name, buttons, selected=selected or None)
        case ComboField(selected=selected, options=options):
            return page.combo_box(node.name, *box, list(options), selected=selected or None)
        case ListField(selected=selected, options=options):
            height = box[3] / len(options)
            return Block.of_seq(enumerate(options)).fold(
                lambda live, item: live.checkbox(
                    f"{node.name}.{item[0]}", box[0], box[1] + item[0] * height, box[2], height, item[1] in selected
                ),
                page,
            )
        case SignatureField():
            return page.signature_field(node.name, *box)
        case ButtonField(label=label):
            return page.push_button(node.name, *box, label or node.name)
        case _ as unreachable:
            assert_never(unreachable)


def _ua_block(page: object, node: DocumentNode, spec: EmitSpec, /) -> object:
    match node:
        case SectionNode(level=level, heading=head):
            return page.heading(level, "".join(run.text for run in head))
        case BlockNode(block=BlockKind.HEADING, level=level, runs=runs):
            return page.heading(level, "".join(run.text for run in runs))
        case BlockNode(runs=runs) if runs:
            return page.paragraph("".join(run.text for run in runs))
        case FigureNode(asset_key=asset_key, alt=alt, intrinsic=intrinsic) if asset_key.hex in spec.assets:
            width, height = intrinsic or (200.0, 150.0)
            return page.image_with_alt(Path(spec.assets[asset_key.hex]).read_bytes(), 20.0, 40.0, width, height, alt)
        case FormulaNode(tex=tex, alt=alt):
            return page.paragraph(alt or tex)
        case AnnotationNode(annot=AnnotKind.NOTE, contents=text):
            return page.footnote("*", text)
        case FieldNode() as field:
            return _ua_field(page, field)
        case TableNode(rows=rows, spans=spans, header_rows=head_n) if rows:
            # one streaming arm serves spanned and span-free tables alike — an empty spans tuple folds to max_rowspan=1
            columns = [OxideColumn(_text(cell)) for cell in rows[0]] if head_n else [OxideColumn("") for _ in rows[0]]
            streaming = page.streaming_table(columns, repeat_header=bool(head_n), max_rowspan=max((s[3] for s in spans), default=1))
            for row in rows[head_n:] if head_n else rows:
                streaming.push_row([_text(cell) for cell in row])
            streaming.finish()
            return page
        case _:
            return page


def _pdf_ua_build(plan: DocumentPlan, /) -> EmitFact:
    builder = DocumentBuilder().title(plan.spec.title or plan.node.meta.key.hex).author(plan.spec.author).tagged_pdf_ua1()
    for face, blob in plan.spec.subset_fonts.items():
        builder = builder.register_embedded_font(face, EmbeddedFont.from_bytes(blob, name=face))
    foreign = tuple(n for n in walk(plan.node) if isinstance(n, StructureNode) and isinstance(n.role, ForeignRole))
    for structure in foreign:
        builder = builder.role_map(role_of(structure), standard_for(structure.role).value)
    sheets = [n for n in walk(plan.node) if isinstance(n, PageNode)] or [plan.node]
    page = builder.a4_page()
    for index, sheet in enumerate(sheets):
        page = page.new_page_same_size() if index else page
        for block in (
            n for n in walk(sheet, prune=(TableNode,)) if isinstance(n, SectionNode | BlockNode | FigureNode | FormulaNode | TableNode | FieldNode | AnnotationNode)
        ):
            page = _ua_block(page, block, plan.spec)
    data = _acroform_applied(page.done().build(), tuple(n for n in walk(plan.node) if isinstance(n, FieldNode)))
    valid, errors = _conformance(data, PdfVariant.UA_1)
    return EmitFact(data=data, pages=_pdf_pages(data), tagged=True, structure=len(foreign), valid=valid, errors=errors)


def _acroform_applied(data: bytes, fields: tuple[FieldNode, ...], /) -> bytes:
    # pdf-oxide's fluent constructors carry no flag/tooltip/limit knobs (catalog `FluentPageBuilder` row), so the
    # model's remaining AcroForm semantics land in ONE post-build pikepdf fold over `/AcroForm/Fields`:
    # `/Ff` read-only (bit 1) + required (bit 2) + combo Edit (bit 19), `/TU` tooltip, text `/MaxLen`. Signature
    # signer/reason are SIGNING facts the exchange/conformance PAdES rail applies; button actions ride the
    # egress annotation pass. A field-free document skips the reopen entirely.
    if not fields:
        return data
    wanted = {node.name: node for node in fields}
    with pikepdf.open(BytesIO(data)) as pdf:
        for ref in pdf.Root.get("/AcroForm", pikepdf.Dictionary()).get("/Fields", []):
            node = wanted.get(str(ref.get("/T", "")))
            if node is None:
                continue
            flags = (1 if node.readonly else 0) | (2 if node.required else 0)
            if isinstance(node.field, ComboField) and node.field.editable:
                flags |= 1 << 18
            if isinstance(node.field, TextField) and node.field.max_length is not None:
                ref.MaxLen = node.field.max_length
            if flags:
                ref.Ff = flags
            if node.tooltip:
                ref.TU = pikepdf.String(node.tooltip)
        sink = BytesIO()
        pdf.save(sink)
        return sink.getvalue()


def _conformance(data: bytes, variant: PdfVariant, /) -> tuple[bool, int]:
    if variant is PdfVariant.NONE:
        return (True, 0)
    # pdf-oxide's Rust handle closes deterministically once the verdicts materialize — never a GC-reaped native document.
    with pdf_oxide.PdfDocument.from_bytes(data) as doc:
        verdicts = (
            *(doc.validate_pdf_a(level) for level in _ORACLE_LEVEL.try_find(variant).to_list()),
            *((doc.validate_pdf_ua(),) if variant.tagged else ()),
        )
        return (all(verdict["valid"] for verdict in verdicts), sum(len(verdict["errors"]) for verdict in verdicts))


def _pdf_pages(data: bytes, /) -> int:
    # terminal page census for the provider arms whose render returns bare bytes (WeasyPrint, the pdf-oxide builder
    # chain, typst): every ReceiptKind.PDF fact carries a real count, so a zero-page receipt over a non-empty PDF is unmintable.
    with pikepdf.open(BytesIO(data)) as pdf:
        return len(pdf.pages)


def _typst_compile(plan: DocumentPlan, /) -> EmitFact:
    title = plan.spec.title or (plan.node.meta.key.hex if plan.spec.variant.tagged else None)
    data, warnings = plan.world(title=title).compile_with_warnings(
        pdf_standards=list(plan.spec.variant.typst),
        timestamp=plan.spec.timestamp or None,
    )
    valid, errors = _conformance(data, plan.spec.variant)
    return EmitFact(data=data, pages=_pdf_pages(data), warnings=len(warnings), tagged=plan.spec.variant.tagged, valid=valid, errors=errors)


def _typst_query(plan: DocumentPlan, /) -> EmitFact:
    result = plan.world().query(plan.spec.selector, field=plan.spec.field_name or None, one=plan.spec.one).encode()
    return EmitFact(data=result, queried=len(result))


def _typst_eval(plan: DocumentPlan, /) -> EmitFact:
    result = plan.world().eval(plan.spec.expression).encode()
    return EmitFact(data=result, queried=len(result))


def _pymupdf_render(plan: DocumentPlan, /) -> EmitFact:
    doc = pymupdf.open(stream=plan.spec.source, filetype="pdf")
    try:
        if plan.spec.password and not doc.authenticate(plan.spec.password):
            raise ValueError("PDF authentication failed")
        indices = plan.spec.pages or tuple(range(doc.page_count))
        if any(index >= doc.page_count for index in indices):
            raise ValueError(f"page index out of range: document holds {doc.page_count} pages")
        frames = tuple(
            doc[index].get_pixmap(matrix=pymupdf.Matrix(plan.spec.scale, plan.spec.scale)).tobytes(output=plan.spec.image_format)
            for index in indices
        )
        data = frames[0] if len(frames) == 1 else msgspec.msgpack.encode(frames)
        return EmitFact(data=data, pages=len(frames), scale=plan.spec.scale)
    finally:
        doc.close()


def _frame(pdf: object, index: int, keywords: frozendict[str, object], /) -> tuple[tuple[int, ...], bytes]:
    page = pdf.get_page(index)
    try:
        bitmap = page.render(**keywords)
        try:
            view = bitmap.to_numpy()
            return (tuple(view.shape), view.tobytes())
        finally:
            bitmap.close()
    finally:
        page.close()


def _pypdfium2_raster(plan: DocumentPlan, /) -> EmitFact:
    pdf = pypdfium2.PdfDocument(plan.spec.source, password=plan.spec.password or None, autoclose=True)
    try:
        indices = plan.spec.pages or tuple(range(len(pdf)))
        if any(index >= len(pdf) for index in indices):
            raise ValueError(f"page index out of range: document holds {len(pdf)} pages")
        frames = tuple(_frame(pdf, index, plan.spec.render_keywords) for index in indices)
        data = b"".join(frame for _shape, frame in frames)
        outline = tuple(_outline_rows(pdf, _OUTLINE_MAX_DEPTH))
        return EmitFact(
            data=data,
            frame_shapes=tuple(shape for shape, _bytes in frames),
            pages=len(frames),
            scale=plan.spec.scale,
            outline_count=len(outline),
            outline=outline,
        )
    finally:
        pdf.close()


def _pdf_oxide_render(plan: DocumentPlan, /) -> EmitFact:
    # pdf-oxide's Rust handle closes deterministically once every frame materializes — never a GC-reaped native document.
    with pdf_oxide.PdfDocument.from_bytes(plan.spec.source, password=plan.spec.password or None) as doc:
        dpi = max(1, int(72 * plan.spec.scale))  # a valid sub-1/72 scale still renders — a zero-dpi request is unspellable
        indices = plan.spec.pages or tuple(range(doc.page_count))
        if any(index >= doc.page_count for index in indices):
            raise ValueError(f"page index out of range: document holds {doc.page_count} pages")
        frames = [doc.render_page(index, dpi=dpi, format=plan.spec.image_format) for index in indices]
    data = frames[0] if len(frames) == 1 else msgspec.msgpack.encode(frames)
    return EmitFact(data=data, pages=len(frames), scale=plan.spec.scale)


def _outline_rows(pdf: object, max_depth: int) -> Iterator[OutlineRow]:
    for mark in pdf.get_toc(max_depth=max_depth):
        dest = mark.get_dest()
        yield (mark.get_count(), mark.get_title(), dest.get_index() if dest else -1)


def _pypdf_assemble(plan: DocumentPlan, /) -> EmitFact:
    writer = PdfWriter()
    writer.append(PdfReader(io.BytesIO(plan.spec.source), password=plan.spec.password or None))
    for page in writer.pages:
        page.compress_content_streams()
    writer.add_metadata({"/Title": plan.spec.title or plan.node.meta.key.hex, "/Author": plan.spec.author, "/Subject": plan.spec.subject})
    sink = io.BytesIO()
    writer.write(sink)
    return EmitFact(data=sink.getvalue(), pages=len(writer.pages))


def _pikepdf_repair(plan: DocumentPlan, /) -> EmitFact:
    sink = io.BytesIO()
    with pikepdf.open(io.BytesIO(plan.spec.source), password=plan.spec.password) as pdf:
        pdf.save(sink, linearize=True, recompress_flate=True, object_stream_mode=pikepdf.ObjectStreamMode.generate, deterministic_id=True)
        return EmitFact(data=sink.getvalue(), pages=len(pdf.pages))


def _font_embed(plan: DocumentPlan, /) -> EmitFact:
    for face, blob in plan.spec.subset_fonts.items():
        pdfmetrics.registerFont(TTFont(face, io.BytesIO(blob), subfontIndex=plan.spec.subfont_index))
    canvas = Canvas(io.BytesIO())
    canvas.setTitle(plan.spec.title or plan.node.meta.key.hex)
    canvas.setAuthor(plan.spec.author)
    canvas.setSubject(plan.spec.subject)
    sheets = [page for page in walk(plan.node) if isinstance(page, PageNode)] or [plan.node]
    for page in sheets:
        for run in (leaf for leaf in walk(page) if isinstance(leaf, RunNode)):
            origin = run.meta.bounds or (0.0, 0.0, 0.0, 0.0)
            canvas.setFont(run.font_key, run.size)
            canvas.setFillColorRGB(run.color[0] / 255, run.color[1] / 255, run.color[2] / 255)
            canvas.drawString(origin[0], origin[1], run.text)
        canvas.showPage()
    canvas.save()
    return EmitFact(data=canvas.getpdfdata(), pages=len(sheets), faces=tuple(plan.spec.subset_fonts))


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
            document.add_heading("".join(run.text for run in runs), level=level)
        case BlockNode(runs=runs, children=kids, block=block):
            paragraph = document.add_paragraph(style="Quote" if block is BlockKind.QUOTE else None)
            for run in runs:
                _docx_run(paragraph.add_run(run.text), run)
            for kid in kids:
                _docx_block(document, kid, spec)
        case ListNode(list_kind=kind, items=items):
            style = "List Number" if kind is ListKind.ORDERED else "List Bullet"
            for item in items:
                paragraph = document.add_paragraph(style=style)
                for run in (r for r in walk(item) if isinstance(r, RunNode)):
                    _docx_run(paragraph.add_run(run.text), run)
        case FormulaNode(tex=tex, alt=alt):
            _docx_run_obj = document.add_paragraph().add_run(alt or tex)
            _docx_run_obj.italic = True
        case FieldNode(field=field):
            document.add_paragraph(field_text(field))
        case AnnotationNode(annot=AnnotKind.LINK, contents=text, link=Uri(href=href)):
            document.add_paragraph(f"{text or href} <{href}>")
        case TableNode(rows=rows, spans=spans, header_rows=head_n) if rows:
            table = document.add_table(rows=len(rows), cols=len(rows[0]), style="Table Grid")
            for ri, row in enumerate(rows):
                for ci, cell in enumerate(row):
                    target = table.cell(ri, ci)
                    target.text, target.vertical_alignment = _text(cell), WD_CELL_VERTICAL_ALIGNMENT.CENTER
                    for run in (r for para in target.paragraphs for r in para.runs):
                        run.bold = ri < head_n
            for row, col, col_span, row_span in spans:
                table.cell(row, col).merge(table.cell(row + row_span - 1, col + col_span - 1))
        case FigureNode(asset_key=asset_key, meta=meta) if asset_key.hex in spec.assets:
            bounds = meta.bounds or (0.0, 0.0, 0.0, 0.0)
            document.add_picture(spec.assets[asset_key.hex], width=Pt(bounds[2]) if bounds[2] else None)
        case SectionNode(level=level, heading=head, children=kids):
            document.add_heading("".join(run.text for run in head), level=level)
            for kid in kids:
                _docx_block(document, kid, spec)
        case PageNode(children=kids):
            for kid in kids:
                _docx_block(document, kid, spec)
            document.add_page_break()
        case _:
            pass


def _docx_scaffold(document: object, spec: EmitSpec, /) -> None:
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


def _docx_emit(plan: DocumentPlan, /) -> EmitFact:
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


def _docxtpl_emit(plan: DocumentPlan, /) -> EmitFact:
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
            case FigureNode(asset_key=asset_key, meta=meta) if asset_key.hex in plan.spec.assets:
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
            case FormulaNode(tex=tex, alt=alt):
                context[role] = alt or tex
            case FieldNode(field=field):
                context[role] = field_text(field)
    undeclared = tuple(template.get_undeclared_template_variables())
    template.render(context, autoescape=True)
    template.render_footnotes(context)
    sink = io.BytesIO()
    template.save(sink)
    return EmitFact(data=sink.getvalue(), undeclared=undeclared, template_path=plan.spec.template)


def _pptx_emit(plan: DocumentPlan, /) -> EmitFact:
    presentation = Presentation()
    presentation.core_properties.title = plan.spec.title or plan.node.meta.key.hex
    presentation.core_properties.author = plan.spec.author
    presentation.core_properties.subject = plan.spec.subject
    blank = presentation.slide_layouts[6]
    for page in [n for n in walk(plan.node) if isinstance(n, PageNode)] or [plan.node]:
        slide = presentation.slides.add_slide(blank)
        frame = slide.shapes.add_textbox(
            Inches(0.5), Inches(0.5), presentation.slide_width - Inches(1), presentation.slide_height - Inches(1)
        ).text_frame
        frame.word_wrap = True
        for block in (n for n in walk(page, prune=(TableNode,)) if isinstance(n, BlockNode | FormulaNode | FieldNode | AnnotationNode | TableNode)):
            match block:
                case TableNode(rows=rows, spans=spans) if rows:
                    grid = slide.shapes.add_table(
                        len(rows), len(rows[0]), Inches(0.5), Inches(0.5), presentation.slide_width - Inches(1), presentation.slide_height - Inches(1)
                    ).table
                    for ri, row in enumerate(rows):
                        for ci, cell in enumerate(row):
                            grid.cell(ri, ci).text = _text(cell)
                    for row, col, col_span, row_span in spans:
                        grid.cell(row, col).merge(grid.cell(row + row_span - 1, col + col_span - 1))
                case FormulaNode(tex=tex, alt=alt):
                    formula_run = frame.add_paragraph().add_run()
                    formula_run.text, formula_run.font.italic = alt or tex, True
                case AnnotationNode(annot=AnnotKind.LINK, contents=text, link=Uri(href=href)):
                    link_run = frame.add_paragraph().add_run()
                    link_run.text = text or href
                    link_run.hyperlink.address = href
                case AnnotationNode():
                    pass
                case FieldNode(field=field):
                    frame.add_paragraph().add_run().text = field_text(field)
                case BlockNode(runs=runs):
                    paragraph = frame.add_paragraph()
                    for run in runs:
                        run_obj = paragraph.add_run()
                        run_obj.text = run.text
                        run_obj.font.bold, run_obj.font.italic, run_obj.font.size = run.weight >= _BOLD_WEIGHT, run.italic, SlidePt(run.size)
                        run_obj.font.underline = TextDecoration.UNDERLINE in run.decorations
                        if run.color != (0, 0, 0):
                            run_obj.font.color.rgb = SlideColor(*run.color)
        for figure in (n for n in walk(page) if isinstance(n, FigureNode) and n.asset_key.hex in plan.spec.assets):
            slide.shapes.add_picture(plan.spec.assets[figure.asset_key.hex], Inches(0.5), Inches(0.5))
    sink = io.BytesIO()
    presentation.save(sink)
    return EmitFact(data=sink.getvalue(), pages=len(presentation.slides))


def _odf_text[E](element: E, text: str, /) -> E:
    addTextToElement(element, text)
    return element


def _odf_style(document: object, cache: dict[object, str], run: RunNode, /) -> str:
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
            body.addElement(_odf_runs(H(outlinelevel=level), runs, document, cache))
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
        case FormulaNode(tex=tex, alt=alt):
            body.addElement(_odf_text(P(), alt or tex))
        case FieldNode(field=field):
            body.addElement(_odf_text(P(), field_text(field)))
        case SectionNode(level=level, heading=head, children=kids):
            body.addElement(_odf_runs(H(outlinelevel=level), head, document, cache))
            for kid in kids:
                _odf_block(document, body, kid, spec, cache)
        case TableNode() as table:
            body.addElement(_odf_sheet(document, table, f"T{table.meta.key.hex[:8]}"))
        case PageNode(children=kids):
            for kid in kids:
                _odf_block(document, body, kid, spec, cache)
        case _:
            pass


def _odf_emit(plan: DocumentPlan, /) -> EmitFact:
    document = OpenDocumentSpreadsheet() if plan.mode is DocumentMode.ODS else OpenDocumentText()
    _odf_scaffold(document, plan.spec, plan.node.meta.key.hex)
    if plan.mode is DocumentMode.ODS:
        tables = [n for n in walk(plan.node) if isinstance(n, TableNode)]
        for index, table in enumerate(tables):
            name = (plan.spec.sheet or "Sheet1") if len(tables) == 1 else f"{plan.spec.sheet or 'Sheet'}{index + 1}"
            document.spreadsheet.addElement(_odf_sheet(document, table, name))
    else:
        _odf_block(document, document.text, plan.node, plan.spec, {})
    sink = io.BytesIO()
    document.write(sink)
    merges = sum(len(t.spans) for t in walk(plan.node) if isinstance(t, TableNode))
    footnotes = sum(1 for n in walk(plan.node) if isinstance(n, AnnotationNode) and n.annot is AnnotKind.NOTE)
    return EmitFact(data=sink.getvalue(), merges=merges, footnotes=footnotes)


def _xlsx_emit(plan: DocumentPlan, /) -> EmitFact:
    tables = tuple(table for table in walk(plan.node) if isinstance(table, TableNode))
    row_count = sum(len(table.rows) for table in tables)
    width = max((len(row) for table in tables for row in table.rows), default=0)
    rows = (tuple(CellValue.of(cell) for cell in row) for table in tables for row in table.rows)
    inferred = XlsxRegime.IN_MEMORY if row_count < _STREAMING_ROW_THRESHOLD else XlsxRegime.STREAMED
    return _xlsx_write(plan, rows, row_count, width, plan.spec.spreadsheet or inferred)


def _xlsx_merge(node: DocumentNode, sheet: object, header_fmt: object, /) -> int:
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


def _xlsx_write(
    plan: DocumentPlan,
    rows: Iterable[tuple[CellValue, ...]],
    row_count: int,
    width: int,
    regime: XlsxRegime,
    /,
) -> EmitFact:
    streamed = regime is XlsxRegime.STREAMED
    sink = io.BytesIO()
    book = xlsxwriter.Workbook(
        sink,
        {
            "constant_memory": streamed,
            "in_memory": not streamed,
            "use_zip64": row_count >= _ZIP64_ROW_THRESHOLD,
            "remove_timezone": True,
            "nan_inf_to_errors": True,
        },
    )
    book.set_properties({"title": plan.spec.title or plan.node.meta.key.hex, "author": plan.spec.author, "subject": plan.spec.subject})
    sheet = book.add_worksheet(plan.spec.sheet or None)
    header_fmt, headed = book.add_format(dict(_FORMATS["header"])), plan.spec.header
    formats = frozendict({kind: book.add_format(dict(_FORMATS[kind])) for kind in ("number", "datetime")})
    last = row_count - 1
    if width:
        sheet.set_column(0, width - 1, plan.spec.column_width)
    if plan.spec.landscape:
        sheet.set_landscape()
    if plan.spec.header_text or plan.spec.footer_text:
        sheet.set_header(f"&L{plan.spec.header_text}&R&P")
        sheet.set_footer(f"&L{plan.spec.footer_text}&R&D")
    for index, row in enumerate(rows):
        sheet.set_row(index, None, header_fmt if index == 0 and headed and streamed else None)
        for column, value in enumerate(row):
            value.write_xlsxwriter(sheet, index, column, formats)
    merges = 0
    if not streamed and row_count and width:
        merges = _xlsx_merge(plan.node, sheet, header_fmt)
        sheet.autofilter(0, 0, last, width - 1) if merges else sheet.add_table(
            0, 0, last, width - 1, {"header_row": headed, "autofilter": True, "banded_rows": True}
        )
        if row_count > 1:
            sheet.conditional_format(1, 0, last, width - 1, {"type": "data_bar"})
    elif headed and row_count and width:
        sheet.freeze_panes(1, 0)
        sheet.autofilter(0, 0, last, width - 1)
    book.close()
    return EmitFact(data=sink.getvalue(), pages=row_count, merges=merges)


def _ruamel_emit(plan: DocumentPlan, /) -> EmitFact:
    engine, sink = YAML(), io.StringIO()
    engine.dump(to_builtins(plan.node), sink)
    return EmitFact(data=sink.getvalue().encode())


def _tomlkit_emit(plan: DocumentPlan, /) -> EmitFact:
    return EmitFact(data=tomlkit.dumps(to_builtins(plan.node)).encode())


def _markdown_emit(plan: DocumentPlan, /) -> EmitFact:
    return EmitFact(data=to_markdown(plan.node).encode())


def _latex_emit(plan: DocumentPlan, /) -> EmitFact:
    return EmitFact(data=to_latex(plan.node).encode())


def _hardened_parse(source: bytes) -> object:
    return etree.parse(io.BytesIO(source), etree.XMLParser(resolve_entities=False, huge_tree=False, no_network=True))


_DTD_EXTERNAL: Final[re.Pattern[bytes]] = re.compile(rb"<!ENTITY\s+%?\s*[^>]*?\b(?:SYSTEM|PUBLIC)\b")


def _hardened_dtd(source: bytes) -> object:
    # etree.DTD accepts no parser controls, so external SYSTEM/PUBLIC entity ids are refused before construction.
    if _DTD_EXTERNAL.search(source):
        raise ValueError("external entity reference in DTD source")
    return etree.DTD(io.BytesIO(source))


_VALIDATOR: Final[Map[SchemaKind, Callable[[bytes], object]]] = Map.of_seq([
    (SchemaKind.XSD, lambda source: etree.XMLSchema(_hardened_parse(source))),
    (SchemaKind.RELAXNG, lambda source: etree.RelaxNG(_hardened_parse(source))),
    (SchemaKind.SCHEMATRON, lambda source: isoschematron.Schematron(_hardened_parse(source), store_report=True)),
    (SchemaKind.DTD, _hardened_dtd),
])


def _lxml_emit(plan: DocumentPlan, /) -> EmitFact:
    tree = to_lxml_tree(plan.node)
    etree.cleanup_namespaces(tree)
    return EmitFact(data=etree.tostring(tree, xml_declaration=True, encoding="utf-8", pretty_print=plan.spec.pretty))


def _lxml_transform(plan: DocumentPlan, /) -> EmitFact:
    access = etree.XSLTAccessControl(read_file=False, write_file=False, create_dir=False, read_network=False, write_network=False)
    transform = etree.XSLT(_hardened_parse(plan.spec.stylesheet), access_control=access)
    quoted = {key: etree.XSLT.strparam(value) for key, value in plan.spec.xslt_params.items()}
    return EmitFact(data=bytes(transform(to_lxml_tree(plan.node), **quoted)))


def _lxml_validate(plan: DocumentPlan, /) -> EmitFact:
    validator = _VALIDATOR[plan.spec.schema_kind](plan.spec.schema)
    tree = to_lxml_tree(plan.node)
    valid = validator.validate(tree)
    return EmitFact(data=etree.tostring(tree, xml_declaration=True, encoding="utf-8"), valid=valid, errors=len(validator.error_log))


def _lxml_query(plan: DocumentPlan, /) -> EmitFact:
    query = etree.XPath(plan.spec.path, namespaces=dict(plan.spec.namespaces), smart_strings=False)
    result = query(to_lxml_tree(plan.node))
    projected = [etree.tostring(hit).decode() if isinstance(hit, etree._Element) else hit for hit in result] if isinstance(result, list) else result
    data = msgspec.msgpack.encode(projected)
    return EmitFact(data=data, queried=len(projected) if isinstance(projected, list) else 1)


def _dispatched(plan: DocumentPlan) -> EmitFact:
    return BACKENDS[plan.mode].arm(plan)


# --- [COMPOSITION] ----------------------------------------------------------------------

_RECEIPT: Final[Map[ReceiptKind, Callable[[ContentKey, EmitFact], "ArtifactReceipt"]]] = Map.of_seq([
    (ReceiptKind.PDF, lambda key, fact: ArtifactReceipt.Pdf(key, len(fact.data), fact.pages)),
    (ReceiptKind.OFFICE, lambda key, fact: ArtifactReceipt.Office(key, len(fact.data))),
    (ReceiptKind.DOCUMENT, lambda key, fact: ArtifactReceipt.Document(key, len(fact.data))),
])

BACKENDS: Final[Map[DocumentMode, Backend]] = Map.of_seq([
    # WeasyPrint layout and the Typst compiler hold the GIL through their native render, so every arm over either body is
    # native-hostile and rides the WORKER band's process kernel — a RELEASING thread row serializes the whole loop behind it.
    (DocumentMode.PDF_AUTHOR, Backend(Band.CORE, _reportlab_author, ReceiptKind.PDF)),
    (DocumentMode.PDF_HTML, Backend(Band.WORKER, _weasyprint_html, ReceiptKind.PDF)),
    (DocumentMode.PDF_OXIDE, Backend(Band.CORE, _pdf_oxide_create, ReceiptKind.PDF)),
    (DocumentMode.PDF_UA, Backend(Band.CORE, _pdf_ua_build, ReceiptKind.PDF)),
    (DocumentMode.PDF_TYPST, Backend(Band.WORKER, _typst_compile, ReceiptKind.PDF)),
    (DocumentMode.TYPST_QUERY, Backend(Band.WORKER, _typst_query, ReceiptKind.DOCUMENT)),
    (DocumentMode.TYPST_EVAL, Backend(Band.WORKER, _typst_eval, ReceiptKind.DOCUMENT)),
    (DocumentMode.TYPST_DATA, Backend(Band.WORKER, _typst_compile, ReceiptKind.PDF)),
    (DocumentMode.PDF_RENDER, Backend(Band.CORE, _pymupdf_render, ReceiptKind.PDF)),
    (DocumentMode.PDF_RASTER, Backend(Band.CORE, _pypdfium2_raster, ReceiptKind.PDF)),
    (DocumentMode.PDF_OXIDE_RENDER, Backend(Band.CORE, _pdf_oxide_render, ReceiptKind.PDF)),
    (DocumentMode.PDF_ASSEMBLE, Backend(Band.CORE, _pypdf_assemble, ReceiptKind.PDF)),
    (DocumentMode.PDF_REPAIR, Backend(Band.WORKER, _pikepdf_repair, ReceiptKind.PDF)),
    (DocumentMode.FONT_EMBED, Backend(Band.CORE, _font_embed, ReceiptKind.PDF)),
    (DocumentMode.DOCX, Backend(Band.CORE, _docx_emit, ReceiptKind.OFFICE)),
    (DocumentMode.DOCX_TEMPLATE, Backend(Band.CORE, _docxtpl_emit, ReceiptKind.OFFICE)),
    (DocumentMode.PPTX, Backend(Band.WORKER, _pptx_emit, ReceiptKind.OFFICE)),
    (DocumentMode.XLSX, Backend(Band.CORE, _xlsx_emit, ReceiptKind.OFFICE)),
    (DocumentMode.ODT, Backend(Band.CORE, _odf_emit, ReceiptKind.OFFICE)),
    (DocumentMode.ODS, Backend(Band.CORE, _odf_emit, ReceiptKind.OFFICE)),
    (DocumentMode.XML, Backend(Band.WORKER, _lxml_emit, ReceiptKind.DOCUMENT)),
    (DocumentMode.XML_TRANSFORM, Backend(Band.WORKER, _lxml_transform, ReceiptKind.DOCUMENT)),
    (DocumentMode.XML_VALIDATE, Backend(Band.WORKER, _lxml_validate, ReceiptKind.DOCUMENT)),
    (DocumentMode.XML_QUERY, Backend(Band.WORKER, _lxml_query, ReceiptKind.DOCUMENT)),
    (DocumentMode.YAML, Backend(Band.CORE, _ruamel_emit, ReceiptKind.DOCUMENT)),
    (DocumentMode.TOML, Backend(Band.CORE, _tomlkit_emit, ReceiptKind.DOCUMENT)),
    (DocumentMode.MARKDOWN, Backend(Band.CORE, _markdown_emit, ReceiptKind.DOCUMENT)),
    (DocumentMode.LATEX, Backend(Band.CORE, _latex_emit, ReceiptKind.DOCUMENT)),
])
```

## [03]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)

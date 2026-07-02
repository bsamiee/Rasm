# [PY_ARTIFACTS_EMIT]

The document-emission and post-processing axis, rebased onto the one `DocumentNode` semantic tree. `DocumentPlan` is ONE frozen owner discriminating document mode over a `frozendict[DocumentMode, Backend]` policy table whose every `Backend` row binds the lowering arm to its runtime `Band`, so the runtime/worker split is a row column rather than a second dispatch surface — every arm is a LOWERING fold FROM the `document/model#NODE` `DocumentNode` tree, never an opaque payload: PDF authoring (reportlab `BaseDocTemplate`+`Frame`+`PageTemplate` sheets with the `onPage` ISO-7200 title-block callback, a `TableStyle`-styled `THead`/`TFoot`/`SPAN` schedule table, escaped run-fidelity `Paragraph` markup carrying each `RunNode`'s real weight/italic/colour/baseline/decoration and its inline `LINK` annotation, and the `afterFlowable` outline+`TableOfContents` resolved across `multiBuild` passes; weasyprint HTML-CSS with the `pdf_variant` PDF/A-PDF/UA archival row plus `base_url`/`output_intent`/`stylesheets`/`attachments` (the PDF/A-3 embedded-source-file seal) and the `full_fonts`/`optimize_images`/`presentational_hints` archival controls; the commercial-safe pdf_oxide create pair — declarative `Pdf.from_html` and the native tagged PDF/UA-1 `DocumentBuilder` fluent author with `role_map` bound to the model's `StructureNode` foreign roles, one `PageNode`-per-sheet `new_page_same_size` flow, and native AcroForm widgets lowered from each `FieldNode`; typst markup typesetting with `pdf_standards` selection over the held font-cached `Compiler` plus `sys.inputs` data-binding and `timestamp` reproducibility), PDF render/raster (pymupdf native `Pixmap.tobytes`, pypdfium2 `to_numpy` — both Pillow-free on the runtime), PDF assembly (pypdf), PDF repair/linearize (pikepdf, worker band), OOXML office (python-docx sections+header/footer+`Styles.add_style`+`_Cell.merge`, python-pptx, docxtpl, and the ONE xlsxwriter body keyed on the `XlsxRegime` policy — the O(1) `constant_memory` stream vs the in-memory `add_table`/`autofilter`/`conditional_format`/`merge_range` pass), OpenDocument (odfpy ODT/ODS authoring over cached appearance-keyed `text:style` runs, `text:list`/`text:note` footnotes/`text:a` links, `odf.number` typed `DateStyle` cells, and `numbercolumnsspanned`/`CoveredTableCell` merge spans, the pure-Python OASIS sibling to the OOXML office rows), and structured-text (lxml/ruamel-yaml/tomlkit). Emission lowers FROM the tree and `document/lens#LENS` recovers TO it, so production and extraction are inverses over the one node algebra defined once at `document/model#NODE`. Every per-arm input is one frozen `EmitSpec` `msgspec.Struct` admitted exactly once at `DocumentPlan.of` through the closed `EmitPayload` `TypedDict` and its module-level `TypeAdapter`, the per-mode `_REQUIRED` precondition making admission total over well-formed requests — never a `dict[str, object]` bag, never re-validated in the interior. Every production threads one `EmitFact` typed-evidence carrier onto the frozen owner through `copy.replace`, the `@receipted(OPEN)` harvest weave draining `DocumentPlan.contribute` off the stepped owner so the receipt reads `EmitFact` without an in-process re-run; `EmitFault` is the closed `@tagged_union` over the `payload`/`unsatisfied` ADMISSION causes `of` produces, while every arm-level provider raise (`typst.TypstError`, `pypdfium2.PdfiumError`, `lxml.etree.XMLSyntaxError`, `xlsxwriter.XlsxWriterException`) converts to the runtime `BoundaryFault` at the `async_boundary` capsule, exactly as `document/egress#FINISH` routes its provider raises.

## [01]-[INDEX]

- [01]-[DOCUMENT]: document-mode dispatch axis whose `Backend`-per-mode policy rows lower from `DocumentNode` across the one `BACKENDS` table — each row binding its `Band` (`CORE`/`WORKER`) so the gated lxml/pikepdf/python-pptx band is a row column the `_worker_emit` re-resolves rather than a parallel `GATED_ARMS` table — carrying the reportlab `BaseDocTemplate` sheet author (styled schedule table, run-fidelity `Paragraph` markup, title-block `onPage` furniture, `afterFlowable` outline + `multiBuild` TOC), the weasyprint archival HTML row over the full PDF/A + accessible-`-a` + PDF/UA `PdfVariant` matrix, the commercial-safe pdf_oxide `Pdf.from_html` create row and the paginated tagged PDF/UA-1 `DocumentBuilder` row with AcroForm `FieldNode` widgets, the Typst held-`Compiler` compile/data rows and the free-function `query`/`eval` introspection, the pymupdf/pypdfium2 Pillow-free raster rows with the pypdfium2 outline harvest, the pypdf assemble and pikepdf repair rows, the python-docx section/style/merge and python-pptx/docxtpl authoring rows, the ONE `XlsxRegime`-keyed xlsxwriter body (stream vs `add_table`/`conditional_format`/`merge_range` in-memory pass), the odfpy ODT/ODS OpenDocument authoring rows (cached run styles, footnotes, links, typed data-style cells, merge spans), and the lxml transform/validate/query rows; `EmitSpec` the one admitted-once typed payload, `EmitFact` the threaded evidence carrier, `EmitFault` the closed admission vocabulary, and the `@receipted` harvest weave over a thin pure `_emit` returning the stepped `Self`.

## [02]-[DOCUMENT]

- Entry: `produced` is the ONE modal-arity entrypoint over `DocumentPlan | Iterable[DocumentPlan]` discriminating on the INPUT SHAPE — a lone plan is `Block.singleton`, an iterable is `Block.of_seq`, normalized once at the head, threaded through the rail with NO `batch`/`mode` knob. `_emitted` constructs ONE held `Compiler` off the batch head OFF the loop through `to_thread.run_sync` under the shared `_OFFLOAD` limiter (the font-discovery scan is blocking native, so the build never stalls the scheduler) yet INSIDE the `async_boundary` capsule (so a `Compiler` construction raise still converts to `BoundaryFault`) when the head mode is a `_HELD_WORLD` Typst row and threads it through every Typst arm so a multi-document Typst render pays font load once; a Typst arm in a non-Typst-headed batch builds its own per-plan world so it never receives a `None` compiler. `_emit` is the thin pure core returning the stepped `Self` (a `ReceiptContributor`) the `@receipted` weave harvests; `_stepped` reads `Backend.band` and NEVER runs a synchronous native render inline on the loop — a `CORE`-band arm crosses `anyio.to_thread.run_sync` (the GIL-releasing native render off the loop) under the shared `_OFFLOAD` `CapacityLimiter`, a `WORKER`-band arm crosses `anyio.to_process.run_sync` onto `_worker_emit` which re-resolves the SAME `BACKENDS` row, so the worker lane carries no second `match` and neither lane stalls the scheduler. `produced` returns a `RuntimeRail[Block[ContentKey]]` keyed over each `EmitFact.data`.
- Auto: every arm receives the `DocumentNode` tree and the held `Compiler` world and lowers the tree — `PDF_AUTHOR` folds it through `_flowables` — each `RunNode` lowering through the escaped `_run_markup` inline markup carrying its real weight/italic/colour/`super`/`sub`/underline/strike (the RUN_FIDELITY the naive plain-`run.text` splice dropped, `<`/`&` escaped once before any tag), a `QUOTE` block to `Italic`, an inline `LINK` annotation to `<a href>`, a `FigureNode` sized from its `intrinsic` with its caption — into a `_SheetDocTemplate` (a `BaseDocTemplate` over one `Frame`/`PageTemplate`), each `TableNode` a `_table_flowable` carrying a real `TableStyle` (`GRID`, a shaded bold `THead` band from `header_rows`, a `TFoot` band, `SPAN` commands folded from `TableNode.spans`, `repeatRows`) over run-fidelity cell `Paragraph`s, the `_title_block` `onPage` callback painting the running header/footer/page-number sheet furniture, and `afterFlowable` minting the PDF outline (`bookmarkPage`+`addOutlineEntry`) and notifying the `TableOfContents` so a `spec.toc` request resolves through `multiBuild`; `PDF_HTML` folds `to_html(node)` through weasyprint `HTML(string, base_url).write_pdf(pdf_variant=, pdf_tags=, pdf_forms=, output_intent=, stylesheets=, attachments=[Attachment(relationship="Source")], full_fonts=, optimize_images=, presentational_hints=)` so the `A_3A` variant actually embeds its PDF/A-3 source files rather than only naming them; `PDF_OXIDE` folds `to_html(node)` through the commercial-safe (MIT/Apache) pdf_oxide `Pdf.from_html(...).to_bytes()` create root; `PDF_UA` folds the block sequence through the fluent tagged `DocumentBuilder().tagged_pdf_ua1()` — `register_embedded_font` per subset face, `role_map` binding each foreign `StructureNode` role to `standard_for(role)`, `_ua_block` flowing each `SectionNode` title + `heading`/`paragraph`/`image_with_alt`, each `FieldNode` as its native AcroForm widget (`text_field`/`checkbox`/`combo_box`/`signature_field`/`push_button`), and each `TableNode` as a native tagged `Table` (`Column` headers + `TD` body rows, real THead/TR/TD structure elements) onto one `a4_page` per `PageNode` (each successor opened by `new_page_same_size` so a multi-page UA document never overflows one buffered page), sealed by `done().build()`; `PDF_TYPST`/`TYPST_DATA` fold `to_typst_source(node).encode()` through the held `Compiler.compile_with_warnings(input=, sys_inputs=, pdf_standards=, timestamp=)`; `TYPST_QUERY` folds the selector through the free `typst.query(input=source-bytes, selector, field, one)` and `TYPST_EVAL` the source expression through the free `typst.eval(input=source-bytes, expression)`; render folds each page through pymupdf `get_pixmap`→native `tobytes`/`numpy`; raster folds each page through pypdfium2 `render(**render_keywords)`→`to_numpy` into one `numpy` frame stack and harvests the `get_toc(max_depth)`→`PdfBookmark.get_count`/`get_title`/`get_dest`→`PdfDest.get_index` outline as `OutlineRow` triples; assembly runs pypdf `PdfWriter.append`; repair runs pikepdf `Pdf.save`; font-embed registers each `CONFORM`-subsetted face through reportlab `TTFont`+`registerFont` and draws every `RunNode`; `DOCX` folds the tree through `_docx_block` after `_docx_scaffold` configures the primary `Section` (orientation/margins + the running header/footer title-block) and registers one `Styles.add_style` caption style, each `TableNode` styling the `THead` band bold, setting `WD_CELL_VERTICAL_ALIGNMENT`, and folding `spans` onto `_Cell.merge`; `PPTX` folds each `PageNode` into a slide; `DOCX_TEMPLATE` folds the tree in one `walk`+`match` pass into the role-keyed `render` context reading `get_undeclared_template_variables` onto the fact; XLSX folds the `TableNode` grid through the ONE `_xlsx_write` body keyed on the `XlsxRegime` policy — the `constant_memory` stream (freeze+autofilter) or the in-memory `add_table`/`conditional_format` (or `_xlsx_merge` `merge_range` where the grid carries merges) pass, plus the `set_landscape`/`set_header`/`set_footer` print layout; `ODT`/`ODS` fold the deepened `_odf_emit` — `_odf_scaffold` registers the shared `odf.number` `DateStyle` + RTL `ParagraphProperties` style and authors the `dc.Title`/`dc.Creator` metadata seal, `_odf_style` caches one appearance-keyed `text:style` per run bound through `OdfSpan`, `_odf_block` lowers `H`/`P`/`text:list`/`text:a` links/`text:note` footnotes/`draw:frame` figures (`addPicture`), and `_odf_sheet` lowers each `TableNode` into a `TableHeaderRows` band + typed `TableCell` (`odf_cell` `boolean`/`float`/`date` value attrs) + `numbercolumnsspanned`/`numberrowsspanned` with `CoveredTableCell` placeholders, each serialized once through `OpenDocument.write`; structured-text folds through ruamel-yaml `YAML().dump`, tomlkit `dumps`, and the four gated lxml rows under the `XMLParser(resolve_entities=False, huge_tree=False, no_network=True)` hardening; the plain-text manuscript rows fold `to_markdown(node)`/`to_latex(node)` — the CommonMark/GFM diffable and journal-submission LaTeX typeset egress of the SAME bound tree the PDF/HTML/Typst arms lower, each escaping through the model owner's `_MD_ESCAPE`/`_LATEX_ESCAPE` maketrans so no active char breaks the source.
- Receipt: the `@receipted(OPEN)` harvest weave stacks over the pure `_emit`, draining `DocumentPlan.contribute` and emitting through `Signals.emit_async`; `contribute` reads the threaded `EmitFact` off `self.fact` (never an in-process re-run of a worker-gated arm), re-mints the content key over `fact.data`, and folds the case off the `Backend.kind` `ReceiptKind` discriminant through the `_RECEIPT` table in one expression — the PDF rows mint `core/receipt#RECEIPT` `ArtifactReceipt.Pdf(key, bytes, pages)`, the Office and structured-text rows mint `ArtifactReceipt.Office(key, bytes)` — the exact `(ContentKey, <scalars>)` arities the receipt owner declares, never a widened tuple and never the typography-rail `Document` case (emit mints only the `Pdf`/`Office` arities). The rich per-arm evidence (render scale, outline count, embedded-face set, `get_undeclared_template_variables` set, queried length, validation verdict, warning/error count) rides the `EmitFact` carrier the consumer reads off `self.fact`; a richer byte-only receipt case is a `core/receipt#RECEIPT` growth concern — the `Document` case is the typography rail's, so emit mints only the `Pdf`/`Office` arities it owns.
- Growth: a new document format is one `DocumentMode` row plus one `Backend` row binding its arm and band, plus a `_REQUIRED` row if it demands an input (the pdf_oxide `PDF_OXIDE`/`PDF_UA` create pair landed exactly as this — two `DocumentMode` members, two `CORE` `Backend` rows, no interior edit; the `MARKDOWN`/`LATEX` manuscript pair landed the same way — two members, two `CORE` rows binding `_markdown_emit`/`_latex_emit` over the `document/model#NODE` `to_markdown`/`to_latex` node lowerings, with no `_REQUIRED` row since neither takes an external input); a new typed cell one `CellValue.odf_cell`/`write_xlsxwriter` arm; a new interactive-field kind one `FieldKind` member plus one `_ua_field` arm over the pdf_oxide form vocabulary; a new archival/accessible profile one `PdfVariant` row projecting through `_PDF_STANDARD`/`_PDF_PROFILE`/`_ACCESSIBLE` to both engines; a new schema dialect one `SchemaKind` row; a new sheet-furniture or layout knob one `EmitSpec`/`EmitPayload` policy-value field (`toc`/`header_text`/`footer_text`/`landscape`); a PDF/A-3 embedded source file one `EmitSpec.attachments` entry; a new evidence fact one `EmitFact` field (`tagged`/`structure`/`merges`/`footnotes`); a new admission cause one `EmitFault` case; the held `Compiler` amortizes font load across a batched `produced`; each `PageNode` opens its own tagged sheet through the pdf_oxide `new_page_same_size` fold so a multi-page UA document never overflows one buffered page; zero new surface.

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
from anyio import CapacityLimiter, to_process, to_thread
from builtins import frozendict
from expression import Error, Ok, Result, case, tag, tagged_union
from expression.collections import Block
from msgspec import Struct, field, to_builtins
from pydantic import TypeAdapter, ValidationError

from rasm.runtime.content_identity import ContentIdentity, ContentKey
from rasm.runtime.faults import RuntimeRail, async_boundary
from rasm.runtime.receipts import OPEN, Receipt, receipted

from artifacts.core.receipt import ArtifactReceipt
from artifacts.document.model import (
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
lazy from reportlab.platypus import BaseDocTemplate, Frame, Image, ListFlowable, ListItem, PageBreak, PageTemplate, Paragraph, Spacer, Table, TableStyle
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
    PDF_OXIDE = "pdf-oxide"       # commercial-safe (MIT/Apache) HTML->PDF create — pdf_oxide `Pdf.from_html`
    PDF_UA = "pdf-ua"             # native tagged PDF/UA-1 fluent author — pdf_oxide `DocumentBuilder`
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
    MARKDOWN = "markdown"         # CommonMark/GFM plain-text manuscript — `document/model#NODE` `to_markdown` node lowering
    LATEX = "latex"               # journal-submission LaTeX manuscript — `document/model#NODE` `to_latex` node lowering


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
    A_2A = "a-2a"    # PDF/A-2 accessible: archival AND tagged — the both-telos preservation+accessibility union
    A_3A = "a-3a"    # PDF/A-3 accessible: archival + tagged + embedded source files (the AEC deliverable seal)
    A_4 = "a-4"      # PDF/A-4 (PDF 2.0 archival)
    UA_1 = "ua-1"

    @property
    def typst(self) -> tuple[str, ...]:
        # the typst `PDFStandard` matrix spells every token below verbatim; the accessible `-a` rows
        # combine their archival token with `ua-1` so one render is BOTH conformant and screen-readable.
        return _PDF_STANDARD.get(self, ())

    @property
    def weasyprint(self) -> str | None:
        # weasyprint ships only the `-b` conformance levels + `a-4`/`ua-1`; an `-a` variant projects to
        # its `-b` profile and the `tagged` flag below sets `pdf_tags=True`, so the emitted tagged
        # PDF/A-Xb carries the accessible structure tree the `-a` conformance level mandates.
        return _PDF_PROFILE.get(self)

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
_OFFLOAD: Final = CapacityLimiter(8)  # bounds the in-process native-render thread fan-out; a CORE arm never runs inline on the loop
_BOLD_WEIGHT: Final = 700
_PT_PER_HALFPT: Final = 2
_HEADER_WIDTH: Final = 18.0
_NUM_FORMAT: Final = "0.############"
_DATE_FORMAT: Final = "yyyy-mm-dd hh:mm:ss"
_BOOL_CELL: Final[frozendict[str, bool]] = frozendict({"true": True, "false": False, "yes": True, "no": False})
# the reportlab intra-`Paragraph` markup escaper: run text is translated ONCE at the boundary before any
# `<b>`/`<font>`/`<a>` tag is composed, so a run carrying `<`/`&`/`"` never breaks the mini-XML parser — the
# TEMPLATE-SAFETY escape-then-compose seam the model's `_TYPST_ESCAPE` mirrors for the Typst lowering.
_MARKUP_ESCAPE: Final = str.maketrans({"&": "&amp;", "<": "&lt;", ">": "&gt;", '"': "&quot;"})
# the typst `PDFStandard` token tuple per variant — the archival token alone for `-b`/`a-4`, the archival
# token PLUS `ua-1` for the accessible `-a` and `ua-1` rows so one render is conformant AND screen-readable.
_PDF_STANDARD: Final[frozendict[PdfVariant, tuple[str, ...]]] = frozendict({
    PdfVariant.A_1B: ("a-1b",), PdfVariant.A_2B: ("a-2b",), PdfVariant.A_3B: ("a-3b",), PdfVariant.A_4: ("a-4",),
    PdfVariant.A_2A: ("a-2a", "ua-1"), PdfVariant.A_3A: ("a-3a", "ua-1"), PdfVariant.UA_1: ("a-3b", "ua-1"),
})
# the weasyprint `pdf_variant` profile per variant — only `-b`/`a-4`/`ua-1` ship, so the accessible `-a`
# rows project to their `-b` profile and ride `pdf_tags=True` (the `tagged` flag) for the structure tree.
_PDF_PROFILE: Final[frozendict[PdfVariant, str]] = frozendict({
    PdfVariant.A_1B: "pdf/a-1b", PdfVariant.A_2B: "pdf/a-2b", PdfVariant.A_3B: "pdf/a-3b", PdfVariant.A_4: "pdf/a-4",
    PdfVariant.A_2A: "pdf/a-2b", PdfVariant.A_3A: "pdf/a-3b", PdfVariant.UA_1: "pdf/ua-1",
})
# the tagged (structure-tree-bearing) variants: the `-a` accessible levels and `ua-1` set `pdf_tags=True`.
_ACCESSIBLE: Final[frozenset[PdfVariant]] = frozenset({PdfVariant.A_2A, PdfVariant.A_3A, PdfVariant.UA_1})
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

    def odf_cell(self) -> tuple[str, frozendict[str, object], str]:
        # the typed OASIS cell — `(office:value-type, its value attributes, the `<text:p>` display run)`: bool and
        # date now key their own `booleanvalue`/`datevalue` attribute rather than collapsing to a `string` cell, so
        # the `date` rows bind the `odf.number` `DateStyle` and the sheet round-trips as typed data, not display text.
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
    outline_count: int = 0                    # reportlab `addOutlineEntry`/TOC entries or the pypdfium2 bookmark count
    faces: tuple[str, ...] = ()
    undeclared: tuple[str, ...] = ()
    template_path: str = ""
    queried: int = 0
    valid: bool = True
    warnings: int = 0
    errors: int = 0
    outline: tuple[OutlineRow, ...] = ()
    tagged: bool = False                      # PDF/UA structure tree emitted (pdf_oxide `tagged_pdf_ua1`, weasyprint `pdf_tags`)
    structure: int = 0                        # tagged structure elements or `role_map` foreign->standard rows
    merges: int = 0                           # merged/spanned office cells folded from `TableNode.spans`
    footnotes: int = 0                        # authored `text:note` footnotes / docx comments


class EmitSpec(Struct, frozen=True, omit_defaults=True):
    source: bytes = b""
    password: str = ""
    title: str = ""
    author: str = ""
    subject: str = ""
    toc: bool = False                                          # reportlab multiBuild table-of-contents + PDF outline from heading flowables
    header_text: str = ""                                      # title-block running header (reportlab onPage sheet furniture, docx/odf section header)
    footer_text: str = ""                                      # title-block running footer; the page-number field renders per page
    landscape: bool = False                                    # page orientation for the reportlab/docx/xlsx print layout
    variant: PdfVariant = PdfVariant.NONE
    forms: bool = False
    output_intent: str = ""                                    # weasyprint PDF/A ICC output-intent profile path
    base_url: str = ""                                         # weasyprint relative-resource resolution root
    stylesheets: tuple[str, ...] = ()                          # weasyprint supplemental CSS paths
    attachments: frozendict[str, str] = field(default_factory=frozendict)  # PDF/A-3 embedded source files: display-name -> file path, the `/Source`-relationship AEC deliverable seal the A_3A variant requires
    full_fonts: bool = False                                   # weasyprint embed complete fonts (archival fidelity) instead of subsetting
    optimize_images: bool = False                              # weasyprint recompress embedded raster images
    presentational_hints: bool = False                         # weasyprint honor legacy HTML presentational attributes
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

    @receipted(OPEN)  # emit facts carry no classified field, so the runtime keep-all `OPEN` rides directly — never a re-minted per-file `Redaction` the observability owner forbids, exactly as `document/egress#FINISH` rides `OPEN`
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
    # every text arm lowers through `_run_markup`/`_runs_markup` so a `Paragraph` carries the run's real
    # weight/italic/colour/baseline/decoration and its `<`/`&` are escaped — the journal-typesetting fidelity
    # the naive plain-`run.text` splice lost; `QUOTE` and inline `LINK` are their own arms, never Normal.
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
            yield ListFlowable(entries, bulletType="1" if kind is ListKind.ORDERED else "bullet", start=node.start if kind is ListKind.ORDERED else None)
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
    return "".join(run.text for run in walk(node) if isinstance(run, RunNode))  # plain text for docx/xlsx/UA-linearize; the reportlab arm uses the markup form below


def _run_markup(run: RunNode, /) -> str:
    # the run's full character appearance as escaped reportlab intra-`Paragraph` markup: colour/decoration/
    # baseline/italic/weight, the RUN_FIDELITY the docx/odf/typst arms carry and the naive plain-`run.text`
    # splice dropped. Font-face markup is withheld — `PDF_AUTHOR` registers no per-run face, so `<font name>`
    # would reference an unregistered font, while colour/weight/style ride safely; `text` is escaped ONCE.
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
    # the drawing-schedule/journal-table primitive: a real `TableStyle` grid with a shaded bold `THead` band,
    # a `TFoot` band, cell padding/valign, and `SPAN` merges folded from `TableNode.spans` — the naive
    # unstyled `Table([[str, ...]])` is the deleted form. `repeatRows` reprints the header on every page.
    data = [[Paragraph(_node_markup(cell), styles["BodyText"]) for cell in row] for row in node.rows]
    head, foot, last = node.header_rows, node.footer_rows, len(node.rows) - 1
    commands: list[tuple[object, ...]] = [
        ("GRID", (0, 0), (-1, -1), 0.5, colors.grey),
        ("VALIGN", (0, 0), (-1, -1), "MIDDLE"),
        ("LEFTPADDING", (0, 0), (-1, -1), 4), ("RIGHTPADDING", (0, 0), (-1, -1), 4),
        ("TOPPADDING", (0, 0), (-1, -1), 2), ("BOTTOMPADDING", (0, 0), (-1, -1), 2),
        *([("BACKGROUND", (0, 0), (-1, head - 1), colors.HexColor("#E8E8E8")), ("FONTNAME", (0, 0), (-1, head - 1), "Helvetica-Bold")] if head else ()),
        *([("BACKGROUND", (0, last - foot + 1), (-1, -1), colors.HexColor("#F4F4F4"))] if foot else ()),
        *_span_commands(node.spans),
    ]
    return Table(data, repeatRows=head or 0, style=TableStyle(commands))


class _SheetDocTemplate(BaseDocTemplate):
    # the multi-pass sheet builder: `afterFlowable` reads each heading flowable's `HeadingN` style, notifies
    # the `TableOfContents` and mints the PDF outline (`bookmarkPage` + `addOutlineEntry`) at the settled page,
    # so `multiBuild` resolves the TOC page numbers and the bookmark tree in one owner — never a hand-computed
    # page number. `outline` counts the emitted entries for the `EmitFact`.
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
    # the ISO 7200 title-block onPage furniture: running header, footer, and the per-page number painted on the
    # canvas outside the content frame; a closure over the spec so the same callback serves first and later pages.
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
    doc = _SheetDocTemplate(
        sink, pagesize=size, title=plan.spec.title or plan.node.meta.key.hex, author=plan.spec.author, subject=plan.spec.subject,
    )
    frame = Frame(20 * mm, 18 * mm, size[0] - 40 * mm, size[1] - 34 * mm, id="body")
    doc.addPageTemplates([PageTemplate(id="sheet", frames=[frame], onPage=_title_block(plan.spec, plan.node.meta.key.hex))])
    toc = TableOfContents(levelStyles=[ParagraphStyle(name=f"TOC{n}", fontSize=11 - n, leftIndent=12 * n) for n in range(3)])
    story = [toc, PageBreak(), *_flowables(plan.node, styles, plan.spec)] if plan.spec.toc else list(_flowables(plan.node, styles, plan.spec))
    doc.multiBuild(story) if plan.spec.toc else doc.build(story)  # multiBuild resolves the TOC/outline page numbers across passes
    return EmitFact(data=sink.getvalue(), pages=doc.page, outline_count=doc.outline)


def _weasyprint_html(plan: DocumentPlan, _world: "Compiler | None", /) -> EmitFact:
    spec = plan.spec
    # PDF/A-3 embeds each source file as an `/AFRelationship /Source` attachment — the "embedded source files"
    # the `A_3A` variant names as the AEC deliverable seal, which the prior arm asserted but never passed (an
    # illusory-capability defect); `full_fonts`/`optimize_images`/`presentational_hints` are the archival controls.
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
    # commercial-safe (MIT/Apache) HTML->PDF create — the license-clean sibling of the AGPL/full-CSS weasyprint
    # arm for the closed/distributed path. Lowers `to_html(node)` and serializes the `Pdf` value; no open handle.
    pdf = Pdf.from_html(to_html(plan.node), title=plan.spec.title or plan.node.meta.key.hex, author=plan.spec.author)
    return EmitFact(data=pdf.to_bytes(), pages=len(pdf))


def _ua_field(page: object, node: FieldNode, /) -> object:
    # lower a `FieldNode` onto the fluent page as a native AcroForm widget positioned from `NodeMeta.bounds`:
    # the pdf_oxide `FluentPageBuilder` owns the full form vocabulary (`text_field`/`checkbox`/`combo_box`/
    # `signature_field`/`push_button`), so the model's interactive-form nodes reach the tagged PDF as real
    # fields — the AEC title-block fill-in and journal-form capability the prior UA flow silently dropped.
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
    # one flowing tagged-block arm over the fluent page builder: section titles + headings/paragraphs auto-flow
    # into the structure tree, a figure emits an alt-carrying image (the PDF/UA alt-text requirement), a
    # `FieldNode` an AcroForm widget, a `TableNode` a NATIVE tagged `Table` (real THead/TR/TD structure elements,
    # never tab-joined text that erases the table's accessibility — the both-telos schedule/journal table); the
    # builder returns `self`, so the caller threads the successor page.
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
    # native tagged PDF/UA-1 authoring — the accessibility-first create path the weasyprint `pdf_tags` flag only
    # approximates. `tagged_pdf_ua1` mints the structure tree, `role_map` binds each foreign `StructureNode` role
    # to its canonical standard element (`document/model#NODE` `standard_for`), embedded faces register once, and
    # each `PageNode` flows onto its OWN sheet through `_ua_block` — the first `a4_page`, every subsequent
    # `new_page_same_size` — so a multi-page UA document never overflows one buffered page; sealed by `done().build()`.
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
    compiler = world if world is not None else plan.world()  # held world amortizes fonts; a non-Typst-headed batch falls back per-plan
    # PDF/UA variants require a document title (a `ua-1` render hard-errors `missing document title`); the
    # `document/model#NODE` `to_typst_source(node, title=)` owns the escaped `#set document(title:)` set-rule
    # through its STRING-context `_typst` escaper, so this seam never hand-rolls the markup escape (TEMPLATE_STRUCTURE_SITE).
    title = (plan.spec.title or plan.node.meta.key.hex) if plan.spec.variant.tagged else None
    source = to_typst_source(plan.node, title=title)
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
    # register the recurring caption style once (`Styles.add_style` — never per-run duplication) and configure the
    # primary section: orientation, margins, and the running header/footer title-block furniture the spec supplies.
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


def _odf_text[E](element: E, text: str, /) -> E:
    addTextToElement(element, text)  # owns the whitespace-correct `<text:s>`/`tab`/`line-break` split
    return element


def _odf_style(document: object, cache: dict[object, str], run: RunNode, /) -> str:
    # one appearance-keyed `text:style` registered once in `automaticstyles` and bound by name — never a per-run
    # style duplication. The key is the FULL run appearance — weight/italic/script/color/decorations PLUS the
    # `fontname`/`fontsize` face and the `fo:language`/`fo:country` locale — so two runs identical in every
    # rendered attribute share one row while a face/size/locale difference keys its own; a face-blind key would
    # collapse distinct fonts onto one row and drop the run fidelity the docx/typst siblings and the odfpy
    # catalogue both carry. `NodeMeta.lang` (a BCP-47 tag when present) splits into `fo:language`/`fo:country`.
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
    # register the shared ISO-8601 `DateStyle` and the RTL paragraph style once, then author the Dublin-Core
    # metadata seal — the descriptive-metadata/provenance surface the OASIS office plane grades against.
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
    # the tabular lowering: a leading `TableHeaderRows` band, typed `TableCell`s bound to the `RasmDate`
    # data-style, and `TableNode.spans` folded onto `numbercolumnsspanned`/`numberrowsspanned` with a
    # `CoveredTableCell` placeholder under every merged position.
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
    # fold each `TableNode.spans` merge onto `merge_range` with the running row offset of concatenated tables;
    # the top-left cell keeps its display value, the covered cells blank — the non-streaming regime only.
    offset, count = 0, 0
    for table in (n for n in walk(node) if isinstance(n, TableNode)):
        for row, col, col_span, row_span in table.spans:
            if col_span > 1 or row_span > 1:
                top = table.rows[row][col] if row < len(table.rows) and col < len(table.rows[row]) else None
                sheet.merge_range(offset + row, col, offset + row + row_span - 1, col + col_span - 1, _text(top) if top is not None else "", header_fmt)
                count += 1
        offset += len(table.rows)
    return count


def _xlsx_write(plan: DocumentPlan, grid: list[tuple[CellValue, ...]], regime: XlsxRegime, /) -> EmitFact:
    # ONE xlsxwriter body keyed on the regime — the O(1) `constant_memory` stream and the rich in-memory pass are a
    # policy value, not two writers (openpyxl's write-only path is superseded for the write concern). The streamed
    # arm freezes+autofilters the header; the in-memory arm gains the structured features the streaming flush cannot
    # back-patch: a banded `add_table` (or `merge_range` spans where the grid carries merges) plus a data-bar
    # `conditional_format`. Page-setup (`set_landscape`/`set_header`/`set_footer`) serves the print-layout plane.
    streamed = regime is XlsxRegime.STREAMED
    sink = io.BytesIO()
    book = xlsxwriter.Workbook(sink, {
        "constant_memory": streamed, "in_memory": plan.spec.in_memory, "use_zip64": len(grid) >= _ZIP64_ROW_THRESHOLD,
        "remove_timezone": True, "nan_inf_to_errors": True,  # `_coerce_cell` admits tz-aware datetimes and `float('inf')`
    })
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
        sheet.autofilter(0, 0, last, width - 1) if merges else sheet.add_table(0, 0, last, width - 1, {"header_row": headed, "autofilter": True, "banded_rows": True})
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

_SPREADSHEET_POLICIES: Final[frozendict[XlsxRegime, SpreadsheetPolicy]] = frozendict({
    XlsxRegime.IN_MEMORY: SpreadsheetPolicy(XlsxRegime.IN_MEMORY, _STREAMING_ROW_THRESHOLD),
    XlsxRegime.STREAMED: SpreadsheetPolicy(XlsxRegime.STREAMED, 1 << 62),
})

_HELD_WORLD: Final[frozenset[DocumentMode]] = frozenset({DocumentMode.PDF_TYPST, DocumentMode.TYPST_DATA})

_RECEIPT: Final[frozendict[ReceiptKind, Callable[[ContentKey, EmitFact], "ArtifactReceipt"]]] = frozendict({
    ReceiptKind.PDF: lambda key, fact: ArtifactReceipt.Pdf(key, len(fact.data), fact.pages),
    ReceiptKind.OFFICE: lambda key, fact: ArtifactReceipt.Office(key, len(fact.data)),
})

BACKENDS: Final[frozendict[DocumentMode, Backend]] = frozendict({
    DocumentMode.PDF_AUTHOR: Backend(Band.CORE, _reportlab_author, ReceiptKind.PDF),
    DocumentMode.PDF_HTML: Backend(Band.CORE, _weasyprint_html, ReceiptKind.PDF),
    DocumentMode.PDF_OXIDE: Backend(Band.CORE, _pdf_oxide_create, ReceiptKind.PDF),  # pdf_oxide Rust core is GIL-releasing -> CORE thread lane
    DocumentMode.PDF_UA: Backend(Band.CORE, _pdf_ua_build, ReceiptKind.PDF),
    DocumentMode.PDF_TYPST: Backend(Band.CORE, _typst_compile, ReceiptKind.PDF),
    DocumentMode.TYPST_QUERY: Backend(Band.CORE, _typst_query, ReceiptKind.OFFICE),
    DocumentMode.TYPST_EVAL: Backend(Band.CORE, _typst_eval, ReceiptKind.OFFICE),
    DocumentMode.TYPST_DATA: Backend(Band.CORE, _typst_compile, ReceiptKind.PDF),
    DocumentMode.PDF_RENDER: Backend(Band.CORE, _pymupdf_render, ReceiptKind.PDF),  # AGPL: native-codec encoded raster; superseded on the commercial-safe path by pdf_oxide `render_page` (roster-flagged for final reconciliation)
    DocumentMode.PDF_RASTER: Backend(Band.CORE, _pypdfium2_raster, ReceiptKind.PDF),  # Apache/BSD: the commercial-safe Pillow-free numpy-frame-stack raster axis
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
    DocumentMode.MARKDOWN: Backend(Band.CORE, _markdown_emit, ReceiptKind.OFFICE),  # pure-Python `to_markdown` node fold — no external input, so no `_REQUIRED` row
    DocumentMode.LATEX: Backend(Band.CORE, _latex_emit, ReceiptKind.OFFICE),        # pure-Python `to_latex` node fold — no external input, so no `_REQUIRED` row
})
```

## [03]-[RESEARCH]

- [PHANTOM_ARMS_REALIZED]: the prior fence bound `_reportlab_author`/`_pikepdf_repair`/`_docx_emit`/`_pptx_emit` in `BACKENDS` but DEFINED NONE of them — four modes were a `NameError` at module load, a stub dressed as a complete table. The rebuild realizes each as a real lowering arm: `_reportlab_author` folds the tree through `_flowables` into a `platypus` `BaseDocTemplate`+`Frame`+`PageTemplate` sheet built by `multiBuild`/`build` (`Paragraph`/`_table_flowable`/`Image`/`ListFlowable`/`PageBreak` per node kind, the `getSampleStyleSheet` heading/normal/code styles), `_docx_emit` folds it through `_docx_block` (`add_heading`/`add_paragraph`+`add_run`/`add_table`+`cell`/`add_picture`/`add_page_break`) with the full `_docx_run` font projection, `_pptx_emit` folds each `PageNode` into a blank-layout slide's textbox `TextFrame` plus `add_picture` figures, and `_pikepdf_repair` opens and re-saves through `Pdf.save(linearize=True, recompress_flate=True, object_stream_mode=generate, deterministic_id=True)`. The `reportlab.platypus`/`lib.styles`/`lib.pagesizes`, `docx.Document`/`add_*`/`Run.font`/`shared.Pt`/`RGBColor`, `pptx.Presentation`/`slide_layouts`/`slides.add_slide`/`SlideShapes.add_textbox`/`add_picture`/`util.Inches`/`Pt`, and `pikepdf.open`/`Pdf.save`/`ObjectStreamMode`/`Pdf.pages` spellings verify against the `reportlab`/`python-docx`/`python-pptx`/`pikepdf` catalogues.
- [RECEIPT_WEAVE]: the prior fence wrote `@receipted(ArtifactReceipt)` and `_emit -> ContentKey` — both wrong against the runtime `receipts.@receipted` contract, which takes a `Redaction` policy value and harvests `contribute()` off the operation's `ReceiptContributor` return. The rebuild matches the runtime owner and the `document/egress#FINISH`/`document/report#REPORT` siblings: `@receipted(OPEN)` over a thin `_emit -> Self` returning the stepped `DocumentPlan` (a `ReceiptContributor` via `contribute`), so the weave drains `self.contribute()` and emits through `Signals.emit_async`, and `_emitted` mints the content key off `plan.fact.data` rather than the pure core. `contribute` reads the threaded `EmitFact` off `self.fact`, folds the `Backend.kind` discriminant through `_RECEIPT`, and mints the real `core/receipt#RECEIPT` `ArtifactReceipt.Pdf(key, bytes, pages)`/`.Office(key, bytes)` arities — never a widened tuple, the typography-rail `Document` case, or an in-process re-run of a worker-gated arm.
- [TYPED_PAYLOAD]: the prior fence carried `extra_items=str` while `of` did `EmitSpec(**payload)` with NO band-fold, so any extension key was either a `TypeError` (unknown kwarg) or a wrong-typed `str` against a non-`str` field — the `extra_items` band the prose claimed was illusory. The rebuild closes the payload (`TypedDict(closed=True)` over the full settable `EmitSpec` surface, per-key `NotRequired[ReadOnly[...]]`), admits once through the module-level `TypeAdapter`, and materializes `EmitSpec(**payload)` over a known-key set; an unknown key faults at `validate_python`. The `_REQUIRED` `frozendict[DocumentMode, tuple[str, ...]]` precondition is the two-tier admission gate making the interior total — a mode whose named input (`source`/`template`/`schema`/`selector`/`subset_fonts`/`stylesheet`/`path`) is empty becomes `EmitFault.unsatisfied` at `of`, so no arm reaches an empty `DocxTemplate("")` or `etree.XSLT(b"")`.
- [FAULT_COLLAPSE]: the prior `EmitFault` carried five cases (`payload`/`source`/`schema`/`content`/`typeset`) but the body CONSTRUCTED only `payload` — four decorative provider-conversion cases the arms never minted (the arms `raise` provider exceptions; nothing built `EmitFault(typeset=...)`). The rebuild collapses to the admission vocabulary `of` actually produces — `payload` (rejected keys) plus `unsatisfied` (mode precondition) — and routes every arm-level provider raise (`TypstError`/`PdfiumError`/`XMLSyntaxError`/`XlsxWriterException`) to the runtime `BoundaryFault` at the `async_boundary` capsule, exactly as `document/egress#FINISH` routes its provider raises rather than re-spelling a per-page provider-fault union the runtime already owns.
- [TYPST_WORLD]: three Typst defects are fixed. (1) `world()` constructed `Compiler(font_paths=[], sys_inputs=...)` with NO `input` — but the `typst` `Compiler` constructor requires `input`; the rebuild seeds it with `to_typst_source(self.node).encode()` (the held world amortizes font load, each arm overriding `input=` per compile). (2) `_typst_compile`/`_typst_query` passed a markup `str` as `input`, which the binding treats as a file PATH (`input` is "source path or bytes"); the rebuild passes `.encode()` bytes. (3) `_typst_query` used the held HEAD world's `Compiler.query`, querying the wrong document in a batch; the rebuild uses the free `typst.query(input=this-plan-source-bytes, selector, field, one)` which compiles a single-shot world over the querying plan's OWN document, so `TYPST_QUERY` leaves `_HELD_WORLD`. The PDF/UA path prepends `#set document(title:)` (the `ua-1` render hard-errors `missing document title` without it), and `timestamp` pins the creation date for byte-deterministic archival output. `Compiler`/`compile_with_warnings(input, sys_inputs, pdf_standards, timestamp)` and the free `query` verify against the `typst` catalogue.
- [OUTLINE_VERIFIED]: the prior `_outline_rows` read `mark.level` — a PHANTOM the `pypdfium2` catalogue does not row (a 5.x `PdfBookmark` exposes `get_title`/`get_count`/`get_dest`, NOT a `.level` attribute), and the prose's claim that it "was corrected to `max_depth - get_count()`" was never applied to the code. The rebuild reads the verified `mark.get_count()` (the signed sub-item count) as the `OutlineRow` head, `mark.get_title()`, and `dest.get_index()` off `mark.get_dest()`, so `outline_count` is the verified bookmark count (`len(outline)`) and the `outline` triples carry the per-bookmark `get_count`/title/dest — never a `.level` depth the `PdfBookmark` does not expose. `get_toc(max_depth)`/`PdfBookmark.get_count`/`get_title`/`get_dest`/`PdfDest.get_index` verify against the `pypdfium2` catalogue rows.
- [RUN_FIDELITY]: the prior `_docx_run_props` mapped only `bold`/`size`/`font`/`rtl`/`url_id`, lowering a `RunNode` that `document/model#NODE` carries as bold-italic-superscript-coloured-language-tagged to plain bold — the exact defect the model page's accessibility rebuild fixed, repeated at emission. The rebuild projects the full `docxtpl` `RichText.add` axis — `italic` from `RunNode.italic`, `superscript`/`subscript` from `RunScript`, `color` from the `Rgb` triple, `lang` from `NodeMeta.lang` — and the `_docx_emit`/`_pptx_emit`/`_font_embed` arms carry the same character appearance (`Run.font.color`, `Font.italic`, `Canvas.setFillColorRGB`). `RichText.add(italic, superscript, subscript, color, rtl, lang)`, `python-docx` `Run.font`, and `reportlab` `setFillColorRGB` verify against the catalogues.
- [DOCX_DENSITY]: the dead `_REPLACE` constant (the `("media", "embedded", "zipname", "pic")` tuple referencing methods the body never called) is gone — the `replace: frozendict[str, str]` `EmitSpec` field now wires the real `DocxTemplate.replace_zipname(name, dst)` pre-render part swap, and `render_footnotes` (after `render`) binds the footnote-part XML the prior fence dropped. The `_FORMAT_SETTERS` `getattr`-setter loop collapses into `_FORMATS` `add_format({...})` property-dict rows. `replace_zipname`/`render_footnotes`/`add_format(properties)` verify against the `docxtpl`/`xlsxwriter` catalogues.
- [WEASY_ARCHIVAL]: the HTML→PDF arm gains the archival-correctness surface its catalogue rows — `base_url` (relative-resource resolution the catalogue mandates over string concatenation), `output_intent` (the ICC profile PDF/A conformance requires), `stylesheets` (supplemental `CSS`), `attachments=[Attachment(filename, name, relationship="Source")]` (the PDF/A-3 embedded source files the `A_3A` variant comment named as the AEC deliverable seal but the prior arm never passed — an illusory-capability defect now closed against the `weasyprint` catalogue `write_pdf(attachments=)`/`Attachment(relationship=)` rows), and the `full_fonts`/`optimize_images`/`presentational_hints` archival-fidelity controls — folded beside the existing `PdfVariant` `pdf_variant`/`pdf_tags`/`pdf_forms` row, and `PdfVariant` gains the `A_1B` profile both engines support (`pdf/a-1b`/`("a-1b",)`). `PdfVariant` additionally rows `A_4` — the weasyprint catalogue's `pdf/a-4` (the PDF 2.0-based archival standard typst does not yet accept), so the `.typst`/`.weasyprint` projections read through `_PDF_STANDARD.get(self, ())`/`_PDF_PROFILE.get(self)` and each engine renders the strongest profile it supports rather than a `KeyError` on a one-engine row; the prior `NONE`-special-cased branches collapse into the defaulted `.get`. `HTML(string, base_url)`/`write_pdf(output_intent, stylesheets)`/`CSS(filename)` and the `pdf/a-4` row verify against the `weasyprint` catalogue.
- [TYPST_EVAL]: the introspection half rowed `TYPST_QUERY` (element selection) but not the catalogue's distinct `eval` (expression evaluation), so `TYPST_EVAL` lands as one `DocumentMode` row over `_typst_eval` — the free `typst.eval(input=this-plan-source-bytes, expression)` single-shot world over the plan's OWN document, the scripting-introspection sibling of `query` and the same `ReceiptKind.OFFICE` `(ContentKey, bytes)` terminal — with `expression` the one `EmitSpec`/`EmitPayload` field its `_REQUIRED` row demands. `eval(input, expression)` verifies against the `typst` catalogue.
- [CELL_FOLD]: `_coerce_cell` ran a per-cell speculative `try/except ValueError` ladder (three parsers attempted in sequence) — the NO-EXCEPTION-HOTLOOP smell, exception flow deciding the cell type once per cell across a whole table grid. It collapses to one module-level compiled `_CELL_SHAPE` classifier whose named alternatives (`int`/`float`/`dt`/`date`) decide the type in a single total `match` over the trimmed text, then `_CELL_TYPED[matched.lastgroup]` dispatches the named total parser: the `int`/`float` arms are regex-guaranteed so they never raise, and only a genuine ISO-shape string reaches the lone `_temporal` guard whose single boundary `try/except` lets a shape-valid but range-invalid stamp (month 13) fall back to raw text — so the common text/number/bool/empty cell never enters an exception path and the speculative ladder is gone. The same cell algebra admits tz-aware `datetime.fromisoformat` results and `float('inf')`, so the `_xlsx_streamed` workbook options carry the catalogue's `remove_timezone=True` and `nan_inf_to_errors=True` so a tz-aware datetime serializes and a non-finite float maps to an Excel error code rather than raising. The `re.Pattern.match`/`Match.lastgroup` spelling is stdlib and `remove_timezone`/`nan_inf_to_errors` verify against the `xlsxwriter` catalogue.
- [RUN_FIDELITY_AUTHOR]: the reportlab `PDF_AUTHOR`/`_flowables` arm — the PRIMARY journal-typesetting PDF path — emitted `Paragraph("".join(run.text for run in runs))`, DROPPING every `RunNode` weight/italic/colour/`RunScript`/`TextDecoration` (the exact RUN_FIDELITY defect the docx `_docx_run_props` rebuild fixed, repeated on the richest path) AND splicing raw `<`/`&` into reportlab's mini-XML `Paragraph` parser (a TEMPLATE-SAFETY hole). The rebuild lowers each run through `_run_markup` — `run.text.translate(_MARKUP_ESCAPE)` escaped ONCE at the boundary, then composed into `<b>`/`<i>`/`<super>`/`<sub>`/`<u>`/`<strike>`/`<font color>` by appearance (`<font name>` withheld since `PDF_AUTHOR` registers no per-run face, `<overline>` absent from reportlab) — the escape-then-compose seam the model's `_styled` Typst lowering mirrors; `_runs_markup`/`_node_markup` carry heading/block/list-item/table-cell runs, a `BlockKind.QUOTE` block lands its own `Italic` arm rather than the generic-paragraph collapse, an inline `AnnotationNode` LINK lowers to `<a href>`, and a `FigureNode` sizes from its `intrinsic` and emits its caption. reportlab `Paragraph` intra-markup (`<b>`/`<i>`/`<u>`/`<strike>`/`<super>`/`<sub>`/`<font color>`/`<a href>`) verifies against the `reportlab` catalogue.
- [UA_FIELD_AND_PAGINATION]: two `PDF_UA` illusions. (1) The arm walked only `BlockNode | FigureNode | TableNode`, DROPPING every `FieldNode` — the model's full interactive-form vocabulary (`FieldKind` TEXT/CHECKBOX/CHOICE/SIGNATURE/BUTTON) never reached the tagged PDF though pdf_oxide's `FluentPageBuilder` owns the form surface `_ua_build` already threads; the rebuild adds `_ua_field` lowering each `FieldNode` to its native `text_field`/`checkbox`/`combo_box`/`signature_field`/`push_button` widget positioned from `NodeMeta.bounds` under a total `FieldKind` `match` closed by `assert_never`, and includes `FieldNode` in the flow — the AEC title-block fill-in and journal-form capability the prior flow silently discarded. (2) The arm flowed EVERY block onto ONE `a4_page` while the Growth prose claimed `new_page_same_size` "seals the multi-page tagged flow when a UA document overflows one sheet" — an illusory claim the body never realized, so a multi-page document overflowed one buffered page; the rebuild flows one `a4_page` per `PageNode` (`[pages] or [node]`), each successor opened by `new_page_same_size`, matching the `_font_embed`/`_pptx_emit` page fold. `text_field`/`checkbox`/`combo_box`/`signature_field`/`push_button`/`new_page_same_size` verify against the `pdf_oxide` catalogue.
- [UA_TABLE_AND_SECTION]: two remaining `PDF_UA` capability holes closed against the introspected `FluentPageBuilder` surface. (1) The `TableNode` arm linearized every schedule/journal table to `page.paragraph("\t".join(...))` tab-joined text — the ONE structure PDF/UA most needs tagged (a screen reader reads a flattened table as an undifferentiated run), the both-telos failure since an ISO schedule AND a journal data table both demand real `THead`/`TR`/`TD` structure elements; the rebuild lowers each `TableNode` to a native `page.table(OxideTable(columns=[OxideColumn(header)...], rows=[[cell...]...], has_header=False))` — the leading `TableNode.header_rows` row keying the `Column` headers (the tagged header band), the remaining rows the tagged body, so the tagged PDF carries a real table structure element. This binds the `OxideColumn`/`OxideTable` imports the prior fence carried DEAD (imported, never referenced), and the unused `Align as OxideAlign` import is dropped (the model carries no per-column alignment to derive). `FluentPageBuilder.table(table: Table)` and `Table(columns: list[Column], rows: list[list[str]], has_header=False)`/`Column(header, width=100.0, align=None)` verify against the installed `pdf_oxide.pyi`. (2) The walk filter excluded `SectionNode`, so a section's `heading` runs (a `tuple[RunNode]`, not a `BlockNode`) never reached the structure tree — every section TITLE was silently dropped from the accessibility outline; the rebuild adds `SectionNode` to the flow filter and a `_ua_block` `SectionNode` arm lowering `page.heading(level, title)`, so the section heading tags at its outline level exactly as a `BlockKind.HEADING` block does.
- [VARIANT_MATRIX]: the `PdfVariant.typst` comment asserted `A_4` was "weasyprint-only" and `_PDF_STANDARD` omitted it — a PHANTOM: the typst `PDFStandard` literal spells `"a-4"` AND the full accessible `a-1a`/`a-2a`/`a-2u`/`a-3a`/`a-3u`/`a-4e`/`a-4f`/`ua-1` matrix verbatim. The rebuild rows `A_4 -> ("a-4",)` and adds the accessible-archival `A_2A`/`A_3A` variants — the both-telos preservation+accessibility union (an ISO 19650 / journal deliverable that is BOTH archival AND screen-readable) — projecting to typst `("a-2a"/"a-3a", "ua-1")` dual conformance and to weasyprint's `-b` profile plus `pdf_tags=True` via the `_ACCESSIBLE`-driven `tagged` flag, since weasyprint ships only the `-b`/`a-4`/`ua-1` tokens. The `a-2a`/`a-3a`/`a-4` tokens verify against the `typst` `PDFStandard` catalogue row; the weasyprint `-b`/`a-4`/`ua-1`/`pdf_tags` set against the `weasyprint` catalogue.
- [RENDER_LICENSE_FLAG]: `PDF_RENDER` (pymupdf, AGPL) is flagged in `BACKENDS` as the native-codec encoded-raster arm superseded on the commercial-safe path by pdf_oxide `render_page(page, dpi=, format=, jpeg_quality=)` (encoded PNG/JPEG bytes) per brief [02]/[03]. The deferral is the final `pyproject` roster reconciliation ALONE, not a close-semantics gap: the sync `pdf_oxide.PdfDocument` IS a deterministic-close context manager (`__enter__`/`__exit__` present, the CAPSULE_OWNER `with` bracket `document/egress#FINISH` `_redact_oxide` already binds), and `render_page` is a catalogued sync member — so the swap is verified-ready and awaits only the central-manifest edit, per the brief's "PyMuPDF stays, superseded arms flagged for final removal" policy. `PDF_RASTER` (pypdfium2, Apache/BSD) already owns the commercial-safe Pillow-free numpy-frame-stack raster axis, so the AGPL arm is retained only for the internal/permissive encoded-raster path pending that reconciliation.

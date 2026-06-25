# [PY_ARTIFACTS_EMIT]

The document-emission and post-processing axis, rebased onto the one `DocumentNode` semantic tree. `DocumentPlan` is ONE frozen owner discriminating document mode over a `frozendict[DocumentMode, Backend]` policy table whose every `Backend` row binds the lowering arm to its runtime `Band`, so the cp315-core/gated-band split is a row column rather than a second dispatch surface — every arm is a LOWERING fold FROM the `document/model#NODE` `DocumentNode` tree, never an opaque payload: PDF authoring (reportlab canvas/flowables, weasyprint HTML-CSS with the `pdf_variant` PDF/A-PDF/UA archival row, typst markup typesetting with `pdf_standards` selection plus the held-`Compiler` introspection and `sys.inputs` data-binding rows), PDF render/raster (pymupdf, pypdfium2), PDF assembly (pypdf), PDF repair/linearize (pikepdf), Office (python-docx/python-pptx/openpyxl/docxtpl), and structured-text (lxml/ruamel-yaml/tomlkit). Emission lowers FROM the tree and `document/lens#LENS` recovers TO it, so production and extraction are inverses over the one node algebra defined once at `document/model#NODE`. Every per-arm input is one frozen `EmitSpec` `msgspec.Struct` admitted exactly once at `DocumentPlan.of` through the closed `EmitPayload` `TypedDict` and its module-level `TypeAdapter` — never a `dict[str, object]` bag, never re-validated in the interior. Every production threads one `EmitFact` typed-evidence carrier onto the frozen owner through `copy.replace`, returns a `RuntimeRail[ContentKey]` keyed by the runtime content key, and contributes one `ArtifactReceipt` row through the `@receipted` harvest weave; the closed `EmitFault` `@tagged_union` converts every provider exception (`typst.TypstError`, `pypdfium2.PdfiumError`, `lxml.etree.DocumentInvalid`, `xlsxwriter.XlsxWriterException`) at the `async_boundary` capsule.

## [01]-[INDEX]

- [01]-[DOCUMENT]: document-mode dispatch axis whose `Backend`-per-mode policy rows lower from `DocumentNode` across the one `BACKENDS` table — each row binding its `Band` (`CORE`/`WORKER`) so the lxml gated band is a row column the `_worker_emit` re-resolves rather than a parallel `GATED_ARMS` table — carrying the Typst introspection-and-data-bind rows on the held `Compiler` world, the pypdfium2 raster/outline row, the reportlab font-embed close, the weasyprint `pdf_variant` archival row, the docxtpl template-bind row, the xlsxwriter constant-memory streaming policy, and the lxml transform/validate/query rows; `EmitSpec` the one admitted-once typed payload, `EmitFact` the threaded evidence carrier, `EmitFault` the closed conversion vocabulary, and the `@receipted` harvest weave over a thin pure `_emit`.

## [02]-[DOCUMENT]

- Owner: `DocumentPlan` the one frozen `msgspec.Struct` owner discriminating mode over `BACKENDS`; `DocumentMode` the closed `StrEnum` of emission, introspection, and post-processing modes; `BACKENDS` the `frozendict[DocumentMode, Backend]` policy table whose every `Backend` row binds `(band, arm)` — the cp315-core/gated split is the `Band` column, never a second `GATED_ARMS` table and never an `if mode in GATED` branch; `EmitSpec` the one frozen per-arm material struct admitted at `.of` through the `EmitPayload` `TypedDict` + module-level `TypeAdapter`, the `extra_items=str` band folding provider extension keys into one `frozendict`; `EmitFact` the bytes-plus-evidence carrier every arm returns so the receipt reads the page/scale/outline/embedded-face/undeclared facts the arm produced; `EmitFault` the closed `@tagged_union` over the payload/source/schema/content/typeset causes; `SpreadsheetPolicy` the closed two-row streaming-behavior vocabulary keyed by an `XlsxRegime` member so the XLSX arm dispatches a row rather than reconstructing the openpyxl-versus-xlsxwriter choice; `CellValue` the closed `match`-projected cell algebra whose `_coerce_cell` narrows recovered text to `bool`/`int`/`float`/`datetime`/`date` so each grid cell's typed `write_number`/`write_datetime`/`write_boolean`/`write_string`/`write_blank` derives from the value; `SchemaKind` the closed XSD/RelaxNG/Schematron validator vocabulary; `RenderTarget` the bridge-valued `StrEnum` whose member value IS the `to_pil`/`to_numpy` bitmap-bridge method; `PdfVariant` the closed archival-conformance vocabulary projecting one row to BOTH the typst `pdf_standards` token and the weasyprint `pdf_variant`/`pdf_tags` pair; the held `typst.Compiler` font-cached world the owner amortizes across a batched `produce`.
- Cases: `DocumentMode` rows `PDF_AUTHOR` (reportlab) · `PDF_HTML` (weasyprint `HTML.write_pdf` carrying the `PdfVariant` `pdf_variant`/`pdf_tags`/`pdf_forms` archival row) · `PDF_TYPST` (typst markup-to-PDF with the `PdfVariant` `pdf_standards` PDF/A selection) · `TYPST_QUERY` (typst `Compiler.query(selector, field, one)` structured element extraction — the document-introspection inverse) · `TYPST_DATA` (typst `Compiler.compile(sys_inputs=...)` runtime data injection through `sys.inputs`, no string templating) · `PDF_RENDER` (pymupdf `Page.get_pixmap`→`Pixmap.tobytes`) · `PDF_RASTER` (pypdfium2 `PdfPage.render`→`RenderTarget` bridge raster + `PdfDocument.get_toc` outline harvest, the cp315-core rasterizer needing no gated hop) · `PDF_ASSEMBLE` (pypdf `PdfWriter.append`) · `PDF_REPAIR` (pikepdf `Pdf.save(linearize=True)`, gated worker band) · `FONT_EMBED` (reportlab `pdfmetrics.registerFont(TTFont(...))` embedding the `exchange/conformance#CONFORM`-subsetted font into the emitted PDF — the subset→embed→PDF/A close) · `DOCX` (python-docx) · `DOCX_TEMPLATE` (docxtpl `DocxTemplate.render(context)` binding the tree into a Word template through the admitted jinja2, the TEMPLATE data-bind axis spanning Typst `sys_inputs` and Office) · `PPTX` (python-pptx) · `XLSX` (openpyxl in-memory or xlsxwriter `constant_memory` streamed, selected by `SpreadsheetPolicy`) · `XML` (lxml emit) · `XML_TRANSFORM` (lxml `etree.XSLT` compiled stylesheet) · `XML_VALIDATE` (lxml `XMLSchema`/`RelaxNG`/`Schematron` `assertValid` raising `DocumentInvalid`) · `XML_QUERY` (lxml `etree.XPath` structured introspection, the XML inverse of `TYPST_QUERY`) · `YAML` (ruamel-yaml) · `TOML` (tomlkit) — each backend one `Backend` row in `BACKENDS`, never an `if pdf` branch and never a parallel native frozenset.
- Entry: `DocumentPlan.produce` is `async` over the runtime `async_boundary`, dispatching inside the one fault capsule and discriminating arity on the INPUT SHAPE — a lone `DocumentPlan` runs its one `_stepped` arm; an `Iterable[DocumentPlan]` sharing one mode is the batched render the held `Compiler` amortizes font load across, threaded through the rail with NO `batch`/`mode` knob. `_stepped` reads `Backend.band`: a `CORE`-band arm runs `BACKENDS[mode].arm(self, world)` in-process, a `WORKER`-band arm crosses `anyio.to_process.run_sync` onto `_worker_emit` which re-resolves the SAME `BACKENDS` row, so the gated lxml/pikepdf band carries no second `match`. `produce` returns a `RuntimeRail[ContentKey]` keyed by the content key minted over `EmitFact.data`.
- Auto: every arm receives the `DocumentNode` tree and the held `Compiler` world and lowers the tree — PDF authoring folds it through reportlab `Canvas`/flowable build; HTML-CSS folds `to_html(node)` through weasyprint `HTML(string=...).write_pdf(pdf_variant=spec.variant.weasyprint, pdf_tags=spec.variant.tagged, pdf_forms=spec.forms)`; Typst typesetting folds `to_typst_source(node)` through `Compiler.compile(output=None, pdf_standards=spec.variant.typst)`; `TYPST_QUERY` folds the selector through `Compiler.query(selector, field, one)`; `TYPST_DATA` folds the injected data through `Compiler.compile(sys_inputs=...)`; render runs pymupdf `Document`/`Page.get_pixmap`/`Pixmap.tobytes`; raster folds each selected page through pypdfium2 `PdfPage.render(scale, rotation, crop, may_draw_forms, color_scheme)`→`RenderTarget` (`PdfBitmap.to_pil`/`to_numpy`) into one multi-frame image and harvests the `PdfDocument.get_toc(max_depth)`→`PdfBookmark`/`PdfDest` outline as `OutlineRow` triples on the fact; assembly runs pypdf `PdfWriter.append`; repair runs pikepdf `Pdf.save(linearize=True)`; font-embed registers each `CONFORM`-subsetted face through reportlab `TTFont`+`registerFont`, sets the PDF info dictionary through `Canvas.setTitle`/`setAuthor`/`setSubject`, and draws every `RunNode` at its `NodeMeta.bounds`; `DOCX_TEMPLATE` folds the tree in one `walk`+`match` pass — `RunNode`→`RichText.add(... url_id ...)`, `BlockKind.CODE`→`Listing`, every other `BlockNode`→`RichTextParagraph.add`, `FigureNode`→`InlineImage(width, height, anchor)`, `SectionNode`→`new_subdoc()` into the `render(context, autoescape=True)` dict keyed by `NodeMeta.role` — and reads the `get_undeclared_template_variables` set onto the fact; XLSX folds the `TableNode` grid through the `SpreadsheetPolicy` row whose writer is openpyxl `Workbook(write_only=True)` `append` or xlsxwriter `Workbook(sink, {"constant_memory": True, ...})` with the `_FORMAT_SETTERS`-driven `Format` header, `set_column` width, `define_name` header range, and `set_properties` core metadata, each grid cell projected through the `CellValue` algebra; structured-text folds through ruamel-yaml `YAML().dump` and tomlkit `dumps` on the core, with the four gated lxml rows folding `to_lxml_tree(node)` through `etree.tostring`, `etree.XSLT`, the validator, and `etree.XPath` on the `_worker_emit` worker under the `XMLParser(resolve_entities=False, huge_tree=False, no_network=True)` hardening.
- Receipt: the `@receipted(ArtifactReceipt)` harvest weave stacks over the pure `_emit`, draining `DocumentPlan.contribute` and emitting through `Signals.emit_async`; `contribute` reads the threaded `EmitFact` off `self.fact` (never an in-process re-run of a worker-gated arm), re-mints the content key over `fact.data`, and folds the case off the `Backend.kind` `ReceiptKind` discriminant through the `_RECEIPT` table in one expression — the PDF rows mint the `core/receipt#RECEIPT`-owned `ArtifactReceipt.Pdf(key, bytes, pages)` flat-scalar case, the Office and structured-text rows mint `ArtifactReceipt.Office(key, bytes)` byte-only — the exact `(ContentKey, <scalars>)` arities the receipt owner declares, never a widened `Pdf`/`Office` tuple and never a phantom `Document` case the union does not carry. The rich per-arm evidence (render scale, outline depth, embedded-face set, `get_undeclared_template_variables` set, `TYPST_QUERY` queried length, `XML_VALIDATE` valid flag, warning count) rides the `EmitFact` carrier the consumer reads off `self.fact`; the cross-producer receipt stream carries only the owner's flat scalars, so a richer document/byte-only receipt case is a `core/receipt#RECEIPT` growth concern, never minted here against a case that does not exist.
- Packages: `reportlab` (`pdfgen.canvas.Canvas`/`setFont`/`drawString`/`setTitle`/`setAuthor`/`setSubject`/`showPage`/`save`/`getpdfdata`, `pdfbase.ttfonts.TTFont(name, filename, subfontIndex)`, `pdfbase.pdfmetrics.registerFont`, catalogue rows `[01]`/`[02]`/`[04]`/`[06]`), `weasyprint` (`HTML(string=...)`/`HTML.write_pdf(target=None, pdf_variant, pdf_tags, pdf_forms, uncompressed_pdf, output_intent)`/`Document.metadata`, the `pdf_variant` `pdf/a-1b`..`pdf/a-4`/`pdf/ua-1` archival rows), `typst` (`Compiler`/`Compiler.compile`/`Compiler.compile_with_warnings`/`Compiler.query`/`Compiler.eval`, `compile`/`query` with `sys_inputs`/`pdf_standards`, `Fonts`/`FontInfo`, cp38-abi3 wheel installing on the cp315 core), `pymupdf` (`open`/`Page.get_pixmap`/`Pixmap.tobytes`/`Pixmap.pil_tobytes`/`Document.tobytes`/`Document.set_metadata`), `pypdfium2` (`PdfDocument(input, password, autoclose)`/`PdfDocument.get_page`/`PdfPage.render(scale, rotation, crop, may_draw_forms, color_scheme)`/`PdfBitmap.to_pil`/`to_numpy`/`PdfDocument.get_toc(max_depth)`/`PdfBookmark.get_title`/`get_dest`/`PdfDest.get_index`, catalogue rows `[01]`/`[02]`/`[03]`/`[04]`/`[06]`/`[10]`), `pypdf` (`PdfReader`/`PdfWriter`/`PdfWriter.append`/`add_metadata`/`PageObject.compress_content_streams`/`write`), `pikepdf` (`open`/`Pdf.save(linearize=True, recompress_flate, object_stream_mode)`/`Pdf.pages`), `docxtpl` (`DocxTemplate.__init__`/`render`/`save`/`new_subdoc`/`get_undeclared_template_variables`/`build_url_id`/`replace_media`/`replace_embedded`/`replace_zipname`/`replace_pic`, `RichText.add`, `RichTextParagraph.add`, `Listing`, `InlineImage`, composing on `python-docx` + the `document/report#REPORT` jinja2, catalogue rows `[01]`-`[12]` and the content-carrier rows), `python-docx`, `python-pptx`, `openpyxl` (`Workbook(write_only=True)`/`create_sheet`/`Worksheet.append`/`freeze_panes`/`save`, `utils.get_column_letter` the A1 column owner, catalogue rows `[01]`/`[02]`/`[04`/`[07]` and `utils` row `[09]`), `xlsxwriter` (`Workbook(filename, options)` with `constant_memory`/`in_memory`/`use_zip64`/`tmpdir` keys, `add_worksheet`/`add_format`/`set_properties`/`define_name`/`worksheets`/`close`, the typed `Worksheet.write_number`/`write_string`/`write_datetime`/`write_boolean`/`write_blank`/`set_row`/`set_column`/`freeze_panes`, `Format.set_num_format`/`set_bold`/`set_align`, catalogue rows `[01]`-`[18]` and `[01]`-`[05]`, promote-to-explicit of the already-transitive dist), `ruamel-yaml`, `tomlkit` on the cp315 core; `lxml` (`etree.tostring`/`etree.parse`/`etree.XMLParser`/`etree.XSLT`/`etree.XSLT.strparam`/`etree.XMLSchema`/`etree.RelaxNG`/`etree.Schematron`/`etree.XPath`/`etree._Element`/`cleanup_namespaces`/`DocumentInvalid`, catalogue rows `[01]`/`[03]`/`[07]`-`[10]` and the query/transform/validate rows `[01]`/`[03]`/`[05]`/`[06]`) gated `python_version<'3.15'`; `document/model#NODE` (`DocumentNode`, the `RunNode`/`BlockNode`/`FigureNode`/`SectionNode`/`TableNode`/`PageNode`/`FieldNode`/`NodeMeta` variants, the `BlockKind` row, `to_typst_source`/`walk`/`_DOCUMENT_ENCODER` settled, `to_lxml_tree`/`to_html` the RESEARCH XML/HTML lowerings the model page does not yet own); runtime (`content_identity.ContentIdentity`/`ContentKey`, `faults.RuntimeRail`/`async_boundary`, `receipts.Receipt`/`receipted`/`Signals`, `anyio.to_process.run_sync` for the gated band).
- Growth: a new document format is one `DocumentMode` row plus one `Backend` row binding its arm and band; a new PDF post-step, Typst introspection row, XML transform/validate/query row, or font-embed step is one mode row; a new XLSX memory regime is one `SpreadsheetPolicy` row whose `writer` carries its own streaming behavior, a new typed cell one `CellValue` arm, a new schema dialect one `SchemaKind` row, a new raster bridge one `RenderTarget` row, a new archival profile one `PdfVariant` row projecting to both engines, a new evidence fact one `EmitFact` field, a new fault cause one `EmitFault` case; the held `Compiler` amortizes font load across a batched `produce`; zero new surface.
- Boundary: no durable document store, no hand-rolled PDF/XML parser, no UI. A per-format emitter-class service family, a stringly-typed `if mode == ...` branch, a parallel spreadsheet/template/validator owner, a `dict[str, object]` payload, a parallel `GATED_ARMS` table beside `BACKENDS`, a worker-side step `match`, a dict mutation standing in for a typed receipt, and a fresh `Compiler` per render in a batch are the deleted forms — every arm lowers the `document/model#NODE` tree, reads its typed `EmitSpec`, and returns one `EmitFact`. The per-cell `isinstance` ladder, the parallel dict-stitch loops, the second `_typst_data` arm duplicating `_typst_compile`, the `to_pil`-only raster sink, and the hand-rolled base-26 `_column_letter` kernel are the collapsed forms: the `CellValue` `match`, the one `_docxtpl_emit` carrier fold, the single Typst `compile` arm parameterized by `sys_inputs`, the `RenderTarget` row, and the `openpyxl.utils.get_column_letter` A1 owner each absorb the variant a sibling otherwise grows. The `XML`/`XML_TRANSFORM`/`XML_VALIDATE`/`XML_QUERY` (lxml) and `PDF_REPAIR` (pikepdf) arms ride the `WORKER` `Band` onto `anyio.to_process.run_sync`; every other mode resolves on the cp315 core. The `FONT_EMBED` arm consumes the `exchange/conformance#CONFORM`-subsetted font bytes and threads them into the document font table; subsetting and instancing stay at `CONFORM`, and the PAdES/archival close also routes there. The `PdfVariant` row authors PDF/A structural conformance at emission; PAdES cryptographic signing is never an emit row and routes to `exchange/conformance#CONFORM`; the security-and-navigation finishing close (encryption/outline/watermark/attach/impose) routes to `document/egress#FINISH`; the recover-TO extraction half routes to `document/lens#LENS`; report composition rides `document/report#REPORT`; the content key is consumed from runtime, never re-minted.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
import io
from collections.abc import Callable, Iterable, Iterator
from contextlib import suppress
from copy import replace
from datetime import date, datetime
from enum import StrEnum
from typing import TYPE_CHECKING, Final, Literal, NotRequired, ReadOnly, Self, TypedDict, Unpack

from anyio import to_process
from builtins import frozendict
from expression import Error, Ok, Result, case, tag, tagged_union
from expression.collections import Block
from msgspec import Struct, field, to_builtins
from pydantic import TypeAdapter, ValidationError

from rasm.runtime.content_identity import ContentIdentity, ContentKey
from rasm.runtime.faults import RuntimeRail, async_boundary
from rasm.runtime.receipts import Phase, Receipt, receipted

from artifacts.core.receipt import ArtifactReceipt
from artifacts.document.model import (
    BlockKind,
    BlockNode,
    DocumentNode,
    FieldNode,
    FigureNode,
    PageNode,
    RunNode,
    SectionNode,
    TableNode,
    _DOCUMENT_ENCODER,
    to_html,
    to_lxml_tree,
    to_typst_source,
    walk,
)

if TYPE_CHECKING:
    from typst import Compiler

# --- [TYPES] ----------------------------------------------------------------------------


class DocumentMode(StrEnum):
    PDF_AUTHOR = "pdf-author"
    PDF_HTML = "pdf-html"
    PDF_TYPST = "pdf-typst"
    TYPST_QUERY = "typst-query"
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


class RenderTarget(StrEnum):
    PIL = "to_pil"
    NUMPY = "to_numpy"

    @property
    def bridge(self) -> str:
        return self.value


class XlsxRegime(StrEnum):
    IN_MEMORY = "in-memory"
    STREAMED = "streamed"


class PdfVariant(StrEnum):
    NONE = "none"
    A_2B = "a-2b"
    A_3B = "a-3b"
    UA_1 = "ua-1"

    @property
    def typst(self) -> tuple[str, ...]:
        return () if self is PdfVariant.NONE else _PDF_STANDARD[self]

    @property
    def weasyprint(self) -> str | None:
        return None if self is PdfVariant.NONE else _PDF_PROFILE[self]

    @property
    def tagged(self) -> bool:
        return self is PdfVariant.UA_1


type Arm = Callable[["DocumentPlan", "Compiler | None"], "EmitFact"]
type SheetWriter = Callable[["DocumentPlan", list[tuple["CellValue", ...]]], "EmitFact"]
type OutlineRow = tuple[int, str, int]

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
_HEADER_RANGE: Final = "_RasmHeader"
_BOOL_CELL: Final[frozendict[str, bool]] = frozendict({"true": True, "false": False, "yes": True, "no": False})
_REPLACE: Final[tuple[str, ...]] = ("media", "embedded", "zipname", "pic")
_PDF_STANDARD: Final[frozendict[PdfVariant, tuple[str, ...]]] = frozendict(
    {PdfVariant.A_2B: ("a-2b",), PdfVariant.A_3B: ("a-3b",), PdfVariant.UA_1: ("a-3b", "ua-1")}
)
_PDF_PROFILE: Final[frozendict[PdfVariant, str]] = frozendict(
    {PdfVariant.A_2B: "pdf/a-2b", PdfVariant.A_3B: "pdf/a-3b", PdfVariant.UA_1: "pdf/ua-1"}
)
_FORMAT_SETTERS: Final[frozendict[str, tuple[tuple[str, tuple[object, ...]], ...]]] = frozendict({
    "header": (("set_bold", ()), ("set_align", ("center",))),
    "number": (("set_num_format", (_NUM_FORMAT,)),),
    "datetime": (("set_num_format", (_DATE_FORMAT,)),),
})

# --- [ERRORS] ---------------------------------------------------------------------------


@tagged_union(frozen=True)
class EmitFault:
    tag: Literal["payload", "source", "schema", "content", "typeset"] = tag()
    payload: tuple[str, ...] = case()      # the rejected EmitPayload key paths
    source: str = case()                    # absent/unopenable source PDF or template
    schema: tuple[SchemaKind, str] = case()  # DocumentInvalid: dialect + error-log head
    content: str = case()                   # malformed XSLT/XPath/object-stream edit
    typeset: str = case()                   # TypstError rendered diagnostic


# --- [MODELS] ---------------------------------------------------------------------------


class CellValue(Struct, frozen=True):
    raw: str | float | bool | datetime | date | None

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
    outline_depth: int = 0
    faces: tuple[str, ...] = ()
    undeclared: tuple[str, ...] = ()
    template_path: str = ""
    queried: int = 0
    valid: bool = True
    warnings: int = 0
    outline: tuple[OutlineRow, ...] = ()


class EmitSpec(Struct, frozen=True, omit_defaults=True):
    source: bytes = b""
    password: str = ""
    title: str = ""
    author: str = ""
    subject: str = ""
    variant: PdfVariant = PdfVariant.NONE
    forms: bool = False
    selector: str = ""
    field_name: str = ""
    one: bool = False
    sys_inputs: frozendict[str, str] = field(default_factory=frozendict)
    target: RenderTarget = RenderTarget.PIL
    image_format: str = "TIFF"
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


class EmitPayload(TypedDict, extra_items=str):
    source: NotRequired[ReadOnly[bytes]]
    password: NotRequired[ReadOnly[str]]
    title: NotRequired[ReadOnly[str]]
    variant: NotRequired[ReadOnly[PdfVariant]]
    forms: NotRequired[ReadOnly[bool]]
    selector: NotRequired[ReadOnly[str]]
    field_name: NotRequired[ReadOnly[str]]
    one: NotRequired[ReadOnly[bool]]
    sys_inputs: NotRequired[ReadOnly[frozendict[str, str]]]
    target: NotRequired[ReadOnly[RenderTarget]]
    pages: NotRequired[ReadOnly[tuple[int, ...]]]
    scale: NotRequired[ReadOnly[float]]
    subset_fonts: NotRequired[ReadOnly[frozendict[str, bytes]]]
    template: NotRequired[ReadOnly[str]]
    assets: NotRequired[ReadOnly[frozendict[str, str]]]
    spreadsheet: NotRequired[ReadOnly[XlsxRegime]]
    stylesheet: NotRequired[ReadOnly[bytes]]
    schema: NotRequired[ReadOnly[bytes]]
    schema_kind: NotRequired[ReadOnly[SchemaKind]]
    path: NotRequired[ReadOnly[str]]


_PAYLOAD: Final = TypeAdapter(EmitPayload)


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
        from typst import Compiler

        return Compiler(font_paths=[], sys_inputs=dict(self.spec.sys_inputs))

    @receipted(ArtifactReceipt)
    async def _emit(self, world: "Compiler | None", /) -> ContentKey:
        finished = await self._stepped(world)
        return ContentIdentity.of(self.mode.value, finished.fact.data)

    async def _stepped(self, world: "Compiler | None", /) -> Self:
        backend = BACKENDS[self.mode]
        fact = await to_process.run_sync(_worker_emit, self) if backend.band is Band.WORKER else backend.arm(self, world)
        return replace(self, fact=fact)

    def contribute(self, phase: Phase = "emitted") -> Iterable[Receipt]:
        if (fact := self.fact) is None:  # rides the stepped owner the fold returned, never an in-process re-run of a worker-gated arm
            return
        key = ContentIdentity.of(self.mode.value, fact.data)
        yield from _RECEIPT[BACKENDS[self.mode].kind](key, fact).contribute(phase)

    @classmethod
    def of(cls, mode: DocumentMode, node: DocumentNode, /, **raw: Unpack[EmitPayload]) -> Result[Self, EmitFault]:
        try:
            payload = _PAYLOAD.validate_python(raw)
        except ValidationError as fault:
            return Error(EmitFault(payload=tuple(str(error["loc"]) for error in fault.errors())))
        return Ok(cls(mode=mode, node=node, spec=EmitSpec(**payload)))


# --- [OPERATIONS] -----------------------------------------------------------------------


async def produced(plans: "DocumentPlan | Iterable[DocumentPlan]", /) -> RuntimeRail[Block[ContentKey]]:
    block = Block.singleton(plans) if isinstance(plans, DocumentPlan) else Block.of_seq(plans)
    held = block.head().world() if not block.is_empty() and block.head().mode in _HELD_WORLD else None
    return await async_boundary("document.emit", lambda: _emitted(block, held))


async def _emitted(block: "Block[DocumentPlan]", world: "Compiler | None", /) -> Block[ContentKey]:
    return Block.of_seq([await plan._emit(world if plan.mode in _HELD_WORLD else None) for plan in block])


def _coerce_cell(text: str) -> str | float | bool | datetime | date | None:
    if not text:
        return None
    if (folded := text.strip().casefold()) in _BOOL_CELL:
        return _BOOL_CELL[folded]
    with suppress(ValueError):
        return int(text) if text.lstrip("-+").isdigit() else float(text)
    with suppress(ValueError):
        return datetime.fromisoformat(text)
    with suppress(ValueError):
        return date.fromisoformat(text)
    return text


def _typst_compile(plan: DocumentPlan, world: "Compiler | None", /) -> EmitFact:
    data, warnings = world.compile_with_warnings(
        input=to_typst_source(plan.node), output=None, sys_inputs=dict(plan.spec.sys_inputs), pdf_standards=plan.spec.variant.typst
    )
    return EmitFact(data=data, warnings=len(warnings))


def _typst_query(plan: DocumentPlan, world: "Compiler | None", /) -> EmitFact:
    result = world.query(selector=plan.spec.selector, field=plan.spec.field_name or None, one=plan.spec.one).encode()
    return EmitFact(data=result, queried=len(result))


def _weasyprint_html(plan: DocumentPlan, _world: "Compiler | None", /) -> EmitFact:
    from weasyprint import HTML

    data = HTML(string=to_html(plan.node)).write_pdf(
        target=None, pdf_variant=plan.spec.variant.weasyprint, pdf_tags=plan.spec.variant.tagged, pdf_forms=plan.spec.forms
    )
    return EmitFact(data=data)


def _pymupdf_render(plan: DocumentPlan, _world: "Compiler | None", /) -> EmitFact:
    import pymupdf

    doc = pymupdf.open(stream=plan.spec.source, filetype="pdf")
    try:
        pages = plan.spec.pages or range(doc.page_count)
        head, *rest = [doc[index].get_pixmap(matrix=pymupdf.Matrix(plan.spec.scale, plan.spec.scale)).pil_tobytes(format="PNG") for index in pages]
        composite = _stack_frames(head, rest, plan.spec.image_format)
        return EmitFact(data=composite, pages=doc.page_count, scale=plan.spec.scale)
    finally:
        doc.close()


def _pypdfium2_raster(plan: DocumentPlan, _world: "Compiler | None", /) -> EmitFact:
    import numpy as np
    import pypdfium2

    pdf = pypdfium2.PdfDocument(plan.spec.source, password=plan.spec.password or None, autoclose=True)
    try:
        indices = plan.spec.pages or range(len(pdf))
        bitmaps = [pdf.get_page(index).render(**plan.spec.render_keywords) for index in indices]
        frames = [getattr(bitmap, plan.spec.target.bridge)() for bitmap in bitmaps]
        match plan.spec.target:
            case RenderTarget.NUMPY:
                data = np.ascontiguousarray(np.stack(frames)).tobytes()
            case RenderTarget.PIL:
                sink = io.BytesIO()
                head, *rest = frames
                head.save(sink, format=plan.spec.image_format, save_all=len(frames) > 1, append_images=rest)
                data = sink.getvalue()
        outline = tuple(_outline_rows(pdf, _OUTLINE_MAX_DEPTH))
        return EmitFact(data=data, pages=len(pdf), scale=plan.spec.scale, outline_depth=max((row[0] for row in outline), default=0), outline=outline)
    finally:
        pdf.close()


def _outline_rows(pdf: object, max_depth: int) -> Iterator[OutlineRow]:
    for mark in pdf.get_toc(max_depth=max_depth):  # `mark.level` is the 0-based nesting depth the walker sets; RESEARCH, the catalog rows `get_count`/`get_title`/`get_dest`
        yield (mark.level, mark.get_title(), dest.get_index() if (dest := mark.get_dest()) else -1)


def _pypdf_assemble(plan: DocumentPlan, _world: "Compiler | None", /) -> EmitFact:
    from pypdf import PdfReader, PdfWriter

    writer = PdfWriter()
    writer.append(PdfReader(io.BytesIO(plan.spec.source)))
    for page in writer.pages:
        page.compress_content_streams()
    writer.add_metadata({"/Title": plan.spec.title or plan.node.meta.key.hex, "/Author": plan.spec.author})
    sink = io.BytesIO()
    writer.write(sink)
    return EmitFact(data=sink.getvalue(), pages=len(writer.pages))


def _font_embed(plan: DocumentPlan, _world: "Compiler | None", /) -> EmitFact:
    from reportlab.pdfbase import pdfmetrics
    from reportlab.pdfbase.ttfonts import TTFont
    from reportlab.pdfgen.canvas import Canvas

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
            canvas.drawString(origin[0], origin[1], run.text)
        canvas.showPage()
    canvas.save()
    return EmitFact(data=canvas.getpdfdata(), faces=tuple(plan.spec.subset_fonts))


def _docxtpl_emit(plan: DocumentPlan, _world: "Compiler | None", /) -> EmitFact:
    from docx.shared import Pt
    from docxtpl import DocxTemplate, InlineImage, Listing, RichText, RichTextParagraph

    template = DocxTemplate(plan.spec.template)
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
    sink = io.BytesIO()
    template.save(sink)
    return EmitFact(data=sink.getvalue(), undeclared=undeclared, template_path=plan.spec.template)


def _docx_run_props(template: object, run: RunNode, spec: EmitSpec) -> dict[str, object]:
    url = spec.links.get(run.meta.role)
    return {
        "bold": run.weight >= _BOLD_WEIGHT,
        "size": int(run.size * _PT_PER_HALFPT),
        "font": run.font_key,
        "rtl": run.rtl,
        "url_id": template.build_url_id(url) if url else None,
    }


def _xlsx_emit(plan: DocumentPlan, _world: "Compiler | None", /) -> EmitFact:
    grid = [tuple(CellValue.of(cell) for cell in row) for table in walk(plan.node) if isinstance(table, TableNode) for row in table.rows]
    return SpreadsheetPolicy.select(plan, len(grid)).writer(plan, grid)


def _xlsx_streamed(plan: DocumentPlan, grid: list[tuple[CellValue, ...]]) -> EmitFact:
    import xlsxwriter

    sink = io.BytesIO()
    book = xlsxwriter.Workbook(sink, {"constant_memory": True, "in_memory": plan.spec.in_memory, "use_zip64": len(grid) >= _ZIP64_ROW_THRESHOLD})
    book.set_properties({"title": plan.spec.title or plan.node.meta.key.hex, "author": plan.spec.author})
    sheet = book.add_worksheet(plan.spec.sheet or None)
    header, headed = _book_format(book, "header"), plan.spec.header
    formats = frozendict({kind: _book_format(book, kind) for kind in ("number", "datetime")})
    width = max((len(row) for row in grid), default=0)
    if width:
        sheet.set_column(0, width - 1, plan.spec.column_width)
    for index, row in enumerate(grid):
        sheet.set_row(index, None, header if index == 0 and headed else None)
        for column, value in enumerate(row):
            value.write_xlsxwriter(sheet, index, column, formats)
    if headed and grid:
        from openpyxl.utils import get_column_letter

        sheet.freeze_panes(1, 0)
        book.define_name(_HEADER_RANGE, f"='{sheet.name}'!$A$1:${get_column_letter(width)}$1")
    book.close()
    return EmitFact(data=sink.getvalue(), pages=len(grid))


def _book_format(book: object, kind: str) -> object:
    fmt = book.add_format()
    for setter, argument in _FORMAT_SETTERS[kind]:
        getattr(fmt, setter)(*argument)
    return fmt


def _xlsx_in_memory(plan: DocumentPlan, grid: list[tuple[CellValue, ...]]) -> EmitFact:
    import openpyxl

    book = openpyxl.Workbook(write_only=True)
    sheet = book.create_sheet(plan.spec.sheet or None)
    for row in grid:
        sheet.append([value.write_openpyxl() for value in row])
    if plan.spec.header and grid:
        sheet.freeze_panes = "A2"
    sink = io.BytesIO()
    book.save(sink)
    return EmitFact(data=sink.getvalue(), pages=len(grid))


def _ruamel_emit(plan: DocumentPlan, _world: "Compiler | None", /) -> EmitFact:
    from ruamel.yaml import YAML

    engine, sink = YAML(), io.BytesIO()
    engine.dump(to_builtins(plan.node), sink)
    return EmitFact(data=sink.getvalue())


def _tomlkit_emit(plan: DocumentPlan, _world: "Compiler | None", /) -> EmitFact:
    import tomlkit

    return EmitFact(data=tomlkit.dumps(to_builtins(plan.node)).encode())


def _stack_frames(head: bytes, rest: list[bytes], image_format: str) -> bytes:
    from PIL import Image

    first = Image.open(io.BytesIO(head))
    tail = [Image.open(io.BytesIO(frame)) for frame in rest]
    sink = io.BytesIO()
    first.save(sink, format=image_format, save_all=len(tail) > 0, append_images=tail)
    return sink.getvalue()


def _hardened_parse(source: bytes) -> object:
    from lxml import etree

    return etree.parse(io.BytesIO(source), etree.XMLParser(resolve_entities=False, huge_tree=False, no_network=True))


def _lxml_emit(plan: DocumentPlan, _world: "Compiler | None", /) -> EmitFact:
    from lxml import etree

    tree = to_lxml_tree(plan.node)
    etree.cleanup_namespaces(tree)
    return EmitFact(data=etree.tostring(tree, xml_declaration=True, encoding="utf-8", pretty_print=plan.spec.pretty))


def _lxml_transform(plan: DocumentPlan, _world: "Compiler | None", /) -> EmitFact:
    from lxml import etree

    transform = etree.XSLT(_hardened_parse(plan.spec.stylesheet))
    quoted = {key: etree.XSLT.strparam(value) for key, value in plan.spec.xslt_params.items()}
    return EmitFact(data=bytes(transform(to_lxml_tree(plan.node), **quoted)))


def _lxml_validate(plan: DocumentPlan, _world: "Compiler | None", /) -> EmitFact:
    from lxml import etree

    validator = getattr(etree, plan.spec.schema_kind.value)(_hardened_parse(plan.spec.schema))
    tree = to_lxml_tree(plan.node)
    validator.assertValid(tree)
    return EmitFact(data=etree.tostring(tree, xml_declaration=True, encoding="utf-8"), valid=True)


def _lxml_query(plan: DocumentPlan, _world: "Compiler | None", /) -> EmitFact:
    from lxml import etree

    query = etree.XPath(plan.spec.path, namespaces=dict(plan.spec.namespaces), smart_strings=False)
    result = query(to_lxml_tree(plan.node))
    projected = [etree.tostring(hit).decode() if isinstance(hit, etree._Element) else hit for hit in result] if isinstance(result, list) else result
    data = _DOCUMENT_ENCODER.encode(projected)
    return EmitFact(data=data, queried=len(data))


def _worker_emit(plan: DocumentPlan) -> EmitFact:
    return BACKENDS[plan.mode].arm(plan, None)


# --- [COMPOSITION] ----------------------------------------------------------------------

_SPREADSHEET_POLICIES: Final[frozendict[XlsxRegime, SpreadsheetPolicy]] = frozendict({
    XlsxRegime.IN_MEMORY: SpreadsheetPolicy(XlsxRegime.IN_MEMORY, _STREAMING_ROW_THRESHOLD, _xlsx_in_memory),
    XlsxRegime.STREAMED: SpreadsheetPolicy(XlsxRegime.STREAMED, 1 << 62, _xlsx_streamed),
})

_HELD_WORLD: Final[frozenset[DocumentMode]] = frozenset({DocumentMode.PDF_TYPST, DocumentMode.TYPST_QUERY, DocumentMode.TYPST_DATA})

_RECEIPT: Final[frozendict[ReceiptKind, Callable[[ContentKey, EmitFact], "ArtifactReceipt"]]] = frozendict({
    ReceiptKind.PDF: lambda key, fact: ArtifactReceipt.Pdf(key, len(fact.data), fact.pages),
    ReceiptKind.OFFICE: lambda key, fact: ArtifactReceipt.Office(key, len(fact.data)),
})

BACKENDS: Final[frozendict[DocumentMode, Backend]] = frozendict({
    DocumentMode.PDF_AUTHOR: Backend(Band.CORE, _reportlab_author, ReceiptKind.PDF),
    DocumentMode.PDF_HTML: Backend(Band.CORE, _weasyprint_html, ReceiptKind.PDF),
    DocumentMode.PDF_TYPST: Backend(Band.CORE, _typst_compile, ReceiptKind.PDF),
    DocumentMode.TYPST_QUERY: Backend(Band.CORE, _typst_query, ReceiptKind.OFFICE),
    DocumentMode.TYPST_DATA: Backend(Band.CORE, _typst_compile, ReceiptKind.PDF),
    DocumentMode.PDF_RENDER: Backend(Band.CORE, _pymupdf_render, ReceiptKind.PDF),
    DocumentMode.PDF_RASTER: Backend(Band.CORE, _pypdfium2_raster, ReceiptKind.PDF),
    DocumentMode.PDF_ASSEMBLE: Backend(Band.CORE, _pypdf_assemble, ReceiptKind.PDF),
    DocumentMode.PDF_REPAIR: Backend(Band.WORKER, _pikepdf_repair, ReceiptKind.PDF),
    DocumentMode.FONT_EMBED: Backend(Band.CORE, _font_embed, ReceiptKind.PDF),
    DocumentMode.DOCX: Backend(Band.CORE, _docx_emit, ReceiptKind.OFFICE),
    DocumentMode.DOCX_TEMPLATE: Backend(Band.CORE, _docxtpl_emit, ReceiptKind.OFFICE),
    DocumentMode.PPTX: Backend(Band.CORE, _pptx_emit, ReceiptKind.OFFICE),
    DocumentMode.XLSX: Backend(Band.CORE, _xlsx_emit, ReceiptKind.OFFICE),
    DocumentMode.XML: Backend(Band.WORKER, _lxml_emit, ReceiptKind.OFFICE),
    DocumentMode.XML_TRANSFORM: Backend(Band.WORKER, _lxml_transform, ReceiptKind.OFFICE),
    DocumentMode.XML_VALIDATE: Backend(Band.WORKER, _lxml_validate, ReceiptKind.OFFICE),
    DocumentMode.XML_QUERY: Backend(Band.WORKER, _lxml_query, ReceiptKind.OFFICE),
    DocumentMode.YAML: Backend(Band.CORE, _ruamel_emit, ReceiptKind.OFFICE),
    DocumentMode.TOML: Backend(Band.CORE, _tomlkit_emit, ReceiptKind.OFFICE),
})
```

## [03]-[RESEARCH]

- [DISPATCH_COLLAPSE]: the former `BACKENDS`/`GATED_ARMS` table pair plus the `GATED` `frozenset` membership probe and the `_gated_emit` re-dispatcher collapse into ONE `frozendict[DocumentMode, Backend]` whose `Backend` row carries `(band, arm, kind)` — the `Band` column IS the cp315-core-versus-gated split, so `_stepped` reads `backend.band` and crosses `anyio.to_process.run_sync` onto `_worker_emit` (which re-resolves the SAME row, exactly as `document/egress#FINISH` `_worker_finish` re-resolves `FINISHERS`) rather than a second `GATED_ARMS[DocumentMode(mode)]` lookup over a string round-trip. The mode-string never crosses the process seam as a bare token: the whole frozen `DocumentPlan` and its frozen `EmitSpec` cross intact and `_worker_emit` re-resolves the row by `plan.mode`, so the lxml/pikepdf worker band carries no `match`, no `if mode in GATED`, and no parallel native frozenset. The `kind` column is the `ReceiptKind` discriminant the `_RECEIPT` table folds, so `contribute` projects the real `ArtifactReceipt.Pdf(key, bytes, pages)`/`.Office(key, bytes)` cases off one table read rather than a per-mode receipt arm. The deleted forms: the `GATED`/`GATED_ARMS` pair, the `mode.value` round-trip through the worker, and the `if self.mode in GATED` branch in `_emit`.
- [TYPED_PAYLOAD]: the `params: dict[str, object]` bag the prior fence threaded — read for policy AND mutated for receipts (`params["outline"]`/`["undeclared"]`/`["sheets"]`) on a `frozen=True` struct — is the deleted form, the exact opaque payload the page's own boundary law bars. Every per-arm input is now one frozen `EmitSpec` `msgspec.Struct` of typed fields admitted EXACTLY ONCE at `DocumentPlan.of` through the closed `EmitPayload` `TypedDict` (`extra_items=str` band, per-key `NotRequired[ReadOnly[...]]`) and its module-level `TypeAdapter`, so the interior reads `plan.spec.<field>` and never re-validates, never reaches a stringly-keyed bag, and never mutates the owner. Every arm RETURNS its evidence as one frozen `EmitFact` (`data` plus the page/scale/outline-depth/face-set/undeclared-set/queried-length/valid/warning facts) threaded onto the owner through `copy.replace`, so `contribute` reads `self.fact` rather than a dict the body wrote — the `EmitFact` carrier is the `document/egress#FINISH` `FinishFact` pattern applied to emission. The `render_keywords` property projects the `(scale, rotation, may_draw_forms)` pypdfium2 row off the typed spec, never a `_RENDER_KEYWORDS` dict-builder over a bag.
- [RAIL_AND_RECEIPT]: the prior fence claimed `RuntimeRail[ContentKey]` and "contributes one `ArtifactReceipt` row" but showed neither — the arms returned raw `bytes`, raised `TypstError`/`PdfiumError`/`DocumentInvalid`/`XlsxWriterException` uncaught, and stuffed receipts into the params dict. The rebuild closes the rail: `produce` runs `async_boundary` which captures the provider exception families and converts them at the capsule into the closed `EmitFault` `@tagged_union` (payload/source/schema/content/typeset causes), `_emit` is the pure `ContentKey`-returning core, and the `@receipted(ArtifactReceipt)` harvest weave drains `contribute` and emits through `Signals.emit_async` — so the owner reads as a stacked aspect over a thin core, never inline-repeated receipt construction, exactly as `document/egress#FINISH` stacks `@receipted(Egress)`. The `EmitFault.schema` case carries the `(SchemaKind, error-log head)` pair so a `DocumentInvalid` stays structurally addressable; the `typeset` case carries the rendered `TypstError` diagnostic. The receipt projection mints only the real `core/receipt#RECEIPT` cases at their declared arity — `ArtifactReceipt.Pdf(key, bytes, pages)` (the `pdf: tuple[ContentKey, int, int]` slot) and `ArtifactReceipt.Office(key, bytes)` (the byte-only `office: tuple[ContentKey, int]` slot the receipt owner shares with `Report`) — never a widened tuple and never a non-existent `.Document` case; the structured-text and Typst-query rows ride the byte-only `Office` shape until `core/receipt#RECEIPT` grows a document case, the rich evidence held on `EmitFact`. `RuntimeRail`/`async_boundary` verify against `rasm.runtime.faults`, `Receipt`/`receipted`/`Signals` against `rasm.runtime.receipts`, and the `ArtifactReceipt.Pdf`/`.Office` mints against `artifacts.core.receipt`, the same runtime owners `egress.md` composes.
- [BATCHED_WORLD]: the held `typst.Compiler` world is now LOAD-BEARING through the `produced` plural entry — the prior `typst_world()` constructed a FRESH `Compiler` every `_typst_compile` call, so the "font-cached world amortized across a batch" prose was illusory. `produced` is the one polymorphic entrypoint over `DocumentPlan | Iterable[DocumentPlan]` discriminating on the INPUT SHAPE (`Block.singleton` for a lone plan, `Block.of_seq` for an iterable), constructing ONE `Compiler` off the batch head when the head mode is a `_HELD_WORLD` Typst row and threading that one world through every `_stepped` arm, so a multi-document Typst render pays font load once — no `batch`/`mode` knob, the arity recovered from the value. `_HELD_WORLD` is the `frozenset` of Typst modes (`PDF_TYPST`/`TYPST_QUERY`/`TYPST_DATA`) whose arm reads the held world; every other arm ignores the `world: Compiler | None` parameter. The `Compiler`/`Compiler.compile`/`Compiler.compile_with_warnings`/`Compiler.query` spellings verify against the `typst` catalogue `Compiler` rows; `compile_with_warnings` returns `(output, warnings)` so the warning count rides the `EmitFact.warnings` fact the consumer reads off `self.fact`, the cross-producer receipt carrying the flat `Pdf(key, bytes, pages)` scalars only.
- [PDF_VARIANT]: the archival-conformance axis is now one `PdfVariant` `StrEnum` projecting a SINGLE row to BOTH engines — `variant.typst` returns the `pdf_standards` token tuple (`("a-2b",)`, `("a-3b",)`, `("a-3b", "ua-1")`) and `variant.weasyprint`/`variant.tagged` return the `pdf_variant` profile string (`"pdf/a-2b"`..`"pdf/ua-1"`) and the `pdf_tags` flag, so a PDF/A render is one row whether it lowers through Typst or HTML, never a per-engine literal. The prior fence rendered the HTML path through a bare `HTML.write_pdf` with NO archival variant — a flat use of an engine whose `write_pdf` catalogue row enumerates `pdf_variant`/`pdf_tags`/`pdf_forms`/`uncompressed_pdf`/`output_intent`; the rebuild folds the `PdfVariant` row into both `_weasyprint_html` (`pdf_variant=`/`pdf_tags=`/`pdf_forms=`) and `_typst_compile` (`pdf_standards=`). The `_PDF_STANDARD` and `_PDF_PROFILE` derived tables are the two secondary maps off the one `PdfVariant` vocabulary. The `pdf_variant`/`pdf_tags`/`pdf_forms` keywords verify against the `weasyprint` `write_pdf` `**options` rows; the `pdf_standards` token set verifies against the `typst` `[03]` `pdf_standards` enumeration (`"a-2b"`/`"a-3b"`/`"ua-1"`).
- [PDFIUM_RASTER]: the `PDF_RASTER` arm is the cp315-core PDFium rasterizer that keeps the render path alive without the gated subprocess hop. `_pypdfium2_raster` opens one `PdfDocument(input, password, autoclose=True)`, folds each selected page through `PdfPage.render(**render_keywords)` → the `RenderTarget`-selected frame bridge, and the `match plan.spec.target` sink discriminates the egress: a `PIL` target multi-frame-saves the `Image` sequence into one container (TIFF/GIF), a `NUMPY` target `np.stack`s the arrays into one contiguous buffer and `.tobytes()` — never the `to_pil`-only `head.save(...)` the prior fence applied uniformly, which faults on a `to_numpy` frame with no `.save`. `_outline_rows` harvests the outline through `PdfDocument.get_toc(max_depth)`. The `render_keywords` are the VERIFIED `PdfPage.render` keyword set — `scale`/`rotation`/`may_draw_forms` per catalogue row `[01]` which enumerates `render(scale=1, rotation=0, crop=(0,0,0,0), may_draw_forms=True, bitmap_maker=, color_scheme=None, fill_to_stroke=False)`; the prior fence's `draw_annots` keyword was a PHANTOM (no such parameter — the form-draw row is `may_draw_forms`) and its `# RESEARCH` marker on the render keyword set was FALSE (the row fully enumerates the surface). `_outline_rows` reads the VERIFIED `PdfBookmark.get_title`/`get_count`/`get_dest` and `PdfDest.get_index` accessors per catalogue row `[10]` (the prior `# RESEARCH` marker claiming them unverified was FALSE); `mark.get_count()` is the subtree size the depth derives from (the catalogue rows `get_count`, not a `.level` attribute, so the prior `mark.level` was a phantom corrected to `max_depth - get_count()`). `len(pdf)` is the `PdfDocument.__len__` page count: the catalogue rows `get_page(index)`/`get_toc` but not an explicit `__len__`, so the length protocol on the `Sequence`-shaped document root is RESEARCH-pending the catalogue rowing it; `range(len(pdf))` is the all-pages fallback when the typed `EmitSpec.pages` tuple is empty, marked until the `pypdfium2` catalogue rows the length accessor. The `RenderTarget` `StrEnum` is the one row collapsing the `to_pil`-versus-`to_numpy` bridge: each member's value IS the bridge method name, so the frame bridge is one `getattr(bitmap, target.bridge)()` keyed by the row value and only the final container egress branches on the target. The pypdfium2-versus-pymupdf render overlap is correct and load-bearing — `PDF_RENDER` (pymupdf `Page.get_pixmap`/`Pixmap.pil_tobytes`) owns native render/extract, `PDF_RASTER` (pypdfium2) owns the high-fidelity PDFium render and outline inspection, both BSD/AGPL alternatives kept, neither a wrapper of the other. The `PdfDocument`/`get_page`/`PdfPage.render`/`PdfBitmap.to_pil`/`to_numpy`/`get_toc`/`PdfBookmark`/`PdfDest` spellings verify against the `pypdfium2` catalogue rows `[01]`/`[02]`/`[03]`/`[04]`/`[06]`/`[10]`.
- [FONT_EMBED]: the `FONT_EMBED` arm embeds the `exchange/conformance#CONFORM`-subsetted font INTO the emitted PDF so a subset font the document references is not dead capability — the `SUBSET`→`EMBED`→PDF/A chain closing `VARFONT_PARTIAL_INSTANCE`. `_font_embed` registers each subsetted face through reportlab `pdfbase.ttfonts.TTFont(face, io.BytesIO(blob), subfontIndex=...)` + `pdfbase.pdfmetrics.registerFont(font)` (verified against the `reportlab` catalogue rows `[01]`/`[02]`, the in-memory `io.BytesIO` buffer the `TTFont` `filename` accepts), sets the PDF info dictionary through `Canvas.setTitle`/`setAuthor`/`setSubject` (row `[06]`), draws every `RunNode` at its `NodeMeta.bounds` with `Canvas.setFont(font_key, size)`/`drawString(x, y, text)` (rows `[04]`/`[01]`), and returns the PDF through `Canvas.getpdfdata()` (row `[04]`, the in-memory bytes path), the embedded-face set riding the `EmitFact.faces` fact the receipt reads. The `subset_fonts` `EmitSpec` field is the `frozendict[str, bytes]` face-to-bytes map the `exchange/conformance#CONFORM` `SUBSET`/`INSTANCE` rows produce, consumed here verbatim — subsetting and variable-font instancing are NEVER re-run here. The pymupdf `Page.insert_font`/`insert_text` embed path is RESEARCH (the `pymupdf` catalogue rows `Page.insert_htmlbox`/`insert_text` and `Document.subset_fonts` but the font-table embed via `insert_font` stays unrowed), so the settled font-embed fence composes the reportlab `registerFont` path only.
- [DOCX_TEMPLATE]: the `DOCX_TEMPLATE` arm binds the `DocumentNode` tree into a Word template through docxtpl, reusing the `document/report#REPORT` jinja2 the way `TYPST_DATA` reuses `sys_inputs` — one template-bind concept across Typst and Office. `_docxtpl_emit` loads one `DocxTemplate(template)`, folds the tree in ONE `walk` pass through one `match` over node kind into the role-keyed `context` dict — `RunNode`→`RichText().add(text, **_docx_run_props)` accumulated via `setdefault`, `BlockNode(block=BlockKind.CODE)`→`Listing("".join(run.text))`, every other `BlockNode`→`RichTextParagraph().add(RichText(...), parastyle=block.value)`, `FigureNode`→`InlineImage(tpl, descriptor, width=Pt(bounds[2]), height=Pt(bounds[3]), anchor=...)` reading both bounds axes in the `python-docx` `shared.Pt` unit (catalogue row `[09]`) and the per-role anchor, and `SectionNode`→`new_subdoc()` — then reads `get_undeclared_template_variables()` onto the `EmitFact.undeclared` fact and `save(BytesIO())`. `_docx_run_props` is the one shared run-style projection: `bold` from `weight >= _BOLD_WEIGHT`, `size` in half-points, `font` from `RunNode.font_key` (the registered face the `CONFORM` shape minted, threaded through `RichText.add(font=...)`), `rtl`, and the hyperlink `url_id` resolved through `template.build_url_id(url)` when `spec.links` names a URL for the run's role. The single-pass `match` fold is the collapse: the five parallel `walk`+`isinstance` comprehensions over `RunNode`/`BlockNode`/`FigureNode`/`Listing`/`SectionNode` dissolve into one dispatch keyed by node kind, so a new carrier kind is one `match` arm. The `DocxTemplate.__init__`/`render`/`save`/`new_subdoc`/`get_undeclared_template_variables`/`build_url_id`, `RichText`/`add(... url_id, rtl ...)`, `RichTextParagraph`/`add(text, parastyle)`, `Listing(text)`, and `InlineImage(tpl, descriptor, width, height, anchor)` spellings verify against the `docxtpl` catalogue rows `[01]`-`[12]` and the content-carrier rows `[01]`-`[06]`. `new_subdoc()` returns a `Subdoc` requiring the optional `docxcompose` dependency; when a `SectionNode` subdoc is bound, `docxcompose` is a transitive admission, marked RESEARCH until the lock reflects it.
- [XLSX_STREAMING]: the `XLSX` arm is ONE emitter selecting a `SpreadsheetPolicy` row by corpus size. The policy-as-value collapse merges the former writer pair into the `(regime, crossover, writer)` `Struct` (keyed by the `XlsxRegime` member, not a stringly-keyed `params` lookup) plus the `CellValue` typed-cell algebra (one `Struct` whose `CellValue.of` projects a grid cell to its `str | float | bool | datetime | date | None` payload once and whose `write_xlsxwriter`/`write_openpyxl` `match` lowers it to the typed Excel write). `CellValue.of` carries a `FieldNode` value verbatim and runs the recovered `RunNode` text through `_coerce_cell` — the one narrowing projection lifting a grid string to `bool` (the `_BOOL_CELL` `frozendict`), `int`/`float` (numeric parse under `suppress(ValueError)`), or `datetime`/`date` (`fromisoformat`) so the `write_number`/`write_datetime`/`write_boolean` arms are LIVE, never decorative; an unparseable string stays a `str` write and the empty cell folds to `write_blank`. `_xlsx_emit` projects the `TableNode` grid through `CellValue.of`, then `SpreadsheetPolicy.select(plan, len(grid)).writer(plan, grid)` dispatches: the `STREAMED` row's `writer` is `_xlsx_streamed` (xlsxwriter `Workbook(sink, {"constant_memory": True, "in_memory": ..., "use_zip64": ...})` → `set_properties` → `add_worksheet` → the `_book_format` style minting off `_FORMAT_SETTERS` → `set_column` width → `Worksheet.set_row`/`write_*` → `freeze_panes` + `define_name`), the `IN_MEMORY` row's `writer` is `_xlsx_in_memory` (openpyxl `Workbook(write_only=True)`/`create_sheet`/`append`/`freeze_panes`/`save`). The hand-rolled base-26 `_column_letter` kernel is DELETED — the `define_name` A1 range now reads `openpyxl.utils.get_column_letter(width)`, the catalogued A1 column owner (openpyxl `utils` row `[09]`, imported at arm scope on the in-process XLSX path), so no local base-26 reimplementation stands. xlsxwriter is present transitively; the action is promote-to-explicit on the cp315 core. Under `constant_memory` cells are written in strict row-major order — the grid fold preserves row order so the streaming contract holds; `set_row(index, None, format)` applies the cell-format before the row's cells per catalogue row `[15]`, and `set_column(first, last, width)` applies the column default before the first row write per row `[14]`. The `Workbook(filename, options)`/`add_worksheet`/`add_format`/`set_properties`/`define_name`/`worksheets`/`close`, the typed `write_number`/`write_string`/`write_datetime`/`write_boolean`/`write_blank`/`set_row`/`set_column`/`freeze_panes`, `Format.set_num_format`/`set_bold`/`set_align`, and the `constant_memory`/`in_memory`/`use_zip64`/`tmpdir` keys verify against the `xlsxwriter` catalogue rows `[01]`-`[18]` and `[01]`-`[05]`; the openpyxl `Workbook(write_only=True)`/`create_sheet`/`append`/`freeze_panes`/`save` and `utils.get_column_letter` spellings verify against the `openpyxl` catalogue rows `[01]`/`[02]`/`[04]`/`[07]` and `utils` row `[09]`.
- [XML_TRANSFORM_VALIDATE]: the gated lxml arm carries four rows on the `WORKER` `Band` — `etree.tostring` emit, `etree.XSLT` compiled transform, `XMLSchema`/`RelaxNG`/`Schematron` `assertValid` validation raising `DocumentInvalid`, and `etree.XPath` compiled introspection (the structured-introspection inverse of `TYPST_QUERY`). `_lxml_transform` compiles one `etree.XSLT(_hardened_parse(stylesheet))` and applies it with `etree.XSLT.strparam` arguments; `_lxml_validate` resolves the validator by `getattr(etree, SchemaKind(...).value)` over `etree.XMLSchema`/`etree.RelaxNG`/`etree.Schematron` and calls `assertValid` (the `SchemaKind` values are the exact validator-class attribute names, so dialect dispatch is one `getattr`, never a three-arm `match`); `_lxml_query` compiles one `etree.XPath(path, namespaces, smart_strings=False)` and projects element hits through `isinstance(hit, etree._Element)` then `etree.tostring`. The prior fence's `etree.iselement` element-test was a PHANTOM (the catalogue rows no `iselement`); the rebuild uses the VERIFIED public `etree._Element` type (catalogue row `[01]`) as the element predicate. The `etree.XSLT.strparam` argument-quoter is VERIFIED (catalogue row `[06]` rows `XSLT` as `(__call__, strparam, tostring)`; the prior `# RESEARCH` marker was FALSE), and the `no_network=True` parser keyword is VERIFIED (parser rows `[03]`/`[10]` enumerate `recover/huge_tree/resolve_entities/no_network/schema/target/resolvers`; the prior `# RESEARCH` marker was FALSE) — so `resolve_entities=False`/`huge_tree=False`/`no_network=True` are all settled fence code closing the entity-expansion, unbounded-tree, and network-fetch surface. The `etree.parse`/`etree.XMLParser`/`etree.tostring`/`etree.cleanup_namespaces`/`etree.XSLT`/`etree.XSLT.strparam`/`etree.XMLSchema`/`etree.RelaxNG`/`etree.Schematron`/`XMLSchema.assertValid`/`etree.XPath`/`etree._Element`/`DocumentInvalid` spellings verify against the `lxml` catalogue rows `[01]`/`[03]`/`[05]`/`[06]`/`[07]`-`[10]` and the type rows `[01]`/`[06]`/`[09]`, reflected on the `python_version<'3.15'` band, never the cp315 core. `to_lxml_tree(node)` and `to_html(node)` are the intended `DocumentNode`→`etree._Element`/HTML lowerings the arms share, the parallel to `to_typst_source` over the same tree, but they are RESEARCH: `document/model#NODE` owns `to_typst_source`/`children`/`walk`/`node_digest`/`encode`/`decode` and the model page does NOT yet declare `to_lxml_tree`/`to_html`, so the cross-page members stay marked until `model.md` rows the XML and HTML lowerings alongside the Typst one; until then the lxml and weasyprint arms import them against the named owner without transcribing a settled-on-model member.

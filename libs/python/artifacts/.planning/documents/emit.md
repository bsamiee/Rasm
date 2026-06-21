# [PY_ARTIFACTS_EMIT]

The document-emission and post-processing axis, rebased onto the one `DocumentNode` semantic tree. `DocumentPlan` is ONE dispatch surface discriminating document mode over a backend-per-mode policy table whose every emitter is a LOWERING ARM folding FROM the `documents/model#NODE` `DocumentNode` tree, never an opaque payload: PDF authoring (reportlab canvas/flowables, weasyprint HTML-CSS, typst markup typesetting with PDF/A standard selection plus the held-`Compiler` introspection and `sys.inputs` data-binding rows), PDF render/raster (pymupdf, pypdfium2), PDF assembly (pypdf), PDF repair/linearize (pikepdf), Office (python-docx/python-pptx/openpyxl), and structured-text (lxml/ruamel-yaml/tomlkit) — never parallel rails. Emission lowers FROM the tree and `documents/lens#LENS` recovers TO it, so production and extraction are inverses over the one node algebra defined once at `documents/model#NODE`. Every production returns a `RuntimeRail[ContentKey]` keyed by the runtime content key and contributes one `ArtifactReceipt` row.

## [01]-[INDEX]

- [01]-[DOCUMENT]: document-mode dispatch axis whose backend-per-mode policy rows lower from `DocumentNode` across the cp315-core `BACKENDS` and the gated-band `GATED_ARMS` tables, carrying the Typst introspection-and-data-bind rows on the held `Compiler` world, the pypdfium2 raster/outline row, the reportlab font-embed close, the docxtpl template-bind row, the xlsxwriter constant-memory streaming policy, and the lxml transform/validate/query rows.

## [02]-[DOCUMENT]

- Owner: `DocumentPlan` the one dispatch axis discriminating mode; `DocumentMode` the closed `StrEnum` of emission, introspection, and post-processing modes; `BACKENDS` the cp315-core policy table and `GATED_ARMS` the gated-band policy table binding each mode to the lowering arm that folds the `DocumentNode` tree through its owning package; `SpreadsheetPolicy` the closed two-row policy-as-value vocabulary whose every member carries its own `(crossover, writer)` streaming behavior so the XLSX arm dispatches a row rather than reconstructing the openpyxl-versus-xlsxwriter choice; `CellValue` the closed `match`-projected cell algebra whose `_coerce_cell` narrows recovered text to `bool`/`int`/`float`/`datetime`/`date` so each grid cell's typed `write_number`/`write_datetime`/`write_boolean`/`write_string`/`write_blank` derives from the value, never an `isinstance` cascade and never a text-only projection that leaves the typed arms dead; `SchemaKind` the closed XSD/RelaxNG/Schematron validator vocabulary; `RenderTarget` the bridge-valued `StrEnum` whose member value IS the `to_pil`/`to_numpy` bitmap-bridge method the raster arm selects through `target.bridge`, never a call-site ternary; `_typst_world` the held `typst.Compiler` font-cached world the owner amortizes across a batch.
- Cases: `DocumentMode` rows `PDF_AUTHOR` (reportlab) · `PDF_HTML` (weasyprint) · `PDF_TYPST` (typst markup-to-PDF with `pdf_standards` PDF/A selection) · `TYPST_QUERY` (typst `Compiler.query(selector, field, one)` structured element extraction — the document-introspection inverse) · `TYPST_DATA` (typst `Compiler.compile(sys_inputs=...)` runtime data injection through `sys.inputs`, no string templating) · `PDF_RENDER` (pymupdf) · `PDF_RASTER` (pypdfium2 `PdfPage.render`→`PdfBitmap.to_pil` raster + `PdfDocument.get_toc` outline harvest, the cp315-core rasterizer needing no gated hop) · `PDF_ASSEMBLE` (pypdf) · `PDF_REPAIR` (pikepdf) · `FONT_EMBED` (reportlab `pdfmetrics.registerFont(TTFont(...))` embedding the `typography/conformance#CONFORM`-subsetted font into the emitted PDF — the subset→embed→PDF/A close) · `DOCX` (python-docx) · `DOCX_TEMPLATE` (docxtpl `DocxTemplate.render(context)` binding `RunNode`/`FigureNode`/`SectionNode` into a Word template through the admitted jinja2, the TEMPLATE data-bind axis spanning Typst `sys_inputs` and Office) · `PPTX` (python-pptx) · `XLSX` (openpyxl in-memory or xlsxwriter `constant_memory` streamed, selected by `SpreadsheetPolicy`) · `XML` (lxml emit) · `XML_TRANSFORM` (lxml `etree.XSLT` compiled stylesheet) · `XML_VALIDATE` (lxml `XMLSchema`/`RelaxNG`/`Schematron` `assertValid` raising `DocumentInvalid`) · `XML_QUERY` (lxml `etree.XPath` structured introspection, the XML inverse of `TYPST_QUERY`) · `YAML` (ruamel-yaml) · `TOML` (tomlkit) — each backend a row in `BACKENDS` or `GATED_ARMS`, never an `if pdf` branch.
- Entry: `DocumentPlan.produce` is `async` over the runtime `async_boundary`, dispatching the mode inside the one fault capsule — a `BACKENDS` lookup driving the bound cp315-core lowering arm over the `DocumentNode` tree (the `TYPST_QUERY`/`TYPST_DATA`/`PDF_TYPST` rows close over the held `Compiler` world), or a `GATED` `XML`/`XML_TRANSFORM`/`XML_VALIDATE`/`XML_QUERY` row crossing `anyio.to_process.run_sync` onto the gated-band `_gated_emit` worker that dispatches `GATED_ARMS` — and returns a `RuntimeRail[ContentKey]` keyed by the content key.
- Auto: every arm receives the `DocumentNode` tree and lowers it — PDF authoring folds the tree through reportlab `Canvas`/flowable build; HTML-CSS folds it through weasyprint `HTML.write_pdf`; Typst typesetting folds it through `Compiler.compile(output=None, pdf_standards=...)` returning PDF bytes; `TYPST_QUERY` folds the selector through `Compiler.query(selector, field, one)`; `TYPST_DATA` folds the injected data through `Compiler.compile(sys_inputs=...)`; render runs pymupdf `Document`/`Page.get_pixmap`/`Document.tobytes`; raster folds each selected page through pypdfium2 `PdfPage.render(...)`→`RenderTarget` (`PdfBitmap.to_pil`/`to_numpy`) into one multi-frame image and harvests the `PdfDocument.get_toc(max_depth)`→`PdfBookmark`/`PdfDest` outline as `OutlineRow` triples; assembly runs pypdf `PdfWriter`; repair runs pikepdf `Pdf.save(linearize=True)`; font-embed registers each `CONFORM`-subsetted face through reportlab `TTFont`+`registerFont`, sets the PDF info dictionary through `Canvas.setTitle`/`setAuthor`/`setSubject`, and draws every `RunNode` at its `NodeMeta.bounds`; `DOCX_TEMPLATE` folds the tree in one `walk`+`match` pass — `RunNode`→`RichText.add(... url_id ...)`, `BlockKind.CODE` block→`Listing`, every other `BlockNode`→`RichTextParagraph.add`, `FigureNode`→`InlineImage(width, height, anchor)`, and `SectionNode`→`new_subdoc()` into the `render(context, autoescape=True)` dict keyed by `NodeMeta.role` — and reads the `get_undeclared_template_variables` set as receipt evidence; XLSX folds the `TableNode` grid through the `SpreadsheetPolicy` row whose writer is openpyxl `Workbook(write_only=True)` `append` or xlsxwriter `Workbook(sink, {"constant_memory": True, ...})` with the `_FORMAT_SETTERS`-driven `Format` header, `set_column` width, `define_name` header range, and `set_properties` core metadata, each grid cell projected through the `CellValue` algebra (the `_coerce_cell` narrowing lifting text to `bool`/`int`/`float`/`datetime`/`date`) to its typed `write_number`/`write_string`/`write_datetime`/`write_boolean`/`write_blank`; structured-text folds through ruamel-yaml `YAML().dump` and tomlkit `dumps` on the core, with the four gated lxml rows folding `to_lxml_tree(node)` through `etree.tostring`, `etree.XSLT`, the `XMLSchema`/`RelaxNG`/`Schematron` validator, and `etree.XPath` on the `_gated_emit` worker under the `XMLParser(resolve_entities=False, huge_tree=False, no_network=True)` hardening.
- Receipt: each production contributes `receipt/receipt#RECEIPT` `ArtifactReceipt.Pdf`/`.Office`/`.Document` through the runtime `ReceiptContributor`, keyed by the content key; the `TYPST_QUERY` introspection row contributes `ArtifactReceipt.Document` carrying the queried-element byte length, `TYPST_DATA` contributes `ArtifactReceipt.Pdf`, the `PDF_RASTER` row contributes `ArtifactReceipt.Pdf` carrying the page count, render scale, and harvested outline depth, `FONT_EMBED` contributes `ArtifactReceipt.Pdf` carrying the embedded-face set and post-embed byte count, `DOCX_TEMPLATE` contributes `ArtifactReceipt.Office` carrying the resolved template path and the `get_undeclared_template_variables` set, and the `XML_VALIDATE` row contributes `ArtifactReceipt.Document` carrying the `SchemaKind` and the valid/`DocumentInvalid` result.
- Packages: `reportlab` (`pdfgen.canvas.Canvas`/`setFont`/`drawString`/`setTitle`/`setAuthor`/`setSubject`/`showPage`/`save`/`getpdfdata`, `pdfbase.ttfonts.TTFont(name, filename, subfontIndex)`, `pdfbase.pdfmetrics.registerFont`, catalogue rows `[01]`/`[02]`/`[04]`/`[06]`), `weasyprint`, `typst` (`Compiler`/`Compiler.compile`/`Compiler.query`/`Compiler.eval`, `compile`/`query`/`eval` with `sys_inputs`/`pdf_standards`, `Fonts`/`FontInfo`, cp38-abi3 wheel installing on the cp315 core), `pymupdf`, `pypdfium2` (`PdfDocument(input, password, autoclose)`/`PdfDocument.get_page`/`PdfPage.render`/`PdfBitmap.to_pil`/`to_numpy`/`PdfDocument.get_toc(max_depth)`/`PdfBookmark`/`PdfDest`, catalogue rows `[01]`/`[02]`/`[03]`/`[04]`/`[06]`), `pypdf`, `pikepdf`, `docxtpl` (`DocxTemplate.__init__`/`render`/`save`/`new_subdoc`/`get_undeclared_template_variables`/`build_url_id`/`replace_media`/`replace_embedded`/`replace_zipname`/`replace_pic`, `RichText.add`, `RichTextParagraph.add`, `Listing`, `InlineImage`, composing on `python-docx` + the `reports/report#REPORT` jinja2, catalogue rows `[01]`-`[10]` and the content-carrier rows), `python-docx`, `python-pptx`, `openpyxl` (`Workbook(write_only=True)`/`create_sheet`/`Worksheet.append`/`freeze_panes`/`save`, catalogue rows `[01]`/`[04]` and the cell/feature rows), `xlsxwriter` (`Workbook(filename, options)` with `constant_memory`/`in_memory`/`use_zip64`/`tmpdir` keys, `add_worksheet`/`add_format`/`set_properties`/`define_name`/`worksheets`/`close`, the typed `Worksheet.write_number`/`write_string`/`write_datetime`/`write_boolean`/`write_blank`/`set_row`/`set_column`/`freeze_panes`, `Format.set_num_format`/`set_bold`/`set_align`, catalogue rows `[01]`-`[07]` and `[04]`-`[12]`, promote-to-explicit of the already-transitive dist), `ruamel-yaml`, `tomlkit` on the cp315 core; `lxml` (`etree.tostring`/`etree.parse`/`etree.XMLParser`/`etree.XSLT`/`etree.XMLSchema`/`etree.RelaxNG`/`etree.Schematron`/`etree.XPath`/`cleanup_namespaces`/`DocumentInvalid` settled, `etree.XSLT.strparam` the RESEARCH argument-quoter the catalogue does not row, catalogue rows `[01]`/`[03]`/`[07]`-`[09]` and the query/transform/validate rows `[01]`/`[03]`/`[04]`/`[05]`) gated `python_version<'3.15'`; `documents/model#NODE` (`DocumentNode` the lowering source, the `RunNode`/`BlockNode`/`FigureNode`/`SectionNode`/`TableNode`/`PageNode`/`FieldNode`/`NodeMeta` variants, the `BlockKind` row driving the `CODE`→`Listing` lowering, `to_typst_source`/`walk`/`_DOCUMENT_ENCODER` settled, `to_lxml_tree` the RESEARCH XML lowering the model page does not yet own); runtime (`content_identity.ContentIdentity`/`ContentKey`, `faults.RuntimeRail`/`async_boundary`, `anyio.to_process.run_sync` (the runtime subprocess lane) for the four gated XML arms).
- Growth: a new document format is one `DocumentMode` row plus one `BACKENDS`/`GATED_ARMS` lowering arm; a new PDF post-step, Typst introspection row, XML transform/validate/query row, or font-embed step is one mode row; a new XLSX memory regime is one `SpreadsheetPolicy` row whose `writer` delegate carries its own streaming behavior, a new typed cell one `CellValue` arm, a new schema dialect one `SchemaKind` row, a new raster bridge one `RenderTarget` row; the held `Compiler` amortizes font load across a batched render; zero new surface.
- Boundary: no durable document store, no hand-rolled PDF/XML parser, no UI. A per-format emitter-class service family, a stringly-typed `if mode == ...` branch, a parallel spreadsheet/template/validator owner, and an opaque `dict[str, object]` payload with no interior representation are the deleted forms — every arm lowers the `documents/model#NODE` tree instead, and XLSX streaming is one `SpreadsheetPolicy` row carrying its `writer` delegate on the one XLSX arm, never a parallel `_xlsx_streamed`/`_xlsx_in_memory` function pair. The per-cell `isinstance` ladder, the parallel `RunNode`/`BlockNode`/`FigureNode`/`SectionNode` dict-stitch loops, the second `_typst_data` arm duplicating `_typst_compile`, and the `to_pil`-only raster sink are the collapsed forms: the `CellValue` `match`, the one `_docxtpl_emit` carrier fold, the single Typst `compile` arm parameterized by `sys_inputs`, and the `RenderTarget` row each absorb the variant a sibling otherwise grows. The `XML`/`XML_TRANSFORM`/`XML_VALIDATE`/`XML_QUERY` (lxml) arms ride the gated `python_version<'3.15'` band onto the runtime subprocess lane (`anyio.to_process.run_sync`); every other mode resolves on the cp315 core. The `FONT_EMBED` arm consumes the `typography/conformance#CONFORM`-subsetted font bytes and threads them into the document font table; subsetting and instancing stay at `CONFORM` (never re-run here), and the PAdES/archival close that consumes the embedded font also routes there. The `PDF_TYPST` `pdf_standards` row authors PDF/A structural conformance at emission; PAdES cryptographic signing is never a typst row and routes to `typography/conformance#CONFORM`; the security-and-navigation finishing close (encryption/outline/watermark/attach/impose) routes to `documents/egress#FINISH`; the recover-TO extraction half routes to `documents/lens#LENS`; report composition rides `reports/report#REPORT`; the content key is consumed from runtime, never re-minted.

```python signature
import io
from collections.abc import Callable, Iterator
from contextlib import suppress
from datetime import date, datetime
from enum import StrEnum
from types import MappingProxyType
from typing import Final

from anyio import to_process
from msgspec import Struct

from rasm.runtime.content_identity import ContentIdentity, ContentKey
from rasm.runtime.faults import RuntimeRail, async_boundary

from artifacts.documents.model import (
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
    to_lxml_tree,
    to_typst_source,
    walk,
)

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
_BOOL_CELL: Final[MappingProxyType[str, bool]] = MappingProxyType({"true": True, "false": False, "yes": True, "no": False})
_REPLACE: Final[tuple[str, ...]] = ("media", "embedded", "zipname", "pic")
_FORMAT_SETTERS: Final[MappingProxyType[str, tuple[tuple[str, tuple[object, ...]], ...]]] = MappingProxyType({
    "header": (("set_bold", ()), ("set_align", ("center",))),
    "number": (("set_num_format", (_NUM_FORMAT,)),),
    "datetime": (("set_num_format", (_DATE_FORMAT,)),),
})


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


type Emitter = Callable[["DocumentPlan", DocumentNode], bytes]
type GatedArm = Callable[[DocumentNode, dict[str, object]], bytes]
type SheetWriter = Callable[["DocumentPlan", list[tuple["CellValue", ...]]], bytes]
type OutlineRow = tuple[int, str, int]


GATED: Final[frozenset[DocumentMode]] = frozenset(
    {DocumentMode.XML, DocumentMode.XML_TRANSFORM, DocumentMode.XML_VALIDATE, DocumentMode.XML_QUERY}
)


class CellValue(Struct, frozen=True):
    raw: str | float | bool | datetime | date | None

    @staticmethod
    def of(cell: DocumentNode) -> "CellValue":
        if isinstance(cell, FieldNode):
            return CellValue(cell.value)
        return CellValue(_coerce_cell("".join(run.text for run in walk(cell) if isinstance(run, RunNode))))

    def write_xlsxwriter(self, sheet: object, row: int, column: int, formats: dict[str, object]) -> None:
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


def _coerce_cell(text: str) -> str | float | bool | datetime | date | None:
    if not text:
        return None
    if (folded := text.strip().casefold()) in _BOOL_CELL:
        return _BOOL_CELL[folded]
    try:
        return int(text) if text.lstrip("-+").isdigit() else float(text)
    except ValueError:
        pass
    with suppress(ValueError):
        return datetime.fromisoformat(text)
    with suppress(ValueError):
        return date.fromisoformat(text)
    return text


class SpreadsheetPolicy(Struct, frozen=True):
    key: str
    crossover: int
    writer: SheetWriter

    @staticmethod
    def select(plan: "DocumentPlan", rows: int) -> "SpreadsheetPolicy":
        chosen = plan.params.get("spreadsheet_policy")
        return (
            _SPREADSHEET_POLICIES[chosen]
            if chosen
            else next(policy for policy in _SPREADSHEET_POLICIES.values() if rows < policy.crossover)
        )


class DocumentPlan(Struct, frozen=True):
    mode: DocumentMode
    node: DocumentNode
    params: dict[str, object]

    def typst_world(self) -> object:
        from typst import Compiler

        return Compiler(
            input=self.params.get("source"),
            font_paths=self.params.get("font_paths", []),
            sys_inputs=self.params.get("sys_inputs", {}),
        )

    async def produce(self) -> RuntimeRail[ContentKey]:
        return await async_boundary(f"document.{self.mode}", self._emit)

    async def _emit(self) -> ContentKey:
        data = (
            await to_process.run_sync(_gated_emit, self.mode.value, self.node, self.params)
            if self.mode in GATED
            else BACKENDS[self.mode](self, self.node)
        )
        return ContentIdentity.of(self.mode.value, data)


def _typst_compile(plan: DocumentPlan, node: DocumentNode) -> bytes:
    return plan.typst_world().compile(
        input=to_typst_source(node),
        output=None,
        sys_inputs=plan.params.get("sys_inputs", {}),
        pdf_standards=plan.params.get("pdf_standards"),
    )


def _typst_query(plan: DocumentPlan, node: DocumentNode) -> bytes:
    return plan.typst_world().query(
        selector=plan.params["selector"], field=plan.params.get("field"), one=plan.params.get("one", False)
    ).encode()


def _pypdfium2_raster(plan: DocumentPlan, node: DocumentNode) -> bytes:
    import pypdfium2

    pdf = pypdfium2.PdfDocument(plan.params["source"], password=plan.params.get("password"), autoclose=True)
    try:
        target = RenderTarget(plan.params.get("target", RenderTarget.PIL))
        # RESEARCH: PdfDocument.__len__ page-count member is unverified (.api/pypdfium2 rows get_page/get_toc, no __len__).
        pages = plan.params.get("pages") or range(len(pdf))
        bitmaps = (pdf.get_page(index).render(**_RENDER_KEYWORDS(plan.params)) for index in pages)
        frames = [getattr(bitmap, target.bridge)() for bitmap in bitmaps]
        head, *rest = frames
        sink = io.BytesIO()
        head.save(sink, format=plan.params.get("format", "TIFF"), save_all=len(frames) > 1, append_images=rest)
        plan.params["outline"] = tuple(_outline_rows(pdf, int(plan.params.get("max_depth", _OUTLINE_MAX_DEPTH))))
        return sink.getvalue()
    finally:
        pdf.close()


def _outline_rows(pdf: object, max_depth: int) -> Iterator[OutlineRow]:
    for mark in pdf.get_toc(max_depth=max_depth):
        # RESEARCH: PdfBookmark.level/get_title/get_dest and PdfDest.get_index accessor members are
        # unverified (.api/pypdfium2 rows PdfBookmark/PdfDest as types without enumerating accessors).
        yield (mark.level, mark.get_title(), dest.get_index() if (dest := mark.get_dest()) else -1)


# RESEARCH: PdfPage.render keyword surface (scale/rotation/draw_annots) is the unverified render-policy
# keyword set the .api/pypdfium2 [01] row names as a "render policy" without enumerating its keywords.
def _RENDER_KEYWORDS(params: dict[str, object]) -> dict[str, object]:
    return {
        "scale": float(params.get("scale", _RASTER_SCALE)),
        "rotation": int(params.get("rotation", 0)),
        "draw_annots": bool(params.get("annotations", True)),
    }


def _font_embed(plan: DocumentPlan, node: DocumentNode) -> bytes:
    from reportlab.pdfbase import pdfmetrics
    from reportlab.pdfbase.ttfonts import TTFont
    from reportlab.pdfgen.canvas import Canvas

    subfont_index = int(plan.params.get("subfont_index", 0))
    for face, blob in plan.params["subset_fonts"].items():
        pdfmetrics.registerFont(TTFont(face, io.BytesIO(blob), subfontIndex=subfont_index))
    canvas = Canvas(io.BytesIO(), pdfVersion=plan.params.get("pdf_version", (1, 7)))
    canvas.setTitle(plan.params.get("title", node.meta.key.hex))
    canvas.setAuthor(plan.params.get("author", ""))
    canvas.setSubject(plan.params.get("subject", ""))
    for page in [page for page in walk(node) if isinstance(page, PageNode)] or [node]:
        for run in (leaf for leaf in walk(page) if isinstance(leaf, RunNode)):
            origin = run.meta.bounds or (0.0, 0.0, 0.0, 0.0)
            canvas.setFont(run.font_key, run.size)
            canvas.drawString(origin[0], origin[1], run.text)
        canvas.showPage()
    canvas.save()
    return canvas.getpdfdata()


def _docxtpl_emit(plan: DocumentPlan, node: DocumentNode) -> bytes:
    from docx.shared import Pt
    from docxtpl import DocxTemplate, InlineImage, Listing, RichText, RichTextParagraph

    template = DocxTemplate(plan.params["template"])
    for kind in _REPLACE:
        for source, destination in plan.params.get(f"replace_{kind}", {}).items():
            getattr(template, f"replace_{kind}")(source, destination)
    context: dict[str, object] = {}
    for child in walk(node):
        role = child.meta.role
        match child:
            case RunNode():
                context.setdefault(role, RichText()).add(child.text, **_docx_run_props(template, child, plan.params))
            case BlockNode(block=BlockKind.CODE, runs=runs):
                context[role] = Listing("".join(run.text for run in runs))
            case BlockNode(runs=runs, block=block):
                paragraph = context.setdefault(role, RichTextParagraph())
                for run in runs:
                    paragraph.add(RichText(run.text, **_docx_run_props(template, run, plan.params)), parastyle=block.value)
            case FigureNode(asset_key=asset_key, meta=meta):
                bounds = meta.bounds or (0.0, 0.0, 0.0, 0.0)
                context[role] = InlineImage(
                    template,
                    plan.params["assets"][asset_key.hex],
                    width=Pt(bounds[2]) if bounds[2] else None,
                    height=Pt(bounds[3]) if bounds[3] else None,
                    anchor=plan.params.get("anchors", {}).get(role),
                )
            case SectionNode():
                context[role] = template.new_subdoc()
    plan.params["undeclared"] = template.get_undeclared_template_variables()
    template.render(context, autoescape=True)
    sink = io.BytesIO()
    template.save(sink)
    return sink.getvalue()


def _docx_run_props(template: object, run: RunNode, params: dict[str, object]) -> dict[str, object]:
    url = params.get("links", {}).get(run.meta.role)
    return {
        "bold": run.weight >= _BOLD_WEIGHT,
        "size": int(run.size * _PT_PER_HALFPT),
        "font": run.font_key,
        "rtl": run.rtl,
        "url_id": template.build_url_id(url) if url else None,
    }


def _xlsx_emit(plan: DocumentPlan, node: DocumentNode) -> bytes:
    grid = [tuple(CellValue.of(cell) for cell in row) for table in walk(node) if isinstance(table, TableNode) for row in table.rows]
    return SpreadsheetPolicy.select(plan, len(grid)).writer(plan, grid)


def _xlsx_streamed(plan: DocumentPlan, grid: list[tuple[CellValue, ...]]) -> bytes:
    import xlsxwriter

    sink = io.BytesIO()
    book = xlsxwriter.Workbook(
        sink,
        {
            "constant_memory": True,
            "in_memory": bool(plan.params.get("in_memory", True)),
            "use_zip64": len(grid) >= _ZIP64_ROW_THRESHOLD,
            "tmpdir": plan.params.get("tmpdir"),
        },
    )
    book.set_properties({"title": plan.params.get("title", plan.node.meta.key.hex), "author": plan.params.get("author", "")})
    sheet = book.add_worksheet(plan.params.get("sheet"))
    header, headed = _book_format(book, "header"), plan.params.get("header", True)
    formats = {kind: _book_format(book, kind) for kind in ("number", "datetime")}
    width = max((len(row) for row in grid), default=0)
    if width:
        sheet.set_column(0, width - 1, float(plan.params.get("column_width", _HEADER_WIDTH)))
    for index, row in enumerate(grid):
        sheet.set_row(index, None, header if index == 0 and headed else None)
        for column, value in enumerate(row):
            value.write_xlsxwriter(sheet, index, column, formats)
    if headed and grid:
        sheet.freeze_panes(1, 0)
        book.define_name(_HEADER_RANGE, f"='{sheet.name}'!$A$1:${_column_letter(width - 1)}$1")
    plan.params["sheets"] = tuple(other.name for other in book.worksheets())
    book.close()
    return sink.getvalue()


def _book_format(book: object, kind: str) -> object:
    fmt = book.add_format()
    for setter, argument in _FORMAT_SETTERS[kind]:
        getattr(fmt, setter)(*argument)
    return fmt


def _column_letter(index: int) -> str:
    letters = ""
    while index >= 0:
        index, remainder = divmod(index, 26)
        letters = chr(65 + remainder) + letters
        index -= 1
    return letters


def _xlsx_in_memory(plan: DocumentPlan, grid: list[tuple[CellValue, ...]]) -> bytes:
    import openpyxl

    book = openpyxl.Workbook(write_only=True)
    sheet = book.create_sheet(plan.params.get("sheet"))
    for row in grid:
        sheet.append([value.write_openpyxl() for value in row])
    if plan.params.get("header", True) and grid:
        sheet.freeze_panes = "A2"
    sink = io.BytesIO()
    book.save(sink)
    return sink.getvalue()


_SPREADSHEET_POLICIES: Final[MappingProxyType[str, SpreadsheetPolicy]] = MappingProxyType({
    "in-memory": SpreadsheetPolicy("in-memory", _STREAMING_ROW_THRESHOLD, _xlsx_in_memory),
    "streamed": SpreadsheetPolicy("streamed", 1 << 62, _xlsx_streamed),
})


def _hardened_parse(source: object) -> object:
    from lxml import etree

    return etree.parse(source, etree.XMLParser(resolve_entities=False, huge_tree=False, no_network=True))


def _lxml_emit(node: DocumentNode, params: dict[str, object]) -> bytes:
    from lxml import etree

    tree = to_lxml_tree(node)
    etree.cleanup_namespaces(tree)
    return etree.tostring(tree, xml_declaration=True, encoding="utf-8", pretty_print=bool(params.get("pretty", False)))


def _lxml_transform(node: DocumentNode, params: dict[str, object]) -> bytes:
    from lxml import etree

    transform = etree.XSLT(_hardened_parse(params["stylesheet"]))
    return bytes(
        transform(to_lxml_tree(node), **{key: etree.XSLT.strparam(value) for key, value in params.get("xslt_params", {}).items()})
    )


def _lxml_validate(node: DocumentNode, params: dict[str, object]) -> bytes:
    from lxml import etree

    validator = getattr(etree, SchemaKind(params["schema_kind"]).value)(_hardened_parse(params["schema"]))
    tree = to_lxml_tree(node)
    validator.assertValid(tree)
    return etree.tostring(tree, xml_declaration=True, encoding="utf-8")


def _lxml_query(node: DocumentNode, params: dict[str, object]) -> bytes:
    from lxml import etree

    query = etree.XPath(params["path"], namespaces=params.get("namespaces", {}), smart_strings=False)
    result = query(to_lxml_tree(node))
    # RESEARCH: etree.iselement element-test predicate is unverified (.api/lxml rows etree.tostring/XPath
    # but no iselement row); stays marked until the lxml catalogue rows the element-test surface.
    return _DOCUMENT_ENCODER.encode(
        [etree.tostring(hit).decode() if etree.iselement(hit) else hit for hit in result] if isinstance(result, list) else result
    )


BACKENDS: Final[MappingProxyType[DocumentMode, Emitter]] = MappingProxyType({
    DocumentMode.PDF_AUTHOR: _reportlab_author,
    DocumentMode.PDF_HTML: _weasyprint_html,
    DocumentMode.PDF_TYPST: _typst_compile,
    DocumentMode.TYPST_QUERY: _typst_query,
    DocumentMode.TYPST_DATA: _typst_compile,
    DocumentMode.PDF_RENDER: _pymupdf_render,
    DocumentMode.PDF_RASTER: _pypdfium2_raster,
    DocumentMode.PDF_ASSEMBLE: _pypdf_assemble,
    DocumentMode.PDF_REPAIR: _pikepdf_repair,
    DocumentMode.FONT_EMBED: _font_embed,
    DocumentMode.DOCX: _docx_emit,
    DocumentMode.DOCX_TEMPLATE: _docxtpl_emit,
    DocumentMode.PPTX: _pptx_emit,
    DocumentMode.XLSX: _xlsx_emit,
    DocumentMode.YAML: _ruamel_emit,
    DocumentMode.TOML: _tomlkit_emit,
})

GATED_ARMS: Final[MappingProxyType[DocumentMode, GatedArm]] = MappingProxyType({
    DocumentMode.XML: _lxml_emit,
    DocumentMode.XML_TRANSFORM: _lxml_transform,
    DocumentMode.XML_VALIDATE: _lxml_validate,
    DocumentMode.XML_QUERY: _lxml_query,
})


def _gated_emit(mode: str, node: DocumentNode, params: dict[str, object]) -> bytes:
    return GATED_ARMS[DocumentMode(mode)](node, params)
```

## [03]-[RESEARCH]

- [LOWERING_BODIES]: each `BACKENDS` arm is a lowering fold FROM the `documents/model#NODE` `DocumentNode` tree on the cp315 core — reportlab `Canvas`/flowable build over the tree's `page`/`block`/`run` nodes, weasyprint `HTML.write_pdf` over the tree serialized to HTML, typst `Compiler.compile(input=to_typst_source(node), output=None, pdf_standards=...)` (the `PDF_TYPST` arm verified against the folder `.api` catalogue for `typst`; `output=None` returns PDF `bytes`, `pdf_standards` selects the PDF/A archival target, `sys_inputs` binds runtime data through `sys.inputs`), pymupdf `Page.get_pixmap`/`Document.tobytes`, pypdfium2 `PdfDocument`, pypdf `PdfWriter`, pikepdf `Pdf.save(linearize=True)`, python-docx/python-pptx `Document`/`Presentation` over the tree, openpyxl `Workbook`, ruamel-yaml `YAML().dump`, tomlkit `dumps`. The `to_typst_source(node)` projection is the module-level lowering defined once on the `DocumentNode` tree at `documents/model#NODE` (the tree is a `type` union alias, so the lowering is a free function over the tree, never a method on the alias) so the three Typst rows share one lowering, never three string templates. The four gated `XML`/`XML_TRANSFORM`/`XML_VALIDATE`/`XML_QUERY` modes never enter `BACKENDS`: `_gated_emit` runs on the `python_version<'3.15'` band through `anyio.to_process.run_sync`, dispatches the mode value through `GATED_ARMS` over the tree and `params`, and each lxml arm imports `from lxml import etree` at boundary scope inside the gated-band worker.
- [TYPST_INTROSPECTION]: the held `typst.Compiler` world is the reusable font-cached compiler the owner amortizes across a batched render — `typst_world()` constructs one `Compiler(input, font_paths, sys_inputs)` and the `PDF_TYPST`/`TYPST_DATA`/`TYPST_QUERY` rows call `Compiler.compile`/`Compiler.query` against it rather than the single-shot module `compile`, so font loading is paid once per batch (catalogue compiler axis: a fresh `Compiler` per render in a batch is the rejected form). `TYPST_QUERY` is the produce-then-introspect inverse: `Compiler.query(selector, field, one)` extracts structured element data (a TOC/index feeding `reports/report#REPORT`) parallel to the PDF document-node seam; `TYPST_DATA` injects runtime data through `sys_inputs` with no string templating. The `Compiler`/`Compiler.compile`/`Compiler.query`/`Compiler.eval`/`compile(sys_inputs, pdf_standards)`/`query(selector, field, one)`/`Fonts`/`FontInfo` spellings verify against the folder `.api` catalogue for `typst`.
- [SPELLINGS]: the cp315-core dists resolve in the active lock, and the reportlab `Canvas`/`SimpleDocTemplate`, weasyprint `HTML.write_pdf`, pymupdf `Page.get_pixmap`/`Document.tobytes`, pikepdf `Pdf.save(linearize=True)`, and python-pptx `Presentation` member spellings verify against the folder `.api` catalogues for each owning package; the gated `lxml` `etree.tostring` spellings verify against the `lxml` catalogue, reflected on the `python_version<'3.15'` band (no CPython 3.15 wheel), and never resolve on the cp315 core.
- [PDFIUM_RASTER]: the `PDF_RASTER` arm is the cp315-core PDFium rasterizer that keeps the render path alive without the gated subprocess hop. `_pypdfium2_raster` opens one `PdfDocument(input, password, autoclose=True)`, folds each selected page resolved through `PdfDocument.get_page(index)` (the catalogue page accessor, never the uncatalogued subscript) and `PdfPage.render(...)` → the `RenderTarget`-selected frame bridge, and saves the frame sequence into one multi-frame image; `_outline_rows` harvests the document outline through `PdfDocument.get_toc(max_depth)`. The `RenderTarget` `StrEnum` is the one row collapsing the `to_pil`-versus-`to_numpy` bridge: each member's value IS the bridge method name (`PIL = "to_pil"`, `NUMPY = "to_numpy"`) and the `bridge` property returns it, so the raster sink is one `getattr(bitmap, target.bridge)()` call keyed by the row value — a NumPy raster (the high-fidelity frame feeding a compute consumer) is a row, never a parallel arm and never a call-site ternary re-deriving the bridge the row already names. The `PdfDocument(input, password, autoclose)`/`PdfDocument.get_page`/`PdfPage.render`/`PdfBitmap.to_pil`/`to_numpy`/`PdfDocument.get_toc(max_depth)`/`PdfBookmark`/`PdfDest` spellings verify against the folder `.api` catalogue for `pypdfium2` rows `[01]`/`[02]`/`[03]`/`[04]`/`[06]`. The pypdfium2-versus-pymupdf render overlap is correct and load-bearing — pymupdf `Page.get_pixmap` owns native render/extract/redact, pypdfium2 owns the high-fidelity PDFium render and outline inspection — both kept, neither a wrapper of the other. The `PdfPage.render` keyword arguments (`scale`, `rotation`, `draw_annots`), the `len(pdf)` `PdfDocument.__len__` page-count member backing the all-pages `range`, and the `PdfBookmark.level`/`get_title`/`get_dest`/`PdfDest.get_index` accessor member spellings are RESEARCH (the `.api` catalogue rows `PdfPage.render` as a "render policy" without enumerating its keyword set, rows `PdfDocument.get_page`/`get_toc` but no `__len__` page-count accessor, and names `PdfDocument.get_toc -> Iterator[PdfBookmark]` and the `PdfBookmark`/`PdfDest` types but not their accessor members); the `PdfDocument`, `render`, `to_pil`, `to_numpy`, and `get_toc` entrypoints are settled fence code, so the unverified render-keyword set rides the explicitly `# RESEARCH`-marked `_RENDER_KEYWORDS` projection and the unverified bookmark/dest accessors ride the explicitly `# RESEARCH`-marked `_outline_rows` body — neither presented as settled fence code — and both stay marked until the `pypdfium2` catalogue rows the render-policy keyword surface and the outline-node accessor surface.
- [FONT_EMBED]: the `FONT_EMBED` arm embeds the `typography/conformance#CONFORM`-subsetted font INTO the emitted PDF so a subset font the document references is not dead capability — the `SUBSET`→`EMBED`→PDF/A chain closing the gap `VARFONT_PARTIAL_INSTANCE` leaves open. `_font_embed` registers each subsetted face through reportlab `pdfbase.ttfonts.TTFont(face, io.BytesIO(blob), subfontIndex=...)` + `pdfbase.pdfmetrics.registerFont(font)` (verified against the `reportlab` catalogue rows `[01]`/`[02]`, the in-memory `io.BytesIO` buffer the `TTFont(name, filename, ...)` `filename` accepts), sets the PDF info dictionary through `Canvas.setTitle`/`setAuthor`/`setSubject` (catalogue row `[06]`), draws every `RunNode` at its `NodeMeta.bounds` with `Canvas.setFont(font_key, size)`/`drawString(x, y, text)` (catalogue rows `[04]`/`[01]`) so the embedded face is the document font table, and returns the PDF through `Canvas.getpdfdata()` (catalogue row `[04]`, the in-memory bytes path the `LOCAL_ADMISSION` law names) rather than a temp-file round-trip. The `subset_fonts` `params` is the `dict[str, bytes]` face-to-bytes map the `typography/conformance#CONFORM` `SUBSET`/`INSTANCE` rows produce — `_subset_font`/`_instance_font` return the `TTFont.save`-serialized bytes, consumed here verbatim. Subsetting and variable-font instancing are NEVER re-run here — fontTools `Subsetter.subset`/`varLib.instancer` stay at `typography/conformance#CONFORM` (`SUBSET`/`INSTANCE` rows); the archival PDF/A output-intent and the PAdES cryptographic close that consume the embedded font also route there. The pymupdf `Page.insert_font`/`insert_text` embed path is RESEARCH (the `pymupdf` catalogue names `Font` as a font handle and rows `Page.insert_image`/`Document.subset_fonts` but no `insert_font`/`insert_text` entrypoint), so the settled font-embed fence composes the reportlab `registerFont` path only and never the unverified pymupdf member.
- [DOCX_TEMPLATE]: the `DOCX_TEMPLATE` arm binds the `DocumentNode` tree into a Word template through docxtpl, reusing the `reports/report#REPORT` jinja2 the way `TYPST_DATA` reuses `sys_inputs` — one template-bind concept across Typst and Office, never an ad-hoc python-docx paragraph-stitch. `_docxtpl_emit` loads one `DocxTemplate(template_file)`, runs the pre-render part-swap fold (the `replace_media`/`replace_embedded`/`replace_zipname`/`replace_pic` catalogue rows `[07]`-`[10]` collapsed into one `_REPLACE` `(media, embedded, zipname, pic)` tuple folded through `getattr(template, f"replace_{kind}")(source, destination)` over the `params["replace_{kind}"]` maps, so swapping an embedded asset before render is one table row keyed by part kind rather than four sibling `replace_*` call sites), then folds the tree in ONE `walk` pass through one `match` over node kind into the role-keyed `context` dict — `RunNode`→`RichText().add(text, **_docx_run_props)` accumulated into the role's shared `RichText` via `setdefault`, `BlockNode(block=BlockKind.CODE)`→`Listing("".join(run.text))` (the `Listing` newline-preserving carrier lowered FROM the `CODE` block, never a `params["listings"]` side-channel that bypasses the tree), every other `BlockNode`→`RichTextParagraph().add(RichText(...), parastyle=block.value)` (the block's `BlockKind` driving the paragraph style), `FigureNode`→`InlineImage(tpl, descriptor, width=Pt(bounds[2]), height=Pt(bounds[3]), anchor=...)` reading both bounds axes in the `python-docx` `shared.Pt` point unit (catalogue row `[09]`, the bounds-native unit, never the uncatalogued `Mm`) and the per-role anchor, and `SectionNode`→`new_subdoc()`, then reads `get_undeclared_template_variables()` into `params["undeclared"]` as receipt evidence and `save(BytesIO())`. `_docx_run_props` is the one shared run-style projection: `bold` from `weight >= _BOLD_WEIGHT`, `size` in half-points, `font` from `RunNode.font_key` (the registered face the `CONFORM` shape minted, threaded through the `RichText.add(font=...)` row rather than dropped), `rtl`, and the hyperlink `url_id` resolved through `template.build_url_id(url)` when `params["links"]` names a URL for the run's role, so a link run is the `RichText.add(url_id=...)` row the catalogue carries rather than dead capability. The single-pass `match` fold is the collapse: the five parallel `walk`+`isinstance` comprehensions over `RunNode`/`BlockNode`/`FigureNode`/`Listing`/`SectionNode` dissolve into one dispatch keyed by node kind, so a new carrier kind is one `match` arm, never a sixth loop. The `DocxTemplate.__init__`/`render`/`save`/`new_subdoc`/`get_undeclared_template_variables`/`build_url_id`, `RichText(text, **prop)`/`add(... url_id, rtl ...)`, `RichTextParagraph()`/`add(text, parastyle)`, `Listing(text)`, and `InlineImage(tpl, image_descriptor, width, height, anchor)` spellings verify against the folder `.api` catalogue for `docxtpl` rows `[01]`-`[10]` and the content-carrier rows `[01]`-`[06]`. `new_subdoc()` returns a `Subdoc` requiring the optional `docxcompose` dependency (catalogue `[02] type scope`/`[04] boundary`); when a `SectionNode` subdoc is bound, `docxcompose` is a transitive admission the manifest carries, marked RESEARCH until the lock reflects it. docxtpl authors templates and is not part of `documents/lens#LENS` ingest — it belongs only to this emit axis.
- [XLSX_STREAMING]: the `XLSX` arm is ONE emitter selecting a `SpreadsheetPolicy` row by corpus size — the Q/collapse landing before the F rows. The collapse merges the former `_xlsx_streamed`/`_xlsx_in_memory`/`_cell_value` triplet into the policy-as-value `SpreadsheetPolicy` `Struct` (a `(key, crossover, writer)` row whose `writer` delegate carries its own streaming behavior, the `_SPREADSHEET_POLICIES` table the two rows live in) plus the `CellValue` typed-cell algebra (one `msgspec.Struct` whose `CellValue.of` projects a grid cell to its `str | float | bool | datetime | date | None` payload once and whose `write_xlsxwriter`/`write_openpyxl` `match` lowers it to the typed Excel write), so the per-cell `isinstance` ladder and the parallel writer functions both dissolve. `CellValue.of` carries a `FieldNode` value verbatim (its `str | bool | None` is already typed) and runs the recovered `RunNode` text through `_coerce_cell` — the one narrowing projection lifting a grid string to `bool` (the `_BOOL_CELL` row table), `int`/`float` (numeric parse), or `datetime`/`date` (`fromisoformat`) so the `write_number`/`write_datetime`/`write_boolean` arms are LIVE, never decorative arms a text-only projection never reaches; an unparseable string stays a `str` write and the empty cell folds to `write_blank`. `_xlsx_emit` projects the `TableNode` grid through `CellValue.of`, then `SpreadsheetPolicy.select(plan, len(grid)).writer(plan, grid)` dispatches the row: the `streamed` row's `writer` is `_xlsx_streamed` (xlsxwriter `Workbook(sink, {"constant_memory": True, "in_memory": ..., "use_zip64": ..., "tmpdir": ...})` → `set_properties` core metadata → `add_worksheet` → the `_book_format` data-table style minting that drives the `Format` `set_bold`/`set_align`/`set_num_format` setter rows off the `_FORMAT_SETTERS` table rather than a properties-dict → `set_column` header-table width policy → `Worksheet.set_row`/`write_number`/`write_string`/`write_datetime`/`write_boolean`/`write_blank` → `freeze_panes` + `define_name` naming the header row range + `worksheets()` sheet-name harvest, flushing each completed row to a temp file at O(1) row memory and sealing a row when a higher row is touched), the `in-memory` row's `writer` is `_xlsx_in_memory` (openpyxl `Workbook(write_only=True)`/`create_sheet`/`Worksheet.append`/`freeze_panes`/`save`). `_book_format` is the one parameterized `Format` factory folding the `_FORMAT_SETTERS` `(setter, argument)` rows through `getattr`, never three sibling `_header_format`/`_number_format`/`_datetime_format` factories. The `_column_letter` base-26 kernel building the `define_name` A1 range is RESEARCH-pending collapse: the `xlsxwriter.utility.xl_col_to_name`/`xl_range` A1-notation owners are the admitted replacement, but the `.api` catalogue rows only `Workbook`/`Worksheet`/`Format` and never the `utility` module, so the local kernel stands as settled fence code until the `xlsxwriter` catalogue rows the `utility` A1-notation surface. xlsxwriter is already present transitively; the action is promote-to-explicit on the cp315 core, zero new resolver pressure. The `Workbook(filename, options)`/`add_worksheet`/`add_format`/`set_properties`/`define_name`/`worksheets`/`close`, the typed `Worksheet.write_number`/`write_string`/`write_datetime`/`write_boolean`/`write_blank`/`set_row`/`set_column`/`freeze_panes`, `Format.set_num_format`/`set_bold`/`set_align`, and the `constant_memory`/`in_memory`/`use_zip64`/`tmpdir` `options` keys verify against the folder `.api` catalogue for `xlsxwriter` rows `[01]`-`[07]` and `[04]`-`[12]`/`[01]`-`[05]`; the openpyxl `Workbook(write_only=True)`/`create_sheet`/`Worksheet.append`/`freeze_panes`/`save` spellings verify against the folder `.api` catalogue for `openpyxl` rows `[01]`/`[04]`/`[02]`/`[07]`. Under `constant_memory` cells are written in strict row-major order — the typed writers ascend the row index, an out-of-order write to a sealed row is silently dropped, and `set_row` height/format applies before a row's cells; the grid fold preserves row order so the streaming contract holds. The `set_row(index, None, format)` third-positional cell-format argument under `constant_memory` verifies against catalogue row `[11]` (the `cell_format` parameter applied before the row's cells), and `set_column(first, last, width)` applies the column-width default before the first row write per row `[10]`.
- [XML_TRANSFORM_VALIDATE]: the gated lxml arm carries four rows on the one `XML` band — `etree.tostring` emit, `etree.XSLT` compiled transform, `XMLSchema`/`RelaxNG`/`Schematron` `assertValid` validation raising `DocumentInvalid`, and `etree.XPath` compiled introspection (the structured-introspection inverse of `TYPST_QUERY`). `_lxml_transform` compiles one `etree.XSLT(_hardened_parse(stylesheet))` and applies it with `etree.XSLT.strparam` arguments; `_lxml_validate` resolves the validator by `getattr(etree, SchemaKind(...).value)` over `etree.XMLSchema`/`etree.RelaxNG`/`etree.Schematron` and calls `assertValid`, the failure raising `DocumentInvalid` on the structured-documents fault rail (the `SchemaKind` `StrEnum` values are the exact validator-class attribute names, so the dialect dispatch is one `getattr`, never a three-arm `match`); `_lxml_query` compiles one reusable `etree.XPath(path, namespaces, smart_strings=False)` and projects element hits through `etree.tostring`, encoding the result list through the shared `_DOCUMENT_ENCODER`. The `etree.parse`/`etree.XMLParser`/`etree.tostring`/`etree.cleanup_namespaces`/`etree.XSLT`/`etree.XMLSchema`/`etree.RelaxNG`/`etree.Schematron`/`XMLSchema.assertValid`/`etree.XPath`/`_Element.xpath`/`DocumentInvalid` spellings verify against the folder `.api` catalogue for `lxml` parse/serialize rows `[01]`/`[07]`/`[08]`/`[09]` and the query/transform/validate rows `[01]`/`[03]`/`[04]`/`[05]`, reflected on the `python_version<'3.15'` band, never on the cp315 core. The `etree.XSLT.strparam` static argument-quoter and the `etree.iselement` element-test predicate are RESEARCH (the `.api` catalogue rows `etree.XSLT` `[03]` as the compiled-stylesheet surface without enumerating the `strparam` member and rows the serialize/query surface without an `iselement` element-test row), so both stay marked until the `lxml` catalogue rows the XSLT argument-quoting and element-test surfaces. The `XMLParser(resolve_entities=False, huge_tree=False, no_network=True)` hardening on the stylesheet and schema parse is the catalogue parser-axis knob set (`[03] type `etree.XMLParser`` carries recover/huge_tree/schema/resolvers), closing the entity-expansion, unbounded-tree, and network-fetch surface; `no_network=True` is RESEARCH (the catalogue names `etree.XMLParser` as the tunable parser carrying resolvers but does not enumerate the `no_network` keyword), so `resolve_entities=False`/`huge_tree=False` are settled and `no_network=True` stays marked until the `lxml` catalogue rows the parser keyword set. `to_lxml_tree(node)` is the intended `DocumentNode`→`etree._Element` lowering the four arms share, the parallel to `to_typst_source` over the same tree, but it is RESEARCH: `documents/model#NODE` owns `to_typst_source`/`children`/`walk`/`node_digest`/`encode`/`decode` and the model page does NOT yet declare `to_lxml_tree`, so the cross-page member stays marked until `model.md` rows the XML lowering alongside the Typst one; until then the four lxml arms import `to_lxml_tree` against the named owner without transcribing a settled-on-model member.

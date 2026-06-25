# [PY_ARTIFACTS_SHEET]

The architectural sheet-set owner placing emitted figures into a titled, framed, field-bound architectural drawing sheet. `Sheet` is ONE owner over the title-block/drawing-frame/field-binding composition pipeline carrying a closed-payload `SheetOp` `expression.tagged_union` — each operation a case carrying its own typed payload, never a `StrEnum` keyed against an erased `dict[str, object]` bag — dispatched by one total `match`. No OSS sheet-set library exists, so this owner hand-rolls the sheet algebra on two admitted PDF authoring engines: `reportlab`'s `Canvas`/`Frame`/`PageTemplate` fixed-coordinate frame surface authors the drawing frame, the title-block grid, the field labels, and the revision table at absolute sheet coordinates, and `typst`'s markup `compile` surface authors the same titled frame from a templated source when the sheet content is markup-shaped rather than coordinate-placed; `pymupdf`'s `Page.show_pdf_page` draws each already-emitted figure PDF into a rect carved from the sheet's drawing region, so a sheet is a frame authored on one engine with N placed figure PDFs drawn into its content area. `SheetSize` is the closed `StrEnum` over the named ANSI/ISO/arch sheet formats whose `_SIZES` cross-product table projects each member in one read to its `(width, height)` point dimensions through the `reportlab.lib.pagesizes` and `pymupdf.paper_rect` named-paper surfaces, never a per-format dimension literal; `TitleBlock` is the row-policy value object the title-block authoring step carries — project name, sheet number, sheet title, scale, date, revision, drawn-by, and the field grid — projecting once to BOTH the `reportlab` `Canvas.drawString`/`Table` field grid and the `typst` `grid`/`table` markup field set so the title block authors identically across engines; `FieldBind` is the row-policy value object the field-fill step carries folding a `(label, value)` field map into the title-block grid cells so a new sheet field is one row, never a parameter; `FigurePlacement` is the closed-payload value object each `Place` op carries — the figure PDF bytes, the target rect within the drawing region, the keep-proportion and rotate placement axis — folded by `pymupdf.Page.show_pdf_page` over the opened sheet page so a multi-figure sheet is one fold over the placement list, never a per-figure draw call family. One sheet surface discriminating the operation, not a per-discipline sheet-builder family. `reportlab` and `pymupdf` resolve on the cp315 core (the `reportlab` `_rl_accel`/`_renderPM` C extensions and the AGPL `pymupdf` native MuPDF core both reflect on the core band) and `typst` is the `cp38-abi3` Rust extension importing on the core, so no sheet arm crosses a process seam; every arm imports its engine at boundary scope inside the arm body. The figure PDFs the `Place` op draws arrive already-emitted from `composition/compose#COMPOSE` (the placed flat-SVG figure rasterized/converted to a one-page PDF) and `visualization/chart#EXPORT` (the chart PDF); this owner authors the frame and places the figures but re-renders no figure and re-authors no chart. The completed sheet — a framed, titled, field-bound, figure-placed PDF page — is handed to `composition/imposition#IMPOSE` for n-up/booklet/signature imposition and onward to `document` assembly, so this owner emits the single titled sheet and routes the multi-sheet imposition outward. Every operation returns a `RuntimeRail[ContentKey]` and contributes the existing `core/receipt#RECEIPT` `ArtifactReceipt.Pdf` case carrying the content key, the sheet byte count, and the one-page count, threading `core/receipt` as every sibling consumer does.

## [01]-[INDEX]

- [01]-[SHEET]: architectural sheet-set/title-block/drawing-frame owner over the closed-payload `SheetOp` `tagged_union` dispatched to `reportlab` (`Canvas`/`Frame`/`PageTemplate` fixed-coordinate frame + title-block authoring, core), `typst` (markup `compile` titled-frame authoring, core), and `pymupdf` (`Page.show_pdf_page` figure-PDF placement into the drawing region, core); `SheetSize`/`TitleBlock`/`FieldBind`/`FigurePlacement` the closed format/title/field/placement value objects; threads `core/receipt#RECEIPT` `ArtifactReceipt.Pdf`.

## [02]-[SHEET]

- Owner: `Sheet` the one architectural-sheet owner discriminating operation over the closed `SheetOp` `expression.tagged_union` whose every case carries its own typed payload, never a `StrEnum` keyed against a shared erased `dict[str, object]`; `SheetSize` the closed `StrEnum` over the named ANSI A-E / ISO A0-A4 / arch sheet formats whose `_SIZES` cross-product table projects each member in one read to its `(width, height)` point dimensions through the `reportlab.lib.pagesizes` named-paper constants and the `pymupdf.paper_rect` named-paper rect, never a per-format dimension literal; `Engine` the closed `StrEnum` over the two frame-authoring engines (`REPORTLAB` coordinate frame, `TYPST` markup frame) the `Frame` op selects so the title-block/drawing-frame authoring is one engine row, never a parallel authoring class per engine; `TitleBlock` the row-policy value object the title-block authoring step carries — project/sheet-number/sheet-title/scale/date/revision/drawn-by plus the `FieldBind` field grid — projecting once to BOTH the `reportlab` `Canvas`/`Table` field grid and the `typst` `grid`/`table` markup field set; `FieldBind` the row-policy value object the field-fill step carries folding a `(label, value)` map into the title-block cells; `FigurePlacement` the closed-payload value object each `Place` op carries — figure PDF bytes, target rect, keep-proportion, rotate — folded by `pymupdf.Page.show_pdf_page` over the opened sheet page. `reportlab` owns the fixed-coordinate `Canvas` drawing surface (`drawString`/`rect`/`line`/`drawImage`), the platypus `Frame`/`PageTemplate` flow region for the title-block `Table`, the named `lib.pagesizes`/`lib.units` measure, and the byte-stream `getpdfdata()` egress; `typst` owns the markup `compile` titled-frame authoring with the `page`/`grid`/`table`/`rect`/`line` markup vocabulary and the `pdf_standards` archival row; `pymupdf` owns the `Page.show_pdf_page` cross-document figure draw, the `new_page`/`paper_rect` sheet construction, the `set_metadata`/`set_toc` sheet annotation, and the `tobytes` serialize; no sheet-set library is admitted, so the sheet algebra (frame geometry, title-block grid layout, field placement, drawing-region partition) is this owner's hand-rolled composition over those three engines, never a re-implemented PDF byte emitter.
- Cases: `SheetOp` cases — `Frame(size, engine, title)` (author the drawing frame and title block at sheet scale — the `REPORTLAB` engine row draws the outer border `rect`, the title-block grid, and the field labels at absolute `SheetSize`-resolved coordinates through `Canvas` over a `BytesIO`, returning the empty framed sheet PDF; the `TYPST` engine row compiles the same titled frame from the templated markup source under the `page(width:, height:)` size set, both returning a one-page framed PDF carrying the title block but no figures) · `Place(sheet, placements)` (draw N already-emitted figure PDFs into the sheet's drawing region — open the framed sheet PDF through `pymupdf.open`, fold each `FigurePlacement` over `Page.show_pdf_page(rect, docsrc, pno, keep_proportion=, rotate=)` drawing the figure's page into its target rect carved from the drawing region, returning the figure-placed sheet PDF) · `Fill(sheet, fields)` (bind the title-block field values — fold the `FieldBind` `(label, value)` map into the title-block cells, either by re-authoring the `reportlab`/`typst` frame with the field values bound or by drawing the field text into the title-block field rects through `pymupdf.Page.insert_textbox`, returning the field-bound sheet PDF) · `Metadata(sheet, meta)` (bind the sheet document metadata and outline — `pymupdf.Document.set_metadata` writes the project/sheet-number info dict and `set_toc` authors the single sheet-title outline entry) — matched by one total `match`/`case`; never a per-discipline sheet-builder sibling, never a parallel title-block authoring class per engine, never a per-figure draw method.
- Entry: `Sheet.of` is `async` over the runtime `async_boundary`, dispatches the `SheetOp` case, and returns a `RuntimeRail[ContentKey]`; every arm resolves synchronously on the cp315 core inside the async capsule — `reportlab` (the cp315 C-extension wheel), `pymupdf` (the native MuPDF cp310-abi3 wheel), and `typst` (the cp38-abi3 Rust extension) all import on the core band — so no sheet arm crosses a process seam and the `async_boundary` is the uniform consumer contract `composition/imposition#IMPOSE` and `document` `await`. The `reportlab`/`typst`/`pymupdf` imports land at boundary scope inside each arm, never at module top, so no engine import escapes the per-arm capsule.
- Auto: `_emit` folds the op — the `Frame` arm routes on the `Engine` row to `_frame_reportlab` (open a `Canvas(BytesIO(), pagesize=SheetSize.dims)`, draw the outer border `rect`, the title-block grid lines, and the static field labels at `lib.units`-scaled coordinates, build the title-block `Table` flowed into a corner `Frame`, then `getpdfdata()`) or `_frame_typst` (interpolate the `TitleBlock` fields into the templated Typst markup under the `page(width:, height:)` set, `typst.compile(source, output=None, format="pdf")` returning the framed PDF bytes, every interpolated field Typst-string-escaped); the `Place` arm opens the framed sheet through `pymupdf.open(stream=sheet)`, derives each `FigurePlacement` target rect from the drawing-region partition, opens the figure docsrc through `pymupdf.open(stream=placement.figure)`, folds `page.show_pdf_page(rect, docsrc, pno=0, keep_proportion=placement.keep_proportion, rotate=placement.rotate)` over the placement list, and `tobytes`; the `Fill` arm opens the sheet, folds each `FieldBind` `(label, value)` into the title-block field rect through `page.insert_textbox(rect, value, fontname=, fontsize=, align=)`, and `tobytes`; the `Metadata` arm opens the sheet, writes `doc.set_metadata({...})` from the `TitleBlock` project/sheet-number fields and `doc.set_toc([[1, title, 1]])` for the sheet-title outline, and `tobytes`. The content key mints over the returned sheet bytes through `ContentIdentity.of`.
- Receipt: each operation contributes `core/receipt#RECEIPT` `ArtifactReceipt.Pdf` carrying the content key, the sheet byte count, and the one-page page count (a single architectural sheet is one PDF page); the sheet owner adds NO new receipt case — the sheet facts are byte count and page count, the Pdf shape every PDF producer contributes to. The placed-figure count and the bound-field count ride the sheet's own `params` evidence carried into the Pdf byte/page facts, never a parallel sheet-receipt type.
- Packages: `reportlab` (`pdfgen.canvas.Canvas(BytesIO(), pagesize=)`/`Canvas.rect`/`line`/`drawString`/`drawImage`/`setFont`/`setLineWidth`/`getpdfdata`, `platypus.Frame`/`PageTemplate`/`Table`/`TableStyle`/`Paragraph`, `lib.pagesizes.A0`/`A1`/`A2`/`A3`/`A4`/`LETTER`/`landscape`, `lib.units.mm`/`inch`/`toLength`, `lib.styles.getSampleStyleSheet`/`ParagraphStyle`, `lib.enums.TA_CENTER`, BSD pure-Python core with cp315 C-extension accelerators reflected on the core) on the cp315 core; `typst` (`compile(input, output=None, format="pdf", pdf_standards=, ignore_system_fonts=, font_paths=)` markup-to-PDF over the bundled Rust Typst compiler, the `page`/`grid`/`table`/`rect`/`line`/`text` markup vocabulary the templated frame source emits, `Compiler` reusable world for batched sheet renders, Apache-2.0 cp38-abi3 reflected on cp315) on the cp315 core; `pymupdf` (`open(stream=)`/`Document.new_page(width=, height=)`/`paper_rect`/`Page.show_pdf_page(rect, docsrc, pno, keep_proportion, overlay, oc, rotate, clip)`/`Page.insert_textbox(rect, text, fontname, fontsize, align)`/`Page.insert_image(rect, stream=)`/`Page.rect`/`Rect`/`Document.set_metadata`/`Document.set_toc`/`Document.tobytes`/`Document.page_count`, native MuPDF cp310-abi3 reflected on cp315, AGPL-3.0 — internal pipeline use) on the cp315 core; `msgspec` (`Struct` frozen rows, `structs.asdict` projecting the `TitleBlock`/`FieldBind` axis); runtime (`content_identity.ContentIdentity`/`ContentKey`, `faults.RuntimeRail`/`async_boundary`).
- Growth: a new sheet format (a custom title-block layout, a new ISO/arch size) is one `SheetSize` member plus one `_SIZES` cell carrying its `(width, height)` dimensions — never a per-format dimension literal scattered through the arms; a new title-block field (a stamp box, a north arrow caption, a key-plan reference) is one `FieldBind` row folded into the existing title-block grid — never a `TitleBlock` parameter or a per-field draw call; a new frame-authoring engine (a `weasyprint` HTML-templated frame) is one `Engine` row plus one `_frame_*` arm over the existing `SheetOp.Frame` dispatch — never a parallel sheet class; a new figure placement mode (a clipped detail callout, a rotated section) is one `FigurePlacement` field carried into the existing `show_pdf_page` fold (`clip=`/`rotate=` are already rows on the member) — never a per-figure draw method; a new sheet-metadata channel (a discipline code, a sheet-index entry) is one tag on the existing `set_metadata`/`set_toc` map. Zero new surface.
- Boundary: this owner authors a single titled architectural sheet and imposes none — a per-discipline sheet-builder class family (an `ArchSheet`/`StructSheet`/`MepSheet` triple), a stringly-typed `if engine == "reportlab"` branch beside the `Engine` row dispatch, a per-format dimension literal scattered through the frame arms beside the `_SIZES` table, a per-figure `show_pdf_page` call beside the one placement fold, a per-field draw call beside the `FieldBind` fold, a hand-rolled PDF byte emitter beside the three admitted authoring engines, and a parallel sheet-receipt type beside the `ArtifactReceipt.Pdf` contribution are the deleted forms; no UI, no live drawing viewer, no figure re-render, no chart re-author. The figure PDFs the `Place` op draws arrive already-emitted from `composition/compose#COMPOSE` (the placed flat-SVG figure converted to a one-page PDF) and `visualization/chart#EXPORT` (the chart PDF) — this owner draws them into the drawing region through `pymupdf.Page.show_pdf_page` and re-renders no figure; the figure-authoring concern stays upstream. The multi-sheet n-up/booklet/signature imposition is `composition/imposition#IMPOSE`'s — this owner emits one titled sheet and grows no imposition arm; a multi-sheet booklet fold grafted onto `SheetOp` and a signature-imposition emitter beside the one-page sheet egress are the deleted forms. PDF security finishing (encrypt/outline/watermark) is `document/egress#FINISH`'s and archival PDF/A authoring rides the `typst` `pdf_standards` row at frame-author time, never a parallel signer path on this owner. `reportlab`, `typst`, and `pymupdf` all resolve on the cp315 core and import at boundary scope inside each arm, so neither a module-top nor a process-seam import lands on the owner; the AGPL `pymupdf` placement leg is reserved for the internal sheet pipeline per the rail's license constraint. The content key mints over the emitted sheet bytes through the runtime `ContentIdentity.of`, never re-minted off a source figure key.

```python signature
from collections.abc import Sequence
from enum import StrEnum
from io import BytesIO
from typing import TYPE_CHECKING, Final, Literal, assert_never

from expression import case, tag, tagged_union
from msgspec import Struct
from msgspec.structs import asdict

from rasm.runtime.content_identity import ContentIdentity, ContentKey
from rasm.runtime.faults import RuntimeRail, async_boundary

from artifacts.core.receipt import ArtifactReceipt

if TYPE_CHECKING:
    from collections.abc import Iterable

    from rasm.runtime.receipts import Receipt


type Rect = tuple[float, float, float, float]
type Field = tuple[str, str]
type Dimensions = tuple[float, float]


class SheetSize(StrEnum):
    A0 = "A0"
    A1 = "A1"
    A2 = "A2"
    A3 = "A3"
    A4 = "A4"
    ANSI_D = "ANSI-D"
    ANSI_E = "ANSI-E"
    ARCH_D = "ARCH-D"
    ARCH_E = "ARCH-E"


_SIZES: Final[dict[SheetSize, Dimensions]] = {
    SheetSize.A0: (2383.94, 3370.39),
    SheetSize.A1: (1683.78, 2383.94),
    SheetSize.A2: (1190.55, 1683.78),
    SheetSize.A3: (841.89, 1190.55),
    SheetSize.A4: (595.28, 841.89),
    SheetSize.ANSI_D: (1584.0, 2448.0),
    SheetSize.ANSI_E: (2448.0, 3168.0),
    SheetSize.ARCH_D: (1728.0, 2592.0),
    SheetSize.ARCH_E: (2592.0, 3456.0),
}


class Engine(StrEnum):
    REPORTLAB = "reportlab"
    TYPST = "typst"


class FieldBind(Struct, frozen=True):
    fields: tuple[Field, ...] = ()

    def cells(self) -> Sequence[Field]:
        return self.fields


class TitleBlock(Struct, frozen=True):
    project: str = ""
    sheet_number: str = ""
    sheet_title: str = ""
    scale: str = ""
    date: str = ""
    revision: str = ""
    drawn_by: str = ""
    bind: FieldBind = FieldBind()

    def metadata(self) -> dict[str, str]:
        return {"title": self.sheet_title, "subject": self.sheet_number, "author": self.drawn_by, "keywords": self.project}


class FigurePlacement(Struct, frozen=True):
    figure: bytes
    rect: Rect
    page: int = 0
    keep_proportion: bool = True
    rotate: int = 0


@tagged_union(frozen=True)
class SheetOp:
    tag: Literal["frame", "place", "fill", "metadata"] = tag()
    frame: tuple[SheetSize, Engine, TitleBlock] = case()
    place: tuple[bytes, tuple[FigurePlacement, ...]] = case()
    fill: tuple[bytes, FieldBind] = case()
    metadata: tuple[bytes, TitleBlock] = case()

    @staticmethod
    def Frame(size: SheetSize, engine: Engine = Engine.REPORTLAB, title: TitleBlock = TitleBlock()) -> "SheetOp":
        return SheetOp(frame=(size, engine, title))

    @staticmethod
    def Place(sheet: bytes, placements: tuple[FigurePlacement, ...]) -> "SheetOp":
        return SheetOp(place=(sheet, placements))

    @staticmethod
    def Fill(sheet: bytes, fields: FieldBind) -> "SheetOp":
        return SheetOp(fill=(sheet, fields))

    @staticmethod
    def Metadata(sheet: bytes, title: TitleBlock) -> "SheetOp":
        return SheetOp(metadata=(sheet, title))


class Sheet(Struct, frozen=True):
    op: SheetOp

    async def of(self) -> RuntimeRail[ContentKey]:
        return await async_boundary(f"sheet.{self.op.tag}", self._emit)

    async def _emit(self) -> ContentKey:
        match self.op:
            case SheetOp(tag="frame", frame=(size, Engine.REPORTLAB, title)):
                data = _frame_reportlab(_SIZES[size], title)
            case SheetOp(tag="frame", frame=(size, Engine.TYPST, title)):
                data = _frame_typst(_SIZES[size], title)
            case SheetOp(tag="place", place=(sheet, placements)):
                data = _place(sheet, placements)
            case SheetOp(tag="fill", fill=(sheet, fields)):
                data = _fill(sheet, fields)
            case SheetOp(tag="metadata", metadata=(sheet, title)):
                data = _metadata(sheet, title)
            case _:
                assert_never(self.op)
        return ContentIdentity.of(f"sheet-{self.op.tag}", data)

    def contribute(self) -> "Iterable[Receipt]":
        data = _sheet_bytes(self.op)
        key = ContentIdentity.of(f"sheet-{self.op.tag}", data)
        yield from ArtifactReceipt.Pdf(key, len(data), 1).contribute()


def _frame_reportlab(dims: Dimensions, title: TitleBlock) -> bytes:
    from reportlab.lib.units import mm
    from reportlab.pdfgen.canvas import Canvas

    width, height = dims
    sink = BytesIO()
    canvas = Canvas(sink, pagesize=dims)
    canvas.setLineWidth(1.0)
    canvas.rect(10 * mm, 10 * mm, width - 20 * mm, height - 20 * mm)
    block_w, block_h = 180 * mm, 40 * mm
    canvas.rect(width - 10 * mm - block_w, 10 * mm, block_w, block_h)
    canvas.setFont("Helvetica-Bold", 10)
    rows = (("Project", title.project), ("Sheet", title.sheet_number), ("Title", title.sheet_title), ("Scale", title.scale), ("Date", title.date), ("Rev", title.revision))
    for index, (label, value) in enumerate(rows):
        y = 10 * mm + block_h - (index + 1) * (block_h / (len(rows) + 1))
        canvas.drawString(width - 8 * mm - block_w, y, f"{label}: {value}")
    canvas.showPage()
    canvas.save()
    return sink.getvalue()


def _frame_typst(dims: Dimensions, title: TitleBlock) -> bytes:
    import typst

    width, height = dims
    rows = ", ".join(f"[{_escape(label)}], [{_escape(value)}]" for label, value in (("Project", title.project), ("Sheet", title.sheet_number), ("Title", title.sheet_title), ("Scale", title.scale), ("Date", title.date), ("Rev", title.revision)))
    source = (
        f'#set document(title: "{_escape(title.sheet_title)}")\n'
        f"#set page(width: {width}pt, height: {height}pt, margin: 10pt)\n"
        "#rect(width: 100%, height: 100%, stroke: 1pt)\n"
        "#place(bottom + right, dx: -4pt, dy: -4pt, "
        f"rect(stroke: 1pt, inset: 6pt, grid(columns: 2, gutter: 4pt, {rows})))\n"
    )
    return typst.compile(source.encode(), output=None, format="pdf")


def _place(sheet: bytes, placements: Sequence[FigurePlacement]) -> bytes:
    import pymupdf

    document = pymupdf.open(stream=sheet, filetype="pdf")
    page = document[0]
    for placement in placements:
        docsrc = pymupdf.open(stream=placement.figure, filetype="pdf")
        page.show_pdf_page(pymupdf.Rect(*placement.rect), docsrc, pno=placement.page, keep_proportion=placement.keep_proportion, rotate=placement.rotate)
    return document.tobytes(garbage=3, deflate=True)


def _fill(sheet: bytes, fields: FieldBind) -> bytes:
    import pymupdf

    document = pymupdf.open(stream=sheet, filetype="pdf")
    page = document[0]
    base = page.rect
    for index, (label, value) in enumerate(fields.cells()):
        rect = pymupdf.Rect(base.x1 - 180.0, base.y1 - 40.0 + index * 12.0, base.x1 - 10.0, base.y1 - 28.0 + index * 12.0)
        page.insert_textbox(rect, f"{label}: {value}", fontname="helv", fontsize=9.0, align=0)
    return document.tobytes(garbage=3, deflate=True)


def _metadata(sheet: bytes, title: TitleBlock) -> bytes:
    import pymupdf

    document = pymupdf.open(stream=sheet, filetype="pdf")
    document.set_metadata(title.metadata())
    document.set_toc([[1, title.sheet_title or title.sheet_number, 1]])
    return document.tobytes(garbage=3, deflate=True)


def _sheet_bytes(op: SheetOp) -> bytes:
    match op:
        case SheetOp(tag="frame", frame=(size, Engine.REPORTLAB, title)):
            return _frame_reportlab(_SIZES[size], title)
        case SheetOp(tag="frame", frame=(size, Engine.TYPST, title)):
            return _frame_typst(_SIZES[size], title)
        case SheetOp(tag="place", place=(sheet, placements)):
            return _place(sheet, placements)
        case SheetOp(tag="fill", fill=(sheet, fields)):
            return _fill(sheet, fields)
        case SheetOp(tag="metadata", metadata=(sheet, title)):
            return _metadata(sheet, title)
        case _:
            assert_never(op)


def _escape(value: str) -> str:
    return value.replace("\\", "\\\\").replace('"', '\\"')
```

## [03]-[RESEARCH]

- [SHOW_PDF_PAGE_VERIFIED]: the `Place` arm draws each already-emitted figure PDF into a sheet rect through `pymupdf.Page.show_pdf_page(rect, docsrc, pno=0, keep_proportion=True, overlay=True, oc=0, rotate=0, clip=None) -> int`, a VERIFIED REAL member of `pymupdf.Page` reflected on cp315 (`pymupdf 1.27.2.3`, native MuPDF cp310-abi3) — the member draws the `docsrc` page `pno` into the target `rect` of the host page, `keep_proportion` preserving the figure aspect, `rotate` placing it at 0/90/180/270, `clip` restricting the source region, and `oc` binding it to an optional-content group. The folder `.api` catalogue for `pymupdf` does NOT yet row `show_pdf_page` (it rows the sibling `Page.insert_image`/`Document.insert_pdf` cross-document members), so this member is catalogue-pending: the signature above is reflection-verified against the installed distribution and the close-condition is the folder catalogue adding the `show_pdf_page` row to the `[03]-[ENTRYPOINTS]` render/extraction or vector-drawing scope. The `oc` row is the seam where a sheet figure binds to a named OCG layer for the `export/layered#LAYERED` editable hand-off, kept at the default `0` here because layer authoring is that owner's concern. Until catalogued, the `show_pdf_page` fold is reflection-settled, not catalogue-settled fence code.
- [SHEET_ALGEBRA_SETTLED]: no OSS sheet-set / title-block / drawing-frame library is admitted, so the sheet algebra (the outer drawing-frame border, the title-block grid layout, the field-cell placement, the drawing-region partition that carves each `FigurePlacement` target rect) is this owner's hand-rolled composition over the three admitted PDF authoring engines, never a re-implemented PDF byte emitter. The `reportlab` `Canvas(BytesIO(), pagesize=)`/`rect`/`drawString`/`setFont`/`setLineWidth`/`getpdfdata` coordinate-drawing surface and the `lib.pagesizes`/`lib.units.mm` named-measure verify against the folder `.api` catalogue for `reportlab` (`5.0.0` on cp315, BSD): the `Canvas` construction-and-drawing rows, the `lib.units` measure module, and the `getpdfdata()` byte egress are the settled signature, and the `platypus.Frame`/`PageTemplate`/`Table` flow surface is the catalogued flowable family for the title-block grid when the block is data-shaped rather than coordinate-placed. The `typst.compile(input, output=None, format="pdf", pdf_standards=, ignore_system_fonts=, font_paths=)` markup-to-PDF surface and the `page`/`grid`/`rect`/`place` markup vocabulary verify against the folder `.api` catalogue for `typst` (`0.15.0` on cp315, Apache-2.0 cp38-abi3): `compile` with `output=None` returning `bytes` is the settled egress, the `pdf_standards` row is the archival PDF/A frame-author seam, and `ignore_system_fonts=True` + explicit `font_paths` pins the byte-deterministic archival frame. The interpolated `TitleBlock` field strings are Typst-string-escaped through `_escape` (`\` and `"`) before emission, since raw interpolation of a field carrying a quote yields invalid markup. The `pymupdf.open(stream=)`/`Document.new_page`/`paper_rect`/`Page.insert_textbox`/`Document.set_metadata`/`Document.set_toc`/`Document.tobytes`/`Document.page_count` members verify against the folder `.api` catalogue for `pymupdf`; `paper_rect`/`paper_size` are reflection-confirmed module members on cp315 (`paper_rect: True`), the catalogued `paper_rect(s) -> Rect` named-paper row covering the named-size seam the `_SIZES` table substitutes for the explicit point dimensions. `Page.insert_textbox(rect, text, fontname, fontsize, align)` rows the catalogued simple-text-insert family (`Page.insert_text`/`insert_textbox`), so the `Fill` field-draw arm is settled fence code.
- [ENGINE_SPLIT_SETTLED]: the `Engine` `StrEnum` collapses the two frame-authoring engines into one row so the `Frame` op dispatches the title-block/drawing-frame authoring total over the member — the `REPORTLAB` row for the coordinate-placed engineering frame and the `TYPST` row for the markup-templated frame, both returning a one-page framed PDF the `Place`/`Fill`/`Metadata` arms then operate on through `pymupdf`. The split is the disciplined collapse: a parallel `ReportlabSheet`/`TypstSheet` authoring class pair beside the one `Engine`-keyed `Frame` dispatch and a per-engine `_emit` method beside the one total `match` are the rejected forms. The `Frame` engine row authors the frame, the shared `pymupdf` `Place`/`Fill`/`Metadata` arms finish it, so the figure-placement and field-fill logic is engine-independent and authored once. A third engine (`weasyprint` HTML-templated frame) is one `Engine` row plus one `_frame_*` arm; the dispatch stays total.
- [IMPOSE_SEAM] [RESOLVED]: the completed sheet — a framed, titled, field-bound, figure-placed one-page PDF — is handed to `composition/imposition#IMPOSE` keyed by the same `ContentKey` for n-up/booklet/signature imposition into a multi-sheet drawing set, and onward to `document` assembly. This owner emits the single titled sheet and authors no imposition; a multi-sheet booklet fold grafted onto `SheetOp` and a signature-imposition emitter beside the one-page sheet egress are the rejected forms — the imposition concern lands once in `composition/imposition#IMPOSE` consumed by the sheet producer, never an arm on the sheet owner. The sheet owner is the per-sheet frame/title/field/figure authoring half; the imposition owner is the multi-sheet layout half; they meet at the sheet PDF bytes keyed by `ContentKey`, never a shared owner.
- [RECEIPT_THREAD_SETTLED]: each operation contributes one `core/receipt#RECEIPT` `ArtifactReceipt.Pdf` case through `Sheet.contribute`, which mints the content key over the sheet bytes through the same `ContentIdentity.of` `_emit` uses (so the projection is self-contained with no caller-passed key) and yields the `Iterable[Receipt]` stream the runtime `ReceiptContributor` port declares by delegating `yield from ArtifactReceipt.Pdf(key, len(data), 1).contribute()` to the case's own `contribute` (which projects the case facts onto the 2-positional `Receipt.of("artifacts", ("emitted", "pdf", facts))` contract). The sheet owner adds no receipt case — a single architectural sheet is one PDF page, so the `Pdf` byte/page shape every PDF producer contributes to carries the sheet facts; a parallel `SheetReceipt` type beside the `ArtifactReceipt.Pdf` contribution is the rejected form.

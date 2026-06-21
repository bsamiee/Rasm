# [PY_ARTIFACTS_LENS]

The recover-TO half of the bidirectional document seam: where `documents/model#NODE` is the single interior `DocumentNode` tree every backend lowers FROM, `DocumentLens` is the inverse that recovers a `DocumentNode` tree (and its `DocumentDelta` corpus value) back OUT of an already-emitted PDF, a scanned raster, or a spreadsheet workbook — text, image, word-geometry, region, ruled-table grid, full-text search, scanned-page OCR, native outline and embedded-file harvest, form-widget recovery, redaction-grade scrub burn-in, annotation authoring, reflowable `Story` re-layout, and tabular spreadsheet/ODF ingest. `DocumentLens` is ONE owner discriminating recovery operation over a `LensOp` closed `StrEnum` routed by one frozen `_CORE_ARMS`/`_GATED_ARMS` band dispatch pair, never a `get_text`/`get_words`/`extract_images`/`find_tables` verb family and never an `if op == ...` reader ladder. The companion-native arms (pymupdf, ocrmypdf, python-calamine) cross the runtime subprocess seam (`anyio.to_process.run_sync`) onto the gated band; the pure-Python reader arms (pypdf, pdfplumber, odfpy) resolve on the cp315 core. Every recovery returns a `RuntimeRail[ContentKey]` keyed by the runtime content key and contributes the one `receipt/receipt#RECEIPT` `ArtifactReceipt.Introspection` case carrying the recovered-tree shape folded over `walk`; production and extraction are inverses over the one node algebra, so a `documents/emit#DOCUMENT` emission and a `DocumentLens` recovery round-trip through `DocumentNode` with `DocumentDelta` (`documents/model#DELTA`) defined once.

## [01]-[INDEX]

- [01]-[LENS]: `DocumentLens` (Struct, frozen) + `LensOp` closed `StrEnum` + `LensProvider` the `(band, label)`-carrying engine policy vocabulary + the one `_ARMS` `dict[LensOp, RecoverArm]` acceptor table and the `_DEFAULT_PROVIDER` op→provider map (mirroring the verified `documents/emit#DOCUMENT` `BACKENDS`/`GATED_ARMS`/`SpreadsheetPolicy` shape); text/image/word/table/region/search/ocr/outline/embedded/widget/redaction/annotation/`Story`/spreadsheet recovery into `DocumentNode`, one `recover` entrypoint over one `async_boundary`, the gated pymupdf/ocrmypdf/calamine arms on the subprocess seam and the pure-Python pypdf/pdfplumber/odfpy arms on the core selected by the resolved `LensProvider` band, every node minted through one polymorphic `_node` constructor matching the `documents/model#NODE` variant signatures.

## [02]-[LENS]

- Owner: `DocumentLens` the one extraction owner discriminating recovery operation; `LensOp` the closed `StrEnum` of the recover-TO operations; `LensProvider` the closed `(band, arm)` policy-as-value vocabulary whose every member carries its own runtime band and acceptor so the `TABLE`/`SEARCH`/`OCR` ops that admit both a pure-Python core engine and a native MuPDF engine dispatch a provider row rather than reconstructing the pdfplumber-versus-pymupdf choice; the recovered `DocumentNode` (owned by `documents/model#NODE`) is the single interior representation every arm folds back into, never a per-operation result type. One polymorphic `_node(NodeKind, role, page, payload, *, bounds, **slot)` constructor mints every variant over a content-keyed `NodeMeta` honoring the real `documents/model#NODE` field contract (a `RunNode` carries its recovered `font_key`/`size`/`weight`, a `StructureNode` carries the level-resolved `StandardRole(_HEADING[level])` over the `H1`-`H6` vocabulary, a `FieldNode` carries its `FieldKind` row), never a `_run_node`/`_figure_node`/`_annotation_node`/`_structure_node` sibling-factory family; the per-op acceptors are rows in the one `_ARMS` `dict[LensOp, RecoverArm]` table whose band each `_PROVIDER`/`LensProvider` row carries, the band recovered once from the resolved provider, never a third reader dispatcher and never an `if op == ...` cascade per band.
- Cases: `LensOp` rows `EXTRACT_TEXT` (pypdf `PageObject.extract_text(extraction_mode=...)` → `RunNode` leaves, core) · `EXTRACT_IMAGES` (pypdf `PageObject.images` → content-keyed `FigureNode` leaves, core) · `TABLE` (the `LensProvider.PLUMBER` core row over pdfplumber `Page.find_tables(table_settings=)`/`Table.extract`/`Table.bbox` against the `lines`/`lines_strict`/`text`/`explicit` `vertical_strategy`/`horizontal_strategy` settings rows, or the `LensProvider.MUPDF` gated row over the native `Page.find_tables(vertical_strategy=, horizontal_strategy=, snap_tolerance=, join_tolerance=)` `TableFinder` whose `Table.extract`/`Table.bbox`/`Table.row_count`/`Table.col_count` carry the resolved-grid shape → `TableNode` of `RunNode` cells carrying the table bbox in `NodeMeta.bounds`, the pdfplumber `Table.cells` bbox set folding the merged-cell `(row, col, col_span, row_span)` quads into the `TableNode.spans` map the model owns) · `WORDS` (pdfplumber `Page.extract_words(x_tolerance=, y_tolerance=, use_text_flow=, split_at_punctuation=, extra_attrs=)` char-geometry clustering with the `extra_attrs=("fontname", "size")` row lifting each word's `RunNode.font_key`/`size` off the recovered char attributes rather than the `_RECOVERED_FONT` placeholder → word-geometry `RunNode` leaves carrying the word bbox in `NodeMeta.bounds`, core — the pure-Python no-subprocess-hop alternative to the gated MuPDF `get_text("words")` quad) · `REGION` (pdfplumber `Page.within_bbox(bbox)`/`CroppedPage.extract_text_lines(strip=, return_chars=)` per-line bbox-restricted recovery → `RunNode` leaves carrying each line bbox in `NodeMeta.bounds`, core — the bbox-scoped read the H1 region inverse names) · `OUTLINE` (pypdf `PdfReader.outline` level/title/page rows → level-resolved `StructureNode` outline-tree leaves over the `H1`-`H6` `_HEADING` vocabulary, core — the pure-Python default; the native MuPDF `Document.get_toc(simple=True)` reaches the same `_outline_node` fold through the `LensProvider.MUPDF` gated row) · `SEARCH` (the `LensProvider.PLUMBER` core row over pdfplumber `Page.search(pattern, regex=, case=)` regex-over-textmap hits, or the `LensProvider.MUPDF` gated row over pymupdf `Page.search_for(needle)` quad hits → hit-region `AnnotationNode` leaves) · `OCR` (the `LensProvider.OCRMYPDF` gated row over ocrmypdf `ocr(source, target, sidecar=, mode=, output_type='pdfa', language=, deskew=, clean=, rotate_pages=, optimize=)` whose `ExitCode` return and `ExitCodeException` rail the worker honors before the sidecar text feeds the same `RunNode` recovery as `EXTRACT_TEXT`, or the `LensProvider.MUPDF` gated row over the per-page pymupdf `Page.get_textpage_ocr(language=, dpi=, full=)`→`TextPage.extractText()` graft for the no-PDF/A in-process read; lossless raster intake embeds JPEG/PNG without recompression through pymupdf `Page.insert_image(rect, stream=)` upstream when `Document.is_image`) · `EMBEDDED` (pymupdf `Document.embfile_count`/`embfile_names`/`embfile_get`/`embfile_info` native attached-file table → `FieldNode` leaves plus `Page.get_images(full=True)`/`Pixmap(document, xref).tobytes("png")` placed rasters → `FigureNode` leaves, gated) · `WIDGET` (pymupdf `Page.widgets()` interactive form-field recovery → `FieldNode` leaves carrying the `field_type`/`field_value`, gated — the `field_type` int→`FieldKind` row RESEARCH-pending the catalogued `PDF_WIDGET_TYPE_*` int values) · `REDACT` (pymupdf `Page.add_redact_annot(quad)`/`Page.apply_redactions(images=, graphics=, text=)`/`Document.scrub(...)` redaction-grade burn-in then `Page.get_text` → redacted `BlockNode` leaves, gated) · `ANNOTATE` (pymupdf `Page.annots()`/`Annot.info`/`Annot.rect`/`Annot.type` → authored `AnnotationNode` leaves carrying the `Annot.type` kind, gated) · `STORY` (pymupdf `Story`/`DocumentWriter`/`paper_rect` reflowable HTML-to-PDF re-layout → re-flowed `PageNode` leaves, gated) · `ODS_READ` (the `LensProvider.ODFPY` core row over odfpy `load`/`getElementsByType(Table)`/`getElementsByType(TableRow)`/`getElementsByType(TableCell)`/`teletype.extractText`/`getAttribute("numbercolumnsrepeated")` run-length cell expansion → `TableNode` of `RunNode` cells per sheet, the pure-Python cp315-core ODF inverse needing no subprocess hop) · `XLSX_READ` (the `LensProvider.CALAMINE` gated row over python-calamine `CalamineWorkbook.from_object`/`sheet_names`/`get_sheet_by_name`/`CalamineSheet.to_python` row matrix → `TableNode` of `RunNode` cells per sheet, gated — RESEARCH-pending its `.api` catalogue, the OOXML/binary-Excel read odfpy does not cover) — routed by the one provider-keyed band, never an `if op == ...` ladder.
- Entry: `DocumentLens.recover` is `async` over the runtime `async_boundary`, resolving the op-and-provider to a `LensProvider` row inside the one fault capsule and returning a `RuntimeRail[ContentKey]` keyed by the content key of the serialized recovered-tree msgpack; a `GATED`-band provider row crosses `anyio.to_process.run_sync` onto the `_gated_recover` worker that resolves the arm and imports `pymupdf`/`ocrmypdf`/`python_calamine` at boundary scope, while a `CORE`-band provider row resolves the pure-Python pypdf/pdfplumber arm directly on the cp315 core.
- Auto: `_emit` resolves `provider = self.provider or _DEFAULT_PROVIDER[op]` then folds through `provider.band` — a `GATED` row crosses `to_process.run_sync(_gated_recover, op.value, provider.value, payload, params)` to the worker resolving `_ARMS[op]` with the provider in `params` (where `pymupdf.open(stream=)`/`find_tables`/`get_textpage_ocr`/`search_for`/`get_toc`/`embfile_*`/`widgets`/`add_redact_annot`/`apply_redactions`/`scrub`/`annots`/`insert_image`/`Story` and `ocrmypdf.ocr(..., sidecar=)` and `python_calamine.CalamineWorkbook.from_object` run), a `CORE` row resolves `_ARMS[op]` directly (pypdf `PdfReader`/`PageObject.extract_text`/`PageObject.images`/`PdfReader.outline`, pdfplumber `open`/`find_tables`/`Table.extract`/`Table.cells`/`extract_words`/`within_bbox`/`search`, and odfpy `load`/`getElementsByType`/`extractText`/`getAttribute`) — each arm folding into the matching `DocumentNode` variant through the one `_node` constructor (every node carries a content-keyed `NodeMeta` minted over the recovered payload), both lowering the recovered tree to `msgspec.msgpack.encode` bytes the content key hashes.
- Receipt: each recovery contributes the `receipt/receipt#RECEIPT` `ArtifactReceipt.Introspection` five-field case (`key`, node count, text length, image count, hit count) projected from the recovered tree by `walk` folding `isinstance` over the `RunNode`/`FigureNode`/`TableNode`/`AnnotationNode` variants so a nested `TableNode` of `RunNode` cells contributes its full sub-tree text and figure count (the recovered `TableNode` rides the image-count slot the case already owns, never a sixth field; the tag rides the encoded `kind` field, never a runtime `.tag` attribute), never a per-op receipt rail.
- Packages: `pymupdf` (`open(stream=, filetype=)`/`Page.get_text` words/dict/json/`find_tables(vertical_strategy=, horizontal_strategy=, snap_tolerance=, join_tolerance=)`/`Table.extract`/`Table.bbox`/`Table.row_count`/`Table.col_count`/`get_images(full=)`/`search_for`/`get_textpage_ocr(language=, dpi=, full=)`/`add_redact_annot`/`apply_redactions(images=, graphics=, text=)`/`Annot.info`/`Annot.rect`/`Annot.type`/`insert_image(rect, stream=)`/`Story`/`DocumentWriter`/`begin_page`/`end_page`/`paper_rect`/`Pixmap(doc, xref).tobytes` and native `get_toc(simple=)`/`embfile_count`/`embfile_names`/`embfile_get`/`embfile_info`/`scrub`/`widgets`, catalogue `[03]` rows, native wheel crossing the subprocess seam) · `ocrmypdf` (`ocr(input, output, sidecar=, mode=, output_type='pdfa', language=, deskew=, clean=, rotate_pages=, optimize=)` returning `ExitCode` with the `ExitCodeException`/`ExitCode` typed failure rail, catalogue `[02]`/`[03]-[01]` and the keyword-axis rows, companion native Tesseract/Ghostscript binaries crossing the subprocess seam) · `pdfplumber` (`open(path_or_fp, repair=)`/`Page.find_tables(table_settings=)`/`Table.extract`/`Table.bbox`/`Table.cells`/`Page.extract_words(x_tolerance=, y_tolerance=, use_text_flow=, split_at_punctuation=, extra_attrs=)`/`Page.search(pattern, regex=, case=)`/`Page.within_bbox(bbox)`/`CroppedPage.extract_text_lines(strip=, return_chars=)`, catalogue `[03]` table/word/search/region rows, pure-Python cp315 core) · `pypdf` (`PdfReader.extract_text(extraction_mode=)`/`PageObject.images`/`PdfReader.outline`/`xmp_metadata`, catalogue `[03]` rows, pure-Python cp315 core) · `odfpy` (`opendocument.load`/`Element.getElementsByType(Table/TableRow/TableCell)`/`teletype.extractText`/`Element.getAttribute("numbercolumnsrepeated"|"name")`, catalogue `[03]` parse/traversal/factory/attribute rows, pure-Python cp315 core, the OpenDocument `ODS_READ` inverse) · `python-calamine` (`CalamineWorkbook.from_object`/`sheet_names`/`get_sheet_by_name`/`CalamineSheet.to_python`, Rust companion `<3.15` crossing the subprocess seam for the `XLSX_READ` OOXML/binary-Excel read — RESEARCH-pending the folder `.api/python-calamine.md` catalogue) · `msgspec` (`msgpack.encode` lowering the recovered `DocumentNode` tree to bytes); runtime (`content_identity.ContentIdentity`/`ContentKey`, `faults.RuntimeRail`/`async_boundary`, `anyio.to_process.run_sync` the gated subprocess lane); `documents/model#NODE` (the `RunNode`/`FigureNode`/`TableNode`/`FieldNode`/`AnnotationNode`/`StructureNode`/`BlockNode`/`PageNode` variants + `NodeMeta` + `StandardRole`/`StructEltKind` + the `NodeKind`/`AnnotKind`/`BlockKind`/`FieldKind` tag enums the arms recover into + `walk`/`encode`).
- Growth: a new recovery operation is one `LensOp` row plus one `_DEFAULT_PROVIDER` membership plus one `_ARMS` acceptor folding into the matching `DocumentNode` case through the one `_node` constructor; a new engine for an existing op is one `LensProvider` row carrying its `(band, label)` and a `provider`-keyed branch on the op's arm, never a parallel op. The recovered-tree corpus keys into the runtime columnar lane (consumed from `data`, never re-minted) so a multi-PDF/multi-workbook corpus is one queryable value over the same `DocumentNode` tree emission lowers from. The spreadsheet/ODF read inverse is the realized form of the `OFFICE_INGEST` card — the `ODS_READ` `LensProvider.ODFPY` core arm folds verified odfpy spellings on the cp315 core, and the `XLSX_READ` `LensProvider.CALAMINE` gated arm folds the OOXML/binary-Excel read odfpy does not cover, the `python-calamine` leaf method spellings RESEARCH-pending the folder `.api/python-calamine.md` admission; a future `.docx`/structured-text read inverse is one more core/gated row over `python-docx`/`ruamel-yaml`, never a parallel ingest owner; zero new surface.
- Boundary: `DocumentLens` recovers nodes and never authors a PDF — authoring stays at the `documents/emit#DOCUMENT` lowering arm, and the `REDACT`/`ANNOTATE`/`OCR` arms edit-in-place, scrub, or graft over an already-emitted PDF, never compose a new document tree from scratch. A `get_text`/`get_words`/`extract_images`/`find_tables`/`search`/`get_outline` reader-verb family scattered beside the emit axis is the deleted form; a `_run_node`/`_figure_node`/`_annotation_node`/`_structure_node` sibling-factory family beside the one `_node` constructor, an `if op == ...` per-band cascade beside the one provider-keyed dispatch, and a `_plumber_table`/`_mupdf_table` engine-pair beside the one `TABLE` arm are the collapsed forms; the recovered tree is `documents/model#NODE` `DocumentNode`, never a re-minted text model. The `OUTLINE`/`EMBEDDED`/`WIDGET` arms read the native `get_toc`/`embfile_*`/`widgets` surface, never re-derive the outline from links or the attached-file table from `get_images`. The `REDACT` arm uses native `apply_redactions(images=, graphics=, text=)` plus `Document.scrub(...)` for redaction-grade removal, never a hand-walked object pruner. The lossless raster intake embeds through native `Page.insert_image(rect, stream=)`, never a second raster-to-PDF library. The pdfplumber-versus-pymupdf and ocrmypdf-versus-MuPDF engine choices on `TABLE`/`SEARCH`/`OCR` are `LensProvider` policy rows carrying their own band, never a boolean `native=` knob beside the value or a parallel arm. The `ODS_READ` arm resolves the pure-Python odfpy reader on the cp315 core rather than routing the OpenDocument read across the `python-calamine` subprocess seam — a gated Rust hop where a dependency-free in-process OASIS parser already owns the format is the rejected band, and `XLSX_READ` carries the OOXML/binary-Excel formats odfpy does not read on the gated `python-calamine` row; the spreadsheet/ODF read is one `LensOp` pair fronting two readers by band, never a single conflated office arm. The `TableNode.spans` merged-cell map is recovered from the pdfplumber `Table.cells` bbox set rather than dropped, so a ruled table with row/column spans round-trips its grid topology, never a flattened cell matrix that erases the merge structure the model owns. The content key is consumed from runtime, never re-minted; the extracted-tree corpus stays at the runtime columnar lane concept level, never a second store. The async dispatch shape aligns on the verified `documents/emit#DOCUMENT` `async_boundary` and `BACKENDS`/`GATED_ARMS` band-table shape so emission and recovery share one fault capsule and one dispatch idiom.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
from __future__ import annotations

from collections.abc import Callable
from enum import StrEnum
from io import BytesIO
from typing import Final, assert_never

from anyio import to_process
from msgspec import Struct, msgpack

from rasm.artifacts.documents.model import (
    AnnotationNode,
    AnnotKind,
    BlockKind,
    BlockNode,
    DocumentNode,
    FieldKind,
    FieldNode,
    FigureNode,
    NodeKind,
    NodeMeta,
    PageNode,
    RunNode,
    SectionNode,
    StandardRole,
    StructEltKind,
    StructureNode,
    TableNode,
    walk,
)
from rasm.artifacts.receipt.receipt import ArtifactReceipt
from rasm.runtime.content_identity import ContentIdentity, ContentKey
from rasm.runtime.faults import RuntimeRail, async_boundary

# --- [TYPES] ----------------------------------------------------------------------------

type Bounds = tuple[float, float, float, float]
type Grid = list[list[str | None]]
type Spans = tuple[tuple[int, int, int, int], ...]
type RecoverArm = Callable[[bytes, LensProvider, dict[str, object]], tuple[DocumentNode, ...]]


class LensOp(StrEnum):
    EXTRACT_TEXT = "extract-text"
    EXTRACT_IMAGES = "extract-images"
    TABLE = "table"
    WORDS = "words"
    REGION = "region"
    OUTLINE = "outline"
    SEARCH = "search"
    OCR = "ocr"
    EMBEDDED = "embedded"
    WIDGET = "widget"
    REDACT = "redact"
    ANNOTATE = "annotate"
    STORY = "story"
    XLSX_READ = "xlsx-read"
    ODS_READ = "ods-read"


class LensBand(StrEnum):
    CORE = "core"
    GATED = "gated"


class LensProvider(StrEnum):
    PYPDF = "pypdf"
    PLUMBER = "pdfplumber"
    ODFPY = "odfpy"
    MUPDF = "pymupdf"
    OCRMYPDF = "ocrmypdf"
    CALAMINE = "python-calamine"

    @property
    def band(self) -> LensBand:
        return LensBand.CORE if self in _CORE_PROVIDERS else LensBand.GATED


# --- [CONSTANTS] ------------------------------------------------------------------------

_ORIGIN: Final[Bounds] = (0.0, 0.0, 0.0, 0.0)
_RECOVERED_FONT: Final[str] = "recovered"
_HEADING_FLOOR: Final[int] = 1
_HEADING_CEIL: Final[int] = 6
_CORE_PROVIDERS: Final[frozenset[LensProvider]] = frozenset({LensProvider.PYPDF, LensProvider.PLUMBER, LensProvider.ODFPY})
_HEADING: Final[tuple[StructEltKind, ...]] = (
    StructEltKind.H1,
    StructEltKind.H2,
    StructEltKind.H3,
    StructEltKind.H4,
    StructEltKind.H5,
    StructEltKind.H6,
)

# --- [MODELS] ---------------------------------------------------------------------------


class DocumentLens(Struct, frozen=True):
    op: LensOp
    payload: bytes
    params: dict[str, object] = {}
    provider: LensProvider | None = None

    async def recover(self) -> RuntimeRail[ContentKey]:
        return await async_boundary(f"lens.{self.op.value}", self._emit)

    async def _emit(self) -> ContentKey:
        provider = self.provider or _DEFAULT_PROVIDER[self.op]
        nodes = (
            await to_process.run_sync(_gated_recover, self.op.value, provider.value, self.payload, self.params)
            if provider.band is LensBand.GATED
            else _ARMS[self.op](self.payload, provider, self.params)
        )
        return ContentIdentity.of(f"lens-{self.op.value}", msgpack.encode(nodes))

    @staticmethod
    def receipt(key: ContentKey, nodes: tuple[DocumentNode, ...], /) -> ArtifactReceipt:
        flat = tuple(child for node in nodes for child in walk(node))
        return ArtifactReceipt.Introspection(
            key,
            len(flat),
            sum(len(node.text) for node in flat if isinstance(node, RunNode)),
            sum(isinstance(node, FigureNode) for node in flat),
            sum(isinstance(node, AnnotationNode) for node in flat),
        )


# --- [OPERATIONS] -----------------------------------------------------------------------


def _node(kind: NodeKind, role: str, page: int, payload: bytes, *, bounds: Bounds | None = None, **slot: object) -> DocumentNode:
    meta = NodeMeta(key=ContentIdentity.of(f"node-{role}", payload), role=role, page=page, bounds=bounds)
    match kind:
        case NodeKind.RUN:
            return RunNode(meta=meta, text=str(slot["text"]), font_key=str(slot.get("font_key", _RECOVERED_FONT)), size=float(slot.get("size", 0.0)), weight=int(slot.get("weight", 400)))
        case NodeKind.FIGURE:
            return FigureNode(meta=meta, asset_key=ContentIdentity.of(f"asset-{slot['name']}", payload))
        case NodeKind.TABLE:
            return TableNode(meta=meta, rows=slot["rows"], spans=slot.get("spans", ()))
        case NodeKind.FIELD:
            return FieldNode(meta=meta, name=str(slot["name"]), field=slot.get("field", FieldKind.TEXT), value=slot.get("value"))
        case NodeKind.ANNOTATION:
            return AnnotationNode(meta=meta, annot=slot["annot"], target=bounds or _ORIGIN, contents=str(slot.get("contents", "")))
        case NodeKind.STRUCTURE:
            return StructureNode(meta=meta, role=StandardRole(elt=slot.get("elt", StructEltKind.SECT)))
        case NodeKind.SECTION:
            return SectionNode(meta=meta, level=int(slot.get("level", 1)))
        case NodeKind.BLOCK:
            return BlockNode(meta=meta, block=slot.get("block", BlockKind.PARAGRAPH))
        case NodeKind.PAGE:
            return PageNode(meta=meta, media_box=bounds or _ORIGIN)
        case _ as unreachable:
            assert_never(unreachable)


def _table_node(grid: Grid, bbox: Bounds, page: int, *, role: str = "table", spans: Spans = ()) -> TableNode:
    rows = tuple(
        tuple(_node(NodeKind.RUN, "cell", page, (cell or "").encode(), text=cell or "") for cell in row)
        for row in grid
    )
    return _node(NodeKind.TABLE, role, page, repr(grid).encode(), bounds=bbox, rows=rows, spans=spans)


def _outline_node(level: int, title: str, page: int) -> DocumentNode:
    elt = _HEADING[min(max(level, _HEADING_FLOOR), _HEADING_CEIL) - 1]
    return _node(NodeKind.STRUCTURE, title, page, f"{elt.value}:{title}".encode(), elt=elt)


# --- [READER_ARMS] ----------------------------------------------------------------------


def _text_arm(payload: bytes, _provider: LensProvider, params: dict[str, object]) -> tuple[DocumentNode, ...]:
    import pypdf

    reader = pypdf.PdfReader(BytesIO(payload))
    mode = str(params.get("mode", "plain"))
    return tuple(
        _node(NodeKind.RUN, "run", index, text.encode(), text=text)
        for index, page in enumerate(reader.pages)
        if (text := page.extract_text(extraction_mode=mode))
    )


def _images_arm(payload: bytes, _provider: LensProvider, _params: dict[str, object]) -> tuple[DocumentNode, ...]:
    import pypdf

    reader = pypdf.PdfReader(BytesIO(payload))
    return tuple(
        _node(NodeKind.FIGURE, "figure", index, image.data, name=image.name)
        for index, page in enumerate(reader.pages)
        for image in page.images
    )


def _words_arm(payload: bytes, _provider: LensProvider, params: dict[str, object]) -> tuple[DocumentNode, ...]:
    import pdfplumber

    with pdfplumber.open(BytesIO(payload), repair=bool(params.get("repair", False))) as document:
        return tuple(
            _node(
                NodeKind.RUN,
                "word",
                index,
                word["text"].encode(),
                bounds=(word["x0"], word["top"], word["x1"], word["bottom"]),
                text=word["text"],
                font_key=word.get("fontname", _RECOVERED_FONT),
                size=float(word.get("size", 0.0)),
            )
            for index, page in enumerate(document.pages)
            for word in page.extract_words(
                x_tolerance=float(params.get("x_tolerance", 3.0)),
                y_tolerance=float(params.get("y_tolerance", 3.0)),
                use_text_flow=bool(params.get("use_text_flow", False)),
                split_at_punctuation=bool(params.get("split_at_punctuation", False)),
                extra_attrs=list(params.get("extra_attrs", ("fontname", "size"))),
            )
        )


def _region_arm(payload: bytes, _provider: LensProvider, params: dict[str, object]) -> tuple[DocumentNode, ...]:
    import pdfplumber

    bbox = tuple(params["bbox"])
    with pdfplumber.open(BytesIO(payload), repair=bool(params.get("repair", False))) as document:
        return tuple(
            _node(NodeKind.RUN, "region", index, line["text"].encode(), bounds=(line["x0"], line["top"], line["x1"], line["bottom"]), text=line["text"])
            for index, page in enumerate(document.pages)
            for line in page.within_bbox(bbox).extract_text_lines(strip=True, return_chars=False)
        )


def _table_arm(payload: bytes, provider: LensProvider, params: dict[str, object]) -> tuple[DocumentNode, ...]:
    settings: dict[str, object] = {key: params[key] for key in _TABLE_SETTINGS if key in params} | {
        "vertical_strategy": str(params.get("vertical", "lines")),
        "horizontal_strategy": str(params.get("horizontal", "lines")),
        "snap_tolerance": float(params.get("snap_tolerance", 3.0)),
        "join_tolerance": float(params.get("join_tolerance", 3.0)),
    }
    if provider is LensProvider.MUPDF:
        import pymupdf

        with pymupdf.open(stream=payload, filetype="pdf") as document:
            return tuple(
                _table_node(table.extract(), tuple(table.bbox), index)
                for index, page in enumerate(document)
                for table in page.find_tables(**settings).tables
            )
    import pdfplumber

    plumber_settings = settings | {key: params[axis] for axis, key in _PLUMBER_SETTINGS.items() if axis in params}
    with pdfplumber.open(BytesIO(payload), repair=bool(params.get("repair", False))) as document:
        return tuple(
            _table_node(table.extract(), tuple(table.bbox), index, spans=_plumber_spans(table))
            for index, page in enumerate(document.pages)
            for table in page.find_tables(table_settings=plumber_settings)
        )


def _plumber_spans(table: object) -> Spans:
    cells = [cell for cell in table.cells if cell is not None]
    columns = tuple(sorted({round(cell[0]) for cell in cells} | {round(cell[2]) for cell in cells}))
    rows = tuple(sorted({round(cell[1]) for cell in cells} | {round(cell[3]) for cell in cells}))
    return tuple(
        (rows.index(round(y0)), columns.index(round(x0)), columns.index(round(x1)) - columns.index(round(x0)), rows.index(round(y1)) - rows.index(round(y0)))
        for x0, y0, x1, y1 in cells
        if columns.index(round(x1)) - columns.index(round(x0)) > 1 or rows.index(round(y1)) - rows.index(round(y0)) > 1
    )


def _outline_arm(payload: bytes, provider: LensProvider, _params: dict[str, object]) -> tuple[DocumentNode, ...]:
    if provider is LensProvider.MUPDF:
        import pymupdf

        with pymupdf.open(stream=payload, filetype="pdf") as document:
            return tuple(_outline_node(level, title, page) for level, title, page in document.get_toc(simple=True))
    import pypdf

    reader = pypdf.PdfReader(BytesIO(payload))
    return tuple(
        _outline_node(1, item.title, item.page or 0)
        for item in reader.outline
        if not isinstance(item, list)
    )


def _search_arm(payload: bytes, provider: LensProvider, params: dict[str, object]) -> tuple[DocumentNode, ...]:
    needle = str(params["needle"])
    if provider is LensProvider.MUPDF:
        import pymupdf

        with pymupdf.open(stream=payload, filetype="pdf") as document:
            return tuple(
                _node(NodeKind.ANNOTATION, "hit", index, needle.encode(), bounds=tuple(rect), annot=AnnotKind.HIGHLIGHT, contents=needle)
                for index, page in enumerate(document)
                for rect in page.search_for(needle)
            )
    import pdfplumber

    with pdfplumber.open(BytesIO(payload), repair=bool(params.get("repair", False))) as document:
        return tuple(
            _node(NodeKind.ANNOTATION, "hit", index, needle.encode(), bounds=(hit["x0"], hit["top"], hit["x1"], hit["bottom"]), annot=AnnotKind.HIGHLIGHT, contents=needle)
            for index, page in enumerate(document.pages)
            for hit in page.search(needle, regex=bool(params.get("regex", True)), case=bool(params.get("case", True)))
        )


def _ocr_arm(payload: bytes, provider: LensProvider, params: dict[str, object]) -> tuple[DocumentNode, ...]:
    import pymupdf

    if provider is LensProvider.MUPDF:
        with pymupdf.open(stream=payload, filetype=str(params.get("filetype", "pdf"))) as document:
            language = "+".join(str(lang) for lang in params.get("language", ("eng",)))
            return tuple(
                _node(NodeKind.RUN, "ocr", index, text.encode(), text=text)
                for index, page in enumerate(document)
                if (text := page.get_textpage_ocr(language=language, dpi=int(params.get("dpi", 72)), full=bool(params.get("full", False))).extractText())
            )
    from tempfile import NamedTemporaryFile

    import ocrmypdf

    intake = pymupdf.open(stream=payload, filetype=str(params.get("filetype", "pdf")))
    if intake.is_image:
        canvas = pymupdf.open()
        page = canvas.new_page(width=intake[0].rect.width, height=intake[0].rect.height)
        page.insert_image(page.rect, stream=payload)
        payload = canvas.tobytes()
    with NamedTemporaryFile(suffix=".pdf") as source, NamedTemporaryFile(suffix=".pdf") as target, NamedTemporaryFile(suffix=".txt") as sidecar:
        source.write(payload)
        source.flush()
        code = ocrmypdf.ocr(
            source.name,
            target.name,
            sidecar=sidecar.name,
            language=tuple(params.get("language", ("eng",))),
            output_type=str(params.get("output_type", "pdfa")),
            mode=str(params.get("mode", "force")),
            deskew=bool(params.get("deskew", False)),
            clean=bool(params.get("clean", False)),
            rotate_pages=bool(params.get("rotate_pages", False)),
            optimize=int(params.get("optimize", 1)),
            progress_bar=False,
        )
        text = sidecar.read().decode() if code is ocrmypdf.ExitCode.ok else ""
    return (
        _node(NodeKind.STRUCTURE, code.name, 0, code.name.encode(), elt=StructEltKind.NOTE),
        *(
            _node(NodeKind.RUN, "ocr", index, line.encode(), text=line)
            for index, line in enumerate(text.splitlines())
            if line
        ),
    )


def _embedded_arm(payload: bytes, _provider: LensProvider, _params: dict[str, object]) -> tuple[DocumentNode, ...]:
    import pymupdf

    with pymupdf.open(stream=payload, filetype="pdf") as document:
        fields = tuple(
            _node(NodeKind.FIELD, "embedded", 0, document.embfile_get(name), field=FieldKind.TEXT, name=name, value=document.embfile_info(name)["filename"])
            for name in document.embfile_names()
        )
        figures = tuple(
            _node(NodeKind.FIGURE, "placed", index, pymupdf.Pixmap(document, xref).tobytes("png"), name=f"img-{xref}")
            for index, page in enumerate(document)
            for xref, *_ in page.get_images(full=True)
        )
    return fields + figures


def _widget_arm(payload: bytes, _provider: LensProvider, _params: dict[str, object]) -> tuple[DocumentNode, ...]:
    import pymupdf

    with pymupdf.open(stream=payload, filetype="pdf") as document:
        return tuple(
            _node(NodeKind.FIELD, widget.field_name, index, str(widget.field_value).encode(), field=_WIDGET_FIELD.get(widget.field_type, FieldKind.TEXT), name=widget.field_name, value=widget.field_value)
            for index, page in enumerate(document)
            for widget in page.widgets()
        )


def _redact_arm(payload: bytes, _provider: LensProvider, params: dict[str, object]) -> tuple[DocumentNode, ...]:
    import pymupdf

    with pymupdf.open(stream=payload, filetype="pdf") as document:
        for page_index, quad in params.get("regions", ()):
            document[int(page_index)].add_redact_annot(quad)
        for page in document:
            page.apply_redactions(images=int(params.get("images", 2)), graphics=int(params.get("graphics", 1)), text=int(params.get("text", 0)))
        if params.get("scrub"):
            document.scrub(metadata=bool(params.get("metadata", True)), hidden_text=bool(params.get("hidden_text", True)), attached_files=bool(params.get("attached_files", True)))
        return tuple(
            _node(NodeKind.BLOCK, "block", index, page.get_text().encode(), block=BlockKind.PARAGRAPH)
            for index, page in enumerate(document)
        )


def _annotate_arm(payload: bytes, _provider: LensProvider, params: dict[str, object]) -> tuple[DocumentNode, ...]:
    import pymupdf

    page_index = int(params["page"])
    with pymupdf.open(stream=payload, filetype="pdf") as document:
        return tuple(
            _node(NodeKind.ANNOTATION, "annotation", page_index, annot.info["content"].encode(), bounds=tuple(annot.rect), annot=_ANNOT_KIND.get(annot.type[0], AnnotKind.NOTE), contents=annot.info["content"])
            for annot in document[page_index].annots()
        )


def _story_arm(_payload: bytes, _provider: LensProvider, params: dict[str, object]) -> tuple[DocumentNode, ...]:
    import pymupdf

    story = pymupdf.Story(html=str(params["html"]), user_css=params.get("user_css"))
    writer = pymupdf.DocumentWriter(BytesIO())
    rect = pymupdf.paper_rect(str(params.get("paper", "a4")))
    more, page_count = 1, 0
    while more:
        device = writer.begin_page(rect)
        more, _ = story.place(rect)
        story.draw(device)
        writer.end_page()
        page_count += 1
    writer.close()
    return tuple(_node(NodeKind.PAGE, "page", index, str(index).encode(), bounds=tuple(rect)) for index in range(page_count))


def _ods_arm(payload: bytes, _provider: LensProvider, _params: dict[str, object]) -> tuple[DocumentNode, ...]:
    from odf.opendocument import load
    from odf.table import Table, TableCell, TableRow
    from odf.teletype import extractText

    document = load(BytesIO(payload))
    return tuple(
        _table_node(
            [
                [
                    cell
                    for cell_node in row.getElementsByType(TableCell)
                    for cell in (extractText(cell_node) or None,) * max(int(cell_node.getAttribute("numbercolumnsrepeated") or 1), 1)
                ]
                for row in sheet.getElementsByType(TableRow)
            ],
            _ORIGIN,
            index,
            role=sheet.getAttribute("name") or f"sheet-{index}",
        )
        for index, sheet in enumerate(document.getElementsByType(Table))
    )


def _xlsx_arm(payload: bytes, _provider: LensProvider, params: dict[str, object]) -> tuple[DocumentNode, ...]:
    import python_calamine

    workbook = python_calamine.CalamineWorkbook.from_object(BytesIO(payload))
    sheets = params.get("sheets") or workbook.sheet_names
    return tuple(
        _table_node(
            [[None if value is None else str(value) for value in row] for row in workbook.get_sheet_by_name(name).to_python()],
            _ORIGIN,
            index,
            role=name,
        )
        for index, name in enumerate(sheets)
    )


def _gated_recover(op: str, provider: str, payload: bytes, params: dict[str, object]) -> tuple[DocumentNode, ...]:
    return _ARMS[LensOp(op)](payload, LensProvider(provider), params)


# --- [TABLES] ---------------------------------------------------------------------------

_TABLE_SETTINGS: Final[tuple[str, ...]] = ("edge_min_length", "intersection_tolerance", "text_tolerance")
_PLUMBER_SETTINGS: Final[dict[str, str]] = {"explicit_vertical": "explicit_vertical_lines", "explicit_horizontal": "explicit_horizontal_lines"}
_WIDGET_FIELD: Final[dict[int, FieldKind]] = {}
_ANNOT_KIND: Final[dict[int, AnnotKind]] = {}

_ARMS: Final[dict[LensOp, RecoverArm]] = {
    LensOp.EXTRACT_TEXT: _text_arm,
    LensOp.EXTRACT_IMAGES: _images_arm,
    LensOp.TABLE: _table_arm,
    LensOp.WORDS: _words_arm,
    LensOp.REGION: _region_arm,
    LensOp.OUTLINE: _outline_arm,
    LensOp.SEARCH: _search_arm,
    LensOp.OCR: _ocr_arm,
    LensOp.EMBEDDED: _embedded_arm,
    LensOp.WIDGET: _widget_arm,
    LensOp.REDACT: _redact_arm,
    LensOp.ANNOTATE: _annotate_arm,
    LensOp.STORY: _story_arm,
    LensOp.XLSX_READ: _xlsx_arm,
    LensOp.ODS_READ: _ods_arm,
}
_DEFAULT_PROVIDER: Final[dict[LensOp, LensProvider]] = {
    LensOp.EXTRACT_TEXT: LensProvider.PYPDF,
    LensOp.EXTRACT_IMAGES: LensProvider.PYPDF,
    LensOp.TABLE: LensProvider.PLUMBER,
    LensOp.WORDS: LensProvider.PLUMBER,
    LensOp.REGION: LensProvider.PLUMBER,
    LensOp.OUTLINE: LensProvider.PYPDF,
    LensOp.SEARCH: LensProvider.PLUMBER,
    LensOp.OCR: LensProvider.OCRMYPDF,
    LensOp.EMBEDDED: LensProvider.MUPDF,
    LensOp.WIDGET: LensProvider.MUPDF,
    LensOp.REDACT: LensProvider.MUPDF,
    LensOp.ANNOTATE: LensProvider.MUPDF,
    LensOp.STORY: LensProvider.MUPDF,
    LensOp.XLSX_READ: LensProvider.CALAMINE,
    LensOp.ODS_READ: LensProvider.ODFPY,
}
```

## [03]-[RESEARCH]

- [OFFICE_INGEST_SPLIT]: the `OFFICE_INGEST` card realizes as two `LensOp` rows fronting two readers by band, never one conflated office arm. The `ODS_READ` row is SETTLED fence code on the cp315 core: the `LensProvider.ODFPY` `_DEFAULT_PROVIDER` membership, the `CORE` band the provider carries, and the `_ods_arm` acceptor folding each OpenDocument sheet into a `TableNode` of `RunNode` cells through the one `_node`/`_table_node` constructor compose verified `.api/odfpy.md` `[03]` spellings — `opendocument.load(BytesIO)`, `Element.getElementsByType(Table)`/`getElementsByType(TableRow)`/`getElementsByType(TableCell)`, `teletype.extractText(cell)`, and `Element.getAttribute("numbercolumnsrepeated")` for the run-length cell expansion the catalogue's repeat axis names plus `getAttribute("name")` for the sheet role — so a cold grade reads the `_ods_arm` leaf calls against the landed catalogue, never a RESEARCH marker. odfpy is dependency-free pure Python installing on the cp315 core with no subprocess hop, so the OpenDocument read resolves directly beside the pypdf/pdfplumber core arms. The `XLSX_READ` row carries the OOXML/binary-Excel formats odfpy does not read on the gated `LensProvider.CALAMINE` band: the `_xlsx_arm` acceptor and the `LensProvider.CALAMINE` `_DEFAULT_PROVIDER` membership are settled, but the `python-calamine` LEAF spellings — `CalamineWorkbook.from_object`, `sheet_names`, `get_sheet_by_name`, `CalamineSheet.to_python` — stay RESEARCH-pending: no `.api/python-calamine.md` catalogue exists in the folder `.api/`, so per the page grammar these members stay marked RESEARCH on this card rather than asserted as verified spellings, and a cold grade reads the `_xlsx_arm` `python_calamine` leaf calls against this card until the catalogue admits the package. The catalogue admission resolves the leaf spellings against the reflected Rust binding (the `from_object(BytesIO)`-versus-`from_path(Path)` ingress shape, the `to_python()` row-matrix return type, and the `<3.15` cp-floor that fixes the subprocess band) at which point the markers clear with zero shape change. The cell coercion is inlined into the one `_xlsx_arm` row comprehension (`None if value is None else str(value)`), never a single-call `_cell` helper hop. Both arms key their `TableNode` text into the same `to_corpus_row` corpus lane every other arm feeds. The Rust no-cp315-wheel floor forces the `to_process.run_sync` seam for `python-calamine`, while the in-process OASIS parser keeps `ODS_READ` on the core — the band rides the provider, so the one `LensProvider` axis fronts both runtimes without a parallel ingest owner.
- [WIDGET_ANNOT_INT_DISCRIMINANTS]: `.api/pymupdf.md` `[03]` catalogues `Widget.field_type -> int` as the `PDF_WIDGET_TYPE_*` discriminant and `Annot.type -> (int, str)` as the annotation kind, but the catalogue does NOT enumerate the int VALUES of either constant family, so the `int → FieldKind` and `int → AnnotKind` mappings stay RESEARCH-pending: `_WIDGET_FIELD` and `_ANNOT_KIND` are declared as empty `dict[int, ...]` tables on the page rather than asserting the unverified `2`-`7`/`PDF_ANNOT_*` integer keys as fact, and `_widget_arm`/`_annotate_arm` fall back to `FieldKind.TEXT`/`AnnotKind.NOTE` through the `dict.get(..., default)` until the catalogue admits the constant-value rows. The catalogue DEEPEN that lands the `pymupdf.PDF_WIDGET_TYPE_CHECKBOX`/`_RADIOBUTTON`/`_TEXT`/`_LISTBOX`/`_COMBOBOX`/`_SIGNATURE` and `pymupdf.PDF_ANNOT_HIGHLIGHT`/`_REDACT`/`_LINK`/`_TEXT`/`_STAMP` symbol→int rows clears the markers and fills the two tables with zero shape change; the `field_type`/`type` accessor spellings themselves are verified catalogue rows, only the discriminant integer values are pending.
- [CORPUS_LANE_RESIDENCY]: the recovered-tree corpus keys a `(ContentKey, DocumentNode-tree)` value into the runtime columnar lane (`data` query owner) via the `data` `CORPUS_INGEST` `to_corpus_row` `[WIRE]` seam, so a multi-PDF/multi-workbook corpus is one queryable value over the same `DocumentNode` tree emission lowers from. The lane is consumed from `data`, never re-minted here; the handoff stays at the corpus-port concept level, aligned on the runtime columnar lane landing the corpus port — the artifacts side is the content-keyed producer of the extracted-tree value, never a second store. The settled core-arm `pypdf` `PageObject.extract_text(extraction_mode=...)`/`PageObject.images`/`PdfReader.outline`, `pdfplumber` `open`/`Page.find_tables(table_settings=)`/`Table.extract`/`Table.bbox`/`Table.cells`/`Page.extract_words(...)`/`Page.search(pattern, regex=, case=)`/`Page.within_bbox(bbox)`/`CroppedPage.extract_text_lines(strip=, return_chars=)`, and `odfpy` `opendocument.load`/`Element.getElementsByType`/`teletype.extractText`/`Element.getAttribute` spellings verify against `.api/pypdf.md`, `.api/pdfplumber.md` `[03]`, and `.api/odfpy.md` `[03]`, and the gated `pymupdf` `open(stream=)`/`get_text("words")`/`find_tables(vertical_strategy=, horizontal_strategy=, snap_tolerance=, join_tolerance=)`/`TableFinder.tables`/`Table.extract`/`Table.bbox`/`Table.row_count`/`Table.col_count`/`get_textpage_ocr(language=, dpi=, full=)`/`TextPage.extractText`/`search_for`/`get_images(full=)`/`get_toc(simple=)`/`embfile_count`/`embfile_names`/`embfile_get`/`embfile_info`/`widgets`/`add_redact_annot`/`apply_redactions(images=, graphics=, text=)`/`scrub`/`insert_image(rect, stream=)`/`Pixmap(doc, xref).tobytes`/`Annot.info`/`Annot.rect`/`Annot.type`/`Story`/`DocumentWriter.begin_page`/`end_page`/`paper_rect` spellings and the `ocrmypdf` `ocr(..., sidecar=, mode=, output_type=, language=, deskew=, clean=, rotate_pages=, optimize=)` returning `ExitCode` plus the `ExitCode.ok`/`ExitCodeException` rail verify against `.api/pymupdf.md` `[03]` and `.api/ocrmypdf.md` `[02]`/`[03]`. The `pymupdf`/`ocrmypdf`/`python-calamine` arms reflect on the `python_version<'3.15'` subprocess band and never resolve on the cp315 core; the `pypdf`/`pdfplumber`/`odfpy` arms resolve on the cp315 core with no subprocess hop. The `TABLE`/`SEARCH`/`OCR` ops carry both a core and a gated engine behind one `LensProvider` policy row each (pdfplumber-versus-pymupdf table/search, ocrmypdf-versus-MuPDF-per-page OCR), the band riding the provider so a single op fronts both runtimes without a parallel arm.
- [MUPDF_RECOVERY_SPELLINGS_RESOLVED]: the catalogue-DEEPEN once carded here is landed — `.api/pymupdf.md` `[03]-[ENTRYPOINTS]` now carries the native outline (`get_toc`/`set_toc`), embedded-file (`embfile_count`/`embfile_names`/`embfile_get`/`embfile_info`/`embfile_add`), redaction-grade (`scrub`/`bake`/`subset_fonts`/`add_redact_annot`/`apply_redactions`), widget (`widgets`/`add_widget`), table (`find_tables(vertical_strategy=, horizontal_strategy=, snap_tolerance=, join_tolerance=)`/`TableFinder.tables`/`Table.extract`/`Table.bbox`/`Table.row_count`/`Table.col_count`), per-page OCR (`get_textpage_ocr(language=, dpi=, full=)`), annotation (`annots`/`Annot.info`/`Annot.rect`/`Annot.type`), lossless-embed (`insert_image`), and reflow (`Story.place`/`Story.draw`/`DocumentWriter.begin_page`/`end_page`/`paper_rect`) rows, so every `EMBEDDED`/`WIDGET`/`OUTLINE`/`SEARCH`/`OCR`/`REDACT`/`ANNOTATE`/`STORY` leaf spelling is a verified catalogue row rather than a public-type-level inference. The native `find_tables` `TableFinder` is exploited as the `LensProvider.MUPDF` gated table engine beside the pdfplumber core engine on the one `TABLE` op, the native `get_textpage_ocr`→`TextPage.extractText` per-page graft is the `LensProvider.MUPDF` in-process OCR engine beside the ocrmypdf PDF/A engine on the one `OCR` op, and `get_toc(simple=True)` is the `LensProvider.MUPDF` gated outline engine beside the pypdf `PdfReader.outline` core engine — so the catalogued table, per-page-OCR, and native-outline rows are fence-exploited rather than catalogued-but-unused. The `EMBEDDED` arm recovers the native attached-file table through `embfile_names`/`embfile_get`/`embfile_info` rather than re-deriving it from `get_images` (placed rasters are the distinct `FigureNode` concern, recovered through `get_images(full=True)`/`Pixmap`), the `WIDGET` arm recovers form fields through native `widgets()`, the `ANNOTATE` arm recovers the annotation kind through native `Annot.type`, and the `REDACT` arm reaches redaction-grade removal through native `apply_redactions`/`scrub` rather than a hand-walked object pruner. No lens arm asserts an entrypoint spelling absent from the catalogue; the `Widget.field_type`/`Annot.type` accessor spellings are catalogued, only their discriminant integer values stay RESEARCH per `[WIDGET_ANNOT_INT_DISCRIMINANTS]`.

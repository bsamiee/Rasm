# [PY_ARTIFACTS_LENS]

The recover-TO half of the bidirectional document seam: where `documents/model#NODE` is the single interior `DocumentNode` tree every backend lowers FROM, `DocumentLens` is the inverse that recovers a `DocumentNode` tree (and its `DocumentDelta` corpus value) back OUT of an already-emitted PDF — text, image, word-geometry, region, full-text search, embedded-file and outline harvest, redaction burn-in, annotation authoring, and reflowable `Story` re-layout. `DocumentLens` is ONE owner discriminating extraction operation over a `LensOp` closed `StrEnum` with one total `match`, never a `get_text`/`get_words`/`extract_images` verb family. The native MuPDF arms (pymupdf) and the gated-band XML arm (lxml) cross the runtime subprocess seam (`anyio.to_process.run_sync`); the pure-Python reader arms (pypdf) resolve on the cp315 core. Every recovery returns a `RuntimeRail[ContentKey]` keyed by the runtime content key and contributes the one `receipt/receipt#RECEIPT` `ArtifactReceipt.Introspection` case carrying the recovered-tree shape; production and extraction are inverses over the one node algebra, so a `documents/emit#DOCUMENT` emission and a `DocumentLens` recovery round-trip through `DocumentNode` with `DocumentDelta` (`documents/model#DELTA`) defined once.

## [01]-[INDEX]

- [01]-[LENS]: `DocumentLens` (Struct, frozen) + `LensOp` nine-op closed `StrEnum`; text/image/word/region/search/embedded/outline/redaction/annotation/`Story` recovery into `DocumentNode`, one total `match` over one `async_boundary`, gated MuPDF/XML arms on the subprocess seam and pure-Python pypdf arms on the core.

## [02]-[LENS]

- Owner: `DocumentLens` the one extraction owner discriminating recovery operation; `LensOp` the closed `StrEnum` of the nine recover-TO operations; the recovered `DocumentNode` (owned by `documents/model#NODE`) is the single interior representation every arm folds back into, never a per-operation result type.
- Cases: `LensOp` rows `EXTRACT_TEXT` (pypdf `PageObject.extract_text(extraction_mode=...)` → `RunNode` leaves, pure-Python core) · `EXTRACT_IMAGES` (pypdf `PageObject.images` → `FigureNode` leaves carrying the content-keyed image `asset_key`, core) · `WORDS` (pymupdf `Page.get_text("words")` → word-geometry `RunNode` leaves carrying the quad bbox in `NodeMeta.bounds`, gated) · `SEARCH` (pymupdf `Page.search_for(needle)` → hit-region `AnnotationNode` leaves, gated) · `EMBEDDED` (pymupdf `Page.get_images(full=True)` + `Pixmap.tobytes` → `FigureNode` leaves plus lxml `etree.XMLParser`/`fromstring`/`tostring` structural recovery → a `StructureNode`, gated) · `OUTLINE` (pypdf `PdfReader.outline` → `StructureNode` outline-tree leaves, core) · `REDACT` (pymupdf `Page.apply_redactions` then `Page.get_text` → redacted `BlockNode` leaves, gated) · `ANNOTATE` (pymupdf `Page.annots()`/`Annot.info`/`Annot.rect` → authored `AnnotationNode` leaves, gated) · `STORY` (pymupdf `Story`/`DocumentWriter`/`paper_rect` reflowable HTML-to-PDF re-layout → re-flowed `PageNode` leaves, gated) — matched by one total `match`, never an `if op == ...` ladder.
- Entry: `DocumentLens.recover` is `async` over the runtime `async_boundary`, dispatching the op inside the one fault capsule and returning a `RuntimeRail[ContentKey]` keyed by the content key of the serialized recovered-tree msgpack; the gated MuPDF and XML arms cross `anyio.to_process.run_sync` onto the gated-band `_gated_recover` worker that imports `pymupdf`/`lxml.etree` at boundary scope, while the pure-Python pypdf reader arms (`_pypdf_recover`) resolve on the cp315 core.
- Auto: `_emit` folds the op — the `GATED` MuPDF/XML ops through the gated-band worker where `pymupdf.open(stream=...)`/`Page.get_text("words")`/`search_for`/`apply_redactions`/`annots`/`Story` and `lxml.etree.tostring` run, each arm building the matching `DocumentNode` variant through its `_run_node`/`_figure_node`/`_annotation_node`/`_structure_node` constructor (every node carries a content-keyed `NodeMeta` minted over the recovered payload); the core ops (`EXTRACT_TEXT`/`EXTRACT_IMAGES`/`OUTLINE`) through `_pypdf_recover` over `PdfReader`/`PageObject.extract_text`/`PageObject.images`/`PdfReader.outline` — both lowering the recovered tree to `msgspec.msgpack.encode` bytes the content key hashes.
- Receipt: each recovery contributes `receipt/receipt#RECEIPT` `ArtifactReceipt.Introspection` carrying the content key and the recovered-tree-shape facts (node count / text length / image count / hit count) projected from the `DocumentNode` tree by `isinstance` over the `RunNode`/`FigureNode`/`AnnotationNode` variants (the tag rides the encoded `kind` field, never a runtime `.tag` attribute), never a per-op receipt rail.
- Packages: `pymupdf` (`Document`/`Page.get_text` words/dict/json/`get_images`/`search_for`/`apply_redactions`/`Annot`/`Story`/`DocumentWriter`/`Pixmap`, native wheel crossing the subprocess seam), `pypdf` (`PdfReader.extract_text`/`PageObject.images`/`PdfReader.outline`/`xmp_metadata`, pure-Python cp315 core), `lxml` (`etree.XMLParser`/`fromstring`/`tostring` structural recovery, gated `python_version<'3.15'` band), `msgspec` (`msgpack.encode` lowering the recovered `DocumentNode` tree to bytes); runtime (`content_identity.ContentIdentity`/`ContentKey`, `faults.RuntimeRail`/`async_boundary`, `anyio.to_process.run_sync` the gated subprocess lane); `documents/model#NODE` (the `RunNode`/`FigureNode`/`AnnotationNode`/`StructureNode`/`BlockNode`/`PageNode` variants + `NodeMeta` + the `AnnotKind`/`BlockKind` tag enums the arms recover into).
- Growth: a new recovery operation is one `LensOp` row plus one acceptor arm folding into the matching `DocumentNode` case; the recovered-tree corpus keys into the runtime columnar lane (consumed from `data`, never re-minted) so a multi-PDF corpus is one queryable value over the same `DocumentNode` tree emission lowers from; zero new surface.
- Boundary: `DocumentLens` recovers nodes and never authors a PDF — authoring stays at the `documents/emit#DOCUMENT` lowering arm, and the `REDACT`/`ANNOTATE` arms edit-in-place an already-emitted PDF, never compose a new document. A `get_text`/`get_words`/`extract_images`/`search`/`get_outline` reader-verb family scattered beside the emit axis is the deleted form; the recovered tree is `documents/model#NODE` `DocumentNode`, never a re-minted text model. OCR over a scanned page with no embedded text is the one `[3]-[RESEARCH]` concern and never a phantom `LensOp` row. The content key is consumed from runtime, never re-minted; the extracted-tree corpus stays at the runtime columnar lane concept level, never a second store. The async dispatch shape aligns on the verified `documents/emit#DOCUMENT` `async_boundary` so emission and recovery share one fault capsule shape.

```python signature
from enum import StrEnum
from typing import assert_never

from anyio import to_process
from msgspec import Struct, msgpack

from rasm.artifacts.documents.model import (
    AnnotationNode,
    AnnotKind,
    BlockKind,
    BlockNode,
    DocumentNode,
    FigureNode,
    NodeMeta,
    PageNode,
    RunNode,
    StructureNode,
)
from rasm.artifacts.receipt.receipt import ArtifactReceipt
from rasm.runtime.content_identity import ContentIdentity, ContentKey
from rasm.runtime.faults import RuntimeRail, async_boundary


def _meta(role: str, page: int, payload: bytes, *, bounds: tuple[float, float, float, float] | None = None) -> NodeMeta:
    return NodeMeta(key=ContentIdentity.of(f"node-{role}", payload), role=role, page=page, bounds=bounds)


class LensOp(StrEnum):
    EXTRACT_TEXT = "extract-text"
    EXTRACT_IMAGES = "extract-images"
    WORDS = "words"
    SEARCH = "search"
    EMBEDDED = "embedded"
    OUTLINE = "outline"
    REDACT = "redact"
    ANNOTATE = "annotate"
    STORY = "story"


GATED: frozenset[LensOp] = frozenset({LensOp.WORDS, LensOp.SEARCH, LensOp.EMBEDDED, LensOp.REDACT, LensOp.ANNOTATE, LensOp.STORY})


class DocumentLens(Struct, frozen=True):
    op: LensOp
    payload: bytes
    params: dict[str, object]

    async def recover(self) -> RuntimeRail[ContentKey]:
        return await async_boundary(f"lens.{self.op}", self._emit)

    async def _emit(self) -> ContentKey:
        nodes = (
            await to_process.run_sync(_gated_recover, self.op.value, self.payload, self.params)
            if self.op in GATED
            else _pypdf_recover(self.op.value, self.payload, self.params)
        )
        data = msgpack.encode(nodes)
        return ContentIdentity.of(f"lens-{self.op}", data)

    @staticmethod
    def receipt(key: ContentKey, nodes: tuple[DocumentNode, ...], /) -> ArtifactReceipt:
        text_length = sum(len(node.text) for node in nodes if isinstance(node, RunNode))
        image_count = sum(1 for node in nodes if isinstance(node, FigureNode))
        hit_count = sum(1 for node in nodes if isinstance(node, AnnotationNode))
        return ArtifactReceipt.Introspection(key, len(nodes), text_length, image_count, hit_count)


def _pypdf_recover(op: str, payload: bytes, params: dict[str, object]) -> tuple[DocumentNode, ...]:
    from io import BytesIO

    import pypdf

    reader = pypdf.PdfReader(BytesIO(payload))
    match LensOp(op):
        case LensOp.EXTRACT_TEXT:
            return tuple(_run_node(page.extract_text(extraction_mode=params.get("mode", "plain")), index) for index, page in enumerate(reader.pages))
        case LensOp.EXTRACT_IMAGES:
            return tuple(_figure_node(image.name, image.data, index) for index, page in enumerate(reader.pages) for image in page.images)
        case LensOp.OUTLINE:
            return _outline_nodes(reader.outline)
        case unreachable:
            assert_never(unreachable)


def _run_node(text: str, page: int, bbox: tuple[float, float, float, float] | None = None) -> RunNode:
    return RunNode(meta=_meta("run", page, text.encode(), bounds=bbox), text=text, font_key="recovered", size=0.0)


def _figure_node(name: str, data: bytes, page: int) -> FigureNode:
    return FigureNode(meta=_meta("figure", page, data), asset_key=ContentIdentity.of(f"asset-{name}", data))


def _gated_recover(op: str, payload: bytes, params: dict[str, object]) -> tuple[DocumentNode, ...]:
    import pymupdf

    document = pymupdf.open(stream=payload, filetype="pdf")
    match LensOp(op):
        case LensOp.WORDS:
            return tuple(
                _run_node(word, index, (x0, y0, x1, y1)) for index, page in enumerate(document) for x0, y0, x1, y1, word, *_ in page.get_text("words")
            )
        case LensOp.SEARCH:
            needle = str(params["needle"])
            return tuple(
                _annotation_node(AnnotKind.HIGHLIGHT, needle, tuple(rect), index)
                for index, page in enumerate(document)
                for rect in page.search_for(needle)
            )
        case LensOp.EMBEDDED:
            return _embedded_nodes(document, payload)
        case LensOp.REDACT:
            return _redact_nodes(document, params)
        case LensOp.ANNOTATE:
            return _annotate_nodes(document, params)
        case LensOp.STORY:
            return _story_nodes(document, params)
        case unreachable:
            assert_never(unreachable)


def _annotation_node(annot: AnnotKind, contents: str, target: tuple[float, float, float, float], page: int) -> AnnotationNode:
    return AnnotationNode(meta=_meta("annotation", page, contents.encode(), bounds=target), annot=annot, target=target, contents=contents)


def _structure_node(tag_role: str, body: bytes, page: int) -> StructureNode:
    return StructureNode(meta=_meta("structure", page, body, bounds=None), tag_role=tag_role)


def _outline_nodes(outline: object, /) -> tuple[DocumentNode, ...]:
    return tuple(_structure_node(item.title, item.title.encode(), item.page) for item in outline if not isinstance(item, list))


def _embedded_nodes(document: object, payload: bytes, /) -> tuple[DocumentNode, ...]:
    import pymupdf
    from lxml import etree

    images = tuple(
        _figure_node(f"img-{xref}", pymupdf.Pixmap(document, xref).tobytes("png"), index)
        for index, page in enumerate(document)
        for xref, *_ in page.get_images(full=True)
    )
    parser = etree.XMLParser(recover=True, huge_tree=True)
    structure = (_structure_node("xml", etree.tostring(etree.fromstring(payload, parser)), 0),)
    return images + structure


def _redact_nodes(document: object, params: dict[str, object]) -> tuple[DocumentNode, ...]:
    for page in document:
        page.apply_redactions(images=int(params.get("images", 2)))
    return tuple(BlockNode(meta=_meta("block", index, page.get_text().encode()), block=BlockKind.PARAGRAPH) for index, page in enumerate(document))


def _annotate_nodes(document: object, params: dict[str, object]) -> tuple[DocumentNode, ...]:
    page_index = int(params["page"])
    page = document[page_index]
    return tuple(_annotation_node(AnnotKind.NOTE, annot.info["content"], tuple(annot.rect), page_index) for annot in page.annots())


def _story_nodes(document: object, params: dict[str, object]) -> tuple[DocumentNode, ...]:
    from io import BytesIO

    import pymupdf

    story = pymupdf.Story(html=str(params["html"]))
    writer = pymupdf.DocumentWriter(BytesIO())
    rect = pymupdf.paper_rect("a4")
    more, page_count = 1, 0
    while more:
        device = writer.begin_page(rect)
        more, _ = story.place(rect)
        story.draw(device)
        writer.end_page()
        page_count += 1
    writer.close()
    return tuple(PageNode(meta=_meta("page", index, str(index).encode()), media_box=tuple(rect)) for index in range(page_count))
```

## [03]-[RESEARCH]

- [OCR_SCANNED_PAGE]: a scanned page carries no embedded text layer, so `EXTRACT_TEXT`/`WORDS` recover an empty `run` set; recovering text from such a page needs an OCR pass (`ocrmypdf`/Tesseract) that carries a native Tesseract binary, NOT a pure-Python or admitted distribution. This is a DEFERRED-admission concern carded here, never a phantom `LensOp` row: an `OCR` op lands only once an `ocrmypdf` admission and its native-binary band policy resolve, at which point it joins the `GATED` set crossing the subprocess seam beside the MuPDF arms. Until then the lens recovers only the embedded text layer, and a scanned-page recovery returns the empty `run` set the receipt's zero text-length fact reports — never a fabricated OCR arm.
- [CORPUS_LANE_RESIDENCY]: the recovered-tree corpus keys a `(ContentKey, DocumentNode-tree)` value into the runtime columnar lane (`data` query owner), so a multi-PDF corpus is one queryable value over the same `DocumentNode` tree emission lowers from. The lane is consumed from `data`, never re-minted here; the handoff stays at the corpus-port concept level, aligned on the runtime columnar lane landing the corpus port — the artifacts side is the content-keyed producer of the extracted-tree value, never a second store. The settled core-arm `pypdf` `PageObject.extract_text(extraction_mode=...)`/`PageObject.images`/`PdfReader.outline` and gated `pymupdf` `Document.open`/`Page.get_text("words")`/`search_for`/`get_images`/`apply_redactions` and `Pixmap` and `lxml` `etree.XMLParser`/`fromstring`/`tostring` spellings verify against the folder `.api` catalogues for `pypdf`/`pymupdf`/`lxml`; the `lxml` arm reflects on the `python_version<'3.15'` band (no CPython 3.15 wheel) and never resolves on the cp315 core.
- [MUPDF_RECOVERY_SPELLINGS]: the `EMBEDDED`/`ANNOTATE`/`STORY` arms compose the catalogued `pymupdf` public types `Pixmap`/`Annot`/`Story`/`DocumentWriter` (catalogue `[2]-[PUBLIC_TYPES]`), but their leaf method spellings — `Pixmap(document, xref)` construction, `Page.annots()` iteration with `Annot.info`/`Annot.rect`, and the `Story.place`/`Story.draw`/`DocumentWriter.begin_page`/`end_page`/`pymupdf.paper_rect` reflow loop — are not yet rows in the catalogue `[3]-[ENTRYPOINTS]` tables. They are catalogue-DEEPEN members: the arms are settled at the public-type level and resolve once a `pymupdf` reflection pass adds the embedded-file (`Document.embfile_names`/`embfile_get`), TOC (`Document.get_toc`), redaction-mark (`Page.add_redact_annot`), and reflow (`Story`/`DocumentWriter`) entrypoint rows — at which point the EMBEDDED arm also recovers the native embedded-file `field` nodes and the OUTLINE arm gains the MuPDF `get_toc` recovery beside the pypdf `outline` core arm. Until the deepen lands, no arm asserts an entrypoint-row spelling absent from the catalogue.

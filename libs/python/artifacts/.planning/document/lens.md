# [PY_ARTIFACTS_LENS]

The recover-TO half of the bidirectional `document` seam: where `document/model#NODE` `DocumentNode` is the single interior tree every backend lowers FROM, `DocumentLens` is the inverse that recovers a `DocumentNode` tree back OUT of an already-emitted PDF, a scanned raster, or an office/structured-text payload — text, image, word-geometry, region, ruled-table grid, tagged-structure tree, hyperlink, document metadata, full-text search, scanned-page OCR, native outline and embedded-file harvest, form-widget recovery, annotation recovery, and tabular spreadsheet/ODF/`.docx`/YAML/TOML/XML ingest. `DocumentLens` is ONE owner discriminating recovery operation over a `LensOp` closed `StrEnum`, routed by the one `_ROUTES` `frozendict[LensOp, tuple[RecoverArm, LensProvider]]` table whose value binds each op's acceptor to its default engine, never a `get_text`/`get_words`/`extract_images`/`find_tables` verb family and never an `if op == ...` reader ladder. Each `LensProvider` carries its own runtime `band`: the native native packages (`pymupdf` `cp310-native`, `pikepdf` `worker-native`, both forward-compatible to runtime and ungated in the manifest) resolve in-process on the core beside the pure-Python `pypdf`/`pdfplumber`/`odfpy`/`python-docx`/`ruamel-yaml`/`tomlkit` readers, while only the no-runtime-package companions (`ocrmypdf` blocked on the `pi-heif`/`libheif` build, `python-calamine` PyO3/Rust, `lxml` libxml2) cross the `anyio.to_process.run_sync` subprocess seam onto the worker lane.

Per-op input is one frozen `LensSpec` admitted exactly once at `DocumentLens.of` through the closed `LensPayload` `TypedDict` and its module-level `TypeAdapter`, the per-op `_REQUIRED` precondition making admission total over well-formed requests — never a `dict[str, object]` bag forwarded into the interior, never re-read by `params.get(...)` per arm. Every recovery steps the frozen owner to `recovered` through `copy.replace` under the `@receipted(_REDACTION)` weave (the pure `_emit` returning the stepped `Self`, a `ReceiptContributor`, exactly as `document/emit#DOCUMENT` `_emit` returns its stepped plan), and `recover` maps that stepped owner to a `RuntimeRail[ContentKey]`; `contribute` reads the stepped `recovered` and mints the one `core/receipt#RECEIPT` `ArtifactReceipt.Introspection` five-field case folded over `walk`, never an in-process re-run of a worker arm. Production and extraction are inverses over the one node algebra, so a `document/emit#DOCUMENT` emission and a `DocumentLens` recovery round-trip through `DocumentNode` with `DocumentDelta` (`document/model#DELTA`) defined once.

## [01]-[INDEX]


## [02]-[LENS]

- Owner: `DocumentLens` the one extraction owner discriminating recovery operation; `LensOp` the closed `StrEnum` of recover-TO operations; `LensProvider` the closed policy-as-value engine vocabulary whose `band` property keys the `_GATED_PROVIDERS` frozenset so the `TABLE`/`SEARCH`/`OCR`/`OUTLINE`/`LINK`/`STRUCTURE` ops that admit two engines dispatch a provider value rather than reconstructing the engine choice; `LensSpec` the one frozen per-op material `Struct` admitted at `.of` through the `LensPayload` `TypedDict(closed=True)` + module-level `TypeAdapter`; `LensFault` the closed `@tagged_union` over the `payload`/`unsatisfied` admission causes; the recovered `DocumentNode` (owned by `document/model#NODE`) is the single interior representation every arm folds back into, never a per-operation result type. One polymorphic `_node(NodeKind, role, page, payload, *, bounds, **slot: Unpack[NodeSlot])` constructor mints every one of the ten variants over a `NodeMeta` whose content key joins the structural discriminant (positional `bounds`, else a sibling `ordinal`/position-prefixed payload) to the content so identical-content siblings never collapse onto one slot, the per-kind material admitted through the closed `NodeSlot` `TypedDict` rather than an untyped `**slot: object` bag, honoring the real field contract — a `RunNode` carries recovered `font_key`/`size`/`weight`/`italic`/`direction`/`script`/`decorations`/`color` over the full `RunNode` field contract (never the `rtl` field the model retired for `direction`), a `TableNode` carries `spans`/`header_rows`, a `FigureNode` carries `media_type`/`intrinsic`, an `AnnotationNode` carries its `AnnotTarget` `link`, a `StructureNode` carries a full `StructRole`, a `ListNode` carries its `ListKind` — never a `_run_node`/`_figure_node`/`_annotation_node`/`_structure_node` sibling-factory family and never the non-total node match the missing `NodeKind.LIST` arm was; the per-op acceptors are values in the one `_ROUTES` table whose `(arm, default_provider)` row collapses the prior parallel `_ARMS`/`_DEFAULT_PROVIDER` pair into one correspondence, the band recovered once from `provider.band`, never a third reader dispatcher and never an `if op == ...` cascade per band.
- Cases: `LensOp` rows `EXTRACT_TEXT` (`LensProvider.PYPDF` core over `PageObject.extract_text(extraction_mode=)` → `RunNode` leaves; the `LensProvider.MUPDF` core row over `Page.get_text("dict", flags=)` whose span `flags` recover `weight`/`italic`/`script` and whose span `color` int unpacks to the `RunNode.color` `Rgb` → `BlockNode` of styled `RunNode` spans) · `EXTRACT_IMAGES` (`LensProvider.PYPDF` core over `PageObject.images` → content-keyed `FigureNode` leaves) · `TABLE` (`LensProvider.PLUMBER` core over pdfplumber `Page.find_tables(table_settings=)`/`Table.extract`/`Table.bbox`/`Table.cells`, or `LensProvider.MUPDF` core over the native `find_tables(vertical_strategy=, horizontal_strategy=, snap_tolerance=, join_tolerance=)` `TableFinder` whose `Table.extract`/`Table.bbox`/`Table.header` carry the grid shape → `TableNode` of `RunNode` cells, the pdfplumber `Table.cells` bbox set folding merged-cell `(row, col, col_span, row_span)` quads into `TableNode.spans` and the MuPDF `Table.header` presence into `TableNode.header_rows`) · `WORDS` (pdfplumber `Page.extract_words(x_tolerance=, y_tolerance=, use_text_flow=, split_at_punctuation=, extra_attrs=("fontname","size"))` lifting each word's `font_key`/`size` off the recovered char attributes → word-geometry `RunNode` leaves, core) · `REGION` (pdfplumber `Page.within_bbox(bbox)`/`CroppedPage.extract_text_lines(strip=, return_chars=)` per-line bbox-restricted recovery → `RunNode` leaves, core) · `OUTLINE` (`LensProvider.MUPDF` core over `Document.get_toc(simple=True)` level/title/page rows → level-resolved `StructureNode` outline tree over the `H1`-`H6` `_HEADING` vocabulary; the `LensProvider.PYPDF` core alternate over `PdfReader.outline` recovering the nested-list depth as the heading level through `_pypdf_outline`) · `STRUCTURE` (`LensProvider.PLUMBER` core over pdfplumber `Page.structure_tree` tagged-PDF logical subtree, or `LensProvider.PIKEPDF` core over the qpdf `Pdf.Root[/StructTreeRoot]`→`/K`→`/S` object walk → `StructureNode` tree whose `_struct_role` resolves each PDF role-name string to a `StandardRole(StructEltKind)` or the one `ForeignRole` escape — the recover-TO inverse the `document/tagged#ACCESS` audit folds over) · `LINK` (`LensProvider.MUPDF` core over `Page.get_links()` URI/GOTO links, or `LensProvider.PLUMBER` core over `Page.hyperlinks` → `AnnotationNode(annot=LINK)` leaves carrying the `AnnotTarget` `Uri(href)`/`Dest(page)` the model owns, so a recovered link round-trips through the emit `#link` lowering rather than dropping its target into `contents`) · `METADATA` (`LensProvider.PYPDF` core over `PdfReader.metadata` info dict → `FieldNode` leaves under one `StructureNode(StructEltKind.DOCUMENT)`, the `_META_KEY` row normalizing each `/Title`/`/Author`/`/Subject`/… key to its canonical field name) · `SEARCH` (`LensProvider.PLUMBER` core over `Page.search(pattern, regex=, case=)`, or `LensProvider.MUPDF` core over `Page.search_for(needle)` → hit-region `AnnotationNode(annot=HIGHLIGHT)` leaves) · `OCR` (`LensProvider.OCRMYPDF` gated over `ocr(source, target, sidecar=, mode=, output_type='pdfa', language=, deskew=, clean=, rotate_pages=, optimize=)` whose `ExitCode` return gates the sidecar text feed, the `pymupdf` single-image intake wrapping a raster losslessly through `new_page`/`Page.insert_image(rect, stream=)` when `not Document.is_pdf`; or `LensProvider.MUPDF` core over the in-process per-page `Page.get_textpage_ocr(language=, dpi=, full=)`→`get_text("words")` graft) · `EMBEDDED` (`LensProvider.MUPDF` core over `Document.embfile_names`/`embfile_get`/`embfile_info` attached-file table → `FieldNode` leaves plus `Page.get_images(full=True)`/`Pixmap(document, xref)` placed rasters → `FigureNode` leaves carrying the pixmap `intrinsic` `(width, height)` and `image/png` `media_type`) · `WIDGET` (`LensProvider.MUPDF` core over `Page.widgets()`/`Widget.field_name`/`field_value`/`field_type` → `FieldNode` leaves, the `field_type` int resolved through `_widget_field` mapping the catalogued `PDF_WIDGET_TYPE_*` symbol names via `getattr` to `FieldKind`) · `ANNOTATE` (`LensProvider.MUPDF` core over `Page.annots()`/`Annot.info`/`Annot.rect`/`Annot.type` → `AnnotationNode` leaves, the `Annot.type[1]` name string resolved through `_annot_kind` to `AnnotKind`) · `ODS_READ` (`LensProvider.ODFPY` core over odfpy `load`/`getElementsByType(Table|TableRow|TableCell)`/`teletype.extractText`/`getAttribute("numbercolumnsrepeated")` run-length cell expansion → `TableNode` of `RunNode` cells per sheet) · `ODT_READ` (`LensProvider.ODFPY` core over odfpy `load(BytesIO)` then a document-order `body.childNodes` walk dispatching each `isInstanceOf(text.H)`/`isInstanceOf(text.P)` element — a `text:h` to a `SectionNode` at its `getAttribute("outlinelevel")` level, a `text:p` to a `BlockNode` of `RunNode`, both content-flattened through `teletype.extractText` — the text-document sibling of `ODS_READ` on the same odfpy core, the recover-TO inverse of the emit `ODT` lowering) · `XLSX_READ` (`LensProvider.CALAMINE` gated over python-calamine `CalamineWorkbook.from_object(BytesIO(payload))`/`sheet_names`/`get_sheet_by_name`/`CalamineSheet.to_python(skip_empty_area=)` → `TableNode` of `RunNode` cells per sheet, `CalamineSheet.merged_cell_ranges` folding into `TableNode.spans`, the OOXML/binary-Excel formats odfpy's OASIS parser does not cover) · `DOCX_READ` (`LensProvider.DOCX` core over python-docx `Document(BytesIO)`/`Document.iter_inner_content()` document-order walk, each heading-styled `Paragraph` folding to a `SectionNode`, each `_DOCX_LIST`-styled run of consecutive paragraphs grouping through `itertools.groupby` into one `ListNode(ListKind)` — the inverse of the emit `List Bullet`/`List Number` lowering, never the deleted `BlockKind.LIST_ITEM` the model retired — each `Table` to a `TableNode`, the tree rooted in one `StructureNode(StructEltKind.DOCUMENT)` carrying the `core_properties.title`/`author`) · `YAML_READ` (`LensProvider.RUAMEL` core over ruamel-yaml `YAML(typ='rt').load_all` — the single-document case subsumed, never a `multi` knob selecting `load` — each mapping folding through `_value_node` to a `BlockNode` of keyed children, each sequence to a `ListNode(ORDERED)`, each scalar to a `RunNode`) · `TOML_READ` (`LensProvider.TOMLKIT` core over tomlkit `parse(payload).unwrap()`, the same `_value_node` recursion) · `XML_READ` (`LensProvider.LXML` gated over lxml `etree.XMLParser(recover=, resolve_entities=False, no_network=True, huge_tree=False)`/`etree.fromstring`, each element folding through `_element_node` into a nested `BlockNode`, ONLY the serialized `DocumentNode` tree crossing back across the interpreter seam) — routed by the one `_ROUTES` row, never an `if op == ...` ladder.
- Entry: `DocumentLens.of(op, payload, *, provider=None, **raw: Unpack[LensPayload])` admits raw kwargs once through the `_PAYLOAD` `TypeAdapter`, materializes the `LensSpec`, and rejects an op whose `_REQUIRED` field is empty before the interior, returning `Result[Self, LensFault]`. `DocumentLens.recover` is `async` over the runtime `async_boundary`: it runs the `@receipted` `_emit`, which resolves `(arm, default) = _ROUTES[self.op]` and `provider = self.provider or default`, then folds through `provider.band` under the shared `_OFFLOAD` limiter so no synchronous native extraction runs inline on the loop — a `GATED` row crosses `to_process.run_sync(_gated_recover, self)` onto the worker that re-resolves the SAME `_ROUTES` row and reifies the module-scope `lazy` `ocrmypdf`/`python_calamine`/`lxml` bindings, while a `CORE` row crosses `to_thread.run_sync` for the GIL-releasing native extraction — and returns the stepped `Self` carrying `recovered`; `recover` maps that owner to `RuntimeRail[ContentKey]` keyed by the model `node_digest` merkle over the recovered roots through `ContentIdentity.of`, never a second `msgspec.msgpack` encoder beside the model's canonical codec.
- Receipt: each recovery contributes the `core/receipt#RECEIPT` `ArtifactReceipt.Introspection(key, nodes, text_len, images, hits)` five-field case projected from the stepped `self.recovered` by `walk` folding `isinstance` over the `RunNode`/`FigureNode`/`AnnotationNode` variants — a nested `TableNode`/`ListNode` of `RunNode` cells contributes its full sub-tree text and figure count, the tag riding the encoded `kind` field never a runtime `.tag` attribute. `contribute` reads the stepped `recovered` directly and never re-runs `_emit`, so a worker-gated arm is never re-imported on the core during the receipt harvest; the prior `self.recovered or self._recovered()` re-run fallback is the deleted form.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
from collections.abc import Callable, Iterable, Iterator, Mapping
from copy import replace
from enum import StrEnum
from io import BytesIO
from itertools import groupby
from typing import Final, Literal, NotRequired, ReadOnly, Self, TypedDict, Unpack, assert_never

from anyio import CapacityLimiter, to_process, to_thread
from builtins import frozendict
from expression import Error, Ok, Result, case, tag, tagged_union
from expression.collections import Map
from msgspec import Struct, field
from pydantic import TypeAdapter, ValidationError

from artifacts.core.receipt import ArtifactReceipt
from artifacts.document.model import (
    AnnotationNode,
    AnnotKind,
    AnnotTarget,
    BlockKind,
    BlockNode,
    Dest,
    DocumentNode,
    FieldFlag,
    FieldKind,
    FieldNode,
    FigureNode,
    ForeignRole,
    ListKind,
    ListNode,
    NodeKind,
    NodeMeta,
    NoTarget,
    PageNode,
    RunNode,
    RunScript,
    SectionNode,
    StandardRole,
    StructEltKind,
    StructRole,
    StructureNode,
    TableNode,
    TextDecoration,
    TextDirection,
    Uri,
    node_digest,
    walk,
)
from rasm.runtime.content_identity import ContentIdentity, ContentKey
from rasm.runtime.faults import RuntimeRail, async_boundary
from rasm.runtime.receipts import OPEN, Receipt, receipted

lazy import docx
lazy import ocrmypdf
lazy import pdfplumber
lazy import pikepdf
lazy import pymupdf
lazy import pypdf
lazy import python_calamine
lazy import tomlkit
lazy from lxml import etree
lazy from odf.opendocument import load
lazy from odf.table import Table, TableCell, TableRow
lazy from odf.teletype import extractText
lazy from odf.text import H, P
lazy from ruamel.yaml import YAML
lazy from tempfile import NamedTemporaryFile

# --- [TYPES] ----------------------------------------------------------------------------

type Bounds = tuple[float, float, float, float]
type Grid = list[list[str | None]]
type Spans = tuple[tuple[int, int, int, int], ...]
type RecoverArm = Callable[[bytes, "LensProvider", "LensSpec"], tuple[DocumentNode, ...]]


class LensOp(StrEnum):
    EXTRACT_TEXT = "extract-text"
    EXTRACT_IMAGES = "extract-images"
    TABLE = "table"
    WORDS = "words"
    REGION = "region"
    OUTLINE = "outline"
    STRUCTURE = "structure"
    LINK = "link"
    METADATA = "metadata"
    SEARCH = "search"
    OCR = "ocr"
    EMBEDDED = "embedded"
    WIDGET = "widget"
    ANNOTATE = "annotate"
    XLSX_READ = "xlsx-read"
    ODS_READ = "ods-read"
    ODT_READ = "odt-read"
    DOCX_READ = "docx-read"
    YAML_READ = "yaml-read"
    TOML_READ = "toml-read"
    XML_READ = "xml-read"


class LensBand(StrEnum):
    CORE = "core"
    GATED = "gated"


class LensProvider(StrEnum):
    PYPDF = "pypdf"
    PLUMBER = "pdfplumber"
    MUPDF = "pymupdf"
    PIKEPDF = "pikepdf"
    ODFPY = "odfpy"
    DOCX = "python-docx"
    RUAMEL = "ruamel-yaml"
    TOMLKIT = "tomlkit"
    OCRMYPDF = "ocrmypdf"
    CALAMINE = "python-calamine"
    LXML = "lxml"

    @property
    def band(self) -> LensBand:
        return LensBand.GATED if self in _GATED_PROVIDERS else LensBand.CORE


# --- [CONSTANTS] ------------------------------------------------------------------------

# the keep-all redaction the `@receipted` weave rides; `Redaction` is a `Struct(classified, salt)` with
# NO named instances, so `Redaction.STRUCTURAL` was a phantom — the empty `classified` map scrubs nothing.
_REDACTION: Final[Redaction] = Redaction(classified=Map.empty())
_ORIGIN: Final[Bounds] = (0.0, 0.0, 0.0, 0.0)
_RECOVERED_FONT: Final[str] = "recovered"
_HEADING_FLOOR: Final[int] = 1
_HEADING_CEIL: Final[int] = 6
_BOLD_FLAG: Final[int] = 16   # pymupdf span flag bit 4 — bold
_ITALIC_FLAG: Final[int] = 2  # pymupdf span flag bit 1 — italic
_SUPER_FLAG: Final[int] = 1   # pymupdf span flag bit 0 — superscript
_GATED_PROVIDERS: Final[frozenset[LensProvider]] = frozenset(
    {LensProvider.OCRMYPDF, LensProvider.CALAMINE, LensProvider.LXML}
)
_OFFLOAD: Final = CapacityLimiter(8)  # bounds the native-extraction thread/process fan-out so no arm runs inline on the loop
_HEADING: Final[tuple[StructEltKind, ...]] = (
    StructEltKind.H1, StructEltKind.H2, StructEltKind.H3, StructEltKind.H4, StructEltKind.H5, StructEltKind.H6,
)

# --- [ERRORS] ---------------------------------------------------------------------------


@tagged_union(frozen=True)
class LensFault:
    # the closed ADMISSION vocabulary `of` produces; every arm-level provider raise
    # (`pymupdf.FileDataError`/`pdfplumber.PdfminerException`/`python_calamine.CalamineError`/
    # `ocrmypdf.ExitCodeException`/`lxml.etree.XMLSyntaxError`) converts to the runtime `BoundaryFault`
    # at the `async_boundary` capsule, never into this interior vocabulary.
    tag: Literal["payload", "unsatisfied"] = tag()
    payload: tuple[str, ...] = case()              # the rejected LensPayload key paths
    unsatisfied: tuple[LensOp, str] = case()       # an op whose `_REQUIRED` input field is empty


# --- [MODELS] ---------------------------------------------------------------------------


class LensSpec(Struct, frozen=True, omit_defaults=True):
    mode: str = "plain"                            # pypdf `extract_text(extraction_mode=)`
    flags: int = 0                                 # pymupdf `get_text("dict", flags=)`; 0 -> TEXTFLAGS_DICT
    x_tolerance: float = 3.0
    y_tolerance: float = 3.0
    use_text_flow: bool = False
    split_at_punctuation: bool = False
    extra_attrs: tuple[str, ...] = ("fontname", "size")
    vertical: str = "lines"                        # pdfplumber/pymupdf `vertical_strategy`
    horizontal: str = "lines"
    snap_tolerance: float = 3.0
    join_tolerance: float = 3.0
    edge_min_length: float = 3.0
    intersection_tolerance: float = 3.0
    text_tolerance: float = 3.0
    explicit_vertical: tuple[float, ...] = ()
    explicit_horizontal: tuple[float, ...] = ()
    bbox: Bounds | None = None                     # REGION crop window
    repair: bool = False                           # pdfplumber Ghostscript pre-repair
    needle: str = ""                               # SEARCH pattern
    regex: bool = True
    case_sensitive: bool = True
    language: tuple[str, ...] = ("eng",)           # OCR Tesseract language packs
    dpi: int = 72
    full: bool = False
    filetype: str = "pdf"
    output_type: str = "pdfa"                      # ocrmypdf PDF/A target
    ocr_mode: str = "force"                        # ocrmypdf processing mode
    deskew: bool = False
    clean: bool = False
    rotate_pages: bool = False
    optimize: int = 1
    load_tables: bool = False                      # python-calamine Excel-table parse
    sheets: tuple[str, ...] = ()
    skip_empty_area: bool = True
    typ: str = "rt"                                # ruamel-yaml round-trip loader
    recover: bool = True                           # lxml recovering parser


class DocumentLens(Struct, frozen=True):
    op: LensOp
    payload: bytes
    spec: LensSpec = field(default_factory=LensSpec)
    provider: LensProvider | None = None
    recovered: tuple[DocumentNode, ...] = ()

    @receipted(_REDACTION)  # the harvest weave drains `contribute` off the stepped owner via `Signals.emit_async`
    async def _emit(self) -> Self:
        # the synchronous native extraction NEVER runs inline on the loop: a GATED arm crosses the process seam,
        # a CORE arm the GIL-releasing thread seam, each bounded by the shared `_OFFLOAD` limiter.
        arm, default = _ROUTES[self.op]
        provider = self.provider or default
        nodes = (
            await to_process.run_sync(_gated_recover, self, limiter=_OFFLOAD)
            if provider.band is LensBand.GATED
            else await to_thread.run_sync(arm, self.payload, provider, self.spec, limiter=_OFFLOAD)
        )
        return replace(self, recovered=nodes)

    async def recover(self) -> RuntimeRail[ContentKey]:
        railed = await async_boundary(f"lens.{self.op.value}", self._emit)
        return railed.map(lambda stepped: ContentIdentity.of(f"lens-{self.op.value}", tuple(node_digest(node) for node in stepped.recovered)))

    def contribute(self) -> Iterable[Receipt]:
        # phase is the constant `"emitted"` the receipt owner fixes (KNOB_TEST); rides the stepped
        # `recovered`, never an in-process re-run — exactly the `core/receipt#RECEIPT` `contribute(self)` port.
        # the artifact key is the model `node_digest` merkle over the recovered roots (the identity owner's
        # `merkle` modality), never a second `msgspec.msgpack` encoder beside the model's canonical codec.
        flat = tuple(child for node in self.recovered for child in walk(node))
        key = ContentIdentity.of(f"lens-{self.op.value}", tuple(node_digest(node) for node in self.recovered))
        yield from ArtifactReceipt.Introspection(
            key,
            len(flat),
            sum(len(node.text) for node in flat if isinstance(node, RunNode)),
            sum(isinstance(node, FigureNode) for node in flat),
            sum(isinstance(node, AnnotationNode) for node in flat),
        ).contribute()

    @classmethod
    def of(cls, op: LensOp, payload: bytes, /, *, provider: LensProvider | None = None, **raw: Unpack[LensPayload]) -> Result[Self, LensFault]:
        try:
            admitted = _PAYLOAD.validate_python(raw)
        except ValidationError as fault:
            return Error(LensFault(payload=tuple(str(error["loc"]) for error in fault.errors())))
        spec = LensSpec(**admitted)
        missing = next((name for name in _REQUIRED.get(op, ()) if not getattr(spec, name)), None)
        return Error(LensFault(unsatisfied=(op, missing))) if missing else Ok(cls(op=op, payload=payload, spec=spec, provider=provider))


# --- [BOUNDARIES] -----------------------------------------------------------------------


class LensPayload(TypedDict, closed=True):
    mode: NotRequired[ReadOnly[str]]
    flags: NotRequired[ReadOnly[int]]
    x_tolerance: NotRequired[ReadOnly[float]]
    y_tolerance: NotRequired[ReadOnly[float]]
    use_text_flow: NotRequired[ReadOnly[bool]]
    split_at_punctuation: NotRequired[ReadOnly[bool]]
    extra_attrs: NotRequired[ReadOnly[tuple[str, ...]]]
    vertical: NotRequired[ReadOnly[str]]
    horizontal: NotRequired[ReadOnly[str]]
    snap_tolerance: NotRequired[ReadOnly[float]]
    join_tolerance: NotRequired[ReadOnly[float]]
    edge_min_length: NotRequired[ReadOnly[float]]
    intersection_tolerance: NotRequired[ReadOnly[float]]
    text_tolerance: NotRequired[ReadOnly[float]]
    explicit_vertical: NotRequired[ReadOnly[tuple[float, ...]]]
    explicit_horizontal: NotRequired[ReadOnly[tuple[float, ...]]]
    bbox: NotRequired[ReadOnly[Bounds | None]]
    repair: NotRequired[ReadOnly[bool]]
    needle: NotRequired[ReadOnly[str]]
    regex: NotRequired[ReadOnly[bool]]
    case_sensitive: NotRequired[ReadOnly[bool]]
    language: NotRequired[ReadOnly[tuple[str, ...]]]
    dpi: NotRequired[ReadOnly[int]]
    full: NotRequired[ReadOnly[bool]]
    filetype: NotRequired[ReadOnly[str]]
    output_type: NotRequired[ReadOnly[str]]
    ocr_mode: NotRequired[ReadOnly[str]]
    deskew: NotRequired[ReadOnly[bool]]
    clean: NotRequired[ReadOnly[bool]]
    rotate_pages: NotRequired[ReadOnly[bool]]
    optimize: NotRequired[ReadOnly[int]]
    load_tables: NotRequired[ReadOnly[bool]]
    sheets: NotRequired[ReadOnly[tuple[str, ...]]]
    skip_empty_area: NotRequired[ReadOnly[bool]]
    typ: NotRequired[ReadOnly[str]]
    recover: NotRequired[ReadOnly[bool]]


_PAYLOAD: Final = TypeAdapter(LensPayload)
# the per-op precondition: a row's named `LensSpec` fields must be non-empty so the interior is total.
_REQUIRED: Final[frozendict[LensOp, tuple[str, ...]]] = frozendict({
    LensOp.REGION: ("bbox",),
    LensOp.SEARCH: ("needle",),
})


# --- [OPERATIONS] -----------------------------------------------------------------------


class NodeSlot(TypedDict, total=False, closed=True):
    # the closed per-kind construction payload `_node` admits — typed ingress replacing the `**slot: object`
    # bag; `ordinal` is the sibling discriminant the content key folds when a node carries no positional `bounds`.
    ordinal: int
    text: str
    font_key: str
    size: float
    weight: int
    italic: bool
    direction: TextDirection
    script: RunScript
    decorations: tuple[TextDecoration, ...]
    color: tuple[int, int, int]
    name: str
    alt: str
    media_type: str
    intrinsic: tuple[float, float] | None
    caption: tuple[DocumentNode, ...]
    rows: tuple[tuple[DocumentNode, ...], ...]
    spans: Spans
    header_rows: int
    field: FieldKind
    value: str | bool | None
    flags: tuple[FieldFlag, ...]
    options: tuple[str, ...]
    annot: AnnotKind
    contents: str
    link: AnnotTarget
    struct_role: StructRole | None
    elt: StructEltKind
    children: tuple[DocumentNode, ...]
    level: int
    heading: tuple[DocumentNode, ...]
    block: BlockKind
    runs: tuple[DocumentNode, ...]
    list_kind: ListKind
    items: tuple[DocumentNode, ...]


def _node(kind: NodeKind, role: str, page: int, payload: bytes, *, bounds: Bounds | None = None, **slot: Unpack[NodeSlot]) -> DocumentNode:
    # the content key joins the STRUCTURAL discriminant (positional `bounds`, else the sibling `ordinal`) to the
    # payload so two identical-content siblings under one parent never collapse onto one slot (`boundaries.md` MEMO_KEY).
    seed = repr(bounds).encode() if bounds is not None else f"#{slot.get('ordinal', 0)}".encode()
    meta = NodeMeta(key=ContentIdentity.of(f"node-{role}-{page}", seed + payload), role=role, page=page, bounds=bounds)
    match kind:
        case NodeKind.RUN:
            return RunNode(meta=meta, text=slot.get("text", ""), font_key=slot.get("font_key", _RECOVERED_FONT), size=slot.get("size", 0.0), weight=slot.get("weight", 400), italic=slot.get("italic", False), direction=slot.get("direction", TextDirection.AUTO), script=slot.get("script", RunScript.NORMAL), decorations=slot.get("decorations", ()), color=slot.get("color", (0, 0, 0)))
        case NodeKind.FIGURE:
            return FigureNode(meta=meta, asset_key=ContentIdentity.of(f"asset-{slot['name']}", payload), alt=slot.get("alt", ""), media_type=slot.get("media_type", "image/png"), intrinsic=slot.get("intrinsic"), caption=slot.get("caption", ()))
        case NodeKind.TABLE:
            return TableNode(meta=meta, rows=slot.get("rows", ()), spans=slot.get("spans", ()), header_rows=slot.get("header_rows", 0))
        case NodeKind.FIELD:
            return FieldNode(meta=meta, name=slot["name"], field=slot.get("field", FieldKind.TEXT), value=slot.get("value"), flags=slot.get("flags", ()), options=slot.get("options", ()))
        case NodeKind.ANNOTATION:
            return AnnotationNode(meta=meta, annot=slot.get("annot", AnnotKind.NOTE), target=bounds or _ORIGIN, contents=slot.get("contents", ""), link=slot.get("link", NoTarget()))
        case NodeKind.STRUCTURE:
            return StructureNode(meta=meta, role=slot.get("struct_role") or StandardRole(elt=slot.get("elt", StructEltKind.SECT)), children=slot.get("children", ()))
        case NodeKind.SECTION:
            return SectionNode(meta=meta, level=slot.get("level", 1), heading=slot.get("heading", ()), children=slot.get("children", ()))
        case NodeKind.BLOCK:
            return BlockNode(meta=meta, block=slot.get("block", BlockKind.PARAGRAPH), level=slot.get("level", 1), runs=slot.get("runs", ()), children=slot.get("children", ()))
        case NodeKind.LIST:
            return ListNode(meta=meta, list_kind=slot.get("list_kind", ListKind.UNORDERED), items=slot.get("items", ()))
        case NodeKind.PAGE:
            return PageNode(meta=meta, media_box=bounds or _ORIGIN)
        case _ as unreachable:
            assert_never(unreachable)


def _rgb(color: int) -> tuple[int, int, int]:
    return ((color >> 16) & 0xFF, (color >> 8) & 0xFF, color & 0xFF)


def _table_node(grid: Grid, bbox: Bounds, page: int, *, role: str = "table", spans: Spans = (), header_rows: int = 0) -> TableNode:
    rows = tuple(
        tuple(_node(NodeKind.RUN, "cell", page, f"{r}:{c}:{cell or ''}".encode(), text=cell or "") for c, cell in enumerate(row))
        for r, row in enumerate(grid)  # the `r:c:` payload prefix keys identical-content cells (empty, repeated) distinctly
    )
    return _node(NodeKind.TABLE, role, page, repr(grid).encode(), bounds=bbox, rows=rows, spans=spans, header_rows=header_rows)


def _outline_node(level: int, title: str, page: int, ordinal: int = 0, /) -> DocumentNode:
    elt = _HEADING[min(max(level, _HEADING_FLOOR), _HEADING_CEIL) - 1]
    return _node(NodeKind.STRUCTURE, title, page, f"{ordinal}:{elt.value}:{title}".encode(), elt=elt)  # ordinal keys same-title siblings distinctly


def _struct_role(name: str) -> StructRole:
    try:
        return StandardRole(elt=StructEltKind(name))
    except ValueError:
        return ForeignRole(role=name) if name else StandardRole(elt=StructEltKind.SECT)


def _link_target(link: Mapping[str, object]) -> AnnotTarget:
    return Uri(href=str(link["uri"])) if "uri" in link else Dest(page=int(link.get("page", 0) or 0))


# --- [READER_ARMS] ----------------------------------------------------------------------


def _text_arm(payload: bytes, provider: LensProvider, spec: LensSpec) -> tuple[DocumentNode, ...]:
    if provider is LensProvider.MUPDF:
        with pymupdf.open(stream=payload, filetype="pdf") as document:
            return tuple(
                _node(
                    NodeKind.BLOCK, "block", index,
                    (block["lines"][0]["spans"][0]["text"].encode() if block.get("lines") and block["lines"][0].get("spans") else b""),
                    bounds=tuple(block["bbox"]), block=BlockKind.PARAGRAPH,
                    runs=tuple(
                        _node(
                            NodeKind.RUN, "span", index, span["text"].encode(), bounds=tuple(span["bbox"]),
                            text=span["text"], font_key=span.get("font", _RECOVERED_FONT), size=float(span.get("size", 0.0)),
                            weight=700 if int(span.get("flags", 0)) & _BOLD_FLAG else 400,
                            italic=bool(int(span.get("flags", 0)) & _ITALIC_FLAG),
                            direction=TextDirection.RTL if line.get("dir", (1.0, 0.0))[0] < 0 else TextDirection.LTR,
                            script=RunScript.SUPER if int(span.get("flags", 0)) & _SUPER_FLAG else RunScript.NORMAL,
                            color=_rgb(int(span.get("color", 0))),
                        )
                        for line in block.get("lines", ()) for span in line.get("spans", ())
                    ),
                )
                for index, page in enumerate(document)
                for block in page.get_text("dict", flags=spec.flags or pymupdf.TEXTFLAGS_DICT)["blocks"]
                if block.get("type", 1) == 0
            )
    reader = pypdf.PdfReader(BytesIO(payload))
    return tuple(
        _node(NodeKind.RUN, "run", index, text.encode(), text=text)
        for index, page in enumerate(reader.pages)
        if (text := page.extract_text(extraction_mode=spec.mode))
    )


def _images_arm(payload: bytes, _provider: LensProvider, _spec: LensSpec) -> tuple[DocumentNode, ...]:
    reader = pypdf.PdfReader(BytesIO(payload))
    return tuple(
        _node(NodeKind.FIGURE, "figure", index, image.data, name=image.name)
        for index, page in enumerate(reader.pages)
        for image in page.images
    )


def _words_arm(payload: bytes, _provider: LensProvider, spec: LensSpec) -> tuple[DocumentNode, ...]:
    with pdfplumber.open(BytesIO(payload), repair=spec.repair) as document:
        return tuple(
            _node(
                NodeKind.RUN, "word", index, word["text"].encode(),
                bounds=(word["x0"], word["top"], word["x1"], word["bottom"]),
                text=word["text"], font_key=word.get("fontname", _RECOVERED_FONT), size=float(word.get("size", 0.0)),
            )
            for index, page in enumerate(document.pages)
            for word in page.extract_words(
                x_tolerance=spec.x_tolerance, y_tolerance=spec.y_tolerance, use_text_flow=spec.use_text_flow,
                split_at_punctuation=spec.split_at_punctuation, extra_attrs=list(spec.extra_attrs),
            )
        )


def _region_arm(payload: bytes, _provider: LensProvider, spec: LensSpec) -> tuple[DocumentNode, ...]:
    bbox = spec.bbox or _ORIGIN
    with pdfplumber.open(BytesIO(payload), repair=spec.repair) as document:
        return tuple(
            _node(NodeKind.RUN, "region", index, line["text"].encode(), bounds=(line["x0"], line["top"], line["x1"], line["bottom"]), text=line["text"])
            for index, page in enumerate(document.pages)
            for line in page.within_bbox(bbox).extract_text_lines(strip=True, return_chars=False)
        )


def _table_arm(payload: bytes, provider: LensProvider, spec: LensSpec) -> tuple[DocumentNode, ...]:
    settings: dict[str, object] = {
        "vertical_strategy": spec.vertical, "horizontal_strategy": spec.horizontal,
        "snap_tolerance": spec.snap_tolerance, "join_tolerance": spec.join_tolerance,
        "edge_min_length": spec.edge_min_length, "intersection_tolerance": spec.intersection_tolerance, "text_tolerance": spec.text_tolerance,
    }
    if provider is LensProvider.MUPDF:
        with pymupdf.open(stream=payload, filetype="pdf") as document:
            return tuple(
                _table_node(table.extract(), tuple(table.bbox), index, header_rows=1 if table.header else 0)
                for index, page in enumerate(document)
                for table in page.find_tables(**settings).tables
            )
    plumber = settings | {
        key: list(value)
        for key, value in (("explicit_vertical_lines", spec.explicit_vertical), ("explicit_horizontal_lines", spec.explicit_horizontal))
        if value
    }
    with pdfplumber.open(BytesIO(payload), repair=spec.repair) as document:
        return tuple(
            _table_node(table.extract(), tuple(table.bbox), index, spans=_plumber_spans(table))
            for index, page in enumerate(document.pages)
            for table in page.find_tables(table_settings=plumber)
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


def _outline_arm(payload: bytes, provider: LensProvider, _spec: LensSpec) -> tuple[DocumentNode, ...]:
    if provider is LensProvider.MUPDF:
        with pymupdf.open(stream=payload, filetype="pdf") as document:
            return tuple(_outline_node(level, title, page, ordinal) for ordinal, (level, title, page) in enumerate(document.get_toc(simple=True)))
    reader = pypdf.PdfReader(BytesIO(payload))
    return tuple(_pypdf_outline(reader.outline, _HEADING_FLOOR))


def _pypdf_outline(items: Iterable[object], level: int) -> Iterator[DocumentNode]:
    for ordinal, item in enumerate(items):
        if isinstance(item, list):
            yield from _pypdf_outline(item, level + 1)
        else:
            yield _outline_node(level, item.title, 0, level * 10_000 + ordinal)  # level-scoped ordinal distinguishes page-0 outline siblings


def _structure_arm(payload: bytes, provider: LensProvider, _spec: LensSpec) -> tuple[DocumentNode, ...]:
    if provider is LensProvider.PLUMBER:
        with pdfplumber.open(BytesIO(payload)) as document:
            return tuple(_struct_branch(branch, index) for index, page in enumerate(document.pages) for branch in page.structure_tree)
    with pikepdf.open(BytesIO(payload)) as pdf:
        root = pdf.Root.get(pikepdf.Name.StructTreeRoot)
        return tuple(_struct_obj(kid) for kid in root.get(pikepdf.Name.K, pikepdf.Array())) if root is not None else ()


def _struct_branch(branch: Mapping[str, object], page: int) -> DocumentNode:
    name = str(branch.get("type", ""))
    kids = tuple(_struct_branch(child, page) for child in branch.get("children", ()) if isinstance(child, Mapping))
    return _node(NodeKind.STRUCTURE, name or "Sect", page, repr(branch).encode(), struct_role=_struct_role(name), children=kids)


def _struct_obj(obj: object, page: int = 0) -> DocumentNode:
    if not isinstance(obj, pikepdf.Dictionary):
        return _node(NodeKind.STRUCTURE, "Sect", page, str(obj).encode(), struct_role=StandardRole(elt=StructEltKind.SECT))
    name = str(obj.get(pikepdf.Name.S, "")).removeprefix("/")
    children = tuple(_struct_obj(kid, page) for kid in obj.get(pikepdf.Name.K, pikepdf.Array()) if isinstance(kid, pikepdf.Dictionary))
    return _node(NodeKind.STRUCTURE, name or "Sect", page, str(obj).encode(), struct_role=_struct_role(name), children=children)


def _link_arm(payload: bytes, provider: LensProvider, spec: LensSpec) -> tuple[DocumentNode, ...]:
    if provider is LensProvider.MUPDF:
        with pymupdf.open(stream=payload, filetype="pdf") as document:
            return tuple(
                _node(NodeKind.ANNOTATION, "link", index, str(link.get("uri", link.get("page", ""))).encode(), bounds=tuple(link["from"]), annot=AnnotKind.LINK, contents=str(link.get("uri", "")), link=_link_target(link))
                for index, page in enumerate(document)
                for link in page.get_links()
            )
    with pdfplumber.open(BytesIO(payload), repair=spec.repair) as document:
        return tuple(
            _node(NodeKind.ANNOTATION, "link", index, str(hit.get("uri", "")).encode(), bounds=(hit["x0"], hit["top"], hit["x1"], hit["bottom"]), annot=AnnotKind.LINK, contents=str(hit.get("uri", "")), link=Uri(href=str(hit["uri"])))
            for index, page in enumerate(document.pages)
            for hit in page.hyperlinks
        )


def _metadata_arm(payload: bytes, _provider: LensProvider, _spec: LensSpec) -> tuple[DocumentNode, ...]:
    reader = pypdf.PdfReader(BytesIO(payload))
    info = reader.metadata or {}
    fields = tuple(
        _node(NodeKind.FIELD, _META_KEY.get(slot, slot), 0, str(value).encode(), field=FieldKind.TEXT, name=_META_KEY.get(slot, slot), value=str(value))
        for slot, value in info.items()
        if value
    )
    root = _node(NodeKind.STRUCTURE, str(info.get("/Title", "document")), 0, str(info.get("/Author", "")).encode(), elt=StructEltKind.DOCUMENT, children=fields)
    return (root,)


def _search_arm(payload: bytes, provider: LensProvider, spec: LensSpec) -> tuple[DocumentNode, ...]:
    needle = spec.needle
    if provider is LensProvider.MUPDF:
        with pymupdf.open(stream=payload, filetype="pdf") as document:
            return tuple(
                _node(NodeKind.ANNOTATION, "hit", index, needle.encode(), bounds=tuple(rect), annot=AnnotKind.HIGHLIGHT, contents=needle)
                for index, page in enumerate(document)
                for rect in page.search_for(needle)
            )
    with pdfplumber.open(BytesIO(payload), repair=spec.repair) as document:
        return tuple(
            _node(NodeKind.ANNOTATION, "hit", index, needle.encode(), bounds=(hit["x0"], hit["top"], hit["x1"], hit["bottom"]), annot=AnnotKind.HIGHLIGHT, contents=needle)
            for index, page in enumerate(document.pages)
            for hit in page.search(needle, regex=spec.regex, case=spec.case_sensitive)
        )


def _ocr_arm(payload: bytes, provider: LensProvider, spec: LensSpec) -> tuple[DocumentNode, ...]:
    if provider is LensProvider.MUPDF:
        with pymupdf.open(stream=payload, filetype=spec.filetype) as document:
            language = "+".join(spec.language)
            return tuple(
                _node(NodeKind.RUN, "ocr", index, word[4].encode(), bounds=(word[0], word[1], word[2], word[3]), text=word[4])
                for index, page in enumerate(document)
                for word in page.get_text("words", textpage=page.get_textpage_ocr(language=language, dpi=spec.dpi, full=spec.full))
            )
    with pymupdf.open(stream=payload, filetype=spec.filetype) as intake:  # deterministic close, never GC-reaped
        if not intake.is_pdf:  # Exemption: native single-image intake wraps a raster into a one-page PDF for the OCR feed
            with pymupdf.open() as canvas:
                page = canvas.new_page(width=intake[0].rect.width, height=intake[0].rect.height)
                page.insert_image(page.rect, stream=payload)
                payload = canvas.tobytes()
    with NamedTemporaryFile(suffix=".pdf") as source, NamedTemporaryFile(suffix=".pdf") as target, NamedTemporaryFile(suffix=".txt") as sidecar:
        source.write(payload)
        source.flush()
        code = ocrmypdf.ocr(
            source.name, target.name, sidecar=sidecar.name, language=spec.language, output_type=spec.output_type,
            mode=spec.ocr_mode, deskew=spec.deskew, clean=spec.clean, rotate_pages=spec.rotate_pages, optimize=spec.optimize, progress_bar=False,
        )
        text = sidecar.read().decode() if code is ocrmypdf.ExitCode.ok else ""
    return (
        _node(NodeKind.STRUCTURE, code.name, 0, code.name.encode(), elt=StructEltKind.NOTE),
        *(_node(NodeKind.RUN, "ocr", index, line.encode(), text=line) for index, line in enumerate(text.splitlines()) if line),
    )


def _embedded_arm(payload: bytes, _provider: LensProvider, _spec: LensSpec) -> tuple[DocumentNode, ...]:
    with pymupdf.open(stream=payload, filetype="pdf") as document:
        fields = tuple(
            _node(NodeKind.FIELD, "embedded", 0, document.embfile_get(name), field=FieldKind.TEXT, name=name, value=document.embfile_info(name)["filename"])
            for name in document.embfile_names()
        )
        figures = tuple(
            _node(NodeKind.FIGURE, "placed", index, pix.tobytes("png"), name=f"img-{xref}", intrinsic=(float(pix.width), float(pix.height)), media_type="image/png")
            for index, page in enumerate(document)
            for xref, *_ in page.get_images(full=True)
            if (pix := pymupdf.Pixmap(document, xref))
        )
    return fields + figures


def _widget_arm(payload: bytes, _provider: LensProvider, _spec: LensSpec) -> tuple[DocumentNode, ...]:
    with pymupdf.open(stream=payload, filetype="pdf") as document:
        return tuple(
            _node(NodeKind.FIELD, widget.field_name, index, str(widget.field_value).encode(), field=_widget_field(pymupdf, widget.field_type), name=widget.field_name, value=widget.field_value)
            for index, page in enumerate(document)
            for widget in page.widgets()
        )


def _annotate_arm(payload: bytes, _provider: LensProvider, _spec: LensSpec) -> tuple[DocumentNode, ...]:
    with pymupdf.open(stream=payload, filetype="pdf") as document:
        return tuple(
            _node(NodeKind.ANNOTATION, "annotation", index, annot.info["content"].encode(), bounds=tuple(annot.rect), annot=_annot_kind(annot.type[1]), contents=annot.info["content"])
            for index, page in enumerate(document)
            for annot in page.annots()
        )


def _widget_field(pymupdf: object, code: int) -> FieldKind:
    # SYMBOLIC_REFERENCE: the catalogued `PDF_WIDGET_TYPE_*` symbol names resolve to their int on the live module.
    return {getattr(pymupdf, name): kind for name, kind in _WIDGET_SYMBOL.items()}.get(code, FieldKind.TEXT)


def _annot_kind(name: str) -> AnnotKind:
    return _ANNOT_NAME.get(name, AnnotKind.NOTE)


def _ods_arm(payload: bytes, _provider: LensProvider, _spec: LensSpec) -> tuple[DocumentNode, ...]:
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
            _ORIGIN, index, role=sheet.getAttribute("name") or f"sheet-{index}",
        )
        for index, sheet in enumerate(document.getElementsByType(Table))
    )


def _odt_arm(payload: bytes, _provider: LensProvider, _spec: LensSpec) -> tuple[DocumentNode, ...]:
    document = load(BytesIO(payload))
    blocks = tuple(_odt_blocks(document.body.childNodes))
    return (_node(NodeKind.STRUCTURE, "document", 0, b"odt", elt=StructEltKind.DOCUMENT, children=blocks),)


def _odt_blocks(nodes: Iterable[object]) -> Iterator[DocumentNode]:
    for index, node in enumerate(nodes):
        probe = getattr(node, "isInstanceOf", None)  # `text:p`/`text:h` are Elements; a Text leaf carries no `isInstanceOf`
        if probe is None:
            continue
        if probe(H):
            text = extractText(node)
            yield _node(NodeKind.SECTION, text or "section", index, text.encode(), level=int(node.getAttribute("outlinelevel") or 1), heading=(_node(NodeKind.RUN, "heading", index, text.encode(), text=text),))
        elif probe(P):
            text = extractText(node)
            yield _node(NodeKind.BLOCK, "paragraph", index, text.encode(), block=BlockKind.PARAGRAPH, runs=(_node(NodeKind.RUN, "run", index, text.encode(), text=text),))
        else:
            yield from _odt_blocks(node.childNodes)


def _xlsx_arm(payload: bytes, _provider: LensProvider, spec: LensSpec) -> tuple[DocumentNode, ...]:
    workbook = python_calamine.CalamineWorkbook.from_object(BytesIO(payload), load_tables=spec.load_tables)
    sheets = spec.sheets or tuple(workbook.sheet_names)
    return tuple(
        _table_node(
            [[None if value is None else str(value) for value in row] for row in sheet.to_python(skip_empty_area=spec.skip_empty_area)],
            _ORIGIN, index, role=name, spans=_calamine_spans(sheet.merged_cell_ranges),
        )
        for index, name in enumerate(sheets)
        if (sheet := workbook.get_sheet_by_name(name))
    )


def _calamine_spans(ranges: object) -> Spans:
    return tuple(
        (r0, c0, c1 - c0 + 1, r1 - r0 + 1)
        for (r0, c0), (r1, c1) in (ranges or ())
        if c1 > c0 or r1 > r0
    )


def _docx_arm(payload: bytes, _provider: LensProvider, _spec: LensSpec) -> tuple[DocumentNode, ...]:
    document = docx.Document(BytesIO(payload))
    blocks = tuple(_docx_blocks(enumerate(document.iter_inner_content())))
    props = document.core_properties
    return (_node(NodeKind.STRUCTURE, props.title or "document", 0, (props.author or "").encode(), elt=StructEltKind.DOCUMENT, children=blocks),)


def _docx_blocks(indexed: Iterable[tuple[int, object]]) -> Iterator[DocumentNode]:
    for list_kind, group in groupby(indexed, key=lambda pair: _docx_list_kind(pair[1])):
        if list_kind is None:
            yield from (_docx_block(block, index) for index, block in group)
        else:
            rows = tuple(group)
            members = tuple(_node(NodeKind.BLOCK, "item", index, block.text.encode(), block=BlockKind.PARAGRAPH, runs=_docx_runs(block, index)) for index, block in rows)
            yield _node(NodeKind.LIST, list_kind.value, rows[0][0], list_kind.value.encode(), list_kind=list_kind, items=members)


def _docx_list_kind(block: object) -> ListKind | None:
    return _DOCX_LIST.get(getattr(getattr(block, "style", None), "name", ""))


def _docx_block(block: object, index: int) -> DocumentNode:
    if hasattr(block, "rows"):
        return _table_node([[cell.text for cell in row.cells] for row in block.rows], _ORIGIN, index)
    style = getattr(block.style, "name", "")
    runs = _docx_runs(block, index)
    level = _DOCX_HEADING.get(style)
    if level is not None:
        return _node(NodeKind.SECTION, block.text, index, block.text.encode(), level=level, heading=runs)
    return _node(NodeKind.BLOCK, "paragraph", index, block.text.encode(), block=_DOCX_BLOCK.get(style, BlockKind.PARAGRAPH), runs=runs)


def _docx_runs(block: object, index: int) -> tuple[DocumentNode, ...]:
    return tuple(
        _node(
            NodeKind.RUN, "run", index, run.text.encode(), text=run.text,
            font_key=run.font.name or (run.style.name if run.style else _RECOVERED_FONT),  # emit writes `font.name`; read its inverse
            size=run.font.size.pt if run.font.size else 0.0, weight=700 if run.bold else 400, italic=bool(run.italic),
            decorations=(TextDecoration.UNDERLINE,) if run.font.underline else (),
            color=tuple(rgb) if (rgb := run.font.color.rgb) else (0, 0, 0),
        )
        for run in block.runs
    )


def _yaml_arm(payload: bytes, _provider: LensProvider, spec: LensSpec) -> tuple[DocumentNode, ...]:
    engine = YAML(typ=spec.typ)
    return tuple(_value_node(document, "yaml", index) for index, document in enumerate(engine.load_all(BytesIO(payload))))


def _toml_arm(payload: bytes, _provider: LensProvider, _spec: LensSpec) -> tuple[DocumentNode, ...]:
    return (_value_node(tomlkit.parse(payload).unwrap(), "toml", 0),)


def _value_node(value: object, role: str, page: int, *, key: str = "") -> DocumentNode:
    match value:
        case Mapping():
            return _node(NodeKind.BLOCK, key or role, page, repr(value).encode(), block=BlockKind.PARAGRAPH, children=tuple(_value_node(child, role, page, key=str(name)) for name, child in value.items()))
        case list() | tuple():
            return _node(NodeKind.LIST, key or role, page, repr(value).encode(), list_kind=ListKind.ORDERED, items=tuple(_value_node(child, role, page, key=str(ordinal)) for ordinal, child in enumerate(value)))
        case _:
            text = "" if value is None else str(value)
            return _node(NodeKind.RUN, key or role, page, text.encode(), text=text)


def _xml_arm(payload: bytes, _provider: LensProvider, spec: LensSpec) -> tuple[DocumentNode, ...]:
    parser = etree.XMLParser(recover=spec.recover, resolve_entities=False, no_network=True, huge_tree=False)
    return (_element_node(etree.fromstring(payload, parser=parser), 0),)


def _element_node(element: object, page: int) -> DocumentNode:
    tag = etree.QName(element).localname if isinstance(element.tag, str) else "comment"
    text = (element.text or "").strip()
    runs = (_node(NodeKind.RUN, tag, page, text.encode(), text=text),) if text else ()
    children = runs + tuple(_element_node(child, page) for child in element if isinstance(child.tag, str))
    return _node(NodeKind.BLOCK, tag, page, etree.tostring(element).strip(), block=BlockKind.PARAGRAPH, level=1, children=children)


def _gated_recover(lens: "DocumentLens") -> tuple[DocumentNode, ...]:
    arm, default = _ROUTES[lens.op]
    return arm(lens.payload, lens.provider or default, lens.spec)


# --- [TABLES] ---------------------------------------------------------------------------

_DOCX_HEADING: Final[frozendict[str, int]] = frozendict({"Title": 1, "Heading 1": 1, "Heading 2": 2, "Heading 3": 3, "Heading 4": 4, "Heading 5": 5, "Heading 6": 6})
_DOCX_LIST: Final[frozendict[str, ListKind]] = frozendict({"List Bullet": ListKind.UNORDERED, "List Number": ListKind.ORDERED})
_DOCX_BLOCK: Final[frozendict[str, BlockKind]] = frozendict({"Quote": BlockKind.QUOTE, "Intense Quote": BlockKind.QUOTE, "Caption": BlockKind.CAPTION})
_META_KEY: Final[frozendict[str, str]] = frozendict({
    "/Title": "title", "/Author": "author", "/Subject": "subject", "/Keywords": "keywords",
    "/Creator": "creator", "/Producer": "producer", "/CreationDate": "created", "/ModDate": "modified",
})
_WIDGET_SYMBOL: Final[frozendict[str, FieldKind]] = frozendict({
    "PDF_WIDGET_TYPE_TEXT": FieldKind.TEXT,
    "PDF_WIDGET_TYPE_CHECKBOX": FieldKind.CHECKBOX,
    "PDF_WIDGET_TYPE_RADIOBUTTON": FieldKind.CHECKBOX,
    "PDF_WIDGET_TYPE_LISTBOX": FieldKind.CHOICE,
    "PDF_WIDGET_TYPE_COMBOBOX": FieldKind.CHOICE,
    "PDF_WIDGET_TYPE_BUTTON": FieldKind.BUTTON,
    "PDF_WIDGET_TYPE_SIGNATURE": FieldKind.SIGNATURE,
})
_ANNOT_NAME: Final[frozendict[str, AnnotKind]] = frozendict({
    "Highlight": AnnotKind.HIGHLIGHT, "Squiggly": AnnotKind.HIGHLIGHT, "Underline": AnnotKind.HIGHLIGHT, "StrikeOut": AnnotKind.HIGHLIGHT,
    "Redact": AnnotKind.REDACTION, "Link": AnnotKind.LINK, "Text": AnnotKind.NOTE, "FreeText": AnnotKind.NOTE, "Stamp": AnnotKind.STAMP,
})
_ROUTES: Final[frozendict[LensOp, tuple[RecoverArm, LensProvider]]] = frozendict({
    LensOp.EXTRACT_TEXT: (_text_arm, LensProvider.PYPDF),
    LensOp.EXTRACT_IMAGES: (_images_arm, LensProvider.PYPDF),
    LensOp.TABLE: (_table_arm, LensProvider.PLUMBER),
    LensOp.WORDS: (_words_arm, LensProvider.PLUMBER),
    LensOp.REGION: (_region_arm, LensProvider.PLUMBER),
    LensOp.OUTLINE: (_outline_arm, LensProvider.MUPDF),
    LensOp.STRUCTURE: (_structure_arm, LensProvider.PLUMBER),
    LensOp.LINK: (_link_arm, LensProvider.MUPDF),
    LensOp.METADATA: (_metadata_arm, LensProvider.PYPDF),
    LensOp.SEARCH: (_search_arm, LensProvider.PLUMBER),
    LensOp.OCR: (_ocr_arm, LensProvider.OCRMYPDF),
    LensOp.EMBEDDED: (_embedded_arm, LensProvider.MUPDF),
    LensOp.WIDGET: (_widget_arm, LensProvider.MUPDF),
    LensOp.ANNOTATE: (_annotate_arm, LensProvider.MUPDF),
    LensOp.XLSX_READ: (_xlsx_arm, LensProvider.CALAMINE),
    LensOp.ODS_READ: (_ods_arm, LensProvider.ODFPY),
    LensOp.ODT_READ: (_odt_arm, LensProvider.ODFPY),
    LensOp.DOCX_READ: (_docx_arm, LensProvider.DOCX),
    LensOp.YAML_READ: (_yaml_arm, LensProvider.RUAMEL),
    LensOp.TOML_READ: (_toml_arm, LensProvider.TOMLKIT),
    LensOp.XML_READ: (_xml_arm, LensProvider.LXML),
})
```

## [03]-[RESEARCH]

- [WIDGET_ANNOT_DISCRIMINANTS_RESOLVED]: the prior int-discriminant RESEARCH deferral is closed against the verified catalogue surface. `.api/pymupdf.md` `[02]` row `[04]` carries `Annot.type -> (int, str)` where the second element is the PDF annotation-subtype name string, and `[07]` enumerates the `PDF_ANNOT_*` member family, so `_annotate_arm` reads `annot.type[1]` and `_annot_kind` maps the verified name strings (`Highlight`/`Redact`/`Link`/`Text`/`FreeText`/`Stamp`/`Squiggly`/`Underline`/`StrikeOut`) to `AnnotKind` through `_ANNOT_NAME` — no unverified integer key. `Widget.field_type -> int` (`[02]` accessor row `[09]`) is the `PDF_WIDGET_TYPE_*` discriminant whose member NAMES (`TEXT`/`CHECKBOX`/`RADIOBUTTON`/`LISTBOX`/`COMBOBOX`/`BUTTON`/`SIGNATURE`, `[02]` row `[06]`) are catalogued, so `_widget_field` resolves each symbol name to its int via `getattr(pymupdf, name)` on the live module and maps it to `FieldKind` through `_WIDGET_SYMBOL` — the SYMBOLIC_REFERENCE form that asserts no hardcoded integer. The empty `_WIDGET_FIELD`/`_ANNOT_KIND` int dicts and the undefined `_WIDGET_SYMBOL`/`_ANNOT_SYMBOL` references the prior page carried are both the deleted form: the tables are now defined, named consistently, and keyed on the verified string/symbol surface.
- [RECEIPT_CONTRACT_AND_LIST_OWNER]: two corrections align the page with its owners. (1) `observability/receipts#RECEIPT` types `@receipted[**P, R: ReceiptContributor]`, so the wrapped `_emit` must return a `ReceiptContributor`; the prior `_emit` returning a bare `ContentKey` violated the bound and the prior `contribute` re-ran `self._recovered()` (re-importing a worker arm on the core during the harvest). `_emit` now returns the stepped `Self` carrying `recovered` (a `ReceiptContributor` through `contribute`), `recover` maps it to the `ContentKey`, and `contribute` reads the stepped `recovered` — the exact `document/emit#DOCUMENT` `_emit -> Self` / `contribute`-off-`self.fact` pattern, never an in-process re-run. `contribute(self)` carries no `phase` parameter — the phase is the constant `"emitted"` the `core/receipt#RECEIPT` owner fixes by construction (KNOB_TEST), so the signature is exactly the `ReceiptContributor.contribute(self)` port emit composes, never a `contribute(phase)` knob the value already answers. (2) `document/model#NODE` `BlockKind` is `PARAGRAPH`/`HEADING`/`QUOTE`/`CODE`/`CAPTION`/`ARTIFACT` with NO `LIST_ITEM` member, and the list concept is the distinct `ListNode`/`ListKind` owner; the prior page's `_DOCX_BLOCK` and `_value_node` referencing `BlockKind.LIST_ITEM` named a member the model retired (a phantom), and `_node` was non-total over `NodeKind` (no `LIST` arm). `_node` now carries the `NodeKind.LIST` arm, `_docx_blocks` groups consecutive `_DOCX_LIST`-styled paragraphs through `itertools.groupby` into one `ListNode` (the inverse of the emit `List Bullet`/`List Number` lowering), and `_value_node` folds a YAML/TOML sequence into a `ListNode(ORDERED)` and a mapping into a keyed `BlockNode` — zero phantom `BlockKind` reference.
- [TYPED_SPEC_AND_WIRED_OPS]: the per-op input is admitted once as the typed `LensSpec` through the closed `LensPayload` `TypedDict` + `_PAYLOAD` `TypeAdapter` at `.of`, mirroring `document/emit#DOCUMENT` `EmitSpec`/`EmitPayload`, so every arm reads a typed field (`spec.x_tolerance`, `spec.vertical`, `spec.needle`, `spec.language`, `spec.bbox`) — the prior `params: dict[str, object]` bag forwarded into every arm and re-read by `params.get(...)` is the deleted `Mapping[str, object]` payload the shape doctrine rejects. The `STRUCTURE`/`LINK`/`METADATA` ops the prior page declared in `LensOp` and gave arm functions but never wired into `_ARMS`/`_DEFAULT_PROVIDER` (orphans that would `KeyError` on dispatch) are now full `_ROUTES` rows: `STRUCTURE` recovers the tagged-PDF tree through pdfplumber `Page.structure_tree` (core) or pikepdf `Pdf.Root[/StructTreeRoot]` (core, `.api/pikepdf.md` object model), `LINK` recovers hyperlinks through pymupdf `Page.get_links` (`.api/pymupdf.md` `[03]` row `[11]`) or pdfplumber `Page.hyperlinks` populating the model's `AnnotTarget` `link`, and `METADATA` recovers the info dict through pypdf `PdfReader.metadata`. The two parallel `_ARMS`/`_DEFAULT_PROVIDER` op-keyed tables collapse into the one `_ROUTES` `(arm, default_provider)` correspondence per `DERIVED_LOGIC`.
- [NODE_FIELD_COVERAGE]: every recovered node carries the full `document/model#NODE` field contract over its one polymorphic `_node` constructor, each field citing a verified member, and the constructor mints a model-legal `RunNode` — the prior `RunNode(rtl=...)` keyword named a field the model retired (`direction: TextDirection` replaced `rtl: bool`), so it would `TypeError` at construction and is the deleted form. The pymupdf `_text_arm` recovers `italic`/`script`/`color` from the span `flags`/`color` (`.api/pymupdf.md` `get_text("dict")` span dict) and `direction` from the line `dir` writing-direction unit vector (`dir[0] < 0` → `TextDirection.RTL`) where the prior arm recovered only `weight`; `decorations` defaults `()` because the MuPDF span dict exposes no underline/strike bit, asserting one would be a phantom. A `TableNode` recovers `header_rows` from `Table.header` (`.api/pymupdf.md` `[03]` row `[19]`) beside the `spans` it already folded; a `FigureNode` recovers `intrinsic` and `media_type` from the `Pixmap(document, xref)` `width`/`height` (`.api/pymupdf.md` `[03]` row `[09]`); an `AnnotationNode` of kind `LINK` recovers the `AnnotTarget` `Uri(href)`/`Dest(page)` so a hyperlink round-trips through the model's `#link` lowering rather than stranding its href in `contents`. The `_docx_runs` `RunNode` recovers `font_key` from python-docx `Run.font.name` (the read inverse of the emit `_docx_run` `font.name` write — the prior `run.style.name` read named the character-style, not the font, and did not round-trip), `size` from `Run.font.size.pt`, `weight`/`italic` from `Run.bold`/`Run.italic`, `color` from `Run.font.color.rgb`, and `decorations` from `Run.font.underline` (`.api/python-docx.md` `[03]` row `[10]` `Run.font`). `FieldNode.flags`/`options` stay at their model defaults: the catalogue exposes `Widget.field_name`/`field_value`/`field_type` but no flags/choice accessor, so asserting one would be a phantom — the field is anticipatorily present on the `_node` `FIELD` arm for the catalogue deepening that lands the accessor, never fabricated here.
- [CORPUS_LANE_RESIDENCY]: the recovered-tree corpus keys a `(ContentKey, DocumentNode-tree)` value into the runtime columnar lane via the `data:tabular/columnar#SCAN` `Corpus` arm `[WIRE]` seam, lifting the `document/model#NODE` `to_corpus(node, CorpusView.RECORD)` flat-`dict` projection (the `msgspec.to_builtins` lowering the `Corpus` arm's `pa.Table.from_pylist` ingests) so a multi-PDF/multi-workbook corpus is one queryable value over the same `DocumentNode` tree emission lowers from. The lane is consumed from `data`, never re-minted here; the artifacts side is the content-keyed producer of the extracted-tree value, never a second store. The settled core spellings verify against `.api/pypdf.md`, `.api/pdfplumber.md` `[03]`, `.api/odfpy.md` `[03]`, `.api/python-docx.md` `[03]`, and the native `pymupdf`/`pikepdf` spellings against `.api/pymupdf.md` `[03]`/`.api/pikepdf.md` `[03]`; the gated `ocrmypdf` `ocr(..., sidecar=, mode=, output_type=, language=, deskew=, clean=, rotate_pages=, optimize=)` returning `ExitCode` plus the `ExitCode.ok`/`ExitCodeException` rail verify against `.api/ocrmypdf.md` `[02]`/`[03]`, and the `python-calamine`/`lxml` gated spellings against their `[03]` rows.

# [PY_ARTIFACTS_LENS]

The recover-TO half of the bidirectional `document` seam: `DocumentLens` recovers a `DocumentNode` tree back OUT of an emitted PDF, a scanned raster, or an office/structured-text payload — one owner discriminating recovery operation over the closed `LensOp`, routed by the one `_ROUTES` `(arm, default_provider)` table, never a `get_text`/`get_words` verb family. Production and extraction are inverses over the one node algebra, so a `document/emit#DOCUMENT` emission and a `DocumentLens` recovery round-trip through `DocumentNode` with `document/model#DELTA` defined once.

The layout-dominant ops default to `LensProvider.PDFOXIDE` — the MIT/Apache Rust-core engine carrying reading-order XY-cut recovery, region crop, in-process OCR, and `FormField`-flag AcroForm recovery — the commercial-safe path where the AGPL `pymupdf` arms are barred on a closed-distributed pipeline, `pymupdf` retained for the permissive/internal lane. Each provider carries its own runtime `band`: the native and pure-Python readers resolve in-process on the core, and only the no-runtime-package companions (`ocrmypdf`, `python-calamine`, `lxml`) cross `Modality.PROCESS`; every reader arm is a `@beartype` boundary narrowing the recovered provider shapes to the model-legal `DocumentNode` before crossing back. `LensSpec` admits exactly once at `.of` under the per-op `_REQUIRED` precondition; the `@receipted(OPEN)` weave drains the stepped owner and `contribute` mints the `core/receipt#RECEIPT` `ArtifactReceipt.Introspection` case.

## [01]-[INDEX]

- [01]-[LENS]: the one recovery owner discriminating `LensOp` over the `_ROUTES` `(arm, default_provider)` table under provider-keyed bands.

## [02]-[LENS]

- Owner: `DocumentLens` — the ops dispatch a provider VALUE rather than reconstructing the engine choice, the band recovered once from `provider.band`. One polymorphic `_node` constructor mints every node variant over a `NodeMeta` whose content key joins the structural discriminant (positional `bounds`, else a sibling ordinal prefix) to the content, so identical-content siblings never collapse onto one slot; the per-kind material admits through the closed `NodeSlot` `TypedDict`, honoring each variant's real field contract — never a per-kind sibling-factory family.
- Cases: the non-obvious per-arm rules the fence cannot self-justify — TABLE reads the MuPDF `Table.header.external` discriminant into `header_rows` (an in-grid header row `1`, an above-body synthesized header `0`, never the always-truthy `Table.header` object) and folds the pdfplumber `Table.cells` bbox set into merged-cell `spans` quads; REGION converts the model `(x0,y0,x1,y1)` bbox once to the pdf_oxide `(x, y, w, h)` convention; OCR defaults to the in-process pdf_oxide Rust engine (no subprocess hop, no PDF/A rewrite), the gated `ocrmypdf` alternate RESERVED for the PDF/A output path with its `ExitCode` return gating the sidecar text feed, and a non-PDF raster wraps losslessly through `new_page`/`insert_image` before OCR; WIDGET folds the pdf_oxide `FormField.is_required`/`is_readonly` into `FieldNode.flags` — the field-flag gap the pymupdf widget accessor cannot fill — while the pymupdf alternate resolves `field_type` ints via `getattr` over the catalogued `PDF_WIDGET_TYPE_*` names; DOCX_READ groups consecutive list-styled paragraphs through `groupby` into one `ListNode`, the inverse of the emit `List Bullet`/`List Number` lowering; YAML_READ rides `load_all` with the single-document case subsumed, never a `multi` knob; XML_READ runs the hardened parser (`resolve_entities=False`, `no_network=True`, `huge_tree=False`) and ONLY the serialized `DocumentNode` tree crosses back across the interpreter seam.
- Entry: the key mints PRE-RUN over `(op, payload, provider, spec)` — the recovered-tree `node_digest` merkle rides the receipt FACTS, never the elision key, and never a second `msgspec.msgpack` encoder beside the model's canonical codec. A GATED row crosses onto the worker that re-resolves the SAME `_ROUTES` row and reifies the module-scope `lazy` bindings there, so the worker lane carries no second dispatch.
- Receipt: `contribute` reads the stepped `recovered` directly and never re-runs `_emit`, so a worker-gated arm is never re-imported on the core during the receipt harvest; the `Introspection` counts project by `walk` over the node variants, the tag riding the encoded `kind` field, never a runtime `.tag` attribute.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
from collections.abc import Callable, Iterable, Iterator, Mapping
from copy import replace
from enum import StrEnum
from io import BytesIO
from itertools import groupby
from typing import Final, Literal, NotRequired, ReadOnly, Self, TypedDict, Unpack, assert_never

from beartype import beartype
from expression import Error, Ok, Result, case, tag, tagged_union
from expression.collections import Map
from msgspec import Struct, field
from pydantic import TypeAdapter, ValidationError

from artifacts.core.plan import Admission, ArtifactWork
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
from rasm.runtime.identity import CANONICAL_POLICY, ContentIdentity, ContentKey
from rasm.runtime.faults import RuntimeRail, async_boundary
from rasm.runtime.lanes import LanePolicy, Modality
from rasm.runtime.resilience import RetryClass
from rasm.runtime.receipts import OPEN, Receipt, receipted

lazy import docx
lazy import ocrmypdf
lazy import pdf_oxide
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
    STORY = "story"  # per-page tree recovery: the recover-TO inverse of `document/report#REPORT REFLOW`
    PATHS = "paths"  # recovered vector geometry (fills/strokes/curves) for the AEC drawing plane
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
    PDFOXIDE = "pdf-oxide"  # MIT/Apache Rust core, ungated CORE — the commercial-safe layout-aware default
    PYPDF = "pypdf"
    PLUMBER = "pdfplumber"
    MUPDF = "pymupdf"  # AGPL-3.0 — reserved for permissive/internal lanes; PDFOXIDE supersedes it on the closed/distributed path
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

_ORIGIN: Final[Bounds] = (0.0, 0.0, 0.0, 0.0)
_RECOVERED_FONT: Final[str] = "recovered"
_VECTOR_MEDIA: Final[str] = "image/svg+xml"  # recovered vector-path FigureNode media type (the PATHS drawing-geometry marker)
_HEADING_FLOOR: Final[int] = 1
_HEADING_CEIL: Final[int] = 6
_BOLD_FLAG: Final[int] = 16  # pymupdf span flag bit 4 — bold
_ITALIC_FLAG: Final[int] = 2  # pymupdf span flag bit 1 — italic
_SUPER_FLAG: Final[int] = 1  # pymupdf span flag bit 0 — superscript
_GATED_PROVIDERS: Final[frozenset[LensProvider]] = frozenset({LensProvider.OCRMYPDF, LensProvider.CALAMINE, LensProvider.LXML})
_HEADING: Final[tuple[StructEltKind, ...]] = (
    StructEltKind.H1,
    StructEltKind.H2,
    StructEltKind.H3,
    StructEltKind.H4,
    StructEltKind.H5,
    StructEltKind.H6,
)

# --- [ERRORS] ---------------------------------------------------------------------------


@tagged_union(frozen=True)
class LensFault:
    # the closed ADMISSION vocabulary `of` produces; every arm-level provider raise
    # (`pymupdf.FileDataError`/`pdfplumber.PdfminerException`/`python_calamine.CalamineError`/
    # `ocrmypdf.ExitCodeException`/`lxml.etree.XMLSyntaxError`) converts to the runtime `BoundaryFault`
    # at the `async_boundary` capsule, never into this interior vocabulary.
    tag: Literal["payload", "unsatisfied"] = tag()
    payload: tuple[str, ...] = case()  # the rejected LensPayload key paths
    unsatisfied: tuple[LensOp, str] = case()  # an op whose `_REQUIRED` input field is empty


# --- [MODELS] ---------------------------------------------------------------------------


class LensSpec(Struct, frozen=True, omit_defaults=True):
    mode: str = "plain"  # pypdf `extract_text(extraction_mode=)`
    reading_order: str = "column_aware"  # pdf_oxide `extract_spans(reading_order=)` XY-cut column detection ("top_to_bottom" | "column_aware")
    include_artifacts: bool = False  # pdf_oxide `extract_words(include_artifacts=)`; False drops running header/footer/watermark spans
    profile: str = ""  # pdf_oxide `extract_words(profile=)` layout-tuning profile — an `ExtractionProfile.available()` name (academic/form/government/scanned_ocr/…); "" keeps the adaptive default heuristics
    flags: int = 0  # pymupdf `get_text("dict", flags=)`; 0 -> TEXTFLAGS_DICT
    x_tolerance: float = 3.0
    y_tolerance: float = 3.0
    use_text_flow: bool = False
    split_at_punctuation: bool = False
    extra_attrs: tuple[str, ...] = ("fontname", "size")
    vertical: str = "lines"  # pdfplumber/pymupdf `vertical_strategy`
    horizontal: str = "lines"
    snap_tolerance: float = 3.0
    join_tolerance: float = 3.0
    edge_min_length: float = 3.0
    intersection_tolerance: float = 3.0
    text_tolerance: float = 3.0
    explicit_vertical: tuple[float, ...] = ()
    explicit_horizontal: tuple[float, ...] = ()
    bbox: Bounds | None = None  # REGION crop window
    repair: bool = False  # pdfplumber Ghostscript pre-repair
    needle: str = ""  # SEARCH pattern
    regex: bool = True
    case_sensitive: bool = True
    language: tuple[str, ...] = ("eng",)  # OCR Tesseract language packs
    dpi: int = 72
    full: bool = False
    filetype: str = "pdf"
    output_type: str = "pdfa"  # ocrmypdf PDF/A target
    ocr_mode: str = "force"  # ocrmypdf processing mode
    deskew: bool = False
    clean: bool = False
    rotate_pages: bool = False
    optimize: int = 1
    load_tables: bool = False  # python-calamine Excel-table parse
    sheets: tuple[str, ...] = ()
    skip_empty_area: bool = True
    typ: str = "rt"  # ruamel-yaml round-trip loader
    recover: bool = True  # lxml recovering parser


class DocumentLens(Struct, frozen=True):
    op: LensOp
    payload: bytes
    spec: LensSpec = field(default_factory=LensSpec)
    provider: LensProvider | None = None
    recovered: tuple[DocumentNode, ...] = ()

    def emit(self, /) -> ArtifactWork:
        return ArtifactWork(key=self._key, work=self._emit, parents=(), admission=Admission(keyed=None), cost=float(len(self.payload)))

    @property
    def _key(self) -> ContentKey:
        return ContentIdentity.of(f"lens-{self.op.value}", (self.op, self.payload, self.provider, self.spec), policy=CANONICAL_POLICY)

    @receipted(OPEN)  # lens facts carry no classified field, so the runtime keep-all `OPEN` policy rides directly, never a re-minted per-file `Redaction`
    async def _recovered(self) -> Self:
        arm, default = _ROUTES[self.op]
        provider = self.provider or default
        crossed = (
            await LanePolicy.offload(_gated_recover, self, modality=Modality.PROCESS, retry=RetryClass.OCCT)
            if provider.band is LensBand.GATED
            else await LanePolicy.offload(arm, self.payload, provider, self.spec, modality=Modality.THREAD)
        )
        return replace(self, recovered=crossed.default_with(lambda fault: _lens_raise(fault)))

    async def _emit(self) -> RuntimeRail[ArtifactReceipt]:
        # the terminal receipt threads the PRE-RUN input key so receipt.slot == node.key.
        railed = await async_boundary(f"lens.{self.op.value}", self._recovered)
        return railed.map(lambda stepped: stepped._receipt(self._key))

    def _receipt(self, key: ContentKey, /) -> ArtifactReceipt:
        flat = tuple(child for node in self.recovered for child in walk(node))
        return ArtifactReceipt.Introspection(
            key,
            len(flat),
            sum(len(node.text) for node in flat if isinstance(node, RunNode)),
            sum(isinstance(node, FigureNode) for node in flat),
            sum(isinstance(node, AnnotationNode) for node in flat),
        )

    def contribute(self) -> Iterable[Receipt]:
        yield from self._receipt(self._key).contribute()

    @classmethod
    def of(cls, op: LensOp, payload: bytes, /, *, provider: LensProvider | None = None, **raw: Unpack[LensPayload]) -> Result[Self, LensFault]:
        try:
            admitted = _PAYLOAD.validate_python(raw)
        except ValidationError as fault:
            return Error(LensFault(payload=tuple(str(error["loc"]) for error in fault.errors())))
        spec = LensSpec(**admitted)
        missing = next((name for name in _REQUIRED.try_find(op).default_value(()) if not getattr(spec, name)), None)
        return Error(LensFault(unsatisfied=(op, missing))) if missing else Ok(cls(op=op, payload=payload, spec=spec, provider=provider))


# --- [BOUNDARIES] -----------------------------------------------------------------------


class LensPayload(TypedDict, closed=True):
    mode: NotRequired[ReadOnly[str]]
    reading_order: NotRequired[ReadOnly[str]]
    include_artifacts: NotRequired[ReadOnly[bool]]
    profile: NotRequired[ReadOnly[str]]
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
_REQUIRED: Final[Map[LensOp, tuple[str, ...]]] = Map.of_seq([(LensOp.REGION, ("bbox",)), (LensOp.SEARCH, ("needle",))])


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
    seed = repr(bounds).encode() if bounds is not None else f"#{slot.get('ordinal', 0)}".encode()
    meta = NodeMeta(key=ContentIdentity.of(f"node-{role}-{page}", seed + payload), role=role, page=page, bounds=bounds)
    match kind:
        case NodeKind.RUN:
            return RunNode(
                meta=meta,
                text=slot.get("text", ""),
                font_key=slot.get("font_key", _RECOVERED_FONT),
                size=slot.get("size", 0.0),
                weight=slot.get("weight", 400),
                italic=slot.get("italic", False),
                direction=slot.get("direction", TextDirection.AUTO),
                script=slot.get("script", RunScript.NORMAL),
                decorations=slot.get("decorations", ()),
                color=slot.get("color", (0, 0, 0)),
            )
        case NodeKind.FIGURE:
            return FigureNode(
                meta=meta,
                asset_key=ContentIdentity.of(f"asset-{slot['name']}", payload),
                alt=slot.get("alt", ""),
                media_type=slot.get("media_type", "image/png"),
                intrinsic=slot.get("intrinsic"),
                caption=slot.get("caption", ()),
            )
        case NodeKind.TABLE:
            return TableNode(meta=meta, rows=slot.get("rows", ()), spans=slot.get("spans", ()), header_rows=slot.get("header_rows", 0))
        case NodeKind.FIELD:
            return FieldNode(
                meta=meta,
                name=slot["name"],
                field=slot.get("field", FieldKind.TEXT),
                value=slot.get("value"),
                flags=slot.get("flags", ()),
                options=slot.get("options", ()),
            )
        case NodeKind.ANNOTATION:
            return AnnotationNode(
                meta=meta,
                annot=slot.get("annot", AnnotKind.NOTE),
                target=bounds or _ORIGIN,
                contents=slot.get("contents", ""),
                link=slot.get("link", NoTarget()),
            )
        case NodeKind.STRUCTURE:
            return StructureNode(
                meta=meta, role=slot.get("struct_role") or StandardRole(elt=slot.get("elt", StructEltKind.SECT)), children=slot.get("children", ())
            )
        case NodeKind.SECTION:
            return SectionNode(meta=meta, level=slot.get("level", 1), heading=slot.get("heading", ()), children=slot.get("children", ()))
        case NodeKind.BLOCK:
            return BlockNode(
                meta=meta,
                block=slot.get("block", BlockKind.PARAGRAPH),
                level=slot.get("level", 1),
                runs=slot.get("runs", ()),
                children=slot.get("children", ()),
            )
        case NodeKind.LIST:
            return ListNode(meta=meta, list_kind=slot.get("list_kind", ListKind.UNORDERED), items=slot.get("items", ()))
        case NodeKind.PAGE:
            return PageNode(meta=meta, media_box=bounds or _ORIGIN, children=slot.get("children", ()))
        case _ as unreachable:
            assert_never(unreachable)


def _rgb(color: int) -> tuple[int, int, int]:
    return ((color >> 16) & 0xFF, (color >> 8) & 0xFF, color & 0xFF)


def _scale8(color: tuple[float, float, float]) -> tuple[int, int, int]:
    # pdf_oxide `TextSpan.color`/`PdfAnnotation.color` are 0..1 floats; `RunNode.color` is the model `Rgb` 0..255.
    return (round(color[0] * 255), round(color[1] * 255), round(color[2] * 255))


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


@beartype
def _text_arm(payload: bytes, provider: LensProvider, spec: LensSpec) -> tuple[DocumentNode, ...]:
    if provider is LensProvider.PDFOXIDE:  # commercial-safe default: XY-cut column-order styled spans -> per-page BlockNode of RunNode
        with pdf_oxide.PdfDocument.from_bytes(payload) as document:
            return tuple(
                _node(
                    NodeKind.BLOCK,
                    "block",
                    page.index,
                    page.text.encode(),
                    bounds=page.bbox,
                    block=BlockKind.PARAGRAPH,
                    runs=tuple(
                        _node(
                            NodeKind.RUN,
                            "span",
                            page.index,
                            span.text.encode(),
                            bounds=span.bbox,
                            text=span.text,
                            font_key=span.font_name or _RECOVERED_FONT,
                            size=span.font_size,
                            weight=700 if span.is_bold else 400,
                            italic=span.is_italic,
                            color=_scale8(span.color),
                        )
                        for span in document.extract_spans(page.index, reading_order=spec.reading_order)
                    ),
                )
                for page in document.pages
            )
    if provider is LensProvider.MUPDF:
        with pymupdf.open(stream=payload, filetype="pdf") as document:
            return tuple(
                _node(
                    NodeKind.BLOCK,
                    "block",
                    index,
                    (block["lines"][0]["spans"][0]["text"].encode() if block.get("lines") and block["lines"][0].get("spans") else b""),
                    bounds=tuple(block["bbox"]),
                    block=BlockKind.PARAGRAPH,
                    runs=tuple(
                        _node(
                            NodeKind.RUN,
                            "span",
                            index,
                            span["text"].encode(),
                            bounds=tuple(span["bbox"]),
                            text=span["text"],
                            font_key=span.get("font", _RECOVERED_FONT),
                            size=float(span.get("size", 0.0)),
                            weight=700 if int(span.get("flags", 0)) & _BOLD_FLAG else 400,
                            italic=bool(int(span.get("flags", 0)) & _ITALIC_FLAG),
                            direction=TextDirection.RTL if line.get("dir", (1.0, 0.0))[0] < 0 else TextDirection.LTR,
                            script=RunScript.SUPER if int(span.get("flags", 0)) & _SUPER_FLAG else RunScript.NORMAL,
                            color=_rgb(int(span.get("color", 0))),
                        )
                        for line in block.get("lines", ())
                        for span in line.get("spans", ())
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


@beartype
def _images_arm(payload: bytes, _provider: LensProvider, _spec: LensSpec) -> tuple[DocumentNode, ...]:
    reader = pypdf.PdfReader(BytesIO(payload))
    return tuple(
        _node(NodeKind.FIGURE, "figure", index, image.data, name=image.name) for index, page in enumerate(reader.pages) for image in page.images
    )


@beartype
def _words_arm(payload: bytes, provider: LensProvider, spec: LensSpec) -> tuple[DocumentNode, ...]:
    if provider is LensProvider.PDFOXIDE:  # commercial-safe default: TextWord geometry -> word RunNode leaves, artifact-tagged spans dropped
        # the per-document-class layout profile tunes the XY-cut word-margin/space heuristics: an
        # `ExtractionProfile.available()` name resolves once on the live module (SYMBOLIC_REFERENCE, as `_widget_field`
        # resolves `PDF_WIDGET_TYPE_*`), "" keeping pdf_oxide's adaptive default rather than a hand-rolled gap table.
        profile = getattr(pdf_oxide.ExtractionProfile, spec.profile)() if spec.profile else None
        with pdf_oxide.PdfDocument.from_bytes(payload) as document:
            return tuple(
                _node(
                    NodeKind.RUN,
                    "word",
                    page.index,
                    word.text.encode(),
                    bounds=word.bbox,
                    text=word.text,
                    font_key=word.font_name or _RECOVERED_FONT,
                    size=word.font_size,
                    weight=700 if word.is_bold else 400,
                    italic=word.is_italic,
                )
                for page in document.pages
                for word in document.extract_words(page.index, include_artifacts=spec.include_artifacts, profile=profile)
            )
    with pdfplumber.open(BytesIO(payload), repair=spec.repair) as document:
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
                x_tolerance=spec.x_tolerance,
                y_tolerance=spec.y_tolerance,
                use_text_flow=spec.use_text_flow,
                split_at_punctuation=spec.split_at_punctuation,
                extra_attrs=list(spec.extra_attrs),
            )
        )


@beartype
def _region_arm(payload: bytes, provider: LensProvider, spec: LensSpec) -> tuple[DocumentNode, ...]:
    bbox = spec.bbox or _ORIGIN
    if provider is LensProvider.PDFOXIDE:  # commercial-safe default: `(x0,y0,x1,y1)` bounds -> pdf_oxide `(x,y,w,h)` region -> per-line RunNode
        x0, y0, x1, y1 = bbox
        with pdf_oxide.PdfDocument.from_bytes(payload) as document:
            return tuple(
                _node(NodeKind.RUN, "region", page.index, line.text.encode(), bounds=line.bbox, text=line.text)
                for page in document.pages
                for line in page.region(x0, y0, x1 - x0, y1 - y0).extract_text_lines()
            )
    with pdfplumber.open(BytesIO(payload), repair=spec.repair) as document:
        return tuple(
            _node(
                NodeKind.RUN, "region", index, line["text"].encode(), bounds=(line["x0"], line["top"], line["x1"], line["bottom"]), text=line["text"]
            )
            for index, page in enumerate(document.pages)
            for line in page.within_bbox(bbox).extract_text_lines(strip=True, return_chars=False)
        )


@beartype
def _story_arm(payload: bytes, provider: LensProvider, spec: LensSpec) -> tuple[DocumentNode, ...]:
    # per-page tree recovery: each page a PageNode carrying its media box + recovered styled content — the recover-TO
    # inverse of `document/report#REPORT` REFLOW authoring a fresh PageNode, so an authored reflow round-trips through PageNode.
    if provider is LensProvider.MUPDF:
        with pymupdf.open(stream=payload, filetype="pdf") as document:
            return tuple(
                _node(
                    NodeKind.PAGE,
                    "page",
                    index,
                    repr(tuple(page.rect)).encode(),
                    bounds=tuple(page.rect),
                    children=tuple(
                        _node(
                            NodeKind.BLOCK,
                            "block",
                            index,
                            repr(block["bbox"]).encode(),
                            bounds=tuple(block["bbox"]),
                            block=BlockKind.PARAGRAPH,
                            runs=tuple(
                                _node(
                                    NodeKind.RUN,
                                    "span",
                                    index,
                                    span["text"].encode(),
                                    bounds=tuple(span["bbox"]),
                                    text=span["text"],
                                    font_key=span.get("font", _RECOVERED_FONT),
                                    size=float(span.get("size", 0.0)),
                                    weight=700 if int(span.get("flags", 0)) & _BOLD_FLAG else 400,
                                    color=_rgb(int(span.get("color", 0))),
                                )
                                for line in block.get("lines", ())
                                for span in line.get("spans", ())
                            ),
                        )
                        for block in page.get_text("dict", flags=spec.flags or pymupdf.TEXTFLAGS_DICT)["blocks"]
                        if block.get("type", 1) == 0
                    ),
                )
                for index, page in enumerate(document)
            )
    with pdf_oxide.PdfDocument.from_bytes(payload) as document:  # commercial-safe default: media box + column-order spans per page
        return tuple(
            _node(
                NodeKind.PAGE,
                "page",
                page.index,
                repr(page.bbox).encode(),
                bounds=page.bbox,
                children=tuple(
                    _node(
                        NodeKind.RUN,
                        "span",
                        page.index,
                        span.text.encode(),
                        bounds=span.bbox,
                        text=span.text,
                        font_key=span.font_name or _RECOVERED_FONT,
                        size=span.font_size,
                        weight=700 if span.is_bold else 400,
                        italic=span.is_italic,
                        color=_scale8(span.color),
                    )
                    for span in document.extract_spans(page.index, reading_order=spec.reading_order)
                ),
            )
            for page in document.pages
        )


@beartype
def _paths_arm(payload: bytes, provider: LensProvider, spec: LensSpec) -> tuple[DocumentNode, ...]:
    # recovered vector geometry (the AEC drawing plane's recovered linework) -> content-keyed FigureNode leaves carrying
    # the path bbox as `intrinsic` + the vector `media_type`; a recovered drawing is a graphic figure, never a text run.
    if provider is LensProvider.MUPDF:
        with pymupdf.open(stream=payload, filetype="pdf") as document:
            return tuple(
                _node(
                    NodeKind.FIGURE,
                    str(path.get("type", "f")),
                    index,
                    repr(rect).encode(),
                    bounds=(rect.x0, rect.y0, rect.x1, rect.y1),
                    name=f"draw-{index}-{ordinal}",
                    media_type=_VECTOR_MEDIA,
                    intrinsic=(rect.width, rect.height),
                )
                for index, page in enumerate(document)
                for ordinal, path in enumerate(page.get_drawings())
                if (rect := path["rect"])
            )
    with pdfplumber.open(BytesIO(payload), repair=spec.repair) as document:  # curves + rects + lines vector-object dicts (MIT-licensed default)
        return tuple(
            _node(
                NodeKind.FIGURE,
                kind,
                index,
                repr(obj).encode(),
                bounds=(obj["x0"], obj["top"], obj["x1"], obj["bottom"]),
                name=f"{kind}-{index}-{ordinal}",
                media_type=_VECTOR_MEDIA,
                intrinsic=(obj["x1"] - obj["x0"], obj["bottom"] - obj["top"]),
            )
            for index, page in enumerate(document.pages)
            for kind, objects in (("curve", page.curves), ("rect", page.rects), ("line", page.lines))
            for ordinal, obj in enumerate(objects)
        )


@beartype
def _table_arm(payload: bytes, provider: LensProvider, spec: LensSpec) -> tuple[DocumentNode, ...]:
    settings: dict[str, object] = {
        "vertical_strategy": spec.vertical,
        "horizontal_strategy": spec.horizontal,
        "snap_tolerance": spec.snap_tolerance,
        "join_tolerance": spec.join_tolerance,
        "edge_min_length": spec.edge_min_length,
        "intersection_tolerance": spec.intersection_tolerance,
        "text_tolerance": spec.text_tolerance,
    }
    if provider is LensProvider.MUPDF:
        with pymupdf.open(stream=payload, filetype="pdf") as document:
            return tuple(
                _table_node(table.extract(), tuple(table.bbox), index, header_rows=0 if table.header.external else 1)
                for index, page in enumerate(document)
                for table in page.find_tables(
                    **settings
                ).tables  # `Table.header` is always a truthy `TableHeader`; `.external` is the real discriminant — an above-body synthesized header is NOT in `extract()` rows (0), an in-grid header row is (1)
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
        (
            rows.index(round(y0)),
            columns.index(round(x0)),
            columns.index(round(x1)) - columns.index(round(x0)),
            rows.index(round(y1)) - rows.index(round(y0)),
        )
        for x0, y0, x1, y1 in cells
        if columns.index(round(x1)) - columns.index(round(x0)) > 1 or rows.index(round(y1)) - rows.index(round(y0)) > 1
    )


@beartype
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


@beartype
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


@beartype
def _link_arm(payload: bytes, provider: LensProvider, spec: LensSpec) -> tuple[DocumentNode, ...]:
    if provider is LensProvider.MUPDF:
        with pymupdf.open(stream=payload, filetype="pdf") as document:
            return tuple(
                _node(
                    NodeKind.ANNOTATION,
                    "link",
                    index,
                    str(link.get("uri", link.get("page", ""))).encode(),
                    bounds=tuple(link["from"]),
                    annot=AnnotKind.LINK,
                    contents=str(link.get("uri", "")),
                    link=_link_target(link),
                )
                for index, page in enumerate(document)
                for link in page.get_links()
            )
    with pdfplumber.open(BytesIO(payload), repair=spec.repair) as document:
        return tuple(
            _node(
                NodeKind.ANNOTATION,
                "link",
                index,
                str(hit.get("uri", "")).encode(),
                bounds=(hit["x0"], hit["top"], hit["x1"], hit["bottom"]),
                annot=AnnotKind.LINK,
                contents=str(hit.get("uri", "")),
                link=Uri(href=str(hit["uri"])),
            )
            for index, page in enumerate(document.pages)
            for hit in page.hyperlinks
        )


@beartype
def _metadata_arm(payload: bytes, _provider: LensProvider, _spec: LensSpec) -> tuple[DocumentNode, ...]:
    reader = pypdf.PdfReader(BytesIO(payload))
    info = reader.metadata or {}
    fields = tuple(
        _node(
            NodeKind.FIELD, _META_KEY.try_find(slot).default_value(slot), 0, str(value).encode(), field=FieldKind.TEXT, name=_META_KEY.try_find(slot).default_value(slot), value=str(value)
        )
        for slot, value in info.items()
        if value
    )
    root = _node(
        NodeKind.STRUCTURE, str(info.get("/Title", "document")), 0, str(info.get("/Author", "")).encode(), elt=StructEltKind.DOCUMENT, children=fields
    )
    return (root,)


@beartype
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
            _node(
                NodeKind.ANNOTATION,
                "hit",
                index,
                needle.encode(),
                bounds=(hit["x0"], hit["top"], hit["x1"], hit["bottom"]),
                annot=AnnotKind.HIGHLIGHT,
                contents=needle,
            )
            for index, page in enumerate(document.pages)
            for hit in page.search(needle, regex=spec.regex, case=spec.case_sensitive)
        )


@beartype
def _ocr_arm(payload: bytes, provider: LensProvider, spec: LensSpec) -> tuple[DocumentNode, ...]:
    if provider is LensProvider.PDFOXIDE:  # commercial-safe in-process OCR (Rust engine), CORE band — no ocrmypdf subprocess, no PDF/A rewrite
        with pdf_oxide.PdfDocument.from_bytes(payload) as document:
            return tuple(
                _node(NodeKind.RUN, "ocr", page.index, line.encode(), text=line)
                for page in document.pages
                for line in document.extract_text_ocr(page.index).splitlines()
                if line
            )
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
            source.name,
            target.name,
            sidecar=sidecar.name,
            language=spec.language,
            output_type=spec.output_type,
            mode=spec.ocr_mode,
            deskew=spec.deskew,
            clean=spec.clean,
            rotate_pages=spec.rotate_pages,
            optimize=spec.optimize,
            progress_bar=False,
        )
        text = sidecar.read().decode() if code is ocrmypdf.ExitCode.ok else ""
    return (
        _node(NodeKind.STRUCTURE, code.name, 0, code.name.encode(), elt=StructEltKind.NOTE),
        *(_node(NodeKind.RUN, "ocr", index, line.encode(), text=line) for index, line in enumerate(text.splitlines()) if line),
    )


@beartype
def _embedded_arm(payload: bytes, _provider: LensProvider, _spec: LensSpec) -> tuple[DocumentNode, ...]:
    with pymupdf.open(stream=payload, filetype="pdf") as document:
        fields = tuple(
            _node(
                NodeKind.FIELD,
                "embedded",
                0,
                document.embfile_get(name),
                field=FieldKind.TEXT,
                name=name,
                value=document.embfile_info(name)["filename"],
            )
            for name in document.embfile_names()
        )
        figures = tuple(
            _node(
                NodeKind.FIGURE,
                "placed",
                index,
                pix.tobytes("png"),
                name=f"img-{xref}",
                intrinsic=(float(pix.width), float(pix.height)),
                media_type="image/png",
            )
            for index, page in enumerate(document)
            for xref, *_ in page.get_images(full=True)
            if (pix := pymupdf.Pixmap(document, xref))
        )
    return fields + figures


@beartype
def _widget_arm(payload: bytes, provider: LensProvider, _spec: LensSpec) -> tuple[DocumentNode, ...]:
    if provider is LensProvider.PDFOXIDE:  # commercial-safe AcroForm recovery WITH the FieldFlag set pymupdf's widget accessor never exposed
        with pdf_oxide.PdfDocument.from_bytes(payload) as document:
            return tuple(
                _node(
                    NodeKind.FIELD,
                    form.name,
                    0,
                    str(form.value).encode(),
                    field=_OXIDE_FIELD.try_find(form.field_type).default_value(FieldKind.TEXT),
                    name=form.name,
                    value=form.value,
                    flags=_oxide_flags(form),
                )
                for form in document.get_form_fields()
            )
    with pymupdf.open(stream=payload, filetype="pdf") as document:
        return tuple(
            _node(
                NodeKind.FIELD,
                widget.field_name,
                index,
                str(widget.field_value).encode(),
                field=_widget_field(pymupdf, widget.field_type),
                name=widget.field_name,
                value=widget.field_value,
            )
            for index, page in enumerate(document)
            for widget in page.widgets()
        )


@beartype
def _annotate_arm(payload: bytes, _provider: LensProvider, _spec: LensSpec) -> tuple[DocumentNode, ...]:
    with pymupdf.open(stream=payload, filetype="pdf") as document:
        return tuple(
            _node(
                NodeKind.ANNOTATION,
                "annotation",
                index,
                annot.info["content"].encode(),
                bounds=tuple(annot.rect),
                annot=_annot_kind(annot.type[1]),
                contents=annot.info["content"],
            )
            for index, page in enumerate(document)
            for annot in page.annots()
        )


def _widget_field(pymupdf: object, code: int) -> FieldKind:
    # SYMBOLIC_REFERENCE: the catalogued `PDF_WIDGET_TYPE_*` symbol names resolve to their int on the live module.
    return {getattr(pymupdf, name): kind for name, kind in _WIDGET_SYMBOL.items()}.get(code, FieldKind.TEXT)


def _oxide_flags(form: object) -> tuple[FieldFlag, ...]:
    # the FieldFlag set pdf_oxide `FormField.is_required`/`is_readonly` expose that the pymupdf widget accessor never carried.
    return tuple(flag for present, flag in ((form.is_required, FieldFlag.REQUIRED), (form.is_readonly, FieldFlag.READONLY)) if present)


def _annot_kind(name: str) -> AnnotKind:
    return _ANNOT_NAME.try_find(name).default_value(AnnotKind.NOTE)


@beartype
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
            _ORIGIN,
            index,
            role=sheet.getAttribute("name") or f"sheet-{index}",
        )
        for index, sheet in enumerate(document.getElementsByType(Table))
    )


@beartype
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
            yield _node(
                NodeKind.SECTION,
                text or "section",
                index,
                text.encode(),
                level=int(node.getAttribute("outlinelevel") or 1),
                heading=(_node(NodeKind.RUN, "heading", index, text.encode(), text=text),),
            )
        elif probe(P):
            text = extractText(node)
            yield _node(
                NodeKind.BLOCK,
                "paragraph",
                index,
                text.encode(),
                block=BlockKind.PARAGRAPH,
                runs=(_node(NodeKind.RUN, "run", index, text.encode(), text=text),),
            )
        else:
            yield from _odt_blocks(node.childNodes)


@beartype
def _xlsx_arm(payload: bytes, _provider: LensProvider, spec: LensSpec) -> tuple[DocumentNode, ...]:
    workbook = python_calamine.CalamineWorkbook.from_object(BytesIO(payload), load_tables=spec.load_tables)
    sheets = spec.sheets or tuple(workbook.sheet_names)
    return tuple(
        _table_node(
            [[None if value is None else str(value) for value in row] for row in sheet.to_python(skip_empty_area=spec.skip_empty_area)],
            _ORIGIN,
            index,
            role=name,
            spans=_calamine_spans(sheet.merged_cell_ranges),
        )
        for index, name in enumerate(sheets)
        if (sheet := workbook.get_sheet_by_name(name))
    )


def _calamine_spans(ranges: object) -> Spans:
    return tuple((r0, c0, c1 - c0 + 1, r1 - r0 + 1) for (r0, c0), (r1, c1) in (ranges or ()) if c1 > c0 or r1 > r0)


@beartype
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
            members = tuple(
                _node(NodeKind.BLOCK, "item", index, block.text.encode(), block=BlockKind.PARAGRAPH, runs=_docx_runs(block, index))
                for index, block in rows
            )
            yield _node(NodeKind.LIST, list_kind.value, rows[0][0], list_kind.value.encode(), list_kind=list_kind, items=members)


def _docx_list_kind(block: object) -> ListKind | None:
    return _DOCX_LIST.try_find(getattr(getattr(block, "style", None), "name", "")).default_value(None)


def _docx_block(block: object, index: int) -> DocumentNode:
    if hasattr(block, "rows"):
        return _table_node([[cell.text for cell in row.cells] for row in block.rows], _ORIGIN, index)
    style = getattr(block.style, "name", "")
    runs = _docx_runs(block, index)
    level = _DOCX_HEADING.try_find(style).default_value(None)
    if level is not None:
        return _node(NodeKind.SECTION, block.text, index, block.text.encode(), level=level, heading=runs)
    return _node(NodeKind.BLOCK, "paragraph", index, block.text.encode(), block=_DOCX_BLOCK.try_find(style).default_value(BlockKind.PARAGRAPH), runs=runs)


def _docx_runs(block: object, index: int) -> tuple[DocumentNode, ...]:
    return tuple(
        _node(
            NodeKind.RUN,
            "run",
            index,
            run.text.encode(),
            text=run.text,
            font_key=run.font.name or (run.style.name if run.style else _RECOVERED_FONT),  # emit writes `font.name`; read its inverse
            size=run.font.size.pt if run.font.size else 0.0,
            weight=700 if run.bold else 400,
            italic=bool(run.italic),
            decorations=(TextDecoration.UNDERLINE,) if run.font.underline else (),
            color=tuple(rgb) if (rgb := run.font.color.rgb) else (0, 0, 0),
        )
        for run in block.runs
    )


@beartype
def _yaml_arm(payload: bytes, _provider: LensProvider, spec: LensSpec) -> tuple[DocumentNode, ...]:
    engine = YAML(typ=spec.typ)
    return tuple(_value_node(document, "yaml", index) for index, document in enumerate(engine.load_all(BytesIO(payload))))


@beartype
def _toml_arm(payload: bytes, _provider: LensProvider, _spec: LensSpec) -> tuple[DocumentNode, ...]:
    return (_value_node(tomlkit.parse(payload).unwrap(), "toml", 0),)


def _value_node(value: object, role: str, page: int, *, key: str = "") -> DocumentNode:
    match value:
        case Mapping():
            return _node(
                NodeKind.BLOCK,
                key or role,
                page,
                repr(value).encode(),
                block=BlockKind.PARAGRAPH,
                children=tuple(_value_node(child, role, page, key=str(name)) for name, child in value.items()),
            )
        case list() | tuple():
            return _node(
                NodeKind.LIST,
                key or role,
                page,
                repr(value).encode(),
                list_kind=ListKind.ORDERED,
                items=tuple(_value_node(child, role, page, key=str(ordinal)) for ordinal, child in enumerate(value)),
            )
        case _:
            text = "" if value is None else str(value)
            return _node(NodeKind.RUN, key or role, page, text.encode(), text=text)


@beartype
def _xml_arm(payload: bytes, _provider: LensProvider, spec: LensSpec) -> tuple[DocumentNode, ...]:
    parser = etree.XMLParser(recover=spec.recover, resolve_entities=False, no_network=True, huge_tree=False)
    return (_element_node(etree.fromstring(payload, parser=parser), 0),)


def _element_node(element: object, page: int) -> DocumentNode:
    tag = etree.QName(element).localname if isinstance(element.tag, str) else "comment"
    text = (element.text or "").strip()
    runs = (_node(NodeKind.RUN, tag, page, text.encode(), text=text),) if text else ()
    children = runs + tuple(_element_node(child, page) for child in element if isinstance(child.tag, str))
    return _node(NodeKind.BLOCK, tag, page, etree.tostring(element).strip(), block=BlockKind.PARAGRAPH, level=1, children=children)


def _lens_raise(fault: object) -> tuple[DocumentNode, ...]:
    # terminal collapse at the extraction boundary: an offload fault reconstructs the raise the node's rail folds.
    raise ValueError(str(fault))


def _gated_recover(lens: "DocumentLens") -> tuple[DocumentNode, ...]:
    arm, default = _ROUTES[lens.op]
    return arm(lens.payload, lens.provider or default, lens.spec)


# --- [TABLES] ---------------------------------------------------------------------------

_DOCX_HEADING: Final[Map[str, int]] = Map.of_seq([
    ("Title", 1),
    ("Heading 1", 1),
    ("Heading 2", 2),
    ("Heading 3", 3),
    ("Heading 4", 4),
    ("Heading 5", 5),
    ("Heading 6", 6),
])
_DOCX_LIST: Final[Map[str, ListKind]] = Map.of_seq([("List Bullet", ListKind.UNORDERED), ("List Number", ListKind.ORDERED)])
_DOCX_BLOCK: Final[Map[str, BlockKind]] = Map.of_seq([
    ("Quote", BlockKind.QUOTE),
    ("Intense Quote", BlockKind.QUOTE),
    ("Caption", BlockKind.CAPTION),
])
_META_KEY: Final[Map[str, str]] = Map.of_seq([
    ("/Title", "title"),
    ("/Author", "author"),
    ("/Subject", "subject"),
    ("/Keywords", "keywords"),
    ("/Creator", "creator"),
    ("/Producer", "producer"),
    ("/CreationDate", "created"),
    ("/ModDate", "modified"),
])
_WIDGET_SYMBOL: Final[Map[str, FieldKind]] = Map.of_seq([
    ("PDF_WIDGET_TYPE_TEXT", FieldKind.TEXT),
    ("PDF_WIDGET_TYPE_CHECKBOX", FieldKind.CHECKBOX),
    ("PDF_WIDGET_TYPE_RADIOBUTTON", FieldKind.CHECKBOX),
    ("PDF_WIDGET_TYPE_LISTBOX", FieldKind.CHOICE),
    ("PDF_WIDGET_TYPE_COMBOBOX", FieldKind.CHOICE),
    ("PDF_WIDGET_TYPE_BUTTON", FieldKind.BUTTON),
    ("PDF_WIDGET_TYPE_SIGNATURE", FieldKind.SIGNATURE),
])
# pdf_oxide `FormField.field_type` string vocabulary -> FieldKind; total `.get(..., TEXT)` default absorbs any unmapped token.
_OXIDE_FIELD: Final[Map[str, FieldKind]] = Map.of_seq([
    ("text", FieldKind.TEXT),
    ("checkbox", FieldKind.CHECKBOX),
    ("radio", FieldKind.CHECKBOX),
    ("listbox", FieldKind.CHOICE),
    ("combobox", FieldKind.CHOICE),
    ("choice", FieldKind.CHOICE),
    ("push_button", FieldKind.BUTTON),
    ("button", FieldKind.BUTTON),
    ("signature", FieldKind.SIGNATURE),
])
_ANNOT_NAME: Final[Map[str, AnnotKind]] = Map.of_seq([
    ("Highlight", AnnotKind.HIGHLIGHT),
    ("Squiggly", AnnotKind.HIGHLIGHT),
    ("Underline", AnnotKind.HIGHLIGHT),
    ("StrikeOut", AnnotKind.HIGHLIGHT),
    ("Redact", AnnotKind.REDACTION),
    ("Link", AnnotKind.LINK),
    ("Text", AnnotKind.NOTE),
    ("FreeText", AnnotKind.NOTE),
    ("Stamp", AnnotKind.STAMP),
])
_ROUTES: Final[Map[LensOp, tuple[RecoverArm, LensProvider]]] = Map.of_seq([
    (LensOp.EXTRACT_TEXT, (_text_arm, LensProvider.PDFOXIDE)),  # layout-aware column-order default supersedes the pypdf running-text arm
    (LensOp.EXTRACT_IMAGES, (_images_arm, LensProvider.PYPDF)),
    (LensOp.TABLE, (_table_arm, LensProvider.PLUMBER)),
    (LensOp.WORDS, (_words_arm, LensProvider.PDFOXIDE)),  # TextWord geometry default; pdfplumber the alternate word arm
    (LensOp.REGION, (_region_arm, LensProvider.PDFOXIDE)),  # PdfPageRegion line default; pdfplumber the alternate crop arm
    (LensOp.STORY, (_story_arm, LensProvider.PDFOXIDE)),
    (LensOp.PATHS, (_paths_arm, LensProvider.PLUMBER)),  # MIT curves+rects+lines default; pymupdf get_drawings the alternate
    (LensOp.OUTLINE, (_outline_arm, LensProvider.MUPDF)),
    (LensOp.STRUCTURE, (_structure_arm, LensProvider.PLUMBER)),
    (LensOp.LINK, (_link_arm, LensProvider.MUPDF)),
    (LensOp.METADATA, (_metadata_arm, LensProvider.PYPDF)),
    (LensOp.SEARCH, (_search_arm, LensProvider.PLUMBER)),
    (LensOp.OCR, (_ocr_arm, LensProvider.PDFOXIDE)),  # in-process CORE OCR default; ocrmypdf reserved for the PDF/A output path
    (LensOp.EMBEDDED, (_embedded_arm, LensProvider.MUPDF)),
    (LensOp.WIDGET, (_widget_arm, LensProvider.PDFOXIDE)),  # FieldFlag-carrying AcroForm default; pymupdf the alternate widget arm
    (LensOp.ANNOTATE, (_annotate_arm, LensProvider.MUPDF)),
    (LensOp.XLSX_READ, (_xlsx_arm, LensProvider.CALAMINE)),
    (LensOp.ODS_READ, (_ods_arm, LensProvider.ODFPY)),
    (LensOp.ODT_READ, (_odt_arm, LensProvider.ODFPY)),
    (LensOp.DOCX_READ, (_docx_arm, LensProvider.DOCX)),
    (LensOp.YAML_READ, (_yaml_arm, LensProvider.RUAMEL)),
    (LensOp.TOML_READ, (_toml_arm, LensProvider.TOMLKIT)),
    (LensOp.XML_READ, (_xml_arm, LensProvider.LXML)),
])
```

## [03]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)

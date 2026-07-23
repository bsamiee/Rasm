# [PY_ARTIFACTS_LENS]

Recover-TO half of the bidirectional `document` seam: `DocumentLens` recovers a `DocumentNode` tree back OUT of an emitted PDF, a scanned raster, or an office/structured-text payload — one owner discriminating recovery operation over the closed `LensOp`, routed by the one `_ROUTES` `Route` table whose rows carry the arm, the default provider, AND the admissible provider set, never a `get_text`/`get_words` verb family. Production and extraction are inverses over the one node algebra, so a `document/emit#DOCUMENT` emission and a `DocumentLens` recovery round-trip through `DocumentNode` with `document/model#DELTA` defined once. Beyond content recovery the op family carries the prepress/forensic examination plane — per-glyph geometry, computed layout metrics, document/page classification, OCG layer and separation-ink census, page-label trees, and FDF/XFDF form-data export — the incoming-file triage a delivery plane accepting consultant files runs before it accepts.

Layout-dominant ops default to `LensProvider.PDFOXIDE` — the MIT/Apache Rust-core engine carrying reading-order XY-cut recovery, region crop, in-process OCR, and `FormField`-flag AcroForm recovery — the commercial-safe path where the AGPL `pymupdf` arms are barred on a closed-distributed pipeline, `pymupdf` retained for the permissive/internal lane. Each provider carries its own runtime `band`: the native and pure-Python readers resolve in-process on the core, and only the process-isolated companions (`ocrmypdf`, `python-calamine`, `lxml`) cross as `KernelTrait.HOSTILE` kernels onto the warm process pool; every reader arm is a `@beartype` boundary narrowing the recovered provider shapes to the model-legal `DocumentNode` before crossing back. `LensSpec` admits exactly once at `.of` under the per-op `_REQUIRED` precondition and the `Route.providers` admission — an op/provider pair outside the row refuses as `LensFault.provider`, so no reader arm can fall through to an engine the caller never named; the `@receipted(OPEN)` weave drains the stepped owner and `contribute` mints the `core/receipt#RECEIPT` `ArtifactReceipt.Introspection` case.

## [01]-[INDEX]

- [01]-[LENS]: one recovery owner discriminating `LensOp` over the `_ROUTES` `Route` rows under provider-keyed bands and path-keyed node identity.

## [02]-[LENS]

- Owner: `DocumentLens` — the ops dispatch a provider VALUE rather than reconstructing the engine choice, the band recovered once from `provider.band`, the offload crossed through the owner's own `lane: LanePolicy` instance (`self.lane.offload`, the runtime-owned bound — never a class-qualified call nor a folder-minted limiter). One polymorphic `_node` constructor mints every node variant over a `NodeMeta` whose content key joins the structural `path` — the `tuple[int, ...]` of child ordinals from the recovery root, the node's structural uid — to the content payload, so identical-content siblings under one parent never collapse onto one slot and a content change at a fixed path re-keys without stealing a sibling's identity; `bounds` stays geometric evidence on the meta, never an identity substitute. Per-kind material admits through the closed `NodeSlot` `TypedDict`, honoring each variant's real field contract — never a per-kind sibling-factory family.
- Cases: the non-obvious per-arm rules the fence cannot self-justify — TABLE under PDFOXIDE reads the native `extract_tables` row dicts (`rows[].cells[].text`, `is_header` folding `header_rows`, the `(x, y, w, h)` bbox converted once), under MUPDF reads the `Table.header.external` discriminant into `header_rows` (an above-body synthesized header `0`, an in-grid header row `1`, never the always-truthy `Table.header` object), and under PLUMBER folds the `Table.cells` bbox set into merged-cell `spans` quads; REGION converts the model `(x0,y0,x1,y1)` bbox once to the pdf_oxide `(x, y, w, h)` convention; OCR defaults to the in-process pdf_oxide Rust engine (no subprocess hop, no PDF/A rewrite), the gated `ocrmypdf` alternate RESERVED for the PDF/A output path with its `ExitCode` return gating the sidecar text feed, and a non-PDF raster wraps losslessly through `new_page`/`insert_image` before OCR; WIDGET folds pdf_oxide `FormField.is_required`/`is_readonly` onto `FieldNode.required`/`readonly` — the policy pair the pymupdf widget accessor cannot fill — and builds the per-mode model `FieldValue` case through the one `_FIELD_BUILDERS` token table (a raw AcroForm `/Btn` reads back as the `"button"` token, so a bool value discriminates it to `CheckboxField`), the pymupdf alternate resolving `field_type` ints via the catalogued `PDF_WIDGET_TYPE_*` names into the same token space; CLASSIFY decodes the engine's JSON verdict strings through `msgspec.json.decode`, never a rendered-string regex; DOCX_READ groups consecutive list-styled paragraphs through `groupby` into one `ListNode` and recovers `Paragraph.hyperlinks` as `AnnotationNode` link children, the inverse of the emit `List Bullet`/`List Number` lowering; YAML_READ rides `load_all` with the single-document case subsumed, never a `multi` knob; XML_READ runs the hardened parser (`resolve_entities=False`, `no_network=True`, `huge_tree=False`) and ONLY the serialized `DocumentNode` tree crosses back across the interpreter seam.
- Auto: every recursive foreign source — the pikepdf `/K` spine, the pdfplumber structure branches, the lxml element tree, a YAML/TOML value graph — rebuilds post-order through the one `_grown` expand/combine marker frontier on an immutable `Block` stack, so adversarial nesting depth never overflows the interpreter frame limit and every rebuilt node receives its structural path from the same frontier; the flattening walks (pypdf outline, ODT containers) run small explicit stacks under the same depth law.
- Entry: the key mints PRE-RUN over `(op, payload, provider, scoped spec)` — the spec preimage narrowed by `_scoped` to the op's `Route.observed` roster so an unobserved knob edit never forks an op's key, and never the recovered tree, whose digest stays a `document/model#NODE` `node_digest` projection downstream consumers derive on demand. A GATED row crosses onto the worker that re-resolves the SAME `_ROUTES` row and reifies the module-scope `lazy` bindings there, so the worker lane carries no second dispatch.
- Receipt: `contribute` reads the stepped `recovered` directly and never re-runs `_emit`, so a worker-gated arm is never re-imported on the core during the receipt harvest; the `Introspection` counts project by `walk` over the node variants, the tag riding the encoded `kind` field, never a runtime `.tag` attribute.
- Growth: a new recovery op is one `LensOp` member, one `Route` row (arm, default, providers, observed), and one `_REQUIRED` row when it needs material; a new provider for an existing op is one member in the row's `providers` set with its arm branch; a new provider knob is one `LensSpec` field, one `LensPayload` row, and its op's `observed` entry; a richer recovered field is one `NodeSlot` key honored by `_node`; a new AcroForm mode is one `_FIELD_BUILDERS` row.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
from collections.abc import Callable, Iterable, Iterator, Mapping
from enum import StrEnum
from io import BytesIO
from itertools import groupby
from typing import Final, Literal, NotRequired, ReadOnly, Self, TypedDict, Unpack, assert_never

import msgspec
from beartype import beartype
from beartype.door import is_bearable
from expression import Error, Ok, Result, case, tag, tagged_union
from expression.collections import Block, Map
from msgspec import Struct, field, structs
from pydantic import TypeAdapter, ValidationError

from rasm.artifacts.core.plan import Admission, ArtifactWork
from rasm.artifacts.core.receipt import ArtifactReceipt
from rasm.artifacts.document.model import (
    AnnotationNode,
    AnnotKind,
    AnnotTarget,
    BlockKind,
    BlockNode,
    ButtonField,
    CheckboxField,
    ComboField,
    Dest,
    DocumentNode,
    FieldNode,
    FieldValue,
    FigureNode,
    ForeignRole,
    FormulaNode,
    ListField,
    ListKind,
    ListNode,
    NodeKind,
    NodeMeta,
    NoTarget,
    PageNode,
    RadioField,
    RunNode,
    RunScript,
    SectionNode,
    SignatureField,
    StandardRole,
    StructEltKind,
    StructRole,
    StructureNode,
    TableNode,
    TextDecoration,
    TextDirection,
    TextField,
    Uri,
    walk,
)
from rasm.runtime.identity import ContentIdentity, ContentKey
from rasm.runtime.faults import RuntimeRail, async_boundary
from rasm.runtime.lanes import LanePolicy
from rasm.runtime.workers import Kernel, KernelTrait
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
lazy from pathlib import Path
lazy from ruamel.yaml import YAML
lazy from tempfile import NamedTemporaryFile

# --- [TYPES] ----------------------------------------------------------------------------

type Bounds = tuple[float, float, float, float]
type Grid = list[list[str | None]]
type Spans = tuple[tuple[int, int, int, int], ...]
type Trail = tuple[int, ...]  # structural path from the recovery root — the node's identity axis; synthesized intra-node leaves take negative slots
type RecoverArm = Callable[[bytes, "LensProvider", "LensSpec"], tuple[DocumentNode, ...]]


class LensOp(StrEnum):
    EXTRACT_TEXT = "extract-text"
    EXTRACT_IMAGES = "extract-images"
    TABLE = "table"
    WORDS = "words"
    CHARS = "chars"  # per-glyph forensic geometry: pdf_oxide TextChar rows carrying font metrics and the marked-content MCID
    REGION = "region"
    STORY = "story"  # per-page tree recovery: the recover-TO inverse of `document/report#REPORT` REFLOW
    PATHS = "paths"  # recovered vector geometry (fills/strokes/curves) for the AEC drawing plane
    LAYOUT = "layout"  # computed per-page layout metrics (columns, medians, gap thresholds) — triage evidence
    CLASSIFY = "classify"  # document/page classification verdicts (scanned/born-digital/form) — intake triage
    LAYERS = "layers"  # OCG optional-content layer census
    INKS = "inks"  # separation-ink (spot/process) census per page — the prepress read-side of `graphic/color/managed#MANAGED`
    PAGE_LABELS = "page-labels"  # the page-label numbering tree
    OUTLINE = "outline"
    STRUCTURE = "structure"
    LINK = "link"
    METADATA = "metadata"
    SEARCH = "search"
    OCR = "ocr"
    EMBEDDED = "embedded"
    WIDGET = "widget"
    FORM_DATA = "form-data"  # FDF/XFDF filled-form export — structured form-data recovery for the delivery plane
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


class InkDepth(StrEnum):
    DIRECT = "direct"
    NESTED = "nested"

    def read(self, document: "pdf_oxide.PdfDocument", page: int, /) -> Iterable[object]:
        match self:
            case InkDepth.DIRECT:
                return document.get_page_inks(page)
            case InkDepth.NESTED:
                return document.get_page_inks_deep(page)
            case unreachable:
                assert_never(unreachable)


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
_KEY_ENCODER: Final = msgspec.msgpack.Encoder(order="deterministic")  # the stable preimage encoding the bare `ContentIdentity.key` mint addresses
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
    # Closed ADMISSION vocabulary `of` produces; every arm-level provider raise
    # (`pymupdf.FileDataError`/`pdfplumber.PdfminerException`/`python_calamine.CalamineError`/
    # `ocrmypdf.ExitCodeException`/`lxml.etree.XMLSyntaxError`) converts to the runtime `BoundaryFault`
    # at the `async_boundary` capsule, never into this interior vocabulary.
    tag: Literal["payload", "unsatisfied", "provider"] = tag()
    payload: tuple[str, ...] = case()  # the rejected LensPayload key paths
    unsatisfied: tuple[LensOp, str] = case()  # an op whose `_REQUIRED` input field is empty
    provider: tuple[LensOp, LensProvider] = case()  # a provider outside the op's `Route.providers` admission set


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
    form_format: Literal["fdf", "xfdf"] = "fdf"  # FORM_DATA pdf_oxide `export_form_data(format=)`; the Literal is the admission bound
    ink_depth: InkDepth = InkDepth.DIRECT  # INKS selects its catalogued reader through the behavior-bearing policy value


class Route(Struct, frozen=True):
    # one row is the whole per-op policy: the arm, the default engine, the admissible provider set, and the spec fields the
    # arm observes — `of` refuses a provider outside `providers`, so an arm's branch set is total over its admitted engines
    # by construction, and `_scoped` keys identity over `observed` alone so an unrelated knob edit never forks an op's key.
    arm: RecoverArm
    default: LensProvider
    providers: frozenset[LensProvider]
    observed: tuple[str, ...] = ()


class DocumentLens(Struct, frozen=True):
    op: LensOp
    payload: bytes
    provider: LensProvider
    # `lane` arrives projected via LanePolicy.of(context) at the composition root — a capacity literal has no owner.
    lane: LanePolicy
    spec: LensSpec = field(default_factory=LensSpec)
    recovered: tuple[DocumentNode, ...] = ()

    def emit(self, /) -> ArtifactWork:
        return ArtifactWork(key=self._key, work=self._emit, parents=(), admission=Admission(keyed=None), cost=float(len(self.payload)))

    @property
    def _key(self) -> ContentKey:
        # `ContentIdentity.key` mints the bare `ContentKey`; `.of` is the railed form and never keys a plan.
        return ContentIdentity.key(f"lens-{self.op.value}", _KEY_ENCODER.encode((self.op, self.payload, self.provider, self._scoped())))

    def _scoped(self) -> LensSpec:
        # identity preimage carries only the fields the op's arm observes (the `Route.observed` roster), so two lenses
        # differing on a foreign op's knob share one key and the elision cache never re-renders on an unobserved edit.
        return LensSpec(**{name: getattr(self.spec, name) for name in _ROUTES[self.op].observed})

    @receipted(OPEN)  # lens facts carry no classified field, so the runtime keep-all `OPEN` policy rides directly, never a re-minted per-file `Redaction`
    async def _recovered(self) -> Self:
        route = _ROUTES[self.op]
        crossed = (
            await self.lane.offload(Kernel.of(_gated_recover, KernelTrait.HOSTILE), self)
            if self.provider.band is LensBand.GATED
            else await self.lane.offload(Kernel.of(route.arm, KernelTrait.RELEASING), self.payload, self.provider, self.spec)
        )
        return structs.replace(self, recovered=crossed.default_with(lambda fault: _lens_raise(fault)))

    async def _emit(self) -> RuntimeRail[ArtifactReceipt]:
        # Terminal receipt threads the PRE-RUN input key so receipt.slot == node.key.
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
    def of(cls, op: LensOp, payload: bytes, /, *, lane: LanePolicy, provider: LensProvider | None = None, **raw: Unpack[LensPayload]) -> Result[Self, LensFault]:
        route = _ROUTES[op]
        selected = route.default if provider is None else provider
        if selected not in route.providers:
            return Error(LensFault(provider=(op, selected)))
        try:
            admitted = _PAYLOAD.validate_python(raw, strict=True)
        except ValidationError as fault:
            return Error(LensFault(payload=tuple(str(error["loc"]) for error in fault.errors())))
        spec = LensSpec(**admitted)
        missing = next((name for name in _REQUIRED.try_find(op).default_value(()) if not getattr(spec, name)), None)
        return Error(LensFault(unsatisfied=(op, missing))) if missing else Ok(cls(op=op, payload=payload, provider=selected, lane=lane, spec=spec))


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
    form_format: NotRequired[ReadOnly[Literal["fdf", "xfdf"]]]
    ink_depth: NotRequired[ReadOnly[InkDepth]]


_PAYLOAD: Final = TypeAdapter(LensPayload)
# Per-op precondition: a row's named `LensSpec` fields must be non-empty so the interior is total.
_REQUIRED: Final[Map[LensOp, tuple[str, ...]]] = Map.of_seq([(LensOp.REGION, ("bbox",)), (LensOp.SEARCH, ("needle",))])


# --- [OPERATIONS] -----------------------------------------------------------------------


class NodeSlot(TypedDict, total=False, closed=True):
    # Closed per-kind construction payload `_node` admits — typed ingress replacing a `**slot: object` bag.
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
    footer_rows: int
    header_cols: int
    field: FieldValue
    required: bool
    readonly: bool
    tooltip: str
    tex: str
    display: bool
    mathml: str
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


def _node(kind: NodeKind, role: str, page: int, payload: bytes, *, path: Trail, bounds: Bounds | None = None, **slot: Unpack[NodeSlot]) -> DocumentNode:
    # Structural path is the identity axis (`shapes.md` OWNER_COMPOSITION): the key preimage joins path to content,
    # so identical-content siblings key distinctly and a content change at a fixed path is the separate change axis.
    trail = "-".join(map(str, path)) or "root"
    meta = NodeMeta(key=ContentIdentity.key(f"lens-{role}-{page}-{trail}", payload), role=role, page=page, bounds=bounds)
    match kind:
        case NodeKind.RUN:
            return RunNode(
                meta=meta,
                text=slot.get("text", ""),
                font_key=slot.get("font_key", _RECOVERED_FONT),
                size=slot.get("size") or 1.0,  # `RunNode.size` is `PositiveFloat`; a provider's absent/zero size lands the recovered floor, never an illegal 0.0
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
                asset_key=ContentIdentity.key(f"asset-{slot['name']}", payload),
                alt=slot.get("alt", ""),
                media_type=slot.get("media_type", "image/png"),
                intrinsic=slot.get("intrinsic"),
                caption=slot.get("caption", ()),
            )
        case NodeKind.TABLE:
            # every recoverable TableNode band survives construction; an arm that cannot observe a band leaves
            # its zero default rather than erasing a sibling arm's recovered value.
            return TableNode(
                meta=meta,
                rows=slot.get("rows", ()),
                spans=slot.get("spans", ()),
                header_rows=slot.get("header_rows", 0),
                footer_rows=slot.get("footer_rows", 0),
                header_cols=slot.get("header_cols", 0),
                caption=slot.get("caption", ()),
            )
        case NodeKind.FIELD:
            return FieldNode(
                meta=meta,
                name=slot["name"],
                field=slot.get("field", TextField()),
                required=slot.get("required", False),
                readonly=slot.get("readonly", False),
                tooltip=slot.get("tooltip", ""),
            )
        case NodeKind.FORMULA:
            return FormulaNode(meta=meta, tex=slot.get("tex", ""), display=slot.get("display", False), alt=slot.get("alt", ""), mathml=slot.get("mathml", ""))
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


def _table_node(grid: Grid, bbox: Bounds, page: int, path: Trail, *, role: str = "table", spans: Spans = (), header_rows: int = 0) -> TableNode:
    rows = tuple(
        tuple(_node(NodeKind.RUN, "cell", page, (cell or "").encode(), path=(*path, r, c), text=cell or "") for c, cell in enumerate(row))
        for r, row in enumerate(grid)  # the (r, c) path extension keys identical-content cells (empty, repeated) distinctly
    )
    return _node(NodeKind.TABLE, role, page, repr(grid).encode(), path=path, bounds=bbox, rows=rows, spans=spans, header_rows=header_rows)


def _outline_node(level: int, title: str, page: int, path: Trail, /) -> DocumentNode:
    elt = _HEADING[min(max(level, _HEADING_FLOOR), _HEADING_CEIL) - 1]
    return _node(NodeKind.STRUCTURE, title, page, f"{elt.value}:{title}".encode(), path=path, elt=elt)


def _struct_role(name: str) -> StructRole:
    try:
        return StandardRole(elt=StructEltKind(name))
    except ValueError:
        return ForeignRole(role=name) if name else StandardRole(elt=StructEltKind.SECT)


def _link_target(link: Mapping[str, object]) -> AnnotTarget:
    return Uri(href=str(link["uri"])) if "uri" in link else Dest(page=int(link.get("page", 0) or 0))


# --- [FRONTIER] ---------------------------------------------------------------------------


_MAX_NODES: Final[int] = 1_000_000  # frontier expansion budget — an alias bomb or runaway breadth refuses instead of exhausting memory


@tagged_union(frozen=True)
class _Frame:
    tag: Literal["expand", "combine"] = tag()
    expand: tuple[object, Trail] = case()
    combine: tuple[object, Trail, int] = case()


def _grown(
    seed: object,
    kids: Callable[[object], tuple[object, ...]],
    built: Callable[[object, Trail, tuple[DocumentNode, ...]], DocumentNode],
    /,
    *,
    base: Trail = (),
) -> DocumentNode:
    # one post-order expand/combine marker frontier (`iteration.md`): every untrusted recursive source — the pikepdf
    # `/K` spine, an XML tree, a YAML graph — rebuilds through it, so adversarial depth never overflows the frame limit
    # and the frontier itself assigns each child its ordinal path. Two admission fences ride the expansion: a repeated
    # CONTAINER identity is a cycle or YAML-alias bomb (interned primitive leaves repeat legitimately and never recurse),
    # and _MAX_NODES caps total expansion so runaway breadth refuses instead of exhausting memory.
    frames, results = Block.singleton(_Frame(expand=(seed, base))), Block.empty()
    seen: set[int] = set()
    expanded = 0
    while not frames.is_empty():  # Exemption: iterative frontier — the untrusted recursive payload forfeits the recursive form
        head, frames = frames.head(), frames.tail()
        match head:
            case _Frame(tag="expand", expand=(node, path)):
                expanded += 1
                if expanded > _MAX_NODES:
                    raise ValueError(f"<lens:node-budget:{_MAX_NODES}>")
                branches = kids(node)
                if branches:
                    if id(node) in seen:
                        raise ValueError("<lens:cyclic-source>")
                    seen.add(id(node))
                    pending = Block.of_seq(_Frame(expand=(child, (*path, ordinal))) for ordinal, child in reversed(tuple(enumerate(branches))))
                    frames = pending.append(frames.cons(_Frame(combine=(node, path, len(branches)))))
                else:
                    results = results.cons(built(node, path, ()))
            case _Frame(tag="combine", combine=(node, path, arity)):
                results = results.skip(arity).cons(built(node, path, tuple(results.take(arity))))
            case _ as unreachable:
                assert_never(unreachable)
    return results.head()


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
                    path=(page.index,),
                    bounds=page.bbox,
                    block=BlockKind.PARAGRAPH,
                    runs=tuple(
                        _node(
                            NodeKind.RUN,
                            "span",
                            page.index,
                            span.text.encode(),
                            path=(page.index, ordinal),
                            bounds=span.bbox,
                            text=span.text,
                            font_key=span.font_name or _RECOVERED_FONT,
                            size=span.font_size,
                            weight=700 if span.is_bold else 400,
                            italic=span.is_italic,
                            color=_scale8(span.color),
                        )
                        for ordinal, span in enumerate(document.extract_spans(page.index, reading_order=spec.reading_order))
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
                    path=(index, slot),
                    bounds=tuple(block["bbox"]),
                    block=BlockKind.PARAGRAPH,
                    runs=tuple(
                        _node(
                            NodeKind.RUN,
                            "span",
                            index,
                            span["text"].encode(),
                            path=(index, slot, ordinal),
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
                        for ordinal, (line, span) in enumerate((line, span) for line in block.get("lines", ()) for span in line.get("spans", ()))
                    ),
                )
                for index, page in enumerate(document)
                for slot, block in enumerate(page.get_text("dict", flags=spec.flags or pymupdf.TEXTFLAGS_DICT)["blocks"])
                if block.get("type", 1) == 0
            )
    reader = pypdf.PdfReader(BytesIO(payload))
    return tuple(
        _node(NodeKind.RUN, "run", index, text.encode(), path=(index,), text=text)
        for index, page in enumerate(reader.pages)
        if (text := page.extract_text(extraction_mode=spec.mode))
    )


@beartype
def _images_arm(payload: bytes, _provider: LensProvider, _spec: LensSpec) -> tuple[DocumentNode, ...]:
    reader = pypdf.PdfReader(BytesIO(payload))
    return tuple(
        _node(NodeKind.FIGURE, "figure", index, image.data, path=(index, ordinal), name=image.name)
        for index, page in enumerate(reader.pages)
        for ordinal, image in enumerate(page.images)
    )


@beartype
def _words_arm(payload: bytes, provider: LensProvider, spec: LensSpec) -> tuple[DocumentNode, ...]:
    if provider is LensProvider.PDFOXIDE:  # commercial-safe default: TextWord geometry -> word RunNode leaves, artifact-tagged spans dropped
        # Per-document-class layout profile tunes the XY-cut word-margin/space heuristics: an
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
                    path=(page.index, ordinal),
                    bounds=word.bbox,
                    text=word.text,
                    font_key=word.font_name or _RECOVERED_FONT,
                    size=word.font_size,
                    weight=700 if word.is_bold else 400,
                    italic=word.is_italic,
                )
                for page in document.pages
                for ordinal, word in enumerate(document.extract_words(page.index, include_artifacts=spec.include_artifacts, profile=profile))
            )
    with pdfplumber.open(BytesIO(payload), repair=spec.repair) as document:
        return tuple(
            _node(
                NodeKind.RUN,
                "word",
                index,
                word["text"].encode(),
                path=(index, ordinal),
                bounds=(word["x0"], word["top"], word["x1"], word["bottom"]),
                text=word["text"],
                font_key=word.get("fontname", _RECOVERED_FONT),
                size=float(word.get("size", 0.0)),
            )
            for index, page in enumerate(document.pages)
            for ordinal, word in enumerate(
                page.extract_words(
                    x_tolerance=spec.x_tolerance,
                    y_tolerance=spec.y_tolerance,
                    use_text_flow=spec.use_text_flow,
                    split_at_punctuation=spec.split_at_punctuation,
                    extra_attrs=list(spec.extra_attrs),
                )
            )
        )


@beartype
def _chars_arm(payload: bytes, _provider: LensProvider, _spec: LensSpec) -> tuple[DocumentNode, ...]:
    # per-glyph forensic geometry: one RunNode per TextChar; the marked-content MCID rides the role
    # (`mcid-N`) so the tag/emit MCID agreement is auditable from the recovered tree alone.
    with pdf_oxide.PdfDocument.from_bytes(payload) as document:
        return tuple(
            _node(
                NodeKind.RUN,
                f"mcid-{glyph.mcid}" if glyph.mcid is not None else "char",
                page.index,
                glyph.char.encode(),
                path=(page.index, ordinal),
                bounds=glyph.bbox,
                text=glyph.char,
                font_key=glyph.font_name or _RECOVERED_FONT,
                size=glyph.font_size,
                italic=glyph.is_italic,
            )
            for page in document.pages
            for ordinal, glyph in enumerate(document.extract_chars(page.index))
        )


@beartype
def _region_arm(payload: bytes, provider: LensProvider, spec: LensSpec) -> tuple[DocumentNode, ...]:
    bbox = spec.bbox or _ORIGIN
    if provider is LensProvider.PDFOXIDE:  # commercial-safe default: `(x0,y0,x1,y1)` bounds -> pdf_oxide `(x,y,w,h)` region -> per-line RunNode
        x0, y0, x1, y1 = bbox
        with pdf_oxide.PdfDocument.from_bytes(payload) as document:
            return tuple(
                _node(NodeKind.RUN, "region", page.index, line.text.encode(), path=(page.index, ordinal), bounds=line.bbox, text=line.text)
                for page in document.pages
                for ordinal, line in enumerate(page.region(x0, y0, x1 - x0, y1 - y0).extract_text_lines())
            )
    with pdfplumber.open(BytesIO(payload), repair=spec.repair) as document:
        return tuple(
            _node(
                NodeKind.RUN,
                "region",
                index,
                line["text"].encode(),
                path=(index, ordinal),
                bounds=(line["x0"], line["top"], line["x1"], line["bottom"]),
                text=line["text"],
            )
            for index, page in enumerate(document.pages)
            for ordinal, line in enumerate(page.within_bbox(bbox).extract_text_lines(strip=True, return_chars=False))
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
                    path=(index,),
                    bounds=tuple(page.rect),
                    children=tuple(
                        _node(
                            NodeKind.BLOCK,
                            "block",
                            index,
                            repr(block["bbox"]).encode(),
                            path=(index, slot),
                            bounds=tuple(block["bbox"]),
                            block=BlockKind.PARAGRAPH,
                            runs=tuple(
                                _node(
                                    NodeKind.RUN,
                                    "span",
                                    index,
                                    span["text"].encode(),
                                    path=(index, slot, ordinal),
                                    bounds=tuple(span["bbox"]),
                                    text=span["text"],
                                    font_key=span.get("font", _RECOVERED_FONT),
                                    size=float(span.get("size", 0.0)),
                                    weight=700 if int(span.get("flags", 0)) & _BOLD_FLAG else 400,
                                    color=_rgb(int(span.get("color", 0))),
                                )
                                for ordinal, span in enumerate(span for line in block.get("lines", ()) for span in line.get("spans", ()))
                            ),
                        )
                        for slot, block in enumerate(page.get_text("dict", flags=spec.flags or pymupdf.TEXTFLAGS_DICT)["blocks"])
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
                path=(page.index,),
                bounds=page.bbox,
                children=tuple(
                    _node(
                        NodeKind.RUN,
                        "span",
                        page.index,
                        span.text.encode(),
                        path=(page.index, ordinal),
                        bounds=span.bbox,
                        text=span.text,
                        font_key=span.font_name or _RECOVERED_FONT,
                        size=span.font_size,
                        weight=700 if span.is_bold else 400,
                        italic=span.is_italic,
                        color=_scale8(span.color),
                    )
                    for ordinal, span in enumerate(document.extract_spans(page.index, reading_order=spec.reading_order))
                ),
            )
            for page in document.pages
        )


@beartype
def _paths_arm(payload: bytes, provider: LensProvider, spec: LensSpec) -> tuple[DocumentNode, ...]:
    # recovered vector geometry (the AEC drawing plane's recovered linework) -> path-keyed FigureNode leaves carrying the
    # path bbox as `intrinsic` + the vector `media_type`; a recovered drawing is a graphic figure, never a text run.
    if provider is LensProvider.MUPDF:
        with pymupdf.open(stream=payload, filetype="pdf") as document:
            return tuple(
                _node(
                    NodeKind.FIGURE,
                    str(path.get("type", "f")),
                    index,
                    repr(rect).encode(),
                    path=(index, ordinal),
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
                path=(index, band, ordinal),
                bounds=(obj["x0"], obj["top"], obj["x1"], obj["bottom"]),
                name=f"{kind}-{index}-{ordinal}",
                media_type=_VECTOR_MEDIA,
                intrinsic=(obj["x1"] - obj["x0"], obj["bottom"] - obj["top"]),
            )
            for index, page in enumerate(document.pages)
            for band, (kind, objects) in enumerate((("curve", page.curves), ("rect", page.rects), ("line", page.lines)))
            for ordinal, obj in enumerate(objects)
        )


@beartype
def _layout_arm(payload: bytes, _provider: LensProvider, _spec: LensSpec) -> tuple[DocumentNode, ...]:
    # computed per-page layout metrics -> FieldNode rows under one per-page structure node; intake-triage evidence.
    with pdf_oxide.PdfDocument.from_bytes(payload) as document:
        return tuple(
            _node(
                NodeKind.STRUCTURE,
                "layout",
                page.index,
                repr(rows).encode(),
                path=(page.index,),
                elt=StructEltKind.SECT,
                children=tuple(
                    _node(NodeKind.FIELD, name, page.index, str(value).encode(), path=(page.index, slot), name=name, field=TextField(value=str(value)))
                    for slot, (name, value) in enumerate(rows)
                ),
            )
            for page in document.pages
            for metrics in (document.page_layout_params(page.index),)
            for rows in (
                (
                    ("column_count", metrics.column_count),
                    ("median_font_size", metrics.median_font_size),
                    ("median_line_spacing", metrics.median_line_spacing),
                    ("median_char_width", metrics.median_char_width),
                    ("word_gap_threshold", metrics.word_gap_threshold),
                    ("line_gap_threshold", metrics.line_gap_threshold),
                ),
            )
        )


@beartype
def _classify_arm(payload: bytes, _provider: LensProvider, _spec: LensSpec) -> tuple[DocumentNode, ...]:
    # document + per-page classification (scanned/born-digital/form …): the engine returns JSON verdict strings,
    # decoded through the one msgspec codec, never a rendered-string regex; per-page kind/confidence/reason land as fields.
    with pdf_oxide.PdfDocument.from_bytes(payload) as document:
        overall = msgspec.json.decode(document.classify_document())
        verdicts = tuple(msgspec.json.decode(document.classify_page(page.index)) for page in document.pages)
    pages = tuple(
        _node(
            NodeKind.STRUCTURE,
            "page-class",
            int(verdict.get("page", index)),
            repr(verdict).encode(),
            path=(index,),
            elt=StructEltKind.SECT,
            children=tuple(
                _node(NodeKind.FIELD, name, int(verdict.get("page", index)), str(verdict.get(name, "")).encode(), path=(index, slot), name=name, field=TextField(value=str(verdict.get(name, ""))))
                for slot, name in enumerate(("kind", "confidence", "reason"))
            ),
        )
        for index, verdict in enumerate(verdicts)
    )
    summary = _node(
        NodeKind.FIELD, "summary", 0, str(overall.get("summary", "")).encode(), path=(-1,), name="summary", field=TextField(value=str(overall.get("summary", "")))
    )
    needing_ocr = ",".join(str(page) for page in overall.get("pages_needing_ocr", ()))  # the verified document-verdict key naming the pages the intake must OCR
    ocr_rows = _node(NodeKind.FIELD, "pages_needing_ocr", 0, needing_ocr.encode(), path=(-2,), name="pages_needing_ocr", field=TextField(value=needing_ocr))
    return (summary, ocr_rows, *pages)


@beartype
def _layers_arm(payload: bytes, _provider: LensProvider, _spec: LensSpec) -> tuple[DocumentNode, ...]:
    # OCG optional-content layer census — the read-side counterpart of `export/layered#LAYERED` authoring.
    with pdf_oxide.PdfDocument.from_bytes(payload) as document:
        return tuple(
            _node(NodeKind.FIELD, "layer", 0, name.encode(), path=(ordinal,), name=name, field=TextField(value=name))
            for ordinal, name in enumerate(document.get_layers())
        )


@beartype
def _inks_arm(payload: bytes, _provider: LensProvider, spec: LensSpec) -> tuple[DocumentNode, ...]:
    # separation-ink census reads the policy-selected direct or nested-content surface without reconstructing a boolean knob.
    with pdf_oxide.PdfDocument.from_bytes(payload) as document:
        return tuple(
            _node(NodeKind.FIELD, "ink", page.index, str(name).encode(), path=(page.index, ordinal), name=str(name), field=TextField(value=str(name)))
            for page in document.pages
            for ordinal, name in enumerate(spec.ink_depth.read(document, page.index))
        )


@beartype
def _labels_arm(payload: bytes, _provider: LensProvider, _spec: LensSpec) -> tuple[DocumentNode, ...]:
    # Page-label numbering tree ("iii", "A-1" …) — one RunNode per label in page order.
    with pdf_oxide.PdfDocument.from_bytes(payload) as document:
        return tuple(
            _node(NodeKind.RUN, "page-label", ordinal, str(label).encode(), path=(ordinal,), text=str(label))
            for ordinal, label in enumerate(document.page_labels() or ())
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
    if provider is LensProvider.PDFOXIDE:
        # native Rust table grid: each row dict carries `rows[].cells[].text` + `is_header`; the `(x, y, w, h)` bbox converts once.
        with pdf_oxide.PdfDocument.from_bytes(payload) as document:
            return tuple(
                _table_node(
                    [[cell.get("text") or None for cell in row["cells"]] for row in table["rows"]],
                    (bx, by, bx + bw, by + bh),
                    page.index,
                    (page.index, ordinal),
                    header_rows=sum(bool(row["is_header"]) for row in table["rows"]),
                )
                for page in document.pages
                for ordinal, table in enumerate(document.extract_tables(page.index))
                for (bx, by, bw, bh) in (table.get("bbox", _ORIGIN),)
            )
    if provider is LensProvider.MUPDF:
        with pymupdf.open(stream=payload, filetype="pdf") as document:
            return tuple(
                _table_node(table.extract(), tuple(table.bbox), index, (index, ordinal), header_rows=0 if table.header.external else 1)
                for index, page in enumerate(document)
                for ordinal, table in enumerate(
                    page.find_tables(**settings).tables
                )  # `Table.header` is always a truthy `TableHeader`; `.external` is the real discriminant — an above-body synthesized header is NOT in `extract()` rows (0), an in-grid header row is (1)
            )
    plumber = settings | {
        key: list(value)
        for key, value in (("explicit_vertical_lines", spec.explicit_vertical), ("explicit_horizontal_lines", spec.explicit_horizontal))
        if value
    }
    with pdfplumber.open(BytesIO(payload), repair=spec.repair) as document:
        return tuple(
            _table_node(table.extract(), tuple(table.bbox), index, (index, ordinal), spans=_plumber_spans(table))
            for index, page in enumerate(document.pages)
            for ordinal, table in enumerate(page.find_tables(table_settings=plumber))
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
            return tuple(_outline_node(level, title, page, (ordinal,)) for ordinal, (level, title, page) in enumerate(document.get_toc(simple=True)))
    reader = pypdf.PdfReader(BytesIO(payload))
    return tuple(_pypdf_outline(reader.outline))


def _pypdf_outline(items: Iterable[object], /) -> Iterator[DocumentNode]:
    stack, emitted = [(item, _HEADING_FLOOR) for item in reversed(tuple(items))], 0
    while stack:  # Exemption: iterative flatten of the untrusted nested outline list — adversarial nesting forfeits recursion
        item, level = stack.pop()
        if isinstance(item, list):
            stack.extend((kid, level + 1) for kid in reversed(item))
        else:
            yield _outline_node(level, item.title, 0, (emitted,))
            emitted += 1


@beartype
def _structure_arm(payload: bytes, provider: LensProvider, _spec: LensSpec) -> tuple[DocumentNode, ...]:
    if provider is LensProvider.PLUMBER:
        with pdfplumber.open(BytesIO(payload)) as document:
            return tuple(
                _grown(branch, _plumber_kids, lambda node, path, grown: _plumber_built(node, path, grown, index), base=(index, ordinal))
                for index, page in enumerate(document.pages)
                for ordinal, branch in enumerate(page.structure_tree)
            )
    with pikepdf.open(BytesIO(payload)) as pdf:
        root = pdf.Root.get(pikepdf.Name.StructTreeRoot)
        kids = root.get(pikepdf.Name.K, pikepdf.Array()) if root is not None else ()
        return tuple(_grown(kid, _pike_kids, _pike_built, base=(ordinal,)) for ordinal, kid in enumerate(kids))


def _plumber_kids(branch: object) -> tuple[object, ...]:
    return tuple(child for child in branch.get("children", ()) if isinstance(child, Mapping)) if isinstance(branch, Mapping) else ()


def _plumber_built(branch: object, path: Trail, grown: tuple[DocumentNode, ...], page: int) -> DocumentNode:
    name = str(branch.get("type", "")) if isinstance(branch, Mapping) else ""
    return _node(NodeKind.STRUCTURE, name or "Sect", page, repr(branch).encode(), path=path, struct_role=_struct_role(name), children=grown)


def _pike_kids(obj: object) -> tuple[object, ...]:
    if not isinstance(obj, pikepdf.Dictionary):
        return ()
    kids = obj.get(pikepdf.Name.K, pikepdf.Array())
    members = kids if isinstance(kids, pikepdf.Array) else (kids,) if isinstance(kids, pikepdf.Dictionary) else ()
    return tuple(kid for kid in members if isinstance(kid, pikepdf.Dictionary))


def _pike_built(obj: object, path: Trail, grown: tuple[DocumentNode, ...]) -> DocumentNode:
    if not isinstance(obj, pikepdf.Dictionary):
        return _node(NodeKind.STRUCTURE, "Sect", 0, str(obj).encode(), path=path, struct_role=StandardRole(elt=StructEltKind.SECT))
    name = str(obj.get(pikepdf.Name.S, "")).removeprefix("/")
    return _node(NodeKind.STRUCTURE, name or "Sect", 0, str(obj).encode(), path=path, struct_role=_struct_role(name), children=grown)


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
                    path=(index, ordinal),
                    bounds=tuple(link["from"]),
                    annot=AnnotKind.LINK,
                    contents=str(link.get("uri", "")),
                    link=_link_target(link),
                )
                for index, page in enumerate(document)
                for ordinal, link in enumerate(page.get_links())
            )
    with pdfplumber.open(BytesIO(payload), repair=spec.repair) as document:
        return tuple(
            _node(
                NodeKind.ANNOTATION,
                "link",
                index,
                str(hit.get("uri", "")).encode(),
                path=(index, ordinal),
                bounds=(hit["x0"], hit["top"], hit["x1"], hit["bottom"]),
                annot=AnnotKind.LINK,
                contents=str(hit.get("uri", "")),
                link=Uri(href=str(hit["uri"])),
            )
            for index, page in enumerate(document.pages)
            for ordinal, hit in enumerate(page.hyperlinks)
        )


@beartype
def _metadata_arm(payload: bytes, _provider: LensProvider, _spec: LensSpec) -> tuple[DocumentNode, ...]:
    reader = pypdf.PdfReader(BytesIO(payload))
    info = reader.metadata or {}
    fields = tuple(
        _node(
            NodeKind.FIELD,
            _META_KEY.try_find(name).default_value(name),
            0,
            str(value).encode(),
            path=(ordinal,),
            name=_META_KEY.try_find(name).default_value(name),
            field=TextField(value=str(value)),
        )
        for ordinal, (name, value) in enumerate(info.items())
        if value
    )
    root = _node(
        NodeKind.STRUCTURE,
        str(info.get("/Title", "document")),
        0,
        str(info.get("/Author", "")).encode(),
        path=(),
        elt=StructEltKind.DOCUMENT,
        children=fields,
    )
    return (root,)


@beartype
def _search_arm(payload: bytes, provider: LensProvider, spec: LensSpec) -> tuple[DocumentNode, ...]:
    needle = spec.needle
    if provider is LensProvider.MUPDF:
        with pymupdf.open(stream=payload, filetype="pdf") as document:
            return tuple(
                _node(NodeKind.ANNOTATION, "hit", index, needle.encode(), path=(index, ordinal), bounds=tuple(rect), annot=AnnotKind.HIGHLIGHT, contents=needle)
                for index, page in enumerate(document)
                for ordinal, rect in enumerate(page.search_for(needle))
            )
    with pdfplumber.open(BytesIO(payload), repair=spec.repair) as document:
        return tuple(
            _node(
                NodeKind.ANNOTATION,
                "hit",
                index,
                needle.encode(),
                path=(index, ordinal),
                bounds=(hit["x0"], hit["top"], hit["x1"], hit["bottom"]),
                annot=AnnotKind.HIGHLIGHT,
                contents=needle,
            )
            for index, page in enumerate(document.pages)
            for ordinal, hit in enumerate(page.search(needle, regex=spec.regex, case=spec.case_sensitive))
        )


@beartype
def _ocr_arm(payload: bytes, provider: LensProvider, spec: LensSpec) -> tuple[DocumentNode, ...]:
    if provider is LensProvider.PDFOXIDE:  # commercial-safe in-process OCR (Rust engine), CORE band — no ocrmypdf subprocess, no PDF/A rewrite
        with pdf_oxide.PdfDocument.from_bytes(payload) as document:
            return tuple(
                _node(NodeKind.RUN, "ocr", page.index, line.encode(), path=(page.index, ordinal), text=line)
                for page in document.pages
                for ordinal, line in enumerate(document.extract_text_ocr(page.index).splitlines())
                if line
            )
    if provider is LensProvider.MUPDF:
        with pymupdf.open(stream=payload, filetype=spec.filetype) as document:
            language = "+".join(spec.language)
            return tuple(
                _node(NodeKind.RUN, "ocr", index, word[4].encode(), path=(index, ordinal), bounds=(word[0], word[1], word[2], word[3]), text=word[4])
                for index, page in enumerate(document)
                for ordinal, word in enumerate(page.get_text("words", textpage=page.get_textpage_ocr(language=language, dpi=spec.dpi, full=spec.full)))
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
        _node(NodeKind.STRUCTURE, code.name, 0, code.name.encode(), path=(-1,), elt=StructEltKind.NOTE),
        *(_node(NodeKind.RUN, "ocr", 0, line.encode(), path=(ordinal,), text=line) for ordinal, line in enumerate(text.splitlines()) if line),
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
                path=(-1 - ordinal,),
                name=name,
                field=TextField(value=document.embfile_info(name)["filename"]),
            )
            for ordinal, name in enumerate(document.embfile_names())
        )
        figures = tuple(
            _node(
                NodeKind.FIGURE,
                "placed",
                index,
                pix.tobytes("png"),
                path=(index, ordinal),
                name=f"img-{xref}",
                intrinsic=(float(pix.width), float(pix.height)),
                media_type="image/png",
            )
            for index, page in enumerate(document)
            for ordinal, (xref, *_rest) in enumerate(page.get_images(full=True))
            if (pix := pymupdf.Pixmap(document, xref))
        )
    return fields + figures


@beartype
def _widget_arm(payload: bytes, provider: LensProvider, _spec: LensSpec) -> tuple[DocumentNode, ...]:
    if provider is LensProvider.PDFOXIDE:  # commercial-safe AcroForm recovery WITH the required/readonly policy pymupdf's widget accessor never exposed
        with pdf_oxide.PdfDocument.from_bytes(payload) as document:
            return tuple(
                _node(
                    NodeKind.FIELD,
                    form.name,
                    0,
                    str(form.value).encode(),
                    path=(ordinal,),
                    name=form.name,
                    field=_field_value(form.field_type, form.name, form.value, max_length=form.max_length),
                    required=form.is_required,
                    readonly=form.is_readonly,
                    tooltip=form.tooltip or "",
                )
                for ordinal, form in enumerate(document.get_form_fields())
            )
    with pymupdf.open(stream=payload, filetype="pdf") as document:
        return tuple(
            _node(
                NodeKind.FIELD,
                widget.field_name,
                index,
                str(widget.field_value).encode(),
                path=(index, ordinal),
                name=widget.field_name,
                field=_field_value(_widget_token(pymupdf, widget.field_type), widget.field_name, widget.field_value),
            )
            for index, page in enumerate(document)
            for ordinal, widget in enumerate(page.widgets())
        )


@beartype
def _form_data_arm(payload: bytes, _provider: LensProvider, spec: LensSpec) -> tuple[DocumentNode, ...]:
    # FDF/XFDF filled-form export: the structured form-data product a delivery plane hands downstream — the
    # exported grammar rides one CODE leaf; `export_form_data` is path-shaped, so the tempfile is its one seam.
    with pdf_oxide.PdfDocument.from_bytes(payload) as document, NamedTemporaryFile(suffix=f".{spec.form_format}", delete_on_close=False) as sink:
        sink.close()  # release the handle first — the provider writes a fresh file at the path, not into the open descriptor
        document.export_form_data(sink.name, format=spec.form_format)
        exported = Path(sink.name).read_bytes()
    # exported octets ride the node payload VERBATIM — the leaf is the delivery product, never a re-encoded projection;
    # `text` decodes strict UTF-8 (XFDF's XML contract) and falls back to latin-1 for FDF's PDFDoc byte strings, the
    # bijective single-byte decode whose re-encode reproduces every octet — `errors="replace"` is the deleted lossy form.
    try:
        projected = exported.decode()
    except UnicodeDecodeError:
        projected = exported.decode("latin-1")
    return (_node(NodeKind.BLOCK, spec.form_format, 0, exported, path=(), block=BlockKind.CODE, runs=(_node(NodeKind.RUN, "form-data", 0, exported, path=(0,), text=projected),)),)


@beartype
def _annotate_arm(payload: bytes, _provider: LensProvider, _spec: LensSpec) -> tuple[DocumentNode, ...]:
    with pymupdf.open(stream=payload, filetype="pdf") as document:
        return tuple(
            _node(
                NodeKind.ANNOTATION,
                "annotation",
                index,
                annot.info["content"].encode(),
                path=(index, ordinal),
                bounds=tuple(annot.rect),
                annot=_annot_kind(annot.type[1]),
                contents=annot.info["content"],
            )
            for index, page in enumerate(document)
            for ordinal, annot in enumerate(page.annots())
        )


def _widget_token(module: object, code: int) -> str:
    # SYMBOLIC_REFERENCE: the catalogued `PDF_WIDGET_TYPE_*` symbol names resolve to their int on the live module,
    # then land in the same canonical token space the pdf_oxide `field_type` strings inhabit — one builder table serves both engines.
    return {getattr(module, name): token for name, token in _WIDGET_TOKEN.items()}.get(code, "text")


def _field_value(token: str, name: str, value: object, /, *, max_length: int | None = None) -> FieldValue:
    built = _FIELD_BUILDERS.try_find(token).default_value(_FIELD_BUILDERS["text"])(name, value)
    # `/MaxLen` is a text-field axis alone; a provider `None` keeps the unbounded default.
    return structs.replace(built, max_length=max_length) if isinstance(built, TextField) and max_length is not None else built


def _choice(value: object) -> tuple[str, ...]:
    # a recovered choice field carries its selected value as the one provable option — neither engine exposes the authored option roster.
    return (str(value),) if value not in (None, "") else ("",)


def _selected(value: object) -> str:
    # explicit absence check: a numeric 0 or "0" provider selection survives where a `value or ""` truthiness fold erases it.
    return "" if value is None else str(value)


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
            (index,),
            role=sheet.getAttribute("name") or f"sheet-{index}",
        )
        for index, sheet in enumerate(document.getElementsByType(Table))
    )


@beartype
def _odt_arm(payload: bytes, _provider: LensProvider, _spec: LensSpec) -> tuple[DocumentNode, ...]:
    document = load(BytesIO(payload))
    blocks = tuple(_odt_blocks(document.body.childNodes))
    return (_node(NodeKind.STRUCTURE, "document", 0, b"odt", path=(), elt=StructEltKind.DOCUMENT, children=blocks),)


def _odt_blocks(nodes: Iterable[object]) -> Iterator[DocumentNode]:
    stack, emitted = list(nodes)[::-1], 0
    while stack:  # Exemption: iterative flatten of the untrusted ODF container graph — adversarial nesting forfeits recursion
        node = stack.pop()
        probe = getattr(node, "isInstanceOf", None)  # `text:p`/`text:h` are Elements; a Text leaf carries no `isInstanceOf`
        if probe is None:
            continue
        if probe(H):
            text = extractText(node)
            yield _node(
                NodeKind.SECTION,
                text or "section",
                0,
                text.encode(),
                path=(emitted,),
                level=int(node.getAttribute("outlinelevel") or 1),
                heading=(_node(NodeKind.RUN, "heading", 0, text.encode(), path=(emitted, 0), text=text),),
            )
            emitted += 1
        elif probe(P):
            text = extractText(node)
            yield _node(
                NodeKind.BLOCK,
                "paragraph",
                0,
                text.encode(),
                path=(emitted,),
                block=BlockKind.PARAGRAPH,
                runs=(_node(NodeKind.RUN, "run", 0, text.encode(), path=(emitted, 0), text=text),),
            )
            emitted += 1
        else:
            stack.extend(list(node.childNodes)[::-1])


@beartype
def _xlsx_arm(payload: bytes, _provider: LensProvider, spec: LensSpec) -> tuple[DocumentNode, ...]:
    # Rust-backed workbook closes deterministically once every selected sheet materializes — never a GC-reaped handle.
    with python_calamine.CalamineWorkbook.from_object(BytesIO(payload), load_tables=spec.load_tables) as workbook:
        sheets = spec.sheets or tuple(workbook.sheet_names)
        return tuple(
            _table_node(
                [[None if value is None else str(value) for value in row] for row in sheet.to_python(skip_empty_area=spec.skip_empty_area)],
                _ORIGIN,
                index,
                (index,),
                role=name,
                spans=_calamine_spans(sheet.merged_cell_ranges),
            )
            for index, name in enumerate(sheets)
            if (sheet := workbook.get_sheet_by_name(name))
        )


def _calamine_spans(ranges: object) -> Spans:
    # `is_bearable` narrows the provider's untyped merged-range payload once; a foreign shape yields the empty span set.
    if not is_bearable(ranges, list[tuple[tuple[int, int], tuple[int, int]]]):
        return ()
    return tuple((r0, c0, c1 - c0 + 1, r1 - r0 + 1) for (r0, c0), (r1, c1) in ranges if c1 > c0 or r1 > r0)


@beartype
def _docx_arm(payload: bytes, _provider: LensProvider, _spec: LensSpec) -> tuple[DocumentNode, ...]:
    document = docx.Document(BytesIO(payload))
    blocks = tuple(_docx_blocks(enumerate(document.iter_inner_content())))
    props = document.core_properties
    return (
        _node(NodeKind.STRUCTURE, props.title or "document", 0, (props.author or "").encode(), path=(), elt=StructEltKind.DOCUMENT, children=blocks),
    )


def _docx_blocks(indexed: Iterable[tuple[int, object]]) -> Iterator[DocumentNode]:
    for list_kind, group in groupby(indexed, key=lambda pair: _docx_list_kind(pair[1])):
        if list_kind is None:
            yield from (_docx_block(block, index) for index, block in group)
        else:
            rows = tuple(group)
            members = tuple(
                _node(NodeKind.BLOCK, "item", 0, block.text.encode(), path=(index,), block=BlockKind.PARAGRAPH, runs=_docx_runs(block, (index,)))
                for index, block in rows
            )
            yield _node(NodeKind.LIST, list_kind.value, 0, list_kind.value.encode(), path=(rows[0][0],), list_kind=list_kind, items=members)


def _docx_list_kind(block: object) -> ListKind | None:
    return _DOCX_LIST.try_find(getattr(getattr(block, "style", None), "name", "")).default_value(None)


def _docx_block(block: object, index: int) -> DocumentNode:
    if hasattr(block, "rows"):
        return _table_node([[cell.text for cell in row.cells] for row in block.rows], _ORIGIN, 0, (index,))
    style = getattr(block.style, "name", "")
    runs = _docx_runs(block, (index,))
    links = _docx_links(block, (index,))
    level = _DOCX_HEADING.try_find(style).default_value(None)
    if level is not None:
        return _node(NodeKind.SECTION, block.text, 0, block.text.encode(), path=(index,), level=level, heading=runs, children=links)
    return _node(
        NodeKind.BLOCK,
        "paragraph",
        0,
        block.text.encode(),
        path=(index,),
        block=_DOCX_BLOCK.try_find(style).default_value(BlockKind.PARAGRAPH),
        runs=runs,
        children=links,
    )


def _docx_runs(block: object, path: Trail) -> tuple[DocumentNode, ...]:
    return tuple(
        _node(
            NodeKind.RUN,
            "run",
            0,
            run.text.encode(),
            path=(*path, ordinal),
            text=run.text,
            font_key=run.font.name or (run.style.name if run.style else _RECOVERED_FONT),  # emit writes `font.name`; read its inverse
            size=run.font.size.pt if run.font.size else 0.0,
            weight=700 if run.bold else 400,
            italic=bool(run.italic),
            decorations=(TextDecoration.UNDERLINE,) if run.font.underline else (),
            color=tuple(rgb) if (rgb := run.font.color.rgb) else (0, 0, 0),
        )
        for ordinal, run in enumerate(block.runs)
    )


def _docx_links(block: object, path: Trail) -> tuple[DocumentNode, ...]:
    # `Paragraph.hyperlinks` recovery — the inverse of the emit hyperlink lowering; each link an AnnotationNode child.
    # A fragment-only hyperlink is an internal bookmark anchor with no page geometry, so it recovers as the URI
    # fragment reference `#<bookmark>` — the model has no named-destination case and a fabricated `Dest(page=0)` lies.
    return tuple(
        _node(
            NodeKind.ANNOTATION,
            "link",
            0,
            (link.address or link.fragment).encode(),
            path=(*path, -1 - ordinal),
            annot=AnnotKind.LINK,
            contents=link.text,
            link=Uri(href=link.address) if link.address else Uri(href=f"#{link.fragment}"),
        )
        for ordinal, link in enumerate(getattr(block, "hyperlinks", ()))
        if link.address or link.fragment
    )


@beartype
def _yaml_arm(payload: bytes, _provider: LensProvider, spec: LensSpec) -> tuple[DocumentNode, ...]:
    engine = YAML(typ=spec.typ)
    return tuple(_value_tree(document, "yaml", index) for index, document in enumerate(engine.load_all(BytesIO(payload))))


@beartype
def _toml_arm(payload: bytes, _provider: LensProvider, _spec: LensSpec) -> tuple[DocumentNode, ...]:
    return (_value_tree(tomlkit.parse(payload).unwrap(), "toml", 0),)


def _value_tree(value: object, role: str, page: int, /) -> DocumentNode:
    # labeled seeds carry each child's mapping key or ordinal into its built node; the frontier owns depth safety.
    return _grown((role, value), _value_kids, lambda seed, path, grown: _value_built(seed, path, grown, page), base=(page,))


def _value_kids(seed: object) -> tuple[object, ...]:
    match seed[1]:
        case Mapping() as mapping:
            return tuple((str(name), child) for name, child in mapping.items())
        case list() | tuple() as items:
            return tuple((str(ordinal), child) for ordinal, child in enumerate(items))
        case _:
            return ()


def _value_built(seed: object, path: Trail, grown: tuple[DocumentNode, ...], page: int) -> DocumentNode:
    label, value = seed
    match value:
        case Mapping():
            return _node(NodeKind.BLOCK, label, page, repr(value).encode(), path=path, block=BlockKind.PARAGRAPH, children=grown)
        case list() | tuple():
            return _node(NodeKind.LIST, label, page, repr(value).encode(), path=path, list_kind=ListKind.ORDERED, items=grown)
        case _:
            text = "" if value is None else str(value)
            return _node(NodeKind.RUN, label, page, text.encode(), path=path, text=text)


@beartype
def _xml_arm(payload: bytes, _provider: LensProvider, spec: LensSpec) -> tuple[DocumentNode, ...]:
    parser = etree.XMLParser(recover=spec.recover, resolve_entities=False, no_network=True, huge_tree=False)
    return (_grown(etree.fromstring(payload, parser=parser), _xml_kids, _xml_built),)


def _xml_kids(element: object) -> tuple[object, ...]:
    return tuple(child for child in element if isinstance(child.tag, str))


def _xml_built(element: object, path: Trail, grown: tuple[DocumentNode, ...]) -> DocumentNode:
    name = etree.QName(element).localname if isinstance(element.tag, str) else "comment"
    text = (element.text or "").strip()
    runs = (_node(NodeKind.RUN, name, 0, text.encode(), path=(*path, -1), text=text),) if text else ()
    # mixed content: each child's `.tail` is the parent's text after that child — woven in sibling order,
    # a tail run slotted at the negative ordinal past the head run so path slots stay unique and deterministic.
    woven = tuple(
        part
        for ordinal, (child, node) in enumerate(zip(_xml_kids(element), grown, strict=True))
        for part in (
            node,
            *(
                (_node(NodeKind.RUN, name, 0, tail.encode(), path=(*path, -(ordinal + 2)), text=tail),)
                if (tail := (child.tail or "").strip())
                else ()
            ),
        )
    )
    return _node(NodeKind.BLOCK, name, 0, etree.tostring(element).strip(), path=path, block=BlockKind.PARAGRAPH, level=1, children=(*runs, *woven))


def _lens_raise(fault: object) -> tuple[DocumentNode, ...]:
    # terminal collapse at the extraction boundary: an offload fault reconstructs the raise the node's rail folds.
    raise ValueError(str(fault))


def _gated_recover(lens: "DocumentLens") -> tuple[DocumentNode, ...]:
    route = _ROUTES[lens.op]
    return route.arm(lens.payload, lens.provider, lens.spec)


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
_WIDGET_TOKEN: Final[Map[str, str]] = Map.of_seq([
    ("PDF_WIDGET_TYPE_TEXT", "text"),
    ("PDF_WIDGET_TYPE_CHECKBOX", "checkbox"),
    ("PDF_WIDGET_TYPE_RADIOBUTTON", "radio"),
    ("PDF_WIDGET_TYPE_LISTBOX", "listbox"),
    ("PDF_WIDGET_TYPE_COMBOBOX", "combobox"),
    ("PDF_WIDGET_TYPE_BUTTON", "button"),
    ("PDF_WIDGET_TYPE_SIGNATURE", "signature"),
])
# canonical token -> model `FieldValue` case builder over `(name, value)`; the pdf_oxide `field_type` strings ARE the token
# space and `_widget_token` folds pymupdf's int discriminant into it. A raw AcroForm /Btn reads back as the reflection-proven
# `"button"` token, so the bool-valued button discriminates to `CheckboxField` on the value shape; total `.default_value("text")`.
# provider values normalize before conversion: a string "false"/"0"/"off"/"no" reads unchecked where `bool(value)`
# reads any non-empty string as checked, and a numeric zero survives into the text value where `value or ""` erases it.
_UNCHECKED: Final[frozenset[str]] = frozenset({"", "0", "false", "off", "no"})
_FIELD_BUILDERS: Final[Map[str, Callable[[str, object], FieldValue]]] = Map.of_seq([
    ("text", lambda _name, value: TextField(value="" if value is None else str(value))),
    ("checkbox", lambda _name, value: CheckboxField(checked=value.strip().lower() not in _UNCHECKED if isinstance(value, str) else bool(value))),
    ("radio", lambda _name, value: RadioField(options=_choice(value), selected=_selected(value))),
    ("listbox", lambda _name, value: ListField(options=_choice(value), selected=(sel,) if (sel := _selected(value)) else ())),
    ("combobox", lambda _name, value: ComboField(options=_choice(value), selected=_selected(value))),
    ("choice", lambda _name, value: ComboField(options=_choice(value), selected=_selected(value))),
    ("push_button", lambda name, _value: ButtonField(label=name)),
    ("button", lambda name, value: CheckboxField(checked=value) if isinstance(value, bool) else ButtonField(label=name)),
    ("signature", lambda _name, _value: SignatureField()),
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
_ROUTES: Final[Map[LensOp, Route]] = Map.of_seq([
    (LensOp.EXTRACT_TEXT, Route(_text_arm, LensProvider.PDFOXIDE, frozenset({LensProvider.PDFOXIDE, LensProvider.MUPDF, LensProvider.PYPDF}), ("mode", "reading_order", "flags"))),
    (LensOp.EXTRACT_IMAGES, Route(_images_arm, LensProvider.PYPDF, frozenset({LensProvider.PYPDF}))),
    (LensOp.TABLE, Route(_table_arm, LensProvider.PLUMBER, frozenset({LensProvider.PDFOXIDE, LensProvider.MUPDF, LensProvider.PLUMBER}), ("vertical", "horizontal", "snap_tolerance", "join_tolerance", "edge_min_length", "intersection_tolerance", "text_tolerance", "explicit_vertical", "explicit_horizontal", "repair"))),
    (LensOp.WORDS, Route(_words_arm, LensProvider.PDFOXIDE, frozenset({LensProvider.PDFOXIDE, LensProvider.PLUMBER}), ("include_artifacts", "profile", "x_tolerance", "y_tolerance", "use_text_flow", "split_at_punctuation", "extra_attrs", "repair"))),
    (LensOp.CHARS, Route(_chars_arm, LensProvider.PDFOXIDE, frozenset({LensProvider.PDFOXIDE}))),
    (LensOp.REGION, Route(_region_arm, LensProvider.PDFOXIDE, frozenset({LensProvider.PDFOXIDE, LensProvider.PLUMBER}), ("bbox", "repair"))),
    (LensOp.STORY, Route(_story_arm, LensProvider.PDFOXIDE, frozenset({LensProvider.PDFOXIDE, LensProvider.MUPDF}), ("reading_order", "flags"))),
    (LensOp.PATHS, Route(_paths_arm, LensProvider.PLUMBER, frozenset({LensProvider.PLUMBER, LensProvider.MUPDF}), ("repair",))),
    (LensOp.LAYOUT, Route(_layout_arm, LensProvider.PDFOXIDE, frozenset({LensProvider.PDFOXIDE}))),
    (LensOp.CLASSIFY, Route(_classify_arm, LensProvider.PDFOXIDE, frozenset({LensProvider.PDFOXIDE}))),
    (LensOp.LAYERS, Route(_layers_arm, LensProvider.PDFOXIDE, frozenset({LensProvider.PDFOXIDE}))),
    (LensOp.INKS, Route(_inks_arm, LensProvider.PDFOXIDE, frozenset({LensProvider.PDFOXIDE}), ("ink_depth",))),
    (LensOp.PAGE_LABELS, Route(_labels_arm, LensProvider.PDFOXIDE, frozenset({LensProvider.PDFOXIDE}))),
    (LensOp.OUTLINE, Route(_outline_arm, LensProvider.MUPDF, frozenset({LensProvider.MUPDF, LensProvider.PYPDF}))),
    (LensOp.STRUCTURE, Route(_structure_arm, LensProvider.PLUMBER, frozenset({LensProvider.PLUMBER, LensProvider.PIKEPDF}))),
    (LensOp.LINK, Route(_link_arm, LensProvider.MUPDF, frozenset({LensProvider.MUPDF, LensProvider.PLUMBER}), ("repair",))),
    (LensOp.METADATA, Route(_metadata_arm, LensProvider.PYPDF, frozenset({LensProvider.PYPDF}))),
    (LensOp.SEARCH, Route(_search_arm, LensProvider.PLUMBER, frozenset({LensProvider.MUPDF, LensProvider.PLUMBER}), ("needle", "regex", "case_sensitive", "repair"))),
    (LensOp.OCR, Route(_ocr_arm, LensProvider.PDFOXIDE, frozenset({LensProvider.PDFOXIDE, LensProvider.MUPDF, LensProvider.OCRMYPDF}), ("language", "dpi", "full", "filetype", "output_type", "ocr_mode", "deskew", "clean", "rotate_pages", "optimize"))),
    (LensOp.EMBEDDED, Route(_embedded_arm, LensProvider.MUPDF, frozenset({LensProvider.MUPDF}))),
    (LensOp.WIDGET, Route(_widget_arm, LensProvider.PDFOXIDE, frozenset({LensProvider.PDFOXIDE, LensProvider.MUPDF}))),
    (LensOp.FORM_DATA, Route(_form_data_arm, LensProvider.PDFOXIDE, frozenset({LensProvider.PDFOXIDE}), ("form_format",))),
    (LensOp.ANNOTATE, Route(_annotate_arm, LensProvider.MUPDF, frozenset({LensProvider.MUPDF}))),
    (LensOp.XLSX_READ, Route(_xlsx_arm, LensProvider.CALAMINE, frozenset({LensProvider.CALAMINE}), ("load_tables", "sheets", "skip_empty_area"))),
    (LensOp.ODS_READ, Route(_ods_arm, LensProvider.ODFPY, frozenset({LensProvider.ODFPY}))),
    (LensOp.ODT_READ, Route(_odt_arm, LensProvider.ODFPY, frozenset({LensProvider.ODFPY}))),
    (LensOp.DOCX_READ, Route(_docx_arm, LensProvider.DOCX, frozenset({LensProvider.DOCX}))),
    (LensOp.YAML_READ, Route(_yaml_arm, LensProvider.RUAMEL, frozenset({LensProvider.RUAMEL}), ("typ",))),
    (LensOp.TOML_READ, Route(_toml_arm, LensProvider.TOMLKIT, frozenset({LensProvider.TOMLKIT}))),
    (LensOp.XML_READ, Route(_xml_arm, LensProvider.LXML, frozenset({LensProvider.LXML}), ("recover",))),
])
```

## [03]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

- [OXIDE_FIELD_FF]-[OPEN]: `pdf_oxide.FormField.flags` reflects as `None` on a flag-free field — does it carry the AcroForm `Ff` bitmask int when set (bit 13 multiline, bit 14 password), so the WIDGET `"text"` builder can select `TextField(mode=TextMode.MULTILINE/PASSWORD)`; verify via `uv run python` over an authored multiline/password form.

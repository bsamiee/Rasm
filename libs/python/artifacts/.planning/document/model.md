# [PY_ARTIFACTS_MODEL]

`DocumentNode` is the single semantic document representation the `document` axis lowers FROM and recovers TO. One recursive `msgspec` tagged-union tree carries a closed `NodeMeta` on every node, and `DocumentDelta` is the sole diff/merge edit algebra keyed by the stable content key; every `document/emit#DOCUMENT` backend folds from this tree and `document/lens#LENS` rebuilds it, so production and extraction are inverses over one node algebra and a structural object-graph diff reuses the delta defined here once.

One deterministic `msgspec` codec round-trips the tree, so a multi-PDF corpus is a content-keyed serialized value; identity comes from `ContentIdentity.key`, never re-minted. `CorpusRow`/`CorpusRecord` egress projections feed the `data/tabular/columnar#SCAN` wire; `NodeMeta.classification` carries the CSI/OmniClass notation as a bounded string, so the substrate tree never depends on the `specification/classify#CODE` folder that lowers INTO it; `TextDirection` rides as interior data while the `typography/shape#SHAPE` shaper owns the bidi reorder.

## [01]-[INDEX]

- [01]-[NODE]: the recursive tagged-union tree, its `StructRole` PDF/UA vocabulary, and the projection algebra every backend lowers from.
- [02]-[DELTA]: the five-variant edit algebra with `diff`/`merge`/`invert` defined once over the tree.

## [02]-[NODE]

- Owner: `DocumentNode` — one `tag`-discriminated `Union` on `tag_field="kind"`, every variant a frozen `Struct` carrying a `NodeMeta` value object; a flat class with a `kind: str` field and an `if kind ==` cascade is the rejected non-total shape.
- Cases: `TableNode` carries `spans` merged-cell quads plus `header_rows`/`footer_rows`/`header_cols` counts BOTH lowerings honor and the lens recovers — `header_cols` is the row-header axis a PDF/UA complex table associates through `scope` — and a `caption` run sequence, so a publication table and an AEC schedule both title their grid. `FormulaNode` keeps the LaTeX source tree-resident with an optional `mathml` island (the ISO 14289 accessible-math representation the HTML lowering prefers), so an equation is source-addressable rather than only a pre-rendered `FigureNode`; `AnnotTarget` closes the link family with one `Xref` whose `Citation` payload selects a detail-on-sheet or classification-section coordinate without empty-field combinations. `FieldNode.field` is the closed per-mode `FieldValue` family for text, checkbox, radio, combo, list, signature, and button payloads; common required/read-only policy stays on the node while mode-only data stays on its case. East-Asian ruby/warichu content authors as `StructureNode` composition — a `RUBY` parent over `RB`/`RT`/`RP` children each carrying `RunNode` content — so the role vocabulary needs no dedicated node variant. `NodeMeta` carries the full ISO 14289 struct-element attribute set: `lang`, `actual_text`, `expansion` (the `/E` abbreviation expansion), and `associated` content keys (the PDF 2.0 `/AF` associated-files edge the `A_3A` deliverable seals).
- Entry: one shared deterministic `_ENCODER` serves the node digest, the corpus byte projection, and the public `encode` — never parallel identical instances; `decode` captures `msgspec.DecodeError` once as the closed `ModelFault` rail before the value enters the interior; `node_digest` content-addresses the tree through `ContentIdentity.key` (the bare-`ContentKey` mint — `ContentIdentity.of` returns the `RuntimeRail` and never feeds the digest fold); `json_schema` publishes the JSON Schema contract the `to_json` interchange consumer validates against.
- Auto: `node_digest` folds `_own_bytes` (the container's non-child fields) beside the child digests, so an identical sub-tree keys identically while a re-parametrized container re-keys rather than colliding on its unchanged children. `walk(node, prune=(TableNode,))` stops descent below any caller-named node type, so a lowering that owns a composite's interior never re-walks its cells as loose blocks. `alt_of` derives one `(AltText, AltStatus)` pair over the `FigureNode | FormulaNode` or-pattern, so the accessibility audit reads alt presence as one column predicate — and the Typst `_image` emitter writes `alt: none` for an un-authored figure, never a meaningless `alt: ""` that erases the `ABSENT` fact. `to_corpus(node, view)` is one view-keyed entrypoint whose `RECORD` projection exists because `pa.Table.from_pylist` rejects a `msgspec.Struct` — the producer owns the flat-record mapping so producer and consumer agree on the Arrow column shape. `_STRUCT_CATEGORY` is the one frozen behavior table keyed by `StructEltKind` carrying each role's `StructCategory` and `heading_level`, so the audit's nesting and heading-monotonicity checks fold one table row rather than a parallel `match` — it rides `frozendict`, never `Map`, because the first-wins inversion depends on declaration order and `Map` iterates key-sorted; `_STANDARD_FOR` derives from it by first-wins category inversion so the `document/tagged#ACCESS` `/RoleMap` foreign-to-standard lowering reads a derived row rather than a hand-kept parallel dict, `ForeignRole` is the one open arm over a `Meta`-constrained non-empty role, and `role_of`/`role_category`/`standard_for` are the model's one role-projection family the tagged owner consumes whole. `to_typst_source` escapes markup-context and string-context interpolations through one shared `maketrans` algebra, and a decorative `BlockKind.ARTIFACT` block lowers through `pdf.artifact[..]` so it is excluded from the tagged structure tree.
- Receipt: owns the tree type and its digest, never a receipt fold — authoring receipts stay at `document/emit`, recovery receipts at `document/lens`.
- Packages: `lxml.etree` defers under module-scope `lazy from`, so a Typst-only or corpus-only consumer never pays the libxml2 load; `msgspec` `UNSET` markers round-trip the wire-absent `NodeMeta` fields under `omit_defaults`.
- Growth: a new document concept is one variant plus one arm in each lowering the total `match` forces; a new standard PDF/UA role is one `StructEltKind` member plus one `_STRUCT_CATEGORY` row — `_STANDARD_FOR` absorbs a new category for free; a new run decoration, direction, baseline, list dialect, field mode, citation kind, or link-target kind is one vocabulary member or payload case plus its total lowering arm.
- Boundary: `to_json` is a real `msgspec.json` interchange serialization of the node tree a downstream consumer decodes — never a schema-shape blob no consumer reads; a Typst `label()`-anchored intra-compilation link is the rejected `Xref` form because the target sheet is a separate compilation the imposition assembly resolves; a `ClassCode` field on the interior tree is the rejected coupling that inverts the `specification`-to-`document` dependency. Recursion depth splits by provenance: `walk`/`node_digest` run depth-safe frontiers because they consume lens-recovered, potentially adversarial trees, while the `to_*` lowerings recurse natively — an authored document's structural nesting is data-bounded, and a lowering of a lens-recovered tree crosses `walk` first.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
from collections.abc import Iterator
from enum import StrEnum
from functools import reduce
from typing import TYPE_CHECKING, Annotated, Final, Literal, assert_never, overload

import msgspec
from expression import Result, pipe
from expression.collections import Block, Map
from expression.extra.result import catch
from builtins import frozendict
from msgspec import Meta, Struct, UnsetType, UNSET

from rasm.runtime.identity import ContentIdentity, ContentKey

lazy from lxml import etree

if TYPE_CHECKING:
    from lxml.etree import _Element

# --- [TYPES] ----------------------------------------------------------------------------


class NodeKind(StrEnum):
    PAGE = "page"
    SECTION = "section"
    BLOCK = "block"
    RUN = "run"
    LIST = "list"
    TABLE = "table"
    FIGURE = "figure"
    FORMULA = "formula"
    FIELD = "field"
    ANNOTATION = "annotation"
    STRUCTURE = "structure"


class BlockKind(StrEnum):
    PARAGRAPH = "paragraph"
    HEADING = "heading"
    QUOTE = "quote"
    CODE = "code"
    CAPTION = "caption"
    ARTIFACT = "artifact"


class ListKind(StrEnum):
    UNORDERED = "unordered"
    ORDERED = "ordered"
    DESCRIPTION = "description"


class RunScript(StrEnum):
    NORMAL = "normal"
    SUPER = "super"
    SUB = "sub"


class TextDirection(StrEnum):
    AUTO = "auto"
    LTR = "ltr"
    RTL = "rtl"


class TextDecoration(StrEnum):
    UNDERLINE = "underline"
    STRIKETHROUGH = "strikethrough"
    OVERLINE = "overline"


class TextMode(StrEnum):
    SINGLE = "single"
    MULTILINE = "multiline"
    PASSWORD = "password"


class ModelFault(StrEnum):
    DECODE = "decode"


class AnnotKind(StrEnum):
    HIGHLIGHT = "highlight"
    REDACTION = "redaction"
    LINK = "link"
    NOTE = "note"
    STAMP = "stamp"


class StructEltKind(StrEnum):
    DOCUMENT = "Document"
    PART = "Part"
    ART = "Art"
    SECT = "Sect"
    DIV = "Div"
    TOC = "TOC"
    TOCI = "TOCI"
    INDEX = "Index"
    NONSTRUCT = "NonStruct"
    PRIVATE = "Private"
    H1 = "H1"
    H2 = "H2"
    H3 = "H3"
    H4 = "H4"
    H5 = "H5"
    H6 = "H6"
    P = "P"
    BLOCKQUOTE = "BlockQuote"
    NOTE = "Note"
    BIBENTRY = "BibEntry"
    CODE = "Code"
    CAPTION = "Caption"
    SPAN = "Span"
    QUOTE = "Quote"
    LINK = "Link"
    REFERENCE = "Reference"
    ANNOT = "Annot"
    RUBY = "Ruby"
    RB = "RB"
    RT = "RT"
    RP = "RP"
    WARICHU = "Warichu"
    WT = "WT"
    WP = "WP"
    L = "L"
    LI = "LI"
    LBL = "Lbl"
    LBODY = "LBody"
    TABLE = "Table"
    THEAD = "THead"
    TBODY = "TBody"
    TFOOT = "TFoot"
    TR = "TR"
    TH = "TH"
    TD = "TD"
    FIGURE = "Figure"
    FORMULA = "Formula"
    FORM = "Form"


class StructCategory(StrEnum):
    GROUPING = "grouping"
    HEADING = "heading"
    BLOCK = "block"
    INLINE = "inline"
    LIST = "list"
    TABLE = "table"
    ILLUSTRATION = "illustration"


class AltStatus(StrEnum):
    PRESENT = "present"
    ABSENT = "absent"
    NA = "na"


class TypstScope(StrEnum):
    STRING = "string"
    MARKUP = "markup"


class CorpusView(StrEnum):
    STRUCT = "struct"
    BYTES = "bytes"
    RECORD = "record"


# --- [BOUNDARIES] -----------------------------------------------------------------------

type ForeignRoleStr = Annotated[str, Meta(min_length=1, max_length=64, pattern=r"\A[A-Za-z][\w.\-]*\Z")]
type ReferencePart = Annotated[str, Meta(min_length=1, max_length=64, pattern=r"\A[A-Za-z0-9][\w .\-]*\Z")]  # `cite()` interpolates into LaTeX labels, Typst links, and markdown anchors, so admission excludes every markup-special character
type FieldName = Annotated[str, Meta(min_length=1, max_length=256)]
type ChoiceOptions = Annotated[tuple[str, ...], Meta(min_length=1)]
type AltText = Annotated[str, Meta(max_length=2048)]
type LangTag = Annotated[str, Meta(min_length=2, max_length=35, pattern=r"\A[A-Za-z]{2,3}(-[A-Za-z0-9]{2,8})*\Z")]
type MediaType = Annotated[str, Meta(min_length=3, max_length=127, pattern=r"\A[\w.+-]+/[\w.+-]+\Z")]
type NonnegativeInt = Annotated[int, Meta(ge=0)]
type PositiveInt = Annotated[int, Meta(ge=1)]
type HeadingLevel = Annotated[int, Meta(ge=1, le=6)]
type PositiveFloat = Annotated[float, Meta(gt=0)]
type FontWeight = Annotated[int, Meta(ge=1, le=1000)]
type RgbChannel = Annotated[int, Meta(ge=0, le=255)]
type ClassificationCode = Annotated[
    str, Meta(min_length=1, max_length=32, pattern=r"\A[A-Za-z0-9][\w .\-]*\Z")
]
type Rgb = tuple[RgbChannel, RgbChannel, RgbChannel]
type Rect = tuple[float, float, float, float]

# --- [MODELS] ---------------------------------------------------------------------------


class NodeMeta(Struct, frozen=True, omit_defaults=True):
    key: ContentKey
    role: str
    page: NonnegativeInt
    bounds: Rect | None = None
    lang: LangTag | UnsetType = UNSET
    actual_text: str | UnsetType = UNSET
    expansion: str | UnsetType = UNSET
    associated: tuple[ContentKey, ...] = ()
    classification: ClassificationCode | UnsetType = UNSET


class StandardRole(Struct, frozen=True, tag="standard", tag_field="role_kind"):
    elt: StructEltKind


class ForeignRole(Struct, frozen=True, tag="foreign", tag_field="role_kind"):
    role: ForeignRoleStr


type StructRole = StandardRole | ForeignRole


class Uri(Struct, frozen=True, tag="uri", tag_field="target"):
    href: Annotated[str, Meta(min_length=1, max_length=4096)]


class Dest(Struct, frozen=True, tag="dest", tag_field="target"):
    page: NonnegativeInt
    point: tuple[float, float] | None = None


class DetailCitation(Struct, frozen=True, tag="detail", tag_field="citation"):
    sheet: ReferencePart
    detail: ReferencePart


class SectionCitation(Struct, frozen=True, tag="section", tag_field="citation"):
    code: ClassificationCode


type Citation = DetailCitation | SectionCitation


class Xref(Struct, frozen=True, tag="xref", tag_field="target"):
    value: Citation

    def cite(self) -> str:
        match self.value:
            case DetailCitation(sheet=sheet, detail=detail):
                return f"{detail}/{sheet}"
            case SectionCitation(code=code):
                return code
            case _ as unreachable:
                assert_never(unreachable)


class NoTarget(Struct, frozen=True, tag="none", tag_field="target"):
    pass


type AnnotTarget = Uri | Dest | Xref | NoTarget


class TextField(Struct, frozen=True, tag="text", tag_field="field"):
    value: str = ""
    mode: TextMode = TextMode.SINGLE
    max_length: NonnegativeInt | None = None  # AcroForm /MaxLen; None = unbounded, and a negative bound never reaches HTML maxlength


class CheckboxField(Struct, frozen=True, tag="checkbox", tag_field="field"):
    checked: bool = False
    export: str = "Yes"


class RadioField(Struct, frozen=True, tag="radio", tag_field="field"):
    options: ChoiceOptions
    selected: str = ""


class ComboField(Struct, frozen=True, tag="combo", tag_field="field"):
    options: ChoiceOptions
    selected: str = ""
    editable: bool = False


class ListField(Struct, frozen=True, tag="list", tag_field="field"):
    options: ChoiceOptions
    selected: tuple[str, ...] = ()


class SignatureField(Struct, frozen=True, tag="signature", tag_field="field"):
    signer: str = ""
    reason: str = ""


class ButtonField(Struct, frozen=True, tag="button", tag_field="field"):
    label: str = ""
    action: AnnotTarget = msgspec.field(default_factory=NoTarget)


type FieldValue = TextField | CheckboxField | RadioField | ComboField | ListField | SignatureField | ButtonField


class PageNode(Struct, frozen=True, tag=NodeKind.PAGE.value, tag_field="kind"):
    meta: NodeMeta
    media_box: Rect
    children: tuple[DocumentNode, ...] = ()


class SectionNode(Struct, frozen=True, tag=NodeKind.SECTION.value, tag_field="kind"):
    meta: NodeMeta
    level: HeadingLevel
    heading: tuple[RunNode, ...] = ()
    children: tuple[DocumentNode, ...] = ()


class BlockNode(Struct, frozen=True, tag=NodeKind.BLOCK.value, tag_field="kind"):
    meta: NodeMeta
    block: BlockKind
    level: HeadingLevel = 1
    runs: tuple[RunNode, ...] = ()
    children: tuple[DocumentNode, ...] = ()


class RunNode(Struct, frozen=True, tag=NodeKind.RUN.value, tag_field="kind"):
    meta: NodeMeta
    text: str
    font_key: str
    size: PositiveFloat
    weight: FontWeight = 400
    italic: bool = False
    direction: TextDirection = TextDirection.AUTO
    script: RunScript = RunScript.NORMAL
    decorations: tuple[TextDecoration, ...] = ()
    color: Rgb = (0, 0, 0)
    features: tuple[str, ...] = ()
    letter_spacing: float = 0.0


class ListNode(Struct, frozen=True, tag=NodeKind.LIST.value, tag_field="kind"):
    meta: NodeMeta
    list_kind: ListKind = ListKind.UNORDERED
    start: PositiveInt = 1
    items: tuple[DocumentNode, ...] = ()


class TableNode(Struct, frozen=True, tag=NodeKind.TABLE.value, tag_field="kind"):
    meta: NodeMeta
    rows: tuple[tuple[DocumentNode, ...], ...] = ()
    spans: tuple[tuple[NonnegativeInt, NonnegativeInt, PositiveInt, PositiveInt], ...] = ()
    header_rows: NonnegativeInt = 0
    footer_rows: NonnegativeInt = 0
    header_cols: NonnegativeInt = 0
    caption: tuple[RunNode, ...] = ()


class FigureNode(Struct, frozen=True, tag=NodeKind.FIGURE.value, tag_field="kind"):
    meta: NodeMeta
    asset_key: ContentKey
    alt: AltText = ""
    media_type: MediaType = "image/png"
    intrinsic: tuple[PositiveFloat, PositiveFloat] | None = None
    caption: tuple[RunNode, ...] = ()


class FormulaNode(Struct, frozen=True, tag=NodeKind.FORMULA.value, tag_field="kind"):
    meta: NodeMeta
    tex: str
    display: bool = False
    alt: AltText = ""
    mathml: str = ""


class FieldNode(Struct, frozen=True, tag=NodeKind.FIELD.value, tag_field="kind"):
    meta: NodeMeta
    name: FieldName
    field: FieldValue
    required: bool = False
    readonly: bool = False
    tooltip: str = ""  # AcroForm /TU alternate description — the accessibility label lens recovery preserves


class AnnotationNode(Struct, frozen=True, tag=NodeKind.ANNOTATION.value, tag_field="kind"):
    meta: NodeMeta
    annot: AnnotKind
    target: Rect
    contents: str = ""
    link: AnnotTarget = msgspec.field(default_factory=NoTarget)


class StructureNode(Struct, frozen=True, tag=NodeKind.STRUCTURE.value, tag_field="kind"):
    meta: NodeMeta
    role: StructRole
    children: tuple[DocumentNode, ...] = ()


type DocumentNode = (
    PageNode | SectionNode | BlockNode | RunNode | ListNode | TableNode | FigureNode | FormulaNode | FieldNode | AnnotationNode | StructureNode
)


class CorpusRow(Struct, frozen=True):
    key: str
    kind: NodeKind
    role: str
    page: int
    text: str
    alt: AltText = ""
    alt_status: AltStatus = AltStatus.NA
    lang: str = ""
    actual_text: str = ""
    expansion: str = ""
    classification: str = ""
    xref: str = ""


# --- [CONSTANTS] ------------------------------------------------------------------------

_ENCODER: Final = msgspec.msgpack.Encoder(order="deterministic")
_JSON_ENCODER: Final = msgspec.json.Encoder(order="deterministic")
_DOCUMENT_DECODER: Final = msgspec.msgpack.Decoder(DocumentNode)
_NULL_KEY: Final = ContentKey(value=0, fmt="", byte_length=0)  # the blanked key leaf digest preimages carry in place of a live mint
_CHILD_FIELDS: Final[frozenset[str]] = frozenset({"children", "heading", "runs", "items", "caption", "rows"})
_TYPST_ESCAPE: Final[Map[TypstScope, dict[int, str]]] = Map.of_seq([
    (TypstScope.STRING, str.maketrans({"\\": "\\\\", '"': '\\"'})),
    (TypstScope.MARKUP, str.maketrans({c: f"\\{c}" for c in "\\[]#*_@$<>`"})),
])
_LIST_MARKUP: Final[Map[ListKind, str]] = Map.of_seq([(ListKind.UNORDERED, "list"), (ListKind.ORDERED, "enum"), (ListKind.DESCRIPTION, "terms")])
_DECORATION_MARKUP: Final[Map[TextDecoration, str]] = Map.of_seq([
    (TextDecoration.UNDERLINE, "underline"),
    (TextDecoration.STRIKETHROUGH, "strike"),
    (TextDecoration.OVERLINE, "overline"),
])
_DECORATION_CSS: Final[Map[TextDecoration, str]] = Map.of_seq([
    (TextDecoration.UNDERLINE, "underline"),
    (TextDecoration.STRIKETHROUGH, "line-through"),
    (TextDecoration.OVERLINE, "overline"),
])
_BLOCK_HTML: Final[Map[BlockKind, str]] = Map.of_seq([
    (BlockKind.PARAGRAPH, "p"),
    (BlockKind.QUOTE, "blockquote"),
    (BlockKind.CAPTION, "figcaption"),
    (BlockKind.ARTIFACT, "div"),
])
_LIST_HTML: Final[Map[ListKind, str]] = Map.of_seq([(ListKind.UNORDERED, "ul"), (ListKind.ORDERED, "ol"), (ListKind.DESCRIPTION, "dl")])
_MD_ESCAPE: Final[dict[int, str]] = str.maketrans({
    c: f"\\{c}" for c in "\\`*_[]<>|"
})
_MD_DECORATION: Final[Map[TextDecoration, tuple[str, str]]] = Map.of_seq([
    (TextDecoration.UNDERLINE, ("<u>", "</u>")),
    (TextDecoration.STRIKETHROUGH, ("~~", "~~")),
    (TextDecoration.OVERLINE, ('<span style="text-decoration:overline">', "</span>")),
])
_LATEX_ESCAPE: Final[dict[int, str]] = str.maketrans({
    "\\": "\\textbackslash{}",
    "~": "\\textasciitilde{}",
    "^": "\\textasciicircum{}",
    "&": "\\&",
    "%": "\\%",
    "$": "\\$",
    "#": "\\#",
    "_": "\\_",
    "{": "\\{",
    "}": "\\}",
})
_LATEX_SECTION: Final[Map[int, str]] = Map.of_seq([
    (1, "section"),
    (2, "subsection"),
    (3, "subsubsection"),
    (4, "paragraph"),
    (5, "subparagraph"),
    (6, "subparagraph"),
])
_LATEX_DECORATION: Final[Map[TextDecoration, str]] = Map.of_seq([
    (TextDecoration.UNDERLINE, "uline"),
    (TextDecoration.STRIKETHROUGH, "sout"),
    (TextDecoration.OVERLINE, "overline"),
])
_MASKED: Final = "••••••"  # the one redaction marker a PASSWORD field projects; the secret value never reaches any text egress
# Link-safety owner every actionable backend reads: `_SAFE_SCHEMES` is the explicit scheme allowlist, `_MD_HREF`
# percent-encodes the markdown destination's structural characters, `_LATEX_HREF` escapes the `\href` URL argument.
_SAFE_SCHEMES: Final[frozenset[str]] = frozenset({"http", "https", "mailto", "tel", "ftp", "ftps"})
_MD_HREF: Final[dict[int, str]] = str.maketrans({"<": "%3C", ">": "%3E", "(": "%28", ")": "%29", " ": "%20", "\n": "%0A"})
_LATEX_HREF: Final[dict[int, str]] = str.maketrans({"\\": "\\\\", "%": "\\%", "#": "\\#", "{": "\\{", "}": "\\}"})
# MathML Core presentation element set — `annotation-xml` (the HTML-island injection vector) and `maction`
# (actiontype/href) are deliberately absent, so a hostile island can never append into the HTML tree.
_MATHML: Final[frozenset[str]] = frozenset({
    "math", "mrow", "mi", "mn", "mo", "ms", "mtext", "mspace", "msqrt", "mroot", "mfrac", "mstyle", "merror",
    "mpadded", "mphantom", "menclose", "msub", "msup", "msubsup", "munder", "mover", "munderover",
    "mmultiscripts", "mprescripts", "none", "mtable", "mtr", "mtd", "mlabeledtr", "mglyph", "semantics", "annotation",
})


def _actionable(href: str) -> bool:
    # Scheme allowlist over the RFC 3986 scheme cut: a scheme-less relative reference stays actionable, and an
    # explicit scheme outside `_SAFE_SCHEMES` (javascript:, vbscript:, data:, file:) demotes the link to inert
    # text at EVERY backend — HTML, Typst, Markdown, and LaTeX all read this one gate, never a per-arm re-check.
    head, sep, _ = href.partition(":")
    return not sep or any(mark in head for mark in "/?#") or head.lower() in _SAFE_SCHEMES

# --- [TABLES] ---------------------------------------------------------------------------

_STRUCT_CATEGORY: Final[frozendict[StructEltKind, tuple[StructCategory, int]]] = frozendict([
    (StructEltKind.SECT, (StructCategory.GROUPING, 0)),
    (StructEltKind.DOCUMENT, (StructCategory.GROUPING, 0)),
    (StructEltKind.PART, (StructCategory.GROUPING, 0)),
    (StructEltKind.ART, (StructCategory.GROUPING, 0)),
    (StructEltKind.DIV, (StructCategory.GROUPING, 0)),
    (StructEltKind.TOC, (StructCategory.GROUPING, 0)),
    (StructEltKind.INDEX, (StructCategory.GROUPING, 0)),
    (StructEltKind.NONSTRUCT, (StructCategory.GROUPING, 0)),
    (StructEltKind.PRIVATE, (StructCategory.GROUPING, 0)),
    (StructEltKind.H1, (StructCategory.HEADING, 1)),
    (StructEltKind.H2, (StructCategory.HEADING, 2)),
    (StructEltKind.H3, (StructCategory.HEADING, 3)),
    (StructEltKind.H4, (StructCategory.HEADING, 4)),
    (StructEltKind.H5, (StructCategory.HEADING, 5)),
    (StructEltKind.H6, (StructCategory.HEADING, 6)),
    (StructEltKind.P, (StructCategory.BLOCK, 0)),
    (StructEltKind.TOCI, (StructCategory.BLOCK, 0)),
    (StructEltKind.BLOCKQUOTE, (StructCategory.BLOCK, 0)),
    (StructEltKind.BIBENTRY, (StructCategory.BLOCK, 0)),
    (StructEltKind.NOTE, (StructCategory.BLOCK, 0)),
    (StructEltKind.CODE, (StructCategory.BLOCK, 0)),
    (StructEltKind.CAPTION, (StructCategory.BLOCK, 0)),
    (StructEltKind.SPAN, (StructCategory.INLINE, 0)),
    (StructEltKind.LINK, (StructCategory.INLINE, 0)),
    (StructEltKind.QUOTE, (StructCategory.INLINE, 0)),
    (StructEltKind.REFERENCE, (StructCategory.INLINE, 0)),
    (StructEltKind.ANNOT, (StructCategory.INLINE, 0)),
    (StructEltKind.RUBY, (StructCategory.INLINE, 0)),
    (StructEltKind.RB, (StructCategory.INLINE, 0)),
    (StructEltKind.RT, (StructCategory.INLINE, 0)),
    (StructEltKind.RP, (StructCategory.INLINE, 0)),
    (StructEltKind.WARICHU, (StructCategory.INLINE, 0)),
    (StructEltKind.WT, (StructCategory.INLINE, 0)),
    (StructEltKind.WP, (StructCategory.INLINE, 0)),
    (StructEltKind.L, (StructCategory.LIST, 0)),
    (StructEltKind.LI, (StructCategory.LIST, 0)),
    (StructEltKind.LBL, (StructCategory.LIST, 0)),
    (StructEltKind.LBODY, (StructCategory.LIST, 0)),
    (StructEltKind.TABLE, (StructCategory.TABLE, 0)),
    (StructEltKind.THEAD, (StructCategory.TABLE, 0)),
    (StructEltKind.TBODY, (StructCategory.TABLE, 0)),
    (StructEltKind.TFOOT, (StructCategory.TABLE, 0)),
    (StructEltKind.TR, (StructCategory.TABLE, 0)),
    (StructEltKind.TH, (StructCategory.TABLE, 0)),
    (StructEltKind.TD, (StructCategory.TABLE, 0)),
    (StructEltKind.FIGURE, (StructCategory.ILLUSTRATION, 0)),
    (StructEltKind.FORMULA, (StructCategory.ILLUSTRATION, 0)),
    (StructEltKind.FORM, (StructCategory.ILLUSTRATION, 0)),
])
_FOREIGN_CATEGORY: Final[tuple[StructCategory, int]] = (
    StructCategory.GROUPING,
    0,
)
_STANDARD_FOR: Final[frozendict[StructCategory, StructEltKind]] = frozendict(
    (category, elt) for elt, (category, _level) in reversed(tuple(_STRUCT_CATEGORY.items()))
)

# --- [OPERATIONS] -----------------------------------------------------------------------


def children(node: DocumentNode) -> tuple[DocumentNode, ...]:
    match node:
        case PageNode(children=kids) | StructureNode(children=kids):
            return kids
        case SectionNode(heading=head, children=kids):
            return (*head, *kids)
        case BlockNode(runs=runs, children=kids):
            return (*runs, *kids)
        case ListNode(items=items):
            return items
        case TableNode(rows=rows, caption=caption):
            return (*caption, *(cell for row in rows for cell in row))
        case FigureNode(caption=caption):
            return caption
        case RunNode() | FieldNode() | AnnotationNode() | FormulaNode():
            return ()
        case _ as unreachable:
            assert_never(unreachable)


def walk(node: DocumentNode, *, prune: tuple[type, ...] = ()) -> Iterator[DocumentNode]:
    stack = Block.singleton(node)
    while not stack.is_empty():
        head, stack = stack.head(), stack.tail()
        yield head
        if not isinstance(head, prune):
            stack = Block.of_seq(children(head)).append(stack)


def _keyable(node: DocumentNode, /) -> DocumentNode:
    # digest preimage blanks the node's OWN key leaf — identity derives from content, never a prior mint.
    return msgspec.structs.replace(node, meta=msgspec.structs.replace(node.meta, key=_NULL_KEY))


def _own_bytes(node: DocumentNode, /) -> bytes:
    # deterministic JSON is the digest codec: msgpack cannot integer-encode the live u128 `ContentKey.value`
    # a sub-payload cell's meta carries, while JSON carries bignums natively under the same field order.
    return _JSON_ENCODER.encode(msgspec.structs.replace(_keyable(node), **{name: () for name in node.__struct_fields__ if name in _CHILD_FIELDS}))


def node_digest(node: DocumentNode) -> ContentKey:
    frontier: Block[tuple[bool, DocumentNode]] = Block.singleton((False, node))
    results: Block[ContentKey] = Block.empty()
    while not frontier.is_empty():
        (combine, current), frontier = frontier.head(), frontier.tail()
        kids = children(current)
        if not kids:
            results = results.cons(ContentIdentity.key(current.meta.key.fmt, _JSON_ENCODER.encode(_keyable(current))))
        elif combine:
            own = ContentIdentity.key(current.meta.key.fmt, _own_bytes(current))
            results = results.skip(len(kids)).cons(ContentIdentity.key(current.meta.key.fmt, (own, *tuple(results.take(len(kids)))[::-1])))
        else:
            frontier = Block.of_seq((False, kid) for kid in reversed(kids)).append(frontier.cons((True, current)))
    return results.head()


def role_of(value: StructRole | DocumentNode) -> str:
    match value:
        case StandardRole(elt=elt):
            return elt.value
        case ForeignRole(role=name):
            return name
        case StructureNode(role=role):
            return role_of(role)
        case _:
            return value.meta.role


def role_category(role: StructRole) -> tuple[StructCategory, int]:
    match role:
        case StandardRole(elt=elt):
            return _STRUCT_CATEGORY[elt]
        case ForeignRole():
            return _FOREIGN_CATEGORY
        case _ as unreachable:
            assert_never(unreachable)


def standard_for(role: StructRole) -> StructEltKind:
    match role:
        case StandardRole(elt=elt):
            return elt
        case ForeignRole():
            return _STANDARD_FOR[role_category(role)[0]]
        case _ as unreachable:
            assert_never(unreachable)


def alt_of(node: DocumentNode) -> tuple[AltText, AltStatus]:
    match node:
        case FigureNode(alt=alt) | FormulaNode(alt=alt):
            return alt, (AltStatus.PRESENT if alt else AltStatus.ABSENT)
        case _:
            return "", AltStatus.NA


def _link_cite(node: DocumentNode) -> str:
    match node:
        case AnnotationNode(link=Xref() as xref):
            return xref.cite()
        case _:
            return ""


@overload
def to_corpus(node: DocumentNode, view: Literal[CorpusView.STRUCT] = ..., /) -> CorpusRow: ...
@overload
def to_corpus(node: DocumentNode, view: Literal[CorpusView.BYTES], /) -> bytes: ...
@overload
def to_corpus(node: DocumentNode, view: Literal[CorpusView.RECORD], /) -> dict[str, object]: ...
def to_corpus(node: DocumentNode, view: CorpusView = CorpusView.STRUCT, /) -> CorpusRow | bytes | dict[str, object]:
    alt, status = alt_of(node)
    row = CorpusRow(
        key=node.meta.key.hex,
        kind=NodeKind(node.__struct_config__.tag),
        role=role_of(node),
        page=node.meta.page,
        text="".join(
            live.text if isinstance(live, RunNode) else field_text(live.field) if isinstance(live, FieldNode) else "" for live in walk(node)
        ),
        alt=alt,
        alt_status=status,
        lang="" if isinstance(node.meta.lang, UnsetType) else node.meta.lang,
        actual_text="" if isinstance(node.meta.actual_text, UnsetType) else node.meta.actual_text,
        expansion="" if isinstance(node.meta.expansion, UnsetType) else node.meta.expansion,
        classification="" if isinstance(node.meta.classification, UnsetType) else node.meta.classification,
        xref=_link_cite(node),
    )
    match view:
        case CorpusView.STRUCT:
            return row
        case CorpusView.BYTES:
            return _ENCODER.encode(row)
        case CorpusView.RECORD:
            return msgspec.to_builtins(row)
        case _ as unreachable:
            assert_never(unreachable)


def to_typst_source(node: DocumentNode, *, title: str | None = None) -> str:
    prelude = f'#set document(title: "{_typst(title, TypstScope.STRING)}")\n' if title is not None else ""
    return prelude + _typst_body(node)


def _typst_body(node: DocumentNode) -> str:
    match node:
        case RunNode():
            return _styled(node)
        case BlockNode(block=BlockKind.HEADING, level=level, runs=runs):
            return _heading(level, runs)
        case BlockNode(block=BlockKind.ARTIFACT, runs=runs, children=kids):
            return f"#pdf.artifact[{_runs(runs)}{''.join(_typst_body(child) for child in kids)}]\n"
        case BlockNode(block=BlockKind.QUOTE, runs=runs, children=kids):
            return f"#quote(block: true)[{_runs(runs)}{''.join(_typst_body(child) for child in kids)}]\n"
        case BlockNode(block=BlockKind.CODE, runs=runs):
            return f'#raw("{_typst("".join(run.text for run in runs), TypstScope.STRING)}", block: true)\n'
        case BlockNode(runs=runs, children=kids):
            return _runs(runs) + "".join(_typst_body(child) for child in kids) + "\n"
        case ListNode(list_kind=ListKind.DESCRIPTION, items=items):
            return f"#terms({', '.join(_term_pair(item) for item in items)})\n"
        case ListNode(list_kind=ListKind.ORDERED, start=start, items=items):
            return f"#enum({f'start: {start}, ' if start != 1 else ''}{_items(items)})\n"
        case ListNode(list_kind=kind, items=items):
            return f"#{_LIST_MARKUP[kind]}({_items(items)})\n"
        case SectionNode(level=level, heading=head, children=kids):
            return _heading(level, head) + "".join(_typst_body(child) for child in kids)
        case PageNode(children=kids):
            return "".join(_typst_body(child) for child in kids) + "#pagebreak()\n"
        case StructureNode(children=kids):
            return "".join(_typst_body(child) for child in kids)
        case TableNode(rows=rows, spans=spans, header_rows=head_n, footer_rows=foot_n, caption=caption):
            span_map, body_end = _span_map(spans), len(rows) - foot_n
            bands = (
                f"table.header(repeat: true, {_cells(rows[:head_n], span_map, 0)})" if head_n else "",
                _cells(rows[head_n:body_end], span_map, head_n),
                f"table.footer({_cells(rows[body_end:], span_map, body_end)})" if foot_n else "",
            )
            table = f"table(columns: {_column_count(rows, span_map)}, {', '.join(part for part in bands if part)})"
            return f"#figure({table}, caption: [{_runs(caption)}], kind: table)\n" if caption else f"#{table}\n"
        case FigureNode(asset_key=asset_key, alt=alt, caption=caption):
            return f"#figure({_image(asset_key, alt)}, caption: [{_runs(caption)}])\n"
        case FormulaNode(tex=tex, display=display):
            return f'#mitex("{_typst(tex, TypstScope.STRING)}")\n' if display else f'#mi("{_typst(tex, TypstScope.STRING)}")'
        case AnnotationNode(annot=AnnotKind.LINK, contents=text, link=Uri(href=href)):
            return f'#link("{_typst(href, TypstScope.STRING)}")[{_typst(text, TypstScope.MARKUP)}]' if _actionable(href) else _typst(text, TypstScope.MARKUP)
        case AnnotationNode(annot=AnnotKind.LINK, contents=text, link=Dest(page=page, point=point)):
            x, y = point if point else (0.0, 0.0)
            return f"#link((page: {page + 1}, x: {x}pt, y: {y}pt))[{_typst(text, TypstScope.MARKUP)}]"
        case AnnotationNode(annot=AnnotKind.LINK, contents=text, link=Xref() as xref):
            return f'#link("{_typst(xref.cite(), TypstScope.STRING)}")[{_typst(text, TypstScope.MARKUP)}]'
        case AnnotationNode(annot=AnnotKind.NOTE, contents=text) if text:
            return f"#footnote[{_typst(text, TypstScope.MARKUP)}]"
        case AnnotationNode(annot=AnnotKind.HIGHLIGHT, contents=text) if text:
            return f"#highlight[{_typst(text, TypstScope.MARKUP)}]"
        case FieldNode(field=field):
            return _typst(field_text(field), TypstScope.MARKUP)
        case AnnotationNode():
            return ""
        case _ as unreachable:
            assert_never(unreachable)


def encode(node: DocumentNode) -> bytes:
    return _ENCODER.encode(node)


def decode(payload: bytes) -> Result[DocumentNode, ModelFault]:
    return catch(exception=msgspec.DecodeError)(_DOCUMENT_DECODER.decode)(payload).map_error(lambda _raised: ModelFault.DECODE)


def _styled(run: RunNode) -> str:
    wrapped = pipe(
        _typst(run.text, TypstScope.MARKUP),
        lambda b: f"#strong[{b}]" if run.weight >= 700 else b,
        lambda b: f"#emph[{b}]" if run.italic else b,
        lambda b: f"#super[{b}]" if run.script is RunScript.SUPER else f"#sub[{b}]" if run.script is RunScript.SUB else b,
        lambda b: reduce(lambda inner, deco: f"#{_DECORATION_MARKUP[deco]}[{inner}]", run.decorations, b),
        lambda b: f"#text(dir: rtl)[{b}]" if run.direction is TextDirection.RTL else b,
    )
    return wrapped if run.color == (0, 0, 0) else f"#text(rgb({run.color[0]}, {run.color[1]}, {run.color[2]}))[{wrapped}]"


def _typst(value: str, scope: TypstScope) -> str:
    return value.translate(_TYPST_ESCAPE[scope])


def _items(items: tuple[DocumentNode, ...]) -> str:
    return ", ".join(f"[{_typst_body(item).strip()}]" for item in items)


def _span_map(spans: tuple[tuple[int, int, int, int], ...]) -> frozendict[tuple[int, int], tuple[int, int]]:
    return frozendict({(row, col): (col_span, row_span) for row, col, col_span, row_span in spans})


def _column_count(rows: tuple[tuple[DocumentNode, ...], ...], span_map: frozendict[tuple[int, int], tuple[int, int]]) -> int:
    return sum(span_map.get((0, col), (1, 1))[0] for col in range(len(rows[0]))) if rows else 0


def _cells(rows: tuple[tuple[DocumentNode, ...], ...], span_map: frozendict[tuple[int, int], tuple[int, int]], base: int) -> str:
    return ", ".join(_cell_markup(_typst_body(cell), span_map.get((base + r, c))) for r, row in enumerate(rows) for c, cell in enumerate(row))


def _cell_markup(content: str, span: tuple[int, int] | None) -> str:
    args = (
        ""
        if span is None
        else ", ".join(part for part in (f"colspan: {span[0]}" if span[0] != 1 else "", f"rowspan: {span[1]}" if span[1] != 1 else "") if part)
    )
    return f"table.cell({args})[{content}]" if args else f"[{content}]"


def _term_pair(item: DocumentNode) -> str:
    kids = children(item)
    term = _typst_body(kids[0]).strip() if kids else _typst_body(item).strip()
    body = "".join(_typst_body(child) for child in kids[1:]).strip()
    return f"terms.item([{term}], [{body}])"


def _runs(runs: tuple[RunNode, ...]) -> str:
    return "".join(_styled(run) for run in runs)


def _heading(level: HeadingLevel, runs: tuple[RunNode, ...]) -> str:
    return f"{'=' * level} {_runs(runs)}\n"


def _image(asset_key: ContentKey, alt: AltText) -> str:
    source = _typst(asset_key.hex, TypstScope.STRING)
    equiv = f'"{_typst(alt, TypstScope.STRING)}"' if alt else "none"
    return f'image("{source}", alt: {equiv})'


def to_html(node: DocumentNode) -> str:
    return etree.tostring(_element(node), method="html", encoding="unicode")


def to_lxml_tree(node: DocumentNode) -> "_Element":
    return _element(node)


def to_c14n(node: DocumentNode) -> bytes:
    return etree.tostring(_element(node), method="c14n2")


def to_json(node: DocumentNode) -> bytes:
    return _JSON_ENCODER.encode(node)


def json_schema() -> dict[str, object]:
    return msgspec.json.schema(DocumentNode)


def _wrapped(inner: "_Element", tag: str) -> "_Element":
    outer = etree.Element(tag)
    outer.append(inner)
    return outer


def _run_element(run: RunNode) -> "_Element":
    inner = etree.Element("span")
    inner.text = run.text
    decoration = " ".join(_DECORATION_CSS[deco] for deco in run.decorations)
    style = "; ".join(
        part
        for part in (
            f"color:rgb({run.color[0]},{run.color[1]},{run.color[2]})" if run.color != (0, 0, 0) else "",
            f"text-decoration:{decoration}" if decoration else "",
            f"letter-spacing:{run.letter_spacing}pt" if run.letter_spacing else "",
            "font-feature-settings:" + ", ".join(f'"{tag}"' for tag in run.features) if run.features else "",
        )
        if part
    )
    if style:
        inner.set("style", style)
    if run.direction is TextDirection.RTL:
        inner.set("dir", "rtl")
    layers = (
        *(("sup",) if run.script is RunScript.SUPER else ("sub",) if run.script is RunScript.SUB else ()),
        *(("em",) if run.italic else ()),
        *(("strong",) if run.weight >= 700 else ()),
    )
    return reduce(_wrapped, layers, inner)


def _filled(element: "_Element", runs: tuple[RunNode, ...], kids: tuple[DocumentNode, ...] = ()) -> "_Element":
    for run in runs:
        element.append(_run_element(run))
    for kid in kids:
        element.append(_element(kid))
    return element


def field_text(value: FieldValue, /) -> str:
    match value:
        case TextField(mode=TextMode.PASSWORD):
            return _MASKED  # a password value is non-exportable — every corpus/Typst/Markdown/LaTeX projection reads the marker
        case TextField(value=text):
            return text
        case CheckboxField(checked=True, export=export):
            return export
        case CheckboxField():
            return ""
        case RadioField(selected=selected) | ComboField(selected=selected):
            return selected
        case ListField(selected=selected):
            return ", ".join(selected)
        case SignatureField(signer=signer, reason=reason):
            return signer or reason
        case ButtonField(label=label):
            return label
        case _ as unreachable:
            assert_never(unreachable)


def _field_element(node: FieldNode, /) -> "_Element":
    match node.field:
        case TextField(value=value, mode=TextMode.MULTILINE, max_length=bound):
            control, root = etree.Element("textarea"), None
            control.text = value
            if bound is not None:
                control.set("maxlength", str(bound))
        case TextField(value=value, mode=mode, max_length=bound):
            control, root = etree.Element("input"), None
            control.set("type", "password" if mode is TextMode.PASSWORD else "text")
            if mode is not TextMode.PASSWORD:  # the secret never lands in markup; the control stays an empty password input
                control.set("value", value)
            if bound is not None:
                control.set("maxlength", str(bound))
        case CheckboxField(checked=checked, export=export):
            control, root = etree.Element("input"), None
            control.set("type", "checkbox")
            control.set("value", export)
            if checked:
                control.set("checked", "checked")
        case RadioField(options=options, selected=selected):
            control = etree.Element("fieldset")
            root = control
            etree.SubElement(control, "legend").text = node.name
            for option in options:
                label = etree.SubElement(control, "label")
                choice = etree.SubElement(label, "input", type="radio", name=node.name, value=option)
                if option == selected:
                    choice.set("checked", "checked")
                choice.tail = option
        case ComboField(options=options, selected=selected, editable=True):
            root = etree.Element("span")
            control = etree.SubElement(root, "input", type="text", value=selected, list=f"{node.name}-options")
            choices = etree.SubElement(root, "datalist", id=f"{node.name}-options")
            for option in options:
                etree.SubElement(choices, "option", value=option)
        case ComboField(options=options, selected=selected):
            control, root = etree.Element("select"), None
            for option in options:
                choice = etree.SubElement(control, "option", value=option)
                choice.text = option
                if option == selected:
                    choice.set("selected", "selected")
        case ListField(options=options, selected=selected):
            control, root = etree.Element("select"), None
            control.set("multiple", "multiple")
            for option in options:
                choice = etree.SubElement(control, "option", value=option)
                choice.text = option
                if option in selected:
                    choice.set("selected", "selected")
        case SignatureField(signer=signer, reason=reason):
            control, root = etree.Element("input"), None
            control.set("type", "text")
            control.set("value", signer)
            control.set("data-signature-reason", reason)
        case ButtonField(label=label, action=action):
            control, root = etree.Element("button"), None
            control.set("type", "button")
            control.text = label or node.name
            match action:
                case Uri(href=href) if _actionable(href):  # an unsafe scheme drops the action; the button body survives
                    control.set("data-href", href)
                case Uri():
                    pass
                case Dest(page=page):
                    control.set("data-destination", f"page-{page + 1}")
                case Xref() as xref:
                    control.set("data-xref", xref.cite())
                case NoTarget():
                    pass
                case _ as unreachable:
                    assert_never(unreachable)
        case _ as unreachable:
            assert_never(unreachable)
    control.set("name", node.name)
    if node.required:
        control.set("required", "required")
    if node.readonly:
        # Native policy attribute per control kind: `readonly` holds only on text entry; a checkbox, select,
        # radio fieldset, or button disables — a fieldset `disabled` natively disables every contained input.
        text_entry = control.tag == "textarea" or (control.tag == "input" and control.get("type") in ("text", "password"))
        control.set("readonly" if text_entry else "disabled", "readonly" if text_entry else "disabled")
        control.set("aria-readonly", "true")
    if node.tooltip:  # AcroForm /TU projects as the visual tooltip and the accessible name
        control.set("title", node.tooltip)
        control.set("aria-label", node.tooltip)
    return control if root is None else root


def _element(node: DocumentNode) -> "_Element":
    match node:
        case RunNode():
            return _run_element(node)
        case BlockNode(block=BlockKind.HEADING, level=level, runs=runs):
            return _filled(etree.Element(f"h{level}"), runs)
        case BlockNode(block=BlockKind.CODE, runs=runs):
            pre = etree.Element("pre")
            etree.SubElement(pre, "code").text = "".join(run.text for run in runs)
            return pre
        case BlockNode(block=block, runs=runs, children=kids):
            return _filled(etree.Element(_BLOCK_HTML[block]), runs, kids)
        case ListNode(list_kind=ListKind.DESCRIPTION, items=items):
            return _filled_terms(etree.Element("dl"), items)
        case ListNode(list_kind=kind, start=start, items=items):
            ordered = etree.Element(_LIST_HTML[kind])
            if kind is ListKind.ORDERED and start != 1:
                ordered.set("start", str(start))
            for item in items:
                etree.SubElement(ordered, "li").append(_element(item))
            return ordered
        case SectionNode(level=level, heading=head, children=kids):
            section = _filled(etree.Element("section"), (), kids)
            section.insert(0, _filled(etree.Element(f"h{level}"), head))
            return section
        case PageNode(children=kids):
            page = _filled(etree.Element("div"), (), kids)
            page.set("class", "page")
            return page
        case StructureNode(children=kids):
            structured = _filled(etree.Element("div"), (), kids)
            structured.set("role", role_of(node))
            return structured
        case TableNode(rows=rows, spans=spans, header_rows=head_n, footer_rows=foot_n, header_cols=head_c, caption=caption):
            return _table_element(rows, head_n, foot_n, head_c, _span_map(spans), caption)
        case FigureNode(asset_key=asset_key, alt=alt, caption=caption):
            figure = etree.Element("figure")
            image = etree.SubElement(figure, "img")
            image.set("src", asset_key.hex)
            image.set("alt", alt)
            return _filled(figure, caption) if caption else figure
        case FormulaNode(tex=tex, display=display, alt=alt, mathml=mathml):
            math = etree.Element("div" if display else "span")
            math.set("class", "math display" if display else "math")
            math.set("role", "math")
            if alt:
                math.set("aria-label", alt)
            if mathml and (island := _mathml(mathml)) is not None:
                math.append(island)
            else:
                # a missing, malformed, or scrub-refused island falls back to the TeX text the serializer escapes
                math.text = f"\\[{tex}\\]" if display else f"\\({tex}\\)"
            return math
        case AnnotationNode(annot=AnnotKind.LINK, contents=text, link=Uri(href=href)):
            return _anchor(href, text)
        case AnnotationNode(annot=AnnotKind.LINK, contents=text, link=Dest(page=page)):
            return _anchor(f"#page-{page + 1}", text)
        case AnnotationNode(annot=AnnotKind.LINK, contents=text, link=Xref() as xref):
            return _anchor(f"#{xref.cite()}", text)
        case AnnotationNode(annot=AnnotKind.NOTE, contents=text) if text:
            aside = etree.Element("aside")
            aside.set("role", "doc-footnote")
            aside.text = text
            return aside
        case AnnotationNode(annot=AnnotKind.HIGHLIGHT, contents=text) if text:
            mark = etree.Element("mark")
            mark.text = text
            return mark
        case FieldNode() as field:
            return _field_element(field)
        case AnnotationNode():
            return etree.Element("span")
        case _ as unreachable:
            assert_never(unreachable)


def _mathml(payload: str) -> "_Element | None":
    # MathML admission: entity/network-hardened parse, then the `_MATHML` whitelist scrub — a comment, PI, or
    # element outside the set drops with its subtree and script-capable attributes (on*, href, src) strip — so
    # only presentation MathML ever appends into the HTML tree; a refused island returns None for the TeX fallback.
    parser = etree.XMLParser(resolve_entities=False, huge_tree=False, no_network=True)
    parsed = catch(exception=etree.XMLSyntaxError)(etree.fromstring)(payload, parser).default_value(None)
    if parsed is None or not isinstance(parsed.tag, str) or etree.QName(parsed).localname != "math":
        return None
    for element in tuple(parsed.iter()):  # Exemption: whitelist scrub mutates the parsed island at the lxml provider seam
        if not isinstance(element.tag, str) or etree.QName(element).localname not in _MATHML:
            parent = element.getparent()
            if parent is not None:
                parent.remove(element)
            continue
        for name in tuple(element.attrib):
            local = etree.QName(name).localname.lower()
            if local.startswith("on") or local in ("href", "src"):
                del element.attrib[name]
    return parsed


def _anchor(href: str, text: str) -> "_Element":
    # an unsafe scheme degrades to an inert span carrying the text — the link body survives, the actionability drops
    live = _actionable(href)
    anchor = etree.Element("a" if live else "span")
    if live:
        anchor.set("href", href)
    anchor.text = text
    return anchor


def _filled_terms(dl: "_Element", items: tuple[DocumentNode, ...]) -> "_Element":
    for item in items:
        kids = children(item)
        etree.SubElement(dl, "dt").append(_element(kids[0]) if kids else _element(item))
        _filled(etree.SubElement(dl, "dd"), (), kids[1:])
    return dl


def _table_element(
    rows: tuple[tuple[DocumentNode, ...], ...],
    head_n: int,
    foot_n: int,
    head_c: int,
    span_map: frozendict[tuple[int, int], tuple[int, int]],
    caption: tuple[RunNode, ...],
) -> "_Element":
    table = etree.Element("table")
    if caption:
        _filled(etree.SubElement(table, "caption"), caption)
    body_end = len(rows) - foot_n
    bands = (("thead", rows[:head_n], "th", 0), ("tbody", rows[head_n:body_end], "td", head_n), ("tfoot", rows[body_end:], "td", body_end))
    for band_tag, band_rows, cell_tag, base in bands:
        if not band_rows:
            continue
        band = etree.SubElement(table, band_tag)
        for r, row in enumerate(band_rows):
            line = etree.SubElement(band, "tr")
            for c, cell in enumerate(row):
                span = span_map.get((base + r, c))
                header_cell = cell_tag == "th" or c < head_c
                td = etree.SubElement(line, "th" if header_cell else "td")
                if header_cell:
                    td.set("scope", "col" if cell_tag == "th" else "row")
                if span and span[0] != 1:
                    td.set("colspan", str(span[0]))
                if span and span[1] != 1:
                    td.set("rowspan", str(span[1]))
                td.append(_element(cell))
    return table


def to_markdown(node: DocumentNode) -> str:
    match node:
        case RunNode():
            return _md_styled(node)
        case BlockNode(block=BlockKind.HEADING, level=level, runs=runs):
            return f"{'#' * level} {_md_runs(runs)}\n\n"
        case BlockNode(block=BlockKind.CODE, runs=runs):
            return f"```\n{''.join(run.text for run in runs)}\n```\n\n"
        case BlockNode(block=BlockKind.QUOTE, runs=runs, children=kids):
            body = _md_runs(runs) + "".join(to_markdown(child) for child in kids)
            return "".join(f"> {line}\n" for line in (body.splitlines() or [""])) + "\n"
        case BlockNode(runs=runs, children=kids):
            return _md_runs(runs) + "".join(to_markdown(child) for child in kids) + "\n\n"
        case ListNode(list_kind=ListKind.DESCRIPTION, items=items):
            return "".join(_md_term(item) for item in items) + "\n"
        case ListNode(list_kind=ListKind.ORDERED, start=start, items=items):
            return "".join(f"{start + index}. {to_markdown(item).strip()}\n" for index, item in enumerate(items)) + "\n"
        case ListNode(items=items):
            return "".join(f"- {to_markdown(item).strip()}\n" for item in items) + "\n"
        case SectionNode(level=level, heading=head, children=kids):
            return f"{'#' * level} {_md_runs(head)}\n\n" + "".join(to_markdown(child) for child in kids)
        case PageNode(children=kids):
            return "".join(to_markdown(child) for child in kids)
        case StructureNode(children=kids):
            return "".join(to_markdown(child) for child in kids)
        case TableNode(rows=rows, header_rows=head_n, caption=caption):
            return _md_table(rows, head_n, caption)
        case FigureNode(asset_key=asset_key, alt=alt, caption=caption):
            figure = f"![{_md(alt)}]({asset_key.hex})\n"
            return f"{figure}\n{_md_runs(caption)}\n\n" if caption else f"{figure}\n"
        case FormulaNode(tex=tex, display=display):
            return f"$$\n{tex}\n$$\n\n" if display else f"${tex}$"
        case AnnotationNode(annot=AnnotKind.LINK, contents=text, link=Uri(href=href)):
            return f"[{_md(text)}]({href.translate(_MD_HREF)})" if _actionable(href) else _md(text)
        case AnnotationNode(annot=AnnotKind.LINK, contents=text, link=Dest(page=page)):
            return f"[{_md(text)}](#page-{page + 1})"
        case AnnotationNode(annot=AnnotKind.LINK, contents=text, link=Xref() as xref):
            return f"[{_md(text)}](#{xref.cite()})"
        case AnnotationNode(annot=AnnotKind.NOTE, contents=text) if text:
            return f"^[{_md(text)}]"
        case FieldNode(field=field):
            return _md(field_text(field))
        case AnnotationNode():
            return ""
        case _ as unreachable:
            assert_never(unreachable)


def to_latex(node: DocumentNode) -> str:
    match node:
        case RunNode():
            return _latex_styled(node)
        case BlockNode(block=BlockKind.HEADING, level=level, runs=runs):
            return f"\\{_LATEX_SECTION[level]}{{{_latex_runs(runs)}}}\n\n"
        case BlockNode(block=BlockKind.CODE, runs=runs):
            return f"\\begin{{verbatim}}\n{''.join(run.text for run in runs)}\n\\end{{verbatim}}\n\n"
        case BlockNode(block=BlockKind.QUOTE, runs=runs, children=kids):
            return f"\\begin{{quote}}\n{_latex_runs(runs)}{''.join(to_latex(child) for child in kids)}\n\\end{{quote}}\n\n"
        case BlockNode(runs=runs, children=kids):
            return _latex_runs(runs) + "".join(to_latex(child) for child in kids) + "\n\n"
        case ListNode(list_kind=ListKind.DESCRIPTION, items=items):
            return f"\\begin{{description}}\n{''.join(_latex_term(item) for item in items)}\\end{{description}}\n\n"
        case ListNode(list_kind=ListKind.ORDERED, start=start, items=items):
            counter = f"\\setcounter{{enumi}}{{{start - 1}}}\n" if start != 1 else ""
            return f"\\begin{{enumerate}}\n{counter}{_latex_items(items)}\\end{{enumerate}}\n\n"
        case ListNode(items=items):
            return f"\\begin{{itemize}}\n{_latex_items(items)}\\end{{itemize}}\n\n"
        case SectionNode(level=level, heading=head, children=kids):
            return f"\\{_LATEX_SECTION[level]}{{{_latex_runs(head)}}}\n\n" + "".join(to_latex(child) for child in kids)
        case PageNode(children=kids):
            return "".join(to_latex(child) for child in kids) + "\\clearpage\n"
        case StructureNode(children=kids):
            return "".join(to_latex(child) for child in kids)
        case TableNode(rows=rows, spans=spans, header_rows=head_n, footer_rows=foot_n, caption=caption):
            return _latex_table(rows, head_n, foot_n, _span_map(spans), caption)
        case FigureNode(asset_key=asset_key, alt=alt, caption=caption):
            cap = f"\\caption{{{_latex_runs(caption)}}}\n" if caption else ""
            note = f"% alt: {_latex(alt)}\n" if alt else ""
            return f"\\begin{{figure}}\n\\centering\n{note}\\includegraphics{{{asset_key.hex}}}\n{cap}\\end{{figure}}\n\n"
        case FormulaNode(tex=tex, display=display):
            return f"\\[\n{tex}\n\\]\n\n" if display else f"${tex}$"
        case AnnotationNode(annot=AnnotKind.LINK, contents=text, link=Uri(href=href)):
            return f"\\href{{{href.translate(_LATEX_HREF)}}}{{{_latex(text)}}}" if _actionable(href) else _latex(text)
        case AnnotationNode(annot=AnnotKind.LINK, contents=text, link=Dest(page=page)):
            return f"\\hyperlink{{page-{page + 1}}}{{{_latex(text)}}}"
        case AnnotationNode(annot=AnnotKind.LINK, contents=text, link=Xref() as xref):
            return f"\\hyperref[{xref.cite()}]{{{_latex(text)}}}"
        case AnnotationNode(annot=AnnotKind.NOTE, contents=text) if text:
            return f"\\footnote{{{_latex(text)}}}"
        case FieldNode(field=field):
            return _latex(field_text(field))
        case AnnotationNode():
            return ""
        case _ as unreachable:
            assert_never(unreachable)


def _md(value: str) -> str:
    return value.translate(_MD_ESCAPE)


def _md_styled(run: RunNode) -> str:
    body = pipe(
        _md(run.text),
        lambda b: f"**{b}**" if run.weight >= 700 else b,
        lambda b: f"*{b}*" if run.italic else b,
        lambda b: f"<sup>{b}</sup>" if run.script is RunScript.SUPER else f"<sub>{b}</sub>" if run.script is RunScript.SUB else b,
        lambda b: reduce(lambda inner, deco: f"{_MD_DECORATION[deco][0]}{inner}{_MD_DECORATION[deco][1]}", run.decorations, b),
    )
    return body if run.color == (0, 0, 0) else f'<span style="color:rgb({run.color[0]},{run.color[1]},{run.color[2]})">{body}</span>'


def _md_runs(runs: tuple[RunNode, ...]) -> str:
    return "".join(_md_styled(run) for run in runs)


def _md_term(item: DocumentNode) -> str:
    kids = children(item)
    term = to_markdown(kids[0]).strip() if kids else to_markdown(item).strip()
    body = "".join(to_markdown(child) for child in kids[1:]).strip()
    return f"**{term}**\n: {body}\n"


def _md_table(rows: tuple[tuple[DocumentNode, ...], ...], header_rows: int, caption: tuple[RunNode, ...]) -> str:
    if not rows:
        return ""
    width = max(len(row) for row in rows)
    head_n = header_rows or 1
    lines = (
        *(f"| {' | '.join(_md_cell(row, col) for col in range(width))} |" for row in rows[:head_n]),
        f"| {' | '.join('---' for _ in range(width))} |",
        *(f"| {' | '.join(_md_cell(row, col) for col in range(width))} |" for row in rows[head_n:]),
    )
    table = "\n".join(lines) + "\n"
    return f"{table}\n{_md_runs(caption)}\n\n" if caption else f"{table}\n"


def _md_cell(row: tuple[DocumentNode, ...], col: int) -> str:
    return to_markdown(row[col]).strip().replace("\n", " ").replace("|", "\\|") if col < len(row) else ""


def _latex(value: str) -> str:
    return value.translate(_LATEX_ESCAPE)


def _latex_styled(run: RunNode) -> str:
    body = pipe(
        _latex(run.text),
        lambda b: f"\\textbf{{{b}}}" if run.weight >= 700 else b,
        lambda b: f"\\textit{{{b}}}" if run.italic else b,
        lambda b: f"\\textsuperscript{{{b}}}" if run.script is RunScript.SUPER else f"\\textsubscript{{{b}}}" if run.script is RunScript.SUB else b,
        lambda b: reduce(lambda inner, deco: f"\\{_LATEX_DECORATION[deco]}{{{inner}}}", run.decorations, b),
    )
    return body if run.color == (0, 0, 0) else f"\\textcolor[RGB]{{{run.color[0]},{run.color[1]},{run.color[2]}}}{{{body}}}"


def _latex_runs(runs: tuple[RunNode, ...]) -> str:
    return "".join(_latex_styled(run) for run in runs)


def _latex_items(items: tuple[DocumentNode, ...]) -> str:
    return "".join(f"\\item {to_latex(item).strip()}\n" for item in items)


def _latex_term(item: DocumentNode) -> str:
    kids = children(item)
    term = to_latex(kids[0]).strip() if kids else to_latex(item).strip()
    body = "".join(to_latex(child) for child in kids[1:]).strip()
    return f"\\item[{term}] {body}\n"


def _latex_table(
    rows: tuple[tuple[DocumentNode, ...], ...],
    head_n: int,
    foot_n: int,
    span_map: frozendict[tuple[int, int], tuple[int, int]],
    caption: tuple[RunNode, ...],
) -> str:
    if not rows:
        return ""
    width = _column_count(rows, span_map)
    spec = "|" + "l|" * width
    boundaries = frozenset(edge for edge in (head_n, len(rows) - foot_n, len(rows)) if 0 < edge <= len(rows))
    lines = "".join(_latex_row(row, r, span_map) + ("\\hline\n" if r + 1 in boundaries else "") for r, row in enumerate(rows))
    cap = f"\\caption{{{_latex_runs(caption)}}}\n" if caption else ""
    return f"\\begin{{table}}\n\\centering\n{cap}\\begin{{tabular}}{{{spec}}}\n\\hline\n{lines}\\end{{tabular}}\n\\end{{table}}\n\n"


def _latex_row(row: tuple[DocumentNode, ...], r: int, span_map: frozendict[tuple[int, int], tuple[int, int]]) -> str:
    return " & ".join(_latex_cell(row[c], r, c, span_map) for c in range(len(row))) + " \\\\\n"


def _latex_cell(cell: DocumentNode, r: int, c: int, span_map: frozendict[tuple[int, int], tuple[int, int]]) -> str:
    content = to_latex(cell).strip().replace("\n", " ")
    span = span_map.get((r, c))
    return f"\\multicolumn{{{span[0]}}}{{|l|}}{{{content}}}" if span and span[0] != 1 else content
```

## [03]-[DELTA]

- Owner: `DocumentDelta` — every edit keyed by the stable `NodeMeta.key` of the node it acts on; `diff` and `merge` are defined once over the tree, never re-minted per consumer.
- Entry: `diff(before, after)` folds the two trees into an ordered edit tuple, `merge(tree, deltas)` folds the patch back, and `invert(before, deltas)` maps each edit to its inverse — a redaction patch is reversible until burned in; `Replaced` carries a changed root identity or kind inside the same algebra, and `decode` converts malformed MessagePack once into `DeltaFault.DECODE`. `redline(before, after, style)` is the visible form of the same algebra: the diff folded back as an annotated tree — inserted runs decorated by the `RedlineStyle` insert row, deleted subtrees retained struck-through at their old slot, a per-delta summary `TableNode` appended — so addendum re-issue and track-changes review render through EVERY `document/emit#DOCUMENT` backend with zero new emission arm.
- Auto: `_index` keys each node by its structural PATH-vector (the child-ordinal sequence from the root) through one iterative frontier — the delta operates on lens-recovered, adversarial-depth trees, so no delta walker recurses natively, and `_stained` re-inks a whole subtree through the `_flat`/`_regrown` expand-combine frontier for the same reason; `_identities` derives the `Map[ContentKey, Path]` so move/reparametrize detection keys on the position-stable `NodeMeta.key` — NEVER on `node_digest`, whose Merkle fold re-keys every ancestor when a descendant changes and spuriously `Moved`s every sibling of an edit. Two distinct keyings, never conflated. A node whose key is new under an also-new parent is carried inside that parent's `Inserted` subtree and emits nothing — only the topmost insert and the topmost survivor delete are edits; a root whose own fields change emits `Reparametrized` on the root key, so `merge(before, diff(before, after)) == after` holds for root-only changes. `_rebuilt` is the one path-rebuild kernel splice, prune, and retarget all resolve through — locate the path off `_identities`, apply at the target, rebuild the ancestor spine bottom-up — never three parallel recursive walkers.
- Receipt: the delta count and changed-node keys ride the lens introspection receipt facts; `DocumentDelta` mints no receipt of its own.
- Growth: a new edit kind is one variant plus one `diff` emit arm and one `merge` apply arm — the totality `match` forces both; a new diff granularity is a `node_digest` policy change, never a parallel delta family; a new redline appearance is one `RedlineStyle` row, never a second annotator.
- Boundary: a per-consumer diff type (a document diff beside a geometry diff beside a wire diff) is the deleted form — `DocumentDelta` is the one edit algebra keyed by `ContentKey`. No positional list patching by index-shift heuristics outside the key algebra, and no second merge owner. Structural insertion/deletion/move targets the spine containers that own a `children` field through `_spine`/`_with_spine`; a `TableNode` cell grid, a `FigureNode` caption, a `SectionNode` heading, a `ListNode` item bag, and a `BlockNode` inline-run bag are bounded OWN-content sub-payloads re-keyed as a whole through `Reparametrized`, so `_spine` carries only the container `children` field and the sub-payload edits ride the own-field overlay. Root identity anchors the algebra: roots sharing `meta.key` and kind diff as edits, while a changed root identity or kind emits one `Replaced` case. One total fold spans the five-variant union; a missing arm is an `assert_never` static failure.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
from collections.abc import Callable
from enum import StrEnum
from functools import reduce
from itertools import accumulate
from typing import Final, assert_never

import msgspec
from expression import Nothing, Option, Result, Some
from expression.collections import Block, Map
from expression.extra.result import catch
from msgspec import Struct

from rasm.runtime.identity import ContentIdentity, ContentKey

from rasm.artifacts.document.model import (
    AnnotationNode,
    BlockNode,
    DocumentNode,
    FieldNode,
    FigureNode,
    FormulaNode,
    ListNode,
    NodeMeta,
    PageNode,
    Rgb,
    RunNode,
    SectionNode,
    StructureNode,
    TableNode,
    TextDecoration,
    walk,
)

# --- [TYPES] ----------------------------------------------------------------------------

type Path = tuple[int, ...]
type IndexEntry = tuple[DocumentNode, ContentKey | None, int]


class DeltaFault(StrEnum):
    DECODE = "decode"

# --- [MODELS] ---------------------------------------------------------------------------


class Inserted(Struct, frozen=True, tag="inserted", tag_field="edit"):
    parent: ContentKey
    index: int
    node: DocumentNode


class Deleted(Struct, frozen=True, tag="deleted", tag_field="edit"):
    key: ContentKey


class Moved(Struct, frozen=True, tag="moved", tag_field="edit"):
    key: ContentKey
    parent: ContentKey
    index: int


class Replaced(Struct, frozen=True, tag="replaced", tag_field="edit"):
    node: DocumentNode


class FieldPatch(Struct, frozen=True, gc=False):
    name: str
    value: msgspec.Raw


class Reparametrized(Struct, frozen=True, tag="reparametrized", tag_field="edit"):
    key: ContentKey
    fields: tuple[FieldPatch, ...]


type DocumentDelta = Inserted | Deleted | Moved | Replaced | Reparametrized


class RedlineInk(Struct, frozen=True):
    color: Rgb
    decorations: tuple[TextDecoration, ...]


class RedlineStyle(Struct, frozen=True):
    insert: RedlineInk
    delete: RedlineInk
    change: RedlineInk
    summary: bool = True


# --- [CONSTANTS] ------------------------------------------------------------------------

_DELTA_ENCODER: Final = msgspec.msgpack.Encoder(order="deterministic")
_DELTA_DECODER: Final = msgspec.msgpack.Decoder(tuple[DocumentDelta, ...])
_SPINE_FIELDS: Final[frozenset[str]] = frozenset({"children", "items"})
REDLINE: Final = RedlineStyle(
    insert=RedlineInk(color=(0, 102, 204), decorations=(TextDecoration.UNDERLINE,)),
    delete=RedlineInk(color=(204, 0, 0), decorations=(TextDecoration.STRIKETHROUGH,)),
    change=RedlineInk(color=(178, 102, 0), decorations=(TextDecoration.UNDERLINE,)),
)

# --- [OPERATIONS] -----------------------------------------------------------------------


def _spine(node: DocumentNode) -> tuple[DocumentNode, ...]:
    match node:
        case PageNode(children=kids) | StructureNode(children=kids) | SectionNode(children=kids) | BlockNode(children=kids):
            return kids
        case ListNode(items=items):
            return items
        case _:
            return ()


def _with_spine(node: DocumentNode, kids: tuple[DocumentNode, ...]) -> DocumentNode:
    match node:
        case PageNode() | StructureNode() | SectionNode() | BlockNode():
            return msgspec.structs.replace(node, children=kids)
        case ListNode():
            return msgspec.structs.replace(node, items=kids)
        case _:
            return node


def _own(node: DocumentNode, /) -> tuple[tuple[str, object], ...]:
    return tuple((name, getattr(node, name)) for name in node.__struct_fields__ if name not in _SPINE_FIELDS)


def _index(root: DocumentNode, /) -> Map[Path, IndexEntry]:
    table: Map[Path, IndexEntry] = Map.empty()
    frontier: Block[tuple[DocumentNode, Path, ContentKey | None, int]] = Block.singleton((root, (), None, 0))
    while not frontier.is_empty():
        (node, path, parent, position), frontier = frontier.head(), frontier.tail()
        table = table.add(path, (node, parent, position))
        frontier = Block.of_seq((kid, (*path, ordinal), node.meta.key, ordinal) for ordinal, kid in enumerate(_spine(node))).append(frontier)
    return table


def _identities(index: Map[Path, IndexEntry], /) -> Map[ContentKey, Path]:
    return Block.of_seq(index.items()).fold(lambda acc, item: acc.add(item[1][0].meta.key, item[0]), Map.empty())


def _by_key(index: Map[Path, IndexEntry], /) -> Map[ContentKey, IndexEntry]:
    return Block.of_seq(index.items()).fold(lambda acc, item: acc.add(item[1][0].meta.key, item[1]), Map.empty())


def diff(before: DocumentNode, after: DocumentNode, /) -> tuple[DocumentDelta, ...]:
    if before.meta.key != after.meta.key or type(before) is not type(after):
        return (Replaced(node=after),)

    old, new = _index(before), _index(after)
    old_at, new_at = _identities(old), _identities(new)

    def survived(node: DocumentNode, key: ContentKey, parent: ContentKey, index: int, prior: IndexEntry) -> Block[DocumentDelta]:
        node_prior, parent_prior, index_prior = prior
        moved = (Moved(key=key, parent=parent, index=index),) if (parent_prior, index_prior) != (parent, index) else ()
        changed = (Reparametrized(key=key, fields=_field_delta(node_prior, node)),) if _own(node_prior) != _own(node) else ()
        return Block.of_seq((*moved, *changed))

    def edits_for(item: tuple[Path, IndexEntry]) -> Block[DocumentDelta]:
        _path, (node, parent, index) = item
        if parent is None:
            prior_root = old.try_find(()).map(lambda entry: entry[0])
            return (
                prior_root
                .filter(lambda held: held.meta.key == node.meta.key and _own(held) != _own(node))
                .map(lambda held: Block.singleton(Reparametrized(key=node.meta.key, fields=_field_delta(held, node))))
                .default_value(Block.empty())
            )
        key = node.meta.key
        return (
            old_at
            .try_find(key)
            .bind(old.try_find)
            .map(lambda prior: survived(node, key, parent, index, prior))
            .default_with(
                lambda: Block.singleton(Inserted(parent=parent, index=index, node=node)) if old_at.try_find(parent).is_some() else Block.empty()
            )
        )

    def deletes_for(item: tuple[Path, IndexEntry]) -> Option[DocumentDelta]:
        _path, (node, parent, _index) = item
        gone = parent is not None and new_at.try_find(node.meta.key).is_none() and new_at.try_find(parent).is_some()
        return Some(Deleted(key=node.meta.key)) if gone else Nothing

    inserts_moves = Block.of_seq(new.items()).collect(edits_for)
    deletes = Block.of_seq(old.items()).choose(deletes_for)
    return tuple(inserts_moves) + tuple(deletes)


def merge(tree: DocumentNode, deltas: tuple[DocumentDelta, ...], /) -> DocumentNode:
    def apply(patched: DocumentNode, delta: DocumentDelta) -> DocumentNode:
        match delta:
            case Inserted(parent=parent, index=index, node=node):
                return _spliced(patched, parent, index, node)
            case Deleted(key=key):
                return _pruned(patched, key)
            case Moved(key=key, parent=parent, index=index):
                return _spliced(_pruned(patched, key), parent, index, _find(tree, key))
            case Replaced(node=node):
                return node
            case Reparametrized(key=key, fields=fields):
                return _retargeted(patched, key, lambda node: _apply_fields(node, fields))
            case _ as unreachable:
                assert_never(unreachable)

    return Block.of_seq(deltas).fold(apply, tree)


def redline(before: DocumentNode, after: DocumentNode, style: RedlineStyle = REDLINE, /) -> DocumentNode:
    deltas = diff(before, after)
    old = _by_key(_index(before))

    def annotated(tree: DocumentNode, delta: DocumentDelta, /) -> DocumentNode:
        match delta:
            case Inserted(node=node):
                return _retargeted(tree, node.meta.key, lambda live: _stained(live, style.insert))
            case Reparametrized(key=key) | Moved(key=key):
                return _retargeted(tree, key, lambda live: _stained(live, style.change))
            case Replaced(node=node):
                return _stained(node, style.change)
            case Deleted(key=key):
                return (
                    old
                    .try_find(key)
                    .bind(lambda entry: Option.of_optional(entry[1]).map(lambda parent: _spliced(tree, parent, entry[2], _stained(entry[0], style.delete))))
                    .default_value(tree)
                )
            case _ as unreachable:
                assert_never(unreachable)

    marked = Block.of_seq(deltas).fold(annotated, after)
    return _with_summary(marked, deltas) if style.summary and deltas else marked


def _flat(node: DocumentNode, /) -> tuple[DocumentNode, ...]:
    match node:
        case PageNode(children=kids) | StructureNode(children=kids):
            return kids
        case SectionNode(heading=head, children=kids):
            return (*head, *kids)
        case BlockNode(runs=runs, children=kids):
            return (*runs, *kids)
        case ListNode(items=items):
            return items
        case TableNode(rows=rows, caption=caption):
            return (*(cell for row in rows for cell in row), *caption)
        case FigureNode(caption=caption):
            return caption
        case RunNode() | FormulaNode() | FieldNode() | AnnotationNode():
            return ()
        case _ as unreachable:
            assert_never(unreachable)


def _regrown(node: DocumentNode, flat: tuple[DocumentNode, ...], /) -> DocumentNode:
    match node:
        case PageNode() | StructureNode():
            return msgspec.structs.replace(node, children=flat)
        case SectionNode(heading=head):
            return msgspec.structs.replace(node, heading=flat[: len(head)], children=flat[len(head) :])
        case BlockNode(runs=runs):
            return msgspec.structs.replace(node, runs=flat[: len(runs)], children=flat[len(runs) :])
        case ListNode():
            return msgspec.structs.replace(node, items=flat)
        case TableNode(rows=rows):
            cells = iter(flat)
            return msgspec.structs.replace(node, rows=tuple(tuple(next(cells) for _ in row) for row in rows), caption=tuple(cells))
        case FigureNode():
            return msgspec.structs.replace(node, caption=flat)
        case _:
            return node


def _inked(node: DocumentNode, ink: RedlineInk, /) -> DocumentNode:
    match node:
        case RunNode(decorations=decos):
            return msgspec.structs.replace(node, color=ink.color, decorations=(*decos, *(d for d in ink.decorations if d not in decos)))
        case _:
            return node


def _stained(node: DocumentNode, ink: RedlineInk, /) -> DocumentNode:
    frames: Block[tuple[bool, DocumentNode]] = Block.singleton((False, node))
    results: Block[DocumentNode] = Block.empty()
    while not frames.is_empty():
        (combine, current), frames = frames.head(), frames.tail()
        parts = _flat(current)
        if combine:
            results = results.skip(len(parts)).cons(_regrown(current, tuple(results.take(len(parts)))[::-1]))
        elif parts:
            frames = Block.of_seq((False, part) for part in reversed(parts)).append(frames.cons((True, current)))
        else:
            results = results.cons(_inked(current, ink))
    return results.head()


def _delta_row(delta: DocumentDelta, /) -> tuple[str, str]:
    match delta:
        case Inserted(node=node):
            return ("inserted", node.meta.key.hex)
        case Deleted(key=key):
            return ("deleted", key.hex)
        case Moved(key=key, index=index):
            return ("moved", f"{key.hex}@{index}")
        case Replaced(node=node):
            return ("replaced", node.meta.key.hex)
        case Reparametrized(key=key, fields=fields):
            return ("reparametrized", f"{key.hex}:{','.join(sorted(field.name for field in fields))}")
        case _ as unreachable:
            assert_never(unreachable)


def _with_summary(tree: DocumentNode, deltas: tuple[DocumentDelta, ...], /) -> DocumentNode:
    fmt = tree.meta.key.fmt

    def cell(text: str, /) -> RunNode:
        meta = NodeMeta(key=ContentIdentity.key(fmt, text.encode()), role="redline", page=tree.meta.page)
        return RunNode(meta=meta, text=text, font_key="", size=9.0)

    rows = ((cell("edit"), cell("node")), *tuple((cell(kind), cell(target)) for kind, target in map(_delta_row, deltas)))
    meta = NodeMeta(key=ContentIdentity.key(fmt, _DELTA_ENCODER.encode(deltas)), role="redline-summary", page=tree.meta.page)
    summary = TableNode(meta=meta, rows=rows, header_rows=1)
    # A leaf root carries no spine for `_with_spine` to grow, so the summary materializes a grouping wrapper
    # instead of vanishing through the replace no-op — every redline promising a summary shows one.
    match tree:
        case PageNode() | StructureNode() | SectionNode() | BlockNode() | ListNode():
            return _with_spine(tree, (*_spine(tree), summary))
        case _:
            wrapper = NodeMeta(key=ContentIdentity.key(fmt, tree.meta.key.hex.encode() + meta.key.hex.encode()), role="redline-summary", page=tree.meta.page)
            return StructureNode(meta=wrapper, role=StandardRole(elt=StructEltKind.DIV), children=(tree, summary))


def invert(before: DocumentNode, deltas: tuple[DocumentDelta, ...], /) -> tuple[DocumentDelta, ...]:
    by_key = _by_key(_index(before))
    return tuple(Block.of_seq(deltas).map(lambda delta: _invert(delta, by_key, before)))[::-1]


def encode(deltas: tuple[DocumentDelta, ...]) -> bytes:
    return _DELTA_ENCODER.encode(deltas)


def decode(payload: bytes) -> Result[tuple[DocumentDelta, ...], DeltaFault]:
    return catch(exception=msgspec.DecodeError)(_DELTA_DECODER.decode)(payload).map_error(lambda _raised: DeltaFault.DECODE)


def _invert(delta: DocumentDelta, old: Map[ContentKey, IndexEntry], root: DocumentNode, /) -> DocumentDelta:
    match delta:
        case Inserted(node=node):
            return Deleted(key=node.meta.key)
        case Deleted(key=key):
            return old.try_find(key).map(lambda e: Inserted(parent=e[1], index=e[2], node=e[0]) if e[1] is not None else delta).default_value(delta)
        case Moved(key=key):
            return old.try_find(key).map(lambda e: Moved(key=key, parent=e[1], index=e[2]) if e[1] is not None else delta).default_value(delta)
        case Replaced():
            return Replaced(node=root)
        case Reparametrized(key=key):
            return old.try_find(key).map(lambda e: Reparametrized(key=key, fields=_all_fields(e[0]))).default_value(delta)
        case _ as unreachable:
            assert_never(unreachable)


def _field_delta(prior: DocumentNode, current: DocumentNode, /) -> tuple[FieldPatch, ...]:
    prior_fields = dict(_own(prior))
    return tuple(FieldPatch(name=name, value=msgspec.Raw(_DELTA_ENCODER.encode(value))) for name, value in _own(current) if prior_fields.get(name) != value)


def _all_fields(node: DocumentNode, /) -> tuple[FieldPatch, ...]:
    return tuple(FieldPatch(name=name, value=msgspec.Raw(_DELTA_ENCODER.encode(value))) for name, value in _own(node))


def _apply_fields(node: DocumentNode, fields: tuple[FieldPatch, ...], /) -> DocumentNode:
    # Total admission over the decoded overlay — one hostile member voids the WHOLE patch, degrading to the
    # established merge no-op rather than a silent partial apply: an unknown, tag, or spine name never rewrites the
    # discriminant or child spine, a duplicate name never last-wins, a torn or mistyped Raw payload never raises past
    # the merge, and a `meta` overlay that would re-key the target is refused so the applied node retains its located key.
    patchable = frozenset(node.__struct_fields__) - _SPINE_FIELDS
    names = tuple(field.name for field in fields)
    if not fields or len(frozenset(names)) != len(names) or not patchable.issuperset(names):
        return node
    merged = catch(exception=msgspec.MsgspecError)(
        lambda: msgspec.convert({**msgspec.to_builtins(node), **{field.name: msgspec.msgpack.decode(field.value) for field in fields}}, type(node))
    )()
    return merged.map(lambda applied: node if applied.meta.key != node.meta.key else applied).default_value(node)


def _rebuilt(tree: DocumentNode, path: Path, fn: Callable[[DocumentNode], DocumentNode | None], /) -> DocumentNode:
    chain = tuple(accumulate(path, lambda node, ordinal: _spine(node)[ordinal], initial=tree))

    def stitched(child: DocumentNode | None, ancestor: tuple[DocumentNode, int], /) -> DocumentNode:
        parent, ordinal = ancestor
        kids = _spine(parent)
        return _with_spine(parent, (*kids[:ordinal], *(() if child is None else (child,)), *kids[ordinal + 1 :]))

    patched = reduce(stitched, zip(reversed(chain[:-1]), reversed(path), strict=True), fn(chain[-1]))
    return tree if patched is None else patched


def _located(tree: DocumentNode, key: ContentKey, /) -> Option[Path]:
    return _identities(_index(tree)).try_find(key)


def _spliced(tree: DocumentNode, parent: ContentKey, index: int, node: DocumentNode, /) -> DocumentNode:
    def grown(target: DocumentNode) -> DocumentNode:
        kids = _spine(target)
        # A stale or hostile index outside 0..len(kids) degrades to the established merge no-op — never a
        # negative index silently wrapping onto the tail through slicing.
        return target if not 0 <= index <= len(kids) else _with_spine(target, (*kids[:index], node, *kids[index:]))

    return _located(tree, parent).map(lambda path: _rebuilt(tree, path, grown)).default_value(tree)


def _pruned(tree: DocumentNode, key: ContentKey, /) -> DocumentNode:
    return _located(tree, key).map(lambda path: _rebuilt(tree, path, lambda _target: None)).default_value(tree)


def _retargeted(tree: DocumentNode, key: ContentKey, fn: Callable[[DocumentNode], DocumentNode], /) -> DocumentNode:
    return _located(tree, key).map(lambda path: _rebuilt(tree, path, fn)).default_value(tree)


def _find(tree: DocumentNode, key: ContentKey, /) -> DocumentNode:
    return next(node for node in walk(tree) if node.meta.key == key)
```

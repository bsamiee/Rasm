# [PY_ARTIFACTS_MODEL]

The semantic document algebra — the single interior representation the `document` axis lowers FROM and recovers TO. `DocumentNode` is one recursive `msgspec` tagged-union tree carrying a closed `NodeMeta` on every node, and `DocumentDelta` is the one diff/merge edit algebra keyed by the stable content key; every `document/emit#DOCUMENT` backend is a lowering arm folding from this tree and `document/lens#LENS` rebuilds it, so production and extraction are inverses over one node algebra and a structural object-graph diff reuses the delta defined here once.

The tree round-trips through one shared deterministic `msgspec` codec, so a multi-PDF corpus is a content-keyed serialized value; identity comes from `ContentIdentity.of`, never re-minted. The `CorpusRow`/`CorpusRecord` egress projections feed the `data/tabular/columnar#SCAN` wire; `NodeMeta.classification` carries the CSI/OmniClass notation as a bounded string, so the substrate tree never depends on the `specification/classify#CLASSIFY` folder that lowers INTO it; `TextDirection` rides as interior data while the `../typography/shape#SHAPE` shaper owns the bidi reorder.

## [01]-[INDEX]

- [01]-[NODE]: the recursive tagged-union tree, its `StructRole` PDF/UA vocabulary, and the projection algebra every backend lowers from.
- [02]-[DELTA]: the four-variant edit algebra with `diff`/`merge`/`invert` defined once over the tree.

## [02]-[NODE]

- Owner: `DocumentNode` — one `tag`-discriminated `Union` on `tag_field="kind"`, every variant a frozen `Struct` carrying a `NodeMeta` value object; a flat class with a `kind: str` field and an `if kind ==` cascade is the rejected non-total shape.
- Cases: `TableNode` carries `spans` merged-cell quads plus `header_rows`/`footer_rows` counts BOTH lowerings honor and the lens recovers, and a `caption` run sequence — a publication table and an AEC schedule both title their grid. `FormulaNode` keeps the LaTeX source tree-resident, so an equation is source-addressable rather than only a pre-rendered `FigureNode`; `AnnotTarget` closes the link family with the AEC `Xref` detail-on-sheet/spec-section case.
- Entry: one shared deterministic `_ENCODER` serves the node digest, the corpus byte projection, and the public `encode` — never parallel identical instances; `node_digest` content-addresses the tree.
- Auto: `node_digest` folds `_own_bytes` (the container's non-child fields) beside the child digests, so an identical sub-tree keys identically while a re-parametrized container re-keys rather than colliding on its unchanged children. `alt_of` derives one `(AltText, AltStatus)` pair over the `FigureNode | FormulaNode` or-pattern, so the accessibility audit reads alt presence as one column predicate — and the Typst `_image` emitter writes `alt: none` for an un-authored figure, never a meaningless `alt: ""` that erases the `ABSENT` fact. `to_corpus(node, view)` is one view-keyed entrypoint whose `RECORD` projection exists because `pa.Table.from_pylist` rejects a `msgspec.Struct` — the producer owns the flat-record mapping so producer and consumer agree on the Arrow column shape. `_STRUCT_CATEGORY` is the one frozen behavior table keyed by `StructEltKind` carrying each role's `StructCategory` and `heading_level`, so the audit's nesting and heading-monotonicity checks fold one table row rather than a parallel `match`, `_STANDARD_FOR` derives from it by first-wins category inversion so the `document/tagged#ACCESS` `/RoleMap` foreign-to-standard lowering reads a derived row rather than a hand-kept parallel dict, `ForeignRole` is the one open arm over a `Meta`-constrained non-empty role, and `role_of`/`role_category`/`standard_for` are the model's one role-projection family the tagged owner consumes whole. `to_typst_source` escapes markup-context and string-context interpolations through one shared `maketrans` algebra, and a decorative `BlockKind.ARTIFACT` block lowers through `pdf.artifact[..]` so it is excluded from the tagged structure tree.
- Receipt: owns the tree type and its digest, never a receipt fold — authoring receipts stay at `document/emit`, recovery receipts at `document/lens`.
- Packages: `lxml.etree` defers under module-scope `lazy from`, so a Typst-only or corpus-only consumer never pays the libxml2 load; `msgspec` `UNSET` markers round-trip the wire-absent `NodeMeta` fields under `omit_defaults`.
- Growth: a new document concept is one variant plus one arm in each lowering the total `match` forces; a new standard PDF/UA role is one `StructEltKind` member plus one `_STRUCT_CATEGORY` row — `_STANDARD_FOR` absorbs a new category for free; a new run decoration, direction, baseline, list dialect, or link-target kind is one vocabulary member plus its markup row.
- Boundary: `to_json` is a real `msgspec.json` interchange serialization of the node tree a downstream consumer decodes — never a schema-shape blob no consumer reads; a Typst `label()`-anchored intra-compilation link is the rejected `Xref` form because the target sheet is a separate compilation the imposition assembly resolves; a `ClassCode` field on the interior tree is the rejected coupling that inverts the `specification`-to-`document` dependency.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
from collections.abc import Iterator
from enum import StrEnum
from functools import reduce
from typing import TYPE_CHECKING, Annotated, Final, Literal, assert_never, overload

import msgspec
from expression import pipe
from expression.collections import Block, Map
from msgspec import Meta, Struct, UnsetType, UNSET

from rasm.runtime.identity import ContentIdentity, ContentKey

lazy from lxml import etree  # the tree -> HTML/`_Element` lowering builder; cold, deferred to first `to_html`/`to_lxml_tree`

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
    ARTIFACT = "artifact"  # decorative content -> Typst `pdf.artifact`, excluded from the tag tree


class ListKind(StrEnum):
    UNORDERED = "unordered"
    ORDERED = "ordered"
    DESCRIPTION = "description"


class RunScript(StrEnum):
    NORMAL = "normal"
    SUPER = "super"
    SUB = "sub"


class TextDirection(StrEnum):
    AUTO = "auto"  # shaper resolves the base level from content (`bidi.get_base_level` → 0/1)
    LTR = "ltr"
    RTL = "rtl"  # the `bidi.get_display(base_dir="R")` paragraph the shaper reorders


class TextDecoration(StrEnum):
    UNDERLINE = "underline"
    STRIKETHROUGH = "strikethrough"
    OVERLINE = "overline"


class FieldKind(StrEnum):
    TEXT = "text"
    CHECKBOX = "checkbox"
    CHOICE = "choice"
    SIGNATURE = "signature"
    BUTTON = "button"


class FieldFlag(StrEnum):
    REQUIRED = "required"
    READONLY = "readonly"
    MULTILINE = "multiline"
    PASSWORD = "password"


class AnnotKind(StrEnum):
    HIGHLIGHT = "highlight"
    REDACTION = "redaction"
    LINK = "link"
    NOTE = "note"
    STAMP = "stamp"


class StructEltKind(StrEnum):
    DOCUMENT = "Document"  # grouping
    PART = "Part"
    ART = "Art"
    SECT = "Sect"
    DIV = "Div"
    TOC = "TOC"
    TOCI = "TOCI"
    INDEX = "Index"
    NONSTRUCT = "NonStruct"  # grouping with no inherent structure (PDF/UA generic container)
    PRIVATE = "Private"  # producer-private content outside the logical structure tree
    H1 = "H1"  # headings
    H2 = "H2"
    H3 = "H3"
    H4 = "H4"
    H5 = "H5"
    H6 = "H6"
    P = "P"  # block-level
    BLOCKQUOTE = "BlockQuote"
    NOTE = "Note"
    BIBENTRY = "BibEntry"
    CODE = "Code"
    CAPTION = "Caption"
    SPAN = "Span"  # inline-level
    QUOTE = "Quote"
    LINK = "Link"
    REFERENCE = "Reference"
    ANNOT = "Annot"
    RUBY = "Ruby"  # East-Asian ruby (furigana) assembly over its RB/RT/RP parts
    RB = "RB"  # ruby base text
    RT = "RT"  # ruby annotation text
    RP = "RP"  # ruby punctuation (fallback delimiters)
    WARICHU = "Warichu"  # East-Asian inline warichu assembly over its WT/WP parts
    WT = "WT"  # warichu text
    WP = "WP"  # warichu punctuation
    L = "L"  # list grouping
    LI = "LI"
    LBL = "Lbl"
    LBODY = "LBody"
    TABLE = "Table"  # table grouping
    THEAD = "THead"
    TBODY = "TBody"
    TFOOT = "TFoot"
    TR = "TR"
    TH = "TH"
    TD = "TD"
    FIGURE = "Figure"  # illustration
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
type AltText = Annotated[str, Meta(max_length=2048)]
type LangTag = Annotated[str, Meta(min_length=2, max_length=35, pattern=r"\A[A-Za-z]{2,3}(-[A-Za-z0-9]{2,8})*\Z")]
type MediaType = Annotated[str, Meta(min_length=3, max_length=127, pattern=r"\A[\w.+-]+/[\w.+-]+\Z")]
type ClassificationCode = Annotated[
    str, Meta(min_length=1, max_length=32, pattern=r"\A[A-Za-z0-9][\w .\-]*\Z")
]  # CSI/OmniClass notation `classify#CLASSIFY` renders/parses
type Rgb = tuple[int, int, int]
type Rect = tuple[float, float, float, float]

# --- [MODELS] ---------------------------------------------------------------------------


class NodeMeta(Struct, frozen=True, omit_defaults=True):
    key: ContentKey
    role: str
    page: int
    bounds: Rect | None = None
    lang: LangTag | UnsetType = UNSET  # PDF/UA `/Lang` BCP-47 tag; absent under `omit_defaults`
    actual_text: str | UnsetType = UNSET  # PDF/UA `/ActualText` replacement for non-textual glyphs
    classification: ClassificationCode | UnsetType = (
        UNSET  # CSI/OmniClass code the `specification/classify#CLASSIFY` resolver keys the drawing<->spec cross-reference on
    )


class StandardRole(Struct, frozen=True, tag="standard", tag_field="role_kind"):
    elt: StructEltKind


class ForeignRole(Struct, frozen=True, tag="foreign", tag_field="role_kind"):
    role: ForeignRoleStr


type StructRole = StandardRole | ForeignRole


class Uri(Struct, frozen=True, tag="uri", tag_field="target"):
    href: Annotated[str, Meta(min_length=1, max_length=4096)]


class Dest(Struct, frozen=True, tag="dest", tag_field="target"):
    page: int
    point: tuple[float, float] | None = None


class Xref(Struct, frozen=True, tag="xref", tag_field="target"):
    # the AEC cross-reference target the drawing/detail#DETAIL callout and the specification keynote both cite:
    # `detail`/`sheet` the `DetailRef.cite()` "3/A-501" detail-on-sheet coordinate, `code` the governing
    # `specification/classify#CLASSIFY` `ClassCode.render()` section — so a drawing<->spec cross-reference
    # resolves over the one tree, its cross-sheet target string resolved at `composition/imposition` assembly.
    sheet: str = ""
    detail: str = ""
    code: str = ""

    def cite(self) -> str:
        return f"{self.detail}/{self.sheet}" if self.detail and self.sheet else self.sheet or self.code


class NoTarget(Struct, frozen=True, tag="none", tag_field="target"):
    pass


type AnnotTarget = Uri | Dest | Xref | NoTarget


class PageNode(Struct, frozen=True, tag=NodeKind.PAGE.value, tag_field="kind"):
    meta: NodeMeta
    media_box: Rect
    children: tuple[DocumentNode, ...] = ()


class SectionNode(Struct, frozen=True, tag=NodeKind.SECTION.value, tag_field="kind"):
    meta: NodeMeta
    level: int
    heading: tuple[RunNode, ...] = ()
    children: tuple[DocumentNode, ...] = ()


class BlockNode(Struct, frozen=True, tag=NodeKind.BLOCK.value, tag_field="kind"):
    meta: NodeMeta
    block: BlockKind
    level: int = 1
    runs: tuple[RunNode, ...] = ()
    children: tuple[DocumentNode, ...] = ()


class RunNode(Struct, frozen=True, tag=NodeKind.RUN.value, tag_field="kind"):
    meta: NodeMeta
    text: str
    font_key: str
    size: float
    weight: int = 400
    italic: bool = False
    direction: TextDirection = TextDirection.AUTO
    script: RunScript = RunScript.NORMAL
    decorations: tuple[TextDecoration, ...] = ()
    color: Rgb = (0, 0, 0)


class ListNode(Struct, frozen=True, tag=NodeKind.LIST.value, tag_field="kind"):
    meta: NodeMeta
    list_kind: ListKind = ListKind.UNORDERED
    start: int = 1  # `ORDERED` first ordinal -> Typst `#enum(start:)`
    items: tuple[DocumentNode, ...] = ()  # one `LI` sub-tree per item


class TableNode(Struct, frozen=True, tag=NodeKind.TABLE.value, tag_field="kind"):
    meta: NodeMeta
    rows: tuple[tuple[DocumentNode, ...], ...] = ()
    spans: tuple[tuple[int, int, int, int], ...] = ()  # (row, present-cell index, col_span, row_span) merged-cell quads BOTH lowerings honor
    header_rows: int = 0  # leading `THead` rows -> Typst `table.header(repeat: true)` + `Table.header`
    footer_rows: int = 0  # trailing `TFoot` rows -> Typst `table.footer`
    caption: tuple[RunNode, ...] = ()  # table title -> Typst `#figure(kind: table, caption:)` + HTML `<caption>`, the PDF/UA `Caption` child


class FigureNode(Struct, frozen=True, tag=NodeKind.FIGURE.value, tag_field="kind"):
    meta: NodeMeta
    asset_key: ContentKey
    alt: AltText = ""
    media_type: MediaType = "image/png"
    intrinsic: tuple[float, float] | None = None
    caption: tuple[RunNode, ...] = ()


class FormulaNode(Struct, frozen=True, tag=NodeKind.FORMULA.value, tag_field="kind"):
    # the tree-resident equation the `FORMULA` `StructEltKind` role lowers, so a formula is source-addressable
    # (journal manuscript egress + AEC `ziamath` SVG) rather than only a pre-rendered `FigureNode`.
    meta: NodeMeta
    tex: str  # LaTeX math source `to_latex`/`to_markdown` emit verbatim, `ziamath` lowers to SVG; a TRUSTED authored math island, never markup-escaped
    display: bool = False  # block/display math (`\[..\]`, `$$..$$`, Typst `#mitex`) vs inline (`$..$`, Typst `#mi`)
    alt: AltText = ""  # the ISO 14289 `Formula` structure-element `/Alt` text equivalent the `folder:document/tagged#ACCESS` AUDIT verifies


class FieldNode(Struct, frozen=True, tag=NodeKind.FIELD.value, tag_field="kind"):
    meta: NodeMeta
    name: str
    field: FieldKind
    value: str | bool | None = None
    flags: tuple[FieldFlag, ...] = ()
    options: tuple[str, ...] = ()  # `CHOICE` candidate values


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
    classification: str = ""  # the `NodeMeta.classification` CSI/OmniClass column the `classify#CLASSIFY` resolver queries
    xref: str = ""  # the `AnnotationNode` `Xref.cite()` column the drawing<->spec cross-reference resolver reads


# --- [CONSTANTS] ------------------------------------------------------------------------

_ENCODER: Final = msgspec.msgpack.Encoder(order="deterministic")  # one deterministic codec for node, digest, and corpus
_JSON_ENCODER: Final = msgspec.json.Encoder(
    order="deterministic"
)  # the structured-data (JSON-LD/JATS-adjacent) tree egress, the `to_json` interchange codec
_DOCUMENT_DECODER: Final = msgspec.msgpack.Decoder(DocumentNode)
_CHILD_FIELDS: Final[frozenset[str]] = frozenset({"children", "heading", "runs", "items", "caption", "rows"})  # every `children`-projected field
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
])  # `HEADING` -> `h{level}` and `CODE` -> `pre`/`code` are arm-built; this table carries the flat one-tag block kinds
_LIST_HTML: Final[Map[ListKind, str]] = Map.of_seq([(ListKind.UNORDERED, "ul"), (ListKind.ORDERED, "ol"), (ListKind.DESCRIPTION, "dl")])
# the plain-text manuscript spelling tables the `to_markdown`/`to_latex` lowerings read — the same
# markup-table discipline `_TYPST_ESCAPE`/`_BLOCK_HTML` hold, one row per active char / decoration / depth.
_MD_ESCAPE: Final[dict[int, str]] = str.maketrans({
    c: f"\\{c}" for c in "\\`*_[]<>|"
})  # CommonMark inline-active set: neutralize emphasis/code/link/autolink/table-pipe, never mangle every hyphen/period
_MD_DECORATION: Final[Map[TextDecoration, tuple[str, str]]] = Map.of_seq([
    (TextDecoration.UNDERLINE, ("<u>", "</u>")),  # GFM raw-HTML — CommonMark has no native underline
    (TextDecoration.STRIKETHROUGH, ("~~", "~~")),  # GFM strikethrough
    (TextDecoration.OVERLINE, ('<span style="text-decoration:overline">', "</span>")),  # GFM raw-HTML — no native overline
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
})  # the ten LaTeX-active characters — the three control-word forms plus the seven single-backslash escapes
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
])  # the `document/emit#DOCUMENT LATEX` preamble carries `ulem` (uline/sout) and a math-mode overline; the emit arm declares the package set

# --- [TABLES] ---------------------------------------------------------------------------

# The ONE primary correspondence: role -> (category, heading_level). The FIRST row of each category
# is its canonical role, so `_STANDARD_FOR` derives by first-wins inversion rather than a parallel literal.
_STRUCT_CATEGORY: Final[Map[StructEltKind, tuple[StructCategory, int]]] = Map.of_seq([
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
)  # an unknown role maps to a neutral grouping, never a figure carrying mandatory alt
# DERIVED secondary: category -> its canonical standard role (the `/RoleMap` target the tagged owner reads),
# first-wins inversion of `_STRUCT_CATEGORY` so the canonical is the first-declared role of each category.
_STANDARD_FOR: Final[Map[StructCategory, StructEltKind]] = Map.of_seq(
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


def walk(node: DocumentNode) -> Iterator[DocumentNode]:
    stack = Block.singleton(node)
    while not stack.is_empty():  # Exemption: iterative pre-order frontier — native recursion overflows on an adversarial-depth tree
        head, stack = stack.head(), stack.tail()
        yield head
        stack = Block.of_seq(children(head)).append(stack)  # children before siblings keeps document order


def _own_bytes(node: DocumentNode, /) -> bytes:
    return _ENCODER.encode(msgspec.structs.replace(node, **{name: () for name in node.__struct_fields__ if name in _CHILD_FIELDS}))


def node_digest(node: DocumentNode) -> ContentKey:
    # depth-safe expand/combine frontier: a leaf keys its encoded bytes, a branch keys (own, *child keys)
    # in document order; the two immutable stacks replace the native recursion an adversarial tree overflows.
    frontier: Block[tuple[bool, DocumentNode]] = Block.singleton((False, node))  # (combine?, node)
    results: Block[ContentKey] = Block.empty()
    while not frontier.is_empty():  # Exemption: depth-safe digest frontier over the recursive node tree
        (combine, current), frontier = frontier.head(), frontier.tail()
        kids = children(current)
        if not kids:
            results = results.cons(ContentIdentity.of(current.meta.key.fmt, _ENCODER.encode(current)))
        elif combine:  # the reversed child push above resolves the kids onto `results` head in document order
            own = ContentIdentity.of(current.meta.key.fmt, _own_bytes(current))
            results = results.skip(len(kids)).cons(ContentIdentity.of(current.meta.key.fmt, (own, *results.take(len(kids)))))
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
    # the `AnnotationNode` `Xref` citation projected to the corpus `xref` column so the drawing<->spec
    # cross-reference resolver reads a column predicate over the corpus, exactly as the audit reads `alt_status`.
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
        text="".join(run.text for run in walk(node) if isinstance(run, RunNode)),
        alt=alt,
        alt_status=status,
        lang="" if isinstance(node.meta.lang, UnsetType) else node.meta.lang,
        actual_text="" if isinstance(node.meta.actual_text, UnsetType) else node.meta.actual_text,
        classification="" if isinstance(node.meta.classification, UnsetType) else node.meta.classification,
        xref=_link_cite(node),
    )
    match view:
        case CorpusView.STRUCT:
            return row
        case CorpusView.BYTES:
            return _ENCODER.encode(row)
        case CorpusView.RECORD:  # `from_pylist` rejects a `Struct`; the producer owns the flat-record projection
            return msgspec.to_builtins(row)
        case _ as unreachable:
            assert_never(unreachable)


def to_typst_source(node: DocumentNode, *, title: str | None = None) -> str:
    # `title` prepends the escaped `#set document(title: "..")` set-rule the `document/emit#DOCUMENT` PDF/UA
    # variants require (a `ua-1` render hard-errors `missing document title` without it); the STRING-context
    # `_typst` escaper owns the quoting, so the emit seam composes this rather than a hand-rolled `.replace`.
    # The recursion routes through the default-`title=None` path, so the set-rule lands ONCE at the root.
    prelude = f'#set document(title: "{_typst(title, TypstScope.STRING)}")\n' if title is not None else ""
    return prelude + _typst_body(node)


def _typst_body(node: DocumentNode) -> str:
    match node:
        case RunNode():
            return _styled(node)
        case BlockNode(block=BlockKind.HEADING, level=level, runs=runs):
            return _heading(level, runs)
        case BlockNode(block=BlockKind.ARTIFACT, runs=runs, children=kids):
            return f"#pdf.artifact[{_runs(runs)}{''.join(to_typst_source(child) for child in kids)}]\n"
        case BlockNode(block=BlockKind.QUOTE, runs=runs, children=kids):
            return f"#quote(block: true)[{_runs(runs)}{''.join(to_typst_source(child) for child in kids)}]\n"
        case BlockNode(block=BlockKind.CODE, runs=runs):
            return f'#raw("{_typst("".join(run.text for run in runs), TypstScope.STRING)}", block: true)\n'
        case BlockNode(runs=runs, children=kids):
            return _runs(runs) + "".join(to_typst_source(child) for child in kids) + "\n"
        case ListNode(list_kind=ListKind.DESCRIPTION, items=items):
            return f"#terms({', '.join(_term_pair(item) for item in items)})\n"
        case ListNode(list_kind=ListKind.ORDERED, start=start, items=items):
            return f"#enum({f'start: {start}, ' if start != 1 else ''}{_items(items)})\n"
        case ListNode(list_kind=kind, items=items):
            return f"#{_LIST_MARKUP[kind]}({_items(items)})\n"
        case SectionNode(level=level, heading=head, children=kids):
            return _heading(level, head) + "".join(to_typst_source(child) for child in kids)
        case PageNode(children=kids):
            return "".join(to_typst_source(child) for child in kids) + "#pagebreak()\n"
        case StructureNode(children=kids):
            return "".join(to_typst_source(child) for child in kids)
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
            # the LaTeX source rides Typst's `mitex` LaTeX-math bridge (the `@preview` registry package the
            # `document/emit#DOCUMENT` Typst preamble imports as `#import "@preview/mitex": mi, mitex`, resolved
            # through the typst compiler's package cache) — `#mitex` for a display block, `#mi` inline; the tex is
            # STRING-escaped so a `\`/`"` survives the Typst string literal into the LaTeX the bridge parses.
            return f'#mitex("{_typst(tex, TypstScope.STRING)}")\n' if display else f'#mi("{_typst(tex, TypstScope.STRING)}")'
        case AnnotationNode(annot=AnnotKind.LINK, contents=text, link=Uri(href=href)):
            return f'#link("{_typst(href, TypstScope.STRING)}")[{_typst(text, TypstScope.MARKUP)}]'
        case AnnotationNode(annot=AnnotKind.LINK, contents=text, link=Dest(page=page, point=point)):
            x, y = point if point else (0.0, 0.0)
            return f"#link((page: {page + 1}, x: {x}pt, y: {y}pt))[{_typst(text, TypstScope.MARKUP)}]"
        case AnnotationNode(annot=AnnotKind.LINK, contents=text, link=Xref() as xref):
            # the cross-sheet citation is a destination string `composition/imposition` sheet-set assembly resolves — never a `label()` this compilation carries
            return f'#link("{_typst(xref.cite(), TypstScope.STRING)}")[{_typst(text, TypstScope.MARKUP)}]'
        case FieldNode() | AnnotationNode():
            return ""
        case _ as unreachable:
            assert_never(unreachable)


def encode(node: DocumentNode) -> bytes:
    return _ENCODER.encode(node)


def decode(payload: bytes) -> DocumentNode:
    return _DOCUMENT_DECODER.decode(payload)


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
    return ", ".join(f"[{to_typst_source(item).strip()}]" for item in items)


def _span_map(spans: tuple[tuple[int, int, int, int], ...]) -> frozendict[tuple[int, int], tuple[int, int]]:
    return frozendict({(row, col): (col_span, row_span) for row, col, col_span, row_span in spans})


def _column_count(rows: tuple[tuple[DocumentNode, ...], ...], span_map: frozendict[tuple[int, int], tuple[int, int]]) -> int:
    # grid width folds row 0's per-cell colspans (a top-row cell is never covered from above), so a merged header spans correctly
    return sum(span_map.get((0, col), (1, 1))[0] for col in range(len(rows[0]))) if rows else 0


def _cells(rows: tuple[tuple[DocumentNode, ...], ...], span_map: frozendict[tuple[int, int], tuple[int, int]], base: int) -> str:
    return ", ".join(_cell_markup(to_typst_source(cell), span_map.get((base + r, c))) for r, row in enumerate(rows) for c, cell in enumerate(row))


def _cell_markup(content: str, span: tuple[int, int] | None) -> str:
    args = (
        ""
        if span is None
        else ", ".join(part for part in (f"colspan: {span[0]}" if span[0] != 1 else "", f"rowspan: {span[1]}" if span[1] != 1 else "") if part)
    )
    return f"table.cell({args})[{content}]" if args else f"[{content}]"


def _term_pair(item: DocumentNode) -> str:
    kids = children(item)
    term = to_typst_source(kids[0]).strip() if kids else to_typst_source(item).strip()
    body = "".join(to_typst_source(child) for child in kids[1:]).strip()
    return f"terms.item([{term}], [{body}])"


def _runs(runs: tuple[RunNode, ...]) -> str:
    return "".join(_styled(run) for run in runs)


def _heading(level: int, runs: tuple[RunNode, ...]) -> str:
    return f"{'=' * min(max(level, 1), 6)} {_runs(runs)}\n"


def _image(asset_key: ContentKey, alt: AltText) -> str:
    source = _typst(asset_key.hex, TypstScope.STRING)
    equiv = f'"{_typst(alt, TypstScope.STRING)}"' if alt else "none"
    return f'image("{source}", alt: {equiv})'


def to_html(node: DocumentNode) -> str:
    # the tree -> HTML lowering the `document/emit#DOCUMENT PDF_HTML` weasyprint arm consumes; serialized
    # from the one escape-safe `_element` builder so a run carrying `<`/`&`/`"` produces valid markup,
    # never an f-string splice the TEMPLATE-SAFETY law rejects.
    return etree.tostring(_element(node), method="html", encoding="unicode")


def to_lxml_tree(node: DocumentNode) -> "_Element":
    # the tree -> lxml `_Element` lowering the `XML`/`XML_TRANSFORM`/`XML_VALIDATE`/`XML_QUERY` arms fold
    # through `etree.tostring`/`XSLT`/`XPath`; one builder serves both HTML and the XML object tree.
    return _element(node)


def to_c14n(node: DocumentNode) -> bytes:
    # the deterministic canonical-XML (Canonical XML 2.0) egress the journal/archival JATS-adjacent structured
    # interchange consumes: `method="c14n2"` fixes attribute order, namespace prefixes, and whitespace so two
    # structurally-identical trees serialize byte-identically, the archival counterpart to the `method="html"`
    # `to_html` presentation lowering the same `_element` builder feeds — a stable content key over the XML form.
    return etree.tostring(_element(node), method="c14n2")


def to_json(node: DocumentNode) -> bytes:
    # the structured-data interchange egress: the whole recursive tagged-union tree lowered to deterministic JSON
    # the JSON-LD/JATS-adjacent structured lowering, distinct from the msgpack `encode`/`to_corpus(BYTES)` byte forms.
    return _JSON_ENCODER.encode(node)


def _wrapped(inner: "_Element", tag: str) -> "_Element":
    outer = etree.Element(tag)  # Exemption: the `lxml.etree` element builder is the platform-forced markup seam the template-safety law mandates
    outer.append(inner)
    return outer


def _run_element(run: RunNode) -> "_Element":
    inner = etree.Element("span")
    inner.text = run.text  # lxml escapes on serialize; never an f-string interpolation into markup
    decoration = " ".join(_DECORATION_CSS[deco] for deco in run.decorations)
    style = "; ".join(
        part
        for part in (
            f"color:rgb({run.color[0]},{run.color[1]},{run.color[2]})" if run.color != (0, 0, 0) else "",
            f"text-decoration:{decoration}" if decoration else "",
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
    for run in runs:  # Exemption: lxml element assembly is the platform markup builder, escape-safe by construction
        element.append(_run_element(run))
    for kid in kids:
        element.append(_element(kid))
    return element


def _element(node: DocumentNode) -> "_Element":
    match node:
        case RunNode():
            return _run_element(node)
        case BlockNode(block=BlockKind.HEADING, level=level, runs=runs):
            return _filled(etree.Element(f"h{min(max(level, 1), 6)}"), runs)
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
            section.insert(0, _filled(etree.Element(f"h{min(max(level, 1), 6)}"), head))
            return section
        case PageNode(children=kids):
            page = _filled(etree.Element("div"), (), kids)
            page.set("class", "page")
            return page
        case StructureNode(children=kids):
            structured = _filled(etree.Element("div"), (), kids)
            structured.set("role", role_of(node))
            return structured
        case TableNode(rows=rows, spans=spans, header_rows=head_n, footer_rows=foot_n, caption=caption):
            return _table_element(rows, head_n, foot_n, _span_map(spans), caption)
        case FigureNode(asset_key=asset_key, alt=alt, caption=caption):
            figure = etree.Element("figure")
            image = etree.SubElement(figure, "img")
            image.set("src", asset_key.hex)
            image.set("alt", alt)  # the `AltStatus.ABSENT` empty string stays the audited fact, never invented
            return _filled(figure, caption) if caption else figure
        case FormulaNode(tex=tex, display=display, alt=alt):
            math = etree.Element("div" if display else "span")  # MathJax/KaTeX-delimited LaTeX — the journal-web math convention
            math.set("class", "math display" if display else "math")
            math.set("role", "math")
            if alt:
                math.set("aria-label", alt)  # the WCAG text equivalent for the `Formula` structure element
            math.text = (
                f"\\[{tex}\\]" if display else f"\\({tex}\\)"
            )  # lxml escapes `<`/`&`/`"` on serialize; the LaTeX body is never an f-string markup splice
            return math
        case AnnotationNode(annot=AnnotKind.LINK, contents=text, link=Uri(href=href)):
            return _anchor(href, text)
        case AnnotationNode(annot=AnnotKind.LINK, contents=text, link=Dest(page=page)):
            return _anchor(f"#page-{page + 1}", text)
        case AnnotationNode(annot=AnnotKind.LINK, contents=text, link=Xref() as xref):
            return _anchor(f"#{xref.cite()}", text)  # the sheet-set cross-reference fragment the imposition assembly resolves
        case FieldNode() | AnnotationNode():
            return etree.Element("span")  # a non-link annotation or form field carries no inline HTML body
        case _ as unreachable:
            assert_never(unreachable)


def _anchor(href: str, text: str) -> "_Element":
    anchor = etree.Element("a")
    anchor.set("href", href)
    anchor.text = text
    return anchor


def _filled_terms(dl: "_Element", items: tuple[DocumentNode, ...]) -> "_Element":
    for item in items:  # one `<dt>`/`<dd>` pair per description item, the head child the term and the tail the body
        kids = children(item)
        etree.SubElement(dl, "dt").append(_element(kids[0]) if kids else _element(item))
        _filled(etree.SubElement(dl, "dd"), (), kids[1:])
    return dl


def _table_element(
    rows: tuple[tuple[DocumentNode, ...], ...],
    head_n: int,
    foot_n: int,
    span_map: frozendict[tuple[int, int], tuple[int, int]],
    caption: tuple[RunNode, ...],
) -> "_Element":
    table = etree.Element("table")
    if caption:  # the PDF/UA `Caption` element is the first child of `<table>`
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
                td = etree.SubElement(line, cell_tag)
                if span and span[0] != 1:
                    td.set("colspan", str(span[0]))
                if span and span[1] != 1:
                    td.set("rowspan", str(span[1]))
                td.append(_element(cell))
    return table


def to_markdown(node: DocumentNode) -> str:
    # the tree -> CommonMark/GFM manuscript lowering the `document/emit#DOCUMENT MARKDOWN` arm encodes: the
    # plain-text diffable egress of the SAME bound tree the PDF/HTML/Typst arms lower, every interpolated
    # `RunNode.text`/heading/caption escaped through the `_MD_ESCAPE` maketrans (trusted-node input, the same
    # f-string-plus-escaper form `to_typst_source` holds) so a `*`/`_`/`[`/`|` never opens spurious markup,
    # the super/sub/underline/overline/colour appearance carried as GFM raw HTML CommonMark cannot express.
    match node:
        case RunNode():
            return _md_styled(node)
        case BlockNode(block=BlockKind.HEADING, level=level, runs=runs):
            return f"{'#' * min(max(level, 1), 6)} {_md_runs(runs)}\n\n"
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
            return f"{'#' * min(max(level, 1), 6)} {_md_runs(head)}\n\n" + "".join(to_markdown(child) for child in kids)
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
            return f"$$\n{tex}\n$$\n\n" if display else f"${tex}$"  # GFM/Pandoc math; the LaTeX island is verbatim, never `_md`-escaped
        case AnnotationNode(annot=AnnotKind.LINK, contents=text, link=Uri(href=href)):
            return f"[{_md(text)}]({href})"
        case AnnotationNode(annot=AnnotKind.LINK, contents=text, link=Dest(page=page)):
            return f"[{_md(text)}](#page-{page + 1})"
        case AnnotationNode(annot=AnnotKind.LINK, contents=text, link=Xref() as xref):
            return f"[{_md(text)}](#{xref.cite()})"
        case FieldNode() | AnnotationNode():
            return ""
        case _ as unreachable:
            assert_never(unreachable)


def to_latex(node: DocumentNode) -> str:
    # the tree -> LaTeX manuscript lowering the `document/emit#DOCUMENT LATEX` arm encodes: the journal-submission
    # egress of the SAME bound tree, every interpolated `RunNode.text`/heading/caption escaped through the
    # `_LATEX_ESCAPE` maketrans so a `&`/`%`/`$`/`_`/`#`/`{`/`}`/`~`/`^`/`\` never breaks the source, the section
    # depth keyed by `_LATEX_SECTION` and the `hyperref`/`graphicx`/`ulem` control words the emit-side preamble carries.
    match node:
        case RunNode():
            return _latex_styled(node)
        case BlockNode(block=BlockKind.HEADING, level=level, runs=runs):
            return f"\\{_LATEX_SECTION[min(max(level, 1), 6)]}{{{_latex_runs(runs)}}}\n\n"
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
            return f"\\{_LATEX_SECTION[min(max(level, 1), 6)]}{{{_latex_runs(head)}}}\n\n" + "".join(to_latex(child) for child in kids)
        case PageNode(children=kids):
            return "".join(to_latex(child) for child in kids) + "\\clearpage\n"
        case StructureNode(children=kids):
            return "".join(to_latex(child) for child in kids)
        case TableNode(rows=rows, spans=spans, header_rows=head_n, footer_rows=foot_n, caption=caption):
            return _latex_table(rows, head_n, foot_n, _span_map(spans), caption)
        case FigureNode(asset_key=asset_key, alt=alt, caption=caption):
            cap = f"\\caption{{{_latex_runs(caption)}}}\n" if caption else ""
            note = f"% alt: {_latex(alt)}\n" if alt else ""  # the alt equivalent rides a source comment — LaTeX carries no figure `alt` slot
            return f"\\begin{{figure}}\n\\centering\n{note}\\includegraphics{{{asset_key.hex}}}\n{cap}\\end{{figure}}\n\n"
        case FormulaNode(tex=tex, display=display):
            return (
                f"\\[\n{tex}\n\\]\n\n" if display else f"${tex}$"
            )  # native LaTeX math, the source verbatim (escaping the `tex` island would corrupt the math)
        case AnnotationNode(annot=AnnotKind.LINK, contents=text, link=Uri(href=href)):
            return f"\\href{{{href}}}{{{_latex(text)}}}"
        case AnnotationNode(annot=AnnotKind.LINK, contents=text, link=Dest(page=page)):
            return f"\\hyperlink{{page-{page + 1}}}{{{_latex(text)}}}"
        case AnnotationNode(annot=AnnotKind.LINK, contents=text, link=Xref() as xref):
            return f"\\hyperref[{xref.cite()}]{{{_latex(text)}}}"  # the sheet-set cross-reference the imposition assembly resolves
        case FieldNode() | AnnotationNode():
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
    # the pandoc description-list spelling: a bold term line then a `: `-prefixed body the head/tail child split feeds
    kids = children(item)
    term = to_markdown(kids[0]).strip() if kids else to_markdown(item).strip()
    body = "".join(to_markdown(child) for child in kids[1:]).strip()
    return f"**{term}**\n: {body}\n"


def _md_table(rows: tuple[tuple[DocumentNode, ...], ...], header_rows: int, caption: tuple[RunNode, ...]) -> str:
    # a GFM pipe table — the leading `header_rows or 1` rows form the header band above the `---` delimiter,
    # merged cells flattening to their top-left content (GFM carries no colspan/rowspan), the `caption` a
    # titling paragraph below; a cell's own newlines/pipes are neutralized so one logical row stays one line.
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
    # a `tabular` inside a `table` float — the column spec folds row 0's colspans to the grid width, an `\hline`
    # rules the header/footer band boundaries (the `head_n`/`foot_n` counts), a colspan cell rides `\multicolumn`
    # (rowspan flattens to the top-left cell, `\multirow` an emit-preamble growth axis), the `caption` titling the float.
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
- Entry: `diff(before, after)` folds the two trees into an ordered edit tuple, `merge(tree, deltas)` folds the patch back, and `invert(before, deltas)` maps each edit to its inverse — a redaction patch is reversible until burned in; the patch round-trips through `msgspec.msgpack` as a content-keyed serialized value.
- Auto: `_index` keys each node by its structural PATH-vector (the child-ordinal sequence from the root), so two identical-content siblings stay distinct slots where a content-derived key silently overwrites one; `_identities` derives the `Map[ContentKey, Path]` so move/reparametrize detection keys on the position-stable `NodeMeta.key` — NEVER on `node_digest`, whose Merkle fold re-keys every ancestor when a descendant changes and spuriously `Moved`s every sibling of an edit. Two distinct keyings, never conflated. A node whose key is new under an also-new parent is carried inside that parent's `Inserted` subtree and emits nothing — only the topmost insert and the topmost survivor delete are edits.
- Receipt: the delta count and changed-node keys ride the lens introspection receipt facts; `DocumentDelta` mints no receipt of its own.
- Growth: a new edit kind is one variant plus one `diff` emit arm and one `merge` apply arm — the totality `match` forces both; a new diff granularity is a `node_digest` policy change, never a parallel delta family.
- Boundary: a per-consumer diff type (a document diff beside a geometry diff beside a wire diff) is the deleted form — `DocumentDelta` is the one edit algebra keyed by `ContentKey`. No positional list patching by index-shift heuristics outside the key algebra, and no second merge owner. Structural insertion/deletion/move targets the spine containers that own a `children` field through `_spine`/`_with_spine`; a `TableNode` cell grid, a `FigureNode` caption, a `SectionNode` heading, a `ListNode` item bag, and a `BlockNode` inline-run bag are bounded OWN-content sub-payloads re-keyed as a whole through `Reparametrized`, so `_spine` carries only the container `children` field and the sub-payload edits ride the own-field overlay. The fold is total over the four-variant union; a missing arm is an `assert_never` static failure.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
from collections.abc import Callable
from typing import Final, assert_never

import msgspec
from expression import Nothing, Option, Some
from expression.collections import Block, Map
from msgspec import Struct

from rasm.runtime.identity import ContentKey

from .model import BlockNode, DocumentNode, ListNode, PageNode, SectionNode, StructureNode, walk

# --- [TYPES] ----------------------------------------------------------------------------

type Path = tuple[int, ...]  # the child-ordinal vector from the root: a node's structural uid
type IndexEntry = tuple[DocumentNode, ContentKey | None, int]

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


class Reparametrized(Struct, frozen=True, tag="reparametrized", tag_field="edit"):
    key: ContentKey
    fields: dict[str, msgspec.Raw]


type DocumentDelta = Inserted | Deleted | Moved | Reparametrized

# --- [CONSTANTS] ------------------------------------------------------------------------

_DELTA_ENCODER: Final = msgspec.msgpack.Encoder(order="deterministic")
_DELTA_DECODER: Final = msgspec.msgpack.Decoder(tuple[DocumentDelta, ...])

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
    return tuple((name, getattr(node, name)) for name in node.__struct_fields__ if name not in {"children", "items"})


def _index(root: DocumentNode, /) -> Map[Path, IndexEntry]:
    # keyed by the structural path-vector so two identical-content siblings never collide on a content-derived
    # `NodeMeta.key`; the entry carries the parent `NodeMeta.key` and ordinal the ContentKey-addressed deltas need.
    def walk_spine(table: Map[Path, IndexEntry], node: DocumentNode, path: Path, parent: ContentKey | None, position: int) -> Map[Path, IndexEntry]:
        seeded = table.add(path, (node, parent, position))
        return Block.of_seq(enumerate(_spine(node))).fold(
            lambda acc, pair: walk_spine(acc, pair[1], (*path, pair[0]), node.meta.key, pair[0]), seeded
        )

    return walk_spine(Map.empty(), root, (), None, 0)


def _identities(index: Map[Path, IndexEntry], /) -> Map[ContentKey, Path]:
    return Block.of_seq(index.items()).fold(lambda acc, item: acc.add(item[1][0].meta.key, item[0]), Map.empty())


def _by_key(index: Map[Path, IndexEntry], /) -> Map[ContentKey, IndexEntry]:
    return Block.of_seq(index.items()).fold(lambda acc, item: acc.add(item[1][0].meta.key, item[1]), Map.empty())


def diff(before: DocumentNode, after: DocumentNode, /) -> tuple[DocumentDelta, ...]:
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
            return Block.empty()
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
                return _splice(patched, parent, index, node)
            case Deleted(key=key):
                return _prune(patched, key)
            case Moved(key=key, parent=parent, index=index):
                return _splice(_prune(patched, key), parent, index, _find(tree, key))
            case Reparametrized(key=key, fields=fields):
                return _retarget(patched, key, lambda node: _apply_fields(node, fields))
            case _ as unreachable:
                assert_never(unreachable)

    return Block.of_seq(deltas).fold(apply, tree)


def invert(before: DocumentNode, deltas: tuple[DocumentDelta, ...], /) -> tuple[DocumentDelta, ...]:
    by_key = _by_key(_index(before))
    return tuple(Block.of_seq(deltas).map(lambda delta: _invert(delta, by_key)))[::-1]


def encode(deltas: tuple[DocumentDelta, ...]) -> bytes:
    return _DELTA_ENCODER.encode(deltas)


def decode(payload: bytes) -> tuple[DocumentDelta, ...]:
    return _DELTA_DECODER.decode(payload)


def _invert(delta: DocumentDelta, old: Map[ContentKey, IndexEntry], /) -> DocumentDelta:
    match delta:
        case Inserted(node=node):
            return Deleted(key=node.meta.key)
        case Deleted(key=key):
            return old.try_find(key).map(lambda e: Inserted(parent=e[1], index=e[2], node=e[0]) if e[1] is not None else delta).default_value(delta)
        case Moved(key=key):
            return old.try_find(key).map(lambda e: Moved(key=key, parent=e[1], index=e[2]) if e[1] is not None else delta).default_value(delta)
        case Reparametrized(key=key):
            return old.try_find(key).map(lambda e: Reparametrized(key=key, fields=_all_fields(e[0]))).default_value(delta)
        case _ as unreachable:
            assert_never(unreachable)


def _field_delta(prior: DocumentNode, current: DocumentNode, /) -> dict[str, msgspec.Raw]:
    prior_fields = dict(_own(prior))
    return {name: msgspec.Raw(_DELTA_ENCODER.encode(value)) for name, value in _own(current) if prior_fields.get(name) != value}


def _all_fields(node: DocumentNode, /) -> dict[str, msgspec.Raw]:
    return {name: msgspec.Raw(_DELTA_ENCODER.encode(value)) for name, value in _own(node)}


def _apply_fields(node: DocumentNode, fields: dict[str, msgspec.Raw], /) -> DocumentNode:
    merged = {**msgspec.to_builtins(node), **{name: msgspec.msgpack.decode(raw) for name, raw in fields.items()}}
    return msgspec.convert(merged, type(node))


def _splice(tree: DocumentNode, parent: ContentKey, index: int, node: DocumentNode, /) -> DocumentNode:
    return _retarget(tree, parent, lambda target: _with_spine(target, (*_spine(target)[:index], node, *_spine(target)[index:])))


def _prune(tree: DocumentNode, key: ContentKey, /) -> DocumentNode:
    kids = _spine(tree)
    if any(child.meta.key == key for child in kids):
        return _with_spine(tree, tuple(child for child in kids if child.meta.key != key))
    return _with_spine(tree, tuple(_prune(child, key) for child in kids)) if kids else tree


def _retarget(tree: DocumentNode, key: ContentKey, fn: Callable[[DocumentNode], DocumentNode], /) -> DocumentNode:
    if tree.meta.key == key:
        return fn(tree)
    kids = _spine(tree)
    return _with_spine(tree, tuple(_retarget(child, key, fn) for child in kids)) if kids else tree


def _find(tree: DocumentNode, key: ContentKey, /) -> DocumentNode:
    return next(node for node in walk(tree) if node.meta.key == key)
```

## [04]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)

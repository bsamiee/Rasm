# [PY_ARTIFACTS_MODEL]

The semantic document algebra: the single interior representation the `document` axis lowers FROM and recovers TO. `DocumentNode` is ONE recursive `msgspec` tagged-union tree (page/section/block/run/list/table/figure/field/annotation/structure) carrying a closed `NodeMeta` tag on every node, and `DocumentDelta` is ONE diff/merge algebra (inserted/deleted/moved/reparametrized edits) keyed by the stable content key and defined once over the tree as one `expression` immutable fold. Every `folder:document/emit#DOCUMENT` backend becomes a lowering arm folding FROM this tree rather than dispatching an opaque payload, and `folder:document/lens#LENS` is the recover-TO inverse that rebuilds it — production and extraction are inverses over one node algebra, the extracted-tree corpus keys into the runtime columnar lane as a queryable value, and the `DocumentDelta` a structural object-graph diff reuses is defined here once. The tree round-trips through `msgspec.msgpack` (the same canonical codec `folder:../../../runtime/evidence/identity#CONTENT_IDENTITY` keys a corpus on) so a multi-PDF corpus is one content-keyed serialized value; identity comes from `ContentIdentity.of`, never re-minted. The lifecycle is fixed: a `DocumentNode` is the `[03]-[CANONICAL_OWNERS]` durable interior shape, the `CorpusRow`/`CorpusRecord` egress projections are its `[06]-[PROJECTIONS_AND_PORTS]` outward derivations, and no boundary codec attribute lives on the interior.

## [01]-[INDEX]

- [01]-[NODE]: `DocumentNode` — the recursive ten-variant `msgspec` tagged-union tree + the `NodeMeta` closed tag every node carries (content key, semantic role, page, optional `bounds`/`lang`/`actual_text` accessibility evidence) + the `StructRole` closed PDF/UA structure-type family (`StandardRole(StructEltKind)` over the standard vocabulary with the `_STRUCT_CATEGORY` `frozendict` behavior table carrying `StructCategory`/`heading_level`, `ForeignRole(role)` the one open arm) + the `CorpusRow`/`CorpusRecord` columnar projections carrying the `AltStatus` alt-presence column; the content-keyed `children`/`walk`/`node_digest`/`role_of`/`role_category`/`standard_for`/`alt_of`/`to_corpus`/`to_typst_source`/`encode`/`decode` tree algebra over one polymorphic projection entrypoint.
- [02]-[DELTA]: `DocumentDelta` — the four-variant edit algebra (inserted/deleted/moved/reparametrized) keyed by the stable `NodeMeta.key`; `diff`/`merge`/`invert` defined once over the tree as one total `expression` `Map`/`Block` fold, never a `list.append` accumulator.

## [02]-[NODE]

- Owner: `DocumentNode` the one recursive interior tree — ten `msgspec.Struct` variants (`PageNode`/`SectionNode`/`BlockNode`/`RunNode`/`ListNode`/`TableNode`/`FigureNode`/`FieldNode`/`AnnotationNode`/`StructureNode`) under one `tag`-discriminated `Union` on `tag_field="kind"`, every variant carrying a `NodeMeta` value object (content key, semantic role, page index, optional `bounds`, optional `lang` BCP-47 tag, optional `actual_text` replacement string). The tree is the algebra emission lowers FROM and extraction recovers TO; a flat `class DocumentNode` with a `kind: str` field and an `if kind == "page"` cascade is the rejected non-total shape.
- Cases: `PageNode` (page-rooted child sequence + media box) · `SectionNode` (heading-level outline node + heading runs + child sequence) · `BlockNode` (paragraph/quote/code/caption block with a `BlockKind` row, a `1`-`6` heading `level`, and inline runs) · `RunNode` (styled text run: text, font key, size, `weight`, `italic`, a `TextDirection` base direction, a `RunScript` super/sub/normal baseline, a `TextDecoration` underline/strike/overline set, and `Rgb` color — the full character appearance the `folder:../typography/shape#SHAPE` shaping surface carries and `_styled` lowers, never a flag the lowering ignores) · `ListNode` (the list-structure node carrying a `ListKind` ordered/unordered/description row, an ordered-list `start` ordinal, and an `LI` item child sequence, the `L`/`LI`/`Lbl`/`LBody` PDF/UA grouping the lens recovers and the `#list`/`#enum(start:)`/`#terms` Typst markup lowers) · `TableNode` (row-major cell grid of child node sequences + merged-cell span map + `header_rows`/`footer_rows` counts designating the leading `THead` and trailing `TFoot` rows PDF/UA distinguishes, the lens `Table.header` recovers, and the Typst `table.header(repeat: true)`/`table.footer` lowering emits) · `FigureNode` (embedded-graphic node: content key of the placed asset + `MediaType` MIME + intrinsic `(width, height)` + caption runs + the `alt` text equivalent the `folder:document/tagged#ACCESS` AUDIT verifies) · `FieldNode` (interactive form field: name, `FieldKind` row, value, `FieldFlag` flag set, choice options) · `AnnotationNode` (markup/redaction/link annotation with an `AnnotKind` row + target rect + an `AnnotTarget` closed link family — external `Uri`, internal `Dest` page destination, or `NoTarget`). Each a frozen `Struct` variant, never a per-kind class hierarchy.
- Role: `StructRole` the one closed PDF/UA structure-type family — `StandardRole(StructEltKind)` over the `StructEltKind` `StrEnum` of the full ISO 14289 standard-structure vocabulary (the grouping `Document`/`Part`/`Art`/`Sect`/`Div`/`TOC`/`TOCI`/`Index`, the headings `H1`-`H6`, the block roles `P`/`BlockQuote`/`Note`/`BibEntry`/`Code`/`Caption`, the inline roles `Span`/`Quote`/`Link`/`Reference`/`Annot`, the list roles `L`/`LI`/`Lbl`/`LBody`, the table roles `Table`/`THead`/`TBody`/`TFoot`/`TR`/`TH`/`TD`, the illustration roles `Figure`/`Formula`/`Form`), and `ForeignRole(role)` the one open arm carrying a `Meta`-constrained non-empty custom role. The closed `StrEnum` row-set plus the single `ForeignRole` arm is the totality the AUDIT closes over: every standard role a `StructEltKind` key (never a sibling struct), every foreign role one `Meta`-validated arm. The vocabulary is not bare — `_STRUCT_CATEGORY` is the one frozen behavior table keyed by `StructEltKind` carrying each role's `StructCategory` (`GROUPING`/`HEADING`/`BLOCK`/`INLINE`/`LIST`/`TABLE`/`ILLUSTRATION`) and its `heading_level` (`1`-`6` on `H1`-`H6`, `0` elsewhere), the one primary correspondence the AUDIT's structural-nesting and heading-monotonicity checks fold over rather than re-enumerating the roles per check, so the category and level a role carries derive from one table row and never a parallel `match`. `_STANDARD_FOR` is the one secondary map genuinely DERIVED from `_STRUCT_CATEGORY` by first-wins category inversion — each category's first-declared row is its canonical standard role (the inline canonical is `Span`, a foreign role's neutral `_FOREIGN_CATEGORY` grouping default is `Sect`) — exposed through the `standard_for` projection so the `folder:document/tagged#ACCESS` `/RoleMap` foreign-to-standard lowering reads a derived row rather than the hand-kept parallel dict that page formerly owned.
- Entry: `DocumentNode` is a `type` alias over the ten-variant `Union`; construction is direct variant instantiation, decode is `_DOCUMENT_DECODER.decode` (a reusable `msgpack.Decoder` typed on the union, the tag round-tripping under `tag_field="kind"`), and re-encode is `_ENCODER.encode` (the one reusable deterministic `msgpack.Encoder` the node digest, the corpus byte projection, and the public `encode` all share rather than three identical instances). `node_digest` folds a node's identity over its content + children into one `ContentKey` so the tree is content-addressed; `walk` yields every node in document order for the lens fold and the corpus projection.
- Auto: `children` is one total `match` projecting each variant to its interior child sequence (leaves return `()`); `node_digest` keys a leaf over `ContentIdentity.of(node.meta.key.fmt, _ENCODER.encode(node))` and an interior node over `ContentIdentity.of(node.meta.key.fmt, (own-field digest, *child digests))` — `_own_bytes` folding the container's own non-child fields (a changed `header_rows`/`level`/`start`/`list_kind`) beside the child digests so an identical sub-tree keys identically and a re-parametrized container re-keys rather than colliding on its unchanged children; `walk` is a pre-order generator over `children`. `alt_of` derives the `(AltText, AltStatus)` pair in one `FigureNode` discrimination (`PRESENT` with the authored `alt` on a non-empty figure, `ABSENT` with `""` on an un-authored figure, `NA` with `""` on every non-figure node), so the accessibility audit queries alt presence as one `kind == FIGURE and alt_status == ABSENT` column predicate over the corpus rather than re-deriving emptiness per row. `to_corpus(node, view)` is the ONE polymorphic columnar projection discriminating on a `CorpusView` axis: `STRUCT` returns the typed `CorpusRow` `Struct` the runtime columnar lane ingests as a value — its `kind` admitted through `NodeKind(node.__struct_config__.tag)` (the variant tag `msgspec` already minted, never a parallel kind `match`), its `page` the `int` preserved, its `lang`/`actual_text` the accessibility columns the audit reads, and its `(alt, alt_status)` the `alt_of` pair; `BYTES` lowers that row through the shared `_ENCODER` so the queryable corpus is one content-keyed serialized value; `RECORD` lowers the same typed `CorpusRow` to the flat `dict[str, object]` (`msgspec.to_builtins`) the `data/tabular/columnar#SCAN` `Corpus` arm's `pa.Table.from_pylist` ingests at the `data ← python:artifacts/document [WIRE]` seam — the producer owns this one mapping projection because `from_pylist` rejects a `msgspec.Struct` directly, the field names and native scalar dtypes (`str`/`StrEnum`-value-`str`/`int`) fixing the columnar Arrow schema so producer and consumer agree on the column shape, the `CorpusRow` `Struct` staying the typed interior and the byte/flat-record forms its two egress projections behind the one `view`-keyed entrypoint, never a `to_corpus_row`/`encode_corpus_row`/`to_corpus_record` sibling triple. `role_of` is the one polymorphic role projection discriminating on input shape — a `StructRole` lowers to its `StructEltKind` value or `ForeignRole` string, a `StructureNode` lowers through its `role` field, and every non-structure `DocumentNode` lowers to `NodeMeta.role` — so the corpus `role` column, the AUDIT role read, and the standard-vs-foreign discriminant resolve through one entrypoint, never a `corpus_role`/`role_of` pair. `role_category` projects a `StructRole` to its `StructCategory`/`heading_level` pair through the `_STRUCT_CATEGORY` table, a `ForeignRole` resolving to the named `_FOREIGN_CATEGORY` open-default row, so the AUDIT's nesting and heading-level checks read one table lookup. `standard_for` projects a `StructRole` to the canonical `StructEltKind` the `folder:document/tagged#ACCESS` `/RoleMap` foreign-to-standard lowering writes — a `StandardRole` to its own `elt`, a `ForeignRole` to its category's first-declared standard role via `_STANDARD_FOR` — so the tagged owner consumes one model projection, never a parallel category dict. `to_typst_source` is the one Typst-markup lowering folding the tree to the source string the `document/emit#DOCUMENT` typst rows compile, escaping every interpolated `RunNode.text`/heading/caption through the markup-context `_typst(..., TypstScope.MARKUP)` and the `FigureNode` `asset_key`/`alt` and link `Uri` through the string-context `_typst(..., TypstScope.STRING)` so a run carrying `]`/`#`/`*` never breaks `caption: [..]`/`#strong[..]` markup, defined once here so the two Typst escaping contexts share one `str.maketrans` algebra rather than per-arm string templates; the `_image` emitter owns the inner `image(source, alt: ..)` per the `.api/typst.md` `[MARKUP_ELEMENT_SCOPE]` `none | str` `alt` law (an authored figure writes `alt: "<escaped>"`, an un-authored figure writes `alt: none` — the `AltStatus.ABSENT` fact the corpus distinguishes, never collapsed to a meaningless `alt: ""`), the enclosing `#figure(.., caption: [..])` reserves its own `alt` slot for custom-content figures; a styled `RunNode` lowers its full appearance through the `_styled` `pipe` fold (`#strong`/`#emph` for weight+italic, `#super`/`#sub` for baseline, the `_DECORATION_MARKUP`-rowed `#underline`/`#strike`/`#overline` decoration set, `#text(dir: rtl)[..]` for a `TextDirection.RTL` run, and `#text(rgb(..))[..]` for a non-black color), a `BlockKind.QUOTE` block lowers through `#quote(block: true)[..]` and a `BlockKind.CODE` block through string-escaped `#raw("..", block: true)` rather than the prior generic-paragraph collapse that ignored the kind, a decorative `BlockKind.ARTIFACT` block lowers through `pdf.artifact[..]` so it is excluded from the tagged structure tree, a list lowers through the `_LIST_MARKUP`-rowed `#list`/`#enum(start:)`/`#terms` builder (an ordered list carrying its `start` ordinal, a description list folding each item to one `terms.item([term], [body])` pair), and a `TableNode` lowers its `header_rows`/`footer_rows` through the Typst `table.header(repeat: true, ..)`/`table.footer(..)` row-band elements rather than the invalid `table.header.repeat` key the prior arm emitted; the future `document/emit#DOCUMENT TYPST_QUERY` pass reads the `image` selector back through `typst.query` to verify every figure carries the equivalent.
- Receipt: the recovered tree contributes the `folder:core/receipt#RECEIPT` introspection case (content key, node count, text length, image count, hit count) at the lens boundary; `model.md` owns the tree type and its digest, never the receipt fold — authoring stays at `document/emit`, recovery at `document/lens`.
- Packages: `msgspec` (`Struct(frozen=True, tag=..., tag_field=...)` variant tree, `Union` alias, one shared `msgpack.Encoder(order="deterministic")` + `msgpack.Decoder` typed round-trip over both the node tree and the corpus row, `to_builtins` the flat-record `RECORD` lowering of the typed `CorpusRow` to the `dict[str, object]` the `data/tabular/columnar#SCAN` `Corpus` arm's `from_pylist` ingests at the WIRE seam, `structs.replace` copy-with, `__struct_config__.tag` the `structs.StructConfig` runtime view of the variant tag the corpus `kind` column admits through `NodeKind(...)` with no kind `match`, `Meta` `Annotated`-constraint admission on the `ForeignRole.role`/`FigureNode.alt`/`NodeMeta.lang`/`CorpusRow.alt`/`MediaType` text fields, `UnsetType`/`UNSET` the wire-absent `NodeMeta.lang`/`actual_text` markers round-tripping under `omit_defaults`); `expression` (`pipe` the `_styled` run-markup composition on the node side; `Map.empty`/`add`/`try_find` the `diff`/`merge` structural index and `Block.of_seq`/`fold`/`choose`/`collect` the immutable edit traversal on the delta side); `functools.reduce` (the `_styled` `_DECORATION_MARKUP` decoration fold over the variable-arity `RunNode.decorations` tuple); `builtins.frozendict` (`_STRUCT_CATEGORY` the primary role behavior table, `_STANDARD_FOR` its first-wins-inverted secondary, `_TYPST_ESCAPE`/`_LIST_MARKUP`/`_DECORATION_MARKUP` the immutable markup-spelling tables); runtime (`content_identity.ContentIdentity`/`ContentKey` for the node digest and corpus key, consumed never re-minted). `TextDirection` carries the `bidi.get_base_level`/`get_display(base_dir=)` paragraph-direction vocabulary as interior data — the `folder:../typography/shape#SHAPE` shaper owns the reorder, never `model.md`.
- Growth: a new document concept is one `DocumentNode` variant (a frozen `Struct` carrying its payload + `NodeMeta`) plus one `children`/`to_typst_source` arm; the decoder, the diff fold, and every backend pick it up by the total `match`. A new structured value on an existing node is one field. A new standard PDF/UA role is one `StructEltKind` member plus one `_STRUCT_CATEGORY` row, never a sibling struct — the remaining East-Asian `Ruby`/`RB`/`RT`/`RP`/`Warichu`/`WT`/`WP` and the `NonStruct`/`Private` roles each land exactly that way; a foreign role rides the one `ForeignRole` arm and the `_STRUCT_CATEGORY` open default; a new structural category is one `StructCategory` member, one `_STRUCT_CATEGORY` re-keying, and the `_STANDARD_FOR` first-wins derivation absorbs it for free; a new run decoration is one `TextDecoration` member plus one `_DECORATION_MARKUP` row; a new run direction or baseline is one `TextDirection`/`RunScript` member plus one `_styled` `pipe` arm; a new link-target kind is one `AnnotTarget` case; a new list dialect one `ListKind` member plus one `_LIST_MARKUP` row; zero new surface.
- Boundary: the opaque `dict[str, object]` payload `document/emit` formerly dispatched over is the deleted form — every backend now lowers from this tree. No durable store, no PDF parser (extraction is `document/lens`'s pymupdf/pypdf/lxml surface), no UI, no second tree type per backend. The tree is the canonical interior representation; the wire projection into the columnar corpus is the typed `CorpusRow` `Struct` lowered to its byte and flat-record egress shapes behind `to_corpus(node, view)`, never a hand-built stringly-typed `dict[str, str]` that erases the `kind`/`page` column types and never a parallel serialized model. `StructureNode.tag_role: str` is the deleted stringly-typed role; the closed `StructEltKind` vocabulary plus the one `ForeignRole(str)` arm is the audited replacement, and a per-role struct hierarchy beside the `StrEnum` is the rejected re-fragmentation. `FigureNode.alt` is the alt-text-presence fact the AUDIT verifies, owned here as one `AltText`-constrained field projected to the `CorpusRow.alt_status` column the audit reads as one predicate; a second alt-text field on a non-figure node, a free-`str` alt escaping the `AltText` bound, and a per-context alt re-derivation outside the one `_image` emission are the rejected re-fragmentations. The deleted `RoleView`/`role_contract`/`_ROLE_CONTRACT` schema-and-inspect view machinery (`json.schema_components`/`inspect.multi_type_info`/`structs.fields` blobs no consumer read) is the prime decorative surface removed: the `folder:document/tagged#ACCESS` AUDIT reads `role_of`/`role_category`/`alt_of`/`children` and nothing else, so a phantom contract view asserting an audit target that does not exist is the rejected illusory density. The `rtl: bool` field stored and round-tripped yet never read by the lowering is the deleted illusory flag — `direction: TextDirection` is the replacement the `_styled` fold honors; the `table.header.repeat: bool` dict key (invalid Typst, the `table.header` element takes `repeat:` as a named argument and the header cells as positional children) is the deleted broken spelling, replaced by the `table.header(repeat: true, ..)`/`table.footer(..)` row-band elements; the `BlockKind.QUOTE`/`CODE` cases collapsed into the generic-paragraph arm — declared vocabulary the lowering silently ignored — are the deleted hollow cases, each now its own `#quote(block: true)`/`#raw` arm; the two parallel identical `msgspec.msgpack.Encoder(order="deterministic")` instances (`_DOCUMENT_ENCODER`/`_CORPUS_ENCODER`) are the deleted duplication, one shared `_ENCODER` serving node, digest, and corpus. The `diff`/`merge` `list.append`+`dict` procedural accumulator is the deleted flat form; the `expression` `Map`/`Block` immutable fold is the rail-shaped replacement.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
from collections.abc import Iterator
from enum import StrEnum
from functools import reduce
from typing import Annotated, Final, Literal, assert_never, overload

import msgspec
from builtins import frozendict
from expression import pipe
from msgspec import Meta, Struct, UnsetType, UNSET

from rasm.runtime.content_identity import ContentIdentity, ContentKey

# --- [TYPES] ----------------------------------------------------------------------------


class NodeKind(StrEnum):
    PAGE = "page"
    SECTION = "section"
    BLOCK = "block"
    RUN = "run"
    LIST = "list"
    TABLE = "table"
    FIGURE = "figure"
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
    AUTO = "auto"      # shaper resolves the base level from content (`bidi.get_base_level` → 0/1)
    LTR = "ltr"
    RTL = "rtl"        # the `bidi.get_display(base_dir="R")` paragraph the shaper reorders


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
    DOCUMENT = "Document"          # grouping
    PART = "Part"
    ART = "Art"
    SECT = "Sect"
    DIV = "Div"
    TOC = "TOC"
    TOCI = "TOCI"
    INDEX = "Index"
    H1 = "H1"                      # headings
    H2 = "H2"
    H3 = "H3"
    H4 = "H4"
    H5 = "H5"
    H6 = "H6"
    P = "P"                        # block-level
    BLOCKQUOTE = "BlockQuote"
    NOTE = "Note"
    BIBENTRY = "BibEntry"
    CODE = "Code"
    CAPTION = "Caption"
    SPAN = "Span"                  # inline-level
    QUOTE = "Quote"
    LINK = "Link"
    REFERENCE = "Reference"
    ANNOT = "Annot"
    L = "L"                        # list grouping
    LI = "LI"
    LBL = "Lbl"
    LBODY = "LBody"
    TABLE = "Table"               # table grouping
    THEAD = "THead"
    TBODY = "TBody"
    TFOOT = "TFoot"
    TR = "TR"
    TH = "TH"
    TD = "TD"
    FIGURE = "Figure"             # illustration
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
type Rgb = tuple[int, int, int]
type Rect = tuple[float, float, float, float]

# --- [MODELS] ---------------------------------------------------------------------------


class NodeMeta(Struct, frozen=True, omit_defaults=True):
    key: ContentKey
    role: str
    page: int
    bounds: Rect | None = None
    lang: LangTag | UnsetType = UNSET          # PDF/UA `/Lang` BCP-47 tag; absent under `omit_defaults`
    actual_text: str | UnsetType = UNSET       # PDF/UA `/ActualText` replacement for non-textual glyphs


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


class NoTarget(Struct, frozen=True, tag="none", tag_field="target"):
    pass


type AnnotTarget = Uri | Dest | NoTarget


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
    start: int = 1                             # `ORDERED` first ordinal -> Typst `#enum(start:)`
    items: tuple[DocumentNode, ...] = ()       # one `LI` sub-tree per item


class TableNode(Struct, frozen=True, tag=NodeKind.TABLE.value, tag_field="kind"):
    meta: NodeMeta
    rows: tuple[tuple[DocumentNode, ...], ...] = ()
    spans: tuple[tuple[int, int, int, int], ...] = ()  # (row, col, col_span, row_span) merged-cell quads
    header_rows: int = 0                        # leading `THead` rows -> Typst `table.header(repeat: true)` + `Table.header`
    footer_rows: int = 0                        # trailing `TFoot` rows -> Typst `table.footer`


class FigureNode(Struct, frozen=True, tag=NodeKind.FIGURE.value, tag_field="kind"):
    meta: NodeMeta
    asset_key: ContentKey
    alt: AltText = ""
    media_type: MediaType = "image/png"
    intrinsic: tuple[float, float] | None = None
    caption: tuple[RunNode, ...] = ()


class FieldNode(Struct, frozen=True, tag=NodeKind.FIELD.value, tag_field="kind"):
    meta: NodeMeta
    name: str
    field: FieldKind
    value: str | bool | None = None
    flags: tuple[FieldFlag, ...] = ()
    options: tuple[str, ...] = ()               # `CHOICE` candidate values


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
    PageNode | SectionNode | BlockNode | RunNode | ListNode
    | TableNode | FigureNode | FieldNode | AnnotationNode | StructureNode
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


# --- [CONSTANTS] ------------------------------------------------------------------------

_ENCODER: Final = msgspec.msgpack.Encoder(order="deterministic")  # one deterministic codec for node, digest, and corpus
_DOCUMENT_DECODER: Final = msgspec.msgpack.Decoder(DocumentNode)
_CHILD_FIELDS: Final[frozenset[str]] = frozenset({"children", "heading", "runs", "items", "caption", "rows"})  # every `children`-projected field
_TYPST_ESCAPE: Final[frozendict[TypstScope, dict[int, str]]] = frozendict({
    TypstScope.STRING: str.maketrans({"\\": "\\\\", '"': '\\"'}),
    TypstScope.MARKUP: str.maketrans({c: f"\\{c}" for c in "\\[]#*_@$<>`"}),
})
_LIST_MARKUP: Final[frozendict[ListKind, str]] = frozendict(
    {ListKind.UNORDERED: "list", ListKind.ORDERED: "enum", ListKind.DESCRIPTION: "terms"}
)
_DECORATION_MARKUP: Final[frozendict[TextDecoration, str]] = frozendict(
    {TextDecoration.UNDERLINE: "underline", TextDecoration.STRIKETHROUGH: "strike", TextDecoration.OVERLINE: "overline"}
)

# --- [TABLES] ---------------------------------------------------------------------------

# The ONE primary correspondence: role -> (category, heading_level). The FIRST row of each category
# is its canonical role, so `_STANDARD_FOR` derives by first-wins inversion rather than a parallel literal.
_STRUCT_CATEGORY: Final[frozendict[StructEltKind, tuple[StructCategory, int]]] = frozendict({
    StructEltKind.SECT: (StructCategory.GROUPING, 0),
    StructEltKind.DOCUMENT: (StructCategory.GROUPING, 0),
    StructEltKind.PART: (StructCategory.GROUPING, 0),
    StructEltKind.ART: (StructCategory.GROUPING, 0),
    StructEltKind.DIV: (StructCategory.GROUPING, 0),
    StructEltKind.TOC: (StructCategory.GROUPING, 0),
    StructEltKind.INDEX: (StructCategory.GROUPING, 0),
    StructEltKind.H1: (StructCategory.HEADING, 1),
    StructEltKind.H2: (StructCategory.HEADING, 2),
    StructEltKind.H3: (StructCategory.HEADING, 3),
    StructEltKind.H4: (StructCategory.HEADING, 4),
    StructEltKind.H5: (StructCategory.HEADING, 5),
    StructEltKind.H6: (StructCategory.HEADING, 6),
    StructEltKind.P: (StructCategory.BLOCK, 0),
    StructEltKind.TOCI: (StructCategory.BLOCK, 0),
    StructEltKind.BLOCKQUOTE: (StructCategory.BLOCK, 0),
    StructEltKind.BIBENTRY: (StructCategory.BLOCK, 0),
    StructEltKind.NOTE: (StructCategory.BLOCK, 0),
    StructEltKind.CODE: (StructCategory.BLOCK, 0),
    StructEltKind.CAPTION: (StructCategory.BLOCK, 0),
    StructEltKind.SPAN: (StructCategory.INLINE, 0),
    StructEltKind.LINK: (StructCategory.INLINE, 0),
    StructEltKind.QUOTE: (StructCategory.INLINE, 0),
    StructEltKind.REFERENCE: (StructCategory.INLINE, 0),
    StructEltKind.ANNOT: (StructCategory.INLINE, 0),
    StructEltKind.L: (StructCategory.LIST, 0),
    StructEltKind.LI: (StructCategory.LIST, 0),
    StructEltKind.LBL: (StructCategory.LIST, 0),
    StructEltKind.LBODY: (StructCategory.LIST, 0),
    StructEltKind.TABLE: (StructCategory.TABLE, 0),
    StructEltKind.THEAD: (StructCategory.TABLE, 0),
    StructEltKind.TBODY: (StructCategory.TABLE, 0),
    StructEltKind.TFOOT: (StructCategory.TABLE, 0),
    StructEltKind.TR: (StructCategory.TABLE, 0),
    StructEltKind.TH: (StructCategory.TABLE, 0),
    StructEltKind.TD: (StructCategory.TABLE, 0),
    StructEltKind.FIGURE: (StructCategory.ILLUSTRATION, 0),
    StructEltKind.FORMULA: (StructCategory.ILLUSTRATION, 0),
    StructEltKind.FORM: (StructCategory.ILLUSTRATION, 0),
})
_FOREIGN_CATEGORY: Final[tuple[StructCategory, int]] = (StructCategory.GROUPING, 0)  # an unknown role maps to a neutral grouping, never a figure carrying mandatory alt
# DERIVED secondary: category -> its canonical standard role (the `/RoleMap` target the tagged owner reads),
# first-wins inversion of `_STRUCT_CATEGORY` so the canonical is the first-declared role of each category.
_STANDARD_FOR: Final[frozendict[StructCategory, StructEltKind]] = frozendict(
    {category: elt for elt, (category, _level) in reversed(tuple(_STRUCT_CATEGORY.items()))}
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
        case TableNode(rows=rows):
            return tuple(cell for row in rows for cell in row)
        case FigureNode(caption=caption):
            return caption
        case RunNode() | FieldNode() | AnnotationNode():
            return ()
        case _ as unreachable:
            assert_never(unreachable)


def walk(node: DocumentNode) -> Iterator[DocumentNode]:
    yield node
    for child in children(node):
        yield from walk(child)


def _own_bytes(node: DocumentNode, /) -> bytes:
    return _ENCODER.encode(msgspec.structs.replace(node, **{name: () for name in node.__struct_fields__ if name in _CHILD_FIELDS}))


def node_digest(node: DocumentNode) -> ContentKey:
    kids = children(node)
    if not kids:
        return ContentIdentity.of(node.meta.key.fmt, _ENCODER.encode(node))
    own = ContentIdentity.of(node.meta.key.fmt, _own_bytes(node))  # the container's own non-child fields key beside the child digests
    return ContentIdentity.of(node.meta.key.fmt, (own, *(node_digest(child) for child in kids)))


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
        case FigureNode(alt=alt):
            return alt, (AltStatus.PRESENT if alt else AltStatus.ABSENT)
        case _:
            return "", AltStatus.NA


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


def to_typst_source(node: DocumentNode) -> str:
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
        case TableNode(rows=rows, header_rows=head_n, footer_rows=foot_n):
            segments = (
                f"table.header(repeat: true, {_cells(rows[:head_n])})" if head_n else "",
                _cells(rows[head_n: len(rows) - foot_n] if foot_n else rows[head_n:]),
                f"table.footer({_cells(rows[len(rows) - foot_n:])})" if foot_n else "",
            )
            return f"#table(columns: {len(rows[0]) if rows else 0}, {', '.join(part for part in segments if part)})\n"
        case FigureNode(asset_key=asset_key, alt=alt, caption=caption):
            return f"#figure({_image(asset_key, alt)}, caption: [{_runs(caption)}])\n"
        case AnnotationNode(annot=AnnotKind.LINK, contents=text, link=Uri(href=href)):
            return f'#link("{_typst(href, TypstScope.STRING)}")[{_typst(text, TypstScope.MARKUP)}]'
        case AnnotationNode(annot=AnnotKind.LINK, contents=text, link=Dest(page=page, point=point)):
            x, y = point if point else (0.0, 0.0)
            return f"#link((page: {page + 1}, x: {x}pt, y: {y}pt))[{_typst(text, TypstScope.MARKUP)}]"
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


def _cells(rows: tuple[tuple[DocumentNode, ...], ...]) -> str:
    return ", ".join(f"[{to_typst_source(cell)}]" for row in rows for cell in row)


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
```

## [03]-[DELTA]

- Owner: `DocumentDelta` the one diff/merge edit algebra — four `msgspec.Struct` variants (`Inserted`/`Deleted`/`Moved`/`Reparametrized`) under one `tag`-discriminated `Union`, every edit keyed by the stable `NodeMeta.key` of the node it acts on. `diff` and `merge` are defined once over the tree as one total `expression` `Map`/`Block` fold; the same algebra a structural object-graph diff reuses lives here, never re-minted per consumer.
- Cases: `Inserted` (a new node + the parent key + position) · `Deleted` (the removed node's key) · `Moved` (a node key + the new parent key + new position) · `Reparametrized` (a node key + the field-name→`Raw`-value map of changed own-content fields, the in-place edit a re-styled run or re-bounded figure produces). Each a frozen `Struct` variant keyed by `ContentKey`; the edit set is the patch a `produce → extract → re-produce` round-trip and a privacy-redaction pass both emit.
- Entry: `diff(before, after)` folds the two trees keyed by each node's stable `NodeMeta.key` into an ordered `tuple[DocumentDelta, ...]` — a key present only in `after` (whose parent already existed) is an `Inserted`, only in `before` (whose parent survives) a `Deleted`, present in both under a different parent/index a `Moved`, present in both with a changed own-content payload a `Reparametrized`; `merge(tree, deltas)` folds the patch back over the tree returning the patched `DocumentNode`; `invert(before, deltas)` maps each edit to its inverse so a redaction patch is reversible until burned in. Every arm a total `match`; the patch round-trips through `msgspec.msgpack` so a corpus diff is a content-keyed serialized value.
- Auto: `_index` builds one `Map[ContentKey, IndexEntry]` over the STRUCTURAL `_spine` (the `children`-field child sequence the containers own — `PageNode`/`SectionNode`/`BlockNode`/`StructureNode`/`ListNode`), keying each node by its stable `NodeMeta.key` and its parent by the parent's `NodeMeta.key` (NEVER by `node_digest`, whose Merkle fold re-keys every ancestor when a descendant changes and would spuriously `Moved` every sibling of an edit), so the diff keys by structural identity while `node_digest` keys content identity — two distinct keyings, never conflated. `diff` reads the two `Map`s through `try_find` and folds the key-set algebra into the edit `Block`: a node whose key is new and whose parent already existed is the topmost `Inserted` of its subtree (a node under an also-new parent is carried inside that subtree and emits nothing), the symmetric topmost survivor is the `Deleted`, a surviving key under a changed parent/index is `Moved`, and a surviving key whose OWN content (every field except the structural `children` — including a `BlockNode`'s `runs`, a `SectionNode`'s `heading`, a `FigureNode`'s `caption`, a `TableNode`'s `rows`) differs is `Reparametrized`; the casualty set is one `Block.choose` over the survivor `try_find`, never a `list.append` accumulator. `merge` reduces the edit `Block` over the tree through `Block.fold`, `Inserted`/`Deleted` re-splicing a parent's `children` spine, `Moved` re-parenting under the new key, `Reparametrized` overlaying the decoded own-field map through `msgspec.convert` — one immutable fold, no in-place mutation.
- Receipt: the delta count and the changed-node keys ride the lens introspection receipt facts; `DocumentDelta` mints no receipt of its own.
- Packages: `msgspec` (`Struct(frozen=True, tag=True, tag_field=...)` edit variants, `Union` alias, `Raw` the opaque own-field values, `structs.replace` for the spine re-splice, `msgpack` round-trip, `convert` the `Reparametrized` re-coerce, `to_builtins` the overlay base); `expression` (`Map.empty`/`add`/`try_find` the structural index, `Block.of_seq`/`fold`/`choose` the immutable edit traversal, `Option` the lookup rail); runtime (`content_identity.ContentKey` keying every edit, consumed never re-minted).
- Growth: a new edit kind is one `DocumentDelta` variant plus one `diff` emit arm and one `merge` apply arm; the totality `match` forces both. A new diff granularity is a `node_digest` policy change, never a parallel delta family.
- Boundary: a per-consumer diff type (a document diff beside a geometry diff beside a wire diff) is the deleted form — `DocumentDelta` is the one edit algebra keyed by `ContentKey`. No mutation, no positional list patching by index-shift heuristics outside the key algebra, no second merge owner, no `list.append`/`dict` procedural accumulator where the `expression` `Map`/`Block` fold states the traversal. Structural insertion/deletion/move targets the spine containers that own a `children` field through `_spine`/`_with_spine`; a `TableNode` cell grid, a `FigureNode` caption, a `SectionNode` heading, a `ListNode` item bag, and a `BlockNode` inline-run bag are bounded OWN-content sub-payloads re-keyed as a whole through `Reparametrized`, so `_spine` carries only the container `children` field and the sub-payload edits ride the own-field overlay. The fold is total over the four-variant union; a missing arm is an `assert_never` static failure.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
from collections.abc import Callable
from typing import Final, assert_never

import msgspec
from expression import Nothing, Some
from expression.collections import Block, Map
from msgspec import Struct

from rasm.runtime.content_identity import ContentKey

from .model import BlockNode, DocumentNode, ListNode, PageNode, SectionNode, StructureNode, walk

# --- [TYPES] ----------------------------------------------------------------------------

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


def _index(root: DocumentNode, /) -> Map[ContentKey, IndexEntry]:
    def walk_spine(table: Map[ContentKey, IndexEntry], node: DocumentNode, parent: ContentKey | None, position: int) -> Map[ContentKey, IndexEntry]:
        seeded = table.add(node.meta.key, (node, parent, position))
        return Block.of_seq(enumerate(_spine(node))).fold(
            lambda acc, pair: walk_spine(acc, pair[1], node.meta.key, pair[0]), seeded
        )

    return walk_spine(Map.empty(), root, None, 0)


def diff(before: DocumentNode, after: DocumentNode, /) -> tuple[DocumentDelta, ...]:
    old, new = _index(before), _index(after)

    def survived(entry: tuple[ContentKey, IndexEntry], prior: IndexEntry) -> Block[DocumentDelta]:
        key, (node, parent, index) = entry
        node_prior, parent_prior, index_prior = prior
        moved = (Moved(key=key, parent=parent, index=index),) if (parent_prior, index_prior) != (parent, index) and parent is not None else ()
        changed = (Reparametrized(key=key, fields=_field_delta(node_prior, node)),) if _own(node_prior) != _own(node) else ()
        return Block.of_seq((*moved, *changed))

    def edits_for(entry: tuple[ContentKey, IndexEntry]) -> Block[DocumentDelta]:
        key, (node, parent, index) = entry
        if parent is None:
            return Block.empty()
        return old.try_find(key).map(lambda prior: survived(entry, prior)).default_with(
            lambda: Block.singleton(Inserted(parent=parent, index=index, node=node)) if old.try_find(parent).is_some() else Block.empty()
        )

    inserts_moves = Block.of_seq(new.items()).collect(edits_for)
    deletes = Block.of_seq(old.items()).choose(
        lambda entry: Some(Deleted(key=entry[0])) if entry[1][1] is not None and new.try_find(entry[0]).is_none() and new.try_find(entry[1][1]).is_some() else Nothing
    )
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
    old = _index(before)
    return tuple(Block.of_seq(deltas).map(lambda delta: _invert(delta, old)))[::-1]


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

- [OCR_DEFERRED]: a scanned page with no embedded text recovers no `RunNode` leaf — `document/lens` reads zero glyphs, so the tree has empty text runs. OCR (ocrmypdf driving Tesseract, a native binary) is the one path that would synthesize `RunNode` text from a rasterized page. It carries a native binary outside the cp315-core wheel set and the `python_version<'3.15'` band, so it is a deferred-admission concern, never a phantom fence member: the tree models the recovered structure faithfully (empty runs on a scanned page), and OCR enrichment lands as a future `document/lens` arm over an admitted OCR owner. `model.md` owns no OCR surface; the tree type is complete without it.
- [DIGEST_VS_IDENTITY]: `node_digest` is the Merkle CONTENT fold — a leaf keys over its serialized bytes and an interior node folds its child digests through `ContentIdentity.of(tuple_of_child_keys)`, so any descendant edit re-keys every ancestor digest. That re-keying is exactly why the diff does NOT key by `node_digest`: an unstable parent reference would spuriously `Moved` every sibling of an inserted node and break `merge`. The diff keys instead by the STABLE `NodeMeta.key` minted once per node at authoring/recovery time, so an edit at one node never perturbs its ancestors' diff keys. `node_digest` serves the cache-hit-by-reference and corpus-residency identity (a content-identical sub-tree keys identically for reuse elision); the diff/merge/move detection serves structural identity through `NodeMeta.key`. Two distinct keyings, never conflated, and both reach `ContentIdentity.of` — the `merkle` arm over child `ContentKey`s for the digest, the `whole` arm over `msgpack` bytes for the corpus.
- [STRUCT_ROLE_TOTALITY]: the PDF/UA structure-type vocabulary is closed except at one extension point, so `StructRole` is a two-arm tagged `Union` under `tag_field="role_kind"` — `StandardRole` wrapping the `StructEltKind` `StrEnum` of the full ISO 14289 standard-structure roles (the grouping `Art`/`Div`/`TOC`/`TOCI`/`Index` beside `Document`/`Part`/`Sect`, the block `BlockQuote`/`BibEntry` beside `P`/`Note`/`Code`/`Caption`, the inline `Span`/`Reference`/`Annot` beside `Quote`/`Link`, the `Lbl`/`LBody` list-item structure beside `L`/`LI`, the `THead`/`TBody`/`TFoot` table sections beside `TR`/`TH`/`TD`, and the `Formula`/`Form` illustration roles beside `Figure` — the seventeen-role gap the prior bounded slice left open against the spec's standard-structure-type table now closed), and `ForeignRole` carrying the one `ForeignRoleStr` escape. A standard role is a `StructEltKind` member (a `StrEnum` row, never a sibling struct), so a new standard role is one enum line plus one `_STRUCT_CATEGORY` row and `role_of`'s `StandardRole` arm plus `role_category`'s table lookup absorb it; a foreign role is the single `ForeignRole` arm `match` reaches, categorized as the named `_FOREIGN_CATEGORY` open-default row. The `StructEltKind` value strings are the literal PDF/UA standard-structure-type names, the one literal source on the page that traces to the external standard vocabulary; `ForeignRoleStr` constrains the foreign role to a non-empty `[A-Za-z][\w.\-]*` identifier through `Meta(min_length, max_length, pattern)`, validated on decode and on the `Reparametrized` re-coerce through `msgspec.convert`. The PDF/UA vocabulary is not bare: `_STRUCT_CATEGORY` is the one frozen `frozendict` behavior table carrying each role's `(StructCategory, heading_level)`, the smart-enum behavior-row collapse the bare `StrEnum` alone defeats, and `_STANDARD_FOR` is the one DERIVED secondary inverting category to its canonical standard role (the inline canonical `Span`, the foreign-default grouping `Sect`), exposed through the `standard_for` projection so the `folder:document/tagged#ACCESS` `/RoleMap` lowering reads a model projection rather than the hand-kept `_STANDARD_FOR` dict that page formerly owned — the `DERIVED_LOGIC` law applied across the model/tagged seam, one primary correspondence declared and the foreign-mapping secondary derived from it. The deleted `RoleView`/`role_contract`/`_ROLE_CONTRACT` machinery (the `json.schema_components((StructRole,), ...)` `(components, defs)` blob, the `inspect.multi_type_info((StructRole,))` node tree, the `structs.fields(FigureNode)` `FieldInfo` set) asserted three "audit views" no consumer reads — the `folder:document/tagged#ACCESS` AUDIT folds `role_of`/`role_category`/`alt_of`/`children` over the authored `pikepdf` tree and never reaches a `msgspec`-introspection blob, so the three views were decorative density (a confident-looking `RoleView`-keyed dispatch carrying no real capability) and are removed; the closed-family totality is proved by the `assert_never` over `StructRole`, not by a schema-component map nothing consumes. The nested `role_kind` discriminant is independent of the outer `kind` discriminant: `StructureNode` decodes its `kind="structure"` tag first, then `role` decodes its own `role_kind` tag, two tag fields on two `Union` levels that never collide because they name distinct fields. The `kind` tag the corpus column reads is recovered from `node.__struct_config__.tag` (the `msgspec.structs.StructConfig` runtime view), never a parallel `match` re-enumerating the ten variants to recover a literal the tag already carries.
- [ACCESSIBILITY_DOMAIN]: the node algebra carries the full PDF/UA + reflow accessibility surface the bare id/role/page slice omitted, each addition citing a real consumer or package member. `NodeMeta.lang` is the `/Lang` BCP-47 tag the PDF/UA structure tree and the `document/report#REPORT` language seam require (the sibling `report.md`/`lens.md` `language` references), typed `LangTag | UnsetType = UNSET` so an untagged node round-trips absent under `omit_defaults` and a tagged node carries the validated tag; `NodeMeta.actual_text` is the `/ActualText` glyph-replacement the PDF/UA spec mandates for ligatures and non-textual glyphs, the same `UnsetType` tri-state. `RunNode` carries `italic`/`script`/`decorations`/`direction`/`color` so a styled run round-trips its full character appearance — the `folder:../typography/shape#SHAPE` shaping surface authors them and the `_styled` `pipe` fold reads every one through `#emph`/`#super`/`#sub`/the `_DECORATION_MARKUP`-rowed `#underline`/`#strike`/`#overline`/`#text(dir: rtl)`/`#text(rgb(..))` markup. The prior `rtl: bool` was the illusory case: stored and round-tripped yet never read by `_styled`, so a right-to-left run emitted no direction markup at all; `direction: TextDirection` (`LTR`/`RTL`/`AUTO`, the python-bidi `base_dir` `'L'`/`'R'`/auto vocabulary the `get_base_level` probe resolves to `0`/`1`) replaces the dead flag with a tri-state the lowering honors and the shaper reorders, and `decorations: tuple[TextDecoration, ...]` adds the underline/strike/overline set the bare weight/italic slice dropped — a run that was bold-italic-superscript-underlined-red lowered to plain bold before. `ListNode` is the distinct `L`/`LI`/`Lbl`/`LBody` PDF/UA list grouping the lens recovers and the `#list`/`#enum(start:)`/`#terms` `_LIST_MARKUP`-rowed Typst markup lowers — the ordered-list `start` ordinal riding `#enum(start:)`, the description list folding each item to one `terms.item([term], [body])` pair — where the prior tree forced a list through a `BlockKind.LIST_ITEM` block that carried no ordered/unordered/description distinction, no start ordinal, and no list-level grouping, a real structural concept the audit's `L → LI` nesting check needs. `TableNode.header_rows`/`footer_rows` are the leading-`THead`-row and trailing-`TFoot`-row counts the pymupdf `Table.header` member recovers (verified in `.api/pymupdf.md`) and PDF/UA distinguishes from `TD` data cells; `to_typst_source` lowers them through the Typst `table.header(repeat: true, ..)`/`table.footer(..)` row-band elements, replacing the invalid `table.header.repeat` dict key the prior arm emitted. `BlockKind.QUOTE`/`CODE` now lower to `#quote(block: true)[..]`/`#raw("..", block: true)` rather than the prior generic-paragraph collapse that declared the kinds yet honored only `HEADING`/`ARTIFACT`. `FigureNode.media_type`/`intrinsic` carry the `MediaType` MIME and intrinsic `(width, height)` the `pikepdf`/`pymupdf` image extraction surfaces (`PdfImage.mode`/`colorspace`, `Pixmap` dimensions) recover and a faithful re-emission needs. `FieldNode.flags`/`options` carry the `FieldFlag` set (required/readonly/multiline/password) and `CHOICE` candidate values the `pymupdf Page.widgets()` recovery (`field_type`/`field_value` plus flags) and the `document/emit#DOCUMENT` form authoring read. `AnnotationNode.link` is the `AnnotTarget` closed family — external `Uri`, internal `Dest` page destination, `NoTarget` — the `LINK`-kind annotation needs so a hyperlink round-trips its href or page jump rather than dropping the target (the sibling `uri` references in `emit.md`/`egress.md`/`report.md`/`lens.md`); `to_typst_source` lowers the `Uri` case through `#link("href")[text]` and the internal `Dest` case through `#link((page:, x:, y:))[text]` (the Typst location-dictionary destination carrying the `Dest.point`), so a recovered page-jump round-trips its target rather than dropping it. `BlockKind.ARTIFACT` is the decorative-content case lowering through Typst `pdf.artifact[..]` (verified `.api/typst.md` `[MARKUP_ELEMENT_SCOPE]` `pdf.artifact`), so a rule/ornament is excluded from the tagged structure tree per the PDF/UA artifact rule rather than mis-tagged as content. Every addition is one field/case on an existing owner reshaping it as if it had always carried the concept, never a parallel surface.
- [ALT_TEXT_PRESENCE]: `FigureNode.alt: AltText` is the alt-text-presence fact the `document/tagged#ACCESS` AUDIT verifies and the `composition/compose#COMPOSE` unit authors, `AltText` an `Annotated[str, Meta(max_length=2048)]` admitting the empty string as the default (an un-authored figure carries `alt=""`, the audit's failing case) while bounding the upper length on decode. `to_corpus(node, STRUCT)` projects the `CorpusRow.alt`/`alt_status` columns from the one `alt_of` `(AltText, AltStatus)` pair so the `FigureNode` discriminant fires once per row, and the audit reads alt-text presence as the single `kind == FIGURE and alt_status == ABSENT` column predicate over the content-keyed corpus rather than re-walking the tree; `to_typst_source` folds the `FigureNode` through the `_image` emitter that owns the `alt: none | str` decision — a non-empty `alt` writes the string-context-escaped `image(source, alt: "<escaped>")`, an empty `alt` writes `image(source, alt: none)`, so the absent case stays `none` rather than collapsing to an empty-string equivalent that asserts a meaningless image, the Typst compiler emits the equivalent into the marked-content figure structure element of the PDF/UA render, and the future `document/emit#DOCUMENT TYPST_QUERY` pass reads the compiled `image` selector back through `typst.query`. The `_typst(value, scope)` escaper is one `TypstScope`-keyed `frozendict` `str.maketrans` algebra: the `STRING` context escapes `\` and `"` for the `alt`/`asset_key`/link-`href` quoted-string arguments, and the `MARKUP` context escapes the Typst content-mode active set `\[]#*_@$<>` and backtick for every interpolated `RunNode.text`, heading, caption, and link-text run, so an `alt` carrying a quote or a run carrying `]`/`#` produces valid markup. The field rides the `Reparametrized` own-field overlay with every other `FigureNode` own field, so a re-authored alt re-keys the figure's `node_digest` and emits one `Reparametrized` edit; no parallel alt-tracking surface exists.
- [MSGSPEC_RECURSIVE_UNION]: the ten `DocumentNode` variants form a recursive `Union` via the string forward references PEP 563 is NOT required for — `msgspec.msgpack.Decoder(DocumentNode)` resolves the forward reference on the `children`/`heading`/`runs`/`items`/`caption`/`rows` fields at decoder construction and discriminates on the `tag_field="kind"` tag (the `NodeKind` value), so the tree round-trips without a custom `dec_hook`, the decoded struct exposing the `kind` only as the encoded field, never a runtime `.kind`/`.tag` attribute, and `from __future__ import annotations` is never written (the active-surface deferred-annotation default and the `msgspec` decoder's own forward-reference resolution carry the recursion; the prior page's `__future__` citation was a phantom justification for a forbidden idiom). The `DocumentDelta` patch decodes as `tuple[DocumentDelta, ...]` under `tag_field="edit"`. The `Reparametrized` field map carries `msgspec.Raw` opaque values over the OWN fields only (every field except the structural `children`/`items`), and `_apply_fields` overlays them onto `msgspec.to_builtins(node)` then re-coerces through `msgspec.convert(merged, type(node))` so the changed own-fields re-validate against the concrete leaf variant's field types in one pass — no per-field annotation lookup (deferred annotations are strings, so a field-type decode would mis-resolve), no eager whole-tree re-validation, and a node's structural children are untouched by an own-content overlay. The `_index`/`diff`/`merge`/`invert` traversals fold through `expression` `Map.add`/`try_find` and `Block.fold`/`choose`/`collect` rather than a `dict[ContentKey, ...]` built by mutation and a `list[DocumentDelta]` grown by `append` — the immutable-traversal law applied to the structural index so the diff is one rail-shaped fold, the casualty `Block.choose` over the survivor `try_find` replacing the prior `edits.extend(...)` generator-into-list pattern.

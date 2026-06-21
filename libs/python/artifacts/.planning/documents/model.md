# [PY_ARTIFACTS_MODEL]

The semantic document algebra: the single interior representation the `documents` axis lowers FROM and recovers TO. `DocumentNode` is ONE recursive `msgspec` tagged-union tree (page/section/block/run/table/figure/field/annotation/structure_element) carrying a closed `NodeMeta` tag on every node, and `DocumentDelta` is ONE diff/merge algebra (inserted/deleted/moved/reparametrized edits) keyed by the runtime content key and defined once over the tree. Every `folder:documents/emit#DOCUMENT` backend becomes a lowering arm folding from this tree rather than dispatching an opaque payload, and `folder:documents/lens#LENS` is the recover-TO inverse that rebuilds it — so production and extraction are inverses over one node algebra, the extracted-tree corpus keys into the runtime columnar lane as a queryable value, and the `DocumentDelta` a structural diff reuses is defined here once. The tree round-trips through `msgspec.json` so a multi-PDF corpus is one content-keyed serialized value; identity comes from `folder:../../../runtime/evidence/identity#CONTENT_IDENTITY` `ContentIdentity.of`, never re-minted.

## [01]-[INDEX]

- [01]-[NODE]: `DocumentNode` — the recursive nine-variant `msgspec` tagged-union tree + the `NodeMeta` closed tag every node carries + the `StructRole` closed PDF/UA structure-type family (`StandardRole(StructEltKind)` over the standard vocabulary with its `_STRUCT_CATEGORY` `StructCategory`/`heading_level` behavior table, `ForeignRole(str)` the one open arm) + the `CorpusRow` typed columnar projection carrying the `AltStatus` alt-presence column; the content-keyed `children`/`walk`/`node_digest`/`role_of`/`role_category`/`role_contract`/`alt_of`/`to_corpus_row`/`encode_corpus_row`/`to_corpus_record`/`to_typst_source`/`encode`/`decode` tree algebra.
- [02]-[DELTA]: `DocumentDelta` — the four-variant edit algebra (inserted/deleted/moved/reparametrized) keyed by `ContentKey`; `diff`/`merge` defined once over the tree as one total fold.

## [02]-[NODE]

- Owner: `DocumentNode` the one recursive interior tree — nine `msgspec.Struct` variants (`PageNode`/`SectionNode`/`BlockNode`/`RunNode`/`TableNode`/`FigureNode`/`FieldNode`/`AnnotationNode`/`StructureNode`) under one `tag`-discriminated `Union`, every variant carrying a `NodeMeta` value object (the closed-family tag: content key, semantic role, page index, optional bounds). The tree is the algebra emission lowers FROM and extraction recovers TO.
- Cases: `PageNode` (page-rooted child sequence + media box) · `SectionNode` (heading-level outline node + child sequence) · `BlockNode` (paragraph/list/quote/heading block with a `BlockKind` row, a `1`-`6` heading `level`, and inline runs) · `RunNode` (styled text run: text, font key, size, weight, RTL flag — the leaf shaped by `folder:../typography/conformance#CONFORM` SHAPE) · `TableNode` (row-major cell grid of child node sequences + span map) · `FigureNode` (embedded-graphic node: content key of the placed asset + caption runs + the `alt` text equivalent the `folder:../accessibility/tagged#ACCESS` AUDIT verifies, the unit `folder:../figures/compose#COMPOSE` produces) · `FieldNode` (interactive form field: name, `FieldKind` row, value) · `AnnotationNode` (markup/redaction/link annotation with an `AnnotKind` row + target rect) · `StructureNode` (PDF/UA structure-element node: the closed `StructRole` tagged-PDF role + child sequence carrying the accessibility tree). Each a frozen `Struct` variant, never a per-kind class hierarchy.
- Role: `StructRole` the one closed PDF/UA structure-type family — `StandardRole(StructEltKind)` over the `StructEltKind` `StrEnum` of the bounded standard-structure vocabulary (`Document`/`Part`/`Sect`/`H1`-`H6`/`P`/`L`/`LI`/`Table`/`TR`/`TD`/`Figure`/`Caption`/`Link`/`Note`), and `ForeignRole(role: str)` the one open extension arm carrying a `Meta`-constrained non-empty custom role string. `StructureNode.tag_role: str` is the deleted stringly-typed escape hatch; the closed `StrEnum` row-set plus the single `ForeignRole` arm is the totality the `folder:../accessibility/tagged#ACCESS` AUDIT closes over, every standard role a `StructEltKind` key (never a sibling struct) and every foreign role one `Meta`-validated arm. The vocabulary is not bare: `_STRUCT_CATEGORY` is the one frozen behavior table keyed by `StructEltKind` — the smart-enum row carrying each standard role's `StructCategory` (`GROUPING`/`HEADING`/`BLOCK`/`INLINE`/`LIST`/`TABLE`/`ILLUSTRATION`) and its `heading_level` (`1`-`6` on `H1`-`H6`, `0` elsewhere), the one primary correspondence the AUDIT's structural-nesting and heading-monotonicity checks fold over rather than re-enumerating the nineteen roles per check, so the category and level a role carries derive from one table row and never a parallel `match`.
- Entry: `DocumentNode` is a `type` alias over the nine-variant `Union`; construction is direct variant instantiation, decode is `_DOCUMENT_DECODER.decode` (a reusable `msgspec.json.Decoder` typed on the union, the tag round-tripping under `tag_field="kind"`), and re-encode is `_DOCUMENT_ENCODER.encode` (a reusable `msgspec.json.Encoder`). `node_digest` folds a node's identity over its content + children into one `ContentKey` so the tree is content-addressed; `walk` yields every node in document order for the lens fold and the corpus projection.
- Auto: `children` is one total `match` projecting each variant to its interior child sequence (leaves return `()`); `node_digest` keys a leaf over its serialized bytes and an interior node over the Merkle fold of its children's keys through `ContentIdentity.of(tuple_of_child_keys)`, so an identical sub-tree keys identically and a re-parametrized node re-keys; `walk` is a pre-order generator over `children`; `to_corpus_row` projects a node to the `CorpusRow` frozen `Struct` the runtime columnar lane ingests as a typed value — its `kind` the `NodeKind` admitted through `NodeKind(node.__struct_config__.tag)` (the variant tag `msgspec` already minted, never a parallel kind `match`), its `page` the `int` preserved (never stringified), and its `alt`/`alt_status` the `(AltText, AltStatus)` pair `alt_of` derives in one `FigureNode` discrimination (`PRESENT` with the authored `alt` on a `FigureNode` whose `alt` is non-empty, `ABSENT` with `""` on an un-authored figure, `NA` with `""` on every non-figure node), so the FigureNode check fires once per row rather than once for the `alt` value and again for the status, and the accessibility audit queries alt-text presence as one `kind == FIGURE and alt_status == ABSENT` column predicate over the corpus rather than re-deriving emptiness per row, `encode_corpus_row` lowers the row to bytes through the shared `_CORPUS_ENCODER` so the queryable corpus is one content-keyed serialized value, never a hand-built `dict[str, str]` that erases the column types, and `to_corpus_record` lowers the same typed `CorpusRow` to the flat `dict[str, object]` mapping (`msgspec.to_builtins`) the `data:tabular/columnar#SCAN` `Corpus` arm's `pa.Table.from_pylist` ingests at the `data ← python:artifacts/documents [WIRE]` seam — the producer owns this one mapping projection because `from_pylist` rejects a `msgspec.Struct` directly, the seven field names (`key`/`kind`/`role`/`page`/`text`/`alt`/`alt_status`) and native scalar dtypes (`str`/`StrEnum`-value-`str`/`int`) fixing the columnar Arrow schema (`key`/`kind`/`role`/`text`/`alt`/`alt_status` Arrow `string`, `page` Arrow `int64`) so the producer and the `from_pylist` consumer agree on the column shape, the `CorpusRow` Struct staying the typed interior and the byte/flat-record forms its two egress projections; `role_of` is the one polymorphic role projection discriminating on input shape — a `StructRole` lowers to its `StructEltKind` value or `ForeignRole` string, a `StructureNode` lowers through its `role` field, and every non-structure `DocumentNode` lowers to `NodeMeta.role` — so the corpus `role` column, the AUDIT role read, and the standard-vs-foreign discriminant resolve through one entrypoint, never a `corpus_role`/`role_of` pair where one calls the other; `role_category` projects a `StructRole` to its `StructCategory`/`heading_level` pair through the `_STRUCT_CATEGORY` table, a `ForeignRole` resolving to the named `_FOREIGN_CATEGORY` open-default row rather than a bare inline literal, so the AUDIT's nesting and heading-level checks read one table lookup and the open default traces to one constant; `role_contract` is the one polymorphic role-contract projection discriminating on a `RoleView` axis — `SCHEMA` returns the `json.schema_components` `(components, defs)` role-coverage contract, `INSPECT` the `inspect.multi_type_info` one-element node-tree tuple wrapping the `inspect.UnionType`, and `CONTRACT` the `structs.fields(FigureNode)` `FieldInfo` set the AUDIT reads to confirm the `alt` field's name, type, and default — so the three audit views resolve through one entrypoint keyed by the view value, never a `role_schema`/`role_inspect` sibling pair; `to_typst_source` is the one Typst-markup lowering folding the tree to the source string the `documents/emit#DOCUMENT` typst rows compile through the shared `_runs` markup-join and the `_heading` level-clamped (`1`-`6`) heading emitter both the `BlockKind.HEADING` block and the `SectionNode` outline arm fold through (so a block heading carries its own `H1`-`H6` level rather than capping at level one), escaping every interpolated `RunNode.text`, heading, and caption run through the markup-context `_typst(..., TypstScope.MARKUP)` and the `FigureNode` `asset_key`/`alt` through the string-context `_typst(..., TypstScope.STRING)` so the marked-content figure structure element carries the text equivalent and a run carrying `]`/`#`/`*` never breaks the `caption: [..]` or `#strong[..]` markup, defined once here so the two Typst escaping contexts share one `str.maketrans` algebra rather than per-arm string templates. The `_image` emitter owns the inner `image(source, alt: ..)` per the `.api/typst.md` `[MARKUP_ELEMENT_SCOPE]` law and the `none | str` `alt` type — a `FigureNode` whose `alt` is authored writes `alt: "<escaped>"`, an un-authored figure writes `alt: none` rather than an empty-string equivalent (the `AltStatus.ABSENT` fact the corpus already distinguishes, never collapsed to `alt: ""` that asserts a meaningless image), so the absent and authored cases stay distinct through to the PDF/UA render; the enclosing `#figure(.., caption: [..])` carries the caption and reserves its own `alt: none | str` slot for custom-content figures whose body is not an image, so an image figure writes one marked-content structure element with no doubled alt, and the future `documents/emit#DOCUMENT TYPST_QUERY` pass reads the `image` selector back through `typst.query` to verify every figure carries the equivalent.
- Receipt: the recovered tree contributes the `folder:../receipt/receipt#RECEIPT` introspection case (content key, node count, text length, image count, hit count) at the lens boundary; `model.md` owns the tree type and its digest, never the receipt fold — authoring stays at `documents/emit`, recovery at `documents/lens`.
- Packages: `msgspec` (`Struct(frozen=True, tag=..., tag_field=...)` variant tree, the `CorpusRow` `Struct` the typed corpus projection, `Union` alias, `json.Encoder`/`json.Decoder` typed round-trip over both the node tree and the corpus row, `convert` boundary coercion, `to_builtins` the flat-record `to_corpus_record` lowering of the typed `CorpusRow` to the `dict[str, object]` the `data:tabular/columnar#SCAN` `Corpus` arm's `from_pylist` ingests at the WIRE seam, `structs.replace` copy-with, `structs.fields(FigureNode)` returning the `structs.FieldInfo` set the `role_contract(RoleView.CONTRACT)` view projects so the AUDIT confirms the `alt` field's name, `AltText` annotation, and `""` default off the live struct rather than re-declaring it, `__struct_config__.tag` the `structs.StructConfig` runtime view of the variant tag the corpus `kind` column admits through `NodeKind(...)` with no kind `match`, `Meta` `Annotated`-constraint admission on the `ForeignRole.role`, `FigureNode.alt`, and `CorpusRow.alt` text fields, `json.schema_components((StructRole,), ref_template=...)` projecting the closed family plus its `#/components/{name}` `$defs` map to the JSON-schema role-coverage contract and `inspect.multi_type_info((StructRole,))` projecting the same union alias to the structured `inspect.UnionType`/`inspect.StructType`/`inspect.EnumType` node tree — the three views the accessibility AUDIT reads through the one `role_contract` dispatch, the schema map for a wire contract a multi-type spec embeds, the `inspect` node tree for the in-process totality walk over the `EnumType.cls` `StructEltKind` members that proves every standard role is covered, and the `FieldInfo` set for the `alt`-field contract; `inspect.Type` the node base the projection tuple is typed over); runtime (`content_identity.ContentIdentity`/`ContentKey` for the node digest, consumed never re-minted).
- Growth: a new document concept is one `DocumentNode` variant (a frozen `Struct` carrying its payload + `NodeMeta`) plus one `children`/`node_digest` arm; the decoder, the diff fold, and every backend pick it up by the total `match`. A new structured value on an existing node is one field. A new standard PDF/UA role is one `StructEltKind` member plus one `_STRUCT_CATEGORY` row, never a sibling struct; a foreign role rides the one `ForeignRole` arm and the `_STRUCT_CATEGORY` open default; a new structural category for the AUDIT is one `StructCategory` member and a re-keyed table column; zero new surface.
- Boundary: the opaque `dict[str, object]` payload `documents/emit` formerly dispatched over is the deleted form — every backend now lowers from this tree. No durable store, no PDF parser (extraction is `documents/lens`'s pymupdf/pypdf/lxml surface), no UI, no second tree type per backend. The tree is the canonical interior representation; the wire projection into the columnar corpus is the typed `CorpusRow` `Struct` (`to_corpus_row`) lowered to its byte (`encode_corpus_row`) and flat-record (`to_corpus_record`, the `msgspec.to_builtins` `dict[str, object]` the `data:tabular/columnar#SCAN` `Corpus` arm's `from_pylist` ingests because `from_pylist` rejects a `Struct` directly) egress shapes, never a hand-built stringly-typed `dict[str, str]` that erases the `kind`/`page` column types and never a parallel serialized model; the flat record's keys and native scalar dtypes are derived from the one `CorpusRow` field set, so the producer and the columnar `from_pylist` schema agree without the consumer re-deriving the column shape. A flat `class DocumentNode` with a stringly-typed `kind: str` field and an `if kind == "page"` cascade is the rejected shape; the closed tag and the total `match` are the totality proof. `StructureNode.tag_role: str` is the deleted stringly-typed role; the closed `StructEltKind` vocabulary plus the one `ForeignRole(str)` arm is the audited replacement, and a per-role struct hierarchy beside the `StrEnum` is the rejected re-fragmentation. `FigureNode.alt` is the alt-text-presence fact the accessibility AUDIT verifies, owned here as one `AltText`-constrained field projected to the `CorpusRow.alt_status` `AltStatus` column the audit reads as one predicate, never a phantom the audit checks against nothing and never a parallel alt-tracking surface; a second alt-text field on a non-figure node, a free-`str` alt escaping the `AltText` length bound, and a per-context alt re-derivation outside the one `to_typst_source` `image(.., alt: ..)` emission are the rejected re-fragmentations.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
from __future__ import annotations

from collections.abc import Iterator
from enum import StrEnum
from typing import Annotated, Final, assert_never

import msgspec
from msgspec import Meta, Struct, inspect as msgspec_inspect

from rasm.runtime.content_identity import ContentIdentity, ContentKey

# --- [TYPES] ----------------------------------------------------------------------------


class NodeKind(StrEnum):
    PAGE = "page"
    SECTION = "section"
    BLOCK = "block"
    RUN = "run"
    TABLE = "table"
    FIGURE = "figure"
    FIELD = "field"
    ANNOTATION = "annotation"
    STRUCTURE = "structure"


class BlockKind(StrEnum):
    PARAGRAPH = "paragraph"
    HEADING = "heading"
    LIST_ITEM = "list-item"
    QUOTE = "quote"
    CODE = "code"
    CAPTION = "caption"


class FieldKind(StrEnum):
    TEXT = "text"
    CHECKBOX = "checkbox"
    CHOICE = "choice"
    SIGNATURE = "signature"
    BUTTON = "button"


class AnnotKind(StrEnum):
    HIGHLIGHT = "highlight"
    REDACTION = "redaction"
    LINK = "link"
    NOTE = "note"
    STAMP = "stamp"


class StructEltKind(StrEnum):
    DOCUMENT = "Document"
    PART = "Part"
    SECT = "Sect"
    H1 = "H1"
    H2 = "H2"
    H3 = "H3"
    H4 = "H4"
    H5 = "H5"
    H6 = "H6"
    P = "P"
    L = "L"
    LI = "LI"
    TABLE = "Table"
    TR = "TR"
    TD = "TD"
    FIGURE = "Figure"
    CAPTION = "Caption"
    LINK = "Link"
    NOTE = "Note"


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


class RoleView(StrEnum):
    SCHEMA = "schema"
    INSPECT = "inspect"
    CONTRACT = "contract"


# --- [BOUNDARIES] -----------------------------------------------------------------------

type ForeignRoleStr = Annotated[str, Meta(min_length=1, max_length=64, pattern=r"\A[A-Za-z][\w.\-]*\Z")]
type AltText = Annotated[str, Meta(max_length=2048)]

# --- [MODELS] ---------------------------------------------------------------------------


class NodeMeta(Struct, frozen=True):
    key: ContentKey
    role: str
    page: int
    bounds: tuple[float, float, float, float] | None = None


class StandardRole(Struct, frozen=True, tag="standard", tag_field="role_kind"):
    elt: StructEltKind


class ForeignRole(Struct, frozen=True, tag="foreign", tag_field="role_kind"):
    role: ForeignRoleStr


type StructRole = StandardRole | ForeignRole


class PageNode(Struct, frozen=True, tag=NodeKind.PAGE.value, tag_field="kind"):
    meta: NodeMeta
    media_box: tuple[float, float, float, float]
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
    rtl: bool = False


class TableNode(Struct, frozen=True, tag=NodeKind.TABLE.value, tag_field="kind"):
    meta: NodeMeta
    rows: tuple[tuple[DocumentNode, ...], ...] = ()
    spans: tuple[tuple[int, int, int, int], ...] = ()


class FigureNode(Struct, frozen=True, tag=NodeKind.FIGURE.value, tag_field="kind"):
    meta: NodeMeta
    asset_key: ContentKey
    alt: AltText = ""
    caption: tuple[RunNode, ...] = ()


class FieldNode(Struct, frozen=True, tag=NodeKind.FIELD.value, tag_field="kind"):
    meta: NodeMeta
    name: str
    field: FieldKind
    value: str | bool | None = None


class AnnotationNode(Struct, frozen=True, tag=NodeKind.ANNOTATION.value, tag_field="kind"):
    meta: NodeMeta
    annot: AnnotKind
    target: tuple[float, float, float, float]
    contents: str = ""


class StructureNode(Struct, frozen=True, tag=NodeKind.STRUCTURE.value, tag_field="kind"):
    meta: NodeMeta
    role: StructRole
    children: tuple[DocumentNode, ...] = ()


type DocumentNode = PageNode | SectionNode | BlockNode | RunNode | TableNode | FigureNode | FieldNode | AnnotationNode | StructureNode


class CorpusRow(Struct, frozen=True):
    key: str
    kind: NodeKind
    role: str
    page: int
    text: str
    alt: AltText = ""
    alt_status: AltStatus = AltStatus.NA


# --- [CONSTANTS] ------------------------------------------------------------------------

_DOCUMENT_ENCODER: Final = msgspec.json.Encoder()
_DOCUMENT_DECODER: Final = msgspec.json.Decoder(DocumentNode)
_CORPUS_ENCODER: Final = msgspec.json.Encoder()
_TYPST_ESCAPE: Final[dict[TypstScope, dict[int, str]]] = {
    TypstScope.STRING: str.maketrans({"\\": "\\\\", '"': '\\"'}),
    TypstScope.MARKUP: str.maketrans({c: f"\\{c}" for c in "\\[]#*_@$<>`"}),
}

# --- [TABLES] ---------------------------------------------------------------------------

_STRUCT_CATEGORY: Final[dict[StructEltKind, tuple[StructCategory, int]]] = {
    StructEltKind.DOCUMENT: (StructCategory.GROUPING, 0),
    StructEltKind.PART: (StructCategory.GROUPING, 0),
    StructEltKind.SECT: (StructCategory.GROUPING, 0),
    StructEltKind.H1: (StructCategory.HEADING, 1),
    StructEltKind.H2: (StructCategory.HEADING, 2),
    StructEltKind.H3: (StructCategory.HEADING, 3),
    StructEltKind.H4: (StructCategory.HEADING, 4),
    StructEltKind.H5: (StructCategory.HEADING, 5),
    StructEltKind.H6: (StructCategory.HEADING, 6),
    StructEltKind.P: (StructCategory.BLOCK, 0),
    StructEltKind.L: (StructCategory.LIST, 0),
    StructEltKind.LI: (StructCategory.LIST, 0),
    StructEltKind.TABLE: (StructCategory.TABLE, 0),
    StructEltKind.TR: (StructCategory.TABLE, 0),
    StructEltKind.TD: (StructCategory.TABLE, 0),
    StructEltKind.FIGURE: (StructCategory.ILLUSTRATION, 0),
    StructEltKind.CAPTION: (StructCategory.BLOCK, 0),
    StructEltKind.LINK: (StructCategory.INLINE, 0),
    StructEltKind.NOTE: (StructCategory.BLOCK, 0),
}
_FOREIGN_CATEGORY: Final[tuple[StructCategory, int]] = (StructCategory.ILLUSTRATION, 0)

type RoleContract = (
    tuple[tuple[dict[str, object], ...], dict[str, dict[str, object]]]
    | tuple[msgspec_inspect.Type, ...]
    | tuple[msgspec.structs.FieldInfo, ...]
)

_ROLE_CONTRACT: Final[dict[RoleView, RoleContract]] = {
    RoleView.SCHEMA: msgspec.json.schema_components((StructRole,), ref_template="#/components/{name}"),
    RoleView.INSPECT: msgspec_inspect.multi_type_info((StructRole,)),
    RoleView.CONTRACT: msgspec.structs.fields(FigureNode),
}

# --- [OPERATIONS] -----------------------------------------------------------------------


def children(node: DocumentNode) -> tuple[DocumentNode, ...]:
    match node:
        case PageNode(children=kids) | StructureNode(children=kids):
            return kids
        case SectionNode(heading=head, children=kids):
            return (*head, *kids)
        case BlockNode(runs=runs, children=kids):
            return (*runs, *kids)
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


def node_digest(node: DocumentNode) -> ContentKey:
    kids = children(node)
    if not kids:
        return ContentIdentity.of(node.meta.key.fmt, _DOCUMENT_ENCODER.encode(node))
    return ContentIdentity.of(node.meta.key.fmt, tuple(node_digest(child) for child in kids))


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


def role_contract(view: RoleView) -> RoleContract:
    return _ROLE_CONTRACT[view]


def alt_of(node: DocumentNode) -> tuple[AltText, AltStatus]:
    match node:
        case FigureNode(alt=alt):
            return alt, (AltStatus.PRESENT if alt else AltStatus.ABSENT)
        case _:
            return "", AltStatus.NA


def to_corpus_row(node: DocumentNode) -> CorpusRow:
    alt, status = alt_of(node)
    return CorpusRow(
        key=node.meta.key.hex,
        kind=NodeKind(node.__struct_config__.tag),
        role=role_of(node),
        page=node.meta.page,
        text="".join(run.text for run in walk(node) if isinstance(run, RunNode)),
        alt=alt,
        alt_status=status,
    )


def encode_corpus_row(node: DocumentNode) -> bytes:
    return _CORPUS_ENCODER.encode(to_corpus_row(node))


def to_corpus_record(node: DocumentNode) -> dict[str, object]:
    # the flat-record WIRE projection the `data:tabular/columnar#SCAN` `Corpus` arm's
    # `pa.Table.from_pylist` consumes: one `msgspec.to_builtins` lowering of the typed `CorpusRow`
    # to a `dict[str, object]` whose keys are the seven `CorpusRow` field names and whose values are
    # the native scalars (`key`/`role`/`text`/`alt` str, `kind`/`alt_status` the StrEnum string value,
    # `page` int) — `from_pylist` rejects a `msgspec.Struct` directly (no `.keys()`), so the producer
    # owns this one mapping projection rather than the consumer re-deriving the column shape; the
    # `CorpusRow` Struct stays the typed interior, `encode_corpus_row` the serialized byte value, and
    # this the columnar-ingestible flat record, three egress shapes of the one `CorpusRow` owner.
    return msgspec.to_builtins(to_corpus_row(node))


def to_typst_source(node: DocumentNode) -> str:
    match node:
        case RunNode(text=text, weight=weight):
            return f"#strong[{_typst(text, TypstScope.MARKUP)}]" if weight >= 700 else _typst(text, TypstScope.MARKUP)
        case BlockNode(block=BlockKind.HEADING, level=level, runs=runs):
            return _heading(level, runs)
        case BlockNode(runs=runs, children=kids):
            return _runs(runs) + "".join(to_typst_source(child) for child in kids) + "\n"
        case SectionNode(level=level, heading=head, children=kids):
            return _heading(level, head) + "".join(to_typst_source(child) for child in kids)
        case PageNode(children=kids):
            return "".join(to_typst_source(child) for child in kids) + "#pagebreak()\n"
        case StructureNode(children=kids):
            return "".join(to_typst_source(child) for child in kids)
        case TableNode(rows=rows):
            cells = ", ".join("[" + to_typst_source(cell) + "]" for row in rows for cell in row)
            return f"#table(columns: {len(rows[0]) if rows else 0}, {cells})\n"
        case FigureNode(asset_key=asset_key, alt=alt, caption=caption):
            return f"#figure({_image(asset_key, alt)}, caption: [{_runs(caption)}])\n"
        case FieldNode() | AnnotationNode():
            return ""
        case _ as unreachable:
            assert_never(unreachable)


def decode(payload: bytes) -> DocumentNode:
    return _DOCUMENT_DECODER.decode(payload)


def encode(node: DocumentNode) -> bytes:
    return _DOCUMENT_ENCODER.encode(node)


def _typst(value: str, scope: TypstScope) -> str:
    return value.translate(_TYPST_ESCAPE[scope])


def _runs(runs: tuple[RunNode, ...]) -> str:
    return "".join(_typst(run.text, TypstScope.MARKUP) for run in runs)


def _heading(level: int, runs: tuple[RunNode, ...]) -> str:
    return f"{'=' * min(max(level, 1), 6)} {_runs(runs)}\n"


def _image(asset_key: ContentKey, alt: AltText) -> str:
    source = _typst(asset_key.hex, TypstScope.STRING)
    equiv = f'"{_typst(alt, TypstScope.STRING)}"' if alt else "none"
    return f'image("{source}", alt: {equiv})'
```

## [03]-[DELTA]

- Owner: `DocumentDelta` the one diff/merge edit algebra — four `msgspec.Struct` variants (`Inserted`/`Deleted`/`Moved`/`Reparametrized`) under one `tag`-discriminated `Union`, every edit keyed by the `ContentKey` of the node it acts on. `diff` and `merge` are defined once over the tree as one total fold; the same algebra a structural object-graph diff reuses lives here, never re-minted per consumer.
- Cases: `Inserted` (a new node + the parent key + position) · `Deleted` (the removed node's key) · `Moved` (a node key + the new parent key + new position) · `Reparametrized` (a node key + the field-name→serialized-value map of changed leaf fields, the in-place edit a re-styled run or re-bounded figure produces). Each a frozen `Struct` variant keyed by `ContentKey`; the edit set is the patch a `produce → extract → re-produce` round-trip and a privacy-redaction pass both emit.
- Entry: `diff(before, after)` folds the two trees keyed by each node's stable `NodeMeta.key` into an ordered `tuple[DocumentDelta, ...]` — a key present only in `after` (whose parent already existed) is an `Inserted`, only in `before` (whose parent survives) a `Deleted`, present in both under a different parent/index a `Moved`, present in both with a changed own-content payload a `Reparametrized`; `merge(tree, deltas)` folds the patch back over the tree returning the patched `DocumentNode`, every arm a total `match`. The patch round-trips through `msgspec.json` so a corpus diff is a content-keyed serialized value.
- Auto: `diff` builds the two `{ContentKey: (DocumentNode, parent_key, index)}` maps over the STRUCTURAL `spine` (the `children`-field child sequence the containers own — `PageNode`/`SectionNode`/`BlockNode`/`StructureNode`), keying each node by its stable `NodeMeta.key` and its parent by the parent's `NodeMeta.key` (NEVER by `node_digest`, whose Merkle fold re-keys every ancestor when a descendant changes and would spuriously `Moved` every sibling of an edit), then folds the key-set algebra into the edit tuple: a node whose key is new and whose parent already existed is the topmost `Inserted` of its subtree (a node under an also-new parent is carried inside that subtree and emits nothing), the symmetric topmost survivor is the `Deleted`, a surviving key under a changed parent/index is `Moved`, and a surviving key whose OWN content (every field except the structural `children` — including a `BlockNode`'s `runs`, a `SectionNode`'s `heading`, a `FigureNode`'s `caption`, a `TableNode`'s `rows`) differs is `Reparametrized`. `merge` reduces the edit tuple over the tree, `Inserted`/`Deleted` re-splicing a parent's `children` spine, `Moved` re-parenting under the new key, `Reparametrized` overlaying the decoded own-field map through `msgspec.convert` — one immutable fold, no in-place mutation. `invert` maps each edit to its inverse so a redaction patch is reversible until burned in.
- Receipt: the delta count and the changed-node keys ride the lens introspection receipt facts; `DocumentDelta` mints no receipt of its own.
- Packages: `msgspec` (`Struct(frozen=True, tag=True, tag_field=...)` edit variants, `Union` alias, `structs.replace` for the `Reparametrized` apply, `json` round-trip, `convert`); runtime (`content_identity.ContentKey` keying every edit, consumed never re-minted).
- Growth: a new edit kind is one `DocumentDelta` variant plus one `diff` emit arm and one `merge` apply arm; the totality `match` forces both. A new diff granularity is a `node_digest` policy change, never a parallel delta family.
- Boundary: a per-consumer diff type (a document diff beside a geometry diff beside a wire diff) is the deleted form — `DocumentDelta` is the one edit algebra keyed by `ContentKey`. No mutation, no positional list patching by index-shift heuristics outside the key algebra, no second merge owner. Structural insertion/deletion/move targets the spine containers that own a `children` field (`PageNode`/`SectionNode`/`BlockNode`/`StructureNode`) through `spine`/`_with_spine`; a `TableNode` cell grid, a `FigureNode` caption, a `SectionNode` heading, and a `BlockNode` inline-run bag are bounded OWN-content sub-payloads (NOT structural children) re-keyed as a whole through `Reparametrized`, so `spine` carries only the container `children` field and the sub-payload edits ride the own-field overlay. The structural spine and the node digest are distinct: `node_digest` ([2]-[NODE]) is the Merkle content fold the cache and corpus keying read, while the diff keys by the stable `NodeMeta.key` so an edit at one node never re-keys its ancestors. The fold is total over the four-variant union; a missing arm is an `assert_never` static failure.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
from __future__ import annotations

from collections.abc import Callable
from typing import Final, assert_never

import msgspec
from msgspec import Struct

from rasm.runtime.content_identity import ContentKey

from .model import BlockNode, DocumentNode, PageNode, SectionNode, StructureNode, walk

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

_DELTA_ENCODER: Final = msgspec.json.Encoder()
_DELTA_DECODER: Final = msgspec.json.Decoder(tuple[DocumentDelta, ...])

# --- [OPERATIONS] -----------------------------------------------------------------------


def _spine(node: DocumentNode) -> tuple[DocumentNode, ...]:
    match node:
        case PageNode(children=kids) | StructureNode(children=kids) | SectionNode(children=kids) | BlockNode(children=kids):
            return kids
        case _:
            return ()


def _with_spine(node: DocumentNode, kids: tuple[DocumentNode, ...]) -> DocumentNode:
    match node:
        case PageNode() | StructureNode() | SectionNode() | BlockNode():
            return msgspec.structs.replace(node, children=kids)
        case _:
            return node


def _own(node: DocumentNode) -> tuple[tuple[str, object], ...]:
    return tuple((name, getattr(node, name)) for name in node.__struct_fields__ if name != "children")


def _index(root: DocumentNode) -> dict[ContentKey, tuple[DocumentNode, ContentKey | None, int]]:
    table: dict[ContentKey, tuple[DocumentNode, ContentKey | None, int]] = {root.meta.key: (root, None, 0)}

    def visit(node: DocumentNode) -> None:
        for position, child in enumerate(_spine(node)):
            table[child.meta.key] = (child, node.meta.key, position)
            visit(child)

    visit(root)
    return table


def diff(before: DocumentNode, after: DocumentNode) -> tuple[DocumentDelta, ...]:
    old, new = _index(before), _index(after)
    edits: list[DocumentDelta] = []
    for key, (node, parent, index) in new.items():
        if parent is None:
            continue
        if key not in old:
            if parent in old:
                edits.append(Inserted(parent=parent, index=index, node=node))
            continue
        placed = old[key]
        if placed[1] != parent or placed[2] != index:
            edits.append(Moved(key=key, parent=parent, index=index))
        if _own(placed[0]) != _own(node):
            edits.append(Reparametrized(key=key, fields=_field_delta(placed[0], node)))
    edits.extend(Deleted(key=key) for key, (_, parent, _) in old.items() if parent is not None and key not in new and parent in new)
    return tuple(edits)


def merge(tree: DocumentNode, deltas: tuple[DocumentDelta, ...]) -> DocumentNode:
    patched = tree
    for delta in deltas:
        match delta:
            case Inserted(parent=parent, index=index, node=node):
                patched = _splice(patched, parent, index, node)
            case Deleted(key=key):
                patched = _prune(patched, key)
            case Moved(key=key, parent=parent, index=index):
                patched = _splice(_prune(patched, key), parent, index, _find(tree, key))
            case Reparametrized(key=key, fields=fields):
                patched = _retarget(patched, key, lambda node: _apply_fields(node, fields))
            case _ as unreachable:
                assert_never(unreachable)
    return patched


def invert(before: DocumentNode, deltas: tuple[DocumentDelta, ...]) -> tuple[DocumentDelta, ...]:
    old = _index(before)
    return tuple(_invert(delta, old) for delta in reversed(deltas))


def encode(deltas: tuple[DocumentDelta, ...]) -> bytes:
    return _DELTA_ENCODER.encode(deltas)


def decode(payload: bytes) -> tuple[DocumentDelta, ...]:
    return _DELTA_DECODER.decode(payload)


def _invert(delta: DocumentDelta, old: dict[ContentKey, tuple[DocumentNode, ContentKey | None, int]]) -> DocumentDelta:
    match delta:
        case Inserted(node=node):
            return Deleted(key=node.meta.key)
        case Deleted(key=key):
            node, parent, index = old[key]
            return Inserted(parent=parent, index=index, node=node) if parent is not None else delta
        case Moved(key=key):
            node, parent, index = old[key]
            return Moved(key=key, parent=parent, index=index) if parent is not None else delta
        case Reparametrized(key=key):
            node, _, _ = old[key]
            return Reparametrized(key=key, fields=_all_fields(node))
        case _ as unreachable:
            assert_never(unreachable)


def _field_delta(prior: DocumentNode, current: DocumentNode) -> dict[str, msgspec.Raw]:
    prior_fields = dict(_own(prior))
    return {name: msgspec.Raw(_DELTA_ENCODER.encode(value)) for name, value in _own(current) if prior_fields.get(name) != value}


def _all_fields(node: DocumentNode) -> dict[str, msgspec.Raw]:
    return {name: msgspec.Raw(_DELTA_ENCODER.encode(value)) for name, value in _own(node)}


def _apply_fields(node: DocumentNode, fields: dict[str, msgspec.Raw]) -> DocumentNode:
    merged = {**msgspec.to_builtins(node), **{name: msgspec.json.decode(raw) for name, raw in fields.items()}}
    return msgspec.convert(merged, type(node))


def _splice(tree: DocumentNode, parent: ContentKey, index: int, node: DocumentNode) -> DocumentNode:
    return _retarget(tree, parent, lambda target: _with_spine(target, (*_spine(target)[:index], node, *_spine(target)[index:])))


def _prune(tree: DocumentNode, key: ContentKey) -> DocumentNode:
    kids = _spine(tree)
    if any(child.meta.key == key for child in kids):
        return _with_spine(tree, tuple(child for child in kids if child.meta.key != key))
    return _with_spine(tree, tuple(_prune(child, key) for child in kids)) if kids else tree


def _retarget(tree: DocumentNode, key: ContentKey, fn: Callable[[DocumentNode], DocumentNode]) -> DocumentNode:
    if tree.meta.key == key:
        return fn(tree)
    kids = _spine(tree)
    return _with_spine(tree, tuple(_retarget(child, key, fn) for child in kids)) if kids else tree


def _find(tree: DocumentNode, key: ContentKey) -> DocumentNode:
    return next(node for node in walk(tree) if node.meta.key == key)
```

## [04]-[RESEARCH]

- [OCR_DEFERRED]: a scanned page with no embedded text recovers no `RunNode` leaf — `documents/lens` reads zero glyphs, so the tree has empty text runs. OCR (ocrmypdf driving Tesseract, a native binary) is the one path that would synthesize `RunNode` text from a rasterized page. It carries a native binary outside the cp315-core wheel set and the `python_version<'3.15'` band, so it is a deferred-admission concern, never a phantom fence member: the tree models the recovered structure faithfully (empty runs on a scanned page), and OCR enrichment lands as a future `documents/lens` arm over an admitted OCR owner. `model.md` owns no OCR surface; the tree type is complete without it.
- [DIGEST_VS_IDENTITY]: `node_digest` is the Merkle CONTENT fold — a leaf keys over its serialized bytes and an interior node folds its child digests through `ContentIdentity.of(tuple_of_child_keys)` (the `folder:../../../runtime/evidence/identity#CONTENT_IDENTITY` little-endian child serialization), so any descendant edit re-keys every ancestor digest. That re-keying is exactly why the diff does NOT key by `node_digest`: an instable parent reference would spuriously `Moved` every sibling of an inserted node and break `merge`. The diff keys instead by the STABLE `NodeMeta.key` minted once per node at authoring/recovery time, so an edit at one node never perturbs its ancestors' diff keys. `node_digest` serves the cache-hit-by-reference and corpus-residency identity (a content-identical sub-tree keys identically for reuse elision); the diff/merge/move detection serves structural identity through `NodeMeta.key`. Two distinct keyings, never conflated.
- [STRUCT_ROLE_TOTALITY]: the PDF/UA structure-type vocabulary is closed except at one extension point, so `StructRole` is a two-arm tagged `Union` under `tag_field="role_kind"` — `StandardRole` wrapping the `StructEltKind` `StrEnum` of the nineteen standard-structure roles (`Document`/`Part`/`Sect`/`H1`-`H6`/`P`/`L`/`LI`/`Table`/`TR`/`TD`/`Figure`/`Caption`/`Link`/`Note`), and `ForeignRole` carrying the one `ForeignRoleStr` escape. A standard role is a `StructEltKind` member (a `StrEnum` row, never a sibling struct), so a new standard role is one enum line plus one `_STRUCT_CATEGORY` row and `role_of`'s `StandardRole` arm plus `role_category`'s table lookup absorb it; a foreign role is the single `ForeignRole` arm `match` reaches, categorized as the named `_FOREIGN_CATEGORY` open-default row rather than a bare inline literal. The `StructEltKind` value strings are the literal PDF/UA standard-structure-type names, the one literal source on the page that traces to the external standard vocabulary rather than an axis; `ForeignRoleStr` constrains the foreign role to a non-empty `[A-Za-z][\w.\-]*` identifier through `Meta(min_length, max_length, pattern)`, validated on decode and on the `Reparametrized` re-coerce through `msgspec.convert`. The PDF/UA vocabulary is not bare: `_STRUCT_CATEGORY` is the one frozen behavior table keyed by `StructEltKind` carrying each role's `StructCategory` (`GROUPING` for `Document`/`Part`/`Sect`, `HEADING` for `H1`-`H6`, `LIST` for `L`/`LI`, `TABLE` for `Table`/`TR`/`TD`, `INLINE` for `Link`, `BLOCK` for `P`/`Caption`/`Note`, `ILLUSTRATION` for `Figure`) and its `heading_level` (`1`-`6` on the heading rows, `0` elsewhere), the one primary correspondence `role_category` projects so the AUDIT folds structural-nesting legality and heading-level monotonicity over one table lookup rather than nineteen per-role arms — the smart-enum behavior-row collapse the bare `StrEnum` vocabulary alone defeats. The closed family and the `FigureNode` alt field carry three audit views behind the one `role_contract(view)` dispatch keyed by `RoleView`, never a `role_schema`/`role_inspect` sibling pair: `RoleView.SCHEMA` returns `msgspec.json.schema_components((StructRole,), ref_template="#/components/{name}")` — the `(components, defs)` JSON-schema role-coverage contract a multi-type audit specification embeds, the first element the per-type schema tuple whose lone entry carries the `anyOf` ref pair plus the `role_kind` discriminator mapping, the second the `$defs` map binding `StandardRole`/`StructEltKind`/`ForeignRole` to their `#/components/{name}` definitions, so the standard `StandardRole` (its `elt` an `enum` over the nineteen `StructEltKind` rows) and foreign `ForeignRole` (its `role` a `pattern`-constrained string) surface as referenced definitions without re-deriving; `RoleView.INSPECT` returns `msgspec.inspect.multi_type_info((StructRole,))` — the one-element `tuple[inspect.Type, ...]` wrapping the `inspect.UnionType` whose `.types` holds the two `inspect.StructType` arms (the `StandardRole` arm's `elt` field an `inspect.EnumType` whose `.cls` is the `StructEltKind` enum carrying every row) the in-process AUDIT totality walk reads to prove the closed family covers the full vocabulary; `RoleView.CONTRACT` returns `msgspec.structs.fields(FigureNode)` — the `FieldInfo` set the AUDIT reads to confirm the `alt` field name, `AltText` annotation, and `""` default off the live struct. The deleted `StructureNode.tag_role: str` had no such audit target, and the bare `json.schema` whole-blob form carried no extractable component map. The nested `role_kind` discriminant is independent of the outer `kind` discriminant: `StructureNode` decodes its `kind="structure"` tag first, then `role` decodes its own `role_kind` tag, two tag fields on two `Union` levels that never collide because they name distinct fields. The `kind` tag the corpus `kind` column reads is recovered from `node.__struct_config__.tag` (the `msgspec.structs.StructConfig` runtime view of the variant's tag), never a parallel `match` re-enumerating the nine variants to recover a literal the tag already carries.
- [ALT_TEXT_PRESENCE]: `FigureNode.alt: AltText` is the alt-text-presence fact the `accessibility/tagged#ACCESS` AUDIT verifies and the `figures/compose#COMPOSE` unit authors, `AltText` an `Annotated[str, Meta(max_length=2048)]` admitting the empty string as the default (an un-authored figure carries `alt=""`, the audit's failing case) while bounding the upper length on decode. `to_corpus_row` projects the `CorpusRow.alt: AltText` and `CorpusRow.alt_status: AltStatus` columns from the one `alt_of` `(AltText, AltStatus)` pair (`PRESENT` with the authored alt on a figure, `ABSENT` with `""` on `alt=""`, `NA` with `""` on every non-figure node) so the `FigureNode` discriminant fires once per row, and the audit reads alt-text presence as the single `kind == FIGURE and alt_status == ABSENT` column predicate over the content-keyed corpus rather than re-walking the tree or re-deriving emptiness per row, and `to_typst_source` folds the `FigureNode` through the `_image` emitter that owns the `alt: none | str` decision — a non-empty `alt` writes the string-context-escaped `image(source, alt: "<escaped>")`, an empty `alt` writes `image(source, alt: none)` (the Typst `image` markup `alt: none | str` default the folder `.api/typst.md` `[MARKUP_ELEMENT_SCOPE]` catalogues), so the absent case stays `none` rather than collapsing to an empty-string equivalent that asserts a meaningless image, the Typst compiler emits the text equivalent into the marked-content figure structure element of the PDF/UA render, and the future `documents/emit#DOCUMENT TYPST_QUERY` pass reads the compiled `image` selector back through `typst.query` to confirm the equivalent landed. The `alt` equivalent rides the inner `image`, never doubled onto the enclosing `figure(.., alt: ..)` — Typst reads both when the figure body is an image with its own `alt`, so the figure-level `alt` stays reserved for custom-content figures. The `_typst(value, scope)` escaper is one `TypstScope`-keyed `str.maketrans` algebra: the `STRING` context escapes `\` and `"` for the `alt`/`asset_key` quoted-string arguments, and the `MARKUP` context escapes the Typst content-mode active set `\[]#*_@$<>` and backtick for every interpolated `RunNode.text`, heading, and caption run, so an `alt` carrying a quote or a run carrying `]`/`#` produces valid markup rather than a broken literal or a mis-parsed `caption: [..]`/`#strong[..]` block — the `.api/typst.md` `[04] markup axis` escaping law applied to both the string and the content interpolation contexts, not only the `alt` string. The field's presence is independently provable: `role_contract(RoleView.CONTRACT)` returns `msgspec.structs.fields(FigureNode)`, so the AUDIT reads the `alt` `FieldInfo` — its `name`, its `AltText` `encode_name`/`type`, and its `""` default — off the live struct rather than re-declaring the field set, the `CONTRACT` view a third row on the same `RoleView` axis the schema and inspect views ride. The field rides the `Reparametrized` own-field overlay with every other `FigureNode` own field, so a re-authored alt re-keys the figure's `node_digest` and emits one `Reparametrized` edit; no parallel alt-tracking surface exists.
- [MSGSPEC_RECURSIVE_UNION]: the nine `DocumentNode` variants form a recursive `Union` via the `from __future__ import annotations` forward reference on the `children`/`heading`/`runs`/`caption`/`rows` fields that recurse into `DocumentNode`; `msgspec.json.Decoder(DocumentNode)` resolves the forward reference at decoder construction and discriminates on the `tag_field="kind"` tag (the `NodeKind` value), so the tree round-trips without a custom `dec_hook` and the decoded struct exposes the `kind` only as the encoded field, never a runtime `.kind`/`.tag` attribute. The `DocumentDelta` patch decodes as `tuple[DocumentDelta, ...]` under `tag_field="edit"`. The `Reparametrized` field map carries `msgspec.Raw` opaque values over the OWN fields only (every field except the structural `children`), and `_apply_fields` overlays them onto `msgspec.to_builtins(node)` then re-coerces through `msgspec.convert(merged, type(node))` so the changed own-fields re-validate against the concrete leaf variant's field types in one pass — no per-field annotation lookup (`__future__` annotations are strings, so a field-type decode would mis-resolve), no eager whole-tree re-validation, and a node's structural children are untouched by an own-content overlay.

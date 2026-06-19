# [PY_ARTIFACTS_MODEL]

The semantic document algebra: the single interior representation the `documents` axis lowers FROM and recovers TO. `DocumentNode` is ONE recursive `msgspec` tagged-union tree (page/section/block/run/table/figure/field/annotation/structure_element) carrying a closed `NodeMeta` tag on every node, and `DocumentDelta` is ONE diff/merge algebra (inserted/deleted/moved/reparametrized edits) keyed by the runtime content key and defined once over the tree. Every `folder:documents/emit#DOCUMENT` backend becomes a lowering arm folding from this tree rather than dispatching an opaque payload, and `folder:documents/lens#LENS` is the recover-TO inverse that rebuilds it — so production and extraction are inverses over one node algebra, the extracted-tree corpus keys into the runtime columnar lane as a queryable value, and the `DocumentDelta` a structural diff reuses is defined here once. The tree round-trips through `msgspec.json` so a multi-PDF corpus is one content-keyed serialized value; identity comes from `folder:../../../runtime/evidence/identity#CONTENT_IDENTITY` `ContentIdentity.of`, never re-minted.

## [1]-[INDEX]

- [1]-[NODE]: `DocumentNode` — the recursive nine-variant `msgspec` tagged-union tree + the `NodeMeta` closed tag every node carries; the content-keyed `children`/`walk`/`node_digest`/`to_corpus_row`/`to_typst_source`/`encode`/`decode` tree algebra.
- [2]-[DELTA]: `DocumentDelta` — the four-variant edit algebra (inserted/deleted/moved/reparametrized) keyed by `ContentKey`; `diff`/`merge` defined once over the tree as one total fold.

## [2]-[NODE]

- Owner: `DocumentNode` the one recursive interior tree — nine `msgspec.Struct` variants (`PageNode`/`SectionNode`/`BlockNode`/`RunNode`/`TableNode`/`FigureNode`/`FieldNode`/`AnnotationNode`/`StructureNode`) under one `tag`-discriminated `Union`, every variant carrying a `NodeMeta` value object (the closed-family tag: content key, semantic role, page index, optional bounds). The tree is the algebra emission lowers FROM and extraction recovers TO.
- Cases: `PageNode` (page-rooted child sequence + media box) · `SectionNode` (heading-level outline node + child sequence) · `BlockNode` (paragraph/list/quote block with a `BlockKind` row + inline runs) · `RunNode` (styled text run: text, font key, size, weight, RTL flag — the leaf shaped by `folder:../typography/conformance#CONFORM` SHAPE) · `TableNode` (row-major cell grid of child node sequences + span map) · `FigureNode` (embedded-graphic node: content key of the placed asset + caption runs, the unit `folder:../figures/compose#COMPOSE` produces) · `FieldNode` (interactive form field: name, `FieldKind` row, value) · `AnnotationNode` (markup/redaction/link annotation with an `AnnotKind` row + target rect) · `StructureNode` (PDF/UA structure-element node: the tagged-PDF role + child sequence carrying the accessibility tree). Each a frozen `Struct` variant, never a per-kind class hierarchy.
- Entry: `DocumentNode` is a `type` alias over the nine-variant `Union`; construction is direct variant instantiation, decode is `_DOCUMENT_DECODER.decode` (a reusable `msgspec.json.Decoder` typed on the union, the tag round-tripping under `tag_field="kind"`), and re-encode is `_DOCUMENT_ENCODER.encode` (a reusable `msgspec.json.Encoder`). `node_digest` folds a node's identity over its content + children into one `ContentKey` so the tree is content-addressed; `walk` yields every node in document order for the lens fold and the corpus projection.
- Auto: `children` is one total `match` projecting each variant to its interior child sequence (leaves return `()`); `node_digest` keys a leaf over its serialized bytes and an interior node over the Merkle fold of its children's keys through `ContentIdentity.of(tuple_of_child_keys)`, so an identical sub-tree keys identically and a re-parametrized node re-keys; `walk` is a pre-order generator over `children`; `to_corpus_row` projects a node to the flat `(key, kind, role, page, text)` row the runtime columnar lane ingests; `to_typst_source` is the one Typst-markup lowering folding the tree to the source string the `documents/emit#DOCUMENT` typst rows compile, defined once here so the three Typst modes share one lowering rather than three string templates.
- Receipt: the recovered tree contributes the `folder:../receipt/receipt#RECEIPT` introspection case (content key, node count, text length, image count, hit count) at the lens boundary; `model.md` owns the tree type and its digest, never the receipt fold — authoring stays at `documents/emit`, recovery at `documents/lens`.
- Packages: `msgspec` (`Struct(frozen=True, tag=True, tag_field=...)` variant tree, `Union` alias, `json.Encoder`/`json.Decoder` typed round-trip, `convert` boundary coercion, `structs.replace` copy-with); runtime (`content_identity.ContentIdentity`/`ContentKey` for the node digest, consumed never re-minted).
- Growth: a new document concept is one `DocumentNode` variant (a frozen `Struct` carrying its payload + `NodeMeta`) plus one `children`/`node_digest` arm; the decoder, the diff fold, and every backend pick it up by the total `match`. A new structured value on an existing node is one field; zero new surface.
- Boundary: the opaque `dict[str, object]` payload `documents/emit` formerly dispatched over is the deleted form — every backend now lowers from this tree. No durable store, no PDF parser (extraction is `documents/lens`'s pymupdf/pypdf/lxml surface), no UI, no second tree type per backend. The tree is the canonical interior representation; the wire projection into the columnar corpus is `to_corpus_row`, never a parallel serialized model. A flat `class DocumentNode` with a stringly-typed `kind: str` field and an `if kind == "page"` cascade is the rejected shape; the closed tag and the total `match` are the totality proof.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
from __future__ import annotations

from collections.abc import Iterator
from enum import StrEnum
from typing import Final, assert_never

import msgspec
from msgspec import Struct

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


# --- [MODELS] ---------------------------------------------------------------------------


class NodeMeta(Struct, frozen=True):
    key: ContentKey
    role: str
    page: int
    bounds: tuple[float, float, float, float] | None = None


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
    tag_role: str
    children: tuple[DocumentNode, ...] = ()


type DocumentNode = PageNode | SectionNode | BlockNode | RunNode | TableNode | FigureNode | FieldNode | AnnotationNode | StructureNode

# --- [CONSTANTS] ------------------------------------------------------------------------

_DOCUMENT_ENCODER: Final = msgspec.json.Encoder()
_DOCUMENT_DECODER: Final = msgspec.json.Decoder(DocumentNode)

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


def to_corpus_row(node: DocumentNode) -> dict[str, str]:
    text = "".join(run.text for run in walk(node) if isinstance(run, RunNode))
    return {"key": node.meta.key.hex, "kind": _kind_of(node).value, "role": node.meta.role, "page": str(node.meta.page), "text": text}


def to_typst_source(node: DocumentNode) -> str:
    match node:
        case RunNode(text=text, weight=weight):
            return f"#strong[{text}]" if weight >= 700 else text
        case BlockNode(block=BlockKind.HEADING, runs=runs):
            return "= " + "".join(run.text for run in runs) + "\n"
        case BlockNode(runs=runs, children=kids):
            return "".join(run.text for run in runs) + "".join(to_typst_source(child) for child in kids) + "\n"
        case SectionNode(level=level, heading=head, children=kids):
            return f"{'=' * level} " + "".join(run.text for run in head) + "\n" + "".join(to_typst_source(child) for child in kids)
        case PageNode(children=kids):
            return "".join(to_typst_source(child) for child in kids) + "#pagebreak()\n"
        case StructureNode(children=kids):
            return "".join(to_typst_source(child) for child in kids)
        case TableNode(rows=rows):
            cells = ", ".join("[" + to_typst_source(cell) + "]" for row in rows for cell in row)
            return f"#table(columns: {len(rows[0]) if rows else 0}, {cells})\n"
        case FigureNode(asset_key=asset_key, caption=caption):
            return f'#figure(image("{asset_key.hex}"), caption: [' + "".join(run.text for run in caption) + "])\n"
        case FieldNode() | AnnotationNode():
            return ""
        case _ as unreachable:
            assert_never(unreachable)


def decode(payload: bytes) -> DocumentNode:
    return _DOCUMENT_DECODER.decode(payload)


def encode(node: DocumentNode) -> bytes:
    return _DOCUMENT_ENCODER.encode(node)


def _kind_of(node: DocumentNode) -> NodeKind:
    match node:
        case PageNode():
            return NodeKind.PAGE
        case SectionNode():
            return NodeKind.SECTION
        case BlockNode():
            return NodeKind.BLOCK
        case RunNode():
            return NodeKind.RUN
        case TableNode():
            return NodeKind.TABLE
        case FigureNode():
            return NodeKind.FIGURE
        case FieldNode():
            return NodeKind.FIELD
        case AnnotationNode():
            return NodeKind.ANNOTATION
        case StructureNode():
            return NodeKind.STRUCTURE
        case _ as unreachable:
            assert_never(unreachable)
```

## [3]-[DELTA]

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

## [4]-[RESEARCH]

- [OCR_DEFERRED]: a scanned page with no embedded text recovers no `RunNode` leaf — `documents/lens` reads zero glyphs, so the tree has empty text runs. OCR (ocrmypdf driving Tesseract, a native binary) is the one path that would synthesize `RunNode` text from a rasterized page. It carries a native binary outside the cp315-core wheel set and the `python_version<'3.15'` band, so it is a deferred-admission concern, never a phantom fence member: the tree models the recovered structure faithfully (empty runs on a scanned page), and OCR enrichment lands as a future `documents/lens` arm over an admitted OCR owner. `model.md` owns no OCR surface; the tree type is complete without it.
- [DIGEST_VS_IDENTITY]: `node_digest` is the Merkle CONTENT fold — a leaf keys over its serialized bytes and an interior node folds its child digests through `ContentIdentity.of(tuple_of_child_keys)` (the `folder:../../../runtime/evidence/identity#CONTENT_IDENTITY` little-endian child serialization), so any descendant edit re-keys every ancestor digest. That re-keying is exactly why the diff does NOT key by `node_digest`: an instable parent reference would spuriously `Moved` every sibling of an inserted node and break `merge`. The diff keys instead by the STABLE `NodeMeta.key` minted once per node at authoring/recovery time, so an edit at one node never perturbs its ancestors' diff keys. `node_digest` serves the cache-hit-by-reference and corpus-residency identity (a content-identical sub-tree keys identically for reuse elision); the diff/merge/move detection serves structural identity through `NodeMeta.key`. Two distinct keyings, never conflated.
- [MSGSPEC_RECURSIVE_UNION]: the nine `DocumentNode` variants form a recursive `Union` via the `from __future__ import annotations` forward reference on the `children`/`heading`/`runs`/`caption`/`rows` fields that recurse into `DocumentNode`; `msgspec.json.Decoder(DocumentNode)` resolves the forward reference at decoder construction and discriminates on the `tag_field="kind"` tag (the `NodeKind` value), so the tree round-trips without a custom `dec_hook` and the decoded struct exposes the `kind` only as the encoded field, never a runtime `.kind`/`.tag` attribute. The `DocumentDelta` patch decodes as `tuple[DocumentDelta, ...]` under `tag_field="edit"`. The `Reparametrized` field map carries `msgspec.Raw` opaque values over the OWN fields only (every field except the structural `children`), and `_apply_fields` overlays them onto `msgspec.to_builtins(node)` then re-coerces through `msgspec.convert(merged, type(node))` so the changed own-fields re-validate against the concrete leaf variant's field types in one pass — no per-field annotation lookup (`__future__` annotations are strings, so a field-type decode would mis-resolve), no eager whole-tree re-validation, and a node's structural children are untouched by an own-content overlay.

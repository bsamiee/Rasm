# [PY_ARTIFACTS_GRAPHIC_LAYER]

Semantic layer structure enters every layered writer through one `LayerPlan`. `LayerNode` is a closed group/leaf family, so empty groups remain groups and no node can carry both children and content. `LayerMeta` owns stable editorial identity, semantic intent, paint order, compositing policy, editor state, and optional AEC naming data; `LayerContent` closes the leaf payload modalities. `LayerComp.visible` selects semantic `LayerPath` values instead of ambiguous unqualified names.

`NamingSchema` is regime's `LayerSchema` widened by the one `EDITORIAL` sentinel: `AIA`, `ISO13567`, and `NCS` lower through `LayerName.compose` off the ONE regime roster, while `EDITORIAL` preserves the stable owned segment. `named` returns `Result` because an AEC projection without `LayerMeta.aec` is invalid. `walked` parameterizes a preorder projection over one `Block.unfold` frontier and emits groups and leaves, so empty folders and group-level state survive. `flattened` sequences every naming result through `expression.extra.result.traverse`; arbitrary producer depth never consumes the Python call stack or rebuilds descendant tuples at each level.

`export/layered#LAYERED` consumes THIS tree through its `Layer.of_plan` bridge over the `flattened` projection — its flat `Layer` value is the per-target WRITER ROW the semantic tree lowers into, never a second semantic model. Producers author `LayerPlan` beside rendered content and pass both as data. `BlendMode` remains canonical in `graphic/color/derive#DERIVE`, and every writer maps the closed family or returns its own typed unsupported-mode fault.

## [01]-[INDEX]

- [01]-[LAYER]: Closed `LayerNode` and `LayerContent` families, shared `LayerMeta`/`LayerState`, semantic `LayerPath` comp selection, complete AIA/ISO 13567/NCS/editorial naming policy, and the stack-safe parameterized `walked` projection plus fault-preserving `flattened` lowering form one layer owner.

## [02]-[LAYER]

- Owner: `LayerPlan` is the sole semantic tree. `roots` retain hierarchy, `LayerIntent` routes meaning independently of producer names, and `LayerComp` records audience view-states against full semantic paths.
- Cases: `LayerNode.group` carries `LayerMeta` plus ordered children, including an empty child set; `LayerNode.leaf` carries the same metadata plus one `LayerContent`. `LayerContent.fragment`, `keyed`, and `entities` close serialized vector, work-graph, and CAD-handle payload timing without nullable fields.
- State: `LayerState` carries independent editor axes, and `Amount` refines both fill and layer opacity to `[0, 1]`. Visibility, locking, print/export inclusion, clipping, isolation, and knockout survive round-trip as data each writer lowers.
- Naming: `LayerMeta.name` is the stable semantic path segment. `named` composes `LayerMeta.aec` through the selected regime grammar and returns `<missing-aec-name>` when an AEC projection lacks its required source value; no AEC schema silently emits an editorial spelling.
- Traversal: `walked` owns one immutable preorder frontier and accepts the node projection as data. `Block.unfold` carries arbitrary depth, emits empty groups as first-class rows, and fixes siblings through `zsorted`; `flattened` uses nested `traverse` calls so every node path succeeds or the first typed naming fault aborts the projection.
- Identity: `LayerPath` contains every stable editorial segment from root to node. View-state membership never keys on a leaf name alone, so equal names in distinct groups remain distinct.
- Growth: a semantic meaning extends `LayerIntent`; a payload modality extends `LayerContent` and each writer match; a naming grammar extends the `drawing/regime#REGIME` `LayerSchema` roster alone — `NamingSchema` and `named` widen with it; an editor capability extends `LayerState`; a compositing mode extends `graphic/color/derive#DERIVE` and each writer match.
- Boundary: PSD/OCG/SVG/IDML/TIFF/ORA mutation belongs to `export/layered#LAYERED`, IDML mutation to `export/indesign#INDESIGN`, and PDF OCG authoring to `document/emit#DOCUMENT`. Geometry, receipt, async, host handles, discipline vocabulary, and compositing vocabulary remain outside this value owner.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
from collections.abc import Callable
from dataclasses import dataclass
from enum import StrEnum
from typing import Annotated, Final, Literal, assert_never

from beartype import beartype
from beartype.vale import Is
from expression import Nothing, Ok, Option, Result, Some, case, tag, tagged_union
from expression.collections import Block
from expression.extra.result import catch, sequence, traverse

from rasm.artifacts.drawing.regime import LayerName, LayerSchema
from rasm.artifacts.graphic.color.derive import Amount, BlendMode
from rasm.runtime.identity import ContentKey

# --- [TYPES] ----------------------------------------------------------------------------
type LayerFault = Literal["<missing-aec-name>", "<uncomposable-name>", "<duplicate-sibling-name>"]
type LayerNameText = Annotated[str, Is[lambda value: bool(value.strip())]]
type Fragment = Annotated[bytes, Is[lambda value: len(value) > 0]]
type EntityHandles = Annotated[tuple[str, ...], Is[lambda values: bool(values) and all(value.strip() for value in values)]]
type LayerPath = tuple[LayerNameText, ...]
type LayerProjection[T] = Callable[[tuple["LayerMeta", ...], "LayerNode"], T]
type FlatLayer = tuple[LayerPath, "LayerNode"]


class LayerIntent(StrEnum):
    BACKGROUND = "background"
    REFERENCE = "reference"
    MODEL = "model"
    DATUM = "datum"
    GRID = "grid"
    LINEWORK = "linework"
    HATCH = "hatch"
    FIGURE = "figure"
    IMAGE = "image"
    TABLE = "table"
    TEXT = "text"
    ANNOTATION = "annotation"
    DIMENSION = "dimension"
    SYMBOL = "symbol"
    MASK = "mask"
    GUIDE = "guide"
    ISSUE = "issue"
    OVERLAY = "overlay"


# published-grammar selection IS regime's `LayerSchema` — ONE roster, no parallel enum whose string values
# reconstruct the canonical member; `EDITORIAL` is the one non-grammar naming mode, a literal sentinel.
type NamingSchema = LayerSchema | Literal["editorial"]

EDITORIAL: Final[Literal["editorial"]] = "editorial"


# --- [MODELS] ---------------------------------------------------------------------------
@beartype
@dataclass(frozen=True, slots=True)
class LayerState:
    visible: bool = True
    locked: bool = False
    printable: bool = True
    exportable: bool = True
    clipping: bool = False
    isolated: bool = False
    knockout: bool = False
    fill_opacity: Amount = 1.0


@tagged_union(frozen=True)
class LayerContent:
    tag: Literal["fragment", "keyed", "entities"] = tag()
    fragment: Fragment = case()
    keyed: ContentKey = case()
    entities: EntityHandles = case()

    @staticmethod
    @beartype
    def Fragment(value: Fragment, /) -> "LayerContent":
        return LayerContent(fragment=value)

    @staticmethod
    @beartype
    def Keyed(value: ContentKey, /) -> "LayerContent":
        return LayerContent(keyed=value)

    @staticmethod
    @beartype
    def Entities(values: EntityHandles, /) -> "LayerContent":
        return LayerContent(entities=values)


@beartype
@dataclass(frozen=True, slots=True)
class LayerMeta:
    name: LayerNameText
    intent: LayerIntent
    z: int = 0
    blend: BlendMode = BlendMode.NORMAL
    opacity: Amount = 1.0
    state: LayerState = LayerState()
    aec: Option[LayerName] = Nothing


type GroupPayload = tuple[LayerMeta, tuple["LayerNode", ...]]
type LeafPayload = tuple[LayerMeta, LayerContent]


@tagged_union(frozen=True)
class LayerNode:
    tag: Literal["group", "leaf"] = tag()
    group: GroupPayload = case()
    leaf: LeafPayload = case()

    @staticmethod
    @beartype
    def Group(meta: LayerMeta, children: tuple["LayerNode", ...], /) -> "LayerNode":
        return LayerNode(group=(meta, children))

    @staticmethod
    @beartype
    def Leaf(meta: LayerMeta, content: LayerContent, /) -> "LayerNode":
        return LayerNode(leaf=(meta, content))

    @property
    def meta(self) -> LayerMeta:
        match self:
            case LayerNode(tag="group", group=(meta, _)) | LayerNode(tag="leaf", leaf=(meta, _)):
                return meta
            case _ as unreachable:
                assert_never(unreachable)


@beartype
@dataclass(frozen=True, slots=True)
class LayerComp:
    name: LayerNameText
    visible: frozenset[LayerPath]


@beartype
@dataclass(frozen=True, slots=True)
class LayerPlan:
    schema: NamingSchema
    roots: tuple[LayerNode, ...]
    comps: tuple[LayerComp, ...] = ()


# --- [OPERATIONS] -----------------------------------------------------------------------
def named(meta: LayerMeta, schema: NamingSchema, /) -> Result[str, LayerFault]:
    # a new published grammar is one regime `LayerSchema` roster edit — no string reconstruction seam here.
    match schema:
        case "editorial":
            return Ok(meta.name)
        case LayerSchema():
            # `compose` refuses an incompatible AEC value (an ISO 13567 minor past its element tail) by raise; the refusal
            # rails typed here so `flattened` reports it instead of leaking a ValueError mid-traverse.
            return meta.aec.to_result("<missing-aec-name>").bind(
                lambda value: catch(exception=ValueError)(value.compose)(schema).map_error(lambda _refused: "<uncomposable-name>")
            )
        case _ as unreachable:
            assert_never(unreachable)


def zsorted(nodes: tuple[LayerNode, ...], /) -> tuple[LayerNode, ...]:
    return tuple(sorted(nodes, key=lambda node: (node.meta.z, node.meta.name)))


def walked[T](plan: LayerPlan, project: LayerProjection[T], /) -> Block[T]:
    # the frontier is a persistent Block, so each visit pops the head and conses children with structural sharing —
    # never a `frontier[1:]` tuple copy that re-materializes the pending tail per node on a wide producer tree.
    type Frame = tuple[tuple[LayerMeta, ...], LayerNode]

    def step(frontier: Block[Frame]) -> Option[tuple[T, Block[Frame]]]:
        if frontier.is_empty():
            return Nothing
        (path, node), tail = frontier.head(), frontier.tail()
        match node:
            case LayerNode(tag="group", group=(meta, children)):
                here = path + (meta,)
                return Some((project(here, node), Block.of_seq((here, child) for child in zsorted(children)).append(tail)))
            case LayerNode(tag="leaf", leaf=(meta, _)):
                return Some((project(path + (meta,), node), tail))
            case _ as unreachable:
                assert_never(unreachable)

    return Block.unfold(step, Block.of_seq(((), root) for root in zsorted(plan.roots)))


def flattened(plan: LayerPlan, /) -> Result[tuple[FlatLayer, ...], LayerFault]:
    # sibling names are path segments: two siblings sharing one name collapse onto one LayerPath, so a comp's
    # `visible` set and every exporter keyed by path would silently merge them — refused here, the one egress
    # every path consumer reads; distinct parents reuse a name freely because the full path stays distinct.
    paths = walked(plan, lambda path, _node: tuple(meta.name for meta in path))
    if len(paths) != len(set(paths)):
        return Error("<duplicate-sibling-name>")

    def project(path: tuple[LayerMeta, ...], node: LayerNode) -> Result[FlatLayer, LayerFault]:
        return traverse(lambda meta: named(meta, plan.schema), Block.of_seq(path)).map(lambda names: (tuple(names), node))

    return sequence(walked(plan, project)).map(tuple)


# --- [EXPORTS] --------------------------------------------------------------------------
__all__ = [
    "EDITORIAL",
    "EntityHandles",
    "FlatLayer",
    "Fragment",
    "LayerComp",
    "LayerContent",
    "LayerFault",
    "LayerIntent",
    "LayerMeta",
    "LayerNameText",
    "LayerNode",
    "LayerPath",
    "LayerPlan",
    "LayerProjection",
    "LayerState",
    "NamingSchema",
    "flattened",
    "named",
    "walked",
    "zsorted",
]
```

## [03]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)

# [PY_ARTIFACTS_GRAPHIC_LAYER]

The semantic layer vocabulary of the output plane — one `LayerPlan` tree every layered producer projects into and every layered exporter composes out of, imported downward from s1. The tree is meaning, never a flat name list: a bounded `LayerIntent` roster closes the top level, groups nest by meaning, and every leaf carries its z-order, `BlendMode`, opacity, and content handle. Naming is one law with two projections — AEC output composes `drawing/regime#REGIME`'s ISO 13567 `LayerName` fields, editorial output carries stable owned names — so a PSD group folder, a PDF `/Order` OCG node, a structured SVG `<g id=>`, an IDML layer, and a layered-TIFF/ORA record all spell the same tree.

`export/layered#LAYERED` composes this tree into its per-target writers and never owns it; a producer builds its `LayerPlan` beside its render and hands both to the exporter as data. No receipt, no plan node, no async — the tree is a frozen value, the projections pure folds.

## [01]-[INDEX]

- [01]-[LAYER]: the semantic layer tree — `LayerPlan` root over recursive `LayerNode` group/leaf, closed `LayerIntent`/`BlendMode`/`LayerContent` vocabularies, and the `named`/`flattened`/`zsorted` folds every writer consumes.

## [02]-[LAYER]

- Owner: `LayerPlan` is the one tree — `roots` under a declared `NamingSchema`, each root's intent from the closed `LayerIntent` roster so a consumer routes a whole intent class without knowing a producer's names. Structure is data — no builder, no mutation.
- Cases: `LayerContent` closes the leaf payload; `keyed(ContentKey)` is the work-graph DATA edge the exporter resolves against its warm receipts. `BlendMode` closes the compositing vocabulary at the separable modes plus `NORMAL`, so a blend survives every lowering by name.
- Naming: `named` is the one projection; the ISO 13567 string derives from `LayerNode.aec` and is never stored. The schema is declared once on the plan, so one tree serves both worlds.
- Folds: `flattened` is the one depth-first walk every writer consumes; `zsorted` orders siblings by `z` then name so paint order is deterministic across targets. Both pure and total.
- Growth: a new top-level meaning is one `LayerIntent` member; a new compositing mode one `BlendMode` member (each writer maps it or falls to `NORMAL`); a new payload modality one `LayerContent` case plus one arm per writer; a new naming grammar one `NamingSchema` member plus one `named` arm; a new exporter is a consumer, zero surface here.
- Boundary: no writer — PSD/OCG/SVG/IDML/TIFF/ORA lowering is `export/layered#LAYERED`'s, IDML mutation `export/indesign#INDESIGN`'s, the PDF OCG arm `document/emit#EMIT`'s; no drawn geometry (projectors render, this page structures); no discipline vocabulary (regime's, composed via `aec`); no receipt, identity, or rail. One tree replaces the per-exporter parallel layer model.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
from enum import StrEnum
from typing import Literal

from expression import Nothing, Option, case, tag, tagged_union
from msgspec import Struct

from rasm.artifacts.drawing.regime import LayerName, LayerSchema
from rasm.runtime.identity import ContentKey

# --- [TYPES] ----------------------------------------------------------------------------
class LayerIntent(StrEnum):  # bounded top-level roster; a consumer routes whole intent classes
    BACKGROUND = "background"  # paper, frames, title block underlay
    REFERENCE = "reference"  # screened context: existing work, grids, site context
    LINEWORK = "linework"  # primary drawn geometry, per-discipline children on AEC output
    HATCH = "hatch"  # section fills and pattern bands
    FIGURE = "figure"  # placed produced figures (charts, images, scene extracts)
    ANNOTATION = "annotation"  # labels, leaders, notes
    DIMENSION = "dimension"  # measured dimension sets
    SYMBOL = "symbol"  # marks, north arrows, scale bars, revision clouds
    OVERLAY = "overlay"  # topmost editorial or QA overlays


class BlendMode(StrEnum):  # separable set shared by PSD, PDF /BM, SVG mix-blend-mode
    NORMAL = "normal"
    MULTIPLY = "multiply"
    SCREEN = "screen"
    OVERLAY = "overlay"
    DARKEN = "darken"
    LIGHTEN = "lighten"
    COLOR_DODGE = "color-dodge"
    COLOR_BURN = "color-burn"
    HARD_LIGHT = "hard-light"
    SOFT_LIGHT = "soft-light"
    DIFFERENCE = "difference"
    EXCLUSION = "exclusion"


class NamingSchema(StrEnum):
    ISO13567 = "iso13567"  # AEC output — names compose regime LayerName fields
    EDITORIAL = "editorial"  # stable owned names everywhere else


# --- [MODELS] ---------------------------------------------------------------------------
@tagged_union(frozen=True)
class LayerContent:
    tag: Literal["fragment", "keyed", "entities"] = tag()
    fragment: bytes = case()  # serialized vector fragment — the common projector currency
    keyed: ContentKey = case()  # parent-keyed produced figure/raster (work-graph DATA edge)
    entities: tuple[str, ...] = case()  # DXF entity handles for the CAD-side writers


class LayerNode(Struct, frozen=True):
    # one recursive node: children present = group, content present = leaf; exactly one of the two is set.
    name: str
    intent: LayerIntent
    z: int = 0
    blend: BlendMode = BlendMode.NORMAL
    opacity: float = 1.0
    aec: Option[LayerName] = Nothing  # present on AEC output — the ISO 13567 name derives from it
    children: tuple["LayerNode", ...] = ()
    content: Option[LayerContent] = Nothing


class LayerPlan(Struct, frozen=True):
    schema: NamingSchema
    roots: tuple[LayerNode, ...]


# --- [OPERATIONS] -----------------------------------------------------------------------
def named(node: LayerNode, schema: NamingSchema, /) -> str:
    # ISO 13567 composes the regime fields when present; editorial passes through.
    return node.aec.map(lambda n: n.compose(LayerSchema.ISO13567)).default_value(node.name) if schema is NamingSchema.ISO13567 else node.name


def zsorted(nodes: tuple[LayerNode, ...], /) -> tuple[LayerNode, ...]:
    return tuple(sorted(nodes, key=lambda n: (n.z, n.name)))


def flattened(plan: LayerPlan, /) -> tuple[tuple[tuple[str, ...], LayerNode], ...]:
    # depth-first (path, leaf): the path's last segment is named() applied once here (never re-derived
    # by a writer), the prefix path[:-1] the group folders; siblings zsorted per level.
    def walk(node: LayerNode, path: tuple[str, ...]) -> tuple[tuple[tuple[str, ...], LayerNode], ...]:
        here = path + (named(node, plan.schema),)
        return ((here, node),) if node.content is not Nothing else tuple(
            row for child in zsorted(node.children) for row in walk(child, here)
        )

    return tuple(row for root in zsorted(plan.roots) for row in walk(root, ()))


# --- [EXPORTS] --------------------------------------------------------------------------
__all__ = [
    "BlendMode",
    "LayerContent",
    "LayerIntent",
    "LayerNode",
    "LayerPlan",
    "NamingSchema",
    "flattened",
    "named",
    "zsorted",
]
```

## [03]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)

# [PY_ARTIFACTS_GRAPHIC_LAYER]

The semantic layer VOCABULARY of the whole output plane — one `LayerPlan` tree every layered producer projects INTO and every layered exporter composes OUT of, sited at s1 so the nine projectors (`composition/compose`, `composition/imposition`, `composition/sheet`, `visualization/diagram/draw`, `graphic/marks/encode`, `drawing/dimension`, `drawing/annotate`, `drawing/symbol`, `drawing/detail`) and the layered exporters (`export/layered`, `export/indesign`, `document/emit`'s OCG arm) all import DOWNWARD. The tree is MEANING, never a flat name list: a bounded `LayerIntent` roster closes the top level, groups nest by meaning (a drawing sheet's `LINEWORK` group holds per-discipline children; a diagram's `ANNOTATION` group holds label and leader children), and every leaf carries its z-order, `BlendMode`, opacity, and content handle. Naming is one law with two projections — AEC output composes `drawing/regime#REGIME`'s ISO 13567 `LayerName` fields, editorial output carries stable owned names — so a PSD group folder, a PDF `/Order` OCG node, a structured SVG `<g id=>`, an IDML layer, and a layered-TIFF/ORA record all spell the SAME tree.

`export/layered#LAYERED` COMPOSES this tree into its per-target writers and never owns it; a producer that draws layered content builds its `LayerPlan` beside its render and hands both to the exporter as data. This page mints no receipt, contributes no plan node, and runs no async — the tree is a frozen value, the projections are pure folds.

## [01]-[INDEX]

- [01]-[LAYER]: the semantic layer tree — `LayerIntent` the bounded top-level roster, `BlendMode` the twelve separable compositing modes plus `NORMAL` shared by PSD/PDF/SVG, `LayerContent` the closed leaf-payload family (vector fragment, keyed raster parent, entity handle set), `LayerNode` the recursive group/leaf union with z-order and opacity, `LayerPlan` the root carrying the naming schema, `named()` the ISO 13567/editorial name projection, `flattened()` the depth-first `(path, leaf)` walk every writer folds, and `zsorted()` the deterministic paint order — imported downward by the nine projectors and composed by the layered exporters.

## [02]-[LAYER]

- Owner: `LayerPlan` is the one tree — `roots: tuple[LayerNode, ...]` under a declared `NamingSchema`, each root's intent drawn from the closed `LayerIntent` roster so a consumer can route a whole intent class (screen every `REFERENCE` group, lock every `LINEWORK` group) without knowing a producer's names. `LayerNode` is the recursive union: `group(name, intent, z, blend, opacity, children)` nests meaning; `leaf(name, intent, z, blend, opacity, content)` carries one drawn payload. Structure is data — no builder object, no mutation; a projector constructs the tree bottom-up in one expression.
- Cases: `LayerContent` closes the leaf payload — `fragment(bytes)` a serialized vector fragment (SVG `<g>` innards, the common projector currency), `keyed(ContentKey)` a parent-keyed produced raster or figure (the work-graph DATA edge — the exporter resolves the key against its warm receipts), `entities(tuple[str, ...])` a DXF entity-handle set for the CAD-side writers. `BlendMode` closes the compositing vocabulary at the twelve separable modes plus `NORMAL` — the exact set the PSD writer, the PDF `/BM` name, and the SVG `mix-blend-mode` property share, so a blend survives every lowering by name.
- Naming: `named(node, schema)` is the one projection — `NamingSchema.ISO13567` composes the regime `LayerName` fields riding `LayerNode.aec` (present on AEC output; the ISO string is derived, never stored), `NamingSchema.EDITORIAL` passes the stable owned `name` through. One tree serves both worlds; the schema is declared once on the plan.
- Folds: `flattened(plan)` walks depth-first into `(path, leaf)` rows whose path ends in the leaf's own schema-resolved name — the one iteration every writer consumes (PSD group folders open/close on `path[:-1]` segments, PDF OCG builds its `/Order` array from the same walk, SVG nests `<g>` per segment); `zsorted(nodes)` orders siblings by `z` then name so paint order is deterministic across targets; both are pure and total.
- Growth: a new top-level meaning is one `LayerIntent` member; a new compositing mode is one `BlendMode` member (each writer maps it or falls to `NORMAL` per its own capability row); a new payload modality is one `LayerContent` case plus one arm per writer; a new naming grammar is one `NamingSchema` member plus one `named` arm; a new exporter is a consumer, zero surface here.
- Boundary: no writer (PSD/OCG/SVG/IDML/TIFF/ORA lowering is `export/layered#LAYERED`'s; IDML template mutation `export/indesign#INDESIGN`'s; the PDF OCG emit arm `document/emit#EMIT`'s); no drawn geometry (projectors render, this page structures); no discipline vocabulary (regime's — composed via the `aec` field); no receipt, no identity mint, no rail. A flat name list, a per-exporter parallel layer model, and a stringly blend name are the deleted forms.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
from enum import StrEnum
from typing import Literal

from expression import Nothing, Option, case, tag, tagged_union
from msgspec import Struct

from rasm.artifacts.drawing.regime import LayerName, LayerSchema
from rasm.runtime.identity import ContentKey

# --- [TYPES] ----------------------------------------------------------------------------
class LayerIntent(StrEnum):  # the bounded top-level roster — a consumer routes whole intent classes
    BACKGROUND = "background"  # paper, frames, title block underlay
    REFERENCE = "reference"  # screened context: existing work, grids, site context
    LINEWORK = "linework"  # primary drawn geometry, per-discipline children on AEC output
    HATCH = "hatch"  # section fills and pattern bands
    FIGURE = "figure"  # placed produced figures (charts, images, scene extracts)
    ANNOTATION = "annotation"  # labels, leaders, notes
    DIMENSION = "dimension"  # measured dimension sets
    SYMBOL = "symbol"  # marks, north arrows, scale bars, revision clouds
    OVERLAY = "overlay"  # topmost editorial or QA overlays


class BlendMode(StrEnum):  # the separable compositing set PSD (/BM PDF, mix-blend-mode SVG) share by name
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
    # ONE naming law, two projections: ISO 13567 composes the regime fields when present; editorial passes through.
    return node.aec.map(lambda n: n.compose(LayerSchema.ISO13567)).default_value(node.name) if schema is NamingSchema.ISO13567 else node.name


def zsorted(nodes: tuple[LayerNode, ...], /) -> tuple[LayerNode, ...]:
    return tuple(sorted(nodes, key=lambda n: (n.z, n.name)))


def flattened(plan: LayerPlan, /) -> tuple[tuple[tuple[str, ...], LayerNode], ...]:
    # depth-first (path, leaf) rows — the one walk every writer folds; the path's LAST segment is the
    # leaf's own schema-resolved name (`named` applied once here, never re-derived by a writer), the
    # prefix `path[:-1]` the group folders; paint order is zsorted per level.
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

One tree, many targets: a sheet producer projects its frame into `BACKGROUND`, its per-discipline linework groups into `LINEWORK` with `aec` rows, its dimension sets into `DIMENSION`, and its placed figures into `FIGURE` as `keyed` parents; a diagram producer projects named editorial groups; a mark producer projects its module rows. `export/layered#LAYERED` then folds `flattened(plan)` once per target — PSD folders from the path segments, the real PDF `/Order` OCG tree, nested SVG `<g>`, IDML layers, layered TIFF/ORA records — and the Illustrator-grade hand-off (dozens of named, meaningfully grouped layers, never thousands of loose elements) is a property of the tree, not of any writer. The nine former upward reaches into the exporter's private `Layer` are nine downward imports of this vocabulary.

# [PY_ARTIFACTS_DIAGRAM_SCHEMATIC]

`Schematic` is the named-symbol producer — the diagram class the seven-mark `visualization/diagram/glyphset#GLYPHSET` grammar cannot express, where a mark IS a resistor, an op-amp, a NAND gate, or a flowchart decision with bound anchor terminals. `schemdraw` owns the authoring spine: the closed `elements` vocabulary, the `flow`/`logic`/`dsp` domain modules, the fluent relative-placement algebra chaining each symbol off the prior element's named anchor, `parsing.logicparse` for boolean-expression gate networks, and the `logic` data-driven owners. Its standalone `'svg'` backend renders in-process under `svgconfig.text = 'path'` so every SVG is font-independent, `get_imagedata(SVG)` the one receipt-addressed egress.

Authoring is DATA over the closed grammar: a `SchematicSpec` carries symbol rows or a domain payload, never imperative canvas code at a consumer; theme aesthetics arrive as `graphic/style#STYLE` diagram rows on schemdraw's `config`/`theme`, labels through `typography/shape#SHAPE`. Custom-geometry `Segment*`/`ElementCompound` marks stay `drawing/symbol#SYMBOL`'s — a parametric drafting mark is not a schematic symbol. `emit()` lands the one producer node with its pre-run input key; the GIL-bound render crosses the runtime offload seam.

## [01]-[INDEX]

- [01]-[SCHEMATIC]: the schemdraw catalog owner — `SchematicSpec` the closed authoring union (circuit/flow/dsp symbol rows, a logic boolean expression, kmap/timing/bitfield/truth-table data payloads), `SymbolRow` the relative-placement row, `Schematic` the producer minting `ArtifactReceipt.Diagram`, and the `SchematicFault` rail; consumed by `core/issue#ISSUE`'s diagram suite.

## [02]-[SCHEMATIC]

- Owner: `Schematic` the one producer `(spec, theme)` — it resolves the spec case onto the schemdraw canvas in a single authoring fold, renders once through the standalone SVG backend, and mints one receipt; the element vocabulary is the provider's closed catalog addressed by NAME (`elements.Resistor`, `flow.Decision`, `dsp.Adc`, `logic.Nand` resolve by row string), so a new symbol is a data row, never a method.
- Cases: `SchematicSpec` closes the domain union — `circuit`/`flow`/`dsp` each a `SymbolRow` tuple authored by relative anchor chains, `logic` a boolean expression `parsing.logicparse` lays out via its Buchheim tree (never the corpus routing engine), `kmap`/`truth_table`/`timing`/`bitfield` the data-driven owners from dict payloads. `SymbolRow` is the whole placement grammar as data — `at` names a prior row's anchor (`"U1.out"`), `style` a theme diagram-row key.
- Entry: `emit()` returns ONE `ArtifactWork` keyed PRE-RUN over the canonical spec+theme bytes; `_emit` authors, renders, and mints `ArtifactReceipt.Diagram(...)` threading the same key. A schematic inside a diagram suite arrives as one node beside draw's per-kind nodes, per-member elision holding.
- Auto: the authoring+render fold offloads through `LanePolicy.offload(..., modality=Modality.INTERPRETER)`, `svgconfig.text = 'path'` set once per render context; provider raises map into `SchematicFault` (`element` an unknown symbol, `anchor` an unresolvable placement reference, `parse` a logic-expression failure, `render` a backend failure).
- Growth: a new symbol is one row (the registry resolves the provider catalog by name); a new domain one `SchematicSpec` case plus one authoring arm; a new aesthetic axis one theme diagram row; zero new surface for a new consumer.
- Boundary: no generic graph layout or routing (`visualization/diagram/layout#LAYOUT`'s engines), no seven-mark rendering (`visualization/diagram/draw#DRAW`'s), no custom `Segment*`/`ElementCompound` geometry (`drawing/symbol#SYMBOL`'s), no rasterization or matplotlib backend (the standalone SVG backend is the egress), no receipt or identity beyond the runtime mint; hand-emitted SVG, imperative consumer canvas code, and a parallel symbol vocabulary are the rejected forms.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
from collections.abc import Mapping
from enum import StrEnum
from typing import Literal

from expression import Result, Some, case, tag, tagged_union
from msgspec import Struct

from rasm.artifacts.core.plan import Admission, ArtifactWork
from rasm.artifacts.core.receipt import ArtifactReceipt
from rasm.artifacts.graphic.layer import LayerContent, LayerIntent, LayerNode, LayerPlan, NamingSchema
from rasm.artifacts.graphic.style import Theme
from rasm.runtime.faults import RuntimeRail
from rasm.runtime.identity import CANONICAL_POLICY, ContentIdentity, ContentKey
from rasm.runtime.lanes import LanePolicy, Modality

lazy import schemdraw
lazy from schemdraw import dsp as _dsp
lazy from schemdraw import elements as _elm
lazy from schemdraw import flow as _flow
lazy from schemdraw import logic as _logic
lazy from schemdraw.parsing import logicparse

# --- [TYPES] ----------------------------------------------------------------------------
type Direction = Literal["right", "left", "up", "down"]


class SchematicKind(StrEnum):  # the receipt kind facet — one member per SchematicSpec case
    CIRCUIT = "circuit"
    FLOW = "flow"
    DSP = "dsp"
    LOGIC = "logic"
    KMAP = "kmap"
    TRUTH_TABLE = "truth_table"
    TIMING = "timing"
    BITFIELD = "bitfield"


# --- [MODELS] ---------------------------------------------------------------------------
class SymbolRow(Struct, frozen=True):
    # one placed symbol as data, chained off a prior row's named anchor.
    ref: str  # the row's own reference ("U1", "R3"); anchor addresses compose "<ref>.<anchor>"
    element: str  # provider catalog name
    at: str = ""  # "<ref>.<anchor>" placement source; "" = chain off the previous row
    anchor: str = ""  # which own anchor seats on `at`
    direction: Direction = "right"
    length: float = 0.0  # two-terminal stretch; 0 = element default
    label: str = ""
    style: str = ""  # theme diagram-row key; "" = theme default ink


@tagged_union(frozen=True)
class SchematicSpec:
    tag: Literal["circuit", "flow", "dsp", "logic", "kmap", "truth_table", "timing", "bitfield"] = tag()
    circuit: tuple[SymbolRow, ...] = case()
    flow: tuple[SymbolRow, ...] = case()
    dsp: tuple[SymbolRow, ...] = case()
    logic: str = case()  # boolean expression -> parsing.logicparse
    kmap: Mapping[str, object] = case()
    truth_table: Mapping[str, object] = case()
    timing: Mapping[str, object] = case()
    bitfield: Mapping[str, object] = case()


# --- [ERRORS] ---------------------------------------------------------------------------
@tagged_union(frozen=True)
class SchematicFault:
    tag: Literal["element", "anchor", "parse", "render"] = tag()
    element: str = case()
    anchor: str = case()
    parse: str = case()
    render: str = case()


# --- [OPERATIONS] -----------------------------------------------------------------------
class Schematic(Struct, frozen=True):
    spec: SchematicSpec
    theme: Theme

    def emit(self, /) -> ArtifactWork:
        return ArtifactWork(key=self._key, work=self._emit, parents=(), admission=Admission(keyed=None), cost=1.0)

    @property
    def _key(self) -> ContentKey:
        # key-over-input: canonical spec+theme-key bytes, computable BEFORE the render runs.
        return ContentIdentity.of("schematic", (self.spec, self.theme.key), policy=CANONICAL_POLICY)

    async def _emit(self) -> RuntimeRail[ArtifactReceipt]:
        drawn = await LanePolicy.offload(self._render, modality=Modality.INTERPRETER)
        return drawn.map(
            lambda r: ArtifactReceipt.Diagram(self._key, r[1].value, r[2], r[3], "schemdraw")
        )

    def _render(self) -> Result[tuple[bytes, SchematicKind, int, int], SchematicFault]:
        # one authoring fold: resolve the spec onto the Drawing canvas, svgconfig.text='path', get_imagedata(SVG) once.
        schemdraw.use("svg")
        schemdraw.svgconfig.text = "path"
        match self.spec:
            case SchematicSpec(tag="logic", logic=expr):
                drawing = logicparse(expr)
                return Result.Ok((drawing.get_imagedata("svg"), SchematicKind.LOGIC, len(drawing.elements), 0))
            case SchematicSpec(tag="circuit", circuit=rows) | SchematicSpec(tag="flow", flow=rows) | SchematicSpec(tag="dsp", dsp=rows):
                kind = SchematicKind(self.spec.tag)
                with schemdraw.Drawing(show=False) as drawing:
                    placed: dict[str, object] = {}
                    for row in rows:
                        family = {"circuit": _elm, "flow": _flow, "dsp": _dsp}[self.spec.tag]
                        cls = getattr(family, row.element, None)
                        if cls is None:
                            return Result.Error(SchematicFault(element=row.element))
                        symbol = cls().label(row.label) if row.label else cls()
                        symbol = getattr(symbol, row.direction)()
                        if row.at:
                            ref, _, anchor = row.at.partition(".")
                            host = placed.get(ref)
                            if host is None:
                                return Result.Error(SchematicFault(anchor=row.at))
                            symbol = symbol.at(getattr(host, anchor))
                        if row.length:
                            symbol = symbol.length(row.length)
                        drawing += symbol
                        placed[row.ref] = symbol
                edges = sum(1 for row in rows if row.at)
                return Result.Ok((drawing.get_imagedata("svg"), kind, len(rows), edges))
            case SchematicSpec(tag="kmap", kmap=data):
                grid = _logic.Kmap(**dict(data))
                return Result.Ok((_lone(grid), SchematicKind.KMAP, len(data), 0))
            case SchematicSpec(tag="truth_table", truth_table=data):
                return Result.Ok((_lone(_logic.Table(**dict(data))), SchematicKind.TRUTH_TABLE, len(data), 0))
            case SchematicSpec(tag="timing", timing=data):
                return Result.Ok((_lone(_logic.TimingDiagram(**dict(data))), SchematicKind.TIMING, len(data), 0))
            case SchematicSpec(tag="bitfield", bitfield=data):
                return Result.Ok((_lone(_logic.BitField(**dict(data))), SchematicKind.BITFIELD, len(data), 0))

    def layers(self, svg: bytes, /) -> LayerPlan:
        # schemdraw renders ONE SVG (symbols + lettering fused), so the plan carries one LINEWORK root;
        # a content-less sibling `flattened()` drops as a phantom branch.
        return LayerPlan(
            schema=NamingSchema.EDITORIAL,
            roots=(
                LayerNode(name="symbols", intent=LayerIntent.LINEWORK, content=Some(LayerContent(fragment=svg))),
            ),
        )


def _lone(element: object, /) -> bytes:
    # render one data-driven owner (Kmap/Table/TimingDiagram/BitField) on its own canvas.
    with schemdraw.Drawing(show=False) as drawing:
        drawing += element
    return drawing.get_imagedata("svg")


# --- [EXPORTS] --------------------------------------------------------------------------
__all__ = [
    "Schematic",
    "SchematicFault",
    "SchematicKind",
    "SchematicSpec",
    "SymbolRow",
]
```

## [03]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)

# [PY_ARTIFACTS_VISUALIZATION_DIAGRAM_SCHEMATIC]

The named-symbol schematic producer — the diagram class the seven-mark `visualization/diagram/glyphset#GLYPHSET` grammar cannot express, where a mark IS a resistor, an op-amp, a NAND gate, an ADC block, or a flowchart decision with bound anchor terminals. `schemdraw` owns the authoring spine: the 226-symbol closed `elements` vocabulary plus the `flow`/`logic`/`dsp` domain modules, the fluent relative-placement algebra (`.at`/`.right`/`.up`/`.to`/`.anchor`/`.label`) chaining each symbol off the prior element's named anchor, `parsing.logicparse` building a gate network from a boolean expression, and the `logic.Kmap`/`Table`/`TimingDiagram`/`BitField` data-driven owners for Karnaugh maps, truth tables, timing diagrams, and bit-field registers. The standalone `'svg'` backend renders in-process with `svgconfig.text = 'path'` so every emitted SVG is font-independent, and `get_imagedata(ImageFormat.SVG)` is the one egress the receipt content-addresses.

Authoring is DATA over the closed grammar: a `SchematicSpec` carries symbol rows (element name, anchor chain, label, style key) or the domain payload (`expression` for logic, the truth/timing dicts), never imperative canvas code at a consumer; theme aesthetics arrive as `graphic/style#STYLE` diagram rows lowered onto schemdraw's `config`/`theme` surface; labels shape through `typography/shape#SHAPE` conventions (the `text='path'` outline egress keeps them portable); the drawn result projects into `graphic/layer#LAYER` as an editorial-named tree. The `Segment*`/`ElementCompound` custom-geometry spine stays `drawing/symbol#SYMBOL`'s — a parametric drafting mark is not a schematic symbol. `emit()` lands the one producer node with its pre-run input key; the GIL-bound render crosses the runtime offload seam.

## [01]-[INDEX]

- [01]-[SCHEMATIC]: the schemdraw catalog owner — `SchematicSpec` the closed authoring union (`circuit` symbol rows, `logic` boolean expression, `flow` flowchart rows, `dsp` block rows, `kmap`/`timing`/`bitfield`/`truth_table` data payloads), `SymbolRow` the relative-placement data row, `Schematic` the producer whose `emit()`/`_emit` pair mints `ArtifactReceipt.Diagram`, and the `SchematicFault` rail — consuming glyphset's kind vocabulary boundary, style theme rows, and the layer tree; consumed by `core/issue#ISSUE`'s diagram suite.

## [02]-[SCHEMATIC]

- Owner: `Schematic` is the one producer `(spec, theme)` — it resolves the spec case onto the schemdraw canvas in a single authoring fold, renders once through the standalone SVG backend, and mints one receipt. The element vocabulary is the provider's closed catalog addressed by NAME (`elements.Resistor`, `flow.Decision`, `dsp.Adc`, `logic.Nand` resolve by row string through one registry read), so a new symbol is a data row, never a method.
- Cases: `SchematicSpec` closes the domain union — `circuit(tuple[SymbolRow, ...])` the electrical/electronic net authored by relative anchor chains; `flow(tuple[SymbolRow, ...])` flowcharts; `dsp(tuple[SymbolRow, ...])` signal-flow blocks; `logic(str)` a boolean expression `parsing.logicparse` lays out (its Buchheim tree placement is the built-in fallback, never the corpus routing engine); `kmap(Mapping)`/`truth_table(Mapping)`/`timing(Mapping)`/`bitfield(Mapping)` the data-driven owners rendered from their dict payloads. `SymbolRow(element, at, anchor, direction, length, label, style)` is the whole placement grammar as data — `at` names a prior row's anchor (`"U1.out"`), `direction` the fluent heading, `style` a theme diagram-row key.
- Entry: `emit()` returns ONE `ArtifactWork` — `key` minted PRE-RUN over the canonical spec+theme bytes, `work=self._emit`, `admission=Admission(keyed=None)`; `_emit` authors, renders, and mints `ArtifactReceipt.Diagram(key, kind, nodes, edges, "schemdraw")` threading the same key. A schematic inside a diagram SUITE arrives as one node beside draw's per-kind nodes — per-member elision holds.
- Auto: the authoring+render fold offloads through `LanePolicy.offload(..., modality=Modality.INTERPRETER)`; `svgconfig.text = 'path'` is set once per render context; provider raises map into `SchematicFault` at the arm (`element` an unknown symbol name, `anchor` an unresolvable placement reference, `parse` a logic-expression failure, `render` a backend failure).
- Growth: a new symbol is one row (the registry resolves the provider catalog by name); a new domain is one `SchematicSpec` case plus one authoring arm; a new aesthetic axis is one theme diagram row; zero new surface for a new consumer.
- Boundary: no generic graph layout or routing (`visualization/diagram/layout#LAYOUT` and its engines); no seven-mark rendering (`visualization/diagram/draw#DRAW`); no custom `Segment*`/`ElementCompound` geometry (`drawing/symbol#SYMBOL`); no rasterization (resvg/vl-convert over the SVG at the consuming plane); no matplotlib backend (the standalone SVG backend is the egress; raster routes to the corpus rasterizers); no receipt beyond the one `Diagram` case; no identity beyond the runtime mint. Hand-emitted SVG tags, imperative consumer canvas code, and a parallel symbol vocabulary are the deleted forms.

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
    # one placed symbol as data: the provider element by name, chained off a prior row's named anchor.
    ref: str  # the row's own reference ("U1", "R3") — anchor addresses compose "<ref>.<anchor>"
    element: str  # provider catalog name — "Resistor", "Opamp", "Decision", "Adc", "Nand"
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
    element: str = case()  # unknown provider symbol name
    anchor: str = case()  # unresolvable "<ref>.<anchor>" placement reference
    parse: str = case()  # logic-expression grammar failure
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
        # one authoring fold: resolve the spec case onto the Drawing canvas, svgconfig.text='path',
        # get_imagedata(SVG) once; the SVG bytes ride the layer projection and the receipt facts.
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
        # editorial layer projection: schemdraw renders ONE SVG (symbols and lettering fused in the
        # path-text backend), so the plan carries one content-bearing LINEWORK root — a content-less
        # sibling is dropped by `flattened()` and would hand exporters a phantom branch.
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

The seven marks stay honest and the symbol world gets its own producer: a single-line electrical diagram, a controls logic network, an equipment signal chain, a flowchart, a Karnaugh map, and a timing diagram all author as data rows over the provider's closed catalog, render font-independent through the path-text SVG backend, and land as `Diagram` receipts beside draw's suite nodes with per-member elision on re-issue. Theme rows keep the office ink and lettering on schematics identical to every other plane, the layer projection hands editors the one content-bearing symbols group (schemdraw fuses linework and path-text lettering in a single SVG), and the boundary rows keep parametric drafting marks with `drawing/symbol#SYMBOL` and generic graph routing with the layout engines — one concern, one owner, no grammar bent past its law.

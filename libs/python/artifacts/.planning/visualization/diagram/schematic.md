# [PY_ARTIFACTS_DIAGRAM_SCHEMATIC]

`Schematic` is the named-symbol producer — the diagram class the seven-mark `visualization/diagram/glyphset#GLYPHSET` grammar cannot express, where a mark IS a resistor, an op-amp, a NAND gate, or a flowchart decision with bound anchor terminals. `schemdraw` owns the authoring spine: the closed `elements` vocabulary, the `flow`/`logic`/`dsp` domain modules (state diagrams ride the `flow` case's element roster, never a parallel spec case), the fluent relative-placement algebra chaining each symbol off the prior element's named anchor, the `Ic`/`IcPin` named-pin builder, `parsing.logicparse` for boolean-expression gate networks, and the `logic` data-driven owners. Its standalone `'svg'` backend renders in-process under `svgconfig.text = 'path'` so every SVG is font-independent, `get_imagedata("svg")` the one egress.

Authoring is DATA over the closed grammar: a `SchematicSpec` carries `SymbolRow` rows or a domain payload, never imperative consumer canvas code. `at`/`anchor`/`tox`/`toy` name prior anchors; `theta`/`flip`/`reverse`/`scale`/`dot`/`idot` carry the fluent surface; `pins` builds a named-pin IC whose `PinRow.name` becomes addressable. Admission accumulates duplicate references, invalid pin seats, non-finite geometry, forward anchors, and unknown provider elements before the mutable canvas opens. `DiagramStyle` and the producer's `ThemeMode` close aesthetic selection through `Theme.diagram_ink`. Provider refusals then close onto `SchematicFault`; no `ValueError` crosses the rail.

## [01]-[INDEX]

- [01]-[SCHEMATIC]: the schemdraw catalog owner — `SchematicSpec` the closed authoring union (circuit/flow/dsp symbol rows, a logic boolean expression, kmap/timing/bitfield/truth-table data payloads), `SymbolRow`/`PinRow` the relative-placement grammar as data, `Schematic` the producer minting `ArtifactReceipt.Diagram`, and the fully-constructed `SchematicFault` rail; consumed through `core/issue#ISSUE`'s `works` arm as one `emit()` node.

## [02]-[SCHEMATIC]

- Owner: `Schematic` the one producer `(spec, theme, mode, lane)` — it resolves the spec case onto the schemdraw canvas in a single authoring fold, renders once through the standalone SVG backend, and mints one receipt; the element vocabulary is the provider's closed catalog addressed by NAME (`elements.Resistor`, `flow.Decision`, `dsp.Adc`, `logic.Nand` resolve by row string), so a new symbol is a data row, never a method.
- Cases: `SchematicSpec` closes the domain union — `circuit`/`flow`/`dsp` each a `SymbolRow` tuple authored by relative anchor chains, `logic` a boolean expression `parsing.logicparse` lays out via its Buchheim tree (never the corpus routing engine), `kmap`/`truth_table`/`timing`/`bitfield` the data-driven owners from dict payloads — one total `match` closed by `assert_never`. `SymbolRow` is the whole placement grammar as data: `at` names a prior row's anchor (`"U1.out"`), `anchor` seats one of the row's own anchors, `tox`/`toy` stretch a two-terminal element coordinate-free to another anchor's axis, `theta` overrides the cardinal `direction`, `flip`/`reverse`/`scale` mirror and size, `dot`/`idot` add connection dots, `pins` builds an `Ic`/`Multiplexer` from `PinRow`s, and `style` is the closed `DiagramStyle` member `Theme.diagram_ink(mode, style)` resolves.
- Entry: `emit()` returns ONE `ArtifactWork` keyed PRE-RUN through `ContentIdentity.key` over the length-framed canonical spec⊕theme⊕mode chunks (`docs/laws/patterns` rows [05]/[06]), so `receipt.slot == node.key` and keyed elision holds; `_emit` authors, renders, and mints `ArtifactReceipt.Diagram(key, kind, nodes, edges, "schemdraw", bytes)` — `nodes` the placed-element tally, `edges` the anchored-connection tally (default chains, `at=` anchors, and `tox`/`toy` stretches; provider-authored logic networks count their materialized wire elements), `bytes` the emitted SVG length.
- Auto: the authoring+render fold offloads through `self.lane.offload(Kernel.of(..., KernelTrait.RELEASING))` because the subinterpreter cannot load schemdraw's `ziafont`/`ziamath` render path; `use('svg')` + `svgconfig.text = 'path'` set ONCE at the rail boundary through the cached `_svg_backend` gate, never re-assigned per render. Admission errors land together in `SchematicFault.admission`; a pin-mismatched constructor or unsupported verb lands `element`; an unresolved provider anchor lands `anchor`; `logicparse` or a malformed structured payload lands `parse`; a backend refusal lands `render`; any remaining provider construction refusal lands `provider`. `BoundaryFault.boundary` closes the interior rail at the runtime seam.
- Growth: a new symbol is one row (the registry resolves the provider catalog by name); a new domain one `SchematicSpec` case plus one authoring arm; a new placement verb one `SymbolRow` field mapped to one fluent call; a new aesthetic axis one theme diagram row; zero new surface for a new consumer.
- Boundary: no generic graph layout or routing (`visualization/diagram/layout#LAYOUT`'s engines), no seven-mark rendering (`visualization/diagram/draw#DRAW`'s), no custom `Segment*`/`ElementCompound` geometry (`drawing/symbol#SYMBOL`'s), no rasterization or matplotlib backend (the standalone SVG backend is the egress), no receipt or identity beyond the runtime mint; hand-emitted SVG, imperative consumer canvas code, a parallel symbol vocabulary, an unconstructed fault case, and a subinterpreter offload of the ziafont-bound kernel are the rejected forms.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
from collections.abc import Mapping
from enum import StrEnum
from functools import cache
from math import isfinite
from typing import Literal, assert_never

from expression import Error, Ok, Result, case, tag, tagged_union
from msgspec import Struct, json

from rasm.artifacts.core.plan import Admission, ArtifactWork
from rasm.artifacts.core.receipt import ArtifactReceipt
from rasm.artifacts.graphic.layer import EDITORIAL, LayerContent, LayerIntent, LayerMeta, LayerNode, LayerPlan
from rasm.artifacts.graphic.style import DiagramStyle, Theme, ThemeMode
from rasm.runtime.faults import BoundaryFault, RuntimeRail
from rasm.runtime.identity import ContentIdentity, ContentKey
from rasm.runtime.lanes import LanePolicy
from rasm.runtime.workers import Kernel, KernelTrait

lazy import schemdraw
lazy from schemdraw import dsp as _dsp
lazy from schemdraw import elements as _elm
lazy from schemdraw import flow as _flow
lazy from schemdraw import logic as _logic
lazy from schemdraw.parsing import logicparse

# --- [TYPES] ----------------------------------------------------------------------------
type Direction = Literal["right", "left", "up", "down"]
type PinSide = Literal["L", "R", "T", "B"]


class SchematicKind(StrEnum):  # receipt kind facet — one member per SchematicSpec case
    CIRCUIT = "circuit"
    FLOW = "flow"
    DSP = "dsp"
    LOGIC = "logic"
    KMAP = "kmap"
    TRUTH_TABLE = "truth_table"
    TIMING = "timing"
    BITFIELD = "bitfield"


# --- [CONSTANTS] ------------------------------------------------------------------------
_CANON = json.Encoder(order="deterministic")


# --- [MODELS] ---------------------------------------------------------------------------
class PinRow(Struct, frozen=True):
    # one named IC pin as data; the placed Ic exposes it as an addressable anchor (`"U1.<name>"`).
    name: str
    side: PinSide = "L"
    pos: float | None = None  # 0..1 side-relative seat; None = even distribution
    invert: bool = False  # inversion bubble


class SymbolRow(Struct, frozen=True):
    # one placed symbol as data, chained off a prior row's named anchor; the full fluent surface rides as fields.
    ref: str  # the row's own reference ("U1", "R3"); anchor addresses compose "<ref>.<anchor>"
    element: str  # provider catalog name
    at: str = ""  # "<ref>.<anchor>" placement source; "" = chain off the previous row
    anchor: str = ""  # which own anchor seats on `at`
    direction: Direction = "right"
    theta: float | None = None  # arbitrary angle in degrees; set overrides `direction`
    tox: str = ""  # "<ref>.<anchor>" x-stretch target (two-terminal elements)
    toy: str = ""  # "<ref>.<anchor>" y-stretch target
    length: float = 0.0  # two-terminal stretch; 0 = element default
    scale: float = 1.0
    flip: bool = False
    reverse: bool = False
    dot: bool = False  # connection dot at the end
    idot: bool = False  # connection dot at the start
    label: str = ""
    style: DiagramStyle = DiagramStyle.PRIMARY
    pins: tuple[PinRow, ...] = ()  # named-pin Ic/Multiplexer construction


@tagged_union(frozen=True)
class SchematicSpec:
    tag: Literal["circuit", "flow", "dsp", "logic", "kmap", "truth_table", "timing", "bitfield"] = tag()
    circuit: tuple[SymbolRow, ...] = case()
    flow: tuple[SymbolRow, ...] = case()  # flowchart AND state-diagram rows; the element roster carries both
    dsp: tuple[SymbolRow, ...] = case()
    logic: str = case()  # boolean expression -> parsing.logicparse
    kmap: Mapping[str, object] = case()
    truth_table: Mapping[str, object] = case()
    timing: Mapping[str, object] = case()
    bitfield: Mapping[str, object] = case()


# --- [ERRORS] ---------------------------------------------------------------------------
@tagged_union(frozen=True)
class SchematicFault:
    tag: Literal["admission", "element", "anchor", "parse", "render", "provider"] = tag()
    admission: tuple[str, ...] = case()  # independent row defects accumulated before canvas mutation
    element: str = case()  # unknown catalog name, or a constructor/verb the element does not own
    anchor: str = case()  # unresolvable "<ref>.<anchor>" reference
    parse: str = case()  # logicparse refusal
    render: str = case()  # backend/validator raise at get_imagedata
    provider: str = case()


# --- [SERVICES] ---------------------------------------------------------------------------
class Schematic(Struct, frozen=True):
    spec: SchematicSpec
    theme: Theme
    # `lane` arrives projected via LanePolicy.of(context) at the composition root — a capacity literal has no owner.
    lane: LanePolicy
    mode: ThemeMode = ThemeMode.LIGHT

    def emit(self, /) -> ArtifactWork:
        return ArtifactWork(key=self._key, work=self._emit, parents=(), admission=Admission(keyed=None), cost=1.0)

    @property
    def _seed(self) -> tuple[bytes, ...]:
        # length-framed canonical preimage chunks (patterns rows [05]/[06]); the lane is execution policy, outside the preimage.
        framed = lambda chunk: len(chunk).to_bytes(8, "little") + chunk
        match self.spec:
            case (
                SchematicSpec(tag="circuit", circuit=rows)
                | SchematicSpec(tag="flow", flow=rows)
                | SchematicSpec(tag="dsp", dsp=rows)
            ):
                body = _CANON.encode(rows)
            case SchematicSpec(tag="logic", logic=expr):
                body = _CANON.encode(expr)
            case (
                SchematicSpec(tag="kmap", kmap=data)
                | SchematicSpec(tag="truth_table", truth_table=data)
                | SchematicSpec(tag="timing", timing=data)
                | SchematicSpec(tag="bitfield", bitfield=data)
            ):
                body = _CANON.encode(dict(data))
            case _ as unreachable:
                assert_never(unreachable)
        # theme identity enters as the content-addressed fingerprint over every render-affecting field — two themes
        # sharing one display key never collide, and a styling edit re-keys the diagram.
        return (framed(self.spec.tag.encode()), framed(body), framed(self.theme.fingerprint.encode()), framed(self.mode.value.encode()))

    @property
    def _key(self) -> ContentKey:
        # PRE-RUN key over the canonical input; receipt.slot == node.key.
        return ContentIdentity.key("schematic", self._seed)

    async def _emit(self) -> RuntimeRail[ArtifactReceipt]:
        drawn = await self.lane.offload(Kernel.of(self._render, KernelTrait.RELEASING))
        return drawn.bind(
            lambda inner: inner.map(lambda r: ArtifactReceipt.Diagram(self._key, r[1].value, r[2], r[3], "schemdraw", len(r[0]))).map_error(
                lambda fault: BoundaryFault(boundary=("diagram.schematic", fault.tag))
            )
        )

    def _render(self) -> Result[tuple[bytes, SchematicKind, int, int], SchematicFault]:
        try:
            return self._resolved()
        except (AttributeError, KeyError, OSError, TypeError, ValueError) as bad:
            return Error(SchematicFault(provider=str(bad)))

    def _resolved(self) -> Result[tuple[bytes, SchematicKind, int, int], SchematicFault]:
        # one authoring fold: resolve the spec onto the Drawing canvas, get_imagedata("svg") once.
        _svg_backend()
        match self.spec:
            case SchematicSpec(tag="logic", logic=expr):
                try:
                    drawing = logicparse(expr)
                except ValueError as bad:
                    return Error(SchematicFault(parse=str(bad)))
                # provider-authored gate networks materialize their connectors as wire/line elements — the tally
                # reads the RENDERED topology, never a hardcoded zero.
                wires = sum(1 for element in drawing.elements if isinstance(element, (_elm.Wire, _elm.Line, _elm.Arrow)))
                return _captured(drawing).map(lambda data: (data, SchematicKind.LOGIC, len(drawing.elements) - wires, wires))
            case (
                SchematicSpec(tag="circuit", circuit=rows)
                | SchematicSpec(tag="flow", flow=rows)
                | SchematicSpec(tag="dsp", dsp=rows)
            ):
                kind = SchematicKind(self.spec.tag)
                return self._authored(rows).bind(
                    lambda drawing: _captured(drawing).map(lambda data: (data, kind, len(rows), _connections(rows)))
                )
            case (
                SchematicSpec(tag="kmap", kmap=data)
                | SchematicSpec(tag="truth_table", truth_table=data)
                | SchematicSpec(tag="timing", timing=data)
                | SchematicSpec(tag="bitfield", bitfield=data)
            ):
                kind = SchematicKind(self.spec.tag)
                owner = {"kmap": _logic.Kmap, "truth_table": _logic.Table, "timing": _logic.TimingDiagram, "bitfield": _logic.BitField}[self.spec.tag]
                required = {"kmap": ("names",), "truth_table": ("table",), "timing": ("waved",), "bitfield": ("reg",)}[self.spec.tag]
                missing = tuple(f"{self.spec.tag}:missing:{field}" for field in required if field not in data)
                return (
                    Error(SchematicFault(admission=missing))
                    if missing
                    else _lone(owner, dict(data)).map(lambda drawn: (drawn[0], kind, drawn[1], 0))  # data figures render no connectors — zero is the true tally
                )
            case _ as unreachable:
                assert_never(unreachable)

    def _authored(self, rows: tuple[SymbolRow, ...], /) -> Result["schemdraw.Drawing", SchematicFault]:
        family = {"circuit": _elm, "flow": _flow, "dsp": _dsp}[self.spec.tag]
        return self._admitted(rows, family).bind(lambda admitted: self._placed(admitted, family))

    def _admitted(self, rows: tuple[SymbolRow, ...], family: object, /) -> Result[tuple[SymbolRow, ...], SchematicFault]:
        refs = tuple(row.ref for row in rows)
        issues = (
            (("rows:empty",) if not rows else ())  # an empty symbol set is a mis-keyed spec, never a blank drawing
            + tuple(f"row:{index}:ref" for index, row in enumerate(rows) if not row.ref)
            + tuple(f"ref:duplicate:{ref}" for index, ref in enumerate(refs) if ref and ref in refs[:index])
            + tuple(f"{row.ref}:element:{row.element}" for row in rows if not row.element or getattr(family, row.element, None) is None)
            + tuple(f"{row.ref}:scale:{row.scale}" for row in rows if not isfinite(row.scale) or row.scale <= 0.0)
            + tuple(f"{row.ref}:length:{row.length}" for row in rows if not isfinite(row.length) or row.length < 0.0)
            + tuple(f"{row.ref}:theta:{row.theta}" for row in rows if row.theta is not None and not isfinite(row.theta))
            + tuple(
                f"{row.ref}:pin:{pin.name or '<empty>'}"
                for row in rows
                for index, pin in enumerate(row.pins)
                if not pin.name or pin.name in tuple(item.name for item in row.pins[:index])
            )
            + tuple(
                f"{row.ref}:pin-seat:{pin.name}:{pin.pos}"
                for row in rows
                for pin in row.pins
                if pin.pos is not None and (not isfinite(pin.pos) or not 0.0 <= pin.pos <= 1.0)
            )
            + tuple(
                f"{row.ref}:{verb}:{address}"
                for row_index, row in enumerate(rows)
                for verb, address in (("at", row.at), ("tox", row.tox), ("toy", row.toy))
                if address
                for ref, separator, anchor in (address.partition("."),)
                if address.count(".") != 1 or separator != "." or not ref or not anchor or ref not in refs[:row_index]
            )
        )
        return Error(SchematicFault(admission=issues)) if issues else Ok(rows)

    def _placed(self, rows: tuple[SymbolRow, ...], family: object, /) -> Result["schemdraw.Drawing", SchematicFault]:
        with schemdraw.Drawing(show=False) as drawing:
            placed: dict[str, object] = {}
            for row in rows:  # Exemption: Drawing is the provider's mutable canvas; += insertion is its one authoring surface
                match _symbol(row, family, placed, self.theme, self.mode):
                    case Result(tag="error", error=fault):
                        return Error(fault)
                    case Result(tag="ok", ok=symbol):
                        drawing += symbol
                        placed[row.ref] = symbol
        return Ok(drawing)

    def layers(self, svg: bytes, /) -> LayerPlan:
        # schemdraw renders ONE SVG (symbols + lettering fused), so the plan carries one LINEWORK root.
        return LayerPlan(
            schema=EDITORIAL,
            roots=(LayerNode.Leaf(LayerMeta(name="symbols", intent=LayerIntent.LINEWORK), LayerContent.Fragment(svg)),),
        )


# --- [OPERATIONS] -----------------------------------------------------------------------
def _anchored(placed: dict[str, object], address: str, /) -> Result[object, SchematicFault]:
    # "<ref>.<anchor>" -> the placed element's anchor value; a missing ref or anchor is the typed refusal, never an AttributeError.
    ref, _, anchor = address.partition(".")
    host = placed.get(ref)
    seat = getattr(host, anchor, None) if host is not None else None
    return Ok(seat) if seat is not None else Error(SchematicFault(anchor=address))


def _symbol(
    row: SymbolRow, family: object, placed: dict[str, object], theme: Theme, mode: ThemeMode, /
) -> Result[object, SchematicFault]:
    # One row constructs, orients, seats, stretches, and styles one provider element.
    cls = getattr(family, row.element, None)
    if cls is None:
        return Error(SchematicFault(element=row.element))
    try:
        symbol = cls(pins=[_elm.IcPin(name=pin.name, side=pin.side, pos=pin.pos, invert=pin.invert) for pin in row.pins]) if row.pins else cls()
        symbol = symbol.theta(row.theta) if row.theta is not None else getattr(symbol, row.direction)()
        symbol = symbol.label(row.label) if row.label else symbol
        symbol = symbol.anchor(row.anchor) if row.anchor else symbol
        symbol = symbol.scale(row.scale) if row.scale != 1.0 else symbol
        symbol = symbol.flip() if row.flip else symbol
        symbol = symbol.reverse() if row.reverse else symbol
        symbol = symbol.length(row.length) if row.length else symbol
        symbol = symbol.dot() if row.dot else symbol
        symbol = symbol.idot() if row.idot else symbol
        symbol = symbol.color(theme.diagram_ink(mode, row.style))
        for verb, address in (("at", row.at), ("tox", row.tox), ("toy", row.toy)):
            if address:
                seated = _anchored(placed, address)
                if seated.is_error():
                    return seated
                symbol = getattr(symbol, verb)(seated.default_value(None))
        return Ok(symbol)
    except (TypeError, AttributeError) as refused:
        # a pin set on a non-IC element, or a two-terminal verb on a one-terminal element
        return Error(SchematicFault(element=f"{row.ref}:{row.element}:{refused}"))


def _connections(rows: tuple["SymbolRow", ...], /) -> int:
    # anchored-connection tally over the authored program: an explicit `at=` anchor or the default chain off the
    # previous row (every row past the first) is one connection, and each `tox`/`toy` stretch target adds one —
    # never a `row.at` presence proxy that misses default chaining.
    chained = sum(1 for index, row in enumerate(rows) if row.at or index > 0)
    return chained + sum(int(bool(row.tox)) + int(bool(row.toy)) for row in rows)


@cache
def _svg_backend() -> None:
    # process-global backend selection, set ONCE at the diagram-rail boundary and idempotent thereafter — the
    # standalone SVG engine with font-independent path text. The cache gate keeps the render path free of
    # repeated global mutation (and of races between concurrent THREAD-lane renders); the values are rail
    # constants and this worker is the process's only schemdraw consumer, so no restore is owed.
    schemdraw.use("svg")
    schemdraw.svgconfig.text = "path"


def _captured(drawing: object, /) -> Result[bytes, SchematicFault]:
    try:
        return Ok(drawing.get_imagedata("svg"))
    except ValueError as bad:  # backend/validator refusals (color, linestyle, image format) raise ValueError
        return Error(SchematicFault(render=str(bad)))


def _lone(owner: type, data: dict[str, object], /) -> Result[tuple[bytes, int], SchematicFault]:
    # render one data-driven owner (Kmap/Table/TimingDiagram/BitField) on its own canvas; a malformed payload
    # refuses typed, and the count reads the RENDERED element tally — never the payload's config-key count.
    try:
        with schemdraw.Drawing(show=False) as drawing:
            drawing += owner(**data)
    except (TypeError, ValueError) as bad:
        return Error(SchematicFault(parse=str(bad)))
    return _captured(drawing).map(lambda payload: (payload, len(drawing.elements)))


# --- [EXPORTS] --------------------------------------------------------------------------
__all__ = [
    "PinRow",
    "Schematic",
    "SchematicFault",
    "SchematicKind",
    "SchematicSpec",
    "SymbolRow",
]
```

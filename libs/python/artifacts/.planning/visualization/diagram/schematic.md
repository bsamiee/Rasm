# [PY_ARTIFACTS_DIAGRAM_SCHEMATIC]

`Schematic` is the named-symbol producer — the diagram class the seven-mark `visualization/diagram/glyphset#GLYPHSET` grammar cannot express, where a mark IS a resistor, an op-amp, a NAND gate, or a flowchart decision with bound anchor terminals. `schemdraw` owns the authoring spine: the closed `elements` vocabulary, the `flow`/`logic`/`dsp` domain modules (state diagrams ride the `flow` case's element roster, never a parallel spec case), the fluent relative-placement algebra chaining each symbol off the prior element's named anchor, the `Ic`/`IcPin` named-pin builder, `parsing.logicparse` for boolean-expression gate networks, and the `logic` data-driven owners. Its standalone `'svg'` backend renders in-process under `svgconfig.text = 'path'` so every SVG is font-independent, `get_imagedata("svg")` the one egress.

Authoring is DATA over the closed grammar: a `SchematicSpec` carries `SymbolRow` rows or a domain payload, never imperative consumer canvas code. `at`/`anchor`/`tox`/`toy` name prior anchors; `theta`/`flip`/`reverse`/`scale`/`dot`/`idot` carry the fluent surface; `pins` builds a named-pin IC whose `PinRow.name` becomes addressable. Admission accumulates duplicate references, invalid pin seats, non-finite geometry, forward anchors, and unknown provider elements before the mutable canvas opens. `DiagramStyle` and the producer's `ThemeMode` close aesthetic selection through `Theme.diagram_ink`. Provider refusals then close onto `SchematicFault`; no `ValueError` crosses the rail.

## [01]-[INDEX]

- [02]-[SCHEMATIC]: the schemdraw catalog owner — `SchematicSpec` the closed authoring union (circuit/flow/dsp symbol rows, a logic boolean expression, kmap/timing/bitfield/truth-table data payloads), `SymbolRow`/`PinRow` the relative-placement grammar as data, `Schematic` the bytes producer, and the fully-constructed `SchematicFault` rail; consumed through `core/issue#ISSUE`'s `works` arm as one `emit()` node.

## [02]-[SCHEMATIC]

- Owner: `Schematic` the one producer `(spec, theme, mode, lane)` — it resolves the spec case onto the schemdraw canvas in a single authoring fold and renders once through the standalone SVG backend; the element vocabulary is the provider's closed catalog addressed by NAME (`elements.Resistor`, `flow.Decision`, `dsp.Adc`, `logic.Nand` resolve by row string), so a new symbol is a data row, never a method.
- Cases: `SchematicSpec` closes the domain union — `circuit`/`flow`/`dsp` each a `SymbolRow` tuple authored by relative anchor chains, `logic` a boolean expression `parsing.logicparse` lays out via its Buchheim tree (never the corpus routing engine), `kmap`/`truth_table`/`timing`/`bitfield` the data-driven owners from dict payloads — one total `match` closed by `assert_never`. `SymbolRow` is the whole placement grammar as data: `at` names a prior row's anchor (`"U1.out"`), `anchor` seats one of the row's own anchors, `tox`/`toy` stretch a two-terminal element coordinate-free to another anchor's axis, `theta` overrides the cardinal `direction`, `flip`/`reverse`/`scale` mirror and size, `dot`/`idot` add connection dots, `pins` builds an `Ic`/`Multiplexer` from `PinRow`s, and `style` is the closed `DiagramStyle` member `Theme.diagram_ink(mode, style)` resolves.
- Entry: `emit()` returns ONE `ArtifactWork` keyed PRE-RUN through `ContentIdentity.key` over the canonical field set the runtime `IdentitySource.parts` fold frames at its one owner (each field length-prefixed under a framed count, so no re-partition of the same bytes collides) — spec⊕theme⊕mode — and `_emit` returns the emitted SVG bytes.
- Auto: the authoring+render fold offloads through `self.lane.offload(Kernel.of(..., KernelTrait.RELEASING))` because the subinterpreter cannot load schemdraw's `ziafont`/`ziamath` render path; `use('svg')` + `svgconfig.text = 'path'` set ONCE at the rail boundary through the cached `_svg_backend` gate, never re-assigned per render. Admission errors land together in `SchematicFault.admission`; a pin-mismatched constructor or unsupported verb lands `element`; an unresolved provider anchor lands `anchor`; `logicparse` or a malformed structured payload lands `parse`; a backend refusal lands `render`; any remaining provider construction refusal lands `provider`. The whole `SchematicFault` crosses the runtime seam on `BoundaryFault.domain` under the `SCHEMATIC_RENDER` coordinate, so the accumulated admission set and every other case survive as matchable evidence rather than as their tag spelling.
- Growth: a new symbol is one row (the registry resolves the provider catalog by name); a new domain one `SchematicSpec` case plus one authoring arm; a new placement verb one `SymbolRow` field mapped to one fluent call; a new aesthetic axis one theme diagram row; zero new surface for a new consumer.
- Boundary: no generic graph layout or routing (`visualization/diagram/layout#LAYOUT`'s engines), no seven-mark rendering (`visualization/diagram/draw#DRAW`'s), no custom `Segment*`/`ElementCompound` geometry (`drawing/symbol#SYMBOL`'s), and no rasterization or matplotlib backend (the standalone SVG backend is the egress); hand-emitted SVG, imperative consumer canvas code, a parallel symbol vocabulary, an unconstructed fault case, and a subinterpreter offload of the ziafont-bound kernel are the rejected forms.

```python
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
from collections.abc import Mapping
from functools import cache, partial
from math import isfinite
from typing import Final, Literal, assert_never

from expression import Error, Ok, Result, case, tag, tagged_union
from expression.collections import Block
from msgspec import Struct, json

from rasm.artifacts.core.hooks import BYTE_VOLUME, DOMAIN, ArtifactsLeg
from rasm.artifacts.core.plan import Admission, ArtifactWork
from rasm.artifacts.graphic.layer import EDITORIAL, LayerContent, LayerIntent, LayerMeta, LayerNode, LayerPlan
from rasm.artifacts.graphic.style import DiagramStyle, Theme, ThemeMode
from rasm.runtime.faults import TRANSIENT, BoundaryFault, FaultRow, RuntimeRail, rostered
from rasm.runtime.identity import ContentIdentity, ContentKey, IdentitySource
from rasm.runtime.lanes import LanePolicy
from rasm.runtime.metrics import Metrics
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


# --- [CONSTANTS] ------------------------------------------------------------------------
_CANON = json.Encoder(order="deterministic")



# --- [TABLES] ---------------------------------------------------------------------------

SCHEMATIC_RENDER: Final[FaultRow[ArtifactsLeg]] = FaultRow(
    leg=ArtifactsLeg.SCHEMATIC, point="render", arm="boundary", defect="schematic-refused", retriability=TRANSIENT
)
RAISES: Final[Block[FaultRow[ArtifactsLeg]]] = rostered(Block.of_seq([SCHEMATIC_RENDER]))

# --- [MODELS] ---------------------------------------------------------------------------
class PinRow(Struct, frozen=True):
    name: str
    side: PinSide = "L"
    pos: float | None = None
    invert: bool = False


class SymbolRow(Struct, frozen=True):
    ref: str
    element: str
    at: str = ""
    anchor: str = ""
    direction: Direction = "right"
    theta: float | None = None
    tox: str = ""
    toy: str = ""
    length: float = 0.0
    scale: float = 1.0
    flip: bool = False
    reverse: bool = False
    dot: bool = False
    idot: bool = False
    label: str = ""
    style: DiagramStyle = DiagramStyle.PRIMARY
    pins: tuple[PinRow, ...] = ()


@tagged_union(frozen=True)
class SchematicSpec:
    tag: Literal["circuit", "flow", "dsp", "logic", "kmap", "truth_table", "timing", "bitfield"] = tag()
    circuit: tuple[SymbolRow, ...] = case()
    flow: tuple[SymbolRow, ...] = case()
    dsp: tuple[SymbolRow, ...] = case()
    logic: str = case()
    kmap: Mapping[str, object] = case()
    truth_table: Mapping[str, object] = case()
    timing: Mapping[str, object] = case()
    bitfield: Mapping[str, object] = case()


# --- [ERRORS] ---------------------------------------------------------------------------
@tagged_union(frozen=True)
class SchematicFault:
    tag: Literal["admission", "element", "anchor", "parse", "render", "provider"] = tag()
    admission: tuple[str, ...] = case()
    element: str = case()
    anchor: str = case()
    parse: str = case()
    render: str = case()
    provider: str = case()


# --- [SERVICES] -------------------------------------------------------------------------
class Schematic(Struct, frozen=True):
    spec: SchematicSpec
    theme: Theme
    lane: LanePolicy
    mode: ThemeMode = ThemeMode.LIGHT

    def emit(self, /) -> ArtifactWork[bytes]:
        key = self._key
        return ArtifactWork(key=key, work=partial(self._emit, key), parents=(), admission=Admission(keyed=None), cost=1.0)

    @property
    def _seed(self) -> tuple[bytes, ...]:
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
        return (self.spec.tag.encode(), body, self.theme.fingerprint.encode(), self.mode.value.encode())

    @property
    def _key(self) -> ContentKey:
        return ContentIdentity.key("schematic", IdentitySource(parts=self._seed))

    async def _emit(self, key: ContentKey, /) -> RuntimeRail[bytes]:
        drawn = await self.lane.offload(Kernel.of(self._render, KernelTrait.RELEASING))
        settled = drawn.bind(lambda inner: inner.map_error(lambda fault: BoundaryFault(domain=(SCHEMATIC_RENDER.subject, fault))))
        match settled:
            case Result(tag="ok", ok=data):
                Metrics.record({BYTE_VOLUME: float(len(data))}, domain=DOMAIN, kind="diagram", scope=self.lane.scope)
                return Ok(data)
            case refused:
                return Error(refused.error)

    def _render(self) -> Result[bytes, SchematicFault]:
        try:
            return self._resolved()
        except (AttributeError, KeyError, OSError, TypeError, ValueError) as bad:
            return Error(SchematicFault(provider=str(bad)))

    def _resolved(self) -> Result[bytes, SchematicFault]:
        _svg_backend()
        match self.spec:
            case SchematicSpec(tag="logic", logic=expr):
                try:
                    drawing = logicparse(expr)
                except ValueError as bad:
                    return Error(SchematicFault(parse=str(bad)))
                return _captured(drawing)
            case (
                SchematicSpec(tag="circuit", circuit=rows)
                | SchematicSpec(tag="flow", flow=rows)
                | SchematicSpec(tag="dsp", dsp=rows)
            ):
                return self._authored(rows).bind(_captured)
            case (
                SchematicSpec(tag="kmap", kmap=data)
                | SchematicSpec(tag="truth_table", truth_table=data)
                | SchematicSpec(tag="timing", timing=data)
                | SchematicSpec(tag="bitfield", bitfield=data)
            ):
                owner = {"kmap": _logic.Kmap, "truth_table": _logic.Table, "timing": _logic.TimingDiagram, "bitfield": _logic.BitField}[self.spec.tag]
                required = {"kmap": ("names",), "truth_table": ("table",), "timing": ("waved",), "bitfield": ("reg",)}[self.spec.tag]
                missing = tuple(f"{self.spec.tag}:missing:{field}" for field in required if field not in data)
                return (
                    Error(SchematicFault(admission=missing))
                    if missing
                    else _lone(owner, dict(data))
                )
            case _ as unreachable:
                assert_never(unreachable)

    def _authored(self, rows: tuple[SymbolRow, ...], /) -> Result["schemdraw.Drawing", SchematicFault]:
        family = {"circuit": _elm, "flow": _flow, "dsp": _dsp}[self.spec.tag]
        return self._admitted(rows, family).bind(lambda admitted: self._placed(admitted, family))

    def _admitted(self, rows: tuple[SymbolRow, ...], family: object, /) -> Result[tuple[SymbolRow, ...], SchematicFault]:
        refs = tuple(row.ref for row in rows)
        issues = (
            (("rows:empty",) if not rows else ())
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
            for row in rows:
                match _symbol(row, family, placed, self.theme, self.mode):
                    case Result(tag="error", error=fault):
                        return Error(fault)
                    case Result(tag="ok", ok=symbol):
                        drawing += symbol
                        placed[row.ref] = symbol
        return Ok(drawing)

    def layers(self, svg: bytes, /) -> LayerPlan:
        return LayerPlan(
            schema=EDITORIAL,
            roots=(LayerNode.Leaf(LayerMeta(name="symbols", intent=LayerIntent.LINEWORK), LayerContent.Fragment(svg)),),
        )


# --- [OPERATIONS] -----------------------------------------------------------------------


def _anchored(placed: dict[str, object], address: str, /) -> Result[object, SchematicFault]:
    ref, _, anchor = address.partition(".")
    host = placed.get(ref)
    seat = getattr(host, anchor, None) if host is not None else None
    return Ok(seat) if seat is not None else Error(SchematicFault(anchor=address))


def _symbol(
    row: SymbolRow, family: object, placed: dict[str, object], theme: Theme, mode: ThemeMode, /
) -> Result[object, SchematicFault]:
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
        return Error(SchematicFault(element=f"{row.ref}:{row.element}:{refused}"))


@cache
def _svg_backend() -> None:
    schemdraw.use("svg")
    schemdraw.svgconfig.text = "path"


def _captured(drawing: object, /) -> Result[bytes, SchematicFault]:
    try:
        return Ok(drawing.get_imagedata("svg"))
    except ValueError as bad:
        return Error(SchematicFault(render=str(bad)))


def _lone(owner: type, data: dict[str, object], /) -> Result[bytes, SchematicFault]:
    try:
        with schemdraw.Drawing(show=False) as drawing:
            drawing += owner(**data)
    except (TypeError, ValueError) as bad:
        return Error(SchematicFault(parse=str(bad)))
    return _captured(drawing)


# --- [EXPORTS] --------------------------------------------------------------------------
__all__ = [
    "PinRow",
    "Schematic",
    "SchematicFault",
    "SchematicSpec",
    "SymbolRow",
]
```

## [03]-[RESEARCH]

<!-- source-only: research row template; every landed row opens on the list dash this placeholder omits, the census reading `^- [TOKEN]-[OPEN|BLOCKED]:` alone:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)

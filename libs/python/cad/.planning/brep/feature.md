# [PY_CAD_FEATURE]

`featured` applies one edge feature to a decoded body: it admits an `EdgeSelection` against the body's own edge map, charges every proven edge at one magnitude, and asks the shared verdict. This owner holds the sub-topology index regime — where a wire ordinal is zero-based, where OCCT's map is one-based, and how far either survives — so no sibling re-derives it.

Edges resolve through the body the fold already decoded at `brep/operation#OPERATION`, and every verdict through `built` at `brep/placement#ADMISSION`; refusal rows come from `faults#ROWS`. `EdgeSelection`, `EdgeIndices`, `FilletOp`, and `ChamferOp` are the frozen wire names, and the proto itself states that ordinals do not survive resealing — the durable-name gap that follows is modelled at `brep/provenance#READING`.

## [01]-[INDEX]

- [02]-[SELECTION]: edge map, the two index regimes, and the admission that makes an out-of-range ordinal unspellable.
- [03]-[FEATURE]: fillet and chamfer rows, the shared charge-and-build spine, and the diagnostic each row can report.
- [04]-[RESEARCH]: open questions.

## [02]-[SELECTION]

- Owner: `selected` — one admission from an `EdgeSelection` to edges already proven against the map that indexes them.
- Law: ordinals are zero-based `TopExp` order inside the exact decoded source artifact and one-based inside `TopTools_IndexedMapOfShape`, so one shift site joins the two regimes.
- Law: ordinals do not survive resealing, which the wire states plainly; this owner never matches geometry to pretend otherwise, and never re-derives an ordinal from a body it did not decode.
- Law: each requested ordinal proves itself against the map extent before any edge is read, so a refusal names the offending ordinal and the extent it exceeded rather than collapsing to one coordinate.
- Law: proving before reading is what makes the invalid state unrepresentable — the admitted block is already inside the extent, so the cast site holds no second guard.
- Law: `EdgeIndices.values` is `uint32` with unique items, so the wire closes the lower bound and duplicate charging; only the upper bound is this owner's to hold.
- Law: `all` reads the whole map, so a selection over a body whose edge count the caller never saw stays total.
- Boundary: the map is built per call and never cached — a cached map outlives the body it indexed and hands a later call ordinals into a shape it never saw.

```python signature
from collections.abc import Callable
from functools import partial
from typing import Final, Protocol, assert_never

from OCP.BRepFilletAPI import BRepFilletAPI_MakeChamfer, BRepFilletAPI_MakeFillet
from OCP.TopAbs import TopAbs_EDGE
from OCP.TopExp import TopExp
from OCP.TopTools import TopTools_IndexedMapOfShape
from OCP.TopoDS import TopoDS, TopoDS_Edge, TopoDS_Shape
from builtins import frozendict
from expression import Error, Ok
from expression.collections import Block
from expression.extra.result import traverse
from protobuf import Oneof
from protobuf.wkt import Empty
from rasm.contracts.rasm.contracts.cad.operations_pb import EdgeIndices, EdgeSelection

from rasm.cad.brep.placement import ShapeBuilder, built
from rasm.cad.faults import BREP_INPUT, BREP_KERNEL, CadFault, CadRail

# --- [OPERATIONS] -----------------------------------------------------------------------


def mapping(shape: TopoDS_Shape, /) -> TopTools_IndexedMapOfShape:
    carrier = TopTools_IndexedMapOfShape()
    TopExp.MapShapes_s(shape, TopAbs_EDGE, carrier)
    return carrier


def _within(extent: int, ordinal: int, /) -> CadRail[int]:
    # `uint32` on the wire closes the lower half already, so the upper bound is the whole question this owner asks.
    return Ok(ordinal) if ordinal < extent else Error(BREP_INPUT.at(f"edge.ordinal:{ordinal}/{extent}"))


def _ordinals(selection: EdgeSelection, extent: int, /) -> CadRail[Block[int]]:
    match selection.selection:
        case Oneof(field="all", value=Empty()):
            return Ok(Block.of_seq(range(extent)))
        case Oneof(field="indices", value=EdgeIndices() as chosen):
            return traverse(partial(_within, extent), Block.of_seq(chosen.values))
        case _ as unreachable:
            assert_never(unreachable)


def selected(shape: TopoDS_Shape, selection: EdgeSelection, /) -> CadRail[Block[TopoDS_Edge]]:
    mapped = mapping(shape)
    # Every ordinal in the admitted block is already inside the extent, so `FindKey` cannot raise and the downcast
    # cannot meet a non-edge key: an edge-only map holds nothing else.
    return _ordinals(selection, mapped.Extent()).map(
        lambda kept: kept.map(lambda ordinal: TopoDS.Edge_s(mapped.FindKey(ordinal + 1)))
    )
```

## [03]-[FEATURE]

- Owner: `featured` — one spine charging every selected edge at one magnitude, then asking `built` for the verdict.
- Cases: `_FEATURES` rows `fillet` and `chamfer`, each minting its builder together with the diagnostic that builder can answer.
- Law: one row mints both halves, so a builder whose failure surface differs never grows a second table keyed on the same field.
- Law: the diagnostic is a thunk closed over the concrete builder, which is what keeps the fillet contour census typed at its own site and priced only on the refusing arm.
- Law: only the kernel half of the verdict is re-spelled — a null or invalid result stays the output refusal `built` graded, because a faulty-contour census describes an unbuildable request rather than a bad body.
- Law: chamfer carries no contour census, so its row says so rather than reporting an empty one that reads like a measurement.
- Law: magnitude arrives already extracted because `FilletOp.radius_m` and `ChamferOp.distance_m` differ only in name, and the fold row that knows the payload type does the reading.
- Growth: a new edge feature is one `_FEATURES` row beside one row at `brep/operation#ARMS`.
- Boundary: variable radius, face-scoped selection, and shelling have no wire arm today, so this owner refuses to invent one and carries the gap as research.

```python signature
# --- [TYPES] ----------------------------------------------------------------------------

type Charged = tuple["EdgeFeature", Callable[[], str]]


# --- [SERVICES] -------------------------------------------------------------------------


class EdgeFeature(ShapeBuilder, Protocol):
    def Add(self, magnitude: float, edge: TopoDS_Edge, /) -> None: ...


# --- [OPERATIONS] -----------------------------------------------------------------------


def _contours(builder: BRepFilletAPI_MakeFillet, /) -> str:
    # `StripeStatus` is keyed on the contour, not the loop index, so the faulty contour is read twice by design.
    stripes = ",".join(
        f"{builder.FaultyContour(index)}:{int(builder.StripeStatus(builder.FaultyContour(index)))}"
        for index in range(1, builder.NbFaultyContours() + 1)
    )
    return f"fillet.contours={stripes};vertices={builder.NbFaultyVertices()}"


def _fillet(shape: TopoDS_Shape, /) -> Charged:
    builder = BRepFilletAPI_MakeFillet(shape)
    return builder, lambda: _contours(builder)


def _chamfer(shape: TopoDS_Shape, /) -> Charged:
    return BRepFilletAPI_MakeChamfer(shape), lambda: "chamfer.edges"


def _charged(builder: EdgeFeature, edges: Block[TopoDS_Edge], magnitude: float, /) -> EdgeFeature:
    for edge in edges:
        builder.Add(magnitude, edge)
    return builder


def _graded(diagnosed: Callable[[], str], fault: CadFault, /) -> CadFault:
    # Row identity is the test: `BREP_KERNEL` is one frozen instance, so an unfinished builder is distinguished from
    # a finished builder's invalid body without re-reading either probe.
    return BREP_INPUT.at(diagnosed()) if fault.row is BREP_KERNEL else fault


def featured(field: str, shape: TopoDS_Shape, selection: EdgeSelection, magnitude: float, /) -> CadRail[TopoDS_Shape]:
    builder, diagnosed = _FEATURES[field](shape)
    return selected(shape, selection).bind(
        lambda edges: built(_charged(builder, edges, magnitude), field).map_error(partial(_graded, diagnosed))
    )


# --- [FEATURES] -------------------------------------------------------------------------

# One row per wire field, each minting its builder and its diagnostic together. `brep/operation#ARMS` reaches these
# keys by literal, so an unknown field is unspellable rather than a lookup this owner guards.
_FEATURES: Final[frozendict[str, Callable[[TopoDS_Shape], Charged]]] = frozendict({"fillet": _fillet, "chamfer": _chamfer})
```

## [04]-[RESEARCH]

- [VARIABLE_RADIUS]-[OPEN]: does a variable-radius fillet earn a wire arm through the `BRepFilletAPI_MakeFillet.Add(r1, r2, edge)` overload, and what request shape carries the two radii; verify the overload against the folder `.api` catalogue and price the proto change.
- [FACE_SELECTION]-[OPEN]: does `cad` need a `FaceSelection` beside `EdgeSelection` for face-scoped features and shelling; verify against `libs/contracts/proto/rasm/contracts/cad/operations.proto` and the wire-contract law.

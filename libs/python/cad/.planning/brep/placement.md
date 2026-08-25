# [PY_CAD_PLACEMENT]


`Frame3` arrives orthogonality-proved and `UnitDirection3` magnitude-proved by protovalidate, so lowering is total and re-validation never runs; refusal opens at the kernel builders alone, on `BREP_INPUT` and `BREP_KERNEL` from `faults#ROWS`, and this page owns the builder-admission rail the whole B-rep leg composes. One `Lowering` correspondence declares each spatial owner as an ordered component read spread onto its `gp` constructor, and one span algebra serves the 3-D `CurveSegment3` family here and the 2-D `ProfileKnot` family at `profile#LOOPS`, parameterized by the point lift alone.

## [01]-[INDEX]

- [02]-[ADMISSION]: one builder verdict for the whole sub-domain, its split grading, and the shape-only arm every non-builder producer reaches.
- [03]-[PLACEMENT]: one correspondence from each spatial owner onto its `gp` value, the once-lowered planar basis, and the rigid transform.
- [04]-[SPANS]: closed edge-kind vocabulary, one lift-parameterized span algebra over both wire families, and the 3-D curve instantiation.

## [02]-[ADMISSION]

- Owner: `built` — one fold over an OCCT builder's `Build`, its `IsDone` verdict, and its shape half, so no arm in the sub-domain re-spells that ladder.
- Law: builder admission seats on this page because `brep/operation#ARMS` imports every arm page, so the same rail seated at that apex forces each arm to import back into it and closes a cycle no seating resolves.
- Law: the halves grade apart — an unfinished builder refuses on `BREP_KERNEL` because the kernel declined the construction, and a null or invalid result refuses on `BREP_OUTPUT` because the kernel returned a body no consumer can measure.
- Law: `admitted` is that shape half alone, reached by producers answering no `Build` — sewing, lofting, offset areas, and the pipe shell each hold a finished shape before any verdict exists to read.
- Law: `admitted` stays generic in the concrete topology type, so a face admits as a face and no caller re-downcasts a `TopoDS_Shape` it had already narrowed before the probe.
- Law: `_edged` admits the typed-accessor edge builders, which answer `Edge` and never `Build`, so the accessor is the discriminant that keeps three admission arms rather than one erased helper.
- Law: a caller wanting a different refusal row re-rows through `map_error` on the returned rail, which keeps the validity probe single-sited while the row stays the arm's own decision.
- Law: this rail is B-rep-leg scoped, so `metrology/properties` keeps its own validity probe under `MEASURE_DEGENERATE`; that leg seats below this page and cannot reach here, and the duplication is deliberate.
- Growth: a new builder family is one admission arm keyed on its accessor, never a second `BRepCheck_Analyzer` call standing beside the probe this cluster owns.
- Boundary: source resolution stays at `exchange/step#CODEC`, the arm roster at `brep/operation#ARMS`, and the seal handoff at `brep/operation#SPINE`; this owner grades one builder's verdict and nothing about the request that reached it.

```python
# --- [SERVICES] -------------------------------------------------------------------------


class ShapeBuilder(Protocol):
    def Build(self) -> None: ...
    def IsDone(self) -> bool: ...
    def Shape(self) -> TopoDS_Shape: ...


# --- [OPERATIONS] -----------------------------------------------------------------------


def admitted[S: TopoDS_Shape](shape: S, coordinate: str, /) -> CadRail[S]:
    return Ok(shape) if not shape.IsNull() and BRepCheck_Analyzer(shape).IsValid() else Error(BREP_OUTPUT.at(coordinate))


def built(builder: ShapeBuilder, coordinate: str, /) -> CadRail[TopoDS_Shape]:
    builder.Build()
    return Error(BREP_KERNEL.at(coordinate)) if not builder.IsDone() else admitted(builder.Shape(), coordinate)


def _edged(builder: BRepBuilderAPI_MakeEdge, coordinate: str, /) -> CadRail[TopoDS_Edge]:
    return Ok(builder.Edge()) if builder.IsDone() else Error(BREP_KERNEL.at(coordinate))
```

## [03]-[PLACEMENT]

- Owner: `Lowering` — one row per spatial owner, reading an ordered component run and spreading it onto its own `gp` constructor.
- Cases: `point`, `direction`, `axis`, `seated`, `frame`, and `datum` are the whole roster, and every composite row reads its leaves through the leaf rows instead of re-spelling a coordinate.
- Law: `axis` and `seated` share ONE component read and differ only in the constructor they spread onto, which is the correspondence paying for itself — a second `Axis3` converter never forms beside the first.
- Law: protovalidate proves coordinate finiteness, direction magnitude, and frame orthogonality on the wire, so every arm in this cluster is total and returns no rail.
- Law: `frame` lowers `Frame3` through `gp_Ax2`, whose validated Z and X axes fix one exact local basis and whose Y axis derives as Z×X, matching the rule the wire states for local coordinates.
- Law: a wire-carried Y axis is the deleted form, because a third transmitted axis can disagree with the cross product the kernel derives and no consumer can tell which one is authoritative.
- Law: `Basis` lowers a frame once into an origin and two planar directions, so a loop of many knots pays one `gp_Ax2` construction; the old planar lift rebuilt that frame on every knot of every loop.
- Law: `displaced` composes the rigid frame-to-frame displacement with the uniform scale seated on the target origin, which is the mapping the wire states only when displacement applies first.
- Growth: a new spatial owner is one `Lowering` row naming its component read and its `gp` constructor; a second target for an existing owner is one more row over the same read.
- Boundary: `placed` is the one arm in this cluster that touches topology, composing `built` at `[02]-[ADMISSION]`; every other arm mints geometry and reads no verdict.

```python
from collections.abc import Callable, Iterable, Sequence
from dataclasses import dataclass
from enum import StrEnum
from functools import partial
from typing import Final, Protocol, assert_never

from OCP.BRepBuilderAPI import BRepBuilderAPI_MakeEdge, BRepBuilderAPI_MakeWire, BRepBuilderAPI_Transform
from OCP.BRepCheck import BRepCheck_Analyzer
from OCP.GC import GC_MakeArcOfCircle
from OCP.GeomAPI import GeomAPI_Interpolate
from OCP.Precision import Precision
from OCP.TColgp import TColgp_HArray1OfPnt
from OCP.TopoDS import TopoDS_Edge, TopoDS_Shape, TopoDS_Wire
from OCP.gp import gp_Ax1, gp_Ax2, gp_Ax3, gp_Dir, gp_Pnt, gp_Trsf
from builtins import frozendict
from expression import Error, Ok
from expression.collections import Block
from expression.extra.result import sequence, traverse
from protobuf import Message, Oneof
# Contracts are retired from this logic.

from rasm.cad.faults import BREP_INPUT, BREP_KERNEL, BREP_OUTPUT, CadRail

# --- [MODELS] ---------------------------------------------------------------------------


@dataclass(frozen=True, slots=True)
class Lowering[W, G, *Cs]:
    read: Callable[[W], tuple[*Cs]]
    mint: Callable[[*Cs], G]

    def __call__(self, value: W, /) -> G:
        return self.mint(*self.read(value))


@dataclass(frozen=True, slots=True)
class Basis:
    origin: gp_Pnt
    x_axis: gp_Dir
    y_axis: gp_Dir

    @staticmethod
    def of(value: Frame3, /) -> "Basis":
        axes = frame(value)
        return Basis(origin=axes.Location(), x_axis=axes.XDirection(), y_axis=axes.YDirection())

    def at(self, x_m: float, y_m: float, /) -> gp_Pnt:
        return gp_Pnt(
            self.origin.X() + x_m * self.x_axis.X() + y_m * self.y_axis.X(),
            self.origin.Y() + x_m * self.x_axis.Y() + y_m * self.y_axis.Y(),
            self.origin.Z() + x_m * self.x_axis.Z() + y_m * self.y_axis.Z(),
        )


# --- [ROWS] -----------------------------------------------------------------------------

point: Final[Lowering[Point3, gp_Pnt, float, float, float]] = Lowering(
    read=lambda value: (value.x_m, value.y_m, value.z_m), mint=gp_Pnt
)
direction: Final[Lowering[UnitDirection3, gp_Dir, float, float, float]] = Lowering(
    read=lambda value: (value.x, value.y, value.z), mint=gp_Dir
)

_SEATING: Final[Callable[[Axis3], tuple[gp_Pnt, gp_Dir]]] = lambda value: (point(value.origin), direction(value.direction))

axis: Final[Lowering[Axis3, gp_Ax1, gp_Pnt, gp_Dir]] = Lowering(read=_SEATING, mint=gp_Ax1)
seated: Final[Lowering[Axis3, gp_Ax2, gp_Pnt, gp_Dir]] = Lowering(read=_SEATING, mint=gp_Ax2)
frame: Final[Lowering[Frame3, gp_Ax2, gp_Pnt, gp_Dir, gp_Dir]] = Lowering(
    read=lambda value: (point(value.origin), direction(value.z_axis), direction(value.x_axis)), mint=gp_Ax2
)
datum: Final[Lowering[Frame3, gp_Ax3, gp_Ax2]] = Lowering(read=lambda value: (frame(value),), mint=gp_Ax3)


# --- [OPERATIONS] -----------------------------------------------------------------------


def displaced(op: TransformOp, /) -> gp_Trsf:
    rigid = gp_Trsf()
    rigid.SetDisplacement(datum(op.source_frame), datum(op.target_frame))
    scale = gp_Trsf()
    scale.SetScale(point(op.target_frame.origin), op.uniform_scale)
    return scale.Multiplied(rigid)


def placed(shape: TopoDS_Shape, op: TransformOp, /) -> CadRail[TopoDS_Shape]:
    return built(BRepBuilderAPI_Transform(shape, displaced(op), True, False), "transform")
```

## [04]-[SPANS]

- Owner: `EdgeKind` — the closed edge vocabulary whose members spell the `line`/`arc`/`spline` oneof field name both span families already carry, so a wire token admits directly and no field-to-kind table sits between them.
- Cases: `RING` is the fourth member, reached only by the closed periodic loop at `profile#LOOPS` and never by an open span, which is why the vocabulary is wider than either dispatch's reachable subset.
- Law: one span algebra carries both wire families — `Span` pairs a kind with the oneof payload and the endpoints its family already derived, and the point type enters through `Lift` alone, so the 2-D and 3-D dispatches are one declaration rather than two hand-written ladders over parallel case classes.
- Law: `_interior` reads the `through` member both span families spell identically; the read is structural over the oneof payload and its totality is the oneof's own closed case set, which is what buys the single dispatch.
- Law: `_EDGE` is total over `EdgeKind` by construction and the interpolator's periodic flag rides the row rather than a caller-passed knob, so no arm reconstructs from a boolean what the kind already answers.
- Law: span builders admit through `_edged` at `[02]-[ADMISSION]`, so the edge family's typed accessor and the `ShapeBuilder` verdict stay one owner's decision rather than two probes drifting apart.
- Exemption: `EdgeKind(field)` under `except ValueError` is the admission seam for a raw oneof token, and the OCCT builder accumulation loops are the platform-forced statement sites; every expression past them is total over admitted members.
- Entry: `curve` pairs every segment endpoint with its successor's start, so continuity is structural, no join coordinate is recomputed, and the open spine the sweep rides carries one coordinate per join.
- Growth: a new span shape is one `EdgeKind` member, one `_EDGE` row, and one `_interior` arm, with both wire families reaching it and neither call site edited.
- Boundary: edges and wires mint here; face construction, hole repair, and offset areas belong to `profile#REGIONS`, and sub-topology selection to `brep/feature#SELECTION`.

```python
# --- [TYPES] ----------------------------------------------------------------------------

type Lift[P] = Callable[[P], gp_Pnt]


class EdgeKind(StrEnum):
    LINE = "line"
    ARC = "arc"
    SPLINE = "spline"
    RING = "ring"


# --- [MODELS] ---------------------------------------------------------------------------


@dataclass(frozen=True, slots=True)
class Span[P]:
    kind: EdgeKind
    case: Message
    start: P
    end: P


# --- [OPERATIONS] -----------------------------------------------------------------------


def _arced(run: Sequence[gp_Pnt], /) -> CadRail[TopoDS_Edge]:
    held = GC_MakeArcOfCircle(run[0], run[1], run[2])
    return _edged(BRepBuilderAPI_MakeEdge(held.Value()), "span.arc") if held.IsDone() else Error(BREP_INPUT.at("span.arc"))


def _fitted(run: Sequence[gp_Pnt], /, *, periodic: bool) -> CadRail[TopoDS_Edge]:
    values = TColgp_HArray1OfPnt(1, len(run))
    for index, held in enumerate(run, 1):
        values.SetValue(index, held)
    fitter = GeomAPI_Interpolate(values, periodic, Precision.Confusion_s())
    fitter.Perform()
    if not fitter.IsDone():
        return Error(BREP_INPUT.at("span.spline"))
    return _edged(BRepBuilderAPI_MakeEdge(fitter.Curve()), "span.spline.edge")


_EDGE: Final[frozendict[EdgeKind, Callable[[Sequence[gp_Pnt]], CadRail[TopoDS_Edge]]]] = frozendict({
    EdgeKind.LINE: lambda run: _edged(BRepBuilderAPI_MakeEdge(run[0], run[-1]), "span.line"),
    EdgeKind.ARC: _arced,
    EdgeKind.SPLINE: partial(_fitted, periodic=False),
    EdgeKind.RING: partial(_fitted, periodic=True),
})


def minted(kind: EdgeKind, run: Sequence[gp_Pnt], /) -> CadRail[TopoDS_Edge]:
    return _EDGE[kind](run)


def joined(edges: Iterable[TopoDS_Edge], /) -> CadRail[TopoDS_Wire]:
    builder = BRepBuilderAPI_MakeWire()
    for edge in edges:
        builder.Add(edge)
        if not builder.IsDone():
            return Error(BREP_INPUT.at(f"wire.join:{int(builder.Error())}"))
    return Ok(builder.Wire())


def _interior[P](kind: EdgeKind, case: Message, /) -> tuple[P, ...]:
    match kind:
        case EdgeKind.LINE:
            return ()
        case EdgeKind.ARC:
            return (case.through,)
        case EdgeKind.SPLINE | EdgeKind.RING:
            return tuple(case.through)
        case _ as unreachable:
            assert_never(unreachable)


def _kind(field: str, /) -> CadRail[EdgeKind]:
    try:
        return Ok(EdgeKind(field))
    except ValueError:
        return Error(BREP_INPUT.at(f"span.kind:{field}"))


def spanning[P](held: Oneof, start: P, end: P, /) -> CadRail[Span[P]]:
    return _kind(held.field).map(lambda kind: Span(kind=kind, case=held.value, start=start, end=end))


def spanned[P](span: Span[P], lift: Lift[P], /) -> CadRail[TopoDS_Edge]:
    return minted(span.kind, tuple(map(lift, (span.start, *_interior[P](span.kind, span.case), span.end))))


def wired[P](spans: Block[Span[P]], lift: Lift[P], /) -> CadRail[TopoDS_Wire]:
    return traverse(lambda span: spanned(span, lift), spans).bind(joined)


def _paired(value: Curve3, /) -> Block[tuple[Oneof, Point3, Point3]]:
    ends = tuple(one.curve.value.end for one in value.segments)
    return Block.of_seq(zip((one.curve for one in value.segments), (value.start, *ends), ends, strict=True))


def curve(value: Curve3, /) -> CadRail[TopoDS_Wire]:
    return sequence(_paired(value).starmap(spanning)).bind(lambda spans: wired(spans, point))
```

## [05]-[RESEARCH]

(none)

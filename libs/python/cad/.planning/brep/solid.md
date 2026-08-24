# [PY_CAD_SOLID]

`solid` owns every B-rep arm that mints a fresh body from parameters and a frame, reading no source artifact: the analytic primitives, the generative sweeps over a placed profile, and the identity-guarded set folds those sweeps reduce through. Arms opening a `SealedStep` operand belong to `brep/boolean` and `brep/feature`; this owner never resolves a source and never seals a result.

Spatial seats lower through `placement#PLACEMENT` and every profile enters as the oriented face run `profile#OFFSET` hands over, so no geometry is re-derived here. Builder admission composes `built` and `admitted` at `placement#ADMISSION` and set algebra composes `nary` over the `BOOLEANS` roster at `brep/boolean#BOOLEAN`, keeping one admission owner and one operator roster for the whole sub-domain. Refusals ride `CadRail` on `BREP_INPUT` here and on `BREP_OUTPUT` through the admission rail's own grading.

## [01]-[INDEX]

- [02]-[PRIMITIVES]: one row per analytic primitive carrying its constructor, its placement read, and its extent read.
- [03]-[FOLD]: identity law over an operand run, and the two guarded reducers every generative arm collapses through.
- [04]-[GENERATIVE]: one spine from a placed profile to one solid, with extrude, revolve, thick, and sweep as per-face arm values.
- [05]-[LOFT]: track lofting under a tabled style column, index-corresponding hole tracks, and the shell each track carves.

## [02]-[PRIMITIVES]

- Owner: `PRIMITIVE` — one row per analytic primitive carrying its OCCT constructor, its placement read, and its extent read, so the five arms that differed only by a class literal become five rows over one body.
- Law: `PRIMITIVE` is the roster the arm fold reads, exactly as `BOOLEANS` at `brep/boolean#BOOLEAN` is the roster its five wire fields read; an arm table restating either roster is the duplicated form, because a new operator then edits two sites.
- Law: `seated` at `placement#PLACEMENT` supplies the axis-seated placement cylinder, cone, and torus share, so the origin-and-direction read is spelled once for the whole family rather than restated per row.
- Law: a full sphere carries no observable rotation, so `_UP` seats the frame the wire declines to spell rather than standing in as a default; an angle-bounded sphere makes that seat observable and rides the open research row.
- Law: `primitive` refuses an unrostered field instead of indexing the roster, so the arm fold routes on membership it never has to prove a second time and no unproved lookup crosses the worker seam.
- Growth: a new primitive is one `PRIMITIVE` row beside one wire case; the arm fold reads the roster and grows no body of its own.
- Boundary: `built` composes from `placement#ADMISSION`, so this owner mints no second builder-admission helper and no second validity probe beside it.

```python signature
from collections.abc import Callable, Sequence
from dataclasses import dataclass
from functools import partial
from typing import Final

from OCP.BRepBuilderAPI import BRepBuilderAPI_PipeDone, BRepBuilderAPI_Transformed
from OCP.BRepOffsetAPI import BRepOffsetAPI_MakePipeShell, BRepOffsetAPI_ThruSections
from OCP.BRepPrimAPI import (
    BRepPrimAPI_MakeBox,
    BRepPrimAPI_MakeCone,
    BRepPrimAPI_MakeCylinder,
    BRepPrimAPI_MakePrism,
    BRepPrimAPI_MakeRevol,
    BRepPrimAPI_MakeSphere,
    BRepPrimAPI_MakeTorus,
)
from OCP.BRepTools import BRepTools
from OCP.Precision import Precision
from OCP.TopAbs import TopAbs_WIRE
from OCP.TopExp import TopExp
from OCP.TopoDS import TopoDS, TopoDS_Face, TopoDS_Shape, TopoDS_Wire
from OCP.TopTools import TopTools_IndexedMapOfShape
from OCP.gp import gp_Ax2, gp_Dir, gp_Vec
from builtins import frozendict
from expression import Error, Ok, Option
from expression.collections import Block
from expression.extra.result import traverse
from protobuf import Message, Oneof
from rasm.contracts.rasm.contracts.cad.operations_pb import (
    ExtrudeOp,
    LoftOp,
    LoftStyle,
    LoftTrack,
    PlacedProfile,
    RevolveOp,
    SweepOp,
    ThickOp,
)
from rasm.contracts.rasm.contracts.cad.types_pb import ProfileLoop, ProfileRegion
from rasm.contracts.rasm.contracts.spatial.vector_pb import UnitDirection3

from rasm.cad.brep.boolean import BOOLEANS, nary
from rasm.cad.brep.placement import Basis, ShapeBuilder, admitted, axis, built, curve, direction, frame, point, seated
from rasm.cad.brep.profile import faces, offset, wire
from rasm.cad.faults import BREP_INPUT, CadRail

# --- [MODELS] ---------------------------------------------------------------------------


@dataclass(frozen=True, slots=True, kw_only=True)
class PrimitiveRow:
    # Every analytic primitive spells one shape: a placement seat followed by an ordered extent run. `mint` holds the
    # OCCT class, so the arms that differed only by that literal become rows over one body.
    mint: Callable[..., ShapeBuilder]
    seat: Callable[[Message], gp_Ax2]
    extents: Callable[[Message], tuple[float, ...]]


# --- [ROWS] -----------------------------------------------------------------------------

# Full spheres carry no observable rotation, so the world-up seat below is the frame the wire declines to spell
# rather than a default standing in for one; an angle-bounded sphere makes that seat observable.
_UP: Final[gp_Dir] = gp_Dir(0.0, 0.0, 1.0)

PRIMITIVE: Final[frozendict[str, PrimitiveRow]] = frozendict({
    "box": PrimitiveRow(
        mint=BRepPrimAPI_MakeBox, seat=lambda op: frame(op.frame), extents=lambda op: (op.x_m, op.y_m, op.z_m)
    ),
    "sphere": PrimitiveRow(
        mint=BRepPrimAPI_MakeSphere, seat=lambda op: gp_Ax2(point(op.center), _UP), extents=lambda op: (op.radius_m,)
    ),
    "cylinder": PrimitiveRow(
        mint=BRepPrimAPI_MakeCylinder, seat=lambda op: seated(op.axis), extents=lambda op: (op.radius_m, op.height_m)
    ),
    "cone": PrimitiveRow(
        mint=BRepPrimAPI_MakeCone,
        seat=lambda op: seated(op.axis),
        extents=lambda op: (op.base_radius_m, op.top_radius_m, op.height_m),
    ),
    "torus": PrimitiveRow(
        mint=BRepPrimAPI_MakeTorus,
        seat=lambda op: seated(op.axis),
        extents=lambda op: (op.major_radius_m, op.minor_radius_m),
    ),
})


# --- [OPERATIONS] -----------------------------------------------------------------------


def primitive(held: Oneof, /) -> CadRail[TopoDS_Shape]:
    return (
        Option.of_optional(PRIMITIVE.get(held.field))
        .to_result_with(lambda: BREP_INPUT.at(f"primitive.kind:{held.field}"))
        .bind(lambda row: built(row.mint(row.seat(held.value), *row.extents(held.value)), f"primitive.{held.field}"))
    )
```

## [03]-[FOLD]

- Owner: `merged` and `carved` — the identity law over an operand run, seated here because every generative arm needs it and the set operators at `brep/boolean#BOOLEAN` state the kernel call alone.
- Law: a lone operand is the fixpoint of the union and an empty tool run the fixpoint of the difference, so the kernel never runs where nothing combines and a hole-free face takes the same path as a hole-bearing one.
- Law: the empty union refuses at `BREP_INPUT` even though every wire `min_items` rule makes it unreachable from a present call site, because totality belongs to the owner and never to the proofs its callers happen to hold.
- Law: `merged` reads `BOOLEANS["fuse"]` rather than spelling a `BRepAlgoAPI` literal, so the operator roster stays single-sited and a sixth operator lands at its owner alone.
- Law: uniting abutting regions is the admitted form over compounding them, because a compound leaves coincident faces standing and the topology census then reports a face count no manifold body carries at all.
- Boundary: fuzzy tolerance, parallel custody, and correspondence capture stay at `brep/boolean#BOOLEAN`; this owner elects operands and reads no history.

```python signature
def merged(shapes: Sequence[TopoDS_Shape], /) -> CadRail[TopoDS_Shape]:
    match shapes:
        case ():
            return Error(BREP_INPUT.at("unite.empty"))
        case (lone,):
            return Ok(lone)
        case _:
            return nary(shapes[:1], shapes[1:], BOOLEANS["fuse"], "unite")


def carved(body: TopoDS_Shape, tools: Sequence[TopoDS_Shape], coordinate: str, /) -> CadRail[TopoDS_Shape]:
    # Empty tool runs leave the body its own difference, so a hole-free region and a hole-bearing one reach the
    # same expression and no caller branches on whether its profile carried holes.
    return Ok(body) if not tools else nary((body,), tools, BOOLEANS["cut"], coordinate)
```

## [04]-[GENERATIVE]

- Owner: `generated` — one spine from a `PlacedProfile` to one solid: place the faces, run the per-face arm, fold the run through `merged`.
- Cases: `Extrusion` is that per-face arm, and extrude, revolve, thick, and sweep are four values of it rather than four bodies that each re-spelled place-then-extrude-then-unite.
- Law: STATED COLLAPSE — primitives and generative sweeps share one identity regime and one page, which LOSES the analytic-versus-generated distinction as a TYPE; after this fold nothing in the type system separates a box from a loft.
- Law: that discriminant stays recoverable from the value as the `ExecuteRequest.operation` field the arm fold routes on and as the refusal coordinate each arm stamps, so what the collapse costs is a receipt column and never an identity.
- Law: a negative thickness walls inward, so the offset sign alone elects which prism bounds the shell; the wire keeps `thickness_m` non-zero, which is what makes that election total without a third arm or a mode flag.
- Law: the pipe shell fixes corrected-Frenet transport, transformed transitions, no contact, and section correction, which the wire declares non-public kernel vocabulary rather than a knob a caller tunes.
- Law: a swept face carries its holes as bored pipe shells cut from the outer solid, so a hole survives the sweep instead of collapsing into the body an outer-wire-only sweep returns.
- Law: `IsDone` alone admits a pipe shell the status still grades as degenerate, so both gates hold before `MakeSolid` and the status ordinal rides the refusal coordinate.
- Boundary: source-artifact arms, the kernel receipt at `metrology/properties#RECEIPT`, and the seal handoff belong to `brep/operation#ARMS`; every arm here reads parameters and a frame alone.

```python signature
# --- [TYPES] ----------------------------------------------------------------------------

type Extrusion = Callable[[TopoDS_Face], CadRail[TopoDS_Shape]]


# --- [OPERATIONS] -----------------------------------------------------------------------


def generated(section: PlacedProfile, extrusion: Extrusion, /) -> CadRail[TopoDS_Shape]:
    # One spine for every generative arm: the placed face run, one arm per face, one fold. Four arms re-spelled this
    # body before the collapse, each differing only in the callable this parameter now carries.
    return (
        faces(section)
        .bind(lambda placed: traverse(extrusion, Block.of_seq(placed)))
        .bind(lambda solids: merged(tuple(solids)))
    )


def _prism(base: TopoDS_Face, heading: UnitDirection3, distance_m: float, /) -> CadRail[TopoDS_Shape]:
    reach = direction(heading)
    return built(
        BRepPrimAPI_MakePrism(base, gp_Vec(reach.X() * distance_m, reach.Y() * distance_m, reach.Z() * distance_m), True, True),
        "prism",
    )


def _piped(spine: TopoDS_Wire, section: TopoDS_Wire, /) -> CadRail[TopoDS_Shape]:
    builder = BRepOffsetAPI_MakePipeShell(spine)
    builder.SetMode(False)
    builder.SetTransitionMode(BRepBuilderAPI_Transformed)
    builder.Add(section, False, True)
    if not builder.IsReady():
        return Error(BREP_INPUT.at("sweep.not-ready"))
    builder.Build()
    # `IsDone` alone passes a shell the pipe status still grades as degenerate, so both gates hold before the solid.
    if not builder.IsDone() or builder.GetStatus() != BRepBuilderAPI_PipeDone:
        return Error(BREP_INPUT.at(f"sweep:{int(builder.GetStatus())}"))
    if not builder.MakeSolid():
        return Error(BREP_INPUT.at("sweep.open-profile"))
    # Pipe shells answer no further `Build` verdict past this point, so the shape half admits through the rail's
    # shape-only arm rather than a second `BRepCheck_Analyzer` call standing here.
    return admitted(builder.Shape(), "sweep.invalid")


def _pipe(spine: TopoDS_Wire, base: TopoDS_Face, /) -> CadRail[TopoDS_Shape]:
    outer = BRepTools.OuterWire_s(base)
    if outer.IsNull():
        return Error(BREP_INPUT.at("sweep.outer"))
    held = TopTools_IndexedMapOfShape()
    TopExp.MapShapes_s(base, TopAbs_WIRE, held)
    # Wire maps on a face are ONE-based and carry the outer loop among the holes, so identity against the outer
    # wire is the only partition; assuming which slot holds the boundary silently bores the body instead.
    holes = tuple(
        TopoDS.Wire_s(held.FindKey(index))
        for index in range(1, held.Extent() + 1)
        if not held.FindKey(index).IsSame(outer)
    )
    return _piped(spine, outer).bind(
        lambda body: traverse(partial(_piped, spine), Block.of_seq(holes)).bind(
            lambda bores: carved(body, tuple(bores), "sweep.holes")
        )
    )


def _wall(op: ThickOp, base: TopoDS_Face, /) -> CadRail[TopoDS_Shape]:
    source = _prism(base, op.direction, op.distance_m)
    shifted = (
        offset(base, op.thickness_m)
        .bind(lambda areas: traverse(lambda area: _prism(area, op.direction, op.distance_m), Block.of_seq(areas)))
        .bind(lambda parts: merged(tuple(parts)))
    )
    # Negative thickness walls inward, so the offset sign alone elects which prism bounds the shell, and the wire
    # keeps `thickness_m` non-zero, which is what makes that election total without a third arm.
    return source.map2(
        shifted, lambda inner, outer: (outer, inner) if op.thickness_m > 0.0 else (inner, outer)
    ).bind(lambda pair: carved(pair[0], (pair[1],), "thick.cut"))


def extruded(op: ExtrudeOp, /) -> CadRail[TopoDS_Shape]:
    return generated(op.section, lambda base: _prism(base, op.direction, op.distance_m))


def revolved(op: RevolveOp, /) -> CadRail[TopoDS_Shape]:
    return generated(
        op.section, lambda base: built(BRepPrimAPI_MakeRevol(base, axis(op.axis), op.angle_rad, True), "revolve")
    )


def thickened(op: ThickOp, /) -> CadRail[TopoDS_Shape]:
    return generated(op.section, partial(_wall, op))


def swept(op: SweepOp, /) -> CadRail[TopoDS_Shape]:
    return curve(op.spine).bind(lambda spine: generated(op.section, partial(_pipe, spine)))
```

## [05]-[LOFT]

- Owner: `lofted` — one `LoftOp` folded across its tracks, each track a shell carved by the solids its index-corresponding hole loops loft into.
- Law: `_RULED` carries the ruled column as data, collapsing the two-arm `match` whose only product was a bare `bool` the style value already reconstructs; a knob the value rebuilds is not a parameter.
- Law: `LoftStyle` arrives `defined_only` and non-zero on the wire, so the row read is total and the unspecified member never reaches this owner to need an arm.
- Law: the wire's hole-correspondence rule proves every section carries index-matching holes, so the hole roster reads off section zero and no arm re-counts per section.
- Law: outer and hole tracks run one loft body under one pick function, so the per-hole loop body is one value in a run rather than a second block standing beside the first.
- Law: a loft section carries a region and a frame and never an offset, so the placed-face run at `profile#OFFSET` is not this owner's path into geometry and the loops lower directly.
- Exemption: `BRepOffsetAPI_ThruSections` accumulates its wires by statement, the platform-forced seam every OCCT `Make*` owner carries.

```python signature
# --- [ROWS] -----------------------------------------------------------------------------

_RULED: Final[frozendict[LoftStyle, bool]] = frozendict({LoftStyle.RULED: True, LoftStyle.SMOOTH: False})


# --- [OPERATIONS] -----------------------------------------------------------------------


def _hole(index: int, region: ProfileRegion, /) -> ProfileLoop:
    return region.holes[index]


def _tracked(style: LoftStyle, wires: Block[TopoDS_Wire], /) -> CadRail[TopoDS_Shape]:
    builder = BRepOffsetAPI_ThruSections(True, _RULED[style], Precision.Confusion_s())
    for held in wires:
        builder.AddWire(held)
    builder.CheckCompatibility(True)
    builder.Build()
    if not builder.IsDone():
        return Error(BREP_INPUT.at(f"loft:{int(builder.GetStatus())}"))
    return admitted(builder.Shape(), "loft.invalid")


def _shell(track: LoftTrack, style: LoftStyle, pick: Callable[[ProfileRegion], ProfileLoop], /) -> CadRail[TopoDS_Shape]:
    return traverse(
        lambda section: wire(pick(section.region), Basis.of(section.frame)), Block.of_seq(track.sections)
    ).bind(lambda wires: _tracked(style, wires))


def _track(track: LoftTrack, style: LoftStyle, /) -> CadRail[TopoDS_Shape]:
    # Outer loop and holes loft through ONE body under a pick function; the wire's hole-correspondence rule proves
    # that count matches across sections, so section zero is the roster and no arm re-counts per section.
    picks: tuple[Callable[[ProfileRegion], ProfileLoop], ...] = (
        lambda region: region.outer,
        *(partial(_hole, index) for index in range(len(track.sections[0].region.holes))),
    )
    return (
        traverse(lambda pick: _shell(track, style, pick), Block.of_seq(picks))
        .map(tuple)
        .bind(lambda run: carved(run[0], run[1:], "loft.hole"))
    )


def lofted(op: LoftOp, /) -> CadRail[TopoDS_Shape]:
    return traverse(lambda track: _track(track, op.style), Block.of_seq(op.tracks)).bind(
        lambda solids: merged(tuple(solids))
    )
```

## [06]-[RESEARCH]

- [PRIMITIVE_ROSTER]-[OPEN]: does the arm fold read `PRIMITIVE` here the way it reads `BOOLEANS` at its owner, or does a second primitive table stand at `brep/operation#ARMS`; settle one roster before realization.
- [PARTIAL_PRIMITIVE]-[OPEN]: which angle-bounded `BRepPrimAPI_MakeSphere`/`MakeCylinder`/`MakeCone`/`MakeTorus` overloads exist, so a bounded primitive earns a request shape; census the installed rail and card the wire fields.
- [RECEIPT_OPERATION]-[OPEN]: does `BrepKernelReceipt` carry the minting operation, so an analytic primitive stays distinguishable from a generated sweep after the fold; route a wire card at the contracts corpus.
- [SOLID_MEMBERS]-[OPEN]: does the catalogue carry `BRepTools.OuterWire_s`, the `BRepPrimAPI_MakeBox(gp_Ax2, dx, dy, dz)` overload, `BRepOffsetAPI_MakePipeShell.SetMode`/`SetTransitionMode`/`MakeSolid`, and `Precision.Confusion_s`; census the installed rail.

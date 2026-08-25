# [PY_CAD_PROFILE]

`profile` owns the planar grammar of `rasm.contracts.cad`: loops become closed wires on a placed frame, regions become oriented faces carrying their holes, and an admitted offset rebuilds those faces as exact areas. Every generative arm at `solid#GENERATIVE` enters through this owner's face run, so profile closure, hole repair, and offset area validity are decided once here and never re-decided per solid.

Loop closure and span kind are structural on the wire, so this owner refuses only what the kernel refuses, returning `CadRail` over `BREP_INPUT` and `BREP_KERNEL`. Span construction composes `placement#SPANS` under the planar lift a once-lowered `Basis` supplies, which is what keeps the 2-D knot family and the 3-D segment family on one algebra. `PlacedProfile.offset` projects to `Option` at its single read and never travels inward as a null.

## [01]-[INDEX]

- [02]-[LOOPS]: closed loop vocabulary keyed on the wire's own oneof field, the planar lift, and the piecewise knot pairing.
- [03]-[REGIONS]: outer-and-holes face construction, boundary-orientation repair, and the validity gate every region crosses.
- [04]-[OFFSET]: exact face offset under a fixed join, area rebuild, and the placed face run every generative arm reads.

## [02]-[LOOPS]

- Owner: `wire` — one `ProfileLoop` lowered to one closed `TopoDS_Wire` on a placed basis.
- Cases: `_LOOP` carries the `piecewise` and `periodic_spline` rows; an unrostered key refuses rather than indexing an unproved token.
- Law: the piecewise row binds `placement#SPANS` with the planar lift, so the 2-D knot family runs the same algebra as the 3-D segment family.
- Law: each knot owns the span to its successor and the closing span returns to knot zero, so loop closure is structural on the wire and no arm re-checks it or repairs an open loop into a closed one.
- Law: the periodic row mints one `RING` edge and leaves the closing point implicit, matching the wire's distinct-tail rule; a repeated terminal point collapses the interpolation's last span to zero length.
- Law: `Basis` at `placement#PLACEMENT` lowers the frame once per call, so a region of many knots pays one `gp_Ax2` construction where the old planar lift paid one per point.
- Boundary: refusal here names the loop coordinate; geometric degeneracy stays OCCT's and surfaces through the span builders.

```python
from collections.abc import Callable, Sequence
from typing import Final

from OCP.BOPAlgo import BOPAlgo_BuilderFace
from OCP.BRepBuilderAPI import BRepBuilderAPI_MakeFace
from OCP.BRepOffsetAPI import BRepOffsetAPI_MakeOffset
from OCP.GeomAbs import GeomAbs_JoinType
from OCP.ShapeFix import ShapeFix_Face
from OCP.TopAbs import TopAbs_EDGE
from OCP.TopExp import TopExp_Explorer
from OCP.TopoDS import TopoDS, TopoDS_Face, TopoDS_Shape, TopoDS_Wire
from OCP.TopTools import TopTools_ListOfShape
from builtins import frozendict
from expression import Error, Ok, Option
from expression.collections import Block
from expression.extra.result import sequence, traverse
from protobuf import Message, Oneof
from rasm.contracts.rasm.contracts.cad.operations_pb import PlacedProfile
from rasm.contracts.rasm.contracts.cad.types_pb import PiecewiseLoop, Point2, ProfileLoop, ProfileRegion

from rasm.cad.brep.placement import Basis, EdgeKind, Lift, admitted, joined, minted, spanning, wired
from rasm.cad.faults import BREP_INPUT, BREP_KERNEL, CadRail

# --- [OPERATIONS] -----------------------------------------------------------------------


def _lifted(basis: Basis, /) -> Lift[Point2]:
    return lambda point: basis.at(point.x_m, point.y_m)


def _knots(loop: PiecewiseLoop, /) -> Block[tuple[Oneof, Point2, Point2]]:
    points = tuple(knot.point for knot in loop.knots)
    return Block.of_seq(
        (knot.outgoing, points[index], points[(index + 1) % len(points)]) for index, knot in enumerate(loop.knots)
    )


_LOOP: Final[frozendict[str, Callable[[Message, Lift[Point2]], CadRail[TopoDS_Wire]]]] = frozendict({
    "piecewise": lambda case, lift: sequence(_knots(case).starmap(spanning)).bind(lambda spans: wired(spans, lift)),
    "periodic_spline": lambda case, lift: minted(EdgeKind.RING, tuple(map(lift, case.through))).bind(
        lambda edge: joined((edge,))
    ),
})


def wire(loop: ProfileLoop, basis: Basis, /) -> CadRail[TopoDS_Wire]:
    return (
        Option.of_optional(_LOOP.get(loop.curve.field))
        .to_result_with(lambda: BREP_INPUT.at(f"loop.kind:{loop.curve.field}"))
        .bind(lambda row: row(loop.curve.value, _lifted(basis)))
    )
```

## [03]-[REGIONS]

- Owner: `face` — one `ProfileRegion` lowered to one oriented `TopoDS_Face` carrying its own holes.
- Law: `_faced` takes the outer loop and the hole run as separate arguments, so an empty wire set is unrepresentable at construction rather than guarded at every use.
- Law: the wire's required `ProfileRegion.outer` is that proof, and the deleted `profile.wires.empty` refusal loses no reachable state.
- Law: `ShapeFix_Face.FixOrientation` repairs boundary orientation alone, because a wider repair pass rewrites the geometry the kernel receipt attests and the mesh reconciliation then compares two different bodies.
- Law: the shape-half probe at `placement#ADMISSION` gates the repaired face, so an invalid region refuses here, never reaches a solid builder, and re-rows to `BREP_INPUT` because a bad region is caller material rather than a kernel output defect.
- Exemption: the face builder accumulates its holes by statement, the platform-forced seam every OCCT `Make*` owner carries.
- Boundary: offsetting a finished face and rebuilding its areas belong to `[04]-[OFFSET]`.

```python
def _faced(outer: TopoDS_Wire, holes: Sequence[TopoDS_Wire], /) -> CadRail[TopoDS_Face]:
    builder = BRepBuilderAPI_MakeFace(outer, True)
    if not builder.IsDone():
        return Error(BREP_INPUT.at("profile.outer"))
    for hole in holes:
        builder.Add(hole)
    builder.Build()
    if not builder.IsDone():
        return Error(BREP_KERNEL.at("profile.holes"))
    fixer = ShapeFix_Face(builder.Face())
    fixer.FixOrientation()
    return admitted(fixer.Face(), "profile.invalid").map_error(lambda _graded: BREP_INPUT.at("profile.invalid"))


def face(region: ProfileRegion, basis: Basis, /) -> CadRail[TopoDS_Face]:
    return (
        wire(region.outer, basis)
        .map2(
            traverse(lambda hole: wire(hole, basis), Block.of_seq(region.holes)),
            lambda outer, holes: (outer, tuple(holes)),
        )
        .bind(lambda pair: _faced(*pair))
    )
```

## [04]-[OFFSET]

- Owner: `faces` — one `PlacedProfile` lowered to the oriented face run every generative arm at `solid#GENERATIVE` consumes.
- Law: `PlacedProfile.offset` projects to `Option` at this one read, so absence never travels inward as a null or a magic default.
- Law: offsets run per oriented face under fixed `GeomAbs_Arc` with `SetApprox(False)`, keeping line, circle, and spline ownership exact.
- Law: `BOPAlgo_BuilderFace` rebuilds valid areas from the returned edges on the base face, so a self-crossing offset refuses at the area partition instead of yielding a torn face the volume fold then attests; an empty partition is its own coordinate because no area survived at all.
- Law: STATED LOSS — pinning the join at `GeomAbs_Arc` LOSES the mitred and the tangent-continuous offset, because `GeomAbs_Intersection` and `GeomAbs_Tangent` are the only alternates `GeomAbs_JoinType` carries and neither is total over the admitted curved profile: an intersection join has no meet for two offset arcs on a reflex corner and a tangent join has none where curvature reverses across the knot.
- Law: pinning the join also LOSES the polygon-sampled offset and the independent outer-versus-hole distance that the deleted cross-section owner carried as a public knob; exactness costs both, and the sampled form cannot return while `SetApprox(False)` keeps line, circle, and spline ownership intact.
- Law: `ProfileOffset` carries `distance_m` alone, so no join has a request shape, no arm reconstructs one from a literal, and the loss stays visible as an absent wire field instead of a silent default.
- Growth: a join axis lands as one `ProfileOffset` case and one row keyed on it, admitted only once totality is proved per join over the admitted loop corpus.
- Boundary: `edges` collects the offset result for the area rebuild alone; sub-topology selection for features belongs to `brep/feature#SELECTION`.

```python
def edges(shape: TopoDS_Shape, /) -> TopTools_ListOfShape:
    result = TopTools_ListOfShape()
    cursor = TopExp_Explorer(shape, TopAbs_EDGE)
    while cursor.More():
        result.Append(cursor.Current())
        cursor.Next()
    return result


def offset(base: TopoDS_Face, distance_m: float, /) -> CadRail[tuple[TopoDS_Face, ...]]:
    builder = BRepOffsetAPI_MakeOffset(base, GeomAbs_JoinType.GeomAbs_Arc, False)
    builder.SetApprox(False)
    builder.Perform(distance_m, 0.0)
    if not builder.IsDone():
        return Error(BREP_INPUT.at("profile.offset"))
    areas = BOPAlgo_BuilderFace()
    areas.SetFace(base)
    areas.SetShapes(edges(builder.Shape()))
    areas.Perform()
    if areas.HasErrors():
        return Error(BREP_INPUT.at("profile.offset.areas"))
    rebuilt = tuple(TopoDS.Face_s(shape) for shape in areas.Areas())
    if not rebuilt:
        return Error(BREP_INPUT.at("profile.offset.empty"))
    return (
        traverse(lambda area: admitted(area, "profile.offset.invalid"), Block.of_seq(rebuilt))
        .map(tuple)
        .map_error(lambda _graded: BREP_INPUT.at("profile.offset.invalid"))
    )


def _shifted(placed: Sequence[TopoDS_Face], distance_m: float, /) -> CadRail[tuple[TopoDS_Face, ...]]:
    return traverse(lambda base: offset(base, distance_m), Block.of_seq(placed)).map(
        lambda nested: tuple(area for areas in nested for area in areas)
    )


def faces(section: PlacedProfile, /) -> CadRail[tuple[TopoDS_Face, ...]]:
    basis = Basis.of(section.frame)
    held = Option.of_optional(section.offset)
    return traverse(lambda region: face(region, basis), Block.of_seq(section.profile.regions)).bind(
        lambda placed: held.map(lambda shift: _shifted(tuple(placed), shift.distance_m)).default_with(
            lambda: Ok(tuple(placed))
        )
    )
```

## [05]-[RESEARCH]

- [OFFSET_JOIN]-[OPEN]: which admitted profile families do `GeomAbs_Tangent` and `GeomAbs_Intersection` close over; drive each join across the line, arc, and spline loop corpus and gate on `BOPAlgo_BuilderFace` area validity.
- [OFFSET_SPLIT]-[OPEN]: does `BRepOffsetAPI_MakeOffset` accept independent outer and hole distances on one face, or does a split offset need one call per loop; probe the face and wire overloads.
- [FACE_MEMBERS]-[OPEN]: does the catalogue carry `BOPAlgo_BuilderFace.Perform`/`HasErrors` and `BRepBuilderAPI_MakeFace.Add`/`Build`/`Face`; census the installed `OCP` rail and land the rows.

# [PY_GEOMETRY_MESH_BREP]

Exact B-rep evaluation: parametric solid construction, robust n-ary Boolean algebra, profile offset/loft generation, feature operations, and watertight tessellation on one `BrepOp` union — five operation kinds, each carrying its inner verb as a closed `StrEnum` row bound to the owning OCCT `BRep*API` builder through an `expression.Map` dispatch table, so a new primitive, set verb, offset mode, or feature is one row plus one entry, never a new surface. The `Offset` arm integrates both kernels on one rail: the 2D profile offsets and simplifies in `manifold3d.CrossSection`, lifts through `BRepBuilderAPI_MakeFace.Add` as a holed planar face, then extrudes/revolves/lofts/thickens into a `TopoDS_Shape`. This owner writes no file — results cross the wire in memory as `TopoDS_Shape` plus optional `trimesh.Trimesh`.

`TessellationPolicy` arrives from `mesh/cad` (minted there, imported downward), and an evaluated solid graduates through the geometry-minted `GeometrySubject.MESH_ALGEBRA` rail. The CPU-bound kernel rides `LanePolicy.offload` — the PEP 734 subinterpreter hop — with the `@receipted(REDACTION)` egress streaming the typed receipt on the `Ok` path. Consumers compose downward: the CAD-STEP hop (`mesh/cad.md#BRIDGE`), the mesh-algebra rail (`mesh/repair.md#MESH`), and the `ifc/structural` profile owner composing `Construct`/`Offset` to evaluate an `IfcProfileDef` cross-section into a `TopoDS_Face` before its section integral.

## [01]-[INDEX]

- [01]-[BREP]: B-rep operation union over OCCT plus `manifold3d.CrossSection`, offloaded to the subinterpreter lane, returning `RuntimeRail[BrepResult]`.

## [02]-[BREP]

- Owner: `BrepOp` — one `@tagged_union` over five operation kinds with verbs as `StrEnum` rows, never a per-operation class family; `JoinPolicy` maps to `manifold3d.JoinType` through `_JOINS` so the join is a policy value, never a verb-keyed hardcode; `BrepResult` is the carrier-contributor and `BrepReceipt` the leaf evidence, the carrier/leaf split the mesh siblings share.
- Cases: linear extrusion/revolution of a profile is the `Offset` arm's `EXTRUDE`/`REVOLVE` row, never a duplicate `Construct` primitive — `MakePrism`/`MakeRevol` are reached once, through the profile leg; `Boolean.section` yields a wire/edge result the consumer re-feeds as a profile, and no downstream owner re-discriminates the operation past this union.
- Auto: `BRepAlgoAPI_*` is the robust BOPAlgo kernel (the legacy `BRepAlgo_*` family never enters) and its operators are n-ary — one `SetArguments`/`SetTools` build, never a pairwise fold rebuilding the kernel N-1 times; Boolean operands require triangulation absent, so a `Boolean` always precedes any `Tessellate` over its result.
- Packages: `cadquery-ocp` (the `OCP.*` band per the fence imports — the retired conda-only `pythonocc-core` `OCC.Core.*` path never enters), `manifold3d` (`CrossSection`/`JoinType`, the 2D leg only — the 3D `Manifold` CSG backend belongs to `mesh/repair.md#MESH`), `trimesh`, `numpy`, `expression`, and `msgspec` per the fence imports; `TessellationPolicy`/`CANONICAL_TESSELLATION` and `GeometrySubject` arrive from the geometry owners, the rails from runtime.
- Growth: a new primitive, set verb, offset mode, feature, or join is one `StrEnum` row plus one `Map` entry; a spine-following `SWEEP` verb is the `BRepOffsetAPI_MakePipeShell` landing site, staged behind a real spine-wire payload field rather than aliased to the linear `EXTRUDE` prism it cannot distinguish without one.
- Boundary: mesh-file/GLB codec is the data `MeshPayload` owner's (`rasm.data.spatial.mesh`); scene/USD/GLTF/OBJ export is `artifacts` figures/scene; the STEP-read-to-GLB hop is `mesh/cad.md#BRIDGE`'s `StepBridge`, a distinct OCCT consumer meeting this evaluator only at the shared `cadquery-ocp` band, never a shared function; triangle-soup repair and mesh CSG are `mesh/repair.md#MESH`'s — exact OCCT B-rep Boolean here, robust triangle-mesh Boolean there, two kernels on two owners.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
from collections.abc import Callable, Iterable
from enum import StrEnum
from typing import Final, Literal, Protocol, Self, assert_never

import numpy as np
import trimesh
from manifold3d import CrossSection, JoinType
from msgspec import Struct
from expression import case, tag, tagged_union
from expression.collections import Map

from OCP.BRep import BRep_Tool
from OCP.BRepAlgoAPI import BRepAlgoAPI_Common, BRepAlgoAPI_Cut, BRepAlgoAPI_Fuse, BRepAlgoAPI_Section
from OCP.BRepAlgoAPI import BRepAlgoAPI_Splitter
from OCP.BRepBuilderAPI import (
    BRepBuilderAPI_MakeEdge,
    BRepBuilderAPI_MakeFace,
    BRepBuilderAPI_MakePolygon,
    BRepBuilderAPI_MakeWire,
    BRepBuilderAPI_NurbsConvert,
    BRepBuilderAPI_Sewing,
)
from OCP.BRepFilletAPI import BRepFilletAPI_MakeChamfer, BRepFilletAPI_MakeFillet
from OCP.BRepGProp import BRepGProp
from OCP.BRepMesh import BRepMesh_IncrementalMesh
from OCP.BRepOffsetAPI import BRepOffsetAPI_MakePipeShell, BRepOffsetAPI_MakeThickSolid, BRepOffsetAPI_ThruSections
from OCP.BRepPrimAPI import (
    BRepPrimAPI_MakeBox,
    BRepPrimAPI_MakeCone,
    BRepPrimAPI_MakeCylinder,
    BRepPrimAPI_MakePrism,
    BRepPrimAPI_MakeRevol,
    BRepPrimAPI_MakeSphere,
    BRepPrimAPI_MakeTorus,
)
from OCP.GeomAPI import GeomAPI_PointsToBSpline
from OCP.GProp import GProp_GProps
from OCP.gp import gp_Ax1, gp_Dir, gp_Pnt, gp_Vec
from OCP.TopAbs import TopAbs_ShapeEnum
from OCP.TopExp import TopExp
from OCP.TopLoc import TopLoc_Location
from OCP.TopoDS import TopoDS, TopoDS_Face, TopoDS_Shape, TopoDS_Wire
from OCP.TColgp import TColgp_Array1OfPnt
from OCP.TopTools import TopTools_IndexedMapOfShape, TopTools_ListOfShape

from rasm.geometry.graduation import GeometrySubject
from rasm.geometry.mesh.cad import CANONICAL_TESSELLATION, TessellationPolicy
from rasm.runtime.faults import RuntimeRail
from rasm.runtime.lanes import LanePolicy
from rasm.runtime.receipts import Phase, Receipt, Redaction, receipted

# --- [TYPES] ----------------------------------------------------------------------------

type Shapes = tuple[TopoDS_Shape, ...]
type Params = tuple[float, ...]
type Profile = tuple[tuple[float, float], ...]
type Census = tuple[int, int, int, int]


class ConstructVerb(StrEnum):
    BOX = "box"
    SPHERE = "sphere"
    CYLINDER = "cylinder"
    CONE = "cone"
    TORUS = "torus"


class BooleanVerb(StrEnum):
    FUSE = "fuse"
    CUT = "cut"
    COMMON = "common"
    SECTION = "section"
    SPLIT = "split"  # arguments partitioned by tools, the non-destructive divide


class OffsetVerb(StrEnum):
    EXTRUDE = "extrude"
    REVOLVE = "revolve"
    LOFT = "loft"
    THICK = "thick"


class FeatureVerb(StrEnum):
    FILLET = "fillet"
    CHAMFER = "chamfer"
    SEW = "sew"  # join open shells into a solid-capable shell; size is the sewing tolerance
    NURBS = "nurbs"  # convert analytic geometry to NURBS form; size unused


class JoinPolicy(StrEnum):
    ROUND = "round"
    MITER = "miter"
    SQUARE = "square"
    BEVEL = "bevel"


# `_built` reads this surface rather than an erased `object`, so a maker missing a leg is a static gap, never a runtime miss.
class OcctBuilder(Protocol):
    def Build(self) -> None: ...
    def IsDone(self) -> bool: ...
    def Shape(self) -> TopoDS_Shape: ...


# --- [ERRORS] ---------------------------------------------------------------------------


# raised INTO the lane's `async_boundary`, never a domain `raise ValueError` the lane re-wraps.
@tagged_union(frozen=True)
class BrepFault(Exception):
    tag: Literal["not_done", "empty_profile", "holed_profile", "unknown_verb"] = tag()
    not_done: str = case()
    empty_profile: str = case()
    holed_profile: str = case()
    unknown_verb: str = case()


# --- [CONSTANTS] ------------------------------------------------------------------------

# keep-all policy: the validity verdict and geometry measures are non-secret, so no field classifies.
REDACTION: Final[Redaction] = Redaction(classified=Map.empty())

# --- [TABLES] ---------------------------------------------------------------------------

# one factory row per verb; an unmapped verb is the `unknown_verb` fault via `try_find`, never a bare `KeyError`.
_PRIMITIVES: Final[Map[ConstructVerb, Callable[[Params], OcctBuilder]]] = Map.of_seq([
    (ConstructVerb.BOX, lambda p: BRepPrimAPI_MakeBox(p[0], p[1], p[2])),
    (ConstructVerb.SPHERE, lambda p: BRepPrimAPI_MakeSphere(p[0])),
    (ConstructVerb.CYLINDER, lambda p: BRepPrimAPI_MakeCylinder(p[0], p[1])),
    (ConstructVerb.CONE, lambda p: BRepPrimAPI_MakeCone(p[0], p[1], p[2])),
    (ConstructVerb.TORUS, lambda p: BRepPrimAPI_MakeTorus(p[0], p[1])),
])

_BOOLEANS: Final[Map[BooleanVerb, Callable[[], OcctBuilder]]] = Map.of_seq([
    (BooleanVerb.FUSE, BRepAlgoAPI_Fuse),
    (BooleanVerb.CUT, BRepAlgoAPI_Cut),
    (BooleanVerb.COMMON, BRepAlgoAPI_Common),
    (BooleanVerb.SECTION, BRepAlgoAPI_Section),
    (BooleanVerb.SPLIT, BRepAlgoAPI_Splitter),
])

# edge-fold rows only: SEW/NURBS carry non-maker call shapes, matched ahead of this table in the feature arm.
_FEATURES: Final[Map[FeatureVerb, Callable[[TopoDS_Shape], OcctBuilder]]] = Map.of_seq([
    (FeatureVerb.FILLET, BRepFilletAPI_MakeFillet),
    (FeatureVerb.CHAMFER, BRepFilletAPI_MakeChamfer),
])

_JOINS: Final[Map[JoinPolicy, JoinType]] = Map.of_seq([
    (JoinPolicy.ROUND, JoinType.Round),
    (JoinPolicy.MITER, JoinType.Miter),
    (JoinPolicy.SQUARE, JoinType.Square),
    (JoinPolicy.BEVEL, JoinType.Bevel),
])

# fixed 4-arity so the `vertex, edge, face, solid` unpack is statically total against the `Census` 4-tuple.
_CENSUS: Final[tuple[TopAbs_ShapeEnum, TopAbs_ShapeEnum, TopAbs_ShapeEnum, TopAbs_ShapeEnum]] = (
    TopAbs_ShapeEnum.TopAbs_VERTEX,
    TopAbs_ShapeEnum.TopAbs_EDGE,
    TopAbs_ShapeEnum.TopAbs_FACE,
    TopAbs_ShapeEnum.TopAbs_SOLID,
)


# --- [MODELS] ---------------------------------------------------------------------------


class BrepReceipt(Struct, frozen=True, gc=False):  # leaf-scalar evidence; owns only its fact projection
    kind: str
    valid: bool
    volume: float
    area: float
    centroid: tuple[float, float, float]
    census: Census
    watertight: bool | None
    closure_gap: float | None
    modified: bool | None = None  # BOPAlgo History().HasModified() on the boolean arm; None elsewhere
    generated: bool | None = None  # BOPAlgo History().HasGenerated()
    subject: GeometrySubject = GeometrySubject.MESH_ALGEBRA

    # `valid` keys the phase: a null/open result (a section wire, an open shell) keys `admitted` — a flagged caveat, never an asserted solid.
    def fact(self) -> tuple[Phase, GeometrySubject, dict[str, object]]:
        phase: Phase = "emitted" if self.valid else "admitted"
        v, e, f, s = self.census
        facts: dict[str, object] = {  # native scalars for the receipts enc_hook=repr renderer
            "kind": self.kind,
            "valid": self.valid,
            "volume": self.volume,
            "area": self.area,
            "centroid": self.centroid,
            "census": f"v{v}/e{e}/f{f}/s{s}",
            "watertight": self.watertight,
            "closure_gap": self.closure_gap,
            "modified": self.modified,
            "generated": self.generated,
        }
        return phase, self.subject, facts


class BrepResult(Struct, frozen=True):
    shape: TopoDS_Shape
    mesh: trimesh.Trimesh | None
    receipt: BrepReceipt

    def contribute(self) -> Iterable[Receipt]:
        yield Receipt.of("mesh.brep", self.receipt.fact())


@tagged_union(frozen=True)
class BrepOp:
    tag: Literal["construct", "boolean", "offset", "feature", "tessellate"] = tag()
    construct: tuple[ConstructVerb, Params] = case()
    boolean: tuple[Shapes, BooleanVerb, float] = case()
    offset: tuple[Profile, OffsetVerb, float, float, JoinPolicy, bool] = case()
    feature: tuple[TopoDS_Shape, FeatureVerb, float] = case()
    tessellate: tuple[TopoDS_Shape, TessellationPolicy] = case()

    @staticmethod
    def Construct(verb: ConstructVerb, params: Params) -> Self:
        return BrepOp(construct=(verb, params))

    @staticmethod
    def Boolean(shapes: Shapes, verb: BooleanVerb, fuzzy: float = 0.0) -> Self:
        # positive fuzz drives SetFuzzyValue tolerant intersection for near-coincident operands
        return BrepOp(boolean=(shapes, verb, fuzzy))

    @staticmethod
    def Offset(
        profile: Profile, verb: OffsetVerb, dist: float, height: float = 0.0, join: JoinPolicy = JoinPolicy.ROUND, smooth: bool = False
    ) -> Self:
        # `smooth` lifts the profile through a B-spline edge instead of the polyline wire
        return BrepOp(offset=(profile, verb, dist, height, join, smooth))

    @staticmethod
    def Feature(shape: TopoDS_Shape, verb: FeatureVerb, size: float) -> Self:
        return BrepOp(feature=(shape, verb, size))

    @staticmethod
    def Tessellate(shape: TopoDS_Shape, policy: TessellationPolicy = CANONICAL_TESSELLATION) -> Self:
        return BrepOp(tessellate=(shape, policy))


# --- [OPERATIONS] -----------------------------------------------------------------------


async def apply(op: BrepOp, lane: LanePolicy) -> "RuntimeRail[BrepResult]":
    # the `Error` path carries no emit — the lane fence already recorded the fault.
    return (await lane.offload(_dispatch, op)).map(_emit)


@receipted(REDACTION)
def _emit(result: BrepResult) -> BrepResult:
    # returns the contributor itself, so the mapped `Ok` arm keeps shape/mesh AND the receipt streams once.
    return result


def _built(builder: OcctBuilder) -> TopoDS_Shape:
    builder.Build()
    if not builder.IsDone():
        raise BrepFault(not_done=type(builder).__name__)
    return builder.Shape()


def _census(shape: TopoDS_Shape) -> Census:
    def extent(kind: TopAbs_ShapeEnum) -> int:
        carrier = TopTools_IndexedMapOfShape()
        TopExp.MapShapes_s(shape, kind, carrier)
        return carrier.Extent()

    vertex, edge, face, solid = _CENSUS
    return (extent(vertex), extent(edge), extent(face), extent(solid))


def _triangulation(shape: TopoDS_Shape) -> trimesh.Trimesh:
    def face_block(face_shape: TopoDS_Shape) -> tuple[np.ndarray, np.ndarray]:
        face, loc = TopoDS.Face_s(face_shape), TopLoc_Location()
        tri = BRep_Tool.Triangulation_s(face, loc)
        # one-based `Poly_Triangulation`; `process=True` below welds the per-face duplicate boundary vertices the kernel emits as
        # independent blocks — without the weld every face block is an island and `is_watertight` is structurally false on a closed solid.
        nodes = [[(p := tri.Node(i)).X(), p.Y(), p.Z()] for i in range(1, tri.NbNodes() + 1)]
        tris = (tri.Triangle(i) for i in range(1, tri.NbTriangles() + 1))
        faces = [[t.Value(1) - 1, t.Value(2) - 1, t.Value(3) - 1] for t in tris]
        return np.asarray(nodes, dtype=np.float64).reshape(-1, 3), np.asarray(faces, dtype=np.int64).reshape(-1, 3)

    faces_map = TopTools_IndexedMapOfShape()
    TopExp.MapShapes_s(shape, TopAbs_ShapeEnum.TopAbs_FACE, faces_map)
    blocks = [face_block(faces_map.FindKey(i)) for i in range(1, faces_map.Extent() + 1)]
    offsets = np.cumsum([0, *(len(nodes) for nodes, _ in blocks[:-1])])
    return trimesh.Trimesh(
        vertices=np.vstack([nodes for nodes, _ in blocks]),
        faces=np.vstack([tris + off for (_, tris), off in zip(blocks, offsets, strict=True)]),
        process=True,
    )


def _evidence(
    kind: str, shape: TopoDS_Shape, mesh: trimesh.Trimesh | None, *, modified: bool | None = None, generated: bool | None = None
) -> BrepResult:
    volume, area = GProp_GProps(), GProp_GProps()
    BRepGProp.VolumeProperties_s(shape, volume)
    BRepGProp.SurfaceProperties_s(shape, area)
    com, mass = volume.CentreOfMass(), float(volume.Mass())
    watertight = None if mesh is None else bool(mesh.is_watertight)
    # divergence-theorem `volume` is meaningless on an open surface: an open result records a `None` gap, never a spurious agreement.
    closure_gap = abs(mass - float(mesh.volume)) if watertight else None
    return BrepResult(
        shape,
        mesh,
        BrepReceipt(
            kind, not shape.IsNull(), mass, float(area.Mass()), (com.X(), com.Y(), com.Z()), _census(shape), watertight, closure_gap, modified, generated
        ),
    )


def _shape_list(shapes: Shapes) -> TopTools_ListOfShape:
    carrier = TopTools_ListOfShape()
    for shape in shapes:
        carrier.Append(shape)
    return carrier


def _boolean(shapes: Shapes, verb: BooleanVerb, fuzzy: float) -> tuple[TopoDS_Shape, bool, bool]:
    op = _BOOLEANS.try_find(verb).default_with(lambda: _raise(BrepFault(unknown_verb=verb)))()
    op.SetArguments(_shape_list(shapes[:1]))
    op.SetTools(_shape_list(shapes[1:]))
    if fuzzy > 0.0:
        op.SetFuzzyValue(fuzzy)  # BOPAlgo tolerant intersection for near-coincident operands
    op.SetRunParallel(True)
    shape = _built(op)
    history = op.History()  # BOPAlgo provenance the receipt records
    return shape, bool(history.HasModified()), bool(history.HasGenerated())


# the `(base, offset)` pair serves both the face lift and the loft rings, so the `manifold3d` 2D leg runs once per profile.
def _sections(profile: Profile, dist: float, join: JoinPolicy) -> tuple[CrossSection, CrossSection]:
    if not profile:
        raise BrepFault(empty_profile="offset")
    base = CrossSection([list(profile)])
    join_type = _JOINS.try_find(join).default_with(lambda: _raise(BrepFault(unknown_verb=join)))
    offset = base.offset(dist, join_type, 2.0, 0).simplify(1e-6) if dist else base
    return base, offset


def _profile_face(section: CrossSection, *, smooth: bool = False) -> TopoDS_Face:
    outer, *holes = section.to_polygons()
    builder = BRepBuilderAPI_MakeFace(_wire(outer, smooth=smooth))
    for hole in holes:
        builder.Add(_wire(hole, smooth=smooth))
    return builder.Face()


def _wire(contour: np.ndarray, z: float = 0.0, *, smooth: bool = False) -> TopoDS_Wire:
    if smooth:
        # a fit B-spline through the contour points — the smooth profile modality, one flag, never a parallel entry.
        points = TColgp_Array1OfPnt(1, len(contour))
        for i, (x, y) in enumerate(contour, start=1):
            points.SetValue(i, gp_Pnt(float(x), float(y), z))
        curve = GeomAPI_PointsToBSpline(points).Curve()
        return _built_wire(BRepBuilderAPI_MakeWire(BRepBuilderAPI_MakeEdge(curve).Edge()))
    polygon = BRepBuilderAPI_MakePolygon()
    for x, y in contour:
        polygon.Add(gp_Pnt(float(x), float(y), z))
    polygon.Close()
    return polygon.Wire()


def _built_wire(builder: "BRepBuilderAPI_MakeWire") -> TopoDS_Wire:
    if not builder.IsDone():
        raise BrepFault(not_done=type(builder).__name__)
    return builder.Wire()


# keeps each `default_with` table-miss fold a one-expression thunk; converts on the lane boundary.
def _raise[T](fault: BrepFault) -> T:
    raise fault


def _dispatch(op: BrepOp) -> BrepResult:
    match op:
        case BrepOp(tag="construct", construct=(verb, params)):
            factory = _PRIMITIVES.try_find(verb).default_with(lambda: _raise(BrepFault(unknown_verb=verb)))
            return _evidence(f"construct.{verb}", _built(factory(params)), None)
        case BrepOp(tag="boolean", boolean=(shapes, verb, fuzzy)):
            shape, modified, generated = _boolean(shapes, verb, fuzzy)
            return _evidence(f"boolean.{verb}", shape, None, modified=modified, generated=generated)
        case BrepOp(tag="offset", offset=(profile, verb, dist, height, join, smooth)):
            base, offset = _sections(profile, dist, join)
            match verb:
                case OffsetVerb.EXTRUDE:
                    shape = _built(BRepPrimAPI_MakePrism(_profile_face(offset, smooth=smooth), gp_Vec(0.0, 0.0, height)))
                case OffsetVerb.REVOLVE:
                    # full revolution about the global Y axis through origin (the canon `_wire`'s z=0 placement establishes);
                    # a profile crossing the axis is a degenerate self-intersecting revolve the `IsDone` gate rails.
                    shape = _built(BRepPrimAPI_MakeRevol(_profile_face(offset, smooth=smooth), gp_Ax1(gp_Pnt(), gp_Dir(0.0, 1.0, 0.0))))
                case OffsetVerb.LOFT:
                    # `ThruSections` lofts one wire skin between the OUTER rings — a holed loft is the staged `Cut`-of-two-skins
                    # arm, so an inner-loop profile rejects rather than silently dropping its holes.
                    base_rings, offset_rings = base.to_polygons(), offset.to_polygons()
                    if len(base_rings) > 1 or len(offset_rings) > 1:
                        raise BrepFault(holed_profile="loft")
                    loft = BRepOffsetAPI_ThruSections(True)
                    loft.AddWire(_wire(base_rings[0], smooth=smooth))
                    loft.AddWire(_wire(offset_rings[0], height, smooth=smooth))
                    shape = _built(loft)
                case OffsetVerb.THICK:
                    solid = _built(BRepPrimAPI_MakePrism(_profile_face(offset, smooth=smooth), gp_Vec(0.0, 0.0, height)))
                    # `MakeThickSolidBySimple` on the no-arg ctor is the operation; the bare 4-arg ctor is a phantom (OCP builder law).
                    thick = BRepOffsetAPI_MakeThickSolid()
                    thick.MakeThickSolidBySimple(solid, -dist)
                    shape = _built(thick)
                case unreachable:
                    assert_never(unreachable)
            return _evidence(f"offset.{verb}", shape, None)
        case BrepOp(tag="feature", feature=(target, FeatureVerb.SEW, size)):
            # Sewing is Perform/SewedShape, not the Build/IsDone/Shape maker family
            sewing = BRepBuilderAPI_Sewing(size)
            sewing.Add(target)
            sewing.Perform()
            return _evidence("feature.sew", sewing.SewedShape(), None)
        case BrepOp(tag="feature", feature=(target, FeatureVerb.NURBS, _)):
            return _evidence("feature.nurbs", _built(BRepBuilderAPI_NurbsConvert(target)), None)
        case BrepOp(tag="feature", feature=(target, verb, size)):
            factory = _FEATURES.try_find(verb).default_with(lambda: _raise(BrepFault(unknown_verb=verb)))
            feature, edges = factory(target), TopTools_IndexedMapOfShape()
            TopExp.MapShapes_s(target, TopAbs_ShapeEnum.TopAbs_EDGE, edges)
            for i in range(1, edges.Extent() + 1):
                feature.Add(size, TopoDS.Edge_s(edges.FindKey(i)))
            return _evidence(f"feature.{verb}", _built(feature), None)
        case BrepOp(tag="tessellate", tessellate=(shape, policy)):
            BRepMesh_IncrementalMesh(shape, policy.deflection, False, policy.angle_tolerance, True)
            return _evidence("tessellate", shape, _triangulation(shape))
        case unreachable:
            assert_never(unreachable)
```

## [03]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)

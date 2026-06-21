# [PY_GEOMETRY_MESH_BREP]

Exact B-rep evaluation — the OCCT-kernel companion the CAD-STEP hop, the mesh-algebra rail, and the IFC profile owner compose for parametric solid construction, robust Boolean algebra, offset/loft/sweep generation, feature operations, and watertight tessellation. `BrepOp` is ONE tagged union discriminating five operation kinds — `Construct`, `Boolean`, `Offset`, `Feature`, `Tessellate` — each carrying its inner verb as a closed `StrEnum` row rather than a parallel per-operation class, so a new primitive, set verb, offset mode, or feature is one verb row plus one dispatch-table entry binding the OCCT `BRep*API` builder that owns it, never a new surface. Every OCCT `Make*` builder rides the identical command pattern (construct → `Build` → `IsDone` → `Shape`), so the union folds that pattern once through `_built` and reads the resulting `TopoDS_Shape` through the shared `GProp_GProps` evidence accumulator. The `Offset` arm weaves `manifold3d.CrossSection` polygon CSG into the OCCT loft/sweep/thick-solid leg — a 2D profile offsets and simplifies in the robust `manifold3d` 2D kernel, the offset contour and its holes egress through `to_polygons()` and lift through `BRepBuilderAPI_MakeFace.Add` into a planar holed `TopoDS_Face`, then extrude/revolve/loft/thick-solid into a 3D `TopoDS_Shape`, one rail integrating both kernels. The `Tessellate` arm meshes the result in place with `BRepMesh_IncrementalMesh` under the shared `IdentityPolicy` deflection and hands an **in-memory `trimesh.Trimesh`** back across the wire — ALL mesh-file/GLB encode defers to the data `MeshPayload` owner (`rasm.data.spatial.mesh`), and ALL scene/USD/GLTF/OBJ scene export defers to `artifacts` figures/scene; this owner writes no file. Every arm returns `BrepResult` pairing the evaluated `TopoDS_Shape` (and the optional tessellation) with `BrepReceipt` carrying the kind, the OCCT validity verdict, the `GProp` volume/area/centroid, the one-pass sub-shape census, the trimesh watertight verdict and the exact-vs-tessellated closure-gap cross-check when a triangulation exists, and the canonical `GeometrySubject` `mesh-algebra` literal so a CSG solid graduates through the one geometry rail. The whole companion-band kernel is CPU-bound, so `apply` is the offload-aware entry — it hands `_dispatch` to the runtime `LanePolicy.offload` as one PEP 734 subinterpreter hand-off under the active OTel context, the lane importing neither OCCT nor the kernel.

## [01]-[INDEX]

- [01]-[BREP]: the five-case B-rep operation union over `cadquery-ocp` OCCT plus `manifold3d.CrossSection`, folded through one OCCT command-pattern dispatch and one `GProp`-plus-trimesh evidence read, handed to the runtime CPU-offload lane, returning an evaluated `TopoDS_Shape` and an in-memory `trimesh.Trimesh` tessellation.

## [02]-[BREP]

- Owner: `BrepOp` — the tagged union discriminating the five operation kinds; `ConstructVerb`/`BooleanVerb`/`OffsetVerb`/`FeatureVerb` the closed `StrEnum` rows selecting the inner OCCT builder so each kind is one row rather than three sibling classes; `JoinPolicy` the closed offset-join vocabulary mapping to `manifold3d.JoinType` through `_JOINS` so the join is a policy value, never a verb-keyed hardcode; `_PRIMITIVES`/`_BOOLEANS`/`_FEATURES` the data-driven dispatch tables binding each verb to its OCCT builder factory so the command pattern is folded once, not re-spelled per arm; `BrepResult` the carrier pairing the evaluated `TopoDS_Shape` with the optional in-memory `trimesh.Trimesh` tessellation and `BrepReceipt`; `BrepReceipt` the typed receipt — itself a `ReceiptContributor` whose `contribute` folds its fields into one emitted-phase `Receipt.of` row — carrying the kind label, the `IsNull` validity verdict, the `GProp` volume/area/centroid, the one-pass vertex/edge/face/solid census, the trimesh watertight verdict and exact-vs-tessellated closure gap (both `None` until tessellation), and the `GeometrySubject` literal defaulted to `mesh-algebra`.
- Cases: `BrepOp` cases `Construct(verb, params)` (primitive box/sphere/cylinder/cone/torus over `BRepPrimAPI_*`), `Boolean(shapes, verb)` (n-ary fuse/cut/common/section over `BRepAlgoAPI_*`), `Offset(profile, verb, dist, height, join)` (the `manifold3d.CrossSection` 2D-offset-simplify leg lifted through `BRepBuilderAPI_MakeFace.Add` then extruded/revolved/lofted/thickened over `BRepPrimAPI`/`BRepOffsetAPI`), `Feature(shape, verb, size)` (fillet/chamfer over `BRepFilletAPI_*`), and `Tessellate(shape, policy)` (the in-place `BRepMesh_IncrementalMesh` triangulation extracted to an in-memory `trimesh.Trimesh`) — matched by `match`/`assert_never`, each binding the OCCT builder family that owns the kind through the dispatch tables. Linear extrusion/revolution of a profile is the `Offset` arm's `EXTRUDE`/`REVOLVE` row, not a duplicate `Construct` primitive — `MakePrism`/`MakeRevol` are reached once, through the profile leg. The `Boolean.section` row yields a `TopoDS` wire/edge result the consumer re-feeds as a profile; the daemon and CAD hop never re-discriminate the operation past this owner.
- Entry: `apply` is `async` — it hands `_dispatch` plus the `BrepOp` to the runtime `LanePolicy.offload`, returning the `RuntimeRail[BrepResult]` the lane drains, so the whole companion-band kernel rides one PEP 734 subinterpreter hop under the active OTel context and the lane imports neither OCCT nor the kernel. `_dispatch` resolves the inner verb through the dispatch tables, invokes the builder factory, calls `_built` (the one `Build`/`IsDone`/`Shape` fold over every OCCT `Make*` builder, raising a `ValueError` the lane's `async_boundary` converts to a `BoundaryFault` when `IsDone()` is false so a degenerate construction never emits a phantom solid), and reads the result through `_evidence`. The `Construct` arm builds the primitive from the typed `params` tuple; the `Boolean` arm folds the shape sequence pairwise through the selected `BRepAlgoAPI_*` operation; the `Offset` arm runs the `manifold3d.CrossSection` offset/simplify leg, lifts the offset contour and its holes through `BRepBuilderAPI_MakeFace.Add` into a planar holed `TopoDS_Face`, and extrudes/revolves/lofts/thickens it through the OCCT family; the `Feature` arm applies the fillet/chamfer over the edges indexed by `TopExp.MapShapes_s`; the `Tessellate` arm meshes in place and extracts vertices/triangles into a `trimesh.Trimesh` carried back in `BrepResult.mesh`. The evaluated `TopoDS_Shape` rides back in `BrepResult.shape`; any persisted GLB/STEP encode is a downstream call — `MeshPayload` for the triangulation, the CAD-STEP `StepBridge` for the B-rep GLB, `artifacts` figures/scene for a visualization scene — never an arm here.
- Auto: every OCCT `Make*` builder shares the command pattern (`Build`/`IsDone`/`Shape`), so `_built` folds it once and every verb row reuses it instead of re-spelling the check; `BRepAlgoAPI_Fuse`/`Cut`/`Common`/`Section` are the robust 2019+ Boolean kernel (the legacy `BRepAlgo_*` path is the deleted alternative) requiring the operands' triangulation absent, so the Boolean arm precedes any tessellate; the `Offset` arm weaves three `manifold3d.CrossSection` capabilities through one OCCT face lift — `offset(delta, join_type, miter_limit, arc_tol)` exact polygon offset under the `_JOINS`-mapped `JoinType`, `simplify(epsilon)` collinear-vertex cull, and the multi-contour `to_polygons()` egress feeding the `BRepBuilderAPI_MakeFace.Add` hole-wire accumulation so a profile with islands lifts as one holed planar face rather than an outer-loop-only approximation; `BRepMesh_IncrementalMesh(shape, deflection, relative, angle, parallel)` mutates the shape's stored triangulation in place from `IdentityPolicy.deflection`/`angle_tolerance`, and `_triangulation` reads `BRep_Tool.Triangulation_s` per face, vectorizing the `(nodes, triangles)` blocks into one `(V,3)`/`(F,3)` pair through `np.cumsum` index offsets under `process=True` so the trimesh merge-and-validate pass welds the per-face duplicate boundary vertices the kernel emits as independent node blocks — without the weld every face block is an island and `is_watertight` is structurally false on a closed solid, the merge deduplicates coincident vertices without re-triangulating the faces, so the weld is the precondition for the closure verdict rather than an optional pass; `BRepGProp.VolumeProperties_s`/`SurfaceProperties_s` populate two `GProp_GProps` accumulators exposing `Mass`/`CentreOfMass` as the exact per-solid evidence, `TopExp.MapShapes_s(shape, kind, TopTools_IndexedMapOfShape)` indexes the vertex/edge/face/solid census in one pass per kind through `Extent`, and when a tessellation exists `_evidence` reads the welded `is_watertight` verdict and cross-checks the exact `GProp` volume against `trimesh.Trimesh.volume` only when that verdict holds — the divergence-theorem `volume` is meaningless on an open surface, so an open shell or section result records `watertight=False` with a `None` closure gap rather than a spurious agreement number, and a closed solid carries the independent kernel-vs-mesh closure verdict rather than a single-source claim.
- Receipt: `BrepReceipt.contribute` is the `ReceiptContributor` method folding the kind label, the validity verdict, the `GProp` volume/area, the centroid, the census, the trimesh watertight verdict, and the closure gap into one emitted-phase `Receipt.of("emitted", "mesh.brep", kind, facts)` row through the runtime stream — no parallel receipt rail; the evaluated solid carries the geometry `GraduationReceipt` subject `mesh-algebra` — the canonical `GeometrySubject` literal (`rasm.compute.graduation.handoff#GeometrySubject`, never a bare `str`) so a CSG/feature/offset result graduates through the one geometry rail the `mesh/repair.md#MESH` boolean arm and the `graph/algebra.md#ALGEBRA` mesh fold share.
- Packages: `cadquery-ocp` (`OCP.BRepPrimAPI.BRepPrimAPI_MakeBox`/`MakeSphere`/`MakeCylinder`/`MakeCone`/`MakeTorus`/`MakePrism`/`MakeRevol`, `OCP.BRepAlgoAPI.BRepAlgoAPI_Fuse`/`Cut`/`Common`/`Section`, `OCP.BRepOffsetAPI.BRepOffsetAPI_ThruSections`/`MakePipeShell`/`MakeThickSolid`, `OCP.BRepFilletAPI.BRepFilletAPI_MakeFillet`/`MakeChamfer`, `OCP.BRepBuilderAPI.BRepBuilderAPI_MakePolygon`/`MakeFace`, `OCP.BRepMesh.BRepMesh_IncrementalMesh`, `OCP.BRep.BRep_Tool`, `OCP.BRepGProp.BRepGProp`, `OCP.GProp.GProp_GProps`, `OCP.TopExp.TopExp`, `OCP.TopTools.TopTools_IndexedMapOfShape`, `OCP.TopAbs.TopAbs_ShapeEnum`, `OCP.TopoDS.TopoDS`/`TopoDS_Shape`/`TopoDS_Face`, `OCP.TopLoc.TopLoc_Location`, `OCP.gp.gp_Pnt`/`gp_Vec`/`gp_Ax1`/`gp_Dir`), `manifold3d` (`CrossSection`/`JoinType` for the 2D profile offset/simplify leg, never a 3D `Manifold` CSG owner — that is the `trimesh.boolean` backend in `mesh/repair.md#MESH`), `trimesh` (`Trimesh` the in-memory triangulation carrier plus its cached `is_watertight`/`volume` properties feeding the closure cross-check), `numpy` (`asarray`/`vstack`/`cumsum` vectorizing the per-face node/triangle blocks into one `(V,3)`/`(F,3)` pair), runtime (`IdentityPolicy`/`CANONICAL_POLICY`/`RuntimeRail`/`LanePolicy`/`Receipt`/`ReceiptContributor`).
- Growth: a new primitive is one `ConstructVerb` row plus one `_PRIMITIVES` factory binding the `BRepPrimAPI_*` builder; a new set verb is one `BooleanVerb` row plus one `_BOOLEANS` entry; a new offset/loft mode is one `OffsetVerb` row binding its `BRepOffsetAPI_*` builder; a spine-following `SWEEP` verb is the imported `BRepOffsetAPI_MakePipeShell` landing site that admits the same turn a spine wire enters the `Offset` payload as a new field, so the verb is staged behind a real input rather than aliased to the linear `EXTRUDE` prism it cannot distinguish without a spine; a new feature is one `FeatureVerb` row plus one `_FEATURES` entry binding `BRepFilletAPI_*`; a new join geometry is one `JoinPolicy` row plus one `_JOINS` entry; the deflection knob is the shared `IdentityPolicy` already keyed by the daemon cache. The CAD-STEP `StepBridge` re-uses the `Tessellate` arm's `_triangulation` read rather than re-implementing per-face node extraction, and an `ifc/structural` profile composes `Construct`/`Offset` to evaluate an `IfcProfileDef` cross-section into a `TopoDS_Face` before the `numpy` section integral. Zero new surface, no parallel per-operation class family.
- Boundary: this owner is the exact-kernel evaluator — it constructs, combines, offsets, features, and triangulates B-rep solids and hands the result across the wire as an in-memory `TopoDS_Shape` plus optional `trimesh.Trimesh`. No mesh-file/GLB decode/encode (the data `MeshPayload` owner `rasm.data.spatial.mesh` holds the three-engine `trimesh`/`meshio`/`rhino3dm` codec, the `MeshBackend.of` router, and the GLB `preview`); no scene/USD/GLTF/OBJ scene export (that is `artifacts` figures/scene); no STEP/IGES read-to-GLB hop (that is the sibling `mesh/cad.md#BRIDGE` `StepBridge` over the XCAF CAF reader — this owner evaluates B-rep already in memory, the bridge reads source bytes); no triangle-soup repair or `trimesh.boolean` mesh CSG (that is `mesh/repair.md#MESH` — exact OCCT B-rep Boolean is here, robust triangle-mesh Boolean is there, the two kernels are distinct rows on the two owners, never a shared third surface). The deleted forms: a per-operation class family (`BoxOp`/`FuseOp`/`FilletOp`) over the `BrepOp` discriminant, a `make_box`/`make_sphere` factory proliferation over the `ConstructVerb` row, a duplicate `Construct.prism`/`Construct.revol` primitive racing the `Offset` extrude/revolve leg, a verb-keyed `JoinType` hardcode racing the `JoinPolicy` row, an imperative `TopExp_Explorer` `while`-loop accumulating the census where `MapShapes_s` indexes it in one pass, a `mass` field duplicating the `volume` `GProp` read, a hand-rolled OCCT command-pattern check re-spelled per arm rather than the `_built` fold, a second 3D `manifold3d.Manifold` CSG owner duplicating the `mesh/repair.md` boolean backend, a synchronous `apply` blocking the companion event loop on the CPU kernel where the offload lane isolates it, ANY `Codec`/`load`/`export`/`write` mesh-file or scene arm re-deriving the `MeshPayload`/`artifacts` seam, and the retired conda-only `pythonocc-core` `OCC.Core.*` path.

```python signature
from enum import StrEnum
from typing import Literal, Self, assert_never

import numpy as np
import trimesh
from manifold3d import CrossSection, JoinType
from msgspec import Struct
from expression import case, tag, tagged_union

from OCP.BRep import BRep_Tool
from OCP.BRepAlgoAPI import BRepAlgoAPI_Common, BRepAlgoAPI_Cut, BRepAlgoAPI_Fuse, BRepAlgoAPI_Section
from OCP.BRepBuilderAPI import BRepBuilderAPI_MakeFace, BRepBuilderAPI_MakePolygon
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
from OCP.GProp import GProp_GProps
from OCP.gp import gp_Ax1, gp_Dir, gp_Pnt, gp_Vec
from OCP.TopAbs import TopAbs_ShapeEnum
from OCP.TopExp import TopExp
from OCP.TopLoc import TopLoc_Location
from OCP.TopoDS import TopoDS, TopoDS_Face, TopoDS_Shape, TopoDS_Wire
from OCP.TopTools import TopTools_IndexedMapOfShape

from rasm.compute.graduation.handoff import GeometrySubject
from rasm.runtime.content_identity import CANONICAL_POLICY, IdentityPolicy
from rasm.runtime.execution.lanes import LanePolicy
from rasm.runtime.faults import RuntimeRail
from rasm.runtime.receipts import Receipt, ReceiptContributor

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


class OffsetVerb(StrEnum):
    EXTRUDE = "extrude"
    REVOLVE = "revolve"
    LOFT = "loft"
    THICK = "thick"


class FeatureVerb(StrEnum):
    FILLET = "fillet"
    CHAMFER = "chamfer"


class JoinPolicy(StrEnum):
    ROUND = "round"
    MITER = "miter"
    SQUARE = "square"
    BEVEL = "bevel"


# --- [CONSTANTS] ------------------------------------------------------------------------

_PRIMITIVES = {
    ConstructVerb.BOX: lambda p: BRepPrimAPI_MakeBox(p[0], p[1], p[2]),
    ConstructVerb.SPHERE: lambda p: BRepPrimAPI_MakeSphere(p[0]),
    ConstructVerb.CYLINDER: lambda p: BRepPrimAPI_MakeCylinder(p[0], p[1]),
    ConstructVerb.CONE: lambda p: BRepPrimAPI_MakeCone(p[0], p[1], p[2]),
    ConstructVerb.TORUS: lambda p: BRepPrimAPI_MakeTorus(p[0], p[1]),
}

_BOOLEANS = {
    BooleanVerb.FUSE: BRepAlgoAPI_Fuse,
    BooleanVerb.CUT: BRepAlgoAPI_Cut,
    BooleanVerb.COMMON: BRepAlgoAPI_Common,
    BooleanVerb.SECTION: BRepAlgoAPI_Section,
}

_FEATURES = {
    FeatureVerb.FILLET: BRepFilletAPI_MakeFillet,
    FeatureVerb.CHAMFER: BRepFilletAPI_MakeChamfer,
}

_JOINS = {
    JoinPolicy.ROUND: JoinType.Round,
    JoinPolicy.MITER: JoinType.Miter,
    JoinPolicy.SQUARE: JoinType.Square,
    JoinPolicy.BEVEL: JoinType.Bevel,
}

_CENSUS: tuple[TopAbs_ShapeEnum, ...] = (
    TopAbs_ShapeEnum.TopAbs_VERTEX,
    TopAbs_ShapeEnum.TopAbs_EDGE,
    TopAbs_ShapeEnum.TopAbs_FACE,
    TopAbs_ShapeEnum.TopAbs_SOLID,
)


# --- [MODELS] ---------------------------------------------------------------------------

class BrepReceipt(Struct, frozen=True):
    kind: str
    valid: bool
    volume: float
    area: float
    centroid: tuple[float, float, float]
    census: Census
    watertight: bool | None
    closure_gap: float | None
    subject: GeometrySubject = "mesh-algebra"

    def contribute(self) -> Receipt:
        v, e, f, s = self.census
        return Receipt.of(
            "emitted",
            "mesh.brep",
            self.kind,
            {
                "valid": str(self.valid),
                "volume": f"{self.volume:.6g}",
                "area": f"{self.area:.6g}",
                "centroid": f"{self.centroid[0]:.6g},{self.centroid[1]:.6g},{self.centroid[2]:.6g}",
                "census": f"v{v}/e{e}/f{f}/s{s}",
                "watertight": "" if self.watertight is None else str(self.watertight),
                "closure_gap": "" if self.closure_gap is None else f"{self.closure_gap:.3g}",
                "subject": self.subject,
            },
        )


class BrepResult(Struct, frozen=True):
    shape: TopoDS_Shape
    mesh: trimesh.Trimesh | None
    receipt: BrepReceipt


@tagged_union(frozen=True)
class BrepOp:
    tag: Literal["construct", "boolean", "offset", "feature", "tessellate"] = tag()
    construct: tuple[ConstructVerb, Params] = case()
    boolean: tuple[Shapes, BooleanVerb] = case()
    offset: tuple[Profile, OffsetVerb, float, float, JoinPolicy] = case()
    feature: tuple[TopoDS_Shape, FeatureVerb, float] = case()
    tessellate: tuple[TopoDS_Shape, IdentityPolicy] = case()

    @staticmethod
    def Construct(verb: ConstructVerb, params: Params) -> Self:
        return BrepOp(construct=(verb, params))

    @staticmethod
    def Boolean(shapes: Shapes, verb: BooleanVerb) -> Self:
        return BrepOp(boolean=(shapes, verb))

    @staticmethod
    def Offset(profile: Profile, verb: OffsetVerb, dist: float, height: float = 0.0, join: JoinPolicy = JoinPolicy.ROUND) -> Self:
        return BrepOp(offset=(profile, verb, dist, height, join))

    @staticmethod
    def Feature(shape: TopoDS_Shape, verb: FeatureVerb, size: float) -> Self:
        return BrepOp(feature=(shape, verb, size))

    @staticmethod
    def Tessellate(shape: TopoDS_Shape, policy: IdentityPolicy = CANONICAL_POLICY) -> Self:
        return BrepOp(tessellate=(shape, policy))


# --- [OPERATIONS] -----------------------------------------------------------------------

async def apply(op: BrepOp, lane: LanePolicy) -> "RuntimeRail[BrepResult]":
    return await lane.offload(_dispatch, op)


def _built(builder: object) -> TopoDS_Shape:
    builder.Build()
    if not builder.IsDone():
        raise ValueError(f"brep builder {type(builder).__name__} reported IsDone() == False")
    return builder.Shape()


def _census(shape: TopoDS_Shape) -> Census:
    def extent(kind: TopAbs_ShapeEnum) -> int:
        carrier = TopTools_IndexedMapOfShape()
        TopExp.MapShapes_s(shape, kind, carrier)
        return carrier.Extent()

    v, e, f, s = (extent(kind) for kind in _CENSUS)
    return (v, e, f, s)


def _triangulation(shape: TopoDS_Shape) -> trimesh.Trimesh:
    def face_block(face_shape: TopoDS_Shape) -> tuple[np.ndarray, np.ndarray]:
        face, loc = TopoDS.Face_s(face_shape), TopLoc_Location()
        tri = BRep_Tool.Triangulation_s(face, loc)
        nodes = np.asarray([(lambda p: (p.X(), p.Y(), p.Z()))(tri.Node(i)) for i in range(1, tri.NbNodes() + 1)], dtype=np.float64)
        faces = np.asarray(
            [(lambda t: (t.Value(1) - 1, t.Value(2) - 1, t.Value(3) - 1))(tri.Triangle(i)) for i in range(1, tri.NbTriangles() + 1)],
            dtype=np.int64,
        )
        return nodes.reshape(-1, 3), faces.reshape(-1, 3)

    faces_map = TopTools_IndexedMapOfShape()
    TopExp.MapShapes_s(shape, TopAbs_ShapeEnum.TopAbs_FACE, faces_map)
    blocks = [face_block(faces_map.FindKey(i)) for i in range(1, faces_map.Extent() + 1)]
    offsets = np.cumsum([0, *(len(nodes) for nodes, _ in blocks[:-1])])
    return trimesh.Trimesh(
        vertices=np.vstack([nodes for nodes, _ in blocks]),
        faces=np.vstack([tris + off for (_, tris), off in zip(blocks, offsets, strict=True)]),
        process=True,
    )


def _evidence(kind: str, shape: TopoDS_Shape, mesh: trimesh.Trimesh | None) -> BrepResult:
    volume, area = GProp_GProps(), GProp_GProps()
    BRepGProp.VolumeProperties_s(shape, volume)
    BRepGProp.SurfaceProperties_s(shape, area)
    com = volume.CentreOfMass()
    watertight = None if mesh is None else bool(mesh.is_watertight)
    closure_gap = abs(float(volume.Mass()) - float(mesh.volume)) if watertight else None
    return BrepResult(
        shape,
        mesh,
        BrepReceipt(
            kind,
            not shape.IsNull(),
            float(volume.Mass()),
            float(area.Mass()),
            (com.X(), com.Y(), com.Z()),
            _census(shape),
            watertight,
            closure_gap,
        ),
    )


def _profile_face(profile: Profile, dist: float, join: JoinPolicy) -> TopoDS_Face:
    section = CrossSection([list(profile)]).offset(dist, _JOINS[join], 2.0, 1e-3).simplify(1e-6) if dist else CrossSection([list(profile)])
    contours = section.to_polygons()
    builder = BRepBuilderAPI_MakeFace(_wire(contours[0]))
    for hole in contours[1:]:
        builder.Add(_wire(hole))
    return builder.Face()


def _wire(contour: np.ndarray, z: float = 0.0) -> TopoDS_Wire:
    polygon = BRepBuilderAPI_MakePolygon()
    for x, y in contour:
        polygon.Add(gp_Pnt(float(x), float(y), z))
    polygon.Close()
    return polygon.Wire()


def _dispatch(op: BrepOp) -> BrepResult:
    match op:
        case BrepOp(tag="construct", construct=(verb, params)):
            return _evidence(f"construct.{verb}", _built(_PRIMITIVES[verb](params)), None)
        case BrepOp(tag="boolean", boolean=(shapes, verb)):
            algo = _BOOLEANS[verb]
            shape = next(iter(shapes))
            for tool in shapes[1:]:
                shape = _built(algo(shape, tool))
            return _evidence(f"boolean.{verb}", shape, None)
        case BrepOp(tag="offset", offset=(profile, verb, dist, height, join)):
            face = _profile_face(profile, dist, join)
            match verb:
                case OffsetVerb.EXTRUDE:
                    shape = _built(BRepPrimAPI_MakePrism(face, gp_Vec(0.0, 0.0, height)))
                case OffsetVerb.REVOLVE:
                    shape = _built(BRepPrimAPI_MakeRevol(face, gp_Ax1(gp_Pnt(), gp_Dir(0.0, 1.0, 0.0))))
                case OffsetVerb.LOFT:
                    loft = BRepOffsetAPI_ThruSections(True)
                    loft.AddWire(_wire(CrossSection([list(profile)]).to_polygons()[0]))
                    loft.AddWire(_wire(CrossSection([list(profile)]).offset(dist, _JOINS[join], 2.0, 1e-3).to_polygons()[0], height))
                    shape = _built(loft)
                case OffsetVerb.THICK:
                    solid = _built(BRepPrimAPI_MakePrism(face, gp_Vec(0.0, 0.0, height)))
                    shape = _built(BRepOffsetAPI_MakeThickSolid(solid, [], -dist, 1e-3))
                case unreachable:
                    assert_never(unreachable)
            return _evidence(f"offset.{verb}", shape, None)
        case BrepOp(tag="feature", feature=(target, verb, size)):
            feature, edges = _FEATURES[verb](target), TopTools_IndexedMapOfShape()
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

- [OCCT_COMMAND_PATTERN]: every `BRepPrimAPI_*`/`BRepAlgoAPI_*`/`BRepOffsetAPI_*`/`BRepFilletAPI_*` builder shares the `Build()`/`IsDone() -> bool`/`Shape() -> TopoDS_Shape` command pattern documented in the branch `cadquery-ocp` catalogue `[04]-[IMPLEMENTATION_LAW]` builder-pattern law on the cp312 companion, so `_built` folds the validity check once across every verb row rather than re-spelling it per arm. The catalogue confirms `BRepPrimAPI_MakeBox(dx,dy,dz)`, `MakeSphere(R)`, `MakeCylinder(R,H)`, `MakeCone`, `MakeTorus`, `MakePrism(face, gp_Vec)`, `MakeRevol(face, axis)`, the `BRepAlgoAPI_Fuse`/`Cut`/`Common`/`Section` 2-arg constructors yielding `Shape()`, the `BRepOffsetAPI_ThruSections`/`MakePipeShell`/`MakeThickSolid` loft/sweep/shell family, the `BRepBuilderAPI_MakeFace(wire)` plus `Add(wire)` hole-wire path and `BRepBuilderAPI_MakePolygon` closed-wire builder, `TopExp.MapShapes_s(shape, type, TopTools_IndexedMapOfShape)` with `Extent()` and one-based `FindKey(i)` as the one-pass sub-shape index (replacing the imperative `TopExp_Explorer` `More`/`Current`/`Next` walk for both census and per-edge feature accumulation), the static `TopoDS.Edge_s`/`Face_s` downcasts, `BRepMesh_IncrementalMesh(shape, deflection)`, and `BRepGProp.VolumeProperties_s`/`SurfaceProperties_s` populating a `GProp_GProps` with `Mass`/`CentreOfMass`. The fence commits to the catalogued 2-arg Boolean constructors and the `GProp` evidence read; the members resolved on the live catalogue rather than the fence are the five-arg `BRepMesh_IncrementalMesh(shape, deflection, relative, angle, parallel)` overload (shared with `mesh/cad.md` on the same seam), the `BRepFilletAPI_MakeFillet.Add(radius, edge)` / `MakeChamfer.Add(dist, edge)` per-edge accumulation, the `BRepOffsetAPI_MakeThickSolid(solid, faces_to_remove, offset, tol)` shell construction (empty removal list yields a uniform-thickness hollow solid), the `TopTools_IndexedMapOfShape.Extent`/`FindKey` index surface, and the `BRep_Tool.Triangulation_s(face, TopLoc_Location) -> Poly_Triangulation` plus `Poly_Triangulation.Node(i)`/`NbNodes()`/`Triangle(i)`/`NbTriangles()` one-based per-face mesh read — the standard OCCT face-triangulation extraction the `Tessellate` arm vectorizes into the `trimesh.Trimesh(vertices, faces, process=True)` constructor, the merge pass welding the per-face duplicate boundary nodes OCCT emits as independent blocks so the shared-edge connectivity closes and `is_watertight` reports the true solid verdict.
- [CROSSSECTION_WEAVE]: the `Offset` arm weaves three `manifold3d.CrossSection` capabilities through the OCCT face lift, confirmed against the branch `manifold3d` catalogue `[03]` CrossSection rows on the cp312 companion — `offset(delta, join_type, miter_limit, arc_tol)` exact polygon offset under the `_JOINS`-mapped `JoinType.Round`/`Miter`/`Square`/`Bevel` vocabulary, `simplify(epsilon)` collinear cull, and the multi-contour `to_polygons() -> list` egress — then `BRepBuilderAPI_MakePolygon`/`MakeFace` with per-hole `Add(wire)` lifts the outer contour and its islands into a planar holed `TopoDS_Face` the OCCT extrude/revolve/loft/thick-solid consumes, so one `Offset` arm integrates the `manifold3d` 2D-CSG kernel and the OCCT 3D-sweep kernel as a single rail rather than flat per-library calls. The catalogued `CrossSection.area()` is the profile-validity guard a future degenerate-profile rejection arm reads, not a current member of the offset leg. The 3D `manifold3d.Manifold` solid CSG is deliberately NOT reached here — exact B-rep Boolean is the OCCT `BRepAlgoAPI_*` arm, robust triangle-mesh Boolean is the `trimesh.boolean`→`manifold3d.Manifold` backend in `mesh/repair.md#MESH`, the two kernels stay one-per-owner.
- [OFFLOAD_AND_CLOSURE_EVIDENCE]: `apply` hands `_dispatch` to the runtime `execution/lanes#LANE` `LanePolicy.offload(kernel, *args)` (`.api/anyio.md` thread-interop `anyio.to_interpreter.run_sync` over PEP 734 subinterpreters, the no-pickle hop, degrading to `anyio.to_thread.run_sync` only where a cp315 build ships no runnable `concurrent.interpreters`, never the `to_process` pickle round-trip the lanes owner rejects), so the companion-band OCCT kernel runs off the event loop under the active OTel context the lane stitches, the lane importing neither OCCT nor `_dispatch`, and a worker-raised `ValueError`/`BrokenWorkerInterpreter` crosses the lane's one `async_boundary` conversion. When tessellation exists, `_triangulation` welds the per-face `Poly_Triangulation` blocks into one connected `trimesh.Trimesh` through the `process=True` merge-and-validate pass (`.api/trimesh.md` IMPLEMENTATION_LAW repair axis) — OCCT triangulates each face into an independent node block, so an unwelded vstack is a soup of edge-coincident duplicates on which `is_watertight` is structurally false and the `volume` divergence integral is undefined; the merge deduplicates the coincident boundary vertices and closes the shared-edge connectivity without re-triangulating the kernel's authoritative faces. `_evidence` then reads the `trimesh.Trimesh.is_watertight` cached property (`.api/trimesh.md` validity axis) and, only when it holds, records the absolute difference between the exact OCCT `GProp_GProps.Mass()` volume and the independent `trimesh.Trimesh.volume` as the `closure_gap`, so the receipt carries a kernel-vs-mesh agreement verdict that surfaces a deflection too coarse for the solid, and an open shell/section records `watertight=False` with a `None` gap rather than a single-source or spurious volume claim.

## [04]-[UPSTREAM]

- [WHEEL_BAND]: `cadquery-ocp 7.9.3.1.1` and `manifold3d` (cp310-cp314 wheels) both ride the `python_version<'3.15'` companion band the branch manifest gates — `cadquery-ocp` `<3.15,>=3.10` (no abi3, no cp315) alongside `manifold3d` and below the tighter `kiss-matcher` `<'3.13'` band. This owner is companion-band only on every arm: the OCCT construct/boolean/offset/feature/tessellate kernel and the `manifold3d.CrossSection` 2D leg both resolve on the Forge `python312` companion interpreter (`forge-companion-env`) the daemon hosts, the cp312 floor the native geometry cores share. The cp315 project venv carries neither wheel, so the whole `mesh/brep` owner is companion-band gated — unlike the `mesh/repair.md` codec arm (cp315-core), the B-rep owner has no cp315-core arm and rides the offload lane to the companion as one hand-off. `trimesh`/`numpy` are pure-Python/prebuilt-wheel admitted on both bands, so the `trimesh.Trimesh` tessellation carrier crosses the wire unchanged.

# [PY_GEOMETRY_MESH_BREP]

Exact B-rep evaluation — the OCCT-kernel companion the CAD-STEP hop, the mesh-algebra rail, and the IFC profile owner compose for parametric solid construction, robust Boolean algebra, offset/loft/sweep generation, feature operations, and watertight tessellation. `BrepOp` is ONE `@tagged_union` discriminating five operation kinds — `Construct`, `Boolean`, `Offset`, `Feature`, `Tessellate` — each carrying its inner verb as a closed `StrEnum` row rather than a parallel per-operation class, so a new primitive, set verb, offset mode, or feature is one verb row plus one `expression.Map` dispatch entry binding the OCCT `BRep*API` builder that owns it, never a new surface. Every OCCT `Make*` builder rides the identical command pattern (`Build` → `IsDone` → `Shape`), so the union folds that pattern once through `_built` over the `OcctBuilder` Protocol and reads the resulting `TopoDS_Shape` through the shared `GProp_GProps` evidence accumulator. The `Offset` arm weaves `manifold3d.CrossSection` polygon CSG into the OCCT loft/sweep/thick-solid leg — a 2D profile offsets and simplifies in the robust `manifold3d` 2D kernel, the offset contour and its holes egress through `to_polygons()` and lift through `BRepBuilderAPI_MakeFace.Add` into a planar holed `TopoDS_Face`, then extrude/revolve/loft/thick-solid into a 3D `TopoDS_Shape`, one rail integrating both kernels. The `Tessellate` arm meshes the result in place with `BRepMesh_IncrementalMesh` under the shared `IdentityPolicy` deflection and hands an in-memory `trimesh.Trimesh` back across the wire — ALL mesh-file/GLB encode defers to the data `MeshPayload` owner (`rasm.data.spatial.mesh`), and ALL scene/USD/GLTF/OBJ scene export defers to `artifacts` figures/scene; this owner writes no file. Every arm returns `BrepResult` pairing the evaluated `TopoDS_Shape` (and the optional tessellation) with `BrepReceipt` carrying the kind, the OCCT validity verdict, the `GProp` volume/area/centroid, the one-pass sub-shape census, the trimesh watertight verdict and the exact-vs-tessellated closure-gap cross-check when a triangulation exists, and the canonical `GeometrySubject` `mesh-algebra` literal so a CSG solid graduates through the one geometry rail. The whole companion-band kernel is CPU-bound, so `apply` is the offload-aware entry — it awaits `LanePolicy.offload(_dispatch, op)`, the one per-subinterpreter primitive that runs the kernel under PEP 734, stitches the active OTel context across the no-pickle hop, and converts a worker raise through its own `async_boundary` to return one `RuntimeRail[BrepResult]`, the lane importing neither OCCT nor the kernel.

## [01]-[INDEX]

- [01]-[BREP]: the five-case B-rep operation union over `cadquery-ocp` OCCT plus `manifold3d.CrossSection`, folded through one OCCT command-pattern dispatch and one `GProp`-plus-trimesh evidence read, resolved through the runtime `LanePolicy.offload` subinterpreter hop, returning a `RuntimeRail[BrepResult]` pairing an evaluated `TopoDS_Shape` with an optional in-memory `trimesh.Trimesh` tessellation.

## [02]-[BREP]

- Owner: `BrepOp` — the `@tagged_union` discriminating the five operation kinds; `ConstructVerb`/`BooleanVerb`/`OffsetVerb`/`FeatureVerb` the closed `StrEnum` rows selecting the inner OCCT builder so each kind is one row rather than three sibling classes; `JoinPolicy` the closed offset-join vocabulary mapping to `manifold3d.JoinType` through `_JOINS` so the join is a policy value, never a verb-keyed hardcode; `_PRIMITIVES`/`_BOOLEANS`/`_FEATURES`/`_JOINS` the `expression.Map` dispatch tables binding each verb to its OCCT builder factory so the command pattern is folded once and a missing verb is the `Map.try_find` `Nothing` the `BrepFault` rails rather than a bare `KeyError`; `OcctBuilder` the structural `Protocol` typing the `Build`/`IsDone`/`Shape` command pattern every `BRep*API` maker satisfies so `_built` reads a real surface rather than an erased `object`; `BrepFault` the typed `@tagged_union(Exception)` the kernel raises INTO the lane's `async_boundary` so a degenerate construction converts through the one `BoundaryFault` taxonomy rather than a domain `raise ValueError`; `BrepResult` the carrier pairing the evaluated `TopoDS_Shape` with the optional in-memory `trimesh.Trimesh` tessellation and `BrepReceipt`; `BrepReceipt` the typed receipt — itself a `ReceiptContributor` whose `contribute` yields the canonical `Iterable[Receipt]` stream (one emitted-phase `Receipt.of("mesh.brep", ("emitted", kind, facts))` row, never a bare `Receipt` the `@receipted` `_stream` fold cannot normalize) — carrying the kind label, the `IsNull` validity verdict, the `GProp` volume/area/centroid, the one-pass vertex/edge/face/solid census, the trimesh watertight verdict and exact-vs-tessellated closure gap (both `None` until tessellation), and the `GeometrySubject` literal defaulted to `mesh-algebra`.
- Cases: `BrepOp` cases `Construct(verb, params)` (primitive box/sphere/cylinder/cone/torus over `BRepPrimAPI_*`), `Boolean(shapes, verb)` (n-ary fuse/cut/common/section over the robust `BRepAlgoAPI_*` BOPAlgo operators driven by `SetArguments`/`SetTools`), `Offset(profile, verb, dist, height, join)` (the `manifold3d.CrossSection` 2D-offset-simplify leg lifted through `BRepBuilderAPI_MakeFace.Add` then extruded/revolved/lofted/thickened over `BRepPrimAPI`/`BRepOffsetAPI`), `Feature(shape, verb, size)` (fillet/chamfer over `BRepFilletAPI_*`), and `Tessellate(shape, policy)` (the in-place `BRepMesh_IncrementalMesh` triangulation extracted to an in-memory `trimesh.Trimesh`) — matched by `match`/`assert_never`, each binding the OCCT builder family that owns the kind through the `Map` dispatch tables. Linear extrusion/revolution of a profile is the `Offset` arm's `EXTRUDE`/`REVOLVE` row, not a duplicate `Construct` primitive — `MakePrism`/`MakeRevol` are reached once, through the profile leg. The `Boolean.section` row yields a `TopoDS` wire/edge result the consumer re-feeds as a profile; the daemon and CAD hop never re-discriminate the operation past this owner.
- Entry: `apply` is `async` — it awaits `LanePolicy.offload(_dispatch, op)`, the runtime's one CPU-kernel subinterpreter primitive (distinct from `drain`, which mints a `DrainReceipt`), returning the `RuntimeRail[BrepResult]` the lane resolves, so the whole companion-band kernel rides one PEP 734 subinterpreter hop under the active OTel context and the lane imports neither OCCT nor the kernel. `_dispatch` resolves the inner verb through the `Map` dispatch tables, invokes the builder factory, calls `_built` (the one `Build`/`IsDone`/`Shape` fold over every `OcctBuilder`, raising a `BrepFault` the lane's `async_boundary` converts to a `BoundaryFault` when `IsDone()` is false so a degenerate construction never emits a phantom solid), and reads the result through `_evidence`. The `Construct` arm builds the primitive from the typed `params` tuple; the `Boolean` arm feeds the head shape to `SetArguments` and the tail to `SetTools` for one robust n-ary `Build`; the `Offset` arm runs the `manifold3d.CrossSection` offset/simplify leg, lifts the offset contour and its holes through `BRepBuilderAPI_MakeFace.Add` into a planar holed `TopoDS_Face`, and extrudes/revolves/lofts/thickens it through the OCCT family; the `Feature` arm applies the fillet/chamfer over the edges indexed by `TopExp.MapShapes_s`; the `Tessellate` arm meshes in place and extracts vertices/triangles into a `trimesh.Trimesh` carried back in `BrepResult.mesh`. The evaluated `TopoDS_Shape` rides back in `BrepResult.shape`; any persisted GLB/STEP encode is a downstream call — `MeshPayload` for the triangulation, the CAD-STEP `StepBridge` for the B-rep GLB, `artifacts` figures/scene for a visualization scene — never an arm here.
- Auto: every OCCT `Make*` builder shares the command pattern (`Build`/`IsDone`/`Shape`), so `_built` folds it once over the `OcctBuilder` Protocol and every verb row reuses it instead of re-spelling the check; `BRepAlgoAPI_Fuse`/`Cut`/`Common`/`Section` are the robust 2019+ BOPAlgo Boolean kernel (the legacy `BRepAlgo_*` path is the deleted alternative) and are n-ary operators — the binary `(S1, S2)` ctor is sugar over `SetArguments`/`SetTools` taking a `TopTools_ListOfShape`, so the `Boolean` arm appends the head to one list and the tail to another, runs one `Build`, and reads the single robust result rather than rebuilding the kernel N-1 times in a pairwise fold; the operands require triangulation absent, so the Boolean arm precedes any tessellate; the `Offset` arm weaves three `manifold3d.CrossSection` capabilities through one OCCT face lift — `_sections` resolves the `_JOINS`-mapped `JoinType` once through `Map.try_find` (never a raw `_JOINS[join]` `KeyError`) and returns the `(base, offset)` section pair the EXTRUDE/REVOLVE/THICK arms lift through `_profile_face` and the LOFT arm reads as its two `AddWire` rings, so the 2D leg runs once per profile: `offset(delta, join_type, miter_limit, circular_segments)` exact polygon offset, `simplify(epsilon)` collinear-vertex cull, and the multi-contour `to_polygons()` egress feeding the `BRepBuilderAPI_MakeFace.Add` hole-wire accumulation so a profile with islands lifts as one holed planar face rather than an outer-loop-only approximation; the `THICK` row constructs the hollow solid through the no-arg `BRepOffsetAPI_MakeThickSolid()` ctor driven by `MakeThickSolidBySimple(solid, -dist)` (the uniform-offset shell — the bare-4-arg ctor is the deleted phantom, the operation being `MakeThickSolidBySimple`/`MakeThickSolidByJoin` per the OCP builder law); `BRepMesh_IncrementalMesh(shape, deflection, relative, angle, parallel)` mutates the shape's stored triangulation in place from `IdentityPolicy.deflection`/`angle_tolerance`, and `_triangulation` reads `BRep_Tool.Triangulation_s` per face, vectorizing the `(nodes, triangles)` blocks into one `(V,3)`/`(F,3)` pair through `np.cumsum` index offsets under `process=True` so the trimesh merge-and-validate pass welds the per-face duplicate boundary vertices the kernel emits as independent node blocks — without the weld every face block is an island and `is_watertight` is structurally false on a closed solid, the merge deduplicates coincident vertices without re-triangulating the faces, so the weld is the precondition for the closure verdict rather than an optional pass; `BRepGProp.VolumeProperties_s`/`SurfaceProperties_s` populate two `GProp_GProps` accumulators exposing `Mass`/`CentreOfMass` as the exact per-solid evidence, `TopExp.MapShapes_s(shape, kind, TopTools_IndexedMapOfShape)` indexes the vertex/edge/face/solid census in one pass per kind through `Extent`, and when a tessellation exists `_evidence` reads the welded `is_watertight` verdict and cross-checks the exact `GProp` volume against `trimesh.Trimesh.volume` only when that verdict holds — the divergence-theorem `volume` is meaningless on an open surface, so an open shell or section result records `watertight=False` with a `None` closure gap rather than a spurious agreement number, and a closed solid carries the independent kernel-vs-mesh closure verdict rather than a single-source claim.
- Receipt: `BrepReceipt.contribute` is the `ReceiptContributor` method yielding the `Iterable[Receipt]` stream — one emitted-phase `Receipt.of("mesh.brep", ("emitted", kind, facts))` row folding the kind label, the validity verdict, the `GProp` volume/area, the centroid, the census, the trimesh watertight verdict, and the closure gap through the runtime stream, the `yield` shape (never a bare `Receipt`) the contract the canonical `ReceiptContributor.contribute(self) -> Iterable[Receipt]` port and the `@receipted` aspect's `_stream` normalizer require — the `facts` a `dict[str, object]` of native scalars the receipt owner's `enc_hook=repr` renderer serializes, never a stringly `str()`-coerced map, and no parallel receipt rail; the evaluated solid carries the geometry `GraduationReceipt` subject `mesh-algebra` — the canonical `GeometrySubject` literal (`rasm.compute.graduation.handoff#GeometrySubject`, never a bare `str`) so a CSG/feature/offset result graduates through the one geometry rail the `mesh/repair.md#MESH` boolean arm and the `graph/algebra.md#ALGEBRA` mesh fold share.
- Packages: `cadquery-ocp` (`OCP.BRepPrimAPI.BRepPrimAPI_MakeBox`/`MakeSphere`/`MakeCylinder`/`MakeCone`/`MakeTorus`/`MakePrism`/`MakeRevol`, `OCP.BRepAlgoAPI.BRepAlgoAPI_Fuse`/`Cut`/`Common`/`Section` with `SetArguments`/`SetTools`/`Build`/`Shape`, `OCP.BRepOffsetAPI.BRepOffsetAPI_ThruSections`/`MakePipeShell`/`MakeThickSolid` with `MakeThickSolidBySimple`, `OCP.BRepFilletAPI.BRepFilletAPI_MakeFillet`/`MakeChamfer`, `OCP.BRepBuilderAPI.BRepBuilderAPI_MakePolygon`/`MakeFace`, `OCP.BRepMesh.BRepMesh_IncrementalMesh`, `OCP.BRep.BRep_Tool`, `OCP.BRepGProp.BRepGProp`, `OCP.GProp.GProp_GProps`, `OCP.TopExp.TopExp`, `OCP.TopTools.TopTools_IndexedMapOfShape`/`TopTools_ListOfShape`, `OCP.TopAbs.TopAbs_ShapeEnum`, `OCP.TopoDS.TopoDS`/`TopoDS_Shape`/`TopoDS_Face`, `OCP.TopLoc.TopLoc_Location`, `OCP.gp.gp_Pnt`/`gp_Vec`/`gp_Ax1`/`gp_Dir`), `manifold3d` (`CrossSection`/`JoinType` for the 2D profile offset/simplify leg, never a 3D `Manifold` CSG owner — that is the `trimesh.boolean` backend in `mesh/repair.md#MESH`), `trimesh` (`Trimesh` the in-memory triangulation carrier plus its cached `is_watertight`/`volume` properties feeding the closure cross-check), `numpy` (`asarray`/`vstack`/`cumsum` vectorizing the per-face node/triangle blocks into one `(V,3)`/`(F,3)` pair), `expression` (`tag`/`case`/`tagged_union` the `BrepOp`/`BrepFault` unions, `Map`/`Map.of_seq`/`Map.try_find` the `_PRIMITIVES`/`_BOOLEANS`/`_FEATURES`/`_JOINS` dispatch tables, `Option.default_with` the missing-verb fold raising `BrepFault`), `msgspec` (`Struct(frozen=True)` the `BrepResult` carrier — tracked since it references the OCCT `TopoDS_Shape` and the `trimesh.Trimesh` container objects — and `Struct(frozen=True, gc=False)` the leaf-scalar `BrepReceipt`, whose fields are all native `str`/`bool`/`float`/tuple so the GC need never trace it, matching the sibling `mesh/repair.md#MESH` `MeshRepairReceipt` convention), runtime (`IdentityPolicy`/`CANONICAL_POLICY`/`RuntimeRail`/`LanePolicy`/`Receipt`/`ReceiptContributor`).
- Growth: a new primitive is one `ConstructVerb` row plus one `_PRIMITIVES` factory binding the `BRepPrimAPI_*` builder; a new set verb is one `BooleanVerb` row plus one `_BOOLEANS` entry; a new offset/loft mode is one `OffsetVerb` row binding its `BRepOffsetAPI_*` builder; a spine-following `SWEEP` verb is the imported `BRepOffsetAPI_MakePipeShell` landing site that admits the same turn a spine wire enters the `Offset` payload as a new field, so the verb is staged behind a real input rather than aliased to the linear `EXTRUDE` prism it cannot distinguish without a spine; a new feature is one `FeatureVerb` row plus one `_FEATURES` entry binding `BRepFilletAPI_*`; a new join geometry is one `JoinPolicy` row plus one `_JOINS` entry; the deflection knob is the shared `IdentityPolicy` already keyed by the daemon cache. The CAD-STEP `StepBridge` re-uses the `Tessellate` arm's `_triangulation` read rather than re-implementing per-face node extraction, and an `ifc/structural` profile composes `Construct`/`Offset` to evaluate an `IfcProfileDef` cross-section into a `TopoDS_Face` before the `numpy` section integral. Zero new surface, no parallel per-operation class family.
- Boundary: this owner is the exact-kernel evaluator — it constructs, combines, offsets, features, and triangulates B-rep solids and hands the result across the wire as an in-memory `TopoDS_Shape` plus optional `trimesh.Trimesh`. No mesh-file/GLB decode/encode (the data `MeshPayload` owner `rasm.data.spatial.mesh` holds the three-engine `trimesh`/`meshio`/`rhino3dm` codec, the `MeshBackend.of` router, and the GLB `preview`); no scene/USD/GLTF/OBJ scene export (that is `artifacts` figures/scene); no STEP/IGES read-to-GLB hop (that is the sibling `mesh/cad.md#BRIDGE` `StepBridge` over the XCAF CAF reader — this owner evaluates B-rep already in memory, the bridge reads source bytes); no triangle-soup repair or `trimesh.boolean` mesh CSG (that is `mesh/repair.md#MESH` — exact OCCT B-rep Boolean is here, robust triangle-mesh Boolean is there, the two kernels are distinct rows on the two owners, never a shared third surface). The deleted forms: a per-operation class family (`BoxOp`/`FuseOp`/`FilletOp`) over the `BrepOp` discriminant, a `make_box`/`make_sphere` factory proliferation over the `ConstructVerb` row, a duplicate `Construct.prism`/`Construct.revol` primitive racing the `Offset` extrude/revolve leg, a verb-keyed `JoinType` hardcode racing the `JoinPolicy` row, a raw `_JOINS[join]` `Map.__getitem__` raising `KeyError` in the loft arm where `_sections` resolves the join once through `try_find` and folds the miss to `BrepFault`, a duplicate `CrossSection([list(profile)])` rebuilt-and-re-offset per loft ring where the `_sections` `(base, offset)` pair runs the `manifold3d` 2D leg once and both rings read it, a `contribute` returning a bare `Receipt` where the canonical `ReceiptContributor.contribute(self) -> Iterable[Receipt]` port and the `@receipted` `_stream` normalizer require a yielded stream, an IIFE `(lambda p: ...)(tri.Node(i))` wrapped per node/triangle in the triangulation comprehension where the walrus binds the node once, a pairwise binary-ctor Boolean fold rebuilding the BOPAlgo kernel N-1 times where one `SetArguments`/`SetTools` build is the robust n-ary form, a bare-4-arg `BRepOffsetAPI_MakeThickSolid(solid, [], -dist, tol)` ctor where the operation is `MakeThickSolidBySimple`/`MakeThickSolidByJoin`, an imperative `TopExp_Explorer` `while`-loop accumulating the census where `MapShapes_s` indexes it in one pass, a `mass` field duplicating the `volume` `GProp` read, a hand-rolled OCCT command-pattern check re-spelled per arm rather than the `_built` fold over the `OcctBuilder` Protocol, an untyped `builder: object` carrier erasing the command pattern, a domain `raise ValueError` where the typed `BrepFault` converts through the lane's `async_boundary`, a plain `dict` dispatch table raising `KeyError` on a missing verb where the `expression.Map` `try_find` Option-folds it, a stringly `dict[str, str]` receipt facts map where the native-scalar `dict[str, object]` rides the `enc_hook=repr` renderer, a 4-positional-arg `Receipt.of("emitted", "mesh.brep", kind, facts)` where the owner's signature is `Receipt.of(owner, (phase, subject, facts))`, an `execution.lanes` import path where the canonical owner is `rasm.runtime.lanes`, a second 3D `manifold3d.Manifold` CSG owner duplicating the `mesh/repair.md` boolean backend, a synchronous `apply` blocking the companion event loop on the CPU kernel where the offload lane isolates it, ANY `Codec`/`load`/`export`/`write` mesh-file or scene arm re-deriving the `MeshPayload`/`artifacts` seam, and the retired conda-only `pythonocc-core` `OCC.Core.*` path.

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
from OCP.TopTools import TopTools_IndexedMapOfShape, TopTools_ListOfShape

from rasm.compute.graduation.handoff import GeometrySubject
from rasm.runtime.content_identity import CANONICAL_POLICY, IdentityPolicy
from rasm.runtime.faults import RuntimeRail
from rasm.runtime.lanes import LanePolicy
from rasm.runtime.receipts import Receipt, ReceiptContributor

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


# the command pattern every OCCT `BRep*API` maker satisfies — `_built` reads this surface
# rather than an erased `object`, so a maker missing a leg is a static gap not a runtime miss.
class OcctBuilder(Protocol):
    def Build(self) -> None: ...
    def IsDone(self) -> bool: ...
    def Shape(self) -> TopoDS_Shape: ...


# --- [ERRORS] ---------------------------------------------------------------------------


# the degenerate-construction failures the kernel raises INTO the lane's `async_boundary` so the
# one `_convert` rail folds them onto `RuntimeRail`; never a domain `raise ValueError` the lane re-wraps.
@tagged_union(frozen=True)
class BrepFault(Exception):
    tag: Literal["not_done", "empty_profile", "unknown_verb"] = tag()
    not_done: str = case()
    empty_profile: str = case()
    unknown_verb: str = case()


# --- [TABLES] ---------------------------------------------------------------------------

# one factory row per verb: the kernel resolves the builder through `Map.try_find` once rather than a
# plain `dict` raising `KeyError`, so an unmapped verb is the `unknown_verb` fault the boundary rails.
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
])

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

# fixed 4-arity (not variadic) so the `vertex, edge, face, solid` unpack is statically total and
# the `(extent, extent, extent, extent)` build unifies with the `Census` 4-tuple.
_CENSUS: Final[tuple[TopAbs_ShapeEnum, TopAbs_ShapeEnum, TopAbs_ShapeEnum, TopAbs_ShapeEnum]] = (
    TopAbs_ShapeEnum.TopAbs_VERTEX,
    TopAbs_ShapeEnum.TopAbs_EDGE,
    TopAbs_ShapeEnum.TopAbs_FACE,
    TopAbs_ShapeEnum.TopAbs_SOLID,
)


# --- [MODELS] ---------------------------------------------------------------------------

class BrepReceipt(Struct, frozen=True, gc=False):
    kind: str
    valid: bool
    volume: float
    area: float
    centroid: tuple[float, float, float]
    census: Census
    watertight: bool | None
    closure_gap: float | None
    subject: GeometrySubject = "mesh-algebra"

    # the `ReceiptContributor` port yields an `Iterable[Receipt]` (not a bare `Receipt`) so the
    # `@receipted` aspect's `_stream` normalizes a multi-row contributor through one fold.
    def contribute(self) -> Iterable[Receipt]:
        v, e, f, s = self.census
        facts: dict[str, object] = {
            "valid": self.valid,
            "volume": self.volume,
            "area": self.area,
            "centroid": self.centroid,
            "census": f"v{v}/e{e}/f{f}/s{s}",
            "watertight": self.watertight,
            "closure_gap": self.closure_gap,
            "subject": self.subject,
        }
        yield Receipt.of("mesh.brep", ("emitted", self.kind, facts))


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
        # `Poly_Triangulation` is one-based; each `Triangle.Value(j)` returns one-based node indices.
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


def _shape_list(shapes: Shapes) -> TopTools_ListOfShape:
    carrier = TopTools_ListOfShape()
    for shape in shapes:
        carrier.Append(shape)
    return carrier


def _boolean(shapes: Shapes, verb: BooleanVerb) -> TopoDS_Shape:
    op = _BOOLEANS.try_find(verb).default_with(lambda: _raise(BrepFault(unknown_verb=verb)))()
    op.SetArguments(_shape_list(shapes[:1]))
    op.SetTools(_shape_list(shapes[1:]))
    return _built(op)


# one offset resolution: the join folds through `try_find` once (never a raw `_JOINS[join]`
# `KeyError`), and the `(base, offset)` section pair is reused by both the face lift and the loft
# rails so the `manifold3d` 2D leg runs once per profile rather than re-built per offset arm.
def _sections(profile: Profile, dist: float, join: JoinPolicy) -> tuple[CrossSection, CrossSection]:
    if not profile:
        raise BrepFault(empty_profile="offset")
    base = CrossSection([list(profile)])
    join_type = _JOINS.try_find(join).default_with(lambda: _raise(BrepFault(unknown_verb=join)))
    offset = base.offset(dist, join_type, 2.0, 0).simplify(1e-6) if dist else base
    return base, offset


def _profile_face(section: CrossSection) -> TopoDS_Face:
    outer, *holes = section.to_polygons()
    builder = BRepBuilderAPI_MakeFace(_wire(outer))
    for hole in holes:
        builder.Add(_wire(hole))
    return builder.Face()


def _wire(contour: np.ndarray, z: float = 0.0) -> TopoDS_Wire:
    polygon = BRepBuilderAPI_MakePolygon()
    for x, y in contour:
        polygon.Add(gp_Pnt(float(x), float(y), z))
    polygon.Close()
    return polygon.Wire()


# expression-form raise so each `Option.default_with` table-miss fold stays a one-expression thunk
# unifying with that site's expected type; the raised `BrepFault` converts on the enclosing lane
# `async_boundary`.
def _raise[T](fault: BrepFault) -> T:
    raise fault


def _dispatch(op: BrepOp) -> BrepResult:
    match op:
        case BrepOp(tag="construct", construct=(verb, params)):
            factory = _PRIMITIVES.try_find(verb).default_with(lambda: _raise(BrepFault(unknown_verb=verb)))
            return _evidence(f"construct.{verb}", _built(factory(params)), None)
        case BrepOp(tag="boolean", boolean=(shapes, verb)):
            return _evidence(f"boolean.{verb}", _boolean(shapes, verb), None)
        case BrepOp(tag="offset", offset=(profile, verb, dist, height, join)):
            base, offset = _sections(profile, dist, join)
            match verb:
                case OffsetVerb.EXTRUDE:
                    shape = _built(BRepPrimAPI_MakePrism(_profile_face(offset), gp_Vec(0.0, 0.0, height)))
                case OffsetVerb.REVOLVE:
                    shape = _built(BRepPrimAPI_MakeRevol(_profile_face(offset), gp_Ax1(gp_Pnt(), gp_Dir(0.0, 1.0, 0.0))))
                case OffsetVerb.LOFT:
                    loft = BRepOffsetAPI_ThruSections(True)
                    loft.AddWire(_wire(base.to_polygons()[0]))
                    loft.AddWire(_wire(offset.to_polygons()[0], height))
                    shape = _built(loft)
                case OffsetVerb.THICK:
                    solid = _built(BRepPrimAPI_MakePrism(_profile_face(offset), gp_Vec(0.0, 0.0, height)))
                    thick = BRepOffsetAPI_MakeThickSolid()
                    thick.MakeThickSolidBySimple(solid, -dist)
                    shape = _built(thick)
                case unreachable:
                    assert_never(unreachable)
            return _evidence(f"offset.{verb}", shape, None)
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

- [OCCT_COMMAND_PATTERN]: every `BRepPrimAPI_*`/`BRepAlgoAPI_*`/`BRepOffsetAPI_*`/`BRepFilletAPI_*` builder shares the `Build()`/`IsDone() -> bool`/`Shape() -> TopoDS_Shape` command pattern documented in the `cadquery-ocp` catalogue `[03]-[ENTRYPOINTS]` shape-construction rows and `[04]-[IMPLEMENTATION_LAW]` builder-pattern law on the cp313-reflected wheel, so `_built` folds the validity check once over the `OcctBuilder` Protocol rather than re-spelling it per arm and never reads an erased `object`. The catalogue confirms `BRepPrimAPI_MakeBox(dx,dy,dz)`, `MakeSphere(R)`, `MakeCylinder(R,H)`, `MakeCone`, `MakeTorus`, `MakePrism(face, gp_Vec)`, `MakeRevol(face, axis)`, the `BRepBuilderAPI_MakeFace(wire)` plus `Add(wire)` hole-wire path and `BRepBuilderAPI_MakePolygon` closed-wire builder, `TopExp.MapShapes_s(shape, type, TopTools_IndexedMapOfShape)` with `Extent()` and one-based `FindKey(i)` as the one-pass sub-shape index (replacing the imperative `TopExp_Explorer` `More`/`Current`/`Next` walk for both census and per-edge feature accumulation), the static `TopoDS.Edge_s`/`Face_s` downcasts, `BRepMesh_IncrementalMesh(shape, deflection)`, and `BRepGProp.VolumeProperties_s`/`SurfaceProperties_s` populating a `GProp_GProps` with `Mass`/`CentreOfMass`. The catalogue `[03]`/`[04]` BOPAlgo law corrects two prior phantoms: the `BRepAlgoAPI_*` operators are n-ary — the binary `(S1, S2)` ctor is sugar over `SetArguments(TopTools_ListOfShape)`/`SetTools(TopTools_ListOfShape)` then `Build()`/`Shape()`, so the `Boolean` arm appends the head to one list and the tail to another for one robust build rather than a pairwise binary-ctor fold rebuilding the kernel N-1 times; and `BRepOffsetAPI_MakeThickSolid` has a NO-arg ctor driven by `MakeThickSolidBySimple(S, Offset)` or `MakeThickSolidByJoin(S, ClosingFaces: TopTools_ListOfShape, Offset, Tol, ...)`, never a 4-positional-arg ctor, so the `THICK` row constructs the empty maker and calls `MakeThickSolidBySimple(solid, -dist)` for the uniform-offset shell. The members resolved on the catalogue rather than a prior fence are the five-arg `BRepMesh_IncrementalMesh(shape, deflection, relative, angle, parallel)` overload (shared with `mesh/cad.md` on the same seam), the `BRepFilletAPI_MakeFillet.Add(radius, edge)` / `MakeChamfer.Add(dist, edge)` per-edge accumulation, the `TopTools_ListOfShape.Append` collection feeding the n-ary Boolean, the `TopTools_IndexedMapOfShape.Extent`/`FindKey` index surface, and the `BRep_Tool.Triangulation_s(face, TopLoc_Location) -> Poly_Triangulation` plus `Poly_Triangulation.Node(i)`/`NbNodes()`/`Triangle(i)`/`NbTriangles()` one-based per-face mesh read — the standard OCCT face-triangulation extraction the `Tessellate` arm vectorizes into the `trimesh.Trimesh(vertices, faces, process=True)` constructor, the merge pass welding the per-face duplicate boundary nodes OCCT emits as independent blocks so the shared-edge connectivity closes and `is_watertight` reports the true solid verdict.
- [CROSSSECTION_WEAVE]: the `Offset` arm weaves three `manifold3d.CrossSection` capabilities through the OCCT face lift, confirmed against the `manifold3d` catalogue `[03]-[ENTRYPOINTS]` CrossSection-operations rows on the cp313-reflected stub — `offset(delta, join_type=Round, miter_limit=2.0, circular_segments=0)` exact polygon offset under the `_JOINS`-mapped `JoinType.Round`/`Miter`/`Square`/`Bevel` vocabulary (`circular_segments=0` deferring to the global segment config, the live signature carrying no `arc_tol` argument), `simplify(epsilon=1e-6)` collinear cull, and the multi-contour `to_polygons() -> list[(N,2)]` egress — then `BRepBuilderAPI_MakePolygon`/`MakeFace` with per-hole `Add(wire)` lifts the outer contour and its islands into a planar holed `TopoDS_Face` the OCCT extrude/revolve/loft/thick-solid consumes, so one `Offset` arm integrates the `manifold3d` 2D-CSG kernel and the OCCT 3D-sweep kernel as a single rail rather than flat per-library calls. The catalogued `CrossSection.area()` is the profile-validity guard a future degenerate-profile rejection arm reads; the present arm gates only on an empty `profile` tuple, raising `BrepFault(empty_profile=...)` before the kernel. The 3D `manifold3d.Manifold` solid CSG is deliberately NOT reached here — exact B-rep Boolean is the OCCT `BRepAlgoAPI_*` arm, robust triangle-mesh Boolean is the `trimesh.boolean`→`manifold3d.Manifold` backend in `mesh/repair.md#MESH`, the two kernels stay one-per-owner.
- [OFFLOAD_AND_CLOSURE_EVIDENCE]: `apply` hands `_dispatch` to the runtime `execution/lanes#LANE` `LanePolicy.offload(kernel, *args) -> RuntimeRail[T]` (`.api/anyio.md` thread-interop `anyio.to_interpreter.run_sync` over PEP 734 subinterpreters, the no-pickle hop, degrading to `anyio.to_thread.run_sync` only where a cp315 build ships no runnable `concurrent.interpreters`, never the `to_process` pickle round-trip the lanes owner rejects), so the companion-band OCCT kernel runs off the event loop under the active OTel context the lane stitches through its `traced_kernel` shim, the lane importing neither OCCT nor `_dispatch`, and a worker-raised `BrepFault`/`BrokenWorkerInterpreter`/deadline `TimeoutError` crosses the lane's one `async_boundary` conversion — so the kernel's `_built`/`_boolean`/`_sections` raises are typed boundary-kernel control flow the lane folds onto `RuntimeRail`, never a bare `ValueError` escaping the hop. When tessellation exists, `_triangulation` welds the per-face `Poly_Triangulation` blocks into one connected `trimesh.Trimesh` through the `process=True` merge-and-validate pass (`.api/trimesh.md` `[05]` conditioning axis) — OCCT triangulates each face into an independent node block, so an unwelded vstack is a soup of edge-coincident duplicates on which `is_watertight` is structurally false and the `volume` divergence integral is undefined; the merge deduplicates the coincident boundary vertices and closes the shared-edge connectivity without re-triangulating the kernel's authoritative faces. `_evidence` then reads the `trimesh.Trimesh.is_watertight` cached property (`.api/trimesh.md` `[02]` manifold-validity row) and, only when it holds, records the absolute difference between the exact OCCT `GProp_GProps.Mass()` volume and the independent `trimesh.Trimesh.volume` as the `closure_gap`, so the receipt carries a kernel-vs-mesh agreement verdict that surfaces a deflection too coarse for the solid, and an open shell/section records `watertight=False` with a `None` gap rather than a single-source or spurious volume claim.

## [04]-[UPSTREAM]

- [WHEEL_BAND]: `cadquery-ocp 7.9.3.1.1` and `manifold3d 3.5.1` both ride the `python_version<'3.15'` companion band the branch manifest gates — `cadquery-ocp` `<3.15,>=3.10` (no abi3, no cp315, hard-linking the exact-pinned `vtk==9.6.2` plus `cadquery-ocp-proxy==7.9.3.1.1`) alongside `manifold3d` (cp310-cp314 wheels, no abi3) and below the tighter `kiss-matcher` `<'3.13'` band. This owner is companion-band only on every arm: the OCCT construct/boolean/offset/feature/tessellate kernel and the `manifold3d.CrossSection` 2D leg both resolve on the Forge `python312` companion interpreter (`forge-companion-env`) the daemon hosts, the cp312 floor the native geometry cores share. The cp315 project venv carries neither wheel, so the whole `mesh/brep` owner is companion-band gated — unlike the `mesh/repair.md` codec arm (cp315-core), the B-rep owner has no cp315-core arm and rides the offload lane to the companion as one hand-off. `trimesh`/`numpy`/`expression`/`msgspec` are pure-Python/prebuilt-wheel admitted on both bands, so the `trimesh.Trimesh` tessellation carrier crosses the wire unchanged.

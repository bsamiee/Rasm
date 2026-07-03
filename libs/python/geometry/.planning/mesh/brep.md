# [PY_GEOMETRY_MESH_BREP]

Exact B-rep evaluation — the OCCT-kernel companion the CAD-STEP hop, the mesh-algebra rail, and the IFC profile owner compose for parametric solid construction, robust Boolean algebra, offset/loft/sweep generation, feature operations, and watertight tessellation. `BrepOp` is ONE `@tagged_union` discriminating five operation kinds — `Construct`, `Boolean`, `Offset`, `Feature`, `Tessellate` — each carrying its inner verb as a closed `StrEnum` row rather than a parallel per-operation class, so a new primitive, set verb, offset mode, or feature is one verb row plus one `expression.Map` dispatch entry binding the OCCT `BRep*API` builder that owns it, never a new surface. Every OCCT `Make*` builder rides the identical command pattern (`Build` → `IsDone` → `Shape`), so the union folds that pattern once through `_built` over the `OcctBuilder` Protocol and reads the resulting `TopoDS_Shape` through the shared `GProp_GProps` evidence accumulator. The `Offset` arm weaves `manifold3d.CrossSection` polygon CSG into the OCCT loft/sweep/thick-solid leg — a 2D profile offsets and simplifies in the robust `manifold3d` 2D kernel, the offset contour and its holes egress through `to_polygons()` and lift through `BRepBuilderAPI_MakeFace.Add` into a planar holed `TopoDS_Face`, then extrude/revolve/loft/thick-solid into a 3D `TopoDS_Shape`, one rail integrating both kernels. The `Tessellate` arm meshes the result in place with `BRepMesh_IncrementalMesh` under the shared `IdentityPolicy` deflection and hands an in-memory `trimesh.Trimesh` back across the wire — ALL mesh-file/GLB encode defers to the data `MeshPayload` owner (`rasm.data.spatial.mesh`), and ALL scene/USD/GLTF/OBJ scene export defers to `artifacts` figures/scene; this owner writes no file. Every arm returns `BrepResult` — the `ReceiptContributor` carrier pairing the evaluated `TopoDS_Shape` (and the optional tessellation) with `BrepReceipt` carrying the kind, the OCCT validity verdict, the `GProp` volume/area/centroid, the one-pass sub-shape census, the trimesh watertight verdict and the exact-vs-tessellated closure-gap cross-check when a triangulation exists, and the canonical `GeometrySubject` `mesh-algebra` literal so a CSG solid graduates through the one geometry rail. The whole worker kernel is CPU-bound, so `apply` is the offload-aware entry — it awaits `LanePolicy.offload(_dispatch, op)`, the one per-subinterpreter primitive that runs the kernel under PEP 734, stitches the active OTel context across the no-pickle hop, and converts a worker raise through its own `async_boundary`, then `.map`s the `Ok` `BrepResult` through the `@receipted(REDACTION)` `_emit` builder so the typed receipt streams on the success path through the canonical `Signals.emit` decorator rail rather than an inline emit threaded through the kernel, returning one `RuntimeRail[BrepResult]`, the lane importing neither OCCT nor the kernel.

## [01]-[INDEX]

- [01]-[BREP]: the five-case B-rep operation union over `cadquery-ocp` OCCT plus `manifold3d.CrossSection`, folded through one OCCT command-pattern dispatch and one `GProp`-plus-trimesh evidence read, resolved through the runtime `LanePolicy.offload` subinterpreter hop, returning a `RuntimeRail[BrepResult]` pairing an evaluated `TopoDS_Shape` with an optional in-memory `trimesh.Trimesh` tessellation.

## [02]-[BREP]

- Owner: `BrepOp` — the `@tagged_union` discriminating the five operation kinds; `ConstructVerb`/`BooleanVerb`/`OffsetVerb`/`FeatureVerb` the closed `StrEnum` rows selecting the inner OCCT builder so each kind is one row rather than three sibling classes; `JoinPolicy` the closed offset-join vocabulary mapping to `manifold3d.JoinType` through `_JOINS` so the join is a policy value, never a verb-keyed hardcode; `_PRIMITIVES`/`_BOOLEANS`/`_FEATURES`/`_JOINS` the `expression.Map` dispatch tables binding each verb to its OCCT builder factory so the command pattern is folded once and a missing verb is the `Map.try_find` `Nothing` the `BrepFault` rails rather than a bare `KeyError`; `OcctBuilder` the structural `Protocol` typing the `Build`/`IsDone`/`Shape` command pattern every `BRep*API` maker satisfies so `_built` reads a real surface rather than an erased `object`; `BrepFault` the typed `@tagged_union(Exception)` the kernel raises INTO the lane's `async_boundary` so a degenerate construction converts through the one `BoundaryFault` taxonomy rather than a domain `raise ValueError`; `BrepResult` the `ReceiptContributor` carrier pairing the evaluated `TopoDS_Shape` with the optional in-memory `trimesh.Trimesh` tessellation and `BrepReceipt`, its `contribute` yielding the canonical `Iterable[Receipt]` stream (one `Receipt.of("mesh.brep", self.receipt.fact())` row over the leaf receipt's `(Phase, subject, facts)` projection, never a bare `Receipt` the `@receipted` `_stream` fold cannot normalize) so the result-carrier and the leaf evidence stay one concept and the emit is the decorator rail; `BrepReceipt` the leaf-scalar typed receipt owning that `fact` projection — carrying the kind label, the `IsNull` validity verdict the `valid` discriminant keys the `emitted`/`admitted` phase off, the `GProp` volume/area/centroid, the one-pass vertex/edge/face/solid census, the trimesh watertight verdict and exact-vs-tessellated closure gap (both `None` until tessellation), and the `GeometrySubject` literal defaulted to `mesh-algebra`.
- Cases: `BrepOp` cases `Construct(verb, params)` (primitive box/sphere/cylinder/cone/torus over `BRepPrimAPI_*`), `Boolean(shapes, verb)` (n-ary fuse/cut/common/section over the robust `BRepAlgoAPI_*` BOPAlgo operators driven by `SetArguments`/`SetTools`), `Offset(profile, verb, dist, height, join)` (the `manifold3d.CrossSection` 2D-offset-simplify leg lifted through `BRepBuilderAPI_MakeFace.Add` then extruded/revolved/lofted/thickened over `BRepPrimAPI`/`BRepOffsetAPI`), `Feature(shape, verb, size)` (fillet/chamfer over `BRepFilletAPI_*`), and `Tessellate(shape, policy)` (the in-place `BRepMesh_IncrementalMesh` triangulation extracted to an in-memory `trimesh.Trimesh`) — matched by `match`/`assert_never`, each binding the OCCT builder family that owns the kind through the `Map` dispatch tables. Linear extrusion/revolution of a profile is the `Offset` arm's `EXTRUDE`/`REVOLVE` row, not a duplicate `Construct` primitive — `MakePrism`/`MakeRevol` are reached once, through the profile leg. The `Boolean.section` row yields a `TopoDS` wire/edge result the consumer re-feeds as a profile; the daemon and CAD hop never re-discriminate the operation past this owner.
- Entry: `apply` is `async` — it awaits `LanePolicy.offload(_dispatch, op)`, the runtime's one CPU-kernel subinterpreter primitive (distinct from `drain`, which mints a `DrainReceipt`), then `.map`s the `Ok` `BrepResult` through the `@receipted(REDACTION)` `_emit` builder so the typed receipt streams once on the success path through the canonical `Signals.emit` decorator rail (the `_emit` builder returns the `BrepResult` itself, the `ReceiptContributor` the aspect harvests, so the mapped `Ok` arm keeps `result.shape`/`result.mesh` AND emits the row — never a bare return dropping the evidence egress; the `Error` arm carries no emit since the lane fence's `async_boundary` `_convert` already recorded the `BoundaryFault` on the active span), returning the `RuntimeRail[BrepResult]` the lane resolves, so the whole worker kernel rides one PEP 734 subinterpreter hop under the active OTel context and the lane imports neither OCCT nor the kernel. `_dispatch` resolves the inner verb through the `Map` dispatch tables, invokes the builder factory, calls `_built` (the one `Build`/`IsDone`/`Shape` fold over every `OcctBuilder`, raising a `BrepFault` the lane's `async_boundary` converts to a `BoundaryFault` when `IsDone()` is false so a degenerate construction never emits a phantom solid), and reads the result through `_evidence`. The `Construct` arm builds the primitive from the typed `params` tuple; the `Boolean` arm feeds the head shape to `SetArguments` and the tail to `SetTools` for one robust n-ary `Build`; the `Offset` arm runs the `manifold3d.CrossSection` offset/simplify leg, lifts the offset contour and its holes through `BRepBuilderAPI_MakeFace.Add` into a planar holed `TopoDS_Face`, and extrudes/revolves/lofts/thickens it through the OCCT family; the `Feature` arm applies the fillet/chamfer over the edges indexed by `TopExp.MapShapes_s`; the `Tessellate` arm meshes in place and extracts vertices/triangles into a `trimesh.Trimesh` carried back in `BrepResult.mesh`. The evaluated `TopoDS_Shape` rides back in `BrepResult.shape`; any persisted GLB/STEP encode is a downstream call — `MeshPayload` for the triangulation, the CAD-STEP `StepBridge` for the B-rep GLB, `artifacts` figures/scene for a visualization scene — never an arm here.
- Auto: every OCCT `Make*` builder shares the command pattern (`Build`/`IsDone`/`Shape`), so `_built` folds it once over the `OcctBuilder` Protocol and every verb row reuses it instead of re-spelling the check; `BRepAlgoAPI_Fuse`/`Cut`/`Common`/`Section` are the robust 2019+ BOPAlgo Boolean kernel (the legacy `BRepAlgo_*` path is the deleted alternative) and are n-ary operators — the binary `(S1, S2)` ctor is sugar over `SetArguments`/`SetTools` taking a `TopTools_ListOfShape`, so the `Boolean` arm appends the head to one list and the tail to another, runs one `Build`, and reads the single robust result rather than rebuilding the kernel N-1 times in a pairwise fold; the operands require triangulation absent, so the Boolean arm precedes any tessellate; the `Offset` arm weaves three `manifold3d.CrossSection` capabilities through one OCCT face lift — `_sections` resolves the `_JOINS`-mapped `JoinType` once through `Map.try_find` (never a raw `_JOINS[join]` `KeyError`) and returns the `(base, offset)` section pair the EXTRUDE/REVOLVE/THICK arms lift through `_profile_face` and the LOFT arm reads as its two outer `AddWire` rings, so the 2D leg runs once per profile: `offset(delta, join_type, miter_limit, circular_segments)` exact polygon offset, `simplify(epsilon)` collinear-vertex cull, and the multi-contour `to_polygons()` egress feeding the `BRepBuilderAPI_MakeFace.Add` hole-wire accumulation so a profile with islands lifts through `_profile_face` as one holed planar face rather than an outer-loop-only approximation; the LOFT arm lofts the outer ring of each section over `BRepOffsetAPI_ThruSections`, which lofts a wire skin and does not natively model a holed section — a holed loft is the staged `Cut`-of-two-skins arm, not silently dropped islands on the face-lift path; the `THICK` row constructs the hollow solid through the no-arg `BRepOffsetAPI_MakeThickSolid()` ctor driven by `MakeThickSolidBySimple(solid, -dist)` (the uniform-offset shell — the bare-4-arg ctor is the deleted phantom, the operation being `MakeThickSolidBySimple`/`MakeThickSolidByJoin` per the OCP builder law); `BRepMesh_IncrementalMesh(shape, deflection, relative, angle, parallel)` mutates the shape's stored triangulation in place from `IdentityPolicy.deflection`/`angle_tolerance`, and `_triangulation` reads `BRep_Tool.Triangulation_s` per face, vectorizing the `(nodes, triangles)` blocks into one `(V,3)`/`(F,3)` pair through `np.cumsum` index offsets under `process=True` so the trimesh merge-and-validate pass welds the per-face duplicate boundary vertices the kernel emits as independent node blocks — without the weld every face block is an island and `is_watertight` is structurally false on a closed solid, the merge deduplicates coincident vertices without re-triangulating the faces, so the weld is the precondition for the closure verdict rather than an optional pass; `BRepGProp.VolumeProperties_s`/`SurfaceProperties_s` populate two `GProp_GProps` accumulators exposing `Mass`/`CentreOfMass` as the exact per-solid evidence, `TopExp.MapShapes_s(shape, kind, TopTools_IndexedMapOfShape)` indexes the vertex/edge/face/solid census in one pass per kind through `Extent`, and when a tessellation exists `_evidence` reads the welded `is_watertight` verdict and cross-checks the exact `GProp` volume against `trimesh.Trimesh.volume` only when that verdict holds — the divergence-theorem `volume` is meaningless on an open surface, so an open shell or section result records `watertight=False` with a `None` closure gap rather than a spurious agreement number, and a closed solid carries the independent kernel-vs-mesh closure verdict rather than a single-source claim.
- Receipt: `BrepResult.contribute` is the `ReceiptContributor` method yielding the `Iterable[Receipt]` stream — one `Receipt.of("mesh.brep", self.receipt.fact())` row over the leaf `BrepReceipt.fact` `(Phase, GeometrySubject, dict[str, object])` projection through the owner's shape-polymorphic `(Phase, subject, facts)` factory, the `yield` shape (never a bare `Receipt`) the canonical `ReceiptContributor.contribute(self) -> Iterable[Receipt]` port and the `@receipted` aspect's `_stream` normalizer both require, so the result-carrier and the leaf evidence stay one concept and the emit is the decorator rail rather than an inline call. `BrepReceipt.fact` keys the phase off the `valid` discriminant — a null/open result (a `Boolean.section` wire/edge, an open shell) keys `phase="admitted"` so an unreliable solid is a flagged caveat rather than an asserted `emitted` solid, the same phase-on-validity split the sibling `mesh/repair.md#MESH` `MeshRepairReceipt.fact` holds — folding the kind label, the validity verdict, the `GProp` volume/area, the centroid, the census, the trimesh watertight verdict, and the closure gap into the `facts`, a `dict[str, object]` of native scalars the receipt owner's `enc_hook=repr` renderer serializes without a `str()`-coerced map, and no parallel receipt rail. The `@receipted(REDACTION)` egress aspect wears the `_emit` builder `apply` `.map`s the `Ok` `BrepResult` through, harvesting `contribute()` and routing it to `Signals.emit` on exit; the `REDACTION` is the empty `Redaction(classified=Map.empty())` policy since the validity verdict and the geometry measures carry no secret field. The evaluated solid carries the geometry `GraduationReceipt` subject `mesh-algebra` — the canonical `GeometrySubject` literal (`rasm.compute.graduation.handoff#GeometrySubject`, never a bare `str`) so a CSG/feature/offset result graduates through the one geometry rail the `mesh/repair.md#MESH` boolean arm and the `graph/algebra.md#ALGEBRA` mesh fold share.
- Packages: `cadquery-ocp` (`OCP.BRepPrimAPI.BRepPrimAPI_MakeBox`/`MakeSphere`/`MakeCylinder`/`MakeCone`/`MakeTorus`/`MakePrism`/`MakeRevol`, `OCP.BRepAlgoAPI.BRepAlgoAPI_Fuse`/`Cut`/`Common`/`Section` with `SetArguments`/`SetTools`/`Build`/`Shape`, `OCP.BRepOffsetAPI.BRepOffsetAPI_ThruSections`/`MakePipeShell`/`MakeThickSolid` with `MakeThickSolidBySimple`, `OCP.BRepFilletAPI.BRepFilletAPI_MakeFillet`/`MakeChamfer`, `OCP.BRepBuilderAPI.BRepBuilderAPI_MakePolygon`/`MakeFace`, `OCP.BRepMesh.BRepMesh_IncrementalMesh`, `OCP.BRep.BRep_Tool`, `OCP.BRepGProp.BRepGProp`, `OCP.GProp.GProp_GProps`, `OCP.TopExp.TopExp`, `OCP.TopTools.TopTools_IndexedMapOfShape`/`TopTools_ListOfShape`, `OCP.TopAbs.TopAbs_ShapeEnum`, `OCP.TopoDS.TopoDS`/`TopoDS_Shape`/`TopoDS_Face`, `OCP.TopLoc.TopLoc_Location`, `OCP.gp.gp_Pnt`/`gp_Vec`/`gp_Ax1`/`gp_Dir`), `manifold3d` (`CrossSection`/`JoinType` for the 2D profile offset/simplify leg, never a 3D `Manifold` CSG owner — that is the `manifold3d.Manifold` backend in `mesh/repair.md#MESH`), `trimesh` (`Trimesh` the in-memory triangulation carrier plus its cached `is_watertight`/`volume` properties feeding the closure cross-check), `numpy` (`asarray`/`vstack`/`cumsum` vectorizing the per-face node/triangle blocks into one `(V,3)`/`(F,3)` pair), `expression` (`tag`/`case`/`tagged_union` the `BrepOp`/`BrepFault` unions, `Map`/`Map.of_seq`/`Map.try_find` the `_PRIMITIVES`/`_BOOLEANS`/`_FEATURES`/`_JOINS` dispatch tables, `Option.default_with` the missing-verb fold raising `BrepFault`), `msgspec` (`Struct(frozen=True)` the `BrepResult` `ReceiptContributor` carrier — tracked since it references the OCCT `TopoDS_Shape` and the `trimesh.Trimesh` container objects — and `Struct(frozen=True, gc=False)` the leaf-scalar `BrepReceipt`, whose fields are native `str`/`bool`/`float` plus the immutable scalar tuples `centroid`/`census` the cyclic GC need never trace, matching the sibling `mesh/repair.md#MESH` `MeshRepairReceipt` convention), runtime (`IdentityPolicy`/`CANONICAL_POLICY` the deflection/tolerance, `RuntimeRail`/`LanePolicy`/`LanePolicy.offload` the offload rail, `Phase`/`Receipt`/`ReceiptContributor`/`Redaction`/`receipted` the typed receipt and the `@receipted(REDACTION)` egress aspect the `_emit` builder wears).
- Growth: a new primitive is one `ConstructVerb` row plus one `_PRIMITIVES` factory binding the `BRepPrimAPI_*` builder; a new set verb is one `BooleanVerb` row plus one `_BOOLEANS` entry; a new offset/loft mode is one `OffsetVerb` row binding its `BRepOffsetAPI_*` builder; a spine-following `SWEEP` verb is the imported `BRepOffsetAPI_MakePipeShell` landing site that admits the same turn a spine wire enters the `Offset` payload as a new field, so the verb is staged behind a real input rather than aliased to the linear `EXTRUDE` prism it cannot distinguish without a spine; a new feature is one `FeatureVerb` row plus one `_FEATURES` entry binding `BRepFilletAPI_*`; a new join geometry is one `JoinPolicy` row plus one `_JOINS` entry; the deflection knob is the shared `IdentityPolicy` already keyed by the daemon cache. The CAD-STEP `StepBridge` does NOT reach into this owner's in-memory `_triangulation`: the `mesh/cad.md#BRIDGE` reads source bytes and writes GLB straight through the native XCAF `RWGltf_CafWriter`, a distinct OCCT consumer that re-uses neither this evaluator's per-face read nor its trimesh extraction — the two OCCT owners meet only at the shared `cadquery-ocp` band, not a shared function. The realized cross-owner seam is an `ifc/structural` profile composing `Construct`/`Offset` to evaluate an `IfcProfileDef` cross-section into a `TopoDS_Face` before the `numpy` section integral. Zero new surface, no parallel per-operation class family.
- Boundary: this owner is the exact-kernel evaluator — it constructs, combines, offsets, features, and triangulates B-rep solids and hands the result across the wire as an in-memory `TopoDS_Shape` plus optional `trimesh.Trimesh`. No mesh-file/GLB decode/encode (the data `MeshPayload` owner `rasm.data.spatial.mesh` holds the three-engine `trimesh`/`meshio`/`rhino3dm` codec, the `MeshBackend.of` router, and the GLB `preview`); no scene/USD/GLTF/OBJ scene export (that is `artifacts` figures/scene); no STEP/IGES read-to-GLB hop (that is the sibling `mesh/cad.md#BRIDGE` `StepBridge` over the XCAF CAF reader — this owner evaluates B-rep already in memory, the bridge reads source bytes); no triangle-soup repair or `manifold3d.Manifold` mesh CSG (that is `mesh/repair.md#MESH` — exact OCCT B-rep Boolean is here, robust triangle-mesh Boolean is there, the two kernels are distinct rows on the two owners, never a shared third surface). The deleted forms: a per-operation class family (`BoxOp`/`FuseOp`/`FilletOp`) over the `BrepOp` discriminant, a `make_box`/`make_sphere` factory proliferation over the `ConstructVerb` row, a duplicate `Construct.prism`/`Construct.revol` primitive racing the `Offset` extrude/revolve leg, a verb-keyed `JoinType` hardcode racing the `JoinPolicy` row, a raw `_JOINS[join]` `Map.__getitem__` raising `KeyError` in the loft arm where `_sections` resolves the join once through `try_find` and folds the miss to `BrepFault`, a duplicate `CrossSection([list(profile)])` rebuilt-and-re-offset per loft ring where the `_sections` `(base, offset)` pair runs the `manifold3d` 2D leg once and both rings read it, a `contribute` returning a bare `Receipt` where the canonical `ReceiptContributor.contribute(self) -> Iterable[Receipt]` port and the `@receipted` `_stream` normalizer require a yielded stream, an IIFE `(lambda p: ...)(tri.Node(i))` wrapped per node/triangle in the triangulation comprehension where the walrus binds the node once, a pairwise binary-ctor Boolean fold rebuilding the BOPAlgo kernel N-1 times where one `SetArguments`/`SetTools` build is the robust n-ary form, a bare-4-arg `BRepOffsetAPI_MakeThickSolid(solid, [], -dist, tol)` ctor where the operation is `MakeThickSolidBySimple`/`MakeThickSolidByJoin`, an imperative `TopExp_Explorer` `while`-loop accumulating the census where `MapShapes_s` indexes it in one pass, a `mass` field duplicating the `volume` `GProp` read, a hand-rolled OCCT command-pattern check re-spelled per arm rather than the `_built` fold over the `OcctBuilder` Protocol, an untyped `builder: object` carrier erasing the command pattern, a domain `raise ValueError` where the typed `BrepFault` converts through the lane's `async_boundary`, a plain `dict` dispatch table raising `KeyError` on a missing verb where the `expression.Map` `try_find` Option-folds it, a stringly `dict[str, str]` receipt facts map where the native-scalar `dict[str, object]` rides the `enc_hook=repr` renderer, a hardcoded `phase="emitted"` on a null/open result where the `valid` discriminant keys `admitted` for an unreliable solid, a 4-positional-arg `Receipt.of("emitted", "mesh.brep", kind, facts)` where the owner's signature is `Receipt.of(owner, (phase, subject, facts))`, a `contribute` living on the leaf `BrepReceipt` where the `BrepResult` carrier the `@receipted` aspect harvests is the `ReceiptContributor` and the receipt owns only its `fact` projection, a `dispatch`-and-return `apply` that never `.map`s the `Ok` `BrepResult` through the `@receipted(REDACTION)` `_emit` egress builder so the typed receipt never reaches `Signals.emit`, an inline `Signals.emit(result.receipt)` threaded through `apply` or the kernel where the egress aspect over the `_emit` builder owns the one emit on the `Ok` path, a fault-arm emit double-recording the `BoundaryFault` the lane fence's `async_boundary` `_convert` already owns, a `_emit` returning the bare `result.shape` dropping the `ReceiptContributor` the aspect harvests, a non-empty `Redaction` classifying a non-secret geometry measure, an `execution.lanes` import path where the canonical owner is `rasm.runtime.lanes`, a second 3D `manifold3d.Manifold` CSG owner duplicating the `mesh/repair.md` boolean backend, a synchronous `apply` blocking the companion event loop on the CPU kernel where the offload lane isolates it, ANY `Codec`/`load`/`export`/`write` mesh-file or scene arm re-deriving the `MeshPayload`/`artifacts` seam, and the retired conda-only `pythonocc-core` `OCC.Core.*` path.

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
from rasm.runtime.receipts import Phase, Receipt, ReceiptContributor, Redaction, receipted

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


# --- [CONSTANTS] ------------------------------------------------------------------------

# the empty field-policy the `@receipted` egress aspect carries into `Signals.emit`; the validity verdict
# and the GProp/closure measures are non-secret, so no field classifies — the same keep-all `Redaction`
# the sibling `mesh/repair.md#MESH` and `mesh/daemon.md#DAEMON` worker owners bind.
REDACTION: Final[Redaction] = Redaction(classified=Map.empty())

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


class BrepReceipt(
    Struct, frozen=True, gc=False
):  # leaf-scalar evidence; owns its (Phase, subject, facts) projection, the BrepResult carrier is the ReceiptContributor
    kind: str
    valid: bool
    volume: float
    area: float
    centroid: tuple[float, float, float]
    census: Census
    watertight: bool | None
    closure_gap: float | None
    subject: GeometrySubject = "mesh-algebra"

    # the `valid` discriminant keys the phase: a null/open result (a `Boolean.section` wire, an open shell)
    # keys `admitted` so an unreliable solid is a flagged caveat rather than an asserted `emitted` solid,
    # the same phase-on-validity split the sibling `mesh/repair.md#MESH` `MeshRepairReceipt.fact` holds.
    def fact(self) -> tuple[Phase, GeometrySubject, dict[str, object]]:
        phase: Phase = "emitted" if self.valid else "admitted"
        v, e, f, s = self.census
        facts: dict[str, object] = {  # native scalars; the receipts owner's enc_hook=repr renderer serializes without a str() coerce
            "kind": self.kind,
            "valid": self.valid,
            "volume": self.volume,
            "area": self.area,
            "centroid": self.centroid,
            "census": f"v{v}/e{e}/f{f}/s{s}",
            "watertight": self.watertight,
            "closure_gap": self.closure_gap,
        }
        return phase, self.subject, facts


class BrepResult(Struct, frozen=True):  # the result-carrier is the ReceiptContributor: it keeps the evaluated shape AND streams its receipt
    shape: TopoDS_Shape
    mesh: trimesh.Trimesh | None
    receipt: BrepReceipt

    # the `ReceiptContributor` port yields an `Iterable[Receipt]` (never a bare `Receipt`) so the
    # `@receipted` aspect's `_stream` normalizes it through one fold — the carrier owns `contribute`,
    # the leaf `BrepReceipt` owns only its `fact` projection, the split the sibling owners share.
    def contribute(self) -> Iterable[Receipt]:
        yield Receipt.of("mesh.brep", self.receipt.fact())


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
    # the kernel offloads onto the lane PEP-734 hop (the lane stitches the active OTel context and folds a
    # worker raise through its own `async_boundary`), then the `Ok` `BrepResult` threads through the
    # `@receipted` egress builder so the typed receipt streams on the success path — emission is the
    # decorator rail, never an inline `Signals.emit` threaded through the kernel; the `Error` path carries
    # no emit because the lane fence's `_convert` already recorded the `BoundaryFault` on the active span.
    return (await lane.offload(_dispatch, op)).map(_emit)


@receipted(REDACTION)
def _emit(result: BrepResult) -> BrepResult:
    # the `@receipted` aspect harvests `result.contribute()` and emits the phase-keyed row on exit; the
    # builder returns the `BrepResult` itself (the contributor) so the mapped `Ok` arm keeps the evaluated
    # `result.shape`/`result.mesh` AND the receipt streams once — never a bare return dropping the evidence.
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
    com, mass = volume.CentreOfMass(), float(volume.Mass())
    watertight = None if mesh is None else bool(mesh.is_watertight)
    closure_gap = abs(mass - float(mesh.volume)) if watertight else None
    return BrepResult(
        shape,
        mesh,
        BrepReceipt(kind, not shape.IsNull(), mass, float(area.Mass()), (com.X(), com.Y(), com.Z()), _census(shape), watertight, closure_gap),
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
                    # full revolution of the XY-plane profile about the global Y axis through origin — the
                    # surface-of-revolution canon `_wire`'s z=0 placement establishes; a profile crossing the
                    # axis is a degenerate self-intersecting revolve the OCCT `IsDone` gate rails.
                    shape = _built(BRepPrimAPI_MakeRevol(_profile_face(offset), gp_Ax1(gp_Pnt(), gp_Dir(0.0, 1.0, 0.0))))
                case OffsetVerb.LOFT:
                    # `ThruSections` lofts a wire skin between the base and offset outer rings — a holed loft is
                    # the staged `Cut`-of-two-skins arm, not a hole wire this skin builder cannot model.
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


## [04]-[UPSTREAM]

# [PY_GEOMETRY_MESH_BREP]

Exact B-rep evaluation on one `BrepOp` union: parametric solid construction, n-ary Boolean algebra, profile offset/loft generation, feature operations, and watertight tessellation — each operation kind carrying its inner verb as a closed `StrEnum` row bound to the owning OCCT `BRep*API` builder through an `expression.Map` dispatch, so a new primitive, set verb, offset mode, or feature is one row and one entry, never a new surface. Its `Offset` arm rides both kernels on one rail: `manifold3d.CrossSection` offsets and simplifies the 2D profile, `BRepBuilderAPI_MakeFace.Add` lifts it as a holed planar face, then extrusion/revolution/loft/thickening yields a `TopoDS_Shape`. No durable file persists — a live `TopoDS_Shape` is a pybind11 handle no pickler carries, so every shape crosses the worker seam as sealed STEP octets (`Brep`) through the `sealed`/`unsealed` codec pair, returning as `Brep` with optional `trimesh.Trimesh`.

`TessellationPolicy` arrives from `mesh/cad` and the `CLOSURE_CEILING` closure-agreement bar from `mesh/repair`, both minted there and imported downward; an evaluated solid graduates through the geometry-minted `GeometrySubject.MESH_ALGEBRA` rail on a key the receipt's own `spec` projection derives. Its CPU-bound kernel rides `LanePolicy.offload` on the `HOSTILE` trait — the OCCT band holds process-global native state and imports under no isolated subinterpreter, so the whole `OCP.*` band defers through module-scope `lazy from` and the warm process pool is the one substrate that composes — and `apply` returns through the graduation `evidence_run` weave seeded `EvidenceScope.MESH_BREP`, whose harvest streams the typed receipt on the `Ok` path. Egress is sealed STEP octets, so a consumer continuing its own OCCT work unseals where it computes while every other consumer reads the optional `trimesh.Trimesh` projection; the two mesh peers this evaluator meets — `mesh/cad#BRIDGE`'s XCAF reader-writer and `mesh/repair#MESH`'s robust triangle CSG — share only the `cadquery-ocp` band and never a function.

## [01]-[INDEX]

- [02]-[BREP]: B-rep operation union over OCCT and `manifold3d.CrossSection`, offloaded to the warm process lane over sealed-brep crossings, returning `RuntimeRail[BrepResult]`.

## [02]-[BREP]

- Owner: `BrepOp` — one `@tagged_union` over the operation kinds with verbs as `StrEnum` rows, never a per-operation class family; `JoinPolicy` VALUES ARE the `manifold3d.JoinType` member names, so `_join` is one attribute read at the 2D leg — a policy value rather than a verb-keyed hardcode, with no mirror table to drift and no module-scope cell reifying the deferred provider; `BrepResult` carries the contributor and `BrepReceipt` the leaf evidence, the carrier/leaf split the mesh siblings share. Shape-bearing cases carry `Brep` sealed octets, never a live handle — `sealed`/`unsealed` is the one STEP `AsIs` codec pair both seam directions resolve through, so the union pickles whole across the process crossing and a native-floor consumer unseals where its own OCCT work begins.
- Cases: linear extrusion/revolution of a profile is the `Offset` arm's `EXTRUDE`/`REVOLVE` row, never a duplicate `Construct` primitive — `MakePrism`/`MakeRevol` are reached once, through the profile leg; `Boolean.section` yields a wire/edge result the consumer re-feeds as a profile, and no downstream owner re-discriminates the operation past this union.
- Law: every factory holding a caller-repairable precondition returns the rail and callers bind rather than construct, because a `raise` from a mint no enclosing fence converts escapes to its caller — `Construct` proves its parameter arity against the same `_PRIMITIVES` row the kernel calls, `Boolean` its two-operand minimum and its finite non-negative fuzz, `Offset` its non-empty profile and finite extents, `Feature` its non-negative finite magnitude; `Tessellate` alone hands the value back, holding no precondition to refuse. Each refusal anchors one `FaultRow` in this page's own table under the folder's `GeometryLeg` roster, so the coordinate a caller reads names the factory it called. Inside `_dispatch`, under the offload fence, the typed `BrepFault` is the refusal and the lane's `async_boundary` converts it.
- Law: `graduates` mints its own evidence key off the receipt's `spec` byte projection through the graduation spine, so no caller threads a key; the measured ledger grades the null-result residual every time and admits the `closure_gap` bar only where the tessellated agreement exists, since an open or unmeshed result records no agreement and grading a fabricated zero against a ceiling clears a bar that never ran.
- Law: `benched` rides the graduation `bench_seam` fold over the whole `apply` crossing — sealed-brep codec, offload, OCCT kernel, weave — subject-keyed `rasm.geometry.mesh.brep.<tag>`, so a boolean row prices the STEP seal beside the solve; latency and throughput rows per operation kind, zero instrument rows, and graduation's `bench_terminal` wraps the fold in the runtime `JobRun.bounded` envelope for a process-terminal run.
- Auto: `BRepAlgoAPI_*` is the robust BOPAlgo kernel (the legacy `BRepAlgo_*` family never enters) and its operators are n-ary — one `SetArguments`/`SetTools` build, never a pairwise fold rebuilding the kernel N-1 times; a Boolean operand requires triangulation absent, so a `Boolean` always precedes any `Tessellate` over its result.
- Packages: `cadquery-ocp` (the `OCP.*` band, every name a module-scope `lazy from` because the distribution is interpreter-marked and a loop floor importing this module for the verb vocabulary must never load OCCT — the retired conda-only `pythonocc-core` `OCC.Core.*` path never enters), `manifold3d` (`CrossSection`/`JoinType` under the same deferral, the 2D leg only — the 3D `Manifold` CSG backend belongs to `mesh/repair#MESH`), `trimesh`, `numpy`, `expression`, and `msgspec` per the fence imports; `TessellationPolicy`/`CANONICAL_TESSELLATION`, `CLOSURE_CEILING`, and `GeometrySubject` arrive from the geometry owners, the rails from runtime.
- Growth: a new primitive is one `ConstructVerb` row and one `(arity, factory)` `_PRIMITIVES` entry, the declared arity reaching the mint gate with no factory edit; a new set verb, offset mode, feature, or join is one `StrEnum` row and one `Map` entry, its cell a call-time thunk or a member NAME so the row never dereferences a deferred provider at import; a new caller-repairable refusal is one `FaultRow` row and one guard at the owning factory; a spine-following `SWEEP` verb lands as one `OffsetVerb` row over `BRepOffsetAPI_MakePipeShell` the moment a real spine-wire payload field carries its path — never aliased to the linear `EXTRUDE` prism it cannot distinguish without one, and never a prelude name the fence does not yet call.
- Boundary: mesh-file/GLB codec is the data `MeshPayload` owner's (`rasm.data.spatial.mesh`); scene/USD/GLTF/OBJ export is `artifacts` figures/scene; the STEP-read-to-GLB hop is `mesh/cad#BRIDGE`'s `StepBridge`, a distinct OCCT consumer meeting this evaluator only at the shared `cadquery-ocp` band, never a shared function; triangle-soup repair and mesh CSG are `mesh/repair#MESH`'s — exact OCCT B-rep Boolean here, robust triangle-mesh Boolean there, two kernels on two owners.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
from collections.abc import Callable, Iterable, Mapping
from enum import StrEnum
from functools import partial
from math import isfinite
from pathlib import Path
from tempfile import TemporaryDirectory
from typing import Final, Literal, Protocol, Self, assert_never

import numpy as np
import trimesh
from msgspec import Struct
from expression import Error, Nothing, Ok, Option, Some, case, tag, tagged_union
from expression.collections import Block, Map

from rasm.geometry.graduation import (
    EvidenceScope,
    GeometryHandoff,
    GeometryLeg,
    GeometrySubject,
    bench_seam,
    bench_subject,
    evidence_key,
    evidence_run,
)
from rasm.geometry.mesh.cad import CANONICAL_TESSELLATION, TessellationPolicy
from rasm.geometry.mesh.repair import CLOSURE_CEILING
from rasm.runtime.faults import TERMINAL, FaultRow, RuntimeRail, rostered
from rasm.runtime.lanes import LanePolicy
from rasm.runtime.profiles import BenchmarkReceipt
from rasm.runtime.receipts import DEFAULT_SCOPE, Phase, Receipt, ScopeKey
from rasm.runtime.workers import Kernel, KernelTrait

# the whole compiled band defers: `cadquery-ocp` is interpreter-marked and `manifold3d` is native, so a loop floor
# importing this module for the `ConstructVerb`/`BooleanVerb`/`OffsetVerb`/`FeatureVerb`/`JoinPolicy` vocabulary
# loads neither. Every table cell below is a call-time thunk or a member NAME, because a module-scope row that
# dereferences one of these names reifies its proxy at import and defeats the deferral outright.
lazy from manifold3d import CrossSection, JoinType

lazy from OCP.BRep import BRep_Tool
lazy from OCP.BRepAlgoAPI import BRepAlgoAPI_Common, BRepAlgoAPI_Cut, BRepAlgoAPI_Fuse, BRepAlgoAPI_Section, BRepAlgoAPI_Splitter
lazy from OCP.BRepBuilderAPI import (
    BRepBuilderAPI_MakeEdge,
    BRepBuilderAPI_MakeFace,
    BRepBuilderAPI_MakePolygon,
    BRepBuilderAPI_MakeWire,
    BRepBuilderAPI_NurbsConvert,
    BRepBuilderAPI_Sewing,
)
lazy from OCP.BRepFilletAPI import BRepFilletAPI_MakeChamfer, BRepFilletAPI_MakeFillet
lazy from OCP.BRepGProp import BRepGProp
lazy from OCP.BRepMesh import BRepMesh_IncrementalMesh
lazy from OCP.BRepOffsetAPI import BRepOffsetAPI_MakeThickSolid, BRepOffsetAPI_ThruSections
lazy from OCP.BRepPrimAPI import (
    BRepPrimAPI_MakeBox,
    BRepPrimAPI_MakeCone,
    BRepPrimAPI_MakeCylinder,
    BRepPrimAPI_MakePrism,
    BRepPrimAPI_MakeRevol,
    BRepPrimAPI_MakeSphere,
    BRepPrimAPI_MakeTorus,
)
lazy from OCP.GProp import GProp_GProps
lazy from OCP.GeomAPI import GeomAPI_PointsToBSpline
lazy from OCP.IFSelect import IFSelect_ReturnStatus
lazy from OCP.STEPControl import STEPControl_Reader, STEPControl_StepModelType, STEPControl_Writer
lazy from OCP.TColgp import TColgp_Array1OfPnt
lazy from OCP.TopAbs import TopAbs_ShapeEnum
lazy from OCP.TopExp import TopExp
lazy from OCP.TopLoc import TopLoc_Location
lazy from OCP.TopTools import TopTools_IndexedMapOfShape, TopTools_ListOfShape
lazy from OCP.TopoDS import TopoDS, TopoDS_Face, TopoDS_Shape, TopoDS_Wire
lazy from OCP.gp import gp_Ax1, gp_Dir, gp_Pnt, gp_Vec

# --- [TYPES] ----------------------------------------------------------------------------

type Brep = bytes  # sealed STEP `AsIs` octets — the one shape form the pickle seam carries
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
    # member VALUE is the `manifold3d.JoinType` member NAME, so `_join` is one `getattr` off the value and the retired
    # `_JOIN_NAME` mirror — a second roster one row could silently drift out of — has no reason to exist. The estate's
    # own spelling law is what the mirror bought, and it is not worth a table nothing else reads.
    ROUND = "Round"
    MITER = "Miter"
    SQUARE = "Square"
    BEVEL = "Bevel"


# --- [TABLES] ---------------------------------------------------------------------------

# this module's whole raise roster: every refusal a CALLER can repair anchors one row here and refuses at its own
# factory before the weave opens, so a caller's arity slip, empty profile, or out-of-domain magnitude never crosses a
# process boundary to be discovered as an opaque native `IndexError` or an `IsDone` false. All TERMINAL — every one is
# a property of the arguments, and re-issuing the same call refuses identically. The interior refusals `_dispatch`
# mints under the offload fence stay `BrepFault` cases, which the lane's own conversion carries whole.
BREP_ARITY: Final[FaultRow[GeometryLeg]] = FaultRow(
    leg=GeometryLeg.BREP, point="construct.arity", arm="config", defect="param-arity", retriability=TERMINAL, slots=("verb", "given", "wanted")
)
BREP_OPERANDS: Final[FaultRow[GeometryLeg]] = FaultRow(
    leg=GeometryLeg.BREP, point="boolean.operands", arm="config", defect="operand-arity", retriability=TERMINAL, slots=("operands",)
)
BREP_FUZZ: Final[FaultRow[GeometryLeg]] = FaultRow(
    leg=GeometryLeg.BREP, point="boolean.fuzz", arm="config", defect="fuzz-domain", retriability=TERMINAL, slots=("fuzzy",)
)
BREP_PROFILE: Final[FaultRow[GeometryLeg]] = FaultRow(
    leg=GeometryLeg.BREP, point="offset.profile", arm="config", defect="empty-profile", retriability=TERMINAL, slots=("verb",)
)
BREP_EXTENT: Final[FaultRow[GeometryLeg]] = FaultRow(
    leg=GeometryLeg.BREP, point="offset.extent", arm="config", defect="extent-domain", retriability=TERMINAL, slots=("verb", "extent")
)
BREP_SIZE: Final[FaultRow[GeometryLeg]] = FaultRow(
    leg=GeometryLeg.BREP, point="feature.size", arm="config", defect="size-domain", retriability=TERMINAL, slots=("verb", "size")
)
RAISES: Final[Block[FaultRow[GeometryLeg]]] = rostered(Block.of_seq([BREP_ARITY, BREP_OPERANDS, BREP_FUZZ, BREP_PROFILE, BREP_EXTENT, BREP_SIZE]))


# `_built` reads this surface rather than an erased `object`, so a maker missing a leg is a static gap, never a runtime miss.
class OcctBuilder(Protocol):
    def Build(self) -> None: ...
    def IsDone(self) -> bool: ...
    def Shape(self) -> TopoDS_Shape: ...


# --- [ERRORS] ---------------------------------------------------------------------------


# raised INTO the lane's `async_boundary`, never a domain `raise ValueError` the lane re-wraps. The token crosses
# the worker seam as `CrossedFault` DATA and re-mints parent-side per `execution/workers#CROSSING`.
@tagged_union(frozen=True)
class BrepFault(Exception):
    # every case is minted INSIDE `_dispatch` under the offload fence; the operand and fuzz refusals a caller can
    # trigger before the weave opens ride `BrepOp.Boolean`'s rail instead, so no case here lacks a minting seam.
    # `BoundaryFault.of` admits a `Tagged()` token AHEAD of every `CLASSIFY` row, so this family crosses the
    # conversion door WHOLE on the `domain` case and the catch-all's `str(cause)` half never renders it — consumers
    # match the CASE. A worker seam carries it whole too: `execution/workers#CROSSING` lowers the token onto
    # `CrossedFault` DATA at `shipped` and re-mints this family's own case parent-side, so a raise inside a HOSTILE
    # kernel needs no edit here and no render stands anywhere on the crossing. `__str__` serves the LOG and HOST
    # edge alone — a token surfacing in a worker traceback or a log line before the seam lowers it — where
    # `Exception.__str__` answers the EMPTY string for a kwarg-only union.
    tag: Literal["not_done", "holed_profile", "unknown_verb"] = tag()
    not_done: str = case()
    holed_profile: str = case()
    unknown_verb: str = case()

    def __str__(self) -> str:
        # the law half IS the tag, so no arm re-spells its own case name and a renamed case cannot drift from its render.
        return f"{self.tag}:{self._coordinate()}"

    def _coordinate(self) -> str:
        match self:
            case BrepFault(tag="not_done", not_done=builder):
                return builder
            case BrepFault(tag="holed_profile", holed_profile=verb):
                return verb
            case BrepFault(tag="unknown_verb", unknown_verb=verb):
                return verb
            case _ as unreachable:
                assert_never(unreachable)


# --- [TABLES] ---------------------------------------------------------------------------

# one `(arity, factory)` row per verb: the arity DECLARES what the thunk indexes, so `BrepOp.Construct` refuses a
# short or long parameter tuple at the mint off the same row the kernel later calls, and the two can never disagree
# about how many magnitudes a primitive takes. An unmapped verb is the `unknown_verb` fault via `try_find`, never a
# bare `KeyError`, on both the mint and the worker side.
_PRIMITIVES: Final[Map[ConstructVerb, tuple[int, Callable[[Params], OcctBuilder]]]] = Map.of_seq([
    (ConstructVerb.BOX, (3, lambda p: BRepPrimAPI_MakeBox(p[0], p[1], p[2]))),
    (ConstructVerb.SPHERE, (1, lambda p: BRepPrimAPI_MakeSphere(p[0]))),
    (ConstructVerb.CYLINDER, (2, lambda p: BRepPrimAPI_MakeCylinder(p[0], p[1]))),
    (ConstructVerb.CONE, (3, lambda p: BRepPrimAPI_MakeCone(p[0], p[1], p[2]))),
    (ConstructVerb.TORUS, (2, lambda p: BRepPrimAPI_MakeTorus(p[0], p[1]))),
])

# nullary thunks, never bare class references: a row holding the class itself dereferences the deferred name at
# module scope and reifies the whole OCCT band at import.
_BOOLEANS: Final[Map[BooleanVerb, Callable[[], OcctBuilder]]] = Map.of_seq([
    (BooleanVerb.FUSE, lambda: BRepAlgoAPI_Fuse()),
    (BooleanVerb.CUT, lambda: BRepAlgoAPI_Cut()),
    (BooleanVerb.COMMON, lambda: BRepAlgoAPI_Common()),
    (BooleanVerb.SECTION, lambda: BRepAlgoAPI_Section()),
    (BooleanVerb.SPLIT, lambda: BRepAlgoAPI_Splitter()),
])

# edge-fold rows only: SEW/NURBS carry non-maker call shapes, matched ahead of this table in the feature arm.
_FEATURES: Final[Map[FeatureVerb, Callable[[TopoDS_Shape], OcctBuilder]]] = Map.of_seq([
    (FeatureVerb.FILLET, lambda shape: BRepFilletAPI_MakeFillet(shape)),
    (FeatureVerb.CHAMFER, lambda shape: BRepFilletAPI_MakeChamfer(shape)),
])

def _join(policy: JoinPolicy) -> JoinType:
    # the policy VALUE already IS the provider member name, so the resolution is one attribute read at the 2D leg and
    # no cell holds a live enum member. A roster the provider stopped carrying surfaces as an `AttributeError` inside
    # `_dispatch`, under the offload fence that converts it — the same seam every other provider divergence reaches.
    return getattr(JoinType, policy.value)


# fixed 4-arity of TopAbs member NAMES so the `vertex, edge, face, solid` unpack is statically total against the
# `Census` 4-tuple while no cell dereferences the deferred enum.
_CENSUS: Final[tuple[str, str, str, str]] = ("TopAbs_VERTEX", "TopAbs_EDGE", "TopAbs_FACE", "TopAbs_SOLID")


# --- [MODELS] ---------------------------------------------------------------------------


class BrepReceipt(Struct, frozen=True, gc=False):  # leaf-scalar evidence; owns only its fact projection
    kind: str
    valid: bool
    volume: float
    area: float
    centroid: tuple[float, float, float]
    census: Census
    # four slots ride the absence carrier because four distinct arms MEASURE nothing for them. ABSENT `watertight`
    # means no triangulation existed to read closure off — only a `Tessellate` result carries one. ABSENT
    # `closure_gap` means no kernel-vs-mesh agreement was measured, since the divergence-theorem volume is
    # meaningless on an open surface. ABSENT `modified`/`generated` mean no BOPAlgo `History` ran at all — every arm
    # but `Boolean`. A `None` past this seam is the sentinel the branch's absence law bars, and the sibling
    # `mesh/repair#MESH` receipt spells the same `closure_gap` measure under the same `CLOSURE_CEILING` bar.
    watertight: Option[bool]
    closure_gap: Option[float]
    modified: Option[bool] = Nothing  # BOPAlgo History().HasModified() on the boolean arm; absent elsewhere
    generated: Option[bool] = Nothing  # BOPAlgo History().HasGenerated()
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
        }
        # ONE omit-fold over every unmeasured slot, so a new optional column is one roster pair and a decimate-style
        # arm reads as four unmeasured slots rather than as a closed, agreeing, unmodified solid nothing produced.
        measured: Block[tuple[str, Option[object]]] = Block.of_seq([
            ("watertight", self.watertight), ("closure_gap", self.closure_gap), ("modified", self.modified), ("generated", self.generated)
        ])
        return phase, self.subject, facts | dict(measured.choose(lambda slot: slot[1].map(lambda held: (slot[0], held))))

    @property
    def spec(self) -> bytes:
        # the byte projection that DEFINES this evidence: the operation kind, the topological census, and the mass
        # pair the kernel measured — two runs yielding the same solid key identically, so the crossing key derives
        # from the result rather than arriving from a caller.
        v, e, f, s = self.census
        return f"{self.kind}|v{v}/e{e}/f{f}/s{s}|{self.volume:.17g}|{self.area:.17g}".encode()

    def graduates(self) -> GeometryHandoff:
        # ceilings derive PER MEASURE: an untessellated or open result measures no kernel-vs-mesh agreement, so the
        # closure bar does not apply rather than grading `0.0` against it; the null-result residual grades always.
        measured = {"null_result": 0.0 if self.valid else 1.0} | self.closure_gap.map(lambda held: {"closure_gap": held}).default_value({})
        ceilings: Mapping[str, float] = {"null_result": 0.0} | self.closure_gap.map(lambda _: {"closure_gap": CLOSURE_CEILING}).default_value({})
        return GeometryHandoff.of(self.subject, evidence_key(self.subject, self.spec), measured, ceilings)


class BrepResult(Struct, frozen=True):
    brep: Brep
    mesh: trimesh.Trimesh | None
    receipt: BrepReceipt

    def unsealed(self) -> TopoDS_Shape:
        # native-floor projection: a consumer whose own OCCT work continues re-inflates the handle where it computes.
        return unsealed(self.brep)

    def contribute(self) -> Iterable[Receipt]:
        yield Receipt.of("rasm.geometry.mesh.brep", self.receipt.fact())


@tagged_union(frozen=True)
class BrepOp:
    tag: Literal["construct", "boolean", "offset", "feature", "tessellate"] = tag()
    construct: tuple[ConstructVerb, Params] = case()
    boolean: tuple[tuple[Brep, ...], BooleanVerb, float] = case()
    offset: tuple[Profile, OffsetVerb, float, float, JoinPolicy, bool] = case()
    feature: tuple[Brep, FeatureVerb, float] = case()
    tessellate: tuple[Brep, TessellationPolicy] = case()

    @staticmethod
    def Construct(verb: ConstructVerb, params: Params) -> "RuntimeRail[BrepOp]":
        # arity refuses at the MINT off the verb's OWN row, RETURNING the rail: `Params` is an unbounded float tuple, so
        # the retired factory handed a short one to a builder that indexed past its end inside the worker and a caller's
        # own slip arrived as an opaque native `IndexError` a whole process crossing away from the site that made it.
        wanted = _PRIMITIVES.try_find(verb).map(lambda row: row[0])
        return (
            Ok(BrepOp(construct=(verb, params)))
            if wanted.map(lambda arity: arity == len(params)).default_value(False)
            else Error(BREP_ARITY.raised(verb, str(len(params)), wanted.map(str).default_value("unrostered")))
        )

    @staticmethod
    def Boolean(breps: tuple[Brep, ...], verb: BooleanVerb, fuzzy: float = 0.0) -> "RuntimeRail[BrepOp]":
        # positive fuzz drives SetFuzzyValue tolerant intersection for near-coincident operands. Every verb partitions
        # arguments-versus-tools, so the OCCT builder demands two operands minimum, and a negative or non-finite fuzz
        # refuses at the mint — RETURNING the rail, because this factory runs BEFORE `apply` opens its weave and a
        # raise no enclosing fence converts escapes to its caller; the caller binds, and `apply` is unchanged.
        if len(breps) < 2:
            return Error(BREP_OPERANDS.raised(str(len(breps))))
        if not (isfinite(fuzzy) and fuzzy >= 0.0):
            return Error(BREP_FUZZ.raised(str(fuzzy)))
        return Ok(BrepOp(boolean=(breps, verb, fuzzy)))

    @staticmethod
    def Offset(
        profile: Profile, verb: OffsetVerb, dist: float, height: float = 0.0, join: JoinPolicy = JoinPolicy.ROUND, smooth: bool = False
    ) -> "RuntimeRail[BrepOp]":
        # `smooth` lifts the profile through a B-spline edge instead of the polyline wire. An EMPTY profile and a
        # non-finite extent both refuse HERE, RETURNING the rail: both are caller inputs no worker can repair, and the
        # retired `_sections` raise discovered the empty one a process crossing later under a subject naming the 2D leg
        # rather than the caller — which is exactly why the `empty_profile` case left `BrepFault` with it.
        if not profile:
            return Error(BREP_PROFILE.raised(verb))
        if not (isfinite(dist) and isfinite(height)):
            return Error(BREP_EXTENT.raised(verb, f"dist={dist},height={height}"))
        return Ok(BrepOp(offset=(profile, verb, dist, height, join, smooth)))

    @staticmethod
    def Feature(brep: Brep, verb: FeatureVerb, size: float) -> "RuntimeRail[BrepOp]":
        # `size` is a fillet radius, a chamfer distance, or a sewing tolerance — ONE non-negative finite magnitude
        # across every verb that reads it, so one domain gate serves the family rather than a per-verb ladder. `NURBS`
        # ignores it and a zero on a maker verb still rails through `IsDone`, which reports only that something failed.
        if not (isfinite(size) and size >= 0.0):
            return Error(BREP_SIZE.raised(verb, str(size)))
        return Ok(BrepOp(feature=(brep, verb, size)))

    @staticmethod
    def Tessellate(brep: Brep, policy: TessellationPolicy = CANONICAL_TESSELLATION) -> Self:
        return BrepOp(tessellate=(brep, policy))


# --- [OPERATIONS] -----------------------------------------------------------------------


async def apply(op: BrepOp, lane: LanePolicy, *, composition: ScopeKey = DEFAULT_SCOPE) -> "RuntimeRail[BrepResult]":
    # graduation weave seeded MESH_BREP: span, fence, and receipt harvest in one composition — the weave's harvest
    # streams the conforming BrepResult once on the cleared Ok, and an Error path carries no emit. HOSTILE is the
    # declared trait because the OCCT band holds process-global native state, so the kernel rides the warm process
    # pool with the WORKER death retry.
    return await evidence_run(
        EvidenceScope.MESH_BREP, f"apply.{op.tag}", partial(lane.offload, Kernel.of(_dispatch, KernelTrait.HOSTILE), op), composition=composition
    )


def benched(
    op: BrepOp, lane: LanePolicy, *, rounds: int = 32, warmup: int = 4, composition: ScopeKey = DEFAULT_SCOPE
) -> "RuntimeRail[BenchmarkReceipt]":
    # kernel macro-bench: each round drives the whole apply crossing — sealed-brep codec, offload, OCCT kernel,
    # weave — so a boolean row prices the STEP seal beside the solve; never an in-kernel probe (the pulse boundary).
    return bench_seam(
        bench_subject(EvidenceScope.MESH_BREP, op.tag),
        partial(apply, op, lane, composition=composition),
        rounds=rounds,
        warmup=warmup,
        composition=composition,
    )


def sealed(shape: TopoDS_Shape) -> Brep:
    # crossing codec, write half: STEP `AsIs` serializes the exact B-rep, so a live pybind11 handle never meets the pickle
    # seam; the scoped temp file is the codec's only disk touch and dies with the call.
    writer = STEPControl_Writer()
    if writer.Transfer(shape, STEPControl_StepModelType.STEPControl_AsIs) != IFSelect_ReturnStatus.IFSelect_RetDone:
        raise BrepFault(not_done="STEPControl_Writer.Transfer")
    with TemporaryDirectory(prefix="brep-seal-") as work:
        path = Path(work, "shape.step")
        if writer.Write(str(path)) != IFSelect_ReturnStatus.IFSelect_RetDone:
            raise BrepFault(not_done="STEPControl_Writer.Write")
        return path.read_bytes()


def unsealed(brep: Brep) -> TopoDS_Shape:
    # crossing codec, read half: roots transfer in bulk and a multi-root seal lands as one compound through `OneShape`.
    reader = STEPControl_Reader()
    with TemporaryDirectory(prefix="brep-seal-") as work:
        path = Path(work, "shape.step")
        path.write_bytes(brep)
        if reader.ReadFile(str(path)) != IFSelect_ReturnStatus.IFSelect_RetDone:
            raise BrepFault(not_done="STEPControl_Reader.ReadFile")
        reader.TransferRoots()
        shape = reader.OneShape()
        if shape.IsNull():  # a zero-root transfer returns a null handle, not a raise — validated before any consumer walks it
            raise BrepFault(not_done="STEPControl_Reader.OneShape")
        return shape


def _built(builder: OcctBuilder) -> TopoDS_Shape:
    builder.Build()
    if not builder.IsDone():
        raise BrepFault(not_done=type(builder).__name__)
    return builder.Shape()


def _census(shape: TopoDS_Shape) -> Census:
    def extent(name: str) -> int:
        carrier = TopTools_IndexedMapOfShape()
        TopExp.MapShapes_s(shape, getattr(TopAbs_ShapeEnum, name), carrier)
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
    kind: str, shape: TopoDS_Shape, mesh: trimesh.Trimesh | None, *, provenance: "Option[tuple[bool, bool]]" = Nothing
) -> BrepResult:
    volume, area = GProp_GProps(), GProp_GProps()
    BRepGProp.VolumeProperties_s(shape, volume)
    BRepGProp.SurfaceProperties_s(shape, area)
    com, mass = volume.CentreOfMass(), float(volume.Mass())
    closed = Option.of_obj(mesh).map(lambda held: bool(held.is_watertight))
    # divergence-theorem `volume` is meaningless on an open surface: an open or untessellated result measures NO gap,
    # never a spurious agreement, and the two absences share one gate so they can never disagree about what ran.
    closure_gap = closed.bind(lambda held: Some(abs(mass - float(mesh.volume))) if held else Nothing)
    return BrepResult(
        sealed(shape),
        mesh,
        BrepReceipt(
            kind, not shape.IsNull(), mass, float(area.Mass()), (com.X(), com.Y(), com.Z()), _census(shape), closed, closure_gap,
            provenance.map(lambda held: held[0]), provenance.map(lambda held: held[1]),
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


# one `(base, offset)` pair serves both the face lift and the loft rings, so the `manifold3d` 2D leg runs once per profile.
def _sections(profile: Profile, dist: float, join: JoinPolicy) -> tuple[CrossSection, CrossSection]:
    # non-emptiness is `BrepOp.Offset`'s admission, so this leg receives a profile already proved and re-proves nothing.
    base = CrossSection([list(profile)])
    offset = base.offset(dist, _join(join), 2.0, 0).simplify(1e-6) if dist else base
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
            row = _PRIMITIVES.try_find(verb).default_with(lambda: _raise(BrepFault(unknown_verb=verb)))
            return _evidence(f"construct.{verb}", _built(row[1](params)), None)
        case BrepOp(tag="boolean", boolean=(breps, verb, fuzzy)):
            shape, modified, generated = _boolean(tuple(unsealed(brep) for brep in breps), verb, fuzzy)
            # the History pair rides ONE slot: both columns come from one `History()` read, so an arm that ran no
            # BOPAlgo cannot report half a provenance and the two can never disagree about whether the walk happened.
            return _evidence(f"boolean.{verb}", shape, None, provenance=Some((modified, generated)))
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
        case BrepOp(tag="feature", feature=(sealed_target, FeatureVerb.SEW, size)):
            # Sewing is Perform/SewedShape, not the Build/IsDone/Shape maker family
            sewing = BRepBuilderAPI_Sewing(size)
            sewing.Add(unsealed(sealed_target))
            sewing.Perform()
            return _evidence("feature.sew", sewing.SewedShape(), None)
        case BrepOp(tag="feature", feature=(sealed_target, FeatureVerb.NURBS, _)):
            return _evidence("feature.nurbs", _built(BRepBuilderAPI_NurbsConvert(unsealed(sealed_target))), None)
        case BrepOp(tag="feature", feature=(sealed_target, verb, size)):
            factory = _FEATURES.try_find(verb).default_with(lambda: _raise(BrepFault(unknown_verb=verb)))
            target = unsealed(sealed_target)
            feature, edges = factory(target), TopTools_IndexedMapOfShape()
            TopExp.MapShapes_s(target, TopAbs_ShapeEnum.TopAbs_EDGE, edges)
            for i in range(1, edges.Extent() + 1):
                feature.Add(size, TopoDS.Edge_s(edges.FindKey(i)))
            return _evidence(f"feature.{verb}", _built(feature), None)
        case BrepOp(tag="tessellate", tessellate=(sealed_target, policy)):
            shape = unsealed(sealed_target)
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

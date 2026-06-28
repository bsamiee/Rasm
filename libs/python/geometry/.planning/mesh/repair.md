# [PY_GEOMETRY_MESH_REPAIR]

Robust mesh algebra — the canonical owner of the `manifold3d.Manifold` 3D boolean kernel and the `trimesh.repair` watertight-conditioning pass, the shared downstream primitive the tessellation, scan-reconstruction, clash-volume, and STEP hops compose. `MeshRepairOp` is ONE `@tagged_union` discriminating two operation kinds: `Condition` selects which winding/normal/inversion/hole-fill/stitch/weld passes run over the supplied `trimesh.Trimesh` through one `RepairStep` step-set, and `Boolean` runs n-ary `OpType`-keyed CSG over `manifold3d.Manifold.batch_boolean`. A new conditioning pass is one `RepairStep` row plus one `_CONDITION` table entry; a new CSG verb is one `BooleanOp` row plus one `_OPTYPES` entry — never a parallel per-operation class. The conditioning pass is parameterized over its step-set on both input and output: the caller selects exactly the passes a reconstruction surface needs rather than a fixed three-call hardcode behind a bare `weld: bool`.

The boolean arm reaches the robust `manifold3d.Manifold` kernel directly. `batch_boolean(manifolds, op)` is the single n-ary owner, never a manual `reduce` over the `trimesh.boolean.union`/`difference`/`intersection` facade nor a `+`/`-`/`^` left-fold, and the arm gates on `status() == Error.NoError` so a non-manifold operand rails through the typed `RepairFault` rather than emitting a phantom solid. Both arms return `MeshResult` pairing the in-memory `trimesh.Trimesh` with `MeshRepairReceipt`. `MeshResult` is the `ReceiptContributor` carrier — its `contribute` yields one phase-keyed `Receipt.of("mesh.repair", (phase, subject, facts))` row over the leaf `MeshRepairReceipt.fact` projection carrying the watertight and winding verdict, the volume and area, the vertex and face counts, the applied step-set or CSG verb, the `manifold3d` `Error` status, and the kernel-vs-mesh `closure_gap` agreement the boolean arm cross-checks. A conditioned reconstruction graduates on the `reconstructed-mesh` subject, an n-ary CSG result on the `mesh-algebra` subject — both the canonical `GeometrySubject` literal.

The `manifold3d` kernel is CPU-bound worker, so `apply` is the offload-aware entry handing `_dispatch` to the runtime `LanePolicy.offload` as one PEP 734 subinterpreter hop under the active OTel context, the lane importing neither `manifold3d` nor the kernel. Receipt egress is the cross-cutting `@receipted(REDACTION)` aspect wearing the `_emit` builder the offload `Ok` `MeshResult` threads through, the canonical decorator rail `observability/receipts#RECEIPT` declares and the sibling `mesh/daemon.md#DAEMON` and `compute/graduation/codegen.md#STUB_CODEGEN` owners hold — never an inline `Signals.emit` threaded through the kernel. This owner conditions and combines triangulations and hands the result across the wire as in-memory geometry; mesh-file decode and encode is the data `MeshPayload` owner (`rasm.data.spatial.mesh`), which binds the three-engine `trimesh`/`meshio`/`rhino3dm` codec plus GLB preview. The geometry shed never opens or writes a mesh file.

## [01]-[INDEX]

- [01]-[MESH]: the conditioning and boolean operations under one `@tagged_union` over the `trimesh.repair` step table and the `manifold3d.Manifold` `batch_boolean` kernel, folded through one offload-aware `_dispatch`, handed to the runtime CPU-offload lane, the `Ok` `MeshResult` `ReceiptContributor` threaded through the `@receipted(REDACTION)` egress aspect, returning an in-memory `trimesh.Trimesh` and one streamed typed receipt.

## [02]-[MESH]

- Owner: `MeshRepairOp` — the `@tagged_union` discriminating the two operation kinds; `RepairStep` the closed `StrEnum` step vocabulary (`FIX_WINDING`/`FIX_NORMALS`/`FIX_INVERSION`/`FILL_HOLES`/`STITCH`/`WELD`) so the conditioning pass is a selected step-set rather than a fixed call sequence behind a bare boolean; `BooleanOp` the closed `StrEnum` selecting the CSG verb so the boolean arm is one row rather than three sibling methods; `RepairFault` the typed `@tagged_union(Exception)` the kernel raises INTO the lane's `async_boundary` so a `status()`-rejected soup or an unmapped verb converts through the one `BoundaryFault` taxonomy rather than a domain `raise ValueError`; `_CONDITION` the `Final[Map[RepairStep, Callable[[trimesh.Trimesh], object]]]` step table binding each step to its `trimesh.repair` verb so the conditioning fold is data-driven and a missing step is the `Map.try_find` `Nothing` the fault rails, never a re-spelled per-step branch nor a bare `KeyError`; `_OPTYPES` the `Final[Map[BooleanOp, str]]` map binding each verb to its `manifold3d.OpType` member name (resolved through `getattr` at the boundary, no module-scope `manifold3d` import) so the CSG kind is a table lookup feeding the one `batch_boolean` owner; `MeshResult` the `ReceiptContributor` carrier pairing the conditioned `trimesh.Trimesh` with `MeshRepairReceipt`, its `contribute` yielding one phase-keyed `Receipt.of("mesh.repair", (phase, subject, facts))` row over the receipt's own `fact` projection so the receipt streams as one cross-cutting aspect rather than living on the leaf evidence struct; `MeshRepairReceipt` the leaf-scalar typed receipt owning that `(Phase, subject, facts)` projection — carrying the op tag, the `valid` verdict (watertight and `NoError`), the watertight and winding flags, the volume and area, the vertex and face counts, the applied step-set or CSG verb, the `manifold3d` `Error` status name, the kernel-vs-mesh `closure_gap`, and the `GeometrySubject` subject.
- Cases: `MeshRepairOp` cases `Condition(mesh, steps)` (the `RepairStep`-keyed conditioning fold over the supplied in-memory `Trimesh`, each selected step binding its `trimesh.repair` verb through `_CONDITION`, the default `STEPS_WATERTIGHT` step-set running the full winding/normal/inversion/hole-fill/weld pass a non-watertight reconstruction needs before the boolean arm consumes it) and `Boolean(meshes, op)` (the n-ary CSG folding the operand sequence through `manifold3d.Manifold.batch_boolean(manifolds, op)`, the single n-ary owner over the robust `manifold3d` kernel) — matched by total `match`/`assert_never`, each binding the package surface that owns the operation. Inputs and outputs are in-memory `trimesh.Trimesh`; no mesh-file format axis and no `Codec` arm live here, since decode and encode are the data `MeshPayload` owner across the `mesh ← data/spatial` seam. `MeshRepairOp.Condition(mesh, steps)` is the one reconstruction-hop entry the `scan/reconstruction.md#RECONSTRUCTION` consumer reads, selecting `STEPS_WATERTIGHT` (re-weld) or `STEPS_ORIENT` (orientation-only) as a named `Steps` value rather than a bare `weld: bool` that re-hardcodes the pass — the step-set parameterizes input and output, so a new reconstruction surface composes a step tuple rather than racing a second factory.
- Entry: `apply` is `async` — it hands `_dispatch` plus the `MeshRepairOp` to the runtime `LanePolicy.offload` and `.map`s the `Ok` `MeshResult` through the `@receipted(REDACTION)` `_emit` builder, returning the `RuntimeRail[MeshResult]` the consumer reads, so the worker `manifold3d` CSG rides one PEP 734 subinterpreter hop under the active OTel context, the lane imports neither `manifold3d` nor the kernel, and the receipt streams once on the success path through the egress aspect rather than an inline `Signals.emit` (the `_emit` builder returns the `MeshResult` itself, the `ReceiptContributor` the aspect harvests, so the `.map` arm keeps `result.mesh` AND emits the row — never a bare `result.mesh` return dropping the evidence); a worker-raised `RepairFault`/`BrokenWorkerInterpreter` crosses the lane's one `async_boundary` conversion to a `BoundaryFault` on the `RuntimeRail`, and the `Error` arm carries no emit since the lane fence's `_convert` already recorded the fault. `_dispatch` resolves the case through total `match`: the `Condition` arm folds the selected `RepairStep` set through `_CONDITION` over the in-place `Trimesh` (`trimesh.repair` verbs mutate toward a consistent outward-oriented closed solid) and reads `is_watertight`/`is_winding_consistent`/`volume`/`area` off the result; the `Boolean` arm round-trips every operand into a `manifold3d.Manifold` through `_to_manifold` (a `Mesh`/`Mesh64` carrier selected by vertex count so a large operand keeps 64-bit triangle indices), resolves the opcode name through `_OPTYPES.try_find(op)` and folds them through `batch_boolean(manifolds, getattr(OpType, opcode))`, rails the kernel status through the `status is Error.NoError or _raise(RepairFault(rejected=status.name))` expression gate when the soup is rejected, re-wraps `to_mesh()` into a `trimesh.Trimesh`, and cross-checks the exact kernel `volume()` against the re-wrapped `Trimesh.volume` as the `closure_gap` agreement. The conditioned `Trimesh` rides back in `MeshResult.mesh`; any persisted GLB/PLY/STL/3dm/VTK encode is a downstream `MeshPayload` call in `rasm.data.spatial.mesh`, never an arm here.
- Auto: `trimesh.repair.fix_winding`/`fix_normals`/`fix_inversion` (`trimesh.md` repair row [08]) mutate the mesh in place toward consistent outward orientation, `repair.fill_holes` (row [07]) closes boundary loops returning a `bool` success, and `repair.stitch` (row [09]) re-joins open boundaries, so a non-watertight reconstruction becomes a valid solid before the boolean arm consumes it; `Trimesh.merge_vertices` welds the coincident vertices the kernel emits as independent blocks. `manifold3d.Manifold.batch_boolean(manifolds, op)` (`manifold3d.md` construction row [10]) is the single n-ary CSG owner (empty=>identity `Manifold`, single=>no-op), the robust guaranteed-manifold kernel the deprecated `compose` (row [12]) and the legacy `trimesh.boolean` left-fold are the deleted alternatives to; `OpType.Add`/`Subtract`/`Intersect` (`manifold3d.md` vocabulary row [01]) are union, difference (tail differenced from head), and intersection. `Manifold(mesh)` ingest (construction row [01]) sets a non-`NoError` `status()` (query row [02]) rather than raising when the soup is not an oriented 2-manifold, so the arm gates on `status()`, not exceptions; `Manifold.to_mesh` (query row [01]) and the `Mesh`/`Mesh64` carriers (type rows [03]/[04], `vert_properties` cols 0-2 XYZ, `tri_verts` `Nx3` index) are the in-memory wire back to a `trimesh.Trimesh`, never a serialized blob; `Manifold.volume`/`surface_area`/`num_vert`/`num_tri` (query rows [05]/[07]) are the exact kernel measures the receipt reads. `Trimesh.is_watertight`/`is_winding_consistent`/`volume`/`area` (`trimesh.md` validity row [04], mass row [01]) are lazily cached properties read once after the conditioning pass; the boolean arm reads the kernel `volume()` and the independent `Trimesh.volume` so the receipt carries a kernel-vs-mesh agreement rather than a single-source claim.
- Receipt: `MeshResult.contribute` is the `ReceiptContributor` method yielding the canonical `Iterable[Receipt]` stream — one `Receipt.of("mesh.repair", (phase, subject, facts))` row over the leaf `MeshRepairReceipt.fact` `(Phase, GeometrySubject, dict[str, object])` projection through the owner's shape-polymorphic `(Phase, subject, facts)` factory, the `yield` shape (never a bare `Receipt`) the `ReceiptContributor.contribute(self) -> Iterable[Receipt]` port and the `@receipted` aspect's `_stream` normalizer both require, so the receipt-carrier and the leaf evidence stay one concept and the emit is the decorator rail rather than an inline call. The `@receipted(REDACTION)` egress aspect wears the `_emit` builder `apply` `.map`s the `Ok` `MeshResult` through, harvesting `contribute()` and routing it to `Signals.emit` on exit; the `REDACTION` is the empty `Redaction(classified=Map.empty())` policy since the watertight/winding verdict and the geometry measures carry no secret field. The `facts` is a `dict[str, object]` of native scalars the receipt owner's `enc_hook=repr` renderer serializes without a `str()`/`f"{...:.6f}"` coerce. The `valid` discriminant (watertight after a `NoError` gate) keys `phase="emitted"`; a conditioning pass that leaves a non-watertight surface keys `phase="admitted"` so an unreliable solid is flagged rather than asserted. A conditioned reconstruction carries the geometry `GraduationReceipt` subject `reconstructed-mesh`, an n-ary CSG result the `mesh-algebra` subject — both the canonical `GeometrySubject` literal (`rasm.compute.graduation.handoff#GeometrySubject`, never a bare `str`) so the cleaned mesh graduates through the one geometry rail the `mesh/brep.md#BREP` boolean arm and the `graph/algebra.md#ALGEBRA` mesh fold share.
- Packages: `trimesh` (`Trimesh`/`Trimesh.is_watertight`/`Trimesh.is_winding_consistent`/`Trimesh.volume`/`Trimesh.area`/`Trimesh.vertices`/`Trimesh.faces`/`Trimesh.merge_vertices`/`repair.fix_winding`/`repair.fix_normals`/`repair.fix_inversion`/`repair.fill_holes`/`repair.stitch`), `manifold3d` (the robust 3D CSG kernel reached directly — `Manifold`/`Manifold.batch_boolean`/`Manifold.status`/`Manifold.to_mesh`/`Manifold.volume`/`Manifold.surface_area`/`Manifold.num_vert`/`Manifold.num_tri`/`Mesh`/`Mesh64`/`OpType`/`Error`, never the `trimesh.boolean` facade nor a manual `+`/`-`/`^` reduce), `numpy` (`asarray`/`iinfo` selecting the `Mesh`/`Mesh64` carrier by vertex count and conditioning the vertex/triangle arrays), `expression` (`tag`/`case`/`tagged_union` the `MeshRepairOp`/`RepairFault` unions, `Map`/`Map.of_seq`/`Map.try_find` the `_CONDITION`/`_OPTYPES` dispatch tables, `Option.default_with` the missing-step/verb fold raising `RepairFault`), `msgspec` (`Struct(frozen=True)` the `MeshResult` `ReceiptContributor` carrier, `Struct(frozen=True, gc=False)` the leaf-scalar `MeshRepairReceipt`), runtime (`LanePolicy`/`LanePolicy.offload`/`RuntimeRail` the offload rail, `Phase`/`Receipt`/`ReceiptContributor`/`Redaction`/`receipted` the typed receipt and the `@receipted(REDACTION)` egress aspect).
- Boundary: no point-cloud registration (that is `scan/registration.md#REGISTRATION`); no IFC tessellation (that is `mesh/daemon.md#DAEMON`); no exact OCCT B-rep Boolean (that is `mesh/brep.md#BREP` over `BRepAlgoAPI_*` — robust triangle-mesh CSG is HERE over `manifold3d.Manifold`, exact B-rep CSG is there, the two kernels are distinct rows on the two owners, never a shared third surface); no mesh decimation, subdivision, smoothing, or quality metric (that is `mesh/quality.md#QUALITY`); no proximity, ray, contains, or sampling query (that is `mesh/spatial.md#SPATIAL`); mesh-file decode/encode is NOT this owner — the data `MeshPayload` owner (`rasm.data.spatial.mesh`) holds the canonical three-engine `trimesh`/`meshio`/`rhino3dm` codec, the `MeshBackend.of` extension router, the named-array `pyarrow` egress, the FE time-series rail, and the GLB `preview` export, so geometry hands in-memory `Trimesh` across the `mesh ← data/spatial` seam and never opens or writes a file. The deleted forms: a `union`/`difference`/`intersection` method family over the `BooleanOp` row, a manual `reduce` over `trimesh.boolean.union`/`difference`/`intersection` or the `manifold3d` `+`/`-`/`^` operators where `batch_boolean` owns n-ary CSG, the deprecated `Manifold.compose` where `batch_boolean(OpType.Add)` is the union owner, a non-manifold soup trusted without a `status()` gate, an imperative `if status != Error.NoError: raise` statement in the kernel where the `status is Error.NoError or _raise(RepairFault(rejected=...))` expression rail folds the gate as one short-circuit unifying with the `_OPTYPES` table-miss thunk, a fixed three-call conditioning hardcode behind a bare `weld: bool` where the `RepairStep` flag-set parameterizes the pass, a re-spelled per-step `if`-chain where `_CONDITION` folds the step table, a plain `dict` dispatch raising `KeyError` on a missing step/verb where the `expression.Map` `try_find` Option-folds it, a domain `raise ValueError` where the typed `RepairFault` converts through the lane's `async_boundary`, a stringly `dict[str, str]` receipt facts map (`str(...)`/`f"{...:.6f}"`-coerced) where the native-scalar `dict[str, object]` rides the `enc_hook=repr` renderer, a 4-positional-arg `Receipt.of("emitted", "mesh.repair", tag, facts)` where the owner's signature is `Receipt.of(owner, (phase, subject, facts))`, a `contribute` returning a bare `Receipt` rather than yielding the `Iterable[Receipt]` stream the Protocol declares, a `contribute` living on the leaf `MeshRepairReceipt` where the `MeshResult` carrier the `@receipted` aspect harvests is the `ReceiptContributor` and the receipt owns only its `fact` projection, an inline `Signals.emit(result.receipt)` threaded through `apply` or the kernel where the `@receipted(REDACTION)` egress aspect over the `_emit` builder owns the one emit on the `Ok` path, a fault-arm `Signals.emit` double-emit where the lane fence's `_convert` already records the `BoundaryFault`, a `_emit` returning the bare `result.mesh` dropping the `ReceiptContributor` the aspect harvests, a `dispatch`-and-return `apply` that never `.map`s the `Ok` result through the egress builder so the typed receipt never reaches `Signals.emit`, a non-empty `Redaction` classifying a non-secret geometry measure, an `execution.lanes` import path where the canonical owner is `rasm.runtime.lanes`, a synchronous `apply` blocking the companion event loop on the CPU kernel where the offload lane isolates it, a hand-rolled watertight-repair or hole-fill kernel where `trimesh.repair` is admitted, a hand-rolled mesh-boolean kernel where `manifold3d.Manifold` is admitted, a co-equal compas-datastructure fold (the compas half-edge algebra is the `graph/algebra.md#ALGEBRA` sibling), and ANY `MeshFormat`/`Codec`/`load`/`export` codec arm re-deriving the `MeshPayload` seam.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
from collections.abc import Callable, Iterable
from enum import StrEnum
from typing import TYPE_CHECKING, Final, Literal, Self, assert_never

import numpy as np
import trimesh
from expression import case, tag, tagged_union
from expression.collections import Map
from msgspec import Struct

from rasm.compute.graduation.handoff import GeometrySubject
from rasm.runtime.lanes import LanePolicy
from rasm.runtime.faults import RuntimeRail
from rasm.runtime.receipts import Phase, Receipt, ReceiptContributor, Redaction, receipted

if TYPE_CHECKING:
    import manifold3d

# --- [TYPES] ----------------------------------------------------------------------------

type OpKind = Literal["condition", "boolean"]
type Meshes = tuple[trimesh.Trimesh, ...]
type Steps = tuple[RepairStep, ...]


class RepairStep(StrEnum):  # the conditioning pass is a selected step-set, never a fixed call sequence behind a bare weld bool
    FIX_WINDING = "fix_winding"
    FIX_NORMALS = "fix_normals"
    FIX_INVERSION = "fix_inversion"
    FILL_HOLES = "fill_holes"
    STITCH = "stitch"
    WELD = "weld"  # Trimesh.merge_vertices weld of the coincident vertices the kernel emits as independent blocks


class BooleanOp(StrEnum):  # the CSG verb is one row feeding the single batch_boolean owner, never three sibling methods
    UNION = "union"
    DIFFERENCE = "difference"
    INTERSECTION = "intersection"


# --- [CONSTANTS] ------------------------------------------------------------------------

# the full winding/normal/inversion/hole-fill/weld pass a non-watertight reconstruction needs before the boolean arm
STEPS_WATERTIGHT: Final[Steps] = (
    RepairStep.FIX_WINDING,
    RepairStep.FIX_NORMALS,
    RepairStep.FIX_INVERSION,
    RepairStep.FILL_HOLES,
    RepairStep.WELD,
)

# the orientation-only pass for an already-merged reconstruction whose coincident vertices need no re-weld
STEPS_ORIENT: Final[Steps] = (
    RepairStep.FIX_WINDING,
    RepairStep.FIX_NORMALS,
    RepairStep.FIX_INVERSION,
    RepairStep.FILL_HOLES,
)

# the empty field-policy the `@receipted` egress aspect carries into `Signals.emit`; the watertight/winding
# verdict and the geometry measures are non-secret, so no field classifies — anchored after the Redaction model it builds.
REDACTION: Final[Redaction] = Redaction(classified=Map.empty())

# --- [ERRORS] ---------------------------------------------------------------------------


# the degenerate-soup and unmapped-verb failures the kernel raises INTO the lane's `async_boundary` so the one
# `_convert` rail folds them onto `RuntimeRail`; never a domain `raise ValueError` the lane re-wraps.
@tagged_union(frozen=True)
class RepairFault(Exception):
    tag: Literal["rejected", "unknown_step"] = tag()
    rejected: str = case()        # the non-NoError manifold3d Error name the status gate trips on
    unknown_step: str = case()    # a RepairStep / BooleanOp absent from its dispatch table


# --- [MODELS] ---------------------------------------------------------------------------


class MeshRepairReceipt(Struct, frozen=True, gc=False):  # leaf-scalar evidence; owns its (Phase, subject, facts) projection, the result-carrier is the ReceiptContributor
    op: OpKind
    valid: bool                       # watertight AND NoError; the phase discriminant
    watertight: bool
    winding_consistent: bool
    volume: float
    area: float
    vertex_count: int
    face_count: int
    verb: str                         # the applied step-set join or the CSG verb
    status: str                       # the manifold3d Error name ("NoError" off the conditioning arm)
    subject: GeometrySubject
    closure_gap: float | None = None  # |kernel volume - re-wrapped Trimesh volume| on the boolean arm, None on conditioning

    # the valid discriminant keys phase: watertight-after-NoError emits; an open conditioned surface is an admitted caveat
    def fact(self) -> tuple[Phase, GeometrySubject, dict[str, object]]:
        phase: Phase = "emitted" if self.valid else "admitted"
        facts: dict[str, object] = {  # native scalars; the receipts owner's enc_hook=repr renderer serializes without a str() coerce
            "op": self.op, "verb": self.verb, "status": self.status,
            "watertight": self.watertight, "winding_consistent": self.winding_consistent,
            "volume": self.volume, "area": self.area,
            "vertex_count": self.vertex_count, "face_count": self.face_count,
            "closure_gap": self.closure_gap,
        }
        return phase, self.subject, facts


class MeshResult(Struct, frozen=True):  # the result-carrier is the ReceiptContributor: it keeps the conditioned mesh AND streams its receipt
    mesh: trimesh.Trimesh
    receipt: MeshRepairReceipt

    # the ReceiptContributor port yields an Iterable[Receipt] (never a bare Receipt) so the @receipted aspect's _stream normalizes it through one fold
    def contribute(self) -> Iterable[Receipt]:
        yield Receipt.of("mesh.repair", self.receipt.fact())


@tagged_union(frozen=True)
class MeshRepairOp:
    tag: OpKind = tag()
    condition: tuple[trimesh.Trimesh, Steps] = case()
    boolean: tuple[Meshes, BooleanOp] = case()

    @staticmethod
    def Condition(mesh: trimesh.Trimesh, steps: Steps = STEPS_WATERTIGHT) -> Self:  # the reconstruction-hop entry; the consumer selects the step-set, never a bare weld bool
        return MeshRepairOp(condition=(mesh, steps))

    @staticmethod
    def Boolean(meshes: Meshes, op: BooleanOp) -> Self:
        return MeshRepairOp(boolean=(meshes, op))


# --- [TABLES] ---------------------------------------------------------------------------

# each RepairStep binds its trimesh.repair verb, so the conditioning fold is one Map.try_find rather than a
# re-spelled per-step branch; an unmapped step is the RepairFault the boundary rails, never a bare KeyError.
_CONDITION: Final[Map[RepairStep, Callable[[trimesh.Trimesh], object]]] = Map.of_seq((
    (RepairStep.FIX_WINDING, trimesh.repair.fix_winding),
    (RepairStep.FIX_NORMALS, trimesh.repair.fix_normals),
    (RepairStep.FIX_INVERSION, trimesh.repair.fix_inversion),
    (RepairStep.FILL_HOLES, trimesh.repair.fill_holes),
    (RepairStep.STITCH, trimesh.repair.stitch),
    (RepairStep.WELD, lambda m: m.merge_vertices()),
))

# the CSG verb maps to a manifold3d.OpType member NAME (no module-scope manifold3d import, banned by import policy);
# _combined resolves the real opcode through getattr at the boundary. Subtract differences the tail from the head.
_OPTYPES: Final[Map[BooleanOp, str]] = Map.of_seq((
    (BooleanOp.UNION, "Add"),
    (BooleanOp.DIFFERENCE, "Subtract"),
    (BooleanOp.INTERSECTION, "Intersect"),
))


# --- [OPERATIONS] -----------------------------------------------------------------------

async def apply(op: MeshRepairOp, lane: LanePolicy) -> "RuntimeRail[MeshResult]":
    # the kernel offloads onto the lane PEP-734 hop (the lane stitches the active OTel context and folds a worker
    # raise through its own async_boundary), then the Ok MeshResult threads through the @receipted egress builder
    # so the typed receipt streams on the success path — emission is the decorator rail, never an inline Signals.emit.
    return (await lane.offload(_dispatch, op)).map(_emit)


@receipted(REDACTION)
def _emit(result: MeshResult) -> MeshResult:
    # the @receipted aspect harvests `result.contribute()` and emits the phase-keyed row on exit; the builder
    # returns the MeshResult itself (the contributor) so the consumer's mapped Ok arm keeps the conditioned
    # result.mesh AND the receipt streams once — never a bare result.mesh return dropping the evidence egress.
    return result


# expression-form raise so the Option.default_with table-miss fold and the status-gate rail stay one-expression
# thunks unifying with the site's expected type; the raised RepairFault converts on the enclosing lane async_boundary.
def _raise[T](fault: RepairFault) -> T:
    raise fault


def _to_manifold(mesh: trimesh.Trimesh) -> "manifold3d.Manifold":  # Mesh64 past the uint32 ceiling so a large operand keeps 64-bit positions and triangle indices
    import manifold3d

    verts, faces = np.asarray(mesh.vertices), np.asarray(mesh.faces)
    if len(verts) > np.iinfo(np.uint32).max:  # the f64 carrier: float64 positions matched to uint64 indices, never a float32 down-cast
        return manifold3d.Manifold(manifold3d.Mesh64(vert_properties=verts.astype(np.float64), tri_verts=faces.astype(np.uint64)))
    return manifold3d.Manifold(manifold3d.Mesh(vert_properties=verts.astype(np.float32), tri_verts=faces.astype(np.uint32)))


def _conditioned(mesh: trimesh.Trimesh, steps: Steps) -> MeshResult:
    for step in steps:
        verb = _CONDITION.try_find(step).default_with(lambda: _raise(RepairFault(unknown_step=step)))
        verb(mesh)  # trimesh.repair verbs mutate in place toward a consistent outward-oriented closed solid
    watertight = bool(mesh.is_watertight)
    return MeshResult(
        mesh,
        MeshRepairReceipt(
            "condition", watertight, watertight, bool(mesh.is_winding_consistent),
            float(mesh.volume), float(mesh.area), len(mesh.vertices), len(mesh.faces),
            "+".join(s.value for s in steps), "NoError", "reconstructed-mesh",
        ),
    )


def _combined(meshes: Meshes, op: BooleanOp) -> MeshResult:
    import manifold3d

    opcode = _OPTYPES.try_find(op).default_with(lambda: _raise(RepairFault(unknown_step=op)))
    solid = manifold3d.Manifold.batch_boolean([_to_manifold(m) for m in meshes], getattr(manifold3d.OpType, opcode))
    status = solid.status()  # the kernel sets status rather than raising; a non-NoError operand rails through RepairFault, never a phantom solid
    _ = status is manifold3d.Error.NoError or _raise(RepairFault(rejected=status.name))
    soup = solid.to_mesh()
    mesh = trimesh.Trimesh(vertices=np.asarray(soup.vert_properties)[:, :3], faces=np.asarray(soup.tri_verts), process=True)
    watertight, kernel_volume = bool(mesh.is_watertight), float(solid.volume())  # status is NoError past the gate, so valid reduces to watertight
    return MeshResult(
        mesh,
        MeshRepairReceipt(
            "boolean", watertight, watertight, bool(mesh.is_winding_consistent),
            kernel_volume, float(solid.surface_area()), solid.num_vert(), solid.num_tri(),
            op.value, status.name, "mesh-algebra",
            abs(kernel_volume - float(mesh.volume)) if watertight else None,  # kernel-vs-mesh agreement, None on an open result
        ),
    )


def _dispatch(op: MeshRepairOp) -> MeshResult:
    match op:
        case MeshRepairOp(tag="condition", condition=(mesh, steps)):
            return _conditioned(mesh, steps)
        case MeshRepairOp(tag="boolean", boolean=(meshes, kind)):
            return _combined(meshes, kind)
        case unreachable:
            assert_never(unreachable)
```

## [03]-[RESEARCH]

- [TRIMESH_CONDITIONING]: the in-place conditioning verbs `repair.fix_winding`/`fix_normals`/`fix_inversion` (`trimesh.md` repair row [08]), `repair.fill_holes` (row [07], `bool` success), and `repair.stitch` (row [09]) plus the `is_watertight`/`is_winding_consistent` validity (row [04]) and `volume`/`area` mass (row [01]) lazy properties confirm against the branch `trimesh` catalogue, and `Trimesh.merge_vertices` is the confirmed weld verb the `trimesh.md` `MESH_TOPOLOGY` `process=True` axis and `LOCAL_ADMISSION` cover. The `_CONDITION` step table binds exactly these verbs, so the `RepairStep` flag-set selects which confirmed passes run rather than re-spelling a fixed sequence. The conditioning shed takes an in-memory `Trimesh` in and hands one back — byte-buffer intake (`wrap_as_stream`/`load`) belongs to the data `MeshPayload` codec, not this owner.
- [MANIFOLD_BOOLEAN]: the boolean arm reaches the `manifold3d.Manifold` kernel DIRECTLY rather than the thin `trimesh.boolean` facade, because this owner IS the canonical 3D-CSG owner the `mesh/brep.md#BREP` and `graph/algebra.md#ALGEBRA` siblings route robust triangle-mesh boolean to. `Manifold.batch_boolean(manifolds, op)` (`manifold3d.md` construction row [10]) is the single `.api`-confirmed n-ary owner (empty=>identity, single=>no-op) the `manifold3d.md` `CSG_TOPOLOGY` boolean axis mandates over a manual `reduce` of the `+`/`-`/`^` operators and over the deprecated `compose` (row [12]); `OpType.Add`/`Subtract`/`Intersect` (vocabulary row [01]) carry the union/difference/intersection opcodes the `_OPTYPES` map names. `Manifold(mesh)` ingest (row [01]) sets a non-`NoError` `status()` (query row [02]) rather than raising on a non-2-manifold soup — the `manifold3d.md` validation axis law to gate on `status()`, not exceptions — so `_combined` reads `status() == Error.NoError` and raises `RepairFault(rejected=status.name)` before trusting the result, the typed fault the lane's `async_boundary` converts. The operand round-trips through `Mesh`/`Mesh64` (type rows [03]/[04], `vert_properties` cols 0-2 XYZ, `tri_verts` `Nx3` index) selected by vertex count past the `uint32` ceiling, and `to_mesh()` (query row [01]) egresses the arrays back to a `trimesh.Trimesh` — the in-memory wire the `manifold3d.md` `STACKING_LAW` confirms, never a serialized blob. `Manifold.volume`/`surface_area`/`num_vert`/`num_tri` (query rows [05]/[07]) are the exact kernel measures the receipt reads, and `_combined` cross-checks the kernel `volume()` against the re-wrapped `trimesh.Trimesh.volume` (`trimesh.md` mass row [01]) only when the re-wrapped solid is watertight, recording the absolute difference as the `closure_gap` agreement — an open CSG result records a `None` gap rather than a spurious single-source number, the parity the `mesh/brep.md#BREP` `_evidence` closure verdict holds.
- [CODEC_SEAM]: cross-format mesh decode/encode — the `.3dm` leg over `rhino3dm.File3dm`, the FEM leg over `meshio.read`/`write`, the trimesh-native `glb`/`ply`/`stl`/`obj` rows, and the GLB `preview` — is NOT this owner: the data `MeshPayload` owner (`rasm.data.spatial.mesh`) holds the three-engine `trimesh`/`meshio`/`rhino3dm` codec, the `MeshBackend.of` extension router, and the named-array `pyarrow` egress. The conditioning shed receives an in-memory `trimesh.Trimesh` and returns one in-memory `MeshResult.mesh` across the `mesh ← data/spatial` seam; the consumer reaches `MeshPayload` for any file IO. No codec member resolves against the `rhino3dm`/`meshio` catalogues here because no codec arm exists here.

## [04]-[UPSTREAM]

- [RECEIPT_EGRESS]: receipt emission rides the cross-cutting `@receipted(REDACTION)` aspect (`observability/receipts#RECEIPT`) `apply` `.map`s the `Ok` `MeshResult` through, never an inline `Signals.emit` threaded through the kernel — the canonical decorator rail the sibling `mesh/daemon.md#DAEMON` `tessellate` and `compute/graduation/codegen.md#STUB_CODEGEN` `_emit` owners hold. The `_emit` builder returns the `MeshResult` itself, the `ReceiptContributor` whose `contribute` yields the one `Receipt.of("mesh.repair", MeshRepairReceipt.fact())` row, so the aspect harvests the stream and routes it to `Signals.emit` on exit while the `.map` arm keeps the conditioned `result.mesh` for the consumer. The split mirrors the offload-fence discipline: the `Ok` path emits the typed receipt through the egress aspect, the `Error` path carries no emit because the lane's `async_boundary` `_convert` already records the `BoundaryFault` on the active span — never a fault-arm double-emit, never a `_emit` returning a bare `result.mesh` that drops the contributor. The empty `Redaction(classified=Map.empty())` policy is correct because every receipt fact (the watertight/winding verdict, the volume/area/counts, the `closure_gap`) is a non-secret geometry measure no classification table scrubs.

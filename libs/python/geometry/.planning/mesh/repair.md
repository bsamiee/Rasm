# [PY_GEOMETRY_MESH_REPAIR]

Robust mesh algebra: the canonical owner of the `manifold3d.Manifold` 3D boolean kernel and the `trimesh.repair` watertight-conditioning pass — the shared downstream primitive the tessellation, scan-reconstruction, clash-volume, and STEP hops compose. `MeshRepairOp` discriminates two kinds: `Condition` folds a selected `RepairStep` step-set over the supplied `trimesh.Trimesh`, and `Boolean` runs n-ary CSG through `manifold3d.Manifold.batch_boolean`. This owner conditions and combines triangulations in memory and never opens or writes a mesh file — decode/encode is the data `MeshPayload` owner's (`rasm.data.spatial.mesh`) across the `mesh ← data/spatial` seam.

`to_manifold` is this owner's PUBLIC kernel and `ManifoldTier` its PUBLIC capability probe: repair is the chartered `manifold3d` owner, so the one uint32-ceiling `Mesh`/`Mesh64` build and the one exact-versus-witness tier resolution live here and the `mesh/spatial` and `mesh/quality` consumers compose both downward, never a re-spelled per-page build or a per-page `find_spec`. A conditioned reconstruction graduates on the `reconstructed-mesh` subject and an n-ary CSG result on the `mesh-algebra` subject — geometry-minted `GeometrySubject` members whose evidence key the receipt's own `spec` projection derives, never a caller-supplied key. Its CPU-bound kernel rides `LanePolicy.offload` on the `HOSTILE` trait — the `trimesh`/`manifold3d`/`numpy` band imports under no isolated subinterpreter, so the warm process pool is the one substrate that composes and the `trimesh.Trimesh` operands cross the pickle seam whole (numpy-backed, picklable) — and `apply` returns through the graduation `evidence_run` weave seeded `EvidenceScope.MESH_REPAIR`, whose harvest streams the typed receipt on the `Ok` path.

## [01]-[INDEX]

- [02]-[MESH]: conditioning and boolean operations under one union over the `trimesh.repair` step table and the `manifold3d` `batch_boolean` kernel, the `ManifoldTier` capability probe, offloaded to the warm process lane, returning `RuntimeRail[MeshResult]`.

## [02]-[MESH]

- Owner: `MeshRepairOp` — one union over the two kinds; `RepairStep` makes the conditioning pass a selected step-set parameterizing input and output, never a fixed three-call hardcode behind a bare `weld: bool`; `BooleanOp` makes the CSG verb one row feeding the single `batch_boolean` owner; `ManifoldTier` is the folder's ONE exact-geometry capability vocabulary, resolved here beside the kernel it selects; `MeshResult` is the carrier-contributor and `MeshRepairReceipt` the leaf evidence, the carrier/leaf split the mesh siblings share.
- Cases: `Condition(mesh, steps)` is the one reconstruction-hop entry the `scan/reconstruction#RECONSTRUCTION` consumer reads, selecting `STEPS_WATERTIGHT` (re-weld) or `STEPS_ORIENT` (orientation-only) as a named `Steps` value, so a new reconstruction surface composes a step tuple rather than racing a second factory; `Boolean(meshes, op)` cross-checks the exact kernel `volume()` against the re-wrapped `Trimesh.volume` as the `closure_gap` agreement, a kernel-vs-mesh verdict rather than a single-source claim.
- Law: `ManifoldTier.resolve` folds the ORDERED provider set — the exact `manifold3d` gap first, the `python-fcl` witness separation second — through one `find_spec` probe and answers `Option`, so a floor carrying neither returns `Nothing` and its consumer answers a typed invalid verdict rather than raising `ModuleNotFoundError` inside a worker kernel; the probe selects a capability tier and never an offload route, since a process-pool worker shares the one venv.
- Law: `graduates` mints its own evidence key off the receipt's `spec` byte projection through the graduation spine, so no caller threads a key and two identical results key identically; the measured ledger omits what was never measured — a conditioned result carries no `closure_gap`, so the ceiling roster drops that bar rather than grading a fabricated zero against it.
- Law: `benched` rides the graduation `bench_seam` fold over the whole `apply` crossing — offload, kernel, weave — subject-keyed `rasm.geometry.mesh.repair.<tag>`, so the conditioning and boolean kernels carry latency and throughput rows beside the per-call evidence-duration histogram with zero instrument rows; the pulse boundary bars any in-kernel probe, and graduation's `bench_terminal` wraps the fold in the runtime `JobRun.bounded` envelope for a process-terminal run.
- Auto: `batch_boolean(manifolds, op)` is the single n-ary CSG owner (empty folds to the identity `Manifold`, a singleton is a no-op) — the deprecated `Manifold.compose`, the `trimesh.boolean` facade, and a manual `+`/`-`/`^` left-fold rebuilding the kernel N-1 times never enter.
- Entry: `apply` and `benched` take the composition `ScopeKey` every geometry entry carries, threaded whole into `evidence_run` and `bench_seam`, so an embedded root's evidence, charter series, and bench receipts partition from the process root's exactly as its registered pulse points do.
- Packages: `trimesh` (the `repair` verbs and cached validity/mass axes, eager since every arm touches it), `manifold3d` (the robust CSG kernel, reached directly through one module-scope `lazy import` because the native band costs nothing until the boolean arm runs), `numpy`, `expression`, `msgspec`, and the runtime rails per the fence imports.
- Growth: a new conditioning pass is one `RepairStep` row and one `_CONDITION` entry; a new CSG verb is one `BooleanOp` row and one `_OPTYPES` entry; a new exact-geometry provider is one `ManifoldTier` row and one `_TIER_MODULE` entry ahead of or behind the rows it supersedes — never a parallel per-operation class and never a second capability probe beside this one.
- Boundary: point-cloud registration is `scan/registration#REGISTRATION`'s; IFC tessellation is `mesh/daemon#DAEMON`'s; exact OCCT B-rep Boolean is `mesh/brep#BREP`'s — robust triangle-mesh CSG here, exact B-rep CSG there, two kernels on two owners; decimation/subdivision/smoothing/metrics are `mesh/quality#QUALITY`'s; proximity/ray/contains/sampling are `mesh/spatial#SPATIAL`'s; the compas half-edge algebra is `graph/algebra#ALGEBRA`'s.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
from collections.abc import Callable, Iterable, Mapping
from enum import StrEnum
from functools import partial
from importlib.util import find_spec
from typing import Final, Literal, Self, assert_never

import numpy as np
import trimesh
from expression import Nothing, Option, Some, case, tag, tagged_union
from expression.collections import Block, Map
from msgspec import Struct

from rasm.geometry.graduation import (
    EvidenceScope,
    GeometryHandoff,
    GeometrySubject,
    bench_seam,
    bench_subject,
    evidence_key,
    evidence_run,
)
from rasm.runtime.faults import RuntimeRail
from rasm.runtime.lanes import LanePolicy
from rasm.runtime.profiles import BenchmarkReceipt
from rasm.runtime.receipts import DEFAULT_SCOPE, Phase, Receipt, ScopeKey
from rasm.runtime.workers import Kernel, KernelTrait

# the chartered native band: cold until the boolean arm or a consumer's `to_manifold` build runs, so the loop floor
# importing this module for the `RepairStep`/`BooleanOp`/`ManifoldTier` vocabulary never loads the extension. Every
# table below keys member NAMES rather than live attributes, so no module-scope cell reifies the proxy at import.
lazy import manifold3d

# --- [TYPES] ----------------------------------------------------------------------------

type OpKind = Literal["condition", "boolean"]
type Meshes = tuple[trimesh.Trimesh, ...]
type Steps = tuple[RepairStep, ...]


class RepairStep(StrEnum):
    FIX_WINDING = "fix_winding"
    FIX_NORMALS = "fix_normals"
    FIX_INVERSION = "fix_inversion"
    FILL_HOLES = "fill_holes"
    STITCH = "stitch"
    WELD = "weld"  # merge_vertices weld of the coincident vertices the kernel emits as independent blocks


class BooleanOp(StrEnum):
    UNION = "union"
    DIFFERENCE = "difference"
    INTERSECTION = "intersection"


class ManifoldTier(StrEnum):
    # the folder's ONE exact-geometry capability vocabulary, ordered by precedence rather than by preference: the
    # exact gap supersedes the conservative separation wherever the richer provider resolves.
    EXACT = "exact"  # manifold3d Manifold.min_gap — the exact solid-to-solid gap, no witness pair
    FCL_WITNESS = "fcl-witness"  # python-fcl narrow phase — signed separation plus the nearest_points witness pair

    @staticmethod
    def resolve() -> "Option[ManifoldTier]":
        # total over the REAL provider set: `manifold3d` carries no interpreter marker and `python-fcl` does, so a
        # floor holding neither answers `Nothing` and the consuming arm returns its own invalid-verdict shape. A
        # probe naming an unprobed second provider as its floor reports a capability that floor cannot deliver.
        return _TIER_MODULE.choose(lambda row: Some(row[0]) if find_spec(row[1]) is not None else Nothing).try_head()


# --- [CONSTANTS] ------------------------------------------------------------------------

# full winding/normal/inversion/hole-fill/weld pass a non-watertight reconstruction needs before the boolean arm
STEPS_WATERTIGHT: Final[Steps] = (RepairStep.FIX_WINDING, RepairStep.FIX_NORMALS, RepairStep.FIX_INVERSION, RepairStep.FILL_HOLES, RepairStep.WELD)

# orientation-only pass for an already-merged reconstruction whose coincident vertices need no re-weld
STEPS_ORIENT: Final[Steps] = (RepairStep.FIX_WINDING, RepairStep.FIX_NORMALS, RepairStep.FIX_INVERSION, RepairStep.FILL_HOLES)

# an open conditioned or boolean result never measures a kernel-vs-mesh agreement, so the closure bar joins the
# ceiling roster only where the residual exists; the watertight residual is the one bar every result carries.
CLOSURE_CEILING: Final[float] = 1e-9

# --- [ERRORS] ---------------------------------------------------------------------------


# raised INTO the lane's `async_boundary`, never a domain `raise ValueError` the lane re-wraps.
@tagged_union(frozen=True)
class RepairFault(Exception):
    tag: Literal["rejected", "unknown_step"] = tag()
    rejected: str = case()  # the non-NoError manifold3d Error name
    unknown_step: str = case()  # a step/verb absent from its dispatch table


# --- [MODELS] ---------------------------------------------------------------------------


class MeshRepairReceipt(Struct, frozen=True, gc=False):  # leaf-scalar evidence; owns only its fact and key projections
    op: OpKind
    valid: bool  # watertight AND NoError; the phase discriminant
    watertight: bool
    winding_consistent: bool
    volume: float
    area: float
    vertex_count: int
    face_count: int
    verb: str  # the applied step-set join or the CSG verb
    status: str  # the manifold3d Error name ("NoError" off the conditioning arm)
    subject: GeometrySubject
    closure_gap: float | None = None  # |kernel volume - re-wrapped Trimesh volume| on the boolean arm

    # watertight-after-NoError emits; an open conditioned surface is an admitted caveat
    def fact(self) -> tuple[Phase, GeometrySubject, dict[str, object]]:
        phase: Phase = "emitted" if self.valid else "admitted"
        facts: dict[str, object] = {  # native scalars for the receipts enc_hook=repr renderer
            "op": self.op,
            "verb": self.verb,
            "status": self.status,
            "watertight": self.watertight,
            "winding_consistent": self.winding_consistent,
            "volume": self.volume,
            "area": self.area,
            "vertex_count": self.vertex_count,
            "face_count": self.face_count,
            "closure_gap": self.closure_gap,
        }
        return phase, self.subject, facts

    @property
    def spec(self) -> bytes:
        # the byte projection that DEFINES this evidence: the operation, its verb, and the measured census the
        # kernel produced — two runs yielding the same solid key identically and a re-run over changed operands
        # keys apart, so the crossing key derives from the result rather than arriving from a caller.
        return f"{self.op}|{self.verb}|{self.vertex_count}|{self.face_count}|{self.volume:.17g}|{self.area:.17g}".encode()

    def graduates(self) -> GeometryHandoff:
        # ceilings derive PER MEASURE: a conditioned result measures no kernel-vs-mesh agreement, so the closure bar
        # does not apply rather than grading `0.0` against it, and the watertight residual is graded every time.
        measured = {"nonwatertight": 0.0 if self.watertight else 1.0} | ({} if self.closure_gap is None else {"closure_gap": self.closure_gap})
        ceilings: Mapping[str, float] = {"nonwatertight": 0.0} | ({} if self.closure_gap is None else {"closure_gap": CLOSURE_CEILING})
        return GeometryHandoff.of(self.subject, evidence_key(self.subject, self.spec), measured, ceilings)


class MeshResult(Struct, frozen=True):
    mesh: trimesh.Trimesh
    receipt: MeshRepairReceipt

    def contribute(self) -> Iterable[Receipt]:
        yield Receipt.of("rasm.geometry.mesh.repair", self.receipt.fact())


@tagged_union(frozen=True)
class MeshRepairOp:
    tag: OpKind = tag()
    condition: tuple[trimesh.Trimesh, Steps] = case()
    boolean: tuple[Meshes, BooleanOp] = case()

    @staticmethod
    def Condition(mesh: trimesh.Trimesh, steps: Steps = STEPS_WATERTIGHT) -> Self:
        return MeshRepairOp(condition=(mesh, steps))

    @staticmethod
    def Boolean(meshes: Meshes, op: BooleanOp) -> Self:
        return MeshRepairOp(boolean=(meshes, op))


# --- [TABLES] ---------------------------------------------------------------------------

# each step binds its trimesh.repair verb; an unmapped step is the RepairFault via try_find, never a bare KeyError.
_CONDITION: Final[Map[RepairStep, Callable[[trimesh.Trimesh], object]]] = Map.of_seq((
    (RepairStep.FIX_WINDING, trimesh.repair.fix_winding),
    (RepairStep.FIX_NORMALS, trimesh.repair.fix_normals),
    (RepairStep.FIX_INVERSION, trimesh.repair.fix_inversion),
    (RepairStep.FILL_HOLES, trimesh.repair.fill_holes),
    (RepairStep.STITCH, trimesh.repair.stitch),
    (RepairStep.WELD, lambda m: m.merge_vertices()),
))

# verb -> OpType member NAME: a live `manifold3d.OpType` cell here would reify the lazy proxy at import, so the row
# carries the name and `getattr` resolves it at the boundary. Subtract differences tail from head.
_OPTYPES: Final[Map[BooleanOp, str]] = Map.of_seq((
    (BooleanOp.UNION, "Add"),
    (BooleanOp.DIFFERENCE, "Subtract"),
    (BooleanOp.INTERSECTION, "Intersect"),
))

# tier -> probe MODULE, ordered by precedence; `resolve` walks it once and takes the head that resolves.
_TIER_MODULE: Final[Block[tuple[ManifoldTier, str]]] = Block.of_seq((
    (ManifoldTier.EXACT, "manifold3d"),
    (ManifoldTier.FCL_WITNESS, "fcl"),
))


# --- [OPERATIONS] -----------------------------------------------------------------------


async def apply(op: MeshRepairOp, lane: LanePolicy, *, composition: ScopeKey = DEFAULT_SCOPE) -> "RuntimeRail[MeshResult]":
    # graduation weave seeded MESH_REPAIR: span, fence, and receipt harvest in one composition — the weave's harvest
    # streams the conforming MeshResult once on the cleared Ok. HOSTILE is the declared trait: a bare callable would
    # silently lift PURE onto a subinterpreter the native band never imports under, so the kernel names the warm
    # process pool and its trait-default WORKER death retry.
    return await evidence_run(
        EvidenceScope.MESH_REPAIR, f"apply.{op.tag}", partial(lane.offload, Kernel.of(_dispatch, KernelTrait.HOSTILE), op), composition=composition
    )


def benched(
    op: MeshRepairOp, lane: LanePolicy, *, rounds: int = 32, warmup: int = 4, composition: ScopeKey = DEFAULT_SCOPE
) -> "RuntimeRail[BenchmarkReceipt]":
    # kernel macro-bench beside the per-call evidence-duration row: the subject keys the op kind under the
    # MESH_REPAIR scope, and each round drives the whole apply crossing — offload, kernel, weave — never an
    # in-kernel probe (the pulse boundary); the warm process lane amortizes across rounds.
    return bench_seam(
        bench_subject(EvidenceScope.MESH_REPAIR, op.tag),
        partial(apply, op, lane, composition=composition),
        rounds=rounds,
        warmup=warmup,
        composition=composition,
    )


# keeps the table-miss folds and the status-gate rail one-expression thunks; converts on the lane boundary.
def _raise[T](fault: RepairFault) -> T:
    raise fault


def to_manifold(mesh: trimesh.Trimesh) -> "manifold3d.Manifold":  # the PUBLIC uint32-ceiling build spatial/quality compose downward
    verts, faces = np.asarray(mesh.vertices), np.asarray(mesh.faces)
    if len(verts) > np.iinfo(np.uint32).max:  # past the uint32 ceiling: f64 positions with u64 indices, never a float32 down-cast
        return manifold3d.Manifold(manifold3d.Mesh64(vert_properties=verts.astype(np.float64), tri_verts=faces.astype(np.uint64)))
    return manifold3d.Manifold(manifold3d.Mesh(vert_properties=verts.astype(np.float32), tri_verts=faces.astype(np.uint32)))


def _conditioned(mesh: trimesh.Trimesh, steps: Steps) -> MeshResult:
    for step in steps:  # Exemption: the trimesh repair verbs mutate in place, the provider's own conditioning seam.
        verb = _CONDITION.try_find(step).default_with(lambda: _raise(RepairFault(unknown_step=step)))
        verb(mesh)  # repair verbs mutate in place toward a consistent outward-oriented closed solid
    watertight = bool(mesh.is_watertight)
    return MeshResult(
        mesh,
        MeshRepairReceipt(
            "condition",
            watertight,
            watertight,
            bool(mesh.is_winding_consistent),
            float(mesh.volume),
            float(mesh.area),
            len(mesh.vertices),
            len(mesh.faces),
            "+".join(s.value for s in steps),
            "NoError",
            GeometrySubject.RECONSTRUCTED_MESH,
        ),
    )


def _combined(meshes: Meshes, op: BooleanOp) -> MeshResult:
    opcode = _OPTYPES.try_find(op).default_with(lambda: _raise(RepairFault(unknown_step=op)))
    solid = manifold3d.Manifold.batch_boolean([to_manifold(m) for m in meshes], getattr(manifold3d.OpType, opcode))
    status = solid.status()  # kernel sets status rather than raising; a non-NoError soup rails, never a phantom solid
    _ = status is manifold3d.Error.NoError or _raise(RepairFault(rejected=status.name))
    soup = solid.to_mesh()
    mesh = trimesh.Trimesh(vertices=np.asarray(soup.vert_properties)[:, :3], faces=np.asarray(soup.tri_verts), process=True)
    watertight, kernel_volume = bool(mesh.is_watertight), float(solid.volume())  # past the gate valid reduces to watertight
    return MeshResult(
        mesh,
        MeshRepairReceipt(
            "boolean",
            watertight,
            watertight,
            bool(mesh.is_winding_consistent),
            kernel_volume,
            float(solid.surface_area()),
            solid.num_vert(),
            solid.num_tri(),
            op.value,
            status.name,
            GeometrySubject.MESH_ALGEBRA,
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

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)

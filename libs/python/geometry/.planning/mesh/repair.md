# [PY_GEOMETRY_MESH_REPAIR]

Robust mesh algebra: the canonical owner of the `manifold3d.Manifold` 3D boolean kernel and the `trimesh.repair` watertight-conditioning pass — the shared downstream primitive the tessellation, scan-reconstruction, clash-volume, and STEP hops compose. `MeshRepairOp` discriminates two kinds: `Condition` folds a selected `RepairStep` step-set over the supplied `trimesh.Trimesh`, and `Boolean` runs n-ary CSG through `manifold3d.Manifold.batch_boolean`. This owner conditions and combines triangulations in memory and never opens or writes a mesh file — decode/encode is the data `MeshPayload` owner's (`rasm.data.spatial.mesh`) across the `mesh ← data/spatial` seam.

`to_manifold` is this owner's PUBLIC kernel: repair is the chartered `manifold3d` owner, so the one uint32-ceiling `Mesh`/`Mesh64` build lives here and the `mesh/spatial` and `mesh/quality` consumers compose it downward, never a re-spelled per-page build. A conditioned reconstruction graduates on the `reconstructed-mesh` subject and an n-ary CSG result on the `mesh-algebra` subject — geometry-minted `GeometrySubject` members. Its CPU-bound kernel rides `LanePolicy.offload` on the `HOSTILE` trait — the `trimesh`/`manifold3d`/`numpy` band imports under no isolated subinterpreter, so the warm process pool is the one substrate that composes and the `trimesh.Trimesh` operands cross the pickle seam whole (numpy-backed, picklable) — and `apply` returns through the graduation `evidence_run` weave seeded `EvidenceScope.MESH_REPAIR`, whose harvest streams the typed receipt on the `Ok` path.

## [01]-[INDEX]

- [01]-[MESH]: conditioning and boolean operations under one union over the `trimesh.repair` step table and the `manifold3d` `batch_boolean` kernel, offloaded to the warm process lane, returning `RuntimeRail[MeshResult]`.

## [02]-[MESH]

- Owner: `MeshRepairOp` — one union over the two kinds; `RepairStep` makes the conditioning pass a selected step-set parameterizing input and output, never a fixed three-call hardcode behind a bare `weld: bool`; `BooleanOp` makes the CSG verb one row feeding the single `batch_boolean` owner; `MeshResult` is the carrier-contributor and `MeshRepairReceipt` the leaf evidence, the carrier/leaf split the mesh siblings share.
- Cases: `Condition(mesh, steps)` is the one reconstruction-hop entry the `scan/reconstruction.md#RECONSTRUCTION` consumer reads, selecting `STEPS_WATERTIGHT` (re-weld) or `STEPS_ORIENT` (orientation-only) as a named `Steps` value, so a new reconstruction surface composes a step tuple rather than racing a second factory; `Boolean(meshes, op)` cross-checks the exact kernel `volume()` against the re-wrapped `Trimesh.volume` as the `closure_gap` agreement, a kernel-vs-mesh verdict rather than a single-source claim.
- Auto: `batch_boolean(manifolds, op)` is the single n-ary CSG owner (empty folds to the identity `Manifold`, a singleton is a no-op) — the deprecated `Manifold.compose`, the `trimesh.boolean` facade, and a manual `+`/`-`/`^` left-fold rebuilding the kernel N-1 times never enter.
- Packages: `trimesh` (the `repair` verbs and cached validity/mass axes), `manifold3d` (the robust CSG kernel, reached directly), `numpy`, `expression`, `msgspec`, and the runtime rails per the fence imports.
- Growth: a new conditioning pass is one `RepairStep` row and one `_CONDITION` entry; a new CSG verb is one `BooleanOp` row and one `_OPTYPES` entry — never a parallel per-operation class.
- Boundary: point-cloud registration is `scan/registration.md#REGISTRATION`'s; IFC tessellation is `mesh/daemon.md#DAEMON`'s; exact OCCT B-rep Boolean is `mesh/brep.md#BREP`'s — robust triangle-mesh CSG here, exact B-rep CSG there, two kernels on two owners; decimation/subdivision/smoothing/metrics are `mesh/quality.md#QUALITY`'s; proximity/ray/contains/sampling are `mesh/spatial.md#SPATIAL`'s; the compas half-edge algebra is `graph/algebra.md#ALGEBRA`'s.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
from collections.abc import Callable, Iterable
from enum import StrEnum
from functools import partial
from typing import TYPE_CHECKING, Final, Literal, Self, assert_never

import numpy as np
import trimesh
from expression import case, tag, tagged_union
from expression.collections import Map
from msgspec import Struct

from rasm.geometry.graduation import EvidenceScope, GeometrySubject, evidence_run
from rasm.runtime.faults import RuntimeRail
from rasm.runtime.lanes import LanePolicy
from rasm.runtime.receipts import Phase, Receipt
from rasm.runtime.workers import Kernel, KernelTrait

if TYPE_CHECKING:
    import manifold3d

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


# --- [CONSTANTS] ------------------------------------------------------------------------

# full winding/normal/inversion/hole-fill/weld pass a non-watertight reconstruction needs before the boolean arm
STEPS_WATERTIGHT: Final[Steps] = (RepairStep.FIX_WINDING, RepairStep.FIX_NORMALS, RepairStep.FIX_INVERSION, RepairStep.FILL_HOLES, RepairStep.WELD)

# orientation-only pass for an already-merged reconstruction whose coincident vertices need no re-weld
STEPS_ORIENT: Final[Steps] = (RepairStep.FIX_WINDING, RepairStep.FIX_NORMALS, RepairStep.FIX_INVERSION, RepairStep.FILL_HOLES)

# --- [ERRORS] ---------------------------------------------------------------------------


# raised INTO the lane's `async_boundary`, never a domain `raise ValueError` the lane re-wraps.
@tagged_union(frozen=True)
class RepairFault(Exception):
    tag: Literal["rejected", "unknown_step"] = tag()
    rejected: str = case()  # the non-NoError manifold3d Error name
    unknown_step: str = case()  # a step/verb absent from its dispatch table


# --- [MODELS] ---------------------------------------------------------------------------


class MeshRepairReceipt(Struct, frozen=True, gc=False):  # leaf-scalar evidence; owns only its fact projection
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

# verb -> OpType member NAME: no module-scope manifold3d import; `getattr` resolves at the boundary. Subtract differences tail from head.
_OPTYPES: Final[Map[BooleanOp, str]] = Map.of_seq((
    (BooleanOp.UNION, "Add"),
    (BooleanOp.DIFFERENCE, "Subtract"),
    (BooleanOp.INTERSECTION, "Intersect"),
))


# --- [OPERATIONS] -----------------------------------------------------------------------


async def apply(op: MeshRepairOp, lane: LanePolicy) -> "RuntimeRail[MeshResult]":
    # graduation weave seeded MESH_REPAIR: span, fence, and receipt harvest in one composition — the weave's harvest
    # streams the conforming MeshResult once on the cleared Ok. HOSTILE is the declared trait: a bare callable would
    # silently lift PURE onto a subinterpreter the native band never imports under, so the kernel names the warm
    # process pool and its trait-default WORKER death retry.
    return await evidence_run(EvidenceScope.MESH_REPAIR, f"apply.{op.tag}", partial(lane.offload, Kernel.of(_dispatch, KernelTrait.HOSTILE), op))


# keeps the table-miss folds and the status-gate rail one-expression thunks; converts on the lane boundary.
def _raise[T](fault: RepairFault) -> T:
    raise fault


def to_manifold(mesh: trimesh.Trimesh) -> "manifold3d.Manifold":  # the PUBLIC uint32-ceiling build spatial/quality compose downward
    import manifold3d

    verts, faces = np.asarray(mesh.vertices), np.asarray(mesh.faces)
    if len(verts) > np.iinfo(np.uint32).max:  # past the uint32 ceiling: f64 positions with u64 indices, never a float32 down-cast
        return manifold3d.Manifold(manifold3d.Mesh64(vert_properties=verts.astype(np.float64), tri_verts=faces.astype(np.uint64)))
    return manifold3d.Manifold(manifold3d.Mesh(vert_properties=verts.astype(np.float32), tri_verts=faces.astype(np.uint32)))


def _conditioned(mesh: trimesh.Trimesh, steps: Steps) -> MeshResult:
    for step in steps:
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
    import manifold3d

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

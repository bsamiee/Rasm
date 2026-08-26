# [PY_GEOMETRY_MESH_REPAIR]

Robust mesh algebra: the canonical owner of the `manifold3d.Manifold` 3D boolean kernel and the `trimesh.repair` watertight-conditioning pass — the shared downstream primitive the tessellation, scan-reconstruction, clash-volume, and STEP hops compose. `MeshRepairOp` discriminates two kinds: `Condition` folds a selected `RepairStep` step-set over the supplied `trimesh.Trimesh`, and `Boolean` runs n-ary CSG through `manifold3d.Manifold.batch_boolean`. This owner conditions and combines triangulations in memory and never opens or writes a mesh file — decode/encode is the data `MeshPayload` owner's (`rasm.data.spatial.mesh`) across the `mesh ← data/spatial` boundary.

`to_manifold` is this owner's public exact-topology kernel and `ManifoldTier` its capability probe. `MeshRepairResult` retains its operation, subject, topology, metric, and optional closure facts directly; `apply` returns it through runtime observation on the HOSTILE lane.

## [01]-[INDEX]

- [02]-[MESH]: conditioning and boolean operations under one union over the `trimesh.repair` step table and the `manifold3d` `batch_boolean` kernel, the `ManifoldTier` capability probe, offloaded to the warm process lane, returning `RuntimeResult[MeshResult]`.

## [02]-[MESH]

- Owner: `MeshRepairOp` — one union over the two kinds; `RepairStep` makes the conditioning pass a selected step-set parameterizing input and output, never a fixed three-call hardcode behind a bare `weld: bool`; `BooleanOp` makes the CSG verb one row feeding the single `batch_boolean` owner; `ManifoldTier` is the folder's ONE exact-geometry capability vocabulary, resolved here beside the kernel it selects; `MeshResult` is the one result — the conditioned or combined mesh beside its closure census, verb, subject, and the boolean `closure_gap` agreement.
- Cases: `Condition(mesh, steps)` is the one reconstruction-hop entry the `scan/reconstruction#RECONSTRUCTION` consumer reads, selecting `STEPS_WATERTIGHT` (re-weld) or `STEPS_ORIENT` (orientation-only) as a named `Steps` value, so a new reconstruction surface composes a step tuple rather than racing a second factory; `Boolean(meshes, op)` cross-checks the exact kernel `volume()` against the re-wrapped `Trimesh.volume` as the `closure_gap` agreement, a kernel-vs-mesh verdict rather than a single-source claim.
- Law: `ManifoldTier.resolve` folds the ORDERED provider set — the exact `manifold3d` gap first, the `python-fcl` witness separation second — through one `find_spec` probe and answers `Option`, so a floor carrying neither returns `Nothing` and its consumer answers a typed invalid verdict rather than raising `ModuleNotFoundError` inside a worker kernel; the probe selects a capability tier and never an offload route, since a process-pool worker shares the one venv.
- Law: `MeshRepairResult` holds only facts the repair fold measured; conditioned results omit `closure_gap`, while exact boolean results carry the provider's closure gap.
- Law: `benched` rides the graduation `bench_boundary` fold over the whole `apply` crossing — offload, kernel, weave — subject-keyed `rasm.geometry.mesh.repair.<tag>`, so the conditioning and boolean kernels carry latency and throughput rows beside the per-call evidence-duration histogram with zero instrument rows; the pulse boundary bars any in-kernel probe, and graduation's `bench_terminal` wraps the fold in the runtime `JobRun.bounded` envelope for a process-terminal run.
- Auto: `batch_boolean(manifolds, op)` is the single n-ary CSG owner (empty folds to the identity `Manifold`, a singleton is a no-op) — the deprecated `Manifold.compose`, the `trimesh.boolean` facade, and a manual `+`/`-`/`^` left-fold rebuilding the kernel N-1 times never enter.
- Entry: `apply` and `benched` take the composition `ScopeKey` every geometry entry carries, threaded whole into `evidence_run`, so an embedded root's evidence and charter series partition from the process root's exactly as its registered pulse points do.
- Packages: `trimesh` (the `repair` verbs and cached validity/mass axes, eager since every arm touches it), `manifold3d` (the robust CSG kernel, reached directly through one module-scope `lazy import` because the native band costs nothing until the boolean arm runs), `numpy`, `expression`, `msgspec`, and the runtime results per the fence imports.
- Growth: a new conditioning pass is one `RepairStep` row and one `_CONDITION` entry; a new CSG verb is one `BooleanOp` row and one `_OPTYPES` entry; a new exact-geometry provider is one `ManifoldTier` row and one `_TIER_MODULE` entry ahead of or behind the rows it supersedes — never a parallel per-operation class and never a second capability probe beside this one.
- Boundary: point-cloud registration is `scan/registration#REGISTRATION`'s; IFC tessellation is `mesh/daemon#DAEMON`'s; exact OCCT B-rep Boolean is `mesh/brep#BREP`'s — robust triangle-mesh CSG here, exact B-rep CSG there, two kernels on two owners; decimation/subdivision/smoothing/metrics are `mesh/quality#QUALITY`'s; proximity/ray/contains/sampling are `mesh/spatial#SPATIAL`'s; the compas half-edge algebra is `graph/algebra#ALGEBRA`'s.

```python
# --- [IMPORTS] --------------------------------------------------------------------------
from collections.abc import Callable, Mapping
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
    GeometrySubject,
    bench_boundary,
    bench_subject,
    evidence_run,
)
from rasm.runtime.faults import RuntimeResult
from rasm.runtime.lanes import LanePolicy
from rasm.runtime.observe import DEFAULT_SCOPE, ScopeKey
from rasm.runtime.profiles import Benchmark
from rasm.runtime.workers import Kernel, KernelTrait

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
    WELD = "weld"


class BooleanOp(StrEnum):
    UNION = "union"
    DIFFERENCE = "difference"
    INTERSECTION = "intersection"


class ManifoldTier(StrEnum):
    EXACT = "exact"
    FCL_WITNESS = "fcl-witness"

    @staticmethod
    def resolve() -> "Option[ManifoldTier]":
        return _TIER_MODULE.choose(lambda row: Some(row[0]) if find_spec(row[1]) is not None else Nothing).try_head()


# --- [CONSTANTS] ------------------------------------------------------------------------

STEPS_WATERTIGHT: Final[Steps] = (RepairStep.FIX_WINDING, RepairStep.FIX_NORMALS, RepairStep.FIX_INVERSION, RepairStep.FILL_HOLES, RepairStep.WELD)

STEPS_ORIENT: Final[Steps] = (RepairStep.FIX_WINDING, RepairStep.FIX_NORMALS, RepairStep.FIX_INVERSION, RepairStep.FILL_HOLES)

# --- [ERRORS] ---------------------------------------------------------------------------


@tagged_union(frozen=True)
class RepairFault(Exception):
    tag: Literal["rejected", "unknown_step"] = tag()
    rejected: str = case()
    unknown_step: str = case()

    def __str__(self) -> str:
        return f"{self.tag}:{self._coordinate()}"

    def _coordinate(self) -> str:
        match self:
            case RepairFault(tag="rejected", rejected=status):
                return status
            case RepairFault(tag="unknown_step", unknown_step=step):
                return step
            case _ as unreachable:
                assert_never(unreachable)


# --- [MODELS] ---------------------------------------------------------------------------


class MeshResult(Struct, frozen=True):
    mesh: trimesh.Trimesh
    op: OpKind
    verb: str
    subject: GeometrySubject
    watertight: bool
    winding_consistent: bool
    volume: float
    area: float
    vertex_count: int
    face_count: int
    closure_gap: Option[float] = Nothing

    @property
    def span_facts(self) -> Mapping[str, object]:
        return {
            "op": self.op,
            "verb": self.verb,
            "subject": self.subject.value,
            "watertight": self.watertight,
            "winding_consistent": self.winding_consistent,
            "volume": self.volume,
            "area": self.area,
            "vertex_count": self.vertex_count,
            "face_count": self.face_count,
        } | self.closure_gap.map(lambda held: {"closure_gap": held}).default_value({})

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

_CONDITION: Final[Map[RepairStep, Callable[[trimesh.Trimesh], object]]] = Map.of_seq((
    (RepairStep.FIX_WINDING, trimesh.repair.fix_winding),
    (RepairStep.FIX_NORMALS, trimesh.repair.fix_normals),
    (RepairStep.FIX_INVERSION, trimesh.repair.fix_inversion),
    (RepairStep.FILL_HOLES, trimesh.repair.fill_holes),
    (RepairStep.STITCH, trimesh.repair.stitch),
    (RepairStep.WELD, lambda m: m.merge_vertices()),
))

_OPTYPES: Final[Map[BooleanOp, str]] = Map.of_seq((
    (BooleanOp.UNION, "Add"),
    (BooleanOp.DIFFERENCE, "Subtract"),
    (BooleanOp.INTERSECTION, "Intersect"),
))

_TIER_MODULE: Final[Block[tuple[ManifoldTier, str]]] = Block.of_seq((
    (ManifoldTier.EXACT, "manifold3d"),
    (ManifoldTier.FCL_WITNESS, "fcl"),
))


# --- [OPERATIONS] -----------------------------------------------------------------------


async def apply(op: MeshRepairOp, lane: LanePolicy, *, composition: ScopeKey = DEFAULT_SCOPE) -> "RuntimeResult[MeshResult]":
    return await evidence_run(
        EvidenceScope.MESH_REPAIR, f"apply.{op.tag}", partial(lane.offload, Kernel.of(_dispatch, KernelTrait.HOSTILE), op), composition=composition
    )


def benched(
    op: MeshRepairOp, lane: LanePolicy, *, rounds: int = 32, warmup: int = 4, composition: ScopeKey = DEFAULT_SCOPE
) -> "RuntimeResult[Benchmark]":
    return bench_boundary(bench_subject(EvidenceScope.MESH_REPAIR, op.tag), partial(apply, op, lane, composition=composition), rounds=rounds, warmup=warmup)


def _raise[T](fault: RepairFault) -> T:
    raise fault


def to_manifold(mesh: trimesh.Trimesh) -> "manifold3d.Manifold":
    verts, faces = np.asarray(mesh.vertices), np.asarray(mesh.faces)
    if len(verts) > np.iinfo(np.uint32).max:
        return manifold3d.Manifold(manifold3d.Mesh64(vert_properties=verts.astype(np.float64), tri_verts=faces.astype(np.uint64)))
    return manifold3d.Manifold(manifold3d.Mesh(vert_properties=verts.astype(np.float32), tri_verts=faces.astype(np.uint32)))


def _conditioned(mesh: trimesh.Trimesh, steps: Steps) -> MeshResult:
    for step in steps:
        verb = _CONDITION.try_find(step).default_with(lambda: _raise(RepairFault(unknown_step=step)))
        verb(mesh)
    return MeshResult(
        mesh=mesh,
        op="condition",
        verb="+".join(s.value for s in steps),
        subject=GeometrySubject.RECONSTRUCTED_MESH,
        watertight=bool(mesh.is_watertight),
        winding_consistent=bool(mesh.is_winding_consistent),
        volume=float(mesh.volume),
        area=float(mesh.area),
        vertex_count=len(mesh.vertices),
        face_count=len(mesh.faces),
    )


def _combined(meshes: Meshes, op: BooleanOp) -> MeshResult:
    opcode = _OPTYPES.try_find(op).default_with(lambda: _raise(RepairFault(unknown_step=op)))
    solid = manifold3d.Manifold.batch_boolean([to_manifold(m) for m in meshes], getattr(manifold3d.OpType, opcode))
    status = solid.status()
    _ = status is manifold3d.Error.NoError or _raise(RepairFault(rejected=status.name))
    soup = solid.to_mesh()
    mesh = trimesh.Trimesh(vertices=np.asarray(soup.vert_properties)[:, :3], faces=np.asarray(soup.tri_verts), process=True)
    watertight, kernel_volume = bool(mesh.is_watertight), float(solid.volume())
    return MeshResult(
        mesh=mesh,
        op="boolean",
        verb=op.value,
        subject=GeometrySubject.MESH_ALGEBRA,
        watertight=watertight,
        winding_consistent=bool(mesh.is_winding_consistent),
        volume=kernel_volume,
        area=float(solid.surface_area()),
        vertex_count=solid.num_vert(),
        face_count=solid.num_tri(),
        closure_gap=Some(abs(kernel_volume - float(mesh.volume))) if watertight else Nothing,
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

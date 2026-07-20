# [PY_GEOMETRY_MESH_QUALITY]

Mesh-topology conditioning and metrology over an in-memory triangulation: `MeshQualityOp` discriminates decimate/subdivide/smooth/metrics on one polymorphic entrypoint, `MeshQualityResult` mirrors the op case, and `QualityMetrics` is the one shape/validity/topology grade — never a metric-per-method family. Tessellation, scan-reconstruction, and STEP hops compose this primitive to coarsen, refine, denoise, and grade a surface before it crosses a downstream rail; the surface arrives and returns as in-memory `trimesh.Trimesh` across the `mesh ← data/spatial` seam, and this owner never opens or writes a mesh file.

`closure_fold` is this owner's PUBLIC exact-closure truth — the one watertight/euler/volume/area/components fold `scan/reconstruction` and `scan/deviation` compose downward, never a per-consumer re-computation. `manifold3d` exact-topology enrichment builds through repair's public `to_manifold` (repair is the chartered `manifold3d` owner — no `Mesh`/`Mesh64` selection re-spelled here) and resolves once per capsule through the two-tier `QualityBackend` probe; the CPU-bound kernels ride `LanePolicy.offload` on the `HOSTILE` trait, because the `trimesh`/`manifold3d` band imports under no isolated subinterpreter and the warm process pool is the one substrate that composes. This owner mints no `GeometrySubject`: it is the read-and-condition primitive, and the graduating subject belongs to the repair and reconstruction owners that emit the conditioned solid.

## [01]-[INDEX]

- [01]-[QUALITY]: decimate, subdivide, smooth, and metrics on one tagged union over the `trimesh`/`numpy` spine with the `manifold3d` exact-topology tier, returning one `MeshQualityResult` union.

## [02]-[QUALITY]

- Owner: `MeshQuality` — the boundary capsule over the four arms; `SmoothKind` makes the smoothing filter family one row on the `Smooth` case, never three parallel entrypoints; the `_arm` cross-cut folds every arm's `Outcome` into the one held receipt (the `mesh/spatial.md#SPATIAL` `_fold` convention), so a new case writes only the geometry body producing an `Outcome`.
- Cases: `Decimate` coarsens mesh topology for a downstream geometry op — render-time decimation for display is the artifacts figures owner's, and an LOD/display-budget arm here trespasses that boundary; `Subdivide` densifies before a curvature-sensitive metric pass; `Smooth` denoises before a deviation pass; `Metrics` is the gate the daemon and the clash/deviation hops read before trusting a surface.
- Auto: the `MANIFOLD3D` tier is enrichment over the always-available `SPINE` Euler-characteristic default, never the spine itself — one offloaded build yields the exact genus (summed over `decompose()` components), the exact counts, and the kernel mass superseding the `trimesh` measure in a single fold.
- Packages: `trimesh` (the conditioning filters, cached validity/mass axes, `vertex_defects`), `numpy` (the half-edge incidence fold and per-cell shape statistics), `manifold3d` (the exact tier, reached only through the `QualityBackend` row), `expression`, `msgspec`, and the runtime rails per the fence imports.
- Growth: a new conditioning op is one `MeshQualityOp` case and its mirrored `MeshQualityResult` arm and one `Outcome`-producing body; a new smoothing filter is one `SmoothKind` row; a new topology backend is one `QualityBackend` row.
- Boundary: watertight repair, hole-fill, and boolean CSG are `mesh/repair`'s; proximity, ray, contains, and sampling queries are `mesh/spatial`'s; registration and reconstruction are `scan/registration`+`scan/reconstruction`'s; mesh-file decode/encode is the data `MeshPayload` owner's (`rasm.data.spatial.mesh`).

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
from collections.abc import Iterable, Sequence
from enum import StrEnum
from importlib.util import find_spec
from typing import Final, Literal, assert_never, overload

import numpy as np
import trimesh
from expression import Ok, case, tag, tagged_union
from expression.collections import Block
from msgspec import Struct

from rasm.geometry.mesh.repair import to_manifold
from rasm.runtime.faults import Disposition, RuntimeRail, boundary, traversed
from rasm.runtime.lanes import LanePolicy
from rasm.runtime.receipts import Phase, Receipt
from rasm.runtime.workers import Kernel, KernelTrait

# --- [TYPES] ----------------------------------------------------------------------------

type OpKind = Literal["decimate", "subdivide", "smooth", "metrics"]


class SmoothKind(StrEnum):
    TAUBIN = "taubin"        # shrink-free band-pass Laplacian
    LAPLACIAN = "laplacian"  # uniform-weight Laplacian
    HUMPHREY = "humphrey"    # Humphrey classes anti-shrink


class QualityBackend(StrEnum):
    SPINE = "spine"            # Euler characteristic V−E+F=2−2g over the half-edge fold, always available
    MANIFOLD3D = "manifold3d"  # worker-enrichment tier: exact integer topology

    @staticmethod
    def resolve() -> "QualityBackend":
        # capability probe over the ONE shared venv — a process-pool worker inherits it, so a module absent here is
        # absent on every floor and the probe selects the tier, never a routing signal.
        return QualityBackend.MANIFOLD3D if find_spec("manifold3d") is not None else QualityBackend.SPINE


# --- [CONSTANTS] ------------------------------------------------------------------------

_QUANTILE_FRACTIONS: Final[np.ndarray] = np.array([0.0, 0.25, 0.5, 0.75, 1.0])  # five-number summary positions
_TAUBIN_K: Final[float] = 0.05  # the Taubin pass-band target `1/lamb - 1/nu`; nu = lamb/(1 - K*lamb) holds it constant across every lamb


# --- [MODELS] ---------------------------------------------------------------------------


class ExactTopology(Struct, frozen=True, gc=False):  # the MANIFOLD3D tier's one fold; never a lone genus() read
    genus: int
    vertex_count: int
    edge_count: int
    face_count: int
    components: int  # decompose() body count
    volume: float
    area: float


class QualityMetrics(Struct, frozen=True):  # holds tuple distributions, so it stays GC-tracked
    watertight: bool
    winding_consistent: bool
    area: float
    volume: float
    vertex_count: int
    face_count: int
    edge_count: int
    boundary_edges: int
    nonmanifold_edges: int
    components: int  # exact decompose() count when the tier resolves, trimesh body_count the fall-through
    genus: int
    euler_characteristic: int
    aspect_ratio: tuple[float, float, float, float, float]   # five-number (min, q1, median, q3, max)
    skewness: tuple[float, float, float, float, float]        # equiangular five-number summary
    angle_defect_mean: float  # mean per-vertex angle defect, a curvature signal, never vertex valence
    angle_defect_std: float

    @property
    def worst(self) -> tuple[float, float]:  # the tail verdict off the held distributions, never a re-run of the cell fold
        return (self.aspect_ratio[4], self.skewness[4])


class MeshQualityReceipt(Struct, frozen=True, gc=False):
    op: OpKind
    backend: QualityBackend
    faces_before: int
    faces_after: int
    watertight: bool
    worst_aspect_ratio: float
    worst_skewness: float
    genus: int


@tagged_union(frozen=True)
class MeshQualityOp:
    tag: OpKind = tag()
    decimate: int = case()
    subdivide: tuple[float, int] = case()
    smooth: tuple[SmoothKind, int, float] = case()
    metrics: bool = case()

    @staticmethod
    def Decimate(target_faces: int) -> "MeshQualityOp":
        return MeshQualityOp(decimate=target_faces)

    @staticmethod
    def Subdivide(max_edge: float, iterations: int = 1) -> "MeshQualityOp":
        return MeshQualityOp(subdivide=(max_edge, iterations))

    @staticmethod
    def Smooth(kind: SmoothKind = SmoothKind.TAUBIN, iterations: int = 10, factor: float = 0.5) -> "MeshQualityOp":
        return MeshQualityOp(smooth=(kind, iterations, factor))

    @staticmethod
    def Metrics() -> "MeshQualityOp":
        return MeshQualityOp(metrics=True)


@tagged_union(frozen=True)
class MeshQualityResult:
    tag: OpKind = tag()
    decimate: trimesh.Trimesh = case()
    subdivide: trimesh.Trimesh = case()
    smooth: trimesh.Trimesh = case()
    metrics: QualityMetrics = case()

    @staticmethod
    def Decimate(mesh: trimesh.Trimesh) -> "MeshQualityResult":
        return MeshQualityResult(decimate=mesh)

    @staticmethod
    def Subdivide(mesh: trimesh.Trimesh) -> "MeshQualityResult":
        return MeshQualityResult(subdivide=mesh)

    @staticmethod
    def Smooth(mesh: trimesh.Trimesh) -> "MeshQualityResult":
        return MeshQualityResult(smooth=mesh)

    @staticmethod
    def Metrics(metrics: QualityMetrics) -> "MeshQualityResult":
        return MeshQualityResult(metrics=metrics)


class Outcome(Struct, frozen=True):  # one arm's payload and receipt facts; never a per-arm receipt build
    result: MeshQualityResult
    faces_before: int
    faces_after: int
    watertight: bool
    worst_aspect_ratio: float
    worst_skewness: float
    genus: int


# --- [ERRORS] ---------------------------------------------------------------------------


@tagged_union(frozen=True)
class QualityFault(Exception):  # raised INTO the lane's async_boundary, never a domain raise ValueError
    tag: Literal["rejected"] = tag()
    rejected: str = case()  # the manifold3d Error status name


# --- [OPERATIONS] -----------------------------------------------------------------------


def _quantiles(values: np.ndarray) -> tuple[float, float, float, float, float]:  # one vectorized gather over the sorted view
    s = np.sort(values)
    if s.size == 0:
        return (0.0, 0.0, 0.0, 0.0, 0.0)
    idx = np.clip(np.round(_QUANTILE_FRACTIONS * (s.size - 1)).astype(np.intp), 0, s.size - 1)
    return tuple(s[idx].tolist())  # type: ignore[return-value]


def _cell_shape(mesh: trimesh.Trimesh) -> tuple[np.ndarray, np.ndarray]:  # per-triangle aspect ratio and equiangular skewness
    tris = np.asarray(mesh.vertices, dtype=np.float64)[np.asarray(mesh.faces)]
    e0, e1, e2 = tris[:, 1] - tris[:, 0], tris[:, 2] - tris[:, 1], tris[:, 0] - tris[:, 2]
    lengths = np.stack([np.linalg.norm(e0, axis=1), np.linalg.norm(e1, axis=1), np.linalg.norm(e2, axis=1)], axis=1)
    longest, shortest = lengths.max(axis=1), np.clip(lengths.min(axis=1), 1e-12, None)
    aspect = longest / shortest
    cos_a = np.clip(-np.sum(e2 * e0, axis=1) / (lengths[:, 2] * lengths[:, 0]), -1.0, 1.0)
    cos_b = np.clip(-np.sum(e0 * e1, axis=1) / (lengths[:, 0] * lengths[:, 1]), -1.0, 1.0)
    cos_c = np.clip(-np.sum(e1 * e2, axis=1) / (lengths[:, 1] * lengths[:, 2]), -1.0, 1.0)
    cosines = np.stack([cos_a, cos_b, cos_c], axis=1)
    skew = np.clip((cosines.min(axis=1) - 0.5) / -0.5, 0.0, 1.0)  # equiangular cell at cos(60°)=0.5 → 0 skew; degenerate → 1
    return aspect, skew


# MANIFOLD3D-tier offloaded kernel: composes repair's public `to_manifold` build, gates `status()` — `Manifold(mesh)` sets a
# non-`NoError` status rather than raising on a non-2-manifold soup — and returns the picklable ExactTopology VALUE, because a live
# `Manifold` is a pybind11 handle no pickler carries; the capsule caches the value across metric passes, never a live `_solid`.
def _topology_kernel(mesh: trimesh.Trimesh) -> ExactTopology:
    import manifold3d

    solid = to_manifold(mesh)
    if solid.status() != manifold3d.Error.NoError:  # a non-2-manifold soup rails rather than yielding a phantom genus/mass
        raise QualityFault(rejected=solid.status().name)
    parts = solid.decompose()
    genus = sum(int(c.genus()) for c in parts)  # genus() is per connected component; sum over disconnected bodies
    return ExactTopology(
        genus, solid.num_vert(), solid.num_edge(), solid.num_tri(), len(parts), float(solid.volume()), float(solid.surface_area())
    )


# CPU-bound edge-collapse rides the warm process lane; the conditioned mesh is numpy-backed and pickles home whole.
def _decimate_kernel(mesh: trimesh.Trimesh, target_faces: int) -> trimesh.Trimesh:
    return mesh.simplify_quadric_decimation(face_count=target_faces)  # keyword: positional arg 0 is `percent`, not the face budget


# --- [SERVICES] -------------------------------------------------------------------------


class MeshQuality:  # structural ReceiptContributor conformance — the base adds nothing, so no subclass
    def __init__(self, mesh: trimesh.Trimesh, lane: LanePolicy, backend: QualityBackend | None = None) -> None:
        self._mesh = mesh
        self._lane = lane  # the offload seam; the lane never imports the kernel
        self._backend = backend or QualityBackend.resolve()
        self._last: MeshQualityReceipt | None = None
        self._exact: ExactTopology | None = None  # the cached VALUE; a live Manifold handle never crosses the pickle seam

    @overload
    async def apply(self, op: MeshQualityOp) -> "RuntimeRail[MeshQualityResult]": ...
    @overload
    async def apply(self, op: Sequence[MeshQualityOp]) -> "RuntimeRail[Block[MeshQualityResult]]": ...
    async def apply(self, op: MeshQualityOp | Sequence[MeshQualityOp]) -> "RuntimeRail[MeshQualityResult] | RuntimeRail[Block[MeshQualityResult]]":
        # a batch awaits each route IN ORDER — every conditioning arm lands its output through `_adopt`, so a
        # downstream Metrics reads the advanced mesh and never a stale capsule; the routes never fan out concurrently.
        match op:
            case MeshQualityOp() as one:
                return await self._route(one)
            case batch:
                rails = Block.of_seq([await self._route(one) for one in batch])
                return traversed(rails, by=Disposition.ABORT)  # abort on the first faulted op; the runtime owns the strategy row

    async def _route(self, op: MeshQualityOp) -> "RuntimeRail[MeshQualityResult]":
        # tier-aware fence: decimate and the MANIFOLD3D metrics kernel offload; subdivide/smooth/spine-metrics run under `boundary`.
        match op:
            case MeshQualityOp(tag="decimate", decimate=target_faces):
                before = len(self._mesh.faces)
                # HOSTILE names the warm process pool — a bare callable lifts PURE onto a subinterpreter the trimesh band never imports under.
                offloaded = await self._lane.offload(Kernel.of(_decimate_kernel, KernelTrait.HOSTILE), self._mesh, target_faces)
                return offloaded.map(
                    lambda out: self._arm(
                        op, Outcome(MeshQualityResult.Decimate(self._adopt(out)), before, len(out.faces), bool(out.is_watertight), 0.0, 0.0, 0)
                    )
                )
            case MeshQualityOp(tag="metrics"):
                # numpy half-edge/cell-shape fold ALWAYS runs under `boundary` on both tiers, so a degenerate-mesh numpy
                # raise converts to a BoundaryFault rather than escaping the rail.
                exact = await self._exact_topology() if self._backend is QualityBackend.MANIFOLD3D and bool(self._mesh.is_watertight) else Ok(None)
                return exact.bind(lambda e: boundary("mesh.quality.metrics", lambda: self._arm(op, self._metrics_outcome(e))))
            case _:
                return boundary(f"mesh.quality.{op.tag}", lambda: self._arm(op, self._spine(op)))

    async def _exact_topology(self) -> "RuntimeRail[ExactTopology]":  # one offloaded HOSTILE build; the returned VALUE reuses across passes
        if self._exact is not None:
            return Ok(self._exact)
        rail = await self._lane.offload(Kernel.of(_topology_kernel, KernelTrait.HOSTILE), self._mesh)
        return rail.map(self._cache_exact)

    def _cache_exact(self, exact: ExactTopology) -> ExactTopology:
        self._exact = exact
        return exact

    def _adopt(self, mesh: trimesh.Trimesh) -> trimesh.Trimesh:
        # every conditioning arm lands its output here: the capsule mesh advances and the cached exact topology
        # invalidates together, so a later Metrics reads the mutated surface and recomputes its exact evidence.
        self._mesh = mesh
        self._exact = None
        return mesh

    def _arm(self, op: MeshQualityOp, out: Outcome) -> MeshQualityResult:  # the receipt cross-cut; a new arm writes only its Outcome body
        self._last = MeshQualityReceipt(
            op.tag, self._backend, out.faces_before, out.faces_after,
            out.watertight, out.worst_aspect_ratio, out.worst_skewness, out.genus,
        )
        return out.result

    def contribute(self) -> Iterable[Receipt]:
        r = self._last or MeshQualityReceipt("metrics", self._backend, 0, 0, True, 0.0, 0.0, 0)
        phase: Phase = "emitted" if r.watertight else "admitted"  # a non-watertight grade is a flagged caveat, never an asserted closed-solid claim
        facts: dict[str, object] = {  # native scalars for the receipts enc_hook=repr renderer
            "backend": r.backend.value, "faces_before": r.faces_before,
            "faces_after": r.faces_after, "watertight": r.watertight,
            "worst_aspect_ratio": r.worst_aspect_ratio, "worst_skewness": r.worst_skewness,
            "genus": r.genus,
        }
        yield Receipt.of("rasm.geometry.mesh.quality", (phase, r.op, facts))  # subject is the op tag, never duplicated into a facts slot

    def _spine(self, op: MeshQualityOp) -> Outcome:  # the in-place conditioning arms only
        before = len(self._mesh.faces)
        match op:
            case MeshQualityOp(tag="subdivide", subdivide=(max_edge, iterations)):
                verts, faces = self._mesh.vertices, self._mesh.faces
                for _ in range(iterations):
                    verts, faces = trimesh.remesh.subdivide_to_size(verts, faces, max_edge)
                # merge edge-split vertices so is_watertight reads true; `_adopt` advances the capsule and drops the exact cache
                out = self._adopt(trimesh.Trimesh(vertices=verts, faces=faces, process=True))
                return Outcome(MeshQualityResult.Subdivide(out), before, len(out.faces), bool(out.is_watertight), 0.0, 0.0, 0)
            case MeshQualityOp(tag="smooth", smooth=(kind, iterations, factor)):
                self._smooth(kind, iterations, factor)
                self._adopt(self._mesh)  # in-place filter moved vertices, so the cached exact volume/area evidence is stale
                return Outcome(MeshQualityResult.Smooth(self._mesh), before, len(self._mesh.faces), bool(self._mesh.is_watertight), 0.0, 0.0, 0)
            case _ as unreachable:  # only subdivide/smooth reach here; a stray kind is a routing fault
                raise AssertionError(unreachable.tag)

    def _smooth(self, kind: SmoothKind, iterations: int, factor: float) -> None:
        match kind:  # one factor row drives each filter's defining parameter
            case SmoothKind.TAUBIN:
                # trimesh SUBTRACTS nu on odd passes: nu is a POSITIVE magnitude solved from the reciprocal band, never a negative
                # nu (double-negates into a second shrink) nor a fixed offset (breaks the band at small lamb).
                trimesh.smoothing.filter_taubin(self._mesh, lamb=factor, nu=factor / (1.0 - _TAUBIN_K * factor), iterations=iterations)
            case SmoothKind.LAPLACIAN:
                trimesh.smoothing.filter_laplacian(self._mesh, lamb=factor, iterations=iterations)
            case SmoothKind.HUMPHREY:
                trimesh.smoothing.filter_humphrey(self._mesh, alpha=factor, iterations=iterations)
            case unreachable:
                assert_never(unreachable)

    def _metrics_outcome(self, exact: ExactTopology | None) -> Outcome:
        metrics = closure_fold(self._mesh, exact)
        worst = metrics.worst
        before = len(self._mesh.faces)
        return Outcome(MeshQualityResult.Metrics(metrics), before, before, metrics.watertight, worst[0], worst[1], metrics.genus)


def closure_fold(mesh: trimesh.Trimesh, exact: ExactTopology | None = None) -> QualityMetrics:
    faces = np.asarray(mesh.faces)
    half_edges = np.sort(  # endpoint-sorted so an edge and its reverse group together
        np.concatenate([faces[:, [0, 1]], faces[:, [1, 2]], faces[:, [2, 0]]]), axis=1
    )
    unique_edges, counts = np.unique(half_edges, axis=0, return_counts=True)  # exact per-unique-edge incidence, never a positional edges_unique/face_adjacency alignment
    boundary_edges = int(np.sum(counts == 1))      # incidence 1 → boundary
    nonmanifold_edges = int(np.sum(counts >= 3))   # incidence ≥3 → non-manifold
    spine = (len(mesh.vertices), len(unique_edges), len(mesh.faces))  # E is the unique-edge fold's own count — one truth
    v, e, f = (exact.vertex_count, exact.edge_count, exact.face_count) if exact else spine
    genus = exact.genus if exact else max(0, (2 - (spine[0] - spine[1] + spine[2])) // 2)  # exact override, else Euler V−E+F = 2−2g
    area = exact.area if exact else float(mesh.area)  # the kernel mass supersedes the cached trimesh measure
    volume = exact.volume if exact else float(mesh.volume)
    components = exact.components if exact else int(mesh.body_count)
    aspect, skew = _cell_shape(mesh)
    defects = np.asarray(mesh.vertex_defects)  # per-vertex angle defect, a curvature proxy
    return QualityMetrics(
        bool(mesh.is_watertight), bool(mesh.is_winding_consistent),
        area, volume, v, f, e,
        boundary_edges, nonmanifold_edges, components, genus, v - e + f,
        _quantiles(aspect), _quantiles(skew),
        float(np.mean(defects)) if defects.size else 0.0,
        float(np.std(defects)) if defects.size else 0.0,
    )
```

## [03]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)

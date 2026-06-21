# [PY_GEOMETRY_MESH_QUALITY]

Mesh-topology conditioning and metrology over an in-memory triangulation — the decimation, subdivision, smoothing, and metric primitive the tessellation, scan-reconstruction, and step hops compose to coarsen, refine, denoise, and grade a built surface before it crosses a downstream rail. `MeshQualityOp` is one tagged union discriminating by operation kind, `apply` is one polymorphic entrypoint folding a single op or a batch sequence through one rail, and `MeshQualityResult` is one union carrier whose case mirrors the op case so a decimated mesh, a subdivided mesh, a smoothed mesh, and a metric record are four arms of one result owner rather than four parallel result types. The conditioning arms (`Decimate`/`Subdivide`/`Smooth`) each carry an algorithm row — the decimation target, the subdivision edge ceiling and iteration count, and the `SmoothKind` filter selector with its `factor`/`iterations` band — so the smoothing family is one parameterized row, never three Taubin/Laplacian/Humphrey entrypoints; the `Metrics` arm folds the cached `trimesh.Trimesh` validity, mass, topology, and normal axes plus per-cell `numpy` shape statistics into one `QualityMetrics` value object carrying aspect-ratio, skewness, angle-defect, manifold-edge, and genus distributions, never a metric-per-method family. The spine is `trimesh` plus `numpy` (intended cp315 core), composed at depth: the conditioning arms weave `simplify_quadric_decimation`/`subdivide_to_size`/`smoothing.filter_taubin`/`filter_laplacian`/`filter_humphrey` over the in-memory body, and the metric arm weaves the lazily-cached `area`/`volume`/`is_watertight`/`is_winding_consistent`/`face_normals`/`vertex_normals`/`edges_unique`/`vertex_defects` axes plus the exact per-edge incidence from the sorted half-edge fold over `mesh.faces` with `numpy` `linalg.norm`/`sort`/`unique`/`clip`/`mean`/`std` reductions into one statistical receipt — never a single flat call. The `manifold3d` exact-topology surface is the one GATED ENRICHMENT tier on the `python_version<'3.15'` companion band, and it is stacked — not a lone `genus()` call: a `Manifold` built once from the surface and cached on the capsule weaves `genus`/`num_edge`/`num_tri`/`num_vert`/`volume`/`surface_area` into one `ExactTopology` value object that sharpens the genus, the edge/face/vertex counts, AND supersedes the `trimesh` `area`/`volume` mass with the exact kernel measure in a single fold, so the spine's Euler-characteristic genus and `trimesh`-derived counts are replaced wholesale by exact integers when the tier resolves, never re-built per metric. The spine derives genus from the Euler characteristic over `edges_unique`/`faces`/`vertices`, resolved per interpreter through `QualityBackend` (`SPINE`/`MANIFOLD3D`), never a second metric owner. This owner is the pure kernel implementing `ReceiptContributor`: it conditions and grades an in-memory `trimesh.Trimesh` and returns in-memory geometry plus typed metrics across the wire — mesh-file decode/encode is the data `MeshPayload` owner (`rasm.data.spatial.mesh`), and render-time decimation is the artifacts figures owner, so the geometry shed never writes a mesh file nor coarsens for display.

## [01]-[INDEX]

- [01]-[QUALITY]: the decimate, subdivide, smooth, and metrics operations under one tagged union over the `trimesh`/`numpy` spine, woven from stacked `.api` members per arm, with the gated `manifold3d` exact-topology enrichment tier (genus + edge/tri/vert counts + kernel mass superseding the trimesh measure, folded into one cached `ExactTopology`) resolved per interpreter through the two-tier `QualityBackend`, returning one unified `MeshQualityResult` union and contributing one typed `MeshQualityReceipt`.

## [02]-[QUALITY]

- Owner: `MeshQuality` — the boundary capsule binding the in-memory `trimesh.Trimesh` to the four conditioning/metric arms, dispatching every op kind through `match`/`assert_never`, and implementing `ReceiptContributor` so the last op's facts contribute one `MeshQualityReceipt` row; `MeshQualityOp` the tagged union discriminating by operation so decimate/subdivide/smooth/metrics are four cases of one request rather than four entrypoints; `SmoothKind` the closed three-row `StrEnum` (`TAUBIN`/`LAPLACIAN`/`HUMPHREY`) so the smoothing filter family is one row on the `Smooth` case, never three sibling smoothing methods; `MeshQualityResult` the union carrier whose case mirrors the op case (`Decimate`/`Subdivide`/`Smooth` carrying the conditioned `trimesh.Trimesh`, `Metrics` carrying the `QualityMetrics` value object); `QualityBackend` the closed two-tier `StrEnum` selecting the always-available `SPINE` Euler-characteristic genus versus the gated `MANIFOLD3D` exact integer topology, with `QualityBackend.resolve` mapping the live interpreter to `MANIFOLD3D` only when its wheel loads and to `SPINE` otherwise; `ExactTopology` the frozen value object the `MANIFOLD3D` tier folds in one pass — exact `genus`/`edge_count`/`face_count`/`vertex_count` plus the kernel `volume`/`area` mass — over a `Manifold` built once and cached on the capsule, so the metric fold overrides the spine's Euler genus and `trimesh` counts wholesale rather than reading one `genus()` and re-deriving the rest; `QualityMetrics` the frozen value object carrying the watertight/winding verdict, the area/volume mass, the face/vertex/edge counts, the non-manifold and boundary edge counts, the genus, the Euler characteristic, and the five-number aspect-ratio and equiangular-skewness distributions (min/q1/median/q3/max) plus the angle-defect mean/std, with a `worst` projection folding the distribution tails into the single conditioning verdict the consumer reads; `Outcome` the one struct each arm returns carrying its `MeshQualityResult` plus the receipt facts (the operation tag, the before/after face count, the watertight verdict, the worst aspect-ratio and skewness, the genus, and the backend tier) so the `@arm` aspect folds one `MeshQualityReceipt` from a uniform payload instead of a per-arm receipt build; `MeshQualityReceipt` the typed receipt carrying the op tag, the backend tier, the before/after face count, the watertight verdict, the worst aspect-ratio and skewness, the genus, and the conditioning verdict.
- Cases: `MeshQualityOp` cases `Decimate(target_faces)` (the quadric edge-collapse simplification through `simplify_quadric_decimation(face_count)` toward the target triangle budget, the topology coarsening the tessellation hop runs before a downstream solid op — distinct from render decimation, which is the artifacts owner), `Subdivide(max_edge, iterations)` (the edge-length-bounded refinement through `remesh.subdivide_to_size(vertices, faces, max_edge)` toward a maximum edge length, the topology densification the scan-reconstruction hop runs before a curvature-sensitive metric pass), `Smooth(kind, iterations, factor)` (the Laplacian-family denoising through the `SmoothKind`-selected `smoothing.filter_taubin`/`filter_laplacian`/`filter_humphrey` over the in-place body, the noise conditioning the scan-reconstruction hop runs before a deviation pass, the filter a `SmoothKind` row not a parallel entrypoint), and `Metrics()` (the full validity/mass/topology/shape grade folding the cached `trimesh` axes plus the per-cell `numpy` aspect-ratio and equiangular-skewness statistics into one `QualityMetrics`, the gate the daemon and the clash/deviation hops read before trusting a surface) — matched by `match`/`assert_never`, each binding the `.api` members that own the kind. Inputs are face budgets, edge ceilings, iteration counts, and `SmoothKind` rows; outputs are the conditioned `trimesh.Trimesh` or the `QualityMetrics` record carried in the mirrored `MeshQualityResult` arm. No mesh-file format axis lives here — the surface arrives as an in-memory `trimesh.Trimesh` across the `mesh ← data/spatial` seam and the conditioned body rides back the same way.
- Entry: `apply` is the one polymorphic entrypoint discriminating a single `MeshQualityOp` or a batch `Sequence[MeshQualityOp]` — a single op lifts through one `boundary`, a batch builds a `Block` of per-op `boundary` rails in one comprehension and folds them through `runtime.faults.traversed(accumulate=False)` into one `RuntimeRail[Block[MeshQualityResult]]`, never a second batch method nor a quadratic singleton-append — and returns a `RuntimeRail[MeshQualityResult]` over the capsule's surface. The `Decimate` arm calls `mesh.simplify_quadric_decimation(target_faces)` returning a new coarsened `Trimesh` and reads the realized face count off the result rather than asserting the target; the `Subdivide` arm folds `remesh.subdivide_to_size(mesh.vertices, mesh.faces, max_edge)` over the supplied iteration count, rebuilding a `Trimesh` from the returned vertices/faces; the `Smooth` arm selects the `SmoothKind`-keyed `smoothing.filter_*` filter binding its defining parameters from the one `factor` row (`filter_taubin`'s `lamb`/`nu` shrink-free band-pass pair derived as `lamb=factor`, `nu=-factor/(1-factor)` so the pass-band stays stable, `filter_laplacian`'s `lamb=factor`, `filter_humphrey`'s `alpha=factor`), runs it in place over the iteration count, and reads `is_watertight`/`is_winding_consistent` after; the `Metrics` arm folds the cached `area`/`volume`/`is_watertight`/`is_winding_consistent` validity-and-mass axes, derives the per-cell aspect-ratio and equiangular-skewness distributions from the triangle edge vectors through `numpy` `linalg.norm`/`sort`/`clip`/`mean`/`std`, counts the boundary (incidence 1), interior-manifold (incidence 2), and non-manifold (incidence ≥3) edges by folding the sorted half-edge array off `mesh.faces` through `numpy.unique(..., axis=0, return_counts=True)` so the per-edge incidence is the exact group-count rather than a positional alignment of `edges_unique` against `face_adjacency`, and resolves topology through the `QualityBackend` tier — `MANIFOLD3D` folds one cached `ExactTopology` reading `manifold3d.Manifold.genus()`/`num_edge()`/`num_tri()`/`num_vert()`/`volume()`/`surface_area()` and supersedes the spine genus, the edge/face/vertex counts, and the trimesh mass in one pass, `SPINE` derives genus from the Euler characteristic `V − E + F = 2 − 2g` over `len(mesh.vertices)`/`len(mesh.edges_unique)`/`len(mesh.faces)`. The cross-cutting concern — folding each arm's `Outcome` into one `MeshQualityReceipt` row with the backend tier and the before/after face count — rides the real `@arm` decorator wrapping the match dispatch, so a new case writes only the geometry body returning an `Outcome`; the `_quantiles` five-number fold and the `_cell_shape` per-triangle aspect/skew kernel are shared `numpy` helpers the `Metrics` arm and the `QualityMetrics.worst` projection call, since the conditioning arms carry no per-cell distribution.
- Auto: `Trimesh.simplify_quadric_decimation(face_count)` (`trimesh.md` entrypoint row [11]) quadric-collapses toward the target triangle budget and returns a new `Trimesh`, the topology coarsening distinct from the render-time decimation the artifacts owner runs; `remesh.subdivide_to_size(vertices, faces, max_edge)` (row [12]) refines every edge past the ceiling and returns new vertices/faces; `smoothing.filter_taubin`/`filter_laplacian`/`filter_humphrey` (rows [08]/[09]/[10]) mutate the mesh in place toward a denoised surface, Taubin shrink-free, the filter a `SmoothKind` row; `Trimesh.area`/`volume` (`trimesh.md` property row [01]), `is_watertight`/`is_winding_consistent` (row [03]), `face_normals`/`vertex_normals` (row [06]), `edges_unique` (row [07], the unique-edge count for the Euler characteristic), and `vertex_defects` (row [08], the per-vertex angle defect the `angle_defect` mean/std fold reads) are lazily-cached properties read once per metric pass and recomputed only on geometry change, while the per-edge boundary/manifold/non-manifold incidence derives from the sorted half-edge array off `mesh.faces` through `numpy.unique(..., axis=0, return_counts=True)`; on the gated `MANIFOLD3D` tier `manifold3d.Manifold.genus()` (`manifold3d.md` topology row [05]), `num_edge()`/`num_tri()`/`num_vert()` (rows [11]/[10]/[09]), and `volume()`/`surface_area()` (rows [06]/[07]) are read in one fold off a `Manifold` built once from the surface's `Mesh`/`Mesh64` and cached on the capsule (`_solid`, reused across metric passes exactly as the `mesh/spatial.md#SPATIAL` sibling caches its ray/clearance solid), the surface entering as `manifold3d.Mesh` (32-bit `tri_verts`) or `Mesh64` (`manifold3d.md` type rows [03]/[04]) selected by vertex count so a surface past the `uint32` ceiling never silently truncates its triangle indices — the one build powers the exact genus, the exact edge/face/vertex counts, and the kernel mass that supersedes the `trimesh` measure, never one `genus()` call discarding the rest of the solid.
- Receipt: `MeshQuality` conforms to the runtime `ReceiptContributor` Protocol — the `@arm` decorator folds each arm's returned `Outcome` into one `MeshQualityReceipt`, and `contribute` emits one `Receipt.of(phase, "mesh.quality", tag, facts)` row carrying the op tag, the `QualityBackend.value` tier string, the before/after face count, the watertight verdict, the worst aspect-ratio and skewness, and the genus; a `Metrics` pass over a non-watertight or non-winding-consistent surface keys `phase="admitted"` (the `Phase` literal the receipt owner admits) so the row is a caveat flagging the unreliable genus/volume rather than asserting a closed-solid grade, while a clean conditioning or grade keys `phase="emitted"`. The quality pass produces no `GraduationReceipt` subject — it is the read-and-condition primitive the tessellation, deviation, and reconstruction hops consume, and the graduating subject (`reconstructed-mesh`, `mesh-algebra`) belongs to the `mesh/repair` and reconstruction owners that emit the conditioned solid; the canonical `GeometrySubject` literal (`rasm.compute.graduation.handoff#GeometrySubject`) is never minted here.
- Packages: `trimesh` (`Trimesh`/`Trimesh.area`/`Trimesh.volume`/`Trimesh.is_watertight`/`Trimesh.is_winding_consistent`/`Trimesh.face_normals`/`Trimesh.vertex_normals`/`Trimesh.edges_unique`/`Trimesh.vertex_defects`/`Trimesh.simplify_quadric_decimation`/`remesh.subdivide_to_size`/`smoothing.filter_taubin`/`smoothing.filter_laplacian`/`smoothing.filter_humphrey`), `numpy` (`ndarray`/`asarray`/`sort`/`stack`/`concatenate`/`clip`/`mean`/`std`/`min`/`max`/`sum`/`unique`/`iinfo`/`linalg.norm` over the per-cell `Nx3` edge-vector shapes, the sorted half-edge incidence fold, and the five-number distribution fold), `manifold3d` (the gated exact-topology tier — `Manifold`/`Mesh`/`Mesh64`/`Manifold.genus`/`Manifold.num_edge`/`Manifold.num_tri`/`Manifold.num_vert`/`Manifold.volume`/`Manifold.surface_area`, folded into one cached `ExactTopology` reached only on the `MANIFOLD3D` backend row), runtime (`RuntimeRail`/`boundary`/`traversed`/`ReceiptContributor`/`Receipt`).
- Growth: a new conditioning op is one `MeshQualityOp` case plus the mirrored `MeshQualityResult` arm returning an `Outcome` over the in-memory body — never a new entrypoint, and the `@arm` aspect folds its receipt without a per-case receipt build; a new smoothing filter is one `SmoothKind` row plus its `smoothing.filter_*` binding inside the `Smooth` arm, never a fourth smoothing method; a new shape metric is one `QualityMetrics` field plus its `numpy` reduction inside the shared `_cell_shape`/`_quantiles` kernel, never a metric-per-method family; a new exact-topology tier is one `QualityBackend` row plus its `resolve` branch and one `ExactTopology` fold only when this owner actually dispatches it, never a second metric owner; a new exact axis the kernel exposes is one `ExactTopology` field plus its `Manifold` read inside the single cached fold, never a second manifold build; the tessellation hop composes `Decimate(target_faces)` to fit a triangle budget before transport then `Metrics()` to certify the coarsened topology, the scan-reconstruction hop composes `Smooth(kind, iterations)` then `Metrics()` before a deviation pass, and the clash/deviation hops read `QualityMetrics.worst` for the single conditioning verdict rather than re-deriving cell shape; the quadric-decimation and the `manifold3d` exact-genus batch (the CPU-bound passes on the `<'3.15'` companion band) hand across the runtime `execution/lanes#LANES` `LanePolicy.offload` per-subinterpreter variant (`anyio.to_interpreter.run_sync` under one `CapacityLimiter`, the no-pickle PEP-734 hop, degrading to `anyio.to_thread.run_sync` only where a cp315 build ships no runnable `concurrent.interpreters`, NEVER a `to_process` pickle round-trip the lanes owner rejects as the process-pool serialization tax) as ONE `offload(kernel, *args)` hand-off call over the already-landed lane — the lane never imports the kernel; zero new surface, no parallel per-op class family.
- Boundary: no robust watertight repair, hole-fill, or boolean CSG (that is `mesh/repair`); no proximity, ray, contains, or sampling query (that is `mesh/spatial`); no point-cloud registration or reconstruction (that is `scan/registration`+`scan/reconstruction`); the `manifold3d` exact-genus kernel is reached only through the `MANIFOLD3D` `QualityBackend` row, never a second direct topology owner; render-time decimation for display is NOT this owner — the artifacts figures owner coarsens for a viewport, while this owner coarsens mesh topology for a downstream geometry op, so a render-LOD or display-budget arm here is a deleted form that trespasses the `mesh ⇄ artifacts/figures` boundary; mesh-file decode/encode is NOT this owner — the data `MeshPayload` owner (`rasm.data.spatial.mesh`) holds the canonical three-engine `trimesh`/`meshio`/`rhino3dm` codec plus GLB preview, so geometry hands in-memory `Trimesh` across the `mesh ← data/spatial` seam and never opens or writes a file. A `decimate`/`subdivide`/`smooth`/`metrics` method family over the `MeshQualityOp` row, three parallel `filter_taubin`/`filter_laplacian`/`filter_humphrey` smoothing entrypoints over the `SmoothKind` row, a metric-per-method family (`aspect_ratio`/`skewness`/`genus`/`manifold_edges` as separate methods) over the one `QualityMetrics` fold, four parallel `DecimateResult`/`SubdivideResult`/`SmoothResult`/`MetricsResult` structs, a hand-rolled quadric-collapse or Loop-subdivision kernel where `trimesh.simplify_quadric_decimation`/`remesh.subdivide_to_size` are admitted, a per-arm receipt build duplicating the `@arm` decorator, treating the `manifold3d` exact genus as the spine rather than a gated tier over the Euler-characteristic default, reading a lone `Manifold.genus()` and discarding the built solid instead of folding the cached `ExactTopology` (exact counts + kernel mass superseding the trimesh measure) off one reused `Manifold`, rebuilding the `Manifold` per metric pass rather than caching `_solid`, dropping the `Smooth` `factor` so Taubin runs without its band-pass `lamb`/`nu` pair, asserting a closed-solid genus/volume over a non-watertight surface instead of admitting the caveat, a render-LOD/display-budget decimation arm trespassing the artifacts boundary, and ANY `MeshFormat`/`Codec`/`load`/`export` arm re-deriving the `MeshPayload` seam are the deleted forms.

```python signature
from collections.abc import Callable, Sequence
from enum import StrEnum
from functools import wraps
from importlib.util import find_spec
from typing import TYPE_CHECKING, Literal, assert_never

import numpy as np
import trimesh
from expression import case, tag, tagged_union
from expression.collections import Block
from msgspec import Struct

from rasm.runtime.faults import RuntimeRail, boundary, traversed
from rasm.runtime.receipts import Receipt, ReceiptContributor

if TYPE_CHECKING:
    import manifold3d

OpKind = Literal["decimate", "subdivide", "smooth", "metrics"]
type Arm = Callable[["MeshQuality", MeshQualityOp], "Outcome"]


class SmoothKind(StrEnum):  # the smoothing filter family is one row on the Smooth case, never three parallel entrypoints
    TAUBIN = "taubin"        # shrink-free band-pass Laplacian
    LAPLACIAN = "laplacian"  # uniform-weight Laplacian
    HUMPHREY = "humphrey"    # Humphrey classes anti-shrink


class QualityBackend(StrEnum):  # only the two tiers this owner dispatches for exact-versus-Euler genus
    SPINE = "spine"            # numpy Euler characteristic V−E+F=2−2g over trimesh edges_unique, always-available cp315 core
    MANIFOLD3D = "manifold3d"  # gated <3.15 companion: the exact integer genus/num_edge surface

    @staticmethod
    def resolve() -> "QualityBackend":  # the cp315 venv carries no manifold3d wheel, so exact genus resolves only on the companion lane
        return QualityBackend.MANIFOLD3D if find_spec("manifold3d") is not None else QualityBackend.SPINE


class ExactTopology(Struct, frozen=True):  # the MANIFOLD3D tier's one fold over a single cached Manifold, never a lone genus() read
    genus: int
    vertex_count: int
    edge_count: int
    face_count: int
    volume: float
    area: float


class QualityMetrics(Struct, frozen=True):  # the one shape/validity/topology grade, never a metric-per-method family
    watertight: bool
    winding_consistent: bool
    area: float
    volume: float
    vertex_count: int
    face_count: int
    edge_count: int
    boundary_edges: int
    nonmanifold_edges: int
    genus: int
    euler_characteristic: int
    aspect_ratio: tuple[float, float, float, float, float]   # five-number (min, q1, median, q3, max)
    skewness: tuple[float, float, float, float, float]        # equiangular skewness five-number summary
    angle_defect_mean: float  # mean per-vertex angle defect (Trimesh.vertex_defects), a curvature signal, never vertex valence
    angle_defect_std: float

    @property
    def worst(self) -> tuple[float, float]:  # the conditioning verdict the deviation/clash hops read: tail aspect-ratio and skewness
        return (self.aspect_ratio[4], self.skewness[4])


class MeshQualityReceipt(Struct, frozen=True):
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
    def Decimate(target_faces: int) -> "MeshQualityOp":  # only the catalogued face_count argument; no uncatalogued aggression keyword
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


class Outcome(Struct, frozen=True):  # one arm's payload plus the receipt facts the @arm aspect folds, never a per-arm receipt build
    result: MeshQualityResult
    faces_before: int
    faces_after: int
    watertight: bool
    worst_aspect_ratio: float
    worst_skewness: float
    genus: int


def _quantiles(values: np.ndarray) -> tuple[float, float, float, float, float]:  # five-number summary over a sorted view, no np.percentile dependency
    s = np.sort(values)
    n = s.size
    if n == 0:
        return (0.0, 0.0, 0.0, 0.0, 0.0)
    pick = lambda q: float(s[min(n - 1, max(0, int(round(q * (n - 1)))))])
    return (float(s[0]), pick(0.25), pick(0.5), pick(0.75), float(s[-1]))


def _cell_shape(mesh: trimesh.Trimesh) -> tuple[np.ndarray, np.ndarray]:  # per-triangle aspect ratio and equiangular skewness from edge vectors
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


def arm(fn: Arm) -> Callable[["MeshQuality", MeshQualityOp], MeshQualityResult]:  # the cross-cutting aspect: one receipt fold, geometry-only bodies below
    @wraps(fn)
    def _wrapped(self: "MeshQuality", op: MeshQualityOp) -> MeshQualityResult:
        out = fn(self, op)
        self._last = MeshQualityReceipt(
            op.tag, self._backend, out.faces_before, out.faces_after,
            out.watertight, out.worst_aspect_ratio, out.worst_skewness, out.genus,
        )
        return out.result

    return _wrapped


class MeshQuality(ReceiptContributor):
    def __init__(self, mesh: trimesh.Trimesh, backend: QualityBackend | None = None) -> None:
        self._mesh = mesh
        self._backend = backend or QualityBackend.resolve()
        self._last: MeshQualityReceipt | None = None
        self._solid: "manifold3d.Manifold | None" = None  # built once on the MANIFOLD3D tier, reused across metric passes (parity with mesh/spatial)

    def apply(self, op: MeshQualityOp | Sequence[MeshQualityOp]) -> "RuntimeRail[MeshQualityResult] | RuntimeRail[Block[MeshQualityResult]]":
        if isinstance(op, MeshQualityOp):
            return boundary(f"mesh.quality.{op.tag}", lambda: self._arm(op))
        rails = Block(boundary(f"mesh.quality.{one.tag}", lambda one=one: self._arm(one)) for one in op)
        return traversed(rails, accumulate=False)

    def contribute(self) -> Receipt:
        r = self._last or MeshQualityReceipt("metrics", self._backend, 0, 0, True, 0.0, 0.0, 0)
        phase: Literal["admitted", "emitted"] = "emitted" if r.watertight else "admitted"
        facts = {
            "op": r.op, "backend": r.backend.value, "faces_before": str(r.faces_before),
            "faces_after": str(r.faces_after), "watertight": str(r.watertight),
            "worst_aspect_ratio": f"{r.worst_aspect_ratio:.4f}", "worst_skewness": f"{r.worst_skewness:.4f}",
            "genus": str(r.genus),
        }
        return Receipt.of(phase, "mesh.quality", r.op, facts)

    @arm
    def _arm(self, op: MeshQualityOp) -> Outcome:
        before = len(self._mesh.faces)
        match op:
            case MeshQualityOp(tag="decimate", decimate=target_faces):
                out = self._mesh.simplify_quadric_decimation(target_faces)
                return Outcome(MeshQualityResult.Decimate(out), before, len(out.faces), bool(out.is_watertight), 0.0, 0.0, 0)
            case MeshQualityOp(tag="subdivide", subdivide=(max_edge, iterations)):
                verts, faces = self._mesh.vertices, self._mesh.faces
                for _ in range(iterations):
                    verts, faces = trimesh.remesh.subdivide_to_size(verts, faces, max_edge)
                out = trimesh.Trimesh(vertices=verts, faces=faces)
                return Outcome(MeshQualityResult.Subdivide(out), before, len(out.faces), bool(out.is_watertight), 0.0, 0.0, 0)
            case MeshQualityOp(tag="smooth", smooth=(kind, iterations, factor)):
                self._smooth(kind, iterations, factor)
                return Outcome(MeshQualityResult.Smooth(self._mesh), before, len(self._mesh.faces), bool(self._mesh.is_watertight), 0.0, 0.0, 0)
            case MeshQualityOp(tag="metrics", metrics=_):
                metrics = self._metrics()
                worst = metrics.worst
                return Outcome(MeshQualityResult.Metrics(metrics), before, before, metrics.watertight, worst[0], worst[1], metrics.genus)
            case unreachable:
                assert_never(unreachable)

    def _smooth(self, kind: SmoothKind, iterations: int, factor: float) -> None:
        match kind:  # one factor row drives each filter's defining parameter; Taubin folds its shrink-free band-pass lamb/nu pair, never a bare iteration count
            case SmoothKind.TAUBIN:
                trimesh.smoothing.filter_taubin(self._mesh, lamb=factor, nu=-factor / (1.0 - factor), iterations=iterations)
            case SmoothKind.LAPLACIAN:
                trimesh.smoothing.filter_laplacian(self._mesh, lamb=factor, iterations=iterations)
            case SmoothKind.HUMPHREY:
                trimesh.smoothing.filter_humphrey(self._mesh, alpha=factor, iterations=iterations)
            case unreachable:
                assert_never(unreachable)

    def _metrics(self) -> QualityMetrics:
        mesh = self._mesh
        faces = np.asarray(mesh.faces)
        half_edges = np.sort(  # the three directed half-edges per face, endpoint-sorted so an edge and its reverse group together
            np.concatenate([faces[:, [0, 1]], faces[:, [1, 2]], faces[:, [2, 0]]]), axis=1
        )
        _, counts = np.unique(half_edges, axis=0, return_counts=True)  # exact per-unique-edge incidence; no positional edges_unique/face_adjacency alignment
        boundary_edges = int(np.sum(counts == 1))      # incidence 1 → boundary
        nonmanifold_edges = int(np.sum(counts >= 3))   # incidence ≥3 → non-manifold
        spine = (len(mesh.vertices), len(mesh.edges_unique), len(mesh.faces))
        watertight = bool(mesh.is_watertight)
        exact = self._topology() if self._backend is QualityBackend.MANIFOLD3D and watertight else None
        v, e, f = (exact.vertex_count, exact.edge_count, exact.face_count) if exact else spine
        genus = exact.genus if exact else max(0, (2 - (spine[0] - spine[1] + spine[2])) // 2)  # exact override, else Euler V−E+F = 2−2g
        area = exact.area if exact else float(mesh.area)  # the gated tier's kernel mass supersedes the cached trimesh measure
        volume = exact.volume if exact else float(mesh.volume)
        aspect, skew = _cell_shape(mesh)
        defects = np.asarray(mesh.vertex_defects)  # per-vertex angle defect (Gaussian-curvature proxy), folded as angle_defect mean/std, never vertex valence
        return QualityMetrics(
            watertight, bool(mesh.is_winding_consistent),
            area, volume, v, f, e,
            boundary_edges, nonmanifold_edges, genus, v - e + f,
            _quantiles(aspect), _quantiles(skew),
            float(np.mean(defects)) if defects.size else 0.0,
            float(np.std(defects)) if defects.size else 0.0,
        )

    def _topology(self) -> ExactTopology:  # one fold over the single cached Manifold: genus + exact counts + kernel mass, never a lone genus()
        m = self._manifold()
        return ExactTopology(int(m.genus()), m.num_vert(), m.num_edge(), m.num_tri(), float(m.volume()), float(m.surface_area()))

    def _manifold(self) -> "manifold3d.Manifold":  # built once on the MANIFOLD3D tier, reused across metric passes
        if self._solid is None:
            self._solid = self._to_manifold(self._mesh)
        return self._solid

    @staticmethod
    def _to_manifold(mesh: trimesh.Trimesh) -> "manifold3d.Manifold":
        import manifold3d

        verts = np.asarray(mesh.vertices, dtype=np.float32)
        faces = np.asarray(mesh.faces)
        if len(verts) > np.iinfo(np.uint32).max:  # 32-bit Mesh overflows past ~4.29B verts; Mesh64 carries 64-bit triangle indices (.api type row [04])
            return manifold3d.Manifold(manifold3d.Mesh64(vert_properties=verts, tri_verts=faces.astype(np.uint64)))
        return manifold3d.Manifold(manifold3d.Mesh(vert_properties=verts, tri_verts=faces.astype(np.uint32)))
```

## [03]-[RESEARCH]

- [TRIMESH_CONDITIONING]: the `Decimate`/`Subdivide`/`Smooth` arms bind the `.api`-confirmed entrypoints exactly — `Trimesh.simplify_quadric_decimation(face_count)` (`trimesh.md` entrypoint row [11]) returns a new quadric-collapsed `Trimesh`, `remesh.subdivide_to_size(vertices, faces, max_edge)` (row [12]) returns refined vertices/faces, and `smoothing.filter_taubin`/`filter_laplacian`/`filter_humphrey` (rows [08]/[09]/[10]) mutate in place. The smoothing keyword pairs are the catalogued call-shapes: row [08] `filter_taubin(mesh)` is `mesh plus lambda/nu` (the shrink-free band-pass requires both the positive `lamb` shrink and the negative `nu` un-shrink, so the `Smooth` arm folds `nu=-factor/(1-factor)` from the one `factor` row rather than passing a bare iteration count that drops the un-shrink pass), row [09] `filter_laplacian(mesh)` is `mesh plus iterations` with `lamb` the step, and row [10] `filter_humphrey(mesh)` is `mesh plus alpha/beta`. `Trimesh.simplify_quadric_decimation` binds only the catalogued `face_count` argument (`trimesh.md` entrypoint row [11]) — the `Decimate` case carries `target_faces` alone and the arm reads the realized face count off the returned `Trimesh`; an `aggression`/quality keyword is NOT a catalogued call-shape and is never minted, so the budget is the one parameter and the realized coarsening is observed, not asserted.
- [CELL_SHAPE_STATISTICS]: the per-triangle aspect ratio and equiangular skewness are computed entirely from catalogued `numpy` reductions over the triangle edge-vector array — `np.asarray`/`stack`/`linalg.norm` (`numpy.md` linalg row [08]) for the three edge lengths, `min`/`max` (math row [03]) for the longest/shortest ratio, `sum` (row [01]) for the edge-dot cosine, `clip` (row [08]) to bound the cosine domain and the skew range, and `sort` (row [09]) for the five-number summary in `_quantiles` rather than `np.percentile` (uncatalogued, so the five-number fold derives from the sorted view's index positions). The equiangular skewness uses the cosine-domain formulation — the minimum interior-angle cosine over the three corners, mapped so the equiangular cell (`cos 60° = 0.5`) lands at 0 skew and the degenerate cell at 1 — avoiding `arccos`, which the `numpy.md` ufunc rows do not enumerate (rows [12]/[13] carry `sin`/`cos`/`tan` and `abs`/`sign`/`round` but no inverse trig). The angle-defect statistics fold `Trimesh.vertex_defects` (`trimesh.md` property row [08], the angle defect per vertex — a curvature signal, named `angle_defect_mean`/`angle_defect_std` for what it carries, never mislabelled vertex valence) through `mean`/`std` (`numpy.md` row [02]). The boundary/interior/non-manifold edge classification derives the exact per-edge incidence from the sorted directed-half-edge array built off `mesh.faces` (always-available, no cached topology axis) folded through `numpy.unique(half_edges, axis=0, return_counts=True)` (`numpy.md` set-ops row [10], counts requested): incidence 1 is a boundary edge, 2 is an interior-manifold edge, ≥3 is a non-manifold edge, and `len(edges_unique)` (`trimesh.md` row [07]) supplies the `E` axis for the Euler characteristic. This is exact by construction — every directed half-edge is counted once and grouped by its sorted endpoint pair — and never depends on a positional alignment of `edges_unique` against `face_adjacency`, which carries no such ordering guarantee.
- [EXACT_TOPOLOGY_TIER]: the `MANIFOLD3D` tier folds one `ExactTopology` over a single `Manifold` built once from the surface's `Mesh`/`Mesh64` (`manifold3d.md` type rows [03]/[04], `Mesh64` selected past the `uint32` vertex ceiling) and cached on the capsule (`_solid`) — `genus()` (`manifold3d.md` topology row [05]), `num_vert()`/`num_edge()`/`num_tri()` (query rows [09]/[11]/[10]), and `volume()`/`surface_area()` (measurement rows [06]/[07]) in one read, so the exact integer genus, the exact edge/face/vertex counts, and the kernel mass supersede the spine's Euler genus, `trimesh`-derived counts, and `trimesh` `area`/`volume` together rather than reading a lone `genus()` and re-deriving the rest from the looser spine axes. The `SPINE` tier derives genus from the Euler characteristic `V − E + F = 2 − 2g` over the `trimesh` `len(vertices)`/`len(edges_unique)`/`len(faces)` axes — exact for a closed orientable surface and an admitted caveat for a surface with boundary. `QualityBackend.resolve` probes `importlib.util.find_spec("manifold3d")` and returns `MANIFOLD3D` when the wheel loads, falling to `SPINE` otherwise, and the cached-solid build mirrors the `mesh/spatial.md#SPATIAL` `_solid` reuse exactly; the gated tier resolves only on the companion lane and the spine genus is the always-available default.

## [04]-[UPSTREAM]

- [WHEEL_BAND]: `trimesh` and `numpy` are pure-Python or prebuilt-wheel admitted on the intended cp315 core, so the conditioning surface (`simplify_quadric_decimation`/`subdivide_to_size`/`smoothing.filter_*`) and the full metric fold (validity/mass/topology axes plus the `numpy` cell-shape statistics and the Euler-characteristic genus) run on the project venv directly — the `SPINE` `QualityBackend.resolve` result is the always-available default. The single gated tier is companion-band only: `manifold3d` ships cp310-cp314 wheels on the `python_version<'3.15'` band (the `MANIFOLD3D` exact-genus row). The cp315 project venv carries no `manifold3d` wheel, so `find_spec("manifold3d")` returns `None` and `resolve` falls to `SPINE`; the gated `MANIFOLD3D` row resolves only on the Forge companion lane (`forge-companion-env`), and the spine metric pass never depends on it.
- [TOPOLOGY_BAND]: because the exact integer topology routes through `manifold3d` (the one `.api`-confirmed exact-topology surface), the `MANIFOLD3D` `ExactTopology` fold is a companion-band enrichment over the always-available Euler-characteristic spine genus and `trimesh`-derived counts/mass — there is no `Ray`/`Clearance`-style hard gate, since the metric fold never faults on the bare cp315 venv: the spine derives genus from the catalogued `trimesh` topology axes and mass from the cached `area`/`volume` properties, and the gated tier only sharpens an already-valid grade with exact genus, exact counts, and the kernel mass superseding the trimesh measure off one cached `Manifold`. The conditioning arms (`Decimate`/`Subdivide`/`Smooth`) are wheel-free on the project venv; the exact-topology tier is the `QualityBackend` row, never a parallel topology owner.
```

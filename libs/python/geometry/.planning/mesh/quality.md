# [PY_GEOMETRY_MESH_QUALITY]

Mesh-topology conditioning and metrology over an in-memory triangulation: `MeshQualityOp` discriminates decimate/subdivide/smooth/metrics on one polymorphic entrypoint, `MeshQualityResult` mirrors the op case, and `QualityMetrics` is the one shape/validity/topology grade — never a metric-per-method family. Tessellation, scan-reconstruction, and STEP hops compose this primitive to coarsen, refine, denoise, and grade a surface before it crosses a downstream boundary; the surface arrives and returns as in-memory `trimesh.Trimesh` across the `mesh ← data/spatial` boundary, and this owner never opens or writes a mesh file.

`closure_fold` is the public exact-closure truth composed by reconstruction and deviation. The exact topology enrichment builds through repair's public `to_manifold`, and CPU-bound kernels use the runtime HOSTILE lane. `QualityMetrics` retains topology and cell-distribution facts directly.

## [01]-[INDEX]

- [02]-[QUALITY]: decimate, subdivide, smooth, and metrics on one tagged union over the `trimesh`/`numpy` spine with the `manifold3d` exact-topology enrichment, returning one `MeshQualityResult` union.

## [02]-[QUALITY]

- Owner: `MeshQuality` — the boundary capsule over the four arms; `SmoothKind` makes the smoothing filter family one row on the `Smooth` case, never three parallel entrypoints; a new case writes only the geometry body producing its `MeshQualityResult` arm.
- Cases: `Decimate` coarsens mesh topology for a downstream geometry op — render-time decimation for display is the artifacts figures owner's, and an LOD/display-budget arm here trespasses that boundary; `Subdivide` densifies before a curvature-sensitive metric pass; `Smooth` denoises before a deviation pass; `Metrics` is the gate the daemon and the clash/deviation hops read before trusting a surface.
- Law: a grade that took no measurement records absence, never a zero — a face-less triangulation has no cell distribution and a vertex-less one no curvature, so both quantile summaries and both defect moments answer `Nothing`, `worst` answers `Nothing` with them, and the frame OMITS the columns rather than publishing a five-zero summary a gate reads as the best surface it has ever seen; a conditioning arm writes ONE `structlog` line at the capsule — face count before and after beside the watertight verdict — bound to the composition logger, so a decimate pass reads as its own census rather than as a grade it never took.
- Law: `_metrics_outcome` records the mesh genus/aspect charter rows at the producing fold. `QualityMetrics.frame` projects the complete topology census and cell distributions through `EvidenceFrame` using the caller's actual subject and key.
- Auto: the exact-topology fold is enrichment over the always-available Euler-characteristic spine, gated on watertightness alone — `manifold3d` carries no interpreter marker, so a tier branch over a probe that cannot fail is a dead arm, and the honest conditional path is the watertight precondition the exact kernel needs. One offloaded build yields the exact genus (summed over `decompose()` components), the exact counts, and the kernel mass superseding the `trimesh` measure in a single fold.
- Packages: `trimesh` (the conditioning filters, cached validity/mass axes, `vertex_defects`), `numpy` (the half-edge incidence fold and per-cell shape statistics), `manifold3d` (the exact tier, reached through repair's `to_manifold` over one module-scope `lazy import` for the `Error` status vocabulary alone), `expression` (`Option` the whole absence axis), `msgspec`, `structlog` through the runtime `logger`, geometry graduation (`EvidenceFrame`/`charter_record`, the charter measure authority, and `GeometryLeg` the folder roster this page's `FaultRow` rows anchor on), and the runtime results per the fence imports.
- Growth: a new conditioning op is one `MeshQualityOp` case, its mirrored `MeshQualityResult` arm, one body producing it, and one `FaultRow` row in `_ARM_RAISED` where the arm fences; a new smoothing filter is one `SmoothKind` row; a new exact-geometry provider is one `ManifoldTier` row at `mesh/repair#MESH`, never a probe minted here.
- Boundary: watertight repair, hole-fill, boolean CSG, and the `ManifoldTier` capability probe are `mesh/repair`'s; proximity, ray, contains, and sampling queries are `mesh/spatial`'s; registration and reconstruction are `scan/registration`+`scan/reconstruction`'s; mesh-file decode/encode is the data `MeshPayload` owner's (`rasm.data.spatial.mesh`).

```python
# --- [IMPORTS] --------------------------------------------------------------------------
from collections.abc import Sequence
from enum import StrEnum
from typing import Final, Literal, assert_never, overload

import numpy as np
import trimesh
from expression import Nothing, Ok, Option, Some, case, tag, tagged_union
from expression.collections import Block, Map
from msgspec import Struct

from rasm.geometry.graduation import EvidenceFrame, GeometryLeg, GeometrySubject, charter_record
from rasm.geometry.mesh.repair import to_manifold
from rasm.runtime.faults import TERMINAL, Catch, Disposition, FaultRow, RuntimeResult, boundary, rostered, traversed
from rasm.runtime.identity import ContentKey
from rasm.runtime.lanes import LanePolicy
from rasm.runtime.observe import DEFAULT_SCOPE, ScopeKey, logger
from rasm.runtime.workers import Kernel, KernelTrait

lazy import manifold3d

# --- [TYPES] ----------------------------------------------------------------------------

type OpKind = Literal["decimate", "subdivide", "smooth", "metrics"]
type Quantiles = tuple[float, float, float, float, float]


class SmoothKind(StrEnum):
    TAUBIN = "taubin"
    LAPLACIAN = "laplacian"
    HUMPHREY = "humphrey"


# --- [CONSTANTS] ------------------------------------------------------------------------

_QUANTILE_FRACTIONS: Final[np.ndarray] = np.array([0.0, 0.25, 0.5, 0.75, 1.0])
_QUANTILE_LABEL: Final[tuple[str, ...]] = ("min", "q1", "median", "q3", "max")
_TAUBIN_K: Final[float] = 0.05

_TRIMESH_RAISES: Final[Catch] = (IndexError, TypeError, ValueError)

# --- [TABLES] ---------------------------------------------------------------------------

QUALITY_METRICS: Final[FaultRow[GeometryLeg]] = FaultRow(
    leg=GeometryLeg.QUALITY, point="metrics", arm="boundary", defect="grade-refused", retriability=TERMINAL
)
QUALITY_SUBDIVIDE: Final[FaultRow[GeometryLeg]] = FaultRow(
    leg=GeometryLeg.QUALITY, point="subdivide", arm="boundary", defect="densify-refused", retriability=TERMINAL
)
QUALITY_SMOOTH: Final[FaultRow[GeometryLeg]] = FaultRow(
    leg=GeometryLeg.QUALITY, point="smooth", arm="boundary", defect="denoise-refused", retriability=TERMINAL
)
RAISES: Final[Block[FaultRow[GeometryLeg]]] = rostered(Block.of_seq([QUALITY_METRICS, QUALITY_SUBDIVIDE, QUALITY_SMOOTH]))
_ARM_RAISED: Final[Map[OpKind, FaultRow[GeometryLeg]]] = Map.of_seq([("subdivide", QUALITY_SUBDIVIDE), ("smooth", QUALITY_SMOOTH)])


# --- [MODELS] ---------------------------------------------------------------------------


class QualityMetrics(Struct, frozen=True):
    watertight: bool
    winding_consistent: bool
    area: float
    volume: float
    vertex_count: int
    face_count: int
    edge_count: int
    boundary_edges: int
    nonmanifold_edges: int
    components: int
    genus: int
    euler_characteristic: int
    aspect_ratio: Option[Quantiles]
    skewness: Option[Quantiles]
    angle_defect_mean: Option[float]
    angle_defect_std: Option[float]

    @property
    def worst(self) -> Option[tuple[float, float]]:
        return self.aspect_ratio.bind(lambda cells: self.skewness.map(lambda skew: (cells[4], skew[4])))

    def frame(self, subject: GeometrySubject, key: ContentKey) -> "RuntimeResult[EvidenceFrame]":
        table: dict[str, list[object]] = {
            "watertight": [self.watertight],
            "winding_consistent": [self.winding_consistent],
            "area": [self.area],
            "volume": [self.volume],
            "vertices": [self.vertex_count],
            "faces": [self.face_count],
            "edges": [self.edge_count],
            "boundary_edges": [self.boundary_edges],
            "nonmanifold_edges": [self.nonmanifold_edges],
            "components": [self.components],
            "genus": [self.genus],
            "euler_characteristic": [self.euler_characteristic],
        }
        moments = {
            name: [held]
            for name, slot in (("angle_defect_mean", self.angle_defect_mean), ("angle_defect_std", self.angle_defect_std))
            for held in slot.to_list()
        }
        spread = {
            f"{prefix}_{label}": [value]
            for prefix, summary in (("aspect", self.aspect_ratio), ("skew", self.skewness))
            for held in summary.to_list()
            for label, value in zip(_QUANTILE_LABEL, held, strict=True)
        }
        return EvidenceFrame.of(subject, key, table | moments | spread)


class ExactTopology(Struct, frozen=True, gc=False):
    genus: int
    vertex_count: int
    edge_count: int
    face_count: int
    components: int
    volume: float
    area: float


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


# --- [ERRORS] ---------------------------------------------------------------------------


@tagged_union(frozen=True)
class QualityFault(Exception):
    tag: Literal["rejected"] = tag()
    rejected: str = case()

    def __str__(self) -> str:
        return f"{self.tag}:{self._coordinate()}"

    def _coordinate(self) -> str:
        match self:
            case QualityFault(tag="rejected", rejected=status):
                return status
            case _ as unreachable:
                assert_never(unreachable)


# --- [OPERATIONS] -----------------------------------------------------------------------


def _quantiles(values: np.ndarray) -> Option[Quantiles]:
    s = np.sort(values)
    if s.size == 0:
        return Nothing
    idx = np.clip(np.round(_QUANTILE_FRACTIONS * (s.size - 1)).astype(np.intp), 0, s.size - 1)
    return Some(tuple(s[idx].tolist()))


def _moments(values: np.ndarray) -> tuple[Option[float], Option[float]]:
    return (Some(float(np.mean(values))), Some(float(np.std(values)))) if values.size else (Nothing, Nothing)


def _cell_shape(mesh: trimesh.Trimesh) -> tuple[np.ndarray, np.ndarray]:
    tris = np.asarray(mesh.vertices, dtype=np.float64)[np.asarray(mesh.faces)]
    e0, e1, e2 = tris[:, 1] - tris[:, 0], tris[:, 2] - tris[:, 1], tris[:, 0] - tris[:, 2]
    lengths = np.stack([np.linalg.norm(e0, axis=1), np.linalg.norm(e1, axis=1), np.linalg.norm(e2, axis=1)], axis=1)
    longest, shortest = lengths.max(axis=1), np.clip(lengths.min(axis=1), 1e-12, None)
    aspect = longest / shortest
    cos_a = np.clip(-np.sum(e2 * e0, axis=1) / (lengths[:, 2] * lengths[:, 0]), -1.0, 1.0)
    cos_b = np.clip(-np.sum(e0 * e1, axis=1) / (lengths[:, 0] * lengths[:, 1]), -1.0, 1.0)
    cos_c = np.clip(-np.sum(e1 * e2, axis=1) / (lengths[:, 1] * lengths[:, 2]), -1.0, 1.0)
    cosines = np.stack([cos_a, cos_b, cos_c], axis=1)
    skew = np.clip((cosines.min(axis=1) - 0.5) / -0.5, 0.0, 1.0)
    return aspect, skew


def _topology_kernel(mesh: trimesh.Trimesh) -> ExactTopology:
    solid = to_manifold(mesh)
    if solid.status() != manifold3d.Error.NoError:
        raise QualityFault(rejected=solid.status().name)
    parts = solid.decompose()
    genus = sum(int(c.genus()) for c in parts)
    return ExactTopology(
        genus, solid.num_vert(), solid.num_edge(), solid.num_tri(), len(parts), float(solid.volume()), float(solid.surface_area())
    )


def _decimate_kernel(mesh: trimesh.Trimesh, target_faces: int) -> trimesh.Trimesh:
    return mesh.simplify_quadric_decimation(face_count=target_faces)


def closure_fold(mesh: trimesh.Trimesh, exact: Option[ExactTopology] = Nothing) -> QualityMetrics:
    faces = np.asarray(mesh.faces)
    half_edges = np.sort(
        np.concatenate([faces[:, [0, 1]], faces[:, [1, 2]], faces[:, [2, 0]]]), axis=1
    )
    unique_edges, counts = np.unique(half_edges, axis=0, return_counts=True)
    boundary_edges = int(np.sum(counts == 1))
    nonmanifold_edges = int(np.sum(counts >= 3))
    spine = (len(mesh.vertices), len(unique_edges), len(mesh.faces))
    v, e, f, genus, area, volume, components = exact.map(
        lambda held: (held.vertex_count, held.edge_count, held.face_count, held.genus, held.area, held.volume, held.components)
    ).default_with(
        lambda: (*spine, max(0, (2 - (spine[0] - spine[1] + spine[2])) // 2), float(mesh.area), float(mesh.volume), int(mesh.body_count))
    )
    aspect, skew = _cell_shape(mesh)
    defect_mean, defect_std = _moments(np.asarray(mesh.vertex_defects))
    return QualityMetrics(
        bool(mesh.is_watertight), bool(mesh.is_winding_consistent),
        area, volume, v, f, e,
        boundary_edges, nonmanifold_edges, components, genus, v - e + f,
        _quantiles(aspect), _quantiles(skew), defect_mean, defect_std,
    )


# --- [SERVICES] -------------------------------------------------------------------------


class MeshQuality:
    def __init__(self, mesh: trimesh.Trimesh, lane: LanePolicy, *, composition: ScopeKey = DEFAULT_SCOPE) -> None:
        self._mesh = mesh
        self._lane = lane
        self._composition = composition
        self._exact: ExactTopology | None = None

    @overload
    async def apply(self, op: MeshQualityOp) -> "RuntimeResult[MeshQualityResult]": ...
    @overload
    async def apply(self, op: Sequence[MeshQualityOp]) -> "RuntimeResult[Block[MeshQualityResult]]": ...
    async def apply(self, op: MeshQualityOp | Sequence[MeshQualityOp]) -> "RuntimeResult[MeshQualityResult] | RuntimeResult[Block[MeshQualityResult]]":
        match op:
            case MeshQualityOp() as one:
                return await self._route(one)
            case batch:
                results = Block.of_seq([await self._route(one) for one in batch])
                return traversed(results, by=Disposition.ABORT)

    async def _route(self, op: MeshQualityOp) -> "RuntimeResult[MeshQualityResult]":
        match op:
            case MeshQualityOp(tag="decimate", decimate=target_faces):
                before = len(self._mesh.faces)
                offloaded = await self._lane.offload(Kernel.of(_decimate_kernel, KernelTrait.HOSTILE), self._mesh, target_faces)
                return offloaded.map(lambda out: self._conditioned(op, MeshQualityResult.Decimate(self._adopt(out)), before))
            case MeshQualityOp(tag="metrics"):
                exact = await self._exact_topology() if bool(self._mesh.is_watertight) else Ok(Nothing)
                return exact.bind(lambda held: boundary(QUALITY_METRICS, lambda: self._metrics(held), catch=_TRIMESH_RAISES))
            case MeshQualityOp(tag="subdivide" | "smooth") as arm:
                return boundary(_ARM_RAISED[arm.tag], lambda: self._spine(arm), catch=_TRIMESH_RAISES)
            case _ as unreachable:
                assert_never(unreachable)

    async def _exact_topology(self) -> "RuntimeResult[Option[ExactTopology]]":
        if self._exact is not None:
            return Ok(Some(self._exact))
        held = await self._lane.offload(Kernel.of(_topology_kernel, KernelTrait.HOSTILE), self._mesh)
        return held.map(self._cache_exact)

    def _cache_exact(self, exact: ExactTopology) -> Option[ExactTopology]:
        self._exact = exact
        return Some(exact)

    def _adopt(self, mesh: trimesh.Trimesh) -> trimesh.Trimesh:
        self._mesh = mesh
        self._exact = None
        return mesh

    def _conditioned(self, op: MeshQualityOp, result: MeshQualityResult, before: int) -> MeshQualityResult:
        logger(self._composition).info(
            "quality.conditioned", op=op.tag, faces_before=before, faces_after=len(self._mesh.faces), watertight=bool(self._mesh.is_watertight)
        )
        return result

    def _spine(self, op: MeshQualityOp) -> MeshQualityResult:
        before = len(self._mesh.faces)
        match op:
            case MeshQualityOp(tag="subdivide", subdivide=(max_edge, iterations)):
                verts, faces = self._mesh.vertices, self._mesh.faces
                for _ in range(iterations):
                    verts, faces = trimesh.remesh.subdivide_to_size(verts, faces, max_edge)
                self._adopt(trimesh.Trimesh(vertices=verts, faces=faces, process=True))
                return self._conditioned(op, MeshQualityResult.Subdivide(self._mesh), before)
            case MeshQualityOp(tag="smooth", smooth=(kind, iterations, factor)):
                self._smooth(kind, iterations, factor)
                self._adopt(self._mesh)
                return self._conditioned(op, MeshQualityResult.Smooth(self._mesh), before)
            case _ as unreachable:
                raise AssertionError(unreachable.tag)

    def _smooth(self, kind: SmoothKind, iterations: int, factor: float) -> None:
        match kind:
            case SmoothKind.TAUBIN:
                trimesh.smoothing.filter_taubin(self._mesh, lamb=factor, nu=factor / (1.0 - _TAUBIN_K * factor), iterations=iterations)
            case SmoothKind.LAPLACIAN:
                trimesh.smoothing.filter_laplacian(self._mesh, lamb=factor, iterations=iterations)
            case SmoothKind.HUMPHREY:
                trimesh.smoothing.filter_humphrey(self._mesh, alpha=factor, iterations=iterations)
            case unreachable:
                assert_never(unreachable)

    def _metrics(self, exact: Option[ExactTopology]) -> MeshQualityResult:
        metrics = closure_fold(self._mesh, exact)
        charter_record(
            GeometrySubject.MESH_ALGEBRA,
            {"genus": metrics.genus} | metrics.worst.map(lambda tail: {"worst_aspect_ratio": tail[0]}).default_value({}),
            composition=self._composition,
        )
        return MeshQualityResult.Metrics(metrics)

```

## [03]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)

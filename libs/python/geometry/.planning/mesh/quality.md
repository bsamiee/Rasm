# [PY_GEOMETRY_MESH_QUALITY]

Mesh-topology conditioning and metrology over an in-memory triangulation: `MeshQualityOp` discriminates decimate/subdivide/smooth/metrics on one polymorphic entrypoint, `MeshQualityResult` mirrors the op case, and `QualityMetrics` is the one shape/validity/topology grade — never a metric-per-method family. Tessellation, scan-reconstruction, and STEP hops compose this primitive to coarsen, refine, denoise, and grade a surface before it crosses a downstream rail; the surface arrives and returns as in-memory `trimesh.Trimesh` across the `mesh ← data/spatial` seam, and this owner never opens or writes a mesh file.

`closure_fold` is this owner's PUBLIC exact-closure truth — the one watertight/euler/volume/area/components fold `scan/reconstruction` and `scan/deviation` compose downward, never a per-consumer re-computation — and `ArmOutcome`/`armed` its PUBLIC receipt cross-cut, the one payload-beside-evidence carrier `mesh/spatial` imports rather than re-spelling. `manifold3d` exact-topology enrichment builds through repair's public `to_manifold` (repair is the chartered `manifold3d` owner — no `Mesh`/`Mesh64` selection and no capability probe re-spelled here); the CPU-bound kernels ride `LanePolicy.offload` on the `HOSTILE` trait, because the `trimesh`/`manifold3d` band imports under no isolated subinterpreter and the warm process pool is the one substrate that composes. This owner mints no `GeometrySubject`: it is the read-and-condition primitive, and the graduating subject belongs to the repair and reconstruction owners that emit the conditioned solid.

## [01]-[INDEX]

- [02]-[QUALITY]: decimate, subdivide, smooth, and metrics on one tagged union over the `trimesh`/`numpy` spine with the `manifold3d` exact-topology enrichment, returning one `MeshQualityResult` union over the shared `ArmOutcome` cross-cut.

## [02]-[QUALITY]

- Owner: `MeshQuality` — the boundary capsule over the four arms; `SmoothKind` makes the smoothing filter family one row on the `Smooth` case, never three parallel entrypoints; `ArmOutcome[R, E]` pairs one arm's payload with the leaf evidence its body already built and `armed` is the ONE cross-cut that lands the evidence and returns the payload, so a new case writes only the geometry body producing the pair and the `mesh/spatial#SPATIAL` capsule composes the same two symbols instead of forking a second `Outcome` struct under the same name.
- Cases: `Decimate` coarsens mesh topology for a downstream geometry op — render-time decimation for display is the artifacts figures owner's, and an LOD/display-budget arm here trespasses that boundary; `Subdivide` densifies before a curvature-sensitive metric pass; `Smooth` denoises before a deviation pass; `Metrics` is the gate the daemon and the clash/deviation hops read before trusting a surface.
- Law: an arm that took no measurement records absence, never a zero — `worst_aspect_ratio`, `worst_skewness`, `genus`, and the exact-tier flag are `None` on every conditioning arm and drop out of the fact projection there, so a support bundle reads a decimate row as three unmeasured slots rather than as a perfect-aspect degenerate-genus grade; the harvest itself yields nothing before the first arm runs, since an empty contributor stream IS the diagnosis a synthetic zero-filled receipt erases.
- Law: `_metrics_outcome` records the mesh genus/aspect charter rows through the graduation `charter_record` derivation at the producing fold — parent-side, `boundary`-fenced, never in a worker kernel, spellings derived, never hand-picked, and stamped with the capsule's own composition key — and `QualityMetrics.frame` projects the whole grade as one `EvidenceFrame` row whose `GeometrySubject` and content key the graduating owner supplies, so one metrics pass feeds the dashboard histograms and the data plane's columnar tier from one fold. The frame carries BOTH cell distributions in full beside the topology census, because the receipt already owns the tail verdict a gate thresholds on and a frame publishing only that extremum lets no consumer separate a mesh with one degenerate cell from a mesh degenerate throughout — the distribution is the row-grain evidence the columnar tier exists to hold, and the receipt keeps the census.
- Auto: the exact-topology fold is enrichment over the always-available Euler-characteristic spine, gated on watertightness alone — `manifold3d` carries no interpreter marker, so a tier branch over a probe that cannot fail is a dead arm, and the honest conditional path is the watertight precondition the exact kernel needs. One offloaded build yields the exact genus (summed over `decompose()` components), the exact counts, and the kernel mass superseding the `trimesh` measure in a single fold.
- Packages: `trimesh` (the conditioning filters, cached validity/mass axes, `vertex_defects`), `numpy` (the half-edge incidence fold and per-cell shape statistics), `manifold3d` (the exact tier, reached through repair's `to_manifold` over one module-scope `lazy import` for the `Error` status vocabulary alone), `expression`, `msgspec`, geometry graduation (`EvidenceFrame`/`charter_record`, the charter measure authority), and the runtime rails per the fence imports.
- Growth: a new conditioning op is one `MeshQualityOp` case and its mirrored `MeshQualityResult` arm and one `ArmOutcome`-producing body; a new smoothing filter is one `SmoothKind` row; a new exact-geometry provider is one `ManifoldTier` row at `mesh/repair#MESH`, never a probe minted here.
- Boundary: watertight repair, hole-fill, boolean CSG, and the `ManifoldTier` capability probe are `mesh/repair`'s; proximity, ray, contains, and sampling queries are `mesh/spatial`'s; registration and reconstruction are `scan/registration`+`scan/reconstruction`'s; mesh-file decode/encode is the data `MeshPayload` owner's (`rasm.data.spatial.mesh`).

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
from collections.abc import Callable, Iterable, Sequence
from enum import StrEnum
from typing import Final, Literal, assert_never, overload

import numpy as np
import trimesh
from expression import Ok, case, tag, tagged_union
from expression.collections import Block
from msgspec import Struct

from rasm.geometry.graduation import EvidenceFrame, GeometrySubject, charter_record
from rasm.geometry.mesh.repair import to_manifold
from rasm.runtime.faults import Disposition, RuntimeRail, boundary, traversed
from rasm.runtime.identity import ContentKey
from rasm.runtime.lanes import LanePolicy
from rasm.runtime.receipts import DEFAULT_SCOPE, Phase, Receipt, ScopeKey
from rasm.runtime.workers import Kernel, KernelTrait

# the `Error` status vocabulary the exact kernel gates on; repair owns the build, this page owns only the gate, and
# the module-scope lazy proxy keeps the extension cold until the offloaded topology kernel runs.
lazy import manifold3d

# --- [TYPES] ----------------------------------------------------------------------------

type OpKind = Literal["decimate", "subdivide", "smooth", "metrics"]


class SmoothKind(StrEnum):
    TAUBIN = "taubin"        # shrink-free band-pass Laplacian
    LAPLACIAN = "laplacian"  # uniform-weight Laplacian
    HUMPHREY = "humphrey"    # Humphrey classes anti-shrink


# --- [CONSTANTS] ------------------------------------------------------------------------

_QUANTILE_FRACTIONS: Final[np.ndarray] = np.array([0.0, 0.25, 0.5, 0.75, 1.0])  # five-number summary positions
# the column suffix per summary position, zipped `strict=True` against the tuple the fractions produce, so a sixth
# fraction breaks the frame projection at its own zip rather than silently publishing a distribution one column short.
_QUANTILE_LABEL: Final[tuple[str, ...]] = ("min", "q1", "median", "q3", "max")
_TAUBIN_K: Final[float] = 0.05  # the Taubin pass-band target `1/lamb - 1/nu`; nu = lamb/(1 - K*lamb) holds it constant across every lamb


# --- [MODELS] ---------------------------------------------------------------------------


class ArmOutcome[R, E](Struct, frozen=True):
    # the mesh capsules' shared cross-cut carrier: one arm's payload beside the leaf evidence its own body built,
    # so the receipt is constructed once at the site that measured it rather than re-derived by a second fold. Two
    # parameters, because the payload union and the leaf receipt are distinct owners per capsule; GC-tracked, since
    # a spatial payload nests numpy arrays.
    result: R
    evidence: E


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
    components: int  # exact decompose() count when the exact fold ran, trimesh body_count the fall-through
    genus: int
    euler_characteristic: int
    aspect_ratio: tuple[float, float, float, float, float]   # five-number (min, q1, median, q3, max)
    skewness: tuple[float, float, float, float, float]        # equiangular five-number summary
    angle_defect_mean: float  # mean per-vertex angle defect, a curvature signal, never vertex valence
    angle_defect_std: float

    @property
    def worst(self) -> tuple[float, float]:  # the tail verdict off the held distributions, never a re-run of the cell fold
        return (self.aspect_ratio[4], self.skewness[4])

    def frame(self, subject: GeometrySubject, key: ContentKey) -> "RuntimeRail[EvidenceFrame]":
        # one columnar row through the graduation frame port; the graduating owners (repair, reconstruction)
        # supply their subject and the key their own `spec` projection minted — quality mints neither — so one grade
        # serves both evidence classes, and the port's rail returns a width mismatch to this producer rather than
        # raising past its consumer. The frame carries the WHOLE grade the fold computed: the receipt keeps the tail
        # verdict a gate thresholds on, and the columnar tier gets both distributions in full, because a frame
        # publishing only `max` lets no consumer tell a mesh with one degenerate cell from a mesh that is degenerate
        # throughout — the distribution IS the row-grain evidence, and flattening it to its extremum here would
        # leave the census on the receipt and nothing behind it. A quantile column derives from the same fraction
        # roster the fold sampled, so a sixth position is one label and one fraction with no projection edit.
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
            "angle_defect_mean": [self.angle_defect_mean],
            "angle_defect_std": [self.angle_defect_std],
            **{f"aspect_{label}": [value] for label, value in zip(_QUANTILE_LABEL, self.aspect_ratio, strict=True)},
            **{f"skew_{label}": [value] for label, value in zip(_QUANTILE_LABEL, self.skewness, strict=True)},
        }
        return EvidenceFrame.of(subject, key, table)


class ExactTopology(Struct, frozen=True, gc=False):  # the exact fold's one product; never a lone genus() read
    genus: int
    vertex_count: int
    edge_count: int
    face_count: int
    components: int  # decompose() body count
    volume: float
    area: float


class MeshQualityReceipt(Struct, frozen=True, gc=False):
    # every shape/topology slot is optional because a conditioning arm measures none of them: `None` spells the
    # absence the fact projection then omits, where a `0.0` publishes a perfect aspect ratio no fold computed.
    op: OpKind
    faces_before: int
    faces_after: int
    watertight: bool
    exact: bool | None = None  # True = the manifold3d fold supplied the counts, False = the Euler spine, None = no grade taken
    worst_aspect_ratio: float | None = None
    worst_skewness: float | None = None
    genus: int | None = None

    def fact(self) -> tuple[Phase, OpKind, dict[str, object]]:
        # a non-watertight grade is a flagged caveat, never an asserted closed-solid claim; the subject is the op
        # tag, never duplicated into a facts slot, and an unmeasured slot leaves the map rather than reading zero.
        phase: Phase = "emitted" if self.watertight else "admitted"
        measured = {"exact": self.exact, "worst_aspect_ratio": self.worst_aspect_ratio, "worst_skewness": self.worst_skewness, "genus": self.genus}
        facts: dict[str, object] = {"faces_before": self.faces_before, "faces_after": self.faces_after, "watertight": self.watertight} | {
            name: value for name, value in measured.items() if value is not None
        }
        return phase, self.op, facts


type QualityArm = ArmOutcome[MeshQualityResult, MeshQualityReceipt]


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
class QualityFault(Exception):  # raised INTO the lane's async_boundary, never a domain raise ValueError
    tag: Literal["rejected"] = tag()
    rejected: str = case()  # the manifold3d Error status name


# --- [OPERATIONS] -----------------------------------------------------------------------


def armed[R, E](hold: Callable[[E], object], out: ArmOutcome[R, E], /) -> R:
    # the ONE receipt cross-cut both mesh capsules compose: the arm's own evidence lands through the capsule's
    # holder — a single slot here, a drained block at `mesh/spatial` — and the payload returns untouched, so a new
    # arm writes only the body producing the pair and neither capsule re-spells the fold.
    hold(out.evidence)
    return out.result


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


# exact-tier offloaded kernel: composes repair's public `to_manifold` build, gates `status()` — `Manifold(mesh)` sets a
# non-`NoError` status rather than raising on a non-2-manifold soup — and returns the picklable ExactTopology VALUE, because a live
# `Manifold` is a pybind11 handle no pickler carries; the capsule caches the value across metric passes, never a live `_solid`.
def _topology_kernel(mesh: trimesh.Trimesh) -> ExactTopology:
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


# --- [SERVICES] -------------------------------------------------------------------------


class MeshQuality:  # structural ReceiptContributor conformance — the base adds nothing, so no subclass
    def __init__(self, mesh: trimesh.Trimesh, lane: LanePolicy, *, composition: ScopeKey = DEFAULT_SCOPE) -> None:
        self._mesh = mesh
        self._lane = lane  # the offload seam; the lane never imports the kernel
        self._composition = composition  # the custody key every charter record this capsule takes stamps
        self._last: MeshQualityReceipt | None = None
        self._exact: ExactTopology | None = None  # the cached VALUE; a live Manifold handle never crosses the pickle seam

    @overload
    async def apply(self, op: MeshQualityOp) -> "RuntimeRail[MeshQualityResult]": ...
    @overload
    async def apply(self, op: Sequence[MeshQualityOp]) -> "RuntimeRail[Block[MeshQualityResult]]": ...
    async def apply(self, op: MeshQualityOp | Sequence[MeshQualityOp]) -> "RuntimeRail[MeshQualityResult] | RuntimeRail[Block[MeshQualityResult]]":
        # a batch awaits each route IN ORDER — every conditioning arm lands its output through `_adopt`, so a
        # downstream Metrics reads the advanced mesh and never a stale capsule; the routes never fan out concurrently,
        # which is also why one terminal receipt is the honest hold here where `mesh/spatial`'s independent reads block.
        match op:
            case MeshQualityOp() as one:
                return await self._route(one)
            case batch:
                rails = Block.of_seq([await self._route(one) for one in batch])
                return traversed(rails, by=Disposition.ABORT)  # abort on the first faulted op; the runtime owns the strategy row

    async def _route(self, op: MeshQualityOp) -> "RuntimeRail[MeshQualityResult]":
        # the exact-topology kernel offloads with decimate; subdivide/smooth/spine-metrics run under `boundary`.
        match op:
            case MeshQualityOp(tag="decimate", decimate=target_faces):
                before = len(self._mesh.faces)
                # HOSTILE names the warm process pool — a bare callable lifts PURE onto a subinterpreter the trimesh band never imports under.
                offloaded = await self._lane.offload(Kernel.of(_decimate_kernel, KernelTrait.HOSTILE), self._mesh, target_faces)
                return offloaded.map(lambda out: armed(self._held, self._conditioned(op, MeshQualityResult.Decimate(self._adopt(out)), before)))
            case MeshQualityOp(tag="metrics"):
                # the exact fold is gated on watertightness alone — `manifold3d` resolves on every floor, so a
                # capability branch here is a dead arm — and the numpy half-edge/cell-shape fold ALWAYS runs under
                # `boundary` so a degenerate-mesh numpy raise converts to a BoundaryFault rather than escaping the rail.
                exact = await self._exact_topology() if bool(self._mesh.is_watertight) else Ok(None)
                return exact.bind(lambda e: boundary("mesh.quality.metrics", lambda: armed(self._held, self._metrics_outcome(e))))
            case _:
                return boundary(f"mesh.quality.{op.tag}", lambda: armed(self._held, self._spine(op)))

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

    def _held(self, receipt: MeshQualityReceipt) -> None:  # the capsule's single-slot hold `armed` writes through
        self._last = receipt

    def _conditioned(self, op: MeshQualityOp, result: MeshQualityResult, before: int) -> QualityArm:
        # one conditioning-arm projection: the shape and topology slots stay `None` because no fold measured them.
        out = self._mesh
        return ArmOutcome(result, MeshQualityReceipt(op.tag, before, len(out.faces), bool(out.is_watertight)))

    def contribute(self) -> Iterable[Receipt]:
        # nothing ran, nothing is claimed: an empty contributor stream IS the diagnosis, where a synthetic
        # zero-filled receipt publishes a watertight metrics grade against a mesh no arm ever read.
        if self._last is None:
            return
        yield Receipt.of("rasm.geometry.mesh.quality", self._last.fact())

    def _spine(self, op: MeshQualityOp) -> QualityArm:  # the in-place conditioning arms only
        before = len(self._mesh.faces)
        match op:
            case MeshQualityOp(tag="subdivide", subdivide=(max_edge, iterations)):
                verts, faces = self._mesh.vertices, self._mesh.faces
                for _ in range(iterations):  # Exemption: the remesh step reads its own predecessor, the provider's iteration seam.
                    verts, faces = trimesh.remesh.subdivide_to_size(verts, faces, max_edge)
                # merge edge-split vertices so is_watertight reads true; `_adopt` advances the capsule and drops the exact cache
                self._adopt(trimesh.Trimesh(vertices=verts, faces=faces, process=True))
                return self._conditioned(op, MeshQualityResult.Subdivide(self._mesh), before)
            case MeshQualityOp(tag="smooth", smooth=(kind, iterations, factor)):
                self._smooth(kind, iterations, factor)
                self._adopt(self._mesh)  # in-place filter moved vertices, so the cached exact volume/area evidence is stale
                return self._conditioned(op, MeshQualityResult.Smooth(self._mesh), before)
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

    def _metrics_outcome(self, exact: ExactTopology | None) -> QualityArm:
        metrics = closure_fold(self._mesh, exact)
        worst = metrics.worst
        # parent-side charter projection at the producing fold: the genus/aspect spellings AND the recorded discriminant
        # both derive from the subject — MESH_ALGEBRA and RECONSTRUCTED_MESH share the rows under the one-member law, so
        # either key derives the same spellings and this engine page mints no subject and spells no kind of its own.
        charter_record(GeometrySubject.MESH_ALGEBRA, {"genus": metrics.genus, "worst_aspect_ratio": worst[0]}, composition=self._composition)
        before = len(self._mesh.faces)
        return ArmOutcome(
            MeshQualityResult.Metrics(metrics),
            MeshQualityReceipt("metrics", before, before, metrics.watertight, exact is not None, worst[0], worst[1], metrics.genus),
        )
```

## [03]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)

# [PY_COMPUTE_SPATIAL]

One array-native computational-geometry owner rules: `SpatialQuery` discriminates Qhull tessellation, KD-tree proximity, the pairwise/condensed distance matrix, the rotation-and-alignment algebra, and the alpha-shape boundary fold over a point set, and `resolve` folds every case to a `SpatialEvidence` outcome the `SpatialReceipt` carries whole. This owner emits point-set evidence as compute-native receipts and never re-owns the geometry-branch `trimesh` mesh surface; the graduation direction is closed one-way — geometry's reconstruction plane mints `reconstructed-mesh`, the alpha-shape `Boundary` stays a compute-native receipt product, and an outward crossing requires a named consumer and a compute-owned axis case, never the geometry case.

Each point set admits through `numerics/array#PAYLOAD` for the finite gate and the operand `ContentKey`; the receipt keys the RESULT through the query-owned `identity_parts` fold handed to `IdentitySource(parts=...)`, so the count-and-length framing runs at the identity owner and two different queries over one point set never share a key; the resolved receipt is the `ReceiptContributor` the weave harvest and the study spine consume. `scipy.spatial` is not Array-API-aware, so the point set is the numpy `np.ndarray` the Qhull/KD-tree/BLAS backends bind under the `RELEASING` trait, isolation and band deriving at the runtime `Kernel` crossing. The direct native kernel enters through `LanePolicy.whole`, and the resulting `LaneGrant.width` is the KD-tree scan width threaded through the kernel — never an allocator-total read or the unbounded `workers=-1` team that multiplies inner and outer parallelism.

## [01]-[INDEX]

- [02]-[SPATIAL]: the `SpatialQuery` cases over one point set, evidence discriminated over `SpatialEvidence`, the two KD-tree routes degrading through the data-driven `NEIGHBOUR_FLOOR`.

## [02]-[SPATIAL]

- Owner: `SpatialQuery` — one owner discriminated by the geometric question, never a `Neighbours`/`Hull`/`Triangulate` method family. `Align` is a paired correspondence fit — `source` and `target` carry the same row count, `procrustes` raising `ValueError` on a mismatch, the fault converting on the `boundary` fence. `AlphaShape` folds its boundary locally because no `scipy.spatial` alpha-shape primitive exists; `_circumradius` stays private to that kernel, never a module-level sibling of the dispatch.
- Output: `SpatialEvidence` parameterizes the result per case, and the `Complex` `cardinality` is the primitive count its `kind` string discriminates — hull facets, Delaunay simplices, Voronoi ridges, halfspace vertices, distance pairs — so a distance summary never wears a `simplices` label and four outcome vocabularies never smuggle through two overloaded columns. Adding a query writes only its geometry body returning a `SpatialEvidence`; `assert_never` closes the dispatch.
- Faults: one `SPATIAL_RESOLVE` fence row spans every query — the tag is a span fact, never nine subject spellings — and its `catch` admits `QhullError` through its `RuntimeError` base, since naming the attribute would reify the lazy proxy and defeat the floor. `_proximity` scopes its `try` to the import dereference ALONE, so a raise out of a live KD-tree query is the defect the fence converts rather than a silent demotion to floor evidence.
- Packages: `scipy.spatial`, `numpy`, `expression`, and `msgspec` per the fence imports; `scipy.spatial` and `optimize.linprog` bind once each as module-scope `lazy` names that defer both trees to the first kernel body, so `resolve` stays a pure tag-dispatch and `linprog` costs nothing until the halfspace Chebyshev-centre interior point asks for it.
- Growth: a new spatial query is one `SpatialQuery` case with one `resolve` arm and one `identity_parts` arm; a new evidence shape is one `SpatialEvidence` case with its `facts()` arm — the receipt carries the evidence whole and needs no edit; a new distance metric is one `Metric` row; a new tessellation backend is one `Tessellation` row; a new degrading route is one `NEIGHBOUR_FLOOR` row.

```python signature
from collections.abc import Callable, Iterable
from enum import StrEnum
from itertools import combinations
from typing import TYPE_CHECKING, Final, Literal, assert_never

import numpy as np
from expression import Nothing, Option, Some, case, tag, tagged_union
from expression.collections import Block, Map
from msgspec import Struct

from rasm.compute.graduation.handoff import ComputeLeg, EvidenceScope, evidence_run
from rasm.compute.numerics.array import ArrayPayload, ArraySource, FiniteGate
from rasm.runtime.identity import ContentIdentity, ContentKey, IdentitySource
from rasm.runtime.lanes import LanePolicy
from rasm.runtime.faults import TERMINAL, FaultRow, RuntimeRail, boundary, rostered
from rasm.runtime.receipts import DEFAULT_SCOPE, Provenance, Receipt, ScopeKey
from rasm.runtime.workers import Kernel, KernelTrait

# cold scientific dependencies: the `lazy` binds defer both scipy trees to the first route body. `_proximity` keeps its
# `try`/`except ImportError` around the `sp.cKDTree` dereference ALONE, so an absent scipy falls to `NEIGHBOUR_FLOOR`
# while a raise out of the query it guards stays the defect it is.
lazy import scipy.spatial as sp
lazy from scipy.optimize import linprog

if TYPE_CHECKING:
    # declared here so the `KdReduction` signature names the real carrier rather than degrading to a bare `object`.
    from scipy.spatial import cKDTree

# --- [TYPES] ----------------------------------------------------------------------------

type Tag = Literal["neighbours", "radius", "pairs", "distances", "hull", "triangulate", "tessellate", "alpha_shape", "align"]
type NeighbourReduction = Callable[[np.ndarray, np.ndarray, float], "SpatialEvidence"]
type KdReduction = Callable[["cKDTree"], "SpatialEvidence"]


class Metric(StrEnum):  # the scipy.spatial.distance metric= argument as a bounded vocabulary, never a string knob
    EUCLIDEAN = "euclidean"  # Minkowski p=2
    CITYBLOCK = "cityblock"  # Minkowski p=1
    CHEBYSHEV = "chebyshev"  # Minkowski p=inf
    COSINE = "cosine"
    CORRELATION = "correlation"


class Tessellation(StrEnum):  # the Voronoi backend selector; the value is the scipy carrier name lowercased
    VORONOI = "voronoi"
    SPHERICAL = "spherical"
    HALFSPACE = "halfspace"


# --- [MODELS] ---------------------------------------------------------------------------


@tagged_union(frozen=True)
class SpatialEvidence:
    tag: Literal["proximity", "complex_", "boundary", "alignment"] = tag()
    # a kNN read has no radius and a radius read has no mean: both are ABSENT on the arm that never measured them,
    # and the retired `0.0` defaults published a coincident-point mean and a zero-radius ball as measured values.
    proximity: tuple[int, Option[float], Option[float]] = case()  # (count, mean_distance, radius)
    complex_: tuple[str, int, float] = case()
    boundary: tuple[int, float] = case()
    alignment: tuple[float, float] = case()

    @staticmethod
    def Proximity(count: int, mean_distance: Option[float] = Nothing, radius: Option[float] = Nothing) -> "SpatialEvidence":
        return SpatialEvidence(proximity=(count, mean_distance, radius))

    @staticmethod
    def Complex(kind: str, cardinality: int, measure: float) -> "SpatialEvidence":
        return SpatialEvidence(complex_=(kind, cardinality, measure))

    @staticmethod
    def Boundary(facets: int, total_radius: float) -> "SpatialEvidence":
        return SpatialEvidence(boundary=(facets, total_radius))

    @staticmethod
    def Alignment(rmsd: float, disparity: float) -> "SpatialEvidence":
        return SpatialEvidence(alignment=(rmsd, disparity))

    def facts(self) -> dict[str, object]:
        # native scalars only — a `str()`/`f""` coerce erases comparability at the receipt layer; rendering is the export layer's.
        match self:
            case SpatialEvidence(tag="proximity", proximity=(count, mean_distance, radius)):
                # each unmeasured column OMITS its key rather than reporting a zero every aggregation folds as a reading.
                return {
                    "count": count,
                    **mean_distance.map(lambda mean: {"mean_distance": mean}).default_value({}),
                    **radius.map(lambda r: {"radius": r}).default_value({}),
                }
            case SpatialEvidence(tag="complex_", complex_=(kind, cardinality, measure)):
                return {"kind": kind, "cardinality": cardinality, "measure": measure}
            case SpatialEvidence(tag="boundary", boundary=(facets, total_radius)):
                return {"facets": facets, "total_radius": total_radius}
            case SpatialEvidence(tag="alignment", alignment=(rmsd, disparity)):
                return {"rmsd": rmsd, "disparity": disparity}
            case _ as unreachable:
                assert_never(unreachable)


class SpatialReceipt(Struct, frozen=True):
    # `lineage` carries the admitted point-set key beside the result key as ONE value, because the spine reads exactly
    # that pair: a receipt naming what it produced without naming what it consumed strands every downstream walk at
    # one hop, and two loose key slots are what lets one drift past the other.
    query: str
    points: int
    lineage: Provenance
    evidence: SpatialEvidence

    @staticmethod
    def of(tag: Tag, points: int, lineage: Provenance, evidence: SpatialEvidence) -> "SpatialReceipt":
        return SpatialReceipt(tag, points, lineage, evidence)

    @property
    def content_key(self) -> ContentKey:
        return self.lineage.produced

    def contribute(self) -> Iterable[Receipt]:
        # ONE settled-receipt spine: the result key IS the spine's `key` column and its `produced` provenance, so no
        # payload slot re-spells the hex render the spine carries, and the admitted point set is the `consumed`
        # roster. The evidence union stays on the payload — the spine owns its six columns and nothing else.
        facts = {"query": self.query, "points": self.points, **self.evidence.facts()}
        yield Receipt.of(
            EvidenceScope.SPATIAL.value,
            ("emitted", self.query, facts),
            key=Some(self.lineage.produced),
            provenance=Some(self.lineage),
        )


@tagged_union(frozen=True)
class SpatialQuery:
    tag: Tag = tag()
    neighbours: tuple[np.ndarray, np.ndarray, int] = case()
    radius: tuple[np.ndarray, np.ndarray, float] = case()
    pairs: tuple[np.ndarray, float] = case()
    distances: tuple[np.ndarray, np.ndarray | None, Metric] = case()
    hull: np.ndarray = case()
    triangulate: np.ndarray = case()
    tessellate: tuple[np.ndarray, Tessellation, float] = case()
    alpha_shape: tuple[np.ndarray, float] = case()
    align: tuple[np.ndarray, np.ndarray] = case()

    @staticmethod
    def Neighbours(points: np.ndarray, queries: np.ndarray, k: int = 1) -> "SpatialQuery":
        return SpatialQuery(neighbours=(points, queries, k))

    @staticmethod
    def Radius(points: np.ndarray, queries: np.ndarray, radius: float) -> "SpatialQuery":
        return SpatialQuery(radius=(points, queries, radius))

    @staticmethod
    def Pairs(points: np.ndarray, radius: float) -> "SpatialQuery":
        return SpatialQuery(pairs=(points, radius))

    @staticmethod
    def Distances(left: np.ndarray, right: np.ndarray | None = None, metric: Metric = Metric.EUCLIDEAN) -> "SpatialQuery":
        # `right=None` is the self-distance discriminant (condensed `pdist`); a real `right` is the
        # cross matrix (`cdist`) — never a mutable `np.empty` class-definition-time default.
        return SpatialQuery(distances=(left, right, metric))

    @staticmethod
    def Hull(points: np.ndarray) -> "SpatialQuery":
        return SpatialQuery(hull=points)

    @staticmethod
    def Triangulate(points: np.ndarray) -> "SpatialQuery":
        return SpatialQuery(triangulate=points)

    @staticmethod
    def Tessellate(points: np.ndarray, kind: Tessellation = Tessellation.VORONOI, radius: float = 1.0) -> "SpatialQuery":
        return SpatialQuery(tessellate=(points, kind, radius))

    @staticmethod
    def AlphaShape(points: np.ndarray, alpha: float) -> "SpatialQuery":
        return SpatialQuery(alpha_shape=(points, alpha))

    @staticmethod
    def Align(source: np.ndarray, target: np.ndarray) -> "SpatialQuery":
        return SpatialQuery(align=(source, target))

    @property
    def points(self) -> np.ndarray:
        # OPERAND buffer the ArrayPayload admits, recovered from every case so admission stays one path; the two-set routes stack
        # both operands so both coordinate buffers seed the operand key and a shared-`left` query with a distinct `right`/`target` never collides.
        match self:
            case SpatialQuery(tag="neighbours", neighbours=(pts, qs, _)) | SpatialQuery(tag="radius", radius=(pts, qs, _)):
                return np.concatenate([pts, qs])
            case SpatialQuery(tag="distances", distances=(left, right, _)) if right is not None:
                return np.concatenate([left, right])
            case SpatialQuery(tag="align", align=(source, target)):
                return np.concatenate([source, target])
            case (
                SpatialQuery(tag="pairs", pairs=(pts, _))
                | SpatialQuery(tag="distances", distances=(pts, *_))
                | SpatialQuery(tag="tessellate", tessellate=(pts, *_))
                | SpatialQuery(tag="alpha_shape", alpha_shape=(pts, _))
                | SpatialQuery(tag="hull", hull=pts)
                | SpatialQuery(tag="triangulate", triangulate=pts)
            ):
                return pts
            case _ as unreachable:
                assert_never(unreachable)

    def identity_parts(self, operand_key: ContentKey) -> tuple[bytes, ...]:
        # N SEMANTIC fields, handed to the identity owner AS fields: enum rows serialize by value and numeric rows as
        # canonical float64 bytes, and the count-and-length framing that makes the preimage injective rides
        # `IdentitySource(parts=...)` at its one owner. The retired form spelled a local `len(part).to_bytes(8, "big")`
        # prefix here — a width chosen at a call site forks the key namespace with no surface able to report it.
        row: tuple[object, ...]
        match self:
            case SpatialQuery(tag="neighbours", neighbours=(_, _, k)):
                row = (k,)
            case SpatialQuery(tag="radius", radius=(_, _, r)) | SpatialQuery(tag="pairs", pairs=(_, r)):
                row = (r,)
            case SpatialQuery(tag="distances", distances=(_, _, metric)):
                row = (metric.value,)
            case SpatialQuery(tag="tessellate", tessellate=(_, kind, r)):
                row = (kind.value, r)
            case SpatialQuery(tag="alpha_shape", alpha_shape=(_, alpha)):
                row = (alpha,)
            case SpatialQuery(tag="hull") | SpatialQuery(tag="triangulate") | SpatialQuery(tag="align"):
                row = ()
            case _ as unreachable:
                assert_never(unreachable)
        return (
            self.tag.encode(),
            operand_key.project("hex").encode(),
            *(cell.encode() if isinstance(cell, str) else np.float64(cell).tobytes() for cell in row),
        )

    @property
    def cardinality(self) -> int:
        # Receipt's reference-set count — the leading coordinate buffer's row count. `query.points.shape[0]` reports
        # `len(pts) + len(queries)` on the two-set routes, conflating the query set into the reference count; the two concerns
        # stay split across the identity operand and this receipt count.
        match self:
            case (
                SpatialQuery(tag="neighbours", neighbours=(pts, *_))
                | SpatialQuery(tag="radius", radius=(pts, *_))
                | SpatialQuery(tag="distances", distances=(pts, *_))
                | SpatialQuery(tag="align", align=(pts, _))
            ):
                return int(pts.shape[0])
            case _:
                return int(self.points.shape[0])

    def resolve(self, workers: int) -> SpatialEvidence:
        # `workers` is the whole-lane grant width both KD-tree scans bind — `-1` creates an unbounded nested team.
        match self:
            case SpatialQuery(tag="neighbours", neighbours=(pts, qs, k)):
                # admission precedes the clamp: a non-positive `k` and an empty reference set are caller defects the
                # `spatial.neighbours` boundary fence converts — clamped or floored past this point, each emits a nan Proximity
                # mean as valid evidence instead of a typed refusal, the raise-at-the-fence contract the align arm also rides.
                if k < 1 or pts.shape[0] == 0:
                    raise ValueError(f"neighbours requires k >= 1 and a non-empty reference set, got k={k}, points={int(pts.shape[0])}")
                # `k` clamps to the point count at the ONE dispatch site, so the cKDTree route and the numpy floor
                # aggregate identical slot counts — an unclamped `k > n` query pads inf distances that poison the
                # Proximity mean on one path while the floor's slice silently narrows on the other.
                kth = min(k, int(pts.shape[0]))
                return _proximity(
                    "neighbours", pts, qs, float(kth), lambda tree: _knn_distances(np.asarray(tree.query(qs, k=kth, workers=workers)[0], dtype=float))
                )  # a kNN read measures a mean and no radius, so the radius column stays ABSENT
            case SpatialQuery(tag="radius", radius=(pts, qs, r)):
                return _proximity(
                    "radius",
                    pts,
                    qs,
                    r,
                    # a radius read COUNTS hits and measures no mean, so the mean column stays ABSENT rather than zero.
                    lambda tree: SpatialEvidence.Proximity(int(sum(len(hit) for hit in tree.query_ball_point(qs, r=r, workers=workers))), radius=Some(r)),
                )
            case SpatialQuery(tag="pairs", pairs=(pts, r)):
                return _pairs(pts, r)
            case SpatialQuery(tag="distances", distances=(left, right, metric)):
                return _distances(left, right, metric)
            case SpatialQuery(tag="hull", hull=pts):
                return _hull(pts)
            case SpatialQuery(tag="triangulate", triangulate=pts):
                return _triangulate(pts)
            case SpatialQuery(tag="tessellate", tessellate=(pts, kind, radius)):
                return _tessellate(pts, kind, radius)
            case SpatialQuery(tag="alpha_shape", alpha_shape=(pts, alpha)):
                return _alpha_shape(pts, alpha)
            case SpatialQuery(tag="align", align=(source, target)):
                return _align(source, target)
            case _ as unreachable:
                assert_never(unreachable)


# --- [TABLES] ---------------------------------------------------------------------------

# this page's raise-side roster under the hub `ComputeLeg` seat. ONE row spans every query and declares NO slots,
# because nothing raises through it — it names a lift FENCE whose detail the classifier supplies. The retired
# `f"spatial.{query.tag}"` subject forked one refusal law into nine coordinates no shared census read could seat; the
# query discriminant rides the weave's own span facts, where a trace already filters on it.
SPATIAL_RESOLVE: Final[FaultRow[ComputeLeg]] = FaultRow(
    leg=ComputeLeg.SPATIAL, point="resolve", arm="boundary", defect="kernel-refused", retriability=TERMINAL
)
RAISES: Final[Block[FaultRow[ComputeLeg]]] = rostered(Block.of_seq([SPATIAL_RESOLVE]))

# Data-driven numpy proximity floor: the ImportError arm reads its row and folds the same Proximity evidence the cKDTree path
# produces, so the floor is table membership rather than per-route try/except blocks. A tag absent from this table has no floor —
# Qhull, the BLAS distance kernel, and the rotation SVD are the gated capability itself.
NEIGHBOUR_FLOOR: Final[Map[Tag, NeighbourReduction]] = Map.of_seq([
    ("neighbours", lambda pts, qs, k: _floor_knn(pts, qs, int(k))),
    ("radius", lambda pts, qs, r: _floor_radius(pts, qs, r)),
])


# --- [OPERATIONS] -----------------------------------------------------------------------


def _proximity(tag: Tag, pts: np.ndarray, qs: np.ndarray, scale: float, reduce: "KdReduction") -> SpatialEvidence:
    # one symmetric fold for both KD-tree proximity routes: run the scipy reduction, or fall to this tag's floor row when the
    # package is absent — both terminate in `Proximity`, so the body carries no per-tag ternary. The `try` scopes the
    # IMPORT SEAM ALONE — the lazy proxy reifies at this one dereference — because a whole-fold funnel re-routes an
    # `ImportError` raised anywhere inside a scipy KD-tree query onto the numpy floor and publishes floor evidence a
    # caller cannot tell from a tree scan; a raise out of the query itself is the defect the fence converts.
    try:
        tree = sp.cKDTree(pts)
    except ImportError:
        return NEIGHBOUR_FLOOR[tag](pts, qs, scale)
    return reduce(tree)


def _pairwise_sq(pts: np.ndarray, qs: np.ndarray) -> np.ndarray:
    # One squared-distance kernel both floor rows read: the kNN floor sorts+`sqrt`s it, the radius floor thresholds it against
    # `r**2` with no per-pair `sqrt`.
    diff = qs[:, None, :] - pts[None, :, :]
    return np.einsum("qnd,qnd->qn", diff, diff)


def _knn_distances(distances: np.ndarray) -> SpatialEvidence:
    # One Proximity mean both the cKDTree `query` distances and the floor's sorted block fold, so the two paths
    # terminate identically; an EMPTY query set has no mean and says so. The retired `else 0.0` was not vacuous
    # evidence — a zero mean distance is the reading that every point coincides, which is exactly the claim an
    # unmeasured fold must not make, and every aggregation downstream folded the fabricated cell as data.
    return SpatialEvidence.Proximity(int(distances.size), Some(float(np.mean(distances))) if distances.size else Nothing)


def _floor_knn(pts: np.ndarray, qs: np.ndarray, k: int) -> SpatialEvidence:
    # `kth` clamps to the point count so a `k >= n` request mirrors the cKDTree tolerance rather
    # than slicing past the column count; `sqrt` lands only on the retained k-nearest columns.
    kth = min(k, pts.shape[0])
    return _knn_distances(np.sqrt(np.sort(_pairwise_sq(pts, qs), axis=1)[:, :kth]))


def _floor_radius(pts: np.ndarray, qs: np.ndarray, r: float) -> SpatialEvidence:
    return SpatialEvidence.Proximity(int(np.count_nonzero(_pairwise_sq(pts, qs) <= r * r)), radius=Some(r))


def _pairs(pts: np.ndarray, r: float) -> SpatialEvidence:
    return SpatialEvidence.Proximity(len(sp.cKDTree(pts).query_pairs(r)), radius=Some(r))


def _hull(pts: np.ndarray) -> SpatialEvidence:
    hull = sp.ConvexHull(pts)
    return SpatialEvidence.Complex("hull", int(hull.simplices.shape[0]), float(hull.volume))


def _triangulate(pts: np.ndarray) -> SpatialEvidence:
    # Delaunay carries no `.volume` the way ConvexHull does, so `measure` reads the triangulated point cardinality — a real fact,
    # never a simplex-count echo.
    tri = sp.Delaunay(pts)
    return SpatialEvidence.Complex("delaunay", int(tri.simplices.shape[0]), float(pts.shape[0]))


def _distances(left: np.ndarray, right: np.ndarray | None, metric: Metric) -> SpatialEvidence:
    # self-distance reduces the condensed `pdist` vector directly, never a dense squareform materialized only to mean over it.
    pairwise = sp.distance.pdist(left, metric=metric.value) if right is None else sp.distance.cdist(left, right, metric=metric.value)
    return SpatialEvidence.Complex(f"distance-{metric.value}", int(pairwise.size), float(np.mean(pairwise)))


def _tessellate(points: np.ndarray, kind: Tessellation, radius: float) -> SpatialEvidence:
    match kind:
        case Tessellation.VORONOI:
            vor = sp.Voronoi(points)
            return SpatialEvidence.Complex("voronoi", int(vor.ridge_points.shape[0]), float(len(vor.regions)))
        case Tessellation.SPHERICAL:
            sphere = sp.SphericalVoronoi(points, radius=radius)
            return SpatialEvidence.Complex("spherical-voronoi", int(sphere.vertices.shape[0]), float(len(sphere.regions)))
        case Tessellation.HALFSPACE:
            # Operand is the `(N, d+1)` halfspace stack `[A | b]`, not a point set; `HalfspaceIntersection` needs a strictly
            # feasible interior point, so the `linprog` Chebyshev centre replaces an `np.zeros` Qhull rejects as infeasible for any
            # stack not straddling the origin.
            verts = sp.HalfspaceIntersection(points, _interior_point(points)).intersections
            return SpatialEvidence.Complex("halfspace", int(verts.shape[0]), float(np.linalg.norm(verts.max(axis=0) - verts.min(axis=0))))
        case _ as unreachable:
            assert_never(unreachable)


def _interior_point(halfspaces: np.ndarray) -> np.ndarray:
    a, b = halfspaces[:, :-1], halfspaces[:, -1]
    norms = np.linalg.norm(a, axis=1, keepdims=True)
    result = linprog(np.concatenate([np.zeros(a.shape[1]), [-1.0]]), A_ub=np.hstack([a, norms]), b_ub=-b, bounds=(None, None))
    # an INFEASIBLE stack carries `x=None`, and the retired unconditional `result.x[:-1]` read it as a subscript —
    # a bare `TypeError` from inside the kernel rather than the refusal the caller can act on. The raise lands at the
    # fence exactly as the `neighbours` admission does, so an empty intersection reports itself as one.
    if not result.success:
        raise ValueError(f"halfspace stack admits no strictly feasible interior point: {result.message}")
    return np.asarray(result.x[:-1], dtype=float)


def _align(source: np.ndarray, target: np.ndarray) -> SpatialEvidence:
    # `align_vectors` returns the rotation and the root-sum-square deviation; the per-vector RMSD
    # is rssd / sqrt(n), so the evidence reports a sample-independent fit error, not a raw sum.
    _, rssd = sp.transform.Rotation.align_vectors(target, source)
    _, _, disparity = sp.procrustes(target, source)
    return SpatialEvidence.Alignment(float(rssd / np.sqrt(source.shape[0])), float(disparity))


def _alpha_shape(points: np.ndarray, alpha: float) -> SpatialEvidence:
    tri = sp.Delaunay(points)
    radii = np.asarray([_circumradius(simplex) for simplex in points[tri.simplices]])
    retained = tri.simplices[kept := radii < alpha]
    # each (d+1)-simplex contributes its (d+1) facets — the (d)-vertex subsets `combinations`
    # enumerates; a facet shared by two retained simplices is interior, one in exactly one is boundary.
    facets = np.sort(np.concatenate([retained[:, list(combo)] for combo in combinations(range(retained.shape[1]), retained.shape[1] - 1)]), axis=1)
    unique, counts = np.unique(facets, axis=0, return_counts=True)
    return SpatialEvidence.Boundary(int(unique[counts == 1].shape[0]), float(np.sum(radii[kept])))


def _circumradius(simplex: np.ndarray) -> float:
    base = simplex[1:] - simplex[0]
    rhs = 0.5 * np.einsum("ij,ij->i", base, base)
    return float(np.linalg.norm(np.linalg.solve(base, rhs)))


def _spatial_kernel(query: SpatialQuery, workers: int) -> "RuntimeRail[SpatialReceipt]":
    # module-level so REFERENCE shipping resolves it by import — a closure pays an eager cloudpickle round-trip
    # no thread arm needs; SYNCHRONOUS by contract, an async def hands the worker a bare coroutine object.
    # `catch` names the scipy.spatial raise surface this body reaches, probed against the installed band: every
    # degenerate Qhull tessellation raises `QhullError`, a `RuntimeError` subclass rather than a `ValueError` one, so
    # `RuntimeError` is what admits it WITHOUT naming `sp.QhullError` — evaluating that attribute in the tuple would
    # reify the lazy proxy at every call and defeat the `NEIGHBOUR_FLOOR` an absent scipy depends on. Ragged operands,
    # refused metrics, correspondence mismatches, the two admission raises, and `np.linalg.LinAlgError` — the narrower
    # subclass, leading so the classifier reads the precise type — cover the rest.
    return ArrayPayload.admit(ArraySource.Live(query.points), (), FiniteGate.REJECT).bind(
        lambda payload: ContentIdentity.of(f"spatial.{query.tag}", IdentitySource(parts=query.identity_parts(payload.content_key))).bind(
            lambda result_key: boundary(
                SPATIAL_RESOLVE,
                lambda: SpatialReceipt.of(
                    query.tag, query.cardinality,
                    Provenance(consumed=Block.singleton(payload.content_key), produced=result_key),
                    query.resolve(workers),
                ),
                catch=(np.linalg.LinAlgError, ValueError, RuntimeError),
            )
        )
    )


async def solve(query: SpatialQuery, lane: LanePolicy, *, composition: ScopeKey = DEFAULT_SCOPE) -> "RuntimeRail[SpatialReceipt]":
    # Weave owns span, fence, and the fenced contributor harvest; the whole-lane grant bounds the KD-tree scan team.
    async def dispatch() -> RuntimeRail[SpatialReceipt]:
        # One flatten from `RuntimeRail[RuntimeRail[SpatialReceipt]]` to `RuntimeRail[SpatialReceipt]`.
        return (
            await lane.whole(
                lambda grant: lane.offload(Kernel.of(_spatial_kernel, KernelTrait.RELEASING), query, grant.width)
            )
        ).bind(lambda rail: rail)

    return await evidence_run(EvidenceScope.SPATIAL, f"spatial.{query.tag}", dispatch, facts={"query": query.tag, "points": query.cardinality}, composition=composition)
```

## [03]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)

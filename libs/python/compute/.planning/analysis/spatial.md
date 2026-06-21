# [PY_COMPUTE_SPATIAL]

The one array-native computational-geometry owner over `scipy.spatial`. `SpatialQuery` is a `@tagged_union` discriminating Qhull tessellation, KD-tree proximity, the pairwise/condensed distance matrix, the rotation-and-alignment algebra, and the alpha-shape boundary fold over a point set, and `SpatialQuery.resolve` is the one total `match` folding every case to a `SpatialEvidence` discriminated outcome — `Proximity`, `Complex`, `Boundary`, or `Alignment` — that `SpatialReceipt` carries whole, the evidence owning its own `facts()` slot projection, exactly as `analysis/symbolic.md#OP` collapses its `Outcome` and the way the sibling `analysis/signal.md#DSP` and `analysis/transform.md#TRANSFORM` owners cite this owner for collapsing `Proximity`/`Complex`/`Boundary`. The point set admits through `numerics/array.md#PAYLOAD` so the `ArrayPayload.content_key` keys the receipt by `ContentIdentity` and a repeated query on identical points is a cache hit by reference. A data-driven `NEIGHBOUR_FLOOR` supplies a numpy brute-force pairwise reduction for the two KD-tree proximity routes so a cp315 run with the scipy wheel absent still returns a `SpatialReceipt` rather than `Error(import_)`; the Qhull hull/Delaunay/Voronoi, the rotation algebra, and the distance matrix carry no floor because their backend is the gated capability itself. `@receipted` is the cross-cutting emit aspect so the point-cloud and sample-set neighbourhood evidence the study spine reads streams without an inline `emit`. This owner emits point-set evidence and never re-owns the geometry-branch `geometry/mesh/spatial.md#SPATIAL` `trimesh` mesh surface, meeting it only at the graduation wire.

## [01]-[INDEX]

- [01]-[SPATIAL]: the `SpatialQuery` `@tagged_union` over Qhull tessellation, KD-tree proximity, the pairwise distance matrix, the rotation-and-alignment algebra, and the alpha-shape boundary fold; `SpatialQuery.resolve` the one total `match` folding every case to the `SpatialEvidence` discriminated outcome `SpatialReceipt` carries whole through the evidence's own `facts()` projection; `Metric`/`Tessellation` the bounded backend-argument vocabularies; `NEIGHBOUR_FLOOR` the data-driven numpy proximity floor for the two KD-tree routes; the `ArrayPayload`-admitted `ContentIdentity` key the study spine's `@receipted` aspect harvests off the `Ok`-arm contributor.

## [02]-[SPATIAL]

- Owner: `SpatialQuery` — the array-native computational-geometry cases over a point set discriminated by the geometric question, never a per-query owner and never a `Neighbours`/`Hull`/`Triangulate` method family. `Neighbours(points, queries, k)` over `scipy.spatial.cKDTree.query` (the `workers=-1` thread-parallel k-nearest), `Radius(points, queries, radius)` over `cKDTree.query_ball_point`, `Pairs(points, radius)` over `cKDTree.query_pairs` (the within-set proximity graph), `Distances(left, right, metric)` over `scipy.spatial.distance.cdist` (cross-set) or `distance.pdist`+`squareform` (self, `right` empty) discriminated by the `Metric` vocabulary, `Hull(points)` over `scipy.spatial.ConvexHull`, `Triangulate(points)` over `scipy.spatial.Delaunay`, `Tessellate(points, kind, radius)` over `scipy.spatial.Voronoi`/`SphericalVoronoi`/`HalfspaceIntersection` discriminated by the `Tessellation` vocabulary, `AlphaShape(points, alpha)` folding the boundary from the Delaunay circumradius, and `Align(source, target)` over `scipy.spatial.transform.Rotation.align_vectors` (Kabsch) and `scipy.spatial.procrustes` (the optimal similarity disparity). `solve` returns `RuntimeRail[SpatialReceipt]`, admitting the point set through `numerics/array.md#PAYLOAD` and folding the resolved `SpatialEvidence` to one `SpatialReceipt`.
- Outcome: `SpatialEvidence` is the `@tagged_union` parameterizing the result the way the input is parameterized — `Proximity(count, mean_distance, radius)` for the two KD-tree routes and the within-set `Pairs` graph, `Complex(kind, cardinality, measure)` for the Qhull hull/Delaunay/Voronoi/halfspace tessellation and the distance-matrix summary, `Boundary(facets, total_radius)` for the alpha-shape reconstruction, and `Alignment(rmsd, disparity)` for the Kabsch/procrustes fit — and `SpatialQuery.resolve` is the one total `match` over the union tag returning `SpatialEvidence`, closed by `assert_never`, so adding a query writes only its geometry body returning a `SpatialEvidence` rather than a parallel `_neighbours`/`_hull`/`_triangulate` helper family. The evidence owns its own `SpatialEvidence.facts()` slot projection so `Proximity` names `count`/`mean_distance`/`radius`, `Complex` names `kind`/`cardinality`/`measure`, `Boundary` names `facets`/`total_radius`, and `Alignment` names `rmsd`/`disparity` — the `Complex` `cardinality` is the primitive count the `kind` string discriminates (hull facets, Delaunay simplices, Voronoi ridges, halfspace vertices, distance pairs), never a `simplices` label lying over a pair count for the distance route, and never a coarse `(count, measure)` pair smuggling four distinct outcome vocabularies through two overloaded columns — the same self-projecting shape `analysis/transform.md#TRANSFORM` holds over `TransformEvidence.facts()` and `analysis/signal.md#DSP` over `Spectral`/`Multiresolution`/`Scale`/`Packet`.
- Floor: `NEIGHBOUR_FLOOR` is the one data-driven `Map[Tag, NeighbourReduction]` carrying the numpy brute-force pairwise reduction for the `neighbours` and `radius` proximity routes, so the `ImportError` arm reads the row and folds the same `Proximity` evidence the `cKDTree` path produces — one floor table, never two `try`/`except ImportError` blocks each re-deriving a pairwise distance. The `pairs`, `distances`, `hull`, `triangulate`, `tessellate`, `alpha_shape`, and `align` tags carry no floor row because Qhull, the BLAS distance kernel, and the rotation SVD are the gated capabilities themselves; a cp315 run without the scipy wheel returns `Error(import_)` for those routes through the `CLASSIFY` `ImportError` row, and the floor table's membership is the explicit record of which routes degrade rather than fail.
- Alpha-shape fold: `_alpha_shape` is the one local kernel beside the scipy queries — no `scipy.spatial` alpha-shape primitive exists. It triangulates the point set, computes each `Delaunay` simplex circumradius from its vertex coordinates through `np.linalg.solve` over the edge basis, retains the simplices whose circumradius is below `alpha`, and reduces them to the boundary facets appearing in exactly one retained simplex through `itertools.combinations(range(d+1), d)` over the facet-vertex subsets and `np.unique(..., return_counts=True)` — the reconstructed boundary the study spine reads, the facet enumeration the stdlib `combinations` primitive rather than a hand-rolled drop-one index generator. `_circumradius` is private to the kernel, not a module-level sibling of the dispatch.
- Receipt: `SpatialReceipt.of(tag, points, key, evidence)` folds the `(query, points, content_key, evidence)` carrier with no per-case projection — the discrimination lives in `SpatialEvidence.facts()`, not a `match` over flattened columns — the same evidence-on-receipt carry `analysis/signal.md#DSP` and `analysis/symbolic.md#OP` hold. `contribute` `yield`s into the `Iterable[Receipt]` the `runtime/observability/receipts#RECEIPT` `ReceiptContributor` Protocol declares (`contribute(self) -> Iterable[Receipt]`) — the identical `yield`-into-`Iterable` shape the co-cited `analysis/signal.md#DSP` and `analysis/transform.md#TRANSFORM` contributors hold, never a `tuple[Receipt, ...]` drift on the one shared convention — one `Receipt.of("compute.spatial", ("emitted", query_tag, facts))` row, the two-argument shape-polymorphic factory over the `(Phase, subject, facts)` `Evidence` triple, never a four-positional `Receipt.of("emitted", owner, subject, facts)` the factory does not admit, whose `facts` spreads the `query`/`points`/`content_key.project("hex")` render plus the matched `SpatialEvidence.facts()` slots a study run records. `solve` is the `RuntimeRail[SpatialReceipt]` boundary owner (the error arm carries no contributor), so emission is not an `@receipted` decorator on `solve` but the study spine harvesting the resolved `SpatialReceipt` contributor through the `runtime/observability/receipts#RECEIPT` `@receipted` aspect on the `Ok` arm — the same convention `analysis/signal.md#DSP` and `analysis/transform.md#TRANSFORM` hold, the receipt the contributor and the rail the boundary form. A reconstructed boundary graduates outward through `graduation/handoff.md#GRADUATION` on the `geometry` `HandoffAxis` case aligned to the scan companion at the wire, never crossing as a geometry-branch mesh.
- Packages: `scipy` (`spatial.cKDTree.query`/`query_ball_point`/`query_pairs`, `spatial.ConvexHull`, `spatial.Delaunay`, `spatial.Voronoi`, `spatial.SphericalVoronoi`, `spatial.HalfspaceIntersection`, `spatial.distance.cdist`/`pdist`/`squareform`, `spatial.transform.Rotation.align_vectors`, `spatial.procrustes` — every spelling catalogued in `compute/.api/scipy.md`'s `scipy.spatial` entrypoint table — and `optimize.linprog` for the halfspace cell's Chebyshev-centre interior point), `numpy` (`asarray`, `einsum` the squared-distance/edge-basis contraction owner, `linalg.norm`, `linalg.solve`, `sort` the k-selection of the brute kNN floor, `sqrt`, `unique`, `concatenate`, `zeros`, `mean`, `count_nonzero`), `expression` (`tagged_union`/`tag`/`case` the `SpatialQuery` and `SpatialEvidence` ADTs, `Map` the `NEIGHBOUR_FLOOR` table), `msgspec` (`Struct(frozen=True)` the `SpatialReceipt`), `numerics/array.md#PAYLOAD` (`ArrayPayload.admit`/`ArrayOp.Live`/`FiniteGate` admitting the point set and keying the `content_key`), runtime (`RuntimeRail`, `boundary`, `Receipt`/`ReceiptContributor` from `runtime/receipts`, `ContentKey.project` carried by the admitted payload). The `runtime/observability/receipts#RECEIPT` `@receipted`/`Redaction` aspect is the study spine's harvest of the `Ok`-arm contributor, not an import this owner threads.
- Growth: a new spatial query is one `SpatialQuery` case plus one `resolve` arm returning a `SpatialEvidence`; a new evidence shape is one `SpatialEvidence` case plus one `facts()` arm (`SpatialReceipt` carries the evidence whole, so the receipt needs no edit); a new distance metric is one `Metric` row; a new tessellation backend is one `Tessellation` row; a new degrading route is one `NEIGHBOUR_FLOOR` table entry; zero new owner surface, never a per-query method family and never a re-implementation of the geometry-branch mesh surface.
- Boundary: array-native computational geometry over a point set only — proximity search, the within-set proximity graph, the pairwise distance matrix, convex hull, Delaunay, planar/spherical Voronoi and halfspace tessellation, the rotation-and-alignment algebra, and the alpha-shape boundary fold are in-scope. `scipy` carries no cp315 wheel, so the bodies are authored against the documented `scipy.spatial` API on the `python_version<'3.15'` companion band with the data-driven numpy floor reachable for the two KD-tree proximity routes. The geometry branch's `geometry/mesh/spatial.md#SPATIAL` `trimesh` mesh-surface proximity/ray/containment over a *built triangulation*, the `open3d`/`small_gicp` cloud-correspondence vertex-KNN, and the `geometry/scan/reconstruction.md#RECONSTRUCTION` mesh reconstruction are distinct owners; this owner operates on an in-memory point set and meets them only through the admitted operand and the graduation wire. A `Neighbours`/`Hull`/`Triangulate` method family, parallel `_neighbours`/`_radius`/`_hull` dispatch helpers where `resolve` folds one tag-keyed match, a flat `SpatialReceipt(query, count, measure, content_key)` flattening four distinct outcome vocabularies into one `(count, measure)` pair where `SpatialEvidence.facts()` carries the per-shape slots, a `contribute` returning a `tuple[Receipt, ...]` where the `ReceiptContributor` port declares `Iterable[Receipt]` and the co-cited siblings `yield`, an overloaded `measure` column smuggling volume/simplex-count/region-count/disparity per route, a `simplices` slot labelling the distance route's pair count where `Complex.cardinality` is the `kind`-discriminated primitive count, a `SpatialReceipt.of` `match` re-deriving columns the evidence already owns, a raw `rssd` reported as RMSD where `align_vectors` returns the root-sum-square deviation the fit normalizes by `sqrt(n)`, a `MINKOWSKI` metric row carrying no exponent that silently aliases `EUCLIDEAN` (`p=2`) where the `EUCLIDEAN`/`CITYBLOCK`/`CHEBYSHEV` rows already span the Minkowski endpoints, a mutable `np.empty((0, 0))` class-definition-time `Distances` default where `right is None` is the self-distance discriminant, a hand-rolled drop-one facet-index generator where `itertools.combinations` enumerates the facet-vertex subsets, two `try`/`except ImportError` blocks where the `NEIGHBOUR_FLOOR` table folds one reduction, a hand-rolled KD-tree or distance kernel `scipy.spatial` owns, a re-catalogued `trimesh`/`open3d` mesh surface, and a dense `mean` over a `pdist` matrix materialized where the condensed vector reduces directly are the deleted forms.

```python signature
from collections.abc import Callable, Iterable
from enum import StrEnum
from itertools import combinations
from typing import Final, Literal, assert_never

import numpy as np
from expression import case, tag, tagged_union
from expression.collections import Map
from msgspec import Struct

from rasm.compute.numerics.array import ArrayOp, ArrayPayload, FiniteGate
from rasm.runtime.content_identity import ContentKey
from rasm.runtime.faults import RuntimeRail, boundary
from rasm.runtime.receipts import Receipt

# --- [TYPES] ----------------------------------------------------------------------------

type Tag = Literal["neighbours", "radius", "pairs", "distances", "hull", "triangulate", "tessellate", "alpha_shape", "align"]
type NeighbourReduction = Callable[[np.ndarray, np.ndarray, float], "SpatialEvidence"]


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
    proximity: tuple[int, float, float] = case()
    complex_: tuple[str, int, float] = case()
    boundary: tuple[int, float] = case()
    alignment: tuple[float, float] = case()

    @staticmethod
    def Proximity(count: int, mean_distance: float, radius: float = 0.0) -> "SpatialEvidence":
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
        match self:
            case SpatialEvidence(tag="proximity", proximity=(count, mean_distance, radius)):
                return {"count": count, "mean_distance": f"{mean_distance:.6g}", "radius": f"{radius:.6g}"}
            case SpatialEvidence(tag="complex_", complex_=(kind, cardinality, measure)):
                return {"kind": kind, "cardinality": cardinality, "measure": f"{measure:.6g}"}
            case SpatialEvidence(tag="boundary", boundary=(facets, total_radius)):
                return {"facets": facets, "total_radius": f"{total_radius:.6g}"}
            case SpatialEvidence(tag="alignment", alignment=(rmsd, disparity)):
                return {"rmsd": f"{rmsd:.6g}", "disparity": f"{disparity:.6g}"}
            case unreachable:
                assert_never(unreachable)


class SpatialReceipt(Struct, frozen=True):
    query: str
    points: int
    content_key: ContentKey
    evidence: SpatialEvidence

    @staticmethod
    def of(tag: Tag, points: int, key: ContentKey, evidence: SpatialEvidence) -> "SpatialReceipt":
        return SpatialReceipt(tag, points, key, evidence)

    def contribute(self) -> Iterable[Receipt]:
        facts = {"query": self.query, "points": self.points, "content_key": self.content_key.project("hex"), **self.evidence.facts()}
        yield Receipt.of("compute.spatial", ("emitted", self.query, facts))


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
        # the point set the ArrayPayload admits and the ContentKey seeds, recovered from every
        # case so admission and identity stay one path; the two-set routes (cross-distance,
        # align) stack both operands so both coordinate buffers seed the key and a shared-`left`
        # query with a distinct `right`/`target` never collides, the concatenated-`tobytes` key
        # `numerics/statistics.md#STAT_CONTENT_KEY` holds over its one-or-two sample buffer.
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
            case unreachable:
                assert_never(unreachable)

    def resolve(self) -> SpatialEvidence:
        match self:
            case SpatialQuery(tag="neighbours", neighbours=(pts, qs, k)):
                dist = lambda t: np.asarray(t.query(qs, k=k, workers=-1)[0], dtype=float)
                return _proximity("neighbours", lambda t: SpatialEvidence.Proximity(int((d := dist(t)).size), float(np.mean(d))), pts, qs, float(k))
            case SpatialQuery(tag="radius", radius=(pts, qs, r)):
                hits = lambda t: int(sum(len(h) for h in t.query_ball_point(qs, r=r)))
                return _proximity("radius", lambda t: SpatialEvidence.Proximity(hits(t), 0.0, r), pts, qs, r)
            case SpatialQuery(tag="pairs", pairs=(pts, r)):
                from scipy.spatial import cKDTree

                return SpatialEvidence.Proximity(len(cKDTree(pts).query_pairs(r)), 0.0, r)
            case SpatialQuery(tag="distances", distances=(left, right, metric)):
                return _distances(left, right, metric)
            case SpatialQuery(tag="hull", hull=pts):
                from scipy.spatial import ConvexHull

                hull = ConvexHull(pts)
                return SpatialEvidence.Complex("hull", int(hull.simplices.shape[0]), float(hull.volume))
            case SpatialQuery(tag="triangulate", triangulate=pts):
                from scipy.spatial import Delaunay

                # measure is the triangulated point cardinality, distinct from the simplex count;
                # Delaunay carries no .volume the way ConvexHull does, so the count read off the
                # admitted operand keeps the Complex measure a real fact rather than a count echo.
                tri = Delaunay(pts)
                return SpatialEvidence.Complex("delaunay", int(tri.simplices.shape[0]), float(pts.shape[0]))
            case SpatialQuery(tag="tessellate", tessellate=(pts, kind, radius)):
                return _tessellate(pts, kind, radius)
            case SpatialQuery(tag="alpha_shape", alpha_shape=(pts, alpha)):
                return _alpha_shape(pts, alpha)
            case SpatialQuery(tag="align", align=(source, target)):
                return _align(source, target)
            case unreachable:
                assert_never(unreachable)


# --- [TABLES] ---------------------------------------------------------------------------

# the data-driven numpy proximity floor: the `neighbours`/`radius` ImportError arm reads its
# row and folds the same Proximity evidence the cKDTree path produces, so the floor is one
# table membership rather than two try/except blocks each re-deriving a pairwise distance.
# A tag absent from this table (hull/triangulate/tessellate/distances/pairs/align) has no
# floor — Qhull, the BLAS distance kernel, and the rotation SVD are the gated capability.
NEIGHBOUR_FLOOR: Final[Map[Tag, NeighbourReduction]] = Map.of_seq([
    ("neighbours", lambda pts, qs, k: _floor_knn(pts, qs, int(k))),
    ("radius", lambda pts, qs, r: _floor_radius(pts, qs, r)),
])


# --- [OPERATIONS] -----------------------------------------------------------------------


def _proximity(tag: Tag, reduce: Callable[[object], SpatialEvidence], pts: np.ndarray, qs: np.ndarray, scale: float) -> SpatialEvidence:
    # the two KD-tree proximity routes share one symmetric fold: the per-route `reduce` closure
    # already terminates in `Proximity` evidence, so the scipy path and the `NEIGHBOUR_FLOOR` row
    # are the same `-> SpatialEvidence` shape and the body carries no per-tag ternary smear — run
    # the scipy reduction, or fall to the floor row for this exact tag when the wheel is absent so
    # a cp315 run still returns `Proximity` rather than `Error(import_)`.
    try:
        from scipy.spatial import cKDTree

        return reduce(cKDTree(pts))
    except ImportError:
        return NEIGHBOUR_FLOOR[tag](pts, qs, scale)


def _floor_knn(pts: np.ndarray, qs: np.ndarray, k: int) -> SpatialEvidence:
    # the squared pairwise distance contracts through `einsum` (the catalogued reduction owner)
    # rather than `linalg.norm`, so the brute floor reads one fused contraction and `sqrt` only
    # the k-nearest columns the sort retains; `kth` clamps to the point count so a `k >= n`
    # request mirrors the cKDTree path's tolerance rather than slicing past the column count.
    diff = qs[:, None, :] - pts[None, :, :]
    sq = np.einsum("qnd,qnd->qn", diff, diff)
    kth = min(k, pts.shape[0])
    dist = np.sqrt(np.sort(sq, axis=1)[:, :kth])
    return SpatialEvidence.Proximity(int(dist.size), float(np.mean(dist)))


def _floor_radius(pts: np.ndarray, qs: np.ndarray, r: float) -> SpatialEvidence:
    # in-radius membership compares squared distance to `r**2`, so the floor counts hits off the
    # `einsum` contraction with no `sqrt` per pair — the same fused reduction the kNN floor reads.
    diff = qs[:, None, :] - pts[None, :, :]
    return SpatialEvidence.Proximity(int(np.count_nonzero(np.einsum("qnd,qnd->qn", diff, diff) <= r * r)), 0.0, r)


def _distances(left: np.ndarray, right: np.ndarray | None, metric: Metric) -> SpatialEvidence:
    from scipy.spatial import distance

    # self-distance (`right is None`) reduces the condensed vector directly — never a dense
    # squareform materialized only to mean over it; cross-distance reads the full cdist matrix.
    # The `cardinality` slot is the pair count; `kind` discriminates that it is a distance summary.
    pairwise = distance.pdist(left, metric=metric.value) if right is None else distance.cdist(left, right, metric=metric.value)
    return SpatialEvidence.Complex(f"distance-{metric.value}", int(pairwise.size), float(np.mean(pairwise)))


def _tessellate(points: np.ndarray, kind: Tessellation, radius: float) -> SpatialEvidence:
    from scipy.spatial import HalfspaceIntersection, SphericalVoronoi, Voronoi

    match kind:
        case Tessellation.VORONOI:
            vor = Voronoi(points)
            return SpatialEvidence.Complex("voronoi", int(vor.ridge_points.shape[0]), float(len(vor.regions)))
        case Tessellation.SPHERICAL:
            sphere = SphericalVoronoi(points, radius=radius)
            return SpatialEvidence.Complex("spherical-voronoi", int(sphere.vertices.shape[0]), float(len(sphere.regions)))
        case Tessellation.HALFSPACE:
            # the operand is the `(N, d+1)` halfspace stack `[A | b]` (`A·x + b <= 0`), not a point
            # set; `HalfspaceIntersection` requires a strictly feasible interior point, so the
            # Chebyshev centre is read off the catalogued `linprog` (maximise the inscribed radius
            # `x* = argmax r s.t. A·x + ||A_i|| r <= -b`) rather than an `np.zeros` that Qhull
            # rejects as infeasible for any stack not straddling the origin.
            verts = HalfspaceIntersection(points, _interior_point(points)).intersections
            return SpatialEvidence.Complex("halfspace", int(verts.shape[0]), float(np.linalg.norm(verts.max(axis=0) - verts.min(axis=0))))
        case unreachable:
            assert_never(unreachable)


def _interior_point(halfspaces: np.ndarray) -> np.ndarray:
    from scipy.optimize import linprog

    a, b = halfspaces[:, :-1], halfspaces[:, -1]
    norms = np.linalg.norm(a, axis=1, keepdims=True)
    result = linprog(
        np.concatenate([np.zeros(a.shape[1]), [-1.0]]),
        A_ub=np.hstack([a, norms]),
        b_ub=-b,
        bounds=(None, None),
    )
    return np.asarray(result.x[:-1], dtype=float)


def _align(source: np.ndarray, target: np.ndarray) -> SpatialEvidence:
    from scipy.spatial import procrustes
    from scipy.spatial.transform import Rotation

    # align_vectors returns the rotation and the root-sum-square deviation; the per-vector RMSD
    # is rssd / sqrt(n), so the evidence reports a sample-independent fit error, not a raw sum.
    _, rssd = Rotation.align_vectors(target, source)
    _, _, disparity = procrustes(target, source)
    return SpatialEvidence.Alignment(float(rssd / np.sqrt(source.shape[0])), float(disparity))


def _alpha_shape(points: np.ndarray, alpha: float) -> SpatialEvidence:
    from scipy.spatial import Delaunay

    tri = Delaunay(points)
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


def solve(query: SpatialQuery) -> "RuntimeRail[SpatialReceipt]":
    operand = query.points
    return ArrayPayload.admit(ArrayOp.Live(operand), (), FiniteGate.REJECT).bind(
        lambda payload: boundary(
            f"spatial.{query.tag}",
            lambda: SpatialReceipt.of(query.tag, int(operand.shape[0]), payload.content_key, query.resolve()),
        )
    )
```

## [03]-[RESEARCH]

- [SCIPY_SPATIAL]: the `scipy.spatial.cKDTree.query(x, k, workers)`/`query_ball_point(x, r)`/`query_pairs(r)`, `ConvexHull(points).simplices`/`.volume`, `Delaunay(points).simplices`, `Voronoi(points).ridge_points`/`.regions`, `SphericalVoronoi(points, radius).vertices`/`.regions`, `HalfspaceIntersection(halfspaces, ip).intersections`, `distance.cdist(XA, XB, metric)`/`pdist(X, metric)`/`squareform`, `transform.Rotation.align_vectors(a, b)` (returning the `(rotation, rssd)` pair whose second element is the root-sum-square deviation), and `procrustes(data1, data2)` (returning the `(mtx1, mtx2, disparity)` triple) spellings hold against `compute/.api/scipy.md`'s `scipy.spatial` entrypoint table rows [01]-[10]; the halfspace cell operand is the `(N, d+1)` halfspace stack `[A | b]`, its strictly-feasible interior point the Chebyshev centre read off the catalogued `scipy.optimize.linprog` (the documented `HalfspaceIntersection` recipe) rather than an `np.zeros` Qhull rejects as infeasible, and it reads its bounding-box diagonal off `.intersections` rather than a dual-volume attribute the catalogue does not enumerate. They carry the `python_version<'3.15'` scipy marker on the gated companion band. The `NEIGHBOUR_FLOOR` numpy reduction runs unconditionally on cp315 for the two KD-tree proximity routes, so both the k-nearest and radius queries return a `SpatialReceipt` rather than `Error(import_)` where the scipy wheel is absent; the hull, Delaunay, Voronoi, distance-matrix, pairs-graph, and alignment routes carry no floor row because Qhull, the BLAS distance kernel, and the rotation SVD are the gated capabilities themselves, so the floor table's membership is the explicit record of which routes degrade versus fail.
- [SPATIAL_EVIDENCE]: the terminal `SpatialEvidence` `@tagged_union` over `Proximity`/`Complex`/`Boundary`/`Alignment` collapses what a flat `SpatialReceipt` of `(query, count, measure, content_key)` would have stored with the `measure` column overloaded across hull volume, Delaunay simplex count, Voronoi region count, and procrustes disparity into one discriminated outcome that `SpatialReceipt(query, points, content_key, evidence)` carries whole, the evidence owning its own `facts()` slot projection so each shape names its real vocabulary, mirroring `analysis/transform.md#TRANSFORM`'s `TransformEvidence.facts()` carry, `analysis/symbolic.md#OP`'s `Outcome` union, and `analysis/signal.md#DSP`'s `Spectral`/`Multiresolution`/`Scale`/`Packet` collapse; the `Alignment` case carries the Kabsch fit RMSD (`align_vectors` returns the root-sum-square deviation the fold normalizes by `sqrt(n)` to a sample-independent error) and the procrustes disparity, so the rotation algebra is first-class evidence rather than smuggled through a shared scalar, and a new outcome is one `SpatialEvidence` case plus one `facts()` arm rather than a new nullable field on a fat struct or a re-derived `SpatialReceipt.of` column.
- [POINT_CONTENT_KEY]: `solve` admits the point set through `ArrayPayload.admit(ArrayOp.Live(query.points), (), FiniteGate.REJECT)` from `numerics/array.md#PAYLOAD` and binds the resolved payload into the `boundary` thunk, so a non-finite point set returns the admission fault rather than producing a degenerate hull or a NaN-poisoned distance, the backend `xp` resolves once through `array_namespace`, and the `ArrayPayload.content_key` keys the `SpatialReceipt` through `ContentIdentity` the way `numerics/statistics.md#STAT_CONTENT_KEY` keys its sample buffer — a repeated query on identical points a cache hit by reference. The `FiniteGate.REJECT` row forbids any non-finite (NaN or ±inf) at admission. The owner never re-rolls the `array_namespace` dispatch, the `FiniteGate` masked reduction, or the `ascontiguousarray(...).tobytes()` canonical-buffer hashing the payload owner holds; `SpatialQuery.points` recovers the admitted operand from the leading element of every case so admission and identity stay one path across the union.

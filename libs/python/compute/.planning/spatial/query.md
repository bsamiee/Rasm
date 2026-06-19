# [PY_COMPUTE_QUERY]

The one array-native computational-geometry owner over `scipy.spatial`. `SpatialQuery` discriminates nearest-neighbour and radius search over a `cKDTree`, convex hull and Delaunay triangulation over Qhull, Voronoi tessellation, and the alpha-shape boundary extraction folded from the Delaunay circumradius, every query folding into one typed `SpatialReceipt` keyed by the `ArrayPayload` content identity. The brute-force neighbour floor runs unconditionally on cp315 where the scipy wheel is absent; the hull, Delaunay, Voronoi, and `cKDTree` paths gate on the scipy `python_version<'3.15'` marker. The owner produces point-cloud and sample-set neighbourhood evidence the study spine reads and that aligns to the geometry-branch scan companion at the wire, distinct from the geometry branch's own `open3d` mesh owner which this owner never re-implements.

## [1]-[INDEX]

[QUERY]: nearest-neighbour/radius search, convex hull, Delaunay, Voronoi, and the alpha-shape boundary fold on one `SpatialQuery` owner.

## [2]-[QUERY]

- Owner: `SpatialQuery` — the array-native computational-geometry cases over a point set; `Neighbours(points, queries, k)` over `scipy.spatial.cKDTree.query` with a numpy brute-force floor, `Radius(points, queries, radius)` over `cKDTree.query_ball_point`, `Hull(points)` over `scipy.spatial.ConvexHull`, `Triangulate(points)` over `scipy.spatial.Delaunay`, `Tessellate(points)` over `scipy.spatial.Voronoi`, and `AlphaShape(points, alpha)` folding the boundary from the Delaunay simplices by the circumradius threshold. `solve` returns `RuntimeRail[SpatialReceipt]`, and `_dispatch` matches the six routes total. Every query keys by the `ArrayPayload` content identity of its point set, so a repeated query on identical points is a cache hit by reference.
- Entry: `SpatialQuery.solve` enters one `boundary(f"spatial.{query.tag}", ...)`; the neighbour route folds the per-query distance-and-index pair into `SpatialReceipt.Proximity`, the hull and Delaunay routes fold the simplex count and the measure (hull volume/area, triangulation simplex count) into `SpatialReceipt.Complex`, the Voronoi route folds the ridge and region counts into `SpatialReceipt.Complex`, and the alpha-shape route folds the retained boundary-facet count into `SpatialReceipt.Boundary`. Both `cKDTree` neighbour routes — k-nearest (`query`) and radius (`query_ball_point`) — reach the numpy brute-force pairwise-distance floor when the scipy wheel is absent, so a cp315 run never returns `Error(Import)` for either neighbour query.
- Alpha-shape fold: `_alpha_shape` is the one local algorithm beside the scipy queries — it triangulates the point set, computes the circumradius of each `Delaunay` simplex from its vertex coordinates, retains the simplices whose circumradius is below `alpha`, and reduces the retained simplices to the set of boundary facets appearing in exactly one retained simplex. The boundary-facet set is the reconstructed shape boundary the study spine reads; no scipy alpha-shape primitive exists, so the circumradius threshold is the single hand-authored kernel.
- Receipt: `SpatialReceipt.contribute` emits one `Receipt.of("emitted", ...)` row carrying the query tag, the primary count, and the content key; a reconstructed boundary graduates outward through `graduation/receipt.md#GRADUATION` aligned to the geometry-branch scan companion at the wire, never crossing as a geometry-branch mesh.
- Packages: `scipy` (`spatial.cKDTree`, `spatial.cKDTree.query`, `spatial.cKDTree.query_ball_point`, `spatial.ConvexHull`, `spatial.Delaunay`, `spatial.Voronoi`), `numpy` (`asarray`, `ascontiguousarray`, `linalg.norm`, `linalg.solve`, `einsum`, `sort`, `unique`, `concatenate`), `arrays/payload.md#PAYLOAD` (the point set admits as an `ArrayPayload` keying through the same `ContentIdentity.of` seed), runtime (`RuntimeRail`, `boundary`, `Receipt`/`ReceiptContributor`, `ContentIdentity`/`ContentKey`/`IdentityPolicy`).
- Growth: a new spatial query is one `SpatialQuery` case plus one `_dispatch` arm; a new receipt shape is one `SpatialReceipt` case; the alpha threshold is one parameter on the `AlphaShape` case; zero new surface, never a per-query owner and never a re-implementation of the geometry-branch mesh surface.
- Boundary: array-native computational geometry over a point set only — neighbour search, convex hull, Delaunay, Voronoi, and the alpha-shape boundary fold are in-scope. `scipy` carries no cp315 wheel, so the `cKDTree`/`ConvexHull`/`Delaunay`/`Voronoi` bodies are authored against the documented `scipy.spatial` API with a reachable numpy brute-force neighbour floor. The geometry branch's `open3d` mesh reconstruction, registration, and surface tessellation are the deleted forms; this owner emits point-set neighbourhood evidence and never re-owns the geometry mesh surface, meeting it only at the graduation wire.

```python signature
from typing import Literal, assert_never

import numpy as np
from expression import case, tag, tagged_union
from msgspec import Struct

from rasm.runtime.content_identity import ContentIdentity, ContentKey, IdentityPolicy
from rasm.runtime.faults import RuntimeRail, boundary
from rasm.runtime.receipts import Receipt


class SpatialReceipt(Struct, frozen=True):
    query: str
    count: int
    measure: float
    content_key: ContentKey

    def contribute(self) -> Receipt:
        facts = {"query": self.query, "count": str(self.count), "measure": f"{self.measure:.6g}"}
        return Receipt.of("emitted", "compute.spatial", self.query, facts)

    @staticmethod
    def Proximity(count: int, mean_distance: float, key: ContentKey) -> "SpatialReceipt":
        return SpatialReceipt("proximity", count, mean_distance, key)

    @staticmethod
    def Complex(query: str, simplices: int, measure: float, key: ContentKey) -> "SpatialReceipt":
        return SpatialReceipt(query, simplices, measure, key)

    @staticmethod
    def Boundary(facets: int, total_measure: float, key: ContentKey) -> "SpatialReceipt":
        return SpatialReceipt("alpha_shape", facets, total_measure, key)


@tagged_union(frozen=True)
class SpatialQuery:
    tag: Literal["neighbours", "radius", "hull", "triangulate", "tessellate", "alpha_shape"] = tag()
    neighbours: tuple[np.ndarray, np.ndarray, int] = case()
    radius: tuple[np.ndarray, np.ndarray, float] = case()
    hull: np.ndarray = case()
    triangulate: np.ndarray = case()
    tessellate: np.ndarray = case()
    alpha_shape: tuple[np.ndarray, float] = case()

    @staticmethod
    def Neighbours(points: np.ndarray, queries: np.ndarray, k: int = 1) -> "SpatialQuery":
        return SpatialQuery(neighbours=(points, queries, k))

    @staticmethod
    def Radius(points: np.ndarray, queries: np.ndarray, radius: float) -> "SpatialQuery":
        return SpatialQuery(radius=(points, queries, radius))

    @staticmethod
    def Hull(points: np.ndarray) -> "SpatialQuery":
        return SpatialQuery(hull=points)

    @staticmethod
    def Triangulate(points: np.ndarray) -> "SpatialQuery":
        return SpatialQuery(triangulate=points)

    @staticmethod
    def Tessellate(points: np.ndarray) -> "SpatialQuery":
        return SpatialQuery(tessellate=points)

    @staticmethod
    def AlphaShape(points: np.ndarray, alpha: float) -> "SpatialQuery":
        return SpatialQuery(alpha_shape=(points, alpha))


def _point_key(points: np.ndarray) -> ContentKey:
    return ContentIdentity.of("point-set", np.ascontiguousarray(points).tobytes(), IdentityPolicy())


def solve(query: SpatialQuery) -> "RuntimeRail[SpatialReceipt]":
    return boundary(f"spatial.{query.tag}", lambda: _dispatch(query))


def _dispatch(query: SpatialQuery) -> SpatialReceipt:
    match query:
        case SpatialQuery(tag="neighbours", neighbours=(pts, qs, k)):
            return _neighbours(pts, qs, k)
        case SpatialQuery(tag="radius", radius=(pts, qs, r)):
            return _radius(pts, qs, r)
        case SpatialQuery(tag="hull", hull=pts):
            return _hull(pts)
        case SpatialQuery(tag="triangulate", triangulate=pts):
            return _triangulate(pts)
        case SpatialQuery(tag="tessellate", tessellate=pts):
            return _tessellate(pts)
        case SpatialQuery(tag="alpha_shape", alpha_shape=(pts, alpha)):
            return _alpha_shape(pts, alpha)
        case unreachable:
            assert_never(unreachable)


def _neighbours(points: np.ndarray, queries: np.ndarray, k: int) -> SpatialReceipt:
    key = _point_key(points)
    try:
        from scipy.spatial import cKDTree

        dist, _ = cKDTree(points).query(queries, k=k)
        return SpatialReceipt.Proximity(int(np.asarray(dist).size), float(np.mean(dist)), key)
    except ImportError:
        diff = queries[:, None, :] - points[None, :, :]
        dist = np.sort(np.linalg.norm(diff, axis=-1), axis=1)[:, :k]
        return SpatialReceipt.Proximity(int(dist.size), float(np.mean(dist)), key)


def _radius(points: np.ndarray, queries: np.ndarray, radius: float) -> SpatialReceipt:
    key = _point_key(points)
    try:
        from scipy.spatial import cKDTree

        hits = cKDTree(points).query_ball_point(queries, r=radius)
        total = int(sum(len(h) for h in hits))
        return SpatialReceipt.Proximity(total, radius, key)
    except ImportError:
        dist = np.linalg.norm(queries[:, None, :] - points[None, :, :], axis=-1)
        return SpatialReceipt.Proximity(int(np.count_nonzero(dist <= radius)), radius, key)


def _hull(points: np.ndarray) -> SpatialReceipt:
    from scipy.spatial import ConvexHull

    hull = ConvexHull(points)
    return SpatialReceipt.Complex("hull", int(hull.simplices.shape[0]), float(hull.volume), _point_key(points))


def _triangulate(points: np.ndarray) -> SpatialReceipt:
    from scipy.spatial import Delaunay

    tri = Delaunay(points)
    return SpatialReceipt.Complex("delaunay", int(tri.simplices.shape[0]), float(tri.simplices.shape[0]), _point_key(points))


def _tessellate(points: np.ndarray) -> SpatialReceipt:
    from scipy.spatial import Voronoi

    vor = Voronoi(points)
    return SpatialReceipt.Complex("voronoi", int(vor.ridge_points.shape[0]), float(len(vor.regions)), _point_key(points))


def _alpha_shape(points: np.ndarray, alpha: float) -> SpatialReceipt:
    from scipy.spatial import Delaunay

    tri = Delaunay(points)
    coords = points[tri.simplices]
    radii = np.asarray([_circumradius(simplex) for simplex in coords])
    retained = tri.simplices[radii < alpha]
    facets = np.sort(np.concatenate([retained[:, list(combo)] for combo in _facet_indices(retained.shape[1])]), axis=1)
    unique, counts = np.unique(facets, axis=0, return_counts=True)
    boundary_facets = unique[counts == 1]
    return SpatialReceipt.Boundary(int(boundary_facets.shape[0]), float(np.sum(radii[radii < alpha])), _point_key(points))


def _facet_indices(vertices_per_simplex: int) -> tuple[tuple[int, ...], ...]:
    return tuple(
        tuple(j for j in range(vertices_per_simplex) if j != drop) for drop in range(vertices_per_simplex)
    )


def _circumradius(simplex: np.ndarray) -> float:
    base = simplex[1:] - simplex[0]
    rhs = 0.5 * np.einsum("ij,ij->i", base, base)
    center_local = np.linalg.solve(base, rhs)
    return float(np.linalg.norm(center_local))
```

## [3]-[RESEARCH]

- [SCIPY_SPATIAL]: the `scipy.spatial.cKDTree(points).query(x, k)` (`(distance, index)` pair), `cKDTree.query_ball_point(x, r)` (per-query index list), `ConvexHull(points).simplices`/`.volume`/`.area`, `Delaunay(points).simplices`/`.points`/`.find_simplex`, and `Voronoi(points).vertices`/`.regions`/`.ridge_points` spellings carry the `python_version<'3.15'` marker and are not yet in `compute/.api/scipy.md` (which catalogues `scipy.linalg`/`sparse`/`optimize`/`integrate`/`interpolate` only — `scipy.spatial`, `scipy.signal`, and `scipy.stats` are named by their owners but not yet captured in the entrypoint tables); they verify against the `.api` catalogue under a uv-sync reflection pass once `scipy.spatial` is captured. The numpy brute-force neighbour floor (the `_neighbours` and `_radius` `ImportError` arms) runs unconditionally on cp315, so both the k-nearest and radius neighbour queries return a `SpatialReceipt` rather than `Error(Import)` where the scipy wheel is absent; the hull, Delaunay, and Voronoi routes carry no floor because Qhull is the gated capability itself.
- [ALPHA_CIRCUMRADIUS]: the `_circumradius` solve over the simplex edge basis and the `_facet_indices` boundary-facet fold are the one local algorithm; no `scipy.spatial` alpha-shape primitive exists, so the circumradius-threshold reconstruction stays a hand-authored kernel beside the scipy queries and reflects against numpy alone.
- [POINT_CONTENT_KEY]: `_point_key` derives the `ContentKey` over the canonical contiguous point buffer through `ContentIdentity.of("point-set", ...)` under the runtime `IdentityPolicy`, so a spatial query on a point set admitted through `arrays/payload.md#PAYLOAD` keys under the same `ContentIdentity` seed algebra; the `ascontiguousarray(...).tobytes()` canonical buffer matches the payload admission's host-transfer buffer for an already-contiguous cpu array.

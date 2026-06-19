# [PY_GEOMETRY_SCAN_DEVIATION]

Scan-vs-model deviation and primitive extraction — the central AEC value of a host-free scan companion, on top of the registered pose. `ScanDeviation` is one mode-discriminated owner: RANSAC plane/primitive segmentation extracting walls/slabs/columns from a registered cloud over the open3d `segment_plane` plus region-growing oversegmentation, and signed nearest-surface deviation (signed max/mean/std distance plus the over-build/under-build band) between the registered cloud and the IFC-tessellated GLB over `trimesh.proximity.signed_distance` composed with `trimesh.proximity.closest_point` for per-triangle attribution, emitting a per-element deviation receipt. The registered transform from `scan/registration.md#REGISTRATION` is the precondition; the reference surface is the `mesh/daemon.md#DAEMON` GLB output keyed by `ContentIdentity`. Deviation rows graduate through the compute `HandoffAxis` geometry case keyed to the IFC element GlobalId, the answer to whether the built geometry matches the IFC design within tolerance — `trimesh.proximity.signed_distance` is positive inside the watertight design solid and negative outside, so under-build (built material missing, the as-built cloud point lies inside the design surface, positive) is distinguished from over-build (excess material, the cloud point lies outside, negative); the construction-verification verdict reads on the absolute band against tolerance while the colored overlay reads the sign.

## [1]-[INDEX]

- [1]-[DEVIATION]: plane/primitive segmentation and nearest-surface deviation under one mode-discriminated owner over the open3d segmentation and trimesh proximity surfaces.

## [2]-[DEVIATION]

- Owner: `ScanDeviation` — the frozen owner discriminating by `DeviationMode` row over a registered `o3d.geometry.PointCloud` and an IFC-tessellated GLB reference; `DeviationResult` the per-element signed max/mean/std distance, the over-build/under-build extrema, and the compliant verdict against the tolerance; `Segment` the extracted-primitive carrier holding the plane model and the inlier count.
- Cases: `DeviationMode` rows `SEGMENT` (RANSAC `segment_plane` plus region-growing oversegmentation extracting the dominant planar primitives) and `DEVIATE` (the signed nearest-surface `proximity.signed_distance` query between the registered cloud and the reference GLB, folding the signed band into signed max/mean/std, the over-build/under-build extrema, and the compliant verdict) — matched by `match`/`assert_never`, each binding the engine that owns it; the `DEVIATE` arm composes the `SEGMENT` extraction only when element-keyed deviation requires per-primitive grouping, and composes `proximity.closest_point` for the triangle-id attribution only when the colored-overlay egress at the graduation seam needs per-face mapping (the band reduction itself needs only the signed array).
- Entry: `ScanDeviation.evaluate` admits the registered cloud, the reference GLB bytes, an element GlobalId, and a mode, runs the mode's pipeline through one `match`, and returns a `RuntimeRail[DeviationResult]`; the `SEGMENT` arm folds `segment_plane` over the residual cloud iteratively (re-segmenting the outlier remainder) into a `Segment` tuple, and the `DEVIATE` arm loads the GLB through `trimesh.load` forcing a `Trimesh`, queries `proximity.signed_distance` for the signed band over every cloud point, and reduces the signed array to the absolute deviation band plus the over/under extrema.
- Auto: `PointCloud.segment_plane(distance_threshold, ransac_n, num_iterations)` returns the plane model plus the inlier index set, and `select_by_index(inliers, invert=True)` peels the remainder so the next segmentation extracts the next dominant plane, the region-growing oversegmentation that separates adjacent walls/slabs; `trimesh.proximity.signed_distance(mesh, points)` returns the per-point signed distance (positive inside the watertight design solid, negative outside — no triangle id), so under-build is the signed `max`, over-build the signed `min`, the deviation band is `numpy` `max`/`mean`/`std` over the `abs` magnitude, and the compliant verdict is that absolute band against the tolerance — the construction-verification answer; `trimesh.proximity.closest_point(mesh, points)` returns the closest surface point, the unsigned distance, and the triangle id the colored-overlay egress composes for per-face attribution when the TS viewer seam needs it, never inside the band reduction.
- Receipt: each evaluation contributes an emitted-phase `Receipt.of` row through `ReceiptContributor` carrying the mode, the element GlobalId, the signed max/mean/std distance, the over/under-build extrema, and elapsed; the deviation produces a geometry `GraduationReceipt` subject (`scan-deviation`) keyed to the IFC element GlobalId, so the per-element deviation reaches the C# owner system and the TS viewer as a colored overlay through the one graduation rail.
- Packages: `open3d` (`geometry.PointCloud.segment_plane`/`select_by_index`), `trimesh` (`load`/`util.wrap_as_stream`/`proximity.signed_distance`/`proximity.closest_point`), `numpy` (`asarray`/`max`/`min`/`mean`/`std`/`abs` over the signed distance array), runtime (`RuntimeRail`/`ReceiptContributor`).
- Growth: a new primitive class (cylinder/column) is one segmentation engine bind inside the `SEGMENT` arm; a new deviation reduction is one fold over the signed distance array; a new tolerance policy is one field; the CPU offload of the per-point proximity loop is one `LanePolicy` hand-off (`scan/registration.md#REGISTRATION` shares the lane seam); zero new surface, no parallel per-primitive class family.
- Boundary: the registered pose is the precondition (the `register` rail from `scan/registration.md#REGISTRATION` supplies it, never re-derived here); the reference GLB is the `mesh/daemon.md#DAEMON` output, never re-tessellated; learned semantic segmentation is out of host-free CPU scope and informs only the segmentation heuristics; no IFC parse (that is `ifc-analysis`), no durable store, no Rhino/GH mutation; a per-primitive `extract_wall`/`extract_slab` family, a hand-rolled point-to-mesh distance kernel where `trimesh.proximity` is admitted, an unsigned-only `closest_point` reduction that cannot distinguish over-build from under-build, and a deviation that re-tessellates the IFC reference are the deleted forms — the signed band reads the construction-verification sign, the `closest_point` triangle id feeds only the overlay attribution.

```python signature
import open3d as o3d
import trimesh
import numpy as np
from enum import StrEnum
from typing import assert_never
from msgspec import Struct

from rasm.runtime.faults import RuntimeRail, boundary
from rasm.runtime.receipts import ReceiptContributor


class DeviationMode(StrEnum):
    SEGMENT = "segment"
    DEVIATE = "deviate"


class Segment(Struct, frozen=True):
    plane: tuple[float, float, float, float]
    inliers: int


class DeviationResult(Struct, frozen=True):
    mode: DeviationMode
    element: str
    max_distance: float
    mean_distance: float
    std_distance: float
    over_build: float
    under_build: float
    compliant: bool
    segments: tuple[Segment, ...] = ()


class ScanDeviation(Struct, frozen=True):
    distance_threshold: float = 0.02
    ransac_n: int = 3
    num_iterations: int = 1000
    tolerance: float = 0.05
    max_planes: int = 8

    def evaluate(self, cloud: o3d.geometry.PointCloud, reference_glb: bytes, element: str, mode: DeviationMode) -> "RuntimeRail[DeviationResult]":
        return boundary(f"deviation.{mode}", lambda: self._dispatch(cloud, reference_glb, element, mode))

    def _dispatch(self, cloud: o3d.geometry.PointCloud, reference_glb: bytes, element: str, mode: DeviationMode) -> DeviationResult:
        match mode:
            case DeviationMode.SEGMENT:
                segments = self._segment(cloud)
                return DeviationResult(mode, element, 0.0, 0.0, 0.0, 0.0, 0.0, True, segments)
            case DeviationMode.DEVIATE:
                mesh = trimesh.load(trimesh.util.wrap_as_stream(reference_glb), file_type="glb", force="mesh")
                points = np.asarray(cloud.points)
                signed = trimesh.proximity.signed_distance(mesh, points)
                magnitude = np.abs(signed)
                max_d, mean_d, std_d = float(magnitude.max()), float(magnitude.mean()), float(magnitude.std())
                under_build, over_build = float(signed.max()), float(signed.min())
                return DeviationResult(mode, element, max_d, mean_d, std_d, over_build, under_build, max_d <= self.tolerance)
            case unreachable:
                assert_never(unreachable)

    def _segment(self, cloud: o3d.geometry.PointCloud) -> tuple[Segment, ...]:
        remainder = cloud
        found: list[Segment] = []
        for _ in range(self.max_planes):
            if len(remainder.points) < self.ransac_n:
                break
            model, inliers = remainder.segment_plane(self.distance_threshold, self.ransac_n, self.num_iterations)
            found.append(Segment(tuple(model), len(inliers)))
            remainder = remainder.select_by_index(inliers, invert=True)
        return tuple(found)
```

## [3]-[RESEARCH]

- [SEGMENT_PLANE_ARITY]: the `PointCloud.segment_plane(distance_threshold, ransac_n, num_iterations) -> tuple[list[float], list[int]]` return (the `[a, b, c, d]` plane model and the inlier index list) and the `select_by_index(index, invert=True)` outlier-peel arity are folder-`.api`-confirmed (`open3d.md` rows `segment_plane`/`select_by_index`/`compute_point_cloud_distance`); the only open item is the owner-local iterative re-segmentation point-count guard, a heuristic the live run tunes, not a catalogue dependency.
- [SIGNED_BAND_REDUCTION]: the `DEVIATE` reduction folds `trimesh.proximity.signed_distance(mesh, points) -> NDArray` (folder-`.api`-confirmed `trimesh.md` row [6], "signed distance field samples", positive inside the watertight solid and negative outside per the trimesh proximity convention) into the over-build (`signed.min`)/under-build (`signed.max`) extrema and the absolute `max`/`mean`/`std` band against tolerance, distinguishing built-material-missing under-build from excess-material over-build for the construction-verification verdict. `trimesh.proximity.closest_point(mesh, points) -> tuple[NDArray, NDArray, NDArray]` (closest points, unsigned per-point distances, triangle ids — `trimesh.md` row [5]) carries no sign, so it composes ONLY for the colored-overlay per-face attribution at the graduation egress, never inside the band reduction. Owner-local detail confirmed against the catalogue: the watertight-input precondition for a valid `signed_distance` sign (a non-watertight reference returns an unreliable sign), satisfied because the reference GLB is the `mesh/daemon.md#DAEMON` weld-vertices output.

## [4]-[CROSS_FOLDER]

- [GRADUATION_SUBJECT]: the `scan-deviation` `GeometrySubject` literal this owner graduates is absent from the compute `graduation/handoff.md#GRADUATION` `GeometrySubject = Literal["registration-transform", "reconstructed-mesh", "topology-graph", "network-graph", "form-finding"]` union; the literal admits as one new `GeometrySubject` row on the compute owner before the `DEVIATE` arm's `GraduationReceipt` keys the element-deviation handoff, a single-literal cross-folder seam the synthesis tier reconciles against the compute owner, never authored into the compute interior from here. This literal is distinct from the `reconstructed-mesh` subject `scan/reconstruction.md#RECONSTRUCTION` produces and from the compute `analysis/spatial.md#SPATIAL` "reconstructed boundary, never crossing as a geometry-branch mesh" subject on the same geometry axis: `scan-deviation` carries the per-element signed deviation band keyed to the IFC GlobalId, a third non-colliding subject the one-pass reconciliation admits alongside the present `reconstructed-mesh`.

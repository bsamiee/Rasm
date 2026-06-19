# [PY_GEOMETRY_SCAN_DEVIATION]

Scan-vs-model deviation and primitive extraction — the central AEC value of a host-free scan companion, on top of the registered pose. `ScanDeviation` is one mode-discriminated owner: RANSAC plane/primitive segmentation extracting walls/slabs/columns from a registered cloud over the open3d `segment_plane` plus region-growing oversegmentation, and nearest-surface deviation (max/mean/std distance) between the registered cloud and the IFC-tessellated GLB over `trimesh.proximity.closest_point`, emitting a per-element deviation receipt. The registered transform from `registration.md#REGISTRATION` is the precondition; the reference surface is the `tessellation/daemon.md` GLB output keyed by `ContentIdentity`. Deviation rows graduate through the compute `HandoffAxis` geometry case keyed to the IFC element GlobalId, the answer to whether the built geometry matches the IFC design within tolerance.

## [1]-[INDEX]

[CLUSTERS]:
- `[2]-[DEVIATION]`: plane/primitive segmentation and nearest-surface deviation under one mode-discriminated owner over the open3d segmentation and trimesh proximity surfaces.

## [2]-[DEVIATION]

- Owner: `ScanDeviation` — the frozen owner discriminating by `DeviationMode` row over a registered `o3d.geometry.PointCloud` and an IFC-tessellated GLB reference; `DeviationResult` the per-element max/mean/std distance and the compliant verdict against the tolerance; `Segment` the extracted-primitive carrier holding the plane model and the inlier count.
- Cases: `DeviationMode` rows `SEGMENT` (RANSAC `segment_plane` plus region-growing oversegmentation extracting the dominant planar primitives) and `DEVIATE` (the nearest-surface `closest_point` query between the registered cloud and the reference GLB, folding the signed-distance band into max/mean/std and the compliant verdict) — matched by `match`/`assert_never`, each binding the engine that owns it; the `DEVIATE` arm composes the `SEGMENT` extraction only when element-keyed deviation requires per-primitive grouping.
- Entry: `ScanDeviation.evaluate` admits the registered cloud, the reference GLB bytes, an element GlobalId, and a mode, runs the mode's pipeline through one `match`, and returns a `RuntimeRail[DeviationResult]`; the `SEGMENT` arm folds `segment_plane` over the residual cloud iteratively (re-segmenting the outlier remainder) into a `Segment` tuple, and the `DEVIATE` arm loads the GLB through `trimesh.load` forcing a `Trimesh`, queries `proximity.closest_point` for every cloud point, and reduces the distance array to the deviation band.
- Auto: `PointCloud.segment_plane(distance_threshold, ransac_n, num_iterations)` returns the plane model plus the inlier index set, and `select_by_index(inliers, invert=True)` peels the remainder so the next segmentation extracts the next dominant plane, the region-growing oversegmentation that separates adjacent walls/slabs; `trimesh.proximity.closest_point(mesh, points)` returns the closest surface point, the per-point distance, and the triangle id, so the deviation band is `numpy` `max`/`mean`/`std` over the distance array; the compliant verdict is the max distance against the tolerance, the construction-verification answer.
- Receipt: each evaluation contributes an emitted-phase `Receipt.of` row through `ReceiptContributor` carrying the mode, the element GlobalId, the max/mean/std distance, and elapsed; the deviation produces a geometry `GraduationReceipt` subject (`scan-deviation`) keyed to the IFC element GlobalId, so the per-element deviation reaches the C# owner system and the TS viewer as a colored overlay through the one graduation rail.
- Packages: `open3d` (`geometry.PointCloud.segment_plane`/`select_by_index`), `trimesh` (`load`/`util.wrap_as_stream`/`proximity.closest_point`/`proximity.signed_distance`), `numpy` (`asarray`/`max`/`mean`/`std` over the distance array), runtime (`RuntimeRail`/`ReceiptContributor`).
- Growth: a new primitive class (cylinder/column) is one segmentation engine bind inside the `SEGMENT` arm; a new deviation reduction is one fold over the distance array; a new tolerance policy is one field; zero new surface, no parallel per-primitive class family.
- Boundary: the registered pose is the precondition (the `register` rail from `registration.md#REGISTRATION` supplies it, never re-derived here); the reference GLB is the `tessellation/daemon.md` output, never re-tessellated; learned semantic segmentation is out of host-free CPU scope and informs only the segmentation heuristics; no IFC parse (that is `ifc-analysis`), no durable store, no Rhino/GH mutation; a per-primitive `extract_wall`/`extract_slab` family, a hand-rolled point-to-mesh distance kernel where `trimesh.proximity` is admitted, and a deviation that re-tessellates the IFC reference are the deleted forms.

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
                return DeviationResult(mode, element, 0.0, 0.0, 0.0, True, segments)
            case DeviationMode.DEVIATE:
                mesh = trimesh.load(trimesh.util.wrap_as_stream(reference_glb), file_type="glb", force="mesh")
                points = np.asarray(cloud.points)
                _, distances, _ = trimesh.proximity.closest_point(mesh, points)
                max_d, mean_d, std_d = float(distances.max()), float(distances.mean()), float(distances.std())
                return DeviationResult(mode, element, max_d, mean_d, std_d, max_d <= self.tolerance)
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
- [CLOSEST_POINT_RETURN]: the `trimesh.proximity.closest_point(mesh, points) -> tuple[NDArray, NDArray, NDArray]` return triple (closest points, per-point distances, triangle ids) is folder-`.api`-confirmed (`trimesh.md` rows `proximity.closest_point`/`proximity.signed_distance`/`load`); the open item is owner-local, not catalogue — whether the `DEVIATE` reduction folds the unsigned `closest_point` distance or the `signed_distance` over/under band into the max/mean/std before the `compliant` verdict distinguishes over-build from under-build.

## [4]-[CROSS_FOLDER]

- [GRADUATION_SUBJECT]: the `scan-deviation` `GeometrySubject` literal this owner graduates is absent from the compute `graduation/receipt.md#GRADUATION` `GeometrySubject = Literal["registration-transform", "reconstructed-mesh", "topology-graph", "network-graph", "form-finding"]` union; the literal admits as one new `GeometrySubject` row on the compute owner before the `DEVIATE` arm's `GraduationReceipt` keys the element-deviation handoff, a single-literal cross-folder seam the synthesis tier reconciles against the compute owner, never authored into the compute interior from here.

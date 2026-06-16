# [PY_GEOMETRY_SCAN_PROCESSING]

Point-cloud and 3D-scan processing. `ScanProcessing` is one registration owner discriminating by registration-mode row: the broad ICP family over `open3d` and the parallel GICP/VGICP speed path over `small_gicp`, plus normal/feature estimation, voxel downsampling, and surface/mesh reconstruction. Registration transforms and reconstructed meshes graduate via the compute `HandoffAxis` geometry case.

## [1]-[INDEX]

| [INDEX] | [CLUSTER]    | [OWNS]                                                      |
| :-----: | :----------- | :---------------------------------------------------------- |
|   [1]   | REGISTRATION | mode-discriminated registration, estimation, reconstruction |

## [2]-[REGISTRATION]

- Owner: `ScanProcessing` — the frozen owner discriminating by `RegistrationMode` row; `ScanPayload` the point-cloud carrier; `RegistrationResult` the 4x4 transform + fitness + inlier RMSE receipt; `Reconstruction` the Poisson/ball-pivoting mesh result.
- Cases: `RegistrationMode` rows `ICP` (point-to-point, open3d) · `COLORED_ICP` (open3d) · `GENERALIZED_ICP` (open3d GICP) · `VGICP` (small_gicp parallel speed path) — matched by `match`/`case`, each binding the engine and estimation method that owns it.
- Entry: `ScanProcessing.register` admits source/target clouds, runs the mode's registration, and returns a `RuntimeRail[RegistrationResult]`; `ScanProcessing.reconstruct` builds a mesh from a registered cloud; `ScanProcessing.estimate` runs normal/covariance estimation and voxel downsampling as the shared pre-step.
- Auto: the open3d path runs `voxel_down_sample` then `estimate_normals` then `pipelines.registration.registration_icp`/`registration_generalized_icp` reading `RegistrationResult.transformation`/`.fitness`/`.inlier_rmse`; the small_gicp speed path runs `preprocess_points` then `align` against a `GaussianVoxelMap` (VGICP) reading `RegistrationResult.T_target_source` with `num_threads` parallelism; reconstruction runs `TriangleMesh.create_from_point_cloud_poisson` or `_ball_pivoting`.
- Receipt: each registration contributes a `Receipt.emitted` row through `ReceiptContributor` carrying the mode, fitness, inlier RMSE, and elapsed; the transform and mesh produce a geometry `GraduationReceipt` subject (`registration-transform`, `reconstructed-mesh`).
- Packages: `open3d` (`geometry.PointCloud`/`io.read_point_cloud`/`pipelines.registration.registration_icp`/`registration_generalized_icp`/`TriangleMesh.create_from_point_cloud_poisson`), `small_gicp` (`preprocess_points`/`align`/`GaussianVoxelMap`/`RegistrationResult.T_target_source`/`estimate_covariances`), runtime (`RuntimeRail`/`ReceiptContributor`/`LanePolicy`).
- Growth: a new registration algorithm is one `RegistrationMode` row plus one dispatch arm; a new reconstruction method is one `Reconstruction` branch; zero new surface, no parallel per-algorithm class family.
- Boundary: no IFC tessellation (that is `ifc-companion`), no durable store, no Rhino/GH mutation; a `get_icp`/`get_vgicp` family, a stringly-typed mode dispatch, and a weaker local registration reimplementation are the deleted forms. This owner is `SPIKE` on the companion floor.

```python signature
from enum import StrEnum

import numpy as np
import open3d as o3d
import small_gicp
from msgspec import Struct


class RegistrationMode(StrEnum):
    ICP = "icp"
    COLORED_ICP = "colored-icp"
    GENERALIZED_ICP = "generalized-icp"
    VGICP = "vgicp"


class RegistrationResult(Struct, frozen=True):
    mode: RegistrationMode
    transform: tuple[float, ...]
    fitness: float
    inlier_rmse: float


class ScanProcessing(Struct, frozen=True):
    voxel: float = 0.05
    max_correspondence: float = 0.1

    def register(self, source: o3d.geometry.PointCloud, target: o3d.geometry.PointCloud, mode: RegistrationMode) -> "RuntimeRail[RegistrationResult]":
        return boundary(f"scan.{mode}", lambda: self._dispatch(source, target, mode))

    def _dispatch(self, source: o3d.geometry.PointCloud, target: o3d.geometry.PointCloud, mode: RegistrationMode) -> RegistrationResult:
        match mode:
            case RegistrationMode.VGICP:
                result = small_gicp.align(np.asarray(target.points), np.asarray(source.points), downsampling_resolution=self.voxel)
                return RegistrationResult(mode, tuple(result.T_target_source.flatten()), 1.0 - result.error, result.error)
            case RegistrationMode.GENERALIZED_ICP:
                reg = o3d.pipelines.registration.registration_generalized_icp(source, target, self.max_correspondence)
                return RegistrationResult(mode, tuple(reg.transformation.flatten()), reg.fitness, reg.inlier_rmse)
            case _:
                reg = o3d.pipelines.registration.registration_icp(source, target, self.max_correspondence)
                return RegistrationResult(mode, tuple(reg.transformation.flatten()), reg.fitness, reg.inlier_rmse)
```

## [3]-[RESEARCH]

- [GICP_ALIGN_ARITY]: the `small_gicp.align` keyword arity (`downsampling_resolution`, voxel-map target vs cloud target) and the `RegistrationResult.error`-to-fitness mapping are verified against `.api/api-small_gicp.md`; the open3d `registration_generalized_icp` default estimation method confirms against `.api/api-open3d.md` on the cp312 companion interpreter.

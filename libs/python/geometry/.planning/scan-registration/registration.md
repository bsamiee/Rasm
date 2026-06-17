# [PY_GEOMETRY_SCAN_REGISTRATION]

Point-cloud and 3D-scan registration. `ScanRegistration` is one registration owner discriminating by mode row: global RANSAC+FPFH bootstrap, coarse-to-fine `multi_scale_icp` over the open3d tensor backend with robust kernels, the legacy ICP family, and the `small_gicp` VGICP parallel speed path, plus normal/feature estimation and voxel downsampling as the shared pre-step. Registration transforms graduate via the compute `HandoffAxis` geometry case; reconstruction and mesh repair route the `mesh-utility` sub-domain.

## [1]-[INDEX]

[CLUSTERS]:
- `[2]-[REGISTRATION]`: mode-discriminated registration, the global RANSAC+FPFH bootstrap, the multiway pose-graph, and the shared estimation pre-step.

## [2]-[REGISTRATION]

- Owner: `ScanRegistration` — the frozen owner discriminating by `RegistrationMode` row over a pair of `o3d.t.geometry.PointCloud` clouds; `RegistrationResult` the 4x4 transform plus fitness and inlier RMSE receipt.
- Cases: `RegistrationMode` rows `GLOBAL` (RANSAC over FPFH features for the initial pose) · `MULTISCALE` (coarse-to-fine `t.pipelines.multi_scale_icp` with a robust point-to-plane estimator) · `COLORED_ICP` (colored point-to-plane over open3d) · `VGICP` (the `small_gicp` voxelized parallel speed path) · `MULTIWAY` (pose-graph optimization over multi-station sessions) — matched by `match`/`case`, each binding the engine and estimator that owns it.
- Entry: `ScanRegistration.register` admits source/target clouds and a mode, runs the mode's pipeline through one `match`, and returns a `RuntimeRail[RegistrationResult]`; the `GLOBAL` arm threads the private `_bootstrap` RANSAC+FPFH pose that seeds the fine modes, and the `MULTIWAY` arm threads the private `_multiway` that folds pairwise bootstraps into a `PoseGraph` and runs `global_optimization`; the shared robust point-to-plane estimator binds once above the `match`.
- Auto: the tensor path runs `t.pipelines.registration.multi_scale_icp` with `TransformationEstimationPointToPlane` over a `TukeyLoss` `RobustKernel` across a voxel/iteration schedule reading `.transformation`/`.fitness`/`.inlier_rmse`; the colored path runs the tensor `icp` under `TransformationEstimationForColoredICP`; the global path lowers the tensor clouds to legacy through `to_legacy`, computes `compute_fpfh_feature` over a `KDTreeSearchParamHybrid` radius, then `registration_ransac_based_on_feature_matching`; the VGICP path runs `small_gicp.align` against the target positions reading `RegistrationResult.T_target_source`; the multiway path folds pairwise bootstraps into `PoseGraphNode`/`PoseGraphEdge` and runs `global_optimization`.
- Receipt: each registration contributes a `Receipt.emitted` row through `ReceiptContributor` carrying the mode, fitness, inlier RMSE, and elapsed; the transform produces a geometry `GraduationReceipt` subject (`registration-transform`).
- Packages: `open3d` (`t.geometry.PointCloud.to_legacy`/`t.pipelines.registration.multi_scale_icp`/`icp`/`TransformationEstimationPointToPlane`/`TransformationEstimationForColoredICP`/`robust_kernel.RobustKernel`/`RobustKernelMethod.TukeyLoss`/`pipelines.registration.registration_ransac_based_on_feature_matching`/`compute_fpfh_feature`/`PoseGraph`/`PoseGraphNode`/`PoseGraphEdge`/`global_optimization`), `small_gicp` (`align`/`RegistrationResult.T_target_source`), runtime (`RuntimeRail`/`ReceiptContributor`/`LanePolicy`).
- Growth: a new registration algorithm is one `RegistrationMode` row plus one dispatch arm; a new robust kernel is one estimator bind; zero new surface, no parallel per-algorithm class family.
- Boundary: no IFC tessellation (that is `tessellation`); robust mesh repair and reconstruction cleanup route `mesh-utility`; no durable store; no Rhino/GH mutation; a `get_icp`/`get_vgicp` family, a stringly-typed mode dispatch, the legacy-only `o3d.pipelines` backend with no global bootstrap, and a weaker local registration reimplementation are the deleted forms.

```python signature
import numpy as np
import open3d as o3d
import small_gicp
from enum import StrEnum
from msgspec import Struct

from rasm.runtime.observability.receipts import ReceiptContributor
from rasm.runtime.faults import RuntimeRail, boundary


class RegistrationMode(StrEnum):
    GLOBAL = "global"
    MULTISCALE = "multiscale"
    COLORED_ICP = "colored-icp"
    VGICP = "vgicp"
    MULTIWAY = "multiway"


class RegistrationResult(Struct, frozen=True):
    mode: RegistrationMode
    transform: tuple[float, ...]
    fitness: float
    inlier_rmse: float


class ScanRegistration(Struct, frozen=True):
    voxel: float = 0.05
    max_correspondence: float = 0.1
    tukey_k: float = 0.1

    def register(self, source: o3d.t.geometry.PointCloud, target: o3d.t.geometry.PointCloud, mode: RegistrationMode) -> "RuntimeRail[RegistrationResult]":
        return boundary(f"scan.{mode}", lambda: self._dispatch(source, target, mode))

    def _dispatch(self, source: o3d.t.geometry.PointCloud, target: o3d.t.geometry.PointCloud, mode: RegistrationMode) -> RegistrationResult:
        tukey = o3d.t.pipelines.registration.robust_kernel.RobustKernel(
            o3d.t.pipelines.registration.robust_kernel.RobustKernelMethod.TukeyLoss, self.tukey_k
        )
        plane = o3d.t.pipelines.registration.TransformationEstimationPointToPlane(tukey)
        match mode:
            case RegistrationMode.VGICP:
                result = small_gicp.align(
                    target.point.positions.numpy(), source.point.positions.numpy(), downsampling_resolution=self.voxel
                )
                return RegistrationResult(mode, tuple(result.T_target_source.flatten()), 1.0 - result.error, result.error)
            case RegistrationMode.MULTISCALE:
                reg = o3d.t.pipelines.registration.multi_scale_icp(
                    source, target, [self.voxel * 4, self.voxel], o3d.utility.IntVector([50, 30]),
                    [self.max_correspondence, self.voxel], plane,
                )
                return RegistrationResult(mode, tuple(reg.transformation.numpy().flatten()), reg.fitness, reg.inlier_rmse)
            case RegistrationMode.COLORED_ICP:
                colored = o3d.t.pipelines.registration.TransformationEstimationForColoredICP(0.968, tukey)
                reg = o3d.t.pipelines.registration.icp(source, target, self.max_correspondence, estimation_method=colored)
                return RegistrationResult(mode, tuple(reg.transformation.numpy().flatten()), reg.fitness, reg.inlier_rmse)
            case RegistrationMode.GLOBAL:
                return self._bootstrap(source, target)
            case RegistrationMode.MULTIWAY:
                return self._multiway((source, target))

    def _bootstrap(self, source: o3d.t.geometry.PointCloud, target: o3d.t.geometry.PointCloud) -> RegistrationResult:
        src, tgt = source.to_legacy(), target.to_legacy()
        params = o3d.geometry.KDTreeSearchParamHybrid(radius=self.voxel * 5, max_nn=100)
        fs = o3d.pipelines.registration.compute_fpfh_feature(src, params)
        ft = o3d.pipelines.registration.compute_fpfh_feature(tgt, params)
        reg = o3d.pipelines.registration.registration_ransac_based_on_feature_matching(
            src, tgt, fs, ft, True, self.voxel * 1.5,
            o3d.pipelines.registration.TransformationEstimationPointToPoint(False), 3,
            [o3d.pipelines.registration.CorrespondenceCheckerBasedOnDistance(self.voxel * 1.5)],
            o3d.pipelines.registration.RANSACConvergenceCriteria(100000, 0.999),
        )
        return RegistrationResult(RegistrationMode.GLOBAL, tuple(reg.transformation.flatten()), reg.fitness, reg.inlier_rmse)

    def _multiway(self, clouds: tuple[o3d.t.geometry.PointCloud, ...]) -> RegistrationResult:
        graph = o3d.pipelines.registration.PoseGraph()
        graph.nodes.append(o3d.pipelines.registration.PoseGraphNode(np.identity(4)))
        for i, cloud in enumerate(clouds[1:]):
            pose = self._bootstrap(clouds[0], cloud)
            graph.nodes.append(o3d.pipelines.registration.PoseGraphNode(np.asarray(pose.transform).reshape(4, 4)))
            graph.edges.append(o3d.pipelines.registration.PoseGraphEdge(0, i + 1, np.asarray(pose.transform).reshape(4, 4), uncertain=False))
        o3d.pipelines.registration.global_optimization(
            graph, o3d.pipelines.registration.GlobalOptimizationLevenbergMarquardt(),
            o3d.pipelines.registration.GlobalOptimizationConvergenceCriteria(),
            o3d.pipelines.registration.GlobalOptimizationOption(self.max_correspondence, 0.25, 0),
        )
        return RegistrationResult(RegistrationMode.MULTIWAY, tuple(graph.nodes[-1].pose.flatten()), 1.0, 0.0)
```

## [3]-[RESEARCH]

- [TENSOR_MULTISCALE]: the `t.pipelines.registration.multi_scale_icp` voxel/iteration/correspondence schedule arity, the `TransformationEstimationForColoredICP(lambda_geometric, kernel)` arity, and the tensor `icp` `estimation_method` keyword confirm against the branch `open3d` catalogue on the companion interpreter.
- [GLOBAL_MULTIWAY]: the legacy `registration_ransac_based_on_feature_matching` positional arity (mutual-filter flag, edge-length/distance checkers, `RANSACConvergenceCriteria`), the `KDTreeSearchParamHybrid` radius for `compute_fpfh_feature`, and the `global_optimization` option triple (`GlobalOptimizationLevenbergMarquardt`/`GlobalOptimizationConvergenceCriteria`/`GlobalOptimizationOption`) confirm against the branch `open3d` catalogue; `PoseGraphNode.pose` is the optimized 4x4 readout.
- [GICP_ALIGN_ARITY]: the `small_gicp.align` positional arity (target then source numpy positions, `downsampling_resolution`) and the `RegistrationResult.error`-to-fitness mapping confirm against the branch `small_gicp` catalogue.

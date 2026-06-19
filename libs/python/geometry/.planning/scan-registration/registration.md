# [PY_GEOMETRY_SCAN_REGISTRATION]

Point-cloud and 3D-scan registration. `ScanRegistration` is one registration owner discriminating by mode row: the `kiss_matcher` initialization-free global bootstrap (Faster-PFH keypoints, ROBIN outlier pruning, GNC solving — no initial pose), coarse-to-fine `multi_scale_icp` over the open3d tensor backend with robust kernels, colored point-to-plane ICP, the `small_gicp` VGICP parallel speed path, and multiway pose-graph optimization, with the robust point-to-plane estimator and voxel/correspondence schedule as the shared pre-step. The `kiss_matcher` `GLOBAL` solution seeds the fine modes and the multiway pairwise edges. Registration transforms graduate via the compute `HandoffAxis` geometry case; reconstruction and mesh repair route the `mesh-utility` sub-domain.

## [1]-[INDEX]

[CLUSTERS]:
- `[2]-[REGISTRATION]`: mode-discriminated registration, the global RANSAC+FPFH bootstrap, the multiway pose-graph, and the shared estimation pre-step.

## [2]-[REGISTRATION]

- Owner: `ScanRegistration` — the frozen owner discriminating by `RegistrationMode` row over a pair of `o3d.t.geometry.PointCloud` clouds; `RegistrationResult` the 4x4 transform plus fitness and inlier RMSE receipt.
- Cases: `RegistrationMode` rows `GLOBAL` (the `kiss_matcher` initialization-free Faster-PFH/ROBIN/GNC global pose) · `MULTISCALE` (coarse-to-fine `t.pipelines.multi_scale_icp` with a robust point-to-plane estimator) · `COLORED_ICP` (colored point-to-plane over open3d) · `VGICP` (the `small_gicp` voxelized parallel speed path) · `MULTIWAY` (pose-graph optimization over multi-station sessions) — matched by `match`/`assert_never`, each binding the engine and estimator that owns it.
- Entry: `ScanRegistration.register` admits source/target clouds and a mode, runs the mode's pipeline through one `match`, and returns a `RuntimeRail[RegistrationResult]`; the `GLOBAL` arm threads the private `_bootstrap` `kiss_matcher` pose (no initial pose required) that seeds the fine modes, and the `MULTIWAY` arm threads the private `_multiway` that folds pairwise `_bootstrap` solutions into a `PoseGraph` and runs `global_optimization`; the shared robust point-to-plane estimator binds once above the `match`.
- Auto: the tensor path runs `t.pipelines.registration.multi_scale_icp` with `TransformationEstimationPointToPlane` over a `TukeyLoss` `RobustKernel` across a voxel/iteration schedule reading `.transformation`/`.fitness`/`.inlier_rmse`; the colored path runs the tensor `icp` under `TransformationEstimationForColoredICP`; the global path runs `KISSMatcher.estimate` over the source/target positions, reads the `RegistrationSolution.rotation`/`translation` into a 4x4 transform, and folds `get_num_final_inliers` into the fitness; the VGICP path runs `small_gicp.align` against the target positions reading `RegistrationResult.T_target_source`/`.num_inliers`/`.error`, folding the inlier ratio into the fitness; the multiway path folds pairwise `kiss_matcher` bootstraps into `PoseGraphNode`/`PoseGraphEdge` and runs `global_optimization`.
- Receipt: each registration contributes an emitted-phase `Receipt.of` row through `ReceiptContributor` carrying the mode, fitness, inlier RMSE, and elapsed; the transform produces a geometry `GraduationReceipt` subject (`registration-transform`).
- Packages: `kiss_matcher` (`KISSMatcher`/`KISSMatcherConfig`/`estimate`/`RegistrationSolution.rotation`/`.translation`/`.valid`/`get_num_final_inliers`), `open3d` (`t.geometry.PointCloud.to_legacy`/`t.pipelines.registration.multi_scale_icp`/`icp`/`TransformationEstimationPointToPlane`/`TransformationEstimationForColoredICP`/`robust_kernel.RobustKernel`/`RobustKernelMethod.TukeyLoss`/`PoseGraph`/`PoseGraphNode`/`PoseGraphEdge`/`global_optimization`), `small_gicp` (`align`/`RegistrationResult.T_target_source`/`.num_inliers`/`.error`), runtime (`RuntimeRail`/`ReceiptContributor`/`LanePolicy`).
- Growth: a new registration algorithm is one `RegistrationMode` row plus one dispatch arm; a new robust kernel is one estimator bind; the `kiss_matcher` `use_quatro` degenerate-scene solver is one `KISSMatcherConfig` field; zero new surface, no parallel per-algorithm class family.
- Boundary: no IFC tessellation (that is `tessellation`); robust mesh repair and reconstruction cleanup route `mesh-utility`; scan-vs-model deviation routes the `deviation.md#DEVIATION` sibling on the registered pose; no durable store; no Rhino/GH mutation; a `get_icp`/`get_vgicp` family, a stringly-typed mode dispatch, the open3d RANSAC+FPFH global bootstrap kiss_matcher's initialization-free path supersedes, and a weaker local registration reimplementation are the deleted forms.

```python signature
import open3d as o3d
import small_gicp
import kiss_matcher
import numpy as np
from enum import StrEnum
from typing import assert_never
from msgspec import Struct

from rasm.runtime.receipts import ReceiptContributor
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
                src = source.point.positions.numpy()
                result = small_gicp.align(target.point.positions.numpy(), src, downsampling_resolution=self.voxel)
                fitness = result.num_inliers / max(len(src), 1)
                return RegistrationResult(mode, tuple(result.T_target_source.flatten()), fitness, result.error)
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
            case unreachable:
                assert_never(unreachable)

    def _bootstrap(self, source: o3d.t.geometry.PointCloud, target: o3d.t.geometry.PointCloud) -> RegistrationResult:
        matcher = kiss_matcher.KISSMatcher(kiss_matcher.KISSMatcherConfig(voxel_size=self.voxel))
        src = source.point.positions.numpy().astype(np.float64).T
        tgt = target.point.positions.numpy().astype(np.float64).T
        solution = matcher.estimate(src, tgt)
        transform = np.identity(4)
        transform[:3, :3] = np.asarray(solution.rotation)
        transform[:3, 3] = np.asarray(solution.translation)
        inliers = matcher.get_num_final_inliers()
        return RegistrationResult(RegistrationMode.GLOBAL, tuple(transform.flatten()), float(solution.valid) * inliers, 0.0)

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
- [GLOBAL_KISS_ARITY]: the `KISSMatcher.estimate(src, tgt)` array-overload dtype/shape (the catalogue's `float64 (3, n)` array form versus the `float32 (3, 1)` sequence form) and the `RegistrationSolution.rotation`/`.translation`/`.valid` readouts plus `get_num_final_inliers` confirm against the branch `kiss_matcher` catalogue on the cp312 companion; the `open3d` `global_optimization` option triple (`GlobalOptimizationLevenbergMarquardt`/`GlobalOptimizationConvergenceCriteria`/`GlobalOptimizationOption`) and `PoseGraphNode.pose` 4x4 readout confirm against the branch `open3d` catalogue.
- [GICP_ALIGN_ARITY]: the `small_gicp.align` positional arity (target then source numpy positions, `downsampling_resolution`) and the `RegistrationResult.num_inliers`/source-count inlier-ratio fitness confirm against the branch `small_gicp` catalogue.

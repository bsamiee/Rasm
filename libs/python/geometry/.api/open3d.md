# [PY_GEOMETRY_API_OPEN3D]

`open3d` supplies the point-cloud and 3D-scan processing surface for the geometry scan rail: a `geometry.PointCloud` and `geometry.TriangleMesh`, the `io` read/write functions, and the `pipelines.registration` module that drives downsampling, normal estimation, plane segmentation, ICP and feature-based registration, pose-graph optimization, and Poisson/ball-pivoting surface reconstruction. The package owner composes `io.read_point_cloud`, `PointCloud.voxel_down_sample`, and `registration.registration_icp` into the scan owner; it never re-implements ICP, FPFH features, or Poisson reconstruction open3d already owns. Its `registration.registration_fgr_based_on_feature_matching` Fast Global Registration path is the global-registration fallback the scan owner selects on a `>=3.13` interpreter where the primary `kiss-matcher` (admitted `python_version<'3.13'`, cp38-cp312 wheels) carries no wheel; both return a rigid transform that seeds the fine `small-gicp` refinement. (This fallback presumes a future open3d cp313+ wheel; `open3d 0.19.0` itself caps at cp312 — see the floor note.)

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `open3d`
- package: `open3d`
- import: `import open3d`
- owner: `geometry`
- rail: scan / global-registration-fallback
- installed: `0.19.0` (current PyPI release); license MIT, `Requires-Python>=3.8`, classifiers cap at Python 3.12; wheels cp38-cp312 ONLY (no cp313/cp314/cp315, no abi3) confirmed against the cached `open3d-0.19.0.dist-info` (`Tag: cp312-cp312-...`) and the PyPI release files => `python_version<'3.13'` companion-only. The `scan/registration.md`/`scan/reconstruction.md` design pages target the broader `python_version<'3.15'` band and select the open3d FGR arm at `>=3.13`: that band is the SHARED admission delta the design records as forward-looking, but it is reachable only when a future open3d release ships a cp313+ wheel — today's `0.19.0` admits a wheel on cp38-cp312 only, so the live marker the manifest carries is `; python_version<'3.13'`. The `assay api resolve open3d` pass returns `unsupported` (no source on the current cp315 environment), not an open3d wheel or interpreter fault
- entry points: none (library only)
- capability: point-cloud and mesh IO, voxel/uniform/random/farthest downsampling, normal and covariance estimation, statistical and radius outlier removal, plane and DBSCAN segmentation, point-to-point/plane/colored/generalized ICP, RANSAC and fast global registration, FPFH features, multiway pose-graph optimization, and Poisson/ball-pivoting/alpha-shape reconstruction

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: geometry roots (`open3d.geometry`)
- rail: scan

| [INDEX] | [SYMBOL]                       | [PACKAGE_ROLE]  | [CAPABILITY]                                             |
| :-----: | :----------------------------- | :-------------- | :------------------------------------------------------- |
|  [01]   | `geometry.PointCloud`          | point cloud     | points/colors/normals/covariances with sampling/filter   |
|  [02]   | `geometry.TriangleMesh`        | triangle mesh   | vertices/triangles with smoothing/decimation/reconstruct |
|  [03]   | `geometry.VoxelGrid`           | voxel grid      | occupancy voxelization of points or mesh                 |
|  [04]   | `geometry.Octree`              | octree          | adaptive spatial subdivision                             |
|  [05]   | `geometry.KDTreeFlann`         | spatial index   | KNN/radius/hybrid nearest-neighbor search                |
|  [06]   | `geometry.OrientedBoundingBox` | bounding volume | oriented and axis-aligned bounds                         |
|  [07]   | `geometry.LineSet`             | line set        | edges between points                                     |
|  [08]   | `geometry.RGBDImage`           | RGBD image      | color-plus-depth source for cloud creation               |

[PUBLIC_TYPE_SCOPE]: registration types (`open3d.pipelines.registration`)
- rail: scan

| [INDEX] | [SYMBOL]                                    | [PACKAGE_ROLE]      | [CAPABILITY]                          |
| :-----: | :------------------------------------------ | :------------------ | :------------------------------------ |
|  [01]   | `RegistrationResult`                        | registration result | transformation/fitness/inlier-rmse    |
|  [02]   | `ICPConvergenceCriteria`                    | convergence policy  | ICP iteration and tolerance limits    |
|  [03]   | `TransformationEstimationPointToPoint`      | estimator           | rigid point-to-point estimation       |
|  [04]   | `TransformationEstimationPointToPlane`      | estimator           | point-to-plane estimation             |
|  [05]   | `TransformationEstimationForGeneralizedICP` | estimator           | plane-to-plane generalized ICP        |
|  [06]   | `Feature`                                   | feature descriptor  | FPFH descriptor for global matching   |
|  [07]   | `PoseGraph`                                 | pose graph          | multiway registration node/edge graph |
|  [08]   | `RobustKernel`                              | robust loss         | Huber/Tukey/Cauchy/GM/L1/L2 weighting |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: cloud filter, segment, and reconstruct
- rail: scan

Methods operate on a `geometry.PointCloud` or `geometry.TriangleMesh`; `create_from_*` are static `TriangleMesh` constructors.

| [INDEX] | [SURFACE]                                            | [CALL_SHAPE]             | [CAPABILITY]                            |
| :-----: | :--------------------------------------------------- | :----------------------- | :-------------------------------------- |
|  [01]   | `PointCloud.voxel_down_sample`                       | voxel size               | voxel-grid downsampling                 |
|  [02]   | `PointCloud.farthest_point_down_sample`              | sample count             | farthest-point downsampling             |
|  [03]   | `PointCloud.estimate_normals`                        | search param             | normal estimation                       |
|  [04]   | `PointCloud.orient_normals_consistent_tangent_plane` | k-neighbors              | globally consistent normal orientation  |
|  [05]   | `PointCloud.remove_statistical_outlier`              | neighbors plus std-ratio | statistical outlier removal             |
|  [06]   | `PointCloud.remove_radius_outlier`                   | points plus radius       | radius outlier removal                  |
|  [07]   | `PointCloud.segment_plane`                           | distance plus ransac n   | dominant-plane RANSAC fit               |
|  [08]   | `PointCloud.cluster_dbscan`                          | eps plus min points      | density clustering                      |
|  [09]   | `PointCloud.compute_point_cloud_distance`            | target cloud             | nearest-neighbor distances              |
|  [10]   | `PointCloud.select_by_index`                         | index list               | subset extraction                       |
|  [11]   | `TriangleMesh.create_from_point_cloud_poisson`       | cloud plus depth         | Poisson surface reconstruction          |
|  [12]   | `TriangleMesh.create_from_point_cloud_ball_pivoting` | cloud plus radii         | ball-pivoting reconstruction            |
|  [13]   | `TriangleMesh.simplify_quadric_decimation`           | target triangle count    | quadric mesh decimation                 |
|  [14]   | `TriangleMesh.filter_smooth_taubin`                  | iteration count          | Taubin/Laplacian smoothing              |
|  [15]   | `TriangleMesh.create_from_point_cloud_alpha_shape`   | cloud plus alpha         | alpha-complex concave-hull surface      |
|  [16]   | `TriangleMesh.remove_vertices_by_mask`               | bool mask                | drop masked vertices (Poisson trim)     |
|  [17]   | `geometry.KDTreeSearchParamHybrid`                   | radius plus max-nn       | hybrid radius/knn normal-search param   |
|  [18]   | `utility.DoubleVector` / `utility.IntVector`         | python sequence          | typed vectors for radii/voxel schedules |

[ENTRYPOINT_SCOPE]: register and optimize
- rail: scan

Registration rows return a `RegistrationResult` (or `PoseGraph`); ICP rows take source, target, max correspondence distance, init transform, estimator, and criteria.

| [INDEX] | [SURFACE]                                                    | [CALL_SHAPE]                 | [CAPABILITY]                     |
| :-----: | :----------------------------------------------------------- | :--------------------------- | :------------------------------- |
|  [01]   | `registration.registration_icp`                              | source/target plus estimator | point-to-point/plane ICP         |
|  [02]   | `registration.registration_colored_icp`                      | source/target plus criteria  | color-aware ICP                  |
|  [03]   | `registration.registration_generalized_icp`                  | source/target plus criteria  | plane-to-plane generalized ICP   |
|  [04]   | `registration.registration_ransac_based_on_feature_matching` | source/target plus features  | RANSAC feature-based global fit  |
|  [05]   | `registration.registration_fgr_based_on_feature_matching`    | source/target plus features  | fast global registration         |
|  [06]   | `registration.compute_fpfh_feature`                          | cloud plus search param      | FPFH descriptor computation      |
|  [07]   | `registration.evaluate_registration`                         | source/target plus transform | fitness/inlier-rmse evaluation   |
|  [08]   | `registration.global_optimization`                           | pose graph plus options      | multiway pose-graph optimization |

[ENTRYPOINT_SCOPE]: tensor registration (`open3d.t.pipelines.registration`)
- rail: scan

The tensor backend operates on `t.geometry.PointCloud` and returns a tensor `RegistrationResult` with `.transformation`/`.fitness`/`.inlier_rmse`; estimators take a `robust_kernel.RobustKernel` for outlier downweighting, and `multi_scale_icp` runs the coarse-to-fine voxel/iteration/correspondence schedule.

| [INDEX] | [SURFACE]                                                                                                                        | [CALL_SHAPE]                    | [CAPABILITY]                          |
| :-----: | :------------------------------------------------------------------------------------------------------------------------------- | :------------------------------ | :------------------------------------ |
|  [01]   | `t.pipelines.registration.icp`                                                                                                   | source/target plus estimator    | single-scale tensor ICP               |
|  [02]   | `t.pipelines.registration.multi_scale_icp(source, target, voxel_sizes, criteria_list, max_correspondence_distances, estimation)` | coarse-to-fine schedule         | multi-scale tensor ICP                |
|  [03]   | `t.pipelines.registration.TransformationEstimationPointToPlane(kernel)`                                                          | optional `RobustKernel`         | point-to-plane estimator              |
|  [04]   | `t.pipelines.registration.TransformationEstimationForColoredICP(lambda_geometric, kernel)`                                       | geometric weight plus kernel    | colored point-to-plane estimator      |
|  [05]   | `t.pipelines.registration.robust_kernel.RobustKernel(method, scaling)`                                                           | `RobustKernelMethod` plus scale | Tukey/Huber/Cauchy/GM/L1/L2 weighting |
|  [06]   | `t.pipelines.registration.robust_kernel.RobustKernelMethod.TukeyLoss`                                                            | enum member                     | Tukey biweight loss kernel            |

[ENTRYPOINT_SCOPE]: input and output (`open3d.io`)
- rail: scan

| [INDEX] | [SURFACE]                        | [CALL_SHAPE]         | [CAPABILITY]                       |
| :-----: | :------------------------------- | :------------------- | :--------------------------------- |
|  [01]   | `io.read_point_cloud`            | filename plus format | read `PLY`/`PCD`/`XYZ`/`PTS` cloud |
|  [02]   | `io.write_point_cloud`           | filename plus cloud  | write a point cloud                |
|  [03]   | `io.read_point_cloud_from_bytes` | bytes plus format    | decode an in-memory cloud          |
|  [04]   | `io.read_triangle_mesh`          | filename             | read `PLY`/`OBJ`/`STL`/`OFF` mesh  |
|  [05]   | `io.write_triangle_mesh`         | filename plus mesh   | write a triangle mesh              |
|  [06]   | `io.read_pose_graph`             | filename             | read a pose graph                  |
|  [07]   | `io.read_feature`                | filename             | read an FPFH feature               |

## [04]-[IMPLEMENTATION_LAW]

[SCAN_PROCESSING]:
- import: `import open3d` at boundary scope only; module-level import is banned by the manifest import policy.
- cloud axis: one `PointCloud` owns points/colors/normals/covariances; downsampling is a method row (`voxel_down_sample`, `uniform_down_sample`, `farthest_point_down_sample`), never parallel cloud subclasses. `KDTreeSearchParamHybrid`/`KNN`/`Radius` parameterize normal estimation and feature search.
- registration axis: legacy `registration_icp` dispatches on its `TransformationEstimation*` argument (point-to-point, point-to-plane, generalized, colored), the estimation method an argument row, never a separate ICP function family; the tensor `t.pipelines.registration` backend mirrors this with `icp`/`multi_scale_icp` over a `t.geometry.PointCloud`, the `TransformationEstimationPointToPlane`/`TransformationEstimationForColoredICP` estimators each taking a `robust_kernel.RobustKernel(RobustKernelMethod.TukeyLoss, scale)` for outlier downweighting and `multi_scale_icp` carrying the coarse-to-fine voxel/iteration/correspondence schedule. Both backends return `RegistrationResult.transformation`/`.fitness`/`.inlier_rmse`; the scan owner composes the tensor backend for the multi-scale and colored arms and the legacy `pipelines.registration` `PoseGraph`/`global_optimization` for the multiway pose-graph.
- reconstruction axis: `TriangleMesh.create_from_point_cloud_poisson`/`ball_pivoting`/`alpha_shape` are the reconstruction rows; the algorithm choice is a static constructor, not a runtime mode flag.
- pipeline: `io.read_point_cloud` -> `voxel_down_sample` -> `estimate_normals` -> `compute_fpfh_feature` -> `registration_ransac_based_on_feature_matching` -> `registration_icp` refinement -> `create_from_point_cloud_poisson`.
- evidence: each registration captures fitness, inlier rmse, correspondence count, and transformation; each reconstruction captures input point count and output vertex/triangle count as a scan receipt. The `open3d.t` tensor API mirrors this surface for batched GPU work.
- fallback law: `registration_fgr_based_on_feature_matching` over `compute_fpfh_feature` descriptors is the Fast Global Registration global-registration arm the scan owner selects only when the primary `kiss-matcher` carries no wheel for the active interpreter. `kiss-matcher` is the initialization-free primary on the `python_version<'3.13'` band (cp38-cp312 wheels); on a `>=3.13` interpreter no `kiss-matcher` wheel resolves, so the `BootstrapEngine.OPEN3D_FGR` arm is selected — which presumes a future open3d cp313+ wheel, since `0.19.0` is itself cp38-cp312 only. Both arms yield a rigid transform seeding the `small-gicp` fine refinement, so the fallback is a per-interpreter selector row keyed on wheel availability, not a parallel registration owner.
- boundary: open3d owns point-cloud registration and reconstruction; triangular mesh exchange routes to `trimesh`/`meshio`, LAS/LAZ scan IO to `laspy`, E57 to `pye57`; live visualization stays outside the headless boundary.

## [05]-[LOCAL_ADMISSION]

[RAIL_LAW]:
- Package: `open3d`
- Owns: point-cloud/mesh IO, downsampling, normal estimation, outlier removal, segmentation, ICP and global registration, pose-graph optimization, and surface reconstruction
- Accept: 3D-scan processing and registration feeding the scan and geometry owners
- Reject: wrapper-renames of `registration_icp`/`read_point_cloud`; a hand-rolled ICP, FPFH, or Poisson kernel where open3d is admitted; an ICP function family over the `TransformationEstimation*` argument row; selecting the open3d FGR arm on a `python_version<'3.13'` interpreter where `kiss-matcher` carries a wheel and is the primary; identity minting the runtime owns

[CAPTURE_GAP]:
- floor: `open3d 0.19.0` ships cp38-cp312 wheels only (no cp313/cp314/cp315, no abi3), so it is a `python_version<'3.13'` companion-only package today; the compiled `cpu/pybind.cpython-3XX` core caps at cp312, and neither cp313, cp314, nor the cp315 project venv admits a wheel. The design's `<'3.15'` admission band and `>=3.13` FGR-fallback selection are forward-looking against a future open3d cp313+ wheel; until that wheel lands, the manifest marker is `; python_version<'3.13'` and the FGR arm only resolves on the cp38-cp312 lane. The project-venv `assay api resolve` returns `unsupported` (no source on the current environment)
- members: open3d's surface lives in the C++ pybind extension (no `.pyi` stubs); the module tree (`geometry`/`io`/`pipelines`/`utility`/`t`) is confirmed against `open3d/__init__.py` (`from open3d.cuda.pybind import core, camera, data, geometry, io, pipelines, utility, t`) in the cached cp312 distribution, and every documented `geometry`/`pipelines.registration`/`t.pipelines.registration`/`io` symbol is a known open3d 0.19 member, confirmed on a live cp312 companion-lane introspection pass, never invented beyond the stated surface

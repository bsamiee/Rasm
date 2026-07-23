# [PY_GEOMETRY_API_OPEN3D]

`open3d` owns the point-cloud and 3D-scan processing surface for the geometry scan rail: `geometry.PointCloud`/`geometry.TriangleMesh` values, `io` read/write, and the `pipelines.registration` module. Scan owners compose `io.read_point_cloud`, `PointCloud.voxel_down_sample`, and `registration.registration_icp`, never re-implementing the ICP, FPFH, or Poisson kernels open3d owns.

Its `registration_fgr_based_on_feature_matching` and `registration_ransac_based_on_feature_matching` paths are the coarse global-registration arm of the point-cloud registration union, each emitting a rigid transform that seeds the fine `small_gicp` refinement.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `open3d`
- package: `open3d` (MIT)
- import: `import open3d`
- owner: `geometry`
- rail: scan / coarse registration / reconstruction
- entry points: none (library only)

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: geometry roots (`open3d.geometry`)

| [INDEX] | [SYMBOL]                       | [TYPE_FAMILY]   | [CAPABILITY]                                             |
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

`RegistrationResult` carries `transformation`/`fitness`/`inlier_rmse`/`correspondence_set`; the multiway graph is built from `PoseGraphNode(pose)` and `PoseGraphEdge(source_node_id, target_node_id, transformation, information, uncertain, confidence)`.

| [INDEX] | [SYMBOL]                                    | [TYPE_FAMILY]       | [CAPABILITY]                                         |
| :-----: | :------------------------------------------ | :------------------ | :--------------------------------------------------- |
|  [01]   | `RegistrationResult`                        | registration result | the ICP/global-fit output carrier                    |
|  [02]   | `ICPConvergenceCriteria`                    | convergence policy  | ICP iteration and tolerance limits                   |
|  [03]   | `TransformationEstimationPointToPoint`      | estimator           | rigid point-to-point estimation                      |
|  [04]   | `TransformationEstimationPointToPlane`      | estimator           | point-to-plane estimation                            |
|  [05]   | `TransformationEstimationForGeneralizedICP` | estimator           | plane-to-plane generalized ICP                       |
|  [06]   | `Feature`                                   | feature descriptor  | FPFH descriptor for global matching                  |
|  [07]   | `PoseGraph`                                 | pose graph          | appendable `.nodes`/`.edges` multiway graph          |
|  [08]   | `PoseGraphNode`                             | pose-graph node     | one node exposing r/w `.pose` 4x4                    |
|  [09]   | `PoseGraphEdge`                             | pose-graph edge     | `information`/`uncertain`/`confidence`-weighted edge |
|  [10]   | `GlobalOptimizationLevenbergMarquardt`      | optimization method | LM solver (paired `GlobalOptimizationGaussNewton`)   |
|  [11]   | `GlobalOptimizationConvergenceCriteria`     | convergence policy  | multiway iteration/tolerance limits                  |
|  [12]   | `GlobalOptimizationOption`                  | optimization option | correspondence/prune/loop-closure/reference tuning   |
|  [13]   | `RobustKernel`                              | robust loss         | Huber/Tukey/Cauchy/GM/L1/L2 weighting                |

[PUBLIC_TYPE_SCOPE]: tensor roots (`open3d.t.geometry`, `open3d.t.pipelines.registration`)

`open3d.t` mirrors the `geometry` types over a device-resident `core.Tensor` buffer; `t.geometry.PointCloud` carries a `.point` `TensorMap` keyed by attribute (`positions`/`colors`/`normals`), and `to_legacy()` projects back to a `geometry.PointCloud` for the multiway and FGR arms the tensor API does not own.

| [INDEX] | [SYMBOL]                                      | [TYPE_FAMILY]      | [CAPABILITY]                                                   |
| :-----: | :-------------------------------------------- | :----------------- | :------------------------------------------------------------- |
|  [01]   | `t.geometry.PointCloud`                       | tensor point cloud | `.point` `TensorMap` attributes plus `to_legacy()` bridge      |
|  [02]   | `t.pipelines.registration.RegistrationResult` | tensor reg result  | `.transformation` `core.Tensor` plus `.fitness`/`.inlier_rmse` |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: cloud filter, segment, and reconstruct

Methods operate on a `geometry.PointCloud` or `geometry.TriangleMesh`; `create_from_*` are static `TriangleMesh` constructors.

| [INDEX] | [SURFACE]                                            | [ENTRY_FAMILY] | [CAPABILITY]                            |
| :-----: | :--------------------------------------------------- | :------------- | :-------------------------------------- |
|  [01]   | `PointCloud.voxel_down_sample`                       | sample         | voxel-grid downsampling                 |
|  [02]   | `PointCloud.farthest_point_down_sample`              | sample         | farthest-point downsampling             |
|  [03]   | `PointCloud.estimate_normals`                        | estimate       | normal estimation                       |
|  [04]   | `PointCloud.orient_normals_consistent_tangent_plane` | estimate       | globally consistent normal orientation  |
|  [05]   | `PointCloud.remove_statistical_outlier`              | filter         | statistical outlier removal             |
|  [06]   | `PointCloud.remove_radius_outlier`                   | filter         | radius outlier removal                  |
|  [07]   | `PointCloud.segment_plane`                           | segment        | dominant-plane RANSAC fit               |
|  [08]   | `PointCloud.cluster_dbscan`                          | segment        | density clustering                      |
|  [09]   | `PointCloud.compute_point_cloud_distance`            | query          | nearest-neighbor distances              |
|  [10]   | `PointCloud.select_by_index`                         | select         | subset extraction                       |
|  [11]   | `TriangleMesh.create_from_point_cloud_poisson`       | reconstruct    | Poisson surface reconstruction          |
|  [12]   | `TriangleMesh.create_from_point_cloud_ball_pivoting` | reconstruct    | ball-pivoting reconstruction            |
|  [13]   | `TriangleMesh.simplify_quadric_decimation`           | mesh op        | quadric mesh decimation                 |
|  [14]   | `TriangleMesh.filter_smooth_taubin`                  | mesh op        | Taubin/Laplacian smoothing              |
|  [15]   | `TriangleMesh.create_from_point_cloud_alpha_shape`   | reconstruct    | alpha-complex concave-hull surface      |
|  [16]   | `TriangleMesh.remove_vertices_by_mask`               | mesh op        | drop masked vertices (Poisson trim)     |
|  [17]   | `geometry.KDTreeSearchParamHybrid`                   | param          | hybrid radius/knn normal-search param   |
|  [18]   | `utility.DoubleVector` / `utility.IntVector`         | param          | typed vectors for radii/voxel schedules |

[ENTRYPOINT_SCOPE]: register and optimize

Registration rows return a `RegistrationResult` (or `PoseGraph`); ICP rows take source, target, max correspondence distance, init transform, estimator, and criteria.

| [INDEX] | [SURFACE]                                                    | [ENTRY_FAMILY] | [CAPABILITY]                     |
| :-----: | :----------------------------------------------------------- | :------------- | :------------------------------- |
|  [01]   | `registration.registration_icp`                              | icp            | point-to-point/plane ICP         |
|  [02]   | `registration.registration_colored_icp`                      | icp            | color-aware ICP                  |
|  [03]   | `registration.registration_generalized_icp`                  | icp            | plane-to-plane generalized ICP   |
|  [04]   | `registration.registration_ransac_based_on_feature_matching` | global         | RANSAC feature-based global fit  |
|  [05]   | `registration.registration_fgr_based_on_feature_matching`    | global         | fast global registration         |
|  [06]   | `registration.compute_fpfh_feature`                          | feature        | FPFH descriptor computation      |
|  [07]   | `registration.evaluate_registration`                         | evaluate       | fitness/inlier-rmse evaluation   |
|  [08]   | `registration.global_optimization`                           | multiway       | multiway pose-graph optimization |

[ENTRYPOINT_SCOPE]: tensor registration (`open3d.t.pipelines.registration`)

Surfaces elide the `t.pipelines.registration.` prefix and operate on `t.geometry.PointCloud`, returning a tensor `RegistrationResult`; estimators take a `robust_kernel.RobustKernel` for outlier downweighting, and `multi_scale_icp(source, target, voxel_sizes, criteria_list, max_correspondence_distances, estimation)` runs the coarse-to-fine schedule.

| [INDEX] | [SURFACE]                                                         | [ENTRY_FAMILY] | [CAPABILITY]                     |
| :-----: | :---------------------------------------------------------------- | :------------- | :------------------------------- |
|  [01]   | `icp`                                                             | icp            | single-scale tensor ICP          |
|  [02]   | `multi_scale_icp`                                                 | icp            | multi-scale tensor ICP           |
|  [03]   | `TransformationEstimationPointToPlane(kernel)`                    | estimator      | point-to-plane estimator         |
|  [04]   | `TransformationEstimationForColoredICP(lambda_geometric, kernel)` | estimator      | colored point-to-plane estimator |
|  [05]   | `robust_kernel.RobustKernel(method, scaling)`                     | kernel         | Tukey/Huber/Cauchy/GM/L1/L2 loss |
|  [06]   | `robust_kernel.RobustKernelMethod.TukeyLoss`                      | kernel         | Tukey biweight loss kernel       |

[ENTRYPOINT_SCOPE]: array-bridge accessors

Array-bridge accessors cross the open3d boundary into numpy: legacy `geometry` buffer properties are `numpy.asarray`-able zero-copy views, `RegistrationResult.correspondence_set` is a `utility.Vector2iVector` answering `len`, and the tensor `to_legacy()`/`.point.positions.numpy()`/`.transformation.numpy()` chain bridges the device-resident `t.geometry` backend to host numpy.

| [INDEX] | [SURFACE]                                     | [ENTRY_FAMILY] | [CAPABILITY]                                    |
| :-----: | :-------------------------------------------- | :------------- | :---------------------------------------------- |
|  [01]   | `geometry.PointCloud.points`                  | buffer         | Nx3 float64 position buffer (also `len`-able)   |
|  [02]   | `geometry.TriangleMesh.vertices`              | buffer         | Nx3 float64 vertex buffer (also `len`-able)     |
|  [03]   | `geometry.TriangleMesh.triangles`             | buffer         | Mx3 int32 face-index buffer (also `len`-able)   |
|  [04]   | `RegistrationResult.correspondence_set`       | count          | `Vector2iVector` inlier-pair count              |
|  [05]   | `t.geometry.PointCloud.to_legacy`             | bridge         | tensor->legacy `geometry.PointCloud` projection |
|  [06]   | `t.geometry.PointCloud.point.positions.numpy` | bridge         | `TensorMap` positions -> host Nx3 array         |
|  [07]   | `RegistrationResult.transformation.numpy`     | bridge         | tensor 4x4 transform -> host array              |

[ENTRYPOINT_SCOPE]: input and output (`open3d.io`)

| [INDEX] | [SURFACE]                        | [ENTRY_FAMILY] | [CAPABILITY]                       |
| :-----: | :------------------------------- | :------------- | :--------------------------------- |
|  [01]   | `io.read_point_cloud`            | read           | read `PLY`/`PCD`/`XYZ`/`PTS` cloud |
|  [02]   | `io.write_point_cloud`           | write          | write a point cloud                |
|  [03]   | `io.read_point_cloud_from_bytes` | read           | decode an in-memory cloud          |
|  [04]   | `io.read_triangle_mesh`          | read           | read `PLY`/`OBJ`/`STL`/`OFF` mesh  |
|  [05]   | `io.write_triangle_mesh`         | write          | write a triangle mesh              |
|  [06]   | `io.read_pose_graph`             | read           | read a pose graph                  |
|  [07]   | `io.read_feature`                | read           | read an FPFH feature               |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- cloud axis: one `PointCloud` owns points/colors/normals/covariances; downsampling is a method row (`voxel_down_sample`, `uniform_down_sample`, `farthest_point_down_sample`), never parallel subclasses, and `KDTreeSearchParamHybrid`/`KNN`/`Radius` parameterize normal and feature search.
- registration axis: `registration_icp` dispatches on its `TransformationEstimation*` argument (point-to-point, point-to-plane, generalized, colored), the estimator an argument row rather than an ICP function family.
- tensor axis: the `t.pipelines.registration` backend mirrors ICP with `icp`/`multi_scale_icp` over a `t.geometry.PointCloud`, each estimator taking a `robust_kernel.RobustKernel(RobustKernelMethod.TukeyLoss, scale)` and `multi_scale_icp` carrying the coarse-to-fine voxel/iteration/correspondence schedule. Both backends return `RegistrationResult.transformation`/`.fitness`/`.inlier_rmse`.
- multiway axis: the scan owner appends `PoseGraphNode(pose)` per cloud and `PoseGraphEdge(source_node_id, target_node_id, transformation, uncertain=...)` per pairwise solution into a `PoseGraph`, then `global_optimization(graph, GlobalOptimizationLevenbergMarquardt(), GlobalOptimizationConvergenceCriteria(), GlobalOptimizationOption(...))` returns `None`, mutating each node's `.pose` to the absolute transform in place.
- multiway trap: `GlobalOptimizationOption` positional order is `(max_correspondence_distance, edge_prune_threshold, preference_loop_closure, reference_node)`, so the loop-closure gain keyword-binds `preference_loop_closure` or it lands in `edge_prune_threshold` and disables loop closure.
- reconstruction axis: `create_from_point_cloud_poisson`/`ball_pivoting`/`alpha_shape` are static `TriangleMesh` constructors, the algorithm a constructor choice rather than a runtime mode flag.
- evidence: each registration captures fitness, inlier rmse, correspondence count, and transformation; each reconstruction captures input point count and output vertex/triangle count as a scan receipt.

[STACKING]:
- `small-gicp`(`.api/small-gicp.md`): `registration_fgr_based_on_feature_matching`/`registration_ransac_based_on_feature_matching` yield a `RegistrationResult.transformation` seeding `small_gicp.align(target, source, init_T_target_source=T)` for fine GICP/VGICP refinement.
- `kiss-matcher`(`.api/kiss-matcher.md`): open3d is the coarse global-registration arm when `kiss_matcher` carries no wheel; both emit a rigid transform seeding the fine `small_gicp` refinement.
- `probreg`(`.api/probreg.md`): `compute_fpfh_feature`/`Feature` feeds `probreg.FPFH(...)` and `registration_filterreg(feature_fn=...)`, lifting the GMM correspondence into open3d FPFH space over an open3d `PointCloud`.
- `trimesh`(`.api/trimesh.md`): triangle-mesh exchange crosses through `io.read_triangle_mesh`/`io.write_triangle_mesh`, and the estimated 4x4 transform applies through `trimesh.apply_transform` as the registration matrix the whole rail shares.
- `meshio`(`libs/python/.api/meshio.md`): a reconstructed `TriangleMesh` crosses as vertex/face arrays into a `meshio.Mesh` for FEM-format and solver-deck export.
- `pye57`(`.api/pye57.md`): a conditioned `read_scan` XYZ block enters as `PointCloud` points for registration and reconstruction.
- within-lib: the coarse-to-fine spine chains `io.read_point_cloud` -> `voxel_down_sample` -> `estimate_normals` -> `compute_fpfh_feature` -> `registration_ransac_based_on_feature_matching` -> `registration_icp` -> `create_from_point_cloud_poisson`; the `open3d.t` tensor backend mirrors it for batched device work, `to_legacy()` bridging to the multiway and FGR arms it does not own.

[LOCAL_ADMISSION]:
- import `open3d` at boundary scope only.
- open3d owns point-cloud registration and reconstruction; triangle-mesh exchange routes to `trimesh`/`meshio`, LAS/LAZ scan IO to `laspy`, E57 to `pye57`, and live visualization stays outside the headless boundary.

[RAIL_LAW]:
- Package: `open3d`
- Owns: point-cloud/mesh IO, downsampling, normal estimation, outlier removal, plane and DBSCAN segmentation, ICP and global registration, multiway pose-graph optimization, and surface reconstruction
- Accept: 3D-scan processing and registration feeding the scan and geometry owners
- Reject: hand-rolled ICP, FPFH, or Poisson reconstruction where open3d is admitted; an ICP function family over the `TransformationEstimation*` argument row; a per-algorithm reconstruction mode flag; identity minting the runtime owns

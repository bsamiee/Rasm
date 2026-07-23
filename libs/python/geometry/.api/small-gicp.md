# [PY_GEOMETRY_API_SMALL_GICP]

`small_gicp` owns the parallel fine point-cloud registration speed-path for the scan-processing rail: the polymorphic `align` entrypoint drives ICP, point-to-plane ICP, GICP, and VGICP to a `RegistrationResult` 4x4 transform, discriminating on the target shape and a `registration_type` string. It fills the fine multi-threaded refinement slot the coarse global `kiss_matcher`/`open3d` engines seed and the core `trimesh`/`manifold3d` spine cannot reach at scan scale.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `small-gicp`
- package: `small-gicp` (MIT)
- module: `small_gicp`
- rail: scan-processing / fine-registration-enrichment
- gate: native pybind11 build; a Forge-worker enrichment beside the core `trimesh`/`manifold3d` spine, which never depends on it

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: cloud, index, and result family

`PointCloud` carries `points`/`normals` as `Nx4` array views and `covs` as `list[4x4]`, with `point(i)`/`normal(i)`/`cov(i)` accessors and `size()`/`empty()`.

| [INDEX] | [SYMBOL]             | [TYPE_FAMILY]       | [CAPABILITY]                                                                  |
| :-----: | :------------------- | :------------------ | :---------------------------------------------------------------------------- |
|  [01]   | `PointCloud`         | point cloud         | `Nx4` points/normals + `covs` `list[4x4]` (accessors in lead)                 |
|  [02]   | `KdTree`             | spatial index       | parallel nearest-neighbor, KNN, and batch search                              |
|  [03]   | `RegistrationResult` | registration result | `T_target_source`, `converged`, `iterations`, `num_inliers`, `error`, `H`/`b` |

[PUBLIC_TYPE_SCOPE]: voxel map and factor family

Voxel maps share `insert`/`set_lru`/`size`/`voxel_points`, the optional `voxel_covs`/`voxel_normals` accessor varying by variant; factors share `linearize`.

| [INDEX] | [SYMBOL]                       | [TYPE_FAMILY] | [CAPABILITY]                                     |
| :-----: | :----------------------------- | :------------ | :----------------------------------------------- |
|  [01]   | `GaussianVoxelMap`             | voxel map     | VGICP target with `voxel_covs`                   |
|  [02]   | `IncrementalVoxelMap`          | voxel map     | LRU point voxels for streaming                   |
|  [03]   | `IncrementalVoxelMapCov`       | voxel map     | LRU voxels with `voxel_covs`                     |
|  [04]   | `IncrementalVoxelMapNormal`    | voxel map     | LRU voxels with `voxel_normals`                  |
|  [05]   | `IncrementalVoxelMapNormalCov` | voxel map     | LRU voxels with `voxel_normals` and `voxel_covs` |
|  [06]   | `ICPFactor`                    | factor        | point-to-point linearization                     |
|  [07]   | `PointToPlaneICPFactor`        | factor        | point-to-plane linearization                     |
|  [08]   | `GICPFactor`                   | factor        | plane-to-plane GICP linearization                |
|  [09]   | `DistanceRejector`             | rejector      | `set_max_distance` correspondence gate           |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: registration (`small_gicp.align`)

`align` overloads on the target shape and returns a `RegistrationResult`: the raw-array overload carries `registration_type` over the full `ICP`/`PLANE_ICP`/`GICP`/`VGICP` set with `voxel_resolution`/`downsampling_resolution` and preprocesses internally, the prepared-`PointCloud` overload over `ICP`/`PLANE_ICP`/`GICP` only, and the `GaussianVoxelMap` overload is VGICP-only with no `registration_type`.
- shared carry: `init_T_target_source`, `max_correspondence_distance`, `num_threads`, `max_iterations`, `rotation_epsilon`, `translation_epsilon`, `verbose`

| [INDEX] | [SURFACE]                                                | [SHAPE] | [CAPABILITY]                                      |
| :-----: | :------------------------------------------------------- | :------ | :------------------------------------------------ |
|  [01]   | `align(target_points, source_points, registration_type)` | static  | numpy `Nx3`/`Nx4` intake with internal preprocess |
|  [02]   | `align(target, source, target_tree)`                     | static  | aligns prepared `PointCloud` pair                 |
|  [03]   | `align(target_voxelmap, source)`                         | static  | VGICP against a `GaussianVoxelMap`                |

[ENTRYPOINT_SCOPE]: preprocessing and attribute estimation

`preprocess_points` fuses downsample, KdTree, and normal/covariance estimation in one parallel pass; it and `voxelgrid_sampling` overload on numpy-array or `PointCloud` input, and the `estimate_*` rows mutate the cloud in place.

| [INDEX] | [SURFACE]                                                          | [SHAPE] | [CAPABILITY]                            |
| :-----: | :----------------------------------------------------------------- | :------ | :-------------------------------------- |
|  [01]   | `preprocess_points(points, ...)`                                   | factory | downsample with KdTree and normals/covs |
|  [02]   | `voxelgrid_sampling(points, downsampling_resolution, num_threads)` | factory | voxel-grid downsample to `PointCloud`   |
|  [03]   | `estimate_normals(points, ...)`                                    | static  | in-place normal estimation              |
|  [04]   | `estimate_covariances(points, ...)`                                | static  | in-place covariance estimation          |
|  [05]   | `estimate_normals_covariances(points, ...)`                        | static  | in-place normals and covariances        |
|  [06]   | `read_ply(filename) -> PointCloud`                                 | factory | XYZ-only test PLY intake                |

[ENTRYPOINT_SCOPE]: search, voxel, and result accessors

`KdTree` search rows return an index and its squared distance; voxel-map rows build the VGICP target; `RegistrationResult` rows carry the optimization receipt.

| [INDEX] | [SURFACE]                                 | [SHAPE]  | [CAPABILITY]                         |
| :-----: | :---------------------------------------- | :------- | :----------------------------------- |
|  [01]   | `tree.nearest_neighbor_search(point)`     | instance | single nearest neighbor              |
|  [02]   | `tree.knn_search(point, k)`               | instance | k nearest neighbors                  |
|  [03]   | `tree.batch_nearest_neighbor_search(pts)` | instance | parallel batch nearest neighbor      |
|  [04]   | `tree.batch_knn_search(pts, k)`           | instance | parallel batch KNN                   |
|  [05]   | `voxelmap.insert(cloud, T)`               | instance | accumulate cloud into voxels         |
|  [06]   | `voxelmap.set_lru(horizon, clear_cycle)`  | instance | configure incremental LRU eviction   |
|  [07]   | `result.T_target_source`                  | property | 4x4 target-from-source transform     |
|  [08]   | `result.converged` / `result.iterations`  | property | convergence flag and iteration count |
|  [09]   | `result.num_inliers` / `result.error`     | property | inlier count and final error         |
|  [10]   | `result.H` / `result.b`                   | property | linearized Hessian and gradient      |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- import: `import small_gicp` at boundary scope only; module-level import is banned by the manifest import policy.
- registration axis: `align` is one polymorphic entrypoint discriminated by target shape (numpy arrays, prepared `PointCloud`, `GaussianVoxelMap`) and the `registration_type` string; the four algorithms are argument rows, never a function family.
- preprocessing axis: `preprocess_points` fuses voxel-grid downsampling, KdTree construction, and normal/covariance estimation in one parallel pass; `voxelgrid_sampling` and `estimate_normals`/`estimate_covariances` are the decomposed path when stages are reused.
- voxel axis: VGICP targets are `GaussianVoxelMap`; incremental streaming uses the `IncrementalVoxelMap*` variants whose optional `voxel_covs`/`voxel_normals` accessor matches the named attribute, and `set_lru` bounds memory for online mapping.
- determinism: `voxelgrid_sampling` with `num_threads>1` admits up to a 10% point-count increase over the single-thread result; discretized voxel coordinates stay within the 21-bit range `[-1048576, 1048575]`, and out-of-range points are dropped.
- evidence: each `align` returns the full `RegistrationResult` receipt, `H`/`b` exposing the final normal equations for chaining into a downstream solver.
- boundary: `small_gicp` owns fine multi-threaded GICP/VGICP registration; coarse feature-based global registration, FPFH, and surface reconstruction route to `open3d`, mesh-mesh ICP to `trimesh.registration`, and general PLY/scan IO to `open3d`/`laspy` rather than the test-only `read_ply`.

[STACKING]:
- `numpy`(`.api/numpy.md`): `align`/`preprocess_points` intake `float64` `Nx3`/`Nx4` buffers directly, `PointCloud.points`/`normals` expose `Nx4` array views, and `RegistrationResult.T_target_source`/`H`/`b` return numpy arrays the scan owner folds onward.
- `open3d`(`.api/open3d.md`): `registration_ransac_based_on_feature_matching`/`registration_fgr_based_on_feature_matching` yield a `RegistrationResult.transformation` seeding `align(target, source, init_T_target_source=T)` for fine GICP/VGICP refinement.
- `kiss_matcher`(`.api/kiss-matcher.md`): `RegistrationSolution.rotation`/`translation` compose the coarse 4x4 pose feeding `align(init_T_target_source=...)` — `small_gicp` is the fine arm of the two-stage registration union.
- `probreg`(`.api/probreg.md`): a rigid-arm `RigidTransformation` (`rot`/`t` as a 4x4) seeds `align(init_T_target_source=...)` for fine refinement following a coarse probabilistic alignment.
- geometry scan owner: composes `read_ply`/numpy intake, `preprocess_points`, and `align` into the scan-registration mode; `RegistrationResult.H`/`b` chain into a downstream solver as the registration receipt.

[LOCAL_ADMISSION]:
- small_gicp is admitted for the fine multi-threaded GICP/VGICP/ICP/point-to-plane refinement slot the coarse global `kiss_matcher`/`open3d` and probabilistic `probreg` engines seed.

[RAIL_LAW]:
- Package: `small-gicp`
- Owns: parallel GICP/VGICP/ICP/point-to-plane registration, voxel-grid downsampling, parallel KdTree search, normal and covariance estimation, incremental voxel maps, and linearized factors
- Accept: fine point-cloud registration feeding the scan-processing owner
- Reject: wrapper-renames of `align`/`preprocess_points`; a hand-rolled GICP linearization or parallel KdTree where small_gicp is admitted; an ICP/GICP/VGICP function family over the `registration_type` argument row; identity minting the runtime owns

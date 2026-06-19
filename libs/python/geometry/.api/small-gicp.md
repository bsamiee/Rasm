# [PY_GEOMETRY_API_SMALL_GICP]

`small_gicp` supplies the parallel fine point-cloud registration speed-path for the scan-processing rail: a `PointCloud` carrier with points/normals/covariances, a parallel `KdTree`, Gaussian and incremental voxel maps, per-correspondence factors, and the polymorphic `align` entrypoint that drives ICP, point-to-plane ICP, GICP, and VGICP to a `RegistrationResult` 4x4 transform. The package owner composes `read_ply`/numpy intake, `preprocess_points`, and `align` into the scan owner; it never re-implements the multi-threaded nearest-neighbor search, the GICP linearization, or the voxel-grid downsampler.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `small-gicp`
- package: `small-gicp`
- import: `import small_gicp`
- owner: `geometry`
- rail: scan-processing
- installed: `1.0.1` reflected via `python -c "import small_gicp"` on cp313
- entry points: none (library only)
- capability: multi-threaded GICP/VGICP/ICP/point-to-plane registration, voxel-grid downsampling, parallel KdTree KNN and batch search, normal and covariance estimation, Gaussian and incremental LRU voxel maps, and linearized normal-equation factors for chaining

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: cloud, index, and result family
- rail: scan-processing

| [INDEX] | [SYMBOL]             | [TYPE_FAMILY]       | [CAPABILITY]                                           |
| :-----: | :------------------- | :------------------ | :----------------------------------------------------- |
|  [01]   | `PointCloud`         | point cloud         | `points`/`normals`/`covs` arrays plus per-index access |
|  [02]   | `KdTree`             | spatial index       | parallel nearest-neighbor, KNN, and batch search       |
|  [03]   | `RegistrationResult` | registration result | transform, convergence, inliers, and Hessian/grad      |

[PUBLIC_TYPE_SCOPE]: voxel map and factor family
- rail: scan-processing

Voxel maps share `insert`/`set_lru`/`size`/`voxel_points`; the optional attribute accessor varies by variant. Factors share `linearize`.

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
- rail: scan-processing

`align` is overloaded on the target shape and returns a `RegistrationResult`; `registration_type` is one of `ICP`, `PLANE_ICP`, `GICP`, `VGICP`. The raw-array overload additionally carries `voxel_resolution=1.0` and `downsampling_resolution=0.25` (it preprocesses internally). All overloads share `init_T_target_source=eye(4)`, `max_correspondence_distance=1.0`, `num_threads=1`, `max_iterations=20`, `rotation_epsilon`, `translation_epsilon`, and `verbose`.

| [INDEX] | [SURFACE]                                                       | [ENTRY_FAMILY] | [CAPABILITY]                                      |
| :-----: | :-------------------------------------------------------------- | :------------- | :------------------------------------------------ |
|  [01]   | `align(target_points, source_points, registration_type='GICP')` | raw arrays     | numpy `Nx3`/`Nx4` intake plus internal preprocess |
|  [02]   | `align(target, source, target_tree=None)`                       | preprocessed   | aligns prepared `PointCloud` pair                 |
|  [03]   | `align(target_voxelmap, source)`                                | voxel target   | VGICP against a `GaussianVoxelMap`                |

[ENTRYPOINT_SCOPE]: preprocessing and attribute estimation
- rail: scan-processing

`preprocess_points`/`voxelgrid_sampling` are overloaded on numpy array or `PointCloud` input; estimation rows mutate the cloud in place.

| [INDEX] | [SURFACE]                                                                                  | [ENTRY_FAMILY] | [CAPABILITY]                             |
| :-----: | :----------------------------------------------------------------------------------------- | :------------- | :--------------------------------------- |
|  [01]   | `preprocess_points(points, downsampling_resolution=0.25, num_neighbors=10, num_threads=1)` | preprocess     | downsample plus KdTree plus normals/covs |
|  [02]   | `voxelgrid_sampling(points, downsampling_resolution, num_threads=1)`                       | downsample     | voxel-grid downsample to `PointCloud`    |
|  [03]   | `estimate_normals(points, tree=None, num_neighbors=20, num_threads=1)`                     | estimate       | in-place normal estimation               |
|  [04]   | `estimate_covariances(points, tree=None, num_neighbors=20, num_threads=1)`                 | estimate       | in-place covariance estimation           |
|  [05]   | `estimate_normals_covariances(points, tree=None, num_neighbors=20, num_threads=1)`         | estimate       | in-place normals and covariances         |
|  [06]   | `read_ply(filename) -> PointCloud`                                                         | read           | XYZ-only test PLY intake                 |

[ENTRYPOINT_SCOPE]: search, voxel, and result accessors
- rail: scan-processing

`KdTree` search rows return index plus squared distance; voxel-map rows build the VGICP target; `RegistrationResult` rows carry the optimization receipt.

| [INDEX] | [SURFACE]                                 | [ENTRY_FAMILY] | [CAPABILITY]                         |
| :-----: | :---------------------------------------- | :------------- | :----------------------------------- |
|  [01]   | `tree.nearest_neighbor_search(point)`     | search         | single nearest neighbor              |
|  [02]   | `tree.knn_search(point, k)`               | search         | k nearest neighbors                  |
|  [03]   | `tree.batch_nearest_neighbor_search(pts)` | search         | parallel batch nearest neighbor      |
|  [04]   | `tree.batch_knn_search(pts, k)`           | search         | parallel batch KNN                   |
|  [05]   | `voxelmap.insert(cloud, T=eye(4))`        | voxel          | accumulate cloud into voxels         |
|  [06]   | `voxelmap.set_lru(horizon, clear_cycle)`  | voxel          | configure incremental LRU eviction   |
|  [07]   | `result.T_target_source`                  | result         | 4x4 target-from-source transform     |
|  [08]   | `result.converged` / `result.iterations`  | result         | convergence flag and iteration count |
|  [09]   | `result.num_inliers` / `result.error`     | result         | inlier count and final error         |
|  [10]   | `result.H` / `result.b`                   | result         | linearized Hessian and gradient      |

## [04]-[IMPLEMENTATION_LAW]

[SCAN_REGISTRATION]:
- import: `import small_gicp` at boundary scope only; module-level import is banned by the manifest import policy.
- registration axis: `align` is one polymorphic entrypoint discriminated by target shape (numpy arrays, prepared `PointCloud`, or `GaussianVoxelMap`) and by the `registration_type` string; ICP, point-to-plane, GICP, and VGICP are argument rows, never a function family.
- preprocessing axis: `preprocess_points` fuses voxel-grid downsampling, KdTree construction, and normal/covariance estimation in one parallel pass; the raw `voxelgrid_sampling` plus `estimate_normals`/`estimate_covariances` rows are the decomposed path when stages are reused.
- voxel axis: VGICP targets are `GaussianVoxelMap`; incremental streaming uses the `IncrementalVoxelMap*` variants whose optional `voxel_covs`/`voxel_normals` accessors match the named attribute, and `set_lru` bounds memory for online mapping.
- determinism: `voxelgrid_sampling` with `num_threads>1` admits up to a 10% point-count deviation from the single-thread result; discretized voxel coordinates must stay within the 21-bit range `[-1048576, 1048575]`, and out-of-range points are dropped.
- evidence: each `align` captures `T_target_source`, `converged`, `iterations`, `num_inliers`, and `error`; `H`/`b` expose the final normal equations for chaining into a downstream solver as a registration receipt.
- boundary: small_gicp owns fine multi-threaded GICP/VGICP registration; coarse feature-based global registration, FPFH, and surface reconstruction route to `open3d`, mesh-mesh ICP to `trimesh.registration`, and general PLY/scan IO to `open3d`/`laspy` rather than the test-only `read_ply`.

## [05]-[LOCAL_ADMISSION]

[RAIL_LAW]:
- Package: `small-gicp`
- Owns: parallel GICP/VGICP/ICP/point-to-plane registration, voxel-grid downsampling, parallel KdTree search, normal and covariance estimation, incremental voxel maps, and linearized factors
- Accept: fine point-cloud registration feeding the scan-processing owner
- Reject: wrapper-renames of `align`/`preprocess_points`; a hand-rolled GICP linearization or parallel KdTree where small_gicp is admitted; an ICP/GICP/VGICP function family over the `registration_type` argument row; identity minting the runtime owns

[CAPTURE_GAP]:
- floor: `small-gicp` is an undeclared candidate package; `1.0.1` ships cp310-cp314 wheels (no abi3, no cp315 load path), so reflection runs on a cp313 companion interpreter with numpy while the `>=3.15` project venv carries no wheel and the project-venv `assay api query` resolves no source there
- members: verified by introspection against the installed cp313 distribution; every documented class, accessor, and overloaded entrypoint resolves against the live pybind signatures — no phantom

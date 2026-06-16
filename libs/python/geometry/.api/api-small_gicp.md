# [PY_GEOMETRY_API_SMALL_GICP]

`small_gicp` API capture placeholder for `geometry`.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `small-gicp`
- package: `small-gicp`
- import: `small_gicp`
- owner: `geometry`
- rail: scan-processing
- capability: parallel GICP/VGICP fine point-cloud registration speed-path under ScanProcessing

## [2]-[CAPTURE]

[PUBLIC_TYPES]:
- `small_gicp.PointCloud` — `points`, `normals`, `covs`, `point`, `normal`, `cov`, `size`, `empty`
- `small_gicp.KdTree` — `nearest_neighbor_search`, `knn_search`, `batch_nearest_neighbor_search`, `batch_knn_search`
- `small_gicp.RegistrationResult` — `T_target_source`, `converged`, `iterations`, `num_inliers`, `H`, `b`, `error`
- voxel maps: `GaussianVoxelMap`, `IncrementalVoxelMap`, `IncrementalVoxelMapCov`, `IncrementalVoxelMapNormal`, `IncrementalVoxelMapNormalCov` — each `insert`, `set_lru`, `size`, `voxel_points`; the optional-attribute accessors are variant-specific: `voxel_covs` on `GaussianVoxelMap`/`IncrementalVoxelMapCov`/`IncrementalVoxelMapNormalCov`, `voxel_normals` on `IncrementalVoxelMapNormal`/`IncrementalVoxelMapNormalCov`
- factors/rejector: `ICPFactor`, `GICPFactor`, `PointToPlaneICPFactor` (`linearize`), `DistanceRejector` (`set_max_distance`)

[ENTRYPOINTS]:
- `small_gicp.align(*args, **kwargs)` — ICP/GICP/VGICP registration entrypoint (point clouds or voxel map target)
- `small_gicp.preprocess_points(*args, **kwargs)` — downsample + KdTree + normals/covariances build
- `small_gicp.voxelgrid_sampling(*args, **kwargs)` — voxel-grid downsample
- `small_gicp.estimate_normals(points, tree=None, num_neighbors=20, num_threads=1) -> None`
- `small_gicp.estimate_covariances(points, tree=None, num_neighbors=20, num_threads=1) -> None`
- `small_gicp.estimate_normals_covariances(...)`, `small_gicp.read_ply(filename) -> PointCloud`

[IMPLEMENTATION_LAW]:
- Speed-path rail: `read_ply`/numpy `PointCloud` -> `preprocess_points` (or `voxelgrid_sampling` + `estimate_covariances`) -> `align` against a `GaussianVoxelMap` (VGICP) or target `PointCloud` (GICP) -> `RegistrationResult.T_target_source` 4x4, with `num_threads` parallelism; `H`/`b` expose the linearized normal equations for chaining.

## [3]-[LOCAL_ADMISSION]

[RAIL_LAW]:
- Package: `small-gicp`
- Owns: parallel GICP/VGICP fine point-cloud registration speed-path under ScanProcessing
- Accept: companion-floor capture on a `python_version<'3.13'` interpreter
- Reject: wrapper-renames and weaker local reimplementation

[CAPTURE_GAP]:
- floor: companion interpreter `python_version<'3.13'`; `1.0.1` ships cp310-cp314 tags (no abi3 wheel, no cp315 load path)
- state: `small-gicp==1.0.1` installs and reflects on a cp312 companion interpreter (numpy required); the `>=3.15` project venv carries no cp315 wheel, so the project-venv `assay api query` resolves no source there
- members: verified by introspection against the installed cp312 distribution; every documented member resolves, no phantom — the variant-specific voxel-map accessors above were corrected against the live class surfaces

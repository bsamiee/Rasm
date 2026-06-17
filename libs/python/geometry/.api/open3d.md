# [PY_GEOMETRY_API_OPEN3D]

`open3d` API capture placeholder for `geometry`.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `open3d`
- package: `open3d`
- import: `open3d`
- owner: `geometry`
- rail: scan-processing
- capability: point-cloud/3D-scan ICP registration, surface reconstruction, mesh processing

## [2]-[CAPTURE]

[PUBLIC_TYPES]:
- `open3d.geometry.PointCloud` — `points`, `colors`, `normals`, `covariances`; `voxel_down_sample`, `uniform_down_sample`, `random_down_sample`, `farthest_point_down_sample`, `estimate_normals`, `estimate_covariances`, `orient_normals_consistent_tangent_plane`, `remove_statistical_outlier`, `remove_radius_outlier`, `segment_plane`, `cluster_dbscan`, `compute_point_cloud_distance`, `compute_convex_hull`, `select_by_index`, `crop`, `transform`
- `open3d.geometry.TriangleMesh` — `vertices`, `triangles`, `vertex_normals`, `triangle_normals`, `vertex_colors`; `compute_vertex_normals`, `simplify_quadric_decimation`, `simplify_vertex_clustering`, `filter_smooth_taubin`/`_laplacian`/`_simple`, `subdivide_loop`/`_midpoint`, `remove_duplicated_vertices`, `remove_non_manifold_edges`, `merge_close_vertices`, `is_watertight`, `is_edge_manifold`, `is_vertex_manifold`, `get_volume`, `get_surface_area`, `sample_points_poisson_disk`, `sample_points_uniformly`; `create_from_point_cloud_poisson`, `create_from_point_cloud_ball_pivoting`, `create_from_point_cloud_alpha_shape`, `create_box`/`sphere`/`cylinder`/`cone`/`torus`/`coordinate_frame`
- `open3d.geometry`: `VoxelGrid`, `Octree`, `KDTreeFlann`, `AxisAlignedBoundingBox`, `OrientedBoundingBox`, `LineSet`, `RGBDImage`, `Image`, `TetraMesh`, `HalfEdgeTriangleMesh`
- `open3d.pipelines.registration`: `RegistrationResult`, `ICPConvergenceCriteria`, `RANSACConvergenceCriteria`, `Feature`, `PoseGraph`/`PoseGraphNode`/`PoseGraphEdge`, `TransformationEstimationPointToPoint`, `TransformationEstimationPointToPlane`, `TransformationEstimationForColoredICP`, `TransformationEstimationForGeneralizedICP`, robust kernels (`HuberLoss`, `TukeyLoss`, `CauchyLoss`, `GMLoss`, `L1Loss`, `L2Loss`), correspondence checkers

[ENTRYPOINTS]:
- `open3d.pipelines.registration.registration_icp(source, target, max_correspondence_distance, init=eye(4), estimation_method=TransformationEstimationPointToPoint(), criteria=ICPConvergenceCriteria(...)) -> RegistrationResult`
- `registration_colored_icp`, `registration_generalized_icp`, `registration_ransac_based_on_feature_matching`, `registration_ransac_based_on_correspondence`, `registration_fgr_based_on_feature_matching`, `registration_fgr_based_on_correspondence`, `compute_fpfh_feature`, `evaluate_registration`, `global_optimization`, `get_information_matrix_from_point_clouds`
- `open3d.io.read_point_cloud`, `write_point_cloud`, `read_point_cloud_from_bytes`, `write_point_cloud_to_bytes`, `read_triangle_mesh`, `write_triangle_mesh`, `read_triangle_model`, `read_voxel_grid`, `read_feature`/`write_feature`, `read_pose_graph`/`write_pose_graph`

[IMPLEMENTATION_LAW]:
- Top-level namespaces: `geometry`, `pipelines`, `io`, `core`, `t` (tensor API), `camera`, `data`, `utility`, `visualization`, `ml`.
- ScanProcessing rail: `io.read_point_cloud` -> `voxel_down_sample` -> `estimate_normals` -> `pipelines.registration.registration_icp`/`registration_generalized_icp` -> `RegistrationResult.transformation`/`.fitness`/`.inlier_rmse`, then `TriangleMesh.create_from_point_cloud_poisson` for reconstruction.

## [3]-[LOCAL_ADMISSION]

[RAIL_LAW]:
- Package: `open3d`
- Owns: point-cloud/3D-scan ICP registration, surface reconstruction, mesh processing
- Accept: companion-floor capture on a `python_version<'3.13'` interpreter
- Reject: wrapper-renames and weaker local reimplementation

[CAPTURE_GAP]:
- floor: companion interpreter `python_version<'3.13'`; compiled core caps at cp312, no cp315 wheel
- state: `open3d==0.19.0` installs and reflects on a cp312 companion interpreter; the `>=3.15` project venv carries no cp315 wheel, so the project-venv `assay api query` resolves no source there
- members: verified by introspection against the installed cp312 distribution; every documented type, method, and entrypoint resolves — no phantom

# [PY_GEOMETRY_API_KISS_MATCHER]

`kiss_matcher` supplies the global, initialization-free point-cloud registration path for the scan-processing rail: a `KISSMatcher` estimator that extracts Faster-PFH keypoints, matches correspondences, prunes outliers through ROBIN plus a graduated-non-convexity solver, and returns a `RegistrationSolution` rigid transform without an initial pose. A `KISSMatcherConfig` carries the voxel, normal, FPFH, and noise-bound parameters, and the matcher exposes per-stage timing and inlier counts as the registration receipt. The package owner composes `estimate` (or `match` plus `prune_and_solve`) into the global registration mode that seeds the fine `small_gicp` refinement; it never re-implements the keypoint extraction, the graph-theoretic outlier rejection, or the GNC pose solver.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `kiss-matcher`
- package: `kiss-matcher`
- import: `import kiss_matcher` (a thin façade that re-exports every public symbol from the compiled `kiss_matcher._kiss_matcher` pybind11 backend)
- owner: `geometry`
- rail: scan-processing / global-registration
- installed: `1.0.2`
- entry points: none (library only)
- members: introspected against the installed cp312 distribution and the `kiss_matcher_pybind.cpp` binding source; the `KISSMatcher`/`KISSMatcherConfig`/`RegistrationSolution` classes, the `def_readwrite` config fields, the overloaded `match` and the `estimate`/`solve`/`prune_and_solve` entries, and the stage/receipt accessors resolve against the live pybind11 signatures — no phantom
- capability: initialization-free global rigid registration, Faster-PFH keypoint extraction, correspondence matching, ROBIN-based outlier pruning, graduated-non-convexity and Quatro pose solving, and per-stage timing plus inlier receipts

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: estimator, config, and result family
- rail: global-registration

`KISSMatcher` is the single estimator; `KISSMatcherConfig` is the parameter carrier; `RegistrationSolution` is the immutable transform receipt.

| [INDEX] | [SYMBOL]               | [TYPE_FAMILY]       | [CAPABILITY]                                           |
| :-----: | :--------------------- | :------------------ | :----------------------------------------------------- |
|  [01]   | `KISSMatcher`          | global matcher      | end-to-end global registration plus stage accessors    |
|  [02]   | `KISSMatcherConfig`    | parameter carrier   | voxel/normal/FPFH/noise-bound and solver-mode settings |
|  [03]   | `RegistrationSolution` | registration result | `rotation`, `translation`, `valid` rigid transform     |

[PUBLIC_TYPE_SCOPE]: `KISSMatcherConfig` fields
- rail: global-registration

Construct with `KISSMatcherConfig(voxel_size=0.3, use_voxel_sampling=True, use_quatro=False, thr_linearity=1.0, num_max_corr=5000, normal_r_gain=3.0, fpfh_r_gain=5.0, robin_noise_bound_gain=1.0, solver_noise_bound_gain=0.75, enable_noise_bound_clamping=True)`; the `normal_r_gain`/`fpfh_r_gain` constructor arguments resolve (scaled by `voxel_size`) into the absolute `normal_radius`/`fpfh_radius` read/write properties below, while `enable_noise_bound_clamping` is a constructor-only argument with no `def_readwrite` accessor. Both the absolute `robin_noise_bound`/`solver_noise_bound` and their `_gain` scalings are read/write.

| [INDEX] | [FIELD]                   | [PROPERTY_KIND] | [CAPABILITY]                                      |
| :-----: | :------------------------ | :-------------- | :------------------------------------------------ |
|  [01]   | `voxel_size`              | read/write      | downsampling voxel edge length                    |
|  [02]   | `use_voxel_sampling`      | read/write      | toggle the voxel-grid downsample stage            |
|  [03]   | `use_quatro`              | read/write      | switch the solver to Quatro for degenerate scenes |
|  [04]   | `thr_linearity`           | read/write      | linearity threshold guarding degenerate geometry  |
|  [05]   | `num_max_corr`            | read/write      | cap on retained correspondences                   |
|  [06]   | `normal_radius`           | read/write      | absolute normal-estimation radius                 |
|  [07]   | `fpfh_radius`             | read/write      | absolute Faster-PFH feature radius                |
|  [08]   | `robin_noise_bound`       | read/write      | ROBIN outlier-pruning noise bound                 |
|  [09]   | `robin_noise_bound_gain`  | read/write      | gain scaling the ROBIN noise bound                |
|  [10]   | `solver_noise_bound`      | read/write      | GNC solver noise bound                            |
|  [11]   | `solver_noise_bound_gain` | read/write      | gain scaling the solver noise bound               |

[PUBLIC_TYPE_SCOPE]: `RegistrationSolution` fields
- rail: global-registration

| [INDEX] | [FIELD]       | [PROPERTY_KIND] | [CAPABILITY]                             |
| :-----: | :------------ | :-------------- | :--------------------------------------- |
|  [01]   | `rotation`    | read/write      | 3x3 rotation matrix                      |
|  [02]   | `translation` | read/write      | 3-vector translation                     |
|  [03]   | `valid`       | read/write      | convergence/validity flag for the result |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: registration (`KISSMatcher.estimate`)
- rail: global-registration

Construct the matcher from a voxel size or a `KISSMatcherConfig`, then call `estimate` for the full pipeline. Source and target are sequences of `float32` `(3, 1)` points; the result is a `RegistrationSolution`.

| [INDEX] | [SURFACE]                                    | [ENTRY_FAMILY] | [CAPABILITY]                         |
| :-----: | :------------------------------------------- | :------------- | :----------------------------------- |
|  [01]   | `KISSMatcher(voxel_size: float)`             | constructor    | matcher from a single voxel size     |
|  [02]   | `KISSMatcher(config: KISSMatcherConfig)`     | constructor    | matcher from a full config           |
|  [03]   | `estimate(src, tgt) -> RegistrationSolution` | pipeline       | full keypoint/match/prune/solve pass |
|  [04]   | `print() -> None`                            | report         | print the registration summary       |
|  [05]   | `clear() -> None` / `reset() -> None`        | lifecycle      | clear cached buffers between runs    |

[ENTRYPOINT_SCOPE]: decomposed match and solve
- rail: global-registration

The pipeline decomposes into a `match` keypoint stage and a `prune_and_solve` or `solve` stage when the matched correspondences are reused. `match` and `prune_and_solve` take `float32` `(3, 1)` sequences (or `float64` `(3, n)` arrays); `solve` takes `float64` `(3, n)` matched arrays directly.

| [INDEX] | [SURFACE]                                                           | [ENTRY_FAMILY] | [CAPABILITY]                               |
| :-----: | :------------------------------------------------------------------ | :------------- | :----------------------------------------- |
|  [01]   | `match(src, tgt) -> tuple[list[NDArray], list[NDArray]]`            | match          | extract and match keypoint correspondences |
|  [02]   | `prune_and_solve(src_matched, tgt_matched) -> RegistrationSolution` | prune+solve    | ROBIN prune plus GNC solve on matches      |
|  [03]   | `solve(src_matched, tgt_matched) -> RegistrationSolution`           | solve          | GNC solve on already-pruned matches        |
|  [04]   | `reset_solver() -> None`                                            | lifecycle      | reset the solver state                     |

[ENTRYPOINT_SCOPE]: stage keypoints, correspondences, and receipts
- rail: global-registration

These accessors expose the intermediate keypoint clouds, the correspondence index pairs, the inlier counts, and the per-stage timings that form the registration receipt.

| [INDEX] | [SURFACE]                                                                      | [ENTRY_FAMILY] | [CAPABILITY]                              |
| :-----: | :----------------------------------------------------------------------------- | :------------- | :---------------------------------------- |
|  [01]   | `get_keypoints_from_faster_pfh() -> tuple[list[NDArray], list[NDArray]]`       | keypoints      | Faster-PFH source/target keypoints        |
|  [02]   | `get_keypoints_from_initial_matching() -> tuple[list[NDArray], list[NDArray]]` | keypoints      | initially matched source/target keypoints |
|  [03]   | `get_processed_input_clouds() -> tuple[list[NDArray], list[NDArray]]`          | clouds         | downsampled source/target clouds          |
|  [04]   | `get_initial_correspondences() -> list[tuple[int, int]]`                       | correspondence | source/target index pairs before pruning  |
|  [05]   | `get_final_correspondences() -> list[tuple[int, int]]`                         | correspondence | inlier index pairs after pruning          |
|  [06]   | `get_num_final_inliers() -> int`                                               | receipt        | retained inlier count                     |
|  [07]   | `get_num_rotation_inliers() -> int`                                            | receipt        | rotation-consistent inlier count          |
|  [08]   | `get_extraction_time()` / `get_matching_time()`                                | receipt        | keypoint and matching stage timings       |
|  [09]   | `get_rejection_time()` / `get_solver_time()`                                   | receipt        | pruning and solver stage timings          |
|  [10]   | `get_processing_time()`                                                        | receipt        | total registration wall time              |

## [04]-[IMPLEMENTATION_LAW]

[GLOBAL_REGISTRATION]:
- import: `import kiss_matcher` at boundary scope only; module-level import is banned by the manifest import policy.
- point shape: `estimate`, `match`, and `prune_and_solve` accept `float32` `(3, 1)` point sequences; `solve` and the array overloads of `match`/`prune_and_solve` accept `float64` `(3, n)` arrays. Feed contiguous numpy buffers and keep the dtype consistent with the chosen overload.
- pipeline axis: `estimate` is the single end-to-end entry; the `match` plus `prune_and_solve`/`solve` decomposition reuses the matched correspondences when downstream stages need the intermediate keypoints or correspondences without re-extraction.
- solver axis: `use_quatro` switches the GNC solver to Quatro for degenerate or planar scenes; `thr_linearity`, `robin_noise_bound`, and `solver_noise_bound` (with their `_gain` scalings) tune the ROBIN pruning and GNC convergence.
- evidence: each run captures `get_num_final_inliers`, `get_num_rotation_inliers`, the initial/final correspondence pairs, and the per-stage timings (`extraction`/`matching`/`rejection`/`solver`/`processing`) as the registration receipt feeding handoff and the fine-refinement decision.
- boundary: `kiss_matcher` owns coarse initialization-free global registration; the resulting `RegistrationSolution` transform seeds fine `small_gicp` GICP/VGICP refinement, surface reconstruction and FPFH-free coarse alignment route to `open3d`, and general PLY/scan IO routes to `open3d`/`laspy` rather than this estimator.

[STACKING_LAW]:
- the scan source is the `laspy` chunked reader: per-chunk `ScaleAwarePointRecord` metric `xyz` buffers feed `estimate`/`match` as `float32` `(3, 1)` sequences (or transposed `float64` `(3, n)` Eigen matrices for the array overloads) — `kiss_matcher` never opens a file, it consumes the numpy buffers the scan-processing reader produces.
- the output stacks into fine refinement: the `RegistrationSolution.rotation`/`translation` compose into the initial 4x4 pose that `small_gicp` GICP/VGICP refines; this estimator is the coarse arm of the two-stage registration union, never the fine/ICP role (that belongs to `small_gicp`) and never identity-minting (the runtime owns identity).
- both inlier counts and the five stage timings are the typed registration receipt — `get_num_final_inliers`/`get_num_rotation_inliers` gate the fine-refinement decision and the inlier fraction is the convergence evidence, not a discarded log line.
- offload: a multi-second `estimate` over a large unposed scan pair is CPU-bound with no async mirror, so it hands to the geometry kernel offload lane (the `GEOMETRY_CPU_OFFLOAD` / `GEOMETRY_KERNEL_OFFLOAD_LANE` seam) rather than blocking the boundary; `clear`/`reset`/`reset_solver` recycle the estimator between offloaded runs.

## [05]-[LOCAL_ADMISSION]

[RAIL_LAW]:
- Package: `kiss-matcher`
- Owns: global initialization-free rigid registration, Faster-PFH keypoint extraction, correspondence matching, ROBIN outlier pruning, and GNC/Quatro pose solving
- Accept: coarse global registration of an unposed scan pair feeding the scan-processing owner and the fine `small_gicp` refinement
- Reject: wrapper-renames of `estimate`/`match`/`solve`; a hand-rolled FPFH keypoint extractor, graph-theoretic outlier rejector, or GNC solver where `kiss_matcher` is admitted; an ICP/fine-refinement role that belongs to `small_gicp`; identity minting the runtime owns

[CAPTURE_GAP]:
- members: verified against the `kiss_matcher_pybind.cpp` `def`/`def_readwrite` binding and the installed cp312 distribution; the `KISSMatcher`/`KISSMatcherConfig`/`RegistrationSolution` classes, the two `match` overloads (`Vector3f` sequence vs `Matrix<double,3,Dynamic>`), the `estimate`/`solve`/`prune_and_solve` entries, the eleven `def_readwrite` config fields (with `enable_noise_bound_clamping` constructor-only), and the stage/receipt accessors resolve against the live pybind11 signatures — no phantom. The internal pybind `__version__` reads `0.3.1` while the distribution is `1.0.2`; the distribution version is authoritative.

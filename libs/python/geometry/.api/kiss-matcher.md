# [PY_GEOMETRY_API_KISS_MATCHER]

`kiss_matcher` supplies the initialization-free global registration path for the scan-processing rail: `KISSMatcher` extracts Faster-PFH keypoints, matches correspondences, prunes outliers through ROBIN and a GNC solver, and returns a `RegistrationSolution` rigid transform, exposing per-stage timing and inlier counts as the receipt. Package owner composes `estimate` (or `match` then `prune_and_solve`) into the global-registration mode seeding fine `small_gicp` refinement; it never re-implements keypoint extraction, graph-theoretic outlier rejection, or the GNC pose solver.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `kiss-matcher`
- package: `kiss-matcher`
- import: `import kiss_matcher`
- owner: `geometry`
- rail: scan-processing / global-registration
- entry points: none (library only)
- capability: initialization-free global rigid registration, Faster-PFH keypoint extraction, correspondence matching, ROBIN outlier pruning, graduated-non-convexity and Quatro pose solving, and per-stage timing with inlier receipts

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: estimator, config, and result family
- rail: global-registration

| [INDEX] | [SYMBOL]               | [TYPE_FAMILY]       | [CAPABILITY]                                           |
| :-----: | :--------------------- | :------------------ | :----------------------------------------------------- |
|  [01]   | `KISSMatcher`          | global matcher      | end-to-end global registration and stage accessors     |
|  [02]   | `KISSMatcherConfig`    | parameter carrier   | voxel/normal/FPFH/noise-bound and solver-mode settings |
|  [03]   | `RegistrationSolution` | registration result | `rotation`, `translation`, `valid` rigid transform     |

[PUBLIC_TYPE_SCOPE]: `KISSMatcherConfig` fields
- rail: global-registration

Construct with `KISSMatcherConfig(voxel_size, *, use_voxel_sampling, use_quatro, thr_linearity, num_max_corr, normal_r_gain, fpfh_r_gain, robin_noise_bound_gain, solver_noise_bound_gain, enable_noise_bound_clamping)`; every field below is a read/write `def_readwrite` property, `normal_r_gain`/`fpfh_r_gain` scale by `voxel_size` into the absolute `normal_radius`/`fpfh_radius`, and `enable_noise_bound_clamping` is constructor-only with no accessor.

| [INDEX] | [FIELD]                   | [CAPABILITY]                                      |
| :-----: | :------------------------ | :------------------------------------------------ |
|  [01]   | `voxel_size`              | downsampling voxel edge length                    |
|  [02]   | `use_voxel_sampling`      | toggle the voxel-grid downsample stage            |
|  [03]   | `use_quatro`              | switch the solver to Quatro for degenerate scenes |
|  [04]   | `thr_linearity`           | linearity threshold guarding degenerate geometry  |
|  [05]   | `num_max_corr`            | cap on retained correspondences                   |
|  [06]   | `normal_radius`           | absolute normal-estimation radius                 |
|  [07]   | `fpfh_radius`             | absolute Faster-PFH feature radius                |
|  [08]   | `robin_noise_bound`       | ROBIN outlier-pruning noise bound                 |
|  [09]   | `robin_noise_bound_gain`  | gain scaling the ROBIN noise bound                |
|  [10]   | `solver_noise_bound`      | GNC solver noise bound                            |
|  [11]   | `solver_noise_bound_gain` | gain scaling the solver noise bound               |

[PUBLIC_TYPE_SCOPE]: `RegistrationSolution` fields
- rail: global-registration

Every field is a read/write `def_readwrite` property.

| [INDEX] | [FIELD]       | [CAPABILITY]                             |
| :-----: | :------------ | :--------------------------------------- |
|  [01]   | `rotation`    | 3x3 rotation matrix                      |
|  [02]   | `translation` | 3-vector translation                     |
|  [03]   | `valid`       | convergence/validity flag for the result |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: registration (`KISSMatcher.estimate`)
- rail: global-registration

Construct from a voxel size or a `KISSMatcherConfig`, then call `estimate` for the full pipeline.

| [INDEX] | [SURFACE]                                    | [ENTRY_FAMILY] | [CAPABILITY]                         |
| :-----: | :------------------------------------------- | :------------- | :----------------------------------- |
|  [01]   | `KISSMatcher(voxel_size: float)`             | constructor    | matcher from a single voxel size     |
|  [02]   | `KISSMatcher(config: KISSMatcherConfig)`     | constructor    | matcher from a full config           |
|  [03]   | `estimate(src, tgt) -> RegistrationSolution` | pipeline       | full keypoint/match/prune/solve pass |
|  [04]   | `print() -> None`                            | report         | print the registration summary       |
|  [05]   | `clear() -> None` / `reset() -> None`        | lifecycle      | clear cached buffers between runs    |

[ENTRYPOINT_SCOPE]: decomposed match and solve
- rail: global-registration

Pipeline decomposes into a `match` keypoint stage and a `prune_and_solve`/`solve` stage reusing the matched correspondences.

| [INDEX] | [SURFACE]                                                           | [ENTRY_FAMILY] | [CAPABILITY]                               |
| :-----: | :------------------------------------------------------------------ | :------------- | :----------------------------------------- |
|  [01]   | `match(src, tgt) -> tuple[list[NDArray], list[NDArray]]`            | match          | extract and match keypoint correspondences |
|  [02]   | `prune_and_solve(src_matched, tgt_matched) -> RegistrationSolution` | prune+solve    | ROBIN prune and GNC solve on matches       |
|  [03]   | `solve(src_matched, tgt_matched) -> RegistrationSolution`           | solve          | GNC solve on already-pruned matches        |
|  [04]   | `reset_solver() -> None`                                            | lifecycle      | reset the solver state                     |

[ENTRYPOINT_SCOPE]: stage keypoints, correspondences, and receipts
- rail: global-registration

These accessors expose each intermediate stage as the registration receipt; keypoint and cloud accessors ([01]-[03]) return `tuple[list[NDArray], list[NDArray]]` source/target.

| [INDEX] | [SURFACE]                                                | [ENTRY_FAMILY] | [CAPABILITY]                              |
| :-----: | :------------------------------------------------------- | :------------- | :---------------------------------------- |
|  [01]   | `get_keypoints_from_faster_pfh()`                        | keypoints      | Faster-PFH source/target keypoints        |
|  [02]   | `get_keypoints_from_initial_matching()`                  | keypoints      | initially matched source/target keypoints |
|  [03]   | `get_processed_input_clouds()`                           | clouds         | downsampled source/target clouds          |
|  [04]   | `get_initial_correspondences() -> list[tuple[int, int]]` | correspondence | source/target index pairs before pruning  |
|  [05]   | `get_final_correspondences() -> list[tuple[int, int]]`   | correspondence | inlier index pairs after pruning          |
|  [06]   | `get_num_final_inliers() -> int`                         | receipt        | retained inlier count                     |
|  [07]   | `get_num_rotation_inliers() -> int`                      | receipt        | rotation-consistent inlier count          |
|  [08]   | `get_extraction_time()` / `get_matching_time()`          | receipt        | keypoint and matching stage timings       |
|  [09]   | `get_rejection_time()` / `get_solver_time()`             | receipt        | pruning and solver stage timings          |
|  [10]   | `get_processing_time()`                                  | receipt        | total registration wall time              |

## [04]-[IMPLEMENTATION_LAW]

[GLOBAL_REGISTRATION]:
- import: `import kiss_matcher` at boundary scope only; module-level import is banned by the manifest import policy.
- point shape: `estimate`/`match`/`prune_and_solve` accept `float32` `(3, 1)` point sequences; `solve` and the array overloads accept `float64` `(3, n)` arrays, contiguous numpy buffers with dtype matching the overload.
- pipeline axis: `estimate` is the single end-to-end entry; the `match` then `prune_and_solve`/`solve` decomposition reuses matched correspondences when downstream stages need the intermediate keypoints without re-extraction.
- solver axis: `use_quatro` switches the GNC solver to Quatro for degenerate or planar scenes; `thr_linearity`, `robin_noise_bound`, and `solver_noise_bound` with their `_gain` scalings tune ROBIN pruning and GNC convergence.
- evidence: each run captures `get_num_final_inliers`, `get_num_rotation_inliers`, the initial/final correspondence pairs, and the five stage timings (`extraction`/`matching`/`rejection`/`solver`/`processing`) as the receipt gating handoff and the fine-refinement decision.
- boundary: `kiss_matcher` owns coarse initialization-free global registration; surface reconstruction and coarse feature alignment route to `open3d`, and PLY/scan IO to `open3d`/`laspy` rather than this estimator.

[STACKING]:
- `laspy` chunked reader is the scan source: per-chunk `ScaleAwarePointRecord` metric `xyz` buffers feed `estimate`/`match` as `float32` `(3, 1)` sequences (transposed `float64` `(3, n)` for the array overloads); `kiss_matcher` consumes the numpy buffers, never opening a file.
- `small_gicp` is the downstream fine arm: `RegistrationSolution.rotation`/`translation` compose the initial 4x4 pose `small_gicp` GICP/VGICP refines — the coarse arm of the two-stage registration union, never the fine/ICP role and never identity minting.
- geometry kernel offload: a multi-second `estimate` over a large unposed scan pair is CPU-bound with no async mirror, handing to the `GEOMETRY_CPU_OFFLOAD`/`GEOMETRY_KERNEL_OFFLOAD_LANE` seam rather than blocking the boundary; `clear`/`reset`/`reset_solver` recycle the estimator between offloaded runs.

[RAIL_LAW]:
- Package: `kiss-matcher`
- Owns: global initialization-free rigid registration, Faster-PFH keypoint extraction, correspondence matching, ROBIN outlier pruning, and GNC/Quatro pose solving
- Accept: coarse global registration of an unposed scan pair feeding the scan-processing owner and fine `small_gicp` refinement
- Reject: wrapper-renames of `estimate`/`match`/`solve`; a hand-rolled FPFH extractor, graph-theoretic rejector, or GNC solver where `kiss_matcher` is admitted; an ICP/fine-refinement role belonging to `small_gicp`; identity minting the runtime owns

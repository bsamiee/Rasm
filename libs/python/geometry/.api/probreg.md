# [PY_GEOMETRY_API_PROBREG]

`probreg` owns correspondence-free probabilistic point-set registration for the scan-processing rail: Gaussian-mixture estimators align a source array onto a target without correspondences, returning a `Transformation` whose non-rigid arm warps a per-point deformation field over arbitrary query points. One `registration_*` entrypoint per algorithm discriminates rigid, affine, and non-rigid on a `tf_type_name` string. It fills the non-rigid slot `kiss_matcher`, `open3d`, and `small_gicp` leave open; native compilation and a hard `open3d` import bind it to the worker interpreter.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `probreg`
- package: `probreg` (MIT)
- module: `probreg` — `cpd`/`filterreg`/`gmmtree`/`l2dist_regs`/`bcpd` submodules; `transformation` aliased `tf`
- owner: `geometry`
- rail: scan-processing / non-rigid-registration
- entry points: none (library only)
- gate: sdist-only native C++ build (`Eigen`/`OpenMP`/`pybind11`) and a hard `open3d` runtime import bind it to the worker interpreter beside `kiss_matcher`/`open3d`
- capability: correspondence-free probabilistic non-rigid registration over point arrays

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: transformation family (`probreg.transformation`)

`Transformation.transform(points, array_type=open3d.utility.Vector3dVector)` warps an array through the estimated model, so a non-rigid instance IS the deformation-field carrier mapping arbitrary query points, not only the registered source; `RigidTransformation` composes `rot`/`t`/`scale` into the rail's 4x4 contract through `inverse()` and `__mul__`, and the non-rigid variants carry the warp basis.

| [INDEX] | [SYMBOL]                   | [TYPE_FAMILY]  | [CAPABILITY]                                                           |
| :-----: | :------------------------- | :------------- | :--------------------------------------------------------------------- |
|  [01]   | `Transformation`           | transform base | `transform(points)` warp; the shared registration result               |
|  [02]   | `RigidTransformation`      | transform      | `rot`/`t`/`scale`, `inverse()`, `__mul__` composition                  |
|  [03]   | `AffineTransformation`     | transform      | `b` matrix plus `t` translation                                        |
|  [04]   | `CombinedTransformation`   | transform      | rigid plus non-rigid displacement `v`                                  |
|  [05]   | `NonRigidTransformation`   | transform      | Gaussian-RBF displacement field `w` over `points` at `beta`            |
|  [06]   | `TPSTransformation`        | transform      | thin-plate-spline warp; `prepare(landmarks)`, `transform_basis(basis)` |
|  [07]   | `DeformableKinematicModel` | transform      | dual-quaternion skinning; `make_weight(pairs, vals)` classmethod       |

[PUBLIC_TYPE_SCOPE]: estimator and receipt family

Every estimator shares `set_source(source)`, `set_callbacks(callbacks)`, and `registration(target, ...)`; CPD, FilterReg, and GMMTree return `MstepResult(transformation, sigma2, q)`, the L2-distance and Bayesian estimators a `Transformation` directly.

| [INDEX] | [SYMBOL]                             | [TYPE_FAMILY]  | [CAPABILITY]                                              |
| :-----: | :----------------------------------- | :------------- | :-------------------------------------------------------- |
|  [01]   | `CoherentPointDrift`                 | estimator base | EM GMM drift; `Rigid`/`Affine`/`NonRigid` subclasses      |
|  [02]   | `RigidCPD`/`AffineCPD`/`NonRigidCPD` | estimator      | rigid, affine, and Gaussian-RBF non-rigid CPD arms        |
|  [03]   | `ConstrainedNonRigidCPD`             | estimator      | landmark-constrained non-rigid CPD                        |
|  [04]   | `FilterReg`                          | estimator      | permutohedral-filter GMM; `Rigid`/`DeformableKinematic`   |
|  [05]   | `DeformableKinematicFilterReg`       | estimator      | skinning-weighted deformable FilterReg                    |
|  [06]   | `GMMTree`                            | estimator      | hierarchical GMM-tree registration                        |
|  [07]   | `L2DistRegistration`                 | estimator base | GMMReg/SVR L2-distance minimization                       |
|  [08]   | `RigidGMMReg`/`TPSGMMReg`            | estimator      | rigid and TPS-non-rigid GMMReg                            |
|  [09]   | `RigidSVR`/`TPSSVR`                  | estimator      | rigid and TPS-non-rigid support-vector registration       |
|  [10]   | `BayesianCoherentPointDrift`         | estimator      | Bayesian CPD posterior over the deformation               |
|  [11]   | `CombinedBCPD`                       | estimator      | combined rigid-plus-non-rigid Bayesian CPD                |
|  [12]   | `MstepResult` / `EstepResult`        | receipt        | `(transformation, sigma2, q)` and EM per-point statistics |

[PUBLIC_TYPE_SCOPE]: feature and callback support

`Feature` functions feed `registration_filterreg(feature_fn=...)`; callbacks receive the running `Transformation` after each EM iteration.

| [INDEX] | [SYMBOL]                   | [TYPE_FAMILY] | [CAPABILITY]                                                 |
| :-----: | :------------------------- | :------------ | :----------------------------------------------------------- |
|  [01]   | `FPFH`                     | feature       | open3d FPFH descriptor; `radius_normal`/`radius_feature`     |
|  [02]   | `GMM`                      | feature       | Gaussian-mixture feature reduction over `n_gmm_components`   |
|  [03]   | `OneClassSVM`              | feature       | support-vector density feature; `sigma`/`gamma`/`nu`/`delta` |
|  [04]   | `Plot2DCallback`           | callback      | per-iteration 2D transform preview                           |
|  [05]   | `Open3dVisualizerCallback` | callback      | per-iteration open3d transform preview                       |
|  [06]   | `CostFunction` family      | cost          | rigid/TPS L2-distance objective and gradient for GMMReg      |
|  [07]   | `Normalizer`               | numeric       | `normalize`/`denormalize` unit-cube conditioning             |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: registration (`probreg.<module>.registration_*`)

Each entrypoint takes `source, target` as a numpy `Nx3`/`Nx6` array or an `open3d.geometry.PointCloud`, the transformation class chosen by the `tf_type_name` string; CPD, FilterReg, and GMMTree return `MstepResult`, GMMReg/SVR/BCPD a `Transformation`.

| [INDEX] | [SURFACE]                          | [ENTRY_FAMILY] | [CAPABILITY]                                                            |
| :-----: | :--------------------------------- | :------------- | :---------------------------------------------------------------------- |
|  [01]   | `cpd.registration_cpd`             | CPD            | `tf_type_name` (rigid/affine/nonrigid/constrained); `w`,`maxiter`,`tol` |
|  [02]   | `filterreg.registration_filterreg` | FilterReg      | `objective_type` pt2pt/pt2pl; `target_normals`,`sigma2`,`feature_fn`    |
|  [03]   | `gmmtree.registration_gmmtree`     | GMMTree        | hierarchical GMM tree; `maxiter`,`tree_level`,`lambda_c`,`lambda_s`     |
|  [04]   | `l2dist_regs.registration_gmmreg`  | GMMReg         | `tf_type_name` rigid/nonrigid L2-distance GMM                           |
|  [05]   | `l2dist_regs.registration_svr`     | SVR            | `tf_type_name` rigid/nonrigid support-vector; inner/outer loops         |
|  [06]   | `bcpd.registration_bcpd`           | BCPD           | combined rigid-plus-non-rigid Bayesian CPD; `w`,`maxiter`               |

[ENTRYPOINT_SCOPE]: result and warp accessors

`MstepResult` carries the optimization receipt; `Transformation.transform` warps arbitrary points through the recovered deformation field.

| [INDEX] | [SURFACE]                               | [ENTRY_FAMILY] | [CAPABILITY]                                           |
| :-----: | :-------------------------------------- | :------------- | :----------------------------------------------------- |
|  [01]   | `result.transformation`                 | result         | estimated source-to-target `Transformation`            |
|  [02]   | `result.sigma2` / `result.q`            | result         | final GMM variance and log-likelihood objective        |
|  [03]   | `transformation.transform(points)`      | warp           | apply the estimated warp to any point array            |
|  [04]   | `RigidTransformation.rot`/`.t`/`.scale` | rigid          | rotation, translation, scale for the 4x4 rail contract |
|  [05]   | `estimator.registration(target, ...)`   | estimator      | run EM from a preconfigured estimator plus callbacks   |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- import: `from probreg import cpd, filterreg, gmmtree, l2dist_regs, bcpd` at boundary scope only; probreg pulls `open3d` transitively at import.
- algorithm axis: one `registration_*` per algorithm family; `tf_type_name` (`rigid`/`affine`/`nonrigid`/`nonrigid_constrained`) selects the transformation, never a per-modality function family, and FilterReg's `objective_type` (`pt2pt`/`pt2pl`) discriminates the geometry term, consuming `target_normals` for the point-to-plane arm.
- deformation axis: the non-rigid arms return a `NonRigidTransformation`, `TPSTransformation`, or `DeformableKinematicModel` whose `transform` warps arbitrary query points — that warp IS the per-point deformation field the scan-deviation owner projects to split rigid residual from deformation magnitude.
- receipt axis: CPD/FilterReg/GMMTree capture `MstepResult(transformation, sigma2, q)`; GMMReg/SVR/BCPD return a bare `Transformation` and expose convergence only through iteration callbacks.
- feature axis: `registration_filterreg(feature_fn=FPFH())` lifts the GMM correspondence into FPFH space; `GMM` and `OneClassSVM` are the alternate reducers, `feature_fn=lambda x: x` the raw-coordinate objective.
- conditioning: `Normalizer` centres and scales a cloud to the unit cube before EM and denormalizes the recovered transform.

[STACKING]:
- `open3d`(`.api/open3d.md`): `probreg.FPFH` computes its descriptor through open3d's `registration.compute_fpfh_feature`/`Feature`, `registration_*` accepts an `open3d.geometry.PointCloud` source/target directly, and `transform(array_type=open3d.utility.Vector3dVector)` returns an open3d buffer the scan owner reuses.
- `small_gicp`(`.api/small-gicp.md`): a rigid-arm `RigidTransformation` (`rot`/`t` as a 4x4) seeds `small_gicp.align(init_T_target_source=...)` for the fine GICP/VGICP refinement following a coarse probabilistic alignment.
- geometry scan owner: composes `cpd.registration_cpd`/`filterreg.registration_filterreg` into the non-rigid registration mode; the non-rigid `Transformation.transform` warp is the per-point deformation field the deviation split consumes.

[LOCAL_ADMISSION]:
- probreg is admitted for the probabilistic non-rigid slot the global `kiss_matcher`, coarse `open3d`, and fine `small_gicp` engines leave open.

[RAIL_LAW]:
- Package: `probreg`
- Owns: correspondence-free probabilistic point-set registration and its non-rigid deformation-field warps
- Accept: probabilistic non-rigid registration feeding the scan-processing owner and the deformation-field deviation split
- Reject: wrapper-renames of `registration_cpd`/`registration_filterreg`; a hand-rolled CPD EM loop, GMM correspondence weighting, or TPS warp where probreg is admitted; a per-modality registration function family over the `tf_type_name`/`objective_type` rows; rigid global registration `kiss_matcher`/`open3d` own; mesh-to-mesh non-rigid deformation `trimesh.registration` owns

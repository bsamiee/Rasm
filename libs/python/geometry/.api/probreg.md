# [PY_GEOMETRY_API_PROBREG]

`probreg` supplies the probabilistic point-set registration path for the scan-processing rail: Gaussian-mixture estimators — Coherent Point Drift, FilterReg, GMMTree, GMMReg/SVR, and Bayesian CPD — align a source array to a target without correspondences and return a `Transformation` mapping source onto target. One `registration_*` entrypoint per algorithm discriminates rigid, affine, and non-rigid types on a `tf_type_name` string, so the non-rigid arm yields a per-point deformation field the rail composes as a warp over arbitrary query points. Package owner composes it into the non-rigid registration mode and never re-implements the EM loop, the GMM correspondence weighting, or the thin-plate-spline warp probreg owns. It completes the geometry registration family beside the global `kiss_matcher`, coarse `open3d`, and fine-GICP `small_gicp` engines — the probabilistic non-rigid slot none of them cover. Native Eigen/OpenMP/pybind11 compilation and a hard `open3d` import keep the arm on the worker interpreter.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `probreg`
- package: `probreg`
- import: `import probreg` with `from probreg import cpd, filterreg, gmmtree, l2dist_regs, bcpd` submodule entries; `probreg.transformation` aliased `tf`
- owner: `geometry`
- rail: scan-processing / non-rigid-registration
- license: `MIT` (own)
- entry points: none (library only)
- gate: sdist-only C++ (`Eigen`/`OpenMP`/`pybind11`) native build and a hard `open3d` runtime import; the row rides the worker interpreter with `kiss_matcher`/`open3d`
- capability: correspondence-free probabilistic registration over point arrays — rigid/affine/non-rigid CPD, point-to-point and point-to-plane FilterReg, hierarchical GMMTree, L2-distance GMMReg and support-vector registration, Bayesian CPD, thin-plate-spline and dual-quaternion-skinning non-rigid warps, FPFH/GMM/one-class-SVM feature functions, and EM-iteration callbacks

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: transformation family (`probreg.transformation`)
- rail: scan-processing

`Transformation` is the shared result carrier: `transform(points, array_type=open3d.utility.Vector3dVector)` warps an array through the estimated model, so a non-rigid instance IS the deformation-field carrier — it maps arbitrary query points, not only the registered source. `RigidTransformation` exposes `rot`/`t`/`scale` composing into the rail's 4x4 contract with `inverse()` and `__mul__`; the non-rigid variants carry the warp basis.

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
- rail: scan-processing

Every estimator shares `set_source(source)`, `set_callbacks(callbacks)`, and `registration(target, ...)`. CPD, FilterReg, and GMMTree return `MstepResult(transformation, sigma2, q)`; the L2-distance and Bayesian estimators return a `Transformation` directly.

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
- rail: scan-processing

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
- rail: scan-processing

Each algorithm exposes one `registration_*` entrypoint over `source, target` (numpy `Nx3`/`Nx6` array or `open3d.geometry.PointCloud`); the transformation class is the `tf_type_name` string, never a function family. CPD, FilterReg, and GMMTree return `MstepResult`; GMMReg, SVR, and BCPD return a `Transformation`.

| [INDEX] | [SURFACE]                          | [ENTRY_FAMILY] | [CAPABILITY]                                                            |
| :-----: | :--------------------------------- | :------------- | :---------------------------------------------------------------------- |
|  [01]   | `cpd.registration_cpd`             | CPD            | `tf_type_name` (rigid/affine/nonrigid/constrained); `w`,`maxiter`,`tol` |
|  [02]   | `filterreg.registration_filterreg` | FilterReg      | `objective_type` pt2pt/pt2pl; `target_normals`,`sigma2`,`feature_fn`    |
|  [03]   | `gmmtree.registration_gmmtree`     | GMMTree        | hierarchical GMM tree; `maxiter`,`tree_level`,`lambda_c`,`lambda_s`     |
|  [04]   | `l2dist_regs.registration_gmmreg`  | GMMReg         | `tf_type_name` rigid/nonrigid L2-distance GMM                           |
|  [05]   | `l2dist_regs.registration_svr`     | SVR            | `tf_type_name` rigid/nonrigid support-vector; inner/outer loops         |
|  [06]   | `bcpd.registration_bcpd`           | BCPD           | combined rigid-plus-non-rigid Bayesian CPD; `w`,`maxiter`               |

[ENTRYPOINT_SCOPE]: result and warp accessors
- rail: scan-processing

`MstepResult` carries the optimization receipt; the `Transformation` warps arbitrary points into the deformation field.

| [INDEX] | [SURFACE]                               | [ENTRY_FAMILY] | [CAPABILITY]                                           |
| :-----: | :-------------------------------------- | :------------- | :----------------------------------------------------- |
|  [01]   | `result.transformation`                 | result         | estimated source-to-target `Transformation`            |
|  [02]   | `result.sigma2` / `result.q`            | result         | final GMM variance and log-likelihood objective        |
|  [03]   | `transformation.transform(points)`      | warp           | apply the estimated warp to any point array            |
|  [04]   | `RigidTransformation.rot`/`.t`/`.scale` | rigid          | rotation, translation, scale for the 4x4 rail contract |
|  [05]   | `estimator.registration(target, ...)`   | estimator      | run EM from a preconfigured estimator plus callbacks   |

## [04]-[IMPLEMENTATION_LAW]

[NONRIGID_REGISTRATION]:
- import: `from probreg import cpd, filterreg, gmmtree, l2dist_regs, bcpd` at boundary scope only; module-level import is banned by the manifest import policy, and probreg pulls `open3d` transitively at import.
- algorithm axis: one `registration_*` entrypoint per algorithm family; `tf_type_name` (`rigid`/`affine`/`nonrigid`/`nonrigid_constrained`) selects the transformation, never a per-modality function family. FilterReg discriminates the geometry term through `objective_type` (`pt2pt`/`pt2pl`), consuming `target_normals` for the point-to-plane arm.
- deformation axis: the non-rigid arms return a `NonRigidTransformation`, `TPSTransformation`, or `DeformableKinematicModel` whose `transform` warps arbitrary query points; that warp IS the per-point deformation field the scan-deviation owner projects to split rigid residual from deformation magnitude.
- receipt axis: CPD/FilterReg/GMMTree capture `MstepResult(transformation, sigma2, q)` — the estimated transform, the converged GMM variance, and the likelihood objective — as the registration receipt; GMMReg/SVR/BCPD return the bare `Transformation` and expose convergence only through iteration callbacks.
- feature axis: `registration_filterreg(feature_fn=FPFH())` lifts the GMM correspondence into FPFH feature space; `GMM` and `OneClassSVM` are the alternate feature reducers. Bare `feature_fn=lambda x: x` keeps the raw-coordinate objective.
- conditioning: `Normalizer` centres and scales a cloud to the unit cube before EM and denormalizes the recovered transform, the standard probreg preconditioning for numerically stable variance updates.
- boundary: probreg owns correspondence-free probabilistic registration (CPD/FilterReg/GMMReg/SVR/GMMTree/BCPD) over point arrays. Global initialization-free rigid registration routes to `kiss_matcher`; coarse feature/FGR/RANSAC and PLY/scan IO to `open3d`; fine multi-threaded GICP/VGICP to `small_gicp`; mesh-to-mesh non-rigid Amberg/Sumner deformation to `trimesh.registration`. Every engine returns a transform the shared rail applies, so the backend is an argument, not a fork.

## [05]-[LOCAL_ADMISSION]

[RAIL_LAW]:
- Package: `probreg`
- Owns: probabilistic GMM registration — rigid/affine/non-rigid CPD, point-to-point/point-to-plane FilterReg, hierarchical GMMTree, L2-distance GMMReg and SVR, Bayesian CPD, thin-plate-spline and dual-quaternion-skinning warps, FPFH/GMM/SVM feature functions, and EM callbacks
- Accept: correspondence-free probabilistic non-rigid registration feeding the scan-processing owner and the deformation-field deviation split
- Reject: wrapper-renames of `registration_cpd`/`registration_filterreg`; a hand-rolled CPD EM loop or TPS warp where probreg is admitted; a per-modality registration function family over the `tf_type_name`/`objective_type` argument rows; rigid global registration the `kiss_matcher`/`open3d` arms already own

# [PY_COMPUTE_TASKLOG]

The open and closed work distilled from `IDEAS.md`. `[1]-[OPEN]` carries task cards whose leader holds a status marker — `[QUEUED]`, `[ACTIVE]`, or `[BLOCKED]` — and three to four scoped bullets: the capability or file to build, the external packages to integrate, the integration points and boundaries or wires, and the key considerations. `[2]-[CLOSED]` carries `[COMPLETE]` and `[DROPPED]` items. One idea spawns one or more tasks; each task names the exact sub-domain or file it lands in.

## [1]-[OPEN]

[QUEUED] Rebuild array admission on the Array API standard — from `ARRAY_API_ADMISSION`.
- Author `arrays/payload.md` `ArrayPayload.admit` over `array_namespace(x)` namespace dispatch, admitting numpy, JAX, Dask, and Sparse backends into one frozen payload that records the backend, dtype, shape, named axes, finite policy, and content key.
- Integrate `array-api-compat` (`array_namespace`, `device`, `to_device`, `is_*_array`) and `array-api-extra` (`at`, `atleast_nd`); both are admitted to the manifest `scientific` group as pure-Python cp315-clean members.
- Wire the content key through the runtime `ContentIdentity` over the host-transferred contiguous buffer so a payload keys identically across backends; every solver route reads the resolved `xp`, never a numpy-only floor.
- Key consideration: the `array-api-compat` admission API is a manifest candidate, so the fence verifies against the branch `.api` catalogue before transcription; JAX rides the namespace as a backend, numba stays a distinct loop-kernel row on the nonlinear route.

[QUEUED] Add the Lineax autodifferentiable linear-operator case — from `JAX_SOLVER_FAMILY`.
- Author the `LinearIntent.Operator` case and the `_operator_receipt` arm in `solvers/linear.md`, lifting a dense matrix or matrix-free operator into a `lineax.MatrixLinearOperator`/`FunctionLinearOperator` and resolving through `lineax.LU`/`QR`/`CG`/`GMRES`/`NormalCG`, folding `Solution.stats` into the one `SolverReceipt` inside the same `_dispatch` match.
- Integrate `lineax` and `equinox`, admitted to the manifest `scientific` group on the jaxlib `python_version<'3.15'` floor.
- Wire the autodifferentiable solve into `differentiation/sensitivity.md#SENSITIVITY` so a VJP through the solve reads the implicit-function-theorem adjoint; the case sits beside the scipy and numpy dense/sparse/eigen routes on the same `LinearIntent` owner, never a parallel free solve function.
- Key consideration: Lineax unifies what the scipy dense, sparse, and iterative calls do as one operator abstraction, so the case folds rather than duplicates; the spellings verify against the branch `.api` catalogue before any fence names them.

[QUEUED] Adopt Optimistix for the nonlinear routes — from `JAX_SOLVER_FAMILY`.
- Author `solvers/nonlinear.md` `NonlinearIntent` root-find, minimise, fixed-point, and least-squares cases over `optimistix.root_find`/`minimise`/`fixed_point`/`least_squares` with `Newton`/`BFGS`/`FixedPointIteration`/`LevenbergMarquardt` solvers, folding `Solution.stats` into `SolverReceipt` with a numpy gradient floor; fixed-point dispatches to `optimistix.fixed_point`, never collapsed into the root-finder.
- Integrate `optimistix` on the jaxlib floor and keep the numba `accelerate` loop-kernel row distinct from the Array-API backend dispatch.
- Wire the implicit-adjoint solve into `differentiation/sensitivity.md#SENSITIVITY`; the scipy `optimize` mid-tier and the numpy central-difference floor are the reachable branches beneath the gated Optimistix tier.
- Key consideration: the owner never carries a gradient-descent training loop; the spellings verify against the branch `.api` catalogue.

[QUEUED] Add the Diffrax differential-equations route — from `DIFFERENTIAL_EQUATIONS`.
- Author `solvers/differential.md` `DifferentialIntent` ODE, SDE, and CDE cases over `diffrax.diffeqsolve` with `Tsit5`/`Dopri5`/`Kvaerno5` solvers, `PIDController`/`ConstantStepSize` stepping, `Event` handling, and `RecursiveCheckpointAdjoint`/`BacksolveAdjoint` adjoint modes, folding `Solution.stats` into `SolverReceipt`.
- Integrate `diffrax` and `equinox` on the jaxlib floor; this is the genuine capability gap beside the scalar quadrature route, so there is no numpy ODE floor.
- Wire the adjoint solve into `differentiation/sensitivity.md#SENSITIVITY` and the parametric-trajectory case of `experiments/study.md#STUDY`, and align the form-finding output to the geometry `HandoffAxis` case at `graduation/receipt.md#GRADUATION`.
- Key consideration: trajectory integration of a stiff or stochastic field is the gated capability itself; the spellings verify against the branch `.api` catalogue.

[QUEUED] Replace the hand-rolled sensitivity with SALib — from `SALIB_SENSITIVITY`.
- Author the SALib sampler-and-analyzer pair in `experiments/study.md` for the Morris, Sobol-indices, and FAST cases over `SALib.sample.morris`/`sobol`/`fast_sampler` and `SALib.analyze.morris`/`sobol`/`fast`, reading `mu_star` and `ST` into the study receipt indices and deleting the numpy Morris and the Saltelli `NotImplementedError` stub.
- Integrate `SALib` (pure-Python over numpy and scipy) for sensitivity, `scipy.stats.qmc` for low-discrepancy sampling, and `scikit-learn` `GaussianProcessRegressor` for surrogates.
- Wire the `problem` dict from the study `ParamAxis` cohort; the sensitivity indices key by axis name so a `RunHistory` compare join lands on matching rows internal to the `experiments` sub-domain.
- Key consideration: SALib owns sensitivity, so the owner composes its pair rather than reimplementing Morris or Saltelli; SALib is a manifest candidate verified against the branch `.api` catalogue.

[QUEUED] Add the sampler-backend axis to inference — from `BAYESIAN_BACKEND_AXIS`.
- Author the `SamplerBackend` discriminant in `inference/bayesian.md` so `_draw` dispatches PyMC-native `sample`, NumPyro `sample_numpyro_nuts`, and Nutpie `compile_pymc_model`/`sample`, with `SamplerPlan.backend` selecting the engine and arviz scoring every trace.
- Integrate `pymc`, `arviz`, `numpyro` (already manifest-admitted on the jaxlib floor), and `nutpie` (admitted to the `scientific` group on the numba marker floor).
- Wire the `InferenceReceipt` convergence diagnostics into the uncertainty-law gate at `graduation/receipt.md#GRADUATION`; the `model_key` keys over the observed data, latents, backend, and plan so a backend change keys distinctly.
- Key consideration: the `arviz.InferenceData` group accessor is `trace.sample_stats`; the backend spellings verify against the branch `.api` catalogue once the cp315 wheels resolve.

[QUEUED] Add the mpmath enclosure floor — from `MPMATH_ENCLOSURE_FLOOR`.
- Author the mpmath interval branch in `validated_numerics/enclosure.md` `_certified_evaluate` beneath the gated python-flint Arb path, lifting inputs to an `mpmath.mpi` interval at `mpmath.mp.prec` and reading the endpoints back as a certified enclosure, with the numpy `nextafter` band as the last-resort uncertified floor.
- Integrate `mpmath` (pure-Python, cp315-clean), admitted to the manifest `scientific` group.
- Wire the floor ladder internal to the `validated_numerics` sub-domain so `evaluate` always has a reachable floor and never returns `Error(Import)`; a certified mpmath enclosure graduates as a proof through `graduation/receipt.md#GRADUATION`.
- Key consideration: mpmath gives a tight certified floor on cp315 where the Arb wheel is unavailable; the spellings verify against the branch `.api` catalogue.

[QUEUED] Open the spatial computational-geometry page — from `SPATIAL_GEOMETRY_QUERIES`.
- Author `spatial/query.md` `SpatialQuery` discriminating nearest-neighbour/radius search over `scipy.spatial.cKDTree`, convex hull and Delaunay over `ConvexHull`/`Delaunay`, Voronoi over `Voronoi`, and the alpha-shape boundary fold over the Delaunay circumradius, each folding into a typed `SpatialReceipt`.
- Integrate `scipy.spatial` (`cKDTree.query`/`query_ball_point`, `ConvexHull`, `Delaunay`, `Voronoi`), already manifest-admitted through scipy on the `python_version<'3.15'` marker, with a numpy brute-force neighbour floor.
- Wire the query input through `arrays/payload.md#PAYLOAD` so a point set keys by content identity, and align the boundary-reconstruction output to the geometry-branch scan companion at the wire; the owner never re-implements the geometry branch's `open3d` mesh surface.
- Key consideration: the alpha-shape circumradius threshold is the one local algorithm beside the scipy queries; the spellings verify against the branch `.api` catalogue.

[QUEUED] Open the simframe mesh-and-field interchange page — from `SIMFRAME_MESH_FIELD`.
- Author `simframe/mesh_field.md` `MeshField` carrying the mesh topology, per-node and per-cell field arrays, and the reusable `assemble` lowering a weak form to the sparse stiffness/load pair, plus a `meshio` read/write interchange entry.
- Integrate `scikit-fem` (`Mesh`, `Basis`, `asm`) for assembly and `meshio` for mesh-and-field file interchange; `meshio` is a manifest candidate admitted to the `scientific` group, `scikit-fem` rides the existing FEM marker floor.
- Wire the assembled stiffness/load pair into `solvers/quadrature.md#QUADRATURE` (the FEM solve consumes the pair rather than re-assembling) and into a Diffrax field problem at `solvers/differential.md#DIFFERENTIAL`; the mesh shape aligns to the geometry-branch tessellation at the wire and never imports its interior.
- Key consideration: this owner holds assembly and interchange only — the solve stays on the quadrature route, so the FEM page consumes `MeshField` rather than the reverse; `meshio` and the `skfem.asm` spellings verify against the branch `.api` catalogue.

[BLOCKED] Confirm each graduation axis against its C# wire-seam owner — from the cross-language wire.
- Map each `HandoffAxis` literal in `graduation/receipt.md`, and each `GeometrySubject` literal (`registration-transform`, `reconstructed-mesh`, `topology-graph`, `network-graph`, `form-finding`) under the geometry axis, to the C# owner that consumes it at the wire before the cases finalize.
- Integrate no new compute package; this is a cross-language wire confirmation against the C# owner planning, carried on this task and never baked into a routing literal on the receipt.
- Align the geometry, solver, model-asset, and uncertainty-law axes to their C# wire-seam owners; geometry-package evidence crosses only on the single geometry rail, and the receipt names the axis alone.
- Blocked on the C# graduation-evidence owner contract; until the per-axis consuming owner confirms, an axis graduates to a wire seam whose receiver is unverified.

## [2]-[CLOSED]

No task has closed.

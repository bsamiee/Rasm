# [PY_COMPUTE_IDEAS]

The forward pool of higher-order folder concepts grounded in the numeric-science domain and the monorepo purpose. `[1]-[OPEN]` carries the active ideas as cards; each card names the capability, what it unlocks, and the gap or modern technique it draws on. `[2]-[CLOSED]` carries the finished or dropped ideas with a one-line disposition so the same idea is never re-litigated. An idea drives one or more `TASKLOG.md` tasks.

## [1]-[OPEN]

(none — the optimization and signal ideas realized this pass; see `[2]-[CLOSED]`.)

## [2]-[CLOSED]

[CONVEX_CONIC_CERTIFICATE]: realized in `optimization/convex.md#CONVEX` — `ConvexProgram` over cvxpy DCP atoms + the Clarabel conic backend folds the duality-gap + per-constraint `dual_value` optimality certificate into one `ConvexReceipt`, graduating on the new `convex-program` `HandoffAxis` literal added at the three `graduation/receipt.md#GRADUATION` sites (alias, Cases bullet, CROSS_OWNER admission gate); the convex analogue of the `validated_numerics` enclosure ladder, companion-band `python_version<'3.15'` on both packages, no floor.

[MULTIRESOLUTION_SIGNAL]: realized in `signal/dsp.md#DSP` — the existing `SignalOp`/`SignalReceipt`/`_apply` owner gains the Wavelet case family (`Decompose`/`Scalogram`/`Packet` over `pywt.wavedec`/`waverec`/`cwt`/`WaveletPacket`) folding per-level energy, dominant scale, and the perfect-reconstruction residual beside the Welch fold, never a second signal surface; companion-band `python_version<'3.15'` beside the scipy.signal path.

[GRADIENT_DRIVEN_INVERSE_DESIGN]: realized in `optimization/design.md#DESIGN` — `DesignProblem` discriminates `Field`/`Mesh`/`Density` driving an `equinox.partition`/`combine` PyTree objective to a stationary point through `optimistix.minimise`/`least_squares` on one tag-keyed `_optimistix_design` route table, reading the default `ImplicitAdjoint` through the converged solve and folding the converged objective, the `jax.grad` first-order/KKT residual (the scalar-objective gradient for minimise, the least-squares-cost gradient `Jᵀr` for the `mesh` route), and `Solution.stats["num_steps"]` into one content-keyed `OptimizationReceipt`; the `optax.adam`-under-`OptaxMinimiser` first-order-descent axis is one row, composes `differentiation/sensitivity#SENSITIVITY`/`solvers/{linear,nonlinear,differential}`/`simframe/mesh_field#MESH_FIELD`, graduates on the existing `solver` `HandoffAxis` case, and carries a reachable numpy central-difference residual floor for every case; never a parallel optimizer surface, never a training loop.

[CONSTRAINED_DISCRETE_PROGRAM]: realized in `optimization/program.md#PROGRAM` — `ProgramIntent` discriminates `Linear`/`Integer`/`Global`/`Constrained`/`Assignment` over `scipy.optimize` (`linprog`/`milp`/`differential_evolution`/`minimize`/`linear_sum_assignment`) on one tag-keyed `_program_receipt` route table folding the `OptimizeResult` success flag, objective, and max constraint-violation residual into one content-keyed `ProgramReceipt`, graduating on the existing `solver` `HandoffAxis` case with no numpy floor.

[ARRAY_API_ADMISSION]: REALIZED in `arrays/payload.md` — `ArrayPayload.admit` over `array_namespace` namespace dispatch keying through `ContentIdentity.of`; JAX rides the namespace as a backend, numba stays a distinct nonlinear-route loop-kernel.

[JAX_SOLVER_FAMILY]: REALIZED in `solvers/linear.md` (`LinearIntent.Operator` over Lineax) and `solvers/nonlinear.md` (`NonlinearIntent` over Optimistix); both fold `Solution.stats` into the one `SolverReceipt`, both autodifferentiable through the implicit-function-theorem adjoint consumed by `sensitivity`.

[DIFFERENTIAL_EQUATIONS]: REALIZED in `solvers/differential.md` — `DifferentialIntent` ODE/SDE/CDE over `diffrax.diffeqsolve` with adjoint modes; the gated capability itself, no numpy floor, feeds the study parametric-trajectory case.

[SALIB_SENSITIVITY]: REALIZED in `experiments/study.md` — the SALib sampler-and-analyzer pair across Morris/Sobol/FAST plus the mined PAWN/DGSM/HDMR rows; the hand-rolled Morris and the Saltelli stub are deleted.

[BAYESIAN_BACKEND_AXIS]: REALIZED in `inference/bayesian.md` — `SamplerBackend` dispatches PyMC-native/NumPyro/Nutpie plus the mined BlackJAX JAX NUTS (`sample_blackjax_nuts`) as the fourth backend, arviz scoring all four through one diagnostic fold.

[MPMATH_ENCLOSURE_FLOOR]: REALIZED in `validated_numerics/enclosure.md` — the `mpmath.mpi` interval floor at `mp.prec` beneath the gated Arb path, the numpy `nextafter` band the uncertified last resort.

[SPATIAL_GEOMETRY_QUERIES]: REALIZED in `spatial/query.md` — `SpatialQuery` folds KD-tree neighbour/radius, hull, Delaunay, Voronoi, and the alpha-shape circumradius boundary into one content-keyed `SpatialReceipt`; numpy brute-force neighbour floor on cp315.

[SIMFRAME_MESH_FIELD]: REALIZED in `simframe/mesh_field.md` — `MeshField` lowers a weak form to the sparse stiffness/load pair through `skfem.Basis`/`asm`, `MeshExchange` rounds through meshio; assembly-and-interchange only, the solve stays on the quadrature route.

[RUNTIME_BOUNDARY_PARITY]: REALIZED across every sub-domain — `Receipt.of`/`BoundaryFault(<case>=...)`/`ContentIdentity.of` over the flat `rasm.runtime.{receipts,faults,content_identity}` imports, audited against the runtime owner pages with no fabricated factory.

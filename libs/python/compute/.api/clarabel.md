# [PY_COMPUTE_API_CLARABEL]

`clarabel` owns the Rust-native primal-dual interior-point solve of a quadratic-conic problem in standard form — sparse `P`/`q`/`A`/`b` with an ordered cone list — returning primal `x`, dual `z`, slack `s`, status, objective, and primal/dual residuals. `cvxpy` selects it as the default conic backend, and its dual `z` with the residual pair is the optimality certificate the compute convex-optimization rail reads. `compute` composes `DefaultSolver`, the cone constructors, `update`, and `solve`; the interior-point iteration stays Clarabel's.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `clarabel`
- package: `clarabel` (Apache-2.0)
- module: `import clarabel`
- rail: convex optimization — conic interior-point solver backend

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: solver, settings, solution, and info roots

| [INDEX] | [SYMBOL]          | [TYPE_FAMILY] | [CAPABILITY]                                                                         |
| :-----: | :---------------- | :------------ | :----------------------------------------------------------------------------------- |
|  [01]   | `DefaultSolver`   | class         | `(P, q, A, b, cones, settings)` construct; owns solve and warm `update`              |
|  [02]   | `DefaultSettings` | class         | tolerances, iteration/time caps, equilibration, presolve, KKT method, regularization |
|  [03]   | `DefaultSolution` | value-object  | primal `x`, dual `z`, slack `s`, status, `obj_val`/`obj_val_dual`, residuals         |
|  [04]   | `DefaultInfo`     | value-object  | `get_info()` convergence receipt: costs, gaps, residuals, ktratio, step, linsolver   |
|  [05]   | `SolverStatus`    | enum          | closed `DefaultSolution.status` verdict set                                          |

`[SolverStatus]`: `Unsolved` `Solved` `PrimalInfeasible` `DualInfeasible` `AlmostSolved` `AlmostPrimalInfeasible` `AlmostDualInfeasible` `MaxIterations` `MaxTime` `NumericalError` `InsufficientProgress` `CallbackTerminated`

[PUBLIC_TYPE_SCOPE]: cone constructors

| [INDEX] | [SYMBOL]                     | [CONE]                 | [CAPABILITY]                                         |
| :-----: | :--------------------------- | :--------------------- | :--------------------------------------------------- |
|  [01]   | `ZeroConeT(dim)`             | zero cone              | equality rows `s = 0`                                |
|  [02]   | `NonnegativeConeT(dim)`      | nonnegative cone       | elementwise `s >= 0`                                 |
|  [03]   | `SecondOrderConeT(dim)`      | second-order cone      | norm cone `s_0 >= norm2(s_1:)`                       |
|  [04]   | `ExponentialConeT()`         | exponential cone       | 3-dimensional exponential cone                       |
|  [05]   | `PowerConeT(alpha)`          | power cone             | 3-dimensional power cone with exponent `alpha`       |
|  [06]   | `GenPowerConeT(alpha, dim2)` | generalized power cone | `alpha` nonneg power vector + `dim2` 2-norm tail dim |
|  [07]   | `PSDTriangleConeT(dim)`      | semidefinite cone      | PSD constraint over the upper-triangle vectorization |

[PUBLIC_TYPE_SCOPE]: `DefaultSettings` tuning fields

| [INDEX] | [FIELD]                                                          | [ROLE]                                                      |
| :-----: | :--------------------------------------------------------------- | :---------------------------------------------------------- |
|  [01]   | `max_iter`                                                       | interior-point iteration cap                                |
|  [02]   | `time_limit`                                                     | wall-clock solve cap (seconds)                              |
|  [03]   | `verbose`                                                        | iteration-log printing                                      |
|  [04]   | `tol_gap_abs` / `tol_gap_rel`                                    | absolute/relative duality-gap termination tolerances        |
|  [05]   | `tol_feas`                                                       | primal/dual feasibility tolerance                           |
|  [06]   | `tol_infeas_abs` / `tol_infeas_rel`                              | infeasibility-certificate detection tolerances              |
|  [07]   | `tol_ktratio`                                                    | kappa/tau ratio termination tolerance                       |
|  [08]   | `equilibrate_enable`                                             | Ruiz equilibration of the KKT system                        |
|  [09]   | `presolve_enable`                                                | presolve elimination of redundant rows                      |
|  [10]   | `chordal_decomposition_enable`                                   | chordal sparsity decomposition for PSD cones                |
|  [11]   | `direct_kkt_solver` / `direct_solve_method`                      | direct KKT linear-system backend selection (`qdldl`/`faer`) |
|  [12]   | `static_regularization_enable` / `dynamic_regularization_enable` | KKT regularization toggles                                  |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: solve, warm-update, problem-load, and solution recovery

| [INDEX] | [SURFACE]                                         | [SHAPE]  | [CAPABILITY]                                           |
| :-----: | :------------------------------------------------ | :------- | :----------------------------------------------------- |
|  [01]   | `DefaultSolver(P, q, A, b, cones, settings)`      | ctor     | construct from standard-form sparse QP + ordered cones |
|  [02]   | `DefaultSolver.solve() -> DefaultSolution`        | instance | run the interior-point solve                           |
|  [03]   | `DefaultSolver.update(P=, q=, A=, b=, settings=)` | instance | in-place data/settings warm re-solve                   |
|  [04]   | `DefaultSolver.get_info() -> DefaultInfo`         | instance | detailed convergence/iteration receipt                 |
|  [05]   | `DefaultSolver.set_termination_callback(fn)`      | instance | early-stop hook; fires `CallbackTerminated`            |
|  [06]   | `load_from_file(filename, settings=None)`         | static   | load a serialized problem into a solver                |
|  [07]   | `get_infinity()` / `set_infinity(v)`              | static   | unbounded cone-bound sentinel threshold                |
|  [08]   | `DefaultSolution.x` / `z` / `s`                   | property | primal, dual, slack vectors                            |
|  [09]   | `DefaultSolution.status -> SolverStatus`          | property | termination verdict                                    |
|  [10]   | `DefaultSolution.obj_val` / `obj_val_dual`        | property | primal and dual objective values                       |
|  [11]   | `DefaultSolution.solve_time` / `iterations`       | property | wall-clock seconds and iteration count                 |
|  [12]   | `DefaultSolution.r_prim` / `r_dual`               | property | primal and dual residuals at termination               |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- One `DefaultSolver(P, q, A, b, cones, settings)` owns the solve of `min 0.5 xᵀPx + qᵀx s.t. Ax + s = b, s ∈ K`; `P` is the upper-triangular sparse cost, never a dense or full-symmetric duplicate.
- `cones` is the single ordered partition of the constraint rows — zero, nonnegative, second-order, exponential, power, generalized-power, and PSD memberships are cone rows whose dimensions sum to the row count, never a per-cone solver or manual slack reformulation.
- `DefaultSettings` carries tolerances, caps, equilibration, presolve, KKT method, and verbosity as one settings row; tune by field, never a parallel solver subclass.
- A sweep changing only `P`/`q`/`A`/`b` magnitudes re-solves through `update(...)` then `solve()`, reusing the symbolic factorization; rebuild a fresh `DefaultSolver` only when the sparsity pattern or cone partition changes.
- Each solve folds status, `x`, `z`, `s`, objective, iterations, time, and residuals into one conic-solve receipt; dual `z` with the residual pair is the optimality certificate, and `PrimalInfeasible`/`DualInfeasible` and their `Almost*` neighbours carry the infeasibility certificate.

[STACKING]:
- `cvxpy`(`.api/cvxpy.md`): `cp.Problem.solve(solver=cp.CLARABEL)` reduces a disciplined-convex model to the exact `(P, q, A, b, cones)` standard form; `get_problem_data(cp.CLARABEL)` exposes that reduction so an offline study drives `DefaultSolver` directly and reads the `DefaultSolution`/`DefaultInfo` receipt with no modeling layer in the hot loop.
- `scipy`(`.api/scipy.md`): `P`/`A` are `scipy.sparse.csc_matrix` — upper-triangular `P` via `scipy.sparse.triu(P).tocsc()`, the cone-block stack assembled with `scipy.sparse.vstack` in cone-list order.
- compute convex backend: `status`/`iterations`/`solve_time`/`r_prim`/`r_dual` and the richer `get_info()` fields fold into the conic-solve receipt handed across the graduation wire; dual `z` is the certificate the consumer reads, never recomputed.

[LOCAL_ADMISSION]:
- `clarabel` is the standalone conic path when the problem is already in cone-standard form, and the `cvxpy`-selected default conic backend for modeled problems.

[RAIL_LAW]:
- Package: `clarabel`
- Owns: primal-dual interior-point solve of quadratic-conic problems across the full cone set, in-place warm `update`, and primal/dual/infeasibility-certificate with residual recovery
- Accept: `DefaultSolver` over standard-form sparse `P`/`q`/`A`/`b` and an ordered `cones` list, `DefaultSettings` tuning, `update` warm re-solve, `DefaultSolution`/`DefaultInfo` recovery, use as the `cvxpy` default conic backend and dual-certificate source
- Reject: wrapper-renames of `DefaultSolver`/`solve`; a hand-rolled interior-point iteration where Clarabel is admitted; a dense or full-symmetric `P` where the upper-triangular sparse form is required; a per-cone solver family where the ordered cone list discriminates; rebuilding `DefaultSolver` per parameter value where `update` warm re-solve applies

# [PY_COMPUTE_API_CLARABEL]

`clarabel` supplies the Rust-native conic interior-point solver for the compute convex-optimization rail: a `DefaultSolver` that takes a quadratic-plus-conic problem in standard form (`P`, `q`, `A`, `b`, a cone list) plus `DefaultSettings`, runs the primal-dual interior-point method, and returns a `DefaultSolution` carrying the primal `x`, dual `z`/`s`, solve status, objective, and primal/dual residuals. It is the default conic backend behind `cvxpy` and the source of its dual-certificate proof of optimality. The package owner composes `DefaultSolver`, the cone constructors, `update`, and `solve` into the convex backend; it never re-implements the interior-point iteration Clarabel owns.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `clarabel`
- package: `clarabel`
- import: `import clarabel`
- owner: `compute`
- rail: convex optimization (conic solver backend)
- license: Apache-2.0 (Rust-native, pyo3/maturin binding)
- installed: marker-gated `python_version<'3.15'` — companion-band: transitively pulls `scipy` (gast/pythran lack CPython 3.15 support), so the wheel resolves only into the companion interpreter band until the scientific stack ships 3.15. The member surface below is authored from official documentation plus the cvxpy clarabel interface and reflection-verifies on uv sync into the companion band.
- entry points: none (library only)
- capability: primal-dual interior-point solve of quadratic programs over the zero, nonnegative, second-order, exponential, power, positive-semidefinite, and generalized-power cones; sparse CSC problem input; in-place data/settings `update` for warm re-solve; primal/dual solution recovery with infeasibility and unboundedness certificates and primal/dual residual reporting

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: solver, settings, and solution roots
- rail: convex optimization

| [INDEX] | [SYMBOL]          | [TYPE_FAMILY]  | [CAPABILITY]                                                                  |
| :-----: | :---------------- | :------------- | :---------------------------------------------------------------------------- |
|  [01]   | `DefaultSolver`   | solver         | `DefaultSolver(P, q, A, b, cones, settings)` — assembles and runs the solve   |
|  [02]   | `DefaultSettings` | settings       | tolerances, iteration cap, time limit, equilibration, presolve, KKT method    |
|  [03]   | `DefaultSolution` | result carrier | `x`, `z`, `s`, `status`, `obj_val`, `solve_time`, `iterations`, `r_prim`, `r_dual` |
|  [04]   | `SolverStatus`    | status enum    | `Unsolved`/`Solved`/`PrimalInfeasible`/`DualInfeasible`/`AlmostSolved`/`AlmostPrimalInfeasible`/`AlmostDualInfeasible`/`MaxIterations`/`MaxTime`/`NumericalError`/`InsufficientProgress` |

[PUBLIC_TYPE_SCOPE]: cone constructors
- rail: convex optimization

The `cones` argument is an ordered list of cone objects whose total dimension matches the row count of `A`/`b`; the row blocks of the constraint stack are partitioned in cone-list order. cvxpy's clarabel reduction emits exactly these constructors (`ZeroConeT`, `NonnegativeConeT`, `SecondOrderConeT`, `PSDTriangleConeT`, `ExponentialConeT`, `PowerConeT`, `GenPowerConeT`), so the cone names below are the consumer-confirmed surface.

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
- rail: convex optimization

`DefaultSettings()` constructs the default row; fields are set by attribute (`settings.verbose = True`, `settings.max_iter = 100`) — the Python interface mirrors the Rust settings surface. Tune by field, never by a parallel solver subclass.

| [INDEX] | [FIELD]                                                | [ROLE]                                                            |
| :-----: | :----------------------------------------------------- | :--------------------------------------------------------------- |
|  [01]   | `max_iter`                                             | interior-point iteration cap                                     |
|  [02]   | `time_limit`                                           | wall-clock solve cap (seconds)                                   |
|  [03]   | `verbose`                                              | iteration-log printing                                           |
|  [04]   | `tol_gap_abs` / `tol_gap_rel`                          | absolute/relative duality-gap termination tolerances             |
|  [05]   | `tol_feas`                                             | primal/dual feasibility tolerance                                |
|  [06]   | `tol_infeas_abs` / `tol_infeas_rel`                    | infeasibility-certificate detection tolerances                   |
|  [07]   | `tol_ktratio`                                          | kappa/tau ratio termination tolerance                            |
|  [08]   | `equilibrate_enable`                                   | Ruiz equilibration of the KKT system                             |
|  [09]   | `presolve_enable`                                      | presolve elimination of redundant rows                           |
|  [10]   | `chordal_decomposition_enable`                         | chordal sparsity decomposition for PSD cones                     |
|  [11]   | `direct_kkt_solver` / `direct_solve_method`            | direct KKT linear-system backend selection (`qdldl`/`faer`)      |
|  [12]   | `static_regularization_enable` / `dynamic_regularization_enable` | KKT regularization toggles                              |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: solve and warm-update entry points
- rail: convex optimization

`P` and `A` are `scipy.sparse.csc_matrix` (CSC); `P` is the upper-triangular quadratic cost, `q` the linear cost, and `A`/`b`/`cones` the conic constraint stack `A x + s = b, s in K`. `DefaultSolver.solve()` runs the interior-point method and returns the `DefaultSolution`; `DefaultSolver.update(P=, q=, A=, b=, settings=)` mutates problem data in place for a warm re-solve without reassembling the factorization symbolics — the standalone analogue of cvxpy DPP warm re-solve.

| [INDEX] | [SURFACE]                       | [CALL_SHAPE]                                  | [CAPABILITY]                               |
| :-----: | :------------------------------ | :-------------------------------------------- | :----------------------------------------- |
|  [01]   | `DefaultSolver`                 | `DefaultSolver(P, q, A, b, cones, settings)`  | construct from standard-form QP + cones    |
|  [02]   | `DefaultSolver.solve`           | `solve()` -> `DefaultSolution`                | run the interior-point solve               |
|  [03]   | `DefaultSolver.update`          | `update(P=, q=, A=, b=, settings=)`           | in-place data/settings update for re-solve |
|  [04]   | `DefaultSolution.x`             | attribute                                     | primal solution vector                     |
|  [05]   | `DefaultSolution.z`             | attribute                                     | dual solution (conic multipliers)          |
|  [06]   | `DefaultSolution.s`             | attribute                                     | primal slack vector                        |
|  [07]   | `DefaultSolution.status`        | attribute -> `SolverStatus`                   | termination status                         |
|  [08]   | `DefaultSolution.obj_val`       | attribute                                     | objective value at the returned point      |
|  [09]   | `DefaultSolution.solve_time`    | attribute                                     | wall-clock solve time (seconds)            |
|  [10]   | `DefaultSolution.iterations`    | attribute                                     | interior-point iteration count             |
|  [11]   | `DefaultSolution.r_prim`        | attribute                                     | primal residual at termination             |
|  [12]   | `DefaultSolution.r_dual`        | attribute                                     | dual residual at termination               |

## [04]-[IMPLEMENTATION_LAW]

[CONIC_SOLVE]:
- import: `import clarabel` at boundary scope only; module-level import is banned by the manifest import policy.
- problem axis: one `DefaultSolver(P, q, A, b, cones, settings)` owns the solve; the problem is standard-form `min 0.5 xᵀPx + qᵀx s.t. Ax + s = b, s in K` — supply the upper-triangular sparse `P`, never a dense or full-symmetric duplicate.
- cone axis: the `cones` list is the single ordered partition of the constraint rows; zero/nonnegative/second-order/exponential/power/generalized-power/PSD memberships are cone rows whose dimensions sum to the constraint count — never a per-cone solver or a manual slack reformulation.
- settings axis: `DefaultSettings` carries tolerances, iteration cap, time limit, equilibration, presolve, KKT method, and verbosity as one settings row; tune by field, never by a parallel solver subclass.
- warm axis: a parametrized sweep that changes only `P`/`q`/`A`/`b` magnitudes re-solves through `DefaultSolver.update(...)` then `solve()`, reusing the symbolic factorization; rebuild a fresh `DefaultSolver` only when the sparsity pattern or cone partition changes.
- backend axis: Clarabel is the default conic backend Cvxpy selects; direct construction is the standalone path when the problem is already in cone-standard form (e.g. from a Cvxpy `get_problem_data(cp.CLARABEL)` reduction whose `(data, chain, inverse)` triple yields the sparse `P`/`q`/`A`/`b` and the cone-dimension partition).
- evidence: each solve captures status, primal `x`, dual `z`, slack `s`, objective, iteration count, solve time, and primal/dual residuals `r_prim`/`r_dual` as a conic-solve receipt; the dual `z` plus the residual pair is the certificate of optimality, and `PrimalInfeasible`/`DualInfeasible` (and their `Almost*` neighbours) statuses carry the corresponding infeasibility certificate.
- boundary: Clarabel owns the interior-point solve and the dual certificate; Cvxpy owns the convex modeling and cone reduction above it; the graduation rail hands the offline solution across the wire; live UI stays outside this package.

[INTEGRATION_STACK]:
- cvxpy backend: `cp.Problem.solve(solver=cp.CLARABEL, ...)` reduces a disciplined-convex model to the exact `(P, q, A, b, cones)` standard form Clarabel consumes; cvxpy `get_problem_data(cp.CLARABEL)` exposes that reduction so a one-shot offline study can drive `DefaultSolver` directly and read the `DefaultSolution` residual receipt without the modeling layer in the hot loop.
- scipy seam: `P`/`A` are `scipy.sparse.csc_matrix`; the upper-triangular `P` is `scipy.sparse.triu(P).tocsc()` and the cone-block stack is assembled with `scipy.sparse.vstack` in cone-list order — Clarabel sits directly on the `scipy.md` sparse owner, never a hand-built CSC.
- receipt seam: `status`/`iterations`/`solve_time`/`r_prim`/`r_dual` fold into one conic-solve receipt the graduation owner hands across the wire; the dual `z` is the optimality certificate the downstream consumer reads, never recomputed.

[RAIL_LAW]:
- Package: `clarabel`
- Owns: primal-dual interior-point solve of quadratic-plus-conic problems with full cone coverage, in-place warm `update`, and primal/dual/infeasibility certificate plus residual recovery
- Accept: `DefaultSolver` over standard-form sparse `P`/`q`/`A`/`b` plus an ordered `cones` list, `DefaultSettings` tuning, `DefaultSolver.update` warm re-solve, `DefaultSolution` primal/dual/residual recovery, use as the Cvxpy default conic backend and dual-certificate source
- Reject: wrapper-renames of `DefaultSolver`/`solve`; a hand-rolled interior-point iteration where Clarabel is admitted; a dense or full-symmetric `P` where the upper-triangular sparse form is required; a per-cone solver family where the ordered cone list discriminates; a phantom `obj_val_dual` field (Clarabel reports one `obj_val` plus the `r_prim`/`r_dual` residual pair); rebuilding `DefaultSolver` per parameter value where `update` warm re-solve applies

# [PY_COMPUTE_API_CLARABEL]

`clarabel` supplies the Rust-native conic interior-point solver for the compute convex-optimization rail: a `DefaultSolver` that takes a quadratic-plus-conic problem in standard form (`P`, `q`, `A`, `b`, a cone list) plus `DefaultSettings`, runs the primal-dual interior-point method, and returns a `DefaultSolution` carrying the primal `x`, dual `z`/`s`, solve status, and objective. It is the default conic backend behind `cvxpy` and the source of its dual-certificate proof of optimality. The package owner composes `DefaultSolver`, the cone constructors, and `solve` into the convex backend; it never re-implements the interior-point iteration Clarabel owns.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `clarabel`
- package: `clarabel`
- import: `import clarabel`
- owner: `compute`
- rail: convex optimization (conic solver backend)
- installed: cp39-abi3 wheel (Rust-native, stable-ABI on CPython 3.15) — present in the lockfile but not yet synced into the active venv; RESEARCH-capture-pending-on-uv-sync, the member surface below is authored from official documentation and reflection-verifies on uv sync
- entry points: none (library only)
- capability: primal-dual interior-point solve of quadratic programs over the zero, nonnegative, second-order, exponential, power, positive-semidefinite, and generalized-power cones; sparse CSC problem input; pluggable settings; primal/dual solution recovery with infeasibility and unboundedness certificates

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: solver, settings, and solution roots
- rail: convex optimization

| [INDEX] | [SYMBOL]          | [TYPE_FAMILY]  | [CAPABILITY]                                                                  |
| :-----: | :---------------- | :------------- | :---------------------------------------------------------------------------- |
|  [01]   | `DefaultSolver`   | solver         | `DefaultSolver(P, q, A, b, cones, settings)` — assembles and runs the solve   |
|  [02]   | `DefaultSettings` | settings       | tolerances, iteration cap, equilibration, direct/indirect KKT, verbosity      |
|  [03]   | `DefaultSolution` | result carrier | `x`, `z`, `s`, `status`, `obj_val`, `obj_val_dual`, `solve_time`, iterations  |
|  [04]   | `SolverStatus`    | status enum    | `Solved`/`PrimalInfeasible`/`DualInfeasible`/`MaxIterations`/`AlmostSolved`/… |

[PUBLIC_TYPE_SCOPE]: cone constructors
- rail: convex optimization

The `cones` argument is an ordered list of cone objects whose total dimension matches the row count of `A`/`b`; the row blocks of the constraint stack are partitioned in cone-list order.

| [INDEX] | [SYMBOL]                     | [CONE]                 | [CAPABILITY]                                         |
| :-----: | :--------------------------- | :--------------------- | :--------------------------------------------------- |
|  [01]   | `ZeroConeT(dim)`             | zero cone              | equality rows `s = 0`                                |
|  [02]   | `NonnegativeConeT(dim)`      | nonnegative cone       | elementwise `s >= 0`                                 |
|  [03]   | `SecondOrderConeT(dim)`      | second-order cone      | norm cone `s_0 >= norm2(s_1:)`                       |
|  [04]   | `ExponentialConeT()`         | exponential cone       | 3-dimensional exponential cone                       |
|  [05]   | `PowerConeT(alpha)`          | power cone             | 3-dimensional power cone with exponent `alpha`       |
|  [06]   | `GenPowerConeT(alpha, dim2)` | generalized power cone | generalized power cone                               |
|  [07]   | `PSDTriangleConeT(dim)`      | semidefinite cone      | PSD constraint over the upper-triangle vectorization |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: solve entry point
- rail: convex optimization

`P` and `A` are `scipy.sparse.csc_matrix` (CSC); `P` is the upper-triangular quadratic cost, `q` the linear cost, and `A`/`b`/`cones` the conic constraint stack `A x + s = b, s in K`. `DefaultSolver.solve()` runs the interior-point method and stores the result; `DefaultSolver.solution` returns the `DefaultSolution`.

| [INDEX] | [SURFACE]                      | [CALL_SHAPE]                                 | [CAPABILITY]                               |
| :-----: | :----------------------------- | :------------------------------------------- | :----------------------------------------- |
|  [01]   | `DefaultSolver`                | `DefaultSolver(P, q, A, b, cones, settings)` | construct from standard-form QP + cones    |
|  [02]   | `DefaultSolver.solve`          | `solve()` -> `DefaultSolution`               | run the interior-point solve               |
|  [03]   | `DefaultSolution.x`            | attribute                                    | primal solution vector                     |
|  [04]   | `DefaultSolution.z`            | attribute                                    | dual solution (conic multipliers)          |
|  [05]   | `DefaultSolution.s`            | attribute                                    | primal slack vector                        |
|  [06]   | `DefaultSolution.status`       | attribute -> `SolverStatus`                  | termination status                         |
|  [07]   | `DefaultSolution.obj_val`      | attribute                                    | primal objective value                     |
|  [08]   | `DefaultSolution.obj_val_dual` | attribute                                    | dual objective value (gap = primal − dual) |

## [04]-[IMPLEMENTATION_LAW]

[CONIC_SOLVE]:
- import: `import clarabel` at boundary scope only; module-level import is banned by the manifest import policy.
- problem axis: one `DefaultSolver(P, q, A, b, cones, settings)` owns the solve; the problem is standard-form `min 0.5 xᵀPx + qᵀx s.t. Ax + s = b, s in K` — supply the upper-triangular sparse `P`, never a dense or full-symmetric duplicate.
- cone axis: the `cones` list is the single ordered partition of the constraint rows; zero/nonnegative/second-order/exponential/power/PSD memberships are cone rows whose dimensions sum to the constraint count — never a per-cone solver or a manual slack reformulation.
- settings axis: `DefaultSettings` carries tolerances, iteration cap, equilibration, KKT solver choice, and verbosity as one settings row; tune by field, never by a parallel solver subclass.
- backend axis: Clarabel is the default conic backend Cvxpy selects; direct construction is the standalone path when the problem is already in cone-standard form (e.g. from a Cvxpy `get_problem_data` reduction).
- evidence: each solve captures status, primal `x`, dual `z`, slack `s`, primal and dual objective, iteration count, and solve time as a conic-solve receipt; the dual `z` plus the primal/dual objective gap is the certificate of optimality, and `PrimalInfeasible`/`DualInfeasible` statuses carry the corresponding infeasibility certificate.
- boundary: Clarabel owns the interior-point solve and the dual certificate; Cvxpy owns the convex modeling and cone reduction above it; the graduation rail hands the offline solution across the wire; live UI stays outside this package.

[RAIL_LAW]:
- Package: `clarabel`
- Owns: primal-dual interior-point solve of quadratic-plus-conic problems with full cone coverage and primal/dual/infeasibility certificate recovery
- Accept: `DefaultSolver` over standard-form sparse `P`/`q`/`A`/`b` plus an ordered `cones` list, `DefaultSettings` tuning, `DefaultSolution` primal/dual recovery, use as the Cvxpy default conic backend and dual-certificate source
- Reject: wrapper-renames of `DefaultSolver`/`solve`; a hand-rolled interior-point iteration where Clarabel is admitted; a dense or full-symmetric `P` where the upper-triangular sparse form is required; a per-cone solver family where the ordered cone list discriminates

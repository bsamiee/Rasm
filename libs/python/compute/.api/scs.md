# [PY_COMPUTE_API_SCS]

`scs` supplies the first-order operator-splitting conic solver completing the compute convex-backend family beside `clarabel`: an `SCS` workspace taking a quadratic-plus-conic problem as `data`/`cone` dicts and keyword settings, running Douglas-Rachford ADMM on the homogeneous self-dual embedding, returning primal `x`, dual `y`, slack `s`, and an `info` receipt. Clarabel's interior-point arm wins accuracy on small-to-medium problems; SCS splitting scales to large sparse cone programs at moderate accuracy, and cvxpy selects either arm of the one backend. Package owner composes `SCS`, the cone dict, `solve`, and `update`, never re-implementing the splitting iteration SCS owns.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `scs`
- package: `scs`
- import: `import scs`
- owner: `compute`
- rail: convex optimization (first-order splitting conic solver backend)
- license: MIT (C core, meson-python native binding, macOS Accelerate BLAS)
- entry points: none (library only)
- capability: Douglas-Rachford ADMM solve of quadratic programs over the zero, nonnegative, box, second-order, semidefinite, exponential (primal and dual), and power cones; sparse CSC problem input as a `data` dict; in-place `b`/`c` `update` for warm re-solve; primal/dual/slack recovery with infeasibility and unboundedness certificates and an `info` residual receipt; single-precision, 64-bit-integer, MKL-linked, and CSV/data-dump build and run variants

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: solver, data, cone, and solution roots
- rail: convex optimization

SCS carries a dict-shaped surface rather than typed objects: `data`, `cone`, and the returned `info` are plain dicts, and settings are constructor keywords. `SCS(data, cone, **settings)` builds the workspace once; `solve()` runs the splitting and returns the solution dict.

| [INDEX] | [SYMBOL]         | [TYPE_FAMILY]  | [CAPABILITY]                                                                            |
| :-----: | :--------------- | :------------- | :------------------------------------------------------------------------------------- |
|  [01]   | `SCS`            | solver         | `SCS(data, cone, **settings)` — assembles the workspace and owns `solve`/`update`       |
|  [02]   | `data`           | problem dict   | keys `P` (upper-triangular sparse quadratic), `A` (sparse constraint), `b`, `c`         |
|  [03]   | `cone`           | cone dict      | ordered cone-block partition of the constraint rows; keys fenced below                  |
|  [04]   | solution dict    | result carrier | keys `x` (primal), `y` (dual), `s` (slack), `info` (receipt)                            |
|  [05]   | `info`           | receipt dict   | `status`, `status_val`, `iter`, objectives, residuals, and timing fields fenced below   |

[PUBLIC_TYPE_SCOPE]: cone dict keys
- rail: convex optimization

`cone` is a single dict whose entries partition the constraint rows of `A`/`b` in fixed cone order: zero, then nonnegative, then box, then the second-order list, then the semidefinite list, then primal-exponential, then dual-exponential, then the power list. Total cone dimension must equal the row count of `A`; cvxpy's SCS reduction emits exactly this dict.

| [INDEX] | [KEY]     | [CONE]                     | [CAPABILITY]                                                        |
| :-----: | :-------- | :------------------------- | :----------------------------------------------------------------- |
|  [01]   | `z`       | zero cone                  | equality rows `s = 0`; dimension count                             |
|  [02]   | `l`       | nonnegative cone           | elementwise `s >= 0`; dimension count                             |
|  [03]   | `bl`/`bu` | box cone bounds            | lower/upper bound vectors for the box cone                        |
|  [04]   | `bsize`   | box cone size              | box-cone block length (`1 + len(bl)`)                             |
|  [05]   | `q`       | second-order cone list     | list of SOC block dimensions `s_0 >= norm2(s_1:)`                 |
|  [06]   | `s`       | semidefinite cone list     | list of PSD side dimensions over the upper-triangle vectorization |
|  [07]   | `ep`      | exponential cone count     | number of primal 3-dimensional exponential cones                 |
|  [08]   | `ed`      | dual exponential cone count| number of dual 3-dimensional exponential cones                   |
|  [09]   | `p`       | power cone list            | list of power-cone exponents (positive primal, negative dual)     |

[PUBLIC_TYPE_SCOPE]: `info` receipt keys
- rail: convex optimization

Every `solve()` returns `info` as the solve receipt; `status` is the human string, `status_val` the closed integer verdict enumerated by the module constants (`SOLVED`, `SOLVED_INACCURATE`, `UNBOUNDED`, `UNBOUNDED_INACCURATE`, `INFEASIBLE`, `INFEASIBLE_INACCURATE`, `INDETERMINATE`, `FAILED`, `SIGINT`, `UNFINISHED`).

| [INDEX] | [FIELD]                                      | [ROLE]                                                     |
| :-----: | :------------------------------------------- | :-------------------------------------------------------- |
|  [01]   | `status` / `status_val`                      | verdict string and its closed integer code                |
|  [02]   | `iter`                                        | ADMM iteration count at termination                       |
|  [03]   | `pobj` / `dobj`                               | primal and dual objective values                          |
|  [04]   | `res_pri` / `res_dual` / `gap`                | primal residual, dual residual, duality gap               |
|  [05]   | `res_infeas` / `res_unbdd_a` / `res_unbdd_p`  | infeasibility and unboundedness certificate residuals     |
|  [06]   | `comp_slack`                                  | complementary-slackness residual                          |
|  [07]   | `setup_time` / `solve_time`                   | workspace-setup and solve wall-clock (milliseconds)       |
|  [08]   | `cone_time` / `lin_sys_time` / `accel_time`   | per-phase timing of cone projection, KKT solve, Anderson  |
|  [09]   | `scale` / `scale_updates`                     | final adaptive-scale value and its update count           |
|  [10]   | `accepted_accel_steps` / `rejected_accel_steps` | Anderson-acceleration accept/reject tallies            |

[PUBLIC_TYPE_SCOPE]: constructor settings
- rail: convex optimization

Settings are constructor keywords on `SCS(data, cone, ...)`; there is no settings object and no solver subclass. Tune by keyword.

| [INDEX] | [SETTING]                                   | [ROLE]                                                        |
| :-----: | :------------------------------------------ | :----------------------------------------------------------- |
|  [01]   | `max_iters`                                 | ADMM iteration cap                                           |
|  [02]   | `time_limit_secs`                           | wall-clock solve cap (seconds; `0` disables)                |
|  [03]   | `eps_abs` / `eps_rel`                       | absolute/relative termination tolerances                    |
|  [04]   | `eps_infeas`                                | infeasibility-certificate detection tolerance               |
|  [05]   | `alpha`                                      | Douglas-Rachford relaxation parameter                       |
|  [06]   | `rho_x`                                      | primal-variable regularization                              |
|  [07]   | `scale` / `adaptive_scale`                  | initial dual scaling and its adaptive-update toggle         |
|  [08]   | `normalize`                                  | data equilibration toggle                                   |
|  [09]   | `acceleration_lookback` / `acceleration_interval` | Anderson-acceleration memory depth and cadence        |
|  [10]   | `mkl`                                        | select the MKL Pardiso direct linear-system backend         |
|  [11]   | `verbose`                                    | iteration-log printing                                      |
|  [12]   | `write_data_filename` / `log_csv_filename`   | problem-data dump and per-iteration CSV trace paths         |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: solve, warm-update, and one-shot entry points
- rail: convex optimization

`data["P"]` and `data["A"]` are `scipy.sparse.csc_matrix`; `P` is the upper-triangular quadratic cost, `c` the linear cost, and `A`/`b`/`cone` the conic constraint stack `A x + s = b, s in K`. `SCS(data, cone, **settings).solve()` runs the splitting and returns the solution dict; `solve(warm_start=True, x=, y=, s=)` warm-starts from the prior solution or an explicit override; `SCS.update(b=, c=)` mutates the linear terms in place for a warm re-solve reusing the factorization. Module-level `scs.solve(data, cone, **settings)` is the one-shot analogue for a single non-repeated solve.

| [INDEX] | [SURFACE]        | [CALL_SHAPE]                                | [CAPABILITY]                                       |
| :-----: | :--------------- | :------------------------------------------ | :------------------------------------------------- |
|  [01]   | `SCS`            | `SCS(data, cone, **settings)`               | construct the workspace from standard-form dicts   |
|  [02]   | `SCS.solve`      | `solve(warm_start=, x=, y=, s=)` -> dict     | run the ADMM splitting, optionally warm-started     |
|  [03]   | `SCS.update`     | `update(b=, c=)`                            | in-place `b`/`c` update for warm re-solve          |
|  [04]   | `scs.solve`      | `scs.solve(data, cone, **settings)` -> dict  | module-level one-shot solve                        |
|  [05]   | solution `x`     | dict key                                    | primal solution vector                             |
|  [06]   | solution `y`     | dict key                                    | dual solution (conic multipliers)                  |
|  [07]   | solution `s`     | dict key                                    | primal slack vector                                |
|  [08]   | solution `info`  | dict key                                    | the solve receipt fenced above                     |

## [04]-[IMPLEMENTATION_LAW]

[CONIC_SOLVE]:
- import: `import scs` at boundary scope only; module-level import is banned by the manifest import policy.
- problem axis: one `SCS(data, cone, **settings)` owns the solve; the problem is standard-form `min 0.5 xᵀPx + cᵀx s.t. Ax + s = b, s in K` — supply the upper-triangular sparse `P`, never a dense or full-symmetric duplicate.
- cone axis: `cone` is the single ordered dict partition of the constraint rows; zero/nonnegative/box/second-order/semidefinite/exponential/dual-exponential/power memberships are cone entries whose dimensions sum to the constraint count — never a per-cone solver or a manual slack reformulation.
- settings axis: constructor keywords carry tolerances, iteration cap, time limit, splitting relaxation, acceleration, scaling, linear-backend, and verbosity as one keyword row; tune by keyword, never by a parallel solver subclass.
- warm axis: a parametrized sweep that changes only the linear terms re-solves through `SCS.update(b=, c=)` then `solve()`, reusing the KKT factorization; a change to `P`/`A` sparsity or the cone partition rebuilds a fresh `SCS` workspace — the warm boundary is narrower than Clarabel's `P`/`q`/`A`/`b` update and is the discriminant between the two backends.
- method axis: SCS runs first-order Douglas-Rachford ADMM on the homogeneous self-dual embedding, trading interior-point accuracy for large-sparse scale; select SCS over Clarabel when the cone program is large and sparse and moderate accuracy suffices, Clarabel when high accuracy on a small-to-medium problem is required.
- evidence: each solve captures `status`/`status_val`, primal `x`, dual `y`, slack `s`, primal and dual objectives, iteration count, per-phase timings, and the residual set (`res_pri`/`res_dual`/`gap`/`comp_slack` with the infeasibility/unboundedness residuals) as a conic-solve receipt; the dual `y` with the residual pair is the certificate of optimality, and the `INFEASIBLE`/`UNBOUNDED` statuses (and their `_INACCURATE` neighbours) carry the corresponding certificate residual.
- boundary: SCS owns the operator-splitting solve and the dual certificate; cvxpy owns the convex modeling and cone reduction above it; the graduation rail hands the offline solution across the wire; live UI stays outside this package.

[INTEGRATION_STACK]:
- cvxpy backend: `cp.Problem.solve(solver=cp.SCS, ...)` reduces a disciplined-convex model to the exact `data`/`cone` dict form SCS consumes; cvxpy `get_problem_data(cp.SCS)` exposes that reduction so a one-shot offline study drives `SCS` directly and reads the `info` residual receipt without the modeling layer in the hot loop. SCS is the first-order arm and Clarabel the interior-point arm of the one convex backend family; the caller selects by problem scale and accuracy demand.
- scipy seam: `data["P"]`/`data["A"]` are `scipy.sparse.csc_matrix`; the upper-triangular `P` is `scipy.sparse.triu(P).tocsc()` and the cone-block stack is assembled with `scipy.sparse.vstack` in cone-dict order — SCS sits directly on the `scipy.md` sparse owner, never a hand-built CSC.
- receipt seam: `status`/`iter`/`solve_time`/`res_pri`/`res_dual` fold into one conic-solve receipt the graduation owner hands across the wire; the dual `y` is the optimality certificate the downstream consumer reads, never recomputed. Receipt shape aligns with the Clarabel receipt so the convex backend emits one uniform conic-solve evidence row regardless of the arm that produced it.

[RAIL_LAW]:
- Package: `scs`
- Owns: first-order operator-splitting solve of quadratic-plus-conic problems with zero/nonnegative/box/second-order/semidefinite/exponential/dual-exponential/power cone coverage, in-place `b`/`c` warm `update`, and primal/dual/slack with infeasibility-certificate and residual recovery
- Accept: `SCS` over standard-form sparse `data` and a `cone` dict, keyword settings tuning, `SCS.update` warm re-solve on `b`/`c`, solution-dict primal/dual/slack/`info` recovery, use as the cvxpy first-order conic backend beside Clarabel for large-sparse moderate-accuracy problems
- Reject: wrapper-renames of `SCS`/`solve`; a hand-rolled splitting iteration where SCS is admitted; a dense or full-symmetric `P` where the upper-triangular sparse form is required; a per-cone solver family where the ordered `cone` dict discriminates; a phantom typed settings object or solution object (SCS carries dict-shaped `data`/`cone`/`info` surfaces); a warm `update` of `P`/`A` where only `b`/`c` are updatable and a sparsity change demands a fresh workspace; choosing SCS for a small high-accuracy problem where Clarabel is the correct arm

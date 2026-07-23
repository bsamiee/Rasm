# [PY_COMPUTE_API_SCS]

`scs` owns the first-order operator-splitting solve of a quadratic-plus-conic problem — a sparse `data` dict (`P`/`A`/`b`/`c`) and an ordered `cone` dict — running Douglas-Rachford ADMM on the homogeneous self-dual embedding and returning primal `x`, dual `y`, slack `s`, and an `info` residual receipt. `cvxpy` selects it as the first-order conic backend, and the dual `y` with the residual pair is the optimality certificate the compute convex-optimization rail reads. `compute` composes `SCS`, the `cone` dict, `solve`, and `update`; the splitting iteration stays SCS's.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `scs`
- package: `scs` (MIT, meson-python native C binding)
- module: `import scs`
- rail: convex optimization — first-order operator-splitting conic solver backend

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: solver, data, cone, and solution roots

| [INDEX] | [SYMBOL] | [TYPE_FAMILY] | [CAPABILITY]                                                                    |
| :-----: | :------- | :------------ | :------------------------------------------------------------------------------ |
|  [01]   | `SCS`    | class         | `SCS(data, cone, **settings)` assembles the workspace and owns `solve`/`update` |
|  [02]   | `data`   | dict          | keys `P` (upper-triangular sparse quadratic), `A` (sparse constraint), `b`, `c` |
|  [03]   | `cone`   | dict          | ordered cone-block partition of the constraint rows; keys below                 |
|  [04]   | solution | dict          | keys `x` (primal), `y` (dual), `s` (slack), `info` (receipt)                    |
|  [05]   | `info`   | dict          | `status`, `status_val`, `iter`, objectives, residuals, and timings; keys below  |

[PUBLIC_TYPE_SCOPE]: `cone` dict keys

`cone` partitions the constraint rows of `A`/`b` in fixed cone order; total cone dimension equals the row count of `A`.

| [INDEX] | [SYMBOL]  | [CONE]                      | [CAPABILITY]                                                      |
| :-----: | :-------- | :-------------------------- | :---------------------------------------------------------------- |
|  [01]   | `z`       | zero cone                   | equality rows `s = 0`; dimension count                            |
|  [02]   | `l`       | nonnegative cone            | elementwise `s >= 0`; dimension count                             |
|  [03]   | `bl`/`bu` | box cone bounds             | lower/upper bound vectors for the box cone                        |
|  [04]   | `bsize`   | box cone size               | box-cone block length (`1 + len(bl)`)                             |
|  [05]   | `q`       | second-order cone list      | list of SOC block dimensions `s_0 >= norm2(s_1:)`                 |
|  [06]   | `s`       | semidefinite cone list      | list of PSD side dimensions over the upper-triangle vectorization |
|  [07]   | `ep`      | exponential cone count      | number of primal 3-dimensional exponential cones                  |
|  [08]   | `ed`      | dual exponential cone count | number of dual 3-dimensional exponential cones                    |
|  [09]   | `p`       | power cone list             | list of power-cone exponents (positive primal, negative dual)     |

[PUBLIC_TYPE_SCOPE]: `info` receipt keys

`status_val` is the closed integer verdict, one of the module status constants:

[STATUS_CONSTANTS]: `SOLVED` `SOLVED_INACCURATE` `UNBOUNDED` `UNBOUNDED_INACCURATE` `INFEASIBLE` `INFEASIBLE_INACCURATE` `INDETERMINATE` `FAILED` `SIGINT` `UNFINISHED`

| [INDEX] | [FIELD]                                         | [ROLE]                                                   |
| :-----: | :---------------------------------------------- | :------------------------------------------------------- |
|  [01]   | `status` / `status_val`                         | verdict string and its closed integer code               |
|  [02]   | `iter`                                          | ADMM iteration count at termination                      |
|  [03]   | `pobj` / `dobj`                                 | primal and dual objective values                         |
|  [04]   | `res_pri` / `res_dual` / `gap`                  | primal residual, dual residual, duality gap              |
|  [05]   | `res_infeas` / `res_unbdd_a` / `res_unbdd_p`    | infeasibility and unboundedness certificate residuals    |
|  [06]   | `comp_slack`                                    | complementary-slackness residual                         |
|  [07]   | `setup_time` / `solve_time`                     | workspace-setup and solve wall-clock (milliseconds)      |
|  [08]   | `cone_time` / `lin_sys_time` / `accel_time`     | per-phase timing of cone projection, KKT solve, Anderson |
|  [09]   | `scale` / `scale_updates`                       | final adaptive-scale value and its update count          |
|  [10]   | `accepted_accel_steps` / `rejected_accel_steps` | Anderson-acceleration accept/reject tallies              |

[PUBLIC_TYPE_SCOPE]: constructor settings

| [INDEX] | [FIELD]                                           | [ROLE]                                              |
| :-----: | :------------------------------------------------ | :-------------------------------------------------- |
|  [01]   | `max_iters`                                       | ADMM iteration cap                                  |
|  [02]   | `time_limit_secs`                                 | wall-clock solve cap (seconds; `0` disables)        |
|  [03]   | `eps_abs` / `eps_rel`                             | absolute/relative termination tolerances            |
|  [04]   | `eps_infeas`                                      | infeasibility-certificate detection tolerance       |
|  [05]   | `alpha`                                           | Douglas-Rachford relaxation parameter               |
|  [06]   | `rho_x`                                           | primal-variable regularization                      |
|  [07]   | `scale` / `adaptive_scale`                        | initial dual scaling and its adaptive-update toggle |
|  [08]   | `normalize`                                       | data equilibration toggle                           |
|  [09]   | `acceleration_lookback` / `acceleration_interval` | Anderson-acceleration memory depth and cadence      |
|  [10]   | `mkl`                                             | select the MKL Pardiso direct linear-system backend |
|  [11]   | `verbose`                                         | iteration-log printing                              |
|  [12]   | `write_data_filename` / `log_csv_filename`        | problem-data dump and per-iteration CSV trace paths |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: construct, solve, warm-update, and solution recovery

| [INDEX] | [SURFACE]                                    | [SHAPE]  | [CAPABILITY]                                     |
| :-----: | :------------------------------------------- | :------- | :----------------------------------------------- |
|  [01]   | `SCS(data, cone, **settings)`                | ctor     | construct the workspace from standard-form dicts |
|  [02]   | `SCS.solve(warm_start=, x=, y=, s=) -> dict` | instance | run the ADMM splitting, optionally warm-started  |
|  [03]   | `SCS.update(b=, c=)`                         | instance | in-place `b`/`c` update for warm re-solve        |
|  [04]   | `scs.solve(data, cone, **settings) -> dict`  | static   | module-level one-shot solve                      |
|  [05]   | solution `x` / `y` / `s`                     | dict key | primal, dual conic multipliers, primal slack     |
|  [06]   | solution `info`                              | dict key | the solve-receipt dict                           |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- One `SCS(data, cone, **settings)` owns the Douglas-Rachford ADMM solve of `min 0.5 xᵀPx + cᵀx s.t. Ax + s = b, s ∈ K` on the homogeneous self-dual embedding; `P` is the upper-triangular sparse cost, never a dense or full-symmetric duplicate.
- `cone` is the single ordered partition of the constraint rows — zero, nonnegative, box, second-order, semidefinite, exponential, dual-exponential, and power memberships are cone entries whose dimensions sum to the row count, never a per-cone solver or manual slack reformulation.
- Constructor keywords carry tolerances, caps, splitting relaxation, acceleration, scaling, linear backend, and verbosity as one keyword row; tune by keyword, never a parallel solver subclass.
- A sweep changing only the linear terms re-solves through `SCS.update(b=, c=)` then `solve()`, reusing the KKT factorization; a change to `P`/`A` sparsity or the cone partition rebuilds a fresh `SCS` — the warm boundary is narrower than Clarabel's `P`/`q`/`A`/`b` update and discriminates the two backends.
- Each solve folds `status`/`status_val`, `x`, `y`, `s`, objectives, iterations, per-phase timings, and residuals into one conic-solve receipt; dual `y` with the residual pair is the optimality certificate, and `INFEASIBLE`/`UNBOUNDED` and their `_INACCURATE` neighbours carry the corresponding certificate residual.

[STACKING]:
- `cvxpy`(`.api/cvxpy.md`): `cp.Problem.solve(solver=cp.SCS)` reduces a disciplined-convex model to the exact `data`/`cone` dict form; `get_problem_data(cp.SCS)` exposes that reduction so an offline study drives `SCS` directly and reads the `info` residual receipt with no modeling layer in the hot loop.
- `scipy`(`.api/scipy.md`): `data["P"]`/`data["A"]` are `scipy.sparse.csc_matrix` — upper-triangular `P` via `scipy.sparse.triu(P).tocsc()`, the cone-block stack assembled with `scipy.sparse.vstack` in cone-dict order.
- compute convex backend: `status`/`iter`/`solve_time`/`res_pri`/`res_dual` fold into the conic-solve receipt handed across the graduation wire; dual `y` is the certificate the consumer reads, never recomputed. Receipt shape aligns with the Clarabel receipt so the backend emits one uniform conic-solve row regardless of arm.

[LOCAL_ADMISSION]:
- `scs` is the `cvxpy`-selected first-order conic backend beside Clarabel's interior-point arm; select SCS when the cone program is large, sparse, and moderate accuracy suffices, Clarabel when high accuracy on a small-to-medium problem is required.

[RAIL_LAW]:
- Package: `scs`
- Owns: first-order operator-splitting solve of quadratic-plus-conic problems across the full cone set, in-place `b`/`c` warm `update`, and primal/dual/slack with infeasibility-certificate and residual recovery
- Accept: `SCS` over standard-form sparse `data` and a `cone` dict, keyword settings tuning, `SCS.update` warm re-solve on `b`/`c`, solution-dict primal/dual/slack/`info` recovery, use as the `cvxpy` first-order conic backend beside Clarabel
- Reject: wrapper-renames of `SCS`/`solve`; a hand-rolled splitting iteration where SCS is admitted; a dense or full-symmetric `P` where the upper-triangular sparse form is required; a per-cone solver family where the ordered `cone` dict discriminates; a phantom typed settings or solution object where SCS carries dict-shaped `data`/`cone`/`info` surfaces; a warm `update` of `P`/`A` where only `b`/`c` are updatable; choosing SCS for a small high-accuracy problem where Clarabel is the correct arm

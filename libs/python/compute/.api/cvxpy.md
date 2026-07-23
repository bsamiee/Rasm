# [PY_COMPUTE_API_CVXPY]

`cvxpy` mints the disciplined-convex-programming modeling algebra for the compute convex rail: `Variable`/`Parameter`/`Expression` terms compose under `Minimize`/`Maximize` and relational/cone constraints into a `Problem`, which `solve` compiles to conic form and dispatches to a pluggable backend (Clarabel by default), recovering the optimal value, primal values, and dual certificates. It owns the geometric (`gp`) and quasiconvex (`qcp`) modes, mixed-integer solves, and DPP warm re-solve; the backend owns the interior-point solve, never re-derived here.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `cvxpy`
- package: `cvxpy`
- import: `import cvxpy as cp`
- owner: `compute`
- rail: convex optimization
- license: Apache-2.0
- entry points: none (library only)
- capability: disciplined convex/geometric/quasiconvex modeling with mixed-integer support, multi-backend conic solve with primal/dual recovery, and DPP warm re-solve

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: leaf, expression, objective, and problem roots
- rail: convex optimization

| [INDEX] | [SYMBOL]     | [TYPE_FAMILY]     | [CAPABILITY]                                                                                 |
| :-----: | :----------- | :---------------- | :------------------------------------------------------------------------------------------- |
|  [01]   | `Variable`   | decision leaf     | `Variable(shape=(), name=None, *, <domain attrs>)` — domain flags in the leaf-attribute law  |
|  [02]   | `Parameter`  | symbolic constant | `Parameter(shape=(), name=None, value=None, **attrs)` — DPP parameter; enables warm re-solve |
|  [03]   | `Constant`   | constant leaf     | a fixed numeric value lifted into the expression algebra                                     |
|  [04]   | `Expression` | expression node   | atom-algebra base (`+`/`@`/`*`); carries `.curvature`/`.sign`/`.shape`/`.value`              |
|  [05]   | `Minimize`   | objective         | `Minimize(expr)` — convex objective                                                          |
|  [06]   | `Maximize`   | objective         | `Maximize(expr)` — concave objective                                                         |
|  [07]   | `Problem`    | problem root      | `Problem(objective, constraints=[])` — compiled and solved                                   |

[PUBLIC_TYPE_SCOPE]: constraint and cone types
- rail: convex optimization

| [INDEX] | [SYMBOL]            | [TYPE_FAMILY]     | [CAPABILITY]                                                         |
| :-----: | :------------------ | :---------------- | :------------------------------------------------------------------- |
|  [01]   | `Constraint`        | constraint base   | abstract base produced by relational operators on expressions        |
|  [02]   | `Zero`              | equality cone     | `expr == 0` membership                                               |
|  [03]   | `NonNeg`            | inequality cone   | elementwise `expr >= 0` membership                                   |
|  [04]   | `SOC`               | second-order cone | `SOC(t, X, axis=0)` — `norm2(X) <= t` second-order-cone membership   |
|  [05]   | `PSD`               | semidefinite cone | `expr >> 0` — symmetric PSD constraint                               |
|  [06]   | `ExpCone`           | exponential cone  | `ExpCone(x, y, z)` membership                                        |
|  [07]   | `PowCone3D`         | power cone        | `PowCone3D(x, y, z, alpha)` three-dimensional power-cone membership  |
|  [08]   | `PowConeND`         | power cone (N-D)  | `PowConeND(W, z, alpha, axis=0)` n-dimensional power-cone membership |
|  [09]   | `FiniteSet`         | combinatorial     | `FiniteSet(expr, vec)` — membership in a finite value set (MI)       |
|  [10]   | `OpRelEntrConeQuad` | spectral cone     | operator-relative-entropy cone (quantum/spectral)                    |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: problem construction and solve
- rail: convex optimization
- solve carry: `solver`, `warm_start`, `verbose`, `gp`, `qcp`, `requires_grad`, `enforce_dpp`, `ignore_dpp`, `canon_backend`, `**kwargs`

Relational operators `==`/`<=`/`>=`/`>>`/`<<` on `Expression` build `Constraint` objects; `Problem.solve` selects the backend by `solver`, returns the optimal value, and writes primal values to `Variable.value` and duals to `Constraint.dual_value`. `gp=True` selects DGP geometric programming; `qcp=True` selects DQCP quasiconvex programming.

| [INDEX] | [SURFACE]                                                 | [CAPABILITY]                                                  |
| :-----: | :-------------------------------------------------------- | :------------------------------------------------------------ |
|  [01]   | `Problem.solve(solver=None, ..., **kwargs) -> float`      | compile+solve; returns the optimal value (lead)               |
|  [02]   | `Problem.value`                                           | property; optimal objective value after solve                 |
|  [03]   | `Problem.status -> str`                                   | `optimal`/`infeasible`/`unbounded`/`*_inaccurate`             |
|  [04]   | `Variable.value`                                          | property (settable for warm start); primal after solve        |
|  [05]   | `Constraint.dual_value`                                   | per-cone dual certificate; layouts in `[05]` below            |
|  [06]   | `Constraint.args -> list[Expression]`                     | universal operand list; per-cone shapes in `[06]` below       |
|  [07]   | `Inequality.expr -> Expression`                           | relational `lhs − rhs` residual; absent on cone rows          |
|  [08]   | `installed_solvers() -> list[str]`                        | available solver backend names (`CLARABEL`, …)                |
|  [09]   | `Problem.get_problem_data(solver, gp=False, ...)`         | reduced conic `(data, chain, inverse_data)` for backend drive |
|  [10]   | `Problem.backward` / `Problem.derivative`                 | differentiate w.r.t. `Parameter` (needs grad)                 |
|  [11]   | `Problem.is_dcp` / `is_dgp` / `is_dqcp` / `is_qp` -> bool | curvature-ruleset classification before solve                 |
|  [12]   | `Problem.parameters` / `Problem.variables`                | enumerate leaves for sweep wiring                             |

- [05]-[DUAL_VALUE]: an `SOC(t, X)` dual is the stacked `[t_dual, X_dual...]` (reshaped per-cone to `(num_cones, 1+dim(X))`); a `PSD` dual is the symmetric `Z` of the primal `(n, n)` shape — the `tr(Z·X)` slackness and `λ_min` feasibility reads consume these.
- [06]-[ARGS]: `Constraint.args` is the universal operand list — `[lhs, rhs]` for `Inequality`/`Equality`, `[t, X]` for `SOC`, `[X]` for `PSD`, `[x, y, z]` for `ExpCone`; a cone row reads its primal through `constraint.args[i].value`.
- [07]-[EXPR]: `Inequality.expr` is the relational-only `lhs − rhs` residual (`<= 0` at feasibility), absent on `SOC`/`PSD`/`ExpCone`; a uniform `Constraint.expr.value` raises `AttributeError` on every cone row.

[ENTRYPOINT_SCOPE]: convex atom library (`cp.<atom>`)
- rail: convex optimization

| [INDEX] | [FAMILY]          | [CAPABILITY]                                                    |
| :-----: | :---------------- | :-------------------------------------------------------------- |
|  [01]   | affine atoms      | affine combinators preserving curvature                         |
|  [02]   | norm atoms        | convex norms, nuclear/spectral norm, total variation            |
|  [03]   | quadratic atoms   | quadratic, matrix-fractional, and robust losses                 |
|  [04]   | log/exp atoms     | exponential-cone-representable and entropy atoms                |
|  [05]   | elementwise/order | extrema, powers, order-statistic and risk atoms                 |
|  [06]   | matrix/spectral   | eigenvalue, log-det, and spectral matrix atoms                  |
|  [07]   | DGP/geometric     | log-log-convex atoms for `gp=True` geometric programs           |
|  [08]   | transform/support | support functions, partial optimization, statistical reductions |

- [01]-[AFFINE]: `sum`, `trace`, `multiply`, `matmul`, `reshape`, `vstack`, `hstack`, `stack`, `diag`, `kron`, `convolve`, `cumsum`, `diff`, `bmat`, `outer`, `vec`, `upper_tri`, `vec_to_upper_tri`, `real`, `imag`, `conj`
- [02]-[NORM]: `norm`, `norm1`, `norm2`, `norm_inf`, `pnorm`, `mixed_norm`, `normNuc`, `sigma_max`, `tv`
- [03]-[QUADRATIC]: `quad_form`, `sum_squares`, `quad_over_lin`, `matrix_frac`, `huber`, `tr_inv`
- [04]-[LOG_EXP]: `exp`, `log`, `log1p`, `log_sum_exp`, `entr`, `kl_div`, `rel_entr`, `logistic`, `log_normcdf`, `loggamma`, `xexp`, `von_neumann_entr`
- [05]-[ELEMENTWISE]: `max`, `min`, `maximum`, `minimum`, `abs`, `pos`, `neg`, `sqrt`, `square`, `power`, `inv_pos`, `sum_largest`, `sum_smallest`, `cummax`, `dotsort`, `cvar`, `ptp`
- [06]-[SPECTRAL]: `lambda_max`, `lambda_min`, `lambda_sum_largest`, `lambda_sum_smallest`, `log_det`, `matrix_frac`, `sigma_max`, `tr_inv`, `pf_eigenvalue`, `eye_minus_inv`, `resolvent`, `psd_wrap`
- [07]-[DGP]: `geo_mean`, `harmonic_mean`, `inv_prod`, `prod`, `gmatmul`, `one_minus_pos`, `diff_pos`, `perspective`
- [08]-[TRANSFORM]: `suppfunc`, `scalene`, `partial_optimize`, `mean`, `std`, `var`

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- One `Variable`/`Parameter`/`Expression` algebra owns the model; the DCP ruleset tracks curvature and sign, so a non-DCP model raises at construction (`Problem.is_dcp()` false) rather than mis-solving — model with atoms, never a hand-rolled numeric objective.
- Every modality rides `Problem` as a row, attribute, or solve flag, never a parallel type: objective sign is `Minimize`/`Maximize`; variable domain (`nonneg`/`pos`/`symmetric`/`PSD`/`NSD`/`hermitian`/`diag`/`boolean`/`integer`/`sparse`/`bounds`) is a `Variable` constructor attribute; cone membership is a relational operator or an explicit `SOC`/`PSD`/`ExpCone`/`PowCone3D`/`PowConeND`/`FiniteSet` row; `gp`/`qcp`/integer is a solve flag or leaf attribute; `solver` selects the backend.
- Primal values read off `Constraint.args` (`.expr` exists only on relational `Inequality`); the per-cone dual layout is row `[05]`.
- `Parameter` under DPP compiles a parametrized family once and warm-re-solves across `Parameter.value`; `enforce_dpp=True` fails fast when a model breaks DPP.
- Each solve captures status, optimal value, primal `Variable.value`, dual `Constraint.dual_value`, solver name, and iteration/residual stats as a convex-solve receipt; the duals are the optimality certificate.

[STACKING]:
- `clarabel`(`.api/clarabel.md`): `cp.CLARABEL` is the default conic backend; cvxpy reduces the DCP model to the `(P, q, A, b, cones)` standard form `DefaultSolver` consumes and reads back `obj_val`/`solve_time`/`iterations` and the dual `z`. `get_problem_data(cp.CLARABEL)` yields `(data, chain, inverse_data)` so `DefaultSolver` runs directly and maps back through `inverse_data`, keeping the modeling layer out of the hot loop.
- `scipy`(`.api/scipy.md`): `Parameter`/`Constant` values and the reduced `data` matrices are NumPy/`scipy.sparse`; large sparse constraint blocks enter as `scipy.sparse` and the reduction preserves sparsity into the Clarabel CSC form.
- `dask`(`.api/dask.md`): a DPP-parametrized family compiles once; a `dask`-fanned sweep sets `Parameter.value` per design point and re-calls `solve`, amortizing one compile across the grid and emitting one convex-solve receipt per point.
- `equinox`(`.api/equinox.md`): `solve(requires_grad=True)` with `Problem.backward()` differentiates the solution map w.r.t. `Parameter`, composing a convex layer into a JAX/equinox outer loop as a differentiable optimization node.
- within-lib: the convex owner composes `Variable`, `Problem`, the atom library, and `problem.solve` into one convex-intent entry, discriminating objective sign, cone membership, `gp`/`qcp`/integer mode, backend, and parameter sweep on request shape rather than parallel entrypoints.

[LOCAL_ADMISSION]:
- `import cvxpy as cp` at boundary scope; the compute manifest owns the module-level-import ban.
- `Variable`/`Parameter` modeling under `Minimize`/`Maximize` with relational/cone constraints, `Problem.solve` over Clarabel, `gp`/`qcp`/mixed-integer as solve/leaf rows, DPP parameter sweeps, `get_problem_data` direct-backend drive, and `requires_grad` differentiation.

[RAIL_LAW]:
- Package: `cvxpy`
- Owns: disciplined convex/geometric/quasiconvex problem modeling, the convex and log-log atom libraries, cone constraints, mixed-integer support, multi-backend conic solve with primal/dual recovery, DPP warm re-solves, and parameter-gradient differentiation
- Accept: `Variable`/`Parameter` modeling with leaf-attribute domains under `Minimize`/`Maximize` and relational/cone constraints, `Problem.solve` with Clarabel as the conic backend, `gp`/`qcp`/mixed-integer modes as solve/leaf rows, dual-value certificates, DPP parameter sweeps, `get_problem_data` direct-backend drive, `requires_grad` differentiation
- Reject: wrapper-renames of `Variable`/`Problem`/`solve`; a hand-rolled interior-point or cone reduction where cvxpy with a backend is admitted; a DCP-violating manual objective; a parallel problem type per objective sign, solver, or `gp`/`qcp`/integer mode; a parallel variable subtype where a leaf attribute expresses the domain; rebuilding the `Problem` per parameter value where DPP warm re-solve applies

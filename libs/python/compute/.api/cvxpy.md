# [PY_COMPUTE_API_CVXPY]

`cvxpy` supplies the disciplined-convex-programming modeling surface for the compute convex-optimization rail: a `Variable`/`Parameter`/`Expression` algebra composed under `Minimize`/`Maximize` and constraint relations into a `Problem`, then `solve`d through a pluggable solver backend (Clarabel by default) that returns the optimal value, primal variable values, and dual certificates. The package owner composes `Variable`, `Problem`, the atom library, and `problem.solve` into the convex-intent owner; it never re-implements the cone reduction or interior-point solve cvxpy and its backends already own.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `cvxpy`
- package: `cvxpy`
- import: `import cvxpy as cp`
- owner: `compute`
- rail: convex optimization
- installed: companion-band `python_version<'3.15'` (cp311-cp314 wheels only; no CPython 3.15 wheel) — RESEARCH-capture-pending-on-uv-sync; the member surface below is authored from official documentation and verifies on uv sync into the companion interpreter band
- entry points: none (library only)
- capability: disciplined convex programming — variable/parameter expression algebra, a convex atom library, equality/inequality/SOC/SDP/exponential-cone constraints, `Minimize`/`Maximize` objectives, a `Problem` compiled to a conic form, multi-backend `solve` with primal/dual recovery, DPP-parametrized warm re-solves, and mixed-integer support

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: leaf, expression, objective, and problem roots
- rail: convex optimization

| [INDEX] | [SYMBOL]     | [TYPE_FAMILY]     | [CAPABILITY]                                                         |
| :-----: | :----------- | :---------------- | :------------------------------------------------------------------- |
|  [01]   | `Variable`   | decision leaf     | optimization variable: `Variable(shape=(), name=None, **attrs)`      |
|  [02]   | `Parameter`  | symbolic constant | DPP parameter whose value is set before solve; enables warm re-solve |
|  [03]   | `Constant`   | constant leaf     | a fixed numeric value lifted into the expression algebra             |
|  [04]   | `Expression` | expression node   | abstract base of the convex/affine atom algebra (`+`/`@`/`*`/atoms)  |
|  [05]   | `Minimize`   | objective         | `Minimize(expr)` — convex objective                                  |
|  [06]   | `Maximize`   | objective         | `Maximize(expr)` — concave objective                                 |
|  [07]   | `Problem`    | problem root      | `Problem(objective, constraints=[])` — compiled and solved           |

[PUBLIC_TYPE_SCOPE]: constraint and cone types
- rail: convex optimization

| [INDEX] | [SYMBOL]     | [TYPE_FAMILY]     | [CAPABILITY]                                                  |
| :-----: | :----------- | :---------------- | :------------------------------------------------------------ |
|  [01]   | `Constraint` | constraint base   | abstract base produced by relational operators on expressions |
|  [02]   | `Zero`       | equality cone     | `expr == 0` membership                                        |
|  [03]   | `NonNeg`     | inequality cone   | elementwise `expr >= 0` membership                            |
|  [04]   | `SOC`        | second-order cone | `SOC(t, X)` — `norm2(X) <= t` second-order-cone membership    |
|  [05]   | `PSD`        | semidefinite cone | `expr >> 0` — symmetric PSD constraint                        |
|  [06]   | `ExpCone`    | exponential cone  | `ExpCone(x, y, z)` membership                                 |
|  [07]   | `PowCone3D`  | power cone        | three-dimensional power-cone membership                       |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: problem construction and solve
- rail: convex optimization

The relational operators `==`, `<=`, `>=`, `>>`, `<<` on `Expression` build `Constraint` objects; `Problem.solve` selects the backend by the `solver` keyword and returns the optimal value, writing primal values to `Variable.value` and duals to `Constraint.dual_value`.

| [INDEX] | [SURFACE]                  | [CALL_SHAPE]                                                                                 | [CAPABILITY]                                  |
| :-----: | :------------------------- | :------------------------------------------------------------------------------------------- | :-------------------------------------------- |
|  [01]   | `Problem.solve`            | `solve(solver=None, warm_start=True, verbose=False, gp=False, qcp=False, **kwargs)` -> float | compile and solve; returns optimal value      |
|  [02]   | `Problem.value`            | property                                                                                     | optimal objective value after solve           |
|  [03]   | `Problem.status`           | property -> `str`                                                                            | `optimal`/`infeasible`/`unbounded`/inaccurate |
|  [04]   | `Variable.value`           | property                                                                                     | primal solution after solve                   |
|  [05]   | `Constraint.dual_value`    | property                                                                                     | dual certificate after solve                  |
|  [06]   | `installed_solvers`        | `installed_solvers()` -> `list[str]`                                                         | available solver backend names                |
|  [07]   | `Problem.get_problem_data` | `get_problem_data(solver)`                                                                   | the reduced conic problem data                |

[ENTRYPOINT_SCOPE]: convex atom library (`cp.<atom>`)
- rail: convex optimization

| [INDEX] | [SURFACE]                                                                        | [FAMILY]             | [RAIL]                                  |
| :-----: | :------------------------------------------------------------------------------- | :------------------- | :-------------------------------------- |
|  [01]   | `sum`, `trace`, `multiply`, `matmul`, `reshape`, `vstack`, `hstack`, `diag`      | affine atoms         | affine combinators preserving curvature |
|  [02]   | `norm`, `norm1`, `norm2`, `norm_inf`, `pnorm`, `mixed_norm`, `tv`                | norm atoms           | convex norms and total variation        |
|  [03]   | `quad_form`, `sum_squares`, `quad_over_lin`, `huber`                             | quadratic atoms      | quadratic and robust losses             |
|  [04]   | `exp`, `log`, `log_sum_exp`, `entr`, `kl_div`, `rel_entr`, `logistic`            | log/exp atoms        | exponential-cone-representable atoms    |
|  [05]   | `max`, `min`, `maximum`, `minimum`, `abs`, `pos`, `neg`, `lambda_max`, `log_det` | elementwise/spectral | extrema, spectral, and matrix atoms     |

## [04]-[IMPLEMENTATION_LAW]

[CONVEX_MODELING]:
- import: `import cvxpy as cp` at boundary scope only; module-level import is banned by the manifest import policy.
- expression axis: one `Variable`/`Parameter`/`Expression` algebra owns the model; curvature (affine/convex/concave) and sign are tracked by the DCP ruleset, so a problem that violates DCP raises at construction, never silently mis-solves — model with atoms, never a hand-rolled numeric objective.
- objective axis: `Minimize` carries the convex objective and `Maximize` the concave one; the sign of the objective is a constructor row, never a parallel problem type.
- constraint axis: relational operators (`==`/`<=`/`>=`/`>>`) produce `Constraint` cone memberships; `SOC`/`PSD`/`ExpCone`/`PowCone3D` are explicit cone rows for problems the relational form cannot express — never a manual cone slack reformulation.
- backend axis: `Problem.solve(solver=...)` selects the cone backend (Clarabel is the default conic solver and the dual-certificate source); the same `Problem` re-solves across backends without remodeling — backend is a solve row, not a parallel model.
- parameter axis: `Parameter` plus DPP makes a parametrized family compile once and warm-re-solve across parameter values; sweep by setting `Parameter.value` and re-calling `solve`, never by rebuilding the `Problem`.
- evidence: each solve captures status, optimal value, primal `Variable.value`, dual `Constraint.dual_value`, solver name, and iteration/residual stats as a convex-solve receipt; the dual values are the certificate of optimality.
- boundary: cvxpy owns convex modeling and the conic reduction; Clarabel owns the interior-point solve and the dual certificate; the graduation rail hands the offline solution across the wire; live UI stays outside this package.

[RAIL_LAW]:
- Package: `cvxpy`
- Owns: disciplined convex problem modeling, the convex atom library, cone constraints, multi-backend conic solve with primal/dual recovery, and DPP-parametrized warm re-solves
- Accept: `Variable`/`Parameter` modeling under `Minimize`/`Maximize` and relational/cone constraints, `Problem.solve` with Clarabel as the conic backend, dual-value certificates, DPP parameter sweeps
- Reject: wrapper-renames of `Variable`/`Problem`/`solve`; a hand-rolled interior-point or cone reduction where cvxpy plus a backend is admitted; a DCP-violating manual objective; a parallel problem type per objective sign or solver; rebuilding the `Problem` per parameter value where DPP warm re-solve applies

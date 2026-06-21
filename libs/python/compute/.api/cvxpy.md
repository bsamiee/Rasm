# [PY_COMPUTE_API_CVXPY]

`cvxpy` supplies the disciplined-convex-programming modeling surface for the compute convex-optimization rail: a `Variable`/`Parameter`/`Expression` algebra composed under `Minimize`/`Maximize` and constraint relations into a `Problem`, then `solve`d through a pluggable solver backend (Clarabel by default) that returns the optimal value, primal variable values, and dual certificates. It also owns disciplined geometric (DGP) and quasiconvex (DQCP) modes, mixed-integer support, and DPP warm re-solve. The package owner composes `Variable`, `Problem`, the atom library, and `problem.solve` into the convex-intent owner; it never re-implements the cone reduction or interior-point solve cvxpy and its backends already own.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `cvxpy`
- package: `cvxpy`
- import: `import cvxpy as cp`
- owner: `compute`
- rail: convex optimization
- license: Apache-2.0
- installed: manifest-pinned `cvxpy>=1.9.1; python_version<'3.15'` — companion-band (cp311-cp314 wheels only; no CPython 3.15 wheel; transitive scipy gates the band). RESEARCH-capture-pending-on-uv-sync; the member surface below is authored from official documentation and verifies on uv sync into the companion interpreter band.
- entry points: none (library only)
- capability: disciplined convex programming — variable/parameter expression algebra, a broad convex/affine/spectral atom library, equality/inequality/SOC/SDP/exponential/power-cone constraints, `Minimize`/`Maximize` objectives, a `Problem` compiled to conic form, multi-backend `solve` with primal/dual recovery, DPP-parametrized warm re-solves, DGP (geometric) and DQCP (quasiconvex) modes, and mixed-integer support

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: leaf, expression, objective, and problem roots
- rail: convex optimization

| [INDEX] | [SYMBOL]     | [TYPE_FAMILY]     | [CAPABILITY]                                                         |
| :-----: | :----------- | :---------------- | :------------------------------------------------------------------- |
|  [01]   | `Variable`   | decision leaf     | `Variable(shape=(), name=None, *, nonneg, pos, symmetric, PSD, NSD, diag, hermitian, boolean, integer, sparse, bounds)` |
|  [02]   | `Parameter`  | symbolic constant | `Parameter(shape=(), name=None, value=None, **attrs)` — DPP parameter set before solve; enables warm re-solve |
|  [03]   | `Constant`   | constant leaf     | a fixed numeric value lifted into the expression algebra             |
|  [04]   | `Expression` | expression node   | abstract base of the convex/affine atom algebra (`+`/`@`/`*`/atoms); carries `.curvature`/`.sign`/`.shape`/`.value` |
|  [05]   | `Minimize`   | objective         | `Minimize(expr)` — convex objective                                  |
|  [06]   | `Maximize`   | objective         | `Maximize(expr)` — concave objective                                 |
|  [07]   | `Problem`    | problem root      | `Problem(objective, constraints=[])` — compiled and solved           |

[PUBLIC_TYPE_SCOPE]: constraint and cone types
- rail: convex optimization

| [INDEX] | [SYMBOL]      | [TYPE_FAMILY]     | [CAPABILITY]                                                  |
| :-----: | :------------ | :---------------- | :------------------------------------------------------------ |
|  [01]   | `Constraint`  | constraint base   | abstract base produced by relational operators on expressions |
|  [02]   | `Zero`        | equality cone     | `expr == 0` membership                                        |
|  [03]   | `NonNeg`      | inequality cone   | elementwise `expr >= 0` membership                            |
|  [04]   | `SOC`         | second-order cone | `SOC(t, X, axis=0)` — `norm2(X) <= t` second-order-cone membership |
|  [05]   | `PSD`         | semidefinite cone | `expr >> 0` — symmetric PSD constraint                        |
|  [06]   | `ExpCone`     | exponential cone  | `ExpCone(x, y, z)` membership                                 |
|  [07]   | `PowCone3D`   | power cone        | `PowCone3D(x, y, z, alpha)` three-dimensional power-cone membership |
|  [08]   | `PowConeND`   | power cone (N-D)  | `PowConeND(W, z, alpha, axis=0)` n-dimensional power-cone membership |
|  [09]   | `FiniteSet`   | combinatorial     | `FiniteSet(expr, vec)` — membership in a finite value set (MI) |
|  [10]   | `OpRelEntrConeQuad` | spectral cone | operator-relative-entropy cone (quantum/spectral)            |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: problem construction and solve
- rail: convex optimization

The relational operators `==`, `<=`, `>=`, `>>`, `<<` on `Expression` build `Constraint` objects; `Problem.solve` selects the backend by the `solver` keyword and returns the optimal value, writing primal values to `Variable.value` and duals to `Constraint.dual_value`. `gp=True` enables DGP geometric programming; `qcp=True` enables DQCP quasiconvex programming.

| [INDEX] | [SURFACE]                  | [CALL_SHAPE]                                                                                 | [CAPABILITY]                                  |
| :-----: | :------------------------- | :------------------------------------------------------------------------------------------- | :-------------------------------------------- |
|  [01]   | `Problem.solve`            | `solve(solver=None, warm_start=True, verbose=False, gp=False, qcp=False, requires_grad=False, enforce_dpp=False, ignore_dpp=False, canon_backend=None, **kwargs)` -> float | compile and solve; returns optimal value |
|  [02]   | `Problem.value`            | property                                                                                     | optimal objective value after solve           |
|  [03]   | `Problem.status`           | property -> `str`                                                                            | `optimal`/`infeasible`/`unbounded`/`*_inaccurate` |
|  [04]   | `Variable.value`           | property (settable for warm start)                                                           | primal solution after solve                   |
|  [05]   | `Constraint.dual_value`    | property                                                                                     | dual certificate after solve                  |
|  [06]   | `installed_solvers`        | `installed_solvers()` -> `list[str]`                                                         | available solver backend names (`CLARABEL`, …) |
|  [07]   | `Problem.get_problem_data` | `get_problem_data(solver, gp=False, enforce_dpp=False, ...)` -> `(data, chain, inverse_data)` | the reduced conic problem data + chain for direct backend drive |
|  [08]   | `Problem.backward` / `Problem.derivative` | method                                                                        | differentiate solution w.r.t. `Parameter` (requires `requires_grad=True`) |
|  [09]   | `Problem.is_dcp` / `is_dgp` / `is_dqcp` / `is_qp` | method -> `bool`                                                              | curvature-ruleset classification before solve |
|  [10]   | `Problem.parameters` / `Problem.variables` | method                                                                      | enumerate leaves for sweep wiring             |

[ENTRYPOINT_SCOPE]: convex atom library (`cp.<atom>`)
- rail: convex optimization

| [INDEX] | [SURFACE]                                                                                            | [FAMILY]             | [RAIL]                                  |
| :-----: | :--------------------------------------------------------------------------------------------------- | :------------------- | :-------------------------------------- |
|  [01]   | `sum`, `trace`, `multiply`, `matmul`, `reshape`, `vstack`, `hstack`, `stack`, `diag`, `kron`, `convolve`, `cumsum`, `diff`, `bmat`, `outer`, `vec`, `upper_tri`, `vec_to_upper_tri`, `real`, `imag`, `conj` | affine atoms | affine combinators preserving curvature |
|  [02]   | `norm`, `norm1`, `norm2`, `norm_inf`, `pnorm`, `mixed_norm`, `normNuc`, `sigma_max`, `tv`            | norm atoms           | convex norms, nuclear/spectral norm, total variation |
|  [03]   | `quad_form`, `sum_squares`, `quad_over_lin`, `matrix_frac`, `huber`, `tr_inv`                        | quadratic atoms      | quadratic, matrix-fractional, and robust losses |
|  [04]   | `exp`, `log`, `log1p`, `log_sum_exp`, `entr`, `kl_div`, `rel_entr`, `logistic`, `log_normcdf`, `loggamma`, `xexp`, `von_neumann_entr` | log/exp atoms | exponential-cone-representable and entropy atoms |
|  [05]   | `max`, `min`, `maximum`, `minimum`, `abs`, `pos`, `neg`, `sqrt`, `square`, `power`, `inv_pos`, `sum_largest`, `sum_smallest`, `cummax`, `dotsort`, `cvar`, `ptp` | elementwise/order | extrema, powers, order-statistic and risk atoms |
|  [06]   | `lambda_max`, `lambda_min`, `lambda_sum_largest`, `lambda_sum_smallest`, `log_det`, `matrix_frac`, `sigma_max`, `tr_inv`, `pf_eigenvalue`, `eye_minus_inv`, `resolvent`, `psd_wrap` | matrix/spectral | eigenvalue, log-det, and spectral matrix atoms |
|  [07]   | `geo_mean`, `harmonic_mean`, `inv_prod`, `prod`, `gmatmul`, `one_minus_pos`, `diff_pos`, `perspective` | DGP/geometric    | log-log-convex atoms for `gp=True` geometric programs |
|  [08]   | `suppfunc`, `scalene`, `partial_optimize`, `mean`, `std`, `var`                                      | transform/support    | support functions, partial optimization, statistical reductions |

## [04]-[IMPLEMENTATION_LAW]

[CONVEX_MODELING]:
- import: `import cvxpy as cp` at boundary scope only; module-level import is banned by the manifest import policy.
- expression axis: one `Variable`/`Parameter`/`Expression` algebra owns the model; curvature (affine/convex/concave) and sign are tracked by the DCP ruleset, so a problem that violates DCP raises at construction (`Problem.is_dcp() is False`), never silently mis-solves — model with atoms, never a hand-rolled numeric objective.
- leaf-attribute axis: variable domain (`nonneg`/`pos`/`symmetric`/`PSD`/`NSD`/`hermitian`/`diag`/`boolean`/`integer`/`sparse`/`bounds`) is a `Variable(...)` constructor attribute, not a parallel constraint or a parallel variable subtype — the domain rides the leaf so the reduction sees it.
- objective axis: `Minimize` carries the convex objective and `Maximize` the concave one; the sign of the objective is a constructor row, never a parallel problem type.
- constraint axis: relational operators (`==`/`<=`/`>=`/`>>`) produce `Constraint` cone memberships; `SOC`/`PSD`/`ExpCone`/`PowCone3D`/`PowConeND`/`FiniteSet` are explicit cone rows for problems the relational form cannot express — never a manual cone slack reformulation.
- mode axis: `solve(gp=True)` switches to disciplined geometric programming over the log-log atom family (`geo_mean`/`prod`/`one_minus_pos`/…); `solve(qcp=True)` switches to disciplined quasiconvex programming via bisection; mode is a solve row, not a parallel `Problem` type. `boolean`/`integer` variable attributes promote the same `Problem` to mixed-integer without a separate model.
- backend axis: `Problem.solve(solver=...)` selects the cone backend (`cp.CLARABEL` is the default conic solver and the dual-certificate source); the same `Problem` re-solves across backends without remodeling — backend is a solve row, not a parallel model.
- parameter axis: `Parameter` plus DPP makes a parametrized family compile once and warm-re-solve across parameter values; sweep by setting `Parameter.value` and re-calling `solve`, never by rebuilding the `Problem`. `enforce_dpp=True` fails fast when a model breaks DPP and silently recompiles.
- evidence: each solve captures status, optimal value, primal `Variable.value`, dual `Constraint.dual_value`, solver name, and iteration/residual stats as a convex-solve receipt; the dual values are the certificate of optimality.
- boundary: cvxpy owns convex modeling and the conic reduction; Clarabel owns the interior-point solve and the dual certificate; the graduation rail hands the offline solution across the wire; live UI stays outside this package.

[INTEGRATION_STACK]:
- clarabel seam: `cp.CLARABEL` is the default conic backend; cvxpy reduces the DCP model to the `(P, q, A, b, cones)` standard form Clarabel consumes and reads back `obj_val`/`solve_time`/`iterations` plus the dual `z` as the optimality certificate (`clarabel.md`). For a hot offline loop, `get_problem_data(cp.CLARABEL)` yields `(data, chain, inverse_data)` so `clarabel.DefaultSolver` runs directly and the result is mapped back through `inverse_data` — the modeling layer stays out of the loop.
- scipy seam: `Parameter`/`Constant` values and the reduced `data` matrices are NumPy/`scipy.sparse` (`scipy.md`); large sparse constraint blocks enter as `scipy.sparse` matrices and the reduction preserves sparsity into the Clarabel CSC form.
- DPP-sweep seam: a parametrized study (`Parameter` per swept axis) compiles once; the sweep sets `Parameter.value` per design point and re-calls `solve`, so a `dask`-fanned (`dask.md`) or sequential sweep amortizes one compile across the whole grid and emits one convex-solve receipt per point.
- gradient seam: `solve(requires_grad=True)` plus `Problem.backward()` differentiates the solution map w.r.t. `Parameter`, so a convex layer composes into a JAX/equinox outer loop (`equinox.md`) as a differentiable optimization node.

[RAIL_LAW]:
- Package: `cvxpy`
- Owns: disciplined convex/geometric/quasiconvex problem modeling, the convex and log-log atom libraries, cone constraints, mixed-integer support, multi-backend conic solve with primal/dual recovery, DPP-parametrized warm re-solves, and parameter-gradient differentiation
- Accept: `Variable`/`Parameter` modeling with leaf-attribute domains under `Minimize`/`Maximize` and relational/cone constraints, `Problem.solve` with Clarabel as the conic backend, `gp`/`qcp`/mixed-integer modes as solve/leaf rows, dual-value certificates, DPP parameter sweeps, `get_problem_data` direct-backend drive, `requires_grad` differentiation
- Reject: wrapper-renames of `Variable`/`Problem`/`solve`; a hand-rolled interior-point or cone reduction where cvxpy plus a backend is admitted; a DCP-violating manual objective; a parallel problem type per objective sign, solver, or `gp`/`qcp`/integer mode; a parallel variable subtype where a leaf attribute expresses the domain; rebuilding the `Problem` per parameter value where DPP warm re-solve applies

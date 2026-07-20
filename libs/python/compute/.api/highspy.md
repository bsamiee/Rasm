# [PY_COMPUTE_API_HIGHSPY]

`highspy` supplies the HiGHS linear, mixed-integer, and convex-quadratic solver completing the compute program-and-convex backend family beside the `clarabel`/`scs` conic arms: a single `Highs` instance owning model assembly, the dual-simplex/interior-point/PDLP LP engines, the branch-and-bound MIP engine, and the active-set QP engine, returning a `HighsSolution` with primal `col_value`, dual `col_dual`, a `HighsModelStatus` verdict, and a `HighsInfo` receipt carrying the simplex/IPM/node counts and the MIP gap. Clarabel and SCS own the conic cones (SOC, PSD, exponential, power); HiGHS owns the LP/MIP/QP regime — simplex-exact bases, integrality, and sensitivity ranging neither conic arm provides — and cvxpy selects it as the LP-and-MIP backend of the one convex family. Package owner composes `Highs`, the `highs_var`/`highs_linear_expression` modeling surface, `run`/`optimize`, and `getSolution`/`getInfo`, never re-implementing the revised simplex or the branch-and-bound search HiGHS owns.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `highspy`
- package: `highspy`
- import: `import highspy`
- owner: `compute`
- rail: mathematical programming (LP/MIP/QP simplex and interior-point solver backend)
- license: MIT (HiGHS C++ core, pybind11 binding, scikit-build-core native build)
- entry points: none (library only)
- capability: revised dual-simplex, IPX interior-point, and PDLP first-order LP solve; branch-and-bound mixed-integer solve over continuous/integer/implicit-integer/semi-continuous/semi-integer columns; active-set convex-QP solve over an upper-triangular Hessian; a fluent `highs_var`/`highs_linear_expression` modeling surface and a sparse `HighsLp`/`HighsModel` matrix surface; incremental row/column add-change-delete for warm re-solve; primal/dual/reduced-cost recovery with primal and dual ray certificates; simplex-basis inverse and reduced-row solves; cost-and-bound sensitivity ranging; irreducible infeasible subsystem extraction; multi-objective blended and lexicographic objectives; a MIP callback surface for improving-solution, user-solution, lazy-constraint, and cut-pool interaction; and background start/join/cancel solve

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: solver, model, and solution roots
- rail: mathematical programming

`Highs` is the one stateful solver object; a model is either built fluently through `highs_var`/`highs_linear_expression` or passed as the typed `HighsModel`/`HighsLp` sparse surface, and the solve returns the typed `HighsSolution`/`HighsBasis`/`HighsInfo` receipts. `kHighsInf` is the free-bound sentinel; `kHighsIInf` the integer sentinel; `kHighsUndefined` the unset-value sentinel.

| [INDEX] | [SYMBOL]                  | [TYPE_FAMILY]       | [CAPABILITY]                                                                          |
| :-----: | :------------------------ | :------------------ | :------------------------------------------------------------------------------------ |
|  [01]   | `Highs`                   | solver              | stateful engine owning model assembly, solve, mutation, options, and solution readout |
|  [02]   | `highs_var`               | modeling variable   | fluent column handle carrying `index`, `name`, and its owning `highs` instance        |
|  [03]   | `highs_linear_expression` | modeling expression | operator-built affine form with `idxs`/`vals`/`constant`, `bounds`, and `simplify`    |
|  [04]   | `highs_cons`              | modeling constraint | row handle over a bounded `highs_linear_expression`                                   |
|  [05]   | `HighsModel`              | problem model       | `lp_` linear part beside the `hessian_` quadratic part                                |
|  [06]   | `HighsLp`                 | linear model        | cost/bound `col_*`/`row_*` vectors, `a_matrix_`, `integrality_`, `sense_`, `offset_`  |
|  [07]   | `HighsSparseMatrix`       | sparse matrix       | `start_`/`index_`/`value_` CSC/CSR triplet with `num_col_`/`num_row_` dimensions      |
|  [08]   | `HighsHessian`            | quadratic term      | `dim_`, `format_`, and `start_`/`index_`/`value_` upper-triangular sparse Hessian     |
|  [09]   | `HighsSolution`           | result carrier      | `col_value`/`col_dual`/`row_value`/`row_dual` with `value_valid`/`dual_valid` flags   |
|  [10]   | `HighsBasis`              | simplex basis       | `col_status`/`row_status` `HighsBasisStatus` vectors with `valid`/`alien` flags       |
|  [11]   | `HighsInfo`               | solve receipt       | objective, status, iteration-count, and MIP-gap fields fenced below                   |
|  [12]   | `HighsRanging`            | sensitivity         | `col_cost_`, `col_bound_`, `row_bound_` up/down sensitivity ranges                    |
|  [13]   | `HighsIis`                | infeasibility       | irreducible infeasible subsystem with `col_index_`/`row_index_` and per-bound status  |
|  [14]   | `HighsLinearObjective`    | multi-objective     | `coefficients`, `weight`, `priority`, `offset`, and `abs_tolerance`/`rel_tolerance`   |
|  [15]   | `HighsCallback`           | callback surface    | `subscribe`/`unsubscribe` over `HighsCallbackEvent` for MIP and simplex interaction   |

[PUBLIC_TYPE_SCOPE]: `HighsInfo` receipt fields
- rail: mathematical programming

`getInfo()` returns `HighsInfo` as the solve receipt; `objective_function_value` is the incumbent objective, `primal_solution_status`/`dual_solution_status` are the closed `SolutionStatus` verdicts, and the MIP fields carry the branch-and-bound proof.

| [INDEX] | [FIELD]                                               | [ROLE]                                                   |
| :-----: | :---------------------------------------------------- | :------------------------------------------------------- |
|  [01]   | `objective_function_value`                            | incumbent objective at termination                       |
|  [02]   | `primal_solution_status` / `dual_solution_status`     | primal and dual `SolutionStatus` feasibility verdicts    |
|  [03]   | `basis_validity`                                      | `BasisValidity` of the returned simplex basis            |
|  [04]   | `simplex_iteration_count` / `ipm_iteration_count`     | revised-simplex and interior-point iteration tallies     |
|  [05]   | `pdlp_iteration_count` / `qp_iteration_count`         | first-order PDLP and active-set QP iteration tallies     |
|  [06]   | `crossover_iteration_count`                           | IPM-to-simplex crossover iteration count                 |
|  [07]   | `mip_node_count`                                      | branch-and-bound node count                              |
|  [08]   | `mip_dual_bound` / `mip_gap`                          | best dual bound and the relative optimality gap          |
|  [09]   | `max_primal_infeasibility` / `max_dual_infeasibility` | worst primal and dual constraint violation               |
|  [10]   | `max_integrality_violation`                           | worst integer-column fractional violation                |
|  [11]   | `max_complementarity_violation`                       | worst complementary-slackness residual                   |
|  [12]   | `primal_dual_integral`                                | primal-dual integral bounding the convergence trajectory |

[PUBLIC_TYPE_SCOPE]: closed status and vocabulary enums
- rail: mathematical programming

Every verdict is a closed enum: `HighsStatus` grades each call, `HighsModelStatus` the terminal model verdict, `HighsBasisStatus` each basis position, and `HighsVarType` each column kind; the `k`-prefixed members are the language-neutral wire names HiGHS emits.

```python signature
# Closed verdict and vocabulary enums; k-prefixed members are the language-neutral wire names HiGHS emits.
HighsStatus         = kOk, kWarning, kError
HighsModelStatus    = kOptimal, kInfeasible, kUnbounded, kUnboundedOrInfeasible, kObjectiveBound, kObjectiveTarget, kTimeLimit, kIterationLimit, kSolutionLimit, kInterrupt, kMemoryLimit, kModelEmpty, kNotset
HighsVarType        = kContinuous, kInteger, kImplicitInteger, kSemiContinuous, kSemiInteger
ObjSense            = kMinimize, kMaximize
HighsBasisStatus    = kLower, kBasic, kUpper, kZero, kNonbasic
SolutionStatus      = kSolutionStatusNone, kSolutionStatusInfeasible, kSolutionStatusFeasible
MatrixFormat        = kColwise, kRowwise, kRowwisePartitioned
HessianFormat       = kTriangular, kSquare
IisStrategy         = kIisStrategyFromLp, kIisStrategyFromRay, kIisStrategyMin, kIisStrategyMax, kIisStrategyIrreducible, kIisStrategyRelaxation, kIisStrategyLight, kIisStrategyColPriority
HighsPresolveStatus = kNotPresolved, kNotReduced, kReduced, kReducedToEmpty, kInfeasible, kUnboundedOrInfeasible, kTimeout
```

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: fluent modeling entry points
- rail: mathematical programming

`Highs()` builds the empty solver; `silent()` mutes the log. `addVariable(lb=, ub=, obj=, type=, name=)` returns a `highs_var`; Python arithmetic over vars yields a `highs_linear_expression`, and a comparison against a bound yields the constraint an `addConstr` row accepts. `minimize`/`maximize` set the sense-and-objective; `optimize` (alias `run`/`solve`) solves; `val(var)`/`vals(vars)` read the primal, and `getInfo()`/`getModelStatus()` read the receipt and verdict.

| [INDEX] | [SURFACE]              | [CALL_SHAPE]                                        | [CAPABILITY]                                          |
| :-----: | :--------------------- | :-------------------------------------------------- | :---------------------------------------------------- |
|  [01]   | `Highs`                | `Highs()`                                           | construct the empty stateful solver                   |
|  [02]   | `Highs.addVariable`    | `addVariable(lb=, ub=, obj=, type=, name=)` -> var  | add one column and return its handle (alias `addVar`) |
|  [03]   | `Highs.addVariables`   | `addVariables(*keys, lb=, ub=, type=)` -> array     | add a keyed column block as a `HighspyArray`          |
|  [04]   | `Highs.addIntegral`    | `addIntegral(lb=, ub=)` / `addBinary()`             | add an integer or binary column                       |
|  [05]   | `Highs.addConstr`      | `addConstr(expr <= rhs)` -> `highs_cons`            | add one bounded linear constraint row                 |
|  [06]   | `Highs.qsum`           | `qsum(exprs)` / `expr(var)`                         | build an affine sum and a fresh expression            |
|  [07]   | `Highs.minimize`       | `minimize(expr)` / `maximize(expr)`                 | set the objective sense and linear objective          |
|  [08]   | `Highs.setObjective`   | `setObjective(expr, sense=)`                        | set the objective from an expression and a sense      |
|  [09]   | `Highs.optimize`       | `optimize()` / `run()` / `solve()` -> `HighsStatus` | solve the assembled LP/MIP/QP model                   |
|  [10]   | `Highs.val`            | `val(var)` / `vals(vars)`                           | read the primal value of a variable or expression     |
|  [11]   | `Highs.getSolution`    | `getSolution()` -> `HighsSolution`                  | read the full primal/dual solution carrier            |
|  [12]   | `Highs.getInfo`        | `getInfo()` -> `HighsInfo`                          | read the solve receipt fenced above                   |
|  [13]   | `Highs.getModelStatus` | `getModelStatus()` -> `HighsModelStatus`            | read the terminal model verdict                       |
|  [14]   | `Highs.setInteger`     | `setInteger(var)` / `setContinuous(var)`            | flip a column's integrality after assembly            |

[ENTRYPOINT_SCOPE]: sparse-matrix, mutation, and advanced-simplex entry points
- rail: mathematical programming

`passModel(HighsModel)` loads a fully-assembled sparse model; `passHessian(HighsHessian)` adds the QP term. Incremental `changeColBounds`/`changeColCost`/`changeColIntegrality`/`changeRowBounds`/`changeCoeff` and `deleteCols`/`deleteRows` mutate the model in place for warm re-solve; `getRanging()` returns cost-and-bound sensitivity, `getIis()` the irreducible infeasible subsystem, and the basis-solve family exposes the simplex tableau.

| [INDEX] | [SURFACE]                     | [CALL_SHAPE]                                     | [CAPABILITY]                                         |
| :-----: | :---------------------------- | :----------------------------------------------- | :--------------------------------------------------- |
|  [01]   | `Highs.passModel`             | `passModel(HighsModel)` -> `HighsStatus`         | load a fully-assembled sparse LP/QP model            |
|  [02]   | `Highs.passHessian`           | `passHessian(HighsHessian)`                      | attach the upper-triangular quadratic objective term |
|  [03]   | `Highs.setOptionValue`        | `setOptionValue(name, value)`                    | solver option: `solver`/`presolve`/`time_limit`      |
|  [04]   | `Highs.changeColBounds`       | `changeColBounds(col, lower, upper)`             | mutate one column's bounds in place                  |
|  [05]   | `Highs.changeColCost`         | `changeColCost(col, cost)`                       | mutate one column's objective coefficient            |
|  [06]   | `Highs.changeCoeff`           | `changeCoeff(row, col, value)`                   | mutate one constraint-matrix entry in place          |
|  [07]   | `Highs.getRanging`            | `getRanging()` -> `HighsRanging`                 | cost-and-bound sensitivity ranges                    |
|  [08]   | `Highs.getIis`                | `getIis()` -> `HighsIis`                         | irreducible infeasible subsystem extraction          |
|  [09]   | `Highs.getDualRay`            | `getDualRay()` / `getPrimalRay()`                | infeasibility and unboundedness ray certificates     |
|  [10]   | `Highs.getBasisInverseRow`    | `getBasisInverseRow(row)` / `getBasisSolve(rhs)` | simplex basis-inverse row and tableau solve          |
|  [11]   | `HighsCallback.subscribe`     | `solver.cbMipSolution.subscribe(fn, user_data)`  | subscribe to one typed callback event                |
|  [12]   | `Highs.startSolve`            | `startSolve()`/`joinSolve()`/`cancelSolve()`     | background solve start, join, and cancel             |
|  [13]   | `Highs.writeModel`            | `writeModel(path)` / `readModel(path)`           | MPS/LP model write and read                          |
|  [14]   | `Highs.feasibilityRelaxation` | `feasibilityRelaxation(glob, local, rhs)`        | minimal-relaxation solve of an infeasible model      |

## [04]-[IMPLEMENTATION_LAW]

[PROGRAM_SOLVE]:
- import: `import highspy` at boundary scope only; module-level import is banned by the manifest import policy.
- model axis: one `Highs` instance owns the model; assemble fluently through `addVariable`/`addConstr`/`minimize` for a small program or `passModel(HighsModel)` with a `HighsLp` and a `HighsSparseMatrix` for a large sparse one — never a dense constraint duplicate, and never a second solver object per objective.
- regime axis: column integrality selects the engine — an all-`kContinuous` model with no Hessian runs the LP simplex/IPM/PDLP arm, any `kInteger`/`kSemiContinuous` column runs the branch-and-bound MIP arm, and a `passHessian` upper-triangular term runs the active-set QP arm; the regime is a property of the model, never a per-engine entry point, and `setOptionValue("solver", ...)` selects among `simplex`/`ipm`/`pdlp` for the LP arm.
- mutation axis: a parametrized sweep re-solves through `changeColCost`/`changeColBounds`/`changeRowBounds`/`changeCoeff` then `run()`, reusing the warm basis; a structural change adds or deletes rows/columns through `addRows`/`deleteCols` — HiGHS retains the incumbent basis across the mutation, so the warm re-solve is the discriminant against a cold rebuild.
- callback axis: each `HighsCallback` descriptor owns `subscribe(callback, user_data)`/`unsubscribe(callback)` for its typed event; the first subscription starts the event and the last removal stops it.
- evidence axis: each solve captures the `HighsModelStatus` verdict, the `HighsSolution` primal `col_value`/dual `col_dual`, the `HighsInfo` objective with the simplex/IPM/PDLP/QP iteration counts and — for the MIP arm — the `mip_node_count`, `mip_dual_bound`, and `mip_gap` as a program-solve receipt; the dual `col_dual` with the reduced costs is the LP optimality certificate, `getRanging()` bounds its stability, and `getDualRay()`/`getPrimalRay()` certify `kInfeasible`/`kUnbounded`.
- infeasibility axis: a `kInfeasible` model resolves through `getIis()` for the irreducible conflict set or `feasibilityRelaxation()` for the minimal-violation solution — never a silent empty solution or an unexplained failure.
- boundary: HiGHS owns the LP/MIP/QP solve, the simplex basis, and the primal/dual certificate; cvxpy owns the disciplined-convex modeling and reduction above it; the graduation rail hands the offline solution across the wire; live UI stays outside this package.

[INTEGRATION_STACK]:
- cvxpy backend: `cp.Problem.solve(solver=cp.HIGHS, ...)` routes a disciplined-convex LP, MILP, or convex-QP model to HiGHS; cvxpy declares `highspy` as its HiGHS binding, so admission of `highspy` is what lights the `cp.HIGHS` backend beside `cp.CLARABEL` and `cp.SCS`. HiGHS is the LP/MIP/QP arm and Clarabel/SCS the conic arms of the one convex backend family; the caller selects HiGHS when the model is linear, mixed-integer, or convex-quadratic and simplex-exact bases or integrality are required, the conic arms when SOC/PSD/exponential/power cones are present.
- scipy seam: `HighsSparseMatrix` mirrors `scipy.sparse` CSC/CSR (`start_`/`index_`/`value_`); assemble the constraint matrix as a `scipy.sparse.csc_matrix` and pass its triplet, and the upper-triangular QP Hessian as `scipy.sparse.triu(P).tocsc()` — HiGHS sits directly on the `scipy.md` sparse owner, never a hand-built index array. `scipy.optimize.linprog(method="highs")` is the same engine through the scipy façade for a one-shot LP without the `Highs` object.
- receipt seam: the `HighsModelStatus`/`HighsInfo` objective/iteration-and-node counts fold into one program-solve receipt the graduation owner hands across the wire; the dual `col_dual` is the certificate the downstream consumer reads, never recomputed. Receipt shape aligns with the Clarabel and SCS conic-solve receipts so the convex backend family emits one uniform solve-evidence row regardless of the arm that produced it.

[RAIL_LAW]:
- Package: `highspy`
- Owns: LP simplex/interior-point/PDLP solve, branch-and-bound MIP solve over integer/semi-continuous columns, active-set convex-QP solve, incremental warm-basis re-solve, primal/dual/reduced-cost recovery with ray certificates, cost-and-bound sensitivity ranging, irreducible-infeasible-subsystem extraction, multi-objective blend/lexicographic solve, and the MIP callback surface
- Accept: `Highs` fluent `addVariable`/`addConstr`/`minimize` assembly or typed `passModel` sparse assembly, `run`/`optimize` solve with `setOptionValue` engine selection, `changeCol*`/`changeRow*`/`changeCoeff` warm re-solve, `HighsSolution`/`HighsInfo`/`HighsRanging`/`HighsIis` recovery, use as the cvxpy `cp.HIGHS` LP/MIP/QP backend beside the Clarabel/SCS conic arms
- Reject: wrapper-renames of `Highs`/`run`/`optimize`; a hand-rolled simplex, branch-and-bound, or QP active-set loop where HiGHS is admitted; a dense constraint matrix where the `HighsSparseMatrix` CSC/CSR form is required; a full-symmetric QP Hessian where the upper-triangular form is required; a second solver object per objective where `HighsLinearObjective` blend/lexicographic multi-objective discriminates; a cold rebuild where `changeCol*`/`changeRow*` warm re-solve applies; choosing HiGHS for a model carrying SOC/PSD/exponential/power cones where Clarabel or SCS is the correct arm

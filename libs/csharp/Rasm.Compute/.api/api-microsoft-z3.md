# [RASM_COMPUTE_API_MICROSOFT_Z3]

`Microsoft.Z3` binds the Z3 SMT theorem prover: a first-order satisfiability engine over combined theories of real and integer arithmetic (NRA/NIA), booleans, bit-vectors, arrays, and uninterpreted functions. A typed rule set lowers to assertions and the solver returns `SATISFIABLE` with a witnessing `Model`, `UNSATISFIABLE` with an `UnsatCore` naming the exact conflicting constraints, or `UNKNOWN` — the verify-and-explain complement of the `Google.OrTools` CP-SAT lane that optimizes.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Microsoft.Z3`
- package: `Microsoft.Z3` (MIT)
- assembly: `Microsoft.Z3` (`netstandard2.0` managed wrapper P/Invoking native `libz3`)
- namespace: `Microsoft.Z3`
- asset: managed wrapper with per-RID `libz3` for win-x64 and osx-x64; the osx-arm64 `libz3` is Forge-provisioned, so a `Context` operation on osx-arm64 without it faults at native init rather than degrading silently
- rail: satisfaction

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: context, expressions, sorts

| [INDEX] | [SYMBOL]    | [TYPE_FAMILY] | [CAPABILITY]                                                                              |
| :-----: | :---------- | :------------ | :---------------------------------------------------------------------------------------- |
|  [01]   | `Context`   | class         | AST factory and arena `: IDisposable`; `Context()` or a settings-dictionary ctor          |
|  [02]   | `Expr`      | class         | AST-node base the sorted terms refine; carries `Sort`, `Simplify`, `Substitute`           |
|  [03]   | `BoolExpr`  | class         | boolean-sorted term `Solver.Assert` takes; boolean and comparison builders return it      |
|  [04]   | `ArithExpr` | class         | numeric term (`IntExpr`/`RealExpr`); `IntNum`/`RatNum` carry `Numerator`/`Denominator`    |
|  [05]   | `Sort`      | class         | term type; cached `BoolSort`/`IntSort`/`RealSort` instances constants mint against        |
|  [06]   | `FuncDecl`  | class         | uninterpreted fn/const decl and its `Symbol`; `Model.ConstDecls`/`FuncDecls` enumerate it |

[PUBLIC_TYPE_SCOPE]: solver, model, verdict

| [INDEX] | [SYMBOL]   | [TYPE_FAMILY] | [CAPABILITY]                                                                                       |
| :-----: | :--------- | :------------ | :------------------------------------------------------------------------------------------------- |
|  [01]   | `Solver`   | class         | incremental assertion stack `: IDisposable`; carries `Model`/`UnsatCore`/`Statistics`/`Params`     |
|  [02]   | `Status`   | enum          | verdict `SATISFIABLE`/`UNSATISFIABLE`/`UNKNOWN` lowered onto the `AssessmentResult`                |
|  [03]   | `Model`    | class         | satisfying assignment `: IDisposable`; `Evaluate`/`Eval`, `ConstInterp`, `ConstDecls`, `NumConsts` |
|  [04]   | `Optimize` | class         | optimizing solver from `MkOptimize`; `MkMaximize`/`MkMinimize` objectives off the verify path      |
|  [05]   | `Params`   | class         | solver config from `MkParams`; `Add(name, value)`, `Tactic`/`Goal` strategy machinery              |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: build constants and constraints — `Context`

| [INDEX] | [SURFACE]                                                     | [SHAPE] | [CAPABILITY]                                         |
| :-----: | :------------------------------------------------------------ | :------ | :--------------------------------------------------- |
|  [01]   | `MkRealConst / MkIntConst / MkBoolConst(string)`              | factory | mint a free variable of the given sort               |
|  [02]   | `MkReal / MkInt(string\|int\|long)`                           | factory | numeric literals; a `"num/den"` rational is exact    |
|  [03]   | `MkAdd / MkMul / MkSub / MkDiv / MkPower(params ArithExpr[])` | factory | arithmetic terms over NRA/NIA                        |
|  [04]   | `MkGe / MkLe / MkGt / MkLt / MkEq(Expr, Expr)`                | factory | relational atoms of a rule constraint                |
|  [05]   | `MkAnd / MkOr / MkNot / MkImplies / MkIff(BoolExpr…)`         | factory | boolean combinators composing a rule set             |
|  [06]   | `MkXor(BoolExpr, BoolExpr) / (IEnumerable<BoolExpr>)`         | factory | exclusive-or over a pair or a set                    |
|  [07]   | `MkITE(BoolExpr, Expr, Expr) / MkUnaryMinus(ArithExpr)`       | factory | guarded rectifier and unary-negate terms             |
|  [08]   | `MkSolver / MkOptimize() / (string logic)`                    | factory | `Solver`/`Optimize`; `"QF_NRA"` selects the fragment |

- `MkMul`/`MkPower`: nonlinear terms drive the NRA/NIA theory the CP-SAT lane cannot reach.
- `MkITE`: lowers `abs`/`signum`/`min`/`max` rectifiers as guarded terms, the result downcast to `ArithExpr` at the caller.

[ENTRYPOINT_SCOPE]: assert, check, explain — `Solver`

| [INDEX] | [SURFACE]                                                        | [SHAPE]  | [CAPABILITY]                                           |
| :-----: | :--------------------------------------------------------------- | :------- | :----------------------------------------------------- |
|  [01]   | `Assert(params BoolExpr[]) / AssertAndTrack(BoolExpr, BoolExpr)` | instance | add constraints; the tracked form feeds the unsat core |
|  [02]   | `Check(params Expr[]) -> Status`                                 | instance | SAT/UNSAT/UNKNOWN verdict; assumptions scope a query   |
|  [03]   | `Model -> Model`                                                 | property | witnessing assignment after `SATISFIABLE`              |
|  [04]   | `UnsatCore -> BoolExpr[]`                                        | property | minimal conflicting literals — violated rule names     |
|  [05]   | `Push() / Pop(uint)`                                             | instance | backtrackable assertion frame for what-if checks       |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- ONE `Context` owns every `Expr`/`Sort`/`Solver`/`Model` for a solve and disposes at the boundary, releasing the native AST arena; a term minted by one context never mixes into another. A rule-verify pass builds a fresh context per content-keyed request, asserts, checks, projects the verdict, and disposes, so the native handles never outlive the `AssessmentResult`.
- Z3 is single-threaded per context, so a parallel sweep runs one context per worker over the `Analysis/assessment` Sweep `JobGraph` fan-out.

[STACKING]:
- `Symbolic/expression`(`api-angourimath.md`): the lowering source — a `SymbolicExpr` constraint (`==`/`<=`/`>=` over a units-checked expression) lowers term-by-term to `MkEq`/`MkLe`/`MkGe`, its `Entity` tree walked to `Context.Mk*` terms, and nonlinear `MkMul`/`MkPower` reach the NRA/NIA theory the CP-SAT lane cannot express.
- `Google.OrTools`(`api-ortools.md`): the disjoint sibling — CP-SAT/MILP finds an optimal feasible assignment where Z3 proves a fixed assignment satisfies or names why not; a compliance check that must explain its failure is Z3's, an objective to minimize is OrTools'.
- `Solver/satisfy`: composes the folder rule-verify — a typed rule set lowers to assertions, `AssertAndTrack` pairs each constraint with a rule-named literal, and the verdict projects to `AssessmentResult` (`Model` witness on SAT, `UnsatCore` rule names on UNSAT, a typed `(Solve, Numeric)` shortfall on UNKNOWN).
- `Runtime/scheduling`: a rule-verify job is a `JobGraph` node keyed by the rule-set and input content key, one `Context` per job disposed at completion.

[LOCAL_ADMISSION]:
- `Solver/satisfy` owns rule satisfaction: a verdict enriches an existing discipline's `AssessmentResult` on that discipline's route, or persists as its own content-keyed `Node.Assessment` the `Analysis/assessment` Sweep dispatches under a seam `Discipline.Compliance` row.

[RAIL_LAW]:
- Package: `Microsoft.Z3`
- Owns: SMT satisfiability and explanation over NRA/NIA, boolean, bit-vector, array, and UF theories — `Context` construction, `Solver` incremental assert/check, `Model` witness extraction, `UnsatCore` conflict explanation, `Optimize` objectives
- Accept: a typed rule set lowered from `SymbolicExpr`, verified SAT/UNSAT with a witnessing model or a rule-naming unsat core
- Reject: an optimization objective CP-SAT/MILP owns; a context shared across sweep workers; a `libz3` assumption on osx-arm64 absent the Forge provisioning; a `Context`/`Model`/`Solver` leaked past the `AssessmentResult` boundary

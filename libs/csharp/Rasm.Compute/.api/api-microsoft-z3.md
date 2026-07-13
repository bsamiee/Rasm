# [RASM_COMPUTE_API_MICROSOFT_Z3]

`Microsoft.Z3` is the managed .NET binding to the Z3 SMT theorem prover: a first-order satisfiability engine over the combined theories of linear/nonlinear real and integer arithmetic (NRA/NIA), booleans, bit-vectors, arrays, and uninterpreted functions. Its value to the federation is VERIFY-and-EXPLAIN — a typed rule set lowers to Z3 assertions and the solver returns `SATISFIABLE` with a witnessing `Model`, `UNSATISFIABLE` with an `UnsatCore` naming the exact conflicting constraints, or `UNKNOWN`; it is the orthogonal complement of `Google.OrTools` CP-SAT (which OPTIMIZES). It is the rule-satisfaction owner of `Solver/satisfy` — a rule verdict either enriches an existing discipline's `AssessmentResult` (riding that discipline's route) or, when it must persist as its own content-keyed `Node.Assessment` the `Analysis/assessment` Sweep dispatches, earns a seam `Discipline.Compliance` row.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Microsoft.Z3`
- package: `Microsoft.Z3`
- license: MIT
- assembly: `Microsoft.Z3` (managed wrapper over the native `libz3`)
- namespace: `Microsoft.Z3`
- asset: netstandard2.0 managed wrapper P/Invoking `libz3`; the native library is bundled per-RID for win-x64/osx-x64. The osx-arm64 `libz3` is Forge-provisioned from the upstream 4.15.x arm64-osx release (the activation gate, mirroring csparse-interop's) — the managed wrapper restores clean, the native is provisioned separately; a `Context` operation on osx-arm64 without the provisioned `libz3` faults at native init, never silently degrading
- rail: satisfaction

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: context, expressions, sorts
- namespace: `Microsoft.Z3`
- rail: satisfaction
- the `Mk*` builder call shapes live in [03]

| [INDEX] | [SYMBOL]    | [CAPABILITY]                                                                                                   |
| :-----: | :---------- | :------------------------------------------------------------------------------------------------------------- |
|  [01]   | `Context`   | AST factory/arena `: IDisposable`; ctor `Context()`/`Context(Dictionary<string,string> settings)`              |
|  [02]   | `Expr`      | AST-node base the sorted terms refine; `Sort`, `Simplify()`, `Substitute(from, to)`                            |
|  [03]   | `BoolExpr`  | boolean-sorted term `Solver.Assert` takes; boolean/comparison `Mk*` builders return it                         |
|  [04]   | `ArithExpr` | numeric-sorted terms (`IntExpr`/`RealExpr`); `IntNum`/`RatNum` literals (`.Numerator`/`.Denominator`/`.Int64`) |
|  [05]   | `Sort`      | term type; `Context.BoolSort`/`IntSort`/`RealSort` cached instances constants mint against                     |
|  [06]   | `FuncDecl`  | (uninterpreted) fn/const decl + its `Symbol` name; `Model.ConstDecls`/`FuncDecls` enumerate them               |

[PUBLIC_TYPE_SCOPE]: solver, model, verdict
- namespace: `Microsoft.Z3`
- rail: satisfaction

| [INDEX] | [SYMBOL]   | [CAPABILITY]                                                                                                          |
| :-----: | :--------- | :-------------------------------------------------------------------------------------------------------------------- |
|  [01]   | `Solver`   | incremental assertion stack `: IDisposable`; assert/check/explain in [03]; `Model`/`UnsatCore`/`Statistics`/`Params`  |
|  [02]   | `Status`   | the verdict enum `SATISFIABLE`/`UNSATISFIABLE`/`UNKNOWN` — lowered onto the `AssessmentResult`                        |
|  [03]   | `Model`    | satisfying assignment `: IDisposable`; `Evaluate`/`Eval`, `ConstInterp`, `ConstDecls`/`FuncDecls`, `NumConsts`        |
|  [04]   | `Optimize` | optimizing solver (`Context.MkOptimize()`); `MkMaximize`/`MkMinimize` objectives — recorded, not the rule-verify path |
|  [05]   | `Params`   | solver config `Context.MkParams()`, `Params.Add(name, value)`; `Tactic`/`Goal` custom-strategy machinery              |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: build constants and constraints — `Context`
- namespace: `Microsoft.Z3`
- rail: satisfaction

| [INDEX] | [SURFACE]                                              | [CALL_SHAPE]                                                |
| :-----: | :----------------------------------------------------- | :---------------------------------------------------------- |
|  [01]   | `ctx.MkRealConst` / `MkIntConst` / `MkBoolConst`       | `(string name)` → `RealExpr`/`IntExpr`/`BoolExpr`           |
|  [02]   | `ctx.MkReal` / `MkInt`                                 | `(string / int / long)` → `RatNum`/`IntNum`                 |
|  [03]   | `ctx.MkAdd` / `MkMul` / `MkSub` / `MkDiv` / `MkPower`  | `(params ArithExpr[])` → `ArithExpr`                        |
|  [04]   | `ctx.MkGe` / `MkLe` / `MkGt` / `MkLt` / `MkEq`         | `(Expr, Expr)` → `BoolExpr`                                 |
|  [05]   | `ctx.MkAnd` / `MkOr` / `MkNot` / `MkImplies` / `MkIff` | `(params BoolExpr[])` / `(BoolExpr, BoolExpr)` → `BoolExpr` |
|  [06]   | `ctx.MkSolver` / `MkOptimize`                          | `()` / `(string logic)` → `Solver` / `Optimize`             |

- [01]-[CONST]: mint a free variable of the given sort — the rule-variable seeds.
- [02]-[LITERAL]: numeric literals (a rational from a `"num/den"` string is exact).
- [03]-[ARITH]: arithmetic terms — NONLINEAR `MkMul`/`MkPower` drive the NRA/NIA theory Z3 owns and CP-SAT cannot.
- [04]-[RELATION]: the relational atoms a rule constraint is built from.
- [05]-[BOOLEAN]: the boolean combinators composing a rule set.
- [06]-[ENGINE]: the solve engine (a named logic e.g. `"QF_NRA"` selects the theory fragment).

[ENTRYPOINT_SCOPE]: assert, check, explain — `Solver`
- namespace: `Microsoft.Z3`
- rail: satisfaction
- composition law: the rule set asserts through `AssertAndTrack` (each constraint paired with a boolean tracking literal named for the rule), `Check()` returns the `Status`, and on `UNSATISFIABLE` the `UnsatCore` tracking literals name the exact violated rules the `AssessmentResult` reports.

| [INDEX] | [SURFACE]                       | [CALL_SHAPE]                                                              |
| :-----: | :------------------------------ | :------------------------------------------------------------------------ |
|  [01]   | `s.Assert` / `s.AssertAndTrack` | `(params BoolExpr[])` / `(BoolExpr constraint, BoolExpr trackingLiteral)` |
|  [02]   | `s.Check`                       | `(params Expr[] assumptions)` → `Status`                                  |
|  [03]   | `s.Model`                       | `→ Model` (valid after a `SATISFIABLE` check)                             |
|  [04]   | `s.UnsatCore`                   | `→ BoolExpr[]` (valid after an `UNSATISFIABLE` check)                     |
|  [05]   | `s.Push` / `s.Pop`              | `()` / `(uint n)`                                                         |

- [01]-[ASSERT]: add constraints; the tracked form is REQUIRED for a named unsat core.
- [02]-[CHECK]: the SAT/UNSAT/UNKNOWN verdict; assumptions scope a query without a `Push`/`Pop`.
- [03]-[MODEL]: the witnessing assignment; `model.Evaluate(v)` reads each rule variable.
- [04]-[EXPLAIN]: the minimal conflicting tracking literals — the exact violated rule names.
- [05]-[FRAME]: scope a backtrackable assertion frame for incremental what-if checks.

## [04]-[IMPLEMENTATION_LAW]

[CONTEXT_ARENA]:
- ONE `Context` owns every `Expr`/`Sort`/`Solver`/`Model` for a solve and is disposed at the boundary (`using`), releasing the native AST arena; an expression minted by one context is never mixed into another. A rule-verify pass builds a fresh context per content-keyed request, asserts, checks, projects the verdict, and disposes — the native handles never outlive the `AssessmentResult`.
- Z3 is single-threaded per context; a parallel sweep runs one context per worker (the `Analysis/assessment` Sweep's `JobGraph` fan-out), never a shared context.

[STACKING]:
- `Solver/satisfy#RULE_SATISFACTION`: a typed rule set lowers to Z3 assertions NATURALLY from `SymbolicExpr` — the CAS (`api-angourimath.md`) is the lowering source, its `Entity` tree walked to `Context.Mk*` terms. The verdict surfaces as an `AssessmentResult`: `SATISFIABLE` carries the `Model` witness, `UNSATISFIABLE` the `UnsatCore` naming the violated rules, `UNKNOWN` a typed `(Solve, Numeric)` shortfall. CP-SAT OPTIMIZES, Z3 VERIFIES-and-EXPLAINS — orthogonal concerns, never a second optimizer.
- `Symbolic/expression` (the lowering source): a `SymbolicExpr` constraint (`==`/`<=`/`>=` over a units-checked expression) lowers term-by-term to `MkEq`/`MkLe`/`MkGe`; nonlinear terms (`MkMul`/`MkPower`) reach the NRA/NIA theory the CP-SAT lane cannot express.
- `Runtime/scheduling` (the Sweep substrate): a rule-verify job is a `JobGraph` node keyed by the rule-set + input content key; one `Context` per job, disposed at completion.
- `Google.OrTools` (`api-ortools.md`): the DISJOINT sibling — OrTools CP-SAT/MILP finds an optimal feasible assignment, Z3 proves a fixed assignment satisfies (or names why not); a compliance check that must EXPLAIN its failure is Z3's, an objective to minimize is OrTools'.

[RAIL_LAW]:
- Package: `Microsoft.Z3` `4.12.2` (MIT, netstandard2.0 managed wrapper; osx-arm64 `libz3` Forge-provisioned)
- Owns: SMT satisfiability + explanation over NRA/NIA/boolean/bit-vector/array/UF theories — `Context` AST construction, `Solver` incremental assert/check, `Model` witness extraction, `UnsatCore` conflict explanation, `Optimize` objectives
- Accept: a typed rule set lowered from `SymbolicExpr`, verified for SAT/UNSAT with a witnessing model or a rule-naming unsat core, surfaced as an `AssessmentResult`
- Reject: an optimization objective where CP-SAT/MILP belongs (Z3 verifies, OrTools optimizes); a shared context across sweep workers; a native `libz3` assumption on osx-arm64 absent the Forge provisioning; a `Context`/`Model`/`Solver` leaked past the `AssessmentResult` boundary

# [RASM_COMPUTE_API_ANGOURIMATH]

`AngouriMath` is the categorical-best managed CAS: a symbolic-algebra engine whose `Entity` expression tree natively owns parsing, canonical simplification, equation solving, differentiation, integration, limits, LaTeX rendering, and `Compile`-to-delegate evaluation — the depth `MathNet.Symbolics` (self-described "basic", no solve/integrate/limits) lacks. It is the `[V11]` CAS re-base of the Symbolic folder: `SymbolicExpr` re-bases its identity on AngouriMath's canonical normal form (the `[ComplexValueObject]` content-key law SURVIVES — identity stays the canonical-NF seed-zero `XxHash128`, the engine's `Entity` replacing the F# `Expression` as the stored-but-excluded member), and it collapses the `FParsec` parser and the `compileExpression` machinery into ONE owner. The re-base is DETERMINISM-GATED: it holds only if the canonical normal form is deterministic across process runs and pin-stable (content-key drift across versions is the disqualifier — probe with frozen expression fixtures at the leg); on failure the corpus keeps `MathNet.Symbolics` (`FParsec` dies transitive-only either way) and records the solve/integrate gap as a bounded charter statement, never a second CAS beside the first.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `AngouriMath`
- package: `AngouriMath`
- version: `1.4.0` (feed stable, active)
- license: MIT
- assembly: `AngouriMath`
- namespace: `AngouriMath` (the `Entity` hierarchy), `static AngouriMath.MathS` (the parse/solve/build facade)
- dependencies: `Antlr4.Runtime.Standard`, `GenericTensor`, `HonkSharp`, `PeterO.Numbers` — all permissive; the `net7.0` asset binds on `net10.0`
- roster consequence: admitting this REMOVES `MathNet.Symbolics` + `MathNet.Numerics.FSharp` + the `FParsec` direct admission (the parser AngouriMath's own ANTLR front-end supersedes) — the four-dependency F#-weight CAS collapses to this one owner
- rail: symbolic

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the `Entity` expression tree
- namespace: `AngouriMath`
- rail: symbolic

| [INDEX] | [SYMBOL]           | [RAIL]   | [CAPABILITY]                                                                                              |
| :-----: | :----------------- | :------- | :------------------------------------------------------------------------------------------------------- |
|  [01]   | `Entity`           | symbolic | the immutable expression-tree root; implicit `string`→`Entity` conversion (`Entity e = "x + 2"`, caching), `Vars` (`IEnumerable<Entity.Variable>` free variables), `Nodes`/`DirectChildren` traversal, `Simplified`/`InnerSimplified`, `Stringize()`/`ToString()`. Owns the operation methods enumerated in [03] |
|  [02]   | `Entity.Variable`  | symbolic | a free symbol; `MathS.Variable("x")` mints it, and it is the argument every solve/differentiate/integrate/limit/compile call binds on |
|  [03]   | `Entity.Number`    | symbolic | the numeric-tower leaf: `Entity.Number.Integer`/`.Rational`/`.Real`/`.Complex` (exact via `PeterO.Numbers` big rationals — the exactness the content key depends on) |
|  [04]   | `Entity.Set`       | symbolic | the solve-result carrier — `FiniteSet`/`Interval`/`ConditionalSet`; a `Solve` returns the solution `Set`, never a bare array, so an empty/parametric/conditional solution is a typed value |
|  [05]   | `Entity.Boolean` / `Entity.Statement` | symbolic | boolean-sorted terms and (in)equality statements (`x2 = 16`, `a > b`) — the constraint form a rule lowers from and `SolveBooleanSystem` consumes |
|  [06]   | `FastExpression`   | symbolic | the compiled-delegate result of `Entity.Compile(vars…)`; `Substitute(args)`/`Call(args)` evaluates the numeric function with the tree walk eliminated — the `lowering.md` `CompiledExpr` cache target |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: parse and build — `MathS`
- namespace: `AngouriMath`
- rail: symbolic

| [INDEX] | [SURFACE]                       | [CALL_SHAPE]                                                                              | [CAPABILITY]                                                  |
| :-----: | :------------------------------ | :--------------------------------------------------------------------------------------- | :----------------------------------------------------------- |
|  [01]   | `MathS.FromString`              | `(string)` → `Entity` (cached) / implicit `Entity e = "..."`                             | parse an expression — the ANTLR front-end replacing the `FParsec` parser |
|  [02]   | `MathS.Variable`                | `(string name)` → `Entity.Variable`                                                       | mint a free symbol                                            |
|  [03]   | `MathS.Sin` / `Cos` / `Sqrt` / `Pow` / `Ln` / … | `(Entity …)` → `Entity`                                                    | typed builders composing an `Entity` without string parsing  |
|  [04]   | `MathS.Equations`               | `(params Entity[])` → an equation system                                                 | the multi-equation solve input                               |

[ENTRYPOINT_SCOPE]: symbolic operations — `Entity`
- namespace: `AngouriMath`
- rail: symbolic

| [INDEX] | [SURFACE]                       | [CALL_SHAPE]                                                                              | [CAPABILITY]                                                  |
| :-----: | :------------------------------ | :--------------------------------------------------------------------------------------- | :----------------------------------------------------------- |
|  [01]   | `entity.Simplify`               | `([int level])` → `Entity`                                                                | canonical-form reduction — the normal form `SymbolicExpr`'s content key hashes (the determinism gate) |
|  [02]   | `entity.Differentiate`          | `(Entity.Variable)` → `Entity` (also the `derivative(f, x, order)` node)                  | symbolic derivative — the exact-gradient source for the cost/QTO formula lane |
|  [03]   | `entity.Integrate`              | `(Entity.Variable)` → `Entity` (also the `integral(f, x)` node)                           | symbolic antiderivative — the quantity-integral AEC formula row |
|  [04]   | `entity.Limit`                  | `(Entity.Variable, Entity dest[, ApproachFrom])` → `Entity`                               | the limit `MathNet.Symbolics` cannot compute                 |
|  [05]   | `entity.Solve`                  | `(Entity.Variable)` → `Entity.Set` / `SolveEquation(var)`                                 | equation solving — the cut-parameter inversion the AEC formula consumers reach |
|  [06]   | `entity.Substitute`             | `(Entity from, Entity to)` → `Entity`                                                     | symbolic substitution (a variable or a sub-tree)             |
|  [07]   | `entity.EvalNumerical` / `EvalBoolean` | `()` → `Entity.Number.Complex` / `Entity.Boolean`                                  | force numeric/boolean evaluation of a closed expression      |
|  [08]   | `entity.Compile`                | `(params Entity.Variable[])` → `FastExpression`                                           | compile to a fast delegate — the `lowering.md` `CompiledExpr` L1-cache target (the content key stays the PAGE's, not the engine's) |
|  [09]   | `entity.Latexise`               | `()` → `string`                                                                           | LaTeX rendering for the receipt/report projection            |

## [04]-[IMPLEMENTATION_LAW]

[IDENTITY_REBASE]:
- `SymbolicExpr`'s `[ComplexValueObject]` content-key law is UNCHANGED: the identity stays the canonical-normal-form seed-zero `XxHash128` over the `Simplify()`-reduced `Stringize()` bytes; the AngouriMath `Entity` REPLACES the stored-but-excluded F# `Expression` member. The determinism gate binds here — the canonical NF must be stable across process runs and pin-stable, else the content key drifts and the re-base is rejected.
- `dimensional.md`'s Q⁷ dimensional fold re-bases its expression walk on the `Entity.Nodes`/`DirectChildren` traversal; `units.md` is UNTOUCHED except its E14 typed-`CorrelationId` repair.

[STACKING]:
- `Symbolic/lowering#COMPILED_EXPR`: `Entity.Compile(vars)` → `FastExpression` re-bases the compiled-expression cache; the L1-over-`CacheLane.ModelResult` discipline and the `Rasm.Persistence/Query/cache` seam-98 crossing are UNCHANGED — the content key is the page's own, not the engine's.
- `Solver/satisfy` (`api-microsoft-z3.md`): the CAS is the Z3 LOWERING SOURCE — a `SymbolicExpr` constraint (an `Entity.Statement`) walks term-by-term to `Context.Mk*` assertions, so the rule-satisfaction owner consumes the `Entity` tree the CAS canonicalizes.
- the cost/QTO formula lane's `SymbolicOp` axis gains `Solve`/`Integrate` rows the AEC formula consumers (cut-parameter inversion, quantity integrals) reach — capability `MathNet.Symbolics` never had.

[RAIL_LAW]:
- Package: `AngouriMath` `1.4.0` (MIT, net7.0 asset binding on net10; permissive ANTLR/GenericTensor/HonkSharp/PeterO deps)
- Owns: the symbolic CAS — parse, canonical simplify, solve/differentiate/integrate/limit, substitution, numeric/boolean evaluation, `Compile`-to-delegate, LaTeX; the `Entity` tree `SymbolicExpr` re-bases on
- Accept: a symbolic expression parsed/built, canonicalized for the content key, solved/integrated/differentiated for the AEC formula lane, compiled for the lowering cache, or lowered to a Z3 rule set
- Reject: a second CAS beside this one (the fallback KEEPS `MathNet.Symbolics`, never adds a rival); a re-base whose canonical NF drifts across runs/versions (the determinism disqualifier); leaking the raw `Entity` where `SymbolicExpr`'s content-keyed value object is the durable surface; re-deriving the numeric solve the `Tensor`/`Solver` owners hold (the CAS is symbolic, not the numeric spine)

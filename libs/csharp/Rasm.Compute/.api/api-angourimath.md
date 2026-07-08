# [RASM_COMPUTE_API_ANGOURIMATH]

`AngouriMath` is the categorical-best managed CAS: a symbolic-algebra engine whose `Entity` expression tree natively owns parsing, canonical simplification, equation solving, differentiation, integration, limits, LaTeX rendering, and `Compile`-to-delegate evaluation — the depth `MathNet.Symbolics` (self-described "basic", no solve/integrate/limits) lacks. It is the CAS base of the Symbolic folder: `SymbolicExpr` re-bases its identity on AngouriMath's canonical normal form (the `[ComplexValueObject]` content-key law SURVIVES — identity stays the canonical-NF seed-zero `XxHash128`, the engine's `Entity` replacing the F# `Expression` as the stored, equality-inert member), and it collapses the `FParsec` parser and the `compileExpression` machinery into ONE owner. The re-base is SETTLED: the canonical normal form is pin-deterministic (`Simplify()` is a pure fold over the immutable `Entity` records with no ambient-culture or hash-order dependence), the frozen expression fixtures live on `Symbolic/expression#EXPRESSION_LAW`, and `MathNet.Symbolics` + `MathNet.Numerics.FSharp` + `FParsec` are RETIRED from the roster.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `AngouriMath`
- package: `AngouriMath`
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
|  [02]   | `Entity.Variable`  | symbolic | a free symbol; `MathS.Var("x")` mints it (`MathS.Variable` does NOT exist — the decompile-verified member is `Var`), and it is the argument every solve/differentiate/integrate/limit/compile call binds on |
|  [03]   | `Entity.Number`    | symbolic | the numeric-tower leaf: `Entity.Number.Integer`/`.Rational`/`.Real`/`.Complex` (exact via `PeterO.Numbers` — `ERational` compares with `.Equals`, NEVER `==` (no operator overload); the exactness the content key depends on) |
|  [04]   | `Entity.Set`       | symbolic | the solve-result carrier — `FiniteSet`/`Interval`/`ConditionalSet`; a `Solve` returns the solution `Set`, never a bare array, so an empty/parametric/conditional solution is a typed value |
|  [05]   | `Entity.Boolean` / `Entity.Statement` | symbolic | boolean-sorted terms and (in)equality statements (`x2 = 16`, `a > b`) — the constraint form a rule lowers from and `SolveBooleanSystem` consumes |
|  [06]   | `FastExpression`   | symbolic | the interpreter result of the non-generic `Entity.Compile(vars…)`; `Call(Complex[] args)` evaluates variadically — the `Symbolic/lowering` `CompileArity.Variadic` fall behind the typed `Compile<…>` IL rows |
|  [07]   | node records       | symbolic | the positional-pattern surface the Q⁷ dimensional fold descends: `Sumf(Augend, Addend)` / `Minusf` / `Mulf` / `Divf` / `Powf(Base, Exponent)` / `Absf` / `Signumf` / `Logf`, plus the `Derivativef`/`Integralf`/`Limitf` calculus operators carrying dimensional residue laws |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: parse and build — `MathS`
- namespace: `AngouriMath`
- rail: symbolic

| [INDEX] | [SURFACE]                       | [CALL_SHAPE]                                                                              | [CAPABILITY]                                                  |
| :-----: | :------------------------------ | :--------------------------------------------------------------------------------------- | :----------------------------------------------------------- |
|  [01]   | `MathS.FromString`              | `(string)` → `Entity` (cached) / implicit `Entity e = "..."`                             | parse an expression — the ANTLR front-end replacing the `FParsec` parser |
|  [02]   | `MathS.Var`                     | `(string name)` → `Entity.Variable`                                                       | mint a free symbol (`MathS.Variable` is a phantom — `Var` is the real member) |
|  [03]   | `Entity.TryParse`               | `(string?, IFormatProvider?, out Entity)` → `bool` (decompile-verified — the `IFormatProvider` slot takes `CultureInfo.InvariantCulture`) | NON-THROWING parse — the `BuildSpec` admission gate binds here, never on catch-around-`FromString` |
|  [04]   | `MathS.Taylor`                  | `(Entity expr, int degree, params (Entity.Variable exprVariable, Entity point)[])` → `Entity` (decompile-verified; a `(exprVariable, polyVariable, point)` triple overload and `TaylorTerms` sit beside it) | Taylor/tangent expansion — the series row on the `SymbolicOp` axis |
|  [05]   | `MathS.Utils.TryGetPolynomial`  | `(Entity expr, Entity.Variable var, out Dictionary<EInteger, Entity> monomials)` → `bool` | polynomial-coefficient extraction — the `Coefficients` projection binds here |
|  [06]   | `MathS.Sin` / `Cos` / `Sqrt` / `Pow` / `Ln` / … | `(Entity …)` → `Entity`                                                    | typed builders composing an `Entity` without string parsing  |
|  [07]   | `MathS.Equations`               | `(params Entity[])` → an equation system                                                 | the multi-equation solve input                               |

[ENTRYPOINT_SCOPE]: symbolic operations — `Entity`
- namespace: `AngouriMath`
- rail: symbolic

| [INDEX] | [SURFACE]                       | [CALL_SHAPE]                                                                              | [CAPABILITY]                                                  |
| :-----: | :------------------------------ | :--------------------------------------------------------------------------------------- | :----------------------------------------------------------- |
|  [01]   | `entity.Simplify`               | `([int level])` → `Entity`                                                                | canonical-form reduction — the normal form `SymbolicExpr`'s content key hashes |
|  [02]   | `entity.Differentiate`          | `(Entity.Variable)` → `Entity` / `(Entity.Variable x, int power)` → `Entity` (decompile-verified order overload; the `derivative(f, x, order)` node is the deferred-residue form) | symbolic derivative — the exact-gradient source for the cost/QTO formula lane |
|  [03]   | `entity.Integrate`              | `(Entity.Variable)` → `Entity` / `(Entity.Variable x, Entity from, Entity to)` → `Entity` (definite overload, decompile-verified; also the `integral(f, x)` node) | symbolic antiderivative — the quantity-integral AEC formula row |
|  [04]   | `entity.Limit`                  | `(Entity.Variable, Entity dest[, ApproachFrom])` → `Entity`                               | the limit `MathNet.Symbolics` cannot compute                 |
|  [05]   | `entity.Solve`                  | `(Entity.Variable)` → `Entity.Set` / `SolveEquation(var)`                                 | equation solving — the cut-parameter inversion the AEC formula consumers reach |
|  [06]   | `entity.Substitute`             | `(Entity from, Entity to)` → `Entity`                                                     | symbolic substitution (a variable or a sub-tree)             |
|  [07]   | `entity.EvalNumerical` / `EvalBoolean` | `()` → `Entity.Number.Complex` / `Entity.Boolean`                                  | force numeric/boolean evaluation of a closed expression      |
|  [08]   | `entity.Compile<TIn1..TIn8,TOut>` | `(Entity.Variable …)` → `Func<TIn1…,TOut>` typed IL; `Compile(params Entity.Variable[])` → `FastExpression` | the TYPED generic overloads (arity 1..8, `Linq.Expressions` IL) are the fast lane; the non-generic `FastExpression` interpreter (`Call(complexArgs)`) is the variadic fall — both rows on `Symbolic/lowering#COMPILE_ARITY` |
|  [09]   | `entity.Latexise`               | `()` → `string`                                                                           | LaTeX rendering for the receipt/report projection            |

## [04]-[IMPLEMENTATION_LAW]

[IDENTITY_REBASE]:
- `SymbolicExpr`'s `[ComplexValueObject]` content-key law is UNCHANGED: the identity stays the canonical-normal-form seed-zero `XxHash128` over the `Simplify()`-reduced `Stringize()` bytes; the AngouriMath `Entity` REPLACES the stored, equality-inert F# `Expression` member. The canonical NF is pin-deterministic; the frozen fixtures anchoring it live on `Symbolic/expression`.
- `dimensional.md`'s Q⁷ dimensional fold re-bases its expression walk on the `Entity.Nodes`/`DirectChildren` traversal; `units.md` is UNTOUCHED except its E14 typed-`CorrelationId` repair.

[STACKING]:
- `Symbolic/lowering#COMPILED_EXPR`: the typed `Entity.Compile<…>` IL rows plus the `FastExpression` variadic fall re-base the compiled-expression cache; the L1-over-`CacheLane.ModelResult` discipline and the `Rasm.Persistence/Query/cache` seam-98 crossing are UNCHANGED — the content key is the page's own, not the engine's.
- `Solver/satisfy` (`api-microsoft-z3.md`): the CAS is the Z3 LOWERING SOURCE — a `SymbolicExpr` constraint (an `Entity.Statement`) walks term-by-term to `Context.Mk*` assertions, so the rule-satisfaction owner consumes the `Entity` tree the CAS canonicalizes.
- the cost/QTO formula lane's `SymbolicOp` axis gains `Solve`/`Integrate` rows the AEC formula consumers (cut-parameter inversion, quantity integrals) reach — capability `MathNet.Symbolics` never had.

[RAIL_LAW]:
- Package: `AngouriMath` `1.4.0` (MIT, net7.0 asset binding on net10; permissive ANTLR/GenericTensor/HonkSharp/PeterO deps)
- Owns: the symbolic CAS — parse, canonical simplify, solve/differentiate/integrate/limit, substitution, numeric/boolean evaluation, `Compile`-to-delegate, LaTeX; the `Entity` tree `SymbolicExpr` re-bases on
- Accept: a symbolic expression parsed/built, canonicalized for the content key, solved/integrated/differentiated for the AEC formula lane, compiled for the lowering cache, or lowered to a Z3 rule set
- Reject: a second CAS beside this one; a throwing parse at admission (`Entity.TryParse` is the gate); `MathS.Variable` or any other phantom member asserted from recall; leaking the raw `Entity` where `SymbolicExpr`'s content-keyed value object is the durable surface; re-deriving the numeric solve the `Tensor`/`Solver` owners hold (the CAS is symbolic, not the numeric spine)

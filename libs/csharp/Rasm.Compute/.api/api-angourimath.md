# [RASM_COMPUTE_API_ANGOURIMATH]

`AngouriMath` is the categorical-best managed CAS: a symbolic-algebra engine whose `Entity` expression tree natively owns parsing, canonical simplification, equation solving, differentiation, integration, limits, LaTeX rendering, and `Compile`-to-delegate evaluation — the depth `MathNet.Symbolics` (self-described "basic", no solve/integrate/limits) lacks. It is the CAS base of the Symbolic folder: `SymbolicExpr` re-bases its identity on AngouriMath's canonical normal form (the `[ComplexValueObject]` content-key law SURVIVES — identity stays the canonical-NF seed-zero `XxHash128`, the engine's `Entity` replacing the F# `Expression` as the stored, equality-inert member), and it collapses the `FParsec` parser and the `compileExpression` machinery into ONE owner. Re-base status is SETTLED: canonical normalization is pin-deterministic (`Simplify()` is a pure fold over the immutable `Entity` records with no ambient-culture or hash-order dependence), frozen expression fixtures live on `Symbolic/expression#EXPRESSION_LAW`, and `MathNet.Symbolics` + `MathNet.Numerics.FSharp` + `FParsec` are RETIRED from the roster.

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

| [INDEX] | [SYMBOL]                              | [ROLE]                              |
| :-----: | :------------------------------------ | :---------------------------------- |
|  [01]   | `Entity`                              | immutable expression-tree root      |
|  [02]   | `Entity.Variable`                     | free symbol                         |
|  [03]   | `Entity.Number`                       | numeric-tower leaf                  |
|  [04]   | `Entity.Set`                          | solve-result carrier                |
|  [05]   | `Entity.Boolean` / `Entity.Statement` | boolean + (in)equality statements   |
|  [06]   | `FastExpression`                      | variadic interpreter result         |
|  [07]   | node records                          | positional-pattern operator surface |

- [01]-[ENTITY]: implicit `string`→`Entity` (`Entity e = "x + 2"`, caching), `Vars` (`IEnumerable<Entity.Variable>`), `Nodes`/`DirectChildren` traversal, `Simplified`/`InnerSimplified`, `Stringize()`/`ToString()`; owns the [03] operation methods.
- [02]-[Entity.Variable]: `MathS.Var("x")` mints it (`MathS.Variable` does NOT exist — the member is `Var`), the argument every solve/differentiate/integrate/limit/compile binds on.
- [03]-[Entity.Number]: `.Integer`/`.Rational`/`.Real`/`.Complex`, exact via `PeterO.Numbers`; `ERational` compares with `.Equals`, never `==` (no operator overload) — the exactness the content key depends on.
- [04]-[Entity.Set]: `FiniteSet`/`Interval`/`ConditionalSet`; `Solve` returns a `Set`, never a bare array, so an empty/parametric/conditional solution is a typed value.
- [05]-[Entity.Boolean]: boolean-sorted terms + (in)equality statements (`x2 = 16`, `a > b`) — the constraint form a rule lowers from and `SolveBooleanSystem` consumes.
- [06]-[FASTEXPRESSION]: the non-generic `Entity.Compile(vars…)` result; `Call(Complex[] args)` evaluates variadically — the `CompileArity.Variadic` fall behind the typed `Compile<…>` IL rows.
- [07]-[NODE_RECORDS]: the Q⁷ dimensional fold descends `Sumf(Augend, Addend)`/`Minusf`/`Mulf`/`Divf`/`Powf(Base, Exponent)`/`Absf(Argument)`/`Signumf(Argument)`/`Logf`, plus `Derivativef`/`Integralf`/`Limitf` carrying dimensional residue laws; the statement records the Z3 rule walk descends are `Equalsf(Left, Right)`/`Greaterf`/`GreaterOrEqualf`/`Lessf`/`LessOrEqualf` (`ComparisonSign` subtypes) and `Andf(Left, Right)`/`Orf(Left, Right)`/`Xorf(Left, Right)`/`Impliesf(Assumption, Conclusion)`/`Notf(Argument)` (`Statement` subtypes), all positional-pattern-matchable sealed records.

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: parse and build — `MathS`
- namespace: `AngouriMath`
- rail: symbolic

- call: `MathS` is the parse/solve/build facade; string parse is cached and also rides implicit `Entity e = "..."`.

| [INDEX] | [SURFACE]                                                                                         | [CAPABILITY]                        |
| :-----: | :------------------------------------------------------------------------------------------------ | :---------------------------------- |
|  [01]   | `MathS.FromString(string)` → `Entity`                                                             | parse (ANTLR, replaces `FParsec`)   |
|  [02]   | `MathS.Var(string name)` → `Entity.Variable`                                                      | mint a free symbol                  |
|  [03]   | `Entity.TryParse(string?, IFormatProvider?, out Entity)` → `bool`                                 | non-throwing parse — admission gate |
|  [04]   | `MathS.Taylor(Entity expr, int degree, params (Entity.Variable, Entity)[])` → `Entity`            | Taylor/tangent expansion            |
|  [05]   | `MathS.Utils.TryGetPolynomial(Entity, Entity.Variable, out Dictionary<EInteger,Entity>)` → `bool` | polynomial-coefficient extraction   |
|  [06]   | `MathS.Sin` / `Cos` / `Sqrt` / `Pow` / `Ln` / … `(Entity …)` → `Entity`                           | typed `Entity` builders             |
|  [07]   | `MathS.Equations(params Entity[])` → equation system                                              | the multi-equation solve input      |

- [03]-[Entity.TryParse]: the `BuildSpec` admission gate binds here, never catch-around-`FromString`; the `IFormatProvider` slot takes `CultureInfo.InvariantCulture`.
- [04]-[MathS.Taylor]: a `(exprVariable, polyVariable, point)` triple overload and `TaylorTerms` sit beside the `params` form.
- [05]-[TRYGETPOLYNOMIAL]: the `Coefficients` projection binds here.

[ENTRYPOINT_SCOPE]: symbolic operations — `Entity`
- namespace: `AngouriMath`
- rail: symbolic

| [INDEX] | [SURFACE]                                                                  | [CAPABILITY]                                            |
| :-----: | :------------------------------------------------------------------------- | :------------------------------------------------------ |
|  [01]   | `entity.Simplify([int level])` → `Entity`                                  | canonical-form reduction — the content-key NF           |
|  [02]   | `entity.Differentiate(Entity.Variable[, int power])` → `Entity`            | symbolic derivative — exact gradient for cost/QTO       |
|  [03]   | `entity.Integrate(Entity.Variable[, Entity from, Entity to])` → `Entity`   | symbolic antiderivative — quantity-integral row         |
|  [04]   | `entity.Limit(Entity.Variable, Entity dest[, ApproachFrom])` → `Entity`    | the limit `MathNet.Symbolics` cannot compute            |
|  [05]   | `entity.Solve(Entity.Variable)` → `Entity.Set`                             | equation solving — cut-parameter inversion              |
|  [06]   | `entity.Substitute(Entity from, Entity to)` → `Entity`                     | symbolic substitution (variable or sub-tree)            |
|  [07]   | `entity.EvalNumerical()` / `EvalBoolean()`                                 | force eval → `Entity.Number.Complex` / `Entity.Boolean` |
|  [08]   | `entity.Compile<TIn1..TIn8,TOut>(…)` / `Compile(params Entity.Variable[])` | typed IL fast lane + variadic `FastExpression` fall     |
|  [09]   | `entity.Latexise()` → `string`                                             | LaTeX rendering for the receipt/report projection       |
|  [10]   | `entity.Evaled` → `Entity`                                                 | non-throwing cached reduction (unbound stays symbolic)  |
|  [11]   | `entity.Expand(int level = 2)` / `Factorize(int level = 2)`                | normalization routes beside `Simplify`                  |
|  [12]   | `entity.InnerSimplified` → `Entity`                                        | cheap structural reduction, no aggressive rules         |
|  [13]   | `entity.Nodes` → `IEnumerable<Entity>`                                     | full-tree node census (residue gates read it)           |
|  [14]   | `entity.Vars` → `IEnumerable<Entity.Variable>`                             | free-symbol census (pi/e excluded)                      |

- [02]-[DIFFERENTIATE]: `(Entity.Variable x, int power)` order overload; the `derivative(f, x, order)` node is the deferred-residue form.
- [03]-[INTEGRATE]: `(Entity.Variable x, Entity from, Entity to)` definite overload; also the `integral(f, x)` node.
- [05]-[SOLVE]: the `SolveEquation(var)` alias; the cut-parameter inversion the AEC formula consumers reach.
- [08]-[COMPILE]: the typed generic overloads (arity 1..8, `Linq.Expressions` IL) are the fast lane; the non-generic `FastExpression` interpreter (`Call(complexArgs)`) is the variadic fall — both on `Symbolic/lowering#COMPILE_ARITY`.
- [10]-[EVALED]: the admitted evaluation read — `EvalNumerical()` throws on an unbound symbol where `Evaled` leaves the residue symbolic for the typed `NumberBox` decline.

[NODE_SHAPES]: decompile-verified record shapes beyond the `[07]-[NODE_RECORDS]` roster.
- numeric leaves: `Entity.Number.Rational.ERational` (`PeterO.Numbers`), `Entity.Number.Real.EDecimal`, `Entity.Number.Real.AsDouble()`.
- unary/binary floors: `IUnaryNode.NodeChild`; `IBinaryNode.NodeFirstChild`/`NodeSecondChild`.
- regime switch: `Entity.Piecewise(IEnumerable<Providedf>)` with `Cases`; `Entity.Providedf(Entity Expression, Entity Predicate)` — the first-class carrier for design-code piecewise formulas the dimensional fold proves.

## [04]-[IMPLEMENTATION_LAW]

[IDENTITY_REBASE]:
- `SymbolicExpr`'s `[ComplexValueObject]` content-key law is UNCHANGED: identity stays the canonical-normal-form seed-zero `XxHash128` over the `Simplify()`-reduced `Stringize()` bytes; AngouriMath `Entity` REPLACES the stored, equality-inert F# `Expression` member. Canonical NF is pin-deterministic; frozen fixtures anchoring it live on `Symbolic/expression`.
- `dimensional.md`'s Q⁷ dimensional fold re-bases its expression walk on the `Entity.Nodes`/`DirectChildren` traversal; `units.md` is UNTOUCHED except its E14 typed-`CorrelationId` repair.

[STACKING]:
- `Symbolic/lowering#COMPILED_EXPR`: binds typed `Entity.Compile<…>` IL rows plus the `FastExpression` variadic fall into the compiled-expression cache; L1-over-`CacheLane.ModelResult` discipline and the `Rasm.Persistence/Query/cache` seam-98 crossing are UNCHANGED — content key ownership stays with the page, not the engine.
- `Solver/satisfy` (`api-microsoft-z3.md`): the CAS is the Z3 LOWERING SOURCE — a `SymbolicExpr` constraint (an `Entity.Statement`) walks term-by-term to `Context.Mk*` assertions, so the rule-satisfaction owner consumes the `Entity` tree the CAS canonicalizes.
- Cost/QTO formula lane's `SymbolicOp` axis gains `Solve`/`Integrate` rows the AEC formula consumers (cut-parameter inversion, quantity integrals) reach — capability `MathNet.Symbolics` never had.

[RAIL_LAW]:
- Package: `AngouriMath` `1.4.0` (MIT, net7.0 asset binding on net10; permissive ANTLR/GenericTensor/HonkSharp/PeterO deps)
- Owns: the symbolic CAS — parse, canonical simplify, solve/differentiate/integrate/limit, substitution, numeric/boolean evaluation, `Compile`-to-delegate, LaTeX; the `Entity` tree `SymbolicExpr` re-bases on
- Accept: a symbolic expression parsed/built, canonicalized for the content key, solved/integrated/differentiated for the AEC formula lane, compiled for the lowering cache, or lowered to a Z3 rule set
- Reject: a second CAS beside this one; a throwing parse at admission (`Entity.TryParse` is the gate); `MathS.Variable` or any other phantom member asserted from recall; leaking the raw `Entity` where `SymbolicExpr`'s content-keyed value object is the durable surface; re-deriving the numeric solve the `Tensor`/`Solver` owners hold (the CAS is symbolic, not the numeric spine)

# [RASM_COMPUTE_API_ANGOURIMATH]

`AngouriMath` owns the managed computer-algebra surface: an immutable `Entity` expression tree that parses, canonically simplifies, solves, differentiates, integrates, takes limits, renders LaTeX, and compiles to a delegate. It is the CAS base of the Symbolic lane — `SymbolicExpr` wraps the canonical `Entity` as its content-keyed value object, and the AEC cost/QTO formula lane with the Z3 solver both consume the tree this surface canonicalizes.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `AngouriMath`
- package: `AngouriMath` (MIT)
- assembly: `AngouriMath`
- namespace: `AngouriMath` (the `Entity` hierarchy), `static AngouriMath.MathS` (the parse/solve/build facade)
- depends: `Antlr4.Runtime.Standard`, `GenericTensor`, `HonkSharp`, `PeterO.Numbers` — permissive; the `net7.0` asset binds on `net10.0`
- rail: symbolic

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the `Entity` expression tree

| [INDEX] | [SYMBOL]                              | [TYPE_FAMILY] | [CAPABILITY]                        |
| :-----: | :------------------------------------ | :------------ | :---------------------------------- |
|  [01]   | `Entity`                              | union         | immutable expression-tree root      |
|  [02]   | `Entity.Variable`                     | record        | free symbol                         |
|  [03]   | `Entity.Number`                       | record        | numeric-tower leaf                  |
|  [04]   | `Entity.Set`                          | record        | solve-result carrier                |
|  [05]   | `Entity.Boolean` / `Entity.Statement` | record        | boolean and (in)equality statements |
|  [06]   | `FastExpression`                      | class         | variadic interpreter result         |
|  [07]   | node records                          | record        | positional-pattern operator surface |

- `Entity`: implicit `string`→`Entity` (`Entity e = "x + 2"`, cached), `Vars`/`Nodes`/`DirectChildren` traversal, `Simplified`/`InnerSimplified`, `Stringize()`; owns the [03] operation methods.
- `Entity.Variable`: `MathS.Var(string)` mints it, the argument every solve/differentiate/integrate/limit/compile binds on.
- `Entity.Number`: `.Integer`/`.Rational`/`.Real`/`.Complex`, exact via `PeterO.Numbers` — `ERational`, `EDecimal`, `AsDouble()`; `ERational` compares by `.Equals`, never `==` (no operator overload), the exactness the content key depends on.
- `Entity.Set`: `FiniteSet`/`Interval`/`ConditionalSet`; `Solve` returns a `Set`, so an empty, parametric, or conditional solution is a typed value.
- `Entity.Boolean`: boolean-sorted terms and (in)equality statements (`x2 = 16`, `a > b`), the constraint form `Solver/satisfy` lowers to Z3.
- `FastExpression`: `Entity.Compile(vars)` returns it; `Call(Complex[])` evaluates variadically behind the typed `Compile<…>` IL rows.
- node records: positional-pattern-matchable sealed records over `IUnaryNode.NodeChild` and `IBinaryNode.NodeFirstChild`/`NodeSecondChild` — arithmetic `Sumf` `Minusf` `Mulf` `Divf` `Powf` `Absf` `Signumf` `Logf`; calculus `Derivativef` `Integralf` `Limitf`; comparison `Equalsf` `Greaterf` `GreaterOrEqualf` `Lessf` `LessOrEqualf`; statement `Andf` `Orf` `Xorf` `Impliesf` `Notf`; regime `Entity.Piecewise` over `Providedf(Expression, Predicate)` cases.

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: parse and build — `MathS`

| [INDEX] | [SURFACE]                                                                   | [SHAPE] | [CAPABILITY]                        |
| :-----: | :-------------------------------------------------------------------------- | :------ | :---------------------------------- |
|  [01]   | `MathS.FromString(string)` -> `Entity`                                      | static  | ANTLR parse                         |
|  [02]   | `MathS.Var(string)` -> `Entity.Variable`                                    | static  | mint a free symbol                  |
|  [03]   | `Entity.TryParse(string?, IFormatProvider?, out Entity)` -> `bool`          | static  | non-throwing parse — admission gate |
|  [04]   | `MathS.Taylor(Entity, int, params (Entity.Variable, Entity)[])` -> `Entity` | static  | Taylor/tangent expansion            |
|  [05]   | `MathS.Utils.TryGetPolynomial(Entity, Entity.Variable, out Dictionary)`     | static  | polynomial-coefficient extraction   |
|  [06]   | `MathS.Sin` / `Cos` / `Sqrt` / `Pow` / `Ln`(Entity) -> `Entity`             | static  | typed `Entity` builders             |
|  [07]   | `MathS.Equations(params Entity[])` -> `EquationSystem`                      | static  | multi-equation solve input          |

- `Entity.TryParse`: `BuildSpec` admission binds here; `IFormatProvider` takes `CultureInfo.InvariantCulture`.
- `MathS.Taylor`: a `(exprVariable, polyVariable, point)` triple overload and `TaylorTerms` sit beside the `params` form.
- `MathS.Utils.TryGetPolynomial`: `out Dictionary<EInteger, Entity>` carries the coefficient projection.

[ENTRYPOINT_SCOPE]: symbolic operations — `Entity`

| [INDEX] | [SURFACE]                                                         | [SHAPE]  | [CAPABILITY]                                    |
| :-----: | :---------------------------------------------------------------- | :------- | :---------------------------------------------- |
|  [01]   | `entity.Simplify(int)` -> `Entity`                                | instance | canonical-form reduction — the content-key NF   |
|  [02]   | `entity.Differentiate(Entity.Variable, int)` -> `Entity`          | instance | symbolic derivative for cost/QTO gradient       |
|  [03]   | `entity.Integrate(Entity.Variable, Entity, Entity)` -> `Entity`   | instance | symbolic antiderivative — quantity-integral row |
|  [04]   | `entity.Limit(Entity.Variable, Entity, ApproachFrom)` -> `Entity` | instance | symbolic limit                                  |
|  [05]   | `entity.Solve(Entity.Variable)` -> `Entity.Set`                   | instance | equation solving — cut-parameter inversion      |
|  [06]   | `entity.Substitute(Entity, Entity)` -> `Entity`                   | instance | variable or sub-tree substitution               |
|  [07]   | `entity.EvalNumerical()` -> `Entity.Number.Complex`               | instance | force numeric evaluation                        |
|  [08]   | `entity.EvalBoolean()` -> `Entity.Boolean`                        | instance | force boolean evaluation                        |
|  [09]   | `entity.Compile<…>(…)` / `Compile(params Entity.Variable[])`      | instance | typed IL fast lane + variadic `FastExpression`  |
|  [10]   | `entity.Latexise()` -> `string`                                   | instance | LaTeX rendering for the receipt projection      |
|  [11]   | `entity.Evaled` -> `Entity`                                       | property | cached non-throwing reduction                   |
|  [12]   | `entity.Expand(int)` -> `Entity`                                  | instance | expansion normalization                         |
|  [13]   | `entity.Factorize(int)` -> `Entity`                               | instance | factorization normalization                     |
|  [14]   | `entity.InnerSimplified` -> `Entity`                              | property | cheap structural reduction, no aggressive rules |
|  [15]   | `entity.Nodes` -> `IEnumerable<Entity>`                           | property | full-tree node census                           |
|  [16]   | `entity.Vars` -> `IReadOnlyList<Entity.Variable>`                 | property | free-symbol census (pi/e excluded)              |

- `entity.Differentiate`: a `(Entity.Variable, int power)` order overload; `Derivativef` is the deferred-residue node.
- `entity.Integrate`: a `(Entity.Variable, Entity from, Entity to)` definite overload; `Integralf` is the residue node.
- `entity.Solve`: `MathS.SolveEquation(Entity, Entity.Variable)` is the equation-system path the AEC formula consumers reach.
- `entity.Compile`: typed generic overloads (arity 1..8, `Linq.Expressions` IL) are the fast lane; the non-generic `FastExpression` interpreter (`Call(Complex[])`) is the variadic fall.
- `entity.Evaled`: leaves an unbound symbol symbolic where `EvalNumerical()` throws.

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `Entity` is an immutable record tree; `Simplify()` is a deterministic pure fold to canonical normal form with no ambient-culture or hash-order dependence.

[STACKING]:
- `Solver/satisfy`(`api-microsoft-z3.md`): an `Entity.Statement` walks term-by-term to `Context.Mk*` assertions, so the rule-satisfaction owner consumes the `Entity` tree this surface canonicalizes.
- `Symbolic/lowering`: typed `Entity.Compile<…>` IL rows and the `FastExpression` variadic fall bind the compiled-expression cache, keyed off the canonical content key.
- Cost/QTO formula lane's `SymbolicOp` axis binds `Solve`/`Integrate`/`Differentiate` rows for cut-parameter inversion and quantity integrals.

[LOCAL_ADMISSION]:
- One CAS owner; `SymbolicExpr` wraps the `Entity` as its content-keyed value object, the raw `Entity` never crossing that boundary.

[RAIL_LAW]:
- Package: `AngouriMath` (MIT; net7.0 asset binding on net10; permissive ANTLR/GenericTensor/HonkSharp/PeterO deps)
- Owns: the symbolic CAS — parse, canonical simplify, solve/differentiate/integrate/limit, substitution, numeric/boolean evaluation, `Compile`-to-delegate, LaTeX
- Accept: a symbolic expression parsed/built, canonicalized for the content key, solved/integrated/differentiated for the AEC formula lane, compiled for the lowering cache, or lowered to a Z3 rule set
- Reject: a throwing parse at admission where `Entity.TryParse` is the gate; leaking the raw `Entity` where `SymbolicExpr`'s content-keyed value object is the durable surface; re-deriving the numeric solve the `Tensor`/`Solver` owners hold

# [RASM_COMPUTE_API_MATHNET_SYMBOLICS]

`MathNet.Symbolics` supplies the basic computer-algebra surface for the `Rasm.Compute/symbolic` owner: the `Expression` symbolic algebra, simplification over `Rational`/`Algebraic`/`Trigonometric`/`Polynomial`, symbolic differentiation and Taylor expansion via `Calculus`, delegate compilation via `Compile`, infix-string parsing and `LaTeX`/`Infix` projection, and the C#-friendly fluent `SymbolicExpression` wrapper. The package is F# (DU-based `Expression`), depends on `MathNet.Numerics.FSharp` and `FParsec` for infix parsing, and ships netstandard2.0 ALC-safe. The C# owner touches only the simplify/differentiate/compile/parse surface and wraps the F# `Expression` in `SymbolicExpr`; `FParsec` is consumed transitively through `Infix.parse` and is not called directly.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `MathNet.Symbolics`
- package: `MathNet.Symbolics`
- assembly: `MathNet.Symbolics`
- namespace: `MathNet.Symbolics`
- asset: runtime library (managed; F# DU core, MIT)
- rail: symbolic

[PACKAGE_SURFACE]: `MathNet.Numerics.FSharp`
- package: `MathNet.Numerics.FSharp`
- assembly: `MathNet.Numerics.FSharp`
- namespace: `MathNet.Numerics`
- asset: runtime library (F# numeric facade; the `Expression`-to-`FloatingPoint` evaluation seam)
- rail: symbolic

[PACKAGE_SURFACE]: `FParsec`
- package: `FParsec`
- assembly: `FParsec`, `FParsecCS`
- namespace: `FParsec`
- asset: runtime library (parser combinators; consumed transitively through `Infix.parse`, not called directly)
- rail: symbolic

## [2]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: symbolic core and the C# fluent wrapper

| [INDEX] | [SYMBOL]             | [TYPE_FAMILY]  | [CAPABILITY]                                                                                                                                                                                   |
| :-----: | :------------------- | :------------- | :--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
|   [1]   | `Expression`         | F# DU (struct) | the symbolic-expression algebra; cases `Number`/`Approximation`/`Identifier`/`Constant`/`Sum`/`Product`/`Power`/`Function`/`Undefined`/`ComplexInfinity`/`PositiveInfinity`/`NegativeInfinity` |
|   [2]   | `SymbolicExpression` | class          | the C#-friendly fluent wrapper over `Expression`: `.Parse`, `.Differentiate`, `.Compile`, `.ToString`, operator overloads, `.Variables`                                                        |
|   [3]   | `FloatingPoint`      | F# DU          | the evaluation result carrier (`Real`/`Complex`/`RealVector`/`...`) `Evaluate.evaluate` returns                                                                                                |
|   [4]   | `VisualExpression`   | F# DU          | the presentation tree `LaTeX`/`Infix` format over                                                                                                                                              |

## [3]-[FUNCTIONAL_MODULES]

[FUNCTIONAL_MODULE_SCOPE]: the static F# modules the C# owner calls (module-function call form `Module.function arg`)
- rail: symbolic

| [INDEX] | [MODULE]        | [MEMBERS]                                                                                                       | [CAPABILITY]                                                       |
| :-----: | :-------------- | :-------------------------------------------------------------------------------------------------------------- | :----------------------------------------------------------------- |
|   [1]   | `Expression`    | `Symbol`, `FromInt32`/`FromInt64`/`FromInteger`, `Zero`, `One`, `Undefined`, `PositiveInfinity`                 | construct symbolic atoms and constants                             |
|   [2]   | `Calculus`      | `differentiate`, `tangentLine`, `taylor`                                                                        | symbolic differentiation, tangent line, Taylor series              |
|   [3]   | `Rational`      | `simplify`, `rationalize`, `reduce`, `expand`                                                                   | rational-form normalization and simplification                     |
|   [4]   | `Algebraic`     | `expand`, `separateFactors`                                                                                     | algebraic expansion and factor separation                          |
|   [5]   | `Trigonometric` | `expand`, `contract`, `simplify`                                                                                | trig identity rewriting                                            |
|   [6]   | `Polynomial`    | `coefficients`, `collectTerms`, `divide`, `quot`, `remainder`, `gcd`, `partialFraction`, `isPolynomial`         | polynomial algebra, division, gcd, partial fractions               |
|   [7]   | `Structure`     | `substitute`, `map`, `collect`                                                                                  | structural traversal and substitution                              |
|   [8]   | `Infix`         | `parse`, `tryParse`, `parseOrThrow`, `parseOrUndefined`, `format`, `formatStrict`                               | infix-string parse (over `FParsec`) and infix projection           |
|   [9]   | `LaTeX`         | `format`                                                                                                        | LaTeX projection of the expression                                 |
|  [10]   | `Evaluate`      | `evaluate`                                                                                                      | numeric evaluation over a symbol→`FloatingPoint` map               |
|  [11]   | `Approximate`   | `approximate`, `approximateSubstitute`                                                                          | floating-point approximation and partial substitution              |
|  [12]   | `Compile`       | `compileExpression`, `compileExpression1`/`compileExpression2`/`compileExpression3`, `compileComplexExpression` | compile an `Expression` to a `Func`/delegate over its free symbols |

## [4]-[RESEARCH]

[RESEARCH]: `Compile` module arity and return shape
- The `Compile.compileExpression`/`compileExpression1..N` and `compileComplexExpression` arities and their exact `Func<double,...>` / `Expression<Func<...>>` return types are not fully decompile-verified in this catalogue; the symbolic owner page transcribes a compile call only after the API pass confirms the precise signature for the chosen free-symbol arity. The fluent `SymbolicExpression.Compile(params string[] variableOrder)` wrapper return shape is verified ahead of the module-level form.

[RESEARCH]: `SymbolicExpression` operator and conversion surface
- The full `SymbolicExpression` operator-overload set (`+`/`-`/`*`/`/`/`^`), implicit conversions from `int`/`double`/`Expression`, and the `.RealNumberValue`/`.Variables` accessors are verified before a fence transcribes the fluent form; the F# `Expression` DU case payloads (`Number of BigRational`, `Function of Function * Expression`) are confirmed before any case-matching fence on the C# side.

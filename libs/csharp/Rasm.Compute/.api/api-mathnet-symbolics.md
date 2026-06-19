# [RASM_COMPUTE_API_MATHNET_SYMBOLICS]

`MathNet.Symbolics` supplies the basic computer-algebra surface for the `Rasm.Compute/Symbolic/expression` owner: the `Expression` symbolic algebra, simplification over `Rational`/`Algebraic`/`Trigonometric`/`Polynomial`, symbolic differentiation and Taylor expansion via `Calculus`, delegate compilation via `Compile`, infix-string parsing and `LaTeX`/`Infix` projection, and the C#-friendly fluent `SymbolicExpression` wrapper. The package is F# (DU-based `Expression`), depends on `MathNet.Numerics.FSharp` and `FParsec` for infix parsing, and ships netstandard2.0 ALC-safe. The C# owner touches only the simplify/differentiate/compile/parse surface and wraps the F# `Expression` in `SymbolicExpr`; `FParsec` is consumed transitively through `Infix.parse` and is not called directly. The `[4]-[COMPILE_SURFACE]` section carries the decompile-verified `Compile` arity/return shape (`FSharpOption<Func<double,...>>` and the fluent unwrapped `Func`/`Delegate` overloads) and the `SymbolicExpression` operator/conversion/accessor surface the `Symbolic/expression` and `Symbolic/lowering` pages transcribe.

## [01]-[PACKAGE_SURFACE]

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

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: symbolic core and the C# fluent wrapper

| [INDEX] | [SYMBOL]             | [TYPE_FAMILY]  | [CAPABILITY]                                                                                                                                                                                   |
| :-----: | :------------------- | :------------- | :--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
|  [01]   | `Expression`         | F# DU (struct) | the symbolic-expression algebra; cases `Number`/`Approximation`/`Identifier`/`Constant`/`Sum`/`Product`/`Power`/`Function`/`Undefined`/`ComplexInfinity`/`PositiveInfinity`/`NegativeInfinity` |
|  [02]   | `SymbolicExpression` | class          | the C#-friendly fluent wrapper over `Expression`: `.Parse`, `.Differentiate`, `.Compile`, `.ToString`, operator overloads, `.Variables`                                                        |
|  [03]   | `FloatingPoint`      | F# DU          | the evaluation result carrier (`Real`/`Complex`/`RealVector`/`...`) `Evaluate.evaluate` returns                                                                                                |
|  [04]   | `VisualExpression`   | F# DU          | the presentation tree `LaTeX`/`Infix` format over                                                                                                                                              |

## [03]-[FUNCTIONAL_MODULES]

[FUNCTIONAL_MODULE_SCOPE]: the static F# modules the C# owner calls (module-function call form `Module.function arg`)
- rail: symbolic

| [INDEX] | [MODULE]        | [MEMBERS]                                                                                                       | [CAPABILITY]                                                       |
| :-----: | :-------------- | :-------------------------------------------------------------------------------------------------------------- | :----------------------------------------------------------------- |
|  [01]   | `Expression`    | `Symbol`, `FromInt32`/`FromInt64`/`FromInteger`, `Zero`, `One`, `Undefined`, `PositiveInfinity`                 | construct symbolic atoms and constants                             |
|  [02]   | `Calculus`      | `differentiate`, `tangentLine`, `taylor`                                                                        | symbolic differentiation, tangent line, Taylor series              |
|  [03]   | `Rational`      | `simplify`, `rationalize`, `reduce`, `expand`                                                                   | rational-form normalization and simplification                     |
|  [04]   | `Algebraic`     | `expand`, `separateFactors`                                                                                     | algebraic expansion and factor separation                          |
|  [05]   | `Trigonometric` | `expand`, `contract`, `simplify`                                                                                | trig identity rewriting                                            |
|  [06]   | `Polynomial`    | `coefficients`, `collectTerms`, `divide`, `quot`, `remainder`, `gcd`, `partialFraction`, `isPolynomial`         | polynomial algebra, division, gcd, partial fractions               |
|  [07]   | `Structure`     | `substitute`, `map`, `collect`                                                                                  | structural traversal and substitution                              |
|  [08]   | `Infix`         | `parse`, `tryParse`, `parseOrThrow`, `parseOrUndefined`, `format`, `formatStrict`                               | infix-string parse (over `FParsec`) and infix projection           |
|  [09]   | `LaTeX`         | `format`                                                                                                        | LaTeX projection of the expression                                 |
|  [10]   | `Evaluate`      | `evaluate`                                                                                                      | numeric evaluation over a symbol→`FloatingPoint` map               |
|  [11]   | `Approximate`   | `approximate`, `approximateSubstitute`                                                                          | floating-point approximation and partial substitution              |
|  [12]   | `Compile`       | `compileExpression`, `compileExpression1`/`compileExpression2`/`compileExpression3`, `compileComplexExpression` | compile an `Expression` to a `Func`/delegate over its free symbols |

## [04]-[COMPILE_SURFACE]

[COMPILE_SURFACE]: `Compile` module arity and return shape — F# `Option`, never `Result`, never `Expression<Func<...>>`
- `Compile.compileExpression : Expression -> Symbol list -> Delegate option` and `compileComplexExpression : Expression -> Symbol list -> Delegate option` return a boxed `LambdaExpression.Compile()` delegate inside an `FSharpOption<Delegate>`; the list-form caller down-casts.
- `Compile.compileExpression1 : Expression -> Symbol -> FSharpOption<Func<double,double>>`; `compileExpression2 : Expression -> Symbol -> Symbol -> FSharpOption<Func<double,double,double>>`; `compileExpression3 : Expression -> Symbol -> Symbol -> Symbol -> FSharpOption<Func<double,double,double,double>>` — each pre-casts the boxed `Delegate` to the arity-exact `Func`. The carrier is `FSharpOption<T>` (compilation declines, e.g. an unsupported `FunctionN` node, return `None`), so the C# owner projects `None` to a typed fault and never unwraps a null delegate.
- The fluent `SymbolicExpression` wrapper returns the unwrapped arity-exact delegate directly: `Func<double,double> Compile(string arg)`, `Func<double,double,double> Compile(string a, string b)`, `Func<double,double,double,double> Compile(string a, string b, string c)`, and the variadic `Delegate Compile(params string[] args)`; the parallel `CompileComplex` overloads return `Func<Complex,Complex>`-family delegates. The fluent form is the verified C#-facing compile surface the lowering page transcribes; the module-level `compileExpression1..3` `FSharpOption` form is the explicit-projection path when the lowering cache must distinguish a compile-decline from a parse-decline.

[COMPILE_SURFACE]: `SymbolicExpression` operator, conversion, and accessor surface
- Arithmetic operators `+`/`-`/`*`/`/` and unary `-` are C# operator overloads over `SymbolicExpression`; exponentiation is the `Pow(SymbolicExpression)` method (there is no C# `^` operator overload — `^` is the F# infix only). Implicit conversions exist from `int`, `double`, `BigInteger`, `Expression`, and `string` (the `string` conversion routes through `Parse`).
- Accessors: `double RealNumberValue { get; }` (the numeric value of a `Number`/`Approximation` leaf), `IEnumerable<SymbolicExpression> CollectVariables()` (the free-identifier set), and `string ToString()` (infix projection). Static factories: `Parse(string)`, `Variable(string)`, `Real(double)`, `Integer(BigInteger)`, `Rational(BigRational)`, and the `Zero`/`One`/`Two`/`MinusOne`/`Undefined` constants. Algebraic methods: `Expand()`, `RationalReduce()`, `RationalExpand()`, `Rationalize()`, `Differentiate(SymbolicExpression variable)`.
- The F# `Expression` DU (`[<StructuralEquality;NoComparison>]`) carries `Number of BigRational`, `Approximation of Approximation`, `Identifier of Symbol`, `Argument of Symbol`, `Constant of Constant`, `Sum of Expression list`, `Product of Expression list`, `Power of Expression * Expression`, `Function of Function * Expression`, `FunctionN of FunctionN * (Expression list)`, plus the `ComplexInfinity`/`PositiveInfinity`/`NegativeInfinity`/`Undefined` nullary cases; the structural-equality + no-comparison attribute is why a canonical-form content key is a hash over the formatted infix, not a structural `CompareTo`. `Function` carries the unary transcendental set (`Abs`/`Exp`/`Ln`/`Lg`/`Sin`/`Cos`/`Tan`/`Csc`/`Sec`/`Cot` and the hyperbolic/inverse families through `Acoth`, plus the Airy quartet) and `FunctionN` the multi-argument set (`Atan2`/`Log`/the Bessel/Hankel families).

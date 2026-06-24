# [RASM_COMPUTE_API_MATHNET_SYMBOLICS]

`MathNet.Symbolics` supplies the computer-algebra surface for the
`Rasm.Compute/Symbolic` owner: the F# `Expression` discriminated-union algebra,
normalization over `Rational`/`Algebraic`/`Trigonometric`/`Polynomial`, symbolic
differentiation / Taylor / tangent-line via `Calculus`, structural traversal /
substitution via `Structure`, numeric evaluation and floating-point approximation
via `Evaluate`/`Approximate`, native-delegate lowering via `Compile` (the
`System.Linq.Expressions` build behind it lives in the `Linq` module), infix /
LaTeX / MathML parse and projection, and the C#-friendly fluent
`SymbolicExpression` wrapper. The package is F# (the core is a
`[<StructuralEquality;NoComparison>]` DU, so a structural `CompareTo` is undefined
and a hash over a formatted projection is the only stable identity), MIT-licensed,
and ships `lib/net48`+`lib/net6.0`+`lib/net8.0`+`lib/netstandard2.0` — the
workspace `net10.0` consumer binds **`lib/net8.0`** (not `netstandard2.0`). It
depends on `MathNet.Numerics`/`MathNet.Numerics.FSharp` (the
`Expression`→`FloatingPoint` evaluation seam, `BigRational`/`Complex` carriers)
and `FParsec`/`FParsecCS` (the infix-parser combinators, consumed transitively
through `Infix.Parse`/`Infix.TryParse`, never called directly).

The decisive C#-consumption fact this catalog hardens is the **F# member-name
mangling split**. Every F# module member except the `Compile` module is exposed to
C# under a `[CompilationSourceName("<fsharp>")]`-carried **PascalCase** name
(`Calculus.Differentiate`, `Rational.Simplify`, `Polynomial.PartialFraction`,
`Structure.Substitute`, `Infix.Format`, `Infix.TryParse`, `Evaluate.Evaluate`,
`Approximate.ApproximateSubstitute`), so the C# owner binds the PascalCase symbol
and the lowercase form is the F#-source spelling only. The `Compile` module is the
sole exception: its members keep their **camelCase** F# names verbatim in C#
(`Compile.compileExpression`, `Compile.compileExpression1`…`compileExpression4`),
so the lowering page calls `compileExpressionN` as written. `[2]`/`[3]` carry the
PascalCase bind name with the F# source name parenthesized; `[4]-[COMPILE_SURFACE]`
carries the camelCase compile arity (to **4**, plus the `*OrThrow` and
`*Complex*` mirrors) and the fluent `SymbolicExpression` operator/conversion/
accessor surface the `Symbolic/expression` and `Symbolic/lowering` pages transcribe.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `MathNet.Symbolics`
- package: `MathNet.Symbolics`
- assembly: `MathNet.Symbolics`
- namespace: `MathNet.Symbolics`
- asset: runtime library (managed; F# DU core, MIT; consumer-bound TFM `lib/net8.0`)
- rail: symbolic

[PACKAGE_SURFACE]: `MathNet.Numerics.FSharp`
- package: `MathNet.Numerics.FSharp`
- assembly: `MathNet.Numerics.FSharp`
- namespace: `MathNet.Numerics`
- asset: runtime library (F# numeric facade; the `Expression`→`FloatingPoint`/`BigRational`/`Complex` carriers the evaluation seam reads)
- rail: symbolic

[PACKAGE_SURFACE]: `FParsec`
- package: `FParsec`
- assembly: `FParsec`, `FParsecCS`
- namespace: `FParsec`
- asset: runtime library (parser combinators; consumed transitively through `Infix.Parse`/`Infix.TryParse`, never called directly — a registry row, not an interior call)
- rail: symbolic

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: symbolic core and the C# fluent wrapper

| [INDEX] | [SYMBOL]                 | [TYPE_FAMILY]      | [CAPABILITY]                                                                                                                                                                                                                          |
| :-----: | :----------------------- | :----------------- | :---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
|  [01]   | `Expression`             | F# DU (`[<StructuralEquality;NoComparison>]`) | the symbolic algebra; cases `Number of BigRational` · `Approximation of Approximation` · `Identifier of Symbol` · `Argument of Symbol` · `Constant of Constant` · `Sum of Expression list` · `Product of Expression list` · `Power of Expression*Expression` · `Function of Function*Expression` · `FunctionN of FunctionN*(Expression list)` · `ComplexInfinity` · `PositiveInfinity` · `NegativeInfinity` · `Undefined` (14) |
|  [02]   | `SymbolicExpression`     | class              | the fluent C# wrapper over `Expression`: factories, parse, projection, operators, implicit conversions, the full algebraic/transcendental method surface (see `[4]`)                                                                    |
|  [03]   | `FloatingPoint`          | F# DU (10 tags)    | the evaluation-result carrier `Evaluate.Evaluate` returns — `Real`(0) · `Complex`(1) · `RealVector`(2) · `ComplexVector`(3) · `RealMatrix`(4) · `ComplexMatrix`(5) · `Undef`(6) · `PosInf`(7) · `NegInf`(8) · `ComplexInf`(9); factories `NewReal`/`NewComplex`/`NewRealVector`/… and the `Tag` discriminant |
|  [04]   | `VisualExpression`       | F# DU              | the presentation tree `Infix`/`LaTeX`/`MathML` format over and `Infix.ParseVisual` parses to                                                                                                                                           |
|  [05]   | `VisualExpressionStyle`  | sealed class       | the only format-style knob — carries `bool CompactPowersOfFunctions` and nothing else; there is **no** strict/parenthesization mode (see `[4]` canonical-form note)                                                                     |
|  [06]   | `Symbol`                 | sealed F# class    | the identifier carrier the `Compile.compileExpression*` `Symbol list`/`Symbol` arguments take (carries a `string Item`); minted by `Symbol.NewSymbol(string)` or harvested by `Structure.CollectIdentifierSymbols` — distinct from the identifier-wrapped `Expression` the algebra fold uses, which is `Expression.Symbol(name)`                                                                  |
|  [07]   | `Approximation`          | F# DU              | the `Real of double` / `Complex of Complex` leaf `Approximate.ApproximateSubstitute`'s `IDictionary<string,Approximation>` map carries and `Approximate.Real`/`Complex` mint                                                            |
|  [08]   | `Constant`               | F# DU              | the named-constant leaf (`E` · `Pi` · `I`) a `Constant` node wraps                                                                                                                                                                     |
|  [09]   | `Function` / `FunctionN` | F# DU              | the unary transcendental set (`Abs`/`Exp`/`Ln`/`Lg`/`Sin`/`Cos`/`Tan`/`Csc`/`Sec`/`Cot` + hyperbolic/inverse through `Acoth` + the Airy quartet) and the multi-argument set (`Atan2`/`Log`/the Bessel/Hankel families)                  |

## [03]-[FUNCTIONAL_MODULES]

[FUNCTIONAL_MODULE_SCOPE]: the static F# modules the C# owner calls in the form `Module.PascalMember arg` — the C# bind name is **PascalCase** for every module here (the F# source name is in parentheses); the `Compile` module is the camelCase exception documented in `[4]`.
- rail: symbolic

| [INDEX] | [MODULE]        | [C#_MEMBERS] (F# source name in parens)                                                                                                                                                                                                                                                                                              | [CAPABILITY]                                                                  |
| :-----: | :-------------- | :----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | :---------------------------------------------------------------------------- |
|  [01]   | `Operators`     | `symbol`(name) · `fromInt32`/`fromInt64`/`fromInteger`/`fromIntegerFraction`/`fromRational`/`fromDouble`/`fromSingle`/`fromComplex`/`fromDecimal` · `zero`/`one`/`two`/`minusOne`/`Pi`/`I`/`E`/`undefined`/`infinity`/`complexInfinity`/`negativeInfinity` · `add`/`multiply`/`pow`/`invert`/`sum`/`product`/`real`/`number` (camelCase-native, like `Compile`) | construct atoms/constants and the term/factor algebra the operation fold recombines through (`Operators.add`/`pow`) |
|  [02]   | `Expression`    | `Symbol`(name) · `Int32`/`Int64`/`Integer`/`IntegerFraction`/`Rational`/`Decimal`/`Real`/`Real32`/`Complex`/`Complex32` · `Zero`/`One`/`MinusOne`/`Undefined`/`PositiveInfinity`/`NegativeInfinity`/`ComplexInfinity` · the `New<Case>` DU constructors `NewNumber`/`NewSum`/`NewProduct`/`NewPower`/`NewFunction`/`NewFunctionN`/`NewIdentifier`/`NewArgument`/`NewConstant`/`NewApproximation` (PascalCase static members on the DU type) | the `string→Expression` identifier factory (`Expression.Symbol`) and the constant atoms / DU-case constructors the algebra fold builds operands from (the raw `Symbol` DU is minted by `Symbol.NewSymbol(string)`, used only for the `Compile.compileExpression*` `Symbol`/`Symbol list` arguments) |
|  [03]   | `Calculus`      | `Differentiate`(differentiate) · `DifferentiateAt`(differentiateAt) · `Taylor`(taylor: `Taylor(int k, Expression symbol, Expression value, Expression expression)`) · `TangentLine`(tangentLine) · `NormalLine`(normalLine)                                                                                                            | symbolic differentiation, Taylor series, tangent/normal line — every `symbol` argument is an `Expression`, not the `Symbol` DU |
|  [04]   | `Rational`      | `Simplify`(simplify) · `Reduce`(reduce) · `Rationalize`(rationalize) · `Expand`(expand) · `Numerator`(numerator) · `Denominator`(denominator) · `Variables`(variables) · `IsRational`(isRational) · `IsMultivariateRational`(isRationalMV)                                                                                              | rational-form normalization; `Simplify(symbol, x)` is symbol-relative          |
|  [05]   | `Algebraic`    | `Expand`(expand) · `ExpandMain`(expandMain) · `Summands`(summands) · `Factors`(factors) · `FactorsInteger`(factorsInteger) · `SeparateFactors`(separateFactors)                                                                                                                                                                       | algebraic expansion, integer/symbolic factor separation                        |
|  [06]   | `Trigonometric` | `Expand`(expand) · `Contract`(contract) · `Simplify`(simplify) · `Substitute`(substitute) · `SeparateFactors`(separateFactors)                                                                                                                                                                                                        | trig identity rewriting (whole-expression, not symbol-relative)                |
|  [07]   | `Polynomial`    | `Coefficients`(coefficients) · `Coefficient`(coefficient) · `CollectTerms`(collectTerms) · `Divide`(divide) · `Quotient`(quot) · `Remainder`(remainder) · `Gcd`(gcd) · `ExtendedGcd`(extendedGcd) · `HalfExtendedGcd`(halfExtendedGcd) · `DiophantineGcd`(diophantineGcd) · `PartialFraction`(partialFraction) · `PseudoDivide`(pseudoDivide) · `Degree`/`TotalDegree`/`MultivariateDegree` · `LeadingCoefficient` · `CommonFactors` · `FactorSquareFree` · `IsPolynomial`/`IsMonomial`/`Symbols`/`Variables`/`FromCoefficients` | polynomial algebra, Euclidean division, (extended/Diophantine) gcd, partial fractions — `collectTerms`/`partialFraction`/`gcd`/`divide` are symbol-relative |
|  [08]   | `Structure`     | `Substitute`(substitute: `Substitute(Expression y, Expression r, Expression x)`) · `Map`(map) · `Fold`(fold) · `Collect`/`CollectAll`/`CollectDistinct`(collect…) · `CollectIdentifiers`/`CollectIdentifierSymbols`/`CollectNumbers`/`CollectNumberValues`/`CollectConstants` · `IsFreeOf`(freeOf) · `NumberOfOperands`/`Operand`/`SortList` | structural traversal, substitution, predicate/chooser collection over the DU   |
|  [09]   | `Infix`         | `Parse`(parse: `FSharpResult<Expression,string>`) · `TryParse`(tryParse: `FSharpOption<Expression>`) · `ParseOrThrow`(parseOrThrow) · `ParseOrUndefined`(parseOrUndefined) · `ParseVisual`(parseVisual) · `Format`(format) · `FormatStyle`(formatStyle: `(VisualExpressionStyle, Expression)`) · `FormatVisual`/`FormatWriter`/`FormatStyleWriter`/`FormatVisualWriter` · `defaultStyle` | infix parse (over `FParsec`) returning a `Result`/`Option`/throw/`Undefined`, and infix projection — **there is no `formatStrict`** |
|  [10]   | `LaTeX`         | `Format`(format) · `FormatStyle`(formatStyle) · `FormatVisual`/`FormatWriter`/`FormatStyleWriter`                                                                                                                                                                                                                                     | LaTeX projection (default or `VisualExpressionStyle`)                          |
|  [11]   | `MathML`/`Xml`  | `MathML.FormatContentStrict`(formatContentStrict) · `MathML.FormatContentStrictXml`(→`XElement`) · `MathML.FormatSemanticsAnnotated`(formatSemanticsAnnotated) · `MathML.FormatSemanticsAnnotatedXml` · `Xml.ofString`(string→`XElement`) · `Xml.ofReader`(`TextReader`→`XElement`) · `Xml.toString`(`XElement`→string) (`Xml` members are camelCase-native) | content-MathML strict / semantics-annotated projection and the `XElement`↔string XML round-trip (there is no `MathML.Parse` — XML ingestion is `Xml.ofString`/`ofReader`) |
|  [12]   | `Evaluate`      | `Evaluate`(evaluate: `Evaluate(IDictionary<string,FloatingPoint> symbols, Expression e)`) · `Real`(freal) · `Complex`(fcomplex) · `fnormalize`/`fadd`/`fmultiply`/`fpower`/`fapply`/`fapplyN` (camelCase-native `FloatingPoint` arithmetic helpers)                                                                                       | numeric evaluation over a `symbol→FloatingPoint` map and the `FloatingPoint` arithmetic the evaluator folds with |
|  [13]   | `Approximate`   | `Approximate`(approximate) · `ApproximateSubstitute`(approximateSubstitute: `(IDictionary<string,Approximation> symbols, Expression x)`) · `Real`/`Complex`                                                                                                                                                                            | float approximation of constants and partial-substitution-plus-approximation (the map carries `Approximation`, not `double`) |
|  [14]   | `Quotations`    | `Parse`(parse: `Parse(FSharpExpr q)`)                                                                                                                                                                                                                                                                                                 | build an `Expression` from an F# code quotation                                |
|  [15]   | `VariableSets`  | `VariableSets.Alphabet.a`…`.z`                                                                                                                                                                                                                                                                                                        | the pre-built single-letter identifier atoms                                   |

## [04]-[COMPILE_SURFACE]

[COMPILE_SURFACE]: `Compile` module arity and return shape — F# `Option`, never `Result`, never `Expression<Func<…>>`; the module is the **camelCase exception** (members keep their F# lowercase names in C#, no `[CompilationSourceName]`), so the lowering page calls `Compile.compileExpressionN` as written.
- `Compile.compileExpression(Expression, FSharpList<Symbol>) : FSharpOption<Delegate>` and `compileComplexExpression(Expression, FSharpList<Symbol>) : FSharpOption<Delegate>` build a `LambdaExpression` through the `Linq` module (`Linq.FormatLambda`/`FormatComplexLambda`), call `.Compile()`, and box it inside `FSharpOption<Delegate>`; the list-form caller down-casts. The `*OrThrow` mirrors (`compileExpressionOrThrow`/`compileComplexExpressionOrThrow`) return a bare `Delegate` and throw on a `None` lambda.
- Arity-exact real overloads: `compileExpression1(Expression, Symbol) : FSharpOption<Func<double,double>>` · `compileExpression2(…2 Symbol) : FSharpOption<Func<double,double,double>>` · `compileExpression3(…3 Symbol) : FSharpOption<Func<double,double,double,double>>` · **`compileExpression4`**(…4 Symbol) : `FSharpOption<Func<double,double,double,double,double>>` — each pre-casts the boxed `Delegate` to the arity-exact `Func`, with parallel `compileExpression1OrThrow`…`compileExpression4OrThrow` returning the bare `Func`. The carrier is `FSharpOption<T>` (compilation declines — e.g. an unsupported `FunctionN` node — by returning `None`), so the C# owner projects `None` to a typed fault and never unwraps a null delegate.
- Complex mirrors: `compileComplexExpression1`…`compileComplexExpression4` (and the `*OrThrow` forms) return `FSharpOption<Func<Complex,…,Complex>>` — the parallel complex-valued lowering surface.
- The module-level `compileExpression1..4` `FSharpOption` form is the admitted lowering path **because** it lets the cache distinguish a compile-decline (`None`) from a parse-decline at the `Fin` seam; the fluent `SymbolicExpression.Compile` wrapper (below) unwraps the option and throws on decline, so it is the rejected source for the rail path. The `Symbol` arguments are minted from `Structure.CollectIdentifierSymbols` or `Symbol.NewSymbol(string)` (the sole public raw-`Symbol` factory — there is no `Symbol.Symbol`/`Symbol.symbol`), fixing the positional argument order the compiled delegate is keyed by.

[COMPILE_SURFACE]: `SymbolicExpression` fluent wrapper — the verified C#-facing operator, conversion, projection, and method surface
- Construction / factories: `SymbolicExpression(Expression)` · `Variable(string)` · `Parse(string)` · `ParseMathML(string)` · `ParseExpression(System.Linq.Expressions.Expression)` · `ParseQuotation(FSharpExpr)` · `Int32`/`Int64`/`Integer`/`IntegerFraction`/`Rational`/`Decimal`/`Real`/`Real32`/`Complex`/`Complex32`; constants `Zero`/`One`/`Two`/`MinusOne`/`I`/`E`/`Pi`/`PositiveInfinity`/`NegativeInfinity`/`ComplexInfinity`/`Undefined`.
- Accessors: `Expression Expression` (the unwrapped F# value) · `SymbolicExpressionType Type` · `double RealNumberValue` · `BigRational RationalNumberValue` · `Complex ComplexNumberValue` · `string VariableName` · `int NumberOfOperands` · `this[int index]` (operand access) · `Structure`-backed `Factors()`/`CollectVariables()`/`CollectFunctions()`/`CollectSums()`/`CollectProducts()`/`CollectPowers()`/`CollectRationalNumbers()`/`CollectRealNumbers()`/`CollectComplexNumbers()` (each `IEnumerable<SymbolicExpression>`).
- Implicit conversions from `int`/`long`/`BigInteger`/`BigRational`/`decimal`/`double`/`float`/`Complex`/`Complex32`/`Expression`/`string` (the `string` conversion routes through `Parse`).
- Operators: `+`/`-`/`*`/`/` binary and unary `+`/`-` over `SymbolicExpression`; exponentiation is the **`Pow(SymbolicExpression)` method** (there is no C# `^` overload — `^` is F# infix only). Named arithmetic mirrors `Add`/`Subtract`/`Multiply`/`Divide`/`Negate`/`Invert`; static aggregators `Sum(params/IEnumerable)`/`Product(params/IEnumerable)`.
- Algebraic / calculus methods: `Differentiate(SymbolicExpression variable)` · `DifferentiateAt(variable, value)` · `Taylor(int k, variable, value)` · `TangentLine(variable, value)` · `Expand()`/`ExpandMain()` · `RationalReduce()`/`RationalExpand()`/`Rationalize()`/`Reduce()` · `Numerator()`/`Denominator()` · `Substitute(SymbolicExpression x, SymbolicExpression replacement)` · the polynomial family `Coefficient(variable,k)`/`Coefficients(variable)`/`LeadingCoefficient(variable)`/`MonomialCoefficient(variable)`/`CollectPolynomialTerms(variable)`/`FactorSquareFree(variable)`/`PolynomialVariables()`/`PolynomialCommonFactors()` and the multivariate `IsMultivariatePolynomial`/`CollectMultivariatePolynomialTerms`/`MultivariatePolynomialDegree(params)`.
- Transcendental methods: `Abs`/`Sqrt`/`Root(n)`/`Exp`/`Ln`/`Log`/`Log(basis)`/`Sin`/`Cos`/`Tan`/`Csc`/`Sec`/`Cot` and the hyperbolic/inverse families.
- Projection: `ToString()` (infix) · `ToCustomString(compactPowersOfFunctions?)` · `ToLaTeX()`/`ToCustomLaTeX(…)` · `ToMathML()`/`ToSemanticMathML()` · `Evaluate(IDictionary<string,FloatingPoint> symbols) : FloatingPoint`.
- Compile (the fluent return is the **unwrapped** arity-exact delegate, throwing on decline): `Func<double,double> Compile(string a)` · `Compile(string a,b)` · `Compile(string a,b,c)` · `Compile(string a,b,c,d)` · variadic `Delegate Compile(params string[])`; parallel `CompileComplex(…)` returning `Func<Complex,…,Complex>`.

[COMPILE_SURFACE]: canonical-form note — there is no strict-infix mode
- The `Expression` DU is `[<StructuralEquality;NoComparison>]`, so a structural `CompareTo`/`SortedSet<Expression>` is undefined and a content-address key must hash a formatted projection. The only format knobs are `Infix.Format(Expression)` (the default style) and `Infix.FormatStyle(VisualExpressionStyle, Expression)` where `VisualExpressionStyle` carries **only** `CompactPowersOfFunctions` (bool) — there is **no** fully-parenthesized "strict" form. A canonical content key therefore hashes the normalized expression's `Infix.Format` output (after the chosen `Rational`/`Polynomial` normalization fixes the form), not a phantom `formatStrict`. `Structure.SortList` is the supported deterministic operand-ordering primitive if a structurally-canonical traversal is needed before formatting.

[COMPILE_SURFACE]: `FloatingPoint` projection seam
- `Evaluate.Evaluate` returns the 10-tag `FloatingPoint` DU; a real-scalar consumer projects `Tag` 0/7/8 (`Real`→value, `PosInf`→+∞, `NegInf`→−∞) to a success and every `Complex`(1)/`RealVector`(2)/`ComplexVector`(3)/`RealMatrix`(4)/`ComplexMatrix`(5)/`Undef`(6)/`ComplexInf`(9) arm to a non-real fault, reading the leaf through the `NewReal`-mirroring `Real` case accessor. The DU never crosses into a real-only interior — it is collapsed to a `double` rail at the seam.

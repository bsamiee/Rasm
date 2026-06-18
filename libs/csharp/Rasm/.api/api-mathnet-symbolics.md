# [RASM_COMPUTE_API_MATHNET_SYMBOLICS]

`MathNet.Symbolics` supplies a symbolic algebra engine over an immutable
discriminated-union `Expression` tree: infix and LaTeX round-trip formatting,
algebraic expansion and simplification, symbolic differentiation, polynomial and
rational arithmetic, numerical evaluation over the `FloatingPoint` carrier, and
LINQ-expression compilation to `Delegate` — all consumed through the `SymbolicExpression`
C# façade or the F#-native module family directly.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `MathNet.Symbolics`
- package: `MathNet.Symbolics`
- assembly: `MathNet.Symbolics`
- namespace: `MathNet.Symbolics`
- asset: runtime library (F# compiled; depends on `MathNet.Numerics` and `FSharp.Core`)
- rail: symbolic

## [2]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: core expression types
- rail: symbolic

| [INDEX] | [SYMBOL]                   | [TYPE_FAMILY]   | [CAPABILITY]                                    |
| :-----: | :------------------------- | :-------------- | :---------------------------------------------- |
|   [1]   | `Expression`               | F# DU class     | root expression node; tag-discriminated         |
|   [2]   | `Expression.Number`        | DU case         | exact rational number (tag 0)                   |
|   [3]   | `Expression.Approximation` | DU case         | floating-point approximation node (tag 1)       |
|   [4]   | `Expression.Identifier`    | DU case         | named symbol / variable (tag 2)                 |
|   [5]   | `Expression.Argument`      | DU case         | lambda argument symbol (tag 3)                  |
|   [6]   | `Expression.Constant`      | DU case         | built-in constant (E, Pi, I) (tag 4)            |
|   [7]   | `Expression.Sum`           | DU case         | n-ary additive node (tag 5)                     |
|   [8]   | `Expression.Product`       | DU case         | n-ary multiplicative node (tag 6)               |
|   [9]   | `Expression.Power`         | DU case         | binary power node (tag 7)                       |
|  [10]   | `Expression.Function`      | DU case         | named unary function (tag 8)                    |
|  [11]   | `Expression.FunctionN`     | DU case         | named n-ary function (tag 9)                    |
|  [12]   | `SymbolicExpression`       | C# façade class | C#-friendly wrapper exposing all DU projections |
|  [13]   | `SymbolicExpressionType`   | enum            | expression kind discriminant for the C# façade  |

[PUBLIC_TYPE_SCOPE]: value and approximation carriers
- rail: symbolic

| [INDEX] | [SYMBOL]                      | [TYPE_FAMILY] | [CAPABILITY]                                      |
| :-----: | :---------------------------- | :------------ | :------------------------------------------------ |
|   [1]   | `FloatingPoint`               | F# DU class   | numerical evaluation result; tag-discriminated    |
|   [2]   | `FloatingPoint.Real`          | DU case       | scalar `double` result (tag 0)                    |
|   [3]   | `FloatingPoint.Complex`       | DU case       | `System.Numerics.Complex` result (tag 1)          |
|   [4]   | `FloatingPoint.RealVector`    | DU case       | `Vector<double>` result (tag 2)                   |
|   [5]   | `FloatingPoint.ComplexVector` | DU case       | `Vector<Complex>` result (tag 3)                  |
|   [6]   | `FloatingPoint.RealMatrix`    | DU case       | `Matrix<double>` result (tag 4)                   |
|   [7]   | `FloatingPoint.ComplexMatrix` | DU case       | `Matrix<Complex>` result (tag 5)                  |
|   [8]   | `FloatingPoint.Undef`         | DU case       | undefined numerical result (tag 6)                |
|   [9]   | `Approximation`               | F# DU class   | internal approx node; `Real` and `Complex` cases  |
|  [10]   | `Symbol`                      | value class   | symbol name carrier for identifiers and arguments |

[PUBLIC_TYPE_SCOPE]: SymbolicExpressionType enum cases
- rail: symbolic

| [INDEX] | [SYMBOL]                                  | [VALUE] | [CAPABILITY]                 |
| :-----: | :---------------------------------------- | ------: | :--------------------------- |
|   [1]   | `SymbolicExpressionType.RationalNumber`   |       1 | exact rational number        |
|   [2]   | `SymbolicExpressionType.RealNumber`       |       2 | floating-point approximation |
|   [3]   | `SymbolicExpressionType.ComplexNumber`    |       3 | complex approximation        |
|   [4]   | `SymbolicExpressionType.Variable`         |       4 | named identifier or argument |
|   [5]   | `SymbolicExpressionType.Sum`              |       5 | additive node                |
|   [6]   | `SymbolicExpressionType.Product`          |       6 | multiplicative node          |
|   [7]   | `SymbolicExpressionType.Power`            |       7 | power node                   |
|   [8]   | `SymbolicExpressionType.Function`         |       9 | function node                |
|   [9]   | `SymbolicExpressionType.ComplexInfinity`  |      10 | complex infinity             |
|  [10]   | `SymbolicExpressionType.PositiveInfinity` |      11 | positive infinity            |
|  [11]   | `SymbolicExpressionType.NegativeInfinity` |      12 | negative infinity            |
|  [12]   | `SymbolicExpressionType.Undefined`        |      13 | undefined expression         |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: operator primitives (`Operators` module)
- rail: symbolic

| [INDEX] | [SURFACE]                                                                   | [ENTRY_FAMILY] | [CAPABILITY]                         |
| :-----: | :-------------------------------------------------------------------------- | :------------- | :----------------------------------- |
|   [1]   | `Operators.zero` / `one` / `two` / `minusOne`                               | static prop    | canonical rational constants         |
|   [2]   | `Operators.Pi` / `I` / `E`                                                  | static prop    | named mathematical constants         |
|   [3]   | `Operators.undefined` / `infinity` / `negativeInfinity` / `complexInfinity` | static prop    | sentinel values                      |
|   [4]   | `Operators.real` (FSharpFunc)                                               | static prop    | `double -> Expression` approximation |
|   [5]   | `Operators.number` (FSharpFunc)                                             | static prop    | `int -> Expression` rational literal |
|   [6]   | `Operators.add(x, y)`                                                       | static method  | symbolic addition                    |
|   [7]   | `Operators.multiply(x, y)`                                                  | static method  | symbolic multiplication              |
|   [8]   | `Operators.pow(x, y)`                                                       | static method  | symbolic exponentiation              |
|   [9]   | `Operators.invert(x)`                                                       | static method  | symbolic multiplicative inverse      |
|  [10]   | `Operators.ln(x)` / `Operators.sin(x)` etc.                                 | static method  | standard function expressions        |

[ENTRYPOINT_SCOPE]: infix parse and format (`Infix` module)
- rail: symbolic

| [INDEX] | [SURFACE]                                              | [ENTRY_FAMILY] | [CAPABILITY]                          |
| :-----: | :----------------------------------------------------- | :------------- | :------------------------------------ |
|   [1]   | `Infix.Parse(string)`                                  | static method  | `FSharpResult<Expression, string>`    |
|   [2]   | `Infix.TryParse(string)`                               | static method  | `FSharpOption<Expression>`            |
|   [3]   | `Infix.ParseOrThrow(string)`                           | static method  | `Expression`; throws on parse failure |
|   [4]   | `Infix.ParseOrUndefined(string)`                       | static method  | `Expression`; returns `Undefined`     |
|   [5]   | `Infix.Format(Expression)`                             | static method  | human-readable infix string           |
|   [6]   | `Infix.FormatStyle(VisualExpressionStyle, Expression)` | static method  | styled infix string                   |
|   [7]   | `Infix.FormatVisual(VisualExpression)`                 | static method  | formatted from visual tree            |
|   [8]   | `Infix.FormatWriter(TextWriter, Expression)`           | static method  | streaming infix output                |

[ENTRYPOINT_SCOPE]: LaTeX format (`LaTeX` module)
- rail: symbolic

| [INDEX] | [SURFACE]                                              | [ENTRY_FAMILY] | [CAPABILITY]                  |
| :-----: | :----------------------------------------------------- | :------------- | :---------------------------- |
|   [1]   | `LaTeX.Format(VisualExpressionStyle, Expression)`      | static method  | LaTeX string from expression  |
|   [2]   | `LaTeX.FormatVisual(VisualExpression)`                 | static method  | LaTeX string from visual tree |
|   [3]   | `LaTeX.FormatStyle(VisualExpressionStyle, Expression)` | static method  | LaTeX with style control      |

[ENTRYPOINT_SCOPE]: algebraic transforms (`Algebraic`, `Rational`, `Polynomial` modules)
- rail: symbolic

| [INDEX] | [SURFACE]                                 | [ENTRY_FAMILY] | [CAPABILITY]                                    |
| :-----: | :---------------------------------------- | :------------- | :---------------------------------------------- |
|   [1]   | `Algebraic.Expand(Expression)`            | static method  | full algebraic expansion of products and powers |
|   [2]   | `Algebraic.SeparateFactors(symbol, expr)` | static method  | splits factors free of symbol from the rest     |
|   [3]   | `Rational.Numerator(Expression)`          | static method  | extracts rational numerator                     |
|   [4]   | `Rational.Denominator(Expression)`        | static method  | extracts rational denominator                   |
|   [5]   | `Rational.Rationalize(Expression)`        | static method  | converts to single rational fraction            |
|   [6]   | `Polynomial.IsPolynomial(symbol, expr)`   | static method  | true when expression is polynomial in symbol    |
|   [7]   | `Polynomial.IsMonomial(symbol, expr)`     | static method  | true when expression is monomial in symbol      |
|   [8]   | `Polynomial.Degree(symbol, expr)`         | static method  | degree of polynomial in symbol                  |
|   [9]   | `Polynomial.Coefficient(symbol, k, expr)` | static method  | coefficient of `symbol^k`                       |
|  [10]   | `Polynomial.Coefficients(symbol, expr)`   | static method  | full coefficient array                          |
|  [11]   | `Polynomial.CollectTerms(symbol, expr)`   | static method  | grouped polynomial terms                        |
|  [12]   | `Polynomial.Variables(expr)`              | static method  | set of polynomial variable identifiers          |

[ENTRYPOINT_SCOPE]: structure and calculus (`Structure`, `Calculus` modules)
- rail: symbolic

| [INDEX] | [SURFACE]                                    | [ENTRY_FAMILY] | [CAPABILITY]                               |
| :-----: | :------------------------------------------- | :------------- | :----------------------------------------- |
|   [1]   | `Calculus.Differentiate(symbol, expr)`       | static method  | symbolic derivative with respect to symbol |
|   [2]   | `Structure.IsFreeOf(symbol, expr)`           | static method  | true when expr contains no occurrence      |
|   [3]   | `Structure.IsFreeOfSet(symbols, expr)`       | static method  | free-of test for a symbol set              |
|   [4]   | `Structure.Substitute(y, replacement, expr)` | static method  | replaces all occurrences of y              |
|   [5]   | `Structure.CollectIdentifiers(expr)`         | static method  | all identifier symbols                     |
|   [6]   | `Structure.CollectNumbers(expr)`             | static method  | all rational number nodes                  |

[ENTRYPOINT_SCOPE]: evaluation and compilation (`Evaluate`, `Compile` modules)
- rail: symbolic

| [INDEX] | [SURFACE]                                                          | [ENTRY_FAMILY] | [CAPABILITY]                                      |
| :-----: | :----------------------------------------------------------------- | :------------- | :------------------------------------------------ |
|   [1]   | `Evaluate.Real(double)`                                            | static method  | constructs a `FloatingPoint.Real` value           |
|   [2]   | `Evaluate.Complex(double, double)`                                 | static method  | constructs a `FloatingPoint.Complex` value        |
|   [3]   | `Evaluate.Evaluate(IDictionary<string,FloatingPoint>, Expression)` | static method  | numerical evaluation over a symbol map            |
|   [4]   | `Compile.compileExpression(expr, args)`                            | static method  | `FSharpOption<Delegate>`; LINQ compilation        |
|   [5]   | `Compile.compileComplexExpression(expr, args)`                     | static method  | `FSharpOption<Delegate>`; complex LINQ            |
|   [6]   | `Compile.compileExpressionOrThrow(expr, args)`                     | static method  | `Delegate`; throws on compilation failure         |
|   [7]   | `Compile.compileExpression1(expr, arg)`                            | static method  | `FSharpOption<Func<double,double>>`               |
|   [8]   | `Compile.compileExpression2(expr, arg1, arg2)`                     | static method  | `FSharpOption<Func<double,double,double>>`        |
|   [9]   | `Compile.compileExpression3(expr, a1, a2, a3)`                     | static method  | `FSharpOption<Func<double,double,double,double>>` |

[ENTRYPOINT_SCOPE]: SymbolicExpression C# façade
- rail: symbolic

| [INDEX] | [SURFACE]                                   | [ENTRY_FAMILY] | [CAPABILITY]                                     |
| :-----: | :------------------------------------------ | :------------- | :----------------------------------------------- |
|   [1]   | `SymbolicExpression.Zero`..`Pi` / `E` / `I` | static prop    | canonical constant façade values                 |
|   [2]   | `SymbolicExpression.PositiveInfinity` etc.  | static prop    | sentinel façade values                           |
|   [3]   | `SymbolicExpression.Type`                   | instance prop  | `SymbolicExpressionType` kind discriminant       |
|   [4]   | `SymbolicExpression.RationalNumberValue`    | instance prop  | `BigRational`; throws if not rational            |
|   [5]   | `SymbolicExpression.RealNumberValue`        | instance prop  | `double`; throws if not real                     |
|   [6]   | `SymbolicExpression.ComplexNumberValue`     | instance prop  | `System.Numerics.Complex`; throws if not complex |
|   [7]   | `SymbolicExpression.VariableName`           | instance prop  | `string`; throws if not identifier/argument      |
|   [8]   | `SymbolicExpression.Expression`             | instance prop  | underlying `Expression` DU node                  |

## [4]-[IMPLEMENTATION_LAW]

[EXPRESSION_TOPOLOGY]:
- namespace: `MathNet.Symbolics` only; single assembly
- primary type: `Expression` is an F# discriminated union compiled to a C# class hierarchy with integer `Tag` values; pattern matching in C# requires `Tag` casts or the `SymbolicExpression` façade
- sentinel tags: `ComplexInfinity` (10), `PositiveInfinity` (11), `NegativeInfinity` (12), `Undefined` (13) are static members on `Expression`
- equality: `Expression` implements `IEquatable<Expression>` and `IStructuralEquatable`; use `Equals` for structural comparison
- `FloatingPoint` tags: `Real` (0), `Complex` (1), `RealVector` (2), `ComplexVector` (3), `RealMatrix` (4), `ComplexMatrix` (5), `Undef` (6), `PosInf` (7), `NegInf` (8), `ComplexInf` (9)
- module access: F# modules `Infix`, `LaTeX`, `Algebraic`, `Calculus`, `Polynomial`, `Rational`, `Structure` carry `[RequireQualifiedAccess]` — call via qualified name from C#

[LOCAL_ADMISSION]:
- Expressions are immutable; every transform returns a new tree; no mutation occurs inside module calls.
- The C# entry point for arbitrary symbolic work is `Infix.ParseOrThrow` + module transforms + `Infix.Format`; the `SymbolicExpression` façade covers value extraction for numeric leaves only.
- `Compile.compileExpression` builds a LINQ expression tree and calls `Compile()`; the returned `Delegate` is castable to the matching typed `Func<...>` arity.
- `Evaluate.Evaluate` takes an `IDictionary<string, FloatingPoint>` keyed by variable name; variables not present in the map evaluate to `FloatingPoint.Undef`.
- `Polynomial` and `Rational` operations assume `Expression` inputs are already in canonical form; call `Algebraic.Expand` first when inputs come from arbitrary user text.

[RAIL_LAW]:
- Package: `MathNet.Symbolics`
- Owns: symbolic expression construction, infix/LaTeX formatting, algebraic expansion, symbolic differentiation, polynomial/rational arithmetic, numerical evaluation, LINQ compilation
- Accept: `Expression` DU values and `SymbolicExpression` façade values; `IDictionary<string, FloatingPoint>` for numerical evaluation
- Reject: mutable expression trees, hand-rolled symbolic differentiation, in-place expression mutation, numeric evaluation without `FloatingPoint` carrier

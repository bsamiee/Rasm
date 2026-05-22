# [H1][MATHNET_SYMBOLICS_RHINO_GH2]
>**Dictum:** *Symbolics owns expression algebra; Rhino owns geometry.*

<br>

[IMPORTANT] `MathNet.Symbolics` works as managed `net10.0` code for Rasm. It is not a RhinoCommon feature. Use it behind Rasm-owned expression types and prove plugin output/load behavior for RhinoWIP.

[IMPORTANT] `MathNet.Symbolics` `0.25.0` ships `net48`, `net6.0`, `net8.0`, and `netstandard2.0` assets. `net10.0` consumes the `net8.0` asset. The package nuspec declares `MathNet.Numerics` and `MathNet.Numerics.FSharp` `6.0.0-beta1`; Rasm centrally pins both to `6.0.0-beta2`.

[IMPORTANT] Baseline: local NuGet metadata. Keep Symbolics behind Rasm expression ownership and do not expose raw F# or MathNet objects to GH2 ports.

---
## [1][DEFINITION]
>**Dictum:** *Symbolics is computer algebra for expressions.*

<br>

| [INDEX] | [SURFACE] | [CAPABILITY] |
| :-----: | --------- | ------------ |
| [1] | `SymbolicExpression` | Primary C# facade for construction, arithmetic, parse, format, substitution, differentiation, evaluation, and compile. |
| [2] | `Expression` | Lower-level F# tree with identifiers, numbers, constants, functions, sums, products, powers. |
| [3] | `Infix` | Parse and format expression text through `Parse`, `TryParse`, `ParseOrThrow`, `ParseOrUndefined`, and `Format`. |
| [4] | `LaTeX`, `MathML` | LaTeX output, MathML parse, strict content XML, and semantic annotation formats. |
| [5] | `Algebraic`, `Rational`, `Polynomial` | Expansion, reduction, factors, coefficients, division, GCD, square-free factorization, partial fractions. |
| [6] | `Calculus` | Differentiate, differentiate-at, Taylor, tangent line, normal line. |
| [7] | `Trigonometric`, `Exponential` | Expand, contract, simplify, substitute named transform families. |
| [8] | `Evaluate`, `Compile`, `FloatingPoint` | Evaluate symbol dictionaries, compile real/complex delegates, carry scalar/vector/matrix floating values. |
| [9] | `Structure`, `Operators`, `VariableSets.Alphabet` | Inspect trees, collect variables/functions/constants, substitute, fold/map, build common symbols. |

---
## [2][CS_API]
>**Dictum:** *C# code starts from `SymbolicExpression`.*

<br>

| [INDEX] | [CATEGORY] | [API] | [RULE] |
| :-----: | ---------- | ----- | ------ |
| [1] | Constants | `Zero`, `One`, `Two`, `MinusOne`, `I`, `E`, `Pi`, infinities, `Undefined`. | Wrap non-finite cases before crossing Rasm boundaries. |
| [2] | Constructors | `Int32`, `Int64`, `Integer`, `IntegerFraction`, `Rational`, `Decimal`, `Real`, `Real32`, `Complex`, `Complex32`, `Variable`. | Validate names and finite values before construction. |
| [3] | Operators | C# `+`, `-`, `*`, `/`, plus `Add`, `Subtract`, `Multiply`, `Divide`, `Pow`, `Invert`, `Abs`, `Root`, `Sqrt`, `Sum`, `Product`. | Prefer named methods in public-facing implementation for traceability. |
| [4] | Functions | `Exp`, `Ln`, `Log`, trig, hyperbolic, inverse trig, Airy, Bessel, Hankel. | Keep operation vocabulary bounded by Rasm concern. |
| [5] | Parse | `SymbolicExpression.Parse(string)`, implicit `string` conversion, `ParseMathML`, `ParseExpression`, lower-level `Infix.Parse`, `TryParse`, `ParseOrUndefined`, `ParseVisual`. | Use `Fin<T>` rails because parse APIs can fail or throw. |
| [6] | Format | `ToString`, `ToCustomString`, `ToLaTeX`, `ToCustomLaTeX`, `ToMathML`, `ToSemanticMathML`. | Treat formatted output as display/interchange, not identity. |
| [7] | Structure | `CollectVariables`, `CollectFunctions`, `CollectSums`, `CollectProducts`, `CollectPowers`, `Summands`, `Factors`, `Operand`, `NumberOfOperands`. | Materialize into Rasm collections immediately. |
| [8] | Algebra | `Expand`, `ExpandMain`, `Rationalize`, `RationalReduce`, `RationalExpand`, `RationalSimplify`, polynomial, trigonometric, and exponential transform methods. | Name every transform explicitly; do not apply broad simplify by default. |
| [9] | Polynomial | `PolynomialVariables`, predicates, degree APIs, coefficients, leading coefficient, divide, quotient, remainder, pseudo-divide, GCD, square-free factorization, collect terms. | Use for exact formula classification before numeric execution. |
| [10] | Calculus | `Differentiate`, `DifferentiateAt`, `Taylor`, `TangentLine`, `NormalLine`. | Use analytic derivatives before sampling gradients or frames. |
| [11] | Evaluation | `Evaluate`, `Compile`, `CompileComplex`, `Approximate`. | Validate symbol coverage, arity, output shape, and Rhino scalar validity. |
| [12] | Floating values | `FloatingPoint.Real`, `Complex`, `RealVector`, `ComplexVector`, `RealMatrix`, `ComplexMatrix`, `PosInf`, `NegInf`, `ComplexInf`, `Undef`. | Convert immediately into Rasm or Rhino values; reject invalid geometry payloads. |

---
## [3][RHINOWIP_RUNTIME]
>**Dictum:** *Runtime proof means the managed closure loads beside the plugin.*

<br>

| [INDEX] | [CONCERN] | [RULE] |
| :-----: | --------- | ------ |
| [1] | Target runtime | Repo targets RhinoWIP with `net10.0`; keep MathNet assets selected by `net10.0`. |
| [2] | Dependency closure | Output must include `MathNet.Symbolics.dll`, `MathNet.Numerics.dll`, `MathNet.Numerics.FSharp.dll`, `FSharp.Core.dll`, `FParsec.dll`, and `FParsecCS.dll`. |
| [3] | Native assets | Symbolics and current Numerics stack are managed; native provider packages remain separate. |
| [4] | Load proof | Static build is necessary; RhinoWIP load-smoke is required before declaring runtime support complete. |
| [5] | Trimming | Do not trim Symbolics or F# dependencies without a dedicated reflection and load test. |

---
## [4][RASM_OWNERSHIP]
>**Dictum:** *Rasm exposes expression intent, not MathNet implementation objects.*

<br>

| [INDEX] | [RASM_CONCEPT] | [SYMBOLICS_BACKING] | [RULE] |
| :-----: | ------------------- | ------------------- | ------ |
| [1] | Formula value | `SymbolicExpression` | Store normalized expression identity plus cached parse. |
| [2] | `FormulaText` | string value object | Preserve canonical persistence and display-independent identity. |
| [3] | `SymbolName` set | `Structure.CollectVariables` / `CollectIdentifiers` | Validate variables against operation-specific vocabularies. |
| [4] | `ExpressionFormat` | Infix, LaTeX, MathML, semantic MathML. | Treat format as explicit output policy, not identity. |
| [5] | `SymbolicTransform` | Algebraic, rational, polynomial, trig, exponential, calculus modules. | Model as bounded transform vocabulary with generated dispatch. |
| [6] | `ExpressionFault` | Parse, compile, evaluate, arity, unknown-symbol, non-finite cases. | Collapse Symbolics and Rhino failures into typed rails. |
| [7] | Evaluation rail | `Evaluate` / `Compile` | Validate every numeric output with Rhino/Rasm scalar validity before geometry use. |
| [8] | Lower-level API | `Expression`, `Structure`, `Operators`, F# option/result/list shapes. | Use only where F# shapes are intentionally contained inside Rasm expression internals. |

---
## [5][RHINO_GH2_USE]
>**Dictum:** *Expressions become geometry only through explicit evaluation.*

<br>

| [INDEX] | [USE_CASE] | [BOUNDARY] |
| :-----: | ---------- | ---------- |
| [1] | Parametric scalar fields | Parse expression, bind allowed symbols, evaluate over Rhino sample points. |
| [2] | Analytic derivatives | Differentiate symbolically, then evaluate derivatives for vectors or frames. |
| [3] | User formulas in GH2 | Accept text at the boundary, parse to `Formula`, and output infix, LaTeX, MathML, or evaluated values by explicit port type. |
| [4] | Curve or surface formulas | Rhino remains owner of generated geometry and validity. |
| [5] | Documentation/display | Use LaTeX or MathML output for presentation, not identity or persistence. |

---
## [6][FORMULA_WORKFLOWS]
>**Dictum:** *Formula workflows stack symbolic intent, numeric execution, and Rhino output.*

<br>

| [INDEX] | [FLOW] | [PIPELINE] |
| :-----: | ------ | ---------- |
| [1] | GH2 parse | Text input to `Infix.Parse`/`TryParse`, `Fin<Formula>`, `CollectVariables`, allowed-symbol `Validation`, explicit transform mode, evaluate or compile, GH-native output. |
| [2] | Display | Choose one representation: infix via `ToString`/`ToCustomString`, LaTeX via `ToLaTeX`/`ToCustomLaTeX`, strict MathML via `ToMathML`, semantic MathML via `ToSemanticMathML`. |
| [3] | Rhino field | Parse `f(x,y,z)`, validate `{x,y,z}`, derive gradients with `Differentiate`, compile `Compile("x","y","z")`, sample Rhino points, reject non-finite `FloatingPoint`, emit Rhino geometry. |
| [4] | Parametric geometry | Use one formula per coordinate or scalar channel; variables such as `u`, `v`, `t`, and named parameters are `SymbolName` values, not loose string arrays. |
| [5] | Numeric stacking | Route compiled delegates through MathNet integration, root finding, optimization, interpolation, matrices/vectors for Jacobians and Hessians, and statistics over residuals. |
| [6] | Cached compile | Treat compiled delegates as cached execution artifacts tied to symbol order; persist canonical expression text plus transform/evaluation policy. |

# [MATHNET_SYMBOLICS]

Runtime safety follows the active assembly load context, not copy-local presence.

## [1][PARSE]

| [INDEX] | [SURFACE]                  | [POLICY]                                                    |
| :-----: | -------------------------- | ----------------------------------------------------------- |
|   [1]   | `SymbolicExpression.Parse` | Throwing parse; boundary adapter only.                      |
|   [2]   | Implicit string conversion | Do not use in domain code.                                  |
|   [3]   | `Infix.Parse`              | F# result-shaped parse; project into `Fin<Formula>`.        |
|   [4]   | `Infix.TryParse`           | F# option-shaped parse; project absence into typed failure. |
|   [5]   | `ParseOrUndefined`         | Sentinel result; reject `Undefined` explicitly.             |

## [2][EVALUATE_COMPILE]

`Evaluate` returns `FloatingPoint`, not a guaranteed `double`. Classify real, complex, vector, matrix, infinity, and undefined cases before geometry projection. `Compile` overloads use ordered symbols; cache compiled delegates by normalized expression plus ordered symbol list.

## [3][TRANSFORMS]

Document exact transform families such as algebraic, rational, polynomial, trigonometric, exponential, differentiation, substitution, and collection. Keep `SymbolicExpression` as the C# facade; use lower-level F# module shapes only for verified internal inspection.

## [4][FAILURES]

Map failures such as parse failure, undefined sentinel, unknown symbol, arity mismatch, compile failure, unsupported `FloatingPoint` case, and non-finite domain value into LanguageExt rails. Host diagnostics include operation name, input label, allowed symbols, ordered compile symbols, and failed stage.

## [5][FORMAT]

Infix, LaTeX, and strict content MathML formatting may be output contracts when verified. Do not claim broad presentation-MathML import or arbitrary semantic annotation parsing without a fixture.

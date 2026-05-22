# [H1][MATHNET_SYMBOLICS]
>**Dictum:** *Symbolics owns formulas before numerical sampling begins.*

<br>

[IMPORTANT] Runtime safety requires assembly identity proof. RhinoWIP/GH2 may carry older Symbolics assemblies, so copy-local presence is not enough.

---
## [1][SOURCE_TRUTH]
>**Dictum:** *Symbolics claims need package and load-context evidence.*

<br>

| [INDEX] | [SOURCE] | [USE] |
| :-----: | -------- | ----- |
| [1] | `MathNet.Symbolics` `0.25.0` local XML/nuspec/DLL | API and package truth. |
| [2] | Symbolics package source commit | Parse/format/evaluate implementation proof. |
| [3] | `.deps.json`, assembly identity tools, bridge load-smoke/check | Runtime load proof. |
| [4] | RhinoWIP/GH2 XML | Host projection and diagnostics. |

---
## [2][PARSE]
>**Dictum:** *Formula admission chooses the failure rail before expression use.*

<br>

| [INDEX] | [SURFACE] | [POLICY] |
| :-----: | --------- | -------- |
| [1] | `SymbolicExpression.Parse` | Throwing parse; boundary adapter only. |
| [2] | Implicit string conversion | Do not use in domain code. |
| [3] | `Infix.Parse` | F# result-shaped parse; project into `Fin<Formula>`. |
| [4] | `Infix.TryParse` | F# option-shaped parse; project absence into typed failure. |
| [5] | `ParseOrUndefined` | Sentinel result; reject `Undefined` explicitly. |

---
## [3][EVALUATE_COMPILE]
>**Dictum:** *Symbol order and result shape are part of cache identity.*

<br>

`Evaluate` returns `FloatingPoint`, not a guaranteed `double`. Classify real, complex, vector, matrix, infinity, and undefined cases before geometry projection. `Compile` overloads use ordered symbols; cache compiled delegates by normalized expression plus ordered symbol list.

---
## [4][TRANSFORMS]
>**Dictum:** *No simplifier is universally safe.*

<br>

Document exact transform families such as algebraic, rational, polynomial, trigonometric, exponential, differentiation, substitution, and collection. Keep `SymbolicExpression` as the C# facade; use lower-level F# module shapes only for verified internal inspection.

---
## [5][FAILURES]
>**Dictum:** *Symbolic failures remain user-visible diagnostics.*

<br>

Map failures such as parse failure, undefined sentinel, unknown symbol, arity mismatch, compile failure, unsupported `FloatingPoint` case, and non-finite geometry value into LanguageExt rails. GH2 diagnostics include operation name, input nickname, allowed symbols, ordered compile symbols, and failed stage.

---
## [6][FORMAT]
>**Dictum:** *Display formats do not imply import coverage.*

<br>

Infix, LaTeX, and strict content MathML formatting may be output contracts when verified. Do not claim broad presentation-MathML import or arbitrary semantic annotation parsing without a fixture.

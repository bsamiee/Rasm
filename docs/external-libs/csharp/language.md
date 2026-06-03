# [H1][CSHARP_LANGUAGE]
>**Dictum:** *C# 14 is the expression substrate; the compiler enforces shape before libraries add rails.*

<br>

[IMPORTANT] Pin C# 14.0 on net10.0 (`LangVersion`, `TargetFramework`, `Nullable=enable`, `ImplicitUsings=enable`).
Compiler toolset: .NET 10 SDK compiler (global compile).
Analyzer authoring: `Microsoft.CodeAnalysis.CSharp` and `Microsoft.CodeAnalysis.Analyzers` at the versions pinned in `Directory.Packages.props`.
Do not use `LangVersion=preview` / `latest`.

Route BCL and host-reference policy through `docs/system-api-map`. Owns language features at the pinned version.

---
## [1][SOURCE_TRUTH]
>**Dictum:** *Build props outrank blog posts and memory.*

<br>

| [INDEX] | [SOURCE]                   | [OWNS]                                                               |
| :-----: | -------------------------- | -------------------------------------------------------------------- |
|   [1]   | `Directory.Build.props`    | `net10.0`, `LangVersion=14.0`, nullable, implicit usings, analyzers. |
|   [2]   | Central package manifest   | Analyzer and Roslyn analyzer-authoring packages                      |
|   [3]   | .NET 10 SDK / Roslyn       | Compiler feature set for C# 14.                                      |

---
## [2][C14_NEW]
>**Dictum:** *C# 14 reduces ceremony at boundaries and makes spans first-class.*

<br>

| [INDEX] | [FEATURE]                         | [FUNCTIONAL_USE]                                                        |
| :-----: | --------------------------------- | ----------------------------------------------------------------------- |
|   [1]   | Extension blocks                  | Receiver extensions—properties, methods, operators, static—no wrappers. |
|   [2]   | `field` keyword                   | Inline accessor validation/normalization without a backing field.       |
|   [3]   | Implicit span conversions         | Arrays, `string`, spans in overloads, inference, conditionals.          |
|   [4]   | Null-conditional assignment       | Optional mutation at host/boundary without guard blocks.                |
|   [5]   | Simple lambda parameter modifiers | Modifiers on untyped lambda parameters.                                 |
|   [6]   | `nameof` on unbound generics      | Stable generic identity in errors and telemetry.                        |
|   [7]   | Partial constructors and events   | Generator/hand-written splits on one type.                              |
|   [8]   | User-defined compound assignment  | Domain `+=`, `-=`, … without unpack/repack.                             |

[NOTES]
- [1] Instance block: `extension<T>(IEnumerable<T> source) { … }`. Static block: `extension<T>(IEnumerable<T>) { … }`.
- [2] Disambiguate with `@field` or `this.field` when a symbol named `field` exists.
- [3] Prefer `ReadOnlySpan` when both span overloads apply.
- [4] `target?.Prop = value;`, `target?.[i] += delta;`. `++`/`--` not allowed on null-conditional LHS.
- [5] `ref`, `in`, `out`, `ref readonly`, `scoped`. `params` requires explicit lambda parameter types.
- [6] `nameof(List<>)` → `"List"`.
- [7] Primary constructor syntax allowed on only one partial declaration.
- [8] Custom `+=`/`-=` on domain types without imperative unpack/repack.

---
## [3][C13_BASELINE]
>**Dictum:** *LangVersion 14.0 inherits C# 13; treat these as active baseline.*

<br>

| [INDEX] | [FEATURE]                               | [FUNCTIONAL_USE]                                                   |
| :-----: | --------------------------------------- | ------------------------------------------------------------------ |
|   [1]   | `params` collections                    | Arity-polymorphic `params` for spans and collection targets.       |
|   [2]   | Partial properties and indexers         | Declaring/implementing split for source generators.                |
|   [3]   | `ref struct` implements interfaces      | Stack-only explicit interface impl; no boxing to interface.        |
|   [4]   | `allows ref struct` generic constraint  | Generic algorithms over stack-only types.                          |
|   [5]   | `ref` / `unsafe` in async and iterators | Stack types only outside `await` / `yield return`.                 |
|   [6]   | Overload resolution priority            | Mark canonical overload when span/`params` expansion ambiguates.   |
|   [7]   | `lock` object type                      | Prefer `System.Threading.Lock` over `lock(obj)` at new boundaries. |

[NOTES]
- [1] Last parameter only. Overload with `ReadOnlySpan` for hot paths.
- [2] Implementing declaration cannot be auto-property body-only.
- [3] Default interface members on external interfaces create recompile/binary break risk.
- [4] `where T : allows ref struct`.
- [5] `ref struct` cannot share a block with `await` or `yield return`.
- [6] `[OverloadResolutionPriority(n)]` on preferred overload.
- [7] .NET 9+ BCL type.

---
## [4][C12_BASELINE]
>**Dictum:** *Collection syntax and primary constructors anchor immutable data carriers.*

<br>

| [INDEX] | [FEATURE]                 | [FUNCTIONAL_USE]                                           |
| :-----: | ------------------------- | ---------------------------------------------------------- |
|   [1]   | Primary constructors      | Params in instance scope; pair with `record` for equality. |
|   [2]   | Collection expressions    | Literal/composition for arrays, spans, lists, targets.     |
|   [3]   | `ref readonly` parameters | Read-only by-ref for large structs and span APIs.          |
|   [4]   | Default lambda parameters | Partial application at expression boundaries.              |
|   [5]   | `using` alias any type    | Semantic names for tuple, pointer, nested generic shapes.  |

[NOTES]
- [1] Non-record types do not auto-generate public properties from primary parameters.
- [2] Spread: `[..left, ..right, item]`.
- [3] Clearer intent than `in` when a variable (not rvalue) is required.
- [4] Same rules as method default parameters.
- [5] Reduces noise in dense generic signatures.

---
## [5][EXPRESSION_BASELINE]
>**Dictum:** *Functional modules at LangVersion 14.0 still depend on C# 8–11 expression features.*

<br>

| [INDEX] | [FEATURE]                         | [FUNCTIONAL_USE]                                               |
| :-----: | --------------------------------- | -------------------------------------------------------------- |
|   [1]   | Switch expressions                | Total value dispatch; prefer over statement `switch`.          |
|   [2]   | Property and logical patterns     | `{ Prop: > 0 and < 100 }`; `and` / `or` / `not`.               |
|   [3]   | List patterns                     | `[]`, `[x]`, `[head, ..tail]` for empty/singleton/tail shapes. |
|   [4]   | `required` members                | Construction invariants without constructor spam.              |
|   [5]   | `init` accessors                  | Immutable surface after construction.                          |
|   [6]   | Static abstract interface members | Static trait dispatch (`INumber<T>`, generic math).            |
|   [7]   | File-scoped namespace/type        | Less nesting in dense modules.                                 |
|   [8]   | Raw string literals               | Multi-line DSL/JSON without escape noise.                      |
|   [9]   | Nullable reference types          | `Nullable=enable`; lift host nulls at boundary only.           |

---
## [6][FUNCTIONAL_RULES]
>**Dictum:** *Language features express shape; libraries express rails.*

<br>

[RULES]
- [1] Prefer switch expressions and patterns over nested statement branching for value-returning transforms.
- [2] Prefer collection expressions and `params` collections over manual array/list construction and overload families.
- [3] Use `ref readonly` and `ReadOnlySpan<T>` at hot read-only boundaries; rely on C# 14 implicit span conversions only where overload intent is unambiguous.
- [4] Use `field` and `required` for inline invariants; keep validation expressions in accessors and factories.
- [5] Use extension blocks for cross-cutting instance/static operators on existing types; do not introduce wrapper types that only forward members.
- [6] Use partial properties/constructors/events only for source-generator or hand/generated splits—not for arbitrary file fragmentation.
- [7] Treat null-conditional assignment as boundary ergonomics; domain logic should remain on typed option/result rails, not nullable mutation chains.
- [8] Audit overload sets where `T[]`, `IEnumerable<T>`, and `ReadOnlySpan<T>` coexist—implicit span conversions can change overload winners.

---
## [7][OUT_OF_SCOPE]
>**Dictum:** *Preview and app-model features are not production baseline.*

<br>

| [INDEX] | [FEATURE]                              | [STATUS]                                       |
| :-----: | -------------------------------------- | ---------------------------------------------- |
|   [1]   | Interceptors                           | Experimental; not production baseline.         |
|   [2]   | File-based app preprocessor directives | C# 14 app model; not general library baseline. |
|   [3]   | `LangVersion=preview` / `latest`       | Avoid; pin explicit `14.0`.                    |

# [H1][CSHARP_LANGUAGE]
>**Dictum:** *C# 14 is the expression substrate; the compiler enforces shape before libraries add rails.*

<br>

[IMPORTANT] Pin **C# 14.0** on **net10.0** (`LangVersion`, `TargetFramework`, `Nullable=enable`, `ImplicitUsings=enable`).
Compiler toolset: **Microsoft.Net.Compilers.Toolset 5.3.0** (global compile).
Analyzer authoring: **Microsoft.CodeAnalysis.CSharp 5.3.0**, **Microsoft.CodeAnalysis.Analyzers 5.3.0**.
Do not use `LangVersion=preview` / `latest`.

Route BCL and host-reference policy through `docs/system-api-map`. This file owns **language features** at the pinned version.

---
## [1][SOURCE_TRUTH]
>**Dictum:** *Build props outrank blog posts and memory.*

<br>

| [INDEX] | [SOURCE] | [OWNS] |
| :-----: | -------- | ------ |
| [1] | `Directory.Build.props` | `TargetFramework=net10.0`, `LangVersion=14.0`, nullable, implicit usings, analyzer posture. |
| [2] | Central package manifest | `Microsoft.Net.Compilers.Toolset` `5.3.0`, `Microsoft.CodeAnalysis.CSharp` `5.3.0`, `Microsoft.CodeAnalysis.Analyzers` `5.3.0`. |
| [3] | .NET 10 SDK / Roslyn 5.3.0 | Compiler feature set for C# 14. |
| [4] | [What's new in C# 14](https://learn.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-14) | Official feature list and semantics. |

---
## [2][C14_NEW]
>**Dictum:** *C# 14 reduces ceremony at boundaries and makes spans first-class.*

<br>

| [INDEX] | [FEATURE] | [FUNCTIONAL_USE] | [NOTES] |
| :-----: | --------- | ------------------ | ------- |
| [1] | **Extension blocks** (`extension` keyword) | Group extension properties, methods, operators, and static members on a receiver type without wrapper types. | Instance block: `extension<T>(IEnumerable<T> source) { … }`. Static block: `extension<T>(IEnumerable<T>) { … }`. |
| [2] | **`field` keyword** | Inline validation/normalization in property accessors without declaring a backing field. | Disambiguate with `@field` or `this.field` when a symbol named `field` exists. |
| [3] | **Implicit span conversions** | Zero-ceremony interop between arrays, `string`, `Span<T>`, and `ReadOnlySpan<T>` in overload resolution, inference, and conditional expressions. | Prefer `ReadOnlySpan` when both span overloads apply. |
| [4] | **Null-conditional assignment** | Optional mutation at host/boundary sites without guard blocks. | `target?.Prop = value;`, `target?.[i] += delta;`. **`++`/`--` not allowed** on null-conditional LHS. |
| [5] | **Simple lambda parameter modifiers** | Parameter modifiers on untyped lambda parameters. | `ref`, `in`, `out`, `ref readonly`, `scoped`. **`params` requires explicit lambda parameter types.** |
| [6] | **`nameof` on unbound generics** | Stable generic identity strings in error rails and telemetry. | `nameof(List<>)` → `"List"`. |
| [7] | **Partial constructors and events** | Source-generator composition for types with generated and hand-written halves. | Primary constructor syntax allowed on only one partial declaration. |
| [8] | **User-defined compound assignment** | Custom semigroup/monoid update operators (`+=`, `-=`, …) on domain types. | Enables domain types to participate in compound updates without imperative unpack/repack. |

---
## [3][C13_BASELINE]
>**Dictum:** *LangVersion 14.0 inherits C# 13; treat these as active baseline.*

<br>

| [INDEX] | [FEATURE] | [FUNCTIONAL_USE] | [NOTES] |
| :-----: | --------- | ------------------ | ------- |
| [1] | **`params` collections** | Arity-polymorphic APIs: `params ReadOnlySpan<T>`, `params Span<T>`, `params IEnumerable<T>`, concrete collection types supporting collection expressions. | Last parameter only. Overload with `ReadOnlySpan` for hot paths. |
| [2] | **Partial properties and indexers** | Source-generator split between declaring and implementing halves. | Implementing declaration cannot be auto-property body-only. |
| [3] | **`ref struct` implements interfaces** | Stack-only abstractions with explicit interface implementation; no boxing conversion to interface type. | Default interface members on external interfaces create recompile/binary break risk. |
| [4] | **`allows ref struct` generic constraint** | Generic algorithms over stack-only types without heap escape. | `where T : allows ref struct`. |
| [5] | **`ref` / `unsafe` in async and iterators** | Limited stack-type use outside `await`/`yield return` blocks. | `ref struct` cannot share a block with `await` or `yield return`. |
| [6] | **Overload resolution priority** | Mark canonical overloads when implicit span/`params` expansions create ambiguity. | `[OverloadResolutionPriority(n)]` on preferred overload. |
| [7] | **`lock` object type** | Prefer `System.Threading.Lock` over `lock(obj)` for new synchronization at boundaries. | .NET 9+ BCL type. |

---
## [4][C12_BASELINE]
>**Dictum:** *Collection syntax and primary constructors anchor immutable data carriers.*

<br>

| [INDEX] | [FEATURE] | [FUNCTIONAL_USE] | [NOTES] |
| :-----: | --------- | ------------------ | ------- |
| [1] | **Primary constructors** (`class` / `struct`) | Capture constructor parameters in instance scope; pair with `record` for generated equality members. | Non-record types do not auto-generate public properties from primary parameters. |
| [2] | **Collection expressions** `[…]` | Uniform literal and composition syntax for arrays, spans, lists, and other collection-target types. | Spread: `[..left, ..right, item]`. |
| [3] | **`ref readonly` parameters** | Read-only by-reference passing for large structs and span-like APIs without copy or accidental mutation. | Clearer intent than `in` when a variable (not rvalue) is required. |
| [4] | **Default lambda parameters** | Partial application shape at expression boundaries. | Same rules as method default parameters. |
| [5] | **`using` alias any type** | Semantic names for tuple, pointer, and nested generic shapes. | Reduces noise in dense generic signatures. |

---
## [5][EXPRESSION_BASELINE]
>**Dictum:** *Functional modules at LangVersion 14.0 still depend on C# 8–11 expression features.*

<br>

| [INDEX] | [FEATURE] | [FUNCTIONAL_USE] |
| :-----: | --------- | ------------------ |
| [1] | Switch expressions | Total dispatch returning values; prefer over statement `switch` for transforms. |
| [2] | Property, relational, and logical patterns | `{ Prop: > 0 and < 100 }`, `and` / `or` / `not` combinators. |
| [3] | List patterns | `[]`, `[x]`, `[head, ..tail]` for empty/singleton/recursive shapes. |
| [4] | `required` members | Construction invariants on records and classes without constructor spam. |
| [5] | `init` accessors | Immutable surface after construction. |
| [6] | Static abstract interface members | Trait-like static dispatch (`INumber<T>`, generic math). |
| [7] | File-scoped namespace/type | Reduced nesting in dense modules. |
| [8] | Raw string literals | Multi-line embedded DSL and JSON without escape noise. |
| [9] | Nullable reference types | Project-wide (`Nullable=enable`); lift host nulls at boundary, not in domain transforms. |

---
## [6][FUNCTIONAL_RULES]
>**Dictum:** *Language features express shape; libraries express rails.*

<br>

| [INDEX] | [RULE] |
| :-----: | ------ |
| [1] | Prefer **switch expressions** and **patterns** over nested statement branching for value-returning transforms. |
| [2] | Prefer **collection expressions** and **`params` collections** over manual array/list construction and overload families. |
| [3] | Use **`ref readonly`** and **`ReadOnlySpan<T>`** at hot read-only boundaries; rely on C# 14 implicit span conversions only where overload intent is unambiguous. |
| [4] | Use **`field`** and **`required`** for inline invariants; keep validation expressions in accessors and factories. |
| [5] | Use **extension blocks** for cross-cutting instance/static operators on existing types; do not introduce wrapper types that only forward members. |
| [6] | Use **partial properties/constructors/events** only for source-generator or hand/generated splits—not for arbitrary file fragmentation. |
| [7] | Treat **null-conditional assignment** as boundary ergonomics; domain logic should remain on typed option/result rails, not nullable mutation chains. |
| [8] | Audit overload sets where **`T[]`**, **`IEnumerable<T>`**, and **`ReadOnlySpan<T>`** coexist—implicit span conversions can change overload winners. |

---
## [7][OFFICIAL_REFERENCES]
>**Dictum:** *Speclets settle ambiguity.*

<br>

| [INDEX] | [TOPIC] | [URL] |
| :-----: | ------- | ----- |
| [1] | C# 14 overview | https://learn.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-14 |
| [2] | Extension members | https://learn.microsoft.com/en-us/dotnet/csharp/programming-guide/classes-and-structs/extension-methods |
| [3] | First-class span types (spec) | https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/proposals/csharp-14.0/first-class-span-types |
| [4] | Collection expressions | https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/operators/collection-expressions |
| [5] | `params` collections (spec) | https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/proposals/csharp-13.0/params-collections |
| [6] | Parameter modifiers (`ref readonly`) | https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/keywords/method-parameters |
| [7] | C# 14 breaking changes (.NET 10) | https://learn.microsoft.com/en-us/dotnet/csharp/whats-new/breaking-changes/compiler%20breaking%20changes%20-%20dotnet%2010 |
| [8] | Extension members (`extension` keyword) | https://learn.microsoft.com/en-us/dotnet/csharp/programming-guide/classes-and-structs/extension-methods |

---
## [8][OUT_OF_SCOPE]
>**Dictum:** *Preview and app-model features are not production baseline.*

<br>

| [INDEX] | [FEATURE] | [STATUS] |
| :-----: | --------- | -------- |
| [1] | Interceptors | Experimental; not production baseline. |
| [2] | File-based app preprocessor directives | C# 14 app model; not general library baseline. |
| [3] | `LangVersion=preview` / `latest` | Avoid; pin explicit `14.0`. |

# [H1][REPLACEMENTS]
>**Dictum:** *Refactoring starts by choosing the canonical owner.*

<br>

[IMPORTANT] Replacement means moving behavior to the owning API, not adding wrappers.

---
## [1][TEXT_AND_LOOKUP]
>**Dictum:** *Grammar, character policy, and lookup lifetime are separate.*

<br>

| [INDEX] | [LOCAL_PATTERN] | [OWNER] |
| :-----: | --------------- | ------- |
| [1] | Hand-built static regex. | `[GeneratedRegex]` with timeout and culture. |
| [2] | Regex for character allow/deny checks. | `SearchValues<char>`. |
| [3] | Rebuilt static dictionaries. | `FrozenDictionary` or generated smart-enum `Items`. |

---
## [2][VALIDATION_AND_DISPATCH]
>**Dictum:** *Closed vocabularies carry lookup and behavior.*

<br>

| [INDEX] | [LOCAL_PATTERN] | [OWNER] |
| :-----: | --------------- | ------- |
| [1] | Primitive validation repeated across files. | Thinktecture value object plus LanguageExt rail. |
| [2] | Parallel dictionaries beside enum-like values. | Thinktecture smart enum with item-owned behavior. |
| [3] | Repeated switch or visitor arms. | Thinktecture union `Switch`/`Map`. |
| [4] | Nested fallible unwrapping. | LanguageExt `Flatten`, `Bind`, or LINQ composition. |

---
## [3][NUMERICS]
>**Dictum:** *Numerical policy exposes diagnostics and delegates execution.*

<br>

| [INDEX] | [LOCAL_PATTERN] | [OWNER] |
| :-----: | --------------- | ------- |
| [1] | Manual matrix solve or decomposition. | MathNet factorization/solver API. |
| [2] | Geometry algorithm already native to Rhino. | RhinoCommon. |
| [3] | Flat hot numeric reduction. | BCL span/tensor primitive after package gate and measurement. |
| [4] | Symbolic formula parsing/evaluation. | MathNet.Symbolics plus LanguageExt admission rail. |

---
## [4][HOST_BOUNDARIES]
>**Dictum:** *Native host structures remain native until projected.*

<br>

| [INDEX] | [LOCAL_PATTERN] | [OWNER] |
| :-----: | --------------- | ------- |
| [1] | Hand-built GH2 tree paths. | GH2 `Garden`, `Tree`, `Coverage`, `WithPathPrefix`. |
| [2] | Rhino tolerance constants copied locally. | `RhinoMath` and document context. |
| [3] | Runtime package assumptions. | `Directory.Build.props` and host load evidence. |

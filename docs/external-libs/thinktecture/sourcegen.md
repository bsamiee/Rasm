# [H1][THINKTECTURE_SOURCEGEN]
>**Dictum:** *Generated code is part of the compile contract.*

<br>

[IMPORTANT] Rasm consumes the core runtime package. Optional framework integration packages are not active unless pinned and consumed.

---
## [1][PACKAGE_CLOSURE]
>**Dictum:** *Generator packages arrive through the runtime package closure.*

<br>

| [INDEX] | [PACKAGE] | [STATE] |
| :-----: | --------- | ------- |
| [1] | `Thinktecture.Runtime.Extensions` | Active central package. |
| [2] | `.Analyzers` | Transitive analyzer package from runtime package. |
| [3] | `.Refactorings` | Transitive refactoring package from runtime package. |
| [4] | `.SourceGenerator` | Transitive source generator package from runtime package. |

---
## [2][CONFIG]
>**Dictum:** *Generator diagnostics stay source-backed.*

<br>

Keep source-generator configuration names only when verified from official docs, local XML, or package source. Do not preserve unverified MSBuild property names. Treat generated diagnostics as architectural pressure, not warning noise.

---
## [3][BOUNDARY_PACKAGES]
>**Dictum:** *Integration packages enter only with a concrete boundary.*

<br>

JSON, Newtonsoft, MessagePack, ASP.NET, EF, OpenAPI, and similar integration packages are not active Rasm guidance unless they appear in central package truth and a project consumes them. Document them as not in graph or omit them.

---
## [4][RULES]
>**Dictum:** *Generated declarations are code, not comments.*

<br>

- Verify generated member names before documenting them.
- Keep sourcegen examples tiny and compilable.
- Do not claim span, serialization, or model-binding behavior without a compiled fixture or source proof.
- Keep analyzer/sourcegen detail in this file; keep domain usage in `rasm.md`.

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

---
## [5][V10_GENERATED_NAMES]
>**Dictum:** *Bind to the generated case name, not a hand-authored alias.*

<br>

In v10 the generator simplified nested union case type names. Hand-authored aliases that duplicated the generated case name are no longer needed and silently shadow the generated symbol. Cite the generated form directly:

```csharp
[Union]
public abstract partial record FileTarget {
    public sealed record Loose(string Path) : FileTarget;
    public sealed record Bundled(string Path, string Slug) : FileTarget;
}

// Bind to FileTarget.Loose / FileTarget.Bundled — NOT a FileLoose / FileBundled alias.
return target switch {
    FileTarget.Loose(var p) => ..., FileTarget.Bundled(var p, var s) => ...,
};
```

Generic `[Union]` on `record<TState>` emits `Switch<TState, TResult>` overloads with `where TState : allows ref struct` (C# 13+). The constraint propagates and conflicts with `sealed record` consumers that use `TState` without the same allowance — keep generic transition unions as `abstract record` + virtual `switch (this)` instead of `[Union]`. Non-generic `[Union]` cases are unaffected.

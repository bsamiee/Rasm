# [THINKTECTURE_SOURCEGEN]

Core runtime package adoption belongs in the package graph. Optional framework integration packages are not active unless package graph state and a consumer make them active.

## [1][PACKAGE_CLOSURE]

| [INDEX] | [PACKAGE]                         | [STATE]                                                   |
| :-----: | --------------------------------- | --------------------------------------------------------- |
|   [1]   | `Thinktecture.Runtime.Extensions` | Active central package.                                   |
|   [2]   | `.Analyzers`                      | Transitive analyzer package from runtime package.         |
|   [3]   | `.Refactorings`                   | Transitive refactoring package from runtime package.      |
|   [4]   | `.SourceGenerator`                | Transitive source generator package from runtime package. |

## [2][CONFIG]

Keep source-generator configuration names only when official docs, local XML, or package source expose them. Do not preserve unsupported MSBuild property names. Treat generated diagnostics as architectural pressure, not warning noise.

## [3][BOUNDARY_PACKAGES]

JSON, Newtonsoft, MessagePack, ASP.NET, EF, OpenAPI, and similar integration packages are not active guidance unless package graph state and a project consumer make them active. Document them as not in graph or omit them.

## [4][RULES]

- Use generated member names from sourcegen output.
- Keep sourcegen examples tiny and compilable.
- Do not claim span, serialization, or model-binding behavior without a compiled fixture or source proof.
- Keep analyzer/sourcegen detail in this file; keep domain usage in `rasm.md`.

## [5][GENERATED_CASE_NAMES]

Use the generated nested union case names directly. Hand-authored aliases that duplicate the generated case name shadow the generated symbol:

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

Union dispatch attributes (`SwitchMapStateParameterName`) — `union-attributes.md`. Union SelfOp emission (`[SkipUnionOps]`, `[GenerateUnionOps]`) — `rasm.md` §4.

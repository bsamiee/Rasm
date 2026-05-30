# [H1][ARCHUNIT_API]
>**Dictum:** *Architecture tests guard dependency direction, not style.*

<br>

[IMPORTANT] Rasm uses `TngTech.ArchUnitNET.xUnitV3 0.13.3` in `tests/csharp/_architecture` for compiled assembly boundary laws that the local analyzer does not own.

---
## [1][PACKAGE]
>**Dictum:** *Use the xUnit v3 ArchUnit package.*

<br>

| [INDEX] | [PACKAGE]                     | [PIN]    | [USE]                             |
| :-----: | ----------------------------- | -------- | --------------------------------- |
|   [1]   | `TngTech.ArchUnitNET.xUnitV3` | `0.13.3` | xUnit v3 architecture assertions. |

[SOURCE] NuGet package page: https://www.nuget.org/packages/TngTech.ArchUnitNET.xUnitV3/0.13.3

---
## [2][SURFACE]
>**Dictum:** *Load assemblies once; assert dependency laws.*

<br>

| [INDEX] | [API]                                              | [RASM_USE]                                                    |
| :-----: | -------------------------------------------------- | ------------------------------------------------------------- |
|   [1]   | `ArchLoader().LoadAssemblies(...).Build()`         | Build a compiled architecture graph from concrete assemblies. |
|   [2]   | `Types()`, `Classes()`, `Interfaces()`             | Select assembly, namespace, or type categories.               |
|   [3]   | `Should().NotDependOnAny(...)`                     | Boundary direction laws.                                      |
|   [4]   | `Slices().Matching(...).Should().BeFreeOfCycles()` | Cycle checks where namespace slices are stable.               |
|   [5]   | `Because(...)`                                     | Encode design reason in failing assertion.                    |

---
## [3][RASM_SCOPE]
>**Dictum:** *Do not duplicate analyzer law.*

<br>

Use ArchUnitNET for assembly/package direction, dependency cycles, and high-level boundary ownership. Keep expression style, helper sprawl, branching, and LanguageExt/Thinktecture rules in `tools/cs-analyzer`.

Run architecture laws in Debug so dependency metadata is not optimized away:

```bash
dotnet test tests/csharp/_architecture/Rasm.Architecture.Tests.csproj --configuration Debug
```

---
## [4][CROSS_LAYER_INTERFACE_COVERAGE]
>**Dictum:** *Closed switches over open type sets are structural debt — assert reachability.*

<br>

When a closed-world dispatch (e.g., `OpAcceptance.ValidityOf` in `Domain/Validation.cs`) must recognize an open set of types from downstream layers (e.g., `[BoundaryAdapter]`-marked records from `Vectors`, `Mesh`, `Field`), the canonical extension hook is a marker interface (`IDomainValid`). The architecture test enumerates all implementers and asserts each is reachable through the dispatch:

```csharp
[Fact]
public void EveryBoundaryAdapterImplementsIDomainValidOrIsRegisteredInValidityOf() {
    var arch = new ArchLoader()
        .LoadAssemblies(typeof(Rasm.Domain.Op).Assembly, typeof(Rasm.Vectors.Matrix).Assembly /*, ...*/)
        .Build();
    var adapters = arch.Types().That().HaveAttribute<BoundaryAdapterAttribute>();
    foreach (var adapter in adapters) {
        Assert.True(
            adapter.ImplementsInterface(typeof(IDomainValid))
            || OpAcceptance.IsRegisteredInValidityOf(adapter.ReflectionType),
            $"{adapter.FullName} is [BoundaryAdapter] but unreachable from OpAcceptance.ValidityOf");
    }
}
```

Catches the entire `AcceptValue / ValidityOf` gap regression class once and forever. See `feedback_acceptvalue_validity_gap` memory for the bug-class background.

---
## [5][CYCLE_DETECTION_SCOPE]
>**Dictum:** *Cycles are architectural, not symbolic.*

<br>

Rasm uses namespace-slice cycle detection only for stable namespaces (Domain, Vectors, Mesh, Field) — not for in-flight refactor namespaces or test/_testkit. Slice patterns:

| [INDEX] | [PATTERN]            | [SCOPE]                                                |
| :-----: | -------------------- | ------------------------------------------------------ |
|   [1]   | `Rasm.Domain.(*)..`  | Domain internal slices must not cycle.                 |
|   [2]   | `Rasm.Vectors.(*)..` | Vectors module slices must not cycle.                  |
|   [3]   | `Rasm.(*)..`         | Top-level module slices must not cycle across modules. |

Avoid `Rasm..` global cycle detection — it would block valid abstraction layers (Domain → Vectors → Mesh) and flag every legitimate boundary crossing.

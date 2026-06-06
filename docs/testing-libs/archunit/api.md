# [ARCHUNIT_API]

[IMPORTANT] Use `TngTech.ArchUnitNET.xUnitV3` in the architecture test project for compiled assembly boundary laws that the local analyzer does not own.

## [1][SURFACE]

| [INDEX] | [API]                                              | [PROJECT_USE]                                                 |
| :-----: | -------------------------------------------------- | ------------------------------------------------------------- |
|   [1]   | `ArchLoader().LoadAssemblies(...).Build()`         | Build a compiled architecture graph from concrete assemblies. |
|   [2]   | `Types()`, `Classes()`, `Interfaces()`             | Select assembly, namespace, or type categories.               |
|   [3]   | `Should().NotDependOnAny(...)`                     | Boundary direction laws.                                      |
|   [4]   | `Slices().Matching(...).Should().BeFreeOfCycles()` | Cycle checks where namespace slices are stable.               |
|   [5]   | `Because(...)`                                     | Encode design reason in failing assertion.                    |

## [2][PROJECT_SCOPE]

Use ArchUnitNET for assembly/package direction, dependency cycles, and high-level boundary ownership. Keep expression style, helper sprawl, branching, and LanguageExt/Thinktecture rules in `tools/cs-analyzer`.

Run architecture laws in Debug so dependency metadata is not optimized away:

```bash
<test-runner> --target <architecture-test-project>
```

## [3][CROSS_LAYER_INTERFACE_COVERAGE]

When a closed-world dispatch must recognize an open set of types from downstream layers, the canonical extension hook is a marker interface or admission protocol. The architecture test enumerates all implementers and asserts each is reachable through the dispatch:

```csharp
[Fact]
public void EveryBoundaryAdapterImplementsAdmissionHookOrIsRegisteredInDispatch() {
    var arch = new ArchLoader()
        .LoadAssemblies(typeof(DomainRoot).Assembly, typeof(DownstreamRoot).Assembly /*, ...*/)
        .Build();
    var adapters = arch.Types().That().HaveAttribute<BoundaryAdapterAttribute>();
    foreach (var adapter in adapters) {
        Assert.True(
            adapter.ImplementsInterface(typeof(IDomainValid))
            || AdmissionRegistry.IsRegistered(adapter.ReflectionType),
            $"{adapter.FullName} is [BoundaryAdapter] but unreachable from the admission registry");
    }
}
```

Catches the downstream-adapter admission gap regression class once and forever.

## [4][CYCLE_DETECTION_SCOPE]

Use namespace-slice cycle detection only for stable namespaces, not for in-flight refactor namespaces or test/testkit surfaces. Slice patterns:

| [INDEX] | [PATTERN]            | [SCOPE]                                                |
| :-----: | -------------------- | ------------------------------------------------------ |
|   [1]   | `<Root>.Domain.(*)..` | Domain internal slices must not cycle.                 |
|   [2]   | `<Root>.Module.(*)..` | Module slices must not cycle.                          |
|   [3]   | `<Root>.(*)..`        | Top-level module slices must not cycle across modules. |

Avoid global cycle detection when it would block valid abstraction layers and flag legitimate boundary crossings.

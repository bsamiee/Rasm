# [archunitnet] — assembly-model architecture rules under the boundary laws

`TngTech.ArchUnitNET.xUnitV3` loads assemblies into a Mono.Cecil-backed architecture model and evaluates a fluent predicate/condition grammar over types, members, and namespace slices. The estate's `_architecture` suite composes the core surface — `ArchLoader`, `ArchRuleDefinition`, `SliceRuleDefinition` — inside `Assert.True(rule.HasNoViolations(architecture))`, always behind the kit vacuity gate because an empty filtered set passes vacuously by construction.

## [01]-[PACKAGE_SURFACE]

- package: `TngTech.ArchUnitNET.xUnitV3` `0.13.3` (carries core `TngTech.ArchUnitNET` `0.13.3`)
- license: `Apache-2.0`
- namespace: `ArchUnitNET.Loader`, `ArchUnitNET.Domain`, `ArchUnitNET.Fluent`, `ArchUnitNET.Fluent.Slices`, `ArchUnitNET.xUnitV3`
- asset: `lib/netstandard2.0/ArchUnitNET.dll` + `ArchUnitNET.xUnitV3.dll`; Mono.Cecil and CycleDetection ride transitively
- rail: evidence — reference topology, dependency direction, and cycle laws over loaded assemblies

## [02]-[PUBLIC_TYPES]

| [INDEX] | [SYMBOL]                                         | [KIND]        | [CAPABILITY]                                                       |
| :-----: | :----------------------------------------------- | :------------ | :----------------------------------------------------------------- |
|  [01]   | `ArchLoader`                                     | builder       | `LoadAssemblies`, dependency-walk + disk-glob modes, `Build()`     |
|  [02]   | `Architecture`                                   | domain model  | `Types`/`Classes`/`Interfaces`/`Namespaces`/`Assemblies`/`Members` |
|  [03]   | `ArchRuleDefinition`                             | fluent root   | `Types()`/`Classes()`/`Members()` -> `.That()` -> `.Should()`      |
|  [04]   | `SliceRuleDefinition`                            | fluent root   | `BeFreeOfCycles()`/`NotDependOnEachOther()` slice conditions       |
|  [05]   | `IArchRule`                                      | rule          | `HasNoViolations(Architecture)`, `Evaluate`, cross-rule `And`/`Or` |
|  [06]   | `ArchRuleExtensions` / `FailedArchRuleException` | xunit adapter | throwing `Check(rule, architecture)` rail over core evaluation     |

## [03]-[ENTRYPOINTS]

Predicates filter by name, namespace, attribute, and visibility across every fluent root; slice capture keys and cycle law are `[SLICES]`.

| [INDEX] | [SURFACE]                                                            | [KIND]      | [CAPABILITY]                           |
| :-----: | :------------------------------------------------------------------- | :---------- | :------------------------------------- |
|  [01]   | `new ArchLoader().LoadAssemblies(params Assembly[]).Build()`         | loader      | parsed from disk location; no dep walk |
|  [02]   | `Types(bool includeReferenced = false).That().ResideInAssembly(...)` | predicate   | assembly-scoped type selection         |
|  [03]   | `.Should().NotDependOnAny(...)`                                      | condition   | forbid any listed dependency           |
|  [04]   | `.Should().OnlyDependOn(...)`                                        | condition   | restrict to the listed set             |
|  [05]   | `.Should().DependOnAnyTypesThat(...)`                                | condition   | require a matching dependency          |
|  [06]   | `Slices().Matching("Rasm.(*)").Should().BeFreeOfCycles()`            | slice rule  | namespace-family cycle law             |
|  [07]   | `rule.HasNoViolations(architecture)`                                 | evaluation  | boolean fold over condition results    |
|  [08]   | `Members()`                                                          | fluent root | member-shape law grammar               |
|  [09]   | `FieldMembers()`                                                     | fluent root | field-shape law grammar                |
|  [10]   | `MethodMembers()`                                                    | fluent root | method-shape law grammar               |
|  [11]   | `PropertyMembers()`                                                  | fluent root | property-shape law grammar             |

```csharp signature
public class ArchLoader {
    public ArchLoader LoadAssemblies(params Assembly[] assemblies);
    public Architecture Build();
}
public static class ArchRuleDefinition {
    public static GivenTypes Types(bool includeReferenced = false);
    public static GivenClasses Classes(bool includeReferenced = false);
}
public static class SliceRuleDefinition {
    public static SliceRuleInitializer Slices();
}
```

## [04]-[IMPLEMENTATION_LAW]

[VACUITY]: negative conditions return `CheckEmpty() == true`, so an empty predicate-filtered set evaluates to zero condition results and `HasNoViolations` folds to a vacuous pass — a rule whose assemblies failed to load passes silently. Every estate rule therefore runs behind the `NonVacuous` gate that first proves the architecture actually loaded types for each asserted assembly; the gate precedes every `Assert.True(rule.HasNoViolations(...))`.

[SLICES]: a slice pattern carries exactly one `(*)` (single segment) or `(**)` (multi segment) capture; `"Rasm.(*)"` keys one slice per `Rasm.<Segment>` namespace and `BeFreeOfCycles` fails on any inter-slice dependency cycle via the CycleDetection graph.

[UNUSED_CAPABILITY]: the member-level grammar, `OnlyDependOn`/`DependOnAnyTypesThat` nested forms, PlantUML conformance (`AdhereToPlantUmlDiagram`), prebuilt `TargetFrameworkRules`, `FollowCustomPredicate`/`FollowCustomCondition` escape hatches, dependency-walking loader modes, and the xUnitV3 throwing `Check` rail are present and admitted; a new architecture law composes these before any hand-rolled reflection walk.

[STACKING]:
- `xunit.v3` (`xunit-v3.md`): the adapter's `FailedArchRuleException` derives from `XunitException`; the estate sinks rule verdicts through core `Assert.True`, sharing the one floated `xunit.v3.assert`.
- `Rasm.TestKit` `Manifests`: manifest facts (csproj topology, slnx roster, CPM discipline) stay XML-parsed manifest laws; ArchUnitNET owns only loaded-type dependency laws — the two lanes never substitute for each other.

[LOCAL_ADMISSION]:
- Architecture rules live in `tests/csharp/_architecture` over the host-free closure; host-closed assemblies stay manifest facts, never loaded types.
- Every rule pairs with the vacuity gate; a bare `HasNoViolations` assertion is the named defect.

[RAIL_LAW]:
- Package: `TngTech.ArchUnitNET.xUnitV3`
- Owns: loaded-assembly dependency direction, containment, naming, and cycle laws.
- Accept: fluent rules over the host-free closure; slice cycle laws per namespace family; member-level grammar when a member-shape law lands.
- Reject: hand-rolled reflection dependency walks, rules without the vacuity gate, or loading host-bound assemblies into the model.

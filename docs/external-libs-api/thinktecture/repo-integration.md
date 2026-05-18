# [H1][THINKTECTURE_REPO_INTEGRATION]
>**Dictum:** *Rasm uses Thinktecture to define shape, then LanguageExt to compose rails.*

<br>

[IMPORTANT] Thinktecture generated APIs map to current repo implementation. Use these anchors before adding local wrappers, helper registries, or visitor-style dispatch.

---
## [1][PACKAGE_WIRING]
>**Dictum:** *The package is workspace-wide by central version and global reference.*

<br>

| [INDEX] | [ANCHOR] | [ROLE] |
| :-----: | -------- | ---- |
| **[1]** | `Directory.Packages.props` | Pins `Thinktecture.Runtime.Extensions` `10.2.0`. |
| **[2]** | `Directory.Build.props` | Adds the package reference for workspace libraries. |
| **[3]** | `Rasm.csproj` and `Rasm.Grasshopper.csproj` | Import the `Thinktecture` namespace. |
| **[4]** | `.cache/nuget/packages/thinktecture.runtime.extensions/10.2.0` | Local package and generated API truth. |

---
## [2][VALUE_BOUNDARIES]
>**Dictum:** *Generated value objects guard native tolerance and operation identity.*

<br>

| [INDEX] | [SYMBOL] | [GENERATED_SURFACE] | [ROLE] |
| :-----: | -------- | ------------------- | ---- |
| **[1]** | `AbsoluteTolerance` | `[ValueObject<double>]` plus factory validation | Positive finite Rhino model tolerance. |
| **[2]** | `RelativeTolerance` | `[ValueObject<double>]` plus factory validation | Fractional tolerance accepted by analysis context. |
| **[3]** | `AngleTolerance` | `[ValueObject<double>]` plus factory validation | Radian tolerance accepted by analysis context. |
| **[4]** | `Op` | `[ValueObject<string>]` plus key comparers | Operation name, error labeling, and validation identity. |
| **[5]** | `Context.Create` | Generated factories lifted into `Validation<Error,Context>` | Public context admission. |
| **[6]** | `OpAcceptance.TryCreateValidated<TVO>` | `IObjectFactory<TVO,double,ValidationError>` | Generic generated-factory bridge to LanguageExt validation. |

[IMPORTANT] Keep value-object validation in the generated factory hook, then map `ValidationError` to `Fault` at the domain boundary.

---
## [3][CLOSED_REGISTRIES]
>**Dictum:** *Smart enums centralize native type and behavior lookup.*

<br>

| [INDEX] | [SYMBOL] | [GENERATED_SURFACE] | [ROLE] |
| :-----: | -------- | ------------------- | ---- |
| **[1]** | `Kind` | `[SmartEnum<int>]` and `Items` | Maps Rhino/GH types to topology categories. |
| **[2]** | `MassKind` | `[SmartEnum<int>]` plus delegates | Carries requirements, compute behavior, and aggregate behavior. |
| **[3]** | `StatKind` | `[SmartEnum<int>]` | Selects statistic behavior. |
| **[4]** | `MeshDefect` | `[SmartEnum<int>]` | Names mesh check defect categories. |
| **[5]** | `MeshMetric` | `[SmartEnum<int>]` | Names mesh metric projections. |
| **[6]** | `PortKind` | `[SmartEnum<string>]`, `Items`, `[UseDelegateFromConstructor]` | Maps GH2 parameter binders, output binders, value types, and wire types. |

[CRITICAL] Add new closed vocabulary as smart-enum items when the vocabulary owns behavior or lookup. Use native enums only for simple protocol constants that do not need generated registry behavior.

[IMPORTANT] Promote validation catalogs to smart enums when checks have keys, applicability predicates, and run delegates. This pattern condenses static check declarations and derives grouped requirements from `Items`.

---
## [4][ASPECT_DISPATCH]
>**Dictum:** *Analysis selection is generated union dispatch into operation rails.*

<br>

| [INDEX] | [SYMBOL] | [GENERATED_SURFACE] | [ROLE] |
| :-----: | -------- | ------------------- | ---- |
| **[1]** | `Measure` | `[Union]` and `Switch<Operation<...>>` | Selects measurement operation families. |
| **[2]** | `Faces` | `[Union]` and state-threaded `Switch` | Selects face subsets and ranking behavior. |
| **[3]** | `IntersectionResult` | `[Union]` and state-threaded `Switch` | Projects native intersection cases into requested outputs. |
| **[4]** | `Boundaries`, `Bounds`, `Curves`, `Location`, `Meshes`, `Points`, `Spatial` | `[Union]` | Model analysis aspects with exhaustive operation selection. |
| **[5]** | `Fault` | `[Union]` | Expected domain failure vocabulary over `LanguageExt.Common.Error`. |
| **[6]** | GH adapter errors | `[BoundaryAdapter]` records | Host-facing expected failures without decorative closed dispatch. |

[IMPORTANT] Use generated union `Switch` for operation selection and output projection. Use LanguageExt `Fin<T>`, `Validation<Error,T>`, and `Eff<Env,T>` for failure, accumulation, and execution after dispatch chooses the operation.

[CRITICAL] Promote projection-only variant records to `[Union]` when multiple derived properties switch over the same cases. Generated `Map` keeps projections exhaustive and removes sentinel fallbacks.

---
## [5][RHINO_AND_GH2_BOUNDARIES]
>**Dictum:** *Thinktecture chooses shape; adapters own native protocol work.*

<br>

| [INDEX] | [BOUNDARY] | [THINKTECTURE_ROLE] | [LANGUAGEEXT_ROLE] |
| :-----: | ---------- | ------------------- | ----------------- |
| **[1]** | Rhino tolerance context | Value objects validate numeric admission. | `Validation<Error,Context>` accumulates boundary errors. |
| **[2]** | Rhino geometry type inference | `Kind.Items` seeds type lookup. | `Option<Kind>` models absence without failure. |
| **[3]** | Analysis operation selection | Aspect unions and smart enums select operation behavior. | `Eff<Env,Seq<T>>` defers native execution. |
| **[4]** | GH2 port binding | `PortKind` stores generated binder delegates. | `Fin<Seq<Flow<T>>>` carries read failures. |
| **[5]** | GH output handling | Boundary records keep host failures categorized. | Adapter `Match` collapses rails into host messages. |

---
## [6][ANALYZER_ALIGNMENT]
>**Dictum:** *Repo analyzers recognize generated shapes as intentional abstractions.*

<br>

| [INDEX] | [ANALYZER_AREA] | [INTEGRATION] |
| :-----: | --------------- | ----------- |
| **[1]** | Primitive public signatures | Prefer `[ValueObject<T>]`, `[SmartEnum<T>]`, or `[Union]` over raw primitive leakage. |
| **[2]** | Type-shape factories | Thinktecture value objects and smart enums satisfy source-generator factory expectations. |
| **[3]** | Polymorphic discriminators | Type-parameterized unions count as generated discriminator shapes. |
| **[4]** | Generated method companions | `Get`, `TryGet`, and dispatch members are treated as generated API. |

[CRITICAL] Refactor toward generated shapes when doing so reduces raw primitive signatures, branch maps, or hand-written closed-vocabulary code. Do not add one-use helper methods around generated members.

---
## [7][NEW_CODE_RULES]
>**Dictum:** *Use generated shape at construction and FP rails at composition.*

<br>

- Add primitive invariants as value objects with factory validation.
- Add closed type registries as keyed smart enums with metadata and item behavior.
- Add operation selectors and expected failure families as regular unions.
- Use generated `Items`, `Get`, `TryGet`, `Switch`, and `Map` before creating local maps or visitors.
- Bridge generated `ValidationError` into `Fault` once, then continue through `Fin<T>`, `Validation<Error,T>`, or `Eff<Env,T>`.
- Keep RhinoCommon and GH2 protocol details in existing boundary modules and local RhinoWIP XML evidence.

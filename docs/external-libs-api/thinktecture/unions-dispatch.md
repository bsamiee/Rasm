# [H1][THINKTECTURE_UNIONS_DISPATCH]
>**Dictum:** *Union dispatch is the closed-world alternative to visitor code.*

<br>

[IMPORTANT] Use unions when a type has a known set of variants and each consumer must acknowledge every variant. Generated dispatch is the API surface; local visitors and helper switches duplicate it.

---
## [1][REGULAR_UNIONS]
>**Dictum:** *Regular unions name domain cases explicitly.*

<br>

| [INDEX] | [SURFACE] | [USE] |
| :-----: | --------- | --- |
| **[1]** | `[Union]` | Marks an abstract or base partial record/class as a generated union. |
| **[2]** | Nested sealed records/classes | Declare cases with domain-specific payloads. |
| **[3]** | Private or protected base constructor | Prevents uncontrolled external inheritance. |
| **[4]** | `SwitchMethods` and `MapMethods` | Controls generated exhaustive dispatch. |
| **[5]** | `NestedUnionParameterNames` | Controls names for nested union parameters in generated methods. |

[CRITICAL] Prefer regular unions for domain concepts with named cases: aspects, faults, geometric outcomes, and selectors.

---
## [2][AD_HOC_UNIONS]
>**Dictum:** *Ad-hoc unions are compact alternatives for value-level choice.*

<br>

| [INDEX] | [SURFACE] | [USE] |
| :-----: | --------- | --- |
| **[1]** | `[Union<T1,T2>]` through `[Union<T1,T2,T3,T4,T5>]` | Declares typed cases without nested case records. |
| **[2]** | `[AdHocUnion]` | Declares cases through type arguments for non-generic attribute scenarios. |
| **[3]** | `TnName` | Gives a case a stable generated method and property name. |
| **[4]** | `TnIsNullableReferenceType` | Marks nullable reference cases deliberately. |
| **[5]** | `TnIsStateless` | Marks cases that carry no value. |
| **[6]** | `UseSingleBackingField` | Stores ad-hoc values in one backing field where appropriate. |
| **[7]** | `TypeParamRef1` through `TypeParamRef5` | References generic ad-hoc union type parameters from attributes. |

[IMPORTANT] Prefer ad-hoc unions when the alternatives are simple values and the nested-record ceremony would obscure the algorithm.

[CRITICAL] Use struct stateless cases for no-payload alternatives. Stateless reference cases imply nullable defaults and add null-handling complexity.

---
## [3][DISPATCH]
>**Dictum:** *Named generated handlers are the stable call-site contract.*

<br>

| [INDEX] | [GENERATED_MEMBER] | [ROLE] |
| :-----: | ------------------ | ---- |
| **[1]** | `Switch` | Exhaustive effect or command dispatch. |
| **[2]** | `Map` | Exhaustive pure projection. |
| **[3]** | State overloads | Threads immutable context into static lambdas. |
| **[4]** | `SwitchPartially` and `MapPartially` | Handles a configured subset and forwards the rest. |
| **[5]** | `UnionSwitchMapOverloadAttribute` | Limits nested generated overloads with `StopAt`. |

[CRITICAL] Use generated state overloads for dense static lambdas. This avoids closure capture while keeping the case logic in one expression.

---
## [4][CONSTRUCTION_AND_CONVERSION]
>**Dictum:** *Generated case conversion never hides domain admission.*

<br>

| [INDEX] | [SETTING] | [CAPABILITY] |
| :-----: | --------- | ------------ |
| **[1]** | `FactoryMethodGeneration` | Generates or suppresses factory methods for union members. |
| **[2]** | `ConversionFromValue` | Converts case values into the union. |
| **[3]** | `ConversionToValue` | Converts the union to the current case value where supported. |
| **[4]** | `UnionConstructorAccessModifier` | Controls generated union constructor accessibility. |
| **[5]** | `DefaultStringComparison` | Controls string matching inside generated union support. |

[IMPORTANT] Enable conversion only when the conversion is unambiguous and reduces code at real call sites. Keep explicit case construction when failure semantics or ownership semantics matter.

---
## [5][PROJECTION_ONLY_UNIONS]
>**Dictum:** *Derived projections stay exhaustive through generated dispatch.*

<br>

| [INDEX] | [PATTERN] | [USE] |
| :-----: | --------- | --- |
| **[1]** | Regular union with derived properties | Replace repeated manual case switches for `Kind`, `Points`, `Curves`, or equivalent projections. |
| **[2]** | Generated `Map` for pure projection | Keep projections exhaustive when new cases appear. |
| **[3]** | Generated `Switch` for rail projection | Return `Fin<T>` or `Validation<Error,T>` from every case and compose directly. |
| **[4]** | No fallback arm | Let generator force new case handling rather than returning unknown sentinels. |

---
## [6][SERIALIZATION_BOUNDARIES]
>**Dictum:** *A union boundary must choose one wire model.*

<br>

| [INDEX] | [UNION_KIND] | [BOUNDARY_POSTURE] |
| :-----: | ------------ | ---------------- |
| **[1]** | Regular union | Prefer normal polymorphic serialization or a deliberate object factory for a single-value wire shape. |
| **[2]** | Ad-hoc union | Prefer object-factory conversion for JSON, EF, or model binding. |
| **[3]** | Fault union | Keep internal domain dispatch and expose host-specific messages at the adapter boundary. |
| **[4]** | Aspect union | Keep serialized shape out of the domain unless the host protocol requires it. |

---
## [7][RASM_ANCHORS]
>**Dictum:** *Analysis aspects are generated dispatch values.*

<br>

| [INDEX] | [SYMBOL] | [ROLE] |
| :-----: | -------- | ---- |
| **[1]** | `Measure` | Dispatches analysis measure cases into `Operation<TGeometry,TOut>`. |
| **[2]** | `Faces` | Dispatches face selectors with state-threaded `Switch`. |
| **[3]** | `IntersectionResult` | Projects native intersection outcomes into typed output rails. |
| **[4]** | `Fault` | Encodes expected domain failures as exhaustive error cases. |
| **[5]** | GH adapter errors | Use boundary records for host-facing failures without adding closed dispatch. |
| **[6]** | `Boundaries`, `Bounds`, `Curves`, `Location`, `Meshes`, `Points`, `Spatial` | Aspect unions that select geometry operations. |

[IMPORTANT] Current Rasm code uses generated `Switch` for operation dispatch. Generated `Map` is available for pure projections when it reduces case-handling code without mixing effects.

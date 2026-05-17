# [H1][LANGUAGEEXT_REPO_INTEGRATION]
>**Dictum:** *Rasm already has canonical LanguageExt shapes; extend them before adding new ones.*

<br>

[IMPORTANT] This file maps LanguageExt API families to the current repo implementation. Use these anchors before creating new rails, wrappers, or helper files.

---
## [1][CORE_ANALYSIS]
>**Dictum:** *`Operation<TGeometry,TOut>` is the analysis rail owner.*

<br>

| [INDEX] | [SYMBOL] | [LANGUAGEEXT_SURFACE] | [ROLE] |
| :-----: | -------- | --------------------- | ------ |
| **[1]** | `Operation<TGeometry,TOut>` | `Func<Seq<TGeometry>, Eff<Env, Seq<TOut>>>` | Canonical executable query shape. |
| **[2]** | `Operation.Apply` | `Eff<Env, Seq<TOut>>` | Runs one geometry or a sequence under runtime context. |
| **[3]** | `Operation.Aggregate` | `Option<Func<Seq<TGeometry>, Eff<Env, Seq<TOut>>>>` | Promotes aggregate-capable queries without overload families. |
| **[4]** | `Operation.Reject` | `Fin.Fail<T>()` lifted into `Eff<Env,T>` | Converts unsupported query shape into an effectful failure. |
| **[5]** | `Analyze.Run` | `Validation<Error, Seq<TOut>>` | Public boundary returning accumulated validation result. |

---
## [2][RUNTIME]
>**Dictum:** *`Env` is the runtime record for geometry effects.*

<br>

| [INDEX] | [SYMBOL] | [LANGUAGEEXT_SURFACE] | [ROLE] |
| :-----: | -------- | --------------------- | ------ |
| **[1]** | `Env` | Runtime record | Carries `Context`, progress, and cancellation. |
| **[2]** | `Env.Asks` and `Env.EnvAsks` | `Eff.runtime<Env>().Map(...).As()` | Reads context or full runtime inside analysis operations. |
| **[3]** | `Context.Create` | `Validation<Error, Context>` | Validates tolerance and unit inputs. |
| **[4]** | `Analyze.Scope` | `Fin<Context>` plus host metadata | Optional boundary context for public API calls. |

[CRITICAL] Do not introduce constructor-injected analysis services for runtime state. Keep runtime state inside `Env` and use `Eff<Env,T>`.

---
## [3][VALIDATION]
>**Dictum:** *`Requirement` bridges native validity into typed rails.*

<br>

| [INDEX] | [SYMBOL] | [LANGUAGEEXT_SURFACE] | [ROLE] |
| :-----: | -------- | --------------------- | ------ |
| **[1]** | `Requirement` | `Seq<Check>` | Immutable check set. |
| **[2]** | `Check` | `Fin<Unit>` | Private single-check failure rail. |
| **[3]** | `Requirement.Apply` | `Validation<Error,T>` | Accumulates requirements for one value. |
| **[4]** | `RequirementContext.Pair` | Applicative `.Apply()` | Validates paired geometry and derived kind requirements. |
| **[5]** | `Fault` | `Error` subtype | Expected domain failures with categories. |

---
## [4][DISPATCH]
>**Dictum:** *Capability lookup is data-driven and rail-preserving.*

<br>

| [INDEX] | [SYMBOL] | [LANGUAGEEXT_SURFACE] | [ROLE] |
| :-----: | -------- | --------------------- | ------ |
| **[1]** | `GeometryKernel.Run` | `Option<T>.ToFin(...).Bind(...)` | Converts capability lookup absence to typed unsupported failure. |
| **[2]** | `Kind.Of`, `GeometryKernel.KindOf` | `Option<Kind>` / `Fin<Kind>` | Absence-only lookup promoted to typed inference failure. |
| **[3]** | `TopologyProjection.Project` | `Fin<Seq<TValue>>` | Owns resource cleanup after fallible projection. |
| **[4]** | `MassKind.Compute` | `Fin<IDisposable>` internally; public overload returns `Eff<Env, IDisposable>` | Lifts native mass properties into analysis runtime. |
| **[5]** | `Bridge.ReadNative`, `Bridge.WriteNative` | `Fin<Seq<Flow<T>>>` / `Unit` | Keeps GH item, twig, and tree handling on one boundary rail. |

---
## [5][GRASSHOPPER_BOUNDARY]
>**Dictum:** *GH2 protocol code collapses rails and reports host messages.*

<br>

| [INDEX] | [SYMBOL] | [LANGUAGEEXT_SURFACE] | [ROLE] |
| :-----: | -------- | --------------------- | ------ |
| **[1]** | `Bridge.Read<T>` | `Fin<Seq<Flow<T>>>` | Reads item, twig, or tree values with typed failures. |
| **[2]** | `Bridge.ReadShape` | `Fin<Seq<Flow<Shape>>>` | Normalizes GH2 and Rhino geometry into `Shape`. |
| **[3]** | `Bridge.Scope` | `Fin<Analyze.Scope>` | Converts GH2 unit and tolerance metadata into analysis context. |
| **[4]** | `GrasshopperRuntime.Read` | `Fin<Option<TVal>>` | Reads optional component parameters. |
| **[5]** | `Output.Of` | `Eff<Env, Seq<Flow<T>>>` collapsed to GH2 writes | Binds one domain aspect to one GH output port. |
| **[6]** | `Flow<T>` | `Option<Site>` plus `Pear<T>` | Carries GH2 metadata and tree location through projection. |
| **[7]** | `Bridge.Write<T>` | `Unit` | Terminal adapter writing GH2 output. |
| **[8]** | `InternalsVisibleTo("Rasm.Grasshopper")` | Internal domain access | Lets the GH adapter use internal kernel rails without widening domain APIs. |

---
## [6][COMPONENTS]
>**Dictum:** *App components should remain thin declarations.*

<br>

Radyab components should:
- Declare `Port<Shape>` and optional `Port<T>` values.
- Use `Output.Of`.
- Select existing `IAspect` values or `Analyze.*` operations.
- Read optional inputs through `GrasshopperRuntime.Read`.
- Avoid direct `IDataAccess`, Rhino tolerance, or output tree plumbing.

---
## [7][RULES]
>**Dictum:** *New code should flow through existing rails.*

<br>

- Add new geometry behavior as an `IAspect` or `Analyze.*` operation.
- Return `Fin<T>` from native projections and smart constructors.
- Return `Validation<Error,T>` from public validation aggregates.
- Return `Eff<Env,T>` when context, cancellation, progress, or deferred native work is required.
- Keep `Match` in `Component.Process`, output writers, app host boundaries, and tests.

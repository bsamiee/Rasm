# [H1][THINKTECTURE_ANALYZERS_SOURCEGEN]
>**Dictum:** *Generated API is only as reliable as the analyzer contract around it.*

<br>

[IMPORTANT] The pinned core package brings matching analyzer, source-generator, and refactoring packages transitively. Treat analyzer warnings as generated-contract feedback, not style noise.

[IMPORTANT] Baseline: `Thinktecture.Runtime.Extensions` `10.2.0` plus transitive analyzer and source-generator packages. Keep generated-shape diagnostics enabled.

---
## [1][SOURCE_GENERATOR]
>**Dictum:** *The source generator materializes the public API promised by attributes.*

<br>

| [INDEX] | [GENERATED_FAMILY] | [MEMBERS] |
| :-----: | ------------------ | ------- |
| [1] | Value objects | Constructors, factories, validation, key member, equality, hash code, string formatting, parsing, conversions, operators, type converters, and optional serialization members. |
| [2] | Complex value objects | Factories, validation, equality, hash code, `ToString`, member comparison, and optional integration support. |
| [3] | Smart enums | Static item registry, lookup, validation, parsing, conversions, comparison, formatting, dispatch, and type converters. |
| [4] | Regular unions | Case-aware constructors, exhaustive `Switch`/`Map`, optional partial dispatch, conversion, equality, and string support. |
| [5] | Ad-hoc unions | Typed case accessors, factories, dispatch, conversion, backing-field layout, and integration hooks. |

---
## [2][MSBUILD_PROPERTIES]
>**Dictum:** *Generator diagnostics stay explicit during investigation.*

<br>

| [INDEX] | [PROPERTY] | [USE] |
| :-----: | ---------- | --- |
| [1] | `ThinktectureRuntimeExtensions_SourceGenerator_Counter` | Enables generator counter behavior. |
| [2] | `ThinktectureRuntimeExtensions_SourceGenerator_GenerateJetBrainsAnnotations` | Emits JetBrains annotations in generated code. |
| [3] | `ThinktectureRuntimeExtensions_SourceGenerator_LogFilePath` | Writes generator diagnostics to a file. |
| [4] | `ThinktectureRuntimeExtensions_SourceGenerator_LogFilePathMustBeUnique` | Forces unique log file paths. |
| [5] | `ThinktectureRuntimeExtensions_SourceGenerator_LogLevel` | Controls generator log verbosity. |
| [6] | `ThinktectureRuntimeExtensions_SourceGenerator_LogMessageInitialBufferSize` | Controls initial log buffer size. |

[IMPORTANT] Use generator logging only for investigation. Do not commit noisy generated logs or broad property changes without an active generator bug.

---
## [3][ANALYZER_RULES]
>**Dictum:** *Analyzer rules protect generated method alignment.*

<br>

| [INDEX] | [RULE] | [POSTURE] |
| :-----: | ------ | -------- |
| [1] | Named `Switch` and `Map` arguments | Required for generated union and smart-enum dispatch. |
| [2] | Internal Thinktecture APIs | Do not use `Thinktecture.Internal.*` types. |
| [3] | Missing string comparers | Add comparer attributes instead of accepting implicit string comparison. |
| [4] | Default structs | Avoid defaultable value-object states unless `AllowDefaultStructs` is an intentional contract. |
| [5] | Unsupported generated combinations | Change attribute settings rather than suppressing diagnostics. |

[CRITICAL] Do not suppress Thinktecture analyzer diagnostics for domain code. Change the generated-shape declaration so the emitted API and usage agree.

---
## [4][OBJECT_FACTORIES]
>**Dictum:** *Object factories define one-value framework contracts.*

<br>

| [INDEX] | [SURFACE] | [USE] |
| :-----: | --------- | --- |
| [1] | `[ObjectFactory<T>]` | Registers a generated or explicit factory for a wire, persistence, or binding value. |
| [2] | `ObjectFactoryAttribute.Type` | Stores runtime value type for non-generic object-factory declarations. |
| [3] | `IObjectFactory<TObject,TValue,TValidationError>.Validate` | Validates the value representation and returns generated object plus validation error. |
| [4] | `IValidationError<TValidationError>.Create` | Creates custom validation errors for generated factories. |
| [5] | `IConvertible<TValue>.ToValue()` | Emits the value representation used by conversion packages. |
| [6] | `UseForSerialization` | Selects object factory for serializer conversion. |
| [7] | `UseWithEntityFramework` | Selects object factory for EF conversion. |
| [8] | `UseForModelBinding` | Selects object factory for ASP.NET model binding. |
| [9] | `ObjectFactory.HasCorrespondingConstructor` / `HasCorrespondingConstructor` | Aligns factory value shape with constructor shape. |
| [10] | `ObjectFactory<ReadOnlySpan<char>>` | Enables System.Text.Json span-based deserialization on .NET 9+. |

[IMPORTANT] Use object factories for custom wire shape, complex value objects, keyless smart enums, and ad-hoc unions. Simple keyed value objects and keyed smart enums often need only generated parsable/conversion members plus the correct integration package.

---
## [5][FRAMEWORK_INTEGRATION]
>**Dictum:** *Core flags do not replace integration packages.*

<br>

| [INDEX] | [BOUNDARY] | [REQUIREMENT] |
| :-----: | ---------- | ----------- |
| [1] | System.Text.Json | Add the JSON integration package and register `ThinktectureJsonConverterFactory` or project dependency generation. |
| [2] | Newtonsoft.Json | Add the Newtonsoft integration package and register its converter support. |
| [3] | MessagePack | Add the MessagePack integration package where MessagePack is the wire protocol. |
| [4] | ASP.NET Minimal APIs | Simple parsable generated types can bind through generated parsing members. |
| [5] | ASP.NET MVC | Add the ASP.NET Core integration package and insert `ThinktectureModelBinderProvider`. |
| [6] | EF Core | Add the EF-major package and register `UseThinktectureValueConverters`, `AddThinktectureValueConverters`, or `HasThinktectureValueConverter`. |
| [7] | Swashbuckle | Add the Swashbuckle package only for OpenAPI schema generation. |

[CRITICAL] `SerializationFrameworks` and object-factory flags select participation; they do not install or register framework integration by themselves.

---
## [6][INTEGRATION_RULES]
>**Dictum:** *Boundary packages activate only where protocol shape demands them.*

<br>

- Use generated `IParsable<T>` for Minimal API binding before adding MVC binder packages.
- Use `ObjectFactory<string>` for one-value cross-framework wire shapes.
- Use `ObjectFactory<ReadOnlySpan<char>>` for System.Text.Json hot paths with known string values.
- Use EF-major package matching the host runtime; do not mix EF8, EF9, and EF10 packages in one project.
- Keep serializer and EF object factories aligned so persistence and wire shape do not diverge.

---
## [7][VERSION_RULES]
>**Dictum:** *v10 names are the only names for new code.*

<br>

- Use `IObjectFactory<TSelf,TValue,TValidationError>` for generated factory constraints.
- Use `ISmartEnum<TSelf>` and `ISmartEnum<TSelf,TKey,TValidationError>` for smart-enum constraints.
- Use generated `Switch`/`Map` parameter names from the pinned package.
- Keep `net9.0` XML as the local API reference for the `10.2.0` package under the repo's computed `net10.0` compatibility.
- Keep optional integration packages aligned to the same stable version as the core package.

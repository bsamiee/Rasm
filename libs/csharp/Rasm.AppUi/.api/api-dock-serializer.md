# [RASM_APPUI_API_DOCK_SERIALIZER]

`Dock.Serializer.SystemTextJson` supplies a `System.Text.Json`-backed implementation of `IDockSerializer` with polymorphic type resolution across all core dock model interfaces, a source-generator analyzer that emits a per-assembly `JsonSerializerContext`, assembly-level registration attributes, and a custom `IList<T>` converter factory.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Dock.Serializer.SystemTextJson` 12.0.0.2
- package: `Dock.Serializer.SystemTextJson` (the Dock 12 docking framework; model interfaces ship transitively under `Dock.Model.ReactiveUI`)
- license: MIT
- floor: `net10.0` consumer; the package multi-targets net6.0 / net8.0 / net10.0 (`lib/<tfm>/Dock.Serializer.SystemTextJson.dll`)
- assembly: `Dock.Serializer.SystemTextJson`
- namespace: `Dock.Serializer.SystemTextJson`
- asset: runtime library (`lib/net10.0/Dock.Serializer.SystemTextJson.dll`)
- asset: source-generator analyzer (`analyzers/dotnet/cs/Dock.Serializer.SystemTextJson.Generators.dll`)
- rail: docking

## [02]-[PUBLIC_TYPES]

[SERIALIZER_TYPES]: IDockSerializer implementation and options factory
- rail: docking

| [INDEX] | [SYMBOL]                           | [VISIBILITY] | [RAIL]                                                       |
| :-----: | :--------------------------------- | :----------- | :----------------------------------------------------------- |
|  [01]   | `DockSerializer`                   | public sealed | `IDockSerializer` implementation                            |
|  [02]   | `JsonConverterFactoryList`         | public       | `JsonConverterFactory` matching `IList<>`                    |
|  [03]   | `JsonConverterList<T>`             | public       | concrete per-element-type `IList<T>` converter              |
|  [04]   | `DockModelPolymorphicTypeResolver` | internal sealed | `DefaultJsonTypeInfoResolver` subclass; the default `TypeInfoResolver` (instantiate via the `DockSerializer()` ctor, not directly) |
|  [05]   | `DockSerializerOptionsFactory`     | internal static | produces the configured `JsonSerializerOptions`; reached through the `DockSerializer` ctors |

[ATTRIBUTE_TYPES]: source-generation registration
- rail: docking

| [INDEX] | [SYMBOL]                            | [RAIL]                              |
| :-----: | :---------------------------------- | :---------------------------------- |
|  [01]   | `DockJsonSourceGenerationAttribute` | activates per-assembly source gen   |
|  [02]   | `DockJsonSerializableAttribute`     | registers additional types into gen |

## [03]-[ENTRYPOINTS]

[DOCKSERIALIZER_CONSTRUCTORS]: `DockSerializer` construction surface
- rail: docking

| [INDEX] | [SURFACE]                                              | [SURFACE_ROOT]   | [RAIL]                                        |
| :-----: | :----------------------------------------------------- | :--------------- | :-------------------------------------------- |
|  [01]   | `DockSerializer()`                                     | `DockSerializer` | defaults: `ObservableCollection<>` + resolver |
|  [02]   | `DockSerializer(Type listType)`                        | `DockSerializer` | custom list type; builds resolver internally  |
|  [03]   | `DockSerializer(Type listType, IJsonTypeInfoResolver)` | `DockSerializer` | custom list type + explicit resolver          |
|  [04]   | `DockSerializer(IJsonTypeInfoResolver)`                | `DockSerializer` | `ObservableCollection<>` + explicit resolver  |

[IDOCKSERIALIZER_SURFACE]: load/save/serialize contract
- rail: docking

| [INDEX] | [SURFACE]                         | [SURFACE_ROOT]   | [RAIL]                        |
| :-----: | :-------------------------------- | :--------------- | :---------------------------- |
|  [01]   | `Serialize<T>(T value)`           | `DockSerializer` | serialize to JSON string      |
|  [02]   | `Deserialize<T>(string text)`     | `DockSerializer` | deserialize from JSON string  |
|  [03]   | `Load<T>(Stream stream)`          | `DockSerializer` | deserialize from UTF-8 stream |
|  [04]   | `Save<T>(Stream stream, T value)` | `DockSerializer` | serialize to UTF-8 stream     |

[OPTIONS_DEFAULTS]: `JsonSerializerOptions` produced by the internal factory
- rail: docking

| [INDEX] | [OPTION]                 | [VALUE]                                                                                 |
| :-----: | :----------------------- | :-------------------------------------------------------------------------------------- |
|  [01]   | `WriteIndented`          | `true`                                                                                  |
|  [02]   | `ReferenceHandler`       | `ReferenceHandler.Preserve`                                                             |
|  [03]   | `DefaultIgnoreCondition` | `JsonIgnoreCondition.WhenWritingNull`                                                   |
|  [04]   | `NumberHandling`         | `JsonNumberHandling.AllowNamedFloatingPointLiterals`                                    |
|  [05]   | `TypeInfoResolver`       | `DockModelPolymorphicTypeResolver` (default) or caller-supplied `IJsonTypeInfoResolver` |
|  [06]   | `Converters`             | `JsonConverterFactoryList(listType)`                                                    |

## [04]-[POLYMORPHIC_RESOLVER]

[POLYMORPHIC_BASE_TYPES]: interfaces resolved polymorphically by `DockModelPolymorphicTypeResolver`
- rail: docking

| [INDEX] | [BASE_TYPE]         | [UNKNOWN_DERIVED_HANDLING]  |
| :-----: | :------------------ | :-------------------------- |
|  [01]   | `IDockable`         | `FallBackToBaseType`        |
|  [02]   | `IDock`             | `FallBackToBaseType`        |
|  [03]   | `IRootDock`         | `FallBackToNearestAncestor` |
|  [04]   | `IDockWindow`       | `FallBackToBaseType`        |
|  [05]   | `IDocumentTemplate` | `FallBackToNearestAncestor` |
|  [06]   | `IToolTemplate`     | `FallBackToNearestAncestor` |

[RESOLVER_BEHAVIOR]:
- Type discriminator property name: `$type`; value: `Type.FullName ?? Type.Name`; `IgnoreUnrecognizedTypeDiscriminators = true`. The resolver overrides `GetTypeInfo` and attaches a cloned `JsonPolymorphismOptions` only for the six base types (dynamic per `GetTypeInfo` so a freshly-loaded plugin assembly's derived types appear without rebuilding the resolver).
- Derived types are discovered at runtime from every non-dynamic loaded assembly (`AppDomain.CurrentDomain.GetAssemblies()`), filtered to `IsClass && !IsAbstract && !ContainsGenericParameters && (IsPublic || IsNestedPublic)` and `baseType.IsAssignableFrom`, then ordered by `FullName` under `StringComparer.Ordinal` — open generics and non-public types are excluded, and `ReflectionTypeLoadException` degrades to its partial `Types` rather than throwing.
- Properties marked `[IgnoreDataMember]` or whose type is assignable to `ICommand` are stripped from serialized output, and the strip also walks every interface the polymorphic base inherits (`BuildInterfaceIgnoredMembers`) so an `ICommand`/`[IgnoreDataMember]` member declared on an inherited interface is removed too — view-model command bindings never serialize.

[CONVERTER_BEHAVIOR]:
- `JsonConverterFactoryList(Type listType)` is a `JsonConverterFactory` whose `CanConvert` matches exactly `IList<>` (open-generic check); `CreateConverter` reads the element type and `Activator.CreateInstance`s `JsonConverterList<element>` passing the caller's `listType`, so every `IList<T>` member deserializes into the concrete list type (default `ObservableCollection<>`) the dock model expects for change tracking.

## [05]-[SOURCE_GENERATOR]

[GENERATOR_SURFACE]: `Dock.Serializer.SystemTextJson.Generators` analyzer
- rail: docking

| [INDEX] | [SYMBOL]                  | [RAIL]                         |
| :-----: | :------------------------ | :----------------------------- |
|  [01]   | `DockJsonSourceGenerator` | Roslyn `IIncrementalGenerator` |

[GENERATOR_EMISSION]: generated artifacts per assembly
- `[assembly: DockJsonSourceGenerationAttribute]` triggers the generator.
- `[assembly: DockJsonSerializableAttribute(typeof(MyType))]` registers additional types.
- Generator emits `internal sealed partial class DockSystemTextJsonContext : JsonSerializerContext`.
- Generator emits `internal sealed class DockSystemTextJsonResolver : IJsonTypeInfoResolver`.
- Generator emits `internal static class DockSystemTextJsonGenerated`.
- Pass the generated `DockSystemTextJsonContext` (which implements `IJsonTypeInfoResolver`) to `DockSerializer(IJsonTypeInfoResolver)` to use source-generated metadata instead of runtime reflection.

## [06]-[IMPLEMENTATION_LAW]

[SERIALIZER_LAW]:
- Package: `Dock.Serializer.SystemTextJson`
- Owns: `IDockSerializer` JSON implementation, polymorphic type resolution for all core dock model interfaces, and the source-generation pipeline
- Accept: construct `DockSerializer()` for default reflection-based serialization; pass a source-generated `IJsonTypeInfoResolver` for AOT-safe serialization
- Reject: constructing custom `JsonSerializerOptions` that replicate the option set owned by this package; hand-rolling polymorphism for `IDockable`/`IDock`/`IRootDock`

[STACKING]:
- Stacks ONTO the `IDockSerializer` contract from `api-dock` (`Serialize<T>`/`Deserialize<T>`/`Load<T>`/`Save<T>` in `Dock.Model.Core`): this concrete `DockSerializer` is the admitted impl, invoked over the `IFactory`/`IDockState.Save(IDock)`/`Restore(IDock)` snapshot (`DockControl` itself exposes no serialize delegate — it binds the `IDock` graph via `Layout`, and the snapshot/serialize handshake is `IDockState` + `IDockSerializer`), so the dock graph (`IRootDock` and its `IDockable`/`IDock`/`IDockWindow`/`IDocumentTemplate`/`IToolTemplate` children) round-trips by `$type` discriminator and dockable `Id` identity. On load `IFactory.DockableLocator` + `RestoreDockable(string)`/`OnDockableRestored` re-resolve and re-own each id-keyed dockable, and `Context` resolves through the host's route index, so a restored dockable rehydrates its view model without a second persistence pass.
- Stacks ONTO the Persistence blob port: `Save<T>(Stream, T)`/`Load<T>(Stream)` write/read UTF-8, so the host serializes the `IRootDock` to a string and crosses the Persistence port as one opaque versioned blob (no file-path overload ships — file I/O is caller-side stream construction). The same serializer instance round-trips an independent board-arrangement blob, so the dock graph and the dashboard board are two blobs over one serializer rail, never two serializer configurations.
- Stacks ONTO the AOT/source-gen path: `[assembly: DockJsonSourceGenerationAttribute]` (+ `[assembly: DockJsonSerializableAttribute(typeof(T))]` per extra type) makes the analyzer emit `DockSystemTextJsonContext : JsonSerializerContext`; passing that context to `DockSerializer(IJsonTypeInfoResolver)` swaps the runtime-reflection `DockModelPolymorphicTypeResolver` for source-generated metadata, so a trimmed/NativeAOT host serializes layout without reflection-discovery of derived types — the one configuration knob that flips reflection-vs-generated, with every other option (`ReferenceHandler.Preserve`, `WhenWritingNull`, `AllowNamedFloatingPointLiterals`, the `IList<T>` factory) preserved.

[CONVERTER_LAW]:
- `JsonConverterFactoryList` and `JsonConverterList<T>` handle `IList<T>` properties using the caller-supplied concrete list type (default `ObservableCollection<>`).
- Accept: pass the generic type definition (e.g. `typeof(ObservableCollection<>)`) as `listType`; the converter closes it at runtime per element type.
- Reject: registering a separate `IList<T>` converter outside this factory surface.

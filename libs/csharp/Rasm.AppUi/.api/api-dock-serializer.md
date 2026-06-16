# [RASM_APPUI_API_DOCK_SERIALIZER]

`Dock.Serializer.SystemTextJson` supplies a `System.Text.Json`-backed implementation of `IDockSerializer` with polymorphic type resolution across all core dock model interfaces, a source-generator analyzer that emits a per-assembly `JsonSerializerContext`, assembly-level registration attributes, and a custom `IList<T>` converter factory.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Dock.Serializer.SystemTextJson`
- package: `Dock.Serializer.SystemTextJson`
- assembly: `Dock.Serializer.SystemTextJson`
- namespace: `Dock.Serializer.SystemTextJson`
- asset: runtime library (`lib/net10.0/Dock.Serializer.SystemTextJson.dll`)
- asset: source-generator analyzer (`analyzers/dotnet/cs/Dock.Serializer.SystemTextJson.Generators.dll`)
- rail: docking

## [2]-[PUBLIC_TYPES]

[SERIALIZER_TYPES]: IDockSerializer implementation and options factory
- rail: docking

| [INDEX] | [SYMBOL]                           | [RAIL]                                 |
| :-----: | :--------------------------------- | :------------------------------------- |
|   [1]   | `DockSerializer`                   | `IDockSerializer` implementation       |
|   [2]   | `JsonConverterFactoryList`         | `IList<T>` converter factory           |
|   [3]   | `JsonConverterList<T>`             | concrete `IList<T>` converter          |
|   [4]   | `DockModelPolymorphicTypeResolver` | `DefaultJsonTypeInfoResolver` subclass |

[ATTRIBUTE_TYPES]: source-generation registration
- rail: docking

| [INDEX] | [SYMBOL]                            | [RAIL]                              |
| :-----: | :---------------------------------- | :---------------------------------- |
|   [1]   | `DockJsonSourceGenerationAttribute` | activates per-assembly source gen   |
|   [2]   | `DockJsonSerializableAttribute`     | registers additional types into gen |

## [3]-[ENTRYPOINTS]

[DOCKSERIALIZER_CONSTRUCTORS]: `DockSerializer` construction surface
- rail: docking

| [INDEX] | [SURFACE]                                              | [SURFACE_ROOT]   | [RAIL]                                        |
| :-----: | :----------------------------------------------------- | :--------------- | :-------------------------------------------- |
|   [1]   | `DockSerializer()`                                     | `DockSerializer` | defaults: `ObservableCollection<>` + resolver |
|   [2]   | `DockSerializer(Type listType)`                        | `DockSerializer` | custom list type; builds resolver internally  |
|   [3]   | `DockSerializer(Type listType, IJsonTypeInfoResolver)` | `DockSerializer` | custom list type + explicit resolver          |
|   [4]   | `DockSerializer(IJsonTypeInfoResolver)`                | `DockSerializer` | `ObservableCollection<>` + explicit resolver  |

[IDOCKSERIALIZER_SURFACE]: load/save/serialize contract
- rail: docking

| [INDEX] | [SURFACE]                         | [SURFACE_ROOT]   | [RAIL]                        |
| :-----: | :-------------------------------- | :--------------- | :---------------------------- |
|   [1]   | `Serialize<T>(T value)`           | `DockSerializer` | serialize to JSON string      |
|   [2]   | `Deserialize<T>(string text)`     | `DockSerializer` | deserialize from JSON string  |
|   [3]   | `Load<T>(Stream stream)`          | `DockSerializer` | deserialize from UTF-8 stream |
|   [4]   | `Save<T>(Stream stream, T value)` | `DockSerializer` | serialize to UTF-8 stream     |

[OPTIONS_DEFAULTS]: `JsonSerializerOptions` produced by the internal factory
- rail: docking

| [INDEX] | [OPTION]                 | [VALUE]                                                                                 |
| :-----: | :----------------------- | :-------------------------------------------------------------------------------------- |
|   [1]   | `WriteIndented`          | `true`                                                                                  |
|   [2]   | `ReferenceHandler`       | `ReferenceHandler.Preserve`                                                             |
|   [3]   | `DefaultIgnoreCondition` | `JsonIgnoreCondition.WhenWritingNull`                                                   |
|   [4]   | `NumberHandling`         | `JsonNumberHandling.AllowNamedFloatingPointLiterals`                                    |
|   [5]   | `TypeInfoResolver`       | `DockModelPolymorphicTypeResolver` (default) or caller-supplied `IJsonTypeInfoResolver` |
|   [6]   | `Converters`             | `JsonConverterFactoryList(listType)`                                                    |

## [4]-[POLYMORPHIC_RESOLVER]

[POLYMORPHIC_BASE_TYPES]: interfaces resolved polymorphically by `DockModelPolymorphicTypeResolver`
- rail: docking

| [INDEX] | [BASE_TYPE]         | [UNKNOWN_DERIVED_HANDLING]  |
| :-----: | :------------------ | :-------------------------- |
|   [1]   | `IDockable`         | `FallBackToBaseType`        |
|   [2]   | `IDock`             | `FallBackToBaseType`        |
|   [3]   | `IRootDock`         | `FallBackToNearestAncestor` |
|   [4]   | `IDockWindow`       | `FallBackToBaseType`        |
|   [5]   | `IDocumentTemplate` | `FallBackToNearestAncestor` |
|   [6]   | `IToolTemplate`     | `FallBackToNearestAncestor` |

[RESOLVER_BEHAVIOR]:
- Type discriminator property name: `$type`; value: `Type.FullName ?? Type.Name`.
- `IgnoreUnrecognizedTypeDiscriminators = true`.
- Derived types are discovered at runtime from all loaded assemblies (`AppDomain.CurrentDomain.GetAssemblies()`), filtered to public non-abstract concrete types assignable to each base.
- Properties marked `[IgnoreDataMember]` or whose type is assignable to `ICommand` are stripped from serialized output.

## [5]-[SOURCE_GENERATOR]

[GENERATOR_SURFACE]: `Dock.Serializer.SystemTextJson.Generators` analyzer
- rail: docking

| [INDEX] | [SYMBOL]                  | [RAIL]                         |
| :-----: | :------------------------ | :----------------------------- |
|   [1]   | `DockJsonSourceGenerator` | Roslyn `IIncrementalGenerator` |

[GENERATOR_EMISSION]: generated artifacts per assembly
- `[assembly: DockJsonSourceGenerationAttribute]` triggers the generator.
- `[assembly: DockJsonSerializableAttribute(typeof(MyType))]` registers additional types.
- Generator emits `internal sealed partial class DockSystemTextJsonContext : JsonSerializerContext`.
- Generator emits `internal sealed class DockSystemTextJsonResolver : IJsonTypeInfoResolver`.
- Generator emits `internal static class DockSystemTextJsonGenerated`.
- Pass the generated `DockSystemTextJsonContext` (which implements `IJsonTypeInfoResolver`) to `DockSerializer(IJsonTypeInfoResolver)` to use source-generated metadata instead of runtime reflection.

## [6]-[IMPLEMENTATION_LAW]

[SERIALIZER_LAW]:
- Package: `Dock.Serializer.SystemTextJson`
- Owns: `IDockSerializer` JSON implementation, polymorphic type resolution for all core dock model interfaces, and the source-generation pipeline
- Accept: construct `DockSerializer()` for default reflection-based serialization; pass a source-generated `IJsonTypeInfoResolver` for AOT-safe serialization
- Reject: constructing custom `JsonSerializerOptions` that replicate the option set owned by this package; hand-rolling polymorphism for `IDockable`/`IDock`/`IRootDock`

[CONVERTER_LAW]:
- `JsonConverterFactoryList` and `JsonConverterList<T>` handle `IList<T>` properties using the caller-supplied concrete list type (default `ObservableCollection<>`).
- Accept: pass the generic type definition (e.g. `typeof(ObservableCollection<>)`) as `listType`; the converter closes it at runtime per element type.
- Reject: registering a separate `IList<T>` converter outside this factory surface.

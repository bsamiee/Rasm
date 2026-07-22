# [RASM_APPUI_API_DOCK_SERIALIZER]

`Dock.Serializer.SystemTextJson` owns the `IDockSerializer` JSON round-trip for the dock model graph: `$type` polymorphism over the core dock interfaces, an `IList<T>` converter binding the caller's concrete list type, and a source-generation path for trimmed and NativeAOT layouts.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Dock.Serializer.SystemTextJson`
- package: `Dock.Serializer.SystemTextJson` (MIT)
- assembly: `Dock.Serializer.SystemTextJson`
- namespace: `Dock.Serializer.SystemTextJson`
- target: `lib/net10.0`
- asset: runtime library
- asset: source-generator analyzer (`analyzers/dotnet/cs/Dock.Serializer.SystemTextJson.Generators.dll`)
- depends: `Dock.Model`
- rail: docking

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPES]: serializer, resolver, converters, registration attributes, and analyzer

| [INDEX] | [SYMBOL]                            | [TYPE_FAMILY] | [CAPABILITY]                               |
| :-----: | :---------------------------------- | :------------ | :----------------------------------------- |
|  [01]   | `DockSerializer`                    | class         | sealed `IDockSerializer` JSON round-trip   |
|  [02]   | `JsonConverterFactoryList`          | class         | `JsonConverterFactory` matching `IList<>`  |
|  [03]   | `JsonConverterList<T>`              | class         | concrete `IList<T>` element converter      |
|  [04]   | `DockModelPolymorphicTypeResolver`  | class         | internal default `$type` resolver          |
|  [05]   | `DockSerializerOptionsFactory`      | class         | internal `JsonSerializerOptions` factory   |
|  [06]   | `DockJsonSourceGenerationAttribute` | attribute     | activates per-assembly source generation   |
|  [07]   | `DockJsonSerializableAttribute`     | attribute     | registers an extra type into generation    |
|  [08]   | `DockJsonSourceGenerator`           | class         | Roslyn `IIncrementalGenerator` in analyzer |

## [03]-[ENTRYPOINTS]

[SERIALIZER_ENTRYPOINTS]: `DockSerializer` construction and the `IDockSerializer` round-trip

| [INDEX] | [SURFACE]                                     | [SHAPE]  | [CAPABILITY]                                 |
| :-----: | :-------------------------------------------- | :------- | :------------------------------------------- |
|  [01]   | `DockSerializer()`                            | ctor     | `ObservableCollection<>` + default resolver  |
|  [02]   | `DockSerializer(Type)`                        | ctor     | custom list type, resolver built internally  |
|  [03]   | `DockSerializer(Type, IJsonTypeInfoResolver)` | ctor     | custom list type + explicit resolver         |
|  [04]   | `DockSerializer(IJsonTypeInfoResolver)`       | ctor     | `ObservableCollection<>` + explicit resolver |
|  [05]   | `Serialize<T>(T) -> string`                   | instance | model to JSON string                         |
|  [06]   | `Deserialize<T>(string) -> T?`                | instance | JSON string to model                         |
|  [07]   | `Load<T>(Stream) -> T?`                       | instance | UTF-8 stream to model                        |
|  [08]   | `Save<T>(Stream, T)`                          | instance | model to UTF-8 stream                        |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `DockSerializer` round-trips the `IRootDock` graph, discriminating every derived type by `$type` (`Type.FullName ?? Type.Name`, `IgnoreUnrecognizedTypeDiscriminators = true`); each `GetTypeInfo` re-scans loaded assemblies, so a plugin-loaded derived type resolves without a resolver rebuild.
- Six base types resolve polymorphically: `IDockable`, `IDock`, `IDockWindow` fall back to base type; `IRootDock`, `IDocumentTemplate`, `IToolTemplate` fall back to nearest ancestor.
- `[IgnoreDataMember]` and `ICommand`-typed members strip from output across the base interface and every inherited interface, so view-model command bindings never serialize.
- `IList<T>` members deserialize into the caller's concrete list type through `JsonConverterFactoryList`, defaulting to `ObservableCollection<>` for change tracking.
- `DockSerializerOptionsFactory` fixes `WriteIndented = true`, `ReferenceHandler.Preserve`, `DefaultIgnoreCondition = WhenWritingNull`, `NumberHandling = AllowNamedFloatingPointLiterals`, the `TypeInfoResolver` (default `DockModelPolymorphicTypeResolver` or caller-supplied), and the `JsonConverterFactoryList` converter.
- `[assembly: DockJsonSourceGenerationAttribute]` with one `[assembly: DockJsonSerializableAttribute(typeof(T))]` per extra type drives the analyzer to emit `DockSystemTextJsonContext : JsonSerializerContext`; passing that context to `DockSerializer(IJsonTypeInfoResolver)` swaps runtime reflection for trimmed and NativeAOT metadata.

[STACKING]:
- `Dock.Avalonia` / `Dock.Model.ReactiveUI`(`.api/api-dock.md`): `DockSerializer.Save<IRootDock>(Stream)`/`Load<IRootDock>` round-trips the `IFactory`-built `IDock` graph over the `IDockState.Save(IDock)`/`Restore(IDock)` snapshot, discriminating the `IDockable`/`IDock`/`IRootDock` tree by `$type` and rehydrating each dockable through `DockableLocator` and `RestoreDockable(string)`.
- AppUi Shell (`.planning/Shell/`): the Shell carries `IRootDock` across the Persistence port as one opaque UTF-8 blob through `Save<T>(Stream)`/`Load<T>(Stream)`, and one serializer instance round-trips an independent board-arrangement blob on the same rail and configuration.

[LOCAL_ADMISSION]:
- `DockSerializer` is the one admitted `IDockSerializer` binding; an AOT build passes a source-generated `IJsonTypeInfoResolver`, and reflection over `DockModelPolymorphicTypeResolver` is the default otherwise.

[RAIL_LAW]:
- Package: `Dock.Serializer.SystemTextJson`
- Owns: the `IDockSerializer` JSON round-trip, `$type` polymorphism over the core dock interfaces, and the source-generation pipeline.
- Accept: `DockSerializer()` for reflection round-trip; a source-generated `IJsonTypeInfoResolver` for AOT-safe round-trip.
- Reject: custom `JsonSerializerOptions` replicating the owned option set; hand-rolled polymorphism for `IDockable`/`IDock`/`IRootDock`; a second `IList<T>` converter outside `JsonConverterFactoryList`.

# [RASM_BIM_API_OPENSTUDIO]

`NREL.OpenStudio.macOS-arm64` is the osx-arm64 SWIG-generated C# binding to the NREL OpenStudio SDK: it owns the OSM model, the EnergyPlus IDF/IDD object store, and the forward/reverse translator matrix between a `Model` and the neutral energy and geometry formats. `OpenStudio.dll` marshals a bundled RID-locked native runtime, and every managed wrapper holds a native handle under `IDisposable`. This is the OSM/IDF runtime leg of the Bim energy-model exchange, meeting the `HoneybeeSchema` HBJSON authoring leg at gbXML; the EnergyPlus solver stays Compute-owned.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `NREL.OpenStudio.macOS-arm64`
- package: `NREL.OpenStudio.macOS-arm64` (NREL OpenStudio License)
- assembly: `OpenStudio` (`lib/netstandard2.0/OpenStudio.dll`, the managed SWIG wrapper)
- namespace: `OpenStudio` (the model/translator/utility domain types, the `Optional<T>` and `*Vector` SWIG marshaling families, and the per-module `OpenStudio*PINVOKE` DllImport classes)
- asset: managed binding TFM `netstandard2.0`, binding forward under net10.0; the native runtime is RID-locked to `osx-arm64`
- native: `runtimes/osx-arm64/native/libopenstudiolib.dylib` (the SDK) with the `libopenstudio_csharp` / `libopenstudio_model_csharp` / `libopenstudio_translators_csharp` P/Invoke shims; `build/OpenStudio.targets` copies the dylibs next to consumer output
- platform: the macOS-arm64 member of a per-RID family; a Windows or Linux host binds the sibling `win-x64` / `linux-x64` package with an identical managed API over a different native runtime
- dependency: empty net-standard dependency group; the native runtime is fully bundled with no managed transitive deps
- consumer: `libs/csharp/Rasm.Bim` (IFC↔OSM/gbXML semantic exchange), `libs/csharp/Rasm.Compute` (whole-building-energy simulation)
- rail: energy

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: SWIG marshaling primitives

| [INDEX] | [SYMBOL]                      | [TYPE_FAMILY] | [CAPABILITY]                                             |
| :-----: | :---------------------------- | :------------ | :------------------------------------------------------- |
|  [01]   | `Optional<T>` family          | class         | `boost::optional` may-miss carrier; gate then unwrap     |
|  [02]   | `*Vector` family              | class         | `std::vector` marshaling, `IList`-shaped plural carrier  |
|  [03]   | `Path` / `OptionalPath`       | class         | `boost::filesystem::path`; no `Path(string)` ctor        |
|  [04]   | `UUID`                        | class         | object identity handle, the `getObject(UUID)` key        |
|  [05]   | `OpenStudioUtilitiesCore`     | class         | SWIG global-function host for path and UUID construction |
|  [06]   | `Logger` / `LogMessageVector` | class         | native log sink; the translator diagnostics carrier      |
|  [07]   | `ProgressBar`                 | class         | SWIG director delivering native progress callbacks       |

- `Optional<T>`: `is_initialized()` `isNull()` `get()` `value_or(T)` `set(T)` `reset()`.
- `OpenStudioUtilitiesCore`: `toPath(string)` `toString(Path)` `createUUID()` `toUUID(string)`.
- `ProgressBar`: `protected ProgressBar()` ctor, `virtual onPercentageUpdated(double)` override sink.

[PUBLIC_TYPE_SCOPE]: model and IDF/IDD object store

| [INDEX] | [SYMBOL]                            | [TYPE_FAMILY] | [CAPABILITY]                                                 |
| :-----: | :---------------------------------- | :------------ | :----------------------------------------------------------- |
|  [01]   | `Model`                             | class         | the OSM model (`: Workspace`), the model-object getter root  |
|  [02]   | `Workspace`                         | class         | the generic IDD-object store backing `Model` and an IDF      |
|  [03]   | `IdfFile` / `OptionalIdfFile`       | class         | the EnergyPlus IDF text model                                |
|  [04]   | `IddFile` / `Idd` / `IddObjectType` | class         | the Input Data Dictionary; `IddObjectType` keys the type get |
|  [05]   | `ModelObject`                       | class         | base of every model object, the `translateModelObject` input |
|  [06]   | leaf `ModelObject`s                 | class         | `Building` `Space` `Surface` `ThermalZone` `Construction`    |

- `Model`: ctors `Model()` `Model(IdfFile)` `Model(Workspace)` `Model(Model)`; `modelObjects(bool sorted)` → `ModelObjectVector`; load and save are `[03]`'s.
- `Workspace`: `getObjectsByType(IddObjectType)` → `WorkspaceObjectVector`; `getObjectByTypeAndName(IddObjectType, string)` → `OptionalWorkspaceObject`; `getObject(UUID)`; `objects(bool sorted)`; `save(Path, bool overwrite)`.

[PUBLIC_TYPE_SCOPE]: translators — the exchange matrix

Each translator is `IDisposable`, exposes `warnings()`/`errors()` → `LogMessageVector`, and takes an optional `ProgressBar`; forward runs `Model` → external format, reverse runs external → `Model`/`OptionalModel`.

| [INDEX] | [SYMBOL]                                                       | [TYPE_FAMILY] | [CAPABILITY]                                 |
| :-----: | :------------------------------------------------------------- | :------------ | :------------------------------------------- |
|  [01]   | `EnergyPlusForwardTranslator`                                  | class         | `translateModel(Model)` → `Workspace`        |
|  [02]   | `EnergyPlusReverseTranslator`                                  | class         | `translateWorkspace(Workspace…)` → `Model`   |
|  [03]   | `GbXMLForwardTranslator` / `GbXMLReverseTranslator`            | class         | gbXML ↔ `Model`, the BIM/IFC energy bridge   |
|  [04]   | `SddForwardTranslator` / `SddReverseTranslator`                | class         | CBECC SDD (California compliance) ↔ `Model`  |
|  [05]   | `ThreeJSForwardTranslator` / `ThreeJSReverseTranslator`        | class         | three.js geometry for web preview            |
|  [06]   | `GltfForwardTranslator`                                        | class         | glTF geometry export for web preview         |
|  [07]   | `FloorplanJSForwardTranslator` / `FloorspaceReverseTranslator` | class         | FloorspaceJS 2D floor-editor exchange        |
|  [08]   | `ISOModelForwardTranslator`                                    | class         | ISO 13790 monthly-model export               |
|  [09]   | `RadianceForwardTranslator`                                    | class         | Radiance daylight export                     |
|  [10]   | `ContamForwardTranslator`                                      | class         | CONTAM airflow export                        |
|  [11]   | `VersionTranslator`                                            | class         | robust `.osm` loader upgrading an older file |

- `EnergyPlusForwardTranslator`: `translateModelObject(ModelObject)` → `Workspace`; `forwardTranslatorOptions()` / `setForwardTranslatorOptions(ForwardTranslatorOptions)`; `setKeepRunControlSpecialDays(bool)`; `setIPTabularOutput(bool)`.
- `VersionTranslator`: `loadModel(Path[, ProgressBar])` → `OptionalModel`; `loadModelFromString(string[, ProgressBar])` → `OptionalModel`; `loadComponent(Path)` → `OptionalComponent`; `originalVersion()` → `VersionString`. It supersedes `Model.load` when the file version is not guaranteed current.

[PUBLIC_TYPE_SCOPE]: files, results, workflow

| [INDEX] | [SYMBOL]                    | [TYPE_FAMILY] | [CAPABILITY]                                                |
| :-----: | :-------------------------- | :------------ | :---------------------------------------------------------- |
|  [01]   | `EpwFile`                   | class         | the EnergyPlus weather file (`EpwFile(Path)`)               |
|  [02]   | `SqlFile`                   | class         | the EnergyPlus results SQLite (`SqlFile(Path)`), post-run   |
|  [03]   | `WorkflowJSON` / `OSRunner` | class         | the OpenStudio Workflow (OSW) and measure runner            |
|  [04]   | `VersionString`             | class         | a parsed OpenStudio version, the `VersionTranslator` output |

[PUBLIC_TYPE_SCOPE]: CLR enums

Only these types are true CLR enums; most OpenStudio "enumerations" are SWIG `*Enum`/`EnumBase` string-classes such as `IddObjectType`, matched on string value.

| [INDEX] | [SYMBOL]                                                  | [TYPE_FAMILY] | [CAPABILITY]                                           |
| :-----: | :-------------------------------------------------------- | :------------ | :----------------------------------------------------- |
|  [01]   | `LogLevel`                                                | enum          | the `Logger` verbosity band (`Trace`…`Fatal`)          |
|  [02]   | `FloatFormat`                                             | enum          | numeric formatting for IDF/text emit                   |
|  [03]   | `InterpMethod` / `ExtrapMethod`                           | enum          | interp/extrap policy for time-series and curve lookups |
|  [04]   | `ThreeSide` / `XMLValidatorType` / `ModelicaCompilerType` | enum          | three.js side culling, XML validator, Modelica target  |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: load and save a model

| [INDEX] | [SURFACE]                                             | [SHAPE]  | [CAPABILITY]                             |
| :-----: | :---------------------------------------------------- | :------- | :--------------------------------------- |
|  [01]   | `VersionTranslator.loadModel(Path)` → `OptionalModel` | instance | robust `.osm` read, upgrades the version |
|  [02]   | `Model.load(Path, Path)` → `OptionalModel`            | static   | direct load when the file is current     |
|  [03]   | `model.save(Path, bool)` → `bool`                     | instance | persist to `.osm`, from `Workspace`      |
|  [04]   | `IdfFile.load(Path, IddFileType)` → `OptionalIdfFile` | static   | read an EnergyPlus IDF into the store    |

[ENTRYPOINT_SCOPE]: translate

| [INDEX] | [SURFACE]                                                           | [SHAPE]  | [CAPABILITY]                           |
| :-----: | :------------------------------------------------------------------ | :------- | :------------------------------------- |
|  [01]   | `EnergyPlusForwardTranslator().translateModel(Model)` → `Workspace` | instance | OSM → EnergyPlus IDF, then diagnostics |
|  [02]   | `EnergyPlusReverseTranslator().translateWorkspace(…)` → `Model`     | instance | EnergyPlus IDF → OSM                   |
|  [03]   | `GbXMLReverseTranslator().loadModel(Path)` → `OptionalModel`        | instance | gbXML → OSM, the BIM/IFC bridge        |
|  [04]   | `SqlFile(Path)` with typed getters                                  | ctor     | read the EnergyPlus results SQLite     |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Every managed type is a thin wrapper around a native handle (`HandleRef swigCPtr` with a `cMemoryOwn` flag) under `IDisposable`; drive the public `OpenStudio.*` wrappers, never the `OpenStudio*PINVOKE` DllImport classes. `build/OpenStudio.targets` stages the native dylibs next to consumer output, RID-locked to `osx-arm64`.
- Every wrapper owns native memory, so `Model`, each translator, `SqlFile`, `EpwFile`, and every `Optional*`/`*Vector` result binds under `using` or `Dispose()`; a dropped handle leaks memory the GC cannot reclaim deterministically. Model mutation is not thread-safe: the native SDK admits one logical owner per `Model`, translators run sequentially, and a translation offloads as one unit of work over this single-threaded boundary.
- Any load or get that can miss returns an `Optional<T>`; the law is `is_initialized()` (or `!isNull()`) then `get()`, else `value_or(default)`, and a bare `get()` on an empty optional faults in native code. Lower the `Optional<T>` onto a `Fin<T>`/`Option<T>` at the Exchange boundary so internal code never sees the SWIG optional.
- Every file API takes a `Path` built with `OpenStudioUtilitiesCore.toPath(string)` and read back with `toString(Path)`; a raw `string` does not compile against the file overloads.

[STACKING]:
- `HoneybeeSchema`(`.api/api-honeybee-schema`): the HBJSON authored model meets this OSM/IDF runtime at gbXML — `GbXMLReverseTranslator.loadModel` ingests the shared gbXML — and at the canonical Bim energy model; the full HBJSON→OSM path runs the external `honeybee-openstudio` Python step.
- `GeometryGymIFC`(`.api/api-geometrygym-ifc`): an IFC building exports gbXML, `GbXMLReverseTranslator.loadModel` folds it into a `Model`, and IFC spaces and zones become OSM spaces and thermal zones at the Exchange/import boundary.
- `SharpGLTF.Ext.3DTiles`(`.api/api-sharpgltf-3dtiles`): `GltfForwardTranslator` and `ThreeJSForwardTranslator` emit a model's geometry onto the Exchange/export delivery rail the glTF and 3D-Tiles legs share.
- `System.IO.Hashing`(`libs/csharp/.api/api-hashing`): a saved `.osm`/IDF UTF-8 string feeds `XxHash3` for the in-process fingerprint and `XxHash128` for the persisted content key into the `Rasm.Persistence` artifact index.
- within-lib: the Bim Energy Exchange lowers each `Optional<Model>` onto `Fin<Model>`, captures translator `errors()`/`warnings()` as a typed receipt, and offloads a translation as one unit of work.

[LOCAL_ADMISSION]:
- Model read enters through `VersionTranslator.loadModel(path)` returning an `OptionalModel` lowered to `Fin<Model>`; model write enters through `model.save(path, overwrite)`.
- Translation enters through the matching `*Translator` under a `using`, capturing `errors()`/`warnings()` as a typed receipt.

[RAIL_LAW]:
- Package: `NREL.OpenStudio.macOS-arm64` (assembly `OpenStudio`)
- Owns: the OSM model and EnergyPlus IDF/IDD object store, the forward/reverse translator matrix, the `VersionTranslator` robust loader, the weather/results/workflow files, and the SWIG native-handle marshaling
- Accept: OSM load/save/version-upgrade, `Model` ↔ EnergyPlus/gbXML/SDD translation, EnergyPlus results reading, the osx-arm64 native energy rail
- Reject: HBJSON authoring (`HoneybeeSchema` owns it), IFC semantics (GeometryGym owns it), running the EnergyPlus solver (the external binary does), cross-platform native portability (each RID admits its own package), and leaking `OpenStudio.*` handles past the Exchange boundary

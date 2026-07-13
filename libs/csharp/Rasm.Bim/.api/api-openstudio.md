# [RASM_BIM_API_OPENSTUDIO]

`NREL.OpenStudio.macOS-arm64` is the osx-arm64 SWIG-generated C# binding to the NREL
OpenStudio SDK — the whole-building-energy-modeling engine that owns the OSM model, the
EnergyPlus IDF/IDD object layer, and the translator matrix to and from EnergyPlus, gbXML,
SDD, three.js/glTF, FloorspaceJS, ISO, Radiance, and CONTAM. The managed `OpenStudio.dll`
(5390 generated types, namespace `OpenStudio`) marshals into a bundled native runtime
(`libopenstudiolib.dylib` + the three `libopenstudio_*_csharp.dylib` SWIG shims); every
wrapper owns a native handle and is `IDisposable`. It is the EnergyPlus leg of the Bim
energy-model exchange owner — the OSM/IDF runtime sibling to the `HoneybeeSchema`
(`api-honeybee-schema`) HBJSON authoring leg, meeting it at gbXML and the canonical Bim
energy model.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `NREL.OpenStudio.macOS-arm64`
- package: `NREL.OpenStudio.macOS-arm64`
- license: NREL OpenStudio License (the `LICENSE.md` at `github.com/NREL/OpenStudio` — a permissive BSD-3-Clause-derived license; the nuspec carries a `licenseUrl`, no SPDX expression)
- assembly: `OpenStudio` (`lib/netstandard2.0/OpenStudio.dll`, the managed SWIG wrapper)
- namespace: `OpenStudio` (the full 5390-type generated surface: the model/translator/utility domain types plus the `Optional<T>` (~743) and `*Vector` (~722) SWIG marshaling families) + 38 per-SWIG-module `OpenStudio*PINVOKE` marshaling classes (`OpenStudioModelPINVOKE`, `OpenStudioEnergyPlusPINVOKE`, `OpenStudioOSVersionPINVOKE`, `OpenStudioGBXMLPINVOKE`, …)
- asset: managed binding TFM `netstandard2.0` (binds forward under net10.0); the runtime is RID-LOCKED to `osx-arm64`
- native: `runtimes/osx-arm64/native/libopenstudiolib.dylib` (the SDK) + `libopenstudio_csharp.dylib` / `libopenstudio_model_csharp.dylib` / `libopenstudio_translators_csharp.dylib` (the SWIG P/Invoke shims); `build/OpenStudio.targets` copies the dylibs next to the consumer output
- platform: this package is the macOS-arm64 RID member of a per-RID family — a Windows/Linux host admits the sibling `NREL.OpenStudio.win-x64` / `NREL.OpenStudio.linux-x64` package; the managed API is identical, only the native runtime differs
- dependency: empty net-standard dependency group (the native runtime is fully bundled; no managed transitive deps)
- consumer: `libs/csharp/Rasm.Bim` (the IFC↔OSM/gbXML SEMANTIC exchange owner) — co-owned with `libs/csharp/Rasm.Compute` (the disjoint whole-building-energy SIMULATION owner, `Rasm.Compute/.api/api-openstudio`) on the one central pin; the two folder-scoped catalogs frame disjoint rails (Bim exchanges IFC↔OSM semantics, Compute simulates the canonical seam graph), aligned not coupled
- rail: energy

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: SWIG marshaling primitives
- package: `NREL.OpenStudio.macOS-arm64`
- namespace: `OpenStudio`
- rail: energy

Every wrapper holds a native `HandleRef` (`swigCPtr`) with a `cMemoryOwn` flag and implements `IDisposable`; the C++ STL/Boost shapes surface as the families below, not as BCL collections.

| [INDEX] | [SYMBOL]                      | [CAPABILITY]                                                                                             |
| :-----: | :---------------------------- | :------------------------------------------------------------------------------------------------------- |
|  [01]   | `Optional<T>` family          | `boost::optional` marshaling; `is_initialized()`/`isNull()`, `get()`, `value_or(T)`, `set(T)`, `reset()` |
|  [02]   | `*Vector` family              | `std::vector` marshaling; `IList`-shaped (indexer, `Count`, `Add`), the plural-result carrier            |
|  [03]   | `Path` / `OptionalPath`       | `boost::filesystem::path`; NO `Path(string)` ctor — use `OpenStudioUtilitiesCore.toPath`/`toString`      |
|  [04]   | `UUID`                        | handle (`createUUID()`/`toUUID(string)`); `WorkspaceObject` identity, `getObject(UUID)` key              |
|  [05]   | `OpenStudioUtilitiesCore`     | SWIG global-function host (`toPath`/`toString`/`createUUID`/`toUUID`); not the `*PINVOKE` shims          |
|  [06]   | `Logger` / `LogMessageVector` | native log sink; `LogLevel` verbosity, translator `warnings()`/`errors()` → `LogMessageVector`           |

[PUBLIC_TYPE_SCOPE]: model + IDF/IDD object store
- package: `NREL.OpenStudio.macOS-arm64`
- namespace: `OpenStudio`
- rail: energy

| [INDEX] | [SYMBOL]                            | [CAPABILITY]                                                                               |
| :-----: | :---------------------------------- | :----------------------------------------------------------------------------------------- |
|  [01]   | `Model`                             | the OSM model (`: Workspace`); the model-object getter root — roster in `[01]-[MODEL]`     |
|  [02]   | `Workspace`                         | the generic IDD-object store backing `Model` and an IDF — roster in `[02]-[WORKSPACE]`     |
|  [03]   | `IdfFile` / `OptionalIdfFile`       | the EnergyPlus IDF text model (`IdfFile.load(Path, IddFileType)` → `OptionalIdfFile`)      |
|  [04]   | `IddFile` / `Idd` / `IddObjectType` | the EnergyPlus Input Data Dictionary; `IddObjectType` is the typed `getObjectsByType` key  |
|  [05]   | `ModelObject`                       | base of every model object; the `ModelObjectVector` element + `translateModelObject` input |
|  [06]   | leaf `ModelObject`s                 | `Building`, `Space`, `Surface`, `ThermalZone`, `Construction`, `Schedule`                  |

- [01]-[MODEL]: ctors `Model()`/`Model(IdfFile)`/`Model(Workspace)`/`Model(Model)`; `modelObjects(bool sorted)` → `ModelObjectVector`; `getModelObjectLists*`. Load/save are `[03]`'s.
- [02]-[WORKSPACE]: `getObjectsByType(IddObjectType)` → `WorkspaceObjectVector`; `getObjectByTypeAndName(IddObjectType, name)` → `OptionalWorkspaceObject`; `getObject(UUID)`; `objects(bool sorted)`; `versionObject`; `save(Path, bool overwrite)`.

[PUBLIC_TYPE_SCOPE]: translators (the exchange matrix)
- package: `NREL.OpenStudio.macOS-arm64`
- namespace: `OpenStudio`
- rail: energy

Each translator is `IDisposable`, exposes `warnings()`/`errors()` → `LogMessageVector`, and takes an optional `ProgressBar`. "Forward" = `Model` → external format; "Reverse" = external → `Model`/`OptionalModel`.

| [INDEX] | [SYMBOL]                                                       | [CAPABILITY]                                                          |
| :-----: | :------------------------------------------------------------- | :-------------------------------------------------------------------- |
|  [01]   | `EnergyPlusForwardTranslator`                                  | `translateModel(Model)` → `Workspace`; options `[01]-[E+FWD]`         |
|  [02]   | `EnergyPlusReverseTranslator`                                  | `translateWorkspace(Workspace, ProgressBar, bool clearLogSink)`       |
|  [03]   | `GbXMLForwardTranslator` / `GbXMLReverseTranslator`            | gbXML ↔ `Model` — the BIM/IFC bridge to other energy tools            |
|  [04]   | `SddForwardTranslator` / `SddReverseTranslator`                | CBECC SDD (California compliance) ↔ `Model`                           |
|  [05]   | `ThreeJSForwardTranslator` / `ThreeJSReverseTranslator`        | three.js geometry for web preview                                     |
|  [06]   | `GltfForwardTranslator`                                        | glTF geometry export for web preview                                  |
|  [07]   | `FloorplanJSForwardTranslator` / `FloorspaceReverseTranslator` | FloorspaceJS 2D floor-editor exchange                                 |
|  [08]   | `ISOModelForwardTranslator`                                    | the ISO 13790 monthly-model export                                    |
|  [09]   | `RadianceForwardTranslator`                                    | the Radiance daylight export                                          |
|  [10]   | `ContamForwardTranslator`                                      | the CONTAM airflow export                                             |
|  [11]   | `VersionTranslator`                                            | robust OSM loader — upgrades an older `.osm`; roster `[11]-[VERSION]` |

- [01]-[E+FWD]: `translateModelObject(ModelObject)` → `Workspace`; `forwardTranslatorOptions()`/`setForwardTranslatorOptions(ForwardTranslatorOptions)`; `setKeepRunControlSpecialDays`; `setIPTabularOutput`.
- [11]-[VERSION]: `loadModel(Path[, ProgressBar])` → `OptionalModel`; `loadModelFromString(string)` → `OptionalModel`; `loadComponent(Path)` → `OptionalComponent`; `originalVersion` → `VersionString`. It supersedes `Model.load` whenever the file version is not guaranteed current.

[PUBLIC_TYPE_SCOPE]: files, results, workflow
- package: `NREL.OpenStudio.macOS-arm64`
- namespace: `OpenStudio`
- rail: energy

| [INDEX] | [SYMBOL]                    | [CAPABILITY]                                                                                     |
| :-----: | :-------------------------- | :----------------------------------------------------------------------------------------------- |
|  [01]   | `EpwFile`                   | the EnergyPlus weather file (`EpwFile(Path)`); `WeatherFile`/`SimulationControl` context         |
|  [02]   | `SqlFile`                   | the EnergyPlus results SQLite (`SqlFile(Path)`); typed hourly/annual accessors — post-run reader |
|  [03]   | `WorkflowJSON` / `OSRunner` | the OpenStudio Workflow (OSW) + measure runner over a model                                      |
|  [04]   | `VersionString`             | a parsed OpenStudio version (`VersionTranslator.originalVersion` output)                         |

[PUBLIC_TYPE_SCOPE]: CLR enums
- package: `NREL.OpenStudio.macOS-arm64`
- namespace: `OpenStudio`
- rail: energy

Only nine true CLR enums exist; most OpenStudio "enumerations" are SWIG `*Enum`/`EnumBase` string-classes (e.g. `IddObjectType`), not CLR enums — match on their string value.

| [INDEX] | [SYMBOL]                                                  | [CAPABILITY]                                                     |
| :-----: | :-------------------------------------------------------- | :--------------------------------------------------------------- |
|  [01]   | `LogLevel`                                                | the `Logger` verbosity band (`Trace`…`Fatal`)                    |
|  [02]   | `FloatFormat`                                             | numeric formatting for IDF/text emit                             |
|  [03]   | `InterpMethod` / `ExtrapMethod`                           | interpolation/extrapolation policy for time-series/curve lookups |
|  [04]   | `ThreeSide` / `XMLValidatorType` / `ModelicaCompilerType` | three.js side culling, XML validator selection, Modelica target  |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: load / save a model
- package: `NREL.OpenStudio.macOS-arm64`
- namespace: `OpenStudio`
- rail: energy

Every load/get returning an `Optional<T>` gates on `is_initialized()` before `get()` (`[04]`).

| [INDEX] | [ENTRYPOINT]                                                                 | [CAPABILITY]                                    |
| :-----: | :--------------------------------------------------------------------------- | :---------------------------------------------- |
|  [01]   | `VersionTranslator.loadModel(Path pathToOldOsm)` → `OptionalModel`           | robust `.osm` read; upgrades to the SDK version |
|  [02]   | `Model.load(Path osmPath, Path workflowJSONPath)` (static) → `OptionalModel` | direct load when the file is current            |
|  [03]   | `model.save(Path, bool overwrite)` → `bool`                                  | persist to `.osm` (inherited from `Workspace`)  |
|  [04]   | `IdfFile.load(Path, IddFileType)` (static) → `OptionalIdfFile`               | read an EnergyPlus IDF into the workspace       |

[ENTRYPOINT_SCOPE]: translate
- package: `NREL.OpenStudio.macOS-arm64`
- namespace: `OpenStudio`
- rail: energy

`EnergyPlusReverseTranslator.translateWorkspace` takes `(Workspace, ProgressBar, bool clearLogSink)` and returns a `Model`.

| [INDEX] | [ENTRYPOINT]                                                               | [CAPABILITY]                                       |
| :-----: | :------------------------------------------------------------------------- | :------------------------------------------------- |
|  [01]   | `new EnergyPlusForwardTranslator().translateModel(Model)` → `Workspace`    | OSM → EnergyPlus IDF; then `errors()`/`warnings()` |
|  [02]   | `new EnergyPlusReverseTranslator().translateWorkspace(…)`                  | EnergyPlus IDF → OSM                               |
|  [03]   | `new GbXMLReverseTranslator().loadModel(Path gbXmlPath)` → `OptionalModel` | gbXML → OSM (the BIM/IFC bridge)                   |
|  [04]   | `new SqlFile(Path)` + typed getters                                        | read the EnergyPlus results SQLite after a run     |

## [04]-[IMPLEMENTATION_LAW]

[NATIVE_BOUNDARY]:
- this is a SWIG binding over a bundled C++ runtime; the managed types are thin wrappers around native handles. `build/OpenStudio.targets` stages `libopenstudiolib.dylib` and the three `libopenstudio_*_csharp.dylib` shims next to the consumer output — they MUST ship with the app, and the package is RID-locked to `osx-arm64`. A Windows/Linux build admits the sibling per-RID package; never assume one runtime asset is portable.
- the `*PINVOKE` classes are the DllImport surface into the native shims; they are an implementation detail, not a call surface — drive the public `OpenStudio.*` wrappers only.

[DISPOSAL_AND_THREADING]:
- every wrapper owns native memory (`cMemoryOwn`); wrap `Model`, every translator, `SqlFile`, `EpwFile`, and the `Optional*`/`*Vector` results in `using` or call `Dispose()` — a dropped handle leaks native memory the GC cannot reclaim deterministically. A `*Vector` returned from a getter is itself disposable.
- the native SDK is not thread-safe for concurrent model mutation; serialize access to one `Model` (one logical owner per model) and run translators sequentially. This is the single-threaded-native boundary the Rasm concurrency rail brackets — offload a translation as one unit of work, not a parallel fan-out over shared model state.

[OPTIONAL_DISCIPLINE]:
- any load/get that can miss returns an `Optional<T>`; the law is `is_initialized()` (or `!isNull()`) THEN `get()`, or `value_or(default)`. A bare `get()` on an empty optional faults in native code. Lower the `Optional<T>` onto a `Fin<T>`/`Option<T>` at the Exchange boundary so internal code never sees the SWIG optional.
- `Path` is mandatory at every file API: build it with `OpenStudioUtilitiesCore.toPath(string)` (there is no `Path(string)` ctor) and read it back with `OpenStudioUtilitiesCore.toString(path)`; passing a raw `string` does not compile against the file overloads.

[STACK_INTEGRATION]:
- Honeybee seam: `HoneybeeSchema` (`api-honeybee-schema`) is the authored HBJSON model; OpenStudio is the EnergyPlus runtime model. They meet at gbXML (`GbXMLReverseTranslator.loadModel`) and the canonical Bim energy model; the full HBJSON->OSM path is the external `honeybee-openstudio` Python step, with OpenStudio owning the OSM/IDF runtime side.
- IFC seam: a GeometryGym IFC building (`api-geometrygym-ifc`) exports gbXML, which `GbXMLReverseTranslator` ingests into a `Model` — IFC spaces/zones become OSM spaces/thermal zones at the `Exchange/import` boundary; the energy model is derived from the BIM model, never re-authored.
- geometry-export seam: `GltfForwardTranslator`/`ThreeJSForwardTranslator` emit a model's geometry for web preview, joining the same `Exchange/export` delivery rail as the glTF/3D-Tiles legs (`api-sharpgltf-3dtiles`) — OpenStudio is the energy source, those are the delivery encodings.
- results seam: after an EnergyPlus run, `SqlFile` reads the results SQLite; the typed outputs land in the Bim analysis receipts and, keyed by content hash, in the `Rasm.Persistence` artifact index.
- identity seam: a saved `.osm`/IDF string (UTF-8 bytes) feeds `System.IO.Hashing` — `XxHash3` for the fast in-process fingerprint, `XxHash128` for the collision-resistant persisted content key (`api-hashing`) — the same content-identity rail (the `Rasm.Persistence` artifact index) as the other energy/IFC exports.

[LOCAL_ADMISSION]:
- model read enters through `new VersionTranslator.loadModel(path)` (version-robust) returning an `OptionalModel` lowered to a `Fin<Model>`; model write enters through `model.save(path, overwrite)`.
- translation enters through the matching `*Translator` instance under a `using`, with `errors()`/`warnings()` captured as a typed receipt; the rejected forms are calling `get()` on an unchecked optional, passing a raw `string` where a `Path` is required, leaking native handles by skipping `Dispose`, and assuming the osx-arm64 runtime on another platform.

[RAIL_LAW]:
- Package: `NREL.OpenStudio.macOS-arm64` (assembly `OpenStudio`)
- Owns: the OpenStudio OSM model + EnergyPlus IDF/IDD object store, the translator matrix (EnergyPlus/gbXML/SDD/three.js/glTF/FloorspaceJS/ISO/Radiance/CONTAM forward+reverse), the `VersionTranslator` robust loader, the weather/results/workflow files, and the SWIG native-handle marshaling
- Accept: OSM load/save/version-upgrade, model<->EnergyPlus/gbXML/SDD translation, EnergyPlus results reading, the osx-arm64 native-runtime energy rail
- Reject: HBJSON authoring (`HoneybeeSchema` owns it), IFC semantics (GeometryGym owns it), running the EnergyPlus solver itself (the external EnergyPlus binary does — this binding produces its input and reads its output), cross-platform native portability (each RID admits its own package), and leaking `OpenStudio.*` SWIG handles past the Exchange boundary

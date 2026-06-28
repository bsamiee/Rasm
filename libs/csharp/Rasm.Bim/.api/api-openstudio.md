# [RASM_BIM_API_OPENSTUDIO]

`NREL.OpenStudio.macOS-arm64` is the osx-arm64 SWIG-generated C# binding to the NREL
OpenStudio SDK — the whole-building-energy-modeling engine that owns the OSM model, the
EnergyPlus IDF/IDD object layer, and the translator matrix to and from EnergyPlus, gbXML,
SDD, three.js/glTF, FloorspaceJS, ISO, Radiance, and CONTAM. The managed `OpenStudio.dll`
(5088 generated types, namespace `OpenStudio`) marshals into a bundled native runtime
(`libopenstudiolib.dylib` + the three `libopenstudio_*_csharp.dylib` SWIG shims); every
wrapper owns a native handle and is `IDisposable`. It is the EnergyPlus leg of the Bim
energy-model exchange owner — the OSM/IDF runtime sibling to the `HoneybeeSchema`
(`api-honeybee-schema`) HBJSON authoring leg, meeting it at gbXML and the canonical Bim
energy model.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `NREL.OpenStudio.macOS-arm64`
- package: `NREL.OpenStudio.macOS-arm64`
- version: `3.11.0`
- license: NREL OpenStudio License (the `LICENSE.md` at `github.com/NREL/OpenStudio` — a permissive BSD-3-Clause-derived license; the nuspec carries a `licenseUrl`, no SPDX expression)
- assembly: `OpenStudio` (`lib/netstandard2.0/OpenStudio.dll`, the managed SWIG wrapper)
- namespace: `OpenStudio` (4179 model/translator/utility types) + ~80 `OpenStudio*PINVOKE` marshaling classes (one per SWIG module — `OpenStudioModelPINVOKE`, `OpenStudioEnergyPlusPINVOKE`, `OpenStudioOSVersionPINVOKE`, `OpenStudioGBXMLPINVOKE`, …)
- asset: managed binding TFM `netstandard2.0` (binds forward under net10.0); the runtime is RID-LOCKED to `osx-arm64`
- native: `runtimes/osx-arm64/native/libopenstudiolib.dylib` (the SDK) + `libopenstudio_csharp.dylib` / `libopenstudio_model_csharp.dylib` / `libopenstudio_translators_csharp.dylib` (the SWIG P/Invoke shims); `build/OpenStudio.targets` copies the dylibs next to the consumer output
- platform: this package is the macOS-arm64 RID member of a per-RID family — a Windows/Linux host admits the sibling `NREL.OpenStudio.win-x64` / `NREL.OpenStudio.linux-x64` package; the managed API is identical, only the native runtime differs
- dependency: empty net-standard dependency group (the native runtime is fully bundled; no managed transitive deps)
- consumer: `libs/csharp/Rasm.Bim` (the OSM/EnergyPlus energy-model exchange owner)
- rail: energy

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: SWIG marshaling primitives
- package: `NREL.OpenStudio.macOS-arm64`
- namespace: `OpenStudio`
- rail: energy

Every wrapper holds a native `HandleRef` (`swigCPtr`) with a `cMemoryOwn` flag and implements
`IDisposable`; the C++ STL/Boost shapes surface as the families below, not as BCL collections.

| [INDEX] | [SYMBOL]                  | [RAIL]  | [CAPABILITY]                                                                                              |
| :-----: | :------------------------ | :------ | :------------------------------------------------------------------------------------------------------- |
|  [01]   | `Optional<T>` family      | energy  | the `boost::optional` marshaling — `OptionalModel`/`OptionalWorkspace`/`OptionalIdfFile`/`OptionalDouble`/`OptionalPath`/`OptionalComponent`/`OptionalWorkspaceObject`/…; `is_initialized()`/`isNull()`, `get()`, `value_or(T)`, `set(T)`, `reset()`. Every "load"/"get" that can miss returns one — check `is_initialized()` before `get()` |
|  [02]   | `*Vector` family          | energy  | the `std::vector` marshaling — `ModelObjectVector`/`WorkspaceObjectVector`/`SurfaceVector`/`SpaceVector`/`StringVector`/`DoubleVector`/`LogMessageVector`/`UUIDVector`/…; `IList`-shaped (indexer, `Count`, `Add`), the carrier for every plural query result |
|  [03]   | `Path` / `OptionalPath`   | energy  | the `boost::filesystem::path` wrapper; there is NO `Path(string)` ctor (only `Path()`/`Path(Path)`) — build from a string with the static `OpenStudioUtilitiesCore.toPath(string)` and read back via `OpenStudioUtilitiesCore.toString(path)`. The REQUIRED carrier at every file API (a raw `string` is not accepted) |
|  [04]   | `UUID`                    | energy  | the model-object handle (`OpenStudioUtilitiesCore.createUUID()` / `toUUID(string)`); `WorkspaceObject` identity and the `getObject(UUID)` key |
|  [05]   | `OpenStudioUtilitiesCore` | energy  | the SWIG global-function host for the cross-cutting free functions (`toPath`/`toString`/`createUUID`/`toUUID`) — the static utility class the file/identity boundaries route through, distinct from the per-module `*PINVOKE` shims |
|  [06]   | `Logger` / `LogMessageVector` | energy | the native log sink; `LogLevel` (enum) sets verbosity, translator `warnings()`/`errors()` return `LogMessageVector` — route these into the Rasm telemetry rail rather than the console |

[PUBLIC_TYPE_SCOPE]: model + IDF/IDD object store
- package: `NREL.OpenStudio.macOS-arm64`
- namespace: `OpenStudio`
- rail: energy

| [INDEX] | [SYMBOL]               | [RAIL]  | [CAPABILITY]                                                                                              |
| :-----: | :--------------------- | :------ | :------------------------------------------------------------------------------------------------------- |
|  [01]   | `Model`                | energy  | the OSM model (`: Workspace`); ctors `Model()` / `Model(IdfFile)` / `Model(Workspace)` / `Model(Model)`, static `load(Path osmPath, Path workflowJSONPath)` -> `OptionalModel`, `modelObjects(bool sorted = …)` -> `ModelObjectVector`, `getModelObjectLists*`, `save` (inherited from `Workspace`). The root every model-object getter hangs off |
|  [02]   | `Workspace`            | energy  | the generic IDD-object store backing both `Model` and an IDF; `getObjectsByType(IddObjectType)` -> `WorkspaceObjectVector`, `getObjectByTypeAndName(IddObjectType, name)` -> `OptionalWorkspaceObject`, `getObject(UUID)`, `objects(bool sorted)`, `versionObject()`, `save(Path, bool overwrite)` |
|  [03]   | `IdfFile` / `OptionalIdfFile` | energy | the EnergyPlus IDF text model (`IdfFile.load(Path, IddFileType)` -> `OptionalIdfFile`); the input EnergyPlus consumes and `EnergyPlusForwardTranslator` produces |
|  [04]   | `IddFile` / `Idd` / `IddObjectType` | energy | the EnergyPlus Input Data Dictionary (the object schema); `IddObjectType` is the typed key `Workspace.getObjectsByType` filters on |
|  [05]   | `ModelObject`          | energy  | the base of every model object (surfaces, spaces, zones, constructions, schedules, HVAC); the `ModelObjectVector` element and the `translateModelObject` input |
|  [06]   | `Building` / `Space` / `Surface` / `ThermalZone` / `Construction` / `Schedule` | energy | representative `ModelObject` leaves — the geometry/thermal/constructive objects a model carries (the full set is the 4000+ generated `OpenStudio.*` model types) |

[PUBLIC_TYPE_SCOPE]: translators (the exchange matrix)
- package: `NREL.OpenStudio.macOS-arm64`
- namespace: `OpenStudio`
- rail: energy

Each translator is `IDisposable`, exposes `warnings()`/`errors()` -> `LogMessageVector`, and
takes an optional `ProgressBar`. "Forward" = `Model` -> external format; "Reverse" = external
format -> `Model`/`OptionalModel`.

| [INDEX] | [SYMBOL]                              | [RAIL]  | [CAPABILITY]                                                                                              |
| :-----: | :------------------------------------ | :------ | :------------------------------------------------------------------------------------------------------- |
|  [01]   | `EnergyPlusForwardTranslator`         | energy  | `translateModel(Model[, ProgressBar])` -> `Workspace` (the IDF for EnergyPlus); `translateModelObject(ModelObject)` -> `Workspace`; `forwardTranslatorOptions()`/`setForwardTranslatorOptions(ForwardTranslatorOptions)`, `setKeepRunControlSpecialDays`, `setIPTabularOutput` |
|  [02]   | `EnergyPlusReverseTranslator`         | energy  | `translateWorkspace(Workspace, ProgressBar, bool clearLogSink)` -> `Model` — bring an existing IDF back into the OSM model |
|  [03]   | `GbXMLForwardTranslator` / `GbXMLReverseTranslator` | energy | gbXML <-> `Model`; `GbXMLReverseTranslator.loadModel(Path[, ProgressBar])` -> `OptionalModel` — the language-neutral bridge to the BIM/IFC side and to other energy tools |
|  [04]   | `SddForwardTranslator` / `SddReverseTranslator` | energy | CBECC SDD (California compliance) <-> `Model` |
|  [05]   | `ThreeJSForwardTranslator` / `ThreeJSReverseTranslator` / `GltfForwardTranslator` / `FloorplanJSForwardTranslator` / `FloorspaceReverseTranslator` | energy | geometry exchange — three.js/glTF for web preview, FloorspaceJS for the 2D floor editor |
|  [06]   | `ISOModelForwardTranslator` / `RadianceForwardTranslator` / `ContamForwardTranslator` | energy | the ISO 13790 monthly model, the Radiance daylight export, and the CONTAM airflow export |
|  [07]   | `VersionTranslator`                   | energy  | the robust OSM reader: `loadModel(Path[, ProgressBar])` -> `OptionalModel`, `loadModelFromString(string)` -> `OptionalModel`, `loadComponent(Path)` -> `OptionalComponent`, `originalVersion()` -> `VersionString`. Upgrades an older `.osm` to the SDK version on load — use it over `Model.load` whenever the file version is not guaranteed current |

[PUBLIC_TYPE_SCOPE]: files, results, workflow
- package: `NREL.OpenStudio.macOS-arm64`
- namespace: `OpenStudio`
- rail: energy

| [INDEX] | [SYMBOL]               | [RAIL]  | [CAPABILITY]                                                                                              |
| :-----: | :--------------------- | :------ | :------------------------------------------------------------------------------------------------------- |
|  [01]   | `EpwFile`              | energy  | the EnergyPlus weather file (`EpwFile(Path)`); the site/design-day context attached to `WeatherFile`/`SimulationControl` |
|  [02]   | `SqlFile`              | energy  | the EnergyPlus results SQLite (`SqlFile(Path)`); typed accessors for hourly/annual outputs — the post-run results reader |
|  [03]   | `WorkflowJSON` / `OSRunner` | energy | the OpenStudio Workflow (OSW) + measure runner — drive parametric measure chains over a model |
|  [04]   | `VersionString`        | energy  | a parsed semantic OpenStudio version (`VersionTranslator.originalVersion()` output) |

[PUBLIC_TYPE_SCOPE]: CLR enums
- package: `NREL.OpenStudio.macOS-arm64`
- namespace: `OpenStudio`
- rail: energy

Only nine true CLR enums exist; most OpenStudio "enumerations" are SWIG `*Enum`/`EnumBase`
string-classes (e.g. `IddObjectType`), not CLR enums — match on their string value.

| [INDEX] | [SYMBOL]                  | [RAIL]  | [CAPABILITY]                                                                                              |
| :-----: | :------------------------ | :------ | :------------------------------------------------------------------------------------------------------- |
|  [01]   | `LogLevel`                | energy  | the `Logger` verbosity band (`Trace`…`Fatal`) |
|  [02]   | `FloatFormat`             | energy  | numeric formatting for IDF/text emit |
|  [03]   | `InterpMethod` / `ExtrapMethod` | energy | the interpolation/extrapolation policy for time-series/curve lookups |
|  [04]   | `ThreeSide` / `XMLValidatorType` / `ModelicaCompilerType` | energy | three.js side culling, XML validator selection, and the Modelica compiler target |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: load / save a model
- package: `NREL.OpenStudio.macOS-arm64`
- namespace: `OpenStudio`
- rail: energy

| [INDEX] | [SURFACE]                          | [CALL_SHAPE]                                                                 | [CAPABILITY]                                                  |
| :-----: | :--------------------------------- | :-------------------------------------------------------------------------- | :----------------------------------------------------------- |
|  [01]   | `new VersionTranslator().loadModel` | `(Path pathToOldOsm)` -> `OptionalModel`                                    | the robust `.osm` read — upgrades to the SDK version; check `is_initialized()` then `get()` |
|  [02]   | `Model.load`                       | `(Path osmPath, Path workflowJSONPath)` (static) -> `OptionalModel`         | direct load when the file is known to be the current version |
|  [03]   | `model.save`                       | `(Path, bool overwrite)` -> `bool` (inherited from `Workspace`)             | persist the model to `.osm` |
|  [04]   | `IdfFile.load`                     | `(Path, IddFileType)` (static) -> `OptionalIdfFile`                         | read an EnergyPlus IDF into the workspace layer |

[ENTRYPOINT_SCOPE]: translate
- package: `NREL.OpenStudio.macOS-arm64`
- namespace: `OpenStudio`
- rail: energy

| [INDEX] | [SURFACE]                                       | [CALL_SHAPE]                                                                 | [CAPABILITY]                                                  |
| :-----: | :---------------------------------------------- | :-------------------------------------------------------------------------- | :----------------------------------------------------------- |
|  [01]   | `new EnergyPlusForwardTranslator().translateModel` | `(Model model)` -> `Workspace`                                              | OSM -> EnergyPlus IDF; then `errors()`/`warnings()` for the receipt |
|  [02]   | `new EnergyPlusReverseTranslator().translateWorkspace` | `(Workspace, ProgressBar, bool clearLogSink)` -> `Model`               | EnergyPlus IDF -> OSM |
|  [03]   | `new GbXMLReverseTranslator().loadModel`        | `(Path gbXmlPath)` -> `OptionalModel`                                       | gbXML -> OSM (the BIM/IFC bridge) |
|  [04]   | `new SqlFile(path)` accessors                   | `(Path)` then typed result getters                                         | read the EnergyPlus results SQLite after a run |

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
- identity seam: a saved `.osm`/IDF string (UTF-8 bytes) feeds `System.IO.Hashing` `XxHash3` (`api-hashing`) for the model snapshot content key, the same content-identity rail as the other energy/IFC exports.

[LOCAL_ADMISSION]:
- model read enters through `new VersionTranslator().loadModel(path)` (version-robust) returning an `OptionalModel` lowered to a `Fin<Model>`; model write enters through `model.save(path, overwrite)`.
- translation enters through the matching `*Translator` instance under a `using`, with `errors()`/`warnings()` captured as a typed receipt; the rejected forms are calling `get()` on an unchecked optional, passing a raw `string` where a `Path` is required, leaking native handles by skipping `Dispose`, and assuming the osx-arm64 runtime on another platform.

[RAIL_LAW]:
- Package: `NREL.OpenStudio.macOS-arm64` (assembly `OpenStudio`)
- Owns: the OpenStudio OSM model + EnergyPlus IDF/IDD object store, the translator matrix (EnergyPlus/gbXML/SDD/three.js/glTF/FloorspaceJS/ISO/Radiance/CONTAM forward+reverse), the `VersionTranslator` robust loader, the weather/results/workflow files, and the SWIG native-handle marshaling
- Accept: OSM load/save/version-upgrade, model<->EnergyPlus/gbXML/SDD translation, EnergyPlus results reading, the osx-arm64 native-runtime energy rail
- Reject: HBJSON authoring (`HoneybeeSchema` owns it), IFC semantics (GeometryGym owns it), running the EnergyPlus solver itself (the external EnergyPlus binary does — this binding produces its input and reads its output), cross-platform native portability (each RID admits its own package), and leaking `OpenStudio.*` SWIG handles past the Exchange boundary

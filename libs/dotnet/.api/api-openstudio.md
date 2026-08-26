# [RASM_API_OPENSTUDIO]

`NREL.OpenStudio.macOS-arm64` is the osx-arm64 SWIG-generated C# binding to the NREL OpenStudio SDK: it owns the OSM `Model`, the EnergyPlus IDF/IDD object store, and the forward/reverse translator matrix between a `Model` and the neutral energy and geometry formats. `OpenStudio.dll` marshals a bundled RID-locked native runtime, and every managed wrapper holds a native handle under `IDisposable`. Two folders own disjoint legs of one binding: `Rasm.Bim` drives the OSM/IDF exchange leg — load, save, version-upgrade, and the gbXML/SDD semantic bridges meeting the `HoneybeeSchema` HBJSON authoring leg — and `Rasm.Compute` drives the `Analysis/energy` simulation lane, building a `Model` in-process from the `Rasm.Element` `ElementGraph`, forward-translating to an EnergyPlus IDF `Workspace`, and reading the post-run `SqlFile`. The EnergyPlus solver runs as a parameterized subprocess, never inside the binding.

## [01]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: SWIG marshaling primitives — the native boundary

Every wrapper holds a native `HandleRef` (`swigCPtr`) with a `cMemoryOwn` flag under `IDisposable`; the C++ STL/Boost shapes surface as the families below, lowered onto `Fin<T>`/`Option<T>` at each consuming boundary so interior code never sees a SWIG type.

| [INDEX] | [SYMBOL]                      | [TYPE_FAMILY] | [CAPABILITY]                                             |
| :-----: | :---------------------------- | :------------ | :------------------------------------------------------- |
|  [01]   | `Optional<T>` family          | class         | `boost::optional` may-miss carrier; gate then unwrap     |
|  [02]   | `OptionalDouble`              | class         | the missing-double every `SqlFile`/`EpwFile` read yields |
|  [03]   | `*Vector` family              | class         | `std::vector` marshaling, `IList`-shaped plural carrier  |
|  [04]   | `Path` / `OptionalPath`       | class         | `boost::filesystem::path`; no `Path(string)` ctor        |
|  [05]   | `Point3d`                     | class         | the surface-vertex value                                 |
|  [06]   | `UUID`                        | class         | object identity handle, the `getObject(UUID)` key        |
|  [07]   | `OpenStudioUtilitiesCore`     | class         | SWIG global-function host for path and UUID construction |
|  [08]   | `Logger` / `LogMessageVector` | class         | native log sink; the translator diagnostics carrier      |
|  [09]   | `ProgressBar`                 | class         | SWIG director delivering native progress callbacks       |

- `Optional<T>`: `is_initialized()` `isNull()` `get()` `value_or(T)` `set(T)` `reset()`; `OptionalModel`/`OptionalWorkspace`/`OptionalConstruction` and siblings.
- `*Vector`: `Point3dVector : IEnumerable<Point3d>` (surface vertices), `MaterialVector : IEnumerable<Material>` (construction layers), `LogMessageVector` (translator sink), `SurfaceVector`/`SpaceVector`; a getter-returned vector is itself `IDisposable`.
- `OpenStudioUtilitiesCore`: `toPath(string)` `toString(Path)` `createUUID()` `toUUID(string)`.
- `ProgressBar`: `class ProgressBar : IDisposable` with a `protected ProgressBar()` ctor and a `virtual onPercentageUpdated(double)` override sink, so every subclass instance brackets `using` like the rest of the SWIG surface. Its whole virtual surface is `minimum`/`setMinimum`/`maximum`/`setMaximum`/`value`/`setValue`/`setRange`/`windowTitle`/`setWindowTitle`/`text`/`isVisible`/`setVisible` beside the percentage sink — NO abort, cancel, or interrupt member exists, so a running translator runs to completion and the director callback is the finest point a managed token READS while native code runs.

[PUBLIC_TYPE_SCOPE]: model and IDF/IDD object store

| [INDEX] | [SYMBOL]                            | [TYPE_FAMILY] | [CAPABILITY]                                                 |
| :-----: | :---------------------------------- | :------------ | :----------------------------------------------------------- |
|  [01]   | `Model`                             | class         | the OSM model (`: Workspace`), the model-object getter root  |
|  [02]   | `Workspace`                         | class         | the generic IDD-object store backing `Model` and an IDF      |
|  [03]   | `IdfFile` / `OptionalIdfFile`       | class         | the EnergyPlus IDF text model                                |
|  [04]   | `IddFile` / `Idd` / `IddObjectType` | class         | the Input Data Dictionary; `IddObjectType` keys the type get |
|  [05]   | `ModelObject`                       | class         | base of every model object, the `translateModelObject` input |
|  [06]   | leaf `ModelObject`s                 | class         | `Building` `Space` `Surface` `SubSurface` `ThermalZone` …    |

- `Model`: ctors `Model()` `Model(IdfFile)` `Model(Workspace)` `Model(Model)`; `modelObjects(bool sorted)` → `ModelObjectVector`; load and save are `[03]`'s.
- `Workspace`: `getObjectsByType(IddObjectType)`/`(IddObject)`/`(string)` → `WorkspaceObjectVector`; `getObjectByTypeAndName(IddObjectType, string)`/`(string, string)` → `OptionalWorkspaceObject`; `getObject(UUID)`; `getObjects(UUIDVector)`; `objects(bool sorted)`; `toIdfFile()`; `save(Path, bool overwrite)` and `save(Path)`. `Model : Workspace`, so an OSM write is `model.save(path, overwrite)`.

[PUBLIC_TYPE_SCOPE]: the handle-keyed typed getters — the ONE downcast path off a statically-based vector element

Every `*Vector` element is typed to its family base (`Material`, `ConstructionBase`), so the concrete leaf is reached by re-reading the element's `handle()` through the model's own typed getter, never a managed cast. Each returns an `Optional*` gated `is_initialized()` then `get()`.

| [INDEX] | [SURFACE]                                                                  | [CAPABILITY]                                         |
| :-----: | :------------------------------------------------------------------------- | :--------------------------------------------------- |
|  [01]   | `Model.getConstruction(UUID)` → `OptionalConstruction`                     | the layered construction behind a `ConstructionBase` |
|  [02]   | `Model.getStandardOpaqueMaterial(UUID)` → `OptionalStandardOpaqueMaterial` | the opaque leaf behind a `Material`                  |
|  [03]   | `Model.getStandardGlazing(UUID)` → `OptionalStandardGlazing`               | the glazing leaf behind a `Material`                 |
|  [04]   | `Model.get*ByName(string)` / `get*sByName(string, bool)`                   | the name-keyed siblings of each typed getter         |
|  [05]   | `Model.getSpaces()` → `SpaceVector`                                        | every space, the raise fold's root read              |

[PUBLIC_TYPE_SCOPE]: geometry and grouping leaves the energy raise reads

| [INDEX] | [SYMBOL]        | [CAPABILITY]                                                                                     |
| :-----: | :-------------- | :----------------------------------------------------------------------------------------------- |
|  [01]   | `Space`         | `surfaces` (PROPERTY → `SurfaceVector`), `buildingStory()`, `thermalZone()`, `multiplier()`      |
|  [02]   | `Surface`       | `surfaceType()` → `string`, `subSurfaces()` → `SubSurfaceVector`, `vertices()`, `construction()` |
|  [03]   | `SubSurface`    | `subSurfaceType()` → `string`, `validSubSurfaceTypeValues()` → `StringVector`, `vertices()`      |
|  [04]   | `BuildingStory` | `nameString()`, `spaces()`, `nominalZCoordinate()`, `nominalFloortoCeilingHeight()`              |
|  [05]   | `ThermalZone`   | `nameString()`, `spaces()`, `multiplier()`                                                       |
|  [06]   | `PlanarSurface` | the `Surface`/`SubSurface` base carrying `construction()` → `OptionalConstructionBase`           |

- `Space.surfaces` is a PROPERTY, not a method — the one shape break in an otherwise method-shaped surface.
- `SubSurface.validSubSurfaceTypeValues()` is the closed roster an opening-type map keys on; `isSubSurfaceTypeDefaulted()` distinguishes an authored token from the assigned default.

[PUBLIC_TYPE_SCOPE]: material leaves — the `OptionalDouble`-versus-bare getter split

`StandardOpaqueMaterial` publishes every thermal input as a BARE `double`. `StandardGlazing` splits: the six normal-incidence transmittance and reflectance fractions return `OptionalDouble` — the IDD field is genuinely unset-able, and the plain `solarTransmittance()`/`visibleTransmittance()` siblings THROW over that same unset field — while the infrared trio, `conductivity()`, and `thickness()` return bare `double`s under IDD defaults.

| [INDEX] | [SURFACE]                                                                                       | [SHAPE]          |
| :-----: | :---------------------------------------------------------------------------------------------- | :--------------- |
|  [01]   | `StandardGlazing.solarTransmittanceatNormalIncidence()`                                         | `OptionalDouble` |
|  [02]   | `StandardGlazing.frontSideSolarReflectanceatNormalIncidence()` / `backSide…`                    | `OptionalDouble` |
|  [03]   | `StandardGlazing.visibleTransmittanceatNormalIncidence()`                                       | `OptionalDouble` |
|  [04]   | `StandardGlazing.frontSideVisibleReflectanceatNormalIncidence()` / `backSide…`                  | `OptionalDouble` |
|  [05]   | `StandardGlazing.infraredTransmittanceatNormalIncidence()` / the two hemispherical emissivities | `double`         |
|  [06]   | `StandardGlazing.conductivity()` / `thickness()`                                                | `double`         |
|  [07]   | `StandardOpaqueMaterial.conductivity()` / `density()` / `specificHeat()` / `thickness()`        | `double`         |
|  [08]   | `ConstructionBase.uFactor()`                                                                    | `OptionalDouble` |

[PUBLIC_TYPE_SCOPE]: model build — constructed from the element graph

Model-object construction reads the `Rasm.Element` `ElementGraph`; each object is `new`-ed against its owning `Model` and lives by the model's lifetime, so only the top-level `Model`, translator, and file/optional/vector handles are bracketed.

| [INDEX] | [SYMBOL]                                          | [TYPE_FAMILY] | [CAPABILITY]                                     |
| :-----: | :------------------------------------------------ | :------------ | :----------------------------------------------- |
|  [01]   | `Space` / `Building`                              | class         | the conditioned volume; the model root container |
|  [02]   | `Surface`                                         | class         | the bounding surface per Object-node face        |
|  [03]   | `ThermalZone`                                     | class         | the EnergyPlus thermal-balance unit              |
|  [04]   | `Construction`                                    | class         | the ordered layered-material stack               |
|  [05]   | `StandardOpaqueMaterial` / `StandardGlazing`      | class         | the opaque and glazing construction layers       |
|  [06]   | `WeatherFile` / `SimulationControl` / `RunPeriod` | class         | site context, annual run period, control flags   |

- `Space`: `: PlanarSurfaceGroup`; each spatial Object node folds into one, assigned to a `ThermalZone : HVACComponent`.
- `Construction`: `: LayeredConstruction`; `setLayers(MaterialVector)` orders the stack the shared `MaterialComposition.LayerSet` lowers to.
- `StandardOpaqueMaterial`: `: OpaqueMaterial`; the ctor's thermal args feed the same shared properties the `Analysis/aggregator` ISO 6946 U-value fold reads.

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
|  [03]   | `IddFileType`               | class         | SWIG `EnumBase` string-class — the IDD schema-key selector  |
|  [04]   | `WorkflowJSON` / `OSRunner` | class         | the OpenStudio Workflow (OSW) and measure runner            |
|  [05]   | `VersionString`             | class         | a parsed OpenStudio version, the `VersionTranslator` output |

- `EpwFile`: site accessors `latitude()`/`longitude()`/`timeZone()`/`elevation()` → `double` and `data()` → the `EpwDataPointVector` whose rows carry `directNormalRadiation()`/`diffuseHorizontalRadiation()`/`globalHorizontalRadiation()` → `OptionalDouble`, the `Analysis/daylight` `WeatherIngress` reader.
- `SqlFile`: annual `totalSiteEnergy()`/`netSiteEnergy()`/`totalSourceEnergy()`/`netSourceEnergy()`, per-end-use `electricityHeating()`/`electricityCooling()`/`electricityInteriorLighting()`/`electricityTotalEndUses()`/`naturalGasHeating()`, and per-month `energyConsumptionByMonth`/`peakEnergyDemandByMonth` all return `OptionalDouble`.
- `LogMessageVector`: collects the translator `warnings()`/`errors()`, folded into each boundary result as provider diagnostics, never the console.

[PUBLIC_TYPE_SCOPE]: CLR enums

Only these types are true CLR enums; most OpenStudio "enumerations" are SWIG `*Enum`/`EnumBase` string-classes such as `IddObjectType` and `IddFileType`, matched on string value.

| [INDEX] | [SYMBOL]                                                  | [TYPE_FAMILY] | [CAPABILITY]                                           |
| :-----: | :-------------------------------------------------------- | :------------ | :----------------------------------------------------- |
|  [01]   | `LogLevel`                                                | enum          | the `Logger` verbosity band (`Trace`…`Fatal`)          |
|  [02]   | `FloatFormat`                                             | enum          | numeric formatting for IDF/text emit                   |
|  [03]   | `InterpMethod` / `ExtrapMethod`                           | enum          | interp/extrap policy for time-series and curve lookups |
|  [04]   | `ThreeSide` / `XMLValidatorType` / `ModelicaCompilerType` | enum          | three.js side culling, XML validator, Modelica target  |

## [02]-[ENTRYPOINTS]

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

[ENTRYPOINT_SCOPE]: build the model from the element graph

| [INDEX] | [SURFACE]                                             | [SHAPE]  | [CAPABILITY]                                      |
| :-----: | :---------------------------------------------------- | :------- | :------------------------------------------------ |
|  [01]   | `new Model() -> Model`                                | ctor     | the empty OSM model the graph folds into          |
|  [02]   | `new Space(Model)`                                    | ctor     | a conditioned volume per spatial Object node      |
|  [03]   | `new ThermalZone(Model)`                              | ctor     | a thermal-balance zone each `Space` assigns to    |
|  [04]   | `new Surface(Point3dVector, Model)`                   | ctor     | a bounding surface from Object-node face vertices |
|  [05]   | `new Construction(Model)`                             | ctor     | the layered-construction owner                    |
|  [06]   | `Construction.setLayers(MaterialVector) -> bool`      | instance | order the layered-material stack                  |
|  [07]   | `new StandardOpaqueMaterial(Model, string, double×4)` | ctor     | an opaque layer from shared thermal properties    |

- `new StandardOpaqueMaterial`: orders args `roughness, thickness, conductivity, density, specificHeat`; shorter overloads default the trailing scalars.

[ENTRYPOINT_SCOPE]: forward-translate and write the IDF

| [INDEX] | [SURFACE]                                                              | [SHAPE]  | [CAPABILITY]                                 |
| :-----: | :--------------------------------------------------------------------- | :------- | :------------------------------------------- |
|  [01]   | `new EnergyPlusForwardTranslator().translateModel(Model) -> Workspace` | instance | OSM → the EnergyPlus IDF                     |
|  [02]   | `translator.warnings() / errors() -> LogMessageVector`                 | instance | the translation diagnostics                  |
|  [03]   | `Workspace.save(Path, bool) -> bool`                                   | instance | persist the IDF to the scratch run directory |
|  [04]   | `new EpwFile(Path) -> EpwFile`                                         | ctor     | the weather file passed to the subprocess    |

[ENTRYPOINT_SCOPE]: read the results

EnergyPlus runs as the `EnergyToolchain`-resolved subprocess (`energyplus -w <epw> -d <out> -r <idf>`), not through this binding; the runner opens `eplusout.sql` after it exits and derives EUI as annual total energy ÷ conditioned floor area. Every `sql.*` accessor returns `OptionalDouble`.

| [INDEX] | [SURFACE]                                                                       | [SHAPE]  | [CAPABILITY]                         |
| :-----: | :------------------------------------------------------------------------------ | :------- | :----------------------------------- |
|  [01]   | `new SqlFile(Path) -> SqlFile`                                                  | ctor     | open `eplusout.sql` after the run    |
|  [02]   | `sql.totalSiteEnergy()`                                                         | instance | annual site/source energy            |
|  [03]   | `sql.electricityHeating() / electricityCooling()`                               | instance | per-end-use annual demand            |
|  [04]   | `sql.energyConsumptionByMonth(EndUseFuelType, EndUseCategoryType, MonthOfYear)` | instance | per-month consumption                |
|  [05]   | `sql.peakEnergyDemandByMonth(EndUseFuelType, EndUseCategoryType, MonthOfYear)`  | instance | per-month peak demand (W)            |
|  [06]   | `new MonthOfYear(int) / new MonthOfYear(string)`                                | ctor     | ordinal and named month construction |

## [03]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Every managed type is a thin wrapper around a native handle (`HandleRef swigCPtr` with a `cMemoryOwn` flag) under `IDisposable`; drive the public `OpenStudio.*` wrappers, never the `OpenStudio*PINVOKE` DllImport classes. `build/OpenStudio.targets` stages the native dylibs next to consumer output, RID-locked to `osx-arm64`.
- Bracket the `Model`, each translator, `Workspace`, `SqlFile`, `EpwFile`, and every `Optional*`/`*Vector` result under `using` or `Dispose()`; a dropped handle leaks memory the GC cannot reclaim deterministically. A model-object (`Space`/`Surface`/`ThermalZone`/`Construction`/material) lives by its owning `Model` and is never disposed independently.
- Model mutation is not thread-safe: the native SDK admits one logical owner per `Model`, translators run sequentially, and a translation or model build offloads as one unit of work over this single-threaded native boundary.
- Any load or get that can miss returns an `Optional<T>`/`OptionalDouble`; the law is `is_initialized()` (or `!isNull()`) then `get()`, else `value_or(default)`, and a bare `get()` on an empty optional faults in native code. Lower the SWIG optional onto `Fin<T>`/`Option<T>` at the consuming boundary — the Bim Exchange edge, or the Compute runner folding a missing required output onto `ComputeFault.AnalysisFailed(SolvePhase.Extraction, FailureKind.Foreign, …)`.
- Every file API takes a `Path` built with `OpenStudioUtilitiesCore.toPath(string)` and read back with `toString(Path)`; a raw `string` does not compile against the file overloads.
- OpenStudio builds the model and reads the results; the EnergyPlus binary is a parameterized subprocess resolved through `Analysis/energy` `EnergyToolchain.Resolve` (`ENERGYPLUS_EXE` → `OPENSTUDIO_ENERGYPLUSDIR` → configured path → the package's bundled-runtime fallback), and an unresolved binary returns `ComputeFault.ToolchainUnresolved` with the probe trail.
- EnergyPlus must match the OpenStudio SWIG version: the SWIG-generated IDF schema tracks that version, so a version-matched EnergyPlus consumes the forward-translated IDF and a resolved-binary mismatch folds into the provider diagnostics.

[STACKING]:
- `HoneybeeSchema`(`Rasm.Bim/.api/api-honeybee-schema.md`): the HBJSON authored model meets this OSM/IDF runtime at gbXML — `GbXMLReverseTranslator.loadModel` ingests the shared gbXML — and at the canonical Bim energy model; the full HBJSON→OSM path runs the external `honeybee-openstudio` Python step.
- `GeometryGymIFC`(`Rasm.Bim/.api/api-geometrygym-ifc.md`): an IFC building exports gbXML, `GbXMLReverseTranslator.loadModel` folds it into a `Model`, and IFC spaces and zones become OSM spaces and thermal zones at the Exchange/import boundary.
- `SharpGLTF.Ext.3DTiles`(`api-sharpgltf-3dtiles.md`): `GltfForwardTranslator` and `ThreeJSForwardTranslator` emit a model's geometry onto the Exchange/export delivery path the glTF and 3D-Tiles legs share.
- `System.IO.Hashing`(`api-hashing.md`): a saved `.osm`/IDF UTF-8 string feeds `XxHash3` for the in-process fingerprint and `XxHash128` for the persisted content key into the `Rasm.Persistence` artifact index — the saved IDF/SQL artifacts ride the same content-identity path as every other Compute artifact.
- Bim consumer anchor: the Energy Exchange lowers each `Optional<Model>` onto `Fin<Model>`, retains translator `errors()`/`warnings()` on the translation result, and offloads a translation as one unit of work; this leg owns OSM load/save/version-upgrade and the gbXML/SDD semantic bridges, and never builds from the element graph.
- Compute consumer anchor: `Analysis/energy` builds the `Model` from the `Rasm.Element` `ElementGraph` — spatial Object nodes become `Space`/`ThermalZone`, faces become `Surface(Point3dVector, Model)`, and `MaterialComposition.LayerSet` becomes `Construction.setLayers(MaterialVector)` over `StandardOpaqueMaterial`/`StandardGlazing` carrying the shared `MaterialPropertySet.Thermal` conductivity and thickness — the graph already lowered from IFC by Bim's projector. The `Analysis/aggregator` ISO 6946 series-U fold reads the same shared thermal properties, so the EnergyPlus U-value and the closed-form value agree by construction; the `SqlFile` annual outputs become an `AssessmentResult` fact stream written back as a content-keyed `Node.Assessment` `GraphDelta`, keyed via `Runtime/codecs` content addressing so a re-run on an unchanged graph reuses the prior result.

[LOCAL_ADMISSION]:
- `Rasm.Bim` exchange leg: model read enters through `VersionTranslator.loadModel(path)` returning an `OptionalModel` lowered to `Fin<Model>`; model write enters through `model.save(path, overwrite)`; translation enters through the matching `*Translator` under a `using`, retaining `errors()`/`warnings()` on the result.
- `Rasm.Compute` simulation leg: `Analysis/energy` builds the energy model in-process from the shared `ElementGraph` (`new Model()` with the `new Space`/`Surface`/`Construction`/`StandardOpaqueMaterial` folds), forward-translates through `new EnergyPlusForwardTranslator().translateModel`, runs the `EnergyToolchain`-resolved subprocess, and reads back through `new SqlFile(toPath(path))`.

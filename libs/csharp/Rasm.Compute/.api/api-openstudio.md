# [RASM_COMPUTE_API_OPENSTUDIO]

`NREL.OpenStudio.macOS-arm64` is the osx-arm64 SWIG-generated C# binding to the NREL OpenStudio SDK: it owns the OSM `Model`, the EnergyPlus IDF/IDD object layer, and the `EnergyPlusForwardTranslator` to EnergyPlus. In `Rasm.Compute` it drives the `Analysis/energy` simulation lane — building an `OpenStudio.Model` in-process from the `Rasm.Element` `ElementGraph`, forward-translating it to an EnergyPlus IDF `Workspace`, and reading the post-run `SqlFile`. EnergyPlus itself runs as a parameterized subprocess resolved through the `EnergyToolchain`, never bundled here.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `NREL.OpenStudio.macOS-arm64`
- package: `NREL.OpenStudio.macOS-arm64` (NREL OpenStudio License, permissive BSD-3-derived; NREL Alliance for Sustainable Energy)
- assembly: `OpenStudio` (`lib/netstandard2.0/OpenStudio.dll`, the managed SWIG wrapper)
- namespace: `OpenStudio` (the model/translator/utility domain types, the `Optional<T>` and `*Vector` SWIG marshaling families, and the per-module `OpenStudio*PINVOKE` DllImport classes)
- asset: managed binding TFM `netstandard2.0`, binding forward under net10.0; the native runtime is RID-locked to `osx-arm64`
- native: `runtimes/osx-arm64/native/libopenstudiolib.dylib` (the SDK) with the `libopenstudio_csharp` / `libopenstudio_model_csharp` / `libopenstudio_translators_csharp` P/Invoke shims; `build/OpenStudio.targets` stages the dylibs next to consumer output
- platform: the macOS-arm64 member of a per-RID family; a Windows or Linux host binds the sibling `NREL.OpenStudio.win-x64` / `linux-x64` package with an identical managed API over a different native runtime
- dependency: empty net-standard dependency group; the native runtime is fully bundled with no managed transitive deps
- consumer: `libs/csharp/Rasm.Compute` (whole-building-energy simulation), co-owned with `libs/csharp/Rasm.Bim` (the disjoint IFC↔OSM semantic exchange owner)
- rail: energy

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: SWIG marshaling primitives — the native boundary

Every wrapper holds a native `HandleRef` (`swigCPtr`) with a `cMemoryOwn` flag under `IDisposable`; the C++ STL/Boost shapes surface as the families below, lowered onto `Fin<T>`/`Option<T>` at the runner boundary so interior code never sees a SWIG type.

| [INDEX] | [SYMBOL]                  | [TYPE_FAMILY] | [CAPABILITY]                                                |
| :-----: | :------------------------ | :------------ | :---------------------------------------------------------- |
|  [01]   | `Optional<T>`             | class         | the boost::optional carrier every missing accessor returns  |
|  [02]   | `OptionalDouble`          | class         | the missing-double the `SqlFile`/`EpwFile` accessors return |
|  [03]   | `*Vector`                 | class         | std::vector marshaling, `IList`-shaped                      |
|  [04]   | `Path`                    | class         | the boost::filesystem path, mandatory at every file API     |
|  [05]   | `Point3d`                 | class         | the surface-vertex value                                    |
|  [06]   | `OpenStudioUtilitiesCore` | class         | the SWIG global-function host                               |

- `Optional<T>`: `OptionalModel`/`OptionalWorkspace`/`OptionalConstruction` and siblings; `is_initialized()` (or `!isNull()`) gates `get()`, else `value_or(T)` — a bare `get()` on an empty optional faults in native code.
- `*Vector`: `Point3dVector : IEnumerable<Point3d>` (surface vertices), `MaterialVector : IEnumerable<Material>` (construction layers), `LogMessageVector` (translator sink), `SurfaceVector`/`SpaceVector`; a getter-returned vector is itself `IDisposable`.
- `Path`: no `Path(string)` ctor — build with `OpenStudioUtilitiesCore.toPath(string)`, read back with `toString(Path)`; a raw `string` is rejected at the file overloads.
- `OpenStudioUtilitiesCore`: hosts `toPath`/`toString`/`createUUID` as static globals, distinct from the `*PINVOKE` shims.

[PUBLIC_TYPE_SCOPE]: model build — constructed from the seam graph

Model-object construction reads the `Rasm.Element` `ElementGraph`; each object is `new`-ed against its owning `Model` and lives by the model's lifetime, so only the top-level `Model`, translator, and file/optional/vector handles are bracketed.

| [INDEX] | [SYMBOL]                                          | [TYPE_FAMILY] | [CAPABILITY]                                     |
| :-----: | :------------------------------------------------ | :------------ | :----------------------------------------------- |
|  [01]   | `Model`                                           | class         | the OSM model root and translator input          |
|  [02]   | `Space` / `Building`                              | class         | the conditioned volume; the model root container |
|  [03]   | `Surface`                                         | class         | the bounding surface per Object-node face        |
|  [04]   | `ThermalZone`                                     | class         | the EnergyPlus thermal-balance unit              |
|  [05]   | `Construction`                                    | class         | the ordered layered-material stack               |
|  [06]   | `StandardOpaqueMaterial` / `StandardGlazing`      | class         | the opaque and glazing construction layers       |
|  [07]   | `WeatherFile` / `SimulationControl` / `RunPeriod` | class         | site context, annual run period, control flags   |

- `Model`: `: Workspace`; built here from the graph, never loaded from an `.osm` (Bim's exchange concern).
- `Space`: `: PlanarSurfaceGroup`; each spatial Object node folds into one, assigned to a `ThermalZone : HVACComponent`.
- `Construction`: `: LayeredConstruction`; `setLayers(MaterialVector)` orders the stack the seam `MaterialComposition.LayerSet` lowers to.
- `StandardOpaqueMaterial`: `: OpaqueMaterial`; the ctor's thermal args feed the same seam properties the `Analysis/aggregator` ISO 6946 U-value fold reads.

[PUBLIC_TYPE_SCOPE]: forward translation and results

| [INDEX] | [SYMBOL]                      | [TYPE_FAMILY] | [CAPABILITY]                                    |
| :-----: | :---------------------------- | :------------ | :---------------------------------------------- |
|  [01]   | `EnergyPlusForwardTranslator` | class         | OSM → EnergyPlus IDF translation with a receipt |
|  [02]   | `Workspace`                   | class         | the generic IDD-object store the IDF is         |
|  [03]   | `IdfFile`                     | class         | the EnergyPlus IDF text model                   |
|  [04]   | `IddFileType`                 | enum          | the IDD schema-key selector                     |
|  [05]   | `EpwFile`                     | class         | the weather file and its site accessors         |
|  [06]   | `SqlFile`                     | class         | the post-run results SQLite reader              |
|  [07]   | `LogMessageVector` / `Logger` | class         | the native translator log sink and logger       |
|  [08]   | `LogLevel`                    | enum          | the log-severity selector                       |

- `EpwFile`: site accessors `latitude()`/`longitude()`/`timeZone()`/`elevation()` → `double` and `data()` → the `EpwDataPointVector` whose rows carry `directNormalRadiation()`/`diffuseHorizontalRadiation()`/`globalHorizontalRadiation()` → `OptionalDouble`, the `Analysis/daylight` `WeatherIngress` reader.
- `SqlFile`: annual `totalSiteEnergy()`/`netSiteEnergy()`/`totalSourceEnergy()`/`netSourceEnergy()`, per-end-use `electricityHeating()`/`electricityCooling()`/`electricityInteriorLighting()`/`electricityTotalEndUses()`/`naturalGasHeating()`, and per-month `energyConsumptionByMonth`/`peakEnergyDemandByMonth` all return `OptionalDouble`.
- `LogMessageVector`: collects the translator `warnings()`/`errors()`, folded into the Compute receipt fact stream, never the console.

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: build the model from the graph

| [INDEX] | [SURFACE]                                             | [SHAPE]  | [CAPABILITY]                                      |
| :-----: | :---------------------------------------------------- | :------- | :------------------------------------------------ |
|  [01]   | `new Model() -> Model`                                | ctor     | the empty OSM model the graph folds into          |
|  [02]   | `new Space(Model)`                                    | ctor     | a conditioned volume per spatial Object node      |
|  [03]   | `new ThermalZone(Model)`                              | ctor     | a thermal-balance zone each `Space` assigns to    |
|  [04]   | `new Surface(Point3dVector, Model)`                   | ctor     | a bounding surface from Object-node face vertices |
|  [05]   | `new Construction(Model)`                             | ctor     | the layered-construction owner                    |
|  [06]   | `Construction.setLayers(MaterialVector) -> bool`      | instance | order the layered-material stack                  |
|  [07]   | `new StandardOpaqueMaterial(Model, string, double×4)` | ctor     | an opaque layer from seam thermal properties      |

- `new StandardOpaqueMaterial`: orders args `roughness, thickness, conductivity, density, specificHeat`; shorter overloads default the trailing scalars.

[ENTRYPOINT_SCOPE]: forward-translate and write the IDF

| [INDEX] | [SURFACE]                                                              | [SHAPE]  | [CAPABILITY]                                 |
| :-----: | :--------------------------------------------------------------------- | :------- | :------------------------------------------- |
|  [01]   | `new EnergyPlusForwardTranslator().translateModel(Model) -> Workspace` | instance | OSM → the EnergyPlus IDF                     |
|  [02]   | `translator.warnings() / errors() -> LogMessageVector`                 | instance | the translation receipt                      |
|  [03]   | `Workspace.save(Path, bool) -> bool`                                   | instance | persist the IDF to the scratch run directory |
|  [04]   | `new EpwFile(Path) -> EpwFile`                                         | ctor     | the weather file passed to the subprocess    |

[ENTRYPOINT_SCOPE]: read the results

EnergyPlus runs as the `EnergyToolchain`-resolved subprocess (`energyplus -w <epw> -d <out> -r <idf>`), not through this binding; the runner opens `eplusout.sql` after it exits and derives EUI as annual total energy ÷ conditioned floor area. Every `sql.*` accessor returns `OptionalDouble`.

| [INDEX] | [SURFACE]                                                                       | [SHAPE]  | [CAPABILITY]                          |
| :-----: | :------------------------------------------------------------------------------ | :------- | :------------------------------------ |
|  [01]   | `new SqlFile(Path) -> SqlFile`                                                  | ctor     | open `eplusout.sql` after the run     |
|  [02]   | `sql.totalSiteEnergy()`                                                         | instance | annual site/source energy             |
|  [03]   | `sql.electricityHeating() / electricityCooling()`                               | instance | per-end-use annual demand             |
|  [04]   | `sql.energyConsumptionByMonth(EndUseFuelType, EndUseCategoryType, MonthOfYear)` | instance | per-month consumption and peak demand |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Managed types wrap native handles (`cMemoryOwn`); the `*PINVOKE` classes are the DllImport surface into the shims, an implementation detail — drive only the public `OpenStudio.*` wrappers.
- Bracket the `Model`, `EnergyPlusForwardTranslator`, `Workspace`, `EpwFile`, `SqlFile`, and every `Optional*`/`*Vector` result under `using`, so a dropped handle never leaks native memory the GC cannot reclaim; a model-object (`Space`/`Surface`/`ThermalZone`/`Construction`/material) lives by its owning `Model` and is never disposed independently.
- Any missing-capable accessor returns `Optional<T>`/`OptionalDouble`: gate `is_initialized()` (or `!isNull()`) before `get()`, else `value_or(default)`, then lower onto `Fin<T>`/`Option<T>` at the runner boundary and rail a missing required output onto `ComputeFault.AnalysisFailed(SolvePhase.Extraction, FailureKind.Foreign, …)`.
- `Path` is mandatory at every file API — build with `OpenStudioUtilitiesCore.toPath(string)`, read back with `toString(Path)`.
- One logical owner builds each `Model` as a single unit of work; the native SDK is not thread-safe for concurrent model mutation, so the Compute concurrency rail brackets the build as a single-threaded native boundary.
- OpenStudio builds the model and reads the results; the EnergyPlus binary is a parameterized subprocess resolved through `Analysis/energy` `EnergyToolchain.Resolve` (`ENERGYPLUS_EXE` → `OPENSTUDIO_ENERGYPLUSDIR` → configured path → the package's bundled-runtime fallback), and an unresolved binary rails `ComputeFault.ToolchainUnresolved` with the probe trail.
- EnergyPlus must match the OpenStudio SWIG version: the SWIG-generated IDF schema tracks that version, so a version-matched EnergyPlus consumes the forward-translated IDF and a resolved-binary mismatch folds a warning into the receipt.

[STACKING]:
- `libs/csharp/.api/api-hashing.md` (`XxHash128`): the saved IDF/SQL artifacts feed the content-keyed artifact index on the same content-identity rail as every other Compute artifact.
- `Rasm.Element` `ElementGraph` (`Analysis/energy` model build): spatial Object nodes become `Space`/`ThermalZone`, faces become `Surface(Point3dVector, Model)`, and `MaterialComposition.LayerSet` becomes `Construction.setLayers(MaterialVector)` over `StandardOpaqueMaterial`/`StandardGlazing` carrying the seam `MaterialPropertySet.Thermal` conductivity and thickness — the model is built from the canonical seam graph, already lowered from IFC by Bim's projector.
- `Analysis/aggregator` (ISO 6946 series-U fold): the OSM construction layers read the same seam thermal conductivity and thickness the aggregator folds, so the EnergyPlus U-value and the closed-form `Analysis/physics` ISO 6946 U-value agree by construction — one thermal-property source, two consumers.
- `Analysis/assessment` (the spine): the `SqlFile` annual outputs become an `AssessmentResult` fact stream written back as a content-keyed `Node.Assessment` `GraphDelta`, keyed via `Runtime/codecs` content addressing so a re-run on an unchanged graph reuses the prior result.

[LOCAL_ADMISSION]:
- `Analysis/energy` builds the energy model in-process from the seam `ElementGraph` (`new Model()` with the `new Space`/`Surface`/`Construction`/`StandardOpaqueMaterial` folds), forward-translates through `new EnergyPlusForwardTranslator().translateModel`, runs the `EnergyToolchain`-resolved subprocess, and reads back through `new SqlFile(toPath(path))`.

[RAIL_LAW]:
- Package: `NREL.OpenStudio.macOS-arm64` (assembly `OpenStudio`)
- Owns: the in-process OSM `Model` build from the seam graph, the `EnergyPlusForwardTranslator` OSM→IDF translation, the `SqlFile` results read, the weather/IDF file marshaling, and the SWIG native-handle discipline — the `Analysis/energy` simulation lane
- Accept: model build from the `Rasm.Element` `ElementGraph`, OSM→IDF forward translation, the version-matched EnergyPlus subprocess driven by the parameterized `EnergyToolchain`, and the `SqlFile` annual read lowered onto the `Analysis/assessment` fact stream and the content-keyed artifact rail
- Reject: IFC↔OSM/gbXML semantic exchange, OSM authoring or version-upgrade from disk, and re-authoring the energy model from IFC — the disjoint `Rasm.Bim/.api/api-openstudio` owner holds the semantic-exchange leg of the same package, and the seam graph is the source here; running the EnergyPlus solver inside the binding (the resolved subprocess owns it), HBJSON authoring (`HoneybeeSchema`), a `get()` on an unchecked SWIG optional, a raw `string` where a `Path` is required, a leaked `OpenStudio.*` handle past the runner boundary, and cross-platform native portability are the hand-rolled forms a denser owner forecloses

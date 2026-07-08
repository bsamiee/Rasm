# [RASM_COMPUTE_API_OPENSTUDIO]

`NREL.OpenStudio.macOS-arm64` is the osx-arm64 SWIG-generated C# binding to the NREL OpenStudio SDK
— the whole-building-energy SDK that owns the OSM model, the EnergyPlus IDF/IDD object layer, and the
`EnergyPlusForwardTranslator` matrix to EnergyPlus. In `Rasm.Compute` it is the SIMULATION engine of
the `Analysis/energy#SIMULATION_RUN` runner — the `Discipline.Energy` arm of the assessment rail: it
BUILDS an `OpenStudio.Model` IN-PROCESS from the concrete `Rasm.Element` `ElementGraph` (spaces,
surfaces, layered constructions, opaque/glazing materials, thermal zones — all read from the graph),
forward-translates it to an EnergyPlus IDF `Workspace`, then reads the post-run results `SqlFile`. It
neither runs nor bundles the EnergyPlus solver: the binary is a PARAMETERIZED subprocess resolved
through the `Analysis/energy#TOOLCHAIN_BOUNDARY` `EnergyToolchain` (env-var → configured-path →
bundled-fallback) and version-locked to the SWIG version (OpenStudio → EnergyPlus). The
managed `OpenStudio.dll` (5390 generated types, namespace `OpenStudio`) marshals into a bundled native
runtime (`libopenstudiolib.dylib` + the three `libopenstudio_*_csharp.dylib` SWIG shims); every
wrapper owns a native handle and is `IDisposable`. This is the SIMULATION concern only — it is the
folder-specific Compute twin of the Bim `Model`-exchange OpenStudio owner (`Rasm.Bim/.api/api-openstudio`,
the IFC↔OSM/gbXML SEMANTIC exchange leg): the two are ALIGNED-not-coupled on the one central pin, framed
to disjoint rails (Compute simulates the canonical seam graph; Bim exchanges IFC↔OSM semantics), and
this catalog never re-authors an IFC↔OSM semantic translator.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `NREL.OpenStudio.macOS-arm64`
- package: `NREL.OpenStudio.macOS-arm64`
- license: NREL OpenStudio License (the `LICENSE.md` at `github.com/NREL/OpenStudio` — a permissive BSD-3-Clause-derived license; the nuspec carries a `licenseUrl`, no SPDX expression)
- assembly: `OpenStudio` (`lib/netstandard2.0/OpenStudio.dll`, the managed SWIG wrapper; binds forward under net10.0)
- namespace: `OpenStudio` (the full 5390-type generated surface: the model/translator/utility domain types plus the `Optional<T>` (~743) and `*Vector` (~722) SWIG marshaling families) + the per-SWIG-module `OpenStudio*PINVOKE` marshaling classes (`OpenStudioModelPINVOKE`, `OpenStudioModelResourcesPINVOKE`, `OpenStudioEnergyPlusPINVOKE`, `OpenStudioUtilitiesSqlPINVOKE`, …)
- asset: managed binding TFM `netstandard2.0`; the runtime is RID-LOCKED to `osx-arm64`
- native: `runtimes/osx-arm64/native/libopenstudiolib.dylib` (the SDK) + `libopenstudio_csharp.dylib` / `libopenstudio_model_csharp.dylib` / `libopenstudio_translators_csharp.dylib` (the SWIG P/Invoke shims); `build/OpenStudio.targets` stages the dylibs next to the consumer output (Compute references the package with `ExcludeAssets="build"` and stages the native runtime explicitly, so the bundled-runtime location is the `EnergyToolchain` fallback `Path.Combine(AppContext.BaseDirectory, "runtimes", "osx-arm64", "native", "EnergyPlus")`)
- platform: the macOS-arm64 RID member of a per-RID family — a Windows/Linux host admits the sibling `NREL.OpenStudio.win-x64` / `NREL.OpenStudio.linux-x64`; the managed API is identical, only the native runtime differs
- dependency: empty net-standard dependency group (the native runtime is fully bundled; no managed transitive deps)
- consumer: `libs/csharp/Rasm.Compute` (the `Analysis/energy` whole-building-energy SIMULATION runner) — co-owned with `libs/csharp/Rasm.Bim` (the disjoint IFC↔OSM SEMANTIC exchange owner) on the one central pin
- rail: `Analysis/energy#SIMULATION_RUN`

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: SWIG marshaling primitives (the native boundary)
- package: `NREL.OpenStudio.macOS-arm64`
- namespace: `OpenStudio`
- rail: energy

Every wrapper holds a native `HandleRef` (`swigCPtr`) with a `cMemoryOwn` flag and is `IDisposable`;
the C++ STL/Boost shapes surface as the families below, not as BCL collections. Lower each onto the
Compute rail (`Fin<T>`/`Option<T>`) at the runner boundary so interior code never sees a SWIG type.

| [INDEX] | [SYMBOL]                  | [RAIL]  | [CAPABILITY]                                                                                              |
| :-----: | :------------------------ | :------ | :------------------------------------------------------------------------------------------------------- |
|  [01]   | `Optional<T>` family      | energy  | the `boost::optional` marshaling — `OptionalDouble`/`OptionalModel`/`OptionalWorkspace`/`OptionalConstruction`/…; `is_initialized()`/`isNull()`, `get()`, `value_or(T)`. Every "load"/"get"/result accessor that can miss returns one — check `is_initialized()` before `get()` (a bare `get()` on an empty optional faults in native code) |
|  [02]   | `OptionalDouble`          | energy  | the `boost::optional<double>` the `SqlFile` annual accessors return (`: IDisposable`) — the missing-output carrier lowered to `Option<double>` at the result edge |
|  [03]   | `*Vector` family          | energy  | the `std::vector` marshaling — `Point3dVector` (`: IEnumerable<Point3d>`, the surface-geometry vertex carrier), `MaterialVector` (`: IEnumerable<Material>`, the layered-construction layer list), `LogMessageVector` (the translator warning/error sink), `SurfaceVector`/`SpaceVector`/…; `IList`-shaped (indexer, `Count`, `Add`) |
|  [04]   | `Path` / `OptionalPath`   | energy  | the `boost::filesystem::path` wrapper; there is NO `Path(string)` ctor — build from a string with the static `OpenStudioUtilitiesCore.toPath(string)`, read back with `OpenStudioUtilitiesCore.toString(path)`. The REQUIRED carrier at every file API (`IdfFile`/`SqlFile`/`EpwFile`/`Workspace.save`) — a raw `string` is not accepted |
|  [05]   | `Point3d` / `OpenStudioUtilitiesCore` | energy | the surface-vertex value (`: IDisposable`) the `Point3dVector` carries; `OpenStudioUtilitiesCore` the SWIG global-function host (`toPath`/`toString`/`createUUID`) the file boundaries route through, distinct from the `*PINVOKE` shims |

[PUBLIC_TYPE_SCOPE]: model build (from the seam graph)
- package: `NREL.OpenStudio.macOS-arm64`
- namespace: `OpenStudio`
- rail: energy

The runner CONSTRUCTS these from the `Rasm.Element` `ElementGraph` — a model-object is `new`-ed
against its owning `Model` and is owned BY the model (never independently `using`-disposed); only the
top-level `Model`, the translator, and the file/optional/vector handles are bracketed.

| [INDEX] | [SYMBOL]                  | [RAIL]  | [CAPABILITY]                                                                                              |
| :-----: | :------------------------ | :------ | :------------------------------------------------------------------------------------------------------- |
|  [01]   | `Model`                   | energy  | the OSM model (`: Workspace`); ctor `Model()`; the root every model-object hangs off and the `EnergyPlusForwardTranslator` input — built here from the graph, never loaded from an `.osm` (that is Bim's exchange concern) |
|  [02]   | `Space` / `Building`      | energy  | `Space : PlanarSurfaceGroup` (`new Space(model)`) the conditioned-volume each spatial Object node folds into; `Building : ParentObject` the model root container |
|  [03]   | `Surface`                 | energy  | `new Surface(Point3dVector vertices, Model model)` — the bounding surface each Object-node face becomes; carries its `Construction` and is owned by the model |
|  [04]   | `ThermalZone`             | energy  | `ThermalZone : HVACComponent` (`new ThermalZone(model)`) the zone each `Space` is assigned to — the EnergyPlus thermal-balance unit |
|  [05]   | `Construction` / `LayeredConstruction` | energy | `Construction : LayeredConstruction` (`new Construction(model)`); `setLayers(MaterialVector)` orders the layered-material stack the seam `MaterialComposition.LayerSet` lowers to |
|  [06]   | `StandardOpaqueMaterial`  | energy  | `: OpaqueMaterial : Material`; ctor `StandardOpaqueMaterial(Model model, string roughness, double thickness, double conductivity, double density, double specificHeat)` — the opaque layer built from the seam thermal properties so the OSM U-value matches the `Analysis/aggregator` ISO 6946 fold |
|  [07]   | `StandardGlazing`         | energy  | `: Glazing : ...`; the glazing layer for a transparent construction (the seam glazing thermal/optical properties) |
|  [08]   | `WeatherFile` / `SimulationControl` / `RunPeriod` | energy | `WeatherFile : ModelObject` (the site/design-day context the `EpwFile` attaches to), `SimulationControl`/`RunPeriod : ParentObject` (the annual-run period and control flags) |

[PUBLIC_TYPE_SCOPE]: forward translation + results
- package: `NREL.OpenStudio.macOS-arm64`
- namespace: `OpenStudio`
- rail: energy

| [INDEX] | [SYMBOL]                      | [RAIL]  | [CAPABILITY]                                                                                              |
| :-----: | :---------------------------- | :------ | :------------------------------------------------------------------------------------------------------- |
|  [01]   | `EnergyPlusForwardTranslator` | energy  | `: IDisposable`; `translateModel(Model)` → `Workspace` (the IDF for EnergyPlus); `errors()`/`warnings()` → `LogMessageVector` for the translator receipt — the ONE translator this runner uses (the reverse/gbXML/SDD/three.js translators are the Bim exchange concern) |
|  [02]   | `Workspace`                   | energy  | `: IDisposable`; the generic IDD-object store the IDF is; `save(Path, bool overwrite)` writes the IDF to the scratch run directory |
|  [03]   | `IdfFile` / `IddFileType`     | energy  | the EnergyPlus IDF text model and its IDD schema key (read back when post-processing a foreign IDF) |
|  [04]   | `EpwFile`                     | energy  | `: IDisposable`; `EpwFile(Path)` the EnergyPlus weather file attached to the `WeatherFile` and passed to the subprocess (`-w`); decompile-verified site accessors `latitude()`/`longitude()`/`timeZone()`/`elevation()` → `double` and `data()` → the `EpwDataPoint` vector whose rows carry `directNormalRadiation()`/`diffuseHorizontalRadiation()`/`globalHorizontalRadiation()` → `OptionalDouble` — the `Analysis/daylight` `WeatherIngress` reader |
|  [05]   | `SqlFile`                     | energy  | `: IDisposable`; `SqlFile(Path)` the EnergyPlus results SQLite; the annual accessors `totalSiteEnergy()`/`netSiteEnergy()`/`totalSourceEnergy()`/`netSourceEnergy()` → `OptionalDouble`, the end-use `electricityHeating()`/`electricityCooling()`/`electricityInteriorLighting()`/`electricityTotalEndUses()`/`naturalGasHeating()`/… → `OptionalDouble`, the per-month `energyConsumptionByMonth(EndUseFuelType, EndUseCategoryType, MonthOfYear)`/`peakEnergyDemandByMonth(…)` → `OptionalDouble` — the post-run result reader |
|  [06]   | `LogMessageVector` / `Logger` / `LogLevel` | energy | the native log sink; the translator `warnings()`/`errors()` returns `LogMessageVector` — fold into the Compute receipt fact stream, never the console |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: build the model from the graph
- package: `NREL.OpenStudio.macOS-arm64`
- namespace: `OpenStudio`
- rail: energy

| [INDEX] | [SURFACE]                                  | [CALL_SHAPE]                                                                 | [CAPABILITY]                                                  |
| :-----: | :----------------------------------------- | :-------------------------------------------------------------------------- | :----------------------------------------------------------- |
|  [01]   | `new Model()`                              | `()` → `Model`                                                              | the empty OSM model the graph is folded into (one `using` owner) |
|  [02]   | `new Space(model)` / `new ThermalZone(model)` | `(Model)` → owned model object                                            | a conditioned volume / a thermal zone per spatial Object node |
|  [03]   | `new Surface(vertices, model)`             | `(Point3dVector, Model)` → `Surface`                                        | a bounding surface from the Object-node face vertices |
|  [04]   | `new Construction(model)` + `setLayers(MaterialVector)` | `(Model)` then `(MaterialVector)`                                | a layered construction from the seam `MaterialComposition.LayerSet` |
|  [05]   | `new StandardOpaqueMaterial(model, roughness, thickness, conductivity, density, specificHeat)` | `(Model, string, double×4)` → layer | an opaque layer from the seam thermal properties (the ISO 6946 U-value match) |

[ENTRYPOINT_SCOPE]: forward-translate + write the IDF
- package: `NREL.OpenStudio.macOS-arm64`
- namespace: `OpenStudio`
- rail: energy

| [INDEX] | [SURFACE]                                          | [CALL_SHAPE]                                                          | [CAPABILITY]                                                  |
| :-----: | :------------------------------------------------- | :------------------------------------------------------------------- | :----------------------------------------------------------- |
|  [01]   | `new EnergyPlusForwardTranslator().translateModel` | `(Model)` → `Workspace`                                              | OSM → EnergyPlus IDF; then `errors()`/`warnings()` for the receipt |
|  [02]   | `idf.save`                                          | `(OpenStudioUtilitiesCore.toPath(idfPath), overwrite: true)` → `bool` | persist the IDF to the scratch run directory |
|  [03]   | `new EpwFile`                                       | `(OpenStudioUtilitiesCore.toPath(epwPath))` → `EpwFile`             | the weather file passed to the subprocess |

[ENTRYPOINT_SCOPE]: run (subprocess) + read the results
- package: `NREL.OpenStudio.macOS-arm64`
- namespace: `OpenStudio`
- rail: energy

| [INDEX] | [SURFACE]                                          | [CALL_SHAPE]                                                          | [CAPABILITY]                                                  |
| :-----: | :------------------------------------------------- | :------------------------------------------------------------------- | :----------------------------------------------------------- |
|  [01]   | `EnergyToolchain.Resolve` → `Process` `energyplus -w <epw> -d <out> -r <idf>` | `(EnergyToolchainPolicy)` → `Fin<string>` then the subprocess | OpenStudio does NOT run EnergyPlus — the resolved binary subprocess does (the parameterized discovery boundary) |
|  [02]   | `new SqlFile`                                       | `(OpenStudioUtilitiesCore.toPath(sqlPath))` → `SqlFile`             | open `eplusout.sql` after the run |
|  [03]   | `sql.totalSiteEnergy()` / `electricityHeating()` / `electricityCooling()` | `()` → `OptionalDouble`                              | the annual site energy / end-use demands — lower `OptionalDouble` to `Option<double>`, derive EUI = total ÷ conditioned floor area |

## [04]-[IMPLEMENTATION_LAW]

[NATIVE_BOUNDARY]:
- this is a SWIG binding over a bundled C++ runtime; the managed types are thin wrappers around native handles. The package is RID-locked to `osx-arm64`; Compute stages `libopenstudiolib.dylib` + the three `libopenstudio_*_csharp.dylib` shims next to the consumer output (the bundled-runtime location is the `EnergyToolchain` fallback). A Windows/Linux build admits the sibling per-RID package; never assume one runtime asset is portable.
- the `*PINVOKE` classes (`OpenStudioModelResourcesPINVOKE`, `OpenStudioUtilitiesSqlPINVOKE`, …) are the DllImport surface into the native shims — an implementation detail, NOT a call surface; drive the public `OpenStudio.*` wrappers only.

[DISPOSAL_AND_THREADING]:
- every wrapper owns native memory (`cMemoryOwn`); bracket the `Model`, the `EnergyPlusForwardTranslator`, the `Workspace` IDF, the `EpwFile`, the `SqlFile`, and the `Optional*`/`*Vector` results under `using` — a dropped handle leaks native memory the GC cannot reclaim deterministically. A `*Vector`/`OptionalDouble` returned from a getter is itself disposable.
- a model-object (`Space`/`Surface`/`ThermalZone`/`Construction`/`StandardOpaqueMaterial`) is owned BY the `Model` it is `new`-ed against — never `using`-dispose it independently (the `Model` owns its lifetime); only the top-level handles are bracketed.
- the native SDK is not thread-safe for concurrent model mutation; serialize the build as one unit of work (one logical owner per `Model`), never a parallel fan-out over shared model state — the single-threaded-native boundary the Compute concurrency rail brackets.

[OPTIONAL_DISCIPLINE]:
- any load/get/result accessor that can miss returns an `Optional<T>`/`OptionalDouble`; the law is `is_initialized()` (or `!isNull()`) THEN `get()`, or `value_or(default)`. A bare `get()` on an empty optional faults in native code. Lower onto `Fin<T>`/`Option<T>` at the runner boundary so interior code never sees a SWIG optional, and rail a missing required output (`totalSiteEnergy()` empty) onto `ComputeFault.AnalysisFailed(SolvePhase.Extraction, FailureKind.Foreign, …)`, never a silent zero-energy result.
- `Path` is mandatory at every file API: build with `OpenStudioUtilitiesCore.toPath(string)` (there is no `Path(string)` ctor), read back with `OpenStudioUtilitiesCore.toString(path)`; a raw `string` does not compile against the file overloads.

[SIMULATION_BOUNDARY]:
- OpenStudio BUILDS the model and READS the results — it neither runs nor bundles the EnergyPlus solver. The EnergyPlus binary is a PARAMETERIZED subprocess resolved through `Analysis/energy#TOOLCHAIN_BOUNDARY` `EnergyToolchain.Resolve` (`ENERGYPLUS_EXE` → `OPENSTUDIO_ENERGYPLUSDIR` → configured-path → the OpenStudio package's bundled-runtime fallback), never a hardcoded path; an unresolved binary rails `ComputeFault.ToolchainUnresolved` with the full probe trail.
- the version-lock is load-bearing: EnergyPlus MUST track the OpenStudio SWIG version — OpenStudio forward-translates the IDF the version-matched EnergyPlus consumes; the mismatched standalone is NEVER selected (the IDF schema tracks the SWIG version), and a resolved-binary version mismatch folds a warning fact into the receipt. `Parametric_Forge` is a DEV/CI probe toolchain only (point `OPENSTUDIO_ENERGYPLUSDIR` at the OpenStudio-bundled); a SHIPPED app owns its own EnergyPlus provisioning.

[STACKING] — the whole-building-energy simulation lane of the Compute Analysis rail:
- with the `Rasm.Element` `ElementGraph` (via `Analysis/energy#MODEL_BUILD`): the runner reads the graph's spatial Object nodes → `Space`/`ThermalZone`, bounding surfaces → `Surface(Point3dVector, Model)`, `MaterialComposition.LayerSet` → `Construction.setLayers(MaterialVector)` over `StandardOpaqueMaterial`/`StandardGlazing` carrying the seam `MaterialPropertySet.Thermal` conductivity/thickness; the model is built from the canonical seam graph (already lowered from IFC by Bim's projector) — never re-authored from IFC here, the IFC↔OSM semantic exchange being the Bim concern.
- with `Analysis/aggregator` (the ISO 6946 series-U fold): the OSM construction layers read the SAME seam thermal conductivity/thickness the aggregator folds, so the EnergyPlus construction U-value and the closed-form `Analysis/physics` ISO 6946 U-value agree by construction — one thermal-property source, two consumers.
- with `Analysis/assessment` (the spine): the `SqlFile` annual outputs (`total-site-energy`/`eui`/`heating-demand`/`cooling-demand`) become an `AssessmentResult` fact stream the assessment spine writes back as a content-keyed `Node.Assessment` `GraphDelta`, keyed by the seam graph content address via `Runtime/codecs#CONTENT_ADDRESSING` so a re-run on an unchanged graph reuses the prior result.
- with `Rasm.Persistence`: the saved IDF/SQL artifacts (UTF-8/SQLite bytes) feed the content-keyed `XxHash128` artifact index (`api-hashing`) on the same content-identity rail as every other Compute artifact.

[LOCAL_ADMISSION]:
- the energy model is BUILT in-process from the seam `ElementGraph` (`new Model()` + the `new Space`/`Surface`/`Construction`/`StandardOpaqueMaterial` model-object folds), forward-translated through `new EnergyPlusForwardTranslator().translateModel`, run as the `EnergyToolchain`-resolved subprocess, and read back through `new SqlFile(toPath(path))` — loading an `.osm`/IFC and re-authoring an OSM model from IFC in Compute is the rejected form (that is the Bim semantic-exchange owner).
- the rejected forms are: a hardcoded EnergyPlus path (use the parameterized `EnergyToolchain`); selecting a version-mismatched EnergyPlus (the → lock holds); calling `get` on an unchecked `Optional`/`OptionalDouble`; passing a raw `string` where a `Path` is required; leaking native handles by skipping `Dispose`; `using`-disposing a model-owned object; and assuming the osx-arm64 runtime on another platform.

[RAIL_LAW]:
- Package: `NREL.OpenStudio.macOS-arm64` (assembly `OpenStudio`)
- Owns: the in-process OSM `Model` build from the seam graph, the `EnergyPlusForwardTranslator` OSM→IDF forward translation, the `SqlFile` EnergyPlus-results read, the weather/IDF file marshaling, and the SWIG native-handle discipline — the `Analysis/energy#SIMULATION_RUN` whole-building-energy lane
- Accept: model build from the `Rasm.Element` `ElementGraph` (via `Analysis/energy#MODEL_BUILD`), OSM→IDF forward translation, the EnergyPlus subprocess driven by the parameterized `EnergyToolchain` (version-locked →), the `SqlFile` annual-result read lowered onto the `Analysis/assessment` fact stream, the content-keyed result/artifact rails
- Reject: IFC↔OSM/gbXML SEMANTIC exchange and OSM authoring/version-upgrade from disk (the Bim `Rasm.Bim/.api/api-openstudio` owner — the disjoint folder-specific framing of the same pin); running the EnergyPlus solver inside this binding (the resolved subprocess does — OpenStudio produces its input and reads its output); HBJSON authoring (`HoneybeeSchema`) and IFC semantics (GeometryGym); re-authoring the energy model from IFC here (the seam graph is the source); calling `get()` on an unchecked SWIG optional; leaking `OpenStudio.*` handles past the runner boundary; cross-platform native portability (each RID admits its own package)

# [PY_GEOMETRY_API_HONEYBEE_OPENSTUDIO]

`honeybee-openstudio` is the in-process OpenStudio/EnergyPlus translator for the geometry energy-modeling rail: it converts a `honeybee-core` `Model` carrying `honeybee-energy` `.properties.energy` into a live OpenStudio `Model` (and onward to `.osm`/`.idf`/`.epJSON`/gbXML) and reads OpenStudio/IDF/gbXML back into honeybee — entirely in-process through the native `openstudio` Python SDK, with no OpenStudio-CLI subprocess. The energy-modeling owner picks this in-process translator over the `honeybee_energy.run` CLI shell-out whenever the native SDK is present, offloads the CPU-bound `model_to_openstudio` translation through `anyio.to_thread.run_sync` under an `opentelemetry` span, references every OpenStudio SDK class through the package's single `honeybee_openstudio.openstudio` re-export indirection (never importing the native `openstudio` module directly), and folds the translation receipt into the `expression` `Result` rail — never re-implementing the honeybee->OpenStudio object mapping, the gbXML writer, or the OSM/IDF round-trip the package already owns. It rides `honeybee-energy` as the spine and is the in-process complement to `honeybee_energy.run` (which orchestrates the external OpenStudio CLI), never a parallel translation layer.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `honeybee-openstudio`
- package: `honeybee-openstudio`
- import: `import honeybee_openstudio` then `from honeybee_openstudio.writer import model_to_openstudio, model_to_osm, model_to_idf`, `from honeybee_openstudio.reader import model_from_osm_file, model_from_idf_file`, `from honeybee_openstudio.openstudio import OSModel` (the SDK re-export indirection)
- owner: `geometry`
- rail: energy-modeling
- consumer: `.planning/energy/simulate.md` (the `WRITERS` format rows behind the `find_spec("openstudio")` probe, lane-offloaded)
- installed: `0.5.5`
- license: AGPL-3.0 (Ladybug Tools copyleft); the bound `openstudio` SDK is BSD-3-Clause
- abi: pure-Python `py3-none-any` wheel itself; it hard-imports the native `openstudio` SDK (`3.10.0`) at module load. That SDK ships a `py3-none-<platform>` wheel (compiled C++ OpenStudio/EnergyPlus core, platform-keyed not interpreter-keyed — e.g. `py3-none-macosx_11_0_arm64`, `Requires-Python >=3.7.1`), installed and importable on the active interpreter. `model_to_openstudio` returns a real `openstudio.openstudiomodelcore.Model`, so any consumer of the return value also links the native SDK.
- depends-on: the native `openstudio` SDK (`3.10.0`, the binary OpenStudio/EnergyPlus runtime) is the only unconditional `Requires-Dist`; `honeybee-energy>=1.118.17` rides the `base` extra (the energy model it translates) and `honeybee-energy-standards==2.3.0` the `standards` extra
- entry points: `honeybee-openstudio` console script (`honeybee_openstudio.cli:openstudio`); the rail composes the API, not the CLI
- capability: in-process honeybee `Model` -> OpenStudio `Model` translation; OSM/IDF/epJSON/gbXML serialization; reverse import from OSM/IDF/gbXML; per-object translators (room/face/aperture/door/shade/shade-mesh) and per-resource translators (construction/material/load/schedule/program-type/HVAC/SHW); `SimulationParameter` -> OpenStudio translation; EPW assignment and climate-zone derivation; the OpenStudio SDK class re-export surface

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: OpenStudio SDK re-export (`honeybee_openstudio.openstudio`)
- rail: energy-modeling
- The single indirection over the native `openstudio` SDK: every OpenStudio C++ class the translators touch is re-exported under an `OS`-prefixed alias, so the rest of the package (and the owner) references `OSModel`/`OSConstruction`/`OSGas` etc. through this module rather than importing `openstudio` directly. This isolates the SDK-version coupling to one module (`OSModel` is the `openstudio.openstudiomodelcore.Model`). Rows drop the shared `OS` alias prefix.

| [INDEX] | [SYMBOL]                                                              | [TYPE_FAMILY]   | [CAPABILITY]                                  |
| :-----: | :-------------------------------------------------------------------- | :-------------- | :-------------------------------------------- |
|  [01]   | `Model`                                                               | model root      | the OpenStudio `Model` the translation builds |
|  [02]   | `Construction` / `ConstructionAirBoundary` / `DefaultConstructionSet` | construction    | construction + default-construction-set       |
|  [03]   | `MasslessOpaqueMaterial` / `Gas` / `GasMixture` / `Blind`             | material        | OpenStudio material objects                   |
|  [04]   | `Lights` / `ElectricEquipment` / `GasEquipment` / `InternalMass`      | load            | load objects (+ their `*Definition` pairs)    |
|  [05]   | `DaylightingControl` / `DesignDay` / `BuildingStory`                  | model object    | daylighting / design-day / story objects      |
|  [06]   | `GeneratorPVWatts`                                                    | generation      | on-site PV generator                          |
|  [07]   | `ElectricLoadCenterDistribution`                                      | generation      | PV load-center distribution                   |
|  [08]   | `ElectricLoadCenterInverterPVWatts`                                   | generation      | PVWatts inverter                              |
|  [09]   | `EnergyManagementSystemProgram`                                       | EMS             | EMS program (dynamic-construction control)    |
|  [10]   | `EnergyManagementSystemActuator`                                      | EMS             | EMS actuator                                  |
|  [11]   | `EnergyManagementSystemSensor`                                        | EMS             | EMS sensor                                    |
|  [12]   | `AirflowNetworkSimpleOpening`                                         | airflow network | natural-ventilation simple opening            |
|  [13]   | `AirflowNetworkCrack`                                                 | airflow network | natural-ventilation crack                     |
|  [14]   | `AirflowNetworkHorizontalOpening`                                     | airflow network | natural-ventilation horizontal opening        |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: model translation writers (`honeybee_openstudio.writer`)
- rail: energy-modeling
- `model_to_openstudio` is the core translation (returns a live OpenStudio `Model`); the `model_to_*` family serializes onward. The per-object translators are the legs the model translator composes — the owner calls `model_to_openstudio`/`model_to_osm`, not the per-object functions. Every `model_to_*` writer takes `(model, seed_model=None, ...)`; `model_to_gbxml(model, triangulate_non_planar_orphaned=True, full_geometry=False, ...)` routes through a distinct geometry path.

| [INDEX] | [SURFACE]                  | [CALL_SHAPE]      | [CAPABILITY]                                                       |
| :-----: | :------------------------- | :---------------- | :----------------------------------------------------------------- |
|  [01]   | `model_to_openstudio`      | honeybee `Model`  | translate to a live OpenStudio `Model` (the in-process core)       |
|  [02]   | `model_to_osm`             | honeybee `Model`  | translate to the serialized OSM string (consumer owns the write)   |
|  [03]   | `model_to_idf`             | honeybee `Model`  | translate to the serialized EnergyPlus IDF string                  |
|  [04]   | `model_to_epjson`          | honeybee `Model`  | translate to the serialized EnergyPlus epJSON string               |
|  [05]   | `model_to_gbxml`           | honeybee `Model`  | translate to the serialized gbXML string (interoperability export) |
|  [06]   | `room_to_openstudio`       | object + os_model | per-object room translator (adjacency-mapped)                      |
|  [07]   | `face_to_openstudio`       | object + os_model | per-object face translator                                         |
|  [08]   | `aperture_to_openstudio`   | object + os_model | per-object aperture translator                                     |
|  [09]   | `door_to_openstudio`       | object + os_model | per-object door translator                                         |
|  [10]   | `shade_to_openstudio`      | object + os_model | per-object shade translator                                        |
|  [11]   | `shade_mesh_to_openstudio` | object + os_model | per-object shade-mesh translator                                   |

[ENTRYPOINT_SCOPE]: model readers (`honeybee_openstudio.reader`)
- rail: energy-modeling
- Reverse import: OpenStudio/IDF/gbXML -> honeybee `Model`. The `extract_all_*` helpers pull the resource tables first (constructions before schedules); the per-object `*_from_openstudio` readers rebuild the geometry+properties. The readers take `reset_properties=False`, and `model_from_osm_file` additionally `print_warnings=False`.

| [INDEX] | [SURFACE]                          | [CALL_SHAPE]       | [CAPABILITY]                                   |
| :-----: | :--------------------------------- | :----------------- | :--------------------------------------------- |
|  [01]   | `model_from_openstudio`            | OpenStudio `Model` | import a live OpenStudio model into honeybee   |
|  [02]   | `model_from_osm_file`              | `.osm` file        | import from an OSM file                        |
|  [03]   | `model_from_osm`                   | `.osm` string      | import from an OSM string                      |
|  [04]   | `model_from_idf_file`              | `.idf` file        | import from EnergyPlus IDF                     |
|  [05]   | `model_from_gbxml_file`            | gbXML file         | import from gbXML                              |
|  [06]   | `extract_all_constructions`        | OpenStudio `Model` | pull the construction resource table           |
|  [07]   | `extract_all_schedules`            | OpenStudio `Model` | pull the schedule resource table               |
|  [08]   | `room_from_openstudio`             | OpenStudio object  | per-object room reverse translator             |
|  [09]   | `face_from_openstudio`             | OpenStudio object  | per-object face reverse translator             |
|  [10]   | `construction_set_from_openstudio` | OpenStudio object  | per-object construction-set reverse translator |
|  [11]   | `program_type_from_openstudio`     | OpenStudio object  | per-object program-type reverse translator     |
|  [12]   | `ideal_air_system_from_openstudio` | OpenStudio object  | per-object ideal-air-system reverse translator |

[ENTRYPOINT_SCOPE]: simulation parameter translation (`honeybee_openstudio.simulation`)
- rail: energy-modeling
- Translate the honeybee `SimulationParameter` family and EPW into OpenStudio model objects, in-process. `simulation_parameter_to_openstudio(sim_par, seed_model=None)` translates the full bundle and `assign_epw_to_model(epw_file, os_model, set_climate_zone=False)` attaches the weather file.

| [INDEX] | [SURFACE]                            | [CALL_SHAPE]          | [CAPABILITY]                                    |
| :-----: | :----------------------------------- | :-------------------- | :---------------------------------------------- |
|  [01]   | `simulation_parameter_to_openstudio` | `SimulationParameter` | translate the full sim-par bundle to OpenStudio |
|  [02]   | `assign_epw_to_model`                | epw + os_model        | attach EPW and derive the ASHRAE climate zone   |
|  [03]   | `run_period_to_openstudio`           | part + os_model       | run-period translator                           |
|  [04]   | `design_day_to_openstudio`           | part + os_model       | design-day translator                           |
|  [05]   | `sizing_to_openstudio`               | part + os_model       | sizing-parameter translator                     |
|  [06]   | `shadow_calculation_to_openstudio`   | part + os_model       | shadow-calculation translator                   |
|  [07]   | `simulation_control_to_openstudio`   | part + os_model       | simulation-control translator                   |
|  [08]   | `simulation_output_to_openstudio`    | part + os_model       | simulation-output translator                    |
|  [09]   | `os_path(path_str)`                  | path                  | SDK path marshalling shim                       |
|  [10]   | `os_vector_len(vector)`              | vector                | SDK vector marshalling shim                     |

## [04]-[IMPLEMENTATION_LAW]

[ENERGY_MODELING_TRANSLATION]:
- import: `import honeybee_openstudio` at boundary scope; the native `openstudio` SDK is imported transitively at module load through `honeybee_openstudio.openstudio`, so a missing or wrong-version SDK fails at import. The owner gates on SDK presence (the `honeybee_openstudio.openstudio` import) before selecting the in-process rail.
- in-process axis: `model_to_openstudio` builds a live OpenStudio `Model` in the SAME process via the native SDK — there is NO OpenStudio-CLI subprocess. This is the IN-PROCESS complement to `honeybee_energy.run.run_osw` (which shells out to the external CLI). The owner picks the in-process translator when the SDK is present (faster, no temp-folder OSW round-trip) and falls back to the `honeybee_energy.run` CLI rail when only the CLI is installed. Because translation is CPU-bound (not a subprocess), the owner offloads it through `anyio.to_thread.run_sync` (CPU work off the event loop), under an `opentelemetry` span, never under a subprocess retry.
- SDK-indirection axis: every OpenStudio SDK class is referenced through `honeybee_openstudio.openstudio` (the `OS*` re-export module), not by importing `openstudio` directly. This isolates the SDK-version coupling to one module, so the owner's code and the version-sensitive C++ surface meet at exactly one indirection — never a scatter of `import openstudio` across the translators. `os_path`/`os_vector_len` are the C++ marshalling shims for path/vector arguments the SDK requires.
- writer/reader symmetry axis: the writers (`model_to_*`) and readers (`model_from_*`) are mirror families. The model writer composes the per-object `*_to_openstudio` translators with an adjacency map; the model reader composes `extract_all_constructions`/`extract_all_schedules` (resource tables first) then the per-object `*_from_openstudio` readers. The owner calls the model-level functions; the per-object legs are owned internals, folded into one translation table keyed by object kind, never a hand-written per-object dispatch.
- serialization-format axis: `model_to_osm`/`model_to_idf`/`model_to_epjson`/`model_to_gbxml` are one format-keyed export family over the same `model_to_openstudio` core (gbXML routes through a distinct geometry path). The owner selects the target format as a row, not a parallel translator.
- receipt axis: the translation result (the OpenStudio `Model` handle or the written file path, plus any translation warnings) folds into the `expression` `Result` rail; `reset_properties` on the readers controls whether inbound OpenStudio objects overwrite or merge honeybee `.properties.energy`.
- subpackages: `writer`, `reader`, `openstudio` (the SDK re-export), `simulation`, and the per-resource translator modules mirroring honeybee-energy (`construction`, `constructionset`, `material`, `load`, `schedule`, `programtype`, `hvac`, `shw`, `internalmass`, `ventcool`, `generator`), plus `cli`.
- boundary: honeybee-openstudio owns the in-process honeybee<->OpenStudio object mapping and the OSM/IDF/epJSON/gbXML serialization. The energy model and the CLI-based `run` rail are `honeybee-energy`; the object model is `honeybee-core`; the standards are `honeybee-standards`; the OpenStudio/EnergyPlus C++ core is the external `openstudio` SDK. The urban consumer that drives this translator per-building is `dragonfly-energy` (its `openstudio` extra pins `honeybee-openstudio`, and `dragonfly_energy.run.base_honeybee_osw`/`run_urbanopt` route each district feature through the OSW->OpenStudio bridge this package owns — the reciprocal seam in `dragonfly-energy.md`). The owner never re-implements the object mapping nor runs the OpenStudio CLI from this package (that is the `honeybee_energy.run` rail).

## [05]-[LOCAL_ADMISSION]

[RAIL_LAW]:
- Package: `honeybee-openstudio`
- Owns: the in-process honeybee `Model` <-> OpenStudio `Model` translation; OSM/IDF/epJSON/gbXML serialization and reverse import; the per-object and per-resource translators; `SimulationParameter` -> OpenStudio translation; EPW/climate-zone assignment; and the `honeybee_openstudio.openstudio` SDK re-export indirection
- Accept: in-process translation feeding the energy-modeling owner when the native `openstudio` SDK is present; the OSW->OpenStudio bridge `dragonfly-energy`'s `openstudio` extra and `run_urbanopt` pipeline drive per district feature; the CPU-bound `model_to_openstudio` offloaded through `anyio.to_thread.run_sync` under an `opentelemetry` span; the translation receipt folded into the `expression` `Result` rail; the SDK referenced exclusively through `honeybee_openstudio.openstudio`
- Reject: a hand-rolled honeybee->OpenStudio object mapping or gbXML/OSM/IDF writer; direct `import openstudio` scattered across translators over the `honeybee_openstudio.openstudio` indirection; running the OpenStudio CLI from this package (that is the `honeybee_energy.run` rail); blocking the event loop on the CPU-bound translation; assuming the native `openstudio` SDK is present without gating on the `honeybee_openstudio.openstudio` import before selecting the in-process rail
- Floor: choose the in-process translator when the native `openstudio` SDK is importable; fall back to the `honeybee_energy.run` external-CLI rail when only the CLI is installed — the two are the in-process/subprocess pair of one translation concept, never two competing owners

[CAPTURE_GAP]:
- the native SDK link is real (verified by `assay api` reflection against the installed `honeybee-openstudio 0.5.5` + its bound native `openstudio` SDK `3.10.0`, both importable on the active interpreter): the honeybee-openstudio wheel is itself `py3-none-any`, but it hard-imports the native `openstudio` SDK at module load through `honeybee_openstudio.openstudio`, and `model_to_openstudio` returns a real `openstudio.openstudiomodelcore.Model` — so any consumer of the return value also links the native SDK, and the owner gates on the `honeybee_openstudio.openstudio` import before selecting the in-process rail.
- in-process vs CLI is a real performance/capability fork: `honeybee_openstudio.writer.model_to_openstudio` translates in-process (no temp folder, no subprocess), whereas `honeybee_energy.run.run_osw` writes an OSW and shells out to the OpenStudio CLI. The in-process path is CPU-bound -> `anyio.to_thread.run_sync` (off-loop CPU), while the CLI path is subprocess-bound -> the runtime lane THREAD band + the runtime `guarded` retry rail. They produce the same OSM/IDF, but only the CLI path runs the actual EnergyPlus simulation; `model_to_openstudio` produces the model for simulation, it does not simulate. `DetailedHVAC` translation specifically requires this OpenStudio measure path.
- the readers depend on resource-table extraction order: `model_from_openstudio` must call `extract_all_schedules` before `extract_all_constructions` (constructions reference schedules) before the per-room readers (rooms reference constructions+schedules) — the model-level `model_from_*` functions own this ordering, so the owner calls them rather than composing the per-object readers by hand. `reset_properties=True` discards inbound OpenStudio `.properties.energy` and rebuilds defaults; `reset_properties=False` (default) merges onto existing honeybee properties.

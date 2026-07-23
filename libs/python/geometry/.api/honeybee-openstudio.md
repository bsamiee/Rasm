# [PY_GEOMETRY_API_HONEYBEE_OPENSTUDIO]

`honeybee-openstudio` translates a `honeybee-core` `Model` carrying `.properties.energy` into a live OpenStudio `Model` and onward to OSM/IDF/epJSON/gbXML, and reads OSM/IDF/gbXML back into honeybee — entirely in-process through the native `openstudio` Python SDK, never an OpenStudio-CLI subprocess. It is the in-process leg of the geometry energy-modeling rail: the complement to the `honeybee_energy.run` external-CLI rail, riding `honeybee-energy` as the spine and reaching every OpenStudio SDK class through the single `honeybee_openstudio.openstudio` re-export.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `honeybee-openstudio`
- package: `honeybee-openstudio` (AGPL-3.0)
- import: `from honeybee_openstudio.writer import model_to_openstudio, model_to_osm, model_to_idf`, `from honeybee_openstudio.reader import model_from_osm_file, model_from_idf_file`, `from honeybee_openstudio.openstudio import OSModel` (the SDK re-export)
- module: `honeybee_openstudio`
- owner: `geometry`
- rail: energy-modeling
- consumer: `.planning/energy/simulate.md` (the `WRITERS` format rows behind the `find_spec("openstudio")` probe, lane-offloaded)
- abi: pure-Python `py3-none-any` wheel that hard-imports the native `openstudio` SDK at module load; that SDK ships a platform-keyed compiled C++ OpenStudio/EnergyPlus wheel (`py3-none-<platform>`, not interpreter-keyed). `model_to_openstudio` returns a real `openstudio.openstudiomodelcore.Model`, so any consumer of the return value also links the native SDK.
- depends-on: the native `openstudio` SDK (the binary OpenStudio/EnergyPlus runtime) is the only unconditional `Requires-Dist`; `honeybee-energy` rides the `base` extra, `honeybee-energy-standards` the `standards` extra
- entry points: `honeybee-openstudio` console script (`honeybee_openstudio.cli:openstudio`); the rail composes the API, not the CLI

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: OpenStudio SDK re-export (`honeybee_openstudio.openstudio`)
- Every OpenStudio C++ class the translators touch re-exports under an `OS`-prefixed alias, isolating the SDK-version coupling to one module; `OSModel` is `openstudio.openstudiomodelcore.Model`. Rows drop the `OS` prefix.

| [INDEX] | [SYMBOL]                                                              | [TYPE_FAMILY]   | [CAPABILITY]                                  |
| :-----: | :-------------------------------------------------------------------- | :-------------- | :-------------------------------------------- |
|  [01]   | `Model`                                                               | model root      | the OpenStudio `Model` the translation builds |
|  [02]   | `Construction` / `ConstructionAirBoundary` / `DefaultConstructionSet` | construction    | construction + default-construction-set       |
|  [03]   | `MasslessOpaqueMaterial` / `Gas` / `GasMixture` / `Blind`             | material        | opaque / gas / blind material objects         |
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
- `model_to_openstudio(model, seed_model=None, ...)` builds the live OpenStudio `Model`; the `model_to_*` family serializes onward over that core, `model_to_gbxml(model, triangulate_non_planar_orphaned=True, full_geometry=False, ...)` through a distinct geometry path. Per-object `*_to_openstudio` legs are owner internals the model writer composes by adjacency.

| [INDEX] | [SURFACE]             | [CALL_SHAPE]     | [CAPABILITY]                                                       |
| :-----: | :-------------------- | :--------------- | :----------------------------------------------------------------- |
|  [01]   | `model_to_openstudio` | honeybee `Model` | translate to a live OpenStudio `Model` (the in-process core)       |
|  [02]   | `model_to_osm`        | honeybee `Model` | translate to the serialized OSM string (consumer owns the write)   |
|  [03]   | `model_to_idf`        | honeybee `Model` | translate to the serialized EnergyPlus IDF string                  |
|  [04]   | `model_to_epjson`     | honeybee `Model` | translate to the serialized EnergyPlus epJSON string               |
|  [05]   | `model_to_gbxml`      | honeybee `Model` | translate to the serialized gbXML string (interoperability export) |

- geometry per-object legs: `room_to_openstudio` `face_to_openstudio` `aperture_to_openstudio` `door_to_openstudio` `shade_to_openstudio` `shade_mesh_to_openstudio`

[ENTRYPOINT_SCOPE]: model readers (`honeybee_openstudio.reader`)
- Reverse import OSM/IDF/gbXML -> honeybee `Model`; readers take `reset_properties=False`, `model_from_osm_file` also `print_warnings=False`. `extract_all_*` pull the resource tables before the per-object `*_from_openstudio` legs rebuild geometry + properties.

| [INDEX] | [SURFACE]                   | [CALL_SHAPE]       | [CAPABILITY]                                 |
| :-----: | :-------------------------- | :----------------- | :------------------------------------------- |
|  [01]   | `model_from_openstudio`     | OpenStudio `Model` | import a live OpenStudio model into honeybee |
|  [02]   | `model_from_osm_file`       | `.osm` file        | import from an OSM file                      |
|  [03]   | `model_from_osm`            | `.osm` string      | import from an OSM string                    |
|  [04]   | `model_from_idf_file`       | `.idf` file        | import from EnergyPlus IDF                   |
|  [05]   | `model_from_gbxml_file`     | gbXML file         | import from gbXML                            |
|  [06]   | `extract_all_constructions` | OpenStudio `Model` | pull the construction resource table         |
|  [07]   | `extract_all_schedules`     | OpenStudio `Model` | pull the schedule resource table             |

- per-object legs: `room_from_openstudio` `face_from_openstudio` `construction_set_from_openstudio` `program_type_from_openstudio` `ideal_air_system_from_openstudio`

[ENTRYPOINT_SCOPE]: simulation parameter translation (`honeybee_openstudio.simulation`)
- `simulation_parameter_to_openstudio(sim_par, seed_model=None)` translates the full sim-par bundle and `assign_epw_to_model(epw_file, os_model, set_climate_zone=False)` attaches the weather file; `os_path`/`os_vector_len` marshal the path/vector arguments the SDK requires.

| [INDEX] | [SURFACE]                            | [CALL_SHAPE]          | [CAPABILITY]                                    |
| :-----: | :----------------------------------- | :-------------------- | :---------------------------------------------- |
|  [01]   | `simulation_parameter_to_openstudio` | `SimulationParameter` | translate the full sim-par bundle to OpenStudio |
|  [02]   | `assign_epw_to_model`                | epw + os_model        | attach EPW and derive the ASHRAE climate zone   |
|  [03]   | `os_path`                            | path                  | SDK path marshalling shim                       |
|  [04]   | `os_vector_len`                      | vector                | SDK vector marshalling shim                     |

- part translators: `run_period_to_openstudio` `design_day_to_openstudio` `sizing_to_openstudio` `shadow_calculation_to_openstudio` `simulation_control_to_openstudio` `simulation_output_to_openstudio`

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- in-process axis: `model_to_openstudio` builds a live OpenStudio `Model` in the same process — no OpenStudio-CLI subprocess, no temp-folder OSW round-trip. It produces the model for simulation; only the external-CLI rail runs the EnergyPlus simulation itself, and `DetailedHVAC` translation requires the OpenStudio measure path.
- SDK-indirection axis: every OpenStudio SDK class is reached through `honeybee_openstudio.openstudio`, meeting the version-sensitive C++ surface at one indirection; the owner gates on that import before selecting the in-process rail.
- writer/reader symmetry axis: `model_to_*` and `model_from_*` are mirror families over one object-kind-keyed translation table — the writer composes the per-object `*_to_openstudio` legs by adjacency, the reader extracts resource tables before the per-object `*_from_openstudio` legs, and the owner calls only the model-level functions.
- serialization-format axis: `model_to_osm`/`model_to_idf`/`model_to_epjson`/`model_to_gbxml` are one format-keyed export family over the `model_to_openstudio` core; the target format is a row, `model_to_gbxml` routing through a distinct geometry path.
- receipt axis: the translation result folds into the `expression` `Result` rail; `reset_properties=True` discards inbound OpenStudio `.properties.energy` and rebuilds defaults, `reset_properties=False` merges onto existing honeybee properties.
- boundary: this package owns the in-process honeybee <-> OpenStudio object mapping and OSM/IDF/epJSON/gbXML serialization, leaving the energy model and CLI `run` rail to `honeybee-energy`, the object model to `honeybee-core`, the standards to `honeybee-standards`, and the OpenStudio/EnergyPlus C++ core to the external `openstudio` SDK. Subpackages `writer`, `reader`, `openstudio`, `simulation`, `cli`, and the per-resource translators mirror honeybee-energy.

[STACKING]:
- `honeybee-energy`(`.api/honeybee-energy.md`): the in-process complement to `honeybee_energy.run.run_osw` — the energy-modeling owner picks `model_to_openstudio` when the native SDK is importable and falls back to the `honeybee_energy.run` external-CLI rail when only the CLI is installed; the two are the in-process/subprocess pair of one translation concept.
- `dragonfly-energy`(`.api/dragonfly-energy.md`): `dragonfly_energy.run.base_honeybee_osw`/`run_urbanopt` route each district feature through the OSW->OpenStudio bridge this package owns, admitting it as a separate runtime companion rather than a pip extra.
- substrate (`libs/python/.api/`): the CPU-bound `model_to_openstudio` offloads through `anyio.to_thread.run_sync` under an `opentelemetry` span — off-loop CPU work, never a subprocess retry.

[RAIL_LAW]:
- Package: `honeybee-openstudio`
- Owns: the in-process honeybee `Model` <-> OpenStudio `Model` translation; OSM/IDF/epJSON/gbXML serialization and reverse import; the per-object and per-resource translators; `SimulationParameter` -> OpenStudio translation; EPW/climate-zone assignment; the `honeybee_openstudio.openstudio` SDK re-export
- Accept: in-process translation feeding the energy-modeling owner when the native `openstudio` SDK is importable; the OSW->OpenStudio bridge driving `dragonfly-energy`'s `run_urbanopt` per district feature; the CPU-bound `model_to_openstudio` offloaded through `anyio.to_thread.run_sync` under an `opentelemetry` span; the translation receipt folded into the `expression` `Result` rail
- Reject: a hand-rolled honeybee->OpenStudio object mapping or gbXML/OSM/IDF writer; `import openstudio` scattered across translators over the `honeybee_openstudio.openstudio` indirection; running the OpenStudio CLI from this package; blocking the event loop on the CPU-bound translation; selecting the in-process rail without gating on the `honeybee_openstudio.openstudio` import

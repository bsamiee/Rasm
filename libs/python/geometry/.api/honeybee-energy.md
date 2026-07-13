# [PY_GEOMETRY_API_HONEYBEE_ENERGY]

`honeybee-energy` is the building-energy extension owner for the geometry energy-modeling rail: it registers `*EnergyProperties` onto every `honeybee-core` object through `_extend_honeybee` (so `model.properties.energy`/`room.properties.energy`/`face.properties.energy` exist), owns the full energy object library (constructions, materials, loads, schedules, program types, HVAC templates, service hot water, generators, internal mass, natural ventilation), the EnergyPlus `SimulationParameter` family, the standards-backed `lib` by-identifier loaders, the EnergyPlus/OpenStudio `run` CLI bridge, and the EnergyPlus SQL/CSV `result` parsers. The energy-modeling owner assigns energy properties through the `.properties.energy` spine, resolves constructions/materials/schedules/program-types by identifier from the standards libraries (the `honeybee-standards` defaults floor plus the optional `honeybee-energy-standards` extension), serializes the energy extension through the abridged HBJSON dict validated by the `honeybee-schema` pydantic-v2 energy models (the universal `pydantic` rail), offloads the blocking `run_idf`/`run_osw` EnergyPlus subprocess through the runtime lane THREAD band under the runtime `guarded` retry rail and the graduation `evidence_run` span weave, and decodes the SQL result rows into the `numpy`/`xarray` data tier — never re-implementing a construction U-value solver, an IDF writer, a schedule expander, or an EnergyPlus result parser the package already owns. It rides `honeybee-core` as the spine and is consumed by `honeybee-openstudio` (the in-process translator) and `dragonfly-energy` (the urban aggregator), never a parallel energy model.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `honeybee-energy`
- package: `honeybee-energy`
- import: `import honeybee_energy` (the `_extend_honeybee` import attaches `.properties.energy`); then `from honeybee_energy.construction.opaque import OpaqueConstruction`, `from honeybee_energy.programtype import ProgramType`, `from honeybee_energy.lib.programtypes import program_type_by_identifier`, `from honeybee_energy.simulation.parameter import SimulationParameter`, `from honeybee_energy.run import to_openstudio_osw, run_osw`
- owner: `geometry`
- rail: energy-modeling
- consumer: `.planning/energy/model.md` (`.properties.energy` assignment, `lib` standards resolution, `HVAC_TYPES_DICT`) + `.planning/energy/simulate.md` (`SimulationParameter`/`SimulationOutput`, the OSW/CLI fall-through, `result.eui`)
- version: `1.109.27`
- license: AGPL-3.0 (Ladybug Tools copyleft)
- abi: pure-Python `py3-none-any` wheel, no compiled payload; the energy simulation itself runs in the external EnergyPlus/OpenStudio CLI, not in-process
- depends-on: `honeybee-core` (the object-model spine) and `honeybee-standards` (the baseline constructions/schedules/programs data backend), with the optional `honeybee-energy-standards` `standards` extra for the large ASHRAE/DOE library; `ladybug-core` arrives transitively through `honeybee-core`; an external EnergyPlus install and the OpenStudio CLI are runtime requirements for the `run` rail (the simulation engine, not a Python dependency)
- entry points: `honeybee-energy` console script (`settings`/`translate`/`simulate`/`baseline`/`result`/`lib` sub-commands)
- capability: energy property assignment via `.properties.energy`; the construction/material/load/schedule/program-type/HVAC/SHW/generator object library; abridged HBJSON serialization; standards by-identifier resolution; ASHRAE 90.1 baseline generation; EnergyPlus `SimulationParameter` assembly; IDF/OSW writing; EnergyPlus/OpenStudio CLI execution; and EnergyPlus SQL/CSV result parsing (EUI, load balance, emissions, generation, comfort matching)

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: constructions (`honeybee_energy.construction.*`)
- rail: energy-modeling
- Layered thermal assemblies assigned to faces/sub-faces. Each carries the `from_dict`/`from_dict_abridged`/`from_idf` triad; abridged forms reference materials by identifier (the wire shape), full forms inline them.

| [INDEX] | [SYMBOL]                    | [TYPE_FAMILY]   | [CAPABILITY]                                                     |
| :-----: | :-------------------------- | :-------------- | :--------------------------------------------------------------- |
|  [01]   | `OpaqueConstruction`        | opaque assembly | layered opaque construction; `from_simple_parameters`, U/R-value |
|  [02]   | `WindowConstruction`        | window assembly | layered glazing+gas+frame construction                           |
|  [03]   | `WindowConstructionShade`   | dynamic window  | window with a switchable interior/exterior shade + control       |
|  [04]   | `WindowConstructionDynamic` | dynamic window  | schedule-switched window-construction states (electrochromic)    |
|  [05]   | `AirBoundaryConstruction`   | air boundary    | air-mixing construction for `AirBoundary` faces                  |
|  [06]   | `ShadeConstruction`         | shade surface   | solar/visible reflectance + specularity for shades               |

[PUBLIC_TYPE_SCOPE]: materials (`honeybee_energy.material.*`)
- rail: energy-modeling
- The leaf thermal materials constructions layer. Each carries `from_dict`/`from_idf`.

| [INDEX] | [SYMBOL]                                                            | [TYPE_FAMILY]   | [CAPABILITY]                                    |
| :-----: | :------------------------------------------------------------------ | :-------------- | :---------------------------------------------- |
|  [01]   | `EnergyMaterial` / `EnergyMaterialNoMass`                           | opaque material | mass / massless (R-value-only) opaque layer     |
|  [02]   | `EnergyMaterialVegetation`                                          | opaque material | green-roof soil+plant layer                     |
|  [03]   | `EnergyWindowMaterialGlazing` / `EnergyWindowMaterialSimpleGlazSys` | glazing         | spectral glazing pane / simple U-SHGC-VT system |
|  [04]   | `EnergyWindowMaterialGas` / `GasCustom` / `GasMixture`              | gas gap         | standard gas / custom-coefficient / mixture gap |
|  [05]   | `EnergyWindowFrame`                                                 | frame           | window frame conductance/projection             |
|  [06]   | `EnergyWindowMaterialShade` / `EnergyWindowMaterialBlind`           | shade material  | roller shade / louvered blind layer             |

[PUBLIC_TYPE_SCOPE]: loads, schedules, and program types (`honeybee_energy.load.*`, `.schedule.*`, `.programtype`)
- rail: energy-modeling
- Loads are per-room internal gains/flows; schedules drive their temporal profile; a `ProgramType` bundles the full reusable load set — people, lighting, equipment, infiltration, ventilation, setpoint, SHW. Loads carry `from_dict`/`from_dict_abridged`/`from_idf` (abridged references schedules by identifier).

| [INDEX] | [SYMBOL]                                                     | [TYPE_FAMILY] | [CAPABILITY]                                            |
| :-----: | :----------------------------------------------------------- | :------------ | :------------------------------------------------------ |
|  [01]   | `People` / `Lighting` / `ElectricEquipment` / `GasEquipment` | internal gain | occupancy / lighting / plug+gas equipment gains         |
|  [02]   | `Infiltration` / `Ventilation`                               | air flow      | envelope infiltration / outdoor-air ventilation         |
|  [03]   | `Setpoint`                                                   | control       | heating/cooling/humidity setpoint schedules             |
|  [04]   | `ServiceHotWater` / `Process` / `DaylightingControl`         | load          | SHW demand / process load / daylight dimming sensor     |
|  [05]   | `ScheduleRuleset` / `ScheduleFixedInterval`                  | schedule      | rule-based (day+rule+holiday) / 8760-value schedule     |
|  [06]   | `ScheduleDay` / `ScheduleRule` / `ScheduleTypeLimit`         | schedule part | day profile / applicability rule / value-range limit    |
|  [07]   | `ProgramType`                                                | load bundle   | the reusable per-program load bundle assigned to a room |

[PUBLIC_TYPE_SCOPE]: assemblies, HVAC, and simulation (`.constructionset`, `.hvac.*`, `.simulation.*`)
- rail: energy-modeling
- A `ConstructionSet` is the per-room default construction lookup (with `Wall`/`Floor`/`RoofCeiling`/`Aperture`/`Door` sub-sets); HVAC templates register in the 20-entry `HVAC_TYPES_DICT`; the `SimulationParameter` family is the EnergyPlus run config, bundling `SimulationOutput`/`RunPeriod`/`SimulationControl`/`SizingParameter`/`ShadowCalculation`/`DaylightSavingTime`.

| [INDEX] | [SYMBOL]              | [TYPE_FAMILY]       | [CAPABILITY]                                                                      |
| :-----: | :-------------------- | :------------------ | :-------------------------------------------------------------------------------- |
|  [01]   | `ConstructionSet`     | construction lookup | per-face-type default construction resolution                                     |
|  [02]   | `IdealAirSystem`      | HVAC                | the default ideal-loads system (no equipment sizing)                              |
|  [03]   | `DetailedHVAC`        | HVAC                | an OpenStudio-measure-backed detailed HVAC spec                                   |
|  [04]   | `HVAC_TYPES_DICT`     | HVAC registry       | 20 entries: `IdealAirSystem`, `DetailedHVAC`, 18 `equipment_type`-keyed templates |
|  [05]   | `SimulationParameter` | run config          | the EnergyPlus run-configuration bundle (assembled from the sim-par parts)        |

[PUBLIC_TYPE_SCOPE]: extension properties (`honeybee_energy.properties.*`)
- rail: energy-modeling
- The `*EnergyProperties` hosts registered onto the core `*Properties` via `_extend_honeybee`; the energy-modeling owner reads `obj.properties.energy` and folds the per-object assignment through one extension table. The rows name the host by its object prefix — the `EnergyProperties` suffix is elided.

| [INDEX] | [SYMBOL]                     | [CAPABILITY]                                                                                        |
| :-----: | :--------------------------- | :-------------------------------------------------------------------------------------------------- |
|  [01]   | `Model`                      | model-level resource collections; `check_all`, `apply_properties_from_dict`, autocalculate/baseline |
|  [02]   | `Room`                       | program/construction-set/HVAC/SHW/setpoint assignment; `absolute_*` loads, `add_default_ideal_air`  |
|  [03]   | `Face` / `Aperture` / `Door` | per-face construction assignment; `u_factor`/`r_factor`/`shgc`, `reset_construction_to_set`         |
|  [04]   | `Shade`                      | shade transmittance schedule + construction                                                         |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: serialization triad and polymorphic decode
- rail: energy-modeling
- Every energy object carries `from_dict` (full, inlined) and most carry `from_dict_abridged` (references resources by identifier — the compact HBJSON wire shape) and `from_idf`. The `dict_to_*` rows elide the shared `honeybee_energy.<domain>.dictutil.` prefix; construction and schedule additionally expose `dict_abridged_to_construction` / `dict_abridged_to_schedule` for the abridged wire shape.

| [INDEX] | [SURFACE]                                          | [CALL_SHAPE]         | [CAPABILITY]                                          |
| :-----: | :------------------------------------------------- | :------------------- | :---------------------------------------------------- |
|  [01]   | `<obj>.from_dict`                                  | dict                 | full reconstruction, resources inlined                |
|  [02]   | `<obj>.from_dict_abridged(data, *resource_dicts)`  | dict + resource maps | identifier-referenced reconstruction                  |
|  [03]   | `<obj>.from_idf(idf_string, ...)`                  | IDF text             | parse an EnergyPlus IDF object into the type          |
|  [04]   | `construction.dictutil.dict_to_construction`       | dict (+ resources)   | polymorphic construction decode (opaque/window/shade) |
|  [05]   | `material.dictutil.dict_to_material`               | dict                 | polymorphic material decode                           |
|  [06]   | `load.dictutil.dict_to_load`                       | dict                 | polymorphic load decode                               |
|  [07]   | `schedule.dictutil.dict_to_schedule`               | dict (+ type-limits) | polymorphic schedule decode                           |
|  [08]   | `ModelEnergyProperties.apply_properties_from_dict` | model HBJSON dict    | re-attach energy properties after a geometry load     |

[ENTRYPOINT_SCOPE]: standards library by-identifier loaders (`honeybee_energy.lib.*`)
- rail: energy-modeling
- The single resolution path into the standards data backend; the owner resolves by identifier and caches, never hand-parsing the standards JSON. At import the loaders seed the small `honeybee-standards` defaults floor (16 constructions, 16 materials, 1 construction set, 2 program types, 8 schedules) and then merge any installed `honeybee-energy-standards` extension from `folders.standards_extension_folders`; the module registries (`OPAQUE_CONSTRUCTIONS`, `OPAQUE_MATERIALS`, `CONSTRUCTION_SETS`, `PROGRAM_TYPES`, `SCHEDULES`) enumerate the union of whatever is installed (the large ASHRAE/DOE counts appear only with the extension present).

| [INDEX] | [SURFACE]                             | [CALL_SHAPE]      | [CAPABILITY]                                          |
| :-----: | :------------------------------------ | :---------------- | :---------------------------------------------------- |
|  [01]   | `opaque_construction_by_identifier`   | identifier string | resolve a standard opaque construction                |
|  [02]   | `window_construction_by_identifier`   | identifier string | resolve a standard window construction                |
|  [03]   | `shade_construction_by_identifier`    | identifier string | resolve a standard shade construction                 |
|  [04]   | `opaque_material_by_identifier`       | identifier string | resolve a standard opaque material                    |
|  [05]   | `window_material_by_identifier`       | identifier string | resolve a standard window material                    |
|  [06]   | `construction_set_by_identifier`      | identifier string | resolve a construction set (per climate zone/vintage) |
|  [07]   | `program_type_by_identifier`          | identifier string | resolve a standard program type by identifier         |
|  [08]   | `building_program_type_by_identifier` | building type     | resolve a whole-building DOE-prototype program        |
|  [09]   | `schedule_by_identifier`              | identifier string | resolve a standard schedule                           |

[ENTRYPOINT_SCOPE]: energy assignment surface (`ModelEnergyProperties`, `RoomEnergyProperties`)
- rail: energy-modeling
- Assignment is the room/model property surface; the owner composes these rather than poking object fields. Rows drop the class prefix, named in the [OWNER] column: `RoomEnergyProperties.` for Room, `ModelEnergyProperties.` for Model.

| [INDEX] | [OWNER] | [SURFACE]                                            | [CALL_SHAPE] | [CAPABILITY]                                       |
| :-----: | :------ | :--------------------------------------------------- | :----------- | :------------------------------------------------- |
|  [01]   | Room    | `program_type` / `construction_set` / `hvac` / `shw` | assignment   | assign program/construction-set/HVAC/SHW to a room |
|  [02]   | Room    | `add_default_ideal_air`                              | none         | attach the default ideal-air system                |
|  [03]   | Room    | `add_daylight_control_to_center`                     | args         | attach a center daylight-control sensor            |
|  [04]   | Room    | `add_internal_mass`                                  | args         | attach internal thermal mass                       |
|  [05]   | Room    | `add_process_load`                                   | args         | attach a process load                              |
|  [06]   | Room    | `absolute_people`                                    | property     | occupancy resolved to an absolute total            |
|  [07]   | Room    | `absolute_lighting`                                  | property     | lighting resolved to an absolute total             |
|  [08]   | Room    | `absolute_ventilation`                               | property     | ventilation resolved to an absolute total          |
|  [09]   | Room    | `absolute_infiltration`                              | property     | infiltration resolved to an absolute total         |
|  [10]   | Model   | `check_all(raise_exception=True, detailed=False)`    | flags        | run every energy-validity check                    |
|  [11]   | Model   | `aperture_constructions`                             | none         | collect the model aperture constructions           |
|  [12]   | Model   | `assign_dynamic_aperture_groups`                     | none         | auto-derive dynamic aperture groups                |
|  [13]   | Model   | `autocalculate_ventilation_simulation_control`       | none         | auto-derive the ventilation simulation control     |

[ENTRYPOINT_SCOPE]: simulation run bridge (`honeybee_energy.run`)
- rail: energy-modeling
- The blocking EnergyPlus/OpenStudio CLI boundary. `run_osw(osw_json, measures_only=True, silent=False)` and `run_idf(idf_file_path, epw_file_path=None, expand_objects=True, silent=False)` shell out to the external engine, and `to_openstudio_osw(osw_directory, model_path, sim_par_json_path=None)` builds the OSW; the owner offloads them through the runtime lane THREAD band under the runtime `guarded` retry rail and the graduation `evidence_run` span weave, never blocking the event loop.

| [INDEX] | [SURFACE]                       | [CALL_SHAPE]           | [CAPABILITY]                                         |
| :-----: | :------------------------------ | :--------------------- | :--------------------------------------------------- |
|  [01]   | `to_openstudio_osw`             | dirs + model + sim-par | build the OpenStudio Workflow (`.osw`)               |
|  [02]   | `run_osw`                       | osw path               | run the OpenStudio CLI on the workflow (blocking)    |
|  [03]   | `run_idf`                       | idf + epw              | run the EnergyPlus CLI directly (blocking)           |
|  [04]   | `to_empty_osm_osw`              | dir + sim-par + epw    | OSW emitting an empty OSM seeded from a sim-par JSON |
|  [05]   | `to_gbxml_osw`                  | model path             | gbXML export workflow                                |
|  [06]   | `to_sdd_osw`                    | model path             | SDD export workflow                                  |
|  [07]   | `measure_compatible_model_json` | model path             | measure-ready model JSON                             |
|  [08]   | `trace_compatible_model_json`   | model path             | TRACE-ready model JSON                               |
|  [09]   | `prepare_idf_for_simulation`    | paths                  | IDF prep for simulation                              |
|  [10]   | `output_energyplus_files`       | paths                  | discover the EnergyPlus output files                 |
|  [11]   | `from_osm_osw`                  | paths                  | reverse-import workflow from OSM                     |
|  [12]   | `from_idf_osw`                  | paths                  | reverse-import workflow from IDF                     |
|  [13]   | `from_gbxml_osw`                | paths                  | reverse-import workflow from gbXML                   |

[ENTRYPOINT_SCOPE]: result parsers (`honeybee_energy.result.*`)
- rail: energy-modeling
- Parse the EnergyPlus SQLite (`eplusout.sql`) and CSV outputs into Python rows; the owner decodes these into the `numpy`/`xarray` data tier, never hand-parsing the SQL schema. Rows elide the shared `result.` package prefix.

| [INDEX] | [SURFACE]                                | [CALL_SHAPE]   | [CAPABILITY]                                  |
| :-----: | :--------------------------------------- | :------------- | :-------------------------------------------- |
|  [01]   | `eui.eui_from_sql`                       | sql path(s)    | energy-use-intensity breakdown by end use     |
|  [02]   | `emissions.emissions_from_sql`           | sql + region   | carbon-emissions intensity from grid factors  |
|  [03]   | `emissions.emissions_region`             | region         | resolve the grid emissions region             |
|  [04]   | `emissions.future_electricity_emissions` | region         | projected future electricity emission factors |
|  [05]   | `generation.generation_data_from_sql`    | sql path       | on-site PV/generation timeseries              |
|  [06]   | `generation.generation_summary_from_sql` | sql path       | on-site PV/generation summary                 |
|  [07]   | `match.match_rooms_to_data`              | objects + data | join result series onto honeybee rooms        |
|  [08]   | `match.match_faces_to_data`              | objects + data | join result series onto honeybee faces        |
|  [09]   | `loadbalance`                            | sql file       | load-balance parser                           |
|  [10]   | `err`                                    | err file       | error-file parser                             |
|  [11]   | `rdd`                                    | rdd file       | report-data-dictionary parser                 |
|  [12]   | `zsz`                                    | zsz file       | zone-sizing parser                            |

## [04]-[IMPLEMENTATION_LAW]

[ENERGY_MODELING_EXTENSION]:
- import: `import honeybee_energy` at boundary scope; the import side-effect (`_extend_honeybee`) registers `*EnergyProperties` onto the core `*Properties` classes, so `.properties.energy` exists only after the extension is imported. The owner imports it once at the energy boundary, never lazily per call.
- extension axis: energy property assignment routes through `obj.properties.energy` (the `_extend_honeybee` spine from honeybee-core), never a parallel object graph. `ModelEnergyProperties` owns the model-level resource collections (constructions/materials/schedules/program-types/HVAC); `RoomEnergyProperties` owns per-room assignment and the `absolute_*` resolved-load accessors. The owner folds per-object energy assignment through one extension-keyed table and serializes via the core `to_dict(included_prop=['energy'])` selector.
- object-library axis: constructions/materials/loads/schedules/program-types/HVAC are bounded type families each carrying the `from_dict`/`from_dict_abridged`/`from_idf` triad. The abridged form is the compact HBJSON wire shape (resources referenced by identifier); the per-domain `dictutil.dict_to_*` are the polymorphic decoders dispatching on the dict `type`. The owner never writes a per-construction-kind or per-load-kind decode branch — `dict_to_construction`/`dict_to_load`/`dict_to_schedule` own the dispatch.
- standards axis: the `lib.*_by_identifier` loaders are the SINGLE access path into the standards data backend — they resolve a standard construction/material/schedule/program-type/construction-set by identifier and cache it. At import they seed the small `honeybee-standards` defaults floor (`folders.defaults_file`: 16 constructions, 2 program types, 8 schedules, 1 construction set) and then scan `folders.standards_extension_folders` for the optional `honeybee-energy-standards` extension (the large ASHRAE 90.1 / DOE-prototype library). The registries enumerate the union of whatever is installed, so the large counts are the extension's contribution, not honeybee-standards'. The owner resolves by identifier; it never reads the standards JSON files directly nor re-ships the standard library.
- run axis: `run_idf`/`run_osw` invoke the external EnergyPlus/OpenStudio CLI — a blocking subprocess, not in-process compute. The owner brackets each run through `anyio.to_thread.run_sync` (the same blocking-offload idiom the sibling `compas.rpc.Proxy` uses), retries the transient subprocess/IO failure under the runtime `guarded` retry rail, and rides the graduation `evidence_run` span weave; the run receipt (success, output file paths, err-file diagnostics) folds into the `expression` `Result` rail. `to_openstudio_osw(osw_directory, model_path, sim_par_json_path=...)` is the one-call OSW assembly the owner prefers over hand-stitching the `SimulationParameter` JSON + workflow.
- result axis: the `result.*_from_sql` parsers decode the EnergyPlus SQLite output into Python rows; the owner promotes those rows into the `numpy`/`xarray` data tier for downstream analysis, and `match_rooms_to_data`/`match_faces_to_data` join the result series back onto the honeybee geometry for visualization. The owner never hand-parses the EnergyPlus SQL schema.
- validation axis: `ModelEnergyProperties.check_all(detailed=True)` returns the energy-specific error rows (one-HVAC-per-zone, air-boundary-with-window, detailed-HVAC room coverage, duplicate resource ids) folded into the same `Result` receipt as the core geometric checks; a single `Model.check_all(detailed=True)` runs both the geometric and energy checks in one pass because honeybee-core invokes each registered extension's `check_all` automatically.
- subpackages: `construction`, `material`, `load`, `schedule`, `programtype`, `constructionset`, `hvac` (allair/doas/heatcool/idealair/detailed templates), `shw`, `generator`, `internalmass`, `ventcool`, `simulation`, `properties`, `lib`, `run`, `result`, `measure` (OpenStudio measure wrapper), `baseline` (ASHRAE 90.1 baseline generation), `reader`, `writer`, `dictutil`, `cli`.
- boundary: honeybee-energy owns the energy model, the standards resolution, the simulation orchestration, and the result parsing. The object model and HBJSON are `honeybee-core`; the standards data is `honeybee-standards`; the in-process OpenStudio/EnergyPlus translation (no subprocess) is `honeybee-openstudio`; urban aggregation is `dragonfly-energy`; HBJSON energy validation is `honeybee-schema.energy` (pydantic v2). The EnergyPlus/OpenStudio simulation engines are external CLIs, not Python code.

## [05]-[LOCAL_ADMISSION]

[RAIL_LAW]:
- Package: `honeybee-energy`
- Owns: the `.properties.energy` extension; the construction/material/load/schedule/program-type/HVAC/SHW/generator/internal-mass/ventilation object library; abridged HBJSON serialization and the per-domain polymorphic decoders; the `lib` standards by-identifier loaders; ASHRAE 90.1 baseline generation; the `SimulationParameter` family; the EnergyPlus/OpenStudio `run` CLI bridge; and the EnergyPlus SQL/CSV `result` parsers
- Accept: energy-property assignment feeding the energy-modeling owner; standards resolution through `lib.*_by_identifier`; abridged HBJSON validated through `honeybee-schema.energy` (the `pydantic` rail); the `run_idf`/`run_osw` subprocess offloaded through the runtime lane THREAD band under `guarded` + the `evidence_run` weave; result rows promoted to the `numpy`/`xarray` data tier; `check_all(detailed=True)` folded into the `expression` `Result` rail
- Reject: a parallel energy object model outside `.properties.energy`; a hand-rolled construction U-value solver, IDF writer, schedule expander, or EnergyPlus result/SQL parser; direct reads of the `honeybee-standards` JSON over the `lib.*_by_identifier` loaders; a per-construction/per-load decode branch over the `dictutil.dict_to_*` dispatchers; blocking the event loop on `run_idf`/`run_osw`; re-shipping or re-deriving the standard construction/program/schedule library

[CAPTURE_GAP]:
- the abridged-vs-full dict split is load-bearing (verified against `honeybee-energy 1.109.27`): `from_dict_abridged` takes the object dict PLUS the resolved resource maps it references by identifier (e.g. `OpaqueConstruction.from_dict_abridged(data, materials)` where `materials` is the identifier->material map), while `from_dict` expects every resource inlined. The HBJSON wire carries the abridged form with a model-level resource table, so model decode resolves the resource maps first (via `dict_abridged_to_*`) and feeds them into the per-object abridged decoders — a flat `from_dict` over an abridged dict raises a `KeyError` on the missing inlined resource. `ModelEnergyProperties.apply_properties_from_dict(data)` performs this ordered resolution internally.
- the `lib` registry universe is layered, not a fixed constant (verified against `honeybee-energy 1.109.27`): the loaders ALWAYS seed the small `honeybee-standards` defaults floor (`folders.defaults_file` -> 16 constructions, 16 materials, 2 program types, 8 schedules, 1 construction set, 9 type limits) and ADDITIONALLY load every library found in `folders.standards_extension_folders`. The large ASHRAE/DOE counts (≈1845 program types, ≈3347 schedules, ≈1039 constructions, ≈256 climate-zone/vintage construction sets — verified against `honeybee-energy-standards 2.3.0`, see `honeybee-energy-standards.md`) appear ONLY when the separate `honeybee-energy-standards` extension is installed there — they are the extension's contribution, never honeybee-standards'. So a registry count is a function of which standards packages are installed; the owner must not assume the extension is present, and `building_program_type_by_identifier(building_type)` (the whole-building DOE-prototype resolver) requires the extension, distinct from `program_type_by_identifier` which resolves any loaded program identifier (defaults included).
- the HVAC template surface is registry-driven, not import-driven: `IdealAirSystem` and `DetailedHVAC` are directly importable, but the 18 template systems (`VAV`/`PVAV`/`PSZ`/`PTAC`/`FCU`/`VRF`/`WSHP`/`Baseboard`/`Radiant`/`ForcedAirFurnace`/`GasUnitHeater`/`EvaporativeCooler`/`WindowAC`/`Residential` + the four `FCU`/`Radiant`/`VRF`/`WSHP` `withDOAS` variants) are dynamically built subclasses registered in `honeybee_energy.hvac.HVAC_TYPES_DICT` (which also holds `IdealAirSystem` and `DetailedHVAC`, 20 entries total), each with an enumerated `equipment_type` vocabulary selecting the vintage/efficiency variant. The owner resolves a template by `HVAC_TYPES_DICT[name]` and sets `equipment_type`, never importing a per-template class. `DetailedHVAC` requires the OpenStudio measure path (it round-trips through `honeybee-openstudio`/the OpenStudio CLI), so it is unavailable to the pure EnergyPlus `run_idf` path.
- `run_idf`/`run_osw` return file paths (sql/err/eso/html) on success and depend on an external EnergyPlus/OpenStudio install discovered through `honeybee_energy.config.folders`; a missing engine is a runtime fault, not a Python `ImportError`, so the owner's `run` rail checks `folders.energyplus_exe`/`folders.openstudio_exe` before bracketing the subprocess.

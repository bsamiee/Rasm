# [PY_GEOMETRY_API_HONEYBEE_ENERGY]

`honeybee-energy` owns the building-energy extension on the geometry energy-modeling domain: `import honeybee_energy` fires `_extend_honeybee` to register `*EnergyProperties` onto every `honeybee-core` object, then it owns the energy object library, abridged HBJSON serialization, the `lib` by-identifier standards loaders, the `SimulationParameter` family, the `run` CLI bridge, and the SQL/CSV `result` parsers. It rides `honeybee-core` as the spine and feeds `honeybee-openstudio` and `dragonfly-energy`, while standards data, HBJSON validation, and the simulation engine stay in siblings.

## [01]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: constructions (`honeybee_energy.construction.*`)

Layered thermal assemblies assigned to faces/sub-faces. Each carries the `from_dict`/`from_dict_abridged`/`from_idf` triad; abridged forms reference materials by identifier (the wire shape), full forms inline them.

| [INDEX] | [SYMBOL]                    | [TYPE_FAMILY]   | [CAPABILITY]                                                     |
| :-----: | :-------------------------- | :-------------- | :--------------------------------------------------------------- |
|  [01]   | `OpaqueConstruction`        | opaque assembly | layered opaque construction; `from_simple_parameters`, U/R-value |
|  [02]   | `WindowConstruction`        | window assembly | layered glazing+gas+frame construction                           |
|  [03]   | `WindowConstructionShade`   | dynamic window  | window with a switchable interior/exterior shade + control       |
|  [04]   | `WindowConstructionDynamic` | dynamic window  | schedule-switched window-construction states (electrochromic)    |
|  [05]   | `AirBoundaryConstruction`   | air boundary    | air-mixing construction for `AirBoundary` faces                  |
|  [06]   | `ShadeConstruction`         | shade surface   | solar/visible reflectance + specularity for shades               |

[PUBLIC_TYPE_SCOPE]: materials (`honeybee_energy.material.*`)

Leaf thermal materials that constructions layer. Each carries `from_dict`/`from_idf`.

| [INDEX] | [SYMBOL]                                                            | [TYPE_FAMILY]   | [CAPABILITY]                                    |
| :-----: | :------------------------------------------------------------------ | :-------------- | :---------------------------------------------- |
|  [01]   | `EnergyMaterial` / `EnergyMaterialNoMass`                           | opaque material | mass / massless (R-value-only) opaque layer     |
|  [02]   | `EnergyMaterialVegetation`                                          | opaque material | green-roof soil+plant layer                     |
|  [03]   | `EnergyWindowMaterialGlazing` / `EnergyWindowMaterialSimpleGlazSys` | glazing         | spectral glazing pane / simple U-SHGC-VT system |
|  [04]   | `EnergyWindowMaterialGas` / `GasCustom` / `GasMixture`              | gas gap         | standard gas / custom-coefficient / mixture gap |
|  [05]   | `EnergyWindowFrame`                                                 | frame           | window frame conductance/projection             |
|  [06]   | `EnergyWindowMaterialShade` / `EnergyWindowMaterialBlind`           | shade material  | roller shade / louvered blind layer             |

[PUBLIC_TYPE_SCOPE]: loads, schedules, and program types (`honeybee_energy.load.*`, `.schedule.*`, `.programtype`)

Loads are per-room internal gains/flows; schedules drive their temporal profile; a `ProgramType` bundles the full reusable per-room load set. Loads carry `from_dict`/`from_dict_abridged`/`from_idf` (abridged references schedules by identifier).

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

A `ConstructionSet` resolves the per-face-type default construction; HVAC templates register in `HVAC_TYPES_DICT`; the `SimulationParameter` family is the EnergyPlus run-configuration bundle.

| [INDEX] | [SYMBOL]              | [TYPE_FAMILY]       | [CAPABILITY]                                                               |
| :-----: | :-------------------- | :------------------ | :------------------------------------------------------------------------- |
|  [01]   | `ConstructionSet`     | construction lookup | per-face-type default construction resolution                              |
|  [02]   | `IdealAirSystem`      | HVAC                | the default ideal-loads system (no equipment sizing)                       |
|  [03]   | `DetailedHVAC`        | HVAC                | an OpenStudio-measure-backed detailed HVAC spec                            |
|  [04]   | `HVAC_TYPES_DICT`     | HVAC registry       | `IdealAirSystem`, `DetailedHVAC`, and the `equipment_type`-keyed templates |
|  [05]   | `SimulationParameter` | run config          | the EnergyPlus run-configuration bundle (assembled from the sim-par parts) |

- `HVAC_TYPES_DICT`: registered template systems are dynamically built subclasses, each carrying an enumerated `equipment_type` vocabulary that selects the vintage/efficiency variant. Resolve a template by `HVAC_TYPES_DICT[name]` and set `equipment_type`, never importing a per-template class; `DetailedHVAC` requires the OpenStudio measure path and is unavailable to the pure `run_idf` route.

[PUBLIC_TYPE_SCOPE]: extension properties (`honeybee_energy.properties.*`)

`*EnergyProperties` hosts registered onto the core `*Properties` via `_extend_honeybee`; the owner reads `obj.properties.energy` and folds per-object assignment through one extension table. Rows name the host by its object prefix — the `EnergyProperties` suffix is elided.

| [INDEX] | [SYMBOL]                     | [CAPABILITY]                                                                                        |
| :-----: | :--------------------------- | :-------------------------------------------------------------------------------------------------- |
|  [01]   | `Model`                      | model-level resource collections; `check_all`, `apply_properties_from_dict`, autocalculate/baseline |
|  [02]   | `Room`                       | program/construction-set/HVAC/SHW/setpoint assignment; `absolute_*` loads, `add_default_ideal_air`  |
|  [03]   | `Face` / `Aperture` / `Door` | per-face construction assignment; `u_factor`/`r_factor`/`shgc`, `reset_construction_to_set`         |
|  [04]   | `Shade`                      | shade transmittance schedule + construction                                                         |

## [02]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: serialization triad and polymorphic decode

Every energy object carries `from_dict` (full, inlined) and most carry `from_dict_abridged` (references resources by identifier — the compact HBJSON wire shape) and `from_idf`. `dict_to_*` rows elide the shared `honeybee_energy.<domain>.dictutil.` prefix; construction and schedule expose `dict_abridged_to_construction` / `dict_abridged_to_schedule` for the abridged wire shape.

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

- `from_dict_abridged`/`from_dict`: `from_dict_abridged(data, materials)` takes the object dict and the identifier-keyed resource maps it references, while `from_dict` expects every resource inlined — a flat `from_dict` over an abridged dict raises `KeyError` on the missing resource. HBJSON wire carries the abridged form with a model-level resource table, so `apply_properties_from_dict(data)` resolves the maps via `dict_abridged_to_*` and feeds the per-object abridged decoders in order.

[ENTRYPOINT_SCOPE]: standards library by-identifier loaders (`honeybee_energy.lib.*`)

Single resolution path into the standards data backend; the owner resolves by identifier and caches, never hand-parsing the standards JSON. At import the loaders seed the `honeybee-standards` defaults floor and merge any installed `honeybee-energy-standards` extension from `folders.standards_extension_folders`; the module registries (`OPAQUE_CONSTRUCTIONS`, `OPAQUE_MATERIALS`, `CONSTRUCTION_SETS`, `PROGRAM_TYPES`, `SCHEDULES`) enumerate the union installed.

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

- `lib` registry universe is layered, not fixed: the loaders always seed the `honeybee-standards` defaults floor via `folders.defaults_file` and load every library under `folders.standards_extension_folders`, so a registry count is a function of which standards packages are installed. `building_program_type_by_identifier` requires the `honeybee-energy-standards` extension; `program_type_by_identifier` resolves any loaded identifier including defaults, and the owner never assumes the extension present.

[ENTRYPOINT_SCOPE]: energy assignment surface (`ModelEnergyProperties`, `RoomEnergyProperties`)

Assignment is the room/model property surface; the owner composes these rather than poking object fields. Rows drop the class prefix named in the [OWNER] column: `RoomEnergyProperties.` for Room, `ModelEnergyProperties.` for Model.

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

Blocking EnergyPlus/OpenStudio CLI boundary; `run_osw`/`run_idf` shell out to the external engine and `to_openstudio_osw` builds the OSW. Owner offloads every run off the event loop, never blocking it (the offload path is `[STACKING]`).

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

- `run_idf`/`run_osw`: return output file paths on success and depend on an external EnergyPlus/OpenStudio install discovered through `honeybee_energy.config.folders`; a missing engine is a runtime fault, not an `ImportError`, so the `run` layer checks `folders.energyplus_exe`/`folders.openstudio_exe` before bracketing the subprocess.

[ENTRYPOINT_SCOPE]: result parsers (`honeybee_energy.result.*`)

Parse the EnergyPlus SQLite (`eplusout.sql`) and CSV outputs into Python rows; the owner decodes these into the `numpy`/`xarray` data tier, never hand-parsing the SQL schema. Rows elide the shared `result.` package prefix.

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

[ENTRYPOINT_SCOPE]: ASHRAE 90.1 baseline generation and performance rating (`honeybee_energy.baseline.*`)

Convert a proposed model into its Appendix G baseline and rate the simulated pair; the named standard IS the authority a compliance verdict cites, so a consuming owner names one of these rows rather than a bare ceiling. Rows elide the shared `baseline.` package prefix and their surface cells carry required parameters alone.

- create carry: `building_type` (default `NonResidential`) wherever the row takes it; `model_to_baseline` and `model_hvac_to_baseline` add `floor_area` and `story_count`, and `model_to_baseline` adds `lighting_by_building`
- pci/result carry: `electricity_cost`, `natural_gas_cost`, `district_cooling_cost`, `district_heating_cost`, and `electricity_emissions` — `result.appendix_g_summary` alone omits the emissions factor; every one reads the `data/` csv tables (`pci_2016`/`pci_2019`/`pci_2022`, `constructions`, `fen_ratios`, `lpd_building`, `shw`), a folder with no callable surface

| [INDEX] | [SURFACE]                                                              | [SHAPE] | [CAPABILITY]                                         |
| :-----: | :--------------------------------------------------------------------- | :------ | :--------------------------------------------------- |
|  [01]   | `create.model_to_baseline(model, climate_zone)`                        | static  | composes rows [02]-[07] into one call                |
|  [02]   | `create.model_geometry_to_baseline(model)`                             | static  | baseline window-wall ratio and orientation           |
|  [03]   | `create.model_constructions_to_baseline(model, climate_zone)`          | static  | envelope constructions for the zone                  |
|  [04]   | `create.model_hvac_to_baseline(model, climate_zone)`                   | static  | baseline HVAC per the Appendix G system map          |
|  [05]   | `create.model_lighting_to_baseline(model)`                             | static  | space-by-space baseline lighting power density       |
|  [06]   | `create.model_lighting_to_baseline_building(model)`                    | static  | whole-building baseline lighting power density       |
|  [07]   | `create.model_shw_to_baseline(model)`                                  | static  | baseline service-hot-water system                    |
|  [08]   | `create.model_remove_ecms(model)`                                      | static  | strip the ECMs a baseline excludes                   |
|  [09]   | `pci.pci_target_from_baseline_sql(sql_results, climate_zone)`          | static  | target Performance Cost Index off the vintage tables |
|  [10]   | `pci.energy_cost_from_proposed_sql(sql_result)`                        | static  | proposed energy cost and emissions from one sql      |
|  [11]   | `pci.comparison_from_sql(proposed_sql, baseline_sqls, climate_zone)`   | static  | the baseline-versus-proposed cost comparison         |
|  [12]   | `result.appendix_g_summary(proposed_sql, baseline_sqls, climate_zone)` | static  | ASHRAE 90.1 Appendix G performance summary           |
|  [13]   | `result.leed_v4_summary(proposed_sql, baseline_sqls, climate_zone)`    | static  | LEED v4 / v4.1 energy-performance summary            |

## [03]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `.properties.energy` exists only after `import honeybee_energy` fires `_extend_honeybee`; energy assignment routes through `obj.properties.energy`, `ModelEnergyProperties` owning the model-level resource collections and `RoomEnergyProperties` per-room assignment and the `absolute_*` resolved-load accessors, folded through one extension-keyed table and serialized via `to_dict(included_prop=['energy'])`, never a parallel object graph.
- constructions/materials/loads/schedules/program-types/HVAC are bounded type families carrying the `from_dict`/`from_dict_abridged`/`from_idf` triad; the per-domain `dictutil.dict_to_*` own polymorphic decode dispatching on the dict `type`, never a per-kind decode branch.
- `lib.*_by_identifier` loaders are the single access path into the standards backend, resolving by identifier and caching, never reading the standards JSON directly nor re-shipping the library.
- `check_all(detailed=True)` returns energy-specific error rows folded into the same `Result` as the core geometric checks; one `Model.check_all(detailed=True)` runs both in one pass because honeybee-core invokes each registered extension's `check_all`.

[STACKING]:
- `honeybee-core`(`.api/honeybee-core.md`): the spine — `import honeybee_energy` fires `_extend_honeybee` registering `*EnergyProperties` onto the core `*Properties`, so energy resources ride the `Model`/`Room`/`Face` graph through `.properties.energy` and serialize via `to_dict(included_prop=['energy'])`.
- `honeybee-standards`(`.api/honeybee-standards.md`): the always-loaded defaults floor — `lib.*_by_identifier` resolves against its `energy_default.json` path constant.
- `honeybee-energy-standards`(`.api/honeybee-energy-standards.md`): the large ASHRAE 90.1 / DOE-prototype JSON dropped into `folders.standards_extension_folders`; `building_program_type_by_identifier` resolves against it, present only when installed.
- `honeybee-openstudio`(`.api/honeybee-openstudio.md`): the in-process OpenStudio translation over the native `openstudio` SDK; `DetailedHVAC` and the measure-backed path round-trip through it rather than the `run_idf` CLI.
- `dragonfly-energy`(`.api/dragonfly-energy.md`): the urban aggregator above — its translators explode a district massing model into per-building honeybee-energy assignments through the same `.properties.energy` accessor.
- `pydantic`(`libs/python/.api/pydantic.md`), `msgspec`(`libs/python/.api/msgspec.md`): abridged HBJSON validated through the `honeybee-schema` energy models (the `pydantic` layer) and decoded through `msgspec` at the boundary.
- `anyio`(`libs/python/.api/anyio.md`), `expression`(`libs/python/.api/expression.md`): each `run_idf`/`run_osw` brackets through `anyio.to_thread.run_sync` off the event loop under runtime observation, folding the provider result into the `expression` `Result`.
- `numpy`(`libs/python/.api/numpy.md`), `xarray`(`libs/python/.api/xarray.md`): the `result.*_from_sql` rows promote into the `numpy`/`xarray` data tier, `match_rooms_to_data`/`match_faces_to_data` joining the series back onto honeybee geometry.
- `baseline.*` needs a PROPOSED simulation beside a baseline roster, so a rating entry runs after two `run_idf` passes and never off one sql; `pci`/`result` read the `data/` vintage csv tables and no other path reaches them.

[LOCAL_ADMISSION]:
- Energy-property assignment feeds the energy-modeling owner; standards resolve through `lib.*_by_identifier`; result rows promote to the `numpy`/`xarray` tier and `check_all(detailed=True)` folds into the `expression` `Result`.
- Consume the AGPL-3.0 stack as a process-boundary companion exchanging HBJSON and shelling out to the external EnergyPlus/OpenStudio CLI, never statically linked into a distributed proprietary artifact.

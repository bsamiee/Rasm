# [PY_GEOMETRY_API_DRAGONFLY_ENERGY]

`dragonfly-energy` is the energy-simulation extension of `dragonfly-core`: importing it registers a `.properties.energy` accessor on every dragonfly object (`Model`/`Building`/`Story`/`Room2D`/`ContextShade`) carrying the Honeybee-Energy attributes (program type, construction set, HVAC, SHW, schedules), and it adds the district-scale translators that turn an urban massing model into an URBANopt analysis, a district energy system (DES — 4th/5th-gen thermal loops with ground heat exchangers), an OpenDSS electrical network, or a REopt techno-economic analysis. It is the DISTRICT-ENERGY-TRANSLATION leg of the geometry energy-companion owner — it builds the typed inputs and drives the external simulation engines; it computes no physics in-process.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `dragonfly-energy`
- package: `dragonfly-energy`
- import: `import dragonfly_energy` (registers `.properties.energy` via `_extend_dragonfly`) — then compose on dragonfly objects; translator `from dragonfly_energy.writer import model_to_urbanopt`, run pipeline `from dragonfly_energy.run import run_urbanopt, run_des_sys_param, run_reopt`
- installed: `1.29.6`
- license: AGPL-3.0 — STRONG network-copyleft; consume as a process-boundary companion (see `dragonfly-core.md` `[LICENSE_BOUNDARY]`)
- module: pure Python — `dragonfly_energy.properties.{model,building,story,room2d,context}` (the `.energy` extension), `dragonfly_energy.{writer,run,reopt,measure}` (translation + run pipeline), `dragonfly_energy.des.{loop,ghe,connector,junction}` (district energy system), `dragonfly_energy.opendss.*` (electrical network)
- owner: `geometry`
- rail: energy-companion (district energy translation)
- abi: pure-Python `py3-none-any` wheel; no compiled payload — the actual simulation is EXTERNAL (see `[RUNTIME_BOUNDARY]`)
- depends: `dragonfly-core==1.57.10` (the host model), `honeybee-energy==1.109.27` (the EnergyPlus object library wrapped by `.properties.energy`); the `standards` extra adds `honeybee-energy-standards`; `honeybee-openstudio` is the separately admitted OSW->OpenStudio runtime companion `run_urbanopt` drives
- capability: attach Honeybee-Energy properties to an urban model, then translate it to URBANopt (feature GeoJSON + scenario), to a DES (4th-gen hot/chilled loop or GHE-based ambient loop with borefield sizing), to an OpenDSS electrical network, or to a REopt analysis — and drive the external engines through the run pipeline
- scope-law: dragonfly-energy owns the DISTRICT-ENERGY TRANSLATION and run orchestration. It is not the room-level energy model (`honeybee-energy`), not the urban geometry (`dragonfly-core`), and not the simulation engine (URBANopt/OpenStudio/Modelica/REopt run out-of-process)

[RUNTIME_BOUNDARY]: the `run` pipeline shells out to EXTERNAL, non-pip tools the design page must provision: the URBANopt CLI + OpenStudio + Ruby (`run_urbanopt`), the GeoJSON-to-Modelica translator + Modelica via Docker (`run_des_modelica`/`run_modelica_docker`), the RNM electrical engine (`run_rnm`), and the NREL REopt web API with a developer key + a URDB rate label (`run_reopt`). dragonfly-energy is the typed input builder and subprocess driver; the engines are process-boundary services.

## [02]-[ENERGY_PROPERTIES]

[EXTENSION_SCOPE]: the `.properties.energy` accessors (`dragonfly_energy.properties.*`) — composed onto dragonfly objects, one extension owning all energy attributes
- rail: energy-companion

| [INDEX] | [SYMBOL] | [ROLE] |
| :-----: | :------- | :----- |
|  [01]   | `Room2DEnergyProperties` (`room2d.properties.energy`) | the per-room energy assignment — `program_type`, `construction_set`, `hvac`, `shw` (honeybee-energy objects); the `is_conditioned`/`has_window_opening` flags, the `process_loads`/`total_process_load` and `window_vent_control`/`window_vent_opening` accessors, and `add_default_ideal_air()` / `add_process_load(...)` / `remove_process_loads()` |
|  [02]   | `ModelEnergyProperties` (`model.properties.energy`) | the model-level resource registry — aggregates `materials`, `constructions`, `face_constructions`, `shade_constructions`, `construction_sets`, `global_construction_set`, `schedule_type_limits`, `schedules` (construction/shade/program-type/hvac/misc), `program_types`, `hvacs`, `shws`; `to_honeybee(new_host)` carries energy props during translation; `apply_properties_from_dict(data)` loads them |
|  [03]   | `BuildingEnergyProperties`, `StoryEnergyProperties`, `ContextShadeEnergyProperties` | the building/story/context energy hosts (construction-set inheritance, context shade constructions) |

[VALIDATION_SCOPE]: `ModelEnergyProperties.check_all(...)` plus the duplicate-identifier checks it folds (`check_duplicate_construction_set_identifiers`, `check_duplicate_program_type_identifiers`, `check_duplicate_hvac_identifiers`, `check_duplicate_shw_identifiers`) — energy-resource integrity before translation. The model-level resource collections (`materials`, `constructions`, `face_constructions`, `shade_constructions`, `construction_sets`, `global_construction_set`, `schedule_type_limits`, `schedules`, `program_types`, `hvacs`, `shws`) are the aggregated registries those checks scan.

## [03]-[DISTRICT_SYSTEMS]

[DES_SCOPE]: district energy system (`dragonfly_energy.des.{loop,ghe,connector,junction}`) — thermal loops, ground heat exchangers, borefield parameters, connectors, junctions
- rail: energy-companion

| [INDEX] | [SYMBOL] | [ROLE] |
| :-----: | :------- | :----- |
|  [01]   | `FourthGenThermalLoop(identifier, chilled_water_setpoint=6, hot_water_setpoint=54)` (`des.loop`) | a conventional hot/chilled-water district loop carrying its two water setpoints; `to_des_param_dict(buildings, tolerance=0.01)` emits the system-parameter inputs |
|  [02]   | `GHEThermalLoop(identifier, ground_heat_exchangers, connectors, clockwise_flow=False, soil_parameters=None, fluid_parameters=None, pipe_parameters=None, borehole_parameters=None, design_parameters=None)` (`des.loop`) | the GHE-based ambient (5th-gen) loop — `ground_heat_exchangers: list[GroundHeatExchanger]`, `connectors: list[ThermalConnector]`, plus the borefield-sizing parameter inputs; same `to_des_param_dict(buildings, tolerance=0.01)` surface |
|  [03]   | `GroundHeatExchanger(identifier, geometry)` (`des.ghe`) | the borefield footprint — `geometry`, `boundary_2d`, `hole_polygon2d`; `from_geojson_dict(...)` / `to_geojson_dict(...)` |
|  [04]   | `SoilParameter` / `FluidParameter` / `PipeParameter` / `BoreholeParameter` / `GHEDesignParameter` (`des.loop`) | the borefield hydraulic + design parameter value-objects a `GHEThermalLoop` carries (ground conductivity, fluid, pipe geometry, borehole layout, sizing targets) |
|  [05]   | `ThermalConnector` (`des.connector`), `ThermalJunction` (`des.junction`) | the piping-network segments and the network junctions |

[OPENDSS_SCOPE]: electrical network (`dragonfly_energy.opendss.*`)
- `ElectricalNetwork` — `from_rnm_geojson(...)` ingests an RNM-generated network, `to_geojson_dict(buildings, location, point, tolerance)` and `to_electrical_database_dict()` emit the OpenDSS inputs; composed of `Substation`, `Transformer`/`TransformerProperties`, `PowerLine`, `Wire` (+ `ConcentricProperties`), `ElectricalJunction`, `ElectricalConnector` with the `opendss.lib.{powerlines,transformers,wires}` equipment catalogs.
- `RoadNetwork` — the road graph (`from_dict`, `to_geojson_dict`) paired with the electrical layout.

[REOPT_SCOPE]: techno-economic analysis (`dragonfly_energy.reopt`)
- `REoptParameter` — the REopt run config; `to_assumptions_dict(base_file, urdb_label)` builds the NREL REopt POST body. Composed of `FinancialParameter`, `WindParameter`, `PVParameter`, `StorageParameter`, `GeneratorParameter`, `GroundMountPV` — the per-technology sizing/cost inputs.

## [04]-[TRANSLATION_AND_RUN]

[TRANSLATE]: the headline writer (`dragonfly_energy.writer`)
- `model_to_urbanopt(model, location, point=Point2D(0,0), shade_distance=None, use_multiplier=True, add_plenum=False, solve_ceiling_adjacencies=False, des_loop=None, electrical_network=None, road_network=None, ground_pv=None, folder=None, tolerance=0.01)` — emit an URBANopt feature GeoJSON + per-building OSW/HBJSON in one pass, optionally layering the DES loop / electrical / road / ground-PV networks. The DES system-parameter JSON itself is produced by the `run.run_des_sys_param` stage below, not a separate writer.

[RUN]: the orchestration pipeline (`dragonfly_energy.run`) — typed drivers over the external engines
- `base_honeybee_osw(...)` builds the OpenStudio workflow; `prepare_urbanopt_folder(feature_geojson, cpu_count=None, verbose=False)` then `run_urbanopt(feature_geojson, scenario_csv, cpu_count=None)` run the URBANopt analysis; `run_default_report(feature_geojson, scenario_csv)` aggregates results.
- `run_reopt(feature_geojson, scenario_csv, urdb_label, reopt_parameters=None, ...)` drives the REopt API; `run_rnm(feature_geojson, scenario_csv, underground_ratio=0.9, lv_only=True, ...)` the RNM electrical sizing.
- `run_des_sys_param(feature_geojson, scenario_csv)`, `run_des_modelica(sys_param_json, feature_geojson, scenario_csv)`, `run_modelica_docker(modelica_project_dir)` — the DES Modelica path (build the system-parameter JSON from the URBANopt scenario, generate the Modelica project, then run it under Docker).
- `MapperMeasure` / `MapperMeasureArgument` (`dragonfly_energy.measure`) — URBANopt mapper-measure injection for per-feature OpenStudio measures.

## [05]-[INTEGRATION]

[SUBSTRATE_STACK]: stacking onto the universal Python rails (`libs/python/.api/`)
- `anyio` — the `run_*` pipeline is long, blocking, subprocess/Docker-bound: drive each stage through `anyio.run_process` / `anyio.to_thread.run_sync` inside a task group with a cancel scope + deadline so a wedged URBANopt/Modelica run reclaims cleanly.
- `stamina` — wrap `run_reopt` (the NREL REopt HTTP API) in a retry context for transient API failures; do not retry the local subprocess stages (they are not idempotent mid-run).
- `structlog` + `opentelemetry` — a span per `model_to_urbanopt`/`run_urbanopt`/`run_des_modelica`/`run_reopt` carrying `(scenario, building_count, cpu_count)`; the run is minutes-long so the span is the unit, not per-building events.
- `psutil` — `prepare_urbanopt_folder`/`run_urbanopt` take a `cpu_count`; size it from the resource governor rather than hardcoding.
- `universal-pathlib` — point the `folder` outputs (feature GeoJSON, OSW, sys-param JSON) at the artifact store.

[SIBLING_STACK]: stacking with the geometry folder + cross-folder data (`libs/python/geometry/.api/`, `libs/python/data/.api/`)
- `dragonfly-core` — the host model this extends; build/zone the massing there, attach `.properties.energy` here, then `model_to_urbanopt` (DES via the `run_des_*` pipeline).
- `honeybee-energy` — the object library `.properties.energy` wraps: `room2d.properties.energy.hvac`/`.construction_set`/`.program_type`/`.shw` are honeybee-energy objects; the room-level energy model and EnergyPlus IDF generation live there.
- `honeybee-openstudio` — the OSW->OpenStudio/EnergyPlus bridge `run_urbanopt` drives; it is a separately admitted runtime companion (dragonfly-energy ships no `openstudio` extra), not a pip extra of this package.
- `honeybee-energy-standards` (`honeybee-energy-standards.md`) — the `standards` extra's large ASHRAE 90.1 / DOE-prototype construction/program/schedule library (`construction_set_by_identifier`/`building_program_type_by_identifier`) for assigning realistic defaults to an urban model; resolved via `honeybee-energy.lib`, layered on the `honeybee-standards` defaults floor.
- `lbt-recipes` / `queenbee` / `pollination-handlers` — the recipe/job runners for packaging a district-energy workflow as a reproducible recipe (companion runners).
- `geopandas` / `shapely` / `pyproj` (data folder, cross-folder) — the URBANopt feature GeoJSON and DES/electrical GeoJSON are standard feature collections; read/edit them with `geopandas` and project with `pyproj`.

[CONTENT_IDENTITY_SEAM]: key a run by `(model id, scenario, engine, epw)` in the persistence reuse ledger so an identical district-energy analysis is deduped; the URBANopt scenario folder is the on-disk artifact the ledger references.

[RAIL_LAW]:
- Package: `dragonfly-energy`
- Owns: the `.properties.energy` extension on dragonfly objects, the district energy system models (`FourthGenThermalLoop`/`GHEThermalLoop`, `GroundHeatExchanger`, the borefield parameter objects, `ThermalConnector`/`ThermalJunction`), the OpenDSS `ElectricalNetwork`, the `REoptParameter` tree, and the URBANopt/DES/REopt/RNM translation + run pipeline
- Accept: `room2d.properties.energy.<attr> = <honeybee-energy object>` as the energy assignment; `model_to_urbanopt(model, location, des_loop=..., electrical_network=...)` as the canonical district translation; `run_des_sys_param` -> `run_des_modelica` -> `run_modelica_docker` for the Modelica DES path; `run_reopt` (under retry) for techno-economics; `ModelEnergyProperties.check_all` before translating
- Reject: importing it expecting in-process simulation (the engines are external — provision URBANopt/OpenStudio/Modelica/REopt at the process boundary); statically linking the AGPL stack into a distributed artifact; hand-building honeybee-energy objects instead of using `honeybee-energy`; retrying non-idempotent local run stages; hardcoding `cpu_count` instead of sizing from the resource governor

# [PY_GEOMETRY_API_DRAGONFLY_ENERGY]

`dragonfly-energy` extends `dragonfly-core` with district-scale energy: importing it registers a `.properties.energy` accessor on every dragonfly object carrying the Honeybee-Energy attributes (program type, construction set, HVAC, SHW, schedules), and it adds the translators that turn an urban massing model into an URBANopt analysis, a district energy system (4th/5th-gen thermal loops with ground heat exchangers), an OpenDSS electrical network, or a REopt techno-economic run. It builds the typed inputs and drives the external engines out-of-process, computing no physics in-process.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `dragonfly-energy`
- package: `dragonfly-energy` (AGPL-3.0 network-copyleft, Ladybug Tools)
- module: `dragonfly_energy` — pure Python; import registers `.properties.energy` via `_extend_dragonfly`
- namespaces: `dragonfly_energy.properties.{model,building,story,room2d,context}`, `dragonfly_energy.{writer,run,reopt,measure}`, `dragonfly_energy.des.{loop,ghe,connector,junction}`, `dragonfly_energy.opendss.*`
- rail: energy-companion (district energy translation)
- owner: `geometry`
- consumer: `.planning/energy/district.md`
- depends: `dragonfly-core` (host model), `honeybee-energy` (the EnergyPlus object library `.properties.energy` wraps); the `standards` extra adds `honeybee-energy-standards`; `honeybee-openstudio` is the separately admitted OSW->OpenStudio runtime companion `run_urbanopt` drives
- scope-law: owns the district-energy translation and run orchestration; not the room-level energy model (`honeybee-energy`), the urban geometry (`dragonfly-core`), or the simulation engine (URBANopt/OpenStudio/Modelica/REopt run out-of-process)

[RUNTIME_BOUNDARY]: the `run` pipeline is a subprocess driver over external, non-pip engines a design page provisions — URBANopt CLI + OpenStudio + Ruby (`run_urbanopt`), the GeoJSON-to-Modelica translator + Modelica via Docker (`run_des_modelica`/`run_modelica_docker`), the RNM electrical engine (`run_rnm`), and the NREL REopt web API with a developer key + URDB rate label (`run_reopt`).

## [02]-[ENERGY_PROPERTIES]

[EXTENSION_SCOPE]: the `.properties.energy` accessors composed onto dragonfly objects — one extension owning every energy attribute and the pre-translation validation.

| [INDEX] | [SYMBOL]                                                                          | [TYPE_FAMILY] | [CAPABILITY]                         |
| :-----: | :-------------------------------------------------------------------------------- | :------------ | :----------------------------------- |
|  [01]   | `Room2DEnergyProperties`                                                          | class         | per-room energy assignment           |
|  [02]   | `ModelEnergyProperties`                                                           | class         | model resource registry + validation |
|  [03]   | `BuildingEnergyProperties` `StoryEnergyProperties` `ContextShadeEnergyProperties` | class         | construction-set / shade inheritance |

- `room2d.properties.energy`: assign `program_type` `construction_set` `hvac` `shw`; flags `is_conditioned` `has_window_opening`; process loads `process_loads` `total_process_load` `add_process_load()` `remove_process_loads()` `add_default_ideal_air()`; ventilation `window_vent_control` `window_vent_opening`
- `model.properties.energy`: registries `materials` `constructions` `face_constructions` `shade_constructions` `construction_sets` `global_construction_set` `schedule_type_limits` `schedules` `program_types` `hvacs` `shws`; `to_honeybee(new_host)` carries props, `apply_properties_from_dict(data)` loads them, `check_all()` folds the duplicate-identifier checks (construction-set/program-type/hvac/shw) before translation

## [03]-[DISTRICT_SYSTEMS]

[DES_SCOPE]: district energy system (`dragonfly_energy.des.{loop,ghe,connector,junction}`) — thermal loops, ground heat exchangers, borefield parameters, connectors, junctions.

| [INDEX] | [SYMBOL]                                                                | [TYPE_FAMILY] | [CAPABILITY]                           |
| :-----: | :---------------------------------------------------------------------- | :------------ | :------------------------------------- |
|  [01]   | `FourthGenThermalLoop` (`des.loop`)                                     | class         | conventional hot/chilled district loop |
|  [02]   | `GHEThermalLoop` (`des.loop`)                                           | class         | GHE-based ambient (5th-gen) loop       |
|  [03]   | `GroundHeatExchanger` (`des.ghe`)                                       | class         | borefield footprint geometry           |
|  [04]   | `ThermalConnector` (`des.connector`) `ThermalJunction` (`des.junction`) | class         | piping segments + junctions            |

[BOREFIELD_PARAMETERS] (`des.ghe`, value-object): `SoilParameter` `FluidParameter` `PipeParameter` `BoreholeParameter` `GHEDesignParameter` — ground conductivity, fluid, pipe geometry, borehole layout, and sizing targets a `GHEThermalLoop` composes.

[DES_OPS]: constructor then `to_des_param_dict` emit per loop.
- `FourthGenThermalLoop(identifier, chilled_water_setpoint=6, hot_water_setpoint=54)` carries the two water setpoints; `to_des_param_dict(buildings, tolerance=0.01)` emits the system-parameter inputs
- `GHEThermalLoop(identifier, ground_heat_exchangers, connectors, clockwise_flow=False, soil/fluid/pipe/borehole/design_parameters=None)` composes the borefield parameters; same `to_des_param_dict(buildings, tolerance=0.01)` surface
- `GroundHeatExchanger(identifier, geometry)` — `geometry` `boundary_2d` `hole_polygon2d`; `from_geojson_dict()`/`to_geojson_dict()`

[OPENDSS_SCOPE]: electrical network (`dragonfly_energy.opendss.*`)
- `ElectricalNetwork` — `from_rnm_geojson()` ingests an RNM-generated network; `to_geojson_dict(buildings, location, point, tolerance)` and `to_electrical_database_dict()` emit the OpenDSS inputs; composed of `Substation`, `Transformer`/`TransformerProperties`, `PowerLine`, `Wire` (+ `ConcentricProperties`), `ElectricalJunction`, `ElectricalConnector` over the `opendss.lib.{powerlines,transformers,wires}` equipment catalogs
- `RoadNetwork` — road graph (`from_dict`, `to_geojson_dict`) paired with the electrical layout

[REOPT_SCOPE]: techno-economic analysis (`dragonfly_energy.reopt`)
- `REoptParameter.to_assumptions_dict(base_file, urdb_label)` builds the NREL REopt POST body; composed of `FinancialParameter`, `WindParameter`, `PVParameter`, `StorageParameter`, `GeneratorParameter`, `GroundMountPV` per-technology sizing/cost inputs

## [04]-[TRANSLATION_AND_RUN]

[TRANSLATE]: `dragonfly_energy.writer`
- `model_to_urbanopt(model, location, point, shade_distance=None, use_multiplier=True, add_plenum=False, solve_ceiling_adjacencies=False, des_loop=None, electrical_network=None, road_network=None, ground_pv=None, folder=None, tolerance=0.01)` — emit an URBANopt feature GeoJSON + per-building OSW/HBJSON in one pass, optionally layering the DES loop / electrical / road / ground-PV networks

[RUN]: `dragonfly_energy.run` — typed drivers over the external engines
- `base_honeybee_osw()` builds the OpenStudio workflow; `prepare_urbanopt_folder(feature_geojson, cpu_count=None)` then `run_urbanopt(feature_geojson, scenario_csv, cpu_count=None)` run the analysis; `run_default_report(feature_geojson, scenario_csv)` aggregates results
- `run_reopt(feature_geojson, scenario_csv, urdb_label, reopt_parameters=None)` drives the REopt API; `run_rnm(feature_geojson, scenario_csv, underground_ratio=0.9, lv_only=True)` the RNM electrical sizing
- `run_des_sys_param(feature_geojson, scenario_csv)` -> `run_des_modelica(sys_param_json, feature_geojson, scenario_csv)` -> `run_modelica_docker(modelica_project_dir)` — the DES Modelica path builds the system-parameter JSON, generates the Modelica project, then runs it under Docker
- `MapperMeasure`/`MapperMeasureArgument` (`dragonfly_energy.measure`) — per-feature OpenStudio mapper-measure injection

## [05]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Every translation folds through the process boundary: `dragonfly-energy` builds typed inputs (feature GeoJSON, OSW/HBJSON, sys-param JSON, REopt POST body) and drives external engines out-of-process, computing no physics in-process
- Key a run by `(model id, scenario, engine, epw)` in the persistence reuse ledger so an identical analysis dedupes; the URBANopt scenario folder is the on-disk artifact the ledger references

[STACKING]:
- `anyio`(`.api/anyio.md`): the `run_*` pipeline runs each subprocess/Docker stage through `anyio.run_process`/`anyio.to_thread.run_sync` in a task group with a cancel scope + deadline so a wedged URBANopt/Modelica run reclaims
- runtime `guarded`(`.api/runtime`): wrap `run_reopt` (the NREL REopt HTTP API) in the retry rail for transient API failures; the local subprocess stages are non-idempotent and never retry
- `structlog` + `opentelemetry`(`.api/opentelemetry-api.md`): one span per `model_to_urbanopt`/`run_urbanopt`/`run_des_modelica`/`run_reopt` carrying `(scenario, building_count, cpu_count)`; the minutes-long run is the span unit
- `psutil`(`.api/psutil.md`): size the `cpu_count` on `prepare_urbanopt_folder`/`run_urbanopt` from the resource governor rather than hardcoding
- `universal-pathlib`(`.api/universal-pathlib.md`): point the `folder` outputs (feature GeoJSON, OSW, sys-param JSON) at the artifact store
- `dragonfly-core`(`.api/dragonfly-core.md`): hosts the model this extends — build/zone the massing there, attach `.properties.energy` here, then `model_to_urbanopt`
- `honeybee-energy`(`.api/honeybee-energy.md`): supplies the objects `.properties.energy` wraps — `room2d.properties.energy.hvac`/`.construction_set`/`.program_type`/`.shw` are honeybee-energy objects; the room-level model + EnergyPlus IDF generation live there
- `honeybee-openstudio`(`.api/honeybee-openstudio.md`): bridges OSW->OpenStudio/EnergyPlus, driven by `run_urbanopt`
- `honeybee-energy-standards`(`.api/honeybee-energy-standards.md`): the `standards` extra's ASHRAE 90.1 / DOE-prototype construction/program/schedule library (`construction_set_by_identifier`/`building_program_type_by_identifier`) for realistic urban defaults, resolved via `honeybee-energy.lib`
- `lbt-recipes`/`queenbee`/`pollination-handlers`: recipe/job runners packaging a district-energy workflow as a reproducible recipe
- `geopandas`/`shapely`/`pyproj` (data folder): the URBANopt/DES/electrical GeoJSON are standard feature collections — read/edit with `geopandas`, project with `pyproj`

[LOCAL_ADMISSION]:
- Admitted as a process-boundary companion; the external engines are the design page's provisioning obligation, not a pip dependency of this catalog

[RAIL_LAW]:
- Package: `dragonfly-energy`
- Owns: the `.properties.energy` extension on dragonfly objects, the district energy system models (`FourthGenThermalLoop`/`GHEThermalLoop`, `GroundHeatExchanger`, the borefield parameter objects, `ThermalConnector`/`ThermalJunction`), the OpenDSS `ElectricalNetwork`, the `REoptParameter` tree, and the URBANopt/DES/REopt/RNM translation + run pipeline
- Accept: `room2d.properties.energy.<attr> = <honeybee-energy object>` as the energy assignment; `model_to_urbanopt(model, location, des_loop=..., electrical_network=...)` as the district translation; `run_des_sys_param` -> `run_des_modelica` -> `run_modelica_docker` for the Modelica DES path; `run_reopt` (under retry) for techno-economics; `ModelEnergyProperties.check_all` before translating
- Reject: importing it for in-process simulation (the engines are external); statically linking the AGPL stack into a distributed artifact; hand-building honeybee-energy objects instead of using `honeybee-energy`; retrying non-idempotent local run stages; hardcoding `cpu_count` instead of sizing from the resource governor

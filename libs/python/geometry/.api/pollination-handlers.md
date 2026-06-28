# [PY_GEOMETRY_API_POLLINATION_HANDLERS]

`pollination-handlers` (import `pollination_handlers`) is the adapter library that bridges the geometry energy domain objects into recipe-input artifacts: a flat catalog of `inputs.*` functions that take a honeybee/dragonfly `Model`, a ladybug `Wea`/`EPW`/`DDY`/`RunPeriod`/`Location`, a honeybee-energy `SimulationParameter`/`Measure`, or a `DataCollection` — or a path to any of them — and return the file path / coerced string the recipe DAG input expects, plus `outputs.*` functions that read a finished simulation result folder back into ladybug `DataCollection`s and summary dicts. Each input handler is polymorphic on input type (domain object | path | value), so one function owns the object-or-path modality. In geometry it is the energy handler-adapter owner — the model-to-recipe-input translation the energy domain composes — while runtime owns the execution-time binding that resolves and invokes these handlers (`runtime/.api/pollination-handlers.md`). The handlers are the targets of the queenbee `IOAliasHandler(module='pollination_handlers.inputs.model', function='model_to_json')` references (`geometry/.api/queenbee.md`) and consume the honeybee/ladybug/dragonfly domain models (`honeybee-energy.md`, `ladybug-core.md`, `ladybug-comfort.md`, `dragonfly-energy.md`). The package owner composes the `inputs.*` adapters into the energy recipe-input rail; it never hand-rolls HBJSON/DFJSON serialization or wea/ddy translation the honeybee/ladybug model classes already own.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `pollination-handlers`
- package: `pollination-handlers`
- import: `import pollination_handlers`
- owner: `geometry`
- rail: energy / recipe-input-adapter
- installed: `0.10.10`
- license: `AGPL-3.0` (network copyleft — the strongest reading; the binding admission flag for this distribution)
- abi: pure-Python (`py2.py3-none-any`, purelib; no native extension)
- depends: `lbt-dragonfly (>=0.9.453)` — the meta-package pulling honeybee/honeybee-energy/honeybee-radiance, ladybug/ladybug-comfort, and dragonfly/dragonfly-energy (the domain model classes the handlers translate)
- entry points: none (library only; functions are referenced by `module`+`function` from a queenbee `IOAliasHandler`)
- capability: object-or-path -> artifact translation for honeybee/dragonfly models, wea/ddy/epw weather, simulation parameters and measures, schedules, run periods, data collections, north, emissions, and boolean/option toggles; result-folder -> `DataCollection`/summary readers for daylight, comfort, EUI, and LEED outputs

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: package layout
- rail: recipe-input-adapter

There are no classes — the package is two module trees of pure functions. Handlers are addressed by their `module`+`function` name from a queenbee `IOAliasHandler`, so the module path is part of the contract.

| [INDEX] | [SYMBOL]                       | [TYPE_FAMILY] | [CAPABILITY]                                           |
| :-----: | :----------------------------- | :------------ | :---------------------------------------------------- |
|  [01]   | `pollination_handlers.inputs`  | module tree   | object/value -> recipe-input artifact (the geometry-facing adapters) |
|  [02]   | `pollination_handlers.outputs` | module tree   | result folder/path -> ladybug `DataCollection`/dict   |
|  [03]   | `pollination_handlers.inputs.helper` / `pollination_handlers.outputs.helper` | helper module | tempfile/CSV writers; sensor-grid result readers shared by the handlers |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: model adapters (`inputs.model`)
- rail: recipe-input-adapter

Each takes a honeybee `Model` (or dragonfly `Model`) or a path; a `Model` is written to a temp HBJSON/DFJSON and the path returned, a path is returned as-is. The `*_check` variants raise a clear `ValueError` when the model lacks the rooms/HVAC/grids/views the recipe needs.

| [INDEX] | [SURFACE]                                                  | [ENTRY_FAMILY] | [CAPABILITY]                              |
| :-----: | :--------------------------------------------------------- | :------------- | :---------------------------------------- |
|  [01]   | `inputs.model.model_to_json(model_obj) -> str`            | model adapter  | honeybee `Model` \| path -> HBJSON path   |
|  [02]   | `inputs.model.model_to_json_room_check` / `model_to_json_hvac_check` | model adapter | HBJSON path with rooms / conditioned-HVAC precondition |
|  [03]   | `inputs.model.model_to_json_grid_check` / `model_to_json_grid_room_check` / `model_to_json_view_check` | model adapter | HBJSON path with sensor-grid / grid+room / view precondition (radiance folder passthrough) |
|  [04]   | `inputs.model.model_dragonfly_to_json(model_obj) -> str`  | model adapter  | dragonfly `Model` \| path -> DFJSON path  |

[ENTRYPOINT_SCOPE]: weather and run-period adapters (`inputs.wea` / `inputs.ddy` / `inputs.runperiod` / `inputs.north` / `inputs.emissions`)
- rail: recipe-input-adapter

These coerce ladybug weather/location primitives; `wea_handler` accepts a `Wea`, an `EPW` (converted to an annual wea), or a path.

| [INDEX] | [SURFACE]                                                       | [ENTRY_FAMILY] | [CAPABILITY]                               |
| :-----: | :------------------------------------------------------------- | :------------- | :----------------------------------------- |
|  [01]   | `inputs.wea.wea_handler(wea_obj) -> str`                       | weather adapter| ladybug `Wea` \| `EPW` \| path -> wea path |
|  [02]   | `inputs.wea.wea_handler_timestep_check` / `wea_handler_timestep_annual_check` | weather adapter | wea path with timestep / annual-coverage precondition |
|  [03]   | `inputs.ddy.ddy_handler(ddy_obj) -> str`                       | weather adapter| ladybug `DDY` \| path -> ddy path          |
|  [04]   | `inputs.runperiod.run_period_to_str(value) -> str`            | period adapter | ladybug `RunPeriod` -> recipe string       |
|  [05]   | `inputs.north.north_vector_to_angle(value) -> float`          | orient adapter | `Vector2D` \| number -> north angle        |
|  [06]   | `inputs.emissions.location_to_electricity_emissions(value) -> float` | factor adapter | ladybug `Location` -> grid emissions factor |

[ENTRYPOINT_SCOPE]: simulation, schedule, and data adapters (`inputs.simulation` / `inputs.schedule` / `inputs.data`)
- rail: recipe-input-adapter

`energy_sim_par_to_json` serializes a honeybee-energy `SimulationParameter`; `value_or_data_to_*` accept a scalar or a ladybug `DataCollection`.

| [INDEX] | [SURFACE]                                                          | [ENTRY_FAMILY] | [CAPABILITY]                                |
| :-----: | :---------------------------------------------------------------- | :------------- | :------------------------------------------ |
|  [01]   | `inputs.simulation.energy_sim_par_to_json(sim_par_obj) -> str`   | sim adapter    | `SimulationParameter` \| path -> JSON path  |
|  [02]   | `inputs.simulation.measures_to_folder(measures_obj) -> str`     | sim adapter    | OpenStudio measures -> measures folder      |
|  [03]   | `inputs.simulation.list_to_additional_strings` / `list_to_additional_idf` / `viz_variables_to_string` / `standard_to_str` | sim adapter | list/standard coercion to recipe strings |
|  [04]   | `inputs.schedule.schedule_to_csv(value) -> str` / `inputs.schedule.data_to_csv(value) -> str` | schedule adapter | schedule / data-collection -> CSV path |
|  [05]   | `inputs.data.value_or_data_to_str(value)` / `value_or_data_to_file(value, file_name=None)` | data adapter | number \| `DataCollection` -> str / file |
|  [06]   | `inputs.data.value_or_data_to_air_speed_file` / `value_or_data_to_met_file` / `value_or_data_to_clo_file` | data adapter | comfort-input coercion to per-sensor files |

[ENTRYPOINT_SCOPE]: option and output-reader surface
- rail: recipe-input-adapter / result-reader

`inputs.bool_options.*` coerce boolean/option toggles to the recipe's acceptable strings via `helper.bool_option_to_str`; `outputs.*` read a finished result folder back into ladybug objects. Raw Radiance parameter strings carry no coercion handler in this package — they pass through to the recipe as-is.

| [INDEX] | [SURFACE]                                                              | [ENTRY_FAMILY] | [CAPABILITY]                              |
| :-----: | :-------------------------------------------------------------------- | :------------- | :---------------------------------------- |
|  [01]   | `inputs.bool_options.*` (`filter_des_days_to_str`, `use_multiplier_to_str`, `is_residential_to_str`, `cloudy_bool_to_str`, `sky_view_metric_to_str`, `skip_overture_to_str`, `glare_control_devices_to_str`, `write_set_map_to_str`, `visible_vs_solar_to_str`, `bldg_lighting_to_str`) | option adapter | bool/option -> acceptable recipe string |
|  [02]   | `inputs.pit.point_in_time_metric_to_str` / `inputs.postprocess.grid_metrics` | option adapter | metric/grid-metric coercion             |
|  [03]   | `outputs.daylight.read_da_from_folder` / `read_udi_from_folder` / `read_cda_from_folder` / `read_ase_from_folder` / `sort_ill_from_folder` / `read_df_from_folder` (+ `read_pit`/`read_ga`/`read_hours`/`read_images`) | result reader | radiance result folder -> per-sensor metric lists / sorted `.ill` |
|  [04]   | `outputs.daylight.read_leed_datacollection_from_folder` / `read_grid_metrics` / `read_json_dict` / `read_json_summary_list` / `read_leed_summary_grid` | result reader | result -> ladybug `DataCollection` / dict / summary list |
|  [05]   | `outputs.comfort.read_comfort_percent_from_folder` / `outputs.eui.eui_json_from_path` / `outputs.summary.json_properties_from_path` / `outputs.summary.contents_from_folder` | result reader | comfort percent / EUI / summary readers |

## [04]-[IMPLEMENTATION_LAW]

[HANDLER_TOPOLOGY]:
- polymorphic-input law: every `inputs.*` handler discriminates on the input value — a domain object (honeybee/dragonfly `Model`, ladybug `Wea`/`EPW`/`DDY`/`RunPeriod`/`DataCollection`, honeybee-energy `SimulationParameter`) is serialized to a temp artifact and its path returned; a path/string is returned (or validated) as-is. One function owns the object-or-path modality — never a `from_object`/`from_path` pair.
- precondition law: the `*_check` model handlers (`model_to_json_room_check`, `..._hvac_check`, `..._grid_check`, `..._view_check`) raise a clear `ValueError` when the model lacks the rooms/HVAC/grids/views the target recipe requires; the precondition lives in the handler, not the recipe DAG.
- temp-artifact law: serialization lands under `inputs.helper.get_tempfile`/`get_tempfolder`; the executor copies the handler's returned path into the project folder. Handlers are pure value-to-artifact maps with no recipe-execution side effects.
- name-contract law: a handler is addressed by `module`+`function` from a queenbee `IOAliasHandler`; renaming or moving a handler breaks the recipe alias contract, so the module layout (`inputs.model`, `inputs.wea`, `outputs.daylight`, …) is part of the public surface.

[STACK_LAW]:
- `queenbee`: the geometry energy domain authors `IOAliasHandler(language='python', module='pollination_handlers.inputs.model', function='model_to_json', index=0)` on an aliased `DAGInput`; this catalog supplies the referenced function (`geometry/.api/queenbee.md`).
- domain models: input handlers consume honeybee/dragonfly/ladybug domain objects and call their `to_dict`/`write`/`to_wea` methods — the model class owns serialization, the handler owns the object-or-path adapter (`honeybee-energy.md`, `ladybug-core.md`, `ladybug-comfort.md`, `dragonfly-energy.md`).
- execution: runtime owns resolving (`importlib`) and invoking these handlers during a recipe run, including chained handlers ordered by `IOAliasHandler.index` (`runtime/.api/pollination-handlers.md`, `runtime/.api/lbt-recipes.md`).

## [05]-[LOCAL_ADMISSION]

[RAIL_LAW]:
- Package: `pollination-handlers`
- Owns: the object-or-path -> recipe-input artifact adapters for honeybee/dragonfly models, ladybug weather/run-period/data, honeybee-energy simulation parameters and measures, schedules, and boolean/option toggles, plus the result-folder -> `DataCollection`/dict/summary readers
- Accept: a domain object or a path/value at every `inputs.*` handler, the `*_check` precondition variants where a recipe requires rooms/HVAC/grids/views, `IOAliasHandler` `module`+`function` addressing, `index`-ordered chained handlers, the `outputs.*` readers over a finished result folder
- Reject: hand-rolled HBJSON/DFJSON/wea/ddy serialization where the honeybee/ladybug model classes own it, `from_object`/`from_path` handler pairs where one polymorphic handler decides, a parallel result-folder parser, renaming a handler without updating the queenbee alias contract

[CAPTURE_GAP]:
- AGPL-3.0: this distribution is network-copyleft; it is a process-boundary tool invoked through the recipe-execution subprocess (`lbt-recipes` -> `queenbee local run`), never linked into a distributed library surface. The license is the binding admission flag.
- execution binding: handler resolution and invocation are the runtime owner's concern; this catalog is the adapter-function surface the energy domain authors against.

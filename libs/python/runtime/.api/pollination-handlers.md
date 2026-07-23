# [PY_RUNTIME_API_POLLINATION_HANDLERS]

`pollination-handlers` (import `pollination_handlers`) owns execution-time IO coercion for the recipe rail: the flat `inputs.*`/`outputs.*` catalog a recipe's queenbee `IOAliasHandler` addresses by `module`+`function`, that `importlib` resolves and the runtime invokes at the two recipe-run boundaries — a job input coerced to the artifact the `queenbee local run` subprocess expects, and the result folder read back into ladybug `DataCollection`s. queenbee owns the addressing schema, `lbt-recipes` the resolver and invocation sites.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `pollination-handlers`
- package: `pollination-handlers` (AGPL-3.0, network copyleft)
- module: `pollination_handlers`
- abi: pure-Python (`py2.py3-none-any`, purelib; no native extension)
- rail: recipe-execution / io-coercion
- depends: `lbt-dragonfly` — the honeybee/ladybug/dragonfly domain classes the handlers read and write

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: module-tree layout (resolution targets)

No classes: two function module-trees rooted at `pollination_handlers.*`, addressed by dotted `module`+`function` — `inputs.*` coerces values to recipe-input artifacts, `outputs.*` reads result folders to ladybug objects, and `inputs.helper`/`outputs.helper` carry the shared tempfile, CSV, and per-grid-reader primitives.

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: input-coercion handlers (invoked by `RecipeInput.handle_value`)

Each handler is polymorphic on the value: a domain object serializes to a temp artifact; a path or scalar validates and returns. Precondition `_check` variants assert their named collection exists before serializing; the wea timestep variants assert timestep 1 or annual coverage.

- [01]-[MODEL] `inputs.model`: honeybee/dragonfly `Model` or path -> HBJSON/DFJSON — `model_to_json` `model_to_json_room_check` `model_to_json_hvac_check` `model_to_json_grid_check` `model_to_json_grid_room_check` `model_to_json_view_check` `model_dragonfly_to_json`
- [02]-[WEATHER] `inputs.wea`/`inputs.ddy`: `Wea`/`EPW`/`DDY` or path -> weather artifact — `wea_handler` `wea_handler_timestep_check` `wea_handler_timestep_annual_check` `ddy_handler`
- [03]-[SIM] `inputs.simulation`: sim-param, measure, and list coercion — `energy_sim_par_to_json` `measures_to_folder` `list_to_additional_strings` `list_to_additional_idf` `viz_variables_to_string` `standard_to_str`
- [04]-[DATA] `inputs.data`/`inputs.schedule`: scalar or `DataCollection` -> str/CSV/file — `value_or_data_to_str` `value_or_data_to_file` `value_or_data_to_air_speed_file` `value_or_data_to_met_file` `value_or_data_to_clo_file` `schedule.schedule_to_csv` `schedule.data_to_csv`
- [05]-[VALUE] `inputs.{north,runperiod,emissions,pit,postprocess}`: orientation, period, emissions, and metric coercion — `north.north_vector_to_angle` `runperiod.run_period_to_str` `emissions.location_to_electricity_emissions` `pit.point_in_time_metric_to_str` `postprocess.grid_metrics`
- [06]-[OPTION] `inputs.bool_options`: bool or option -> the recipe CLI flag string — `use_multiplier_to_str` `is_residential_to_str` `cloudy_bool_to_str` `sky_view_metric_to_str` `visible_vs_solar_to_str` `glare_control_devices_to_str` `filter_des_days_to_str` `skip_overture_to_str` `write_set_map_to_str` `bldg_lighting_to_str` `helper.bool_option_to_str`

[ENTRYPOINT_SCOPE]: result-reader handlers (invoked by `RecipeOutput.value`)

Each reader turns the joined result folder into the typed ladybug object or summary dict the job receipt carries.

- [01]-[METRIC] `outputs.daylight.read_*_from_folder`: radiance result folder -> per-sensor metric lists — `read_da_from_folder` `read_udi_from_folder` `read_cda_from_folder` `read_ga_from_folder` `read_df_from_folder` `read_pit_from_folder` `read_hours_from_folder` `read_ase_from_folder`
- [02]-[COLLECTION] `outputs.daylight`: sorted `.ill` files, images, and LEED `DataCollection`s — `sort_ill_from_folder` `read_images_from_folder` `read_leed_datacollection_from_folder`
- [03]-[SUMMARY] `outputs.daylight`: LEED/credit summaries and grid-metric dicts — `read_leed_summary_grid` `read_leed_shade_transmittance_schedule` `read_grid_metrics` `ill_credit_json_from_path` `read_json_dict` `read_json_summary_list`
- [04]-[OTHER] `outputs.{comfort,eui,summary}`: comfort percent, EUI, and summary properties/contents — `comfort.read_comfort_percent_from_folder` `eui.eui_json_from_path` `summary.json_properties_from_path` `summary.contents_from_folder`
- [05]-[PRIMITIVE] `outputs.helper`: shared per-grid result readers over `(result_folder, extension, grid_key, is_percent=True, factor=1)` — `read_sensor_grid_result` `read_grid_results` (`read_grid_results` for one value per grid)

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- resolution: a handler binds by address, never a static import — `lbt-recipes` `_RecipeParameter` resolves `importlib.import_module(module)` + `getattr(module, function)` from the queenbee `IOAliasHandler`, whose `module`+`function` is the sole handler address.
- chaining: multiple handlers on one input run in `IOAliasHandler.index` order, each output feeding the next, the full chain applied in `handle_value` before the `DAGInput` cast.
- boundary: input handlers run at `RecipeInput.handle_value` before `inputs.json` writes, output handlers at `RecipeOutput.value` after the subprocess exits; both bracket the `queenbee local run` luigi worker in the runtime process, never inside it.
- purity: handlers are pure value maps writing only temp artifacts under `inputs.helper.get_tempfile`; a failed precondition raises `ValueError` the runtime lifts into a typed IO-seam boundary fault.

[STACKING]:
- `queenbee`(`queenbee.md`): the `IOAliasHandler` (`language`/`module`/`function`/`index`) addressing a handler and the `JobArgument` (`name`/`value`) the input handler coerces are queenbee models.
- `lbt-recipes`(`lbt-recipes.md`): `_RecipeParameter` is the `importlib` resolution site, `RecipeInput.handle_value`/`RecipeOutput.value` the invocation sites, and `RecipeInput.INPUT_TYPES` casts the handler output to the `DAGInput` python type.
- runtime rails: handler invocation rides the recipe-execution process lane inside the recipe-run OpenTelemetry span; a handler `ValueError` lifts into the boundary-fault `Result` rail and the run receipt.

[LOCAL_ADMISSION]:
- AGPL-3.0: network-copyleft — handlers run in the recipe-execution process bracketing a subprocess, never linked into a distributed library surface; the license is the binding admission flag.
- authoring boundary: the geometry energy domain authors which handler each input uses; this catalog owns the execution-time resolution and invocation of the same package.

[RAIL_LAW]:
- Package: `pollination-handlers`
- Owns: the input-coercion and result-reader handler functions the recipe executor binds at the IO boundary — value-to-artifact before the subprocess, result-folder-to-`DataCollection` after it
- Accept: `module`+`function` addressing from a queenbee `IOAliasHandler`, `importlib` resolution by `lbt-recipes`, `index`-ordered chained handlers, invocation at `RecipeInput.handle_value`/`RecipeOutput.value` bracketing the `queenbee local run` subprocess, a handler `ValueError` lifted into the boundary-fault rail
- Reject: a static handler import, re-implemented handler resolution or chain ordering, a handler run inside the luigi worker, a parallel result-folder parser, a swallowed handler `ValueError`

# [PY_RUNTIME_API_POLLINATION_HANDLERS]

`pollination-handlers` (import `pollination_handlers`) is the execution-time IO coercion layer for the runtime recipe rail: the flat catalog of `inputs.*` and `outputs.*` functions that a recipe's queenbee `IOAliasHandler(language, module, function, index)` names, that `lbt-recipes` resolves by `importlib.import_module(module)` + `getattr(module, function)`, and that the runtime invokes at two boundaries of a recipe run — coercing each job input value to the artifact/scalar the `queenbee local run` subprocess expects (`inputs.*`, ordered by `index` for chained handlers), and reading the finished result folder back into ladybug `DataCollection`s and summary dicts (`outputs.*`). It pairs with `runtime/.api/queenbee.md` (the `IOAliasHandler`/`JobArgument` schema that addresses these functions) and `runtime/.api/lbt-recipes.md` (the `_RecipeParameter` resolver and the `RecipeInput.handle_value`/`RecipeOutput.value` invocation sites). The geometry energy domain authors which handler an input uses (`geometry/.api/pollination-handlers.md`); the runtime owns resolving and running them under its process-resource lane. The package owner composes the handler catalog into the recipe IO-coercion seam; it never re-implements handler resolution, the chained-handler ordering, or the result-folder parsing this package and `lbt-recipes` already own.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `pollination-handlers`
- package: `pollination-handlers`
- import: `import pollination_handlers`
- owner: `runtime`
- rail: recipe-execution / io-coercion
- installed: `0.10.10`
- license: `AGPL-3.0` (network copyleft; the binding admission flag for this distribution)
- abi: pure-Python (`py2.py3-none-any`, purelib; no native extension)
- depends: `lbt-dragonfly (>=0.9.453)` — honeybee/honeybee-energy/honeybee-radiance, ladybug/ladybug-comfort, dragonfly/dragonfly-energy (the domain classes the handlers read and write)
- entry points: none (library only; functions are addressed by `module`+`function` from a queenbee `IOAliasHandler` and resolved via `importlib`)
- capability: the input-coercion handlers (object/value -> recipe-input artifact) and result-reader handlers (result folder -> ladybug `DataCollection`/dict) that the recipe executor binds and invokes at the recipe IO boundary

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: package layout (resolution targets)
- rail: io-coercion

No classes — two module trees of pure functions addressed by dotted `module`+`function`. The module path is the resolution contract: `lbt-recipes` calls `importlib.import_module('pollination_handlers.inputs.model')` then `getattr(mod, 'model_to_json')`.

| [INDEX] | [SYMBOL]                       | [TYPE_FAMILY] | [CAPABILITY]                                          |
| :-----: | :----------------------------- | :------------ | :-------------------------------------------------- |
|  [01]   | `pollination_handlers.inputs`  | module tree   | resolved at job-argument time: value -> recipe-input artifact |
|  [02]   | `pollination_handlers.outputs` | module tree   | resolved at run-completion time: result folder -> ladybug object/dict |
|  [03]   | `pollination_handlers.inputs.helper` / `pollination_handlers.outputs.helper` | helper module | tempfile/CSV writers; `read_sensor_grid_result`/`read_grid_results` shared readers |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: input-coercion handlers (invoked by `RecipeInput.handle_value`)
- rail: io-coercion

At `handle_value` the executor runs the handler chain (ordered by `IOAliasHandler.index`) on the input value, then casts to the `DAGInput` type. Each handler is polymorphic on the value — a domain object is serialized to a temp artifact, a path/scalar is validated and returned.

| [INDEX] | [SURFACE]                                                                       | [ENTRY_FAMILY] | [CAPABILITY]                              |
| :-----: | :------------------------------------------------------------------------------ | :------------- | :---------------------------------------- |
|  [01]   | `inputs.model.model_to_json(model_obj)` (+ precondition variants `model_to_json_{room,hvac,grid,grid_room,view}_check`) / `inputs.model.model_dragonfly_to_json` | model coercion | honeybee/dragonfly `Model` \| path -> HBJSON/DFJSON path; the `_check` variants assert Rooms/HVAC/SensorGrids/Views exist before serializing |
|  [02]   | `inputs.wea.wea_handler` (+ `wea_handler_timestep_check`/`wea_handler_timestep_annual_check`) / `inputs.ddy.ddy_handler` | weather coercion | ladybug `Wea`/`EPW`/`DDY` \| path -> weather artifact; the timestep variants assert timestep==1 / annual coverage |
|  [03]   | `inputs.simulation.energy_sim_par_to_json` / `measures_to_folder` / `list_to_additional_strings` / `list_to_additional_idf` / `viz_variables_to_string` / `standard_to_str` | sim coercion | simulation parameter/measure/list coercion |
|  [04]   | `inputs.data.value_or_data_to_str` / `value_or_data_to_file` / `value_or_data_to_air_speed_file` / `value_or_data_to_met_file` / `value_or_data_to_clo_file` / `inputs.schedule.schedule_to_csv` / `inputs.schedule.data_to_csv` | data coercion | scalar \| `DataCollection` -> str/CSV/per-sensor file |
|  [05]   | `inputs.north.north_vector_to_angle` / `inputs.runperiod.run_period_to_str` / `inputs.emissions.location_to_electricity_emissions` / `inputs.pit.point_in_time_metric_to_str` / `inputs.postprocess.grid_metrics` | value coercion | orientation/period/emissions/metric coercion |
|  [06]   | `inputs.bool_options.*` (`use_multiplier_to_str`, `is_residential_to_str`, `cloudy_bool_to_str`, `sky_view_metric_to_str`, `visible_vs_solar_to_str`, `glare_control_devices_to_str`, `filter_des_days_to_str`, `skip_overture_to_str`, `write_set_map_to_str`, `bldg_lighting_to_str`) / `inputs.helper.bool_option_to_str` | option coercion | bool/option -> the acceptable CLI flag string the recipe expects |

[ENTRYPOINT_SCOPE]: result-reader handlers (invoked by `RecipeOutput.value`)
- rail: io-coercion

At `RecipeOutput.value(simulation_folder)` the executor joins the output path and runs the output handler chain, turning the raw result folder into a typed Python object the job receipt carries.

| [INDEX] | [SURFACE]                                                                                         | [ENTRY_FAMILY] | [CAPABILITY]                              |
| :-----: | :----------------------------------------------------------------------------------------------- | :------------- | :---------------------------------------- |
|  [01]   | `outputs.daylight.read_da_from_folder` / `read_udi_from_folder` / `read_cda_from_folder` / `read_ga_from_folder` / `read_df_from_folder` / `read_pit_from_folder` / `read_hours_from_folder` / `read_ase_from_folder` | metric reader | radiance result folder -> per-sensor metric lists |
|  [02]   | `outputs.daylight.sort_ill_from_folder` / `read_images_from_folder` / `read_leed_datacollection_from_folder` | collection reader | sorted `.ill` files, images, LEED `DataCollection` |
|  [03]   | `outputs.daylight.read_leed_summary_grid` / `read_leed_shade_transmittance_schedule` / `read_grid_metrics` / `ill_credit_json_from_path` / `read_json_dict` / `read_json_summary_list` | summary reader | LEED/credit summaries and grid-metric dicts |
|  [04]   | `outputs.comfort.read_comfort_percent_from_folder` / `outputs.eui.eui_json_from_path` / `outputs.summary.json_properties_from_path` / `outputs.summary.contents_from_folder` | summary reader | comfort percent, EUI, summary properties/contents |
|  [05]   | `outputs.helper.read_sensor_grid_result(result_folder, extension, grid_key, is_percent=True, factor=1)` / `outputs.helper.read_grid_results(result_folder, extension, grid_key, is_percent=True, factor=1)` | reader primitive | the shared per-grid result-file readers the daylight handlers compose over |

## [04]-[IMPLEMENTATION_LAW]

[BINDING_TOPOLOGY]:
- resolution law: a handler is bound, not imported statically — `lbt_recipes._RecipeParameter` reads the recipe alias `handler` spec and resolves `importlib.import_module(handler['module'])` + `getattr(module, handler['function'])`. The `module`+`function` string from the queenbee `IOAliasHandler` is the only address; the runtime never hard-codes a handler import.
- chain-order law: when an input declares more than one handler they run in `IOAliasHandler.index` order (output of one feeds the next); the executor applies the full chain in `handle_value` before casting. Handlers are composable, not single-shot.
- boundary law: input handlers run at `RecipeInput.handle_value` (before the `inputs.json` is written for the subprocess) and output handlers at `RecipeOutput.value` (after the subprocess completes). Both run in the runtime's process, not inside the `queenbee local run` luigi worker — they bracket the subprocess.
- purity law: handlers are pure value maps with no recipe-execution side effects (input handlers only write temp artifacts under `inputs.helper.get_tempfile`); a failing precondition raises `ValueError`, which the runtime lifts into a typed boundary fault at the IO seam.

[STACK_LAW]:
- `queenbee` (schema): the `IOAliasHandler(language='python', module=..., function=..., index=...)` that addresses a handler and the `JobArgument` whose value the input handler coerces are queenbee models (`runtime/.api/queenbee.md`).
- `lbt-recipes` (resolver/invoker): `_RecipeParameter` resolves the handler via `importlib`; `RecipeInput.handle_value` and `RecipeOutput.value` are the invocation sites; `RecipeInput.INPUT_TYPES` casts the handler output to the `DAGInput` python type (`runtime/.api/lbt-recipes.md`).
- runtime rails: handler invocation rides the recipe-execution process lane; a handler `ValueError` (missing rooms/grids, bad path) lifts into the boundary-fault `Result` rail and the run receipt, never an inline exception; the bracketing input/output coercion is part of the recipe-run OpenTelemetry span.
- domain authoring: which handler an input uses is the geometry energy domain's choice (`geometry/.api/pollination-handlers.md`); the runtime owns resolving and running it.

## [05]-[LOCAL_ADMISSION]

[RAIL_LAW]:
- Package: `pollination-handlers`
- Owns: the input-coercion and result-reader handler functions the recipe executor binds at the IO boundary — value-to-artifact coercion before the subprocess, result-folder-to-`DataCollection` reading after it
- Accept: `module`+`function` addressing from a queenbee `IOAliasHandler`, `importlib`-based resolution by `lbt-recipes`, `index`-ordered chained handlers, invocation at `RecipeInput.handle_value`/`RecipeOutput.value` bracketing the `queenbee local run` subprocess, handler `ValueError` lifted into the boundary-fault rail
- Reject: static/hard-coded handler imports, re-implementing handler resolution or the chained-handler ordering, running a handler inside the luigi worker rather than bracketing the subprocess, a parallel result-folder parser, swallowing a handler `ValueError` instead of lifting it into the receipt

[CAPTURE_GAP]:
- AGPL-3.0: this distribution is network-copyleft; handlers run in the recipe-execution process bracketing a subprocess, never linked into a distributed library surface. The license is the binding admission flag.
- domain authoring: the geometry energy domain owns which handler each input uses; this catalog is the execution-time resolution + invocation surface of the same package (`geometry/.api/pollination-handlers.md`).

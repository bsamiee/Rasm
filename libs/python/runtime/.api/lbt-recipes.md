# [PY_RUNTIME_API_LBT_RECIPES]

`lbt-recipes` (import `lbt_recipes`) owns out-of-process execution of the packaged Ladybug Tools simulation recipes: a `Recipe` loads a recipe by name or folder, coerces each input through its `pollination_handlers` chain to the queenbee `DAGInput` type, writes `<simulation_id>_inputs.json`, shells `queenbee local run` — the `queenbee-local` luigi DAG runner — over Radiance/OpenStudio/EnergyPlus, then reads the result folder back through the output handlers.

`lbt-recipes` holds the executor boundary of the recipe rail: queenbee owns the consumed schema and `pollination-handlers` the bound IO coercion, leaving lbt-recipes the subprocess run and its luigi/error-log receipt; the runtime composes `Recipe` + `RecipeSettings` + `Recipe.run` into the simulation-job owner.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `lbt-recipes`
- package: `lbt-recipes` (AGPL-3.0, network copyleft)
- module: `lbt_recipes`
- abi: pure-Python (`py2.py3-none-any`, purelib; no native extension), shelling out to the Radiance, OpenStudio, and EnergyPlus engines
- rail: recipe-execution
- depends: `pollination-handlers` (the IO handler chain), `queenbee-local` (the luigi `queenbee local run` executor — the subprocess target), `click`; transitively honeybee/honeybee-radiance/honeybee-energy/ladybug for `Model`/config/engine folders

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: recipe execution family

`Recipe` runs a single-output recipe; `RecipeInput`/`RecipeOutput` extend `_RecipeParameter`, each wrapping one IO with its handler chain, and `RecipeInput.INPUT_TYPES` binds the queenbee `DAGInput` type string to its python caster (`str`/`int`/`float`/`bool`/`tuple`/`dict`).

| [INDEX] | [SYMBOL]                  | [TYPE_FAMILY] | [CAPABILITY]                                                                            |
| :-----: | :------------------------ | :------------ | :-------------------------------------------------------------------------------------- |
|  [01]   | `recipe.Recipe`           | runner        | load a recipe by name/folder; handle inputs; `run`; read outputs                        |
|  [02]   | `recipe.RecipeInput`      | io wrapper    | one input: `value` (handler-triggering setter), `INPUT_TYPES`/`PATH_TYPES`, `is_path`   |
|  [03]   | `recipe.RecipeOutput`     | io wrapper    | one output: `value(simulation_folder)` runs the output handler chain                    |
|  [04]   | `recipe._RecipeParameter` | io base       | `name`/`description`/`handlers` — resolves handlers via `importlib` from the alias spec |
|  [05]   | `settings.RecipeSettings` | settings      | `folder`/`workers`/`reload_old`/`report_out`/`debug_folder` run config                  |

[PUBLIC_TYPE_SCOPE]: packaged recipe set

Each packaged recipe is a folder — `package.json` contract + `run.py` luigi entry + grasshopper handler aliases — addressed by `Recipe(recipe_name)`; `__init__` joins `recipe_name.replace('-', '_')` onto the install folder, so hyphens normalize to the underscored internal name.

| [INDEX] | [SYMBOL]                   | [TYPE_FAMILY] | [CAPABILITY]                              |
| :-----: | :------------------------- | :------------ | :---------------------------------------- |
|  [01]   | `annual_daylight`          | daylight      | annual daylight metrics (DA/UDI/sDA/ASE)  |
|  [02]   | `annual_daylight_enhanced` | daylight      | higher-accuracy two-phase daylight        |
|  [03]   | `annual_irradiance`        | radiance      | annual irradiance                         |
|  [04]   | `cumulative_radiation`     | radiance      | cumulative radiation                      |
|  [05]   | `direct_sun_hours`         | radiance      | direct sun hours                          |
|  [06]   | `daylight_factor`          | radiance      | daylight factor                           |
|  [07]   | `point_in_time_grid`       | radiance      | point-in-time grid                        |
|  [08]   | `point_in_time_view`       | radiance      | point-in-time view                        |
|  [09]   | `sky_view`                 | radiance      | sky view                                  |
|  [10]   | `imageless_annual_glare`   | radiance      | annual glare autonomy without images      |
|  [11]   | `annual_energy_use`        | energy        | OpenStudio/EnergyPlus annual energy + EUI |
|  [12]   | `adaptive_comfort_map`     | comfort       | adaptive thermal-comfort map              |
|  [13]   | `pmv_comfort_map`          | comfort       | PMV thermal-comfort map                   |
|  [14]   | `utci_comfort_map`         | comfort       | UTCI thermal-comfort map                  |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: recipe construction and inputs

`Recipe(recipe_name)` loads the recipe's `package.json` and builds the `RecipeInput`/`RecipeOutput` wrappers (each resolving its handler chain via `importlib`); setting a value defers handling, and `handle_inputs`/`handle_value` runs the chain and casts to the `DAGInput` type.

| [INDEX] | [SURFACE]                                                        | [SHAPE]  | [CAPABILITY]                                |
| :-----: | :--------------------------------------------------------------- | :------- | :------------------------------------------ |
|  [01]   | `Recipe(recipe_name)`                                            | ctor     | load a packaged recipe by name or folder    |
|  [02]   | `.input_value_by_name(input_name, input_value)`                  | instance | set one input (handler fires on `handle`)   |
|  [03]   | `.handle_inputs()`                                               | instance | run every input handler chain               |
|  [04]   | `.inputs` / `.outputs` / `.input_names` / `.output_names`        | property | the IO wrapper tuples and their names       |
|  [05]   | `.default_project_folder` (get/set) / `.simulation_id` (get/set) | property | project-folder and simulation-id resolution |
|  [06]   | `.name` / `.tag` / `.path`                                       | property | recipe name, version tag, install folder    |

[ENTRYPOINT_SCOPE]: execution and result reading

`run` writes the handled `<simulation_id>_inputs.json`, shells `queenbee local run` with `--workers`/`--env PATH,RAYPATH`/`--name <simulation_id>` (with an optional `--debug`), and returns the project folder; result handlers run through `output_value_by_name`.
- `run` params (all optional): `settings`, `radiance_check`, `openstudio_check`, `energyplus_check`, `queenbee_path`, `silent`, `debug_folder`; `write_inputs_json(project_folder, indent=4, cpu_count)` where `cpu_count` overrides the recipe's `cpu-count` input. Both return a path (`-> str`).

| [INDEX] | [SURFACE]                                                 | [SHAPE]  | [CAPABILITY]                                           |
| :-----: | :-------------------------------------------------------- | :------- | :----------------------------------------------------- |
|  [01]   | `.write_inputs_json(...) -> str`                          | instance | handle inputs, copy path artifacts, write inputs.json  |
|  [02]   | `.run(...) -> str`                                        | instance | `queenbee local run` subprocess; return project folder |
|  [03]   | `.output_value_by_name(output_name, project_folder=None)` | instance | resolve a result path + run its output handlers        |
|  [04]   | `.luigi_execution_summary(project_folder=None)`           | instance | parse the luigi `logs.log` into a summary              |
|  [05]   | `.error_summary(...)` / `.failure_message(...)`           | instance | parse `err.log`; the failure message                   |
|  [06]   | `RecipeInput.handle_value()`                              | instance | run the handler chain + cast                           |
|  [07]   | `RecipeInput.is_path` / `.is_required` / `.is_handled`    | property | path/required/handled flags                            |
|  [08]   | `RecipeOutput.value(simulation_folder)`                   | instance | join the output path + run the output handler chain    |

- `lbt_recipes.cli:main`: drives the same run out-of-process as the `lbt-recipes` console script.

[ENTRYPOINT_SCOPE]: settings and engine checks

`RecipeSettings(folder, workers, reload_old, report_out, debug_folder)` carries the run knobs, `workers` defaulting to `cpu_count - 1`; the `version.check_*` gates assert a compatible engine against the `version.RADIANCE_DATE`/`OS_VERSION`/`EP_VERSION` floors before a run.

| [INDEX] | [SURFACE]                                                        | [SHAPE]  | [CAPABILITY]                                  |
| :-----: | :--------------------------------------------------------------- | :------- | :-------------------------------------------- |
|  [01]   | `RecipeSettings(...)`                                            | ctor     | construct run config                          |
|  [02]   | `.from_string(settings_string)` / `.duplicate()`                 | instance | parse a `--folder/--workers/...` string; copy |
|  [03]   | `.workers / .folder / .reload_old / .report_out / .debug_folder` | property | run knobs (`workers` -> `cpu_count - 1`)      |
|  [04]   | `version.check_radiance_date()`                                  | static   | assert compatible Radiance installed          |
|  [05]   | `version.check_openstudio_version()`                             | static   | assert compatible OpenStudio installed        |
|  [06]   | `version.check_energyplus_version()`                             | static   | assert compatible EnergyPlus installed        |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- subprocess law: `Recipe.run` shells `queenbee local run` (the `queenbee-local` luigi runner) with `--workers`, `--name <simulation_id>`, an `--env PATH/RAYPATH` for the Radiance binaries, and a cleared `PYTHONHOME`, never running the DAG in-process; the runtime drives it through the process-resource lane under a deadline scope and parses the result rather than trusting the exit code alone.
- handler-binding law: `_RecipeParameter` resolves each handler via `importlib.import_module(module)` + `getattr(module, function)` from the recipe's grasshopper alias spec; `RecipeInput.handle_value` runs the chain and casts through `INPUT_TYPES`.
- result law: `Recipe` targets single-output recipes whose `results` output is a folder; `output_value_by_name` joins the project/simulation path and runs the `pollination_handlers.outputs.*` reader, so the product is a ladybug `DataCollection`/dict, never a raw path.
- engine-gate law: pass `radiance_check`/`openstudio_check`/`energyplus_check=True` (or call `version.check_*`) so a missing/incompatible engine fails before the subprocess, not mid-run.

[STACKING]:
- `queenbee`(`queenbee.md`): `RecipeInput.INPUT_TYPES` keys are queenbee `DAGInput` type strings and the `package.json` at `Recipe.path` is a queenbee recipe contract — loadable as `BakedRecipe.from_folder` submitting as a `Job`, the schema-side complement to the `queenbee local run` luigi path `Recipe.run` drives.
- `pollination-handlers`(`pollination-handlers.md`): the input/output handler chains are `pollination_handlers` functions; lbt-recipes is the `importlib` resolution site and the `RecipeInput.handle_value`/`RecipeOutput.value` invocation site.
- runtime rails: `Recipe.run` rides the process-resource lane + `anyio` deadline scope; the luigi/error logs parse into the runtime receipt; a transient engine-precheck retries through the `stamina` owner; the run is an OpenTelemetry span.

[LOCAL_ADMISSION]:
- AGPL-3.0: this distribution and its `queenbee-local` executor are network-copyleft, admitted only as a process-boundary subprocess, never linked into a distributed library surface — the license is the binding admission flag.
- `queenbee-local` reaches only through the `queenbee local run` CLI; it is the subprocess target, not a directly composed library, and carries no catalog of its own.

[RAIL_LAW]:
- Package: `lbt-recipes`
- Owns: the recipe execution wrapper, the packaged LBT recipes, handler-driven typed input coercion, parametric input/output management, local luigi execution via `queenbee local run`, engine-version checks, and luigi/error-log parsing
- Accept: `Recipe(recipe_name)` construction, `input_value_by_name`/`handle_inputs` then `run(settings=RecipeSettings(...))`, `output_value_by_name` for typed results, `version.check_*` engine gates, the run driven through the runtime process-resource lane + deadline scope + receipt + OTel span
- Reject: re-implementing the luigi DAG scheduler or `queenbee local run`, an in-process recipe runner, hand-rolled handler resolution or input casting, trusting the subprocess exit code without parsing the logs, running without an engine precheck

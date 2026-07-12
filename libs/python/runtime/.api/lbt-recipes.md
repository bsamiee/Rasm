# [PY_RUNTIME_API_LBT_RECIPES]

`lbt-recipes` (import `lbt_recipes`) is the recipe execution wrapper and simulation-workflow owner for the runtime recipe rail: a `Recipe` class that loads one of the 14 packaged Ladybug Tools recipes (or an external recipe folder), runs each input through its `pollination_handlers` handler chain, casts it to the type the queenbee `DAGInput` declares, writes a handled `<simulation_id>_inputs.json`, and executes the recipe by shelling out to `queenbee local run` — the `queenbee-local` luigi DAG runner — over Radiance/OpenStudio/EnergyPlus engines, then reads the result folder back through the output handlers. It pairs with `runtime/.api/queenbee.md` (the schema the executor consumes) and `runtime/.api/pollination-handlers.md` (the IO coercion the executor binds); the geometry energy plane (`geometry/.planning/energy/simulate.md`) binds the model-input and result-readback side, while this catalog owns the out-of-process execution. Because `Recipe.run` is an out-of-process subprocess, the runtime owner wraps it under the process-resource lane, a deadline scope, a `stamina` engine-precheck, and a `structlog`/OpenTelemetry span, parsing the luigi execution and error logs into a typed receipt rather than trusting the exit code. The package owner composes `Recipe(recipe_name)` + `RecipeSettings` + `Recipe.run` into the simulation-job owner; it never re-implements the luigi DAG scheduler, the handler resolution, or the engine-version checks lbt-recipes already owns.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `lbt-recipes`
- package: `lbt-recipes`
- import: `import lbt_recipes`
- owner: `runtime`
- rail: recipe-execution
- installed: `0.26.17`
- license: `AGPL-3.0` (network copyleft; the binding admission flag for this distribution)
- abi: pure-Python (`py2.py3-none-any`, purelib; no native extension), but shells out to native simulation engines (below)
- depends: `pollination-handlers==0.10.10` (the IO handler chain), `queenbee-local==0.6.7` (the luigi-based `queenbee local run` executor — the subprocess target), `click==8.1.7`; transitively honeybee/honeybee-radiance/honeybee-energy/ladybug for `Model`/config/engine folders
- entry points: `lbt-recipes` console script (`lbt_recipes.cli:main`)
- external engines: Radiance, OpenStudio, and EnergyPlus binaries are required at run time and validated by `lbt_recipes.version` against the pinned compatibility floor (`RADIANCE_DATE=(2021, 3, 28)`, `OS_VERSION=(3, 7, 0)`, `EP_VERSION=(23, 2, 0)`)
- capability: load/execute the 14 packaged LBT recipes (or an external recipe folder), handler-driven typed input coercion, parametric input/output management, local luigi execution via `queenbee local run`, and luigi/error-log parsing for run summaries

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: recipe execution family
- rail: recipe-execution

`Recipe` is the single-output recipe runner; `RecipeInput`/`RecipeOutput` (both extend `_RecipeParameter`) wrap one IO with its handler chain. `RecipeInput.INPUT_TYPES` is the binding from the queenbee `DAGInput` type string to a python caster.

| [INDEX] | [SYMBOL]                  | [TYPE_FAMILY] | [CAPABILITY]                                                                            |
| :-----: | :------------------------ | :------------ | :-------------------------------------------------------------------------------------- |
|  [01]   | `recipe.Recipe`           | runner        | load a recipe by name/folder; handle inputs; `run`; read outputs                        |
|  [02]   | `recipe.RecipeInput`      | io wrapper    | one input: `value` (handler-triggering setter), `INPUT_TYPES`/`PATH_TYPES`, `is_path`   |
|  [03]   | `recipe.RecipeOutput`     | io wrapper    | one output: `value(simulation_folder)` runs the output handler chain                    |
|  [04]   | `recipe._RecipeParameter` | io base       | `name`/`description`/`handlers` — resolves handlers via `importlib` from the alias spec |
|  [05]   | `settings.RecipeSettings` | settings      | `folder`/`workers`/`reload_old`/`report_out`/`debug_folder` run config                  |

[PUBLIC_TYPE_SCOPE]: packaged recipe set
- rail: recipe-execution

Each packaged recipe is a sub-package folder (a `package.json` recipe contract + `run.py` luigi entry + grasshopper handler aliases) addressed by name through `Recipe(recipe_name)`; `Recipe.__init__` joins `recipe_name.replace('-', '_')` onto the install folder, so names use underscores internally and hyphens normalize. The 14 folders carrying a `package.json`:

| [INDEX] | [SYMBOL]                                                                     | [TYPE_FAMILY]   | [CAPABILITY]                                                                                  |
| :-----: | :--------------------------------------------------------------------------- | :-------------- | :-------------------------------------------------------------------------------------------- |
|  [01]   | `annual_daylight` / `annual_daylight_enhanced`                               | daylight recipe | annual daylight metrics (DA/UDI/sDA/ASE); `_enhanced` runs the higher-accuracy two-phase path |
|  [02]   | `annual_irradiance` / `cumulative_radiation` / `direct_sun_hours`            | radiance recipe | annual irradiance, cumulative radiation, sun hours                                            |
|  [03]   | `daylight_factor` / `point_in_time_grid` / `point_in_time_view` / `sky_view` | radiance recipe | daylight factor, point-in-time grid/view, sky view                                            |
|  [04]   | `imageless_annual_glare`                                                     | radiance recipe | annual glare autonomy without images                                                          |
|  [05]   | `annual_energy_use`                                                          | energy recipe   | OpenStudio/EnergyPlus annual energy + EUI                                                     |
|  [06]   | `adaptive_comfort_map` / `pmv_comfort_map` / `utci_comfort_map`              | comfort recipe  | spatial thermal-comfort mapping (adaptive/PMV/UTCI)                                           |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: recipe construction and inputs
- rail: recipe-execution

`Recipe(recipe_name)` loads the recipe's `package.json` and builds `RecipeInput`/`RecipeOutput` wrappers (each resolving its python handler chain via `importlib`). Setting a value defers handling; `handle_inputs`/`handle_value` runs the chain and casts to the `DAGInput` type.

| [INDEX] | [SURFACE]                                                                         | [ENTRY_FAMILY] | [CAPABILITY]                                            |
| :-----: | :-------------------------------------------------------------------------------- | :------------- | :------------------------------------------------------ |
|  [01]   | `Recipe(recipe_name)`                                                             | construct      | load a packaged recipe by name or a recipe folder path  |
|  [02]   | `Recipe.input_value_by_name(input_name, input_value)`                             | set input      | set one input (triggers its handler on `handle`)        |
|  [03]   | `Recipe.handle_inputs()`                                                          | handle         | run every input handler chain, ready for simulation     |
|  [04]   | `Recipe.inputs` / `Recipe.outputs` / `Recipe.input_names` / `Recipe.output_names` | accessor       | the `RecipeInput`/`RecipeOutput` tuples and their names |
|  [05]   | `Recipe.default_project_folder` (get/set) / `Recipe.simulation_id` (get/set)      | accessor       | project-folder and simulation-id resolution             |
|  [06]   | `Recipe.name` / `Recipe.tag` / `Recipe.path`                                      | accessor       | recipe name, version tag, install folder                |

[ENTRYPOINT_SCOPE]: execution and result reading
- rail: recipe-execution

`run` writes the handled `<simulation_id>_inputs.json` and shells `"<queenbee>" local run "<recipe>" "<project>" -i "<inputs.json>" --workers N --env PATH/RAYPATH --name <simulation_id>` (plus an optional `--debug <folder>`); it returns the project folder. Result handlers are run by `output_value_by_name`.

| [INDEX] | [SURFACE]                                                                                                                                                     | [ENTRY_FAMILY] | [CAPABILITY]                                                                                                                                                    |
| :-----: | :------------------------------------------------------------------------------------------------------------------------------------------------------------ | :------------- | :-------------------------------------------------------------------------------------------------------------------------------------------------------------- |
|  [01]   | `Recipe.write_inputs_json(project_folder=None, indent=4, cpu_count=None) -> str`                                                                              | prepare        | handle inputs + copy path artifacts into the project folder + write `<simulation_id>_inputs.json` (returns its path); `cpu_count` overrides a `cpu-count` input |
|  [02]   | `Recipe.run(settings=None, radiance_check=False, openstudio_check=False, energyplus_check=False, queenbee_path=None, silent=False, debug_folder=None) -> str` | execute        | run via `queenbee local run` subprocess; return project folder                                                                                                  |
|  [03]   | `Recipe.output_value_by_name(output_name, project_folder=None)`                                                                                               | read output    | resolve a result path + run its output handlers                                                                                                                 |
|  [04]   | `Recipe.luigi_execution_summary(project_folder=None)` / `Recipe.error_summary(...)` / `Recipe.failure_message(...)`                                           | read log       | parse the luigi `logs.log`/`err.log` into a summary                                                                                                             |
|  [05]   | `RecipeInput.handle_value()` / `RecipeInput.is_path` / `RecipeInput.is_required` / `RecipeInput.is_handled`                                                   | io             | run the handler chain + cast; path/required/handled flags                                                                                                       |
|  [06]   | `RecipeOutput.value(simulation_folder)`                                                                                                                       | io             | join the output path + run the output handler chain                                                                                                             |

[ENTRYPOINT_SCOPE]: settings and engine checks
- rail: recipe-execution

`RecipeSettings` carries the run knobs; `workers` defaults to `cpu_count - 1`. The `version.check_*` functions assert a compatible engine is installed before a run.

| [INDEX] | [SURFACE]                                                                                                                                      | [ENTRY_FAMILY] | [CAPABILITY]                                                    |
| :-----: | :--------------------------------------------------------------------------------------------------------------------------------------------- | :------------- | :-------------------------------------------------------------- |
|  [01]   | `RecipeSettings(folder=None, workers=None, reload_old=False, report_out=False, debug_folder=None)`                                             | settings       | construct run config                                            |
|  [02]   | `RecipeSettings.from_string(settings_string)` / `RecipeSettings.duplicate()`                                                                   | settings       | parse a `--folder/--workers/...` string; copy                   |
|  [03]   | `RecipeSettings.workers` / `RecipeSettings.folder` / `RecipeSettings.reload_old` / `RecipeSettings.report_out` / `RecipeSettings.debug_folder` | property       | run knobs (`workers` -> `cpu_count - 1`)                        |
|  [04]   | `version.check_radiance_date()` / `version.check_openstudio_version()` / `version.check_energyplus_version()`                                  | engine check   | assert a compatible Radiance/OpenStudio/EnergyPlus is installed |

## [04]-[IMPLEMENTATION_LAW]

[EXECUTION_TOPOLOGY]:
- subprocess law: `Recipe.run` does not execute the DAG in-process — it shells `queenbee local run` (the `queenbee-local` luigi runner) with `--workers`, `--name <simulation_id>`, an `--env PATH/RAYPATH` for the Radiance binaries, and a cleared `PYTHONHOME` in the copied subprocess env. The runtime owner runs it through the process-resource lane under a deadline scope and reads the result, never treating the exit code alone as success.
- handler-binding law: `_RecipeParameter` resolves each handler with `importlib.import_module(module)` + `getattr(module, function)` from the recipe's grasshopper alias spec, then `RecipeInput.handle_value` runs the chain and casts via `INPUT_TYPES` (the queenbee `DAGInput` type -> `str`/`int`/`float`/`bool`/`tuple`/`dict`). The handlers live in `pollination-handlers`.
- result law: `Recipe` targets single-output recipes whose `results` output is a folder; `output_value_by_name` joins the project/simulation path and runs the `pollination_handlers.outputs.*` reader, so the run product is a ladybug `DataCollection`/dict, not a raw path.
- engine-gate law: the recipe needs external Radiance/OpenStudio/EnergyPlus binaries; pass `radiance_check`/`openstudio_check`/`energyplus_check=True` (or call `version.check_*`) so a missing/incompatible engine fails with a clear message before the subprocess, not mid-run.

[STACK_LAW]:
- `queenbee` (schema): `RecipeInput.INPUT_TYPES` keys are queenbee `DAGInput` type strings, and the packaged recipe `package.json` at `Recipe.path` is a queenbee recipe contract — loadable as a `BakedRecipe.from_folder(...)` that submits as a queenbee `Job` (the `BakedRecipe` -> `Job` schema path queenbee owns), the schema-side complement to the local `queenbee local run` luigi path `Recipe.run` drives. lbt-recipes is the executor over the schema queenbee owns (`runtime/.api/queenbee.md`).
- `pollination-handlers` (IO coercion): the input/output handler chains are `pollination_handlers` functions resolved by `importlib`; lbt-recipes is the resolution + invocation site (`runtime/.api/pollination-handlers.md`).
- runtime rails: `Recipe.run` rides the process-resource lane + `anyio` deadline scope (subprocess execution under structured concurrency); the luigi/error logs parse into the runtime receipt; transient engine-precheck failures retry through the `stamina` owner; the run is an OpenTelemetry span — the exit code is never the sole success signal.

## [05]-[LOCAL_ADMISSION]

[RAIL_LAW]:
- Package: `lbt-recipes`
- Owns: the recipe execution wrapper, the 14 packaged LBT recipes, handler-driven typed input coercion, parametric input/output management, local luigi execution via `queenbee local run`, engine-version checks, and luigi/error-log parsing
- Accept: `Recipe(recipe_name)` construction, `input_value_by_name`/`handle_inputs` then `run(settings=RecipeSettings(...))`, `output_value_by_name` for typed results, `version.check_*` engine gates, the run executed through the runtime process-resource lane + deadline scope + receipt + OTel span
- Reject: re-implementing the luigi DAG scheduler or `queenbee local run`, an in-process recipe runner, hand-rolled handler resolution or input casting, trusting the subprocess exit code without parsing the logs, running without an engine precheck

[CAPTURE_GAP]:
- AGPL-3.0: this distribution and its `queenbee-local` executor are network-copyleft; recipe execution is a process-boundary subprocess invoked by the runtime, never linked into a distributed library surface. The license is the binding admission flag.
- `queenbee-local`: the luigi runner reached by `Recipe.run` is a transitive dependency (`queenbee-local==0.6.7`) exposed only through the `queenbee local run` CLI; it has no admitted catalog of its own — it is the subprocess target, not a directly composed library.

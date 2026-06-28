# [PY_GEOMETRY_API_LBT_RECIPES]

`lbt-recipes` is the recipe contract and execution adapter over the Ladybug Tools simulation workflows: it packages 19 named queenbee recipe DAGs (`annual_daylight`, `annual_energy_use`, `annual_irradiance`, `daylight_factor`, `direct_sun_hours`, `point_in_time_grid`/`_view`, `sky_view`, `cumulative_radiation`, `imageless_annual_glare`, the `adaptive_comfort_map`/`pmv_comfort_map`/`utci_comfort_map` comfort maps, and the daylight-credit recipes `leed_daylight_option_one`/`_two`, `breeam_daylight_4b`, `well_daylight`, `annual_daylight_en17037`/`_enhanced`), binds their typed `RecipeInput`/`RecipeOutput` descriptors through `pollination-handlers`, validates the external simulation-engine versions (EnergyPlus, OpenStudio, Radiance), and dispatches execution to the `queenbee-local` Luigi engine. It is the RUNTIME-tier adapter: geometry/honeybee builds the model, the recipe owner here binds the recipe and coerces its inputs, and the runtime owns the actual Luigi DAG execution and the native-engine subprocess; results are read back through `ladybug-core` `SQLiteResult`/result files and `ladybug-comfort` map kernels into `DataCollection`s. The recipe owner composes `Recipe(recipe_name)`, the `handle_inputs` coercion, `RecipeSettings`, and the version gate into one recipe-binding owner; it never re-implements the queenbee workflow schema, the Luigi DAG executor, the pollination input handlers, or the native EnergyPlus/OpenStudio/Radiance engines this package already adapts.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `lbt-recipes`
- package: `lbt-recipes`
- import: `import lbt_recipes`
- owner: `geometry` (RUNTIME-tier adapter; runtime owns Luigi execution)
- rail: energy / recipe
- installed: `0.28.4`
- license: AGPL-3.0 (strong copyleft; the recipe rail runs as the out-of-process simulation runner graduating result artifacts across the wire)
- marker: `python_version < '3.15'` — resolves only in the cp<3.15 companion environment; absent from the default cp315 `assay api` distribution set
- entry points: `lbt-recipes` console script; the rail composes the API
- dependency: `queenbee-local` (Luigi DAG executor), `pollination-handlers` (input coercion), `click`; pulls the honeybee-energy/honeybee-radiance model stack transitively
- external engines: shells out to native EnergyPlus, OpenStudio, and Radiance binaries gated at `version.EP_VERSION=(25, 1, 0)`, `OS_VERSION=(3, 10, 0)`, `RADIANCE_DATE=(2023, 11, 5)` — not self-contained; the engines must be installed and version-matched on the runtime host
- capability: 19 packaged queenbee recipe DAGs spanning annual/point-in-time daylight and irradiance, energy use, glare, sky view, cumulative radiation, the PMV/UTCI/adaptive comfort maps, and the LEED/BREEAM/WELL/EN-17037 daylight-credit recipes; typed input/output descriptors with pollination-handler coercion; recipe execution via the queenbee-local Luigi engine; and external-engine version validation

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: recipe binding and execution (`lbt_recipes`)
- rail: energy / recipe

One `Recipe` concept loaded by name; the recipe is the `recipe_name` argument, never a class per workflow. Inputs/outputs are typed descriptors; settings is the execution config.

| [INDEX] | [SYMBOL]                    | [TYPE_FAMILY]   | [CAPABILITY]                                                          |
| :-----: | :-------------------------- | :-------------- | :------------------------------------------------------------------- |
|  [01]   | `recipe.Recipe`             | recipe binding  | load a named queenbee recipe; `inputs`/`outputs`, `handle_inputs`, `run`, `write_inputs_json` |
|  [02]   | `recipe.RecipeInput`        | input descriptor | `name`/`value`/`default_value`/`is_required`/`is_path`/`handlers`; `handle_value` via pollination-handlers |
|  [03]   | `recipe.RecipeOutput`       | output descriptor | `name`/`description`/`handlers`; the result-artifact contract       |
|  [04]   | `settings.RecipeSettings`   | execution config | `folder`/`workers`/`reload_old`/`report_out`/`debug_folder`; `from_string` |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: recipe construction, input handling, and execution (`recipe`)
- rail: energy / recipe

`Recipe(recipe_name)` loads one of the 19 packaged recipes; `input_names`/`output_names` enumerate the typed contract; `handle_inputs`/`input_value_by_name` coerce raw values through the input handlers; `run(settings, ...)` dispatches the Luigi DAG and `output_value_by_name` reads a result after.

| [INDEX] | [SURFACE]                                                                       | [CALL_SHAPE]              | [CAPABILITY]                                          |
| :-----: | :------------------------------------------------------------------------------ | :------------------------ | :--------------------------------------------------- |
|  [01]   | `Recipe(recipe_name)`                                                            | recipe name string        | load a packaged recipe by name (e.g. `'annual_daylight'`) |
|  [02]   | `Recipe.input_names` / `Recipe.inputs` / `Recipe.output_names` / `Recipe.outputs` | property                  | enumerate the typed input/output descriptors          |
|  [03]   | `Recipe.input_value_by_name(name)` / `Recipe.handle_inputs(...)` / `Recipe.write_inputs_json(folder, ...)` | name / values | coerce + persist inputs through the handlers           |
|  [04]   | `Recipe.run(settings=None, radiance_check=False, openstudio_check=False, energyplus_check=False, queenbee_path=None, silent=False, debug_folder=None)` | `RecipeSettings` + engine checks | execute the Luigi DAG (returns the project folder) |
|  [05]   | `Recipe.output_value_by_name(name, ...)` / `Recipe.error_summary` / `Recipe.failure_message` / `Recipe.luigi_execution_summary` | name | read a result; the execution diagnostics             |
|  [06]   | `Recipe.default_project_folder` / `Recipe.simulation_id` / `Recipe.tag` / `Recipe.path` | property                  | the recipe identity and default output location       |

[ENTRYPOINT_SCOPE]: input descriptors, settings, and engine version gate (`recipe`, `settings`, `version`)
- rail: energy / recipe

`RecipeInput.handle_value` runs the pollination handler chain that coerces a honeybee model/HBJSON/`Wea`/`EPW` into the recipe-ready path. `RecipeSettings` is the execution config. The `version` module is the native-engine boundary check.

| [INDEX] | [SURFACE]                                                                       | [CALL_SHAPE]              | [CAPABILITY]                                          |
| :-----: | :------------------------------------------------------------------------------ | :------------------------ | :--------------------------------------------------- |
|  [01]   | `RecipeInput.handle_value()` / `.is_handled` / `.handlers`                       | set `.value` first        | run the pollination-handler coercion chain for one input |
|  [02]   | `RecipeInput.is_path` / `.is_required` / `.default_value` / `.description`       | property                  | the input contract metadata the boundary validates    |
|  [03]   | `RecipeSettings(folder=None, workers=None, reload_old=False, report_out=False, debug_folder=None)` / `RecipeSettings.from_string(s)` | config | recipe execution settings (output folder, parallelism) |
|  [04]   | `version.check_energyplus_version()` / `check_openstudio_version()` / `check_radiance_date()` | none | validate the installed native engine against the gate |
|  [05]   | `version.EP_VERSION` / `OS_VERSION` / `RADIANCE_DATE` / `version.energy_folders` / `version.rad_folders` | constant | the required engine versions and resolved engine paths |

[ENTRYPOINT_SCOPE]: the packaged recipe set (`lbt_recipes/<recipe>`)
- rail: energy / recipe

The 19 named recipes the `Recipe(recipe_name)` loader binds. Each is a queenbee recipe folder with a typed input/output contract; the recipe is selected by name, never a subclass.

| [INDEX] | [RECIPE GROUP]                                                                  | [RECIPE NAMES]            | [CAPABILITY]                                          |
| :-----: | :------------------------------------------------------------------------------ | :------------------------ | :--------------------------------------------------- |
|  [01]   | annual daylight                                                                 | `annual_daylight`, `annual_daylight_enhanced`, `annual_daylight_en17037`, `imageless_annual_glare` | annual daylight metrics (DA/cDA/UDI), glare          |
|  [02]   | daylight credit                                                                 | `leed_daylight_option_one`, `leed_daylight_option_two`, `breeam_daylight_4b`, `well_daylight` | rating-system daylight compliance                    |
|  [03]   | irradiance + radiation                                                          | `annual_irradiance`, `cumulative_radiation`, `direct_sun_hours`, `sky_view` | solar irradiance, sun hours, sky exposure            |
|  [04]   | point-in-time + factor                                                          | `point_in_time_grid`, `point_in_time_view`, `daylight_factor` | single-state grid/view illuminance, daylight factor  |
|  [05]   | energy                                                                          | `annual_energy_use`       | EnergyPlus annual energy simulation                  |
|  [06]   | comfort map                                                                     | `pmv_comfort_map`, `utci_comfort_map`, `adaptive_comfort_map` | per-sensor spatial thermal-comfort maps              |

## [04]-[INTEGRATION_PATTERNS]

[STACK_QUEENBEE_LUIGI]: `Recipe.run` <-> `queenbee-local` Luigi executor
- `Recipe(name)` loads the packaged queenbee recipe (a workflow DAG of containerized functions); `Recipe.run(settings)` translates it to a `queenbee-local` Luigi graph and executes it, with `luigi_execution_summary`/`error_summary`/`failure_message` as the diagnostics. The recipe owner here binds the recipe and validates inputs; the runtime owns the Luigi DAG scheduling and the subprocess fan-out. This is the boundary the RUNTIME tag marks: the `.api` lives in geometry because the model+recipe binding is a geometry concern, but execution is delegated, not owned.

[STACK_POLLINATION_HANDLERS]: `RecipeInput.handle_value` <-> `pollination-handlers`
- Each `RecipeInput` carries a `handlers` chain from `pollination-handlers`; `handle_value(raw)` coerces a live honeybee `Model`/HBJSON, a `ladybug-core` `Wea`/`EPW`, a sensor grid, or a radiance-parameter string into the file path the recipe step consumes. The recipe owner threads raw band objects straight in and lets the handler chain own the coercion — it never hand-writes the model-to-path serialization that `pollination-handlers` already owns. `write_inputs_json` persists the coerced input set for the run.

[STACK_HONEYBEE_MODEL_INPUT]: recipe `model` input <-> honeybee model graph
- Every recipe's `model` input is a honeybee `Model` (built on the `ladybug-geometry` `Face3D`/`Polyface3D` substrate, enriched by `honeybee-energy`/`honeybee-radiance`); a district-scale run originates that `Model` from a `dragonfly` `Model.to_honeybee(object_per_model=...)` explode (urban massing -> per-building honeybee models -> recipe), so the urban band enters the recipe through honeybee, never as a raw dragonfly object. The `wea`/`epw` input is a `ladybug-core` `Wea`/`EPW`; the comfort-map recipes additionally take a `ladybug-comfort` `Parameter`. The recipe binding is therefore the convergence point of the whole energy band — geometry substrate -> climate -> model -> recipe -> simulation — and the input contract (`input_names`) is the discriminator the owner reads to know which band objects a recipe needs.

[STACK_RESULT_READBACK]: recipe outputs <-> `ladybug-core`/`ladybug-comfort` decode
- The recipe writes native result artifacts the band reads back: `annual_energy_use` produces an EnergyPlus `.sql` decoded by `ladybug-core` `SQLiteResult.data_collections_by_output_name` into `DataCollection`s; the daylight recipes produce illuminance matrices and `.metrics`/`.results` outputs (`annual_daylight` outputs `da`/`cda`/`udi`/`metrics`/`results`); the comfort-map recipes produce per-sensor matrices fed to `ladybug-comfort` `map.mrt`/`tcp`. `Recipe.output_value_by_name(name)` is the typed read; the band decoders own turning the artifact into labeled data.

[STACK_ENGINE_VERSION_GATE]: `version` <-> the native-engine boundary
- lbt-recipes is not self-contained: it dispatches to installed native EnergyPlus, OpenStudio, and Radiance binaries, and `version.check_energyplus_version()`/`check_openstudio_version()`/`check_radiance_date()` validate the installed engine against `EP_VERSION=(25, 1, 0)`/`OS_VERSION=(3, 10, 0)`/`RADIANCE_DATE=(2023, 11, 5)` before a run (the `*_check` flags on `Recipe.run` opt the gate in). `version.energy_folders`/`rad_folders` resolve the engine paths. The runtime/provisioning owner is responsible for installing and version-matching these engines on the host; a version mismatch is a typed precondition the owner surfaces before the Luigi DAG starts, not a mid-run subprocess failure.

## [05]-[IMPLEMENTATION_LAW]

[ENERGY_RECIPE]:
- import: `import lbt_recipes` at boundary scope only; module-level import is banned by the manifest import policy, and the companion-env marker means the import resolves only under cp<3.15.
- recipe axis: one `Recipe` concept loaded by `recipe_name` from the 19 packaged queenbee recipes; the recipe is the name argument, never a class per workflow. `input_names`/`output_names`/`inputs`/`outputs` are the typed contract; `Recipe.run(settings, ...)` dispatches the Luigi DAG and returns the project folder.
- input axis: `RecipeInput.handle_value` runs the `pollination-handlers` coercion chain that turns a honeybee `Model`/`Wea`/`EPW`/grid into the recipe-ready path; the owner threads raw band objects in and lets the handler chain own serialization. `is_path`/`is_required`/`default_value` are the contract the boundary validates; `write_inputs_json` persists the coerced set.
- execution axis: `queenbee-local` owns the Luigi DAG scheduling and the containerized-function subprocess fan-out; `RecipeSettings(folder, workers, ...)` is the execution config; `luigi_execution_summary`/`error_summary`/`failure_message` are the diagnostics. This is the RUNTIME boundary — geometry binds, runtime executes.
- engine axis: the native EnergyPlus/OpenStudio/Radiance binaries are external dependencies gated by `version.EP_VERSION`/`OS_VERSION`/`RADIANCE_DATE`; `check_*_version` validates the installed engine and `energy_folders`/`rad_folders` resolve their paths. A version mismatch is a typed precondition surfaced before the run.
- result axis: recipe outputs are native artifacts (EnergyPlus `.sql`, daylight matrices, comfort matrices) read back by `ladybug-core` `SQLiteResult` and `ladybug-comfort` `map`; `output_value_by_name` is the typed read, the band decoders own the labeled-data conversion.
- evidence: each run captures the recipe name, `simulation_id`, the coerced input set, the engine versions validated, the Luigi execution summary, and the output-artifact paths as a recipe receipt.
- boundary: lbt-recipes owns the recipe binding, the typed input/output contract, the pollination-handler coercion dispatch, and the engine version gate. The Luigi DAG execution is owned by `queenbee-local`; the input handlers by `pollination-handlers`; the recipe/workflow schema by `queenbee`; the native simulation engines are external binaries the provisioning owner installs; the model inputs by honeybee; the result decode by `ladybug-core`/`ladybug-comfort`; recipe-execution orchestration and artifact graduation by the Python runtime owner, never re-implemented here.

## [06]-[LOCAL_ADMISSION]

[RAIL_LAW]:
- Package: `lbt-recipes`
- Owns: the 19 packaged queenbee recipe DAGs loaded by name; the `Recipe` binding with its typed `RecipeInput`/`RecipeOutput` contract; the `pollination-handlers` input-coercion dispatch; `RecipeSettings` execution config; and the EnergyPlus/OpenStudio/Radiance version gate
- Accept: the recipe-binding adapter between the honeybee model band and the runtime Luigi executor; the convergence point of geometry-substrate -> climate -> model -> recipe -> simulation; the result artifacts read back by `ladybug-core`/`ladybug-comfort`
- Reject: wrapper-renames of `Recipe`/`RecipeSettings`; a class-per-workflow ladder over the `recipe_name` loader; a hand-rolled queenbee workflow schema, Luigi executor, pollination input handler, or native-engine driver where this package already adapts them; a re-implementation of the result decode (owned by `ladybug-core`/`ladybug-comfort`); in-process recipe execution where the runtime owns the Luigi DAG; identity minting the runtime owns
- Note: AGPL-3.0 copyleft and external native-engine dependency — the recipe rail runs out-of-process, requires version-matched EnergyPlus/OpenStudio/Radiance binaries on the host, and graduates result artifacts across the wire, never linked into a distributed host binary

[CAPTURE_GAP]:
- members: verified by live introspection against an installed `lbt-recipes==0.28.4` companion distribution (the `python_version < '3.15'` marker excludes it from the default cp315 `assay api` set; `assay api resolve --key lbt-recipes` returns `unsupported` under cp315). Every documented type, property, method, and packaged recipe resolves with the signatures shown — no phantom. Confirmed shapes: `Recipe(recipe_name)`; `Recipe.run(settings=None, radiance_check=False, openstudio_check=False, energyplus_check=False, queenbee_path=None, silent=False, debug_folder=None)`; `RecipeInput.handle_value()` (no-arg; the value is set through the `RecipeInput.value` setter before `handle_value()` runs the chain); `RecipeSettings(folder=None, workers=None, reload_old=False, report_out=False, debug_folder=None)`; the 19 packaged recipe folders enumerated (each carrying a `package.json` queenbee contract); `version.EP_VERSION=(25, 1, 0)`, `OS_VERSION=(3, 10, 0)`, `RADIANCE_DATE=(2023, 11, 5)` with `energy_folders`/`rad_folders` engine-path resolvers; `annual_daylight` confirmed inputs (`model`/`north`/`schedule`/`radiance-parameters`/`grid-filter`/...) and outputs (`da`/`cda`/`udi`/`metrics`/`results`/...). License confirmed AGPL-3.0; `queenbee-local`/`pollination-handlers` confirmed as the runtime dependencies.

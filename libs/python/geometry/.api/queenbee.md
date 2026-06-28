# [PY_GEOMETRY_API_QUEENBEE]

`queenbee` is the workflow schema/contract authority for the geometry energy rail: a pure pydantic-v2 model tree describing a `Recipe` (a DAG of plugin functions), its typed input/output families, the `Plugin`/`Function` templates a recipe depends on, and the `Job` that submits parametric argument sets against a recipe. In geometry it is the recipe-adapter contract layer — the energy domain authors and reads a `RecipeInterface` (metadata + typed `inputs`/`outputs`), populates `Job.arguments` from honeybee/ladybug/dragonfly models, and validates the recipe contract — while runtime owns recipe execution (`runtime/.api/queenbee.md`, `runtime/.api/lbt-recipes.md`). Every IO and template family is a pydantic discriminated union keyed on a literal `type` field, so it stacks directly on the `pydantic` substrate catalog (`model_validate`/`model_dump(by_alias=True)` round-trip, `Field(discriminator='type')` decode); the `IOAliasHandler(language, module, function)` on each aliased input names a `pollination_handlers` function (`geometry/.api/pollination-handlers.md`). The package owner composes `Recipe.from_folder`, `BakedRecipe.from_recipe`, `RecipeInterface.from_recipe`, and `Job.validate_arguments` into the energy recipe-contract owner; it never hand-rolls the DAG schema, the typed-IO discriminator, or YAML/JSON round-trips queenbee already owns.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `queenbee`
- package: `queenbee`
- import: `import queenbee`
- owner: `geometry`
- rail: energy / recipe-contract
- installed: `2.0.1`
- license: `MIT`
- abi: pure-Python (`py3-none-any`, purelib; no native extension); `pydantic>=2.0,<3.0` model floor with `pyyaml` + `jsonschema` runtime deps
- companion-gated: admitted `python_version < '3.15'` alongside the Ladybug Tools cluster; absent from the cp315 frozen env (`api resolve --frozen` cannot reach it), so the members below are confirmed against the resolved companion wheel source (`2.0.1`)
- depends: `pydantic (>=2.0,<3.0)` (model base + discriminated unions), `pyyaml (>=6.0)` (recipe folder round-trip), `jsonschema (>=4.17.3)` (DAG input `spec` validation)
- entry points: the `queenbee` schema graph is the admitted surface; queenbee's own click-based CLI (`[cli]` extra) is not admitted (the repo parser is `cyclopts`), and the `queenbee local run` executor is added by `queenbee-local` (runtime's concern)
- capability: pydantic-v2 schema for `Recipe`/`BakedRecipe`/`RecipeInterface`, `Plugin`/`Function` templates with docker/local config, the nine typed DAG/Function/Step IO discriminated unions plus their alias + reference families, `Job` parametric argument submission with status models, recipe/plugin repository packaging, and YAML/JSON folder serialization

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: base model family
- rail: recipe-contract

`BaseModel` adds a content `__hash__` (sha-256 over the model dict) and a `type` discriminant on top of `BaseModelNoType`; both expose the YAML/JSON serialization surface. Every queenbee model subclasses one of them, so the whole tree is pydantic-v2.

| [INDEX] | [SYMBOL]                  | [TYPE_FAMILY]  | [CAPABILITY]                                                    |
| :-----: | :------------------------ | :------------- | :------------------------------------------------------------- |
|  [01]   | `base.basemodel.BaseModel`        | pydantic base  | typed model with content `__hash__`, `to_dict`/`to_json`/`to_yaml` |
|  [02]   | `base.basemodel.BaseModelNoType`  | pydantic base  | extension base without an enforced `type` discriminant         |
|  [03]   | `base.metadata.MetaData`          | metadata model | recipe/plugin metadata: `name`/`tag`/`keywords`/`maintainers`/`license` |
|  [04]   | `base.metadata.Maintainer` / `base.metadata.License` | metadata model | maintainer and license sub-records |

[PUBLIC_TYPE_SCOPE]: workflow template family
- rail: recipe-contract

`Recipe.flow` is a list of `DAG`s; `BakedRecipe` is the fully-resolved compile where every dependency `Plugin`/`Recipe` is inlined into `templates` under a content `digest`; `RecipeInterface` is the lightweight metadata + IO projection for UI.

| [INDEX] | [SYMBOL]                            | [TYPE_FAMILY]   | [CAPABILITY]                                          |
| :-----: | :---------------------------------- | :-------------- | :--------------------------------------------------- |
|  [01]   | `recipe.recipe.Recipe`              | recipe          | `api_version`/`metadata`/`dependencies`/`flow: List[DAG]` |
|  [02]   | `recipe.recipe.BakedRecipe`         | recipe          | `Recipe` + `digest` + inlined `templates: List[TemplateFunction \| DAG]` |
|  [03]   | `recipe.recipe.RecipeInterface`     | recipe view     | `metadata`/`source`/`inputs: List[DAGInputs]`/`outputs: List[DAGOutputs]` |
|  [04]   | `recipe.recipe.TemplateFunction`    | template        | a `Function` carrying its `PluginConfig` (`from_plugin`) |
|  [05]   | `recipe.dag.DAG`                    | dag             | `name`/`inputs`/`tasks: List[DAGTask]`/`outputs`     |
|  [06]   | `recipe.task.DAGTask` / `recipe.task.DAGTaskLoop` | dag task | a flow node: `template`/`needs`/`arguments`/`loop`/`returns` |
|  [07]   | `recipe.dependency.Dependency` / `recipe.dependency.DependencyKind` | dependency | a recipe/plugin dependency reference with `fetch` |
|  [08]   | `plugin.plugin.Plugin`              | plugin          | `metadata`/`config: PluginConfig`/`functions: List[Function]` |
|  [09]   | `plugin.function.Function`          | function        | `command`/`source`/`language`/typed `inputs`/`outputs` |
|  [10]   | `plugin.plugin.PluginConfig` / `plugin.plugin.DockerConfig` / `plugin.plugin.LocalConfig` | plugin config | docker (`image`/`registry`/`workdir`) vs local execution context |

[PUBLIC_TYPE_SCOPE]: typed IO discriminated families
- rail: recipe-contract schema

Each family is a `Union[...]` discriminated on the literal `type` field; `IOBase` is the inputs/outputs base for `Function`/`DAG` and owns the artifact-vs-parameter partition and unique-name validation. `IOAliasHandler` is the Grasshopper alias handler reference resolved by the executor.

| [INDEX] | [SYMBOL]                       | [TYPE_FAMILY]        | [CAPABILITY]                                              |
| :-----: | :----------------------------- | :------------------- | :------------------------------------------------------- |
|  [01]   | `io.common.IOBase`             | io base              | `inputs`/`outputs` lists + artifact/parameter accessors  |
|  [02]   | `io.common.ItemType`           | enum                 | list item type: `Generic`/`String`/`Integer`/`Number`/`Boolean`/`Array`/`JSONObject` |
|  [03]   | `io.common.GenericInput` / `io.common.GenericOutput` / `io.common.PathOutput` / `io.common.FromOutput` | io base | IO roots with `is_artifact`/`is_parameter`/`referenced_values` |
|  [04]   | `io.inputs.dag.DAGInputs`      | discriminated union  | 10-member typed recipe inputs (`DAGStringInput` … `DAGJSONObjectInput`, `DAGGenericInput`) |
|  [05]   | `io.outputs.dag.DAGOutputs`    | discriminated union  | 10-member typed recipe outputs (`DAGFileOutput`/`DAGFolderOutput`/`DAGPathOutput` are artifacts) |
|  [06]   | `io.inputs.function.FunctionInputs` / `io.outputs.function.FunctionOutputs` | discriminated union | 9-member typed function IO |
|  [07]   | `io.inputs.step.StepInputs` / `io.outputs.step.StepOutputs` | discriminated union | resolved per-step IO values |
|  [08]   | `io.inputs.task.TaskArguments` / `io.outputs.task.TaskReturns` | discriminated union | `TaskArgument`/`TaskPathArgument`; `TaskReturn`/`TaskPathReturn` |
|  [09]   | `io.inputs.alias.DAGAliasInputs` / `io.outputs.alias.DAGAliasOutputs` | discriminated union | platform-aliased IO carrying `IOAliasHandler`s |
|  [10]   | `io.common.IOAliasHandler`     | handler reference    | `language`/`module`/`function`/`index` — names the `pollination_handlers` function |

[PUBLIC_TYPE_SCOPE]: reference, artifact-source, job, and repository family
- rail: recipe-contract

References resolve `{{...}}` template variables; artifact sources locate file inputs; `Job` submits parametric `arguments` (a list-of-lists for the parametric sweep) against a recipe `source`.

| [INDEX] | [SYMBOL]                              | [TYPE_FAMILY] | [CAPABILITY]                                          |
| :-----: | :------------------------------------ | :------------ | :--------------------------------------------------- |
|  [01]   | `io.reference` family (`InputReference`/`TaskReference`/`ItemReference`/`ValueReference`/`FileReference`/`FolderReference`) | reference | typed `{{...}}` template-variable resolution between tasks |
|  [02]   | `io.artifact_source.HTTP` / `io.artifact_source.S3` / `io.artifact_source.ProjectFolder` | artifact source | where a `JobPathArgument`/path input is fetched from |
|  [03]   | `job.job.Job`                         | job           | `source`/`arguments: List[List[JobArguments]]`/`name`/`labels` |
|  [04]   | `io.inputs.job.JobArgument` / `io.inputs.job.JobPathArgument` (`JobArguments`) | job argument | `name`+`value` parameter, or `name`+`source` artifact |
|  [05]   | `job.job.JobStatus` / `job.job.JobStatusEnum`  | status        | job lifecycle: `Created`/`Pre-Processing`/`Running`/`Failed`/`Cancelled`/`Completed`/`Unknown` |
|  [06]   | `job.run.RunStatus` / `job.run.StepStatus` / `job.result.Results` | status | per-run / per-step status and aggregate `Results.from_runs` |
|  [07]   | `repository.index.RepositoryIndex` / `repository.package.PackageVersion` | registry | recipe/plugin package index and versioned manifest |
|  [08]   | `config.Config` / `config.repositories.RepositoryReference` / `config.auth` (`BaseAuth`/`HeaderAuth`/`JWTAuth`) | config | registry endpoints + auth headers for `Dependency.fetch` |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: model serialization (every `BaseModel`)
- rail: recipe-contract

`to_dict`/`to_json`/`to_yaml` wrap pydantic-v2 `model_dump`/`model_dump_json` with `by_alias=True`; round-tripping uses `model_validate`. The energy owner converts at the boundary and never re-parses.

| [INDEX] | [SURFACE]                                                                 | [ENTRY_FAMILY] | [CAPABILITY]                              |
| :-----: | :------------------------------------------------------------------------ | :------------- | :---------------------------------------- |
|  [01]   | `BaseModel.from_file(filepath)`                                          | load           | parse a JSON or YAML file (by extension) and `model_validate` |
|  [02]   | `BaseModel.to_dict(exclude_defaults=False, by_alias=True, exclude_none=False)` | serialize | model -> dict (alias keys)                |
|  [03]   | `BaseModel.to_json(filepath, indent=None)` / `BaseModel.to_yaml(filepath, ...)` / `BaseModel.yaml(...)` | serialize | write JSON / write YAML / YAML string     |
|  [04]   | `BaseModel.model_validate(obj)` / `BaseModel.model_dump(by_alias=True)`   | pydantic       | decode/encode through the pydantic substrate |
|  [05]   | `BaseModel.__hash__`                                                      | identity       | content sha-256 digest used by the bake   |

[ENTRYPOINT_SCOPE]: recipe authoring and baking
- rail: recipe-contract

`from_folder` reads a `package.yaml`/`dependencies.yaml`/`flow/` recipe tree; `BakedRecipe.from_recipe` fetches every dependency, inlines its templates, and replaces template refs into a self-contained digest-keyed recipe.

| [INDEX] | [SURFACE]                                                                | [ENTRY_FAMILY] | [CAPABILITY]                                  |
| :-----: | :----------------------------------------------------------------------- | :------------- | :-------------------------------------------- |
|  [01]   | `Recipe.from_folder(folder_path) -> Recipe`                              | factory        | load a recipe folder into the model           |
|  [02]   | `Recipe.to_folder(folder_path, readme_string=None)` / `Recipe.write_dependencies(folder_path, config=Config())` | serialize | write the recipe + fetched dependency tree |
|  [03]   | `Recipe.root_dag` / `Recipe.inputs -> List[DAGInputs]` / `Recipe.outputs -> List[DAGOutputs]` | accessor | entrypoint DAG and its typed IO |
|  [04]   | `Recipe.lock_dependencies(config=Config())` / `Recipe.is_locked`        | dependency     | pin dependency digests / report locked state  |
|  [05]   | `BakedRecipe.from_recipe(recipe, config=Config()) -> BakedRecipe`       | bake           | resolve + inline all dependency templates     |
|  [06]   | `BakedRecipe.from_folder(folder_path, refresh_deps=True, config=Config())` / `BakedRecipe.template_by_name(...)` | bake | bake from folder; resolve a template by name |
|  [07]   | `RecipeInterface.from_recipe(recipe, source=None) -> RecipeInterface`   | interface      | project metadata + IO for a UI/contract       |

[ENTRYPOINT_SCOPE]: IO partition, job, and dependency operations
- rail: recipe-contract

`IOBase` partitions inputs/outputs into artifacts (file/folder/path) and parameters; `Job.validate_arguments` checks a submitted argument set against the recipe's `DAGInputs`.

| [INDEX] | [SURFACE]                                                                  | [ENTRY_FAMILY] | [CAPABILITY]                                |
| :-----: | :------------------------------------------------------------------------- | :------------- | :------------------------------------------ |
|  [01]   | `IOBase.artifact_inputs` / `IOBase.parameter_inputs` / `IOBase.artifacts` / `IOBase.parameters` | accessor | partition IO into artifacts vs parameters |
|  [02]   | `IOBase.artifact_input_by_name(name)` / `IOBase.parameter_input_by_name(name)` (+ output variants) | query | one IO item by name |
|  [03]   | `Job.populate_default_arguments(inputs)` / `Job.validate_arguments(inputs)` | job | fill defaults / validate args against `DAGInputs` |
|  [04]   | `Dependency.fetch(verify_digest=True, auth_header={}) -> PackageVersion`   | dependency     | fetch a dependency manifest from a registry  |
|  [05]   | `DAG.get_task(name)` / `DAG.templates` / `DAG.find_task_return(...)`        | dag            | task lookup, referenced-template set, return resolution |
|  [06]   | `Plugin.from_folder(folder_path)` / `Plugin.to_folder(folder_path)` / `TemplateFunction.from_plugin(plugin)` | plugin | plugin folder round-trip; functions -> templates |
|  [07]   | `Function.is_script` / `Results.from_runs(runs) -> Results`                 | accessor       | script-vs-command function; aggregate run results |

## [04]-[IMPLEMENTATION_LAW]

[SCHEMA_TOPOLOGY]:
- discriminant law: every IO and template family (`DAGInputs`, `DAGOutputs`, `FunctionInputs`/`FunctionOutputs`, `StepInputs`/`StepOutputs`, `TaskArguments`/`TaskReturns`, `JobArguments`, `DAGAliasInputs`/`DAGAliasOutputs`) is a `Union[...]` discriminated on a literal `type` string; decode through `model_validate` / `Field(discriminator='type')`, never an `isinstance` ladder.
- api-version law: `Recipe`/`Plugin`/`Job`/`RecipeInterface` carry `api_version='v1beta1'` (read-only); a schema bump is an additive version, never a field mutation in place.
- bake law: `BakedRecipe.from_recipe` is the compile step — it `model_copy(deep=True)`s the recipe, fetches each `Dependency`, inlines plugin functions as `TemplateFunction` and sub-recipe DAGs into `templates`, replaces template refs, and seals the result under a content `digest`. Author against `Recipe`/`RecipeInterface`; submit against the `BakedRecipe`.
- artifact law: `is_artifact` (file/folder/path) vs `is_parameter` is the IO partition; `Job.arguments` is `List[List[JobArguments]]` because a job is a parametric sweep — the outer list is the parameter axis.

[STACK_LAW]:
- `pydantic` substrate: every model is pydantic-v2; `model_validate(payload)` decodes a recipe/job dict (e.g. from an `httpx` `Response.json()` over the runtime transport) straight into the discriminated tree, and `model_dump(by_alias=True)` re-emits it — one rail, no intermediate dict re-parse.
- `jsonschema`: `DAGGenericInput.spec` is validated by `validate_spec`; the energy domain attaches a JSON-schema `spec` to a recipe input and queenbee enforces it, rather than a hand-rolled validator.
- `pollination_handlers`: an aliased input/output carries `IOAliasHandler(language='python', module='pollination_handlers.inputs.model', function='model_to_json')`; geometry authors this reference, the function body lives in `pollination-handlers`, and the executor (`lbt-recipes`/`queenbee-local`) resolves it via `importlib` at run time.
- domain models: `Job.arguments` values are minted from honeybee/ladybug/dragonfly domain objects (`geometry/.api/honeybee-energy.md`, `ladybug-core.md`, `dragonfly-energy.md`) after the input handler coerces them to paths/scalars.

## [05]-[LOCAL_ADMISSION]

[RAIL_LAW]:
- Package: `queenbee`
- Owns: the pydantic-v2 schema for recipes, plugins, functions, DAGs, jobs, typed IO discriminated families, references, artifact sources, run/job status, and recipe/plugin repository packaging
- Accept: `Recipe`/`BakedRecipe`/`RecipeInterface` authoring via `from_folder`/`from_recipe`, `model_validate`/`model_dump(by_alias=True)` round-trip, `Field(discriminator='type')` decode of every IO union, `IOBase` artifact/parameter accessors, `Job.validate_arguments` against `DAGInputs`, `IOAliasHandler` references to `pollination_handlers`, JSON-schema `spec` validation
- Reject: hand-rolled DAG/recipe schemas or YAML/JSON serializers, `isinstance` ladders over the IO unions where the `type` discriminant decodes, re-parsing `model_dump` output, mutating `api_version` in place, a parallel job-argument or status model, queenbee's click `[cli]` extra (the repo parser is `cyclopts`)

[CAPTURE_GAP]:
- execution: the `queenbee local run` CLI is added by `queenbee-local` (the luigi executor), not by `queenbee` itself; recipe execution is the runtime owner's concern (`runtime/.api/lbt-recipes.md`, `runtime/.api/queenbee.md`). This catalog is the schema/contract surface only.

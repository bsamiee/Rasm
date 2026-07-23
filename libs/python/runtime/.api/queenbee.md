# [PY_RUNTIME_API_QUEENBEE]

`queenbee` mints the Pollination workflow-language schema graph the runtime recipe owner composes: every node is a `type`-discriminated `pydantic` v2 `BaseModel` carrying a free-form `annotations` dict, and every model round-trips disk YAML, a validated graph, and a wire dict through one serialization spine. It is the schema authority alone — the recipe/workflow vocabulary is admitted, its bundled click CLI and `urllib` transfer/repository-fetch helpers rejected, so `cyclopts` owns the parser and the `httpx`/`obstore` rails own byte movement.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `queenbee`
- package: `queenbee` (MIT)
- module: `queenbee`
- namespaces: `queenbee.base`, `queenbee.io` (`inputs`/`outputs`/`reference`/`artifact_source`/`common`), `queenbee.plugin`, `queenbee.recipe`, `queenbee.job`, `queenbee.repository`, `queenbee.config`
- abi: pure-Python (`py3-none-any`, no native extension)
- rail: recipe (workflow schema contract)

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: base model + metadata (`queenbee.base`)

| [INDEX] | [SYMBOL]          | [TYPE_FAMILY] | [CAPABILITY]                                     |
| :-----: | :---------------- | :------------ | :----------------------------------------------- |
|  [01]   | `BaseModel`       | base model    | discriminated base; owns the serialization spine |
|  [02]   | `BaseModelNoType` | base model    | type-free extension base (no `type` enforcement) |
|  [03]   | `MetaData`        | metadata      | package/recipe descriptive metadata              |
|  [04]   | `Maintainer`      | metadata      | `name`/`email`                                   |
|  [05]   | `License`         | metadata      | `name`/`url`                                     |

[PUBLIC_TYPE_SCOPE]: plugin + function operation templates (`queenbee.plugin`)

| [INDEX] | [SYMBOL]             | [TYPE_FAMILY]      | [CAPABILITY]                                       |
| :-----: | :------------------- | :----------------- | :------------------------------------------------- |
|  [01]   | `Plugin`             | operation owner    | plugin manifest: config + function set             |
|  [02]   | `Function`           | operation (IOBase) | one containerized step template                    |
|  [03]   | `PluginConfig`       | config             | `docker: DockerConfig`/`local: LocalConfig`        |
|  [04]   | `DockerConfig`       | config             | `image`/`registry`/`workdir` — the execution image |
|  [05]   | `LocalConfig`        | config             | local (no-container) execution marker              |
|  [06]   | `ScriptingLanguages` | enum               | function source language (`python`)                |

[PUBLIC_TYPE_SCOPE]: recipe + DAG flow (`queenbee.recipe`)

| [INDEX] | [SYMBOL]                        | [TYPE_FAMILY]  | [CAPABILITY]                                                   |
| :-----: | :------------------------------ | :------------- | :------------------------------------------------------------- |
|  [01]   | `Recipe`                        | flow owner     | the authored, unresolved recipe                                |
|  [02]   | `BakedRecipe`                   | flow owner     | dependencies inlined + `digest` locked — the runnable artifact |
|  [03]   | `RecipeInterface`               | flow interface | lean inputs/outputs projection for job-form discovery          |
|  [04]   | `DAG`                           | flow (IOBase)  | one dependency graph                                           |
|  [05]   | `DAGTask`                       | flow node      | one task invocation                                            |
|  [06]   | `DAGTaskLoop`                   | flow control   | `from_` — fan-out item source for a looped task                |
|  [07]   | `TemplateFunction`              | flow template  | a `Function` baked into the template set                       |
|  [08]   | `Dependency`                    | dependency     | a recipe/plugin reference                                      |
|  [09]   | `DependencyKind`                | enum           | `recipe` / `plugin`                                            |
|  [10]   | `TaskReturn` / `TaskPathReturn` | flow output    | declared parameter / path return                               |

[PUBLIC_TYPE_SCOPE]: typed IO input/output algebra (`queenbee.io.inputs` / `queenbee.io.outputs`)
- Every IO node is `{DAG|Function|Step|Alias}<Type>{Input|Output}` over `ItemType` — the tier sets the binding stage, the type sets the payload; construct by composing tier + type, the bases below anchoring the family.

| [INDEX] | [SYMBOL]                                          | [TYPE_FAMILY]   | [CAPABILITY]                                                       |
| :-----: | :------------------------------------------------ | :-------------- | :----------------------------------------------------------------- |
|  [01]   | `DAGGenericInput` (+`DAG<Type>Input`)             | dag input       | recipe-level declared input; `default`/`alias`/`required`/`spec`   |
|  [02]   | `Function<Type>Input`                             | function input  | function-level input; `path` on artifact inputs                    |
|  [03]   | `Step<Type>Input`                                 | step input      | runtime-bound input; `value` / `source`                            |
|  [04]   | `DAG<Type>InputAlias`                             | input alias     | UI/handler-aliased input; `platform`/`handler`                     |
|  [05]   | `DAG<Type>Output`                                 | dag output      | recipe-level output from a task return; `from_`/`alias`/`required` |
|  [06]   | `Function<Type>Output`                            | function output | function-level produced output; `path`/`required` (`PathOutput`)   |
|  [07]   | `Step<Type>Output`                                | step output     | runtime-resolved output; `value` / `source`                        |
|  [08]   | `DAG<Type>OutputAlias`                            | output alias    | UI/handler-aliased output; `platform`/`handler`                    |
|  [09]   | `Generic{Input,Output}`/`PathOutput`/`FromOutput` | io.common head  | the matrix derivation base                                         |
|  [10]   | `IOAliasHandler`                                  | alias handler   | alias post-processor; `language`/`module`/`function`/`index`       |
|  [11]   | `ItemType`                                        | enum            | the value-type axis: `String`…`JSONObject`                         |

[PUBLIC_TYPE_SCOPE]: references, artifact sources, arguments (`queenbee.io.reference` / `io.artifact_source` / `io.inputs`)
- Each `${{...}}` template-reference variant resolves a value at bake/run time; the family is a `type`-discriminated union.

| [INDEX] | [SYMBOL]                                               | [TYPE_FAMILY]   | [CAPABILITY]                                                |
| :-----: | :----------------------------------------------------- | :-------------- | :---------------------------------------------------------- |
|  [01]   | `InputReference` (+`Input{File,Folder,Path}Reference`) | reference       | `${{inputs.<variable>}}` — recipe/function input ref        |
|  [02]   | `TaskReference` (+`Task{File,Folder,Path}Reference`)   | reference       | `${{tasks.<name>.outputs.<variable>}}` — task output ref    |
|  [03]   | `ItemReference`                                        | reference       | `${{item...}}` — loop-item ref                              |
|  [04]   | `ValueReference` (+`Value{List,File,Folder}Reference`) | reference       | inline literal value / list / file-path value ref           |
|  [05]   | `FileReference` / `FolderReference`                    | reference       | `io.common` artifact path references                        |
|  [06]   | `ProjectFolder` / `HTTP` / `S3`                        | artifact source | discriminated `_ArtifactSource` union                       |
|  [07]   | `JobArgument` / `JobPathArgument`                      | argument        | job parameter (`name`/`value`) / artifact (`name`/`source`) |
|  [08]   | `TaskArgument` / `TaskPathArgument`                    | argument        | task parameter (`name`/`from_`) / artifact (+`sub_path`)    |

[PUBLIC_TYPE_SCOPE]: job submission + run lifecycle (`queenbee.job`)

| [INDEX] | [SYMBOL]         | [TYPE_FAMILY]        | [CAPABILITY]                                                                                 |
| :-----: | :--------------- | :------------------- | :------------------------------------------------------------------------------------------- |
|  [01]   | `Job`            | submission           | job submission: source + per-run argument lists                                              |
|  [02]   | `JobStatus`      | receipt              | job-level status + per-run counts                                                            |
|  [03]   | `RunStatus`      | receipt (BaseStatus) | `id`/`job_id`/`entrypoint`/`status: RunStatusEnum`/`steps`/timestamps                        |
|  [04]   | `StepStatus`     | receipt (BaseStatus) | step execution status + DAG position                                                         |
|  [05]   | `BaseStatus`     | receipt base         | `inputs`/`outputs`/`message`/`started_at`/`finished_at`/`source`                             |
|  [06]   | `Results`        | typed list           | `list`-subclass of run results with `from_runs(...)`                                         |
|  [07]   | `JobStatusEnum`  | enum                 | `Created`/`Pre-Processing`/`Running`/`Failed`/`Cancelled`/`Completed`/`Unknown`              |
|  [08]   | `RunStatusEnum`  | enum                 | `Created`/`Scheduled`/`Running`/`Post-Processing`/`Failed`/`Cancelled`/`Succeeded`/`Unknown` |
|  [09]   | `StepStatusEnum` | enum                 | `Scheduled`/`Running`/`Failed`/`Succeeded`/`Skipped`/`Unknown`                               |
|  [10]   | `StatusType`     | enum                 | `Function`/`DAG`/`Loop`/`Container`/`Unknown` — the step-kind discriminant                   |

[PUBLIC_TYPE_SCOPE]: package repository + client config (`queenbee.repository` / `queenbee.config`)

| [INDEX] | [SYMBOL]                              | [TYPE_FAMILY]       | [CAPABILITY]                                                     |
| :-----: | :------------------------------------ | :------------------ | :--------------------------------------------------------------- |
|  [01]   | `RepositoryIndex`                     | registry            | the package-index manifest                                       |
|  [02]   | `RepositoryMetadata`                  | registry            | `name`/`description`/`source`/`plugin_count`/`recipe_count`      |
|  [03]   | `PackageVersion`                      | registry (MetaData) | one published plugin/recipe version                              |
|  [04]   | `Config`                              | client config       | `auth: List[BaseAuth]`/`repositories: List[RepositoryReference]` |
|  [05]   | `BaseAuth` / `JWTAuth` / `HeaderAuth` | client auth         | repository credentials `domain`/`access_token`                   |
|  [06]   | `RepositoryReference`                 | client config       | `name`/`path` — a named repository endpoint                      |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: serialization spine; surfaces are `BaseModel.*`
- One round-trip surface: `from_file`/`to_*` wrap pydantic v2 `model_validate`/`model_dump` with the queenbee defaults `by_alias=True` and `exclude_none` on writes.

| [INDEX] | [SURFACE]                                                | [SHAPE]  | [CAPABILITY]                                                     |
| :-----: | :------------------------------------------------------- | :------- | :--------------------------------------------------------------- |
|  [01]   | `from_file(filepath) -> Self`                            | factory  | load: parse JSON/YAML by ext; raises `ValueError` on failure     |
|  [02]   | `model_validate(obj)` / `model_validate_json(json_data)` | factory  | parse: pydantic v2 validation from a dict / JSON bytes           |
|  [03]   | `to_dict(*, exclude_defaults, by_alias, exclude_none)`   | instance | encode: model → wire dict (the cross-rail projection)            |
|  [04]   | `to_json(filepath, ...)` / `to_yaml(filepath, ...)`      | instance | write: persist as JSON / YAML file                               |
|  [05]   | `yaml(...)` / `model_dump_json(...)`                     | instance | encode: YAML / JSON string in memory                             |
|  [06]   | `model_json_schema()`                                    | factory  | schema: pydantic v2 JSON-Schema generator (contract publication) |

[ENTRYPOINT_SCOPE]: recipe authoring, baking, and folder IO (`Recipe`/`BakedRecipe`/`RecipeInterface`/`Plugin`)

| [INDEX] | [SURFACE]                                                | [SHAPE]  | [CAPABILITY]                                                |
| :-----: | :------------------------------------------------------- | :------- | :---------------------------------------------------------- |
|  [01]   | `Recipe.from_folder(...)` / `Recipe.to_folder(...)`      | factory  | folder IO: load/write a multi-file recipe folder            |
|  [02]   | `Recipe.lock_dependencies(config=Config())`              | instance | resolve: pin dependency digests against repositories        |
|  [03]   | `Recipe.write_dependencies` / `write_dependency_file`    | instance | resolve: write the resolved lock file(s) to the folder      |
|  [04]   | `BakedRecipe.from_recipe` / `from_folder`                | factory  | bake: inline all dependencies into a runnable `BakedRecipe` |
|  [05]   | `BakedRecipe.template_by_name(...)` / `dag_by_name(...)` | static   | navigate: resolve a template/DAG by name                    |
|  [06]   | `BakedRecipe.dependency_by_name(...)` / `check_inputs()` | static   | navigate: resolve a dependency by name; validate inputs     |
|  [07]   | `RecipeInterface.from_recipe(recipe, source=None)`       | factory  | project: the lean inputs/outputs interface for a job form   |
|  [08]   | `Plugin.from_folder(...)` / `Plugin.to_folder(...)`      | factory  | folder IO: load/write a plugin folder                       |

[ENTRYPOINT_SCOPE]: DAG/function navigation, job binding, repository index (`DAG`/`Function`/`Job`/`RepositoryIndex`)
- `IOBase` (shared base of `Function`/`DAG`/`BaseStatus`) owns the polymorphic IO accessors — one discriminating lookup, never a `get`/`getMany`/`getBy` family.

| [INDEX] | [SURFACE]                                                    | [SHAPE]  | [CAPABILITY]                                                   |
| :-----: | :----------------------------------------------------------- | :------- | :------------------------------------------------------------- |
|  [01]   | `IOBase.{artifact,parameter}_{input,output}_by_name(name)`   | instance | navigate: IO lookup on `Function`/`DAG`/`BaseStatus`           |
|  [02]   | `DAG.get_task` / `find_task_return` / `check_dag_references` | instance | navigate: task lookup, return resolution, reference validation |
|  [03]   | `Function.check_either_source_or_command()`                  | instance | validate: enforce the source-xor-command invariant             |
|  [04]   | `Job.validate_arguments` / `populate_default_arguments`      | instance | bind: validate/fill job arguments against the recipe inputs    |
|  [05]   | `RepositoryIndex.from_folder(...)` / `merge_folder(...)`     | factory  | registry build: build/extend an index from packaged resources  |
|  [06]   | `RepositoryIndex.index_{plugin,recipe}_version(...)`         | instance | registry build: add one packaged plugin/recipe version         |
|  [07]   | `RepositoryIndex.package_by_{tag,digest}(...)`               | instance | registry query: resolve a `PackageVersion` by tag or digest    |
|  [08]   | `RepositoryIndex.search(...)` / `get_latest(versions)`       | static   | registry query: search the index or pick the latest version    |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- discriminator law: every node is a `type`-discriminated `BaseModel`; the `type` literal (pinned by `ensure_type_match`) tags the IO/reference/artifact-source unions, so boundary decode resolves the variant from `type`, never an `isinstance` ladder. `annotations` is the free-form extension carrier — never widen a model with fields it can hold.
- single-spine law: a recipe/plugin/job round-trips through one surface — `from_file`/`model_validate(_json)` in, `to_dict`/`to_json`/`to_yaml` out — under the `by_alias=True`/`exclude_none` wire defaults; no second parser, no hand-rolled YAML walk.
- bake law: an authored `Recipe` (digest-free, unresolved `Dependency` references) is distinct from a `BakedRecipe` (dependencies inlined, `digest` locked, `templates` populated); execution consumes a `BakedRecipe`, `BakedRecipe.from_recipe`/`from_folder` the sole bake path and `RecipeInterface` the lean form-discovery projection.
- IO-matrix law: an input/output is selected by tier (`DAG`/`Function`/`Step`/`Alias`) × `ItemType`, never a bespoke per-recipe class; `Step*` carries the runtime `value`/`source`, `*Alias` the `IOAliasHandler` post-processor.

[STACKING]:
- `pydantic`(`libs/python/.api/pydantic.md`): external recipe/plugin YAML/JSON decodes through `Recipe.from_file`/`BakedRecipe.from_recipe` into a validated `type`-discriminated graph; a `pydantic.ValidationError` (wrapped `ValueError` by `from_file`) folds into the runtime `Result`/boundary-fault rail, never raised through the daemon.
- `obstore`(`libs/python/.api/obstore.md`) / `httpx`(`httpx.md`): a `Job` `S3`/`HTTP` artifact source's `(bucket, key, endpoint)`/`url` feeds an `obstore` `get` or an `httpx` stream; a `ProjectFolder` source feeds the `universal-pathlib`(`libs/python/.api/universal-pathlib.md`)/`fsspec` local root — queenbee declares the source, the runtime moves the bytes.
- `stamina`(`stamina.md`): a remote `RepositoryIndex`/`PackageVersion` fetches over the `httpx` async client under a `stamina` retry policy, decodes by `RepositoryIndex.model_validate_json(resp.content)`, then `search`/`get_latest`/`package_by_tag` resolve the version.
- `pollination-handlers`(`pollination-handlers.md`) / `lbt-recipes`(`lbt-recipes.md`): a `DAG<Type>InputAlias.handler` (`IOAliasHandler`) resolves to a `pollination-handlers` callable, and an `lbt-recipes` simulation recipe loads as a `BakedRecipe` and submits as a `Job` — the three-package cluster meets on the queenbee schema.
- within-lib telemetry: synchronous queenbee calls (`from_folder`, repository IO) cross `anyio.to_thread.run_sync` off the async lane; a submission/poll opens one OpenTelemetry span + `structlog` event keyed by `Job.name` + `RunStatus.id` + `BakedRecipe.digest`, `JobStatusEnum`/`RunStatusEnum`/`StepStatusEnum` map to span status, and `Results.from_runs` collects the typed set.
- within-lib identity: a runtime cache key rides the `evidence/identity` `ContentIdentity` owner over `recipe.to_json` bytes — distinct from queenbee's own `digest` lock identity, which keys repository resolution.

[LOCAL_ADMISSION]:
- recipe owner admits the queenbee schema graph alone; the `[cli]` click extra is rejected — `cyclopts` owns the CLI, recipe/job commands composing `Recipe.from_file`/`Job.validate_arguments`.
- queenbee's bundled transfer/fetch helpers (`base.request.make_request`/`get_uri`/`resolve_local_source`, the `repository`/`config` urllib fetches) stay unused: repository index and package fetch route through the runtime `httpx` client, and `S3`/`HTTP`/`ProjectFolder` movement through the `transport/roots` transfer rails.
- queenbee models stay at the recipe-definition boundary; crossing the C#↔Python companion wire projects through `to_dict(by_alias=True)`, never a second `msgspec`/protobuf mint of a queenbee schema.

[RAIL_LAW]:
- Package: `queenbee`
- Owns: the Pollination workflow-language schema graph — plugin/function templates, recipe/baked-recipe/DAG flow with dependency resolution, the tier×type IO algebra and `${{...}}` reference family, job submission + run/step lifecycle receipts, and the package-repository index, all as `type`-discriminated pydantic v2 models over one JSON/YAML spine
- Accept: `Recipe`/`BakedRecipe`/`Plugin`/`Job` construction via `from_file`/`from_folder`/`from_recipe`, `model_validate(_json)` at the boundary, `to_dict`/`to_json`/`to_yaml` projection, the tier×type IO matrix, `IOBase` polymorphic accessors, `RepositoryIndex` query methods, `BakedRecipe.from_recipe` as the sole bake path
- Reject: the queenbee `[cli]` click extra, its urllib `make_request`/`get_uri` transfer + repository fetch, re-minting a queenbee schema as a parallel `msgspec`/protobuf struct, hand-rolled YAML/JSON walks bypassing the spine, bespoke per-recipe IO classes outside the tier×type matrix, and executing an unbaked `Recipe`

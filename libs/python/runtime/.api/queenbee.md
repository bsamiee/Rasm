# [PY_RUNTIME_API_QUEENBEE]

`queenbee` is the recipe/workflow schema-contract vocabulary the runtime recipe owner composes: a `pydantic` v2 object graph for the Pollination workflow language whose every node is a `type`-discriminated `BaseModel` carrying a free-form `annotations` dict. It mints the full computational-DAG schema — `Plugin`/`Function` (containerized operation templates), `Recipe`/`BakedRecipe`/`DAG`/`DAGTask` (the dependency-resolved flow), the typed input/output IO algebra (`{DAG,Function,Step,Alias}` × `{String,Integer,Number,Boolean,Folder,File,Path,Array,JSONObject}`), the `${{...}}` template-reference family (`InputReference`/`TaskReference`/`ItemReference`/`ValueReference`), `Job`/`JobArgument` submission shapes with `JobStatus`/`RunStatus`/`StepStatus` lifecycle receipts, and the `RepositoryIndex`/`PackageVersion` package-registry models. Every model shares one serialization spine — `from_file` (JSON or YAML by extension), `to_dict`/`to_json`/`to_yaml`/`yaml`, and the native `pydantic` `model_validate`/`model_dump` rails — so a recipe round-trips between disk YAML, a validated graph, and a wire dict through one surface. It is the schema authority only: the recipe/workflow vocabulary is admitted, never queenbee's own click-based CLI (the `[cli]` extra is excluded — `cyclopts` is the sole admitted parser) and never its bundled `urllib` transfer/repository-fetch helpers (the runtime `httpx`/`obstore` rails own transfer).

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `queenbee`
- package: `queenbee`
- import: `import queenbee`
- owner: `runtime`
- rail: recipe (workflow schema contract)
- installed: `2.0.1`
- license: `MIT`
- abi: pure-Python (`py3-none-any`, no native extension); `pydantic>=2.0,<3.0` model floor, `pyyaml>=6.0` + `jsonschema>=4.17.3` runtime deps; the `[cli]` extra (`click` + `click-plugins`) is NOT admitted
- namespaces: `queenbee.base`, `queenbee.io` (`io.inputs`/`io.outputs`/`io.reference`/`io.artifact_source`/`io.common`), `queenbee.plugin`, `queenbee.recipe`, `queenbee.job`, `queenbee.repository`, `queenbee.config`, `queenbee.env`
- capability: the Pollination workflow-language schema graph — plugin/function operation templates, recipe/baked-recipe/DAG flow with dependency resolution, the typed IO input/output algebra and `${{...}}` reference family, job submission + run/step status lifecycle, and the package-repository index — all as `type`-discriminated pydantic v2 models over one JSON/YAML serialization spine

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: base model + metadata (`queenbee.base`)
- rail: recipe
- Every queenbee node descends from `BaseModel`; `type` is the discriminator (pinned to the class name by the `ensure_type_match` validator), `annotations` is a free-form, optional `Dict[str, Any]` carrier (`default_factory=dict`; the sibling `convert_none_to_empty_dict` before-validator coerces `None` to `{}`).

| [INDEX] | [SYMBOL]          | [TYPE_FAMILY] | [RAIL]                                                                                                                   |
| :-----: | :---------------- | :------------ | :----------------------------------------------------------------------------------------------------------------------- |
|  [01]   | `BaseModel`       | base model    | discriminated base: `type` literal + `annotations` dict; owns the `from_file`/`to_dict`/`to_json`/`to_yaml`/`yaml` spine |
|  [02]   | `BaseModelNoType` | base model    | type-free base for extension subclasses (same serialization spine, no `type` enforcement)                                |
|  [03]   | `MetaData`        | metadata      | `name`/`tag`/`app_version`/`keywords`/`maintainers`/`home`/`sources`/`icon`/`deprecated`/`description`/`license`         |
|  [04]   | `Maintainer`      | metadata      | `name`/`email`                                                                                                           |
|  [05]   | `License`         | metadata      | `name`/`url`                                                                                                             |

[PUBLIC_TYPE_SCOPE]: plugin + function operation templates (`queenbee.plugin`)
- rail: recipe

| [INDEX] | [SYMBOL]             | [TYPE_FAMILY]      | [RAIL]                                                                                                 |
| :-----: | :------------------- | :----------------- | :----------------------------------------------------------------------------------------------------- |
|  [01]   | `Plugin`             | operation owner    | `api_version`/`metadata`/`config: PluginConfig`/`functions: List[Function]`                            |
|  [02]   | `Function`           | operation (IOBase) | `name`/`description`/`command`/`language`/`source`/`inputs`/`outputs`; one containerized step template |
|  [03]   | `PluginConfig`       | config             | `docker: DockerConfig`/`local: LocalConfig`                                                            |
|  [04]   | `DockerConfig`       | config             | `image`/`registry`/`workdir` — the execution image                                                     |
|  [05]   | `LocalConfig`        | config             | local (no-container) execution marker                                                                  |
|  [06]   | `ScriptingLanguages` | enum               | function source language (`python`)                                                                    |

[PUBLIC_TYPE_SCOPE]: recipe + DAG flow (`queenbee.recipe`)
- rail: recipe

| [INDEX] | [SYMBOL]                        | [TYPE_FAMILY]  | [RAIL]                                                                                                        |
| :-----: | :------------------------------ | :------------- | :------------------------------------------------------------------------------------------------------------ |
|  [01]   | `Recipe`                        | flow owner     | `api_version`/`metadata`/`dependencies: List[Dependency]`/`flow: List[DAG]` — the authored, unresolved recipe |
|  [02]   | `BakedRecipe`                   | flow owner     | `Recipe` + `digest` (content lock) + `templates` — all dependencies inlined; the runnable artifact            |
|  [03]   | `RecipeInterface`               | flow interface | lean `metadata`/`source`/`inputs`/`outputs` projection (no flow body) for job-form discovery                  |
|  [04]   | `DAG`                           | flow (IOBase)  | `name`/`fail_fast`/`tasks: List[DAGTask]`/`inputs`/`outputs` — one dependency graph                           |
|  [05]   | `DAGTask`                       | flow node      | `name`/`template`/`needs`/`arguments`/`loop: DAGTaskLoop`/`sub_folder`/`returns` — one task invocation        |
|  [06]   | `DAGTaskLoop`                   | flow control   | `from_` — fan-out item source for a looped task                                                               |
|  [07]   | `TemplateFunction`              | flow template  | a `Function` baked into the recipe template set                                                               |
|  [08]   | `Dependency`                    | dependency     | `kind: DependencyKind`/`name`/`digest`/`alias`/`tag`/`source` — a recipe/plugin reference                     |
|  [09]   | `DependencyKind`                | enum           | `recipe` / `plugin`                                                                                           |
|  [10]   | `TaskReturn` / `TaskPathReturn` | flow output    | a task's declared parameter (`name`) / path (`path`/`required`) return                                        |

[PUBLIC_TYPE_SCOPE]: typed IO input/output algebra (`queenbee.io.inputs` / `queenbee.io.outputs`)
- rail: recipe
- A regular matrix: every IO node is `{DAG | Function | Step | <DAG…>Alias}<Type>{Input | Output}` over the value-type axis `{String, Integer, Number, Boolean, Folder, File, Path, Array, JSONObject}` (plus the `Generic`/`Linked` heads). The tier sets the binding stage; the type sets the payload. A design page constructs any member by composing tier + type; the representative bases below anchor the family.

| [INDEX] | [TIER_BASE]                                                             | [DISTINGUISHING_FIELDS_OVER_TYPE_NAME_DESCRIPTION]                                                                            | [RAIL] |
| :-----: | :---------------------------------------------------------------------- | :---------------------------------------------------------------------------------------------------------------------------- | :----- |
|  [01]   | `DAGGenericInput` (+`DAG<Type>Input`)                                   | `default`/`alias`/`required`/`spec` (+`extensions` on File/Path, +`items_type` on Array) — recipe-level declared input        |        |
|  [02]   | `Function<Type>Input` (`FunctionStringInput`…`FunctionJSONObjectInput`) | DAG fields + `path` on artifact inputs (Folder/File/Path) — function-level input                                              |        |
|  [03]   | `Step<Type>Input`                                                       | Function fields + `value` (parameters) / `source` (artifacts) — the runtime-bound input value                                 |        |
|  [04]   | `DAG<Type>InputAlias`                                                   | `GenericInput` + `platform`/`handler: IOAliasHandler`/`default`/`required`/`spec` — UI/handler-aliased input                  |        |
|  [05]   | `DAG<Type>Output`                                                       | `from_` (reference) / `alias`/`required` — recipe-level output drawn from a task return                                       |        |
|  [06]   | `Function<Type>Output`                                                  | `path`/`required` (`PathOutput`-based) — function-level produced output                                                       |        |
|  [07]   | `Step<Type>Output`                                                      | Function/DAG output + `value` / `source` — the runtime-resolved output value                                                  |        |
|  [08]   | `DAG<Type>OutputAlias`                                                  | `platform`/`handler`/`required`/`from_` — UI/handler-aliased output                                                           |        |
|  [09]   | `GenericInput`/`GenericOutput`/`PathOutput`/`FromOutput`                | the `io.common` heads the matrix derives from                                                                                 |        |
|  [10]   | `IOAliasHandler`                                                        | `language`/`module`/`function`/`index` — the post-processing handler an alias binds (resolved against `pollination-handlers`) |        |
|  [11]   | `ItemType`                                                              | enum: `Generic`/`String`/`Integer`/`Number`/`Boolean`/`Array`/`JSONObject` — the value-type axis                              |        |

[PUBLIC_TYPE_SCOPE]: references, artifact sources, arguments (`queenbee.io.reference` / `io.artifact_source` / `io.inputs`)
- rail: recipe
- The `${{...}}` template-reference family is a `type`-discriminated union; each variant resolves a value at bake/run time.

| [INDEX] | [SYMBOL]                                                                                | [TYPE_FAMILY]   | [RAIL]                                                                                         |
| :-----: | :-------------------------------------------------------------------------------------- | :-------------- | :--------------------------------------------------------------------------------------------- |
|  [01]   | `InputReference` (+`InputFileReference`/`InputFolderReference`/`InputPathReference`)    | reference       | `${{inputs.<variable>}}` — recipe/function input ref                                           |
|  [02]   | `TaskReference` (+`TaskFileReference`/`TaskFolderReference`/`TaskPathReference`)        | reference       | `${{tasks.<name>.outputs.<variable>}}` — task output ref                                       |
|  [03]   | `ItemReference`                                                                         | reference       | `${{item...}}` — loop-item ref                                                                 |
|  [04]   | `ValueReference` / `ValueListReference` / `ValueFileReference` / `ValueFolderReference` | reference       | inline literal value / list / file-path value ref                                              |
|  [05]   | `FileReference` / `FolderReference`                                                     | reference       | `io.common` artifact path references                                                           |
|  [06]   | `ProjectFolder` / `HTTP` / `S3`                                                         | artifact source | discriminated `_ArtifactSource`: `path` / `url` / `key`+`endpoint`+`bucket`+`credentials_path` |
|  [07]   | `JobArgument` / `JobPathArgument`                                                       | argument        | job-level parameter (`name`/`value`) / artifact (`name`/`source`)                              |
|  [08]   | `TaskArgument` / `TaskPathArgument`                                                     | argument        | task-level parameter (`name`/`from_`) / artifact (`name`/`from_`/`sub_path`)                   |

[PUBLIC_TYPE_SCOPE]: job submission + run lifecycle (`queenbee.job`)
- rail: recipe

| [INDEX] | [SYMBOL]         | [TYPE_FAMILY]        | [RAIL]                                                                                                                                                     |
| :-----: | :--------------- | :------------------- | :--------------------------------------------------------------------------------------------------------------------------------------------------------- |
|  [01]   | `Job`            | submission           | `api_version` (`v1beta1`)/`source`/`arguments: List[List[JobArgument\|JobPathArgument]]` (one inner list per parametric run)/`name`/`description`/`labels` |
|  [02]   | `JobStatus`      | receipt              | `id`/`status: JobStatusEnum`/`message`/`started_at`/`finished_at`/`source`/`runs_pending`/`runs_running`/`runs_completed`/`runs_failed`/`runs_cancelled`   |
|  [03]   | `RunStatus`      | receipt (BaseStatus) | `id`/`job_id`/`entrypoint`/`status: RunStatusEnum`/`steps`/`inputs`/`outputs`/timestamps                                                                   |
|  [04]   | `StepStatus`     | receipt (BaseStatus) | `id`/`name`/`status: StepStatusEnum`/`status_type: StatusType`/`template_ref`/`command`/`boundary_id`/`children_ids`/`outbound_steps`                      |
|  [05]   | `BaseStatus`     | receipt base         | `inputs`/`outputs`/`message`/`started_at`/`finished_at`/`source`                                                                                           |
|  [06]   | `Results`        | typed list           | `list`-subclass of run results with `from_runs(...)` projection                                                                                            |
|  [07]   | `JobStatusEnum`  | enum                 | `Created`/`Pre-Processing`/`Running`/`Failed`/`Cancelled`/`Completed`/`Unknown`                                                                            |
|  [08]   | `RunStatusEnum`  | enum                 | `Created`/`Scheduled`/`Running`/`Post-Processing`/`Failed`/`Cancelled`/`Succeeded`/`Unknown`                                                               |
|  [09]   | `StepStatusEnum` | enum                 | `Scheduled`/`Running`/`Failed`/`Succeeded`/`Skipped`/`Unknown`                                                                                             |
|  [10]   | `StatusType`     | enum                 | `Function`/`DAG`/`Loop`/`Container`/`Unknown` — the step-kind discriminant                                                                                 |

[PUBLIC_TYPE_SCOPE]: package repository + client config (`queenbee.repository` / `queenbee.config`)
- rail: recipe

| [INDEX] | [SYMBOL]                              | [TYPE_FAMILY]       | [RAIL]                                                                                                        |
| :-----: | :------------------------------------ | :------------------ | :------------------------------------------------------------------------------------------------------------ |
|  [01]   | `RepositoryIndex`                     | registry            | `api_version`/`generated`/`metadata`/`plugin`/`recipe` — the package-index manifest                           |
|  [02]   | `RepositoryMetadata`                  | registry            | `name`/`description`/`source`/`plugin_count`/`recipe_count`                                                   |
|  [03]   | `PackageVersion`                      | registry (MetaData) | `MetaData` + `url`/`created`/`digest`/`slug`/`kind`/`readme`/`manifest` — one published plugin/recipe version |
|  [04]   | `Config`                              | client config       | `auth: List[BaseAuth]`/`repositories: List[RepositoryReference]`                                              |
|  [05]   | `BaseAuth` / `JWTAuth` / `HeaderAuth` | client auth         | `domain`/`access_token` (+`header_name` on `HeaderAuth`) — repository credentials                             |
|  [06]   | `RepositoryReference`                 | client config       | `name`/`path` — a named repository endpoint                                                                   |
|  [07]   | `OS` (`queenbee.env`)                 | env                 | platform-switch helper (not an enum): `is_windows`/`is_nix` bools + `file_uri_prefix` for local-file URIs     |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: serialization spine (every `BaseModel`)
- rail: recipe
- The one round-trip surface; `from_file` and `to_*` wrap pydantic v2 `model_validate`/`model_dump` with queenbee defaults (`by_alias=True`, `exclude_none` on writes).

| [INDEX] | [SURFACE]                                                                                 | [ENTRY_FAMILY] | [RAIL]                                                                                                   |
| :-----: | :---------------------------------------------------------------------------------------- | :------------- | :------------------------------------------------------------------------------------------------------- |
|  [01]   | `BaseModel.from_file(filepath) -> Self`                                                   | load           | parse a JSON or YAML file (by extension) and `model_validate`; raises `ValueError` on validation failure |
|  [02]   | `BaseModel.model_validate(obj)` / `model_validate_json(json_data)`                        | parse          | pydantic v2 validation from a dict / JSON bytes — the boundary entry                                     |
|  [03]   | `BaseModel.to_dict(*, exclude_defaults=False, by_alias=True, exclude_none=False)`         | encode         | model -> wire dict (the cross-rail projection)                                                           |
|  [04]   | `BaseModel.to_json(filepath, *, indent=None)` / `to_yaml(filepath, *, exclude_none=True)` | write          | persist as JSON / YAML file                                                                              |
|  [05]   | `BaseModel.yaml(*, exclude_defaults=False, exclude_none=False)` / `model_dump_json(...)`  | encode         | YAML string / JSON string in memory                                                                      |
|  [06]   | `BaseModel.model_json_schema()`                                                           | schema         | the pydantic v2 JSON-Schema generator for the model (contract publication)                               |

[ENTRYPOINT_SCOPE]: recipe authoring, baking, and folder IO (`Recipe`/`BakedRecipe`/`RecipeInterface`/`Plugin`)
- rail: recipe

| [INDEX] | [SURFACE]                                                                                                                       | [ENTRY_FAMILY] | [RAIL]                                                     |
| :-----: | :------------------------------------------------------------------------------------------------------------------------------ | :------------- | :--------------------------------------------------------- |
|  [01]   | `Recipe.from_folder(folder_path)` / `Recipe.to_folder(folder_path, readme_string=None)`                                         | folder IO      | load/write a multi-file recipe folder                      |
|  [02]   | `Recipe.lock_dependencies(config=Config())` / `write_dependencies(folder_path, config)` / `write_dependency_file(folder_path)`  | resolve        | resolve + pin dependency digests against repositories      |
|  [03]   | `BakedRecipe.from_recipe(recipe, config=Config())` / `BakedRecipe.from_folder(folder_path, refresh_deps=True, config=Config())` | bake           | inline all dependencies into a runnable `BakedRecipe`      |
|  [04]   | `BakedRecipe.template_by_name(...)` / `dag_by_name(flow, name)` / `dependency_by_name(deps, name)` / `check_inputs()`           | navigate       | resolve a template/DAG/dependency by name; validate inputs |
|  [05]   | `RecipeInterface.from_recipe(recipe, source=None)`                                                                              | project        | derive the lean inputs/outputs interface for a job form    |
|  [06]   | `Plugin.from_folder(folder_path)` / `Plugin.to_folder(folder_path, *, readme_string=None)`                                      | folder IO      | load/write a plugin folder                                 |

[ENTRYPOINT_SCOPE]: DAG/function navigation, job binding, repository index (`DAG`/`Function`/`Job`/`RepositoryIndex`)
- rail: recipe
- `IOBase` (the shared base of `Function`/`DAG`/`BaseStatus`) owns the polymorphic IO accessors — one discriminating lookup, never a `get`/`getMany`/`getBy` family.

| [INDEX] | [SURFACE]                                                                                                                                                      | [ENTRY_FAMILY] | [RAIL]                                                        |
| :-----: | :------------------------------------------------------------------------------------------------------------------------------------------------------------- | :------------- | :------------------------------------------------------------ |
|  [01]   | `IOBase.artifact_input_by_name(name)` / `artifact_output_by_name(name)` / `parameter_input_by_name(name)` / `parameter_output_by_name(name)`                   | navigate       | polymorphic IO lookup shared by `Function`/`DAG`/`BaseStatus` |
|  [02]   | `DAG.get_task(name)` / `DAG.find_task_return(tasks, reference)` / `DAG.check_dag_references()`                                                                 | navigate       | task lookup, return resolution, reference validation          |
|  [03]   | `Function.check_either_source_or_command()`                                                                                                                    | validate       | enforce the source-xor-command invariant                      |
|  [04]   | `Job.validate_arguments(inputs)` / `Job.populate_default_arguments(inputs)`                                                                                    | bind           | validate/fill job arguments against the recipe input set      |
|  [05]   | `RepositoryIndex.from_folder(folder_path)` / `index_plugin_version(...)` / `index_recipe_version(...)` / `merge_folder(...)`                                   | registry build | build/extend a repository index from packaged resources       |
|  [06]   | `RepositoryIndex.package_by_tag(kind, name, tag)` / `package_by_digest(kind, name, digest)` / `search(kind=None, search_string=None)` / `get_latest(versions)` | registry query | resolve a `PackageVersion` by tag/digest/search/latest        |

## [04]-[IMPLEMENTATION_LAW]

[SCHEMA_TOPOLOGY]:
- discriminator law: every node is a `type`-discriminated `BaseModel`; the `type` literal (pinned by `ensure_type_match`) is the union tag for the IO/reference/artifact-source families. Boundary decode resolves the concrete variant from `type`, never a manual `isinstance` ladder. The `annotations` dict is the free-form extension carrier — never widen a model with parallel fields a downstream schema can hold in `annotations`.
- single-spine law: a recipe/plugin/job round-trips through exactly one surface — `from_file` (or `model_validate(_json)`) in, `to_dict`/`to_json`/`to_yaml` out. No second parser, no hand-rolled YAML walk; the queenbee defaults (`by_alias=True`, `exclude_none` on writes) are the canonical wire shape.
- bake law: authored `Recipe` (digest-free, `Dependency` references unresolved) is distinct from `BakedRecipe` (dependencies inlined, `digest` locked, `templates` populated). Execution always consumes a `BakedRecipe`; `BakedRecipe.from_recipe`/`from_folder` is the only resolution path, and `RecipeInterface` is the lean form-discovery projection — never re-derive inputs/outputs by hand.
- IO-matrix law: an input/output is selected by tier (`DAG`/`Function`/`Step`/`Alias`) × value-type (`ItemType`), not by a bespoke per-recipe class. `Step*` carries the runtime-bound `value`/`source`; `*Alias` carries the `IOAliasHandler` post-processor. Construct from the matrix; do not invent a parallel IO model.

[LOCAL_ADMISSION]:
- The recipe owner admits the queenbee SCHEMA graph only. queenbee's optional click-based CLI (`[cli]` extra) is rejected — `cyclopts` is the sole admitted parser; recipe/job commands are cyclopts commands that compose `Recipe.from_file`/`Job.validate_arguments`, never queenbee's own entrypoints.
- queenbee's bundled transfer/fetch helpers (`base.request.make_request`/`get_uri`/`resolve_local_source`, `repository`/`config` urllib fetches) are NOT used: repository index and package fetch route through the runtime `httpx` async client, and `S3`/`HTTP`/`ProjectFolder` artifact movement routes through the `transport/roots#RESOURCE` rails — `obstore` for object-store schemes (the `roots.md` `OBJECT_STORE_SCHEMES` table), `httpx` for HTTP, `universal-pathlib`/`fsspec` for local roots. queenbee declares the artifact source; the runtime moves the bytes.
- queenbee models stay at the recipe-definition boundary. To cross the C#↔Python companion wire (protobuf/`msgspec.msgpack`), project through `to_dict(by_alias=True)` — never re-mint a queenbee schema as a second `msgspec`/protobuf struct (single-mint invariant: queenbee owns the recipe vocabulary, the wire owner owns transport).

[STACK_LAW]:
- pydantic boundary: external recipe/plugin YAML or JSON -> `Recipe.from_file` / `BakedRecipe.from_recipe` -> a validated `type`-discriminated graph; boundary failures surface as `pydantic.ValidationError` (wrapped `ValueError` by `from_file`) folded into the runtime `Result`/boundary-fault rail, not raised through the daemon.
- artifact + transfer: a `Job` `S3`/`HTTP` artifact source's `(bucket, key, endpoint)` / `url` feeds an `obstore` `get` (the roots `Transfer` acquisition rail) or an `httpx` stream; a `ProjectFolder` source feeds the `universal-pathlib`/`fsspec` local root. The artifact-content key for the runtime cache is the `evidence/identity#IDENTITY` `ContentIdentity` owner over `recipe.to_json` bytes — distinct from queenbee's own `BakedRecipe.digest`/`PackageVersion.digest` lock identity, which keys repository resolution.
- registry fetch: a remote `RepositoryIndex`/`PackageVersion` is fetched over the `httpx` async client under a `stamina` retry policy and decoded by `RepositoryIndex.model_validate_json(resp.content)`; `RepositoryIndex.search`/`get_latest`/`package_by_tag` then resolve the version — no queenbee blocking `make_request`.
- run lifecycle telemetry: a job submission/poll opens one OpenTelemetry span and a `structlog` event keyed by `Job.name` + `RunStatus.id` + `BakedRecipe.digest`; `JobStatusEnum`/`RunStatusEnum`/`StepStatusEnum` map to the span status, and `Results.from_runs` collects the typed result set. Synchronous queenbee calls (`from_folder`, repository IO) cross `anyio.to_thread.run_sync` so the async lane is never blocked.
- handler resolution: a `DAG<Type>InputAlias.handler` (`IOAliasHandler` `language`/`module`/`function`) resolves to a `pollination-handlers` callable; an `lbt-recipes` simulation recipe loads as a queenbee `BakedRecipe` and submits as a `Job` — the three-package recipe cluster meets on the queenbee schema.

[RAIL_LAW]:
- Package: `queenbee`
- Owns: the Pollination workflow-language schema graph — plugin/function templates, recipe/baked-recipe/DAG flow with dependency resolution, the typed IO input/output algebra and `${{...}}` reference family, job submission + run/step lifecycle receipts, and the package-repository index, all as `type`-discriminated pydantic v2 models over one JSON/YAML spine
- Accept: `Recipe`/`BakedRecipe`/`Plugin`/`Job` construction via `from_file`/`from_folder`/`from_recipe`, `model_validate(_json)` at the boundary, `to_dict`/`to_json`/`to_yaml` projection, the tier×type IO matrix, `IOBase` polymorphic accessors, `RepositoryIndex` query methods, `BakedRecipe.from_recipe` as the sole bake path
- Reject: the queenbee `[cli]` click extra (cyclopts owns the CLI), queenbee's urllib `make_request`/`get_uri` transfer + repository fetch (httpx/obstore own transfer), re-minting a queenbee schema as a parallel `msgspec`/protobuf struct, hand-rolled YAML/JSON walks bypassing the serialization spine, bespoke per-recipe IO classes outside the tier×type matrix, and executing an unbaked `Recipe` (always bake to `BakedRecipe` first)

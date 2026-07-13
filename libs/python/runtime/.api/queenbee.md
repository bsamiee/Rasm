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

| [INDEX] | [SYMBOL]          | [TYPE_FAMILY] | [RAIL]                                           |
| :-----: | :---------------- | :------------ | :----------------------------------------------- |
|  [01]   | `BaseModel`       | base model    | discriminated base; owns the serialization spine |
|  [02]   | `BaseModelNoType` | base model    | type-free extension base (no `type` enforcement) |
|  [03]   | `MetaData`        | metadata      | package/recipe descriptive metadata              |
|  [04]   | `Maintainer`      | metadata      | `name`/`email`                                   |
|  [05]   | `License`         | metadata      | `name`/`url`                                     |

- `BaseModel` fields: `type` literal + `annotations` dict; owns the `from_file`/`to_dict`/`to_json`/`to_yaml`/`yaml` spine
- `MetaData` fields: `name`/`tag`/`app_version`/`keywords`/`maintainers`/`home`/`sources`/`icon`/`deprecated`/`description`/`license`

[PUBLIC_TYPE_SCOPE]: plugin + function operation templates (`queenbee.plugin`)
- rail: recipe

| [INDEX] | [SYMBOL]             | [TYPE_FAMILY]      | [RAIL]                                             |
| :-----: | :------------------- | :----------------- | :------------------------------------------------- |
|  [01]   | `Plugin`             | operation owner    | plugin manifest: config + function set             |
|  [02]   | `Function`           | operation (IOBase) | one containerized step template                    |
|  [03]   | `PluginConfig`       | config             | `docker: DockerConfig`/`local: LocalConfig`        |
|  [04]   | `DockerConfig`       | config             | `image`/`registry`/`workdir` — the execution image |
|  [05]   | `LocalConfig`        | config             | local (no-container) execution marker              |
|  [06]   | `ScriptingLanguages` | enum               | function source language (`python`)                |

- `Plugin` fields: `api_version`/`metadata`/`config: PluginConfig`/`functions: List[Function]`
- `Function` fields: `name`/`description`/`command`/`language`/`source`/`inputs`/`outputs`

[PUBLIC_TYPE_SCOPE]: recipe + DAG flow (`queenbee.recipe`)
- rail: recipe

| [INDEX] | [SYMBOL]                        | [TYPE_FAMILY]  | [RAIL]                                                         |
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

- `Recipe` fields: `api_version`/`metadata`/`dependencies: List[Dependency]`/`flow: List[DAG]`
- `BakedRecipe` fields: `Recipe` + `digest` (content lock) + `templates`
- `DAG` fields: `name`/`fail_fast`/`tasks: List[DAGTask]`/`inputs`/`outputs`
- `DAGTask` fields: `name`/`template`/`needs`/`arguments`/`loop: DAGTaskLoop`/`sub_folder`/`returns`
- `Dependency` fields: `kind: DependencyKind`/`name`/`digest`/`alias`/`tag`/`source`

[PUBLIC_TYPE_SCOPE]: typed IO input/output algebra (`queenbee.io.inputs` / `queenbee.io.outputs`)
- rail: recipe
- A regular matrix: every IO node is `{DAG | Function | Step | <DAG…>Alias}<Type>{Input | Output}` over the value-type axis `{String, Integer, Number, Boolean, Folder, File, Path, Array, JSONObject}` (plus the `Generic`/`Linked` heads). The tier sets the binding stage; the type sets the payload. A design page constructs any member by composing tier + type; the representative bases below anchor the family.

| [INDEX] | [TIER]                                            | [ADDS]                                 | [BINDING]                              |
| :-----: | :------------------------------------------------ | :------------------------------------- | :------------------------------------- |
|  [01]   | `DAGGenericInput` (+`DAG<Type>Input`)             | `default`/`alias`/`required`/`spec`    | recipe-level declared input            |
|  [02]   | `Function<Type>Input`                             | + `path` on artifact inputs            | function-level input                   |
|  [03]   | `Step<Type>Input`                                 | + `value` / `source`                   | runtime-bound input value              |
|  [04]   | `DAG<Type>InputAlias`                             | + `platform`/`handler`                 | UI/handler-aliased input               |
|  [05]   | `DAG<Type>Output`                                 | `from_`/`alias`/`required`             | recipe-level output from a task return |
|  [06]   | `Function<Type>Output`                            | `path`/`required` (`PathOutput`)       | function-level produced output         |
|  [07]   | `Step<Type>Output`                                | + `value` / `source`                   | runtime-resolved output value          |
|  [08]   | `DAG<Type>OutputAlias`                            | + `platform`/`handler`                 | UI/handler-aliased output              |
|  [09]   | `Generic{Input,Output}`/`PathOutput`/`FromOutput` | `io.common` heads                      | the matrix derivation base             |
|  [10]   | `IOAliasHandler`                                  | `language`/`module`/`function`/`index` | alias post-processor                   |
|  [11]   | `ItemType`                                        | enum: `String`…`JSONObject`            | the value-type axis                    |

[PUBLIC_TYPE_SCOPE]: references, artifact sources, arguments (`queenbee.io.reference` / `io.artifact_source` / `io.inputs`)
- rail: recipe
- The `${{...}}` template-reference family is a `type`-discriminated union; each variant resolves a value at bake/run time.

| [INDEX] | [SYMBOL]                                               | [TYPE_FAMILY]   | [RAIL]                                                      |
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
- rail: recipe

| [INDEX] | [SYMBOL]         | [TYPE_FAMILY]        | [RAIL]                                                                                       |
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

- `Job` fields: `api_version` (`v1beta1`)/`source`/`arguments: List[List[JobArgument|JobPathArgument]]` (one inner list per run)/`name`/`description`/`labels`
- `JobStatus` fields: `id`/`status: JobStatusEnum`/`message`/`started_at`/`finished_at`/`source` + `runs_{pending,running,completed,failed,cancelled}` counts
- `StepStatus` fields: `id`/`name`/`status: StepStatusEnum`/`status_type: StatusType`/`template_ref`/`command`/`boundary_id`/`children_ids`/`outbound_steps`

[PUBLIC_TYPE_SCOPE]: package repository + client config (`queenbee.repository` / `queenbee.config`)
- rail: recipe

| [INDEX] | [SYMBOL]                              | [TYPE_FAMILY]       | [RAIL]                                                           |
| :-----: | :------------------------------------ | :------------------ | :--------------------------------------------------------------- |
|  [01]   | `RepositoryIndex`                     | registry            | the package-index manifest                                       |
|  [02]   | `RepositoryMetadata`                  | registry            | `name`/`description`/`source`/`plugin_count`/`recipe_count`      |
|  [03]   | `PackageVersion`                      | registry (MetaData) | one published plugin/recipe version                              |
|  [04]   | `Config`                              | client config       | `auth: List[BaseAuth]`/`repositories: List[RepositoryReference]` |
|  [05]   | `BaseAuth` / `JWTAuth` / `HeaderAuth` | client auth         | repository credentials `domain`/`access_token`                   |
|  [06]   | `RepositoryReference`                 | client config       | `name`/`path` — a named repository endpoint                      |
|  [07]   | `OS` (`queenbee.env`)                 | env                 | platform switch: `is_windows`/`is_nix` + `file_uri_prefix`       |

- `RepositoryIndex` fields: `api_version`/`generated`/`metadata`/`plugin`/`recipe`
- `PackageVersion` fields: `MetaData` + `url`/`created`/`digest`/`slug`/`kind`/`readme`/`manifest`
- `HeaderAuth` adds `header_name`; `OS.file_uri_prefix` builds local-file URIs.

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: serialization spine; surfaces are `BaseModel.*`
- rail: recipe
- The one round-trip surface; `from_file` and `to_*` wrap pydantic v2 `model_validate`/`model_dump` with queenbee defaults (`by_alias=True`, `exclude_none` on writes).

| [INDEX] | [SURFACE]                                                | [ENTRY_FAMILY] | [RAIL]                                                   |
| :-----: | :------------------------------------------------------- | :------------- | :------------------------------------------------------- |
|  [01]   | `from_file(filepath) -> Self`                            | load           | parse JSON/YAML by ext; raises `ValueError` on failure   |
|  [02]   | `model_validate(obj)` / `model_validate_json(json_data)` | parse          | pydantic v2 validation from a dict / JSON bytes          |
|  [03]   | `to_dict(*, exclude_defaults, by_alias, exclude_none)`   | encode         | model → wire dict (the cross-rail projection)            |
|  [04]   | `to_json(filepath, ...)` / `to_yaml(filepath, ...)`      | write          | persist as JSON / YAML file                              |
|  [05]   | `yaml(...)` / `model_dump_json(...)`                     | encode         | YAML / JSON string in memory                             |
|  [06]   | `model_json_schema()`                                    | schema         | pydantic v2 JSON-Schema generator (contract publication) |

[ENTRYPOINT_SCOPE]: recipe authoring, baking, and folder IO (`Recipe`/`BakedRecipe`/`RecipeInterface`/`Plugin`)
- rail: recipe

| [INDEX] | [SURFACE]                                                | [ENTRY_FAMILY] | [RAIL]                                                  |
| :-----: | :------------------------------------------------------- | :------------- | :------------------------------------------------------ |
|  [01]   | `Recipe.from_folder(...)` / `Recipe.to_folder(...)`      | folder IO      | load/write a multi-file recipe folder                   |
|  [02]   | `Recipe.lock_dependencies(config=Config())`              | resolve        | resolve + pin dependency digests against repositories   |
|  [03]   | `Recipe.write_dependencies` / `write_dependency_file`    | resolve        | write the resolved lock file(s) to the folder           |
|  [04]   | `BakedRecipe.from_recipe` / `from_folder`                | bake           | inline all dependencies into a runnable `BakedRecipe`   |
|  [05]   | `BakedRecipe.template_by_name(...)` / `dag_by_name(...)` | navigate       | resolve a template/DAG by name                          |
|  [06]   | `BakedRecipe.dependency_by_name(...)` / `check_inputs()` | navigate       | resolve a dependency by name; validate inputs           |
|  [07]   | `RecipeInterface.from_recipe(recipe, source=None)`       | project        | derive the lean inputs/outputs interface for a job form |
|  [08]   | `Plugin.from_folder(...)` / `Plugin.to_folder(...)`      | folder IO      | load/write a plugin folder                              |

[ENTRYPOINT_SCOPE]: DAG/function navigation, job binding, repository index (`DAG`/`Function`/`Job`/`RepositoryIndex`)
- rail: recipe
- `IOBase` (the shared base of `Function`/`DAG`/`BaseStatus`) owns the polymorphic IO accessors — one discriminating lookup, never a `get`/`getMany`/`getBy` family.

| [INDEX] | [SURFACE]                                                    | [ENTRY_FAMILY] | [RAIL]                                                  |
| :-----: | :----------------------------------------------------------- | :------------- | :------------------------------------------------------ |
|  [01]   | `IOBase.{artifact,parameter}_{input,output}_by_name(name)`   | navigate       | polymorphic IO lookup on `Function`/`DAG`/`BaseStatus`  |
|  [02]   | `DAG.get_task` / `find_task_return` / `check_dag_references` | navigate       | task lookup, return resolution, reference validation    |
|  [03]   | `Function.check_either_source_or_command()`                  | validate       | enforce the source-xor-command invariant                |
|  [04]   | `Job.validate_arguments` / `populate_default_arguments`      | bind           | validate/fill job arguments against the recipe inputs   |
|  [05]   | `RepositoryIndex.from_folder(...)` / `merge_folder(...)`     | registry build | build/extend a repository index from packaged resources |
|  [06]   | `RepositoryIndex.index_{plugin,recipe}_version(...)`         | registry build | add one packaged plugin/recipe version to the index     |
|  [07]   | `RepositoryIndex.package_by_{tag,digest}(...)`               | registry query | resolve a `PackageVersion` by tag or digest             |
|  [08]   | `RepositoryIndex.search(...)` / `get_latest(versions)`       | registry query | search the index or pick the latest version             |

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

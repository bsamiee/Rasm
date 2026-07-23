# [PY_ARTIFACTS_API_PAPERMILL]

`papermill` owns parameterized notebook execution for the artifacts notebook rail: it injects a typed parameters cell, translates it to the target kernel language, runs the notebook end-to-end through a registered engine, and routes notebook I/O across local and cloud path schemes through a scheme-dispatched handler registry. It drives the run over the `nbclient` kernel loop rather than owning the kernel protocol, sitting above `nbclient` and beside `jupytext`/`nbconvert` on the reports chain.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `papermill`
- package: `papermill`
- import: `papermill`
- owner: `artifacts`
- rail: notebook
- entry points: `papermill` CLI (`click`-backed), executing over the `nbclient` engine

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: execution and engine family
- rail: notebook — `papermill.engines`

| [INDEX] | [SYMBOL]                   | [TYPE_FAMILY]     | [CAPABILITY]                                                                            |
| :-----: | :------------------------- | :---------------- | :-------------------------------------------------------------------------------------- |
|  [01]   | `Engine`                   | engine base       | `execute_notebook` (wraps in manager) + `execute_managed_notebook` contract             |
|  [02]   | `NBClientEngine`           | nbclient engine   | executes via `PapermillNotebookClient` over `nbclient`; default engine                  |
|  [03]   | `PapermillEngines`         | engine registry   | `register`/`register_entry_points`/`get_engine`/`execute_notebook_with_engine`          |
|  [04]   | `NotebookExecutionManager` | execution tracker | callbacks, tqdm pbar, autosave, timing; states `PENDING`/`RUNNING`/`COMPLETED`/`FAILED` |

[PUBLIC_TYPE_SCOPE]: I/O handler family
- rail: notebook — `papermill.iorw`

Every handler implements the four-method contract `read(path)`/`write(buf, path)`/`listdir(path)`/`pretty_path(path)`; `PapermillIO` dispatches by scheme prefix.

| [INDEX] | [SYMBOL]              | [TYPE_FAMILY] | [CAPABILITY]                                               |
| :-----: | :-------------------- | :------------ | :--------------------------------------------------------- |
|  [01]   | `PapermillIO`         | I/O registry  | route path strings to registered handlers by scheme prefix |
|  [02]   | `LocalHandler`        | local I/O     | file-system read/write (also exposes `cwd` for the kernel) |
|  [03]   | `S3Handler`           | S3 I/O        | AWS S3 notebook read/write (`s3://`; `boto3` extra)        |
|  [04]   | `GCSHandler`          | GCS I/O       | Google Cloud Storage read/write (`gs://`; `gcsfs` extra)   |
|  [05]   | `ABSHandler`          | ABS I/O       | Azure Blob Storage read/write (`abfs://`; `azure` extra)   |
|  [06]   | `ADLHandler`          | ADLS I/O      | Azure Data Lake Storage read/write (legacy `adl://`)       |
|  [07]   | `HDFSHandler`         | HDFS I/O      | Hadoop FS read/write (`hdfs://`; `pyarrow` extra)          |
|  [08]   | `HttpHandler`         | HTTP I/O      | HTTP/HTTPS URL notebook fetch                              |
|  [09]   | `GithubHandler`       | GitHub I/O    | GitHub raw URL read                                        |
|  [10]   | `StreamHandler`       | stream I/O    | file-like stream read/write                                |
|  [11]   | `NotebookNodeHandler` | in-memory I/O | route an in-memory `NotebookNode` as input without a path  |
|  [12]   | `NoIOHandler`         | null I/O      | discard-output handler for execute-only-no-write runs      |

[PUBLIC_TYPE_SCOPE]: parameter translator family
- rail: notebook — `papermill.translators`

`Translator.codify(parameters, comment)` folds the parameter dict into a kernel-language parameters cell through `translate`/`assign`/`comment` and the per-type `translate_*` rows; `inspect` reads declared parameters back. Each translator registers under its language key.

| [INDEX] | [SYMBOL]               | [TYPE_FAMILY]       | [CAPABILITY]                                                                          |
| :-----: | :--------------------- | :------------------ | :------------------------------------------------------------------------------------ |
|  [01]   | `Translator`           | translator base     | `codify`/`assign`/`comment` + per-type `translate_*` parameter serialization contract |
|  [02]   | `PapermillTranslators` | translator registry | `find_translator(kernel, language)` -> `Translator`; `register(language, translator)` |
|  [03]   | `PythonTranslator`     | Python              | registry key `python`                                                                 |
|  [04]   | `RTranslator`          | R                   | registry key `R`                                                                      |
|  [05]   | `JuliaTranslator`      | Julia               | registry key `julia`                                                                  |
|  [06]   | `ScalaTranslator`      | Scala               | registry key `scala`                                                                  |
|  [07]   | `BashTranslator`       | Bash                | registry key `bash`                                                                   |
|  [08]   | `MatlabTranslator`     | Matlab              | registry key `matlab`                                                                 |
|  [09]   | `CSharpTranslator`     | .NET C#             | registry key `.net-csharp`                                                            |
|  [10]   | `FSharpTranslator`     | .NET F#             | registry key `.net-fsharp`                                                            |
|  [11]   | `PowershellTranslator` | .NET PowerShell     | registry key `.net-powershell`                                                        |

[PUBLIC_TYPE_SCOPE]: exception family
- rail: notebook — `papermill.exceptions`

`PapermillExecutionError` carries `(cell_index, exec_count, source, ename, evalue, traceback)` from the failing cell, with the error output already embedded in the written notebook.

| [INDEX] | [SYMBOL]                               | [TYPE_FAMILY]       | [CAPABILITY]                                                          |
| :-----: | :------------------------------------- | :------------------ | :-------------------------------------------------------------------- |
|  [01]   | `PapermillException`                   | base fault          | base for all papermill errors                                         |
|  [02]   | `PapermillExecutionError`              | execution fault     | typed cell fault carrying the failing cell's fields (see lead)        |
|  [03]   | `PapermillMissingParameterException`   | param fault         | required parameter absent during parameterization                     |
|  [04]   | `PapermillRateLimitException`          | I/O rate fault      | cloud I/O handler rate limit exceeded (drives `tenacity` retry)       |
|  [05]   | `PapermillOptionalDependencyException` | extra-missing fault | cloud handler extra (`boto3`/`gcsfs`/`azure`/`pyarrow`) not installed |
|  [06]   | `AwsError`                             | AWS fault           | underlying AWS/S3 transport error surfaced from the handler           |
|  [07]   | `PapermillWarning`                     | warning base        | base for papermill warnings                                           |
|  [08]   | `PapermillParameterOverwriteWarning`   | param warning       | a passed parameter overwrites a built-in/declared default             |

[PUBLIC_TYPE_SCOPE]: model family
- rail: notebook — `papermill.models`

| [INDEX] | [SYMBOL]    | [TYPE_FAMILY] | [CAPABILITY]                                                           |
| :-----: | :---------- | :------------ | :--------------------------------------------------------------------- |
|  [01]   | `Parameter` | named tuple   | `name`, `inferred_type_name`, `default`, `help` fields from inspection |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: notebook execution and inspection
- rail: notebook — `papermill`

`execute_notebook`/`inspect_notebook` export at top-level `papermill`; `parameterize_notebook`/`add_builtin_parameters` live in `papermill.parameterize` and `translate_parameters` in `papermill.translators`.

| [INDEX] | [SURFACE]                                                              | [CAPABILITY]                                     |
| :-----: | :--------------------------------------------------------------------- | :----------------------------------------------- |
|  [01]   | `execute_notebook(input_path, output_path, parameters, ...)`           | parameterize and execute a notebook end-to-end   |
|  [02]   | `inspect_notebook(notebook_path, parameters)`                          | extract declared parameters without executing    |
|  [03]   | `parameterize_notebook(nb, parameters, report_mode, kernel_name, ...)` | inject a parameters cell into a `NotebookNode`   |
|  [04]   | `add_builtin_parameters(parameters)`                                   | merge papermill built-in parameters into a dict  |
|  [05]   | `translate_parameters(kernel_name, language, parameters, comment)`     | serialize parameter dict to kernel-specific code |

[ENTRYPOINT_SCOPE]: I/O operations
- rail: notebook — `papermill.iorw`

| [INDEX] | [SURFACE]                                   | [CAPABILITY]                                  |
| :-----: | :------------------------------------------ | :-------------------------------------------- |
|  [01]   | `load_notebook_node(notebook_path)`         | read a notebook from any registered path      |
|  [02]   | `write_ipynb(nb, path)`                     | write a `NotebookNode` to any registered path |
|  [03]   | `list_notebook_files(path)`                 | list notebook files under a directory path    |
|  [04]   | `PapermillIO.register(scheme, handler)`     | register a custom I/O handler for a scheme    |
|  [05]   | `PapermillIO.read(path, extensions)`        | read via routing to the matching handler      |
|  [06]   | `PapermillIO.write(buf, path, extensions)`  | write via routing to the matching handler     |
|  [07]   | `PapermillIO.get_handler(path, extensions)` | resolve the handler serving a path            |
|  [08]   | `PapermillIO.listdir(path)`                 | list notebook files via the matching handler  |
|  [09]   | `PapermillIO.register_entry_points()`       | load handlers declared by installed extras    |
|  [10]   | `PapermillIO.reset()`                       | clear and re-seed the default handler table   |

[ENTRYPOINT_SCOPE]: execution manager lifecycle
- rail: notebook — `papermill.engines.NotebookExecutionManager`

| [INDEX] | [SURFACE]                                    | [CAPABILITY]                                   |
| :-----: | :------------------------------------------- | :--------------------------------------------- |
|  [01]   | `notebook_start(**kwargs)`                   | record notebook start time; initialize pbar    |
|  [02]   | `notebook_complete(**kwargs)`                | mark notebook complete; finalize pbar and save |
|  [03]   | `cell_start(cell, cell_index, **kwargs)`     | mark cell execution start; update pbar         |
|  [04]   | `cell_complete(cell, cell_index, **kwargs)`  | mark cell complete; autosave if configured     |
|  [05]   | `cell_exception(cell, cell_index, **kwargs)` | record cell-level exception for the manager    |
|  [06]   | `save(**kwargs)`                             | write current notebook state to output path    |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- primary axis: `execute_notebook(input_path, output_path, parameters={...})` is the single call for a parameterized run; `prepare_only=True` writes the parameterized notebook without executing; `report_mode=True` hides the injected parameters cell in the output.
- engine axis: `engine_name=None` selects `NBClientEngine` (built on `papermill.clientwrap.PapermillNotebookClient` over `nbclient.NotebookClient`); a custom engine registers via `papermill_engines.register(name, engine)` or entry points, and `**engine_kwargs` (`timeout`, `start_timeout`, `kernel_name`) pass straight into the underlying `nbclient` client.
- I/O axis: `papermill_io` routes `input_path`/`output_path` by scheme prefix; a cloud handler lazy-imports its extra and raises `PapermillOptionalDependencyException` when absent.
- retry axis: cloud reads/writes run under a `tenacity` `retry(retry_if_exception_type, stop_after_attempt, wait_exponential)` policy, so a `PapermillRateLimitException` triggers exponential backoff — papermill's own retry, threaded inside any outer boundary rather than duplicated by it.
- parameter axis: `parameters` translate to kernel code through `PapermillTranslators.find_translator(kernel_name, language).codify(...)`; the cell tagged `parameters` marks the injection point; `add_builtin_parameters` merges papermill's built-ins; a value overwriting a default emits `PapermillParameterOverwriteWarning`.
- inspection axis: `inspect_notebook(path)` returns a dict of `Parameter` named tuples keyed by name via the translator's `inspect` classmethod, starting no kernel.
- fault axis: a failing cell raises `PapermillExecutionError` (fields in `[02]`), and the egress folds this typed receipt onto the rail fault channel.

[STACKING]:
- `nbclient`(`.api/nbclient`): `NBClientEngine` runs the parameterized node over `PapermillNotebookClient` (a `nbclient.NotebookClient` subclass) — papermill drives the run and nbclient owns the kernel loop, so `**engine_kwargs` (`timeout`/`start_timeout`/`kernel_name`) pass through to the client and every cell/timeout/kernel fault surfaces as an `nbclient` exception the boundary folds.
- `jupytext`(`.api/jupytext`): `parameterize_notebook` injects the parameters cell into the `jupytext.reads` source `NotebookNode` before execution; the executed node archives via `jupytext.writes(executed, "ipynb")`.
- `nbconvert`(`.api/nbconvert`): the executed node lowers downstream through `nbconvert.get_exporter(...).from_notebook_node(executed)`.
- `expression`(`.api/expression`): the `document/report#NBCLIENT_ENGINE` owner folds `PapermillExecutionError`/`PapermillRateLimitException`/`PapermillMissingParameterException` through the `runtime/reliability/faults#FAULT` boundary onto `RuntimeRail = Result[T, BoundaryFault]`; papermill's `tenacity` cloud-retry sits inside that boundary.
- within-lib (`document/report`): `NotebookEngine.client_kwargs()` projects the frozen reproducibility struct to `execute_notebook`/engine traits, and `_notebook_arm` composes `parameterize_notebook` -> `NBClientEngine` -> `nbconvert` as one arm.

[LOCAL_ADMISSION]:
- import `papermill` at boundary scope only.
- `execute_notebook` returns the mutated `nbformat.NotebookNode` and writes it to `output_path` throughout the run (`request_save_on_cell_execute=True` drives autosave).
- `progress_bar=True` emits a tqdm cell-level bar to stdout (`False` in headless/log pipelines); `log_output=True` streams cell stdout/stderr to the logger, with `stdout_file`/`stderr_file` capturing them independently; `cwd` sets the kernel working directory.
- a custom handler registers via `papermill_io.register(scheme, handler)` before `execute_notebook`; `NotebookNodeHandler` routes an in-memory `NotebookNode` with no path, `NoIOHandler` discards output for execute-only runs.

[RAIL_LAW]:
- Package: `papermill`
- Owns: parameterized notebook execution, parameter injection, kernel-language translation across Python/R/Julia/Scala/Bash/Matlab/.NET, pluggable notebook I/O over the four-method handler protocol, `tenacity`-backed cloud retry, and execution lifecycle management with progress/autosave/typed cell faults
- Accept: any path scheme with a registered handler; parameter dicts with kernel-serializable values; a `NotebookNode` from `nbformat` via `NotebookNodeHandler`
- Reject: hand-rolled parameter injection outside `parameterize_notebook`/`Translator.codify`; a custom execution loop duplicating `execute_notebook` or re-implementing the `nbclient` kernel protocol; a hand-rolled cloud-IO retry where `tenacity` already backs off on `PapermillRateLimitException`; re-parsing a traceback string where `PapermillExecutionError` carries the typed cell fault; a per-scheme reader/writer split where the four-method handler protocol owns routing

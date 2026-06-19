# [PY_ARTIFACTS_API_PAPERMILL]

`papermill` supplies parameterized notebook execution for the artifacts notebook rail: `execute_notebook` is the single end-to-end entrypoint; `inspect_notebook` extracts declared parameters; `parameterize_notebook` injects a parameters cell; `PapermillIO` owns pluggable notebook I/O across local, S3, GCS, ABS, ADLS, and HTTP paths; `Translator` and `PapermillTranslators` own parameter serialization across Python, R, Julia, Scala, Bash, and other kernels.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `papermill`
- package: `papermill`
- import: `papermill`
- owner: `artifacts`
- rail: notebook
- entry points: `papermill` CLI (`jupyter nbconvert` backend)
- capability: parameterized notebook execution, parameter injection, kernel-language translation, pluggable notebook I/O (local/S3/GCS/ABS/ADLS/HTTP), and execution lifecycle management

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: execution and engine family
- rail: notebook — `papermill.engines`

| [INDEX] | [SYMBOL]                   | [TYPE_FAMILY]     | [CAPABILITY]                                                |
| :-----: | :------------------------- | :---------------- | :---------------------------------------------------------- |
|  [01]   | `Engine`                   | engine base       | abstract execution contract; `execute_notebook_with_engine` |
|  [02]   | `NBClientEngine`           | nbclient engine   | executes via `nbclient.NotebookClient`; default engine      |
|  [03]   | `PapermillEngines`         | engine registry   | maps engine name strings to `Engine` subclasses             |
|  [04]   | `NotebookExecutionManager` | execution tracker | lifecycle callbacks; progress bar; autosave; cell timing    |

[PUBLIC_TYPE_SCOPE]: I/O handler family
- rail: notebook — `papermill.iorw`

| [INDEX] | [SYMBOL]        | [TYPE_FAMILY] | [CAPABILITY]                              |
| :-----: | :-------------- | :------------ | :---------------------------------------- |
|  [01]   | `PapermillIO`   | I/O registry  | route path strings to registered handlers |
|  [02]   | `LocalHandler`  | local I/O     | file-system read/write                    |
|  [03]   | `S3Handler`     | S3 I/O        | AWS S3 notebook read/write                |
|  [04]   | `GCSHandler`    | GCS I/O       | Google Cloud Storage read/write           |
|  [05]   | `ABSHandler`    | ABS I/O       | Azure Blob Storage read/write             |
|  [06]   | `ADLHandler`    | ADLS I/O      | Azure Data Lake Storage read/write        |
|  [07]   | `HttpHandler`   | HTTP I/O      | HTTP URL notebook fetch                   |
|  [08]   | `StreamHandler` | stream I/O    | file-like stream read/write               |
|  [09]   | `GithubHandler` | GitHub I/O    | GitHub raw URL read                       |

[PUBLIC_TYPE_SCOPE]: parameter translator family
- rail: notebook — `papermill.translators`

| [INDEX] | [SYMBOL]               | [TYPE_FAMILY]       | [CAPABILITY]                                         |
| :-----: | :--------------------- | :------------------ | :--------------------------------------------------- |
|  [01]   | `Translator`           | translator base     | abstract parameter serialization contract            |
|  [02]   | `PapermillTranslators` | translator registry | maps kernel/language names to `Translator` instances |
|  [03]   | `PythonTranslator`     | Python              | Python parameter cell code generation                |
|  [04]   | `RTranslator`          | R                   | R parameter cell code generation                     |
|  [05]   | `JuliaTranslator`      | Julia               | Julia parameter cell code generation                 |
|  [06]   | `ScalaTranslator`      | Scala               | Scala parameter cell code generation                 |
|  [07]   | `BashTranslator`       | Bash                | Bash parameter cell code generation                  |
|  [08]   | `CSharpTranslator`     | C#                  | C# parameter cell code generation                    |
|  [09]   | `FSharpTranslator`     | F#                  | F# parameter cell code generation                    |

[PUBLIC_TYPE_SCOPE]: exception family
- rail: notebook — `papermill.exceptions`

| [INDEX] | [SYMBOL]                             | [TYPE_FAMILY]   | [CAPABILITY]                                      |
| :-----: | :----------------------------------- | :-------------- | :------------------------------------------------ |
|  [01]   | `PapermillException`                 | base fault      | base for all papermill errors                     |
|  [02]   | `PapermillExecutionError`            | execution fault | notebook cell raised an unhandled exception       |
|  [03]   | `PapermillMissingParameterException` | param fault     | required parameter absent during parameterization |
|  [04]   | `PapermillRateLimitException`        | I/O rate fault  | cloud I/O handler rate limit exceeded             |

[PUBLIC_TYPE_SCOPE]: model family
- rail: notebook — `papermill.models`

| [INDEX] | [SYMBOL]    | [TYPE_FAMILY] | [CAPABILITY]                                                           |
| :-----: | :---------- | :------------ | :--------------------------------------------------------------------- |
|  [01]   | `Parameter` | named tuple   | `name`, `inferred_type_name`, `default`, `help` fields from inspection |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: notebook execution and inspection
- rail: notebook — `papermill`

| [INDEX] | [SURFACE]                                                                                                                                    | [ENTRY_FAMILY] | [CAPABILITY]                                     |
| :-----: | :------------------------------------------------------------------------------------------------------------------------------------------- | :------------- | :----------------------------------------------- |
|  [01]   | `execute_notebook(input_path, output_path, parameters, engine_name, request_save_on_cell_execute, prepare_only, kernel_name, language, ...)` | execute        | parameterize and execute a notebook end-to-end   |
|  [02]   | `inspect_notebook(notebook_path, parameters)`                                                                                                | inspect        | extract declared parameters without executing    |
|  [03]   | `parameterize_notebook(nb, parameters, report_mode, comment, kernel_name, language, engine_name)`                                            | parameterize   | inject a parameters cell into a `NotebookNode`   |
|  [04]   | `add_builtin_parameters(parameters)`                                                                                                         | param augment  | merge papermill built-in parameters into a dict  |
|  [05]   | `translate_parameters(kernel_name, language, parameters, comment)`                                                                           | translate      | serialize parameter dict to kernel-specific code |

[ENTRYPOINT_SCOPE]: I/O operations
- rail: notebook — `papermill.iorw`

| [INDEX] | [SURFACE]                                  | [ENTRY_FAMILY]   | [CAPABILITY]                                  |
| :-----: | :----------------------------------------- | :--------------- | :-------------------------------------------- |
|  [01]   | `load_notebook_node(notebook_path)`        | read             | read a notebook from any registered path      |
|  [02]   | `write_ipynb(nb, path)`                    | write            | write a `NotebookNode` to any registered path |
|  [03]   | `list_notebook_files(path)`                | list             | list notebook files under a directory path    |
|  [04]   | `PapermillIO.register(scheme, handler)`    | handler register | register a custom I/O handler for a scheme    |
|  [05]   | `PapermillIO.read(path, extensions)`       | registry read    | read via routing to the matching handler      |
|  [06]   | `PapermillIO.write(buf, path, extensions)` | registry write   | write via routing to the matching handler     |

[ENTRYPOINT_SCOPE]: execution manager lifecycle
- rail: notebook — `papermill.engines.NotebookExecutionManager`

| [INDEX] | [SURFACE]                                    | [ENTRY_FAMILY]     | [CAPABILITY]                                   |
| :-----: | :------------------------------------------- | :----------------- | :--------------------------------------------- |
|  [01]   | `notebook_start(**kwargs)`                   | lifecycle start    | record notebook start time; initialize pbar    |
|  [02]   | `notebook_complete(**kwargs)`                | lifecycle complete | mark notebook complete; finalize pbar and save |
|  [03]   | `cell_start(cell, cell_index, **kwargs)`     | cell start         | mark cell execution start; update pbar         |
|  [04]   | `cell_complete(cell, cell_index, **kwargs)`  | cell complete      | mark cell complete; autosave if configured     |
|  [05]   | `cell_exception(cell, cell_index, **kwargs)` | cell fault         | record cell-level exception for the manager    |
|  [06]   | `save(**kwargs)`                             | save               | write current notebook state to output path    |

## [04]-[IMPLEMENTATION_LAW]

[NOTEBOOK_TOPOLOGY]:
- primary axis: `execute_notebook(input_path, output_path, parameters={...})` is the single call for parameterized runs; `prepare_only=True` skips execution and writes the parameterized notebook only
- engine axis: `engine_name=None` defaults to `NBClientEngine` (nbclient-backed); custom engines register via `papermill_engines.register`
- I/O axis: `input_path` and `output_path` are routed through `papermill_io`; scheme prefix selects the handler (e.g., `s3://`, `gs://`, `abfs://`)
- parameter axis: `parameters` dict is translated to kernel-specific code by `PapermillTranslators`; tagged cells (`parameters` tag) mark the injection point
- inspection axis: `inspect_notebook(path)` returns a dict of `Parameter` named tuples keyed by name; no kernel is started

[LOCAL_ADMISSION]:
- `execute_notebook` returns the mutated `NotebookNode`; the output notebook is also written to `output_path` throughout execution.
- `progress_bar=True` emits a tqdm cell-level progress bar to stdout; set `progress_bar=False` in headless/log pipelines.
- `log_output=True` streams cell stdout/stderr to the logger during execution; `stdout_file` and `stderr_file` capture them independently.
- `cwd` sets the kernel working directory; when absent the kernel inherits the process working directory.
- Custom I/O handlers must implement `read`, `write`, `listdir`, and `pretty_path`; register before calling `execute_notebook`.

[RAIL_LAW]:
- Package: `papermill`
- Owns: parameterized notebook execution, parameter injection, kernel-language translation, pluggable notebook I/O, and execution lifecycle management
- Accept: any path scheme with a registered handler; parameter dicts with Python-serializable values
- Reject: hand-rolled parameter injection outside `parameterize_notebook`; custom execution loops that duplicate `execute_notebook`

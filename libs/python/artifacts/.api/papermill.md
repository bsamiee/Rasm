# [PY_ARTIFACTS_API_PAPERMILL]

`papermill` supplies parameterized notebook execution for the artifacts notebook rail: `execute_notebook` is the single end-to-end entrypoint; `inspect_notebook` extracts declared parameters; `parameterize_notebook` injects a parameters cell; `PapermillIO` owns pluggable notebook I/O across local, S3, GCS, ABS, ADLS, and HTTP paths; `Translator` and `PapermillTranslators` own parameter serialization across Python, R, Julia, Scala, Bash, and other kernels.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `papermill`
- package: `papermill`
- import: `papermill`
- owner: `artifacts`
- rail: notebook
- installed: `2.7.0` reflected via reflection on cp315 (Python 3.15)
- license: BSD-3-Clause; pure Python (`papermill-2.7.0-py3-none-any.whl`); core runtime deps `click`/`nbclient`/`nbformat`/`tenacity`/`requests`/`tqdm`/`ansicolors`; cloud handlers are extras (`papermill[s3]`/`[gcs]`/`[azure]`/`[hdfs]`/`[github]`) that lazy-import `boto3`/`gcsfs`/`azure-*`/`pyarrow`/`PyGithub` and raise `PapermillOptionalDependencyException` when the extra is absent; no compiled extension, installs clean on cp315
- entry points: `papermill` CLI (`click`-backed; executes via the `nbclient` engine, not nbconvert)
- capability: parameterized notebook execution, parameter injection, kernel-language translation (Python/R/Julia/Scala/Bash/Matlab/.NET-C#/.NET-F#/.NET-PowerShell), pluggable notebook I/O (local/S3/GCS/ABS/ADLS/HDFS/HTTP/GitHub/stream/in-memory), execution lifecycle management with progress bar and autosave, and `tenacity`-backed retry on cloud rate limits

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: execution and engine family
- rail: notebook — `papermill.engines`

`Engine.execute_notebook` wraps the notebook in a `NotebookExecutionManager` and delegates the real run to `execute_managed_notebook`; `PapermillEngines.execute_notebook_with_engine(engine_name, nb, kernel_name, **kwargs)` is the registry dispatch; `register(name, engine)`/`register_entry_points()`/`get_engine(name=None)` manage the engine table (`engine_name=None` -> default `NBClientEngine`).

| [INDEX] | [SYMBOL]                   | [TYPE_FAMILY]     | [CAPABILITY]                                                |
| :-----: | :------------------------- | :---------------- | :---------------------------------------------------------- |
|  [01]   | `Engine`                   | engine base       | `execute_notebook` (wraps in manager) + `execute_managed_notebook` contract |
|  [02]   | `NBClientEngine`           | nbclient engine   | executes via `papermill.clientwrap.PapermillNotebookClient` over `nbclient`; default engine |
|  [03]   | `PapermillEngines`         | engine registry   | `register`/`register_entry_points`/`get_engine`/`execute_notebook_with_engine` |
|  [04]   | `NotebookExecutionManager` | execution tracker | lifecycle callbacks; tqdm pbar; autosave; cell timing; `PENDING`/`RUNNING`/`COMPLETED`/`FAILED` cell states |

[PUBLIC_TYPE_SCOPE]: I/O handler family
- rail: notebook — `papermill.iorw`

Every handler conforms structurally to the four-method contract `read(path)`/`write(buf, path)`/`listdir(path)`/`pretty_path(path)`; the registry dispatches by scheme prefix. There is one handler protocol, never a parallel per-scheme reader/writer split.

| [INDEX] | [SYMBOL]              | [TYPE_FAMILY]   | [CAPABILITY]                                                  |
| :-----: | :-------------------- | :-------------- | :----------------------------------------------------------- |
|  [01]   | `PapermillIO`         | I/O registry    | route path strings to registered handlers by scheme prefix   |
|  [02]   | `LocalHandler`        | local I/O       | file-system read/write (also exposes `cwd` for the kernel)   |
|  [03]   | `S3Handler`           | S3 I/O          | AWS S3 notebook read/write (`s3://`; `boto3` extra)           |
|  [04]   | `GCSHandler`          | GCS I/O         | Google Cloud Storage read/write (`gs://`; `gcsfs` extra)     |
|  [05]   | `ABSHandler`          | ABS I/O         | Azure Blob Storage read/write (`abfs://`; `azure` extra)     |
|  [06]   | `ADLHandler`          | ADLS I/O        | Azure Data Lake Storage read/write (legacy `adl://`)         |
|  [07]   | `HDFSHandler`         | HDFS I/O        | Hadoop FS read/write (`hdfs://`; `pyarrow` extra)            |
|  [08]   | `HttpHandler`         | HTTP I/O        | HTTP/HTTPS URL notebook fetch                                |
|  [09]   | `GithubHandler`       | GitHub I/O      | GitHub raw URL read                                          |
|  [10]   | `StreamHandler`       | stream I/O      | file-like stream read/write                                  |
|  [11]   | `NotebookNodeHandler` | in-memory I/O   | route an in-memory `NotebookNode` as input without a path    |
|  [12]   | `NoIOHandler`         | null I/O        | discard-output handler for execute-only-no-write runs        |

[PUBLIC_TYPE_SCOPE]: parameter translator family
- rail: notebook — `papermill.translators`

`Translator` is a base whose `codify(parameters, comment)` classmethod folds the parameter dict into a kernel-language parameters cell via the `translate`/`assign`/`comment` and per-type `translate_int`/`translate_float`/`translate_str`/`translate_bool`/`translate_dict`/`translate_list`/`translate_none`/`translate_escaped_str`/`translate_raw_str` rows; `inspect` reads declared parameters back. Registered keys: `python`, `R`, `scala`, `julia`, `matlab`, `.net-csharp`, `.net-fsharp`, `.net-powershell`, `bash`, plus the `pysparkkernel`/`sparkkernel`/`sparkrkernel` aliases.

| [INDEX] | [SYMBOL]                | [TYPE_FAMILY]       | [CAPABILITY]                                         |
| :-----: | :---------------------- | :------------------ | :--------------------------------------------------- |
|  [01]   | `Translator`            | translator base     | `codify`/`assign`/`comment` + per-type `translate_*` parameter serialization contract |
|  [02]   | `PapermillTranslators`  | translator registry | `find_translator(kernel_name, language)` -> `Translator`; `register(language, translator)` |
|  [03]   | `PythonTranslator`      | Python              | Python parameter cell code generation                |
|  [04]   | `RTranslator`           | R                   | R parameter cell code generation                     |
|  [05]   | `JuliaTranslator`       | Julia               | Julia parameter cell code generation                 |
|  [06]   | `ScalaTranslator`       | Scala               | Scala parameter cell code generation                 |
|  [07]   | `BashTranslator`        | Bash                | Bash parameter cell code generation                  |
|  [08]   | `MatlabTranslator`      | Matlab              | Matlab parameter cell code generation                |
|  [09]   | `CSharpTranslator`      | .NET C#             | C# parameter cell code generation (`.net-csharp`)    |
|  [10]   | `FSharpTranslator`      | .NET F#             | F# parameter cell code generation (`.net-fsharp`)    |
|  [11]   | `PowershellTranslator`  | .NET PowerShell     | PowerShell parameter cell code generation (`.net-powershell`) |

[PUBLIC_TYPE_SCOPE]: exception family
- rail: notebook — `papermill.exceptions`

`PapermillExecutionError` is the typed execution receipt: it carries `(cell_index, exec_count, source, ename, evalue, traceback)` from the failing cell, and the error output is already embedded in the written notebook — the fault is structured, never a bare string.

| [INDEX] | [SYMBOL]                               | [TYPE_FAMILY]      | [CAPABILITY]                                                  |
| :-----: | :------------------------------------- | :----------------- | :----------------------------------------------------------- |
|  [01]   | `PapermillException`                   | base fault         | base for all papermill errors                                |
|  [02]   | `PapermillExecutionError`              | execution fault    | typed cell fault: `cell_index`/`exec_count`/`source`/`ename`/`evalue`/`traceback` |
|  [03]   | `PapermillMissingParameterException`   | param fault        | required parameter absent during parameterization            |
|  [04]   | `PapermillRateLimitException`          | I/O rate fault     | cloud I/O handler rate limit exceeded (drives `tenacity` retry) |
|  [05]   | `PapermillOptionalDependencyException` | extra-missing fault| a cloud handler's extra (`boto3`/`gcsfs`/`azure`/`pyarrow`) is not installed |
|  [06]   | `AwsError`                             | AWS fault          | underlying AWS/S3 transport error surfaced from the handler  |
|  [07]   | `PapermillWarning`                     | warning base       | base for papermill warnings                                  |
|  [08]   | `PapermillParameterOverwriteWarning`   | param warning      | a passed parameter overwrites a built-in/declared default    |

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
|  [07]   | `PapermillIO.get_handler(path, extensions)`| handler resolve  | resolve the handler that would serve a path   |
|  [08]   | `PapermillIO.listdir(path)`                | registry list    | list notebook files via the matching handler  |
|  [09]   | `PapermillIO.register_entry_points()`      | entry-point load | load handlers declared by installed extras     |
|  [10]   | `PapermillIO.reset()`                      | registry reset   | clear and re-seed the default handler table    |

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
- import: `import papermill` (or targeted `from papermill import execute_notebook, inspect_notebook`) at boundary scope only; module-level import is banned by the manifest import policy.
- primary axis: `execute_notebook(input_path, output_path, parameters={...})` is the single call for parameterized runs; `prepare_only=True` skips execution and writes the parameterized notebook only; `report_mode=True` hides the injected parameters cell in the output
- engine axis: `engine_name=None` defaults to `NBClientEngine` (built on `papermill.clientwrap.PapermillNotebookClient` over `nbclient.NotebookClient`); custom engines register via `papermill_engines.register(name, engine)` or entry points; `**engine_kwargs` (e.g. `timeout`, `start_timeout`, `kernel_name`) pass straight through to the underlying `nbclient` client
- I/O axis: `input_path` and `output_path` are routed through `papermill_io`; scheme prefix selects the handler (`s3://`, `gs://`, `abfs://`, `adl://`, `hdfs://`, `http(s)://`, `file://`/bare path); cloud handlers lazy-import their extra and raise `PapermillOptionalDependencyException` when absent
- retry axis: cloud reads/writes are wrapped in a `tenacity` `retry(retry_if_exception_type, stop_after_attempt, wait_exponential)` policy so a `PapermillRateLimitException` triggers exponential backoff rather than failing the run — this is papermill's own retry, distinct from any outer `stamina` boundary the rail may add
- parameter axis: `parameters` dict is translated to kernel-specific code by `PapermillTranslators.find_translator(kernel_name, language).codify(...)`; the cell tagged `parameters` marks the injection point; `add_builtin_parameters` merges papermill's built-ins; a passed value overwriting a default emits `PapermillParameterOverwriteWarning`
- inspection axis: `inspect_notebook(path)` returns a dict of `Parameter` named tuples keyed by name via the translator's `inspect` classmethod; no kernel is started
- fault axis: a failing cell raises `PapermillExecutionError` carrying `cell_index`/`exec_count`/`source`/`ename`/`evalue`/`traceback`, with the error output already embedded in the written notebook — the egress maps this typed receipt onto the rail's fault channel rather than re-parsing a traceback string

[LOCAL_ADMISSION]:
- `execute_notebook` returns the mutated `nbformat.NotebookNode`; the output notebook is also written to `output_path` throughout execution (autosave is driven by `request_save_on_cell_execute=True`).
- `progress_bar=True` emits a tqdm cell-level progress bar to stdout; set `progress_bar=False` in headless/log pipelines.
- `log_output=True` streams cell stdout/stderr to the logger during execution; `stdout_file` and `stderr_file` capture them independently.
- `cwd` sets the kernel working directory; when absent the kernel inherits the process working directory.
- Custom I/O handlers must implement `read`, `write`, `listdir`, and `pretty_path`; register via `papermill_io.register(scheme, handler)` before calling `execute_notebook`. `NotebookNodeHandler` routes an in-memory `NotebookNode` with no path; `NoIOHandler` discards output for execute-only runs.
- papermill consumes `nbformat`/`nbclient` as the notebook model and kernel runtime: `inspect_notebook`/`load_notebook_node` return the `nbformat` `NotebookNode`, and the default engine delegates the kernel lifecycle to `nbclient` — papermill never re-implements the kernel protocol the sibling owner holds.

[RAIL_LAW]:
- Package: `papermill`
- Owns: parameterized notebook execution, parameter injection, kernel-language translation across Python/R/Julia/Scala/Bash/Matlab/.NET, pluggable notebook I/O (local/S3/GCS/ABS/ADLS/HDFS/HTTP/GitHub/stream/in-memory), `tenacity`-backed cloud retry, and execution lifecycle management with progress/autosave/typed cell faults
- Accept: any path scheme with a registered handler; parameter dicts with kernel-serializable values; `NotebookNode` from `nbformat` via `NotebookNodeHandler`
- Reject: hand-rolled parameter injection outside `parameterize_notebook`/`Translator.codify`; custom execution loops that duplicate `execute_notebook` or re-implement the `nbclient` kernel protocol; a hand-rolled cloud-IO retry where `tenacity` already backs off on `PapermillRateLimitException`; re-parsing a traceback string where `PapermillExecutionError` carries the typed cell fault; a per-scheme reader/writer split where the four-method handler protocol owns routing

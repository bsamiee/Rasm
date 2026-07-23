# [PY_ARTIFACTS_API_JUPYTEXT]

`jupytext` owns notebook<->text conversion for the artifacts reports rail: a polymorphic `read`/`reads`/`write`/`writes` quartet exchanging an `nbformat.NotebookNode` across `.ipynb` and the text grammars, timestamp-ordered paired-file synchronization, executed-output-into-text merge, and round-trip equivalence assertion. It is the text-pairing layer beneath the `nbclient`/`papermill` execution rail and the `nbconvert` export rail, consuming `nbformat` for the node schema and `markdown-it-py` for the MyST parse.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `jupytext`
- package: `jupytext` (MIT)
- module: `jupytext`
- rail: reports
- deps: `markdown-it-py` + `mdit-py-plugins` (MyST parse), `nbformat` (notebook-node schema, `.ipynb` round-trip), `pyyaml` (embedded YAML header), `packaging` (text-format version compare)
- entry points: console scripts `jupytext` (conversion) and `jupytext-config` (contents-manager config); jupyter-server contents-manager plugins

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: conversion exchange and pairing roots

`read`/`reads`/`write`/`writes` exchange an `nbformat.NotebookNode`, never a jupytext-owned type; `fmt` is a short string (`py:percent`) or a long-form dict (`{"extension": ".py", "format_name": "percent"}`). `config.JupytextConfiguration` is the traitlets policy carrying paired formats, metadata filters, and cell-marker conventions, threaded through `config=` and resolved by `config.load_jupytext_config`. Each `[SYMBOL]` cell carries its import path: a bare name exports at top level, a dotted prefix is submodule-qualified.

| [INDEX] | [SYMBOL]                                  | [TYPE_FAMILY]    | [CAPABILITY]                                                            |
| :-----: | :---------------------------------------- | :--------------- | :---------------------------------------------------------------------- |
|  [01]   | `config.JupytextConfiguration`            | policy object    | traitlets `Configurable` over paired/metadata/marker/magic traits       |
|  [02]   | `NOTEBOOK_EXTENSIONS`                     | constant         | accepted notebook/text extensions (`.ipynb`/`.md`/`.py`/`.R`/`.jl`/...) |
|  [03]   | `formats.JUPYTEXT_FORMATS`                | registry         | `list[NotebookFormatDescription]` the `fmt` selector resolves over      |
|  [04]   | `formats.NotebookFormatDescription`       | registry row     | one grammar's reader/exporter row the `fmt` selector resolves to        |
|  [05]   | `formats.MYST_FORMAT_NAME`                | constant         | the `myst` format-name literal                                          |
|  [06]   | `formats.EXTENSION_PREFIXES`              | constant         | the `auto:`/`light:`-style fmt prefixes                                 |
|  [07]   | `formats.FORMATS_WITH_NO_CELL_METADATA`   | constant         | the cell-metadata-free format set                                       |
|  [08]   | `pairs.NotebookFile`                      | named tuple      | `(path, fmt, timestamp)` — one member of a paired-notebook group        |
|  [09]   | `jupytext.jupytext.TextNotebookConverter` | converter        | text<->`NotebookNode` engine `read`/`write` route to (submodule)        |
|  [10]   | `TextFileContentsManager`                 | contents manager | jupyter-server sync contents manager for paired files                   |
|  [11]   | `AsyncTextFileContentsManager`            | contents manager | jupyter-server async contents manager for paired files                  |

[CONFIG_TRAITS]: `JupytextConfiguration` carries `formats` `default_jupytext_formats` `preferred_jupytext_formats_read` `preferred_jupytext_formats_save` `notebook_extensions` `notebook_metadata_filter` `cell_metadata_filter` `root_level_metadata_filter` `hide_notebook_metadata` `root_level_metadata_as_raw_cell` `cell_markers` `default_cell_markers` `split_at_heading` `doxygen_equation_markers` `comment_magics` `custom_cell_magics` `custom_language_magics` `outdated_text_notebook_margin`.
[FORMAT_FIELDS]: each `NotebookFormatDescription` carries `format_name` `extension` `header_prefix` `header_suffix` `cell_reader_class` `cell_exporter_class` `current_version_number` `min_readable_version_number`.

[PUBLIC_TYPE_SCOPE]: faults

`paired_paths.InconsistentPath` heads a path-fault family: `InconsistentExtension` `InconsistentPrefix` `InconsistentSuffix` `InconsistentPrefixDirectory` `NonNotebookExtension`.

| [INDEX] | [SYMBOL]                                        | [TYPE_FAMILY] | [CAPABILITY]                                          |
| :-----: | :---------------------------------------------- | :------------ | :---------------------------------------------------- |
|  [01]   | `formats.JupytextFormatError`                   | format fault  | unknown/mismatched format name or extension           |
|  [02]   | `config.JupytextConfigurationError`             | config fault  | invalid `jupytext.toml`/`pyproject` config            |
|  [03]   | `pairs.PairedFilesDiffer`                       | sync fault    | a paired group's members carry incompatible content   |
|  [04]   | `paired_paths.InconsistentPath`                 | path fault    | a candidate path breaks the configured paired pattern |
|  [05]   | `jupytext.jupytext.NotSupportedNBFormatVersion` | version fault | the text-format version is newer than jupytext reads  |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: read and write conversion

`read`/`write` take a file name or object, `reads`/`writes` a string. `fmt` is optional on `read`/`reads` (auto-detected) and on `write` to a named file (extension-derived), mandatory on `writes` and on `write` to a file object; `as_version`/`version` default to `nbformat.NO_CONVERT`, so no implicit schema upgrade.

| [INDEX] | [SURFACE]                                                                      | [CAPABILITY]                                         |
| :-----: | :----------------------------------------------------------------------------- | :--------------------------------------------------- |
|  [01]   | `read(fp, as_version=nbformat.NO_CONVERT, fmt=None, config=None, **kwargs)`    | read a notebook from a file name or file object      |
|  [02]   | `reads(text, fmt=None, as_version=nbformat.NO_CONVERT, config=None, **kwargs)` | read a notebook from a string                        |
|  [03]   | `write(nb, fp, version=nbformat.NO_CONVERT, fmt=None, config=None, **kwargs)`  | write a notebook to a file name or file object       |
|  [04]   | `writes(notebook, fmt, version=nbformat.NO_CONVERT, config=None, **kwargs)`    | return the notebook's text representation as a `str` |

[ENTRYPOINT_SCOPE]: format detection, lookup, and normalization (`jupytext.formats`)

`guess_format` resolves `(format_name, options)` from content and extension, `divine_format` from content alone; `get_format_implementation` resolves the `NotebookFormatDescription` and raises `JupytextFormatError` on mismatch. `long_form_*`/`short_form_*` normalize between the string and dict `fmt`, so format identity stays data.

| [INDEX] | [SURFACE]                                                                    | [CAPABILITY]                                             |
| :-----: | :--------------------------------------------------------------------------- | :------------------------------------------------------- |
|  [01]   | `guess_format(text, ext)`                                                    | infer `(format_name, format_options)` from content + ext |
|  [02]   | `divine_format(text)`                                                        | infer the format from content alone                      |
|  [03]   | `get_format_implementation(ext, format_name=None)`                           | resolve the `NotebookFormatDescription` for an extension |
|  [04]   | `read_metadata(text, ext)`                                                   | extract the jupytext YAML header metadata                |
|  [05]   | `long_form_one_format(jupytext_format, metadata=None, update=None)`          | string `fmt` -> dict form                                |
|  [06]   | `short_form_one_format(jupytext_format)`                                     | dict form -> short string `fmt`                          |
|  [07]   | `long_form_multiple_formats(jupytext_formats, ...)`                          | comma-list `fmt` -> list of dict forms                   |
|  [08]   | `short_form_multiple_formats(jupytext_formats)`                              | list of dict forms -> comma-list string `fmt`            |
|  [09]   | `check_file_version(notebook, source_path, outputs_path)`                    | guard the jupytext text-format version                   |
|  [10]   | `is_myst_available()` / `is_pandoc_available()`                              | probe optional myst/pandoc markdown engines              |
|  [11]   | `validate_one_format(jupytext_format)`                                       | reject an unknown key/value in a long-form `fmt`         |
|  [12]   | `update_jupytext_formats_metadata(...)` / `rearrange_jupytext_metadata(...)` | update the entry / normalize a stale metadata layout     |
|  [13]   | `format_name_for_ext(...)` / `auto_ext_from_metadata(metadata)`              | default format / `auto` extension for a kernel language  |

[ENTRYPOINT_SCOPE]: paired-path derivation and synchronization (`jupytext.paired_paths`, `jupytext.pairs`, `jupytext.sync_pairs`)

Pairing is the reports rail's paired-editing core: `paired_paths` enumerates every path a notebook pairs to under a `formats` policy, `sync_pairs.read_pair`/`write_pair` read a paired group into the merged latest inputs and outputs and write back through one `write_one_file` callback, and `pairs.latest_inputs_and_outputs` resolves the inputs- and outputs-holders by timestamp. `read`/`writes` stays the single-file path.

| [INDEX] | [SURFACE]                                                           | [CAPABILITY]                                             |
| :-----: | :------------------------------------------------------------------ | :------------------------------------------------------- |
|  [01]   | `paired_paths(main_path, fmt, formats) -> list[tuple[str, dict]]`   | every `(path, fmt)` a notebook pairs to under the policy |
|  [02]   | `find_base_path_and_format(main_path, formats) -> tuple[str, dict]` | recover the base stem + format of a paired path          |
|  [03]   | `base_path(main_path, fmt, formats=None)` / `full_path(base, fmt)`  | strip a path to its paired stem / rebuild from stem+fmt  |
|  [04]   | `read_pair(inputs, outputs, read_one_file, must_match=False)`       | read a paired group into the latest inputs + outputs     |
|  [05]   | `write_pair(path, formats, write_one_file)`                         | write a notebook to every paired path via one callback   |
|  [06]   | `latest_inputs_and_outputs(path, fmt, formats, get_timestamp)`      | resolve inputs- + outputs-holder by timestamp            |

[ENTRYPOINT_SCOPE]: output merge and round-trip equivalence (`jupytext.combine`, `jupytext.compare`)

`combine_inputs_with_outputs` grafts executed outputs back onto a re-read text source by cell-content match, so a text-paired notebook regains its outputs without re-execution. `compare_notebooks`/`compare_cells` diff at the cell level into a `NotebookDifference` (inputs only unless `compare_outputs=True`) and `test_round_trip_conversion` asserts a text->node->text cycle lossless — the equivalence oracle a reports receipt cites.

| [INDEX] | [SURFACE]                                                         | [CAPABILITY]                                           |
| :-----: | :---------------------------------------------------------------- | :----------------------------------------------------- |
|  [01]   | `combine_inputs_with_outputs(nb_source, nb_outputs, fmt=None)`    | graft executed outputs back onto a re-read text source |
|  [02]   | `map_outputs_to_inputs(nb_source, nb_outputs)`                    | the cell-content alignment the merge folds over        |
|  [03]   | `compare_notebooks(notebook_actual, notebook_expected, fmt=None)` | cell-level diff -> `NotebookDifference`                |
|  [04]   | `test_round_trip_conversion(nb, fmt, args, ...)`                  | assert a text->node->text cycle is lossless            |

[ENTRYPOINT_SCOPE]: metadata-filter projection (`jupytext.metadata_filter`)

`metadata_filter_as_dict` parses the `+key,-key,all,-all` string into an include/exclude dict, `metadata_filter_as_string` inverts it, and `filter_metadata`/`restore_filtered_metadata` apply and reverse the resolved filter — keyed by the `notebook_metadata_filter`/`cell_metadata_filter` config trait, so a text form drops volatile keys without a per-call list.

| [INDEX] | [SURFACE]                                                 | [CAPABILITY]                                            |
| :-----: | :-------------------------------------------------------- | :------------------------------------------------------ |
|  [01]   | `metadata_filter_as_dict(metadata_config)`                | parse a filter string into an additive/subtractive dict |
|  [02]   | `metadata_filter_as_string(metadata_filter)`              | render a filter dict back to the `+/-key` string        |
|  [03]   | `filter_metadata(...)` / `restore_filtered_metadata(...)` | drop volatile metadata for the text form / restore it   |

[ENTRYPOINT_SCOPE]: direct grammar converters (`jupytext.jupytext`)

Per-grammar `*_to_notebook`/`notebook_to_*` functions are the converters `TextNotebookConverter` dispatches to; reached only to drive one grammar without the `fmt` selector.

| [INDEX] | [SURFACE]                                         | [CAPABILITY]                     |
| :-----: | :------------------------------------------------ | :------------------------------- |
|  [01]   | `md_to_notebook` / `notebook_to_md`               | markdown grammar conversion      |
|  [02]   | `myst_to_notebook` / `notebook_to_myst`           | MyST markdown grammar conversion |
|  [03]   | `qmd_to_notebook` / `notebook_to_qmd`             | Quarto `.qmd` grammar conversion |
|  [04]   | `marimo_py_to_notebook` / `notebook_to_marimo_py` | marimo `.py` grammar conversion  |

[ENTRYPOINT_SCOPE]: configuration and contents-manager builders

`load_jupytext_config` reads the nearest `jupytext.toml`/`jupytext.yml`/`pyproject.toml` into a `JupytextConfiguration`; the builder rows construct the jupyter-server contents managers over the host base manager passed through `*args`/`**kwargs`.

| [INDEX] | [SURFACE]                                                      | [CAPABILITY]                                      |
| :-----: | :------------------------------------------------------------- | :------------------------------------------------ |
|  [01]   | `load_jupytext_config(nb_file)`                                | resolve the `JupytextConfiguration` for a path    |
|  [02]   | `build_sync_jupytext_contents_manager_class(*args, **kwargs)`  | build a sync paired-file contents-manager class   |
|  [03]   | `build_async_jupytext_contents_manager_class(*args, **kwargs)` | build an async paired-file contents-manager class |
|  [04]   | `cli.jupytext(args=None, *, notary=None)`                      | programmatic CLI entry (conversion/pairing)       |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- conversion: one `read`/`reads` pair parses and one `write`/`writes` pair serializes; the file-vs-string split is the `fp`/`text` argument, and `read`/`write` delegate to `reads`/`writes` through one `TextNotebookConverter`.
- format: `fmt` is the single selector, short string or long dict, normalized through `long_form_*`/`short_form_*`; `.ipynb` round-trips through `nbformat` while text grammars route through the `JUPYTEXT_FORMATS` registry implementation.
- detection: `guess_format`/`divine_format` resolve `(format_name, options)` and `get_format_implementation` the registry row; format identity is data over `JUPYTEXT_FORMATS`.
- config: one `JupytextConfiguration` carries paired formats, metadata filters, and cell-marker conventions through `config=`; `metadata_filter_as_dict`/`filter_metadata` are the functional projection of those traits.
- exchange: the payload is the `nbformat.NotebookNode`; jupytext owns the text<->node mapping over it.
- sync: paired editing routes `paired_paths` -> `sync_pairs.read_pair`/`write_pair` with `pairs.latest_inputs_and_outputs` timestamp-resolving the holders, and `combine.combine_inputs_with_outputs` merges executed outputs onto a re-read text source, so an executed `.ipynb` and an edited `.py:percent` reconcile through the package's own timestamp-ordered algorithm.
- pairing: `TextFileContentsManager`/`AsyncTextFileContentsManager` own paired `.ipynb`<->text editing inside a jupyter-server host, deferred raisers when jupyter-server is absent.
- equivalence: `compare.compare_notebooks`/`test_round_trip_conversion` assert the text->node->text cycle lossless into a `NotebookDifference`, the equivalence fact a reports receipt cites.

[STACKING]:
- `expression`/runtime rail (`.api/expression`): every `read`/`reads`/`writes`/`combine_inputs_with_outputs` call is a `RuntimeRail` `Result` node mapping `JupytextFormatError`/`PairedFilesDiffer`/`NotSupportedNBFormatVersion` to a typed `Error`, `beartype`-checked at entry, spanned under `structlog`+OpenTelemetry with the resolved `format_name`/extension, folding the `ArtifactReceipt.Report` case via `core/receipt#RECEIPT`.
- `msgspec`/`pydantic` (`.api/msgspec`): the `fmt` selector is modeled as data over `JUPYTEXT_FORMATS`, normalized once through `long_form_one_format`/`short_form_one_format` to a `NotebookFormatDescription` row.
- `anyio` (`.api/anyio`): the sync arm runs its `read_one_file`/`write_one_file` callbacks inside the artifacts structured-concurrency boundary, fanning paired writes under one task group and cancellation scope.
- folder reports chain (`jupytext` -> `papermill`/`nbclient` -> `nbconvert`): `read`/`reads` produces the `NotebookNode` those owners execute and export, `combine_inputs_with_outputs` re-absorbs executed outputs, and `writes` serializes the text twin — jupytext stacks below execution/export and above `nbformat`.

[LOCAL_ADMISSION]:
- import `jupytext` at boundary scope only.
- each conversion contributes the `ArtifactReceipt.Report` case through the runtime `ReceiptContributor` port, carrying source extension, resolved `format_name`, format options, `nbformat` major/minor, and output byte length.
- jupytext owns notebook<->text conversion, format detection, paired-path sync, output merge, and pairing config over `nbformat`; `nbformat` owns the node schema, `markdown-it-py`/`mdit-py-plugins` the MyST parse, `nbclient`/`papermill` execution, `nbconvert` export, and jupyter-server the live host.

[RAIL_LAW]:
- Package: `jupytext`
- Owns: bidirectional notebook<->text conversion, content-based format detection, per-format lookup and `fmt` normalization, paired-path derivation with timestamp-ordered synchronization, executed-output-into-text merge, round-trip equivalence assertion, metadata-filter projection, traitlets pairing configuration, and jupyter-server paired-file contents managers
- Accept: notebook<->text pairing, format detection, paired-set sync, output merge, and pairing config feeding the `nbclient`/`papermill` execution and `nbconvert` export owners, each threaded through the universal `Result` rail under a `structlog`+OTel span and folded to `ArtifactReceipt.Report`
- Reject: a wrapper-rename of the `read`/`reads`/`write`/`writes` quartet; a hand-rolled percent/light/myst parser, `nbformat` round-trip, `.ipynb`<->text sync, or output-zip where `sync_pairs`/`combine_inputs_with_outputs` own it; a string-equality round-trip check where `compare_notebooks` is the oracle; a per-call metadata key list where `metadata_filter_as_dict` projects the config trait; a parallel notebook model or per-format reader/writer type where `fmt` is a call row; a jupytext-specific receipt shape; a notebook executor or HTML renderer jupytext does not own

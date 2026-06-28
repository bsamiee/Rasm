# [PY_ARTIFACTS_API_JUPYTEXT]

`jupytext` supplies the notebook<->text pairing surface for the artifacts reports rail: a polymorphic `read`/`reads`/`write`/`writes` quartet that exchanges an `nbformat.NotebookNode` across `.ipynb` and the text grammars (`py:percent`, `py:light`, `md`, `md:myst`, `Rmd`, `qmd`, `R`, `jl`, marimo, ...), a `config.JupytextConfiguration` traitlets policy object that drives paired-format selection and metadata filtering, the `jupytext.formats` registry that resolves and normalizes the `fmt` selector, and the jupyter-server contents managers for live paired editing. The package owner composes `read`, `writes`, `guess_format`, and `config.JupytextConfiguration` into the reports pairing path; it removes any hand-rolled cell-block parser because jupytext owns the percent/light/myst grammar end to end, and it never re-implements the `nbformat` round-trip jupytext already drives. jupytext is the text-source pairing layer feeding the `nbclient`/`papermill` execution rail and the `nbconvert` export rail, never a parallel notebook executor or HTML renderer.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `jupytext`
- package: `jupytext`
- import: `jupytext`
- owner: `artifacts`
- rail: reports
- installed: `1.19.4`
- license: MIT
- entry points: console scripts `jupytext` (conversion CLI) and `jupytext-config` (contents-manager config); jupyter-server contents-manager plugins; library use is import-only
- capability: bidirectional notebook<->text conversion across `.ipynb`/`md`/`markdown`/`Rmd`/`qmd`/`myst`/`mystnb` and the script extensions (`py:percent`, `py:light`, `R`, `jl`, marimo, sphinx-gallery, ...), content-based format auto-detection, per-format implementation lookup, traitlets-backed pairing configuration with metadata filtering, optional pandoc/myst markdown engines, and jupyter-server contents managers for paired editing

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: conversion exchange and pairing roots
- rail: reports

`read`/`reads`/`write`/`writes` exchange an `nbformat` notebook (`nbformat.NotebookNode`), not a jupytext-owned type; `fmt` is a short-form string (`py:percent`) or a long-form dict (`{"extension": ".py", "format_name": "percent"}`). `config.JupytextConfiguration` is the traitlets policy object that carries the paired formats, metadata filters, and cell-marker conventions; it is constructed once (or resolved by `config.load_jupytext_config`) and threaded through `config=`. The top-level `jupytext` re-exports only the verb quartet, `NOTEBOOK_EXTENSIONS`, `get_format_implementation`, `guess_format`, and the two contents-manager classes plus their builders (its `__all__`); `JupytextConfiguration` and `TextNotebookConverter` are submodule symbols (`jupytext.config` / `jupytext.jupytext`), never top-level. `NOTEBOOK_EXTENSIONS` enumerates every extension the conversion surface accepts. `TextFileContentsManager`/`AsyncTextFileContentsManager` are the jupyter-server contents managers built by the class builders; when jupyter-server is absent they evaluate to a deferred raiser.

| [INDEX] | [SYMBOL]                       | [TYPE_FAMILY]    | [RAIL]                                                  |
| :-----: | :----------------------------- | :--------------- | :----------------------------------------------------- |
|  [01]   | `config.JupytextConfiguration` | policy object    | traitlets config: `default_jupytext_formats`, `preferred_jupytext_formats_save`, `notebook_metadata_filter`, `cell_metadata_filter`, `cell_markers`, `comment_magics`, `hide_notebook_metadata`, `root_level_metadata_as_raw_cell` |
|  [02]   | `NOTEBOOK_EXTENSIONS`          | constant         | accepted notebook/text extensions list (`.ipynb`/`.md`/`.Rmd`/`.qmd`/`.py`/`.R`/`.jl`/`.myst`/...) |
|  [03]   | `formats.JUPYTEXT_FORMATS`     | registry         | list of `NotebookFormatDescription` rows (ext + format_name + reader/exporter) |
|  [04]   | `formats.MYST_FORMAT_NAME`     | constant         | the `myst` format-name literal                         |
|  [05]   | `jupytext.jupytext.TextNotebookConverter` | converter | text<->`NotebookNode` engine `read`/`write` route to (submodule symbol, not top-level) |
|  [06]   | `TextFileContentsManager`      | contents manager | jupyter-server sync contents manager for paired files (top-level `__all__`)  |
|  [07]   | `AsyncTextFileContentsManager` | contents manager | jupyter-server async contents manager for paired files (top-level `__all__`) |

[PUBLIC_TYPE_SCOPE]: faults
- rail: reports

| [INDEX] | [SYMBOL]                              | [PACKAGE_ROLE]   | [CAPABILITY]                                |
| :-----: | :------------------------------------ | :--------------- | :------------------------------------------ |
|  [01]   | `formats.JupytextFormatError`         | format fault     | unknown/mismatched format name or extension |
|  [02]   | `config.JupytextConfigurationError`   | config fault     | invalid `jupytext.toml`/`pyproject` config  |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: read and write conversion
- rail: reports

The conversion rows share the `fmt` and `config` policy; `read`/`write` operate on a file name or file object, `reads`/`writes` operate on a string. `fmt` is optional on `read`/`reads` (auto-detected via `guess_format`) and on `write` when `fp` is a file name (derived from the extension); `fmt` is mandatory on `writes` and on `write` to a file object. `as_version`/`version` default to `nbformat.NO_CONVERT` (no implicit schema upgrade).

| [INDEX] | [SURFACE] | [CALL_SHAPE]                                                                   | [CAPABILITY]                                             |
| :-----: | :-------- | :----------------------------------------------------------------------------- | :------------------------------------------------------- |
|  [01]   | `read`    | `read(fp, as_version=nbformat.NO_CONVERT, fmt=None, config=None, **kwargs)`    | read a notebook from a file name or file object          |
|  [02]   | `reads`   | `reads(text, fmt=None, as_version=nbformat.NO_CONVERT, config=None, **kwargs)` | read a notebook from a string                            |
|  [03]   | `write`   | `write(nb, fp, version=nbformat.NO_CONVERT, fmt=None, config=None, **kwargs)`  | write a notebook to a file name or file object           |
|  [04]   | `writes`  | `writes(notebook, fmt, version=nbformat.NO_CONVERT, config=None, **kwargs)`    | return the text representation of a notebook as a string |

[ENTRYPOINT_SCOPE]: format detection, lookup, and normalization (`jupytext.formats`)
- rail: reports

`guess_format` inspects content plus extension to resolve the format name and options; `divine_format` infers the format from content alone; `get_format_implementation` resolves the `NotebookFormatDescription` for an extension/name and raises `JupytextFormatError` on mismatch; `read_metadata` extracts the embedded jupytext YAML header. The long/short-form normalizers convert between the string `fmt` selector and the dict form — `fmt` identity is data, never a hard-coded branch.

| [INDEX] | [SURFACE]                       | [CALL_SHAPE]                                       | [CAPABILITY]                                              |
| :-----: | :------------------------------ | :------------------------------------------------- | :------------------------------------------------------- |
|  [01]   | `guess_format`                  | `guess_format(text, ext)`                          | infer `(format_name, format_options)` from content + ext |
|  [02]   | `divine_format`                 | `divine_format(text)`                              | infer the format from content alone                      |
|  [03]   | `get_format_implementation`     | `get_format_implementation(ext, format_name=None)` | resolve the `NotebookFormatDescription` for an extension |
|  [04]   | `read_metadata`                 | `read_metadata(text, ext)`                         | extract the jupytext YAML header metadata                |
|  [05]   | `long_form_one_format`          | `long_form_one_format(jupytext_format, metadata=None, update=None, auto_ext_requires_language_info=True)` | string `fmt` -> dict form |
|  [06]   | `short_form_one_format`         | `short_form_one_format(jupytext_format: dict)`     | dict form -> short string `fmt`                          |
|  [07]   | `long_form_multiple_formats`    | `long_form_multiple_formats(jupytext_formats: str, metadata=None, ...)` | comma-list `fmt` -> list of dict forms        |
|  [08]   | `short_form_multiple_formats`   | `short_form_multiple_formats(jupytext_formats: list)` | list of dict forms -> comma-list string `fmt`         |
|  [09]   | `check_file_version`            | `check_file_version(notebook, source_path, outputs_path)` | guard the jupytext text-format version          |
|  [10]   | `is_myst_available` / `is_pandoc_available` | `is_myst_available()` / `is_pandoc_available()` | probe optional myst/pandoc markdown engines     |

[ENTRYPOINT_SCOPE]: direct grammar converters (`jupytext.jupytext`)
- rail: reports

Per-grammar `*_to_notebook`/`notebook_to_*` functions are the lower-level converters `TextNotebookConverter` dispatches to; the canonical owner uses the `reads`/`writes` quartet and reaches these only when a grammar must be driven without the `fmt` selector.

| [INDEX] | [SURFACE]              | [CAPABILITY]                                           |
| :-----: | :--------------------- | :----------------------------------------------------- |
|  [01]   | `md_to_notebook` / `notebook_to_md`         | markdown grammar conversion       |
|  [02]   | `myst_to_notebook` / `notebook_to_myst`     | MyST markdown grammar conversion  |
|  [03]   | `qmd_to_notebook` / `notebook_to_qmd`       | Quarto `.qmd` grammar conversion  |
|  [04]   | `marimo_py_to_notebook` / `notebook_to_marimo_py` | marimo `.py` grammar conversion |

[ENTRYPOINT_SCOPE]: configuration and contents-manager builders
- rail: reports

`load_jupytext_config` reads the nearest `jupytext.toml`/`jupytext.yml`/`pyproject.toml` config into a `JupytextConfiguration`; the builder rows construct the jupyter-server contents-manager classes over the host base manager (passed through `*args`/`**kwargs`) and return a paired-file subclass.

| [INDEX] | [SURFACE]                                     | [CALL_SHAPE]                                                   | [CAPABILITY]                                      |
| :-----: | :-------------------------------------------- | :------------------------------------------------------------- | :------------------------------------------------ |
|  [01]   | `config.load_jupytext_config`                 | `load_jupytext_config(nb_file)`                                | resolve the `JupytextConfiguration` for a path    |
|  [02]   | `build_sync_jupytext_contents_manager_class`  | `build_sync_jupytext_contents_manager_class(*args, **kwargs)`  | build a sync paired-file contents-manager class   |
|  [03]   | `build_async_jupytext_contents_manager_class` | `build_async_jupytext_contents_manager_class(*args, **kwargs)` | build an async paired-file contents-manager class |
|  [04]   | `cli.jupytext`                                | `jupytext(args=None, *, notary=None)`                          | programmatic CLI entry (conversion/pairing)       |

## [04]-[IMPLEMENTATION_LAW]

[REPORTS_NOTEBOOK]:
- import: `import jupytext` at boundary scope only; module-level import is banned by the manifest import policy.
- conversion axis: one `read`/`reads` pair owns parsing and one `write`/`writes` pair owns serialization; the file-vs-string distinction is the `fp`/`text` argument, never a parallel reader type; `read`/`write` delegate to `reads`/`writes` after extension handling and route every text grammar through one `TextNotebookConverter`.
- format axis: `fmt` is the single format selector — short form (`py:percent`) or long-form dict — keyed per call and normalized through `long_form_*`/`short_form_*`; `.ipynb` round-trips through `nbformat` while text formats route through the registry implementation, never a per-format function family at the boundary.
- detection axis: `guess_format`/`divine_format` resolve `(format_name, format_options)` from content and extension; `get_format_implementation` resolves the `NotebookFormatDescription` row; format identity is data over `JUPYTEXT_FORMATS`, never a hard-coded branch.
- config axis: one `config.JupytextConfiguration` (via `config.load_jupytext_config`) carries paired formats, metadata filters, and cell-marker conventions threaded through `config=`; metadata filtering and paired-save policy are config traits, never per-call flags scattered across the owner.
- exchange axis: the notebook payload is the `nbformat.NotebookNode` from `nbformat.read`/`reads`; jupytext owns the text<->node mapping and never mints a parallel notebook model.
- pairing axis: `TextFileContentsManager`/`AsyncTextFileContentsManager`, built by the contents-manager class builders, own paired `.ipynb`<->text editing inside a jupyter-server host; absent jupyter-server they are deferred raisers, never a silent no-op.
- evidence: each conversion captures source extension, resolved `format_name`, format options, `nbformat` major/minor version, and output byte length as a reports receipt.
- boundary: jupytext owns notebook<->text conversion, content-based format detection, and pairing config over `nbformat` payloads; `nbformat` owns the notebook node schema and `markdown-it-py`/`mdit-py-plugins` own the myst markdown parse; headless notebook execution routes to `nbclient`/`papermill`; executed-notebook export to HTML/Markdown routes to `nbconvert`; live jupyter-server hosting stays outside this package.

[RAIL_LAW]:
- Package: `jupytext`
- Owns: bidirectional notebook<->text conversion, content-based format detection, per-format implementation lookup and `fmt` normalization, traitlets pairing configuration with metadata filtering, and jupyter-server paired-file contents managers
- Accept: notebook<->text read/write pairing, format detection, and pairing config feeding the reports execution (`nbclient`/`papermill`) and export (`nbconvert`) owners
- Reject: wrapper-renames of `read`/`reads`/`write`/`writes`; a hand-rolled percent/light/myst cell parser; a hand-rolled `nbformat` round-trip; a parallel notebook model jupytext does not mint; a parallel reader/writer type per format where `fmt` is a call row; a notebook executor or HTML renderer jupytext does not own

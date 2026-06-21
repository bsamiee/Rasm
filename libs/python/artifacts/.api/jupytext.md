# [PY_ARTIFACTS_API_JUPYTEXT]

`jupytext` supplies the notebook<->text pairing surface for the artifacts reports rail: a `read`/`reads` pair that parses a notebook from a file or string into an `nbformat` notebook, and a `write`/`writes` pair that serializes a notebook back to `.ipynb` or a text representation (`md`, `py:percent`, `myst`, `Rmd`, `qmd`, ...) selected by `fmt`. The package owner composes `read`, `writes`, and `guess_format` into the report pairing path; it removes any hand-rolled cell-block parser because jupytext owns the percent/light/myst grammar end to end, and it never re-implements the `nbformat` round-trip jupytext already drives.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `jupytext`
- package: `jupytext`
- import: `jupytext`
- owner: `artifacts`
- rail: reports
- installed: `1.19.3` reflected via `assay api` on cp315
- entry points: console scripts `jupytext` and `jupytext-config` (CLI); jupyter-server contents-manager plugins; library use is import-only
- capability: bidirectional notebook<->text conversion across `.ipynb`/`md`/`markdown`/`Rmd`/`qmd`/`myst` and the script extensions (`py:percent`, `py:light`, `R`, `jl`, ...), format auto-detection from content, per-format implementation lookup, and jupyter-server contents managers for paired editing

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: conversion and pairing roots
- rail: reports

`read`/`reads`/`write`/`writes` exchange an `nbformat` notebook (a `nbformat.NotebookNode`), not a jupytext-owned type; `fmt` is a short-form string (`py:percent`) or long-form dict. `NOTEBOOK_EXTENSIONS` enumerates the extensions the conversion surface accepts. `TextFileContentsManager`/`AsyncTextFileContentsManager` are the jupyter-server contents managers built by `build_sync_jupytext_contents_manager_class`/`build_async_jupytext_contents_manager_class`; when jupyter-server is absent they evaluate to a deferred raiser.

| [INDEX] | [SYMBOL]                       | [TYPE_FAMILY]    | [RAIL]                                                 |
| :-----: | :----------------------------- | :--------------- | :----------------------------------------------------- |
|  [01]   | `TextFileContentsManager`      | contents manager | jupyter-server sync contents manager for paired files  |
|  [02]   | `AsyncTextFileContentsManager` | contents manager | jupyter-server async contents manager for paired files |
|  [03]   | `NOTEBOOK_EXTENSIONS`          | constant         | accepted notebook/text extensions list                 |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: read and write functions
- rail: reports

The conversion rows share the `fmt` and `config` policy; `read`/`write` operate on a file name or file object, `reads`/`writes` operate on a string. `fmt` is optional on read (auto-detected via `guess_format`) and on `write` when `fp` is a file name (derived from the extension); `fmt` is mandatory on `writes` and on `write` to a file object.

| [INDEX] | [SURFACE] | [CALL_SHAPE]                                                                   | [CAPABILITY]                                             |
| :-----: | :-------- | :----------------------------------------------------------------------------- | :------------------------------------------------------- |
|  [01]   | `read`    | `read(fp, as_version=nbformat.NO_CONVERT, fmt=None, config=None, **kwargs)`    | read a notebook from a file name or file object          |
|  [02]   | `reads`   | `reads(text, fmt=None, as_version=nbformat.NO_CONVERT, config=None, **kwargs)` | read a notebook from a string                            |
|  [03]   | `write`   | `write(nb, fp, version=nbformat.NO_CONVERT, fmt=None, config=None, **kwargs)`  | write a notebook to a file name or file object           |
|  [04]   | `writes`  | `writes(notebook, fmt, version=nbformat.NO_CONVERT, config=None, **kwargs)`    | return the text representation of a notebook as a string |

[ENTRYPOINT_SCOPE]: format detection and lookup
- rail: reports

`guess_format` inspects content plus extension to resolve the format name and options; `get_format_implementation` resolves the format implementation for a given extension and optional name, raising `JupytextFormatError` on mismatch.

| [INDEX] | [SURFACE]                   | [CALL_SHAPE]                                       | [CAPABILITY]                                             |
| :-----: | :-------------------------- | :------------------------------------------------- | :------------------------------------------------------- |
|  [01]   | `guess_format`              | `guess_format(text, ext)`                          | infer `(format_name, format_options)` from content + ext |
|  [02]   | `get_format_implementation` | `get_format_implementation(ext, format_name=None)` | resolve the format implementation for an extension       |

[ENTRYPOINT_SCOPE]: contents-manager class builders
- rail: reports

The builder rows construct the jupyter-server contents-manager classes over the host base manager; they take the host manager class through `*args`/`**kwargs` and return a paired-file contents-manager subclass.

| [INDEX] | [SURFACE]                                     | [CALL_SHAPE]                                                   | [CAPABILITY]                                      |
| :-----: | :-------------------------------------------- | :------------------------------------------------------------- | :------------------------------------------------ |
|  [01]   | `build_sync_jupytext_contents_manager_class`  | `build_sync_jupytext_contents_manager_class(*args, **kwargs)`  | build a sync paired-file contents-manager class   |
|  [02]   | `build_async_jupytext_contents_manager_class` | `build_async_jupytext_contents_manager_class(*args, **kwargs)` | build an async paired-file contents-manager class |

## [04]-[IMPLEMENTATION_LAW]

[REPORTS_NOTEBOOK]:
- import: `import jupytext` at boundary scope only; module-level import is banned by the manifest import policy.
- conversion axis: one `read`/`reads` pair owns parsing and one `write`/`writes` pair owns serialization; the file-vs-string distinction is the `fp`/`text` argument, never a parallel reader type; `read`/`write` delegate to `reads`/`writes` after extension handling.
- format axis: `fmt` is the single format selector — short form (`py:percent`) or long-form dict — keyed per call; `.ipynb` round-trips through `nbformat` while text formats route through the `TextNotebookConverter`, never a per-format function family.
- detection axis: `guess_format` resolves `(format_name, format_options)` from content and extension; `get_format_implementation` resolves the implementation row; format identity is data, never a hard-coded branch.
- exchange axis: the notebook payload is the `nbformat.NotebookNode` from `nbformat.read`/`reads`; jupytext owns the text<->node mapping and never mints a parallel notebook model.
- pairing axis: `TextFileContentsManager`/`AsyncTextFileContentsManager`, built by the contents-manager class builders, own paired `.ipynb`<->text editing inside a jupyter-server host; absent jupyter-server they are deferred raisers, never a silent no-op.
- evidence: each conversion captures source extension, resolved `format_name`, format options, `nbformat` major/minor version, and output byte length as a reports receipt.
- boundary: jupytext owns notebook<->text conversion and format detection over `nbformat` payloads; `nbformat` owns the notebook node schema; report rendering and document assembly route to the document and visuals owners; live jupyter-server hosting stays outside this package.

[RAIL_LAW]:
- Package: `jupytext`
- Owns: bidirectional notebook<->text conversion, content-based format detection, per-format implementation lookup, and jupyter-server paired-file contents managers
- Accept: notebook<->text read/write pairing and format detection feeding the reports, document, and visuals owners
- Reject: wrapper-renames of `read`/`reads`/`write`/`writes`; a hand-rolled percent/light/myst cell parser; a hand-rolled `nbformat` round-trip; a parallel notebook model jupytext does not mint; a parallel reader/writer type per format where `fmt` is a call row

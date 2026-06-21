# [PY_ARTIFACTS_API_NBCONVERT]

`nbconvert` supplies the notebook-to-document exporter family for the artifacts report rail: a `get_exporter` registry that resolves an export-name into an `Exporter` class, the concrete `PDFExporter`/`WebPDFExporter`/`HTMLExporter`/`LatexExporter` and sibling exporters, and a single `export` entrypoint plus the `from_notebook_node`/`from_filename`/`from_file` conversion methods that turn a `NotebookNode` into rendered bytes with its resources dict. The package owner composes `get_exporter`, the exporter classes, and `from_notebook_node` into the `ReportOp.Render` path; it removes ad-hoc `subprocess` calls to the `jupyter-nbconvert` CLI because every export target is in-process, and it never re-implements the Jinja templating, preprocessor pipeline, or LaTeX/Chromium PDF assembly nbconvert already owns.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `nbconvert`
- package: `nbconvert`
- import: `nbconvert`
- owner: `artifacts`
- rail: report
- installed: `7.17.1` reflected via `assay api` on cp315
- entry points: console scripts `jupyter-nbconvert` (CLI conversion) and `jupyter-dejavu` (notebook diff); 14 `nbconvert.exporters` plugin rows (`asciidoc`, `custom`, `html`, `latex`, `markdown`, `notebook`, `pdf`, `python`, `qtpdf`, `qtpng`, `rst`, `script`, `slides`, `webpdf`)
- capability: resolve an export target by name, instantiate the matching `Exporter`, and convert a `NotebookNode`/file/stream to PDF/HTML/LaTeX/Markdown/RST/AsciiDoc/Python/script/slides/notebook output paired with a resources dict, driving the Jinja template, preprocessor, and PDF-assembly pipeline in-process

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: exporter classes and resolution failures
- rail: report

`Exporter` is the base that runs preprocessors and produces `(output, resources)`; `TemplateExporter` adds the Jinja template layer that the format exporters extend. `PDFExporter` assembles via LaTeX, `WebPDFExporter` via headless Chromium, `QtPDFExporter`/`QtPNGExporter` via a Qt screenshot. `ExporterNameError` is raised when `get_exporter` cannot resolve a name; `FilenameExtension` is the traitlets trait that types an exporter's output extension.

| [INDEX] | [SYMBOL]            | [TYPE_FAMILY] | [RAIL]                                              |
| :-----: | :------------------ | :------------ | :-------------------------------------------------- |
|  [01]   | `Exporter`          | exporter base | preprocessor-runner producing `(output, resources)` |
|  [02]   | `TemplateExporter`  | exporter base | Jinja-template exporter base for format exporters   |
|  [03]   | `HTMLExporter`      | exporter      | HTML document export                                |
|  [04]   | `PDFExporter`       | exporter      | PDF via LaTeX assembly                              |
|  [05]   | `WebPDFExporter`    | exporter      | PDF via headless Chromium                           |
|  [06]   | `LatexExporter`     | exporter      | LaTeX `.tex` document export                        |
|  [07]   | `MarkdownExporter`  | exporter      | Markdown `.md` document export                      |
|  [08]   | `RSTExporter`       | exporter      | reStructuredText document export                    |
|  [09]   | `ASCIIDocExporter`  | exporter      | AsciiDoc `.asciidoc` document export                |
|  [10]   | `SlidesExporter`    | exporter      | reveal.js HTML slides export                        |
|  [11]   | `NotebookExporter`  | exporter      | round-trip `.ipynb` notebook export                 |
|  [12]   | `PythonExporter`    | exporter      | Python `.py` source export                          |
|  [13]   | `ScriptExporter`    | exporter      | kernel-language script export                       |
|  [14]   | `QtPDFExporter`     | exporter      | PDF via Qt screenshot                               |
|  [15]   | `QtPNGExporter`     | exporter      | PNG via Qt screenshot                               |
|  [16]   | `FilenameExtension` | trait         | traitlets filename-extension trait                  |
|  [17]   | `ExporterNameError` | error         | unknown/unresolvable export-name failure            |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: module resolution and export functions
- rail: report

`get_exporter` resolves an export name (`pdf`, `webpdf`, `html`, `latex`, ...) or dotted import path to an `Exporter` class, raising `ExporterNameError` on miss and `ExporterDisabledError` when disabled. `export` instantiates the exporter and dispatches on the `nb` argument shape: a `NotebookNode` routes to `from_notebook_node`, a `str` to `from_filename`, anything else to `from_file`. `get_export_names` lists the enabled export targets.

| [INDEX] | [SURFACE]          | [CALL_SHAPE]                                          | [CAPABILITY]                                               |
| :-----: | :----------------- | :---------------------------------------------------- | :--------------------------------------------------------- |
|  [01]   | `get_exporter`     | `get_exporter(name, config=None)` -> `type[Exporter]` | resolve an export-name or import path to an exporter class |
|  [02]   | `export`           | `export(exporter, nb, **kw)` -> `tuple[str, dict]`    | instantiate and convert, dispatching on `nb` shape         |
|  [03]   | `get_export_names` | `get_export_names(config=None)` -> `list[str]`        | list enabled export targets                                |

[ENTRYPOINT_SCOPE]: `Exporter` construct and convert
- rail: report

The exporter constructor takes `config` plus traitlets `**kw`; the same constructor shape spans every concrete exporter. The three `from_*` methods each return `(NotebookNode-output, resources-dict)`; `from_notebook_node` is the in-memory path the campaign consumes, `from_filename`/`from_file` read from disk or a stream.

| [INDEX] | [SURFACE]                     | [CALL_SHAPE]                                                                                                                      | [CAPABILITY]                                                |
| :-----: | :---------------------------- | :-------------------------------------------------------------------------------------------------------------------------------- | :---------------------------------------------------------- |
|  [01]   | `Exporter`                    | `Exporter(config=None, **kw)`                                                                                                     | construct an exporter with config and traitlets kwargs      |
|  [02]   | `Exporter.from_notebook_node` | `from_notebook_node(nb: NotebookNode, resources: t.Any \| None = None, **kw: t.Any) -> tuple[NotebookNode, dict[str, t.Any]]`     | convert an in-memory notebook node to `(output, resources)` |
|  [03]   | `Exporter.from_filename`      | `from_filename(filename: str, resources: dict[str, t.Any] \| None = None, **kw: t.Any) -> tuple[NotebookNode, dict[str, t.Any]]`  | convert a notebook file path to `(output, resources)`       |
|  [04]   | `Exporter.from_file`          | `from_file(file_stream: t.Any, resources: dict[str, t.Any] \| None = None, **kw: t.Any) -> tuple[NotebookNode, dict[str, t.Any]]` | convert a notebook stream to `(output, resources)`          |

## [04]-[IMPLEMENTATION_LAW]

[REPORT_NOTEBOOK_EXPORT]:
- import: `import nbconvert` at boundary scope only; module-level import is banned by the manifest import policy.
- resolution axis: one `get_exporter` owns export-target resolution; `pdf`/`webpdf`/`html`/`latex`/`markdown`/`rst`/`asciidoc`/`slides`/`notebook`/`python`/`script`/`qtpdf`/`qtpng` are name rows on the `nbconvert.exporters` entrypoint registry, never a per-format selector function; a dotted import path resolves a third-party exporter through the same call.
- conversion axis: `export` is the single conversion surface that dispatches on the `nb` shape (`NotebookNode` -> `from_notebook_node`, `str` -> `from_filename`, else `from_file`); the campaign uses `from_notebook_node` for the in-memory notebook and reads `output` plus the `resources` dict, never a parallel per-format convert function.
- exporter axis: each concrete exporter is a `TemplateExporter`/`Exporter` subclass selected by name, never a hand-rolled renderer; `PDFExporter` assembles through LaTeX, `WebPDFExporter` through headless Chromium, `QtPDFExporter`/`QtPNGExporter` through a Qt screenshot, and the template/preprocessor/filter pipeline stays owned by nbconvert.
- config axis: exporter behavior (template name, preprocessors, execution, extraction) is traitlets config passed through `config=` or `**kw` at construction, never a parallel exporter subclass per option.
- evidence: each render captures the resolved export name, exporter class, output byte length, the `resources` keys (extracted outputs, output extension, metadata), and the resolved `FilenameExtension` as a report receipt.
- boundary: nbconvert owns notebook-to-document conversion and the Jinja template/preprocessor pipeline; `from_notebook_node` consumes a `nbformat.NotebookNode` from the upstream notebook owner; PDF output feeds the document owner and HTML output feeds the visuals owner; the `jupyter-nbconvert` CLI stays outside the in-process path.

[RAIL_LAW]:
- Package: `nbconvert`
- Owns: export-target resolution, the `Exporter`/`TemplateExporter` class family, and notebook-to-PDF/HTML/LaTeX/Markdown/RST/AsciiDoc/slides/script conversion with its `(output, resources)` contract
- Accept: notebook-node export feeding the report, document, and visuals owners
- Reject: wrapper-renames of `get_exporter`/`export`/`from_notebook_node`; a `subprocess` shell-out to the `jupyter-nbconvert` CLI where the in-process exporters convert directly; a parallel convert function per output format; a hand-rolled Jinja template or LaTeX/Chromium PDF assembler nbconvert already owns; a per-config exporter subclass where traitlets config is a constructor kwarg

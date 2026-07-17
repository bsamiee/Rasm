# [PY_ARTIFACTS_API_NBCONVERT]

`nbconvert` supplies the notebook-to-document exporter family for the artifacts report rail: a `get_exporter` registry that resolves an export-name into an `Exporter` class, the concrete `PDFExporter`/`WebPDFExporter`/`HTMLExporter`/`LatexExporter` and sibling exporters, and a single `export` entrypoint plus the `from_notebook_node`/`from_filename`/`from_file` conversion methods that turn a `NotebookNode` into rendered output paired with its resources dict. Package owner `document/report#REPORT` composes `get_exporter`, the exporter classes, and `from_notebook_node` into the `ReportKind.NOTEBOOK` arm of `COMPOSE_ARMS`; it excludes ad-hoc `subprocess` calls to `jupyter-nbconvert` and never reimplements Jinja, preprocessors, or LaTeX/Chromium assembly. Blocking render crosses the report owner's runtime `LanePolicy.offload` thread lane, `ExportPolicy` projects traits through class-keyed `Config` rows, `(output, resources)` partitions on `str`/`bytes` into semantic text or a content-addressed binary asset, and `ExporterNameError`/`ExporterDisabledError` close into `ReportFault.export` at admission.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `nbconvert`
- package: `nbconvert`
- import: `nbconvert`
- owner: `artifacts`
- rail: report
- installed: `7.17.1`
- license: BSD-3-Clause
- deps: `nbformat` (notebook model), `nbclient` (the `ExecutePreprocessor` execution wrapper), `jupyter_core`/`traitlets` (config), `jinja2` (template), `mistune` (Markdown), `bleach`/`defusedxml`/`tinycss2` (HTML sanitize), `beautifulsoup4`, `pygments`/`jupyterlab-pygments` (highlight), `pandocfilters`; `nbconvert[webpdf]` pulls `playwright` (headless Chromium), `nbconvert[qtpdf]`/`[qtpng]` pull `pyqtwebengine`
- entry points: console scripts `jupyter-nbconvert` (CLI conversion) and `jupyter-dejavu` (notebook diff); the `nbconvert.exporters` plugin registry — `get_export_names()` returns the 14 enabled rows (`asciidoc`, `custom`, `html`, `latex`, `markdown`, `notebook`, `pdf`, `python`, `qtpdf`, `qtpng`, `rst`, `script`, `slides`, `webpdf`), and `webpdf` resolves enabled but raises at construct time when the `[webpdf]` extra is absent
- capability: resolve an export target by name (or dotted import path), instantiate the matching `Exporter`, and convert a `NotebookNode`/file/stream to PDF/HTML/LaTeX/Markdown/RST/AsciiDoc/Python/script/slides/notebook output paired with a resources dict, driving the Jinja template, preprocessor chain, filter map, and LaTeX/Chromium PDF-assembly pipeline in-process; shape the conversion through traitlets `config=`/`**kw` (template selection, content exclusion, preprocessor enablement, Jinja filters); persist via the `writers` family

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: exporter classes and resolution failures
- rail: report

`Exporter` is the base that runs preprocessors and produces `(output, resources)` — the base alone returns `(NotebookNode, dict)`, while `TemplateExporter` (the layer the document exporters extend) renders through Jinja and returns `(str, dict)`. `PDFExporter` assembles via LaTeX, `WebPDFExporter` via headless Chromium, `QtPDFExporter`/`QtPNGExporter` via a Qt screenshot. `ExporterNameError` is raised when `get_exporter` cannot resolve a name and `ExporterDisabledError` when the name resolves but is disabled by config; both live in `nbconvert.exporters`. `FilenameExtension` is the traitlets trait typing an exporter's output extension; `ResourcesDict` is the dict subtype returned as the second half of every conversion tuple (auto-creates missing keys).

| [INDEX] | [SYMBOL]                | [TYPE_FAMILY]  | [RAIL]                                                         |
| :-----: | :---------------------- | :------------- | :------------------------------------------------------------- |
|  [01]   | `Exporter`              | exporter base  | preprocessor-runner producing `(NotebookNode, resources)`      |
|  [02]   | `TemplateExporter`      | exporter base  | Jinja-template exporter base; produces `(str, resources)`      |
|  [03]   | `HTMLExporter`          | exporter       | HTML document export                                           |
|  [04]   | `PDFExporter`           | exporter       | PDF via LaTeX assembly                                         |
|  [05]   | `WebPDFExporter`        | exporter       | PDF via headless Chromium                                      |
|  [06]   | `LatexExporter`         | exporter       | LaTeX `.tex` document export                                   |
|  [07]   | `MarkdownExporter`      | exporter       | Markdown `.md` document export                                 |
|  [08]   | `RSTExporter`           | exporter       | reStructuredText document export                               |
|  [09]   | `ASCIIDocExporter`      | exporter       | AsciiDoc `.asciidoc` document export                           |
|  [10]   | `SlidesExporter`        | exporter       | reveal.js HTML slides export                                   |
|  [11]   | `NotebookExporter`      | exporter       | round-trip `.ipynb` notebook export                            |
|  [12]   | `PythonExporter`        | exporter       | Python `.py` source export                                     |
|  [13]   | `ScriptExporter`        | exporter       | kernel-language script export                                  |
|  [14]   | `QtPDFExporter`         | exporter       | PDF via Qt screenshot                                          |
|  [15]   | `QtPNGExporter`         | exporter       | PNG via Qt screenshot                                          |
|  [16]   | `FilenameExtension`     | trait          | traitlets filename-extension trait                             |
|  [17]   | `ResourcesDict`         | result carrier | auto-vivifying dict returned as the second tuple element       |
|  [18]   | `ExporterNameError`     | error          | unknown/unresolvable export-name failure (from `get_exporter`) |
|  [19]   | `ExporterDisabledError` | error          | export-name resolves but is config-disabled                    |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: module resolution and export functions
- rail: report

`get_exporter` resolves an export name (`pdf`, `webpdf`, `html`, `latex`,...) or dotted import path to an `Exporter` class, raising `ExporterNameError` on miss and `ExporterDisabledError` when disabled. `export` instantiates the exporter and dispatches on the `nb` argument shape: a `NotebookNode` routes to `from_notebook_node`, a `str` to `from_filename`, anything else to `from_file`. `get_export_names` lists the enabled export targets.

| [INDEX] | [SURFACE]          | [CALL_SHAPE]                                          | [CAPABILITY]                                               |
| :-----: | :----------------- | :---------------------------------------------------- | :--------------------------------------------------------- |
|  [01]   | `get_exporter`     | `get_exporter(name, config=None)` -> `type[Exporter]` | resolve an export-name or import path to an exporter class |
|  [02]   | `export`           | `export(exporter, nb, **kw)` -> `tuple[str, dict]`    | instantiate and convert, dispatching on `nb` shape         |
|  [03]   | `get_export_names` | `get_export_names(config=None)` -> `list[str]`        | list enabled export targets                                |

[ENTRYPOINT_SCOPE]: `Exporter` construct and convert
- rail: report

The exporter constructor takes `config` plus traitlets `**kw`; the same constructor shape spans every concrete exporter. The three `from_*` methods share `(<source>, resources=None, **kw) -> tuple[str | NotebookNode, dict[str, Any]]`; the output type is `str` for every `TemplateExporter` subclass (HTML/LaTeX/Markdown/RST/slides/script) and `NotebookNode` for the base `Exporter`/`NotebookExporter` round-trip. `from_notebook_node` is the in-memory path the campaign consumes; `from_filename`/`from_file` read from disk or a stream. Surfaces are `Exporter.*` except `register_filter` on `TemplateExporter`.

| [INDEX] | [SURFACE]               | [CALL_SHAPE]                                         | [CAPABILITY]                                         |
| :-----: | :---------------------- | :--------------------------------------------------- | :--------------------------------------------------- |
|  [01]   | `Exporter`              | `Exporter(config=None, **kw)`                        | construct an exporter with config + traitlets kwargs |
|  [02]   | `from_notebook_node`    | `from_notebook_node(nb, resources, **kw)`            | convert an in-memory notebook node                   |
|  [03]   | `from_filename`         | `from_filename(filename, resources, **kw)`           | convert a notebook file path                         |
|  [04]   | `from_file`             | `from_file(file_stream, resources, **kw)`            | convert a notebook stream                            |
|  [05]   | `register_preprocessor` | `register_preprocessor(preprocessor, enabled=False)` | append a preprocessor (class/instance/dotted-name)   |
|  [06]   | `register_filter`       | `register_filter(name, jinja_filter)`                | register a Jinja filter callable for the template    |

[ENTRYPOINT_SCOPE]: preprocessor chain and output writers
- rail: report

The preprocessor chain mutates `(nb, resources)` before the template renders; nbconvert ships the chain as configurable `Preprocessor` subclasses rather than a hard-coded transform. `ExecutePreprocessor` is the nbconvert wrapper over `nbclient.NotebookClient` — the campaign executes through nbclient directly and leaves `enabled=False` (default) to avoid a double execution. The `writers` family is the persistence half: `FilesWriter` writes `output` plus every entry in `resources['outputs']` (extracted figures) to disk under a build directory, which is how `ExtractOutputPreprocessor`'s figure bytes reach the filesystem. Each `preprocessors.*` toggles on its `enabled` config trait unless a trait is named; `TagRemovePreprocessor` reads `remove_cell_tags`/`remove_input_tags`/`remove_all_outputs_tags`, `RegexRemovePreprocessor` reads `patterns`, and the writers take `write(output, resources, notebook_name)`.

| [INDEX] | [SURFACE]                                            | [CAPABILITY]                                                   |
| :-----: | :--------------------------------------------------- | :------------------------------------------------------------- |
|  [01]   | `ExecutePreprocessor`                                | run the notebook via `nbclient` before render (off by default) |
|  [02]   | `ExtractOutputPreprocessor`                          | pull display outputs (figures) into `resources['outputs']`     |
|  [03]   | `ExtractAttachmentsPreprocessor`                     | pull cell attachments into `resources`                         |
|  [04]   | `TagRemovePreprocessor`                              | drop tagged cells/inputs/outputs before render                 |
|  [05]   | `ClearOutputPreprocessor`                            | strip all outputs (clean-notebook export)                      |
|  [06]   | `ClearMetadataPreprocessor`                          | strip cell/notebook metadata                                   |
|  [07]   | `CoalesceStreamsPreprocessor`                        | merge consecutive stream outputs                               |
|  [08]   | `RegexRemovePreprocessor`                            | drop cells whose source matches the `patterns` regex           |
|  [09]   | `CSSHTMLHeaderPreprocessor`                          | inject Pygments/CSS into `resources` for HTML                  |
|  [10]   | `HighlightMagicsPreprocessor`                        | tag `%%`-magic cells with their language for highlighting      |
|  [11]   | `SVG2PDFPreprocessor` / `ConvertFiguresPreprocessor` | rasterize/convert figures for LaTeX/PDF                        |
|  [12]   | `writers.FilesWriter`                                | persist output + extracted resources to a build directory      |
|  [13]   | `writers.StdoutWriter`                               | stream output to stdout                                        |
|  [14]   | `postprocessors.ServePostProcessor`                  | serve reveal.js slides over HTTP (`call(input)`)               |

## [04]-[EXPORTER_CONFIG]

[CONFIG_SCOPE]: traitlets config surface — the `ExportPolicy` projection target
- rail: report

Exporter behavior is traitlets config, set at construction through `config=` (a `traitlets.config.Config`, nested `{"<ClassName>": {"<trait>": value}}`) or directly through `**kw` (top-level traits). The `document/report#REPORT` `ExportPolicy` frozen value object projects its bands onto these two surfaces through `ExportPolicy.exporter_kwargs()` — the preprocessor bands ride a nested `Config({"TagRemovePreprocessor": {"enabled": True, "remove_cell_tags": {...}}})` while content-exclusion rides top-level `**kw` (`exclude_input=`, `exclude_output=`,...) — so a new export-shaping knob is one field on `ExportPolicy` carried into the exporter with zero call-site edit, never a per-option exporter subclass. The exclusion traits are the in-process, MIME-agnostic cell/region strippers that complement the tag-driven `TagRemovePreprocessor`: tags target specifically-marked cells, the `exclude_*` traits drop a whole class (all inputs, all outputs, all markdown) without tagging. `traitlets` `List`/`Dict` traits reject a `tuple` default until assigned, and the `Set` trait under `TagRemovePreprocessor` requires a `set` (not a tuple) — the projection materializes the band as the trait's concrete container type.

| [INDEX] | [TRAIT]                 | [OWNER]            | [TYPE_DEFAULT] | [CAPABILITY]                                                       |
| :-----: | :---------------------- | :----------------- | :------------- | :----------------------------------------------------------------- |
|  [01]   | `template_name`         | `TemplateExporter` | `Unicode=''`   | named template tree to render (`lab`/`classic`/`reveal`/`basic`)   |
|  [02]   | `template_file`         | `TemplateExporter` | `Unicode=None` | single template file override within the resolved paths            |
|  [03]   | `template_paths`        | `TemplateExporter` | `List=[.]`     | search roots for the template tree                                 |
|  [04]   | `extra_template_paths`  | `TemplateExporter` | `List=[]`      | additional template roots prepended to the default search          |
|  [05]   | `filters`               | `TemplateExporter` | `Dict={}`      | name->callable Jinja-filter overrides (= `register_filter`)        |
|  [06]   | `raw_mimetypes`         | `TemplateExporter` | `List=[]`      | raw-cell MIME types the template renders rather than drops         |
|  [07]   | `exclude_input`         | `TemplateExporter` | `Bool=False`   | drop all code-cell inputs from the render                          |
|  [08]   | `exclude_output`        | `TemplateExporter` | `Bool=False`   | drop all cell outputs from the render                              |
|  [09]   | `exclude_input_prompt`  | `TemplateExporter` | `Bool=False`   | drop the `In[ ]:` input prompt gutters                             |
|  [10]   | `exclude_output_prompt` | `TemplateExporter` | `Bool=False`   | drop the `Out[ ]:` output prompt gutters                           |
|  [11]   | `exclude_markdown`      | `TemplateExporter` | `Bool=False`   | drop all markdown cells                                            |
|  [12]   | `exclude_code_cell`     | `TemplateExporter` | `Bool=False`   | drop all code cells entirely                                       |
|  [13]   | `exclude_raw`           | `TemplateExporter` | `Bool=False`   | drop all raw cells                                                 |
|  [14]   | `exclude_unknown`       | `TemplateExporter` | `Bool=False`   | drop cells of unrecognized type                                    |
|  [15]   | `preprocessors`         | `Exporter`         | `List=[]`      | extra preprocessor chain entries (class/instance/dotted-name)      |
|  [16]   | `optimistic_validation` | `Exporter`         | `Bool=False`   | skip strict nbformat validation on input (speed for trusted nodes) |

[CONFIG_SCOPE]: per-exporter assembly control — HTML / WebPDF / PDF / Slides deltas
- rail: report

Each concrete exporter adds the traits that govern its assembly backend. `HTMLExporter.embed_images=True` inlines figures as base64 data URIs for a single-file HTML; `sanitize_html=True` runs the bleach/tinycss2 sanitizer for an untrusted notebook; `theme` selects the light/dark CSS. `WebPDFExporter` drives the Playwright Chromium spawn: `allow_chromium_download=True` permits an on-demand browser fetch, `disable_sandbox=True` is required inside an already-sandboxed/container runtime, `page_render_timeout` bounds the render, `paginate=False` produces one continuous page, and `browser_args` passes raw Chromium flags. `PDFExporter` wraps the LaTeX toolchain: `latex_command`/`bib_command` are the argv templates, `latex_count` is the multi-pass count for cross-references, `verbose` surfaces the LaTeX log. These are the knobs that make `WEBPDF`/`PDF` viable in a headless pipeline.

| [INDEX] | [TRAIT]                   | [OWNER]          | [TYPE_DEFAULT]                     | [CAPABILITY]                                         |
| :-----: | :------------------------ | :--------------- | :--------------------------------- | :--------------------------------------------------- |
|  [01]   | `embed_images`            | `HTMLExporter`   | `Bool=False`                       | inline images as base64 data URIs (single-file HTML) |
|  [02]   | `sanitize_html`           | `HTMLExporter`   | `Bool=False`                       | run the bleach/tinycss2 HTML sanitizer (untrusted)   |
|  [03]   | `theme`                   | `HTMLExporter`   | `Unicode='light'`                  | CSS theme (`light`/`dark`)                           |
|  [04]   | `mathjax_url`             | `HTMLExporter`   | `Unicode=<cdn>`                    | MathJax script URL for math rendering                |
|  [05]   | `exclude_anchor_links`    | `HTMLExporter`   | `Bool=False`                       | drop heading anchor links                            |
|  [06]   | `skip_svg_encoding`       | `HTMLExporter`   | `Bool=False`                       | emit raw inline SVG instead of base64-encoding it    |
|  [07]   | `allow_chromium_download` | `WebPDFExporter` | `Bool=False`                       | permit on-demand Playwright Chromium download        |
|  [08]   | `disable_sandbox`         | `WebPDFExporter` | `Bool=False`                       | drop the Chromium sandbox (container/root run)       |
|  [09]   | `page_render_timeout`     | `WebPDFExporter` | `Int=100`                          | seconds to wait for the Chromium page render         |
|  [10]   | `paginate`                | `WebPDFExporter` | `Bool=True`                        | paginate to PDF pages (`False` = one page)           |
|  [11]   | `browser_args`            | `WebPDFExporter` | `List=[]`                          | raw extra Chromium launch flags                      |
|  [12]   | `latex_command`           | `PDFExporter`    | `List=[xelatex,{filename},-quiet]` | LaTeX compile argv template                          |
|  [13]   | `bib_command`             | `PDFExporter`    | `List=[bibtex,{filename}]`         | bibliography compile argv template                   |
|  [14]   | `latex_count`             | `PDFExporter`    | `Int=3`                            | LaTeX compile passes (cross-reference resolution)    |
|  [15]   | `reveal_theme`            | `SlidesExporter` | `Unicode='simple'`                 | reveal.js slide theme                                |
|  [16]   | `reveal_transition`       | `SlidesExporter` | `Unicode='slide'`                  | reveal.js slide transition                           |
|  [17]   | `reveal_scroll`           | `SlidesExporter` | `Bool=False`                       | enable scroll mode in the reveal.js deck             |

[CONFIG_SCOPE]: filter map — the Jinja transforms `register_filter` and the `filters` trait reach
- rail: report

`nbconvert.filters` is the named-callable map every `TemplateExporter` exposes to its templates; `register_filter(name, fn)` and the `filters` config trait both write into this map. A custom report template references these by name (`{{ cell.source | highlight2html }}`) rather than re-implementing ANSI/markdown/highlight conversion. The owner reaches them only when authoring a custom template; the default templates wire them internally.

| [INDEX] | [FILTER]                                                | [CAPABILITY]                                                                 |
| :-----: | :------------------------------------------------------ | :--------------------------------------------------------------------------- |
|  [01]   | `markdown2html` / `markdown2html_mistune`               | render Markdown to HTML (mistune backend)                                    |
|  [02]   | `markdown2latex` / `markdown2rst` / `markdown2asciidoc` | render Markdown to the target document language                              |
|  [03]   | `highlight` + `Highlight2HTML` / `Highlight2Latex`      | Pygments highlight; `highlight` + `Highlight2HTML`/`Highlight2Latex` classes |
|  [04]   | `ansi2html` / `ansi2latex` / `strip_ansi`               | convert or strip terminal ANSI color codes                                   |
|  [05]   | `html2text` / `clean_html`                              | strip HTML to text / sanitize HTML fragment                                  |
|  [06]   | `convert_pandoc` / `citation2latex`                     | pandoc-backed conversion / BibTeX citation rendering                         |
|  [07]   | `add_anchor` / `add_prompts` / `wrap_text`              | inject heading anchors / `In/Out` prompts / wrap long lines                  |
|  [08]   | `path2url` / `posix_path` / `strip_files_prefix`        | path normalization for embedded-resource links                               |

## [05]-[IMPLEMENTATION_LAW]

[REPORT_NOTEBOOK_EXPORT]:
- import: `import nbconvert` at boundary scope only; module-level import is banned by the manifest import policy.
- resolution axis: one `get_exporter` owns export-target resolution; `pdf`/`webpdf`/`html`/`latex`/`markdown`/`rst`/`asciidoc`/`slides`/`notebook`/`python`/`script`/`qtpdf`/`qtpng` are name rows on the `nbconvert.exporters` entrypoint registry, never a per-format selector function; a dotted import path resolves a third-party exporter through the same call.
- conversion axis: `export` is the single conversion surface that dispatches on the `nb` shape (`NotebookNode` -> `from_notebook_node`, `str` -> `from_filename`, else `from_file`); the campaign uses `from_notebook_node` for the in-memory notebook and reads `output` plus the `resources` dict, never a parallel per-format convert function.
- exporter axis: each concrete exporter is a `TemplateExporter`/`Exporter` subclass selected by name, never a hand-rolled renderer; the base `Exporter` returns a `NotebookNode`, every `TemplateExporter` returns rendered `str`; `PDFExporter` assembles through LaTeX, `WebPDFExporter` through headless Chromium, `QtPDFExporter`/`QtPNGExporter` through a Qt screenshot; `ExporterDisabledError` is raised when a resolved name is config-disabled.
- pipeline axis: the preprocessor chain mutates `(nb, resources)` before render and is configured as `Preprocessor` rows or appended via `register_preprocessor`, never inlined into the exporter; the campaign executes through `nbclient` directly and leaves `ExecutePreprocessor` disabled to avoid double execution; figure extraction (`ExtractOutputPreprocessor`) and tag removal (`TagRemovePreprocessor`) are configured stages, not bespoke transforms.
- writer axis: persisting `(output, resources)` is the `writers.FilesWriter`/`StdoutWriter` job (writes `output` plus `resources['outputs']` figure bytes), never a hand-rolled file write that re-derives the resources layout.
- config axis: exporter behavior (template name, preprocessors, execution, extraction, Jinja filters via `register_filter`) is traitlets config passed through `config=` or `**kw` at construction, never a parallel exporter subclass per option.
- evidence: each render captures the resolved export name, exporter class, output byte length, sorted `resources` keys, and `output_extension` in the `ReportFact.notebook` case; the flat report receipt projects output byte length.
- boundary: nbconvert owns notebook-to-document conversion and the Jinja template/preprocessor pipeline; `from_notebook_node` consumes a `nbformat.NotebookNode` from the upstream notebook owner; PDF output feeds the document owner and HTML output feeds the visuals owner; the `jupyter-nbconvert` CLI stays outside the in-process path.

[CROSS_TIER_STACKING]:
- runtime lane: `from_notebook_node` is blocking native/subprocess work, so `ReportPlan.lane.offload(Kernel.of(_exported, KernelTrait.RELEASING), spec, executed)` bounds every exporter through the runtime owner rather than an inline await or folder-local limiter.
- msgspec policy rail (shared `libs/python/.api/msgspec`): the export-shaping traits never reach the call site as loose kwargs — they are fields on the frozen `ExportPolicy` `msgspec.Struct`, projected through `ExportPolicy.exporter_kwargs()` into class-keyed `traitlets.config.Config` rows. A new knob is one struct field; the table-driven projection is the structural peer of the execution-side `NotebookEngine.client_kwargs()` projection onto `nbclient`.
- expression/runtime fault rail (shared `libs/python/.api/expression` + the runtime `faults` owner): `ReportPlan.of` maps `ExporterNameError` and `ExporterDisabledError` onto `ReportFault.export` before execution; provider raises from `papermill`, `nbclient`, and exporter execution close through the plan's `async_boundary` capsule. One fault algebra owns the `NOTEBOOK` arm.
- node-tree rail (folder `document/model#NODE`): the `(output, resources)` pair is partitioned on the output's own `str`-vs-`bytes` type — a `TemplateExporter` `str` wraps as a `BlockKind.CODE` HTML leaf, while binary output becomes a content-addressed `FigureNode` asset with the exporter MIME type. `resources['outputs']` contributes extracted `FigureNode` assets, and sorted resource keys remain evidence in `ReportFact.notebook`; no export-name membership set re-derives either route.
- folder sibling chain (`jupytext` -> `papermill` -> `nbclient` -> nbconvert): nbconvert is the terminal lowering stage — `jupytext.reads` parses the source, `papermill.parameterize_notebook` injects parameters, `NotebookClient.async_execute` runs the kernel, and `get_exporter(...).from_notebook_node(executed)` lowers the result. `jupytext.writes(executed, "ipynb")` owns the round-trip archive, so admission accepts enabled/plugin exporter names and rejects only the `notebook` exporter whose `NotebookNode` result violates the `str`/`bytes` result contract.

[RAIL_LAW]:
- Package: `nbconvert`
- Owns: export-target resolution, the `Exporter`/`TemplateExporter` class family, and notebook-to-PDF/HTML/LaTeX/Markdown/RST/AsciiDoc/slides/script conversion with its `(output, resources)` contract
- Accept: notebook-node export feeding the report, document, and visuals owners
- Reject: wrapper-renames of `get_exporter`/`export`/`from_notebook_node`; CLI subprocess conversion; per-format render functions; hand-rolled Jinja, LaTeX, Chromium, or cell-stripping pipelines; per-config exporter subclasses; inline blocking render; loose trait kwargs outside `ExportPolicy.exporter_kwargs()`; escaping registry faults; export-name output routing; and the `notebook` round-trip target already owned by `jupytext.writes`

# [PY_ARTIFACTS_API_NBCONVERT]

`nbconvert` owns notebook-to-document conversion for the artifacts report rail: `get_exporter` resolves an export name or dotted import path to an `Exporter` class, `export` dispatches one `NotebookNode`/file/stream to it, and every conversion returns `(output, resources)` — rendered text or bytes paired with its extracted-asset dict. Rendering drives the Jinja template, preprocessor chain, filter map, and LaTeX/Chromium/Qt assembly in-process; `document/report#REPORT` composes it into the `ReportKind.NOTEBOOK` arm.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `nbconvert`
- package: `nbconvert`
- import: `nbconvert`
- owner: `artifacts`
- rail: report
- license: BSD-3-Clause
- deps: `jinja2` drives the template and filter map; the `[webpdf]` extra pulls `playwright` (headless Chromium) and `[qtpdf]`/`[qtpng]` pull `pyqtwebengine`, gating those exporters
- capability: resolve an export target by name or dotted import path, instantiate the `Exporter`, and convert a `NotebookNode`/file/stream to a registered document target paired with a resources dict, driving the Jinja template, preprocessor chain, filter map, and LaTeX/Chromium/Qt assembly in-process; behavior shapes through traitlets `config=`/`**kw`, persistence through the `writers` family

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: exporter classes and resolution failures

`Exporter` runs preprocessors and returns `(NotebookNode, resources)`; `TemplateExporter` renders through Jinja and returns `(str, resources)`, the base every document exporter extends. `get_exporter` raises `ExporterNameError` on an unresolvable name and `ExporterDisabledError` on a resolved-but-config-disabled name. `ResourcesDict` is the auto-vivifying second tuple element; `FilenameExtension` types an exporter's output extension.

| [INDEX] | [SYMBOL]                | [TYPE_FAMILY]  | [CAPABILITY]                                                   |
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

`get_exporter` resolves an export name or dotted import path against the `nbconvert.exporters` entry-point registry; `export` instantiates the exporter and dispatches on the `nb` shape — `NotebookNode` to `from_notebook_node`, `str` to `from_filename`, else `from_file`.

| [INDEX] | [SURFACE]                                        | [SHAPE] | [CAPABILITY]                                               |
| :-----: | :----------------------------------------------- | :------ | :--------------------------------------------------------- |
|  [01]   | `get_exporter(name, config) -> type[Exporter]`   | static  | resolve an export-name or import path to an exporter class |
|  [02]   | `export(exporter, nb, **kw) -> tuple[str, dict]` | static  | instantiate and convert, dispatching on `nb` shape         |
|  [03]   | `get_export_names(config) -> list[str]`          | static  | list enabled export targets                                |

- `webpdf`: resolves enabled but raises at construct time when the `[webpdf]` extra is absent.

[ENTRYPOINT_SCOPE]: `Exporter` construct and convert

`Exporter(config, **kw)` is one constructor shape across every concrete exporter. Three `from_*` methods share `(<source>, resources=None, **kw) -> tuple[str | NotebookNode, dict]`; output is `str` for every `TemplateExporter` subclass and `NotebookNode` for the base `Exporter`/`NotebookExporter` round-trip. `from_notebook_node` is the in-memory path the campaign consumes.

| [INDEX] | [SURFACE]                                      | [SHAPE]  | [CAPABILITY]                                          |
| :-----: | :--------------------------------------------- | :------- | :---------------------------------------------------- |
|  [01]   | `Exporter(config, **kw)`                       | ctor     | construct an exporter with config + traitlets kwargs  |
|  [02]   | `from_notebook_node(nb, resources, **kw)`      | instance | convert an in-memory notebook node                    |
|  [03]   | `from_filename(filename, resources, **kw)`     | instance | convert a notebook file path                          |
|  [04]   | `from_file(file_stream, resources, **kw)`      | instance | convert a notebook stream                             |
|  [05]   | `register_preprocessor(preprocessor, enabled)` | instance | append a preprocessor (class/instance/dotted-name)    |
|  [06]   | `register_filter(name, jinja_filter)`          | instance | register a Jinja filter callable (`TemplateExporter`) |

[ENTRYPOINT_SCOPE]: preprocessor chain and output writers

A configured `Preprocessor` chain mutates `(nb, resources)` before render, appended via `register_preprocessor`. `ExecutePreprocessor` wraps `nbclient.NotebookClient`; the campaign executes through nbclient directly and leaves it `enabled=False` to avoid double execution. `writers` persist `output` with every `resources['outputs']` figure to a build directory, the path `ExtractOutputPreprocessor`'s figure bytes reach disk.

| [INDEX] | [SURFACE]                                            | [SHAPE]       | [CAPABILITY]                                                   |
| :-----: | :--------------------------------------------------- | :------------ | :------------------------------------------------------------- |
|  [01]   | `ExecutePreprocessor`                                | preprocessor  | run the notebook via `nbclient` before render (off by default) |
|  [02]   | `ExtractOutputPreprocessor`                          | preprocessor  | pull display outputs (figures) into `resources['outputs']`     |
|  [03]   | `ExtractAttachmentsPreprocessor`                     | preprocessor  | pull cell attachments into `resources`                         |
|  [04]   | `TagRemovePreprocessor`                              | preprocessor  | drop tagged cells/inputs/outputs before render                 |
|  [05]   | `ClearOutputPreprocessor`                            | preprocessor  | strip all outputs (clean-notebook export)                      |
|  [06]   | `ClearMetadataPreprocessor`                          | preprocessor  | strip cell/notebook metadata                                   |
|  [07]   | `CoalesceStreamsPreprocessor`                        | preprocessor  | merge consecutive stream outputs                               |
|  [08]   | `RegexRemovePreprocessor`                            | preprocessor  | drop cells whose source matches the `patterns` regex           |
|  [09]   | `CSSHTMLHeaderPreprocessor`                          | preprocessor  | inject Pygments/CSS into `resources` for HTML                  |
|  [10]   | `HighlightMagicsPreprocessor`                        | preprocessor  | tag `%%`-magic cells with their language for highlighting      |
|  [11]   | `SVG2PDFPreprocessor` / `ConvertFiguresPreprocessor` | preprocessor  | rasterize/convert figures for LaTeX/PDF                        |
|  [12]   | `writers.FilesWriter`                                | writer        | persist output + extracted resources to a build directory      |
|  [13]   | `writers.StdoutWriter`                               | writer        | stream output to stdout                                        |
|  [14]   | `postprocessors.ServePostProcessor`                  | postprocessor | serve reveal.js slides over HTTP (`call(input)`)               |

[ENTRYPOINT_SCOPE]: base content and preprocessor traits

Exporter behavior is traitlets config, set through `config=` (a nested `Config({"<ClassName>": {"<trait>": value}})`) or top-level `**kw`. Every `exclude_*` trait drops a whole cell class (all inputs, all outputs, all markdown) in-process, complementing `TagRemovePreprocessor` which targets specifically-tagged cells. `traitlets` `List`/`Dict` traits reject a `tuple` default and `TagRemovePreprocessor`'s `Set` trait requires a `set`, so a projection materializes each band as the trait's concrete container type.

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

[ENTRYPOINT_SCOPE]: per-exporter assembly-backend traits

Each concrete exporter adds its assembly-backend traits: `HTMLExporter` inlines/sanitizes/themes the HTML, `WebPDFExporter` drives the Playwright Chromium spawn (`disable_sandbox` required inside an already-sandboxed runtime), `PDFExporter` wraps the LaTeX toolchain, `SlidesExporter` shapes the reveal.js deck.

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

[ENTRYPOINT_SCOPE]: filter map

`nbconvert.filters` is the named-callable map every `TemplateExporter` exposes to its templates; `register_filter` and the `filters` trait both write into it. A custom template references these by name (`{{ cell.source | highlight2html }}`) rather than re-implementing ANSI/Markdown/highlight conversion.

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

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- resolution: one `get_exporter` owns export-target resolution; every enabled name and a dotted import path resolve through it, never a per-format selector.
- conversion: one `export` dispatches on the `nb` shape; the campaign reads `from_notebook_node(nb)` -> `(output, resources)` in memory, never a per-format convert function.
- exporter: each target is a `TemplateExporter`/`Exporter` subclass selected by name; the base returns a `NotebookNode`, every `TemplateExporter` a `str`; `PDFExporter`/`WebPDFExporter`/`QtPDFExporter`/`QtPNGExporter` assemble through LaTeX/Chromium/Qt; a config-disabled resolved name raises `ExporterDisabledError`.
- pipeline: the preprocessor chain mutates `(nb, resources)` as configured `Preprocessor` rows; `ExecutePreprocessor` stays `enabled=False` because nbclient executes upstream; figure extraction and tag removal are configured stages, not bespoke transforms.
- persistence: `writers.FilesWriter`/`StdoutWriter` write `output` with `resources['outputs']` figure bytes, never a hand-rolled write re-deriving the resources layout.
- config: template, preprocessor, exclusion, and Jinja-filter behavior is traitlets config through `config=`/`**kw`, never a per-option exporter subclass.
- evidence: each render keeps the resolved export name, exporter class, output byte length, sorted `resources` keys, and `output_extension` in the `ReportFact.notebook` case.

[STACKING]:
- runtime lane: `from_notebook_node` is blocking, so `ReportPlan.lane.offload(Kernel.of(_exported, KernelTrait.RELEASING), spec, executed)` bounds every render through the runtime owner, never an inline await or folder-local limiter.
- `msgspec`(`.api/msgspec` under `libs/python/.api`): export-shaping traits are fields on the frozen `ExportPolicy` `msgspec.Struct`, projected through `ExportPolicy.exporter_kwargs()` into class-keyed `traitlets.config.Config` rows — preprocessor bands nested, content-exclusion top-level `**kw`. A new knob is one struct field, the structural peer of the execution-side `NotebookEngine.client_kwargs()` projection.
- `expression`(`.api/expression` under `libs/python/.api` + the runtime `faults` owner): `ReportPlan.of` maps `ExporterNameError`/`ExporterDisabledError` onto `ReportFault.export`; raises from `papermill`, `nbclient`, and exporter execution close through the plan's `async_boundary` capsule. One fault algebra owns the `NOTEBOOK` arm.
- node-tree rail (`document/model#NODE`): `(output, resources)` partitions on the output's `str`-vs-`bytes` type — a `TemplateExporter` `str` wraps a `BlockKind.CODE` HTML leaf, binary output a content-addressed `FigureNode` asset carrying the exporter MIME type, and `resources['outputs']` contributes extracted `FigureNode` assets; PDF output feeds the document owner, HTML the visuals owner.
- folder chain (`jupytext` -> `papermill` -> `nbclient` -> nbconvert): nbconvert is the terminal lowering stage — `get_exporter(...).from_notebook_node(executed)` lowers the executed node while `jupytext.writes(executed, "ipynb")` owns the round-trip archive.

[LOCAL_ADMISSION]:
- `document/report#REPORT` composes `get_exporter`, exporter classes, and `from_notebook_node` into the `ReportKind.NOTEBOOK` arm; admission accepts enabled and plugin exporter names and rejects the `notebook` round-trip target whose `NotebookNode` result violates the `str`/`bytes` contract, already owned by `jupytext.writes`.

[RAIL_LAW]:
- Package: `nbconvert`
- Owns: export-target resolution, the `Exporter`/`TemplateExporter` class family, and notebook-to-PDF/HTML/LaTeX/Markdown/RST/AsciiDoc/slides/script conversion with its `(output, resources)` contract
- Accept: notebook-node export feeding the report, document, and visuals owners
- Reject: wrapper-renames of `get_exporter`/`export`/`from_notebook_node`; CLI subprocess conversion; per-format render functions; hand-rolled Jinja, LaTeX, Chromium, or cell-stripping pipelines; per-config exporter subclasses; inline blocking render; loose trait kwargs outside `ExportPolicy.exporter_kwargs()`; escaping registry faults; export-name output routing; and the `notebook` round-trip target owned by `jupytext.writes`
</content>
</invoke>

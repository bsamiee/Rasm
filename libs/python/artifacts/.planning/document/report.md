# [PY_ARTIFACTS_REPORT]

The reproducible-report composition layer binding data and visual outputs into the one `document/model#NODE` `DocumentNode` tree, the composition peer of the `document/emit#DOCUMENT` lowering owner it is structurally identical to. `ReportPlan` is ONE frozen `msgspec.Struct` owner discriminating report kind over the `COMPOSE_ARMS` `frozendict[ReportKind, ComposeArm]` coroutine-row policy table — the `BACKENDS`/`_CORE_ARMS` band-table idiom `document/emit#DOCUMENT` and `document/lens#LENS` prove — so `_emit` is one uniform `await COMPOSE_ARMS[self.kind](self)` with zero branch. The canonical interior every kind PRODUCES is the `DocumentNode` tree, never a string: section composition (a `Section` value-object algebra folding heading level, an optional CSI/OmniClass `classification` onto the `SectionNode` `NodeMeta`, a rich `SectionBlock` body — prose blocks of any `BlockKind`, `ListNode` lists, AND inline `FigureRef` figures INTERLEAVED in one ordered flow — and nested subsections into a `SectionNode`/`BlockNode`/`ListNode`/`FigureNode` subtree keyed by structural path, with a leading table-of-contents `SectionNode` whose entries ride a real `document/model#NODE` `ListNode`/`ListKind.ORDERED` grouping, never a phantom block kind and never a `figures` trail forced after every block), `jinja2` template composition (a loader-row engine over the trusted/sandboxed/native `Environment` policy built at boundary scope under the serializable `extensions` dotted-path band plus the report-scoped `_REPORT_FILTERS`/`template_globals`/deterministic-JSON-policy registries and an optional `FileSystemBytecodeCache` for repeated reproducible runs, a strict-undefined async render over the bound `context` data band emitting a free-form HTML `BlockNode` leaf — or, under `nativetypes.NativeEnvironment`, a computed Python value lowered through the total `_NATIVE` encoder to a deterministic structured-data leaf), parameterized-notebook execution (a `ReportSource`-row `jupytext` text-source pairing into a `NotebookNode` — `AUTO` deferring to `jupytext.reads(fmt=None)` content detection — the supplied parameters admitted once against the `ReportParams` value-space band through the plan's own `ReportPayload` gate, a `papermill.parameterize.parameterize_notebook` injection threading the `NotebookEngine.kernel_name` so a non-`python3` kernel routes its own `PapermillTranslators` row, a host-free `nbclient.NotebookClient.async_execute` run under the frozen `NotebookEngine` traits projected through `NotebookEngine.client_kwargs` and the `resources` cwd seed so a relative-asset notebook resolves, an executed-notebook archive, and an `nbconvert.get_exporter(...)` lowering shaped by the `ExportPolicy` `TagRemovePreprocessor` tag bands and dispatched through `from_notebook_node` whose `(output, resources)` pair feeds a `BlockNode` leaf — a `str` output a `BlockKind.CODE` HTML leaf and the binary `PDF`/`WEBPDF` `bytes` output a `BlockKind.QUOTE` hex leaf partitioned on the output's own type, never an HTML wrapper over PDF bytes), reflowable re-layout (the `pymupdf.Story.write`/`write_stabilized`/`DocumentWriter` HTML-into-PDF reflow the `REFLOW` kind AUTHORS on the runtime subprocess lane as a fresh `PageNode` plus the reflowed PDF bytes, `Story.add_pdf_links` folding placed-element positions into live links and `write_stabilized` converging the TOC/cross-reference layout), and commercial-safe authoring (the MIT/Apache `pdf_oxide.Pdf.from_markdown`/`from_html`/`from_markdown_with_template`/`OfficeConverter` arm the `AUTHOR` kind runs on the `to_thread` lane as the license-clean peer of the AGPL `REFLOW` arm, the running-header/footer `PageTemplate` authoring the AEC titled sheet).

Every per-kind input is one frozen `ReportSpec` `msgspec.Struct` admitted exactly once at `ReportPlan.of` through the closed `ReportPayload` `TypedDict` and its module-level `TypeAdapter` — never a `dict[str, object]` bag, never re-validated in the interior — the per-kind `_REQUIRED` precondition making admission total over well-formed requests. Every kind threads ONE `ReportFact` typed-evidence carrier onto the frozen owner through `copy.replace` (a thin pure `_emit` core returning the stepped `Self`), and the `@receipted(OPEN)` harvest weave drains `ReportPlan.contribute` off the stepped owner so the receipt reads `ReportFact` without an in-process re-run; `ReportFault` is the closed `@tagged_union` over the `payload`/`unsatisfied` ADMISSION causes `of` produces, while every arm-level provider raise (`jinja2.UndefinedError`/`TemplateSyntaxError`/`TemplateNotFound`/`sandbox.SecurityError`, `pydantic.ValidationError`, `papermill.PapermillMissingParameterException`/`PapermillExecutionError`, `nbclient.exceptions.CellExecutionError`/`CellTimeoutError`/`DeadKernelError`, `nbconvert.ExporterNameError`) converts to the runtime `BoundaryFault` at the `async_boundary` capsule, exactly as `document/emit#DOCUMENT` routes its provider raises. It owns neither the visual nor the codec — it binds the figures keyed by content key into the tree (a `FigureNode` per `asset_key`, never re-rendering the chart/table/scene), executes a notebook under a bounded-safety engine, reflows HTML through the native MuPDF `Story.write`/`write_stabilized` sweep, authors a commercial-safe PDF through the pdf_oxide markdown/HTML/office arm, threads each emitted artifact onto its `ReportFact`, and hands the tree to the document axis.

## [01]-[INDEX]

- [01]-[REPORT]: report-composition axis over the section/template/notebook/reflow/author kinds producing one `DocumentNode` interior through the one `COMPOSE_ARMS` coroutine-row `frozendict`, the `Section`/`SectionBlock`/`FigureRef` heading-rich-body-and-figure value objects the `COMPOSE` kind folds into a path-keyed tree (prose blocks of any `BlockKind`, `ListNode` lists, and inline `figure` units interleaved in one ordered body, an optional CSI/OmniClass `Section.classification` onto the `SectionNode` `NodeMeta`, nested subsections, the TOC over a real `ListNode`), the closed `ReportPayload` `TypedDict` ingress plus its `ReportSpec` carrier, the `ReportParams` `extra_items` notebook-parameter value-space band, the `ReportFact` typed-evidence carrier threaded onto the owner, the `NotebookEngine` bounded-safety engine-config value with its `client_kwargs` nbclient projection and the `resources` cwd seed, the boundary-scoped `jinja2` loader-sandbox-native sub-axis over the serializable `extensions` dotted-path band with the report-scoped `_REPORT_FILTERS`/`template_globals`/policy registries, the `FileSystemBytecodeCache`, and the `context` render-data band, the `TemplateRender` string-vs-native render policy, the `ReportSource` `jupytext` text-source sub-axis (the `AUTO` auto-detect row included), the `ReportExport` closed `nbconvert` export-name vocabulary discriminated by the rendered output's own `str`-vs-`bytes` type, the `ExportPolicy` nbconvert export-shaping value with its `TagRemovePreprocessor` `exporter_kwargs` projection, the `ReflowPaper` named-paper-size and `ReflowLayout` direct-vs-stabilized sub-axes, the `AuthorSource` commercial-safe pdf_oxide authoring vocabulary over the `_AUTHOR_BUILD` derived dispatch (the running-header/footer `PageTemplate` sheet + `OfficeConverter` office source), the `@receipted` harvest weave over a thin pure `_emit` returning `Self`, and the one `rendered` modal-arity entry over `ReportPlan | Iterable[ReportPlan]`.

## [02]-[REPORT]

- Owner: `ReportPlan` the one frozen `msgspec.Struct` composition axis discriminating report kind over the `COMPOSE_ARMS` `frozendict[ReportKind, ComposeArm]` (every row a `Callable[[ReportPlan], Awaitable[ReportFact]]`, the `document/emit#DOCUMENT BACKENDS`/`document/lens#LENS _CORE_ARMS` band-table idiom) and producing one `DocumentNode` interior; `ReportKind` the closed `StrEnum` over `COMPOSE`/`TEMPLATE`/`NOTEBOOK`/`REFLOW`/`AUTHOR` (the AGPL `pymupdf.Story` reflow and the MIT/Apache `pdf_oxide` commercial-safe author its two PDF-producing arms); `ReportSpec` the one frozen `msgspec.Struct(omit_defaults=True)` carrier holding the admitted report source, sections, figure refs, loader/source/export rows, engine, trust flag, paper, and TOC knobs — admitted once at `ReportPlan.of` from the closed `ReportPayload` `TypedDict(closed=True)` + module-level `TypeAdapter` and never re-validated; `_REQUIRED` the `frozendict[ReportKind, tuple[str, ...]]` precondition table whose row names the spec fields a kind demands so `of` rejects an empty `sections`/`source` before the interior; `ReportFact` the threaded `msgspec.Struct` evidence carrier (the rendered `DocumentNode`, the content-keyed `body` bytes, the optional notebook-archive bytes, the reflow page count, the resolved export/loader/source rows, and the per-cell-timing and widget-state flags) read by `contribute` to mint the receipt rows; `Section` the frozen value object carrying a heading level, an optional CSI/OmniClass `classification`, a `SectionBlock` body sequence (prose/list/figure units), and nested subsections the `COMPOSE` fold lowers into a `SectionNode`/`BlockNode`/`ListNode`/`FigureNode` subtree; `SectionBlock` the closed body-unit `@tagged_union` (`prose`/`listing`/`figure`) interleaving a figure IN flow between its sibling blocks rather than a parallel `Section.figures` trail; `FigureRef` the one figure-reference value object (`asset_key` content key, `alt` equivalent, `caption`, plus the `media_type`/`intrinsic` the lowered `FigureNode` carries) the `COMPOSE` `SectionBlock.figure` case and the `TEMPLATE` `spec.figures` kwarg both bind; `ReportParams` the `TypedDict(extra_items=ParamScalar)` notebook-parameter value-space band admitted through the one `ReportPayload` gate; `ReportLoader` the closed `StrEnum` selecting the `jinja2` loader composition (`DictLoader`/`FileSystemLoader`/`PackageLoader`/`PrefixLoader`/`ModuleLoader` folded through `ChoiceLoader`) the boundary-scoped `match` resolves; `ReportSource` the closed `StrEnum` selecting the `jupytext` text representation, with `AUTO` deferring to `reads(fmt=None)` content detection; `ReportExport` the closed `StrEnum` over the `nbconvert.exporters` registry name rows `get_exporter` resolves; `NotebookEngine` the frozen engine-config value object carrying the full `nbclient.NotebookClient` bounded-safety trait set projected to constructor kwargs through `NotebookEngine.client_kwargs`; `ExportPolicy` the frozen nbconvert export-shaping value object carrying the `TagRemovePreprocessor` `remove_cell_tags`/`remove_input_tags`/`remove_all_outputs_tags` bands projected to the exporter's `config=` parameter through `ExportPolicy.exporter_kwargs` — the export-side peer of the execution-side `NotebookEngine`, stripping the report's scaffolding cells before the `from_notebook_node` lowering; `ReflowPaper` the closed `StrEnum` over the `pymupdf.paper_rect` named-size vocabulary every kind's `media_box` derives from through `_box`; the bind layer between visual outputs and document inputs.
- Cases: `_compose_arm` (the `COMPOSE` row) folds the `Section` value-object sequence into a `SectionNode` outline (each `SectionBlock` through `_block_node` — a `prose` unit to a `BlockNode`, a `listing` to a `ListNode`, a `figure` to an `asset_key`-keyed `FigureNode` in its own flow slot — and the section's `classification` onto the `SectionNode` `NodeMeta`), synthesizes a leading table-of-contents `SectionNode` carrying a `ListNode(list_kind=ListKind.ORDERED)` of per-heading entries through `_toc`, and returns a `ReportFact` over the assembled `PageNode` whose `body` is `encode(page)`; `_template_arm` (the `TEMPLATE` row) builds the trusted `jinja2.Environment` or untrusted `jinja2.sandbox.ImmutableSandboxedEnvironment` at boundary scope under the strict-undefined async policy and the serializable `extensions` dotted-path band, resolves the `ReportLoader` row through one boundary-scoped `match`, renders `Environment.from_string(spec.source).render_async(sections=, figures=)` into an HTML string `_output_page` wraps as one `BlockNode(block=BlockKind.CODE)` HTML leaf the `document/emit#DOCUMENT PDF_HTML` weasyprint arm consumes; `_notebook_arm` (the `NOTEBOOK` row) admits the source through the `ReportSource` `jupytext` row (`AUTO` passing `fmt=None` so `reads` content-detects) into one `nbformat.NotebookNode`, injects the `ReportPayload`-admitted parameters through `papermill.parameterize.parameterize_notebook` over `papermill.parameterize.add_builtin_parameters` threading `kernel_name=spec.engine.kernel_name`, executes the node headlessly through `nbclient.NotebookClient(node, **spec.engine.client_kwargs()).async_execute()`, reads the `(output, resources)` pair from `nbconvert.get_exporter(spec.export.value)(**spec.export_policy.exporter_kwargs()).from_notebook_node(executed)` (the `ExportPolicy` tag bands stripping the report's scaffolding cells through one `TagRemovePreprocessor` `config`), and threads a `ReportFact` carrying the lowered `BlockNode` page, the `jupytext.writes(executed, "ipynb")` archive bytes, the resolved export row, and the engine's timing/widget flags; `_reflow_arm` (the `REFLOW` row) flows the source HTML through one `pymupdf.Story(html, user_css, em)`/`DocumentWriter(BytesIO)` `place`/`draw` loop over the `ReflowPaper`-resolved `paper_rect` on the runtime subprocess lane (`anyio.to_process.run_sync`), recovers the reflowed PDF bytes plus the page count as the keyed `body`, and threads a `ReportFact` carrying the `PageNode` and the page count the receipt's `ArtifactReceipt.Pdf` case lands on. `ReportLoader` rows `DICT` (`DictLoader` in-memory sections) · `FILESYSTEM` (`FileSystemLoader` template roots) · `PACKAGE` (`PackageLoader` package-resource tree) · `PREFIX` (`PrefixLoader` namespaced child loaders) · `MODULE` (`ModuleLoader` pre-compiled template tree) — the `FunctionLoader` callable provider deleted because a callable is no serializable spec value. `ReportSource` rows `IPYNB` · `MYST` · `PERCENT` (`py:percent`) · `LIGHT` (`py:light`) · `MARKDOWN` · `RMD` (`Rmd`) · `QMD` (`qmd`) · `AUTO` (content-detected through `reads(fmt=None)`). `ReportExport` rows `HTML` · `PDF` · `WEBPDF` · `LATEX` · `MARKDOWN` · `RST` · `ASCIIDOC` · `SLIDES` · `PYTHON` · `SCRIPT` — each an `nbconvert.exporters` name `get_exporter` resolves, raising `ExporterNameError` on an unknown name; the `NOTEBOOK` round-trip target is deleted because it returns a `NotebookNode` the `jupytext.writes` archive already owns. `ReflowPaper` rows `A4` · `A3` · `A5` · `LETTER` · `LEGAL` — each a `pymupdf.paper_rect` named-size string.
- Entry: `rendered(plans)` is the ONE modal-arity entrypoint over `ReportPlan | Iterable[ReportPlan]` discriminating on the INPUT SHAPE — a lone plan is `Block.singleton`, an iterable is `Block.of_seq`, normalized once at the head and threaded through the rail with NO `batch`/`mode` knob and no sibling singular `render`; it wraps `_composed(block)` in one `async_boundary("report.compose", ...)` and returns `RuntimeRail[Block[ContentKey]]`, the exact `document/emit#DOCUMENT produced` shape. `_emit` is the thin pure core under the `@receipted(OPEN)` weave: it threads `replace(self, fact=await COMPOSE_ARMS[self.kind](self))` and returns `Self` so the harvest drains `contribute`, and `_composed` reads each stepped `plan.fact.body` to mint the `ContentKey` block; the `async_boundary` capsule converts every provider-exception family into `BoundaryFault` through the runtime `CLASSIFY` rows, never a per-kind `try`/`except` inside `_emit`.
- Auto: the `COMPOSE` fold reduces the `Section` sequence into a `SectionNode` outline through `_section_node` — each `SectionBlock` folds through `_block_node` in document order (a `prose` unit to a `BlockNode` of its `BlockKind`, a `listing` to a `ListNode`, a `figure` to an `asset_key`-keyed `FigureNode` carrying the ref's `alt`/`media_type`/`intrinsic` between its sibling blocks), the section's `classification` rides the `SectionNode` `NodeMeta`, and `_toc` synthesizes a leading table-of-contents `SectionNode` whose `ListNode(list_kind=ListKind.ORDERED)` rows one `BlockNode` entry per heading — so the interleaved figures and the outline land IN the tree rather than as a jinja kwarg; template binding folds the sections and the one `FigureRef` sequence into `Environment.from_string(...).render_async` (strict-undefined: a missing section key is a `jinja2.UndefinedError` fault, never a silent blank) and wraps the rendered HTML as one `BlockNode` HTML leaf; notebook execution admits the `ReportSource` text through `jupytext.reads(text, fmt)` (`AUTO` passing `fmt=None`), injects the `ReportPayload`-admitted parameters through `parameterize_notebook(node, add_builtin_parameters(dict(spec.notebook_parameters)), report_mode=False, kernel_name=spec.engine.kernel_name)` (the one injection point `papermill` owns, the `kernel_name` selecting the `PapermillTranslators` row so an `R`/`julia`/`.net-csharp` kernel serializes its parameter cell in its own language), runs the node through `nbclient.NotebookClient(node, **spec.engine.client_kwargs()).async_execute()` on the async kernel lane (a runaway cell raising `CellTimeoutError`/`CellExecutionError`, a kernel death `DeadKernelError`), serializes the executed node — whose per-cell timing `record_timing=True` already stamped into its metadata and whose widget state `store_widget_state=True` captured — to the reproducibility-archive bytes through `jupytext.writes(executed, "ipynb")`, and lowers the executed node through `nbconvert.get_exporter(export.value)(**spec.export_policy.exporter_kwargs()).from_notebook_node(executed)` — the `ExportPolicy` tag bands entering through the one `TagRemovePreprocessor` `config` so the report strips its scaffolding cells/inputs/outputs before the lowering — reading the `(output, resources)` pair: a text exporter's `str` output wraps as a `BlockKind.CODE` HTML leaf while a `PDF`/`WEBPDF` export's `bytes` output wraps as a `BlockKind.QUOTE` hex leaf, the `isinstance(output, str)` partition `_output_page` owns so a binary export never mis-renders as HTML text, the `resources` sidecar staying the export-extraction channel; the `REFLOW` loop opens one `pymupdf.DocumentWriter(io.BytesIO())` over a held buffer, lays the HTML through `pymupdf.Story(html, user_css, em).place(rect)` until `more == 0`, draws each filled slice onto the `begin_page(paper_rect)` device, closes the writer, and returns the buffer's reflowed-PDF bytes, the page count, and the rect; every kind's non-reflow `media_box` derives from `_box(spec.paper)` over `pymupdf.paper_rect(...)`, never a hand-built `(0, 0, 595, 842)` literal.
- Receipt: the `@receipted(OPEN)` harvest weave stacks over the pure `_emit`, draining `ReportPlan.contribute` and emitting through `Signals.emit_async`; `contribute` reads the threaded `ReportFact` off `self.fact` (never an in-process re-run of an arm), re-mints the content key over `fact.body`, and folds the case off the `ReportKind` discriminant in one total `match self.kind` closed by `assert_never` — the `COMPOSE`/`TEMPLATE` kinds key once through `ContentIdentity.of(f"report-{kind}", fact.body)` as a `core/receipt#RECEIPT`-owned `ArtifactReceipt.Report(key, byte_count)` row, the `NOTEBOOK` kind keys the rendered tree as one `Report` row plus the `fact.archive` archive through a second `ContentIdentity.of` as a second `Report` row, and the `REFLOW` kind keys `fact.body` (the reflowed PDF bytes) as the page-count-bearing `ArtifactReceipt.Pdf(key, byte_count, page_count)` row (the receipt owner's three-field PDF case — a `Report` row would silently drop the page count the reflow loop already counts), each minted through the no-phase `ArtifactReceipt.<Case>(...).contribute()` port `core/receipt#RECEIPT` declares, never a widened tuple and never a `phase` knob. The rich per-render evidence (the resolved loader/source/export row, the gated `ReportParams` field set, the per-cell timing and widget-state flags) rides the `ReportFact` carrier the consumer reads off `self.fact`; a richer report-evidence receipt case is a `core/receipt#RECEIPT` growth concern, never minted here against a case that does not exist.
- Packages: `jinja2` (`Environment(extensions=)`/`sandbox.ImmutableSandboxedEnvironment`/`sandbox.SecurityError`/`StrictUndefined`/`select_autoescape`/`FileSystemLoader`/`DictLoader`/`PackageLoader`/`PrefixLoader`/`ModuleLoader`/`ChoiceLoader`/`Environment.from_string`/`Template.render_async`/`ext.Extension`/`UndefinedError`/`TemplateSyntaxError`/`TemplateNotFound`, catalogue loader rows `[01]`-`[03]`/`[06]`-`[08]`, engine rows `[01]`/`[05]`, the extension/identifier rows, undefined/fault families — all deferred to first use through the module-scope `lazy` import per the catalogue import policy), `papermill` (`papermill.parameterize.parameterize_notebook(nb, parameters, report_mode, kernel_name)`/`papermill.parameterize.add_builtin_parameters`/`PapermillTranslators`/`PapermillMissingParameterException`/`PapermillExecutionError`, catalogue rows `[03]`/`[04]` resolved from `papermill.parameterize`, the translator-family rows, the exception family), `nbclient` (`NotebookClient`/`async_execute`, `CellExecutionError`/`CellTimeoutError`/`DeadKernelError`, the `timeout`/`startup_timeout`/`kernel_name`/`allow_errors`/`allow_error_names`/`force_raise_errors`/`record_timing`/`interrupt_on_timeout`/`error_on_timeout`/`iopub_timeout`/`raise_on_iopub_timeout`/`coalesce_streams`/`store_widget_state`/`skip_cells_with_tag`/`display_data_priority`/`extra_arguments`/`shutdown_kernel` traits, catalogue config rows `[01]`-`[18]`), `jupytext` (`reads`/`writes`, `fmt` rows `[02]`/`[04]`, the `fmt=None` auto-detect path), `nbconvert` (`get_exporter`/`Exporter.from_notebook_node`/`get_export_names`/`ExporterNameError`/`preprocessors.TagRemovePreprocessor` `remove_cell_tags`/`remove_input_tags`/`remove_all_outputs_tags`, catalogue rows `[01]`/`[02]`, the preprocessor row), `traitlets` (`config.Config` the `ExportPolicy` exporter `config=` projection), `nbformat` (`NotebookNode`), `pymupdf` (`Story(html, user_css, em)`/`Story.place`/`Story.draw`/`DocumentWriter`/`DocumentWriter.begin_page`/`end_page`/`close`/`paper_rect`, catalogue story-layout rows `[01]`/`[04]`/`[06]`-`[10]`), `msgspec` (`Struct`/`field`/`structs.asdict`), `pydantic` (`TypeAdapter`/`ValidationError` the `ReportPayload` admission), runtime (`content_identity.ContentIdentity`/`ContentKey`, `faults.RuntimeRail`/`async_boundary`, `receipts.Receipt`/`receipted`/`OPEN` the keep-all redaction policy, `anyio.to_process.run_sync` the gated subprocess lane for `REFLOW`), `document/model#NODE` (`DocumentNode`/`PageNode`/`SectionNode`/`BlockNode`/`RunNode`/`FigureNode`/`ListNode`/`NodeMeta`/`BlockKind`/`ListKind`/`encode`), `artifacts.core.receipt` (`ArtifactReceipt.Report`/`ArtifactReceipt.Pdf`).
- Growth: a new report kind is one `ReportKind` row plus one `COMPOSE_ARMS` coroutine-row returning a `ReportFact` plus one `_REQUIRED` row if it demands an input; a new section-body unit is one `SectionBlock` case plus one `_block_node` arm (the inline `figure` case is exactly that); a new section attribute is one `Section` field plus one `_section_node` thread (the CSI `classification` onto `NodeMeta` is exactly that); a new loader root is one `ReportLoader` row plus one `match` arm; a new text source is one `ReportSource` row plus one `jupytext` `fmt` value; a new export target is one `ReportExport` row on the `nbconvert.exporters` registry; a new paper size is one `ReflowPaper` row; a new bounded-safety trait is one field on `NotebookEngine`, carried into the client by `client_kwargs` with zero call-site edit; a new export-shaping trait is one field on `ExportPolicy`, carried into the exporter by `exporter_kwargs` with zero call-site edit; a new jinja capability is one `extensions` dotted-path row; a new evidence scalar is one `ReportFact` field; a new admission cause is one `ReportFault` case; zero new surface.
- Boundary: a string-returning report that never produces a node, a figure passed only as a jinja kwarg where the `COMPOSE` fold splices a `FigureNode` into the tree, a `Section.figures` trail parallel to the block flow where the `SectionBlock.figure` case interleaves a figure IN document order, a hand-rolled section-composition loop, a phantom `BlockKind.LIST_ITEM` where the real `ListNode`/`ListKind` owner rows the outline, an inline `match self.kind` ladder where the `COMPOSE_ARMS` table dispatches, a `MappingProxyType` table where `frozendict` owns the dispatch rows, a sibling `render` method beside the one modal `rendered` entry, a lenient default `Environment` that blanks missing keys, a jinja `Environment` with no `extensions` band where a `{% do %}`/`{% break %}` template needs the `jinja2.ext.do`/`loopcontrols` dotted-path row, a sync straggler render, an untrusted-template `Environment` with the sandbox disabled, a module-level EAGER `jinja2`/`jupytext`/`nbconvert`/`nbclient`/`papermill`/`pymupdf` import where the manifest policy demands the module-scope `lazy` deferral, a top-level `papermill.parameterize_notebook` access where the symbol lives in `papermill.parameterize`, a `papermill.inspect_notebook` parameter gate on a path where the in-memory node demands the `ReportPayload`-admitted `ReportParams` band, a second in-arm `TypeAdapter` re-validation of the already-admitted parameters, a bare `dict[str, object]` parameter bag where the closed `ReportParams` band value-types each scalar, a hand-rolled parameter injection outside `parameterize_notebook`, a Python-defaulted `parameterize_notebook` where the `kernel_name` routes the matching `PapermillTranslators` row, a `FunctionLoader` callable on a frozen serializable spec, a bare `str` export knob where the closed `ReportExport` vocabulary resolves through `get_exporter`, an `HTML_EXPORTS` membership table where the output's own `str`-vs-`bytes` type discriminates the leaf, a `NOTEBOOK` export round-trip where the `jupytext.writes` archive already owns the `.ipynb`, a bare `nbconvert` exporter ignoring the `ExportPolicy` tag bands where the one `TagRemovePreprocessor` `config` strips the report's scaffolding cells, a hand-rolled cell-stripping pass outside `TagRemovePreprocessor`, a path-based `papermill.execute_notebook` blocking the async boundary, a `subprocess` shell-out to `jupyter-nbconvert`, a parallel `tuple[ContentKey, ...]` figure channel beside the `FigureRef` value object, a discarded reflowed-PDF buffer where the bytes are the artifact, a hand-built media-box literal where `_box` derives the rect through `paper_rect`, an `ArtifactReceipt.Report` row on the reflowed PDF that drops the page count, an `ArtifactReceipt` channel threaded through the rail payload where the `@receipted` harvest owns it, a `phase` knob on `contribute` where the runtime port takes none, a `frozendict` projected raw into a `traitlets.Dict` trait where `client_kwargs` derives the nbclient boundary dict, a `schema: type[Struct]` carried as a plan value, and a `from __future__ import annotations` header on a py3.15 fence are the deleted forms — the report binds `FigureRef` content keys produced by `visualization/chart#CHART`, `visualization/table#TABLE`, and `scene#SCENE` (never re-rendering them) into the tree, parses the text source through `jupytext`, runs the node on the async `nbclient` kernel lane, exports the executed node in-process through `nbconvert`, reflows HTML through the native `pymupdf.Story` loop into a keyed PDF artifact, and threads each artifact onto one `ReportFact`. The produced `DocumentNode` tree hands to `document/emit#DOCUMENT` for lowering (`PDF_HTML` for the HTML leaves, `PDF_TYPST`/`PDF_AUTHOR` for the structured tree); PAdES/PDF-A finishing routes to `exchange/conformance#CONFORM`; the security-and-navigation finishing close routes to `document/egress#FINISH`; the `REFLOW` kind is this owner's HTML-INTO-PDF authoring half, the directional inverse of the `document/lens#LENS STORY` recover-OUT-of-PDF extraction arm — authoring and extraction are two arms of the one `pymupdf.Story` capability split across the two owners by direction, not a duplicate; no live kernel server, no UI; the content key is consumed from runtime, never re-minted.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
import io
from collections.abc import Awaitable, Callable, Iterable
from copy import replace
from enum import StrEnum
from typing import Final, Literal, NotRequired, ReadOnly, Self, TypedDict, Unpack, assert_never

from anyio import CapacityLimiter, to_process, to_thread
from builtins import frozendict
from expression import Error, Ok, Result, case, tag, tagged_union
from expression.collections import Block
from msgspec import UNSET, Struct, field
from msgspec.json import Encoder
from msgspec.structs import asdict
from pydantic import TypeAdapter, ValidationError

from rasm.runtime.content_identity import ContentIdentity, ContentKey
from rasm.runtime.faults import RuntimeRail, async_boundary
from rasm.runtime.receipts import OPEN, Receipt, receipted

from artifacts.core.receipt import ArtifactReceipt
from artifacts.document.model import (
    BlockKind,
    BlockNode,
    DocumentNode,
    FigureNode,
    ListKind,
    ListNode,
    NodeMeta,
    PageNode,
    RunNode,
    SectionNode,
    encode,
)

lazy import jupytext
lazy import nbconvert
lazy import pymupdf
lazy from jinja2 import (
    BaseLoader, ChoiceLoader, DictLoader, Environment, FileSystemBytecodeCache, FileSystemLoader,
    ModuleLoader, PackageLoader, PrefixLoader, StrictUndefined, select_autoescape,
)
lazy from jinja2.nativetypes import NativeEnvironment
lazy from jinja2.sandbox import ImmutableSandboxedEnvironment
lazy from nbclient import NotebookClient
lazy from papermill.parameterize import add_builtin_parameters, parameterize_notebook
lazy from pdf_oxide import Footer, Header, OfficeConverter, PageTemplate, Pdf
lazy from traitlets.config import Config

# --- [TYPES] ----------------------------------------------------------------------------


class ReportKind(StrEnum):
    COMPOSE = "compose"
    TEMPLATE = "template"
    NOTEBOOK = "notebook"
    REFLOW = "reflow"      # AGPL pymupdf.Story HTML-into-PDF authoring
    AUTHOR = "author"      # MIT/Apache pdf_oxide markdown/HTML/office authoring — the commercial-safe REFLOW peer


class TemplateRender(StrEnum):
    STRING = "string"      # jinja `Environment` -> HTML `BlockKind.CODE` leaf
    NATIVE = "native"      # jinja `NativeEnvironment` -> a computed Python value -> deterministic structured-data leaf


class AuthorSource(StrEnum):
    # the commercial-safe pdf_oxide authoring-source vocabulary; each row binds one `Pdf`/`OfficeConverter`
    # entry in `_AUTHOR_BUILD`, so a new source is one member plus one row, never an `if source == ...` ladder.
    MARKDOWN = "markdown"
    HTML = "html"
    SHEET = "sheet"        # markdown + a running-header/footer `PageTemplate` — the AEC titled-sheet report
    DOCX = "docx"          # `OfficeConverter.from_docx(path)` office-source report
    PPTX = "pptx"
    XLSX = "xlsx"


class ReflowLayout(StrEnum):
    DIRECT = "direct"          # `Story.write` one-shot page sweep
    STABILIZED = "stabilized"  # `Story.write_stabilized` re-lays regenerated HTML until stable, its `add_header_ids=True` default injecting navigable header anchors — the TOC/cross-reference-convergence layout


class ReportLoader(StrEnum):
    DICT = "dict"
    FILESYSTEM = "filesystem"
    PACKAGE = "package"
    PREFIX = "prefix"
    MODULE = "module"


class ReportSource(StrEnum):
    IPYNB = "ipynb"
    MYST = "md:myst"
    PERCENT = "py:percent"
    LIGHT = "py:light"
    MARKDOWN = "md"
    RMD = "Rmd"
    QMD = "qmd"
    AUTO = "auto"


class ReportExport(StrEnum):
    HTML = "html"
    PDF = "pdf"
    WEBPDF = "webpdf"
    LATEX = "latex"
    MARKDOWN = "markdown"
    RST = "rst"
    ASCIIDOC = "asciidoc"
    SLIDES = "slides"
    PYTHON = "python"
    SCRIPT = "script"


class ReflowPaper(StrEnum):
    A4 = "a4"
    A3 = "a3"
    A5 = "a5"
    LETTER = "letter"
    LEGAL = "legal"


type ParamScalar = str | int | float | bool | None
type Rect = tuple[float, float, float, float]
type ComposeArm = Callable[["ReportPlan"], Awaitable["ReportFact"]]

# --- [CONSTANTS] ------------------------------------------------------------------------

# the TOTAL deterministic encoder the `NATIVE` template render lowers a computed Python value through; `enc_hook=str`
# never raises on an unsupported type, so a derived report datum crosses into the keyed body without an interior raise.
_NATIVE: Final[Encoder] = Encoder(enc_hook=str)

# --- [MODELS] ---------------------------------------------------------------------------


# the notebook-parameter value space: any name, every value a kernel-serializable `ParamScalar`,
# admitted once through `_PAYLOAD`. A notebook with a strict declared-parameter set subclasses with
# `Required[]` keys; the bare band rejects a non-scalar value `papermill` could not codify.
class ReportParams(TypedDict, extra_items=ParamScalar):
    pass


class NotebookEngine(Struct, frozen=True):
    # the full nbclient bounded-safety trait set; a new trait is one field carried by `client_kwargs`
    # with zero call-site edit. The lifecycle-hook traits (`on_cell_executed`/...) stay off: a
    # `Callable` is a per-run observation channel the runtime observability owner holds, never a
    # serializable reproducibility fact the keyed plan carries.
    timeout: int | None = 600
    startup_timeout: int = 60
    allow_errors: bool = False
    allow_error_names: tuple[str, ...] = ()
    force_raise_errors: bool = True
    record_timing: bool = True
    interrupt_on_timeout: bool = True
    error_on_timeout: frozendict[str, str] = field(default_factory=frozendict)
    iopub_timeout: int = 4
    raise_on_iopub_timeout: bool = True
    coalesce_streams: bool = True
    store_widget_state: bool = True
    skip_cells_with_tag: str = "skip-execution"
    display_data_priority: tuple[str, ...] = ()
    extra_arguments: tuple[str, ...] = ()
    shutdown_kernel: Literal["graceful", "immediate"] = "graceful"
    kernel_name: str = "python3"

    def client_kwargs(self) -> dict[str, object]:
        # the nbclient boundary view: a `traitlets.Dict` rejects a `frozendict` (no `dict` subclass),
        # so the one map field projects to a real `dict` (empty -> the client's `None` default); the
        # tuple fields ride as-is because `traitlets.List` coerces a tuple.
        return {**asdict(self), "error_on_timeout": dict(self.error_on_timeout) or None}


class ExportPolicy(Struct, frozen=True):
    # the nbconvert exporter-shaping trait set, the export-side peer of the execution-side `NotebookEngine`:
    # the `TagRemovePreprocessor` tag bands strip the report's scaffolding cells/inputs/outputs before the
    # `from_notebook_node` lowering, projected through `exporter_kwargs` so a new shaping trait is one field
    # with zero call-site edit. The preprocessor enables only when a band is non-empty, so a bare export
    # pays no preprocessor pass and a tag set never rides as a serializable `Callable` the keyed plan rejects.
    remove_cell_tags: tuple[str, ...] = ()
    remove_input_tags: tuple[str, ...] = ()
    remove_all_outputs_tags: tuple[str, ...] = ()

    def exporter_kwargs(self) -> dict[str, object]:
        # the nbconvert exporter boundary view: the verified `TagRemovePreprocessor` tag bands enter through
        # the one `traitlets.config.Config` the exporter's `config=` constructor parameter reads, each band a
        # `set` because the traitlets `Set` trait rejects a tuple; an empty band yields no `config` so the
        # exporter constructs with its defaults.
        bands = frozendict({
            "remove_cell_tags": set(self.remove_cell_tags),
            "remove_input_tags": set(self.remove_input_tags),
            "remove_all_outputs_tags": set(self.remove_all_outputs_tags),
        })
        active = {name: value for name, value in bands.items() if value}
        return {"config": Config({"TagRemovePreprocessor": {"enabled": True, **active}})} if active else {}


class FigureRef(Struct, frozen=True):
    # the figure-reference the COMPOSE/TEMPLATE kinds bind by content key, never re-rendering the asset;
    # `media_type`/`intrinsic` ride through so the lowered `FigureNode` carries the producer's MIME and
    # dimensions rather than the model defaults.
    asset_key: ContentKey
    alt: str = ""
    caption: str = ""
    media_type: str = "image/png"
    intrinsic: tuple[float, float] | None = None


@tagged_union(frozen=True)
class SectionBlock:
    # one section-body content unit — a prose block (any `BlockKind`), a list, or an inline figure — so a COMPOSE
    # section INTERLEAVES lists/quotes/code/figures in document order rather than only paragraphs with a figure
    # trail forced after every block; `_block_node` folds each case onto the `document/model#NODE` owner it names,
    # and a new body concern is one case, never a parallel field beside `blocks`.
    tag: Literal["prose", "listing", "figure"] = tag()
    prose: tuple[BlockKind, tuple[str, ...]] = case()      # (PARAGRAPH/HEADING/QUOTE/CODE/CAPTION, body lines)
    listing: tuple[ListKind, tuple[str, ...]] = case()     # (ORDERED/UNORDERED/DESCRIPTION, item texts)
    figure: FigureRef = case()                             # an inline figure placed IN flow -> a `FigureNode` between its sibling blocks


class Section(Struct, frozen=True):
    # the journal-grade + AEC-documentation section: a heading, a rich `SectionBlock` body (prose/list/figure units
    # interleaved in document order), an optional CSI/OmniClass `classification` code landing on the `SectionNode`
    # `NodeMeta.classification`, and nested subsections (`children`) so a report tree carries the full section
    # hierarchy the `_section_node` fold lowers over a structural path, never the flat paragraph-only
    # `body: tuple[str, ...]` slice and never a `figures` trail parallel to the block flow.
    level: int
    heading: str
    blocks: tuple[SectionBlock, ...] = ()
    classification: str = ""                               # CSI/OmniClass code -> `NodeMeta.classification`, the specification/section#SECTION consumer
    children: tuple["Section", ...] = ()


class ReportFact(Struct, frozen=True):
    # the threaded evidence the @receipted harvest reads off `self.fact`: `body` the content-keyed
    # bytes (the encoded tree for COMPOSE/TEMPLATE/NOTEBOOK, the reflowed PDF for REFLOW), `archive`
    # the executed-notebook reproducibility bytes, `pages` the reflow page count, plus the resolved
    # rows and the engine's timing/widget flags the flat-scalar receipt stream never carries.
    node: DocumentNode
    body: bytes
    archive: bytes = b""
    pages: int = 0
    export: ReportExport = ReportExport.HTML
    loader: ReportLoader = ReportLoader.DICT
    report_source: ReportSource = ReportSource.IPYNB
    timed: bool = False
    widgets: bool = False


class ReportSpec(Struct, frozen=True, omit_defaults=True):
    # the one admitted-once carrier materialized at `ReportPlan.of` from the closed `ReportPayload`,
    # never re-validated interior-side; `source` is the per-kind material (template/notebook/HTML),
    # `sections`/`figures` the COMPOSE content.
    source: str = ""
    sections: tuple[Section, ...] = ()
    figures: tuple[FigureRef, ...] = ()
    notebook_parameters: ReportParams = field(default_factory=dict)
    section_templates: frozendict[str, str] = field(default_factory=frozendict)
    template_roots: tuple[str, ...] = ("templates",)
    package: str = ""
    package_path: str = "templates"
    module_path: str = ""
    loader: ReportLoader = ReportLoader.DICT
    trusted: bool = True
    render: TemplateRender = TemplateRender.STRING             # TEMPLATE: `Environment` HTML string vs `NativeEnvironment` computed value
    context: frozendict[str, object] = field(default_factory=frozendict)  # TEMPLATE: the jinja render-context data band spread through `render_async(**context)`
    template_globals: frozendict[str, ParamScalar] = field(default_factory=frozendict)  # TEMPLATE: report-scoped jinja `Environment.globals` injections
    bytecode_cache: str = ""                                   # TEMPLATE: `FileSystemBytecodeCache` directory for repeated reproducible renders
    extensions: tuple[str, ...] = ()                            # jinja2 `Environment(extensions=)` dotted-path rows (`jinja2.ext.do`/`loopcontrols`/`i18n`)
    report_source: ReportSource = ReportSource.IPYNB
    resource_path: str = ""                                    # NOTEBOOK: cwd seeded into `NotebookClient(resources=)` so a relative-asset notebook resolves
    engine: NotebookEngine = field(default_factory=NotebookEngine)
    export: ReportExport = ReportExport.HTML
    export_policy: ExportPolicy = field(default_factory=ExportPolicy)
    paper: ReflowPaper = ReflowPaper.A4
    user_css: str = ""
    em: float = 12.0
    layout: ReflowLayout = ReflowLayout.DIRECT                 # REFLOW: `Story.write` one-shot vs `Story.write_stabilized` convergence
    author_source: AuthorSource = AuthorSource.MARKDOWN        # AUTHOR: the commercial-safe pdf_oxide source kind
    title: str = ""                                            # AUTHOR: pdf_oxide document title metadata
    author: str = ""                                           # AUTHOR: pdf_oxide document author metadata
    header: str = ""                                           # AUTHOR SHEET: running-header center text for the titled sheet
    footer: str = ""                                           # AUTHOR SHEET: running-footer center text
    toc: bool = True
    toc_title: str = "Contents"


# --- [ERRORS] ---------------------------------------------------------------------------


@tagged_union(frozen=True)
class ReportFault:
    # the closed ADMISSION vocabulary `of` produces; every arm-level provider raise
    # (`UndefinedError`/`SecurityError`/`ValidationError`/`PapermillExecutionError`/`CellExecutionError`/
    # `ExporterNameError`) converts to the runtime `BoundaryFault` at the `async_boundary` capsule.
    tag: Literal["payload", "unsatisfied"] = tag()
    payload: tuple[str, ...] = case()              # the rejected ReportPayload key paths
    unsatisfied: tuple[ReportKind, str] = case()   # a kind whose `_REQUIRED` input field is empty


# --- [BOUNDARIES] -----------------------------------------------------------------------


class ReportPayload(TypedDict, closed=True):
    source: NotRequired[ReadOnly[str]]
    sections: NotRequired[ReadOnly[tuple[Section, ...]]]
    figures: NotRequired[ReadOnly[tuple[FigureRef, ...]]]
    notebook_parameters: NotRequired[ReadOnly[ReportParams]]
    section_templates: NotRequired[ReadOnly[frozendict[str, str]]]
    template_roots: NotRequired[ReadOnly[tuple[str, ...]]]
    package: NotRequired[ReadOnly[str]]
    package_path: NotRequired[ReadOnly[str]]
    module_path: NotRequired[ReadOnly[str]]
    loader: NotRequired[ReadOnly[ReportLoader]]
    trusted: NotRequired[ReadOnly[bool]]
    render: NotRequired[ReadOnly[TemplateRender]]
    context: NotRequired[ReadOnly[frozendict[str, object]]]
    template_globals: NotRequired[ReadOnly[frozendict[str, ParamScalar]]]
    bytecode_cache: NotRequired[ReadOnly[str]]
    extensions: NotRequired[ReadOnly[tuple[str, ...]]]
    report_source: NotRequired[ReadOnly[ReportSource]]
    resource_path: NotRequired[ReadOnly[str]]
    engine: NotRequired[ReadOnly[NotebookEngine]]
    export: NotRequired[ReadOnly[ReportExport]]
    export_policy: NotRequired[ReadOnly[ExportPolicy]]
    paper: NotRequired[ReadOnly[ReflowPaper]]
    user_css: NotRequired[ReadOnly[str]]
    em: NotRequired[ReadOnly[float]]
    layout: NotRequired[ReadOnly[ReflowLayout]]
    author_source: NotRequired[ReadOnly[AuthorSource]]
    title: NotRequired[ReadOnly[str]]
    author: NotRequired[ReadOnly[str]]
    header: NotRequired[ReadOnly[str]]
    footer: NotRequired[ReadOnly[str]]
    toc: NotRequired[ReadOnly[bool]]
    toc_title: NotRequired[ReadOnly[str]]


_PAYLOAD: Final = TypeAdapter(ReportPayload)
_OFFLOAD: Final = CapacityLimiter(8)  # the nbconvert export (LaTeX/headless-Chromium subprocess) is blocking native; off the loop
# the per-kind precondition: a kind's named `ReportSpec` field must be non-empty so the interior is total.
_REQUIRED: Final[frozendict[ReportKind, tuple[str, ...]]] = frozendict({
    ReportKind.COMPOSE: ("sections",),
    ReportKind.TEMPLATE: ("source",),
    ReportKind.NOTEBOOK: ("source",),
    ReportKind.REFLOW: ("source",),
    ReportKind.AUTHOR: ("source",),   # markdown/HTML text or an office file path
})

# --- [OPERATIONS] -----------------------------------------------------------------------


def _box(paper: ReflowPaper) -> Rect:
    return tuple(pymupdf.paper_rect(paper.value))


def _meta(role: str, label: str, path: tuple[int, ...], classification: str = "", /) -> NodeMeta:
    # key by the structural PATH (the node's uid per `document/model#NODE` composition law) joined to the label,
    # so two identical-heading siblings under distinct parents never collapse onto one content slot; a section's
    # CSI/OmniClass code rides `NodeMeta.classification` as `UNSET` when empty so an unclassified node's digest
    # stays omit-defaults-stable exactly as `document/model#NODE` mandates.
    trail = "-".join(map(str, path)) or "root"
    return NodeMeta(
        key=ContentIdentity.of(f"report-{role}-{trail}", label.encode()),
        role=role, page=path[0] if path else 0, classification=classification or UNSET,
    )


def _runs(role: str, path: tuple[int, ...], *lines: str) -> tuple[RunNode, ...]:
    return tuple(RunNode(meta=_meta(f"{role}-run", line, path), text=line, font_key="body", size=11.0) for line in lines if line)


def _block_node(path: tuple[int, ...], block: SectionBlock, /) -> DocumentNode:
    # one section-body unit folds onto its `document/model#NODE` owner: a prose block to a `BlockNode` of any
    # `BlockKind`, a list to a `ListNode`/`ListKind` whose items are `LI`-role `BlockNode`s — never a phantom
    # `BlockKind.LIST_ITEM` the model retired, and never the paragraph-only slice the flat `body` field was.
    match block:
        case SectionBlock(tag="prose", prose=(kind, lines)):
            return BlockNode(meta=_meta("block", kind.value, path), block=kind, runs=_runs("body", path, *lines))
        case SectionBlock(tag="listing", listing=(list_kind, items)):
            return ListNode(
                meta=_meta("list", list_kind.value, path),
                list_kind=list_kind,
                items=tuple(
                    BlockNode(meta=_meta("item", item, (*path, ordinal)), block=BlockKind.PARAGRAPH, runs=_runs("item", (*path, ordinal), item))
                    for ordinal, item in enumerate(items)
                ),
            )
        case SectionBlock(tag="figure", figure=ref):  # an inline figure lands as a `FigureNode` at its own path slot, in flow between its siblings
            return FigureNode(
                meta=_meta("figure", ref.alt, path), asset_key=ref.asset_key, alt=ref.alt,
                media_type=ref.media_type, intrinsic=ref.intrinsic, caption=_runs("caption", path, ref.caption),
            )
        case _ as unreachable:
            assert_never(unreachable)


def _section_node(path: tuple[int, ...], section: Section, /) -> SectionNode:
    # the recursive lowering: the rich `SectionBlock` body (prose/list/figure INTERLEAVED in flow), then the nested
    # subsections, every child keyed by its own path extension so the whole hierarchy carries distinct content slots;
    # the section's CSI/OmniClass code rides the `SectionNode` `NodeMeta.classification` the model tree already carries.
    blocks = tuple(_block_node((*path, index), block) for index, block in enumerate(section.blocks))
    kids = tuple(_section_node((*path, len(blocks) + index), child) for index, child in enumerate(section.children))
    return SectionNode(
        meta=_meta("section", section.heading, path, section.classification),
        level=section.level,
        heading=_runs("heading", path, section.heading),
        children=(*blocks, *kids),
    )


def _toc(title: str, sections: tuple[Section, ...], /) -> SectionNode:
    return SectionNode(
        meta=_meta("toc", title, (0,)),
        level=1,
        heading=_runs("toc-heading", (0,), title),
        children=(
            ListNode(
                meta=_meta("toc-list", title, (0,)),
                list_kind=ListKind.ORDERED,
                items=tuple(
                    BlockNode(meta=_meta("toc-entry", section.heading, (index,)), block=BlockKind.PARAGRAPH, runs=_runs("toc-item", (index,), section.heading))
                    for index, section in enumerate(sections)
                ),
            ),
        ),
    )


def _output_page(spec: ReportSpec, role: str, output: str | bytes) -> PageNode:
    # the rendered output's own type is the discriminant: a text render's `str` wraps as a `CODE` HTML/markup or
    # native-value leaf, a binary `PDF`/`WEBPDF` export's `bytes` as a hex `QUOTE` leaf, so an assembled-PDF byte
    # payload never mis-renders as an HTML code block.
    text, block = (output, BlockKind.CODE) if isinstance(output, str) else (output.hex(), BlockKind.QUOTE)
    return PageNode(
        meta=_meta("page", role, (0,)),
        media_box=_box(spec.paper),
        children=(BlockNode(meta=_meta(role, role, (0,)), block=block, runs=_runs(role, (0,), text)),),
    )


async def _compose_arm(plan: "ReportPlan") -> "ReportFact":
    spec = plan.spec
    outline = (_toc(spec.toc_title, spec.sections),) if spec.toc else ()
    page = PageNode(
        meta=_meta("page", plan.kind.value, (0,)),
        media_box=_box(spec.paper),
        children=(*outline, *(_section_node((len(outline) + index,), section) for index, section in enumerate(spec.sections))),
    )
    return ReportFact(node=page, body=encode(page))


def _loader(spec: ReportSpec, /) -> BaseLoader:
    # one closed `match spec.loader` over five serializable loader rows; `FunctionLoader`'s callable provider is
    # deleted because a `Callable` is no frozen-spec value, and the `ChoiceLoader` fallback is applied at the env.
    match spec.loader:
        case ReportLoader.DICT:
            return DictLoader({"<root>": spec.source, **spec.section_templates})
        case ReportLoader.FILESYSTEM:
            return FileSystemLoader(list(spec.template_roots))
        case ReportLoader.PACKAGE:
            return PackageLoader(spec.package, spec.package_path)
        case ReportLoader.PREFIX:
            return PrefixLoader({prefix: DictLoader({"<root>": source}) for prefix, source in spec.section_templates.items()})
        case ReportLoader.MODULE:
            return ModuleLoader(spec.module_path)
        case _ as unreachable:
            assert_never(unreachable)


def _environment(spec: ReportSpec, /) -> Environment:
    # the boundary-scoped engine: `NativeEnvironment` for a computed-value render, else the trusted `Environment`
    # or the untrusted `ImmutableSandboxedEnvironment`, always strict-undefined + async. The report-scoped
    # scientific/AEC `_REPORT_FILTERS`, the spec's serializable `template_globals`, and a deterministic in-template
    # JSON policy install onto the ONE engine (never a second), and a `FileSystemBytecodeCache` directory compiles
    # once for repeated reproducible renders where the prior fence rebuilt bytecode per run.
    factory = NativeEnvironment if spec.render is TemplateRender.NATIVE else Environment if spec.trusted else ImmutableSandboxedEnvironment
    env = factory(
        loader=ChoiceLoader([_loader(spec), DictLoader({})]),
        extensions=list(spec.extensions),
        autoescape=select_autoescape(enabled_extensions=("html", "xml")),
        undefined=StrictUndefined, enable_async=True, trim_blocks=True, lstrip_blocks=True,
        bytecode_cache=FileSystemBytecodeCache(spec.bytecode_cache) if spec.bytecode_cache else None,
    )
    env.filters.update(_REPORT_FILTERS)
    env.globals.update(dict(spec.template_globals))
    env.policies["json.dumps_kwargs"] = {"sort_keys": True}   # deterministic in-template JSON for a reproducible render
    return env


async def _template_arm(plan: "ReportPlan") -> "ReportFact":
    spec = plan.spec
    # one strict-undefined async render; the `context` band spreads the pipeline's bound render values into the
    # jinja context beside `sections`/`figures`, so a missing key is a `jinja2.UndefinedError` fault, never a blank.
    output = await _environment(spec).from_string(spec.source).render_async(sections=spec.sections, figures=spec.figures, **dict(spec.context))
    match spec.render:
        case TemplateRender.STRING:
            page = _output_page(spec, "template", output)                              # the rendered HTML string -> `CODE` leaf
        case TemplateRender.NATIVE:
            page = _output_page(spec, "template", _NATIVE.encode(output).decode())     # the computed Python value -> deterministic JSON leaf
        case _ as unreachable:
            assert_never(unreachable)
    return ReportFact(node=page, body=encode(page), loader=spec.loader)


def _exported(spec: ReportSpec, executed: object, /) -> tuple[str | bytes, object]:
    # the blocking nbconvert render — a PDF/WEBPDF export spawns a LaTeX/headless-Chromium subprocess — crosses
    # the thread seam so it never stalls the loop; the `(output, resources)` pair feeds the `_output_page` partition.
    return nbconvert.get_exporter(spec.export.value)(**spec.export_policy.exporter_kwargs()).from_notebook_node(executed)


def _resources(spec: ReportSpec, /) -> dict[str, object]:
    # the nbclient `resources` dict seeding `metadata.path=cwd` so a notebook referencing a relative asset resolves
    # it against `resource_path`; empty when unset, exactly the one-shot `execute(cwd=)` seed the async rail omits.
    return {"metadata": {"path": spec.resource_path}} if spec.resource_path else {}


async def _notebook_arm(plan: "ReportPlan") -> "ReportFact":
    spec = plan.spec
    fmt = None if spec.report_source is ReportSource.AUTO else spec.report_source.value
    node = jupytext.reads(spec.source, fmt=fmt)
    parameterized = parameterize_notebook(
        node, add_builtin_parameters(dict(spec.notebook_parameters)), report_mode=False, kernel_name=spec.engine.kernel_name,
    )
    executed = await NotebookClient(parameterized, resources=_resources(spec), **spec.engine.client_kwargs()).async_execute()
    archive = jupytext.writes(executed, fmt="ipynb").encode()
    output, _resources = await to_thread.run_sync(_exported, spec, executed, limiter=_OFFLOAD)  # blocking nbconvert render off the loop
    page = _output_page(spec, "notebook", output)
    return ReportFact(
        node=page, body=encode(page), archive=archive, export=spec.export,
        report_source=spec.report_source, timed=spec.engine.record_timing, widgets=spec.engine.store_widget_state,
    )


async def _reflow_arm(plan: "ReportPlan") -> "ReportFact":
    spec = plan.spec
    pdf, count, box = await to_process.run_sync(_reflow, spec.source, spec.user_css, spec.em, spec.paper.value, spec.layout)
    key = ContentIdentity.of(f"report-{plan.kind.value}", pdf)
    page = PageNode(meta=NodeMeta(key=key, role="reflow", page=count), media_box=box)
    return ReportFact(node=page, body=pdf, pages=count)


def _reflow(html: str, user_css: str, em: float, paper: str, layout: ReflowLayout) -> tuple[bytes, int, Rect]:
    # the gated subprocess lane (`anyio.to_process.run_sync`, the GIL-hostile native call): the MuPDF Story lays the
    # whole HTML in ONE `Story.write`/`write_stabilized` entry (the manual `place`/`draw` slice loop retired), a
    # `pagefn` counts pages off the writer, a `positionfn` collects placed-element positions, and `add_pdf_links`
    # injects live PDF links from those positions (a no-op when the HTML has no `<a>`), so the returned bytes are the
    # link-enriched reflowed PDF the receipt keys, never a discarded `getvalue` sink.
    rect = pymupdf.paper_rect(paper)
    buffer = io.BytesIO()
    writer = pymupdf.DocumentWriter(buffer)
    positions: list[object] = []
    pages = 0

    def rectfn(rect_number: int, filled: object, /) -> tuple[object, object, None]:
        return (rect, rect, None)

    def pagefn(page_number: int, mediabox: object, device: object, after: int, /) -> None:
        nonlocal pages  # Exemption: the writer's page callback threads the count through the one imperative page sweep
        if after:
            pages = page_number + 1

    def positionfn(position: object, /) -> None:
        positions.append(position)

    match layout:
        case ReflowLayout.STABILIZED:
            # the static entry builds its own Story and re-lays the regenerated HTML until the layout is stable;
            # `add_header_ids=True` (the parameter default, pinned explicit) injects the navigable header anchors the AEC TOC/cross-reference convergence needs.
            pymupdf.Story.write_stabilized(writer, lambda _prev: html, rectfn, user_css=user_css or None, em=em, positionfn=positionfn, pagefn=pagefn, add_header_ids=True)
        case ReflowLayout.DIRECT:
            pymupdf.Story(html=html, user_css=user_css or None, em=em).write(writer, rectfn, positionfn=positionfn, pagefn=pagefn)
        case _ as unreachable:
            assert_never(unreachable)
    writer.close()
    return pymupdf.Story.add_pdf_links(buffer, positions).tobytes(), pages, tuple(rect)


async def _author_arm(plan: "ReportPlan") -> "ReportFact":
    # the commercial-safe (MIT/Apache) authoring peer of the AGPL `REFLOW` arm: pdf_oxide's Rust core releases the
    # GIL, so it crosses the `to_thread` lane (never `to_process`), returning the pdf bytes and the `len(pdf)` page
    # count the `ArtifactReceipt.Pdf` case lands on exactly as REFLOW does.
    spec = plan.spec
    pdf, count = await to_thread.run_sync(_authored, spec, limiter=_OFFLOAD)
    key = ContentIdentity.of(f"report-{plan.kind.value}", pdf)
    page = PageNode(meta=NodeMeta(key=key, role="author", page=count), media_box=_box(spec.paper))
    return ReportFact(node=page, body=pdf, pages=count)


def _authored(spec: ReportSpec, /) -> tuple[bytes, int]:
    # dispatch the source through the derived `_AUTHOR_BUILD` row to its pdf_oxide entry, then read the bytes and the
    # native page count off the built `Pdf` — one row per source, never an `if source == ...` construction ladder.
    pdf = _AUTHOR_BUILD[spec.author_source](spec)
    return pdf.to_bytes(), len(pdf)


# --- [TABLES] ---------------------------------------------------------------------------

# the report-scoped scientific/AEC formatting filters installed on EVERY report `Environment` (never registered
# per call), so a template renders `{{ load | sigfig }}` / `{{ span | dimension }}` without a bespoke filter map.
_REPORT_FILTERS: Final[frozendict[str, Callable[..., str]]] = frozendict({
    "sigfig": lambda value, digits=3: f"{float(value):.{digits}g}",
    "si": lambda value, unit="": f"{float(value):.3g} {unit}".rstrip(),
    "ratio_pct": lambda value: f"{float(value) * 100:.1f}%",
    "dimension": lambda value, unit="mm": f"{float(value):.0f} {unit}",
})

# the commercial-safe pdf_oxide authoring dispatch: one row per `AuthorSource` binding it to its `Pdf`/
# `OfficeConverter` entry, the SHEET row composing a running-header/footer `PageTemplate` for the AEC titled sheet,
# the office rows reading `spec.source` as a file path — a new source is one member plus one row, zero body edit.
_AUTHOR_BUILD: Final[frozendict[AuthorSource, Callable[[ReportSpec], Pdf]]] = frozendict({
    AuthorSource.MARKDOWN: lambda spec: Pdf.from_markdown(spec.source, title=spec.title or None, author=spec.author or None),
    AuthorSource.HTML: lambda spec: Pdf.from_html(spec.source, title=spec.title or None, author=spec.author or None),
    AuthorSource.SHEET: lambda spec: Pdf.from_markdown_with_template(
        spec.source, PageTemplate().header(Header.center(spec.header)).footer(Footer.center(spec.footer)),
        title=spec.title or None, author=spec.author or None,
    ),
    AuthorSource.DOCX: lambda spec: OfficeConverter.from_docx(spec.source),
    AuthorSource.PPTX: lambda spec: OfficeConverter.from_pptx(spec.source),
    AuthorSource.XLSX: lambda spec: OfficeConverter.from_xlsx(spec.source),
})

COMPOSE_ARMS: Final[frozendict[ReportKind, ComposeArm]] = frozendict({
    ReportKind.COMPOSE: _compose_arm,
    ReportKind.TEMPLATE: _template_arm,
    ReportKind.NOTEBOOK: _notebook_arm,
    ReportKind.REFLOW: _reflow_arm,
    ReportKind.AUTHOR: _author_arm,
})

# --- [COMPOSITION] ----------------------------------------------------------------------


class ReportPlan(Struct, frozen=True):
    kind: ReportKind
    spec: ReportSpec = field(default_factory=ReportSpec)
    fact: ReportFact | None = None

    @receipted(OPEN)  # the runtime keep-all redaction the receipts owner exports (report facts carry no classified field), never a re-minted per-file `Redaction`, exactly as `document/emit#DOCUMENT`/`document/egress#FINISH` ride `OPEN`
    async def _emit(self) -> Self:
        # the thin pure core: thread the arm's `ReportFact` onto the frozen owner and return the
        # stepped `Self` the harvest weave drains; the content key is minted by `_composed` off
        # `fact.body`, never inside the core.
        return replace(self, fact=await COMPOSE_ARMS[self.kind](self))

    def contribute(self) -> Iterable[Receipt]:
        if (fact := self.fact) is None:  # rides the stepped owner the fold returned, never a re-run
            return
        key = ContentIdentity.of(f"report-{self.kind.value}", fact.body)
        match self.kind:
            case ReportKind.REFLOW | ReportKind.AUTHOR:
                yield from ArtifactReceipt.Pdf(key, len(fact.body), fact.pages).contribute()
            case ReportKind.NOTEBOOK:
                yield from ArtifactReceipt.Report(key, len(fact.body)).contribute()
                yield from ArtifactReceipt.Report(ContentIdentity.of("report-notebook-archive", fact.archive), len(fact.archive)).contribute()
            case ReportKind.COMPOSE | ReportKind.TEMPLATE:
                yield from ArtifactReceipt.Report(key, len(fact.body)).contribute()
            case _ as unreachable:
                assert_never(unreachable)

    @classmethod
    def of(cls, kind: ReportKind, /, **raw: Unpack[ReportPayload]) -> Result[Self, ReportFault]:
        try:
            payload = _PAYLOAD.validate_python(raw)
        except ValidationError as fault:
            return Error(ReportFault(payload=tuple(str(error["loc"]) for error in fault.errors())))
        spec = ReportSpec(**payload)
        missing = next((name for name in _REQUIRED.get(kind, ()) if not getattr(spec, name)), None)
        return Error(ReportFault(unsatisfied=(kind, missing))) if missing else Ok(cls(kind=kind, spec=spec))


async def rendered(plans: "ReportPlan | Iterable[ReportPlan]", /) -> RuntimeRail[Block[ContentKey]]:
    block = Block.singleton(plans) if isinstance(plans, ReportPlan) else Block.of_seq(plans)
    return await async_boundary("report.compose", lambda: _composed(block))


async def _composed(block: "Block[ReportPlan]", /) -> Block[ContentKey]:
    stepped = [await plan._emit() for plan in block]
    return Block.of_seq([ContentIdentity.of(f"report-{plan.kind.value}", plan.fact.body) for plan in stepped])
```

## [03]-[RESEARCH]

- [REPORT_INTERIOR_TREE]: every `ReportKind` produces a `document/model#NODE` `DocumentNode`, never a string — the canonical interior the `document/emit#DOCUMENT` axis lowers. `COMPOSE` is the figure-binding kind: `_compose_arm` folds the typed `Section` sequence into a `PageNode` carrying a `SectionNode` per section, each `SectionBlock` folding through `_block_node` in document order — a `prose` unit to a `BlockNode` of its `BlockKind`, a `listing` to a `ListNode`, and a `figure` `FigureRef` into a `FigureNode(asset_key=, alt=, media_type=, intrinsic=)` keyed by the bound content key BETWEEN its sibling blocks — so a figure interleaves in flow (a `FigureNode` the `document/emit#DOCUMENT` lowering and the `document/tagged#ACCESS` audit both read) rather than trailing every block on a parallel `Section.figures` channel or riding a jinja `figures=` kwarg that never splices a node; the section's CSI/OmniClass `classification` rides the `SectionNode` `NodeMeta.classification` (`UNSET` when empty) so the `specification/section#SECTION` CSI author composes this COMPOSE fold rather than a parallel owner, the code landing on the one `document/model#NODE` tree the `specification/classify#CLASSIFY` `ReferenceIndex` resolves over. `_toc` synthesizes the leading table-of-contents `SectionNode` over a real `ListNode(list_kind=ListKind.ORDERED)` whose `BlockNode` items row the heading hierarchy — the `BlockKind.LIST_ITEM` the prior fence cited is a PHANTOM (`document/model#NODE BlockKind` is `PARAGRAPH`/`HEADING`/`QUOTE`/`CODE`/`CAPTION`/`ARTIFACT` only), and the `ListNode`/`ListKind` owner the model now ships is the real `L`/`LI` grouping the audit's list-nesting check and the Typst `#enum` lowering both consume. `TEMPLATE` wraps its strict-rendered HTML and `NOTEBOOK` its `from_notebook_node` output through the shared `_output_page` builder both kinds reuse — a `str` output lands as a `BlockKind.CODE` HTML leaf the `document/emit#DOCUMENT PDF_HTML` weasyprint arm lowers, while a binary `PDF`/`WEBPDF` `bytes` output lands as a `BlockKind.QUOTE` hex leaf — and `REFLOW` returns one `PageNode` over the reflowed-PDF content key carrying the page count and rect. The `PageNode`/`SectionNode`/`BlockNode`/`RunNode`/`FigureNode`/`ListNode`/`NodeMeta`/`BlockKind`/`ListKind` spellings are owned by `document/model#NODE` and composed here, never re-declared.
- [JINJA_STRICT_ASYNC]: the jinja2 catalogue's `[REPORT_TEMPLATING]` law fixes `undefined=StrictUndefined`, `enable_async=True`, and `autoescape=select_autoescape(enabled_extensions=(...))` as the default report-templating policy, so `_template_arm` constructs its `Environment` at boundary scope (the catalogue defers the import to the module-scope `lazy` form) with a loader composed through `ChoiceLoader` and awaits `render_async` — a missing section key raises `jinja2.UndefinedError` rather than blanking, the sync straggler is retired onto `async_boundary`, and the untrusted source rides `ImmutableSandboxedEnvironment` where a forbidden access is a typed `sandbox.SecurityError` on the rail. The loader is one closed `match spec.loader` over `DictLoader`/`FileSystemLoader`/`PackageLoader`/`PrefixLoader`/`ModuleLoader` (five of the catalogue's eight loader rows, `BaseLoader` abstract and `ChoiceLoader` the composition wrapper), each row reading the spec fields it needs — the `FunctionLoader` callable provider the prior fence carried is DELETED because a `Callable[[str], str]` is no serializable value a frozen `ReportSpec` admits (the same law that keeps an nbclient hook off `NotebookEngine`), and `ModuleLoader(spec.module_path)` is the pre-compiled-template row that survives. The `extensions` band is the serializable capability axis the prior fence ignored: `Environment(extensions=list(spec.extensions))` admits the catalogue's `ext.Extension` identifiers as the dotted-path strings (`jinja2.ext.do` expression statements, `jinja2.ext.loopcontrols` `{% break %}`/`{% continue %}`, `jinja2.ext.i18n` locale strings, `jinja2.ext.debug`) a frozen `ReportSpec` admits where the `FunctionLoader` `Callable` cannot — a `tuple[str, ...]` of import paths is a serializable spec value, so a report template needing loop control or expression statements enables it through one band rather than a parallel engine, and a custom `{% %}`-tag filter/global registry stays off the frozen spec for the same callable law. The `Environment(extensions=)`/`Environment.from_string`/`Template.render_async`/`ext.Extension`/`select_autoescape`/`StrictUndefined`/`ChoiceLoader`/`DictLoader`/`FileSystemLoader`/`PackageLoader`/`PrefixLoader`/`ModuleLoader`/`UndefinedError`/`TemplateSyntaxError`/`TemplateNotFound`/`SecurityError` spellings verify against the folder `.api` catalogue for `jinja2` loader-axis rows `[01]`-`[03]`/`[06]`-`[08]`, engine entrypoint rows `[01]`/`[05]`, the extension/identifier rows, and the undefined/fault families.
- [NOTEBOOK_PARAM_GATE]: the notebook parameters are admitted EXACTLY ONCE at `ReportPlan.of` through `_PAYLOAD.validate_python(raw)` over the closed `ReportPayload` whose `notebook_parameters: ReportParams` field is the `TypedDict(extra_items=ParamScalar)` value-space band — every supplied value is type-checked against the kernel-serializable `ParamScalar = str | int | float | bool | None` union before any kernel boots, so a non-scalar value `papermill` could not codify faults at admission. There is NO second in-arm `TypeAdapter` re-validation: re-validating an already-admitted owner violates the boundary law, so `_notebook_arm` reads `spec.notebook_parameters` directly. `papermill.inspect_notebook(notebook_path, parameters)` is NOT the gate — the catalogue's row reads a notebook PATH and starts no kernel, so it cannot gate the in-memory `NotebookNode` the async boundary executes (the prior fence's bare `inspect_notebook` reference was a dead gate). The admitted dict merges the papermill built-ins through `add_builtin_parameters` and injects through `parameterize_notebook(node, ..., report_mode=False, kernel_name=spec.engine.kernel_name)` — the one injection point the catalogue's `[RAIL_LAW]` rejects hand-rolling outside, the `kernel_name` carrying the engine's configured kernel so `parameterize_notebook` resolves the matching `papermill.PapermillTranslators` row (`PythonTranslator`/`RTranslator`/`JuliaTranslator`/`ScalaTranslator`/`BashTranslator`/`CSharpTranslator`/`FSharpTranslator`) and serializes the parameter cell in the kernel's own language rather than defaulting every kernel to Python. A notebook with a strict declared-parameter set supplies a `ReportParams` subclass with `Required[]` keys whose absence faults at `_PAYLOAD`. The `parameterize_notebook(nb, parameters, report_mode, kernel_name, ...)`/`add_builtin_parameters(parameters)`/`PapermillTranslators`/`PapermillMissingParameterException`/`PapermillExecutionError` spellings verify against the folder `.api` catalogue for `papermill` rows `[03]`/`[04]`, the translator-family rows, and the exception family. The path-routed `papermill.execute_notebook` is the rejected sync end-to-end call that blocks the async boundary.
- [NBCLIENT_ENGINE]: the parameterized node executes through `nbclient.NotebookClient(node, **spec.engine.client_kwargs()).async_execute()` — the catalogue's preferred async entrypoint over the in-memory `nbformat.NotebookNode`, returning the mutated node — where `client_kwargs` is the `NotebookEngine` boundary projection: `msgspec.structs.asdict(self)` yields every verified `nbclient.NotebookClient` trait, and the one map field `error_on_timeout` projects from its immutable `frozendict` to a real `dict` (or the client's `None` default) because a `traitlets.Dict` trait rejects a `frozendict`, which is no `dict` subclass — the tuple fields ride as-is because `traitlets.List` coerces a tuple. The trait set covers `timeout`/`startup_timeout`/`kernel_name`/`allow_errors`/`allow_error_names`/`force_raise_errors`/`record_timing`/`interrupt_on_timeout`/`error_on_timeout`/`iopub_timeout`/`raise_on_iopub_timeout`/`coalesce_streams`/`store_widget_state`/`skip_cells_with_tag`/`display_data_priority`/`extra_arguments`/`shutdown_kernel` (the configuration-trait rows `[01]`-`[18]` of the folder `.api` catalogue for `nbclient`) — `extra_arguments` (row `[17]`, extra kernel-launch CLI args) and `shutdown_kernel` (row `[10]`, the `graceful`/`immediate` kernel-shutdown policy) are the two reproducibility-affecting traits the prior fence omitted, each one field carried by `client_kwargs` with zero call-site edit. `record_timing=True` stamps per-cell timing into the executed node's metadata and `store_widget_state=True` captures the widget state for UI replay; `interrupt_on_timeout=True` with `force_raise_errors=True` makes a runaway cell a `CellTimeoutError`/`CellExecutionError` and a kernel death a `DeadKernelError`, each funneled through the boundary into `BoundaryFault`. The lifecycle-hook traits (`on_cell_execute`/`on_cell_executed`/`on_notebook_start`/`on_notebook_complete`/`on_notebook_error`, catalogue rows `[19]`-`[26]`) stay OFF the frozen `NotebookEngine` because a `Callable` is not a reproducibility-receipt fact the content key keys — a hook is a per-run observation channel the runtime observability owns, so admitting one would smuggle a non-serializable runtime capability onto the keyed plan. The `async_execute`/`store_widget_state`/`CellExecutionError`/`CellTimeoutError`/`DeadKernelError` spellings verify against the folder `.api` catalogue for `nbclient` row `[04]`, the config-trait table, and the exception family.
- [NBCONVERT_HTML]: the executed node lowers to document bytes through `nbconvert.get_exporter(spec.export.value)(**spec.export_policy.exporter_kwargs()).from_notebook_node(executed)` — `get_exporter` resolves the `html`/`pdf`/`webpdf`/`latex`/`markdown`/`rst`/`asciidoc`/`slides`/`python`/`script` name row on the `nbconvert.exporters` registry (raising `ExporterNameError` on an unknown name), constructs the exporter under the `ExportPolicy` `config`, and `from_notebook_node(nb)` dispatches the in-memory `NotebookNode` returning the `(output, resources)` pair the catalogue's conversion axis fixes — the in-process path that retires the `subprocess` shell-out to the `jupyter-nbconvert` CLI. The `ExportPolicy` is the export-shaping axis the prior fence omitted: its `remove_cell_tags`/`remove_input_tags`/`remove_all_outputs_tags` bands project through `exporter_kwargs` into the one `traitlets.config.Config({"TagRemovePreprocessor": {"enabled": True, ...}})` the exporter's `config=` constructor parameter reads, so a report strips its scaffolding/setup cells, hides input prompts, or drops noisy outputs before render — the reproducible-report cell-stripping mechanism `TagRemovePreprocessor` owns — each band a `set` because the traitlets `Set` trait rejects a tuple, and an empty band yields no `config` so a bare export pays no preprocessor pass; a hand-rolled cell walk deleting tagged cells outside the preprocessor is the rejected re-implementation, and a `Callable` export hook stays off the frozen `ExportPolicy` for the same serializability law that keeps an nbclient lifecycle hook off `NotebookEngine`. The `output` is the discriminant the `isinstance(output, str)` partition reads: a text exporter (`HTMLExporter`/`SlidesExporter`/`LatexExporter`/`MarkdownExporter`/`RSTExporter`/`ASCIIDocExporter`/`PythonExporter`/`ScriptExporter`) returns a `str` that `_output_page` wraps as a `BlockKind.CODE` leaf feeding `document/emit#DOCUMENT PDF_HTML`, while a binary exporter (`PDFExporter` via LaTeX, `WebPDFExporter` via headless Chromium) returns `bytes` that `_output_page` hex-encodes into a `BlockKind.QUOTE` leaf — the output's OWN type IS the discriminant, so the prior fence's `HTML_EXPORTS` frozenset membership table is DELETED as a restating of what the value already answers (DERIVED_LOGIC). The `NOTEBOOK` round-trip target is dropped: it returns a `NotebookNode` (neither `str` nor `bytes`) the `jupytext.writes(executed, "ipynb")` archive already owns, so it would break the partition. The `resources` dict is the exporter's extracted-output sidecar, bound only when an exporter extracts assets, never a captured receipt fact. The `get_exporter(name, config)`/`Exporter.from_notebook_node(nb, resources, **kw) -> tuple[output, resources]`/`get_export_names`/`ExporterNameError`/`preprocessors.TagRemovePreprocessor` `remove_cell_tags`/`remove_input_tags`/`remove_all_outputs_tags` spellings verify against the folder `.api` catalogue for `nbconvert` module-function rows `[01]`/`[03]`, convert-method row `[02]`, and the preprocessor row, the `traitlets.config.Config` projection the exporter `config=` parameter reads.
- [REFLOW_STORY]: the `REFLOW` kind is the HTML-INTO-PDF authoring direction of the `pymupdf.Story` reflow capability, the directional inverse of the `document/lens#LENS STORY` `LensOp` row that recovers a `PageNode` sequence OUT of an emitted PDF — one capability, two arms split by direction across the producing and recovering owners, never one arm duplicating the other. `_reflow` runs on the runtime subprocess lane (`anyio.to_process.run_sync`, the gated native lane for a GIL-hostile native call per the concurrency `SCOPE_CHOOSER`) over a held `io.BytesIO()` buffer: it opens one `pymupdf.DocumentWriter(buffer)`, builds one `pymupdf.Story(html=, user_css=, em=)`, loops `begin_page(paper_rect)` -> `Story.place(rect) -> (more, filled)` -> `Story.draw(device)` -> `end_page()` until `more == 0`, calls `close()`, and returns `buffer.getvalue()` — the reflowed-PDF bytes the receipt keys as `fact.body`, never a discarded sink whose bytes the prior fence lost — alongside the page count and the rect. The reflow is the one kind whose artifact is a PDF, so `contribute` keys it as `ArtifactReceipt.Pdf(key, len(fact.body), fact.pages)` — the receipt owner's three-field `(ContentKey, byte_count, page_count)` PDF case — where an `ArtifactReceipt.Report` row would silently drop the `count` the reflow loop increments. The paper rect resolves through `pymupdf.paper_rect(ReflowPaper(...).value)` over the named-size vocabulary, the same `_box` derivation every non-reflow `media_box` reads so the `(0, 0, 595, 842)` hand-built literal is deleted everywhere. The `Story(html, user_css, em, archive)`/`Story.place(where, flags) -> (more, filled)`/`Story.draw(device, matrix)`/`DocumentWriter(path, options)`/`DocumentWriter.begin_page(mediabox) -> Device`/`end_page()`/`close()`/`paper_rect(s) -> Rect` spellings verify against the folder `.api` catalogue for `pymupdf` story-layout rows `[01]`/`[04]`/`[06]`-`[10]`. The report owns the authoring arm; the lens owns the recovering arm; neither is the other's deleted form.
- [RECEIPT_AND_ENTRY]: the bound tree and the executed-notebook archive return as `ArtifactReceipt.Report(key, byte_count)` rows and the reflowed PDF as the `ArtifactReceipt.Pdf(key, byte_count, page_count)` row the `core/receipt#RECEIPT` fold consumes — the prior fence's `_emit` threading `tuple[ContentKey, tuple[ArtifactReceipt, ...]]` through the rail payload is DELETED: `_emit` is now the thin pure core returning the stepped `Self`, the `@receipted(OPEN)` weave drains `ReportPlan.contribute` off the threaded `self.fact`, and `_composed` reads each `plan.fact.body` to mint the `ContentKey` block on the one `RuntimeRail`. `contribute` is the no-phase `ReceiptContributor` port `core/receipt#RECEIPT` declares — the prior fence's receipts-in-the-rail-payload and the singular `render` method beside the batch `rendered` are both COLLAPSED, the one `rendered(plans: ReportPlan | Iterable[ReportPlan])` entry discriminating arity on the input shape (a lone plan `Block.singleton`, an iterable `Block.of_seq`, normalized once) and threading through one `async_boundary("report.compose", ...)`, the exact `document/emit#DOCUMENT produced` shape. The `ArtifactReceipt.Report(key, byte_count)` and `ArtifactReceipt.Pdf(key, byte_count, page_count)` cases are owned by `core/receipt#RECEIPT` and composed here; the resolved loader/source/export row and the engine flags stay recoverable from the keyed `ReportFact` rather than a parallel fact map; the content key is consumed from runtime, never re-minted.

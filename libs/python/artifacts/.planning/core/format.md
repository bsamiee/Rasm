# [PY_ARTIFACTS_FORMAT]

The declarative one-context-to-many-formats template engine binding a single structured context into a closed output vocabulary, owning NO producer of its own. `TemplatePipeline` is ONE owner discriminating output format over the `TemplateTarget` closed `StrEnum` AND modality over `TemplateTarget | Iterable[TemplateTarget]` — singular renders one format, plural fans the SAME bound owner across many formats inside one `anyio` task-group failure boundary, never a per-format pipeline class and never a sheet-set helper beside the singular path. The `DocumentNode` tree `pipeline.node` is the universal structural carrier every emit-delegated format lowers from — the same bound tree egresses as a paginated document, a machine-readable structured-data sidecar, AND a spreadsheet schedule (its `TableNode` grid lowered into a workbook) — so one bound QTO/schedule/spec context reaches the whole publication AND AEC-documentation output set; `pipeline.bindings` is the late-bound data-injection band BOTH the typst `sys.inputs` PDF arm AND the jinja HTML `ReportSpec.context` render band read (a render binding never a docxtpl context, since the emit docxtpl arm derives its context from the tree), so one bound `{key: value}` band reaches every data-injecting egress at once. Every `TemplateTarget` DELEGATES to the sibling owner that owns its producer: the `DOCX`/`PDF`/`PPTX`/`ODF` document forms, the `ODS`/`XLSX` spreadsheet-schedule forms, the `XML`/`YAML`/`TOML` structured-data forms, the `MARKDOWN`/`LATEX` plain-text manuscript forms (the docs-as-code CommonMark/GFM and journal-submission LaTeX egress of the SAME bound tree), and the `TYPST_QUERY`/`TYPST_EVAL` introspection-sidecar forms (the bound typst document queried by `<label>` selector or evaluated by expression into a structured JSON/YAML schedule extract) construct a `document/emit#DOCUMENT` `DocumentPlan` over a `DocumentMode` — the `ODF` form riding the emit owner's `DocumentMode.ODT` odfpy `OpenDocumentText` arm and the `ODS` form its `DocumentMode.ODS` odfpy `OpenDocumentSpreadsheet` arm (the `XLSX` form the emit openpyxl/xlsxwriter workbook), because `document/emit#DOCUMENT` IS the document-lowering owner and already declares every one, so the pipeline NEVER re-authors ODF/spreadsheet — while the `HTML` form constructs a `document/report#REPORT` `ReportPlan` over `ReportKind.TEMPLATE`. The pipeline mints no `jinja2.Environment`, no `DocxTemplate`, no typst `Compiler`, no `python-pptx` `Presentation`, no odfpy `OpenDocument`, and no xlsxwriter `Workbook` — it binds the owner once and constructs the owning producer's plan, a pure bind-and-route layer with zero owned engine.

Raw binding material is admitted EXACTLY ONCE: `TemplatePipeline.of(node, **payload)` validates the keyword material through the closed `TemplatePayload` `TypedDict` and its module-level `_PAYLOAD` `TypeAdapter`, folding the `extra_items=object` heterogeneous render band into the frozen `bindings` `frozendict` evidence and materializing every declared key (`template`/`figures`/`assets`/`variant`/`sheet`/`loader`/`trusted`) onto a typed field of the ONE frozen owner — there is no separate `TemplateContext` wrapper, the admitted owner IS the dispatcher — so a malformed payload rails as `Error("<invalid-payload>")` and the interior is total over the admitted owner, never a `dict[str, object]` bag a binding arm re-coerces and never a raw constructor a caller fills by hand. The delegated plans are constructed DIRECTLY from the already-admitted owner (`DocumentPlan(mode=..., node=pipeline.node, spec=EmitSpec(...))`, `ReportPlan(kind=..., spec=ReportSpec(...))`) — trusted construction, never a second `DocumentPlan.of`/`ReportPlan.of` re-validation of values the seam already proved. The `assets` band is the `asset_key.hex -> path` figure map threaded into the office arms' `EmitSpec.assets` so the same bound figures embed across `DOCX`/`PPTX`/`ODF`; the `sheet` band is the one `SheetProfile` the `ODS`/`XLSX` schedule targets project through `_sheet_spec`, the report owner reading the typed `figures: tuple[FigureRef, ...]` for `HTML`. Every arm returns `RuntimeRail[ContentKey]` — the delegated producer's own head key, threaded unchanged, never cracked to `.value` and re-wrapped. The plural fan-out threads the per-target keys through the runtime `faults#FAULT` `traversed` fail-fast reducer into `RuntimeRail[Block[ContentKey]]`, so a spec-sheet rendered to `DOCX`, `PDF`, and `ODS` is ONE bound owner driven three ways under one cancellation scope, its first delegated fault cancelling the siblings.

Receipt ownership stays wholly with the producing sibling: `TemplatePipeline` mints and carries NO receipt because it owns no producer whose bytes it would key. Every emit-delegated arm's `ArtifactReceipt` is weave-emitted by the emit owner's `produced` entrypoint through the `@receipted` harvest on `DocumentPlan._emit` (the `Office` row for `DOCX`/`PPTX`/`ODF`/`ODS`/`XLSX`/structured-text, the `Pdf` row for the typst `PDF` arm), and the `HTML` arm's `Report` receipt is weave-emitted by the report owner's `rendered` entrypoint through its own `@receipted` harvest — so every receipt is already on the runtime `Signals` stream the `core/plan#PLAN` planner and the `observability/metrics#METRIC` `MeterProvider` read, and the pipeline threads back only the delegated head `ContentKey`. There is no `contribute` fold and no `Keyed` receipt-carrying pair: neither `produced` nor `rendered` returns receipts in-band, so an in-band drain would be a fold over empty tuples — the receipt seam is the delegated owner's weave, never a re-surfacing rail here.

## [01]-[INDEX]

- [01]-[FORMAT]: the output-format-and-modality dispatch axis binding one `TemplatePipeline` owner through the `DELEGATES` mode table and the `TARGETS` arm table into the owning producer — the `TemplatePayload` closed `TypedDict` plus the module-level `_PAYLOAD` `TypeAdapter` admitting raw keyword material once into the frozen owner through `TemplatePipeline.of`, the `TemplateTarget` closed output vocabulary (`DOCX`/`PDF`/`HTML`/`PPTX`/`ODF` document forms, the `ODS`/`XLSX` spreadsheet-schedule forms, the `XML`/`YAML`/`TOML` structured-data forms, and the `TYPST_QUERY`/`TYPST_EVAL` introspection-sidecar forms the delegated emit owner lowers the same `DocumentNode` tree to), the one frozen `TemplatePipeline` owner whose `node` is the structural carrier and `bindings` is a `frozendict` data-injection band not a `dict[str, object]` bag, the `SheetProfile` spreadsheet-egress policy the schedule targets project through `_sheet_spec` and the `QueryProfile` typst-introspection policy the `TYPST_QUERY`/`TYPST_EVAL` targets project through `_query_spec`, the fused `DELEGATES` `frozendict[TemplateTarget, (DocumentMode, EmitSpec-builder)]` collapsing the eleven `DocumentPlan` arms (`DOCX`/`PDF`/`PPTX`/`ODF`/`ODS`/`XLSX`/`XML`/`YAML`/`TOML`/`TYPST_QUERY`/`TYPST_EVAL`) into one derived row whose mode and spec builder cannot drift, the `TARGETS` `frozendict[TemplateTarget, Bind]` carrying only the one arm `DELEGATES` cannot — the `HTML` `ReportPlan` row, the report owner being a distinct producer from the emit owner, the `RuntimeRail[ContentKey]` payload every arm threads (the delegated producer's own head key, the receipt owned by that producer's `@receipted` weave, never re-surfaced here), and the `bound` modal entrypoint discriminating `TemplateTarget | Iterable[TemplateTarget]` so the sheet-set family is one `start_soon`/`TaskHandle.return_value` fan-out not a sibling.

## [02]-[FORMAT]

- Owner: `TemplatePipeline` the one frozen `msgspec.Struct(frozen=True)` format-and-modality dispatch owner — admitted once, holding the bound material AND the dispatch, no second wrapper: the `node: DocumentNode` structural tree every emit-delegated format lowers, the `bindings: frozendict[str, object]` data-injection band (NOT a `dict[str, object]` bag) feeding the typst `sys.inputs` data-bind, the typed `figures: tuple[FigureRef, ...]` the HTML delegate splices, the `assets: frozendict[str, str]` `asset_key.hex -> path` figure band threaded into the office arms' `EmitSpec.assets`, the `template: str` source (the docxtpl/`python-pptx`/`.odt` template path, the `jinja2` `from_string` source), the `sheet: SheetProfile` spreadsheet-egress policy the `ODS`/`XLSX` schedule targets bind, and the `variant: PdfVariant`/`loader`/`trusted` policy values — ONE owner every target binds, never a per-format parameter bag, never the parallel `context`/`sections` two-dict split a delegate kwarg-rename would smuggle in, and never the `TemplateContext`-wrapped-by-`TemplatePipeline` one-field indirection a separate value object would re-introduce; `SheetProfile` the frozen `msgspec.Struct` collapsing the four emit sheet knobs (`sheet` name / `header` toggle / `column_width` / `regime` override) into ONE spreadsheet-egress value the schedule targets project, never four flat pipeline fields; `QueryProfile` the frozen `msgspec.Struct` collapsing the typst-introspection knobs (`selector` / `field` / `one` for the query, `expression` for the eval) into ONE value the `TYPST_QUERY`/`TYPST_EVAL` targets project through `_query_spec`, never four flat pipeline fields, so a bound QTO/schedule context egresses ALSO as a queryable structured sidecar (every `<schedule-row>` label extracted to JSON/YAML) beside its rendered forms; `TemplateTarget` the closed `StrEnum` over the output formats (the `DOCX`/`PDF`/`HTML`/`PPTX`/`ODF` document forms, the `ODS`/`XLSX` spreadsheet-schedule forms, the `XML`/`YAML`/`TOML` structured-data forms, and the `TYPST_QUERY`/`TYPST_EVAL` introspection-sidecar forms), one row per output, never a per-format class; `TemplatePayload` the closed `TypedDict(extra_items=object)` ingress contract with `Required[ReadOnly[]]` `template` plus `NotRequired[ReadOnly[]]` `figures`/`assets`/`variant`/`sheet`/`query`/`loader`/`trusted`, admitted exactly once through the module-level `_PAYLOAD` `TypeAdapter` at `TemplatePipeline.of` so the heterogeneous render kwargs fold into the `bindings` band and a malformed payload faults at the seam, never an interior re-validation; `DELEGATES` the fused `frozendict[TemplateTarget, (DocumentMode, EmitSpec-builder)]` binding each `DOCX`/`PDF`/`PPTX`/`ODF`, spreadsheet `ODS`/`XLSX`, structured-data `XML`/`YAML`/`TOML`, and introspection `TYPST_QUERY`/`TYPST_EVAL` target to BOTH its `document/emit#DOCUMENT` `DocumentMode` AND its `EmitSpec` builder (`_office_spec`/`_typst_spec`/`_sheet_spec`/`_query_spec`/`_bare_spec`) in ONE row so the mode and the spec can never drift, the eleven emit arms ONE derived dispatch rather than eleven sibling functions (the gated `XML` row riding the emit owner's own `to_process` band transparently, the binding never re-deriving the subprocess hop the emit mode internalizes); `TARGETS` the `frozendict[TemplateTarget, Bind]` carrying only the one arm `DELEGATES` cannot — the `HTML` `ReportPlan` row, the table left open so a future distinct-producer target lands as one row; `DocumentPlan`/`EmitSpec`/`ReportPlan`/`ReportSpec` the `document/emit#DOCUMENT`/`document/report#REPORT` owners the pipeline composes for every target, constructed directly from the trusted owner and minting no engine of its own; the bind-and-route layer between a structured owner and the format-owning producers.
- Cases: the `DOCX`/`PDF`/`PPTX`/`ODF` template-bind arms, the `ODS`/`XLSX` spreadsheet-schedule arms, and the `XML`/`YAML`/`TOML` structured-text arms collapse into ONE `_via_document` derived dispatch — `DELEGATES[target]` resolves the `DocumentMode` (`DOCX_TEMPLATE`/`TYPST_DATA`/`PPTX`/`ODT`/`ODS`/`XLSX`/`XML`/`YAML`/`TOML`/`TYPST_QUERY`/`TYPST_EVAL`) AND its `EmitSpec` builder in ONE row so the two cannot drift: `_typst_spec` projects `sys_inputs=` the total-coerced `bindings` plus `variant=pipeline.variant` for the typst data-bind `PDF`, `_office_spec` `template=pipeline.template` plus `assets=pipeline.assets` for the `DOCX`/`PPTX`/`ODF` office arms so the bound template and the figure band reach docxtpl/`python-pptx`/odfpy alike, `_sheet_spec` the `SheetProfile` sheet knobs (`sheet`/`header`/`column_width`/`regime`) for the `ODS`/`XLSX` schedule arms so a bound `TableNode` grid egresses as a named, header-styled workbook (`regime=None` letting the emit XLSX arm auto-select in-memory vs streamed by row count), `_query_spec` the introspection builder projecting the `QueryProfile` selection knobs (`selector`/`field_name`/`one`/`expression`) for the `TYPST_QUERY`/`TYPST_EVAL` targets so the bound `node` (lowered to typst source exactly as the PDF arm lowers it) egresses as a queried structured sidecar, and `_bare_spec` the bare default for the node-lowered structured-text AND `MARKDOWN`/`LATEX` manuscript arms (the CommonMark/GFM and journal-submission LaTeX egress of the same bound tree, carrying no template/sheet/query knob) — so one `DocumentPlan(mode=mode, node=pipeline.node, spec=spec_of(pipeline))` is threaded through the emit owner's `produced` entrypoint and `rail.map(lambda keys: keys.head())` carries the head `ContentKey` (the emit `@receipted` weave already owns that arm's row), never a second engine, never eleven near-identical arm functions, never a phantom `params=` constructor, and never an owned odfpy/xlsxwriter re-author where `DocumentMode.ODT`/`ODS`/`XLSX` already lower the tree; `_html` binds the owner into a `document/report#REPORT` `ReportPlan(kind=ReportKind.TEMPLATE, spec=ReportSpec(source=pipeline.template, figures=pipeline.figures, context=pipeline.bindings, loader=pipeline.loader, trusted=pipeline.trusted))` — threading `pipeline.bindings` into the report owner's `ReportSpec.context` band it spreads through `render_async(..., **dict(spec.context))`, so the HTML arm carries the same data-injection the tree-lowered arms do — and AWAITS the report owner's `rendered` modal entry — which returns the same `RuntimeRail[Block[ContentKey]]` shape as `produced`, so the arm reads `keys.head()` exactly as `_via_document` does, never a bare `ReportPlan.render` (the report owner declares none) nor a flattened `ReportPlan(source=, sections=, ...)` constructor (`ReportPlan` takes `kind`+`spec`), the HTML render being the report owner's `TEMPLATE`-kind strict-undefined jinja `Environment` over the `ReportLoader` axis, never a second `Environment`.
- Modality: `TemplatePipeline.bound` is the one polymorphic entrypoint over `target: TemplateTarget | Iterable[TemplateTarget]`, two `@overload` arms carrying the per-modality output shape (`TemplateTarget -> RuntimeRail[ContentKey]`, `Iterable[TemplateTarget] -> RuntimeRail[Block[ContentKey]]`) so a caller narrows on the input it passes rather than the runtime union, the body normalizing once at the head through one structural `match` — the lone `TemplateTarget()` arm read FIRST so a `StrEnum` target (itself iterable) never shatters through the `stream` catch-all that folds any `tuple`/`frozenset`/iterable uniformly through `Block.of_seq` — so arity is a property of the value, never a `batch`/`many` knob and never a `tuple()`/`frozenset()` arm beside an identical catch-all. A lone target awaits the single `_dispatch` rail directly; a target family fans the SHARED owner across one `anyio.create_task_group` — each format an independent `group.start_soon(self._dispatch, target)` returning a `TaskHandle[RuntimeRail[ContentKey]]`, the child rails its own faults so the group exits clean and every handle settles `FINISHED`, and the post-scope `handles.map(lambda handle: handle.return_value)` reads each carrier with no re-raise — then threads the `Block[RuntimeRail[ContentKey]]` through the runtime `faults#FAULT` `traversed` fail-fast reducer into one `RuntimeRail[Block[ContentKey]]`, the first delegated fault cancelling the siblings inside the boundary. The child carrier rides the handle the task group already owns (the concurrency `CHILD_CARRIER` law), never a `create_memory_object_stream` result-gather rig re-implementing it. `_dispatch` is the per-target capsule — `DELEGATES` resolves a `DocumentPlan` threaded through the emit owner's `produced` entrypoint, else `TARGETS` resolves the HTML `ReportPlan` threaded through the report owner's `rendered` entrypoint — returning the producer's `RuntimeRail[ContentKey]` head key threaded unchanged.
- Auto: the pipeline binds ONE structured owner and routes it — the `DOCX` arm reaches docxtpl `DocxTemplate.render` through the emit owner's `DOCX_TEMPLATE` mode lowering `node` (the docxtpl context is the emit owner's node walk, the `template`/`assets` the bound `.docx` template and figure band), the `PDF` arm reaches typst `Compiler.compile_with_warnings(sys_inputs=..., pdf_standards=variant.typst)` through the emit owner's `TYPST_DATA` mode lowering `node` to typst source with `bindings` total-coerced (through the `_INPUT` `enc_hook=str` encoder, never raising) into `EmitSpec.sys_inputs` and `variant` selecting the archival `PdfVariant`, the `HTML` arm reaches the report owner's `TEMPLATE`-kind strict-undefined jinja `Environment.from_string(source).render_async(figures=figures)` over the `ReportLoader` axis through `rendered`, the `PPTX` arm reaches `python-pptx` `Presentation` template-clone through the emit owner's `PPTX` mode, the `ODF` arm reaches the emit owner's odfpy `OpenDocumentText` author through the `ODT` mode lowering the SAME `node` tree to `.odt`, the `ODS`/`XLSX` arms reach the emit owner's `OpenDocumentSpreadsheet` (odfpy) / `Workbook` (openpyxl/xlsxwriter) authors through the `ODS`/`XLSX` modes lowering the `node`'s `TableNode` grid into a `SheetProfile`-named workbook so a bound QTO/schedule context egresses as a spreadsheet schedule, and the `XML`/`YAML`/`TOML` arms reach the emit owner's modes lowering the bound `node` tree through lxml `etree.tostring` / ruamel-yaml `YAML().dump` / tomlkit `dumps` so the same bound tree also egresses as a machine-readable structured-data sidecar, and the `TYPST_QUERY`/`TYPST_EVAL` arms reach the emit owner's `TYPST_QUERY`/`TYPST_EVAL` modes lowering the bound `node` to typst source and then `typst.query(input=source, selector, field, one)` / `typst.eval(input=source, expression)` so the same bound context egresses ALSO as a queried structured schedule sidecar (matched `<label>` elements as JSON/YAML) or a computed evaluation. The pipeline constructs no engine for any target. The `node` tree is the structural collapse: one immutable `DocumentNode` drives every emit lowering (paginated document, spreadsheet schedule, and structured-data sidecar alike), while `bindings` is the one data-injection band BOTH the typst `sys.inputs` PDF arm AND the jinja HTML `ReportSpec.context` render band read ([03]-[HTML_CONTEXT], now landed on the report owner), so a spec-sheet rendered to `DOCX`, `PDF`, and `ODS` in one `bound((DOCX, PDF, ODS))` call is one owner bound three ways through three `_dispatch` arms rather than three parameter bags and three call sites.
- Receipt: `TemplatePipeline` mints and carries NO receipt and imports no `ArtifactReceipt` — there is no owned producer whose bytes it would key, and the bind-and-route layer adds no case to the `core/receipt#RECEIPT` family. Ownership stays wholly with the producing sibling's `@receipted` weave: every emit-delegated arm's `ArtifactReceipt` (`Office` for `DOCX`/`PPTX`/`ODF`/`ODS`/`XLSX`/structured-text, `Pdf` for the typst `PDF` arm) is harvested by the emit owner's `produced` entrypoint off `DocumentPlan._emit`, and the `HTML` arm's `Report` receipt is harvested by the report owner's `rendered` entrypoint off `ReportPlan._emit` — so every receipt is already on the runtime `Signals` stream the `core/plan#PLAN` planner and the `observability/metrics#METRIC` `MeterProvider` consume, and this arm re-surfaces none. There is no `contribute` fold and no `Keyed` receipt-carrying pair: neither `produced` nor `rendered` returns receipts in-band (both use the weave), so an in-band drain would fold over empty tuples — the illusion the prior fence carried after the report owner moved off its in-band `render`. The resolved `TemplateTarget`, its delegated `DocumentMode`/`ReportKind`, and the bound material stay recoverable from the `TemplatePipeline` value, never a parallel fact map and never a dead descriptor struct beside the dispatch.
- Fault: fault capture rides the per-arm fences the rail owner mints, not an invented entrypoint weave — every arm returns a `produced`/`rendered` rail already fenced by the delegate's own `async_boundary` (with the delegate's contract arm folded innermost), so a docxtpl/typst/jinja2/odfpy/xlsxwriter binding failure rails as `BoundaryFault` rather than raises, surfaced unchanged through the task-group edge. The plural `_fanned` runs inside one `anyio.create_task_group` whose `__aexit__` converts a child raise into the group edge, and the runtime `faults#FAULT` `traversed` reducer short-circuits the gathered rails on the first `Error`; the all-clean `handle.return_value` read is reached only when every child settled `FINISHED`, because a child raise would have propagated the group's `BaseExceptionGroup` before the post-scope read. The pipeline weaves no `async_boundary`, no span, and no retry of its own — it owns no engine to fence, the in-process bind carries no transient provider, and the delegated owners already fence and span their work, so re-deriving any of them here would duplicate a concern the owning sibling holds.
- Packages: the pipeline COMPOSES the sibling owners and cites their binding surfaces, owning none — `document/emit#DOCUMENT` (`DocumentPlan`/`DocumentMode`/`EmitSpec`/`PdfVariant`/`XlsxRegime` the `DOCX`/`PDF`/`PPTX`/`ODF`/spreadsheet/structured-text delegate constructed directly and threaded through the `produced` modal entrypoint, the receipt owner for every emit-delegated arm; `DocumentMode.DOCX_TEMPLATE`/`TYPST_DATA`/`PPTX`/`ODT`/`ODS`/`XLSX`/`XML`/`YAML`/`TOML`/`TYPST_QUERY`/`TYPST_EVAL` the resolved rows, `EmitSpec.sys_inputs`/`variant`/`template`/`assets`/`sheet`/`header`/`column_width`/`spreadsheet`/`selector`/`field_name`/`one`/`expression` the per-mode fields, the docxtpl/typst/`python-pptx`/odfpy/openpyxl/xlsxwriter/lxml/ruamel-yaml/tomlkit engines all held inside the emit owner), `document/report#REPORT` (`ReportPlan`/`ReportSpec`/`ReportKind.TEMPLATE`/`ReportLoader`/`FigureRef`/`rendered` the HTML delegate threaded through the report owner's `rendered` modal entrypoint, the `jinja2` `Environment` engine and the `@receipted` `Report`-receipt weave held inside the report owner), `document/model#NODE` (`DocumentNode` the structural tree the bound owner carries and every delegate lowers); `anyio` (`create_task_group`/`start_soon` returning the `TaskHandle[RuntimeRail[ContentKey]]` whose `return_value` carries each child's rail back, the fan-out failure boundary the plural modality runs inside — no `to_thread`/offload of its own, the pipeline owning no blocking author); `expression` (`Block`/`Block.of_seq`/`Block.map` the normalized target family and the per-child carrier map, `Result`/`Ok`/`Error` the `of` admission rail); `pydantic` (`TypeAdapter` the module-level `_PAYLOAD` admission gate over the `TemplatePayload` `TypedDict`, `ValidationError` the seam fault mapped to `<invalid-payload>`); `msgspec` (`Struct(frozen=True)`/`field` the `TemplatePipeline` and `SheetProfile` owners, `json.Encoder(enc_hook=str)` the TOTAL typst `sys.inputs` coercion that never raises outside the emit boundary); `builtins.frozendict` (the fused `DELEGATES` route table, the `TARGETS` arm table, the `bindings` data band, and the `assets` `asset_key.hex -> path` figure band, never a `MappingProxyType` view over a mutable dict and never a `dict[str, object]` interior bag); runtime (`content_identity.ContentKey` the keyed result each arm threads, `faults.RuntimeRail` the rail type and `faults.traversed` the branch-canonical `Block[RuntimeRail[T]]`-to-`RuntimeRail[Block[T]]` fail-fast fan-out reducer). No new external library, no directly-imported producer, and no `core/receipt#RECEIPT` import — every engine and every receipt is held inside the emit or report owner the pipeline composes.
- Growth: a new output format with a `DocumentPlan` arm is one `TemplateTarget` row plus one fused `DELEGATES` route row carrying its `DocumentMode` AND `EmitSpec` builder (reusing `_office_spec`/`_sheet_spec`/`_bare_spec`/`_typst_spec`, or one new builder only for a genuinely distinct spec shape — the `ODS`/`XLSX` spreadsheet-schedule targets and the `TYPST_QUERY`/`TYPST_EVAL` introspection-sidecar targets are exactly that growth, each pair two `TemplateTarget` rows plus two `DELEGATES` rows reusing one builder — `_sheet_spec` for the spreadsheet pair, `_query_spec` for the introspection pair, both projecting one collapsed value object — never a fifth engine); a new output format with a distinct producer (a sibling owner neither emit nor report) is one `TemplateTarget` row plus one `TARGETS` arm returning a `RuntimeRail[ContentKey]`; a new declared ingress key is one `TemplatePayload` `NotRequired[ReadOnly[]]` line plus one `TemplatePipeline` field plus one `of` materialization branch; a new spreadsheet knob is one `SheetProfile` field reaching `_sheet_spec` with zero arm edit; a new introspection knob is one `QueryProfile` field reaching `_query_spec` with zero arm edit; a new template source is one `template` value; a new bound render value rides the `extra_items=object` band with zero schema edit, folded into `bindings` automatically; a new embeddable figure is one `assets` band entry threaded into the office arms with zero arm edit; a new archival PDF profile is one `PdfVariant` value threaded into the `TYPST_DATA` delegate; a new loader root is one `ReportLoader` value on the HTML delegate; a new sandbox policy is the `trusted` flag the report owner's jinja `Environment` reads; a new format batched into the sheet-set family is automatic — the modal `bound` already fans any `TemplateTarget` family with zero edit. A richer ODF/spreadsheet lowering is NOT a growth site here — it is a growth site on `document/emit#DOCUMENT`'s `ODT`/`ODS`/`XLSX` arms, which the pipeline reaches by delegation. Zero new surface — the pipeline grows by target row, always delegating to the sibling owner that holds the producer.
- Boundary: no `jinja2.Environment`, no `DocxTemplate`, no typst `Compiler`, no `python-pptx` `Presentation`, and no odfpy `OpenDocument` construction of its own — the `document/emit#DOCUMENT` `DocumentPlan` owns the docxtpl/typst/`python-pptx`/odfpy/structured-text arms and the `document/report#REPORT` `ReportPlan` owns the jinja2 arm, and `TemplatePipeline` binds the owner and constructs them, owning NO engine. The deleted forms: an owned `_odf_lower` odfpy `OpenDocumentText` recursion (the `attach`/`inject`/`emphasis`/`direction` fold, the `_odf_keyed`/`_cell_runs` helpers, the `to_thread` offload, the `ArtifactReceipt.Office(key, len(blob))` mint) justified by the illusory claim "no emit/report owner declares an ODF WRITE arm; a phantom emit ODF `DocumentMode` does not exist" — `document/emit#DOCUMENT` DECLARES `DocumentMode.ODT`/`ODS` and lowers the same `DocumentNode` tree to `.odt`/`.ods`, so an owned arm is a double-owned re-implementation of the document-lowering owner's job and the `ODF` target collapses into the `DELEGATES` row `ODF -> ODT`; an owned xlsxwriter `Workbook`/odfpy `OpenDocumentSpreadsheet` spreadsheet author where `document/emit#DOCUMENT` `DocumentMode.ODS`/`XLSX` already lower the `node`'s `TableNode` grid, so the `ODS`/`XLSX` schedule targets are `DELEGATES` rows reusing `_sheet_spec`, never an owned spreadsheet engine; four flat `sheet`/`header`/`column_width`/`regime` pipeline fields where one `SheetProfile` value object collapses the spreadsheet-egress policy; a `TemplateContext` frozen value wrapped one-field-deep inside `TemplatePipeline` where the admitted owner IS the dispatcher (the wrapper added no invariant and forced a `pipeline.context` hop on every arm), collapsed into the one `TemplatePipeline` owner carrying both the bound fields and the dispatch; an `assets: frozendict[str, tuple[bytes, str]]` in-memory `(bytes, mediatype)` band where the emit owner resolves figures by `asset_key.hex -> path` through `EmitSpec.assets` and the office arms read that band, replaced by the path-aligned `assets: frozendict[str, str]` threaded into every office arm so the same figures embed across `DOCX`/`PPTX`/`ODF` rather than only an owned ODF arm; the whole in-band `Keyed`/`contribute(outcomes)` receipt machinery — a `Keyed = (ContentKey, tuple[ArtifactReceipt, ...])` pair and a module-fold `contribute` drain — kept after the report owner moved off its in-band `render` onto the `@receipted` weave, so every arm's tuple was empty and the drain folded over nothing (stale illusory code deleted; the delegated emit/report weaves own every receipt on the runtime `Signals` stream and the arms thread a bare `ContentKey`); a `TemplatePayload.template` bare `ReadOnly[str]` where the doctrine and the sibling `EmitPayload`/`ReportPayload` ingress declare the required identity key `Required[ReadOnly[str]]`; a phantom `DocumentPlan(mode=, node=, params=...)` constructor where the emit owner takes a typed `spec: EmitSpec`, replaced by `DocumentPlan(mode=mode, node=pipeline.node, spec=spec_of(pipeline))` read off the fused `DELEGATES` row and constructed directly from the trusted owner with no second admission; a `pdf_standards: tuple[str, ...]` field where the emit owner takes a `variant: PdfVariant` projecting to both the typst `pdf_standards` and the weasyprint profile; a `bindings`-as-docxtpl-context model where the emit docxtpl arm derives its context from the `DocumentNode` tree (so a render binding reaches the typst `sys.inputs` AND the jinja HTML `ReportSpec.context` band per [03]-[HTML_CONTEXT], never the docx context); a `dict[str, object]` `bindings` bag a binding arm re-coerces and a raw `TemplatePipeline(...)` construction a caller fills by hand, where `TemplatePipeline.of` admits the closed `TemplatePayload` once through the `_PAYLOAD` `TypeAdapter`; a `create_memory_object_stream` result-gather rig draining each child's rail off a cloned `MemoryObjectSendStream` where `start_soon` returns the `TaskHandle` whose `return_value` already carries the child's rail (the concurrency `CHILD_CARRIER` law's rejected "stream gather rig"); a claim that `TemplatePipeline` contributes an `ArtifactReceipt` or is itself the `ReceiptContributor` the runtime keys, where the delegated emit/report `@receipted` weaves are the sole contributors and the pipeline imports no `ArtifactReceipt`; an `_html` arm calling a non-existent `ReportPlan.render()` for an in-band `Keyed` pair, and a flattened `ReportPlan(source=, sections=dict(bindings), figures=, loader=, trusted=)` constructor where `ReportPlan` takes `kind`+`spec: ReportSpec` and its `sections` is a `tuple[Section, ...]` not a bindings dict — the rebuilt arm calls the report owner's `rendered(ReportPlan(kind=ReportKind.TEMPLATE, spec=ReportSpec(...)))` and reads `keys.head()` exactly as `_via_document` does; a fabricated emit receipt on any emit-delegated arm where the emit owner weave-emits its own row and the arm threads only the head `ContentKey`; a per-format parameter bag or the parallel `context`/`sections` two-dict split where one `node`+`bindings` owner binds every target; a `TemplateBinding` descriptor struct and a `BINDINGS` table read by nothing — the fused `DELEGATES` route row and the `TARGETS` arm carry the dispatch; the parallel `DELEGATES` mode map plus a separate `_emit_spec` target match where one fused route row carries both (a second target-keyed map is the drift the fusion forecloses); a `_docx`/`_pdf`/`_pptx`/`_odf`/`_ods`/`_xlsx`/`_xml`/`_yaml`/`_toml`/`_typst_query`/`_typst_eval` sibling-function family where the eleven `DocumentPlan` arms differ only by `(DocumentMode, EmitSpec)` and collapse into one `_via_document` derived dispatch; an `if target == ...` branch where `DELEGATES`/`TARGETS` dispatch; a single-`target` owner with a sheet-set helper beside it where the modal `bound` discriminates `TemplateTarget | Iterable[TemplateTarget]`; and a `rail.value` crack mid-flow where the delegated rail threads unchanged. `TemplatePipeline` is the bind-and-route layer composing the emit and report binding owners, never a re-implementation of a binding a sibling already owns.

```python signature
from collections.abc import Awaitable, Callable, Iterable
from enum import StrEnum
from typing import Final, Literal, NotRequired, ReadOnly, Required, TypedDict, Unpack, overload

import msgspec
from anyio import TaskHandle, create_task_group
from builtins import frozendict
from expression import Block, Error, Ok, Result
from msgspec import Struct, field
from pydantic import TypeAdapter, ValidationError

from rasm.runtime.content_identity import ContentKey
from rasm.runtime.faults import RuntimeRail, traversed

from artifacts.document.emit import DocumentMode, DocumentPlan, EmitSpec, PdfVariant, XlsxRegime, produced
from artifacts.document.model import DocumentNode
from artifacts.document.report import FigureRef, ReportKind, ReportLoader, ReportPlan, ReportSpec, rendered

# --- [TYPES] ----------------------------------------------------------------------------


class TemplateTarget(StrEnum):
    DOCX = "docx"
    PDF = "pdf"
    HTML = "html"
    PPTX = "pptx"
    ODF = "odf"
    ODS = "ods"
    XLSX = "xlsx"
    XML = "xml"
    YAML = "yaml"
    TOML = "toml"
    MARKDOWN = "markdown"        # plain-text diffable manuscript — the emit `DocumentMode.MARKDOWN` `to_markdown` node lowering
    LATEX = "latex"              # journal-submission typeset manuscript — the emit `DocumentMode.LATEX` `to_latex` node lowering
    TYPST_QUERY = "typst-query"  # introspection sidecar: query the lowered typst document by `<label>` selector -> JSON/YAML schedule extract
    TYPST_EVAL = "typst-eval"    # introspection sidecar: evaluate a typst source expression against the lowered document


type Bind = Callable[["TemplatePipeline", TemplateTarget], Awaitable[RuntimeRail[ContentKey]]]
type DocumentRoute = tuple[DocumentMode, Callable[["TemplatePipeline"], EmitSpec]]  # the fused emit row: a target's mode AND its spec builder, never two parallel target-keyed maps
type TemplateFault = Literal["<invalid-payload>"]

# --- [MODELS] ---------------------------------------------------------------------------


class SheetProfile(Struct, frozen=True):
    # the spreadsheet-egress policy the ODS/XLSX schedule targets bind — one value object collapsing the
    # four emit sheet knobs so the schedule egress carries a named tab, a header-row toggle, a column
    # width, and an XLSX `regime` override (`None` = the emit writer's row-count auto-select) rather than
    # four flat pipeline fields; `_sheet_spec` projects it onto `EmitSpec` for both spreadsheet targets.
    sheet: str = ""
    header: bool = True
    column_width: float = 18.0  # the emit sheet-column default; a wide description column overrides per schedule
    regime: XlsxRegime | None = None


class QueryProfile(Struct, frozen=True):
    # the typst-introspection policy the TYPST_QUERY/TYPST_EVAL targets bind — one value object collapsing the
    # emit query knobs so a bound context egresses as a structured schedule SIDECAR (query the lowered typst
    # document by `<label>` selector to JSON/YAML) or a computed evaluation, rather than four flat pipeline
    # fields; `_query_spec` projects it onto `EmitSpec.selector`/`field_name`/`one`/`expression` for both
    # introspection targets, the bound `node` being the queried document exactly as the PDF arm lowers it.
    selector: str = ""       # TYPST_QUERY: the `<label>`/element selector matched against the lowered document
    field: str = ""          # TYPST_QUERY: the element field projected (empty = whole matched element)
    one: bool = False        # TYPST_QUERY: single-match value vs a match sequence
    expression: str = ""     # TYPST_EVAL: the typst source expression evaluated against the lowered document


class TemplatePayload(TypedDict, extra_items=object):
    template: Required[ReadOnly[str]]
    figures: NotRequired[ReadOnly[tuple[FigureRef, ...]]]
    assets: NotRequired[ReadOnly[frozendict[str, str]]]
    variant: NotRequired[ReadOnly[PdfVariant]]
    sheet: NotRequired[ReadOnly[SheetProfile]]
    query: NotRequired[ReadOnly[QueryProfile]]
    loader: NotRequired[ReadOnly[ReportLoader]]
    trusted: NotRequired[ReadOnly[bool]]

# --- [OPERATIONS] -----------------------------------------------------------------------


# the per-target `EmitSpec` builders the fused `DELEGATES` route binds BESIDE each `DocumentMode`, so a
# target's mode and its spec are ONE row that cannot drift; the `node` is the structural carrier every
# emit arm lowers, the spec carrying only the per-mode knobs. One builder per spec SHAPE, reused across
# the targets that share it (office across DOCX/PPTX/ODF, bare across XML/YAML/TOML).
def _office_spec(pipeline: "TemplatePipeline", /) -> EmitSpec:
    return EmitSpec(template=pipeline.template, assets=pipeline.assets)


def _typst_spec(pipeline: "TemplatePipeline", /) -> EmitSpec:
    # typst reads `bindings` as `sys.inputs` (str-valued): a str passes through, any other value rides the
    # TOTAL `_INPUT` encoder (`enc_hook=str`, which never raises on an unsupported type), so the coercion
    # cannot escape the emit owner's `produced` boundary as an unconverted raise — the fallible JSON step
    # is removed, not relocated; `variant` selects the archival profile.
    inputs = frozendict({key: value if isinstance(value, str) else _INPUT.encode(value).decode() for key, value in pipeline.bindings.items()})
    return EmitSpec(sys_inputs=inputs, variant=pipeline.variant)


def _sheet_spec(pipeline: "TemplatePipeline", /) -> EmitSpec:
    # the spreadsheet-schedule builder both the `ODS` (odfpy) and `XLSX` (openpyxl/xlsxwriter) targets
    # reuse — the bound `node`'s `TableNode` grid IS the schedule, so only the `SheetProfile` sheet knobs
    # cross into the spec, `regime` selecting the emit in-memory/streamed writer (`None` = the emit XLSX
    # arm's row-count auto-select). No `template`/`assets`: the spreadsheet arms lower the grid, not a
    # template clone or a figure band.
    profile = pipeline.sheet
    return EmitSpec(sheet=profile.sheet, header=profile.header, column_width=profile.column_width, spreadsheet=profile.regime)


def _query_spec(pipeline: "TemplatePipeline", /) -> EmitSpec:
    # the introspection builder both the `TYPST_QUERY` and `TYPST_EVAL` targets reuse — the bound `node` IS
    # the queried document (the emit `_typst_query`/`_typst_eval` arms lower `to_typst_source(plan.node)`
    # exactly as the PDF arm does), so only the `QueryProfile` selection knobs cross into the spec, the
    # unused half defaulting empty per target (`selector`/`field`/`one` for query, `expression` for eval).
    q = pipeline.query
    return EmitSpec(selector=q.selector, field_name=q.field, one=q.one, expression=q.expression)


def _bare_spec(_pipeline: "TemplatePipeline", /) -> EmitSpec:
    return EmitSpec()  # the node-lowered XML/YAML/TOML arms carry no spec knobs; the bound `node` is the whole input


async def _via_document(pipeline: "TemplatePipeline", target: TemplateTarget, /) -> RuntimeRail[ContentKey]:
    # the emit owner's production surface is the `produced` modal entrypoint; a lone plan normalizes to a
    # one-element `Block`, so its head key is the delegated artifact and the emit `@receipted` weave owns
    # that arm's `ArtifactReceipt` on the runtime `Signals` stream — this bind-and-route layer mints and
    # carries no receipt. The fused `DELEGATES` row resolves the `DocumentMode` AND its `EmitSpec` builder
    # together — one row, so the mode and the spec can never drift the way two parallel target-keyed maps would.
    mode, spec_of = DELEGATES[target]
    rail = await produced(DocumentPlan(mode=mode, node=pipeline.node, spec=spec_of(pipeline)))
    return rail.map(lambda keys: keys.head())


async def _html(pipeline: "TemplatePipeline", _target: TemplateTarget, /) -> RuntimeRail[ContentKey]:
    # HTML delegates to the report owner's `rendered` modal entry over a `TEMPLATE` `ReportPlan` — report
    # holds the jinja engine and its `@receipted` weave owns the `Report` receipt, so this arm threads the
    # head key exactly as `_via_document` does, never a bare `ReportPlan.render` (deleted) nor a flattened
    # constructor. The bound `bindings` band threads into `ReportSpec.context`, which the report owner's
    # `_template_arm` spreads through `render_async(sections=, figures=, **dict(spec.context))` — so the same
    # data-injection the typst PDF and spreadsheet arms carry reaches the jinja HTML context ([03]-[HTML_CONTEXT]).
    plan = ReportPlan(
        kind=ReportKind.TEMPLATE,
        spec=ReportSpec(source=pipeline.template, figures=pipeline.figures, context=pipeline.bindings, loader=pipeline.loader, trusted=pipeline.trusted),
    )
    rail = await rendered(plan)
    return rail.map(lambda keys: keys.head())

# --- [TABLES] ---------------------------------------------------------------------------

# the fused document-delegate route: each target binds its `DocumentMode` AND its `EmitSpec` builder in
# ONE row, so a new emit-delegated target lands as one row carrying both — the mode and the spec can never
# drift the way two parallel target-keyed maps would; `_via_document` reads both off the one row.
DELEGATES: Final[frozendict[TemplateTarget, DocumentRoute]] = frozendict({
    TemplateTarget.DOCX: (DocumentMode.DOCX_TEMPLATE, _office_spec),
    TemplateTarget.PDF: (DocumentMode.TYPST_DATA, _typst_spec),
    TemplateTarget.PPTX: (DocumentMode.PPTX, _office_spec),
    TemplateTarget.ODF: (DocumentMode.ODT, _office_spec),
    TemplateTarget.ODS: (DocumentMode.ODS, _sheet_spec),
    TemplateTarget.XLSX: (DocumentMode.XLSX, _sheet_spec),
    TemplateTarget.XML: (DocumentMode.XML, _bare_spec),
    TemplateTarget.YAML: (DocumentMode.YAML, _bare_spec),
    TemplateTarget.TOML: (DocumentMode.TOML, _bare_spec),
    TemplateTarget.MARKDOWN: (DocumentMode.MARKDOWN, _bare_spec),  # the node-lowered CommonMark/GFM manuscript reuses `_bare_spec` — no template/sheet/query knob
    TemplateTarget.LATEX: (DocumentMode.LATEX, _bare_spec),        # the node-lowered LaTeX manuscript reuses `_bare_spec`
    TemplateTarget.TYPST_QUERY: (DocumentMode.TYPST_QUERY, _query_spec),
    TemplateTarget.TYPST_EVAL: (DocumentMode.TYPST_EVAL, _query_spec),
})

TARGETS: Final[frozendict[TemplateTarget, Bind]] = frozendict({TemplateTarget.HTML: _html})

_PAYLOAD: Final = TypeAdapter(TemplatePayload)
# `enc_hook=str` makes the typst sys.inputs coercion TOTAL: an unsupported binding value rides `str`
# instead of raising, so `_typst_spec`'s coercion cannot escape the emit boundary as an unconverted raise.
_INPUT: Final = msgspec.json.Encoder(enc_hook=str)
_DECLARED: Final[frozenset[str]] = TemplatePayload.__required_keys__ | TemplatePayload.__optional_keys__

# --- [COMPOSITION] ----------------------------------------------------------------------


class TemplatePipeline(Struct, frozen=True):
    template: str
    node: DocumentNode
    bindings: frozendict[str, object] = field(default_factory=frozendict)
    figures: tuple[FigureRef, ...] = ()
    assets: frozendict[str, str] = field(default_factory=frozendict)
    variant: PdfVariant = PdfVariant.NONE
    sheet: SheetProfile = field(default_factory=SheetProfile)
    query: QueryProfile = field(default_factory=QueryProfile)
    loader: ReportLoader = ReportLoader.DICT
    trusted: bool = True

    @classmethod
    def of(cls, node: DocumentNode, /, **raw: Unpack[TemplatePayload]) -> Result["TemplatePipeline", TemplateFault]:
        try:
            payload = _PAYLOAD.validate_python(raw)
        except ValidationError as fault:
            fault.add_note(f"<template.payload:{[error['loc'] for error in fault.errors()]}>")
            return Error("<invalid-payload>")
        bindings = frozendict({name: value for name, value in payload.items() if name not in _DECLARED})
        return Ok(cls(
            template=payload["template"],
            node=node,
            bindings=bindings,
            figures=payload.get("figures", ()),
            assets=payload.get("assets", frozendict()),
            variant=payload.get("variant", PdfVariant.NONE),
            sheet=payload.get("sheet", SheetProfile()),
            query=payload.get("query", QueryProfile()),
            loader=payload.get("loader", ReportLoader.DICT),
            trusted=payload.get("trusted", True),
        ))

    @overload
    async def bound(self, target: TemplateTarget, /) -> RuntimeRail[ContentKey]: ...
    @overload
    async def bound(self, target: Iterable[TemplateTarget], /) -> RuntimeRail[Block[ContentKey]]: ...
    async def bound(self, target: TemplateTarget | Iterable[TemplateTarget], /) -> RuntimeRail[ContentKey] | RuntimeRail[Block[ContentKey]]:
        # `TemplateTarget()` is read before the `Iterable` arm because a `StrEnum` is itself
        # iterable; the overloads carry the per-modality output shape so a caller narrows on the
        # target it passes, never re-matching the runtime union.
        match target:
            case TemplateTarget() as one:
                return await self._dispatch(one)
            case stream:
                return await self._fanned(Block.of_seq(stream))

    async def _dispatch(self, target: TemplateTarget, /) -> RuntimeRail[ContentKey]:
        arm = _via_document if target in DELEGATES else TARGETS[target]
        return await arm(self, target)

    async def _fanned(self, targets: Block[TemplateTarget], /) -> RuntimeRail[Block[ContentKey]]:
        # each child rails its own faults so the group exits clean and every `TaskHandle` settles
        # `FINISHED`; the post-scope `return_value` read carries each rail back with no re-raise —
        # the concurrency `CHILD_CARRIER` law, never a memory-stream result-gather rig; the delegated
        # emit/report `@receipted` weaves own every receipt on the runtime `Signals` stream.
        async with create_task_group() as group:
            handles: Block[TaskHandle[RuntimeRail[ContentKey]]] = targets.map(lambda target: group.start_soon(self._dispatch, target))
        return traversed(handles.map(lambda handle: handle.return_value))
```

## [03]-[SIGNALS]

- [ODT_DEEPEN] [RESOLVED]: the `ODF` target delegates to `document/emit#DOCUMENT` `DocumentMode.ODT`, whose `_odf_emit`/`_odf_block`/`_odf_style`/`_odf_sheet`/`_odf_scaffold` arm is now the FULL `DocumentNode` lowering — a `match`-per-node fold authoring `text.Span` over a cached appearance-keyed `style.Style`+`TextProperties` for EVERY run (weight/italic/`fontname`/`fontsize`/`color`/`textposition` from `RunScript` plus `fo:language`/`fo:country` from `NodeMeta.lang`, the cache key carrying the face/size/locale so distinct fonts never collapse onto one row), `text.List`/`text.ListItem` nesting, `table.Table`+`TableHeaderRows`+`CoveredTableCell`+`numberrowsspanned`/`numbercolumnsspanned` over the `TableNode.spans` `(row, col, col_span, row_span)` quad, `FigureNode` embed through `addPicture` resolved from `EmitSpec.assets`, `AnnotationNode` LINK/NOTE via `text.A`/`text.Note(noteclass="footnote")`, and RTL via `style.ParagraphProperties(writingmode="rl-tb")`. The delegated `ODF` render carries the same run fidelity the `DOCX`/`PDF` arms do, so the ODF-target shed boundary is honoured by the document-lowering owner; the residual `.api/odfpy.md` `NoteBody`/`NoteCitation` write-side catalogue rows ride [ODF_CATALOG], and template-seeded ODF authoring stays out of scope exactly as the `PPTX` from-scratch arm leaves its threaded `template` unused. No open cross-file item remains on the emit ODT depth.
- [ODF_CATALOG] [RESOLVED]: the `.api/odfpy.md` folder catalogue now rows every write-side member the deepened `ODT` arm composes — `odf.text.A`/`List`/`ListItem`/`Note`/`NoteCitation`/`NoteBody`, `odf.style.ParagraphProperties`, and `odf.office.Annotation` (the `text`/`style`/`office`/`table`/`number` rows verified present, alongside `CoveredTableCell`/`TableHeaderRows`/`numbercolumnsspanned`/`numberrowsspanned`/`addPicture`/`DateStyle`). The delegated emit `ODT` deepening cites only catalogued members; the sole trailing nit is the `.api/odfpy.md` convenience import EXAMPLE line, which lists `from odf.text import ... Note` and omits the sibling `NoteBody`/`NoteCitation` names the member row already documents — a one-line catalogue polish, not a missing capability. Spans `.api/odfpy.md`.
- [REPORT_RAIL_SEAM] [RESOLVED]: the report owner settled the contract in favor of the `@receipted`-weave form — `document/report.md` now declares `_emit -> Self` under `@receipted(_REDACTION)`, the module entry `rendered(plans) -> RuntimeRail[Block[ContentKey]]` (keys only), and NO `ReportPlan.render()`. The `_html` arm here is reconciled to that: it constructs `ReportPlan(kind=ReportKind.TEMPLATE, spec=ReportSpec(...))` and threads `rendered(plan)` -> `keys.head()` exactly as `_via_document` threads `produced`, and the stale in-band `Keyed`/`contribute` machinery is deleted. No open cross-file item remains on this seam.
- [HTML_CONTEXT] [RESOLVED]: `document/report.md` grew the `ReportSpec` context band — `ReportSpec` now declares `context: frozendict[str, object]` admitted through `ReportPayload`, and `_template_arm` renders `render_async(sections=spec.sections, figures=spec.figures, **dict(spec.context))`, so the arbitrary render binding now has a live HTML home. The `_html` arm here is reconciled to that: it constructs `ReportSpec(source=, figures=, context=pipeline.bindings, loader=, trusted=)` so `pipeline.bindings` threads into the jinja context, restoring the one-context-to-HTML data-injection the `DOCX`/`PDF`/spreadsheet arms already carry through the tree — the same `frozendict[str, object]` band the typst `TYPST_DATA` `sys.inputs` arm reads. No open cross-file item remains on this seam.
- [MANUSCRIPT] [RESOLVED]: the plain-text diffable/typeset manuscript egress landed across the three owners — `document/model#NODE` grew `to_markdown(node)` (CommonMark/GFM, the `_MD_ESCAPE`/`_MD_DECORATION` spelling tables) and `to_latex(node)` (journal-submission LaTeX, the `_LATEX_ESCAPE`/`_LATEX_SECTION`/`_LATEX_DECORATION` tables), each a total `match` fold over the same ten-variant tree the `to_typst_source`/`to_html` arms lower; `document/emit#DOCUMENT` grew `DocumentMode.MARKDOWN`/`LATEX` binding `_markdown_emit`/`_latex_emit` (`CORE` band, `ReceiptKind.OFFICE`, no `_REQUIRED` input); and this page added the `TemplateTarget.MARKDOWN`/`LATEX` rows plus two `DELEGATES` rows reusing `_bare_spec` — the docs-as-code Markdown and journal-submission LaTeX egress of the SAME bound `node` tree now have a home with zero new format surface here (the model lowerings and emit modes carry the whole capability, this page routes to them).
- [ASSET_BAND] [BLOCKED]: the pipeline carries figure assets as `assets: frozendict[str, str]` (`asset_key.hex -> path`) to match the emit owner's `EmitSpec.assets: frozendict[str, str]` path band, threaded into the `DOCX`/`PPTX`/`ODF` office arms. A binding/template engine more naturally carries IN-MEMORY render-pipeline figure bytes; decide whether `EmitSpec.assets` should widen to `frozendict[str, tuple[bytes, str]]` (`(bytes, mediatype)`, embedded through odfpy `addPictureFromString` / docx/pptx/reportlab `BytesIO`) so binding-time figures need no temp-file materialization, or whether the path band stays canonical and the pipeline materializes. Spans `document/emit.md`, `core/format.md`, `document/report.md`.

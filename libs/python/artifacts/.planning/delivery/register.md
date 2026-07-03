# [PY_ARTIFACTS_REGISTER]

The ISO 19650 delivery-register / drawing-index owner — the information-container metadata authority at the delivery boundary. `Register` is ONE owner over the closed-payload `RegisterOp` `expression.tagged_union` (`Index` renders the sheet-index publication table through `visualization/table#TABLE`, `Container` serializes the ISO 19650 information-container metadata as a structured `lxml` COBie/BS 1192 XML tree, schema-validates it against the owned `isoschematron.Schematron` required-metadata rules, and emits C14N-canonical byte-reproducible bytes, `Audit` folds the coverage verdict over the owned suitability/revision vocabularies through the accumulating `RegisterFault.combined` disposition, and `Render` streams the native `xlsxwriter` register spreadsheet — sized columns, owned-vocabulary validation dropdowns, per-reference asset hyperlinks, a page-of-N running header, and read-only protection on an issued register — or round-trips an existing client register through `openpyxl`) dispatched by one total `match` and folded ONCE into a `Composed` evidence struct the `emit`/`_emit` projections share, never a `StrEnum` keyed against an erased `dict[str, object]` bag and never a per-target register-builder family. No ISO 19650 register library exists, so this owner composes the delivery algebra over the owned standards vocabularies plus `polars`/`great-tables`/`xlsxwriter`/`openpyxl`/`lxml`, never a re-implemented byte emitter.

The status vocabularies are OWNED closed families authored to the EXACT BS EN ISO 19650‑2:2018 UK National Annex Table NA.1 cardinality, never a stringly code the body re-parses and never a subset that mis-states the standard. `SuitabilityCode` is the closed `S0..S7` shared/WIP band — `S0` initial WIP, `S1` coordination, `S2` information, `S3` review-and-comment, `S4` stage approval, `S5` withdrawn (kept in the sequence so the cardinality is exact, its `withdrawn` flag the audit reads rather than a silent omission that lies about the published set), `S6` PIM authorization, `S7` AIM authorization — whose `_SUITABILITY` `frozendict` is the ONE `code -> (ContainerState, description, withdrawn, revision_kind)` correspondence every consumer derives from — the shared/withdrawn semantics reading the row's own `state`/`withdrawn` field through the `Suitability` properties, `_S_VALUES` the derived admission-membership set. `PublishedCode` is the parametric published-contractual family (`A1..An` authorized-and-accepted, `B1..Bn` partially-authorized-with-comments) carried as a `(PublishedPrefix, ordinal)` value object whose OPEN ordinal is a value the case holds rather than a fabricated closed enum, and `Suitability` is the closed `shared`/`published`/`project` `tagged_union` projecting `.state`/`.code`/`.contractual`/`.withdrawn` over one total `match` — the `project` case the semi-closed extension band the NA's "codes may be expanded to suit specific project requirements providing the required codes are documented in the project's information standard" clause admits (an archive `CR` as-constructed record or any documented client code lands here, never a fabricated closed `RecordCode` enum claiming ISO standing). `RevisionCode` is the `P{NN}[.{NN}]` preliminary / `C{NN}` contractual value object (NA.4.3, the two-integer WIP `version` suffix distinguishing preliminary versions, e.g. `P02.05`) whose `render` composes the leading-zero canonical string and whose `succeeds` orders revisions so the audit reads monotonicity off the value rather than a string compare mis-sorting `C09` before `C10`; `ContainerState` is the four-member CDE container axis (`WIP`/`SHARED`/`PUBLISHED`/`ARCHIVE`); `Classification` is the lean ISO 12006‑2 / Uniclass 2015 reference the container carries as ISO 19650‑2 5.1.7.c metadata, the full MasterFormat/UniFormat/OmniClass code tables staying the `specification/classify#CLASSIFY` owner's.

`InformationContainer` is the frozen owner of the whole ISO 19650 information-container concept — the BS 1192 container-reference naming fields (`project`/`originator`/`functional`/`spatial`/`form`/`discipline`/`number` composed by `reference` through one symbolic `_FIELD_SEP` join), the `suitability`/`revision`/`classification`/`purpose` metadata, the `asset_key` sheet-artifact `ContentKey.hex` the register co-identifies (feeding the `core/plan#PLAN` node's parent keys), and the aggregated `composition/sheet#SHEET` `TitleBlock` sheet-index facts (`sheet_total`/`author`/`checker`/`approver`/`issued`) — whose `admitted` classmethod admits a raw client-register row EXACTLY ONCE through one module-level `pydantic` `TypeAdapter` over a closed `ContainerPayload` `TypedDict` and the `Suitability.parse`/`RevisionCode.parse` binders composed applicatively through `map2`, so a malformed code surfaces as a `RegisterFault` at the seam and the interior is total over admitted containers, and whose `from_title_block` aggregates a drawn sheet's title block into a register row rather than a second data entry. `Register` carries the container set, the `ContainerMeta` ISO 19650 project header (project/appointing-party/lead-party/delivery-stage/milestone), and the register-level `suitability`/`revision`/`classification`/`issued` issue metadata, its two accumulating ingress constructors `admit` (raw client rows) and `of_sheets` (`SheetEntry` title-block aggregation) reporting every casualty of a batch through the `_accumulated` monoid fold, and derives `frame` (the one `polars.DataFrame` the `Index` publication table and the `document/model#MODEL` `DocumentNode` `TableNode` both lower from) and `evidence` (the `RegisterEvidence` coverage verdict `delivery/transmittal#TRANSMITTAL` composes as the issued manifest).

`_composed` is the one `_GUARD`-contracted total `match` both entrypoints read, offloaded off the event loop through a `CapacityLimiter`-bounded `to_thread.run_sync` so the GIL-releasing `great-tables`/`lxml`/`xlsxwriter` render never blocks the loop; the `Index` arm builds a `visualization/table#TABLE` `TablePlan` from `frame` and the `_index_ops` `TableOp` sequence and calls `.build()` (SINGLE-FACT — one render, one content key, exactly as `drawing/schedule#SCHEDULE` lowers), the `Container` arm builds the namespaced `lxml` tree through `etree.SubElement` so a title carrying `<`/`&`/`"` is serializer-escaped rather than f-string-spliced, validates it against the owned `isoschematron.Schematron` required-metadata rules, and emits `etree.tostring(method="c14n2")` byte-reproducible bytes, the `Audit` arm encodes the `RegisterEvidence.facts`, and the `Render` arm streams the `xlsxwriter` `Workbook(constant_memory=True)` register with sized columns, suitability/state-keyed `conditional_format`, owned-vocabulary `data_validation`, per-reference `write_url` asset links, a running header, and issued-register `protect`, and folds an `openpyxl` client-register round-trip under revision-latest merge. `Register.emit` returns `RuntimeRail[ArtifactReceipt]` minted once at `async_boundary(subject, self._emit, catch=_FAULTS)` — the register IS the single content-keyed production entry a `core/plan#PLAN` `ArtifactWork` node wraps, the returned `ArtifactReceipt.Register(key, kind, sheets, suitability, revision, classification, validation, bytes)` case (the new delivery evidence the seam-unification target admits as a CASE on the one family) IS the `ReceiptContributor`, so no second render mints the receipt. This owner authors no drawing, no title block, no publication table, and no IFC — it composes the sheet register, folds the ISO 19650 coverage verdict, and lowers to table / XML / spreadsheet over the OWNED delivery vocabularies.

## [01]-[INDEX]

- [01]-[REGISTER]: the ISO 19650 delivery-register axis — the exact-cardinality `SuitabilityCode` (`S0..S7`, `S5` withdrawn), `PublishedCode` (`A1..An`/`B1..Bn`), `Suitability` (`shared`/`published`/`project`), `RevisionCode` (`P`/`C`), `ContainerState`, and `Classification` owned vocabularies over the one `_SUITABILITY` correspondence; the `InformationContainer` owner with its `pydantic`-`TypeAdapter` `ContainerPayload` admission and `from_title_block` `composition/sheet#SHEET` aggregation; the `Register` owner with its `ContainerMeta` header, its accumulating `admit`/`of_sheets` ingress, its discipline-then-ordinal-sorted `polars` `frame`, and its `RegisterEvidence` coverage verdict; the closed `RegisterOp` (`Index` great-tables publication index / `Container` lxml COBie·BS 1192 XML `isoschematron.Schematron`-validated and `c14n2`-canonical / `Audit` `RegisterFault.combined` coverage fold / `Render` xlsxwriter spreadsheet with owned-vocabulary `data_validation`, per-reference `write_url`, running header, and issued-register `protect` + openpyxl round-trip) folded once through the `to_thread`-offloaded `_composed`; and the new `ArtifactReceipt.Register(...validation, bytes)` case carrying the container conformance state the plan node wraps.

## [02]-[REGISTER]

- Owner: `Register` the one delivery-register owner discriminating operation over the closed `RegisterOp` `tagged_union`, every arm folded ONCE into the `Composed` evidence struct (`data` bytes, `kind` the receipt discriminant, `sheets`/`suitability`/`revision`/`classification` the register facts) both `emit` and `_emit` read — no second render. `ContainerState` is the closed four-member CDE container axis; `SuitabilityCode` is the closed `S0..S7` shared/WIP band whose `_SUITABILITY` `frozendict[SuitabilityCode, SuitabilityRow]` is the ONE `(state, description, withdrawn, revision_kind)` correspondence the exact NA Table NA.1 forces this owner to carry (`S5` the withdrawn member kept in the sequence so the cardinality is exact, its `withdrawn` flag the audit reads off the row through `Suitability.withdrawn`), `_S_VALUES` derived from it by comprehension, never a per-code literal or a `_value2member_map_` private-dunder probe; `PublishedPrefix` is the closed `A`/`B` prefix pair and `PublishedCode` the `(prefix, ordinal)` value object rendering the parametric `A1..An`/`B1..Bn` families; `Suitability` is the closed `shared`/`published`/`project` `tagged_union` projecting `.state`/`.code`/`.contractual`/`.withdrawn` over one total `match`, its `parse` the boundary admission of a raw code through the `_S_VALUES` membership then the `_PUBLISHED` grammar (never a private-dunder reach), the `project` case the semi-closed extension band a documented client/record code lands on. `RevisionKind` is the `P`/`C` pair and `RevisionCode` the `(kind, revision, version)` value object whose `render` composes `P01`/`P02.05`/`C01` and whose `succeeds` orders revisions; `ClassificationSystem` is the closed reference axis and `Classification` the lean `(system, code, title)` reference. `InformationContainer` is the frozen ISO 19650 information-container owner carrying the BS 1192 naming fields, the `suitability`/`revision`/`classification`/`purpose` metadata, the `asset_key` co-identification, and the aggregated sheet-index facts, its `reference` the symbolic `_FIELD_SEP` join, `admitted` the one `pydantic`-`TypeAdapter` admission composing `Suitability.parse`.`map2`(`RevisionCode.parse`), and `from_title_block` the `TitleBlock` aggregation. `ContainerMeta` is the project-level ISO 19650 header (project/appointing-party/lead-party/stage/milestone) the `Container` XML roots on; `RegisterEvidence` is the coverage-verdict receipt (`containers`/per-state counts/`contractual`/`withdrawn`/`duplicates`/`complete`/`dominant_suitability`/`latest_revision`, `severed: Option[RegisterFault]`) folded once and read by `Audit`, the receipt, and `delivery/transmittal#TRANSMITTAL`. `polars` owns the `frame` seam, `great-tables` (via `visualization/table#TABLE` `TablePlan`) the publication index, `lxml` the `Container` COBie/BS 1192 XML, `xlsxwriter` the native register `Workbook`, `openpyxl` the client-register round-trip; the delivery algebra (container aggregation, suitability audit, index composition, spreadsheet layout) is this owner's composition over those engines, never a re-implemented emitter.
- Cases: `RegisterOp` cases — `Index(theme, fmt)` (lower `Register.frame` into `visualization/table#TABLE` — build the `TablePlan` from the frame, the `_index_ops` `TableOp` sequence (`Header` carrying the project/revision, `Stub` row-grouping by `discipline` — the AEC-canonical register organization, three `Spanner` groups for identification/status/responsibility, `Color` CDE-state fill from `_STATE_HEX` on the `state` BODY column — grouped-and-coloured on one column would conflict, so `discipline` groups and `state` colours, a `GrandSummary` container count, an ISO 19650 source note), and the `Theme` publication identity, returning the rendered HTML/LaTeX/PDF sheet-index bytes — the rendered index the brief's "no rendered index is under-built" bar demands) · `Container(dialect)` (serialize the ISO 19650 information-container metadata as one structured `lxml` `informationContainerSet` tree — the `ContainerMeta` project header, one `informationContainer` `SubElement` per container, each `metadata_rows` field a Clark-notation `SubElement` whose `.text` carries the value, the `ContainerDialect` selecting the `iso19650`/`bs1192`/`cobie` namespace profile — then schema-validate the built tree against the owned `_container_schema` `isoschematron.Schematron` (one non-empty `sch:assert` per `_REQUIRED_META` §5.1.7 localname; a per-render validator since the ISO engine is not thread-re-entrant on `error_log`), folding the `"valid"`/`"invalid:{n}"` verdict onto the `Composed.validation` receipt scalar, and emit `etree.tostring(tree, method="c14n2")` C14N-canonical bytes so the delivery container content-addresses byte-reproducibly run-to-run — never an f-string markup splice and never an un-validated un-canonical ship) · `Audit(policy)` (fold the coverage verdict — `register.audited(policy)` per-container `_container_faults` (a suitability-state vs revision-kind mismatch, a withdrawn container issued, a policy-required classification or contractual suitability absent) plus set-level `_sequence_faults` (a non-monotonic revision on a repeated reference, a `Counter`-detected duplicate reference, a sheet-index number gap) accumulated through the `RegisterFault.combined` monoid into `RegisterEvidence`, `Nothing` for a clean register, the `_ENCODER.encode(evidence.facts)` JSON the audit report bytes) · `Render(merge)` (stream the native register spreadsheet — an `xlsxwriter.Workbook(constant_memory=True)` `Register` worksheet, per-column `set_column` widths from `_column_widths`, the `_COLUMNS` header in a shared `Format`, one `spreadsheet_row` per container with the Reference cell a `write_url` to `{_ASSET_URI}{asset_key}` when the container co-identifies an artifact, suitability/state-keyed `conditional_format` bands, owned-vocabulary `data_validation` dropdowns (State the closed CDE list hard-stop, Suitability the S-band soft, Revision the P/C grammar as guidance), `autofilter`/`freeze_panes`/`set_landscape`/`fit_to_pages`/`repeat_rows` plus the `set_header`/`set_footer` page-of-N running header and issue-date footer for the plotted register, `set_properties` metadata, and `protect` read-only on an issued register; a present `merge` `Some(bytes)` round-trips an existing client register through `openpyxl` `load_workbook(read_only=True, data_only=True)` and the revision-latest `_merged` fold before emit) — matched by one total `match`/`case` lowering to the one `Composed` fold; never a per-format register builder, never a per-operation `_emit` method, never a second render for the receipt.
- Entry: `Register.emit` dispatches over the runtime `async_boundary` returning `RuntimeRail[ArtifactReceipt]` (mirroring `visualization/table#TABLE` `TablePlan.render`), so the register IS the single content-keyed production entry a `core/plan#PLAN` `ArtifactWork` node wraps — `ArtifactWork(key=<register key>, work=register.emit, parents=<the constituent InformationContainer.asset_key content keys>, admission=Admission(keyed=None))` — never a per-operation entrypoint and never an `of -> ContentKey` that fails to satisfy `Work[ArtifactReceipt]`. `_emit` offloads the `_composed` fold through `anyio.to_thread.run_sync` under the module-level `_GATE` `CapacityLimiter` (the explicit thread bound, never the per-loop 40-token default) so the GIL-releasing render never blocks the loop, mints the content key over the produced bytes through `ContentIdentity.of` (co-identifying the durable register with the `csharp:Rasm.Persistence` `XxHash128` seed), and returns the `ArtifactReceipt.Register` case directly — the returned receipt IS the `ReceiptContributor`, so no second `contribute` re-renders the register. The two ingress constructors `Register.admit`/`of_sheets` return `Result[Register, RegisterFault]` under the accumulating disposition so a batch reports every casualty, distinct from `emit`. `async_boundary` narrows on `_FAULTS = (LxmlError, XlsxWriterException, KeyError, ValueError, BeartypeCallHintViolation)` so a malformed frame, an XML build fault, a `_SUITABILITY`/`_STATE_HEX` miss, a bad workbook, or a `_GUARD`-rejected container each discriminates into its own `BoundaryFault` case, and cancellation re-raises as the structured signal.
- Auto: `_composed(register) -> Composed` is the ONE `_GUARD`-contracted total `match` over `register.op` both `emit` and `_emit` read — the `Index` arm builds `TablePlan(frame=register.frame, ops=_index_ops(register), fmt=fmt, theme=theme).build()` (the frame the `polars.from_dicts` over each `container.row`), the `Container` arm builds the namespaced `lxml` document through `_container_document`, validates it against the cached-once `_container_schema` `isoschematron.Schematron` (a per-render `Schematron` wrapper so the bounded `_GATE` threads never race one validator's `error_log`), and serializes `etree.tostring(tree, method="c14n2")` C14N bytes threading the schema verdict onto `Composed.validation`, the `Audit` arm reads `register.audited(policy)` and `_ENCODER.encode`s its `facts`, the `Render` arm builds the `xlsxwriter` workbook through `_workbook` over the `_merged` container set — each returning `Composed(data, kind=register.op.tag, sheets=len(register.containers), suitability=register.suitability.code, revision=register.revision.render(), classification=register.classification.system.value, validation=validation)` (the schema state `"unchecked"` for every arm but `Container`) so the register-level facts fold once and the body stays one `match`-shaped path, never an inline `try`/`except` ladder beside it and never a `msgpack` encode of the tagged-union-bearing rows the encoder cannot serialize. `register.frame` is the one `polars.DataFrame` — ordered once by discipline then numeric sheet ordinal through the typed `_sheet_ordinal` so the discipline-grouped index reads deterministic, not insertion, order — the `Index` `TablePlan` styles AND the `document/model#MODEL` `TableNode` lowers from, so the sheet-index is authorable into the document tree without a second projection. `register.audited(policy)` flat-maps per-container `_container_faults` (a data table of `(predicate, RegisterFault)` rows filtered to the flagged, never a mutable append) through `Block.collect`, appends the set-level `_sequence_faults`, and reduces the casualties through the associative `RegisterFault.combined` monoid into `RegisterEvidence`, the accumulating disposition the coverage verdict needs. `InformationContainer.admitted` is the one boundary: the single `try` is the `_PAYLOAD.validate_python` gate, a `ValidationError` maps to `RegisterFault.malformed`, and `Suitability.parse`.`map2`(`RevisionCode.parse`) binds the raw code strings before construction, so the interior never re-parses a code — and `_ingest`/`_merged` compose the same admission through `Block.choose`/`Map.change`, never a mutable `list`/`dict` accumulator or a function-local import.
- Receipt: each operation contributes `core/receipt#RECEIPT` off the one `Composed` fold through the NEW `ArtifactReceipt.Register(key, kind, sheets, suitability, revision, classification, validation, bytes)` case — the content key, the `RegisterOp` tag, the container count, the register-aggregate suitability code, the register revision, the classification system, the `Container`-XML schema-conformance state (`"valid"`/`"invalid:{n}"`/`"unchecked"`), and the produced byte count, the register-container evidence the reading map names (sheet count, suitability distribution, revision, classification) plus the delivery-container conformance the ISO 19650 primitive requires. The `Composed.kind` discriminant carries the op tag onto the facts so the `Index`/`Container`/`Audit`/`Render` lines stay routable by the `MeterProvider` per-kind instrument, and `_emit` mints the single case off the one render exactly as the sibling `drawing/schedule#SCHEDULE` mints its `ArtifactReceipt.Schedule` off one `TablePlan.build` — never a per-kind facts `Struct` re-wrapping the scalars the mint takes positionally, never a phantom `ArtifactReceipt.of(key, facts)` the receipt owner rejects, never a parallel register-receipt type, and never a second render. The register contributes exactly ONE case to the shared family (the `[07]-[SEAM_UNIFICATION]` target's delivery evidence as a CASE, never a parallel rail), and the `core/plan#PLAN` planner reads it as the content-keyed evidence its sub-graph elision distinguishes hit from miss on.
- Packages: `expression` (`tagged_union`/`tag`/`case` the `Suitability`/`RegisterFault`/`RegisterOp` unions; `Result`/`Ok`/`Error`/`Option`/`Some`/`Nothing` the admission and coverage rails, `.map2` the applicative code admission, `.swap`/`.to_option` the accumulating partition; `Block.of_seq`/`collect`/`reduce`/`empty`/`append`/`choose` the fault fold and `Map.of_seq`/`change`/`try_find`/`add`/`empty`/`values`/`fold` the merge, state-histogram, and sequence folds; `.default_value`/`.is_none`/`.is_empty` the carrier projections); `msgspec` (`Struct(frozen=True)` the `SuitabilityRow`/`PublishedCode`/`RevisionCode`/`Classification`/`InformationContainer`/`ContainerMeta`/`SheetEntry`/`AuditPolicy`/`RegisterEvidence`/`Composed`/`Register` value objects, `json.Encoder` the audit-report egress); `polars` (`from_dicts`/`DataFrame`/`col` the register frame the `TablePlan` styles and the diagram/document lowerings read, a settled input the register shapes but never sources); `great-tables` (through `visualization/table#TABLE` `TablePlan`/`TableOp`/`Theme`/`TableFormat` — the publication sheet-index, composed at the `build` seam, never a direct great-tables reach); `xlsxwriter` (`Workbook(constant_memory=True)`/`add_worksheet`/`add_format`/`get_default_url_format`/`set_column`/`write_row`/`write_url`/`conditional_format`/`data_validation`/`autofilter`/`freeze_panes`/`set_landscape`/`set_paper`/`fit_to_pages`/`repeat_rows`/`set_header`/`set_footer`/`protect`/`set_properties`/`close` the native register spreadsheet — the validation dropdowns, asset hyperlinks, running header, and issued-register lock composed under `constant_memory`, which forbids only `add_table`/`autofit`); `openpyxl` (`load_workbook(read_only=True, data_only=True)`/`Workbook.sheetnames`/`Worksheet.iter_rows(values_only=True)` the client-register round-trip ingest); `lxml` (`etree.Element`/`SubElement`/`QName`/`fromstring`/`tostring` the namespaced COBie/BS 1192 container XML, `etree.tostring(method="c14n2")` the byte-reproducible C14N egress, `isoschematron.Schematron`/`.validate`/`.error_log` the owned required-metadata conformance oracle); `functools` (`cache` memoizing the compiled-once Schematron schema bytes); `pydantic` (`TypeAdapter`/`ValidationError` the `ContainerPayload` admission gate); `beartype` (`BeartypeConf`/`beartype`/`BeartypeCallHintViolation` the `_GUARD` boundary contract); `anyio` (`CapacityLimiter`/`to_thread.run_sync` the bounded offload); `collections.Counter`/`re`/`itertools.pairwise` (the duplicate multiset, the module-level `_REVISION`/`_PUBLISHED` grammars, the gap detection); runtime (`content_identity.ContentIdentity`, `faults.RuntimeRail`/`async_boundary`); `composition/sheet#SHEET` (`TitleBlock` the aggregated sheet-index source); `core/receipt#RECEIPT` (`ArtifactReceipt.Register`). No register library and no per-code literal — the vocabularies are `StrEnum`+`frozendict` tables to the exact NA cardinality, the audit is `expression` folds, the imports are module-scope `lazy`/eager, never function-local.
- Growth: a new project-defined or record suitability code (an archive `CR`) is one `documented` policy row (the `Register.documented` `frozendict[str, ContainerState]` the raw-row `admit` AND the client-register round-trip both thread through `Suitability.parse`) admitting the token onto the `Suitability.project` case — the NA §NA.4.2 documented-extension band — never a new S-code enum edit, a fabricated `RecordCode`, or a documented code silently dropped as `malformed`; a new published family is one `PublishedPrefix` member the `PublishedCode` render derives; a new CDE state is one `ContainerState` member reaching `_STATE_HEX` (the `_index_ops` `Color` state-band palette AND the `_workbook` `conditional_format` band); a new container-metadata field is one `InformationContainer` slot reaching `row`/`metadata_rows`/`spreadsheet_row` and the `ContainerPayload` band for free; a new coverage rule is one `RegisterFault` case plus one `_container_faults`/`_sequence_faults` row (the `assert_never` tail breaking the `severed` match until the arm exists); a new register operation is one `RegisterOp` case with its typed payload and one `_composed` arm; a new XML profile is one `ContainerDialect` member and one `_NSMAP` namespace; a new classification system is one `ClassificationSystem` member; a new publication-index column is one `_index_ops` `TableOp`; a new spreadsheet column is one `_COLUMNS` entry reaching `spreadsheet_row` (and its width from `_column_widths` for free); a newly-mandated ISO 19650 container-metadata field is one `_REQUIRED_META` localname the owned Schematron derives one `sch:assert` per; a new receipt scalar is one slot on the shared `ArtifactReceipt.Register` case (the `validation` conformance state is exactly this growth, threaded once from `Composed`); zero new surface — the register grows by case, row, member, and derived projection, never by method.
- Boundary: no artifact production beyond the register/index/manifest close (the drawn sheets arrive already emitted from `composition/sheet#SHEET`, the QTO/schedule frames from `drawing/schedule#SCHEDULE`), no IFC authoring (the `csharp:Rasm.Bim` graph owns the model; the register composes container facts at the wire, never re-authoring the IFC), no COBie-SpreadsheetML authoring (`ContainerDialect` marks the XML profile and declares its namespace on ONE ISO 19650 metadata tree; the distinct COBie spreadsheet schema is out of scope), no classification-table authoring (the register references `Classification`; `specification/classify#CLASSIFY` owns MasterFormat/UniFormat/OmniClass), no publication-table render authority (`visualization/table#TABLE` owns the great-tables render; the register composes the `TablePlan.build` seam), no data sourcing (the `polars.DataFrame` arrives settled; the register shapes but never `read_*`/`scan_*`s), no content-key minting the runtime owns, and no second scheduler beside the `core/plan#PLAN` lane. A seven-of-eight `SuitabilityCode` subset dropping `S5` where the exact NA sequence carries it withdrawn, a fabricated closed `RecordCode`/`A1..A99` enum where the `project` band and the `(prefix, ordinal)` value carry the open codes, a `_value2member_map_` private-dunder membership probe where `_S_VALUES` and the grammar admit the code, a bare `str` suitability/revision the interior re-parses, a function-local `import xlsxwriter`/`openpyxl`/`polars` where the module-scope `lazy`/eager import binds it, an `msgpack.Encoder().encode(rows)` the tagged-union suitability field cannot serialize, a mutable-`dict`/`list` fault or ingest accumulator where `Block.choose`/`Counter`/`Map.fold` own it, a dead `index` dict computed only for a truthiness check, a double-render `of`-then-`contribute` where the single `emit -> ArtifactReceipt` renders once, an f-string XML splice where `etree.SubElement(...).text` escapes the value, an un-canonical un-validated container ship where the `method="c14n2"` bytes content-address reproducibly and the `isoschematron.Schematron` verdict makes conformance the `validation` receipt scalar, a shared `Schematron` validator raced across the `_GATE` threads where the per-render wrapper isolates its `error_log`, an `add_table`/`autofit` under `constant_memory` where `data_validation`/`conditional_format` and the `_column_widths` `set_column` pre-pass ride the streaming path, a hardcoded header/footer or column width where `_running_header`/`_column_widths` derive them, a first-failure abort where the coverage verdict accumulates, a re-validated container past the `admitted` gate, a phantom `ArtifactReceipt.of`, and a parallel register-receipt rail are the deleted forms — `Register` is the ISO 19650 delivery-register owner of the owned vocabularies, the container-metadata aggregation, the coverage audit, and the index/manifest close.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
import re
from collections import Counter
from collections.abc import Iterable
from enum import StrEnum
from functools import cache
from io import BytesIO
from itertools import pairwise
from typing import Final, Literal, NotRequired, ReadOnly, Required, Self, TypedDict, Unpack, assert_never

import polars as pl
from anyio import CapacityLimiter, to_thread
from beartype import BeartypeConf, beartype
from beartype.roar import BeartypeCallHintViolation
from builtins import frozendict
from expression import Error, Nothing, Ok, Option, Result, Some, case, tag, tagged_union
from expression.collections import Block, Map
from msgspec import Struct, json
from pydantic import TypeAdapter, ValidationError

from rasm.runtime.content_identity import ContentIdentity
from rasm.runtime.faults import RuntimeRail, async_boundary

from artifacts.composition.sheet import TitleBlock
from artifacts.core.receipt import ArtifactReceipt
from artifacts.visualization.table import TableFormat, TableOp, TablePlan, Theme

lazy from lxml import etree, isoschematron
lazy from lxml.etree import LxmlError
lazy import xlsxwriter
lazy from xlsxwriter.exceptions import XlsxWriterException
lazy from openpyxl import load_workbook

# --- [TYPES] ----------------------------------------------------------------------------


class ContainerState(StrEnum):  # the four ISO 19650 CDE container states — a new state is one member
    WIP = "WIP"
    SHARED = "Shared"
    PUBLISHED = "Published"
    ARCHIVE = "Archive"


class SuitabilityCode(StrEnum):  # BS EN ISO 19650-2:2018 NA Table NA.1 S-band, exact — S5 is withdrawn, kept for cardinality
    S0 = "S0"
    S1 = "S1"
    S2 = "S2"
    S3 = "S3"
    S4 = "S4"
    S5 = "S5"
    S6 = "S6"
    S7 = "S7"


class PublishedPrefix(StrEnum):  # published-contractual families — A authorized-and-accepted, B partially-accepted-with-comments
    AUTHORIZED = "A"
    PARTIAL = "B"


class RevisionKind(StrEnum):  # NA.4.3 revision prefixes — P preliminary (WIP/Shared), C contractual (Published)
    PRELIMINARY = "P"
    CONTRACTUAL = "C"


class ClassificationSystem(StrEnum):  # the ISO 12006-2 classification reference; classify.md owns the code tables
    UNICLASS_2015 = "Uniclass 2015"
    ISO_12006_2 = "ISO 12006-2"
    OMNICLASS = "OmniClass"
    MASTERFORMAT = "MasterFormat"
    UNIFORMAT = "UniFormat"


class ContainerDialect(StrEnum):  # the Container XML namespace profile — a new dialect is one member plus one _NSMAP row
    ISO_19650 = "iso19650"
    BS_1192 = "bs1192"
    COBIE = "cobie"


# the closed ISO 19650 container-reference naming payload admitted ONCE through _PAYLOAD; the
# suitability/revision codes arrive as raw strings the Suitability/RevisionCode parsers bind at the seam.
class ContainerPayload(TypedDict, closed=True):
    project: Required[ReadOnly[str]]
    originator: Required[ReadOnly[str]]
    functional: Required[ReadOnly[str]]
    spatial: Required[ReadOnly[str]]
    form: Required[ReadOnly[str]]
    discipline: Required[ReadOnly[str]]
    number: Required[ReadOnly[str]]
    suitability: Required[ReadOnly[str]]
    revision: Required[ReadOnly[str]]
    title: NotRequired[ReadOnly[str]]
    purpose: NotRequired[ReadOnly[str]]
    classification: NotRequired[ReadOnly[str]]
    classification_system: NotRequired[ReadOnly[str]]
    asset_key: NotRequired[ReadOnly[str]]
    issued: NotRequired[ReadOnly[str]]


# --- [CONSTANTS] ------------------------------------------------------------------------
_FIELD_SEP: Final[str] = "-"  # the BS 1192 container-reference field separator (Project-Originator-...-Number)
_NS_19650: Final[str] = "https://rasm.dev/schema/iso19650/register"
_NSMAP: Final[frozendict[str | None, str]] = frozendict({
    None: _NS_19650,
    "cobie": "https://docs.buildingsmart.org/cobie",
    "bs1192": "https://rasm.dev/schema/bs1192",
})
_SCHEMATRON_NS: Final[str] = "http://purl.oclc.org/dsdl/schematron"
# the ISO 19650-2 §5.1.7 mandated per-container metadata localnames — the ONE required-field row the owned
# Schematron derives one non-empty `sch:assert` per, so a serialized Container dropping or blanking a mandated
# element fails conformance (the `ArtifactReceipt.Register.validation` scalar) rather than shipping silently; a
# newly-mandated field is one row here, never a hand-authored grammar. VALUE coherence stays `audited`'s.
_REQUIRED_META: Final[tuple[str, ...]] = ("suitability", "state", "revision")
_COLUMNS: Final[tuple[str, ...]] = (
    "Reference",
    "Title",
    "Form",
    "Number",
    "Discipline",
    "Suitability",
    "State",
    "Revision",
    "Classification",
    "Purpose",
    "Issued",
    "Author",
    "Checker",
    "Approver",
)
_SUITABILITY_COL: Final[int] = _COLUMNS.index("Suitability")
_STATE_COL: Final[int] = _COLUMNS.index("State")
_REFERENCE_COL: Final[int] = _COLUMNS.index("Reference")
_REVISION_COL: Final[int] = _COLUMNS.index("Revision")
_A4_PAPER: Final[int] = 9  # the xlsxwriter paper-size code for the ISO 5457 A4 plotted register
_MAX_WIDTH: Final[int] = 48  # the set_column width ceiling (constant_memory forbids the post-write autofit pass)
_PAD: Final[int] = 2
# the content-addressed artifact-link scheme: write_url composes `{_ASSET_URI}{asset_key}` per co-identified
# sheet so a client opening the register jumps from a container reference to its durable artifact, never a dead cell.
_ASSET_URI: Final[str] = "rasm-artifact://"
# the module-level compiled code grammars (system-apis REGEX law: never an inline re.compile per parse)
_REVISION: Final[re.Pattern[str]] = re.compile(r"^(?P<kind>[PC])(?P<rev>\d{2,})(?:\.(?P<ver>\d{2,}))?$")
_PUBLISHED: Final[re.Pattern[str]] = re.compile(r"^(?P<prefix>[AB])(?P<ordinal>\d+)$")
# the native-offload bound: the _composed render fold crosses one CapacityLimiter-bounded to_thread band
# so the GIL-releasing great-tables/lxml/xlsxwriter render never blocks the loop, distinct from the 40-token default.
_GATE: Final[CapacityLimiter] = CapacityLimiter(4)
_ENCODER: Final = json.Encoder()
# the boundary contract: a malformed InformationContainer/Register inside a well-tagged RegisterOp raises
# BeartypeCallHintViolation from the guarded _composed fold, which the _FAULTS-narrowed async_boundary discriminates.
_GUARD: Final = beartype(conf=BeartypeConf(violation_type=BeartypeCallHintViolation))
# the real engine raise tuple async_boundary narrows on so each discriminates into its own BoundaryFault case;
# naming the lxml/xlsxwriter roots reifies those lazy modules at load, the deliberate cost of precise fault split.
_FAULTS: Final[tuple[type[BaseException], ...]] = (LxmlError, XlsxWriterException, KeyError, ValueError, BeartypeCallHintViolation)

# --- [TABLES] ---------------------------------------------------------------------------


class SuitabilityRow(Struct, frozen=True, gc=False):  # one NA Table NA.1 row — the correspondence the S-band derives from
    state: ContainerState
    description: str
    withdrawn: bool
    revision_kind: RevisionKind


# the ONE code -> (state, description, withdrawn, revision_kind) correspondence every consumer derives
# from — the exact BS EN ISO 19650-2:2018 NA Table NA.1 S-band, S5 kept withdrawn so the cardinality is exact.
_SUITABILITY: Final[frozendict[SuitabilityCode, SuitabilityRow]] = frozendict({
    SuitabilityCode.S0: SuitabilityRow(ContainerState.WIP, "Initial status", False, RevisionKind.PRELIMINARY),
    SuitabilityCode.S1: SuitabilityRow(ContainerState.SHARED, "Suitable for coordination", False, RevisionKind.PRELIMINARY),
    SuitabilityCode.S2: SuitabilityRow(ContainerState.SHARED, "Suitable for information", False, RevisionKind.PRELIMINARY),
    SuitabilityCode.S3: SuitabilityRow(ContainerState.SHARED, "Suitable for review and comment", False, RevisionKind.PRELIMINARY),
    SuitabilityCode.S4: SuitabilityRow(ContainerState.SHARED, "Suitable for stage approval", False, RevisionKind.PRELIMINARY),
    SuitabilityCode.S5: SuitabilityRow(ContainerState.SHARED, "Withdrawn", True, RevisionKind.PRELIMINARY),
    SuitabilityCode.S6: SuitabilityRow(ContainerState.SHARED, "Suitable for PIM authorization", False, RevisionKind.PRELIMINARY),
    SuitabilityCode.S7: SuitabilityRow(ContainerState.SHARED, "Suitable for AIM authorization", False, RevisionKind.PRELIMINARY),
})
# the derived admission-membership vocabulary (one edit site above; comprehension here) — the raw-code seam
# admits against it. The shared/withdrawn semantics ride the `_SUITABILITY[s].state`/`.withdrawn` row and the
# `Suitability.state`/`.withdrawn` properties directly, so no parallel per-flag frozenset is minted.
_S_VALUES: Final[frozenset[str]] = frozenset(c.value for c in SuitabilityCode)
_CLASS_VALUES: Final[frozenset[str]] = frozenset(s.value for s in ClassificationSystem)
# the xlsxwriter/great-tables state-band fill keyed on the CDE state (WIP amber, shared blue, published green, archive grey).
_STATE_HEX: Final[frozendict[ContainerState, str]] = frozendict({
    ContainerState.WIP: "#FEF3C7",
    ContainerState.SHARED: "#DBEAFE",
    ContainerState.PUBLISHED: "#DCFCE7",
    ContainerState.ARCHIVE: "#E5E7EB",
})

# --- [ERRORS] ---------------------------------------------------------------------------


# the closed coverage/admission fault vocabulary — each case its offending container-reference set so the
# root recovers on the cause (re-code a mismatched suitability, withdraw a withdrawn container, re-sequence a
# gap), never a bool collapsing the causes; aggregate is the associative combination the audit accumulates.
@tagged_union(frozen=True)
class RegisterFault:
    tag: Literal["malformed", "misstated", "withdrawn_issued", "non_monotonic", "duplicate", "unclassified", "gap", "aggregate"] = tag()
    malformed: frozenset[str] = case()
    misstated: frozenset[str] = case()
    withdrawn_issued: frozenset[str] = case()
    non_monotonic: frozenset[str] = case()
    duplicate: frozenset[str] = case()
    unclassified: frozenset[str] = case()
    gap: frozenset[str] = case()
    aggregate: tuple["RegisterFault", ...] = case()

    @staticmethod
    def _members(fault: "RegisterFault", /) -> tuple["RegisterFault", ...]:
        return fault.aggregate if fault.tag == "aggregate" else (fault,)

    @staticmethod
    def combined(left: "RegisterFault", right: "RegisterFault", /) -> "RegisterFault":
        return RegisterFault(aggregate=(*RegisterFault._members(left), *RegisterFault._members(right)))


# --- [MODELS] ---------------------------------------------------------------------------


class PublishedCode(Struct, frozen=True):  # the A1..An / B1..Bn parametric published family — a value object, never a fixed enum
    prefix: PublishedPrefix
    ordinal: int

    def render(self) -> str:
        return f"{self.prefix.value}{self.ordinal}"


@tagged_union(frozen=True)
class Suitability:
    # the closed suitability family: an S-code (WIP/Shared), a published A/B code, or a documented
    # project/record extension code (an archive CR) the NA's "codes may be expanded" clause admits.
    tag: Literal["shared", "published", "project"] = tag()
    shared: SuitabilityCode = case()
    published: PublishedCode = case()
    project: tuple[str, ContainerState] = case()

    @classmethod
    def parse(cls, code: str, /, *, documented: frozendict[str, ContainerState] = frozendict()) -> Result[Self, RegisterFault]:
        # the raw-code seam: standard S-band, then the A/B published grammar, then the project's OWN documented
        # extension band (NA §NA.4.2 "codes may be expanded providing they are documented in the project's
        # information standard") carried as a POLICY_VALUE — so an archive `CR` a client register uses lands on
        # the `project` case rather than dropping as malformed, and admission REACHES the band the prose asserts.
        token = code.strip().upper()
        if token in _S_VALUES:
            return Ok(cls(shared=SuitabilityCode(token)))
        if (match := _PUBLISHED.match(token)) is not None:
            return Ok(cls(published=PublishedCode(prefix=PublishedPrefix(match["prefix"]), ordinal=int(match["ordinal"]))))
        if (state := documented.get(token)) is not None:
            return Ok(cls(project=(token, state)))
        return Error(RegisterFault(malformed=frozenset({code})))

    @property
    def code(self) -> str:
        match self:
            case Suitability(tag="shared", shared=s):
                return s.value
            case Suitability(tag="published", published=p):
                return p.render()
            case Suitability(tag="project", project=(token, _)):
                return token
            case _ as unreachable:
                assert_never(unreachable)

    @property
    def state(self) -> ContainerState:
        match self:
            case Suitability(tag="shared", shared=s):
                return _SUITABILITY[s].state
            case Suitability(tag="published"):
                return ContainerState.PUBLISHED
            case Suitability(tag="project", project=(_, state)):
                return state
            case _ as unreachable:
                assert_never(unreachable)

    @property
    def contractual(self) -> bool:
        return self.state is ContainerState.PUBLISHED

    @property
    def withdrawn(self) -> bool:
        match self:
            case Suitability(tag="shared", shared=s):
                return _SUITABILITY[s].withdrawn
            case _:
                return False


class RevisionCode(Struct, frozen=True):
    # NA.4.3 revision code — P{NN}[.{NN}] preliminary (the two-integer WIP version suffix distinguishing
    # preliminary versions, e.g. P02.05), C{NN} contractual; render composes the leading-zero canonical form.
    kind: RevisionKind
    revision: int
    version: int | None = None

    def render(self) -> str:
        base = f"{self.kind.value}{self.revision:02d}"
        return f"{base}.{self.version:02d}" if self.version is not None else base

    def succeeds(self, prior: "RevisionCode", /) -> bool:
        # contractual outranks preliminary; within a kind, higher (revision, version) wins — the audit reads
        # monotonicity off this order rather than a string compare that mis-sorts C09 before C10.
        rank = lambda r: (r.kind is RevisionKind.CONTRACTUAL, r.revision, r.version or 0)
        return rank(self) > rank(prior)

    @classmethod
    def parse(cls, mark: str, /) -> Result[Self, RegisterFault]:
        if (match := _REVISION.match(mark.strip().upper())) is None:
            return Error(RegisterFault(malformed=frozenset({mark})))
        version = int(match["ver"]) if match["ver"] is not None else None
        return Ok(cls(kind=RevisionKind(match["kind"]), revision=int(match["rev"]), version=version))


class Classification(Struct, frozen=True):  # the lean ISO 12006-2 classification reference; classify.md owns the code tables
    system: ClassificationSystem = ClassificationSystem.UNICLASS_2015
    code: str = ""
    title: str = ""


class InformationContainer(Struct, frozen=True):
    # the ISO 19650 information container — BS 1192 naming fields, the 5.1.7.c metadata triad, the asset-key
    # co-identification, and the aggregated composition/sheet TitleBlock facts; reference is the symbolic join.
    project: str
    originator: str
    functional: str
    spatial: str
    form: str
    discipline: str
    number: str
    suitability: Suitability
    revision: RevisionCode
    classification: Classification = Classification()
    title: str = ""
    purpose: str = ""
    asset_key: str = ""
    sheet_total: str = ""
    issued: str = ""
    author: str = ""
    checker: str = ""
    approver: str = ""

    @property
    def reference(self) -> str:
        return _FIELD_SEP.join((self.project, self.originator, self.functional, self.spatial, self.form, self.discipline, self.number))

    @classmethod
    def admitted(cls, documented: frozendict[str, ContainerState] = frozendict(), /, **raw: Unpack[ContainerPayload]) -> Result[Self, RegisterFault]:
        # `documented` (positional-only so it never collides with the `Unpack` payload keys) is the project's
        # information-standard extension band the suitability seam admits a documented `CR`/client code through.
        try:  # Exemption: the pydantic TypeAdapter admission kernel — the one statement seam, every interior signature past it holding the admitted container.
            payload = _PAYLOAD.validate_python(raw)
        except ValidationError:
            return Error(RegisterFault(malformed=frozenset({raw.get("number", "?")})))
        system = (
            ClassificationSystem(raw_sys)
            if (raw_sys := payload.get("classification_system", "")) in _CLASS_VALUES
            else ClassificationSystem.UNICLASS_2015
        )
        return Suitability.parse(payload["suitability"], documented=documented).map2(
            RevisionCode.parse(payload["revision"]),
            lambda suit, rev: cls(
                project=payload["project"],
                originator=payload["originator"],
                functional=payload["functional"],
                spatial=payload["spatial"],
                form=payload["form"],
                discipline=payload["discipline"],
                number=payload["number"],
                suitability=suit,
                revision=rev,
                title=payload.get("title", ""),
                purpose=payload.get("purpose", ""),
                asset_key=payload.get("asset_key", ""),
                issued=payload.get("issued", ""),
                classification=Classification(system=system, code=payload.get("classification", "")),
            ),
        )

    @classmethod
    def from_title_block(cls, entry: "SheetEntry", /) -> Result[Self, RegisterFault]:
        # aggregate a drawn sheet's title block into a register row — the latest revision mark parsed to a
        # RevisionCode (falling to P01 when the mark is a bare status), the sheet-set count and approvals carried.
        block = entry.block
        latest = block.revisions[-1].mark if block.revisions else "P01"
        return (
            RevisionCode
            .parse(latest)
            .or_else_with(lambda _: RevisionCode.parse("P01"))
            .map(
                lambda rev: cls(
                    project=block.project,
                    originator=entry.originator,
                    functional=entry.functional,
                    spatial=entry.spatial,
                    form=entry.form,
                    discipline=block.discipline,
                    number=block.sheet_number,
                    suitability=entry.suitability,
                    revision=rev,
                    classification=entry.classification,
                    title=block.sheet_title,
                    purpose=block.status,
                    asset_key=entry.asset_key,
                    sheet_total=block.sheet_total,
                    issued=block.date,
                    author=block.drawn_by,
                    checker=block.checked_by,
                    approver=block.approved_by,
                )
            )
        )

    def row(self) -> dict[str, object]:  # the register-frame row the polars DataFrame and the document TableNode read
        return {
            "reference": self.reference,
            "title": self.title,
            "form": self.form,
            "number": self.number,
            "discipline": self.discipline,
            "suitability": self.suitability.code,
            "state": self.suitability.state.value,
            "revision": self.revision.render(),
            "classification": self.classification.code,
            "purpose": self.purpose,
            "issued": self.issued,
            "author": self.author,
            "checker": self.checker,
            "approver": self.approver,
        }

    def spreadsheet_row(self) -> tuple[object, ...]:  # the xlsxwriter row, ordered by _COLUMNS
        return (
            self.reference,
            self.title,
            self.form,
            self.number,
            self.discipline,
            self.suitability.code,
            self.suitability.state.value,
            self.revision.render(),
            self.classification.code,
            self.purpose,
            self.issued,
            self.author,
            self.checker,
            self.approver,
        )

    def metadata_rows(self) -> tuple[tuple[str, str], ...]:  # the ISO 19650 container-metadata (localname, value) pairs the lxml build reads
        return (
            ("project", self.project),
            ("originator", self.originator),
            ("functionalBreakdown", self.functional),
            ("spatialBreakdown", self.spatial),
            ("form", self.form),
            ("discipline", self.discipline),
            ("number", self.number),
            ("title", self.title),
            ("suitability", self.suitability.code),
            ("state", self.suitability.state.value),
            ("revision", self.revision.render()),
            ("classificationSystem", self.classification.system.value),
            ("classificationCode", self.classification.code),
            ("purpose", self.purpose),
            ("assetKey", self.asset_key),
            ("issued", self.issued),
        )


_PAYLOAD: Final = TypeAdapter(ContainerPayload)


class SheetEntry(Struct, frozen=True):  # the sheet-aggregation ingress — a TitleBlock plus the ISO naming context from_title_block needs
    block: TitleBlock
    originator: str
    functional: str
    spatial: str
    form: str
    suitability: Suitability
    classification: Classification = Classification()
    asset_key: str = ""


class ContainerMeta(Struct, frozen=True):  # the project-level ISO 19650 header the Container XML is rooted on
    project: str = ""
    project_id: str = ""
    appointing_party: str = ""
    lead_party: str = ""
    stage: str = ""  # the RIBA / ISO 19650 delivery stage
    milestone: str = ""  # the information-delivery milestone the issue satisfies


class AuditPolicy(Struct, frozen=True):  # the coverage-audit requirements — POLICY_VALUES, never a per-check flag the body re-derives
    require_contractual: bool = False
    require_classification: bool = True
    require_sequence: bool = True


_DEFAULT_AUDIT: Final[AuditPolicy] = AuditPolicy()


class RegisterEvidence(Struct, frozen=True, gc=False):
    # the coverage-verdict receipt folded once — the Audit arm encodes its facts and delivery/transmittal reads it.
    containers: int
    wip: int
    shared: int
    published: int
    archive: int
    contractual: int
    withdrawn: int
    duplicates: int
    complete: bool
    dominant_suitability: str
    latest_revision: str
    classification: str
    severed: Option[RegisterFault] = Nothing

    @classmethod
    def of(cls, register: "Register", severed: Option[RegisterFault], complete: bool, /) -> Self:
        states = Block.of_seq(register.containers).fold(
            lambda acc, c: acc.change(c.suitability.state, lambda held: Some(held.default_value(0) + 1)), Map.empty()
        )
        counts = Counter(c.reference for c in register.containers)
        return cls(
            containers=len(register.containers),
            wip=states.try_find(ContainerState.WIP).default_value(0),
            shared=states.try_find(ContainerState.SHARED).default_value(0),
            published=states.try_find(ContainerState.PUBLISHED).default_value(0),
            archive=states.try_find(ContainerState.ARCHIVE).default_value(0),
            contractual=sum(1 for c in register.containers if c.suitability.contractual),
            withdrawn=sum(1 for c in register.containers if c.suitability.withdrawn),
            duplicates=sum(1 for count in counts.values() if count > 1),
            complete=complete,
            dominant_suitability=register.suitability.code,
            latest_revision=register.revision.render(),
            classification=register.classification.system.value,
            severed=severed,
        )

    @property
    def facts(self) -> dict[str, object]:  # native scalars the json.Encoder serializes unstringified, plus the gating cause
        return {
            "containers": self.containers,
            "wip": self.wip,
            "shared": self.shared,
            "published": self.published,
            "archive": self.archive,
            "contractual": self.contractual,
            "withdrawn": self.withdrawn,
            "duplicates": self.duplicates,
            "complete": self.complete,
            "dominant_suitability": self.dominant_suitability,
            "latest_revision": self.latest_revision,
            "classification": self.classification,
            "severed": self.severed.map(lambda fault: fault.tag).default_value("ok"),
        }


class Composed(Struct, frozen=True):  # the one evidence struct emit/_emit read — no second render
    data: bytes
    kind: str
    sheets: int
    suitability: str
    revision: str
    classification: str
    validation: str = "unchecked"  # the Container-XML schema-conformance state; the non-XML ops leave the default


@tagged_union(frozen=True)
class RegisterOp:  # the closed delivery vocabulary lowered once into Composed
    tag: Literal["index", "container", "audit", "render"] = tag()
    index: tuple[Theme, TableFormat] = case()
    container: ContainerDialect = case()
    audit: AuditPolicy = case()
    render: Option[bytes] = case()

    @staticmethod
    def Index(theme: Theme = Theme(), fmt: TableFormat = TableFormat.HTML) -> "RegisterOp":
        return RegisterOp(index=(theme, fmt))

    @staticmethod
    def Container(dialect: ContainerDialect = ContainerDialect.ISO_19650) -> "RegisterOp":
        return RegisterOp(container=dialect)

    @staticmethod
    def Audit(policy: AuditPolicy = _DEFAULT_AUDIT) -> "RegisterOp":
        return RegisterOp(audit=policy)

    @staticmethod
    def Render(merge: Option[bytes] = Nothing) -> "RegisterOp":
        return RegisterOp(render=merge)


# --- [SERVICES] -------------------------------------------------------------------------


class Register(Struct, frozen=True):
    # the register model plus the operation — containers the aggregated set, meta the ISO 19650 project
    # header, op the RegisterOp folded into Composed, plus the register-level issue metadata.
    op: RegisterOp
    containers: tuple[InformationContainer, ...] = ()
    meta: ContainerMeta = ContainerMeta()
    suitability: Suitability = Suitability(shared=SuitabilityCode.S2)
    revision: RevisionCode = RevisionCode(kind=RevisionKind.PRELIMINARY, revision=1)
    classification: Classification = Classification()
    issued: str = ""
    # the project information-standard's documented extension codes (NA §NA.4.2) — one POLICY_VALUE the raw-row
    # admission AND the client-register round-trip both read so a documented `CR`/client code round-trips
    # rather than dropping; empty for a standard-codes-only register, never a per-call flag.
    documented: frozendict[str, ContainerState] = frozendict()

    @classmethod
    def admit(
        cls,
        *payloads: ContainerPayload,
        op: RegisterOp = RegisterOp.Index(),
        meta: ContainerMeta = ContainerMeta(),
        documented: frozendict[str, ContainerState] = frozendict(),
    ) -> Result["Register", RegisterFault]:
        # the raw client-row ingress: each payload admitted through the InformationContainer gate under the
        # project's `documented` extension band, every casualty of the whole batch accumulated through the
        # monoid rather than aborting on the first; the band is retained so the round-trip reads it back.
        admitted = Block.of_seq(payloads).map(lambda payload: InformationContainer.admitted(documented, **payload))
        return _accumulated(admitted).map(lambda containers: cls(op=op, containers=containers, meta=meta, documented=documented))

    @classmethod
    def of_sheets(
        cls,
        entries: Iterable[SheetEntry],
        /,
        *,
        op: RegisterOp = RegisterOp.Index(),
        meta: ContainerMeta = ContainerMeta(),
        documented: frozendict[str, ContainerState] = frozendict(),
    ) -> Result["Register", RegisterFault]:
        # `from_title_block` reads `entry.suitability` (a pre-built `Suitability`, project codes included), so
        # it needs no `documented` at admission; the band is still retained so a later `Render` round-trip
        # admits documented codes from the merged client register.
        admitted = Block.of_seq(entries).map(InformationContainer.from_title_block)
        return _accumulated(admitted).map(lambda containers: cls(op=op, containers=containers, meta=meta, documented=documented))

    async def emit(self) -> RuntimeRail[ArtifactReceipt]:
        return await async_boundary(f"register.{self.op.tag}", self._emit, catch=_FAULTS)

    async def _emit(self) -> ArtifactReceipt:
        # the _composed render offloads off the loop through the bounded to_thread band so the GIL-releasing
        # great-tables/lxml/xlsxwriter render never blocks it; the returned receipt IS the ReceiptContributor.
        composed = await to_thread.run_sync(_composed, self, limiter=_GATE)
        key = ContentIdentity.of(f"register-{self.op.tag}", composed.data)
        return ArtifactReceipt.Register(
            key,
            composed.kind,
            composed.sheets,
            composed.suitability,
            composed.revision,
            composed.classification,
            composed.validation,
            len(composed.data),
        )

    @property
    def frame(self) -> pl.DataFrame:
        # the ONE register frame the Index TablePlan styles and the document/model TableNode lowers from, ordered
        # by discipline then numeric sheet ordinal so the discipline-grouped index and the document table read a
        # deterministic display order (and stable great-tables group order), never insertion order — the typed
        # `_sheet_ordinal` sorts `A-10` after `A-9` where a lexical `number` cast would not, and a polars `.sort`
        # would need a leaked ordinal column or a boundary-crossing `.str` extract the overlay reserves for `data`.
        if not self.containers:
            return pl.DataFrame()
        ordered = sorted(self.containers, key=lambda c: (c.discipline, _sheet_ordinal(c) or 0, c.number))
        return pl.from_dicts([container.row() for container in ordered])

    def audited(self, policy: AuditPolicy = _DEFAULT_AUDIT, /) -> RegisterEvidence:
        casualties = Block.of_seq(self.containers).collect(lambda c: _container_faults(c, policy)).append(_sequence_faults(self.containers, policy))
        severed = Nothing if casualties.is_empty() else Some(casualties.reduce(RegisterFault.combined))
        return RegisterEvidence.of(self, severed, casualties.is_empty())

    @property
    def evidence(self) -> RegisterEvidence:  # the default-policy coverage verdict delivery/transmittal composes as the issued manifest
        return self.audited()


# --- [OPERATIONS] -----------------------------------------------------------------------


def _accumulated(results: Block[Result[InformationContainer, RegisterFault]], /) -> Result[tuple[InformationContainer, ...], RegisterFault]:
    # the accumulating disposition: partition the railed containers, combine every fault through the monoid,
    # and return the whole set only when the casualty set is empty — the Validation-style fold, never an abort.
    faults = results.choose(lambda outcome: outcome.swap().to_option())
    return Ok(tuple(results.choose(lambda outcome: outcome.to_option()))) if faults.is_empty() else Error(faults.reduce(RegisterFault.combined))


@_GUARD
def _composed(register: Register) -> Composed:  # the one pure render fold both emit and _emit read
    validation = "unchecked"  # the non-XML ops mint no validatable container; only Container is schema-checked
    match register.op:
        case RegisterOp(tag="index", index=(theme, fmt)):
            data = TablePlan(frame=register.frame, ops=_index_ops(register), fmt=fmt, theme=theme).build()
        case RegisterOp(tag="container", container=dialect):
            tree = _container_document(register, dialect)
            schema = isoschematron.Schematron(
                etree.fromstring(_container_schema()), store_report=True
            )  # per-call: not re-entrant across the bounded _GATE threads
            validation = "valid" if schema.validate(tree) else f"invalid:{len(schema.error_log)}"
            data = etree.tostring(
                tree, method="c14n2"
            )  # C14N canonical bytes: the ISO 19650 container content-addresses byte-reproducibly run-to-run
        case RegisterOp(tag="audit", audit=policy):
            data = _ENCODER.encode(register.audited(policy).facts)
        case RegisterOp(tag="render", render=merge):
            data = _workbook(register, merge)
        case _ as unreachable:
            assert_never(unreachable)
    return Composed(
        data=data,
        kind=register.op.tag,
        sheets=len(register.containers),
        suitability=register.suitability.code,
        revision=register.revision.render(),
        classification=register.classification.system.value,
        validation=validation,
    )


def _index_ops(register: Register, /) -> tuple[TableOp, ...]:
    # the publication sheet-index TableOp sequence — the frame styled by visualization/table, row-grouped by
    # discipline (the AEC-canonical register organization), the state column CDE-coloured, one grand-summary
    # count; a new column is one TableOp, never a chain. State is a body column here, so it is coloured, not
    # the row-group (the group column leaves the body, so grouping and colouring one column would conflict).
    return (
        TableOp.Header(f"Drawing Register — {register.meta.project}", subtitle=f"{register.revision.render()} · {register.issued}"),
        TableOp.Stub(rowname="reference", group="discipline"),
        TableOp.Spanner("Identification", columns=["title", "form", "number"]),
        TableOp.Spanner("Status", columns=["suitability", "state", "revision", "classification"]),
        TableOp.Spanner("Responsibility", columns=["author", "checker", "approver"]),
        TableOp.Color(columns=["state"], palette=[_STATE_HEX[state] for state in ContainerState], domain=[state.value for state in ContainerState]),
        TableOp.GrandSummary({"containers": pl.col("reference").count().alias("containers")}),
        TableOp.SourceNote(f"ISO 19650-2 information containers · {register.classification.system.value}"),
    )


@cache
def _container_schema() -> bytes:
    # the ISO Schematron schema serialized ONCE (immutable bytes, thread-safe to share) — one non-empty
    # `sch:assert` per mandated `_REQUIRED_META` localname folded over each informationContainer, built through
    # the structured `etree.SubElement` node tree (never an f-string schema splice), the `ic` prefix bound to
    # `_NS_19650` so the context/test XPaths resolve against the emitted container tree. The validator wrapping
    # these bytes compiles per Container render (`isoschematron.Schematron` is not thread-re-entrant on `error_log`).
    sch = lambda local: etree.QName(_SCHEMATRON_NS, local)
    schema = etree.Element(sch("schema"), nsmap={"sch": _SCHEMATRON_NS})
    etree.SubElement(schema, sch("ns"), prefix="ic", uri=_NS_19650)
    rule = etree.SubElement(etree.SubElement(schema, sch("pattern")), sch("rule"), context="ic:informationContainer")
    for local in _REQUIRED_META:
        etree.SubElement(rule, sch("assert"), test=f"ic:{local} != ''").text = f"missing mandated ISO 19650 {local}"
    return etree.tostring(schema)


def _column_widths(containers: tuple["InformationContainer", ...], /) -> tuple[float, ...]:
    # the per-column display width from the header and the widest cell, bounded — the `set_column` sizing under
    # `constant_memory`, where the post-write `autofit` pass cannot read the already-flushed streamed widths.
    rows = (_COLUMNS, *(tuple(str(cell) for cell in c.spreadsheet_row()) for c in containers))
    return tuple(min(_MAX_WIDTH, max(len(row[col]) for row in rows) + _PAD) for col in range(len(_COLUMNS)))


def _running_header(register: Register, /) -> tuple[str, str]:
    # the print header/footer field-code strings xlsxwriter interpolates (`&P` page, `&N` of-N, `&D` print date);
    # dynamic values `&`-escaped so a project carrying `&` stays a literal rather than a spurious field code.
    project = register.meta.project.replace("&", "&&")
    return f"&L{project}&RPage &P of &N", f"&L{register.revision.render()}&C&D&R{register.issued}"


def _container_document(register: Register, dialect: ContainerDialect, /) -> "etree._Element":
    # the namespaced ISO 19650 container-metadata document — a structured lxml build, each field a Clark-notation
    # SubElement whose .text carries the value, never an f-string splice into markup (the injection form).
    qname = lambda local: etree.QName(_NS_19650, local)
    root = etree.Element(qname("informationContainerSet"), nsmap=dict(_NSMAP))
    root.set("dialect", dialect.value)
    header = etree.SubElement(root, qname("project"))
    for local, value in (
        ("name", register.meta.project),
        ("reference", register.meta.project_id),
        ("appointingParty", register.meta.appointing_party),
        ("leadAppointedParty", register.meta.lead_party),
        ("stage", register.meta.stage),
        ("milestone", register.meta.milestone),
        ("revision", register.revision.render()),
        ("issued", register.issued),
    ):
        etree.SubElement(header, qname(local)).text = value
    containers = etree.SubElement(root, qname("containers"))
    for container in register.containers:
        node = etree.SubElement(containers, qname("informationContainer"))
        node.set("reference", container.reference)
        for local, value in container.metadata_rows():
            etree.SubElement(node, qname(local)).text = value
    return root


def _workbook(register: Register, merge: Option[bytes], /) -> bytes:
    # the native register spreadsheet — constant-memory streaming rows, per-column sizing, suitability/state
    # conditional bands, owned-vocabulary validation dropdowns, per-reference asset hyperlinks, a page-of-N
    # running header + issue-date footer, and read-only protection on an issued register; a present merge
    # round-trips an existing client register first.
    containers = _merged(register, merge)
    header, footer = _running_header(register)
    sink = BytesIO()
    with xlsxwriter.Workbook(
        sink, {"constant_memory": True, "in_memory": True}
    ) as book:  # the with-exit close packages the zip into sink, deterministic on every exit
        sheet = book.add_worksheet("Register")
        head = book.add_format({"bold": True, "bg_color": "#1F2937", "font_color": "#FFFFFF", "border": 1})
        link = book.get_default_url_format()  # the shared hyperlink style, never a per-cell mint
        published = book.add_format({"bg_color": _STATE_HEX[ContainerState.PUBLISHED]})
        archived = book.add_format({"bg_color": _STATE_HEX[ContainerState.ARCHIVE]})
        for col, width in enumerate(_column_widths(containers)):  # set_column before the streamed rows flush
            sheet.set_column(col, col, width)
        sheet.write_row(0, 0, _COLUMNS, head)
        for index, container in enumerate(containers, start=1):
            row = container.spreadsheet_row()
            if container.asset_key:  # the Reference cell links to the co-identified durable artifact, the rest streams from column 1
                sheet.write_url(index, _REFERENCE_COL, f"{_ASSET_URI}{container.asset_key}", link, string=container.reference)
                sheet.write_row(index, _REFERENCE_COL + 1, row[_REFERENCE_COL + 1 :])
            else:
                sheet.write_row(index, 0, row)
        last = max(len(containers), 1)
        sheet.conditional_format(
            1,
            _SUITABILITY_COL,
            last,
            _SUITABILITY_COL,
            {"type": "text", "criteria": "begins with", "value": PublishedPrefix.AUTHORIZED.value, "format": published},
        )
        sheet.conditional_format(
            1, _STATE_COL, last, _STATE_COL, {"type": "text", "criteria": "containing", "value": ContainerState.ARCHIVE.value, "format": archived}
        )
        # the owned vocabularies as Excel validation: State the closed CDE list (hard-stop), Suitability the S-band
        # (soft — published A/B and documented project codes extend it), Revision the P/C grammar as input guidance.
        sheet.data_validation(
            1,
            _STATE_COL,
            last,
            _STATE_COL,
            {
                "validate": "list",
                "source": [s.value for s in ContainerState],
                "error_type": "stop",
                "input_title": "CDE state",
                "input_message": "One of the four ISO 19650 container states.",
            },
        )
        sheet.data_validation(
            1,
            _SUITABILITY_COL,
            last,
            _SUITABILITY_COL,
            {
                "validate": "list",
                "source": [c.value for c in SuitabilityCode],
                "error_type": "information",
                "input_title": "Suitability",
                "input_message": "Standard S-band; published A/B and documented project codes extend it.",
            },
        )
        sheet.data_validation(
            1,
            _REVISION_COL,
            last,
            _REVISION_COL,
            {"validate": "any", "input_title": "Revision", "input_message": "Preliminary P{NN}[.{NN}] or contractual C{NN}."},
        )
        sheet.autofilter(0, 0, last, len(_COLUMNS) - 1)
        sheet.freeze_panes(1, 0)
        sheet.set_landscape()
        sheet.set_paper(_A4_PAPER)
        sheet.fit_to_pages(1, 0)
        sheet.repeat_rows(0, 0)
        sheet.set_header(header)
        sheet.set_footer(footer)
        book.set_properties({
            "title": f"Drawing Register — {register.meta.project}",
            "subject": register.revision.render(),
            "author": register.meta.lead_party,
        })
        if register.issued:  # an issued register is read-only; a draft stays editable under the validation dropdowns
            sheet.protect(options={"autofilter": True, "sort": True, "select_locked_cells": True})
    return sink.getvalue()


def _merged(register: Register, merge: Option[bytes], /) -> tuple[InformationContainer, ...]:
    # the revision-latest merge — an existing client register ingested and each reference kept at its highest
    # revision, so a re-issue supersedes rather than duplicates; a Nothing merge is the register's own set.
    if merge.is_none():
        return register.containers
    seed: Map[str, InformationContainer] = Map.of_seq((c.reference, c) for c in _ingest(merge.value, register.documented))
    latest = Block.of_seq(register.containers).fold(
        lambda acc, c: acc.change(
            c.reference, lambda held, incoming=c: Some(incoming if held.is_none() or incoming.revision.succeeds(held.value.revision) else held.value)
        ),
        seed,
    )
    return tuple(latest.values())


def _ingest(data: bytes, documented: frozendict[str, ContainerState], /) -> tuple[InformationContainer, ...]:
    # the client-register read — openpyxl values-only rows admitted through the same pydantic gate under the
    # project's `documented` extension band so a documented `CR`/client code round-trips; malformed rows dropped.
    with load_workbook(BytesIO(data), read_only=True, data_only=True) as book:  # the read_only handle closes deterministically on with-exit
        rows = tuple(book[book.sheetnames[0]].iter_rows(min_row=2, values_only=True))
    return tuple(Block.of_seq(rows).choose(lambda row: InformationContainer.admitted(documented, **_row_payload(row)).to_option()))


def _row_payload(row: tuple[object, ...], /) -> ContainerPayload:
    # project a _COLUMNS-ordered client row into the ContainerPayload the admission gate validates; the naming
    # fields are recovered by splitting the reference on _FIELD_SEP, so the ingest reads the shape it emits.
    cells = tuple(str(cell) if cell is not None else "" for cell in row)
    padded = (*cells, *("" for _ in range(len(_COLUMNS) - len(cells))))
    parts = padded[0].split(_FIELD_SEP)
    fields = (*parts, *("" for _ in range(7 - len(parts))))
    return ContainerPayload(
        project=fields[0],
        originator=fields[1],
        functional=fields[2],
        spatial=fields[3],
        form=fields[4],
        discipline=fields[5],
        number=fields[6],
        suitability=padded[_SUITABILITY_COL],
        revision=padded[_COLUMNS.index("Revision")],
        title=padded[1],
        purpose=padded[_COLUMNS.index("Purpose")],
        classification=padded[_COLUMNS.index("Classification")],
    )


def _container_faults(container: InformationContainer, policy: AuditPolicy, /) -> Block[RegisterFault]:
    # one container's coverage faults — a data table of (predicate, RegisterFault) rows filtered to the flagged,
    # never a mutable append: the suitability/revision-kind disagreement, a withdrawn container issued, and the
    # policy-required classification / contractual suitability absences.
    ref = frozenset({container.reference})
    checks: tuple[tuple[bool, RegisterFault], ...] = (
        (container.suitability.contractual is not (container.revision.kind is RevisionKind.CONTRACTUAL), RegisterFault(misstated=ref)),
        (container.suitability.withdrawn, RegisterFault(withdrawn_issued=ref)),
        (policy.require_classification and not container.classification.code, RegisterFault(unclassified=ref)),
        (policy.require_contractual and not container.suitability.contractual, RegisterFault(misstated=ref)),
    )
    return Block.of_seq(fault for flagged, fault in checks if flagged)


def _sequence_faults(containers: tuple[InformationContainer, ...], policy: AuditPolicy, /) -> Block[RegisterFault]:
    # the set-level integrity faults — a non-monotonic revision on a repeated reference (a fold threading the
    # last-seen revision), a Counter-detected duplicate reference, and a gap in the numeric sheet-index sequence.
    if not policy.require_sequence:
        return Block.empty()

    def step(acc: tuple[Map[str, RevisionCode], frozenset[str]], container: InformationContainer, /) -> tuple[Map[str, RevisionCode], frozenset[str]]:
        seen, flagged = acc
        prior = seen.try_find(container.reference)
        regressed = prior.is_some() and not container.revision.succeeds(prior.value)
        return seen.add(container.reference, container.revision), (flagged | {container.reference} if regressed else flagged)

    _, regressed = Block.of_seq(containers).fold(step, (Map.empty(), frozenset()))
    duplicated = frozenset(ref for ref, count in Counter(c.reference for c in containers).items() if count > 1)
    ordinals = sorted({ordinal for container in containers if (ordinal := _sheet_ordinal(container)) is not None})
    gaps = frozenset(str(missing) for low, high in pairwise(ordinals) for missing in range(low + 1, high))
    faults: tuple[tuple[bool, RegisterFault], ...] = (
        (bool(regressed), RegisterFault(non_monotonic=regressed)),
        (bool(duplicated), RegisterFault(duplicate=duplicated)),
        (bool(gaps), RegisterFault(gap=gaps)),
    )
    return Block.of_seq(fault for flagged, fault in faults if flagged)


def _sheet_ordinal(container: InformationContainer, /) -> int | None:
    return int(digits) if (digits := "".join(ch for ch in container.number if ch.isdigit())) else None
```

## [03]-[RESEARCH]

- [ISO_19650_SUITABILITY] [RESOLVED]: the `SuitabilityCode` `S0..S7` band, its `_SUITABILITY` descriptions/states/withdrawn flags, the `PublishedCode` `A1..An`/`B1..Bn` parametric published families, and the four `ContainerState` CDE states verify against BS EN ISO 19650‑2:2018 UK National Annex Table NA.1 (§NA.4.2). `S5` is the withdrawn member kept in the sequence so the cardinality is exact (the `withdrawn` flag the audit reads, never a silent omission that mis-states the published set as seven codes); the published `A`/`B` codes are open-ordinal families whose reason-for-issue `An` is defined in the project's information standard, so `PublishedCode` carries the ordinal rather than a fixed enum, and the NA's "codes may be expanded (or excluded) to suit specific project requirements providing the required codes are documented in the project's information standard" clause is the `Suitability.project` semi-closed band (an archive `CR` as-constructed record lands there, never a fabricated closed `RecordCode` enum claiming ISO standing). The `RevisionCode` `P{NN}[.{NN}]` preliminary / `C{NN}` contractual form and the two-integer WIP `version` suffix verify against §NA.4.3; classification per §NA.4.4 is ISO 12006‑2 (UK Uniclass 2015), carried as the lean `Classification` reference the `specification/classify#CLASSIFY` code tables own.
- [SEAM_AND_PACKAGE] [RESOLVED]: `TablePlan(frame, ops, fmt, theme)` / `.build()`, `TableOp.Header`/`Stub`/`Spanner`/`Color`/`RowGroupOrder`/`GrandSummary`/`SourceNote`, `Theme`, `TableFormat`, and the `frame.style → GT` seam verify against `visualization/table#TABLE` and the `.api` `great-tables`/`polars` catalogues, so the `Index` publication index lowers through the sibling table owner's public `build` bytes seam (SINGLE-FACT, one render one content key) exactly as `drawing/schedule#SCHEDULE` does. `xlsxwriter.Workbook(constant_memory=True)`/`add_worksheet`/`add_format`/`get_default_url_format`/`set_column`/`write_row`/`write_url`/`conditional_format`/`data_validation`/`autofilter`/`freeze_panes`/`set_landscape`/`set_paper`/`fit_to_pages`/`repeat_rows`/`set_header`/`set_footer`/`protect`/`set_properties`/`close`, `openpyxl.load_workbook(read_only=True, data_only=True)`/`Workbook.sheetnames`/`Worksheet.iter_rows(values_only=True)`, and `lxml.etree.Element`/`SubElement`/`QName`/`fromstring`/`tostring(method="c14n2")`/`isoschematron.Schematron`/`.validate`/`.error_log` verify against the folder `.api` catalogues; `conditional_format`/`data_validation`/`write_url`/`set_column`/`set_header`/`set_footer`/`protect` are all available under `constant_memory` (only `add_table` and the post-write `autofit` are not — hence the `_column_widths` `set_column` pre-pass), so the validation dropdowns, asset hyperlinks, sized columns, running header, and issued lock ride the streaming path. C14N (`method="c14n2"`, W3C Canonical XML 2.0) yields the byte-reproducible delivery container the content key seeds on, and the owned `isoschematron.Schematron` (ISO phases/SVRL over the partial libxml2 `etree.Schematron`) validates the SERIALIZED §5.1.7 metadata presence — orthogonal to `audited`, which checks the domain-model VALUE coherence. `composition/sheet#SHEET` `TitleBlock` supplies the aggregated sheet-index facts through `SheetEntry`, and `core/receipt#RECEIPT` gains the `ArtifactReceipt.Register(...validation, bytes)` case the mint composes.

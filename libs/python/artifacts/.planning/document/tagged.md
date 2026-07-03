# [PY_ARTIFACTS_TAGGED]

The PDF/UA (ISO 14289-1) structural close over the document rail, unified with the ISO 15930 PDF/X print-production close. `Access` is ONE owner that authors the marked-content structure-element tree into an emitted PDF, audits its conformance against BOTH a `pikepdf` self-audit AND an independent in-process oracle, upgrades the emitted PDF to a PDF/A archival profile, AND preflights it against a PDF/X print-production oracle — discriminating a closed `AccessOp` (`TAG`/`AUDIT`/`ARCHIVE`/`PREFLIGHT`) over the frozen `_ARM` `frozendict` data-row dispatch, never a `tag_pdf`/`audit_pdf`/`archive_pdf`/per-role writer family and never a `MappingProxyType` view. `TAG` lowers the `document/model#NODE` `StructureNode` tree into the `pikepdf` `/StructTreeRoot` (the `/Type /StructElem` indirect dictionaries keyed by `/S`, the multi-page `/ParentTree` number-tree, the `/RoleMap` foreign→standard map, the `/MarkInfo /Marked` flag) AND the catalog-level PDF/UA requirements ISO 14289 mandates beyond the tree — the `/Lang` natural-language tag, the `/ViewerPreferences /DisplayDocTitle` flag, and the XMP `pdfuaid:part` identifier plus `dc:title` over `Pdf.open_metadata`; `AUDIT` walks the authored tree AND the catalog into a typed `StructureAudit` whose `failures` is a closed `UaCheck` clause set RECONCILING the `pikepdf` per-clause self-audit against the `pdf_oxide` `validate_pdf_ua` veraPDF-grade in-process oracle, and whose `conformant` is its emptiness; `ARCHIVE` upgrades the emitted PDF to the ISO 19005 archival profile through `pdf_oxide` `convert_to_pdf_a`, the archival sibling of the accessibility close over the same emitted-PDF bytes; `PREFLIGHT` validates the emitted PDF against the ISO 15930 PDF/X print-production profile through `pdf_oxide` `validate_pdf_x`, reconciling the document's OWN declared /pdfxid against that independent oracle so the `pdfx_claim` the AUDIT records as decorative evidence becomes a TWO-SOURCE verdict — the print-plane sibling of the ARCHIVE close under the unified publication/print telos, closing the decorative-claim trap the `[02]` boundary warned about.

The structure vocabulary is CONSUMED, never re-minted: `document/model#NODE` owns the `StructureNode` variant, the `StructEltKind`/`StructRole` family, the `_STRUCT_CATEGORY` behavior table, and the `role_of`/`role_category`/`alt_of`/`children`/`standard_for` projections this owner reads, so `tagged.md` lowers FROM that algebra into PDF marked content rather than re-declaring a `_STANDARD_ROLES`/`_STANDARD_FOR` parallel — the `/S`-string algebra the audit reads is DERIVED once from `StructEltKind` (`_ELT` decoding a read `/S` Name to its model member, its key set the standard-structure membership the `/RoleMap` completeness check reads; `_CATEGORY` projecting each member to the model `role_category` `(StructCategory, heading_level)` row the heading and nesting checks fold over), tracking the model vocabulary with zero re-declared `_HEADINGS`/`_STANDARD_TYPES` literal. `pikepdf` and `pdf_oxide` are admitted and ungated; this owner closes an already-emitted PDF, contributes the settled `core/receipt#RECEIPT` `Egress`/`Pdf` cases through the `@receipted(OPEN)` harvest weave over a thin pure `_emit`, and threads `StructureAudit.conformant` into the `exchange/conformance#CONFORMANCE` `AuditSpec.structural_conformant` so the structural verdict pyhanko honestly disclaims is closed by the producer that authored the structure AND cross-checked it against an independent commercial-safe oracle. Every arm returns a `RuntimeRail[ContentKey]` keyed by the runtime content key.

## [01]-[INDEX]

- [01]-[ACCESS]: `pikepdf` `StructTreeRoot`/`StructElem` marked-content tagged-PDF authoring (`TAG`, its `/ParentTree` the modeled `pikepdf.NumberTree` and its re-emit `deterministic_id=True` for stable content-addressing), the two-source ISO 14289-1 structural audit reconciling the `pikepdf` self-audit against the `pdf_oxide.validate_pdf_ua` in-process oracle (`AUDIT`), the `pdf_oxide.convert_to_pdf_a` ISO 19005 archival upgrade (`ARCHIVE`), and the `pdf_oxide.validate_pdf_x` ISO 15930 PDF/X print-production preflight reconciling the declared /pdfxid against the oracle (`PREFLIGHT`) owner over the closed `AccessOp` `_ARM` `frozendict` dispatch; `AccessParams` is the one trusted authoring policy (`source` tree, `lang`/`title`/`ua_part` catalog requirements, `pdfa_level` the archival target, `pdfx_level` the print-production target); `AccessFact` the bytes-plus-evidence carrier each arm threads onto the frozen owner through `structs.replace`; `StructureAudit` the typed structural-conformance verdict whose `UaCheck` `failures` (the fourteen structural clauses PLUS the `pikepdf` `check_pdf_syntax` `SYNTAX` clause, the `pdf_oxide` per-page `has_text_layer` `TEXT_LAYER` and `has_xfa` `NO_XFA` clauses, and the `pdf_oxide` oracle `ORACLE` cross-check) plus the `structured_warnings` diagnostic evidence set the `core/receipt#RECEIPT` `Egress`/`Pdf` cases evidence and the `exchange/conformance#CONFORMANCE` `AuditSpec.structural_conformant` consumes; `PreflightAudit` the typed PDF/X print-production verdict whose `PreflightCheck` `failures` (`PDFX_VALID` the oracle pass, `CLAIM_HONEST` the declared-claim reconciliation) gate its `conformant`; the `@receipted(OPEN)` weave harvests `contribute` off the stepped owner the pure `_emit` returns and `async_boundary` is the fault-converting capsule.

## [02]-[ACCESS]

- Owner: `Access` the one PDF conformance-close owner discriminating the access op; `AccessOp` the closed `StrEnum` over tag-authoring, structural audit, archival upgrade, and PDF/X print-production preflight; `_ARM` the `frozendict[AccessOp, Arm]` data-row dispatch mapping each op to its single `AccessFact`-returning arm with zero `match`/`case` sprawl, the closed `StrEnum` membership total over the table by construction (the sibling `document/egress#FINISH` `FINISHERS` `frozendict`-table dispatch shape, modernized off `MappingProxyType` onto the `frozendict` builtin). `pikepdf` owns the qpdf object model (`Object`/`Dictionary`/`Array`/`Name`/`String`), the indirect-object mint (`Pdf.make_indirect`), the catalog (`Pdf.Root`), the XMP metadata context (`Pdf.open_metadata` → `PdfMetadata`, carrying `pdfa_status`/`pdfx_status`), the syntax gate (`Pdf.check_pdf_syntax` → `list[str]`), the page collection, and the content-stream mint (`Pdf.make_stream`)/marked-content emitter; `pdf_oxide` owns the independent MIT/Apache in-process Rust oracle (`PdfDocument.validate_pdf_ua` → the `{valid, errors, warnings}` PDF/UA verdict, `validate_pdf_x(level)` → the `{valid, level, errors, warnings}` PDF/X print-production verdict the PREFLIGHT reconciles, `structured_warnings` → the `{category, page, message, spec_section}` structure-diagnostic rows the AUDIT folds as evidence, `has_structure_tree` the tagged-PDF confirmation, `convert_to_pdf_a` → the `{success, actions, errors}` in-place PDF/A upgrade, `to_bytes`/`page_count`, all under the `__enter__`/`__exit__` deterministic-close capsule); `document/model#NODE` owns the source `StructureNode` tree, the `StructEltKind`/`StructRole`/`StructCategory` vocabulary, the `_STRUCT_CATEGORY` table, and the `role_of`/`role_category`/`alt_of`/`children`/`standard_for` projections this owner reads rather than re-deriving — the `TAG` arm reads `role_of`/`standard_for` for the `/S` write and the `/RoleMap` lowering, the `AUDIT` arm reads `role_category` (through the derived `_CATEGORY` table) for its heading-level and structure-nesting clauses. `StructureAudit` is the carried structural verdict; `AccessParams` the trusted policy bundle; `AccessFact` the threaded evidence.
- Cases: `AccessOp` rows `TAG` (fold the `StructureNode` tree into the `pikepdf` structure tree — set `/MarkInfo << /Marked true >>` and `/ViewerPreferences << /DisplayDocTitle true >>` on the catalog, write `/Lang` from `AccessParams.lang` or the root node `NodeMeta.lang`, write the XMP `pdfuaid:part` identifier and `dc:title` through `open_metadata`, mint the indirect `/StructTreeRoot` carrying `/K`/`/ParentTree`/`/ParentTreeNextKey`/`/RoleMap`, recurse the `StructureNode` children into indirect `/Type /StructElem` dictionaries keyed by `/S` the `role_of` value, attach `/P`/`/Pg`/`/Alt` (from the `FigureNode` child `alt_of`)/`/ActualText`+`/Lang` (from `NodeMeta`), assign each leaf an integer MCID into its page's parent-tree slot keyed by the page `/StructParents` index through the modeled `pikepdf.NumberTree` mapping-view (the `/ParentTree` IS a PDF number-tree, so `NumberTree.new(pdf)` + `parent_tree[page_key] = Array(...)` + `struct_root.ParentTree = parent_tree.obj` replaces the hand-assembled flat `Nums` Array), and re-emit through `Pdf.save(deterministic_id=True)` under a pinned `settings.set_decimal_precision` so two identical-tree runs content-key identically) · `AUDIT` (walk the authored `/StructTreeRoot` `/K` spine AND the catalog/XMP into a `StructureAudit`, then RECONCILE that `pikepdf` self-audit against the independent `pdf_oxide.validate_pdf_ua` oracle — the element count, tag-tree depth, per-role tallies, figure-alt coverage, heading monotonicity, role-map completeness, standard-legal structure nesting (list/table AND ruby/warichu), table/list/link structure, multi-page `/StructParents` keying, the `check_pdf_syntax` well-formedness gate, the per-page `has_text_layer` full-coverage `TEXT_LAYER` gate (an image-only page fails PDF/UA even when tagged), the `has_xfa` `NO_XFA` gate (dynamic XFA is prohibited), the oracle's `valid`/`errors`/`warnings` verdict AND its independent `has_structure_tree` confirmation folded into the two-source `STRUCT_TREE` clause (`has_struct and has_tree` — the tree exists only when BOTH engines agree), and the closed `UaCheck` `failures` set gating `conformant`) · `ARCHIVE` (upgrade the emitted PDF to the ISO 19005 archival profile — `pdf_oxide.convert_to_pdf_a(pdfa_level)` normalizes the document in place, then `pdf_oxide.validate_pdf_a(pdfa_level)` independently re-validates the SAME upgraded handle so the convert never self-certifies; `to_bytes` re-emits the archival bytes, the `{success, actions, errors}` convert outcome and the `{valid, errors}` post-upgrade verdict together riding the fact's `applied`/`residual` counts, `residual` summing the converter's own errors and the oracle's post-upgrade failures — the two-source archival close mirroring the AUDIT) · `PREFLIGHT` (validate the emitted PDF against the ISO 15930 PDF/X print-production profile — `pdf_oxide.validate_pdf_x(pdfx_level)` returns the `{valid, level, errors, warnings}` verdict, reconciled against the document's OWN declared /pdfxid read read-only through `pikepdf.open_metadata(set_pikepdf_as_editor=False).pdfx_status` so a claim the oracle refutes fails the `CLAIM_HONEST` clause, `structured_warnings` folded as diagnostic evidence, the `PreflightAudit` whose `PDFX_VALID`/`CLAIM_HONEST` `failures` gate `conformant` content-addressed by the PREFLIGHT key — the print-plane sibling of ARCHIVE turning the decorative `pdfx_claim` into a two-source verdict) — selected by the frozen `_ARM` row, never a chain of `is`-probes. Role lowering is one model read: a `StandardRole` writes `/S` the `StructEltKind` value directly, a `ForeignRole` writes `/S` the foreign role string AND registers `/RoleMap[<foreign>] = standard_for(role)` so the non-standard role maps to a standard structure type per the PDF/UA requirement, the foreign→standard correspondence resolved through the model `standard_for` projection rather than a parallel table owned here.
- Auto: `_tag` opens the PDF through `pikepdf.open`, authors the catalog requirements (`Root.MarkInfo`/`Root.ViewerPreferences`/`Root.Lang` plus the `open_metadata` XMP `pdfuaid:part`/`dc:title`), mints the indirect `/StructTreeRoot`, then folds `_elem` over the source tree once threading one `_Author` boundary accumulator (the foreign→standard `role_map`, the per-page `slots` parent-tree arrays where `slots[page][mcid]` is the `StructElem` owning that marked-content id, and the running element/figure counts). `_elem` mints one `/Type /StructElem` per `StructureNode`, writes `/S` through `role_of`, registers a `ForeignRole` in `role_map` through `standard_for`, writes `/Alt` from the `FigureNode` child's `alt_of` and `/ActualText`/`/Lang` from `NodeMeta`, recurses the `StructureNode` branches into `/K`, and binds a content leaf's MCID into `slots[node.meta.page]` — the structural recursion driven by the model `children` projection, never a re-enumeration of the node variants; the in-stream `/Tag <</MCID n>> BDC … EMC` operator emission for each region is `document/emit#DOCUMENT`'s drawing concern, authored in the same document order so the MCIDs `_elem` assigns match the regions emit marked. `_tag` then populates the per-page slots into a `pikepdf.NumberTree.new(pdf)` (binding `struct_root.ParentTree = parent_tree.obj`, the modeled number-tree primitive over the hand-built flat `Nums` Array), writes the `/StructParents` page keys and the `/RoleMap`, pins `settings.set_decimal_precision`, and `Pdf.save(deterministic_id=True)` re-emits the enriched bytes with a reproducible `/ID` so two identical-`StructureNode`-tree runs content-key identically. `_audit` reopens the PDF, walks the `/StructTreeRoot` `/K` spine through the depth-safe `_walk` frontier (one immutable `Block` work-stack threading `(elem, depth, parent)` in pre-order document order, never native recursion over the UNTRUSTED external tree — the `document/model#NODE` `walk`/`node_digest` law applied to the adversarial-depth audit read) discriminating each kid by its qpdf-coerced kind (`isinstance` against `pikepdf.Dictionary`/`pikepdf.Array`, the surface of the `pikepdf.ObjectType` axis: a `Dictionary` `/Type /StructElem` is a child structure element pushed onto the frontier; an `int` MCID and an `/MCR`/`/OBJR` dictionary are content leaves), decoding each read `/S` Name to its model `StructEltKind` through `_ELT` and `match`-ing on that member rather than a `/Figure`/`/Table` string literal, folds one `_Tally` boundary accumulator over the per-role facts — the heading bucket and level read off `_CATEGORY[elt]` (never a `_HEADINGS` set or an `int(role[2:])` slice), the nesting clause threading each element's parent member down the recursion and tallying a `misnested` count against the `_NESTING` legal-parent policy — resolves every `pikepdf`-touching clause predicate to a plain `bool`/`int`/`str` BEFORE the handle frees (never a qpdf `Object` escaping the borrow window), reads the `check_pdf_syntax` warning count and — through a read-only `open_metadata(set_pikepdf_as_editor=False, update_docinfo=False)` that never mutates the bytes it audits — the `pdfuaid:part`/`dc:title` presence and the `pdfa_status`/`pdfx_status` declared claims, THEN opens the independent `pdf_oxide.PdfDocument` oracle to read the `validate_pdf_ua` `{valid, errors, warnings}` verdict, the `has_structure_tree` confirmation, the `has_xfa` prohibited-form probe, the per-page `has_text_layer` coverage sum, and the `structured_warnings` diagnostic count once each (the structure confirmation reconciled into the two-source `STRUCT_TREE` clause `has_struct and has_tree`, the `warnings` count kept as whole-verdict evidence so no oracle key is a 2-of-3 slice, the `structured_warnings` rows folded as additional structural evidence beside the clause set, the text-layer sum gating `TEXT_LAYER` on full-page coverage and the XFA probe gating `NO_XFA`), and projects the `StructureAudit` whose `failures` is the one data-driven fold over the `(UaCheck, predicate)` rows, never a per-metric re-traversal. `_archive` opens the `pdf_oxide.PdfDocument`, runs `convert_to_pdf_a(pdfa_level)` in place, independently re-validates the upgraded handle through `validate_pdf_a(pdfa_level)`, and returns the `to_bytes` archival PDF with the `applied` action count and the `residual` summing the converter's own errors and the oracle's post-upgrade conformance failures. `_preflight` reads the document's OWN declared /pdfxid read-only through `pikepdf.open_metadata(set_pikepdf_as_editor=False, update_docinfo=False).pdfx_status` (resolved to a plain `str` before the qpdf handle frees), THEN opens the independent `pdf_oxide.PdfDocument` oracle to read `validate_pdf_x(pdfx_level)` `{valid, level, errors, warnings}`, `structured_warnings`, and `page_count` once each, and folds the `(PreflightCheck, predicate)` rows — `PDFX_VALID` on the oracle's `valid and errors == 0`, `CLAIM_HONEST` on `not pdfx_claim or valid` (a declared claim the oracle refutes fails) — into the content-addressed `PreflightAudit`, the two-source print-production close mirroring the AUDIT's structural close.
- Receipt: the `TAG` arm contributes `ArtifactReceipt.Egress` carrying the content key, the post-author byte count, the page count, and the structural facts mapped onto the existing flat `Egress` slots (the structure-element count riding `outline_depth`, the figure count riding `overlays` — the finishing-facts case the `document/egress#FINISH` owner shares); the `ARCHIVE` arm contributes the SAME `Egress` case carrying the archival byte count, page count, the applied-action count on `outline_depth`, and the residual-error count on `overlays` (the two produce ops sharing the one content-composition finishing case, exactly as egress folds an OCG-strip and a form-flatten onto those slots); and the `AUDIT` AND `PREFLIGHT` arms both contribute `ArtifactReceipt.Pdf` carrying the content key, the audited/preflighted byte count, and the page count — the settled `core/receipt#RECEIPT` `Egress`/`Pdf` reuse target the receipt page fixes for this producer, never a fifteenth receipt case (the two producing ops share `Egress`, the two validating ops share `Pdf`). The `@receipted(OPEN)` weave drains `contribute` off the stepped owner the pure `_emit` returns and emits through the runtime `Signals.emit_async` without an inline emit per arm — `OPEN` the keep-all redaction policy the runtime receipts owner exports (access facts carry no classified field), never a re-minted per-file `Redaction`; `contribute` reads the threaded `AccessFact` and folds the case off the `op` discriminant through one total `match` closed by `assert_never`. The `StructureAudit` value is content-addressed by the `AUDIT` content key (its `msgpack` encoding IS the audited artifact) and decoded by the composition root to thread `conformant` into the `exchange/conformance#CONFORMANCE` `AuditSpec.structural_conformant`, so the structural result is interior evidence the `ConformanceVerdict` carries rather than a new receipt field, the same one-way acyclic edge the `ConformanceVerdict` value carries; the `PreflightAudit` is the print-plane twin, content-addressed by the `PREFLIGHT` key and decoded for its own `conformant` print-production verdict, both verdicts riding the `AccessFact` (`audit`/`preflight`) the byte-only `Pdf` case keys.
- Packages: `pikepdf` (`open` document factory; `Pdf.Root` catalog mutation; `Pdf.make_indirect` indirect-object mint; `Pdf.make_stream` content-stream mint; `Pdf.open_metadata(set_pikepdf_as_editor=, update_docinfo=)` → `PdfMetadata` XMP context mapping (the `TAG` write-back editor form; the `AUDIT` read-only `False`/`False` form that never mutates the audited bytes) carrying `pdfa_status`/`pdfx_status` conformance-claim probes; `Pdf.check_pdf_syntax` → `list[str]` qpdf-check structural warnings; `Pdf.pages`/`Pdf.save(deterministic_id=True)` the reproducible-`/ID` re-emit; `NumberTree.new(pdf)` the modeled `/ParentTree` number-tree mapping-view (`parent_tree[page_key] = Array(...)`, `.obj` the backing dict); `settings.set_decimal_precision` the pinned real-number precision; the `Object` model `Dictionary`/`Array`/`Name`/`String`/`Boolean` typed scalars with `.get`/`.keys` traversal; the `ObjectType` kind discriminant for the read walk; `Page.obj` the backing page dictionary the `/StructParents` index binds onto; `Page.contents_add` + `canvas.ContentStreamBuilder.begin_marked_content` the marked-content operator surface emit composes); `pdf_oxide` (`PdfDocument.from_bytes` byte intake; `validate_pdf_ua` → the `{valid, errors, warnings}` in-process Rust PDF/UA oracle verdict; `validate_pdf_x(level)` → the `{valid, level, errors, warnings}` PDF/X print-production verdict the `PREFLIGHT` reconciles against the declared /pdfxid; `structured_warnings` → the `{category, page, message, spec_section}` structure-diagnostic rows the `AUDIT` folds as evidence; `has_structure_tree` the independent tagged-PDF confirmation; `has_text_layer(page)` the per-page real-text probe the `TEXT_LAYER` clause sums; `has_xfa` the prohibited-dynamic-form probe the `NO_XFA` clause reads; `convert_to_pdf_a` → the `{success, actions, errors}` in-place ISO 19005 upgrade; `validate_pdf_a` → the `{valid, level, errors, warnings}` independent post-convert PDF/A cross-check the `ARCHIVE` residual folds; `to_bytes`/`page_count` (the int-like `_PageCount` wrapper, read through `int(...)`) the archival emit + count; `__enter__`/`__exit__` the deterministic-close capsule); `document/model#NODE` (`StructureNode`/`FigureNode`, the `StructEltKind`/`StandardRole`/`ForeignRole`/`StructCategory`/`LangTag` vocabulary, `role_of`/`role_category`/`alt_of`/`children`/`standard_for` the consumed tree algebra — never re-minted); `msgspec` (`Struct(frozen=True)` the value owners, `structs.replace` the fact thread, `msgpack.Encoder(order="deterministic")` the `StructureAudit` content codec, `UnsetType` the `NodeMeta` absent-marker read); `expression` (`Result`/`Ok`/`Error`/`tagged_union` the admission rail and fault family; `Block.singleton`/`of_seq`/`append`/`head`/`tail` the `_walk` depth-safe frontier work-stack over the untrusted `/K` spine); `itertools` (`pairwise` the heading-monotonicity consecutive-level pairing); `anyio` (`to_thread.run_sync` under a bounded `CapacityLimiter` the GIL-releasing native fold crosses); `frozendict` (the `_ARM` dispatch table, the model-derived `_ELT`/`_CATEGORY` `/S`-string projections, the `_NESTING` legal-parent policy); runtime (`content_identity.ContentIdentity`/`ContentKey`, `faults.RuntimeRail`/`async_boundary`, `receipts.OPEN`/`Receipt`/`receipted`).
- Growth: a new access op is one `AccessOp` row plus one `_ARM` arm entry (`ARCHIVE` and `PREFLIGHT` are exactly this — one row, one arm, one `Egress`/`Pdf` case reuse, `PREFLIGHT` also one `PreflightAudit` verdict struct + one `PreflightCheck` clause set + one `AccessFact.preflight` field for the distinct print-production plane); a new structural conformance clause is one `UaCheck` member plus one `(UaCheck, predicate)` row in the `failures` fold (`SYNTAX`, `TEXT_LAYER`, `NO_XFA`, and `ORACLE` are exactly this — one member and one fold row each, `TEXT_LAYER`/`NO_XFA` each adding one oracle read plus one `StructureAudit` evidence field); a new structural metric or oracle evidence field is one `StructureAudit` field plus one `_Tally` slot or oracle read; a new standard nesting rule is one `_NESTING` row keyed by the constrained `StructEltKind` (the ruby/warichu assembly rows are exactly this, tracking the model vocabulary); a new standard PDF/UA role is absorbed upstream as one `document/model#NODE` `StructEltKind` member (the `_elem` `/S` write, the `standard_for` lowering, and the derived `_ELT` membership plus the `_CATEGORY` heading/nesting row pick it up with zero change here); a foreign role rides the one `ForeignRole` arm and the `/RoleMap` registration; a new catalog requirement is one `AccessParams` field; a new archival level is one `PdfaLevel` `Literal` member; a new print-production level is one `PdfxLevel` `Literal` member; zero new surface.
- Boundary: no document authoring (that stays at `document/emit#DOCUMENT`, which authors a born-PDF/A document from the `DocumentNode` tree — distinct from `ARCHIVE`'s in-place upgrade of an ALREADY-emitted PDF), no structure-tree TYPE ownership (the `StructureNode`/`StructEltKind`/`StructRole` family and the `_STRUCT_CATEGORY`/`_STANDARD_FOR` tables are `document/model#NODE`'s, consumed here), no PDF signing (that is `exchange/conformance#CONFORMANCE`), no security/navigation finishing (that is `document/egress#FINISH`), no OCG optional-content authoring (that is `export/layered#LAYERED`); the owner authors, audits, and archival-upgrades the marked-content structure tree plus the catalog PDF/UA requirements over an already-emitted PDF. `pikepdf` exposes no high-level `StructTreeRoot`/`StructElem` helper class — the tree is authored over the raw `Object` model with `make_indirect`, so a phantom `pdf.add_structure_tree()` convenience is the rejected form and the object-model spike is the real surface; `pdf_oxide.DocumentBuilder.tagged_pdf_ua1()`/`role_map` is the alternative fluent tagged-PDF/UA author to this raw spike, reserved for a from-scratch born-tagged document at `document/emit#DOCUMENT`, never a second structure author over an existing PDF here. The deleted forms: a `MappingProxyType` dispatch where the `frozendict` builtin is the table owner; a re-declared `_STANDARD_ROLES` frozenset and a `_STANDARD_FOR` `dict[str, str]` keyed by category string where the model owns the `StructEltKind` vocabulary and the `standard_for` projection; a `MarkContext` `msgspec.Struct` with mutable `{}`/`[]` defaults where a `_Author` boundary `dataclass` accumulator threads the spike; a single-page `pages[0]`/`StructParents = 0` hardcode where the multi-page `/ParentTree` keys every page by `NodeMeta.page`; a hand-assembled flat `struct_root.ParentTree.Nums = Array(nums)` where the `/ParentTree` IS a PDF number-tree the modeled `pikepdf.NumberTree.new(pdf)` mapping-view owns; a non-deterministic `pdf.save(sink)` whose randomized `/ID` content-keys two identical-`StructureNode`-tree TAG runs differently and defeats the reuse-fabric elision the receipt spine depends on, where `save(deterministic_id=True)` under a pinned `settings.set_decimal_precision` is the content-addressing precondition (the audit encoder already carries `order="deterministic"` but the TAG byte emit did not); a `/Alt`-only figure check ignoring `/ActualText`; a naive seven-field `StructureAudit` slice ignoring `/Lang`, document title, the XMP `pdfuaid:part`, `/Suspects`, role-map completeness, standard structure nesting, table/list/link structure, `check_pdf_syntax` well-formedness, AND the independent oracle cross-check; a decorative `has_structure_tree` confirmation computed, stored on `StructureAudit.has_tree`, and prose-claimed to cross-check `STRUCT_TREE` yet never folded into any clause — the illusory-density trap where the oracle probe rode as ornament rather than gating conformance, now reconciled into the two-source `STRUCT_TREE` predicate `has_struct and has_tree`; the SAME decorative-evidence trap in the `pdfx_claim` XMP `pdfx_status` read stored as `StructureAudit.pdfx_claim` yet never independently validated — the exact defect this rebuild closes — now turned into a two-source verdict by the `PREFLIGHT` op's `validate_pdf_x` reconciliation, a declared /pdfxid the independent oracle refutes failing the `CLAIM_HONEST` clause, the print-plane sibling of the ARCHIVE PDF/A re-validation; a `validate_pdf_ua` verdict read for `valid`/`errors` while discarding `warnings` (a 2-of-3 oracle slice) where the whole verdict is captured; an `ARCHIVE` arm trusting `convert_to_pdf_a`'s own `success` self-report with no independent `validate_pdf_a` re-validation of the upgraded bytes; a native-recursion `_walk` over the UNTRUSTED external `/StructTreeRoot` spine whose adversarial nesting depth overflows the interpreter frame limit where the depth-safe `Block` frontier the `document/model#NODE` owner mandates is the form; an `AUDIT` reading metadata through the default write-back `open_metadata` that mutates the very bytes it audits where the read-only `set_pikepdf_as_editor=False`/`update_docinfo=False` form leaves them untouched; a `tag_pdf`/`audit_pdf`/`archive_pdf` writer family; an inline-described receipt no `contribute`/`@receipted` weave wires; a re-minted per-file `Redaction(classified=Map.empty())` where the runtime `OPEN` keep-all policy rides directly (the `document/egress#FINISH` convention); a stringly-typed `/S` bypassing the `StructEltKind` vocabulary; a string-literal `match role` re-spelling `/Figure`/`/Table` plus a re-derived `_HEADINGS` frozenset and an `int(role[2:])` level slice; a content-addressed `StructureAudit` encoder lacking `order="deterministic"`; a qpdf `Object` escaping its `with` borrow window into the post-close `failures` fold (every clause resolves to a plain value first); and — the prime illusory-density defect this rebuild removes — a self-audit DISCLAIMING any external grade with the stale `no pure-Python PDF/UA structural validator resolves on PyPI` / `no JVM, no external grade` framing, FALSIFIED by the installed `pdf_oxide.PdfDocument.validate_pdf_ua` (an in-process Rust, MIT/Apache, commercial-safe, veraPDF-grade PDF/UA conformance oracle): `conformant` is now empty only when the `pikepdf` per-clause self-audit AND that independent oracle BOTH agree, the self-audit owning the explainable per-clause verdict and the oracle owning the cross-check the clause set cannot enumerate. The `AUDIT` boolean threads into `exchange/conformance#CONFORMANCE` as interior structural evidence, closing the gap pyhanko discloses with a genuine two-source verdict rather than a lone self-assessment.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
from collections.abc import Callable, Iterable
from dataclasses import dataclass, field as dc_field
from enum import StrEnum
from io import BytesIO
from itertools import pairwise
from typing import TYPE_CHECKING, Final, Literal, Self, assert_never

import msgspec
from anyio import CapacityLimiter, to_thread
from builtins import frozendict
from expression import Error, Ok, Result, case, tag, tagged_union
from expression.collections import Block
from msgspec import Struct, field, structs

from rasm.runtime.content_identity import ContentIdentity, ContentKey
from rasm.runtime.faults import RuntimeRail, async_boundary
from rasm.runtime.receipts import OPEN, Receipt, receipted

from artifacts.core.receipt import ArtifactReceipt
from artifacts.document.model import (
    FigureNode,
    ForeignRole,
    FormulaNode,
    LangTag,
    StandardRole,
    StructCategory,
    StructEltKind,
    StructureNode,
    alt_of,
    children,
    role_category,
    role_of,
    standard_for,
)

lazy import pikepdf
lazy import pdf_oxide
lazy from pikepdf import Array, Dictionary, Name, NumberTree, String

if TYPE_CHECKING:
    import pikepdf

# --- [TYPES] ----------------------------------------------------------------------------

type Arm = Callable[["Access"], "AccessFact"]
type PdfaLevel = Literal["1a", "1b", "2a", "2b", "2u", "3a", "3b", "3u"]  # the ISO 19005 conformance levels `pdf_oxide.convert_to_pdf_a` admits
type PdfxLevel = Literal[
    "1a_2001", "3_2002", "4"
]  # the ISO 15930 PDF/X print-production levels `pdf_oxide.validate_pdf_x` admits (the exact accepted token set)


class AccessOp(StrEnum):
    TAG = "tag"
    AUDIT = "audit"
    ARCHIVE = "archive"
    PREFLIGHT = "preflight"  # the ISO 15930 PDF/X print-production close, the print-plane sibling of the ARCHIVE PDF/A close


class UaCheck(StrEnum):  # the ISO 14289-1 structural-conformance clauses the AUDIT closes over
    MARKED = "marked"  # /MarkInfo /Marked true
    STRUCT_TREE = "struct-tree"  # /StructTreeRoot present
    LANG = "lang"  # catalog /Lang present
    TITLE = "title"  # XMP dc:title + /ViewerPreferences /DisplayDocTitle true
    UA_ID = "ua-id"  # XMP pdfuaid:part identifier
    NOT_SUSPECT = "not-suspect"  # /MarkInfo /Suspects absent or false
    FIGURE_ALT = "figure-alt"  # every /Figure carries /Alt or /ActualText
    HEADING_NESTING = "heading-nesting"  # no skipped heading level in document order
    ROLE_MAP = "role-map"  # every non-standard /S mapped in /RoleMap
    STRUCTURE_NESTING = "structure-nesting"  # every constrained /S nests under a standard-legal parent role
    TABLE_REGULAR = "table-regular"  # every /Table carries /TR rows
    LIST_STRUCTURE = "list-structure"  # every /L carries /LI items
    LINK_CONTENT = "link-content"  # every /Link carries content (kids)
    PAGES_KEYED = "pages-keyed"  # every page keyed into the /ParentTree via /StructParents
    SYNTAX = "syntax"  # pikepdf `check_pdf_syntax` (qpdf --check) reports no structural warning
    TEXT_LAYER = "text-layer"  # every page carries an extractable text layer (`pdf_oxide.has_text_layer`) — content is real text, not image-only
    NO_XFA = "no-xfa"  # no dynamic XFA form (`pdf_oxide.has_xfa` false) — ISO 14289-1 §7.18.1 prohibits it
    ORACLE = "oracle"  # the independent `pdf_oxide.validate_pdf_ua` in-process oracle agrees (valid, zero errors)


class PreflightCheck(StrEnum):  # the ISO 15930 PDF/X print-production clauses the PREFLIGHT closes over
    PDFX_VALID = "pdfx-valid"  # the independent `pdf_oxide.validate_pdf_x(level)` oracle agrees (valid, zero errors)
    CLAIM_HONEST = "claim-honest"  # a document that DECLARES a /pdfxid (`pdfx_claim`) actually validates — the decorative-claim close


# --- [ERRORS] ---------------------------------------------------------------------------


@tagged_union(frozen=True)
class AccessFault:
    # the closed ADMISSION vocabulary `of` produces; the arm-level `pikepdf.PdfError`/`pdf_oxide` raise converts
    # to the runtime `BoundaryFault` at the `async_boundary` capsule, never this interior vocabulary.
    tag: Literal["incomplete"] = tag()
    incomplete: AccessOp = case()  # a TAG op admitted without its required `AccessParams.source` tree


# --- [MODELS] ---------------------------------------------------------------------------


class StructureAudit(Struct, frozen=True, gc=False):
    elements: int
    depth: int
    pages: int
    pages_keyed: int
    figures: int
    figures_with_alt: int
    headings: int
    tables: int
    lists: int
    links: int
    role_map: int
    misnested: int
    syntax_warnings: int  # pikepdf `check_pdf_syntax` structural-warning count; the SYNTAX clause reads its emptiness
    oracle_valid: bool  # the `pdf_oxide.validate_pdf_ua` external verdict `valid` boolean
    oracle_errors: int  # the external oracle's reported error count; the ORACLE clause reads valid AND zero errors
    oracle_warnings: int  # the external oracle's non-fatal warning count; captured whole (never a 2-of-3 slice), evidence not a clause since a warning is not a conformance failure
    structured_warnings: int  # the `pdf_oxide.structured_warnings()` structure-diagnostic count (`{category, page, message, spec_section}` rows), additional two-source structural evidence beside the clause set
    has_tree: bool  # the oracle's independent `has_structure_tree` confirmation the STRUCT_TREE clause reconciles against the pikepdf `/StructTreeRoot` read
    pages_with_text: int  # count of pages carrying an extractable text layer (`pdf_oxide.has_text_layer`); the TEXT_LAYER clause reads full coverage
    has_xfa: bool  # dynamic XFA presence (`pdf_oxide.has_xfa`); the NO_XFA clause reads its absence
    pdfa_claim: str  # the document's OWN declared PDF/A conformance (pikepdf XMP `pdfa_status`), evidence not a clause
    pdfx_claim: str  # the document's OWN declared PDF/X conformance (pikepdf XMP `pdfx_status`)
    failures: tuple[UaCheck, ...]

    @property
    def coverage(self) -> float:
        return 1.0 if self.figures == 0 else self.figures_with_alt / self.figures

    @property
    def conformant(self) -> bool:  # the boolean exchange/conformance#CONFORMANCE folds via AuditSpec.structural_conformant
        return not self.failures

    def facts(self) -> dict[str, str]:  # the scalar projection a span/log consumer reads off the decoded content-addressed audit
        return {
            "elements": str(self.elements),
            "depth": str(self.depth),
            "pages": str(self.pages),
            "pages_keyed": str(self.pages_keyed),
            "figures": str(self.figures),
            "coverage": f"{self.coverage:.6f}",
            "role_map": str(self.role_map),
            "misnested": str(self.misnested),
            "syntax_warnings": str(self.syntax_warnings),
            "oracle_valid": str(self.oracle_valid),
            "oracle_errors": str(self.oracle_errors),
            "oracle_warnings": str(self.oracle_warnings),
            "structured_warnings": str(self.structured_warnings),
            "has_tree": str(self.has_tree),
            "pages_with_text": str(self.pages_with_text),
            "has_xfa": str(self.has_xfa),
            "pdfa_claim": self.pdfa_claim,
            "pdfx_claim": self.pdfx_claim,
            "failures": ",".join(self.failures),
            "conformant": str(self.conformant),
        }


class PreflightAudit(Struct, frozen=True, gc=False):
    # the ISO 15930 PDF/X print-production verdict PREFLIGHT mints — the print-plane sibling of `StructureAudit`,
    # reconciling the document's OWN declared /pdfxid (`pdfx_claim`) against the independent `validate_pdf_x` oracle
    # so a decorative claim becomes a two-source verdict; `conformant` gates the PADES/print-issue close.
    level: PdfxLevel
    pdfx_valid: bool  # the `pdf_oxide.validate_pdf_x(level)` external verdict `valid` boolean
    pdfx_errors: int  # the oracle's reported error count; the PDFX_VALID clause reads valid AND zero errors
    pdfx_warnings: int  # the oracle's non-fatal warning count; evidence, not a clause
    pdfx_claim: (
        str  # the document's OWN declared PDF/X conformance (pikepdf XMP `pdfx_status`); a non-empty claim the oracle refutes fails CLAIM_HONEST
    )
    structured_warnings: int  # the `pdf_oxide.structured_warnings()` diagnostic count folded as print-side evidence
    failures: tuple[PreflightCheck, ...]

    @property
    def conformant(self) -> bool:  # empty only when the oracle passes AND a declared claim is honest
        return not self.failures

    def facts(self) -> dict[str, str]:  # the scalar projection a span/log consumer reads off the decoded content-addressed verdict
        return {
            "level": self.level,
            "pdfx_valid": str(self.pdfx_valid),
            "pdfx_errors": str(self.pdfx_errors),
            "pdfx_warnings": str(self.pdfx_warnings),
            "pdfx_claim": self.pdfx_claim,
            "structured_warnings": str(self.structured_warnings),
            "failures": ",".join(self.failures),
            "conformant": str(self.conformant),
        }


class AccessParams(Struct, frozen=True, kw_only=True):
    # the one trusted authoring policy each arm reads its own slice of: `source` the model StructureNode tree
    # TAG lowers, the catalog PDF/UA requirements (`lang`/`title`/`ua_part`) authored alongside it, and
    # `pdfa_level` the ISO 19005 target ARCHIVE upgrades to (the shared-bundle shape the egress `Finishing` owner mirrors).
    source: StructureNode | None = None
    lang: LangTag | None = None
    title: str = ""
    ua_part: Literal[1, 2] = 1  # PDF/UA-1 (ISO 14289-1) or PDF/UA-2 (ISO 14289-2); the only defined parts
    pdfa_level: PdfaLevel = "2b"  # the ARCHIVE target; an out-of-range level is unrepresentable rather than a runtime-rejected string
    pdfx_level: PdfxLevel = "4"  # the PREFLIGHT PDF/X target (PDF/X-4 the modern print standard); an out-of-range level is unrepresentable


class AccessFact(Struct, frozen=True):
    # the bytes-plus-evidence carrier each arm threads onto the owner; `audit` the AUDIT verdict the content
    # key addresses and exchange/conformance#CONFORMANCE decodes for `conformant`, `elements`/`figures` the TAG
    # structure counts and `applied`/`residual` the ARCHIVE convert action/error counts, both riding the shared Egress slots.
    data: bytes
    pages: int = 0
    elements: int = 0
    figures: int = 0
    applied: int = 0
    residual: int = 0
    audit: StructureAudit | None = None
    preflight: PreflightAudit | None = None  # the PREFLIGHT PDF/X print-production verdict the content key addresses


class Access(Struct, frozen=True):
    op: AccessOp
    pdf: bytes
    params: AccessParams = field(default_factory=AccessParams)
    fact: AccessFact | None = None

    @classmethod
    def of(cls, op: AccessOp, pdf: bytes, /, *, params: AccessParams = AccessParams()) -> Result[Self, AccessFault]:
        ready = op is not AccessOp.TAG or params.source is not None  # only TAG needs a source tree; AUDIT/ARCHIVE read the emitted bytes
        return Ok(cls(op=op, pdf=pdf, params=params)) if ready else Error(AccessFault(incomplete=op))

    def _stepped(self) -> Self:
        return structs.replace(self, fact=_ARM[self.op](self))

    @receipted(
        OPEN
    )  # the keep-all redaction policy the runtime receipts owner exports (never a re-minted per-file `Redaction`); drains `contribute` off the stepped owner, emits via Signals.emit_async
    async def _emit(self) -> Self:
        # the sync pikepdf + pdf_oxide (Rust) object-model folds are GIL-releasing native, so they cross the thread seam
        # under the shared limiter rather than running inline on the loop; the stepped owner is the `ReceiptContributor`.
        return await to_thread.run_sync(self._stepped, limiter=_OFFLOAD)

    async def author(self) -> RuntimeRail[ContentKey]:
        return (await async_boundary(f"access.{self.op}", self._emit)).map(lambda done: ContentIdentity.of(f"access-{done.op}", done.fact.data))

    def contribute(self) -> Iterable[Receipt]:
        if (fact := self.fact) is None:  # contribute rides the stepped owner the weave returned, never the seed
            return
        key = ContentIdentity.of(f"access-{self.op}", fact.data)
        match (
            self.op
        ):  # the two producing ops share the Egress finishing case, the two validating ops the byte-only Pdf case — never a fifteenth receipt
            case AccessOp.TAG:
                emitted = ArtifactReceipt.Egress(key, len(fact.data), fact.pages, 0, fact.elements, fact.figures)
            case AccessOp.ARCHIVE:
                emitted = ArtifactReceipt.Egress(key, len(fact.data), fact.pages, 0, fact.applied, fact.residual)
            case AccessOp.AUDIT | AccessOp.PREFLIGHT:  # both validators content-address their verdict and land the byte-only Pdf case
                emitted = ArtifactReceipt.Pdf(key, len(fact.data), fact.pages)
            case _ as unreachable:
                assert_never(unreachable)
        yield from emitted.contribute()


# --- [CONSTANTS] ------------------------------------------------------------------------

# The /S-string algebra DERIVED once from the model StructEltKind vocabulary: `_ELT` decodes a read
# `/S` Name to its model member AND its key set IS the standard-structure membership the /RoleMap
# completeness check reads; `_CATEGORY` projects each member to the model's (category, heading_level)
# row through the public `role_category` projection — so the heading bucket, the heading level, and
# the standard set track the model enum with zero re-declared `_HEADINGS`/`_STANDARD_TYPES` literal.
_ELT: Final[frozendict[str, StructEltKind]] = frozendict({f"/{elt.value}": elt for elt in StructEltKind})
_CATEGORY: Final[frozendict[StructEltKind, tuple[StructCategory, int]]] = frozendict({
    elt: role_category(StandardRole(elt=elt)) for elt in StructEltKind
})
# the standard structure-nesting policy this AUDIT owns: each constrained role's legal parent set per ISO 14289
# (list/table grouping + the East-Asian ruby/warichu assemblies the model vocabulary carries); a role absent from
# the table nests anywhere, a foreign role is unconstrained.
_NESTING: Final[frozendict[StructEltKind, frozenset[StructEltKind]]] = frozendict({
    StructEltKind.LI: frozenset({StructEltKind.L}),
    StructEltKind.LBL: frozenset({StructEltKind.LI}),
    StructEltKind.LBODY: frozenset({StructEltKind.LI}),
    StructEltKind.THEAD: frozenset({StructEltKind.TABLE}),
    StructEltKind.TBODY: frozenset({StructEltKind.TABLE}),
    StructEltKind.TFOOT: frozenset({StructEltKind.TABLE}),
    StructEltKind.TR: frozenset({StructEltKind.TABLE, StructEltKind.THEAD, StructEltKind.TBODY, StructEltKind.TFOOT}),
    StructEltKind.TH: frozenset({StructEltKind.TR}),
    StructEltKind.TD: frozenset({StructEltKind.TR}),
    StructEltKind.RB: frozenset({StructEltKind.RUBY}),  # ruby base text nests under its `Ruby` assembly
    StructEltKind.RT: frozenset({StructEltKind.RUBY}),  # ruby annotation text
    StructEltKind.RP: frozenset({StructEltKind.RUBY}),  # ruby fallback punctuation
    StructEltKind.WT: frozenset({StructEltKind.WARICHU}),  # warichu text nests under its `Warichu` assembly
    StructEltKind.WP: frozenset({StructEltKind.WARICHU}),  # warichu punctuation
})
_AUDIT_ENCODER: Final = msgspec.msgpack.Encoder(order="deterministic")  # the content key addresses this stable encoding
_OFFLOAD: Final = CapacityLimiter(8)  # the pikepdf + pdf_oxide native folds are GIL-releasing; cross the thread seam, never the loop
_DECIMAL_PRECISION: Final = 8  # pinned qpdf real-number precision so a re-emit serializes coordinates identically — the content-addressing precondition beside `deterministic_id`


# --- [OPERATIONS] -----------------------------------------------------------------------


@dataclass(slots=True)
class _Author:  # the TAG boundary accumulator: foreign role map, per-page MCID slots, running counts
    role_map: dict[str, str] = dc_field(default_factory=dict)
    slots: dict[int, list["pikepdf.Object"]] = dc_field(default_factory=dict)
    elements: int = 0
    figures: int = 0


def _elem(pdf: "pikepdf.Pdf", node: StructureNode, parent: "pikepdf.Object", build: _Author, /) -> "pikepdf.Object":
    build.elements += 1
    role = role_of(node)
    elem = pdf.make_indirect(Dictionary(Type=Name.StructElem, S=Name("/" + role), P=parent, Pg=pdf.pages[node.meta.page].obj))
    if isinstance(node.role, ForeignRole):
        build.role_map[role] = standard_for(node.role).value  # /RoleMap maps the foreign role to its standard type
    if (illustrated := next((kid for kid in children(node) if isinstance(kid, FigureNode | FormulaNode)), None)) is not None:
        if isinstance(illustrated, FigureNode):
            build.figures += 1  # a formula is alt-bearing but not a figure — the receipt count stays figure-only
        if alt := alt_of(illustrated)[0]:
            elem.Alt = String(alt)  # ISO 14289: a /Figure AND a /Formula both carry the /Alt text equivalent `alt_of` projects
    if not isinstance(node.meta.actual_text, msgspec.UnsetType) and node.meta.actual_text:
        elem.ActualText = String(node.meta.actual_text)
    if not isinstance(node.meta.lang, msgspec.UnsetType) and node.meta.lang:
        elem.Lang = String(node.meta.lang)
    if branches := tuple(kid for kid in children(node) if isinstance(kid, StructureNode)):
        elem.K = Array([_elem(pdf, kid, elem, build) for kid in branches])
    else:  # a content leaf binds one MCID into its page's parent-tree slot array (emit marks the in-stream span)
        owners = build.slots.setdefault(node.meta.page, [])
        elem.K = len(owners)
        owners.append(elem)
    return elem


def _tag(access: "Access") -> AccessFact:
    params, source = access.params, access.params.source
    with pikepdf.open(BytesIO(access.pdf)) as pdf:  # deterministic close, never GC-reaped
        pdf.Root.MarkInfo = Dictionary(Marked=True)
        pdf.Root.ViewerPreferences = Dictionary(DisplayDocTitle=True)
        meta_lang = None if source is None or isinstance(source.meta.lang, msgspec.UnsetType) else source.meta.lang
        if (lang := params.lang or meta_lang) is not None:
            pdf.Root.Lang = String(lang)
        with pdf.open_metadata() as xmp:  # XMP PDF/UA identifier + document title
            xmp["pdfuaid:part"] = str(params.ua_part)
            if params.title:
                xmp["dc:title"] = params.title
        struct_root = pdf.make_indirect(Dictionary(Type=Name.StructTreeRoot, K=Array([]), ParentTreeNextKey=0, RoleMap=Dictionary()))
        pdf.Root.StructTreeRoot = struct_root
        build = _Author()
        struct_root.K = Array([_elem(pdf, source, struct_root, build)]) if source is not None else Array([])
        role_map = Dictionary()
        for foreign, standard in build.role_map.items():
            role_map[Name("/" + foreign)] = Name("/" + standard)
        struct_root.RoleMap = role_map
        parent_tree = NumberTree.new(
            pdf
        )  # the /ParentTree IS a PDF number-tree; the modeled `NumberTree` mapping-view owner replaces the hand-assembled flat `Nums` Array
        for page_key in sorted(build.slots):
            pdf.pages[page_key].obj.StructParents = page_key
            parent_tree[page_key] = Array(build.slots[page_key])
        struct_root.ParentTree = parent_tree.obj
        struct_root.ParentTreeNextKey = max(build.slots) + 1 if build.slots else 0
        pikepdf.settings.set_decimal_precision(_DECIMAL_PRECISION)  # pin real-number precision before the content-addressed emit
        sink = BytesIO()
        pdf.save(
            sink, deterministic_id=True
        )  # deterministic /ID: two identical-`StructureNode`-tree TAG runs content-key IDENTICALLY, the reuse-fabric elision the receipt spine depends on
        return AccessFact(sink.getvalue(), pages=len(pdf.pages), elements=build.elements, figures=build.figures)


@dataclass(slots=True)
class _Tally:  # the AUDIT boundary accumulator over the authored /StructTreeRoot spine
    elements: int = 0
    depth: int = 0
    figures: int = 0
    figures_with_alt: int = 0
    headings: int = 0
    tables: int = 0
    table_rows: int = 0
    lists: int = 0
    list_items: int = 0
    links: int = 0
    links_with_content: int = 0
    misnested: int = 0
    roles: set[str] = dc_field(default_factory=set)
    levels: list[int] = dc_field(default_factory=list)


def _walk(root: "pikepdf.Object", tally: _Tally, /) -> None:
    # Depth-safe pre-order frontier over the authored `/StructTreeRoot` `/K` spine: the AUDIT reads an
    # UNTRUSTED external PDF whose adversarial nesting depth forfeits native recursion (the `document/model#NODE`
    # `walk`/`node_digest` law), so one immutable work-stack threads `(elem, depth, parent-elt)` and pushes
    # children before siblings to keep the document order the `tally.levels` monotonicity read depends on.
    stack: Block[tuple["pikepdf.Object", int, StructEltKind | None]] = Block.singleton((root, 1, None))
    while not stack.is_empty():  # Exemption: iterative frontier — the untrusted `/K` spine forfeits the recursive form
        (elem, depth, parent), stack = stack.head(), stack.tail()
        tally.elements += 1
        tally.depth = max(tally.depth, depth)
        role = str(elem.get(Name.S, ""))
        tally.roles.add(role)
        elt = _ELT.get(role)  # the model member behind the read /S, or None for a foreign role (nesting-exempt)
        if elt is not None and parent is not None and elt in _NESTING and parent not in _NESTING[elt]:
            tally.misnested += 1
        match elt:
            case StructEltKind.FIGURE:
                tally.figures += 1
                tally.figures_with_alt += bool(elem.get(Name.Alt) or elem.get(Name.ActualText))
            case StructEltKind.TABLE:
                tally.tables += 1
            case StructEltKind.TR:
                tally.table_rows += 1
            case StructEltKind.L:
                tally.lists += 1
            case StructEltKind.LI:
                tally.list_items += 1
            case StructEltKind.LINK:
                tally.links += 1
                tally.links_with_content += bool(elem.get(Name.K))
            case StructEltKind() as heading if _CATEGORY[heading][0] is StructCategory.HEADING:
                tally.headings += 1
                tally.levels.append(_CATEGORY[heading][1])
            case _:
                pass
        kids = elem.get(Name.K)
        members = kids if isinstance(kids, pikepdf.Array) else (kids,) if isinstance(kids, pikepdf.Dictionary) else ()
        branches = Block.of_seq(
            (kid, depth + 1, elt) for kid in members if isinstance(kid, pikepdf.Dictionary) and kid.get(Name.Type) == Name.StructElem
        )
        stack = branches.append(stack)  # children before siblings keeps pre-order document order


def _audit(access: "Access") -> AccessFact:
    with pikepdf.open(BytesIO(access.pdf)) as pdf:  # deterministic close; every clause resolves to a plain value before the handle frees
        root = pdf.Root
        mark_info = root.get(Name.MarkInfo, Dictionary())
        struct_root = root.get(Name.StructTreeRoot)
        role_map = struct_root.get(Name.RoleMap, Dictionary()) if struct_root is not None else Dictionary()
        tally = _Tally()
        for kid in struct_root.get(Name.K, Array([])) if struct_root is not None else ():
            if isinstance(kid, pikepdf.Dictionary) and kid.get(Name.Type) == Name.StructElem:
                _walk(kid, tally)
        pages = len(pdf.pages)
        keyed = sum(Name.StructParents in page.obj for page in pdf.pages)
        syntax = len(pdf.check_pdf_syntax())  # qpdf --check structural-syntax warnings; a well-formed PDF is the ISO 14289 precondition
        mapped = {str(name) for name in role_map.keys()}  # the foreign /S Names the /RoleMap registers
        with pdf.open_metadata(set_pikepdf_as_editor=False, update_docinfo=False) as xmp:  # read-only audit never mutates the audited bytes
            ua_id, has_title = "pdfuaid:part" in xmp, "dc:title" in xmp
            pdfa_claim, pdfx_claim = xmp.pdfa_status, xmp.pdfx_status  # the document's OWN declared PDF/A·PDF/X claim (evidence, not a UaCheck)
        # every pikepdf-touching predicate resolved to a plain bool/int here, so no qpdf `Object` escapes the borrow window
        marked = bool(mark_info.get(Name.Marked, False))
        has_struct = struct_root is not None
        has_lang = Name.Lang in root
        title_ok = has_title and bool(root.get(Name.ViewerPreferences, Dictionary()).get(Name.DisplayDocTitle, False))
        not_suspect = not bool(mark_info.get(Name.Suspects, False))
        unmapped = any(role not in _ELT and role not in mapped for role in tally.roles)  # standard membership IS `_ELT`'s key set
        monotone = all(b - a <= 1 for a, b in pairwise(tally.levels) if b > a)
        role_map_n = len(mapped)
    with pdf_oxide.PdfDocument.from_bytes(access.pdf) as oracle:  # independent in-process Rust oracle, deterministic close via __exit__
        verdict = oracle.validate_pdf_ua()  # {'valid': bool, 'errors': list, 'warnings': list} — the veraPDF-grade cross-check, read once
        oracle_valid, oracle_errors, oracle_warnings = bool(verdict["valid"]), len(verdict["errors"]), len(verdict["warnings"])
        has_tree = oracle.has_structure_tree()  # the independent structure-tree confirmation STRUCT_TREE reconciles against the pikepdf read
        xfa = oracle.has_xfa()  # a dynamic XFA form is a PDF/UA-1 §7.18.1 violation
        text_pages = sum(oracle.has_text_layer(page_index) for page_index in range(int(oracle.page_count)))  # pages with real extractable text
        structured = len(
            oracle.structured_warnings()
        )  # the oracle's `{category, page, message, spec_section}` structure diagnostics, folded as additional two-source evidence
    failures = tuple(
        check
        for check, ok in (
            (UaCheck.MARKED, marked),
            (UaCheck.STRUCT_TREE, has_struct and has_tree),  # two-source: pikepdf sees /StructTreeRoot AND the oracle confirms a structure tree
            (UaCheck.LANG, has_lang),
            (UaCheck.TITLE, title_ok),
            (UaCheck.UA_ID, ua_id),
            (UaCheck.NOT_SUSPECT, not_suspect),
            (UaCheck.FIGURE_ALT, tally.figures == tally.figures_with_alt),
            (UaCheck.HEADING_NESTING, monotone),
            (UaCheck.ROLE_MAP, not unmapped),
            (UaCheck.STRUCTURE_NESTING, tally.misnested == 0),
            (UaCheck.TABLE_REGULAR, tally.tables == 0 or tally.table_rows > 0),
            (UaCheck.LIST_STRUCTURE, tally.lists == 0 or tally.list_items > 0),
            (UaCheck.LINK_CONTENT, tally.links == tally.links_with_content),
            (UaCheck.PAGES_KEYED, keyed == pages),
            (UaCheck.SYNTAX, syntax == 0),
            (UaCheck.TEXT_LAYER, text_pages == pages),  # every page has real text — an image-only page fails PDF/UA even when tagged
            (UaCheck.NO_XFA, not xfa),  # a dynamic XFA form is prohibited
            (UaCheck.ORACLE, oracle_valid and oracle_errors == 0),  # the independent oracle catches what the clause set cannot enumerate
        )
        if not ok
    )
    audit = StructureAudit(
        elements=tally.elements,
        depth=tally.depth,
        pages=pages,
        pages_keyed=keyed,
        figures=tally.figures,
        figures_with_alt=tally.figures_with_alt,
        headings=tally.headings,
        tables=tally.tables,
        lists=tally.lists,
        links=tally.links,
        role_map=role_map_n,
        misnested=tally.misnested,
        syntax_warnings=syntax,
        oracle_valid=oracle_valid,
        oracle_errors=oracle_errors,
        oracle_warnings=oracle_warnings,
        structured_warnings=structured,
        has_tree=has_tree,
        pages_with_text=text_pages,
        has_xfa=xfa,
        pdfa_claim=pdfa_claim,
        pdfx_claim=pdfx_claim,
        failures=failures,
    )
    return AccessFact(_AUDIT_ENCODER.encode(audit), pages=pages, audit=audit)


def _archive(access: "Access") -> AccessFact:
    level = access.params.pdfa_level
    with pdf_oxide.PdfDocument.from_bytes(access.pdf) as doc:  # deterministic close via __exit__, never GC-reaped
        outcome = doc.convert_to_pdf_a(level)  # {'success': bool, 'actions': list[str], 'errors': list[str]}, upgraded in place
        verified = doc.validate_pdf_a(
            level
        )  # {'valid': bool, 'level': str, 'errors': list, 'warnings': list} — the independent post-convert oracle on the SAME upgraded handle
        residual = len(outcome["errors"]) + len(
            verified["errors"]
        )  # two-source archival close: the converter self-report PLUS the oracle's post-upgrade conformance verdict, never a lone self-trust
        return AccessFact(doc.to_bytes(), pages=int(doc.page_count), applied=len(outcome["actions"]), residual=residual)


def _preflight(access: "Access") -> AccessFact:
    # the ISO 15930 PDF/X print-production close, the print-plane sibling of ARCHIVE: reconcile the document's OWN
    # declared /pdfxid (`pdfx_claim`, pikepdf XMP) against the independent `pdf_oxide.validate_pdf_x` oracle so the
    # `pdfx_claim` AUDIT records as decorative evidence becomes a TWO-SOURCE verdict — a claim the oracle refutes fails.
    level = access.params.pdfx_level
    with (
        pikepdf.open(BytesIO(access.pdf)) as pdf,
        pdf.open_metadata(set_pikepdf_as_editor=False, update_docinfo=False) as xmp,
    ):  # read the OWN /pdfxid, read-only, never mutating the audited bytes
        pdfx_claim = str(xmp.pdfx_status)  # the document's OWN declared PDF/X conformance, resolved to a plain str before the qpdf handle frees
    with pdf_oxide.PdfDocument.from_bytes(access.pdf) as oracle:  # independent in-process Rust PDF/X oracle, deterministic close via __exit__
        verdict = oracle.validate_pdf_x(
            level
        )  # {'valid': bool, 'level': str, 'errors': list, 'warnings': list} — read once, every value resolved before the handle frees
        valid, errors, warnings = bool(verdict["valid"]), len(verdict["errors"]), len(verdict["warnings"])
        structured, pages = len(oracle.structured_warnings()), int(oracle.page_count)
    failures = tuple(
        check
        for check, ok in (
            (PreflightCheck.PDFX_VALID, valid and errors == 0),  # the independent oracle passes
            (
                PreflightCheck.CLAIM_HONEST,
                not pdfx_claim or valid,
            ),  # a declared /pdfxid the oracle refutes is a false claim — the decorative-evidence trap closed
        )
        if not ok
    )
    audit = PreflightAudit(
        level=level,
        pdfx_valid=valid,
        pdfx_errors=errors,
        pdfx_warnings=warnings,
        pdfx_claim=pdfx_claim,
        structured_warnings=structured,
        failures=failures,
    )
    return AccessFact(_AUDIT_ENCODER.encode(audit), pages=pages, preflight=audit)


# --- [COMPOSITION] ----------------------------------------------------------------------
_ARM: Final[frozendict[AccessOp, Arm]] = frozendict({
    AccessOp.TAG: _tag,
    AccessOp.AUDIT: _audit,
    AccessOp.ARCHIVE: _archive,
    AccessOp.PREFLIGHT: _preflight,
})
```

## [03]-[RESEARCH]

- [TAGGED_AUTHORING] [RESOLVED]: the `pikepdf` tagged-PDF structure tree is authored over the raw qpdf `Object` model — no high-level `StructTreeRoot`/`StructElem` helper class exists on the installed distribution (`10.9.1`, libqpdf `12.3.2`), so the catalogue `[02]-[PUBLIC_TYPES]` object-model rows `Object`/`Dictionary`/`Array`/`Name`/`String` and the `[03]-[ENTRYPOINTS]` `Pdf.Root`/`Pdf.make_stream`/`Pdf.open_metadata` surface are the authoring primitives. `Pdf.make_indirect(obj) -> Object` mints the indirect dictionaries the structure tree requires (`/StructTreeRoot`, each `/StructElem`) — it is a live `pikepdf` member used here and across the corpus structure authoring, NOT enumerated in the `[03]-[ENTRYPOINTS]` rows that list only `make_stream`, so the catalogue under-documents it (the cross-file residual). The `/ParentTree` is NOT hand-built: it IS a PDF number-tree, so `pikepdf.NumberTree.new(pdf)` mints the modeled mapping-view owner (a `MutableMapping[int, Object]` with `.obj` the backing dict), `parent_tree[page_key] = Array(slots)` populates each page's MCID-indexed `StructElem` array, and `struct_root.ParentTree = parent_tree.obj` binds it — the hand-assembled flat `struct_root.ParentTree.Nums = Array(nums)` the prior fence wrote is the deleted re-implementation of a primitive pikepdf models (verified: `NumberTree.new(pdf, *, auto_repair=True)` on `pikepdf 10.9.1`). `pdf.Root.MarkInfo = Dictionary(Marked=True)` sets the catalog tagged flag, `pdf.Root.ViewerPreferences = Dictionary(DisplayDocTitle=True)` and `pdf.Root.Lang = String(tag)` author the two ISO 14289 catalog requirements, `pdf.Root.StructTreeRoot = struct_root` binds the tree, and each page's `pdf.pages[i].obj.StructParents = key` keys it into the number-tree. The re-emit is `Pdf.save(sink, deterministic_id=True)` under a pinned `settings.set_decimal_precision`: the prior `pdf.save(sink)` minted a randomized `/ID`, so two identical-`StructureNode`-tree TAG runs content-keyed DIFFERENTLY and defeated the core/plan reuse-fabric elision the receipt spine assumes — `deterministic_id=True` is the content-addressing precondition (verified byte-identical across runs; the audit's `msgpack` encoder already carried `order="deterministic"`, but the TAG byte emit did not). The `/StructElem` dictionary carries `/Type /StructElem`, `/S` the structure-type `Name`, `/P` the parent ref, `/Pg` the page ref, `/K` (an `Array` of child refs and integer MCIDs, or a bare MCID for a single-leaf element), `/Alt`/`/ActualText`/`/Lang` the accessibility evidence, and the `/RoleMap` `Dictionary` maps each foreign role `Name` to a standard structure-type `Name`. `pdf_oxide.DocumentBuilder.tagged_pdf_ua1().role_map(custom, standard)` is the alternative fluent tagged-PDF/UA author — reserved for a from-scratch born-tagged document at `document/emit#DOCUMENT`, never a second structure author over an existing PDF here where the raw object-model spike is the surface.
- [CATALOG_REQUIREMENTS] [RESOLVED]: ISO 14289-1 (PDF/UA-1) conformance is more than the structure tree — the catalog `/MarkInfo /Marked true`, the document `/Lang` natural-language tag, the document title surfaced through `/ViewerPreferences /DisplayDocTitle true` plus an XMP `dc:title`, and the XMP `pdfuaid:part` identifier are mandatory. `Pdf.open_metadata()` (catalogue `[03]-[ENTRYPOINTS]` `[08]`, returning the `models.PdfMetadata` mapping) is the XMP context manager the `TAG` arm sets `pdfuaid:part`/`dc:title` through and the `AUDIT` arm reads back, and the `AUDIT` arm ALSO reads the `PdfMetadata.pdfa_status`/`pdfx_status` conformance-claim probes (the document's OWN declared `pdfaid`/`pdfxid`, recorded as `StructureAudit.pdfa_claim`/`pdfx_claim` evidence rather than a `UaCheck` failure since PDF/UA does not require PDF/A) beside the `pdfuaid:part` read. `AccessParams.lang` falls back to the root `StructureNode`'s `NodeMeta.lang`, and `NodeMeta.actual_text` lowers to `/ActualText` on every element. `AccessParams.ua_part` is a `Literal[1, 2]`: ISO 14289-1 (PDF/UA-1) and ISO 14289-2 (PDF/UA-2) are the only defined parts, so an out-of-range identifier is unrepresentable at the type; `AccessParams.pdfa_level` is a `Literal` over the eight `1a`/`1b`/`2a`/`2b`/`2u`/`3a`/`3b`/`3u` ISO 19005 levels `pdf_oxide.convert_to_pdf_a` admits, closing the illegal-value gap a bare `str` left open.
- [MODEL_TREE_CONSUMED] [RESOLVED]: the source structure tree is the `document/model#NODE` `StructureNode` variant carrying the `StructRole` (`StandardRole(StructEltKind)` | `ForeignRole(str)`) the model owns; this owner CONSUMES `role_of`/`alt_of`/`children`/`standard_for` and the `StructEltKind` vocabulary rather than re-declaring the tree, the ARCHITECTURE `[02]-[SEAMS]` `document/model → document/tagged [NODE]` edge. `role_of(node)` projects a `StructureNode` to its `/S` string in one model call; `isinstance(node.role, ForeignRole)` is the standard-vs-foreign discriminant; `standard_for(role)` projects a foreign role to its canonical standard `StructEltKind` through the model's first-wins `_STANDARD_FOR` category inversion, so the `/RoleMap` lowering reads a model projection rather than a hand-kept parallel `dict`. The `AUDIT` arm consumes the model `role_category`/`StandardRole`/`StructCategory` surface the same way: `_CATEGORY = frozendict({elt: role_category(StandardRole(elt=elt)) for elt in StructEltKind})` is the one DERIVED `(StructCategory, heading_level)` projection this page keeps, and `_ELT = frozendict({f"/{elt.value}": elt for elt in StructEltKind})` decodes a read `/S` Name to its model member with its key set serving as the standard-structure membership the `/RoleMap` completeness audit reads — both tracking the model enum with zero re-declared `_HEADINGS`/`_STANDARD_TYPES` literal. The standard-nesting policy `_NESTING` (each constrained list/table role's legal parent set PLUS the East-Asian ruby/warichu assembly rows — `RB`/`RT`/`RP` under `Ruby`, `WT`/`WP` under `Warichu` — tracking the model's CJK vocabulary) is the one conformance table this AUDIT owns, keyed by the model `StructEltKind` it composes.
- [STRUCTURE_AUDIT] [RESOLVED]: the `AUDIT` arm is a TWO-SOURCE reconciliation, not a lone self-assessment. The `pikepdf` self-audit recurses the authored `/StructTreeRoot` `/K` spine over the `Object` model AND reads the catalog/XMP, folding one `_Tally` into fourteen structural `UaCheck` clauses — `MARKED`/`STRUCT_TREE`/`LANG`/`TITLE`/`UA_ID`/`NOT_SUSPECT`/`FIGURE_ALT`/`HEADING_NESTING`/`ROLE_MAP`/`STRUCTURE_NESTING`/`TABLE_REGULAR`/`LIST_STRUCTURE`/`LINK_CONTENT`/`PAGES_KEYED` — plus the `SYNTAX` clause off `Pdf.check_pdf_syntax()` (the qpdf `--check` structural-warning list, a well-formed PDF being the ISO 32000 substrate ISO 14289 builds on), the `TEXT_LAYER` clause off the per-page `pdf_oxide.has_text_layer` coverage sum (an image-only page with no real text fails PDF/UA even when tagged), and the `NO_XFA` clause off `pdf_oxide.has_xfa` (ISO 14289-1 §7.18.1 prohibits a dynamic XFA form); the `pdf_oxide.structured_warnings()` `{category, page, message, spec_section}` diagnostic count folds as additional two-source structural EVIDENCE (`StructureAudit.structured_warnings`, not a clause — a warning is not a conformance failure, exactly as `oracle_warnings` is captured-not-gated), the opt-in structured surface over stderr text the oracle exposes. The prior page's `[STRUCTURE_AUDIT]` finding asserted "no pure-Python PDF/UA structural validator resolves on PyPI" and disclaimed any external grade — FALSIFIED by the installed `pdf_oxide.PdfDocument.validate_pdf_ua()`, an in-process Rust (MIT OR Apache-2.0, commercial-safe, no JVM) PDF/UA conformance oracle whose docstring returns `{'valid': bool, 'errors': list, 'warnings': list}`. The `AUDIT` arm opens the independent `pdf_oxide.PdfDocument` under its `__enter__`/`__exit__` capsule, reads that `{valid, errors, warnings}` verdict once and the `has_structure_tree()` confirmation, folds the `ORACLE` clause (`valid` AND zero `errors`) AND reconciles the confirmation INTO the `STRUCT_TREE` clause (`has_struct and has_tree` — the structure tree counts as present only when the `pikepdf` `/StructTreeRoot` read AND the oracle's independent probe agree, closing the decorative-evidence gap where `has_tree` was formerly computed and stored but never gated a clause), the `warnings` count kept as whole-verdict evidence beside `errors` so no oracle key is a 2-of-3 slice, so `conformant` is empty ONLY when the `pikepdf` per-clause self-audit AND the independent oracle both agree — the self-audit owning the explainable per-clause verdict, the oracle owning the veraPDF-grade cross-check the fourteen clauses cannot enumerate. qpdf auto-coerces a read `/K` element to its Python kind, so the depth-safe `_walk` frontier — one immutable `Block` work-stack threading `(elem, depth, parent)` in pre-order document order, the `document/model#NODE` `walk`/`node_digest` law applied because the `AUDIT` reads an untrusted external tree whose adversarial nesting depth forfeits native recursion — discriminates `isinstance(kid, pikepdf.Dictionary)` `/Type /StructElem` (pushed onto the frontier) from the `int` MCID and `/MCR`/`/OBJR` leaves, decoding each `/S` Name to its model `StructEltKind` through `_ELT` and `match`-ing the member. Every `pikepdf`-touching clause predicate resolves to a plain `bool`/`int`/`str` inside the `with pikepdf.open(...)` window before the handle frees, so no qpdf `Object` escapes into the post-close `failures` fold; the `StructureAudit` round-trips through `msgspec.msgpack(order="deterministic")` so the reconciled verdict is the content the `AUDIT` content key addresses, decoded by the composition root to thread `conformant` into `exchange/conformance#CONFORMANCE`.
- [ARCHIVE_UPGRADE] [RESOLVED]: the `ARCHIVE` arm upgrades the emitted PDF to the ISO 19005 archival profile through `pdf_oxide.PdfDocument.convert_to_pdf_a(pdfa_level)` — the in-place Rust converter whose docstring returns `{'success': bool, 'actions': list[str], 'errors': list[str]}` over the eight defined levels (`1a`/`1b`/`2a`/`2b`/`2u`/`3a`/`3b`/`3u`) — then `pdf_oxide.PdfDocument.validate_pdf_a(pdfa_level)` (the in-process oracle returning `{'valid': bool, 'level': str, 'errors': list, 'warnings': list}`) independently re-validates the SAME upgraded handle so the converter never self-certifies its own output, and `to_bytes()` re-emits the archival document and `int(page_count)` (the int-like `_PageCount` wrapper) counts it — the `actions` cardinality riding the fact's `applied` count and the `residual` summing the converter's own `errors` and the oracle's post-upgrade validation `errors`, the two-source archival close mirroring the AUDIT's two-source structural close rather than a lone convert self-report. This is the archival sibling of the accessibility close over the same already-emitted bytes and is DISTINCT from `document/emit#DOCUMENT`'s born-PDF/A authoring of a fresh document from the `DocumentNode` tree (`Pdf.from_*`/`DocumentBuilder`) — `ARCHIVE` upgrades an existing PDF, emit authors one from scratch, the boundary the pdf_oxide catalogue draws by routing `convert_to_pdf_a` to `document/tagged#ACCESS`. The archival telos is load-bearing under the AEC ISO 19650 delivery plane the campaign fixes: an information-container is issued as PDF/A, so the archival upgrade is a first-class conformance close, not an afterthought.
- [PDFX_PREFLIGHT] [RESOLVED]: the `PREFLIGHT` arm is the ISO 15930 PDF/X print-production close, the print-plane sibling of `ARCHIVE` under the unified publication/print telos the campaign fixes. It reconciles the document's OWN declared /pdfxid — read read-only through `pikepdf.open_metadata(set_pikepdf_as_editor=False, update_docinfo=False).pdfx_status` (a plain `str` resolved before the qpdf handle frees) — against the independent `pdf_oxide.PdfDocument.validate_pdf_x(pdfx_level)` in-process Rust oracle, whose docstring returns `{'valid': bool, 'level': str, 'errors': list, 'warnings': list}` over the EXACT accepted level set `1a_2001`/`3_2002`/`4` (verified: any other token raises `ValueError: Unknown PDF/X level`, so `PdfxLevel = Literal["1a_2001", "3_2002", "4"]` is a closed vocabulary and an illegal level is unrepresentable at the type). The `PreflightAudit` folds two `(PreflightCheck, predicate)` rows: `PDFX_VALID` on `valid and errors == 0` (the oracle passes), and `CLAIM_HONEST` on `not pdfx_claim or valid` (a document that DECLARES a /pdfxid the oracle refutes is a FALSE claim). This closes the exact decorative-evidence trap the `[02]` boundary and the [STRUCTURE_AUDIT] `has_tree` fix both name: the prior fence read `pdfx_status` and stored it as `StructureAudit.pdfx_claim` yet NEVER independently validated it — an ornamental claim rode as evidence, the same illusory-density defect the `has_structure_tree` confirmation exhibited before it gated `STRUCT_TREE`. Now `pdfx_claim` is a two-source verdict: the document's declaration AND the independent print-production oracle, `conformant` empty only when both agree, exactly as `ARCHIVE` re-validates its `convert_to_pdf_a` output through `validate_pdf_a` rather than trusting the converter's own `success`. `structured_warnings` folds as print-side diagnostic evidence beside the verdict. `PREFLIGHT` contributes the byte-only `ArtifactReceipt.Pdf` case AUDIT already reuses (the `PreflightAudit` content-addressed by the PREFLIGHT key), needs no source tree (`Access.of`'s `op is not AccessOp.TAG` guard already admits it), and adds one `AccessOp` row + one `_preflight` arm + one `PreflightAudit`/`PreflightCheck` pair + one `AccessFact.preflight` field — the print-production plane is genuinely distinct from the PDF/UA accessibility plane, so its verdict is its own small owner rather than PDF/X fields smuggled onto `StructureAudit` where they would conflate two conformance planes under one `conformant`. The `validate_pdf_x`/`structured_warnings` spellings verify against the folder `.api` catalogue for `pdf_oxide` conformance rows `[03]`-`[05]` and diagnostic row `[02]`.
- [RECEIPT_AND_ASPECT] [RESOLVED]: the receipt is WIRED, not described — the `@receipted(OPEN)` weave over the thin pure `_emit` drains `Access.contribute` off the stepped owner and emits through the runtime `Signals.emit_async`, the artifacts convention `document/egress#FINISH` establishes (`OPEN` the keep-all redaction policy the runtime receipts owner exports, since access facts carry no classified field — never a re-minted per-file `Redaction(classified=Map.empty())`, the prior page's exact anti-pattern egress warns against). The `TAG` and `ARCHIVE` arms both contribute `ArtifactReceipt.Egress` — `TAG` mapping the structure-element count onto `outline_depth` and the figure count onto `overlays`, `ARCHIVE` mapping the applied-action count onto `outline_depth` and the residual-error count onto `overlays` (the two produce ops sharing the one content-composition finishing case exactly as egress folds an OCG-strip and a form-flatten onto those slots) — and the `AUDIT` arm contributes `ArtifactReceipt.Pdf(key, bytes, pages)`, the settled `core/receipt#RECEIPT` `Egress`/`Pdf` reuse target, never a fifteenth case; `contribute` folds the case off the `op` discriminant through one total `match` closed by `assert_never`. The owner crosses the synchronous pikepdf + pdf_oxide object-model folds onto the GIL-releasing `anyio.to_thread` seam under a bounded `CapacityLimiter` inside the async `_emit` — in-process but never inline on the loop, using the SYNC `pdf_oxide.PdfDocument` root (never the `AsyncPdfDocument` mirror, which would double-offload its own pool through the thread seam) — over `pikepdf.open(...)`/`pdf_oxide.PdfDocument.from_bytes(...)` handles closed deterministically through `with`/`__exit__` rather than left for GC; `async_boundary` converts a `pikepdf.PdfError` or `pdf_oxide` raise into the runtime `BoundaryFault`, and `Access.of` rejects a `TAG` without a `source` tree into `AccessFault.incomplete` before the fold.
- [SIGN_SEAM] [RESOLVED]: the `StructureAudit.conformant` boolean threads into the `exchange/conformance#CONFORMANCE` `ConformanceVerdict.structural_conformant` through the spec's `AuditSpec.structural_conformant` (carried at sign time on `SignSpec.structural_conformant`, projected into the `AuditSpec` every spec's `audit()` mints), the ARCHITECTURE `[02]-[SEAMS]` `document/tagged → exchange [SIGN]` and `exchange/conformance ← document/tagged [ACCESS]` edges — the live `exchange/conformance#CONFORMANCE` `_audited` epilogue reads `structural_conformant=spec.structural_conformant` and its boundary explicitly disclaims pyhanko's lack of PDF/UA enforcement, naming the `document/tagged#ACCESS` owner as the structural authority. The `StructureAudit` is the leaf value this owner mints (content-addressed by the `AUDIT` key) and the composition root decodes to supply the boolean, the one-way acyclic edge mirroring the `ConformanceVerdict` value; where the conformance owner reads the document's OWN `pdfa_claim`/`pdfx_claim` from XMP as declared evidence, THIS owner now closes the structural verdict with a genuine two-source grade (self-audit AND `pdf_oxide` oracle) rather than the lone self-assessment pyhanko honestly discloses it cannot supply.
- [MARKED_CONTENT_SEAM] [SEAM]: the `/StructElem` MCID references the integer index into its page's `/StructParents` `/ParentTree` array, and the in-stream `/Tag <</MCID n>> BDC … EMC` operator sequence binding the drawn region to that MCID is authored by `document/emit#DOCUMENT` (the producer that draws the content through `canvas.ContentStreamBuilder`/the typst/weasyprint auto-tagging backends emit.md names), since only the drawing producer knows each region's content-stream extent. This owner assigns MCIDs in document order per page into the `/ParentTree /Nums` and the emit drawing marks the regions in the same document order, so the two agree by construction. The cross-file obligation — emit stamping the marked-content operators (or carrying a `NodeMeta` marked-content id) so a canvas-drawn region resolves to the MCID this owner assigns — is the residual; `Pdf.make_stream`/`Page.contents_add`/`ContentStreamBuilder.begin_marked_content` are the confirmed surface emit composes for the canvas backends, and `begin_marked_content_proplist(mctype, mcid)` (the MCID property-list variant carrying the explicit `<</MCID n>>`) is now CATALOGUED in the `pikepdf` `.api` `[03]-[ENTRYPOINTS]` rows and verified present on `pikepdf 10.9.1`, so the post-hoc canvas path stamps the deterministic MCID through it — the gate is lifted, no longer document-order convention alone. The born-tagged backends emit the marked content and its MCIDs natively at author time (`pdf_oxide.DocumentBuilder` tagged PDF/UA-1, typst `pdf_standards`, weasyprint PDF/UA), so the seam resolves deterministically on both the born-tagged and post-hoc-canvas paths.

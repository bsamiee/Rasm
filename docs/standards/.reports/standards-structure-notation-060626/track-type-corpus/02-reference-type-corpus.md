Question: Which structure and notation issues in `docs/standards/reference/*.md` should promote to active owners?
Type: standards-research
Lane: track-type-corpus/reference
Merge key: reference type corpus :: structure and notation systems :: promote normalized fixes
Target owner: `docs/standards/reference/*.md`, `docs/standards/information-structure.md`, `docs/standards/formatting.md`, and `docs/standards/proof.md`
Source basis: full local reads of the reference type corpus and shared owners; official OpenAPI, TypeScript, Python, Bash, C#, API Extractor, mkdocstrings, and PostgreSQL sources where the type standards make current capability claims
Promotion target: reference type standards plus shared form, notation, and proof owners
Outcome: PROMOTE

## [FINDINGS]

### [F1][README_HEADING_EXAMPLE]

Path: `docs/standards/reference/readme.md:41-51`
System: examples, heading idiom, bracket tokens
Issue: The rejected and accepted repo-internal H1 examples both show `# [ASSAY_OPERATOR]`. The prose says the rejected shape is wrong and then tells the reader to use the same visible shape, so the example cannot teach the distinction between `# [H1][...]` theater and the accepted repo-internal bracketed H1.
Active owner: `docs/standards/reference/readme.md` for README examples; `docs/standards/formatting.md` for heading idiom.
Ripple: Every README audit that uses this example as the repair pattern can preserve the bad H1 because the counterexample is indistinguishable from the accepted example.
Decision: PROMOTE. Replace the rejected block with a genuinely rejected heading such as `# [H1][ASSAY_OPERATOR]`, or rewrite the pair as a compact contrast record instead of two fences.
Proof gap: none. Local file content proves the contradiction.

### [F2][RELATION_RECORD_VARIANTS]

Path: `docs/standards/reference/api.md:178-187`, `docs/standards/reference/reference.md:200-207`, `docs/standards/reference/support-matrix.md:356-367`, `docs/standards/reference/code-documentation.md:94-107`, `docs/standards/reference/readme.md:319-323`
System: relation records, records, proof fields, co-location
Issue: The corpus uses the shared adjacent-route field run (`Changed fact`, `Consumed by`, `Use in this document`, `Update when`, `Close when`, `Route-away`) but each type bends the preamble differently. API prepends `Surface` and `Profile`; support matrix prepends migration fields; code documentation lists relation fields and local proof fields but does not publish a generated-reference handoff template; README shows a rejected relation card without a nearby accepted relation shape. `information-structure.md:160-164` allows local identity/status fields before the relation run only when the type standard declares the exception before the example.
Active owner: `docs/standards/information-structure.md` owns relation-record field order; each reference type owns its local relation variant.
Ripple: Agents can copy a relation variant into another type without preserving the shared field run or without knowing which preamble fields are local context versus relation fields.
Decision: PROMOTE. Add one sentence to each affected type standard before the first variant: local context fields may precede the shared relation run only for the named profile, and once `Changed fact` appears the shared field order is preserved. Add an accepted generated-reference handoff template to code documentation if that handoff remains a conditional record.
Proof gap: none. Local field-order comparison proves the drift.

### [F3][PROOF_GAP_FIELD_OMISSION]

Path: `docs/standards/reference/api.md:93-107`, `docs/standards/reference/reference.md:121-129`, `docs/standards/reference/reference.md:169-180`, `docs/standards/reference/support-matrix.md:117-126`, `docs/standards/reference/support-matrix.md:258-271`
System: proof fields, records, templates
Issue: Proof-bearing templates repeatedly encode `Evidence`, `Generated from`, `Source of truth`, `Last verified`, and `Review trigger`, but omit the explicit `Proof gap:` field that `proof.md:41-49` defines for unavailable or intentionally unrun source, render, command, or validation proof. Some placeholders allow "proof gap" inside `Evidence`, which weakens the proof-local field run defined in `proof.md:54-67`.
Active owner: `docs/standards/proof.md` owns proof labels and order; each reference type owns local proof-bearing templates.
Ripple: New reference pages can bury an unrun gate inside `Evidence` instead of making uncertainty scannable. This matters most for generated contracts, renderer claims, upstream API facts, and support matrices.
Decision: PROMOTE. Add `Proof gap: <missing source, unrun command, unsupported renderer proof, or omit when proved>` after `Source of truth:` in proof-bearing templates that claim generated, current, renderer, support, command-output, or upstream behavior. Keep it omitted in produced documents when proof exists.
Proof gap: none for the structural finding. The active proof owner already defines the field.

### [F4][CODE_DOC_VERSION_BASELINES]

Path: `docs/standards/reference/code-documentation.md:71`, `docs/standards/reference/code-documentation.md:202`, `docs/standards/reference/code-documentation.md:238`, `docs/standards/reference/code-documentation.md:271`, `docs/standards/reference/code-documentation.md:305`
System: source-truth capability claims, statuses, proof fields
Issue: The language capsules name version baselines as toolchain facts. Current primary sources support Bash 5.3 and PostgreSQL 18.4 as current released docs, and Python 3.15 docs exist as beta docs, but the official TypeScript source read in this pass labels TypeScript 7.0 as Beta on 2026-04-21. The standard can remain future-facing under repo policy, but the wording should not read as if every named version is a stable current release unless the maintained source proves that state.
Active owner: `docs/standards/reference/code-documentation.md` for language capsule baselines; `docs/standards/proof.md` for current-source proof.
Ripple: `coding-ts`, generated TypeScript API docs, and package README guidance can adopt a beta baseline as a stable "current" source-truth claim.
Decision: PROMOTE. Add a compact source-model or baseline note before language capsules: version labels are target baselines; current release state must be verified from maintained sources when a produced document claims toolchain availability. For TypeScript 7 specifically, either say `TypeScript 7 beta target` until the official release source changes, or add a `Proof gap:` when no current release proof is checked.
Proof gap: C# 14 was not separately researched in this pass. TypeScript 7 beta, Python 3.15 beta docs, Bash 5.3, and PostgreSQL 18.4 were checked against current primary sources.

### [F5][SUPPORT_IMPORTED_FIELD_SOURCE]

Path: `docs/standards/reference/support-matrix.md:128-131`
System: support statuses, matrices, source-truth fields
Issue: The lifecycle import rule names very specific boolean/date pairs such as `isEoas`/`eoasFrom`, `isEol`/`eolFrom`, and `isDiscontinued`/`discontinuedFrom` without naming the imported source model that owns those exact field names. That is stronger than a generic lifecycle-import rule and reads like a current API schema claim.
Active owner: `docs/standards/reference/support-matrix.md` for support lifecycle import shape; `docs/standards/proof.md` for maintained-source proof.
Ripple: A support matrix author can cargo-cult those field names into sources that use different lifecycle schemas, or treat an unavailable field as missing data instead of a source-model mismatch.
Decision: PROMOTE. Either name the source family before the field list and attach evidence, or generalize the rule to "preserve source field names before mapping" and move the field pairs into a source-specific example with `Evidence`, `Source of truth`, and `Review trigger`.
Proof gap: The pass confirmed endoflife.date has a maintained API surface, but did not obtain a primary schema page proving the exact `isEoas` field family.

### [F6][README_EXAMPLE_GALLERY]

Path: `docs/standards/reference/readme.md:327-403`
System: examples, isolated sentences, code fences, co-location
Issue: The README standard collects several examples in one late `EXAMPLES` section after the structure rules. `information-structure.md:575-582` says examples should sit beside the rule they clarify and should not become detached reusable patterns. The examples are useful, but the cluster forces readers to map hub-index, child-prose, root-first-path, and root-command rejection examples back to earlier rules manually.
Active owner: `docs/standards/reference/readme.md` for README examples; `docs/standards/information-structure.md` for example placement.
Ripple: Later type standards can copy the "late gallery" layout and separate examples from the rule that prevents misuse.
Decision: PROMOTE. Move each example beside its controlling rule or reframe `EXAMPLES` as a compact "reader-route examples" section whose lead names which earlier rules it demonstrates. Keep the root first-path example if it remains the only way to show nested fences and command proof together.
Proof gap: none. This is a structure consistency finding.

### [F7][MERMAID_RENDER_PROOF]

Path: `docs/standards/reference/support-matrix.md:193-219`
System: diagrams, renderer claims, proof fields
Issue: The support matrix includes a Mermaid lifecycle diagram with renderer `config:` and accessibility text. This is structurally valid under `information-structure.md:436-452`, and the diagram has a text equivalent. The proof gap is that no current render proof is attached to the report, and renderer-dependent claims require render proof or an explicit gap under `proof.md:154-164` when the diagram or renderer behavior is changed.
Active owner: `docs/standards/reference/support-matrix.md` for support lifecycle diagram use; `docs/standards/proof.md` for renderer proof.
Ripple: If promoted edits touch this diagram or its renderer config, the close gate must include `pnpm exec mmdc` or an explicit render proof gap.
Decision: HOLD. Do not edit only to add proof to an untouched conceptual diagram, but preserve this as a required gate if the diagram changes.
Proof gap: local Mermaid render command not run in this pass.

## [EVIDENCE]

### [LOCAL_CORPUS_CATALOG]

`docs/standards/reference/api.md`
    Lines: 228
    Code fences: `markdown template` at 48; `text template` at 93, 112, and 178.
    Tables or matrices: profile table at 32-40; API change maintenance table at 166-172.
    Bracket tokens and group labels: `AUTHORING_CONTRACT`, profile group labels, validation group labels.
    Status/progress/glyphs: no progress bars or glyph legends; status appears as API failure/status carrier vocabulary, not lifecycle state.
    Records: contract record, field card, API surface relation card.
    Proof fields: `Evidence`, `Generated from`, `Source of truth`, `Last verified`, `Review trigger`; no explicit `Proof gap` field in templates.
    Relation records: API surface card with local `Surface` and `Profile` preamble.
    Diagrams: none.
    Isolated sentences: valid leads and boundaries; no standalone sentence defect found.

`docs/standards/reference/code-documentation.md`
    Lines: 422
    Code fences: one `text template` source-comment decision record at 56-66.
    Tables or matrices: none.
    Bracket tokens and group labels: dense group labels across decision router, source truth, language capsules, and validation.
    Status/progress/glyphs: lifecycle state vocabulary at 339-346; no progress bars or glyph legends.
    Records: source-comment decision record; relation field list; lifecycle state class list.
    Proof fields: local conditional fields include `Evidence`, `Generated from`, `Source of truth`, `Last verified`, and `Review trigger`.
    Relation records: relation fields are defined, but the conditional generated-reference handoff has no accepted copyable template.
    Diagrams: none.
    Isolated sentences: no unsupported sentence islands found; language capsule leads function as local H3 leads.

`docs/standards/reference/readme.md`
    Lines: 457
    Code fences: `markdown rejected`, `markdown conceptual`, `markdown template`, `text template`, `text conceptual`, `text rejected`, and nested `bash copy-safe` fences inside a four-backtick conceptual block.
    Tables or matrices: heading-mode table at 35-39; profile table at 57-63; surface-split table at 224-230; local hub-index example table at 354-359.
    Bracket tokens and group labels: README profile groups and validation groups; ordinary README pages reject invocation markers at 232.
    Status/progress/glyphs: README status records and local status vocabulary rules at 237-250; no progress bars or glyph legends.
    Records: route card, package/tool entry card, rejected relation card.
    Proof fields: proof is routed to `proof.md`; command proof appears in examples and validation.
    Relation records: only a rejected multi-route card appears in the examples.
    Diagrams: conditional Mermaid/image diagrams are allowed by rule, but no real diagram appears.
    Isolated sentences: examples use several valid captions, but the H1 contrast at 41-51 is contradictory.

`docs/standards/reference/reference.md`
    Lines: 382
    Code fences: `markdown template`, `text template`, and `markdown conceptual`.
    Tables or matrices: lookup archetype selector at 66-72; changed-fact route table at 186-196; status decision table at 221-225; flag lookup example at 259-262.
    Bracket tokens and group labels: profile labels, section cardinality, data dictionary field groups, validation groups.
    Status/progress/glyphs: glossary status values and capability status records; no progress bars or glyph legends.
    Records: source model, fact card, proof-bearing fact card, adjacent route record, command family card, CLI envelope record, glossary entry, data element card, command fact entry.
    Proof fields: `Evidence`, `Generated from`, `Source of truth`, `Last verified`, `Review trigger`; no explicit `Proof gap` field in proof-bearing templates.
    Relation records: canonical adjacent route record at 200-207.
    Diagrams: none.
    Isolated sentences: valid leads and captions; no standalone sentence defect found.

`docs/standards/reference/support-matrix.md`
    Lines: 422
    Code fences: `markdown template`, `text template`, `text conceptual`, `markdown conceptual`, and `mermaid`.
    Tables or matrices: support matrix table example at 247-251; lifecycle/source model records; status vocabulary records.
    Bracket tokens and group labels: authoring contract, cardinality groups, matrix field groups, validation groups.
    Status/progress/glyphs: support-display status set at 137-164; no progress bars or glyph legends.
    Records: support regime source model, status definition, lifecycle record, matrix-row record, skew bound record, deprecation record, migration anchor record.
    Proof fields: `Evidence`, `Generated from`, `Source of truth`, `Last verified`, `Review trigger`; no explicit `Proof gap` field in proof-bearing templates.
    Relation records: migration anchor embeds adjacent-route fields after support-specific preamble fields.
    Diagrams: one Mermaid lifecycle diagram at 195-217 with text equivalent at 219.
    Isolated sentences: valid leads, notes, and captions; no unsupported sentence island found.

### [CURRENT_SOURCE_CHECKS]

OpenAPI 3.2.0
    Source: `https://spec.openapis.org/oas/v3.2.0.html`
    Used for: `api.md:128` and `api.md:222`.
    Result: supports the default-new-local-HTTP-contract claim, subject to consumer toolchain pins.

TypeScript 7.0
    Source: `https://devblogs.microsoft.com/typescript/`
    Used for: `code-documentation.md:71` and `code-documentation.md:202`.
    Result: official TypeScript blog listed "Announcing TypeScript 7.0 Beta" on 2026-04-21, so produced docs should qualify the baseline as beta or verify a later stable release.

Python 3.15 annotation handling
    Source: `https://docs.python.org/3.15/library/annotationlib.html`
    Used for: `code-documentation.md:238` and `code-documentation.md:258`.
    Result: Python 3.15 docs exist as beta docs; annotationlib documentation supports deferred annotation inspection through PEP 649 and PEP 749.

Bash 5.3
    Source: `https://www.gnu.org/software/bash/manual/html_node/index.html`
    Used for: `code-documentation.md:271` and `code-documentation.md:293-299`.
    Result: GNU Bash manual identifies Bash version 5.3 and Edition 5.3.

C# XML comments and DocFX
    Source: `https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/xmldoc/`
    Used for: `code-documentation.md:171-198`.
    Result: Microsoft documents compiler XML doc generation and DocFX as a .NET API documentation generator.

API Extractor and TSDoc
    Source: `https://api-extractor.com/pages/tsdoc/doc_comment_syntax/`
    Used for: `code-documentation.md:202-234`.
    Result: API Extractor documents its TSDoc dialect and release tags; this supports generated-consumer wording independent of the TypeScript 7 release-state issue.

mkdocstrings Python docstrings
    Source: `https://mkdocstrings.github.io/python/usage/configuration/docstrings/`
    Used for: `code-documentation.md:238-267`.
    Result: mkdocstrings-python documents `docstring_style: google`, supporting the Google docstring generated-reference path.

PostgreSQL 18 catalog comments
    Source: `https://www.postgresql.org/docs/18/catalog-pg-description.html`
    Used for: `code-documentation.md:305-333`.
    Result: PostgreSQL 18 docs confirm `pg_description` stores comments manipulated with `COMMENT` and viewed through `psql` describe commands.

endoflife.date API availability
    Source: `https://endoflife.date/` and `https://endoflife.date/docs/api/v1/`
    Used for: `support-matrix.md:128-131`.
    Result: API availability is source-backed, but exact field-family proof for `isEoas`/`eoasFrom` was not obtained.

## [RECOMMENDATIONS]

1. Fix the README H1 rejected/accepted example first; it is the only direct contradiction found in the reference corpus.
2. Normalize proof-bearing templates so `Proof gap:` is a first-class proof field instead of a possible `Evidence:` value.
3. Declare local relation-record preamble exceptions in API, support matrix, and code documentation, then keep the shared relation field run contiguous.
4. Source-qualify code-documentation version baselines where the maintained source says beta, preview, or target rather than stable current release.
5. Decide whether the README examples remain a late section because they demonstrate combined structure, or move them beside their rules to match the example-placement owner.

## [PROOF_GAPS]

- Local Mermaid render proof was not run for `support-matrix.md:195-217`.
- C# 14 release-state proof was not separately checked; only XML documentation and DocFX capability sources were checked.
- Exact primary schema proof for the `support-matrix.md:128-131` `isEoas` field-family names was not obtained.
- No local Markdown link, anchor, or docs-build validation ran because this pass wrote one report and did not change active standards.

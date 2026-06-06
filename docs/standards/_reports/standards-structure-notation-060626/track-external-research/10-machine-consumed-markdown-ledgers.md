Question: What current source-backed rule should govern machine-consumed Markdown ledgers such as Roslyn analyzer release files and parser-owned generated fragments?
Type: standards-research
Lane: track-external-research
Merge key: docs/standards/information-structure.md :: machine-consumed Markdown ledgers :: promote parser-owned exception
Target owner: docs/standards/information-structure.md
Source basis: active standards and session reports; local Roslyn analyzer ledger files; local release-discipline parser tests; official Roslyn analyzer release-tracking help; CommonMark 0.31.2; GitHub Flavored Markdown 0.29-gfm
Promotion target: docs/standards/information-structure.md; docs/standards/proof.md; docs/standards/formatting.md; docs/standards/reference/code-documentation.md
Outcome: PROMOTE

## [FINDINGS]

### [1][LEDGER_CONSUMER]

Recommendation: ADD.
Owner route: `docs/standards/information-structure.md` should promote machine-consumed Markdown from the late callouts/details area to a first-class record near structured records, code blocks, and generated surfaces.
Evidence: `docs/standards/information-structure.md:481-488` already allows a narrower shape when a parser, analyzer, release ledger, or generator consumes exact headings, fields, or row order. `docs/standards/reference/code-documentation.md:90` already exempts Roslyn analyzer release ledgers, generated API mirrors, and generated catalog fragments from ordinary prose reshaping for fields the consumer reads.
External proof: Roslyn analyzer release tracking expects `AnalyzerReleases.Unshipped.md` and `AnalyzerReleases.Shipped.md` as additional files for analyzer projects, uses `### New Rules`, `### Removed Rules`, and `### Changed Rules` sections, and defines a table with `Rule ID`, `Category`, `Severity`, and `Notes`.
Local proof: `tools/cs-analyzer/AnalyzerReleases.Unshipped.md:1-8` uses semicolon metadata comments, an unbracketed `### New Rules` heading, and a non-indexed pipe table. `tools/cs-analyzer/AnalyzerReleases.Shipped.md:1-2` is an empty shipped ledger with the same official help pointer.
Correction: the active rule should say parser-owned Markdown keeps the consumer's required filename, heading, row, field, and delimiter shape even when that violates ordinary bracketed headings, `[INDEX]` tables, fence intent labels, or prose-softening rules.
Validation route: name the consumer and run the consumer's parser or closest local validator. For Rasm's current ledger rows, `tests/tools/cs-analyzer/ReleaseDisciplineTests.cs` is the local validator.
Proof gap: the local analyzer project references `Microsoft.CodeAnalysis.Analyzers`, but `tools/cs-analyzer/CsAnalyzer.csproj:15-18` does not declare `AnalyzerReleases.*.md` as `AdditionalFiles`; official Roslyn release-tracking analyzer activation in the project build is therefore a project-wiring proof gap unless another imported target adds them.

### [2][LOCAL_PARSER_SHAPE]

Recommendation: CHANGE.
Owner route: `docs/standards/proof.md` should require machine-consumed Markdown proof to separate shape enforcement, source provenance, semantic validation, and runtime or build wiring.
Evidence: `tests/tools/cs-analyzer/ReleaseDisciplineTests.cs:12-24` parses the unshipped ledger and checks active diagnostic IDs, categories, and severities against `AnalyzerTestHarness.SupportedDiagnostics()`. `tests/tools/cs-analyzer/ReleaseDisciplineTests.cs:27-35` checks shipped and unshipped IDs do not overlap. `tests/tools/cs-analyzer/ReleaseDisciplineTests.cs:99-103` defines the row regex as `CSPdddd | Category | Severity |`.
Implication: the local parser proves the row shape and active rule inventory, not the semicolon comment lines, `### New Rules` heading, release-section movement, or official additional-file integration.
Correction: proof wording should name which consumer field set is actually validated. A Roslyn-style release ledger can have both an upstream parser contract and a repo-local test parser; a report or active standard must not collapse those into one proof claim.
Validation route: use `uv run python -m tools.quality test run --target tests/tools/cs-analyzer/CsAnalyzer.Tests.csproj` only when changing the ledger rows, analyzer descriptor IDs/categories/severities, or parser tests. For docs-only standards wording, `git diff --check` is sufficient unless the wording claims the test passed.
Proof gap: this report did not run the C# test rail because it created source material only and did not change the ledger or analyzer code.

### [3][NO_NORMALIZE_RULE]

Recommendation: ADD.
Owner route: `docs/standards/formatting.md` should point to the machine-consumed exception before applying bracketed headings, enumerable table rubrics, alignment, absence markers, and source-comment notation.
External proof: GFM tables are leaf blocks with a header row, delimiter row, and data rows; inline content may appear in cells, block-level content cannot. GFM also allows body rows to have fewer or more cells than the header, inserting empty cells or ignoring excess cells in rendered output. CommonMark parsing is line and block oriented before inline parsing.
Implication: Markdown source shape is not only presentation. A consuming parser may choose a narrower row regex, heading scan, or field-order scan than the renderer. Prettifying a machine ledger can preserve rendered HTML while breaking the consumer.
Correction: ordinary standards normalization applies around the parser-owned block, not through it. Surrounding human guidance still follows Rasm heading, table, proof, and prose rules; the consumed block keeps the external or local parser's shape.
Validation route: run a parser-aware check, not only a renderer or `git diff --check`, when the consumed block changes. Use `git diff --check` only for whitespace safety.
Proof gap: no generic Markdown parser or table validator exists in the bounded repo proof for these files; consumer-specific tests are the validation route.

### [4][GENERATED_AND_TEMPLATE_SURFACES]

Recommendation: MERGE.
Owner route: `docs/standards/agentic-documentation.md` should keep generated mirrors and retrieval surfaces in its generated-surface lane, while `docs/standards/information-structure.md` owns the carrier shape for machine-consumed Markdown blocks.
Evidence: `docs/standards/agentic-documentation.md` already separates generated mirrors, retrieval provenance, structured-output contracts, and MCP catalogs. `docs/standards/proof.md` already requires generated artifacts to name `Generated from:` and `Source of truth:`. Existing report `track-external-research/01-gfm-github-markdown-capabilities.md` covers renderer capabilities but does not cover Roslyn release ledger parser ownership.
Correction: generated Markdown and parser-owned Markdown should share the same first fields only where useful: `Consumer`, `Required shape`, `Source of truth`, `Validation command` or `Proof gap`, and `Review trigger`. Generated mirrors additionally need `Generated from`, `Omissions`, and `Edit rule`; hand-maintained ledgers need `No-normalize rule` and release movement behavior.
Validation route: generated mirrors use regeneration or generated-output comparison; parser-owned hand-maintained ledgers use the consumer/parser test. Do not validate generated mirrors by hand-editing table shape.
Proof gap: no generated contract docs were exhaustively inspected in this pass; this finding merges prior generated-surface rules with the local analyzer ledger evidence only.

## [EVIDENCE]

[PRIOR_REPORTS_EXTENDED]:
- `track-external-research/01-gfm-github-markdown-capabilities.md`: renderer and GitHub Markdown capability proof. This report adds parser-owned ledger evidence, not another renderer pass.
- `track-information-structure/02-information-structure-cross-type-fit.md`: identified machine-consumed Markdown as a high-authority exception and left exact Roslyn release tracking as a proof gap.
- `track-repo-markdown/02-non-standards-markdown-patterns.md`: sampled `AnalyzerReleases.Unshipped.md` as a machine-consumed exception and left current Roslyn shape confirmation as a proof gap.
- `track-synthesis/00-collective-task-list.md`: open task to move machine-consumed Markdown earlier and name parser-owned exceptions.

[LOCAL_SOURCE_SET]:
- `docs/standards/information-structure.md:481-488`.
- `docs/standards/reference/code-documentation.md:90`.
- `tools/cs-analyzer/AnalyzerReleases.Unshipped.md:1-94`.
- `tools/cs-analyzer/AnalyzerReleases.Shipped.md:1-2`.
- `tools/cs-analyzer/CsAnalyzer.csproj:15-18`.
- `Directory.Packages.props:60`.
- `tests/tools/cs-analyzer/Infrastructure/AnalyzerTestHarness.cs:24-27`.
- `tests/tools/cs-analyzer/ReleaseDisciplineTests.cs:12-35`.
- `tests/tools/cs-analyzer/ReleaseDisciplineTests.cs:67-103`.

[PRIMARY_SOURCE_SET_ACCESSED_2026_06_06]:
- Roslyn analyzer release tracking help: https://github.com/dotnet/roslyn-analyzers/blob/main/src/Microsoft.CodeAnalysis.Analyzers/ReleaseTrackingAnalyzers.Help.md.
- GitHub Flavored Markdown Spec 0.29-gfm tables extension: https://github.github.com/gfm/#tables-extension-.
- CommonMark 0.31.2 parsing strategy: https://spec.commonmark.org/0.31.2/#appendix-a-parsing-strategy.

[SOURCE_NOTES]:
- Roslyn release tracking help is upstream source for the two ledger filenames, release movement model, rule-change categories, table columns, and `AdditionalFiles` enablement route.
- GFM is source proof for table grammar and renderer constraints only; it does not prove a non-GFM consumer's parser behavior.
- CommonMark is source proof that Markdown parsing starts with line-oriented block structure, supporting the no-normalize rule for parser-owned source shapes.

## [RECOMMENDATIONS]

[PROMOTE]:
- Add a high-salience `[MACHINE_CONSUMED_MARKDOWN]` section in `docs/standards/information-structure.md` before ordinary table and code-fence normalization can be read as universal.
- Required record fields: `Consumer`, `Required shape`, `Source of truth`, `Validation command` or `Proof gap`, `Review trigger`, and `Exception`.
- State the no-normalize rule: do not convert parser-owned headings, tables, field labels, comments, row order, or fence grammar into ordinary documentation style unless the named consumer changes.
- Add a Roslyn release-ledger example as a named exception: preserve `AnalyzerReleases.Shipped.md`, `AnalyzerReleases.Unshipped.md`, `### New Rules`, optional release sections in shipped files, and the `Rule ID | Category | Severity | Notes` table shape.

[MERGE]:
- Merge generated-mirror wording with `agentic-documentation.md` and `proof.md` instead of creating a second generated-surface vocabulary in `information-structure.md`.
- Merge row-level proof with `proof.md`: shape validation proves only parse shape; semantic validation proves descriptor/category/severity match; build wiring proves official analyzer integration.
- Merge notation exceptions into `formatting.md`: bracketed headings and `[INDEX]` tables are default notation, not parser-owned ledger notation.

[KEEP]:
- Keep `docs/standards/reference/code-documentation.md:90` directionally intact: source-comment standards may preserve parser-required shapes for release ledgers, generated API mirrors, and generated catalog fragments.
- Keep ordinary surrounding guidance under shared standards. The exception protects the consumed block, not the whole document.

## [CANDIDATE_WORDING]

Candidate owner: `docs/standards/information-structure.md`.

```markdown template
## [N][MACHINE_CONSUMED_MARKDOWN]

Machine-consumed Markdown keeps the shape the named consumer parses. Apply ordinary heading, table, fence, proof, and prose rules around the consumed block, not through the fields, headings, rows, comments, or order the consumer reads.

[MACHINE_CONSUMED_RECORD]:
- Consumer: parser, analyzer, release ledger, generator, renderer, retrieval index, or other named tool.
- Required shape: filenames, headings, fields, row order, delimiters, comments, or fence grammar the consumer reads.
- Source of truth: upstream format document, local parser source, generator, schema, or generated contract.
- Validation command: exact consumer/parser/generator check, or `Proof gap:` when no configured check exists.
- Review trigger: consumer version, parser regex, generated schema, ledger release process, or source-shape rule changes.
- Exception: parser-owned Markdown must not be normalized into ordinary documentation style unless the named consumer changes.
```

Candidate Roslyn note:

```markdown template
Roslyn analyzer release ledgers preserve `AnalyzerReleases.Shipped.md`, `AnalyzerReleases.Unshipped.md`, release headings, rule-change section headings, and the `Rule ID | Category | Severity | Notes` table shape required by the release-tracking consumer. Do not add `[INDEX]`, bracketed headings, or prose records to the consumed table.
```

## [PROOF_GAPS]

- Official Roslyn release-tracking analyzer activation is not proven from the bounded local project read because `tools/cs-analyzer/CsAnalyzer.csproj` references `Microsoft.CodeAnalysis.Analyzers` but does not declare the two release ledger files as `AdditionalFiles`.
- The local release-discipline tests prove row parse shape and descriptor/category/severity consistency, but they do not prove the semicolon comment header, shipped release sections, or upstream release movement workflow.
- No C# test rail ran in this pass. The report changed only `_reports/**` source material.
- No generic Markdown parser, link checker, anchor checker, or renderer gate ran. This pass used source reads and primary external docs only.
- Generated ledgers beyond the analyzer release files were not exhaustively inventoried. Future promotion should inspect generated contract docs or state that the rule applies by carrier shape, not by a complete generated-file list.

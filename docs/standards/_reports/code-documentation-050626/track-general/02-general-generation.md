# [GENERAL_02_GENERATION_RESEARCH]

Focus: modern API reference generation, source comments versus generated mirrors, stale-doc prevention, and generated catalog boundaries for `docs/standards/reference/code-documentation.md`.

Scope: research report only. No active standards were edited. The worktree already had a modification in `docs/standards/reference/code-documentation.md`; this report treats that file as current local input and leaves it untouched.

## [1][CHECKS]

Local source reads:
- `CLAUDE.md`
- `AGENTS.md`
- `docs/standards/README.md`
- `docs/standards/AGENTS.md`
- `docs/standards/reference/code-documentation.md`
- `docs/standards/style-guide.md`
- adjacent checks: `docs/standards/reference/api.md`, `docs/standards/proof.md`

Local searches:
- `rg -n "GENERAL 2|general-02|code-documentation-research|generated catalog|generated-reference|source comments|generated mirrors|DocFX|TSDoc|API Extractor|TypeDoc|Griffe|mkdocstrings" docs/standards docs -g '*.md'`
- `rg -n "docfx|typedoc|api-extractor|@microsoft/api-extractor|mkdocstrings|griffe|pydoc|sphinx|DocumentationFile|GenerateDocumentationFile|CS1591|NoWarn|TreatWarningsAsErrors|xml documentation|COMMENT ON|pg_description|obj_description|col_description|shobj_description" . -g '!docs/standards/_reports/**' -g '!node_modules/**' -g '!bin/**' -g '!obj/**'`
- `fd -H 'docfx|typedoc|api-extractor|mkdocs|mkdocstrings|griffe' . -t f`
- `rg -n "docfx|typedoc|api-extractor|mkdocstrings|griffe" package.json pnpm-lock.yaml pnpm-workspace.yaml pyproject.toml uv.lock Directory.Packages.props Directory.Build.props .config/dotnet-tools.json`

Local configured truth:
- `Directory.Build.props:66-68` sets `TreatWarningsAsErrors=true`, `GenerateDocumentationFile=true`, and suppresses `CS1591`.
- `pyproject.toml:481-488` configures Ruff pydocstyle with `convention = "google"` and pydoclint one-line docstring behavior.
- No repo manifest or file-discovery hit proves configured `docfx`, `typedoc`, `api-extractor`, `mkdocstrings`, or `griffe` generation in this checkout.

Current primary sources checked:
- DocFX .NET API docs: https://dotnet.github.io/docfx/docs/dotnet-api-docs.html
- Microsoft C# XML documentation tags: https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/xmldoc/recommended-tags
- API Extractor TSDoc comment syntax: https://api-extractor.com/pages/tsdoc/doc_comment_syntax/
- TypeDoc doc comments: https://typedoc.org/documents/Doc_Comments.html
- mkdocstrings Python usage: https://mkdocstrings.github.io/python/usage/
- mkdocstrings handlers: https://mkdocstrings.github.io/usage/handlers/
- Griffe docstring parser reference: https://mkdocstrings.github.io/griffe/reference/docstrings/
- Python 3.15 `annotationlib`: https://docs.python.org/3.15/library/annotationlib.html
- GNU Bash 5.3 manual: https://www.gnu.org/software/bash/manual/bash.html
- PostgreSQL 18 `COMMENT`: https://www.postgresql.org/docs/18/sql-comment.html
- PostgreSQL 18 `pg_description`: https://www.postgresql.org/docs/18/catalog-pg-description.html
- PostgreSQL 18 system information functions: https://www.postgresql.org/docs/18/functions-info.html

## [2][FINDINGS]

[F1][SOURCE_AND_GENERATED_SPLIT]

The current standard's core boundary is correct. Source declarations own machine shape, source comments own caller-visible semantics that the declaration cannot express, and generated mirrors expose source truth. DocFX supports this model directly: XML documentation comments feed rendered HTML, and .NET API docs run through a metadata stage that produces YAML before the build stage transforms that YAML into HTML. API Extractor and TypeDoc also parse source comments rather than treating generated docs as authoring truth.

Impact: keep the source-first rule. The generated mirror should stay a consumer and proof surface, not a second place to edit symbol text.

[F2][CONFIGURED_GENERATOR_VERSUS_AVAILABLE_PROFILE]

The repo currently proves C# XML documentation file generation through `GenerateDocumentationFile=true`, but it does not prove adoption of DocFX, TypeDoc, API Extractor, mkdocstrings, or Griffe as configured generation rails. The standard can name those tools as language profiles, but it should not imply any of them are current repository gates or generated artifacts unless a manifest, command, checked-in output, or local tool route proves adoption.

Impact: active wording should distinguish "toolchain parses this syntax" from "this repository generates this output."

[F3][TSDOC_IS_COMMENT_SYNTAX_NOT_GENERATED_OUTPUT]

`TSDoc` is a comment syntax/profile consumed by API Extractor and other tools. It is not itself a generated reference artifact. The current produced-shape field lists `TSDoc` beside `compiler XML`, `DocFX`, `API Extractor`, `TypeDoc`, `Griffe`, `mkdocstrings`, and catalog output under `Generated reference`; that creates a small category leak.

Impact: split generated-reference wording into comment syntax and generator/output. Keep `TSDoc` in TypeScript comment profile language, not in a generated-reference field.

[F4][API_EXTRACTOR_AND_TYPEDOC_HAVE_DIFFERENT_COMMENT_DISCOVERY_RISK]

API Extractor parses comments that appear in emitted `.d.ts`, use the `/**` delimiter, and sit immediately before an exported declaration or package documentation block. TypeDoc mirrors TypeScript comment discovery in most cases but also supports extra locations such as comments on union branches and export specifiers. That difference matters because an API Extractor-backed package canon should not rely on comments that only TypeDoc discovers.

Impact: generated-reference handoffs for TypeScript should name the controlling generator. If API Extractor is the strict canon, require emitted declaration visibility. If TypeDoc is only the browsing renderer, treat TypeDoc-only discovery as renderer behavior, not package contract proof.

[F5][CS1591_SUPPRESSION_SUPPORTS_SELECTIVE_COMMENT_POLICY]

Rasm enables XML documentation file generation but suppresses `CS1591`. Microsoft documents `CS1591` as the warning for missing XML comments when `/doc` is used, and the repo suppression means public visibility does not force comment churn. That aligns with `code-documentation.md`: public visibility creates a review question, not an automatic comment requirement.

Impact: keep the "Reject missing-comment churn" rule. Tighten any validation phrase that could imply warning-as-error enforces missing public comments in Rasm.

[F6][CREF_AND_PARAM_VALIDATION_STILL_MATTER]

Microsoft's XML documentation guidance states that the compiler checks parameter existence for `<param>` and checks `cref` references. With `TreatWarningsAsErrors=true`, unresolved or malformed XML documentation warnings remain relevant unless separately suppressed. This supports the standard's rule to resolve code references through the controlling toolchain or omit them.

Impact: keep the reference-resolution rule, but phrase it around checked reference correctness rather than broad public-member coverage.

[F7][PYTHON_GENERATION_PROFILE_IS_ADOPTABLE_NOT_CONFIGURED]

mkdocstrings Python can inject API reference from `::: path.to.object`, uses a Python handler by default when installed, and supports source lookup through configured `paths`. The project currently proves Google docstring style through Ruff configuration, not mkdocstrings or Griffe generation. Python 3.15 `annotationlib` also proves that annotation introspection is a live runtime surface, so the standard's "annotations own shape, docstrings own semantics" split is sound.

Impact: keep Google docstrings as repo style. Do not claim generated Python API docs exist until `mkdocs`, `mkdocstrings`, `griffe`, generated pages, or a build route exists.

[F8][POSTGRESQL_CATALOG_COMMENTS_ARE_GENERATED_CATALOG_INPUT]

PostgreSQL 18 stores comments in `pg_description`; `COMMENT` output can be viewed by `psql` describe commands and retrieved with `obj_description`, `col_description`, and `shobj_description`. The current standard correctly treats `COMMENT ON` as durable catalog documentation and SQL source comments as local rationale.

Impact: keep the catalog extraction boundary. Generated dictionaries should extract catalog facts and comments; they should not copy migration prose or maintain a parallel hand-written catalog.

[F9][BASH_HAS_NO_DOCSTRING_GENERATION_ROUTE]

The Bash 5.3 manual verifies current-shell command substitution forms and persistent side effects. It does not create a source-comment-to-generated-reference mechanism. The current standard correctly treats Bash comments as contract comments for scripts, functions, traps, environment, stdout, stderr, and exit-status behavior rather than generated API reference input.

Impact: keep Bash out of generated-reference handoffs by default.

## [3][RECOMMENDATIONS]

[ADD]

- Add one sentence near `SOURCE_TRUTH` or `LANGUAGE_CAPSULES`: generator names are profile candidates until repository manifests, checked-in generated output, or current command output proves adoption.
- Add a TypeScript caveat: API Extractor-backed contract docs require comments that survive into emitted declarations; TypeDoc-only discovery is renderer behavior unless TypeDoc is the declared generated-reference owner.
- Add a C# local-proof note in validation or C# capsule language: Rasm generates XML documentation files and suppresses `CS1591`, so missing public comments are not a repo gate; malformed XML, bad `<param>`, and bad `cref` remain checked documentation correctness concerns where warnings are active.

[CHANGE]

- Change the produced-shape `Generated reference` examples so `TSDoc` is not listed as generated output. Suggested split:
  - `Comment syntax/profile`: compiler XML comments, TSDoc, Google docstrings, Bash contract comments, SQL `COMMENT ON`.
  - `Generated reference`: compiler XML file, DocFX YAML or HTML, API Extractor model or report, TypeDoc HTML or JSON, Griffe or mkdocstrings output, PostgreSQL catalog output, UID, or anchor.
- Reword "API Extractor is the strict comment canon, and TypeDoc is the browsing renderer" to "when configured" language unless the repo adopts those tools. Current repo truth does not prove either tool is installed or required.
- Reword generated-reference handoff examples to require a changed generated output, generated anchor, or adjacent reader action. Pure source-comment clarity fixes should remain no-adjacent-change by default.

[REMOVE]

- Remove any wording that lets generated mirrors be interpreted as editable source truth.
- Remove any implication that a generated catalog should be maintained in Markdown when a source generator, catalog extraction query, or checked-in generated artifact owns the surface.
- Remove any implied configured generator claim for DocFX, TypeDoc, API Extractor, mkdocstrings, or Griffe unless a manifest or command route is added.

## [4][NO_CHANGE_CONFIRMATIONS]

- Keep the opening rule: code documentation exists only when public callers need semantics the declaration cannot express.
- Keep the route-away boundary to `api.md` for generated and contract-backed API reference.
- Keep the route-away boundary to `reference.md` for curated lookup facts and command dictionaries.
- Keep "generated catalogs are regenerated or linked, not rebuilt by hand."
- Keep "source comments document semantics; generated references mirror source truth and expose anchors."
- Keep lifecycle tags limited to external support contracts; do not preserve greenfield internal surfaces through deprecation labels.
- Keep PostgreSQL `COMMENT ON` as durable schema and catalog documentation, with SQL source comments limited to local rationale.
- Keep Bash contract comments as non-generated source documentation.
- Keep proof-field ownership in `proof.md`; do not duplicate proof vocabulary inside `code-documentation.md` beyond route-specific field names.

## [5][STALE_DOC_PREVENTION_MODEL]

Use this chain for generated API reference maintenance:

1. Edit source declaration, schema, SQL object, or source comment.
2. Regenerate or compare the generated artifact when a configured generator exists.
3. Update an API page only when the generated artifact identity, route, generated anchor, or caller action changes.
4. Update README, architecture, reference, support, runbook, how-to, or tutorial only when the changed fact alters that reader action.
5. Delete or route away hand-maintained mirrors that repeat generated member, endpoint, command, or catalog rows.

This model matches the active standards split: `code-documentation.md` owns source-comment semantics, `api.md` owns generated library reference records, `reference.md` owns curated lookup facts, and `proof.md` owns generated artifact evidence fields.

## [6][CONFIDENCE]

High for the source/generated split, generated catalog boundary, C# XML documentation behavior, PostgreSQL catalog comment behavior, and Bash non-docstring boundary.

Medium for TypeScript generator role wording because API Extractor and TypeDoc are both valid current tools but no repo manifest proves either is configured here.

Medium for Python generator wording because current primary sources support mkdocstrings and Griffe as a generated-reference profile, while local repo truth only proves Google docstring style.

## [7][PROOF_GAPS]

- No generated-reference rail was run. This was a research-only report, and the active standard says generated-reference rails stay unrun unless executable source, configs, generated artifacts, or tooling change.
- No active standards were edited, so link and anchor validation for the active corpus was not required by this report.
- Current external pages were checked on 2026-06-05. For future standards edits, re-check drift-prone generator behavior before claiming latest support or configured adoption.

## [8][TRANSCRIPT_SUMMARY]

Commands run from `/Users/bardiasamiee/Documents/99.Github/Rasm`:
- `rg` over memory registry for prior `docs/standards` context.
- `fd` for root and nested instruction files.
- `wc -l` over required standards files.
- `sed` and `nl -ba` full reads of required standards and target file.
- `rg` for generated-reference, source-comment, and generated-catalog language across `docs/standards`.
- `rg` and `fd` for configured generator tools in repo manifests and files.
- `git status --short -- docs/standards` before writing; pre-existing `M docs/standards/reference/code-documentation.md` was observed.
- Context7 resolution and docs queries for DocFX, TypeDoc, and mkdocstrings.
- Web primary-source checks for DocFX, Microsoft XML docs, API Extractor, TypeDoc, mkdocstrings, Griffe, Python 3.15, GNU Bash 5.3, and PostgreSQL 18.

Write action:
- Created `docs/standards/_reports/code-documentation-050626/track-general/02-general-generation.md` only.

## [9][POST_WRITE_VERIFICATION]

- `git diff --check -- docs/standards/_reports/code-documentation-050626/track-general/02-general-generation.md` passed.
- `git status --short -- docs/standards` still showed the pre-existing `M docs/standards/reference/code-documentation.md` plus multiple untracked sibling `_reports/` reports from other workers. This worker changed only `general-02-generation.md`.

# standards reference context-poison audit

## Scope read

Read every Markdown file in `/Users/bardiasamiee/Documents/99.Github/Rasm/docs/standards/reference` fully:

- `/Users/bardiasamiee/Documents/99.Github/Rasm/docs/standards/reference/api.md`
- `/Users/bardiasamiee/Documents/99.Github/Rasm/docs/standards/reference/code-documentation.md`
- `/Users/bardiasamiee/Documents/99.Github/Rasm/docs/standards/reference/readme.md`
- `/Users/bardiasamiee/Documents/99.Github/Rasm/docs/standards/reference/reference.md`
- `/Users/bardiasamiee/Documents/99.Github/Rasm/docs/standards/reference/support-matrix.md`

I also read the active repo instruction chain and standards review context needed to audit this folder: `/Users/bardiasamiee/Documents/99.Github/Rasm/CLAUDE.md`, `/Users/bardiasamiee/Documents/99.Github/Rasm/AGENTS.md` from the prompt, `/Users/bardiasamiee/Documents/99.Github/Rasm/docs/standards/README.md`, `/Users/bardiasamiee/Documents/99.Github/Rasm/docs/standards/AGENTS.md`, and the shared standards `/Users/bardiasamiee/Documents/99.Github/Rasm/docs/standards/agentic-documentation.md`, `/Users/bardiasamiee/Documents/99.Github/Rasm/docs/standards/information-structure.md`, `/Users/bardiasamiee/Documents/99.Github/Rasm/docs/standards/style-guide.md`, `/Users/bardiasamiee/Documents/99.Github/Rasm/docs/standards/proof.md`, and `/Users/bardiasamiee/Documents/99.Github/Rasm/docs/standards/formatting.md`.

## Method

I audited for generic-standard context poisoning: examples or templates that future authors might copy as real project policy, local project coupling in generic material, source/provenance chatter that crowds the durable rule, fragile exact versions or lifecycle facts, route-body duplication, validation/load-order repetition, defensive compatibility wording, and anti-pattern examples that teach the wrong shape by being too specific.

No target standard files were edited. This report is the only file written.

## Findings

### F-01: support-matrix examples encode local Rasm-like support surfaces

- Severity: high
- Confidence: evidence-backed
- Location: `/Users/bardiasamiee/Documents/99.Github/Rasm/docs/standards/reference/support-matrix.md:179`, `/Users/bardiasamiee/Documents/99.Github/Rasm/docs/standards/reference/support-matrix.md:252`, `/Users/bardiasamiee/Documents/99.Github/Rasm/docs/standards/reference/support-matrix.md:322`
- Evidence:
  - Lines 179-191 show a lifecycle example for a "runtime or framework line" whose phase grants are "compile target for repository projects" and whose controlling source is "repository target-framework configuration or support policy."
  - Lines 252-254 use `Library projects`, `Host runtime refs`, `Generated API lookup`, `host manifest`, and `<api metadata command>`.
  - Line 322 rejects "treating an unsupported platform package as a runtime dependency", "host-specific collection semantics as generic lists", and "resolving host API symbols without maintained metadata."
- Why it may poison context:
  - The examples are meant to be generic support-matrix shapes, but they preserve the current repository's library, host-runtime, generated-API, and metadata-routing concerns. A future agent could reproduce Rasm-specific host/package assumptions in unrelated support matrices.
  - The examples also teach local source-map concerns as the default support-matrix mental model, which can bias generic support docs toward host/API lookup rather than product, platform, or runtime support.
- Suggested disposition:
  - Replace the examples with neutral surfaces such as `Client library`, `Managed runtime`, `Provider integration`, and `<compatibility-check output>`.
  - Keep one explicit local example only if the file is intentionally project-specific; otherwise move Rasm-like host/API wording to a repo-specific support or system-api-map page.

### F-02: deprecation examples look like a stale local command migration

- Severity: high
- Confidence: evidence-backed
- Location: `/Users/bardiasamiee/Documents/99.Github/Rasm/docs/standards/reference/support-matrix.md:340`
- Evidence:
  - Lines 340-352 give a deprecation record for `former split inventory command`, `<replacement health-check command>`, `<tool> <replacement-command>`, and source inventory returning through "one declared response envelope."
- Why it may poison context:
  - This reads like a specific tool-rebuild or command-surface migration disguised as a generic support-matrix example. Even with placeholders, the terms `split inventory`, `health-check command`, `source inventory`, and `response envelope` teach a local command lineage as reusable support doctrine.
  - Future docs could copy the shape and preserve old command names or migration narratives instead of documenting the current support status directly.
- Suggested disposition:
  - Replace with an abstract deprecated API or feature example: `Surface: legacy export endpoint`, `Replacement: batch export API`, `Warning signal: response header`, `Removal: source policy date`.
  - If command deprecation is important, put it in the API or command reference standard with neutral names and no implied local migration history.

### F-03: README examples leak local route taxonomy into generic templates

- Severity: medium
- Confidence: evidence-backed
- Location: `/Users/bardiasamiee/Documents/99.Github/Rasm/docs/standards/reference/readme.md:351`, `/Users/bardiasamiee/Documents/99.Github/Rasm/docs/standards/reference/readme.md:368`
- Evidence:
  - Lines 351-363 show a hub-index table whose rows are "standard-library, runtime, and package posture", "external dependency facts and local adoption posture", and "test-tool API facts and local gate routing."
  - Lines 392-400 route the root README example to `<tool-reference>` and `<runtime-or-dependency-reference>` for local tool, API, runtime, host, dependency, package, and replacement API lookup.
- Why it may poison context:
  - These examples are generic README guidance, but their route taxonomy matches this repository's `docs/system-api-map`, `docs/external-libs`, and `docs/testing-libs` posture. That makes local research lanes look like universal README sections.
  - A future README author could over-prioritize local tool/reference maps and under-serve public entry, install, usage, or domain-specific reader routes.
- Suggested disposition:
  - Change the examples to neutral documentation routes: `deployment support`, `API contracts`, `operations guide`, `architecture overview`, or `integration reference`.
  - Keep local system/package/test-tool route names out of generic templates unless the produced README is explicitly for this standards corpus.

### F-04: command-reference example uses a repository quality-rail shape

- Severity: medium
- Confidence: evidence-backed
- Location: `/Users/bardiasamiee/Documents/99.Github/Rasm/docs/standards/reference/reference.md:329`
- Evidence:
  - Lines 329-338 show a command fact entry with `Invocation: <tool> report <scope>`, `Flag: report`, and `Effect: reports diagnostics for the scoped project closure.`
- Why it may poison context:
  - "report diagnostics for the scoped project closure" is quality-tool language, not a neutral command-reference example. It can make future generic reference docs inherit Rasm's scoped static/report/build vocabulary.
  - The example also labels `report` as a `Flag`, even though the invocation reads like a subcommand. That can teach command-surface imprecision in a standard meant to prevent it.
- Suggested disposition:
  - Replace with a neutral flag example such as `Invocation: <tool> export --format <format>`, `Flag: --format`, `Effect: selects the output encoding`.
  - If demonstrating subcommands, rename the field to `Subcommand:` and keep flags separate.

### F-05: exact future-version claims may age into false authority

- Severity: medium
- Confidence: evidence-backed
- Location: `/Users/bardiasamiee/Documents/99.Github/Rasm/docs/standards/reference/api.md:129`, `/Users/bardiasamiee/Documents/99.Github/Rasm/docs/standards/reference/code-documentation.md:71`, `/Users/bardiasamiee/Documents/99.Github/Rasm/docs/standards/reference/code-documentation.md:121`, `/Users/bardiasamiee/Documents/99.Github/Rasm/docs/standards/reference/code-documentation.md:125`, `/Users/bardiasamiee/Documents/99.Github/Rasm/docs/standards/reference/code-documentation.md:217`, `/Users/bardiasamiee/Documents/99.Github/Rasm/docs/standards/reference/support-matrix.md:130`
- Evidence:
  - `api.md` line 129 mandates OpenAPI 3.2.0 for new local HTTP contracts unless a consumer pin requires another supported line; line 223 repeats the validation check.
  - `code-documentation.md` line 71 requires language capsules for C# 14, TypeScript 7, Python 3.15, Bash 5.3+, and PostgreSQL 18.4; lines 125-131 repeat those version labels in a capsule index.
  - `code-documentation.md` line 121 states these are target standards and does not claim current repository execution on Python 3.15, but later capsule text still presents detailed feature claims such as Python PEP references on lines 217-224.
  - `support-matrix.md` line 130 imports exact lifecycle field names such as `isEoas`, `eoasFrom`, `isEol`, `eolFrom`, `isEoes`, `eoesFrom`, and `isDiscontinued`.
- Why it may poison context:
  - Exact future or current-looking versions are high-authority tokens. They can be copied into produced docs as if they are current project or upstream facts, especially when embedded in validation checks.
  - Some labels are deliberately aspirational target baselines, but the distinction is local to one sentence and easy to lose during retrieval or excerpting.
- Suggested disposition:
  - Keep the target-standard intent, but move exact version tables into a clearly labeled `Target baselines` record with `Evidence` or `Proof gap` fields, or phrase generic standards as "the project-declared language baseline."
  - For OpenAPI, either cite a maintained source route or make the rule "latest project-approved OpenAPI line" with a consumer-pin escape.
  - For imported lifecycle field names, name the source schema family or convert the exact field list into an example that cannot read as universal support-schema law.

### F-06: proof templates repeat full provenance packets until they look mandatory

- Severity: medium
- Confidence: evidence-backed
- Location: `/Users/bardiasamiee/Documents/99.Github/Rasm/docs/standards/reference/api.md:93`, `/Users/bardiasamiee/Documents/99.Github/Rasm/docs/standards/reference/reference.md:119`, `/Users/bardiasamiee/Documents/99.Github/Rasm/docs/standards/reference/support-matrix.md:117`
- Evidence:
  - `api.md` lines 93-109 include contract records with `Evidence`, `Generated from`, `Controlling source`, `Proof gap`, `Last verified`, and `Review trigger`.
  - `reference.md` lines 119-130 include source model records with the same proof packet plus `Refresh command`.
  - `support-matrix.md` lines 117-127 include support-regime records with the same proof packet plus `Imported fields` and `Missing-value rule`.
- Why it may poison context:
  - The shared proof standard says to use the smallest field set needed, but each type standard repeats nearly complete provenance packets. Future agents may fill every field mechanically in produced documents, creating source/provenance chatter and placeholder fields.
  - This is especially risky because the target folder already warns to omit absent fields; repeated full templates can override that message by visual weight.
- Suggested disposition:
  - Replace full proof packets in type standards with a short type-local identity record plus a link to the proof field run, then show one compact example that omits absent fields.
  - If the full packet stays, mark it more aggressively as "maximum field set; omit every untriggered field" before the fence.

### F-07: authoring-contract and validation boilerplate is repeated across every type standard

- Severity: low
- Confidence: evidence-backed
- Location: `/Users/bardiasamiee/Documents/99.Github/Rasm/docs/standards/reference/api.md:20`, `/Users/bardiasamiee/Documents/99.Github/Rasm/docs/standards/reference/code-documentation.md:16`, `/Users/bardiasamiee/Documents/99.Github/Rasm/docs/standards/reference/readme.md:15`, `/Users/bardiasamiee/Documents/99.Github/Rasm/docs/standards/reference/reference.md:17`, `/Users/bardiasamiee/Documents/99.Github/Rasm/docs/standards/reference/support-matrix.md:15`
- Evidence:
  - Each file opens with an `AUTHORING_CONTRACT` carrying `Agent use`, `Produced structure` or `Required produced structure`, `Section cardinality`, `Adjacent checks`, and `Maintenance triggers`.
  - Each file closes with a long `VALIDATION` checklist: `api.md` lines 205-229, `code-documentation.md` lines 343-380, `readme.md` lines 422-459, `reference.md` lines 359-384, and `support-matrix.md` lines 400-427.
- Why it may poison context:
  - The pattern is locally intentional, but the same meta scaffold repeated in every type standard can be copied into produced documentation or agent prompts. It raises the odds that future docs include "agent use", "adjacent checks", and validation boilerplate where the reader needed content-specific facts.
  - The repeated route lists also make every reference type look responsible for every adjacent document, even where the real rule is "only when reader action changes."
- Suggested disposition:
  - Keep the opening contract if the corpus requires it, but tighten each instance to type-specific deltas and link shared field semantics to `information-structure.md`.
  - Consider a single shared authoring-contract pattern in the root standards with each type standard naming only its deviations and required produced sections.

### F-08: code-documentation language capsules import library-specific doctrine

- Severity: low
- Confidence: likely
- Location: `/Users/bardiasamiee/Documents/99.Github/Rasm/docs/standards/reference/code-documentation.md:145`, `/Users/bardiasamiee/Documents/99.Github/Rasm/docs/standards/reference/code-documentation.md:157`, `/Users/bardiasamiee/Documents/99.Github/Rasm/docs/standards/reference/code-documentation.md:165`, `/Users/bardiasamiee/Documents/99.Github/Rasm/docs/standards/reference/code-documentation.md:177`
- Evidence:
  - Lines 145-150 name `Fin<T>`, `Validation<Error,T>`, `Eff<RT,T>`, `IO<T>`, `Bracket`, and `K<F,T>`.
  - Line 157 names Thinktecture value objects, complex value objects, smart enums, and unions.
  - Lines 165-195 define TypeScript comment rules around `Effect<A, E, R>`, `Exit`, `Cause`, `Stream`, `Layer`, release tags, and Promise terminal boundaries.
- Why it may poison context:
  - These are useful for this repo's preferred programming style, but in a generic code-documentation standard they can make specific ecosystem libraries feel mandatory or universally canonical.
  - If future agents retrieve only this capsule, they may apply LanguageExt, Thinktecture, or Effect doctrine to projects that use different result, value-object, or effect systems.
- Suggested disposition:
  - Rename these as "project-declared effect/value-object examples" or route them to language-skill docs, keeping the generic rule as "document success, typed failure, runtime context, and resource contract for the project's effect carrier."
  - Keep concrete library names only where this repository's code-documentation standard is explicitly scoped to this repository rather than reusable standards material.

## Clean or intentionally scoped areas

- I found no literal `Rasm` string, absolute local machine path, private username path, `_reports/**` transcript, or session-role label inside the five target files.
- The files consistently route generated catalogs away instead of encouraging hand-maintained mirrors. Clear examples include `api.md` lines 26, 83, 130, 148, and 153; `code-documentation.md` lines 22 and 324; and `reference.md` line 311.
- The files generally avoid empty optional headings by explicitly saying to omit untriggered sections, for example `api.md` lines 46 and 72, `reference.md` lines 94-100, and `support-matrix.md` lines 70-86.
- Boundaries are present and line-based in all five files. That is healthy for route selection, though some route lists contribute to F-07's boilerplate risk.

## Gaps and follow-up reads

- I did not run link, anchor, Mermaid, or Markdown-render validation because this was a read-only audit except for this report, and the target standards themselves were not edited.
- I audited only `/Users/bardiasamiee/Documents/99.Github/Rasm/docs/standards/reference/*.md`. Adjacent standards may already contain the shared replacement rules that would resolve F-06 and F-07.
- The worktree already contained many dirty files, including the five target files. Findings refer to current workspace content read during this audit, not a clean main-branch baseline.

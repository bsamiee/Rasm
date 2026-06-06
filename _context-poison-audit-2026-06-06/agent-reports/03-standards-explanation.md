# Standards explanation context-poison audit

## Scope read

Read the full target folder with `nl -ba` line evidence:
- `/Users/bardiasamiee/Documents/99.Github/Rasm/docs/standards/explanation/adr.md` lines 1-358.
- `/Users/bardiasamiee/Documents/99.Github/Rasm/docs/standards/explanation/architecture.md` lines 1-438.
- `/Users/bardiasamiee/Documents/99.Github/Rasm/docs/standards/explanation/design-doc.md` lines 1-446.
- `/Users/bardiasamiee/Documents/99.Github/Rasm/docs/standards/explanation/roadmap.md` lines 1-445.
- `/Users/bardiasamiee/Documents/99.Github/Rasm/docs/standards/explanation/test-strategy.md` lines 1-454.

Also read the active standards routing and report contract: `CLAUDE.md`, root `AGENTS.md` from the provided prompt, `/Users/bardiasamiee/Documents/99.Github/Rasm/docs/standards/README.md`, `/Users/bardiasamiee/Documents/99.Github/Rasm/docs/standards/AGENTS.md`, the five shared standards, `/Users/bardiasamiee/Documents/99.Github/Rasm/docs/standards/agents-md.md`, and `/Users/bardiasamiee/Documents/99.Github/Rasm/_context-poison-audit-2026-06-06/agent-reports/README.md`.

## Method

I reviewed for context-poisoning smells only: local project coupling in generic standards and examples, version or compatibility fear, duplicated route or validation bodies, prompt/session/report leakage, source/provenance chatter, fragile exact facts, copyable examples that teach local paths or tool names, and overbroad requirements that make future docs noisier.

No target standards were edited. No docs validation gates, renderer checks, or link checks were run; this is a read-only audit plus this report write.

## Findings

### F1: Repeated sample codebase can be copied as project truth
- Severity: high
- Confidence: evidence-backed
- Location: `/Users/bardiasamiee/Documents/99.Github/Rasm/docs/standards/explanation/architecture.md:192`, `/Users/bardiasamiee/Documents/99.Github/Rasm/docs/standards/explanation/roadmap.md:141`, `/Users/bardiasamiee/Documents/99.Github/Rasm/docs/standards/explanation/design-doc.md:151`
- Evidence:
  - `architecture.md` lines 192-214 define a full pseudo tree with `<package-root>/`, `Contracts/`, `Admission/`, `Execution/`, `Storage/`, `Legacy/`, `[ACTIVE M2]`, and `[DEPRECATED]`.
  - `architecture.md` lines 274-279 reuse the same domain in Mermaid nodes: `external callback`, `<entrypoint>`, `<validator>`, `<worker> [ACTIVE M2]`, `<store>`, and `<legacy-reader> [DEPRECATED]`.
  - `roadmap.md` lines 141-145 use `# [EVENT_PIPELINE_ROADMAP]`, `src/EventPipeline/`, `Package build`, and `event system`.
  - `roadmap.md` lines 160-166 use `src/EventPipeline/EventPipeline.csproj`, `Contracts/`, `Admission/`, `Execution/`, project board `event-pipeline`, and `src/EventPipeline/_ARCHITECTURE.md`.
  - `design-doc.md` lines 151-153 use `# [FREEZE_EVENT_CONTRACT]`, `Date: 2026-06-04`, `Source: API contract design path`, and event-contract consumer language.
- Why it may poison context:
  The folder is a generic standards corpus. Reusing one concrete-looking sample system across type standards makes the examples read like a hidden canonical project, not a neutral placeholder. Future agents can copy `EventPipeline`, `Admission`, `Execution`, `M2`, issue IDs, and `.csproj` shapes into unrelated docs because the examples look complete and source-like.
- Suggested disposition:
  Collapse the sample domain into placeholders where the point is structure, or mark one short neutral sample as conceptual and avoid carrying it across multiple standards. Keep concrete paths only where the standard is explicitly teaching path placement, and prefer `<package-root>`, `<entrypoint-folder>`, `<contract-artifact>`, and `<live-planner-route>` over named sample products.

### F2: Generic test strategy leaks a C#/.NET toolchain catalog
- Severity: high
- Confidence: evidence-backed
- Location: `/Users/bardiasamiee/Documents/99.Github/Rasm/docs/standards/explanation/test-strategy.md:218`
- Evidence:
  - `test-strategy.md` lines 218-230 say produced strategies should replace rows with local gates, but the table still names `xUnit/MTP`, `CsCheck`, `coverlet`, `Stryker`, `Verify`, `ArchUnitNET`, `SharpFuzz`, `BenchmarkDotNet`, and `bridge runtime`.
  - `test-strategy.md` lines 91-93 correctly say repository truth carries gate names, commands, runners, status-check identifiers, artifacts, repair paths, and release policy, which conflicts with publishing a tool-name catalog in the generic standard.
- Why it may poison context:
  The table turns one ecosystem's likely tooling into the default mental model for all test strategies. Agents writing generic or non-C# strategies may preserve these names as expected rails, invent unsupported gates, or route proof to tools that do not exist in the target scope.
- Suggested disposition:
  Replace the `Typical local source` cells with tool-neutral carrier names such as `<unit-test-runner>`, `<mutation-tool>`, `<snapshot-artifact>`, `<architecture-gate>`, and `<host-runtime-scenario>`. Put concrete C# tool mappings in a repository-specific test strategy or test-tool reference where current manifests and commands prove them.

### F3: Compatibility examples normalize stale-path preservation
- Severity: medium
- Confidence: evidence-backed
- Location: `/Users/bardiasamiee/Documents/99.Github/Rasm/docs/standards/explanation/architecture.md:123`, `/Users/bardiasamiee/Documents/99.Github/Rasm/docs/standards/explanation/roadmap.md:11`
- Evidence:
  - `architecture.md` lines 121-125 include `Excluded: legacy export retirement after the compatibility window; support matrix carries timing`.
  - `architecture.md` lines 180-186 allow `support-controlled legacy path`, planned path state, `[DEPRECATED]`, `migration`, and `compatibility rule` language inside the architecture codemap rules.
  - `architecture.md` lines 212-213 show a `Legacy/` path and `<legacy-reader> [DEPRECATED] migration reads only`.
  - `roadmap.md` lines 9-16 include sequencing a `migration`, `removal`, and `compatibility window`.
  - `roadmap.md` lines 160-166 repeat `legacy export retirement after the compatibility support row ends`, and lines 397-399 define a deferred work record around a compatibility window.
- Why it may poison context:
  The repo-level policy rejects compatibility prose unless an owner route and current proof justify it. These examples are bounded by support-matrix language, but their volume makes legacy/compatibility paths feel like normal architecture and roadmap content. Agents may keep stale paths as status overlays instead of deleting or routing them.
- Suggested disposition:
  Keep one small support-controlled retirement example if the distinction is necessary, and remove the repeated `Legacy/`, migration, and compatibility-window examples from codemaps and milestone records. Make the positive pattern "delete or route to support matrix when support proof exists" rather than "carry a deprecated path marker."

### F4: Route maps are repeated enough to become drift-prone boilerplate
- Severity: medium
- Confidence: evidence-backed
- Location: `/Users/bardiasamiee/Documents/99.Github/Rasm/docs/standards/explanation/architecture.md:17`, `/Users/bardiasamiee/Documents/99.Github/Rasm/docs/standards/explanation/design-doc.md:16`, `/Users/bardiasamiee/Documents/99.Github/Rasm/docs/standards/explanation/roadmap.md:16`, `/Users/bardiasamiee/Documents/99.Github/Rasm/docs/standards/explanation/test-strategy.md:24`, `/Users/bardiasamiee/Documents/99.Github/Rasm/docs/standards/explanation/adr.md:21`
- Evidence:
  - `architecture.md` line 17 gives a route-away sentence, line 23 gives adjacent checks, lines 396-410 list boundary routes, and lines 417-421 validate adjacent routes again.
  - `design-doc.md` line 16 gives route-away, line 22 gives adjacent checks, lines 402-416 list boundary routes, and line 446 repeats handoff field requirements.
  - `roadmap.md` line 16 gives route-away, line 22 gives adjacent checks, lines 401-414 list boundary routes, and lines 435-445 validate dependencies, handoffs, and closure.
  - `test-strategy.md` lines 18 and 24 route command and adjacent strategy content, lines 410-423 list boundary routes, and line 454 adds a boundary-link count rule.
  - `adr.md` line 21 lists adjacent checks, and lines 324-337 repeat explanation, reference, and task routes.
- Why it may poison context:
  Repeated route catalogs teach agents to paste neighboring-standard inventories into every produced artifact. That increases noise, creates multiple places for route facts to drift, and can make future docs preserve background links that do not change reader action.
- Suggested disposition:
  Keep one high-salience route-away rule per type standard and one compact `Boundaries` section. Move repeated validation wording to a single generic check such as "adjacent links appear only when they change reader action, proof, status, validation, or maintenance." Avoid copying full neighbor lists into authoring contract, boundaries, and validation simultaneously.

### F5: Exact dates, issue IDs, and milestone IDs make examples look executed
- Severity: medium
- Confidence: evidence-backed
- Location: `/Users/bardiasamiee/Documents/99.Github/Rasm/docs/standards/explanation/adr.md:52`, `/Users/bardiasamiee/Documents/99.Github/Rasm/docs/standards/explanation/roadmap.md:173`, `/Users/bardiasamiee/Documents/99.Github/Rasm/docs/standards/explanation/design-doc.md:312`
- Evidence:
  - `adr.md` lines 52-55 include table rows `0001`, `0007`, `2026-01-12`, `2026-03-04`, and `0023`.
  - `adr.md` lines 168-177 repeat `Date: 2026-01-12`, concrete-looking decision source, and generated event contract proof.
  - `roadmap.md` lines 173-176 include `issue/101`, `issue/112`, `M1`, and `M2` in a current-status snapshot.
  - `roadmap.md` lines 280-301 include a complete `M2_WORKER_EXECUTION` record with checked exit criteria, `EventPipeline.csproj`, `issue/112`, and build-output proof wording.
  - `design-doc.md` lines 310-313 include a final-check table with `2026-06-01`.
- Why it may poison context:
  Exact dates and issue IDs are allowed in real docs, but in standards examples they look like executed proof or current source facts. Agents can accidentally preserve them as real evidence or imitate proof claims without running the matching checks.
- Suggested disposition:
  Use placeholders for dates, issue IDs, and proof receipts in generic examples unless the exact value is the subject of the rule. If realism is required, mark examples as `conceptual` and avoid checked boxes or proof phrasing that implies a gate actually passed.

### F6: Broad mandatory cross-cutting records can generate `n/a` clutter
- Severity: low
- Confidence: likely
- Location: `/Users/bardiasamiee/Documents/99.Github/Rasm/docs/standards/explanation/design-doc.md:171`, `/Users/bardiasamiee/Documents/99.Github/Rasm/docs/standards/explanation/test-strategy.md:192`
- Evidence:
  - `design-doc.md` line 171 requires `Cross-cutting implications` to cover security, privacy, accessibility, internationalization, data, operational, compatibility, and runtime concerns as records for `Standard` and `Public-contract` designs.
  - `design-doc.md` lines 243-246 define `Applies: yes | no`, `Proof route`, and `Decision`, explicitly allowing `n/a reason`.
  - `test-strategy.md` line 192 says to name at least the High and Extreme risks currently local and back-link them to levels or gates.
- Why it may poison context:
  These rules can force broad concern inventories even when only one or two concerns change the proposal or test strategy. That invites filler `n/a` records and makes future docs noisier.
- Suggested disposition:
  Gate broad concern records on a named trigger: include only concerns whose source, proof route, risk, or reader action changes. Keep one sentence requiring authors to state omitted concern classes only when omission would be surprising or safety-relevant.

## Clean or intentionally scoped areas

- No direct Rasm path, `/Users/...` machine path, `_TMP`, `_reports`, session transcript, chat prompt, or report narration appeared in the five target standards.
- The files generally separate durable document types from task prompts and route generated contracts, command procedures, and operational recovery away from explanation pages.
- The final sections are consistently `VALIDATION`; I did not find colon-led fence labels or obvious prompt-era labels such as `dictum`, `dossier`, or `stage N`.
- Proof fields are often heavy, but most occurrences are attached to drift-prone examples or templates rather than page-level source chatter. The poison risk is mainly copied exact values, not the existence of proof fields.

## Gaps and follow-up reads

- I did not run link, anchor, Mermaid, Markdown render, or docs-build validation.
- I did not read adjacent `reference`, `task`, or `learning` type standards, so cross-folder duplication beyond the explanation family is out of scope.
- The five explanation files were already modified in the worktree before this report was written; line evidence reflects the current working-tree content read with `nl -ba`.

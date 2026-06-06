# Code comments and tool/operator docs context-poison audit

## Scope read

Audited repository prose/comment surfaces outside `docs/` and `.claude/`, focused on `tools/**`, `libs/**`, `apps/**`, `tests/**`, and `scripts` if present. There is no `scripts/` directory in this checkout. Generated, vendored, cache, artifact, `bin/`, and `obj/` paths were excluded from searches.

Primary inspected surfaces:

- `/Users/bardiasamiee/Documents/99.Github/Rasm/tools/assay/README.md`
- `/Users/bardiasamiee/Documents/99.Github/Rasm/tools/quality/README.md`
- `/Users/bardiasamiee/Documents/99.Github/Rasm/tools/rhino-bridge/README.md`
- `/Users/bardiasamiee/Documents/99.Github/Rasm/tools/assay/AGENTS.md`
- `/Users/bardiasamiee/Documents/99.Github/Rasm/tools/rhino-bridge/AGENTS.md`
- `/Users/bardiasamiee/Documents/99.Github/Rasm/libs/csharp/Rasm.AppHost/{README.md,_ARCHITECTURE.md,ROADMAP.md}`
- `/Users/bardiasamiee/Documents/99.Github/Rasm/libs/csharp/Rasm.AppUi/{README.md,_ARCHITECTURE.md,ROADMAP.md}`
- `/Users/bardiasamiee/Documents/99.Github/Rasm/libs/csharp/Rasm.Compute/{README.md,_ARCHITECTURE.md,ROADMAP.md}`
- `/Users/bardiasamiee/Documents/99.Github/Rasm/libs/csharp/Rasm.Persistence/{README.md,_ARCHITECTURE.md,ROADMAP.md}`
- `/Users/bardiasamiee/Documents/99.Github/Rasm/libs/csharp/Rasm/Vectors/_ARCHITECTURE.md`
- `/Users/bardiasamiee/Documents/99.Github/Rasm/apps/README.md`
- Representative C#, Python, and scenario comment/docstring clusters in `tools/**`, `libs/**`, `apps/**`, and `tests/**`

Inventory searched: 290 scoped source/prose files and 34 scoped Markdown/instruction files.

## Method

- Enumerated scoped source/prose files with `fd`, excluding `docs/**`, `.claude/**`, generated output, dependency, cache, and artifact paths.
- Searched candidate text with `rg` for: `legacy`, `compat`, `deprecated`, `obsolete`, `stale`, `temporary`, `prompt`, `agent`, `assistant`, `validation`, `source of truth`, `generated from`, `provenance`, `current truth`, `caveat`, `migration`, `route-away`, `latest`, `version`, `fallback`, `shim`, `wrapper`, `bedrock`, `xfail strict`, `BRIDGE-DEFERRED`, `_TMP`, and local path/project examples.
- Deep-read representative high-hit docs before classifying findings.
- Sampled source comments/docstrings from tool code, analyzer code, C# libraries, scenarios, and tests to avoid reporting search-result noise.
- Did not edit any audited file; only this report was written.

## Findings

### L-01: Assay README carries transitional adoption state as durable operator truth
- Severity: high
- Confidence: evidence-backed
- Location: `/Users/bardiasamiee/Documents/99.Github/Rasm/tools/assay/README.md:3`
- Evidence:
  - Line 3 says Assay is intended to replace `tools.quality`, but root policy still names `tools.quality` until migration.
  - Lines 7-16 repeat the same adoption state as status, replacement, current policy, use-now, update-when, and route-away rows.
  - Lines 18-30 give migration guidance from `tools.quality`, including command replacement intent, changed semantics, and new capability buckets.
  - Line 218 preserves a migration note that the payload locations replace the old `tools.quality` `data` convention.
- Why it may poison context: It teaches future agents to reason from an in-between migration state rather than from the current root owner route. The repeated "intended successor / not root-canonical" language also invites compatibility-preservation behavior even though the repo's root policy rejects stale aliases and compatibility padding.
- Suggested disposition: Collapse this into one short status fact or remove it once root routing is settled. Keep command behavior in the command table; route migration policy to the root quality owner rather than this operator README.

### L-02: Assay README embeds source/proof boilerplate and task-process caveats in the operator manual
- Severity: medium
- Confidence: evidence-backed
- Location: `/Users/bardiasamiee/Documents/99.Github/Rasm/tools/assay/README.md:300`
- Evidence:
  - Lines 302-348 repeatedly use `Current truth`, `Caveat`, and `Reader action` records for artifacts, history, run scopes, logs, tracing, environment, remote execution, and fsspec.
  - Line 302 frames observability and remote execution around "how agents consume proof."
  - Line 337 says durable requirements must come from source/settings, not "task System Information."
  - Lines 350-372 contain a source-owner table plus maintainer-flow instructions for surgical additions.
  - Lines 374-383 include README validation commands and a blocker-recording instruction.
- Why it may poison context: The source/proof schema is useful during a rewrite pass, but in a tool manual it creates a second documentation standard outside `docs/standards`, reinforces task-process vocabulary, and may cause agents to preserve source/provenance chatter instead of replacing it with direct operator facts.
- Suggested disposition: Keep only operator-facing contract text: stdout/stderr, artifact roots, remote limits, and command behavior. Move proof/source-owner grammar to `AGENTS.md` or docs standards if still needed, and remove task-process references such as "task System Information" and "record the blocker."

### L-03: `tools.quality` README is both tool reference and agent instruction surface
- Severity: medium
- Confidence: evidence-backed
- Location: `/Users/bardiasamiee/Documents/99.Github/Rasm/tools/quality/README.md:3`
- Evidence:
  - Lines 3-4 call `tools.quality` an "agent-only CLI" and tell readers to report command, exit, and evidence path.
  - Line 13 includes an output-contract warning plus the non-durable phrase "no World-A/World-B duality."
  - Lines 262-283 contain an `AGENT_ROUTING` section with use/avoid guidance.
  - Line 286 tells readers to load `.claude/skills/coding-python/SKILL.md` before Python edits.
  - Lines 295-313 provide README/Mermaid validation and dependency-currency commands.
- Why it may poison context: This mixes operator reference, agent runtime posture, `.claude` skill loading, and validation boilerplate in one README. Future agents can treat the README as standing instruction authority even when `CLAUDE.md`, root `AGENTS.md`, and nested overlays should own routing and skill behavior.
- Suggested disposition: Rename or shrink `AGENT_ROUTING` into an operator "rail selection" reference if needed. Move skill-loading and validation ladders to the instruction overlay or root quality route. Remove session-era phrases such as "World-A/World-B duality."

### L-04: Rhino bridge README preserves agent guidance, legacy wire migration, and local WIP evidence
- Severity: high
- Confidence: evidence-backed
- Location: `/Users/bardiasamiee/Documents/99.Github/Rasm/tools/rhino-bridge/README.md:3`
- Evidence:
  - Lines 3-7 instruct "coding agents" how to use the bridge and warn not to treat it as a unit-test framework.
  - Line 18 says the bridge is for health checks "when agents need Rhino runtime facts before editing code."
  - Line 32 labels the actor in the architecture diagram as `Coding Agent`.
  - Lines 203-209 describe scripts returning "structured agent evidence."
  - Lines 235-237 include `Agent guidance` plus a `Wire format migration` section that tells agents not to parse legacy `key=value` lines.
  - Lines 241-248 name current local evidence and XML availability.
  - Lines 256-257 warn not to document a command form until another compile-reference projection exists.
- Why it may poison context: The README is a runtime operator reference, but it also acts like an agent behavior memo, migration note, and local evidence transcript. The legacy migration section is especially risky because it keeps removed wire behavior alive in retrieval context.
- Suggested disposition: Keep the marker contract, output fields, and scenario path rules. Delete or fold the `Agent guidance` and `Wire format migration` paragraphs into a terse current-contract statement. Move bridge-use policy to `tools/rhino-bridge/AGENTS.md`; move transient WIP/XML evidence to the API/source owner or a dated report.

### L-05: AppUi docs carry host-specific investigation transcripts and future-gate instructions
- Severity: medium
- Confidence: evidence-backed
- Location: `/Users/bardiasamiee/Documents/99.Github/Rasm/libs/csharp/Rasm.AppUi/_ARCHITECTURE.md:54`
- Evidence:
  - Lines 54-65 list mandatory embedding rules and a long `[DEFERRED]` GH2-Avalonia embedding paragraph, including decompiled `Grasshopper2.dll`, absent APIs, canvas NSView reachability, and a command trigger for each WIP drop.
  - Line 112 has a `SETTLED` versus `STILL-OPEN` SkiaSharp native gate paragraph with local RhinoWIP evidence and exact shell/bridge commands.
  - Line 258 lists evidence categories, including deferred GH2 coexistence.
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/libs/csharp/Rasm.AppUi/ROADMAP.md:75` says unsettled items need "an agent with the macOS host."
  - Roadmap lines 77-107 preserve already-confirmed facts, restore gates, Rhino native gates, runtime proofs, and a future WIP watch command.
- Why it may poison context: These sections read like a research closeout and gate ledger rather than durable architecture. The "current WIP" and "per WIP drop" language may become stale quickly, while "needs an agent" couples the roadmap to local execution process.
- Suggested disposition: Split durable constraints from evidence chronology. Keep only stable host embedding rules and current supported/unsupported decisions in architecture. Move command gates and dated confirmations to a runbook or report with freshness markers.

### L-06: Forward-looking package docs include version-scared caveats and `.claude` source anchors
- Severity: medium
- Confidence: likely
- Location: `/Users/bardiasamiee/Documents/99.Github/Rasm/libs/csharp/Rasm.Compute/_ARCHITECTURE.md:180`
- Evidence:
  - Compute lines 180-190 warn about "current CoreML EP API," deprecated ORT 1.20 P/Invoke, Rhino crash-prone `RunAsync`, native dylib probing, and CoreML ANE downcast validation.
  - Persistence line 35 references `.claude/skills/coding-csharp` examples as guidance not to apply.
  - Persistence lines 37-46 use "newest viable," "no version pins," `DO NOT ADD`, deprecated SQLCipher, paid SEE/SQLite3MC, and deferred encryption as durable architecture prose.
  - Persistence lines 165-178 include `SOURCE_ANCHORS` to `.claude/skills` plus external package docs.
- Why it may poison context: Some of these are legitimate constraints, but the docs mix stable architecture, version-sensitive package guidance, rejected implementation options, and `.claude` skill provenance. That can train agents to preserve old-package fear and prompt-source anchors instead of verifying current manifests and upstream docs.
- Suggested disposition: Keep stable owner decisions and failure cases. Replace time-sensitive API/package claims with route-to-proof rules or dated evidence. Remove `.claude` skill paths from reusable architecture documents unless they are intentionally active repo instruction sources.

### L-07: Test modules embed audit metadata and placeholders as docstrings
- Severity: medium
- Confidence: evidence-backed
- Location: `/Users/bardiasamiee/Documents/99.Github/Rasm/tests/tools/assay/core/test_engine.py:1`
- Evidence:
  - `test_engine.py` lines 1-8 contain a module docstring with `Source surface` and `Laws` metadata.
  - `test_model.py` lines 1-7 use the same `Source surface` / `Laws` pattern.
  - `test_rail_package.py` lines 1-8 list P1/P2 `xfail strict` details, then lines 13-14 skip a `bedrock: coverage pending` placeholder.
  - `tests/tools/assay/conftest.py` lines 269-273 mention "foundation/W3 law" and a future field escaping the resolver.
  - `tests/tools/assay/conftest.py` lines 922-931 document a methodology hole and session-scoped autouse strategy validation.
- Why it may poison context: This metadata is useful while building a law matrix, but it is easy for context retrieval to treat it as current proof inventory or source-truth routing. `bedrock`, `W3`, `xfail strict`, and methodology-hole language are especially process-local and can make incomplete tests look intentionally accepted.
- Suggested disposition: Keep test names and assertions as executable truth. Convert module docstrings to shorter functional test intent or remove them where they only restate file ownership. Replace `bedrock` skips with explicit TODO issue references, xfail markers with conditions, or implemented tests.

### L-08: `BRIDGE-DEFERRED` comments are repeated static-test posture that can fossilize gaps
- Severity: low
- Confidence: evidence-backed
- Location: `/Users/bardiasamiee/Documents/99.Github/Rasm/tests/csharp/libs/Rasm/Analysis/Bounds.spec.cs:9`
- Evidence:
  - `Bounds.spec.cs` line 9: native bounds evaluation is `BRIDGE-DEFERRED`; static owns catalogs/factories/dispatch.
  - `Geometry.spec.cs` line 8: native `GeometryKernel`/coercion/closest is `BRIDGE-DEFERRED`; static owns catalogs and construction.
  - `Cloud.spec.cs` line 11: native ring/polyline metrics are `BRIDGE-DEFERRED`; static owns metadata/projection/mass/receipts.
  - Search showed the same pattern across many `tests/csharp/libs/Rasm/Analysis/*.spec.cs`, `Domain/*.spec.cs`, and `Vectors/*.spec.cs` files.
- Why it may poison context: The comments correctly classify proof boundaries, but the repeated all-caps token can become a standing excuse not to promote runtime scenarios after bridge proof lands. It also acts like a parallel status system outside the scenario owner.
- Suggested disposition: Keep the distinction only where it changes test behavior. Prefer a shared test overlay rule plus narrow per-file comments for actual gaps. When runtime proof lands, remove stale `BRIDGE-DEFERRED` comments in the same change.

### L-09: Local project-coupled examples appear in reusable layout/tool docs
- Severity: low
- Confidence: evidence-backed
- Location: `/Users/bardiasamiee/Documents/99.Github/Rasm/apps/README.md:13`
- Evidence:
  - `apps/README.md` lines 13-16 use `apps/grasshopper/Radyab/` as the example in the top-level app layout table.
  - `apps/README.md` lines 41-49 call `apps/grasshopper/Radyab/Radyab.csproj` the live exemplar and show its exact folder layout.
  - `tools/assay/README.md` lines 141-151 use `tests/csharp/libs/Rasm.Rhino` and `rasm-bridge` as command examples.
- Why it may poison context: In project-specific docs these examples are not automatically wrong, but they can couple reusable instructions to one local plugin/tool slug. Future agents may copy Radyab or `rasm-bridge` paths into new plugin/package work rather than treating them as examples.
- Suggested disposition: Keep Radyab only if `apps/README.md` is intentionally a repo-specific app convention guide. For generic command examples, replace concrete slugs/paths with placeholders or explicitly label them as the current exemplar.

### L-10: Source comments are mostly clean, but a few retain implementation-provenance chatter
- Severity: note
- Confidence: evidence-backed
- Location: `/Users/bardiasamiee/Documents/99.Github/Rasm/tools/assay/composition/registry.py:564`
- Evidence:
  - `registry.py` lines 564-569 explain probe-cache token provenance, lockfile mtimes, launcher tokens, and stale hits.
  - `registry.py` line 613 says parse-error cache folds to live spawn rather than a "stale lie."
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/tools/quality/rails/api.py:658` explains regex anchoring by contrasting `Weld`, `Unweld`, and XML doc comments.
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/libs/csharp/Rasm.Grasshopper/UI/Motion.cs:2169` explains why a lambda rereads mutable `pendingDt` instead of using a method group.
- Why it may poison context: Most sampled source comments explain non-obvious invariants and should remain. The only risk is wording such as "stale lie" or lengthy provenance of past defects, which can drift toward narrative rather than invariant.
- Suggested disposition: Leave invariant comments in place unless a cleanup pass is already touching the file. If edited, phrase them as current behavior/invariant, not history.

## Clean or intentionally scoped areas

- Most C# production source comments sampled in `libs/csharp/Rasm.*` explain non-obvious runtime or mathematical invariants rather than agent behavior.
- `tools/assay/AGENTS.md` and `tools/rhino-bridge/AGENTS.md` are instruction overlays by design. They contain strong "No ..." rules, but those are in the intended instruction surface rather than accidental prose.
- Analyzer release files under `tools/cs-analyzer` are structured rule inventories; search hits on `Validation`, `Generated`, `Receipt`, and `Proof` were mostly rule names, not prose poison.
- Scenario comments sampled under `tests/csharp/libs/**/scenarios` were generally runtime-law labels or API-specific context, not prompt/source chatter.

## Gaps and follow-up reads

- Did not read all 290 scoped files line by line. The audit used broad search plus deep reads of the highest-risk docs and representative code/comment clusters.
- Did not audit `docs/**` or `.claude/**` by request.
- Did not inspect generated `obj/`, `.artifacts/`, `.cache/`, `.venv/`, or `node_modules/` text.
- Existing dirty worktree state included modified tool README/AGENTS files before this report was written; findings describe the current file contents read during this audit, not authorship.
- A second-wave cleanup should decide whether nested `AGENTS.md` files themselves are in remediation scope or only source/operator docs.

# root/shared docs standards context-poison audit

## Scope read

- `/Users/bardiasamiee/Documents/99.Github/Rasm/CLAUDE.md`
- `/Users/bardiasamiee/Documents/99.Github/Rasm/docs/standards/README.md`
- `/Users/bardiasamiee/Documents/99.Github/Rasm/docs/standards/AGENTS.md`
- `/Users/bardiasamiee/Documents/99.Github/Rasm/docs/standards/agents-md.md`
- `/Users/bardiasamiee/Documents/99.Github/Rasm/docs/standards/agentic-documentation.md`
- `/Users/bardiasamiee/Documents/99.Github/Rasm/docs/standards/information-structure.md`
- `/Users/bardiasamiee/Documents/99.Github/Rasm/docs/standards/style-guide.md`
- `/Users/bardiasamiee/Documents/99.Github/Rasm/docs/standards/proof.md`
- `/Users/bardiasamiee/Documents/99.Github/Rasm/docs/standards/formatting.md`
- `/Users/bardiasamiee/Documents/99.Github/Rasm/docs/standards/_reports/AGENTS.md`
- `/Users/bardiasamiee/Documents/99.Github/Rasm/_context-poison-audit-2026-06-06/agent-reports/README.md` for the required report shape.

I did not read `docs/standards/_reports/**` beyond `docs/standards/_reports/AGENTS.md`.

## Method

I read the active root and shared standards with `nl -ba`, then searched the scoped files for context-poison smells: provider claims, project-coupled wording, repeated read-order and validation machinery, source/provenance chatter, process labels, report-frame leakage, compatibility or baseline wording, local examples, and duplicated instruction bodies. Findings below use the 5 shared axes required by `docs/standards/AGENTS.md`: position, form, craft, evidence, and notation.

## Findings

### F01: provider-specific prompt rules are durable without adjacent proof
- Severity: high
- Confidence: evidence-backed
- Location: `/Users/bardiasamiee/Documents/99.Github/Rasm/docs/standards/agentic-documentation.md:208`
- Evidence:
  - Lines 208-246 define provider-specific sections for `OPENAI_CODEX`, `CLAUDE_CODE`, and `GEMINI`.
  - Lines 214-220 state Codex defaults for structure, delimiters, long context, structured output, state, caching, and output control.
  - Lines 224-231 state Claude Code defaults, including an `AGENTS.md` loading claim at line 230.
  - Lines 235-240 state Gemini defaults.
  - Lines 244-246 correctly warn that provider-specific rules are local defaults and need current proof, but the provider sections themselves carry no `Evidence:`, `Last verified:`, or source route beside the claims.
- Why it may poison context:
  - The section gives future agents a compact provider manual inside the standards corpus. Because the claims can drift and are not claim-level proved, future prompts or standards can copy stale provider behavior as durable truth.
  - It also encourages provider-name coupling in otherwise portable prompt and documentation standards.
- Suggested disposition:
  - Collapse provider sections to a provider-agnostic contract: outcome, constraints, evidence boundary, output shape, and stop rule.
  - If provider defaults remain, move each provider claim into a proof record with maintained-source or local-output evidence and a freshness trigger, or route the provider-specific details to a separate maintained provider reference.

### F02: `agents-md.md` imports repo coding doctrine into a generic instruction-file standard
- Severity: high
- Confidence: evidence-backed
- Location: `/Users/bardiasamiee/Documents/99.Github/Rasm/docs/standards/agents-md.md:129`
- Evidence:
  - Lines 131-135 prescribe trigger-owner-action wording for produced overlays.
  - Lines 137-143 define an `OWNER_RAIL_CHOOSER` with C#, TypeScript, Python, Bash, SQL, and tests/bridge rails.
  - Lines 145-146 require dependency, backend, runtime, host API, storage, provider, and tool integrations to bind into local owner rails before exposing public surfaces.
  - Line 149 uses host-facing assertion and host-runtime scenario proof examples.
- Why it may poison context:
  - This is an `AGENTS.md` authoring standard, but it teaches language-specific Rasm implementation doctrine as if every produced overlay should know C# `Fin`/`Validation`/`Eff`, TypeScript Effect layers, Python `msgspec`, Rhino-style host proof, and bridge scenario rails.
  - Those rules are already owned by `CLAUDE.md`, language skills, and local subtree overlays. Keeping them here makes future generic instruction files more likely to carry project-only rail names and overfit unrelated folders.
- Suggested disposition:
  - Replace the language rail chooser with a generic rule: name the local canonical owner and extension action only when the folder has one.
  - Route language-specific rail examples to the language skills, `CLAUDE.md`, or local code/test overlays. Keep `agents-md.md` focused on overlay shape, local delta, route-away behavior, and proof gaps.

### F03: anonymous `project-declared implementation-proof owner` wording is repeated without a route target
- Severity: medium
- Confidence: evidence-backed
- Location: `/Users/bardiasamiee/Documents/99.Github/Rasm/docs/standards/README.md:28`
- Evidence:
  - Line 28 says the project-declared implementation-proof owner applies to cross-stack owner precedence, implementation proof, host-library routing, and command/tooling claims outside the standards corpus.
  - Line 205 repeats that the project-declared implementation-proof owner carries cross-stack implementation precedence and proof order outside the standards library.
  - Line 227 repeats the same concept in validation.
- Why it may poison context:
  - The phrase is a local routing abstraction without a concrete target in the README. Future standards or reports can copy the phrase instead of naming the active owner file, which turns proof routing into provenance chatter.
  - It also couples a root/shared documentation standard to a project-specific proof ladder while keeping the target implicit.
- Suggested disposition:
  - Name the concrete owner route where this repository has one, or rewrite the rule as a generic fallback: use the active repo instruction route for implementation proof outside the standards corpus.
  - Keep this concept in one owner only, preferably `proof.md` or the root repo instruction router, and remove the duplicate validation restatement.

### F04: validation sections risk becoming boilerplate artifacts instead of proof selectors
- Severity: medium
- Confidence: evidence-backed
- Location: `/Users/bardiasamiee/Documents/99.Github/Rasm/docs/standards/information-structure.md:413`
- Evidence:
  - Lines 413-417 require `Validation` in the standard file anatomy and say every long standard needs a validation section.
  - `README.md` has a validation checklist at lines 208-231.
  - `AGENTS.md` has a validation checklist at lines 113-135.
  - `agentic-documentation.md` has a validation checklist at lines 259-288.
  - `information-structure.md` has a validation checklist at lines 471-503.
  - `style-guide.md` has a validation checklist at lines 191-214.
  - `proof.md` has a validation checklist at lines 294-316.
  - `formatting.md` has a validation checklist at lines 264-284.
- Why it may poison context:
  - The standards repeatedly close with broad self-check lists. This makes future standards, prompts, and reports more likely to append generic validation boilerplate even when the actual proof selector is a command, source route, renderer proof, or explicit proof gap.
  - It also competes with `proof.md` line 152, which already defines how a final validation line should select configured gates or state a proof gap.
- Suggested disposition:
  - Keep proof gate selection in `proof.md`; shorten per-standard validation sections to only checks unique to that owner.
  - For standards where validation is just a restated checklist, replace it with a compact proof-route close or owner-specific review checklist.

### F05: read-order rules duplicate global/root behavior and can inflate audit context
- Severity: medium
- Confidence: evidence-backed
- Location: `/Users/bardiasamiee/Documents/99.Github/Rasm/docs/standards/AGENTS.md:13`
- Evidence:
  - Lines 13-24 require the active instruction chain, README, full target file, routed owner standard, linked adjacent standards, affected folder or type family, and mode-specific extra reads before changes.
  - `README.md` lines 33-41 also define a standards-library read order.
  - The root instruction file already routes docs edits through `docs/standards/README.md` and, for standards edits, through `docs/standards/AGENTS.md`.
- Why it may poison context:
  - The repeated read-order ladder can make every standards task behave like a full-corpus audit. That increases the chance that unrelated standards, adjacent examples, or report mechanics leak into a narrow edit or prompt.
  - It also duplicates the general root rule to read governing files before edits, so the local overlay is carrying more generic workflow than local delta.
- Suggested disposition:
  - Keep only the local delta: which standards-family files are required for root audits, type edits, named `_reports/**`, and cross-stack claims.
  - Route the generic "read full target and owner first" behavior to root instructions and `README.md`.

### F06: proof/evaluation receipt machinery is strong but overbroad for ordinary docs
- Severity: medium
- Confidence: evidence-backed
- Location: `/Users/bardiasamiee/Documents/99.Github/Rasm/docs/standards/proof.md:197`
- Evidence:
  - Lines 197-246 define agent-surface evaluation receipts.
  - Lines 214-219 add stochastic rigor fields including 20-50 questions, 3-5 trials, Wilson score 95% confidence intervals, traces, latency, and tool errors.
  - Lines 235-244 provide a sample trace with model or provider version, tool set, token budget, p50 latency, and tool errors.
- Why it may poison context:
  - The rigor fields are useful for real machine-facing evaluations, but their placement in the shared proof standard can cause ordinary docs, prompts, and standards reports to overproduce evaluation metadata.
  - The sample values can also become copied pseudo-proof when no actual evaluation ran.
- Suggested disposition:
  - Keep a minimal deterministic receipt in shared `proof.md`.
  - Move stochastic evaluation rigor into a named optional subsection or agent-surface evaluation profile with a strict trigger: only when the document claims retrieval quality, ranking, tool selection, latency, or provider behavior.

### F07: `_reports/AGENTS.md` is well-scoped but process-heavy enough to leak if promoted
- Severity: low
- Confidence: evidence-backed
- Location: `/Users/bardiasamiee/Documents/99.Github/Rasm/docs/standards/_reports/AGENTS.md:64`
- Evidence:
  - Lines 64-80 define report types and reject worker-role, wave, confidence, and implementation-phase report types.
  - Lines 82-105 define a report shape and reject top-level `TRANSCRIPT`, `READ_TRANSCRIPT`, `CONFIDENCE`, `VALIDATION`, `NO_CHANGE_CONFIRMATIONS`, `DRAFT_EDIT_MAP`, worker-role headings, and task-history sections.
  - Lines 155-160 reject turning `_reports/` into a second standards corpus, provider manual, validation ledger, transcript store, or instruction source.
- Why it may poison context:
  - The file correctly rejects process leakage, but it still contains dense report mechanics, status vocabularies, lane language, merge keys, and archive operations. If agents promote from this file instead of only reading it as a leaf overlay, active standards can inherit report-process scaffolding.
- Suggested disposition:
  - Keep the file as source-material mechanics, but add a sharper lead or boundary sentence saying its report-shape vocabulary must not be copied into active standards or non-report artifacts.
  - When promoting findings, route only durable rules, source routes, proof gaps, corrections, and candidate wording.

### F08: report-session naming examples are local and date-coded
- Severity: low
- Confidence: evidence-backed
- Location: `/Users/bardiasamiee/Documents/99.Github/Rasm/docs/standards/_reports/AGENTS.md:21`
- Evidence:
  - Line 21 gives "Good examples" such as `agents-md-050626`, `code-documentation-050626`, `proof-060626`, and `external-libs-060626`.
  - Lines 21-29 define date-coded session and track naming mechanics.
- Why it may poison context:
  - The examples are appropriate for this `_reports/` leaf, but they are not universal documentation examples. If copied into active docs standards, they would normalize local date-coded archive names and standards-file slugs as reusable artifact patterns.
- Suggested disposition:
  - Keep the examples only in `_reports/AGENTS.md`.
  - If similar guidance is needed in active standards, use neutral placeholders instead of current standards filenames and dates.

## Clean or intentionally scoped areas

- I found no literal `/Users/...` machine paths or `Rasm` project names inside the active root/shared standards source files I audited.
- `README.md` and `AGENTS.md` consistently exclude `_reports/**` from the active standards corpus unless the task explicitly names it.
- `style-guide.md` has strong anti-noise rules at lines 43-48 and 153-165 that directly counter prompt-era process vocabulary and low-value examples.
- `formatting.md` cleanly separates invocation markers, alerts, table styling, group labels, and status tokens; that reduces marker-family confusion.
- `proof.md` has the right core evidence hierarchy and proof-field labels. The issue is mostly scope and placement of advanced evaluation machinery, not the evidence model itself.

## Gaps and follow-up reads

- I did not read `docs/standards/_reports/**` report bodies by instruction.
- I did not verify current provider documentation for Codex, Claude Code, or Gemini. Provider freshness is identified as a proof gap in the standards, not resolved here.
- I did not run link, anchor, renderer, or docs-build validation because this was a read-only audit plus one requested report file.
- Follow-up should map each finding to a single owner before editing: provider defaults to `agentic-documentation.md` plus `proof.md`, implementation-rail leakage to `agents-md.md`, validation boilerplate to `information-structure.md` and `proof.md`, and report leakage to `_reports/AGENTS.md`.

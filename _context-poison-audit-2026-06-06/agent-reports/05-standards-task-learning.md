# Standards task and learning context-poison audit

## Scope read

Read the active instruction and standards context before auditing:
- `/Users/bardiasamiee/Documents/99.Github/Rasm/CLAUDE.md`
- `/Users/bardiasamiee/Documents/99.Github/Rasm/AGENTS.md`
- `/Users/bardiasamiee/Documents/99.Github/Rasm/docs/standards/README.md`
- `/Users/bardiasamiee/Documents/99.Github/Rasm/docs/standards/AGENTS.md`
- `/Users/bardiasamiee/Documents/99.Github/Rasm/docs/standards/agentic-documentation.md`
- `/Users/bardiasamiee/Documents/99.Github/Rasm/docs/standards/information-structure.md`
- `/Users/bardiasamiee/Documents/99.Github/Rasm/docs/standards/style-guide.md`
- `/Users/bardiasamiee/Documents/99.Github/Rasm/docs/standards/proof.md`
- `/Users/bardiasamiee/Documents/99.Github/Rasm/docs/standards/formatting.md`
- `/Users/bardiasamiee/Documents/99.Github/Rasm/_context-poison-audit-2026-06-06/agent-reports/README.md`

Read every Markdown file in the requested folders:
- `/Users/bardiasamiee/Documents/99.Github/Rasm/docs/standards/task/contributing.md` - 316 lines
- `/Users/bardiasamiee/Documents/99.Github/Rasm/docs/standards/task/how-to.md` - 295 lines
- `/Users/bardiasamiee/Documents/99.Github/Rasm/docs/standards/task/runbook.md` - 356 lines
- `/Users/bardiasamiee/Documents/99.Github/Rasm/docs/standards/learning/onboarding.md` - 296 lines
- `/Users/bardiasamiee/Documents/99.Github/Rasm/docs/standards/learning/tutorial.md` - 394 lines

## Method

Audited the target files against context-poison smells named in the task and the active standards-library rules: artifact separation, no task/session/process narration, no project-coupled examples in reusable standards, claim-level proof instead of page-level source chatter, omission of decorative proof fields, and examples that teach transferable artifact shape rather than local validation rituals. Findings below cite file and line evidence from the full target-file reads.

## Findings

### F1: Markdown validation examples leak local docs-only gates into reusable type standards
- Severity: high
- Confidence: evidence-backed
- Location: `/Users/bardiasamiee/Documents/99.Github/Rasm/docs/standards/task/how-to.md:80-105`; `/Users/bardiasamiee/Documents/99.Github/Rasm/docs/standards/task/how-to.md:167-177`; `/Users/bardiasamiee/Documents/99.Github/Rasm/docs/standards/learning/tutorial.md:107-113`; `/Users/bardiasamiee/Documents/99.Github/Rasm/docs/standards/learning/tutorial.md:251-258`
- Evidence:
  - `how-to.md` uses a complete mini-guide titled `# [VALIDATE_MARKDOWN_CHANGE]` with `git status --short -- <changed-paths>`, `git diff --check -- <changed-paths>`, local path and anchor validation, and a proof-gap checklist.
  - The same file repeats `git diff --check -- <changed-paths>` as the accepted command and rejects unscoped `git diff --check`.
  - `tutorial.md` uses `# [VALIDATE_MARKDOWN_DIFF]` as both the template title and accepted title, then uses a configured link and anchor validation lesson step.
- Why it may poison context:
  - These are reusable standards for how-to and tutorial documents, but the examples teach future agents to center Markdown validation, Git diff safety, and local path or anchor checks as generic produced-artifact examples.
  - The content duplicates proof and docs-as-code ownership from `proof.md` into task and learning type examples. Agents can cargo-cult the validation ladder into unrelated how-to guides and tutorials, especially because the examples are full enough to resemble publishable artifacts.
- Suggested disposition:
  - Replace the docs-only Git examples with neutral, domain-agnostic artifacts that prove container shape without naming repository validation gates. Keep only one minimal command-shaped example where command placement is the point, and route docs-as-code gate selection back to `proof.md`.

### F2: Gate and proof examples over-specify validation result vocabulary outside the proof owner
- Severity: medium
- Confidence: evidence-backed
- Location: `/Users/bardiasamiee/Documents/99.Github/Rasm/docs/standards/task/contributing.md:190-199`; `/Users/bardiasamiee/Documents/99.Github/Rasm/docs/standards/task/how-to.md:195-205`; `/Users/bardiasamiee/Documents/99.Github/Rasm/docs/standards/task/how-to.md:220-227`; `/Users/bardiasamiee/Documents/99.Github/Rasm/docs/standards/learning/tutorial.md:327-334`
- Evidence:
  - `contributing.md` shows a quality-gate table with `Markdown update`, `git diff --check -- <paths>`, `[PASS] no whitespace drift`, and `[SKIP] validator access needed`.
  - `how-to.md` shows verification checklist examples around `git diff --check`, link and anchor validation, and a rejected verification form.
  - `how-to.md` includes `Evidence: <validation-command> ran against the documented path set; local path and anchor validation passed or the proof gap was recorded` plus `Last verified: 2026-06-04`.
  - `tutorial.md` repeats accepted done checks centered on `<validation-command>`, scoped changed paths, and missing link or anchor checkers.
- Why it may poison context:
  - Type standards should say what a contributing guide, how-to, or tutorial must prove locally, but these examples drift into a concrete validation ladder and proof-field wording. That makes future generated artifacts more likely to paste generic gate rows, old dates, and proof fields even when the produced document has no maintained checker.
  - The fixed date `2026-06-04` is especially copyable and likely to become stale in examples.
- Suggested disposition:
  - Keep the rule that gates must be honest and claim-local, but collapse concrete command rows into abstract field examples unless the command itself is the subject. Remove fixed dates from examples or make them unmistakable placeholders such as `YYYY-MM-DD` where the date is demonstrating field shape.

### F3: Onboarding standard leaks prompt/session provenance and local instruction-source chatter
- Severity: high
- Confidence: evidence-backed
- Location: `/Users/bardiasamiee/Documents/99.Github/Rasm/docs/standards/learning/onboarding.md:7-13`; `/Users/bardiasamiee/Documents/99.Github/Rasm/docs/standards/learning/onboarding.md:68-81`; `/Users/bardiasamiee/Documents/99.Github/Rasm/docs/standards/learning/onboarding.md:293-296`
- Evidence:
  - The use-case list says a future agent needs durable local preparation "without replaying chat history."
  - The section cardinality block bans role, team, owner, shadowing, progress, permission, and escalation metadata unless local tool output uses the field, then says produced ramps must replace placeholders with local truth.
  - The final validation checklist says the page follows the "strict agent-only metadata ban in [AGENTS.md](../AGENTS.md)."
- Why it may poison context:
  - The standard is otherwise a durable type rule for agent ramps, but these lines import session/process vocabulary and point to `AGENTS.md` as a named anti-metadata source. That creates a provenance trail inside a reusable standard and risks teaching generated ramps to mention chat history, local instruction bans, or metadata discourse instead of simply producing the ramp.
  - The metadata ban is real, but the owner route is already enforced by the standards overlay; repeating it by source name inside the produced-type validation encourages source/provenance chatter in artifacts.
- Suggested disposition:
  - Rephrase the use case as durable source preparation without task history or prior interaction context, or remove the clause. Move the `AGENTS.md` citation out of the produced-ramp validation and keep the durable rule local: produced ramps omit role/team/owner/progress scaffolding unless a named consumer reads it.

### F4: Tutorial thresholds look arbitrary enough to train rigid generated lessons
- Severity: medium
- Confidence: likely
- Location: `/Users/bardiasamiee/Documents/99.Github/Rasm/docs/standards/learning/tutorial.md:3-5`; `/Users/bardiasamiee/Documents/99.Github/Rasm/docs/standards/learning/tutorial.md:15`; `/Users/bardiasamiee/Documents/99.Github/Rasm/docs/standards/learning/tutorial.md:19-25`; `/Users/bardiasamiee/Documents/99.Github/Rasm/docs/standards/learning/tutorial.md:57-71`; `/Users/bardiasamiee/Documents/99.Github/Rasm/docs/standards/learning/tutorial.md:90`
- Evidence:
  - The file declares a "three-lesson threshold" as a local taxonomy rule.
  - It says a lesson has `3 to 12` checkpoint steps, repeats that value in section cardinality, and uses an "under about 15 minutes" compactness rule.
  - The final validation checklist repeats `3 to 12` and three-or-more path entries.
- Why it may poison context:
  - Some exact thresholds are useful, but these are not tied to an external consumer, renderer, or proof mechanism. Repeating them across the lead, structure, and validation makes generated tutorials likely to force-fit lesson counts, step counts, and time estimates instead of shaping the artifact around learner proof.
  - The repeated phrase "local rule" also exposes standards-internal taxonomy reasoning rather than the artifact rule a future agent needs.
- Suggested disposition:
  - Keep thresholds only where they materially prevent bad artifacts. Prefer one cardinality declaration plus the reason it protects learner proof. Remove repeated "local rule" phrasing and avoid using time estimates as structural triggers unless the produced tutorial truly publishes duration.

### F5: Runbook response diagram mostly restates section order and may be copied as decorative structure
- Severity: low
- Confidence: likely
- Location: `/Users/bardiasamiee/Documents/99.Github/Rasm/docs/standards/task/runbook.md:62-91`; `/Users/bardiasamiee/Documents/99.Github/Rasm/docs/standards/task/runbook.md:102-147`; `/Users/bardiasamiee/Documents/99.Github/Rasm/docs/standards/task/runbook.md:232-243`
- Evidence:
  - The response-path diagram is introduced as optional, then renders the generic sequence `Trigger -> Impact -> Safety -> Triage -> Decision -> Mitigation/Rollback/Escalation -> Verification/Evidence`.
  - The required structure below already names the same sections and states their order and role.
  - Later triage guidance says Mermaid should be used only when branch sequence and rejoin are harder to follow than a numbered list or decision table.
- Why it may poison context:
  - The diagram is not wrong, and it includes a text equivalent. The risk is that it models a generic lifecycle diagram in a standard whose own rules reject decorative or duplicated diagrams. Agents may copy the response-path diagram into produced runbooks even when a table or ordered sequence is enough.
- Suggested disposition:
  - Either remove the generic diagram or make the example show a specific branch decision that cannot be represented well by the required structure alone. Keep the optional-diagram rule, but avoid a diagram that mirrors the section spine.

### F6: Cross-type `Adjacent checks` lines are broad and repetitive, but mostly intentional
- Severity: note
- Confidence: evidence-backed
- Location: `/Users/bardiasamiee/Documents/99.Github/Rasm/docs/standards/task/contributing.md:23-30`; `/Users/bardiasamiee/Documents/99.Github/Rasm/docs/standards/task/how-to.md:15-22`; `/Users/bardiasamiee/Documents/99.Github/Rasm/docs/standards/task/runbook.md:17-24`; `/Users/bardiasamiee/Documents/99.Github/Rasm/docs/standards/learning/onboarding.md:15-20`; `/Users/bardiasamiee/Documents/99.Github/Rasm/docs/standards/learning/tutorial.md:19-25`
- Evidence:
  - Every target file uses an `AUTHORING_CONTRACT` with `Agent use`, produced structure, cardinality, adjacent checks, and maintenance triggers.
  - The pattern repeats across all scoped files, but each line states the type-specific condition under which adjacent standards matter.
- Why it may poison context:
  - Broad adjacent-check inventories can train agents to perform or paste cross-doc checklists even when a produced artifact does not consume adjacent facts.
  - In this corpus, the repetition appears to be the current type-standard opening contract, not accidental context poison, because each file uses condition language such as "only when their fact changes" or "only when the contributor workflow or evidence changes."
- Suggested disposition:
  - Do not treat the pattern as a cleanup target by itself. If tightening, shorten only the broad inventories and preserve the conditional trigger. The stronger fixes are F1 through F5.

## Clean or intentionally scoped areas

- `contributing.md` keeps security reporting out of normal issue and pull-request flow and repeatedly routes branch protection, CODEOWNERS, response-time promises, and security policy to maintained policy sources instead of inventing them.
- `how-to.md` has a strong type boundary: one competent-reader task, outcome proof, task-local troubleshooting only, and no broad option catalogs.
- `runbook.md` correctly blocks mutation when incident-process truth is absent and avoids importing provider severity defaults as local policy.
- `onboarding.md` is mostly precise about agent ramps as bounded source-area preparation, not tutorials, contribution workflow, or status trackers.
- `tutorial.md` correctly separates single tutorials from learning paths and requires primary-path execution proof before publication as available.

## Gaps and follow-up reads

- This was read-only for the standards files. No cleanup patch was made to `docs/standards/task/*.md` or `docs/standards/learning/*.md`.
- I did not run Markdown link, anchor, Mermaid, or docs-build validation because the requested operation was an audit report only and no active standards files were edited.
- I did not inspect generated outputs, prior reports, or non-target standards beyond the instruction chain and shared standards needed to judge the target files.
- If a second-wave editor follows this report, the highest-value pass is to replace the Markdown/Git validation examples in `how-to.md` and `tutorial.md`, then remove the `AGENTS.md` and chat-history provenance leakage from `onboarding.md`.

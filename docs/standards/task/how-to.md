# [HOW_STANDARDS]

A how-to guide carries one competent reader through one practical goal or problem to an observable outcome. Lead with the outcome, keep only the actions and judgment points needed to reach it, and close with outcome evidence rather than command completion. The reader already knows the domain and exercises judgment; the guide supplies the usable route, not teaching, background, operational recovery, or a lookup catalog.

## [1][USE_WHEN]

Write a how-to guide when every condition holds:
- the reader can already act in the domain and needs the reliable path, not instruction;
- the document drives exactly one repeatable task or problem with a named outcome;
- the outcome is observable, so the guide can end on a check that proves it;
- background, concepts, option catalogs, operational recovery, and support facts can live elsewhere and be named here, not embedded.

Route a first-success learning path, an operational symptom response, a contribution workflow, an API surface, supported-version facts, or a conceptual explanation to its own type. The README corpus map resolves the reader need to a type; this standard carries the how-to type only.

[AUTHORING_CONTRACT]:
- Agent use: state one observable task outcome, write the ordered path for a competent reader, and finish on outcome proof rather than command completion.
- Required produced structure: `Goal`, ordered `Procedure`, and `Verification`, plus triggered prerequisites, rollback, troubleshooting, boundaries, checklist, or maintenance sections.
- Section cardinality: one goal, one primary successful path, and one verification surface; conditional sections appear only when the task consumes their facts or recovery.
- Adjacent checks: check API, code documentation, README, architecture, roadmap, reference, runbook, tutorial, onboarding, contributing, test strategy, and support matrix only when their fact controls a target, branch, artifact, input, safety boundary, or verification.
- Maintenance triggers: update the how-to when a command, tool version, path, target, UI label, prerequisite, artifact, rollback path, troubleshooting signal, adjacent fact, proof gate, or verification result changes.

## [2][HOW_TO_BASELINE]

This standard operationalizes the Diátaxis how-to guide. A how-to is goal-oriented action for an already-competent user who knows what they want to achieve. Its structure follows the user's work, keeps focus on one goal, links explanation and reference instead of embedding them, and phrases unavoidable branches as conditional imperatives. Evidence: [Diátaxis how-to guidance](https://diataxis.fr/_/downloads/en/latest/pdf/).

The rules below add agent-facing structure, conditional section discipline, rollback behavior, troubleshooting boundaries, cross-document handoffs, and claim-level proof. Use those additions to make the route executable; do not turn a how-to into a learning course, operating runbook, API catalog, or roadmap update.

## [3][SECTION_RULES]

A how-to guide has a required core and conditional support. The required core is `# [HOW_TASK]`, `## [1][GOAL]`, `## [2][PROCEDURE]`, and `## [3][VERIFICATION]`. Conditional sections appear only when the task consumes the fact or action they carry, and every inserted conditional section renumbers in document order.

- `Prerequisites` appears when the task consumes access, target context, tools, versions, prepared artifacts, or explicit permissions the reader can confirm before starting.
- `Rollback` appears when the task changes state; if no reverse action exists, the section states that fact and routes recovery to a runbook by topic.
- `Troubleshooting` appears only for task-local failures with concrete recovery that returns the reader to the same path.
- `Boundaries` appears when an adjacent maintained document carries what this guide deliberately excludes.
- `Checklist` appears in a published guide only when an agent or configured workflow consumes reader-visible verification gates.
- `Maintenance` appears when the how-to describes a path whose commands, signals, targets, or adjacent routes drift independently.

Do not classify guides by project maturity, product size, or broad task family. Required and conditional sections derive from the work the reader must perform and the proof the outcome requires.

## [4][REQUIRED_STRUCTURE]

Use the section set below; each `##` heading is a standalone retrieval unit a reader may open out of order. The base template includes only universal sections, so agents do not publish empty conditional headings. Add conditional sections only when their trigger applies, and renumber headings in document order.

```markdown template
# [HOW_TASK]

## [1][GOAL]

## [2][PROCEDURE]

## [3][VERIFICATION]
```

Add these conditional sections only when their trigger applies:
```markdown template
## [N][PREREQUISITES]

## [N][ROLLBACK]

## [N][TROUBLESHOOTING]

## [N][BOUNDARIES]

## [N][CHECKLIST]

## [N][MAINTENANCE]
```

Required sections carry one purpose each: `Goal` names the single task outcome in the reader's terms; `Procedure` gives a logical sequence ordered by dependency, reader flow, setup context, or judgment sequence; `Verification` proves the `Goal` outcome and states whether the path ran or which check remains unrun. Conditional sections carry only the fact their trigger requires: prerequisites are checkable records, rollback is the reverse path or recovery route, troubleshooting is task-local recovery, boundaries route adjacent routes, checklist records consumed verification gates, and maintenance names drift triggers.

Omit a conditional section when the condition is absent. Do not publish empty placeholders, generic readiness text, reference inventories, broad recovery branches, or author scaffolding to make the template look complete.

## [5][MINIMAL_PATTERN]

A minimal how-to still carries the full outcome path. Use a compact skeleton when the task is small rather than adding placeholder sections:
```markdown conceptual
# [VALIDATE_STANDARDS_MARKDOWN]

## [1][GOAL]

Validate a standards-only Markdown change so the proof record names whitespace safety and any link-check gap.

## [2][PROCEDURE]

1. In the repository root, list the standards changes.
    Operation: `git status --short -- docs/standards`
    Expected result: only intended Markdown standards files appear.
    If code files appear, stop and split the change before validating it as docs-only.
2. Check patch safety and whitespace.
    Operation: `git diff --check -- docs/standards`
    Expected result: the command exits 0 and prints no whitespace error lines.
    If the command reports a file and line, fix that line and rerun the check.

## [3][VERIFICATION]

- [ ] `git status --short -- docs/standards` shows only the intended standards files.
- [ ] `git diff --check -- docs/standards` exits 0.
- [ ] Any missing configured link or anchor checker is stated as a proof gap.

Evidence: `git diff --check -- docs/standards`; local path and anchor validation result or explicit proof gap.
```

The skeleton uses a checklist because two independent outcome conditions must hold. A single-condition `Verification` may be a short proof statement with `Evidence:` beside it; do not force a one-item checklist.

## [6][SCOPE_RULES]

How-to scope follows these rules:
- Solve one task per guide and state its outcome in the title and `Goal`.
- Start and end where a competent reader starts and ends; do not re-teach setup the reader already carries.
- List only prerequisites this task consumes; name broader environment setup by topic and route it elsewhere.
- Keep action and necessary judgment in the guide; move concepts, API catalogs, option inventories, broad failure analysis, and support facts to their controlling types and name them by topic.
- Carry one primary successful path. Add branches only for unavoidable judgment points, state the decision condition, and rejoin when possible; split broad variants into separate guides.

When a procedure depends on adjacent truth to choose a target, branch, artifact, procedure input, safety boundary, or verification, carry one task-specific handoff record at the first point of use. Do not copy the adjacent document's map, roadmap, catalog, lesson, or policy body:
```markdown template
Changed fact: <specific target, branch, artifact, input, safety boundary, or verification this procedure consumes>
Consumed by: <procedure step, prerequisite, rollback, troubleshooting entry, or verification check>
Use in this document: <what the reader does with that adjacent fact in this guide>
Update when: <adjacent source fact, command, support target, generated artifact, or route changes>
Close when: <procedure no longer consumes the fact, or adjacent document routes it away>
Route-away: <concept, lookup, recovery, learning, status fact, gate policy, or entry route that stays in the adjacent document>
```

Omit the handoff when the guide merely links further reading after completion. Include it only when the procedure would be unsafe, ambiguous, or incomplete without the adjacent route. API-call procedures, generated-reference-driven steps, agent-practice guides, contributor tasks, and gate-driven tasks are valid handoff cases only when they change the work inside the procedure.

## [7][PREREQUISITES_RULES]

A prerequisite is a record the reader scans and confirms before starting, so render `Prerequisites` as a definition block with one `label: value` per line, never as a paragraph and never as a one-row table. Each record names a concrete, checkable fact, not a vague readiness claim:
- `Access`: the named permission, credential, or scope, with the surface it grants on.
- `Target context`: the named environment, host, directory, document, or resource the task acts on.
- `Tools`: the named CLI, package, service, or UI plus the minimum version the procedure assumes, with the version as a numeral the reader can compare.
- `Prepared artifact`: the named file, object, plan, or input the procedure consumes.

State what the reader confirms, not how they obtain it; route environment setup the reader must perform first to its own guide by topic. A prerequisite the reader cannot observe before starting is a procedure step, not a prerequisite.

```markdown conceptual
Access: repository checkout with docs-edit permission
Target context: repository root at `<repo-root>`
Tools: Git plus repository Python environment for `uv run python -m tools.quality`
Prepared artifact: standards-only Markdown diff under `docs/standards/`
```

## [8][PROCEDURE_RULES]

Procedure rules split into step order and step wording:
[STEP_ORDER]:
- Number steps as the default procedure form; order may follow dependency, reader flow, setup context, or judgment sequence.
- Use peer bullets only inside a step or for genuinely independent checks whose order does not affect the reader's path.
- Combine actions that share a place and yield one logical result; split a step that changes place or produces a separately verifiable result.
- Link a repeated sub-procedure to its canonical guide instead of copying it.

[STEP_WORDING]:
- Open each step with an imperative verb and an input-neutral UI verb, so the step holds across mouse, keyboard, command, and automation surfaces.
- State the place of action before the action when the tool, shell, host, directory, UI, or document is not obvious from the prior step.
- State a gating condition before the action it controls as a conditional imperative: `If <signal>, <action>`.
- State the expected result of a step when the reader needs that signal to proceed, so the reader confirms progress without running the full `Verification` block.
- Mark an optional step with a leading `Optional:` and mark an irreversible step with a leading `Irreversible:` so the reader sees the stakes before acting.

A step that uses a command should bind operation, expected result, and next condition in the same record so an agent does not leave the reader with a command-only instruction:
```markdown conceptual
1. In the repository root, check the reviewed diff.
    Operation: `git diff --check -- docs/standards`
    Expected result: the command exits 0 and prints no whitespace error lines.
    If a file and line are reported, fix that line before rerunning.
```

Use a fenced command only when the command is multi-line, copy-safe as written, or clearer outside the step record. Include a rejected near-miss only when it prevents a likely material error:
```bash conceptual
git diff --check -- docs/standards
```

```bash rejected
git diff --check
```

For a forking procedure, use prose or a numbered branch first. Use a decision table when independent conditions jointly choose an action; use Mermaid only when branch sequence and rejoin are harder to follow as steps or a decision table.

```markdown conceptual
| [INDEX] | [DRIFT_DETECTED] | [PLAN_VALID] | [ACTION]                                              |
| :-----: | :--------------- | :----------- | :---------------------------------------------------- |
|   [1]   | no               | yes          | run standards validation                              |
|   [2]   | yes              | yes          | reconcile docs drift, then validate                   |
|   [3]   | any              | no           | route the invalid plan to design-doc or roadmap first |
```

## [9][VERIFICATION_ROLLBACK_RULES]

End on a `Verification` block that observes the `Goal` outcome, not that a command exited zero. Bind the check to the task's actual outcome: setup proves convergence to the target state, mutation proves the new state, and read-only work proves the artifact, reading, or export shape. State each expected result beside the command, query, dashboard, generated artifact, UI path, or review gate that proves it.

Render `Verification` as a checklist when the outcome carries several independently observable conditions:
```markdown conceptual
## [3][VERIFICATION]

- [ ] `git diff --check -- docs/standards` exits 0.
- [ ] Local path and anchor validation passes, or the missing checker is recorded as a proof gap.
```

Reject verification that proves only command completion:
```markdown rejected
## [3][VERIFICATION]

- [ ] `git diff --check` exited 0.
```

The rejected form omits the requested revision, target state, and readiness outcome, so it cannot prove the `Goal`.

For a state-changing task, give `Rollback` the reverse action, its expected result, and its own check. When no reverse exists, say so and route recovery to a runbook by topic:
```markdown conceptual
Reverse action: revert the edited Markdown section in the same patch.
Expected result: the previous rendered section text and links are restored.
Verification: `git diff --check -- docs/standards` exits 0 after the revert.
```

```markdown conceptual
Reverse action: none; this task permanently exports and publishes the artifact.
Recovery route: use the publication rollback runbook if the published artifact is wrong.
```

A how-to guide claims a path works, so the path must have been run or its gaps stated. Use the proof labels from [proof.md](../proof.md) beside the affected step or outcome:
```markdown conceptual
Evidence: `git diff --check -- docs/standards` ran against the documented path set; local path and anchor validation passed or the proof gap was recorded.
Last verified: 2026-06-04
```

State an unrun step honestly: mark it provisional and name the gate that would prove it, rather than asserting a path that was not executed.

## [10][TROUBLESHOOTING_RULES]

Keep `Troubleshooting` to failure modes that block this task and have a concrete recovery; route operational recovery, escalation, and incident evidence to a runbook by topic. A failure-mode set is finite and scannable, so render each entry as a record carrying its signal, cause, and recovery, never as a flat paragraph:
- `Signal`: the observable symptom the reader sees — an error string, a failed check, or a wrong reading.
- `Cause`: the condition that produced the signal, when known.
- `Recovery`: the concrete action that returns the reader to the path, or the route to the controlling runbook when recovery exceeds this task.

Render each entry as a `### [N.M][SYMPTOM]` H3 whose body carries the fields one `label: value` per line:
```markdown conceptual
### [N.M][TARGET_NOT_FOUND]

Signal: unresolved anchor is reported for `docs/standards/reference/api.md#generated-library-reference`
Cause: the heading label changed without updating the in-repo link.
Recovery: update the link target or restore the heading anchor, then rerun local path and anchor validation.
```

A symptom-to-recovery set with three or more short entries may use a lookup table keyed on signal, but do not publish both record blocks and a table for the same failures. Choose the container the reader will scan fastest.

## [11][FORMAT_CHOICES]

Choose forms by the reader action this how-to requires. Use numbered procedure steps for the normal path, step records for commands with expected results, definition blocks for prerequisites, checklists for multi-condition verification, and signal-keyed records or lookup tables for task-local troubleshooting. Route general table limits, record escalation, diagrams, and code-block intent labels to [information-structure.md](../information-structure.md).

For a forking procedure, choose prose or a numbered branch first. Use a decision table when independent conditions jointly choose an action. Use Mermaid only when branch sequence and rejoin are easier to follow visually than as steps or a decision table, and keep a text equivalent beside it.

## [12][MAINTENANCE]

Review a how-to when a command, tool version, target path, UI label, expected output, prerequisite, prepared artifact, rollback path, troubleshooting signal, adjacent source, proof gate, or verification signal changes. Update it from real use when readers choose the wrong branch, need hidden context, rerun unsafe commands, or report outcome checks that no longer prove the goal. Delete or split a how-to whose goal has become multiple tasks rather than preserving broad variants in one guide.

## [13][BOUNDARIES]

These adjacent standards own routed material:
[TASK_DOCUMENTS]:
- [runbook.md](runbook.md) carries operational symptom response, recovery, escalation, rollback under incident pressure, communication, and incident evidence; a how-to performs normal tasks and routes operational recovery there.
- [contributing.md](contributing.md) carries contribution workflow, review collaboration, and pull-request evidence.
- [tutorial.md](../learning/tutorial.md) teaches a first-success path and carries learning ramps; a how-to completes a task for a reader who can already act.

[SHARED_STANDARDS]:
- [proof.md](../proof.md) carries evidence strength, verification gaps, and claim-level evidence details.
- [information-structure.md](../information-structure.md) carries container, table, record, decision-table, checklist, diagram form, and the intent-label vocabulary this guide applies to fenced blocks.
- This guide carries the leading step markers `Optional:` and `Irreversible:` because they change how a how-to reader acts before a step.
- [style-guide.md](../style-guide.md) carries imperative and input-neutral phrasing and the conditional-imperative form this guide requires of procedure steps.
- [README.md](../README.md) carries reader-need classification, document-type choice, placement, and lifecycle; route a draft that serves another reader need there.

## [14][CHECKLIST]

Use this checklist by group:
[SCOPE]:
- [ ] H1 uses a bracketed semantic task label and `Goal` names one observable outcome.
- [ ] Conditional sections appear only when the task consumes their facts or actions.
- [ ] Adjacent routes are named by topic in prose and linked once each only in `Boundaries` when adjacent maintained routes exist.
- [ ] Adjacent-route handoff records appear only when adjacent truth controls a procedure decision, target, artifact, input, safety boundary, or verification.

[PROCEDURE]:
- [ ] Prerequisites are a definition block of checkable access, context, versioned-tool, or prepared-artifact records, listing only what this task consumes.
- [ ] Procedure steps are imperative, input-neutral, ordered by reader flow, condition-first, and place-clear.
- [ ] Optional and irreversible steps carry their leading markers when present.
- [ ] Fenced commands appear only when they are clearer than a step record, and every ordinary fenced block carries one intent label.

[VERIFICATION]:
- [ ] Verification proves the outcome, states each expected result, and uses claim-level evidence through [proof.md](../proof.md).
- [ ] The path was run or the unrun gate is stated beside the affected step or check.
- [ ] State-changing guides carry `Rollback` with a reverse check, or state that no reverse exists and route recovery to a runbook by topic.
- [ ] Troubleshooting is task-local, rendered as signal-cause-recovery records or a lookup table, and actionable.
- [ ] Maintenance triggers cover commands, tools, targets, expected outputs, rollback, troubleshooting, adjacent routes, and verification signals.

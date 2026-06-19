# [AGENT_RAMP_STANDARDS]

An agent ramp prepares a future agent to operate safely in one bounded source area before broad edits. It gives the agent the minimum code path, read-first questions, local constraints, first safe action, readiness gate, and stop rules needed to decide whether the next action is safe. It is not a tutorial, contribution workflow, runbook, architecture map, permission ladder, status tracker, or target sequencing artifact.

## [01]-[USE_WHEN]

Use an agent ramp when a source area needs a repeatable preparation path before edits:
- a complex package, tool, bridge, generated surface, or host integration has local hazards;
- the first safe action must be constrained before broader edits;
- confirmation gates, generated artifacts, or source maps must be read in a fixed order;
- a future agent needs durable local preparation before edits.

Route first-success teaching to [tutorial.md](tutorial.md), normal repeatable work to [how-to.md](../task/how-to.md), operational recovery to [runbook.md](../task/runbook.md), contribution workflow to [contributing.md](../task/contributing.md), gate-portfolio policy to [test-strategy.md](../explanation/test-strategy.md), and lookup facts to [reference.md](../reference/reference.md).

[AUTHORING_CONTRACT]:
- Agent use: identify the bounded source area, read the exact sources, complete one low-risk action, and stop unless the readiness gate proves the next action is safe.
- Required produced structure: `Scope`, `Code path`, `Read first`, `Constraints`, `First safe action`, `Readiness gate`, `Stop rules`, `Boundaries`, and `Result check`.
- Section cardinality: one source area, one first safe action, one readiness gate, and one stop-rule set.
- Adjacent checks: check architecture, API, support matrix, roadmap, code documentation, how-to, runbook, contributing, reference, and README only when their fact changes the reading path, first action, readiness gate, or stop rule.
- Maintenance triggers: update the ramp when code path, generated artifact, command, support boundary, first safe action, readiness gate, stop rule, or adjacent route changes.

## [02]-[REQUIRED_STRUCTURE]

Use this produced-document skeleton. The structure is a ramp contract, not a mandate to fill every section with metadata; omit optional sections and records that do not change agent behavior.

```markdown template
# [<SOURCE_AREA>_AGENT_RAMP]

<Lead: one sentence naming the bounded source area, first safe action, readiness gate, and stop condition.>

## [1]-[SCOPE]

## [2]-[SOURCE_PATH]

## [3]-[READ_FIRST]

## [4]-[CONSTRAINTS]

## [5]-[FIRST_SAFE_ACTION]

## [6]-[READINESS_GATE]

## [7]-[STOP_RULES]

## [8]-[BOUNDARIES]

## [N]-[GENERATED_SURFACES]

## [N]-[DRIFT_TRIGGERS]

## [N]-[AVAILABILITY_RECORDS]
```

[SECTION_CARDINALITY]:
- Opening paragraph, `Scope`, `Code path`, `Read first`, `Constraints`, `First safe action`, `Readiness gate`, `Stop rules`, `Boundaries`, and `Result check` are required.
- Conditional sections appear only when their decision-table trigger holds.
- Produced ramps contain no generic role, team, owner, shadowing, progress, permission, or escalation metadata unless a local tool output uses that literal field.

Ramp-local availability is a readiness axis, not a roadmap lifecycle. Use it only for independently tracked readings, generated surfaces, or actions, before the first record that carries `Status`:
- `READY`: code path, first action, and readiness gate are current.
- `PROVISIONAL`: usable only with a stated confirmation gap.
- `BLOCKED`: required source, generated artifact, or readiness gate is unavailable.
- `DROPPED`: no longer part of the ramp.

Shared lifecycle states omitted here are intentional: ramps do not use `QUEUED`, `ACTIVE`, `RETURNABLE`, `COMPLETE`, or `CANCELED`. A ramp either allows the next action, blocks it, records a confirmation gap, or routes away. Do not use progress percentages, phase names, or completion bars in learning ramp docs.

Produced ramps must replace every placeholder with local truth. A ramp is incomplete if it contains `<...>` fields, broad source areas, unnamed gates, unowned stop routes, or source records that do not change the first safe action or readiness gate.

## [03]-[RAMP_CHOOSER]

Choose an agent ramp only when preparation before edits is the reader problem. Use this decision table before drafting:

| [INDEX] | [NEED]                             | [ROUTE]       |
| :-----: | :--------------------------------- | :------------ |
|  [01]   | prepare an agent before first edit | agent ramp    |
|  [02]   | teach first success                | tutorial      |
|  [03]   | repeat a known task                | how-to        |
|  [04]   | recover from symptom               | runbook       |
|  [05]   | follow contribution workflow       | contributing  |
|  [06]   | choose test gate policy            | test strategy |
|  [07]   | explain current structure          | architecture  |
|  [08]   | look up facts or contracts         | lookup route  |

Route labels map to current standard files:
- tutorial: [tutorial.md](tutorial.md)
- how-to: [how-to.md](../task/how-to.md)
- runbook: [runbook.md](../task/runbook.md)
- contributing: [contributing.md](../task/contributing.md)
- test strategy: [test-strategy.md](../explanation/test-strategy.md)
- architecture: [architecture.md](../explanation/architecture.md)
- lookup route: use [reference.md](../reference/reference.md) for standalone facts, [api.md](../reference/api.md) for API contracts, [support-matrix.md](../reference/support-matrix.md) for support status, and [code-documentation.md](../reference/code-documentation.md) for public symbol behavior.

If the source area has no local hazard, no constrained first action, and no readiness gate, do not write an agent ramp. Route to the smallest adjacent document that changes the agent's next action.

## [04]-[SCOPE]

State the smallest source area that owns the hazard and readiness gate. Name what is in scope, what is out of scope, the source route that proves the boundary, and the condition that splits one ramp into two.

Use a short prose paragraph when the boundary is obvious. Use a definition block only when the ramp needs independently scannable fields:

```markdown template
Area: `<bounded code path or generated surface>`
In: `<files, generated artifacts, or commands the ramp covers>`
Out: `<adjacent source area or document route>`
Boundary source: `<architecture, README, manifest, generated contract, or code path>`
Split when: `<second hazard, second readiness gate, or unrelated first action appears>`
```

[RAMP_AREA]:
- Rejected area: whole repository
- Reason: the rejected form is valid only when the readiness gate and first safe action are truly repository-wide.

## [05]-[SOURCE_PATH]

Name only the code paths the agent must open before acting. A code path record is a retrieval primitive, not a summary.

```markdown template
Path: `<repository path, generated artifact, manifest, or command output>`
Use: `<decision this source controls>`
Status: READY | PROVISIONAL | BLOCKED | DROPPED
Observed result: `<path exists, command output, generated artifact, or confirmation gap>`
Update when: `<path, artifact, command, or adjacent route changes>`
```

Use a GroupedRecord (`[RECORD_KEY]:` plus `- Field: value` bullets) when several source records share this schema. Keep each record short; if a source needs explanation, route to architecture, API, reference, support matrix, or code documentation instead of embedding the explanation.

## [06]-[READ_FIRST]

Order readings by dependency question, not directory order. Each reading item must answer one question the agent needs before the first safe action; if the list grows past 5 items, group it by dependency question or split the ramp.

```markdown template
Question: `<what the agent must know before acting>`
Owner: `<path or generated artifact>`
Check: `<observable answer, symbol, command result, or confirmation gap>`
Route-away: `<body that stays in architecture, API, reference, support, how-to, or runbook>`
```

[READING_SHAPE]:
- Accepted: `Question: Which generated files are source-owned?`
- Rejected: `README.md`, `src/`, `tests/`
- Reason: the accepted form names the decision, source, check, and route-away; the rejected form is a path inventory.

## [07]-[CONSTRAINTS]

Constraints are allowed only when they change the first safe action or readiness gate. Each constraint names the source that proves it and routes broader explanation away.

```markdown template
Constraint: `<forbidden edit, required gate, generated-file rule, support limit, or host-runtime limit>`
Owner: `<path, generated artifact, command output, or confirmation gap>`
Action effect: `<what the agent must do, avoid, or verify>`
Update when: `<source or adjacent route changes>`
Route-away: `<architecture, API, code documentation, support matrix, test strategy, confirmation, how-to, or runbook body>`
```

[CONSTRAINT_ROUTES]:
- architecture owns topology and invariants;
- API and code documentation own generated contracts and public symbols;
- support matrix owns support limits;
- test strategy owns test-portfolio gates;
- confirmation owns confirmation mechanics;
- how-to and runbook own procedures and recovery.

Do not add generic conduct, collaboration, escalation, permission, role, or team fields.

## [08]-[FIRST_SAFE_ACTION]

The first safe action is the smallest real action that proves the agent can operate in the source area without broad edits. It may be a low-risk edit, read-only trace, fixture reproduction, generated-output comparison, or confirmation dry-run.

```markdown template
Action: `<single low-risk edit, read-only trace, fixture reproduction, generated-output comparison, or confirmation dry-run>`
Scope: `<one file, fixture, command, generated artifact, or source slice>`
Forbidden: `<edits, commands, paths, or side effects outside the action>`
Expected result: `<observable output, diff, artifact, or answer>`
Observed result: `<command, diff, generated output, or confirmation gap>`
```

If the action needs step-by-step instructions, route to a how-to. If the action teaches first success, route to a tutorial. If the action responds to a failure symptom, route to a runbook.

## [09]-[READINESS_GATE]

The readiness gate closes the ramp. The agent may proceed only when the gate passes or the confirmation gap is explicitly acceptable for the next action.

```markdown template
Gate: `<command, generated comparison, path check, source inspection, or review-free confirmation>`
Pass signal: `<exit code, output, artifact, diff, or exact observation>`
Failure action: `<fix source, regenerate, route to how-to, route to runbook, or stop>`
Observed result: `<captured result or confirmation gap>`
```

Keep the readiness gate local to the ramp. Broad test portfolio policy belongs to [test-strategy.md](../explanation/test-strategy.md), and confirmation mechanics belong to [proof.md](../proof.md).

## [10]-[STOP_RULES]

Stop rules constrain the next action. State only conditions that force the agent to stop or route away.

Use definition records for one or two stop rules. Use a compact table when several stop conditions share the same fields:

| [INDEX] | [STOP_WHEN]              | [NEXT_ROUTE] | [EVIDENCE_TO_PRESERVE] |
| :-----: | :----------------------- | :----------- | :--------------------- |
|  [01]   | missing source           | architecture | confirmation gap       |
|  [02]   | stale generated artifact | API          | artifact path          |
|  [03]   | readiness gate fails     | how-to       | command output         |
|  [04]   | unsupported target       | support      | support row            |
|  [05]   | destructive side effect  | runbook      | error or diff          |
|  [06]   | ambiguous boundary       | README       | boundary question      |

The table above is a shape example. Produced ramps replace each row with local stop conditions, exact route documents, and confirmation.

## [11]-[GENERATED_SURFACES]

Add this section only when generated files, mirrors, or contracts affect the first safe action or readiness gate. Generated-surface records state the controlling source and the action boundary; they do not explain the generated format.

```markdown template
Surface: `<generated path, mirror, or contract>`
Owner: `<generator, code path, manifest, or command>`
Refresh route: `<command or adjacent generated-contract document>`
Edit rule: `<read-only, regenerate only, or source-owned edits only>`
Stop when: `<source missing, generator unavailable, output drift, or confirmation gap>`
```

Route generated contract meaning to API or code documentation, generator operation to how-to, and confirmation strength to confirmation.

## [12]-[DRIFT_TRIGGERS]

Add this section only when several stale events need a shared route. A single drift-prone claim carries its own confirmation and update condition beside the claim.

Use these common drift triggers:
- code path, generated artifact, command, or manifest changes;
- support target, host runtime, or external contract changes;
- first safe action or readiness gate changes;
- stop rule, route-away target, or boundary source changes;
- adjacent architecture, API, support, how-to, runbook, contributing, reference, or README route changes.

Events beat calendar dates unless a maintained policy requires a scheduled review.

## [13]-[AVAILABILITY_VOCABULARY]

The closed availability set is declared before first record use in `Required structure`. This section exists only to route extended availability explanation when a produced ramp needs it; do not restate the vocabulary after records.

## [14]-[BOUNDARIES]

These adjacent standards own routed material:

[LEARNING_TASK_TYPES]:
- [tutorial.md](tutorial.md) teaches a first success; an agent ramp prepares a future agent to operate in one source area.
- [how-to.md](../task/how-to.md) carries repeatable competent-reader procedures.
- [runbook.md](../task/runbook.md) carries operational symptom response.
- [contributing.md](../task/contributing.md) carries contribution workflow and pull-request confirmation.

[EXPLANATION_REFERENCE_TYPES]:
- [architecture.md](../explanation/architecture.md) carries current structure, dependency direction, and invariants.
- [roadmap.md](../explanation/roadmap.md) carries future-work sequence, task identity, and task exit confirmation.
- [api.md](../reference/api.md) carries generated or contract-backed API truth.
- [code-documentation.md](../reference/code-documentation.md) carries public-symbol comments and generated source-reference contracts.
- [support-matrix.md](../reference/support-matrix.md) carries support status, lifecycle, platform, version, and compatibility truth.
- [reference.md](../reference/reference.md) carries standalone lookup facts.
- [README.md](../README.md) carries document-type routing, placement, lifecycle, and split/link decisions.

[SHARED_STANDARDS]:
- [information-structure.md](../information-structure.md) carries container choice, record shape, table decomposition, and checklist form.
- [style-guide.md](../style-guide.md) carries prose, terminology, links, and code-safe Markdown.
- [proof.md](../proof.md) carries confirmation strength, refresh, confirmation gaps, and verification gates.

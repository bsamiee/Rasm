# [AGENT_RAMP_STANDARDS]

An agent ramp prepares a future agent to operate safely in one bounded source area. It gives the agent the minimum reading path, local constraints, safe first action, validation gate, and stop rule needed before it changes files. It is not a tutorial, contribution workflow, runbook, architecture map, permission ladder, or planning artifact.

## [1][USE_WHEN]

Use an agent ramp when a source area needs a repeatable preparation path before edits:
- a complex package, tool, bridge, generated surface, or host integration has local hazards;
- the first safe action must be constrained before broader edits;
- validation gates, generated artifacts, or source maps must be read in a fixed order;
- a future agent needs a durable ramp without replaying chat history.

Route first-success teaching to [tutorial.md](tutorial.md), normal repeatable work to [how-to.md](../task/how-to.md), operational recovery to [runbook.md](../task/runbook.md), contribution workflow to [contributing.md](../task/contributing.md), and lookup facts to [reference.md](../reference/reference.md).

Authoring contract:
- Agent use: identify the bounded source area, read the exact sources, complete one low-risk action, and stop unless the validation gate proves readiness.
- Required produced structure: `Scope`, `Source path`, `Read first`, `Constraints`, `First safe action`, `Validation`, `Stop rules`, `Boundaries`, and `Checklist`.
- Section cardinality: one source area, one first safe action, one validation gate, and one stop-rule set.
- Adjacent checks: check architecture, API, support matrix, roadmap, code documentation, how-to, runbook, contributing, reference, and README only when their fact changes the reading path, first action, validation, or stop rule.
- Maintenance triggers: source path, generated artifact, command, support boundary, first safe action, validation gate, stop rule, or adjacent route changes.

## [2][REQUIRED_STRUCTURE]

Use this skeleton. Omit optional sections that do not change agent behavior.

```markdown template
# [<SOURCE_AREA>_AGENT_RAMP]

<Lead: one sentence naming the bounded source area, first safe action, validation gate, and stop condition.>

## [1][SCOPE]

## [2][SOURCE_PATH]

## [3][READ_FIRST]

## [4][CONSTRAINTS]

## [5][FIRST_SAFE_ACTION]

## [6][VALIDATION]

## [7][STOP_RULES]

## [8][BOUNDARIES]

## [9][CHECKLIST]
```

Add these conditional headings only when they change agent behavior:

```markdown template
## [N][GENERATED_SURFACES]

## [N][DRIFT_TRIGGERS]
```

## [3][STATUS_VOCABULARY]

Agent ramp records use this closed availability set when the ramp contains independently tracked readings or actions:
- `READY`: source path, first action, and validation gate are current.
- `PROVISIONAL`: usable only with a stated proof gap.
- `BLOCKED`: required source, generated artifact, or validation gate is unavailable.
- `DROPPED`: no longer part of the ramp.

Do not use progress percentages, phase names, or completion bars. A ramp either allows the next action, blocks it, or routes away.

## [4][SOURCE_PATH]

Name only the source paths the agent must open before acting. A source path record is a retrieval primitive, not a summary.

```markdown template
Path: `<repository path, generated artifact, manifest, or command output>`
Use: `<decision this source controls>`
Status: READY | PROVISIONAL | BLOCKED | DROPPED
Evidence: `<path exists, command output, generated artifact, or proof gap>`
Update when: `<path, artifact, command, or adjacent route changes>`
```

Keep source records short. If a source needs explanation, route to architecture, API, reference, or support matrix instead of embedding it.

## [5][READ_FIRST]

Order readings by dependency, not directory order. A reading item must answer one question the agent needs before the first safe action.

```markdown template
Question: `<what the agent must know before acting>`
Source: `<path or generated artifact>`
Check: `<observable answer, symbol, command result, or proof gap>`
Route-away: `<body that stays in architecture, API, reference, support, how-to, or runbook>`
```

Reject reading lists that only name files:

```markdown rejected
- `README.md`
- `src/`
- `tests/`
```

## [6][CONSTRAINTS]

Constraints are allowed only when they change the first safe action or validation gate. Each constraint names the source that proves it.

```markdown template
Constraint: `<forbidden edit, required gate, generated-file rule, support limit, or host-runtime limit>`
Source: `<path, generated artifact, command output, or proof gap>`
Action effect: `<what the agent must do, avoid, or verify>`
Update when: `<source or adjacent route changes>`
```

Do not add generic conduct, collaboration, or escalation fields.

## [7][FIRST_SAFE_ACTION]

The first safe action is the smallest real action that proves the agent can operate in the source area without broad edits.

```markdown template
Action: `<single low-risk edit, read-only trace, fixture reproduction, generated-output comparison, or validation dry-run>`
Scope: `<one file, fixture, command, generated artifact, or source slice>`
Forbidden: `<edits, commands, paths, or side effects outside the action>`
Expected result: `<observable output, diff, artifact, or answer>`
Evidence: `<command, diff, generated output, or proof gap>`
```

If the action needs step-by-step instructions, route to a how-to. If the action responds to a failure symptom, route to a runbook.

## [8][VALIDATION]

Validation closes the ramp. The agent may proceed only when the gate passes or the proof gap is explicitly acceptable for the next action.

```markdown template
Gate: `<command, generated comparison, path check, source inspection, or review-free proof>`
Pass signal: `<exit code, output, artifact, diff, or exact observation>`
Failure action: `<fix source, regenerate, route to how-to, route to runbook, or stop>`
Evidence: `<captured result or proof gap>`
```

Keep validation local to the ramp. Broad test portfolio policy belongs to [test-strategy.md](../explanation/test-strategy.md), and evidence mechanics belong to [proof.md](../proof.md).

## [9][STOP_RULES]

Stop rules prevent the ramp from becoming permission theater. State only conditions that force the agent to stop or route away.

```markdown template
Stop when: `<missing source, stale generated artifact, failed gate, unsupported target, destructive side effect, or ambiguous boundary>`
Next route: `<architecture, API, support matrix, how-to, runbook, roadmap, contributing, reference, or README>`
Evidence to preserve: `<diff, command output, artifact path, error, or proof gap>`
```

## [10][BOUNDARIES]

These adjacent standards own routed material:
- [tutorial.md](tutorial.md) teaches a first success; an agent ramp prepares a future agent to operate in one source area.
- [architecture.md](../explanation/architecture.md) carries current structure, dependency direction, and invariants.
- [api.md](../reference/api.md) carries generated or contract-backed API truth.
- [support-matrix.md](../reference/support-matrix.md) carries support status, lifecycle, platform, version, and compatibility truth.
- [how-to.md](../task/how-to.md) carries repeatable competent-reader procedures.
- [runbook.md](../task/runbook.md) carries operational symptom response.
- [contributing.md](../task/contributing.md) carries contribution workflow and pull-request evidence.
- [reference.md](../reference/reference.md) carries standalone lookup facts.
- [README.md](../README.md) carries document-type routing, placement, lifecycle, and split/link decisions.

## [11][CHECKLIST]

Use this checklist before publishing an agent ramp:
- [ ] The ramp names one bounded source area.
- [ ] Source records carry only path, use, status, evidence, and update trigger.
- [ ] Read-first items each answer one pre-action question.
- [ ] Constraints change the first safe action or validation gate.
- [ ] The first safe action is one low-risk action with explicit forbidden scope.
- [ ] Validation has a concrete pass signal and failure action.
- [ ] Stop rules route away instead of adding process metadata.
- [ ] The page follows the strict agent-only metadata ban in [AGENTS.md](../AGENTS.md).

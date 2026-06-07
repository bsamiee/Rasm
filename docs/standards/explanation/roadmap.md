# [ROADMAP_STANDARDS]

A roadmap sequences code work for a concrete route: a repository, package, project, feature folder, integration, migration, refactor, or new module. It carries durable implementation order, milestone umbrellas, task identity, dependency edges, completion proof rules, terminal-work records, and adjacent-document handoffs; it is not a product promise, release-history tail, status diary, or second copy of a live planning system.

The controlling rule: a roadmap helps agents decide which task to take next, which larger outcome that task serves, what must not start yet, what proof closes the task, which sibling documents must move, and which deferred, dropped, or canceled work must not be rediscovered.

## [1][USE_WHEN]

Use a roadmap when a code scope has a coordinated sequence that cannot be understood from architecture, ADRs, design docs, or a live planner alone:
- building a package, project, tool, feature folder, generated contract, integration, or runtime surface;
- sequencing a refactor, collapse, migration, removal, source-backed support window, or stabilization effort;
- coordinating architecture changes with implementation milestones, task records, and proof gates;
- tracking documentation handoffs as code structure, public contracts, tests, tutorials, runbooks, or support policy change;
- assigning durable task IDs before the work can be delegated safely.

Route current structure to [architecture.md](architecture.md), durable decisions to [adr.md](adr.md), pre-code proposal review to [design-doc.md](design-doc.md), gate taxonomy to [test-strategy.md](test-strategy.md), supported-version lifecycle to [support-matrix.md](../reference/support-matrix.md), release history to the project release mechanism, and repeatable procedures to [runbook.md](../task/runbook.md) or how-to/tutorial documents.

[AUTHORING_CONTRACT]:
- Agent use: select the next executable task, understand its milestone umbrella, see blockers, prove task completion, and update adjacent documents only when task output changes them.
- Required produced structure: lead, scope, roadmap contract, lifecycle and IDs, boundaries, milestones with child tasks, terminal work when present, and validation.
- Section cardinality: one scope, one roadmap contract, one lifecycle and ID rule set, one boundaries section before milestones, one milestone section, one validation section, and optional current status, constraints, dependencies, handoffs, work register, or deferred/dropped/canceled work only when referenced.
- Adjacent checks: check architecture, ADR, design, API/reference/code docs, support matrix, test strategy, and task or learning docs only when the adjacent fact changes scope, proof, status interpretation, validation, or handoff.
- Maintenance triggers: changed code scope, milestone outcome, task state, proof requirement, dependency edge, public contract, support row, generated output, live-planner source, or terminal-work reason.

## [2][ROADMAP_CONTRACT]

Use the nearest live source for mutable planning facts and the Markdown roadmap for durable sequencing shape. A live issue, milestone, release, project board, workflow, or planning system carries mutable dates, assignments, comments, and runtime task logs. Repository source, manifests, generated contracts, architecture, ADRs, tests, and release artifacts own proof. The roadmap carries mission, sequence type, milestone umbrellas, task ID policy, task status, dependency edges, documentation handoffs, and closure rules.

When Markdown and a live planner disagree, the live planner controls mutable facts. If no live planner exists, the Markdown roadmap may control task IDs and lifecycle status, but every `[COMPLETE]` claim still needs proof under [proof.md](../proof.md). Runtime todo lists, agent scratch plans, and transient session tasks do not become roadmap IDs unless they are promoted into durable task records with dependency, exit, proof, or handoff fields.

Pick one sequence type in the lead:

| [INDEX] | [TYPE]                   | [SCOPE]                           | [PROOF]                                        |
| :-----: | :----------------------- | :-------------------------------- | :--------------------------------------------- |
|   [1]   | Package build            | package, app, library, or tool    | project file, manifest, gate, generated output |
|   [2]   | Feature build            | feature, command, or workflow     | source path, route, scenario, acceptance gate  |
|   [3]   | Refactor or collapse     | route, surface, path, or edge     | diff, invariant, gate, deleted stale paths     |
|   [4]   | Integration or migration | host, contract, storage, support  | contract, adapter, support row, runtime proof  |
|   [5]   | Hardening sequence       | stability, performance, readiness | strategy gate, benchmark, support, release     |

Use this heading order for a standalone roadmap:

```markdown template
# [<CODE_SCOPE>_ROADMAP]

<Lead: name roadmap state, sequence type, mission, code scope, live planning route, and proof route.>

## [1][SCOPE]

## [2][ROADMAP_CONTRACT]

## [3][LIFECYCLE_IDS]

## [4][BOUNDARIES]

## [5][MILESTONES]

## [6][VALIDATION]
```

Add conditional sections only when they change sequencing or maintenance behavior. `Current status` follows `Roadmap contract`; `Constraints` and `Dependencies and blockers` appear before `Boundaries`; `Documentation handoffs`, `Work register`, and `Deferred dropped canceled work` appear after `Milestones` in that order. Between `Milestones` and final `Validation`, include only task-bearing lifecycle sections so expanding work records do not bury boundaries, proof rules, or authoring guidance.

`No roadmap` is a route-away verdict:
Roadmap: not authored.
Reason: current architecture and accepted ADRs already carry the active route truth; cite `<architecture path>` and `<ADR path when one controls the verdict>`.

## [3][LIFECYCLE_IDS]

`Roadmap state` describes the document as a planning source. Render roadmap state as one bracketed token in the lead:
- `[ACTIVE]`: the roadmap controls the current coordinated sequence.
- `[SNAPSHOT]`: the roadmap summarizes a live planner and every mutable row links to that route.
- `[PAUSED]`: the sequence is intentionally stopped and names the return condition.
- `[CLOSING]`: remaining items are being completed, canceled, or moved.
- `[ARCHIVED]`: the roadmap is not active and points to release history, architecture, ADRs, or the live planner that replaced it.

Roadmap-local task `Status` values use the shared lifecycle vocabulary rendered as bracketed tokens:
- `[QUEUED]`: accepted for sequencing, not yet executing.
- `[ACTIVE]`: active inside the task scope.
- `[BLOCKED]`: a named dependency, decision, access gap, or proof gap prevents progress.
- `[DEFERRED]`: moved outside the current sequence with a return event.
- `[COMPLETE]`: exit is satisfied, proof agrees, and required handoffs are closed or routed away.
- `[DROPPED]`: removed before accepted execution because it is duplicate, invalid, superseded outside this roadmap, or has no remaining dependency or handoff reference.
- `[CANCELED]`: accepted work stopped; successor, rollback, or retired need is stated.

Milestone status is derived from child tasks unless the roadmap declares a real milestone-level lifecycle reason. If shown, milestone status uses the same bracketed tokens and must not contradict child task state. A milestone is `[COMPLETE]` only when its completion rule is satisfied.

Semantic IDs are the default:
- Milestone IDs use `<AREA>-000`, where `<AREA>` is a stable uppercase semantic area for the outcome umbrella.
- Task IDs use `<AREA>-010`, `<AREA>-020`, `<AREA>-030`, with gaps of 10 so new tasks can be inserted without renumbering.
- A milestone ID is never reused after completion, deferral, drop, cancellation, or replacement.
- A task ID is never reused; if work splits, create new task IDs and route or close the old one.
- External IDs from issue trackers or live planners stay in `Live task source`, `Source route`, or `External ID` fields; do not replace local Markdown IDs unless the live planner controls the work.
- Issue, risk, blocker, and proof-gap IDs exist only when dependencies, proof, milestones, tasks, or adjacent documents reference them.

Allowed legacy and local variants:
- `M<N>` may appear only in legacy roadmaps, migration notes, rejected examples, or external live sources that already use that shape.
- `M<N>.T<N>` may remain only when an existing roadmap already depends on those IDs; new roadmap standards use semantic area IDs.
- If tasks are design change slices, route them to the design doc as `S<N>` slices.
- If tasks are proof subchecks for one exit condition, keep them nested under that task's acceptance detail and do not assign separate task IDs.

Progress is optional. Use it only when a maintained actor updates it and the roadmap defines numerator, denominator, closure rule, and proof surface before first use. Default progress is derived from child tasks whose `Status` is `[COMPLETE]` and whose proof agrees. The rendered line is only the 20-cell bar and percentage; put calculation basis in `Completion rule` or a task-progress field, not in milestone proof boilerplate.

## [4][BOUNDARIES]

[EXPLANATION_TYPES]:
- [architecture.md](architecture.md) carries current code structure, project identity, path states, flows, dependency direction, and invariants.
- [adr.md](adr.md) carries durable decisions and their confirmation.
- [design-doc.md](design-doc.md) carries pre-code proposals, risks, validation slices, and accepted approach.
- [test-strategy.md](test-strategy.md) carries gate taxonomy, flake policy, and proof vocabulary.
- [support-matrix.md](../reference/support-matrix.md) carries supported versions, support windows, and lifecycle status.

[TASK_HISTORY_ROUTES]:
- [reference.md](../reference/reference.md) carries lookup facts; release history belongs to the project release mechanism.
- [runbook.md](../task/runbook.md) carries operational procedures and recovery.
- External product roadmap commitments belong to the maintained product planning or release surface; this standard applies only when a code scope needs implementation sequence, task identity, exit proof, dependency edges, or documentation handoffs.
- Research findings, agent scratch plans, and automation spillover stay source evidence until promoted into a milestone, task, terminal-work record, or live-planner route.
- [README.md](../README.md) carries document-type routing, placement, and lifecycle.

## [5][MILESTONES]

A milestone is an umbrella outcome: a larger sequence of tasks under one coherent code or documentation objective. It is not the executable work item. Use H3 milestone anchors when the roadmap is Markdown-controlled or another document links to the milestone.

Milestone fields:
- `ID`: semantic milestone ID ending in `-000`.
- `Status`: optional; omit when status is fully derived from child tasks.
- `Outcome`: large umbrella outcome the milestone completes.
- `Why now`: sequencing pressure or value that makes the outcome relevant.
- `Scope`: paths, concept family, implementation area, generated output, or handoff surface.
- `Included tasks`: child task IDs in intended order.
- `Dependency rule`: what must precede this milestone; omit when absent.
- `Completion rule`: normally all included tasks are `[COMPLETE]`, and required handoffs are closed or routed away.

Task fields:
- `ID`: semantic task ID in the same area as its milestone.
- `Status`: bracketed lifecycle token.
- `Milestone`: parent milestone ID.
- `Work`: one executable work unit.
- `Target`: paths, documents, contracts, commands, generated outputs, or source surfaces touched.
- `Reference material`: source material to read or mine before execution, such as research notes, plan docs, design docs, artifacts, folders, files, or source routes; omit when ordinary target reading is enough.
- `Depends on`: task, milestone, source condition, live route, or proof gate; omit when absent.
- `Exit`: observable completion condition.
- `Proof required`: source, command, generated artifact, link check, review, or stated proof gap required before `[COMPLETE]`.
- `Handoff`: adjacent document route only when this task changes it.

`Reference material` is context, not closure. It does not replace `Target`, `Depends on`, `Proof required`, or `Handoff`; it names material that changes how the task should be interpreted.

Use acceptance checklists inside a task only when multiple checks are inseparable proof conditions. If a checklist item can be delegated independently, split it into its own task ID.

Use this accepted milestone with task records:

```markdown template
### [5.1][<AREA>_POLICY]

ID: <AREA>-000
Outcome: implementation policy is represented as one coherent boundary.
Why now: related implementation rules currently split agent decisions.
Scope: `<policy-doc>`, `<companion-doc>`, `<usage-route>`.
Included tasks: <AREA>-010, <AREA>-020, <AREA>-030.
Completion rule: all included tasks are `[COMPLETE]`, and required handoffs are closed or routed away.

Task:
ID: <AREA>-010
Status: [QUEUED]
Milestone: <AREA>-000
Work: author implementation policy.
Target: `<policy-doc>`, `<readme-route>`.
Reference material: `<research-note>`, `<design-doc>`, `<source-folder>`.
Exit: ownership rules are stated without wrapper leakage or route ambiguity.
Proof required: changed Markdown link check plus source-owner review.
Handoff: README route only when the new page is admitted.
```

Rejected milestone: `M<N> work`; `Goal: add <file>`; `Exit criteria: three anonymous checklist items`; `Proof map: 0/3`.
Reason: the rejected record has a vague ID, makes a task look like a milestone, hides delegateable work in anonymous bullets, and repeats proof arithmetic without improving task selection.

## [6][DEPENDENCIES_HANDOFFS]

Use this conditional section when dependency edges, blockers, or document handoffs change sequence or milestone closure. Omit it when milestone and task fields are enough.

Dependencies are sequencing edges, not task lists. Use the edge table when dependent work cannot start, complete, or prove exit until the required fact changes.

| [INDEX] | [EDGE]                  | [DEPENDENT] | [REQUIRES]  | [RELATION]          | [ROUTE]        | [LIVE]          | [RULE]                       |
| :-----: | :---------------------- | :---------- | :---------- | :------------------ | :------------- | :-------------- | :--------------------------- |
|   [1]   | E-<AREA>-010-<AREA>-020 | <AREA>-020  | <AREA>-010  | blocked by          | internal task  | `<live-source>` | start after policy admission |
|   [2]   | E-<AREA>-030-SUPPORT    | <AREA>-030  | support row | external dependency | support matrix | support row     | remove after support closes  |

Blocker records cover access blockers, proof blockers, decision blockers, and support blockers that are not clean milestone-to-task or task-to-task edges:

```text template
ID: B-<task-id>-N
Status: [BLOCKED]
Blocks: <task ID> exit `<exit name>`
Blocked by: <decision, access, proof gate, support row, or external contract>
Source route: <path, ADR, design, support row, live issue, or gate route>
Go/no-go rule: <binary rule>
Evidence: <current source or stated proof gap>
Close when: <fact changes>
Route-away: <discussion, procedure, or support body that belongs elsewhere>
```

Use a handoff record when task completion changes another document route. Omit absent routes; do not write `none`.

```text template
ID: H-<task-id>-N
Status: [QUEUED]
Task: <task ID>
Destination path: <architecture, ADR, design, API, reference, code documentation, README, how-to, tutorial, runbook, support, or test-strategy path>
Changed fact: `<execution-entrypoint>` becomes the single execution path.
Consumed by: architecture codemap, code-documentation public semantics, and test-strategy gate mapping.
Use in this document: <task ID> cannot close until linked document routes consume or route away the fact.
Update when: execution entrypoint, flow, generated contract, or proof gate changes.
Close when: linked documents are updated or explicitly route the fact away.
Route-away: architecture body, source-comment content, and gate taxonomy stay in their routes.
```

Every `Handoff` listed in a task must have a matching handoff record or an explicit route-away note that explains why the adjacent route does not change.

## [7][WORK_REGISTER]

Use this conditional section only when a roadmap-local issue, risk, task, proof gap, or promoted research finding changes sequence, dependencies, or adjacent updates. Live issue bodies stay in the live planner; the roadmap carries only the fact that changes agent action.

Register kinds define status semantics before records:
- `Kind: issue` uses `OPEN`, `ASSIGNED`, `CLOSED`, `DROPPED`.
- `Kind: risk` uses `OPEN`, `MITIGATED`, `ACCEPTED-AS-RISK`, `CLOSED`, `DROPPED`.
- `Kind: blocker`, `Kind: task`, and `Kind: proof-gap` may use roadmap lifecycle tokens only when they directly block a milestone or task.

Field order is fixed: `ID`, `Kind`, `Status`, `Changed fact`, `Consumed by`, `Use in this document`, `Exit`, `Depends`, `Evidence`, `Update when`, `Close when`, `Route-away`, then local detail fields.

```markdown template
### [7.1][RISK_CONTRACT_DRIFT]

ID: R-<AREA>-010-1
Kind: risk
Status: OPEN
Changed fact: generated contract may change after <AREA>-010 lands.
Consumed by: <AREA>-010 exit and downstream generated-reference task.
Use in this document: <AREA>-010 cannot close until contract drift is either proven absent or routed to a later task.
Exit: contract drift is proven absent, mitigated, or accepted by a successor task.
Evidence: generated contract diff or stated unrun gap.
Update when: contract generator, schema source, or consumer compatibility gate changes.
Close when: contract diff is reviewed or successor task accepts the remaining work.
Route-away: design discussion and issue comments stay in the live planner.
```

## [8][DEFERRED_DROPPED_CANCELED_WORK]

Use this conditional section only when exclusions are not clear enough inside active records or when prior work must remain visible so agents do not recreate it. Keep each record short: preserve the original ID, what the work was, why it left sequence, and how it returns or what supersedes it.

Deferred work states the return event. Dropped work states why the item no longer belongs in this roadmap and removes dependency references to it. Canceled work states successor, rollback, or retired need. Do not split dropped and canceled into separate sections unless another maintained tool consumes that exact split.

```text template
ID: X-<original-id>-<N>
Original ID: <milestone or task ID>
Kind: milestone | task
Status: [DEFERRED] | [DROPPED] | [CANCELED]
Work: <one-line preserved outcome or task>
Reason: <why it left active sequence>
Successor: <replacement task, milestone, live route, ADR, or retired need when relevant>
Return event: <event that moves deferred work back into sequence>
Close when: <references are removed, successor owns it, or external route preserves it>
```

Original IDs are never reused. Do not copy full old task bodies into terminal work unless unique acceptance or proof information would otherwise be lost.

## [9][VALIDATION]

[SCOPE_SEQUENCE]:
- [ ] The roadmap sequences a concrete code scope: repository, package, project, feature folder, integration, migration, refactor, or new module.
- [ ] The lead names roadmap state, sequence type, mission, code scope, live planning route, and proof route.
- [ ] `Scope` states mission, included paths, excluded work, live planner, architecture route, proof route, and release-history route where present.
- [ ] `Roadmap contract` states which live source controls mutable planning facts and which Markdown fields remain durable.
- [ ] Sequence type names the proof emphasis before milestones are written.
- [ ] Boundaries appear before milestones, and only task-bearing lifecycle sections appear after milestones.

[LIFECYCLE_IDS]:
- [ ] Roadmap state and roadmap-local task statuses use bracketed tokens from the closed vocabulary.
- [ ] Milestones are umbrella outcomes and use semantic `-000` IDs.
- [ ] Tasks use semantic zero-padded IDs with gaps of 10 and are never reused.
- [ ] Task status is primary; milestone status is derived or justified by a real milestone-level lifecycle reason.
- [ ] Runtime todos and live-planner tasks are not promoted into roadmap IDs without durable task fields.

[RECORDS_HANDOFFS]:
- [ ] Every milestone names outcome, scope, included tasks, dependency rule when present, and completion rule.
- [ ] Every task names work, target, reference material when task-specific context is required, exit, proof required, dependency when present, and handoff when it changes an adjacent document.
- [ ] `Reference material` names context to read and does not replace target edits, dependency sequencing, completion proof, or adjacent-document handoff.
- [ ] Acceptance checklists appear inside a task only when their checks are inseparable; delegateable checks have task IDs.
- [ ] Dependencies use edge records when field-level dependency references are not enough.
- [ ] Handoffs use the shared relation fields and appear only when task completion changes another document route.
- [ ] Deferred, dropped, and canceled records preserve original ID, kind, reason, successor or return event when relevant, and close condition.
- [ ] A task is `[COMPLETE]` only when exit, proof required, and required handoffs agree.

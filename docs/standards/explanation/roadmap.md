# [ROADMAP_STANDARDS]

A roadmap document is the maintained sequence for planned work inside one scope. It states current position, the structural vocabulary, active milestone bodies, terminal work, boundaries, and validation. It is not an architecture document, design proposal, release log, issue tracker mirror, changelog, or session report.

The controlling rule: roadmap hierarchy is data, not section structure. Produced roadmap files use H2 sections only for page regions. Milestones are cohesive bodies, phases are nested list rows inside a milestone, and tasks are checkbox rows inside a phase.

## [1][USE_WHEN]

Use a roadmap when future agents need to understand ordered implementation work:
- active build, refactor, migration, integration, hardening, or documentation sequence;
- current task position and proof route;
- milestone and phase progress derived from task completion;
- task-level references, dependencies, proof requirements, and handoff routes;
- deferred, dropped, or canceled work that still affects active sequence or future cleanup.

Route current structure to [architecture.md](architecture.md), ambiguous approach or task-scale specification detail to [design-doc.md](design-doc.md), accepted durable decisions to [adr.md](adr.md), gate taxonomy to [test-strategy.md](test-strategy.md), operational response to [runbook.md](../task/runbook.md), and release chronology to the project's release route. Link an adjacent document only when it changes current task action, proof, status interpretation, or maintenance.

[AUTHORING_CONTRACT]:
- Agent use: read current position, execute the first active task whose dependencies and proof route are satisfied, update task status from evidence, and keep milestone or phase progress derived from child tasks.
- Required produced structure: lead, `Current position`, `Structural components`, `Boundaries`, `Active work`, optional `Terminal work`, and `Validation`.
- Section cardinality: one current-position section, one structural-components section, one boundaries section, one active-work section, at most one terminal-work section, and one validation section.
- Adjacent checks: architecture for current structure, design or `SPEC.<slug>.md` for ambiguous task detail, proof owners for completion evidence, and live planning route only when mutable planning data is owned outside Markdown.
- Maintenance triggers: task status changes, proof route changes, active task insertion, milestone or phase split, dependency change, terminal disposition, adjacent spec move, or live planning route change.

## [2][PLANNING_FOLDER]

Roadmaps may live in a scope-local `.planning/` folder when the scope needs planning files without mixing planned sequence into ordinary README or architecture routes.

```text conceptual
<scope>/.planning/
├── ARCHITECTURE.md
├── README.md
├── ROADMAP.md
├── SPEC.<slug>.md
└── SPEC.<slug>.md
```

Keep `.planning/` flat by default. `README.md` appears only when 2 or more planning files need routing. `SPEC.<slug>.md` is the spec-sheet carrier governed by [design-doc.md](design-doc.md) when it carries ambiguity, alternatives, risks, slices, or proof planning. Do not create `.planning/specs/` unless many active specs or tooling justify the extra lookup layer.

The `.planning/` folder is scope-local: a root `.planning/` covers the repository or workspace, and a nested `.planning/` covers only the directory or package boundary that contains it.

## [3][PRODUCED_STRUCTURE]

Produced roadmap files use this page outline. Do not add H3 or H4 headings to represent milestones, phases, tasks, deferred groups, or task detail.

```markdown template
# [<CODE_SCOPE>_ROADMAP]

[ACTIVE] <sequence type> for `<code-scope>`: <mission>. Current position: T-0010 is active in P-0010 under M-0010. Proof route: <source, command, artifact, review, or proof gap>. Live planning route: Markdown-controlled.

## [1][CURRENT_POSITION]

## [2][STRUCTURAL_COMPONENTS]

## [3][BOUNDARIES]

## [4][ACTIVE_WORK]

## [5][TERMINAL_WORK]

## [6][VALIDATION]
```

Omit `Terminal work` when no deferred, dropped, or canceled item remains useful. Keep produced H2 labels exact; H2s are page regions only.

## [4][CURRENT_POSITION]

`Current position` carries the minimum state needed to resume work without scanning every task field first.

```markdown template
## [1][CURRENT_POSITION]

State: [ACTIVE]
Sequence type: <Package build | Feature build | Refactor or collapse | Integration or migration | Hardening sequence>
Current focus: T-0010 in P-0010 under M-0010.
Progress basis: <N>/<N> tasks; counted only when task `Status` is `[COMPLETE]` and proof agrees.
Progress: [████░░░░░░░░░░░░░░░░] 20%
Proof route: <source, command, artifact, review, or proof gap>.
Live planning route: Markdown-controlled.
```

`State` uses the roadmap lifecycle vocabulary:
- `[ACTIVE]`: active task sequence; agents may execute the next task whose dependencies are satisfied.
- `[PAUSED]`: sequence is preserved, but execution is intentionally stopped until the named return event.
- `[SNAPSHOT]`: recorded for state transfer; another route owns current execution.
- `[CLOSING]`: active work is complete and remaining edits are proof, handoff, cleanup, or archival.
- `[ARCHIVED]`: no active work remains; keep only when the roadmap still explains a maintained route.

`Sequence type` describes the work family; it is not a status. Use the listed values unless the roadmap standard is updated with another reusable family.

`Proof route` appears only when it names the proof class that closes the current task set: source review, generated artifact, link check, command gate, external review, or proof gap. Do not copy full validation ladders into the current-position block.

`Live planning route` appears when mutable comments, dates, assignments, runtime logs, issue threads, boards, milestones, or workflow state are owned outside Markdown. Use `Markdown-controlled` when no external live planning route exists.

Do not add `Next executable task` or `Blocked by`. The next task is inferred from active task order, `Status`, `Depends on`, and proof requirements.

## [5][STRUCTURAL_COMPONENTS]

Roadmap IDs are neutral and scope-local:
- `M-0010`: milestone.
- `P-0010`: phase.
- `T-0010`: task.

Start at `0010` and increment by 10. Do not encode language, package, layer, component, owner, date, status, or semantic prefixes into the ID. Put those facts in the relevant field only when they change task action.

Only tasks carry lifecycle status. Milestones and phases carry derived progress only.

[TASK_STATUS]:
- `[QUEUED]`: ready to remain in the sequence, but not active.
- `[ACTIVE]`: current execution target or explicitly in progress.
- `[BLOCKED]`: cannot proceed until the `Depends on` or proof gate resolves.
- `[DEFERRED]`: removed from active execution but returnable through terminal work.
- `[COMPLETE]`: exit condition is satisfied and proof agrees.
- `[DROPPED]`: no longer wanted.
- `[CANCELED]`: superseded or invalidated by a later route.

Active milestones use these fields:
- `ID`
- `Outcome`
- `Completion rule`
- `Progress basis`
- `Progress`
- `Phases`

Active phases use these fields:
- `ID`
- `Outcome`
- `Scope`
- `Depends on`
- `Completion rule`
- `Progress basis`
- `Progress`
- `Tasks`

Active tasks use this row and field vocabulary:
- Row: `- [ ] T-0010 [QUEUED] <concise executable title>`
- `Work`
- `Target`
- `Reference material`
- `Depends on`
- `Exit`
- `Proof required`
- `Handoff`

Task row `ID` and `Status` are encoded in the checkbox row. Do not repeat `ID:` or `Status:` as nested task fields unless a parser or local tool requires those labels.

Omit absent optional fields instead of writing `none`, `n/a`, empty placeholders, or filler records. Required active-task fields are `Work`, `Target`, `Exit`, and `Proof required`. `Reference material`, `Depends on`, and `Handoff` appear only when they change execution.

`Reference material` points to material an agent should read before execution when ordinary target-file reading is not enough. Use it for `SPEC.<slug>.md#<anchor>`, architecture sections, design sections, generated artifacts, or source routes that change the task. Do not use it as a link dump.

`Proof required` names the proof class that can close the task. It may name a proof gap, but the task cannot become `[COMPLETE]` until proof agrees.

## [6][ACTIVE_WORK]

`Active work` contains one or more milestone bodies. A roadmap may have one milestone. Each milestone remains a cohesive body; do not promote phases, tasks, or task fields to headings.

```markdown template
## [4][ACTIVE_WORK]

Milestone:
- ID: M-0010
- Outcome: <umbrella implementation outcome>.
- Completion rule: all child tasks are `[COMPLETE]`, and required handoffs are closed or routed away.
- Progress basis: complete child tasks over all tasks in child phases.
- Progress: [░░░░░░░░░░░░░░░░░░░░] 0%
- Phases:
    - P-0010: <phase outcome>.
        - Scope: `<path-or-surface>`, `<contract>`.
        - Completion rule: all child tasks are `[COMPLETE]`.
        - Progress basis: complete child tasks over this phase's tasks.
        - Progress: [░░░░░░░░░░░░░░░░░░░░] 0%
        - Tasks:
            - [ ] T-0010 [QUEUED] <concise executable title>
                - Work: <one executable work unit>.
                - Target: `<path>`, `<document>`, `<contract>`.
                - Reference material: `SPEC.<slug>.md#<anchor>`.
                - Depends on: <task, phase, source condition, or proof gate>.
                - Exit: <observable completion condition>.
                - Proof required: <source, command, artifact, review, or proof gap>.
                - Handoff: <adjacent route>.
            - [ ] T-0020 [QUEUED] <second executable title>
                - Work: <one executable work unit>.
                - Target: `<path>`.
                - Exit: <observable completion condition>.
                - Proof required: <source, command, artifact, review, or proof gap>.
    - P-0020: <phase outcome>.
        - Scope: `<path-or-surface>`.
        - Depends on: P-0010.
        - Completion rule: all child tasks are `[COMPLETE]`.
        - Progress basis: complete child tasks over this phase's tasks.
        - Progress: [░░░░░░░░░░░░░░░░░░░░] 0%
        - Tasks:
            - [ ] T-0030 [QUEUED] <third executable title>
                - Work: <one executable work unit>.
                - Target: `<path>`.
                - Exit: <observable completion condition>.
                - Proof required: <source, command, artifact, review, or proof gap>.
```

Use `-` for phase rows and task-field rows. Use `- [ ]` or `- [x]` only for task rows. Indent each child level by four spaces. Do not insert blank lines inside one milestone tree.

Milestone progress is complete child tasks over all tasks in all child phases. Phase progress is complete child tasks over tasks in that phase. A task counts complete only when its row status is `[COMPLETE]` and proof agrees. Milestone and phase status fields are rejected because progress already derives their state.

Multiple milestone bodies may appear in `Active work` only when each milestone has a distinct umbrella outcome. Do not split one milestone into multiple bodies merely to shorten the page.

## [7][TERMINAL_WORK]

`Terminal work` preserves deferred, dropped, or canceled work only when deleting it would lose useful sequencing, successor, or return information. It is one cohesive body and never uses headings to nest milestone, phase, or task hierarchy.

```markdown template
## [5][TERMINAL_WORK]

Terminal item:
- ID: X-<original-id>-<N>
- Original ID: <milestone, phase, or task ID>
- Kind: milestone | phase | task
- Status: [DEFERRED] | [DROPPED] | [CANCELED]
- Work: <one-line preserved outcome or task>.
- Reason: <why it left active sequence>.
- Successor: <replacement route>.
- Return event: <required for `[DEFERRED]`>.
- Close when: <references are removed, successor owns it, or external route preserves it>.
- Preserved structure:
    - Phase P-0010: <phase outcome>.
        - Task T-0010: <task title>; <terminal disposition or successor>.
        - Task T-0020: <task title>; <terminal disposition or successor>.
    - Task T-0030: <task title>; use directly when `Kind: task`.
```

`Original ID` keeps the active-sequence reference stable after removal. `ID` starts with `X-` so terminal records cannot be confused with active milestone, phase, or task IDs.

For `Kind: milestone`, preserve relevant phase and task rows under `Preserved structure`. For `Kind: phase`, preserve relevant task rows. For `Kind: task`, use the direct task row and omit phase rows. Omit `Successor` when absent. `Return event` is required only for `[DEFERRED]`.

Delete terminal work when successor routes own the information, all references are removed, or the item no longer changes future task action.

## [8][BOUNDARIES]

[EXPLANATION_TYPES]:
- [architecture.md](architecture.md) carries current structure, current codemaps, invariants, dependency direction, and planned architecture only under the planning-architecture rules.
- [design-doc.md](design-doc.md) carries ambiguous approach, alternatives, risk records, change slices, proof planning, and `SPEC.<slug>.md` spec sheets when task detail outgrows the roadmap.
- [adr.md](adr.md) carries accepted durable decisions.
- [test-strategy.md](test-strategy.md) carries reusable gate taxonomy.

[REFERENCE_TASK_LEARNING]:
- [README.md](../README.md) carries document-type routing, placement, and lifecycle.
- [readme.md](../reference/readme.md) carries scope entrypoints and navigation.
- [runbook.md](../task/runbook.md) carries operational recovery.
- API, reference, and code-documentation routes carry lookup and generated contract facts.

[ROADMAP_BOUNDARIES]:
- Current structure routes to architecture.
- Ambiguous approach or reusable task-scale detail routes to `SPEC.<slug>.md`.
- Mutable comments, dates, assignments, issue discussion, and runtime logs route to the live planning route when one exists.
- Completed chronology routes to release notes, changelog, ADR, source proof, or history only when those routes exist and change reader action.

## [9][VALIDATION]

[CURRENT_POSITION]:
- [ ] The lead and current-position section name state, sequence type, current focus, progress basis, progress, proof route when useful, and live planning route.
- [ ] There is no `Next executable task` or `Blocked by` field.
- [ ] `Proof route` names only the route-level proof class for the current task set.
- [ ] `Live planning route` is `Markdown-controlled` unless an external or local live planning system owns mutable planning data.

[STRUCTURE]:
- [ ] Produced H2s are page regions only: `Current position`, `Structural components`, `Boundaries`, `Active work`, optional `Terminal work`, and `Validation`.
- [ ] No H3 or H4 heading represents a milestone, phase, task, terminal group, or task-detail body.
- [ ] IDs use neutral `M-0010`, `P-0010`, and `T-0010` shapes without semantic prefixes.
- [ ] Only task rows carry lifecycle status.
- [ ] Milestone and phase progress derive from child task completion and use the 20-cell progress bar.

[ACTIVE_WORK]:
- [ ] Each milestone is one cohesive body with `ID`, `Outcome`, `Completion rule`, `Progress basis`, `Progress`, and `Phases`.
- [ ] Each phase is a nested `- P-0010: <outcome>` row with `Scope`, optional `Depends on`, `Completion rule`, `Progress basis`, `Progress`, and `Tasks`.
- [ ] Each task is a checkbox row with neutral ID, bracketed status, concise executable title, and nested task fields.
- [ ] Task fields use only `Work`, `Target`, `Reference material`, `Depends on`, `Exit`, `Proof required`, and `Handoff`.
- [ ] Optional fields are omitted when absent; no filler `none`, `n/a`, or placeholder-only rows remain.
- [ ] Task `Reference material` points only to material that changes execution.

[TERMINAL_WORK]:
- [ ] Terminal items use a cohesive body with `ID`, `Original ID`, `Kind`, `Status`, `Work`, `Reason`, conditional `Successor`, conditional `Return event`, `Close when`, and `Preserved structure`.
- [ ] Terminal `Kind` determines whether preserved structure carries phase rows, task rows, or one direct task row.
- [ ] Deferred work names a return event; dropped or canceled work names a reason and close condition.

[BOUNDARIES]:
- [ ] Current structure routes to architecture instead of roadmap prose.
- [ ] Ambiguous approach or reusable task detail routes to `SPEC.<slug>.md`.
- [ ] Mutable planning data routes to the live planning route instead of duplicating in Markdown.

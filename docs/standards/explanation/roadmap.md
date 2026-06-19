# [ROADMAP_STANDARDS]

A roadmap document is the maintained sequence for target work inside one scope. It states current position, the structural vocabulary, active milestone bodies, terminal work, boundaries, and confirmation. It is not an architecture document, design proposal, release log, issue tracker mirror, changelog, or session report.

The controlling rule: roadmap hierarchy is data, not section structure. Produced roadmap files use H2 sections only for page regions. Milestones are cohesive bodies, phases are nested list rows inside a milestone, and tasks are checkbox rows inside a phase.

## [01]-[USE_WHEN]

Use a roadmap when future agents need to understand ordered implementation work:
- active build, refactor, migration, integration, hardening, or documentation sequence;
- current task position and confirmation route;
- milestone and phase progress derived from task completion;
- task-level references, dependencies, confirmation requirements, and handoff routes;
- returnable, dropped, or canceled work that still affects active sequence or future cleanup.

Route current structure to [architecture.md](architecture.md), ambiguous approach or task-scale specification detail to [design-doc.md](design-doc.md), accepted durable decisions to [adr.md](adr.md), gate taxonomy to [test-strategy.md](test-strategy.md), operational response to [runbook.md](../task/runbook.md), and release chronology to the project's release route. Link an adjacent document only when it changes current task action, confirmation, status interpretation, or maintenance.

[AUTHORING_CONTRACT]:
- Agent use: read current position, execute the first active task whose dependencies and confirmation route are satisfied, update task status from confirmation, and keep milestone or phase progress derived from child tasks.
- Required produced structure: lead, `Current position`, `Structural components`, `Boundaries`, `Active work`, optional `Terminal work`, and `Result check`.
- Section cardinality: one current-position section, one structural-components section, one boundaries section, one active-work section, at most one terminal-work section, and one confirmation section.
- Adjacent checks: architecture for current structure, design or `SPEC.<slug>.md` for ambiguous task detail, confirmation owners for completion confirmation, and live target route only when mutable target sequencing data is owned outside Markdown.
- Maintenance triggers: task status changes, confirmation route changes, active task insertion, milestone or phase split, dependency change, terminal disposition, adjacent spec move, or live target route change.

## [02]-[PLANNING_FOLDER]

Roadmaps may live in a scope-local `.planning/` folder when the scope needs target files without mixing target sequence into ordinary README or architecture routes.

```text conceptual
<scope>/.planning/
├── ARCHITECTURE.md
├── README.md
├── ROADMAP.md
├── SPEC.<slug>.md
└── SPEC.<slug>.md
```

Keep `.planning/` flat by default. `README.md` appears only when 2 or more target files need routing. `SPEC.<slug>.md` is the spec-sheet carrier governed by [design-doc.md](design-doc.md) when it carries ambiguity, alternatives, risks, slices, or confirmation target sequencing. Do not create `.planning/specs/` unless many active specs or tooling justify the extra lookup layer.

The `.planning/` folder is scope-local: a root `.planning/` covers the repository or workspace, and a nested `.planning/` covers only the directory or package boundary that contains it.

## [03]-[PRODUCED_STRUCTURE]

Produced roadmap files use this page outline. Do not add H3 or H4 headings to represent milestones, phases, tasks, returnable groups, or task detail.

```markdown template
# [<CODE_SCOPE>_ROADMAP]

[ACTIVE] <sequence type> for `<code-scope>`: <mission>. Current position: T-0010 is active in P-0010 under M-0010. Confirmation route: <source, command, artifact, review, or confirmation gap>. Live target route: Markdown-controlled.

## [1]-[CURRENT_POSITION]

## [2]-[STRUCTURAL_COMPONENTS]

## [3]-[BOUNDARIES]

## [4]-[ACTIVE_WORK]

## [5]-[TERMINAL_WORK]

## [4]-[CURRENT_POSITION]

`Current position` carries the minimum state needed to resume work without scanning every task field first.

```markdown template
## [01]-[CURRENT_POSITION]

State: [ACTIVE]
Sequence type: <Package build | Feature build | Refactor or collapse | Integration or migration | Hardening sequence>
Current focus: T-0010 in P-0010 under M-0010.
Progress basis: <N>/<N> tasks; counted only when task `Status` is `[COMPLETE]` and confirmation agrees.
Progress: [████░░░░░░░░░░░░░░░░] 20%
Confirmation route: <source, command, artifact, review, or confirmation gap>.
Live target route: Markdown-controlled.
```

`State` uses the roadmap lifecycle vocabulary:
- `[ACTIVE]`: active task sequence; agents may execute the next task whose dependencies are satisfied.
- `[PAUSED]`: sequence is preserved, but execution is intentionally stopped until the named return event.
- `[SNAPSHOT]`: recorded for state transfer; another route owns current execution.
- `[CLOSING]`: active work is complete and remaining edits are confirmation, handoff, cleanup, or archival.
- `[ARCHIVED]`: no active work remains; keep only when the roadmap still explains a maintained route.

`Sequence type` describes the work family; it is not a status. Use the listed values unless the roadmap standard is updated with another reusable family.

`Confirmation route` appears only when it names the confirmation class that closes the current task set: source review, generated artifact, link check, command gate, external review, or confirmation gap. Do not copy full confirmation ladders into the current-position block.

`Live target route` appears when mutable comments, dates, assignments, runtime logs, issue threads, boards, milestones, or workflow state are owned outside Markdown. Use `Markdown-controlled` when no external live target route exists.

Do not add `Next executable task` or `Blocked by`. The next task is inferred from active task order, `Status`, `Depends on`, and confirmation requirements.

## [5]-[STRUCTURAL_COMPONENTS]

Roadmap IDs are neutral and scope-local:
- `M-0010`: milestone.
- `P-0010`: phase.
- `T-0010`: task.

Start at `0010` and increment by 10. Do not encode language, package, layer, component, owner, date, status, or semantic prefixes into the ID. Put those facts in the relevant field only when they change task action.

Only tasks carry lifecycle status. Milestones and phases carry derived progress only.

[TASK_STATUS]:
- `[QUEUED]`: ready to remain in the sequence, but not active.
- `[ACTIVE]`: current execution target or explicitly in progress.
- `[BLOCKED]`: cannot proceed until the `Depends on` or confirmation gate resolves.
- `[RETURNABLE]`: removed from active execution but returnable through terminal work.
- `[COMPLETE]`: exit condition is satisfied and confirmation agrees.
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
- `Confirmation required`
- `Handoff`

Task row `ID` and `Status` are encoded in the checkbox row. Do not repeat `ID:` or `Status:` as nested task fields unless a parser or local tool requires those labels.

Omit absent optional fields instead of writing `none`, `n/a`, empty placeholders, or filler records. Required active-task fields are `Work`, `Target`, `Exit`, and `Confirmation required`. `Reference material`, `Depends on`, and `Handoff` appear only when they change execution.

`Reference material` points to material an agent should open before execution when ordinary target-file reading is not enough. Use it for `SPEC.<slug>.md#<anchor>`, architecture sections, design sections, generated artifacts, or owner routes that change the task. Do not use it as a link dump.

`Confirmation required` names the confirmation class that can close the task. It may name a confirmation gap, but the task cannot become `[COMPLETE]` until confirmation agrees.

## [6]-[ACTIVE_WORK]

`Active work` contains one or more milestone bodies. A roadmap may have one milestone. Each milestone remains a cohesive body; do not promote phases, tasks, or task fields to headings.

```markdown template
## [04]-[ACTIVE_WORK]

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
                - Depends on: <task, phase, source condition, or confirmation gate>.
                - Exit: <observable completion condition>.
                - Confirmation required: <source, command, artifact, review, or confirmation gap>.
                - Handoff: <adjacent route>.
            - [ ] T-0020 [QUEUED] <second executable title>
                - Work: <one executable work unit>.
                - Target: `<path>`.
                - Exit: <observable completion condition>.
                - Confirmation required: <source, command, artifact, review, or confirmation gap>.
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
                - Confirmation required: <source, command, artifact, review, or confirmation gap>.
```

Use `-` for phase rows and task-field rows. Use `- [ ]` or `- [x]` only for task rows. Indent each child level by four spaces. Do not insert blank lines inside one milestone tree.

Milestone progress is complete child tasks over all tasks in all child phases. Phase progress is complete child tasks over tasks in that phase. A task counts complete only when its row status is `[COMPLETE]` and confirmation agrees. Milestone and phase status fields are rejected because progress already derives their state.

Multiple milestone bodies may appear in `Active work` only when each milestone has a distinct umbrella outcome. Do not split one milestone into multiple bodies merely to shorten the page.

## [7]-[TERMINAL_WORK]

`Terminal work` preserves returnable, dropped, or canceled work only when deleting it would lose useful sequencing, successor, or return information. It is one cohesive body and never uses headings to nest milestone, phase, or task hierarchy.

```markdown template
## [05]-[TERMINAL_WORK]

Terminal item:
- ID: X-<original-id>-<N>
- Original ID: <milestone, phase, or task ID>
- Kind: milestone | phase | task
- Status: [RETURNABLE] | [DROPPED] | [CANCELED]
- Work: <one-line preserved outcome or task>.
- Reason: <why it left active sequence>.
- Successor: <replacement route>.
- Return event: <required for `[RETURNABLE]`>.
- Close when: <references are removed, successor owns it, or external route preserves it>.
- Preserved structure:
    - Phase P-0010: <phase outcome>.
        - Task T-0010: <task title>; <terminal disposition or successor>.
        - Task T-0020: <task title>; <terminal disposition or successor>.
    - Task T-0030: <task title>; use directly when `Kind: task`.
```

`Original ID` keeps the active-sequence reference stable after removal. `ID` starts with `X-` so terminal records cannot be confused with active milestone, phase, or task IDs.

For `Kind: milestone`, preserve relevant phase and task rows under `Preserved structure`. For `Kind: phase`, preserve relevant task rows. For `Kind: task`, use the direct task row and omit phase rows. Omit `Successor` when absent. `Return event` is required only for `[RETURNABLE]`.

Delete terminal work when successor routes own the information, all references are removed, or the item no longer changes future task action.

## [8]-[BOUNDARIES]

[EXPLANATION_TYPES]:
- [architecture.md](architecture.md) carries current structure, current codemaps, invariants, dependency direction, and target architecture only under the target-architecture rules.
- [design-doc.md](design-doc.md) carries ambiguous approach, alternatives, risk records, change slices, confirmation target sequencing, and `SPEC.<slug>.md` spec sheets when task detail outgrows the roadmap.
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
- Mutable comments, dates, assignments, issue discussion, and runtime logs route to the live target route when one exists.
- Completed chronology routes to release notes, changelog, ADR, source confirmation, or history only when those routes exist and change reader action.

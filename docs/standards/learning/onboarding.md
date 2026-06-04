# [ONBOARDING_STANDARDS]

An onboarding document prepares one person to hold one role through guided reading, supervised practice, progressive access, and observable readiness gates. Lead it with the role, the observable readiness target, and the ramp horizon; close it with the owner-recorded authority transition and remaining limits. Onboarding prepares people for responsibility. It is not a product lesson, a normal task procedure, an incident recovery path, a contribution workflow, or a lookup catalog.

## [1][USE_WHEN]

Use onboarding when a named person must become ready to hold a role that carries standing responsibility:

- contribution authority on a subsystem;
- maintainership, review, or merge authority;
- operational or on-call production responsibility;
- support or cross-functional review authority over work they do not own.

Route a single product lesson to tutorial, a one-time competent-reader task to how-to, incident recovery to runbook, the shared contribution workflow to contributing, and standalone facts to reference. When a draft needs two separate readiness targets, split it by role ramp before writing.

## [2][RAMP_BASELINES]

Use the first source that decides a readiness claim. Repository truth owns every factual claim, command, permission, grant, owner, source path, and sign-off; external practice supports reusable ramp behavior only.

1. Current repository source, manifests, generated contracts, command output, permission grants, and recorded sign-off.
2. This onboarding standard for ramp shape, gate structure, and required sections.
3. External onboarding practice for learning-checklist, shadowing, feedback, and readiness-measurement patterns.
4. DORA only for application or service delivery-flow metrics when the ramp explicitly measures software delivery performance.

Keep each external source scoped to the behavior it supports:

- External evidence: [Google SRE on-call onboarding](https://sre.google/sre-book/accelerating-sre-on-call/) supports ordered learning, on-call learning checklists, `Know before moving on` gates, comprehension questions, shadow and reverse-shadow practice, mentor review, and documentation apprenticeship.
- External evidence: [SHRM onboarding success metrics](https://www.shrm.org/topics-tools/topics/onboarding/measuring-success) supports feedback and success measures such as time to productivity, retention, new-hire surveys, employee satisfaction and engagement, performance measures, and informal feedback.
- Local rule: 30/60/90-style cadence is a scheduling convention for checkpoints and phase labels, not source authority for engineering readiness.
- Local rule: role profiles, proof fields, access progression, and sign-off policy come from this standard and the instantiated repository source of truth, not from external onboarding practice.
- [DORA metrics](https://dora.dev/guides/dora-metrics/) apply only when the ramp uses application or service delivery-flow measures such as change lead time, deployment frequency, failed deployment recovery time, change fail rate, or deployment rework rate. DORA is not a general readiness model, and first merge, first supervised production action, gate completion, and owner sign-off are local onboarding outcome metrics.

Source of truth: repository source, access records, commands, permission grants, and recorded sign-off for instantiated ramps; linked external sources for ramp-shape evidence only.
Last verified: 2026-06-04.
Review trigger: Google SRE onboarding guidance, SHRM onboarding measurement guidance, DORA metric definitions, or local access/sign-off policy changes.

## [3][ROLE_RAMP_PROFILES]

Pick exactly one primary ramp; each ramp changes the readiness target, the access stage, the practice activities, and the sign-off owner. Split the document when two ramps need distinct readiness criteria rather than widening one document to cover both.

| [INDEX] | [RAMP]           | [READINESS_TARGET]         | [PRACTICE]              | [ACCESS_STAGE]                  | [SIGN_OFF]      |
| :-----: | :--------------- | :------------------------- | :---------------------- | :------------------------------ | :-------------- |
|   [1]   | Contributor      | first owner-reviewed merge | build, change, PR       | read access to reviewed change  | subsystem owner |
|   [2]   | Maintainer       | triage/review authority    | triage, review, release | triage access to scoped approve | maintainer lead |
|   [3]   | Operator         | scoped production action   | shadow, reverse-shadow  | observe access to action grant  | on-call lead    |
|   [4]   | Cross-functional | bounded support or review  | review tasks, triage    | read access to task-tool action | reviewing owner |

These access stages are abstract. An instantiated ramp replaces them with current permission records, access surfaces, and sign-off sources in opening metadata and provisioning records. A ramp that grants production action or merge rights must define supervised practice before that grant. A cross-functional ramp must name the single review or support task it ends on, so readiness stays bounded.

## [4][PLACEMENT]

Route document-type, placement, and lifecycle questions to the standards index by topic. Place onboarding beside the role owner that can refresh it when the role, learning path, role-learning map, exercises, or readiness criteria move. Do not invent a corpus-wide onboarding path unless the repository already maintains that path.

## [5][REQUIRED_STRUCTURE]

Copy only the required skeleton below. Add conditional sections after the required section they support, and omit them when their condition is false. Keep the document as small as the role readiness target allows; a section that does not change readiness, proof, access, or sign-off is filler.

```markdown template
# [ROLE_ONBOARDING]

Role: <single role this ramp confers>
Owner: <refresh owner role>
Ramp horizon: <expected duration>
Source of truth: <role, access, and sign-off record path or owner-maintained source>
Last reviewed: YYYY-MM-DD
Review trigger: <event that makes this ramp stale>

<Lead: the role, readiness target, ramp horizon, authority transition, and remaining limits.>

## [1][AUDIENCE]

## [2][READINESS_TARGET]

## [3][PROVISIONING_PREREQUISITES]

## [4][LEARNING_PRACTICE]

## [5][READINESS_CRITERIA]

## [6][OWNER_ROLES]

## [7][BOUNDARIES]

## [8][REVIEW_CHECKLIST]
```

Section cardinality:

**Identity and setup**
- Opening metadata: required, one; holds `Role`, `Owner`, `Ramp horizon`, `Source of truth`, `Last reviewed`, and `Review trigger`.
- Audience: required, one; holds the single role and prior knowledge assumed before opening the ramp.
- Readiness target: required, one; holds the observable end-state and ramp horizon.
- Provisioning and prerequisites: required, one; holds status-tracked, dated access gates plus reading needed before item one.

**Learning and practice**
- Learning and practice: required, one; holds the ordered learning checklist and any supervised practice needed for the readiness target.
- Readiness criteria: required, repeatable checklist items or H3 records; holds observable gates ending in the recorded transition.

**Ownership and closure**
- Owner roles: required, one; holds buddy, reviewer, and sign-off owner.
- Boundaries: required, one; holds one route-away link per adjacent owner.
- Review checklist: required, one; holds observable author self-checks.

Conditional additions:

- `Role-learning map`: required when the role crosses multiple owner boundaries, external systems, failure modes, or critical-path components. Omit only when source paths and learning items already make the role boundary obvious.
- `Ramp phases`: required when the ramp lasts longer than one review cycle, has staged access, or uses 30/60/90-style checkpoints.
- `First safe tasks`: required when readiness depends on supervised work, fresh-eyes documentation stewardship, or a low-blast-radius ownership trial.
- `Shadowing and review path`: required when elevated authority, production access, user impact, merge rights, release authority, or cross-functional review authority transfers. Omit only when first safe tasks provide the complete supervised-practice gate and no elevated authority transfers.
- `Feedback and refresh`: required when the ramp is reused, when external onboarding practice is cited, or when the role owner wants learner feedback to update the ramp.
- `Ramp flow`: optional; add only when branch states, access transitions, or authority gates are hard to recover from records alone.
- `Examples`: avoid a late examples section by default. Place an accepted/rejected example beside the rule it clarifies; add a section only when several rules need examples and each example cannot sit near its rule.

Generated onboarding documents must not include empty conditional headings. Add a conditional section only when its stated condition is true; otherwise keep the ramp on the required spine and let adjacent owners carry their own document types.

The compact micro-ramp below shows how the required and conditional pieces interlock without becoming a task guide or architecture page:

```markdown conceptual
Role-learning map: request path enters `<api-path>`, crosses `<owner-boundary>`, and fails safe at `<failure-mode>`.
Learning item: trace that path aloud from `<architecture-path>` and answer the owner question about `<critical-component>`.
First safe task: update one stale role-facing note under reviewer supervision; procedure owner is `<how-to-or-reference-path>`.
Sign-off gate: owner records that the trace and reviewed task passed, with remaining limits in `<sign-off-record>`.
```

## [6][AUDIENCE]

Name the single role this ramp confers, matching opening metadata `Role` and the readiness-target end-state. Then list only the prior knowledge the reader is assumed to hold before opening the document, using canonical repository source paths where they exist. Reading required before learning-checklist item one belongs in `Provisioning and prerequisites`.

```markdown template
This ramp confers: Contributor on <subsystem>.
Assumes prior knowledge:
- <source-path> — <why this source is prerequisite knowledge>
- <source-path> — <why this source is prerequisite knowledge>
```

## [7][READINESS_TARGET]

State the readiness target as the observable end-state that confers authority, paired with opening metadata `Ramp horizon` so the ramp is time-boxed rather than open-ended. Measure the ramp by outcomes such as time to first owner-reviewed merge, time to first supervised production action, gate completion, or owner-recorded sign-off. Do not use activity metrics such as commit count, message volume, or hours logged as readiness proof.

## [8][PROVISIONING_PREREQUISITES]

Split this section into a tracked provisioning gate and the reading required before learning-checklist item one. Provisioning is a dated, owned gate when the role depends on tools or permissions, because onboarding must provide the access and information needed to become productive.

Render provisioning as status-tagged H3 records so an owner can filter state and update dates independently. Each grant carries `Access`, `Status` from `PLANNED`, `IN-PROGRESS`, `BLOCKED`, `DONE`, `DROPPED`, `Due before`, `Completed` when closed, `Unlocks`, and `Owner`. Access widens only as the prior gate closes.

```markdown template
### [N.M][READ_ACCESS]

Access: <read-only repository, dashboard, or source access>
Status: PLANNED
Due before: <YYYY-MM-DD or ramp day>
Completed: <YYYY-MM-DD or n/a>
Unlocks: learning-checklist item one
Owner: <grant owner role>
Source of truth: <permission, access, or owner record path>
```

List prerequisite reading as canonical repository source paths, not prose summaries. A missing source path is a documentation gap; do not fill it with an invented path.

## [9][ROLE_LEARNING_MAP]

Add a role-learning map when the learner must understand owner edges, failure modes, external systems, or critical-path components before practice is safe. The map is a learning aid over existing architecture and source truth; it does not create architecture truth, C4 modeling, or current system structure. Each critical-path component named here anchors a mandatory comprehension question in the learning checklist, so the facts must be recoverable even when no diagram is present.

Render role-learning map entries as records when each edge needs owner, source, failure, and comprehension fields:

```markdown template
### [N.M][BOUNDARY_EDGE]

Role edge: <boundary or responsibility the learner must hold>
Architecture/source: <existing architecture, README, source path, or none>
Owner: <role that owns the edge>
Failure mode: <observable way this edge fails or escalates>
Critical component: <component, path, or workflow the learner must explain>
Comprehension gate: <question or trace-aloud prompt used in the learning checklist>
```

Use a small table only when three or more edges share short fields. Use a diagram only when it helps a learner see already-owned relationships that records obscure. A local onboarding diagram must name its architecture/source input and include a text equivalent; it must not introduce C4 levels, deployment topology, or component truth that the architecture owner has not already published.

The accepted shape ties source truth to a gate the learner must pass:

```markdown conceptual
Role edge: request admission crosses from `apps/api/` into `libs/policy/`.
Architecture/source: `apps/api/ARCHITECTURE.md`; `libs/policy/_ARCHITECTURE.md`
Owner: API owner for admission; policy owner for decision rules.
Failure mode: invalid requests bypass policy or policy errors lack admission evidence.
Critical component: admission envelope and policy result boundary.
Comprehension gate: trace one request from admission to policy result and explain where evidence is recorded.
```

The rejected shape names parts without owner, failure, or learning proof:

```markdown rejected
Components: API, policy, storage.
Diagram: draw boxes showing API connects to policy and storage.
```

## [10][RAMP_PHASES]

State the time-boxed spine as a phase table: the temporal container the gates hang on. Without it the document is a gate list with no horizon, no cadence, and no per-phase exit. Each phase names one measurable outcome, its check-in cadence, and its exit criterion.

| [INDEX] | [PHASE]      | [OUTCOME]             | [CADENCE]       | [EXIT]                    |
| :-----: | :----------- | :-------------------- | :-------------- | :------------------------ |
|   [1]   | Orientation  | reviewed change ships | weekly buddy    | env built; change shipped |
|   [2]   | Contribution | scoped area owned     | weekly reviewer | minimal support           |
|   [3]   | Independence | authority segment led | sign-off owner  | no unsafe escalation      |

Adapt the phase names to the role, but keep the required shape: outcome, cadence, and exit criterion.

## [11][LEARNING_CHECKLIST]

Order checklist items by how understanding builds, not by file tree: request flow, dependency chain, lifecycle stage, escalation path, or maintainer responsibility. Use file-tree order only when the tree is the learning path.

Default to 3-5 learning items. Add more only when the role-learning map proves more distinct owner edges or critical components; otherwise combine adjacent readings into one item and keep the proof observable.

Render each learning item as an H3 record block because it carries more than 5 fields and is updated independently:

```markdown template
### [N.M][ITEM_LABEL]

Scope: <boundary, subsystem, workflow, or responsibility being learned>
Source: <canonical repository source path>
Understanding: <concept or behavior to hold before the next item>
Proof: <observable exercise, comprehension question, review task, or trace-aloud>
Owner: <role for questions and review>
Access: <permission level this item needs, or n/a>
Status: <PLANNED | IN-PROGRESS | BLOCKED | DONE | DROPPED>
Sign-off: <evidence that closes the item, naming who recorded closure>
```

Carry at least one observable comprehension question in the `Proof` field for every critical-path component named in the role-learning map, in the `Know before moving on` form. A checklist item proves nothing until `Proof` names something a reviewer can observe and `Sign-off` names who recorded closure.

The rejected shape below fails because its proof is unobservable:

```markdown rejected
Scope: request lifecycle through <subsystem>
Understanding: how the request reaches the runtime
Proof: read the module and feel comfortable with it
Sign-off: done
```

The accepted shape names source truth, an observable proof, an owner, state, and sign-off:

```markdown conceptual
Scope: request lifecycle through `<subsystem-path>`
Source: <subsystem-path>/README.md
Understanding: how a request reaches the runtime and returns evidence
Proof: trace one request path aloud to the owner and answer two follow-ups
Owner: <subsystem owner role>
Access: read-only repository access
Status: IN-PROGRESS
Sign-off: owner recorded the trace as accepted
```

## [12][FIRST_SAFE_TASKS]

A first safe task gives the learner real ownership while keeping its blast radius contained to one reviewed unit and its supervision explicit. It is safe because its scope is bounded and its reviewer is named, not because it is low value.

Default to 1-3 first safe tasks. Add more only when each task exercises a different readiness criterion or authority boundary.

Render selected tasks as H3 records so the learner, reviewer, exit gate, and proof stay together:

```markdown template
### [N.M][TASK_LABEL]

Task: <supervised low-blast-radius task>
Scope: <single reviewed unit and forbidden blast radius>
Procedure owner: <how-to, contributing, runbook, reference, or n/a>
Source: <canonical policy, guide, issue, or source path>
Reviewer: <owner role gating the result>
Exit: <observable accepted result>
Proof: <link, command result, review note, or artifact that closes the task>
Status: <PLANNED | IN-PROGRESS | BLOCKED | DONE | DROPPED>
Fresh-eyes: <yes/no; if yes, name the documentation deliverable>
```

Draw first safe tasks from this set. Select a fresh-eyes documentation task when the ramp owns documentation stewardship or exposes a likely documentation gap; otherwise capture the learner's fresh perspective in `Feedback and refresh` instead of forcing a documentation deliverable into the task set.

- correct a stale onboarding or reference section, with owner review and `Fresh-eyes: yes`;
- draw the role-learning map from existing architecture/source truth and have an owner review it, with `Fresh-eyes: yes` when the map documents a current gap;
- answer one bounded comprehension question from source truth;
- reproduce one documented local verification path end to end;
- triage one non-urgent issue against existing policy;
- review one low-risk change behind a maintainer's second review;
- observe one operational event and write an owner-reviewed summary.

This section names supervised practice only. A task qualifies as first safe only when it touches no users, production state, release authority, or repository-wide risk without a named reviewer gating the result. Task procedure routes to how-to, contribution mechanics route to contributing, operational response routes to runbook, and lookup facts route to reference. If a task needs the actual steps, link the procedure owner; do not embed the procedure.

## [13][SHADOWING_REVIEW_PATH]

Add this section when elevated authority, production access, user impact, merge rights, release authority, or cross-functional review authority transfers. Define, per shadowed activity, what the learner observes, what the mentor does, the abort signal, and the debrief artifact that closes every session. Shadowing and reverse-shadowing come from onboarding practice; debrief artifacts and abort signals are local proof and risk controls.

Use one shadow/reverse-shadow path per authority type unless the role boundary proves that separate activities carry separate risk. A shadowing record routes the procedure it observes; it does not duplicate review, release, support, runbook, or incident-response steps.

Render each activity as an H3 record:

```markdown template
### [N.M][ACTIVITY_LABEL]

Activity: <review, triage, release, support, incident response, or on-call>
Procedure owner: <contributing, how-to, runbook, support policy, or n/a>
Source: <canonical policy, runbook, guide, or source path>
Prerequisite: <what the learner completes before the first session>
Artifact: <debrief notes, questions, or summary produced every session>
Mentor: <owner role responsible for feedback>
Lead-under-supervision: <when the learner may reverse-shadow or lead>
Abort: <signal that makes the activity too risky for training, or n/a>
Status: <PLANNED | IN-PROGRESS | BLOCKED | DONE | DROPPED>
Readiness signal: <observable proof before independent authority>
```

Follow shadow then reverse-shadow ordering, and produce a debrief artifact after every session. The `Abort` field is mandatory whenever the activity can affect production state or users, because the supervised session must have a defined stop before risk exceeds training value.

## [14][READINESS_CRITERIA]

State readiness criteria as observable, role-specific gates an owner can confirm, rendered as a checklist when each gate is short and proof-light. A criterion that an owner cannot observe is not a gate. The closing criterion is always the recorded authority transition with the owner's recorded remaining limits.

Gate the transition on this ordered set:

- [ ] Learner closed every learning-checklist item that the role requires.
- [ ] Learner explained the role-learning map and its failure modes to the sign-off owner, when the ramp includes one.
- [ ] Learner completed first safe tasks, including any selected fresh-eyes deliverable, with accepted review.
- [ ] Sign-off owner approved the authority transition and recorded the remaining limits.

Add the authority-transfer gates below only when the ramp includes `Shadowing and review path`:

- [ ] Learner completed required shadow and reverse-shadow activities with accepted debriefs.
- [ ] Learner led one supervised review, drill, release rehearsal, or incident segment without unsafe escalation.
- [ ] Sign-off owner confirmed that abort signals, remaining limits, and independent-authority boundaries are recorded.

Promote a proof-heavy gate to an H3 record so the gate and its proof read as one unit:

```markdown template
### [N.M][SIGN_OFF_AUTHORITY]

Status: PLANNED
Exit: sign-off owner approved the authority transition and recorded the remaining limits.
Evidence: <maintained sign-off record or review artifact>
Access: <granted authority and remaining limits>
Source of truth: <access table, owner record, or permission source>
Last verified: <YYYY-MM-DD or n/a for conceptual example>
Review trigger: <event that makes the grant stale>
```

Attach proof beside the gate it closes: the source path or maintained document, the exact command or exercise when runnable behavior is the gate, the owner sign-off, the access boundary when authority changes, and the freshness field for any drift-prone section. Use evidence fields from the evidence standard; do not invent new staleness machinery.

Reject status-complete theater: a readiness record that says `Status: DONE` without sign-off, source truth, and remaining limits is not proof.

```markdown rejected
### [N.M][SIGN_OFF_AUTHORITY]

Status: DONE
Exit: learner is ready.
Evidence: all checklist items complete.
```

## [15][OWNER_ROLES]

Name three owner roles distinctly so the learner knows who to ask, who reviews work, and who confers authority:

- Buddy: answers daily questions during the ramp.
- Reviewer: reviews first safe tasks and scoped work.
- Sign-off owner: assesses readiness and approves the authority transition.

One person may hold more than one role, but the document names all three so no responsibility is implicit.

## [16][FEEDBACK_REFRESH]

Name a dated feedback-capture point that feeds ramp improvement and the drift check that keeps the document current. The feedback point captures the learner's experience while it is fresh — at week one, mid-ramp, or completion — and the drift check keeps factual claims current.

Render the refresh as a short definition block unless an existing maintained checklist owns it:

```markdown template
Feedback point: <week one, mid-ramp, completion, or named checkpoint>
Captures: <learner pain points, stale source paths, missing permission, unclear owner, or readiness-gap signal>
Refresh owner: <role that updates the ramp>
Drift check: <role, source path, access grant, tool, or sign-off policy that makes this ramp stale>
Source of truth: <maintained checklist or ramp source, or n/a>
```

State the refresh as a named maintained checklist where one already owns it, naming that checklist where the section would otherwise stand rather than dropping the section. Do not invent retention, satisfaction, or delivery-flow metrics unless a current owner already measures them.

## [17][RAMP_FLOW]

Add a diagram only when branch states, access transitions, or authority gates are hard to recover from records alone. Do not add a diagram to restate the required phase table, learning records, and readiness checklist.

Use `stateDiagram-v2` for a lifecycle of gated states, and include visible text explaining what the conceptual diagram proves:

```mermaid
---
config:
  layout: elk
  look: neo
  theme: base
  elk:
    mergeEdges: false
    nodePlacementStrategy: BRANDES_KOEPF
    cycleBreakingStrategy: GREEDY_MODEL_ORDER
---
stateDiagram-v2
    accTitle: Onboarding authority ramp
    accDescr: Onboarding proceeds from provisioning through orientation, contribution, supervised practice, and authority transition before the owner records grants and limits.
    [*] --> Provisioning
    Provisioning --> Orientation: access granted
    Orientation --> Contribution: understanding gates signed off
    Contribution --> SupervisedPractice: first safe task accepted
    SupervisedPractice --> AuthorityTransition: authority gates accepted
    AuthorityTransition --> [*]: owner recorded grant and limits
```

Text equivalent: provisioning precedes orientation, orientation precedes contribution, contribution precedes supervised practice, supervised practice precedes authority transition, and the owner records the final grant and limits before the ramp closes.

The diagram proves only gate order. Optional shadowing and reverse-shadowing fit inside `SupervisedPractice` when authority transfer requires them. Access widens only as the prior gate closes; no transition skips a state. The instantiated role, access, sign-off, and remaining-limit records remain the source of truth for the authority transition.

## [18][BOUNDARIES]

- [tutorial.md](tutorial.md) owns teaching a learner through a first success; onboarding owns preparing a person for standing role responsibility.
- [architecture.md](../explanation/architecture.md) owns current structure, C4 modeling, and system-boundary explanation; onboarding may require a learner-facing role-learning map but does not own architecture truth.
- [roadmap.md](../explanation/roadmap.md) owns role-related milestone sequence, planned readiness work, and future authority-transfer sequencing when those are active.
- [code-documentation.md](../reference/code-documentation.md) owns public-symbol comment stewardship and generated source-reference contracts when onboarding includes code-documentation maintenance.
- [how-to.md](../task/how-to.md) owns repeatable competent-reader tasks; onboarding may assign a supervised practice task but does not embed the procedure.
- [runbook.md](../task/runbook.md) owns operational symptom response, mitigation, rollback, escalation, and verification.
- [contributing.md](../task/contributing.md) owns shared contribution workflow, pull-request evidence, and review mechanics.
- [reference.md](../reference/reference.md) owns standalone lookup facts, command catalogs, status vocabularies, and source-of-truth tables.
- [README.md](../README.md) owns document-type routing, placement, lifecycle, split/link decisions, and adjacent-type selection.

## [19][REVIEW_CHECKLIST]

**Identity and source**
- [ ] Opening metadata carries `Role`, `Owner`, `Ramp horizon`, `Source of truth`, `Last reviewed`, and `Review trigger`.
- [ ] Audience names exactly one role and the prior knowledge assumed.
- [ ] One primary ramp profile is chosen and its readiness target is observable and time-boxed.
- [ ] Readiness target uses outcome metrics, never activity metrics.
- [ ] Provisioning grants are dated H3 records with `Access`, `Status`, `Due before`, `Completed`, `Unlocks`, `Owner`, and `Source of truth`.
- [ ] Role-learning map appears only when the ramp needs it and names owner edges, source truth, failure modes, critical components, and comprehension gates.
- [ ] Diagrams summarize already-owned architecture/source truth only when records are insufficient, and architecture owns C4 modeling rules.

**Learning and practice**
- [ ] Ramp phases appear only when staged access, ramp length, or checkpoint cadence requires them, and each phase names a measurable outcome, a check-in cadence, and an exit criterion.
- [ ] Learning checklist is ordered by how understanding builds and defaults to 3-5 items unless the role-learning map proves more.
- [ ] Each learning item carries scope, source, understanding, proof, owner, status, and sign-off, with access where it matters.
- [ ] At least one comprehension question exists per critical-path component named in the role-learning map.
- [ ] First safe tasks default to 1-3, carry real ownership behind a named reviewer, stay inside the blast-radius boundary, route procedure owners, and mark any selected fresh-eyes deliverable.
- [ ] Shadowing appears only when authority transfers, follows shadow then reverse-shadow, routes the observed procedure owner, carries debrief artifacts, and names abort signals where production or users are reachable.

**Proof and closure**
- [ ] Readiness criteria are observable gates ending in a recorded transition with remaining limits.
- [ ] Authority-transfer readiness gates appear only when shadowing or elevated authority applies.
- [ ] Owner roles name the buddy, reviewer, and sign-off owner distinctly.
- [ ] Feedback-capture point and drift check are both named when the ramp is reused or feedback changes future ramp quality.
- [ ] Proof beside each gate names evidence and sign-off; authority-changing gates also name the access boundary and freshness field.
- [ ] Boundaries route tutorial, architecture, roadmap, code-documentation, how-to, runbook, contributing, reference, and README content away from onboarding.

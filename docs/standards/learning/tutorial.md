# [TUTORIAL_STANDARDS]

A tutorial teaches one learner outcome by guiding the learner through concrete action, visible results, and a primary path the author executed end to end before publication. The author carries reliability: a published lesson must be reproducible from the stated start state, and any unverified dependency must stay outside the core success path or mark the lesson as draft or blocked.

A learning path orders multiple tested lessons so later lessons reuse skill, vocabulary, or artifacts established earlier. This standard carries lesson shape, learner-path ordering, and execution confirmation; it does not own competent-reader procedures, readiness, incident recovery, lookup facts, API contract truth, support policy, contribution workflow, or conceptual explanation.

## [1]-[USE_WHEN]

Use a tutorial when every condition holds:
- the reader is learning the subject, not performing routine work.
- the path has fixed inputs and an observable result.
- one document can carry a complete first success.
- the exercise is repeatable, reversible, or disposable.

Use a learning path when multiple tested lessons build toward one broader skill and later lessons depend on earlier completion confirmation. Related lessons may link to each other, but they are not a learning-path index until ordered path maintenance changes learner action.

Route elsewhere by topic when the reader is a competent operator completing a known task, a person becoming ready for a route, an operator recovering from an incident trigger, a reader looking up facts, a contributor following PR workflow, or a reader seeking concepts and trade-offs.

[AUTHORING_CONTRACT]:
- Agent use: choose single tutorial or learning path, prove the start state, then write the executed learner path with visible working-state checkpoints.
- Tutorial structure: `What we will build`, `Learning outcome`, `Prerequisites`, `Start state`, `Steps`, `Result`, `What to notice`, `Next steps`, `Boundaries`, and `Result check`.
- Learning-path structure: `Reader`, `Outcome`, `Prerequisites`, `Path`, `Completion`, `Boundaries`, and `Result check`.
- Section cardinality: one observable outcome; enough checkpoint steps to prove the result without adding a second lesson; multiple tested lessons for a learning path; conditional recovery appears only for observed or locally maintained learner traps.
- Adjacent checks: check how-to, API, reference, support matrix, runbook, contributing, onboarding, roadmap, architecture, code documentation, and README only when a lesson consumes their fact after or inside the learning path.
- Maintenance triggers: update the tutorial when start state, fixture, command, tested stack, generated artifact, support target, lesson order, learner trap, result gate, or adjacent learning route changes.

## [2]-[REQUIRED_STRUCTURE]

A single tutorial uses this spine. `Learner-trap recovery` appears only when the author observed recoverable failures or can name a documented learner trap that does not fit a step-local `If wrong` field.

```markdown template
# [BUILD_OBSERVABLE_ARTIFACT]

<Lead: one sentence naming the artifact, difficulty, estimated time, and tested stack.>

## [1]-[WHAT_WE_WILL_BUILD]

## [2]-[LEARNING_OUTCOME]

## [3]-[PREREQUISITES]

## [4]-[START_STATE]

## [5]-[STEPS]

## [6]-[RESULT]

## [7]-[WHAT_TO_NOTICE]

## [8]-[NEXT_STEPS]

## [9]-[BOUNDARIES]

## [3]-[AUTHORING_RULES]

Tutorial rules split into these groups:

[PATH_DISCIPLINE]:
- Show the destination first. Local rule: the final artifact appears before step one as screenshot plus text equivalent, exact output, or a small diagram.
- Deliver a visible, comprehensible result at every step; keep the example in a working state at every checkpoint.
- Tell the learner what result to confirm before they run a step, and give exact example output where output is the signal.
- Point out what the learner should notice; never assume the result speaks for itself.
- Give the learner meaningful work before abstract explanation.

[SCOPE_DISCIPLINE]:
- Flag known learner traps inline at the step where they happen; consolidate only observed or locally maintained recoverable failures into learner-trap recovery.
- Minimize explanation and options: one concrete path, with branches linked out after completion.
- Keep inputs reproducible, repeatable, and disposable so a learner can rerun from a clean start.
- Keep short lessons compact: keep entry sections to short blocks unless confirmation requires more.

## [4]-[STRUCTURAL_CHOICES]

Choose one structure before writing. A single tutorial and a learning path have different closure surfaces, so do not blend them.

| [INDEX] | [STRUCTURE]     | [READER]     | [TITLE]  | [SPINE]      | [CLOSURE]                 |
| :-----: | :-------------- | :----------- | :------- | :----------- | :------------------------ |
|   [1]   | Single tutorial | first-timer  | artifact | lesson spine | stated end-state          |
|   [2]   | Learning path   | path learner | skill    | path spine   | composed final capability |

Treat reader, difficulty, tool family, and concept depth as entry context and prose constraints inside the chosen structure, not as additional variants. Split lessons that need separate inputs, confirmation, titles, or success artifacts.

## [5]-[TITLE_OUTCOME_RULES]

Lead the title with the observable artifact or skill outcome, not an internal abstraction:

```markdown template
# [BUILD_ARTIFACT]
```

[TITLE_SHAPE]:
- Accepted title: `# [BUILD_ARTIFACT]`
- Rejected title: `# [UNDERSTANDING_DOCUMENTATION_RULES]`
- Reason: the accepted title names an artifact the lesson can produce; the rejected title names cognition, which routes to explanation. State the learning outcome as a specific capability the learner can perform: `you can validate a scoped Markdown diff`, never `understand documentation rules`.

## [6]-[LEARNING_PATHS]

A learning path index uses this spine when multiple tested lessons compose one broader skill:

```markdown template
# [SKILL_OUTCOME_LEARNING]

<Lead: one sentence naming the skill outcome, difficulty, lesson count, and whether stack confirmation is lesson-local or shared.>

## [1]-[READER]

## [2]-[OUTCOME]

## [3]-[PREREQUISITES]

## [4]-[PATH]

## [5]-[COMPLETION]

## [6]-[BOUNDARIES]

## [7]-[END_STATE_PREVIEW]

An end-state preview must show the final artifact, not decorate the opening. Exact output is enough when the result is textual:

```text conceptual
Changed files: <changed-document>
Check: <confirmation-command>
Result: PASS
Next route: no path or whitespace failures
```

Use a diagram only when the artifact is a relationship the learner must inspect and the rendered shape carries meaning that output text cannot. The diagram must show the final artifact, not merely the fact that a command produces a file.

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
flowchart LR
    accTitle: Request route
    accDescr: The completed lesson builds one request route that validates an input, writes the accepted receipt, and rejects invalid payloads before storage.
    Input["Incoming request"] --> Validate{"Payload valid?"}
    Validate -->|yes| Receipt["accepted receipt"]
    Validate -->|no| Reject["rejected response"]
    Receipt --> Store["receipt store"]
```

Text equivalent: the completed request route validates an incoming payload, writes an accepted receipt to storage only when the payload is valid, and returns a rejected response for invalid payloads.

Reject a generic success diagram that does not prove the learner's final artifact.

## [8]-[STEP_RECORDS]

Each step is a checkpoint record, not a bare instruction line. A step must leave the learner in a verified working state. Render steps as numbered checkpoint records; add H3 milestone sets only when a longer tutorial needs skimmable boundaries, and keep actual steps numbered under the set.

Step records use these fields:
- `Operation`: required; exact command, file edit, UI action, fixture use, or captured interaction. For commands, state the copy-safe command first, then its expected signal.
- `Expected`: required; exact output, named file change with content or row count, specific UI state, screenshot path, or another observable signal.
- `Working state`: required; what now compiles, runs, renders, or passes so the learner is not broken.
- `Action`: optional; present only when the numbered checkpoint title cannot carry the imperative clearly.
- `Execution`: optional; present only for optional side effects outside the core success path, or for draft/blocked lessons.
- `Notice`: conditional; required when the learner must observe this result before the next step makes sense.
- `If wrong`: optional; inline failure flag for this step's known trap, naming cause and fix.

State each term in the step that first needs it. Use fixed inputs, deterministic commands, and realistic unambiguous placeholder data. Defer variants to adjacent how-to or reference documents linked after completion.

```markdown conceptual
3. Validate the scoped diff.
    Operation: `<confirmation-command> <changed-paths>`
    Expected: the command exits 0 and prints no whitespace error lines.
    Working state: changed Markdown has no whitespace errors.
    Notice: no output on success is the expected signal for this command.
    If wrong: a `trailing whitespace` line names the file and line to fix before continuing.
```

[ORDERED_STEP_SHAPE]:
- Rejected step: `3. Run the check.`
- Reason: the rejected form carries no exact signal and no working-state gate. Keep every command copy-safe per the craft route: omit prompts, use long flags, use full file paths where needed, and use realistic placeholders.

## [9]-[EXECUTION_VOCABULARY]

A published tutorial's core success path must be author-run from start state to result. Use execution tags only for draft or blocked lessons, or for optional side effects outside the core path that depend on hardware, credentials, validator access, or live services the author could not exercise.

Use this closed execution-tag vocabulary:
- `NEEDS-FIXTURE`: the lesson is draft or blocked because a fixture or seed must be staged before verification.
- `UNVERIFIED-REQUIRES-<X>`: an optional side effect or draft/blocked lesson depends on hardware, credentials, validator access, or a live service the author could not exercise; name the dependency in place of `<X>`.

The tag rides in the step record's optional `Execution` field. Define the set inline at first use and apply no tag beyond this closed set. A tutorial with a tagged primary-path step is not publishable as `AVAILABLE`; the path entry stays `DRAFT` or `BLOCKED` until the primary path runs front to back.

```markdown conceptual
7. Run the configured result confirmation.
    Operation: `<confirmation-command> <target>`
    Expected: unresolved result gaps are listed, or the command exits 0.
    Working state: `<confirmation-command>` still exits 0.
    Execution: UNVERIFIED-REQUIRES-CONFIGURED-CHECK
    If wrong: no configured checker means the confirmation gap stays visible instead of claiming confirmation.
```

This shape is valid only when the link checker is outside the core success path or the lesson is not published as available.

## [10]-[RESULT_EXIT_GATE]

State `Result` as the final artifact compared against the end-state preview: reference diff, exact output, screenshot plus text equivalent, or diagram plus caption. Close the section with `Done when` checks so the learner and an agent validating the document know whether the lesson closed. Test automation and release policy route to [test-strategy.md](../explanation/test-strategy.md).

```markdown template
## [6]-[RESULT]

The scoped diff passes the stated end-state preview: changed Markdown has no whitespace errors and the confirmation gap is explicit.

The result is done when these checks pass:
- [ ] the confirmation result matches `What we will build`
- [ ] `<confirmation-command>` exits 0
- [ ] another agent can reproduce the same check without the author present.
```

Each `Done when` item is observable and falsifiable. The final item proves the learning outcome capability, not just the last step.

## [11]-[LEARNER_TRAP_RECOVERY]

Add learner-trap recovery only for failures observed during front-to-back execution or locally maintained learner traps that cannot fit in a step-local `If wrong` field. Do not invent symptom-cause-fix rows because a step could theoretically fail. Operational recovery routes to runbook, and routine task repair routes to how-to.

```markdown template
| [INDEX] | [STEP] | [SIGNAL]                      | [RECOVERY]              | [EVIDENCE]       |
| :-----: | :----- | :---------------------------- | :---------------------- | :--------------- |
|   [1]   | Step 2 | `trailing whitespace` appears | remove spaces and rerun | observed dry run |
|   [2]   | Step 4 | unresolved anchor appears     | update link or heading  | maintained trap  |
```

Order rows by the step where the signal first appears. Use the table only while each row fits `Step`, `Signal`, `Recovery`, and `Evidence` as short atomic facts; do not add separate cause and fix columns when they make the table read like prose. The `Evidence` cell must identify an observed run, maintained local trap, or documented support condition; otherwise use a step-local `If wrong` field or omit the row.

Promote a trap to a record when the cause, recovery, or confirmation needs more than a short cell:

```markdown template
### [N.M]-[UNRESOLVED_ANCHOR]

Step: Step 4
Signal: unresolved anchor appears.
Cause: heading label changed without updating the in-repo link.
Recovery: update the link target or restore the heading anchor, then rerun local path and anchor confirmation.
Observed result: maintained trap.
```

## [12]-[NEXT_STEPS]

Close a tutorial with one reinforcement exercise when one can reuse the new skill without introducing a second lesson. Link adjacent documents only when maintained adjacent content exists and the route changes the learner's next action.

The reinforcement exercise fails when it introduces a new tool, subsystem, account, deployment surface, access grant, incident path, contribution workflow, or second artifact. Move that work to another tutorial, learning-path entry, how-to guide, runbook, contributing guide, or onboarding ramp, then link it only if the adjacent document exists.

Do not invent adjacent links just to cover every reader-need route. Missing adjacent content is a documentation gap, not a reason to embed another document type in the tutorial; full route ownership stays in `Boundaries`.

## [13]-[EXECUTION_CLOSURE]

Execute the primary path as written before publishing a tutorial as available. Claim support attaches to the drift-prone fact, not the page footer. Generic claim-support field mechanics route to [proof.md](../proof.md); this section names only tutorial-specific closure obligations.

[CLOSURE_SURFACES]:
- exact operations the path uses: commands, UI actions, repository paths, fixtures, or captured interactions.
- final observable result and its stated end-state preview.
- expected intermediate signals at any step where a learner could lose confidence.
- grouped checks for shared stack, toolchain, support target, account, or fixture dependencies.
- step-local checks for unique drift-prone command, fixture, account, service, captured interaction, generated artifact, or support condition.

A learning path additionally closes lesson order and composed capability: prerequisites exist, each lesson is independently testable from its own start state, no later lesson relies on unexplained state, earlier lesson results feed later lessons wherever claimed, and the final lesson demonstrates the composed skill.

Use short excerpts to prove closure shape; do not publish a complete second tutorial inside the standard.

[DONE_CHECKS]:
- [ ] `<confirmation-command>` exits 0
- [ ] the changed path set is scoped to the lesson artifact
- [ ] any missing link or anchor checker is recorded as a confirmation gap.

[CLOSURE_EXAMPLE]:
- Accepted: Notice the lesson validates one bounded documentation path instead of teaching general Git or contribution workflow.
- Rejected: a full mini-tutorial repeated inside `Execution closure`.
- Reason: title, preview, steps, result, and confirmation already have local examples beside their rules.

Learner-facing first person such as `We will build` is correct because the document tutors. Author notes, task history, interaction fragments, and local machine paths are not.

## [14]-[BOUNDARIES]

These adjacent routes own material outside the tutorial:

[LEARNING_TASK_TYPES]:
- Document-type choice, placement, splitting, and lifecycle route to [README.md](../README.md).
- One repeatable task for a competent reader routes to [how-to.md](../task/how-to.md).
- Operational symptom response, mitigation, rollback, escalation, and recovery route to [runbook.md](../task/runbook.md).
- Agent ramps, first safe actions, and readiness gates route to [onboarding.md](onboarding.md).
- Contribution workflow, pull-request confirmation, and review mechanics route to [contributing.md](../task/contributing.md).
- Concepts, trade-offs, and architecture route to [architecture.md](../explanation/architecture.md) or another explanation route.
- Target lesson sequence, milestone order, and future learning work route to [roadmap.md](../explanation/roadmap.md) when a maintained roadmap exists.

[REFERENCE_TYPES]:
- Generated or callable API contract truth routes to [api.md](../reference/api.md).
- Lookup facts, command catalogs, option tables, and status vocabularies route to [reference.md](../reference/reference.md).
- Supported-version, platform, lifecycle, and compatibility truth route to [support-matrix.md](../reference/support-matrix.md).
- Public-symbol comments and generated source-reference contracts route to [code-documentation.md](../reference/code-documentation.md).

[SHARED_STANDARDS]:
- Container choice, code-block intent labels, table decomposition, and diagram type route to [information-structure.md](../information-structure.md).
- Command mechanics, terminology, and copy-safe Markdown route to [style-guide.md](../style-guide.md).
- Claim-level confirmation and preservation route to [proof.md](../proof.md).

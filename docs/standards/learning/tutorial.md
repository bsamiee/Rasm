# [TUTORIAL_STANDARDS]

A tutorial teaches one learner outcome by guiding them through concrete action, visible results, and a path the author executed end to end before publication. The author owns reliability — the lesson works for every learner, every time — and the learner owns only attention. A learning path orders multiple tested lessons so each later lesson reuses skill, vocabulary, or artifacts established earlier. This standard owns lesson shape, learner-path ordering, and execution proof; it does not own task procedure for a competent reader, role readiness, incident recovery, lookup facts, or conceptual explanation.

## [1][USE_WHEN]

Use a tutorial when every condition holds:

- the reader is learning the subject, not performing routine work;
- the path has fixed inputs and an observable result;
- one document can carry a complete first success;
- the exercise is repeatable, reversible, or disposable.

Use a learning path when three or more tested lessons build toward one broader skill and later lessons depend on earlier completion proof.

Route elsewhere by topic when the reader is a competent operator completing a known task, a person becoming ready for a role, an operator recovering from an incident trigger, a reader looking up facts, or a reader seeking concepts and trade-offs.

## [2][AUTHORING_DOCTRINE]

These imperatives govern every section below; they are the binding contract of the type, drawn from canonical tutorial doctrine. `Source of truth:` Diataxis tutorial doctrine (`diataxis.fr/tutorials/`) and Refactoring English, "Rules for Writing Software Tutorials" (revised January 2025, `refactoringenglish.com/chapters/rules-for-software-tutorials/`). `Last verified:` 2026-06-04. `Review trigger:` Diataxis tutorial or named tutorial-writing guidance changes.

- Show the destination first: state the artifact and show its end state before step one, never a prose promise alone.
- Deliver a visible, comprehensible result at every step; keep the example in a working state at every checkpoint.
- Keep a narrative of expectation: tell the learner what they will see before they run a step, and give exact example output.
- Flag likely failure inline at the step where it happens, and consolidate recoverable failures into a troubleshooting table.
- Point out what the learner should notice; never assume the result speaks for itself.
- Give the learner meaningful work before abstract explanation: the learner acts and sees a result, then any explanation follows.
- Minimise explanation ruthlessly and ignore options and alternatives: one concrete path, every branch linked out from `Next steps`.
- Permit repetition: keep inputs reproducible, repeatable, and disposable so a learner can rerun from a clean start.

## [3][VARIANT_PROFILES]

Choose one primary variant before writing. Each variant fixes its own title shape, required spine, completion proof, and default difficulty. Keep variant facts together so an author can compare the four routes in one scan.

| [INDEX] | [VARIANT]         | [READER]     | [TITLE]    | [SPINE_DELTA] | [PROOF]        | [DIFFICULTY]   |
| :-----: | :---------------- | :----------- | :--------- | :------------ | :------------- | :------------- |
|   [1]   | Single tutorial   | first-timer  | artifact   | none          | reference diff | `beginner`     |
|   [2]   | Tool tutorial     | tool adopter | task       | reset path    | sample output  | `beginner`     |
|   [3]   | Concept grounding | model-first  | capability | notice notes  | prediction     | `intermediate` |
|   [4]   | Path index        | path learner | skill      | `Path` spine  | final proof    | `intermediate` |

Split the document when two variants each need separate inputs, separate proof, or separate titles. Do not blend a single tutorial with a path index in one file. The `Default difficulty` column maps the chosen variant to the opening metadata `Difficulty` value; override it only when the lesson's actual prerequisites justify a different level.

## [4][METADATA]

Open every tutorial with an in-body metadata block. Use only the fields below unless a named renderer, indexer, or review workflow consumes another field.

```markdown template
Description: <one line naming what this teaches>
Difficulty: beginner | intermediate | advanced
Estimated time: <e.g. 20 minutes>
Version: <exact tested stack, e.g. Node.js v22.12.0, pnpm 9.x>
Last verified: YYYY-MM-DD
```

- `Description`: required; one line, what the lesson teaches.
- `Difficulty`: required; exactly one of the closed set `beginner`, `intermediate`, `advanced`. Default to the variant's `Default difficulty`.
- `Estimated time`: required; the reader's entry contract for completion cost.
- `Version`: required; the exact tested toolchain, runtime, and package versions, never `latest`. This is the drift anchor `Prerequisites` repeats with a verify command.
- `Last verified`: required; `YYYY-MM-DD` of the last front-to-back execution, per the evidence standard.

A learning path index adds `Lesson count` and omits `Estimated time` at the file level when each `Path` entry carries its own.

## [5][TITLE_OUTCOME_RULES]

Lead the title with the observable artifact, not the internal skill:

```markdown template
# [BUILD_CSV_EXPORT]

```

Reject titles that promise learning the path cannot show:

```markdown rejected
# [UNDERSTANDING_EXPORT_SUBSYSTEM]

```

The first becomes the accepted title shape once the lesson is written; the second is `rejected` because it names cognition, which routes to explanation by topic.

State the learning outcome as a specific capability the learner can perform, not a vague aspiration: `you can configure and run a CSV export endpoint`, never `understand exports`. The artifact is what the learner holds; the outcome is what the learner can now do.

## [6][REQUIRED_STRUCTURE]

A single tutorial uses this spine. Field cardinality is fixed per section.

```markdown template
# [BUILD_OBSERVABLE_ARTIFACT]

Description: <one line>
Difficulty: beginner | intermediate | advanced
Estimated time: <duration>
Version: <pinned tested stack>
Last verified: YYYY-MM-DD

## [1][WHAT_WE_WILL]

## [2][LEARNING_OUTCOME]

## [3][PREREQUISITES]

## [4][START_STATE]

## [5][STEPS]

## [6][RESULT]

## [7][WHAT_NOTICE]

## [8][TROUBLESHOOTING]

## [9][NEXT_STEPS]

## [10][BOUNDARIES]

## [11][REVIEW_CHECKLIST]

```

- `What we will build`: required, one paragraph naming exactly one artifact, plus a required artifact preview — a screenshot path under the repository, an exact final-output block, or a small end-state diagram. A prose promise alone fails this section.
- `Learning outcome`: required; 1 to 3 bullets, each a specific capability the learner can perform afterward.
- `Prerequisites`: required; tools, versions, accounts, and prior lessons as a bulleted list, each item independently checkable. Each version names the exact tested value and a verify command where one exists.
- `Start state`: required; the exact repository state, named branch, commit, sample data, or fixture the learner begins from, reproducible without the author present.
- `Steps`: required; an ordered list of 3 to 12 numbered checkpoint records, each rendered as one numbered list item with indented `label: value` continuation lines carrying the fields in `Step records` below.
- `Result`: required; the final observable artifact the learner diffs against a stated reference, plus a `Done when` exit gate.
- `What to notice`: required; 1 to 5 observations the learner should register after key results.
- `Troubleshooting`: required when any step can plausibly fail; a symptom-cause-fix table of the failure points the author hit during execution.
- `Next steps`: required; links to how-to guides, reference, and explanation surfaced only after the lesson completes, plus one reinforcement exercise that reuses the new skill.
- `Boundaries`: required; adjacent owners and route-away rules, one link per adjacent owner.
- `Review checklist`: required; observable author gates.

A learning path index uses this spine instead:

```markdown template
# [SKILL_OUTCOME_LEARNING]

Description: <one line>
Difficulty: beginner | intermediate | advanced
Lesson count: <number of lessons>
Last verified: YYYY-MM-DD

## [1][AUDIENCE]

## [2][OUTCOME]

## [3][PREREQUISITES]

## [4][PATH]

## [5][COMPLETION_PROOF]

## [6][RELATED]

```

Each `Path` entry is a record carried as a subsection-per-record block, because the entries are updated independently and order is load-bearing. Repeatable: the `Path` section holds two or more such entries.

```markdown template
### [N.M][LESSON_TITLE_LINK]

Outcome: <skill, vocabulary, or artifact the lesson produces>
Prerequisite: <prior lesson or named starting condition>
Proof: <observable result that closes the lesson>
Availability: AVAILABLE | DRAFT | BLOCKED
Estimated time: <duration>
Optional next: <branch lesson, when the path forks>
```

- `Outcome`, `Prerequisite`, `Proof`: required.
- `Availability`: required; exactly one of the closed set `AVAILABLE`, `DRAFT`, `BLOCKED`. `BLOCKED` names a lesson whose prerequisite lesson is not yet published. This is the lesson-publication axis — whether a learner can start the lesson now — and is deliberately orthogonal to the form owner's work-lifecycle `Status` vocabulary (`PLANNED | IN-PROGRESS | BLOCKED | DONE | DROPPED`). The field is named `Availability`, not `Status`, so it does not collide with that reserved token; do not substitute the form owner's lifecycle values here, and do not use a `Status:` label for this axis.
- `Estimated time`: required; the per-lesson completion cost.
- `Optional next`: optional; present only when the path forks.

Order entries so each later lesson consumes a prior lesson's proof. If the entries read in any order without loss, the document is a hub index routed to README by topic, not a learning path.

## [7][STEP_RECORDS]

Each step is a checkpoint record, not a bare instruction line. A step must leave the learner in a verified working state, not merely "run this". Render each step as a numbered list item whose continuation lines carry one `label: value` per line, indented under the numbered item. The numbered item is the step's action sentence; the labels are its checkpoint fields. This is the ordered-list form, not an H3 heading and not a standalone definition block, so the 3-to-12 ceiling never forces a wall of headings:

- `Action`: required; one imperative naming what the learner does.
- `Command`: required; the exact copy-safe command or the exact file path and edit. State the command first, then its expected signal.
- `Expected`: required; the exact output, a named file change with content or row count, a specific UI state, or a captured screenshot path. Never "it should work" or "you should see something".
- `Working state`: required; what now compiles, runs, or passes so the learner is provably not broken. Name the stub or placeholder content where full implementation is deferred to a later step.
- `Execution`: optional; the execution-proof tag from `Execution vocabulary` below, present only when the step is not plainly author-run. A step the author ran front to back and observed carries no tag; a step the author could not exercise carries `NEEDS-FIXTURE` or `UNVERIFIED-REQUIRES-<X>` so the unverified claim never reads as asserted fact.
- `Notice`: optional; what the learner should register at this step.
- `If wrong`: optional; the inline failure flag for this step's known trap, naming the cause and the fix.

State each term in the step that first needs it, in one sentence, not in a glossary the learner must hold. Use fixed inputs, sample data, and deterministic commands; defer variants to how-to guides linked from `Next steps`.

```markdown conceptual
3. Run the export against the seeded fixture.
   Action: emit the sample report.
   Command: `npm run export -- --out sample.csv`
   Expected: `sample.csv` appears with one header line and 12 data rows.
   Working state: the project still builds; `npm run build` exits 0.
   Notice: the header row is emitted before any data row.
   If wrong: no file means the `--out` flag was dropped in this step.
```

That block shows the runnable command shape a real tutorial step must earn: the command, the row count, and the build gate are all checkable, and it carries no `Execution` tag only when the author ran it front to back. A step the author could not exercise carries the tag in the same slot:

```markdown conceptual
7. Send the generated report to the configured recipient.
   Action: deliver the report over SMTP.
   Command: `npm run export -- --out sample.csv --send`
   Expected: the run logs `delivered to ops@example.com` and exits 0.
   Working state: the export still builds; `npm run build` exits 0 with delivery stubbed.
   Execution: UNVERIFIED-REQUIRES-SMTP
   If wrong: a connection error means no SMTP host was reachable in this step.
```

The `Execution: UNVERIFIED-REQUIRES-SMTP` line pins the step as not author-run: the author could not exercise a live SMTP service, so the step's `Expected` signal is a documented intent, not an observed result, and a learner or agent reading the record knows to stage SMTP before trusting it. Contrast the collapsed form a low-value author would otherwise leave:

```markdown rejected
3. Run the export command. It should work.
```

The second is `rejected` because it carries no exact signal and no working-state gate, so the learner cannot tell whether they finished the step or broke the build.

Keep every command copy-safe per the craft owner: no shell prompt in input lines, long flags over short, full file paths for edits, and realistic unambiguous placeholder data — never `foo`, `user`, or `string`. Code-block intent labels and command mechanics route to the form and craft owners named in `Boundaries`.

## [8][EXECUTION_VOCABULARY]

When a tutorial spans multiple sessions or carries steps the author could not fully execute, tag each checkpoint with a closed execution-proof vocabulary so a learner or agent knows which steps were actually run:

- `VERIFIED`: the author ran this step front to back and observed the `Expected` signal.
- `NEEDS-FIXTURE`: the step depends on a fixture or seed the learner must stage first.
- `UNVERIFIED-REQUIRES-<X>`: the step depends on hardware, credentials, or a live service the author could not exercise; name the dependency in place of `<X>`.

The tag rides in the step record's optional `Execution` field defined in `Step records` above — one tag per step, on its own `label: value` continuation line — so it occupies a fixed, machine-readable slot rather than floating in prose. Define the set inline at first use and apply no tag beyond this closed set; per-item sigils are notation spam this corpus rejects. A short tutorial whose every step is verified needs no tags. The tag makes the execution-proof claim machine-checkable and prevents an unverified step from reading as asserted fact.

## [9][RESULT_EXIT_GATE]

State the `Result` as the final artifact diffed against a stated reference — a reference repository branch, a golden file path, or the exact final output — then close the section with a `Done when` gate. The gate turns the result from a description into a checkable completion contract: the learner and an agent validating the document both know objectively whether the lesson closed.

```markdown template
## [1][RESULT]

`sample.csv` matches `reference/sample.csv` byte for byte.

Done when:
- the artifact matches `reference/sample.csv`;
- `npm run build` exits 0 from a clean checkout;
- you can run the export and reproduce the result without the author present.
```

Each `Done when` item is observable and falsifiable, and the final item proves the `Learning outcome` capability, not just the last step.

## [10][TROUBLESHOOTING]

Capture every failure point the author hit during the mandatory front-to-back execution. Recoverable failures consolidate into a symptom-cause-fix table; the rows are execution-proof artifacts an author can only fill by running the path. Keep step-local flags inline as the `If wrong` field; promote the cross-step or recurring ones here.

| [INDEX] | [SYMPTOM]                | [LIKELY_CAUSE]             | [FIX]                                 |
| :-----: | :----------------------- | :------------------------- | :------------------------------------ |
|   [1]   | Empty `sample.csv`       | Fixture not loaded         | Re-run the `Start state` seed step    |
|   [2]   | Build fails after step 3 | Stub handler left unfilled | Complete the handler body from step 4 |

Order rows by the step at which the symptom first appears, and keep each cell within the form owner's cell ceiling, carrying any long qualifier in a note after the table.

## [11][ACCEPTANCE_CRITERIA]

A tutorial is publishable only when every criterion is observably met:

- the opening metadata carries `Description`, `Difficulty`, `Estimated time`, the pinned `Version`, and `Last verified`;
- the title names the artifact, not the internal skill;
- `What we will build` shows the end state via screenshot path, output block, or diagram before step one;
- the learning outcome states a specific capability;
- the start state is reproducible from the named fixture, branch, or repository state;
- every step is a checkpoint record pairing one action, one copy-safe command, one exact expected signal, and a verified working state;
- the result is an artifact the learner diffs against a stated reference, and the `Done when` gate is observable;
- known failure points are captured as `If wrong` fields or troubleshooting rows, not omitted;
- the path runs front to back from the start state with no undocumented step;
- terminology appears at first use, not in a glossary the learner must hold;
- one concept and one artifact carry the lesson, with no in-step variants.

A learning path is publishable only when, in addition:

- each lesson is independently testable from its own start state;
- each later lesson's prerequisite names an earlier lesson's proof;
- each `Path` entry carries `Outcome`, `Prerequisite`, `Proof`, `Availability`, and `Estimated time`;
- the final lesson proves the composed skill, not just its own step.

## [12][EXECUTION_PROOF]

Execute the full path as written before publication. Proof is claim-level and attaches to the drift-prone fact, not the page footer.

- exact commands, UI actions, or repository paths the path uses;
- the final observable result and its reference;
- expected intermediate signals at any step where a learner could lose confidence;
- `Evidence:` and `Last verified: YYYY-MM-DD` for any external command, version, or service the lesson depends on;
- known-unverified steps when execution depends on reviewer access, hardware, credentials, or a live service, tagged with the `Execution vocabulary` rather than asserted as passing.

A learning path additionally proves lesson order: prerequisites exist, no later lesson relies on unexplained state, and an earlier lesson's completion proof feeds the next lesson wherever the path claims it does.

Learner-facing first person such as `We will build` is correct because the document tutors. Author notes, task history, interaction fragments, and local machine paths are not.

## [13][BOUNDARIES]

- Document-type choice, placement under `docs/tutorials/`, splitting, and lifecycle route to the standards router: [README.md](../README.md).
- One repeatable task for a competent reader routes to the how-to owner: [how-to.md](../task/how-to.md).
- Role readiness, shadowing, and readiness gates route to the onboarding owner: [onboarding.md](onboarding.md).
- Concepts, trade-offs, and architecture the lesson references route to the explanation owner: [architecture.md](../explanation/architecture.md).
- Container choice, code-block intent labels, table decomposition, and diagram type route to the form owner: [information-structure.md](../information-structure.md).
- Command mechanics, terminology, and copy-safe Markdown route to the craft owner: [style-guide.md](../style-guide.md).
- Claim-level evidence, freshness fields, and proof preservation route to the evidence owner: [proof.md](../proof.md).

## [14][REVIEW_CHECKLIST]

- [ ] One variant profile is chosen; single tutorial and path index are not blended.
- [ ] Opening metadata carries `Description`, `Difficulty`, `Estimated time`, the pinned `Version`, and `Last verified`.
- [ ] The title names the observable artifact, not an internal skill.
- [ ] The learning outcome states a specific capability, not a vague aspiration.
- [ ] `What we will build` shows the end state via screenshot, output, or diagram before step one.
- [ ] Audience and start state are explicit and reproducible from a named fixture, branch, or commit.
- [ ] The base spine is present and every required section carries its content.
- [ ] `Steps` holds 3 to 12 numbered checkpoint records.
- [ ] Each step record carries `Action`, a copy-safe `Command`, an exact `Expected` signal, and a verified `Working state`.
- [ ] Known failure points are captured as `If wrong` fields or troubleshooting rows.
- [ ] Commands are copy-safe: no prompt, long flags, full paths, realistic placeholders.
- [ ] Terminology is introduced at first use, beside the step that needs it.
- [ ] Inputs are fixed, deterministic, or intentionally disposable.
- [ ] The result diffs against a stated reference, and the `Done when` gate is observable.
- [ ] The full path was executed front to back, or proof gaps are tagged with the execution vocabulary, not asserted.
- [ ] `Next steps` links how-to, reference, and explanation, and includes one reinforcement exercise.
- [ ] Path entries carry `Outcome`, `Prerequisite`, `Proof`, `Availability`, and `Estimated time`, and are ordered because later lessons consume earlier proof.
- [ ] How-to, reference, onboarding, and explanation material is linked from `Next steps` or `Related`, not embedded.

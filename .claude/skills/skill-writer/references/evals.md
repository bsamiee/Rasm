# [EVALS]

A skill's value is measured, never assumed, along exactly two axes: trigger — the skill loads on the tasks it owns and stays silent on its neighbors — and adherence — the loaded body changes output relative to a run without it. Both run before a bundle ships and after every material revision; the suite is authored before the body it measures, so a body rule ships only after an adherence run proves it changes output, and a skill without an eval history is a hypothesis wearing a description.

## [01]-[TRIGGER_EVALS]

- [QUERY_SETS]: Build must-fire and must-not-fire sets of realistic task phrasings — around twenty per side — drawn from how tasks arrive, not from the description's vocabulary. Every must-fire query names a substantive multi-step task in the skill's domain — a trivial one-step request measures task triviality, not the description, and is excluded. Must-not-fire sets carry near-misses: prompts naming an adjacent deliverable a sibling owns, and ordinary-working-day prompts with no domain noun.
- [HOLDOUT]: When iterating the description against miss rates, the query set splits: the description is tuned against one half and judged against the held-out half. A description tuned and judged on the same queries memorizes phrasings instead of learning the boundary.
- [ATTRIBUTION]: A must-fire miss is a starved or buried discriminant — fixed in the description's first clause. A must-not-fire hit is a boundary defect — fixed by a sharper discriminant or an added refusal, on both colliding skills. Neither is ever fixed in the body: the body is invisible to selection.
- [ACCUMULATION]: Every trigger miss observed in real work becomes a permanent query row; the sets only grow.

## [02]-[ADHERENCE_EVALS]

- [PAIRED_RUNS]: Each task runs twice under identical conditions — once with the skill installed, once baseline — launched together so timing and environment match. Revising an existing skill snapshots the prior version as the baseline.
- [BLIND_COMPARISON]: A judge scores both outputs against a task-specific rubric without knowing which run carried the skill, and is pushed off ties. Blindness is what separates measured value from confirmation.
- [UNBLINDING]: After the verdict, analysis unblinds and reads both transcripts to attribute the difference to specific instructions — which rule the winner obeyed, which the loser lacked or misread. Attribution, not the score, is what drives the next revision.
- [RATIONALIZATIONS]: Baseline transcripts are mined for the model's own justifications at the moment they deviate — recorded verbatim, each becoming the body rule or gate that forecloses it. Pressure re-runs iterate until a run yields no new rationalization; a rule authored against an imagined excuse rather than a recorded one is speculation wearing law.
- [DEAD_LAW]: A body rule that changes no output across the suite is dead weight and is deleted; the suite is the only defensible census of which lines earn their context.
- [NON_DISCRIMINATING]: An assertion that returns the same verdict under the skill and under the baseline measures nothing about the skill and silently inflates its score; it is flagged and cut. An assertion earns its place only when its verdict differs across the paired runs — the DEAD_LAW census turned on the suite that judges the body, not only on the body.

## [03]-[GRADERS]

- [DETERMINISTIC_FIRST]: Structure checks, gates, exit codes, and state assertions grade everything they can reach; rubric judgment covers only what determinism cannot.
- [RUBRIC_ISOLATION]: A model grader scores one dimension per rubric, calibrated against human verdicts on a sample, with partial credit and an admissible abstain verdict — a grader forced to answer fabricates.
- [ARTIFACT_NOT_PATH]: Grading binds to what the run produced, never to the tool-call sequence that produced it; a sequence check fails valid approaches the eval author did not anticipate.
- [TASK_QUALITY]: A task is admissible when two domain experts independently reach the same verdict on any output, and a reference solution proves both solvability and grader correctness.
- [BALANCE]: Suite carries positive and negative cases in both families; a one-sided suite optimizes one-sidedly — a suite of only must-fire queries breeds an over-triggering description.

## [04]-[TRIALS_AND_LIFECYCLE]

- [CONSISTENCY]: Runs are non-deterministic, so verdicts aggregate over trials: any-success measures whether the skill unlocks a capability at all; every-success measures whether it holds under repetition. A deliverable's consistency requirement picks the metric.
- [MODEL_VARIANCE]: Adherence runs cover every model the skill deploys under. A body tuned against a strong model under-guides a weaker tier and over-explains to a stronger one; a rule that changes output on the strong model but not the weak one is under-specified for the weak tier, not dead, and a rule the strong model never needs is over-explanation the weak tier still reads.
- [GRADUATION]: A capability suite starts hard with low pass rates and marks the frontier; once passing is routine it graduates into the regression suite, which stays near full marks and exists to catch decay.
- [SEEDING]: First suite is twenty to fifty tasks harvested from real failures and manual checks, prioritized by impact — never synthetic tasks invented to pass.
- [TRANSCRIPTS]: Failures are read, not counted: the transcript decides whether a miss is trigger, routing, freedom mismatch, or law, and routes the fix to the description, a route row, a band change, or a body rule. A saturated task teaches nothing and retires.
- [TRAVERSAL]: That same read covers how the run moved through the bundle — a reference no run opens, a file previewed with `head` instead of read whole, exploration in an order the routing did not predict, one file carrying every read. Promote an over-read file into the root, sharpen the route row runs ignore, re-signal or delete a file no run reaches; DEAD_LAW indicts a rule by its output, traversal the structure by its use.

## [05]-[TOOLING]

- [ROLE_SPLIT]: Grading, comparison, and attribution run as separate fresh-context agents — a grader verifying each run against its task's expectations, a blind comparator ranking the pair, an unblinded analyzer attributing the difference to specific instructions. One agent holding every role leaks run identity into the verdict.
- [DURABLE_SUITE]: Query sets, verdicts, transcripts, and revision history persist as typed JSON beside the bundle under iteration; a revision measures against recorded baselines, and the suite outlives the session that built it.
- [ISOLATION]: Each trial runs in a clean environment — no shared state, cached artifacts, or history leaking between the paired runs; an environment-correlated failure indicts the harness, never the skill.

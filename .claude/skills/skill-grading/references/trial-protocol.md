# [TRIAL_PROTOCOL]

Fresh-context trials compare skill behavior against baseline behavior through paired prompts, mechanical assertions, blind grading, and deterministic aggregate output. The protocol measures deltas; it never accepts author confidence as evidence.

## [01]-[EVAL_SHAPE]

The eval manifest carries one skill name and one prompt batch.

```json
{
    "skill_name": "<skill-name>",
    "evals": [
        {
            "id": "<case-id>",
            "prompt": "<realistic task prompt>",
            "expected_output": "<observable result>",
            "files": ["<fixture-path>"],
            "expectations": [
                {
                    "id": "<expectation-id>",
                    "kind": "script",
                    "command": ["python3", "<assertion-script>", "<run-output>"],
                    "text": "<mechanical assertion>"
                }
            ]
        }
    ]
}
```

Fields are stable contract.

| [INDEX] | [FIELD]             | [VALUE]              |
| :-----: | :------------------ | :------------------- |
|  [01]   | `skill_name`        | skill under trial    |
|  [02]   | `evals[].id`        | stable case key      |
|  [03]   | `evals[].prompt`    | realistic task       |
|  [04]   | `expected_output`   | observable target    |
|  [05]   | `files`             | fixture list         |
|  [06]   | `expectations`      | assertion list       |

Prompts name the work, input symptoms, target artifact, and success shape. Prompts do not mention the skill name, skill folder, skill file, bundled references, or intended activation path.

## [02]-[BATCHING]

Each eval runs twice in one batch: baseline and candidate. Baseline is the skill disabled, absent, or held at the previous version. Candidate is the skill under trial.

[PAIRING]:
- Same eval batch.
- Same fixture bytes.
- Same tool policy.
- Same model class.
- Fresh context per run.
- No authoring-session context.
- No leaked expected verdict.

Run order is randomized or alternated by case key. The grader receives anonymized output labels until all assertions and rubric scores are recorded.

## [03]-[CAPTURE]

Every run emits a complete evidence packet.

| [INDEX] | [ARTIFACT]       | [CAPTURED]        |
| :-----: | :--------------- | :---------------- |
|  [01]   | transcript       | full message log  |
|  [02]   | output files     | file snapshots    |
|  [03]   | tool calls       | ordered events    |
|  [04]   | mutations        | changed paths     |
|  [05]   | tokens           | usage count       |
|  [06]   | duration         | elapsed time      |
|  [07]   | output size      | byte count        |
|  [08]   | transcript size  | byte count        |
|  [09]   | activation       | harness events    |

Activation-positive prompts require a skill event when the harness exposes one. Activation-negative near misses require no skill event. A harness without skill events records activation as unavailable and removes activation claims from the hard aggregate.

## [04]-[ASSERTIONS]

Mechanical assertions precede judgment. A script assertion checks facts the artifact itself exposes: file existence, schema validity, row count, link count, JSON validity, rendered dimensions, command output, HTML parse, diagram parse, or document render probe.

[ASSERTION_LAW]:
- Assertions fail known bad output.
- Assertions pass known good output.
- Assertions emit `text`, `passed`, and `evidence`.
- Assertions carry exact failure detail.
- Assertions avoid model judgment first.
- Model grading enters only after mechanical coverage ends.

Discriminativeness is mandatory. An assertion that passes both bad and good outputs is deleted or rebuilt before the trial result is trusted.

## [05]-[BLIND_GRADE]

The grader sees anonymized output packets before unblinding. It scores every rubric axis and records assertion verdicts without knowing baseline or candidate identity.

[UNBLINDING]:
1. Freeze assertion verdicts and rubric scores.
2. Reveal candidate labels.
3. Compute deltas.
4. Flag always-pass and always-fail assertions, high-variance cases, and cost outliers.

Judgment comments cite evidence from the run packet. A comment without packet evidence is discarded.

## [06]-[AGGREGATE]

The aggregate report carries one row per eval and one total row.

| [INDEX] | [METRIC]     | [BASIS]             |
| :-----: | :----------- | :------------------ |
|  [01]   | pass rate    | assertion verdicts  |
|  [02]   | delta        | paired difference   |
|  [03]   | variance     | repeated runs       |
|  [04]   | tokens       | usage totals        |
|  [05]   | time         | duration totals     |
|  [06]   | tool calls   | event count         |
|  [07]   | activation   | harness events      |
|  [08]   | rubric       | blind scores        |

Stored regression keys derive from skill hash, model, runner, fixture hash, and tool policy. The key is deterministic. Interpretation of drift remains judgment.

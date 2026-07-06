---
name: skill-grading
description: >-
  Grades, lints, evaluates, and improves Claude Code skills through deterministic static checks
  and before/after skill trials. Use when grading a skill, linting the skills, evaluating skill
  quality, improving a skill, running before/after skill trials, checking skill regression, or
  answering "grade this skill", "lint the skills", or "is this skill good".
---

# [SKILL_GRADING]

Skill quality is measured, never asserted. Static defects fail deterministically; behavioral claims earn proof through fresh-context paired trials; subjective quality stays outside the hard rail until a calibrated grader owns it.

## [01]-[LINT]

Run the bundled lint before grading by judgment.

```bash
python scripts/skill_lint.py <skill-dir>...
```

Machine consumers use NDJSON.

```bash
python scripts/skill_lint.py --json <skill-dir>...
```

The lint vocabulary is closed.

| [INDEX] | [CHECK]                       | [STATUS]      |
| :-----: | :---------------------------- | :------------ |
|  [01]   | `frontmatter-name`            | fail          |
|  [02]   | `frontmatter-description`     | fail          |
|  [03]   | `frontmatter-fields`          | fail          |
|  [04]   | `body-size`                   | fail or warn  |
|  [05]   | `path-reachability`           | fail          |
|  [06]   | `reference-depth`             | fail          |
|  [07]   | `bold-prose`                  | fail          |
|  [08]   | `caps-label`                  | fail          |
|  [09]   | `table-index`                 | warn          |
|  [10]   | `read`                        | fail          |

Rows use `{"file","line","check","status","detail"}`. Human output uses `file:line: FAIL|WARN <check> <detail>`. Exit code `0` means no fail rows.

[CHECK_CONTRACT]:
- `frontmatter-name`: name matches folder and hyphen grammar.
- `frontmatter-description`: description exists, fits the length bound, and carries trigger language.
- `frontmatter-fields`: only admitted metadata keys exist.
- `body-size`: `SKILL.md` stays navigable.
- `path-reachability`: linked bundled paths resolve on disk.
- `reference-depth`: reference pages do not link to reference pages.
- `bold-prose`: prose never uses bold as emphasis.
- `caps-label`: standalone labels stay bracketed.
- `table-index`: enumerable tables expose `[INDEX]`.
- `read`: unreadable files fail as rows.

[OUTPUT_CONTRACT]:
- `fail` rows block the rail.
- `warn` rows report quality pressure.
- Empty stdout is a clean human run.
- Empty NDJSON is a clean machine run.
- Read failures never crash the process.

## [02]-[TRIAL]

Trial mechanics live in [references/trial-protocol.md](references/trial-protocol.md). A trial compares a skill against a baseline or previous version by running the same realistic prompt batch twice, with fresh context for every run.

[TRIAL_LAW]:
- Prompts name tasks and symptoms; they do not name the skill.
- Paired runs execute in the same batch to reduce temporal drift.
- Each run starts from fresh context.
- Blind grading happens before unblinding.
- Mechanical assertions run as scripts first.
- Assertion burden sits on the expectation.
- A passing result without discriminative assertions is false confidence.

Captured evidence includes transcript, output files, tool calls, file mutations, tokens, duration, output size, and transcript size. Activation evidence comes only from harness events when the harness exposes them.

[RUN_ORDER]:
1. Build the prompt batch.
2. Freeze fixtures.
3. Execute baseline and candidate runs.
4. Capture every output packet.
5. Run mechanical assertions.
6. Grade blind.
7. Unblind labels.
8. Compute deltas.
9. Store aggregate output.

[ASSERTION_PRIORITY]:
- File assertions precede transcript assertions.
- Schema assertions precede model judgment.
- Render assertions precede visual judgment.
- Activation assertions require harness events.
- A missing event stream removes activation from hard scoring.

## [03]-[RUBRIC]

Score every axis from `0` to `5`; anchors define the hard endpoints and the weak middle.

| [INDEX] | [AXIS]                         | [FAIL_0]              | [WEAK_3]              | [STRONG_5]             |
| :-----: | :----------------------------- | :-------------------- | :-------------------- | :--------------------- |
|  [01]   | `trigger-precision`            | positive misses       | partial firing        | positives fire         |
|  [02]   | `trigger-recall`               | negatives fire        | occasional false fire | negatives stay silent  |
|  [03]   | `output-pass-rate`             | assertions fail       | trivial pass          | substantive pass       |
|  [04]   | `delta-vs-baseline`            | no lift               | marginal lift         | clear lift             |
|  [05]   | `cost-delta`                   | wasteful blowup       | proportional gain     | gain justifies cost    |
|  [06]   | `variance`                     | unstable repeats      | some drift            | repeats converge       |
|  [07]   | `assertion-discriminativeness` | bad output passes     | weak separation       | bad output fails       |
|  [08]   | `instruction-following`        | unchanged process     | partial shift         | process changes        |
|  [09]   | `script-leverage`              | helpers reimplemented | mixed helper use      | helpers used           |
|  [10]   | `progressive-disclosure`       | body becomes textbook | partial loading       | demand loads detail    |
|  [11]   | `permission-posture`           | ambient risk          | broad grants          | grants match task      |
|  [12]   | `portability`                  | harness breakage      | minor breaks          | harness survives       |

Aggregate output carries pass rate, deltas, variance, time, tokens, tool-call count, transcript size, assertion verdicts, and activation verdicts.

[SCORING_LAW]:
- `0` means the axis fails its purpose.
- `3` means the axis partially holds.
- `5` means the axis carries evidence.
- Deltas outrank absolute scores.
- Variance discounts isolated wins.
- Cost counts only against measured gain.

## [04]-[LIMITS]

Deterministic lint is a floor, not a complete grade.

[HONESTY]:
- Subjective writing, design, and diagram quality need calibrated model grading with agreement checks.
- Activation telemetry depends on the harness event stream.
- Regression stores use deterministic keys over skill hash, model, runner, fixtures, and tool policy.
- Drift interpretation remains judgment even when the key is deterministic.
- Eval meaningfulness is a grading claim, not a lint claim.

## [05]-[GOTCHAS]

- A passing grade on a weak assertion creates false confidence.
- An eval that mentions the skill by name tests activation theatre.
- Authoring-session residue hides missing instructions.
- Fresh context is mandatory for every trial run.
- A baseline that already passes leaves no delta claim.
- A static pass never proves behavioral improvement.

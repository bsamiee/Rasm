# [REVIEW]

Steps here join `SKILL.md` [STEPS] for grading workflows, reading `scripts/runs.py` output, linting, trials, and refactoring.

## [01]-[READINGS]

`scripts/runs.py` fields each answer one question, and each answer names the layer of `SKILL.md` [01] owning its fix:

| [INDEX] | [FIELD]         | [MEANS]                                                  | [DECIDES]                               |
| :-----: | :-------------- | :------------------------------------------------------- | :-------------------------------------- |
|  [01]   | `first attempt` | Runs passing with no rerun                               | Flake and defect load a person feels    |
|  [02]   | `pending`       | Run creation to its first job, the concurrency wait      | Group key and queue setting             |
|  [03]   | `wall`          | First job created to last job completed                  | Critical path length                    |
|  [04]   | `last`          | Job finishing last, counted per run                      | Which job's p90 bounds `wall`           |
|  [05]   | `start`         | Job creation after the run's first job, its `needs` wait | Graph hops on that job's path           |
|  [06]   | `queue`         | Job creation to runner pickup                            | Runner capacity and concurrency limits  |
|  [07]   | `p50`, `p90`    | Duration percentiles over the runs read                  | Typical and tail cost                   |
|  [08]   | `max`           | Longest passing run                                      | `timeout-minutes` at it plus a tenth    |
|  [09]   | `flaky`         | Failed on attempt 1 in runs a rerun turned green         | Flakes to fix at their cause            |
|  [10]   | `skipped`       | Runs skipping the job, `n=0` skipping it every time      | Dead paths to delete                    |
|  [11]   | Step rows       | Steps holding a tenth of their job's p90 or more         | Which step a change targets             |
|  [12]   | `cold`          | Runs with a cache `miss`, against warm runs              | Whether keys or scope cost time         |
|  [13]   | `billed`        | Hosted minutes, each job rounded up                      | Cost of a label at its per-minute price |
|  [14]   | Cache rows      | Entries, size, `other` refs, and outcomes per key shape  | Key design and save gating              |

Readings follow distributions:
- Hosted runners differ in CPU model and image version, inter-runner variation reaching 14%, so single runs prove nothing
- Changes count past both 20% and 60 seconds on p90 with 20 runs or more per side, the PostHog band, set there on p95 over 7-day windows
- Durations split by runner image `Version` where one window holds more than one, as hosted images roll weekly over 2 to 3 days
- Percentiles with p90 at 5 times p50 or more mark a bimodal job (warm and cold, affected and unaffected), split by `cold` and step rows first
- `last` dominated by one job names the job whose speed moves `wall`, and work elsewhere changes nothing a person waits for
- `pending` above `wall` means runs wait on each other longer than they run, a concurrency fix ahead of speed work
- Filtered listings return 1000 runs at most, and run history ends at the repository's retention, 90 days at most on public repositories

## [02]-[LINT]

Linters decide what is a finding, and review explains, refutes, and fixes each:
1. Read the linter config first, as `jactionlint` profile `correctness` leaves cache poisoning, credential persistence, and checkout rules off
2. Run review under the default profile (`jactionlint --profile default --format oneline`), and `--online` with `GH_TOKEN` for impostor commits
3. Run `poutine analyze_local .` for known-vulnerable build components, and CodeQL `actions` queries for taint across composite actions and reusable workflows
4. Read exit codes as the result: `zizmor` exits 0 clean, 11 to 14 with findings by severity, 1 to 3 when no scan ran
5. Refute each security finding against its trigger's trust: a finding stands where a lower-trust actor reaches a step holding a higher scope
6. Fix at the line, suppress with the linter's ignore form and a reason, or disable a rule in config where it never applies
7. Rerun to 0 findings at the chosen severity, as a workflow that never ran stays unverified

Fixes by rule:
- `unpinned-uses`, `missing-version-comment`, `ref-version-mismatch` -> SHA resolved from the tag, its tag as comment
- `excessive-permissions`, `missing-permissions` -> per-job scopes matching the steps
- `artipacked` -> `persist-credentials: false`
- `cache-poisoning` -> no restore in jobs building release files, `cache-mode: none`
- `template-injection` -> expression moved into `env:`, quoted in shell
- `concurrency-cancels-prs`, `concurrency-deadlock`, `gate-job-skipped-on-failure` -> the forms of `references/jobs.md`
- `missing-timeout` -> `timeout-minutes` from the job's `max`
- `unsound-ternary` -> `case()`
- `secrets-inherit` -> secrets listed by name in the calling job

## [03]-[RUBRIC]

Gates hold or fail on evidence, and performance reports measured deltas, never a blended score:

| [INDEX] | [GATE]     | [HOLDS_WHEN]                                                                      | [EVIDENCE]                       |
| :-----: | :--------- | :-------------------------------------------------------------------------------- | :------------------------------- |
|  [01]   | Lint       | Repository linter exits 0, `default` and `poutine` findings judged                | Linter output, exit codes        |
|  [02]   | Armed      | One injected break per gated class turns its rule red on a scratch copy           | Break, rule, red run             |
|  [03]   | Weakening  | No new write scope, privileged trigger, mutable ref, `secrets: inherit`, rule off | Workflow and config diff         |
|  [04]   | Tokens     | `permissions: {}` opening, per-job scopes, OIDC over stored keys                  | `permissions:` blocks, secrets   |
|  [05]   | Pinning    | SHA with tag comment, `$/` for own files, images by digest                        | `uses:` and `image:` lines       |
|  [06]   | Untrusted  | No head code or values reach a job holding secrets or write scopes                | Triggers, checkouts, `env:` keys |
|  [07]   | Bounds     | `timeout-minutes` on every job near its `max`                                     | Job keys, `max=` per job         |
|  [08]   | Gate check | One static gate reading every job, no `paths:` on required checks                 | Gate job, `on:` blocks           |
|  [09]   | Verified   | Workflow ran on its trigger route, `gh run watch` exiting 0                       | Run id, plan diff for a refactor |

Armed breaks per class: `${{ github.event.issue.title }}` inside `run:`, `permissions: write-all`, a tag-pinned third-party action, and a head checkout under `pull_request_target`, so a profile leaving a class's rule off fails that class.

Performance dimensions each report a delta against a baseline window, a weak one stated on its own row:

| [INDEX] | [DIMENSION] | [DELTA]                                                            | [EVIDENCE]                              |
| :-----: | :---------- | :----------------------------------------------------------------- | :-------------------------------------- |
|  [01]   | Speed       | `wall` and `last` job p50 and p90, queue p90 per runner label      | Runs over successful first attempts     |
|  [02]   | Reliability | First-attempt rate, failure signatures, time to green              | `first attempt`, `failures`, `to green` |
|  [03]   | Cost        | Billed cost per run at equal coverage, cache bytes against the cap | `cost`, cache rows                      |
|  [04]   | Structure   | Dead paths, facts declared twice, jobs skipped in every run        | Linter `unused-*` rules, `skipped`      |

Report form, one row per gate and dimension:

```text
review <owner/repo> <workflow or change>
window: <first>..<last> runs=<n>
baseline: <first>..<last> runs=<n>
lint: pass profile=default findings=0 refuted=1
armed: fail class=cache-poisoning rule=off
speed: wall p50=6m52s (-38s) p90=10m55s (-12s)
reliability: first-attempt=94% runs=50 signatures=2
gap: logs expired runs=6
```

- Rows cite their evidence line, and a delta without a window and run count counts as unmeasured
- Critics move a row with a run or a file line, never a preference

## [04]-[IMPROVEMENTS]

Changes count by what a person or a bill measures afterward:

| [INDEX] | [CHANGE]                          | [REAL_WHEN]                                         | [FALSE_WHEN]                                   |
| :-----: | :-------------------------------- | :-------------------------------------------------- | :--------------------------------------------- |
|  [01]   | Removing a job hop                | `start` of every later job falls by the hop's setup | Its work moved into each matrix row            |
|  [02]   | Adding a cache                    | Restore p90 plus save cost sits below the rebuild   | Restore of a large entry outlasts an install   |
|  [03]   | Background steps                  | Main step p50 holds while the job's p50 falls       | Background work takes a CPU-bound step's cores |
|  [04]   | Narrower selection                | Every input reaching a task still selects it        | Coverage drops for a faster number             |
|  [05]   | Paid or larger runner             | `wall` p90 falls because the job is `last`          | Jobs off the critical path get faster          |
|  [06]   | Splitting into matrix rows        | Rows run in parallel, each restoring its own state  | Each row pays the same setup and compile       |
|  [07]   | Folding matrix cells              | Cells' p50 sat below their own setup time           | Folded cells push the job past `last`          |
|  [08]   | Retry or `continue-on-error`      | Never, a cause fix replaces each                    | Always, as it hides its cause                  |
|  [09]   | Generated YAML or wrapper scripts | Never where one team edits the workflows            | Always, a second source for every fact         |

Measured VM-time savings across 952 repositories rank job timeouts first, then pull request cancellation, caching, `fail-fast`, and trigger filters.

## [05]-[TRIALS]

Trials prove a change before it merges, on the route its trigger allows:
1. Baseline: `scripts/runs.py` over 20 runs or more of the workflow, branch, and event the change targets
2. Plan diff: `jactionlint run --list --json` before and after, where equal jobs, matrix cells, `needs` stages, and `if` verdicts prove a pure refactor
3. Local run: `jactionlint run` executes `run:` steps, matrices, background steps, and composite actions in an OS sandbox, by the limits below
4. Branch run on the trigger's route below, typed input `uncached: true` timing cold work, then a second run timing warm work
5. Compare: `scripts/runs.py <run URL>` per trial against the baseline, kept past the band of [01]
6. Record: the setting, its measured reason, and what reopens it, where the repository records settings

Trial routes by trigger:
- `push` -> push the branch
- `pull_request` -> a draft pull request
- `workflow_dispatch` -> `gh workflow run <file> --ref <branch>`, once the file has run on the default branch
- `merge_group` -> a merge queue entry
- `schedule` -> a `workflow_dispatch` trigger beside it in the same file
- `pull_request_target`, `workflow_run` -> none on a branch, as both run the default branch's file

Local runners each leave facts unmodeled:
- `jactionlint run` -> no `permissions`, `concurrency`, `environment` protection, OIDC, billing, Windows steps, hosted image tools, or real event payload
- `act` -> ignores `concurrency`, `run-name`, `permissions`, `timeout-minutes`, `continue-on-error`, OIDC, `environment`, summaries, and annotations
- `act` gaps -> `case()`, `background` and `parallel` steps, `$/`, and `cache-mode` each fail, so workflows using them take `jactionlint run`
- `act` guard -> act sets `ACT`, so steps needing GitHub's artifact service or secrets skip locally with `if: ${{ !env.ACT }}`

## [06]-[REFACTORING]

Refactors reduce declarations and keep every run's behavior:
1. Parse every workflow and action once (`yq -o json`), keying each step by its `uses` without ref plus `with`, or its folded `run` text
2. Shared step sequences and job graphs -> the reusable form `SKILL.md` names for the layer they repeat
3. Facts declared twice (tool versions, env values, timeouts, groups) -> one declaration at the file owning them, YAML anchors inside one file
4. Jobs skipped in every run of the window (`n=0`) -> deleted with each `needs` naming them, unless a trigger the window lacks runs them
5. Sibling jobs differing in values alone -> one matrix job, `name: ${{ matrix.<key> }}` keeping check names stable
6. Each candidate kept only when step 5 of [05] shows equal or lower p50 and p90 for the jobs it touches

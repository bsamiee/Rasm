---
name: actions
description: "Use when writing, reviewing, measuring, or debugging GitHub Actions workflows, composite actions, reusable workflows, runners, and caches, covering job graphs, matrices, concurrency, cache scope, tokens, releases, and run-history analysis"
argument-hint: "[analyze|diagnose|change] [workflow files] [run id or URL]"
---

# [GITHUB_ACTIONS]

Arguments `$ARGUMENTS` name a mode (`analyze` by default, `diagnose`, or `change`), workflow or action files, and a run in any order, each told by its form, and the request supplies values they lack:
- Mode: `analyze` measures and grades, `diagnose` explains one run, `change` writes or rebuilds, and a run alone selects `diagnose`
- Files: paths under `.github/` narrow every step to them and the jobs they reach, every workflow and action when absent
- Run: a run id or `https://github.com/<owner>/<repo>/actions/runs/<id>` URL, else the newest failed run `scripts/runs.py` names

Work in one category below sends your next call to Read its reference before any edit or run, its steps joining [STEPS] for that category; Read it again after a compaction: triggers, job graphs, matrices, gates, data between jobs, or concurrency -> `references/jobs.md`, caches, state stores, runners, disk, or minutes -> `references/caching.md`, tokens, OIDC, pinning, untrusted events, environments, or publishing -> `references/security.md`, grading, lint findings, trials, or `scripts/runs.py` output -> `references/review.md`.

[REFERENCES]:
- [01]-[JOBS](references/jobs.md): Triggers, job graphs, matrices, data between jobs, gates, run and step concurrency
- [02]-[CACHING](references/caching.md): Cache keys and scope, state stores, runner choice, disk, billed minutes, self-hosted runners
- [03]-[SECURITY](references/security.md): Tokens, OIDC, pinning, untrusted events, environments, and registry publishing
- [04]-[REVIEW](references/review.md): Run-history readings, grading rubric, trials, and lint findings

[TEMPLATES]:
- [01]-[CI](templates/ci.yml): Planner job, dynamic matrix, single gate job, cancel-on-PR concurrency, default-branch cache writes
- [02]-[ACTION](templates/action.yml): Composite setup action with typed inputs and a restore and save split
- [03]-[RELEASE](templates/release.yml): Dispatch release with dry run, serial queue, uncached build, OIDC publishing, and attestations

[SCRIPTS]:
- [01]-[RUNS](scripts/runs.py): Run history into duckdb, reporting waits, last jobs, percentiles, cache outcomes, and cold jobs, or a run's failures

Files and linters read as Claude Code loads this skill, and a Read of this file sends each line as its own parallel call first:

```!
git ls-files -- .github 2>/dev/null || true
command -v jactionlint actionlint zizmor gh 2>/dev/null || true
```

## [01]-[LAYERS]

Workflow runs stack layers, each owning one decision and composing with its neighbors, so a change lands at the layer owning its decision:

| [INDEX] | [LAYER]         | [UNIT]                                   | [DECIDES]                                      |
| :-----: | :-------------- | :--------------------------------------- | :--------------------------------------------- |
|  [01]   | Event           | `on:` trigger and its filters            | Runs that exist, token trust, cache scope      |
|  [02]   | Workflow        | `.github/workflows/<file>.yml`           | Run-wide `permissions`, `env`, `cache-mode`    |
|  [03]   | Graph           | `jobs.<id>` and `needs`                  | Order, parallel width, runner starts           |
|  [04]   | Matrix          | `strategy.matrix`, static or `fromJSON`  | One job shape over many values                 |
|  [05]   | Runner          | `runs-on`, `container`, `services`       | CPU, disk, OS, billed minutes, kept state      |
|  [06]   | Step            | `run`, `uses`, `background`, `wait`      | Work order inside one runner                   |
|  [07]   | Action          | Composite, JavaScript, or Docker action  | One reusable step sequence or program          |
|  [08]   | Called workflow | `on: workflow_call`, `jobs.<id>.uses`    | One reusable job graph with inputs and secrets |
|  [09]   | State           | Actions cache, artifacts, outside stores | What a fresh runner starts with                |
|  [10]   | Trust           | `GITHUB_TOKEN`, OIDC token, environments | What a job writes, which secrets it reads      |

Each reusable form takes the layer it repeats:
- Steps repeated across jobs -> composite action at `.github/actions/<name>/action.yml`, called as `uses: $/.github/actions/<name>`
- Job graphs repeated across workflows -> reusable workflow, its matrix and concurrency on the calling job, secrets listed by name
- Logic needing the API, a post step, or `ACTIONS_CACHE_MODE` -> JavaScript action on `node24`, which reads that variable where `run` steps cannot
- Values repeated inside one file -> YAML anchor (`&name`, `*name`) over a whole step or value, as GitHub refuses merge keys (`<<:`)
- Jobs differing by values alone -> matrix rows, from a planner job's `fromJSON` output where a change selects them

Composite actions and reusable workflows differ at every boundary:

| [INDEX] | [BOUNDARY]  | [COMPOSITE_ACTION]                                | [REUSABLE_WORKFLOW]                                                    |
| :-----: | :---------- | :------------------------------------------------ | :--------------------------------------------------------------------- |
|  [01]   | Runs on     | Caller's job and runner, logged as one step       | Own jobs and runners, every step logged                                |
|  [02]   | Inputs      | Strings alone, compared as `inputs.<x> == 'true'` | Typed `boolean`, `number`, `string` inputs                             |
|  [03]   | Secrets     | Arrive as inputs, no `secrets` context            | `secrets:` by name, `inherit` inside one owner                         |
|  [04]   | Step keys   | No `timeout-minutes`, `background`, or `parallel` | Every job and step key                                                 |
|  [05]   | Environment | Caller's `env` and contexts                       | Values cross as inputs and outputs alone                               |
|  [06]   | Outputs     | `value:` from a step output                       | `value:` reading `jobs.<job>.outputs`, last row to complete and set it |
|  [07]   | Limits      | 10 nesting levels                                 | 10 nesting levels, 50 per workflow file                                |

Calling jobs take `with`, `secrets`, `strategy`, `needs`, `if`, `concurrency`, `permissions`, and `cache-mode` alone, `runs-on` and `timeout-minutes` sitting in the called workflow, and `with:` reads no `env`, so values cross as `vars` or literals.

Facts every layer obeys:
- `$/` reads the workflow's repository at the running commit with no checkout and no `@ref`, failing on any dangling symlink
- `case(<predicate>, <value>, ..., <default>)` returns the first true predicate's value, where `a && b || c` returns `c` on a falsy `b`
- Outputs are strings, so `if: needs.<job>.outputs.<flag>` is true for `'false'`, compared as `== 'true'` or read through `fromJSON`
- Job outputs hold 1 MB each and 50 MB per run, an output matching a secret is dropped, and an empty one is never set
- `strategy` reads `github`, `needs`, `vars`, and `inputs` alone, and job-level `if` reads no `matrix`
- Workflow-level `concurrency` reads `github`, `inputs`, and `vars`, and job level adds `needs`, `strategy`, and `matrix`
- `hashFiles` returns an empty string when no file matches, so a mistyped glob keys one entry across every change
- Event fields (titles, branch names, labels, issue bodies) reach shell text through `env:` alone, quoted in shell, as `${{ }}` inside `run:` splices them into the script
- `shell: bash` written out runs `bash --noprofile --norc -eo pipefail {0}`, the implicit default runs without `pipefail`, and container jobs default to `sh`
- Jobs default to 360 minutes, `GITHUB_TOKEN` expires after 24 hours, and a matrix expands to 256 jobs
- `$GITHUB_STEP_SUMMARY` holds 1 MiB per step and shows 20 per job, the place for results a reader acts on, linking a job as `actions/runs/${{ github.run_id }}/job/${{ job.check_run_id }}`
- Problem matchers and `::error` lines annotate 10 errors, 10 warnings, and 10 notices per step and 50 per job, the rest in logs alone

## [02]-[STEPS]

Every mode starts with steps 1 and 2, `analyze` ends after 2 with the report of step 6, `diagnose` adds 3, and `change` adds 4 to 6:
1. Read: every workflow, composite action, and reusable workflow whole, the linter config, and settings the repository records with a reason
2. Measure: `uv run --locked --script scripts/runs.py <owner/repo> [--workflow <file>] [--runs <n>] [--branch <ref>] [--event <event>]`
3. Diagnose: `uv run --locked --script scripts/runs.py <run URL>`, then the cause case below, then the fix at that cause's layer
4. Change: the smallest layer of [01] owning the decision, one declaration per fact, a template of [TEMPLATES] as a new file's base
5. Verify: the repository's lint gate at 0 findings, `jactionlint --profile default` (`--online` with `GH_TOKEN`) findings judged by `references/review.md`, the trial route of the trigger, `gh run watch <id> --exit-status`, then step 3's command
6. Report: subject `actions <mode> <scope>`, `label: value` facts, rows in `scripts/runs.py` field order, each change with its delta and run

Each step carries one condition:
- Measure runs once in the background over many runs, and readings take p50 and p90 by `references/review.md`, never one run
- Settings recorded with a measured reason change on new evidence alone
- Removals name what the removed form protected and where that protection now sits
- Templates hold `<placeholder>` commands (`<plan>` printing matrix JSON, `<task>`, `<install>`, `<build>`, `<publish>`) the repository fills
- Verification compares the branch run against the baseline of step 2, the change kept past the noise band `references/review.md` states

Diagnosis cases, matched from the failed step's log and annotation lines:
- Output names a failing test, lint, or build task -> code defect, fixed in the code, never by a retry or `continue-on-error`
- Step fails before its command (`Set up job`, action download, `Collection was modified`) -> runner or action preparation, `references/jobs.md`
- Job ends on disk, memory, or a lost runner -> runner capacity, `references/caching.md`
- Step runs past its p90 with a cache `miss` -> cold state, `references/caching.md`
- `pending` above its p50 -> concurrency group, `references/jobs.md`
- `Resource not accessible by integration`, a `403`, or an OIDC refusal -> trust layer, `references/security.md`
- One failure signature on unrelated commits -> flake, fixed at its cause, as fork pull requests get no rerun to prove it

## [03]-[LINT]

Linters read workflow files alone and run before every push, the first one installed taking the role:

| [INDEX] | [LINTER]      | [COVERS]                                                        | [RUN]                                  |
| :-----: | :------------ | :-------------------------------------------------------------- | :------------------------------------- |
|  [01]   | `jactionlint` | Checks of `actionlint`, zizmor audits as rules, step keys       | `jactionlint --strict-exit`            |
|  [02]   | `zizmor`      | Security audits, `self-repository`, `cache-poisoning`, lockfile | `zizmor --min-severity medium .github` |
|  [03]   | `actionlint`  | Syntax, expressions, shellcheck                                 | `actionlint`                           |

- Gates run the repository's own linter config, and review adds the default profile, whose extra findings are candidates
- Findings are fixed at their line, a suppression naming its reason in the linter's ignore form
- `jactionlint update --apply` moves each pinned action to its newest release SHA with its version comment and relocks `actions.lock`
- Runner labels sit in the linter config under `strict-labels`, so a new label joins it in the same change
- `actionlint` predates `background`, `cache-mode`, and `$/`, so its findings on them are false

## [04]-[RUNS]

`scripts/runs.py` writes only its store, `~/.cache/github-actions/<owner>/<repo>/<run>.json` per completed run, so a rerun reads new runs alone:
- Target: `owner/repo`, or none for the working folder's repository, reports history, and a run id or URL reports that run
- History reads attempt 1 of each completed run, as later attempts rerun a subset
- Cache outcomes come from `actions/cache` log lines, so stores logging other lines report none
- Logs expire with the repository's retention setting, and a `gap:` line counts runs read without them
- `next:` names the newest failed run's diagnosis

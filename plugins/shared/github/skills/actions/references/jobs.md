# [JOBS]

Steps here join `SKILL.md` [STEPS] for triggers, job graphs, matrices, data between jobs, gates, and concurrency, taken in that order.

## [01]-[TRIGGERS]

Triggers decide which runs exist, and every later layer reads their trust and ref:
1. List each workflow's `on:` events and the jobs each reaches, as one commit firing `push` and `pull_request` reports one check twice
2. Restrict `push` to the default and release branches where `pull_request` covers topic branches
3. Gate work inside the graph ([04]), never with `on.<event>.paths`, as a workflow its filter skips reports no check
4. Type every `workflow_dispatch` input (`boolean`, `choice`, `environment`, `number`, `string`), booleans defaulting safe (`dry-run: true`)
5. Name dispatch runs with `run-name:` over `inputs`, and default each input in expressions shared with `push` (`inputs.<x> || '<default>'`)
6. Add `merge_group` beside `pull_request` once a merge queue exists, as a workflow lacking it never reports to the queue
7. Give `schedule` entries a `timezone:` where the hour matters

Event cases:
- `push`, `schedule`, `workflow_dispatch`, `repository_dispatch` -> trusted, scopes as declared, cache writes to the ref
- `pull_request` from the repository -> scopes as declared, caches saved under `refs/pull/<n>/merge` alone
- `pull_request` from a fork -> read token, no secrets, no OIDC token, first-time contributors held for approval
- `pull_request` with `types: [closed]` -> a run joining the pull request's group cancels its running CI, every job skipped by `github.event.action`
- `merge_group` -> takes `branches` and no `paths`, keyed in concurrency by `github.event.merge_group.head_sha`
- Events made with `GITHUB_TOKEN` -> start no run, `workflow_dispatch` and `repository_dispatch` aside, so chains go through `needs`, `workflow_call`, `gh workflow run`, or a GitHub App token
- Pull requests a workflow opens with `GITHUB_TOKEN` -> their `pull_request` runs wait for approval
- `workflow_run` -> privileged follow-up reading another run's artifacts, firing only for a file on the default branch, chains stopping at 3 levels
- `pull_request_target`, `issue_comment` -> base-branch code with secrets, restoring default-branch cache entries and saving none, `references/security.md` before any head checkout

## [02]-[GRAPH]

Job graphs trade a VM start, tool install, and checkout per job against parallel width:
1. Read `last:` and `start=` of `scripts/runs.py`: `start` waits on upstream jobs, `queue` on a runner
2. Remove each job whose output a step can compute, as every hop adds a runner start and its setup to the critical path
3. Keep a planner job where its output picks runners or rows, on `ubuntu-slim`
4. Set `timeout-minutes` on each job at its longest passing run plus a tenth (`max=`), as a p90 bound kills passing runs
5. Read `jactionlint --suggest-timeouts` for the same bound from the last 20 passing runs, the longest times 2, from 3 samples up
6. Bound network steps tighter than their job, one value converted for every step (`timeout-minutes: ${{ fromJSON(env.<NAME>) }}`)
7. Run expensive legs on the default branch or in the merge queue alone, the job's `if:` reading `github.ref` or `github.event_name`

Diff base for change detection:
- Pull request -> `git rev-parse HEAD^1` of the merge ref, as `pull_request.base.sha` can predate commits on the target
- Push -> the last green run's commit (`nrwl/nx-set-shas`, the runs API), else `github.event.before`, all zeros on a new branch
- History -> checkout with `fetch-depth: 0` and `filter: blob:none`, or shallow then `git fetch --no-tags --filter=blob:none --unshallow origin`
- No history -> `gh api repos/{owner}/{repo}/pulls/<n>/files --paginate`, capped at 3000 files, a full run forced at `changed_files >= 3000`

## [03]-[MATRICES]

Matrices hold one job shape over values, static where rows are fixed and planned where a change selects them:
1. Static rows -> vectors with `exclude:` and `include:`, `fail-fast: false` where every row's result matters
2. Planned rows -> a planner printing `matrix=<json>` to `$GITHUB_OUTPUT`, the job taking `include: ${{ fromJSON(...) }}` alone
3. Empty plans -> the job's `if:` reads `needs.<planner>.outputs.matrix != '[]'`, as an empty vector fails the run
4. Row objects -> each row carries `name`, `runner`, `container`, `timeout`, and nested objects, steps reading `matrix.<field>`
5. Row environment -> `env: { EXTRA: '${{ toJSON(matrix.env) }}' }` read by a step writing `$GITHUB_ENV`, as `env:` takes a whole object expression or literal keys, never both
6. Optional containers -> `container: ${{ matrix.container || null }}` on Linux rows alone, with `defaults.run.shell: bash`, as `run` steps default to `sh` inside a container
7. Reusable workflows -> matrix on the calling job, as a skipped called workflow never expands its matrix and leaves row checks pending
8. Width -> `max-parallel` where rows share a rate-limited service or must run in sequence, a shared `concurrency` group across jobs

Include and exclude rules:
- `exclude` runs before `include`, so an include entry adds back an excluded row
- Include entries matching a row on every original key add their new keys to it, and entries matching none add a row
- Include entries never overwrite an original value, and a later include never extends a row an earlier include added
- Partial exclude entries (`{arch: x86, debug: true}`) remove every row holding both values
- Rows lacking an added key read it as `matrix.<key> || ''`

Failure policy by row:
- Merge gating -> default `fail-fast`, one failure deciding the outcome and cancelling the rest
- Release matrices and experiments -> `fail-fast: false`, `continue-on-error: ${{ matrix.experimental == true }}` per row
- Allowed failures -> the gate lists them, as a row failing under `continue-on-error` reports `success` to `needs`

## [04]-[DATA]

Values cross jobs as outputs or artifacts, chosen by size and count:
- Flags and short lists -> job outputs, one JSON record per output read as `fromJSON(needs.<job>.outputs.<record>).<field>`
- Whole records into `env:` -> `env: ${{ fromJSON(needs.<job>.outputs.<record>) }}` makes each key a variable
- Per-row results -> artifacts named by row (`<kind>-${{ matrix.name }}`) with `overwrite: true`, never outputs
- Fan-in -> `actions/download-artifact` with `pattern: <kind>-*` and `merge-multiple: true`
- Reusable workflow outputs -> `on.workflow_call.outputs.<id>.value: ${{ jobs.<job>.outputs.<id> }}`
- Multiline values -> files, as `name<<DELIMITER` breaks on a value holding the delimiter line
- Reruns -> "Re-run failed jobs" reruns their dependents, and jobs left out keep their attempt's outputs and artifacts, so names carry no `github.run_attempt`

## [05]-[GATES]

Branch protection requires one check name, so one gate job reads every result:
1. Name the gate statically, list every job in its `needs`, and set `if: ${{ !cancelled() }}`, as a skipped job counts as success
2. Fail it on any result besides `success` and `skipped`: `jq --exit-status 'all(.[]; .result == "success" or .result == "skipped")'`
3. Read the results from `env: { NEEDS: '${{ toJSON(needs) }}' }`, and hold the gate at `permissions: {}`, `ubuntu-slim`, `timeout-minutes: 5`
4. List allowed failures and skips by name in the gate, computed from planner outputs where rows vary

Gating forms, coarsest first, each inside the graph:
- Planner flag -> `if: needs.<planner>.outputs.<flag> == 'true'` on a job, its check reported through the gate
- Step skip -> a kept job skips its steps, reporting success with no work
- Task graph -> `nx affected`, `turbo run --affected`, `cargo nextest run -E 'rdeps(<crate>)'` inside one job
- Test impact -> a planner writes the test files a change reaches as an artifact the test jobs read

## [06]-[CONCURRENCY]

Concurrency groups order runs, and the key decides which runs collide:
- Pull request CI -> `<workflow>-<pull request number>` with `cancel-in-progress: true`
- Push CI -> `<workflow>-${{ github.run_id }}`, never colliding, as a shared push group waits on the run before it
- Reruns -> `case(github.run_attempt == 1, github.event.pull_request.number, github.sha)`, so an old rerun never cancels the newest push
- Merge queue -> `github.event.merge_group.head_sha`, never cancelling
- Mixed events in one workflow -> one `case()` over `github.event_name` choosing each event's key above, cancel on pull requests alone
- Shared refs by design (try branches, bot pushes) -> grouped by `github.sha` on those refs alone
- Serial writers (deploy, release, tag move, publish) -> `<workflow>` with `queue: max` and no cancel
- Called workflows -> group on the calling job, as a group inside the called workflow takes the caller's workflow name and deadlocks

Queue rules:
- `queue: single` (default) keeps one pending run and cancels it when another arrives, losing a pending release
- `queue: max` keeps up to 100 pending runs in order of their wait start and refuses `cancel-in-progress: true`
- Group names compare case-insensitively, and `pending:` of `scripts/runs.py` measures each group's wait

## [07]-[STEPS]

Step concurrency runs independent work beside the critical step on one runner:
1. Mark each independent step (install, lint, download, disk cleanup) `background: true`, at most 10 at once per job
2. Join with `wait: <id>` or `wait: [<ids>]` before the first reader of its outputs or files, `wait-all: true` before readers of all
3. Stop a server step with `cancel: <id>` once its clients end, the runner sending SIGTERM then SIGKILL
4. Gate a background step with its own `if:`, as `wait`, `wait-all`, and `cancel` always run and take no `if`
5. Group a fixed set as `parallel: [<steps>]`, background steps with a `wait` at the group's end
6. Read failures at the join, as a failed background step fails the job at the next `wait` naming it

Cases keeping steps in sequence:
- Windows runners -> concurrent starts race on `event.json`, and `cancel` sends Ctrl+C to every running step
- Composite actions overlapping each other -> the runner's step list races (`Collection was modified`)
- Cache restores beside a step writing their folder -> join before the writer, as writers of one folder collide
- CPU-bound main step on 1 to 4 vCPU -> background work shares its cores, kept only where the main step's p50 holds

## [08]-[RERUNS]

Reruns recover infrastructure failures and never code failures:
- Network installs -> retried inside the step by the tool's own retry setting (`CARGO_NET_RETRY`, `UV_HTTP_RETRIES`, apt `Acquire::Retries`)
- Bad hosts -> a second job `needs` the first, `if: ${{ !cancelled() && needs.<job>.outputs.retry == 'true' }}`, its steps a YAML alias
- Default-branch flakes -> a `workflow_run` workflow on `failure`, `run_attempt < 3`, posting `repos/{owner}/{repo}/actions/runs/<id>/rerun-failed-jobs`
- Bound -> 50 reruns per run, the third failure reported to a person, pull request flakes left visible to their authors

# [WORKFLOWS]

Reasons and runner behavior of GitHub Actions workflows their files do not state.

## [01]-[CI]

Workflow `CI` and its jobs:
- Jobs select projects by tag through `nx affected --exclude`, as `nx affected` takes no `--projects`
- Job `affected` runs `nx sync:check` and `nx conformance:check`, as CI task runs skip sync and no Nx task runs conformance
- Job `macos` runs `nx conformance:check` again, as Apple projects and their targets enter only the macOS graph
- Job `affected` names the Playwright image Deploy publishes by the `@playwright/test` row of the pnpm catalog
- Runs at a Playwright version with no published image fail to pull it until Deploy on `main` publishes its tag
- Job `affected` runs `nrwl/nx-set-shas` once for every job but `unreal`, as a CI run finishing between jobs moves the base `nx-set-shas` finds
- `nx-set-shas` walks first-parent ancestors of `HEAD~1` (`per-commit`), as its default list can pick a later commit for a re-run of an older run
- `nx-set-shas` stops at the first commit with a successful push run, and each walked commit costs one API request
- `max-commits` (150) covers a streak of failed CI runs on `main`, as each run takes every commit pushed while the previous one ran
- Pushes to `main` run CI one at a time, so a walk to `max-commits` in each run stays under the token's 1000 requests an hour
- `nx-set-shas` falls back to the first commit where no successful run is in reach, and every task runs, as default `HEAD~1` checks one commit
- Manual runs skip `nx-set-shas` and take the first commit as base, so every task runs
- Job `linux` runs affected projects tagged `os:linux` or no `os:` on instance `host`, except `host:rhino`, `host:browser`, `host:unreal`
- Configuration `ci` of job `linux` adds GitHub annotations to `check--knip` findings
- Job `browser` runs `host:browser` projects on instance `container` with Chromium of its Playwright image
- Selenium Manager downloads stable Firefox and `geckodriver` for WebDriver suites, and the image holds the libraries Firefox loads
- `pack--playwright` renders in job `browser`, extension UI in its image's Linux `system-ui`
- Job `browser` runs Chromium in a job container, as on the host each network Docker adds fails queued requests with `net::ERR_NETWORK_CHANGED`
- Job containers and user `node` of the Playwright image take user `runner`'s uid, as every job of one instance shares its checkout
- Jobs `linux` and `browser` remove each report path before their run, as their kept checkout holds earlier runs' reports
- Job `macos` runs affected `os:darwin` projects except `host:rhino`, as no Linux graph holds an Apple project
- Job `macos` leaves out root `rasm`, as job `linux` runs its other checks and each Swift project's tasks depend on its Swift targets
- Job `macos` runs `hk check` steps `swiftlint`, `swift-format`, `plutil`, and `xcprojformatter` from base to head, as they need macOS
- Job `linux` runs every other `hk check` step before its tasks, so file findings arrive first, and its tasks run past a failed check
- Job `linux` runs every `codegen` target before `hk check`, as type-aware lint reads the sources those targets write
- Job `windows` starts only where job `affected` lists an `os:win32` or `host:rhino` project, and installs Rhino where one is `host:rhino`
- Job `windows` runs `test -c headless` over affected `os:win32` and `host:rhino` projects with core-hour token `RHINO_TOKEN`
- Job `unreal` runs the projects `affected` lists in Epic's `dev-slim` image of each engine a tracked `.uproject` names
- Unreal projects hold no target on a runner outside their engine image
- Unreal image pulls sign in to GHCR as the repository owner, the account `EPIC_GHCR_TOKEN` belongs to
- Job containers install mise tools in the runner's tool cache
- Windows jobs install mise tools under `C:\mise`, as `cmd.exe` finds no command once `PATH` passes 8191 characters
- CMake checks uninitialized variables in each file inside the source tree, vcpkg's toolchain file included where mise installs it there
- CI, Deploy, and Release checkouts fetch full history, as `nx affected` and `nx release` compare commits a partial clone fetches one at a time
- CI and Release checkouts on self-hosted runners keep ignored files (`clean: false`)
- Self-hosted jobs run tasks on half their cores (`NX_PARALLEL`), as instances `host` and `container` share one machine
- Action `tools` removes untracked files and bootstraps mise tools alone, and mise hook `postinstall` installs project dependencies
- GitHub-hosted runners and job containers load mise profile `ci`, installing pnpm packages alone, as Python tasks run on instance `host`
- Job `affected` loads profile `ci` too, as graph creation reads no Python environment and every other job waits for this job
- Profile `ci-<job id>` sets `enable_tools` to the tools a job's steps and Nx tasks run, and a job with no such file installs every tool
- Profiles list the tools graph creation spawns: `cargo`, `dotnet`, `uv`, and `jscpd`
- Profiles repeat those tools, as a later mise file replaces `enable_tools` whole
- Profile `ci-macos` holds `yq`, as `mise.macos.toml` reads the Rhino major through it
- Workflows name composite actions and reusable workflows by `$/` paths, read from the archive of the commit a run started from
- Runners fail a job before its first step where a tracked symlink of that archive names an untracked file

## [02]-[CACHE]

Nx remote cache and Actions caches per workflow and machine:
- Self-hosted Linux jobs keep `.cache/nx/workspace-data` between runs
- Jobs `macos`, `windows`, and `deploy` restore the newest `.cache/nx/workspace-data` saved on their OS, by key prefix `nx-<OS>-`
- Nx plugins check each entry of their `*.hash` file against its source files and recompute a stale one
- Graph caches leave out `cache`, the task database, `task-cache.jsonl` of one run's tasks, and the `xcode-packages` clones
- Graph cache paths name `workspace-data/*`, as an excluded folder inside a named folder still enters its archive
- Graph caches save after the run passes or fails, keyed by the hash of every `*.hash` file, as those change only with plugin entries
- Graph cache saves skip a hash the restored key ends with
- Caches save from runs on `main` alone, as a pull request run's cache serves re-runs of that pull request alone
- Past the 10 GB repository limit GitHub evicts caches by oldest last access
- Action `jdx/mise-action` saves a GitHub-hosted job's tools after install, keyed by platform, profile, `mise.toml`, `mise.lock`, profile files, and `rust-toolchain.toml`
- GitHub-hosted jobs hold rustup and cargo homes inside the mise folder that action caches, so a restored cache holds the Rust toolchain
- Row `nx-apple` of mise builds the Apple analyzer with `swift build` and writes its stamp before any Nx graph exists, as graph creation reads it
- Jobs on macOS restore that analyzer, its stamp, and mise freshness state by runner image and analyzer sources, so a changed source alone rebuilds it
- Tool cache keys leave out OS and machine mise files, as those state environment and bootstrap rows alone
- GitHub-hosted jobs cache the pnpm store by OS and `pnpm-lock.yaml`, falling back to the newest store of their OS
- Job `macos` caches no Swift build folder, as restored folders shortened no build and Swift and Xcode tasks hit the Nx cache
- Runs read and write with `NX_SELF_HOSTED_REMOTE_CACHE_ACCESS_TOKEN`, and a pull request from a fork holds no secret and uses its local cache
- Owner's Mac reads the server and token from Doppler `dev` through mise `[env]`, and uses its local cache while `dev` holds no row
- Tasks the owner's Mac runs fill the cache, and CI jobs read them where each hash input matches, job `macos` its Swift and Xcode tasks
- Test targets hash variable `CI`, as Vitest, Playwright, and MSBuild change behavior under it, so CI runs its own tests
- Swift targets hash compiler version and platform, as `swift --version` adds the macOS version of its host, which a runner and a Mac differ in
- Nx sets no remote cache timeout and stops a run on a read answered other than 200 or 404 or left unconnected, and no setting overrides the stop
- `@rasm/nx-distribution` reads the cache server with its token once before each `nx` run's tasks, outside any task
- Reads answered other than 200 or 404, refused, or past option `remoteCacheTimeout` print one warning and empty `NX_SELF_HOSTED_REMOTE_CACHE_SERVER`
- Nx stores nothing and reports no error for a write answered 403 or 409, and fails a task after 6 attempts of any other failed write

## [03]-[DEPLOY]

Workflow `Deploy` triggers and steps:
- CI job `deploy` calls `Deploy` on pushes to `main` after every self-hosted job, failed test jobs included, as fixes apply past a failing test
- CI job `deploy` waits for the self-hosted jobs of its run alone, as `mise bootstrap remote` can restart Docker under a job on that machine
- Jobs `macos` and `windows` can still read the cache server while a deploy replaces its container, which a changed definition alone causes
- Called runs check out the commit CI tested, and scheduled and dispatched runs check out the tip of `main`
- Pushes apply only where `@rasm/infra` is affected between the base and head of job `affected`
- Failed deploys fail their CI run, so `nx-set-shas` keeps the base before it and the next push applies again
- CI job `deploy` passes `secrets: inherit`, as a called job reads environment `deploy` secrets only where its caller passes them
- Deploy jobs wait in one queue (`queue: max`), as a single pending job gives way to a newer one and drops a Doppler request
- CI runs on `main` hold their concurrency group through job `deploy`, so a later push waits for the deploy
- Doppler config `deploy` changes start `Deploy` through a Doppler webhook sending GitHub a `repository_dispatch` event
- Stacks apply from commits on `main` alone, as each `Deploy` trigger applies the program at the tip of `main` over a local apply
- `Deploy` runs daily, applying image tags that moved with no commit
- Job `deploy` runs on a GitHub-hosted runner and reads Doppler `deploy`
- Job `deploy` signs in to Google Cloud as service account `deploy` through workload identity federation, under `id-token: write`
- Job `deploy` accepts each machine's host key on first contact and runs `mise bootstrap remote` on every machine before `nx deploy`
- `mise bootstrap remote` continues past an unreachable machine, and `nx deploy` runs after a failed bootstrap
- `nx deploy` applies the repository stack before machine stacks, and an unreachable machine fails its own stack alone
- Job `deploy` allows 120 minutes, the `jactionlint` limit, as a machine stack allows an Unreal image pull `2h` (`customTimeouts`)
- `Deploy` runs with no Nx remote cache, independent of the cache server it deploys
- `nx deploy @rasm/infra` creates the whole Nx project graph, and a plugin failing graph creation stops each deploy before any stack applies
- Unchanged stack inputs apply nothing
- After the apply, job `deploy` runs `nx dry-run @rasm/infra -c converged`, and `--expect-no-changes` fails it where a second apply changes a resource
- Job `deploy` ends by removing the BuildKit builders its image builds created

## [04]-[RELEASE]

Workflow `Release` credentials and jobs:
- `Release` runs wait in one queue (`queue: max`), as a single pending run gives way to a newer one and drops a release request
- Job `version` runs `nx release` on macOS, as Apple release projects enter only the macOS graph
- Job `version` outputs the release commit and its projects only where `nx release` committed, as each versioned project writes its manifest
- Job `version` reads the projects a release tagged from `git tag --points-at` its commit, each tag `<project>@<version>`
- Job `version` assigns tagged `host:unreal` projects to job `unreal`, as their `attach` target exists only inside their engine image
- Job `version` assigns each other tagged project with an `attach` target by the tasks its `attach` reaches in the task graph
- Tasks of a `host:browser` project send it to job `browser`, as CI runs those tasks in the Playwright image, and the rest go to job `deliver`
- Jobs `deliver`, `browser`, and `unreal` attach the projects of the job `version` output named as the job
- Jobs run only where job `version` lists a project for them, and `deliver` also to publish, so a preview or a release with no tag delivers nothing
- Jobs `deliver`, `browser`, and `unreal` check out the release commit, and a failure after pushed tags re-runs that job alone
- Job `browser` runs in the Playwright image Deploy publishes, named by the pnpm catalog row, so store images render as in CI
- `import-codesign-certs` imports one p12 per call, so the Developer ID import reuses the Apple Development import's keychain where both run
- `pack` targets find each codesign identity by name in the user keychain list, which `import-codesign-certs` sets to its keychain first
- `Release` sets `cache-mode: none`, as files a release restores from a cache reach what it publishes
- Job `version` holds `contents: write` for tags and releases
- Job `deliver` holds `contents: write` for release files, and `id-token: write` for the OIDC tokens of npm trusted publishing and Google Cloud
- Job `deliver` runs `nx release publish` over the tagged projects
- Before `nx release publish`, job `deliver` requests a `chromewebstore` token as `chrome-web-store` and passes it as `CHROME_WEB_STORE_TOKEN`

## [05]-[MAINTENANCE]

Workflow `Maintenance` jobs per runner instance:
- Workflow `Maintenance` runs weekly before the machine update hour
- Job `container` checks out clean and removes every ignored file of its work folder, package store included
- Job `host` keeps ignored files and prunes package stores, the cargo build folder, unused mise tools and Docker objects, and week-old build cache
- Jobs `container` and `host` each take their instance's queue, and a reset or prune never overlaps a job there as a machine timer can

## [06]-[TIMINGS]

Sources of run, job, and task times and cache hits:
- Nx ends each run with its duration, cache hit share, and critical path, in every job log
- `@rasm/nx-distribution` appends one line per `nx` run to `task-cache.jsonl` of Nx workspace data
- Record lines hold command, seconds, local and remote hits and misses by target, slowest misses, and each failed task
- Runs on GitHub add their hit tally to the job summary and print one error annotation per failed task
- Jobs `linux`, `browser`, `macos`, and `windows` upload `task-cache.jsonl` with their reports, holding earlier runs on a self-hosted runner
- Jobs upload `project-graph.json` beside it, and a task machines hash apart differs in an input or in its project's configuration there
- Nx hashes every target of a project into each of its tasks, so a target inferred from an installed tool or a core count splits the cache by machine
- Nx task database `*.db` of workspace data holds start, end, and status of every task in `task_history`, keyed to `task_details` by hash
- `duckdb` reads the record through `read_json` and the task database through `ATTACH '<file>' (TYPE sqlite)`, times in milliseconds
- Batch executors stamp each task with its own start and end, so slowest misses and the critical path of a batch hold task times
- `gh api repos/<owner>/<repository>/actions/runs/<run>/jobs` returns start and end of each job and step, and `created_at` to `started_at` is queue time
- `NX_PERF_LOGGING=true` prints each task's time and graph creation times, and `NX_PROFILE=<file>` writes a trace of a run's tasks
- `vitest bench workspace.bench.ts --reporter=verbose` in `tests/products/developer/nx/distribution` times graph creation and one task per ecosystem
- Task benchmarks time a cache restore, what every run pays, and a run past the cache over kept tool state, the least a change costs

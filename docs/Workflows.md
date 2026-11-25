# [WORKFLOWS]

Reasons and runner behavior of GitHub Actions workflows their files do not state.

## [01]-[PRINCIPLES]

Rules every workflow follows:
- Jobs install the tools and environments their own steps read, as a job waiting on another waits out its installs
- Jobs install a host application only once a task needing it misses the cache, as a cached task reads no host
- Jobs wait for the jobs whose machine or outputs they need alone, as each wait lengthens the chain a run ends with
- Work no other job waits for runs as steps of a job already on its runner instance, as an instance runs one job at a time

## [02]-[CI]

Workflow `CI` queues, bases, and checkouts:
- Pushes to `main` run one at a time from queue `main` (`queue: max`), where no waiting run gives way to a newer push
- Manual runs each take a concurrency group of their own and share runner instances with pushes, whose jobs wait behind theirs there
- Job `shas` runs `nrwl/nx-set-shas` once for every job but `unreal`, as a CI run finishing between jobs moves the base `nx-set-shas` finds
- `nx-set-shas` walks first-parent ancestors of `HEAD~1` (`per-commit`), as its default list can pick a later commit for a re-run of an older run
- `nx-set-shas` stops at the first commit with a successful push run, and each walked commit costs one API request
- `max-commits` (150) covers a streak of failed CI runs on `main`, as each run walks back to the last commit with a successful run
- `nx-set-shas` fails a run past the token's 1000 requests an hour, which seven runs walking to `max-commits` reach
- `nx-set-shas` falls back to the first commit where no successful run is in reach, and every task runs, as default `HEAD~1` checks one commit
- Manual runs skip `nx-set-shas` and take the first commit as base, and every task runs
- Jobs select projects by tag through `nx affected --exclude`, as `nx affected` takes no `--projects`
- `nx affected` compares file names between base and head, and reads lockfiles and root manifests changed between them at base, one request each
- Jobs `macos` and `windows` clone every commit without file contents and fetch the files of their own commit alone (`filter: blob:none`)
- Self-hosted checkouts fetch full history and keep ignored files (`clean: false`), as a kept checkout fetches new commits alone
- CI jobs run Nx tasks on all cores (`NX_PARALLEL`), and `Release` jobs `browser` and `unreal` on half
- Pushes and manual runs read and write the Nx remote cache with secret `NX_SELF_HOSTED_REMOTE_CACHE_ACCESS_TOKEN`
- Pull request runs read with `NX_SELF_HOSTED_REMOTE_CACHE_READ_TOKEN` and store nothing, and fork pull requests hold no secret and use local cache

## [03]-[CI_JOBS]

Jobs of workflow `CI` and the projects each runs:
- Jobs `shas` and `affected` run on instance `graph`, as other jobs wait for them and instance `host` holds a job behind job `linux` of an earlier run
- Jobs `linux` and `macos` wait for job `shas` alone, and every other job for job `affected` too
- Job `affected` lists each job's projects through `nx show projects --affected --withTarget` over that job's targets and exclusions
- Job `affected` lists job `windows` and its Rhino steps over `test,build`, as Linux graphs hold `build` alone of a suite limited to another OS
- `test` targets read `build` outputs, and a listing over `test,build` reaches each suite a change selects through its build
- Job `affected` names the Playwright image Deploy publishes by the `@playwright/test` row of the pnpm catalog
- Runs at a Playwright version with no published image fail to pull it until Deploy on `main` publishes its tag
- Job `deploy` of such a run still builds that tag, as it waits for job `browser` without needing its success, and a re-run of `browser` passes
- Job `linux` runs affected projects tagged `os:linux` or no `os:` on instance `host`, except `host:rhino`, `host:browser`, `host:unreal`
- Job `linux` runs `nx sync:check` and `nx conformance:check`, as CI task runs skip sync and no Nx task runs conformance
- Job `affected` runs neither check, as jobs `browser`, `windows`, `unreal`, and `deploy` wait for it and neither check feeds one
- Job `linux` runs every `codegen` target first, as type-aware lint reads the sources those targets write
- Job `linux` runs `hk check` from base to head before its tasks, which run past a failed check once code generation passed
- Configuration `ci` of job `linux` adds GitHub annotations to `check--knip` findings
- Job `browser` turns batching off (`NX_BATCH_MODE`)
- Jobs `linux` and `browser` remove each report path before their run, as their kept checkout holds earlier runs' reports
- Job `browser` runs `host:browser` projects on instance `container` with Chromium of its Playwright image
- Job `browser` runs Chromium in a job container, as on the host each network Docker adds fails queued requests with `net::ERR_NETWORK_CHANGED`
- Job containers and user `node` of the Playwright image take user `runner`'s uid, as every job of one instance shares its checkout
- Selenium Manager downloads stable Firefox and `geckodriver` for WebDriver suites, and the image holds the libraries Firefox loads
- `pack--playwright` renders in job `browser`, extension UI in its image's Linux `system-ui`
- Job `macos` runs affected `os:darwin` projects except `host:rhino` and `host:unreal`, as no Linux graph holds an Apple project and its runner holds no engine
- Job `macos` runs `nx conformance:check` again, as Apple projects and their targets enter only the macOS graph
- Job `macos` runs `hk check` steps `swiftlint`, `swift-format`, `plutil`, and `xcprojformatter` from base to head, as hk defines them on macOS alone
- Job `macos` runs `hk check` through `git` (`HK_LIBGIT2`), as libgit2 fails on a file a partial clone lacks where `git` lists names without it
- Job `windows` starts only where job `affected` lists an `os:win32` or `host:rhino` project, and runs `test -c headless` over affected `os:win32` projects
- Job `windows` exports as `RHINO_INSTALLER` the installer URL McNeel's download redirect names for the pinned Rhino major, which every `host:rhino` task hashes
- Job `windows` first runs `test -c headless` over affected `host:rhino` projects without Rhino, which passes where every task hits the cache
- Job `windows` installs that installer where that run fails, from a cache miss or a failing test, then runs those projects again with core-hour token `RHINO_TOKEN`
- Job `unreal` runs the projects `affected` lists in Epic's `dev-slim` image of each engine a tracked `.uproject` names
- Unreal projects hold their targets on every machine, and a task run without the engine its `.uproject` associates refuses, naming where it looked
- Unreal image pulls sign in to GHCR as the repository owner, the account `EPIC_GHCR_TOKEN` belongs to

## [04]-[TOOLS]

Composite actions and the mise profile jobs load:
- Workflows name composite actions and reusable workflows by `$/` paths, read from the archive of the commit a run started from
- Runners fail a job before its first step where a tracked symlink of that archive names an untracked file
- Action `tools` removes untracked files and runs `mise bootstrap --only tools`, and mise hook `postinstall` installs project dependencies
- Action `tools` points `TMPDIR` at the runner's per-job temp folder
- Action `tools` replaces a kept mise once a newer release is a day old, as an older mise refuses a newer lock format
- Job containers install mise tools in the runner's tool cache
- CMake checks uninitialized variables in each file inside the source tree, vcpkg's toolchain file included where mise installs it there
- Windows jobs install mise tools under `C:\mise`, as `cmd.exe` finds no command once `PATH` passes 8191 characters
- GitHub-hosted runners alone load mise profile `ci`, which skips Python environments and Playwright browsers
- Profile `ci` sets `enable_tools` to the tools hosted jobs' steps, Nx tasks, and graph creation run
- Profile `ci` states no `[tools]` row, as mise installs a row stated there only from a `mise.ci.toml` lock the repository lacks
- Self-hosted jobs, job containers and job `affected` included, load no profile and install every tool

## [05]-[CACHE]

Actions caches per workflow and machine:
- Self-hosted Linux jobs keep `.cache/nx/workspace-data` between runs
- Jobs `macos`, `windows`, and `deploy` restore the newest `.cache/nx/workspace-data` saved on their OS, by key prefix `nx-<OS>-`
- Nx plugins check each entry of their `*.hash` file against its source files and recompute a stale one
- Graph caches leave out `cache`, the task database, and the `xcode-packages` clones
- Graph cache paths name `workspace-data/*`, as an excluded folder inside a named folder still enters its archive
- Graph caches save after the run passes or fails, keyed by the hash of every `*.hash` file, as those change only with plugin entries
- Graph cache saves skip a hash the restored key ends with
- Graph caches save from runs on `main` and tool caches from pushes to `main` alone, as a pull request run's cache serves re-runs of that pull request alone
- `jdx/mise-action` saves hosted jobs' tools by platform, profile, bootstrap arguments, `mise.lock`, `mise.ci.toml`, and `rust-toolchain.toml`
- Tool cache keys leave out `mise.toml` and the OS and machine mise files, as `mise.lock` pins every tool and those files change with hooks and rows no tool reads
- GitHub-hosted jobs hold rustup and cargo homes inside the mise folder that action caches, and a restored cache holds the Rust toolchain
- Jobs on macOS restore the Apple analyzer, its stamp, and mise freshness state by analyzer sources
- GitHub-hosted jobs restore the MSBuild analyzer and mise freshness state by analyzer sources and `mise.lock`
- Analyzer caches save a new entry where the mise file holding the analyzer row changed, as mise rebuilds where restored state names another command
- GitHub-hosted jobs cache no pnpm store, as restoring its archive takes longer than the downloads a warm store saves
- Past the 10 GB repository limit GitHub evicts caches by oldest last access

## [06]-[DEPLOY]

Workflow `Deploy` triggers and steps:
- CI job `deploy` calls `Deploy` on pushes to `main`, failed test jobs included, as fixes apply past a failing test
- Pushes apply only where a change between the base and head of job `shas` reaches an input of `deploy--pulumi` of `@rasm/infra`
- `deploy--pulumi` hashes every file of `@rasm/infra` and of each project it depends on, and an edit to any of them starts Deploy
- Failed deploys fail their CI run, `nx-set-shas` keeps the base before it, and the next push applies again
- CI job `deploy` waits for the self-hosted jobs of its run alone, as `mise bootstrap remote` can restart Docker under a job on that machine
- Jobs `macos` and `windows` run on while a deploy replaces the cache server's container
- CI job `deploy` passes `secrets: inherit`, as a called job reads environment `deploy` secrets only where its caller passes them
- `Deploy` runs wait in a queue (`queue: max`), as a single pending run gives way to a newer one and drops a Doppler request
- Scheduled and dispatched `Deploy` runs wait in queue `main` of pushes, which keeps a deploy from starting beside a push run's jobs on a machine
- Calls from CI job `deploy` wait in queue `deploy`
- Manual CI runs wait for no deploy, and Docker keeps their containers through a daemon restart (`live-restore`) while a `docker` call during one fails
- Doppler config `deploy` changes start `Deploy` through a Doppler webhook sending GitHub a `repository_dispatch` event
- `Deploy` runs daily, applying image tags that moved with no commit
- Called runs check out the commit CI tested on a first attempt, and scheduled and dispatched runs the tip of `main`
- Re-runs check out the tip of `main`, as a re-run keeps its run's commit, and stacks there can be older than `main`
- Deploy checkouts fetch their own commit alone, as `nx deploy` and `nx dry-run` read no earlier commit
- Job `deploy` runs on a GitHub-hosted runner, reads Doppler `deploy`, and runs with no Nx remote cache, independent of the cache server it deploys
- Job `deploy` signs in to Google Cloud as service account `deploy` through workload identity federation, under `id-token: write`
- Job `deploy` accepts each machine's host key on first contact and runs `mise bootstrap remote` on every machine before `nx deploy`
- `mise bootstrap remote` continues past an unreachable machine, and `nx deploy` runs after a failed bootstrap
- `nx deploy` applies the repository stack before machine stacks, and an unreachable machine fails its own stack alone
- `nx deploy @rasm/infra` creates the whole Nx project graph, and a plugin failing graph creation stops each deploy before any stack applies
- Job `deploy` allows 120 minutes, the `jactionlint` limit, as a machine stack allows an Unreal image pull `2h` (`customTimeouts`)
- Unchanged stack inputs apply nothing
- After the apply, job `deploy` runs `nx dry-run @rasm/infra -c converged`, and `--expect-no-changes` fails it where a second apply changes a resource
- Job `deploy` ends by removing the BuildKit builders its image builds created, with their build cache

## [07]-[RELEASE]

Workflow `Release` credentials and jobs:
- `Release` runs wait in one queue (`queue: max`), as a single pending run gives way to a newer one and drops a release request
- Release checkouts fetch full history, as `nx release` reads lockfiles and root manifests at each commit after a tag, one request each in a partial clone
- Job `version` runs `nx release` on macOS, as Apple release projects enter only the macOS graph
- Job `version` outputs the release commit and its projects only where `nx release` committed, as each versioned project writes its manifest
- Job `version` reads the projects a release tagged from `git tag --points-at` its commit, each tag `<project>@<version>`
- Job `version` assigns tagged `host:unreal` projects to job `unreal`, as their `attach` target packs inside their engine image
- Job `version` assigns each other tagged project with an `attach` target by the tasks its `attach` reaches in the task graph
- Tasks of a `host:browser` project send it to job `browser`, as CI runs those tasks in the Playwright image, and the rest go to job `deliver`
- Jobs `deliver`, `browser`, and `unreal` attach the projects of the job `version` output named as the job
- Jobs run only where job `version` lists a project for them, `deliver` also to publish, and a preview or a release with no tag delivers nothing
- Jobs `deliver`, `browser`, and `unreal` check out the release commit keeping ignored files (`clean: false`), and a failure after pushed tags re-runs that job alone
- Job `browser` runs in the Playwright image Deploy publishes, named by the pnpm catalog row, and store images render as in CI
- `import-codesign-certs` imports one p12 per call, and the Developer ID import reuses the Apple Development import's keychain where both run
- `pack` targets find each codesign identity by name in the user keychain list, which `import-codesign-certs` sets to its keychain first
- Apple Development expires 2027-09-13 and Developer ID Application 2027-12-24, and a renewal replaces its p12 rows in Doppler and 1Password
- `Release` sets `cache-mode: none`, as files a release restores from a cache reach what it publishes
- `Release` reads no Nx remote cache, as a pack or build another machine cached reaches what a release publishes the same way
- Job `version` holds `contents: write` for tags and releases
- Job `deliver` holds `contents: write` for release files, and `id-token: write` for the OIDC tokens of npm trusted publishing and Google Cloud
- Job `deliver` runs `nx release publish` without `--projects`, which drops publish task dependencies, and each publish target skips an untagged project
- Before `nx release publish`, job `deliver` requests a `chromewebstore` token as `chrome-web-store` and passes it as `CHROME_WEB_STORE_TOKEN`

## [08]-[MAINTENANCE]

Workflow `Maintenance` and its jobs on instances `host` and `container`:
- Workflow `Maintenance` runs weekly before the machine update hour
- Job `host` keeps ignored files and prunes package stores, Docker objects, and week-old build cache
- Job `host` prunes unused mise tools of its machine, and job `container` those of instance `container`'s tool cache in the Playwright image
- Both prunes pass `--yes`, as `mise prune` refuses with no terminal attached
- Job `container` prunes inside a job container, as tool receipts there name container paths, and a prune from the machine refuses them
- Jobs `host` and `container` take their instance's queue, and a prune never overlaps a job there as a machine timer can
- Job `host` runs `check--cargo-deny-advisories` past the Nx cache, as an advisory can name a crate of an unchanged lock

## [09]-[TIMINGS]

Sources of run, job, and task times and cache hits:
- Nx ends each run with its duration, cache hit share, and critical path, in every job log
- Nx appends a run report to the job summary of each top-level run on GitHub, and `@rasm/nx-distribution` annotates each failed or stopped task
- Nx task database `*.db` of workspace data holds start, end, and status of every task in `task_history`, keyed to `task_details` by hash
- Jobs `linux`, `browser`, `macos`, and `windows` upload the task database with their reports, holding earlier runs on a self-hosted runner
- Jobs upload `project-graph.json` beside it, and a task machines hash apart differs in an input or in its project's configuration there
- `duckdb` reads the task database through `ATTACH '<file>' (TYPE sqlite)`, times in milliseconds
- Batch executors stamp each task with its own start and end, and the critical path of a batch holds task times
- First `nx` commands of a job create the project graph, and their time holds graph creation, longest after a plugin change
- `gh api repos/<owner>/<repository>/actions/runs/<run>/jobs` returns start and end of each job and step, and `created_at` to `started_at` is queue time
- Manual CI runs take input `uncached`, the jobs running every task of their `nx affected` step past the Nx cache over state their tools keep
- `NX_PERF_LOGGING=true` prints each task's time and graph creation times, and `NX_PROFILE=<file>` writes a trace of a run's tasks
- `vitest bench workspace.bench.ts --reporter=verbose` in `tests/products/developer/nx/distribution` times graph creation and one task per ecosystem
- Task benchmarks time a cache restore, what every run pays, and a run past the cache over kept tool state, the least a change costs

# [WORKFLOWS]

Reasons and runner behavior of GitHub Actions workflows their files do not state.

## [01]-[PRINCIPLES]

Rules every workflow follows:
- Jobs run on GitHub-hosted runners, and Unreal Engine jobs alone on derby's runner, where role `Runner` pulls each engine image ahead of them
- Jobs install the tools and environments their own steps read, as a job waiting on another waits out its installs
- Jobs install a host application only once a task needing it misses the cache, as a cached task reads no host
- Jobs wait for the jobs whose outputs they need alone, as each wait lengthens the chain a run ends with
- Work no other job waits for runs as background steps of a job holding its tools, as each job installs its tools on a new machine
- Jobs share one Nx remote cache with the owner's Mac, which pushes and manual runs write and pull request runs read alone
- State sits in the store its lifetime matches: task results in the Nx remote cache, cargo state in bucket `cargo`, graph and tool caches in Actions caches
- Workflows `Deploy` and `Release` queue their runs (`queue: max`), as GitHub replaces a single pending run with a newer one and drops its request
- Settings a measurement decided (runner label, mise environment, cache key, job placement) change in a commit naming a newer measurement
- Pushes changing a file every task hashes, a lock file, or an Nx plugin follow a manual run of `CI` on a branch holding the pushed commits
- Manual runs store every result the push reads and warm no Actions cache or cargo state, as both restore on `main` what `main` saved

## [02]-[CI]

Workflow `CI` concurrency, bases, and checkouts:
- Pull request runs share one concurrency group per ref and cancel the older run, and pushes and manual runs each take a group of their own
- Job `shas` runs `nrwl/nx-set-shas` once for every row and job `unreal`, as a CI run finishing between jobs moves the base `nx-set-shas` finds
- Job `shas` runs on `ubuntu-slim` over a clone without trees (`filter: tree:0`), as its steps read commits and the `HEAD` tree alone
- `nx-set-shas` walks first-parent ancestors of `HEAD~1` (`per-commit`), as its default list can pick a later commit for a re-run of an older run
- `nx-set-shas` stops at the first commit with a successful push run, at one API request per walked commit
- `max-commits` (150) covers a streak of failed runs on `main`, and seven runs walking that far reach the token's 1000 requests an hour
- Manual runs skip `nx-set-shas`, take the first commit as base, and run every task
- `nx-set-shas` falls back to the first commit where no successful run is in reach, in place of default `HEAD~1`, and every task runs
- Jobs select projects by tag through `nx affected --exclude`, as `nx affected` takes no `--projects`
- `nx affected` compares file names between base and head, and reads lockfiles and root manifests changed between them at base
- Rows clone every commit without file contents (`filter: blob:none`), each read at base costing one request
- Fork pull requests hold no secret and run on their local cache

## [03]-[CI_JOBS]

Rows of workflow `CI`, job `unreal`, and the projects each runs:
- Rows share one job body, and matrix fields set runner, mise environment, targets, configuration, exclusions, and checks
- Rows start on every run and end after tool setup where `nx affected` selects no task, as a listing job would add a machine and a graph to each row's wait
- Row `Linux` runs every Linux project but Rust crates and `host:rhino`, `host:browser`, `host:unreal`, and `host:claude-code` projects
- Rows `Rust check` and `Rust test` run Linux projects tagged `technology:rust`, target `check` apart from `test,build`, as both builds share no compile unit
- Plugin `@rasm/nx-cargo` tags each crate `technology:rust` and root project `rasm` none, as crate names share no prefix
- Row `Browser` runs `host:browser` projects over `check,test,build,pack`
- Row `macOS` runs `os:darwin` projects on `xcode-27` under Xcode `latest-stable`, Rhino, Unreal, and Claude Code projects aside
- Row `Windows` runs `test -c headless` over `os:win32` and `host:rhino` projects
- Linux rows delete the Android SDK, system .NET, GHC, and CodeQL in the background, as mise supplies .NET and a row's tools outgrow the free disk
- Rows pull public test images with no Docker Hub sign-in, as Docker Hub applies no rate limit to GitHub-hosted runners
- Jobs leave out `host:claude-code` projects, as no runner holds the engine declarations their programs list
- Hk profile `ci` of workflow env `HK_PROFILE` skips step `oxlint-claude-code`, as type-aware oxlint reports each such program as an invalid tsconfig
- Hk profile `ci` skips step `clang-format`, as CI installs no LLVM, and the Mac's hk run checks those files
- Rows `Linux` and `macOS` run `nx sync:check` and `nx conformance:check`, as CI task runs skip sync, no Nx task runs conformance, and Apple projects enter the macOS graph alone
- Row `Linux` runs every `codegen` target before `nx sync:check`, as type-aware lint reads sources code generation writes
- Rows `Linux` and `macOS` run `nx conformance:check` and `hk check` from base to head as background steps, and tasks run past a failed check once code generation passed
- Configuration `ci` of row `Linux` adds GitHub annotations to `check--knip` findings
- Row `macOS` runs `hk check` steps `swiftlint`, `swift-format`, `plutil`, and `xcprojformatter`, as hk defines them on macOS alone
- Workflow env `HK_LIBGIT2` runs `hk check` through `git`, as libgit2 fails on a file a partial clone lacks where `git` lists names without it
- Row `Windows` runs no graph check, hk step, or background step, as background steps on Windows hit `event.json` locks and a cancel stops every step
- Row `Windows` exports as `RHINO_INSTALLER` the installer URL McNeel's download redirect names for the pinned Rhino major, which every `host:rhino` task hashes
- Row `Windows` runs its projects without Rhino first, which passes where every task hits the cache
- Row `Windows` installs Rhino where the run without it fails, from a cache miss or a failing test, then runs those projects again with core-hour token `RHINO_TOKEN`
- Selenium Manager downloads stable Firefox and `geckodriver` for WebDriver suites, and the runner image holds the libraries Firefox loads
- `pack--playwright` renders on row `Browser`, extension UI in the runner image's Linux `system-ui`
- Job `unreal` runs on derby's runner, labels `self-hosted` and `unreal`, in push and manual runs alone, as a pull request would run its code on the machine
- Job `unreal` runs affected `host:unreal` projects in Epic's `dev-slim` image of each engine a tracked `.uproject` names
- Unreal projects hold their targets on every machine, and a task run without the engine its `.uproject` associates refuses, naming where it looked
- Unreal image pulls sign in to GHCR as the repository owner, the account `EPIC_GHCR_TOKEN` belongs to

## [04]-[TOOLS]

Composite actions and the mise environments jobs load:
- Workflows name composite actions and reusable workflows by `$/` paths, read from the archive of the commit a run started from
- Runners fail a job before its first step where a tracked symlink of the archive names an untracked file
- Action `tools` runs `mise bootstrap --only tools` under the environment input `mise-env` names, and mise hook `postinstall` installs project dependencies
- Action `tools` replaces a restored mise once a newer release is a day old, as an older mise refuses a newer lock format
- Job containers install mise tools in the runner's tool cache, outside the checkout
- CMake checks uninitialized variables in each file inside the source tree, vcpkg's toolchain file included where mise installs it there
- Windows jobs install mise tools under `C:/mise`, as `cmd.exe` finds no command once `PATH` passes 8191 characters
- Hosted Linux rows load mise environment `ci-linux` with a second environment per row, and macOS, Windows, and Unreal jobs load `ci`
- Environment `ci` sets `enable_tools` to the tools its jobs' steps, Nx tasks, and graph creation run, and skips Python environments and Playwright browsers
- Environment `ci-linux` sets `disable_tools` to the tools no Linux job runs, as the full Linux set outgrows the free disk, and a new tool row joins Linux jobs by itself
- Environment `ci-linux` skips the Blender and darkroom environments, as Nx Python targets sync their own environments through `uv run`
- Mise environments replace each other's arrays, so Linux jobs take an environment of their own in place of rows added to `ci`
- Environment `ci-llvm` adds LLVM to row `Linux` under tool alias `llvm`, as `disable_tools` matches the configured name and the base row stays disabled
- Row `Linux` frees disk before its tools install, as LLVM unpacks to about 12 GB, and other rows free it in the background
- Environment `ci-no-uv` disables mise's uv dependency provider on rows whose tasks read no root Python environment
- Environments state a `[tools]` row only with a lock of their own file, as a locked install fails on a row its lock lacks

## [05]-[CACHE]

Actions caches and cargo state of each workflow:
- Rows and Deploy restore the newest graph cache saved on their OS
- Nx plugins check each entry of their `*.hash` file against its source files and recompute a stale one
- Graph caches leave out `cache`, the task database, and the `xcode-packages` clones
- Graph cache paths name `workspace-data/*`, as an excluded folder inside a named folder enters its archive
- Graph caches save whether a run passes or fails, keyed by a hash of every `*.hash` file, as those files change with plugin entries alone
- Graph cache saves skip a hash the restored key ends with, and rows of one OS saving one key log a warning after the first save
- Graph caches save from runs on `main` and `jdx/mise-action` tool caches from pushes to `main` alone, as a pull request's cache serves its own re-runs alone
- Analyzer caches save from every ref, as a composite action's `actions/cache` step takes no save condition
- `jdx/mise-action` saves hosted jobs' tools by platform, mise environment, bootstrap arguments, `mise.lock`, the environment's file, and `rust-toolchain.toml`
- Tool cache keys leave out `mise.toml` and the OS and machine mise files, as `mise.lock` pins every tool and those files change with hooks and rows no tool reads
- GitHub-hosted jobs hold rustup and cargo homes inside the mise folder `jdx/mise-action` caches, and a restored cache holds the Rust toolchain
- Jobs on macOS restore the Apple analyzer, its stamp, and mise freshness state by analyzer sources
- GitHub-hosted jobs restore the MSBuild analyzer and mise freshness state by analyzer sources and `mise.lock`
- Analyzer caches save a new entry where the mise file holding the analyzer row changed, as mise rebuilds where restored state names another command
- GitHub-hosted jobs cache no pnpm store, as restoring its archive takes longer than the downloads a warm store saves
- Past the 10 GB repository limit GitHub evicts caches by oldest last access
- Rust rows keep cargo state in bucket `cargo` through `runs-on/cache`, as a state per row and lock would evict the mise entries every row restores
- Cargo state holds the cargo build folder, Dylint outputs and drivers, and the registry and git database of the mise cargo home
- Cargo state keys name row, architecture, `rust-toolchain.toml`, and `Cargo.lock`, and a changed lock restores the newest state of its toolchain
- Rust rows restore state only where `nx show projects --affected` lists a project for their targets
- Pushes missing the exact state key run Rust tasks past the Nx cache, then save state, as state saved over cache hits lacks the new build

## [06]-[DEPLOY]

Workflow `Deploy` triggers and steps:
- `Deploy` runs on each push to `main` apart from `CI`, as fixes apply past a failing test
- Pushes apply only where a change since the last successful Deploy push reaches an input of `deploy--pulumi` of `@rasm/infra`
- `nx-set-shas` over `deploy.yml` finds that base, and a failed deploy keeps it, so the next push applies again
- `deploy--pulumi` hashes every file of `@rasm/infra` and of each project it depends on, and an edit to any of them starts an apply
- Pushes reaching no input of `deploy--pulumi` end after the graph read, with no machine bootstrap and no apply
- One concurrency group queues push, scheduled, and dispatched runs, as applies of the same stacks must not overlap
- CI runs wait for no deploy, and Docker keeps the Unreal job's container through a daemon restart (`live-restore`) while a `docker` call during one fails
- Doppler config `deploy` changes start `Deploy` through a Doppler webhook sending GitHub a `repository_dispatch` event
- `Deploy` runs daily, applying image tags that moved with no commit
- Push runs check out their commit on a first attempt, and scheduled, dispatched, and re-run runs the tip of `main`
- Re-runs check out the tip of `main`, as a re-run keeps its run's commit, and stacks there can be older than `main`
- Deploy checkouts clone every commit without file contents, as `nx-set-shas` and `nx affected` read history
- Job `deploy` runs on a GitHub-hosted runner, reads Doppler `deploy`, and runs with no Nx remote cache, independent of the cache server it deploys
- Job `deploy` signs in to Google Cloud as service account `deploy` through workload identity federation, under `id-token: write`
- Job `deploy` accepts each machine's host key on first contact and runs `mise bootstrap remote` on every machine before `nx deploy`
- `mise bootstrap remote` continues past an unreachable machine, and `nx deploy` runs after a failed bootstrap
- `nx deploy` applies the repository stack before machine stacks, and an unreachable machine fails its own stack alone
- `nx deploy @rasm/infra` creates the whole Nx project graph, and a plugin failing graph creation stops each deploy before any stack applies
- Job `deploy` allows 120 minutes, the `jactionlint` limit, as a machine stack allows an Unreal image pull `2h` (`customTimeouts`)
- After the apply, job `deploy` runs `nx dry-run @rasm/infra -c converged`, and `--expect-no-changes` fails it where a second apply changes a resource

## [07]-[RELEASE]

Workflow `Release` credentials and jobs:
- Release checkouts fetch full history, as `nx release` reads lockfiles and root manifests at each commit after a tag, one request each in a partial clone
- Job `version` runs `nx release` on macOS, as Apple release projects enter only the macOS graph
- Job `version` outputs the release commit and its projects only where `nx release` committed, as each versioned project writes its manifest
- Job `version` assigns tagged `host:unreal` projects to job `unreal`, as their `attach` target packs inside their engine image on derby's runner
- Job `version` assigns each other tagged project with an `attach` target by the tasks its `attach` reaches in the task graph
- Tasks of a `host:browser` project send it to job `browser`, as its renders take the Linux runner image of CI row `Browser`, and the rest go to job `deliver`
- Jobs run only where job `version` lists a project for them or, for `deliver`, where the run publishes, and a preview or a release with no tag delivers nothing
- Jobs `deliver`, `browser`, and `unreal` check out the release commit, and a failure after pushed tags re-runs that job alone
- `import-codesign-certs` imports one p12 per call, and the Developer ID import reuses the Apple Development import's keychain where both run
- `pack` targets find each codesign identity by name in the user keychain list, which `import-codesign-certs` sets to its keychain first
- Apple Development expires 2027-09-13 and Developer ID Application 2027-12-24, and a renewal replaces its p12 rows in Doppler and 1Password
- `Release` sets `cache-mode: none` and reads no Nx remote cache, as a file restored from a cache or a pack another machine cached reaches what it publishes
- Job `version` holds `contents: write` for tags and releases, and job `deliver` for release files
- Job `deliver` holds `id-token: write` for the OIDC tokens of trusted publishing on npm, PyPI, NuGet, and crates.io, and of Google Cloud
- Job `version` lists the publish technologies of tagged projects, and job `deliver` signs in to NuGet and crates.io only where one publishes there
- `NuGet/login` and `rust-lang/crates-io-auth-action` trade the OIDC token for the `NUGET_API_KEY` and `CARGO_REGISTRY_TOKEN` publish targets read
- Job `deliver` runs `nx release publish` without `--projects`, which drops publish task dependencies, and each publish target skips an untagged project

## [08]-[MAINTENANCE]

Workflow `Maintenance` and its job:
- Job `advisories` runs `check--cargo-deny-advisories` weekly past the Nx cache, as an advisory can name a crate of an unchanged lock

## [09]-[TIMINGS]

Sources of run, job, and task times and cache hits:
- Nx ends each run with its duration, cache hit share, and critical path, in every job log
- Nx appends a run report to the job summary of each top-level run on GitHub, and `@rasm/nx-distribution` annotates each failed or stopped task
- Nx task database `*.db` of workspace data holds start, end, and status of every task in `task_history`, keyed to `task_details` by hash
- Rows upload the task database of their run with their reports as artifact `<row>-reports-<attempt>`
- Rows upload `project-graph.json` beside it, and a task machines hash apart differs in an input or in its project's configuration there
- Row `macOS` uploads each `test--xcodebuild` result bundle, holding attachments and window captures of failing tests
- `duckdb` reads the task database through `ATTACH '<file>' (TYPE sqlite)`, times in milliseconds
- Batch executors stamp each task with its own start and end, and the critical path of a batch holds task times
- First `nx` commands of a job create the project graph, and their time holds graph creation, longest after a plugin change
- `gh api repos/<owner>/<repository>/actions/runs/<run>/jobs` returns start and end of each job and step, a job's `created_at` to `started_at` its runner pickup
- Runs wait in a concurrency queue from their own `created_at` to the earliest `created_at` of their jobs
- Manual runs of `CI` take input `uncached`, which runs every task of each row's `nx affected` step past the Nx cache
- `NX_PERF_LOGGING=true` prints each task's time and graph creation times, and `NX_PROFILE=<file>` writes a trace of a run's tasks
- `vitest bench workspace.bench.ts --reporter=verbose` in `tests/products/developer/nx/distribution` times graph creation and one task per ecosystem
- Task benchmarks time a cache restore, what every run pays, and a run past the cache over kept tool state, the least a change costs

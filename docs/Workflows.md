# [WORKFLOWS]

Reasons and runner behavior of GitHub Actions workflows their files do not state.

## [01]-[CI]

Workflow `CI` and its jobs:
- Jobs select projects by tag through `nx affected --exclude`, as `nx affected` takes no `--projects`
- Job `affected` runs `nx sync:check` and `nx conformance:check`, as CI task runs skip sync and no Nx task runs conformance
- Job `affected` names the Playwright image Deploy publishes by installed `@playwright/test` version
- Job `affected` runs `nrwl/nx-set-shas` once for every job but `unreal`, as a CI run finishing between jobs moves the base `nx-set-shas` finds
- `nx-set-shas` falls back to the first commit where no successful run is in reach, and every task runs, as default `HEAD~1` checks one commit
- Job `linux` runs affected projects tagged `os:linux` or no `os:` on instance `host`, except `host:rhino`, `host:browser`, `host:unreal`
- Configuration `ci` of job `linux` adds GitHub annotations to `check--knip` findings
- Job `browser` runs `host:browser` projects on instance `container` with Chromium of its Playwright image
- Selenium Manager downloads stable Firefox and `geckodriver` for WebDriver suites, and the image holds the libraries Firefox loads
- `pack--playwright` renders in job `browser`, extension UI in its image's Linux `system-ui`, and `Release` reads its images from remote cache
- Runs at a Playwright version with no published image fail to pull it until Deploy on `main` publishes its tag
- Job `browser` runs Chromium in a job container, as on the host each network Docker adds fails queued requests with `net::ERR_NETWORK_CHANGED`
- Job containers and user `node` of the Playwright image take user `runner`'s uid, as every job of one instance shares its checkout
- Jobs `linux` and `browser` remove each report path before their run, as their kept checkout holds earlier runs' reports
- Job `macos` runs affected `os:darwin` projects except `host:rhino`, as no Linux graph holds an Apple project
- Job `macos` leaves out root `rasm`, as job `linux` runs its other checks and each Swift project's tasks depend on its Swift targets
- Job `macos` runs `hk check` steps `swiftlint`, `swift-format`, `plutil`, and `xcprojformatter` from base to head, as they need macOS
- Job `files` runs every other `hk check` step on instance `host`, which keeps the Nx graph between runs
- Job `files` runs every `codegen` target first, as type-aware lint reads the sources those targets write
- Job `windows` starts only where job `affected` lists an `os:win32` or `host:rhino` project, and installs Rhino where one is `host:rhino`
- Job `windows` runs `test -c headless` over affected `os:win32` and `host:rhino` projects with core-hour token `RHINO_TOKEN`
- Job `unreal` runs the projects `affected` lists in Epic's `dev-slim` image of each engine a tracked `.uproject` names
- Unreal projects hold no target on a runner outside their engine image
- Job containers install mise tools in the runner's tool cache and keep their pnpm store in checkout `.cache`
- Windows jobs install mise tools under `C:\mise`, as `cmd.exe` finds no command once `PATH` passes 8191 characters
- CMake checks uninitialized variables in each file inside the source tree, vcpkg's toolchain file included where mise installs it there
- CI, Deploy, and Release checkouts fetch full history, as `nx affected` and `nx release` compare commits a partial clone fetches one at a time
- CI and Release checkouts on self-hosted runners keep ignored files (`clean: false`)
- Self-hosted jobs run tasks on half their cores (`NX_PARALLEL`), as instances `host` and `container` share one machine
- Action `tools` removes untracked files and bootstraps mise tools alone, and hook `post-tools` installs project dependencies
- GitHub-hosted runners and job containers load mise profile `ci`, installing pnpm packages alone, as Python tasks run on instance `host`
- Profile `ci-<job id>` sets `enable_tools` to the tools a job's steps and Nx tasks run, and a job with no such file installs every tool
- Profiles list the tools graph creation spawns: `cargo`, `dotnet`, `uv`, `jscpd`, and CMake with LLVM, Ninja, and vcpkg
- Profiles repeat those tools, as a later mise file replaces `enable_tools` whole
- Profile `ci-macos` holds `yq`, as `mise.macos.toml` reads the Rhino major through it
- Workflows name composite actions as `$/.github/actions/<name>`, read from the archive of the commit a run started from
- Runners fail a job before its first step where a tracked symlink of that archive names an untracked file

## [02]-[CACHE]

Nx remote cache and Actions caches per workflow and machine:
- Self-hosted Linux jobs keep `.cache/nx/workspace-data` between runs
- Jobs `macos`, `windows`, and `deploy` restore `.cache/nx/workspace-data` without `cache` and task database by lock, `nx.json`, and plugins
- Graph cache paths name `workspace-data/*`, as an excluded folder inside a named folder still enters its archive
- Jobs `macos` and `windows` save a missed `workspace-data` after `nx affected` passes or fails
- Action `jdx/mise-action` saves a GitHub-hosted job's tools after install, keyed by platform, profile, `mise.toml`, `mise.lock`, and profile files
- Tool cache keys leave out OS and machine mise files, as those state environment and bootstrap rows alone
- GitHub-hosted jobs cache no pnpm store, as `pnpm store prune` on APFS removes every package of a store pnpm clones from
- Pushes to `main` read and write with `NX_CACHE_WRITE_TOKEN`, and other runs read with `NX_SELF_HOSTED_REMOTE_CACHE_ACCESS_TOKEN`
- Owner's Mac reads the server and read-only token from Doppler `dev` through mise `[env]`, and uses its local cache while `dev` holds no row
- Nx sets no remote cache timeout and stops a run on a read answered other than 200 or 404 or left unconnected, and no setting overrides the stop
- `@rasm/nx-distribution` reads the cache server with its token once before each `nx` run's tasks, outside any task
- Reads answered other than 200 or 404, refused, or past option `remoteCacheTimeout` print one warning and empty `NX_SELF_HOSTED_REMOTE_CACHE_SERVER`
- Nx stores nothing and reports no error for a write answered 403 or 409, and fails a task after 6 attempts of any other failed write

## [03]-[DEPLOY]

Workflow `Deploy` triggers and steps:
- Workflow `Deploy` runs `nx deploy @rasm/infra` when a CI run on `main` ends, failed runs included, and fixes apply past a failing test
- `Deploy` checks out and applies the commit its CI run tested, as CI runs on `main` end in push order and a newer push replaces a waiting run
- Cancelled CI runs start no deploy
- `Deploy` names each run by the commit it applies
- Runs a CI run starts apply only where `@rasm/infra` is affected since the commit of the last successful run, else the first commit
- Doppler config `deploy` changes start `Deploy` through a Doppler webhook sending GitHub a `repository_dispatch` event
- `Deploy` runs daily, applying image tags that moved with no commit
- Job `deploy` runs on a GitHub-hosted runner and reads Doppler `deploy`
- Job `deploy` signs in to Google Cloud as service account `deploy` through workload identity federation, under `id-token: write`
- Job `deploy` accepts each machine's host key on first contact and runs `mise bootstrap remote` on every machine before `nx deploy`
- `Deploy` runs with no Nx remote cache, independent of the cache server it deploys
- `Deploy` sets `cache-mode: write`, as `workflow_run` runs otherwise restore caches alone, and its cache keys name its own profile and runner
- `nx deploy @rasm/infra` creates the whole Nx project graph, and a plugin failing graph creation stops each deploy before any stack applies
- Unchanged stack inputs apply nothing
- After the apply, job `deploy` runs `nx dry-run @rasm/infra -c converged`, and `--expect-no-changes` fails it where a second apply changes a resource
- Job `deploy` ends by removing the BuildKit builders its image builds created

## [04]-[RELEASE]

Workflow `Release` credentials and jobs:
- Job `release` imports codesign identities into a temporary keychain only where their secrets are set
- Job `release` holds `contents: write` for tags and releases, and `id-token: write` for the OIDC tokens of npm trusted publishing and Google Cloud
- Before `nx release publish`, job `release` requests a `chromewebstore` token as `chrome-web-store` and passes it as `CHROME_WEB_STORE_TOKEN`
- Job `unreal` attaches `host:unreal` projects from each engine image at the released commit, and job `release` attaches every other project

## [05]-[MAINTENANCE]

Workflow `Maintenance` jobs per runner instance:
- Workflow `Maintenance` runs weekly before the machine update hour
- Job `container` checks out clean and removes every ignored file of its work folder, package store included
- Job `host` keeps ignored files and prunes package stores, the cargo build folder, unused mise tools and Docker objects, and week-old build cache
- Jobs `container` and `host` each take their instance's queue, and a reset or prune never overlaps a job there as a machine timer can

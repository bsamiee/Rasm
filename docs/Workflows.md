# [WORKFLOWS]

GitHub Actions workflows and the jobs each one runs.

## [01]-[CI]

Workflow `CI` and its jobs:
- Workflow `CI` runs on pull requests, pushes to `main`, and dispatch
- Newer pushes to a pull request cancel its running CI
- Jobs run `check`, `test`, and `build` over affected projects, and job `browser` adds `pack`
- Jobs select projects by tag through `nx affected --exclude`, as `nx affected` takes no `--projects`
- Job `affected` runs `nx sync:check`, as CI task runs skip sync
- Job `affected` runs `nx conformance:check`, as no Nx task runs it
- Job `affected` lists affected projects of tags `host:unreal` and `host:browser`, and the engines every tracked `.uproject` names
- Job `affected` names the Playwright image Deploy publishes by installed `@playwright/test` version
- Unreal projects hold no target on a runner outside their engine image
- Job `affected` runs `nrwl/nx-set-shas` once for every job but `unreal`, as a CI run finishing between jobs moves the base `nx-set-shas` finds
- `nx-set-shas` falls back to the first commit where no successful run is in reach, and every task runs, as default `HEAD~1` checks one commit
- Job `linux` runs affected projects tagged `os:linux`, `os:all`, or no `os:` on instance `host`, except `host:rhino`, `host:browser`, `host:unreal`
- Configuration `ci` of job `linux` adds GitHub annotations to `check--knip` findings
- Job `browser` pulls the Playwright image by `github.token` under `packages: read` and runs `host:browser` projects in it on instance `container`
- Job `browser` launches Chromium from its image and installs no browser
- `pack--playwright` renders in job `browser`, extension UI in its image's Linux `system-ui`, and `Release` reads its images from remote cache
- Runs at a Playwright version with no published image fail to pull it until Deploy on `main` publishes its tag
- Job `browser` runs Chromium in a job container, as on the host each network Docker adds fails queued requests with `net::ERR_NETWORK_CHANGED`
- Job containers and user `node` of the Playwright image take user `runner`'s uid, as every job of one instance shares its checkout
- Jobs upload CMake configure logs, .NET test results, and Playwright and Vitest reports as artifact `<job>-reports`
- Jobs `linux` and `browser` remove each uploaded path first, as the kept checkout holds earlier runs' reports
- Job `macos` runs affected `os:darwin` and `os:all` projects except `host:rhino` on GitHub-hosted macOS, as no Linux graph holds an Apple project
- Job `files` runs `hk check` over changes from base to head on macOS, as `swiftlint`, `swift format`, `plutil`, and `xcprojformatter` run there alone
- Job `windows` runs `test -c headless` over affected `os:win32` and `host:rhino` projects on GitHub-hosted Windows with core-hour token `RHINO_TOKEN`
- Job `windows` installs Rhino through `mcneel/setup-rhino3d` where an affected project holds tag `host:rhino`
- Job `unreal` runs the projects `affected` lists in Epic's `dev-slim` image of each engine on instance `container`, where both lists hold one
- Job containers install mise tools in the runner's tool cache and keep the pnpm store in checkout `.cache`
- CMake checks uninitialized variables in each file inside the source tree, vcpkg's toolchain file included where mise installs it there
- CI and Release checkouts fetch full history, as `nx affected` and `nx release` compare commits a partial clone fetches one at a time
- CI and Release checkouts on self-hosted runners keep ignored files (`clean: false`)
- Action `tools` removes untracked files and runs `mise --locked bootstrap --only tools`, and hook `post-tools` installs pnpm and analyzer packages
- Workflows name repository actions as `$/<path>`, and runner 2.336.0 or later resolves each path at the workflow's commit

## [02]-[CACHE]

Nx remote cache per workflow and machine:
- `nx.json` sets `neverConnectToCloud`, and role `Cache` serves the remote cache
- Self-hosted Linux jobs keep `.cache/nx/workspace-data` between runs
- Job `macos` restores `.cache/nx/workspace-data` without folder `cache` and task database by lock, `nx.json`, and plugin sources
- Job `macos` saves a missed `workspace-data` after `nx affected` passes or fails
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
- `Deploy` applies only when its CI run tested the tip of `main`, and services stay up under a later push's CI run
- Cancelled CI runs start no deploy, and each deploy checks out the tip of `main` its event names
- Doppler config `deploy` changes start `Deploy` through a Doppler webhook sending GitHub a `repository_dispatch` event
- Job `deploy` runs on a GitHub-hosted runner and reads Doppler `deploy`
- Job `deploy` signs in to Google Cloud as service account `deploy` through workload identity federation, under `id-token: write`
- Job `deploy` accepts each machine's host key on first contact
- Job `deploy` runs `mise bootstrap remote` on every machine before `nx deploy`
- `Deploy` runs with no remote cache and `cache-mode: none`, independent of the cache server it deploys and of caches other runs wrote
- `nx deploy @rasm/infra` creates the whole Nx project graph, and a plugin failing graph creation stops each deploy before any stack applies
- Unchanged stack inputs apply nothing
- After the apply, job `deploy` runs `nx dry-run @rasm/infra -c converged`, and `--expect-no-changes` fails it where a second apply changes a resource
- Job `deploy` ends by removing the BuildKit builders its image builds created

## [04]-[RELEASE]

Workflow `Release` inputs and job steps:
- Workflow `Release` runs by dispatch with inputs `specifier` and `projects` of `nx release`, `dry-run` (default on), and `publish`
- Job `release` runs on a GitHub-hosted macOS runner, attaches with `dry-run` off, and publishes with `dry-run` off and input `publish`
- Job `release` imports codesign identities into a temporary keychain only where their secrets are set
- Job `release` holds `contents: write` for tags and releases, and `id-token: write` for the OIDC tokens of npm trusted publishing and Google Cloud
- Before `nx release publish`, job `release` requests a `chromewebstore` token as `chrome-web-store` and passes it as `CHROME_WEB_STORE_TOKEN`
- Job `unreal` attaches `host:unreal` projects from each engine image at the released commit, and job `release` attaches every other project

## [05]-[MAINTENANCE]

Workflow `Maintenance` jobs per runner instance:
- Workflow `Maintenance` runs weekly before the machine update hour, on both runner instances, and by dispatch
- Job `container` checks out clean and removes every ignored file of its work folder, the package store included
- Job `container` removes container tools from the runner's tool cache
- Job `host` keeps ignored files, installs tools and packages, then prunes the pnpm store, uv cache, and unused mise tool versions
- Job `host` removes stopped Docker containers, unused anonymous volumes, and dangling images
- Job `host` removes old unused images labelled with the repository as source
- Jobs `container` and `host` each take their instance's queue, and a reset or prune never overlaps a job there as a machine timer can

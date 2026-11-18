# [WORKFLOWS]

## [01]-[CI]

- Workflow `CI` runs on pull requests, pushes to `main`, and dispatch
- Newer pushes to a pull request cancel its running CI
- Jobs run `nx affected` over `check`, `test`, and `build`
- Job `affected` runs `nx sync:check`, as CI task runs skip sync
- Job `affected` runs `nx conformance:check`, as no Nx task runs the `nx.json` `conformance` rules
- Job `affected` lists affected projects of technology `unreal` and the engines every tracked `.uproject` names
- Unreal projects hold no target on a runner outside their engine image
- Job `affected` runs `nrwl/nx-set-shas` once and passes base and head to jobs `linux` and `macos`, as a CI run finishing between jobs moves the base `nx-set-shas` finds
- `nx-set-shas` takes the first commit as base where the latest 100 commits hold no successful run, so every task runs, as its own fallback `HEAD~1` checks one commit alone
- Job `linux` runs the affected projects tagged `os:linux`, `os:all`, or no `os:` tag on runner instance `host`
- Job `linux` removes earlier runs' reports first, as the kept checkout holds them
- Job `linux` starts daemon `playwright` before `nx affected` and stops it after, as the runner leaves a job's container holding its port
- Job `macos` runs on GitHub-hosted runner `xcode-27` and selects `os:darwin` and `os:all` projects itself, as no Linux graph holds an Apple project
- Job `windows` tests affected `os:win32` projects on GitHub-hosted Windows Server, under core-hour billing token `RHINO_TOKEN`
- Job `windows` installs the newest Rhino of RhinoCommon's major through `mcneel/setup-rhino3d` where its graph lists an affected `os:win32` project
- Job `windows` installs only the mise tools its steps and its Nx graph call, through `MISE_ENABLE_TOOLS`
- Job `unreal` runs the projects `affected` lists in Epic's `dev-slim` image of each engine on instance `container`, where both lists hold one
- Job containers install only the mise rows graph creation and Unreal targets read, under `.cache` of the checkout
- CI and Release checkouts fetch full history, as `nx affected` and `nx release` compare commits a partial clone fetches one at a time
- CI and Release checkouts on self-hosted runners keep ignored files (`clean: false`)
- Action `tools` removes untracked files, then `jdx/mise-action` runs `mise --locked bootstrap --only tools` on finding `.config/mise.lock`, and its `post-tools` hook installs pnpm packages

## [02]-[CACHE]

- `nx.json` sets `neverConnectToCloud`, and the remote cache is the server of role `Cache`
- `NX_SKIP_REMOTE_CACHE` misses `nx release publish`, where an empty server variable and `--skip-remote-cache` reach every command
- Workflow `Release` runs with an empty server variable on its local cache alone, as Nx advises for released artifacts
- Self-hosted Linux jobs keep `.cache/nx/workspace-data` between runs
- GitHub-hosted macOS jobs restore the pnpm store by `pnpm-lock.yaml`, and `.cache/nx/workspace-data` without folder `cache` and task database by lock, `nx.json`, and plugin sources
- GitHub-hosted macOS jobs save a missed store after install and `pnpm store prune`, and a missed `workspace-data` after `nx affected` passes or fails
- Pushes to `main` read and write with the write token, and other runs read with the read-only token, as Nx documents for protected branches
- This Mac reads the server and read-only token from Doppler `dev` through mise `[env]`, and uses its local cache while `dev` holds no row
- `@rasm/nx-distribution` reads the server with its token once before the tasks of each `nx` run outside a task
- Reads answered other than 200 or 404, refused, or unanswered within `nx.json` option `remoteCacheTimeout` print one warning and empty `NX_SELF_HOSTED_REMOTE_CACHE_SERVER` for that run
- Nx builds no remote cache where `NX_SELF_HOSTED_REMOTE_CACHE_SERVER` is empty
- Nx sets no timeout on a remote cache request
- Nx stops a run on a read answered other than 200 or 404 or left unconnected, and no Nx setting continues past the error
- Nx stores nothing and reports no error for a write answered 403 or 409
- Nx fails a task after 6 attempts of any other failed write

## [03]-[DEPLOY]

- Workflow `Deploy` runs `nx deploy @rasm/infra` when a CI run on `main` ends, passed or failed, so infrastructure fixes apply past a failing test
- `Deploy` applies only when that CI run tested the tip of `main`, keeping services up under the CI run of a later push
- Cancelled CI runs start no deploy, and each deploy checks out `main`
- Doppler config `deploy` changes start `Deploy` through a Doppler webhook sending GitHub a `repository_dispatch` event
- Job `deploy` runs on GitHub-hosted `ubuntu-26.04` and reads Doppler `deploy`
- Job `deploy` accepts each machine's host key on first contact
- Job `deploy` runs `mise bootstrap remote --all` before `nx deploy`, first with `--dry-run` to print each machine's drift, then with `--yes`
- `Deploy` runs with no remote cache and `cache-mode: none`, independent of the cache server it deploys and of caches other runs wrote
- `nx deploy @rasm/infra` creates the whole Nx project graph, and a plugin failing graph creation stops the deploy before any stack applies
- Unchanged stack inputs apply nothing

## [04]-[RELEASE]

- Workflow `Release` runs by dispatch with inputs `specifier`, `projects`, `dry-run` (default on), `publish`
- Empty `specifier` reads conventional commits after each project's last tag, a version moves the `projects` input names, as alpha to stable
- Job `release` runs on `xcode-27`, attaches with `dry-run` off, and publishes with `dry-run` off and input `publish`
- Job `release` imports codesign identities into a temporary keychain only where their secrets are set
- Job `unreal` then attaches the projects of technology `unreal` from each engine image at the released commit

## [05]-[MAINTENANCE]

- Workflow `Maintenance` runs each Sunday at 09:00 UTC, before the machine update hour, on both runner instances and by dispatch
- Job `container` checks out clean, removing every ignored file of its work folder, the container tools and package store included
- Job `host` keeps ignored files, installs tools and packages, then prunes the pnpm store, uv cache, and unused mise tool versions
- Job `host` removes stopped Docker containers created over an hour before, unused anonymous volumes, and dangling images
- Jobs `container` and `host` each take the queue of their instance, so a reset or prune never overlaps a job there as a machine timer would

# [WORKFLOWS]

## [01]-[CI]

- Workflow `CI` runs on pull requests, pushes to `main`, and dispatch
- Newer pushes to a pull request cancel its running CI
- Jobs run `nx affected` over `check`, `test`, and `build`, as many tasks at once as their machine has cores (mise `[env]` `NX_PARALLEL`)
- Job `affected` runs `nx sync:check`, as CI task runs skip sync, and lists the affected projects of technology `unreal` and the engines `.uproject` files name
- Job `affected` lists projects, as an Unreal project holds no target on a runner outside its engine image
- Job `affected` runs `nrwl/nx-set-shas` once and passes base and head to jobs `linux` and `macos`, so each compares the same commits while a CI run finishing between jobs moves the base `nx-set-shas` finds
- Job `linux` runs the affected projects tagged `os:linux`, `os:all`, or no `os:` tag on runner instance `host`
- Job `linux` removes earlier runs' reports first, as the kept checkout holds them
- Job `macos` runs on GitHub-hosted runner `xcode-27` and selects `os:darwin` and `os:all` projects itself, as no Linux graph holds an Apple project
- Job `unreal` runs the projects `affected` lists in Epic's `dev-slim` image of each engine on instance `container`, where both lists hold one
- Job containers install only the mise rows graph creation and Unreal targets read, under `.cache` of the checkout
- CI and Release checkouts fetch full history, as `nx affected` and `nx release` compare commits a partial clone fetches one at a time
- CI and Release checkouts on self-hosted runners keep ignored files (`clean: false`)
- Action `tools` removes untracked files, then `mise bootstrap` installs the mise tools, checks the remote cache, and installs pnpm packages

## [02]-[CACHE]

- `nx.json` sets `neverConnectToCloud`, and the remote cache is the server of role `Cache`
- `NX_SKIP_REMOTE_CACHE` misses `nx release publish`, where an empty server variable and `--skip-remote-cache` reach every command
- Workflow `Release` runs with an empty server variable on its local cache alone, as Nx advises for released artifacts
- Self-hosted Linux jobs keep `.cache/nx/workspace-data` between runs
- GitHub-hosted macOS jobs restore the pnpm store by `pnpm-lock.yaml`, and `.cache/nx/workspace-data` without folder `cache` and task database by lock, `nx.json`, and plugin sources
- GitHub-hosted macOS jobs save a missed store after install and `pnpm store prune`, and a missed `workspace-data` after `nx affected` passes or fails
- Pushes to `main` read and write with the write token, and other runs read with the read-only token, as Nx documents for protected branches
- This Mac reads the server and read-only token from Doppler `dev` through mise `[env]`, and uses its local cache while `dev` holds no row
- `.config/mise.toml` row `NX_SELF_HOSTED_REMOTE_CACHE_SERVER` reads the server with its token, once per 15 minutes, and `jdx/mise-action` exports the result to every later step
- Reads answered other than 200 or 404, or unanswered in 5 seconds, print one warning and empty `NX_SELF_HOSTED_REMOTE_CACHE_SERVER`
- Nx builds no remote cache where `NX_SELF_HOSTED_REMOTE_CACHE_SERVER` is empty
- Nx sets no timeout on a remote cache request
- Nx stops a run on a read answered other than 200 or 404 or left unconnected, and no Nx setting continues past the error
- Nx counts a write answered 403 as stored
- Nx fails a task after 6 attempts of any other failed write

## [03]-[DEPLOY]

- Workflow `Deploy` runs `nx deploy @rasm/infra` when a CI run on `main` ends, passed or failed, so infrastructure fixes apply past a failing test
- `Deploy` applies only when that CI run tested the tip of `main`, keeping services up under the CI run of a later push
- Cancelled CI runs start no deploy, and each deploy checks out `main`
- Doppler config `deploy` changes start `Deploy` through a Doppler webhook sending GitHub a `repository_dispatch` event
- Job `deploy` runs on GitHub-hosted `ubuntu-26.04` and reads Doppler `deploy`
- Job `deploy` accepts each machine's host key on first contact
- `Deploy` runs with no remote cache and `cache-mode: none`, independent of the cache server it deploys and of caches other runs wrote
- `nx deploy @rasm/infra` creates the whole Nx project graph, and a plugin failing graph creation stops the deploy before any stack applies
- Unchanged stack inputs apply nothing

## [04]-[RELEASE]

- Workflow `Release` runs by dispatch with inputs `specifier` (empty reads conventional commits), `dry-run` (default on), `publish`
- First releases state `specifier`, as Nx refuses a commit in the history it reads whose scope matches more than one release project
- Job `release` runs on `xcode-27`, attaches with `dry-run` off, and publishes with `dry-run` off and input `publish`
- Job `release` imports codesign identities into a temporary keychain only where their secrets are set
- Job `unreal` then attaches the projects of technology `unreal` from each engine image at the released commit

## [05]-[MAINTENANCE]

- Workflow `Maintenance` runs each Sunday at 09:00 UTC, before the machine update hour, on both runner instances and by dispatch
- Job `container` checks out clean, removing every ignored file of its work folder, the container tools and package store included
- Job `host` checks out clean, installs tools and packages, then prunes the pnpm store, uv cache, mise tool versions, and Docker leftovers

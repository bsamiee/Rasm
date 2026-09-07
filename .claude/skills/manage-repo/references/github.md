# [GITHUB]

GitHub Actions runs the repository events and the hosted jobs, Nx selects the projects and orders the tasks, and act runs the Linux jobs on the machine.

## [01]-[WORKFLOWS]

Each workflow declares the event that requires its work, reuses jobs through `workflow_call` and steps through composite actions, and holds commands alone, because the targets and scripts hold the build logic:

| [INDEX] | [WORKFLOW]    | [TRIGGER]                         | [DEPENDENCY]                    |
| :-----: | :------------ | :-------------------------------- | :------------------------------ |
|  [01]   | `ci.yml`      | Pull request, main push, dispatch | .NET consumes the native feed   |
|  [02]   | `native.yml`  | `workflow_call`                   | Matrix stage precedes pack      |
|  [03]   | `release.yml` | Dispatch with project filter      | Native feed precedes publishing |

`needs` names the job with the outputs a job consumes, `strategy.matrix.include` lists the runner and runtime-identifier pairs, and `fail-fast: false` keeps every platform's result. Python and TypeScript run without the native feed.

Steps run in sequence, and the step concurrency GitHub added in June 2026 (`background: true`, `wait`, `wait-all`, `cancel`, and `parallel` as a group with an implicit wait, at most 10 background steps per job, none inside a composite action) stays out of the workflows, because actionlint 1.7.12 reports each key as unexpected and act 0.2.89 fails the schema validation, and Nx runs the concurrent project work.

`defaults.run.shell: bash` covers every workflow step, and a composite action step declares its own `shell`, because the workflow defaults do not reach it. Expression values reach a script through the step's `env` map, read as a shell variable, because GitHub substitutes `${{ }}` into the script text before the shell parses it, and actionlint reports the untrusted contexts (`github.event.*`) alone.

Concurrency groups name the shared operation, the group name is case-insensitive, and `queue: max` refuses `cancel-in-progress: true`:
- `ci.yml` groups by workflow and ref with `cancel-in-progress` on pull requests, and a pending run replaces the previous pending one
- `release.yml` groups by workflow with `queue: max`, up to 100 pending runs processed in the order each began waiting

The token permissions are the ones the jobs call: `contents: read` for the checkout, `actions: read` for the run lookup of `nx-set-shas`, and on the release job `id-token: write` for the OIDC exchange and `contents: write` for the tags and releases `nx release` pushes.

## [02]-[AFFECTED_PROJECTS]

`actions/checkout` runs with `fetch-depth: 0` for the tags and merge bases and `filter: tree:0`, a treeless clone that keeps every commit and fetches trees on demand. `nrwl/nx-set-shas` runs in each job that calls `nx affected`: it reads the last successful `push` run of the current workflow on `main` through the workflow token, exports `NX_BASE` and `NX_HEAD`, and without a successful run it warns and uses `HEAD~1`, because `error-on-no-successful-workflow` is false.

Nx project tags select the language and the graph keeps the task order:

```bash
nx affected -t check --exclude='*,!tag:language:typescript'
```

Each language job runs `nx affected -t format`, then `git diff --exit-code`, then `nx affected -t check` under the same filter, and the .NET job runs `build test lint` in place of `check` with the binlog switch after `--`. `nx run rasm:coverage --language <x>` follows the tests, each language uploads its coverage directory as its own artifact with the runtime identifier in a matrix name, and the .NET job uploads its binlog directory under `failure()`.

Hosted runners and act set `CI`, and Nx runs without its daemon under `CI` unless `NX_DAEMON` overrides it. The affected targets' inputs hold the runtime versions and the files the workflow consumes.

## [03]-[SETUP]

The setup composite action installs the toolchain and the workspace dependencies. `jdx/mise-action` installs `mise.toml` and, with its `env` and `export_path` defaults, exports the `[env]` values and the `_.path` entries to the later steps, where `nx` resolves from `node_modules/.bin`. Its `cache` input is false, because a restored installation satisfies `mise install` without resolving the newer release a `latest` row names, and its `github_token` default is the workflow token, which lifts the GitHub download rate limit for the tools mise fetches from releases.

`pnpm install` runs with no flag, and `uv sync --locked` takes its group flags from the action's `python` input, `--only-group eng` by default and `--all-groups` from the Python job, read into a Bash array and passed as quoted words.

Package downloads cache under the files that determine their contents:

| [INDEX] | [CACHE]              | [KEY INPUTS]                                                      |
| :-----: | :------------------- | :---------------------------------------------------------------- |
|  [01]   | NuGet packages       | Central versions, NuGet config, shared build files, project files |
|  [02]   | uv                   | `uv.lock`                                                         |
|  [03]   | pnpm store and cache | `pnpm-lock.yaml`                                                  |

Every key prefixes the runner's operating system and architecture, and no entry declares `restore-keys`, because a prefix match restores an older folder and the post step saves a new entry on every run. `actions/cache` restores an exact key match, saves the paths under the key when the job succeeds and no exact match existed, and never changes an existing entry. `uv cache prune --ci` after installation keeps the wheels built from source and removes the pre-built wheels and unzipped archives a runner re-downloads.

## [04]-[NATIVE_ARTIFACTS]

The native cache action restores each entry with `actions/cache/restore` keyed on the runtime identifier and the manifests under `eng/native/` that decide it, and outputs `cache-primary-key` for each miss. The workflow saves each non-empty key with `actions/cache/save` under `always()`, because the combined action saves under `success()` alone and a failed build kept nothing, and the build output entry saves on the one runtime identifier that builds it.

The staged trees travel as artifacts:
- The Linux x64 job installs the assembler the native port needs through the runner's package manager
- Each matrix job archives `native/*/stage` into `$RUNNER_TEMP` and uploads the tar file with `archive: false`
- The pack job downloads `native-stage-*` with `merge-multiple: true` into one directory and extracts each tar file under `.artifacts/`
- The pack job packs with `--excludeTaskDependencies` and uploads the local NuGet feed
- `download-artifact` checks the SHA-256 digest `upload-artifact` recorded, and `digest-mismatch` defaults to `error`

`archive: false` uploads the one file as it is, takes the file name as the artifact name, ignores `name`, and skips the zip compression that drops executable modes. `upload-artifact` excludes hidden files unless `include-hidden-files` is true, `overwrite: true` deletes a same-named artifact before the upload, and `compression-level` applies to the zip alone. Artifacts carry outputs another job consumes, caches carry downloads and intermediate builds, and every upload path, download path, Nx output, and package consumer name the same directory.

## [05]-[PUBLISHING]

`release.yml` versions from the git tags and publishes with no stored registry token. Both release steps take the dispatch input as `${PROJECTS:+--projects="$PROJECTS"}`, the whole optional argument quoted so a filter with spaces or globs stays one Nx value. The release runs on dispatch, because GitHub raises no push event when one push creates more than three tags and independent versioning tags every released project.

The git identity of the run is the Actions bot, set by `git config` before the release step, and `nx release --skip-publish` tags and pushes under the workflow token. The NuGet trusted publishing policy on nuget.org names the repository owner, the repository, the workflow file name alone (`release.yml`), and an optional environment, and a policy for a private repository stays temporarily active for 7 days until a first publish supplies the repository id. `NuGet/login@v1` sends the job's OIDC token, under `id-token: write`, to `https://www.nuget.org/api/v2/token` with the audience `https://www.nuget.org` and outputs `NUGET_API_KEY`, a key valid for one hour that each token buys once, so the login step runs right before the pushes. Its `user` input is the nuget.org profile name, never an email address. The publishing steps read the key from the step output through `env:`, and no step writes it to a log, because nothing the login step outputs is masked there.

`dotnet nuget push --skip-duplicate` publishes the staged native feed, the native dependencies the reusable workflow prepared, independently of the managed project filter. `nx release publish --dry-run` runs the custom publishers with `NX_DRY_RUN` set, and each publisher reads it through its CLI parser before a registry write.

## [06]-[LOCAL_EXECUTION]

The root `lint` target runs actionlint, which runs shellcheck over every `run` step and pyflakes over Python steps when the binaries sit on PATH, and the root `workflow` target runs act. Exceptions sit in `.github/actionlint.yaml` as an `ignore` regex per path: the release entry accepts GitHub's `queue` field until actionlint accepts it, the file names the upstream issue, and a neighboring invalid field still fails.

| [INDEX] | [CHECK]                                            | [PURPOSE]                                                             |
| :-----: | :------------------------------------------------- | :-------------------------------------------------------------------- |
|  [01]   | `nx run rasm:lint`                                 | Workflow expressions, action inputs, shell diagnostics, project rules |
|  [02]   | `nx run rasm:workflow -- --list`                   | List push jobs without execution                                      |
|  [03]   | `nx run rasm:workflow -- workflow_dispatch --list` | List dispatch jobs without execution                                  |
|  [04]   | `nx run rasm:workflow -- --job=<job>`              | Execute the selected job and its dependencies                         |

The `workflow` target selects `ci.yml`, the push event, Linux x64, one image per Linux runner label through `-P <label>=<image>`, and the action, cache-server, and artifact-server storage under `.cache/act/`, where `--artifact-server-path` turns the artifact service on. Pass the event positionally and `--workflows <file>` to select another file, `--job=<job>` and `--matrix <key>:<value>` to select a job and its matrix entry, and `--container-architecture` with the runner image for an ARM entry, because act applies one architecture to every container of the invocation:

```bash
nx run rasm:workflow -- --job=dotnet --matrix=rid:linux-arm64 \
  --container-architecture=linux/arm64 \
  --platform=ubuntu-24.04-arm=ghcr.io/catthehacker/ubuntu:full-24.04
```

act reads `.env`, `.secrets`, `.vars`, and `.input` from the working directory and an `.actrc` from the working directory, the home directory, and the XDG configuration directory, pulls images on every run unless `--pull=false`, and skips a job on a runner label with no image, so a Linux run proves nothing about macOS or Windows. act rejects `release.yml` at schema validation on the `queue` field, so local checks select `ci.yml`.

Hosted execution proves concurrency, cancellation, job timeouts, permissions, and OIDC, which act does not implement, a listing proves job selection alone, and a job run proves command execution and artifact transfer.

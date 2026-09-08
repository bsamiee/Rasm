# [GITHUB]

GitHub Actions runs repository events and hosted jobs, Nx selects projects and orders tasks, and act runs Linux jobs on the machine.

## [01]-[WORKFLOWS]

Each workflow declares the event that requires its work and reuses jobs through `workflow_call` and steps through composite actions:

| [INDEX] | [WORKFLOW]    | [TRIGGER]                         | [PERMISSIONS]                             | [DEPENDENCY]                    |
| :-----: | :------------ | :-------------------------------- | :---------------------------------------- | :------------------------------ |
|  [01]   | `ci.yml`      | Pull request, main push, dispatch | `contents` and `actions` read             | .NET consumes the native feed   |
|  [02]   | `native.yml`  | `workflow_call`                   | `contents` read                           | Matrix stage precedes pack      |
|  [03]   | `release.yml` | Dispatch with project filter      | `contents` read, the release job writes   | Native feed precedes publishing |
|  [04]   | `codeql.yml`  | Pull request, main push, schedule | `contents` read, `security-events` write  | None                            |

`needs` names the job with outputs a job consumes. `strategy.matrix.include` lists runner and runtime-identifier pairs. `fail-fast: false` keeps every platform result. Python and TypeScript run without the native feed.

GitHub scores a skipped required check as passing. `ci.yml` ends in `required`, the one status-check context the `main` ruleset names.

`codeql.yml` is the advanced setup, a matrix of `actions`, `csharp`, `javascript-typescript`, and `python` at `build-mode: none` with `category: /language:<language>` on the analyze step. Default setup disables the workflow and stays off in repository settings.

Caches and artifacts read a token persisted in `.git/config`, every checkout that pushes nothing sets `persist-credentials: false`. Release checkout keeps the token for the tags `nx release` pushes, with the reason on its `# zizmor: ignore[artipacked] <reason>` comment.

actionlint and act reject the step concurrency keys (`background`, `wait`, `wait-all`, `cancel`, `parallel`) and `queue` as unknown. Steps run in sequence until they accept them. Nx runs projects concurrently.

`defaults.run.shell: bash` covers every workflow step. Each composite action step declares `shell`. GitHub substitutes `${{ }}` into script text before the shell parses it. Expression values reach a script through the step `env` map as a shell variable. actionlint reports untrusted contexts (`github.event.*`) alone.

Concurrency group names are case-insensitive and name the shared operation. `queue: max` refuses `cancel-in-progress: true`:
- `ci.yml` groups by workflow and ref with `cancel-in-progress` on pull requests, a pending run replacing the previous pending one
- `codeql.yml` groups per job by workflow, ref, and matrix language, with the same cancellation
- `release.yml` groups by workflow with `queue: max`, up to 100 pending runs processed in the order each began waiting

## [02]-[AFFECTED_PROJECTS]

`actions/checkout` runs with `fetch-depth: 0` for tags and merge bases and `filter: tree:0`, a treeless clone that keeps every commit and fetches trees on demand. `nrwl/nx-set-shas` runs in each job that calls `nx affected` with `workflow-id: ci.yml`, reads the last successful `push` run of that workflow on `main` through the workflow token, and exports `NX_BASE` and `NX_HEAD`. Under `error-on-no-successful-workflow: false` the action warns and uses `HEAD~1` when no run succeeded. Without `workflow-id` it looks up the current run id, present on hosted runs alone.

Nx project tags select the language and the graph keeps the task order:

```bash
nx affected -t check --exclude='*,!tag:language:typescript,rasm'
```

Job `check` runs `nx run rasm:check` over the tree. Job `format` runs `nx run rasm:format` then `git diff --exit-code`. Each language job runs `nx affected -t check` under its filter, `typescript` with the root project excluded. .NET job runs `build test` in place of `check` with the binlog switch after `--` and uploads the binlog directory under `failure()`. `nx run rasm:coverage --language <x>` follows the tests. Each language uploads its coverage directory as an artifact, named with the runtime identifier in a matrix.

## [03]-[SETUP]

Setup composite action takes no input and installs the toolchain and workspace dependencies. `jdx/mise-action` installs `mise.toml`, reads `.miserc.toml` and the platform file it loads, and under its `env` and `export_path` defaults exports the `[env]` rows and `_.path` entries to later steps. `cache` input is false, a restored installation satisfies `mise install` without resolving the newer release a `latest` row names. `github_token` defaults to the workflow token, lifting GitHub's download rate limit for tools mise fetches from releases.

`pnpm install` runs with no flag. `uv sync --locked --only-group eng` installs the script group alone. Python job provisions the native releases sdist-only lock members build against before `uv sync --locked` over every group.

Package downloads cache under the files that decide their contents:

| [INDEX] | [CACHE]              | [KEY_INPUTS]                                                      |
| :-----: | :------------------- | :---------------------------------------------------------------- |
|  [01]   | NuGet packages       | Central versions, NuGet config, shared build files, project files |
|  [02]   | uv                   | `uv.lock`                                                         |
|  [03]   | pnpm store and cache | `pnpm-lock.yaml`                                                  |

Every key prefixes the runner operating system and architecture. No entry declares `restore-keys`, a prefix match restores an older folder and the post step saves a new entry on every run. `actions/cache` restores an exact key match, saves the paths under the key when the job succeeds and no exact match existed, and never changes an existing entry. `uv cache prune --ci` after installation keeps wheels built from source and removes pre-built wheels and unzipped archives a runner re-downloads.

## [04]-[NATIVE_ARTIFACTS]

Native cache action restores each entry with `actions/cache/restore` keyed on the runtime identifier and the `eng/native/` manifests that decide it, and outputs `cache-primary-key` for each miss. `actions/cache` saves under `success()` alone and keeps nothing from a failed build, the workflow saves each non-empty key with `actions/cache/save` under `always()`. Build output entries save on the one rid that builds them.

Staged trees pass between jobs as artifacts:
- Linux x64 job installs the assembler the native port needs through the runner package manager
- Each matrix job archives `native/*/stage` into `$RUNNER_TEMP` and uploads the tar file with `archive: false`
- Pack job downloads `native-stage-*` with `merge-multiple: true` into one directory and extracts each tar file under `.artifacts/`
- Pack job packs with `--excludeTaskDependencies` and uploads the local NuGet feed
- `download-artifact` checks the SHA-256 digest `upload-artifact` recorded, and `digest-mismatch` defaults to `error`

`archive: false` uploads one file as it is, takes the file name as the artifact name, ignores `name`, and skips the zip compression that drops executable modes. `upload-artifact` excludes hidden files unless `include-hidden-files` is true. `overwrite: true` deletes a same-named artifact before upload. `compression-level` applies to the zip alone. Artifacts hold outputs another job consumes. Caches hold downloads and intermediate builds. Every upload path, download path, Nx output, and package consumer names one directory.

## [05]-[PUBLISHING]

`release.yml` versions from git tags and publishes with no stored registry token. Each release step takes the dispatch input as `${PROJECTS:+--projects="$PROJECTS"}`, the whole optional argument quoted, a filter with spaces or globs staying one Nx value. Release runs on dispatch, GitHub raises no push event when one push creates more than three tags and independent versioning tags every released project.

Release job runs in the `release` environment, with OIDC subject `repo:<owner>/<repo>:environment:release` and a deployment record per run:
- Git identity of the run is the Actions bot, set by `git config` before the release step
- `nx release --skip-publish` tags and pushes under the workflow token
- nuget.org trusted publishing policy names the owner, repository, workflow file name alone (`release.yml`), and an optional environment
- `NuGet/login@v1` sends the job OIDC token under `id-token: write` to `https://www.nuget.org/api/v2/token` with audience `https://www.nuget.org`
- Login outputs `NUGET_API_KEY` right before the pushes, valid for one hour and issued once per token
- `user` input is the nuget.org profile name, the `vars.NUGET_USER` variable row of the program
- Publishing steps read the key from the step output through `env:`
- Login step masks the key with `core.setSecret` before setting the output

`dotnet nuget push --skip-duplicate` publishes the staged native feed the reusable workflow prepared, independent of the managed project filter. `nx release publish --dry-run` runs the custom publishers with `NX_DRY_RUN` set.

`nx release publish` packs the managed packages and builds the Python distributions after the native push. `actions/attest@v4` runs after the last push under `attestations: write` with `subject-path` naming the native feed, managed packages, and Python distributions.

## [06]-[REPOSITORY_FILES]

Repository is public under a personal account. Rulesets, environments, code scanning, secret scanning push protection, and artifact attestations apply. Merge queue (`merge_group`) does not. Configuration files beside the workflows:

| [INDEX] | [FILE]             | [HOLDS]                                                                                              |
| :-----: | :----------------- | :--------------------------------------------------------------------------------------------------- |
|  [01]   | `dependabot.yml`   | `github-actions` over `/.github/workflows` and `/.github/actions/*`, weekly, one group, no cooldown  |
|  [02]   | `actionlint.yaml`  | `config-variables` with every `vars.*` the program declares, an `ignore` regex per path for `queue`  |
|  [03]   | `zizmor.yml`       | `unpinned-uses` at `"*": ref-pin`, `dependabot-cooldown` and `self-repository` off with the reasons  |

`dependabot.yml` names no package ecosystem, `upgrade` moves every dependency. Dependabot pull requests wait for the `required` check. Owner pushes bypass through the admin role. `actionlint.yaml` `ignore` entry accepts GitHub's `queue` field alone until actionlint accepts it. `ref-pin` accepts any tag, branch, or SHA, the form for major tags Dependabot moves in place of digest pins. `self-repository` stays off while actionlint rejects the `$/` form of a local `uses` reference.

Files GitHub reads from `.github/` that stay out:

| [INDEX] | [FILE]                                    | [REASON]                                                    |
| :-----: | :---------------------------------------- | :---------------------------------------------------------- |
|  [01]   | `CODEOWNERS`                              | One owner, no review routing                                |
|  [02]   | Issue, pull request, discussion templates | No contributor form to collect                              |
|  [03]   | `FUNDING.yml`                             | No sponsorship                                              |
|  [04]   | `SECURITY.md`, `CONTRIBUTING.md`          | Root `README.md` holds the repository prose                 |
|  [05]   | `release.yml` notes configuration         | `nx release` writes the release notes                       |
|  [06]   | `labeler.yml`                             | No label taxonomy                                           |
|  [07]   | `copilot-instructions.md`                 | Copilot reads `AGENTS.md`                                   |
|  [08]   | `workflow-templates/`                     | Organization repositories alone                             |

## [07]-[LOCAL_EXECUTION]

Root `workflow` runs act. Root `lint` runs yamllint, actionlint, and zizmor over the workflow files:
- actionlint runs shellcheck over every `run` step and pyflakes over Python steps when the binaries sit on PATH
- `zizmor --offline` covers workflow and action files under the default persona and skips audits that resolve refs over the network
- Security audits come from zizmor, repository forms from the `github` rule family under `tools/ast-grep/rules/yaml/github/`

| [INDEX] | [CHECK]                                            | [PURPOSE]                                                              |
| :-----: | :------------------------------------------------- | :--------------------------------------------------------------------- |
|  [01]   | `nx run rasm:lint`                                 | Expressions, action inputs, shell diagnostics, security audits, rules  |
|  [02]   | `nx run rasm:workflow -- --list`                   | List push jobs without execution                                       |
|  [03]   | `nx run rasm:workflow -- workflow_dispatch --list` | List dispatch jobs without execution                                   |
|  [04]   | `nx run rasm:workflow -- --job=<job>`              | Execute the selected job and its dependencies                          |

`eng/scripts/workflow.py` runs act over `ci.yml` against the machine Docker daemon with the repository flags and the arguments after `--`. Exit cleanup removes every `act-*` container and per-run volume, act leaves them after a failed container start or an interrupt. Pass the event positionally and `--job=<job>` with `--matrix <key>:<value>` for a job and its matrix entry. Default runs copy the working tree, the `git diff --exit-code` step reporting pending edits. `--no-skip-checkout` runs the pushed commit through `actions/checkout` with the local workflow file:

```bash
nx run rasm:workflow -- --job=dotnet --matrix=rid:linux-arm64 --no-skip-checkout
```

| [INDEX] | [FLAG]                                  | [REASON]                                                                          |
| :-----: | :-------------------------------------- | :-------------------------------------------------------------------------------- |
|  [01]   | `--rm`                                  | Removes the container after a failed job, act removes it after a succeeded one    |
|  [02]   | `--workflows=.github/workflows/ci.yml`  | act rejects `release.yml` at schema validation on the `queue` field               |
|  [03]   | `--container-architecture=linux/<arch>` | Host architecture, a machine `actrc` value makes act pull the other variant       |
|  [04]   | `--container-daemon-socket=-`           | No job runs docker, and the `DOCKER_HOST` socket has no path inside the VM        |
|  [05]   | `--platform=<label>=<image>`            | `ubuntu-24.04` and `ubuntu-24.04-arm` on `ghcr.io/catthehacker/ubuntu:act-24.04`  |
|  [06]   | `--action-cache-path`, the server paths | `.cache/act/{actions,cache,artifacts}`, the artifact path starts that service     |

Docker daemon is a machine profile service, Pulumi has no provider for a local VM. `DOCKER_HOST` names the daemon socket. `act-24.04` holds the tools the setup action and `run` steps call. `full-24.04`, the hosted runner filesystem, adds tools vcpkg fetches itself.

act behavior:
- Reads `.env`, `.secrets`, `.vars`, and `.input` from the working directory
- Prepends the XDG `act/actrc`, `~/.actrc`, and `./.actrc` to the command line, the later scalar flag and the later `-P` for one label win
- `--action-offline-mode` from a machine file turns image and action pulls off
- `act --bug-report` prints the files read, the docker host, and the engine storage driver
- Pulls a present tag as one manifest check per run unless `--pull=false`
- Takes `GITHUB_TOKEN` from `gh auth token` and sets `GITHUB_RUN_ID` to 1
- Skips a job on a runner label with no image, a Linux run proving nothing about macOS or Windows
- `--reuse` keeps a succeeded job's container for state between runs, removed by the script cleanup

Daemon's containerd image store keeps each pulled platform variant compressed and unpacked under `/var/lib/containerd` on the instance disk and unpacks a variant at container creation. `docker image ls --tree` shows each variant with its unpacked size, and `docker system df` the total. `no space left on device` at `Set up job` is a variant unpacking into less free space than its size. `docker image rm --force --platform <platform> <image>` removes one variant, `docker image rm <image>` every variant. After a run, `docker ps -a --filter name=act- -q` prints nothing and `docker volume ls -q --filter name=act-` prints `act-toolcache`, the volume every run shares.

Hosted execution proves concurrency, cancellation, job timeouts, permissions, and OIDC, a listing proves job selection, and a job run proves command execution and artifact transfer.

## [08]-[DECISIONS]

New workflows, jobs, and actions take each structural decision from its row, matched by `templates/`:

| [INDEX] | [DECISION]      | [FORM]                                                                                                  |
| :-----: | :-------------- | :------------------------------------------------------------------------------------------------------ |
|  [01]   | Event           | Repository event or schedule that requires the work, `workflow_dispatch` with typed inputs              |
|  [02]   | Permissions     | Keys the jobs call at workflow level, a job block narrows them, `permissions: {}` for none              |
|  [03]   | Concurrency     | Workflow and ref with cancellation on pull requests, `queue: max` for a serialized operation            |
|  [04]   | Checkout        | `fetch-depth: 0`, `filter: tree:0`, `persist-credentials: false` unless the job pushes                  |
|  [05]   | Setup           | Setup composite action, then the native cache action where a job stages or provisions                   |
|  [06]   | Affected filter | `nx-set-shas` with `workflow-id`, then `nx affected -t <target> --exclude='*,!tag:language:<language>'` |
|  [07]   | Artifacts       | One upload per matrix entry named by the runtime identifier, `archive: false` for a single file         |
|  [08]   | Publish         | `release` environment, `id-token: write`, registry login action, attestation after the push             |
|  [09]   | Status check    | Fan-in job with `needs` over every job, `if: always()`, `permissions: {}`, and the `jq` result test     |

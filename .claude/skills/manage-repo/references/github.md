# [GITHUB]

GitHub Actions runs the repository events and the hosted jobs, Nx selects the projects and orders the tasks, and act runs the Linux jobs on the machine.

## [01]-[WORKFLOWS]

Each workflow declares the event that requires its work and reuses jobs through `workflow_call` and steps through composite actions. Targets and scripts hold the build logic, and a workflow holds commands alone:

| [INDEX] | [WORKFLOW]    | [TRIGGER]                         | [PERMISSIONS]                             | [DEPENDENCY]                    |
| :-----: | :------------ | :-------------------------------- | :---------------------------------------- | :------------------------------ |
|  [01]   | `ci.yml`      | Pull request, main push, dispatch | `contents` and `actions` read             | .NET consumes the native feed   |
|  [02]   | `native.yml`  | `workflow_call`                   | `contents` read                           | Matrix stage precedes pack      |
|  [03]   | `release.yml` | Dispatch with project filter      | `contents` read, the release job writes   | Native feed precedes publishing |
|  [04]   | `codeql.yml`  | Pull request, main push, schedule | `contents` read, `security-events` write  | None                            |

`needs` names the job with the outputs a job consumes, `strategy.matrix.include` lists the runner and runtime-identifier pairs, and `fail-fast: false` keeps every platform's result. Python and TypeScript run without the native feed.

GitHub scores a skipped required check as passing, and `ci.yml` ends in `required`, the one status-check context the `main` ruleset names.

`codeql.yml` is the advanced setup, a matrix of `actions`, `csharp`, `javascript-typescript`, and `python` at `build-mode: none` with `category: /language:<language>` on the analyze step. Enabling the default setup disables the workflow, and repository settings keep it off.

Persisted tokens sit in `.git/config`, where caches and artifacts read them, and every checkout that pushes nothing sets `persist-credentials: false`. The release checkout keeps its token for the tags `nx release` pushes, the reason on its `# zizmor: ignore[artipacked] <reason>` comment.

actionlint and act reject the step concurrency keys (`background`, `wait`, `wait-all`, `cancel`, `parallel`) and `queue` as unknown. Steps run in sequence until both accept them, and Nx runs concurrent project work.

`defaults.run.shell: bash` covers every workflow step and reaches no composite action step, which declares its own `shell`. GitHub substitutes `${{ }}` into script text before the shell parses it, expression values reach a script through the step's `env` map as a shell variable, and actionlint reports untrusted contexts (`github.event.*`) alone.

Concurrency groups name the shared operation, the group name is case-insensitive, and `queue: max` refuses `cancel-in-progress: true`:
- `ci.yml` groups by workflow and ref with `cancel-in-progress` on pull requests, and a pending run replaces the previous pending one
- `codeql.yml` groups per job by workflow, ref, and matrix language, with the same cancellation
- `release.yml` groups by workflow with `queue: max`, up to 100 pending runs processed in the order each began waiting

## [02]-[AFFECTED_PROJECTS]

`actions/checkout` runs with `fetch-depth: 0` for the tags and merge bases and `filter: tree:0`, a treeless clone that keeps every commit and fetches trees on demand. `nrwl/nx-set-shas` runs in each job that calls `nx affected`, with `workflow-id: ci.yml`: it reads the last successful `push` run of that workflow on `main` through the workflow token, exports `NX_BASE` and `NX_HEAD`, and under `error-on-no-successful-workflow: false` warns and uses `HEAD~1` when no run succeeded. Without the input it looks the current run id up first, which a hosted run alone has.

Nx project tags select the language and the graph keeps the task order:

```bash
nx affected -t check --exclude='*,!tag:language:typescript,rasm'
```

Job `check` runs `nx run rasm:check` over the tree, job `format` runs `nx run rasm:format` then `git diff --exit-code`, and each language job runs `nx affected -t check` under its filter, `typescript` with the root project excluded. The .NET job runs `build test` in place of `check` with the binlog switch after `--` and uploads its binlog directory under `failure()`. `nx run rasm:coverage --language <x>` follows the tests, and each language uploads its coverage directory as an artifact, named with the runtime identifier in a matrix.

## [03]-[SETUP]

The setup composite action takes no input and installs the toolchain and the workspace dependencies. `jdx/mise-action` installs `mise.toml`, reads the committed `.miserc.toml` and the platform file it loads, and, with its `env` and `export_path` defaults, exports the `[env]` rows and the `_.path` entries to the later steps, where `nx` resolves from `node_modules/.bin`. Restored installations satisfy `mise install` without resolving the newer release a `latest` row names, and the `cache` input is false. `github_token` defaults to the workflow token, which lifts GitHub's download rate limit for tools mise fetches from releases.

`pnpm install` runs with no flag, and `uv sync --locked --only-group eng` installs the script group alone. Sdist-only lock members build against native releases, the Python job provisions them before `uv sync --locked` over every group, and `python.md` names the build inputs.

Package downloads cache under the files that decide their contents:

| [INDEX] | [CACHE]              | [KEY INPUTS]                                                      |
| :-----: | :------------------- | :---------------------------------------------------------------- |
|  [01]   | NuGet packages       | Central versions, NuGet config, shared build files, project files |
|  [02]   | uv                   | `uv.lock`                                                         |
|  [03]   | pnpm store and cache | `pnpm-lock.yaml`                                                  |

Every key prefixes the runner's operating system and architecture. Prefix matches restore an older folder and the post step then saves a new entry on every run, and no entry declares `restore-keys`. `actions/cache` restores an exact key match, saves the paths under the key when the job succeeds and no exact match existed, and never changes an existing entry. `uv cache prune --ci` after installation keeps the wheels built from source and removes the pre-built wheels and unzipped archives a runner re-downloads.

## [04]-[NATIVE_ARTIFACTS]

The native cache action restores each entry with `actions/cache/restore` keyed on the runtime identifier and the manifests under `eng/native/` that decide it, and outputs `cache-primary-key` for each miss. `actions/cache` saves under `success()` alone and a failed build kept nothing, the workflow saves each non-empty key with `actions/cache/save` under `always()`, and a build output entry saves on the one rid that builds it.

The staged trees travel as artifacts:
- The Linux x64 job installs the assembler the native port needs through the runner's package manager
- Each matrix job archives `native/*/stage` into `$RUNNER_TEMP` and uploads the tar file with `archive: false`
- The pack job downloads `native-stage-*` with `merge-multiple: true` into one directory and extracts each tar file under `.artifacts/`
- The pack job packs with `--excludeTaskDependencies` and uploads the local NuGet feed
- `download-artifact` checks the SHA-256 digest `upload-artifact` recorded, and `digest-mismatch` defaults to `error`

`archive: false` uploads the one file as it is, takes the file name as the artifact name, ignores `name`, and skips the zip compression that drops executable modes. `upload-artifact` excludes hidden files unless `include-hidden-files` is true, `overwrite: true` deletes a same-named artifact before the upload, and `compression-level` applies to the zip alone. Artifacts hold outputs another job consumes, caches hold downloads and intermediate builds, and every upload path, download path, Nx output, and package consumer name the same directory.

## [05]-[PUBLISHING]

`release.yml` versions from the git tags and publishes with no stored registry token. Both release steps take the dispatch input as `${PROJECTS:+--projects="$PROJECTS"}`, the whole optional argument quoted, and a filter with spaces or globs stays one Nx value. GitHub raises no push event when one push creates more than three tags, independent versioning tags every released project, and the release runs on dispatch.

The release job runs in the `release` environment: the OIDC subject becomes `repo:<owner>/<repo>:environment:release`, each run writes a deployment record, and the environment's `main` branch policy is a program row. The git identity of the run is the Actions bot, set by `git config` before the release step, and `nx release --skip-publish` tags and pushes under the workflow token. The NuGet trusted publishing policy on nuget.org names the repository owner, the repository, the workflow file name alone (`release.yml`), and an optional environment. `NuGet/login@v1` sends the job's OIDC token, under `id-token: write`, to `https://www.nuget.org/api/v2/token` with the audience `https://www.nuget.org` and outputs `NUGET_API_KEY`, a key valid for one hour that each token buys once, and the login step runs right before the pushes. Its `user` input is the nuget.org profile name, the `vars.NUGET_USER` variable row of the program. The publishing steps read the key from the step output through `env:`, and the login step masks it with `core.setSecret` before it sets the output.

`dotnet nuget push --skip-duplicate` publishes the staged native feed, the native dependencies the reusable workflow prepared, independently of the managed project filter. `nx release publish --dry-run` runs the custom publishers with `NX_DRY_RUN` set, and each publisher reads it through its CLI parser before a registry write.

`nx release publish` packs the managed packages and builds the Python distributions after the native push, and `actions/attest@v4` runs after the last push under `attestations: write`, with `subject-path` naming the native feed, the managed packages, and the Python distributions.

## [06]-[REPOSITORY_FILES]

The repository is public under a personal account: rulesets, environments, code scanning, secret scanning push protection, and artifact attestations apply, and a merge queue (`merge_group`) does not. The configuration files beside the workflows:

| [INDEX] | [FILE]             | [HOLDS]                                                                                              |
| :-----: | :----------------- | :--------------------------------------------------------------------------------------------------- |
|  [01]   | `dependabot.yml`   | `github-actions` over `/.github/workflows` and `/.github/actions/*`, weekly, one group, no cooldown  |
|  [02]   | `actionlint.yaml`  | `config-variables` with every `vars.*` the program declares, an `ignore` regex per path for `queue`  |
|  [03]   | `zizmor.yml`       | `unpinned-uses` at `"*": ref-pin`, `dependabot-cooldown` and `self-repository` off with the reasons  |

The `upgrade` target moves every dependency to its newest prerelease, `dependabot.yml` names no package ecosystem, and a Dependabot pull request waits for the `required` check while the owner's pushes bypass through the admin role. The `actionlint.yaml` `ignore` entry accepts GitHub's `queue` field until actionlint accepts it, the file names the upstream issue, and a neighboring invalid field still fails. `ref-pin` accepts any tag, branch, or SHA, the form for major tags Dependabot moves in place of digest pins, and `self-repository` stays off while actionlint rejects the `$/` form of a local `uses` reference.

The root `lint` target runs `zizmor --offline` over the workflow and action files under the default persona, and `--offline` skips the audits that resolve refs over the network. Use zizmor for the security audits and the `github` rule family under `tools/ast-grep/rules/yaml/github/` for the repository's own forms.

Files GitHub reads from `.github/` that stay out, with the reason:

| [INDEX] | [FILE]                                    | [REASON]                                                    |
| :-----: | :---------------------------------------- | :---------------------------------------------------------- |
|  [01]   | `CODEOWNERS`                              | One owner, no review routing                                |
|  [02]   | Issue, pull request, discussion templates | No contributor form to collect                              |
|  [03]   | `FUNDING.yml`                             | No sponsorship                                              |
|  [04]   | `SECURITY.md`, `CONTRIBUTING.md`          | The root `README.md` holds the repository prose             |
|  [05]   | `release.yml` notes configuration         | `nx release` writes the release notes                       |
|  [06]   | `labeler.yml`                             | No label taxonomy                                           |
|  [07]   | `copilot-instructions.md`                 | Copilot reads `AGENTS.md`                                   |
|  [08]   | `workflow-templates/`                     | Organization repositories alone                             |

## [07]-[LOCAL_EXECUTION]

Root `lint` runs yamllint, actionlint, and zizmor over the workflow files, and root `workflow` runs act. actionlint runs shellcheck over every `run` step and pyflakes over Python steps when the binaries sit on PATH.

| [INDEX] | [CHECK]                                            | [PURPOSE]                                                              |
| :-----: | :------------------------------------------------- | :--------------------------------------------------------------------- |
|  [01]   | `nx run rasm:lint`                                 | Expressions, action inputs, shell diagnostics, security audits, rules  |
|  [02]   | `nx run rasm:workflow -- --list`                   | List push jobs without execution                                       |
|  [03]   | `nx run rasm:workflow -- workflow_dispatch --list` | List dispatch jobs without execution                                   |
|  [04]   | `nx run rasm:workflow -- --job=<job>`              | Execute the selected job and its dependencies                          |

`eng/scripts/workflow.py` runs act over `ci.yml` against the machine's Docker daemon with the repository flags and the arguments after `--`. act removes containers only when a job's pipeline reaches its cleanup and leaves them after a failed container start or an interrupt, and the script removes every `act-*` container and per-run volume on every exit. Pass the event positionally and `--job=<job>` with `--matrix <key>:<value>` for a job and its matrix entry. The default run copies the working tree with its pending edits and the `git diff --exit-code` step reports them, and `--no-skip-checkout` runs the pushed commit through `actions/checkout` while the workflow file stays the local one:

```bash
nx run rasm:workflow -- --job=dotnet --matrix=rid:linux-arm64 --no-skip-checkout
```

| [INDEX] | [FLAG]                                  | [REASON]                                                                          |
| :-----: | :-------------------------------------- | :-------------------------------------------------------------------------------- |
|  [01]   | `--rm`                                  | Removes the container after a failed job, act removes it after a succeeded one    |
|  [02]   | `--workflows=.github/workflows/ci.yml`  | act rejects `release.yml` at schema validation on the `queue` field               |
|  [03]   | `--container-architecture=linux/<arch>` | The host architecture, a machine `actrc` value makes act pull the other variant   |
|  [04]   | `--container-daemon-socket=-`           | No job runs docker, and the `DOCKER_HOST` socket has no path inside the VM        |
|  [05]   | `--platform=<label>=<image>`            | `ubuntu-24.04` and `ubuntu-24.04-arm` on `ghcr.io/catthehacker/ubuntu:act-24.04`  |
|  [06]   | `--action-cache-path`, the server paths | `.cache/act/{actions,cache,artifacts}`, the artifact path starts that service     |

Each resource of a workflow has one owner: a setting, secret, variable, tool, or image takes its row from the `manage-repo` placement table, the Docker daemon is a service of the machine profile, and a run flag or runner image is a row in the script. `DOCKER_HOST` names the daemon's socket, and Pulumi has no provider for a local VM. The setup action installs the toolchain through mise, and a job needs an image with the tools that action and the `run` steps call: `act-24.04` holds them, and `full-24.04`, the hosted runner's filesystem, is many times larger and adds tools vcpkg fetches itself.

act reads `.env`, `.secrets`, `.vars`, and `.input` from the working directory and prepends the XDG `act/actrc`, `~/.actrc`, and `./.actrc` to the command line, where the later scalar flag wins, the later `-P` for the same label wins, and `--action-offline-mode` from a machine file turns image and action pulls off. `act --bug-report` prints the files it read, the docker host, and the engine's storage driver in one call. act pulls a present tag as one manifest check per run unless `--pull=false`, takes `GITHUB_TOKEN` from `gh auth token`, sets `GITHUB_RUN_ID` to 1, and skips a job on a runner label with no image, and a Linux run proves nothing about macOS or Windows. `--reuse` keeps a succeeded job's container for state between runs, and the script's cleanup removes it.

The daemon uses the containerd image store, which keeps each pulled platform variant compressed and unpacked under `/var/lib/containerd` on the instance disk and unpacks a variant at container creation. `docker image ls --tree` shows each variant with its unpacked size, and `docker system df` the total. `no space left on device` at `Set up job` is a variant unpacking into less free space than its size, `docker image rm --force --platform <platform> <image>` removes one variant, and `docker image rm <image>` every variant. After a run, `docker ps -a --filter name=act- -q` prints nothing and `docker volume ls -q --filter name=act-` prints `act-toolcache`, the volume every run shares.

Hosted execution proves concurrency, cancellation, job timeouts, permissions, and OIDC, which act does not implement, a listing proves job selection alone, and a job run proves command execution and artifact transfer.

## [08]-[DECISIONS]

New workflows, jobs, and actions take each structural decision from the form its row names, and the templates of the skill hold the same forms:

| [INDEX] | [DECISION]      | [FORM]                                                                                                  |
| :-----: | :-------------- | :------------------------------------------------------------------------------------------------------ |
|  [01]   | Event           | The repository event or schedule that requires the work, `workflow_dispatch` with typed inputs          |
|  [02]   | Permissions     | The keys the jobs call at the workflow level, a job block narrows them, `permissions: {}` for none      |
|  [03]   | Concurrency     | Workflow and ref with cancellation on pull requests, `queue: max` for a serialized operation            |
|  [04]   | Checkout        | `fetch-depth: 0`, `filter: tree:0`, `persist-credentials: false` unless the job pushes                  |
|  [05]   | Setup           | The setup composite action, then the native cache action where a job stages or provisions               |
|  [06]   | Affected filter | `nx-set-shas` with `workflow-id`, then `nx affected -t <target> --exclude='*,!tag:language:<language>'` |
|  [07]   | Artifacts       | One upload per matrix entry named by the runtime identifier, `archive: false` for a single file         |
|  [08]   | Publish         | The `release` environment, `id-token: write`, the registry login action, the attestation after the push |
|  [09]   | Status check    | The fan-in job with `needs` over every job, `if: always()`, `permissions: {}`, and the `jq` result test |

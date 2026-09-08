# [GITHUB]

GitHub Actions runs the repository events and the hosted jobs, Nx selects the projects and orders the tasks, and act runs the Linux jobs on the machine.

## [01]-[WORKFLOWS]

Each workflow declares the event that requires its work, reuses jobs through `workflow_call` and steps through composite actions, and holds commands alone, because the targets and scripts hold the build logic:

| [INDEX] | [WORKFLOW]    | [TRIGGER]                         | [PERMISSIONS]                             | [DEPENDENCY]                    |
| :-----: | :------------ | :-------------------------------- | :---------------------------------------- | :------------------------------ |
|  [01]   | `ci.yml`      | Pull request, main push, dispatch | `contents` and `actions` read             | .NET consumes the native feed   |
|  [02]   | `native.yml`  | `workflow_call`                   | `contents` read                           | Matrix stage precedes pack      |
|  [03]   | `release.yml` | Dispatch with project filter      | `contents` read, the release job writes   | Native feed precedes publishing |
|  [04]   | `codeql.yml`  | Pull request, main push, schedule | `contents` read, `security-events` write  | None                            |

`needs` names the job with the outputs a job consumes, `strategy.matrix.include` lists the runner and runtime-identifier pairs, and `fail-fast: false` keeps every platform's result. Python and TypeScript run without the native feed. The runner labels `ubuntu-slim`, `ubuntu-26.04` and `ubuntu-26.04-arm` as previews, `windows-2025-vs2026`, `macos-26`, and `xcode-27` exist beside the matrix labels.

`ci.yml` ends in the `required` job, the one status-check context the `main` ruleset names, because GitHub scores a skipped required check as passing.

`codeql.yml` is the advanced setup, a matrix of `actions`, `csharp`, `javascript-typescript`, and `python` at `build-mode: none` with `category: /language:<language>` on the analyze step. The default setup stays off in the repository settings, because enabling it disables the workflow.

Every checkout that pushes nothing sets `persist-credentials: false`, because the token otherwise sits in `.git/config`, where caches and artifacts read it. The release checkout keeps the token for the tags `nx release` pushes, with the reason on the inline `# zizmor: ignore[artipacked] <reason>` comment.

Steps run in sequence, because actionlint 1.7.12 rejects the step concurrency keys (`background`, `wait`, `wait-all`, `cancel`, `parallel`) and `queue` as `unexpected key` and act 0.2.89 rejects them as `Unknown Property` (rhysd/actionlint#657 and #693, nektos/act#6124), and Nx runs the concurrent project work.

`defaults.run.shell: bash` covers every workflow step, and a composite action step declares its own `shell`, because the workflow defaults do not reach it. Expression values reach a script through the step's `env` map, read as a shell variable, because GitHub substitutes `${{ }}` into the script text before the shell parses it, and actionlint reports the untrusted contexts (`github.event.*`) alone.

Concurrency groups name the shared operation, the group name is case-insensitive, and `queue: max` refuses `cancel-in-progress: true`:
- `ci.yml` groups by workflow and ref with `cancel-in-progress` on pull requests, and a pending run replaces the previous pending one
- `codeql.yml` groups per job by workflow, ref, and matrix language, with the same cancellation
- `release.yml` groups by workflow with `queue: max`, up to 100 pending runs processed in the order each began waiting

The token permissions are the ones the jobs call: `contents: read` for the checkout, `actions: read` for the run lookup of `nx-set-shas`, `security-events: write` for the SARIF upload, and on the release job `id-token: write` for the OIDC exchange, `contents: write` for the tags and releases `nx release` pushes, and `attestations: write` for the provenance, and jobs that call nothing hold `permissions: {}`.

## [02]-[AFFECTED_PROJECTS]

`actions/checkout` runs with `fetch-depth: 0` for the tags and merge bases and `filter: tree:0`, a treeless clone that keeps every commit and fetches trees on demand. `nrwl/nx-set-shas` runs in each job that calls `nx affected`, with `workflow-id: ci.yml`: it reads the last successful `push` run of that workflow on `main` through the workflow token, exports `NX_BASE` and `NX_HEAD`, and without a successful run it warns and uses `HEAD~1`, because `error-on-no-successful-workflow` is false. Without the input it looks the current run id up first, which a hosted run alone has.

Nx project tags select the language and the graph keeps the task order:

```bash
nx affected -t check --exclude='*,!tag:language:typescript'
```

Each language job runs `nx affected -t format`, then `git diff --exit-code`, then `nx affected -t check` under the same filter, and the .NET job runs `build test lint` in place of `check` with the binlog switch after `--`. `nx run rasm:coverage --language <x>` follows the tests, each language uploads its coverage directory as its own artifact with the runtime identifier in a matrix name, and the .NET job uploads its binlog directory under `failure()`.

Hosted runners and act set `CI`, and Nx runs without its daemon under `CI` unless `NX_DAEMON` overrides it. The affected targets' inputs hold the runtime versions and the files the workflow consumes.

## [03]-[SETUP]

The setup composite action takes no input and installs the toolchain and the workspace dependencies. `jdx/mise-action` installs `mise.toml`, reads the committed `.miserc.toml` and the platform file it loads, and, with its `env` and `export_path` defaults, exports the `[env]` rows and the `_.path` entries to the later steps, where `nx` resolves from `node_modules/.bin`. Its `cache` input is false, because a restored installation satisfies `mise install` without resolving the newer release a `latest` row names, and its `github_token` default is the workflow token, which lifts the GitHub download rate limit for the tools mise fetches from releases.

`pnpm install` runs with no flag, and `uv sync --locked --only-group eng` installs the script group alone. The Python job provisions the native releases before its `uv sync --locked` over every group, because the lock's sdist-only members build against them, and `python.md` names the build inputs.

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

`release.yml` versions from the git tags and publishes with no stored registry token. Both release steps take the dispatch input as `${PROJECTS:+--projects="$PROJECTS"}`, the whole optional argument quoted, and a filter with spaces or globs stays one Nx value. The release runs on dispatch, because GitHub raises no push event when one push creates more than three tags and independent versioning tags every released project.

The release job runs in the `release` environment: the OIDC subject becomes `repo:<owner>/<repo>:environment:release`, each run writes a deployment record, and the environment's `main` branch policy is a program row. The git identity of the run is the Actions bot, set by `git config` before the release step, and `nx release --skip-publish` tags and pushes under the workflow token. The NuGet trusted publishing policy on nuget.org names the repository owner, the repository, the workflow file name alone (`release.yml`), and an optional environment. `NuGet/login@v1` sends the job's OIDC token, under `id-token: write`, to `https://www.nuget.org/api/v2/token` with the audience `https://www.nuget.org` and outputs `NUGET_API_KEY`, a key valid for one hour that each token buys once, and the login step runs right before the pushes. Its `user` input is the nuget.org profile name, the `vars.NUGET_USER` variable row of the program, set once with `doppler secrets set NUGET_USER`. The publishing steps read the key from the step output through `env:`, and the login step masks it with `core.setSecret` before it sets the output.

`dotnet nuget push --skip-duplicate` publishes the staged native feed, the native dependencies the reusable workflow prepared, independently of the managed project filter. `nx release publish --dry-run` runs the custom publishers with `NX_DRY_RUN` set, and each publisher reads it through its CLI parser before a registry write.

`actions/attest@v4` runs after the last push under `attestations: write`, with `subject-path` naming the native feed, the managed packages, and the Python distributions, because `nx release publish` packs the managed packages and builds the Python distributions after the native push.

## [06]-[REPOSITORY_FILES]

The repository is public under a personal account: rulesets, environments, code scanning, secret scanning push protection, and artifact attestations apply, and a merge queue (`merge_group`) does not. The configuration files beside the workflows:

| [INDEX] | [FILE]             | [HOLDS]                                                                                              |
| :-----: | :----------------- | :--------------------------------------------------------------------------------------------------- |
|  [01]   | `dependabot.yml`   | `github-actions` over `/.github/workflows` and `/.github/actions/*`, weekly, one group, no cooldown  |
|  [02]   | `actionlint.yaml`  | `config-variables` with every `vars.*` the program declares, an `ignore` regex per path for `queue`  |
|  [03]   | `zizmor.yml`       | `unpinned-uses` at `"*": ref-pin`, `dependabot-cooldown` and `self-repository` off with the reasons  |

`dependabot.yml` names no package ecosystem, because the `upgrade` target moves every dependency to its newest prerelease, and a Dependabot pull request waits for the `required` check while the owner's pushes bypass through the admin role. The `actionlint.yaml` `ignore` entry accepts GitHub's `queue` field until actionlint accepts it, the file names the upstream issue, and a neighboring invalid field still fails. `ref-pin` accepts any tag, branch, or SHA, the form for major tags Dependabot moves in place of digest pins, and `self-repository` stays off because actionlint 1.7.12 rejects the `$/` form of a local `uses` reference.

The root `lint` target runs `zizmor --offline .github` under the default persona, and `--offline` skips the four audits that resolve refs over the network. Use zizmor for the security audits and the `github` rule family under `tools/ast-grep/rules/yaml/github/` for the repository's own forms.

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

The root `lint` target runs actionlint, which runs shellcheck over every `run` step and pyflakes over Python steps when the binaries sit on PATH, and zizmor, and the root `workflow` target runs act.

| [INDEX] | [CHECK]                                            | [PURPOSE]                                                              |
| :-----: | :------------------------------------------------- | :--------------------------------------------------------------------- |
|  [01]   | `nx run rasm:lint`                                 | Expressions, action inputs, shell diagnostics, security audits, rules  |
|  [02]   | `nx run rasm:workflow -- --list`                   | List push jobs without execution                                       |
|  [03]   | `nx run rasm:workflow -- workflow_dispatch --list` | List dispatch jobs without execution                                   |
|  [04]   | `nx run rasm:workflow -- --job=<job>`              | Execute the selected job and its dependencies                          |

The `workflow` target runs `eng/scripts/workflow.py`, which runs act over `ci.yml` against the Docker daemon the machine provides, with the repository's flags and the arguments after `--`, and removes every `act-*` container and per-run volume on every exit, because act removes them only when a job's pipeline reaches its cleanup and leaves them after a failed container start or an interrupt. Pass the event positionally, `--job=<job>` and `--matrix <key>:<value>` for a job and its matrix entry, and `--no-skip-checkout` to run the pushed commit through `actions/checkout`, because the default copies the working tree with its pending edits and the `git diff --exit-code` step reports them, while the workflow file stays the local one:

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

Each resource of a workflow has one owner: a setting, secret, variable, tool, or image takes its row from the `manage-repo` placement table, the Docker daemon is a service of the machine profile, which declares the colima instance in `colima.yaml` under `COLIMA_HOME` and starts it through a launchd agent with `colima start default -f --activate=true --save-config=false`, and a run flag or runner image is a row in the script. `DOCKER_HOST` names the daemon's socket, `colima list` shows the instance, and Pulumi has no provider for a local VM. The setup action installs the toolchain through mise, and a job needs an image with sudo, apt, git, curl, python3, a C compiler, node, and the docker CLI: `act-24.04` holds them and unpacks to 2.3 GB, and `full-24.04`, the hosted runner's filesystem, unpacks to about 60 GB for amd64 and adds tools vcpkg fetches itself.

act reads `.env`, `.secrets`, `.vars`, and `.input` from the working directory and prepends the XDG `act/actrc`, `~/.actrc`, and `./.actrc` to the command line, where the later scalar flag wins, the later `-P` for the same label wins, and `--action-offline-mode` from a machine file turns image and action pulls off. `act --bug-report` prints the files it read, the docker host, and the engine's storage driver in one call. act pulls a present tag as one manifest check per run unless `--pull=false`, takes `GITHUB_TOKEN` from `gh auth token`, sets `GITHUB_RUN_ID` to 1, and skips a job on a runner label with no image, and a Linux run proves nothing about macOS or Windows. `--reuse` keeps a succeeded job's container for state between runs, and the script's cleanup removes it.

The daemon uses the containerd image store, which keeps each pulled platform variant compressed and unpacked under `/var/lib/containerd` on the instance disk and unpacks a variant at container creation. `colima ssh -- df -h /var/lib/docker` shows the disk, `docker image ls --tree` each variant with its unpacked size, and `docker system df` the total. `no space left on device` at `Set up job` is a variant unpacking into less free space than its size, `docker image rm --force --platform <platform> <image>` removes one variant, and `docker image rm <image>` every variant. After a run, `docker ps -a --filter name=act- -q` prints nothing and `docker volume ls -q --filter name=act-` prints `act-toolcache`, the volume every run shares, and the gate of the maintainer reads both.

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

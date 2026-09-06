# [GITHUB]

Everything under `.github/` is the workflows, the composite actions they call, and the local run of their Linux jobs, and the hosted runner and act perform each job the same way.

## [01]-[WORKFLOWS]

Each workflow declares its trigger, `contents: read`, and bash as the run shell, and the reusable native workflow precedes every job that consumes packages:

| [INDEX] | [WORKFLOW]    | [TRIGGER]                                      | [JOBS]                                                                 |
| :-----: | :------------ | :--------------------------------------------- | :--------------------------------------------------------------------- |
|  [01]   | `ci.yml`      | Pull request, base branch push, dispatch       | `native`, then one job per language, the native-package job by rid     |
|  [02]   | `native.yml`  | `workflow_call` alone                          | `stage` as a matrix by rid, then `pack` on one host                    |
|  [03]   | `release.yml` | Dispatch with an optional project filter       | `native`, then `release` with `contents: write` and `id-token: write`  |

The `ci.yml` concurrency group is the workflow and the ref, and a newer pull request run cancels the older one. Each language job runs the same steps in order:

| [INDEX] | [STEP]                                                     | [REASON]                                                                 |
| :-----: | :--------------------------------------------------------- | :----------------------------------------------------------------------- |
|  [01]   | `actions/checkout`, `fetch-depth: 0`, `filter: tree:0`     | Full history for the base commit and the tag readers, trees on demand    |
|  [02]   | `nrwl/nx-set-shas` in each language job                    | Exports `NX_BASE` and `NX_HEAD` for `nx affected`, no graph job exists   |
|  [03]   | Setup action                                               | Toolchain through mise, package folders restored, dependencies installed |
|  [04]   | `actions/download-artifact` of the local feed              | Native-package job restores the packed feed before it builds             |
|  [05]   | `nx affected -t <targets> --exclude='*,!tag:language:<x>'` | One command runs the affected targets of one language in graph order     |
|  [06]   | `git diff --exit-code` after the rewriting targets         | Fails the job on a rewrite `lint` or `format` made                       |
|  [07]   | `nx run <root>:coverage --language <x>`                    | Merges one language's coverage, `--excludeTaskDependencies` skips tests  |
|  [08]   | `actions/upload-artifact` of the coverage directory        | One artifact per language, per rid for the matrix job                    |

`CI` is set on every hosted runner and under act, Nx then computes the graph in process with no daemon, and `NX_DAEMON` overrides it.

The native-package job passes the binlog switch to the build after `--` and uploads the binlog directory under `failure()`.

The native workflow moves the staged trees from the matrix to one host:
- `stage` installs the assembler the Linux x64 port needs through the runner's package manager, runs `stage`, and saves the native caches
- `stage` archives `native/*/stage` into the runner's temporary directory and uploads the archive with `archive: false`
- `pack` downloads every archive with `merge-multiple`, extracts them under `.artifacts/`, and runs `pack` with `--excludeTaskDependencies`
- `pack` uploads the local feed as the artifact the language job and the release job download

## [02]-[SETUP]

The setup action installs the toolchain through mise, restores each package folder keyed on the files that decide its contents, and installs the workspace dependencies:
- `jdx/mise-action` with `cache: false` installs every `[tools]` row per job and exports `[env]` and the PATH additions to later steps
- The action's cache key hashes the config files alone, `mise install` treats `latest` as installed, and a cached `latest` froze at its build
- `pnpm install` runs with no flag
- `uv sync --locked` takes its group flags from the action's `python` input, the scripts' group by default, and the Python job passes `--all-groups`
- `uv cache prune --ci` removes the pre-built wheels and unzipped archives a runner re-downloads, and wheels built from source stay

| [INDEX] | [FOLDER]                     | [KEY]                                                                                             |
| :-----: | :--------------------------- | :------------------------------------------------------------------------------------------------ |
|  [01]   | NuGet global packages folder | `Directory.Packages.props`, `NuGet.config`, the `Directory.Build.*` files, and every project file |
|  [02]   | uv cache                     | `uv.lock`                                                                                         |
|  [03]   | pnpm store and cache         | `pnpm-lock.yaml`                                                                                  |

Every key prefixes the runner's operating system and architecture, and no cache entry declares `restore-keys`, because a prefix match restores an older folder and the post step saves a new entry.

## [03]-[NATIVE_CACHE]

The native cache action restores the vcpkg binary cache and downloads, the pinned release downloads, and one library's build output for one runtime identifier, and the save steps sit in the workflow:
- Each restore step is `actions/cache/restore` keyed on the rid and the manifests under `eng/native/` that decide the entry
- The action outputs each entry's primary key, and the workflow saves with `actions/cache/save` under `always()` when the key is non-empty
- Failed stages save what they built, and `actions/cache` alone saves under `success()`, which kept nothing from a failed build
- The build output entry saves on the one rid that builds it

## [04]-[PUBLISHING]

The release workflow versions from the git tags and publishes through trusted publishing, with no registry token stored:
- `nx release --skip-publish` runs under the workflow token, with `--projects` from the dispatch input when one is given, and pushes the tags
- The git identity of the run is the Actions bot, set by `git config` before the release step
- `NuGet/login` exchanges the job's OIDC token, under `id-token: write`, for a short-lived API key keyed on the workflow file name
- `nx release publish` and a `dotnet nuget push` of the local feed with `--skip-duplicate` read that key from the step output through `env:`
- Nothing the store injects is masked in workflow logs, and a secret reaches a log through no step

## [05]-[LOCAL_RUN]

The root `lint` target runs `actionlint` over the workflows, and the root `workflow` target runs the Linux jobs under act, both after a workflow or action changes:
- `actionlint` runs from the root `lint` target with `forwardAllArgs: false`, and the `.github/` tree is one of the target's inputs
- The `workflow` target runs `act push` with an image per Linux runner label, the daemon socket, and the server paths under `.cache/act/`
- act reads no `.actrc` from the repository, it reads the XDG, home, and current directory files alone, and the flags sit on the `workflow` target
- Artifact upload and download steps carry no act guard, because the target passes `--artifact-server-path` and act serves them
- act skips a job on a runner label with no image as an unsupported platform

## [06]-[ANTI_PATTERNS]

| [INDEX] | [SMELL]                                                  | [CORRECT_FORM]                                          |
| :-----: | :------------------------------------------------------- | :------------------------------------------------------ |
|  [01]   | `!env.ACT` on upload steps with unguarded download steps | Artifacts on under act through the artifact server path |

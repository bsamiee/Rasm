<!-- Source for monorepo-build-infrastructure [05]-[CI] and references/ci-workflow.md, nothing integrated yet -->
# CI on GitHub Actions for Rasm

Settled decisions:
- mise installs node, pnpm, python, uv, and the .NET SDK on every runner and on every developer machine
- act and actionlint come from mise, and the GitHub CLI stays a machine install
- No coverage threshold or gate exists anywhere
- No security, audit, signing, or compliance tooling exists
- No package publishes to a registry, the repository gets GitHub releases from tags, and MinVer reads the version from the tag
- Benchmarks stay
- Nx is the task runner and every check is one Nx target

## [00]-[REPOSITORY_FACTS]

Read from the repository root on 2026-09-03:

- No `.github/` directory exists, from the root listing
- `global.json` pins the .NET SDK at `10.0.400` with `rollForward: disable` and sets the test runner to `Microsoft.Testing.Platform`
- `Directory.Build.props` lines 34-36 set `RestorePackagesWithLockFile` true and gate `RestoreLockedMode` and `ContinuousIntegrationBuild` on `'$(CI)' == 'true'`
- `Directory.Build.props` lines 4 and 31-37 set `ArtifactsPath` to `.artifacts/dotnet`, `TreatWarningsAsErrors` and `MSBuildTreatWarningsAsErrors` true, `AnalysisLevel` `latest-all`, and `NuGetAudit` false
- `Directory.Build.props` lines 57-66 fall `RhinoAppPath` back to `/Applications/RhinoWIP.app` when no macOS Rhino bundle exists
- `NuGet.config` declares `nuget.org` and a `local` source at `.artifacts/nuget`, and `packageSourceMapping` sends `Rasm.*` to `local` and `*` to nuget.org
- `nx.json` lines 3-5 set the cache directory `.cache/nx/cache`, `neverConnectToCloud: true`, and `defaultBase: main`
- `nx.json` lines 64-93 register the plugins `@nx/vite`, `@nx/vitest`, `@nx/dotnet` (excluding `eng/native/**`), and the local `./tools/nx/native-packaging.ts`
- `eng/project.json` defines `eng:provision` as `uv run python -m eng.scripts.provision` with `cache: false` and `parallelism: false`
- `tools/nx/native-packaging.ts` lines 213-220 infer the `stage` target as `uv run python -m eng.scripts.stage <library>` with `cache: false`, `parallelism: false`, `dependsOn` `eng:provision`, and outputs `{workspaceRoot}/.artifacts/native/<library>/stage`
- `tools/nx/native-packaging.ts` lines 222-242 infer the `pack` target as `dotnet pack <root> --configuration Release --output <local source> --nologo` with `cache: true` and `dependsOn` the native project's `stage`
- `eng/scripts/provision.py` lines 55-59 and 279-283 clone vcpkg to `.cache/vcpkg` at commit `30ef65cad98f08e7197c9a1656fbd871bcb72f2d`, export the binary cache `.cache/vcpkg-archives` as `VCPKG_DEFAULT_BINARY_CACHE`, and build host tools in `.cache/vcpkg-hosttools`
- `eng/scripts/provision.py` lines 384-401: `provision.main` runs `dotnet tool restore` and fetches EnergyPlus 25.2.0, the DuckDB extensions, and the sqlite-vec loadable
- `.config/dotnet-tools.json` is committed (`git ls-files .config/`) and holds exactly one tool, `dotnet-stryker` 4.16.0
- `fd -H` over the repository finds no `mise.toml`, `.mise.toml`, `.config/mise.toml`, `mise.lock`, `.tool-versions`, `.python-version`, `.nvmrc`, or `.node-version`
- `eng/scripts/provision.py` line 20 lists the supported rids `osx-arm64`, `linux-x64`, `linux-arm64`, and `win-x64`
- `provision.py` lines 348-362 call `otool`, `install_name_tool`, and `codesign --force --sign -` during macOS staging, and `stage.py` line 244 calls `xcrun --sdk macosx` for emgucv
- `pyproject.toml` lines 4 and 377-383 set `requires-python = ">=3.15"`, `[tool.uv] default-groups = ["workspace", "dev"]`, and three `tool.uv.sources` entries that are a git rev or a URL archive
- `[tool.pytest]` sets `cache_dir = ".cache/pytest"` and an `addopts` with `-x`, `--disable-socket`, `--allow-unix-socket`, `--benchmark-storage=file://.artifacts/python/benchmarks`, `--benchmark-autosave`, and `-m "not benchmark"`
- `pyproject.toml` lines 637-640 set ruff `cache-dir = ".cache/ruff"`, `preview = true`, and `line-length = 150`
- `pnpm-workspace.yaml` lines 7-11 set `cacheDir: .cache/pnpm/cache`, `storeDir: .cache/pnpm/store`, and `minimumReleaseAge: 1440`
- `package.json` sets `engines.node >= 24.15.0` and `devEngines.packageManager` `pnpm >=11.9.0 <12` with `onFail: error`, and has no `packageManager` field
- `vitest.config.ts` lines 12, 58-61, and 102-145 switch on `process.env.CI === 'true'`: the reporters become `['dot','json','junit','github-actions','blob']`, `retry: 2`, `shuffle: true`, `allowOnly: false`, `hideSkippedTests: true`, and the outputs sit under `.artifacts/typescript/`
- `vitest.config.ts` lines 19 and 119-125 set `cacheDir: '.cache/vitest'` and coverage thresholds of 95 per file (removed by the fifth-round decision)
- `.gitignore` ignores `.cache/`, `**/.artifacts/`, and `.nx/*`
- `pnpm-workspace.yaml` lines 272-277 pin `nx`, `@nx/devkit`, `@nx/dotnet`, `@nx/js`, `@nx/vite`, and `@nx/vitest` at `23.1.3`
- Nx's newest release is `23.2.0`, published 2026-09-02 (<https://github.com/nrwl/nx/releases/tag/23.2.0>)

Consequences for every workflow:

1. `CI` is "Always set to `true`" on every GitHub Actions runner (<https://docs.github.com/en/actions/reference/workflows-and-actions/variables>), `RestoreLockedMode` and `ContinuousIntegrationBuild` turn themselves on, and no workflow flag is needed for either
2. Every cache and artifact path the repo uses is inside the workspace (`.cache/`, `.artifacts/`), and the NuGet global packages folder alone defaults outside it

## [01]-[FILE_KINDS]

GitHub reads these kinds of files from `.github/`:

| [INDEX] | [KIND]                                      | [PATH]                                                                       | [REFERENCE] |
| :-----: | :------------------------------------------ | :--------------------------------------------------------------------------- | :---------- |
|  [01]   | Workflow                                    | `.github/workflows/*.yml`, no subdirectories                                 | <https://docs.github.com/en/actions/reference/workflows-and-actions/workflow-syntax> |
|  [02]   | Composite action                            | `.github/actions/<name>/action.yml`                                          | <https://docs.github.com/en/actions/tutorials/create-actions/create-a-composite-action> |
|  [03]   | Reusable workflow                           | `.github/workflows/*.yml` with `on: workflow_call`                           | <https://docs.github.com/en/actions/how-tos/reuse-automations/reuse-workflows> |
|  [04]   | Code owners                                 | `.github/CODEOWNERS`, or the root or `docs/`                                 | <https://docs.github.com/en/repositories/managing-your-repositorys-settings-and-features/customizing-your-repository/about-code-owners> |
|  [05]   | Issue template (Markdown), issue form (YAML) | `.github/ISSUE_TEMPLATE/*.md`, `*.yml`                                      | <https://docs.github.com/en/communities/using-templates-to-encourage-useful-issues-and-pull-requests/configuring-issue-templates-for-your-repository> |
|  [06]   | Pull request template                       | `.github/pull_request_template.md`, or `.github/PULL_REQUEST_TEMPLATE/*.md` for more than one | <https://docs.github.com/en/communities/using-templates-to-encourage-useful-issues-and-pull-requests/creating-a-pull-request-template-for-your-repository> |
|  [07]   | Discussion category form                    | `.github/DISCUSSION_TEMPLATE/<category-slug>.yml`                            | <https://docs.github.com/en/discussions/managing-discussions-for-your-community/creating-discussion-category-forms> |
|  [08]   | `actionlint` config, third-party            | `.github/actionlint.yaml` or `.yml`                                          | <https://github.com/rhysd/actionlint/blob/v1.7.12/docs/config.md>, present in `prefix-dev/pixi` (`.yml`) and `grafana/grafana` (`.yaml`) |

Facts that constrain the layout:

- "Subdirectories of the `workflows` directory are not supported" (reuse-workflows page), and composite actions have no such rule and sit one directory deep under `.github/actions/`
- Reusable workflows are entire workflows called with `jobs.<id>.uses`, and composite actions collect "a series of workflow job steps into a single action" that runs inside a caller's job (composite-action tutorial, section "Composite actions and reusable workflows")
- "Issue forms are currently in public preview and subject to change" (issue template page, read 2026-09-03), and discussion category forms use the same GitHub form schema
- Pull request templates can live at the repository root or under `docs/`, and `CODEOWNERS` is searched in `.github/`, the root, then `docs/`

### [01.1]-[PLACEMENT]

- CI concerns: `.github/workflows/`, `.github/actions/`, and everything under the Actions documentation tree
- Repository hosting concerns: `CODEOWNERS`, `ISSUE_TEMPLATE/`, `pull_request_template.md`, and `DISCUSSION_TEMPLATE/`, which change no build. `CODEOWNERS` acts together with required reviews alone, which is gating and absent from Rasm
- Absent by decision: `dependabot.yml`, `FUNDING.yml`, `SECURITY.md`, `zizmor.yml`, `renovate.json5`, and every scanning workflow

## [02]-[WORKFLOW_DESIGN]

### [02.1]-[ACTION_VERSIONS]

Newest releases, read 2026-09-03:

| [INDEX] | [ACTION]                    | [RELEASE] | [DATE]     | [NOTES]                                                            |
| :-----: | :-------------------------- | :-------- | :--------- | :----------------------------------------------------------------- |
|  [01]   | `actions/checkout`          | `v7.0.1`  | 2026-07-20 | <https://github.com/actions/checkout/releases/tag/v7.0.1>          |
|  [02]   | `actions/cache`             | `v6.1.0`  | 2026-06-26 | "handle read-only cache access"                                    |
|  [03]   | `actions/upload-artifact`   | `v7.0.1`  | 2026-04-10 | <https://github.com/actions/upload-artifact/releases/tag/v7.0.1>   |
|  [04]   | `actions/download-artifact` | `v8.0.1`  | 2026-03-11 | <https://github.com/actions/download-artifact/releases/tag/v8.0.1> |
|  [05]   | `nrwl/nx-set-shas`          | `v5.0.1`  | 2026-03-20 | `runs.using: node24`                                               |
|  [06]   | `jdx/mise-action`           | `v4.3.0`  | 2026-08-25 | Adds `minimum_release_age`, `runs.using: node24`                   |

The mise decision removes these actions from the design, and their current releases are recorded against an older re-pin:

- `actions/setup-dotnet` v6.0.0 (2026-07-16)
- `actions/setup-node` v7.0.0 (2026-07-14)
- `astral-sh/setup-uv` v10.0.1 (2026-08-14)
- `pnpm/action-setup` v6.0.10 (2026-08-03), its release notes "point users to the successor pnpm/setup action"
- `pnpm/setup` v2.1.0 (2026-08-28)

Facts about them survive as design constraints: `actions/setup-dotnet` reads `global.json` when `dotnet-version` is absent, and `pnpm/setup` reads `devEngines.packageManager`. mise reads the same `global.json` (its `dotnet` registry entry declares `idiomatic_files = ["global.json"]`, <https://github.com/jdx/mise/blob/main/registry/dotnet.toml>), the SDK pin stays in one file.

### [02.2]-[RUNNER_LABELS]

Labels from `actions/runner-images` `README.md` on `main`, read 2026-09-03 (<https://github.com/actions/runner-images>):

| [INDEX] | [RID]         | [LABEL]                                                          | [IMAGE]                                     |
| :-----: | :------------ | :--------------------------------------------------------------- | :------------------------------------------ |
|  [01]   | `osx-arm64`   | `macos-26` or `macos-latest`                                    | macOS 26 Arm64                              |
|  [02]   | `linux-x64`   | `ubuntu-24.04` or `ubuntu-latest`                               | Ubuntu 24.04 x64                            |
|  [03]   | `linux-arm64` | `ubuntu-24.04-arm`                                               | Ubuntu 24.04 Arm64                          |
|  [04]   | `win-x64`     | `windows-2025`, `windows-latest`, or `windows-2025-vs2026`    | Windows Server 2025 with Visual Studio 2026 |

The README states the `-latest` migration "is gradual and happens over 1-2 months" and that "To avoid unwanted migration, users can specify a specific OS version in the yaml file". Builds that stage native binaries per rid use the explicit label, because a silent `macos-latest` bump changes the deployment target of every staged `.dylib`.

### [02.3]-[TRIGGERS]

Nx's GitHub Actions page (<https://nx.dev/docs/features/ci-features/github-integration>) shows the minimal trigger set, `push: branches: [main]` and `pull_request`.

- `nx.json` sets `defaultBase: main`, and `nx affected` compares against `main`
- `nrwl/nx-set-shas` sets `NX_BASE` from the last successful run of the same workflow on the main branch, its `last-successful-event` input defaults to `push`, and "if no successful workflow is found on the main branch to determine the SHA, we will log a warning and use HEAD~1" (`action.yml` at `refs/tags/v5.0.1`)
- `workflow_dispatch` makes a workflow runnable from the Actions tab without a push (<https://docs.github.com/en/actions/reference/workflows-and-actions/events-that-trigger-workflows>)
- A tag push (`push: tags: ['*']`) is the release trigger

### [02.4]-[CONCURRENCY]

"Use `concurrency` to ensure that only a single job or workflow using the same concurrency group will run at a time" (workflow syntax reference). The group for a monorepo is `${{ github.workflow }}-${{ github.ref }}` with `cancel-in-progress: ${{ github.event_name == 'pull_request' }}`, pull-request pushes cancel their predecessor while `main` runs finish. `main` runs finish because `nx-set-shas` reads the last successful `push` run on `main`, and a cancelled run is not a successful one. With `queue: max`, "up to 100 jobs or workflow runs can be queued per concurrency group" (<https://docs.github.com/en/actions/reference/limits>).

### [02.5]-[PERMISSIONS]

The CI workflow declares `contents: read` and `actions: read`, the pair Nx's own example workflow declares and that `nrwl/nx-set-shas` needs to read the last successful workflow run (<https://nx.dev/docs/features/ci-features/github-integration>). The release workflow alone adds `contents: write` for `gh release create`. No `packages: write` (no registry publishing), no `id-token`, and no `security-events` appear.

### [02.6]-[JOB_DEPENDENCIES]

`eng/scripts/stage.py` states in its module docstring (lines 1-5) that "Each CI host stages its own runtime identifier under `.artifacts/native/<library>/stage`, then one job collects the staged trees and runs the pack target alone." The Nx plugin encodes the same edge, `pack` `dependsOn` the native project's `stage` (`tools/nx/native-packaging.ts` line 225).

Lines that block the four-rid matrix, and the CI work corrects them because the docstring commits to per-host staging: `_ENERGYPLUS_ASSETS` pins an asset for `osx-arm64` only (`provision.py` lines 63-65), and `eng:provision` as written fails on a Linux or Windows runner at `SystemExit(f"no EnergyPlus {_ENERGYPLUS_VERSION} asset pinned for {rid}")`. `_stage_emgucv` raises `SystemExit` for any rid other than `osx-arm64` (`stage.py` lines 260-263), and its build calls `xcrun`. Provisioning becomes rid-conditional (EnergyPlus pinned per rid, emgucv staged on macOS only), and the matrix then runs every rid the staging code supports.

### [02.7]-[ARTIFACTS]

`actions/upload-artifact` README at `refs/tags/v7.0.1`: "Artifact names must be unique since each created artifact is idempotent so multiple jobs cannot modify the same artifact", and matrix jobs "name the artifact with a prefix or suffix from the matrix".

Breaking changes across the current majors, from the release notes:

- upload v5.0.0 (2025-10-24): Node 24 support, "not a breaking change per-se but we're treating it as such", `@actions/artifact` v4.0.0
- upload v6.0.0 (2025-12-12): `runs.using: node24`, "requires a minimum Actions Runner version of 2.327.1"
- upload v7.0.0 (2026-02-26): `archive: false` uploads a single file unzipped, the `name` input is ignored and the file name becomes the artifact name, "The action will fail if the glob passed resolves to multiple files", ESM
- download v5.0.0 (2025-08-05): breaking, a single artifact downloaded by ID extracts to `path/` instead of `path/<artifact-name>/`
- download v6.0.0 (2025-10-24): Node 24 support, `@actions/artifact` v4.0.0
- download v7.0.0 (2025-12-12): `runs.using: node24`, minimum runner 2.327.1
- download v8.0.0 (2026-02-26): breaking, "Hash mismatches will now error by default" (`digest-mismatch` input), no longer unzips non-zipped downloads, checks `Content-Type` first, new `skip-decompress` input, ESM

Limits from the upload README that bear on staged native trees:

- "File permissions are not maintained during zipped artifact upload. All directories will have `755` and all files will have `644`." The README's remedy is to `tar` the tree and upload the tarball with `archive: false`
- "Within an individual job, there is a limit of 500 artifacts that can be created for that job."

The permission point is load-bearing: `stage_closure` sets `0o755` on every staged library (`provision.py` line 374) and `_relink` re-signs each `.dylib` with `codesign --force --sign -` (line 362). Staged native trees cross the job boundary as a tarball.

### [02.8]-[TIMEOUTS]

- `jobs.<job_id>.timeout-minutes`: "The maximum number of minutes to let a job run before GitHub automatically cancels it. Default: 360" (`content/actions/reference/workflows-and-actions/workflow-syntax.md` in `github/docs`)
- `jobs.<job_id>.strategy.fail-fast` "applies to the entire matrix", and when true GitHub "will cancel all in-progress and queued jobs in the matrix if any job in the matrix fails" (`data/reusables/actions/jobs/section-using-a-build-matrix-for-your-jobs-failfast.md`)
- GitHub-hosted jobs run for at most 6 hours, and a job matrix generates at most 256 jobs per workflow run (<https://docs.github.com/en/actions/reference/limits>)

Every job sets `timeout-minutes`, the 360-minute default fits nothing here but a cold vcpkg build. `fail-fast: false` on the native-staging matrix keeps the other legs producing their trees and logs when one port breaks, because the legs are not interchangeable.

## [03]-[CACHING]

### [03.1]-[CACHE_MECHANICS]

From <https://docs.github.com/en/actions/reference/workflows-and-actions/dependency-caching>:

- Restore order is the exact `key`, then partial matches of `key`, then each `restore-keys` entry in order, and on a partial match "the most recent cache is restored"
- "You cannot change the contents of an existing cache. Instead, you can create a new cache with a new key."
- Keys have a maximum length of 512 characters
- `cache-hit` is true on an exact key match alone
- `enableCrossOsArchive` defaults to `false` and stays there, a `.cache/vcpkg-archives` tree is not portable across OSes
- "By default, the limit is 10 GB per repository, but this limit can be increased by enterprise owners, organization owners, or repository administrators."
- Caches "not been accessed in over 7 days" are removed, and over the limit "the eviction policy will create space by deleting the caches in order of last access date, from oldest to most recent"
- Scope: a run restores caches from its own branch, the default branch, and (for a pull request) the base branch, it cannot restore from child or sibling branches or other tags, and a pull-request cache is scoped to the merge ref and "can only be restored by re-runs of the pull request"
- "We recommend that you don't store any sensitive information, such as access tokens or login credentials, in files in the cache path."

The 10 GB ceiling is the binding constraint. A cold vcpkg tree with ffmpeg, gmsh, z3, blosc2, and lcms2 across four triplets, a NuGet global-packages folder for `Directory.Packages.props` (160 KB of package entries), and a pnpm store for the dependency list in `package.json` will not all fit when every branch keeps its own copy. Keys make the `main` copy the one every branch restores from.

### [03.2]-[TOOLCHAIN_AND_PACKAGE_CACHES]

mise installs the toolchain. `jdx/mise-action@v4.3.0` caches the mise data directory (`cache` default `true`, `cache_save`, `cache_key_prefix` default `mise-v1`, `cache_key` template), and its `install_args` description states "When a repo mise lock file is present, the action automatically adds `--locked` unless you already provided it" (`action.yml` at `refs/tags/v4.3.0`). mise's CI page recommends "pinning tools to specific versions so the environment is reproducible" and runs commands through `mise x --` (<https://mise.jdx.dev/continuous-integration.html>). The action's `wings_enabled` input "Requires an active mise-wings subscription" and `permissions: id-token: write`, it defaults to `false` and stays off.

The package caches are three `actions/cache` entries, because no setup action owns them once mise installs the tools:

| [INDEX] | [CONCERN]                     | [PATH]                                                                | [KEY_SOURCE]            |
| :-----: | :---------------------------- | :-------------------------------------------------------------------- | :---------------------- |
|  [01]   | NuGet global packages         | `NUGET_PACKAGES` set inside the workspace                             | `**/packages.lock.json` |
|  [02]   | uv cache                      | `UV_CACHE_DIR` set inside the workspace, `uv cache prune --ci` before the save | `uv.lock`      |
|  [03]   | pnpm store and metadata cache | `.cache/pnpm/store` and `.cache/pnpm/cache` (`pnpm-workspace.yaml`)   | `pnpm-lock.yaml`        |

`uv cache prune --help` describes `--ci` as "Optimize the cache for persistence in a continuous integration environment, like GitHub Actions".

pnpm's CI page names the metadata cache as the entry that pays: "Since v11.22.0, `pnpm cache path` prints the directory pnpm uses for its metadata cache... That directory also holds the lockfile verification log, which lets a job skip re-checking an unchanged lockfile against the configured supply-chain policies" (<https://pnpm.io/continuous-integration>), and warns "Only cache pnpm's store and cache directories in locations writable by trusted jobs." Rasm sets both directories in `pnpm-workspace.yaml`, no `pnpm cache path` lookup is needed.

`NUGET_PACKAGES` names the global packages folder, and "The environment variable takes precedence over the configuration setting" (<https://learn.microsoft.com/en-us/nuget/consume-packages/managing-the-global-packages-and-cache-folders>). Pointing it inside the workspace keeps every cache under `.cache/`.

### [03.3]-[UV_CACHE]

The uv GitHub guide (<https://docs.astral.sh/uv/guides/integration/github/>, dated 2026-09-01) documents both the action cache and the manual route, and the manual route ends with `uv cache prune --ci`. `UV_CACHE_DIR` is the variable it names. The three `[tool.uv.sources]` entries (beartype, connectrpc, protoc-gen-connectrpc) are fetched over the network on a cold cache, the uv cache is worth more here than in a wheels-only project.

### [03.4]-[VCPKG_BINARY_CACHE]

The `x-gha` provider no longer exists. The vcpkg binary-caching reference lists `x-gha` as "Removed: This feature has been removed from vcpkg" (<https://learn.microsoft.com/en-us/vcpkg/reference/binarycaching>). `microsoft/vcpkg-tool` PR #1662 (merged 2025-04-29) removed it because "Recent changes in GitHub Actions Cache's API have rendered the `x-gha` binary caching provider useless", and GitHub's engineers stated their internal APIs "were not intended to be consumed the way our program does" and the "Only intended method to consume the service is through actions/cache" (<https://github.com/microsoft/vcpkg-tool/pull/1662>). "What's New in vcpkg (June 2025)" announces the removal (<https://devblogs.microsoft.com/cppblog/whats-new-in-vcpkg-june-2025/>).

The PR names the alternatives: GitHub Packages with NuGet (needs a `packages: write` token and a hosted feed, out of scope) and `actions/cache` over the binary-cache directory, "not integrated with vcpkg but is the method endorsed by the GitHub Actions team. The downside of this method is that the whole installed tree gets cached as a single artifact, so single port updates invalidate all packages."

Rasm is shaped for the second: `provision.native_build_tools` sets `VCPKG_DEFAULT_BINARY_CACHE` to `.cache/vcpkg-archives` (`provision.py` lines 58, 283), and `VCPKG_DEFAULT_BINARY_CACHE` "redirects the default location to store binary packages" (<https://learn.microsoft.com/en-us/vcpkg/users/config-environment>). The default files provider writes per-port archives there, caching that directory gives per-port reuse inside a cache entry, and the entry alone is monolithic.

Other vcpkg directories:

- `.cache/vcpkg`, the checkout pinned to commit `30ef65cad98f08e7197c9a1656fbd871bcb72f2d` and bootstrapped into a `vcpkg` binary (`provision.py` lines 128-141), keyed on that constant, the cheapest and most stable cache in the workspace
- `.cache/vcpkg-hosttools`, the pkgconf host tool built once per host triplet (`provision.py` lines 144-168)
- `.cache/vcpkg/downloads`, source tarballs. `stage.py` `_source_root` reads `vcpkg.parent / "downloads"` and re-runs `vcpkg install --only-downloads` when a binary-cache hit fetched no source (`stage.py` lines 171-180). `provision.py` never sets `VCPKG_DOWNLOADS`, vcpkg uses "the internal `downloads/` directory" (<https://learn.microsoft.com/en-us/vcpkg/users/config-environment>), nested inside the checkout. Two overlapping entries over `.cache/vcpkg` and `.cache/vcpkg/downloads` save the same bytes twice against the quota. `native_build_tools` sets `VCPKG_DOWNLOADS` to a sibling directory (`VCPKG_DOWNLOADS` "should always be set to an absolute path"), a one-line change, and the two caches become separate

### [03.5]-[KEY_DESIGN]

Every key names the runner, the tool, and the file with the content that decides the answer:

- NuGet global packages: path `NUGET_PACKAGES`, key `nuget-${{ runner.os }}-${{ hashFiles('**/packages.lock.json') }}`, restore key `nuget-${{ runner.os }}-`
- uv: path `UV_CACHE_DIR`, key `uv-${{ runner.os }}-${{ hashFiles('uv.lock') }}`, restore key `uv-${{ runner.os }}-`
- pnpm store and metadata: paths `.cache/pnpm/store` and `.cache/pnpm/cache`, key `pnpm-${{ runner.os }}-${{ hashFiles('pnpm-lock.yaml') }}`, restore key `pnpm-${{ runner.os }}-`
- vcpkg checkout and bootstrapped binary: path `.cache/vcpkg`, key `vcpkg-tree-${{ runner.os }}-<the pinned commit>`, no restore key
- vcpkg binary archives: path `.cache/vcpkg-archives`, key `vcpkg-bin-${{ runner.os }}-${{ matrix.rid }}-${{ hashFiles('eng/native/*/vcpkg.json') }}`, restore key `vcpkg-bin-${{ runner.os }}-${{ matrix.rid }}-`
- vcpkg source downloads: path the `VCPKG_DOWNLOADS` directory, key `vcpkg-dl-${{ runner.os }}-${{ hashFiles('eng/native/*/vcpkg.json') }}`, restore key `vcpkg-dl-${{ runner.os }}-`
- vcpkg host tools: path `.cache/vcpkg-hosttools`, key `vcpkg-host-${{ runner.os }}-<the pinned commit>`, no restore key
- pinned release archives: paths `.cache/duckdb-extensions`, `.cache/sqlite-vec`, and `.cache/energyplus`, key `pins-${{ runner.os }}-${{ hashFiles('eng/native/*/extensions.json', 'eng/native/*/loadable.json', 'eng/scripts/provision.py') }}`, restore key `pins-${{ runner.os }}-`

Notes from the repo:

- `eng/native/*/vcpkg.json` holds `builtin-baseline`, and `provision.py` line 56 asserts the pinned commit "Equals the builtin-baseline in eng/native/lcms2/vcpkg.json", both belong in the vcpkg key
- The pinned-release key includes `provision.py` because the EnergyPlus version and digest are constants in that file (lines 62-65) and in no manifest
- `.artifacts/nuget`, the `local` NuGet source, is a build output where `pack` writes and where restore reads `Rasm.*` from, it crosses jobs as an artifact

### [03.6]-[NX_LOCAL_CACHE]

Nx refuses a cache made elsewhere: "when artifacts in the local cache are created by a different machine, we cannot make such assumption. By default, Nx will refuse to use such artifacts and will throw the 'Invalid Cache Directory' error or 'Unrecognized Cache Artifacts' error" (<https://nx.dev/docs/troubleshooting/unknown-local-cache>). `NX_REJECT_UNKNOWN_LOCAL_CACHE=0` applies "When using the legacy file system cache (deprecated in Nx 20)" alone and "this approach is discouraged". Nx on value: "A local cache only helps one machine. CI machines are usually ephemeral, so a local cache alone does almost nothing for pipeline times: every run starts cold" (<https://nx.dev/docs/kb/ci-caching>).

`nx affected` keeps CI small here, and the task cache does not. It needs `fetch-depth: 0` on checkout and `NX_BASE`/`NX_HEAD` (<https://nx.dev/docs/features/ci-features/github-integration>).

### [03.7]-[EXCLUDED_PATHS]

Paths no cache entry holds:

| [INDEX] | [PATH]                              | [REASON]                                                                                        |
| :-----: | :---------------------------------- | :---------------------------------------------------------------------------------------------- |
|  [01]   | `.cache/nx/cache`                   | Nx rejects artifacts from another machine                                                       |
|  [02]   | `node_modules`                      | pnpm's store is the cache, a restored tree bypasses the `--frozen-lockfile` check               |
|  [03]   | `.venv`                             | uv's cache is `UV_CACHE_DIR`, a restored `.venv` bypasses `uv sync --locked`                    |
|  [04]   | `.artifacts/**`                     | Build outputs, `.artifacts/nuget` and `.artifacts/native/*/stage` cross jobs as artifacts       |
|  [05]   | `obj/`, `bin/`, `.artifacts/dotnet` | MSBuild incrementality is timestamp-based, a restored `obj` from another runner skips wrongly   |
|  [06]   | Lock files                          | Inputs under version control, a cache that can supply them defeats `--locked-mode`              |
|  [07]   | Anything holding credentials        | GitHub cache reference                                                                          |
|  [08]   | Installed SDKs                      | mise installs them from `mise.toml` and `mise.lock`, a cached SDK tree can diverge from the pin |

### [03.8]-[KEY_COLLISIONS]

1. Too-loose `restore-keys`: a prefix like `vcpkg-` with no rid segment lets a `linux-x64` run restore an `osx-arm64` archive tree, vcpkg misses and rebuilds, and the entry counts against the quota and pushes useful entries out under oldest-first eviction
2. Same key, different content: because "You cannot change the contents of an existing cache", the first job to save under a key wins. Two matrix legs that compute the same key but hold different directories leave one leg reading the other's data. Every matrix-scoped cache holds the matrix value in the key, as `upload-artifact` needs it in the artifact name

## [04]-[DOTNET]

### [04.1]-[RESTORE]

From <https://learn.microsoft.com/en-us/dotnet/core/tools/dotnet-restore>:

- `--locked-mode`: "Don't allow updating project lock file."
- `--use-lock-file`: "Enables project lock file to be generated and used with restore."
- `--force-evaluate`: "Forces restore to reevaluate all dependencies even if a lock file already exists." The flag breaks the guarantee and never appears in CI
- `--packages <PACKAGES_DIRECTORY>`: "Specifies the directory for restored packages", an alternative to `NUGET_PACKAGES`
- `dotnet restore` runs implicitly for `dotnet new`, `build`, `build-server`, `run`, `test`, `publish`, and `pack`, and "To prevent the implicit NuGet restore, you can use the `--no-restore` flag with any of these commands."

Passing `--locked-mode` on the command line beside the property is harmless and makes the contract visible. `README.md` states the ordering: "Inferred `build` targets pass `--no-restore`, `dotnet restore Workspace.slnx` precedes `nx affected -t build test`."

`NuGetAudit` is `false` (`Directory.Build.props` line 37), matching the decision. The same page records the default that this switches off: "In .NET 8 and .NET 9, only *direct* package references are audited by default. Starting in .NET 10, NuGet audits both *direct* and *transitive* package references by default."

### [04.2]-[ORDER]

1. `dotnet tool restore`, the manifest holds the MCP servers, Stryker, ReportGenerator, and binlogtool once the manifest decision lands
2. `dotnet restore Workspace.slnx --locked-mode`, one restore for the whole solution
3. `nx affected -t build`, the `@nx/dotnet` inferred `build` passes `--no-restore`
4. `nx affected -t test`

`--artifacts-path` "must be explicitly cascaded in any `dotnet` command that depends on the output of another `dotnet` command", and Rasm sets `ArtifactsPath` in `Directory.Build.props`, which every invocation reads, no cascading flag is needed.

### [04.3]-[CONTINUOUS_INTEGRATION_BUILD]

"The `ContinuousIntegrationBuild` property indicates whether a build is executing on a continuous integration (CI) server. When set to `true`, this property enables settings that only apply to official builds as opposed to local builds on a developer machine. For example, stored file paths are normalized for official builds" (<https://learn.microsoft.com/en-us/dotnet/core/project-sdk/msbuild-props#continuousintegrationbuild>). Rasm conditions on `CI`, which Actions sets, and no change is needed.

### [04.4]-[RELEASE_VERSION]

MinVer is a PackageReference in the root `Directory.Build.props` with `PrivateAssets="All"` and never a tool. From its README (<https://github.com/adamralph/minver>):

- "Your project will be versioned according to the latest tag found in the commit history"
- A tag (`git tag 1.2.3`) on the released commit is the version, and a commit without a version tag gets the next patch, `alpha.0`, and the commit height
- It sets `AssemblyVersion`, `FileVersion`, `InformationalVersion`, `PackageVersion`, and `Version`
- `MinVerTagPrefix` handles a `v` prefix
- "To build with GitHub Actions, set the fetch depth appropriately", the same `fetch-depth: 0` the affected computation needs

The latest release is 7.0.0 (2026-01-05, nuget.org). The `eng/native` subtree does not import the root `Directory.Build.props` (`eng/native/Directory.Build.props` line 2), its manifest-derived `Version` checks are untouched.

### [04.5]-[CONSOLE_AND_PROCESS_SWITCHES]

From the MSBuild command-line reference (`docs/msbuild/msbuild-command-line-reference.md`, `ms.date` 2026-04-03):

- `-terminalLogger[:auto|on|off]` / `-tl`: "Specify `auto` (or use the option without arguments) to use the terminal logger only if the standard output is not redirected. Don't parse the output or otherwise rely on it remaining unchanged in future versions." On a runner stdout is redirected, `auto` already selects the console logger, and `-tl:off` states the choice
- `-nodeReuse:{value}` / `-nr`: "True. Nodes remain after the build finishes so that subsequent builds can use them (default). False. Nodes don't remain after the build completes." The MSBuild tips page adds "Don't leave MSBuild.exe processes hanging around (and possibly locking files) after the build completes... Note that using this when building repeatedly will cause slower builds" and names `MSBUILDDISABLENODEREUSE=1` as the environment form (<https://github.com/dotnet/msbuild/blob/main/documentation/wiki/MSBuild-Tips-%26-Tricks.md>), which fits a workflow-level `env:` block
- `-maxCpuCount` / `-m`: "If you don't include this switch, the default value is 1. If you include this switch without specifying a value, MSBuild uses up to the number of processors in the computer."
- `-binaryLogger` / `-bl`: "Serializes all build events to a compressed binary file. By default the file is in the current directory and named *msbuild.binlog*... A binary log is usually 10-20x smaller than the most detailed text diagnostic-level log, but it contains more information." `ProjectImports` defaults to `Embed`, and `-bl:output.binlog;ProjectImports=None` omits the imports

`CLAUDE.md` routes all `.binlog` work through the `dotnet-msbuild-diagnostics` skill, the workflow produces the file and uploads it on failure: `-bl:.artifacts/dotnet/build.binlog` on the build, then `actions/upload-artifact@v7` with `if: failure()`.

### [04.6]-[BUILD_SERVERS]

- `dotnet build-server shutdown` "Shuts down build servers that are started from dotnet. By default, all servers are shut down", with `--msbuild`, `--razor`, `--vbcscompiler` (<https://learn.microsoft.com/en-us/dotnet/core/tools/dotnet-build-server>)
- MSBuild Server is off by default, enabled by `DOTNET_CLI_USE_MSBUILD_SERVER`, and "is generally not helpful in CI scenarios such as Azure Pipeline builds, because pipelines typically stand up a build environment on demand for each build and then dispose of it when the build is completed" (<https://learn.microsoft.com/en-us/visualstudio/msbuild/msbuild-server>)

On hosted runners nothing is enabled and nothing needs shutting down. A shutdown step earns its place when a later step in the same job needs a file a lingering compiler node holds open, on Windows the staged `.dll` closure.

### [04.7]-[ENVIRONMENT_VARIABLES]

From <https://learn.microsoft.com/en-us/dotnet/core/tools/dotnet-environment-variables> (`ms.date` 2026-05-15):

- `DOTNET_CLI_TELEMETRY_OPTOUT`: "Set to `true` to opt-out of the telemetry feature (values `true`, `1`, or `yes` accepted)... If not set, the default is `false` and the telemetry feature is active."
- `DOTNET_NOLOGO`: "Set to `true` to mute these messages... This flag does not affect telemetry."
- `NUGET_PACKAGES`: "The global packages folder. If not set, it defaults to `~/.nuget/packages` on Unix or `%userprofile%\.nuget\packages` on Windows."
- `DOTNET_SKIP_FIRST_TIME_EXPERIENCE`: "This environment variable is no longer supported in .NET Core 3.0 and later. Use `DOTNET_NOLOGO` as a replacement." Dead, and never copied in
- `DOTNET_CLI_UI_LANGUAGE`: "Sets the language of the CLI UI using a locale value such as `en-us`." Pinning it keeps diagnostic text stable across runner image changes
- `DOTNET_CLI_WORKLOAD_UPDATE_NOTIFY_DISABLE`: "Disables background download of advertising manifests for workloads. Default is `false`." `dotnet restore` "initiates an asynchronous background download of advertising manifests for workloads"
- `DOTNET_SDK_VULNERABILITY_CHECK_DISABLE`: "Disables the opt-in SDK vulnerability, end-of-life, and feature-band discontinuation check... The default is `false`." It can emit NETSDK1238, NETSDK1239, NETSDK1240, which `TreatWarningsAsErrors` turns into failures

Where each variable belongs follows the installer decision: shell and tool discovery in `mise.toml` `[env]`, task runtime variables as Nx `.env` inputs, and CI-only values in the workflow `env:` block. No variable is declared in two places.

### [04.8]-[TEST_MODE]

`global.json` declares `"test": { "runner": "Microsoft.Testing.Platform" }`, which enables MTP mode (<https://learn.microsoft.com/en-us/dotnet/core/testing/microsoft-testing-platform-integration-dotnet-test>). Consequences:

- "Since this mode is specifically designed for MTP, neither `TestingPlatformDotnetTestSupport` nor the additional `--` are required."
- "If passing a specific solution (or directory containing solution), for example, `dotnet test MySolution.sln`, this should become `dotnet test --solution MySolution.sln`", and likewise `--project` for a project and `--test-modules` for a dll (which "also supports globbing"). A workflow that writes `dotnet test Workspace.slnx` is written for the old mode
- "This mode is only compatible with MTP version 1.7.0 and later."
- "If your test project supports VSTest but does not support MTP, an error will be generated."
- "The separator avoids a parser quirk where unrecognized arguments change meaning when interleaved with options that `dotnet test` understands."

### [04.9]-[TRX_REPORTING]

"The `--report-trx` option isn't built into MTP. Each targeted test application must register the extension by referencing `Microsoft.Testing.Extensions.TrxReport` directly or through a test SDK configuration or profile that includes the package. Otherwise, the test application rejects the option with MTP exit code 5." In a solution with mixed frameworks or extension sets, "Options that are valid for one project are unrecognized by another, causing exit code 5 (invalid command-line arguments)", and the fix is "the `TestingPlatformCommandLineArguments` MSBuild property with conditions". `Microsoft.Testing.Extensions.TrxReport` 2.3.3 is pinned at `Directory.Packages.props` line 773, and the reference belongs in every test project, per `CLAUDE.md`'s direct-reference rule.

### [04.10]-[EXIT_CODES]

From <https://learn.microsoft.com/en-us/dotnet/core/testing/microsoft-testing-platform-exit-codes>: `0` success, `1` unknown errors, `2` at least one test failure, `3` session aborted, `4` invalid extension setup, `5` invalid command-line arguments, `6` no longer produced, `7` session unable to complete (likely crashed), `8` ran zero tests, `9` minimum execution policy violated, `10` test adapter infrastructure failure, `11` dependent process exited, `12` unsupported protocol version, `13` stopped by `--maximum-failed-tests`.

Exit code `8` is the one a monorepo hits by accident: a selection that matches nothing fails the job, which matches `vitest.config.ts`'s `passWithNoTests: false`. `--ignore-exit-code` (or `TESTINGPLATFORM_EXITCODE_IGNORE`) never appears, and "A common scenario is to consider that test failures shouldn't result in a nonzero exit code (which corresponds to ignoring exit-code `2`)" is the misuse the page itself describes. Diagnostic logging is `--diagnostic` with `--diagnostic-output-directory`, or `TESTINGPLATFORM_DIAGNOSTIC=1` and `TESTINGPLATFORM_DIAGNOSTIC_OUTPUT_DIRECTORY`, and "Environment variables take precedence over the command line arguments."

### [04.11]-[NATIVE_MATRIX]

| [INDEX] | [LIBRARY]                                 | [MECHANISM]                         | [CROSS_RID]                                         |
| :-----: | :---------------------------------------- | :---------------------------------- | :-------------------------------------------------- |
|  [01]   | `blosc2`, `ffmpeg`, `gmsh`, `lcms2`, `z3` | vcpkg build with a per-rid triplet  | Yes, `vcpkg_target` handles `win`, `osx`, and Linux |
|  [02]   | `duckdbextensions`                        | Pinned download per DuckDB platform | Yes, `_DUCKDB_PLATFORMS` covers all four            |
|  [03]   | `sqlitevec`                               | Pinned download per platform        | Yes, `_SQLITE_VEC_PLATFORMS` covers all four        |
|  [04]   | `emgucv`                                  | Source build                        | No, `_stage_emgucv` raises `SystemExit`             |

The emgucv message is `SystemExit(f"emgucv builds osx-arm64 only, not {rid}")`.

## [05]-[PYTHON]

### [05.1]-[INTERPRETER]

`requires-python = ">=3.15"` names a version in release candidate. PEP 790: "3.15.0 candidate 2: Tuesday, 2026-09-01" and "3.15.0 final: Thursday, 2026-10-01" (<https://peps.python.org/pep-0790/>). The release manager's post of 2026-09-01 confirms rc2 is "the second and final Python 3.15 release candidate" (<https://hugovk.dev/blog/2026/help-test-python-315/>).

mise pins the interpreter in `mise.toml`, the same file the developer machine reads, CI and the machine resolve the same build, and `mise.lock` records it. That build is 3.15.0rc2 until 2026-10-01, then 3.15.0. The repo holds evidence of the pre-release: `[tool.coverage.run] core = "sysmon"` with the comment "CPython 3.15 beta lacks coverage's compiled C tracer", and `[tool.uv.sources] beartype` pinned to a git rev because "0.22.9 imports typing.no_type_check_decorator (removed in 3.15)". No `.python-version` file exists and none is added, `mise.toml` is the pin.

### [05.2]-[INSTALL_AND_SYNC]

The uv GitHub guide (dated 2026-09-01) gives the sequence install uv, install Python, `uv sync --locked`, then `uv run`. With mise supplying uv and Python, the job runs `uv sync --locked` then `uv run`.

- `uv sync --locked`: "Assert that the `uv.lock` will remain unchanged [env: UV_LOCKED=]" (`uv sync --help`, uv 0.12.5), the Python counterpart of `--locked-mode` and `--frozen-lockfile`
- `--frozen`: "Sync without updating the `uv.lock` file", it installs from a stale lockfile without complaint, and CI wants `--locked`
- `[tool.uv] default-groups = ["workspace", "dev"]`, a bare `uv sync --locked` installs both, `--all-groups` includes every group, and `--no-dev` disables the `dev` group
- `uv run --no-sync`: "Avoid syncing the virtual environment [env: UV_NO_SYNC=]". After one `uv sync --locked`, every later `uv run` passes `--no-sync` and no step re-resolves

### [05.3]-[NX_TARGETS]

`CLAUDE.md` and `README.md` require `uv run`, Nx encodes it (`eng:provision` and every `stage` target run `uv run python -m eng.scripts.<module>`), and the fifth-round decision makes each checker one Nx target declared once in `targetDefaults` with check and write configurations, CI and a developer run the identical target:

| [INDEX] | [CHECK]      | [COMMAND]                      | [CONFIGURATION]                                                  |
| :-----: | :----------- | :----------------------------- | :--------------------------------------------------------------- |
|  [01]   | Lint         | `uv run ruff check .`          | `[tool.ruff]`                                                    |
|  [02]   | Format check | `uv run ruff format --check .` | `[tool.ruff.format]`                                             |
|  [03]   | Types (ty)   | `uv run ty check`              | `[tool.ty.environment]`, `[tool.ty.rules]`, `[tool.ty.terminal]` |
|  [04]   | Types (mypy) | `uv run mypy`                  | `[tool.mypy]`, `[tool.pydantic-mypy]`                            |
|  [05]   | Tests        | `uv run pytest`                | `[tool.pytest]`                                                  |

`README.md` requires ruff, ty, and mypy to pass with no warnings or errors, they are separate targets and never one `&&` chain, a chained step reports one failure and hides the rest.

### [05.4]-[PYTEST_AND_COVERAGE]

Facts in `pyproject.toml` the workflow respects:

- `addopts` contains `-x`, and CI runs the same target with the same options, there is one set of checks and no CI variation
- `--disable-socket` and `--allow-unix-socket`, and tests needing real network carry the `network` marker (`tests/README.md`)
- `-m "not benchmark"`, `--benchmark-storage=file://.artifacts/python/benchmarks`, and `--benchmark-autosave`, and `filterwarnings` suppresses the autosave no-op warning. Benchmarks run under their own target and marker, outside the functional session, as `tests/README.md` classifies them
- `required_plugins` lists eight plugins including `pytest-xdist`, and the file's own comment forbids `-n` in the default `addopts` because `pytest-benchmark` and `pytest-xdist` conflict under `filterwarnings = ["error"]`
- `filterwarnings = ["error", ...]` turns warnings into failures, a new runner image with a newer transitive dependency can fail CI on a `DeprecationWarning`, the run must be reproducible from the lockfile
- `[tool.coverage.run] data_file = ".cache/coverage/.coverage"`, `core = "sysmon"`, `patch = ["subprocess"]`, `relative_files = true`, and JSON, HTML, XML, and LCOV outputs under `.artifacts/python/coverage/`. `fail_under = 90` is removed by the fifth-round decision, coverage is one LCOV report per language, information about what is tested, with no threshold

The whole Python job: `uv sync --locked`, the five targets, upload `.artifacts/python/`.

## [06]-[TYPESCRIPT]

### [06.1]-[COREPACK]

The Node.js TSC vote of 2025-03-19 decided to "stop distributing Corepack (i.e. the distribution will no longer contain a corepack executable) on future (i.e. 25+) release lines of Node.js" (quoted in <https://github.com/nodejs/node/pull/61207>, merged 2026-01-01). The Corepack README: "Corepack is distributed with Node.js from version 14.19.0 up to (but not including) 25.0.0" (<https://github.com/nodejs/corepack>). pnpm's CI page: "Earlier versions of this page used Corepack. Corepack installs a JavaScript shim in place of pnpm, so every `pnpm` call starts Node.js to run the shim before pnpm itself starts... Installing pnpm itself avoids that entirely" (<https://pnpm.io/continuous-integration>).

On the `devEngines.packageManager` field, Yarn's maintainer: "I have no interest supporting this field in Yarn or any Yarn-derived tool right now" (<https://github.com/nodejs/corepack/issues/687>). mise installs pnpm on the runner, and `mise.toml` pins the same version the `devEngines` range accepts.

### [06.2]-[INSTALL]

"In a CI environment, installation fails if a lockfile is present but needs an update" (<https://pnpm.io/cli/install>). "When pnpm detects that it is running in CI, it switches to frozen-lockfile mode automatically. Since v11, pnpm also fails on incompatible lockfiles in CI" (CI page). `--frozen-lockfile` is redundant on Actions and written anyway, the contract is visible.

The lock records `@pnpm/exe@11.24.0` and `devEngines.packageManager` allows `>=11.9.0 <12`, the runner's pnpm must sit in that range.

- `--no-runtime` (v11.1.0+): "Skip installing runtime entries (e.g. Node.js downloaded via devEngines.runtime). The lockfile is left untouched, so frozen installs still validate... useful in CI matrices where the runtime is provisioned externally." mise provisions the runtime, the install passes `--no-runtime`
- `--update-checksums` (v11.4.0+) exists because "since v11.4.0, an integrity mismatch is a hard failure: `pnpm install` exits with `ERR_PNPM_TARBALL_INTEGRITY`". It never appears in CI

`minimumReleaseAge: 1440` affects resolution and not install from a lockfile.

### [06.3]-[STORE_CACHING]

pnpm's CI page: "In all the provided configuration files the store is cached. However, this is not required, and it is not guaranteed that caching the store will make installation faster." The metadata cache is the entry that pays. One `actions/cache` entry covers both directories, and a second mechanism over the same store produces the same-key collision.

### [06.4]-[CHECKS]

- Lint and format: `pnpm biome ci .`. "Compared to the `check` command, the `ci` command: Doesn't provide any `--write`/`--fix` option. Integrates better with specific runners. For example, when run on GitHub, the diagnostics are printed using the GitHub annotations. Allows controlling the number of threads." (<https://biomejs.dev/recipes/continuous-integration/>)
- Types: `pnpm tsc --build`, the `nx.json` `typecheck` target is `tsc --build --pretty false` with `cwd: {projectRoot}`, cached, `dependsOn: ["^typecheck"]`
- Tests: `nx affected -t test`, the `nx.json` `@nx/vitest` plugin with `testTargetName: test`

Biome is a workspace `devDependency` (`@biomejs/biome` 2.5.11 in the catalog), the workspace copy runs and the lockfile fixes its version. `--pretty false` gives plain diagnostics GitHub's log viewer can read.

### [06.5]-[VITEST_REPORTERS]

`vitest.config.ts` maps `json`, `junit`, and `blob` through `outputFile` into `.artifacts/typescript/test-results`. From <https://vitest.dev/guide/reporters>:

- GitHub Actions reporter: "Output workflow commands to provide annotations for test failures." The documented condition is `process.env.GITHUB_ACTIONS === 'true'`, and Rasm gates on `CI`, which constructs the reporter on any other CI system, harmlessly
- Blob reporter: "Stores test results on the machine so they can be later merged using `--merge-reports` command", as `--merge-reports=reports`. Each job uploads its blob and a final job merges them, and the config writes blobs to `.artifacts/typescript/test-results/.vitest-reports`
- JUnit reporter: "Outputs a report of the test results in JUnit XML format." Dot reporter: "Prints a single dot for each completed test"

`retry: 2` and `sequence.shuffle: true` under `CI` are the tool's own configuration and no workflow variation, they stay as configured, with the consequence recorded: a flaky test can pass on the third try, and order varies per run.

### [06.6]-[STRYKER]

`stryker.config.json` (TypeScript) and `stryker-config.json` (.NET) both exist. The dashboard reporter posts to a hosted service with `STRYKER_DASHBOARD_API_KEY` (<https://stryker-mutator.io/docs/General/dashboard/>), an external service and a secret, out of scope. Incremental mode: "When running in `--incremental` mode, StrykerJS will track the changes you make to your code and tests and only runs mutation testing on the changed code", state in `reports/stryker-incremental.json` overridable with `--incrementalFile`. Limitations: "Stryker will not detect any changes you've made in files other than mutated files and test files" and "Any other changes to your environment are not detected, such as updates to other files, updated (dev) dependencies, changes to environment variables, changes to `.snap` files" (<https://stryker-mutator.io/docs/stryker-js/incremental/>). The incremental file needs a cache entry, correct while those limitations hold. Mutation testing runs on a manually triggered workflow and not on every pull request, and `break: 80` in `stryker-config.json` is a threshold the fifth-round decision removes.

## [07]-[LOCAL_TOOLS]

### [07.1]-[ACTIONLINT]

Newest release `v1.7.12`, 2026-03-30 (<https://github.com/rhysd/actionlint/releases/tag/v1.7.12>), mise registry shorthand `actionlint` (`aqua:rhysd/actionlint`, <https://mise.jdx.dev/registry.html>).

From the README, the checks: "Syntax check for workflow files", "Strong type check for `${{ }}` expressions", "Actions usage check to check that inputs at `with:` and outputs in `steps.{id}.outputs` are correct", "Reusable workflow check", "shellcheck and pyflakes integrations for scripts at `run:`", and "glob syntax validation, dependencies check for `needs:`, runner label validation, cron syntax validation". The runner-label check matters most for a rid matrix: its error enumerates the labels it knows, and the README's list includes `windows-2025-vs2026`, `ubuntu-24.04-arm`, `macos-26`, `macos-26-intel`, and `ubuntu-slim`. A typo like `linux-latest` is caught before a push.

The config file `actionlint.yaml` or `actionlint.yml` "can be put in `.github` directory" and is optional, it holds `self-hosted-runner.labels`, `config-variables`, and per-path `ignore` patterns (<https://github.com/rhysd/actionlint/blob/v1.7.12/docs/config.md>). `prefix-dev/pixi` lists self-hosted labels and ignores every error in its generated `release.yml`. The shellcheck integration catches unquoted expansions of matrix values where a rid string meets a shell.

### [07.2]-[RATCHET]

ratchet is pending the pinning decision. Newest release `v0.12.0`, 2026-07-18 (<https://github.com/sethvargo/ratchet/releases/tag/v0.12.0>). No mise registry shorthand exists (`registry/ratchet.toml` is absent), the entry is the full backend name `github:sethvargo/ratchet`.

ratchet converts a floating reference into a commit SHA and records the constraint in a trailing comment (README at `v0.12.0`): `uses: 'actions/checkout@11bd71901bbe5b1630ceea73d27597364c9af683' # ratchet:actions/checkout@v4`. Commands: `pin`, `unpin`, `update` ("updates all versions to the latest matching constraint"), `upgrade` ("upgrades all versions to the latest version, changing the ratchet comment and also updating the ref", "only works with GitHub Actions references"), and `lint`, which "reports if all versions are pinned, printing any violations, and exiting with a non-zero error code when entries are not pinned". `-bake-delay` "refuses to resolve any commit, release, or image younger than the given duration... pnpm calls this `minimumReleaseAge`; Dependabot calls it `cooldown`. It is off by default", and `pnpm-workspace.yaml` sets `minimumReleaseAge: 1440`, `-bake-delay 24h` matches the existing configuration. Read as reproducibility: "GitHub labels are mutable and Docker tags are mutable."

Known limits: "Indentation is always set to 2 spaces", "Does not support resolving values in anchors or aliases", `${{ }}` in a `uses:` is ignored, `# ratchet:exclude` opts a line out, and the GitHub resolver "defaults to public github.com" and takes `GITHUB_TOKEN` to avoid rate limits. The uv documentation's own examples are ratchet-shaped: `actions/checkout@3d3c42e5aac5ba805825da76410c181273ba90b1 # v7.0.1`.

### [07.3]-[ACT]

Newest release `v0.2.89`, 2026-06-01 (<https://github.com/nektos/act/releases/tag/v0.2.89>), mise registry shorthand `act` (`aqua:nektos/act`).

"`act` depends on `docker` (exactly Docker Engine API) to run workflows in containers" and "is currently not supported with `podman` or other container backends" (<https://nektosact.com/installation/index.html>).

Documented gaps (<https://nektosact.com/not_supported.html>): `concurrency` ignored, `run-name` ignored, step summary not processed, problem matcher ignored, annotations ignored, incomplete `github` context, run-step cancellation not implemented, `job.permissions` ignored, `job.timeout-minutes` ignored, `job.continue-on-error` ignored, OIDC url not defined, `job.environment` ignored, and Docker context is "not going to be worked on". For this workflow the cancel rule, the permissions pair, the timeouts, the Biome and Vitest annotations, and the `nx-set-shas` lookup are all untested locally.

Runner images: "These default images do not contain all the tools that GitHub Actions offers by default in their runners. Many things can work improperly or not at all while running those image... GitHub Actions are running in fully virtualized machines while `act` is using Docker containers (e.g. Docker does not support running `systemd`)", and the full-fidelity image has "WARNING - this image is
>18GB" (<https://nektosact.com/usage/runners.html>). macOS and Windows jobs do not run in containers: "you can opt out of docker and run them directly on your host system", `act -P macos-latest=-self-hosted`, and on the developer's Mac the `macos-26` legs execute against the real Rhino, Xcode, and Homebrew, which is not the runner environment. The `.actrc` example for Apple Silicon is `--container-architecture=linux/amd64` and `--action-offline-mode` (<https://nektosact.com/usage/index.html>), and emulated builds make a full vcpkg leg under act unrealistic. A secret typed on the command line "might be saved as plain text to history file provided by your shell".

act validates a workflow's YAML shape, step ordering, `needs` graph, and `run:` commands on the `ubuntu-24.04` legs with `--dryrun` or `-l`, and it cannot exercise `nx affected`, caching, artifacts, or annotations.

### [07.4]-[NX_TARGETS]

`README.md`: "Targets running a single command name that command directly." All three are single commands, all three are direct-command targets on `eng/project.json` beside `provision`:

- A lint target running `actionlint`, cacheable, `inputs` naming `{workspaceRoot}/.github/workflows/**/*.yml`, `{workspaceRoot}/.github/actions/**/*.yml`, and `{workspaceRoot}/.github/actionlint.yaml`
- An act target running `act --dryrun`, `cache: false` and `parallelism: false`, like `provision`
- A pin-check target running `ratchet lint .github/workflows/*.yml`, cacheable, same inputs, once pinning is adopted

The workflow files join `sharedGlobals` in `nx.json`, which lists `global.json`, `NuGet.config`, `biome.json`, and the TypeScript configs, a workflow change marks the targets affected. On the runners the tools arrive through `jdx/mise-action` like every other tool.

## [08]-[CONSISTENCY_RULES]

### [08.1]-[POLYGLOT_REPOSITORIES]

`.github/` listings beyond `workflows/`, read from the default branch on 2026-09-03:

- `astral-sh/uv` (Rust with Python): `ISSUE_TEMPLATE/`, `PULL_REQUEST_TEMPLATE.md`, `actions/`, `renovate.json5`, `zizmor.yml`, JSON policy files
- `pola-rs/polars` (Rust with Python): `CODEOWNERS`, `CODE_OF_CONDUCT.md`, `FUNDING.yml`, `ISSUE_TEMPLATE/`, `codecov.yml`, `dependabot.yml`, `pull_request_template.md`, three `release-drafter*.yml`, `scripts/`, labeler and PR-title configs
- `prefix-dev/pixi` (Rust with Python packaging): `CODE_OF_CONDUCT.md`, `CONTRIBUTING.md`, `ISSUE_TEMPLATE/`, `SECURITY.md`, `actionlint.yml`, `pull_request_template.md`, `renovate.json5`, `zizmor.yml`
- `vercel/turborepo` (Rust with TypeScript): `CODEOWNERS`, `DISCUSSION_TEMPLATE/`, `ISSUE_TEMPLATE/`, `actions/` (nine composite actions), `pull_request_template.md`, `release.yml`, one bot config
- `nrwl/nx` (TypeScript with Rust): `ISSUE_TEMPLATE/`, `PULL_REQUEST_TEMPLATE.md` and `PULL_REQUEST_TEMPLATE/`, `SAVED_REPLIES.md`, `dependabot.yml`, `agents/`, `prompts/`, `skills/`
- `grafana/grafana` (Go with TypeScript): `CODEOWNERS`, `ISSUE_TEMPLATE/`, `PULL_REQUEST_TEMPLATE.md`, `actionlint.yaml`, `actions/`, `dependabot.yml`, `renovate.json5`, `zizmor.yml`, JSON bot configs

### [08.2]-[WORKFLOW_PER_CONCERN]

`pola-rs/polars/.github/workflows/`: `lint-global.yml`, `lint-python.yml`, `lint-rust.yml`, `test-python.yml`, `test-rust.yml`, `test-coverage.yml`, `docs-global.yml`, `docs-python.yml`, `docs-rust.yml`, `release-python.yml`, `release-rust.yml`, with `benchmark.yml`, `clear-caches.yml`, and labeler workflows. `astral-sh/uv` uses a verb prefix and one aggregate `ci.yml`: `check-fmt.yml`, `check-lint.yml`, `check-lock.yml`, `check-docs.yml`, `check-generated-files.yml`, `test.yml`, `test-integration.yml`, `test-smoke.yml`, `test-system.yml`, `test-ecosystem.yml`, `build-*.yml`, `publish-*.yml`, `release*.yml`. `prefix-dev/pixi` keeps `ci.yml`, `docs.yml`, `release.yml`, `schema.yml`, `test_common_wheels.yml`. `vercel/turborepo` prefixes product workflows, `turborepo-test.yml`, `turborepo-release.yml`, `turborepo-library-release.yml`, and includes `pr-clean-caches.yml`. The unit is the concern, and nothing is one giant workflow or one workflow per project.

### [08.3]-[NAMING]

Lowercase, hyphen-separated, `.yml`, verb or concern first, language or product second. The `name:` key inside is prose, `name: Lint Python` in `pola-rs/polars`. `nrwl/nx` keeps helper TypeScript in `.github/workflows/nightly/` (`analyze-failures.ts`, `process-matrix.ts`, `process-result.ts`), consistent with the no-subdirectory rule for workflows.

### [08.4]-[COMPOSITE_ACTIONS]

`vercel/turborepo/.github/actions/`: `setup-environment`, `setup-node`, `setup-rust`, `setup-protoc`, `setup-capnproto`, `setup-zig`, `install-global-turbo`, `find-rust-changes`, `check-release-pr`. One composite action per toolchain with one per repeated piece of logic. For Rasm one composite action prepares the runner: `jdx/mise-action`, the three package caches, `pnpm install --frozen-lockfile --no-runtime`, `uv sync --locked`, `dotnet tool restore`. `astral-sh/uv` and `grafana/grafana` keep `.github/actions/` as well, and `pola-rs/polars` keeps `.github/scripts/` for shared scripts.

### [08.5]-[REUSABLE_WORKFLOW_VERSUS_COMPOSITE_ACTION]

Preparing a runner is steps inside a job, a composite action. Running the same jobs across four rids is jobs, a matrix. A reusable workflow earns its place when two triggers need the same job graph.

### [08.6]-[WORKFLOW_DISPATCH]

`astral-sh/uv` and `prefix-dev/pixi` carry manually triggered workflows for the expensive paths. The fast checks are `pull_request` with `push` to the default branch, and anything slow or rarely needed is `workflow_dispatch`. That is the home for the all-rids staging run and for mutation testing.

### [08.7]-[CONCURRENCY_AND_PATH_FILTERS]

`pola-rs/polars/.github/workflows/lint-python.yml`, verbatim:

```
on:
  pull_request:
    paths:
      - py-polars/**
      - .github/workflows/lint-python.yml

concurrency:
  group: ${{ github.workflow }}-${{ github.ref }}
  cancel-in-progress: true
```

The path filter includes the workflow file itself, and without it a change to the workflow does not run the workflow it changed. The same file sets `fail-fast: false` on its version matrix and runs `ruff check --no-fix`, `ruff format --diff`, `mypy`, and `pyrefly check` as separate steps. For Rasm, `nx affected` replaces path filters, and self-inclusion comes through `sharedGlobals`.

### [08.8]-[AVOIDED_MISTAKES]

Each item names the mistake, then the evidence:

- Cache key collisions: GitHub's rule "You cannot change the contents of an existing cache", `upload-artifact`'s matrix-naming advice
- Matrix explosions: the 256-jobs limit, `polars` splits per language into separate workflows
- Long-running side effects: `vercel/turborepo` has `pr-clean-caches.yml` and `pola-rs/polars` has `clear-caches.yml`, because caches accumulate against the quota
- Secrets in logs: GitHub's cache note, act's warning about shell history
- Tests that write outside the workspace: Rasm routes every cache and output under `.cache/` and `.artifacts/`
- Stale runner labels: `actionlint`'s runner-label check, the `-latest` migration "happens over 1-2 months"
- A workflow that never runs on its own change: the `paths:` self-inclusion line in `polars`

## [09]-[MISTAKES_AND_CORRECT_FORM]

Each item names the mistake, then the correct form, then the source:

- `VCPKG_BINARY_SOURCES=clear;x-gha,readwrite` with `ACTIONS_CACHE_URL`: `actions/cache` over `.cache/vcpkg-archives` and the `VCPKG_DOWNLOADS` directory, keyed on the vcpkg commit and `eng/native/*/vcpkg.json` (`x-gha` is "Removed" in the vcpkg binarycaching reference, vcpkg-tool PR #1662)
- Caching `.cache/nx` between runs: no cache entry, `nx affected` (nx.dev unknown-local-cache)
- `permissions: contents: read` alone with `nx-set-shas`: `contents: read` and `actions: read` (nx.dev github-integration)
- `actions/checkout` with default `fetch-depth: 1`: `fetch-depth: 0` with `filter: tree:0` ("`fetch-depth: 0` on the checkout step gives Nx access to the full git history" on nx.dev, and MinVer needs the tags in history)
- `corepack enable` to get pnpm: mise installs pnpm and `mise.toml` pins it (Corepack "up to (but not including) 25.0.0", pnpm CI page)
- Four `setup-*` actions restating pins: `jdx/mise-action` reading `mise.toml` and `mise.lock` (mise-action `action.yml` v4.3.0)
- Same artifact name from two matrix legs: name the artifact with the matrix value (upload-artifact README v7.0.1)
- Uploading a staged native tree as a zipped artifact: `tar` the tree and upload with `archive: false` ("File permissions are not maintained during zipped artifact upload")
- Leaving `timeout-minutes` at its default: set it per job ("Default: 360", hard ceiling 6 hours)
- `fail-fast: true` on the native-staging matrix: `fail-fast: false` (GitHub docs fail-fast reusable)
- `cancel-in-progress: true` on `push` to `main`: cancel on pull requests alone (`nx-set-shas` needs the last successful run)
- `dotnet test Workspace.slnx`: `dotnet test --solution Workspace.slnx` (MTP mode migration step 5)
- `--report-trx` without the extension package: reference `Microsoft.Testing.Extensions.TrxReport` in every test project ("Otherwise, the test application rejects the option with MTP exit code 5")
- Treating a nonzero `dotnet test` exit as "tests failed": read the code, `2` failure, `5` bad arguments, `8` zero tests, `4` bad extension setup (MTP exit codes page)
- `--ignore-exit-code 2` to get a green run: never (same page)
- `DOTNET_SKIP_FIRST_TIME_EXPERIENCE`: `DOTNET_NOLOGO` ("no longer supported in .NET Core 3.0 and later")
- `uv sync --frozen` in CI: `uv sync --locked` (`uv sync --help`)
- `biome check` in CI: `biome ci` (biomejs.dev CI recipe)
- Two cache mechanisms over the same pnpm store: one `actions/cache` entry ("You cannot change the contents of an existing cache")
- Caching `node_modules`, `.venv`, `obj/`, or the SDK: cache the package-manager stores alone (restoring the installed tree bypasses the lock-file assertions)
- A `run:` step chaining `ruff && ty && mypy`: one target each (`README.md` requires all three to pass)
- Running `act` to validate the macOS staging job: `act --dryrun` on the Linux legs, and validate macOS on a real runner (act runs macOS jobs "directly on your host system")
- Trusting an `act` green run: read the unsupported list first (nektosact.com not_supported)
- A workflow that does not run when it changes: add the workflow path to `sharedGlobals` (`pola-rs/polars` `lint-python.yml`)
- A stale runner label: run `actionlint` and use explicit labels (runner-images README)
- A version typed into a file for a release: tag the commit, MinVer reads the tag (MinVer README)

## [10]-[JOB_GRAPH]

Names follow the consistency rules.

### [10.1]-[CI]

`ci.yml` runs `on: pull_request`, `push: branches: [main]`, `workflow_dispatch`, with `concurrency: group: ${{ github.workflow }}-${{ github.ref }}`, `cancel-in-progress: ${{ github.event_name == 'pull_request' }}`, and `permissions: contents: read`, `actions: read`. Every job sets `timeout-minutes`. Every job starts with checkout (`fetch-depth: 0`, `filter: tree:0`) and the setup composite action (`jdx/mise-action@v4` with `install_args` under `--locked`, the three package caches, `pnpm install --frozen-lockfile --no-runtime`, `uv sync --locked`, `dotnet tool restore`).

1. `graph`, `ubuntu-24.04`, about 10 min. `nrwl/nx-set-shas@v5`, then `nx graph --file=.artifacts/nx/graph.json`. Outputs `NX_BASE`, `NX_HEAD`, and the graph as an artifact. Later jobs read the two SHAs from this job's outputs, because `nx-set-shas` sets the variables "within the current Job" alone
2. `python`, `ubuntu-24.04`, needs `graph`, about 20 min. The five Python targets through `nx affected`, one step each, then upload `.artifacts/python/` on always
3. `typescript`, `ubuntu-24.04`, needs `graph`, about 20 min. `nx affected -t lint` (`biome ci`), `nx affected -t typecheck`, `nx affected -t test`, then upload `.artifacts/typescript/` on always
4. `stage-native`, matrix over rid with the runner labels, `fail-fast: false`, about 120 min each, needs nothing. Each leg restores the vcpkg caches and the pinned-release cache, then runs `nx run-many -t stage` over the libraries that support the rid. `eng:provision` needs no separate step because the inferred `stage` target declares `dependsOn: [{ projects: ['eng'], target: 'provision' }]` (`native-packaging.ts` line 217). Then `tar` `.artifacts/native/*/stage` and upload the single tarball as `native-stage-${{ matrix.rid }}` with `archive: false`, the `0o755` modes survive. `stage` and `provision` are `cache: false`, the vcpkg binary-archive cache is the one thing that makes a rerun cheap
5. `pack`, one job, needs `stage-native`, about 20 min. Downloads every `native-stage-*` artifact, untars into `.artifacts/native/`, runs the `pack` targets, which write into `.artifacts/nuget`, the `local` NuGet source, then uploads `.artifacts/nuget/` as `nuget-local`. This is the shape `stage.py`'s docstring describes. Runner: `ubuntu-24.04` when the packaging projects build without a host toolchain, `eng/native/` has its own `Directory.Build.props`, `Directory.Build.targets`, and `Directory.Packages.props` isolating it from the root files, verified on the first run, and `macos-26` otherwise
6. `dotnet`, `macos-26`, needs `graph` and `pack`, about 45 min. Downloads `nuget-local` into `.artifacts/nuget`, then `dotnet restore Workspace.slnx --locked-mode`, `nx affected -t build` with `-bl:.artifacts/dotnet/build.binlog`, and `nx affected -t test`. Uploads the binlog with `if: failure()` and the test results on always. macOS because `Directory.Build.props` resolves `RhinoAppPath` to a macOS bundle and `Directory.Build.targets` holds host references. `pack` produces the `.artifacts/nuget` feed every run, and `.gitignore` ignores `**/.artifacts/`, nothing is committed
7. `coverage`, `ubuntu-24.04`, `needs: [python, typescript, dotnet]`, `if: always()`, about 5 min. Downloads the result artifacts, merges Vitest blobs with `--merge-reports`, merges the .NET Cobertura files with ReportGenerator, and uploads one report per language. No threshold and no gate, the job fails when a needed job did not succeed, one job name stands for the run

Ordering: `stage-native` does not depend on `graph`, `dotnet` depends on `pack` because `NuGet.config` maps `Rasm.*` to `.artifacts/nuget`, which exists on a fresh runner after `pack` alone, and `python` and `typescript` are independent of both.

### [10.2]-[RELEASE]

`release.yml` runs `on: push: tags: ['*']` with `permissions: contents: write`. One job: checkout with `fetch-depth: 0`, the setup composite action, `dotnet restore Workspace.slnx --locked-mode`, `nx run-many -t build` (MinVer stamps the version from the tag), then `gh release create "$GITHUB_REF_NAME" --generate-notes` with the `gh` on the runner image. Nothing is pushed to a registry, and the release holds the notes and the tag.

### [10.3]-[STAGE_ALL]

`stage-all.yml` runs on `workflow_dispatch` with `rid` and `library` inputs, a single failing port can be rebuilt without the whole matrix. The full rid matrix with `pack`, for when a vcpkg pin or a native manifest changes.

### [10.4]-[MUTATION]

`mutation.yml` runs on `workflow_dispatch` alone. Stryker for TypeScript and .NET through their Nx targets, with the incremental file cached and `--force` available as an input.

### [10.5]-[LINT_WORKFLOWS]

`lint-workflows.yml` runs on `pull_request` with `paths: ['.github/**']`. `actionlint` through its Nx target, and `ratchet lint` joins it when pinning is adopted.

## [11]-[OPEN_QUESTIONS]

1. Whether the .NET job needs a real Rhino. `Directory.Build.props` falls back to `/Applications/RhinoWIP.app`, which does not exist on a GitHub runner, and `Directory.Build.targets` `VerifyRhinoHostBundle` raises `RASM0003` for a `HostCommon` project with referenced bundle files missing. No project sets `RasmHost`, the check is inert, and the first host project decides whether the `dotnet` job is possible on hosted runners at all
2. Whether action pinning is adopted. The plan makes `ratchet` conditional on that answer. Yes reformats every workflow to two-space indentation and adds an `update`/`upgrade` habit, and the answer changes how every `uses:` line is written from the first file

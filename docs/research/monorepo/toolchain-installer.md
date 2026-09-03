<!-- Source for monorepo-build-infrastructure [02]-[TOOLCHAIN] and references/installer.md, nothing integrated yet -->

# Toolchain installer research: mise for Rasm

Versions and release dates were read from the GitHub Releases API on 2026-09-03, and the mise documentation quotations come from the source repository at tag `v2026.9.1`.

Settled: mise pins node, pnpm, python, and uv, and reads `global.json` for the .NET SDK. `[tasks]` stays unused because Nx owns tasks, `[env]` is limited to shell and tool discovery, task runtime variables stay Nx `.env` inputs, and native library paths stay machine state.

## [01]-[REPOSITORY_BASELINE]

The repository pins the following values, and the installer restates none of them.

| [WHAT] | [VALUE] | [FILE] |
| :----- | :------ | :----- |
| .NET SDK band | `10.0.400`, `rollForward: disable` | `global.json:2-5` |
| .NET test runner | `Microsoft.Testing.Platform` | `global.json:6-8` |
| .NET local tool | `dotnet-stryker` `4.16.0` | `.config/dotnet-tools.json:5-8` |
| .NET package versions | Central package management, 160 KB of `PackageVersion` rows | `Directory.Packages.props` |
| Node engine range | `node >=24.15.0` in `engines` | `package.json:290-292` |
| Package manager | `pnpm >=11.9.0 <12`, `onFail: error` in `devEngines.packageManager` | `package.json:293-298` |
| TypeScript versions | One catalog of 279 entries, every root `package.json` entry is `catalog:` | `pnpm-workspace.yaml:12+`, `package.json:9-289` |
| pnpm store and cache | `cacheDir: .cache/pnpm/cache`, `storeDir: .cache/pnpm/store` | `pnpm-workspace.yaml:7,9` |
| pnpm release delay | `minimumReleaseAge: 1440` | `pnpm-workspace.yaml:11` |
| Python floor | `requires-python = ">=3.15"` in `[project]` | `pyproject.toml:4` |
| Python dependencies | `[dependency-groups]` bare names, `workspace` 310, `dev` 35, `uv.lock` fixes versions | `pyproject.toml:6-372`, `uv.lock` |
| uv default groups | `default-groups = ["workspace", "dev"]` in `[tool.uv]` | `pyproject.toml:382-383` |
| Nx cache directory | `.cache/nx/cache` | `nx.json:4` |
| Nx cloud | `neverConnectToCloud: true` | `nx.json:5` |
| vcpkg commit | `30ef65cad98f08e7197c9a1656fbd871bcb72f2d` | `eng/scripts/provision.py:56` |
| CMake | Fetched through `vcpkg fetch cmake` | `eng/scripts/provision.py:263-265` |
| vcpkg binary cache | `VCPKG_DEFAULT_BINARY_CACHE=.cache/vcpkg-archives` | `eng/scripts/provision.py:58, 283` |
| Pinned native releases | EnergyPlus `25.2.0` with sha256, DuckDB extensions, sqlite-vec, emgucv commit | `eng/scripts/provision.py:61-89` |

`eng/native/*/*.json` holds the pinned native release manifests beside `provision.py`.

The directory listing confirms the following absences:

- No `.tool-versions`, `mise.toml`, `flake.nix`, `.envrc`, `aqua.yaml`, `devbox.json`, or `.prototools`
- No `.github/` directory, the plan assumes one exists and `act` and `actionlint` enter the installer with the first workflow from the CI thread
- No `dotnet.dotnetPath`, `biome.lspBin`, or `ruff.path` in `.vscode/settings.json`

`eng/scripts/provision.py` is the only installer the repository owns. Its `main()` runs `dotnet tool restore`, bootstraps vcpkg, and fetches pinned archives (`:384-401`), and it assumes `dotnet`, `git`, `curl`, `otool`, `install_name_tool`, and `codesign` on `PATH` (`:178, :199, :257-260, :350-362`).

### [01.1]-[ENVIRONMENT_VARIABLES]

The repository depends on the variables in the table, and the source column names the file that sets each.

| [VAR] | [SOURCE] | [PURPOSE] |
| :---- | :------- | :-------- |
| `DOTNET_ROOT` | `modules/home/programs/languages/dev-tools.nix:201` (Nix store path) | Roslyn and SDK-discovery tools |
| `DOTNET_NOLOGO` | `modules/home/environments/shell.nix:84` | Quiet CLI |
| `DOTNET_CLI_TELEMETRY_OPTOUT` | `modules/home/environments/shell.nix:85` | Telemetry off |
| `UV_CACHE_DIR` | `modules/common/toolchain-env.nix:49` | Points the uv cache at `$XDG_CACHE_HOME/uv` |
| `UV_PYTHON_PREFERENCE=only-system` | `modules/common/toolchain-env.nix:50` | Forces uv onto the machine CPython 3.15 |
| `UV_PYTHON_DOWNLOADS=never` | `modules/common/toolchain-env.nix:51` | Stops uv supplying an interpreter |
| `MACOSX_DEPLOYMENT_TARGET` | `modules/common/toolchain-env.nix:56` | Reaches the Python sdist builds and every native compile |
| `DYLD_FALLBACK_LIBRARY_PATH` | `modules/home/programs/languages/scientific-tools.nix:401` | `pyvips`, `python-magic` `dlopen` unqualified names |
| `GDAL_CONFIG`, `GDAL_DATA` | `modules/common/toolchain-env.nix:59-60` | `geopandas`, `rasterio`, `shapely` |
| `PROJ_DATA`, `PROJ_DIR`, `PROJ_INCDIR`, `PROJ_LIB`, `PROJ_LIBDIR` | `modules/common/toolchain-env.nix:62-66` | `pyproj` |
| `VCPKG_DEFAULT_BINARY_CACHE` | Rasm, `eng/scripts/provision.py:283` | vcpkg binary cache under `.cache/` |

`PUPPETEER_EXECUTABLE_PATH` is built at `modules/common/toolchain-env.nix:148` and exported from `modules/home/environments/languages.nix:56` and `modules/darwin/settings/system.nix:126`, because the catalog holds `@mermaid-js/mermaid-cli` with `allowBuilds: puppeteer: false`.

The plan lists `DOTNET_ROOT`, `DOTNET_NOLOGO`, `DOTNET_CLI_TELEMETRY_OPTOUT`, `UV_PYTHON_PREFERENCE`, `UV_PYTHON_DOWNLOADS`, `MACOSX_DEPLOYMENT_TARGET`, `DYLD_FALLBACK_LIBRARY_PATH`, the GDAL and PROJ set, `PUPPETEER_EXECUTABLE_PATH`, and the four XDG cache variables as couplings to cut.

## [02]-[CANDIDATE_VERSIONS]

The GitHub Releases API reported the following releases on 2026-09-03.

| [TOOL] | [REPO] | [LATEST_STABLE] | [RELEASED] |
| :----- | :----- | :-------------- | :--------- |
| mise | `jdx/mise` | `v2026.9.1` | 2026-09-02 |
| mise-action | `jdx/mise-action` | `v4.3.0` | 2026-08-25 |
| aqua | `aquaproj/aqua` | `v2.62.3` | 2026-08-02 |
| vfox | `version-fox/vfox` | `v1.0.11` | 2026-04-29 |
| proto | `moonrepo/proto` | `v0.61.2` | 2026-09-01 |
| devbox | `jetify-com/devbox` | `0.18.0` | 2026-08-16 |
| pkgx | `pkgxdev/pkgx` | `v2.11.0` | 2026-07-22 |
| asdf | `asdf-vm/asdf` | `v0.20.0` | 2026-07-07 |
| act | `nektos/act` | `v0.2.89` | 2026-06-01 |
| actionlint | `rhysd/actionlint` | `v1.7.12` | 2026-03-30 |
| uv | `astral-sh/uv` | `0.12.9` | 2026-09-01 |
| ruff | `astral-sh/ruff` | `0.16.5` | 2026-08-27 |
| ty | `astral-sh/ty` | `0.0.78` | 2026-09-02 |
| Biome | `biomejs/biome` | `@biomejs/biome@2.5.12` | 2026-09-03 |
| Nx | `nrwl/nx` | `23.2.0` | 2026-09-02 |
| pnpm | `pnpm/pnpm` | `v12.3.1` (`v11.25.0` on the 11 line, 2026-08-29) | 2026-09-03 |

mise publishes calendar-versioned releases about weekly, vfox's newest stable release is four months old, and proto is `0.x`.

mise's own CI documentation at tag `v2026.9.1` shows `uses: jdx/mise-action@v3` (`docs/continuous-integration.md:85`) while the action's latest release is `v4.3.0`. Rasm uses `@v4`.

## [03]-[RUNTIMES]

Core means the installer implements the tool itself, plugin means a third-party definition, and registry package means an entry in a curated index the installer builds against.

mise `v2026.9.1` installs node and python as core plugins (`core:node`, `core:python`), pnpm from the registry (`aqua:pnpm/pnpm` then `npm:pnpm`), uv from the registry (`aqua:astral-sh/uv`, `asdf:asdf-community/asdf-uv`, `pipx:uv`), and the .NET SDK as a core plugin (`core:dotnet`) with vfox and asdf fallbacks, and it reads `sdk.version` alone from `global.json`.

- aqua `v2.62.3`: `pkgs/nodejs/node`, `pkgs/pnpm/pnpm`, `pkgs/astral-sh/uv`, no python package, no .NET package
- vfox `v1.0.11`: `nodejs.json`, `python.json`, `dotnet.json`, no pnpm or uv plugin in the registry, ignores `global.json`
- proto `v0.61.2`: node, pnpm, python, and uv built in, .NET through the `asdf` backend alone, `global.json` undocumented
- devbox `0.18.0`: a nixpkgs attribute per tool, ignores `global.json`
- pkgx `v2.11.0` with `dev` `v1.8.1`: a pantry project per tool, ignores `global.json`
- nix flake: a nixpkgs attribute per tool, `dotnet-sdk_*` for the SDK, ignores `global.json`
- asdf `v0.20.0`: a plugin per tool, `global.json` support plugin-dependent

Sources for each cell follow.

mise registry entries at tag `v2026.9.1` are `registry/{node,pnpm,python,uv,dotnet}.toml`. `dotnet.toml` reads `backends = ["core:dotnet", "vfox:mise-plugins/vfox-dotnet", "asdf:mise-plugins/mise-dotnet"]` and `idiomatic_files = ["global.json"]`, `python.toml` reads `idiomatic_files = [".python-version", ".python-versions"]`, `node.toml` reads `[".nvmrc", ".node-version", "package.json"]`, and `pnpm.toml` reads `["package.json"]`.

mise .NET behavior, `docs/lang/dotnet.md`: "The core .NET plugin installs .NET SDKs using Microsoft's official install script. All SDK versions are installed side-by-side under a shared `DOTNET_ROOT` directory". Its environment table sets `DOTNET_ROOT`, `DOTNET_MULTILEVEL_LOOKUP=0`, and `DOTNET_CLI_TELEMETRY_OPTOUT` "Only set when `dotnet.cli_telemetry_optout` is configured" (`:132-140`).

aqua package inventory, `repos/aquaproj/aqua-registry/git/trees/main?recursive=1`: 2290 `pkg.yaml` files. `pkgs/nodejs/node`, `pkgs/pnpm/pnpm`, `pkgs/astral-sh/uv`, `pkgs/astral-sh/ruff`, `pkgs/biomejs/biome`, `pkgs/Kitware/CMake`, `pkgs/nektos/act`, and `pkgs/rhysd/actionlint` exist. No path contains `python`, and `dotnet` matches the unrelated `pkgs/microsoft/*` entries alone (`component-detection`, `edit`, `kiota`, `ripgrep-prebuilt`, `vscode/code`, `winappCli`).

proto built-ins, `crates/core/src/config.rs` at `v0.61.2`, `inherit_builtin_plugins()` lines 138-262: tools `bun`, `deno`, `go`, `java`/`jdk`/`jre`, `node`, `npm`/`nub`/`pnpm`/`yarn`, `poetry`, `python`, `uv`, `ruby`, `rust`, `swift`, and moonrepo's own CLI, backends `asdf`, `cargo`, `npm`. No `dotnet`.

pkgx pantry, `repos/pkgxdev/pantry/git/trees/main?recursive=1`: `projects/nodejs.org`, `projects/pnpm.io`, `projects/python.org`, `projects/astral.sh/{uv,ruff,ty}`, `projects/dotnet.microsoft.com`. The .NET `package.yml` sets `runtime.env.DOTNET_ROOT: '{{prefix}}'`, resolves versions from `github: dotnet/sdk/tags`, and downloads `https://dotnetcli.azureedge.net/dotnet/Sdk/{{version}}/dotnet-sdk-{{version}}-${PLATFORM}.tar.gz`.

devbox packages, `https://www.jetify.com/docs/devbox/configuration/`: `packages` is a list or map of Nix packages pinned with `@version`. asdf, `https://asdf-vm.com/manage/configuration.html`: `.tool-versions` is the only version file, and every tool comes from a plugin.

vfox plugin registry, `repos/version-fox/vfox-plugins/contents/plugins`: 40 plugins with `index.json` (`bun, chaosblade, clang, cmake, crystal, dart, deno, dotnet, elixir, erlang, etcd, flutter, gcc-arm-none-eabi, golang, gradle, grails, groovy, java, julia, kotlin, kubectl, lua, make, maven, mongo, mongod, ninja, nodejs, php, protobuf, python, ruby, rust, scala, terraform, tomcat, typst, vagrant, vlang, zig`). No `pnpm`, no `uv`. `dotnet.json` points at `vfox-dotnet 0.3.0` with description "dotnet plugin, support for dotnet sdks 6.0, 7.0, 8.0". mise's registry names a different fork, `vfox:mise-plugins/vfox-dotnet`.

### [03.1]-[GLOBAL_JSON_IN_MISE]

mise's core dotnet plugin reads `global.json` with the parser at `src/plugins/core/dotnet.rs`, `_parse_idiomatic_file`, lines 158-168:

```rust
let global_json: GlobalJson = serde_json::from_str(&content)?;
let sdk = global_json
    .sdk
    .ok_or_else(|| eyre::eyre!("no sdk.version found in {}", path.display()))?;
if sdk.version.is_empty() { return Ok(vec![]); }
Ok(vec![sdk.version])
```

The parser reads `sdk.version` alone and never reads `rollForward` or `allowPrerelease`. mise installs the literal string `10.0.400`, which agrees with Rasm's `"rollForward": "disable"` by value rather than by semantics. The reader is off by default: "Enable idiomatic version file support: `mise settings set idiomatic_version_file_enable_tools=dotnet`" (`docs/lang/dotnet.md:56`), and Rasm sets it in `mise.toml` `[settings]`. `global.json` holds `"test": { "runner": "Microsoft.Testing.Platform" }` as well, and mise's `GlobalJsonSdk` struct ignores everything but `sdk`.

### [03.2]-[SDK_IN_OTHER_CANDIDATES]

aqua cannot install the .NET SDK. Its seven package types are `github_release`, `github_content`, `github_archive`, `http`, `go_install`, `go_build`, and `cargo` (`pkg/config/registry/package_info.go:30-42` at `v2.62.3`). An `http` package is expressible, but no `dotnet` and no `python` package exists among the 2290, and Rasm would author and maintain both.

proto reaches .NET through its `asdf` backend running an asdf `dotnet` plugin, and its lockfile is "currently unstable" (`https://moonrepo.dev/docs/proto/lockfile`). devbox and a nix flake install whatever `dotnet-sdk_*` attribute the pinned nixpkgs provides, a nixpkgs revision decision rather than a `10.0.400` decision. pkgx sets `DOTNET_ROOT`, and nothing in its package reads `global.json`. mise is the only candidate with .NET SDK support in a core plugin and the only one that reads `global.json`.

## [04]-[BACKENDS_AND_LOCKFILES]

mise `v2026.9.1` reaches GitHub releases through `github:`, `gitlab:`, `forgejo:`, `ubi:`, `aqua:`, `http:`, and `s3:`, npm through `npm:`, PyPI through `pipx:`, cargo through `cargo:`, and NuGet through `dotnet:`, and its lockfile is `mise.lock` with `lockfile_version = 1`.

- aqua `v2.62.3`: GitHub release and `http` package types, a `cargo` type, no npm, PyPI, or NuGet, `aqua-checksums.json`
- vfox `v1.0.11`: plugin-driven releases, npm, and PyPI, no cargo or NuGet, no documented lockfile
- proto `v0.61.2`: TOML/WASM plugins, `npm` and `cargo` backends, no PyPI (`poetry` and `uv` as tools), no NuGet, `.protolock` documented as unstable
- devbox `0.18.0`: flake refs alone, no package ecosystem, `devbox.lock`
- pkgx `v2.11.0`: pantry packages alone, no package ecosystem, no lockfile
- nix flake: fixed-output derivations you write, no package ecosystem, `flake.lock`
- asdf `v0.20.0`: plugin-driven releases, npm, and PyPI, no cargo or NuGet, no lockfile

Sources: mise `docs/dev-tools/backends/index.md` and `docs/dev-tools/mise-lock.md` at `v2026.9.1`, proto `https://moonrepo.dev/docs/proto/lockfile`, aqua `https://aquaproj.github.io/docs/reference/security/checksum`. mise `docs/dev-tools/backends/dotnet.md` reads "The following installs the latest version of GitVersion.Tool", shows the config form `"dotnet:GitVersion.Tool" = "5.12.0"`, and offers an `install_env` option for the `dotnet tool install` command. The `dotnet:` backend installs .NET global tools from NuGet, and the separate core plugin installs the SDK.

### [04.1]-[MISE_LOCK]

`docs/dev-tools/mise-lock.md` at `v2026.9.1` (`:392-397`) records the entry shape per backend:

- Full (version, checksum, size, URL): `aqua`, `http`, `github`, `gitlab`
- Partial (version, URL, provenance): `vfox` tool plugins
- Partial (version, checksum, size): `ubi`
- Basic (version, checksum): `core` (some tools)
- Version only: `asdf`, `npm`, `cargo`, `pipx`

The same page (`:262`): "`asdf`, `cargo`, `gem`, `go`, `npm`, `pipx`, `ubi`, `core:dotnet`, `core:rust`, and `core:swift` install through an external tool or resolve their download at install time ... so strict mode skips them instead of failing".

Rasm's tool set gets full entries for `pnpm`, `uv`, `act`, and `actionlint` (all resolve to `aqua:` entries), checksum-only entries for `node` and `python` (`core`), and no entry for the .NET SDK (`core:dotnet`). `global.json` pins the SDK and Microsoft's install script installs it, and the lockfile adds nothing for it. `mise.lock` is committed as the reproducibility record, the role `uv.lock` and `pnpm-lock.yaml` play, and the `locked` strict mode stays off.

mise's `minimum_release_age` "defaults to `24h`" (`docs/dev-tools/mise-lock.md:528`). Rasm leaves it at the default and declares nothing.

## [05]-[ENVIRONMENT_AND_ACTIVATION]

mise holds per-directory variables in the `[env]` table (literal, `default`, `required`, `redact`, `_.file` dotenv, `tools = true` lazy values) and `PATH` entries in the `_.path` directive, activates through `mise activate <shell>`, reaches an editor without a shell hook through shims alone (`[env]` values reach the process only when a shim runs), and reaches CI through `mise exec --`, `mise run`, shims on `PATH`, or `mise-action` writing `GITHUB_ENV` and `GITHUB_PATH`.

- aqua: no env, `$AQUA_ROOT_DIR/bin` entries lazy by default, a `PATH` entry with no hook
- vfox: plugin-supplied env alone, per-directory `PATH`, `eval "$(vfox activate zsh)"`, editor and CI use undocumented
- proto: `[env]` with `${VAR}` substitution and `file = ".env"`, per-directory `PATH`, `proto activate`, shims for editors and CI
- devbox: `env` object in `devbox.json`, per-directory `PATH`, `devbox shell` or direnv, `devbox generate direnv` for editors, `devbox run` in CI
- pkgx with `dev`: package `runtime.env` alone, per-directory `PATH`, `dev` shellcode or `pkgm` shims, shims for editors, `pkgx +pkg -- cmd` in CI
- nix flake: `shellHook` or `env` in `mkShell`, per-directory `PATH`, `nix develop` or direnv, direnv with an editor plugin, `nix develop -c` in CI
- asdf: no env, shims for `PATH`, shims on `PATH` in the shell, editors and CI through the same shims

aqua and asdf work in an editor and in CI without a shell hook.

mise `[env]`, `docs/environments/index.md`: plain assignment, `{ default = "…" }`, `{ required = true }`, `{ redact = true }`, `_.file`, `_.path`, and `tools = true` for values that need tool paths resolved first. The caveat (`:119-121`): "Environment variables typically are resolved before tools ... This does not apply to variables that configure mise itself, such as `MISE_DATA_DIR` or `MISE_INSTALLS_DIR`. These variables are read when the process starts". mise editor caveat, `docs/ide-integration.md:84-85`: "using `shims` doesn't work with all mise features. For example, arbitrary env vars in `[env]` will only be set if a shim is executed."

aqua config attributes, `https://aquaproj.github.io/docs/reference/config/`: `registries`, `packages`, `checksum`, `import_dir`, and no `env` key. proto `[env]`, `https://moonrepo.dev/docs/proto/config`. devbox schema, `https://www.jetify.com/docs/devbox/configuration/`.

### [05.1]-[RASM_VARIABLES]

mise's dotnet plugin sets `DOTNET_ROOT`, and `[env]` does not. The variable with a Nix store path value becomes an installer-owned, portable value.

`MACOSX_DEPLOYMENT_TARGET`, `GDAL_*`, `PROJ_*`, `DYLD_FALLBACK_LIBRARY_PATH`, and `PUPPETEER_EXECUTABLE_PATH` name a native library prefix. mise `[env]` can hold them when mise installs the library the path points into, and mise has no `gdal`, `proj`, `pango`, or Chromium runtime in its core set. The installer moves the .NET, Node, Python, and CLI-tool variables into the repository and leaves the native-library ones as machine state, as the plan states.

## [06]-[CONFIG_FILES]

mise reads `mise.toml` (with `.mise.toml`, `mise/config.toml`, `.config/mise.toml`, `conf.d/*.toml`), walks up from the working directory merging with the nearer file winning, works from one root `mise.toml` in a monorepo (`monorepo_root = true` adds path-prefixed task names, `docs/tasks/monorepo.md`, unused here), and installs idempotently with `mise install` (`--locked` when a lockfile exists).

- aqua: `aqua.yaml` or `.aqua/aqua.yaml`, walks up reading all, one root file with `import` and `import_dir`, `aqua i`
- vfox: `.vfox.toml` per directory, no documented monorepo model, `vfox install`
- proto: `.prototools`, `local`, `global`, `user` modes with `--config-mode`, one root file, `proto install`
- devbox: `devbox.json` with `devbox.lock` in the project directory, `include` composes, one root file, `devbox install`
- pkgx with `dev`: no file of its own, reads project files in the project directory, no monorepo model, `dev .`
- nix flake: `flake.nix` with `flake.lock` at the repository root, one root file, `nix develop`
- asdf: `.tool-versions`, applies to the directory and its subdirectories, one root file, `asdf install`

mise write target, `docs/configuration.md:131-133`: "they use the lowest precedence file in the highest precedence directory ... If both `mise.toml` and `mise.local.toml` exist, writes go to `mise.toml`". A single root `mise.toml` stays the write target. Trust, `docs/security.md:81-83`: "Untrusted configs still require `mise trust` (or a trusted config path) ... Safe mode limits what a config can do; trust limits which configs are loaded." A fresh clone runs `mise trust` once, and CI names the workspace through the `trusted_config_paths` setting (`MISE_TRUSTED_CONFIG_PATHS`) before `mise install`.

## [07]-[CI]

- mise: `jdx/mise-action@v4` (`v4.3.0`, 2026-08-25), caches by default (`cache_key_prefix: mise-v1`, templated `cache_key`), exports env and `PATH`
- aqua: `aquaproj/aqua-installer@v4`, no built-in caching (`actions/cache` over packages and registries), `PATH` alone
- vfox: no action (`repos/version-fox/vfox-action` returns 404), no export
- proto: `moonrepo/setup-toolchain`, caching through that action, no export
- devbox: `jetify-com/devbox-install-action`, Nix store cache, `devbox run` wrapping alone
- pkgx: `pkgxdev/setup`, caching undocumented, no export
- nix flake: third-party installer actions, a separate binary-cache action, no export
- asdf: `asdf-vm/actions` (repository last pushed 2026-03-01, no tagged release), `actions/cache`, no export

The relevant `jdx/mise-action@v4.3.0` inputs, from `action.yml` at that tag:

| [INPUT] | [DEFAULT] | [DESCRIPTION] |
| :------ | :-------- | :------------ |
| `version` | latest | "The version of mise to use." |
| `minimum_release_age` | none | Installs the newest stable release older than the cutoff (`24h`, `7d`, ISO dates) |
| `install` | `"true"` | "if false, will not run `mise install` or `mise bootstrap`" |
| `install_args` | none | "When a repo mise lock file is present, the action automatically adds `--locked` unless you already provided it." |
| `cache` | `"true"` | "if false, action will not read or write to cache" |
| `cache_key_prefix` | `"mise-v1"` | "change this to invalidate the cache" |
| `add_shims_to_path` | `"true"` | "if false, will not add mise shims directory to PATH" |
| `env` | `"true"` | "Automatically load mise environment variables for subsequent steps." |
| `export_path` | `"true"` | "Add PATH entries produced by mise, including `[env] _.path`, to subsequent steps through GITHUB_PATH." |
| `github_token` | `${{ github.token }}` | Token for GitHub-hosted tool downloads |

`env: true` with `export_path: true` is how mise activates in CI without shell hooks: the action writes to `GITHUB_ENV` and `GITHUB_PATH`, and `[env]` variables and `_.path` entries reach every later step, including plain `run:` steps. aqua and asdf give `PATH` and nothing else.

## [08]-[EDITOR_INTEGRATION]

Rasm's `.vscode/settings.json` names one path (`:47`), `"python.defaultInterpreterPath": "${workspaceFolder}/.venv/bin/python"`, the uv-created project venv, with the comment "Unlike ruff and ty, mypy needs this pin to run the uv.lock version" (`:46`).

`hverlin/mise-vscode` (`v1.25.0`, 2026-08-27) is the extension `docs/ide-integration.md` links. Its `docs/src/content/docs/reference/Supported-extensions.md` at `v1.25.0` lists the settings it writes, and the rows that intersect Rasm's toolchain follow:

| [VS_CODE_EXTENSION] | [SETTING_IT_WRITES] | [NOTE_FROM_THE_TABLE] |
| :------------------ | :------------------ | :-------------------- |
| `ms-python.python` | `python.defaultInterpreterPath` | "You will still need to select the interpreter." |
| `charliermarsh.ruff` | `ruff.path`, `ruff.interpreter` | |
| `astral-sh.ty` | `ty.path` | |
| `biomejs.biome` | `biome.lsp.bin` | "Not enabled by default ... Use it only if you don't install biome with npm." |
| `twxs.cmake` | `cmake.cmakePath` | |
| `ms-dotnettools.vscode-dotnet-runtime` | `dotnetAcquisitionExtension.sharedExistingDotnetPath` | |

`mypy-type-checker` is absent from the table. Rasm pins mypy through the venv interpreter path with `"mypy-type-checker.importStrategy": "fromEnvironment"` (`:48`), and uv keeps owning it. The Biome row warns against installer-managed Biome when npm supplies it, Rasm installs `@biomejs/biome` through the pnpm catalog (`package.json:14`), and mise never installs Biome, `@ast-grep/cli`, or any other `catalog:` entry. Auto-configuration is off by default: "By default, Mise VSCode will not modify your `.vscode/settings.json` file" (`Supported-extensions.md:11`), and the extension's `package.json` at `v1.25.0` declares `mise.configureExtensionsAutomatically` default `false` and `mise.configureExtensionsAutomaticallyIgnoreList` default `["biomejs.biome", "oxc.oxc-vscode"]`. Rasm keeps auto-configuration off and hand-writes the committed settings file.

None of aqua, asdf, vfox, pkgx, or devbox has a comparable extension.

## [09]-[MANIFEST_BOUNDARIES]

`pnpm-workspace.yaml` `catalog:` owns every TypeScript dependency version, including `@biomejs/biome`, `@ast-grep/cli`, `@bufbuild/buf`, `vitest`, and `nx`. The installer never installs `biome`, `ast-grep`, `buf`, `nx`, or any catalog package, and it owns `node` and `pnpm`, which the catalog cannot install.

`package.json` `engines` and `devEngines` own `node >=24.15.0` and the pnpm range with `onFail: error`. The installer never contradicts those ranges, and it owns the exact `node` and `pnpm` patch inside them.

`pyproject.toml` with `uv.lock` owns every Python dependency, and the `dev` group declares `mypy` (`:344`), `ruff>=0.16.2` (`:356`), and `ty` (`:362`). The installer never installs `ruff`, `ty`, or `mypy`, and it owns `python` and `uv`.

`.config/dotnet-tools.json` owns `dotnet-stryker 4.16.0`. The installer never installs `dotnet:dotnet-stryker` and owns nothing there, and `dotnet tool restore` stays in `eng/scripts/provision.py`.

`global.json` owns .NET SDK `10.0.400`. The installer never restates the version in a second file and reads it through `idiomatic_version_file_enable_tools`.

`Directory.Packages.props` owns every NuGet package version, and the installer owns nothing there. `eng/scripts/provision.py` owns the vcpkg commit, CMake through `vcpkg fetch cmake`, and the pinned native archives, and the installer never installs a second `cmake`.

Collisions, each settled:

- CMake: `eng/scripts/provision.py:263-265` returns the CMake that `vcpkg fetch cmake` produces, chosen to match the vcpkg baseline
- ruff, ty, mypy: all three sit in the `dev` group, and `uv.lock` fixes their versions
- Node and pnpm: `package.json` `engines` states a range and fails a mismatched run, and nothing in the repository installs a conforming Node

A `cmake` entry in `mise.toml` puts a second CMake earlier on `PATH`, and `provision.py` keeps owning CMake while `mise.toml` never names it. With `ruff` in `[tools]`, mise's registry installs `ruff` from `aqua:astral-sh/ruff` and shadows the venv copy, and `CLAUDE.md` `[DEPENDENCY_SOURCES]` settles it: "ALWAYS spell ALL Python dependency rows as bare unpinned names, `uv.lock` alone fixes versions". `mise.toml` never names them, and the same rule makes `pyproject.toml:356` `"ruff>=0.16.2"` a row to correct to the unpinned name. `pnpm-workspace.yaml` cannot install its own pnpm, and Node and pnpm are the installer's.

The installer pins language runtimes and standalone binaries, and everything reachable through `catalog:`, `uv.lock`, `Directory.Packages.props`, or `.config/dotnet-tools.json` stays where it is.

## [10]-[NX_TASK_ENVIRONMENT]

Nx owns task-runtime variables, and the loading is on by default. The rules come from the installed Nx `23.1.3` source under `node_modules/nx/dist`.

### [10.1]-[FILE_ORDER]

`src/tasks-runner/task-env-paths.js` builds the list. For a task on project root `P`, target `T`, configuration `C`, the identifiers are, in order: `T.C`, `C`, `T`, then the empty identifier. For each identifier it emits four names, and it emits the whole sequence for the project root first and then the workspace root:

```
${path}.env.${identifier}.local
${path}.env.${identifier}
${path}.${identifier}.local.env
${path}.${identifier}.env
```

and for the empty identifier `${path}.env.local`, `${path}.local.env`, `${path}.env`.

### [10.2]-[PRECEDENCE]

`src/tasks-runner/task-env.js`, `getTaskSpecificEnv`:

```js
const taskEnv = unloadDotEnvFiles({ ...process.env });
const env = process.env.NX_LOAD_DOT_ENV_FILES === 'true' ? loadDotEnvFilesForTask(task, graph, taskEnv) : taskEnv;
```

`getEnvVariablesForTask` spreads the dotenv result first and Nx's own task variables last ("Nx Env Variables overrides everything", `:36, :71`). `loadAndExpandDotEnvFile` calls dotenv with `override = false` (`:138`), earlier files in the list win over later ones, and a value already in the process environment wins over every file ("User Process Env Variables override Dotenv Variables", `:34`). `unloadDotEnvFiles` (`:193`) first removes workspace-root `.env`, `.local.env`, and `.env.local` values from the inherited environment, and a task-scoped file can beat an init-time root load.

Ordering, highest to lowest:

1. `NX_*` task variables Nx injects (`NX_TASK_TARGET_PROJECT`, `NX_TASK_HASH`, `NX_WORKSPACE_ROOT`, `FORCE_COLOR`, ...)
2. An `nx:run-commands` target's `env` option, with the schema description "This property has priority over the `.env` files."
3. The inherited shell environment, minus the unloaded root `.env` values
4. Dotenv files in the order of the file list, project-scoped before workspace-scoped

### [10.3]-[DEFAULT]

`src/command-line/run-many/run-many.js:17`, `run/run-one.js:19`, and `affected/affected.js:19` all set `loadDotEnvFiles: process.env.NX_LOAD_DOT_ENV_FILES !== 'false'`, and `src/tasks-runner/run-command.js:666` stamps `process.env.NX_LOAD_DOT_ENV_FILES = 'true'` for the run. Rasm's `nx.json` does not set the variable and the repository has no `.env` file, and the machinery is on and idle.

### [10.4]-[VARIABLE_OWNERS]

Shell and tool discovery variables, set before any tool runs and reaching an editor's language server and a `dotnet build` typed at a prompt, belong in mise `[env]`: `DOTNET_ROOT` and `DOTNET_MULTILEVEL_LOOKUP` (set by the dotnet plugin), `PATH`, `DOTNET_NOLOGO`, `DOTNET_CLI_TELEMETRY_OPTOUT`, and `NX_WORKSPACE_DATA_DIRECTORY` (read at graph construction, before any task). Nx sets variables only for a task it runs, and an editor's Roslyn server, the `nx` process itself, and a hand-typed `dotnet` never pass through `getTaskSpecificEnv`.

Repository configuration with a manifest spelling belongs in the owning manifest: `UV_CACHE_DIR`, `UV_PYTHON_PREFERENCE`, and `UV_PYTHON_DOWNLOADS` (`[tool.uv] cache-dir`, `python-preference`, `python-downloads`), and `MACOSX_DEPLOYMENT_TARGET` where a build file can carry it. One owner per fact holds, and an environment variable beside the manifest outranks it and hides it.

Task runtime variables, needed only while a target executes, belong to Nx through the target's `env` option or a `.env.<target>` file: `VCPKG_DEFAULT_BINARY_CACHE`, `NODE_ENV`, per-target flags. Nx loads, hashes, and scopes them to one target, and a copy in `[env]` is a lower-precedence duplicate that Nx overrides.

CI secrets (registry tokens, `GITHUB_TOKEN`, cloud credentials) belong to the CI provider's secret store, injected as job environment (Doppler through IaC per the plan). A repository config file is the wrong place for a secret.

Machine-only paths (`GDAL_CONFIG`, `GDAL_DATA`, `PROJ_*`, `DYLD_FALLBACK_LIBRARY_PATH`, `PUPPETEER_EXECUTABLE_PATH`) belong to the machine (Parametric_Forge). Each names a prefix inside a native package the repository does not install.

No variable is declared in two tools: `VCPKG_DEFAULT_BINARY_CACHE` belongs to the Nx target that runs vcpkg, `DOTNET_ROOT` comes from the dotnet plugin, and the uv variables become `[tool.uv]` settings and leave Forge's environment.

## [11]-[REPOSITORIES]

GitHub code search on 2026-09-03 found the following repositories, and `gh api repos/<owner>/<repo>` supplied star counts and `pushed_at` on the same day.

### [11.1]-[renovatebot/renovate]

22,400 stars, pushed 2026-09-03. `mise.toml` at `main`, the whole file:

```toml
[tools]
node = "24.20.0"
pnpm = "11.24.0"
uv   = "0.12.9"

[hooks]
postinstall = '''
pnpm install
uv sync
'''

[settings]
# needed for hooks
experimental = true
```

The node, pnpm, and uv triple Rasm needs, with exact patch pins. mise installs the package managers, `pnpm install` and `uv sync` own the packages, and `[hooks] postinstall` chains them at the cost of `experimental = true`.

### [11.2]-[pulumi/pulumi]

25,640 stars, pushed 2026-09-03. `.mise.toml` at `master`, excerpt:

```toml
[tools]
node = "22"
"npm:pnpm" = "11"
python = '3.11'
uv = "0.11.28"
dotnet = "8"

[env]
PULUMI_TEST_USE_NPM = "true"
```

Pins node, pnpm, python, uv, and the .NET SDK in one file (as do baml and omes), reaches pnpm through `npm:`, and uses `[env]` for one test flag.

### [11.3]-[BoundaryML/baml]

9,139 stars, pushed 2026-09-03. `mise.toml` at `canary`, excerpt:

```toml
[tools]
python = "3.13"     # Will use uv for Python package management
node = "22"         # For pnpm
dotnet = "10.0.301"
uv = "0.11.3"
cmake = "4.3.3"
ninja = "1.13.2"
"npm:pnpm" = "11.1.3"
"pipx:ruff" = "0.15.22"
ast-grep = "0.44.1"
"cargo:cargo-shear" = { version = "latest", install_env = { RUSTUP_TOOLCHAIN = "1.97.1" } }
```

The closest match to Rasm's tool list: .NET 10, uv, pnpm, `cmake`, and `ast-grep`, with the per-tool `install_env` option. Its comments state the division, `python` is there because uv manages packages and `node` "For pnpm". Rasm differs on the rows `cmake` (vcpkg owns it) and `pipx:ruff` (`uv.lock` owns it).

### [11.4]-[get-convex/convex-backend]

12,476 stars, pushed 2026-09-03. `mise.toml` at `main`, header comment and settings:

```toml
# Tool versions for Convex local dev + CI, managed by mise (https://mise.jdx.dev).
# Node is NOT here -- it stays in .nvmrc (via idiomatic_version_file in [settings]),
# the shared source of truth for the builder image, copybara, and the OSS repo.
min_version = "2026.8.9"

[settings]
# Read Node's version from the existing .nvmrc.
idiomatic_version_file_enable_tools = ["node"]
# Commit mise.lock so binaries are checksum-pinned across platforms.
lockfile = true
```

The same boundary Rasm draws: the installer holds only what nothing else holds, idiomatic version files stay the source of truth, `mise.lock` is committed, and `min_version` makes an old mise binary refuse the config. Its `[env]` holds three build-discovery variables with mise's Tera templating, for example `ROCKSDB_LIB_DIR = "{% if os() == 'macos' %}/opt/homebrew/lib{% elif os() == 'linux' %}/usr/lib{% endif %}"`.

### [11.5]-[foxminchan/BookWorm]

503 stars, pushed 2026-08-28. `mise.toml` at `main`, excerpt:

```toml
[tools]
node = "24"
dotnet = "10"

[settings]
task.run_auto_install = false
idiomatic_version_file_enable_tools = ['bun', 'dotnet']
```

Its `global.json` pins `"version": "10.0.400"` with `"rollForward": "latestFeature"`, `"allowPrerelease": true`, and the same `"test": { "runner": "Microsoft.Testing.Platform" }` block as Rasm. Same SDK string, same runner block, same `.slnx` solution model.

### [11.6]-[mehdihadeli/food-delivery-microservices]

1,007 stars, pushed 2026-07-18. `mise.toml` at `main`:

```toml
[tools]
node = "24"
dotnet = "10"

[settings]
task.run_auto_install = false
idiomatic_version_file_enable_tools = ['dotnet']

[tasks.restore]
description = "Restore NuGet packages and local tools"
run = ["dotnet restore food-delivery-microservices.slnx", "dotnet tool restore"]
```

The `[tasks.restore]` body is the pair `eng/scripts/provision.py:387` runs (`dotnet tool restore`) with the `dotnet restore <solution>.slnx` Rasm's `README.md` prescribes. In Rasm both are Nx targets.

### [11.7]-[temporalio/omes]

101 stars, pushed 2026-09-02. `mise.toml` at `main`, excerpt:

```toml
[tools]
dotnet = "8.0.100"
node = "24"
npm = "11.17.0"
pnpm = "10.32.1"
python = "3.10"
uv = "0.7.19"
buf = "1.72.0"
```

Node, pnpm, python, uv, and the .NET SDK with exact pins, with `buf`, which Rasm holds in the pnpm catalog (`@bufbuild/buf`, `pnpm-workspace.yaml:280`) and leaves out of `mise.toml`.

### [11.8]-[Azure/typespec-azure]

27 stars, pushed 2026-09-03. `mise.toml`:

```toml
[tools]
python = "3.12"
uv = "latest"
node = "26"

[settings]
idiomatic_version_file_enable_tools = ["pnpm"]
```

It takes pnpm's version from `package.json` (mise's `pnpm.toml` lists `idiomatic_files = ["package.json"]`) rather than restating it.

### [11.9]-[siemens/element]

78 stars, pushed 2026-09-03. `mise.toml` is three lines: `node = "24"`, `pnpm = "11"`, `uv = "0.12"`.

### [11.10]-[COMMON_TRAITS]

1. One `mise.toml` at the repository root, none per project and no `conf.d`
2. The installer stops at package managers, every repository installs `pnpm` and `uv` and lets them own packages
3. `[env]` is small or absent: pulumi one variable, convex three build-discovery variables, the other six none
4. `idiomatic_version_file_enable_tools` reuses an existing pin
5. No aqua, proto, vfox, devbox, or pkgx equivalent turned up in the search

convex reuses `.nvmrc`, typespec-azure `package.json`, and BookWorm and food-delivery-microservices `global.json`.

### [11.11]-[PRECEDENCE]

BookWorm and food-delivery-microservices declare both `dotnet = "10"` in `[tools]` and `idiomatic_version_file_enable_tools = ['dotnet']`. In mise's config loader (`src/config/mod.rs` at `v2026.9.1`, `Config::load`, lines 309-314) idiomatic filenames are chained before `DEFAULT_CONFIG_FILENAMES`, and the ordering comment at line 1816 reads "(later wins, matching LOCAL_CONFIG_FILENAMES ordering)". `mise.toml` `[tools]` outranks `global.json`, and in those repositories the `global.json` reading is a fallback.

For Rasm, `global.json` stays the single source of the SDK version, `[settings]` sets `idiomatic_version_file_enable_tools = ["dotnet", "python"]`, and `[tools]` never holds `dotnet`. `mise cfg` and `mise current dotnet` confirm the resolution before the file is committed.

## [12]-[SKILLS]

### [12.1]-[OFFICIAL_MATERIAL]

No official Claude skill for mise exists. `jdx/mise` at `v2026.9.1` includes the following artifacts, none a skill:

- `llms.txt` (29,098 bytes) and `docs/public/llms.txt` (38,453 bytes), one flat reference for any assistant reading a repository or the docs site
- The MCP server `mise mcp` (`docs/mcp.md`), for an assistant that needs live state
- Contributor instructions in `AGENTS.md`, `CLAUDE.md`, `.claude/`, `.cursor/`, `.codex/`, for developing mise itself

The `llms.txt` files cover what mise is, install methods, config file names, tools, tasks, and environments. They are documentation and say what mise is, not what an agent does when it sees a `mise.toml` beside `pnpm-workspace.yaml`. The MCP server exposes the resources `mise://tools`, `mise://tasks`, `mise://env`, `mise://config`, one working tool `run_task`, `install_tool` "not yet implemented", and requires `MISE_EXPERIMENTAL=1`.

### [12.2]-[COMMUNITY_SKILLS]

Both visible community skills were read at their default branches on 2026-09-03.

`TheBushidoCollective/han` (191 stars, pushed 2026-08-25), `plugins/tools/mise/skills/tool-management/SKILL.md`, frontmatter `name: mise-tool-management`, `user-invocable: false`, `description: Use when managing development tool versions with Mise. Covers installing tools, version pinning, and replacing language-specific version managers.`, with an `allowed-tools` list. It gets right a triggering description written as a condition, `user-invocable: false`, and `mise install`, `mise use`, and `[tools]` mechanics. It lacks `mise.lock`, backend syntax, `[env]`, `idiomatic_version_file_enable_tools`, monorepo guidance, and the rule against duplicating another package manager's pins.

`sickn33/agentic-awesome-skills` (45,896 stars, an aggregator), `skills/mise-configurator/SKILL.md`, `author: community`, `date_added: "2026-04-16"`. A generation procedure: detect project files, emit a `mise.toml`, emit `mise trust` with `mise install`, optionally emit CI. It reuses detected pins and names `mise trust` as a required step. It treats `mise.toml` as a file to generate rather than a boundary against `pnpm-workspace.yaml` and `uv.lock`.

### [12.3]-[RASM_REFERENCE_MATERIAL]

Neither community skill is adoptable under Rasm's standards (one owner per fact, behavior instruction, placement rules). The material worth carrying into the mise reference of the monorepo skill:

1. `mise trust` as the first step on a fresh clone (`docs/security.md`)
2. The backend table and which backend each tool comes from (`docs/dev-tools/backends/index.md`, `registry/*.toml`)
3. What `mise.lock` records per backend (`docs/dev-tools/mise-lock.md`)
4. The boundary: the installer pins runtimes and standalone binaries, and every package manager keeps pinning its own packages
5. The precedence of `[tools]` over an idiomatic file

## [13]-[VERDICT]

mise `v2026.9.1` is chosen as the only candidate that installs all five runtimes, reads `global.json`, has a per-directory `[env]`, a committed lockfile, an official action that exports env and `PATH`, and an editor extension. Its gaps: `[env]` reaches an editor only through shims, `core:dotnet` has no lockfile entry, `global.json` parsing reads `sdk.version` alone, a fresh clone runs `mise trust`, and native library paths stay machine state.

- aqua `v2.62.3`: CLI binaries alone, no `python` and no `dotnet` package, no environment section, no built-in CI caching
- proto `v0.61.2`: runtimes for node, pnpm, python, uv, no built-in .NET, a lockfile self-declared unstable, `0.x`
- devbox `0.18.0`: reproducible through Nix, requires Nix on every machine, `global.json`'s `10.0.400` not expressible
- nix flake: maximum reproducibility, the same nixpkgs constraint, duplicates Parametric_Forge
- pkgx `v2.11.0` with `dev` `v1.8.1`: zero-config activation, `dev` last released 2025-05-07, no config file, lockfile, or `[env]`
- vfox `v1.0.11`: cross-platform plugins, no `pnpm` or `uv` plugin, dotnet SDKs 6.0 to 8.0, no action, lockfile, `[env]`, or monorepo model
- asdf `v0.20.0`: `.tool-versions` universally understood, no environment variables, no lockfile, no `dotnet:`, `npm:`, or `pipx:` backends

The pkgx .NET package does not read `global.json`.

### [13.1]-[ROOT_MISE_TOML]

`[tools]` holds `node`, `pnpm`, `python`, `uv`, `act`, and `actionlint` at exact patch versions, and `ratchet` once action pinning is adopted. It never holds `dotnet` (`global.json` owns it), `ruff`, `ty`, or `mypy` (`uv.lock`), `biome`, `ast-grep`, `buf`, or `nx` (the pnpm catalog), or `cmake` (vcpkg through `provision.py`).

- `[settings]`: `idiomatic_version_file_enable_tools = ["dotnet", "python"]`, `lockfile = true`, `min_version` at the adopted release
- `[env]`: `DOTNET_NOLOGO`, `DOTNET_CLI_TELEMETRY_OPTOUT`, `NX_WORKSPACE_DATA_DIRECTORY = ".cache/nx/workspace-data"`
- `mise.lock`, committed
- No `[tasks]` and no `[hooks]`, Nx owns the task graph (`nx.json`), and `pnpm install` and `uv sync` run inside Nx targets
- `.python-version` holds `3.15`, read by both mise (idiomatic file) and uv
- `.vscode/settings.json` keeps its hand-written entries, and the extension's auto-configuration stays off
- CI: `jdx/mise-action@v4` with the defaults `install`, `cache`, `env`, `export_path`, and `MISE_TRUSTED_CONFIG_PATHS` set to the workspace

`DOTNET_ROOT` and `DOTNET_MULTILEVEL_LOOKUP` need no `[env]` entry because the dotnet plugin sets them, and the uv variables become `[tool.uv]` settings in `pyproject.toml`.

The Forge removal note records the couplings that leave the machine once this lands: `DOTNET_ROOT`, `DOTNET_NOLOGO`, `DOTNET_CLI_TELEMETRY_OPTOUT`, `UV_CACHE_DIR`, `UV_PYTHON_PREFERENCE`, `UV_PYTHON_DOWNLOADS`, the Python shims in `python-tools.nix`, and the Node, pnpm, and uv packages Forge installs.

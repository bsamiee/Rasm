<!-- Source for monorepo-build-infrastructure [02]-[TOOLCHAIN] and [03]-[CONFIGURATION], nothing integrated yet -->
# uv capabilities for the Rasm Python area

Versions and documentation were read on 2026-09-03.

## [00]-[BASELINE]

Newest stable uv is `0.12.9`, published 2026-09-01T21:58:26Z (<https://github.com/astral-sh/uv/releases/tag/0.12.9>), and Rasm has `0.12.5 (aarch64-apple-darwin)` installed (`uv --version`). Every doc quote is from `refs/tags/0.12.9` of `astral-sh/uv` (`https://raw.githubusercontent.com/astral-sh/uv/0.12.9/docs/<path>`). The settings and environment references are the rendered pages (<https://docs.astral.sh/uv/reference/settings/>, <https://docs.astral.sh/uv/reference/environment/>), generated and absent from the tagged tree.

### [00.1]-[REPOSITORY]

`pyproject.toml` holds:
- `[project] name = "rasm-workspace"`, `version = "0.0.0"`, `requires-python = ">=3.15"` (`:2-4`)
- No `[project.dependencies]`, no `[build-system]`, no `[tool.uv.workspace]`
- Dependency groups `workspace` (310 entries, `:7-336`) and `dev` (35 entries, `:337-372`)
- `[tool.uv] default-groups = ["workspace", "dev"]` (`:382-383`)
- `[tool.uv.sources]` (`:377`) with one `git` rev pin (beartype) and two `url` archive pins (connectrpc, protoc-gen-connectrpc)
- `[tool.uv.config-settings-package]` (`:386`) for scipy and `[tool.uv.extra-build-variables]` (`:390`) for opencolorio
- Four `[[tool.uv.dependency-metadata]]` entries (`:394`, `:401`, `:427`, `:448`), each with a `version`
- The entries are colour-cxf 0.1.1, colour-science 0.4.7, pytensor 2.35.1, substrait 0.30.0
- 29 requirements in the `workspace` group with `python_version<'3.15'` markers (`:97-256`), none in `dev`
- `[tool.mypy] plugins = ["pydantic.mypy"]` (`:513`), and `[tool.mypy] exclude` lists `^dist/`
- `[tool.ty.environment] root = ["."]` (`:467`), and `[tool.ty.src] exclude` lists `dist`
- `[tool.pytest] pythonpath = ["."]` (`:564`), `addopts` loading `-p tests.python.support.runtime`, `required_plugins` naming eight plugins
- `[tool.ruff] required-version = ">=0.16.2"` (`:641`), and `extend-exclude` omits `dist`

`uv.lock` has `version = 1`, `revision = 3`, `requires-python = ">=3.15"`, four `resolution-markers`, a `[manifest]` table replaying the four `dependency-metadata` overrides, and 9435 lines.

`README.md` states that root manifests own versions, `.cache/` holds caches, `.artifacts/` holds outputs, every action is an Nx target, and scripts under `eng/scripts/` run under `uv run`.

`apps/README.md` (`:38-39`) states "Root `pyproject.toml` owns resolution, dependency groups, and `uv.lock`, member manifests carry membership and bare-name dependencies" and "`tool.uv.workspace.members` includes `libs/python/*` by glob and each app project by explicit entry, because a glob over mixed-language trees fails".

`eng/project.json` holds the one Python Nx target: `"provision": { "command": "uv run python -m eng.scripts.provision", "cache": false, "parallelism": false }`.

`eng/scripts/stage.py` imports `from eng.scripts.provision import ...` (`:25`), and `eng.scripts` resolves as an implicit namespace package from the repository root (matched by `pythonpath = ["."]` and the ruff `implicit-namespace-package` ignore, `pyproject.toml:700`). Both scripts use `cyclopts` (`stage.py:20`, `provision.py:14`).

On disk, `libs/python/` holds `.gitkeep`, `apps/` holds `README.md`, no `.python-version` and no `uv.toml` exist, and `.venv/` exists. `.venv/pyvenv.cfg` reads `home = /nix/store/w54namv1ckp7803ww08j8ihqrgg5rmx0-python3-3.15.0rc1/bin`, `implementation = CPython`, `uv = 0.12.5`, `version_info = 3.15.0rc1`, a Nix store CPython.

### [00.2]-[INTERPRETER_OWNER]

The shell that runs uv exports the following from Parametric_Forge (`modules/common/toolchain-env.nix:49-51`):

```nix
UV_CACHE_DIR = "${xdgCacheHome}/uv";
UV_PYTHON_PREFERENCE = "only-system";
UV_PYTHON_DOWNLOADS = "never";
```

`modules/home/programs/languages/python-tools.nix` installs project-first shims for `python`, `python3`, `ruff`, `ty`, and `mypy` that exec the project `.venv` (or `$UV_PROJECT_ENVIRONMENT`) inside a project root. `docs/atlas/interconnection.md:44` records why the shims never invoke `uv run`: "an implicit `uv run` deadlocks against an external uv holding the project lock ... and materializes the full dependency set as a side effect of incidental probes. Provisioning is explicit via `uv sync`."

The plan settles the owner: mise pins python (with node, pnpm, and uv), the uv variables and the shims are couplings to cut, and caches route under `.cache/`.

## [01]-[WORKSPACES]

Source: `docs/concepts/projects/workspaces.md` at tag 0.12.9.

### [01.1]-[DEFINITION]

> a workspace is "a collection of one or more packages, called _workspace members_, that are managed together."

> In a workspace, each package defines its own `pyproject.toml`, but the workspace shares a single lockfile, ensuring that the workspace operates with a consistent set of dependencies.

> `uv lock` operates on the entire workspace at once, while `uv run` and `uv sync` operate on the workspace root by default, though both accept a `--package` argument

### [01.2]-[MEMBERS]

> In defining a workspace, you must specify the `members` (required) and `exclude` (optional) keys, which direct the workspace to include or exclude specific directories as members respectively, and accept lists of globs

```toml
[tool.uv.workspace]
members = ["packages/*"]
exclude = ["packages/seeds"]
```

> Every directory included by the `members` globs (and not excluded by the `exclude` globs) must contain a `pyproject.toml` file.

> Every workspace needs a root, which is _also_ a workspace member.

A member always needs its own `pyproject.toml`, the mechanical reason behind `apps/README.md`: `apps/*/*` matches C# and TypeScript directories with no manifest, app members are enumerated explicitly, and `libs/python/*` is globbed. `members` and `exclude` "Support both globs and explicit paths" and "If a package matches both `members` and `exclude`, it will be excluded" (settings reference), and `members = ["libs/python/*", "apps/<app>/<project>"]` is a legal mixed list.

### [01.3]-[SOURCES_INHERITANCE]

> Any `tool.uv.sources` definitions in the workspace root apply to all members, unless overridden in the `tool.uv.sources` of a specific member.

> If a workspace member provides `tool.uv.sources` for some dependency, it will ignore any `tool.uv.sources` for the same dependency in the workspace root, even if the member's source is limited by a [marker] that doesn't match the current platform.

The three source pins in the root reach every member without restatement, a member restating one silently wins, and members carry no `[tool.uv.sources]`. `configuration-files.md` says uv "will begin its search at the workspace root, ignoring any configuration defined in workspace members", and the two pages agree once `tool.uv.sources` is read as member-scoped project metadata and the rest of `[tool.uv]` as workspace-root configuration. The rule is the same either way: members declare no uv configuration.

### [01.4]-[MEMBER_DEPENDENCY]

> To declare a dependency on a workspace member, add the member name with `{ workspace = true }`. All workspace members must be explicitly stated. Workspace members are always editable. (`dependencies.md`)

### [01.5]-[ALL_PACKAGES]

`uv help sync` (verified on 0.12.5): `--all-packages` "Sync all packages in the workspace. The workspace's environment (`.venv`) is updated to include all workspace members.", and `--package <PACKAGE>` "Sync for specific packages in the workspace". Without `--all-packages`, `uv sync` at the root installs the root project, its dependencies, and the default groups, and members that are not dependencies of the root stay uninstalled. `uv lock` covers the whole workspace regardless.

### [01.6]-[REQUIRES_PYTHON]

> uv's workspaces enforce a single `requires-python` for the entire workspace, taking the intersection of all members' `requires-python` values.

Every member declares `requires-python = ">=3.15"`, the root's string, and the intersection never narrows.

### [01.7]-[UNSUITED_CASES]

> Workspaces are _not_ suited for cases in which members have conflicting requirements, or desire a separate virtual environment for each member. In this case, path dependencies are often preferable.

> As Python does not provide dependency isolation, uv can't ensure that a package uses its declared dependencies and nothing else. For workspaces specifically, uv can't ensure that packages don't import dependencies declared by another workspace member.

### [01.8]-[SETTLED_SHAPE]

A member needs its own `pyproject.toml`, and the owner's rule permits one for a package other code imports. The facts that fix the form:

`libs/python/` holds no package yet, and the workspace table is added with the first member, because a glob matching nothing is undocumented.

A member `pyproject.toml` is membership metadata: `[project]` with `name`, `version`, `requires-python`, and bare-name `dependencies`, nothing else.

`[build-system]` decides importability, membership does not: "uv uses the presence of a build system to determine if a project contains a package that should be installed in the project virtual environment. If a build system is not defined, uv will not attempt to build or install the project itself, just its dependencies" (`config.md:104`). A member without one is the "virtual" case in `dependencies.md`: "Workspace members that are _not_ dependencies can be virtual by default ... the `child` workspace member would not be installed, but the transitive dependency `anyio` would be." A `libs/python/<pkg>` other code imports declares `[build-system]`, and publication is a separate, excluded question.

Intra-repository edges are `[tool.uv.sources] <pkg> = { workspace = true }` at the root.

`eng/scripts` is neither a package nor a member. `eng/project.json` runs it as a module from the repository root, resolving `eng.scripts` as an implicit namespace package through `pythonpath = ["."]`. `uv help run` documents only the environment and discovery halves ("When used in a project, the project environment will be created and updated before invoking the command", "the project or workspace is discovered from the current working directory"). A member manifest under `eng/` puts build automation into the resolved graph for no gain.

The root-only shape (today) requires nothing new and buys one manifest, with no per-package dependency declaration, nothing importable outside the repository root, and no Python dependency edge for `nx affected`. The workspace shape (settled) requires one minimal `pyproject.toml` per `libs/python/*` package and per Python app project, `members` listing `libs/python/*` by glob and app projects explicitly, and `{ workspace = true }` sources at the root. It buys per-package dependency sets, editable installs, `uv run --package`, `uv sync --all-packages`, dependency edges Nx can read, and `[project.scripts]` where a package needs one.

## [02]-[DEPENDENCIES]

Sources: `docs/concepts/projects/dependencies.md`, `docs/concepts/projects/config.md`, `docs/concepts/projects/sync.md` at 0.12.9, and the rendered settings reference.

### [02.1]-[FIELDS]

> - `project.dependencies`: Published dependencies. - `project.optional-dependencies`: Published optional dependencies, or "extras". - `dependency-groups`: Local dependencies for development. - `tool.uv.sources`: Alternative sources for dependencies during development.

The Rasm root uses only `[dependency-groups]`: nothing is published, and "development dependencies are local-only and will _not_ be included in the project requirements when published".

### [02.2]-[GROUPS]

> uv uses the `[dependency-groups]` table (as defined in PEP 735) for declaration of development dependencies.

> The `dev` group is special-cased; there are `--dev`, `--only-dev`, and `--no-dev` flags ... the `dev` group is synced by default.

> Once groups are defined, the `--all-groups`, `--no-default-groups`, `--group`, `--only-group`, and `--no-group` options can be used to include or exclude their dependencies.

> `--only-group` are the same as `--only-dev`, the project will not be included. However, `--only-group` will also exclude default groups. (`sync.md`)

> Group exclusions always take precedence over inclusions, so given the command: `uv sync --no-group foo --group foo` ... The `foo` group would not be installed. (`sync.md`)

> uv requires that all dependency groups are compatible with each other and resolves all groups together when creating the lockfile.

`workspace` and `dev` resolve as one solution. Group nesting exists (`dev = [{include-group = "lint"}, {include-group = "test"}]`, "An included group's dependencies cannot conflict with the other dependencies declared in a group"), and the plan keeps ruff, ty, mypy, and pytest in the `dev` group as it is, because splitting changes nothing about resolution.

### [02.3]-[DEFAULT_GROUPS]

> By default, uv includes the `dev` dependency group in the environment ... The default groups to include can be changed using the `tool.uv.default-groups` setting.

Reference: "Can also be the literal `"all"` to default enable all groups. Default value: `["dev"]`." The explicit Rasm list equals `"all"` today and keeps a later non-default group possible.

### [02.4]-[GROUP_REQUIRES_PYTHON]

> If a dependency group requires a different range of Python versions than your project, you can specify a `requires-python` for the group in `[tool.uv.dependency-groups]`

Reference: "Currently this can only be used to add `requires-python` constraints to dependency groups". It raises a floor and cannot lower a ceiling, and it leaves the 3.15 marker set untouched.

### [02.5]-[EXTRAS_AND_CONFLICTS]

"Optional dependencies are specified in `[project.optional-dependencies]` ... Extras are requested with the `package[<extra>]` syntax", and "uv does not sync extras by default. Use the `--extra` option ... To quickly enable all extras, use the `--all-extras` option." Rasm uses extras only on the consuming side (`confluent-kafka[schemaregistry,avro,json,protobuf]`, `hishel[httpx]`, `moto[server]`, `substrait[extensions]`, `validate-pyproject[all]`). `[tool.uv] conflicts = [[{ group = "group1" }, { group = "group2" }]]` is the escape hatch when two sections cannot resolve together.

### [02.6]-[SOURCES]

Source kinds: index, git, URL, path, workspace. "Sources are only respected by uv. If another tool is used, only the definitions in the standard project tables will be used."

- Git: `git` with one of `tag`, `branch`, `rev`, and an optional `subdirectory`
- URL: a `https://` wheel or source distribution, "A `subdirectory` may be specified if the source distribution isn't in the archive root."
- Index: `{ index = "name" }` with `[[tool.uv.index]]`
- Path: `{ path = "..." }`, `editable = true` optional
- Marker-scoped sources and lists of sources disambiguated by `marker`
- `--no-sources` (`UV_NO_SOURCES`) "will also prevent uv from discovering any workspace members that could satisfy a given dependency."

On the index kind, "an `explicit` flag can be included to indicate that the index should _only_ be used for packages that explicitly specify it".

The Rasm beartype entry uses `rev`, immune to the `main`-branch preference rule in `sync.md` ("uv will prefer the locked commit SHA in an existing `uv.lock` file over the latest commit on the `main` branch, unless the `--upgrade` or `--upgrade-package` flags are used"). The two connectrpc entries use the URL kind.

### [02.7]-[DEPENDENCY_METADATA]

Reference: "Pre-defined static metadata for dependencies of the project (direct or transitive). When provided, enables the resolver to use the specified metadata instead of querying the registry or building the relevant package from source." Fields respected: `name`, optional `version`, `requires-dist`, `requires-python`, `provides-extra`, per Metadata 2.3. `config.md`: the `version` field "is optional for registry-based dependencies ... but _required_ for direct URL dependencies (like Git dependencies)."

All four Rasm overrides pin a `version` and are registry packages, narrower than required, the safe direction. Each is replayed into `uv.lock` under `[manifest]`.

### [02.8]-[BUILD_SETTINGS]

`config-settings-package`: "Settings to pass to the PEP 517 build backend for specific packages ... Accepts a map from package names to string key-value pairs." The Rasm `scipy = { setup-args = "-Duse-pythran=false" }` is the documented form.

`extra-build-variables`: "Extra environment variables to set when building certain packages." `config.md:341`: "The use of `extra-build-dependencies` and `extra-build-variables` are tracked in the uv cache, such that changes to these settings will trigger a reinstall and rebuild of the affected packages." Editing the Rasm `opencolorio = { CMAKE_ARGS = "-DOCIO_BUILD_APPS=OFF" }` forces the rebuild with no cache step. The sibling `extra-build-dependencies` supports `{ requirement = "torch", match-runtime = true }`, and `config.md:229` states the preference: "we recommend augmenting the build dependencies rather than disabling build isolation entirely".

### [02.9]-[REQUIRED_VERSION]

Reference: "Enforce a requirement on the version of uv. ... Accepts a PEP 440 specifier". Rasm leaves it unset. It is the analogue of `global.json` and of `[tool.ruff] required-version = ">=0.16.2"`. The settled value is `[tool.uv] required-version = ">=0.12.9"` with a comment naming the reason (the lock schema `version = 1`, `revision = 3`, and the newest stable), raised only when a feature or fix needs it, as Airflow and pydantic-ai do.

### [02.10]-[LOCKFILE]

`layout.md`: "`uv.lock` is a _universal_ or _cross-platform_ lockfile that captures the packages that would be installed across all possible Python markers", "This file should be checked into version control", and "managed by uv and should not be edited manually. The `uv.lock` format is specific to uv and not usable by other tools."

The Rasm header:

```toml
version = 1
revision = 3
requires-python = ">=3.15"
resolution-markers = [
    "platform_machine == 'ARM64' and sys_platform == 'win32'",
    "platform_machine != 'ARM64' and sys_platform == 'win32'",
    "sys_platform == 'emscripten'",
    "sys_platform != 'emscripten' and sys_platform != 'win32'",
]
```

The `resolution-markers` are forks the resolver created, including one for `sys_platform == 'emscripten'`, a platform the README leaves unsupported ("Development targets macOS first, all code and tooling stay portable to Linux and Windows"). The lever is `[tool.uv] environments` ("you can restrict the set of supported environments to improve performance and avoid unsatisfiable branches"), settled as the three README platforms:

```toml
[tool.uv]
environments = ["sys_platform == 'darwin'", "sys_platform == 'linux'", "sys_platform == 'win32'"]
```

`required-environments` is the opposite lever and "is only relevant for packages that do not publish a source distribution (like PyTorch)" (`config.md`).

### [02.11]-[ROOT_ONLY_SETTINGS]

`build-constraint-dependencies`, `constraint-dependencies`, `exclude-dependencies`, and `override-dependencies` are read from the workspace-root `pyproject.toml` alone (reference: "uv will only read ... from the `pyproject.toml` at the workspace root, and will ignore any declarations in other workspace members or `uv.toml` files"). The general guarantee is `configuration-files.md:14`: "In workspaces, uv will begin its search at the workspace root, ignoring any configuration defined in workspace members." uv enforces root ownership of uv settings.

## [03]-[INTERPRETER]

Source: `docs/concepts/python-versions.md` at 0.12.9.

### [03.1]-[MANAGED_VERSUS_SYSTEM]

> uv does not distinguish between Python versions installed by the operating system vs those installed and managed by other tools. For example, if a Python installation is managed with `pyenv`, it would still be considered a _system_ Python version in uv.

A mise-installed interpreter is a system interpreter to uv, with no integration beyond the preference setting. mise and uv install the same artifacts: "uv instead uses pre-built distributions from the Astral `python-build-standalone` project. `python-build-standalone` is also is used in many other Python projects, like Mise".

### [03.2]-[PREFERENCE_AND_DOWNLOADS]

> By default, the `python-preference` is set to `managed` which prefers managed Python installations over system Python installations. However, system Python installations are still preferred over downloading a managed Python version.

Values: `only-managed`, `managed`, `system`, `only-system` ("Only use system Python installations; never use managed Python installations").

> By default, uv will automatically download Python versions when needed. The `python-downloads` option can be used to disable this behavior. By default, it is set to `automatic`; set to `manual` to only allow Python downloads during `uv python install`.

Settled: `[tool.uv] python-preference = "only-system"` and `python-downloads = "manual"` in `pyproject.toml`, replacing the Forge `UV_PYTHON_PREFERENCE` and `UV_PYTHON_DOWNLOADS` exports. With mise owning the interpreter, `uv python install`, `uv python upgrade`, and the managed-version upgrade of virtual environments (a minor-version symlink directory under `~/.local/share/uv/python/`) are unused. The rendered reference documents `automatic` and `manual` for `python-downloads`, `never` is only what Forge exports, and `uv 0.12.5` rejects `--python-downloads never` ("unexpected argument").

### [03.3]-[PYTHON_VERSION_FILE]

> The `.python-version` file can be used to create a default Python version request. uv searches for a `.python-version` file in the working directory and each of its parents.

> uv will not search for `.python-version` files beyond project or workspace boundaries (except the user configuration directory).

> When searching for a system Python version, uv will use the first compatible version — not the newest version.

Under `only-system`, `requires-python` alone selects the first `PATH` entry satisfying `>=3.15`. `.python-version` narrows that and is the idiomatic mise file for python (`registry/python.toml` at `v2026.9.1`: `idiomatic_files = [".python-version", ".python-versions"]`), and one file, `.python-version` holding `3.15`, is read by both tools with `idiomatic_version_file_enable_tools` naming `python`.

### [03.4]-[PRERELEASES]

> Python pre-releases will not be selected by default. Python pre-releases will be used if there is no other available installation matching the request.

The Rasm environment is 3.15.0rc1 today because it is the only interpreter satisfying `>=3.15`. The mise `3.15` pin resolves to the newest 3.15 build available in its release list, and `uv sync` recreates `.venv` once.

### [03.5]-[FREE_THREADING]

> For Python 3.14+, uv will allow use of free-threaded Python 3.14+ interpreters without explicit selection. ... if a free-threaded interpreter comes before a GIL-enabled build on the `PATH`, it will be used.

> If both free-threaded and GIL-enabled Python versions are available ... you can use the `+gil` variant specifier.

`.python-version` accepts `3.15+gil` or `3.15t` when both builds share the machine.

## [04]-[RUNNING]

Sources: `docs/concepts/projects/run.md`, `sync.md`, `config.md`, `docs/guides/scripts.md` at 0.12.9, and `uv help run` (0.12.5).

### [04.1]-[SEMANTICS]

`uv help run`: "When used in a project, the project environment will be created and updated before invoking the command." "When running a script, the project or workspace is discovered from the script's directory. Otherwise, the project or workspace is discovered from the current working directory." "All options to uv must be provided before the command". `sync.md:9`: "Locking and syncing are _automatic_ in uv. For example, when `uv run` is used, the project is locked and synced before invoking the requested command".

### [04.2]-[ESCAPE_FLAGS]

`uv help run`: `--locked` "Assert that the `uv.lock` will remain unchanged ... If the lockfile is missing or needs to be updated, uv will exit with an error" (`UV_LOCKED`), `--frozen` "Sync without updating the `uv.lock` file ... uses the versions in the lockfile as the source of truth" (`UV_FROZEN`), `--no-sync` "Avoid syncing the virtual environment. Implies `--frozen`" (`UV_NO_SYNC`), and `--no-project` "Avoid discovering the project or workspace". Strictest to loosest: `--locked`, `--frozen`, `--no-sync`. `--isolated` forces a fresh environment. `sync.md:94-108`: `uv sync` is exact by default (removes packages absent from the lock), `uv run` is inexact, and `uv run --exact` restores exactness.

### [04.3]-[GROUP_FLAGS]

`--no-group` "always takes precedence over default groups, `--all-groups`, and `--group`", and `--only-group` "Implies `--no-default-groups`" and omits the project. `uv run --only-group dev ruff check` installs the `dev` group alone, skipping the 310-entry `workspace` group.

### [04.4]-[WITH]

"The `--with` option is used to include a dependency for the invocation" (`run.md:29`), and in a project "these dependencies will be included _in addition_ to the project's dependencies" (`scripts.md:134`). `--with` resolves outside `uv.lock` and never appears in a committed Nx target.

### [04.5]-[SCRIPTS]

`scripts.md`: "The `dependencies` field must be provided even if empty.", "When using inline script metadata, even if `uv run` is used in a _project_, the project's dependencies will be ignored. The `--no-project` flag is not required.", `uv lock --script example.py` "will create a `.lock` file adjacent to the script (e.g., `example.py.lock`)", and `#!/usr/bin/env -S uv run --script` makes a file executable.

Inline metadata is the wrong shape for `eng/scripts/`: it puts a dependency list inside a `.py` file and a second lockfile beside it, against the rule that every dependency is recorded once in the root `pyproject.toml` and `uv.lock`.

### [04.6]-[ENTRY_POINTS]

`config.md:35-49`: `[project.scripts]` entries run as `uv run hello`, and "Using the entry point tables requires a build system to be defined." The root has no `[build-system]` and gains none, because a root named `rasm-workspace` with no modules has nothing to build.

`uv help run`: `-m, --module` "Run a Python module. Equivalent to `python -m <module>`." The settled command for `eng/project.json` is `uv run -m eng.scripts.provision`, the direct form.

### [04.7]-[ENG_SCRIPTS]

1. `eng/scripts/*.py` are modules, and `stage.py:25` imports `from eng.scripts.provision import ...`
2. That import needs `eng.scripts` on `sys.path` from the repository root, `uv run` from the root supplies it, `uv run --script` does not
3. Their dependencies (`anyio`, `cyclopts`, `msgspec`, `structlog`) sit in the root `workspace` group
4. `uv run` locks and syncs before running, and a target that invokes it needs no separate install step
5. A cached target passes `--locked`, and the lock state is an assertion rather than a side effect:

```json
{ "command": "uv run --locked -m eng.scripts.stage {args.library}" }
```

`eng:provision` keeps the unqualified `uv run -m eng.scripts.provision`, the one place the lock and environment update.

Facts for target authoring: `--env-file <ENV_FILE>` "Load environment variables from a `.env` file.", and signal forwarding (`run.md:90-96`): "uv does not cede control of the process to the spawned command ... On Unix systems, uv will forward most signals (with the exception of SIGKILL, SIGCHLD, SIGIO, and SIGPOLL) to the child process. ... uv will only forward a SIGINT to the child process if it is sent more than once or the child process group differs from uv's."

## [05]-[TOOLS]

Source: `docs/concepts/tools.md` at 0.12.9.

> Tools are Python packages that provide command-line interfaces.

> a `uvx` alias is provided for `uv tool run` — the two commands are exactly equivalent.

> Tools can also be installed with `uv tool install`, in which case their executables are available on the `PATH` — an isolated virtual environment is still used, but it is not removed when the command completes.

> In most cases, executing a tool with `uvx` is more appropriate than installing the tool.

### [05.1]-[DEV_GROUP]

> If the tool should not be isolated from the project, e.g., when running `pytest` or `mypy`, then `uv run` should be used instead of `uv tool run`.

> The invocation `uv tool run <name>` (or `uvx <name>`) is nearly equivalent to: `uv run --no-project --with <name> -- <name>`

`--no-project` disqualifies all four:
- `mypy` — `[tool.mypy] plugins = ["pydantic.mypy"]` loads a plugin from the environment, `strict` resolves each `py.typed` and the `dev` stubs
- `ty` — `[tool.ty.environment] root = ["."]` resolves imports against the environment
- `pytest` — `required_plugins` names eight plugins, `addopts` loads `-p tests.python.support.runtime`, tests import the code under test
- `ruff` — needs no environment, and `required-version = ">=0.16.2"` with the whole `[tool.ruff]` block sits in `pyproject.toml`

The `dev` group already carries ruff, and two version records for one tool is what the rule avoids.

A second reason: "Each tool environment is linked to a specific Python version ... but will ignore non-global Python version requests like `.python-version` files and the `requires-python` value from a `pyproject.toml`." A `uv tool`-installed mypy can run on an interpreter other than 3.15.

### [05.2]-[VERSION_SEMANTICS]

`uv tool install` installs the latest unless a version is given, `uvx` uses the latest "on the first invocation" then the cached version, `{package}@{version}` and `@latest` are honored, upgrades respect the constraints and settings given at install time, "Executables provided by dependencies of tool packages are not installed.", and "Installation of tools will not overwrite executables in the executable directory that were not previously installed by uv."

### [05.3]-[PIN_COMPARISON]

`uv tool install X==V` records the version from an imperative command in the tool environment metadata, with no repository file. Both manifest pairs are committed, and the Python pins are `requires-python` and `.python-version`.

| [PROPERTY]                   | [`uv tool install X==V`]  | [GROUP ENTRY]                   | [MISE]                                     |
| :--------------------------- | :------------------------ | :------------------------------ | :----------------------------------------- |
| Version record               | Tool environment metadata | `pyproject.toml` with `uv.lock` | `mise.toml` with `mise.lock`               |
| Clean-checkout reproduction  | No                        | Yes, `uv sync`                  | Yes, `mise install`                        |
| Sees the project environment | No                        | Yes                             | No                                         |
| Python pins honored          | No                        | Yes                             | For the interpreter, via `.python-version` |

The group entry is the only one that puts the version in `pyproject.toml` and `uv.lock`, the plan's rule. mise never names `ruff`, `ty`, or `mypy`, and the Forge shims that resolve them to `.venv` leave the machine. No `[tool.uv]` table holds tool settings, `uv tool` is configured by flags and `UV_TOOL_DIR` / `UV_TOOL_BIN_DIR`.

## [06]-[CACHES_AND_OUTPUTS]

Sources: `docs/concepts/cache.md`, `docs/reference/storage.md` at 0.12.9, and the rendered environment reference.

### [06.1]-[CACHE_DIRECTORY]

> uv determines the cache directory according to, in order: 1. A temporary cache directory, if `--no-cache` was requested. 2. The specific cache directory specified via `--cache-dir`, `UV_CACHE_DIR`, or `tool.uv.cache-dir`. 3. A system-appropriate cache directory, e.g., `$XDG_CACHE_HOME/uv` or `$HOME/.cache/uv` on Unix

> uv _always_ requires a cache directory. ... In most cases, `--refresh` should be used instead of `--no-cache`

> It is important for performance for the cache directory to be located on the same file system as the Python environment uv is operating on. Otherwise, uv will not be able to link files from the cache into the environment and will instead need to fallback to slow copy operations.

Settled: `[tool.uv] cache-dir = ".cache/uv"` in `pyproject.toml`, on the same volume as `.venv`, and Forge stops exporting `UV_CACHE_DIR` (the variable outranks the setting: `--cache-dir` beats `UV_CACHE_DIR` beats `tool.uv.cache-dir`). The cost is a per-clone cache in place of a machine-wide one, the same trade the pnpm store already makes.

### [06.2]-[PROJECT_ENVIRONMENT]

`config.md:164-196`: `UV_PROJECT_ENVIRONMENT` configures the environment path, a relative path resolves against the workspace root, an absolute path shared across projects is "only recommended for use for a single project in CI or Docker images", uv "does not read the `VIRTUAL_ENV` environment variable during project operations", and `--active` opts in. No `pyproject.toml` setting holds the path. `layout.md:34`: `.venv` "is stored inside the project to make it easy for editors to find". `.venv` stays at the root as a written environment rather than build output. The `centralized-project-envs` preview stores it in the cache with a `.venv` link (`layout.md:63`) and stays off.

### [06.3]-[CACHE_COMMANDS]

> `uv cache clean` removes _all_ cache entries ... `uv cache clean ruff` removes all cache entries for the `ruff` package ... `uv cache prune` removes all _unused_ cache entries and all centralized project environments. ... `uv cache prune` is safe to run periodically

> Note that it's _never_ safe to modify the cache directly

> It's safe to run multiple uv commands concurrently, even against the same virtual environment. uv's cache is designed to be thread-safe and append-only ... uv applies a file-based lock to the target virtual environment when installing

> uv blocks cache-modifying operations while other uv commands are running. By default, those `uv cache` commands have a 5 min timeout ... changed with `UV_LOCK_TIMEOUT`.

Nx runs targets in parallel, and concurrent `uv run` invocations against one `.venv` are safe by design. `eng:provision` sets `parallelism: false` for its own reasons, and other uv targets need no such flag.

### [06.4]-[CI_CACHE]

> in continuous integration environments, persisting pre-built wheels may be undesirable. With uv, it turns out that it's often faster to _omit_ pre-built wheels from the cache ... caching wheels that are built from source tends to be worthwhile

> uv provides a `uv cache prune --ci` command, which removes all pre-built wheels and unzipped source distributions from the cache, but retains any wheels that were built from source. We recommend running `uv cache prune --ci` at the end of your continuous integration job

Rasm builds from source (`scipy` with a `setup-args` override, `opencolorio` with `CMAKE_ARGS`, `PyICU`, `scikit-image` through pythran, every 3.15 package with no wheel), `uv cache prune --ci` is the last step of the CI job, and `.cache/uv` is the `actions/cache` path.

### [06.5]-[CACHE_KEYS]

> By default, uv will _only_ rebuild and reinstall local directory dependencies (e.g., editables) if the `pyproject.toml`, `setup.py`, or `setup.cfg` file in the directory root has changed, or if a `src` directory is added or removed.

> Setting `tool.uv.cache-keys` will replace defaults, so any necessary files (like `pyproject.toml`) should still be included

Forms: `{ file = "..." }` (globs, "The use of globs can be expensive"), `{ dir = "..." }` ("will only track changes to the directory itself"), `{ git = { commit = true, tags = true } }`, `{ env = "..." }`. Rasm has no `src/` directory, and once `libs/python/*` members exist as editables, each declares `cache-keys = [{ file = "pyproject.toml" }, { file = "**/*.py" }]` scoped to the member, or `[tool.uv] reinstall-package` forces a rebuild on every run.

### [06.6]-[CACHE_VERSIONING]

> Each bucket is versioned, such that if a release contains a breaking change to the cache format, uv will not attempt to read from or write to an incompatible cache bucket.

### [06.7]-[STORAGE_DIRECTORIES]

| [DIRECTORY]          | [UNIX RESOLUTION ORDER]                                      | [OVERRIDE]                                         |
| :------------------- | :----------------------------------------------------------- | :------------------------------------------------- |
| Temporary            | `$TMPDIR`, `/tmp`                                            |                                                    |
| Cache                | `$XDG_CACHE_HOME/uv`, `$HOME/.cache/uv`                      | `--cache-dir`, `UV_CACHE_DIR`, `tool.uv.cache-dir` |
| Persistent data      | `$XDG_DATA_HOME/uv`, `$HOME/.local/share/uv`, `$CWD/.uv`     |                                                    |
| User configuration   | `$XDG_CONFIG_HOME/uv`, `$HOME/.config/uv`                    |                                                    |
| System configuration | `$XDG_CONFIG_DIRS/uv`, `/etc/uv`                             | `UV_NO_SYSTEM_CONFIG`                              |
| Executables          | `$XDG_BIN_HOME`, `$XDG_DATA_HOME/../bin`, `$HOME/.local/bin` |                                                    |
| Managed Python       | `python/` under persistent data                              | `UV_PYTHON_INSTALL_DIR`, `UV_PYTHON_BIN_DIR`       |
| Tools                | `tools/` under persistent data                               | `UV_TOOL_DIR`, `UV_TOOL_BIN_DIR`                   |
| uv itself            | Executable directory                                         | `UV_INSTALL_DIR`                                   |
| Project environments | `.venv` in the project or workspace root                     | `UV_PROJECT_ENVIRONMENT`                           |
| Script environments  | Cache directory                                              |                                                    |

`.uv/` in the Rasm `.gitignore:46` matches the persistent-data fallback `$CWD/.uv`.

### [06.8]-[ENVIRONMENT_VARIABLES]

The rendered environment reference lists 135 distinct `UV_*` names. The subset a repository sets, with the manifest spelling where one exists:

| [VARIABLE]             | [`[tool.uv]` SPELLING] |
| :--------------------- | :--------------------- |
| `UV_CACHE_DIR`         | `cache-dir`            |
| `UV_PYTHON_PREFERENCE` | `python-preference`    |
| `UV_PYTHON_DOWNLOADS`  | `python-downloads`     |
| `UV_LINK_MODE`         | `link-mode`            |
| `UV_COMPILE_BYTECODE`  | `compile-bytecode`     |
| `UV_OFFLINE`           | `offline`              |
| `UV_EXCLUDE_NEWER`     | `exclude-newer`        |
| `UV_PREVIEW_FEATURES`  | `preview-features`     |
| `UV_NO_SOURCES`        | `no-sources`           |

Variables with no spelling: `UV_PROJECT_ENVIRONMENT`, `UV_PROJECT`, `UV_PYTHON`, `UV_LOCKED`, `UV_FROZEN`, `UV_NO_SYNC`, `UV_NO_DEFAULT_GROUPS`, `UV_NO_GROUP`, `UV_NO_CONFIG`, `UV_CONFIG_FILE`, `UV_NO_SYSTEM_CONFIG`, `UV_ENV_FILE`, `UV_LOCK_TIMEOUT`, `UV_HTTP_RETRIES`, `UV_TOOL_DIR`, `UV_TOOL_BIN_DIR`, `UV_PYTHON_INSTALL_DIR`, `UV_PYTHON_BIN_DIR`.

Where each belongs:
1. Anything with a `[tool.uv]` spelling sits in `pyproject.toml`, the committed owner file and the one home of the four root-only settings
2. A per-invocation policy with no spelling is the flag on the Nx target command line, visible at the call site
3. Nothing uv-related is declared in mise `[env]` or in Forge once the three variables move

Rule 1 covers `cache-dir`, `python-preference`, and `python-downloads`. Rule 2 covers `UV_LOCKED`, `UV_NO_SYNC`, `UV_NO_DEFAULT_GROUPS`, and `UV_NO_GROUP`. `uv.toml` is a fourth home and is never added.

## [07]-[BUILD_BACKENDS_AND_MARKERS]

Sources: `docs/concepts/projects/build.md`, `docs/concepts/build-backend.md`, `docs/guides/package.md`, `docs/concepts/resolution.md`, `docs/reference/policies/versioning.md` at 0.12.9.

### [07.1]-[UV_BUILD]

"When using `uv build`, uv acts as a build frontend and only determines the Python version to use and invokes the build backend." "`uv build` will first build a source distribution, and then build a binary distribution (wheel)", `--sdist`, `--wheel`, `--build-constraint`, `--no-sources` exist, and "`uv build --package <PACKAGE>` will build the specified package within the current workspace." (`package.md:39`). The default output is `dist/`, `-o, --out-dir` overrides it, and the Rasm rule routes it to `.artifacts/python/dist`. `dist/` is ignored by `.gitignore:7` and excluded by `[tool.mypy]` and `[tool.ty.src]`, and `[tool.ruff] extend-exclude` omits it. "If your project does not include a `[build-system]` definition ... uv will not build it during `uv sync` operations in the project, but will fall back to the legacy setuptools build system during `uv build`."

### [07.2]-[UV_BUILD_BACKEND]

> The uv build backend currently **only supports pure Python code**. An alternative backend is required to build a library with extension modules.

> when build scripts or a more flexible project layout are required, consider using the hatchling build backend instead.

Declaration at 0.12.9: `requires = ["uv_build>=0.12.9,<0.13"]`, `build-backend = "uv_build"`, and "Including an upper bound on the `uv_build` version ensures that your package continues to build correctly". The `uv` executable includes a copy of the backend used when compatible, "Other build frontends, such as `python -m build`, will always use the `uv_build` package." `uv_build` carries no preview mark, and `uv init` uses it by default (`config.md:109`).

The default module root is `src/` ("By default, a single root module is expected at `src/<package_name>/__init__.py`"), which the README forbids. The settings that place the module at the project root:

```toml
[tool.uv.build-backend]
module-name = "FOO"
module-root = ""
```

Namespace packages use a dotted `module-name` (`"foo.bar"`, "The `__init__.py` file is not included in `foo`"), `namespace = true` "disables safety checks", and `-stubs` packages look for `__init__.pyi`. A `libs/python/<pkg>` other code imports declares `uv_build` with `module-root = ""`, two lines per package, unless it carries extension modules.

### [07.3]-[MARKER_SET]

29 requirements in the `workspace` group carry `python_version<'3.15'` because their wheels stop at cp314 or their build refuses 3.15. Two documented facts govern them:

> During universal resolution, all required packages must be compatible with the _entire_ range of `requires-python` ... the project's `requires-python` must be a subset of the `requires-python` of all its dependencies. (`resolution.md:126`)

> When evaluating `requires-python` ranges for dependencies, uv only considers lower bounds and ignores upper bounds entirely. (`resolution.md:136`)

With `requires-python = ">=3.15"`, `python_version<'3.15'` is never true in any supported environment, those requirements are present in the manifest and absent from every resolution, `uv.lock` records no version for them, and removing the marker is the whole re-enablement step. The plan settles the handling: the markers are a hedge to remove after checking which group they sit in, because `uv run` syncs the default groups for every `eng` script. They all sit in `workspace`, a default group, removing a marker adds that package to every `uv run`, and the scientific set itself is out of scope and untouched. The alternatives (a non-default group, `[tool.uv.dependency-groups]` floors, `environments`, `required-environments`, `dependency-metadata`) each do something else, and none makes a 3.14-only package installable on 3.15.

Three of the four `dependency-metadata` overrides state their purpose as removing an upper `Requires-Python` bound (colour-cxf "declares an incorrect Requires-Python <3.14 upper bound", colour-science "declares Requires-Python <3.15"), which `resolution.md:136` says uv ignores anyway. Whether they are load-bearing for another reason, avoiding a source build, since `dependency-metadata` also skips "building the relevant package from source", decides whether the four overrides stay, the kept question.

### [07.4]-[LOCKFILE_COMPATIBILITY]

`versioning.md`: "The `uv.lock` file uses a versioned schema ... will reject lockfiles with a greater schema version.", "The schema version is considered part of the public API, and so is only bumped in minor releases", and "The `revision` field of the lockfile is used to track backwards compatible changes". The Rasm lock is `version = 1`, `revision = 3`, and `required-version` gates the schema.

## [08]-[CONFIGURATION_FILES]

Source: `docs/concepts/configuration-files.md` at 0.12.9.

### [08.1]-[PRECEDENCE]

> uv will search for a `pyproject.toml` or `uv.toml` file in the current directory, or in the nearest parent directory.

> For `tool` commands, which operate at the user level, local configuration files will be ignored.

> In workspaces, uv will begin its search at the workspace root, ignoring any configuration defined in workspace members.

> (If there is no such table, the `pyproject.toml` file will be ignored, and uv will continue searching in the directory hierarchy.)

> `uv.toml` files take precedence over `pyproject.toml` files, so if both `uv.toml` and `pyproject.toml` files are present in a directory, configuration will be read from `uv.toml`, and `[tool.uv]` section in the accompanying `pyproject.toml` will be ignored.

The precedence is total, per file rather than per key. A `uv.toml` at the root silently disables the whole `[tool.uv]`, `[tool.uv.sources]`, `[tool.uv.config-settings-package]`, `[tool.uv.extra-build-variables]`, and `[[tool.uv.dependency-metadata]]` set, and Rasm never adds one. "User- and system-level configuration files cannot use the `pyproject.toml` format", a machine-wide default is `~/.config/uv/uv.toml`, installer territory, unused.

Merge order, lowest to highest: system `uv.toml`, user `uv.toml`, project (`uv.toml` if present, else `[tool.uv]`), environment variables, command line ("Settings provided via environment variables take precedence over persistent configuration, and settings provided via the command line take precedence over both."). Arrays concatenate with the project's entries first. `--no-config` disables discovery, `--config-file` replaces it, and `UV_NO_SYSTEM_CONFIG` skips the system level.

`[tool.uv.pip]` is a separate namespace for `uv pip` alone, and Rasm has none and uses the project interface.

### [08.2]-[ENV_LOADING]

`uv run` loads dotenv files through `--env-file` (repeatable, later files override) or `UV_ENV_FILE` (space-separated), `--no-env-file` or `UV_NO_ENV_FILE=1` disables it, and "If the same variable is defined in the environment and in a `.env` file, the value from the environment will take precedence."

### [08.3]-[CHECK_COMMANDS]

`sync.md:47-54`: `uv lock --check` "is equivalent to the `--locked` flag for other commands", the lockfile is outdated when the manifest changed or a constraint excludes the locked version, and "uv will not consider lockfiles outdated when new versions of packages are released". `uv help sync`: `--check` "Check if the Python environment is synchronized with the project. If the environment is not up to date, uv will exit with an error."

| [COMMAND]         | [ASSERTS]                          |
| :---------------- | :--------------------------------- |
| `uv lock --check` | `uv.lock` matches `pyproject.toml` |
| `uv sync --check` | `.venv` matches `uv.lock`          |

Both run in one Nx target on `eng`, before the Python `lint`, `format`, `typecheck`, and `test` targets in CI.

### [08.4]-[TREE_AND_EXPORT]

`uv help tree` (0.12.5): `--universal`, `--format text|json`, `--depth`, `--prune`, `--package`, `--no-dedupe`, `--invert`. `uv tree --invert --package <pkg>` answers what pulls a package in. `sync.md` notes that commands reading the lockfile "will automatically update it before running", and an Nx use passes `--locked`.

`uv export` writes `requirements.txt`, `pylock.toml` (PEP 751), or CycloneDX, and "we recommend against using both a `uv.lock` and a `requirements.txt` file." Rasm exports nothing: no consumer exists, and inventory output is compliance tooling the plan excludes.

## [09]-[MONOREPOS]

Re-read on 2026-09-03 through the GitHub API and raw files at `HEAD`.

| [REPOSITORY]              | [STARS] | [LAST PUSH] | [SHAPE]                                                                                   |
| :------------------------ | ------: | :---------- | :---------------------------------------------------------------------------------------- |
| apache/airflow            |   46706 | 2026-09-03  | Explicit members, `required-version`, `exclude-newer`                                     |
| pydantic/pydantic-ai      |   19698 | 2026-09-03  | 5 members, `default-groups`, `conflicts`, `constraint-dependencies`, per-Python `.venv3XX` |
| elastic/rally             |    2029 | 2026-09-01  | `members = ["."]`, a workspace of one                                                     |
| deepsense-ai/ragbits      |    1668 | 2026-05-18  | Explicit members under `packages/`, root is a `-workspace` project                        |
| pact-foundation/pact-python |     682 | 2026-09-02  | 2 members, nested `include-group` graph                                                   |
| livekit/python-sdks       |     381 | 2026-09-03  | 3 members, one `dev` group                                                                |

None has a root `uv.toml` (all six return 404).

### [09.1]-[AIRFLOW]

`[tool.uv]` (`pyproject.toml:1426-1437`):

```toml
# Bump this only when the project actually relies on a newer uv feature/fix. It is a
# minimum contributors must install, NOT the uv CI pins to ...
required-version = ">=0.11.8"
no-build-isolation-package = ["sphinx-redoc"]
exclude-newer = "4 days"

[tool.uv.exclude-newer-package]
# Automatically generated exclude-newer-package entries (update_airflow_pyproject_toml.py)
```

Its `dev` group (`:1359-1361`) lists the workspace's own distributions (`apache-airflow[all]`, `apache-airflow-breeze`, ...), and `uv sync` installs the whole workspace through one group. CI (`.github/workflows/basic-tests.yml`): `uv tool run --from apache-airflow-breeze pytest -n auto`, `uv run --group dev pytest`, `uv run --project . pytest`, `uv run --no-sync airflow standalone`. `airflow-distributions-tests.yml`: `uv tool install hatch==1.16.5`, `uv tool install twine && twine check dist/*.whl`. `pytest` runs under `uv run` (needs the project), `hatch` and `twine` under `uv tool` (do not).

### [09.2]-[PYDANTIC_AI]

```toml
[tool.uv.workspace]
members = ["pydantic_ai_slim", "pydantic_evals", "pydantic_graph", "clai", "examples"]

[tool.uv]
# `exclude-newer` relative durations need >= 0.9.17, `exclude-newer-package` `false` needs >= 0.9.25
required-version = ">=0.9.25"
default-groups = ["dev", "lint"]
exclude-newer = "7 days"
conflicts = [ ... ]
constraint-dependencies = ["ray>=2.55.0", "authlib>=1.6.7", ...]
```

`required-version` carries a comment naming the feature that sets the floor, and `constraint-dependencies` raises floors on transitive packages with a comment naming the reason. `Makefile`: `uv sync --frozen --all-extras --no-extra mcp-tasks --all-packages --group lint`, and `UV_PROJECT_ENVIRONMENT=.venv310 uv sync --python 3.10 ...` per interpreter, the documented multi-interpreter pattern, with `uv run ruff format --check` and `uv run ruff check`.

### [09.3]-[RALLY]

```toml
[tool.uv]
python-preference = "only-managed"

[tool.uv.sources]
esrally = { workspace = true }

[tool.uv.workspace]
members = ["."]
```

`python-preference = "only-managed"` is the opposite of the Rasm setting and is written in `pyproject.toml` rather than the environment, the choice a project makes when it wants uv to own the interpreter. `Makefile`: `uv lock`, `uv sync --locked --extra=develop`, `uv run -- pre-commit run --all-files`.

### [09.4]-[RAGBITS]

The root is a non-published workspace project, `name = "ragbits-workspace"`, `requires-python = ">=3.10"`, declaring its members as `project.dependencies` (`ragbits-cli`, `ragbits-core[chroma,...]`, ...), each `{ workspace = true }` in `[tool.uv.sources]`, with `members = ["packages/ragbits", ...]`. CI (`shared-packages.yml`): `uv run pre-commit run --all-files`, `uv run ruff format --check`, `uv run ruff check --output-format=github`, `uv run mypy .`, `for dir in packages/*/; do uv build "$dir" --out-dir dist; done`, `uv sync --only-group dev`.

### [09.5]-[PACT_PYTHON]

```toml
[dependency-groups]
dev = [
  "ruff==0.16.2",
  { include-group = "docs" },
  { include-group = "example" },
  { include-group = "test" },
  { include-group = "types" },
]
```

with `members = ["pact-python-cli", "pact-python-ffi"]`. CI (`test.yml`): `uv python install ${{ matrix.python-version }}`, `uv run --python ... --group test --with pytest-cov pytest`, `uv tool install hatch`, the managed-interpreter workflow the Rasm `only-system` setting forecloses.

### [09.6]-[LIVEKIT]

```toml
[tool.uv.workspace]
members = ["livekit-rtc", "livekit-api", "livekit-protocol"]

[tool.uv.sources]
livekit = { workspace = true }

[dependency-groups]
dev = ["ruff>=0.8.5", "mypy>=1.13.0", ...]
```

CI: `uv sync --all-extras --dev`, `uv run ruff check --output-format=github .`, `uv run ruff format --check .`, `uv run mypy livekit-protocol livekit-api livekit-rtc`.

### [09.7]-[AGREEMENT]

1. Members are enumerated when the tree is mixed, and a Python-only directory alone is globbed, which is `libs/python/*`
2. The root is a member: airflow includes `"."`, rally is `["."]`, ragbits and pydantic-ai make the root the aggregator
3. Tools that need the project run under `uv run`, tools that do not run under `uv tool`
4. CI passes `--frozen` or `--locked`
5. `required-version` is set with a comment naming the reason (airflow, pydantic-ai)
6. `uv build --out-dir` routes distributions
7. None has a root `uv.toml`

## [10]-[WARNINGS]

Each item names the mistake, then the correct form after the dash, with the documentation sentence on the following line.

- [A] Editing state uv owns — change `pyproject.toml`, let `uv lock` and `uv sync` produce the state, `uv cache clean` or `prune` for the cache ("`uv.lock` ... should not be edited manually", "It is _not_ recommended to modify the project environment manually, e.g., with `uv pip install`", tool environments "_not_ intended to be mutated directly", the cache "_never_ safe to modify")
- [B] A second configuration file — one project configuration, `[tool.uv]` ("`uv.toml` files take precedence over `pyproject.toml` files")
- [C] Assuming member configuration is read — member manifests carry `[project]` and nothing else, and members declare no `tool.uv.sources` (a member's `tool.uv.sources` overrides, "ignoring any configuration defined in workspace members")
- [D] Treating a workspace as isolation — one environment and one lock, path dependencies when members must not share ("uv can't ensure that packages don't import dependencies declared by another workspace member")
- [E] Expecting a lock to update itself — `uv lock --upgrade` or `--upgrade-package <name>` deliberately ("uv will not consider lockfiles outdated when new versions of packages are released")
- [F] Group flag order — `--only-group X` for "this group and nothing else" ("Group exclusions always take precedence over inclusions")
- [G] The legacy `tool.uv.dev-dependencies` field — `[dependency-groups]`, which Rasm uses ("Use of this field is not recommend anymore")
- [H] Assuming `tool.uv.sources` travels — `project.dependencies` correct on its own, `uv build --no-sources` before publishing ("Sources are only respected by uv")
- [I] Disabling build isolation first — `dependency-metadata`, `extra-build-dependencies`, `extra-build-variables`, `no-build-isolation-package` (in that order, "we recommend augmenting the build dependencies rather than disabling build isolation entirely")
- [J] Partial install flags — layered container builds only ("`--no-install-project` will omit the _project_ but not any of its dependencies ... can result in a broken environment")
- [K] `cache-keys` replaces rather than extends — restate `{ file = "pyproject.toml" }`, scope globs to the member (globs "can be expensive", `dir` tracks only the directory itself)
- [L] `--no-cache` in place of `--refresh` — `--refresh`
- [M] Cache on a different filesystem from the environment — `.cache/uv` beside `.venv`
- [N] An absolute `UV_PROJECT_ENVIRONMENT` shared across projects — relative paths, as the pydantic-ai `.venv310`
- [O] Expecting `VIRTUAL_ENV` to be honored — `--active` opts in
- [P] `uv_build` where it does not apply — hatchling for extension modules, explicit module names (pure Python only, `namespace = true` "disables safety checks")
- [Q] Entry points without a build system — a package needing `[project.scripts]` declares `[build-system]`, the root aggregator declares neither ("Using the entry point tables requires a build system to be defined")
- [R] A `requirements.txt` beside the lock — none
- [S] Assuming a managed Python is always installable — mise owns the interpreter ("The available Python versions are frozen for each uv release", "Upgrades are only supported for uv-managed Python versions")
- [T] Committing `.venv` — `.gitignore:42` excludes it
- [U] A `pyproject.toml` without `[tool.uv]` as a configuration file — the workspace rule in [C] governs (uv "will continue searching in the directory hierarchy")
- [V] `--python-platform` fidelity — `[tool.uv] environments` in the manifest ("may lose fidelity for complex package and platform combinations")

## [11]-[SETTLED_AND_KEPT]

Settled:
1. Workspace shape: `[tool.uv.workspace]` with `libs/python/*` by glob and app projects explicitly, added with the first member
2. Member manifests are membership metadata, and a package other code imports declares `[build-system]` with `uv_build` and `module-root = ""`
3. `[tool.uv]` gains `cache-dir = ".cache/uv"`, `python-preference = "only-system"`, and `python-downloads = "manual"`
4. `[tool.uv]` gains `required-version = ">=0.12.9"` with a reason comment, and `environments` for darwin, linux, win32
5. The Forge `UV_CACHE_DIR`, `UV_PYTHON_PREFERENCE`, and `UV_PYTHON_DOWNLOADS` exports and the Python shims are recorded for removal
6. `.python-version` holds `3.15`, read by mise and uv
7. `eng/project.json` runs `uv run -m eng.scripts.provision`, and cached targets pass `--locked`
8. `uv lock --check` and `uv sync --check` form one `eng` target ahead of the Python check targets
9. `uv cache prune --ci` ends the CI job, and `.cache/uv` is the CI cache path
10. `uv build`, when a package is built, writes to `.artifacts/python/dist`
11. The `dev` group stays as one group, ruff, ty, mypy, and pytest stay in it, and `mise.toml` never names them
12. No `uv.toml`, no `[tool.uv.pip]`, no `uv export`, no inline script metadata under `eng/scripts/`
13. The 29 markers stay until each package's 3.15 wheel lands, and removing a marker is the re-enablement step that adds the package to every `uv run`

Kept, because the plan does not decide it and the answer changes the manifest: whether the four `[[tool.uv.dependency-metadata]]` overrides are load-bearing for skipping a source build, given that `resolution.md:136` says uv ignores a dependency's upper `requires-python` bound, which three of the four name as their reason.

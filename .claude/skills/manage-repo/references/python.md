# [PYTHON]

uv owns distribution, resolution, lock, and environment through root `pyproject.toml`.

## [01]-[RUNTIMES]

- Root Ruff `target-version` and ty `python-version` follow the Python version resolved by mise
- Host folders with code a host runs or imports state its interpreter floor once in a nested `requires-python`
- Host `pyproject.toml` files are virtual uv workspace members (`[tool.uv] package = false`) holding `name`, a static `version`, the floor, `[project] dependencies`, and Ruff `extend`, and create no Nx project
- The `blender` library is a packaged workspace member, its hatchling wheel mapping its own folder onto the `blender` import root, its `[project] dependencies` naming every package its modules import
- Root `rasm` is a virtual workspace root packaging no module and depending on `blender` through a workspace source, no package depending on `rasm`
- Extension rows name `blender` alone, a new extension joining root `[tool.uv.workspace] members` and `uv.lock` first, ty's `missing-direct-dependency` off under `apps/blender/` where extensions import the library's closure
- Fitout bundles the closure `uv export --project <extension> --no-editable` reads from `uv.lock`, building workspace members as wheels it copies in as submodules
- Virtual members install no distribution, `uv sync` installs their dependencies into the root environment
- Host runtime compatibility covers code a host executes or imports, shared libraries included, wheel membership alone imposes none
- Use newest syntax, APIs, and typing supported by each execution runtime

## [02]-[GROUPS]

- Runtime requirements join `[project] dependencies`, dependency groups stay outside wheel requirements
- Inline version bounds cover package metadata gaps or machine library compatibility, recorded beside each bound
- `no-build-package` names packages installable from wheels alone, a resolution needing an unbuildable sdist fails at lock time
- Inline markers exclude platforms with no wheel
- Rust extensions without interpreter wheels build from sdist on rustup's default toolchain and their own Cargo settings
- `dev` lists every package a file CI checks imports outside `[project] dependencies`, CI syncs the root project and `dev`

## [03]-[LOCK]

- `uv.lock` resolves project requirements at root `requires-python` and groups at their own declared floors
- Native build settings stay at root under their package
- Pixi environments hold no Python, `pixi run --environment <name>` supplies their build variables
- Packages building their libraries from their own recipes take the setting that forces it, linking none from the machine
- Machine libraries stay where no package-scoped variable reaches the build, beside the reason in the package row
- Source-built wheel cache keys hold package settings, extra build dependencies, and build variables, a machine library change reuses the wheel
- Pixi rows a source build links name a soname series, `pixi update` moves within it and leaves built consumers valid
- Soname series moves update Pixi constraints and each consuming package's build-variable series together
- `pixi.lock` takes no downgrade check without a catalog package ordering conda versions, `pixi update --json` lists each move's `before` and `after`
- Consumers holding no series variable rebuild through `uv sync --reinstall-package <package>`
- Packages sharing one installed path use root `[tool.uv] exclude-dependencies` to retain one provider throughout resolution
- `uv run` installs a PEP 723 script's inline dependencies into an ephemeral environment `uv.lock` does not pin

## [04]-[ENVIRONMENT]

- `uv sync` installs the `blender` library editable, its hatch `dev-mode-dirs` exposing the folder holding its import root through `.pth` without a `PYTHONPATH` row
- Target commands find the synced `.venv` on `PATH` through mise `python.uv_venv_auto`, `uv run` syncs before every run
- Builds reuse objects or a CMake cache an earlier build left in the cached sdist tree
- Modules in `sys.stdlib_module_names` shadow standard libraries when earlier on `sys.path` or `mypy_path`

## [05]-[CHECKERS]

- `mypy` skips dot-prefixed children in a directory walk, each hidden tree takes its own `files` row
- Ruff prefers a nested `requires-python` over an extended `target-version` and runs once, ty and mypy take one version per run
- Workspace inference runs ty, mypy, and refurb over root code at the configured version and once per nested `requires-python` over its folders
- Editors and bare ty and mypy runs check host folders at the root version
- Python ast-grep rules for a form below or from one release set `metadata` `python-below` or `python-from` as `'<major>.<minor>'`
- Root scans select version rules from resolved mise metadata and each host's `requires-python`
- Host-supplied modules with no typings package type as Any through ty `replace-imports-with-any` `<root>.**` rows and mypy `ignore_missing_imports`
- PEP 723 scripts read ty settings from their block alone, a block's `[tool.ty.rules]` opens with root `[tool.ty] rules` entries
- `mypy` overrides name a PEP 723 script by its unqualified module
- `refurb` parses options through mypy, a command naming no path checks `[tool.mypy] files`
- `refurb`'s own entry runs mypy in fine-grained mode with no cache, each run analyzing every imported package cold
- `python -m tools.refurb` takes refurb's arguments and type-checks each `.py` source from its text under `preserve_asts`
- Stock `refurb` suppresses a visitor `RecursionError` and passes an incomplete traversal, the driver raises it
- `lint:refurb` takes the arguments `typecheck:mypy` takes per runtime split, each run reading the cache mypy wrote at its version
- Cache-affecting mypy options (`allow_redefinition` among them) match between both runs, a differing value invalidates every cached module
- `[tool.refurb] enable_all` loads every check a release adds
- `[tool.refurb] load` imports every `tools/refurb/` module, `__main__` guards its run
- Check modules hold one `RASM` `Error` subclass and one `check`
- `deptry` reads every dependency group as development, `lint:deptry` reads the `blender` library's manifest and `[tool.deptry]` table and scans the host and wheel roots importing its dependencies
- Imports there fail on a group-only, transitive, or undeclared package, type checks over a fully synced environment pass them
- Host-supplied modules take a `per_rule_ignores` `DEP001` row, their typings package names `-stubs` folders deptry maps to no module
- Requirements no repository file imports (data packages, modules a dependency imports undeclared) take a `per_rule_ignores` `DEP002` row

## [06]-[PROJECTS]

- Concern project names derive from `py.typed` directories relative to the folders packaged members' hatch `dev-mode-dirs` name
- `blender_manifest.toml` defines an extension project by manifest `id`
- Project dependencies include runtime and type-only static imports between source owners

## [07]-[HOST_PACKAGING]

- Use `eng/python/fitout/` for host extension assembly and installation
- Each packaged member's wheel maps its folder onto its import root through hatch `sources`, `only-include` keeping its manifest out
- Extension assembly copies first-party wheel packages in as submodules with relative imports and bundles every other distribution as a wheel
- Host interpreter's markers and wheel tags govern `pip wheel` compatibility of wheels an extension bundles
- Fitout omits Blender-provided distributions from locked export
- `blender_manifest.toml` holds no `version`, `pack` writes the one MinVer computes from `blender-<id>-v<version>` tags into the staged manifest
- `install` runs `install-file --repo user_default --enable` in a running GUI on the same user profile, else `install-file` in a background process
- `install` adds a preference row the native handler left absent, a failed enable included, and saves preferences for the next startup's wheel sync

## [08]-[TYPING]

- Retain native runtime types when provider declarations are incomplete
- Diagnostics a typing provider's wrong or missing declaration causes take a rule-specific ignore per line, or one file-top directive for repeats
- Root configuration holds no local exceptions
- Successful resolution, build, or import changes no dependency's published interpreter support

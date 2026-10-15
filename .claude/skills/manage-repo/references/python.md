# [PYTHON]

uv owns distribution, resolution, lock, and environment through root `pyproject.toml`.

## [01]-[RUNTIMES]

- Repository code targets the `mise.toml` `python` row, root Ruff `target-version` and ty `python-version` state that version for every checker reading configuration
- Host folders state their interpreter floor once in a nested `requires-python`, every checker derives host versions from it
- Host folders are no uv projects, their `pyproject.toml` holds `name`, `version`, the floor, `[tool.uv] managed = false`, and Ruff `extend` naming the root file, `uv sync` there refuses and `uv run` there runs in the root environment
- Root `[project] requires-python` is the wheel and lock floor uv reads, covering every host interpreter
- Use newest syntax, APIs, and typing supported by each execution runtime
- Host compatibility covers code executed or imported by a host, including shared libraries
- Different runtimes take scoped declarations when actual consumers require them
- Wheel membership imposes no host runtime on code outside a host's execution

## [02]-[GROUPS]

- Runtime requirements join `[project] dependencies`, dependency groups stay outside wheel requirements
- Inline version bounds cover package metadata gaps or machine library compatibility, recorded beside each bound
- `no-build-package` names packages installable from wheels alone
- Inline markers exclude platforms with no wheel
- `default-groups` names groups `uv sync` installs, `"all"` every group, `--only-group <group>` one group without the project
- `prerelease = "allow"` accepts prereleases for every package
- Rust extensions without interpreter wheels build from sdist on rustup's default toolchain and their own Cargo settings
- `dev` lists every package a file CI checks imports outside `[project] dependencies`, CI syncs the root project and `dev`

## [03]-[LOCK]

- `--locked` fails a command when `uv.lock` is missing or stale
- `environments` restricts resolution to disjoint PEP 508 markers
- `uv.lock` resolves dependencies and groups from `[project] requires-python`, forking where interpreter markers split a package
- Host compatibility constraints use interpreter markers and match bundled API versions without declaring bundle membership
- Native build settings stay at root under their package
- Native libraries an sdist links come from `conda:` rows, `[env]` passes each prefix's `latest` link through a variable its package's build reads
- Packages building their libraries from their own recipes take the setting that forces it, linking none from the machine
- Machine libraries stay where no package-scoped variable reaches the build, beside the reason in the package row
- Built wheel keys read no environment variable, a row in `[tool.uv.extra-build-variables]` names the series of the library a package links
- Soname series moves edit the library's `conda:` row and the package's series row together, a patch inside the series keeps built wheels
- Packages sharing one installed path use root `[tool.uv] exclude-dependencies` to retain one provider throughout resolution
- `uv run` installs a PEP 723 script's inline dependencies into an ephemeral environment `uv.lock` does not pin

## [04]-[ENVIRONMENT]

- Shared test support holds no `pyproject.toml` and reaches tests through `[tool.pytest] pythonpath` and `conftest.py` alone
- `uv sync` installs root editable, exposing backend `module-root` through `.pth` without a `PYTHONPATH` row
- Target commands find the synced `.venv` on `PATH` through mise `python.uv_venv_auto`, `uv run` syncs before every run
- Builds reuse objects or a CMake cache an earlier build left in the cached sdist tree, `uv cache clean <package>` precedes a changed row's rebuild
- Modules in `sys.stdlib_module_names` shadow standard libraries when earlier on `sys.path` or `mypy_path`

## [05]-[CHECKERS]

- `ruff check` and `ty check` take `--config '<key> = <value>'` to override one row, `mypy` takes option flags
- `mypy` skips dot-prefixed children in a directory walk, each hidden tree takes its own `files` row
- Nested declarations create no Nx project or distribution
- Ruff prefers a nested `requires-python` over an extended `target-version` and runs once, ty and mypy take one version per run
- Workspace inference runs ty and mypy over root code at the configured version and once per nested `requires-python` over its folders
- Editors and bare ty and mypy runs check host folders at the root version
- Each ast-grep rule reporting a form below one Python release carries `metadata: {python-below: '<major>.<minor>'}`
- Root scans turn off each such rule the `mise.toml` version reaches and run it over marked folders whose `requires-python` sits below it
- Inferred `options.args` holds checker policy when explicit project commands replace inferred commands
- Formatter paths use native trailing arguments, preserving inferred policy arguments
- Modules a host supplies at run time with no typings package type as Any through ty `replace-imports-with-any` `<root>.**` rows and mypy `ignore_missing_imports`
- PEP 723 scripts read ty settings from their block alone, a block's `[tool.ty.rules]` opens with root `[tool.ty] rules` entries
- `mypy` overrides name a PEP 723 script by its unqualified module
- `refurb` parses options through mypy, a command naming no path checks `[tool.mypy] files`
- `refurb` forces mypy's fine-grained mode, which reads and writes no cache, each run builds in full and `[tool.refurb] mypy_args` keeps the store it opens out of `.cache/mypy`
- `refurb` runs once at the interpreter version, its checks gate no suggestion between host and repository versions
- `[tool.refurb] enable_all` loads every check a release adds, a check whose rewrite another checker rejects or whose form a Ruff rule reports takes a `disable` row naming the reason
- Ruff rules whose form a type-aware refurb check reports on more receivers take a Ruff `ignore` row naming that check
- `[tool.refurb] load` imports first-party checks from `tools/refurb/`, each module holding one `Error` subclass with prefix `RASM` and one `check`
- Match arms implement domain cases, checker suggestions supply no missing-case behavior
- Calls through a union of bound methods with differing signatures fail mypy and ty, each `match` arm calls its own typed method
- `deptry` reads every dependency group as development, `lint:deptry` scans the host and wheel roots that import `[project] dependencies` alone
- Imports there fail on a group-only, transitive, or undeclared package, which type checks over a fully synced environment pass
- Host-supplied modules take a `per_rule_ignores` `DEP001` row, their typings package names `-stubs` folders deptry maps to no module
- Requirements loaded with no import of their own (a data package, a module a dependency imports without declaring) take a `per_rule_ignores` `DEP002` row

## [06]-[PROJECTS]

- Top-level directories under root backend `module-root` are import roots named for a domain or a host, a host directory holds every package that host imports beside its runtime declaration
- Concern project names derive from `py.typed` directories relative to root backend `module-root`
- `blender_manifest.toml` defines an extension project by manifest `id`
- Python projects acquire `check` and `test` when root pytest file patterns discover tests in the project's mirror under pytest `testpaths`
- Project dependencies include runtime and type-only static imports between source owners

## [07]-[HOST_PACKAGING]

- Use `eng/python/fitout/` for host extension assembly and installation
- Root backend `module-name` lists each import root holding code, built into one wheel excluding runtime declarations
- Extension assembly copies the first-party wheel's packages into the extension as submodules with relative first-party imports, and bundles every other distribution as a wheel
- Host interpreter's markers and wheel tags govern `pip wheel` compatibility of wheels an extension bundles
- Root uv constraint rows whose markers hold for a host's interpreter name the ABI providers the host bundles, export omits them

## [08]-[TYPING]

- Correct logic, derivation, and native API use before investigating published typing providers
- Retain native runtime types when provider declarations are incomplete
- Diagnostics a typing provider's wrong or missing declaration causes take a rule-specific ignore on their line, or one file-top directive for repeats
- Root configuration holds no local exceptions
- Checker policies conflicting with program semantics are corrected at their owner
- Successful resolution, build, or import changes no dependency's published interpreter support

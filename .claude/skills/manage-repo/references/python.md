# [PYTHON]

uv owns distribution, resolution, lock, and environment through root `pyproject.toml`.

## [01]-[RUNTIMES]

- Root Ruff `target-version` and ty `python-version` state the `mise.toml` `python` version for every checker reading configuration
- Host folders whose code a host runs or imports state its interpreter floor once in a nested `requires-python`
- Host `pyproject.toml` files hold `name`, `version`, the floor, `[tool.uv] managed = false`, and Ruff `extend`, and create no Nx project
- `managed = false` makes `uv sync` refuse a host folder and `uv run` there use the root environment
- Host runtime compatibility covers code a host executes or imports, shared libraries included, wheel membership alone imposes none
- Use newest syntax, APIs, and typing supported by each execution runtime

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
- `uv.lock` resolves dependencies and groups from root `[project] requires-python`, the floor under every host, forking on interpreter markers
- Native build settings stay at root under their package
- `[env]` passes each `conda:` prefix's `latest` link through a variable its package's build reads
- Packages building their libraries from their own recipes take the setting that forces it, linking none from the machine
- Machine libraries stay where no package-scoped variable reaches the build, beside the reason in the package row
- Built wheel keys read no environment variable
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
- Ruff prefers a nested `requires-python` over an extended `target-version` and runs once, ty and mypy take one version per run
- Workspace inference runs ty, mypy, and refurb over root code at the configured version and once per nested `requires-python` over its folders
- Editors and bare ty and mypy runs check host folders at the root version
- Each ast-grep rule reporting a form below one Python release carries `metadata: {python-below: '<major>.<minor>'}`
- Root scans turn off each such rule the `mise.toml` version reaches and run it over marked folders whose `requires-python` sits below it
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
- Match arms implement domain cases, checker suggestions supply no missing-case behavior
- Calls through a union of bound methods with differing signatures fail mypy and ty, each `match` arm calls its own typed method
- `deptry` reads every dependency group as development, `lint:deptry` scans the host and wheel roots that import `[project] dependencies` alone
- Imports there fail on a group-only, transitive, or undeclared package, which type checks over a fully synced environment pass
- Host-supplied modules take a `per_rule_ignores` `DEP001` row, their typings package names `-stubs` folders deptry maps to no module
- Requirements no repository file imports (data packages, modules a dependency imports undeclared) take a `per_rule_ignores` `DEP002` row

## [06]-[PROJECTS]

- Import roots are top-level `module-root` directories named for a domain or host, a host root holding its imports beside its runtime declaration
- Concern project names derive from `py.typed` directories relative to root backend `module-root`
- `blender_manifest.toml` defines an extension project by manifest `id`
- Python projects acquire `check` and `test` when root pytest file patterns discover tests in the project's mirror under pytest `testpaths`
- Project dependencies include runtime and type-only static imports between source owners

## [07]-[HOST_PACKAGING]

- Use `eng/python/fitout/` for host extension assembly and installation
- Root backend `module-name` lists each import root holding code, runtime declarations stay out of the wheel
- Extension assembly copies first-party wheel packages in as submodules with relative imports and bundles every other distribution as a wheel
- Host interpreter's markers and wheel tags govern `pip wheel` compatibility of wheels an extension bundles
- Root uv constraint rows under a host interpreter's markers pin the ABI providers the host bundles to its versions, export omits them

## [08]-[TYPING]

- Correct logic, derivation, and native API use before investigating published typing providers
- Retain native runtime types when provider declarations are incomplete
- Diagnostics a typing provider's wrong or missing declaration causes take a rule-specific ignore per line, or one file-top directive for repeats
- Root configuration holds no local exceptions
- Checker policies conflicting with program semantics are corrected at their owner
- Successful resolution, build, or import changes no dependency's published interpreter support

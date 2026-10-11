# [PYTHON]

uv owns distribution, resolution, lock, and environment through root `pyproject.toml`.

## [01]-[RUNTIMES]

- Repository source and default checkers use Python from `mise.toml`
- Use newest syntax, APIs, and typing supported by each execution runtime
- Root `[project] requires-python` sets distribution compatibility independently of source and tooling runtimes
- Host compatibility covers code actually executed or imported by a host, including shared libraries
- Namespace membership and wheel assembly impose no host runtime on code outside a host's execution
- Different runtimes take scoped declarations when actual consumers require them

## [02]-[GROUPS]

- Runtime requirements join `[project] dependencies`, dependency groups stay outside wheel requirements
- Inline version bounds cover package metadata gaps or machine library compatibility, recorded beside each bound
- `no-build-package` names packages installable from wheels alone
- Inline markers exclude platforms with no wheel
- `default-groups` names groups `uv sync` installs, `"all"` every group, `--only-group <group>` one group without the project
- `prerelease = "allow"` accepts prereleases for every package
- Rust extensions without interpreter wheels build from sdist through mise's declared Rust toolchain
- `dev` lists every package a file CI checks imports outside `[project] dependencies`, CI syncs the root project and `dev`

## [03]-[LOCK]

- `--locked` fails a command when `uv.lock` is missing or stale
- `environments` restricts resolution to disjoint PEP 508 markers
- `[tool.uv] dependency-groups` rows raise a group's `requires-python` above `[project] requires-python`
- `uv.lock` forks environment markers at raised group requirements, runtime dependencies resolve from `[project] requires-python`
- Host compatibility constraints use interpreter markers and match bundled API versions without declaring bundle membership
- `[tool.uv.sources]` selects package sources by interpreter marker
- Native build settings stay at root under their package
- Packages sharing one installed path use root `[tool.uv] exclude-dependencies` to retain one provider throughout resolution
- `uv run` installs a PEP 723 script's inline dependencies into an ephemeral environment `uv.lock` does not pin

## [04]-[ENVIRONMENT]

- Shared test support holds no `pyproject.toml` and reaches tests through `[tool.pytest] pythonpath` and `conftest.py` alone
- `uv sync` installs root editable, exposing backend `module-root` through `.pth` without a `PYTHONPATH` row
- Target commands find the synced `.venv` on `PATH` through mise `python.uv_venv_auto`, `uv run` syncs before every run
- Modules in `sys.stdlib_module_names` shadow standard libraries when earlier on `sys.path` or `mypy_path`

## [05]-[CHECKERS]

- `ruff check` and `ty check` take `--config '<key> = <value>'` to override one row, `mypy` takes option flags
- `mypy` skips dot-prefixed children in a directory walk, each hidden tree takes its own `files` row
- Default checker versions follow repository tooling independently of distribution compatibility
- Standalone runtime declarations use root Ruff `per-file-target-version` with literal workspace-relative Python paths
- Nested runtime declarations hold project name, `requires-python = ">=<major>.<minor>"`, and Ruff `extend` naming root file
- Nested declarations create neither uv members nor Nx projects, distributions, locks, or environments
- Ruff prefers local `requires-python` over an extended `target-version`; ty and mypy take one runtime per run
- Existing static-import extraction derives runtime closure from declarations, excluding `TYPE_CHECKING` edges from execution
- Workspace inference derives versioned ty and mypy targets, default exclusions, and Ruff file versions from runtime closure
- Inferred `options.args` holds checker policy when explicit project commands replace inferred commands
- Formatter paths use native trailing arguments, preserving inferred policy arguments
- Rhino, Grasshopper, and .NET modules type as Any through ty `replace-imports-with-any` `<root>.**` rows and mypy `ignore_missing_imports`
- PEP 723 scripts read ty settings from their block alone, a block's `[tool.ty.rules]` opens with root `[tool.ty] rules` entries
- `mypy` overrides name a PEP 723 script by its unqualified module
- Match arms implement domain cases, checker suggestions supply no missing-case behavior
- Calls through a union of bound methods with differing signatures fail mypy and ty, each `match` arm calls its own typed method

## [06]-[PROJECTS]

- Concern project names derive from `py.typed` directories relative to root backend `module-root`
- `blender_manifest.toml` defines an extension project by manifest `id`
- Python projects acquire `check` and `test` when root pytest file patterns discover tests
- Project dependencies include runtime and type-only static imports between source owners

## [07]-[HOST_PACKAGING]

- Use `eng/python/fitout/` for Blender extension assembly and installation
- Root backend builds concerns into one namespace wheel, excluding runtime declarations
- Extension assembly selects source from derived runtime imports, with companion stubs and data beneath selected package directories
- Selected modules enter packaging as wheel-relative arguments, package membership supplies no import inventory
- First-party imports use `from rasm.<module> import <member>` for relocation into private extension packages
- Blender supplies extension identity as `bl_ext.<repo>.<id>`, extension-local imports use relative paths within that package
- Ruff import policy permits extension-local relative imports, including parent imports within extension boundaries
- Native uv export supplies locked runtime requirements through artifact `pylock.toml`, root `uv.lock` remains committed
- Selected Blender interpreter's markers and wheel tags govern `pip wheel` compatibility
- Native host metadata identifies bundled ABI providers to omit, repository environment builds establish no portable wheel contract
- Blender supplies executable identity for matching running processes

## [08]-[TYPING]

- Correct logic, derivation, and native API use before investigating published typing providers
- Retain native runtime types when provider declarations are incomplete
- Unavoidable external defects permit diagnostic-specific ignores after native evidence and provider alternatives establish the limitation
- Diagnostic-specific ignores use affected lines or file-top directives for repeated occurrences
- Root configuration holds no local exceptions
- Checker policies conflicting with program semantics are corrected at their owner
- Successful resolution, build, or import changes no dependency's published interpreter support

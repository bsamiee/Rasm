# [PYTHON]

uv owns resolution, lock, and environment of the root project file.

## [01]-[GROUPS]

- Inline version bounds cover only a gap in the bounded package's metadata or a machine library line, row comment states which
- `no-build-package` names packages installable from wheels alone
- Inline markers exclude platforms with no wheel
- `default-groups` names groups `uv sync` installs, `"all"` every group, `--only-group <group>` one group without the project
- `prerelease = "allow"` accepts prereleases for every package
- Packages with a Rust extension and no wheel for the interpreter build from their sdist with `rustc` from the `mise.toml` `rust` row
- `dev` lists every package a file CI checks imports outside `[project] dependencies`, CI syncs the root project and `dev`

## [02]-[LOCK]

- `--locked` fails a command when `uv.lock` is missing or stale
- `environments` restricts resolution to disjoint PEP 508 markers
- `[tool.uv] dependency-groups` rows raise a group's `requires-python` above `[project] requires-python`
- `uv.lock` forks each `environments` marker at a group's raised floor, `[project]` dependencies resolve in both forks from the project floor
- `constraint-dependencies` rows marked `python_full_version < '3.15'` pin 3.13 fork to versions a host bundles
- When packages install one path, `exclude-dependencies` in root `[tool.uv]` drops all but one from resolution however a dependency requests them
- `uv run` installs a PEP 723 script's inline dependencies into an ephemeral environment `uv.lock` does not pin

## [03]-[ENVIRONMENT]

- Shared test support holds no `pyproject.toml` and reaches tests through `[tool.pytest] pythonpath` and `conftest.py` alone
- `uv sync` installs the root project editable, a `.pth` file puts its module root on each `.venv` process's `sys.path` in place of a `PYTHONPATH` row
- Target commands find the synced `.venv` on `PATH` through mise `python.uv_venv_auto`, `uv run` syncs before every run
- Modules named in `sys.stdlib_module_names` can shadow the standard library when their directory comes first on `sys.path` or `mypy_path`

## [04]-[CHECKERS]

- `ruff check` and `ty check` take `--config '<key> = <value>'` to override one row, `mypy` an option flag or a temporary `--config-file`
- `mypy` skips dot-prefixed children in a directory walk, each hidden tree takes its own `files` row
- Ruff `target-version` and ty `python-version` hold the raised group `requires-python`, each reads `[project] requires-python` when unset
- File runtimes use root Ruff `per-file-target-version` with literal workspace-relative Python paths outside nested host trees
- Host trees use nested `requires-python` and Ruff `extend` naming root file
- Nested `pyproject.toml` files are neither uv members nor Nx projects
- Blender loads extensions as `bl_ext.<repo>.<id>`, extension `pyproject.toml` files add `lint.flake8-tidy-imports.ban-relative-imports = "parents"`
- Ruff reads `requires-python` from the closest `[tool.ruff]` file over an extended `target-version`, ty and mypy take one `--python-version` per run
- Workspace plugin groups file runtime mappings and nested `requires-python` by version for ty and mypy, excluding each scope from default runs
- Rhino, Grasshopper, and .NET modules type as Any through ty `replace-imports-with-any` `<root>.**` rows and mypy `ignore_missing_imports`
- PEP 723 scripts read ty settings from their block alone, a block's `[tool.ty.rules]` opens with root `[tool.ty] rules` entries
- `mypy` overrides name a PEP 723 script by its unqualified module
- `exhaustive-match` notes in mypy offer `case _: pass`, a `case None:` arm with a body or a narrowing before `match` clears the error
- Calls through a union of bound methods with differing signatures fail mypy and ty, each `match` arm calls its own typed method

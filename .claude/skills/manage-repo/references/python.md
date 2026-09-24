# [PYTHON]

uv owns resolution, the lock, and the environment of the root project file.

## [01]-[GROUPS]

- Version bounds exist for a resolver conflict, stated in the row comment
- `default-groups` names the groups `uv sync` installs, `"all"` every group, `--only-group <group>` one group without the project
- `prerelease = "allow"` accepts prereleases for every package

## [02]-[LOCK]

- `--locked` fails a command when `uv.lock` is missing or stale
- `environments` restricts resolution to disjoint PEP 508 markers
- `required-environments` names platforms a package without a source distribution must publish a wheel for
- `[tool.uv.workspace] members` globs name the `pyproject.toml` files one root lock covers
- Dependencies on a member take `{ workspace = true }` in `[tool.uv.sources]`

## [03]-[ENVIRONMENT]

- `uv sync` installs the workspace root, `--all-packages` every member
- `python-preference = "only-system"` excludes uv-managed interpreters, `UV_PYTHON` names the interpreter
- Script folder a target runs is a workspace member with a `pyproject.toml` naming its dependencies
- Targets run a member's script as `python <path>` from the synced `.venv` on `PATH`, `uv run` syncs the environment before every run
- `[project.scripts]` needs a build backend and an editable install, the install puts the module root on `sys.path` of every environment process
- Module named like a standard-library module shadows it for every process with its directory on `sys.path` or `mypy_path`

## [04]-[CHECKERS]

- `ruff check` and `ty check` take `--config '<key> = <value>'` to override one row, `mypy` an option flag or a scratch `--config-file`
- `# ty: ignore[<code>]` above the first statement covers the whole file, as `# mypy: disable-error-code=<code>` and `# ruff: file-ignore[<code>]` do
- `respect-type-ignore-comments = false` makes ty read `ty: ignore` comments alone, a line ignoring both checkers carries both comments
- Packages with no stubs or `py.typed` take a mypy `ignore_missing_imports` override by module, ty reads their source
- Modules that build their members at import (pyobjc's `AppKit`) take ty's `replace-imports-with-any`, ty finds no member in their source

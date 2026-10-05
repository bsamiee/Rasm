# [PYTHON]

uv owns resolution, lock, and environment of the root project file.

## [01]-[GROUPS]

- Version bounds exist for a resolver conflict or an sdist build against an older machine library line, stated in the row comment
- `default-groups` names groups `uv sync` installs, `"all"` every group, `--only-group <group>` one group without the project
- `prerelease = "allow"` accepts prereleases for every package
- Packages with a Rust extension and no wheel for the interpreter build from their sdist with `rustc` from the `mise.toml` `rust` row
- `dev` lists every package a file CI checks imports, CI syncs `dev` alone

## [02]-[LOCK]

- `--locked` fails a command when `uv.lock` is missing or stale
- `environments` restricts resolution to disjoint PEP 508 markers
- `[tool.uv] dependency-groups` rows raise one group's `requires-python` above `[project] requires-python`
- `uv.lock` forks each `environments` marker at a group's raised floor, `[project]` dependencies resolve in both forks from the project floor
- `[tool.uv.workspace] members` globs name the `pyproject.toml` files one root lock covers, one per tool the repository runs as a program
- When packages install one path, `exclude-dependencies` in root `[tool.uv]` drops all but one from resolution however a dependency requests them
- `uv run` installs a PEP 723 script's inline dependencies into an ephemeral environment `uv.lock` does not pin

## [03]-[ENVIRONMENT]

- `mise.toml` `[env]` `UV_PYTHON` names the interpreter by path, a path request selects it at any `python-preference`
- Members without `[build-system]` lock as `virtual` and never install
- Targets run a member as `python -m <member>.<module>` from its parent folder or with the parent on `mise.toml` `[env]` `PYTHONPATH`
- Shared test support holds no `pyproject.toml` and reaches tests through `[tool.pytest] pythonpath` and `conftest.py` alone
- Target commands find the synced `.venv` on `PATH` through mise `python.uv_venv_auto`, `uv run` syncs before every run
- Modules named in `sys.stdlib_module_names` shadow the standard library for every process with their directory on `sys.path` or `mypy_path`

## [04]-[CHECKERS]

- `ruff check` and `ty check` take `--config '<key> = <value>'` to override one row, `mypy` an option flag or a temporary `--config-file`
- `# ty: ignore[<code>]` above the first statement covers its whole file, as `# mypy: disable-error-code=<code>` and `# ruff: file-ignore[<code>]` do
- `respect-type-ignore-comments = false` makes ty read `ty: ignore` comments alone, a line ignoring both checkers holds both comments
- Packages with no stubs or `py.typed` take a mypy `ignore_missing_imports` override by module, ty reads their source
- Header `disable-error-code` codes that suppress nothing stay silent in mypy
- `mypy` skips dot-prefixed children in a directory walk, each hidden tree takes its own `files` row
- Ruff `target-version` and ty `python-version` hold the group `requires-python`, each reads `[project] requires-python` when unset
- Per-path target versions exist in ruff alone, mypy checks every file at one version, ty checks a PEP 723 script at its `requires-python`
- PEP 723 scripts take ty settings from their block's `[tool.ty]` tables alone, a block's `[tool.ty.rules]` opens with root `[tool.ty] rules` entries
- `mypy` overrides name a PEP 723 script by its unqualified module
- `exhaustive-match` notes in mypy offer `case _: pass`, a `case None:` arm with a body or a narrowing before `match` clears the error
- Calls through a union of bound methods with differing signatures fail mypy and ty, each `match` arm calls its own typed method

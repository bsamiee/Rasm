# [PYTHON]

uv owns resolution, lock, and environment of the root project file.

## [01]-[GROUPS]

- Version bounds exist for a resolver conflict, stated in the row comment
- `default-groups` names groups `uv sync` installs, `"all"` every group, `--only-group <group>` one group without the project
- `prerelease = "allow"` accepts prereleases for every package
- CI syncs `dev` and every member, `dev` lists packages that files outside members import and no member installs

## [02]-[LOCK]

- `--locked` fails a command when `uv.lock` is missing or stale
- `environments` restricts resolution to disjoint PEP 508 markers
- `required-environments` names platforms a package without a source distribution must publish a wheel for
- `[tool.uv.workspace] members` globs name the `pyproject.toml` files one root lock covers
- When packages install one path, `exclude-dependencies` in root `[tool.uv]` drops all but one from resolution however a dependency requests them
- `uv run` installs a PEP 723 script's inline dependencies into an ephemeral environment `uv.lock` does not pin

## [03]-[ENVIRONMENT]

- `uv sync` installs the workspace root, `--all-packages` every member
- `python-preference = "only-system"` excludes uv-managed interpreters, `UV_PYTHON` names the interpreter
- Members without `[build-system]` lock as `virtual` and never install, a packaged member installs when a root dependency group names it
- Targets run a member as `python -m <member>.<module>` from its parent folder or with the parent on `mise.toml` `[env]` `PYTHONPATH`
- Target commands find the synced `.venv` on `PATH` through mise `python.uv_venv_auto`, `uv run` syncs before every run
- `[project.scripts]` needs a build backend and an editable install
- Editable installs put their module root on `sys.path` of every environment process
- Modules named in `sys.stdlib_module_names` shadow the standard library for every process with their directory on `sys.path` or `mypy_path`
- Relative cache paths resolve against ruff's configuration file and mypy's working directory, `$MYPY_CONFIG_FILE_DIR` pins mypy's

## [04]-[CHECKERS]

- `ruff check` and `ty check` take `--config '<key> = <value>'` to override one row, `mypy` an option flag or a temporary `--config-file`
- `# ty: ignore[<code>]` above the first statement covers its whole file, as `# mypy: disable-error-code=<code>` and `# ruff: file-ignore[<code>]` do
- `respect-type-ignore-comments = false` makes ty read `ty: ignore` comments alone, a line ignoring both checkers holds both comments
- Packages with no stubs or `py.typed` take a mypy `ignore_missing_imports` override by module, ty reads their source
- Header `disable-error-code` codes that suppress nothing stay silent in mypy
- `mypy` skips dot-prefixed children in a directory walk, each hidden tree takes its own `files` row
- Per-path target versions exist in ruff alone, mypy checks every file at one version, ty checks a PEP 723 script at its `requires-python`
- PEP 723 scripts are ty projects outside `[[tool.ty.overrides]]`, a header `ty: ignore` exempts one, mypy overrides name its unqualified module
- `exhaustive-match` notes in mypy offer `case _: pass`, a `case None:` arm with a body or a narrowing before `match` clears the error
- Calls through a union of bound methods with differing signatures fail mypy and ty, each `match` arm calls its own typed method

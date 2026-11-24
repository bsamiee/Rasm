# [TESTING]

pytest suites of the Python packages Nx runs.

## [01]-[PRINCIPLES]

Rules every pytest suite follows:
- Test projects are virtual uv members under `tests/` at their source's path, never built or published
- Tests import packages by import name through the editable install, as a consumer reads them
- One `[tool.pytest]` table in root `pyproject.toml` configures every suite, as pytest never merges configuration files
- Fixtures shared across suites sit in the `tests/support/python` package and load as pytest plugins

## [02]-[CONFIGURATION]

Effects of root `[tool.pytest]` options:
- `pytest` walks up from its path arguments' common ancestor, and the first `pytest.toml` or pytest table it finds wins
- `strict = true` enables every strictness option of the installed pytest, later ones included, and `uv.lock` pins pytest
- `timeout` fails a test running past its seconds through pytest-timeout, and `--disable-socket --allow-unix-socket` fails each network connection
- `randomly` shuffles test order per run and prints the `--randomly-seed` value that reruns it
- `cache_dir = "$PYTEST_CACHE_DIR"` takes the folder from mise `[env]`, as pytest expands variables there and reads no cache variable
- Plugins `-p` names that pytest cannot find exit with code 4
- `rootdir` sets node ids and the cache folder and never changes `sys.path`
- Suites under a Blender root list each plugin the table loads, as their environment installs no root dependency group

## [03]-[IMPORT_MODE]

Module names `--import-mode=importlib` assigns:
- `importlib` mode names a test module from its package chain, else from its path relative to `rootdir`
- Test modules in folders that are not packages keep same-named files of different members apart in one session
- Package-named test folders give members one shared module name, and `pytest` returns the first module under it
- Rootdir-relative names starting with an importable package's name make `pytest` import their test folder as the package
- Test modules cannot import each other, and helpers sit in an importable package

## [04]-[RUNS]

Targets `nx-uv` infers for suites:
- `nx-uv` infers `test--pytest` per member holding test files and runs it in an environment holding only what its member declares
- Option `environmentPath` keeps that environment between runs with `UV_PROJECT_ENVIRONMENT` and `uv run --exact`, else `uv run --isolated` builds it
- `--group <group>` gives a member the root group holding `pytest`
- `nx test <member> -c coverage` adds `--cov` and writes `lcov.info` and `.coverage` to the member's coverage folder

# [TESTING]

pytest suites of the Python packages Nx runs.

## [01]-[PRINCIPLES]

- Test projects are virtual uv members under `tests/`, at the path of the source they test, never built or published
- Tests import packages by `rasm.<name>` through the editable install, as a consumer reads them
- One `tests/pytest.toml` configures every suite, as pytest never merges configuration files
- Suites run under `--import-mode=importlib`, the mode pytest recommends for new projects
- Fixtures shared across suites sit in the `tests/support/python` package and load as pytest plugins by module name

## [02]-[CONFIGURATION]

- `pytest` walks up from the common ancestor of its path arguments, and the first `pytest.toml` or pytest table it finds wins
- `pytest.toml` takes precedence over other files, even when empty
- `strict = true` enables `strict_config`, `strict_markers`, `strict_parametrization_ids`, `strict_xfail`, and every later strictness option
- `uv.lock` pins pytest, which strict mode needs
- `cache_dir = "$PYTEST_CACHE_DIR"` takes the folder from mise `[env]`, as pytest expands variables there and reads no cache variable
- `collect_imported_tests = false` collects only what a test file defines
- `--disable-plugin-autoload` loads only the plugins `-p` names, and a plugin pytest cannot find exits with code 4
- Plugins load by module name, since `pytest-socket`'s entry point name `socket` is a standard library module
- `rootdir` anchors node ids and the cache and never changes `sys.path`
- Suites under a 3.13 root hold their own pytest table, as `tests/pytest.toml` is no ancestor of them

## [03]-[IMPORT_MODE]

- `importlib` mode names a test module from its package chain, else from its path relative to `rootdir`
- Test modules in folders that are not packages keep same-named files of different members apart in one session
- Package-named test folders give members one shared module name, and `pytest` returns the first module of that name
- Rootdir-relative names whose first part equals an importable package make `pytest` import the test folder under that name
- Test modules cannot import each other, so helpers sit in an importable package

## [04]-[RUNS]

- `nx-uv` infers `test--pytest` for each member holding test files, run as `uv run --no-active --locked --isolated --package <member>`
- `--isolated` runs each member in a fresh environment, so a suite imports only what its member declares
- `--group <group>` gives a member the root group holding `pytest`
- `nx test <member> -c coverage` adds `--cov` and writes `coverage.lcov` and `.coverage` to the member's coverage folder

## [05]-[REGRESSIONS]

| [INDEX] | [WRONG]                                                    | [CORRECT]                                           |
| :-----: | :--------------------------------------------------------- | :-------------------------------------------------- |
|  [01]   | Tests importing a sibling by file path or `sys.path` edits | `rasm.<name>` through the editable install          |
|  [02]   | `prepend` import mode                                      | `--import-mode=importlib`                           |
|  [03]   | Plugins found by entry-point autoload                      | `--disable-plugin-autoload` and one `-p` per plugin |
|  [04]   | Test helpers imported from another test module             | Helpers in an importable package                    |
|  [05]   | Dependencies kept after the fixture using them left        | Rows removed with their last importer               |

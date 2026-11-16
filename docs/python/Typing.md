# [TYPING]

ty, mypy, and ruff configuration of the Python packages Nx checks.

## [01]-[PRINCIPLES]

- `ty` reports every rule, and mypy owns the codes ty lacks: exhaustive matches, `disallow_any_*`, `no-any-return`, unreachable code, unused ignores
- Each defect has one owner, so a code both checkers report stays enabled in one of them
- One `[tool.mypy]` table at the repository root serves every uv root
- Each uv root holds its own `[tool.ty]` table
- Each member is checked at the floor of its own `requires-python`
- Every package ships `py.typed` in its import package

## [02]-[PY_TYPED]

| [INDEX] | [CASE]                                         | [MARKER]         | [REASON]                                                       |
| :-----: | :--------------------------------------------- | :--------------- | :------------------------------------------------------------- |
|  [01]   | Sibling member mypy reaches through its `.pth` | Required         | `mypy` reads it as an installed package, `import-untyped` else |
|  [02]   | Same sibling under ty or pyright               | Not read         | Both resolve untyped packages from source                      |
|  [03]   | Stub-only `*-stubs` package                    | None             | Folder suffix `-stubs` marks it, `partial\n` a partial one     |
|  [04]   | Wheel a consumer installs                      | Inside the wheel | `uv_build` packs every file of the module folder               |

- `mypy` silences errors inside installed packages, so each sibling's errors appear once, in its own run
- Siblings missing the marker turn into `Any` in every importer's mypy run
- `Typing :: Typed` and `Typing :: Stubs Only` classifiers have no reader

## [03]-[MYPY]

| [INDEX] | [OPTION]                               | [VALUE]          | [REASON]                                            |
| :-----: | :------------------------------------- | :--------------- | :-------------------------------------------------- |
|  [01]   | `strict`                               | `true`           | Strict flag set                                     |
|  [02]   | `warn_unreachable`                     | `true`           | Outside `--strict`                                  |
|  [03]   | `disallow_any_explicit`, `_unimported` | `true`           | Outside `--strict`                                  |
|  [04]   | `strict_equality_for_none`             | `true`           | Extends `strict_equality`, outside `--strict`       |
|  [05]   | `explicit_package_bases`               | `true`           | Modules name from the run's folder, the member root |
|  [06]   | `enable_error_code`                    | Optional codes   | No ty rule reports them                             |
|  [07]   | `disable_error_code`                   | Codes ty reports | Each owned by ty                                    |
|  [08]   | `plugins`                              | `pydantic.mypy`  | Typed model constructors                            |
|  [09]   | `[tool.pydantic-mypy] init_typed`      | `true`           | `__init__` arguments typed                          |

- `mypy` walks up from the working folder to `.git` and skips a `pyproject.toml` without `[tool.mypy]`, so the root table reaches every root
- `import` is the parent code of `import-not-found` and `import-untyped`, and disabling a parent disables both children
- `import` stays enabled, so `import-untyped` reports a package without types and guards each `py.typed`
- `enable_error_code` holds `exhaustive-match`, `redundant-self`, and `truthy-bool`, codes no ty rule reports
- `ty` reports `deprecated`, missing `@override`, truthiness of iterables, and unused awaitables, so mypy leaves those codes off
- `nx-uv` runs mypy and refurb from the member's base folder: the member root, or its parent where the root holds `__init__.py`
- `refurb` passes `--explicit-package-bases --namespace-packages` on every run
- `python_version` is global in mypy, so nx-uv passes `--python-version` per run

## [04]-[TY]

- `ty` walks up to the first `pyproject.toml` holding `[tool.ty]`, or a `ty.toml`, and merges no parent table
- `ty` has no `extends`, so each uv root restates `environment` and `rules`
- `TY_CONFIG_FILE` replaces project options whole, so it cannot share one table across roots
- `ty` takes the Python version from the table holder's `requires-python`, the root's `>=3.15` for every root member
- `nx-uv` passes `--python-version` from each member's `requires-python` floor to ty and mypy
- Members with a `>=3.13` floor are checked at 3.13, the Python Blender loads them under
- `--python-version` sits above file options, so root rules stay in force
- `ty` overrides carry `rules` and `analysis` alone, with no `environment`
- Paths passed on the command line bypass `src.exclude`, which keeps a bare root `ty check` off the 3.13 roots
- `TY_UV=1` makes ty ask uv for the workspace root and its environment
- `missing-direct-dependency` is a preview rule, running because `all = "error"` selects preview rules and `TY_UV=1` is set

## [05]-[RUFF]

- `src` lists project roots (`products/cli/*`, `libs/python/*/*`), so ruff finds each import package as first party
- `import-private-name` reads a sibling's `_` module as internal to its own package and an outside one as external
- `ruff` takes `target-version` from the `requires-python` beside the `[tool.ruff]` it finds
- Members with a `>=3.13` floor hold `[tool.ruff] extend` for their own range, and 3.15 members hold none
- `ruff check --force-exclude` honors each file's closest configuration, so hk runs ruff at file level across roots

## [06]-[STUBS]

- Stubs arrive as PEP 561 packages in dependency groups, so no `mypy_path`, `extra-paths`, or `stubPath` exists
- `rasm-opencolorio-stubs` generates `PyOpenColorIO-stubs` with pybind11-stubgen in a hatchling build hook, editable installs included
- `pybind11` binds a required parameter after a defaulted one, which Python refuses
- Hook `hatch_build.py` drops each default a required parameter follows, and drops `from __future__ import annotations` from the stubs

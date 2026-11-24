# [TYPING]

ty, mypy, and ruff configuration of the Python packages Nx checks.

## [01]-[PRINCIPLES]

Ownership of each type check:
- `ty` reports every rule, and mypy owns the codes ty lacks: exhaustive matches, `disallow_any_*`, `no-any-return`, unreachable code, unused ignores
- Each defect has one owner, and a code both checkers report stays enabled in one of them
- One `[tool.mypy]` table at the repository root serves every uv root
- Every package holds `py.typed` in its import package

## [02]-[PY_TYPED]

Reader of the `py.typed` marker per case:

| [INDEX] | [CASE]                                         | [MARKER] | [REASON]                                                    |
| :-----: | :--------------------------------------------- | :------- | :---------------------------------------------------------- |
|  [01]   | Sibling member mypy reaches through its `.pth` | Required | `mypy` reads it as installed package, `import-untyped` else |
|  [02]   | Same sibling under ty or pyright               | Not read | Both resolve untyped packages from source                   |
|  [03]   | Stub-only `*-stubs` package                    | None     | Folder suffix `-stubs` marks it, `partial\n` partial stubs  |

Marker effects across siblings:
- `mypy` silences errors inside installed packages, and each sibling's errors appear once, in its own run
- Siblings missing the marker turn into `Any` in every importer's mypy run
- `Typing :: Typed` and `Typing :: Stubs Only` classifiers have no reader

## [03]-[MYPY]

Options and runs of mypy and refurb:
- `warn_unreachable`, `disallow_any_explicit`, `disallow_any_unimported`, and `strict_equality_for_none` sit outside `--strict` and stand beside it
- `mypy` walks up from the working folder to `.git` and skips a `pyproject.toml` without `[tool.mypy]`
- `disable_error_code` names `import-not-found` in place of its parent code `import`, and `import-untyped` guards each `py.typed`
- `nx-uv` runs mypy and refurb from each member root, or from its parent where the member root holds `__init__.py`
- `refurb` passes `--explicit-package-bases --namespace-packages` on every run
- `refurb` runs mypy in fine-grained mode, which reads and writes no cache, and each refurb run repeats a full mypy build
- `python_version` is global in mypy, and nx-uv passes the lowest version of each member's `requires-python` to mypy and refurb

## [04]-[TY]

Configuration lookup of ty:
- `ty` walks up to the first `pyproject.toml` holding `[tool.ty]`, or a `ty.toml`, and merges no parent table
- `ty` has no `extends`, and each uv root restates `environment` and `rules`
- `TY_CONFIG_FILE` replaces project options whole and shares no table across roots
- `ty` takes its Python version from the `requires-python` beside its table, a uv root's for each of its members
- `ty` overrides hold `rules` and `analysis` alone, with no `environment`
- `src.exclude` keeps a root `ty check` with no paths off Blender roots, and command-line paths bypass it
- `TY_UV=1` makes ty ask uv for the workspace root and environment, as preview rule `missing-direct-dependency` needs both under `all = "error"`

## [05]-[RUFF]

Project roots and configuration lookup of ruff:
- `src` lists project roots, and ruff reads each import package there as first party
- `import-private-name` reads a sibling's `_` module as internal to its own package and an outside one as external
- `ruff` takes `target-version` from the `requires-python` beside each `[tool.ruff]` it finds
- Members requiring a Python below the root's hold `[tool.ruff] extend` for their own range, and other members hold none
- `ruff check --force-exclude` honors each file's closest configuration across roots

## [06]-[STUBS]

Stub packages and their generation:
- Stubs arrive as PEP 561 packages in dependency groups, and no `mypy_path`, `extra-paths`, or `stubPath` exists
- `rasm-opencolorio-stubs` generates `PyOpenColorIO-stubs` with pybind11-stubgen in a hatchling build hook, editable installs included
- Hook `hatch_build.py` drops defaults before required parameters, a pybind11 form Python refuses, and `from __future__ import annotations`

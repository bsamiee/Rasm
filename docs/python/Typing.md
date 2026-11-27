# [TYPING]

Checker configuration of the Python packages.

## [01]-[PRINCIPLES]

Ownership of each type check:
- Each defect has one checker: ty runs every rule its `rules` table leaves on, and mypy runs only codes ty lacks
- One `[tool.mypy]` table at the repository root serves every uv root, as mypy walks up from its working folder to `.git` past each `pyproject.toml` without one
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
- `disable_error_code` names `import-not-found` in place of its parent code `import`, and `import-untyped` guards each `py.typed`
- `@rasm/nx-uv` runs mypy and refurb from each member root, or from its parent where the member root holds `__init__.py`
- `refurb` passes `--explicit-package-bases --namespace-packages` on every run
- `@rasm/nx-uv` passes mypy `--cache-dir <environmentPath>/.mypy_cache`, one per member, as mypy keeps one SQLite store per folder and concurrent runs on one fail
- `refurb` runs mypy in fine-grained mode, which reads and writes no cache, and each refurb run repeats a full mypy build
- `@rasm/nx-uv` passes refurb `-- --cache-dir <environmentPath>/.refurb_cache`, as that mypy build opens a store of its own
- `MYPY_NUM_WORKERS` and `[tool.mypy] num_workers` stay unset, as refurb's option parsing reads both and four workers dropped findings and crashed a run
- `python_version` is global in mypy, and `@rasm/nx-uv` passes the lowest minor each member's `requires-python` admits to mypy and refurb

## [04]-[TY]

Configuration lookup of ty:
- `@rasm/nx-uv` runs one `ty check` per uv root, from that root over every member root, in an environment holding every package of its workspace
- `ty` walks up to the first `pyproject.toml` holding `[tool.ty]`, or a `ty.toml`, and merges no parent table
- `ty` has no `extends`, and each uv root restates `environment`, `project-manager`, and `rules`
- `TY_CONFIG_FILE` replaces project options whole and shares no table across roots
- `ty` takes its Python version from the `requires-python` beside its table, a uv root's for each of its members
- `ty` overrides hold `rules` and `analysis` alone, with no `environment`
- `src.exclude` keeps a root `ty check` with no paths off Blender roots, and command-line paths bypass it
- `project-manager.project = "uv"` makes ty ask uv for the workspace root and environment, as preview rule `missing-direct-dependency` needs both under `all = "error"`, and a set `TY_UV` overrides it

## [05]-[RUFF]

Project roots and configuration lookup of ruff:
- `src` lists project roots, and ruff reads each import package there as first party
- `import-private-name` reads a sibling's `_` module as internal to its own package and an outside one as external
- `ruff` takes `target-version` from the `requires-python` beside each `[tool.ruff]` it finds
- Members hold a `[tool.ruff]` extending the table above them where their `requires-python` floor or a rule differs, and other members hold none
- `ruff check` reads each file's closest `[tool.ruff]`, and hk's `--force-exclude` applies its `exclude` to the paths hk names

## [06]-[STUBS]

Stub packages and their generation:
- Stubs arrive as PEP 561 packages in dependency groups, and no `mypy_path`, `extra-paths`, or `stubPath` exists
- `rasm-opencolorio-stubs` generates `PyOpenColorIO-stubs` with pybind11-stubgen in a hatchling build hook, editable installs included
- Hook `hatch_build.py` drops defaults before required parameters, a pybind11 form Python refuses, and `from __future__ import annotations`

# [PUBLISHING]

Wheel and source distribution builds of the Python packages Nx releases.

## [01]-[PRINCIPLES]

- Every packaged member is a release project and keeps the same publish fields, private ones included
- Classifier `Private :: Do Not Upload` stops the upload alone, every other field stays
- Builds run `uv build --no-sources`, the form uv advises before publishing
- Each distribution holds the license text and readme of its own project folder
- Wheels and their editable installs expose the same import names

## [02]-[PUBLISH_FIELDS]

| [INDEX] | [FIELD]         | [VALUE]                        | [REASON]                                                         |
| :-----: | :-------------- | :----------------------------- | :--------------------------------------------------------------- |
|  [01]   | `license`       | SPDX expression                | Written as `License-Expression`                                  |
|  [02]   | `license-files` | `["LICENSE"]`                  | One `License-File` per match, packed into `.dist-info/licenses/` |
|  [03]   | `readme`        | `"README.md"`                  | Long description `twine check --strict` requires                 |
|  [04]   | `authors`       | One row                        | Same owner on every release project                              |
|  [05]   | `urls`          | `Homepage`, `Source`, `Issues` | PEP 753 well-known labels                                        |
|  [06]   | `version`       | Static                         | `uv_build` refuses `dynamic`, and nx-uv version actions write it |

- `license-files` globs resolve inside the project folder and refuse `..`, so each project holds a real `LICENSE`
- `hatchling` stores a symlinked `LICENSE` as a link in the sdist, and the wheel built from that sdist drops it with no error
- `uv_build` copies a symlink's target into both archives
- `License ::` classifiers are deprecated beside a `license` expression

## [03]-[CONTENTS]

| [INDEX] | [CONTENT]                     | [WHEEL] | [SDIST] | [REASON]                                               |
| :-----: | :---------------------------- | :------ | :------ | :----------------------------------------------------- |
|  [01]   | Import package modules        | Yes     | Yes     | `uv_build` packs every file of the module folder       |
|  [02]   | `py.typed`                    | Yes     | Yes     | Sits in the module folder                              |
|  [03]   | `LICENSE`                     | Yes     | Yes     | `license-files`                                        |
|  [04]   | `README.md`                   | No      | Yes     | Metadata long description in the wheel                 |
|  [05]   | `pyproject.toml`              | No      | Yes     | `uv_build` adds the rewritten manifest and its `.orig` |
|  [06]   | Tests, `uv.lock`, other files | No      | No      | Outside the module folder                              |

- `source-exclude` applies to wheels too, `wheel-exclude` to wheels alone, and defaults exclude `__pycache__`, `*.pyc`, and `*.pyo`
- Non-Python files inside the module folder (a Flamenco job script) ship with no include row

## [04]-[PACK_CHECKS]

| [INDEX] | [CHECK]        | [COMMAND]                                                            | [FINDS]                                    |
| :-----: | :------------- | :------------------------------------------------------------------- | :----------------------------------------- |
|  [01]   | Build          | `uv build --package <member> --no-sources`                           | Wheel and sdist from the committed lock    |
|  [02]   | Metadata       | `twine check --strict <dist>/*`                                      | Missing or unrenderable long description   |
|  [03]   | Wheel contents | `check-wheel-contents <dist>/*.whl`                                  | Stray top-level files, duplicates, `.pyc`  |
|  [04]   | Install test   | Wheel installed in an empty venv, then an import of each import name | Missing modules and broken dependency rows |

## [05]-[VERSIONING]

- `nx-uv` tags every packaged member `release` and versions it through its own `version-actions`
- Darkroom's version sits in `blender_manifest.toml`, which hatchling reads as its dynamic version and nx release writes

## [06]-[PUBLISH]

- `uv publish` uploads with `UV_PUBLISH_TOKEN`, and `--index <name>` selects an index with a `publish-url`
- `--dry-run` does everything but the upload
- `--check-url <index>` skips files the index holds, so a rerun after a partial failure uploads what remains

## [07]-[REGRESSIONS]

| [INDEX] | [WRONG]                                               | [CORRECT]                                        |
| :-----: | :---------------------------------------------------- | :----------------------------------------------- |
|  [01]   | `license = "MIT"` with no license text in any archive | Real `LICENSE` and `license-files = ["LICENSE"]` |
|  [02]   | Symlinked `LICENSE`                                   | Regular file per project                         |
|  [03]   | No `readme`                                           | `readme = "README.md"`                           |
|  [04]   | Publish fields on some release projects alone         | Same fields on every release project             |
|  [05]   | Wheel `include` rows listing `py.typed`               | `uv_build` packing the module folder whole       |
|  [06]   | Unbounded `requires = ["uv_build"]`                   | Bounded to the uv minor                          |

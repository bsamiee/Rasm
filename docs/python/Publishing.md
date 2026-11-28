# [PUBLISHING]

Wheel and source distribution builds of the Python packages Nx releases.

## [01]-[PRINCIPLES]

Rules every packaged member follows:
- Every packaged member is a release project and keeps the same publish fields, private ones included
- Classifier `Private :: Do Not Upload` stops the upload alone, every other field stays
- Builds run `uv build --no-sources`
- `uv build` writes each wheel from its sdist, and each file its sdist misses fails the wheel
- `uv_build` refuses a missing module folder, `readme`, or `license-files` match

## [02]-[LICENSES]

License and classifier rules:
- `license-files` globs resolve inside the project folder and refuse `..`, and each project holds a real `LICENSE`
- `hatchling` stores a symlinked `LICENSE` as a link in its sdist, and wheels built from it drop the file with no error
- `uv_build` copies a symlink's target into both archives
- PEP 639 deprecates `License ::` classifiers beside a `license` expression

## [03]-[CONTENTS]

Files each archive holds:

| [INDEX] | [CONTENT]                     | [WHEEL] | [SDIST] | [REASON]                                           |
| :-----: | :---------------------------- | :------ | :------ | :------------------------------------------------- |
|  [01]   | Import package modules        | Yes     | Yes     | `uv_build` packs every module folder file          |
|  [02]   | `py.typed`                    | Yes     | Yes     | Sits in module folder                              |
|  [03]   | `LICENSE`                     | Yes     | Yes     | `license-files`                                    |
|  [04]   | `README.md`                   | No      | Yes     | Metadata long description in wheel                 |
|  [05]   | `pyproject.toml`              | No      | Yes     | `uv_build` adds rewritten manifest and its `.orig` |
|  [06]   | Tests, `uv.lock`, other files | No      | No      | Outside module folder                              |

Include and exclude rules:
- `source-exclude` applies to sdists and wheels, `wheel-exclude` to wheels alone, and defaults exclude `__pycache__`, `*.pyc`, and `*.pyo`
- Non-Python files inside the module folder (a Flamenco job script) enter both archives with no include row

## [04]-[VERSIONING]

Version sources:
- `@rasm/nx-uv` versions each packaged member through its own `version-actions`
- `@rasm/nx-distribution` versions Darkroom, a Blender extension, in `blender_manifest.toml`

## [05]-[PUBLISH]

Upload routes and credentials:
- `uv publish` uploads by trusted publishing under `id-token: write` alone, to PyPI, or to the `publish-url` of the first `[[tool.uv.index]]` row holding one
- First PyPI releases register a pending trusted publisher under the project name
- `--dry-run` does everything but the upload

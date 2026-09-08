# [PYTHON]

Configure Python dependencies and checkers in the root manifest. Expose automation through the owning Nx target. Package identity sits in the project inferred or declared for the package.

## [01]-[MANIFEST]

Each dependency group in root `pyproject.toml` holds unpinned names and serves one consumer (workspace packages, `eng/` scripts, development tooling):
- `uv run` syncs the groups `default-groups` lists on every invocation, `all` syncs every root group, and `--only-group <group>` syncs one
- `uv sync --locked` fails on lock drift
- `uv sync --all-groups` syncs every group
- `prerelease = "allow"` accepts every prerelease
- `python-preference = "only-system"` keeps the interpreter the toolchain manager installs
- Use PEP 508 markers in `environments` to constrain resolution and in `required-environments` to require supported platforms
- `[tool.uv.sources]` pins an archive URL where the registry release lacks a member
- `[[tool.uv.dependency-metadata]]` corrects `Requires-Python`
- Cache paths sit in each tool's `[tool.*]` table (`cache-dir` for uv and ruff, `cache_dir` for mypy and pytest), under `.cache/<tool>`
- Import roots are stated once per checker (`src` for ruff, `root` for ty, `pythonpath` for pytest) with the same values

`required-environments` requires a matching wheel for packages without a source distribution. Packages with one build from source. Sdist-only lock members build against native libraries (Arrow C++, ICU, vips, PDAL, OpenEXR). pyktx has no Linux wheel in any release.

## [02]-[PROJECTS]

Python packages hold no manifest. Local plugin infers a project from a package marker:
- `__init__.py` one level under the library root or an application marks the project, named by the last segment of its root
- Directories outside the marker glob (scripts, test support) declare targets in a `project.json` tagged with the language
- Cached targets take the manifests, `UV_PYTHON` path, and uv version through the `python` named input
- `typecheck` runs ty and mypy in order
- `test` runs pytest with `--cov` on the project root
- Each `test` target sets `COVERAGE_FILE` to a per-project data file beside the root data file
- `benchmark` configuration of `test` passes `-m benchmark`
- Publish target copies the package into a build directory with a generated manifest, the group named after the package as dependencies
- Generated manifest takes the version from the newest `<name>@<version>` tag reachable from HEAD

## [03]-[CHECKERS]

Configure checkers through root `[tool.*]` tables:
- `required_plugins` names every pytest plugin a test relies on
- `-p no:<name>` of a plugin in `required_plugins` fails at startup
- `-p no:<name>` takes the entry-point name (`pytest_cov`) or its suffix (`cov`)
- `-p no:<distribution>` (`pytest-cov`) drops nothing
- `addopts` loads the test support runtime module as a plugin with `-p <module>`
- `-p no:<plugin>` in `addopts` drops a plugin another package registers
- `conftest.py` at the test root registers the package tree of the library root
- Each test directory matches the package it tests
- `timeout` is a string, the plugin registers the option as one and TOML mode rejects an integer
- `[tool.coverage.run]` `patch = ["subprocess"]` and `relative_files` make child processes measure into parallel data files by relative path
- Report commands (`coverage lcov`, `coverage xml`) combine the parallel data files
- `coverage combine` fails on empty input
- Coverage script runs one language's report commands over the data files its tests left, or exits 0 with a report when none exist

## [04]-[PROVISIONING]

Provisioning script places every pinned build tool and archive under `.cache/`, after the toolchain install on a fresh clone:

| [INDEX] | [TOOL]         | [MANIFEST]                          | [PLACEMENT]                                | [IDEMPOTENCE]                    |
| :-----: | :------------- | :---------------------------------- | :----------------------------------------- | :------------------------------- |
|  [01]   | Python scripts | `pyproject.toml` and `uv.lock`      | `.venv/`                                   | `uv run` syncs on each invocation |
|  [02]   | vcpkg          | `builtin-baseline` per `vcpkg.json` | `.cache/vcpkg/`, archives, downloads       | Checkout on a HEAD mismatch      |
|  [03]   | Host tools     | Script port name, or `release.json` | `.cache/vcpkg-hosttools/`, `.cache/tools/` | Skip when the executable exists  |
|  [04]   | Release files  | Manifest digest per rid             | `.cache/<name>/<version>/<rid>/`           | Skip when the pinned file exists |

- Verify a pinned digest on every download, unlink the file on a mismatch, and give a partial download a temporary name
- Pin every checkout to a commit, fetch with depth one, and update HEAD when it differs
- Find the repository root as the nearest ancestor directory holding the root lock file
- Take every tool a package manager can pin from the manager, and download the rest
- Create the vcpkg binary cache and downloads directories before the first run, vcpkg reads cache variables for existing absolute paths alone
- Link the release root (executable directory, or its parent when named `bin`) at `.cache/tools/<library>` with a relative link
- Consumers join the manifest `path` onto the link
- Relative links restore from a cache on another machine

pyktx sdist reads `LIBKTX_VERSION` at import and compiles against `ktx.h` and `-lktx` from the `LIBKTX_*` directories the mise environment names. uv's cached PEP 517 build keeps the process environment of its run. After a variable changes, `uv cache clean pyktx` then `uv sync` rebuilds it.

## [05]-[STAGING]

Staging script builds or fetches one library for a runtime identifier and writes the layout the packaging project packs:
- Run vcpkg with `--x-manifest-root` and `--x-install-root` under `.artifacts/` and `VCPKG_DEFAULT_BINARY_CACHE` under `.cache/`
- Provisioning fails on a second `builtin-baseline` in a `vcpkg.json`
- Staging checks the port version against `version-string`
- On macOS, rewrite every install name in a shared library closure to `@loader_path` and sign it ad hoc for loading from its directory
- Take the runtime identifier as an argument with the host as default
- Libraries with no asset or build for the runtime identifier stage nothing and report it
- Key the output of a long compile by commit under `.cache/` for a repeat run to copy
- Generate binding sources into `stage/managed/` from the port's pinned source archive, downloaded against an empty install root
- Staging imports the provisioning module
- Provisioning imports no script

## [06]-[ANTI_PATTERNS]

| [INDEX] | [SMELL]                                         | [CORRECT_FORM]                                                               |
| :-----: | :---------------------------------------------- | :--------------------------------------------------------------------------- |
|  [01]   | One script per library, each with download code | One module, lookup table from library to staging function, shared operations |
|  [02]   | Manifest versions copied into a script constant | Script reads the manifest, the project checks `Version` against it           |

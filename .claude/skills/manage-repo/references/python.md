# [PYTHON]

The Python manifest, scripts, and checker configuration of a language area is one root manifest with its lock, the scripts the targets run, and the checker tables the targets read. Every Python file type is scanned for scattering, duplication, and misplaced declarations, a manifest exists only where one belongs, scripts are declared nowhere they do not belong, and the root holds the dependency groups.

## [01]-[MANIFEST]

The root `pyproject.toml` holds every dependency group as unpinned names and `uv.lock` pins them, and each group serves one consumer: the workspace packages, the scripts under `eng/`, and development tooling:
- `uv run` syncs the groups `default-groups` lists before every invocation, `all` syncs every root group, and `--only-group <group>` syncs one
- `uv sync --locked` fails on lock drift, and `--all-groups` syncs every group
- `prerelease = "allow"` accepts every prerelease, and `python-preference = "only-system"` keeps the interpreter the toolchain manager installs
- `environments` and `required-environments` name the runtime identifiers the lock solves, and the lock fails when one lacks a wheel
- `[tool.uv.sources]` pins an archive URL where the registry release lacks a member, and `[[tool.uv.dependency-metadata]]` corrects `Requires-Python`
- Cache paths belong in the `[tool.*]` table of the tool (`cache-dir` for uv and ruff, `cache_dir` for mypy and pytest), under `.cache/<tool>`
- The import roots are stated once per checker (`src` for ruff, `root` for ty, `pythonpath` for pytest) and hold the same values

## [02]-[PROJECTS]

Python packages carry no manifest, and the local plugin infers a project from a package marker:
- `__init__.py` one level under the library root or an application marks the project, and the plugin names it by the last segment of its root
- Directories the marker glob does not cover (the scripts, the test support) declare their targets in a `project.json` tagged with the language
- The tag-filtered defaults run the linter with `--fix`, the formatter, both type checkers in order, and pytest with `--cov` on the project root
- Each `test` target sets `COVERAGE_FILE` to a per-project data file beside the root data file, and a `benchmark` configuration passes `-m benchmark`
- The publish target copies the package into a build directory with a generated manifest, the group named like the package as its dependencies
- The generated manifest takes the version from the newest `<name>@<version>` tag reachable from HEAD, and `uv publish` uses trusted publishing

## [03]-[CHECKERS]

Each checker reads its `[tool.*]` table from the root manifest, and the tables state one fact each:
- `required_plugins` names every pytest plugin a test relies on, and `-p no:<plugin>` of a listed plugin fails at startup
- `addopts` loads the test support runtime module as a plugin with `-p <module>`, and `-p no:<plugin>` drops one another package registers
- `conftest.py` at the test root registers the package tree of the library root, and a test directory mirrors the package it tests
- `timeout` is a string, the plugin registers the option as one and TOML mode rejects a native integer
- `[tool.coverage.run]` sets `patch = ["subprocess"]` and `relative_files`, and child processes measure into parallel data files by relative path
- The report commands (`coverage lcov`, `coverage xml`) combine the parallel data files, and a `coverage combine` step fails on empty input
- The coverage script runs the report commands of one language when its tests left data files, and a language without data exits 0 with a report

## [04]-[PROVISIONING]

The provisioning script places every pinned build tool and archive the host needs under `.cache/`, and the toolchain install precedes it on a fresh clone:

| [INDEX] | [TOOL]         | [MANIFEST]                          | [PLACEMENT]                                | [IDEMPOTENCE]                    |
| :-----: | :------------- | :---------------------------------- | :----------------------------------------- | :------------------------------- |
|  [01]   | Python scripts | `pyproject.toml` and `uv.lock`      | `.venv/`                                   | `uv run` syncs each invocation   |
|  [02]   | vcpkg          | `builtin-baseline` per `vcpkg.json` | `.cache/vcpkg/`, archives, downloads       | Checkout on a HEAD mismatch      |
|  [03]   | Host tools     | Script port name, or `release.json` | `.cache/vcpkg-hosttools/`, `.cache/tools/` | Skip when the executable exists  |
|  [04]   | Release files  | Manifest digest per rid             | `.cache/<name>/<version>/<rid>/`           | Skip when the pinned file exists |

Provisioning rules:
- Verify a pinned digest on every download, unlink the file on a mismatch, and give a partial download a temporary name
- Pin every checkout to a commit, fetch with depth one, and update HEAD when it differs
- Find the repository root as the nearest ancestor directory holding the root lock file
- Take every tool a package manager can pin from the manager, and download the rest
- Create the vcpkg binary cache and downloads directories before the first run, vcpkg reads its cache variables for existing absolute paths alone
- Link a host tool's version-free path under `.cache/tools/<library>` with a relative link, and the tree restores from a cache on another machine

## [05]-[STAGING]

The staging script builds or fetches one library for a runtime identifier and writes the layout the packaging project packs:
- Stage every library from one script module, a lookup table maps each library to its staging function, and the shared operations exist once
- Run vcpkg with `--x-manifest-root` and `--x-install-root` under `.artifacts/` and `VCPKG_DEFAULT_BINARY_CACHE` under `.cache/`
- Pin one `builtin-baseline` in every `vcpkg.json`, provisioning fails on a second one, and staging checks the port version against `version-string`
- On macOS, rewrite every install name in a shared library closure to `@loader_path` and sign it ad hoc, and the set loads from its own directory
- Take the runtime identifier as an argument with the host as default, and a library with no asset or build for it stages nothing and reports it
- Key the output of a long compile by commit under `.cache/`, and a repeat run copies it
- Generate binding sources into `stage/managed/` from the port's pinned source archive, downloaded against an empty install root
- The staging script imports the provisioning module, and the provisioning module imports no script

## [06]-[ANTI_PATTERNS]

| [INDEX] | [SMELL]                                           | [CORRECT_FORM]                                                     |
| :-----: | :------------------------------------------------ | :----------------------------------------------------------------- |
|  [01]   | One script per library with its own download code | One script module with a lookup table and shared operations        |
|  [02]   | Manifest versions copied into a script constant   | Script reads the manifest, the project checks `Version` against it |

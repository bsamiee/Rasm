# [WORKSPACE]

uv workspace layout of the Python packages Nx runs.

## [01]-[PRINCIPLES]

Build backends and sibling resolution:
- `uv_build` builds every package, and hatchling builds a package alone where a build hook or a dynamic field needs it
- Siblings resolve through editable installs exposing the import names their wheels expose

## [02]-[ROOTS]

uv roots and the Python each one resolves:

| [INDEX] | [ROOT]                                          | [PYTHON]        | [HOLDS]                                               |
| :-----: | :---------------------------------------------- | :-------------- | :---------------------------------------------------- |
|  [01]   | `pyproject.toml`                                | Newest          | Shared libraries, CLI products, `tools/refurb`, tests |
|  [02]   | `libs/python/blender/pyproject.toml`            | Blender's minor | Blender libraries, OpenColorIO stubs, Blender suites  |
|  [03]   | `products/apps/blender/darkroom/pyproject.toml` | Blender's minor | Darkroom, `package = false`, Blender libraries        |

Rules each root follows:
- Roots state their Python in `requires-python` alone, and no root holds the `.python-version` or `.python-versions` file uv reads where one exists
- Blender code takes its own roots, as a member pinned to Blender's minor under the repository root empties its `requires-python` range and `uv lock` fails
- Blender roots source shared libraries supporting Blender's minor through `{ workspace = "<path>" }`, with their own root's sources
- Darkroom is its own root, as uv finds a workspace from parent folders alone and no Blender root is a parent of `products/`
- `uv`, mise, and the extension build reach Darkroom's lock from its own folder, with no `--project` the distribution plugin would derive
- Members without `[build-system]` are virtual: uv installs their dependencies and never builds them

## [03]-[PACKAGE_MANIFEST]

Library manifest:

```toml
[project]
name              = "rasm-<name>"
version           = "0.1.0"
description       = "<description>"
readme            = "README.md"
license           = "MIT"
license-files     = ["LICENSE"]
authors           = [{ name = "<author>", email = "<email>" }]
classifiers       = ["Private :: Do Not Upload"]
requires-python   = ">=<python>"
import-names      = ["rasm.<name>"]
import-namespaces = ["rasm"]
dependencies      = ["<dependency>"]

[project.urls]
Homepage = "https://github.com/<owner>/<repository>/tree/main/<project path>"
Source   = "https://github.com/<owner>/<repository>"
Issues   = "https://github.com/<owner>/<repository>/issues"

[build-system]
requires      = ["uv_build>=<version>,<<next minor>"]
build-backend = "uv_build"

[tool.uv.build-backend]
module-name = "rasm.<name>"
module-root = ""
```

Fields every built package states:

| [INDEX] | [FIELD]             | [VALUE]                         | [REASON]                                                         |
| :-----: | :------------------ | :------------------------------ | :--------------------------------------------------------------- |
|  [01]   | `license`           | SPDX expression                 | Written as `License-Expression`                                  |
|  [02]   | `license-files`     | `["LICENSE"]`                   | One `License-File` per match, packed into `.dist-info/licenses/` |
|  [03]   | `readme`            | `"README.md"`                   | Long description, content type read from extension               |
|  [04]   | `authors`           | One row                         | Same owner on every release project                              |
|  [05]   | `urls`              | `Homepage`, `Source`, `Issues`  | PEP 753 well-known labels                                        |
|  [06]   | `version`           | Static                          | `uv_build` refuses `dynamic`                                     |
|  [07]   | `module-root`       | `""`                            | `rasm/` folder sits in project folder, no `src/`                 |
|  [08]   | `module-name`       | `rasm.<name>`                   | `uv_build` derives `rasm_<name>` from the name                   |
|  [09]   | `import-names`      | `rasm.<name>`                   | PEP 794 row naming the module `module-name` builds               |
|  [10]   | `import-namespaces` | `["rasm"]`                      | PEP 794 row naming the namespace every package joins             |
|  [11]   | `requires`          | `uv_build` bounded to its minor | Unbounded row warns on sdist builds                              |

## [04]-[IMPORT_NAMES]

Distribution and import names of every package:
- Packages take distribution `rasm-<name>` and import `rasm.<name>`, libraries, products, test support, and tool plugins alike
- `rasm/` holds no `__init__.py`, and every package adds its portion to one PEP 420 namespace `rasm`
- Plain import names collide in Blender's shared extension site-packages and on PyPI, and namespace `rasm` keeps every name unique
- Library import names never reach Blender's shared site-packages
- PEP 561 stub distributions take `<package>-stubs`
- Plugins a tool loads by entry point name their `rasm.<name>` module (`rasm = "rasm.refurb"`)
- `uv_build` refuses a module root outside its project ("Module root must be inside the project"), and no project folder is itself an import package
- Entry `__init__.py` files forward public names and close on `__all__`, private modules and internal subpackages take `_`

## [05]-[EDITABLE_INSTALLS]

Editable form per build backend:

| [INDEX] | [BACKEND]                            | [EDITABLE_FORM]                 | [SYS_PATH]                            |
| :-----: | :----------------------------------- | :------------------------------ | :------------------------------------ |
|  [01]   | `uv_build`, `module-root = ""`       | One `.pth` holding project path | Project folder and every folder in it |
|  [02]   | `hatchling`, `dev-mode-exact = true` | `editables` proxy modules       | Nothing mypy or ty resolve            |
|  [03]   | `setuptools`, any layout             | Import hook finder              | Nothing mypy, ty, or pyright resolve  |

Readers of each editable form:
- `dev-mode-exact` adds `Requires-Dist: editables`, a dependency the lock omits and `uv sync` never installs
- `ty` reads every path line of each `site-packages/*.pth`, and mypy reads the interpreter's `sys.path` after `site` processes them
- `uv_build` builds editables in process, and hatchling through a PEP 517 subprocess

## [06]-[BLENDER]

Blender extension loading and bundled packages:
- Blender installs the wheels of every enabled extension into one `extensions/.local/lib/python<version>/site-packages`, ahead of its bundled one
- Top-level names of bundled wheels share one namespace across every installed extension
- `vendored.py` of `@rasm/nx-distribution` unpacks Darkroom's local wheels into the staged extension and makes their absolute imports relative
- Blender loads an extension root as package `bl_ext.<repository>.<id>`, and its modules import each other relatively
- Blender bundles MaterialX, OpenEXR, `pxr`, `opentimelineio`, `openvdb`, `oslquery`, and Imath without dist-info
- Dependency closures holding their PyPI copies shadow Blender's own process-wide

## [07]-[ENVIRONMENTS]

Environments and tool sources of each task:
- `python.uv_venv_auto = "source"` of mise exports the `.venv` of the uv root around the shell's folder as `VIRTUAL_ENV`, which ty and mypy rank first
- Processes keep the `VIRTUAL_ENV` their shell exported, and ty or mypy started outside `uv run` in another uv root checks against that root's `site-packages`
- `@rasm/nx-uv` runs ty once per uv root and mypy, refurb, and pytest once per member, through `uv run --frozen --exact --no-active`
- Option `environmentPath` holds one environment per project and dependency group, `<environmentPath>/<group>`, or `.venv` for a tool the project itself declares
- `uv run --isolated` builds a temporary environment per run where `environmentPath` is absent
- Tasks of a uv root select `--all-packages` and tasks of a member `--package <member>`, so a uv root that is itself a checked member checks in the wider one
- `check--uv` runs `uv lock --check`, the one task failing on a lock its manifests disagree with
- Locks behind their manifests leave each member a project with no checker, test, or build target, and one warning names the lock and `uv lock`
- Members take a checker or `test--pytest` target where uv resolves the tool for them, from their dependencies or the first group of theirs or their root holding it
- `nx clean <member> -c cache` empties the member's folder under `environmentPath` and removes its `__pycache__` folders
- `ruff` and `tombi` come from mise `[tools]` and `vulture` from the root `dev` group on `PATH`, as hk runs them over the files or member folder it names
- Root `.venv` runs no Nx task and serves `pixi run --locked python`, `rg` over `site-packages`, editors, ty's language server, hand `uv run`, and hk's vulture
- `default-groups = "all"` reaches commands targeting the root project alone, and `uv run --package <member>` installs a root group only where `--group` names it
- `UV_PYTHON_PREFERENCE = "only-managed"` in mise `[env]` limits uv to Pythons it installs itself
- `UV_PYTHON` acts as `--python`, uv refuses a version outside a root's `requires-python`, and no `[env]` row sets it
- `RUFF_CACHE_DIR` in mise `[env]` moves the ruff cache, and a `cache-dir` row overrides it
- `UV_CACHE_DIR` in mise `[env]` keeps uv's cache inside the checkout, and a new checkout folder builds every sdist again

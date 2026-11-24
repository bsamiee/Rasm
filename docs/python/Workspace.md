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
- Members pinned to Blender's minor under the root empty its `requires-python` range, and `uv lock` fails
- Blender roots source shared libraries supporting Blender's minor through `{ workspace = "<path>" }`, with their own root's sources
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

| [INDEX] | [FIELD]             | [VALUE]                         | [REASON]                                             |
| :-----: | :------------------ | :------------------------------ | :--------------------------------------------------- |
|  [01]   | `module-root`       | `""`                            | `rasm/` folder sits in project folder, no `src/`     |
|  [02]   | `module-name`       | `rasm.<name>`                   | `uv_build` derives `rasm_<name>` from the name       |
|  [03]   | `import-names`      | `rasm.<name>`                   | PEP 794 row naming the module `module-name` builds   |
|  [04]   | `import-namespaces` | `["rasm"]`                      | PEP 794 row naming the namespace every package joins |
|  [05]   | `requires`          | `uv_build` bounded to its minor | Unbounded row warns on sdist builds                  |

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
- Darkroom keeps hatchling for `dynamic = ["version"]`, as dynamic fields make uv run a metadata build under `package = false`
- `setuptools`, uv's fallback backend, refuses a flat folder holding more than one top-level package
- Blender bundles MaterialX, OpenEXR, `pxr`, `opentimelineio`, `openvdb`, `oslquery`, and Imath without dist-info
- Dependency closures holding their PyPI copies shadow Blender's own process-wide

## [07]-[ENVIRONMENT]

Environment variables and tool sources the roots share:
- `mise` `python.uv_venv_auto = "source"` exports root `.venv` as `VIRTUAL_ENV`, the environment ty and mypy rank first
- `ty check` or mypy run from a nested root without `uv run --no-active` checks Blender code against the root's `site-packages`
- `nx-uv` runs every checker and test through `uv run --no-active --locked --package <member>`
- `UV_PYTHON` acts as `--python` and overrides a Blender root's range, and no `[env]` row sets it
- `ruff` comes from mise `[tools]`, as hk runs it at file level from `PATH`
- `RUFF_CACHE_DIR` in mise `[env]` moves the ruff cache, and a `cache-dir` row overrides it
- `UV_CACHE_DIR` in mise `[env]` keeps uv's cache inside the checkout, and a new checkout folder builds every sdist again

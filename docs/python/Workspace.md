# [WORKSPACE]

uv workspace layout of the Python packages Nx runs.

## [01]-[PRINCIPLES]

- `uv_build` builds every package, and hatchling builds a package alone where a build hook or a dynamic field needs it
- One uv workspace resolves one `requires-python`, so each Python minor takes its own uv root
- Siblings resolve through editable installs, which expose the import names their wheels expose
- Manifests state PEP 794 import names, so tools read the import name in place of guessing it from the distribution name

## [02]-[ROOTS]

| [INDEX] | [ROOT]                                          | [PYTHON]   | [HOLDS]                                               |
| :-----: | :---------------------------------------------- | :--------- | :---------------------------------------------------- |
|  [01]   | `pyproject.toml`                                | `>=3.15`   | Shared libraries, CLI products, `tools/refurb`, tests |
|  [02]   | `libs/python/blender/pyproject.toml`            | `==3.13.*` | Blender libraries, OpenColorIO stubs, Blender suites  |
|  [03]   | `products/apps/blender/darkroom/pyproject.toml` | `==3.13.*` | Darkroom, `package = false`, Blender libraries        |

- `uv` enforces one `requires-python` per workspace, and a `==3.13.*` member under a `>=3.15` root empties the range so `uv lock` fails
- Constraints, overrides, build constraints, and `environments` come from the locking root alone, and uv ignores a member `[tool.uv]` silently
- `{ workspace = "<path>" }` sources a member of another root with that root's sources, which brings `>=3.13` shared libraries into the 3.13 roots
- `environments` limits the platforms uv solves for
- `required-environments` makes `uv lock` fail when a package lacks a wheel or source distribution for a listed platform
- `minimum-libc-version` constrains wheels only through `required-environments`
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
requires-python   = ">=3.13"
import-names      = ["<import name>"]
dependencies      = ["<dependency>"]

[project.urls]
Homepage = "https://github.com/<owner>/<repository>/tree/main/<project path>"

[build-system]
requires      = ["uv_build>=0.13.8,<0.14.0"]
build-backend = "uv_build"

[tool.uv.build-backend]
module-name = "<import name>"
module-root = ""
```

| [INDEX] | [FIELD]                      | [VALUE]                         | [REASON]                                                  |
| :-----: | :--------------------------- | :------------------------------ | :-------------------------------------------------------- |
|  [01]   | `module-name`                | Import name                     | Module `uv_build` builds when it differs from the name    |
|  [02]   | `module-root`                | `""`                            | Module folder sits in the project folder, no `src/`       |
|  [03]   | `import-names`               | Import name                     | PEP 794 row tools read in place of guessing from the name |
|  [04]   | `requires`                   | `uv_build` bounded to its minor | Unbounded row warns on sdist builds                       |
|  [05]   | `classifiers`                | `Private :: Do Not Upload`      | Publish switch nx-uv reads                                |
|  [06]   | `[tool.ruff] extend`         | Root `pyproject.toml`           | `>=3.13` members, ruff targets their range                |
|  [07]   | `dependencies = []`          | Absent                          | Empty list is the absent value                            |
|  [08]   | `Typing :: Typed` classifier | Absent                          | No tool reads it                                          |

## [04]-[IMPORT_NAMES]

- `uv_build`'s default module is the normalized distribution name (`rasm_<name>`), and `module-name` sets another
- `uv_build` refuses a module root outside the project ("Module root must be inside the project"), so a project folder cannot itself be the package
- Entry `__init__.py` files forward public names and close on `__all__`, private modules and internal subpackages take `_`
- `[project.entry-points."refurb.plugins"]` names `rasm.refurb`, and refurb loads that module and walks every module under it
- PyPI holds projects named `records`, `telemetry`, `foundry`, `extension`, and `rendering`, so bare top-level names collide

## [05]-[EDITABLE_INSTALLS]

| [INDEX] | [BACKEND]                             | [EDITABLE_FORM]                     | [ON_SYS_PATH]                         |
| :-----: | :------------------------------------ | :---------------------------------- | :------------------------------------ |
|  [01]   | `uv_build`, `module-root = ""`        | One `.pth` holding the project path | Project folder and every folder in it |
|  [02]   | `hatchling`, `dev-mode-dirs = [".."]` | One `.pth` holding the parent       | Every sibling project folder          |
|  [03]   | `hatchling`, `dev-mode-exact = true`  | `editables` proxy modules           | Nothing mypy or ty resolve            |
|  [04]   | `setuptools`, any layout              | Import hook finder                  | Nothing mypy, ty, or pyright resolve  |

- `hatchling` detection mode refuses a `sources` rewrite that adds a prefix, so a package folder equal to its project folder needs `dev-mode-dirs`
- `dev-mode-exact` adds `Requires-Dist: editables`, which `uv sync` never installs, as it installs from the lock
- `ty` reads every path line of each `site-packages/*.pth`, and mypy reads the interpreter's `sys.path` after `site` processes them
- `uv_build` builds editables in process, and hatchling through a PEP 517 subprocess

## [06]-[BLENDER]

- Blender installs the wheels of every enabled extension into one `extensions/.local/lib/python3.13/site-packages`, ahead of its bundled one
- Top-level names of bundled wheels therefore share one namespace across every installed extension
- Darkroom's extension build (`vendored.py` of `@rasm/nx-distribution`) unpacks local wheels into the staged extension
- Script `vendored.py` rewrites each absolute import of their top-level names to a relative one, so the libraries sit inside the extension package
- Blender loads an extension as `bl_ext.<repository>.<id>`, so the extension root is its package and its modules import each other relatively
- Darkroom keeps hatchling for `dynamic = ["version"]`, read from `blender_manifest.toml`
- Dynamic fields make uv run a metadata build even under `package = false`, falling back to setuptools without a backend
- `setuptools` refuses a flat folder holding more than one top-level package
- Blender bundles MaterialX, OpenEXR, `pxr`, `opentimelineio`, `openvdb`, `oslquery`, and Imath without dist-info
- Dependency closures holding their PyPI copies shadows Blender's own process-wide

## [07]-[ENVIRONMENT]

- `mise` `python.uv_venv_auto = "source"` exports the root `.venv` as `VIRTUAL_ENV`, which ty and mypy rank first
- Bare `ty check` or mypy runs from a nested root then checks 3.13 code against 3.15 `site-packages`, and `uv run --no-active` avoids it
- `nx-uv` runs every checker and test through `uv run --no-active --locked --package <member>`
- `UV_PYTHON` acts as `--python` and overrides a root's `==3.13.*`, so no `[env]` row sets it
- `UV_CACHE_DIR` sits per checkout, as wheels uv builds from sdists record the checkout's Pixi library path
- `ruff` and yamllint come from mise `[tools]`, since hk runs them at file level from `PATH`
- `RUFF_CACHE_DIR` in mise `[env]` moves the ruff cache, and a `cache-dir` row would override that variable

# [PYTHON]

uv owns resolution, the lock, and the environment of the root project file.

## [01]-[GROUPS]

- Version bounds exist for a resolver conflict, stated in the row comment
- `default-groups` names the groups `uv sync` installs, `"all"` every group, `--only-group <group>` one group without the project
- `prerelease = "allow"` accepts prereleases for every package

## [02]-[LOCK]

- `--locked` fails a command when `uv.lock` is missing or stale
- `environments` restricts resolution to disjoint PEP 508 markers
- `required-environments` names platforms a package without a source distribution must publish a wheel for
- `[tool.uv.workspace] members` globs name package manifests one root lock covers
- Dependencies on a member take `{ workspace = true }` in `[tool.uv.sources]`

## [03]-[ENVIRONMENT]

- `uv sync` installs the workspace root, `--all-packages` every member
- `python-preference = "only-system"` excludes uv-managed interpreters, `UV_PYTHON` names the interpreter
- Automation with control flow is a package with a `[project.scripts]` entry a target runs by name

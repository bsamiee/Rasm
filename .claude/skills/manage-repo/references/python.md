# [PYTHON]

## [01]-[PROJECT_FILE]

- One `pyproject.toml` at the root holds the dependency groups and every tool table
- Tools with a table (`ruff`, `mypy`, `ty`, `pytest`, `coverage`) are configured in `pyproject.toml` alone
- Dependencies are unversioned names in their group
- Version bounds state a resolver fact the row comments
- Groups are named for their consumer

## [02]-[LOCK]

- Lock file is the only pin
- Upgrades move the lock
- `uv sync --locked` proves the lock matches the project file without writing a lock
- `[tool.uv] environments` lists disjoint PEP 508 markers the resolver locks for
- `[tool.uv] required-environments` lists the platforms a wheel must exist for

## [03]-[ENVIRONMENT]

- Virtual environment sits at the root with `bin` on the path
- Checkers run by name
- Interpreter comes from the tool manager
- Project file names the minimum interpreter version
- `uv run` prefixes around a checker signal a path without the environment, put the environment `bin` on the path

## [04]-[PACKAGES]

- `[tool.uv.workspace] members` globs name the packages
- Each package directory holds its own `pyproject.toml`
- Task graph infers a project from each package `pyproject.toml`, tags it, and applies the language defaults
- Script directories under an engineering tree with a dependency group and a shared library signal tooling wrapped in Python
- Automation with real logic is a package with a command entry point
- One-line tool calls are targets

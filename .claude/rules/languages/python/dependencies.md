---
paths:
  - "**/*.py"
  - "pyproject.toml"
---

Python runs and dependencies go through `uv` and the root manifest:
- `uv` for every run
- Dependencies from the root `pyproject.toml` groups, and the packages the groups already hold before a new addition

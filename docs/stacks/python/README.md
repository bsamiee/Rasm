# [STACKS_PYTHON]

This folder is the Python stack decision atlas. It routes language, shape, surface, rail, boundary, runtime, algorithm, platform, and proof decisions to the concept page that owns the coding choice.

## [1]-[CHOOSE]

This table is a lookup by reader decision.

| [INDEX] | [DECISION]               | [READ]                                    |
| :-----: | :----------------------- | :---------------------------------------- |
|   [1]   | language syntax          | [language](language.md)                   |
|   [2]   | domain shape             | future `domain-shapes.md`                 |
|   [3]   | surface and dispatch     | future `surfaces-and-dispatch.md`         |
|   [4]   | result and effect flow   | future `rails-and-effects.md`             |
|   [5]   | boundaries and codecs    | future `boundaries-and-codecs.md`         |
|   [6]   | runtime and concurrency  | future `runtime-and-concurrency.md`       |
|   [7]   | algorithms and data      | future `algorithms-and-data.md`           |
|   [8]   | package or platform fact | future `platform/build-and-packages.md`   |
|   [9]   | proof rail               | future `testing/README.md`                |
|  [10]   | planned stack buildout   | [roadmap](.planning/ROADMAP.md)           |
|  [11]   | planned stack topology   | [architecture](.planning/ARCHITECTURE.md) |

## [2]-[OWNER_RULE]

Concept pages own coding decisions. Package versions, tool versions, optional dependency groups, and static-analysis configuration live in `pyproject.toml`; package-backed decisions live in the concept page that owns the capability.

Approved libraries are normal Python implementation material. Do not create package-named pages or folders; write the domain rule in the page that owns the decision and mention the package only when it changes implementation choice.

When the stronger package is not admitted yet, keep the target capability in the owning concept and record the adoption gap in the current planning or platform route.

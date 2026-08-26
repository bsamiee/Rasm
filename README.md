# [RASM_WORKSPACE]

Rasm is a polyglot monorepo whose `libs/` branches hold independently adoptable C#, Python, and TypeScript capability, and whose apps, plugins, and services compose those repos exactly as they take an external package.

## [01]-[MAP]

| [INDEX] | [TREE]  | [HOLDS]                                                                         |
| :-----: | :------ | :------------------------------------------------------------------------------ |
|  [01]   | `libs`  | independently adoptable language branches                                       |
|  [02]   | `apps`  | Each `apps/<app-name>/` holds one app's projects across any number of languages |
|  [03]   | `tools` | Repo tools and operators                                                        |
|  [04]   | `tests` | C#, Python, and TypeScript test suites                                          |
|  [05]   | `docs`  | Location for all durable documentation                                          |

## [02]-[TOOL_OWNERS]

| [INDEX] | [TOOL]               | [ROLE]                                                                       |
| :-----: | :------------------- | :--------------------------------------------------------------------------- |
|  [01]   | `tools/rhino-bridge` | Live Rhino owner: host lifecycle, scenario execution, cargo, spool, evidence |
|  [02]   | `tools/biome`        | Promoted GritQL lint rules the root `biome.json` registers at error          |
|  [03]   | `tools/yak`          | Tracked Yak package manifests, one per package slug                          |
|  [04]   | `Parametric_Forge`   | Sibling repo owning machine composition, executables, and credential policy  |

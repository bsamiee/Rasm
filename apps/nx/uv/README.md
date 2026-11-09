# [NX_UV]

`@rasm/nx-uv` is an Nx 24 plugin for uv workspaces. Projects, edges, external nodes, and targets come from `uv workspace metadata` per workspace root, and from the checker tables each project's configuration files declare.

## [01]-[USE]

```json
{ "plugins": [{ "plugin": "@rasm/nx-uv", "options": { "outputPath": ".artifacts" } }] }
```

- `outputPath`: folder, relative to the workspace root, holding `uv/<member>` distributions, since uv reads no setting for its output
- Unset, uv builds into `<uv root>/dist/<member>`
- Every other fact comes from each tool's own configuration
- Every member it versions carries the `release` tag, so `nx.json` selects Python releases with `tag:release` in `release.projects`, since `nx release` selects npm libraries alone by default

## [02]-[INFERENCE]

| [INDEX] | [SOURCE]                                        | [GIVES]                                                               |
| :-----: | :---------------------------------------------- | :-------------------------------------------------------------------- |
|  [01]   | `uv.lock`                                       | One workspace root, read through one `uv workspace metadata`          |
|  [02]   | Metadata `members` and `workspace`              | One project per member as uv names it, and one for a virtual root     |
|  [03]   | Metadata `resolution`                           | `pypi:<root>/<name>@<version>` external nodes, keyed per locking root |
|  [04]   | Member dependencies, groups, extras             | Implicit edges to members, other roots' members, and external nodes   |
|  [05]   | Checker tables reachable from the member folder | `check--<tool>` and `test` targets                                    |

- Stale locks read as uv resolves them, and each task's `--locked` and `check--uv` report them with uv's own message
- Roots uv cannot read (a manifest that does not parse) log uv's message and keep their project with `check--uv`, which fails with that message
- Checker configuration files that do not parse hold no table for inference, and each tool reports the error when its target runs
- Virtual roots (no `[project]`) take the folder name, Nx's own rule for a project no manifest names, and hold the workspace targets
- Members whose `requires-python` floor differs from their root's run ty, mypy, and refurb with `--python-version <floor>`
- Members of another root reached through `{ workspace = "<path>" }` sources become edges to that root's project
- Inference caches by every manifest, lock, checker configuration file, and plugin source file
- Each graph build globs the files a checker reads (`*.py`, `*.pyi`, pytest's `python_files`), and members holding none get no target for it
- Members whose folder holds other members get only the workspace targets
- Paths `.gitignore` or `.nxignore` lists stay out

## [03]-[TARGETS]

| [INDEX] | [TARGET]             | [COMMAND]                                                  | [WHEN]                                    | [CACHE] |
| :-----: | :------------------- | :--------------------------------------------------------- | :---------------------------------------- | :-----: |
|  [01]   | `check--ty`          | `ty check <member>`                                        | `ty.toml` or `[tool.ty]` reachable        |   Yes   |
|  [02]   | `check--mypy`        | `mypy <member>`                                            | `mypy.ini`, `[tool.mypy]`, or `setup.cfg` |   Yes   |
|  [03]   | `check--refurb`      | `refurb --config-file <file> .` in the member folder       | `[tool.refurb]` reachable                 |   Yes   |
|  [04]   | `test`               | `pytest <member>`                                          | Test file and pytest configuration        |   Yes   |
|  [05]   | `check--uv`          | `uv lock --check`                                          | Workspace root                            |   Yes   |
|  [06]   | `upgrade--uv`        | `uv lock --upgrade`                                        | Workspace root                            |   No    |
|  [07]   | `build--uv`          | `uv build --package <member> --no-sources --out-dir <dir>` | Member with a build backend               |   Yes   |
|  [08]   | `nx-release-publish` | `uv publish <build--uv output>/*`                          | Released member (`[04]-[RELEASE]`)        |   No    |
|  [09]   | `<verb>`             | `nx:noop` over `<verb>--*`                                 | Any `<verb>--*` target                    |   No    |

- Checkers and `test` run as `uv run --locked --package <member> --group <group>`, the group holding the tool, so its stubs and plugins install too
- Tools only the member's own dependencies hold run without `--group`
- Targets exist only where their tool is locked in reach of the member, so a checker a workspace never locks gets none
- `--locked` fails a task on a stale lock with uv's own message, and never writes `uv.lock`
- `ty` receives the member path, since from a member folder it checks the whole workspace
- `TY_UV` reaches ty from the caller's environment (`uv check` sets `1`, adding undeclared-dependency findings), and the plugin hashes it as an `env` input
- Checker inputs: member sources, configuration files present on the discovery path up to the one the tool reads, the root `.python-version`
- Dependency members' files and the selected group's external nodes join them; `test` reads every member file outside the build folder
- Aggregates take the form every plugin declares (`dependsOn: ["<verb>--*"]`), their inputs `'...'` then the union of their members' inputs
- Arguments after `--` go to a tool-named target, since Nx forwards them to every dependency of an aggregate
- `nx affected -t <verb>` selects an aggregate on any input of its dependencies, and `'...'` keeps other plugins' inputs for the project
- `build--uv` outputs `<outputPath>/uv/<member>` alone, emptied first through `--clear`, and `nx-release-publish` uploads the folder `build--uv` declares, so each member publishes its own files
- `nx-release-publish` runs after `build--uv`, skips a project `nx release` left unversioned, and maps `dryRun` or `NX_DRY_RUN` to `--dry-run`
- `index` names the `[[tool.uv.index]]` entry passed as `--index`, whose `url` uv checks before upload

## [04]-[RELEASE]

Members with a build backend whose classifiers are static and carry no `Private ::` entry (PyPI rejects any upload carrying one, and uv documents `Private :: Do Not Upload` as the mark; dynamic or unreadable classifiers infer no publish), take the `release` tag and `release.version.versionActions: "@rasm/nx-uv/release/version-actions"`, which versions `pyproject.toml`:
- Reads `[project].version` through a TOML parser and converts PEP 440 to SemVer (`1.0.0a1` reads as `1.0.0-alpha.1`); an epoch, post, dev, or local part fails by name
- Writes the PEP 440 form into Nx's tree with layout and comments kept (`1.0.0-rc.2` writes `1.0.0rc2`); a prerelease id other than `alpha`, `beta`, or `rc` fails by name
- Reads the registry version as the newest of the `versions` the index's PEP 691 JSON page lists (PEP 700), from PyPI or the `[[tool.uv.index]]` entry `currentVersionResolverMetadata.index` names in the root's `uv.toml` or `pyproject.toml`
- Writes each released sibling as a floor (`rasm-records>=1.2.0`) into the dependent's dependencies, so a published dependent never accepts an older sibling
- Keeps the extras and marker `uv.lock` records for each rewritten requirement
- `versionActionsOptions.lock` names the member's `uv.lock`, and `afterAllProjectsVersioned` runs one `uv lock` per lock the graph's versioned projects name, so no other tool's lock (mise's tool sidecars) is touched

## [05]-[STATE]

| [INDEX] | [STATE]       | [LOCATION]                 | [STATED_BY]                                                                 |
| :-----: | :------------ | :------------------------- | :-------------------------------------------------------------------------- |
|  [01]   | Environment   | `<workspace root>/.venv`   | Default of uv, shared with editors                                          |
|  [02]   | Packages      | Per-user uv cache          | `uv cache dir`                                                              |
|  [03]   | Checker state | Each tool's own setting    | `[tool.ruff] cache-dir`, `[tool.mypy] cache_dir`, `[tool.pytest] cache_dir` |
|  [04]   | Distributions | `<outputPath>/uv/<member>` | `outputPath` option                                                         |

## [06]-[LAYOUT]

| [INDEX] | [PATH]                       | [HOLDS]                                                                           |
| :-----: | :--------------------------- | :-------------------------------------------------------------------------------- |
|  [01]   | `plugins/project-graph.ts`   | Package entry: metadata decoder, graph cache, `createNodes`, `createDependencies` |
|  [02]   | `plugins/targets.ts`         | Checker table, configuration discovery, target builders                           |
|  [03]   | `version-actions.ts`         | `VersionActions` and `afterAllProjectsVersioned`                                  |
|  [04]   | `executors/release-publish/` | `nx-release-publish` executor and its Nx schema                                   |

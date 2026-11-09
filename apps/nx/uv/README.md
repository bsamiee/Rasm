# [NX_UV]

`@rasm/nx-uv` is an Nx 24 plugin for uv workspaces. Projects, edges, external nodes, and targets come from `uv workspace metadata` per workspace root and from the checker tables each project's configuration files declare.

## [01]-[USE]

```json
{ "plugins": ["@rasm/nx-uv"] }
```

- The plugin takes no options: each tool's own configuration states every fact it reads
- Releases select Python projects through `release.projects` or `release.groups` in `nx.json`, since `nx release` selects npm libraries alone by default

## [02]-[INFERENCE]

| [INDEX] | [SOURCE]                                        | [GIVES]                                                               |
| :-----: | :---------------------------------------------- | :-------------------------------------------------------------------- |
|  [01]   | `uv.lock`                                       | One workspace root, read through one `uv workspace metadata --locked` |
|  [02]   | Metadata `members`                              | One project per member, named and placed as uv reports it             |
|  [03]   | Metadata `resolution`                           | `pypi:<name>@<version>` external nodes hashed from artifact hashes    |
|  [04]   | Member dependencies, groups, extras             | Implicit edges to members, other roots' members, and external nodes   |
|  [05]   | Checker tables reachable from the member folder | `check--<tool>` and `test` targets                                    |

- Inference fails by name when the metadata schema moves, a lock is stale, or a manifest does not parse, and keeps every other root's projects
- Members of another root reached through `{ workspace = "<path>" }` sources become edges to that root's project
- Inference caches by every manifest, lock, and checker configuration file, and by plugin source; `test` detection globs test files on each graph build
- A member whose folder holds other members gets only the workspace targets
- Paths `.gitignore` or `.nxignore` lists stay out

## [03]-[TARGETS]

| [INDEX] | [TARGET]             | [COMMAND]                                                                  | [WHEN]                                    | [CACHE] |
| :-----: | :------------------- | :------------------------------------------------------------------------- | :---------------------------------------- | :-----: |
|  [01]   | `check--ty`          | `ty check <member>`, `TY_UV=1`                                             | `ty.toml` or `[tool.ty]` reachable        |   Yes   |
|  [02]   | `check--mypy`        | `mypy <member>`                                                            | `mypy.ini`, `[tool.mypy]`, or `setup.cfg` |   Yes   |
|  [03]   | `check--refurb`      | `refurb --config-file <file> .` from the member folder                     | `[tool.refurb]` reachable                 |   Yes   |
|  [04]   | `test`               | `pytest <member>`                                                          | pytest configuration and a test file      |   Yes   |
|  [05]   | `check--uv`          | `uv lock --check`                                                          | Workspace root                            |   Yes   |
|  [06]   | `upgrade--uv`        | `uv lock --upgrade`                                                        | Workspace root                            |   No    |
|  [07]   | `build`              | `uv build --package <member> --no-sources --out-dir <member>/dist --clear` | Member with a build backend               |   Yes   |
|  [08]   | `nx-release-publish` | `uv publish <member>/dist/*`                                               | Member with a build backend               |   No    |
|  [09]   | `check`, `upgrade`   | `nx:noop` over `check--*`, `upgrade--*`                                    | Any `check--*` or `upgrade--*`            |   No    |

- Checkers and `test` run as `uv run --locked --package <member> --group <group>`, the group holding the tool, so its stubs and plugins install too
- Tools only the member's own dependencies hold run without `--group`
- A target exists only when its tool is locked in reach of the member; a workspace declaring a checker it never locks gets none
- `--locked` fails a task on a stale lock with uv's own message, and never writes `uv.lock`
- `ty` receives the member path, since from a member folder it checks the whole workspace
- `TY_UV=1` is the setting `uv check` gives ty, adding undeclared-dependency findings
- Checker inputs: member sources, configuration files present on the discovery path up to the one the tool reads, the root `.python-version`
- Dependency members' files and the selected group's external nodes join them; `test` reads every member file outside `dist`
- `check` declares the union of its dependencies' inputs, so `nx affected -t check` selects it on any of them
- `check` and `upgrade` are the aggregate every plugin declares alike (`dependsOn: ["check--*"]`), and `check` inputs open with `'...'`, keeping the inputs other plugins infer for the same project
- `build` outputs `{projectRoot}/dist`, so `uv publish` uploads that member's files alone
- `nx-release-publish` runs after `build`, skips a project `nx release` left unversioned, and maps `dryRun` (or `NX_DRY_RUN`) to `--dry-run`, `registry` to `--publish-url`, and `index` to `--index`

## [04]-[RELEASE]

Members with a build backend take `release.version.versionActions: "@rasm/nx-uv/release/version-actions"`:
- Reads `[project].version` through a TOML parser and converts PEP 440 to SemVer (`1.0.0a1` reads as `1.0.0-alpha.1`); an epoch, post, dev, or local part fails by name
- Writes the PEP 440 form into Nx's tree with layout and comments kept (`1.0.0-rc.2` writes `1.0.0rc2`); a prerelease id other than `alpha`, `beta`, or `rc` fails by name
- Reads the registry version from the PyPI JSON API, or the `registry` in `currentVersionResolverMetadata`
- `afterAllProjectsVersioned` runs one `uv lock` per workspace root and reports the locks it changed

## [05]-[STATE]

| [INDEX] | [STATE]       | [LOCATION]                          | [STATED_BY]                     |
| :-----: | :------------ | :---------------------------------- | :------------------------------ |
|  [01]   | Environment   | `<workspace root>/.venv`            | uv default, shared with editors |
|  [02]   | Packages      | Per-user uv cache                   | `uv cache dir`                  |
|  [03]   | mypy cache    | `.mypy_cache` in the working folder | mypy default                    |
|  [04]   | pytest cache  | `.pytest_cache` in `rootdir`        | pytest default                  |
|  [05]   | Distributions | `<member>/dist`                     | `build` target                  |

## [06]-[LAYOUT]

| [INDEX] | [PATH]                       | [HOLDS]                                                                           |
| :-----: | :--------------------------- | :-------------------------------------------------------------------------------- |
|  [01]   | `plugins/workspace.ts`       | Package entry: metadata decoder, graph cache, `createNodes`, `createDependencies` |
|  [02]   | `plugins/targets.ts`         | Checker table, configuration discovery, target builders                           |
|  [03]   | `version-actions.ts`         | `VersionActions` and `afterAllProjectsVersioned`                                  |
|  [04]   | `executors/release-publish/` | `nx-release-publish` executor and its Nx schema                                   |

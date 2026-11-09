# [NX_UV]

`@rasm/nx-uv` is an Nx 24 plugin for uv workspaces. Projects, edges, external nodes, and targets come from `uv workspace metadata` per workspace root, from the checker tables each project's configuration files declare, and from the `blender_manifest.toml` of a member that is a Blender extension.

## [01]-[USE]

```json
{ "plugins": [{ "plugin": "@rasm/nx-uv", "options": { "outputPath": ".artifacts" } }] }
```

- `outputPath`: folder, relative to the workspace root, holding `uv/<member>` distributions and `blender/<id>` extension archives, since neither tool reads a setting for its output
- Unset, uv builds into `<uv root>/dist/<member>` and Blender into the extension folder, its own default
- Every other fact comes from each tool's own configuration
- Every member it versions carries the `release` tag, so `nx.json` selects Python releases with `tag:release` in `release.projects`, since `nx release` selects npm libraries alone by default

## [02]-[INFERENCE]

| [INDEX] | [SOURCE]                                        | [GIVES]                                                                    |
| :-----: | :---------------------------------------------- | :------------------------------------------------------------------------- |
|  [01]   | `uv.lock`                                       | One workspace root, read through one `uv workspace metadata --locked`      |
|  [02]   | Metadata `members`                              | One project per member, named and placed as uv reports it                  |
|  [03]   | Metadata `resolution`                           | `pypi:<root>/<name>@<version>` external nodes, keyed per locking root      |
|  [04]   | Member dependencies, groups, extras             | Implicit edges to members, other roots' members, and external nodes        |
|  [05]   | Checker tables reachable from the member folder | `check--<tool>` and `test` targets                                         |
|  [06]   | `blender_manifest.toml` in a member folder      | `build--blender`, with archive names from its `id`, `version`, `platforms` |

- Inference fails by name when the metadata schema moves, a lock is stale, or a manifest does not parse, and keeps every other root's projects
- Members of another root reached through `{ workspace = "<path>" }` sources become edges to that root's project
- Inference caches by every manifest, lock, and checker configuration file, and by plugin source; each graph build globs the files a checker reads (`*.py`, `*.pyi`, pytest's `python_files`), and a member holding none gets no target for it
- A member whose folder holds other members gets only the workspace targets
- Paths `.gitignore` or `.nxignore` lists stay out

## [03]-[TARGETS]

| [INDEX] | [TARGET]                    | [COMMAND]                                                          | [WHEN]                                    | [CACHE] |
| :-----: | :-------------------------- | :----------------------------------------------------------------- | :---------------------------------------- | :-----: |
|  [01]   | `check--ty`                 | `ty check <member>`                                                | `ty.toml` or `[tool.ty]` reachable        |   Yes   |
|  [02]   | `check--mypy`               | `mypy <member>`                                                    | `mypy.ini`, `[tool.mypy]`, or `setup.cfg` |   Yes   |
|  [03]   | `check--refurb`             | `refurb --config-file <file> .` from the member folder             | `[tool.refurb]` reachable                 |   Yes   |
|  [04]   | `test`                      | `pytest <member>`                                                  | pytest configuration and a test file      |   Yes   |
|  [05]   | `check--uv`                 | `uv lock --check`                                                  | Workspace root                            |   Yes   |
|  [06]   | `upgrade--uv`               | `uv lock --upgrade`                                                | Workspace root                            |   No    |
|  [07]   | `build--uv`                 | `uv build --package <member> --no-sources --out-dir <dir> --clear` | Member with a build backend               |   Yes   |
|  [08]   | `build--blender`            | `blender --command extension build`, `-c release` bundling wheels  | Member holding `blender_manifest.toml`    |   Yes   |
|  [09]   | `nx-release-publish`        | `uv publish <build--uv output>/*`                                  | Member with a build backend and no `Private ::` classifier |   No    |
|  [10]   | `check`, `build`, `upgrade` | `nx:noop` over `<verb>--*`                                         | Any `<verb>--*` target                    |   No    |

- Checkers and `test` run as `uv run --locked --package <member> --group <group>`, the group holding the tool, so its stubs and plugins install too
- Tools only the member's own dependencies hold run without `--group`
- A target exists only when its tool is locked in reach of the member; a workspace declaring a checker it never locks gets none
- `--locked` fails a task on a stale lock with uv's own message, and never writes `uv.lock`
- `ty` receives the member path, since from a member folder it checks the whole workspace
- ty reads `TY_UV` from the caller's environment (`uv check` sets `1`, adding undeclared-dependency findings); the plugin sets no value and hashes it as an `env` input
- Checker inputs: member sources, configuration files present on the discovery path up to the one the tool reads, the root `.python-version`
- Dependency members' files and the selected group's external nodes join them; `test` reads every member file outside the build folder
- Each aggregate is the one every plugin declares alike (`dependsOn: ["<verb>--*"]`; arguments after `--` go to a tool-named target, since Nx passes forwarded arguments to every dependency's members too), its inputs `'...'` then the union of its dependencies' inputs
- `nx affected -t <verb>` selects an aggregate on any input of its dependencies, and `'...'` keeps other plugins' inputs for the project
- `build--uv` outputs `<outputPath>/uv/<member>` alone, and `nx-release-publish` uploads the folder `build--uv` declares, so each member publishes its own files
- `build--blender` outputs the archives Blender names from the manifest, `<id>-<version>.zip` or one `<id>-<version>-<platform>.zip` per `platforms` entry, under its `outputPath` option: `<outputPath>/blender/<id>`, and `<outputPath>/blender/<id>/release` in the `release` configuration, since both configurations name their archives alike and one would overwrite the other
- `@rasm/nx-distribution` installs and publishes those archives; `blender` resolves from `PATH`, where Blender's manual puts it on each platform
- `-c release` stages the extension under `<outputPath>/blender/<id>/release/source` for the running Blender's platform alone, with a manifest naming that platform and its wheels
- Staged wheels: `uv export` pins fetched by `pip download` per tag Blender's interpreter accepts, and each workspace dependency built by `uv build --wheel`
- Wheels whose top-level modules Blender's own `site-packages` holds stay out, since a wheel replaces a bundled copy process-wide
- `nx-release-publish` runs after `build`, skips a project `nx release` left unversioned, and maps `dryRun` (or `NX_DRY_RUN`) to `--dry-run`, `registry` to `--publish-url`, and `index` to `--index`

## [04]-[RELEASE]

Members with a build backend and no `Private ::` classifier (PyPI rejects any upload carrying one, and uv documents `Private :: Do Not Upload` as the mark), and members holding a `blender_manifest.toml`, take the `release` tag and `release.version.versionActions: "@rasm/nx-uv/release/version-actions"`, with `versionActionsOptions.manifest` naming the file holding the version: `blender_manifest.toml` for an extension (Blender reads `version` from it alone), else `pyproject.toml`:
- Reads and writes an extension's `version` as written, since Blender requires SemVer
- Reads `[project].version` through a TOML parser and converts PEP 440 to SemVer (`1.0.0a1` reads as `1.0.0-alpha.1`); an epoch, post, dev, or local part fails by name
- Writes the PEP 440 form into Nx's tree with layout and comments kept (`1.0.0-rc.2` writes `1.0.0rc2`); a prerelease id other than `alpha`, `beta`, or `rc` fails by name
- Reads the registry version from the PyPI JSON API, or the `registry` in `currentVersionResolverMetadata`; an extension has no registry version and fails by name
- `afterAllProjectsVersioned` runs one `uv lock` per workspace root and reports the locks it changed

## [05]-[STATE]

| [INDEX] | [STATE]       | [LOCATION]                  | [STATED_BY]                                                                 |
| :-----: | :------------ | :-------------------------- | :-------------------------------------------------------------------------- |
|  [01]   | Environment   | `<workspace root>/.venv`    | uv default, shared with editors                                             |
|  [02]   | Packages      | Per-user uv cache           | `uv cache dir`                                                              |
|  [03]   | Checker state | Each tool's own setting     | `[tool.ruff] cache-dir`, `[tool.mypy] cache_dir`, `[tool.pytest] cache_dir` |
|  [04]   | Distributions | `<outputPath>/uv/<member>`  | `outputPath` option                                                         |
|  [05]   | Archives      | `<outputPath>/blender/<id>`, `-c release` in `<outputPath>/blender/<id>/release` | `outputPath` option, one folder per configuration |

## [06]-[LAYOUT]

| [INDEX] | [PATH]                       | [HOLDS]                                                                              |
| :-----: | :--------------------------- | :----------------------------------------------------------------------------------- |
|  [01]   | `plugins/workspace.ts`       | Package entry: metadata decoder, graph cache, `createNodes`, `createDependencies`    |
|  [02]   | `plugins/targets.ts`         | Checker table, configuration discovery, target builders                              |
|  [03]   | `version-actions.ts`         | `VersionActions` and `afterAllProjectsVersioned`                                     |
|  [04]   | `executors/release-publish/` | `nx-release-publish` executor and its Nx schema                                      |
|  [05]   | `executors/extension-build/` | `build--blender` executor, its Nx schema, and the Blender interpreter report it runs |

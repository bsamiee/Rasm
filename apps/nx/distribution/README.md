# [NX_DISTRIBUTION]

`@rasm/nx-distribution` is an Nx 24 plugin that puts host packages into their hosts. `install` feeds the local build through the installer the host itself uses, and `nx-release-publish` attaches a release to its GitHub release and sends it to the host's package server where one takes uploads. Language plugins build Yak, UXP, and CEP packages; this plugin builds Blender extensions and Unreal Engine projects and plug-ins, since each host's own tool (`extension build`, UnrealBuildTool, AutomationTool's `BuildPlugin`) reads the host folder alone and no language owns it.

## [01]-[USE]

```json
{ "plugins": ["@rasm/nx-dotnet", "@rasm/nx-uv", "@rasm/nx-typescript", { "plugin": "@rasm/nx-distribution", "options": { "outputPath": ".artifacts" } }] }
```

- `outputPath`: folder, relative to the workspace root, holding `blender/<id>` extension archives and `unreal/<plug-in>` plug-in archives, since neither Blender nor BuildPlugin reads a setting for its output; unset, archives go to `dist/<project root>`, Nx's own default output
- `engines`: Unreal engine roots by association (`{ "5.8": "/opt/UnrealEngine" }`), read before Epic's install registry, for engines no registry lists (a source build Launch Services never registered, a CI machine with an extracted engine)
- List it after the language plugins, so its `nx-release-publish` replaces theirs on a host project, which Nx merges whole when the executors differ

## [02]-[INFERENCE]

| [INDEX] | [MANIFEST]              | [HOST]                    | [PACKAGE] | [TARGETS]                                                            |
| :-----: | :---------------------- | :------------------------ | :-------- | :------------------------------------------------------------------- |
|  [01]   | `blender_manifest.toml` | Blender extension         | `.zip`    | `build--blender`, `build`, `install`, `nx-release-publish`, versions |
|  [02]   | `manifest.yml`          | Rhino and Grasshopper Yak | `.yak`    | `install`, `nx-release-publish`                                      |
|  [03]   | `manifest.json`         | Adobe UXP plug-in         | `.ccx`    | `install`, `nx-release-publish`                                      |
|  [04]   | `CSXS/manifest.xml`     | Adobe CEP extension       | `.zxp`    | `install`, `nx-release-publish`                                      |
|  [05]   | `*.uproject`            | Unreal Engine project     | None      | `build--unreal`, `build`, `check--unreal`, `check`, `install`        |
|  [06]   | `*.uplugin`             | Unreal Engine plug-in     | `.zip`    | `build--unreal`, `build`, `check--unreal`, `check`, `install`, `nx-release-publish`, versions |

- Manifests name a project at their folder (`CSXS/manifest.xml` at the folder above `CSXS`), merging with the language plugin's project there
- Unreal descriptors are read as JSON and name their project by file name, Epic's identity, since no language plugin reads them; their build is their compile, so `check--unreal` is a no-op over `build--unreal`, and each module's `*.Build.cs` states its warning policy
- A plug-in's hosts are the `.uproject` files whose `Plugins/` folder or `AdditionalPluginDirectories` hold it, unless the host's `Plugins` entry disables it; a project lists its hosted plug-ins and the plug-ins its `Plugins` names as implicit dependencies, so `nx affected` on a plug-in file reaches its host
- A `.uplugin` stating `Installed: true` is a package installed on top of the engine, not source, so it names no project: its files belong to the host project, whose editor build compiles or links it as its `*.Build.cs` states (`bUsePrecompiled`), the one place UnrealBuildTool decides prebuilt modules
- Files named `manifest.yml` or `manifest.json` lacking the host's required keys (Yak `name`, `version`, `authors`, `description`; UXP `manifestVersion`, `id`, `name`, `version`) name nothing
- A `blender_manifest.toml` that does not parse or lacks `schema_version`, `id`, or `version` fails its own file alone, the other projects kept

## [03]-[TARGETS]

| [INDEX] | [TARGET]           | [RUNS]                                              | [SUCCESS]                                                 |
| :-----: | :----------------- | :-------------------------------------------------- | :-------------------------------------------------------- |
|  [01]   | `build--blender`   | `blender --command extension build`                 | Exit status                                               |
|  [02]   | Blender `install`  | `blender --command extension install-file --enable` | `STATUS Installed` or `Reinstalled "<id>"`                |
|  [03]   | Yak `install`      | `yak install <file>.yak`                            | `Successfully installed <name>`                           |
|  [04]   | UXP, CEP `install` | UPIA `--remove <name>`, `--install`                 | `-406` or `Removal Successful`, `Installation Successful` |
|  [05]   | Every publish      | `gh release upload <tag> <file>`                    | Exit status; held assets skipped                          |
|  [06]   | Yak publish        | `yak push <file>`                                   | `Successfully published`                                  |
|  [07]   | `.uproject` `build--unreal` | `RunUBT.sh -TargetType=Editor <host> Development -Project=<uproject> -WaitMutex` | Exit status |
|  [08]   | `.uplugin` `build--unreal`  | `RunUAT.sh BuildPlugin` once per host engine, each package zipped | Exit status |
|  [09]   | Unreal `install`   | The editor build of the project, or of each host of a plug-in | Exit status |

- UnrealBuildTool resolves the editor target from `-TargetType=Editor` through the project's own rules and `DefaultEditorTarget`, and builds every enabled plug-in of the project in place, where the editor loads them at its next start; `build--unreal` on a project stays uncached, since it writes the incremental state the editor shares
- `build--unreal` on a plug-in runs BuildPlugin in a temporary package folder for each engine its hosts' `EngineAssociation` names (its own `EngineVersion` when nothing hosts it), with each workspace plug-in its `Plugins` names as `-Dependencies`, and zips the package as `<outputPath>/unreal/<plug-in>/<plug-in>-<VersionName>-<engine major.minor>-<host>.zip` with the descriptor at the archive's root; cached on the plug-in, its dependencies, and each host `.uproject`
- `install` never writes into an engine; a plug-in no project hosts has no `install`
- One `unreal-build` batch executor (`preferBatch`) runs every Unreal task of a run one at a time in dependency order, since UnrealBuildTool runs once per engine and BuildPlugin's own UnrealBuildTool runs never wait for its lock
- The engine follows Epic's resolution (`DesktopPlatformBase.cpp`): the `engines` option, a path association relative to the `.uproject`, an empty association's enclosing engine tree, `Install.ini` `[Installations]`, then `LauncherInstalled.dat` row `UE_<association>`, under `~/Library/Application Support/Epic` on macOS or `~/.config/Epic` on Linux
- Arguments after `--` reach UnrealBuildTool or AutomationTool; BuildPlugin forwards none to its own UnrealBuildTool runs, which read UnrealBuildTool's `BuildConfiguration.xml` or `UnrealBuildTool_BuildConfiguration__<Field>` variables

- `build--blender` is cached on the project's files and its dependencies' named inputs, outputs `<outputPath>/blender/<id>/<id>-<version>*.zip`, and deletes the folder's earlier `<id>-*.zip` archives first, since Blender has no clean switch
- A folder holding `pyproject.toml` builds from a staged copy in a temporary folder: the running Blender's interpreter report (`blender.py`) gives its platform, wheel tags, and bundled modules; registry dependencies are `uv export` pins fetched by `pip download` for those tags, minus wheels whose modules Blender bundles; the staged manifest names that platform and those wheels, and keeps the committed `version`
- Local dependencies (the lock's `directory` packages) become subpackages of the extension, since Blender installs wheels into one `site-packages` every extension shares: `uv build --wheel` builds each, `vendored.py` unpacks it into the stage and rewrites every absolute import of it across the stage to a relative one through python-vendorize, so source imports `<package>` while the archive loads `bl_ext.<repository>.<id>.<package>`, one copy per extension
- A folder without `pyproject.toml` builds as it stands, with `--split-platforms` when its manifest declares `platforms`
- `build` is the `nx:noop` aggregate every plugin declares alike (`dependsOn: ["build--*"]`, inputs `'...'` then its members' inputs)
- `install` and `nx-release-publish` stay uncached, since each writes outside the workspace; `install` depends on `build`, while `nx-release-publish` builds nothing and depends on `^nx-release-publish` alone
- Packages come from the outputs of `build` and of every same-project target its `dependsOn` reaches, read through Nx's own `getDependencyConfigs`: for `install`, resolved for the configuration the run gives each target; for `nx-release-publish`, in the `release` configuration it defaults to, since `nx release publish` passes no configuration and Nx gives a dependency the command-line configuration or its own default, never the dependent's, so `build` in `release` runs before publishing (`nx run-many -t build -c release`) and a missing package fails by name
- Kind, name, version, and platforms come from the manifest inside each archive; Yak's platform comes from the tag ending its file name
- `install` installs the packages for this platform (a Blender `platforms` entry, a Yak `mac` or `win` tag); a package naming no platform installs anywhere
- Blender installs into the repository `extension list` shows holding the id, else `user_default`, since an id held by more repositories loads once per repository
- Blender's `install-file` and UPIA exit 0 on failure, so each target reads its host's success line and fails with the host's output
- UPIA removes the installed copy first, since a same-version install reports success and changes nothing; removal deletes the plug-in's saved data
- `nx-release-publish` skips an unversioned project and fails a package at another version than `nx release` set
- Every package attaches to the GitHub release of its project, the tag filled from `nx.json` `release.releaseTag.pattern` with the documented `{projectName}` and the package's `{version}`; one `gh api graphql` read per package finds the asset by name and the release's `immutable` flag, a held asset is skipped, an immutable or missing release fails, and `gh` reads its token from `GH_TOKEN`, `GITHUB_TOKEN`, or its own login, never argv
- Yak packages also go to the Yak server, skipping the distributions `GET <server>/versions/<name>/<version>` lists; Blender extensions stay off extensions.blender.org, which takes add-ons under `GPL-3.0-or-later` alone with every bundled wheel unmodified from PyPI (`docs/research/nx/distribution/known-problems.md` [19])
- `dryRun` or `NX_DRY_RUN` logs each upload in place of sending it
- `registry` replaces the Yak server for both the lookup and `yak push --source`; `yak push` reads `YAK_TOKEN`
- `blender` and `uv` resolve from `PATH`; Yak runs from the Rhino install MSBuild resolves, read back as the project's evaluated `YakExecutable` (`dotnet msbuild -getProperty:YakExecutable` in the project folder); UPIA runs from the path Creative Cloud installs it to on macOS

## [04]-[RELEASE]

Blender extension and Unreal plug-in projects take the `release` tag and `release.version.versionActions: "@rasm/nx-distribution/release/version-actions"` with their manifest as `versionActionsOptions.manifest`:
- Reads and writes `blender_manifest.toml` `version`, the one field Blender, `extension build`, and inference read, with layout and comments kept
- Reads `.uplugin` `VersionName` and writes the new version there, raising the integer `Version` by one, through `jsonc-parser` keeping the layout; the editor's wizard writes `1.0`, so a released plug-in states `major.minor.patch`
- Neither has a registry version, so that resolver fails by name; dependencies bundle as code, so a released dependency writes nothing into a manifest

## [05]-[LAYOUT]

| [INDEX] | [PATH]                       | [HOLDS]                                                                             |
| :-----: | :--------------------------- | :---------------------------------------------------------------------------------- |
|  [01]   | `plugins/project-graph.ts`   | Package entry: `createNodes` over host manifests                                    |
|  [02]   | `plugins/host-packages.ts`   | Host table: manifest decoders, platforms, installers, publishers, package discovery |
|  [03]   | `version-actions.ts`         | `VersionActions` over each host row that writes its manifest's version              |
|  [04]   | `executors/extension-build/` | `build--blender` executor, Nx schema, interpreter report, vendoring script and lock |
|  [05]   | `executors/install/`         | `install` executor and its Nx schema                                                |
|  [06]   | `executors/release-publish/` | `nx-release-publish` executor and its Nx schema                                     |
|  [07]   | `executors/unreal-build/`    | `build--unreal` batch executor: engine resolution, UnrealBuildTool, BuildPlugin, archives |

# [NX_DISTRIBUTION]

`@rasm/nx-distribution` is an Nx 24 plugin that puts host packages into their hosts. `install` feeds the local build through the installer the host itself uses, and `nx-release-publish` sends a release to the host's package server. Language plugins build the packages; this plugin reads them from the outputs those builds declare and builds nothing.

## [01]-[USE]

```json
{ "plugins": ["@rasm/nx-apple", "@rasm/nx-dotnet", "@rasm/nx-uv", "@rasm/nx-typescript", "@rasm/nx-distribution"] }
```

- Options: none, since each host manifest and each built package states every fact the plugin reads
- List it after the language plugins, so its `nx-release-publish` replaces theirs on a host project, which Nx merges whole when the executors differ

## [02]-[INFERENCE]

| [INDEX] | [MANIFEST]              | [HOST]                    | [PACKAGE] | [TARGETS]                       |
| :-----: | :---------------------- | :------------------------ | :-------- | :------------------------------ |
|  [01]   | `blender_manifest.toml` | Blender extension         | `.zip`    | `install`                       |
|  [02]   | `manifest.yml`          | Rhino and Grasshopper Yak | `.yak`    | `install`, `nx-release-publish` |
|  [03]   | `manifest.json`         | Adobe UXP plug-in         | `.ccx`    | `install`                       |
|  [04]   | `CSXS/manifest.xml`     | Adobe CEP extension       | `.zxp`    | `install`                       |

- Manifests name a project at their folder (`CSXS/manifest.xml` at the folder above `CSXS`), merging with the language plugin's project there
- Files named `manifest.yml` or `manifest.json` lacking the host's required keys (Yak `name`, `version`, `authors`, `description`; UXP `manifestVersion`, `id`, `name`, `version`) name nothing

## [03]-[TARGETS]

| [INDEX] | [TARGET]           | [RUNS]                                              | [SUCCESS]                                                 |
| :-----: | :----------------- | :-------------------------------------------------- | :-------------------------------------------------------- |
|  [01]   | Blender `install`  | `blender --command extension install-file --enable` | `STATUS Installed` or `Reinstalled "<id>"`                |
|  [02]   | Yak `install`      | `yak install <file>.yak`                            | `Successfully installed <name>`                           |
|  [03]   | UXP, CEP `install` | UPIA `--remove <name>`, `--install`                 | `-406` or `Removal Successful`, `Installation Successful` |
|  [04]   | Yak publish        | `yak push <file>`                                   | `Successfully published`                                  |

- Both targets depend on `build` and stay uncached, since each writes outside the workspace
- Packages come from the outputs of `build` and of every target of the same project its `dependsOn` reaches (`build--*` included), resolved for the configuration the run gives `build`
- Kind, name, version, and platforms come from the manifest inside each archive; Yak's platform comes from the tag ending its file name
- `install` installs the packages for this platform (a Blender `platforms` entry, a Yak `mac` or `win` tag); a package naming no platform installs anywhere
- Blender installs into the repository `extension list` shows holding the id, else `user_default`, since an id held by more repositories loads once per repository
- Blender's `install-file` and UPIA exit 0 on failure, so each target reads its host's success line and fails with the host's output
- UPIA removes the installed copy first, since a same-version install reports success and changes nothing; removal deletes the plug-in's saved data
- `nx-release-publish` skips an unversioned project and the distributions `GET <server>/versions/<name>/<version>` lists, and fails a package at another version than `nx release` set
- `dryRun` or `NX_DRY_RUN` logs each upload in place of sending it
- `registry` replaces the Yak server for both the lookup and `yak push --source`; `yak push` reads `YAK_TOKEN`
- `blender` and `yak` resolve from `PATH`; UPIA runs from the path Creative Cloud installs it to on macOS

## [04]-[LAYOUT]

| [INDEX] | [PATH]                       | [HOLDS]                                                                             |
| :-----: | :--------------------------- | :---------------------------------------------------------------------------------- |
|  [01]   | `plugins/project-graph.ts`   | Package entry: `createNodes` over host manifests                                    |
|  [02]   | `plugins/host-packages.ts`   | Host table: manifest decoders, platforms, installers, publishers, package discovery |
|  [03]   | `executors/install/`         | `install` executor and its Nx schema                                                |
|  [04]   | `executors/release-publish/` | `nx-release-publish` executor and its Nx schema                                     |

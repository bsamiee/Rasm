# [NX_DISTRIBUTION]

`@rasm/nx-distribution` is an Nx 24 plugin that puts host packages into their hosts. `install` feeds the local build through the installer the host itself uses, and `nx-release-publish` attaches a release to its GitHub release and sends it to the host's package server where one takes uploads. Language plugins build the packages; this plugin reads them from the outputs those builds declare and builds nothing.

## [01]-[USE]

```json
{ "plugins": ["@rasm/nx-apple", "@rasm/nx-dotnet", "@rasm/nx-uv", "@rasm/nx-typescript", "@rasm/nx-distribution"] }
```

- Options: none, since each host manifest and each built package states every fact the plugin reads
- List it after the language plugins, so its `nx-release-publish` replaces theirs on a host project, which Nx merges whole when the executors differ

## [02]-[INFERENCE]

| [INDEX] | [MANIFEST]              | [HOST]                    | [PACKAGE] | [TARGETS]                       |
| :-----: | :---------------------- | :------------------------ | :-------- | :------------------------------ |
|  [01]   | `blender_manifest.toml` | Blender extension         | `.zip`    | `install`, `nx-release-publish` |
|  [02]   | `manifest.yml`          | Rhino and Grasshopper Yak | `.yak`    | `install`, `nx-release-publish` |
|  [03]   | `manifest.json`         | Adobe UXP plug-in         | `.ccx`    | `install`, `nx-release-publish` |
|  [04]   | `CSXS/manifest.xml`     | Adobe CEP extension       | `.zxp`    | `install`, `nx-release-publish` |

- Manifests name a project at their folder (`CSXS/manifest.xml` at the folder above `CSXS`), merging with the language plugin's project there
- Files named `manifest.yml` or `manifest.json` lacking the host's required keys (Yak `name`, `version`, `authors`, `description`; UXP `manifestVersion`, `id`, `name`, `version`) name nothing

## [03]-[TARGETS]

| [INDEX] | [TARGET]           | [RUNS]                                              | [SUCCESS]                                                 |
| :-----: | :----------------- | :-------------------------------------------------- | :-------------------------------------------------------- |
|  [01]   | Blender `install`  | `blender --command extension install-file --enable` | `STATUS Installed` or `Reinstalled "<id>"`                |
|  [02]   | Yak `install`      | `yak install <file>.yak`                            | `Successfully installed <name>`                           |
|  [03]   | UXP, CEP `install` | UPIA `--remove <name>`, `--install`                 | `-406` or `Removal Successful`, `Installation Successful` |
|  [04]   | Every publish      | `gh release upload <tag> <file>`                    | Exit status; held assets skipped                          |
|  [05]   | Yak publish        | `yak push <file>`                                   | `Successfully published`                                  |

- Both targets stay uncached, since each writes outside the workspace; `install` depends on `build`, while `nx-release-publish` builds nothing and depends on `^nx-release-publish` alone
- Packages come from the outputs of `build` and of every target of the same project its `dependsOn` reaches (`build--*` included): for `install`, resolved for the configuration the run gives each target; for `nx-release-publish`, in the `release` configuration it defaults to, since `nx release publish` passes no configuration and Nx gives a dependency the command-line configuration or its own default, never the dependent's, so `build` in `release` runs before publishing (`nx run-many -t build -c release`) and a missing package fails by name
- Kind, name, version, and platforms come from the manifest inside each archive; Yak's platform comes from the tag ending its file name
- `install` installs the packages for this platform (a Blender `platforms` entry, a Yak `mac` or `win` tag); a package naming no platform installs anywhere
- Blender installs into the repository `extension list` shows holding the id, else `user_default`, since an id held by more repositories loads once per repository
- Blender's `install-file` and UPIA exit 0 on failure, so each target reads its host's success line and fails with the host's output
- UPIA removes the installed copy first, since a same-version install reports success and changes nothing; removal deletes the plug-in's saved data
- `nx-release-publish` skips an unversioned project and fails a package at another version than `nx release` set
- Every package attaches to the GitHub release of its project, the tag filled from `nx.json` `release.releaseTag.pattern` with the documented `{projectName}` and the package's `{version}`; assets the release holds are skipped (one `gh api graphql` read), a missing release fails, and `gh` reads its token from `GH_TOKEN`, `GITHUB_TOKEN`, or its own login, never argv
- Yak packages also go to the Yak server, skipping the distributions `GET <server>/versions/<name>/<version>` lists; Blender extensions stay off extensions.blender.org, which takes add-ons under `GPL-3.0-or-later` alone with every bundled wheel unmodified from PyPI (`docs/research/nx/distribution/known-problems.md` [19])
- `dryRun` or `NX_DRY_RUN` logs each upload in place of sending it
- `registry` replaces the Yak server for both the lookup and `yak push --source`; `yak push` reads `YAK_TOKEN`
- `blender` resolves from `PATH`; Yak runs from the Rhino install MSBuild resolves, read back as the project's evaluated `_YakExecutable` (`dotnet msbuild -getProperty:_YakExecutable` in the project folder); UPIA runs from the path Creative Cloud installs it to on macOS

## [04]-[LAYOUT]

| [INDEX] | [PATH]                       | [HOLDS]                                                                             |
| :-----: | :--------------------------- | :---------------------------------------------------------------------------------- |
|  [01]   | `plugins/project-graph.ts`   | Package entry: `createNodes` over host manifests                                    |
|  [02]   | `plugins/host-packages.ts`   | Host table: manifest decoders, platforms, installers, publishers, package discovery |
|  [03]   | `executors/install/`         | `install` executor and its Nx schema                                                |
|  [04]   | `executors/release-publish/` | `nx-release-publish` executor and its Nx schema                                     |

# [PLUGINS]

Packages come from the Yak server through `yak`, and compiled builds load from their output folder into a spawned document.

## [01]-[PACKAGES]

`yak` runs from `/Applications/RhinoBETA.app/Contents/Resources/bin` outside Rhino and writes `~/Library/Application Support/McNeel/Rhinoceros/packages/9.0/<package>/<version>/`, the folder Rhino and Grasshopper 2 load packages from:

| [INDEX] | [NEED]                          | [COMMAND]                                                                                         |
| :-----: | :------------------------------ | :------------------------------------------------------------------------------------------------ |
|  [01]   | Package for a capability        | `yak search <words>` matches names and keywords, one `<package> (<newest stable version>)` a line |
|  [02]   | Package of a plugin id          | `yak search guid:<plugin id>`, an empty result means no package supplies it                       |
|  [03]   | Package of a Grasshopper 2 type | `yak search ioid:<component guid>` with the `guid` `g2_search_components` returns                 |
|  [04]   | Newest stable version           | `yak install <package>`                                                                           |
|  [05]   | Chosen version                  | `yak install <package> <version>` installs the version's first distribution Rhino 9 accepts       |
|  [06]   | Installed packages              | `yak list`                                                                                        |
|  [07]   | Remove                          | `yak uninstall <package>` deletes every version, `yak list` shows the result                      |

`yak search --prerelease` names each match's newest version on any platform, Yak's versions API names the newest one Rhino 9 runs, prereleases included:

```bash
# Newest version with a macOS or cross-platform distribution for any Rhino, an earlier major, or Rhino 9 at or below the running service release
curl -s https://yak.rhino3d.com/versions/<package> | jq -r --argjson release "$(defaults read /Applications/RhinoBETA.app/Contents/Info CFBundleShortVersionString | cut -d. -f2)" 'first(.[] | select(any(.distributions[]; .platform != "win" and (.rhino_version == "any" or (.rhino_version | ltrimstr("rh") | split("_") | map(tonumber)) <= [9, $release])))) | .version'
```

Packages installed while Rhino runs load in the running session:
1. `canvas.plugins()` loads each Grasshopper 2 library in Rhino's assembly search paths and lists its `id` and components
2. `document.assembly("<package folder>/<package>/<version>/<file>.rhp")` loads its Rhino plugin and returns the commands `command` runs

- `get_commands` lists no command of a newly installed package plugin until it loads
- `plugins()` lists a loaded library `yak uninstall` deleted at its old location until Rhino quits

## [02]-[LOAD]

1. `spawn_slot {"version": "9"}` gives a new document that becomes Rhino's active document and takes commands
2. `document.assembly("</abs/build>/<project>.rhp")` in `run_python` on the spawned slot returns an `AssemblyRecord`

| [INDEX] | [FIELD]        | [MEANING]                                                                          |
| :-----: | :------------- | :--------------------------------------------------------------------------------- |
|  [01]   | `commands`     | English names a macro calls                                                        |
|  [02]   | `path`         | File the held build came from                                                      |
|  [03]   | No `plugin` id | Rhino holds a library, or a plugin with no assembly id when `commands` lists names |

- Rhino keeps a loaded assembly until it quits and answers every same-named path with it, a new build loads after a relaunch
- Collectible `AssemblyLoadContext`s that loaded `LanguageExt.Core` never unload and break later `PlugIn.LoadPlugIn` calls, load plugins before any
- `PlugIn.LoadPlugIn(id, loadQuietly=False, forceLoad=True)` loads after a failed load, `GetPlugInInfo(id)` reads `IsLoaded` and `PlugInLoadTime`
- Development builds register `PlugInRegistry/6/<id>` with `LoadMode` 2, a record with no file stays until `PlugInRegistry/6` `DeleteChild(<id>)`
- `LoadMode` key writes take effect at the next start

## [03]-[COMMAND]

`command(doc, "<Command> <answers>", "<Project>")` runs a plugin command on a layer like any Rhino command:
- Prompts and options come from plugin source, the listener help resource serves Rhino's own commands alone
- `output` of a run lists each prompt with its options as the command asked it

## [04]-[LIBRARY]

Pure functions of a plugin or library take exact inputs in any document, active or not, with no prompt:

```python
import document
print(document.assembly("</abs/build>/<project>.rhp"))
from <Namespace> import <Type>
print(<Type>.<Function>(<arguments>))
```

- Python calls the net10.0 build directly, `LanguageExt` results print their case (`Succ(...)`, `Fail(...)`)

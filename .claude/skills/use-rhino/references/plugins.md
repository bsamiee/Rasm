# [PLUGINS]

Packages come from the Yak server through the `yak` CLI, compiled builds load from their output folder, and each proves itself in a spawned document.

## [01]-[PACKAGES]

`yak` runs from `/Applications/RhinoBETA.app/Contents/Resources/bin` outside Rhino and writes `~/Library/Application Support/McNeel/Rhinoceros/packages/9.0/<package>/<version>/`, the folder Rhino and Grasshopper 2 load packages from:

| [INDEX] | [NEED]                          | [COMMAND]                                                                                   |
| :-----: | :------------------------------ | :------------------------------------------------------------------------------------------ |
|  [01]   | Package for a capability        | `yak search <words>` matches package names and keywords, one `<package> (<version>)` a line |
|  [02]   | Package of a plugin id          | `yak search guid:<plugin id>`, an empty result means no package supplies it                 |
|  [03]   | Package of a Grasshopper 2 type | `yak search ioid:<component guid>` with the `guid` `g2_search_components` returns           |
|  [04]   | Install                         | `yak install <package>` takes the newest distribution Rhino 9 runs                          |
|  [05]   | Installed packages              | `yak list`                                                                                  |
|  [06]   | Remove                          | `yak uninstall <package>` deletes every version, `yak list` shows the result                |

Packages installed while Rhino runs load in that session:
1. `canvas.plugins()` loads each Grasshopper 2 library in Rhino's assembly search paths and lists its `id` and components
2. `assembly.load("<package folder>/<package>/<version>/<file>.rhp")` loads its Rhino plugin and returns the commands `command` runs

- `get_commands` lists no command of a newly installed package plugin until it loads
- `plugins()` lists a loaded library `yak uninstall` deleted at its old location until Rhino quits

## [02]-[LOAD]

1. `spawn_slot {"version": "9"}` for the proof, the new document is Rhino's active document and takes commands
2. `assembly.load("</abs/build>/<project>.rhp")` in `run_python` on that slot

| [INDEX] | [RESULT]         | [NEXT]                                                                                        |
| :-----: | :--------------- | :-------------------------------------------------------------------------------------------- |
|  [01]   | `AssemblyRecord` | `commands` lists the English names a macro calls, `path` the file the held build came from    |
|  [02]   | No `plugin` id   | Rhino refused the file as a plugin and holds it as a library                                  |
|  [03]   | `Assembly` fault | Rhino holds another build, `value` is its module id and `accepted` the file's, relaunch Rhino |

- Rhino keeps a loaded assembly until it quits and answers every path of that name with it, a new build loads after a relaunch
- Collectible `AssemblyLoadContext`s that loaded `LanguageExt.Core` never unload and break later `PlugIn.LoadPlugIn` calls, load plugins before any
- `PlugIn.LoadPlugIn(id, loadQuietly=False, forceLoad=True)` loads after a failed load, `GetPlugInInfo(id)` reads `IsLoaded` and `PlugInLoadTime`
- Development builds register `PlugInRegistry/6/<id>` with `LoadMode` 2, a record with no file stays until `PlugInRegistry/6` `DeleteChild(<id>)`
- `LoadMode` key writes take effect at the next start

## [03]-[COMMAND]

`command(doc, "<Command> <answers>", "Proof::<Project>")` runs a plugin command on a proof layer like any Rhino command:
- Prompts and options come from the plugin's source, the listener help resource serves Rhino's own commands alone
- `output` of a run lists each prompt with its options as the command asked it

## [04]-[LIBRARY]

Pure functions of a plugin or library take exact inputs in any document, active or not, with no prompt:

```python
import assembly
print(assembly.load("</abs/build>/<project>.rhp"))
from <Namespace> import <Type>
print(<Type>.<Function>(<arguments>))
```

- Python calls the net10.0 build directly, `LanguageExt` results print their case (`Succ(...)`, `Fail(...)`)

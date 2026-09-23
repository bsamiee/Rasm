# [PLUGINS]

Packages come from the Yak server through the `yak` CLI, repository builds load from `.artifacts/dotnet/bin/`, and each proves itself in a spawned document.

## [01]-[PACKAGES]

`yak` runs outside Rhino and writes `~/Library/Application Support/McNeel/Rhinoceros/packages/9.0/<package>/<version>/`, the folder Rhino and Grasshopper 2 load packages from:

| [INDEX] | [NEED]                          | [COMMAND]                                                                                   |
| :-----: | :------------------------------ | :------------------------------------------------------------------------------------------ |
|  [01]   | Package for a capability        | `yak search <words>` matches package names and keywords, one `<package> (<version>)` a line |
|  [02]   | Package of a plugin id          | `yak search guid:<plugin id>`, an empty result means no package supplies it                 |
|  [03]   | Package of a Grasshopper 2 type | `yak search ioid:<component guid>` with the `guid` `g2_search_components` returns           |
|  [04]   | Install                         | `yak install <package>` takes the newest distribution for Rhino 9 or earlier                |
|  [05]   | Installed packages              | `yak list`                                                                                  |
|  [06]   | Remove                          | `yak uninstall <package>` deletes every version, `yak list` shows the result                |

Packages installed while Rhino runs load in that session:
1. `canvas.plugins()` loads its Grasshopper 2 libraries with the assemblies beside them and lists each plugin's `id` and components
2. `assembly.load("<package folder>/<package>/<version>/<file>.rhp")` loads its Rhino plugin and returns the commands `command` runs

- `get_commands` lists no command of a package plugin until it loads
- Rhino holds every loaded assembly after `yak uninstall` until it quits, `plugins()` lists such a library at its deleted location
- `yak uninstall` removes a package a task installed for a proof when the proof ends

## [02]-[BUILD]

Plugin projects set `RhinoHost` to `common`, `EnableDynamicLoading` to `true`, and `TargetExt` to `.rhp`:
- `nx run <project>:build` writes `.artifacts/dotnet/bin/<project>/debug/<project>.rhp` beside its dependencies
- Use `manage-repo` for the project file and `dotnet-coding` for the code

## [03]-[LOAD]

1. `spawn_slot {"version": "9"}` for the proof, the new document is Rhino's active document and takes commands
2. `assembly.load("<root>/.artifacts/dotnet/bin/<project>/debug/<project>.rhp")` in `run_python` on that slot

| [INDEX] | [RESULT]         | [NEXT]                                                                                           |
| :-----: | :--------------- | :----------------------------------------------------------------------------------------------- |
|  [01]   | `AssemblyRecord` | `commands` lists the English names a macro calls, `path` the file the held build came from       |
|  [02]   | No `plugin` id   | Rhino refused the file as a plugin and holds it as a library                                     |
|  [03]   | `Assembly` fault | Rhino holds an earlier build, `value` is its module id and `accepted` the file's, relaunch Rhino |

- Rhino keeps a loaded assembly until it quits and registers a loaded plugin's path for later sessions, load a build right before proving it
- Assemblies Rhino already holds under a name answer for every path of that name, `AssemblyRecord.path` names where the held one came from

## [04]-[RELAUNCH]

New builds load into a new Rhino process, and quitting Rhino takes every slot with it:
1. `save(doc)` in every slot whose document is titled and modified, `describe(doc).modified` reads `False` on the next call
2. Untitled documents with edits hold the quit, Rhino prompts for them and the prompt blocks the process
3. `Rhino.RhinoApp.Exit()` in `run_python` quits Rhino, the call returns `rhino_closed`
4. `spawn_slot {"version": "9"}` launches Rhino, `open -g -b com.mcneel.rhinoceros.9 <file>...` reopens every saved file, each as its own slot
5. `assembly.load` of the build returns its `AssemblyRecord`

## [05]-[COMMAND]

`command(doc, "<Command> <answers>", "Proof::<Project>")` runs a plugin command on a proof layer like any Rhino command:
- Prompts and options come from the plugin's source, the listener help resource serves Rhino's own commands alone
- `output` of a run lists each prompt with its options as the command asked it

## [06]-[LIBRARY]

Pure functions of a plugin or library take exact inputs in any document, active or not, with no prompt:

```python
import assembly
print(assembly.load("<root>/.artifacts/dotnet/bin/<project>/debug/<project>.rhp"))
from <Namespace> import <Type>
print(<Type>.<Function>(<arguments>))
```

- Python calls the net10.0 build directly, `LanguageExt` results print their case (`Succ(...)`, `Fail(...)`)

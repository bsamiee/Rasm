# [PLUGINS]

Packages install through `yak` on PATH, and packages, libraries, and repository builds load into the running Rhino through `document.assembly`.

## [01]-[FIND]

Searches run without Rhino, and each prints one `<package> (<version>)` line per match:
1. `yak search guid:<plugin id>` with a `PluginRequirement` fault's `value`, else `yak search ioid:<component guid>` with a `g2_search_components` `guid`
2. `yak search <words>` matches package names and keywords when an id search prints nothing, `yak spec` writes id keywords alone
3. `yak search --all <package>` lists every version, `--prerelease` adds prerelease versions of every platform
4. `yak list` names the package directory and each installed `<package> (<version>)`

## [02]-[PACKAGES]

`yak` writes `<package directory>/<package>/<version>/`, which Rhino registers at its next start:
1. `yak install <package>` installs the newest stable version, `yak install <package> <version>` a chosen or prerelease one
2. `document.assembly("<package directory>/<package>/<version>/<file>.rhp")` in `run_python` loads a Rhino plugin the running Rhino has not registered
3. `canvas.plugins()` loads each Grasshopper 2 library installed since the editor started and lists its components
4. `yak uninstall <package>` deletes every version, and Rhino keeps a loaded plugin until it quits

Packages installed while Rhino runs stay out of `get_commands` and `PlugIn.IdFromName` until `assembly` loads them.

## [03]-[BUILDS]

Repository builds load into a document the task spawned:
1. `nx run <project>:build` writes `.artifacts/dotnet/bin/<project>/debug/<project>.dll`, a plugin project `<project>.rhp`
2. `spawn_slot {"version": "9"}` gives the task's document
3. `document.assembly("<repository>/.artifacts/dotnet/bin/<project>/debug/<project>.<suffix>")` in `run_python` on that slot returns an `AssemblyRecord`

| [INDEX] | [FIELD]    | [DECIDES]                                                                                     |
| :-----: | :--------- | :-------------------------------------------------------------------------------------------- |
|  [01]   | `result`   | `Success` loaded now, `SuccessAlreadyLoaded` held before, `NotRhinoPlugIn` a library          |
|  [02]   | `path`     | File the held assembly came from, a package folder when a package loaded it first             |
|  [03]   | `plugin`   | Plugin id `GetPlugInInfo` and `LoadPlugIn` take                                               |
|  [04]   | `commands` | English names `command` runs, hidden commands included                                        |
|  [05]   | `stale`    | Assemblies of the build's folder Rhino holds from another build, each bound in place of its file |

Rhino holds one assembly per name until it quits, `stale` names each one a call runs from another build:
- Libraries a package plugin loaded at startup (`Rasm.Rhino.*` of an installed app) bind from the package folder, `nx run <app>:install` packs the current build into it
- Builds `stale` names reach Rhino through a relaunch from the setup reference, after `nx run <app>:install` for a package's copy

## [04]-[CALLS]

`command(doc, "<Command> <answers>", "<A::B>")` runs a plugin command like a built-in one, prompts and options come from plugin source, and `output` lists each prompt as the command asked it.

Pure functions of a plugin or library take exact inputs in any document with no prompt:

```python
# Call a library function
import document
from Rhino.Geometry import Point3d, Transform

print(document.assembly("<repository>/.artifacts/dotnet/bin/Rasm.Rhino.Blocks/debug/Rasm.Rhino.Blocks.dll"))
from Rasm.Rhino.Blocks import InstanceMotion

print(InstanceMotion.Of(Transform.Scale(Point3d.Origin, 2.0), __rhino_doc__.ModelAbsoluteTolerance))
```

- `IO` results run through `.Run()`, `.RunSafe()` returns `Fin` printing `Succ(...)` or `Fail(...)`

## [05]-[STATE]

Scripts read a registered plugin's state by id from `PlugIn.IdFromName("<name>")` or `assembly`:
- `PlugIn.GetPlugInInfo(id)` holds `IsLoaded`, `PlugInLoadTime`, `FileName`, and `Version`, `None` for an unregistered id
- `PlugIn.LoadPlugIn(id, False, True)` loads a registered plugin after a failed load
- `get_commands {"filter": "<part>"}` lists registered commands by English name, loaded or not, hidden commands left out
- Hidden commands run from macros, `assembly` lists them, and `Command.IsCommand("<name>")` reads `False` for them
- Loads from a build folder register `PlugInRegistry/6/<id>` with `LoadMode` 2, a record whose file is gone stays until `DeleteChild(<id>)`

Use the settings reference for a plugin's settings tree and command settings.

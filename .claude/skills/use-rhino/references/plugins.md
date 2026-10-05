# [PLUGINS]

Packages install through `yak` on PATH, and packages, libraries, and repository builds load into the running Rhino through `document.assembly`.

## [01]-[FIND]

Searches run without Rhino, and each prints one `<package> (<version>)` line per match:
1. `yak search guid:<plugin id>` takes a `PluginRequirement` fault's `value`, `yak search ioid:<guid>` a `g2_search_components` `guid`
2. `yak search <words>` matches package names and keywords when an id search prints nothing
3. `yak search --all <package>` lists every version, `--prerelease` adds prerelease versions of every platform
4. `yak list` names the package directory and each installed `<package> (<version>)`

## [02]-[PACKAGES]

`yak` writes `<package directory>/<package>/<version>/`, which Rhino registers at its next start:
1. `yak install <package>` installs the newest stable version, `yak install <package> <version>` a chosen or prerelease one
2. `document.assembly("<package directory>/<package>/<version>/<file>.rhp")` in `run_python` loads its Rhino plugin into the running Rhino
3. `canvas.plugins()` loads its Grasshopper 2 library
4. `yak uninstall <package>` deletes every version, and Rhino keeps a loaded plugin until it quits
5. `PersistentSettings.RhinoAppSettings.GetChild("PlugInRegistry").GetChild("6").DeleteChild("<plugin id>")` after a restart drops its record

Packages installed while Rhino runs stay out of `get_commands` and `PlugIn.IdFromName` until `assembly` loads them.

## [03]-[BUILDS]

Repository builds load into the Rhino process every slot shares, and their calls run on a document the task spawned:
1. `nx run <project>:build` writes `.artifacts/dotnet/bin/<project>/debug/<project>.dll`, a plugin project `<project>.rhp`
2. `spawn_slot` gives the task's document
3. `document.assembly("<repository>/.artifacts/dotnet/bin/<project>/debug/<project>.<suffix>")` on that slot returns an `AssemblyRecord`

| [INDEX] | [FIELD]    | [DECIDES]                                                                    |
| :-----: | :--------- | :--------------------------------------------------------------------------- |
|  [01]   | `current`  | `True` when Rhino runs the file's build, `False` when it holds another build |
|  [02]   | `path`     | File the held build came from, a package folder when a package loaded first  |
|  [03]   | `plugin`   | Plugin id `GetPlugInInfo` and `LoadPlugIn` take, `None` for a library        |
|  [04]   | `commands` | English names `command` runs, hidden commands included                       |

Rhino holds one assembly per name until it quits, and a build's references bind to the copies it holds:
- `assembly` on a dependency in the build folder (`Rasm.Rhino.Document.dll`) reads which build a call binds to
- Builds reading `current` `False` reach Rhino through the setup reference's relaunch, after `nx run <app>:install` for an installed app's package copy

## [04]-[CALLS]

`command(doc, "<Command> <answers>", "<A::B>")` runs a plugin command like a built-in one, its prompts and options stated in plugin source.

Pure functions of a plugin or library take exact inputs in any document with no prompt, here a scale by `factor`:

```python
# Call library functions
import document
from LanguageExt import IOExtensions
from Rhino.Geometry import Point3d, Transform

print(document.assembly("<repository>/.artifacts/dotnet/bin/Rasm.Rhino.Blocks/debug/Rasm.Rhino.Blocks.dll"))
from Rasm.Rhino.Blocks import BlockState, Definitions, InstanceMotion
from Rasm.Rhino.Document import ComponentRef

print(InstanceMotion.Of(Transform.Scale(Point3d.Origin, factor), __rhino_doc__.ModelAbsoluteTolerance))
print(IOExtensions.RunSafe[BlockState](Definitions.Snapshot(__rhino_doc__, ComponentRef.ByName("<block>"))))
```

- Records print their members, `Fin` results print `Succ(...)` or `Fail(<error record>)`
- `IO` results run into a `Fin` through `IOExtensions.RunSafe[T](io)`, and `io.Run()` raises a failure as `WrappedErrorExpectedException`

## [05]-[STATE]

Scripts read a registered plugin's state by id from `PlugIn.IdFromName("<name>")` or `assembly`:
- `PlugIn.GetPlugInInfo(id)` holds `IsLoaded`, `PlugInLoadTime`, `FileName`, and `Version`, `None` for an unregistered id
- Listener resource `rhino://host/plugins` lists registered plugins outside McNeel's organization with `IsLoaded` and a partial `CommandNames`
- `PlugIn.LoadPlugIn(id, False, True)` loads a registered plugin after a failed load
- Loads from a build folder register `PlugInRegistry/6/<id>` with `LoadMode` 2

Use `settings.md` for a plugin's settings tree and command settings.

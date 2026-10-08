# [PLUGINS]

Use `yak` from PATH.

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

Packages installed while Rhino runs stay out of `get_commands` and `PlugIn.IdFromName` until `assembly` loads them.

## [03]-[BUILDS]

Repository builds load into Rhino's shared process for calls on a task document:
1. `nx run <project>:build` writes `.artifacts/dotnet/bin/<project>/debug/<project>.dll`, a plugin project `.artifacts/dotnet/bin/<project>/debug_<rid>/<project>.rhp`
2. `spawn_slot` gives the task's document
3. `document.assembly("<repository>/<file>")` on that slot, `<file>` a file step 1 wrote, returns an `AssemblyRecord`

| [INDEX] | [FIELD]    | [DECIDES]                                                                    |
| :-----: | :--------- | :--------------------------------------------------------------------------- |
|  [01]   | `current`  | `True` when Rhino runs the file's build, `False` when it holds another build |
|  [02]   | `path`     | File the held build came from, a package folder when a package loaded first  |
|  [03]   | `plugin`   | Plugin id `GetPlugInInfo` and `LoadPlugIn` take, `None` for a library        |
|  [04]   | `commands` | English names `command` runs, hidden commands included                       |

Rhino holds one assembly per name until it quits:
- Workspace dependencies beside a plugin are named `<PlugIn>.<Project>.dll`, plain library builds `<Project>.dll`
- `assembly` on a dependency in the build folder (`Rasm.Rhino.dll` beside a library, `<PlugIn>.Rasm.Rhino.dll` beside a plugin) reads which build a call binds to

## [04]-[CALLS]

`command(doc, "<Command> <answers>", "<A::B>")` runs plugin commands. Read plugin source for prompts and options.

Functions take explicit inputs, here scale factor `factor`:

```python
# Call library functions
import document
from LanguageExt import IOExtensions
from Rhino.DocObjects import InstanceDefinition
from Rhino.Geometry import Point3d, Transform

print(document.assembly("<repository>/.artifacts/dotnet/bin/Rasm.Rhino/debug/Rasm.Rhino.dll"))
from Rasm.Rhino.Blocks import DefinitionState, Definitions
from Rasm.Rhino.Document.Shapes import Decomposition
from Rasm.Rhino.Document.Tables import ComponentRef

print(Decomposition.Of(Transform.Scale(Point3d.Origin, factor)))
print(IOExtensions.RunSafe[DefinitionState](Definitions.State(__rhino_doc__, ComponentRef[InstanceDefinition].ByName("<block>"))))
```

- Records print their members, `Option` results print `Some(...)` or `None`, `Fin` results print `Succ(...)` or `Fail(<error record>)`
- `IO` results run into a `Fin` through `IOExtensions.RunSafe[T](io)`, and `io.Run()` raises a failure as `WrappedErrorExpectedException`

Library calls taking a delegate, and builds `current` reads `False` for, run as a C# script with no Python on Rhino's stack:
1. `<file>.cs` writes `start` to `<output>`, appends each reading, and writes `done` or the exception last
2. `LoadFromStream` of each build file's bytes on a collectible `AssemblyLoadContext` loads the rebuilt assembly beside Rhino's copy
3. Context's `Resolving` loads each dependency the build folder lacks from `<repository>/.cache/nuget/packages/`, `<project>.deps.json` naming its file
4. Types come from the loaded assembly through `GetType("<namespace>.<Type>")` and reflection
5. `rhinocode --rhino <pipeId> script <file>.cs`, `pipeId` from `rhinocode list --json`, returns before the script ends
6. `<output>` holds the run once its last line reads `done`

## [05]-[STATE]

Scripts read a registered plugin's state by id from `PlugIn.IdFromName("<name>")` or `assembly`:
- `PlugIn.GetPlugInInfo(id)` holds `IsLoaded`, `PlugInLoadTime`, `FileName`, and `Version`, `None` for an unregistered id
- Listener resource `rhino://host/plugins` lists registered plugins outside McNeel's organization with `IsLoaded` and a partial `CommandNames`
- `PlugIn.LoadPlugIn(id, False, True)` loads a registered plugin after a failed load
- Loads from a build folder register `PlugInRegistry/6/<id>` with `LoadMode` 2

Use `settings.md` for a plugin's settings tree and command settings.

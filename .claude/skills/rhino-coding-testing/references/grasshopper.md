# [GRASSHOPPER]

Grasshopper 1 (`Grasshopper.dll`, `GH_IO.dll`) and Grasshopper 2 (`Grasshopper2.dll`, `GrasshopperIO.dll`) ship inside Rhino 9 as `GrasshopperPlugin.rhp` and `Grasshopper2Plugin.rhp`. NuGet packages `Grasshopper` and `Grasshopper2` hold reference assemblies, `GrasshopperIO` ships inside `Grasshopper2`.

## [01]-[GRASSHOPPER_2]

### [01.1]-[DOCUMENTS]

| [INDEX] | [MEMBER]                                                              | [RESULT]                                                |
| :-----: | :-------------------------------------------------------------------- | :------------------------------------------------------ |
|  [01]   | `Document.NewInertDocument()`                                         | Document with no display, the form for tests            |
|  [02]   | `Document.NewInactiveDocument()`, `NewActiveDocument()`               | Documents an editor shows                               |
|  [03]   | `new DocumentIO(null, false, false, false, false).Open(<path>)`       | `.ghz` read into `DocumentIO.Document`                  |
|  [04]   | `new DocumentIO(<document>, false, false, false, false).Save(<path>)` | `.ghz` written                                          |
|  [05]   | `Differences(Document a, Document b)`                                 | `Count`, `All`, `Some(DiffType)` per object, wire, data |

- `DocumentIO` arguments are document, `trackFiles`, `reportErrors`, `retainRoot`, `resolvePlugins`, and `resolvePlugins: true` opens a modal dialog for a missing plug-in
- `.gh` and `.ghx` open through Grasshopper 1 migration, which asks through a dialog, and Grasshopper 2 fixtures stay `.ghz`
- `Differences` of a document and its reopened save holds no `Object`, `Data`, or `Wire` entry when components store their state

### [01.2]-[COMPONENTS]

- Construct the plug-in's own component type (`new SlabVolumeComponent()`) and `document.Objects.Add(component)`
- Components of other plug-ins come from `ObjectProxies.FindById(<ioId>).Emit()` after `PluginServer.LoadAllScopedPlugins()`
- `document.Objects.FindComponent(<instanceId>)`, `FindParameter`, and `ActiveObjects` locate objects of an opened fixture
- `Connections.Connect(<source>, <target>)` wires an output parameter to an input parameter
- Plug-in id of Grasshopper 2 is `8307876d-a461-4daa-bb77-eb3715925513`, `PlugIn.IdFromName("Grasshopper2")` returns it
- `LoadAllScopedPlugins()` returns `(int loaded, int failed)`, assert `failed` is zero

### [01.3]-[INPUTS]

`IParameter.PersistentDataWeak` takes a tree for every parameter class, and parameter classes split into generic (`NumberParameter : Parameter<double>`) and non-generic (`CurveParameter : Parameter`) forms, and a cast to `Parameter<T>` fails for the non-generic ones:

```csharp
component.Parameters.Input(0).PersistentDataWeak = Garden.TreeFromList([outline]);
```

- `Garden.TreeFromArrays` builds one branch per array, `TreeFromPears` attaches metadata to items, `TreeFromList(<path>, <items>)` places items on one path
- `Requirement.MustExist` inputs stop `Process` from running when empty, assert that case through the component's messages

### [01.4]-[SOLUTIONS]

- `document.Solution.Start(<CancellationTokenSource>, SolutionMode.Headless)` returns `Task<Solution>`, start it on Rhino's main thread and await it on the test thread
- `Solution.Phase` ends `Completed`, `Cancelled`, or `Faulted`, and a new solution cancels the one running
- `StartWait` solves on the calling thread and blocks Rhino when called on the main thread
- Components run `Process` on worker threads in parallel by default (`ThreadingState.MultiThreaded`), components mutating inputs or touching `RhinoDoc` fail under load alone
- Grasshopper 2 turns itself off on Linux

### [01.5]-[RESULTS]

Every document object exposes `State` (`ObjectSolutionState`) after a solution:

| [INDEX] | [MEMBER]               | [HOLDS]                                                            |
| :-----: | :--------------------- | :----------------------------------------------------------------- |
|  [01]   | `State.Phase`          | `Expired`, `Computing`, `Completed`, `Cancelled`, `Faulted`        |
|  [02]   | `State.Data.Tree()`    | `ITree` of a parameter, `TryConvert<T>(token)` for typed items     |
|  [03]   | `State.Data.Messages`  | `Errors`, `Warnings`, `Remarks`, `ErrorCount`, `WorstMessageLevel` |
|  [04]   | `State.FaultException` | Exception that faulted the object                                  |

- Outputs read from `component.Parameters.Output(<index>).State.Data.Tree()`, messages from `component.State.Data.Messages`
- `ITree` exposes `Paths`, `ItemCount`, `AllItems`, and `NonNullItems`, assert paths and counts before values
- Messages added by `IDataAccess.AddError`, `AddWarning`, `AddRemark`, and the `Verify*` and `Rectify*` members carry `Text` and `Level`
- `.ghz` fixtures assert through their own assert components (IoId `00000000-94d4-407f-b522-7a5f103b2e78`): every item `true`, and no object above `MessageLevel.Remark` unless the fixture expects it

## [02]-[GRASSHOPPER_1]

### [02.1]-[LOADING]

- `PlugIn.LoadPlugIn(new Guid("b45a29b1-4343-4035-989e-044e8580d9cf"))`, then `(RhinoApp.GetPlugInObject("Grasshopper") as GH_RhinoScriptInterface).RunHeadless()`
- `.gha` files of the plug-in under test load through Rhino.Testing `RhinoSetupFixture.LoadGHA`, which calls the private `GH_ComponentServer.LoadGHA`
- Installed `.gha` libraries load with Grasshopper, and a test needing a clean component set replaces `Instances.ComponentServer` before parsing chosen assemblies
- Grasshopper 1 solves on Rhino's main thread, and a worker expires a solution through `RhinoApp.InvokeOnUiThread`

### [02.2]-[SOLVING]

One component without a document:
- Build each input parameter (`Param_Number`), call `CreateAttributes()`, append values to `PersistentData`, and `component.Params.Input[<i>].AddSource(<param>)`
- `component.ExpireSolution(true)`, then `component.Params.Output[<i>].CollectData()` and read `VolatileData`

Components in a document:
- `new GH_Document { Enabled = true }`, `AddObject(<object>, false)`, wire with `AddSource`
- `document.NewSolution(true, GH_SolutionMode.Silent)` solves synchronously without a progress window
- `GH_ActiveObject.RuntimeMessages(GH_RuntimeMessageLevel.Error)` lists errors per object, assert it empty across `document.ActiveObjects()`
- `VolatileData.AllData(true)` lists items, `GH_Structure.TopologyDescription` and `DataDescription` state tree shape for one assertion

Components as functions:
- `Rhino.NodeInCode.Components.FindComponent(<name>)` gives a component function, `Evaluate` returns outputs and messages, `Invoke` throws on the first warning

### [02.3]-[FILES]

- `GH_DocumentIO io = new(); io.Open(<path>)` then `io.Document`, with `Enabled = true` before `NewSolution`
- `GH_NumberSlider.TrySetSliderValue` sets a slider found by `NickName`, then `CollectData` and `ComputeData` on its targets
- Rhino.Testing `TestGrasshopper` passes when every `GH_Boolean` from Context Bake, Context Print, and Assert components is `true`, and fails when none exist
- Round trip through `GH_DocumentIO.SaveQuiet(<path>)` and a reopen, or `GH_Archive.AppendObject` and `ExtractObject`, proves component serialization

## [03]-[FIXTURES]

- Fixture files carry the scenario: inputs on named sliders or panels, assert components, and the expected outcome in the file name (`test_<case>_error.ghz`, `test_<case>_warning.ghz`)
- One fixture per behavior, small enough that a diff of `Differences` or an `.ghx` text diff names the change
- Rhino-free component tests cover only types that never reach native Rhino code, anything that solves needs an installed Rhino

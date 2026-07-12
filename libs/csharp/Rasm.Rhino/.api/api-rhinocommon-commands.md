# [RASM_RHINO_API_RHINOCOMMON_COMMANDS]

This catalog owns the interactive command boundary: the `Rhino.Commands.Command` lifecycle (registration identity, execution, history replay, begin/end/undo signals), `RhinoApp` UI-thread marshalling and script dispatch, `RhinoGet` one-shot modal acquisition, and the `Rhino.Input.Custom` getter family that carries prompts, defaults, command-line options, object-selection policy, constrained dynamic point picking, and transform capture. Every acquisition resolves to a `GetResult`/`Result` outcome and every geometry payload crosses at the boundary the geometry catalog owns; the boundary projects host outcomes onto the `LanguageExt` rails and wraps the closed host vocabularies as `Thinktecture` generated owners.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: RhinoCommon command surface

- host: `RhinoCommon` (Rhino host runtime, in-process)
- assembly: `RhinoCommon`
- namespaces: `Rhino.Commands`, `Rhino`, `Rhino.Input`, `Rhino.Input.Custom`, `Rhino.DocObjects`, `Rhino.DocObjects.Tables`, `Rhino.UI`
- kernel: `Rasm` (host-agnostic vocabularies and numeric owners composed, never re-derived)
- substrate: `LanguageExt.Core`, `Thinktecture.Runtime.Extensions`
- rail: command-boundary

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: lifecycle and dispatch

- rail: command-boundary

| [INDEX] | [SYMBOL]                                 | [KIND]           | [CAPABILITY]                                                            |
| :-----: | :--------------------------------------- | :--------------- | :---------------------------------------------------------------------- |
|  [01]   | `Command`                                | base class       | command identity, execution, history, and lifecycle events              |
|  [02]   | `Result`                                 | outcome enum     | command terminal discriminant                                           |
|  [03]   | `RunMode`                                | enum             | `Interactive`/`Scripted` execution mode                                 |
|  [04]   | `Rhino.DocObjects.ReplayHistoryData`     | borrowed carrier | callback-scoped history inputs and result mutation                      |
|  [05]   | `RhinoApp`                               | static surface   | UI-thread marshalling, lifecycle signals, script dispatch, prompt state |
|  [06]   | `CommandEventArgs` / `UndoRedoEventArgs` | event args       | begin/end and undo/redo lifecycle payloads                              |
|  [07]   | `Rhino.UI.CommandPromptChangedEventArgs` | event args       | command-prompt text, default, and callback-scoped options               |

[PUBLIC_TYPE_SCOPE]: acquisition and getters

- rail: command-boundary

| [INDEX] | [SYMBOL]                      | [KIND]          | [CAPABILITY]                                                                    |
| :-----: | :---------------------------- | :-------------- | :------------------------------------------------------------------------------ |
|  [01]   | `RhinoGet`                    | static surface  | immediate modal acquisition of primitives, geometry, and views                  |
|  [02]   | `GetBaseClass`                | getter base     | prompts, typed defaults, command-line options, accept policy, result projection |
|  [03]   | `GetObject`                   | getter          | pre/post-select policy, geometry filters, single and multiple picks             |
|  [04]   | `GetPoint`                    | getter          | constrained dynamic point acquisition with snap and draw callbacks              |
|  [05]   | `GetTransform`                | abstract getter | subclass-defined transform calculation over interactive point acquisition       |
|  [06]   | `GetResult`                   | outcome enum    | getter terminal discriminant                                                    |
|  [07]   | `Rhino.DocObjects.ObjectType` | flags enum      | geometry-type acquisition filter passed to object getters                       |
|  [08]   | `ObjRef`                      | reference       | acquired object reference with component and geometry accessors                 |
|  [09]   | `GetObjectGeometryFilter`     | delegate        | custom per-candidate acceptance predicate                                       |
|  [10]   | `GetPointMouseEventArgs`      | event args      | dynamic mouse-move payload during point acquisition                             |
|  [11]   | `GetPointDrawEventArgs`       | event args      | per-frame dynamic-draw payload during point acquisition                         |

[PUBLIC_TYPE_SCOPE]: command-line option carriers

- rail: command-boundary

| [INDEX] | [SYMBOL]                         | [KIND]             | [CAPABILITY]                                                 |
| :-----: | :------------------------------- | :----------------- | :----------------------------------------------------------- |
|  [01]   | `CommandLineOption`              | option handle      | the registered option returned by `Option()` after selection |
|  [02]   | `CommandLineOptionType`          | enum               | registered option-kind discriminant                          |
|  [03]   | `OptionToggle`                   | disposable carrier | boolean on/off command-line option state                     |
|  [04]   | `OptionDouble` / `OptionInteger` | disposable carrier | bounded numeric command-line option state                    |
|  [05]   | `OptionString` / `OptionColor`   | disposable carrier | string and color command-line option state                   |

[HOST_ENUMS]:

- `Result` — `Success`/`Cancel`/`Nothing`/`Failure`/`UnknownCommand`/`CancelModelessDialog`/`ExitRhino`.
- `RunMode` — `Interactive`/`Scripted`.
- `GetResult` — `NoResult`/`Cancel`/`Nothing`/`Option`/`Number`/`Color`/`Undo`/`Miss`/`Point`/`Point2d`/`Line2d`/`Rectangle2d`/`Object`/`String`/`CustomMessage`/`Timeout`/`Circle`/`Plane`/`Cylinder`/`Sphere`/`Angle`/`Distance`/`Direction`/`Frame`/`User1`–`User5`/`ExitRhino`.
- `CommandLineOptionType` — `Simple`/`Number`/`Toggle`/`Color`/`List`/`Hidden`.

## [03]-[ENTRYPOINTS]

[COMMAND_LIFECYCLE]:

- `Rhino.Commands.Command.EnglishName : string` — invariant English command identity used for lookup and history.
- `Rhino.Commands.Command.LocalName : string` — localized display name.
- `Rhino.Commands.Command.Settings : PersistentSettings` — per-command persistent settings bag.
- `Rhino.Commands.Command.RunCommand(RhinoDoc doc, RunMode mode) : Result` — protected abstract execution seam each command overrides.
- `Rhino.Commands.Command.ReplayHistory(Rhino.DocObjects.ReplayHistoryData replayData) : bool` — protected virtual history re-solve seam; the host owns and disposes the callback argument, while overrides read its typed values and update its `ReplayHistoryResult` roster inside the call.
- `Rhino.Commands.Command.BeginCommand : EventHandler<CommandEventArgs>` — static command-start signal.
- `Rhino.Commands.Command.EndCommand : EventHandler<CommandEventArgs>` — static command-end signal.
- `Rhino.Commands.Command.UndoRedo : EventHandler<UndoRedoEventArgs>` — static undo/redo recording and replay signal.
- `Rhino.Commands.CommandEventArgs.CommandId : Guid` / `CommandEnglishName : string` / `CommandLocalName : string` / `CommandHelpURL : string` / `CommandPluginName : string` / `CommandResult : Result` / `DocumentRuntimeSerialNumber : uint` / `Document : RhinoDoc` — begin/end command identity, result, and nullable live-document projection.
- `Rhino.Commands.UndoRedoEventArgs.DocumentSerialNumber : uint` / `CommandId : Guid` / `UndoSerialNumber : uint` — undo-record identity beside the mutually exclusive `IsBeforeBeginRecording`/`IsBeginRecording`/`IsBeforeEndRecording`/`IsEndRecording`/`IsBeginUndo`/`IsEndUndo`/`IsBeginRedo`/`IsEndRedo`/`IsPurgeRecord` state flags.

[COMMAND_REGISTRY]:

- `Rhino.Commands.Command.IsValidCommandName(string name) : bool` — command-name validity check.
- `Rhino.Commands.Command.IsCommand(string name) : bool` — registered-command presence probe.
- `Rhino.Commands.Command.LookupCommandId(string name, bool searchForEnglishName) : Guid` — name-to-id resolution.
- `Rhino.Commands.Command.LookupCommandName(Guid commandId, bool englishName) : string` — id-to-name resolution.
- `Rhino.Commands.Command.GetCommandNames(bool english, bool loaded) : string[]` — registered command-name roster.
- `Rhino.Commands.Command.GetMostRecentCommands() : MostRecentCommandDescription[]` — recent-command history roster.
- `Rhino.Commands.Command.GetCommandStack() : Guid[]` — active nested-command id stack.
- `Rhino.Commands.Command.InCommand() : bool` — whether any command is executing.
- `Rhino.Commands.Command.InScriptRunnerCommand() : bool` — whether a script-runner command is executing.
- `Rhino.Commands.Command.RunProxyCommand(Command.RunCommandDelegate commandCallback, RhinoDoc doc, object data) : void` — invokes a delegate-backed proxy command.

[APP_DISPATCH]:

- `Rhino.RhinoApp.IsOnMainThread : bool` — current-thread UI-affinity probe.
- `Rhino.RhinoApp.InvokeRequired : bool` — whether a marshalled invoke is required.
- `Rhino.RhinoApp.InvokeOnUiThread(Delegate method, params object[] args) : void` — fire-and-forget UI-thread marshalling.
- `Rhino.RhinoApp.InvokeAndWait(Action action) : void` — blocking UI-thread marshalling.
- `Rhino.RhinoApp.Idle : EventHandler` — application-idle signal.
- `Rhino.RhinoApp.MainLoop : EventHandler` — per-main-loop-tick signal.
- `Rhino.RhinoApp.CommandPrompt : string` — current command-prompt text.
- `Rhino.RhinoApp.CommandPromptChanged : EventHandler<Rhino.UI.CommandPromptChangedEventArgs>` — prompt-change signal; `Prompt`, `PromptDefault`, and the `CommandLineOption[] Options` roster are callback-scoped snapshots, and option handles never survive the event callback.
- `Rhino.RhinoApp.RunScript(string script, bool echo) : bool` — active-document script execution.
- `Rhino.RhinoApp.RunScript(uint documentSerialNumber, string script, bool echo) : bool` — targeted-document script execution.
- `Rhino.RhinoApp.RunScript(uint documentSerialNumber, string script, string mruDisplayString, bool echo) : bool` — targeted-document script execution with an MRU display string.
- `Rhino.RhinoApp.ExecuteCommand(RhinoDoc document, string commandName) : Result` — named-command execution against a document.
- `Rhino.RhinoApp.RefreshScreen() : void` — forces a screen refresh.

[MODAL_ACQUISITION]:

- `Rhino.Input.RhinoGet.GetPoint(string prompt, bool acceptNothing, out Point3d point) : Result` — modal point acquisition.
- `Rhino.Input.RhinoGet.GetOneObject(string prompt, bool acceptNothing, ObjectType filter, out ObjRef rhObject) : Result` — single filtered object pick.
- `Rhino.Input.RhinoGet.GetMultipleObjects(string prompt, bool acceptNothing, ObjectType filter, out ObjRef[] rhObjects) : Result` — multiple filtered object pick.
- `Rhino.Input.RhinoGet.GetString(string prompt, bool acceptNothing, ref string outputString) : Result` — string acquisition.
- `Rhino.Input.RhinoGet.GetBool(string prompt, bool acceptNothing, string offPrompt, string onPrompt, ref bool boolValue) : Result` — toggle acquisition.
- `Rhino.Input.RhinoGet.GetNumber(string prompt, bool acceptNothing, ref double outputNumber, double lowerLimit, double upperLimit) : Result` — bounded number acquisition.
- `Rhino.Input.RhinoGet.GetInteger(string prompt, bool acceptNothing, ref int outputNumber, int lowerLimit, int upperLimit) : Result` — bounded integer acquisition.
- `Rhino.Input.RhinoGet.GetPlane(out Plane plane) : Result` — plane acquisition.
- `Rhino.Input.RhinoGet.GetRectangle(out Point3d[] corners) : Result` — rectangle-corner acquisition.
- `Rhino.Input.RhinoGet.GetBox(out Box box) : Result` — box acquisition.
- `Rhino.Input.RhinoGet.GetLine(out Line line) : Result` — line acquisition.
- `Rhino.Input.RhinoGet.GetPolyline(out Polyline polyline) : Result` — polyline acquisition.
- `Rhino.Input.RhinoGet.GetArc(out Arc arc) : Result` — arc acquisition.
- `Rhino.Input.RhinoGet.GetCircle(out Circle circle) : Result` — circle acquisition.
- `Rhino.Input.RhinoGet.GetView(string commandPrompt, out RhinoView view) : Result` — view acquisition.
- `Rhino.Input.RhinoGet.GetViewports(string commandPrompt, out RhinoViewport[] viewports) : Result` — viewport-set acquisition.
- `Rhino.Input.RhinoGet.GetDistance(string commandPrompt, double defaultDistance, out double distance) : Result` — distance acquisition.
- `Rhino.Input.RhinoGet.GetFileName(GetFileNameMode mode, string defaultName, string title, object parent) : string` — file-name dialog acquisition; cancellation returns non-admitted empty text rather than a `Result`.
- `Rhino.Input.RhinoGet.InGet(RhinoDoc doc) : bool` — active-getter state probe.
- `Rhino.Input.RhinoGet.InGetPoint(RhinoDoc doc) : bool` — active point-getter state probe.
- `Rhino.Input.RhinoGet.InGetObject(RhinoDoc doc) : bool` — active object-getter state probe.
- `Rhino.RhinoDoc.InGetPoint : bool` — document-scoped active point-getter probe.
- every `out` object, array, view, viewport, or geometry payload is admitted only when the returned `Result` is `Success`; cancellation, nothing, and failure leave the payload non-admitted, and each returned `ObjRef` remains caller-disposed.

[GETTER_PROMPTS_AND_DEFAULTS]:

- `Rhino.Input.Custom.GetBaseClass.SetCommandPrompt(string prompt) : void` — sets the getter prompt.
- `Rhino.Input.Custom.GetBaseClass.SetCommandPromptDefault(string defaultValue) : void` — sets the displayed default token.
- `Rhino.Input.Custom.GetBaseClass.SetDefaultPoint(Point3d point) : void` — typed default point.
- `Rhino.Input.Custom.GetBaseClass.SetDefaultNumber(double defaultNumber) : void` — typed default number.
- `Rhino.Input.Custom.GetBaseClass.SetDefaultInteger(int defaultValue) : void` — typed default integer.
- `Rhino.Input.Custom.GetBaseClass.SetDefaultString(string defaultValue) : void` — typed default string.
- `Rhino.Input.Custom.GetBaseClass.SetDefaultColor(Color defaultColor) : void` — typed default color.

[GETTER_OPTIONS]:

- `Rhino.Input.Custom.GetBaseClass.AddOption(string englishOption) : int` / `AddOption(string englishOption, string englishOptionValue) : int` / `AddOption(string englishOption, string englishOptionValue, bool hiddenOption) : int` — bare, value-named, and hidden English option registrations.
- `Rhino.Input.Custom.GetBaseClass.AddOption(LocalizeStringPair optionName) : int` / `AddOption(LocalizeStringPair optionName, LocalizeStringPair optionValue) : int` / `AddOption(LocalizeStringPair optionName, LocalizeStringPair optionValue, bool hiddenOption) : int` — localized option registrations.
- `Rhino.Input.Custom.GetBaseClass.AddOptionToggle(string englishName, ref OptionToggle toggleValue) : int` — registers a toggle option.
- `Rhino.Input.Custom.GetBaseClass.AddOptionToggle(LocalizeStringPair optionName, ref OptionToggle toggleValue) : int` — localized toggle registration.
- `Rhino.Input.Custom.GetBaseClass.AddOptionDouble(string englishName, ref OptionDouble numberValue) : int` / `AddOptionDouble(string englishName, ref OptionDouble numberValue, string prompt) : int`; the same pair accepts `LocalizeStringPair optionName` — English or localized numeric registration.
- `Rhino.Input.Custom.GetBaseClass.AddOptionInteger(string englishName, ref OptionInteger intValue) : int` / `AddOptionInteger(string englishName, ref OptionInteger intValue, string prompt) : int`; the same pair accepts `LocalizeStringPair optionName` — English or localized integer registration.
- `Rhino.Input.Custom.GetBaseClass.AddOptionColor(string englishName, ref OptionColor colorValue) : int` / `AddOptionColor(string englishName, ref OptionColor colorValue, string prompt) : int`; the same pair accepts `LocalizeStringPair optionName` — English or localized color registration.
- `Rhino.Input.Custom.GetBaseClass.AddOptionString(string englishName, ref OptionString stringValue) : int` / `AddOptionString(string englishName, ref OptionString stringValue, string prompt) : int`; the same pair accepts `LocalizeStringPair optionName` — English or localized string registration.
- `Rhino.Input.Custom.GetBaseClass.AddOptionList(string englishOptionName, IEnumerable<string> listValues, int listCurrentIndex) : int` — registers a string-list option.
- `Rhino.Input.Custom.GetBaseClass.AddOptionList(LocalizeStringPair optionName, IEnumerable<LocalizeStringPair> listValues, int listCurrentIndex) : int` — registers a localized string-list option.
- `Rhino.Input.Custom.GetBaseClass.AddOptionEnumList<T>(string englishOptionName, T defaultValue) : int` / `AddOptionEnumList<T>(string englishOptionName, T defaultValue, T[] include) : int where T : struct, IConvertible` — registers an enum-backed option list with an admitted subset overload.
- `Rhino.Input.Custom.GetBaseClass.AddOptionEnumSelectionList<T>(string englishOptionName, IEnumerable<T> enumSelection, int listCurrentIndex) : int where T : struct, IConvertible` — registers a filtered enum option list.
- `Rhino.Input.Custom.GetBaseClass.GetSelectedEnumValue<T>() : T` / `GetSelectedEnumValueFromSelectionList<T>(IEnumerable<T> selectionList) : T where T : struct, IConvertible` — resolves the selected enum value against the registered complete or filtered roster.
- `Rhino.Input.Custom.GetBaseClass.ClearCommandOptions() : void` — clears every registered option before a replacement registration pass.
- `OptionToggle`, `OptionDouble`, `OptionInteger`, `OptionString`, and `OptionColor` implement `IDisposable`; the owner that constructs a carrier retains it through every getter call using the registered option and disposes it after the getter window closes.

[GETTER_ACCEPT_POLICY]:

- `Rhino.Input.Custom.GetBaseClass.EnableTransparentCommands(bool enable) : void` — permits transparent-command interruption.
- `Rhino.Input.Custom.GetBaseClass.AcceptNothing(bool enable) : void` — accepts an empty-enter terminal.
- `Rhino.Input.Custom.GetBaseClass.AcceptUndo(bool enable) : void` — accepts an undo terminal.
- `Rhino.Input.Custom.GetBaseClass.AcceptEnterWhenDone(bool enable) : void` — accepts enter as done.
- `Rhino.Input.Custom.GetBaseClass.AcceptNumber(bool enable, bool acceptZero) : void` — accepts a number terminal.
- `Rhino.Input.Custom.GetBaseClass.AcceptPoint(bool enable) : void` — accepts a point terminal.
- `Rhino.Input.Custom.GetBaseClass.AcceptColor(bool enable) : void` — accepts a color terminal.
- `Rhino.Input.Custom.GetBaseClass.AcceptString(bool enable) : void` — accepts a string terminal.

[GETTER_RESULT]:

- `Rhino.Input.Custom.GetBaseClass.Result() : GetResult` — the terminal getter outcome discriminant.
- `Rhino.Input.Custom.GetBaseClass.CommandResult() : Result` — the getter outcome projected as a command `Result`.
- `Rhino.Input.Custom.GetBaseClass.Option() : CommandLineOption` — the pointer-backed selected option, or `null` unless `Result()` is `GetResult.Option`; it never outlives its getter.
- `Rhino.Input.Custom.GetBaseClass.OptionIndex() : int` — the selected option index, or `-1` when no option exists.
- `Rhino.Input.Custom.GetBaseClass.Number() : double` / `Point() : Point3d` / `Vector() : Vector3d` / `Color() : Color` — typed terminal payload reads gated by their corresponding `GetResult` cases.
- `Rhino.Input.Custom.GetBaseClass.View() : RhinoView` — the picked view, or `null` when the terminal carries no view.
- `Rhino.Input.Custom.GetBaseClass.PickRectangle() : Rectangle` / `Point2d() : System.Drawing.Point` / `Rectangle2d() : Rectangle` / `Line2d() : System.Drawing.Point[]` — screen-space terminal payloads.
- `Rhino.Input.Custom.GetBaseClass.GotDefault() : bool` — whether the terminal outcome was the seeded default.
- `Rhino.Input.Custom.GetBaseClass.StringResult() : string` — the acquired string payload; absent native text projects as `string.Empty`.
- `Rhino.Input.Custom.GetBaseClass.SetWaitDuration(int milliseconds) : void` — arms the `GetResult.Timeout` terminal.
- `Rhino.Input.Custom.GetBaseClass.SetOptionVaries(int optionIndex, bool varies) : void` — marks an option value as varying.
- `Rhino.Input.Custom.GetBaseClass : IDisposable` with public `Dispose()` — every getter lifetime rides a using scope or lease, and every pointer-backed option or result projection stays inside that lifetime.
- `Rhino.Input.Custom.OptionToggle(bool initialValue, string offValue, string onValue)` / `(bool initialValue, LocalizeStringPair offValue, LocalizeStringPair onValue)` — toggle carrier constructors.
- `Rhino.Input.Custom.OptionToggle.InitialValue : bool` / `CurrentValue : bool` — initial and mutable live toggle state.
- `Rhino.Input.Custom.OptionDouble(double initialValue)` / `(double initialValue, double lowerLimit, double upperLimit)` / `(double initialValue, bool setLowerLimit, double limit)` and `OptionInteger` equivalents over `int` — numeric carrier constructors with `InitialValue` and mutable `CurrentValue`.
- `Rhino.Input.Custom.OptionString(string initialString)` / `(string initialString, bool allowEmptyString)` and `OptionColor(Color initialValue)` — value carrier constructors with `InitialValue` and mutable `CurrentValue`.
- `Rhino.UI.LocalizeStringPair(string english, string local)` — immutable localized option binder with `English`/`Local` reads and implicit string projection.
- `Rhino.Input.Custom.CommandLineOption.Index : int` / `OptionType : CommandLineOptionType` / `EnglishName : string` / `LocalName : string` — selected option identity and kind.
- `Rhino.Input.Custom.CommandLineOption.CurrentListOptionIndex : int` / `CurrentToggleValue : bool?` / `CurrentNumericValue : double` / `StringOptionValue : string` — selected option values gated by `OptionType`.
- `Rhino.Input.Custom.CommandLineOption.ListOptions(bool english) : string[]` / `ToggleValues(bool english, out string offValue, out string onValue) : void` — localized list and toggle labels.
- `Rhino.Input.Custom.CommandLineOption.IsValidOptionName(string) : bool` / `IsValidOptionValueName(string) : bool` — option-name admission probes.

[OBJECT_SELECTION]:

- `Rhino.Input.Custom.GetObject.EnablePreSelect(bool enable, bool ignoreUnacceptablePreselectedObjects) : void` — pre-selection policy.
- `Rhino.Input.Custom.GetObject.EnablePostSelect(bool enable) : void` — post-selection policy.
- `Rhino.Input.Custom.GetObject.EnableSelPrevious(bool enable) : void` — previous-selection reuse policy.
- `Rhino.Input.Custom.GetObject.EnableHighlight(bool enable) : void` — highlight policy.
- `Rhino.Input.Custom.GetObject.EnableIgnoreGrips(bool enable) : void` — grip-ignore policy.
- `Rhino.Input.Custom.GetObject.EnablePressEnterWhenDonePrompt(bool enable) : void` — enter-when-done prompt policy.
- `Rhino.Input.Custom.GetObject.SetCustomGeometryFilter(GetObjectGeometryFilter filter) : void` — per-candidate acceptance predicate.
- `Rhino.Input.Custom.GetObject.GetMultiple(int minimumNumber, int maximumNumber) : GetResult` — bounded multi-object acquisition.
- `Rhino.Input.Custom.GetObject.Objects() : ObjRef[]` — acquired object references; each returned `ObjRef` owns its native reference copy, may outlive the getter, and remains caller-disposed.
- `Rhino.DocObjects.ObjRef.CurveParameter(out double parameter) : Curve` / `SurfaceParameter(out double u, out double v) : Surface` — nullable parametric projections; the returned geometry is non-null only when the reference addresses that part.
- `Rhino.DocObjects.ObjRef.SelectionMethod() : SelectionMethod` / `SelectionPoint() : Point3d` / `SelectionView() : RhinoView` / `SelectionViewDetailSerialNumber() : uint` — detached pick method, point, nullable view, and detail serial evidence.
- `Rhino.DocObjects.ObjRef.Geometry()` / `Object()` / `Curve()` / `Surface()` / `Brep()` / `Face()` / `Edge()` / `Trim()` / `Mesh()` / `SubD()` / `SubDFace()` / `SubDEdge()` / `SubDVertex()` / `Point()` / `InstanceDefinitionPart()` / `ClippingPlaneSurface()` / `TextDot()` / `TextEntity()` / `PointCloud()` / `Light()` / `Hatch()` — nullable typed projections selected by the referenced whole object or component.
- `Rhino.Input.Custom.PickContext : IDisposable` — per-pick context with `View`, `PickLine`, `PickStyle`, `PickMode`, `PickGroupsEnabled`, `SubObjectSelectionEnabled`, `GetObjectUsed`, `SetPickTransform(Transform)`, and `UpdateClippingPlanes()`; `PickStyle` is `None`/`PointPick`/`WindowPick`/`CrossingPick`, and `PickMode` is `Wireframe`/`Shaded`.
- `Rhino.DocObjects.Tables.ObjectTable.PickObjects(PickContext pickContext) : ObjRef[]` — programmatic pick; the context and every returned reference are caller-disposed after projection.

[POINT_ACQUISITION]:

- `Rhino.Input.Custom.GetPoint.MouseMove : EventHandler<GetPointMouseEventArgs>` — dynamic mouse-move signal.
- `Rhino.Input.Custom.GetPoint.MouseDown : EventHandler<GetPointMouseEventArgs>` — dynamic mouse-down signal.
- `Rhino.Input.Custom.GetPoint.DynamicDraw : EventHandler<GetPointDrawEventArgs>` — per-frame dynamic-draw signal.
- `Rhino.Input.Custom.GetPoint.PostDrawObjects : EventHandler<DrawEventArgs>` — post-object overlay-draw signal.
- `Rhino.Input.Custom.GetPoint.SetBasePoint(Point3d basePoint, bool showDistanceInStatusBar) : void` — anchors the base point.
- `Rhino.Input.Custom.GetPoint.Constrain(Curve curve, bool allowPickingPointOffObject) : bool` — constrains acquisition to a curve.
- `Rhino.Input.Custom.GetPoint.Constrain(Surface surface, bool allowPickingPointOffObject) : bool` — constrains acquisition to a surface.
- `Rhino.Input.Custom.GetPoint.Constrain(Brep brep, int wireDensity, int faceIndex, bool allowPickingPointOffObject) : bool` — constrains acquisition to a brep face.
- `Rhino.Input.Custom.GetPoint.Constrain(Mesh mesh, bool allowPickingPointOffObject) : bool` — constrains acquisition to a mesh.
- `Rhino.Input.Custom.GetPoint.ConstrainToConstructionPlane(bool throughBasePoint) : bool` — constrains acquisition to the construction plane.
- `Rhino.Input.Custom.GetPoint.AddSnapPoint(Point3d point) : int` — registers a snap point.
- `Rhino.Input.Custom.GetPoint.AddConstructionPoint(Point3d point) : int` — registers a construction point.
- `Rhino.Input.Custom.GetPoint.Get(bool onMouseUp, bool get2DPoint) : GetResult` — runs point acquisition.
- `Rhino.Input.Custom.GetPoint.Constrain(Point3d from, Point3d to) : bool` / `Constrain(Line line) : bool` / `Constrain(Arc arc) : bool` / `Constrain(Circle circle) : bool` / `Constrain(Plane plane, bool allowElevator) : bool` / `Constrain(Sphere sphere) : bool` / `Constrain(Cylinder cylinder) : bool` — analytic constraints beside the curve/surface/brep/mesh overloads.
- `Rhino.Input.Custom.GetPoint.ConstrainToTargetPlane() : void` / `ConstrainToVirtualCPlaneIntersection(Plane plane) : bool` / `ConstrainDistanceFromBasePoint(double distance) : void` — target, virtual-construction-plane, and radial constraints.
- `Rhino.Input.Custom.GetPoint.SetCursor(CursorStyle cursor) : void` / `EnableObjectSnapCursors(bool enable) : void` — cursor policy.
- `Rhino.Input.Custom.GetPoint.PermitOrthoSnap(bool permit) : void` / `PermitObjectSnap(bool permit) : void` / `PermitConstraintOptions(bool permit) : void` / `PermitFromOption(bool permit) : void` / `PermitTabMode(bool permit) : void` / `PermitElevatorMode(int permitMode) : void` — snap and modality permits.
- `Rhino.Input.Custom.GetPoint.EnableCurveSnapTangentBar(bool drawTangentBarAtSnapPoint, bool drawEndPoints) : void` / `EnableCurveSnapPerpBar(bool drawPerpBarAtSnapPoint, bool drawEndPoints) : void` / `EnableCurveSnapArrow(bool drawDirectionArrowAtSnapPoint, bool reverseArrow) : void` / `EnableSnapToCurves(bool enable) : void` — curve-snap feedback policy.
- `Rhino.Input.Custom.GetPoint.EnableNoRedrawOnExit(bool noRedraw) : void` / `FullFrameRedrawDuringGet : bool { get; set; }` — redraw policy across and during the get.
- `Rhino.Input.Custom.GetPoint.DrawLineFromPoint(Point3d startPoint, bool showDistanceInStatusBar) : void` / `EnableDrawLineFromPoint(bool enable) : void` — rubber-band line policy.
- `Rhino.Input.Custom.GetPoint.AddSnapPoints(Point3d[] points) : int` / `AddConstructionPoints(Point3d[] points) : int` / `GetSnapPoints() : Point3d[]` / `GetConstructionPoints() : Point3d[]` — bulk snap/construction registration and readback.
- `Rhino.Input.Custom.GetPoint.TryGetBasePoint(out Point3d basePoint) : bool` — the seeded base-point read.
- `Rhino.Input.Custom.GetPoint.OsnapEventType : OsnapModes` — the snap kind the dynamic events report.
- `Rhino.Input.Custom.GetNumber.SetLowerLimit(double lowerLimit, bool strictlyGreaterThan) : void` / `SetUpperLimit(double upperLimit, bool strictlyLessThan) : void`; `GetInteger` carries the same pair over `int` — numeric window gates.
- `Rhino.RhinoApp.WriteLine(string message) : void` — the command-line transcript write the interaction receipts compose.

[TRANSFORM_ACQUISITION]:

- `Rhino.Input.Custom.GetTransform.CalculateTransform(RhinoViewport viewport, Point3d point) : Transform` — resolves the transform from the current pick.
- `Rhino.Input.Custom.GetTransform.GetXform() : GetResult` — runs interactive transform acquisition.

## [04]-[IMPLEMENTATION_LAW]

[COMMAND_TOPOLOGY]:

- `Command` owns identity and the `RunCommand`/`ReplayHistory` execution seam; `RhinoApp` owns thread affinity and script dispatch; `RhinoGet` owns one-shot modal acquisition; the `Rhino.Input.Custom` getters own multi-step interactive acquisition with options, constraints, and dynamic draw. Acquisition state (`InGet`/`InGetPoint`/`InGetObject`) is read from `RhinoGet` and `RhinoDoc`, never inferred.
- every getter terminates in a `GetResult`, a command terminates in a `Result`, and both discriminants carry the cancel and nothing outcomes distinctly from failure; the boundary reads the discriminant and never treats a cancel as an error.
- all mutating and prompt-bearing calls run on the UI thread; off-thread work marshals through `RhinoApp.InvokeOnUiThread` (fire-and-forget) or `RhinoApp.InvokeAndWait` (blocking), gated by `IsOnMainThread`/`InvokeRequired`.

[STACKING]:

- `LanguageExt.Core`(`api-languageext`): a successful `Result`/`GetResult` payload projects to `Fin<A>.Succ`, fault terminals project to typed `Error`, and `Cancel`/`Nothing` remain explicit non-fault union cases; a bounded multi-object `GetMultiple` fans per-candidate acceptance into `Validation<Error, Seq<ObjRef>>`; detached object projections and option rosters land as `Seq<A>`/`Arr<A>`, and a nullable acquired value lifts to `Option<A>`.
- `Thinktecture.Runtime.Extensions`(`api-thinktecture-runtime-extensions`): the closed host discriminants — `Result`, `RunMode`, `GetResult`, `CommandLineOptionType`, and the `ObjectType` acquisition filter — wrap as `[SmartEnum<TKey>]` / `[Flags]`-backed owners; the getter terminal (point, object, option, number, string, cancel, nothing) models as a `[Union]`; a `Guid` command identity wraps as a `[ValueObject<T>]`.
- `Rasm` kernel: bounded numeric ranges, unit values, and easing/interpolation for dynamic-draw feedback compose the kernel owners; the boundary re-derives none of them.

[LOCAL_ADMISSION]:

- a command enters through a `Command`-derived owner whose `RunCommand` returns a projected `Result`; interactive acquisition enters through a getter owner that registers options and defaults, applies the accept policy, runs `Get`/`GetMultiple`/`GetXform`, and projects the `GetResult` onto a `Fin` rail keyed to the acquired payload.
- host getter and option carrier types never leak past the boundary; downstream code holds the projected rail value and the canonical geometry payload the geometry catalog admits.

[RAIL_LAW]:

- Surface: `Rhino.Commands` + `Rhino.Input` + `Rhino.Input.Custom`
- Owns: the command lifecycle, UI-thread and script dispatch, modal acquisition, and the interactive getter family with options, constraints, and dynamic draw.
- Accept: command execution and history, one-shot and multi-step acquisition, command-line options, selection policy, constrained point picking, and transform capture projected onto `Fin`/`Validation` rails.
- Reject: exception-style getter outcomes (the `GetResult` discriminant is the rail), inferred acquisition state, off-thread prompt or mutation calls, and leaking host getter/option types past the boundary.

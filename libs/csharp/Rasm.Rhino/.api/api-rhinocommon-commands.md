# [RASM_RHINO_API_RHINOCOMMON_COMMANDS]

This catalog owns the interactive command boundary: the `Rhino.Commands.Command` lifecycle (registration identity, execution, history replay, begin/end/undo signals), `RhinoApp` UI-thread marshalling and script dispatch, `RhinoGet` one-shot modal acquisition, and the `Rhino.Input.Custom` getter family that carries prompts, defaults, command-line options, object-selection policy, constrained dynamic point picking, and transform capture. Every acquisition resolves to a `GetResult`/`Result` outcome and every geometry payload crosses at the boundary the geometry catalog owns; the boundary projects host outcomes onto the `LanguageExt` rails and wraps the closed host vocabularies as `Thinktecture` generated owners.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: RhinoCommon command surface
- host: `RhinoCommon` (Rhino host runtime, in-process)
- assembly: `RhinoCommon`
- namespaces: `Rhino.Commands`, `Rhino`, `Rhino.Input`, `Rhino.Input.Custom`
- kernel: `Rasm` (host-agnostic vocabularies and numeric owners composed, never re-derived)
- substrate: `LanguageExt.Core`, `Thinktecture.Runtime.Extensions`
- rail: command-boundary

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: lifecycle and dispatch
- rail: command-boundary

| [INDEX] | [SYMBOL]                                 | [KIND]         | [CAPABILITY]                                                                 |
| :-----: | :--------------------------------------- | :------------- | :--------------------------------------------------------------------------- |
|  [01]   | `Command`                                | base class     | per-command identity, execution override, history, and lifecycle events      |
|  [02]   | `Result`                                 | outcome enum   | command/getter terminal result (`Success`, `Cancel`, `Nothing`, `Failure`)   |
|  [03]   | `RunMode`                                | enum           | interactive versus scripted execution mode passed to `RunCommand`            |
|  [04]   | `ReplayHistoryData`                      | carrier        | history inputs delivered to `ReplayHistory`                                  |
|  [05]   | `RhinoApp`                               | static surface | UI-thread marshalling, idle/main-loop signals, script dispatch, prompt state |
|  [06]   | `CommandEventArgs` / `UndoRedoEventArgs` | event args     | begin/end and undo/redo lifecycle payloads                                   |
|  [07]   | `CommandPromptChangedEventArgs`          | event args     | command-prompt presentation change payload                                   |

[PUBLIC_TYPE_SCOPE]: acquisition and getters
- rail: command-boundary

| [INDEX] | [SYMBOL]                  | [KIND]         | [CAPABILITY]                                                                     |
| :-----: | :------------------------ | :------------- | :------------------------------------------------------------------------------- |
|  [01]   | `RhinoGet`                | static surface | immediate modal acquisition of primitives, geometry, and views                   |
|  [02]   | `GetBaseClass`            | getter base    | prompts, typed defaults, command-line options, accept policy, result projection  |
|  [03]   | `GetObject`               | getter         | pre/post-select policy, geometry filters, single and multiple picks              |
|  [04]   | `GetPoint`                | getter         | constrained dynamic point acquisition with snap and draw callbacks               |
|  [05]   | `GetTransform`            | getter         | interactive transform capture from a base viewport pick                          |
|  [06]   | `GetResult`               | outcome enum   | the getter terminal outcome discriminant (point, object, option, number, cancel) |
|  [07]   | `ObjectType`              | flags enum     | geometry-type acquisition filter passed to object getters                        |
|  [08]   | `ObjRef`                  | reference      | acquired object reference with component and geometry accessors                  |
|  [09]   | `GetObjectGeometryFilter` | delegate       | custom per-candidate acceptance predicate                                        |
|  [10]   | `GetPointMouseEventArgs`  | event args     | dynamic mouse-move payload during point acquisition                              |
|  [11]   | `GetPointDrawEventArgs`   | event args     | per-frame dynamic-draw payload during point acquisition                          |

[PUBLIC_TYPE_SCOPE]: command-line option carriers
- rail: command-boundary

| [INDEX] | [SYMBOL]                         | [KIND]          | [CAPABILITY]                                                 |
| :-----: | :------------------------------- | :-------------- | :----------------------------------------------------------- |
|  [01]   | `CommandLineOption`              | option handle   | the registered option returned by `Option()` after selection |
|  [02]   | `CommandLineOptionType`          | enum            | option category discriminant                                 |
|  [03]   | `OptionToggle`                   | toggle carrier  | boolean on/off command-line option state                     |
|  [04]   | `OptionDouble` / `OptionInteger` | numeric carrier | bounded numeric command-line option state                    |
|  [05]   | `OptionString` / `OptionColor`   | value carrier   | string and color command-line option state                   |

## [03]-[ENTRYPOINTS]

[COMMAND_LIFECYCLE]:
- `Rhino.Commands.Command.EnglishName : string` — invariant English command identity used for lookup and history.
- `Rhino.Commands.Command.LocalName : string` — localized display name.
- `Rhino.Commands.Command.Settings : PersistentSettings` — per-command persistent settings bag.
- `Rhino.Commands.Command.RunCommand(RhinoDoc doc, RunMode mode) : Result` — the execution seam each command overrides.
- `Rhino.Commands.Command.ReplayHistory(ReplayHistoryData replayData) : bool` — history-driven re-solve of a recorded command.
- `Rhino.Commands.Command.BeginCommand : EventHandler<CommandEventArgs>` — command-start signal.
- `Rhino.Commands.Command.EndCommand : EventHandler<CommandEventArgs>` — command-end signal.
- `Rhino.Commands.Command.UndoRedo : EventHandler<UndoRedoEventArgs>` — undo/redo signal.

[COMMAND_REGISTRY]:
- `Rhino.Commands.Command.IsValidCommandName(string name) : bool` — command-name validity check.
- `Rhino.Commands.Command.LookupCommandId(string name, bool searchForEnglishName) : Guid` — name-to-id resolution.
- `Rhino.Commands.Command.LookupCommandName(Guid commandId, bool englishName) : string` — id-to-name resolution.
- `Rhino.Commands.Command.GetCommandNames(bool english, bool loaded) : string[]` — registered command-name roster.
- `Rhino.Commands.Command.GetMostRecentCommands() : MostRecentCommandDescription[]` — recent-command history roster.
- `Rhino.Commands.Command.GetCommandStack() : Guid[]` — active nested-command id stack.
- `Rhino.Commands.Command.InCommand() : bool` — whether any command is executing.
- `Rhino.Commands.Command.InScriptRunnerCommand() : bool` — whether a script-runner command is executing.
- `Rhino.Commands.Command.RunProxyCommand(RunCommandDelegate commandCallback, RhinoDoc doc, object data) : void` — invokes a delegate-backed proxy command.

[APP_DISPATCH]:
- `Rhino.RhinoApp.IsOnMainThread : bool` — current-thread UI-affinity probe.
- `Rhino.RhinoApp.InvokeRequired : bool` — whether a marshalled invoke is required.
- `Rhino.RhinoApp.InvokeOnUiThread(Delegate method, params object[] args) : void` — fire-and-forget UI-thread marshalling.
- `Rhino.RhinoApp.InvokeAndWait(Action action) : void` — blocking UI-thread marshalling.
- `Rhino.RhinoApp.Idle : EventHandler` — application-idle signal.
- `Rhino.RhinoApp.MainLoop : EventHandler` — per-main-loop-tick signal.
- `Rhino.RhinoApp.CommandPrompt : string` — current command-prompt text.
- `Rhino.RhinoApp.CommandPromptChanged : EventHandler<CommandPromptChangedEventArgs>` — prompt-change signal.
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
- `Rhino.Input.RhinoGet.GetFileName(GetFileNameMode mode, string defaultName, string title, object parent) : string` — file-name dialog acquisition.
- `Rhino.Input.RhinoGet.InGet(RhinoDoc doc) : bool` — active-getter state probe.
- `Rhino.Input.RhinoGet.InGetPoint(RhinoDoc doc) : bool` — active point-getter state probe.
- `Rhino.Input.RhinoGet.InGetObject(RhinoDoc doc) : bool` — active object-getter state probe.
- `Rhino.RhinoDoc.InGetPoint : bool` — document-scoped active point-getter probe.

[GETTER_PROMPTS_AND_DEFAULTS]:
- `Rhino.Input.Custom.GetBaseClass.SetCommandPrompt(string prompt) : void` — sets the getter prompt.
- `Rhino.Input.Custom.GetBaseClass.SetCommandPromptDefault(string defaultValue) : void` — sets the displayed default token.
- `Rhino.Input.Custom.GetBaseClass.SetDefaultPoint(Point3d point) : void` — typed default point.
- `Rhino.Input.Custom.GetBaseClass.SetDefaultNumber(double defaultNumber) : void` — typed default number.
- `Rhino.Input.Custom.GetBaseClass.SetDefaultInteger(int defaultValue) : void` — typed default integer.
- `Rhino.Input.Custom.GetBaseClass.SetDefaultString(string defaultValue) : void` — typed default string.
- `Rhino.Input.Custom.GetBaseClass.SetDefaultColor(Color defaultColor) : void` — typed default color.

[GETTER_OPTIONS]:
- `Rhino.Input.Custom.GetBaseClass.AddOptionToggle(string englishName, ref OptionToggle toggleValue) : int` — registers a toggle option.
- `Rhino.Input.Custom.GetBaseClass.AddOptionDouble(string englishName, ref OptionDouble numberValue, string prompt) : int` — registers a numeric option.
- `Rhino.Input.Custom.GetBaseClass.AddOptionInteger(string englishName, ref OptionInteger intValue, string prompt) : int` — registers an integer option.
- `Rhino.Input.Custom.GetBaseClass.AddOptionColor(string englishName, ref OptionColor colorValue, string prompt) : int` — registers a color option.
- `Rhino.Input.Custom.GetBaseClass.AddOptionString(string englishName, ref OptionString stringValue, string prompt) : int` — registers a string option.
- `Rhino.Input.Custom.GetBaseClass.AddOptionList(string englishOptionName, IEnumerable<string> listValues, int listCurrentIndex) : int` — registers a string-list option.
- `Rhino.Input.Custom.GetBaseClass.AddOptionEnumList<T>(string englishOptionName, T defaultValue) : int` — registers an enum-backed option list.
- `Rhino.Input.Custom.GetBaseClass.AddOptionEnumSelectionList<T>(string englishOptionName, IEnumerable<T> enumSelection, int listCurrentIndex) : int` — registers a filtered enum option list.

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
- `Rhino.Input.Custom.GetBaseClass.Option() : CommandLineOption` — the selected command-line option.
- `Rhino.Input.Custom.GetBaseClass.OptionIndex() : int` — the selected option index.

[OBJECT_SELECTION]:
- `Rhino.Input.Custom.GetObject.EnablePreSelect(bool enable, bool ignoreUnacceptablePreselectedObjects) : void` — pre-selection policy.
- `Rhino.Input.Custom.GetObject.EnablePostSelect(bool enable) : void` — post-selection policy.
- `Rhino.Input.Custom.GetObject.EnableSelPrevious(bool enable) : void` — previous-selection reuse policy.
- `Rhino.Input.Custom.GetObject.EnableHighlight(bool enable) : void` — highlight policy.
- `Rhino.Input.Custom.GetObject.EnableIgnoreGrips(bool enable) : void` — grip-ignore policy.
- `Rhino.Input.Custom.GetObject.EnablePressEnterWhenDonePrompt(bool enable) : void` — enter-when-done prompt policy.
- `Rhino.Input.Custom.GetObject.SetCustomGeometryFilter(GetObjectGeometryFilter filter) : void` — per-candidate acceptance predicate.
- `Rhino.Input.Custom.GetObject.GetMultiple(int minimumNumber, int maximumNumber) : GetResult` — bounded multi-object acquisition.
- `Rhino.Input.Custom.GetObject.Objects() : ObjRef[]` — the acquired object references.

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

[TRANSFORM_ACQUISITION]:
- `Rhino.Input.Custom.GetTransform.CalculateTransform(RhinoViewport viewport, Point3d point) : Transform` — resolves the transform from the current pick.
- `Rhino.Input.Custom.GetTransform.GetXform() : GetResult` — runs interactive transform acquisition.

## [04]-[IMPLEMENTATION_LAW]

[COMMAND_TOPOLOGY]:
- `Command` owns identity and the `RunCommand`/`ReplayHistory` execution seam; `RhinoApp` owns thread affinity and script dispatch; `RhinoGet` owns one-shot modal acquisition; the `Rhino.Input.Custom` getters own multi-step interactive acquisition with options, constraints, and dynamic draw. Acquisition state (`InGet`/`InGetPoint`/`InGetObject`) is read from `RhinoGet` and `RhinoDoc`, never inferred.
- every getter terminates in a `GetResult`, a command terminates in a `Result`, and both discriminants carry the cancel and nothing outcomes distinctly from failure; the boundary reads the discriminant and never treats a cancel as an error.
- all mutating and prompt-bearing calls run on the UI thread; off-thread work marshals through `RhinoApp.InvokeOnUiThread` (fire-and-forget) or `RhinoApp.InvokeAndWait` (blocking), gated by `IsOnMainThread`/`InvokeRequired`.

[STACKING]:
- `LanguageExt.Core`(`api-languageext`): a `Result`/`GetResult` outcome projects to `Fin<A>` (`Success` to `Succ`, `Cancel`/`Nothing`/`Failure` to a typed `Error`); a bounded multi-object `GetMultiple` fans its per-candidate acceptance into `Validation<Error, Seq<ObjRef>>`; `ObjRef[]` and option rosters land as `Seq<A>`/`Arr<A>`; a nullable acquired value lifts to `Option<A>`.
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

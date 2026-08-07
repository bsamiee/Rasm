# [RASM_GRASSHOPPER_API_GH2_DOCUMENT]

`Grasshopper2` is the Rhino 9 WIP visual-programming host, and its `Document` is the single mutable authority over one canvas definition. Every structural change enters through `DocumentMethods` (or `Grasshopper2.Parameters.Connections` for wire mutation) paired with a `Grasshopper2.Undo.ActionList`, so a mutation and its undo record seal as one act. Graph traversal reads through `Grasshopper2.Doc.Connectivity` over `ConnectiveObject`s, and execution runs on `SolutionServer` publishing the solution lifecycle over `Solution`/`SolutionRecord` phase state.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Grasshopper2` document graph
- host: `Grasshopper2.dll` inside `Grasshopper2Plugin.rhp`, loaded in-process by Rhino 9 WIP
- namespace: `Grasshopper2.Doc` — document graph, object list, connectivity, solution server
- namespace: `Grasshopper2.Doc` — `IAttributes` layout and draw contract; the concrete `Attributes<T>`/`ComponentAttributes`/`ResizableAttributes<T>` bases are `api-gh2-components.md`'s
- namespace: `Grasshopper2.Parameters` — wire mutation, parameter and pin endpoints
- namespace: `Grasshopper2.Undo` / `Grasshopper2.Undo.Actions` — undo history and action records
- namespace: `Grasshopper2.Framework` / `GrasshopperIO` — snippet and `IWriter`/`IStorable` persistence seam
- rail: gh2-document-graph

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: document, mutation verbs, object list

| [INDEX] | [SYMBOL]          | [KIND] | [CAPABILITY]                                                                           |
| :-----: | :---------------- | :----- | :------------------------------------------------------------------------------------- |
|  [01]   | `Document`        | class  | the graph root over objects, methods, undo, solution, persistence, and state           |
|  [02]   | `DocumentMethods` | class  | the mutation verb surface — clipboard, group, delete, visibility, drop, split, migrate |
|  [03]   | `ObjectList`      | class  | the membership index — find, reach, groups, wires, bounds, window select, id remap     |
|  [04]   | `GroupObject`     | class  | member objects grouped under a name and colour family                                  |
|  [05]   | `WireEnds`        | struct | a `(Source, Target)` `Guid` pair naming one wire by its endpoint ids                   |

[PUBLIC_TYPE_SCOPE]: object identity, attributes, keyed values

| [INDEX] | [SYMBOL]              | [KIND]    | [CAPABILITY]                             |
| :-----: | :-------------------- | :-------- | :--------------------------------------- |
|  [01]   | `IDocumentObject`     | interface | identity, state, attributes, and compute |
|  [02]   | `IAttributes`         | interface | layout, paint, movement, and owner       |
|  [03]   | `KeyedValues`         | class     | typed `Get`, `Set`, and `Delete` storage |
|  [04]   | `IParameter` / `IPin` | interface | local and global connection endpoints    |

[PUBLIC_TYPE_SCOPE]: graph traversal and connection mutation

| [INDEX] | [SYMBOL]           | [KIND]       | [CAPABILITY]                                                                                |
| :-----: | :----------------- | :----------- | :------------------------------------------------------------------------------------------ |
|  [01]   | `Connectivity`     | class        | read-side traversal — immediate and transitive reach, linearity, causal sort, relay elision |
|  [02]   | `ConnectiveObject` | class        | a graph-node handle addressing an object or one of its parameters                           |
|  [03]   | `Connections`      | static class | write-side wire mutation under `Grasshopper2.Parameters`, each `ActionList`-recorded        |
|  [04]   | `GraphTopology`    | enum         | subset topology verdict — `Empty`, `Singleton`, `Convex`, `Disjoint`, `Concave`             |

[PUBLIC_TYPE_SCOPE]: solution execution and undo history

| [INDEX] | [SYMBOL]                       | [KIND]       | [CAPABILITY]                                                                   |
| :-----: | :----------------------------- | :----------- | :----------------------------------------------------------------------------- |
|  [01]   | `SolutionServer`               | class        | the execution controller — start, stop, delayed expiry, and solution lifecycle |
|  [02]   | `Solution`                     | sealed class | one in-flight run — id, phase, mode, counters, cooperative cancellation        |
|  [03]   | `SolutionRecord`               | class        | a completed run — id, culmination phase, and the start/end window              |
|  [04]   | `SolutionId` / `SolutionPhase` | type         | the run identity and the phase vocabulary both run views carry                 |
|  [05]   | `ServerState`                  | enum         | the server-wide posture beside any one run's phase                             |
|  [06]   | `History`                      | class        | undo as a `Node` tree — do, undo, redo, and branch navigation                  |
|  [07]   | `ActionList`                   | class        | the mutation-action buffer a verb fills, sealed into a `Record` by `VerbNoun`  |
|  [08]   | `Node` / `Record`              | class        | an undo-tree node and the replayable action record it carries                  |
|  [09]   | `VerbNoun`                     | struct       | the verb-plus-noun label naming one undoable act                               |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: document lifecycle, persistence, state

[Document facets]: `File : FileUtility` `Display : DocumentDisplay` `Dependencies : DocumentDependencies` `Notes : string` `Hash : Guid` `Identity : Guid` `NamedViews : NamedViews` `Globals : GlobalServer` `CustomValues : KeyedValues` `Projection : (PointF centre, float zoom)` `IsEmpty : bool`

| [INDEX] | [SURFACE]                                            | [SHAPE]                     | [CAPABILITY]                                 |
| :-----: | :--------------------------------------------------- | :-------------------------- | :------------------------------------------- |
|  [01]   | `Document.New*Document`                              | static → `Document`         | mint at inert, inactive, or active tier      |
|  [02]   | `Document.AllDocuments`                              | static property             | the live open-document roster                |
|  [03]   | `Document.Store`                                     | `(IWriter[, FileContents])` | serialize through the `GrasshopperIO` writer |
|  [04]   | `Document.Close`                                     | `()`                        | tear down and release objects                |
|  [05]   | `Document.Objects` / `Methods` / `Undo` / `Solution` | property                    | object list, verbs, undo, solution server    |
|  [06]   | `Document.State` / `Parent`                          | property                    | `DocumentState`, `IDocumentParent`           |
|  [07]   | `Document.Modified` / `Modifications`                | property                    | `bool` deriving from an `int` count          |
|  [08]   | `Document.Modify` / `Unmodify`                       | `()`                        | raise and clear the modified flag            |

- `Parent` is `null` on a root document, so the read projects at the boundary; `Identity` is the runtime id that survives no save, while `Hash` is the content identity.

[ENTRYPOINT_SCOPE]: mutation verbs (`DocumentMethods`)

[Whole-graph selection]: `SelectAll` `DeselectAll` `InvertSelection` `ShiftSelection(bool upstream)` `GrowSelection(bool upstream, bool downstream)` `MoveSelection(int, int)` — each `-> int`, the touched count
[Explicit-set twins]: six posture verbs twin — `EnableObjects` `DisableObjects` `ShowObjects` `HideObjects` `ToggleDisplayObjects` `SetColourOverrideObjects(IDocumentObject[], Colour, …)` — beside `GroupObjects(IDocumentObject[], string?, Family?, …)` / `ChainObjects` / `ClusterObjects` / `DeleteObjects` / `DeleteObjectData`; the four pin-side reveals (`Show`/`Hide` + `SelectedInputs`/`SelectedOutputs`) and `CopySelection`/`CutSelection` carry NO explicit-set peer
[Preflight]: `CanCreateChain` / `CanCreateCluster(IEnumerable<IDocumentObject>, out string whyNot) -> bool`
[Verb tail]: every row below closes on a trailing `ActionList actions = null`; `CopySelection` alone omits it
[Posture verbs]: `Enable` `Disable` `Show` `Hide` `ToggleDisplay` + `Selected`, and `Show`/`Hide` + `SelectedInputs`/`SelectedOutputs`

| [INDEX] | [SURFACE]                                  | [SHAPE]                                             | [RETURNS]                    |
| :-----: | :----------------------------------------- | :-------------------------------------------------- | :--------------------------- |
|  [01]   | `DropObject` / `DropSnippet`               | `(IDocumentObject\|Guid\|Snippet, PointF)`          | `bool` changed               |
|  [02]   | `DeleteSelection` / `DeleteObjects`        | `([IDocumentObject[], WireEnds[]])`                 | `int` deleted                |
|  [03]   | `DeleteSelectionData` / `DeleteObjectData` | `([IDocumentObject[]])`                             | `int` cleared                |
|  [04]   | `CopySelection`                            | `(ClipboardKind)` — no tail                         | `bool` changed               |
|  [05]   | `CutSelection` / `PasteFromClipboard`      | `(ClipboardKind[, PasteBehaviour])`                 | `bool` changed               |
|  [06]   | `PasteGrasshopper1XmlFromClipboard`        | `()`                                                | `bool` changed               |
|  [07]   | `GroupSelection`                           | `(string?, OpenColor.Family?)`                      | `GroupObject`                |
|  [08]   | `ChainSelection` / `ClusterSelection`      | `()`                                                | `Chain` / `IDocumentObject`  |
|  [09]   | the nine posture verbs above               | `()`                                                | `int` touched                |
|  [10]   | `SetColourOverrideSelected`                | `(Colour?)`                                         | `int` touched                |
|  [11]   | `IsolateObject`                            | `(IDocumentObject, bool ×3)` + `(…, HashSet<Guid>)` | `void`                       |
|  [12]   | `SplitWire`                                | `(IParameter ×2, string, PointF, out ×2)`           | `bool`; out `Shout`/`Listen` |
|  [13]   | `MigrateObjects`                           | `(IEnumerable<IDocumentObject>, PointF)`            | `Dictionary<Guid,Guid>`      |
|  [14]   | `AddDependency` / `ShowDependencyGraph`    | `(PointF)` / `()`                                   | `Listen` / `void`            |
|  [15]   | `MakeRoom`                                 | `(RectangleF before, RectangleF after)`             | `void`                       |

- Every mutating verb ANSWERS: a touched `int`, a changed `bool`, the wrapper it minted, or an id map. Discarding that return publishes a settled act no producer measured, and the count is the only evidence that a selection verb reached anything.
- `IsolateObject`'s three flags are `pins`, `inputs`, `outputs` — never an upstream/downstream/remainder reach — and they forward positionally, so a consumer vocabulary names the axis and the order once.
- `CopySelection` alone takes no `ActionList`: copying mutates nothing, so it seals no undo record.

[ENTRYPOINT_SCOPE]: object list, traversal, connection mutation

[ObjectList projections]: `Forwards` `Backwards` `Groups` `ActiveObjects` `ExpiredObjects` `AllWires` `SelectedWires` `Pins` `SupportedPins` `AttributeBounds` `PivotBounds` `Connectivity`

[ObjectList grips]: `FindByInlet` / `FindByOutlet(PointF) -> IParameter`; `FindByInletOrOutlet(PointF) -> (IParameter, bool inletWithinRange, bool outletWithinRange)`; all `null` on a miss
[Repair report]: `RepairPins` rows are `(PinRepair method, Guid pin, Guid cushion)`; `FindNear<T>` constrains `where T : IDocumentObject`

| [INDEX] | [SURFACE]                                         | [SHAPE]                                     | [RETURNS]                              |
| :-----: | :------------------------------------------------ | :------------------------------------------ | :------------------------------------- |
|  [01]   | `ObjectList.Find` / `FindParameter`               | `(Guid)`                                    | `IDocumentObject` / `IParameter`       |
|  [02]   | `ObjectList.SearchUpstream` / `SearchDownstream`  | `(IParameter)`                              | `IEnumerable<IDocumentObject>`         |
|  [03]   | `ObjectList.WindowSelect`                         | `(WindowSelection, SelectionMode, bool ×3)` | `SelectionResult`                      |
|  [04]   | `ObjectList.ChangeAllIds` / `ApplyIdMap`          | `()` / `(Dictionary<Guid,Guid>)`            | `Dictionary<Guid,Guid>` / `void`       |
|  [05]   | `ObjectList.AddGlobalPin` / `ExpireAll`           | `(IPin)` / `()`                             | `bool` admitted / `void`               |
|  [06]   | `ObjectList.RepairPins`                           | `(PinRepair = Default)`                     | `(PinRepair, Guid, Guid)[]`            |
|  [07]   | `ObjectList.FindNear<T>`                          | `(PointF, int, float)`                      | `T[]` relevance-sorted                 |
|  [08]   | `ObjectList.Pins` / `SupportedPins`               | property                                    | `IEnumerable<IPin>` / `<Guid>`         |
|  [09]   | `ObjectList.AttributeBounds` / `PivotBounds`      | property                                    | `RectangleF` envelopes                 |
|  [10]   | `ObjectList.Connectivity`                         | property                                    | a fresh `Connectivity` snapshot        |
|  [11]   | `Connectivity.FindImmediate*` / `FindAll*`        | `(ConnectiveObject)`                        | immediate and transitive reach         |
|  [12]   | `Connectivity.FindConnections`                    | `(ConnectiveObject ×2)`                     | `IEnumerable<ConnectiveObject[]>`      |
|  [13]   | `Connectivity.IsLinear`                           | `(IEnumerable<Guid>\|node)`                 | `bool`; out `ConnectiveObject` ×2      |
|  [14]   | `Connectivity.SubsetTopology`                     | `(Guid\|IDocumentObject)`                   | `GraphTopology` CLASS, not a view      |
|  [15]   | `Connectivity.SortCausally`                       | `(ConnectiveObject[])`                      | `ConnectiveObject[]` in order          |
|  [16]   | `Connectivity.WithoutRelays(bool ×3)`             | instance                                    | `Connectivity` relay-elided view       |
|  [17]   | `Connections.Connect` / `Disconnect`              | `(IParameter×2, …)`                         | `bool` — add or remove one wire        |
|  [18]   | `Connections.DisconnectAll*Except`                | `(IParameter, Guid\|HashSet, …)`            | `int` — prune one side but a kept set  |
|  [19]   | `Connections.DisconnectAllInputs` / `Outputs`     | `(IParameter, …)`                           | `int` — the bare full-side clear       |
|  [20]   | `Connections.ReplaceSource` / `ReplaceTarget`     | `(IParameter×3, …)`                         | `bool` — re-point a wire endpoint      |
|  [21]   | `Connections.SwapSources`                         | `(sourceA, sourceB, targetA, targetB, …)`   | `bool` — exchange two targets' sources |
|  [22]   | `Connections.CutOutMiddleMan`                     | `(IParameter×3, …)`                         | `bool` — bypass an intermediate        |
|  [23]   | `Connections.CopyAllInputs` / `MigrateAllOutputs` | `(IParameter×2, …)`                         | `int` — duplicate or move a wire set   |

- The spatial finders and the near search live on `ObjectList`, never on `Connectivity` — the connection snapshot carries no coordinate, and `ObjectList.Connectivity` mints a fresh one per read.
- The two replace verbs order their parameters DIFFERENTLY — `ReplaceSource(oldSource, newSource, target, undo)` against `ReplaceTarget(source, oldTarget, newTarget, undo)` — so call sites bind by name or silently re-point the wrong end.
- `History.FindCommonAncestor`/`FindShortestPath` are nonpublic — branch reconciliation walks the public `Node` topology (`Parent`/`ParentIfNotRoot`/`Depth`/`PrimaryChild`/`SecondaryChildren`).
- Event-args families are public typed wires: `Grasshopper2.Doc` publishes `DocumentModifiedEventArgs`/`DocumentStateEventArgs`/`AfterAddObjectEventArgs`/`AfterRemoveObjectEventArgs`/`ObjectEventArgs`/`ObjectNameEventArgs`/`ObjectGuidEventArgs`/`SolutionIdEventArgs`/`SolutionEventArgs`/`SolutionExceptionEventArgs`, `Grasshopper2.Undo` publishes `UndoEventArgs`/`UndoNodeEventArgs`/`UndoNodeMovedEventArgs`, and the generic `Grasshopper2.BeforeAfterEventArgs<TValue, TOwner>` carries the parent swap.
- `FindByInlet`/`FindByOutlet` answer the closest grip EVEN WHERE OCCLUDED; `FindByInletOrOutlet` refuses an occluded grip and reports which side fell within range. All three return `null` on a miss.
- `FindNear<T>` filters by `T` inside its bounded `(maxResults, maxDistance)` search, so post-filtering an `IDocumentObject` result returns fewer rows than requested whenever a nearer foreign object consumed a slot.
- `ObjectList.Transfer` is `private` on both its `IDocumentObject` and `IPin` overloads: there is no public cross-document pull, and a consumer reaches one through the clipboard round-trip or `MigrateObjects`.
- `WindowSelect(window, mode, considerForeground = true, considerBackground = true, considerWires = true)` INCLUDES each band on true; its `SelectionResult` is a mutable pick accumulator, not a value.
- `AttributeBounds` unions the laid-out attribute bounds and the wires joining them may exceed it; `PivotBounds` grows over pivots alone, needing no layout pass — quicker and less accurate.
- `SubsetTopology` MEASURES rather than projects: it answers `GraphTopology.{Empty, Singleton, Convex, Disjoint, Concave}` for the subset. Reading it as a `Connectivity` view is the defect the name invites, and only `WithoutRelays` returns a view.
- `WithoutRelays(dangling, simple, complex)` REMOVES on true, keyed by relay arity — `dangling` has no inputs or no outputs, `simple` exactly one of each, `complex` two or more on a side. A consumer vocabulary spells the arity and the removal polarity; three positional bools carry neither.
- `FindConnections` yields one `ConnectiveObject[]` per causal PATH between the pair, never the wires joining them.

[ENTRYPOINT_SCOPE]: object identity, solution, undo

Solution events fire in the listed lifecycle order; document, object-list, and history events fire per mutation. Each event binds its family `EventArgs`.

[Solution events]: `SolutionAboutToStart` (`SolutionIdEventArgs`), `SolutionStarted` `SolutionStopped` `SolutionCancelled` `SolutionCompleted` (`SolutionEventArgs`), `SolutionFaulted` (`SolutionExceptionEventArgs`, adds `Exception`)
[Document events]: `ModifiedChanged` `StateChanged` (`DocumentEventArgs<T>`), `ParentChanged` (`BeforeAfterEventArgs<Document, IDocumentParent>`)
[ObjectList events]: `ObjectAdded` `ObjectRemoved` `ObjectNameChanged` `ObjectInstanceIdChanged`, and `ObjectSelectionChanged` `ObjectExpired` `ObjectEnabledChanged` `ObjectRelevanceChanged` `ObjectLayoutChanged` `ObjectDisplayChanged` (`ObjectEventArgs`)
[History events]: `Undone` `Redone` `Modified` (`UndoEventArgs`), `NodeAdded` `NodeRemoved` `NodeMerged` (`UndoNodeEventArgs`), `NodeMoved` (`UndoNodeMovedEventArgs`)

`PivotAction(IDocumentObject)` snapshots the pre-move pivot; its `Extends` relation folds consecutive nudges into one undo record.

[Solution start]: `Start` / `StartWait(CancellationTokenSource?, SolutionMode = Regular)`
[Live run state]: `Solution.Id : SolutionId` `Phase : SolutionPhase` `Mode : SolutionMode` `Token` `Time` `Age`; `ComputableCount` `InvalidParameters` `OverallProgress` are measured `int`s
[Completed run]: `SolutionRecord.SolutionId` `Culmination : SolutionPhase` `StartTime` `EndTime` `Duration`

| [INDEX] | [SURFACE]                                         | [SHAPE]      | [CAPABILITY]                         |
| :-----: | :------------------------------------------------ | :----------- | :----------------------------------- |
|  [01]   | `IDocumentObject.InstanceId` / `Nomen` / `State`  | properties   | identity and object state            |
|  [02]   | `IDocumentObject.Expire` / `Compute`              | operations   | expiry and evaluation                |
|  [03]   | `AddUndoRecord` / `RequestAutoSave`               | operations   | undo and autosave admission          |
|  [04]   | `IAttributes.Move` / `Layout` / `Draw`            | operations   | relocation, layout, paint            |
|  [05]   | `Undo.Actions.PivotAction`                        | constructor  | deduplicating pivot undo             |
|  [06]   | `KeyedValues.Get<T>` / `Set` / `Delete`           | keyed access | typed read, write, remove            |
|  [07]   | `SolutionServer.Start`                            | execution    | `Task<Solution>` on a pool worker    |
|  [08]   | `SolutionServer.StartWait`                        | execution    | `Solution`; deadlocks on the marshal |
|  [09]   | `SolutionServer.Stop` / `DelayedExpire` / `State` | execution    | halt, queue expiry, `ServerState`    |
|  [10]   | `Solution.Cancel` / `Cancelled` / `StateChanged`  | run control  | cooperative cancel and phase edge    |
|  [11]   | `History.Do` / `Undo` / `Redo`                    | history      | record and traverse                  |
|  [12]   | `FindCommonAncestor` / `FindShortestPath`         | history      | branch reconciliation                |
|  [13]   | `ActionList.ToRecord` / `Node.PromoteChild`       | history      | seal and rebranch                    |
|  [14]   | `Record.Undo` / `Record.Redo`                     | replay       | backward and forward application     |

- `IAttributes` publishes `Pivot` (settable), `Bounds`, `AggregateBounds`, `Owner`, `Snappable`, the three hit tests, `ShowTooltipAt`, `HandleDoubleClick`, `InvalidateLayout`, `InvalidateDisplay`, and `Draw(Context, Skin)`; a policy surface reading placement therefore takes the interface, never a concrete attributes base.
- `Start` settles the whole solve on a threadpool worker, so its `Task<Solution>` completes independently of the UI idle loop and a caller may block its OWN thread on it; `StartWait` does that block inside the host and deadlocks when the caller holds the UI thread.
- `SolutionServer.ExpireDelayedObjects` is `private`: the server drains its own deferred queue at run start, and there is no consumer-drivable flush.
- `SolutionRecord.ExpiredCount`, `SolvedCount`, and `Progress` are auto-properties its only constructor never assigns — every record reads them as zero, so they are a structural zero, not a measurement.

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- One `Document` is the single authority over its graph; `ObjectList` is the membership index and `Connectivity` the read view, and no object exists in the graph without an `ObjectList` entry.
- Every structural mutation carries an `ActionList`; `DocumentMethods` and `Grasshopper2.Parameters.Connections` fill it, and `History.Do`/`ActionList.ToRecord` seal it into the undo tree under one `VerbNoun`.
- Wire topology reads through `Grasshopper2.Doc.Connectivity` over `ConnectiveObject`s and writes through `Grasshopper2.Parameters.Connections` over `IParameter`s; a `WireEnds` names one wire by its source and target ids.
- Execution is `SolutionServer`-owned: `Start` opens a run, `Solution` carries phase and cancellation, the solution event family publishes lifecycle, and a `SolutionRecord` records culmination and counts.
- Undo is a branching `Node` tree, not a linear stack; `FindCommonAncestor`/`FindShortestPath` reconcile branches and `PromoteChild` re-roots one.

[STACKING]:
- `api-languageext`(`.api/api-languageext.md`): `Store` and document load fold onto `Fin<Document>`; `ObjectList.Find`/`FindParameter` and `KeyedValues.Get<T>` return `Option`; a `SolutionServer` run lowers to `Eff<SolutionRecord>` with `SolutionFaulted`/`SolutionCancelled` mapping to `Error`; the three grip finders and `Find`/`FindParameter` project their `null` miss to `Option`; `MigrateObjects` returns its id correspondence as a `HashMap<Guid, Guid>` and every `int`-returning verb its touched count on the settlement receipt; `ObjectList` projections carry as `Seq`/`HashMap` and `Solution.Phase` rides an `Atom` cell.
- `api-thinktecture-runtime-extensions`(`.api/api-thinktecture-runtime-extensions.md`): host discriminants — `SolutionMode`, `ClipboardKind`, `PasteBehaviour`, `AutoSaveReason`, `SelectionMode`, `PinRepair`, `VerbNoun`, `SolutionRecord.Culmination` — own `[SmartEnum]`/`[Union]` vocabularies so a mutation verb dispatches through exhaustive `Switch`, and a `WireEnds` endpoint pair is a `[ComplexValueObject]` with structural equality.

[LOCAL_ADMISSION]:
- `Rasm.Grasshopper` owns the document graph as its folder domain, composing the Rasm kernel for host-agnostic logic and referencing no sibling Rasm package.
- Every mutation enters the folder owner through one `ActionList`-carrying verb; a mutation without its undo record is not admitted.

[RAIL_LAW]:
- Package: `Grasshopper2` (document graph)
- Owns: the `Document`/`ObjectList`/`DocumentMethods` graph, `Connectivity` traversal, `Connections` wire mutation, `SolutionServer` execution, and `History` undo branching
- Accept: graph query, `ActionList`-recorded mutation, solution lifecycle control, and undo-tree navigation over document objects
- Reject: canvas paint and picking (`api-gh2-canvas`), component execution and pin typing (`api-gh2-components`), and canvas interaction and layout (`api-gh2-interaction`)

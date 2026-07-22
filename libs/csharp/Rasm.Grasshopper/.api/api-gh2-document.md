# [RASM_GRASSHOPPER_API_GH2_DOCUMENT]

`Grasshopper2` is the Rhino 9 WIP visual-programming host, and its `Document` is the single mutable authority over one canvas definition. Every structural change enters through `DocumentMethods` (or `Grasshopper2.Parameters.Connections` for wire mutation) paired with a `Grasshopper2.Undo.ActionList`, so a mutation and its undo record seal as one act. Graph traversal reads through `Grasshopper2.Doc.Connectivity` over `ConnectiveObject`s, and execution runs on `SolutionServer` publishing the solution lifecycle over `Solution`/`SolutionRecord` phase state.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Grasshopper2` document graph
- host: `Grasshopper2.dll` inside `Grasshopper2Plugin.rhp`, loaded in-process by Rhino 9 WIP
- namespace: `Grasshopper2.Doc` — document graph, object list, connectivity, solution server
- namespace: `Grasshopper2.Doc.Attributes` — `IAttributes` layout and draw contract
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
|  [02]   | `ConnectiveObject` | struct       | a graph-node handle addressing an object or one of its parameters                           |
|  [03]   | `Connections`      | static class | write-side wire mutation under `Grasshopper2.Parameters`, each `ActionList`-recorded        |

[PUBLIC_TYPE_SCOPE]: solution execution and undo history

| [INDEX] | [SYMBOL]          | [KIND] | [CAPABILITY]                                                                   |
| :-----: | :---------------- | :----- | :----------------------------------------------------------------------------- |
|  [01]   | `SolutionServer`  | class  | the execution controller — start, stop, delayed expiry, and solution lifecycle |
|  [02]   | `Solution`        | class  | one in-flight run — id, phase, invalid parameters, cooperative cancellation    |
|  [03]   | `SolutionRecord`  | class  | a completed run — culmination, expired/solved counts, progress                 |
|  [04]   | `History`         | class  | undo as a `Node` tree — do, undo, redo, and branch navigation                  |
|  [05]   | `ActionList`      | class  | the mutation-action buffer a verb fills, sealed into a `Record` by `VerbNoun`  |
|  [06]   | `Node` / `Record` | class  | an undo-tree node and the replayable action record it carries                  |
|  [07]   | `VerbNoun`        | struct | the verb-plus-noun label naming one undoable act                               |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: document lifecycle, persistence, state

[Document facets]: `File` `Display` `Dependencies` `Notes` `Hash` `NamedViews` `Globals` `CustomValues`

| [INDEX] | [SURFACE]                                            | [SHAPE]                   | [CAPABILITY]                                 |
| :-----: | :--------------------------------------------------- | :------------------------ | :------------------------------------------- |
|  [01]   | `Document.New*Document`                              | static → `Document`       | mint at the inert, inactive, or active tier  |
|  [02]   | `Document.AllDocuments`                              | static property           | the live open-document roster                |
|  [03]   | `Document.Store`                                     | `(IWriter, FileContents)` | serialize through the `GrasshopperIO` writer |
|  [04]   | `Document.Close`                                     | `()`                      | tear down and release objects                |
|  [05]   | `Document.Objects` / `Methods` / `Undo` / `Solution` | property                  | object list, verbs, undo, solution server    |
|  [06]   | `Document.Modified` / `State` / `Parent`             | property                  | dirty flag, state, parent for nested graphs  |
|  [07]   | `Document.Modify` / `Unmodify`                       | `()`                      | raise and clear the modified flag            |

[ENTRYPOINT_SCOPE]: mutation verbs (`DocumentMethods`)

[Whole-graph selection]: `SelectAll` `DeselectAll` `InvertSelection`

| [INDEX] | [SURFACE]                                                | [SHAPE]                        | [CAPABILITY]                          |
| :-----: | :------------------------------------------------------- | :----------------------------- | :------------------------------------ |
|  [01]   | `DropObject` / `DropSnippet`                             | `(object, PointF, …)`          | drop an object or snippet at a point  |
|  [02]   | `DeleteSelection` / `DeleteObjects`                      | `(…, WireEnds[], …)`           | delete selection or an explicit set   |
|  [03]   | `CopySelection` / `CutSelection` / `PasteFromClipboard`  | `(ClipboardKind, …)`           | clipboard round-trip                  |
|  [04]   | `PasteGrasshopper1XmlFromClipboard`                      | `(ActionList)`                 | ingest a legacy GH1 clipboard payload |
|  [05]   | `GroupSelection` / `ChainSelection` / `ClusterSelection` | `([string, Family?] …)`        | wrap into group, chain, or cluster    |
|  [06]   | `EnableSelected` / `DisableSelected`                     | `(ActionList)`                 | flip activity on the selection        |
|  [07]   | `ShowSelected` / `HideSelected`                          | `(ActionList)`                 | flip display on the selection         |
|  [08]   | `SetColourOverrideSelected`                              | `(Colour, …)`                  | apply a display colour override       |
|  [09]   | `IsolateObject`                                          | `(IDocumentObject, bool×3, …)` | isolate one object's reach            |
|  [10]   | `SplitWire`                                              | `(…, out Shout, out Listen)`   | split a wire into a wireless pair     |
|  [11]   | `MigrateObjects`                                         | `(IEnumerable<…>, PointF, …)`  | relocate a transferred object set     |
|  [12]   | `AddDependency` / `ShowDependencyGraph`                  | `(PointF, …)` / `()`           | add and reveal dependencies           |

[ENTRYPOINT_SCOPE]: object list, traversal, connection mutation

[ObjectList projections]: `Forwards` `Backwards` `Groups` `ActiveObjects` `ExpiredObjects` `AllWires` `SelectedWires` `AttributeBounds` `PivotBounds`

| [INDEX] | [SURFACE]                                                        | [SHAPE]                    | [CAPABILITY]                         |
| :-----: | :--------------------------------------------------------------- | :------------------------- | :----------------------------------- |
|  [01]   | `ObjectList.Find` / `FindParameter`                              | `(Guid)`                   | resolve an object or parameter by id |
|  [02]   | `ObjectList.SearchUpstream` / `SearchDownstream`                 | `(IParameter)`             | transitive parameter reach one way   |
|  [03]   | `ObjectList.WindowSelect`                                        | `(WindowSelection, …)`     | rectangle selection over the graph   |
|  [04]   | `ObjectList.Transfer` / `ChangeAllIds` / `ApplyIdMap`            | `(IDocumentObject)`        | cross-document pull and id remap     |
|  [05]   | `ObjectList.AddGlobalPin` / `RepairPins` / `ExpireAll`           | `(IPin)` / `(PinRepair)`   | pin membership, repair, expiry       |
|  [06]   | `Connectivity.FindImmediate*` / `FindAll*`                       | `(ConnectiveObject)`       | immediate and transitive reach       |
|  [07]   | `Connectivity.FindConnections` / `IsLinear`                      | `(ConnectiveObject, …)`    | edge lookup and chain detect         |
|  [08]   | `Connectivity.SubsetTopology` / `SortCausally` / `WithoutRelays` | `(ConnectiveObject[])`     | subgraph, order, relay-elided view   |
|  [09]   | `Connections.Connect` / `Disconnect`                             | `(IParameter×2, …)`        | add or remove one wire               |
|  [10]   | `Connections.DisconnectAll*Except`                               | `(IParameter, HashSet, …)` | prune one side but a kept set        |
|  [11]   | `Connections.ReplaceSource` / `ReplaceTarget`                    | `(IParameter×3, …)`        | re-point a wire endpoint             |
|  [12]   | `Connections.CutOutMiddleMan`                                    | `(IParameter×3, …)`        | bypass an intermediate parameter     |
|  [13]   | `Connections.CopyAllInputs` / `MigrateAllOutputs`                | `(IParameter×2, …)`        | duplicate or move a wire set         |

[ENTRYPOINT_SCOPE]: object identity, solution, undo

Solution events fire in the listed lifecycle order; document, object-list, and history events fire per mutation. Each event binds its family `EventArgs`.

[Solution events]: `SolutionAboutToStart` (`SolutionIdEventArgs`), `SolutionStarted` `SolutionStopped` `SolutionCancelled` `SolutionCompleted` (`SolutionEventArgs`), `SolutionFaulted` (`SolutionExceptionEventArgs`, adds `Exception`)
[Document events]: `ModifiedChanged` `StateChanged` (`DocumentEventArgs<T>`), `ParentChanged` (`BeforeAfterEventArgs<Document, IDocumentParent>`)
[ObjectList events]: `ObjectAdded` `ObjectRemoved` `ObjectNameChanged` `ObjectInstanceIdChanged`, and `ObjectSelectionChanged` `ObjectExpired` `ObjectEnabledChanged` `ObjectRelevanceChanged` `ObjectLayoutChanged` `ObjectDisplayChanged` (`ObjectEventArgs`)
[History events]: `Undone` `Redone` `Modified` (`UndoEventArgs`), `NodeAdded` `NodeRemoved` `NodeMerged` (`UndoNodeEventArgs`), `NodeMoved` (`UndoNodeMovedEventArgs`)

`PivotAction(IDocumentObject)` snapshots the pre-move pivot; its `Extends` relation folds consecutive nudges into one undo record.

| [INDEX] | [SURFACE]                                        | [SHAPE]      | [CAPABILITY]                     |
| :-----: | :----------------------------------------------- | :----------- | :------------------------------- |
|  [01]   | `IDocumentObject.InstanceId` / `Nomen` / `State` | properties   | identity and object state        |
|  [02]   | `IDocumentObject.Expire` / `Compute`             | operations   | expiry and evaluation            |
|  [03]   | `AddUndoRecord` / `RequestAutoSave`              | operations   | undo and autosave admission      |
|  [04]   | `IAttributes.Move` / `Layout` / `Draw`           | operations   | relocation, layout, paint        |
|  [05]   | `Undo.Actions.PivotAction`                       | constructor  | deduplicating pivot undo         |
|  [06]   | `KeyedValues.Get<T>` / `Set` / `Delete`          | keyed access | typed read, write, remove        |
|  [07]   | `SolutionServer.Start` / `StartWait` / `Stop`    | execution    | begin, await, halt               |
|  [08]   | `DelayedExpire` / `ExpireDelayedObjects`         | execution    | queue and flush expiry           |
|  [09]   | `Solution.Cancel` / `Id` / `Phase`               | run state    | cancellation and inspection      |
|  [10]   | `History.Do` / `Undo` / `Redo`                   | history      | record and traverse              |
|  [11]   | `FindCommonAncestor` / `FindShortestPath`        | history      | branch reconciliation            |
|  [12]   | `ActionList.ToRecord` / `Node.PromoteChild`      | history      | seal and rebranch                |
|  [13]   | `Record.Undo` / `Record.Redo`                    | replay       | backward and forward application |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- One `Document` is the single authority over its graph; `ObjectList` is the membership index and `Connectivity` the read view, and no object exists in the graph without an `ObjectList` entry.
- Every structural mutation carries an `ActionList`; `DocumentMethods` and `Grasshopper2.Parameters.Connections` fill it, and `History.Do`/`ActionList.ToRecord` seal it into the undo tree under one `VerbNoun`.
- Wire topology reads through `Grasshopper2.Doc.Connectivity` over `ConnectiveObject`s and writes through `Grasshopper2.Parameters.Connections` over `IParameter`s; a `WireEnds` names one wire by its source and target ids.
- Execution is `SolutionServer`-owned: `Start` opens a run, `Solution` carries phase and cancellation, the solution event family publishes lifecycle, and a `SolutionRecord` records culmination and counts.
- Undo is a branching `Node` tree, not a linear stack; `FindCommonAncestor`/`FindShortestPath` reconcile branches and `PromoteChild` re-roots one.

[STACKING]:
- `api-languageext`(`.api/api-languageext.md`): `Store` and document load fold onto `Fin<Document>`; `ObjectList.Find`/`FindParameter` and `KeyedValues.Get<T>` return `Option`; a `SolutionServer` run lowers to `Eff<SolutionRecord>` with `SolutionFaulted`/`SolutionCancelled` mapping to `Error`; `MigrateObjects` and clipboard paste accumulate faults through `Validation<Error, Seq<IDocumentObject>>`; `ObjectList` projections carry as `Seq`/`HashMap` and `Solution.Phase`/`SolutionRecord.Progress` ride an `Atom` cell.
- `api-thinktecture-runtime-extensions`(`.api/api-thinktecture-runtime-extensions.md`): host discriminants — `SolutionMode`, `ClipboardKind`, `PasteBehaviour`, `AutoSaveReason`, `SelectionMode`, `PinRepair`, `VerbNoun`, `SolutionRecord.Culmination` — own `[SmartEnum]`/`[Union]` vocabularies so a mutation verb dispatches through exhaustive `Switch`, and a `WireEnds` endpoint pair is a `[ComplexValueObject]` with structural equality.

[LOCAL_ADMISSION]:
- `Rasm.Grasshopper` owns the document graph as its folder domain, composing the Rasm kernel for host-agnostic logic and referencing no sibling Rasm package.
- Every mutation enters the folder owner through one `ActionList`-carrying verb; a mutation without its undo record is not admitted.

[RAIL_LAW]:
- Package: `Grasshopper2` (document graph)
- Owns: the `Document`/`ObjectList`/`DocumentMethods` graph, `Connectivity` traversal, `Connections` wire mutation, `SolutionServer` execution, and `History` undo branching
- Accept: graph query, `ActionList`-recorded mutation, solution lifecycle control, and undo-tree navigation over document objects
- Reject: canvas paint and picking (`api-gh2-canvas`), component execution and pin typing (`api-gh2-components`), and canvas interaction and layout (`api-gh2-interaction`)

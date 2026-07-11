# [RASM_GRASSHOPPER_API_GH2_DOCUMENT]

`Grasshopper2` is the Rhino 9 WIP visual-programming host, and its document graph is the single mutable authority over the canvas definition — a `Document` owns an `ObjectList`, the `DocumentMethods` verb surface, `History` undo branching, a `SolutionServer`, persistence, display, dependencies, notes, hashes, custom values, and modification state. Every structural change enters through `DocumentMethods` (or `Grasshopper2.Parameters.Connections` for wire mutation) paired with a `Grasshopper2.Undo.ActionList`, so a mutation and its undo record are one act. Graph traversal is `Grasshopper2.Doc.Connectivity` over `ConnectiveObject`s, and execution is `SolutionServer` publishing the solution event family over `Solution`/`SolutionRecord` phase state.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Grasshopper2` document graph
- host: `Grasshopper2.dll` inside `Grasshopper2Plugin.rhp`, loaded in-process by Rhino 9 WIP
- namespace: `Grasshopper2.Doc` — `Document`, `DocumentMethods`, `ObjectList`, `Connectivity`, `IDocumentObject`, `SolutionServer`, `Solution`, `WireEnds`, `KeyedValues`
- namespace: `Grasshopper2.Doc.Attributes` — `IAttributes` layout and draw contract
- namespace: `Grasshopper2.Parameters` — `Connections` wire mutation, `IParameter`, `IPin`, `PinRepair`
- namespace: `Grasshopper2.Undo` / `Grasshopper2.Undo.Actions` — `History`, `ActionList`, `Node`, `Record`, `VerbNoun`
- namespace: `Grasshopper2.Framework` — `Snippet`; `GrasshopperIO` — `IWriter`/`IStorable` persistence seam
- rail: gh2-document-graph

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: document, mutation verbs, object list
- rail: gh2-document-graph

| [INDEX] | [SYMBOL]          | [KIND] | [CAPABILITY]                                                                           |
| :-----: | :---------------- | :----- | :------------------------------------------------------------------------------------- |
|  [01]   | `Document`        | class  | the graph root over objects, methods, undo, solution, persistence, and state           |
|  [02]   | `DocumentMethods` | class  | the mutation verb surface — clipboard, group, delete, visibility, drop, split, migrate |
|  [03]   | `ObjectList`      | class  | the membership index — find, reach, groups, wires, bounds, window select, id remap     |
|  [04]   | `GroupObject`     | class  | member objects grouped under a name and colour family                                  |
|  [05]   | `WireEnds`        | struct | a `(Source, Target)` `Guid` pair naming one wire by its endpoint ids                   |

[PUBLIC_TYPE_SCOPE]: object identity, attributes, keyed values
- rail: gh2-document-graph

| [INDEX] | [SYMBOL]              | [KIND]    | [CAPABILITY]                                                                          |
| :-----: | :-------------------- | :-------- | :------------------------------------------------------------------------------------ |
|  [01]   | `IDocumentObject`     | interface | identity, activity, selection, display, phase, state, attributes, expiry, and compute |
|  [02]   | `IAttributes`         | interface | the layout and paint contract — pivot, bounds, snappable, move, layout, draw          |
|  [03]   | `KeyedValues`         | class     | typed keyed storage over `IStorable`; `Get<T>` presence, `Set`, `Delete`              |
|  [04]   | `IParameter` / `IPin` | interface | a component pin and a global pin, the endpoints connection mutation binds             |

[PUBLIC_TYPE_SCOPE]: graph traversal and connection mutation
- rail: gh2-document-graph

| [INDEX] | [SYMBOL]           | [KIND]       | [CAPABILITY]                                                                                |
| :-----: | :----------------- | :----------- | :------------------------------------------------------------------------------------------ |
|  [01]   | `Connectivity`     | class        | read-side traversal — immediate and transitive reach, linearity, causal sort, relay elision |
|  [02]   | `ConnectiveObject` | struct       | a graph-node handle addressing an object or one of its parameters                           |
|  [03]   | `Connections`      | static class | write-side wire mutation under `Grasshopper2.Parameters`, each `ActionList`-recorded        |

[PUBLIC_TYPE_SCOPE]: solution execution and undo history
- rail: gh2-document-graph

| [INDEX] | [SYMBOL]          | [KIND] | [CAPABILITY]                                                                        |
| :-----: | :---------------- | :----- | :---------------------------------------------------------------------------------- |
|  [01]   | `SolutionServer`  | class  | the execution controller — start, stop, delayed expiry, and the six-event lifecycle |
|  [02]   | `Solution`        | class  | one in-flight run — id, phase, invalid parameters, cooperative cancellation         |
|  [03]   | `SolutionRecord`  | class  | a completed run — culmination, expired/solved counts, progress                      |
|  [04]   | `History`         | class  | undo as a `Node` tree — do, undo, redo, and branch navigation                       |
|  [05]   | `ActionList`      | class  | the mutation-action buffer a verb fills, sealed into a `Record` by `VerbNoun`       |
|  [06]   | `Node` / `Record` | class  | an undo-tree node and the replayable action record it carries                       |
|  [07]   | `VerbNoun`        | struct | the verb-plus-noun label naming one undoable act                                    |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: document lifecycle, persistence, state
- rail: gh2-document-graph

New documents are minted through `NewInertDocument`, `NewInactiveDocument`, and `NewActiveDocument`; facets read through `File`, `Display`, `Dependencies`, `Notes`, `Hash`, `NamedViews`, `Globals`, and `CustomValues`.

| [INDEX] | [SURFACE]                                            | [CALL_SHAPE]              | [CAPABILITY]                                 |
| :-----: | :--------------------------------------------------- | :------------------------ | :------------------------------------------- |
|  [01]   | `Document.New*Document`                              | static → `Document`       | mint at the inert, inactive, or active tier  |
|  [02]   | `Document.AllDocuments`                              | static property           | the live open-document roster                |
|  [03]   | `Document.Store`                                     | `(IWriter, FileContents)` | serialize through the `GrasshopperIO` writer |
|  [04]   | `Document.Close`                                     | `()`                      | tear down and release objects                |
|  [05]   | `Document.Objects` / `Methods` / `Undo` / `Solution` | property                  | object list, verbs, undo, solution server    |
|  [06]   | `Document.Modified` / `State` / `Parent`             | property                  | dirty flag, state, parent for nested graphs  |
|  [07]   | `Document.Modify` / `Unmodify`                       | `()`                      | raise and clear the modified flag            |

[ENTRYPOINT_SCOPE]: mutation verbs (`DocumentMethods`)
- rail: gh2-document-graph

Whole-graph selection state is `SelectAll`, `DeselectAll`, and `InvertSelection`; each verb below takes a trailing `ActionList`.

| [INDEX] | [SURFACE]                                                | [CALL_SHAPE]                   | [CAPABILITY]                          |
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
- rail: gh2-document-graph

`ObjectList` projects the graph through `Forwards`, `Backwards`, `Groups`, `ActiveObjects`, `ExpiredObjects`, `AllWires`, `SelectedWires`, `AttributeBounds`, and `PivotBounds`. Neighbour reach is `FindImmediateInputs`/`FindImmediateOutputs` and `FindAllInputs`/`FindAllOutputs`.

| [INDEX] | [SURFACE]                                                        | [CALL_SHAPE]               | [CAPABILITY]                         |
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
- rail: gh2-document-graph

The solution lifecycle events are `SolutionAboutToStart` (`SolutionIdEventArgs`: `Document`/`Id`), `SolutionStarted`, `SolutionStopped`, `SolutionCancelled`, `SolutionCompleted` (all `SolutionEventArgs`: `Solution`/`SolutionId`/`Document`), and `SolutionFaulted` (`SolutionExceptionEventArgs` adds `Exception`), fired in that order; a `SolutionRecord` carries `Culmination`, `ExpiredCount`, `SolvedCount`, and `Progress`. `Document` publishes `ModifiedChanged` (`DocumentModifiedEventArgs`), `StateChanged` (`DocumentStateEventArgs` — both `DocumentEventArgs<T>` with `Document`/`Oldstate`/`NewState`), and `ParentChanged` (`BeforeAfterEventArgs<Document, IDocumentParent>` with `Owner`/`Old`/`New`). `ObjectList` publishes ten events: `ObjectAdded` (`AfterAddObjectEventArgs`), `ObjectRemoved` (`AfterRemoveObjectEventArgs`), `ObjectNameChanged` (`ObjectNameEventArgs`), `ObjectInstanceIdChanged` (`ObjectGuidEventArgs` with `OldId`/`NewId`), and `ObjectSelectionChanged`/`ObjectExpired`/`ObjectEnabledChanged`/`ObjectRelevanceChanged`/`ObjectLayoutChanged`/`ObjectDisplayChanged` (all `ObjectEventArgs` with `Object`). `History` publishes seven: `Undone`/`Redone`/`Modified` (`UndoEventArgs`), `NodeAdded`/`NodeRemoved`/`NodeMerged` (`UndoNodeEventArgs`), and `NodeMoved` (`UndoNodeMovedEventArgs` with `Nodes`/`OldParent`/`NewParent`).

| [INDEX] | [SURFACE]                                                | [CALL_SHAPE]                    | [CAPABILITY]                        |
| :-----: | :------------------------------------------------------- | :------------------------------ | :---------------------------------- |
|  [01]   | `IDocumentObject.InstanceId` / `Nomen` / `State`         | property                        | identity and per-object state       |
|  [02]   | `IDocumentObject.Expire` / `Compute`                     | `()` / `(Solution, …)`          | expire and evaluate one object      |
|  [03]   | `IDocumentObject.AddUndoRecord` / `RequestAutoSave`      | `(VerbNoun, Action)`            | attach undo and request autosave    |
|  [04]   | `IAttributes.Move` / `Layout` / `Draw`                   | `(float×2)` / `(Context, Skin)` | relocate, lay out, and paint        |
|  [05]   | `KeyedValues.Get<T>` / `Set` / `Delete`                  | `(string, T)`                   | typed keyed read, write, remove     |
|  [06]   | `SolutionServer.Start` / `StartWait` / `Stop`            | `([Cts, ]SolutionMode)`         | begin, await, or halt a run         |
|  [07]   | `SolutionServer.DelayedExpire` / `ExpireDelayedObjects`  | `(IDocumentObject)` / `()`      | queue and flush deferred expiry     |
|  [08]   | `Solution.Cancel` / `Id` / `Phase` / `InvalidParameters` | `()` / property                 | cancellation and run inspection     |
|  [09]   | `History.Do` / `Undo` / `Redo`                           | `(VerbNoun, ActionList)`        | record a mutation and step the tree |
|  [10]   | `History.FindCommonAncestor` / `FindShortestPath`        | `(Node, Node)`                  | branch reconciliation               |
|  [11]   | `ActionList.ToRecord` / `Node.PromoteChild`              | `(VerbNoun)` / `(Node)`         | seal actions and re-branch          |
|  [12]   | `Record.Undo` / `Record.Redo`                            | `(Document)`                    | replay a record forward or back     |

## [04]-[IMPLEMENTATION_LAW]

[GH2_DOCUMENT_TOPOLOGY]:
- One `Document` is the single authority over its graph; the `ObjectList` is the membership index and `Connectivity` the read view, and no object exists in the graph without an `ObjectList` entry.
- Every structural mutation carries an `ActionList`; `DocumentMethods` and `Grasshopper2.Parameters.Connections` fill it, and `History.Do`/`ActionList.ToRecord` seal it into the undo tree under one `VerbNoun`, so mutation and undo are inseparable.
- Wire topology reads through `Grasshopper2.Doc.Connectivity` over `ConnectiveObject`s and writes through `Grasshopper2.Parameters.Connections` over `IParameter`s; a `WireEnds` names one wire by its source and target ids.
- Execution is `SolutionServer`-owned: `Start` opens a run, `Solution` carries its phase and cancellation, the six-event family publishes lifecycle, and a `SolutionRecord` records culmination and counts.
- Undo is a branching `Node` tree, not a linear stack; `FindCommonAncestor`/`FindShortestPath` reconcile branches and `PromoteChild` re-roots one.

[STACKING]:
- `api-languageext`: document load and `Store` fold onto `Fin<Document>`; `ObjectList.Find`/`FindParameter` and `KeyedValues.Get<T>` become `Option`-returning lookups; a `SolutionServer` run lowers to `Eff<SolutionRecord>` whose `SolutionFaulted`/`SolutionCancelled` map to `Error`; `MigrateObjects` and clipboard paste accumulate per-object faults through `Validation<Error, Seq<IDocumentObject>>`; `ObjectList` projections carry as `Seq`/`HashMap`, and `Solution.Phase`/`SolutionRecord.Progress` ride an `Atom` cell.
- `api-thinktecture-runtime-extensions`: the host discriminants — `SolutionMode`, `ClipboardKind`, `PasteBehaviour`, `AutoSaveReason`, `SelectionMode`, `PinRepair`, undo `VerbNoun`, and `SolutionRecord.Culmination` — are owned as `[SmartEnum]`/`[Union]` vocabularies so a mutation verb dispatches through exhaustive `Switch`, and a `WireEnds` endpoint pair is a `[ComplexValueObject]` with structural equality.

[LOCAL_ADMISSION]:
- The document graph is the Rasm.Grasshopper folder's own domain; it composes the Rasm kernel for host-agnostic logic and never references a sibling Rasm package.
- Every mutation enters the folder's owner through one `ActionList`-carrying verb; a mutation without its undo record is not admitted.

[RAIL_LAW]:
- Package: `Grasshopper2` (document graph)
- Owns: the `Document`/`ObjectList`/`DocumentMethods` graph, `Connectivity` traversal, `Connections` wire mutation, `SolutionServer` execution, and `History` undo branching
- Accept: graph query, `ActionList`-recorded mutation, solution lifecycle control, and undo-tree navigation over document objects
- Reject: canvas paint and picking (`api-gh2-canvas`), component execution and pin typing (`api-gh2-components`), and canvas interaction and layout (`api-gh2-interaction`)

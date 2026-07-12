# [RASM_APPUI_API_NODEEDITOR]

`NodeEditorAvalonia` is the node/pin/connector graph-editing canvas realizing `Editing/graph.md` (`[V8]`c — the promised parametric/dependency-graph canvas on ReactiveUI, with `LoroTree` the co-edit data seam under one echo-suppressed bidirectional projection, and the notebook `DependencyGraph` as a read projection), split into two assemblies: `NodeEditorAvalonia.Model` (assembly `NodeEditorAvalonia.Model.dll`, namespace `NodeEditor.Model`) is the FRAMEWORK-AGNOSTIC graph core — the `IDrawingNode`/`INode`/`IConnector`/`IPin` contracts, the headless `DrawingNodeEditor` engine (clipboard, align/distribute/order, connection-validation), `INodeSerializer` round-trip, `IDrawingNodeSettings`, and the full typed event family; `NodeEditorAvalonia` (assembly `NodeEditorAvalonia.dll`, namespace `NodeEditor.*`) is the Avalonia layer — the `Editor`/`DrawingNode`/`Node`/`Pin`/`Connector` controls, the `Xaml.Behaviors`-driven interaction behaviors, `NodeZoomBorder` (a `PanAndZoom` `ZoomBorder` subclass), the `OrthogonalRouter` + `RTree` spatial index, the ink layer, and the `ExportRenderer`. Product code implements `IDrawingNode`/`INode` view-models on the admitted `ReactiveUI` rail (`api-reactiveui-avalonia.md`) — the `CommunityToolkit.Mvvm` variant is NOT admitted — and the `ExportRenderer` PNG/SVG/PDF/XPS paths converge with the drafting export set (`api-drafting-export.md`, `api-pdfsharp.md`).

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `NodeEditorAvalonia.Model`
- package: `NodeEditorAvalonia.Model`
- license: MIT
- floor: `net10.0` consumer (`lib/net10.0/NodeEditorAvalonia.Model.dll`); multi-targets net8.0 / net10.0, `net10.0` bound
- assembly: `NodeEditorAvalonia.Model`
- namespace: `NodeEditor.Model` — the framework-agnostic graph contracts, the `DrawingNodeEditor` engine, and the typed `{Node,Pin,Connector}*EventArgs` family
- rail: graph-editing

[PACKAGE_SURFACE]: `NodeEditorAvalonia`
- package: `NodeEditorAvalonia`
- license: MIT
- floor: `net10.0` consumer (`lib/net10.0/NodeEditorAvalonia.dll`); multi-targets net8.0 / net10.0, `net10.0` bound
- assembly: `NodeEditorAvalonia`
- namespace: `NodeEditor.Controls` (`Editor`, `DrawingNode`, `Node`, `Pin`, `Connector`, `NodeZoomBorder`, `Toolbox`, ink + adorner controls), `NodeEditor.Behaviors` (drag/connect/select/resize/rotate interaction behaviors over `Xaml.Behaviors`), `NodeEditor.Converters`, `NodeEditor.Services` (`StorageService`, `ExportRenderer`), plus `OrthogonalRouter`/`RTree`/`HitTestIndex`/`SnapHelper`/`ConnectorPathHelper` graph utilities
- depends: `NodeEditorAvalonia.Model`, `Avalonia` (12.x), `Avalonia.Controls.PanAndZoom` (the pre-rename id — a DISTINCT assembly from the admitted `PanAndZoom.dll`, no collision since this project compiles no consumer source) + `Avalonia.Xaml.Behaviors`; the consumer-facing ReactiveUI view-model layer is supplied by product code, not this package
- health: `12.0.0` is the newest `NodeEditorAvalonia` line; its deprecated Avalonia behavior/PanAndZoom transitive ids remain upstream-owned until that package republishes on the renamed ids.
- rail: graph-editing

## [02]-[PUBLIC_TYPES]

[GRAPH_CONTRACTS]: framework-agnostic model graph — `NodeEditor.Model`
- rail: graph-editing

| [INDEX] | [SYMBOL]               | [KIND]                  |
| :-----: | :--------------------- | :---------------------- |
|  [01]   | `INode`                | graph node              |
|  [02]   | `IDrawingNode : INode` | root canvas node        |
|  [03]   | `IConnector`           | graph edge              |
|  [04]   | `IPin`                 | node port               |
|  [05]   | `IConnectablePin`      | typed node port         |
|  [06]   | `IDrawingNodeSettings` | graph policy            |
|  [07]   | `IDrawingNodeFactory`  | graph factory           |
|  [08]   | `INodeSerializer`      | round-trip serializer   |
|  [09]   | `IUndoRedoHost`        | undo host               |
|  [10]   | `INodeTemplate`        | palette template        |
|  [11]   | `INodeTemplatesHost`   | template host           |
|  [12]   | `INodeFactory`         | node factory            |
|  [13]   | `IEditor`              | editor binding          |
|  [14]   | `DrawingNodeEditor`    | headless editing engine |

[INode]:

- Permissions: `CanSelect`, `CanRemove`, `CanMove`, `CanResize`
- Operations: `Move`, `Resize(dx,dy,dir)`
- Events: `On{Created,Removed,Moved,Selected,Deselected,Resized}`

[IDrawingNode]:

- State: `Nodes`, `Connectors`, selection state, connector-drag state
- Operations: the editing command surface

[IConnector]:

- Permissions: `CanSelect`, `CanRemove`
- Events: `On{Created,Removed,Selected,Deselected,StartChanged,EndChanged}`

[IPin]:

- Permissions: `CanConnect`, `CanDisconnect`
- Events: `On{Created,Removed,Moved,Selected,Deselected,Resized,Connected,Disconnected}`

[IConnectablePin]:

- Direction: `Direction : PinDirection`
- Width: `BusWidth : int`
- Rules: directional and bus-width-matched connections

[IDrawingNodeSettings]:

- Connections: `EnableConnections`, `RequireDirectionalConnections`, `RequireMatchingBusWidth`, `EnableMultiplePinConnections`
- Admission: `AllowSelfConnections`, `AllowDuplicateConnections`
- Ink: `EnableInk`, `IsInkMode`
- Snap: `EnableSnap`, `SnapX`, `SnapY`
- Nudge: `NudgeStep`, `NudgeMultiplier`
- Grid: `EnableGrid`, `GridCellWidth`, `GridCellHeight`
- Guides: `EnableGuides`

[IDrawingNodeFactory]:

- Operations: `CreatePin()`, `CreateConnector()`, `CreateList<T>()`
- Ownership: the engine mints pins, connectors, and lists through this factory

[INodeSerializer]:

- Operations: `Serialize<T>(T) : string`, `Deserialize<T>(string) : T?`
- Ownership: graph persistence and clipboard round trips

[IUndoRedoHost]:

- State: `CanUndo`, `CanRedo`
- Operations: `Undo()`, `Redo()`, `BeginUndoBatch()`, `EndUndoBatch()`
- Ownership: coalesced undo history

[IEditor]:

- Drawing: `Drawing : IDrawingNode?`
- Templates: `Templates : IList<INodeTemplate>?`

[DrawingNodeEditor]:

- Constructor: `new DrawingNodeEditor(IDrawingNode, IDrawingNodeFactory)`
- Ownership: connect, disconnect, clipboard, and `ConnectionValidationContext` validation

[GRAPH_ENUMS]: `NodeEditor.Model` graph vocabulary
- rail: graph-editing

| [INDEX] | [SYMBOL]                    | [KIND]               |
| :-----: | :-------------------------- | :------------------- |
|  [01]   | `ConnectorRoutingMode`      | routing mode         |
|  [02]   | `ConnectorRoutingAlgorithm` | routing algorithm    |
|  [03]   | `ConnectorStyle`            | connector style      |
|  [04]   | `ConnectorOrientation`      | endpoint orientation |
|  [05]   | `ConnectorArrowStyle`       | arrowhead            |
|  [06]   | `ConnectorPoint`            | anchor point         |
|  [07]   | `PinAlignment`              | pin-edge alignment   |
|  [08]   | `PinDirection`              | IO direction         |
|  [09]   | `NodeAlignment`             | alignment            |
|  [10]   | `NodeDistribution`          | distribution         |
|  [11]   | `NodeOrder`                 | z-order              |
|  [12]   | `NodeResizeDirection`       | resize handle        |
|  [13]   | `InkShape`                  | ink shape            |
|  [14]   | `InkPen`                    | ink pen              |
|  [15]   | `InkPoint`                  | ink point            |
|  [16]   | `InkStroke`                 | ink stroke           |

[ConnectorRoutingMode]: `Manual`, `Auto`

[ConnectorRoutingAlgorithm]: `Auto`, `Orthogonal`, `Octilinear`

[ConnectorStyle]: `Bezier`, `Straight`, `Orthogonal`

The ink vocabulary binds the `EnableInk` layer.

[GRAPH_EVENTS]: `NodeEditor.Model` typed event-args family
- rail: graph-editing

| [INDEX] | [SYMBOL]                                                           | [KIND]                   |
| :-----: | :----------------------------------------------------------------- | :----------------------- |
|  [01]   | `Node{Created,Removed,Moved,Resized,Selected,Deselected}EventArgs` | node lifecycle           |
|  [02]   | `Pin{Created,Removed,Moved,Resized,Selected,Deselected}EventArgs`  | pin lifecycle            |
|  [03]   | `Pin{Connected,Disconnected}EventArgs`                             | pin connection lifecycle |
|  [04]   | `Connector{Created,Removed,Selected,Deselected}EventArgs`          | connector lifecycle      |
|  [05]   | `Connector{Start,End}ChangedEventArgs`                             | endpoint rebind          |
|  [06]   | `ConnectionRejectedEventArgs`                                      | connection rejection     |
|  [07]   | `ConnectorExtensions`                                              | extension container      |

`IDrawingNodeSettings` or a validator raises `ConnectionRejectedEventArgs`. `ConnectorExtensions` carries connector geometry and orientation helpers.

[CANVAS_CONTROLS]: `NodeEditor.Controls` Avalonia layer
- rail: graph-editing

| [INDEX] | [SYMBOL]                    | [ROLE]                      |
| :-----: | :-------------------------- | :-------------------------- |
|  [01]   | `Editor`                    | top-level control           |
|  [02]   | `DrawingNode`               | canvas control              |
|  [03]   | `DrawingNodeProperties`     | canvas properties           |
|  [04]   | `Node`                      | node host control           |
|  [05]   | `Nodes`                     | nodes panel                 |
|  [06]   | `Pin`                       | pin control                 |
|  [07]   | `Pins`                      | pins panel                  |
|  [08]   | `Connector`                 | connector control           |
|  [09]   | `Connectors`                | connectors panel            |
|  [10]   | `NodeZoomBorder`            | pan-and-zoom viewport       |
|  [11]   | `Toolbox`                   | node-template palette       |
|  [12]   | `InkLayer`                  | ink overlay                 |
|  [13]   | `InkStrokePresenter`        | stroke presenter            |
|  [14]   | `InkGeometryBuilder`        | ink geometry builder        |
|  [15]   | `InkColorHelper`            | ink color helper            |
|  [16]   | `GridDecorator`             | grid overlay                |
|  [17]   | `GuidesAdorner`             | guides overlay              |
|  [18]   | `SelectionAdorner`          | selection overlay           |
|  [19]   | `SelectedAdorner`           | selected-item overlay       |
|  [20]   | `ConnectorSelectedAdorner`  | connector-selection overlay |
|  [21]   | `ConnectorCrossingsAdorner` | connector-crossing overlay  |
|  [22]   | `EditableTextBlock`         | inline rename label         |
|  [23]   | `ExportRoot`                | export render root          |

`Editor` binds `Drawing`, `Templates`, `InputSource`, `ZoomControl`, and `AdornerCanvas` through `StyledProperty` values. `NodeZoomBorder` hosts the canvas as a `PanAndZoom` `ZoomBorder` subclass.

[CANVAS_UTILITIES]: `NodeEditor.*` graph algorithms + behaviors
- rail: graph-editing

| [INDEX] | [SYMBOL]                  | [ROLE]                  |
| :-----: | :------------------------ | :---------------------- |
|  [01]   | `OrthogonalRouter`        | orthogonal routing      |
|  [02]   | `RTree`                   | spatial index           |
|  [03]   | `HitTestIndex`            | indexed hit testing     |
|  [04]   | `HitTestHelper`           | hit-test operations     |
|  [05]   | `SnapHelper`              | grid snapping           |
|  [06]   | `ConnectorPathHelper`     | connector path geometry |
|  [07]   | `NodeEditor.Behaviors.*`  | interaction namespace   |
|  [08]   | `NodeEditor.Converters.*` | converter namespace     |

[OrthogonalRouter]:

- Algorithm: A\*-style orthogonal routing
- Heap: internal `MinHeap`
- Field: obstacle field

[HIT_TESTING]: `RTree`, `HitTestIndex`, and `HitTestHelper` own connector and node hit testing plus crossing detection.

[NodeEditor.Behaviors]:

- Drawing: `Drawing{Pressed,Moved,Released,Selection}Behavior`
- Connector: `ConnectorInteractionBehavior`
- Pin: `PinPressedBehavior`
- Node: `Node{Resize,Rotate}Behavior`
- Selection: `*SelectedBehavior`
- Drop: `DrawingDropHandler`, `IDrawingDropTarget`
- Insert: `InsertTemplateOnDoubleTappedBehavior`
- Toolbox: `ToolboxDragBehavior`
- Driver: `Xaml.Behaviors`

[NodeEditor.Converters]: `PinToPointConverter`, `PinMarginConverter`, `EnumToCheckedConverter`, `EnumEqualsConverter`, `NullToBoolConverter`,
`BoolInvertConverter`, …

[CANVAS_SERVICES]: `NodeEditor.Services` IO + export
- rail: graph-editing

| [INDEX] | [SYMBOL]         | [ROLE]         |
| :-----: | :--------------- | :------------- |
|  [01]   | `StorageService` | picker presets |
|  [02]   | `ExportRenderer` | canvas export  |

[StorageService]: `FilePickerFileType` presets — `Json`, `ImageSvg`, `ImagePng`, `ImageSkp`, `Pdf`, `Xps`, `All`.

[ExportRenderer]: static canvas export — `RenderPng`, `RenderSvg`, `RenderSkp`, `RenderPdf`, `RenderXps(Control, Size, Stream, double dpi)`.

## [03]-[ENTRYPOINTS]

[DRAWING_COMMANDS]: `IDrawingNode` editing operations — the canvas mutates HERE, not by hand-moving controls
- rail: graph-editing

| [INDEX] | [SURFACE]                                         | [ROLE]                      |
| :-----: | :------------------------------------------------ | :-------------------------- |
|  [01]   | `CutNodes()`                                      | clipboard cut               |
|  [02]   | `CopyNodes()`                                     | clipboard copy              |
|  [03]   | `PasteNodes()`                                    | clipboard paste             |
|  [04]   | `DuplicateNodes()`                                | selection duplication       |
|  [05]   | `DeleteNodes()`                                   | selection deletion          |
|  [06]   | `AlignSelectedNodes(NodeAlignment)`               | selection alignment         |
|  [07]   | `DistributeSelectedNodes(NodeDistribution)`       | selection distribution      |
|  [08]   | `OrderSelectedNodes(NodeOrder)`                   | selection z-order           |
|  [09]   | `LockSelection()`                                 | selection lock              |
|  [10]   | `SetSelectedNodes(ISet<INode>?)`                  | node selection setter       |
|  [11]   | `GetSelectedNodes()`                              | node selection getter       |
|  [12]   | `SetSelectedConnectors(...)`                      | connector selection setter  |
|  [13]   | `GetSelectedConnectors()`                         | connector selection getter  |
|  [14]   | `DrawingLeftPressed(x,y)`                         | left-press transition       |
|  [15]   | `DrawingRightPressed(x,y)`                        | right-press transition      |
|  [16]   | `ConnectorLeftPressed(IPin, bool showWhenMoving)` | connector-drag start        |
|  [17]   | `ConnectorMove(x,y)`                              | connector-drag move         |
|  [18]   | `CancelConnector()`                               | connector-drag cancel       |
|  [19]   | `CanConnectPin(IPin)`                             | connection gate             |
|  [20]   | `IsPinConnected(IPin)`                            | connection state            |
|  [21]   | `IsConnectorMoving()`                             | drag state                  |
|  [22]   | `CanSelectNodes()`                                | node selection gate         |
|  [23]   | `CanSelectConnectors()`                           | connector selection gate    |
|  [24]   | `NotifyConnectionRejected(IPin, IPin)`            | rejection event             |
|  [25]   | `NotifySelectionChanged()`                        | selection event             |
|  [26]   | `NotifyDeselectedNodes()`                         | node deselection event      |
|  [27]   | `NotifyDeselectedConnectors()`                    | connector deselection event |
|  [28]   | `GetSerializer()`                                 | serializer getter           |
|  [29]   | `SetSerializer(INodeSerializer?)`                 | serializer setter           |

The press and move operations drive the pointer-based connector-drag state machine. The notification operations raise the typed events, and the serializer accessors bind graph persistence plus clipboard round trips.

[ENGINE_OPERATIONS]: `DrawingNodeEditor` headless engine — host-free graph editing + validation
- rail: graph-editing

| [INDEX] | [SURFACE]                                                               | [ROLE]                |
| :-----: | :---------------------------------------------------------------------- | :-------------------- |
|  [01]   | `new DrawingNodeEditor(IDrawingNode node, IDrawingNodeFactory factory)` | engine binding        |
|  [02]   | `Clone<T>(T source) : T?`                                               | deep clone            |
|  [03]   | `IsPinConnected(IPin)`                                                  | connection state      |
|  [04]   | `IsConnectorMoving()`                                                   | connector-drag state  |
|  [05]   | `CancelConnector()`                                                     | connector-drag cancel |
|  [06]   | `CanConnectPin(IPin)`                                                   | connection gate       |
|  [07]   | `ConnectionValidationContext(node, start, end)`                         | validation context    |

`Clone<T>` uses `INodeSerializer` for paste and duplication. The connection-state operations honor `IDrawingNodeSettings`, and the validation context admits directional, bus-width, self-connection, and duplicate-connection validators before commit.

[EXPORT_PERSIST]: `ExportRenderer` + `StorageService` + `INodeSerializer`
- rail: graph-editing

| [INDEX] | [SURFACE]                                  | [SURFACE_ROOT]    | [ROLE]             |
| :-----: | :----------------------------------------- | :---------------- | :----------------- |
|  [01]   | `RenderPng(Control, Size, Stream, dpi=96)` | `ExportRenderer`  | PNG export         |
|  [02]   | `RenderSvg(...)`                           | `ExportRenderer`  | SVG export         |
|  [03]   | `RenderPdf(..., dpi=72)`                   | `ExportRenderer`  | PDF export         |
|  [04]   | `RenderSkp(...)`                           | `ExportRenderer`  | SKP export         |
|  [05]   | `RenderXps(...)`                           | `ExportRenderer`  | XPS export         |
|  [06]   | `StorageService.Json`                      | `StorageService`  | JSON picker        |
|  [07]   | `StorageService.ImageSvg`                  | `StorageService`  | SVG picker         |
|  [08]   | `StorageService.ImagePng`                  | `StorageService`  | PNG picker         |
|  [09]   | `StorageService.ImageSkp`                  | `StorageService`  | SKP picker         |
|  [10]   | `StorageService.Pdf`                       | `StorageService`  | PDF picker         |
|  [11]   | `StorageService.Xps`                       | `StorageService`  | XPS picker         |
|  [12]   | `StorageService.All`                       | `StorageService`  | all-file picker    |
|  [13]   | `Serialize<T>(T) : string`                 | `INodeSerializer` | graph serializer   |
|  [14]   | `Deserialize<T>(string) : T?`              | `INodeSerializer` | graph deserializer |

`ExportRenderer` writes raster or vector canvas output to a stream. `StorageService` owns the `FilePickerFileType` presets for export and open dialogs, and `INodeSerializer` owns graph plus clipboard JSON round trips.

## [04]-[IMPLEMENTATION_LAW]

[MODEL_LAW]:
- Package: `NodeEditorAvalonia.Model`
- Owns: the framework-agnostic graph core — `IDrawingNode`/`INode`/`IConnector`/`IPin`/`IConnectablePin` contracts, the `DrawingNodeEditor` headless engine, `IDrawingNodeSettings` connection policy, `INodeSerializer` round-trip, `IUndoRedoHost`, the `INodeTemplate`/`INodeFactory` palette contracts, and the typed `{Node,Pin,Connector}*EventArgs` family + enums.
- Accept: product view-models implement `IDrawingNode`/`INode`/`IConnector`/`IPin` on the admitted `ReactiveUI` rail (`ReactiveObject`-backed, `api-reactiveui-avalonia.md`); the `Shell/Editing` parametric and dependency-graph surfaces drive editing through the `IDrawingNode` commands and the `DrawingNodeEditor` engine; connection rules (`RequireDirectionalConnections`/`RequireMatchingBusWidth`/`AllowSelfConnections`/`AllowDuplicateConnections`) live in `IDrawingNodeSettings`, with `IConnectablePin.Direction`/`BusWidth` typing the ports.
- Reject: implementing the model on `CommunityToolkit.Mvvm` (not admitted); hand-rolling a node/connector graph type the model already owns; mutating the graph by repositioning controls instead of through the `IDrawingNode`/`DrawingNodeEditor` operations; a bespoke undo stack where `IUndoRedoHost` (with `Begin`/`EndUndoBatch`) owns coalesced history.

[CANVAS_LAW]:
- Package: `NodeEditorAvalonia`
- Owns: the Avalonia layer — the `Editor`/`DrawingNode`/`Node`/`Pin`/`Connector` controls bound to the model graph, the `Xaml.Behaviors` interaction behaviors, `NodeZoomBorder` (the `PanAndZoom` viewport), the `OrthogonalRouter`+`RTree` routing/hit-test, the ink layer + adorners, and the `ExportRenderer`/`StorageService` IO.
- Accept: `Editor.Drawing` binds an `IDrawingNode` and `Editor.Templates` a palette; pan/zoom is RULED `NodeZoomBorder` (the package's own distinct `Avalonia.Controls.PanAndZoom` asset — NOT the separately admitted `PanAndZoom.dll`/`ZoomBorder` from `api-panandzoom.md`, which keeps its five page consumers — so both restore without collision and no dup exists); connector routing is `ConnectorRoutingAlgorithm`/`ConnectorStyle` over `OrthogonalRouter`, never a hand-rolled path layout; the `LoroTree` co-edit bridge (`api-loro.md`) discriminates `EventTriggerKind.Local`/`Import`/`Checkout` so a local UI mutation commits as tree ops without re-applying its own echo and a remote `Import` applies to the ReactiveUI graph model without re-emitting.
- Reject: hosting the canvas in a second pan/zoom border; re-implementing connector routing or hit-testing where `OrthogonalRouter`/`RTree`/`HitTestIndex` exist; building a parallel export path — `ExportRenderer.RenderPdf`/`RenderSvg` converge with the drafting/PDF export set (`api-drafting-export.md`, `api-pdfsharp.md`) and `StorageService` supplies the matching `FilePickerFileType` presets.

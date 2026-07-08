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

| [INDEX] | [SYMBOL]                       | [KIND]                                                                                  |
| :-----: | :----------------------------- | :------------------------------------------------------------------------------------- |
|  [01]   | `INode`                        | graph node — `CanSelect`/`CanRemove`/`CanMove`/`CanResize`, `Move`/`Resize(dx,dy,dir)`, `On{Created,Removed,Moved,Selected,Deselected,Resized}` |
|  [02]   | `IDrawingNode : INode`         | the root canvas node — owns `Nodes`/`Connectors`, selection state, the connector-drag machine, and the editing commands (see entrypoints) |
|  [03]   | `IConnector`                   | edge between two pins — `CanSelect`/`CanRemove`, `On{Created,Removed,Selected,Deselected,StartChanged,EndChanged}` |
|  [04]   | `IPin`                         | node port — `CanConnect`/`CanDisconnect`, `On{Created,Removed,Moved,Selected,Deselected,Resized,Connected,Disconnected}` |
|  [05]   | `IConnectablePin`              | typed pin — `Direction : PinDirection`, `BusWidth : int` (directional + bus-width-matched connection rules) |
|  [06]   | `IDrawingNodeSettings`         | connection + interaction policy — `EnableConnections`, `RequireDirectionalConnections`, `RequireMatchingBusWidth`, `EnableMultiplePinConnections`, `AllowSelfConnections`, `AllowDuplicateConnections`, `EnableInk`/`IsInkMode`, `EnableSnap`/`SnapX`/`SnapY`, `NudgeStep`/`NudgeMultiplier`, `EnableGrid`/`GridCellWidth`/`GridCellHeight`, `EnableGuides` |
|  [07]   | `IDrawingNodeFactory`          | `CreatePin()` / `CreateConnector()` / `CreateList<T>()` — the factory the engine mints pins/connectors/lists through |
|  [08]   | `INodeSerializer`              | `Serialize<T>(T) : string` / `Deserialize<T>(string) : T?` — graph persistence + clipboard round-trip contract |
|  [09]   | `IUndoRedoHost`                | `CanUndo`/`CanRedo`, `Undo()`/`Redo()`, `BeginUndoBatch()`/`EndUndoBatch()` — coalesced undo history |
|  [10]   | `INodeTemplate` / `INodeTemplatesHost` / `INodeFactory` | palette template + template-host + node factory contracts            |
|  [11]   | `IEditor`                      | top-level editor binding — `Drawing : IDrawingNode?`, `Templates : IList<INodeTemplate>?` |
|  [12]   | `DrawingNodeEditor`            | the headless graph-editing ENGINE — `new DrawingNodeEditor(IDrawingNode, IDrawingNodeFactory)`; owns connect/disconnect, clipboard, and the `ConnectionValidationContext` validation pipeline |

[GRAPH_ENUMS]: `NodeEditor.Model` graph vocabulary
- rail: graph-editing

| [INDEX] | [SYMBOL]                                            | [RAIL]                                                          |
| :-----: | :-------------------------------------------------- | :------------------------------------------------------------ |
|  [01]   | `ConnectorRoutingMode`                              | `Manual` / `Auto`                                              |
|  [02]   | `ConnectorRoutingAlgorithm`                         | `Auto` / `Orthogonal` / `Octilinear`                          |
|  [03]   | `ConnectorStyle`                                    | `Bezier` / `Straight` / `Orthogonal`                          |
|  [04]   | `ConnectorOrientation` / `ConnectorArrowStyle` / `ConnectorPoint` | connector endpoint orientation, arrowhead, anchor point |
|  [05]   | `PinAlignment` / `PinDirection`                     | pin edge alignment + IO direction                             |
|  [06]   | `NodeAlignment` / `NodeDistribution` / `NodeOrder` / `NodeResizeDirection` | align / distribute / z-order / resize-handle vocabulary |
|  [07]   | `InkShape` / `InkPen` / `InkPoint` / `InkStroke`    | ink-annotation primitives (`EnableInk` layer)                 |

[GRAPH_EVENTS]: `NodeEditor.Model` typed event-args family
- rail: graph-editing

| [INDEX] | [SYMBOL]                                                          | [KIND]                                                          |
| :-----: | :--------------------------------------------------------------- | :------------------------------------------------------------- |
|  [01]   | `Node{Created,Removed,Moved,Resized,Selected,Deselected}EventArgs` | node lifecycle events                                        |
|  [02]   | `Pin{Created,Removed,Moved,Resized,Selected,Deselected,Connected,Disconnected}EventArgs` | pin lifecycle + connection events             |
|  [03]   | `Connector{Created,Removed,Selected,Deselected}EventArgs` / `Connector{Start,End}ChangedEventArgs` | connector lifecycle + endpoint rebind |
|  [04]   | `ConnectionRejectedEventArgs`                                   | a connection blocked by `IDrawingNodeSettings`/validator       |
|  [05]   | `ConnectorExtensions`                                           | connector geometry/orientation helper extensions               |

[CANVAS_CONTROLS]: `NodeEditor.Controls` Avalonia layer
- rail: graph-editing

| [INDEX] | [SYMBOL]                                            | [RAIL]                                                          |
| :-----: | :-------------------------------------------------- | :------------------------------------------------------------ |
|  [01]   | `Editor`                                            | top-level editor control — `Drawing`/`Templates`/`InputSource`/`ZoomControl`/`AdornerCanvas` `StyledProperty` bindings |
|  [02]   | `DrawingNode` / `DrawingNodeProperties`            | the drawing-canvas control + its property surface              |
|  [03]   | `Node` / `Nodes`                                   | node host control + nodes panel                                |
|  [04]   | `Pin` / `Pins`                                     | pin control + pins panel                                       |
|  [05]   | `Connector` / `Connectors`                         | connector control + connectors panel                           |
|  [06]   | `NodeZoomBorder`                                   | the pan/zoom viewport (a `PanAndZoom` `ZoomBorder` subclass) hosting the canvas |
|  [07]   | `Toolbox`                                          | node-template palette                                          |
|  [08]   | `InkLayer` / `InkStrokePresenter` / `InkGeometryBuilder` / `InkColorHelper` | ink-annotation overlay + geometry builder      |
|  [09]   | `GridDecorator` / `GuidesAdorner` / `SelectionAdorner` / `SelectedAdorner` / `ConnectorSelectedAdorner` / `ConnectorCrossingsAdorner` | grid, guides, selection, and connector-crossing overlays |
|  [10]   | `EditableTextBlock` / `ExportRoot`                 | inline-rename label + export render root                       |

[CANVAS_UTILITIES]: `NodeEditor.*` graph algorithms + behaviors
- rail: graph-editing

| [INDEX] | [SYMBOL]                                            | [RAIL]                                                          |
| :-----: | :-------------------------------------------------- | :------------------------------------------------------------ |
|  [01]   | `OrthogonalRouter`                                 | A*-style orthogonal connector router (internal `MinHeap`) over the obstacle field |
|  [02]   | `RTree` / `HitTestIndex` / `HitTestHelper`         | spatial index for connector/node hit-testing + crossing detection |
|  [03]   | `SnapHelper` / `ConnectorPathHelper`               | grid snapping + connector path geometry                        |
|  [04]   | `NodeEditor.Behaviors.*`                           | `Xaml.Behaviors`-driven interaction: `Drawing{Pressed,Moved,Released,Selection}Behavior`, `ConnectorInteractionBehavior`, `PinPressedBehavior`, `Node{Resize,Rotate}Behavior`, `*SelectedBehavior`, `DrawingDropHandler`/`IDrawingDropTarget`, `InsertTemplateOnDoubleTappedBehavior`, `ToolboxDragBehavior` |
|  [05]   | `NodeEditor.Converters.*`                          | `PinToPointConverter`, `PinMarginConverter`, `EnumToCheckedConverter`, `EnumEqualsConverter`, `NullToBoolConverter`, `BoolInvertConverter`, … |

[CANVAS_SERVICES]: `NodeEditor.Services` IO + export
- rail: graph-editing

| [INDEX] | [SYMBOL]                                            | [RAIL]                                                          |
| :-----: | :-------------------------------------------------- | :------------------------------------------------------------ |
|  [01]   | `StorageService`                                   | `FilePickerFileType` presets — `Json`, `ImageSvg`, `ImagePng`, `ImageSkp`, `Pdf`, `Xps`, `All` |
|  [02]   | `ExportRenderer`                                   | static canvas export — `RenderPng`/`RenderSvg`/`RenderSkp`/`RenderPdf`/`RenderXps(Control, Size, Stream, double dpi)` |

## [03]-[ENTRYPOINTS]

[DRAWING_COMMANDS]: `IDrawingNode` editing operations — the canvas mutates HERE, not by hand-moving controls
- rail: graph-editing

| [INDEX] | [SURFACE]                                                       | [SURFACE_ROOT] | [RAIL]                                |
| :-----: | :-------------------------------------------------------------- | :------------- | :------------------------------------ |
|  [01]   | `CutNodes()` / `CopyNodes()` / `PasteNodes()` / `DuplicateNodes()` / `DeleteNodes()` | `IDrawingNode` | clipboard + delete over the selection |
|  [02]   | `AlignSelectedNodes(NodeAlignment)` / `DistributeSelectedNodes(NodeDistribution)` / `OrderSelectedNodes(NodeOrder)` | `IDrawingNode` | align / distribute / z-order |
|  [03]   | `LockSelection()` / `SetSelectedNodes(ISet<INode>?)` / `GetSelectedNodes()` / `SetSelectedConnectors(...)` / `GetSelectedConnectors()` | `IDrawingNode` | lock + selection state |
|  [04]   | `DrawingLeftPressed(x,y)` / `DrawingRightPressed(x,y)` / `ConnectorLeftPressed(IPin, bool showWhenMoving)` / `ConnectorMove(x,y)` / `CancelConnector()` | `IDrawingNode` | the pointer-driven connector-drag machine |
|  [05]   | `CanConnectPin(IPin)` / `IsPinConnected(IPin)` / `IsConnectorMoving()` / `CanSelectNodes()` / `CanSelectConnectors()` | `IDrawingNode` | connection + selection gates |
|  [06]   | `NotifyConnectionRejected(IPin, IPin)` / `NotifySelectionChanged()` / `NotifyDeselectedNodes()` / `NotifyDeselectedConnectors()` | `IDrawingNode` | notification raisers (drive the typed events) |
|  [07]   | `GetSerializer()` / `SetSerializer(INodeSerializer?)`           | `IDrawingNode` | bind the persistence/clipboard serializer |

[ENGINE_OPERATIONS]: `DrawingNodeEditor` headless engine — host-free graph editing + validation
- rail: graph-editing

| [INDEX] | [SURFACE]                                                       | [SURFACE_ROOT]      | [RAIL]                                |
| :-----: | :-------------------------------------------------------------- | :------------------ | :------------------------------------ |
|  [01]   | `new DrawingNodeEditor(IDrawingNode node, IDrawingNodeFactory factory)` | `DrawingNodeEditor` | bind the engine to a node + factory   |
|  [02]   | `Clone<T>(T source) : T?`                                       | `DrawingNodeEditor` | deep-clone via `INodeSerializer` (paste/duplicate backing) |
|  [03]   | `IsPinConnected(IPin)` / `IsConnectorMoving()` / `CancelConnector()` / `CanConnectPin(IPin)` | `DrawingNodeEditor` | connection-state machine (respects `IDrawingNodeSettings`) |
|  [04]   | connection validation: `ConnectionValidationContext(node, start, end)` consulted before a connector is committed | `DrawingNodeEditor` | custom connection-validator pipeline (directional + bus-width + self/duplicate gates) |

[EXPORT_PERSIST]: `ExportRenderer` + `StorageService` + `INodeSerializer`
- rail: graph-editing

| [INDEX] | [SURFACE]                                                       | [SURFACE_ROOT]   | [RAIL]                                |
| :-----: | :-------------------------------------------------------------- | :--------------- | :------------------------------------ |
|  [01]   | `RenderPng(Control, Size, Stream, dpi=96)` / `RenderSvg(...)` / `RenderPdf(..., dpi=72)` / `RenderSkp(...)` / `RenderXps(...)` | `ExportRenderer` | rasterize/vectorize the canvas to a stream |
|  [02]   | `StorageService.{Json,ImageSvg,ImagePng,ImageSkp,Pdf,Xps,All}` | `StorageService` | `FilePickerFileType` presets for the export/open dialog |
|  [03]   | `Serialize<T>(T) : string` / `Deserialize<T>(string) : T?`     | `INodeSerializer` | graph + clipboard JSON round-trip      |

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

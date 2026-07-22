# [RASM_APPUI_API_NODEEDITOR]

`NodeEditorAvalonia` owns node/pin/connector graph editing across two assemblies. `NodeEditorAvalonia.Model` mints the framework-agnostic graph core — contracts, the headless `DrawingNodeEditor` engine, connection policy, serialization, and the typed event family — and `NodeEditorAvalonia` binds it to the Avalonia controls, `Xaml.Behaviors` interaction, the `NodeZoomBorder` viewport, the `OrthogonalRouter`+`RTree` spatial index, the ink layer, and the `ExportRenderer`.

Every mutation folds through the `IDrawingNode` command surface and the `DrawingNodeEditor` engine, never a repositioned control.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `NodeEditorAvalonia.Model`
- package: `NodeEditorAvalonia.Model` (MIT)
- assembly: `NodeEditorAvalonia.Model`
- namespace: `NodeEditor.Model` — framework-agnostic graph contracts, the `DrawingNodeEditor` engine, and the typed `{Node,Pin,Connector}*EventArgs` family
- asset: managed runtime library (`lib/net10.0` binds the `net10.0` consumer; `lib/net8.0` fallback)
- rail: graph-editing

[PACKAGE_SURFACE]: `NodeEditorAvalonia`
- package: `NodeEditorAvalonia` (MIT)
- assembly: `NodeEditorAvalonia`
- namespace: `NodeEditor.Controls` (`Editor`, `DrawingNode`, `Node`, `Pin`, `Connector`, `NodeZoomBorder`, `Toolbox`, ink + adorner controls), `NodeEditor.Behaviors` (drag/connect/select/resize/rotate over `Xaml.Behaviors`), `NodeEditor.Converters`, `NodeEditor.Services` (`StorageService`, `ExportRenderer`), with `OrthogonalRouter`/`RTree`/`HitTestIndex`/`SnapHelper`/`ConnectorPathHelper` utilities
- asset: managed runtime library (`lib/net10.0` binds the `net10.0` consumer; `lib/net8.0` fallback)
- depends: `NodeEditorAvalonia.Model`, `Avalonia`, `Avalonia.Controls.PanAndZoom`, `Avalonia.Xaml.Behaviors`
- rail: graph-editing

## [02]-[PUBLIC_TYPES]

[GRAPH_CONTRACTS]: framework-agnostic model graph — `NodeEditor.Model`

| [INDEX] | [SYMBOL]               | [TYPE_FAMILY] | [CAPABILITY]                            |
| :-----: | :--------------------- | :------------ | :-------------------------------------- |
|  [01]   | `INode`                | interface     | graph node                              |
|  [02]   | `IDrawingNode : INode` | interface     | root canvas node + editing command host |
|  [03]   | `IConnector`           | interface     | graph edge                              |
|  [04]   | `IPin`                 | interface     | node port                               |
|  [05]   | `IConnectablePin`      | interface     | typed node port, direction + bus width  |
|  [06]   | `IDrawingNodeSettings` | interface     | connection, ink, snap, grid policy      |
|  [07]   | `IDrawingNodeFactory`  | interface     | pin/connector/list factory              |
|  [08]   | `INodeSerializer`      | interface     | round-trip serializer                   |
|  [09]   | `IUndoRedoHost`        | interface     | coalesced undo history                  |
|  [10]   | `INodeTemplate`        | interface     | palette template                        |
|  [11]   | `INodeTemplatesHost`   | interface     | template host                           |
|  [12]   | `INodeFactory`         | interface     | node factory                            |
|  [13]   | `IEditor`              | interface     | editor binding                          |
|  [14]   | `DrawingNodeEditor`    | class         | headless editing + validation engine    |

[INode]: `CanSelect` `CanRemove` `CanMove` `CanResize` `Move` `Resize(dx,dy,dir)` `On{Created,Removed,Moved,Selected,Deselected,Resized}`
[IDrawingNode]: `Nodes` `Connectors` + selection and connector-drag state
[IConnector]: `CanSelect` `CanRemove` `On{Created,Removed,Selected,Deselected,StartChanged,EndChanged}`
[IPin]: `CanConnect` `CanDisconnect` `On{Created,Removed,Moved,Selected,Deselected,Resized,Connected,Disconnected}`
[IConnectablePin]: `Direction : PinDirection` `BusWidth : int`
[IDrawingNodeSettings]: `EnableConnections` `RequireDirectionalConnections` `RequireMatchingBusWidth` `EnableMultiplePinConnections` `AllowSelfConnections` `AllowDuplicateConnections` `EnableInk` `IsInkMode` `EnableSnap` `SnapX` `SnapY` `NudgeStep` `NudgeMultiplier` `EnableGrid` `GridCellWidth` `GridCellHeight` `EnableGuides`
[IDrawingNodeFactory]: `CreatePin()` `CreateConnector()` `CreateList<T>()`
[IUndoRedoHost]: `CanUndo` `CanRedo` `Undo()` `Redo()` `BeginUndoBatch()` `EndUndoBatch()`
[IEditor]: `Drawing : IDrawingNode?` `Templates : IList<INodeTemplate>?`

[GRAPH_ENUMS]: `NodeEditor.Model` graph vocabulary (enums)

| [INDEX] | [SYMBOL]                    | [CAPABILITY]         |
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

[ConnectorRoutingMode]: `Manual` `Auto`
[ConnectorRoutingAlgorithm]: `Auto` `Orthogonal` `Octilinear`
[ConnectorStyle]: `Bezier` `Straight` `Orthogonal`

[GRAPH_EVENTS]: `NodeEditor.Model` typed event-args family (classes)

| [INDEX] | [SYMBOL]                                                           | [CAPABILITY]                              |
| :-----: | :----------------------------------------------------------------- | :---------------------------------------- |
|  [01]   | `Node{Created,Removed,Moved,Resized,Selected,Deselected}EventArgs` | node lifecycle                            |
|  [02]   | `Pin{Created,Removed,Moved,Resized,Selected,Deselected}EventArgs`  | pin lifecycle                             |
|  [03]   | `Pin{Connected,Disconnected}EventArgs`                             | pin connection lifecycle                  |
|  [04]   | `Connector{Created,Removed,Selected,Deselected}EventArgs`          | connector lifecycle                       |
|  [05]   | `Connector{Start,End}ChangedEventArgs`                             | endpoint rebind                           |
|  [06]   | `ConnectionRejectedEventArgs`                                      | rejection raised by settings or validator |
|  [07]   | `ConnectorExtensions`                                              | connector geometry + orientation helpers  |

[CANVAS_CONTROLS]: `NodeEditor.Controls` Avalonia layer (controls)

| [INDEX] | [SYMBOL]                    | [CAPABILITY]                |
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

`Editor` binds `Drawing`/`Templates`/`InputSource`/`ZoomControl`/`AdornerCanvas` as `StyledProperty` values; `NodeZoomBorder` subclasses the package's own `PanAndZoom` `ZoomBorder`.

[CANVAS_UTILITIES]: `NodeEditor.*` graph algorithms + behaviors

| [INDEX] | [SYMBOL]                  | [CAPABILITY]                              |
| :-----: | :------------------------ | :---------------------------------------- |
|  [01]   | `OrthogonalRouter`        | A\*-style orthogonal routing over a field |
|  [02]   | `RTree`                   | spatial index                             |
|  [03]   | `HitTestIndex`            | indexed hit testing + crossing detection  |
|  [04]   | `HitTestHelper`           | hit-test operations                       |
|  [05]   | `SnapHelper`              | grid snapping                             |
|  [06]   | `ConnectorPathHelper`     | connector path geometry                   |
|  [07]   | `NodeEditor.Behaviors.*`  | interaction namespace                     |
|  [08]   | `NodeEditor.Converters.*` | converter namespace                       |

[NodeEditor.Behaviors]: `Drawing{Pressed,Moved,Released,Selection}Behavior` `ConnectorInteractionBehavior` `PinPressedBehavior` `Node{Resize,Rotate}Behavior` `*SelectedBehavior` `DrawingDropHandler` `IDrawingDropTarget` `InsertTemplateOnDoubleTappedBehavior` `ToolboxDragBehavior`
[NodeEditor.Converters]: `PinToPointConverter` `PinMarginConverter` `EnumToCheckedConverter` `EnumEqualsConverter` `NullToBoolConverter` `BoolInvertConverter`

[CANVAS_SERVICES]: `NodeEditor.Services` IO + export

| [INDEX] | [SYMBOL]         | [CAPABILITY]                       |
| :-----: | :--------------- | :--------------------------------- |
|  [01]   | `StorageService` | `FilePickerFileType` presets       |
|  [02]   | `ExportRenderer` | static raster/vector canvas export |

## [03]-[ENTRYPOINTS]

[DRAWING_COMMANDS]: `IDrawingNode` editing operations

| [INDEX] | [SURFACE]                                         | [CAPABILITY]                |
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

Press and move operations drive the pointer connector-drag state machine; the serializer accessors bind graph persistence and clipboard round trips.

[ENGINE_OPERATIONS]: `DrawingNodeEditor` headless engine — host-free editing + validation

| [INDEX] | [SURFACE]                                                               | [CAPABILITY]          |
| :-----: | :---------------------------------------------------------------------- | :-------------------- |
|  [01]   | `new DrawingNodeEditor(IDrawingNode node, IDrawingNodeFactory factory)` | engine binding        |
|  [02]   | `Clone<T>(T source) : T?`                                               | deep clone            |
|  [03]   | `IsPinConnected(IPin)`                                                  | connection state      |
|  [04]   | `IsConnectorMoving()`                                                   | connector-drag state  |
|  [05]   | `CancelConnector()`                                                     | connector-drag cancel |
|  [06]   | `CanConnectPin(IPin)`                                                   | connection gate       |
|  [07]   | `ConnectionValidationContext(node, start, end)`                         | validation context    |

`Clone<T>` runs through `INodeSerializer` for paste and duplication; the connection-state operations honor `IDrawingNodeSettings`, and the validation context admits directional, bus-width, self-connection, and duplicate-connection validators before commit.

[EXPORT_PERSIST]: `ExportRenderer` + `StorageService` + `INodeSerializer`

| [INDEX] | [SURFACE]                                  | [SURFACE_ROOT]    | [CAPABILITY]       |
| :-----: | :----------------------------------------- | :---------------- | :----------------- |
|  [01]   | `RenderPng(Control, Size, Stream, dpi=96)` | `ExportRenderer`  | PNG export         |
|  [02]   | `RenderSvg(...)`                           | `ExportRenderer`  | SVG export         |
|  [03]   | `RenderPdf(..., dpi=72)`                   | `ExportRenderer`  | PDF export         |
|  [04]   | `RenderSkp(...)`                           | `ExportRenderer`  | SKP export         |
|  [05]   | `RenderXps(...)`                           | `ExportRenderer`  | XPS export         |
|  [06]   | `Json`                                     | `StorageService`  | JSON picker        |
|  [07]   | `ImageSvg`                                 | `StorageService`  | SVG picker         |
|  [08]   | `ImagePng`                                 | `StorageService`  | PNG picker         |
|  [09]   | `ImageSkp`                                 | `StorageService`  | SKP picker         |
|  [10]   | `Pdf`                                      | `StorageService`  | PDF picker         |
|  [11]   | `Xps`                                      | `StorageService`  | XPS picker         |
|  [12]   | `All`                                      | `StorageService`  | all-file picker    |
|  [13]   | `Serialize<T>(T) : string`                 | `INodeSerializer` | graph serializer   |
|  [14]   | `Deserialize<T>(string) : T?`              | `INodeSerializer` | graph deserializer |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `NodeEditorAvalonia.Model` owns the graph algebra host-free; `NodeEditorAvalonia` binds it to Avalonia controls. Every mutation folds through the `IDrawingNode` command surface and the `DrawingNodeEditor` engine, never a repositioned control; connection policy rides `IDrawingNodeSettings` and `IConnectablePin.Direction`/`BusWidth`.

[STACKING]:
- `ReactiveUI.Avalonia`(`api-reactiveui-avalonia.md`): product view-models implement `IDrawingNode`/`INode`/`IConnector`/`IPin` as `ReactiveObject`-backed models; the `CommunityToolkit.Mvvm` model variant is unadmitted.
- `Loro`(`api-loro.md`): the `LoroTree` co-edit bridge projects graph mutation bidirectionally under one echo-suppressed handshake — `EventTriggerKind.Local` commits a local UI mutation as tree ops without re-applying its own echo, `Import` applies a remote merge to the model without re-emitting.
- `PanAndZoom`(`api-panandzoom.md`): `NodeZoomBorder` subclasses this package's own `Avalonia.Controls.PanAndZoom` `ZoomBorder`, a distinct asset from the separately admitted `PanAndZoom.dll` `ZoomBorder` — the two viewport surfaces carry no collision.
- `ACadSharp`/`PDFsharp`(`api-drafting-export.md`, `api-pdfsharp.md`): `ExportRenderer.RenderPdf`/`RenderSvg` canvas output converges with the drafting and PDF export set on the shared vector-export rail.
- within-lib: the parametric and dependency-graph editing surfaces drive every mutation through the `IDrawingNode` commands and the `DrawingNodeEditor` engine; `StorageService` supplies the matching `FilePickerFileType` presets.

[MODEL_LAW]:
- Package: `NodeEditorAvalonia.Model`
- Owns: the framework-agnostic graph core — `IDrawingNode`/`INode`/`IConnector`/`IPin`/`IConnectablePin` contracts, the `DrawingNodeEditor` headless engine, `IDrawingNodeSettings` connection policy, `INodeSerializer` round-trip, `IUndoRedoHost`, the `INodeTemplate`/`INodeFactory` palette contracts, and the typed `{Node,Pin,Connector}*EventArgs` family + enums.
- Accept: product models implement the contracts on the ReactiveUI rail; editing drives through the `IDrawingNode` commands and `DrawingNodeEditor`; connection rules ride `IDrawingNodeSettings` with `IConnectablePin` typing the ports.
- Reject: implementing the model on `CommunityToolkit.Mvvm`; hand-rolling a node/connector graph type the model owns; mutating the graph by repositioning controls instead of the `IDrawingNode`/`DrawingNodeEditor` operations; a bespoke undo stack where `IUndoRedoHost` owns coalesced history.

[CANVAS_LAW]:
- Package: `NodeEditorAvalonia`
- Owns: the Avalonia layer — the `Editor`/`DrawingNode`/`Node`/`Pin`/`Connector` controls bound to the model graph, the `Xaml.Behaviors` interaction behaviors, `NodeZoomBorder`, the `OrthogonalRouter`+`RTree` routing and hit-test, the ink layer + adorners, and `ExportRenderer`/`StorageService` IO.
- Accept: `Editor.Drawing` binds an `IDrawingNode` and `Editor.Templates` a palette; connector routing is `ConnectorRoutingAlgorithm`/`ConnectorStyle` over `OrthogonalRouter`; pan/zoom is `NodeZoomBorder`.
- Reject: hosting the canvas in a second pan/zoom border; re-implementing connector routing or hit-testing where `OrthogonalRouter`/`RTree`/`HitTestIndex` exist; a parallel export path beside `ExportRenderer`.

# [RASM_APPUI_API_NODEEDITOR]

`NodeEditorAvalonia` owns node/pin/connector graph editing across two assemblies: `NodeEditorAvalonia.Model` mints the framework-agnostic graph contracts, the headless `DrawingNodeEditor` engine, connection policy, and ink; `NodeEditorAvalonia` binds them to Avalonia controls, `Xaml.Behaviors` interaction, adorners, and Skia export.

Every mutation folds through `IDrawingNode` and `DrawingNodeEditor`; every visual folds through the compiled `Themes/` control themes and their variant resource keys.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `NodeEditorAvalonia.Model`
- package: `NodeEditorAvalonia.Model` (MIT)
- assembly: `NodeEditorAvalonia.Model`
- namespace: `NodeEditor.Model`
- asset: managed library carrying no dependency; `lib/net10.0` binds the `net10.0` consumer, `lib/net6.0` and `lib/netstandard2.0` fall back
- rail: graph-editing

[PACKAGE_SURFACE]: `NodeEditorAvalonia`
- package: `NodeEditorAvalonia` (MIT)
- assembly: `NodeEditorAvalonia`
- namespace: `NodeEditor.Controls`, `NodeEditor.Behaviors`, `NodeEditor.Converters`, `NodeEditor.Services`
- asset: managed library with compiled AXAML under `avares://NodeEditorAvalonia/Themes/`; `lib/net10.0` binds the consumer, `lib/net8.0` falls back
- depends: `NodeEditorAvalonia.Model`, `Avalonia`, `Avalonia.Skia`, `Avalonia.Controls.PanAndZoom`, `Avalonia.Xaml.Behaviors`
- rail: graph-editing

## [02]-[PUBLIC_TYPES]

[GRAPH_CONTRACTS]: `NodeEditor.Model` graph algebra, editing engine, and validation rail

| [INDEX] | [SYMBOL]                       | [TYPE_FAMILY] | [CAPABILITY]                                             |
| :-----: | :----------------------------- | :------------ | :------------------------------------------------------- |
|  [01]   | `INode`                        | interface     | graph node: geometry, rotation, content, pins, lifecycle |
|  [02]   | `IDrawingNode : INode`         | interface     | root canvas node, command host, editing operations       |
|  [03]   | `IConnector`                   | interface     | graph edge: endpoints, routing, arrows, waypoints        |
|  [04]   | `IPin`                         | interface     | node port: geometry, alignment, connection lifecycle     |
|  [05]   | `IConnectablePin`              | interface     | read-only direction and bus-width typing                 |
|  [06]   | `IDrawingNodeSettings`         | interface     | connection, ink, snap, grid, guide, and routing policy   |
|  [07]   | `IDrawingNodeFactory`          | interface     | pin, connector, and list factory                         |
|  [08]   | `INodeFactory`                 | interface     | template roster and drawing factory                      |
|  [09]   | `INodeSerializer`              | interface     | generic round-trip serializer                            |
|  [10]   | `INodeTemplate`                | interface     | palette template: title, template, preview               |
|  [11]   | `INodeTemplatesHost`           | interface     | template-collection host                                 |
|  [12]   | `IUndoRedoHost`                | interface     | coalesced undo history                                   |
|  [13]   | `IEditor`                      | interface     | drawing and template binding pair                        |
|  [14]   | `DrawingNodeEditor`            | class         | headless editing and validation engine                   |
|  [15]   | `ConnectionValidationContext`  | struct        | drawing, start, and end validation payload               |
|  [16]   | `ConnectionValidationHandler`  | delegate      | `bool` gate over the validation context                  |
|  [17]   | `SelectionChangedEventHandler` | delegate      | selection-change signal                                  |

[INode]: `Name` `Parent : INode?` `X` `Y` `Width` `Height` `Rotation` `Content : object?` `Pins : IList<IPin>?` `IsVisible` `IsLocked`
[INode.ops]: `CanSelect()` `CanRemove()` `CanMove()` `CanResize()` `Move(dx, dy)` `Resize(dx, dy, NodeResizeDirection)` `On{Created,Removed,Moved,Selected,Deselected,Resized}()`
[IDrawingNode]: `Nodes : IList<INode>?` `Connectors : IList<IConnector>?` `InkStrokes : IList<InkStroke>?` `Settings : IDrawingNodeSettings` + events `SelectionChanged` `ConnectionRejected`
[IConnector]: `Name` `Parent : IDrawingNode?` `Orientation : ConnectorOrientation` `Style : ConnectorStyle` `RoutingMode : ConnectorRoutingMode` `StartArrow`/`EndArrow : ConnectorArrowStyle`
[IConnector.geometry]: `Start`/`End : IPin?` `Offset : double` `Waypoints : IList<ConnectorPoint>` `IsVisible` `IsLocked` `CanSelect()` `CanRemove()` `On{Created,Removed,Selected,Deselected,StartChanged,EndChanged}()`
[IPin]: `Name` `Parent : INode?` `X` `Y` `Width` `Height` `Alignment : PinAlignment` `CanConnect()` `CanDisconnect()` `On{Created,Removed,Moved,Selected,Deselected,Resized,Connected,Disconnected}()`
[IConnectablePin]: `Direction : PinDirection { get; }` `BusWidth : int { get; }` — a product pin implements this beside `IPin` to admit directional and bus-width policy
[IDrawingNodeSettings.connection]: `EnableConnections` `RequireDirectionalConnections` `RequireMatchingBusWidth` `EnableMultiplePinConnections` `AllowSelfConnections` `AllowDuplicateConnections` `ConnectionValidator : ConnectionValidationHandler?`
[IDrawingNodeSettings.ink]: `EnableInk` `IsInkMode` `InkPens : IList<InkPen>?` `ActivePen : InkPen?`
[IDrawingNodeSettings.lattice]: `EnableSnap` `SnapX` `SnapY` `NudgeStep` `NudgeMultiplier` `EnableGrid` `GridCellWidth` `GridCellHeight` `EnableGuides` `GuideSnapTolerance`
[IDrawingNodeSettings.routing]: `EnableConnectorRouting` `RoutingGridSize` `RoutingObstaclePadding` `RoutingAlgorithm : ConnectorRoutingAlgorithm` `RoutingBendPenalty` `RoutingDiagonalCost` `RoutingCornerRadius` `RoutingMaxCells : int` `DefaultConnectorStyle : ConnectorStyle`
[IDrawingNodeFactory]: `CreatePin() : IPin` `CreateConnector() : IConnector` `CreateList<T>() : IList<T>`
[INodeFactory]: `CreateTemplates() : IList<INodeTemplate>` `CreateDrawing(string?) : IDrawingNode`
[INodeTemplate]: `Title : string?` `Template : INode?` `Preview : INode?`
[IUndoRedoHost]: `CanUndo` `CanRedo` `Undo()` `Redo()` `BeginUndoBatch()` `EndUndoBatch()`
[IEditor]: `Drawing : IDrawingNode?` `Templates : IList<INodeTemplate>?`
[ConnectionValidationContext]: `Drawing : IDrawingNode` `Start : IPin` `End : IPin` — `ConnectionValidationHandler` returns `bool` over this struct

[GRAPH_VOCABULARY]: `NodeEditor.Model` bounded case sets (enums)

| [INDEX] | [SYMBOL]                    | [TYPE_FAMILY] | [CAPABILITY]                            |
| :-----: | :-------------------------- | :------------ | :-------------------------------------- |
|  [01]   | `ConnectorRoutingMode`      | enum          | per-connector manual or automatic route |
|  [02]   | `ConnectorRoutingAlgorithm` | enum          | routing lattice selection               |
|  [03]   | `ConnectorStyle`            | enum          | connector path shape                    |
|  [04]   | `ConnectorOrientation`      | enum          | control-point projection axis           |
|  [05]   | `ConnectorArrowStyle`       | enum          | endpoint arrowhead                      |
|  [06]   | `PinAlignment`              | enum          | pin-edge attachment                     |
|  [07]   | `PinDirection`              | enum          | IO direction                            |
|  [08]   | `NodeAlignment`             | enum          | selection alignment axis                |
|  [09]   | `NodeDistribution`          | enum          | selection distribution axis             |
|  [10]   | `NodeOrder`                 | enum          | z-order move                            |
|  [11]   | `NodeResizeDirection`       | enum          | resize handle                           |

[ConnectorRoutingMode]: `Manual` `Auto`
[ConnectorRoutingAlgorithm]: `Auto` `Orthogonal` `Octilinear`
[ConnectorStyle]: `Bezier` `Straight` `Orthogonal`
[ConnectorOrientation]: `Auto` `Horizontal` `Vertical`
[ConnectorArrowStyle]: `None` `Arrow` `Circle` `Diamond`
[PinAlignment]: `None` `Left` `Right` `Top` `Bottom`
[PinDirection]: `Input` `Output` `Bidirectional`
[NodeAlignment]: `Left` `Center` `Right` `Top` `Middle` `Bottom`
[NodeDistribution]: `Horizontal` `Vertical`
[NodeOrder]: `BringToFront` `SendToBack` `BringForward` `SendBackward`
[NodeResizeDirection]: `Top` `Bottom` `Left` `Right` `TopLeft` `TopRight` `BottomLeft` `BottomRight`

[GRAPH_CARRIERS]: `NodeEditor.Model` data carriers and the typed event-args family

| [INDEX] | [SYMBOL]                                                           | [TYPE_FAMILY] | [CAPABILITY]                               |
| :-----: | :----------------------------------------------------------------- | :------------ | :----------------------------------------- |
|  [01]   | `ConnectorPoint`                                                   | class         | observable connector waypoint              |
|  [02]   | `InkPen`                                                           | class         | ink pen preset                             |
|  [03]   | `InkPoint`                                                         | class         | pressure- and time-stamped ink sample      |
|  [04]   | `InkStroke`                                                        | class         | sampled stroke with its paint              |
|  [05]   | `InkShape`                                                         | class         | single-stroke shape carrier                |
|  [06]   | `ConnectorExtensions`                                              | static class  | control-point projection over `IConnector` |
|  [07]   | `Node{Created,Removed,Moved,Resized,Selected,Deselected}EventArgs` | class         | node lifecycle payloads                    |
|  [08]   | `Pin{Created,Removed,Moved,Resized,Selected,Deselected}EventArgs`  | class         | pin lifecycle payloads                     |
|  [09]   | `Pin{Connected,Disconnected}EventArgs`                             | class         | pin connection payloads                    |
|  [10]   | `Connector{Created,Removed,Selected,Deselected}EventArgs`          | class         | connector lifecycle payloads               |
|  [11]   | `Connector{Start,End}ChangedEventArgs`                             | class         | endpoint rebind payloads                   |
|  [12]   | `ConnectionRejectedEventArgs`                                      | class         | rejection carrying `Start` and `End` pins  |

[ConnectorPoint]: `X : double` `Y : double` + `INotifyPropertyChanged`; `ConnectorPoint()` / `ConnectorPoint(x, y)`
[InkPen]: `Id : string` `Name : string` `Color : uint` `Thickness : double` `Opacity : double` — `Color` is packed ARGB, never an Avalonia `Color`
[InkPoint]: `X` `Y` `Pressure` `Timestamp : long`; `InkPoint(x, y, pressure = 1.0, timestamp = 0)`
[InkStroke]: `Points : IList<InkPoint>` `Color : uint` `Thickness : double` `Opacity : double` `Name : string?`
[InkShape]: `Stroke : InkStroke`
[ConnectorExtensions]: `GetControlPoints(this IConnector, ConnectorOrientation, offset, PinAlignment, PinAlignment, ref p1X, ref p1Y, ref p2X, ref p2Y)`
[EVENT_PAYLOAD]: every `Node*`/`Pin*`/`Connector*EventArgs` carries its subject alone; `*Moved` adds `X`/`Y` and `*Resized` adds `X`/`Y`/`Width`/`Height`

[CANVAS_CONTROLS]: `NodeEditor.Controls` Avalonia layer — every graph binding rides a `*Source` styled property

| [INDEX] | [SYMBOL]                    | [TYPE_FAMILY] | [CAPABILITY]                                        |
| :-----: | :-------------------------- | :------------ | :-------------------------------------------------- |
|  [01]   | `Editor`                    | class         | top-level templated host                            |
|  [02]   | `DrawingNode`               | class         | canvas host                                         |
|  [03]   | `DrawingNodeProperties`     | class         | two-way settings editor over `IDrawingNodeSettings` |
|  [04]   | `Node`                      | class         | node content host, `:selected` pseudo-class         |
|  [05]   | `Nodes`                     | class         | nodes panel                                         |
|  [06]   | `Pin`                       | class         | pin control, initiates connector drag               |
|  [07]   | `Pins`                      | class         | pins panel                                          |
|  [08]   | `Connector`                 | class         | connector `Shape`                                   |
|  [09]   | `Connectors`                | class         | connectors panel                                    |
|  [10]   | `NodeZoomBorder`            | class         | pan-and-zoom viewport with command methods          |
|  [11]   | `Toolbox`                   | class         | node-template palette                               |
|  [12]   | `InkLayer`                  | class         | ink capture and render overlay                      |
|  [13]   | `InkStrokePresenter`        | class         | single-stroke presenter                             |
|  [14]   | `GridDecorator`             | class         | grid overlay                                        |
|  [15]   | `GuideLine`                 | struct        | alignment-guide segment                             |
|  [16]   | `GuidesAdorner`             | class         | alignment-guide overlay                             |
|  [17]   | `SelectionAdorner`          | class         | rubber-band overlay                                 |
|  [18]   | `SelectedAdorner`           | class         | selected-bounds overlay                             |
|  [19]   | `ConnectorSelectedAdorner`  | class         | selected-connector overlay                          |
|  [20]   | `ConnectorCrossingsAdorner` | class         | connector-crossing hop overlay                      |
|  [21]   | `EditableTextBlock`         | class         | inline rename label                                 |
|  [22]   | `ExportRoot`                | class         | export render root                                  |

[Editor]: `DrawingSource : IDrawingNode?` `InputSource : Control?` `AdornerCanvas : Canvas?` `ZoomControl : NodeZoomBorder?` — `OnApplyTemplate` fills `ZoomControl` from `PART_ZoomBorder` and `AdornerCanvas` from `PART_AdornerCanvas`
[DrawingNode]: `DrawingSource : IDrawingNode?` `InputSource : Control?` `AdornerCanvas : Canvas?`
[Nodes]: `DrawingSource : IDrawingNode?` `InputSource : Control?` `AdornerCanvas : Canvas?`
[Node]: `NodeSource : INode?`
[Pin]: `PinSource : IPin?` `Alignment : PinAlignment` `Direction : PinDirection` (default `Bidirectional`) `BusWidth : int` (default `1`) `Id : string?`
[Pins]: `NodeSource : INode?`
[Connector]: `ConnectorSource : IConnector?` `StartPoint : Point` `EndPoint : Point` `Offset : double` `ConnectorStyle : ConnectorStyle` `Orientation : ConnectorOrientation` `LabelPoint : Point` `SwapDirectionCommand : ICommand`
[Connectors]: `DrawingSource : IDrawingNode?`
[Toolbox]: `TemplatesSource : IEnumerable<INodeTemplate>?` `DrawingSource : IDrawingNode?`
[InkLayer]: `DrawingSource : IDrawingNode?` — captures pointer strokes into `IDrawingNode.InkStrokes` when `Settings.IsInkMode`
[InkStrokePresenter]: `Stroke : InkStroke?`
[GridDecorator]: `EnableGrid : bool` `GridCellWidth : double` `GridCellHeight : double` — both cells default `0.0`, so a grid renders only under explicit sizing
[GuideLine]: `Start : Point` `End : Point`
[GuidesAdorner]: `Guides : IReadOnlyList<GuideLine>?` `Stroke : IBrush?` `StrokeThickness : double` (default `1.0`)
[SelectionAdorner]: `TopLeft : Point` `BottomRight : Point` `GetRect() : Rect`
[SelectedAdorner]: `Rect : Rect`
[ConnectorSelectedAdorner]: `Connectors : IReadOnlyList<IConnector>?` `Stroke : IBrush?` `StrokeThickness : double` (default `2.0`)
[ConnectorCrossingsAdorner]: `Connectors : IReadOnlyList<IConnector>?` `Stroke : IBrush?` `Background : IBrush?` `StrokeThickness : double` (default `2.0`) `ArcRadius : double` (default `6.0`)
[EditableTextBlock]: `Text : string?` (two-way) `Placeholder : string?` `IsEditing : bool` `AcceptsReturn : bool` `TextWrapping : TextWrapping` `TextAlignment : TextAlignment`
[DrawingNodeProperties]: two-way styled properties mirror `IsInkMode`, the connection flags, the whole lattice and routing set, `DrawingWidth`, and `DrawingHeight`
[DrawingNodeProperties.gap]: `ConnectionValidator` `EnableInk` `InkPens` `ActivePen` `DefaultConnectorStyle` carry no control property, so a view-model binds them on `IDrawingNode.Settings` directly

[CANVAS_INTERACTION]: `NodeEditor.Behaviors` gestures and drop rails over `Avalonia.Xaml.Behaviors`, and `NodeEditor.Converters` binding adapters

| [INDEX] | [SYMBOL]                                                                  | [TYPE_FAMILY]  | [CAPABILITY]                            |
| :-----: | :------------------------------------------------------------------------ | :------------- | :-------------------------------------- |
|  [01]   | `Drawing{Pressed,Moved,Released,Selection}Behavior`                       | class          | canvas pointer state machine            |
|  [02]   | `ConnectorInteractionBehavior`                                            | class          | connector pick, drag, and waypoints     |
|  [03]   | `ConnectionFeedbackBehavior`                                              | class          | rejection stroke and label overlay      |
|  [04]   | `PinPressedBehavior`                                                      | class          | pin-initiated connector drag            |
|  [05]   | `Node{Resize,Rotate}Behavior`                                             | class          | thumb-driven resize and rotate          |
|  [06]   | `{Nodes,Connectors}SelectedBehavior`                                      | class          | selection adorner projection            |
|  [07]   | `InsertTemplateOnDoubleTappedBehavior`                                    | class          | palette double-tap insert               |
|  [08]   | `ToolboxDragBehavior`                                                     | class          | palette drag initiation                 |
|  [09]   | `IDrawingDropTarget`                                                      | interface      | text and file drop contract             |
|  [10]   | `DefaultDropHandler`                                                      | abstract class | `IDropHandler` base                     |
|  [11]   | `DrawingDropHandler`                                                      | class          | canvas drop onto a drawing              |
|  [12]   | `TemplatesListBoxDropHandler`                                             | class          | template-list reorder drop              |
|  [13]   | `PinToPointConverter` / `PinMarginConverter` / `ColumnWidthConverter`     | class          | geometry binding adapters               |
|  [14]   | `EnumToCheckedConverter` / `EnumEqualsConverter`                          | class          | enum binding adapters                   |
|  [15]   | `NullToBoolConverter` / `BoolInvertConverter` / `StringNotEmptyConverter` | class          | predicate binding adapters              |
|  [16]   | `PlaceholderVisibilityConverter`                                          | class          | `IMultiValueConverter` placeholder gate |

[Drawing{Pressed,Moved}Behavior]: `DrawingSource : IDrawingNode?` `InputSource : Control?`
[DrawingReleasedBehavior]: `DrawingSource : IDrawingNode?` `InputSource : Control?` `PinHitTolerance : double`
[DrawingSelectionBehavior]: `DrawingSource : IDrawingNode?` `InputSource : Control?` `AdornerCanvas : Canvas?`
[ConnectionFeedbackBehavior]: `DrawingSource` `AdornerCanvas` `RejectionStroke : IBrush?` `LabelBackground` `LabelBorderBrush` `LabelForeground` `LabelText : string?`
[NodeResizeBehavior]: `NodeSource : INode?` `Direction : NodeResizeDirection`
[NodeRotateBehavior]: `NodeSource : INode?` `AngleReadoutBackground` `AngleReadoutBorderBrush` `AngleReadoutForeground`
[ToolboxDragBehavior]: `DragThreshold : double`
[IDrawingDropTarget]: `CanDropText(string, Point)` `DropText(string, Point)` `CanDropFiles(IReadOnlyList<IStorageItem>, Point)` `DropFiles(IReadOnlyList<IStorageItem>, Point)`
[DrawingDropHandler]: `DrawingSource : IDrawingNode?` `RelativeTo : Control?`

[CANVAS_SERVICES]: `NodeEditor.Services` IO and export

| [INDEX] | [SYMBOL]         | [TYPE_FAMILY] | [CAPABILITY]                                  |
| :-----: | :--------------- | :------------ | :-------------------------------------------- |
|  [01]   | `StorageService` | static class  | `FilePickerFileType` presets, provider lookup |
|  [02]   | `ExportRenderer` | static class  | Skia raster and vector control export         |

## [03]-[ENTRYPOINTS]

[DRAWING_OPERATIONS]: `IDrawingNode` editing surface — the model implements each against `DrawingNodeEditor`

| [INDEX] | [SURFACE]                                             | [SHAPE]  | [CAPABILITY]                 |
| :-----: | :---------------------------------------------------- | :------- | :--------------------------- |
|  [01]   | `CutNodes()`                                          | instance | clipboard cut                |
|  [02]   | `CopyNodes()`                                         | instance | clipboard copy               |
|  [03]   | `PasteNodes()`                                        | instance | clipboard paste              |
|  [04]   | `DuplicateNodes()`                                    | instance | selection duplication        |
|  [05]   | `DeleteNodes()`                                       | instance | selection deletion           |
|  [06]   | `AlignSelectedNodes(NodeAlignment)`                   | instance | selection alignment          |
|  [07]   | `DistributeSelectedNodes(NodeDistribution)`           | instance | selection distribution       |
|  [08]   | `OrderSelectedNodes(NodeOrder)`                       | instance | selection z-order            |
|  [09]   | `LockSelection()` / `UnlockSelection()`               | instance | selection lock toggle        |
|  [10]   | `HideSelection()` / `ShowSelection()`                 | instance | selection visibility toggle  |
|  [11]   | `ShowAll()`                                           | instance | reveal every hidden item     |
|  [12]   | `SelectAllNodes()` / `DeselectAllNodes()`             | instance | selection sweep              |
|  [13]   | `SetSelectedNodes(ISet<INode>?)`                      | instance | node selection setter        |
|  [14]   | `GetSelectedNodes()`                                  | instance | node selection getter        |
|  [15]   | `SetSelectedConnectors(ISet<IConnector>?)`            | instance | connector selection setter   |
|  [16]   | `GetSelectedConnectors()`                             | instance | connector selection getter   |
|  [17]   | `DrawingLeftPressed(x, y)`                            | instance | left-press transition        |
|  [18]   | `DrawingRightPressed(x, y)`                           | instance | right-press transition       |
|  [19]   | `ConnectorLeftPressed(IPin, bool showWhenMoving)`     | instance | connector-drag start         |
|  [20]   | `ConnectorMove(x, y)`                                 | instance | connector-drag move          |
|  [21]   | `CancelConnector()`                                   | instance | connector-drag cancel        |
|  [22]   | `CanConnectPin(IPin)`                                 | instance | connection gate              |
|  [23]   | `IsPinConnected(IPin)`                                | instance | connection state             |
|  [24]   | `IsConnectorMoving()`                                 | instance | drag state                   |
|  [25]   | `CanSelectNodes()` / `CanSelectConnectors()`          | instance | selection gates              |
|  [26]   | `Clone<T>(T source)`                                  | instance | serializer-backed deep clone |
|  [27]   | `NotifyConnectionRejected(IPin, IPin)`                | instance | rejection event              |
|  [28]   | `NotifySelectionChanged()`                            | instance | selection event              |
|  [29]   | `NotifyDeselectedNodes()`                             | instance | node deselection event       |
|  [30]   | `NotifyDeselectedConnectors()`                        | instance | connector deselection event  |
|  [31]   | `GetSerializer()` / `SetSerializer(INodeSerializer?)` | instance | serializer accessors         |

[BOUND_COMMANDS]: every operation above also rides a get-only `ICommand` for XAML binding — `{Cut,Copy,Paste,Duplicate,Delete}NodesCommand` `{Align,Distribute,Order}NodesCommand` `{SelectAll,DeselectAll}NodesCommand` `{Lock,Unlock,Hide,Show}SelectionCommand` `ShowAllCommand` `UndoCommand` `RedoCommand` `DrawInkCommand` `ConvertInkCommand` `AddPenCommand` `ClearInkCommand`

[ENGINE_OPERATIONS]: `DrawingNodeEditor` headless engine — host-free editing, selection, and validation

| [INDEX] | [SURFACE]                                                               | [SHAPE]  | [CAPABILITY]                     |
| :-----: | :---------------------------------------------------------------------- | :------- | :------------------------------- |
|  [01]   | `new DrawingNodeEditor(IDrawingNode node, IDrawingNodeFactory factory)` | ctor     | engine binding                   |
|  [02]   | `Clone<T>(T source) : T?`                                               | instance | serializer round-trip deep clone |
|  [03]   | `CanConnectPin(IPin)` / `IsPinConnected(IPin)`                          | instance | connection gate and state        |
|  [04]   | `IsConnectorMoving()` / `CancelConnector()`                             | instance | connector-drag state and cancel  |
|  [05]   | `CanSelectNodes()` / `CanSelectConnectors()`                            | instance | selection gates                  |
|  [06]   | `DrawingLeftPressed(x, y)` / `DrawingRightPressed(x, y)`                | instance | canvas press transitions         |
|  [07]   | `ConnectorLeftPressed(IPin, bool)` / `ConnectorMove(x, y)`              | instance | connector-drag drive             |
|  [08]   | `{Cut,Copy,Paste,Duplicate,Delete}Nodes()`                              | instance | clipboard and deletion           |
|  [09]   | `AlignSelectedNodes` / `DistributeSelectedNodes` / `OrderSelectedNodes` | instance | selection layout                 |
|  [10]   | `{Lock,Unlock,Hide,Show}Selection()` / `ShowAll()`                      | instance | selection lock and visibility    |
|  [11]   | `SelectAllNodes()` / `DeselectAllNodes()`                               | instance | selection sweep                  |

- `DrawingNodeEditor.Clone<T>`: routes through the drawing's `INodeSerializer`, so paste and duplication return `null` while no serializer is set.
- `DrawingNodeEditor.CanConnectPin`: folds `IDrawingNodeSettings` policy with the `ConnectionValidator` delegate; a refusal raises `ConnectionRejected` carrying both pins.

[EXPORT_PERSIST]: `ExportRenderer`, `StorageService`, and `INodeSerializer`

| [INDEX] | [SURFACE]                                                      | [SHAPE]  | [CAPABILITY]                         |
| :-----: | :------------------------------------------------------------- | :------- | :----------------------------------- |
|  [01]   | `ExportRenderer.RenderPng(Control, Size, Stream, dpi = 96)`    | static   | PNG export                           |
|  [02]   | `ExportRenderer.RenderSvg(Control, Size, Stream, dpi = 96)`    | static   | SVG export                           |
|  [03]   | `ExportRenderer.RenderSkp(Control, Size, Stream, dpi = 96)`    | static   | SKP export                           |
|  [04]   | `ExportRenderer.RenderPdf(Control, Size, Stream, dpi = 72)`    | static   | PDF export                           |
|  [05]   | `ExportRenderer.RenderXps(Control, Size, Stream, dpi = 72)`    | static   | XPS export                           |
|  [06]   | `StorageService.GetStorageProvider()`                          | static   | lifetime-agnostic `IStorageProvider` |
|  [07]   | `StorageService.{All,Json,ImageSvg,ImagePng,ImageSkp,Pdf,Xps}` | static   | `FilePickerFileType` presets         |
|  [08]   | `INodeSerializer.Serialize<T>(T) : string`                     | instance | graph serializer                     |
|  [09]   | `INodeSerializer.Deserialize<T>(string) : T?`                  | instance | graph deserializer                   |

- `ExportRenderer.Render*`: measures and arranges the target itself, so an unrealized control exports at its measured size; `RenderXps` rides `SKDocument.CreateXps`, a Windows-only backend.
- `StorageService.GetStorageProvider`: resolves the desktop main window first, then the single-view top level, and yields `null` outside both.

[THEMING]: `avares://NodeEditorAvalonia/Themes/` compiled AXAML — the only visual surface

| [INDEX] | [SURFACE]                                                     | [SHAPE] | [CAPABILITY]                                          |
| :-----: | :------------------------------------------------------------ | :------ | :---------------------------------------------------- |
|  [01]   | `avares://NodeEditorAvalonia/Themes/NodeEditorTheme.axaml`    | static  | the `Styles` root a host adds to `Application.Styles` |
|  [02]   | `avares://NodeEditorAvalonia/Themes/Controls/<Control>.axaml` | static  | one `ResourceDictionary` per control theme            |

- `NodeEditorTheme.axaml`: carries a `ResourceDictionary` whose `ThemeDictionaries` maps `ThemeVariant.Default` and `ThemeVariant.Dark` onto sibling `IThemeVariantProvider` dictionaries, merges the eleven control dictionaries, and adds the `Style` rows binding adorner properties to the keys below.
- `Themes/Controls/`: `Connector` `Connectors` `DrawingNode` `DrawingNodeProperties` `EditableTextBlock` `Editor` `Node` `Nodes` `Pin` `Pins` `Toolbox` — each dictionary keys one `ControlTheme` on `typeof(<Control>)`, so the theme applies implicitly and a host override supplies its own `ControlTheme` under the same type key.
- Overriding a visual replaces the `ControlTheme` keyed on that control type or redefines the resource key in a dictionary composed after this theme; no code-level node, pin, or connector colour property exists, and `Connector` derives `Shape`, so `Stroke` and `StrokeThickness` reach it only through a style or theme setter.

[VARIANT_KEYS]: each key below resolves per `ThemeVariant.Default` and `ThemeVariant.Dark`; brushes unless noted
[PIN_KEYS]: `PinBackgroundBrush` `PinPointerOverBackgroundBrush` `PinForegroundBrush` `PinPointerOverForegroundBrush`
[CONNECTOR_KEYS]: `ConnectorBackgroundBrush` `ConnectorLabelBackgroundBrush` `ConnectorLabelBorderBrush`
[CROSSING_KEYS]: `ConnectorCrossingStrokeBrush` `ConnectorCrossingBackgroundBrush` `ConnectorCrossingStrokeThickness` (`double`) `ConnectorCrossingArcRadius` (`double`)
[REJECTION_KEYS]: `ConnectionRejectedBrush` `ConnectionRejectedLabelBackgroundBrush` `ConnectionRejectedLabelBorderBrush` `ConnectionRejectedLabelForegroundBrush` `ConnectionRejectedLabelText` (`string`)
[NODE_KEYS]: `NodeResizeHandleFillBrush` `NodeResizeHandleBorderBrush` `RotationSnapReadoutBackgroundBrush` `RotationSnapReadoutBorderBrush` `RotationSnapReadoutForegroundBrush`
[GUIDE_KEYS]: `GuideLineBrush` — the theme binds it to both `GuidesAdorner.Stroke` and `ConnectorSelectedAdorner.Stroke`, so a selected-connector recolour splits the key first
[HOST_KEYS]: consumed by the templates yet defined nowhere in the package, so the host supplies each — `EditorBackground` `DrawingBackground` `EditorCutIcon` `EditorCopyIcon` `EditorPasteIcon` `DeleteIcon`

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `NodeEditorAvalonia.Model` owns the graph algebra host-free and `NodeEditorAvalonia` binds it to Avalonia. Every mutation folds through the `IDrawingNode` command surface and the `DrawingNodeEditor` engine, never a repositioned control; connection policy rides `IDrawingNodeSettings` with `IConnectablePin.Direction`/`BusWidth` typing the ports, and every visual rides a `ControlTheme` or a variant resource key.
- Routing, hit-testing, spatial indexing, snapping, and connector-path geometry are internal to `NodeEditorAvalonia`; a consumer steers them through `IDrawingNodeSettings.Routing*` and the lattice knobs, never by calling the algorithms.

[STACKING]:
- `ReactiveUI.Avalonia`(`api-reactiveui-avalonia.md`): product view-models implement `IDrawingNode`/`INode`/`IConnector`/`IPin` as `ReactiveObject`-backed models, and each `On*` notifier raises from the reactive setter so `Connector.ConnectorSource` and `Node.NodeSource` bindings track state.
- `Avalonia.Skia`(`api-avalonia-skia.md`) and `SkiaSharp`(`api-skiasharp.md`): `ExportRenderer` renders through `Avalonia.Skia.Helpers.DrawingContextHelper.RenderAsync` onto an `SKCanvas`, `RenderPdf`/`RenderXps` wrapping it in `SKDocument.CreatePdf`/`CreateXps` pages.
- `Avalonia.Xaml.Behaviors`(`api-behaviors.md`): every interaction rides `Behavior<T>`, and `DrawingDropHandler` extends the `IDropHandler` rail `ContextDropBehavior.Handler` consumes.
- `Loro`(`api-loro.md`): the `LoroTree` co-edit bridge projects graph mutation bidirectionally under one echo-suppressed handshake — `EventTriggerKind.Local` commits a local UI mutation as tree ops without re-applying its own echo, `Import` applies a remote merge to the model without re-emitting.
- `ACadSharp`/`PDFsharp`(`api-drafting-export.md`, `api-pdfsharp.md`): `ExportRenderer.RenderPdf`/`RenderSvg` canvas output converges with the drafting and PDF export set on the shared vector-export rail.
- within-lib: the parametric and dependency-graph editing surfaces drive every mutation through the `IDrawingNode` commands and the `DrawingNodeEditor` engine; `StorageService` supplies the matching `FilePickerFileType` presets, and `INodeSerializer` binds one serializer for clipboard, duplication, and persistence alike.

[LOCAL_ADMISSION]:
- The collision is a package-id RENAME, not a namespace clash: this package's nuspec declares the legacy `Avalonia.Controls.PanAndZoom` id, whose `Avalonia.Controls.PanAndZoom.dll` publishes `Avalonia.Controls.PanAndZoom.ZoomBorder`, and the admitted `PanAndZoom`(`api-panandzoom.md`) `PanAndZoom.dll` publishes the same full type name — so a plain `ZoomBorder` mention is `CS0433`. The resolution is `Aliases` metadata on the LEGACY `PackageReference` (with its central `PackageVersion` row): `ExcludeAssets` is unusable because `NodeZoomBorder` inherits the type it removes, aliasing the pinned package instead lifts every `PanAndZoom` type out of global scope so existing `ZoomBorder` mentions silently rebind the legacy type and only the absent members fault, and a source `extern alias` cannot repair the Avalonia name generator's own emitted partial, which names the type unqualified.
- `NodeZoomBorder` derives the LEGACY `ZoomBorder` and adds seven parameterless command shims and nothing else — no property, field, or override — so `SavedView`, view history, discrete zoom, grid, rotation, and `ExportState` are unreachable through it; `Editor` fills `ZoomControl` from `PART_ZoomBorder` with that type, so a host wanting the pinned viewport mounts `DrawingNode` inside `PanAndZoom` `ZoomBorder` and skips `Editor`.
- `NodeZoomBorder` shims: `ResetZoomCommand()` -> `ResetMatrix()`; `ZoomToCommand(object?)` parses a string factor, resets, then zooms about the child bounds centre; `ZoomInCommand()`/`ZoomOutCommand()` -> `ZoomIn`/`ZoomOut`; `FitCanvasCommand()` -> `Uniform`; `FitToFillCommand()` -> `UniformToFill`; `FillCanvasCommand()` -> `Fill`.
- `InkLayer` renders `IDrawingNode.InkStrokes` UNCONDITIONALLY and captures only while `Settings.EnableInk && Settings.IsInkMode`, so a host driving its own stroke fold leaves ink mode false and keeps the layer as renderer alone. Its capture writes a constant `1.0` pressure and reads `GetCurrentPoint`, discarding the coalesced burst; `RenderStroke` builds ONE `ImmutablePen` at `Math.Max(0.5, stroke.Thickness)` for the whole stroke and reads `InkPoint.Pressure` nowhere, so a pressure-varying line renders as a per-run stroke sequence or not at all. Capture wraps `BeginUndoBatch`/`EndUndoBatch` when the drawing implements `IUndoRedoHost`.
- `NodeEditor.SnapHelper` is `internal static` and unreachable: `Snap(double value, double snap)` returns `value` when `Math.Abs(snap) <= 0.0` and otherwise `Math.Round(value / Math.Abs(snap), MidpointRounding.AwayFromZero) * Math.Abs(snap)`, and `Snap(Point, snapX, snapY, enabled)` applies it PER AXIS. A host committing its own coordinates transcribes that body to land on the lattice the interactive drag quantizes to; calling it, or approximating with `Math.Round` at the default banker's midpoint, converges two peers to two positions for one gesture.
- Host resources resolve `EditorBackground`, `DrawingBackground`, and the four icon keys before `NodeEditorTheme.axaml` composes; an unresolved `DynamicResource` renders that chrome blank rather than faulting.

[RAIL_LAW]:
- Package: `NodeEditorAvalonia.Model`
- Owns: the framework-agnostic graph core — `IDrawingNode`/`INode`/`IConnector`/`IPin`/`IConnectablePin`, the `DrawingNodeEditor` engine, `IDrawingNodeSettings` policy, `INodeSerializer` round-trip, `IUndoRedoHost`, the `INodeTemplate`/`INodeFactory` palette contracts, the ink carriers, the enums, and the typed event-args family
- Accept: product models implement the contracts on the ReactiveUI rail; editing drives through the `IDrawingNode` operations and their bound `ICommand` twins; connection rules ride `IDrawingNodeSettings` with `IConnectablePin` typing the ports and `ConnectionValidator` carrying domain refusals
- Reject: hand-rolling a node, pin, or connector graph type the model owns; mutating the graph by repositioning controls instead of the `IDrawingNode` operations; a bespoke undo stack where `IUndoRedoHost` owns coalesced history; a second clone path beside `Clone<T>` over `INodeSerializer`
- Package: `NodeEditorAvalonia`
- Owns: the Avalonia layer — the `*Source`-bound controls, the `Xaml.Behaviors` interaction set, `NodeZoomBorder`, the ink layer and adorners, `ExportRenderer`/`StorageService` IO, and the compiled `Themes/` control themes with their variant resource keys
- Accept: `Editor.DrawingSource` binds an `IDrawingNode` and `Toolbox.TemplatesSource` a palette; connector shape is `ConnectorStyle` with `IDrawingNodeSettings.RoutingAlgorithm` selecting the lattice; pan and zoom is `NodeZoomBorder`; restyling is a `ControlTheme` keyed on the control type or a redefined resource key
- Reject: hosting the canvas in a second pan-and-zoom border; re-implementing connector routing or hit-testing the package already runs internally; a parallel export path beside `ExportRenderer`; a code-side colour or brush property where the theme owns the visual; `NodeZoomBorder` as the viewport wherever saved views, view history, discrete zoom, the grid, rotation, or state export are wanted

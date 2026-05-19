# [H1][GRASSHOPPER_UI_API]
>**Dictum:** *Local GH2 XML and decompile output define usable UI integration.*

<br>

[IMPORTANT] Source snapshot: RhinoWIP `9.0.26132.12306`. `Grasshopper2.xml`, `GrasshopperIO.xml`, `RhinoCommon.xml`, and Eto XML are available locally. `Rhino.UI.dll` exists, but `Rhino.UI.xml` is missing.

---
## [1][REFERENCE_SOURCES]
>**Dictum:** *Source class determines confidence.*

<br>

| [INDEX] | [SOURCE] | [STATUS] | [USE] |
| :-----: | -------- | :------: | ----- |
| **[1]** | `Grasshopper2.dll` / `Grasshopper2.xml` | Primary | GH2 editor, canvas, document, component, UI, snapping, Eto-backed controls. |
| **[2]** | `GrasshopperIO.dll` / `GrasshopperIO.xml` | Primary | GH2 archive/store/read/write support. |
| **[3]** | `RhinoCommon.dll` / `RhinoCommon.xml` | Primary | Rhino geometry, document, runtime, command, and cross-platform RhinoCommon API. |
| **[4]** | `Eto.dll` / fallback `Eto.xml` | Primary | Cross-platform UI controls, drawing, menus, forms, mouse/key events, bitmap resources. |
| **[5]** | `Rhino.UI.dll` | Decompile | Rhino host UI only. No local XML. Keep outside default GH2 rail. |

---
## [2][IMPORTS]
>**Dictum:** *Imports describe integration classes, not wrapper names.*

<br>

Core usable namespaces:

- `Grasshopper2.UI`, `Grasshopper2.UI.Canvas`, `Grasshopper2.UI.Flex`.
- `Grasshopper2.UI.InputPanel`, `Grasshopper2.UI.Toolbar`, `Grasshopper2.UI.Primitives`.
- `Grasshopper2.UI.Skinning`, `Grasshopper2.UI.Icon`, `Grasshopper2.UI.Snap`.
- `Grasshopper2.Doc`, `Grasshopper2.Doc.Attributes`, `Grasshopper2.Parameters`.
- `Grasshopper2.Extensions` for keyboard and selection-mode helpers.
- `Eto.Drawing`, `Eto.Forms`.
- `Rhino` for `RhinoApp.IsOnMainThread` and `RhinoApp.InvokeAndWait`.

---
## [3][EDITOR]
>**Dictum:** *Editor owns window, active canvas, document set, and host fallback.*

<br>

| [INDEX] | [API] | [CAPABILITY] |
| :-----: | ----- | ------------ |
| **[1]** | `Editor.Instance` | Current editor singleton, nullable before creation. |
| **[2]** | `Editor.ShowEditor(bool, string)` | Create or return editor. |
| **[3]** | `Editor.EnsureVisible()` | Repair hidden or off-screen editor visibility. |
| **[4]** | `Editor.ThisOrRhino` | Editor window or Rhino main window fallback. |
| **[5]** | `Editor.Canvas` | Active canvas control. |
| **[6]** | `Editor.Documents` | Loaded document collection. |
| **[7]** | `Editor.Tabs`, `Editor.BreadCrumbs`, `Editor.StatusBar` | Native editor UI panels. |
| **[8]** | `Editor.BeginRhinoGetter(RhinoDoc)` | Hide editor around Rhino viewport getter. |

---
## [4][CANVAS]
>**Dictum:** *Canvas owns document display, paint, picking, bitmap, and active UI policy.*

<br>

| [INDEX] | [API] | [CAPABILITY] |
| :-----: | ----- | ------------ |
| **[1]** | `Canvas.Document` | Displayed GH2 document. Never returns null per XML. |
| **[2]** | `DocumentChanged`, `DocumentModified` | Canvas document lifecycle events. |
| **[3]** | `AllowedActions` | Mutable `CanvasActions`; mutate only on owned canvases. |
| **[4]** | `Skin`, `SkinLit`, `SkinDim`, `Kind`, `Tiles` | Canvas presentation state. |
| **[5]** | `WindowSelectObjects/Wires/Groups` | Window selection policy flags. |
| **[6]** | `Before/AfterPaintBackground` | Background grid paint hooks. |
| **[7]** | `Before/AfterPaintGroups` | Group paint hooks. |
| **[8]** | `Before/AfterPaintWires` | Wire paint hooks. |
| **[9]** | `Before/AfterPaintObjects` | Object paint hooks. |
| **[10]** | `DrawToBitmap()` / overload | Canvas bitmap render. |
| **[11]** | `DrawPickMap()` | Pick-index map render. |
| **[12]** | `ResolvePick(...)` | Object, grip, background, and wire picking. |
| **[13]** | `DeleteSelection()` | Delete selected objects in current document. |
| **[14]** | `ShowInstantiationPopup(bool, string)` | Native object instantiation popup. |
| **[15]** | `ShowSearchPopup()` | Public stub by decompile. Do not expose as full search rail. |
| **[16]** | `SnapXAction`, `SnapYAction` | Draw currently resolved snapping feedback. |
| **[17]** | `RedrawOnMouseMove` | One-redraw mouse move flag. Reassign per redraw. |
| **[18]** | `ViewportDragging` | Enable direct Rhino viewport geometry dragging from canvas state. |

---
## [5][CANVAS_ACTIONS]
>**Dictum:** *Action policy gates interaction only for canvases Rasm owns.*

<br>

`CanvasActions` exposes `AllowDrag`, `AlloWireSelect`, `AllowObjectSelect`, `AllowMakeWire`, `AllowDeleteWire`, `AllowModifyWire`, `MakeWireFilter`, `DeleteWireFilter`, `AllowMakeObject`, `AllowDeleteObject`, `AllowObjectResponse`, `AllowDropFile`, `AllowWireMenu`, `AllowObjectMenu`, and `AllowCanvasMenu`.

Use this surface as read-only on active GH editor canvases. Mutate it only for custom canvases constructed by Rasm.

---
## [6][DOCUMENT]
>**Dictum:** *Document is GH2 aggregate; methods and objects are its public facades.*

<br>

Document core:

- Creation and lifecycle: `NewInertDocument`, `NewInactiveDocument`, `NewActiveDocument`, `Close`, `Store`, `AllDocuments`.
- State: `Undo`, `NamedViews`, `Projection`, `File`, `Display`, `Dependencies`, `CustomValues`, `Globals`, `Solution`, `Parent`, `State`, `Modified`, `Modify`, `Unmodify`.
- Facades: `Document.Methods` and `Document.Objects`.

Internal or avoided: `Document.References`, private constructors, and lifecycle internals.

---
## [7][DOCUMENT_METHODS]
>**Dictum:** *DocumentMethods is preferred mutation facade.*

<br>

Public mutation capabilities:

- Object add/migrate: `DropObject`, `MigrateObjects`, `AddDependency`.
- Wire insertion: `SplitWire`.
- Delete/data: `DeleteSelection`, `DeleteObjects`, `DeleteSelectionData`, `DeleteObjectData`.
- Clipboard: `CopySelection`, `CutSelection`, `PasteFromClipboard`, `PasteGrasshopper1XmlFromClipboard`.
- Grouping: `GroupSelection`, `GroupObjects`, `ChainSelection`, `ChainObjects`, `ClusterSelection`, `ClusterObjects`.
- Selection: `SelectAll`, `DeselectAll`, `InvertSelection`, `ShiftSelection`, `GrowSelection`.
- State: `EnableSelected/Objects`, `DisableSelected/Objects`, `ShowSelected/Objects`, `HideSelected/Objects`, `ToggleDisplaySelected/Objects`.
- Style/visibility: `SetColourOverrideSelected/Objects`, `ShowSelectedInputs/Outputs`, `HideSelectedInputs/Outputs`.

Public but weak until live-proven: `MoveSelection(int, int)` decompiles as no useful movement in current WIP.

---
## [8][OBJECT_LIST]
>**Dictum:** *ObjectList owns query, bounds, selection, wires, and graph lookup.*

<br>

Public query capabilities:

- Events: object expired, name, selection, enabled, relevance, layout, display, instance id, added, removed.
- Counts: `Count`, `PinCount`, `ExpiredCount`, `SelectedCount`, `SelectedWireCount`.
- Enumerations: `Pins`, `Forwards`, `Backwards`, `PrimaryAndSecondary`, `Groups`, `ActiveObjects`, `ExpiredObjects`, `Attributes`, `AllAttributes`.
- Bounds: `AttributeBounds`, `PivotBounds`.
- State sets: `SelectedObjects`, `UnselectedObjects`, `EnabledObjects`, `DisabledObjects`.
- Wires: `AllWires`, `SelectedWires`, `IsWireSelected`, `SelectWire`, `DeselectWire`, `DeselectAllWires`.
- Picking: `ResolvePick`, `ResolveGripPick`, `WindowSelect`.
- Lookup: `Find`, `FindParameter`, `FindComponent`, `FindGroup`, `FindByInlet`, `FindByOutlet`, `FindByInletOrOutlet`, `FindNear`.
- Graph: `Connectivity`, `SearchUpstream`, `SearchDownstream`, `ShootRay`.
- Maintenance: `ChangeAllIds`, `ApplyIdMap`, `ExpireAll`, `ClearDisplayCaches`, `DisplayableObjects`.

Avoid: constructor, caches, backing lists, `WireRepository`, low-level clear/rebuild internals.

---
## [9][PICKING_SELECTION]
>**Dictum:** *Picking crosses canvas and document scopes.*

<br>

Use `Canvas.ResolvePick` when wires matter. Use `ObjectList.ResolvePick` when only objects, grips, foreground, background, and recursive subobjects matter.

Snapshot fields from `SelectionResult`: `Kind`, `Point`, selected/deselected counts, selected wire/object counts, `WireUnderPick`, `ObjectUnderPick`, `InletUnderPick`, `OutletUnderPick`.

Use `KeyboardExtensionMethods.SelectionMode(...)` for Eto control, mouse event, and window-selection event mapping.

---
## [10][WIRES]
>**Dictum:** *Wire API is public through parameters, endpoints, and document/object facades.*

<br>

Public wire surfaces:

- `IParameter.Inputs` and `IParameter.Outputs`.
- `Connections.Connect`, `Disconnect`, and related parameter connection operations.
- `WireEnds.Source`, `WireEnds.Target`.
- `WireData` where exposed by object-list/document APIs.
- `ObjectList.AllWires`, `SelectedWires`, wire select/deselect methods.
- `DocumentMethods.SplitWire`.
- `Canvas.ResolvePick(... includeWires: true ...)`.
- `WireShape` for public drawing geometry where caller owns shape creation.

Avoid `WireRepository` and `Canvas.WireDrawCache`; both are internal implementation/display cache surfaces.

---
## [11][LAYOUT_SNAPPING]
>**Dictum:** *Arrange tools compose native bounds, movement, snapping, and narrow alignment.*

<br>

Layout primitives:

- `IAttributes.Pivot`, `Bounds`, `AggregateBounds`, `Move`, `Layout`, `Invalidate`, `Snappable`.
- `IParameterAttributes.HasInlet`, `HasOutlet`, `Inlet`, `Outlet`.
- `ObjectList.AttributeBounds`, `PivotBounds`.

Snapping primitives:

- `SnappingSettings.Default`, `Current`, `Rules`, `WithRules`, `WithoutRules`, `WithFeedback`.
- `SnappingSettings.AlignLeftEdges`, `AlignRightEdges`, `AlignTopEdges`, `AlignBottomEdges`, `AlignMiddles`, `AlignWires`, `SpaceVerticalGaps`, `SpaceHorizontalGaps`.
- `SnappingSettings.VerticalGapSize`, `HorizontalGapSize`, `EdgeRadius`, `WireRadius`, `Feedback`, `Colour`.
- `SnappingRule` values for edge, middle, gap, and wire alignment.
- `SnappingConstraints.CreateFromDocument`, `SnapObject`, `SnapRectangle`, `SnapWires`, `DrawSnappingBoxes`.
- `SnappingAction` factory methods for left/right/top/bottom/centre alignment, gap spacing, ortho constraints, and wire straightening.

Native narrow alignment:

- `OCD.AlignObjects(Component, Component, Fixed)`.
- `OCD.AlignObjects(IParameter, Component, Fixed)`.
- `OCD.AlignObjects(Component, IParameter, Fixed)`.
- `OCD.AlignObjects(IParameter, IParameter, Fixed)`.

No general one-call auto-arrange API surfaced. Rasm auto-align must be an algorithm over these primitives.

---
## [12][COMPONENT_ATTRIBUTES]
>**Dictum:** *Component UI customizes attributes, not global canvas state.*

<br>

Core contracts:

- `IAttributes`: owner, pivot, bounds, aggregate bounds, move, hit testing, tooltip, double-click, snappable, layout, invalidate, draw.
- `ComponentAttributes`: central/content/label boxes, input/output parameter layout, bounds layout, draw phases, message bars, grips, icon/name/label/content drawing, foreground decorations, tentative grips.
- `IParameterAttributes`: inlet/outlet availability and grip points.
- `IResponsiveAttributes`: mouse/key response surface implemented through `ComponentAttributes` protected hooks.
- `IContextMenuAware.AppendToMenu(ContextMenu)`.
- `ICursorAwareAttributes.CursorAt(PointF)`.
- `IMouseHoverAttributes.RespondToMouseHover(PointF, PointF)`.

Persistent component UI state belongs on owner document objects, not attributes.

---
## [13][INPUT_PANEL_TOOLBAR]
>**Dictum:** *GH2 provides native component editing and compact control rails.*

<br>

`InputPanel` public surface:

- `BeginCategory`, `Add`, `AddLabel`, `AddCheck`, `AddText`, `AddBar`.
- `MoveCategoryBelow`, `RenameCategory`, `RemoveCategory`.
- `ShowAsForm`, `ToEtoControl`.

`Toolbar` public surface:

- `Bar`, `BarControl`, `BarItem`, `BarStyle`, `BarShortcut`.
- Item families: `NumberSlider`, `PushButton`, `RadioToggle`, `Spacer`, `TextField`, `ValueIcon`.
- Operations: `Add`, `AddSpacer`, `AddRadioToggle`, `AddPushButton`, `AddTextField`, `AddColours`, `AddToggle`, `Find<T>`, `Layout`, `Render`.

Avoid `InputPanel.FindBar`; decompile marks it internal.

---
## [14][FLEX_INTERACTION]
>**Dictum:** *Flex owns map, projection, navigation, responsiveness, and interaction stack.*

<br>

Public Flex surface:

- `IFlexControl` and `FlexControl` mapping: `Map`, `VisibleFrame`, `ContentBounds`, `ControlToContent`, `ContentToControl`, `Projection`.
- Navigation: `Navigate(...)`, `AnimatedZoomFactor`, `MinimumZoom`, `MaximumZoom`, `AllowPan`, `AllowZoom`.
- Interaction: `PushInteraction`, `Interactions`, `FocusObject`, `RegisterIResponsive`, `UnregisterIResponsive`, `Responsives`.
- Window selection: `BeginWindowSelect`, `EndWindowSelect`, `CurrentWindowSelection`, window selection drawing.
- Redraw/animation: `ScheduleRedraw`, `Animate<T>`, `AddSparkle`.
- Built-ins: `PanInteraction`, `ZoomInteraction`.

Implement custom behavior through `AbstractResponsive` and `AbstractInteraction`, not direct raw interface implementations.

---
## [15][PAINT_SKINNING]
>**Dictum:** *Drawing stays native Eto plus GH2 skin primitives.*

<br>

Use:

- Eto: `Context`, `Graphics`, `PointF`, `RectangleF`, `LineF`, `Color`, `OpenColor`, `Pen`, `Brush`, `Bitmap`, `ImageFormat`.
- GH2 skinning: `Skin`, `CanvasSkin`, `GridSkin`, `WireSkin`, `WiresSkin`, `GripSkin`, `GripsSkin`, `Shade`, `Shades`, `Shape`, `Fades`, `FontDescription`, `FillDescription`.
- GH2 primitives: `Capsule`, `Plug`.
- GH2 icons: `IIcon`, `AbstractIcon`, `StandardIcons`, vector icon builder/elements, palette/filter/icon controls.

Dispose caller-created Eto pens, brushes, and bitmaps. Do not dispose cached brushes/pens owned by GH2 `Shade` or skin descriptors.

---
## [16][KEYBOARD_MAC]
>**Dictum:** *GH2 owns platform modifier translation.*

<br>

Use `KeyboardExtensionMethods`:

- `SelectionMode(Control)`, `SelectionMode(MouseEventArgs)`, `SelectionMode(WindowSelectionEventArgs)`.
- `HasShift(Keys)`.
- `HasCommand(Keys)`: Control on Windows, Command/Application on Mac.
- `HasOption(Keys)`: Alt on Windows, Option on Mac.
- `IsShift(Keys)`.
- `IsControl(Keys)`: Control on Windows, Command/Application on Mac.
- `ImpliedSelectionMode()`.

Avoid local macOS modifier tables.

---
## [17][ETO]
>**Dictum:** *Eto is the cross-platform UI substrate for this rail.*

<br>

Use Eto for forms, controls, context menus, mouse/key/text input events, drawing resources, bitmap capture, point/rectangle geometry, colors, pens, brushes, and fonts. Keep Eto resources inside boundary lifetimes unless encoded or snapshotted.

---
## [18][RHINO_HOST]
>**Dictum:** *Rhino host APIs feed GH workflows only when explicitly required.*

<br>

Default GH2 rail uses `Rhino.RhinoApp` for UI-thread dispatch only. Host dialogs, Rhino status bar, Rhino panels, Rhino options pages, viewport overlays, mouse callbacks, and gumballs belong to `Rasm.Rhino.UI` or a later explicitly Rhino-UI-aware lane.

---
## [19][NON_SURFACES]
>**Dictum:** *Avoid APIs that look public only through XML noise or implementation shape.*

<br>

- `WireRepository`, `Canvas.WireDrawCache`.
- `Canvas.DropData`, `Canvas.DragState`, `Canvas.ViewportMouseDragger` internals.
- `PopupSearchResults`, popup implementation forms.
- `ObjectList` caches and backing lists.
- `Document.References` and private constructors.
- `SelectionResult` mutators outside GH2 internals.
- `DocumentMethods.MoveSelection` until live-proven.
- Direct `Rhino.UI` through hidden transitive dependency.

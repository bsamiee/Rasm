# [RASM_RHINO_API_RHINO_UI]

The `Rhino.UI` boundary owns host integration for native chrome: panel and page registration and lifecycle, the Eto host bridge that gives a document-owned window to an Eto control and styles it as native, the built-in native dialogs, the gumball manipulator, the mouse-callback and in-viewport interaction surface, the status-bar and toolbar/RUI state, and the SVG/preview resource utilities. The surface spans two host assemblies — `RhinoEtoApp` and `EtoExtensions` resolve from `Rhino.UI.dll`, while `Panels`, `StatusBar`, `StackedDialogPage`, `DrawingUtilities`, and the `RhinoApp` UI-thread members resolve from `RhinoCommon.dll`. The Eto framework itself (controls, layouts, drawing, runtime dispatch) is owned by the folder's Eto catalogs (`api-eto-forms.md`, `api-eto-drawing.md`, `api-eto-runtime.md`); this boundary owns only the seam that hosts an Eto surface inside Rhino, and the in-viewport `UserInterfaceObject` family that draws through the display pipeline of `api-rhinocommon-display.md`.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `RhinoCommon` + `Rhino.UI` (host UI bridge)
- package: `RhinoCommon` (with `Rhino.UI` companion assembly)
- license: proprietary McNeel SDK (host-provided, not centrally pinned)
- assembly: `Rhino.UI.dll` (`RhinoEtoApp`, `EtoExtensions`, dialog and control hosts)
- assembly: `RhinoCommon.dll` (`Panels`, `StatusBar`, `StackedDialogPage`, `DrawingUtilities`, gumball, mouse, toolbar, UI-thread)
- namespace: `Rhino.UI` (panels, dialogs, pages, mouse, status, toolbar, resources, in-viewport UI objects)
- namespace: `Rhino.UI.Gumball` (`GumballObject`, `GumballDisplayConduit`, `GumballFrame`)
- namespace: `Rhino.UI.Controls` (`EtoCollapsibleSection`, `EtoCollapsibleSectionHolder`)
- asset: host-resolved managed reference; the boundary composes it, the manifest never pins it
- rail: host-boundary native-ui

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: panels, pages, and the Eto host bridge
- rail: host-boundary native-ui

| [INDEX] | [SYMBOL]                                                                      | [KIND]          | [CAPABILITY]                                                                                               |
| :-----: | :---------------------------------------------------------------------------- | :-------------- | :--------------------------------------------------------------------------------------------------------- |
|  [01]   | `Panels`                                                                      | panel registry  | register/open/float/close/sibling/visibility/dock-bar management, icon change, show/close events           |
|  [02]   | `IPanel` / `PanelType`                                                        | panel contract  | the shown/hidden/closing lifecycle contract and the panel host-type discriminant                           |
|  [03]   | `RhinoEtoApp`                                                                 | window owner    | the document-owned main/properties/preferences window that parents an Eto surface                          |
|  [04]   | `EtoExtensions`                                                               | host styling    | native styling, semi-modal display, position persistence, and document-window discovery for an Eto control |
|  [05]   | `StackedDialogPage`                                                           | stacked page    | a nested options page with child-page tree, activation, and navigation                                     |
|  [06]   | `OptionsDialogPage`                                                           | options page    | a document/application options page host                                                                   |
|  [07]   | `ObjectPropertiesPage` / `ObjectPropertiesPageEventArgs` / `PropertyPageType` | properties page | a selection-driven object-properties page with display predicate and update hooks                          |
|  [08]   | `EtoCollapsibleSection` / `EtoCollapsibleSectionHolder`                       | section host    | collapsible page sections for a properties/options surface                                                 |
|  [09]   | `LocalizeStringPair`                                                          | localized label | an English/localized string pair for page and menu captions                                                |

[PUBLIC_TYPE_SCOPE]: dialogs, gumball, and mouse interaction
- rail: host-boundary native-ui

| [INDEX] | [SYMBOL]                                    | [KIND]          | [CAPABILITY]                                                                                                               |
| :-----: | :------------------------------------------ | :-------------- | :------------------------------------------------------------------------------------------------------------------------- |
|  [01]   | `Dialogs`                                   | native dialogs  | color/message/text/list-box/multi-list/check-list/property-list/edit-box/number-box/layer/linetype/print-width/sun dialogs |
|  [02]   | `NamedColorList` / `NamedColor`             | color palette   | the named-color palette backing the color dialog                                                                           |
|  [03]   | `GumballObject` / `GumballFrame`            | gumball state   | the manipulator geometry and its frame set from geometry or a plane                                                        |
|  [04]   | `GumballDisplayConduit`                     | gumball conduit | the display conduit drawing and picking the gumball, driving pre/gumball/total transforms                                  |
|  [05]   | `GumballAppearanceSettings` / `GumballMode` | gumball config  | appearance settings and the active manipulation mode                                                                       |
|  [06]   | `MouseCallback`                             | mouse hook      | the viewport mouse hook: move/down/up/double-click/enter/hover/leave with begin/end pairs                                  |
|  [07]   | `MouseCallbackEventArgs` / `MouseButton`    | mouse args      | the callback handle (`ViewportPoint`, `IsOverGumball`) and the button discriminant                                         |
|  [08]   | `MouseCursor` / `WaitCursor`                | cursor          | tooltip-carrying cursor control and a scoped wait cursor                                                                   |

[PUBLIC_TYPE_SCOPE]: in-viewport UI objects
- rail: host-boundary native-ui

| [INDEX] | [SYMBOL]                                       | [KIND]              | [CAPABILITY]                                                                             |
| :-----: | :--------------------------------------------- | :------------------ | :--------------------------------------------------------------------------------------- |
|  [01]   | `MouseState`                                   | interaction state   | the picked mouse state: `Button`, `FrustumLine`, `View`, and curve/line hit tests        |
|  [02]   | `UserInterfaceObjectBase`                      | in-viewport UI      | a registered in-viewport widget with draw and mouse-event overrides and visibility state |
|  [03]   | `GripUserInterfaceObject`                      | draggable grip      | a constrained snap-point grip with location, radius, and object-snap permission          |
|  [04]   | `DirectionGripUserInterfaceObject`             | direction grip      | a directional arrow grip with viewport-visibility and arrow radius                       |
|  [05]   | `RotationGripUserInterfaceObject`              | rotation grip       | a rotation arc grip with per-viewport visibility and a rotation-drag hook                |
|  [06]   | `TextDotUserInterfaceObject`                   | in-viewport label   | a text-dot widget with text and height                                                   |
|  [07]   | `UserInterfaceControl` / `UserInterfaceSlider` | in-viewport control | an SVG-backed control and a ranged slider with a value-changed event                     |
|  [08]   | `CommandPromptChangedEventArgs`                | prompt state        | the command prompt, its default, and the current command-line options                    |

[PUBLIC_TYPE_SCOPE]: status, toolbar, and resources
- rail: host-boundary native-ui

| [INDEX] | [SYMBOL]                                            | [KIND]           | [CAPABILITY]                                                                   |
| :-----: | :-------------------------------------------------- | :--------------- | :----------------------------------------------------------------------------- |
|  [01]   | `StatusBar`                                         | status chrome    | command-prompt, message/distance/number/point panes, and the progress meter    |
|  [02]   | `RuiUpdateUi`                                       | menu state       | menu-item registration and menu-state synchronization                          |
|  [03]   | `ToolbarFile` / `ToolbarFileCollection` / `Toolbar` | toolbar state    | open/find `.rui` files, group and enumerate toolbars                           |
|  [04]   | `DrawingUtilities`                                  | resource loader  | SVG/bitmap/icon loading, mesh/curve preview images, linetype preview geometry  |
|  [05]   | `RhinoApp` (UI-thread members)                      | thread marshal   | `InvokeOnUiThread`, `InvokeAndWait`, `IsOnMainThread` for main-thread dispatch |
|  [06]   | `RhinoView.ShowToast`                               | transient notice | a viewport toast notification                                                  |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: `Panels`, pages, and the Eto host bridge
- rail: host-boundary native-ui

| [INDEX] | [SURFACE]                                                                                                                                                                                                                                                                        | [CALL_SHAPE] | [CAPABILITY]                                                                                     |
| :-----: | :------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | :----------- | :----------------------------------------------------------------------------------------------- |
|  [01]   | `Panels.RegisterPanel(PlugIn, Type, string, Icon, PanelType)` / `RegisterPanel(PlugIn, Type, string, Assembly, string, PanelType)`                                                                                                                                               | register     | register a panel type by icon or embedded-resource icon                                          |
|  [02]   | `Panels.OpenPanel(Type, bool)` / `OpenPanel(Guid, Type, bool)` / `OpenPanelAsSibling(Guid, Guid, bool)` / `FloatPanel(Type, FloatPanelMode)` / `ClosePanel(Type, RhinoDoc)`                                                                                                      | lifecycle    | open, sibling-open, float, and close a panel                                                     |
|  [03]   | `Panels.GetPanel(Guid, RhinoDoc)` / `GetPanels<T>(RhinoDoc)` / `GetPanels<T>(uint)` / `IsPanelVisible(Type, bool)` / `PanelDockBars(Guid)` / `GetOpenPanelIds()` / `DockBarIdInUse(Guid)` / `ChangePanelIcon(Type, Icon)` / `ChangePanelIcon(Type, string)` / `IconSizeInPixels` | query        | resolve panel instances, visibility, dock bars, open census, icon, and icon size                 |
|  [04]   | `Panels.Show` (`ShowPanelEventArgs.PanelId/DocumentSerialNumber/Show`) / `Panels.Closed` (`PanelEventArgs`) / `IsShowing(ShowPanelReason)` / `IsHiding(ShowPanelReason)` ; `OnShowPanel(Guid, uint, bool)` / `OnClosePanel(Guid, uint)`                                          | events       | panel show/close notifications, reason polarity probes, and the `IPanel` hooks                   |
|  [05]   | `IPanel.PanelShown(uint, ShowPanelReason)` / `PanelHidden(uint, ShowPanelReason)` / `PanelClosing(uint, bool)`                                                                                                                                                                   | contract     | the per-instance panel lifecycle callbacks                                                       |
|  [06]   | `RhinoEtoApp.MainWindow` / `MainWindowForDocument(RhinoDoc)` / `DocumentPropertiesWindowForPage(OptionsDialogPage)` / `ApplicationPreferencesWindowForPage(OptionsDialogPage)`                                                                                                   | window       | resolve the application or document-owned `Eto.Forms.Window` that parents an Eto surface         |
|  [07]   | `EtoExtensions.UseRhinoStyle(Control)` / `Show(Form, RhinoDoc)` / `ShowSemiModal<T>(Dialog<T>, RhinoDoc, Control)` / `ShowSemiModal(Dialog, RhinoDoc, Control)`                                                                                                                  | host         | style native, show, and semi-modally host an Eto window against a document                       |
|  [08]   | `EtoExtensions.SavePosition(Window, Type)` / `RestorePosition(Window, Type)` / `LocalizeAndRestore(Window, Type)` / `WindowsFromDocument<T>(RhinoDoc)` / `GetRhinoDoc(Form)`                                                                                                     | host         | persist/restore position, localize, discover document windows, and invert to the owning document |
|  [09]   | `ThemeSettings.ThemeChanged` (public static `EventHandler` field, `+=` subscription)                                                                                                                                                                                             | theme        | the host light/dark transition edge; the notifier behind `EtoExtensions` is private              |
|  [10]   | `StackedDialogPage.AddChildPage(StackedDialogPage)` / `MakeActivePage()` / `OnActivate(bool)` / `SetActivePageTo(string, bool)`                                                                                                                                                  | page         | build and navigate the stacked-page tree                                                         |
|  [11]   | `ObjectPropertiesPage.ShouldDisplay(...)` / `UpdatePage(...)` / `ModifyPage(Action<...>)` / `GetSelectedObjects(ObjectType)`                                                                                                                                                     | page         | selection-gated properties page display and update                                               |
|  [12]   | `ObjectPropertiesPageEventArgs.Document` / `DocRuntimeSerialNumber` / `EventRuntimeSerialNumber` / `View` / `Viewport` / `ObjectCount` / `GetObjects(ObjectType)` / `GetObjects<T>()` / `IncludesObjectsType(ObjectType, bool)`                                                  | page args    | the selection-event projection: document, serials, view, and typed object reads                  |
|  [13]   | `EtoCollapsibleSection.Caption` / `SectionHeight` / `Collapsible` / `Hidden` / `InitiallyExpanded` / `CommandOptionName` ; `EtoCollapsibleSectionHolder.Add(ICollapsibleSection)` / `UseScrollbars` / `UseCheckBoxes`                                                            | section      | the collapsible section overrides and the holder stack                                           |

[ENTRYPOINT_SCOPE]: dialogs, gumball, and mouse callbacks
- rail: host-boundary native-ui

| [INDEX] | [SURFACE]                                                                                                                                                                         | [CALL_SHAPE] | [CAPABILITY]                                                       |
| :-----: | :-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | :----------- | :----------------------------------------------------------------- |
|  [01]   | `Dialogs.ShowMessage(...)` / `ShowColorDialog(object, ref Color4f, bool, NamedColorList, OnColorChangedEvent)`                                                                    | dialog       | native message and live-preview color dialogs                      |
|  [02]   | `Dialogs.ShowMultiListBox(...)` / `ShowCheckListBox(...)` / `ShowPropertyListBox(...)` / `ShowSelectMultipleLayersDialog(...)`                                                    | dialog       | list, check-list, property-list, and multi-layer selection dialogs |
|  [03]   | `GumballDisplayConduit.SetBaseGumball(GumballObject, GumballAppearanceSettings)` / `PickGumball(PickContext, GetPoint)` / `UpdateGumball(Point3d, Line)` / `UpdateGumball(Plane)` | gumball      | seat, pick, and update the manipulator across a drag               |
|  [04]   | `MouseCallback.OnMouseMove/OnMouseDown/OnMouseUp(MouseCallbackEventArgs)` (+ `OnEndMouseMove/Down/Up`)                                                                            | override     | the viewport mouse hook with begin/end phase pairs                 |
|  [05]   | `MouseCallbackEventArgs.ViewportPoint` / `IsOverGumball()` ; `MouseCursor.SetToolTip(string)`                                                                                     | read         | the callback hit point, gumball-over test, and cursor tooltip      |

[ENTRYPOINT_SCOPE]: in-viewport UI objects
- rail: host-boundary native-ui

| [INDEX] | [SURFACE]                                                                                                                                  | [CALL_SHAPE] | [CAPABILITY]                                                      |
| :-----: | :----------------------------------------------------------------------------------------------------------------------------------------- | :----------- | :---------------------------------------------------------------- |
|  [01]   | `UserInterfaceObjectBase.RegisterForAllDocuments()` / `Unregister()`                                                                       | lifecycle    | register an in-viewport widget across documents and retire it     |
|  [02]   | `UserInterfaceObjectBase.OnDraw(DrawEventArgs)` / `OnMouseClick/OnMouseDoubleClick/OnMouseDown/OnMouseMove/OnMouseUp(MouseState)`          | override     | draw through the display pipeline and react to picked mouse state |
|  [03]   | `UserInterfaceObjectBase.BoundToActiveView` / `Visible`                                                                                    | state        | active-view binding and visibility                                |
|  [04]   | `GripUserInterfaceObject.SetSnapPoints(IEnumerable<Point3d>)` / `Constrain(Curve)` ; `GripLocation` / `GripRadius` / `ObjectSnapPermitted` | grip         | a constrained snap-point grip                                     |
|  [05]   | `DirectionGripUserInterfaceObject.ArrowsVisibleInViewport(RhinoViewport)` ; `GripDirection` / `ArrowRadius`                                | grip         | a directional arrow grip                                          |
|  [06]   | `RotationGripUserInterfaceObject.ArcVisibleInViewport(RhinoViewport)` / `OnRotationDrag(double, MouseState)`                               | grip         | a rotation-arc grip with a drag hook                              |
|  [07]   | `UserInterfaceControl.SetSvg(string)` ; `UserInterfaceSlider.Range` / `Value` / `ValueChanged` event / `OnValueChanged()`                  | control      | an SVG-backed control and a ranged slider                         |
|  [08]   | `MouseState.IsMouseOver(Curve, out double)` / `IsMouseOver(Line)` ; `Button` / `FrustumLine` / `View`                                      | hit test     | pick-line hit testing against curves and lines                    |

[ENTRYPOINT_SCOPE]: status, toolbar, resources, and UI thread
- rail: host-boundary native-ui

| [INDEX] | [SURFACE]                                                                                                                                                                                         | [CALL_SHAPE] | [CAPABILITY]                                                                                                                  |
| :-----: | :------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ | :----------- | :---------------------------------------------------------------------------------------------------------------------------- |
|  [01]   | `StatusBar.ShowProgressMeter(uint, int, int, string, bool, bool)` / `UpdateProgressMeter(uint, string, int, bool)` / `HideProgressMeter(uint)`                                                    | status       | drive the status-bar progress meter                                                                                           |
|  [02]   | `RuiUpdateUi.RegisterMenuItem(Guid, Guid, Guid, UpdateMenuItemEventHandler)` ; `RuiUpdateUi.Enabled` / `Checked` / `RadioChecked` / `Text`                                                        | menu         | register a menu item and mutate its live state                                                                                |
|  [03]   | `ToolbarFileCollection.Open(string)` / `FindByPath(string)` ; `ToolbarFile.GetToolbar(int)`                                                                                                       | toolbar      | open, find, and index `.rui` toolbar files                                                                                    |
|  [04]   | `DrawingUtilities.BitmapFromSvg(string, int, int, bool)` / `PixelsFromSvg(string, int, int, bool, Color, bool)` / `CreateMeshPreviewImage(RhinoDoc, IEnumerable<Mesh>, IEnumerable<Color>, Size)` | resource     | SVG rasterization and mesh preview images                                                                                     |
|  [05]   | `DrawingUtilities.CreateLinetypePreviewGeometryEx(Curve, Linetype, int, int, double, int)`                                                                                                        | resource     | linetype preview geometry keyed by the trailing `kind` channel: 0 dash fill, 1 curve-shape stroke, 2 text-shape even-odd fill |
|  [06]   | `RhinoApp.InvokeOnUiThread(Delegate, params object[])` / `InvokeAndWait(Action)` / `IsOnMainThread` ; `RhinoView.ShowToast(...)`                                                                  | thread       | main-thread marshaling and a viewport toast                                                                                   |

## [04]-[IMPLEMENTATION_LAW]

[UI_TOPOLOGY]:
- Native chrome registers once per plug-in and lives in one owner: `Panels.RegisterPanel` seats a panel type, `StackedDialogPage`/`OptionsDialogPage`/`ObjectPropertiesPage` seat pages, and the returned host resolves instances through `GetPanel`/`GetPanels<T>` — a second registration of the same type is the collapsed form.
- The Eto host bridge is the only path from an Eto surface to a Rhino window: `RhinoEtoApp` resolves the document-owned parent, `EtoExtensions.UseRhinoStyle` applies native styling, and `ShowSemiModal`/`Show` present it against a document; the Eto control tree itself is authored through the folder Eto catalogs, and the bridge never re-implements a control.
- Interaction has two tiers: `MouseCallback` is the document-wide viewport mouse hook (begin/end phase pairs), while a `UserInterfaceObjectBase` and its grip/slider subclasses are registered in-viewport widgets that draw through the display pipeline and receive a picked `MouseState`. The gumball is the third, dedicated manipulator — a `GumballDisplayConduit` seated from a `GumballObject`, never a hand-rolled grip cluster.
- Every host callback runs on the UI thread: work that touches document or UI state from a background context marshals through `RhinoApp.InvokeOnUiThread`/`InvokeAndWait`, gated by `IsOnMainThread` — a direct cross-thread UI mutation is the deleted form.

[STACKING]:
- `api-eto-forms.md` / `api-eto-drawing.md` / `api-eto-runtime.md`: the Eto framework is the folder's own sub-domain; this boundary composes it through the host bridge only. A panel or dialog's content is an Eto control tree from those catalogs; `Rhino.UI` supplies the window ownership, native styling, and semi-modal presentation the tree lacks on its own.
- `api-languageext.md`(`../../.api/api-languageext.md`): panel registration, page activation, dialog results, and resource loads are trapped onto the rail — `Try.lift(() => Panels.RegisterPanel(...)).Run()` and `Optional(Dialogs.ShowColorDialog(...)).ToFin(error)`; a dialog result or a loaded preview image crosses as `Fin<A>`, never as a nullable host handle.
- `api-thinktecture-runtime-extensions.md`(`../../.api/api-thinktecture-runtime-extensions.md`): the host UI enums (`PanelType`, `FloatPanelMode`, `ShowPanelReason`, `MouseButton`, `GumballMode`, `PropertyPageType`, the dialog button/icon selectors) map at the edge to `[SmartEnum]` owners, and a panel/page `Guid` is a `[ValueObject<Guid>]`; the domain composes the bounded owner.
- `api-rhinocommon-display.md`: the in-viewport `UserInterfaceObjectBase.OnDraw` receives a `DrawEventArgs` and draws through the same `DisplayPipeline` the display catalog owns, and the gumball is a display conduit — the UI widget is a pipeline participant, not a private renderer.

[LOCAL_ADMISSION]:
- The `Rhino.UI` types are host handles trapped and mapped at the boundary; a `Panels` registration id, a `Dialogs` result, or a `MouseState` never appears in a domain signature — the domain sees a `Fin<A>`, a bounded owner, or a canonical shape.
- One panel type, one page host, one gumball conduit, and one mouse hook own their concern; a parallel registration or a second hook drawing the same overlay is the collapsed form.

[RAIL_LAW]:
- Package: `RhinoCommon` + `Rhino.UI` (host UI bridge)
- Owns: panel and page registration and lifecycle, the Eto host bridge (window ownership, native styling, semi-modal, position), native dialogs, the gumball manipulator, mouse callbacks and in-viewport UI objects, status/toolbar/RUI state, SVG and preview resources, and UI-thread marshaling
- Accept: a panel/page registered once and resolved through the host, an Eto surface hosted through `RhinoEtoApp`/`EtoExtensions`, a gumball conduit or in-viewport widget drawing through the display pipeline, host handles trapped through `Try.lift(...).Run()`, and UI work marshaled onto the main thread
- Reject: a duplicate registration of one panel/page type, a hand-rolled control where an Eto surface fits, a hand-rolled grip cluster where the gumball or a `UserInterfaceObject` fits, a cross-thread UI mutation without `InvokeOnUiThread`, and a `Panels`/`Dialogs`/`MouseCallback`/`StackedDialogPage` handle escaping into a domain signature

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

`Panels` owns registration, opening, sibling opening, floating, closing, visibility, dock bars, icons, and show/close events. `RhinoEtoApp` resolves the application- or document-owned `Eto.Forms.Window` that parents an Eto surface.

| [INDEX] | [SYMBOL]                        | [KIND]          | [CAPABILITY]                          |
| :-----: | :------------------------------ | :-------------- | :------------------------------------ |
|  [01]   | `Panels`                        | panel registry  | complete panel lifecycle              |
|  [02]   | `IPanel`                        | panel contract  | shown/hidden/closing callbacks        |
|  [03]   | `PanelType`                     | discriminant    | panel host type                       |
|  [04]   | `RhinoEtoApp`                   | window owner    | document-owned Eto parent             |
|  [05]   | `EtoExtensions`                 | host styling    | native Eto window integration         |
|  [06]   | `StackedDialogPage`             | stacked page    | nested page activation and navigation |
|  [07]   | `OptionsDialogPage`             | options page    | document/application page host        |
|  [08]   | `ObjectPropertiesPage`          | properties page | selection-driven page hooks           |
|  [09]   | `ObjectPropertiesPageEventArgs` | properties args | selection-event projection            |
|  [10]   | `PropertyPageType`              | discriminant    | properties-page type                  |
|  [11]   | `EtoCollapsibleSection`         | section         | collapsible page section              |
|  [12]   | `EtoCollapsibleSectionHolder`   | section host    | properties/options section stack      |
|  [13]   | `LocalizeStringPair`            | localized label | English/localized caption pair        |

[PUBLIC_TYPE_SCOPE]: dialogs, gumball, and mouse interaction

- rail: host-boundary native-ui

`Dialogs` owns color, message, text, list-box, multi-list, check-list, property-list, edit-box, number-box, layer, linetype, print-width, and sun dialogs. `GumballDisplayConduit` drives pre-, gumball-, and total transforms. `MouseCallback` owns move, down, up, double-click, enter, hover, and leave hooks with begin/end pairs.

| [INDEX] | [SYMBOL]                    | [KIND]          | [CAPABILITY]                     |
| :-----: | :-------------------------- | :-------------- | :------------------------------- |
|  [01]   | `Dialogs`                   | native dialogs  | built-in dialog suite            |
|  [02]   | `NamedColorList`            | color palette   | color-dialog palette             |
|  [03]   | `NamedColor`                | named color     | palette entry                    |
|  [04]   | `GumballObject`             | gumball state   | manipulator geometry             |
|  [05]   | `GumballFrame`              | gumball frame   | geometry-derived or planar frame |
|  [06]   | `GumballDisplayConduit`     | gumball conduit | drawing, picking, and transforms |
|  [07]   | `GumballAppearanceSettings` | gumball config  | manipulator appearance           |
|  [08]   | `GumballMode`               | discriminant    | active manipulation mode         |
|  [09]   | `MouseCallback`             | mouse hook      | viewport mouse callbacks         |
|  [10]   | `MouseCallbackEventArgs`    | mouse args      | viewport point and gumball hit   |
|  [11]   | `MouseButton`               | discriminant    | callback button                  |
|  [12]   | `MouseCursor`               | cursor          | tooltip-carrying cursor control  |
|  [13]   | `WaitCursor`                | cursor          | scoped wait cursor               |

[PUBLIC_TYPE_SCOPE]: in-viewport UI objects

- rail: host-boundary native-ui

| [INDEX] | [SYMBOL]                           | [KIND]            | [CAPABILITY]                  |
| :-----: | :--------------------------------- | :---------------- | :---------------------------- |
|  [01]   | `MouseState`                       | interaction state | picked state and hit tests    |
|  [02]   | `UserInterfaceObjectBase`          | in-viewport UI    | registered draw/mouse widget  |
|  [03]   | `GripUserInterfaceObject`          | draggable grip    | constrained snap-point grip   |
|  [04]   | `DirectionGripUserInterfaceObject` | direction grip    | viewport-visible arrow grip   |
|  [05]   | `RotationGripUserInterfaceObject`  | rotation grip     | viewport-visible rotation arc |
|  [06]   | `TextDotUserInterfaceObject`       | in-viewport label | text and height               |
|  [07]   | `UserInterfaceControl`             | control           | SVG-backed control            |
|  [08]   | `UserInterfaceSlider`              | slider            | ranged value-changed control  |
|  [09]   | `CommandPromptChangedEventArgs`    | prompt state      | prompt, default, and options  |

[PUBLIC_TYPE_SCOPE]: status, toolbar, and resources

- rail: host-boundary native-ui

`StatusBar` owns command-prompt, message, distance, number, point, and progress panes. `DrawingUtilities` owns SVG, bitmap, icon, mesh-preview, curve-preview, and linetype-preview resources.

| [INDEX] | [SYMBOL]                       | [KIND]           | [CAPABILITY]                     |
| :-----: | :----------------------------- | :--------------- | :------------------------------- |
|  [01]   | `StatusBar`                    | status chrome    | status panes and progress meter  |
|  [02]   | `RuiUpdateUi`                  | menu state       | live menu synchronization        |
|  [03]   | `ToolbarFile`                  | toolbar file     | `.rui` file access               |
|  [04]   | `ToolbarFileCollection`        | toolbar registry | `.rui` open/find collection      |
|  [05]   | `Toolbar`                      | toolbar state    | toolbar grouping and enumeration |
|  [06]   | `DrawingUtilities`             | resource loader  | native UI resource utilities     |
|  [07]   | `RhinoApp` (UI-thread members) | thread marshal   | main-thread dispatch             |
|  [08]   | `RhinoView.ShowToast`          | transient notice | viewport toast                   |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: `Panels`, pages, and the Eto host bridge

- rail: host-boundary native-ui

| [INDEX] | [SURFACE]                                                                 | [CALL_SHAPE] | [CAPABILITY]                   |
| :-----: | :------------------------------------------------------------------------ | :----------- | :----------------------------- |
|  [01]   | `Panels.RegisterPanel(PlugIn, Type, string, Icon, PanelType)`             | register     | icon-backed panel type         |
|  [02]   | `Panels.RegisterPanel(PlugIn, Type, string, Assembly, string, PanelType)` | register     | resource-backed panel type     |
|  [03]   | `Panels.OpenPanel(Type, bool)`                                            | lifecycle    | open panel by type             |
|  [04]   | `Panels.OpenPanel(Guid, Type, bool)`                                      | lifecycle    | open panel by identifiers      |
|  [05]   | `Panels.OpenPanelAsSibling(Guid, Guid, bool)`                             | lifecycle    | sibling-open panel             |
|  [06]   | `Panels.FloatPanel(Type, FloatPanelMode)`                                 | lifecycle    | float panel                    |
|  [07]   | `Panels.ClosePanel(Type, RhinoDoc)`                                       | lifecycle    | close document panel           |
|  [08]   | `Panels.GetPanel(Guid, RhinoDoc)`                                         | query        | resolve panel instance         |
|  [09]   | `Panels.GetPanels<T>(RhinoDoc)`                                           | query        | resolve typed document panels  |
|  [10]   | `Panels.GetPanels<T>(uint)`                                               | query        | resolve typed serial panels    |
|  [11]   | `Panels.IsPanelVisible(Type, bool)`                                       | query        | read panel visibility          |
|  [12]   | `Panels.PanelDockBars(Guid)`                                              | query        | resolve panel dock bars        |
|  [13]   | `Panels.GetOpenPanelIds()`                                                | query        | read open panel identifiers    |
|  [14]   | `Panels.DockBarIdInUse(Guid)`                                             | query        | test dock-bar identifier       |
|  [15]   | `Panels.ChangePanelIcon(Type, Icon)`                                      | query        | replace panel icon             |
|  [16]   | `Panels.ChangePanelIcon(Type, string)`                                    | query        | replace resource icon          |
|  [17]   | `Panels.IconSizeInPixels`                                                 | query        | read native icon size          |
|  [18]   | `Panels.Show`                                                             | events       | panel show notification        |
|  [19]   | `ShowPanelEventArgs.PanelId`                                              | event args   | shown panel identifier         |
|  [20]   | `ShowPanelEventArgs.DocumentSerialNumber`                                 | event args   | shown document serial          |
|  [21]   | `ShowPanelEventArgs.Show`                                                 | event args   | show-state flag                |
|  [22]   | `Panels.Closed`                                                           | events       | panel close notification       |
|  [23]   | `PanelEventArgs`                                                          | event args   | close-event payload            |
|  [24]   | `Panels.IsShowing(ShowPanelReason)`                                       | events       | test showing reason            |
|  [25]   | `Panels.IsHiding(ShowPanelReason)`                                        | events       | test hiding reason             |
|  [26]   | `Panels.OnShowPanel(Guid, uint, bool)`                                    | events       | panel show hook                |
|  [27]   | `Panels.OnClosePanel(Guid, uint)`                                         | events       | panel close hook               |
|  [28]   | `IPanel.PanelShown(uint, ShowPanelReason)`                                | contract     | per-instance shown callback    |
|  [29]   | `IPanel.PanelHidden(uint, ShowPanelReason)`                               | contract     | per-instance hidden callback   |
|  [30]   | `IPanel.PanelClosing(uint, bool)`                                         | contract     | per-instance closing callback  |
|  [31]   | `RhinoEtoApp.MainWindow`                                                  | window       | application Eto parent         |
|  [32]   | `RhinoEtoApp.MainWindowForDocument(RhinoDoc)`                             | window       | document Eto parent            |
|  [33]   | `RhinoEtoApp.DocumentPropertiesWindowForPage(OptionsDialogPage)`          | window       | document-properties Eto parent |
|  [34]   | `RhinoEtoApp.ApplicationPreferencesWindowForPage(OptionsDialogPage)`      | window       | application-preferences parent |
|  [35]   | `EtoExtensions.UseRhinoStyle(Control)`                                    | host         | apply native styling           |
|  [36]   | `EtoExtensions.Show(Form, RhinoDoc)`                                      | host         | show document-owned form       |
|  [37]   | `EtoExtensions.ShowSemiModal<T>(Dialog<T>, RhinoDoc, Control)`            | host         | show typed semi-modal dialog   |
|  [38]   | `EtoExtensions.ShowSemiModal(Dialog, RhinoDoc, Control)`                  | host         | show semi-modal dialog         |
|  [39]   | `EtoExtensions.SavePosition(Window, Type)`                                | host         | persist window position        |
|  [40]   | `EtoExtensions.RestorePosition(Window, Type)`                             | host         | restore window position        |
|  [41]   | `EtoExtensions.LocalizeAndRestore(Window, Type)`                          | host         | localize and restore window    |
|  [42]   | `EtoExtensions.WindowsFromDocument<T>(RhinoDoc)`                          | host         | discover document windows      |
|  [43]   | `EtoExtensions.GetRhinoDoc(Form)`                                         | host         | resolve owning document        |
|  [44]   | `ThemeSettings.ThemeChanged`                                              | theme        | light/dark transition edge     |
|  [45]   | `StackedDialogPage.AddChildPage(StackedDialogPage)`                       | page         | append child page              |
|  [46]   | `StackedDialogPage.MakeActivePage()`                                      | page         | activate page                  |
|  [47]   | `StackedDialogPage.OnActivate(bool)`                                      | page         | page activation hook           |
|  [48]   | `StackedDialogPage.SetActivePageTo(string, bool)`                         | page         | navigate stacked-page tree     |
|  [49]   | `ObjectPropertiesPage.ShouldDisplay(...)`                                 | page         | selection display predicate    |
|  [50]   | `ObjectPropertiesPage.UpdatePage(...)`                                    | page         | update properties page         |
|  [51]   | `ObjectPropertiesPage.ModifyPage(Action<...>)`                            | page         | modify properties page         |
|  [52]   | `ObjectPropertiesPage.GetSelectedObjects(ObjectType)`                     | page         | read selected objects          |
|  [53]   | `ObjectPropertiesPageEventArgs.Document`                                  | page args    | selected document              |
|  [54]   | `ObjectPropertiesPageEventArgs.DocRuntimeSerialNumber`                    | page args    | document runtime serial        |
|  [55]   | `ObjectPropertiesPageEventArgs.EventRuntimeSerialNumber`                  | page args    | event runtime serial           |
|  [56]   | `ObjectPropertiesPageEventArgs.View`                                      | page args    | selected view                  |
|  [57]   | `ObjectPropertiesPageEventArgs.Viewport`                                  | page args    | selected viewport              |
|  [58]   | `ObjectPropertiesPageEventArgs.ObjectCount`                               | page args    | selected object count          |
|  [59]   | `ObjectPropertiesPageEventArgs.GetObjects(ObjectType)`                    | page args    | read filtered objects          |
|  [60]   | `ObjectPropertiesPageEventArgs.GetObjects<T>()`                           | page args    | read typed objects             |
|  [61]   | `ObjectPropertiesPageEventArgs.IncludesObjectsType(ObjectType, bool)`     | page args    | test included object type      |
|  [62]   | `EtoCollapsibleSection.Caption`                                           | section      | override section caption       |
|  [63]   | `EtoCollapsibleSection.SectionHeight`                                     | section      | override section height        |
|  [64]   | `EtoCollapsibleSection.Collapsible`                                       | section      | configure collapsibility       |
|  [65]   | `EtoCollapsibleSection.Hidden`                                            | section      | configure visibility           |
|  [66]   | `EtoCollapsibleSection.InitiallyExpanded`                                 | section      | configure initial expansion    |
|  [67]   | `EtoCollapsibleSection.CommandOptionName`                                 | section      | bind command option            |
|  [68]   | `EtoCollapsibleSectionHolder.Add(ICollapsibleSection)`                    | section      | append section                 |
|  [69]   | `EtoCollapsibleSectionHolder.UseScrollbars`                               | section      | configure scrollbars           |
|  [70]   | `EtoCollapsibleSectionHolder.UseCheckBoxes`                               | section      | configure check boxes          |

`ThemeSettings.ThemeChanged` is a public static `EventHandler` field subscribed through `+=`; the notifier behind `EtoExtensions` is private.

[ENTRYPOINT_SCOPE]: dialogs, gumball, and mouse callbacks

- rail: host-boundary native-ui

| [INDEX] | [SURFACE]                                                                                 | [CALL_SHAPE] | [CAPABILITY]              |
| :-----: | :---------------------------------------------------------------------------------------- | :----------- | :------------------------ |
|  [01]   | `Dialogs.ShowMessage(...)`                                                                | dialog       | native message dialog     |
|  [02]   | `Dialogs.ShowColorDialog(object, ref Color4f, bool, NamedColorList, OnColorChangedEvent)` | dialog       | live-preview color dialog |
|  [03]   | `Dialogs.ShowMultiListBox(...)`                                                           | dialog       | multi-list selection      |
|  [04]   | `Dialogs.ShowCheckListBox(...)`                                                           | dialog       | check-list selection      |
|  [05]   | `Dialogs.ShowPropertyListBox(...)`                                                        | dialog       | property-list selection   |
|  [06]   | `Dialogs.ShowSelectMultipleLayersDialog(...)`                                             | dialog       | multi-layer selection     |
|  [07]   | `GumballDisplayConduit.SetBaseGumball(GumballObject, GumballAppearanceSettings)`          | gumball      | seat manipulator          |
|  [08]   | `GumballDisplayConduit.PickGumball(PickContext, GetPoint)`                                | gumball      | pick manipulator          |
|  [09]   | `GumballDisplayConduit.UpdateGumball(Point3d, Line)`                                      | gumball      | update drag from line     |
|  [10]   | `GumballDisplayConduit.UpdateGumball(Plane)`                                              | gumball      | update drag from plane    |
|  [11]   | `MouseCallback.OnMouseMove(MouseCallbackEventArgs)`                                       | override     | begin mouse-move phase    |
|  [12]   | `MouseCallback.OnEndMouseMove(...)`                                                       | override     | end mouse-move phase      |
|  [13]   | `MouseCallback.OnMouseDown(MouseCallbackEventArgs)`                                       | override     | begin mouse-down phase    |
|  [14]   | `MouseCallback.OnEndMouseDown(...)`                                                       | override     | end mouse-down phase      |
|  [15]   | `MouseCallback.OnMouseUp(MouseCallbackEventArgs)`                                         | override     | begin mouse-up phase      |
|  [16]   | `MouseCallback.OnEndMouseUp(...)`                                                         | override     | end mouse-up phase        |
|  [17]   | `MouseCallbackEventArgs.ViewportPoint`                                                    | read         | callback viewport point   |
|  [18]   | `MouseCallbackEventArgs.IsOverGumball()`                                                  | read         | test gumball hover        |
|  [19]   | `MouseCursor.SetToolTip(string)`                                                          | read         | set cursor tooltip        |

[ENTRYPOINT_SCOPE]: in-viewport UI objects

- rail: host-boundary native-ui

| [INDEX] | [SURFACE]                                                                 | [CALL_SHAPE] | [CAPABILITY]                  |
| :-----: | :------------------------------------------------------------------------ | :----------- | :---------------------------- |
|  [01]   | `UserInterfaceObjectBase.RegisterForAllDocuments()`                       | lifecycle    | register across documents     |
|  [02]   | `UserInterfaceObjectBase.Unregister()`                                    | lifecycle    | retire widget                 |
|  [03]   | `UserInterfaceObjectBase.OnDraw(DrawEventArgs)`                           | override     | draw through display pipeline |
|  [04]   | `UserInterfaceObjectBase.OnMouseClick(MouseState)`                        | override     | handle picked click           |
|  [05]   | `UserInterfaceObjectBase.OnMouseDoubleClick(MouseState)`                  | override     | handle picked double-click    |
|  [06]   | `UserInterfaceObjectBase.OnMouseDown(MouseState)`                         | override     | handle picked mouse-down      |
|  [07]   | `UserInterfaceObjectBase.OnMouseMove(MouseState)`                         | override     | handle picked mouse-move      |
|  [08]   | `UserInterfaceObjectBase.OnMouseUp(MouseState)`                           | override     | handle picked mouse-up        |
|  [09]   | `UserInterfaceObjectBase.BoundToActiveView`                               | state        | bind active view              |
|  [10]   | `UserInterfaceObjectBase.Visible`                                         | state        | control visibility            |
|  [11]   | `GripUserInterfaceObject.SetSnapPoints(IEnumerable<Point3d>)`             | grip         | set snap points               |
|  [12]   | `GripUserInterfaceObject.Constrain(Curve)`                                | grip         | constrain grip curve          |
|  [13]   | `GripUserInterfaceObject.GripLocation`                                    | grip         | read grip location            |
|  [14]   | `GripUserInterfaceObject.GripRadius`                                      | grip         | read grip radius              |
|  [15]   | `GripUserInterfaceObject.ObjectSnapPermitted`                             | grip         | read object-snap permission   |
|  [16]   | `DirectionGripUserInterfaceObject.ArrowsVisibleInViewport(RhinoViewport)` | grip         | test arrow visibility         |
|  [17]   | `DirectionGripUserInterfaceObject.GripDirection`                          | grip         | read grip direction           |
|  [18]   | `DirectionGripUserInterfaceObject.ArrowRadius`                            | grip         | read arrow radius             |
|  [19]   | `RotationGripUserInterfaceObject.ArcVisibleInViewport(RhinoViewport)`     | grip         | test arc visibility           |
|  [20]   | `RotationGripUserInterfaceObject.OnRotationDrag(double, MouseState)`      | grip         | rotation-drag hook            |
|  [21]   | `UserInterfaceControl.SetSvg(string)`                                     | control      | set SVG resource              |
|  [22]   | `UserInterfaceSlider.Range`                                               | control      | read slider range             |
|  [23]   | `UserInterfaceSlider.Value`                                               | control      | read slider value             |
|  [24]   | `UserInterfaceSlider.ValueChanged`                                        | event        | value-changed event           |
|  [25]   | `UserInterfaceSlider.OnValueChanged()`                                    | control      | value-change hook             |
|  [26]   | `MouseState.IsMouseOver(Curve, out double)`                               | hit test     | hit-test curve                |
|  [27]   | `MouseState.IsMouseOver(Line)`                                            | hit test     | hit-test line                 |
|  [28]   | `MouseState.Button`                                                       | state        | read mouse button             |
|  [29]   | `MouseState.FrustumLine`                                                  | state        | read frustum line             |
|  [30]   | `MouseState.View`                                                         | state        | read picked view              |

[ENTRYPOINT_SCOPE]: status, toolbar, resources, and UI thread

- rail: host-boundary native-ui

The trailing `kind` channel of `DrawingUtilities.CreateLinetypePreviewGeometryEx` selects `0` dash fill, `1` curve-shape stroke, or `2` text-shape even-odd fill.

| [INDEX] | [SURFACE]                                                                                        | [KIND]   | [CAPABILITY]            |
| :-----: | :----------------------------------------------------------------------------------------------- | :------- | :---------------------- |
|  [01]   | `StatusBar.ShowProgressMeter(uint, int, int, string, bool, bool)`                                | status   | show progress meter     |
|  [02]   | `StatusBar.UpdateProgressMeter(uint, string, int, bool)`                                         | status   | update progress meter   |
|  [03]   | `StatusBar.HideProgressMeter(uint)`                                                              | status   | hide progress meter     |
|  [04]   | `RuiUpdateUi.RegisterMenuItem(Guid, Guid, Guid, UpdateMenuItemEventHandler)`                     | menu     | register menu item      |
|  [05]   | `RuiUpdateUi.Enabled`                                                                            | menu     | mutate enabled state    |
|  [06]   | `RuiUpdateUi.Checked`                                                                            | menu     | mutate checked state    |
|  [07]   | `RuiUpdateUi.RadioChecked`                                                                       | menu     | mutate radio state      |
|  [08]   | `RuiUpdateUi.Text`                                                                               | menu     | mutate item text        |
|  [09]   | `ToolbarFileCollection.Open(string)`                                                             | toolbar  | open `.rui` file        |
|  [10]   | `ToolbarFileCollection.FindByPath(string)`                                                       | toolbar  | find `.rui` file        |
|  [11]   | `ToolbarFile.GetToolbar(int)`                                                                    | toolbar  | index toolbar           |
|  [12]   | `DrawingUtilities.BitmapFromSvg(string, int, int, bool)`                                         | resource | rasterize SVG bitmap    |
|  [13]   | `DrawingUtilities.PixelsFromSvg(string, int, int, bool, Color, bool)`                            | resource | rasterize SVG pixels    |
|  [14]   | `DrawingUtilities.CreateMeshPreviewImage(RhinoDoc, IEnumerable<Mesh>, IEnumerable<Color>, Size)` | resource | create mesh preview     |
|  [15]   | `DrawingUtilities.CreateLinetypePreviewGeometryEx(Curve, Linetype, int, int, double, int)`       | resource | create linetype preview |
|  [16]   | `RhinoApp.InvokeOnUiThread(Delegate, params object[])`                                           | thread   | marshal to UI thread    |
|  [17]   | `RhinoApp.InvokeAndWait(Action)`                                                                 | thread   | marshal synchronously   |
|  [18]   | `RhinoApp.IsOnMainThread`                                                                        | thread   | test main thread        |
|  [19]   | `RhinoView.ShowToast(...)`                                                                       | thread   | show viewport toast     |

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

# [RASM_RHINO_API_RHINO_UI]

`Rhino.UI` owns Rhino host integration for native chrome: panel and page registration and lifecycle, `RhinoEtoApp` window ownership, the multi-value and document-scoped native dialogs, the gumball manipulator, the mouse-callback and in-viewport interaction surface, status-bar and toolbar/RUI state, SVG and preview resources, locale-aware formatting, and UI-thread marshaling. The `EtoExtensions` styling and window-binding bridge and the single-value prompts are the registered branch surface; the Eto framework composes through them and is never re-implemented here.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: RhinoCommon host-UI-bridge surface (`Rhino.UI` companion)
- host: Rhino host runtime, in-process (proprietary McNeel SDK); `Rhino.UI` is the companion assembly
- assembly: `Rhino.UI.dll` (`RhinoEtoApp`, dialog and control hosts)
- assembly: `RhinoCommon.dll` (`Panels`, `StatusBar`, `StackedDialogPage`, `DrawingUtilities`, gumball, mouse, toolbar, UI-thread)
- namespace: `Rhino.UI` (panels, dialogs, pages, mouse, status, toolbar, resources, in-viewport UI objects)
- namespace: `Rhino.UI.Gumball` (`GumballObject`, `GumballDisplayConduit`, `GumballFrame`)
- namespace: `Rhino.UI.Controls` (`EtoCollapsibleSection`(+`Holder`) section hosts; the full `Rhino.UI.dll` control library is `api-rhino-ui-controls.md`)
- namespace: `Rhino.UI.Controls.DataSource` (`ProviderIds` provider-identity roster, `EventArgs`/`EventInfoArgs` payloads — `RhinoCommon.dll`)
- asset: host-resolved managed reference; the boundary composes it, the manifest never pins it
- rail: host-boundary native-ui

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: panels, pages, and the Eto host bridge

| [INDEX] | [SYMBOL]                        | [KIND]          | [CAPABILITY]                          |
| :-----: | :------------------------------ | :-------------- | :------------------------------------ |
|  [01]   | `Panels`                        | panel registry  | complete panel lifecycle              |
|  [02]   | `IPanel`                        | panel contract  | shown/hidden/closing callbacks        |
|  [03]   | `PanelType`                     | discriminant    | panel host type                       |
|  [04]   | `RhinoEtoApp`                   | window owner    | document-owned Eto parent             |
|  [05]   | `StackedDialogPage`             | stacked page    | nested page activation and navigation |
|  [06]   | `OptionsDialogPage`             | options page    | document/application page host        |
|  [07]   | `ObjectPropertiesPage`          | properties page | selection-driven page hooks           |
|  [08]   | `ObjectPropertiesPageEventArgs` | properties args | selection-event projection            |
|  [09]   | `PropertyPageType`              | discriminant    | properties-page type                  |
|  [10]   | `EtoCollapsibleSection`         | section         | collapsible page section              |
|  [11]   | `EtoCollapsibleSectionHolder`   | section host    | properties/options section stack      |
|  [12]   | `LocalizeStringPair`            | localized label | English/localized caption pair        |

- Registers the `Rhino.UI` host-bridge seams (`libs/csharp/.api/api-rhino-ui.md`): `EtoExtensions.UseRhinoStyle`, `Show`/`GetRhinoDoc` document binding, `ShowSemiModal`, `SavePosition`/`RestorePosition`/`LocalizeAndRestore`, `WindowsFromDocument<T>`, and the `Dialogs` edit and number value prompts carry their algebra there; `RhinoEtoApp` supplies the window ownership those members present against, and the rows here are the subsystem this boundary adds beyond the bridge.

[PUBLIC_TYPE_SCOPE]: dialogs, gumball, and mouse interaction

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
|  [09]   | `Localization`                 | locale service   | language id + unit formatting    |

[PUBLIC_TYPE_SCOPE]: RDK data-source provider identities
- namespace: `Rhino.UI.Controls.DataSource`

`DataSource.ProviderIds` mints the RDK data-provider identity `Guid` roster a UI data source binds; `DataSource.EventArgs`/`EventInfoArgs` are the read-only event payloads a provider raises.

| [INDEX] | [SYMBOL]                   | [KIND]        | [CAPABILITY]                                                            |
| :-----: | :------------------------- | :------------ | :---------------------------------------------------------------------- |
|  [01]   | `DataSource.ProviderIds`   | Guid roster   | RDK provider ids: sun, environment, render, content, decal, post-effect |
|  [02]   | `DataSource.EventArgs`     | event payload | read-only `DataType` on a provider event                                |
|  [03]   | `DataSource.EventInfoArgs` | event payload | read-only `DataType` and native `EventInfoPtr`                          |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: `Panels`, pages, and the Eto host bridge

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
|  [35]   | `ThemeSettings.ThemeChanged`                                              | theme        | light/dark transition edge     |
|  [36]   | `StackedDialogPage.AddChildPage(StackedDialogPage)`                       | page         | append child page              |
|  [37]   | `StackedDialogPage.MakeActivePage()`                                      | page         | activate page                  |
|  [38]   | `StackedDialogPage.OnActivate(bool)`                                      | page         | page activation hook           |
|  [39]   | `StackedDialogPage.SetActivePageTo(string, bool)`                         | page         | navigate stacked-page tree     |
|  [40]   | `ObjectPropertiesPage.ShouldDisplay(...)`                                 | page         | selection display predicate    |
|  [41]   | `ObjectPropertiesPage.UpdatePage(...)`                                    | page         | update properties page         |
|  [42]   | `ObjectPropertiesPage.ModifyPage(Action<...>)`                            | page         | modify properties page         |
|  [43]   | `ObjectPropertiesPage.GetSelectedObjects(ObjectType)`                     | page         | read selected objects          |
|  [44]   | `ObjectPropertiesPageEventArgs.Document`                                  | page args    | selected document              |
|  [45]   | `ObjectPropertiesPageEventArgs.DocRuntimeSerialNumber`                    | page args    | document runtime serial        |
|  [46]   | `ObjectPropertiesPageEventArgs.EventRuntimeSerialNumber`                  | page args    | event runtime serial           |
|  [47]   | `ObjectPropertiesPageEventArgs.View`                                      | page args    | selected view                  |
|  [48]   | `ObjectPropertiesPageEventArgs.Viewport`                                  | page args    | selected viewport              |
|  [49]   | `ObjectPropertiesPageEventArgs.ObjectCount`                               | page args    | selected object count          |
|  [50]   | `ObjectPropertiesPageEventArgs.GetObjects(ObjectType)`                    | page args    | read filtered objects          |
|  [51]   | `ObjectPropertiesPageEventArgs.GetObjects<T>()`                           | page args    | read typed objects             |
|  [52]   | `ObjectPropertiesPageEventArgs.IncludesObjectsType(ObjectType, bool)`     | page args    | test included object type      |
|  [53]   | `EtoCollapsibleSection.Caption`                                           | section      | override section caption       |
|  [54]   | `EtoCollapsibleSection.SectionHeight`                                     | section      | override section height        |
|  [55]   | `EtoCollapsibleSection.Collapsible`                                       | section      | configure collapsibility       |
|  [56]   | `EtoCollapsibleSection.Hidden`                                            | section      | configure visibility           |
|  [57]   | `EtoCollapsibleSection.InitiallyExpanded`                                 | section      | configure initial expansion    |
|  [58]   | `EtoCollapsibleSection.CommandOptionName`                                 | section      | bind command option            |
|  [59]   | `EtoCollapsibleSectionHolder.Add(ICollapsibleSection)`                    | section      | append section                 |
|  [60]   | `EtoCollapsibleSectionHolder.UseScrollbars`                               | section      | configure scrollbars           |
|  [61]   | `EtoCollapsibleSectionHolder.UseCheckBoxes`                               | section      | configure check boxes          |
|  [62]   | `StackedDialogPage.SetEnglishPageTitle(string)`                           | page         | retitle page                   |
|  [63]   | `StackedDialogPage.Modified` (get/set)                                    | page         | dirty-state flag               |
|  [64]   | `StackedDialogPage.RemovePage()`                                          | page         | remove own page                |
|  [65]   | `StackedDialogPage.NavigationTextColor` (get/set)                         | page         | Windows navigation color       |
|  [66]   | `StackedDialogPage.NavigationTextIsBold` (get/set)                        | page         | Windows navigation bold        |

`ThemeSettings.ThemeChanged` is a public static `EventHandler` field subscribed through `+=`; the `EtoExtensions` notifier behind it is private. Native styling, document-owned presentation, semi-modal display, and window position persistence are the registered branch bridge (`libs/csharp/.api/api-rhino-ui.md`), which the `RhinoEtoApp` parents above present against.

[ENTRYPOINT_SCOPE]: dialogs, gumball, and mouse callbacks

| [INDEX] | [SURFACE]                                                                                 | [CALL_SHAPE] | [CAPABILITY]              |
| :-----: | :---------------------------------------------------------------------------------------- | :----------- | :------------------------ |
|  [01]   | `Dialogs.ShowMessage(...)`                                                                | dialog       | native message dialog     |
|  [02]   | `Dialogs.ShowColorDialog(object, ref Color4f, bool, NamedColorList, OnColorChangedEvent)` | dialog       | live-preview color dialog |
|  [03]   | `Dialogs.ShowMultiListBox(...)`                                                           | dialog       | multi-list selection      |
|  [04]   | `Dialogs.ShowCheckListBox(...)`                                                           | dialog       | check-list selection      |
|  [05]   | `Dialogs.ShowPropertyListBox(...)`                                                        | dialog       | property-list selection   |
|  [06]   | `Dialogs.ShowSelectMultipleLayersDialog(...)`                                             | dialog       | multi-layer selection     |
|  [07]   | `Dialogs.ShowTextDialog(string, string)`                                                  | dialog       | text transcript           |
|  [08]   | `Dialogs.ShowContextMenu(IEnumerable<string>, Point, IEnumerable<int>)`                   | dialog       | context-menu selection    |
|  [09]   | `Dialogs.ShowListBox(string, string, IList)` / `(string, string, IList, object)`          | dialog       | single-list selection     |
|  [10]   | `Dialogs.ShowSelectLayerDialog(ref int, string, bool, bool, ref bool)`                    | dialog       | single-layer selection    |
|  [11]   | `Dialogs.ShowLayerMaterialDialog(RhinoDoc, IEnumerable<int>)`                             | dialog       | layer-material edit       |
|  [12]   | `Dialogs.ShowLineTypes(string, string, RhinoDoc, Guid)`                                   | dialog       | linetype identity choice  |
|  [13]   | `Dialogs.ShowSelectLinetypeDialog(ref int, bool)`                                         | dialog       | linetype index choice     |
|  [14]   | `Dialogs.ShowPrintWidths(string, string)` / `(string, string, double)`                    | dialog       | print-width choice        |
|  [15]   | `Dialogs.ShowSunDialog(Sun)`                                                              | dialog       | sun editor                |
|  [16]   | `OpenFileDialog.ShowOpenDialog()` / `SaveFileDialog.ShowSaveDialog()`                     | dialog       | native file selection     |
|  [17]   | `GumballDisplayConduit.SetBaseGumball(GumballObject, GumballAppearanceSettings)`          | gumball      | seat manipulator          |
|  [18]   | `GumballDisplayConduit.PickGumball(PickContext, GetPoint)`                                | gumball      | pick manipulator          |
|  [19]   | `GumballDisplayConduit.UpdateGumball(Point3d, Line)`                                      | gumball      | update drag from line     |
|  [20]   | `GumballDisplayConduit.UpdateGumball(Plane)`                                              | gumball      | update drag from plane    |
|  [21]   | `MouseCallback.OnMouseMove(MouseCallbackEventArgs)`                                       | override     | begin mouse-move phase    |
|  [22]   | `MouseCallback.OnEndMouseMove(...)`                                                       | override     | end mouse-move phase      |
|  [23]   | `MouseCallback.OnMouseDown(MouseCallbackEventArgs)`                                       | override     | begin mouse-down phase    |
|  [24]   | `MouseCallback.OnEndMouseDown(...)`                                                       | override     | end mouse-down phase      |
|  [25]   | `MouseCallback.OnMouseUp(MouseCallbackEventArgs)`                                         | override     | begin mouse-up phase      |
|  [26]   | `MouseCallback.OnEndMouseUp(...)`                                                         | override     | end mouse-up phase        |
|  [27]   | `MouseCallbackEventArgs.ViewportPoint`                                                    | read         | callback viewport point   |
|  [28]   | `MouseCallbackEventArgs.IsOverGumball()`                                                  | read         | test gumball hover        |
|  [29]   | `MouseCursor.SetToolTip(string)`                                                          | read         | set cursor tooltip        |

- The `Dialogs` single-value prompts — `ShowEditBox` and both `ShowNumberBox` overloads — are the registered branch fast lane (`libs/csharp/.api/api-rhino-ui.md`); the rows above are the multi-value, document-scoped, and resource-scoped dialogs this boundary alone reaches.

[ENTRYPOINT_SCOPE]: in-viewport UI objects

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

`DrawingUtilities.CreateLinetypePreviewGeometryEx` reads a trailing `kind` channel: `0` dash fill, `1` curve-shape stroke, `2` text-shape even-odd fill.

| [INDEX] | [SURFACE]                                                                                        | [CALL_SHAPE] | [CAPABILITY]           |
| :-----: | :----------------------------------------------------------------------------------------------- | :----------- | :--------------------- |
|  [01]   | `StatusBar.ShowProgressMeter(uint, int, int, string, bool, bool)`                                | status       | show progress meter    |
|  [02]   | `StatusBar.UpdateProgressMeter(uint, string, int, bool)`                                         | status       | update progress meter  |
|  [03]   | `StatusBar.HideProgressMeter(uint)`                                                              | status       | hide progress meter    |
|  [04]   | `RuiUpdateUi.RegisterMenuItem(Guid, Guid, Guid, UpdateMenuItemEventHandler)`                     | menu         | register menu item     |
|  [05]   | `RuiUpdateUi.Enabled`                                                                            | menu         | mutate enabled state   |
|  [06]   | `RuiUpdateUi.Checked`                                                                            | menu         | mutate checked state   |
|  [07]   | `RuiUpdateUi.RadioChecked`                                                                       | menu         | mutate radio state     |
|  [08]   | `RuiUpdateUi.Text`                                                                               | menu         | mutate item text       |
|  [09]   | `ToolbarFileCollection.Open(string)`                                                             | toolbar      | open `.rui` file       |
|  [10]   | `ToolbarFileCollection.FindByPath(string)`                                                       | toolbar      | find `.rui` file       |
|  [11]   | `ToolbarFile.GetToolbar(int)`                                                                    | toolbar      | index toolbar          |
|  [12]   | `DrawingUtilities.BitmapFromSvg(string, int, int, bool)`                                         | resource     | rasterize SVG bitmap   |
|  [13]   | `DrawingUtilities.PixelsFromSvg(string, int, int, bool, Color, bool)`                            | resource     | rasterize SVG pixels   |
|  [14]   | `DrawingUtilities.CreateMeshPreviewImage(RhinoDoc, IEnumerable<Mesh>, IEnumerable<Color>, Size)` | resource     | create mesh preview    |
|  [15]   | `DrawingUtilities.CreateLinetypePreviewGeometryEx(Curve, Linetype, int, int, double, int)`       | resource     | linetype preview       |
|  [16]   | `RhinoApp.InvokeOnUiThread(Delegate, params object[])`                                           | thread       | marshal to UI thread   |
|  [17]   | `RhinoApp.InvokeAndWait(Action)`                                                                 | thread       | marshal synchronously  |
|  [18]   | `RhinoApp.IsOnMainThread`                                                                        | thread       | test main thread       |
|  [19]   | `RhinoView.ShowToast(...)`                                                                       | thread       | show viewport toast    |
|  [20]   | `ToolbarFileCollection.FindByName(string, bool)`                                                 | toolbar      | find `.rui` by name    |
|  [21]   | `ToolbarFileCollection.SidebarIsVisible` / `MruSidebarIsVisible` (static get/set)                | toolbar      | sidebar visibility     |
|  [22]   | `ToolbarFile.Save()` / `SaveAs(string)` / `Close(bool prompt)` (each `→ bool`)                   | toolbar      | persist or close file  |
|  [23]   | `ToolbarFile.GetGroup(int)` / `GetGroup(string)`                                                 | toolbar      | index or name a group  |
|  [24]   | `ToolbarGroup.Visible` (get/set) / `IsDocked`                                                    | toolbar      | group visibility state |
|  [25]   | `Toolbar.BitmapSize` / `TabSize` (static `Size` get/set)                                         | toolbar      | global toolbar sizing  |
|  [26]   | `StatusBar.SetMessagePane(string)` / `ClearMessagePane()`                                        | status       | message pane write     |
|  [27]   | `StatusBar.SetDistancePane(double)` / `SetNumberPane(double)` / `SetPointPane(Point3d)`          | status       | value pane writes      |
|  [28]   | `DrawingUtilities.IconFromResource(string, Size, Assembly)`                                      | resource     | load sized icon        |
|  [29]   | `DrawingUtilities.BitmapFromIconResource(string, Size, Assembly)`                                | resource     | load icon bitmap       |
|  [30]   | `DrawingUtilities.ImageFromResource(string, Assembly)`                                           | resource     | load drawing image     |
|  [31]   | `DrawingUtilities.LoadBitmapWithScaleDown(string, int, Assembly)`                                | resource     | load reduced bitmap    |
|  [32]   | `DrawingUtilities.LoadIconWithScaleDown(string, int, Assembly)`                                  | resource     | load reduced icon      |
|  [33]   | `DrawingUtilities.CreateCurvePreviewGeometry(Curve, Linetype, int, int)`                         | resource     | create curve preview   |
|  [34]   | `NamedColorList.Default`                                                                         | resource     | default named palette  |
|  [35]   | `WaitCursor()` / `Dispose()`                                                                     | cursor       | scope host wait cursor |

[ENTRYPOINT_SCOPE]: `Localization` — locale identity and unit-aware formatting

`Localization` is a `RhinoCommon.dll` static in `Rhino.UI`; its `LocalizeString`/`LocalizeCommandName`/`LocalizeDialogItem`/`LocalizeForm` family resolves plug-in XML string tables and returns the English input unchanged when none ship, while the locale-identity and unit-formatting members are table-free.

| [INDEX] | [SURFACE]                                                                                 | [CALL_SHAPE] | [CAPABILITY]                |
| :-----: | :---------------------------------------------------------------------------------------- | :----------- | :-------------------------- |
|  [01]   | `CurrentLanguageId : int`                                                                 | locale       | host language LCID          |
|  [02]   | `RunningAsEnglish : bool`                                                                 | locale       | English-locale discriminant |
|  [03]   | `LogicalSort(string string1, string string2) : int`                                       | locale       | digit-aware sort            |
|  [04]   | `UnitSystemName(UnitSystem, bool capitalize, bool singular, bool abbreviate) : string`    | formatting   | localized unit-system name  |
|  [05]   | `FormatNumber(double, UnitSystem, DistanceDisplayMode, int, bool) : string`               | formatting   | unit-aware number text      |
|  [06]   | `FormatNumber(double, LengthUnit, DistanceDisplayMode, int, bool) : string`               | formatting   | length-unit number text     |
|  [07]   | `FormatNumber(double x) : string`                                                         | formatting   | locale number text          |
|  [08]   | `FormatDistanceAndTolerance(double, UnitSystem, DimensionStyle, bool alternate) : string` | formatting   | style-driven distance text  |
|  [09]   | `FormatArea(double, UnitSystem, DimensionStyle, bool alternate) : string`                 | formatting   | style-driven area text      |
|  [10]   | `FormatVolume(double, UnitSystem, DimensionStyle, bool alternate) : string`               | formatting   | style-driven volume text    |
|  [11]   | `LocalizeString(string english, int contextId) : string`                                  | string map   | plug-in string-table lookup |
|  [12]   | `LocalizeCommandName(string english) : string`                                            | string map   | command-name lookup         |
|  [13]   | `LocalizeCommandOptionName(string english, int contextId) : LocalizeStringPair`           | string map   | option-name pair            |
|  [14]   | `LocalizeCommandOptionValue(string english, int contextId) : LocalizeStringPair`          | string map   | option-value pair           |

[ENTRYPOINT_SCOPE]: RDK data-source provider identities
- namespace: `Rhino.UI.Controls.DataSource`

`ProviderIds` members are static `Guid` getters; a UI data source binds a provider by its identity, and the event payloads carry the changed data type.

- `DataSource.ProviderIds` render-settings ids: `Sun`, `CurrentEnvironment`, `RhinoSettings`, `Skylight`, `GroundPlane`, `Dithering`, `LinearWorkflow`, `RenderChannels`.
- `DataSource.ProviderIds` content and decal ids: `ContentDatabase`, `ContentLookup`, `ContentSelection`, `ContentParam`, `Decals`.
- `DataSource.ProviderIds` rendering-pipeline ids: `RdkRendering`, `RdkRenderingProgress`, `RdkRenderingGamma`, `RdkRenderingToneMapping`, `RdkRenderingPostEffects`, `RdkRenderingPostEffectDOF`, `RdkRenderingPostEffectGlare`.
- `DataSource.EventArgs.DataType : Guid` / `DataSource.EventInfoArgs.EventInfoPtr : nint` — event-payload changed-data-type and native info pointer.

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Native chrome registers once per plug-in in one owner: `Panels.RegisterPanel` seats a panel type, `StackedDialogPage`/`OptionsDialogPage`/`ObjectPropertiesPage` seat pages, and the host resolves instances through `GetPanel`/`GetPanels<T>`; a second registration of the same type is the collapsed form.
- Every Eto surface reaches a Rhino window through one path: `RhinoEtoApp` resolves the document-owned parent and the registered bridge applies native styling and presents the surface against a document; the control tree is authored through the folder Eto catalogs, never re-implemented here.
- Interaction runs two tiers: `MouseCallback` is the document-wide viewport mouse hook with begin/end phase pairs, while `UserInterfaceObjectBase` and its grip/slider subclasses are registered in-viewport widgets that draw through the display pipeline and receive a picked `MouseState`; a gumball is the dedicated third manipulator — a `GumballDisplayConduit` seated from a `GumballObject`, never a hand-rolled grip cluster.
- Every host callback runs on the UI thread: work touching document or UI state from a background context marshals through `RhinoApp.InvokeOnUiThread`/`InvokeAndWait`, gated by `IsOnMainThread`.

[STACKING]:
- `api-rhino-ui`(`../../.api/api-rhino-ui.md`): the registered host bridge — `UseRhinoStyle`, document-owned `Show`/`GetRhinoDoc`, `ShowSemiModal`, position persistence, and the single-value prompts this boundary composes and re-tables none of.
- `api-eto-forms.md`/`api-eto-drawing.md`/`api-eto-runtime.md`: a panel or dialog's content is an Eto control tree from those catalogs; this boundary supplies the `RhinoEtoApp` window ownership the registered bridge presents against.
- `api-languageext.md`(`../../.api/api-languageext.md`): panel registration, page activation, dialog results, and resource loads trap onto the rail — `Try.lift(() => Panels.RegisterPanel(...)).Run()` and `Optional(Dialogs.ShowColorDialog(...)).ToFin(error)`; a dialog result or a loaded preview image crosses as `Fin<A>`, never a nullable host handle.
- `api-thinktecture-runtime-extensions.md`(`../../.api/api-thinktecture-runtime-extensions.md`): host UI enums (`PanelType`, `FloatPanelMode`, `ShowPanelReason`, `MouseButton`, `GumballMode`, `PropertyPageType`, the dialog button/icon selectors) map at the edge to `[SmartEnum]` owners, and a panel/page `Guid` is a `[ValueObject<Guid>]`.
- `api-rhinocommon-display.md`: in-viewport `UserInterfaceObjectBase.OnDraw` receives a `DrawEventArgs` and draws through the same `DisplayPipeline` the display catalog owns, and the gumball is a display conduit — the UI widget is a pipeline participant, not a private renderer.
- `api-rhino-ui-controls.md`: `Rhino.UI.dll`'s full control library composes into a panel or page hosted through this boundary; a `DataSource.ProviderIds` `Guid` binds a UI data source, and an `EventInfoArgs.EventInfoPtr` native pointer traps at the edge, never crossing into a domain signature.

[LOCAL_ADMISSION]:
- `Rhino.UI` types are host handles trapped and mapped at the boundary; a `Panels` id, a `Dialogs` result, or a `MouseState` never enters a domain signature — the domain sees a `Fin<A>`, a bounded owner, or a canonical shape.
- One panel type, one page host, one gumball conduit, and one mouse hook own their concern; a parallel registration or a second hook drawing the same overlay is the collapsed form.
- `Rhino.UI.Controls.DataSource.EventInfoArgs.EventInfoPtr` is a raw native pointer trapped at the boundary, never a domain field; the dead `Rhino.UI.Controls.ThumbnailUI` surface is never admitted.

[RAIL_LAW]:
- Partition: `RhinoCommon` + `Rhino.UI` Rhino host-boundary subsystem over the registered host bridge
- Owns: panel and page registration and lifecycle, `RhinoEtoApp` window ownership, the multi-value and document-scoped native dialogs, the gumball manipulator, mouse callbacks and in-viewport UI objects, status, toolbar, and RUI state, SVG and preview resources, locale-aware formatting, the RDK data-source provider identities, and UI-thread marshaling
- Accept: a panel or page registered once and resolved through the host, an Eto surface parented through `RhinoEtoApp` and presented by the registered bridge, a gumball conduit or in-viewport widget drawing through the display pipeline, host handles trapped through `Try.lift(...).Run()`, and UI work marshaled onto the main thread
- Reject: a re-tabling of the registered host bridge or its single-value prompts, a duplicate registration of one panel or page type, a hand-rolled control where an Eto surface fits, a hand-rolled grip cluster where the gumball or a `UserInterfaceObject` fits, a cross-thread UI mutation without `InvokeOnUiThread`, and a `Panels`/`Dialogs`/`MouseCallback`/`StackedDialogPage` handle escaping into a domain signature

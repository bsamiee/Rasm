# [RASM_RHINO_API_RHINO_UI]

`Rhino.UI` owns Rhino host integration for native chrome: panel and page registration and lifecycle, `RhinoEtoApp` window ownership, the multi-value and document-scoped native dialogs, the gumball manipulator, the mouse-callback and in-viewport interaction surface, status-bar and toolbar/RUI state, SVG and preview resources, locale-aware formatting, and UI-thread marshaling. The `EtoExtensions` styling and window-binding bridge and the single-value prompts are the registered branch surface; the Eto framework composes through them and is never re-implemented here.

## [01]-[PUBLIC_TYPES]

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

- Registers the `Rhino.UI` host-bridge adapters (`libs/dotnet/.api/api-rhino-ui.md`): `EtoExtensions.UseRhinoStyle`, `Show`/`GetRhinoDoc` document binding, `ShowSemiModal`, `SavePosition`/`RestorePosition`/`LocalizeAndRestore`, `WindowsFromDocument<T>`, and the `Dialogs` edit and number value prompts carry their algebra there; `RhinoEtoApp` supplies the window ownership those members present against, and the rows here are the subsystem this boundary adds beyond the bridge.

[PUBLIC_TYPE_SCOPE]: dialogs, gumball, and mouse interaction

| [INDEX] | [SYMBOL]                    | [KIND]          | [CAPABILITY]                     |
| :-----: | :-------------------------- | :-------------- | :------------------------------- |
|  [01]   | `Dialogs`                   | native dialogs  | built-in dialog suite            |
|  [02]   | `OpenFileDialog`            | file dialog     | native open, single or multi     |
|  [03]   | `SaveFileDialog`            | file dialog     | native save, single name         |
|  [04]   | `NamedColorList`            | color palette   | color-dialog palette             |
|  [05]   | `NamedColor`                | named color     | palette entry                    |
|  [06]   | `GumballObject`             | gumball state   | manipulator geometry             |
|  [07]   | `GumballFrame`              | gumball frame   | geometry-derived or planar frame |
|  [08]   | `GumballDisplayConduit`     | gumball conduit | drawing, picking, and transforms |
|  [09]   | `GumballAppearanceSettings` | gumball config  | manipulator appearance           |
|  [10]   | `GumballMode`               | discriminant    | active manipulation mode         |
|  [11]   | `MouseCallback`             | mouse hook      | viewport mouse callbacks         |
|  [12]   | `MouseCallbackEventArgs`    | mouse args      | viewport point and gumball hit   |
|  [13]   | `MouseButton`               | discriminant    | callback button                  |
|  [14]   | `MouseCursor`               | cursor          | tooltip-carrying cursor control  |
|  [15]   | `WaitCursor`                | cursor          | scoped wait cursor               |

[PUBLIC_TYPE_SCOPE]: prompt state

| [INDEX] | [SYMBOL]                        | [KIND]       | [CAPABILITY]                 |
| :-----: | :------------------------------ | :----------- | :--------------------------- |
|  [01]   | `CommandPromptChangedEventArgs` | prompt state | prompt, default, and options |

- Registers the in-viewport UI object family (`libs/dotnet/Rasm.Rhino/.api/api-rhinocommon-custom-objects.md`): `UserInterfaceObjectBase` with its grip, direction, rotation, text-dot, control, and slider derivations, `MouseState`, and `ViewUserInterfaceTable` all carry their members there, beside the custom-object authoring surface sharing their derivation contract. This boundary supplies only the namespace and the pipeline boundary.

[PUBLIC_TYPE_SCOPE]: status, toolbar, and resources

| [INDEX] | [SYMBOL]                       | [KIND]           | [CAPABILITY]                    |
| :-----: | :----------------------------- | :--------------- | :------------------------------ |
|  [01]   | `StatusBar`                    | status chrome    | status panes and progress meter |
|  [02]   | `RuiUpdateUi`                  | menu state       | live menu synchronization       |
|  [03]   | `ToolbarFile`                  | toolbar file     | `.rui` file access              |
|  [04]   | `ToolbarFileCollection`        | toolbar registry | `.rui` open/find collection     |
|  [05]   | `Toolbar`                      | toolbar state    | toolbar identity, global sizing |
|  [06]   | `ToolbarGroup`                 | toolbar group    | group identity and visibility   |
|  [07]   | `DrawingUtilities`             | resource loader  | native UI resource utilities    |
|  [08]   | `RhinoApp` (UI-thread members) | thread marshal   | main-thread dispatch            |
|  [09]   | `RhinoView.ShowToast`          | transient notice | viewport toast                  |
|  [10]   | `Localization`                 | locale service   | language id + unit formatting   |

[PUBLIC_TYPE_SCOPE]: RDK data-source provider identities
- namespace: `Rhino.UI.Controls.DataSource`

`DataSource.ProviderIds` mints the RDK data-provider identity `Guid` roster a UI data source binds; `DataSource.EventArgs`/`EventInfoArgs` are the read-only event payloads a provider raises.

| [INDEX] | [SYMBOL]                   | [KIND]        | [CAPABILITY]                                                            |
| :-----: | :------------------------- | :------------ | :---------------------------------------------------------------------- |
|  [01]   | `DataSource.ProviderIds`   | Guid roster   | RDK provider ids: sun, environment, render, content, decal, post-effect |
|  [02]   | `DataSource.EventArgs`     | event payload | read-only `DataType` on a provider event                                |
|  [03]   | `DataSource.EventInfoArgs` | event payload | read-only `DataType` and native `EventInfoPtr`                          |

## [02]-[ENTRYPOINTS]

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
|  [67]   | `ObjectPropertiesPage.EnglishPageTitle` (abstract) / `LocalPageTitle`     | page         | English and localized titles   |
|  [68]   | `ObjectPropertiesPage.PageControl -> object`                              | page         | host control carrier           |
|  [69]   | `ObjectPropertiesPage.PageType -> PropertyPageType` / `Index -> int`      | page         | page kind and button order     |
|  [70]   | `ObjectPropertiesPage.SupportedTypes -> ObjectType`                       | page         | selection type filter          |
|  [71]   | `ObjectPropertiesPage.AllObjectsMustBeSupported` / `SupportsSubObjects`   | page         | selection admission width      |
|  [72]   | `ObjectPropertiesPage.PageIconEmbeddedResourceString`/`PageIcon(Size)`    | page         | page icon resolution           |
|  [73]   | `ObjectPropertiesPage.OnActivate(bool)` / `OnHelp()`                      | page         | activation and help hooks      |
|  [74]   | `ObjectPropertiesPage.OnCreateParent(nint)`/`OnSizeParent(int, int)`      | page         | native parent hooks            |
|  [75]   | `ObjectPropertiesPage.SelectedObjects -> RhinoObject[]`                   | page         | supported-type selection read  |
|  [76]   | `ObjectPropertiesPage.GetSelectedObjects<T>() -> T[]`                     | page         | typed selection read           |
|  [77]   | `ObjectPropertiesPage.AnySelectedObject<T>()` / `<T>(bool allMatch)`      | page         | typed selection test           |
|  [78]   | `ObjectPropertiesPage.RunScript(ObjectPropertiesPageEventArgs)`           | page         | scripted properties run        |
|  [79]   | `OptionsDialogPage.OptionsPageType -> PageType` (nested `enum`)           | page         | options versus doc properties  |
|  [80]   | `OptionsDialogPage.RunScript(RhinoDoc, RunMode)`                          | page         | scripted options run           |

`ThemeSettings.ThemeChanged` is a public static `EventHandler` field subscribed through `+=`; the `EtoExtensions` notifier behind it is private. Native styling, document-owned presentation, semi-modal display, and window position persistence are the registered branch bridge (`libs/dotnet/.api/api-rhino-ui.md`), which the `RhinoEtoApp` parents above present against.

- `OptionsDialogPage : StackedDialogPage` adds exactly three members — `OptionsPageType`, the nested `enum PageType` (`Options`, `DocumentProperties`), and `RunScript(RhinoDoc, RunMode)`. Every other member an options leaf overrides (`PageControl`, `LocalPageTitle`, `PageImage`, `ShowApplyButton`, `ShowDefaultsButton`, `OnApply`, `OnCancel`, `OnActivate`, `OnDefaults`, `OnHelp`, `OnCreateParent`, `OnSizeParent`) belongs to `StackedDialogPage` and is tabled above; `OptionsPageType` carries an `internal set`, so the host seats the kind and a page reads it.
- `ObjectPropertiesPage` carries four `[Obsolete]` members the boundary never overrides — `Icon` (superseded by `PageIcon(Size)`), `ShouldDisplay(RhinoObject)`, `InitializeControls(RhinoObject)`, and `RunScript(RhinoDoc, RhinoObject[])`. Each is the default target of its live successor (`ShouldDisplay(e)` calls the obsolete arm, `UpdatePage(e)` calls `InitializeControls(null)`, `RunScript(e)` calls the pair-taking arm), so overriding the live member alone is complete and overriding both is the doubled form.
- `ObjectPropertiesPage.PageControl`'s base implementation REFLECTS for a subclass property named `PageControl` typed `System.Windows.Forms.Control` and answers `null` on every other shape, swallowing the exception to the command line. An Eto or `NSView` page therefore overrides `PageControl` outright — inheriting it on macOS binds nothing and reports nothing.
- `ObjectPropertiesPage.SelectedObjects` is `GetSelectedObjects(SupportedTypes)`, so the roster read and the type filter are one decision: widening `SupportedTypes` widens the read, and `AllObjectsMustBeSupported` flips `AnySelectedObject<T>` from any-match to every-match by substituting `ObjectType.AnyObject` for the per-type filter.

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
|  [16]   | `new OpenFileDialog()` / `new SaveFileDialog()`                                           | dialog       | mint file dialog          |
|  [17]   | `OpenFileDialog.DefaultExt`/`FileName`/`Title`/`Filter`/`InitialDirectory` (get/set)      | dialog       | open-dialog text state    |
|  [18]   | `OpenFileDialog.MultiSelect` (get/set) / `FileNames` (get) / `ShowOpenDialog() -> bool`   | dialog       | multi-select open run     |
|  [19]   | `SaveFileDialog.DefaultExt`/`FileName`/`Title`/`Filter`/`InitialDirectory` (get/set)      | dialog       | save-dialog text state    |
|  [20]   | `SaveFileDialog.ShowSaveDialog() -> bool`                                                 | dialog       | save-name run             |
|  [21]   | `new GumballObject()` / `GumballObject.Dispose()`                                         | gumball      | mint and release state    |
|  [22]   | `GumballObject.SetFromBoundingBox(BoundingBox)` / `(Plane, BoundingBox)`                  | gumball      | seat from extents         |
|  [23]   | `GumballObject.SetFromLine/Plane/Arc/Circle/Ellipse/Curve/Extrusion/Light/Hatch(...)`     | gumball      | seat from one carrier     |
|  [24]   | `GumballObject.Frame -> GumballFrame` (get/set)                                           | gumball      | plane, scale grip, mode   |
|  [25]   | `new GumballDisplayConduit()` / `(ActiveSpace)` / `Dispose()`                             | gumball      | mint and release conduit  |
|  [26]   | `GumballDisplayConduit.Enabled` (get/set)                                                 | gumball      | arm conduit participation |
|  [27]   | `GumballDisplayConduit.SetBaseGumball(GumballObject[, GumballAppearanceSettings])`        | gumball      | seat manipulator          |
|  [28]   | `GumballDisplayConduit.PickGumball(PickContext, GetPoint)`                                | gumball      | pick manipulator          |
|  [29]   | `GumballDisplayConduit.UpdateGumball(Point3d, Line)` / `UpdateGumball(Plane)`             | gumball      | update drag, `bool`       |
|  [30]   | `GumballDisplayConduit.TotalTransform` / `GumballTransform`                               | gumball      | cumulative and step xform |
|  [31]   | `GumballDisplayConduit.PreTransform` (get/set) / `InRelocate`                             | gumball      | seed xform, relocate mode |
|  [32]   | `GumballDisplayConduit.BaseGumball` / `Gumball` / `PickResult`                            | gumball      | conduit-owned projections |
|  [33]   | `GumballDisplayConduit.CheckShiftAndControlKeys()`                                        | gumball      | live modifier refresh     |
|  [34]   | `new GumballAppearanceSettings()`                                                         | gumball      | default appearance mint   |
|  [35]   | `MouseCallback.Enabled` (get/set)                                                         | mouse        | arm the viewport hook     |
|  [36]   | `MouseCallback.OnMouseMove/Down/Up(MouseCallbackEventArgs)`                               | override     | begin phase per button    |
|  [37]   | `MouseCallback.OnEndMouseMove/Down/Up(MouseCallbackEventArgs)`                            | override     | end phase per button      |
|  [38]   | `MouseCallback.OnMouseDoubleClick/Enter/Hover/Leave(MouseCallbackEventArgs)`              | override     | atomic pointer phases     |
|  [39]   | `MouseCallbackEventArgs.Cancel` (get/set, `CancelEventArgs`)                              | veto         | suppress default handling |
|  [40]   | `MouseCallbackEventArgs.ViewportPoint` / `View` / `MouseButton` / `Button`                | read         | point, view, and button   |
|  [41]   | `MouseCallbackEventArgs.ShiftKeyDown` / `CtrlKeyDown`                                     | read         | modifier flags            |
|  [42]   | `MouseCallbackEventArgs.IsOverGumball()`                                                  | read         | test gumball hover        |
|  [43]   | `MouseCursor.SetToolTip(string)`                                                          | read         | set cursor tooltip        |

- The `Dialogs` single-value prompts — `ShowEditBox` and both `ShowNumberBox` overloads — are the registered branch fast lane (`libs/dotnet/.api/api-rhino-ui.md`); the rows above are the multi-value, document-scoped, and resource-scoped dialogs this boundary alone reaches.
- `MouseCallback.Enabled` is the sole arming member and it REFLECTS over the subclass to subscribe only the `RhinoView` static events whose overrides the subclass declares, so a hook that declares no override arms nothing; a `false` write detaches all ten unconditionally, and every callback runs on the host UI thread.
- `MouseCallbackEventArgs` derives `System.ComponentModel.CancelEventArgs`, so the pointer callback IS veto-capable: a begin-phase override setting `Cancel = true` suppresses Rhino's own default handling of that event, and the matching `OnEnd*` override reads `Cancel` to learn whether the default ran. The read is the evidence, the write the veto — a page claiming the mouse callback is observe-only contradicts the type's own base.
- `GumballObject` and `GumballDisplayConduit` are both `IDisposable` with public constructors, and both `SetFrom*` and `UpdateGumball*` answer `bool`; `GumballAppearanceSettings` carries the whole look as plain settables behind a public parameterless constructor — per-axis translate, rotate, and scale toggles, `FreeTranslate`, axis colors, `Radius`, arrowhead and grip sizing, `AxisThickness`/`ArcThickness`, menu placement — so appearance is one value the seat call takes, never a mutation sequence.
- The conduit's `BaseGumball`, `Gumball`, and `PickResult` are lazily minted conduit-owned projections, not caller-owned handles: reading one binds it to the conduit for the conduit's lifetime, so a lease disposes the conduit and never those.
- `Rhino.UI.OpenFileDialog` and `SaveFileDialog` are plain `public class` with a public parameterless constructor and NO disposer — they hold a private managed `FileDialogBase` and free the native dialog inside the show call. Their Eto siblings bracket because `Eto.Forms.CommonDialog : Widget : IDisposable`; these two never do, and a `using` over either is the wrong shape rather than a missing one. `SaveFileDialog` carries no multi-select axis at all: `MultiSelect`/`FileNames` are `OpenFileDialog`-only, and a save run reads `FileName` back off the instance.
- `OpenFileDialog.ShowDialog()` and `SaveFileDialog.ShowDialog()` are `[Obsolete]` `System.Windows.Forms.DialogResult` shims over `ShowOpenDialog()`/`ShowSaveDialog()`; the `bool` members are the admitted run, and the `DialogResult` return drags a Windows Forms type across a cross-platform boundary.

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
|  [36]   | `RhinoApp.ToolbarFiles -> ToolbarFileCollection` (static get)                                    | toolbar      | process `.rui` set     |
|  [37]   | `ToolbarFileCollection.Count` / `this[int]` / `GetEnumerator() -> IEnumerator<ToolbarFile>`      | toolbar      | enumerate open files   |
|  [38]   | `ToolbarFile.Id -> Guid` / `Name -> string` / `Path -> string`                                   | toolbar      | file identity, alias   |
|  [39]   | `ToolbarFile.GroupCount -> int` / `ToolbarCount -> int`                                          | toolbar      | file roster arity      |
|  [40]   | `ToolbarFile.GetSvg(Guid imageId, bool darkMode) -> string`                                      | toolbar      | per-image SVG payload  |
|  [41]   | `ToolbarGroup.Id`/`Name`; `Toolbar.Id`/`Name` (each `Guid`/`string` get)                         | toolbar      | group and bar identity |

- `RhinoApp.ToolbarFiles` is the process-wide `.rui` set — a lazily minted `ToolbarFileCollection : IEnumerable<ToolbarFile>` whose `Count`/`this[int]` read the native file registry live, so the collection is a cursor over host state, never a snapshot to retain.
- `ToolbarFile.Path` and `Name` are the SAME native read (`CRhinoUiFile_FileName`) with the `isAlias` flag flipped: `Path` is the file location and `Name` its display alias. A page treating `Name` as a filename addresses nothing.
- `ToolbarFile.GetGroup(int)`/`GetToolbar(int)` answer `null` for an index the host does not resolve, and `GetGroup(string)` is a linear `GroupCount` scan over the index overload with a culture-sensitive `string.Compare` — a name lookup is O(groups) and case-sensitive, so an identity walk keys on `ToolbarGroup.Id`.
- `Toolbar` carries identity only; `BitmapSize`/`TabSize` are STATIC process settings, not per-toolbar state, so writing either re-sizes every toolbar in the session.
- `ToolbarFile.Close(bool prompt)` shows a host yes/no `Dialogs.ShowMessage` when `prompt` is true and answers `false` on a declined close, so the `bool` discriminates user refusal from failure only when `prompt` is false.

[ENTRYPOINT_SCOPE]: `Localization` — locale identity and unit-aware formatting

`Localization` is a `RhinoCommon.dll` static in `Rhino.UI`; its `LocalizeString`/`LocalizeCommandName`/`LocalizeDialogItem`/`LocalizeForm` family resolves plug-in XML string tables and returns the English input unchanged when none ship, while the locale-identity and unit-formatting members are table-free.

| [INDEX] | [SURFACE]                                                                                  | [CALL_SHAPE] | [CAPABILITY]                |
| :-----: | :----------------------------------------------------------------------------------------- | :----------- | :-------------------------- |
|  [01]   | `CurrentLanguageId -> int`                                                                 | locale       | host language LCID          |
|  [02]   | `RunningAsEnglish -> bool`                                                                 | locale       | English-locale discriminant |
|  [03]   | `LogicalSort(string string1, string string2) -> int`                                       | locale       | digit-aware sort            |
|  [04]   | `UnitSystemName(UnitSystem, bool capitalize, bool singular, bool abbreviate) -> string`    | formatting   | localized unit-system name  |
|  [05]   | `FormatNumber(double, UnitSystem, DistanceDisplayMode, int, bool) -> string`               | formatting   | unit-aware number text      |
|  [06]   | `FormatNumber(double, LengthUnit, DistanceDisplayMode, int, bool) -> string`               | formatting   | length-unit number text     |
|  [07]   | `FormatNumber(double x) -> string`                                                         | formatting   | locale number text          |
|  [08]   | `FormatDistanceAndTolerance(double, UnitSystem, DimensionStyle, bool alternate) -> string` | formatting   | style-driven distance text  |
|  [09]   | `FormatArea(double, UnitSystem, DimensionStyle, bool alternate) -> string`                 | formatting   | style-driven area text      |
|  [10]   | `FormatVolume(double, UnitSystem, DimensionStyle, bool alternate) -> string`               | formatting   | style-driven volume text    |
|  [11]   | `LocalizeString(string english, int contextId) -> string`                                  | string map   | plug-in string-table lookup |
|  [12]   | `LocalizeCommandName(string english) -> string`                                            | string map   | command-name lookup         |
|  [13]   | `LocalizeCommandOptionName(string english, int contextId) -> LocalizeStringPair`           | string map   | option-name pair            |
|  [14]   | `LocalizeCommandOptionValue(string english, int contextId) -> LocalizeStringPair`          | string map   | option-value pair           |

[ENTRYPOINT_SCOPE]: RDK data-source provider identities
- namespace: `Rhino.UI.Controls.DataSource`

`ProviderIds` members are static `Guid` getters; a UI data source binds a provider by its identity, and the event payloads carry the changed data type.

- `DataSource.ProviderIds` render-settings ids: `Sun`, `CurrentEnvironment`, `RhinoSettings`, `Skylight`, `GroundPlane`, `Dithering`, `LinearWorkflow`, `RenderChannels`.
- `DataSource.ProviderIds` content and decal ids: `ContentDatabase`, `ContentLookup`, `ContentSelection`, `ContentParam`, `Decals`.
- `DataSource.ProviderIds` rendering-pipeline ids: `RdkRendering`, `RdkRenderingProgress`, `RdkRenderingGamma`, `RdkRenderingToneMapping`, `RdkRenderingPostEffects`, `RdkRenderingPostEffectDOF`, `RdkRenderingPostEffectGlare`.
- `DataSource.EventArgs.DataType -> Guid` / `DataSource.EventInfoArgs.EventInfoPtr -> nint` — event-payload changed-data-type and native info pointer.

## [03]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Native chrome registers once per plug-in in one owner: `Panels.RegisterPanel` seats a panel type, `StackedDialogPage`/`OptionsDialogPage`/`ObjectPropertiesPage` seat pages, and the host resolves instances through `GetPanel`/`GetPanels<T>`; a second registration of the same type is the collapsed form.
- Every Eto surface reaches a Rhino window through one path: `RhinoEtoApp` resolves the document-owned parent and the registered bridge applies native styling and presents the surface against a document; the control tree is authored through the folder Eto catalogs, never re-implemented here.
- Interaction runs three tiers with disjoint owners: `MouseCallback` is the document-wide viewport mouse hook with begin/end phase pairs and a per-event veto, a gumball is the dedicated manipulator — a `GumballDisplayConduit` seated from a `GumballObject`, never a hand-rolled grip cluster — and the registered in-viewport widget family is `libs/dotnet/Rasm.Rhino/.api/api-rhinocommon-custom-objects.md`'s, reached here only as the namespace and pipeline boundary it draws through.
- Every host callback runs on the UI thread: work touching document or UI state from a background context marshals through `RhinoApp.InvokeOnUiThread`/`InvokeAndWait`, gated by `IsOnMainThread`.

[STACKING]:
- `RhinoCommon` value substrate(`libs/dotnet/.api/api-rhinocommon.md`): the `Point3d`/`Plane`/`Line`/`BoundingBox` carriers this boundary threads cross the wire from the substrate; it composes them and re-derives none.
- `libs/dotnet/.api/api-rhino-ui.md`: the registered host bridge — `UseRhinoStyle`, document-owned `Show`/`GetRhinoDoc`, `ShowSemiModal`, position persistence, and the single-value prompts this boundary composes and re-tables none of.
- `libs/dotnet/Rasm.Rhino/.api/api-eto-forms.md`/`libs/dotnet/Rasm.Rhino/.api/api-eto-drawing.md`/`libs/dotnet/Rasm.Rhino/.api/api-eto-runtime.md`: a panel or dialog's content is an Eto control tree from those catalogs; this boundary supplies the `RhinoEtoApp` window ownership the registered bridge presents against.
- `libs/dotnet/.api/api-languageext.md`: panel registration, page activation, dialog results, and resource loads cross through `Try.lift`; nullable results bind `Optional(...).ToFin(Fail: new KernelFault.InvalidResult())`, so a dialog result or loaded preview image crosses as `Fin<A>`, never a nullable host handle.
- `libs/dotnet/.api/api-thinktecture-runtime-extensions.md`: host UI enums (`PanelType`, `FloatPanelMode`, `ShowPanelReason`, `MouseButton`, `GumballMode`, `PropertyPageType`, the dialog button/icon selectors) map at the edge to `[SmartEnum]` owners, and a panel/page `Guid` is a `[ValueObject<Guid>]`.
- `libs/dotnet/Rasm.Rhino/.api/api-rhinocommon-custom-objects.md`: the whole in-viewport widget family — `UserInterfaceObjectBase` and its grip, direction, rotation, text-dot, control, and slider derivations, `MouseState`, and `ViewUserInterfaceTable` — carries its members there beside the custom-object derivation contract it shares; this boundary registers it and tables none of it.
- `libs/dotnet/Rasm.Rhino/.api/api-rhinocommon-display.md`: a widget's `OnDraw` receives a `DrawEventArgs` and draws through the same `DisplayPipeline` the display catalog owns, and the gumball is a display conduit — the UI widget is a pipeline participant, not a private renderer.
- `libs/dotnet/Rasm.Rhino/.api/api-rhino-ui-controls.md`: `Rhino.UI.dll`'s full control library composes into a panel or page hosted through this boundary; a `DataSource.ProviderIds` `Guid` binds a UI data source, and an `EventInfoArgs.EventInfoPtr` native pointer traps at the edge, never crossing into a domain signature.

[LOCAL_ADMISSION]:
- `Rhino.UI` types are host handles trapped and mapped at the boundary; a `Panels` id, a `Dialogs` result, or a `MouseState` never enters a domain signature — the domain sees a `Fin<A>`, a bounded owner, or a canonical shape.
- One panel type, one page host, one gumball conduit, and one mouse hook own their concern; a parallel registration or a second hook drawing the same overlay is the collapsed form.
- `Rhino.UI.Controls.DataSource.EventInfoArgs.EventInfoPtr` is a raw native pointer trapped at the boundary, never a domain field; the dead `Rhino.UI.Controls.ThumbnailUI` surface is never admitted.

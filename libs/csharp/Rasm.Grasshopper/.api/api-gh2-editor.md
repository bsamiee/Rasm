# [RASM_GRASSHOPPER_API_GH2_EDITOR]

`Editor` is the Grasshopper2 singleton shell owning the hosted canvas, open-document set, tabs, layouts, status bar, and the sole Rhino getter handoff through `BeginRhinoGetter`. Its chrome substrate — toolbar bars, category input panels, tooltip frames, stateful vector icons, and `UI.Flex` floating buttons — draws over `Eto.Drawing`, while canvas paint composition and the `IFlexControl` interaction seam ride sibling rails.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: host assembly `Grasshopper2`
- package: `Grasshopper2` (Rhino 9 WIP host plug-in bundle)
- assembly: `Grasshopper2`
- namespace: `Grasshopper2.UI` (`Editor`)
- namespace: `Grasshopper2.UI.Flex` (`FloatingButton`, `FloatingButtonCollection`)
- namespace: `Grasshopper2.UI.Icon`
- namespace: `Grasshopper2.UI.Toolbar`
- namespace: `Grasshopper2.UI.InputPanel`
- namespace: `Grasshopper2.UI.Tooltip`
- namespace: `Rhino` (`RhinoDoc` crossing at `Editor.BeginRhinoGetter`)
- asset: `Grasshopper2.dll` under `Grasshopper2Plugin.rhp`, loaded in the Rhino assembly-load context, drawing chrome over `Eto.Drawing`
- rail: host-grasshopper

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: editor shell

| [INDEX] | [SYMBOL]      | [TYPE_FAMILY] | [CAPABILITY]                         |
| :-----: | :------------ | :------------ | :----------------------------------- |
|  [01]   | `Editor`      | class         | singleton editor shell               |
|  [02]   | `DocumentBag` | class         | open-document set, exposes `Current` |

[PUBLIC_TYPE_SCOPE]: floating buttons

| [INDEX] | [SYMBOL]                   | [TYPE_FAMILY] | [CAPABILITY]           |
| :-----: | :------------------------- | :------------ | :--------------------- |
|  [01]   | `FloatingButton`           | class         | anchored button        |
|  [02]   | `FloatingButtonCollection` | class         | button authority       |
|  [03]   | `FloatingPosition`         | enum          | anchor corner          |
|  [04]   | `FloatingState`            | enum          | button lifecycle state |

[PUBLIC_TYPE_SCOPE]: vector icons

| [INDEX] | [SYMBOL]         | [TYPE_FAMILY] | [CAPABILITY]                     |
| :-----: | :--------------- | :------------ | :------------------------------- |
|  [01]   | `IIcon`          | interface     | stateful vector icon             |
|  [02]   | `AbstractIcon`   | class         | concrete icon base and factories |
|  [03]   | `IconContext`    | class         | draw filter and transform chain  |
|  [04]   | `CodeDiagnostic` | class         | `FromCode` compile diagnostic    |

[PUBLIC_TYPE_SCOPE]: toolbars, input panels, and tooltips

| [INDEX] | [SYMBOL]      | [TYPE_FAMILY] | [CAPABILITY]           |
| :-----: | :------------ | :------------ | :--------------------- |
|  [01]   | `Bar`         | class         | constructible item row |
|  [02]   | `RadioToggle` | class         | toolbar radio item     |
|  [03]   | `TextField`   | class         | toolbar text item      |
|  [04]   | `InputPanel`  | class         | category control panel |
|  [05]   | `Frame`       | class         | static tooltip host    |
|  [06]   | `Nomen`       | class         | label carrier          |
|  [07]   | `BarShortcut` | class         | shortcut carrier       |
|  [08]   | `BarItem`     | class         | toolbar item base      |
|  [09]   | `LazyStrings` | class         | lazy label carrier     |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: editor lifecycle, layout, and Rhino handoff

| [INDEX] | [SURFACE]                                      | [SHAPE]  | [CAPABILITY]                                                 |
| :-----: | :--------------------------------------------- | :------- | :----------------------------------------------------------- |
|  [01]   | `Editor.ShowEditor(bool, string) -> Editor`    | static   | creates or re-shows the editor                               |
|  [02]   | `Editor.Instance -> Editor`                    | static   | live editor, null before first show                          |
|  [03]   | `Editor.ThisOrRhino -> Window`                 | static   | editor or Rhino-host fallback                                |
|  [04]   | `Editor.Canvas -> Canvas`                      | property | hosted canvas                                                |
|  [05]   | `Editor.Documents -> DocumentBag`              | property | open-document set                                            |
|  [06]   | `Editor.Tabs -> TabControl`                    | property | layout tab strip                                             |
|  [07]   | `Editor.StatusBar -> StatusBar`                | property | status bar                                                   |
|  [08]   | `Editor.DefinedLayouts -> IEnumerable<string>` | static   | settings-backed layout names                                 |
|  [09]   | `Editor.InitialLayout -> string`               | static   | active layout-rules name                                     |
|  [10]   | `Editor.MostRecentActiveDocument -> string`    | property | recent active document path                                  |
|  [11]   | `Editor.MostRecentLoadedDocuments -> string[]` | property | recent loaded document paths                                 |
|  [12]   | `Editor.MostRecentCount -> int`                | property | loadable recent-path tally                                   |
|  [13]   | `Editor.Collapsed -> bool`                     | property | shell-collapse toggle                                        |
|  [14]   | `Editor.ShowNotes -> bool`                     | property | note-visibility toggle                                       |
|  [15]   | `Editor.BeginRhinoGetter(RhinoDoc) -> bool`    | static   | arbitrates one getter; `false` on no target or active getter |

[ENTRYPOINT_SCOPE]: floating buttons

| [INDEX] | [SURFACE]                                                                | [SHAPE]  | [CAPABILITY]                        |
| :-----: | :----------------------------------------------------------------------- | :------- | :---------------------------------- |
|  [01]   | `FloatingButtonCollection.Add(FloatingPosition, string, ...)`            | instance | mints corner-anchored button        |
|  [02]   | `FloatingButtonCollection.AddAnchored(PointF, string, ...)`              | instance | mints point-anchored button         |
|  [03]   | `FloatingButtonCollection.FindByName(string) -> FloatingButton`          | instance | resolves by name                    |
|  [04]   | `FloatingButtonCollection.FindByPoint(PointF) -> FloatingButton`         | instance | resolves by hit point               |
|  [05]   | `FloatingButtonCollection.IsDefined(string) -> bool`                     | instance | probes name                         |
|  [06]   | `FloatingButtonCollection.Show(params string[])`                         | instance | shows named buttons                 |
|  [07]   | `FloatingButtonCollection.Hide(params string[])`                         | instance | hides named buttons                 |
|  [08]   | `FloatingButtonCollection.Close(params string[])`                        | instance | removes named buttons               |
|  [09]   | `FloatingButtonCollection.CloseAll()`                                    | instance | removes every button                |
|  [10]   | `FloatingButtonCollection.ModifyInfo(string, string)`                    | instance | mutates button info                 |
|  [11]   | `FloatingButtonCollection.ModifyIcon(string, IIcon)`                     | instance | mutates button icon                 |
|  [12]   | `FloatingButtonCollection.ModifyColour(string, Color)`                   | instance | mutates button colour               |
|  [13]   | `FloatingButtonCollection.ModifyAnchor(string, PointF, bool)`            | instance | mutates button anchor               |
|  [14]   | `FloatingButtonCollection.StateCount(FloatingState) -> int`              | instance | counts lifecycle state              |
|  [15]   | `FloatingButtonCollection.Names -> IEnumerable<string>`                  | property | projects names                      |
|  [16]   | `FloatingButtonCollection.Buttons -> IEnumerable<FloatingButton>`        | property | projects buttons                    |
|  [17]   | `FloatingButtonCollection.VisibleButtons -> IEnumerable<FloatingButton>` | property | projects visible buttons            |
|  [18]   | `FloatingButton.MakeNumeric(UiNumber, string)`                           | instance | binds `NumericValue`/`ValueChanged` |
|  [19]   | `FloatingButton.ModifyAnchor(PointF, bool)`                              | instance | mutates anchor                      |
|  [20]   | `FloatingButton.ModifyColour(Color)`                                     | instance | mutates colour                      |
|  [21]   | `FloatingButton.Show()`                                                  | instance | shows button                        |
|  [22]   | `FloatingButton.Hide()`                                                  | instance | hides button                        |
|  [23]   | `FloatingButton.Close()`                                                 | instance | removes button                      |

- `FloatingButton`: `Name` `Info` `Icon` `Colour` `Anchor` `State` `Enabled` display accessors.

[ENTRYPOINT_SCOPE]: vector icons

| [INDEX] | [SURFACE]                                                                            | [SHAPE]  | [CAPABILITY]              |
| :-----: | :----------------------------------------------------------------------------------- | :------- | :------------------------ |
|  [01]   | `AbstractIcon.FromResource(Type)`                                                    | static   | loads resource icon       |
|  [02]   | `AbstractIcon.FromFile(string) -> IIcon`                                             | static   | loads file icon           |
|  [03]   | `AbstractIcon.FromStream(Stream) -> IIcon`                                           | static   | loads stream icon         |
|  [04]   | `AbstractIcon.FromBitmap(params Bitmap[]) -> IIcon`                                  | static   | loads raster frames       |
|  [05]   | `AbstractIcon.FromCode(string, out CodeDiagnostic[], out CodeDiagnostic[]) -> IIcon` | static   | compiles vector code      |
|  [06]   | `IIcon.States -> IEnumerable<IconState>`                                             | property | enumerates keyed poses    |
|  [07]   | `IIcon.FindState(string) -> IconState`                                               | instance | resolves keyed pose       |
|  [08]   | `IIcon.SetState(double, string)`                                                     | instance | jumps to keyed pose       |
|  [09]   | `IIcon.MoveState(double, string, Duration?, Motion?)`                                | instance | animates to keyed pose    |
|  [10]   | `IIcon.Draw(IconContext)`                                                            | instance | renders filter context    |
|  [11]   | `IIcon.DrawToBitmap(Size, int, Color) -> Bitmap`                                     | instance | renders owned bitmap      |
|  [12]   | `IconContext.WithDisabledFilter() -> IconContext`                                    | instance | derives disabled context  |
|  [13]   | `IconContext.WithGreyscaleFilter() -> IconContext`                                   | instance | derives greyscale context |
|  [14]   | `IconContext.WithFadingFilter(Color, float) -> IconContext`                          | instance | derives faded context     |
|  [15]   | `IconContext.WithPalette(IconPalette) -> IconContext`                                | instance | derives palette context   |
|  [16]   | `IconContext.WithFilter(Func<Color, Color>) -> IconContext`                          | instance | derives filtered context  |

- `AbstractIcon.FromCode` out-channels arrive warnings-first; `IIcon.SetState`/`MoveState` select the default pose on a null name.

[ENTRYPOINT_SCOPE]: toolbar, input panel, and tooltip

| [INDEX] | [SURFACE]                                                                                 | [SHAPE]  | [CAPABILITY]                    |
| :-----: | :---------------------------------------------------------------------------------------- | :------- | :------------------------------ |
|  [01]   | `Bar.AddPushButton(IIcon, Nomen, Action, BarShortcut) -> PushButton`                      | instance | appends push button             |
|  [02]   | `Bar.AddRadioToggle(IIcon, Nomen, bool, Action<bool>, BarShortcut) -> RadioToggle`        | instance | appends radio toggle            |
|  [03]   | `Bar.AddTextField(IIcon, Nomen, string, string) -> TextField`                             | instance | appends text field              |
|  [04]   | `Bar.AddToggle(Nomen, bool, params string[]) -> RadioToggle`                              | instance | appends section toggle          |
|  [05]   | `Bar.AddSpacer(Nomen, int, int) -> Spacer`                                                | instance | appends spacer                  |
|  [06]   | `Bar.Add(BarItem)`                                                                        | instance | appends raw item                |
|  [07]   | `Bar.AddLifeColours(Nomen, Family, Action<Family>)`                                       | instance | appends life colours            |
|  [08]   | `Bar.AddCoolColours(Nomen, Family, Action<Family>)`                                       | instance | appends cool colours            |
|  [09]   | `Bar.AddWarmColours(Nomen, Family, Action<Family>)`                                       | instance | appends warm colours            |
|  [10]   | `Bar.AddColours(Nomen, Family[], Family, Action<Family>)`                                 | instance | appends custom colours          |
|  [11]   | `Bar.CreateStandardColourBars(Nomen, Family, Action<Family>) -> (Bar, Bar, Bar)`          | static   | emits life/cool/warm bars       |
|  [12]   | `Bar.Layout()`                                                                            | instance | lays out items                  |
|  [13]   | `Bar.Render(Context)`                                                                     | instance | draws items                     |
|  [14]   | `Bar.ShowTooltipAt(PointF) -> bool`                                                       | instance | shows item tooltip              |
|  [15]   | `Bar.Invalidate()`                                                                        | instance | repaints bar                    |
|  [16]   | `Bar.Find<T>(string) -> T`                                                                | instance | resolves named item             |
|  [17]   | `InputPanel.BeginCategory(string) -> IDisposable`                                         | instance | opens category scope            |
|  [18]   | `InputPanel.AddLabel(string, bool, string) -> Label`                                      | instance | appends label                   |
|  [19]   | `InputPanel.AddCheck(string, bool, Action<bool>, string) -> CheckBox`                     | instance | appends check                   |
|  [20]   | `InputPanel.AddText(string, Action<string>, string) -> TextBox`                           | instance | appends text box                |
|  [21]   | `InputPanel.AddBar(bool, params BarItem[]) -> Bar`                                        | instance | appends embedded bar            |
|  [22]   | `InputPanel.Add(Control)`                                                                 | instance | appends raw control             |
|  [23]   | `InputPanel.MoveCategoryBelow(string, string) -> bool`                                    | instance | moves category                  |
|  [24]   | `InputPanel.RenameCategory(string, string) -> bool`                                       | instance | renames category                |
|  [25]   | `InputPanel.RemoveCategory(string) -> bool`                                               | instance | removes category                |
|  [26]   | `InputPanel.ShowAsForm(Control, PointF, RectangleF) -> Form`                              | instance | floats owned form               |
|  [27]   | `InputPanel.ToEtoControl() -> Control`                                                    | instance | embeds panel                    |
|  [28]   | `Frame.Show(IIcon, string, string, ...) -> void`                                          | static   | shows text/item/painter tooltip |
|  [29]   | `Frame.CreateShortcutPainter(string, Keys, string) -> (Action<Context, Rectangle>, Size)` | static   | mints shortcut painter          |
|  [30]   | `Frame.CreateTextAndIconPainter(object[]) -> (Action<Context, Rectangle>, Size)`          | static   | mints content painter           |
|  [31]   | `Frame.Hide()`                                                                            | static   | hides tooltip                   |
|  [32]   | `Frame.Invalidate()`                                                                      | static   | repaints tooltip                |
|  [33]   | `Frame.Visible`                                                                           | property | probes tooltip                  |
|  [34]   | `Frame.ScreencapFolder`                                                                   | property | aims screen capture             |

- `RadioToggle`: `(IIcon, Nomen, bool, Action<bool>)` ctor; `SetState`/`Toggle`, `StateChanged`, `OnText`/`OffText`/`Optional`.
- `TextField`: `(IIcon, Nomen, string)` ctor; `SetText`, `Placeholder`, `EnterPressed`/`EscapePressed`, `TextChanged`/`ActiveChanged`.

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `Editor` is the singleton shell: `Instance` resolves the live editor, `ThisOrRhino` falls back to the Rhino host, and layouts, tabs, breadcrumbs, and status bar are shell state.
- `FloatingButton`/`FloatingButtonCollection` live in `Grasshopper2.UI.Flex`, not `Canvas`: the collection is the sole mint against the internal button constructor, and layout, occlusion, `Position*`, and `*Ux` channels stay host-internal.
- `IIcon` is a stateful vector icon: `MoveState` animates between keyed poses over a `Duration`/`Motion`, `SetState` jumps, and `Draw` renders through an `IconContext` disabled/greyscale/fading filter chain.
- `Toolbar.Bar` keys item rows by `IIcon` + `Nomen` + `BarShortcut` and draws over `Eto.Drawing.Context`.
- `InputPanel` is category-structured: `BeginCategory` opens a disposable section scope and `ShowAsForm` floats the panel.
- `Tooltip.Frame` is the static host for plain, item-list, and custom-painter tooltips; the painter factories mint reusable content.
- `Editor.BeginRhinoGetter(RhinoDoc)` arbitrates one Rhino getter against the active `RhinoDoc`.

[STACKING]:
- `api-gh2-flex.md`(`.api/api-gh2-flex.md`): `FloatingButton` anchors to an `IFlexControl` host and `IIcon.MoveState` consumes the `Animation` `Motion`/`Duration` vocabulary the flex catalog owns.
- `api-languageext.md`(`.api/api-languageext.md`): the `AddCheck`/`AddText` callbacks fold through `Fin`/`Eff`, `FindByName`/`Find<T>` return `Option`, and the open-document and layout sets are `Seq`/`HashMap`.
- `api-thinktecture-runtime-extensions.md`(`.api/api-thinktecture-runtime-extensions.md`): `FloatingPosition`/`FloatingState`, the icon state key, and the toolbar item kind lower onto `SmartEnum`/`Union` owners.
- `api-unicolour.md`(`.api/api-unicolour.md`): `FloatingButton.ModifyColour` and `CreateStandardColourBars` cross `Eto.Drawing.Color`, blended in a perceptual space.

[LOCAL_ADMISSION]:
- chrome enters through the `Editor` shell and the `UI.Toolbar`/`InputPanel`/`Tooltip`/`Icon` host types; a parallel in-folder toolbar or icon is the deleted form.
- `Editor.BeginRhinoGetter` arbitrates the Rhino getter; a direct `RhinoDoc` getter bypassing the editor is the deleted form.
- icon animation composes the `Animation` `Motion`/`Duration` vocabulary, never a second easing derivation.

[RAIL_LAW]:
- Package: `Grasshopper2` (host assembly)
- Owns: the editor shell (canvas, documents, tabs, breadcrumbs, layouts, status bar), floating-button chrome, stateful vector icons, toolbars, input panels, tooltips, and the `BeginRhinoGetter` handoff
- Accept: editor lifecycle, chrome construction, icon state animation, floating-button placement, the Rhino getter handoff
- Reject: canvas paint composition (`api-gh2-canvas.md`), the `IFlexControl` seam internals (`api-gh2-flex.md`), document mutation, the GH1 `GH_DocumentEditor`/`ToolStrip` chrome idiom

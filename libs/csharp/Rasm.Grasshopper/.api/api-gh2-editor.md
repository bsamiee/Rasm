# [RASM_GRASSHOPPER_API_GH2_EDITOR]

The Grasshopper2 `Editor` is the singleton shell: the hosted canvas, the open-document set, tabs, breadcrumbs, layouts, the status bar, and the single Rhino handoff through `BeginRhinoGetter`. Its chrome is `UI.Toolbar` bars, `UI.InputPanel` category panels, `UI.Tooltip` frames, stateful `UI.Icon` vector icons, and `UI.Flex` floating buttons — the buttons and their layout live in `Grasshopper2.UI.Flex`, not `Canvas`. Every member is catalog-verified against the installed RhinoWIP `Grasshopper2.xml` adjacent to `Grasshopper2.dll`; the GH1 `GH_DocumentEditor`/`ToolStrip` chrome idiom is absent by construction.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: host assembly `Grasshopper2`

- package: `Grasshopper2` (Rhino 9 WIP host plug-in bundle; not a NuGet pin — the in-process `Grasshopper2.dll` under `Grasshopper2Plugin.rhp` is the resolved asset)
- assembly: `Grasshopper2`
- namespace: `Grasshopper2.UI` (`Editor`)
- namespace: `Grasshopper2.UI.Flex` (`FloatingButton`, `FloatingButtonCollection`, `FloatingButtonLayout`)
- namespace: `Grasshopper2.UI.Icon`
- namespace: `Grasshopper2.UI.Toolbar`
- namespace: `Grasshopper2.UI.InputPanel`
- namespace: `Grasshopper2.UI.Tooltip`
- namespace: `Rhino` (`RhinoDoc` crossing at `Editor.BeginRhinoGetter`)
- asset: host assembly; managed WIP plug-in loaded in the Rhino assembly-load context, drawing chrome over `Eto.Drawing`
- rail: host-grasshopper

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: editor shell

- namespace: `Grasshopper2.UI`
- rail: host-grasshopper

[EDITOR]:

- Kind: shell
- Capability: The singleton editor owns `Instance`/`ThisOrRhino`, `Canvas`, `Documents` (`DocumentBag`), `Tabs`, `StatusBar`, the static `DefinedLayouts`/`InitialLayout`, the most-recent path rows with `MostRecentCount`, `Collapsed`, `ShowNotes`, and the static `ShowEditor`/`BeginRhinoGetter`; `BreadCrumbs` and `EnsureVisible` are non-public.

[PUBLIC_TYPE_SCOPE]: floating buttons

- namespace: `Grasshopper2.UI.Flex`
- rail: host-grasshopper

| [INDEX] | [SYMBOL]                   | [KIND]     | [CAPABILITY]           |
| :-----: | :------------------------- | :--------- | :--------------------- |
|  [01]   | `FloatingButton`           | button     | anchored button        |
|  [02]   | `FloatingButtonCollection` | collection | button authority       |
|  [03]   | `FloatingPosition`         | enum       | anchor corner          |
|  [04]   | `FloatingState`            | enum       | button lifecycle state |

[FLOATING_BUTTON]:

- Public: `Name`/`Info`/`Icon`/`Colour`/`Anchor`/`State`/`Enabled`, `MakeNumeric` with `NumericValue` and `ValueChanged`, `ModifyAnchor`/`ModifyColour`, and per-button `Show`/`Hide`/`Close`
- Internal: the constructor, `Position*` placement, `*Ux` channels, and `AnchorChanged`/`ColourChanged`/`StateChanged`

[FLOATING_BUTTON_COLLECTION]:

- Mint: `Add`/`AddAnchored(…, click, mouseDown, mouseUp)`
- Lookup: `FindByName`/`FindByPoint`/`IsDefined`
- Lifecycle: `Show`/`Hide`/`Close`/`CloseAll`
- Mutation: `Modify*`
- Projections: `StateCount`, `Names`/`Buttons`/`VisibleButtons`

[FLOATING_LAYOUT]: `FloatingButtonLayout` is internal, so placement and occlusion remain host-private.

[PUBLIC_TYPE_SCOPE]: vector icons

- namespace: `Grasshopper2.UI.Icon`
- rail: host-grasshopper

| [INDEX] | [SYMBOL]       | [KIND]  | [CAPABILITY]         |
| :-----: | :------------- | :------ | :------------------- |
|  [01]   | `IIcon`        | icon    | stateful vector icon |
|  [02]   | `AbstractIcon` | icon    | concrete icon base   |
|  [03]   | `IconContext`  | context | draw filter chain    |

[ICON_MEMBERS]:

- `IIcon`: `States`/`FindState`/`SetState`/`MoveState` animate between keyed poses, and `Draw`/`DrawToBitmap` render.
- `AbstractIcon`: `FromResource`/`FromCode` factories and `MoveState`.
- `IconContext`: `WithDisabledFilter`/`WithGreyscaleFilter`/`WithFadingFilter`.

[PUBLIC_TYPE_SCOPE]: toolbars, input panels, and tooltips

- namespace: `Grasshopper2.UI.Toolbar`, `Grasshopper2.UI.InputPanel`, `Grasshopper2.UI.Tooltip`
- rail: host-grasshopper

| [INDEX] | [SYMBOL]      | [KIND]  | [CAPABILITY]           |
| :-----: | :------------ | :------ | :--------------------- |
|  [01]   | `Bar`         | toolbar | constructible item row |
|  [02]   | `RadioToggle` | item    | toolbar radio item     |
|  [03]   | `TextField`   | item    | toolbar text item      |
|  [04]   | `InputPanel`  | panel   | category control panel |
|  [05]   | `Frame`       | tooltip | static tooltip host    |
|  [06]   | `Nomen`       | value   | label carrier          |
|  [07]   | `BarShortcut` | value   | shortcut carrier       |
|  [08]   | `BarItem`     | value   | toolbar item carrier   |
|  [09]   | `LazyStrings` | value   | lazy label carrier     |

[BAR]:

- Construction: `Bar()`/`Bar(params BarItem[])`, `Add(BarItem)`, `AddPushButton`/`AddRadioToggle`/`AddTextField`/`AddSpacer`/`AddToggle`, the `AddLifeColours`/`AddCoolColours`/`AddWarmColours`/`AddColours` rows, and `CreateStandardColourBars(out life, cool, warm)`
- Lookup: `Find<T>`/`ItemAt`/indexers
- State: `Enabled`/`ElementHeight`/`Style`
- Operations: `Layout`/`Render`/`ShowTooltipAt`/`Invalidate`
- Events: `Invalidated`/`TooltipDetails`/`CloseRequested`

[RADIO_TOGGLE]: The public `(IIcon, Nomen, bool, Action<bool>)` constructor composes `SetState`/`Toggle`, `StateChanged`, `OnText`/`OffText`/`Optional`.

[TEXT_FIELD]: The public `(IIcon, Nomen, string)` constructor composes `SetText`, `Placeholder`, `EnterPressed`/`EscapePressed`, and `TextChanged`/`ActiveChanged`.

[INPUT_PANEL]: `BeginCategory` opens a disposable scope; `AddLabel`/`AddCheck`/`AddText`/`AddBar`/`Add(Control)` return typed controls, category move/rename/remove operations return `bool`, `ShowAsForm` returns `Form`, and `ToEtoControl` returns the embedded control.

[TOOLTIP_FRAME]: `Show` accepts plain, item, or painter content with trailing `warnings`/`errors` flags; `Hide`/`Invalidate`/`Visible`/`ScreencapFolder` manage the host, and `CreateShortcutPainter` (`Keys` or `char`)/`CreateTextAndIconPainter` return `(painter, size)`.

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: editor lifecycle, layout, and Rhino handoff

- namespace: `Grasshopper2.UI`
- rail: host-grasshopper

| [INDEX] | [SURFACE]                          | [CAPABILITY]              |
| :-----: | :--------------------------------- | :------------------------ |
|  [01]   | `Editor.ShowEditor`                | creates or returns editor |
|  [02]   | `Editor.Instance`                  | resolves live editor      |
|  [03]   | `Editor.ThisOrRhino`               | resolves host fallback    |
|  [04]   | `Editor.Canvas`                    | resolves hosted canvas    |
|  [05]   | `Editor.Documents`                 | resolves open documents   |
|  [06]   | `Editor.Tabs`                      | resolves tab strip        |
|  [07]   | `Editor.StatusBar`                 | resolves status bar       |
|  [08]   | `Editor.DefinedLayouts`            | enumerates layouts        |
|  [09]   | `Editor.InitialLayout`             | resolves startup layout   |
|  [10]   | `Editor.MostRecentActiveDocument`  | resolves active path      |
|  [11]   | `Editor.MostRecentLoadedDocuments` | resolves recent paths     |
|  [12]   | `Editor.Collapsed`                 | toggles shell collapse    |
|  [13]   | `Editor.ShowNotes`                 | toggles note visibility   |
|  [14]   | `Editor.BeginRhinoGetter`          | arbitrates Rhino getter   |

[EDITOR_CALL_SHAPES]:

- `Editor.ShowEditor`: static `(bool createVisible = true, string layoutRules = null)` → `Editor`; re-shows a hidden editor.
- `Editor.Instance` / `Editor.ThisOrRhino`: static property (`Editor` / `Eto.Forms.Window`); `EnsureVisible` is internal.
- `Editor.Canvas` / `Editor.Documents`: property (`Canvas` / `DocumentBag` with `Current`).
- `Editor.Tabs` / `Editor.StatusBar`: property (`TabbedPanel.TabControl` / `StatusBar`); `BreadCrumbs` is private.
- `Editor.DefinedLayouts` / `Editor.InitialLayout`: static property (`IEnumerable<string>` / `string`), settings-backed.
- `Editor.MostRecentActiveDocument` / `Editor.MostRecentLoadedDocuments`: property (`string` / `string[]`); `MostRecentCount` tallies loadable paths.
- `Editor.Collapsed` / `Editor.ShowNotes`: property.
- `Editor.BeginRhinoGetter`: static `(RhinoDoc doc = null)` → `bool`; `false` means no target document or an active getter.

[ENTRYPOINT_SCOPE]: floating buttons

- namespace: `Grasshopper2.UI.Flex`
- rail: host-grasshopper

| [INDEX] | [SURFACE]                                 | [CAPABILITY]             |
| :-----: | :---------------------------------------- | :----------------------- |
|  [01]   | `FloatingButtonCollection.Add`            | mints button             |
|  [02]   | `FloatingButtonCollection.AddAnchored`    | mints anchored button    |
|  [03]   | `FloatingButton.MakeNumeric`              | binds numeric channel    |
|  [04]   | `FloatingButton.ModifyAnchor`             | mutates anchor           |
|  [05]   | `FloatingButton.ModifyColour`             | mutates colour           |
|  [06]   | `FloatingButton.Show`                     | shows button             |
|  [07]   | `FloatingButton.Hide`                     | hides button             |
|  [08]   | `FloatingButton.Close`                    | removes button           |
|  [09]   | `FloatingButtonCollection.FindByName`     | resolves name            |
|  [10]   | `FloatingButtonCollection.FindByPoint`    | resolves hit point       |
|  [11]   | `FloatingButtonCollection.IsDefined`      | probes name              |
|  [12]   | `FloatingButtonCollection.Show`           | shows named buttons      |
|  [13]   | `FloatingButtonCollection.Hide`           | hides named buttons      |
|  [14]   | `FloatingButtonCollection.Close`          | removes named buttons    |
|  [15]   | `FloatingButtonCollection.CloseAll`       | removes every button     |
|  [16]   | `FloatingButtonCollection.ModifyInfo`     | mutates button info      |
|  [17]   | `FloatingButtonCollection.ModifyIcon`     | mutates button icon      |
|  [18]   | `FloatingButtonCollection.ModifyColour`   | mutates button colour    |
|  [19]   | `FloatingButtonCollection.ModifyAnchor`   | mutates button anchor    |
|  [20]   | `FloatingButtonCollection.StateCount`     | counts lifecycle state   |
|  [21]   | `FloatingButtonCollection.Names`          | projects names           |
|  [22]   | `FloatingButtonCollection.Buttons`        | projects buttons         |
|  [23]   | `FloatingButtonCollection.VisibleButtons` | projects visible buttons |

[FLOATING_CALL_SHAPES]:

- `FloatingButtonCollection.Add` / `AddAnchored`: `(FloatingPosition | PointF, string name, string info = null, Color? colour = null, IIcon icon = null, FloatingButtonHandler click/mouseDown/mouseUp = null)`; the collection is the sole mint because the `FloatingButton` constructor is internal.
- `FloatingButton.MakeNumeric`: `(UiNumber number, string valueKey)`; `NumericValue` and the public `ValueChanged` event expose the channel.
- `FloatingButton.ModifyAnchor` / `ModifyColour` / `Show` / `Hide` / `Close`: `(PointF, bool immediate)` / `(Color)` / `()`.
- `FloatingButtonCollection.FindByName` / `FindByPoint` / `IsDefined`: `(string)` / `(PointF)` / `(string)`.
- `FloatingButtonCollection.Show` / `Hide` / `Close` / `CloseAll`: `(params string[])` / `()`.
- `FloatingButtonCollection.ModifyInfo` / `ModifyIcon` / `ModifyColour` / `ModifyAnchor`: `(string, …)`; `ModifyAnchor` is `(string, PointF, bool immediate)`.
- `FloatingButtonCollection.StateCount` / `Names` / `Buttons` / `VisibleButtons`: `(FloatingState)` / property; `FloatingButtonLayout` and the `Position*`/`*Ux` set are internal.

[ENTRYPOINT_SCOPE]: vector icons

- namespace: `Grasshopper2.UI.Icon`
- rail: host-grasshopper

| [INDEX] | [SURFACE]                         | [CAPABILITY]              |
| :-----: | :-------------------------------- | :------------------------ |
|  [01]   | `AbstractIcon.FromResource`       | loads resource icon       |
|  [02]   | `AbstractIcon.FromFile`           | loads file icon           |
|  [03]   | `AbstractIcon.FromStream`         | loads stream icon         |
|  [04]   | `AbstractIcon.FromBitmap`         | loads raster frames       |
|  [05]   | `AbstractIcon.FromCode`           | compiles vector code      |
|  [06]   | `IIcon.States`                    | enumerates keyed poses    |
|  [07]   | `IIcon.FindState`                 | resolves keyed pose       |
|  [08]   | `IIcon.SetState`                  | jumps to keyed pose       |
|  [09]   | `IIcon.MoveState`                 | animates to keyed pose    |
|  [10]   | `IIcon.Draw`                      | renders filter context    |
|  [11]   | `IIcon.DrawToBitmap`              | renders owned bitmap      |
|  [12]   | `IconContext.WithDisabledFilter`  | derives disabled context  |
|  [13]   | `IconContext.WithGreyscaleFilter` | derives greyscale context |
|  [14]   | `IconContext.WithFadingFilter`    | derives faded context     |
|  [15]   | `IconContext.WithPalette`         | derives palette context   |
|  [16]   | `IconContext.WithFilter`          | derives filtered context  |
|  [17]   | `CodeDiagnostic`                  | compile diagnostic row    |

[ICON_CALL_SHAPES]:

- `AbstractIcon.FromResource` / `FromFile` / `FromStream` / `FromBitmap`: `(Type)` / `(string, Type)` / `(string)` / `(Stream)` / `(params Bitmap[])`.
- `AbstractIcon.FromCode`: `(string, out CodeDiagnostic[] warnings, out CodeDiagnostic[] errors)`; out-channels arrive warnings-first.
- `IIcon.States` / `FindState` / `SetState`: `IEnumerable<IconState>` / `(string)` → `IconState` / `(double, string = null)`; a null name selects the default state.
- `IIcon.MoveState`: `(double, string = null, Duration? = null, Motion? = null)`.
- `IIcon.Draw` / `DrawToBitmap`: `(IconContext)` / `(Size size, int padding, Color background)` → `Bitmap`.
- `IconContext`: public `(Context, RectangleF, Color)` constructor with `WithDisabledFilter`/`WithGreyscaleFilter`/`WithFadingFilter(Color, float)`/`WithPalette(IconPalette)`/`WithFilter(Func<Color, Color>)` derivations.
- `CodeDiagnostic`: `Description`/`Location`/`Length`/`Line`/`Column` and `IsWarning`/`IsError`.

[ENTRYPOINT_SCOPE]: toolbar, input panel, and tooltip

- namespace: `Grasshopper2.UI.Toolbar`, `Grasshopper2.UI.InputPanel`, `Grasshopper2.UI.Tooltip`
- rail: host-grasshopper

| [INDEX] | [SURFACE]                        | [CAPABILITY]             |
| :-----: | :------------------------------- | :----------------------- |
|  [01]   | `Bar.AddPushButton`              | appends push button      |
|  [02]   | `Bar.AddRadioToggle`             | appends radio toggle     |
|  [03]   | `Bar.AddTextField`               | appends text field       |
|  [04]   | `Bar.AddSpacer`                  | appends spacer           |
|  [05]   | `Bar.AddToggle`                  | appends section toggle   |
|  [06]   | `Bar.Add`                        | appends raw item         |
|  [07]   | `Bar.AddLifeColours`             | appends life colours     |
|  [08]   | `Bar.AddCoolColours`             | appends cool colours     |
|  [09]   | `Bar.AddWarmColours`             | appends warm colours     |
|  [10]   | `Bar.AddColours`                 | appends custom colours   |
|  [11]   | `Bar.CreateStandardColourBars`   | emits colour-family bars |
|  [12]   | `Bar.Layout`                     | lays out items           |
|  [13]   | `Bar.Render`                     | draws items              |
|  [14]   | `Bar.ShowTooltipAt`              | shows item tooltip       |
|  [15]   | `Bar.Invalidate`                 | repaints bar             |
|  [16]   | `Bar.Find<T>`                    | resolves named item      |
|  [17]   | `InputPanel.BeginCategory`       | opens category scope     |
|  [18]   | `InputPanel.Add*`                | appends typed controls   |
|  [19]   | `InputPanel.MoveCategoryBelow`   | moves category           |
|  [20]   | `InputPanel.RenameCategory`      | renames category         |
|  [21]   | `InputPanel.RemoveCategory`      | removes category         |
|  [22]   | `InputPanel.ShowAsForm`          | floats owned form        |
|  [23]   | `InputPanel.ToEtoControl`        | embeds panel             |
|  [24]   | `Frame.Show`                     | shows tooltip            |
|  [25]   | `Frame.CreateShortcutPainter`    | mints shortcut painter   |
|  [26]   | `Frame.CreateTextAndIconPainter` | mints content painter    |
|  [27]   | `Frame.Hide`                     | hides tooltip            |
|  [28]   | `Frame.Invalidate`               | repaints tooltip         |
|  [29]   | `Frame.Visible`                  | probes tooltip           |
|  [30]   | `Frame.ScreencapFolder`          | aims screen capture      |

[CHROME_CALL_SHAPES]:

- `Bar.AddPushButton` / `AddRadioToggle`: `(IIcon, Nomen, Action = null, BarShortcut = null)` / `(IIcon, Nomen, bool, Action<bool> = null, BarShortcut = null)` → `PushButton`/`RadioToggle`.
- `Bar.AddTextField` / `AddSpacer` / `AddToggle` / `Add`: `(IIcon, Nomen, string initial, string placeholder)` → `TextField` / `(Nomen, int, int)` → `Spacer` / `(Nomen, bool, params string[])` → `RadioToggle` / `(BarItem)`.
- `Bar.AddLifeColours` / `AddCoolColours` / `AddWarmColours` / `AddColours`: `(Nomen, OpenColor.Family, Action<OpenColor.Family>)` / `(Nomen, Family[], Family, Action<Family>)`.
- `Bar.CreateStandardColourBars`: `(Nomen, OpenColor.Family, Action<OpenColor.Family>, out Bar life, out Bar cool, out Bar warm)`.
- `Bar.Layout` / `Render` / `ShowTooltipAt` / `Invalidate` / `Find<T>`: `()` / `(Context)` / `(PointF)` → `bool` / `()` / `(string)` → `T : BarItem`.
- `InputPanel.BeginCategory` / `Add*`: `(string)` → `IDisposable` / `AddLabel(string, bool italic, string)`/`AddCheck(string, bool, Action<bool>, string)`/`AddText(string, Action<string>, string)`/`AddBar(bool, [int,] params BarItem[])`/`Add(Control)` with typed control returns.
- `InputPanel.MoveCategoryBelow` / `RenameCategory` / `RemoveCategory`: `(string category, string above)` / `(string, string)` / `(string)` → `bool`; `false` means a missing category.
- `InputPanel.ShowAsForm` / `ToEtoControl`: `(Control, PointF, RectangleF)` → `Form` / `()` → `Control`.
- `Frame.Show`: static `(IIcon, string, string, [LazyStrings | LazyStrings[] | Action<Context, Rectangle> + Size,] bool warnings = false, bool errors = false)` over text, items, or a custom painter.
- `Frame.CreateShortcutPainter` / `CreateTextAndIconPainter`: `(string, Keys | char, string)` / `(object[])` → `(Action<Context, Rectangle> painter, Size size)`.
- `Frame.Hide` / `Invalidate` / `Visible` / `ScreencapFolder`: static `()` / property.

## [04]-[IMPLEMENTATION_LAW]

[EDITOR_TOPOLOGY]:

- `Editor` is the singleton shell: `Instance` is the live editor, `ThisOrRhino` falls back to the Rhino host, `Canvas` is the hosted canvas, and `Documents` is the open-document set; layouts, tabs, breadcrumbs, and status bar are shell state
- `FloatingButton`/`FloatingButtonCollection` live in `Grasshopper2.UI.Flex`, not `Canvas`: the collection is the sole mint and owns add/find/show/hide/close/tally, the button owns per-instance mutation and the public `ValueChanged` channel; layout, occlusion, `Position*`, and the `*Ux` channels are host-internal
- `IIcon` is a stateful vector icon: `States` enumerates the keyed poses, `MoveState` animates between them over a `Duration`/`Motion`, `SetState` jumps, and `Draw` renders through an `IconContext` disabled/greyscale/fading filter chain
- `Toolbar.Bar` builds item rows keyed by `IIcon` + `Nomen` + `BarShortcut`; `CreateStandardColourBars` emits the three colour-family bars, and `Layout`/`Render` draw over `Eto.Drawing.Context`
- `InputPanel` is category-structured: `BeginCategory` opens a section, the `Add*` family appends controls, categories move/rename/remove, and `ShowAsForm` floats it
- `Tooltip.Frame` shows plain / item-list / custom-painter tooltips, and the painter factories mint reusable content
- `Editor.BeginRhinoGetter(RhinoDoc)` is the single Rhino handoff: the editor arbitrates a Rhino getter against the active `RhinoDoc`

[STACKING]:

- `api-gh2-flex.md`(`.api/api-gh2-flex.md`): `FloatingButton` anchors to an `IFlexControl` host and `IIcon.MoveState` consumes the `Animation` `Motion`/`Duration` vocabulary; the flex catalog owns `IFlexControl` + `Animated<T>`
- `api-languageext.md`(`.api/api-languageext.md`): the `AddCheck`/`AddText` callbacks fold through `Fin`/`Eff`, `FindByName`/`FindBar` return `Option`, and the open-document and layout sets are `Seq`/`HashMap`
- `api-thinktecture-runtime-extensions.md`(`.api/api-thinktecture-runtime-extensions.md`): `FloatingPosition`/`FloatingState`, the icon state key, and the toolbar item kind lower onto `SmartEnum`/`Union` owners
- `api-unicolour.md`(`.api/api-unicolour.md`): `FloatingButton.ModifyColour` and `CreateStandardColourBars` cross `Eto.Drawing.Color`, blended in a perceptual space

[LOCAL_ADMISSION]:

- chrome enters through the `Editor` shell + `UI.Toolbar`/`InputPanel`/`Tooltip`/`Icon` host types; a parallel in-folder toolbar or icon is the deleted form
- the Rhino getter is arbitrated only through `Editor.BeginRhinoGetter`; a direct `RhinoDoc` getter bypassing the editor is the deleted form
- icon animation composes the `Animation` `Motion`/`Duration` vocabulary, never a second easing derivation

[RAIL_LAW]:

- Package: `Grasshopper2` (host assembly)
- Owns: the editor shell (canvas, documents, tabs, breadcrumbs, layouts, status bar), floating-button chrome, stateful vector icons, toolbars, input panels, tooltips, and the `BeginRhinoGetter` handoff
- Accept: editor lifecycle, chrome construction, icon state animation, floating-button placement, the Rhino getter handoff
- Reject: canvas paint composition (`api-gh2-canvas.md`), the `IFlexControl` seam internals (`api-gh2-flex.md`), document mutation, the GH1 `GH_DocumentEditor`/`ToolStrip` chrome idiom

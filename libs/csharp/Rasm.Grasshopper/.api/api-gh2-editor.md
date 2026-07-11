# [RASM_GRASSHOPPER_API_GH2_EDITOR]

The Grasshopper2 `Editor` is the singleton shell: the hosted canvas, the open-document set, tabs, breadcrumbs, layouts,
the status bar, and the single Rhino handoff through `BeginRhinoGetter`. Its chrome is `UI.Toolbar` bars, `UI.InputPanel`
category panels, `UI.Tooltip` frames, stateful `UI.Icon` vector icons, and `UI.Flex` floating buttons — the buttons and
their layout live in `Grasshopper2.UI.Flex`, not `Canvas`. Every member is catalog-verified against the installed
RhinoWIP `Grasshopper2.xml` adjacent to `Grasshopper2.dll`; the GH1 `GH_DocumentEditor`/`ToolStrip` chrome idiom is
absent by construction.

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

| [INDEX] | [SYMBOL] | [KIND] | [CAPABILITY]                                                                                                                                                                                                                                                                                                                   |
| :-----: | :------- | :----- | :----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
|  [01]   | `Editor` | shell  | the singleton editor: `Instance`/`ThisOrRhino`, `Canvas`, `Documents` (`DocumentBag`), `Tabs`, `StatusBar`, the static `DefinedLayouts`/`InitialLayout`, the most-recent path rows + `MostRecentCount`, `Collapsed`, `ShowNotes`, the static `ShowEditor`/`BeginRhinoGetter`; `BreadCrumbs` and `EnsureVisible` are non-public |

[PUBLIC_TYPE_SCOPE]: floating buttons
- namespace: `Grasshopper2.UI.Flex`
- rail: host-grasshopper

| [INDEX] | [SYMBOL]                             | [KIND]     | [CAPABILITY]                                                                                                                                                                                                                                                                                                             |
| :-----: | :----------------------------------- | :--------- | :----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
|  [01]   | `FloatingButton`                     | button     | one anchored button; `Name`/`Info`/`Icon`/`Colour`/`Anchor`/`State`/`Enabled`, `MakeNumeric` + `NumericValue` + `ValueChanged`, `ModifyAnchor`/`ModifyColour`, per-button `Show`/`Hide`/`Close`; the constructor, `Position*` placement, `*Ux` channels, and `AnchorChanged`/`ColourChanged`/`StateChanged` are internal |
|  [02]   | `FloatingButtonCollection`           | collection | the sole button mint and authority; `Add`/`AddAnchored(…, click, mouseDown, mouseUp)`, `FindByName`/`FindByPoint`/`IsDefined`, `Show`/`Hide`/`Close`/`CloseAll`, `Modify*`, `StateCount`, `Names`/`Buttons`/`VisibleButtons`                                                                                             |
|  [03]   | `FloatingPosition` / `FloatingState` | enum       | the anchor corner / lifecycle state a button carries; `FloatingButtonLayout` is internal — placement and occlusion are host-private                                                                                                                                                                                      |

[PUBLIC_TYPE_SCOPE]: vector icons
- namespace: `Grasshopper2.UI.Icon`
- rail: host-grasshopper

| [INDEX] | [SYMBOL]       | [KIND]  | [CAPABILITY]                                                                                                                  |
| :-----: | :------------- | :------ | :---------------------------------------------------------------------------------------------------------------------------- |
|  [01]   | `IIcon`        | icon    | a stateful vector icon; `States`/`FindState`/`SetState`/`MoveState` animate between keyed poses, `Draw`/`DrawToBitmap` render |
|  [02]   | `AbstractIcon` | icon    | the concrete icon base; `FromResource`/`FromCode` factories, `MoveState`                                                      |
|  [03]   | `IconContext`  | context | a draw filter chain; `WithDisabledFilter`/`WithGreyscaleFilter`/`WithFadingFilter`                                            |

[PUBLIC_TYPE_SCOPE]: toolbars, input panels, and tooltips
- namespace: `Grasshopper2.UI.Toolbar`, `Grasshopper2.UI.InputPanel`, `Grasshopper2.UI.Tooltip`
- rail: host-grasshopper

| [INDEX] | [SYMBOL]                                            | [KIND]  | [CAPABILITY]                                                                                                                                                                                                                                                                                                                                                                                                                                     |
| :-----: | :-------------------------------------------------- | :------ | :----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
|  [01]   | `Bar`                                               | toolbar | a constructible item row (`Bar()`/`Bar(params BarItem[])`); `Add(BarItem)`, `AddPushButton`/`AddRadioToggle`/`AddTextField`/`AddSpacer`/`AddToggle`, the `AddLifeColours`/`AddCoolColours`/`AddWarmColours`/`AddColours` rows, `CreateStandardColourBars(out life, cool, warm)`, `Find<T>`/`ItemAt`/indexers, `Enabled`/`ElementHeight`/`Style`, `Layout`/`Render`/`ShowTooltipAt`/`Invalidate`, `Invalidated`/`TooltipDetails`/`CloseRequested` |
|  [02]   | `RadioToggle`                                       | item    | a toolbar radio item; public `(IIcon, Nomen, bool, Action<bool>)` constructor, `SetState`/`Toggle`, `StateChanged`, `OnText`/`OffText`/`Optional`                                                                                                                                                                                                                                                                                                |
|  [03]   | `TextField`                                         | item    | a toolbar text item; public `(IIcon, Nomen, string)` constructor, `SetText`, `Placeholder`, `EnterPressed`/`EscapePressed`, `TextChanged`/`ActiveChanged`                                                                                                                                                                                                                                                                                        |
|  [04]   | `InputPanel`                                        | panel   | a category-structured control panel; `BeginCategory` (disposable scope), `AddLabel`/`AddCheck`/`AddText`/`AddBar`/`Add(Control)` returning their typed controls, category move/rename/remove returning `bool`, `ShowAsForm` → `Form`, `ToEtoControl`                                                                                                                                                                                             |
|  [05]   | `Frame`                                             | tooltip | the STATIC tooltip host; `Show` over plain/item/painter content with trailing `warnings`/`errors` flags, `Hide`/`Invalidate`/`Visible`/`ScreencapFolder`, `CreateShortcutPainter`(`Keys` or `char`)/`CreateTextAndIconPainter` returning `(painter, size)`                                                                                                                                                                                       |
|  [06]   | `Nomen` / `BarShortcut` / `BarItem` / `LazyStrings` | value   | the label, keyboard-shortcut, bar-item, and lazy-label carriers the chrome factories take                                                                                                                                                                                                                                                                                                                                                        |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: editor lifecycle, layout, and Rhino handoff
- namespace: `Grasshopper2.UI`
- rail: host-grasshopper

| [INDEX] | [SURFACE]                                                       | [CALL_SHAPE]                                                               | [CAPABILITY]                                                                       |
| :-----: | :-------------------------------------------------------------- | :------------------------------------------------------------------------- | :--------------------------------------------------------------------------------- |
|  [01]   | `Editor.ShowEditor`                                             | static `(bool createVisible = true, string layoutRules = null)` → `Editor` | creates or returns the editor, re-showing a hidden one                             |
|  [02]   | `Editor.Instance` / `ThisOrRhino`                               | static property (`Editor` / `Eto.Forms.Window`)                            | the live editor, or the Rhino host fallback; `EnsureVisible` is internal           |
|  [03]   | `Editor.Canvas` / `Documents`                                   | property (`Canvas` / `DocumentBag` with `Current`)                         | the hosted canvas and the open-document set                                        |
|  [04]   | `Editor.Tabs` / `StatusBar`                                     | property (`TabbedPanel.TabControl` / `StatusBar`)                          | the tab strip and status bar; `BreadCrumbs` is private                             |
|  [05]   | `Editor.DefinedLayouts` / `InitialLayout`                       | static property (`IEnumerable<string>` / `string`)                         | the layout set and the startup layout, settings-backed                             |
|  [06]   | `Editor.MostRecentActiveDocument` / `MostRecentLoadedDocuments` | property (`string` / `string[]`)                                           | the recent-document paths; `MostRecentCount` tallies loadable ones                 |
|  [07]   | `Editor.Collapsed` / `ShowNotes`                                | property                                                                   | the collapsed-shell and notes-visibility toggles                                   |
|  [08]   | `Editor.BeginRhinoGetter`                                       | static `(RhinoDoc doc = null)` → `bool`                                    | arbitrates a Rhino getter; `false` = no target document or a getter already active |

[ENTRYPOINT_SCOPE]: floating buttons
- namespace: `Grasshopper2.UI.Flex`
- rail: host-grasshopper

| [INDEX] | [SURFACE]                                                                              | [CALL_SHAPE]                                                                                                                                                   | [CAPABILITY]                                                                              |
| :-----: | :------------------------------------------------------------------------------------- | :------------------------------------------------------------------------------------------------------------------------------------------------------------- | :---------------------------------------------------------------------------------------- |
|  [01]   | `FloatingButtonCollection.Add` / `AddAnchored`                                         | `(FloatingPosition \| PointF, string name, string info = null, Color? colour = null, IIcon icon = null, FloatingButtonHandler click/mouseDown/mouseUp = null)` | the SOLE button mint — the `FloatingButton` constructor is internal                       |
|  [02]   | `FloatingButton.MakeNumeric`                                                           | `(UiNumber number, string valueKey)`                                                                                                                           | binds a numeric value channel; `NumericValue` + `ValueChanged` (the one public event)     |
|  [03]   | `FloatingButton.ModifyAnchor` / `ModifyColour` / `Show` / `Hide` / `Close`             | `(PointF, bool immediate)` / `(Color)` / `()`                                                                                                                  | per-button mutation and visibility                                                        |
|  [04]   | `FloatingButtonCollection.FindByName` / `FindByPoint` / `IsDefined`                    | `(string)` / `(PointF)` / `(string)`                                                                                                                           | resolves a button by name or hit point                                                    |
|  [05]   | `FloatingButtonCollection.Show` / `Hide` / `Close` / `CloseAll`                        | `(params string[])` / `()`                                                                                                                                     | toggles or removes named buttons, or all                                                  |
|  [06]   | `FloatingButtonCollection.ModifyInfo` / `ModifyIcon` / `ModifyColour` / `ModifyAnchor` | `(string, …)` — `ModifyAnchor(string, PointF, bool immediate)`                                                                                                 | mutates a named button                                                                    |
|  [07]   | `FloatingButtonCollection.StateCount` / `Names` / `Buttons` / `VisibleButtons`         | `(FloatingState)` / property                                                                                                                                   | the census projections; `FloatingButtonLayout` and the `Position*`/`*Ux` set are internal |

[ENTRYPOINT_SCOPE]: vector icons
- namespace: `Grasshopper2.UI.Icon`
- rail: host-grasshopper

| [INDEX] | [SURFACE]                                                              | [CALL_SHAPE]                                                                                                                                                                               | [CAPABILITY]                                                               |
| :-----: | :--------------------------------------------------------------------- | :----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | :------------------------------------------------------------------------- |
|  [01]   | `AbstractIcon.FromResource` / `FromFile` / `FromStream` / `FromBitmap` | `(Type)` / `(string, Type)` / `(string)` / `(Stream)` / `(params Bitmap[])`                                                                                                                | loads an icon from a resource, file, stream, or raster frames              |
|  [02]   | `AbstractIcon.FromCode`                                                | `(string, out CodeDiagnostic[] warnings, out CodeDiagnostic[] errors)`                                                                                                                     | compiles an icon from vector code — the out-channels arrive warnings-first |
|  [03]   | `IIcon.States` / `FindState` / `SetState`                              | `IEnumerable<IconState>` / `(string)` → `IconState` / `(double, string = null)`                                                                                                            | enumerates, resolves, and jumps to a keyed pose; null name = default state |
|  [04]   | `IIcon.MoveState`                                                      | `(double, string = null, Duration? = null, Motion? = null)`                                                                                                                                | animates to a keyed pose                                                   |
|  [05]   | `IIcon.Draw` / `DrawToBitmap`                                          | `(IconContext)` / `(Size size, int padding, Color background)` → `Bitmap`                                                                                                                  | renders through a filter context / to an owned bitmap                      |
|  [06]   | `IconContext` derivations                                              | `WithDisabledFilter`/`WithGreyscaleFilter`/`WithFadingFilter(Color, float)`/`WithPalette(IconPalette)`/`WithFilter(Func<Color, Color>)`; public `(Context, RectangleF, Color)` constructor | derives a filtered draw context                                            |
|  [07]   | `CodeDiagnostic`                                                       | `Description`/`Location`/`Length`/`Line`/`Column`, `IsWarning`/`IsError`                                                                                                                   | one compile diagnostic row                                                 |

[ENTRYPOINT_SCOPE]: toolbar, input panel, and tooltip
- namespace: `Grasshopper2.UI.Toolbar`, `Grasshopper2.UI.InputPanel`, `Grasshopper2.UI.Tooltip`
- rail: host-grasshopper

| [INDEX] | [SURFACE]                                                                 | [CALL_SHAPE]                                                                                                                                                                                                                                 | [CAPABILITY]                                              |
| :-----: | :------------------------------------------------------------------------ | :------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | :-------------------------------------------------------- |
|  [01]   | `Bar.AddPushButton` / `AddRadioToggle`                                    | `(IIcon, Nomen, Action = null, BarShortcut = null)` / `(IIcon, Nomen, bool, Action<bool> = null, BarShortcut = null)` → `PushButton`/`RadioToggle`                                                                                           | appends a push button / radio toggle                      |
|  [02]   | `Bar.AddTextField` / `AddSpacer` / `AddToggle` / `Add`                    | `(IIcon, Nomen, string initial, string placeholder)` → `TextField` / `(Nomen, int, int)` → `Spacer` / `(Nomen, bool, params string[])` → `RadioToggle` / `(BarItem)`                                                                         | appends a text field, spacer, section toggle, or raw item |
|  [03]   | `Bar.AddLifeColours` / `AddCoolColours` / `AddWarmColours` / `AddColours` | `(Nomen, OpenColor.Family, Action<OpenColor.Family>)` / `(Nomen, Family[], Family, Action<Family>)`                                                                                                                                          | appends the named or custom colour rows                   |
|  [04]   | `Bar.CreateStandardColourBars`                                            | `(Nomen, OpenColor.Family, Action<OpenColor.Family>, out Bar life, out Bar cool, out Bar warm)`                                                                                                                                              | emits the three colour-family bars                        |
|  [05]   | `Bar.Layout` / `Render` / `ShowTooltipAt` / `Invalidate` / `Find<T>`      | `()` / `(Context)` / `(PointF)` → `bool` / `()` / `(string)` → `T : BarItem`                                                                                                                                                                 | lays out, draws, tooltips, repaints, and probes items     |
|  [06]   | `InputPanel.BeginCategory` / `Add*`                                       | `(string)` → `IDisposable` / `AddLabel(string, bool italic, string)`/`AddCheck(string, bool, Action<bool>, string)`/`AddText(string, Action<string>, string)`/`AddBar(bool, [int,] params BarItem[])`/`Add(Control)` — typed control returns | opens a disposable category scope and appends controls    |
|  [07]   | `InputPanel.MoveCategoryBelow` / `RenameCategory` / `RemoveCategory`      | `(string category, string above)` / `(string, string)` / `(string)` → `bool`                                                                                                                                                                 | restructures categories; `false` = missing category       |
|  [08]   | `InputPanel.ShowAsForm` / `ToEtoControl`                                  | `(Control, PointF, RectangleF)` → `Form` / `()` → `Control`                                                                                                                                                                                  | floats the panel as an owned form / embeds it             |
|  [09]   | `Frame.Show` (static)                                                     | `(IIcon, string, string, [LazyStrings \| LazyStrings[] \| Action<Context, Rectangle> + Size,] bool warnings = false, bool errors = false)`                                                                                                   | shows a tooltip over text, items, or a custom painter     |
|  [10]   | `Frame.CreateShortcutPainter` / `CreateTextAndIconPainter`                | `(string, Keys \| char, string)` / `(object[])` → `(Action<Context, Rectangle> painter, Size size)`                                                                                                                                          | mints a reusable tooltip painter with its extent          |
|  [11]   | `Frame.Hide` / `Invalidate` / `Visible` / `ScreencapFolder`               | static `()` / property                                                                                                                                                                                                                       | hides, repaints, probes, and aims screen capture          |

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

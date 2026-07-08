# [RASM_GRASSHOPPER_API_GH2_EDITOR]

Catalog scope: the Grasshopper2 editor shell and chrome — editor lifecycle, toolbars, input panels, tooltips, icons, floating buttons, and the Rhino handoff seam.

[NAMESPACES]:
- `Grasshopper2.UI` — `Editor` (`ShowEditor`, `Instance`, `ThisOrRhino`, `Canvas`, `Documents.Current`, `Collapsed`, `ShowNotes`, `StatusBar`, layout families, most-recent document families, `BeginRhinoGetter`), `ResizingFrame`, `LazyStrings`.
- `Grasshopper2.UI.Canvas` chrome — `DiffCanvas.ShowFileCompareForm`, `History.ShowHistory`, `FloatingButtonCollection` (add/anchored/find/close/show/hide/modify families), `Grasshopper2.UI.Flex.FloatingButton` (name/info/position/state/enabled/colour/anchor/numeric families, `ValueChanged`), `FloatingPosition`/`FloatingState`.
- `Grasshopper2.UI.Toolbar` — `Bar` (push-button/toggle/radio/text-field/colour/spacer add families, `CreateStandardColourBars`, `Layout`, sizing), `PushButton`, `TextField`, `NumberSlider`, `RadioToggle`, `BarShortcut`, `Nomen`.
- `Grasshopper2.UI.InputPanel` — `InputPanel` (`AddLabel`/`AddCheck`/`AddText`/`AddBar`, `Category`, `ShowAsForm`).
- `Grasshopper2.UI.Tooltip` — `Frame` (show overloads: plain/items/panes/painter, `Hide`, `Invalidate`, `ScreencapFolder`, shortcut/text-and-icon painters).
- `Grasshopper2.UI.Icon` — `IIcon` (`Draw`, `DrawToBitmap`), `AbstractIcon.FromResource`, `IconContext` (disabled/greyscale/fading filters), `StandardIcons`.
- `Grasshopper2.Settings` — `CanvasSnapToObjects` and sibling settings rows.
- `Grasshopper2.Parsing` — `Result<Unit>` (inline-edit parse results).
- `Rhino` crossing set — `RhinoDoc` (getter arbitration via `Editor.BeginRhinoGetter`), `Rhino.UI.WaitCursor`, `Rhino.UI.EtoExtensions.UseRhinoStyle`, `Rhino.UI.Dialogs.ShowEditBox`/`ShowNumberBox`.

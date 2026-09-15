# [GUI_PHOTOSHOP]

GUI specification for Adobe Photoshop Beta 27.11.0 (`<photoshop.installFolder>/`, bundle id `com.adobe.Photoshop`, process `<photoshop.processName>`, prefs `<photoshop.prefsFolder>/`, support folder `<photoshop.supportFolder>/`; the four tokens are rows of `.artifacts/creative-cloud/resolved-paths.json`, produced by the `@rasm/creative-cloud-host` `resolve` target, `rasm-integration.md` [04] row 13). Reference layout: `gui-illustrator.md` [03.1] (its mirror-rule column decides every zone). Paths: `<Inputs>` = `plan/inputs`, `<Sources>` = `plan/research/sources`. Every setting row reads `current → target`; current values are the rows of `<Inputs>/photoshop/preferences.json` (ExtendScript `app.preferences` and `app` members, the seven action sets), `<Inputs>/photoshop/application-descriptor.json` (the Action Manager application descriptor under its `application` key: 123 top-level keys, `colorSettings`, `exportAssetsPrefs`, `imageProcessingPrefs`, the preference sub-descriptors `generalPreferences`, `interfacePrefs`, `workspacePreferences`, `toolsPreferences`, `fileSavePrefs`, `cachePrefs`, `guidesPrefs`, `typePreferences`, `experimentalFeatures`, `pluginPicker`, `historyLogPreferences`, `preferences/notificationsPreferences`, and `tools[]` with 113 entries carrying `toolID`, `toolTip`, `inToolBar`), `<Inputs>/photoshop/preset-manager.json` (`presetManager` groups: brushes, swatches, gradients, styles, patterns, contours, tool presets with `class`), and `<Inputs>/ax/photoshop.json` (written by `<Inputs>/ax/run.sh photoshop`; `menus` holds every menu item with its `menubar/…` path, `AXMenuItemCmdChar`, `AXMenuItemCmdModifiers`, and `AXMenuItemMarkChar`; `windowlist`, `tree`, `status`). Machine: display 3024 × 1964 physical, 1800 × 1169 pt logical, menu bar 39 pt, Photoshop window `{0, 39, 1800, 1081}` (the Illustrator reference window is `{0, 39, 1800, 1121}`). Channels: bridge (`architecture.md` [06.2]: `set_preferences` over twelve classes, `get_preferences`, `list_presets`, `run_action`, `batch_play` descriptors, `execute` bodies named per row), ES (`osascript … do javascript`, `app.preferences`, `app.systemInformation`, `executeActionGet`), AX (System Events on the menu bar, on the Preferences dialog with its 21 panes, and on the Cocoa dialogs; the Preferences dialog's AX readability is [09] row 23), CU (computer-use drag or click on Drover-drawn surfaces, read back by a window-id screenshot with zoom), FILE (preference file read or copy with the app quit).

## [01]-[INTENT_AS_RULES]

| [INDEX] | [RULE] | [PHOTOSHOP_MEANING] | [DECISION] | [TEST] |
| :-----: | :----- | :------------------ | :--------- | :----- |
| [01] | No top panel | The only top strip is the Options bar (`Window > Options`, `panelid.static.options`, `control-bar` dock `anchor=top`); it carries the Home button at left, tool options in the middle, and the app-bar icons at right | Options bar stays. Adobe's workspace overview (2026-06-05): "Options bar: Displays settings for the currently selected tool"; the Properties panel carries layer, document, and shape properties, never brush size, opacity, flow, selection mode, or sampling; the Contextual Task Bar covers only the workflows Adobe lists. Hiding it removes tool options with no replacement. Kept minimal: `Enable Narrow Options Bar` on (current on), `Show AI Assisted Editor` off (current off), Home screen auto-show off (current off) | Window > Options checkmark `✓`; screenshot shows one strip, no second bar |
| [02] | Thin help strip at the bottom removed, status bar kept | Photoshop's document-window status bar holds the zoom field and the document readout with its `>` menu; no separate hint strip exists in 27.11 (`toggleStatusBar` is the only related binary string, no menu item) | Nothing to remove. Status bar readout `Document Sizes`. The Info panel's `Show Tool Hints` is moot because Info is closed | Screenshot with a document open shows zoom and `Doc:` readout, no hint text |
| [03] | Rulers fully enabled | `View > Rulers` (⌘R, disabled with no document; per-view state persisted by the app) | On, units Pixels (`rulerUnits rulerPixels` current) | With the template open, AX `AXMenuItemMarkChar` of View > Rulers = `✓` after relaunch |
| [04] | Every tool, two columns, icon-only, in the tool-type sequence | Edit > Toolbar; the Tools panel `>>` chevron toggles single/double column (`Editorial Images.psw` records `size-variant="vertical-narrow"`); the tool-type sequence is Adobe's tools-overview order: selection, crop and slice, measure, retouch, paint, draw and type, navigate | Section [02]: 21 slots holding every one of the 72 `inToolBar` tools of `application-descriptor.json` `tools[]`, Extra Tools empty, extras slot hidden, double column; every slot's letter follows the house scheme of [06.6] | Screenshot: two columns, 11 rows; new `.psw` `size-variant` token differs from `vertical-narrow`; Keyboard Shortcuts dialog Tools tab equals [06.6] |
| [05] | Same panel structure as Illustrator | `gui-illustrator.md` [03.1] mirror rules: zone 3 "a second icon dock holds export and inspection panels; an app with one dock folds them into it"; column A "arrangement group top, properties middle, pages/artboards bottom"; column B "color group top, automations middle, Layers bottom, tallest"; no second icon dock; no floating panel but the Contextual Task Bar | Section [03]: iconic pane A (type, library, inspection panels), TK9 pane B 235 pt (Photoshop-only addition), column C 280 pt holds Properties/Adjustments alone (Photoshop has no Align, Pathfinder, Artboards, or Links panel, so the arrangement and artboard slots are empty), column D 340 pt = Swatches/Color/Gradients → Actions/History → Layers/Channels/Paths | Window and Plugins menu `AXMenuItemMarkChar` `✓` set equals the open panels of [03] (System Events; `app.panelList` and `app.workspaceList` are `undefined` in ExtendScript 27.11); screenshot column order matches |
| [06] | Low-value panels removed whether open or closed | `Window` menu of this build has 37 panel items plus five toggles; the saved `.psw` records every panel id with `closed=true/false` | Every closed verdict in [03] is closed in the saved workspace, not merely hidden | `.psw` decode (`workspace-panels.txt` method) shows `closed=true` for each |
| [07] | Windows we need added and configured | TK9 Multi-Mask and Combo docked; Channels and Paths with Layers; Brush Settings with Brushes | [03] rows 06, 08, 30, 32 and the TK9 rows | Plugins menu `AXMenuItemMarkChar` `✓` on TK9 Multi-Mask and TK9 Combo; `app.systemInformation` lists both `Loaded` |
| [08] | Dark modern UI, smallest elements | Interface: Color Theme, UI Font Size (`Tiny, Small, Medium, Large`, Adobe 2026-02-23), `Scale UI To Font`, Technology Preview `Enable Modern User Interface` | Darkest theme, `Tiny`, scale off, Modern UI on | AX read of the Interface pane after relaunch |
| [09] | Factory presets replaced, Adobe groups deleted | Panels read the resident `.psp` stores; `Presets/` files beside the app are load-on-demand only | Section [06] | `list_presets` per kind returns only our group names |
| [10] | Size catalogue as New Document presets and templates, default type styles | `.psdt` opens as an untitled `.psd` instance (Adobe, Create documents); `New Doc Sizes.json` `user` presets; `Type > Save Default Type Styles` (live menu) | [06] rows 14–17 and [06.1]: one preset and one `.psdt` per row of the InDesign catalogue (`gui-indesign.md` [07]), print rows at 300 ppi, the master digital row and every named screen row at 72 ppi, the family module carried as guides; `Default Template.psdt` is `Digital 3840x2160.psdt` | `open` of each `.psdt` yields `Untitled-1` with the row's pixel size, resolution, profile, and guide count; `New Doc Sizes.json` holds one `user` preset per catalogue row; Load Default Type Styles lists Body, Heading, Caption |
| [11] | Everything saved as `Default *`, exported to Drive | Section [07] | every [07] artifact with a Drive name under `99.Default Profiles/Photoshop/`, the size catalogue under its `Sizes/` | `exports.sha256` rows equal the on-disk hashes |
| [12] | Contextual task bar kept | `Window > Contextual Task Bar` (current `✓`) | On, pinned (bar menu `Pin bar position`; the bar menu strings of this build are `Hide bar`, `Pin bar position`, `Reset bar position`, `ps-binary-strings.txt` `$$$/CxUI/Menu/Common/*`) | Window menu checkmark; screenshot with a document open |
| [13] | No assistant, promo, help, share, comment, learn, Home, Discover surfaces in saved layouts; the rule strikes those surface classes alone, never a tool that edits pixels | Panels: AI Assistant, Adobe Stock, Beta Feedback, Comments, Content Credentials (Beta), Libraries, Version History, Materials closed; `Show AI Assisted Editor` off; Home auto-show off; menu items hidden through Edit > Menus (captured by the workspace, Adobe 2026-02-23 and 2023-05-24). Tools stay whatever model runs them: Remove with its two on-device add-ons ([02] rows 32–33), Select > Subject, Select > Sky, Object Selection, Content-Aware Tracing, Camera Raw Denoise | Fixed by Adobe, no Settings control (`showAIAssistant` and `showEmbeddedDiscoverPanel` exist only as binary strings; the pane read of [09] row 23 records any control that names one): the Home button on the Options bar, the app-bar icons (Beta flask, Share, Notifications bell, Search/Discover, AI Assistant, workspace switcher), the `Discover Panel` UXP extension | Screenshot: none of the closed panels visible; Edit > Menus shows the hidden rows; the fixed icons recorded in [11] as accepted; toolbar slot 08 holds Remove |

## [02]-[TOOLBAR]

Facts of this build: Customize Toolbar dialog (`Edit > Toolbar...`) is AX-visible only for its buttons (`Done`, `Cancel`, `Restore Defaults`, `Clear Tools`, `Save Preset...`, `Load Preset...`, checkbox `Disable Shortcuts for Hidden Toolbar Extras`); the two lists are Drover-drawn, so every move is a computer-use drag. No `Toolbar Customization.psp` exists in the settings folder, so the current toolbar is the 27.11 factory default; the dialog's Extra Tools column is empty. Show row at the bottom: six chips (`…` extras slot, foreground/background, Quick Mask, Screen Mode, chip 5, chip 6). Adobe (2026-02-23): "Drag and drop tools and groups to reorganize the toolbar", "Move less frequently used tools to Extra Tools", "Select Toggle, which shows extra tools in the last toolbar slot", `Save Preset` saves and `Load Preset` opens a custom toolbar layout, `Restore Defaults`, `Clear Tools` "to move all the tools to Extra Tools", `Done`/`Cancel`; the page names no file path. The preset folder is `<photoshop.supportFolder>/Presets/Custom Toolbars/` (present, empty; the binary's `DefaultCustomToolbarPresetsDir=Custom Toolbars` and `load-custom-toolbar-preset`). Whether dragging a sub-tool above its parent makes it the slot's visible tool is [09] row 08. The tool set is `application.tools[]` of `<Inputs>/photoshop/application-descriptor.json`: 113 entries, 72 tools with `inToolBar true` (the 73rd, `editToolbar`, is the `Edit Toolbar...` chip), each with its `toolID` and a `toolTip` that carries the factory letter in parentheses (`Brush Tool (B)`); the entries with `inToolBar false` are workspace-internal tools (Select and Mask, Content-Aware Fill, Neural Filters, Liquify, the adjustment scrubbers, `removeBrushTool`, `distort`, the two `place*` tools) and never sit in a slot. Tool names below are the `toolTip` names without their letter.

Two-column layout fills slots in reading order, two per row. Final order, 21 slots, 11 rows, every one of the 72 tools placed once:

| [INDEX] | [ROW.COL] | [SLOT_TOOLS_IN_ORDER] | [KEY] | [GROUP] | [REASON] |
| :-----: | :-------- | :-------------------- | :---- | :------ | :------- |
| [01] | 1.1 | Move, Artboard | `V` | Move | Factory group, visible tool Move |
| [02] | 1.2 | Rectangular Marquee, Elliptical Marquee, Single Row Marquee, Single Column Marquee | `U` (from `M`, [06.6] row 07; Single Row, Single Column none) | Select: geometric | Factory group |
| [03] | 2.1 | Lasso, Polygonal Lasso, Magnetic Lasso, Selection Brush | `⇧A` (from `L`, [06.6] row 03) | Select: freehand | Factory group reordered: Lasso becomes the visible tool (Selection Brush was first) |
| [04] | 2.2 | Object Selection, Quick Selection, Magic Wand | `W` | Select: automatic | Factory group |
| [05] | 3.1 | Crop, Perspective Crop, Slice, Slice Select | `C` | Crop | Factory group |
| [06] | 3.2 | Frame | `K` | Frame | Single tool, beside Crop (both define bounds) |
| [07] | 4.1 | Eyedropper, Color Sampler, Ruler, Note, Count | `I` | Measure and sample | Factory group |
| [08] | 4.2 | Spot Healing Brush, Healing Brush, Patch, Content-Aware Move, Red Eye, Remove | `J` | Retouch | Factory group; Remove (`removeTool`) runs on the two on-device add-ons of rows 32–33 with Image Processing `Faster` ([05] row 84) |
| [09] | 5.1 | Brush, Pencil, Color Replacement, Mixer Brush, Adjustment Brush | `B`; Pencil `N` (from `B`, [06.6] row 04); Adjustment Brush none | Paint | Factory group; Adjustment Brush stays here |
| [10] | 5.2 | Clone Stamp, Pattern Stamp | `S` | Stamp | Factory group, beside Paint |
| [11] | 6.1 | History Brush, Art History Brush | `Y` | History paint | Factory group |
| [12] | 6.2 | Eraser, Background Eraser, Magic Eraser | `E` | Erase | Factory group |
| [13] | 7.1 | Gradient, Paint Bucket | `G` | Fill | Factory group |
| [14] | 7.2 | Blur, Sharpen, Smudge | none | Focus | Factory group |
| [15] | 8.1 | Dodge, Burn, Sponge | `O` | Tone | Factory group, beside Focus |
| [16] | 8.2 | Pen, Freeform Pen, Curvature Pen, Content-Aware Tracing, Add Anchor Point, Delete Anchor Point, Convert Point | `P` (the three anchor tools none) | Vector: draw | Factory group; Content-Aware Tracing present because Technology Preview `Enable Content-Aware Tracing Tool` is on |
| [17] | 9.1 | Horizontal Type, Vertical Type, Horizontal Type Mask, Vertical Type Mask | `T` | Type | Factory group |
| [18] | 9.2 | Path Selection, Direct Selection | `A` | Vector: select | Factory group, beside Type and Shapes |
| [19] | 10.1 | Rectangle, Ellipse, Triangle, Polygon, Line, Star, Custom Shape | `M` (from `U`, [06.6] row 05); Ellipse `L` ([06.6] row 06); Line `Q` (from `U`, [06.6] row 02) | Shapes | Factory group; Star (`StarTool`, `inToolBar true`) is a member of this build's factory group |
| [20] | 10.2 | Hand, Rotate View | `H`, `R` | Navigate | Factory group |
| [21] | 11.1 | Zoom | `Z` | Navigate | Factory single |

The `[KEY]` column is the factory letter of each tool's `toolTip` in `tools[]`, changed where [06.6] says so; a slot's tools share their letter and `⇧letter` cycles them (`Use Shift Key for Tool Switch`, [05] row 40). The dialog readback is [08] step 08.

| [INDEX] | [DIALOG_ITEM] | [DECISION] | [MECHANICS] |
| :-----: | :------------ | :--------- | :---------- |
| [22] | Extra Tools column | Empty (current empty) | No tool loses its function to another; every tool stays in a slot |
| [23] | `…` extras slot (Show chip 1) | Hidden | Empty extras list; chip click in the Show row (CU) |
| [24] | Foreground/Background chip | Shown | Needed for D and X feedback |
| [25] | Quick Mask chip | Shown | Mask work with TK9 |
| [26] | Screen Mode chip | Hidden | F cycles modes; icon-only footer |
| [27] | Chips 5 and 6 | Hidden | Both are non-tool surfaces added after the four classic chips; their tooltips are [09] row 03 |
| [28] | `Disable Shortcuts for Hidden Toolbar Extras` | Unchecked (current unchecked) | Nothing hidden |
| [29] | Slot order | Drag whole groups in the left list (CU); reorder inside a slot by dragging the sub-tool to the top | One inner drag: Lasso above Selection Brush (row 03); every group keeps its factory membership |
| [30] | Two columns | Tools panel top chevron `>>` (CU click) after `Done` | Recorded in the workspace `.psw` toolbar `size-variant` |
| [31] | Persistence | `Toolbar Customization.psp` + `Toolbar Customization Primary.psp` on quit ("Toolbar Customization Preferences - Contains the user defined settings in the Toolbar dialog", Adobe preference-file page 2024-03-01; neither file exists, so the current toolbar is factory); `Save Preset...` → `Default Toolbar` in `Presets/Custom Toolbars/` | The New Workspace dialog captures Panel Locations, Keyboard Shortcuts, Menus (Adobe 2026-02-23); the `.psw` holds the toolbar only as a dock entry `<toolbar id=4 size-variant=vertical-narrow is-closed=false>` (`workspace-dock-tree.txt` line 12; every factory `.psw` in `Contents/Required/Workspaces/` carries `vertical-narrow`), not the slot list; both `.psp` files and the preset are saved and exported |

Photoshop (Beta) add-ons installed through the Creative Cloud app. Both are on-device model bundles under `AddOnModules/sensei_model_cache/inpainting_ai/` (machine copy `/Library/Application Support/Adobe/Adobe Photoshop (Beta)/`, 686 MB `super_caf/` holding `clio_multidiffusion`, `CMGAN_Tiny_V3`, `GeneralDistractor`, `MDCuration`, `PeopleDistractorV1`, `TiledCMGANSR_640px`, `WireGlobal`, `WireLocal`, and 4.9 GB `ultra_caf/` holding `Nano`, `Nano_data_1`, `Nano_data_2`; user copy `~/Library/Application Support/Adobe/Adobe Photoshop (Beta)/`, 4.1 GB `ultra_caf/` holding `Nano`; `ls` and `du -sh` of the three folders). Neither is a UXP extension (`app.systemInformation` lists none), so `Load Extension Panels` off ([05] row 103) leaves them untouched. What the running app exposes for each, and where it lands:

| [INDEX] | [ADD_ON] | [PANEL] | [TOOL] | [MENU_ITEM] | [PROOF] |
| :-----: | :------- | :------ | :----- | :---------- | :------ |
| [32] | Remove Tool components (`super_caf/`) | none: the Window menu of this build (37 panel items, [03]) gains no item and the `.psw` no panel id; the tool's controls live in the Options bar (`Required/layouts/Painting/Tools/removeBrushToolOptions-4175.exv`; flyouts `Required/layouts/Painting/Flyouts/RemoveToolRemovalAreaFlyout.eve`, `RemoveToolFindDistractionsFlyout.eve`, `RemoveToolGenAIModeFlyout.eve`, `removeBrushToolOptionsFlyout-4176.exv`) and, from 27.9.1, in the Contextual Task Bar (`photoshop-brushes-and-skills.md` [6.4]) | Remove (`removeTool`, `inToolBar true`), slot 08 of the retouch group, the sixth tool in the tool-type sequence of that slot; `removeBrushTool` (`inToolBar false`) is its internal brush and never a slot | none: no menu item of `<Inputs>/ax/photoshop.json` `menus` names the tool; the Contextual Task Bar `Remove` button stays visible ([11] row 03) | `ls` of both `inpainting_ai/` folders; Options bar screenshot with Remove selected shows Size, `Remove after each stroke`, `Sample all layers`, and the `Find distractions` menu (binary strings `$$$/RemoveBrushToolOptions/Name/FindDistractions=Find distractions`, events `Find People in Background`, `Remove Wires and Cables`, `Find General Distractions`) |
| [33] | Remove Tool advanced components (`ultra_caf/`) | none, as row 32 | the same Remove tool: the advanced bundle serves the `Find distractions` modes and the on-device fill quality at Image Processing `Remove` = `Faster` (descriptor `imageProcessingRemoveToolProcessingPrefsStr imageProcessingPerformantMode`, [05] row 84); its generative mode flyout (`RemoveToolGenAIModeFlyout.eve`) is left at the on-device setting | none | descriptor value after the pass; a Remove stroke on the template completes with no download prompt and no cloud call (Plugins > `Allow Extensions to Connect to the Internet` off, [05] row 102, and the run finishes offline) |

## [03]-[PANELS]

Window menu of this build, read by System Events (order as in the menu): Arrange, Workspace, Actions, Adjustments, Adobe Stock, AI Assistant, Beta Feedback, Brush Settings, Brushes, Channels, Character, Character Styles, Clone Source, Color, Comments, Content Credentials (Beta), Glyphs, Gradients, Histogram, History, Info, Layer Comps, Layers, Libraries, Materials, Measurement Log, Navigator, Notes, Paragraph, Paragraph Styles, Paths, Patterns, Properties, Shapes, Styles, Swatches, Timeline, Tool Presets, Version History, AI Assisted Editor, Application Frame `✓`, Options `✓`, Tools `✓`, Contextual Task Bar `✓`. Checked panels now: Color, Layers, Properties. Plugins menu: Plugins Panel, Browse Plugins..., Manage Plugins..., TK9 Combo `✓`, TK9 Cx, TK9 Export, TK9 Multi-Mask `✓`, TK9 My Actions, TK9 My Actions-Tab 1–4 (each with an `info` tooltip panel).

Window > Workspace submenu of this build (`<Inputs>/ax/photoshop.json` `menus`): Core Tools, Editorial Images `✓`, Essentials (Default), Motion, Painting, Photography, Graphic and Web, Reset Editorial Images, New Workspace..., Delete Workspace..., Keyboard Shortcuts & Menus..., Lock Workspace; no restore item on the menu (`Restore Default Workspaces` is a button in Settings > Workspace, AX dump and Adobe 2026-02-23).

Dock structure (right dock, panes inner→outer), each zone from the `gui-illustrator.md` [03.1] mirror rule: pane A iconic (auto-collapse on; the type, library, and inspection panels) → pane B TK9 235 pt (Photoshop-only; Illustrator has no equivalent zone) → pane C 280 pt (Illustrator column A: Photoshop holds only the properties slot) → pane D 340 pt (Illustrator column B: color group top, automations middle, Layers bottom). Widths are decisions of this table (pane B 235, pane C 280, pane D 340 pt; the current layout holds one main column of 346 pt and a TK9 column of 232 pt, read from a window-id screenshot of `windowlist` window `Adobe Photoshop (Beta)` at scale 2); the canvas takes the remainder of the 1800 pt window after the two-column toolbar and pane A. Left dock: toolbar, two columns. Bottom dock: empty (Timeline group removed). Heights at the 1081 pt window, Illustrator's 220/220/remainder pattern: pane D Swatches group 220, Actions group 220, Layers group remainder (≈ 520); pane C Properties group full height; pane B Multi-Mask 480, Combo remainder, Export minimized tab at the bottom.

| [INDEX] | [PANEL] | [ID] | [CURRENT] | [VERDICT] | [PLACE] |
| :-----: | :------ | :--- | :-------- | :-------- | :------ |
| [01] | Swatches | `static.swatches` | open, main pane group 1 | Docked expanded | D, group 1, order 1 (front tab); Illustrator Swatches/Color/Gradient |
| [02] | Color | `static.picker` | open `✓` | Docked expanded | D, group 1, order 2 |
| [03] | Gradients | `static.gradients` | open | Docked expanded | D, group 1, order 3 |
| [04] | Patterns | `static.patterns` | open | Docked iconic | A, order 6; a library panel (Illustrator keeps Brushes, Graphic Styles, Symbols iconic) |
| [05] | Styles | `static.styles` | collapsed group | Docked iconic | A, order 7; Illustrator Graphic Styles is iconic |
| [06] | Shapes | `static.customshapes` | collapsed group | Docked iconic | A, order 8; Illustrator Symbols is iconic |
| [07] | Actions | `static.actions` | collapsed group | Docked expanded | D, group 2, order 1 (automations, middle) |
| [08] | History | `static.history` | collapsed group | Docked expanded | D, group 2, order 2 |
| [09] | Layers | `static.layers` | open `✓` | Docked expanded | D, group 3, order 1, tallest |
| [10] | Channels | `static.channels` | open | Docked expanded | D, group 3, order 2 |
| [11] | Paths | `static.paths` | open | Docked expanded | D, group 3, order 3 |
| [12] | Properties | `static.properties` | open `✓` | Docked expanded | C, group 1, order 1, full height (Illustrator Properties/Appearance, the only column-A slot Photoshop fills) |
| [13] | Adjustments | `static.create` | open | Docked expanded | C, group 1, order 2 |
| [14] | Character | `static.textcharacter` | collapsed group | Docked iconic | A, order 1 (Illustrator Character/Paragraph/OpenType is iconic; Properties carries type options for a type layer) |
| [15] | Paragraph | `static.textparagraph` | collapsed group | Docked iconic | A, order 2 |
| [16] | Glyphs | `static.textglyphspanel` | collapsed group | Docked iconic | A, order 3 (Illustrator Glyphs is iconic) |
| [17] | Character Styles | `static.textcharstyle` | collapsed group | Docked iconic | A, order 4 (Illustrator Paragraph Styles/Character Styles is iconic) |
| [18] | Paragraph Styles | `static.textparastyle` | collapsed group | Docked iconic | A, order 5 |
| [19] | Brushes | `static.brushpresets` | collapsed group | Docked iconic | A, order 9 (Illustrator Brushes is iconic) |
| [20] | Brush Settings | `static.brushstyler` | collapsed group | Docked iconic | A, order 10, grouped with Brushes |
| [21] | TK9 Multi-Mask | `uxp/com.tk.multimask/tkmultimaskv9` | open `✓` | Docked expanded | B, group 1 |
| [22] | TK9 Combo | `uxp/com.tk.comboV8/tkcombocxv9` | open `✓` | Docked expanded | B, group 2 |
| [23] | TK9 Export | `uxp/com.tk.export/tkexportv9` | docked minimized | Docked minimized | B, bottom tab (no flyout form exists in Photoshop) |
| [24] | Clone Source | `static.clonesource` | collapsed group | Docked iconic | A, order 11 (inspection) |
| [25] | Layer Comps | `static.comps` | collapsed group | Docked iconic | A, order 12 |
| [26] | Tool Presets | `static.toolpresets` | collapsed group | Docked iconic | A, order 13 |
| [27] | TK9 My Actions | `uxp/com.tk.myactionsV8/tkmyactions` | docked minimized, unconfigured | Closed | Actions panel holds `Default Actions`; no second action list |
| [28] | TK9 My Actions-Tab 1–4 | `uxp/com.tk.myactionstab1..4` | closed | Closed | Same |
| [29] | TK9 Cx | `uxp/com.tk.cxV8/tkcombocxv9` | closed | Closed | Combo carries the same functions at Multi-Mask width (TK9 manual: "The Combo module is the same width as the Multi-Mask module") |
| [30] | TK9 `info` tooltip panels (five) | `uxp/com.tk.*/toolTips` | closed | Closed | Help surface |
| [31] | Plugins Panel | `uxp/com.adobe.pluginspanel/pluginsPanel` | closed | Closed | Marketplace surface |
| [32] | Info | `static.info` | closed | Closed | Readouts duplicated by the status bar and Properties; hint strip source |
| [33] | Navigator | `static.navigator` | collapsed group | Closed | Zoom field on the status bar, Hand and Zoom tools |
| [34] | Histogram | `static.histogram` | collapsed group | Closed | Curves and Levels show histograms in Properties |
| [35] | Timeline | `static.animation` + `uxp/com.adobe.ccx.timeline/ccxTimeline` | bottom dock, open | Closed | Not a still-image surface |
| [36] | Measurement Log | `static.measurement` | closed | Closed | Same |
| [37] | Notes | `static.annotation` | collapsed group | Closed | Comment surface |
| [38] | Comments | `uxp/com.adobe.ccx.comments-webview/…` | closed | Closed | Comment surface |
| [39] | Libraries | `uxp/com.adobe.cclibrariespanel/ccLibrariesPanel` (`.psw` id 57, `closed=false`); `CC Libraries Panel (Prepared) 4.14.50.0 - from Shared CEP folder` in `system-information.txt` | open, in the Properties group | Closed, and `Load Extension Panels` off so the CEP panel never loads | Promo and sync surface; brushes are not exchanged through it |
| [40] | AI Assistant | (Window item) | closed | Closed | AI surface |
| [41] | Adobe Stock | `uxp/com.adobe.stock.unified.content.panel/…` | closed | Closed | Promo |
| [42] | Beta Feedback | `uxp/com.adobe.bfp.betafeatures/…` | closed | Closed | Feedback surface |
| [43] | Content Credentials (Beta) | `uxp/com.adobe.cai.uxp/panel` | closed | Closed | Credentials prefs `none` |
| [44] | Version History | (Window item) | closed | Closed | Cloud surface |
| [45] | Materials | `uxp/com.adobe.photoshop-material-filters/*` (three) | closed | Closed | Substance surface |
| [46] | Photoshop Utility Panel | `uxp/com.adobe.unifiedpanel/panel` | closed | Closed | Discover and search host |
| [47] | Share sheet panels | `uxp/com.adobe.ccx.sharesheet/invite`, `/review` | closed | Closed | Share surface |
| [48] | OCIO, Patch Match (three), Smart Brush | `static.ocio`, `static.patchmatch*`, `static.smartbrush` | closed | Closed | Workspace-internal panels; never docked |
| [49] | AI Assisted Editor | Window toggle | off (`Show AI Assisted Editor` 0) | Off | Interface pane |
| [50] | Application Frame | Window toggle | `✓` | On | Illustrator parity |
| [51] | Options | Window toggle | `✓` | On | [01] row 01 |
| [52] | Tools | Window toggle | `✓` | On | |
| [53] | Contextual Task Bar | Window toggle | `✓` | On, pinned | [01] row 12 |
| [54] | Workspace > Lock Workspace | Window toggle | off | Off | Lock hides nothing; a locked dock refuses the TK9 tab drags |

## [04]-[PANEL_OPTIONS]

| [INDEX] | [PANEL] | [OPTION] | [CURRENT → TARGET] | [CHANNEL] |
| :-----: | :------ | :------- | :----------------- | :-------- |
| [01] | Layers | Panel Options > Thumbnail Size | `layerThumbnailSize medium` → Small | CU (flyout `Panel Options...`); readback descriptor `layerThumbnailSize` |
| [02] | Layers | Thumbnail Contents | Layer Bounds (default) → Layer Bounds | same |
| [03] | Layers | Use Default Masks on Fill Layers | on → on | same |
| [04] | Layers | Expand New Effects | on → off | same |
| [05] | Layers | Add "copy" to Copied Layers and Groups | on → off | same |
| [06] | Layers | Filter row (`Kind` popup) | shown → shown | fixed |
| [07] | Channels, Paths | Panel Options > Thumbnail Size | Medium → Small | CU |
| [08] | Swatches | View | Small Thumbnail (Adobe, Use the Color and Swatches panels, 2023-05-24) → Small Thumbnail; Show Recent Colors off | CU flyout; `Small Thumbnail` and `Show Recent Colors` are strings of the binary (`ps-binary-strings.txt`) |
| [09] | Gradients, Styles, Shapes, Patterns (iconic, opened from pane A) | View | Small Thumbnail → Small Thumbnail; Show Recent off | CU flyout |
| [10] | Brushes (iconic, opened from pane A) | View | Brush Tip on, Brush Name on, Brush Stroke off, Show Additional Preset Info off, Show Recent Brushes off, thumbnail slider at minimum | CU flyout; `Brush Tip`, `Brush Name`, `Brush Stroke`, `Show Additional Preset Info`, `Show Recent Brushes` are strings of the binary; their placement in the flyout is [09] row 06 |
| [11] | Brush Settings | Live tip preview strip | default → default | none |
| [12] | Actions | Button Mode | off → off; one set expanded | CU flyout |
| [13] | History | History Options | `createFirstSnapshot true`, `nonLinearHistory false` → same; Show New Snapshot Dialog off; Make Layer Visibility Changes Undoable off | `set_preferences` `history.createFirstSnapshot`, `history.nonLinearHistory` (both in the 43, `PreferencesHistory.d.ts`), CU for the two dialog-only boxes |
| [14] | Character, Paragraph (iconic, opened from pane A) | Show options expanded | default → expanded | CU flyout |
| [15] | Tool Presets (iconic) | flyout view (Adobe, Create tool presets: `Show All Tool Presets`, `Sort by Tool`, `Show Current Tool Presets`, `Text Only`, `Small List`, `Large List`) | Show All → `Show Current Tool Presets`; `Small List` | CU flyout |
| [16] | Color | Model | Hue Cube → Hue Cube | none |
| [17] | Properties, Adjustments | defaults | keep | none |
| [18] | Clone Source, Layer Comps | defaults | keep | none |
| [19] | TK9 modules | button saturation | Combo `colorOpacityValue 1` → 1 | TK9 fly-out |

## [05]-[PREFERENCES]

Settings submenu of this build (`Photoshop (Beta) > Settings…`, `<Inputs>/ax/photoshop.json` `menus`): General, Interface, Workspace, Notifications, Tools, History, File Handling, Export, Performance, Image Processing, Scratch Disks, Cursors, Transparency & Gamut, Units & Rulers, Guides, Grid & Slices, Plugins, Type, Enhanced Controls, Technology Previews, Early Access, Product Improvement, then Camera Raw after a separator. The Preferences dialog's left list holds the 21 panes (its list titles are the readback of [09] row 23); `Camera Raw...` opens the Camera Raw Preferences dialog instead. `set_preferences` keys are the 43 documented ones (`photoshop-uxp.md` [06]); ES keys are the `app.preferences` members of `<Inputs>/photoshop/preferences.json`; everything else is AX on the labelled control, and every key named in a row is one of the 43, an `app.preferences` member, or an AX control. Readback column: how the value is proven after relaunch; `descriptor` means the Action Manager application descriptor (`application-descriptor.json` method, `batchPlay` `get` on the application; a named key sits in one of the sub-descriptors the header lists). A `current` value in a row whose readback reads `descriptor` without a key name, or `AX`, is confirmed by the baseline read of [09] row 23 at [08] step 01 before any write; a differing read replaces the value and the row's target stands.

| [INDEX] | [PANE] | [SETTING] | [CURRENT → TARGET] | [CHANNEL] | [READBACK] |
| :-----: | :----- | :-------- | :----------------- | :-------- | :--------- |
| [01] | General | Color Picker | Adobe → Adobe | `general.colorPicker` | `get_preferences` |
| [02] | General | HUD Color Picker | Hue Strip (Small) → Hue Strip (Small) | AX popup | AX |
| [03] | General | Image Interpolation | Bicubic Automatic → Bicubic Automatic | `general.imageInterpolation` | `get_preferences` |
| [04] | General | Auto-Update Open File-based Documents | off → off | `general.autoUpdateOpenDocuments` | `get_preferences` |
| [05] | General | Auto show the Home Screen | off → off | AX checkbox | descriptor `autoShowHomeScreen false` |
| [06] | General | Use Legacy "New Document" Interface | off → off | AX | descriptor `useClassicFileNewDialog false` |
| [07] | General | Skip Transform when Placing | off → off | AX | descriptor `skipTransformSOFromLibrary` |
| [08] | General | Use Legacy Free Transform | off → off | AX | descriptor |
| [09] | General | Beep When Done | off → off | `general.beepWhenDone` | `get_preferences` |
| [10] | General | Export Clipboard | on → on | `general.exportClipboard` | `get_preferences` |
| [11] | General | Resize Image During Place | off → off | AX | descriptor `resizePastePlace false` |
| [12] | General | Always Create Smart Objects when Placing | on → on | AX | descriptor `placeRasterSmartObject true` |
| [13] | General | Create new layer when brushing | off → off | AX | descriptor `nonDestructiveBrushTool false` |
| [14] | Interface | Color Theme | Dark (second of four swatch buttons, descriptor `kuiBrightnessLevel kPanelBrightnessMediumGray`) → Darkest (first swatch) | CU click on the first of the four swatch buttons left of `Highlight Color` (whether the swatches carry an AX title is part of [09] row 23) | descriptor `kuiBrightnessLevel` changes from `kPanelBrightnessMediumGray`; screenshot |
| [15] | Interface | Highlight Color | Blue → Gray | AX popup | descriptor `highlightColorOption` |
| [16] | Interface | Standard Screen Mode canvas color / border | Default / Drop Shadow → Default / Drop Shadow | none | descriptor |
| [17] | Interface | Full Screen canvas color | Black → Black | none | descriptor |
| [18] | Interface | Neutral Color Mode | off → off | AX | AX |
| [19] | Interface | UI Language | English → English | none | `uiLanguageKey` |
| [20] | Interface | UI Font Size | Small (popup `Small`; descriptor `paletteEnhancedFontTypeKey preferSmallPaletteFontType`) → Tiny (`Constants.FontSize.TINY`, `photoshop-uxp.md` [06] row 02; `preferTinyPaletteFontType`, `preferSmallPaletteFontType`, `preferMediumPaletteFontType`, `preferLargePaletteFontType` are the four strings of the binary) | `interface.textFontSize` (restart; the pane says "Changes will take effect the next time you start Photoshop") | `get_preferences`; descriptor `paletteEnhancedFontTypeKey`; screenshot zoom |
| [21] | Interface | Scale UI To Font | off → off | AX | AX |
| [22] | Interface | Show Channels in Color | off → off | `interface.colorChannelsInColor` | `get_preferences` |
| [23] | Interface | Show Menu Colors | on → off | AX | descriptor `showMenuColors` |
| [24] | Interface | Show AI Assisted Editor | off → off | AX | AX checkbox ([09] row 23); the persisted key name is `showAIAssistedButton` in `MachinePrefs.psp` (strings), absent from the descriptor; Window menu item stays listed |
| [25] | Interface | Dynamic Color Sliders | on → on | `interface.dynamicColorSliders` | `get_preferences` |
| [26] | Interface | Show Simplified Right Click And Flyout Menus | off → off | AX | AX |
| [27] | Workspace | Auto-Collapse Iconic Panels | on → on | AX | descriptor `autoCollapseDrawers` |
| [28] | Workspace | Auto-Show Hidden Panels | off → off | AX | `autoShowRevealStrips` |
| [29] | Workspace | Open Documents as Tabs | on → on | AX | `openNewDocsAsTabs` |
| [30] | Workspace | Enable Floating Document Window Docking | on → on | AX | descriptor |
| [31] | Workspace | Large Tabs | off → off | AX | `enableLargeTabs` |
| [32] | Workspace | Enable Narrow Options Bar | on → on | AX | `enableNarrowOptionBar` |
| [33] | Workspace | Enable Native Full Screen | off → off | AX | descriptor |
| [34] | Notifications | Enable quiet mode | `quietMode` is absent from the descriptor and from every `.psp` (strings); the current value is the `from` field of the `set_preferences` `applied` row at [08] step 04 → on, written last in the class. Typing (`PreferencesNotifications.d.ts`): "When Quiet Mode is enabled, certain notification preferences become read-only and cannot be modified until Quiet Mode is disabled"; `showFeatureOnboarding`, `showWhatsNew`, `useRichToolTips` each say "This preference will be locked when Quiet Mode is enabled"; getters carry no such note. Adobe (2025-07-29): "reduce in-app pop-ups and non-essential notifications" | `notifications.quietMode` | `get_preferences` over the class after the write ([09] row 16: a getter is not documented to throw; on `preference-locked` the descriptor keys `preferences/notificationsPreferences/useRichToolTips` and `useRichToolTipsRestore` plus the Notifications pane checkboxes (AX) are the readback); a second `showWhatsNew` write after the quiet write returns `preference-locked` |
| [35] | Notifications | Tooltips | on → on | `notifications.showToolTips` (also `tools.showToolTips`) | `get_preferences` |
| [36] | Notifications | Rich Tooltips | on (`useRichToolTips true`, and `useRichToolTipsRestore true` in the descriptor: the app keeps a restore copy for the value Quiet Mode rewrites) → on, written before quiet mode; the value the app holds while Quiet Mode is on is [09] row 15 and Quiet Mode wins | `notifications.useRichToolTips` | `get_preferences` before the quiet write; descriptor `useRichToolTips` and `useRichToolTipsRestore` after it |
| [37] | Notifications | What's new | `showWhatsNew` absent from the descriptor and every `.psp`; current = the `from` field of the `applied` row → off, written before the quiet mode write | `notifications.showWhatsNew` | `get_preferences` |
| [38] | Notifications | Feature Onboarding | `showFeatureOnboarding` absent from the descriptor and every `.psp`; current = the `from` field of the `applied` row → off, written before the quiet mode write | `notifications.showFeatureOnboarding` | `get_preferences` |
| [39] | Tools | Enable Gestures | on → on | AX | descriptor |
| [40] | Tools | Use Shift Key for Tool Switch | on → on | `tools.useShiftKeyForToolSwitch` | `get_preferences` |
| [41] | Tools | Overscroll | on → on | AX | descriptor |
| [42] | Tools | Enable Flick Panning | on → on | AX | descriptor `flick` |
| [43] | Tools | Double Click Layer Mask Launches Select and Mask | on → on | AX | descriptor |
| [44] | Tools | Vary Round Brush Hardness based on HUD vertical movement | on → on | AX | descriptor |
| [45] | Tools | Disable brush opacity based on HUD vertical movement | off → off | AX | descriptor |
| [46] | Tools | Arrow Keys Rotate Brush Tip | on → on | AX | descriptor |
| [47] | Tools | Use Paintbrush Tip to Erase While Using a Stylus Eraser | off → off | AX | descriptor |
| [48] | Tools | Snap Vector Tools and Transforms to Pixel Grid | on → on | AX | descriptor |
| [49] | Tools | Show Reference Point when using Transform | on → on | AX | descriptor |
| [50] | Tools | Spring-loaded Tool Shortcuts | on, 200 ms → on, 200 ms | AX | descriptor |
| [51] | Tools | Zoom with Scroll Wheel | off → off | AX | descriptor |
| [52] | Tools | Animated Zoom | on → on | AX | descriptor `animationKey` |
| [53] | Tools | Zoom Resizes Windows | off → off | `tools.keyboardZoomResizesWindows` | `get_preferences` |
| [54] | Tools | Zoom Clicked Point to Center | off → off | AX | AX |
| [55] | Tools | Show HUD | Top Right → Top Right | AX popup | descriptor |
| [56] | History | History Log | off → off | `history.useHistoryLog` | `get_preferences` |
| [57] | History | History states | 50 → 50 | `history.numberOfHistoryStates` | `get_preferences` |
| [58] | History & Content Credentials | Content Credentials document options | None → None; Ask when opening on (`contentCredentialsDocumentAsk true`) → off; export None | AX (`Ask when opening` checkbox) | descriptor `contentCredentialsDocumentAsk` |
| [59] | File Handling | Image Previews | Always Save, Thumbnail on → same | `fileHandling.imagePreviews` | `get_preferences` |
| [60] | File Handling | Append Extension | Always, Use Lower Case → same | `fileHandling.useLowerCaseExtension` | `get_preferences` |
| [61] | File Handling | Default File Location | On your computer (`defaultCloudSave false`) → same | AX popup | descriptor |
| [62] | File Handling | Save As to Original Folder | on → on | AX | descriptor |
| [63] | File Handling | Save in Background | on → on | AX | descriptor |
| [64] | File Handling | Automatically Save Recovery Information | on, 5 min → same | AX | descriptor |
| [65] | File Handling | Enable legacy "Save As" | off → off | AX | descriptor |
| [66] | File Handling | Do not append "copy" | off → off | AX | descriptor |
| [67] | File Handling | Prefer Adobe Camera Raw for Supported Raw Files | on → on | AX | descriptor `cameraRaw` |
| [68] | File Handling | Use ACR to Convert 32→16/8 | off → off | AX | descriptor |
| [69] | File Handling | Ignore EXIF Profile Tag, Ignore Rotation Metadata | off, off → same | AX | descriptor |
| [70] | File Handling | Ask Before Saving Layered TIFF | on → on | `fileHandling.askBeforeSavingLayeredTIFF` | `get_preferences` |
| [71] | File Handling | Disable Compression of PSD and PSB | off → off | AX | descriptor |
| [72] | File Handling | Maximize PSD and PSB Compatibility | Always → Always | `fileHandling.maximizeCompatibility` | `get_preferences` |
| [73] | File Handling | Recent File List Contains | 20 → 20 | `fileHandling.recentFileListMaximum` (ES returns undefined) | `get_preferences` |
| [74] | Export | Quick Export Format | PNG, Transparency on, Smaller File off → same | AX | descriptor |
| [75] | Export | Quick Export Location | current `exportAsLocationSetting 2` → Ask where to export each time | AX radio | descriptor |
| [76] | Export | Export As metadata / Convert to sRGB | None, on → same | AX | descriptor |
| [77] | Export | Export Assets location | `exportAssetsLocationSetting 3` → last location specified | AX radio | descriptor |
| [78] | Performance | Let Photoshop Use | 70 % → 70 % | `performance.maxRAMuse` | `get_preferences` |
| [79] | Performance | Cache Levels / Tile Size | 4 / 1024K → same | `performance.imageCacheLevels`; tile AX | `get_preferences` |
| [80] | Performance | Use Graphics Processor | on → on | AX | `app.systemInformation` `useGPU 1` |
| [81] | Performance | Advanced: OpenCL, GPU compositing, anti-alias guides, 30-bit display | on, on, on, on → same | AX | descriptor `openglAdvanced` |
| [82] | Performance | Multithreaded compositing, Foreground composite caching, Multithreaded PSD reading | on, on, on → same | AX | descriptor |
| [83] | Image Processing | Select Subject and Remove Background | Device → Device | AX popup | descriptor `imageProcessingSelectSubjectPrefs` |
| [84] | Image Processing | Selections, Remove, Enhance processing | Faster → Faster | AX | descriptor |
| [85] | Scratch Disks | Startup only | on → on | AX | `app.systemInformation` |
| [86] | Cursors | Painting / Other | Normal Brush Tip / Precise → same | `cursors.paintingCursors`, `cursors.otherCursors` | `get_preferences` |
| [87] | Cursors | Crosshair in tip on; only crosshair while painting off; no cursor while painting off; brush leash off | same → same | AX | descriptor |
| [88] | Transparency & Gamut | Grid Size / Colors / Gamut warning | Medium / Light / gray 100 % → same | `transparencyAndGamut.gridSize`, `.gamutWarningOpacity` | `get_preferences` |
| [89] | Units & Rulers | Rulers / Type | Pixels / Pixels → same | `unitsAndRulers.rulerUnits`, `.typeUnits` | `get_preferences` |
| [90] | Units & Rulers | Column 180 px, Gutter 12 px | same → same | AX | descriptor |
| [91] | Units & Rulers | New Document Preset Resolutions | Print 300, Screen 72 → same | AX | descriptor (`21600`, `5184` in 1/72) |
| [92] | Units & Rulers | Point/Pica Size | PostScript → PostScript | `unitsAndRulers.pointSize` | `get_preferences` |
| [93] | Guides, Grid & Slices | Guides | Cyan, solid (`lens`), 1 px → same | `guidesGridsAndSlices.guideStyle`; color AX | `get_preferences` |
| [94] | Guides, Grid & Slices | Artboard guides | Light Blue active solid, inactive dashed, inactive shown off → same | AX | descriptor |
| [95] | Guides, Grid & Slices | Smart Guides | Magenta → Magenta | AX | descriptor |
| [96] | Guides, Grid & Slices | Grid | Custom gray, solid, 64 px, 4 subdivisions → Custom gray, solid, gridline every 45 px, 3 subdivisions (the master digital module and its 15 px third; the per-family print modules travel as guides inside each `.psdt`, [06.1]) | `guidesGridsAndSlices.gridStyle`, `.gridSubDivisions`; gridline size and unit AX (`Gridline Every` field, unit popup `Pixels`) | `get_preferences`; descriptor grid size |
| [97] | Guides, Grid & Slices | Slices | Light Blue, numbers on → same | `guidesGridsAndSlices.showSliceNumber` | `get_preferences` |
| [98] | Plugins | Show all Filter Gallery groups and names | off → off | AX | descriptor `pluginPicker` |
| [99] | Plugins | Enable Developer Mode | off → off | AX | descriptor |
| [100] | Plugins | Enable Generator | off → off | AX | descriptor `generatorEnabled` |
| [101] | Plugins | Enable Remote Connections | off → off | AX | AX |
| [102] | Plugins | Allow Extensions to Connect to the Internet | on → off | AX | AX |
| [103] | Plugins | Load Extension Panels | on (`extensionsOn true`) → off | AX (restart) | descriptor; `app.systemInformation` no `CC Libraries Panel` row |
| [104] | Type | Use Smart Quotes | on → on | `type.smartQuotes` | `get_preferences` |
| [105] | Type | Enable Missing Glyph Protection | on → on | AX | descriptor `enableFontFallback` |
| [106] | Type | Show Font Names in English | on → on | `type.showEnglishFontNames` | `get_preferences` |
| [107] | Type | Use ESC key to commit text | on → on | AX | descriptor |
| [108] | Type | Set default font size automatically | off → off | AX | descriptor |
| [109] | Type | Enable Type layer glyph alternates | on → on | AX | descriptor |
| [110] | Type | Automatic detection of lists | off → off | AX | descriptor |
| [111] | Type | Fill new type layers with placeholder text | off → off | AX | descriptor |
| [112] | Type | Language Options (Type menu) | Default Features (menu `✓` in `menus`; descriptor `typePreferences/textComposerChoice defaultTextInterface`) → Middle Eastern Features | `type.showTextFeatures = 'middleEasternInterface'` | `get_preferences`; Type > Language Options > Middle Eastern Features `✓` |
| [113] | Type | Font Preview Size (Type menu: None, Small, Medium, Large, Extra Large, Huge) | Large (menu `✓`; `preferences.json` `fontPreviewSize FontPreviewType.LARGE`) → Small | ES `app.preferences.fontPreviewSize` | ES readback; Type > Font Preview Size > Small `✓` |
| [114] | Type | Recent fonts | 10 → 10 | none | descriptor |
| [115] | Enhanced Controls | Show Touch Bar property adjustments | on → off | AX | descriptor |
| [116] | Technology Previews | Preserve Details 2.0 Upscale | on → on | AX | descriptor `expFeatureDeepUpscale` |
| [117] | Technology Previews | Content-Aware Tracing Tool | on → on | AX | descriptor |
| [118] | Technology Previews | Precise color management for HDR display | on → on | AX | descriptor |
| [119] | Technology Previews | Precise previews for 16-bit documents | on → on | AX | descriptor |
| [120] | Technology Previews | Open JPEG without Background layer | on → on | AX | AX |
| [121] | Technology Previews | Enable Modern User Interface | on → on | AX | descriptor `DroverUI true` |
| [122] | Early Access | none listed | empty → empty | none | descriptor `earlyAccessPrefs` empty |
| [123] | Product Improvement | Yes, I'd like to participate | off → off | AX | AX |
| [124] | Camera Raw | see [06] rows 20–27 | | | |
| [125] | Edit > Color Settings | `Default Color Settings` ([06.2] row 01 holds every field) | keep | none (already applied) | descriptor `colorSettings` equals [06.2] row 01; `app.colorSettings` = `Default Color Settings` |
| [126] | Edit > Keyboard Shortcuts | `Photoshop Defaults`; Legacy Undo off; Legacy Channel off | the house scheme of [06.6], saved as `Default Keyboard Shortcuts`; the two Legacy boxes stay off | AX (dialog list and Shortcut column), `Save Set` | dialog popup `Default Keyboard Shortcuts`; `Summarize` writes the HTML list, its Tools rows equal [06.6] |
| [127] | Edit > Menus | factory visibility | hide the AI, promo, learn rows listed in [06] row 30. Adobe (2023-05-24): Visibility button per row, `Save Set` (saving over Photoshop Defaults opens the Save dialog for a set name), hidden rows return through `Show All Menu Items` or ⌘-click, "To permanently reveal all menu items, choose Window > Workspace > Essentials" (menus are workspace state, captured by the Menus box); the page says nothing about shortcuts, and no hidden row is a command this pass uses by shortcut | AX (dialog list) or CU | dialog re-read; each hidden row's Shortcut column in the Keyboard Shortcuts tab recorded; System Events menu read shows the rows absent |
| [128] | View > Rulers | off → on | with the template open | AX | checkmark |
| [129] | View > Snap | on → on | none | checkmark |
| [130] | View > Show Extras Options | all → all | none | |
| [131] | Home screen | `homeScreenVisibility false`, `autoShowHomeScreen false` → same | none | descriptor |
| [132] | Discover panel, Learn | no preference; Help > Photoshop Help… and Hands-on Tutorials… hidden through Edit > Menus | AX | menu read |
| [133] | Adobe Fonts auto-activation | no Photoshop preference (Type pane holds eight controls, none for activation); owner is the Creative Cloud app Fonts setting, decided in the Creative Cloud pass | none | Type pane AX dump |

## [06]-[DEFAULTS_REPLACEMENT]

| [INDEX] | [ITEM] | [CURRENT → TARGET] | [MECHANICS] | [PROOF] |
| :-----: | :----- | :----------------- | :---------- | :------ |
| [01] | Where panel presets live | `Brushes.psp` 1,236,268 B (28 factory), `Swatches.psp` 10,809 B (122), `Gradients.psp` 139,245 B (183), `Styles.psp` 10,812,911 B (21), `Patterns.psp` 8,594,532 B (10), `CustomShapes.psp` 2,312,463 B (54); no `ToolPresets.psp`, no `Contours.psp` | Stores rewritten on quit; `<photoshop.installFolder>/Presets/` is `root:admin` load-on-demand content, never edited | sizes after quit |
| [02] | Delete factory groups, Brushes | General Brushes, Dry Media Brushes, Wet Media Brushes, Special Effects Brushes → none | Brushes panel: select the group, panel trash button (`brushesDelete`), confirm; repeat per group; never `Restore Default Brushes`. Adobe brush page (2024-11-11) documents only "select a brush, and select the Delete brush icon from the panel menu"; `Delete Group` is a string of the binary (6 hits), its context-menu placement is [09] row 07 | `list_presets brush` count 0 before import |
| [03] | Delete factory groups, Swatches | RGB, CMYK, Grayscale, Pastel, Light, Pure, Dark, Darker, Pale → none | Swatches panel: select group, trash (`swatchesDelete`); never `Reset Swatches` | `list_presets swatch` |
| [04] | Delete factory groups, Gradients | Basics, Blues, Purples, Pinks, Reds, Oranges, Greens, Grays, Cloud, Iridescent, Pastels, Neutrals → none | same pattern (`gradientsDelete`) | `list_presets gradient` |
| [05] | Delete factory groups, Patterns | Trees, Grass, Water → none | Patterns panel, select group, trash; Adobe (2023-05-24) names `Delete Pattern` for items and `Restore Default Patterns` (never run) | `list_presets pattern` |
| [06] | Delete factory groups, Styles | Basics, Natural, Fur, Fabric → none | same (`stylesDelete`) | `list_presets style` |
| [07] | Delete factory groups, Shapes | Wild Animals, Leaf Trees, Boats, Flowers → none | same (`customShapesDelete`) | `batch_play` `get` of `presetManager` (the [06.2] row 08 descriptor of `architecture.md`; `list_presets` has no shapes kind), custom shapes group count 0 |
| [08] | Tool presets | 21 factory in memory (`<Inputs>/photoshop/preset-manager.json`, the eighth group: five Crop, two Type, eight shape (two Rectangle, one Ellipse, two Polygon, one Line, two Custom Shape), `Healing Brush 21 pixels`, `Magnetic Lasso 24 pixels`, `Fill with Bubbles Pattern` (Paint Bucket), `Peanut Dash` (Pen), `Circular Rainbow` (Gradient), `Background Eraser 30 pixels`) → the 23 presets of [06.4] | Preset Manager (`Edit > Presets > Preset Manager...`, type Tools) select all, `Delete`; each [06.4] preset is created from the Tool Presets panel `Create New Tool Preset` with its tool active and its Options bar set; then `Save Set...` → `Default Tool Presets.tpl` (the app's `Presets/Tools/` ships `Crop and Marquee.tpl`, `Text.tpl`); Adobe (Use presets, 2023-05-24): "The Preset Manager lets you save or load your presets for contours and tools", `Delete`, `Save Set...` | `list_presets tool` returns the 23 names of [06.4] |
| [09] | Contours | 12 factory → 12 factory | Kept; no user contour exists and layer styles reference them | none |
| [10] | Import ours | Drive `Default Brushes.abr` (`cloud-libraries.md` [03.5], [08.1] row 01), `Default Swatches.aco` (the unified colour set, `cloud-libraries.md` [08.1] row 05; this file defines no colour), `Default Gradients.grd` ([08.1] row 03), `Default Patterns.pat` ([08.1] row 02), `Default Shapes.csh`; no `Default Styles.asl`: `Styles.psp` keeps no factory group and no resident user group, and `Super Riso 300.asl`, `Mr Toner 300.asl`, JHN, Glassmorph load per project through `Import Styles...` (`cloud-libraries.md` [05.6]) | Panel flyout `Import Brushes...` (`brushesImport`; Adobe 2024-11-11: "use the Import Brushes option in the Brushes panel flyout menu to locate the downloaded ABR file"), `Import Swatches...` (`swatchesImport`), `Import Gradients...`, `Import Patterns...`, `Import Styles...`, `Import Shapes...` (SDK `PIStringTerminology.h`; `Import Brushes` and `Export Selected Brushes` are strings of the binary); an imported `.abr` creates a group named after the file (Adobe community 2018-01-09) | group names in `list_presets` |
| [11] | Resident ceiling | `Brushes.psp` ≤ 150,000,000 B, every `.psp` summed ≤ 300,000,000 B (the measured failure points are `photoshop-brushes-and-skills.md` [1.2]; the resident set and its overflow rule are `cloud-libraries.md` [03.5]) | Residents: the five subgroups of `cloud-libraries.md` [03.5]; the six group packs stay as `.abr` on Drive ([03.6]), imported per project through `Import Brushes...` and deleted after | `stat` of every `.psp` after quit; sum < 300 MB |
| [12] | On-demand libraries | Drive `…/Photoshop/Libraries/<Kind>/` | Never copied into `Presets/`; imported from Drive by the flyout | file list |
| [13] | Export ours | Brushes: right-click group → `Export Selected Brushes` (Adobe 2026-02-23); Swatches `Export Selected Swatches`, Gradients, Patterns, Styles, Shapes `Export Selected …` (SDK `*Export`) → Drive `Default *.<ext>` | hashes |
| [14] | Default Type Styles | `Default Type Styles.psp` 39,367 B is Adobe's seed: an `8BPS` PSD with XMP `CreatorTool Adobe Photoshop CS6 (13.1 x001)`, XMP create date 2012-08-13, holding the paragraph style `Basic Paragraph` and the fonts `MyriadPro-Regular`, `MyriadPro-Bold`, `TimesNewRomanPSMT` (UTF-16 string scan of the file); no user style → Body, Heading, Caption from typography | Open the template, define Paragraph and Character styles, `Type > Save Default Type Styles` (live menu item; enabled with a document) | new document shows the three styles; `Type > Load Default Type Styles` into a scratch document lists them |
| [15] | New Document presets | `<photoshop.prefsFolder>/New Doc Sizes.json` is `{"sections":[{"section":"user","presets":[]}]}` (104 B) → one preset per catalogue row ([06.1]) | New Document dialog (⌘N): Preset Details fields per [06.1] row 02, save icon, the row's name, `Save Preset` (Adobe, Create documents: Width, Height, Orientation, Artboards, Color Mode, Resolution, Background Contents, Advanced Options Color Profile and Pixel Aspect Ratio; "You can later access the new preset from the Saved tab"); the dialog is UXP, so CU; the preset object schema is [09] row 20 | `jq '.sections[] \| select(.section == "user") \| .presets \| length'` equals the catalogue row count, every entry carries the row's fields |
| [16] | Templates `.psdt` | none → `Sizes/<name>.psdt` per catalogue row and `Default Template.psdt` = the `Digital 3840x2160` file | [06.1] rows 03–05: built by the `build_template` `batch_play` body (`architecture.md` [06.2]), saved as `.psd`, renamed `.psdt` (saving with `.psdt` typed yields `.psdt.psd`, community 2021-03-02, so the rename is the route); copies to Drive `…/Photoshop/Sizes/` and `~/Documents/Adobe/Photoshop/Sizes/`, the master also as `~/Documents/Adobe/Photoshop/Default Template.psdt` | `open` of each → `app.activeDocument.name` `Untitled-1`, `saved false`, the row's pixel size, resolution, `Adobe RGB (1998)`, 16 bit, guide count per [06.1] row 04 |
| [17] | Layer style defaults | factory (no `Effect Prefs.psp` in the settings folder) → factory; Global Light 90° / 30° | none | folder listing |
| [18] | Smart filter defaults | `placeRasterSmartObject true`; Filter > Convert for Smart Filters on demand | keep | descriptor |
| [19] | Filter Gallery, Neural Filters | Filter Gallery groups collapsed (`showAllFilterGalleryEntries false`); Neural Filters plugin `Registered`, never opened, no model downloaded → same; `Filter > Neural Filters...` hidden through Edit > Menus | AX | menu read |
| [20] | Camera Raw settings store | Camera Raw Database (`DNGSidecarHandling 0`, `Settings/` holds two index files) → Sidecar ".XMP" Files | Camera Raw Preferences dialog (`Photoshop (Beta) > Settings… > Camera Raw...`), `Save Image Settings In` menu: `Camera Raw Database` (stored under `~/Library/Application Support/Adobe/CameraRaw/Settings`) or `Sidecar ".XMP" Files` (Adobe, Manage Camera Raw settings, 2026-06-09) | `Defaults/Preferences.xmp` re-read |
| [21] | Camera Raw DNG | Ignore sidecar off → off | same dialog, `Ignore Sidecar ".XMP" Files` in the DNG File Handling section (same page) | same |
| [22] | Camera Raw JPEG, TIFF, HEIC, AVIF, JXL handling | Open if has settings → same | same | same |
| [23] | Camera Raw cache | 5.0 GB at default path, empty → 20 GB, default path | Performance pane | `NegativeCacheMaximumSize 20.0` |
| [24] | Camera Raw GPU | Auto, self-test passed → Auto | none | `Camera Raw GPU Config.txt` |
| [25] | Camera Raw Generative AI | on (default) → off | Camera Raw Preferences, the generative-AI control; the Manage Camera Raw settings page (2026-06-09) has no generative section, so the control's pane and label are [09] row 14 | screenshot of the dialog |
| [26] | Camera Raw workflow | Display P3 (`ClrS DiP3`), 16 bit (`BtDp 16`), Smart Object off (`SmPI 0`) → Adobe RGB (1998), 16 bit, native size, 300 ppi, Sharpen For none, `Open In Photoshop As Smart Objects` on | "clicking the underlined text at the bottom of the Camera Raw dialog box": Space ("set Space to the color profile you use for your Photoshop RGB working space"), Depth, Size, Resolution, Sharpen For, Open In Photoshop As Smart Objects (Adobe 2026-06-09); the page names no workflow preset; the dialog's preset control is [09] row 17 | `Adobe Camera Raw Prefs` decoded `ClrS`, `BtDp 16`, `SmPI 1` (the file on disk is `~/Library/Preferences/Adobe Camera Raw Prefs`, 4,340 B; Adobe's table names it `Adobe Camera Raw [version] Prefs`) |
| [27] | Camera Raw raw defaults | Adobe Default, master only → same | none | `RawDefaults.xmp` |
| [28] | TK9 Multi-Mask preferences | `language English`, `autoSetGraySpace true` (Monitor Gray Working Space), `autoShowProperties false`, `autoHideSelection false`, `showSelectionIndicator true`, `FXOverlayColor #ff00ff` → same | fly-out menu (TK9 manual pp. 46–48) | `PluginsStorage/PHSPBETA/27/External/com.tk.multimask/PluginData/*.ini` |
| [29] | TK9 Combo preferences | `autoCloseTKActions true`, `colorOpacityValue 1`, `overlayColor #ff00ff`, indicators on, watermark defaults, `webSharpenFileType jpg10` → same; web-sharpen save folder `~/Pictures/TK9 Web Sharpen` | fly-out and web-sharpen interface | `com.tk.comboV8/PluginData/*.ini` |
| [30] | Edit > Menus hidden rows | none hidden → hidden (every row is an item of `<Inputs>/ax/photoshop.json` `menus`, cited by path in [11]): File > Invite to Edit..., Share for Review, Export > Send to Firefly Boards, Search Adobe Stock..., Search Adobe Express Templates..., Version History; Edit > Prompt to Edit..., Generative Fill..., Generate Image..., Reflection Removal..., Sky Replacement...; Image > Generative Upscale...; Layer > Harmonize, Layer Mask > Enhance edge; Type > More from Adobe Fonts...; Filter > Neural Filters..., Parametric Filters..., AI Denoise..., AI Sharpen...; Window > AI Assistant, AI Assisted Editor, Adobe Stock, Beta Feedback, Comments, Libraries, Version History, Materials; Plugins > Browse Plugins...; Help > Photoshop Help..., Hands-on Tutorials..., What's New..., Learn more about generative credits, Adobe generative AI user guidelines. Kept: Select > Subject, Select > Sky ([11] row 06), Plugins > Manage Plugins..., Help > System Info..., GPU Compatibility..., Manage My Account..., Updates... | Keyboard Shortcuts and Menus dialog, Menus tab, Visibility buttons; `Save Set` → `Default Menus` (`Presets/Menu Customization/` exists, empty) | dialog re-read; `Menu Customization.psp` and `Menu Customization Primary.psp` present (Adobe 2024-03-01 names both) |
| [31] | TK9 Export preferences | `radioOutputLocation SameFolder`, source `CurrentImage`, crop `Centered`, bars `ColorBars`, logo `CenterCenter`, offset `PX`, `saveFileType jpg10`, `loadLastPreset false` → output `Folder…` = `~/Pictures/TK9 Export`, `loadLastPreset true`, rest same | module Save section (manual section 10) | `com.tk.export/PluginData/radioOutputLocation.ini` |
| [32] | TK9 backups | none → one folder per module under Drive `…/Photoshop/TK9 Backups/<module>/` | each module's fly-out `Backup user data` (manual "Back Up and Restore": one folder per module, folder contents replaced) | folder listing |
| [33] | Actions panel | 7 sets, 62 actions (`<Inputs>/photoshop/preferences.json` `actionSets`: `TK9 actions` 26, `Basic Adjustments` 5, `Subject & Background` 5, `Creative Effects` 6, `Guides` 8, `Resize` 6, `Export` 6) → one set `Default Actions` holding the 30 members of [06.5]; no action bound to a script, no launcher | [06.5]: `New Set...` `Default Actions`; `Load Actions...` of `01.Overlay Actions.atn`, drag `Place Overlay` in; drag the 26 TK9 actions in; record the three house actions; delete the six Adobe sets, the emptied `TK9 actions` set, and the emptied overlay set; `Save Actions...` to `Presets/Actions/Default Actions.atn`; ⌘⌥ `Save Actions...` writes the step listing | `executeActionGet` on `ASet` count 1, name `Default Actions`, `Actn` count 30; the step listing equals [06.5] |
| [34] | Adjustments panel presets | factory `Adjustments_Presets/presets.json` → factory | No panel deletion for shipped groups exists; left as is | none |
| [35] | Creative Cloud Libraries | Libraries panel open in the Properties group, CEP panel loaded → panel closed, `Load Extension Panels` off, no preset or brush travels through a CC Library; every import is a Drive copy through the panel flyouts of row 10 | Creative Cloud Libraries are excluded from every library (`cloud-libraries.md` [08.1]) | `app.systemInformation` has no `CC Libraries Panel` row; `.psw` `ccLibrariesPanel closed=true` |

### [06.1]-[SIZE_CATALOGUE]

One New Document preset and one `.psdt` per row of the InDesign catalogue: the print families, the technical sheets, the boards, `Digital 3840x2160`, and every named screen format the catalogue lists (`gui-indesign.md` [07], the catalogue paragraph and the size tables own the row list, every dimension, and every module value). The row source is `apps/creative-cloud/indesign/page-sizes.json` (`gui-indesign.md` [07] row 07: the `@rasm/indesign` project holds the file, the typography data builder writes its rows from the size tables); the Photoshop `build_template` body reads the same file, so the two catalogues cannot drift.

| [INDEX] | [FIELD] | [VALUE] | [PROOF] |
| :-----: | :------ | :------ | :------ |
| [01] | Names and count | the catalogue's names, one preset and one file each; `Default Template.psdt` is a second copy of `Digital 3840x2160.psdt` | the `user` section's `presets` length in `New Doc Sizes.json` ([06] row 15 `jq`) equals the row count of `page-sizes.json` plus one (row 07); `ls Sizes/` count equals the row count |
| [02] | Preset Details per row | Width and Height in the row's unit (`in` on US, ANSI, ARCH, and board rows, `mm` on ISO rows, `px` on digital and screen rows); Orientation as the row; Artboards off; Color Mode `RGB Color`, `16 bit`; Resolution `300 Pixels/Inch` on print, technical, and board rows, `72` on `Digital 3840x2160` and every screen row (the two values of [05] row 91); Background Contents `White`; Advanced Options: Color Profile `Adobe RGB (1998)`, Pixel Aspect Ratio `Square Pixels`; preset name = row name | each `user` preset of `New Doc Sizes.json` carries `name`, `width`, `height`, `units` (`inchesUnit`, `millimetersUnit`, `pixelsUnit`), `resolution 300` or `72` with `resolutionUnits inchesUnit`, `mode "RGB"`, `depth 16`, `fill "white"`, `profile "Adobe RGB (1998)"`, `scale 1`, `guides []`, `artboards []` (the schema of [09] row 20; orientation is width against height) |
| [03] | `.psdt` content | one `Background` layer, white; guides per row 04; the three default type styles of row 14; no artboard; saved through `save` as `.psd` with `maximizeCompatibility` (the app preference of [05] row 72), renamed `.psdt` | `open` → `Untitled-1`, `saved false`, `layers.length 1`, width, height, `resolution`, `colorProfileName Adobe RGB (1998)`, `bitsPerChannel 16` |
| [04] | Guides carry the module | per row, from its `page-sizes.json` record (`margins`, `columns`, `rows`, `firstBaseline`, `lastBaseline`, `folio`, `footer`; the size tables of `gui-indesign.md` [07] own every value): the two side margins, the top margin, the column edges (`columns.count` columns and their `1 L` gutters: 12 on document rows, 2 to 6 on screen rows), the row tops and row baselines, the last baseline, the folio or footer line; units pt on print rows converted at 300 ppi (`px = pt × 300 / 72`, fractional positions kept: a guide `position` takes `_unit "pointsUnit"`), px on digital and screen rows; technical sheets carry their sheet border (24 pt on inch sheets, ISO 5457 on ISO sheets) and the `108 pt` module lines and no column; digital and screen rows carry the 45 px module (sides `m` L per the screen rule, 135 on the master, gutters 45) | a `batch_play` `get` of `{_ref: "document"}` with `_property "guides"` lists the positions and their count equals the row's line count (`get_document` carries no guide field, `architecture.md` [06.2] row 05) |
| [05] | Build | `build_template` `batch_play` body (the `.psdt` template body of `architecture.md` [06.2]), run once per row of `page-sizes.json`: per row `make` `document` `{width, height, resolution, mode "RGBColorMode", depth 16, fill "white", profile "Adobe RGB (1998)", pixelScaleFactor 1}`, `make` `guide` per line, `save` to `.artifacts/creative-cloud/photoshop/Sizes/<name>.psd`, then `mv` to `.psdt`; descriptors in one `batch_play` call per row, `continueOnError false` | `batch_play` `failed []`; per-row readback of row 03 |
| [06] | Copies | `~/Documents/Adobe/Photoshop/Sizes/<name>.psdt`, `~/Documents/Adobe/Photoshop/Default Template.psdt`; Drive `99.Default Profiles/Photoshop/Sizes/<name>.psdt` and `Default Template.psdt`; `Default New Doc Sizes.json` | `exports.sha256` rows |
| [07] | CMYK preset | one CMYK-mode New Document preset, `Print CMYK Letter 8.5x11`: 8.5 × 11 in, portrait, `CMYK Color`, `8 bit`, 300, White, Color Profile `GRACoL2013_CRPC6.icc` (the working CMYK of [06.2] row 01), Square Pixels; every other print row stays RGB and is proofed through [06.2] rows 02–03 | the `user` preset named `Print CMYK Letter 8.5x11` carries `mode "CMYK"`, `depth 8`, `profile "GRACoL2013_CRPC6.icc"` (the value strings of [09] row 20) |
| [08] | Screen rows | the named Web and Mobile formats of the catalogue, each at 72 ppi on the 45 px module with the guides its catalogue line states | as rows 02–04 |

### [06.2]-[COLOUR_AND_PROOF]

| [INDEX] | [ITEM] | [VALUE] | [CHANNEL] | [PROOF] |
| :-----: | :----- | :------ | :-------- | :------ |
| [01] | Edit > Color Settings `Default Color Settings` | Working Spaces: RGB `Adobe RGB (1998)`, CMYK `GRACoL2013_CRPC6.icc`, Gray `Gray Gamma 2.2`, Spot `Dot Gain 20%`; Color Management Policies: RGB, CMYK, Gray `Preserve Embedded Profiles`; Profile Mismatches `Ask When Opening` off, `Ask When Pasting` on; Missing Profiles `Ask When Opening` on; Conversion Options: Engine `Adobe (ACE)`, Intent `Relative Colorimetric`, `Use Black Point Compensation` on, `Use Dither` on, `Compensate for Scene-referred Profiles` off; Advanced Controls: `Desaturate Monitor Colors` off, `Blend RGB Colors Using Gamma` off, `Blend Text Colors Using Gamma` 1.45 → keep every field; the file `~/Library/Application Support/Adobe/Color/Settings/Default Color Settings.csf` is the set the other three apps load | none (applied); nothing rewrites the `.csf` | descriptor `colorSettings` (`workingRGB`, `workingCMYK`, `workingGray`, `workingSpot`, `policyRGB preserve`, `policyCMYK preserve`, `policyGray preserve`, `askMismatchOpening false`, `askMismatchPasting true`, `askMissing true`, `engine Adobe (ACE)`, `intent colorimetric`, `mapBlack true`, `dither true`, `renderSceneReferred false`, `monitorCompression false`, `RGBBlendGamma false`, `TextBlendGamma 1.45`); `exports.sha256` row of the `.csf` |
| [02] | View > Proof Setup > Custom… `Default Proof` | opened with no document open (Adobe, Proofing colors: "if you want the custom proof setup to be the default proof setup for documents, close all document windows before choosing the View > Proof Setup > Custom command"); Device to Simulate `GRACoL2013_CRPC6.icc`; `Preserve CMYK Numbers` off; Rendering Intent `Relative Colorimetric`; `Black Point Compensation` on; `Simulate Paper Color` off; `Simulate Black Ink` off; `Save` in the default location so the preset appears in the menu (Adobe: "To ensure that the new preset appears in the View > Proof Setup menu, save the preset in the default location") | AX on the dialog's popups and checkboxes; a window-id screenshot reads any control without an AX title | `ls ~/Library/Application\ Support/Adobe/Color/Proofing/` lists `Default Proof.psf`; System Events read of View > Proof Setup lists `Default Proof` |
| [03] | Proof channel per document | `View > Proof Colors` (⌘Y) on while preparing a print row, off on digital rows; `View > Gamut Warning` (⇧⌘Y) toggled while correcting, gamut colour gray 100 % ([05] row 88); the document title carries the proof name while Proof Colors is on (Adobe: "the name of the proof preset or profile appears at the top of the document window") | menu (AX) | `AxDump windowlist` title of the document window ends in the proof profile; View menu `AXMenuItemMarkChar` |
| [04] | Print conversion | `Edit > Convert to Profile`: Destination `GRACoL2013_CRPC6.icc`, Engine `Adobe (ACE)`, Intent `Relative Colorimetric`, `Use Black Point Compensation` on, `Use Dither` on, `Flatten Image` off; recorded as the house action of [06.5] row 30 | action | the action's step listing |
| [05] | Screen conversion | `Edit > Convert to Profile`: Destination `sRGB IEC61966-2.1`, the same engine, intent, BPC, dither, no flatten, after `Image > Mode > 8 Bits/Channel`; recorded as [06.5] row 29; Export As converts to sRGB on its own ([06.7] row 02) | action | step listing |
| [06] | Camera Raw workflow | Space `Adobe RGB (1998)`, Depth 16 bit ([06] row 26): raw files land in the working RGB | [06] row 26 | `Adobe Camera Raw Prefs` decode |

### [06.3]-[CHANNELS_AND_MASKS]

| [INDEX] | [FIELD] | [CURRENT → TARGET] | [CHANNEL] | [PROOF] |
| :-----: | :------ | :----------------- | :-------- | :------ |
| [01] | Channels and Paths panel thumbnails | Medium → Small ([04] row 07); `Show Channels in Color` off ([05] row 22) | CU flyout `Panel Options...`; bridge | screenshot of each panel's option dialog; `get_preferences interface` |
| [02] | Quick Mask Options (double-click the Quick Mask chip, Adobe, Create a temporary quick mask: `Masked Areas`, `Selected Areas`, colour box, opacity 0–100 %) | Color Indicates `Masked Areas`, colour red, opacity 50 % (Adobe's defaults) → `Masked Areas`, colour `#FF00FF`, opacity 50 % (the TK9 overlay colour of [06] rows 28–29, one mask overlay colour across the app) | CU dialog | reopen the dialog after relaunch, screenshot: colour box `FF00FF`, opacity 50 |
| [03] | Layer Mask Display Options (Channels panel, double-click a layer-mask channel) | red 50 % → `#FF00FF`, 50 % (the `\` overlay while a mask is targeted) | CU dialog on the template's scratch mask | screenshot of the reopened dialog |
| [04] | Alpha channel options (`Channel Options...` on a saved selection) | Color Indicates `Masked Areas`, colour `#FF00FF`, opacity 50 %; the dialog keeps the last values for the next new channel | CU | screenshot |
| [05] | Select and Mask workspace (`Select > Select and Mask...`, ⌥⌘R; `Double Click Layer Mask Launches Select and Mask` on, [05] row 43) | factory → View `Overlay`, Opacity 50 %, Color `#FF00FF`, Indicates `Masked Areas`; `Show Edge` off, `Show Original` off, `High Quality Preview` off; Refine Mode `Object Aware`; Edge Detection Radius 0 px, `Smart Radius` off; Global Refinements Smooth 0, Feather 0 px, Contrast 0 %, Shift Edge 0 %; Output Settings `Decontaminate Colors` off, Output To `Layer Mask`; `Remember Settings` on | CU in the workspace; the workspace remembers the values through `Remember Settings` | reopen the workspace after relaunch, screenshot of the Properties column |
| [06] | Fill and adjustment layer masks | `Use Default Masks on Fill Layers` on ([04] row 03); adjustment layers open with a mask (factory, the Adjustments panel behaviour) → keep | none | Layers panel screenshot after `Curves` from the Adjustments panel shows a mask thumbnail |
| [07] | Gray working space and TK9 | Color Settings Gray `Gray Gamma 2.2` ([06.2] row 01); TK9 Multi-Mask `autoSetGraySpace true` ([06] row 28) switches the gray space to `Monitor Gray` while it computes luminosity masks (TK9 manual pp. 46–48) and the Combo buttons honour the Select Subject processing choice ([11] row 06) → keep both | [06] rows 28–29 | `PluginData/*.ini`; descriptor `workingGray` unchanged after a TK9 run |
| [08] | On-device selection and removal | Select Subject and Remove Background `Device`, Selections, Remove, Enhance `Faster` ([05] rows 83–84); the Remove add-ons of [02] rows 32–33 | bridge, AX | descriptor `imageProcessingPrefs` |
| [09] | Mask painting presets | `Mask Soft 300`, `Mask Hard 60` of [06.4] rows 11–12 (Brush tool, black or white foreground, `X` swaps) | [06.4] | `list_presets tool` |
| [10] | Apply Image and Calculations | dialog defaults (factory, remembered per session) → untouched; channel maths runs through TK9 | none | none |

### [06.4]-[TOOL_PRESETS]

Every preset is created from the Tool Presets panel `Create New Tool Preset` with its tool active and its Options bar set (Adobe, Create tool presets, 2026-02-23), `Include Color` off; the panel view is [04] row 15. Brush-tool presets pair a resident brush with its category default: the brush is the first `keep` row of that resident subgroup in `decisions.tsv` order (`A/brushes/decisions.tsv`: skeleton written by lane 5b of `cloud-libraries.md` [09] row 02, filled by job L-P1, [03.2]; the resident subgroups are [03.5]), the size is the category default of `cloud-libraries.md` [03.4] (cited by row, never restated), and the Options bar state is Mode `Normal`, Opacity 100 %, Flow 100 %, Smoothing 0 %, pressure toggles off, Symmetry off, airbrush off; spacing, jitter, and scatter live in the brush preset ([03.4]). The set is saved as `Default Tool Presets.tpl` ([06] row 08, [07] row 06). 23 presets:

| [INDEX] | [PRESET] | [TOOL] | [BRUSH_OR_SETTINGS] |
| :-----: | :------- | :----- | :------------------ |
| [01] | `Brush People Elevation` | Brush | resident subgroup [03.5] row 02, size [03.4] row 01 |
| [02] | `Brush People Plan` | Brush | [03.5] row 02, size [03.4] row 02 |
| [03] | `Brush Annotation` | Brush | [03.5] row 03, size [03.4] row 06 |
| [04] | `Brush Trees Elevation` | Brush | [03.5] row 04, size [03.4] row 08 |
| [05] | `Brush Trees Plan` | Brush | [03.5] row 05, size [03.4] row 09 |
| [06] | `Brush Trees Plan Filled` | Brush | the first `keep` row of the `Plan View - Filled` source of [03.5] row 05, size [03.4] row 09 (Grass is not resident: [03.5] places it in the on-demand `Landscape.abr`) |
| [07] | `Brush Drawing Pencil` | Brush | the pencil preset of [03.5] row 01, vendor size and dynamics ([03.4] row 18) |
| [08] | `Brush Drawing Ink` | Brush | the ink preset of [03.5] row 01, vendor |
| [09] | `Brush Drawing Marker` | Brush | the marker preset of [03.5] row 01, vendor |
| [10] | `Brush Drawing Charcoal` | Brush | the broad charcoal preset of [03.5] row 01, vendor |
| [11] | `Mask Soft 300` | Brush | round tip defined in the preset: Size 300 px, Hardness 0 %, Spacing 25 %, Opacity 100 %, Flow 50 %, pen pressure on Opacity |
| [12] | `Mask Hard 60` | Brush | round tip: Size 60 px, Hardness 100 %, Spacing 25 %, Opacity 100 %, Flow 100 % |
| [13] | `Eraser Soft 300` | Eraser | round tip 300 px, Hardness 0 %, Mode Brush, Opacity 100 %, Flow 100 %, `Erase to History` off |
| [14] | `Eraser Hard 60` | Eraser | round tip 60 px, Hardness 100 %, Mode Brush |
| [15] | `Clone Aligned 300` | Clone Stamp | round tip 300 px Hardness 0 %, Mode Normal, Opacity 100 %, Flow 100 %, `Aligned` on, Sample `Current & Below`, ignore adjustment layers on |
| [16] | `Heal Sample Below 60` | Healing Brush | round tip 60 px Hardness 100 %, Mode Normal, Source `Sampled`, `Aligned` off, Sample `Current & Below`, Diffusion 5 |
| [17] | `Spot Heal Content-Aware 60` | Spot Healing Brush | round tip 60 px, Mode Normal, Type `Content-Aware`, `Sample All Layers` on |
| [18] | `Gradient Foreground to Transparent` | Gradient | a two-stop gradient set in the Options bar Gradient Editor, foreground 100 % opacity to foreground 0 %, named `Foreground to Transparent` and stored inside the tool preset (every factory gradient group is deleted, [06] row 04, so no panel gradient is referenced), Linear, Mode Normal, Opacity 100 %, `Reverse` off, `Dither` on, `Transparency` on, Method `Perceptual` |
| [19] | `Crop Digital 3840x2160 72` | Crop | `W x H x Resolution`: 3840 px, 2160 px, 72 px/in; `Delete Cropped Pixels` off; `Content-Aware` off; Overlay Rule of Thirds |
| [20] | `Crop Letter 8.5x11 300` | Crop | 8.5 in, 11 in, 300 px/in; same toggles |
| [21] | `Crop A4 210x297 300` | Crop | 210 mm, 297 mm, 300 px/in |
| [22] | `Crop A3 297x420 300` | Crop | 297 mm, 420 mm, 300 px/in |
| [23] | `Marquee 16:9` | Rectangular Marquee | Style `Fixed Ratio` 16 : 9, Feather 0 px, `Anti-alias` on |

### [06.5]-[ACTIONS]

One set, `Default Actions`, 30 actions, no function key, no colour, every step's modal-control box off, no `Stop`. Members and their recorded steps; the ⌘⌥ `Save Actions...` text export (Adobe, Play and manage actions, 2024-04-23: "Press Command+Option when you choose the Save Actions command to save the actions in a text file") is written to `.artifacts/creative-cloud/passes/photoshop/default-actions.txt` and is the proof that every member's steps equal this table:

| [INDEX] | [ACTION] | [ORIGIN] | [RECORDED_STEPS] |
| :-----: | :------- | :------- | :--------------- |
| [01] | `Place Overlay` | `01.Overlay Actions.atn` (Drive `00.Golden Hour Overlays/`, `cloud-libraries.md` [02.2] row 07), loaded with `Load Actions...`, the action dragged into `Default Actions`, its source set deleted | the steps the `.atn` records, unedited; the text export lists them |
| [02]–[27] | the 26 TK9 actions: `B and C Landscape`, `B and C Subject`, `B and C General`, `Zone Colour Grading`, `Tight Landscape`, `Tight Subject`, `Intersect Foreground`, `Intersect Sky`, `Intersect Subject`, `Intersect Background`, `Intersect Selection`, `Vignette No Darks`, `Mask The Mask`, `Modify Fill`, `Midtone Lighten`, `Midtone Contrast`, `Shadows Lighten`, `Colour Dodge Burn`, `Multiply Curve`, `Screen Curve`, `Lift Warm`, `Drop Cool`, `Drop Saturated`, `Paint Out Saturation`, `Lift Unsaturated`, `Close Panels` | the `TK9 actions` set of `<Inputs>/photoshop/preferences.json` `actionSets[0]`, dragged into `Default Actions` in that order; the emptied set deleted | the steps TK9 v4 recorded, unedited (TK9 manual, the actions chapter); the text export lists them |
| [28] | `Stamp Visible` | recorded | 1 `Select > All Layers`; 2 `Layer > Duplicate Layers`; 3 `Layer > Merge Layers`; 4 `Layer > Rename Layer...` → `Stamp` |
| [29] | `Convert to sRGB 8-bit` | recorded | 1 `Image > Duplicate...` name `Web`, `Duplicate Merged Layers Only` on; 2 `Image > Mode > 8 Bits/Channel`; 3 `Edit > Convert to Profile...` Destination `sRGB IEC61966-2.1`, Engine `Adobe (ACE)`, Intent `Relative Colorimetric`, `Use Black Point Compensation` on, `Use Dither` on, `Flatten Image` off |
| [30] | `Convert to GRACoL` | recorded | 1 `Image > Duplicate...` name `Print`, `Duplicate Merged Layers Only` on; 2 `Edit > Convert to Profile...` Destination `GRACoL2013_CRPC6.icc`, Engine `Adobe (ACE)`, Intent `Relative Colorimetric`, `Use Black Point Compensation` on, `Use Dither` on, `Flatten Image` off |

### [06.6]-[SHORTCUTS]

The cross-app owner is `gui-illustrator.md` [02.4] HOUSE_SHORTCUTS: one scheme for every app, a tool keeps the letter its shape family owns, the line family sits on `Q` and `⇧Q`, a tool held by more than one app carries one letter everywhere (the shared factory letter where the factory sets agree, Illustrator's where they differ), a tool one app alone holds keeps its factory key, and tool shortcuts take a single key or `⇧` plus a key. Photoshop's factory letters are the `toolTip` suffixes of `tools[]` in `<Inputs>/photoshop/application-descriptor.json` and the `[KEY]` column of [02]; the live list is the Tools category of `Edit > Keyboard Shortcuts` read at [08] step 08. Photoshop's rows under the owner's table, everything else factory:

| [INDEX] | [PHOTOSHOP_TOOL] | [FACTORY] | [HOUSE] | [OWNER_ROW] | [PROOF] |
| :-----: | :--------------- | :-------- | :------ | :---------- | :------ |
| [01] | `Toggle Standard/Quick Mask Modes` (Tools-list command) | `Q` | none; the Quick Mask chip stays the entry ([02] row 25) | [02.4] row 01 takes `Q` for the line family | Shortcut cell empty after `Delete Shortcut` |
| [02] | Line (shape slot, [02] row 19) | `U` shared with the shape slot | `Q` | [02.4] row 01 | Shortcut cell `Q`; `Q` selects Line, `⇧U` cycles the other shape tools |
| [03] | Lasso, Polygonal Lasso, Magnetic Lasso, Selection Brush ([02] row 03) | `L` | `⇧A` on the four rows | [02.4] row 04 (beside Direct Selection `A`) | Shortcut cells `⇧A`; Path Selection and Direct Selection keep `A`; [09] row 21 |
| [04] | Pencil ([02] row 09) | `B` shared with the paint slot | `N` | [02.4] row 14 (`N` Pencil in every app) | Shortcut cell `N`; `⇧B` cycles Brush, Color Replacement, Mixer Brush |
| [05] | Rectangle, Triangle, Polygon, Star, Custom Shape (shape slot, [02] row 19) | `U` | `M` | [02.4] row 15 (Rectangle `M` in every app) | Shortcut cells `M`; `⇧M` cycles the shape slot |
| [06] | Ellipse (shape slot, [02] row 19) | `U` | `L` | [02.4] row 16 (Ellipse `L` in every app; `L` freed by row 03) | Shortcut cell `L` |
| [07] | Rectangular Marquee, Elliptical Marquee ([02] row 02) | `M` | `U` (the letter the shape slot frees) | [02.4] row 15 | Shortcut cells `U`; `⇧U` cycles the two; Single Row and Single Column Marquee stay unassigned |
| [08] | Every other tool | its factory letter ([02] `[KEY]`) | unchanged: `V`, `A`, `P`, `T`, `B`, `E`, `G`, `I`, `H`, `Z`, `W`, `C`, `K`, `J`, `S`, `Y`, `O`, `R` and the Tools-list commands `D`, `X`, `F`, `/`, `[`, `]`, `{`, `}`, `,`, `.`, `<`, `>` | [02.4] row 14 | Summarize HTML Tools rows equal the `[KEY]` column with rows 01–07 applied |

Set saved as `Default Keyboard Shortcuts` ([05] row 126, [07] row 03); Legacy Undo and Legacy Channel boxes stay off. A conflict alert on any row is answered with `Accept and Go To Conflict` and the conflicting row's cell is read (Adobe, Customize keyboard shortcuts, 2023-05-24).

### [06.7]-[EXPORT_PRESETS]

| [INDEX] | [SURFACE] | [FIELDS] | [CHANNEL] | [PROOF] |
| :-----: | :-------- | :------- | :-------- | :------ |
| [01] | Quick Export (Settings > Export) | Quick Export Format `PNG`, `Transparency` on, `Smaller File` off; Quick Export Location `Ask where to export each time`; Quick Export Metadata `None`; Color Space `Convert to sRGB` on; Export As Location `Export assets to the last location specified`; `Use legacy "Export As"` off ([05] rows 74–77; Adobe, Quick Export as, 2026-02-23) | AX | descriptor `exportAssetsPrefs`: `exportFileType PNG`, `exportPNGTransparency true`, `exportAsLocationSetting` changed from `2` ([05] row 75), `exportMetaData 0`, `exportConvertToSRGB true`, `exportExportAsLegacy false`, `exportAssetsLocationSetting 3` |
| [02] | Export As dialog (`File > Export > Export As...`, ⌥⇧⌘W) | File Settings Format `PNG`, `Transparency` on, `Smaller File` off; Image Size Scale `1x`, Resample `Bicubic Automatic`; Canvas Size = Image Size; Metadata `None`; Color Space `Convert to sRGB` on, `Embed Color Profile` on; Scale All one row `1x`, no suffix (Adobe, Export settings and export location preferences, 2026-02-23, names every field); the dialog keeps its last values | CU on the UXP dialog (`Photoshop UXP Export-As` extension) with the template open, `Cancel` | reopen after relaunch, screenshot of the right column |
| [03] | Image Processor (`File > Scripts > Image Processor...`) | Select the images: `Use Open Images`, `Open first image to apply settings` off; Location: `Save in Same Location`; File Type: `Save as JPEG` on, Quality `10`, `Resize to Fit` on W `3840` H `2160`, `Convert Profile to sRGB` on; `Save as PSD` off; `Save as TIFF` off; Preferences: `Run Action` off, Copyright Info empty, `Include ICC Profile` on (Adobe, Convert files with the Image Processor, 2026-02-23: "Select Save to save the settings. To use them again, select Load"); `Save...` → `Default Image Processor.xml` | AX on the script dialog (Cocoa), `Save...` sheet | the XML on Drive `99.Default Profiles/Photoshop/` and under `.artifacts/creative-cloud/photoshop/`; `Load...` in a fresh session repopulates every field |
| [04] | Export pipeline | print: [06.5] row 30 then `File > Save As` TIFF; screen: [06.5] row 29 then row 02 of this table | actions | step listing |

## [07]-[PERSISTENCE_AND_EXPORT]

| [INDEX] | [ARTIFACT] | [WHAT_IT_CAPTURES] | [WHERE] | [DRIVE_NAME] |
| :-----: | :--------- | :----------------- | :------ | :----------- |
| [01] | Workspace | New Workspace dialog, Capture: Panel Locations, Keyboard Shortcuts, Menus (Adobe 2026-02-23); the toolbar slot list is not in the capture, the toolbar dock entry with `size-variant` is | Files that change on save: `WorkSpaces/Default Workspace.psw` created ("Workspaces - Contains any custom workspaces that have been saved"), `Workspace Prefs.psp` rewritten ("Contains list of workspaces (psw files) that are loaded and/or modified"), edits after the save land in `WorkSpaces (Modified)/Default Workspace.psw` (Adobe 2024-03-01); `WorkSpaces/` holds `Editorial Images.psw` and `WorkSpaces (Modified)/` holds `Editorial Images.psw` and `Essentials.psw`, all 43,199 B | `Default Workspace.psw` |
| [02] | Toolbar | Customize Toolbar `Save Preset...` | `<photoshop.supportFolder>/Presets/Custom Toolbars/Default Toolbar.<ext>` plus `Toolbar Customization.psp` and `Toolbar Customization Primary.psp` in the Settings folder (Adobe 2024-03-01) | `Default Toolbar.<ext>` (extension read at save, [09] row 04) |
| [03] | Keyboard shortcuts | dialog `Save Set` (Adobe 2023-05-24: saving over Photoshop Defaults opens the Save dialog for a set name) | `Presets/Keyboard Shortcuts/Default Keyboard Shortcuts.kys` (folder exists, empty) plus `Keyboard Shortcuts.psp` and `Keyboard Shortcuts Primary.psp` (Adobe 2024-03-01) | `Default Keyboard Shortcuts.kys` |
| [04] | Menus | dialog `Save Set` | `Presets/Menu Customization/Default Menus.mnu` (folder exists, empty) plus `Menu Customization.psp` and `Menu Customization Primary.psp` | `Default Menus.mnu` |
| [05] | Type styles | `Type > Save Default Type Styles` | `Default Type Styles.psp` | `Default Type Styles.psp` |
| [06] | Tool presets | Preset Manager `Save Set...` | `Presets/Tools/Default Tool Presets.tpl` | `Default Tool Presets.tpl` |
| [07] | Actions | `Save Actions...` | `Presets/Actions/Default Actions.atn` | `Default Actions.atn` |
| [08] | New Document presets | `Save Preset` per catalogue row ([06.1]) | `<photoshop.prefsFolder>/New Doc Sizes.json`, the `user` section's `presets` | `Default New Doc Sizes.json` (copy) |
| [09] | Templates | `build_template` body, rename ([06.1] rows 03–06) | `~/Documents/Adobe/Photoshop/Sizes/<name>.psdt`, `~/Documents/Adobe/Photoshop/Default Template.psdt` | `Sizes/<name>.psdt`, `Default Template.psdt` |
| [10] | Color settings | already saved | `~/Library/Application Support/Adobe/Color/Settings/Default Color Settings.csf` | `Default Color Settings.csf` |
| [11] | Camera Raw | prefs and defaults | `~/Library/Preferences/Adobe Camera Raw Prefs` (4,340 B), `~/Library/Application Support/Adobe/CameraRaw/Defaults/*.xmp` | `Default Camera Raw Prefs`, `Camera Raw Defaults/` |
| [12] | TK9 | module backups | `…/TK9 Backups/<module>/` | `TK9 Backups/` |
| [13] | Libraries | panel exports | Drive only | `Default Brushes.abr`, `Default Swatches.aco`, `Default Gradients.grd`, `Default Patterns.pat`, `Default Shapes.csh` |
| [14] | Hashes | `shasum -a 256` | `.artifacts/creative-cloud/exports.sha256` | |
| [15] | Workspace list | `Window > Workspace > Delete Workspace...` (dialog: "select a workspace from the dropdown menu, and then select Delete") | delete `Editorial Images` (the user's Essentials copy), `Core Tools`, `Motion`, `Painting`, `Photography`, `Graphic and Web` ("Photoshop warns you before deleting a preset workspace", Adobe 2026-02-23); `Essentials (Default)` stays; the `Restore Default Workspaces` button in Settings > Workspace (Adobe: "Edit > Preferences > Workspace > Restore Default Workspaces") never clicked; `Reset Default Workspace` on the submenu never run | System Events read of Window > Workspace lists Essentials (Default), Default Workspace, and the four commands alone |
| [16] | Relaunch verification | quit through the app menu item `Quit Photoshop` (System Events click), `open -b com.adobe.Photoshop`, then: `AXMenuItemMarkChar` of Window > Workspace > Default Workspace `✓` (System Events); the Window and Plugins menu `✓` set equals the open panels of [03] and the Workspace submenu lists the two workspaces of row 15 (`app.panelList` and `app.workspaceList` are `undefined` in ExtendScript 27.11, so System Events is the channel); window id from `AxDump windowlist` (G10; `2655` in the 2026-09 dump) → `screencapture -l <id>`, read with zoom (toolbar two columns, four panes, no closed panel visible); `app.systemInformation` shows no `CC Libraries Panel` row and TK9 Multi-Mask, Combo, Export `Loaded`; `get_preferences` over the twelve classes equals [05]; `list_presets` per kind equals [06], `list_presets tool` the 23 names of [06.4]; `executeActionGet` `ASet` count 1 and `Actn` count 30 ([06.5]); Keyboard Shortcuts `Summarize` HTML Tools rows equal [06.6]; `ls Sizes/` count equals the catalogue row count and one `open` per file gives the [06.1] row 03 readback; View > Proof Setup lists `Default Proof`; `.psw` decode by the `workspace-panels.txt` method shows `closed=true` for every closed row (`strings -n 6 <psw>` yields only the `is-closed`, `size-variant`, `preferred-iconic-length` attributes, 61 `is-closed="true"` and 47 `"false"` in `Editorial Images.psw`, no panel ids); `New Doc Sizes.json` and `Default Type Styles.psp` hashes unchanged | Photoshop's window is AX-opaque (13 nodes), so no AX layout diff exists | |
| [17] | Proof setup | View > Proof Setup > Custom… `Save` ([06.2] row 02) | `~/Library/Application Support/Adobe/Color/Proofing/Default Proof.psf` | `Default Proof.psf` |
| [18] | Image Processor settings | dialog `Save...` ([06.7] row 03) | `.artifacts/creative-cloud/photoshop/Default Image Processor.xml` | `Default Image Processor.xml` |
| [19] | Action step listing | ⌘⌥ `Save Actions...` ([06.5]) | `.artifacts/creative-cloud/passes/photoshop/default-actions.txt` | none (evidence file) |

## [08]-[ORDERED_PASS]

| [INDEX] | [STEP] | [CHANNEL] | [EVIDENCE] |
| :-----: | :----- | :-------- | :--------- |
| [01] | Baseline: copy the settings folder listing, `.psw` decodes, `New Doc Sizes.json`, `Adobe Camera Raw Prefs`, TK9 `PluginData`, `PS.json` rows, Window menu checkmarks, the `AddOnModules/` and `Color/Proofing/` listings | FILE, System Events | `baseline/` under `.artifacts/creative-cloud/passes/photoshop/` |
| [02] | P0 `.psjs` runtime test | AX File > Scripts > Browse… | recorded result, nothing depends on it |
| [03] | P1 bridge install, relaunch, POC steps 3–6 | `nx run @rasm/photoshop:deploy`, `get_health` | `PS.json` row `enabled` |
| [04] | Preferences, `set_preferences` keys ([05] rows with a class key); order inside `notifications`: `showToolTips`, `useRichToolTips`, `showWhatsNew`, `showFeatureOnboarding`, then `quietMode true`; `history.createFirstSnapshot`, `history.nonLinearHistory` in the same pass | bridge | `applied[]` with from/to; `get_preferences notifications` after the quiet write recorded verbatim ([09] rows 15–16); a second `showWhatsNew` write afterwards returns `preference-locked` |
| [05] | Preferences, ES-only keys (`fontPreviewSize`) | ES | readback string |
| [06] | Preferences, AX-only controls per pane, Interface first (theme by CU on the first swatch, `Highlight Color` popup, `Show Menu Colors`), Guides, Grid & Slices (`Gridline Every` 45 Pixels, `Subdivisions` 3), Plugins (`Load Extension Panels` off), Enhanced Controls, Technology Previews; `Tiny` is the bridge key of step 04 | AX on `Preferences` window, CU for the theme swatches | AX re-read per pane; descriptor for the theme and the grid |
| [07] | Edit > Menus hidden rows, `Save Set` `Default Menus` | AX dialog | dialog re-read |
| [08] | Edit > Keyboard Shortcuts, Tools category: read every tool's Shortcut cell (the live tool-letter list), the four [06.6] rows 01–04 in order, `Save Set` `Default Keyboard Shortcuts`, `Summarize` | AX dialog list and Shortcut column | the read list equals the `[KEY]` column of [02] before the changes; `.kys` present; the Summarize HTML Tools rows equal [06.6]; [09] row 21 |
| [09] | Quit, relaunch (font size, extension panels, Modern UI take effect) | System Events click on `Quit Photoshop` (application menu), `open -b com.adobe.Photoshop` | `app.version`, no Libraries row in `app.systemInformation` |
| [10] | Proof setup `Default Proof` with no document open ([06.2] row 02), `Save` | AX dialog, window-id screenshot | `Default Proof.psf` present; View > Proof Setup lists it |
| [11] | Toolbar: Edit > Toolbar, the drag of [02] row 29, Show chips, Done, `Save Preset...` `Default Toolbar`, `>>` two columns | AX for buttons, CU for drags | screenshot; file in `Custom Toolbars/` ([09] rows 03–04, 08) |
| [12] | Close every [03] closed panel; drag TK9 Multi-Mask and Combo into pane B, Export minimized below; pane C = Properties/Adjustments; pane D = Swatches/Color/Gradients, Actions/History, Layers/Channels/Paths; pane A iconic = Character, Paragraph, Glyphs, Character Styles, Paragraph Styles, Patterns, Styles, Shapes, Brushes, Brush Settings, Clone Source, Layer Comps, Tool Presets; remove the bottom dock | Window menu by AX, panel drags by CU | screenshot per pane; Window and Plugins menu `✓` set (System Events) |
| [13] | Panel options per [04] | CU flyouts | descriptor `layerThumbnailSize small`; screenshot |
| [14] | Camera Raw preferences and workflow ([06] rows 20–27) | Photoshop (Beta) > Settings… > Camera Raw…; the workflow link needs the Camera Raw Filter open on a scratch document | decoded prefs; [09] rows 14, 17 |
| [15] | TK9 preferences per module ([06] rows 28, 29, 31) | module fly-outs (CU) | `.ini` files |
| [16] | L-P1 brushes: review sheets (`render_brush_sheet` body), import, normalize, export | bridge + panel flyouts | `list_presets brush` |
| [17] | L-P2 patterns, styles, gradients, swatches (the unified set, `cloud-libraries.md` [08] row 05 and [09] row 09), shapes: review, import, export | same | `list_presets` per kind |
| [18] | L-P3 delete every factory group ([06] rows 02–07) and the 21 factory tool presets ([06] row 08) | CU + Preset Manager | `list_presets` counts |
| [19] | Tool presets: the 23 of [06.4] from the Tool Presets panel, `Save Set...` `Default Tool Presets.tpl` | CU panel and Options bar, Preset Manager | `list_presets tool` = 23 names |
| [20] | Actions per [06.5]: `New Set...`, `Load Actions...` of the overlay set, drags, the three recordings, deletions, `Save Actions...`, ⌘⌥ `Save Actions...` text export | panel flyout (CU), recording through the menus | `ASet` 1, `Actn` 30; `default-actions.txt` equals [06.5] |
| [21] | Type styles: scratch document by `batch_play` `make document` (the [06.1] row 05 descriptor of `Digital 3840x2160`), `apply_type_styles` body for Body, Heading, Caption, `Type > Save Default Type Styles`, close without saving | bridge, AX | `Type > Load Default Type Styles` into a second scratch document lists the three |
| [22] | Size catalogue: `build_template` body once per row of `page-sizes.json` ([06.1] rows 03–05), rename to `.psdt`, copies to `~/Documents/Adobe/Photoshop/Sizes/` and `Default Template.psdt` | bridge, FILE | per-row `open` readback ([06.1] row 03) |
| [23] | New Document presets: one `Save Preset` per catalogue row with the [06.1] row 02 fields, `Print CMYK Letter 8.5x11` last | CU in the New Document dialog | the `user` section's `presets` length ([06] row 15 `jq`); [09] row 20 after the first save |
| [24] | Channels and masks on the master template: Quick Mask Options, Layer Mask Display Options, Channel Options, Select and Mask defaults with `Remember Settings` ([06.3] rows 02–05) | CU dialogs and workspace | reopened dialogs screenshots |
| [25] | Export defaults: Export As dialog state and `Cancel`, Image Processor fields and `Save...` ([06.7] rows 02–03) | CU (UXP dialog), AX (script dialog) | reopened Export As screenshot; `Default Image Processor.xml` |
| [26] | Remove add-ons: Options bar with Remove selected, one stroke on a scratch copy of the template, offline ([02] rows 32–33) | CU, screenshot | Options bar labels; the stroke completes; descriptor `imageProcessingRemoveToolProcessingPrefsStr` |
| [27] | Rulers on with the template open (`View > Rulers` is disabled with no document); contextual task bar pinned (`Pin bar position`) | AX View > Rulers, CU bar menu | checkmark; [09] row 13 |
| [28] | `Window > Workspace > New Workspace...` `Default Workspace`, three Capture boxes on | AX dialog | `WorkSpaces/Default Workspace.psw`; [09] row 05 |
| [29] | Delete the other user and preset workspaces ([07] row 15) | AX Delete Workspace dialog | System Events read of Window > Workspace; [09] row 12 |
| [30] | Quit; copy every [07] artifact to Drive `05.Software Related Assets/99.Default Profiles/Photoshop/`; hashes | FILE | `exports.sha256` |
| [31] | Relaunch verification ([07] row 16) | AX, ES, bridge, screencapture | evidence files |
| [32] | Consistency-agent review against Illustrator's column | consistency agent | consistency cell |

## [09]-[PROBES]

Settled rows name the input that answers them; probe rows name the channel the executing agent uses and the readback it records, at the [08] step the row cites.

| [INDEX] | [QUESTION] | [CHANNEL] | [EXPECTED_READBACK] |
| :-----: | :--------- | :-------- | :------------------ |
| [01] | Every top-level menu, the Settings, Workspace, Presets, Language Options, Font Preview Size, and Export submenus, checkmarks, the window frame | settled: `<Inputs>/ax/photoshop.json` `menus`, `windowlist` | as [03], [05], [06] row 30, [11] cite them |
| [02] | Current preference values | settled for every row naming a key of `<Inputs>/photoshop/preferences.json` or `application-descriptor.json`; `quietMode`, `showWhatsNew`, `showFeatureOnboarding` are absent from both and from every `.psp`, so their current value is the `from` field of the [08] step 04 `applied` row; the rows with no key are row 23 | the `current` half of every [05] row |
| [03] | Identity of toolbar Show chips 5 and 6 | CU hover over each chip in the Customize Toolbar dialog, zoomed window-id screenshot ([08] step 11) | two tooltip strings, recorded in [02] row 27; both chips hidden |
| [04] | Toolbar preset file extension | `Save Preset...` `Default Toolbar`, then `ls "<photoshop.supportFolder>/Presets/Custom Toolbars/"` ([08] step 11) | one file `Default Toolbar.<ext>`; `<ext>` fills [07] row 02 and the Drive name |
| [05] | `.psw` `size-variant` token of the double-column toolbar | `strings -n 6 "WorkSpaces/Default Workspace.psw" \| rg -o 'size-variant="[a-z-]*"'` after [08] step 28 | one token, not `vertical-narrow`; recorded in [02] row 30 |
| [06] | Placement of the Brushes panel flyout view items (`Brush Tip`, `Brush Name`, `Brush Stroke`, `Show Additional Preset Info`, `Show Recent Brushes`, binary strings) | CU open the flyout, screenshot ([08] step 13) | the five items visible with their check states set per [04] row 10 |
| [07] | Group-delete command in the six preset panels (`Delete Group`, a binary string) | right-click a group, screenshot the context menu ([08] step 18) | `Delete Group` in the menu; the deletion proceeds through it |
| [08] | Dialog order of the 72 tools and whether dragging a sub-tool to the top of its slot makes it the visible tool | scroll the Customize Toolbar left list with screenshots; drag Lasso above Selection Brush, `Done` ([08] step 11) | every `inToolBar` tool of `tools[]` appears once in the list; the slot icon shows Lasso |
| [09] | Contents of the current `Default Type Styles.psp` | settled: UTF-16 scan, Adobe's CS6 seed with `Basic Paragraph` only ([06] row 14) | replaced at [08] step 21 |
| [10] | Digital canvas | settled: `Digital 3840x2160` on the 45 px module, the master of `gui-indesign.md` [07] ([06.1]) | none |
| [11] | `set_preferences` acceptance of `type.showTextFeatures` | bridge `set_preferences {type: {showTextFeatures: "middleEasternInterface"}}` ([08] step 04) | `applied` holds `{section: "type", key: "showTextFeatures", from: "defaultTextInterface", to: "middleEasternInterface"}`; Type > Language Options > Middle Eastern Features `✓` |
| [12] | Which workspaces the `Delete Workspace...` popup offers | AX read of the dialog popup ([08] step 29) | `Editorial Images`, `Core Tools`, `Motion`, `Painting`, `Photography`, `Graphic and Web`, `Default Workspace` listed; `Essentials` absent or refused |
| [13] | Relaunch persistence of View > Rulers | AX `AXMenuItemMarkChar` of View > Rulers after [08] step 31 with the template open | `✓` |
| [14] | Camera Raw Preferences generative-AI control in ACR 18.6 | screenshot of every pane of the Camera Raw Preferences dialog ([08] step 14) | the pane list and each pane's controls; the pane and label of the generative control, set off, recorded in [06] row 25 and [11] row 22 |
| [15] | Value of `useRichToolTips` while Quiet Mode is on | `get_preferences notifications` and descriptor `useRichToolTips`, `useRichToolTipsRestore` after [08] step 04 | the two values, recorded in [05] row 36 |
| [16] | Whether a `get_preferences` read of a locked key throws under Quiet Mode | the same read | a value, or `preference-locked{section, key}`; the descriptor keys of row 15 are the readback in the second case |
| [17] | Camera Raw Workflow Options preset control | screenshot of the Workflow Options dialog ([08] step 14) | its controls; a preset control saves `Default Workflow`, the six values of [06] row 26 are recorded in either case |
| [18] | Shortcut column of every hidden menu row | settled: `<Inputs>/ax/photoshop.json` `menus` `AXMenuItemCmdChar` | one hidden row carries a shortcut, `Image > Generative Upscale...` (`menubar/4/0/9`, `U`, `AXMenuItemCmdModifiers 3` = ⌥⇧⌘U); no [06.6] row and no pass step uses it |
| [19] | `Restore Default Workspaces` button and `Reset <workspace>` item never clicked | settled: AX dump `AXButton`, System Events submenu | none |
| [20] | `New Doc Sizes.json` preset object shape | settled by the sibling store `<photoshop.prefsFolder>/MRU New Doc Sizes.json` (1,098 B), whose `presets[0]` is `{name, identifier, group, width, height, units "inchesUnit", profile, resolution 300, resolutionUnits "inchesUnit", depth 16, scale 1, mode "RGB", fill "transparent", guides [], artboards [], lastUsedTime}`; confirmed on the `user` store by `jq '.sections[] \| select(.section == "user") \| .presets[0]'` after the first `Save Preset` of [08] step 23 | the same key set; the `units` string for millimetres and pixels and the `mode` string for CMYK recorded from the saved entries; [06.1] rows 01–02 and 07 read them |
| [21] | `⇧A` accepted on the four lasso tools beside the `A` slot | type `⇧A` in the Lasso, Polygonal Lasso, Magnetic Lasso, and Selection Brush rows of the Tools list ([08] step 08) | the Shortcut column shows `⇧A` on the four rows and `A` on Path Selection and Direct Selection with no conflict alert; the readback names the alert text otherwise |
| [22] | Remove tool Options bar labels | window-id screenshot with Remove selected ([08] step 26) | `Remove after each stroke`, `Sample all layers`, the `Find distractions` menu with its three entries |
| [23] | AX readability of the Preferences dialog and the baseline of every [05] row without a key | at [08] step 01: `batch_play` `get` of the application's preference sub-descriptors listed in the header (`multiGet`, `photoshop-uxp.md` [05] BATCH_PLAY row 09) written to `baseline/application-descriptor.json`; at [08] step 06: System Events dump of the `Preferences` window with each of the 21 panes active (`<Inputs>/ax/run.sh photoshop` writes only the menu bar and the AX-opaque main window, `tree` 25 nodes, so no pane dump exists in `<Inputs>`) | the pane list titles; per pane, every titled checkbox, popup, radio, and field with its value, recorded as `baseline/preferences-<pane>.json`; a pane whose dump holds no titled control is read by a window-id screenshot with zoom and its rows switch to CU; the four theme swatches' AX titles or their absence ([05] row 14); the key name of every [05] row whose readback reads `descriptor` without one, filled from the sub-descriptor read |

## [10]-[SOURCES]

| [INDEX] | [SOURCE] | [DATE] |
| :-----: | :------- | :----- |
| [01] | Adobe, Customize toolbar, `helpx.adobe.com/photoshop/desktop/get-started/set-up-toolbars-panels/customize-the-toolbar.html` | 2026-02-23 |
| [02] | Adobe, Save custom workspaces, `…/learn-the-basics/save-custom-workspaces.html` | 2026-02-23 |
| [03] | Adobe, Delete workspaces, `…/learn-the-basics/delete-workspaces.html` | 2026-02-23 |
| [04] | Adobe, Workspace overview, `…/learn-the-basics/workspace-overview.html` | 2026-06-05 |
| [05] | Adobe, Home screen overview, `…/learn-the-basics/homescreen-overview.html` | 2026-02-23 |
| [06] | Adobe, Change text size in panels and tooltips, `…/learn-the-basics/change-text-size.html` | 2026-02-23 |
| [07] | Adobe, Boost workflows with the Contextual Task Bar | 2026-02-23 |
| [08] | Adobe, List of technology preview features | 2026-02-23 |
| [09] | Adobe, Modify preferences in Photoshop, `helpx.adobe.com/photoshop/using/preferences.html` (Enable quiet mode, Image Processing Cloud/Device) | 2025-07-29 |
| [10] | Adobe, Preference file functions, names, locations, `helpx.adobe.com/photoshop/kb/preference-file-names-locations-photoshop.html` | 2024-03-01 |
| [11] | Adobe, Tools missing from the toolbar, `helpx.adobe.com/photoshop/kb/bringing-back-tools.html` | 2024-10-17 |
| [12] | Adobe, Use presets in Photoshop (Preset Manager: contours and tools, Save Set, Load), `helpx.adobe.com/photoshop/using/presets.html` | 2023-05-24 |
| [13] | Adobe, Manage pattern libraries and presets, `helpx.adobe.com/photoshop/using/patterns-libraries-presets.html` | 2023-05-24 |
| [14] | Adobe, Use the Color and Swatches panels | 2023-05-24 |
| [15] | Adobe, Create documents (Preset Details fields, Save Preset, Saved tab, `.psdt` opens as a `.psd` instance), `helpx.adobe.com/photoshop/using/create-documents.html` | 2024-02-07 |
| [16] | Adobe, Brush presets (Import Brushes flyout option, Delete brush, groups), `helpx.adobe.com/photoshop/using/brush-presets.html` | 2024-11-11 |
| [17] | Adobe, Manage Camera Raw settings (Save Image Settings In, DNG sidecar, workflow options, preference file table), `helpx.adobe.com/camera-raw/using/camera-raw-settings.html` | 2026-06-09 |
| [18] | Adobe, Panels and menus (Edit > Menus, Visibility button, Save Set, Show All Menu Items), `helpx.adobe.com/photoshop/using/panels-menus.html` | 2023-05-24 |
| [19] | Adobe, Customize keyboard shortcuts (Shortcuts For: Application Menus, Panel Menus, Tools, Taskspaces; conflict alert, Accept and Go To Conflict; Save Set, Save Set As, Delete Shortcut, Summarize), `helpx.adobe.com/photoshop/using/customizing-keyboard-shortcuts.html` | 2023-05-24 |
| [20] | Adobe, Set up default settings for raw images | 2025-08-20 |
| [21] | Adobe, AI Assistant overview | 2026-06-23 |
| [22] | Adobe, Photoshop on desktop release notes (27.8) | 2026-06-18 |
| [23] | Adobe UXP Photoshop reference, `Preferences`, `PreferencesNotifications` (via `photoshop-uxp.md` [06]) | current |
| [24] | Adobe `PIStringTerminology.h` (`brushesImport`, `swatchesImport`, `gradientsImport`, `patternsImport`, `stylesImport`, `customShapesImport`, `*Export`, `new*Group`) | 2024-12-20 commit |
| [25] | Tony Kuyper, TK9 v4 Instructions Manual (`Instructions-Manual-TK9-v4.pdf`), TK9 v4 release notes, tk9.html | May–June 2026 |
| [26] | Adobe community, `.psdt.psd` save behaviour | 2021-03-02 |
| [27] | Adobe community, `.abr` import creates a group | 2018-01-09 |
| [28] | `photoshop-brushes-and-skills.md` [1.2], [2.3], [6.1]–[6.4]; `cloud-libraries.md` [02.2], [03.2], [03.4], [03.5], [08], [09]; `gui-indesign.md` [07]; `architecture.md` [06.2]; `photoshop-uxp.md` [05], [06] | current |
| [29] | Adobe, Proofing colors (Proof Setup presets, Custom options, default proof setup with no document open, Save in the default location), `helpx.adobe.com/photoshop/using/proofing-colors.html` | 2023-05-24 |
| [30] | Adobe, Color settings (working spaces, policies, conversion options, advanced controls, Save and Load), `helpx.adobe.com/photoshop/using/color-settings.html` | 2023-05-24 |
| [31] | Adobe, Create a temporary quick mask (Quick Mask Options: Masked Areas, Selected Areas, colour, opacity), `helpx.adobe.com/photoshop/using/create-temporary-quick-mask.html` | 2023-05-24 |
| [32] | Adobe, Export settings and export location preferences (File Settings, Image Size, Canvas Size, Metadata, Color Space, Scale All), `helpx.adobe.com/photoshop/desktop/save-and-export/export-files-to-different-formats/export-settings-and-export-location-preferences.html` | 2026-02-23 |
| [33] | Adobe, Export your work using the Quick Export as option (the Export preferences), `helpx.adobe.com/photoshop/using/export-artboards-layers.html` | 2026-02-23 |
| [34] | Adobe, Convert files with the Image Processor (every field, Save and Load), `helpx.adobe.com/photoshop/using/processing-batch-files.html` | 2026-02-23 |
| [35] | Adobe, Create tool presets (Tool Presets panel, Create New Tool Preset, flyout view items), `helpx.adobe.com/photoshop/desktop/get-started/set-up-toolbars-panels/create-tool-preset.html` | 2026-02-23 |
| [36] | Adobe, Play and manage actions (Save Actions, Command+Option text export, Load Actions, Replace Actions), `helpx.adobe.com/photoshop/using/playing-actions.html` | 2024-04-23 |
| [37] | Adobe, Record an action (New Action: Name, Set, Function Key, Color), `helpx.adobe.com/photoshop/using/creating-actions.html` | 2026-02-23 |
| [38] | Adobe, Remove unwanted objects and distractions, `helpx.adobe.com/photoshop/using/remove-tool.html`; Remove tool minimum and recommended hardware requirements, `helpx.adobe.com/photoshop/desktop/repair-retouch/remove-objects-fill-space/remove-tool-minimum-and-recommended-hardware-requirements.html`; What's new in Adobe Photoshop on desktop (Find distractions, on-device model) | 2026-01-29; 2026-01-29; 2026-08-28 |
| [39] | Machine: `<Inputs>/photoshop/{preferences,application-descriptor,preset-manager}.json`, `<Inputs>/ax/photoshop.json`; `<photoshop.prefsFolder>/New Doc Sizes.json` and `MRU New Doc Sizes.json`; `app.version`, `app.colorSettings`, `app.preferences.fontPreviewSize`, `typeof app.workspaceList`, `typeof app.panelList`, and `app.systemInformation` through `osascript`; `AddOnModules/sensei_model_cache/inpainting_ai/` listings and sizes; `strings -n 6` over the Photoshop binary for the Remove tool labels; `<Sources>/photoshop/probe-photoshop.md`, `system-information.txt`, `workspace-panels.txt`, `workspace-dock-tree.txt`; the `.psw` files; the TK9 `PluginData/*.ini` files | current |

## [11]-[AI_AND_PROMO_SURFACES]

Classes: `KEY` one of the 43 `set_preferences` keys (current value shown); `AX` a Settings-dialog control (path: Preferences window, pane, control title); `MENU` an Edit > Menus visibility toggle (captured by the workspace); `PANEL` a Window or Plugins item closed in the workspace; `PLUGIN` an Adobe UXP extension in `Contents/Required/UXP/` or the Shared UXP folder (`app.systemInformation`: "from Required folder", "from Shared UXP folder"), none of which the Plugins panel or Creative Cloud lists as disableable; `FIXED` server-flagged or hard-wired with no local control (no descriptor key names the surface, and the pane read of [09] row 23 records any Settings control that does). Menu rows cite `<Inputs>/ax/photoshop.json` `menus` paths (`menubar/2` File, `3` Edit, `4` Image, `5` Layer, `6` Type, `8` Filter, `10` Plugins, `11` Window, `12` Help).

| [INDEX] | [SURFACE] | [CLASS] | [MECHANISM_AND_CURRENT] | [RESIDUAL] | [SOURCE] |
| :-----: | :-------- | :------ | :---------------------- | :--------- | :------- |
| [01] | AI Assistant panel | PANEL + MENU | Window > AI Assistant closed (current closed); menu row hidden | App-bar AI Assistant icon (top-right): FIXED, `showAIAssistant` exists only as a binary string, no Settings control | Adobe AI Assistant overview 2026-06-23 ("Select the AI Assistant icon in the top-right corner"); AX dump |
| [02] | AI Assisted Editor mode | AX + MENU | Preferences > Interface > `Show AI Assisted Editor` off (current value read at [09] row 23; the Window toggle is unchecked in `menus`); Window > AI Assisted Editor row hidden | none | `menus` `menubar/11` (AI Assisted Editor item, no `AXMenuItemMarkChar`); `showAIAssistedButton` in `MachinePrefs.psp` (strings) |
| [03] | Generative Fill, Generate Image, Prompt to Edit, Reflection Removal, Sky Replacement (Edit menu) | MENU | Edit > Menus hides the five rows (current visible) | Contextual Task Bar buttons `Generative Fill`, `Generative Expand`, the Prompt to Edit field (27.10), and the Remove tool `Find distractions` menu: FIXED inside the bar the intent keeps | Adobe Contextual Task Bar page 2026-02-23; `menus` `menubar/3/0/20`–`23`, `menubar/3/0/33` |
| [04] | Generative Expand in Crop, Generative layer controls in Properties | FIXED | No preference; Properties shows generative controls only on a generative layer, none in our documents | Crop tool contextual `Generative Expand` button | Adobe CTB page |
| [05] | Properties Quick Actions (Remove Background, Select Subject) | FIXED, on-device | Image Processing > Select Subject and Remove Background = `Device` (AX popup; descriptor `imageProcessingModeDevice`); Selections, Remove, Enhance = `Faster` | Buttons stay in Properties; processing never leaves the machine | descriptor; Adobe preferences page 2025-07-29 (Cloud option described, Device chosen) |
| [06] | Select > Subject, Select > Sky | retained on-device AI | Kept: TK9 Combo's Select Subject and Select Sky buttons call them; processing `Device` | none | TK9 manual ("The panel honors the Select Subject Processing choice") |
| [07] | Image > Generative Upscale, Layer > Harmonize, Layer Mask > Enhance edge, Filter > Neural Filters, Filter > AI Denoise, Filter > AI Sharpen, File > Export > Send to Firefly Boards, File > Search Adobe Stock, File > Search Adobe Express Templates | MENU | Edit > Menus hides each row (current visible) | Neural Filters (`Neural Filters (Registered) from Required folder`) and the Camera Raw Filter's own Denoise stay installed: PLUGIN, not disableable | `menus` `menubar/2/0/17/0/7`, `2/0/19`–`20`, `4/0/9`, `5/0/15`, `5/0/18/0/12`, `8/0/4`, `8/0/11`–`12`; `system-information.txt` |
| [08] | File > Invite to Edit, Share for Review; app-bar Share icon; share sheets | MENU + PANEL + FIXED | Menu rows hidden; `ccx.sharesheet/invite`, `/review` closed; `Share Panel (Loaded) from Required folder` is PLUGIN | Share icon in the app bar | `menus` `menubar/2/0/15`–`16`; `.psw` ids 43–44 |
| [09] | Comments | PANEL + MENU + FIXED | Window > Comments closed and hidden; `CCX Commenting UXP Webview (Loaded) from Required folder` | Comments icon in the app bar | `system-information.txt` |
| [10] | Notifications bell, in-app messaging, gifts, promo badges | KEY | `notifications.quietMode` (no descriptor key, no persisted key; current = the `applied` row's `from`) → on; `In app notifications`, `Photoshop In App Messaging` extensions stay PLUGIN | The bell icon itself | Adobe preferences page 2025-07-29; UXP `PreferencesNotifications` |
| [11] | What's New | KEY + MENU | `notifications.showWhatsNew` (no descriptor key; current = the `applied` row's `from`) → off, written before quiet mode; Help > What's New... hidden | none | same; `menus` `menubar/12/0/2` |
| [12] | Feature onboarding coach marks | KEY | `notifications.showFeatureOnboarding` (no descriptor key; current = the `applied` row's `from`) → off, written before quiet mode | none | same |
| [13] | Rich tooltips | KEY | `notifications.useRichToolTips` (current true, `useRichToolTipsRestore true`) → on, written before quiet mode (kept as tool help, not promo); locked while Quiet Mode is on, value re-read | none | same |
| [14] | Labs flask (Beta features and feedback) | PANEL + FIXED | Window > Beta Feedback closed and hidden; `Beta Feedback (Prepared) from Required folder` PLUGIN; the flask icon has no control | Flask icon in the app bar of the Beta build | `system-information.txt`; AX dump |
| [15] | Home screen | AX + FIXED | Preferences > General > `Auto show the Home Screen` = 0 (current 0; descriptor `autoShowHomeScreen false`, `homeScreenVisibility false`); `Home Screen (Loaded) from Shared UXP folder` PLUGIN | Home button at the left of the Options bar (Adobe: "select Home in the Options bar to return to the Home screen") | Adobe Home screen overview 2026-02-23 |
| [16] | Learn | MENU + FIXED | Help > Photoshop Help..., Hands-on Tutorials... hidden; Home's Learn tab unreachable with Home off | Search icon opens the Discover panel | `menus` `menubar/12/0/0`–`1` |
| [17] | Discover panel | FIXED | `Discover Panel (Loaded) from Required folder` PLUGIN; `DisableEmbeddedDiscoverPanel` is a binary string with no Settings control; `Photoshop Utility Panel` closed in the workspace | Search icon in the app bar | `ps-binary-strings.txt`; `.psw` id 61 |
| [18] | Generative credits rows | MENU | Help > Learn more about generative credits, Adobe generative AI user guidelines hidden | none | `menus` `menubar/12/0/7`–`8` |
| [19] | Adobe Stock panel | PANEL + MENU | Window > Adobe Stock closed and hidden; `Adobe Stock (Prepared) from Required folder` PLUGIN | none visible | `.psw` id 48 |
| [20] | Substance 3D Materials | PANEL | Window > Materials closed; three `photoshop-material-filters` panels closed; `Substance 3D Viewer Plugin (Prepared) from Shared UXP folder` PLUGIN | Filter > Parametric Filters... row (hidden through Menus) | `.psw` ids 45–47 |
| [21] | Content Credentials | AX + PANEL | History & Content Credentials pane: document options None, `Ask when opening` off (current on), export None; Window > Content Credentials (Beta) closed | `Content Credentials (Loaded)` PLUGIN | descriptor `contentCredentialsDocumentAsk` |
| [22] | Camera Raw generative features | AX (Camera Raw Preferences dialog) | The generative-AI control, off (pane and label: [09] row 14; the Manage Camera Raw settings page of 2026-06-09 names none) | Camera Raw Denoise and Enhance models in `ModelZoo/` (550 MB cache) stay on disk | Adobe Manage Camera Raw settings 2026-06-09; `probe-photoshop.md` [03] |
| [23] | AI suggestion strips inside panels (Layers generative bar, Adjustments presets) | FIXED | Layers bar appears only for generative layers (`cxui_generative_layer_bar`); Adjustments presets are static factory groups, not AI | none in our documents | `ps-binary-strings.txt`; `Adjustments_Presets/presets.json` |
| [24] | Creative Cloud Libraries | PANEL + AX | Panel closed; Plugins > `Load Extension Panels` off removes the CEP panel | none | [06] row 35 |
| [25] | Plugins marketplace | MENU + PANEL | Plugins > Browse Plugins... hidden; Plugins Panel closed | Plugins > Manage Plugins... row kept for TK9 | `menus` `menubar/10/0/1`–`2` |
| [26] | Adobe Fonts promo row | MENU | Type > More from Adobe Fonts... hidden | Font activation itself is a Creative Cloud app setting ([05] row 133) | `menus` `menubar/6/0/0` |
| [27] | Cloud document history | MENU + PANEL | File > Version History submenu and Window > Version History hidden; panel closed | none | `menus` `menubar/2` (File) and `menubar/11` (Window) |

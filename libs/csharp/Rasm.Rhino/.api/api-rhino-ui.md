# [RASM_RHINO_API_RHINO_UI]

Catalog scope: the `Rhino.UI` native integration surface — panel registration, Eto bridging, native dialogs, page adapters, gumball, mouse callbacks, status/toolbar chrome.

[NAMESPACES]:
- `Rhino.UI` panels — `Panels` (register/open/float/close/sibling/visibility/dock-bar families, `ChangePanelIcon`, `OnShowPanel`/`OnClosePanel`), `IPanel`, `PanelType`.
- `Rhino.UI` Eto bridge — `RhinoEtoApp.MainWindowForDocument`, `EtoExtensions` (`UseRhinoStyle`, `RestorePosition`/`SavePosition`, `Show`/`ShowSemiModal`, `WindowsFromDocument`).
- `Rhino.UI` dialogs — `Dialogs` (color/message/text/list-box/multi-list/check-list/property-list/context-menu/edit-box/number-box/layer/multi-layer/layer-material/linetype/print-width/sun families).
- `Rhino.UI` pages — `StackedDialogPage` (child pages, activation, navigation styling), `OptionsDialogPage`, `ObjectPropertiesPage`/`ObjectPropertiesPageEventArgs`/`PropertyPageType`, `LocalizeStringPair`.
- `Rhino.UI` gumball — `GumballObject`, `GumballDisplayConduit`, `GumballAppearanceSettings`, `GumballMode`, `GumballFrame` (set-from geometry families, `PickGumball`, `UpdateGumball`, pre/gumball/total transforms).
- `Rhino.UI` interaction — `MouseCallback` (move/down/up/double-click/enter/hover/leave families), `MouseCallbackEventArgs`, `MouseButton`, `MouseCursor`, `WaitCursor`.
- `Rhino.UI` resources — `DrawingUtilities` (mesh/curve preview images, icon/bitmap/SVG resource loading with scale-down), `NamedColorList`/`NamedColor`.
- `Rhino.UI` chrome — `StatusBar` (command prompt, message/distance/number/point panes, progress meter), `RuiUpdateUi` (menu state sync, menu-item registration), `ToolbarFile`/`ToolbarFileCollection`/`Toolbar` (open/find/group/save/sidebar families).
- `Rhino.UI.Controls` — `EtoCollapsibleSection`/`EtoCollapsibleSectionHolder`.
- `Rhino.Display` — `RhinoView.ShowToast` (view toast overloads).
- `Rhino` — `RhinoApp` UI-thread members (`InvokeOnUiThread`, `InvokeAndWait`, `IsOnMainThread`).

# [SETTINGS]

Application settings write through their owning class or `PersistentSettings` inside `run_python`, read back in the same call, and again after a relaunch.

## [01]-[STORES]

Paths sit under `~/Library/Application Support/McNeel/Rhinoceros/9.0/` unless absolute:

| [INDEX] | [STORE]                                                          | [HOLDS]                                                         |
| :-----: | :--------------------------------------------------------------- | :-------------------------------------------------------------- |
|  [01]   | `settings/settings-Scheme__Default.xml`                          | Options, display modes, aliases, shortcuts, plugin registry     |
|  [02]   | `settings/Scheme__Default/containers.xml`                        | Launch layout, every dock bar the process held at its last quit |
|  [03]   | `UI/default.rui`                                                 | Toolbar groups, toolbars, macros                                |
|  [04]   | `Plug-ins/<name> (<guid>)/settings/settings-Scheme__Default.xml` | `PlugIn.Settings` and `<command name>` blocks of one plugin     |
|  [05]   | `../Template Files/Default.3dm`                                  | `FileSettings.TemplateFile` new documents open from             |
|  [06]   | `~/Library/Preferences/com.mcneel.rhinoceros.9.plist`            | Window frames, `MRDefaultTemplateFilename`, Sparkle update keys |
|  [07]   | `~/Library/Application Support/Grasshopper2/Settings/*.ghs`      | Grasshopper 2 settings, one binary node per file                |

- Settings XML stores keys that differ from their registered default alone, a key equal to its default drops at quit and reads its default
- Colors store as `A,R,G,B`, plugin settings files delete when every value equals its default
- Native settings objects (General, Mouse, File, Appearance, Grid, View, ContextMenu, ShortcutKeys) write the tree at quit from their own memory
- Rhino rewrites the settings XML at quit, on the idle after a `PersistentSettings` write, and on `UpdateDisplayMode(mode)` and `SaveDisplayModes()`
- Crashes skip the quit rewrite and lose every write no idle saved
- Rhino overwrites edits to a file it holds in memory, file edits go between a quit and a relaunch
- `cfprefsd` rewrites the defaults domain while Rhino runs, `defaults write` runs while Rhino is closed
- Grasshopper 2 `.ghs` files rewrite whole 1 second after each setter, never at quit, a `.ghs` edited beside a running editor is overwritten

## [02]-[WRITES]

- `Rhino.ApplicationSettings.<Class>.<Member> = value` writes the native object at once and persists, a member read by name proves it
- `PersistentSettings.RhinoAppSettings.AddChild("Options").AddChild("<Group>")` reaches a key's child, `GetChild` raises when the child is absent
- `child.Set<Type>(key, value)` writes, `child.TryGet<Type>(key)` reads `(found, value)`, `Get<Type>(key, default)` writes `default` into the store
- One-argument getters (`GetBool(key)`) read a registered key and raise `NotSupportedException` when the stored text is another type
- `SetEnumValue` and `TryGetEnumValue` take an enum type, `SetStringList(key, Array[String](...))` takes an array
- `SetStringDictionary(key, Array[KeyValuePair[String, String]](...))` writes a dictionary, rows read as `pair.Key` and `pair.Value`
- `child.TryGetDefault.Overloads[<clr type>, <clr type>.MakeByRefType()](key)` reads a registered default, the plain call fails to bind
- Key writes a native object owns reach it through the idle save's reload, `PlugIn.FlushSettingsSavedQueue()` saves and reloads at once
- Write stages end with that flush, a quit before the next idle loses unflushed key writes, the native object writes the value it loaded over them
- Internal owners write through `<Type>.GetProperty(name).SetMethod.CreateDelegate(Action[T])`, `PropertyInfo.SetValue` refuses a Python `int`
- `System.Type.GetType("<Namespace>.<Type>, <Assembly>", throwOnError=True)` reaches an internal enum or type by its assembly-qualified name
- `PersistentSettings.FromPlugInId(id)` and `PlugIn.Find(id).CommandSettings("<Command>")` reach a plugin's tree and one command's block
- Settings pages rewrite whole classes on hide (Modeling Aids), tables on edit (Keyboard) or hide (Aliases), and modes on leave (Display Modes)
- Settings windows close before a write stage, a page left open reverts the writes it snapshots

## [03]-[CLASSES]

- `ApplicationSettings` members read one by name, a reflection walk over its statics crashes Rhino
- `ThicknessAnalysisSettings` state reads and the `SmartTrackSettings` integer getters crash Rhino, `Options/SmartTrack` keys read the integers
- `GetCurrentState()` misreports `OpenGLSettings.AntialiasLevel` and `ChooseOneObjectSettings.HighlightColor`, the static members read them right
- `AppearanceSettings.ShowStatusBar` reads `False` while the bar draws, the key `Options/Appearance/ShowStatusbar` holds it
- `FileSettings.AutoSaveEnabled` reads back `False` after a `True` write, macOS autosave is `Options/FileSettings/AutoSaveVersionsEnabled`
- `AppearanceSettings.CommandPromptFontSize` and key `CommandPromptFontHeight` hold tenths of a point, 110 is 11 pt
- `HostUtils.ExecuteNamedCallback("Rhino.UI.Internal.DockBars.CommandLine.GetPresentationStyle", args)` reads the prompt location as `mode`
- `Options/Advanced/DarkMode` changes nothing on macOS, chrome follows the system appearance
- Grid and appearance color writes drop alpha, `FromArgb(153, r, g, b)` reads back `A=255`
- `AllowUnadornedShortcuts` True runs a bound macro on a bare letter typed at an empty prompt, which breaks one-letter aliases

## [04]-[ALIASES_AND_SHORTCUTS]

- `CommandAliasList.Update(List[CommandAlias]([CommandAlias(name, macro, instant=False), ...]), replaceAll=True)` converges the whole alias set
- `CommandAliasList.Add` refuses a name that is a command name and deletes nothing, `GetDefaults()` lists the factory set the write overlays
- Alias files hold one `alias macro` line split at the first space, `-_Options _Aliases _Export <path>` writes that form
- `Command.IsCommand(name)` False on every alias name keeps aliases from shadowing commands
- `ShortcutKeySettings.SetMacro(ShortcutKey.<Key>, macro)` writes one binding, `Update(..., replaceAll=True)` drops bindings `GetShortcuts()` omits
- `ShortcutKey` names are Windows forms, `Ctrl` is Cmd on macOS, `SetMacro(KeyboardKey, ModifierKey, macro)` reaches Ctrl+Cmd chords
- `IsAcceptableKeyCombo` refuses bare letters and digits, Shift-only letters, Escape, and Option with E, I, N, R, or U
- Cmd+PageDown bindings store as Cmd+' from a keycode collision, PageUp bindings hold

## [05]-[PANELS_AND_LAYOUT]

- `Panels.OpenPanel(panelId, True)` opens and selects a panel, `OpenPanel(dockBarId, panelId, makeSelectedPanel=False)` appends it to a container
- `Panels.PanelDockBar(panelId)` names the container, `Guid.Empty` for a closed panel, `GetOpenPanelIds()` returns an empty list
- `Panels.ClosePanel` closes a floating panel and leaves a docked tab in place, a docked tab closes through `containers.xml` after a quit
- `Rhino.UI.PanelIds` lacks Named Views, Layouts, Block Definitions, Snapshots, Layer States, and Named Positions, those take their literal guids
- Panels with no `last_collection_panel_was_in` row open floating, and a floating bar a probe created lands in `containers.xml` at the next quit
- Container tab order is the row order under `<tabs>`, a band size sits twice, in `placement@dock_band_size` and the `dock_site` band `size`
- `default.rui` rewrites only after an in-app toolbar edit, deleting it makes Rhino write the factory file at launch
- Toolbar files hold no tab membership or button API, `RhinoApp.ToolbarFiles` reads groups and toolbars by name, buttons are XML
- `TabPanelSettings.ToolBarImageSize` and `TabIconSize` cache at load and show after a relaunch, padding and cascade keys apply at the next layout
- `LayersPanel` column lists read once at panel construction and write at panel close, `LayoutsPanel/Width` holds 4 widths and ignores any other count
- `Reset` command's toolbar reset deletes every file in `settings/Scheme__Default/`, `containers.xml` included

## [06]-[TEMPLATE]

- `RhinoDoc.CreateHeadless(<template>)`, table edits, `doc.WriteFile(path, FileWriteOptions())`, and `Dispose()` in one call rewrite a template
- `File3dm.Read(path)`, `view.Maximized`, and `file.Write(path, File3dmWriteOptions())` set the maximized view, a headless write reads `False`
- `File3dm` creates no render content and holds no section style table, those write through the headless document
- New-document choosers list `Template Files/` beside `Default.3dm` before the bundle's templates, a second template needs no `TemplateFolder` change
- `doc.GetGridDefaults()` and `SetGridDefaults(defaults)` hold the grid new viewports inherit, each view's `ConstructionPlane` holds its own spacing
- `doc.DimStyles.BuiltInStyles` names carry the `Template ` prefix, `style.CopyFrom(<built-in>)` then `Modify(style, style.Id, True)` commits
- Nudge steps are one application value in the active document's model units, steps set for Feet move millimeters in a millimeter document

## [07]-[COLORS]

- Appearance colors write through `AppearanceSettings` members, `GridThinLineColor` and `GridThickLineColor` draw one device pixel wide at full color
- Widget and direction arrow colors write through `AppearanceSettings.SetWidgetColor` and `Options/Appearance/DirectionArrowColor<U|V|W>` keys
- Interface chrome keys sit under `UI/ThemeSettings` (`Frame.*`, `Content.*`), a macOS appearance change clears every key back to system colors
- Native controls (Properties text fields, buttons, dropdowns) and the Layers header read no theme key
- `BlackWhiteSwitching` True turns pure black curves white below 30% background brightness and pure white curves black above 70%

## [08]-[FONTS]

- `Options/Appearance/CommandPromptFontName` names the prompt and history font, `AppearanceSettings.UpdateFromState(state)` writes it
- Rhino panels draw in the macOS system font with no size or face key
- `Resources/Fonts/SansSerif.txt` and `Monospace.txt` under the Grasshopper 2 support folder read once at startup, edited with Rhino closed
- Each file holds one family cascade, the hidden system face cannot head one, and a named installed family (Geist, Geist Mono) resolves
- `Grasshopper2.Folders.ResourceFolder(ResourceFolder.Fonts)` names the folder, a `Resources/version` change rewrites every file in it
- `StandardFonts.Sans(FontSize.Normal).ControlObject.FontName` proves the resolved face, both import from `Eto.Drawing` after `import Grasshopper2`

## [09]-[GRASSHOPPER_2]

- `Grasshopper2.Settings.<Name>.Value = v` writes a setting and `.Value` reads it back, usable after `PlugIn.LoadPlugIn` without the editor
- `Settings.SandBox` True holds every write in memory and the next write after it ends saves them all, `RevertToDefault` resets nothing
- `SkinServer.Import(path, True)` copies a `.ghskin` under its file name, `Settings.CanvasSkin.Value = "<name>"` and `DarkMode.Value = True` apply it
- Unbound skin keys fall back to the built-in skin's values, a skin binds every key whose meaning changes
- `SettingsFolder("ComponentTabs").SetText("<name>.rules", text)` writes a ribbon rule set
- `SettingsFolder("ComponentTabs").GetSettings("Control").Set("CurrentRuleSet", "<name>.rules")` then `TrySaveSettingsToFile()` activates it
- Rule sets named without `.rules` save as `.txt` and vanish from the lists, `Cite Default` first keeps the stock layout under later rules
- `Grasshopper2.UI.TabbedPanel.Layout.<Constant>` setters write `ribbon.ghs` and relay out the ribbon live, none clamps
- `Grasshopper2.Display.Defaults.UserDefault = Guises(standard, selected)` sets preview guises, `Guise.WithColour` tints facets off scale
- `SnappingSettings.Current = SnappingSettings.Current.WithFeedback(drawFeedback=True, colour=...)` writes every snapping key at once
- `PlugIn.Find(PlugIn.IdFromName("Grasshopper2")).CommandSettings("GH2")` holds `ShowBanner`, `ShowEditor`, and `LoadLevel`
- `Settings.UserDays.Value` only grows, 15 or more keeps object panel labels at the small font

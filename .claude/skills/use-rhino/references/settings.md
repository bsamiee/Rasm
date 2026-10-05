# [SETTINGS]

Application settings are one state every document, session, and the user share, reached through their owning `Rhino.ApplicationSettings` class, a `PersistentSettings` key, a plugin's settings tree, or a Grasshopper 2 `Settings` member.

[OWNERS]: Each interface module under `tools/interface/rhino/` declares one family and names its owner and target:
- `script/options.py`: `ApplicationSettings` members, `Options/*` and internal tab and toolbar keys, the command prompt font, macOS defaults
- `script/appearance.py`: every appearance, theme, widget, and analysis color as a role of `tools/interface/roles.py`
- `script/keyboard.py` with `script/aliases.txt`: the whole alias set and each bound shortcut key, written one key at a time
- `script/containers.py` with `window.py`: content panel keys, Rendering panel sections, and the window layout
- `script/plugins.py`, `script/display.py`: load modes and plugin keys, display modes
- `script/template.py`: `Template Files/Default.3dm` and `Metric.3dm` per unit system, and `FileSettings.TemplateFile`
- `script/grasshopper.py` with `script/Dark.ghskin`, `components.rules`, `functions.rules`: every Grasshopper 2 store
- `stores.py`: settings XML children, plugin settings files, and `UI/default.rui` ribbon tab order and package buttons, edited once Rhino quit

## [01]-[READ]

Owner, store, and live value of a setting come from the interface, the research tree, and one call:
1. `rg -n '<member or key>' tools/interface/rhino` names the module that declares it, its row form, and its target
2. `rg -n '<member or key>' docs/research/applications/rhino/facts` names its key, owner, factory value, and write path
3. `rhinocommon-crashes.txt` lists members that end the process, a class it names reads through its stored keys
4. One `run_python` call reads the member by name, or the key's stored text, runtime type, and registered default

```python
# Value, stored type, and registered default of one Options key
import clr
from Rhino import PersistentSettings
from System import String

child = PersistentSettings.RhinoAppSettings.GetChild("Options").GetChild("<Group>")
text = clr.GetClrType(String)
print(child.TryGetString("<Key>"), child.GetSettingType("<Key>"), child.TryGetDefault.Overloads[text, text.MakeByRefType()]("<Key>"))
```

- `TryGet<Type>` and one-argument getters read without writing, `Get<Type>(key, default)` writes `default` as the key's default and as its empty value
- `GetSettingType(key)` answers the type of the last typed call on the key in the process, the store holding text alone
- `PersistentSettings.FromPlugInId(<id>)` opens a plugin's tree and `PlugIn.Find(<id>).CommandSettings("<Command>")` one command's block
- Internal owners read through `System.Type.GetType("<Namespace>.<Type>, <Assembly>", throwOnError=True).GetProperty("<Name>").GetValue(None)`
- `Grasshopper2.Settings.<Name>.Value` reads after `import Grasshopper2`

## [02]-[TRIAL]

Trials read the held value, write, read back, and restore the held value in the `finally` of the same call, here with trial channels `red`, `green`, and `blue`:

```python
# Trial of one appearance color, drawn and restored in one call
from Rhino.ApplicationSettings import AppearanceSettings
from System.Drawing import Color
import document

held = AppearanceSettings.ViewportBackgroundColor
try:
    AppearanceSettings.ViewportBackgroundColor = Color.FromArgb(red, green, blue)
    print(held, AppearanceSettings.ViewportBackgroundColor, document.capture(__rhino_doc__, "<name>", zoom=None))
finally:
    AppearanceSettings.ViewportBackgroundColor = held
```

Each owner takes its own write and restore inside the same `try`:

| [INDEX] | [OWNER]                        | [WRITE]                                                     | [RESTORE]                           |
| :-----: | :----------------------------- | :---------------------------------------------------------- | :---------------------------------- |
|  [01]   | `ApplicationSettings` member   | `<Class>.<Member> = value`, live at once                    | Held value                          |
|  [02]   | Key with a native owner        | `child.Set<Type>(key, value)`, then a flush                 | Held value, then a flush            |
|  [03]   | Key with no owner              | `child.Set<Type>(key, value)`                               | Held value or `DeleteItem(key)`     |
|  [04]   | Internal owner                 | `SetMethod.CreateDelegate(Action[T])` called with the value | Delegate called with the held value |
|  [05]   | Plugin or command setting      | `Set<Type>` on its tree, the plugin loaded                  | Held value                          |
|  [06]   | Grasshopper 2 `Settings` entry | `Settings.<Name>.Value = value`, the `.ghs` written at once | Held value                          |

- `PlugIn.FlushSettingsSavedQueue()` is the flush, saving every changed key and reloading each native owner before it returns
- `document.capture` inside the call shows a viewport change, a chrome or panel change shows in a window capture only after the call returns
- Trials start while `RhinoEtoApp.ApplicationPreferencesWindowForPage(None)` reads `None`, no Settings window holding a page snapshot over them

## [03]-[DECLARED_STATE]

Lasting changes edit the declaring module's target and converge it, live rows in the running Rhino and file rows through the apply.

Live rows of one module converge in the running Rhino without a quit:

```python
# Declared rows of one interface module against the live values
# env: <repository>/tools
import sys

for name in [name for name in sys.modules if name.partition(".")[0] == "interface"]:
    del sys.modules[name]
from interface.report import changes
from interface.rhino.script import keyboard
from interface.rhino.script.accessors import plain

for row in keyboard.rows():
    print(*changes(row.label, plain(row.read()), plain(row.target)), sep="\n")
```

- `changes` lines name each row off its target as label, held value, and target, no line means the store holds the declaration
- `converged(row, plain)` in place of `changes` writes each differing row, a `PlugIn.FlushSettingsSavedQueue()` in a `finally` saves the keys
- Each module's `rows` takes the arguments `main` in `script/__init__.py` passes it, a `str` it yields being a report line printed as is
- `containers.rows` restores the window layout as it yields, the layout converges through the apply

File rows (`stores.py` children and toolbars) converge through `nx run rasm:interface -- rhino`:
1. `documents()` shows every untitled document as the task's own, the apply discarding untitled documents at its quit
2. `save(doc)` saves each titled document with `modified` reading `True`, the apply fails on one holding unsaved edits
3. `nx run rasm:interface -- rhino` quits every Rhino, converges the live rows in a fresh Rhino, quits it, edits the files, and reopens titled files
4. Its JSON outcome lists each change as label, held value, and target, a rerun that lists none shows every write held

Use `setup.md` for a one-off file edit between quit and relaunch.

## [04]-[ALIASES_AND_SHORTCUTS]

- `CommandAliasList.GetMacro("<name>")` reads an alias, `None` for none, `ToDictionary()` the set and `GetDefaults()` the factory set
- `Command.IsCommand("<name>")` reads `False` for every alias name a trial or `aliases.txt` adds
- Trial aliases take `Add(name, macro)`, a `command(doc, "<name>", "<A::B>")` run, and `Delete(name)` or `SetMacro(name, held)` in the `finally`
- Instant flags take `Update(List[CommandAlias]([CommandAlias(name, macro, instant), ...]), replaceAll=True)` with the held rows in the `finally`
- `aliases.txt` holds one `name macro` line per alias and one `name` line per instant alias
- `ShortcutKeySettings.GetMacro(ShortcutKey.<Key>)` reads a binding, `None` unbound, `GetShortcuts()` every bound row
- `ShortcutKeySettings.IsAcceptableKeyCombo(KeyboardKey.<Key>, ModifierKey.<Modifier>)` reads True before a trial `SetMacro`
- `ModifierKey.MacCommand` and the `Ctrl` of `ShortcutKey` names stand for Cmd
- `KeyboardKey` and `ModifierKey` import from `Rhino.UI`, `ShortcutKey` and `ShortcutKeySettings` from `Rhino.ApplicationSettings`
- Trial bindings take `SetMacro(key, macro)` and `SetMacro(key, held or "")` in the `finally`, `""` unbinding

## [05]-[PANELS_AND_TOOLBARS]

Panel calls act on `RhinoDoc.ActiveDoc`'s window, a trial's call reads `RhinoDoc.ActiveDoc == doc` for its own document first:
- `window.PanelId` and `Rhino.UI.PanelIds` hold panel ids, `Panels.PanelDockBar(<id>)` names the holding container, `Guid.Empty` closed
- `Panels.OpenPanel(<container>, <id>, makeSelectedPanel=False)` appends a tab, `Panels.PanelDockBars(<id>)` reads every holder
- `HostUtils.ExecuteNamedCallback("Rhino.UI.Internal.NamedCallbacks.RhinoUiCloseDockbarTab", args)` removes the trial tab in the `finally`
- `containers.called(doc, "<callback>", "documentSerialNumber", factoryId=<id>)` runs a callback with the document's serial and answers its `result`
- `window.Container` and `window.RETURNS` declare each container's tabs and each panel's return container, the apply restores and keeps them
- `RhinoApp.ToolbarFiles` reads toolbar groups and toolbars by name
- Tab icon and toolbar image size trials show after a relaunch, internal `ToolbarSettings.Instance.Buttons` trials at the next layout

## [06]-[TEMPLATE]

- `FileSettings.TemplateFile` names the file new documents open from, `file3dm.py <file>` reads its tables without Rhino
- Template trials edit a copy of the file in one call, here a layer with channels `red`, `green`, and `blue`:

```python
# Edit a template copy through a headless document
from Rhino import FileIO, RhinoDoc
from System.Drawing import Color

doc = RhinoDoc.CreateHeadless("</abs/copy.3dm>")
try:
    doc.Layers.Add("<name>", Color.FromArgb(red, green, blue))
    print(doc.WriteFile("</abs/copy.3dm>", FileIO.FileWriteOptions()))
finally:
    doc.Dispose()
```

- `open -g -b com.mcneel.rhinoceros.9 <copy>` opens the edited copy for a capture

## [07]-[COLORS_AND_FONTS]

- `UI/ThemeSettings` keys take a trial through `PersistentSettings.RhinoAppSettings.GetChild("UI").GetChild("ThemeSettings").SetColor`
- Command prompt font takes `AppearanceSettings.UpdateFromState(state)` with `CommandPromptFontName` set on `GetCurrentState()`
- Grasshopper 2 font cascades take `Settings.TypefaceSans`, `TypefaceMono`, `TypefaceSerif`, and `TypefaceScript` values, each applied live

## [08]-[GRASSHOPPER_2]

- `SkinServer.Load("<name>")[0].ToText()` reads a stored skin, `Settings.CanvasSkin.Value` and `Settings.DarkMode.Value` the active one
- `SettingsFolder("ComponentTabs").GetText("<name>.rules")` reads a rule set, `GetSettings("Control")` the active `CurrentRuleSet`
- `Grasshopper2.UI.TabbedPanel.Layout.<Constant>` trials relay out the ribbon live and save `ribbon.ghs` 1 second after the call
- `Defaults.UserDefault` and `SnappingSettings.Current` trials assign the held object back in the `finally`

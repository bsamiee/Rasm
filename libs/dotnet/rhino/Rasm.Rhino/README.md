# [RASM_RHINO]

`Rasm.Rhino` holds RhinoCommon as typed effects, with plug-ins declared as data. The package carries `Rasm.Rhino.Analyzers` under `analyzers/dotnet/cs`: the generator emitting each plug-in's `.rhp` shell and the rules holding code to RhinoCommon, Eto, and Rasm.Rhino facts.

## [01]-[ANALYZERS]

- NuGet passes `analyzers/` assets to every project referencing `Rasm.Rhino`, directly or through a library depending on it
- `buildTransitive/Rasm.Rhino.globalconfig` states the Rhino and Grasshopper values of `Rasm.Analyzers` options (`RASM1203.reporters`, `RASM3003.exempt`, `RASM3101.exempt`, `RASM3102.sentinels`)
- Rules naming Rasm.Rhino members run where `Rasm.Rhino` is referenced: RASM2003, 2006, 2009, 2010, 2012, 2102–2105, 2203, 2204
- Shell generator binds `PlugInShell` entry methods by name: tuple elements to host virtuals, later parameters to host members, CS8785 on a miss

## [02]-[RULES]

Each category holds one block of IDs.

### [02.1]-[RHINOCOMMON]

Category `RhinoCommon`:

| [INDEX] | [ID]                          | [TITLE]                                                              |
| :-----: | :---------------------------- | :------------------------------------------------------------------- |
|  [01]   | <a id="RASM2002"></a>RASM2002 | Unchecked item count, compare with input count                       |
|  [02]   | <a id="RASM2003"></a>RASM2003 | `RhinoDoc` read off UI thread, read through `OnUiThread()`           |
|  [03]   | <a id="RASM2004"></a>RASM2004 | `ActiveDoc` read with document in scope, read the held document      |
|  [04]   | <a id="RASM2005"></a>RASM2005 | `RunScript` in command without `ScriptRunner` style                  |
|  [05]   | <a id="RASM2006"></a>RASM2006 | Single call marshaled to UI thread, compose one `OnUiThread()` `IO`  |
|  [06]   | <a id="RASM2007"></a>RASM2007 | `SetDisplayModeOverride` result read, discard through `_ =`          |
|  [07]   | <a id="RASM2008"></a>RASM2008 | macOS-breaking member outside macOS guard, guard or replace the call |
|  [08]   | <a id="RASM2009"></a>RASM2009 | `GrayscaleDib` result read without null check                        |
|  [09]   | <a id="RASM2010"></a>RASM2010 | Page view built in `PageControl`, realize in `OnActivate(true)`      |
|  [10]   | <a id="RASM2011"></a>RASM2011 | Floating view docked by assignment, delete the assignment            |
|  [11]   | <a id="RASM2012"></a>RASM2012 | Owned `DisplayModeDescription` left undisposed                       |
|  [12]   | <a id="RASM2014"></a>RASM2014 | Host object round-tripped through text, hold the object              |
|  [13]   | <a id="RASM2101"></a>RASM2101 | `PersistentSettings` read through a default, call `TryGet<Kind>`     |
|  [14]   | <a id="RASM2102"></a>RASM2102 | View redraw outside `Commits`, pass a `RedrawPolicy`                 |
|  [15]   | <a id="RASM2103"></a>RASM2103 | Cast or type test on `GeometryBase` duplicate                        |
|  [16]   | <a id="RASM2104"></a>RASM2104 | Conduit toggled outside `Conduits`, toggle through `Conduits`        |
|  [17]   | <a id="RASM2105"></a>RASM2105 | `RenderSettings` written outside `RenderSettingsSources`             |

### [02.2]-[ETO]

Category `Eto`:

| [INDEX] | [ID]                          | [TITLE]                                                                   |
| :-----: | :---------------------------- | :------------------------------------------------------------------------ |
|  [01]   | <a id="RASM2201"></a>RASM2201 | Cached system font disposed, read without release                         |
|  [02]   | <a id="RASM2202"></a>RASM2202 | Eto container acquired by plain dispose, detach children before `Dispose` |
|  [03]   | <a id="RASM2203"></a>RASM2203 | Retained control transferred, pass it without `Transfer`                  |
|  [04]   | <a id="RASM2204"></a>RASM2204 | Native callback or Eto override reading host outside `Session`            |
|  [05]   | <a id="RASM2205"></a>RASM2205 | Delegate registered as platform handler, declare a handler interface      |

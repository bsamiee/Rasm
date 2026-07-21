# [RASM_GRASSHOPPER_API_RHINO_UI]

`Rhino.UI` bridges Eto controls into Rhino chrome through one static `EtoExtensions` surface: `UseRhinoStyle` stamps native styling onto an Eto `Control`, the window family binds a form to a `RhinoDoc` and persists its screen position, and the conversion family carries `System.Drawing` GDI and Rhino `Color4f` values into `Eto.Drawing` — the same GDI-to-Eto bridge the GH1 interop icon boundary rides. Every member is catalog-verified against the installed RhinoWIP `Rhino.UI.dll`.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: host assembly `Rhino.UI`
- package: `Rhino.UI` (Rhino 9 WIP host UI bridge; not a NuGet pin — the in-process `Rhino.UI.dll` shipped in the Rhino application bundle is the resolved asset)
- assembly: `Rhino.UI`
- namespace: `Rhino.UI` (`EtoExtensions`)
- asset: host assembly; `/Applications/RhinoWIP.app/Contents/Frameworks/RhCore.framework/Versions/Current/Resources/Rhino.UI.dll` resolved from the installed RhinoWIP bundle
- rail: host-rhino

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the EtoExtensions bridge
- namespace: `Rhino.UI`
- rail: host-rhino

`EtoExtensions` is a static extension surface over Eto `Control`/`Window`/`Form`/`Dialog<T>`; every member extends a managed Eto type with Rhino styling, document binding, position persistence, or a value conversion.

| [INDEX] | [SYMBOL]        | [KIND] | [CAPABILITY]                                     |
| :-----: | :-------------- | :----- | :----------------------------------------------- |
|  [01]   | `EtoExtensions` | static | Rhino styling, doc binding, and value conversion |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: Rhino styling
- namespace: `Rhino.UI`
- rail: host-rhino

`UseRhinoStyle` is the styling seam behind `Eto/windows.md`'s `ChromeRow.Rhino` and `Shell/session.md`'s `SessionOp.StyleCase`; it stamps native chrome onto an Eto control in place.

| [INDEX] | [SURFACE]                    | [CALL_SHAPE]              | [CAPABILITY]                  |
| :-----: | :--------------------------- | :----------------------- | :---------------------------- |
|  [01]   | `Control.UseRhinoStyle`      | `(this Control)` → `void` | apply Rhino chrome to an Eto control |

[ENTRYPOINT_SCOPE]: Rhino-document window binding
- namespace: `Rhino.UI`
- rail: host-rhino

Form and dialog members bind a window to a `RhinoDoc`, resolve the owning document back from a form, and persist a window's screen position keyed by a caller `Type`.

| [INDEX] | [SURFACE]                          | [CALL_SHAPE]                                   | [CAPABILITY]                     |
| :-----: | :--------------------------------- | :--------------------------------------------- | :------------------------------- |
|  [01]   | `Form.Show`                        | `(this Form, RhinoDoc)`                         | show a form bound to a document  |
|  [02]   | `Form.GetRhinoDoc`                 | `(this Form)` → `RhinoDoc`                      | owning document lookup           |
|  [03]   | `Dialog<T>.ShowSemiModal`          | `(this Dialog<T>, RhinoDoc, Control)` → `T`     | semi-modal typed dialog          |
|  [04]   | `WindowsFromDocument<T>`           | `(RhinoDoc)` → `IEnumerable<T>`                 | document-scoped window roster    |
|  [05]   | `Window.LocalizeAndRestore`        | `(this Window, Type)`                           | localize and restore layout      |
|  [06]   | `Window.SavePosition` / `RestorePosition` | `(this Window, Type)`                    | persist and restore screen position |
|  [07]   | `Panel.PushPickButton` / `Window.PushPickButton` | `(this Panel/Window, EventHandler<EventArgs>)` | pick-button attachment           |

[ENTRYPOINT_SCOPE]: GDI and Rhino value conversion
- namespace: `Rhino.UI`
- rail: host-rhino

Conversion members carry `System.Drawing` GDI carriers and Rhino `Color4f`/`Font` values across into `Eto.Drawing`; `ToEto(System.Drawing.Bitmap)` is the same GDI-to-Eto bridge the GH1 interop icon seam rides, and the reverse `ToSystemDrawing` forms return to GDI at a native handoff.

| [INDEX] | [SURFACE]                     | [CALL_SHAPE]                                            | [CAPABILITY]                     |
| :-----: | :---------------------------- | :----------------------------------------------------- | :------------------------------- |
|  [01]   | `System.Drawing.Bitmap.ToEto` | `(this Bitmap[, bool])` → `Eto.Drawing.Bitmap`/`Image`  | GDI bitmap into Eto              |
|  [02]   | `System.Drawing.Icon.ToEto`   | `(this Icon, bool)` → `Eto.Drawing.Icon`               | GDI icon into Eto                |
|  [03]   | `System.Drawing.Color.ToEto`  | `(this Color)` → `Eto.Drawing.Color`                   | GDI colour into Eto              |
|  [04]   | `Eto.Drawing.Color.ToSystemDrawing` | `(this Color)` → `System.Drawing.Color`          | Eto colour back to GDI           |
|  [05]   | `Color4f.ToEto` / `Color.ToColor4f` | `(this Color4f)`/`(this Color)`                  | Rhino float colour bridge        |
|  [06]   | `Rhino.DocObjects.Font.ToEto` | `(this Font, float, FontDecoration)` → `Eto.Drawing.Font` | Rhino font into Eto           |
|  [07]   | `Point.ToEtoScreen` / `Rectangle.ToEtoScreen` | `(this Point/Rectangle, Screen)`       | GDI screen geometry into Eto     |

## [04]-[IMPLEMENTATION_LAW]

[RHINO_UI_TOPOLOGY]:
- `UseRhinoStyle` and folder-owned `StyleRow` cosmetics are two seams: a Rhino-styled surface routes through `SessionOp.StyleCase`, a `StyleRow` scopes folder cosmetics by tag, and a window wears both when a Rhino-styled surface also joins a folder style scope; neither seam re-implements the other
- window binding pairs a form with a `RhinoDoc` at `Show(Form, RhinoDoc)` and resolves the document back through `GetRhinoDoc`; position persistence keys on a caller `Type` so a window restores its own screen slot across sessions
- conversion members are the one bridge for GDI and Rhino colour, font, and image carriers into `Eto.Drawing`; `ToEto(System.Drawing.Bitmap)` is the exact GDI-to-Eto path the GH1 interop icon boundary composes

[STACKING]:
- `api-languageext`(`libs/csharp/.api/api-languageext.md`): `UseRhinoStyle` and `Show(Form, RhinoDoc)` are side-effecting host calls lowered onto `Op.Side`/`Eff`; `GetRhinoDoc(Form)` null-gates through `Optional(...)` into `Option<RhinoDoc>`; a `WindowsFromDocument<T>` roster carries as a `Seq<T>`; a `RestorePosition` `bool` return folds to `Fin<Unit>`
- `api-thinktecture-runtime-extensions`(`libs/csharp/.api/api-thinktecture-runtime-extensions.md`): a folder style scope is a `[SmartEnum]`/tag vocabulary the `StyleRow` reads, and the `FontDecoration` host enum projects onto a `[SmartEnum<TKey>]` owner where the folder attaches a decoration behaviour to a case

[LOCAL_ADMISSION]:
- styling enters through `UseRhinoStyle` at the `SessionOp.StyleCase` seam; a parallel hand-rolled Rhino-chrome stamp is the deleted form
- GDI and Rhino carriers cross into Eto only through the `ToEto` conversion family; the GH1 interop icon bridge and any Rhino colour handoff reuse these members rather than re-deriving a conversion
- Rhino-document window binding is `Show`/`GetRhinoDoc`; a folder owner never re-derives the form-to-document association

[RAIL_LAW]:
- Package: `Rhino.UI` (Eto and Rhino UI bridge)
- Owns: `UseRhinoStyle` native styling, Rhino-document window show and lookup, window position persistence, and the Eto ↔ `System.Drawing` ↔ Rhino colour, font, and image conversions
- Accept: applying Rhino chrome to an Eto control, binding a form to a `RhinoDoc`, converting GDI and Rhino carriers into `Eto.Drawing`
- Reject: Eto handler resolution and native embedding (`api-eto-platform`), the AppKit native seam (`api-macos-native`), GH2 chrome hosts (`api-gh2-editor`)

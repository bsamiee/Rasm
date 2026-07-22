# [RASM_GRASSHOPPER_API_RHINO_UI]

`Rhino.UI` bridges Eto controls into Rhino chrome through the static `EtoExtensions` extension surface. Eto handler resolution and the AppKit native embedding seam stay outside this bridge.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: host assembly `Rhino.UI`
- package: `Rhino.UI` (Rhino 9 WIP host UI bridge)
- assembly: `Rhino.UI` (in-process host DLL from the RhinoWIP `RhCore.framework` bundle)
- namespace: `Rhino.UI` (`EtoExtensions`)
- rail: host-rhino

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the static extension surface

| [INDEX] | [SYMBOL]        | [TYPE_FAMILY] | [CAPABILITY]                                     |
| :-----: | :-------------- | :------------ | :----------------------------------------------- |
|  [01]   | `EtoExtensions` | static class  | Rhino styling, doc binding, and value conversion |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: Rhino styling

`UseRhinoStyle` stamps native chrome onto an Eto control in place.

| [INDEX] | [SURFACE]                 | [SHAPE]   | [CAPABILITY]                    |
| :-----: | :------------------------ | :-------- | :------------------------------ |
|  [01]   | `Control.UseRhinoStyle()` | extension | apply Rhino chrome to a control |

[ENTRYPOINT_SCOPE]: Rhino-document window binding

Members bind a form to a `RhinoDoc`, resolve the document back, and persist window position keyed by a caller `Type`.

| [INDEX] | [SURFACE]                                            | [SHAPE]   | [CAPABILITY]                       |
| :-----: | :--------------------------------------------------- | :-------- | :--------------------------------- |
|  [01]   | `Form.Show(RhinoDoc)`                                | extension | show a form bound to a document    |
|  [02]   | `Form.GetRhinoDoc() -> RhinoDoc`                     | extension | owning document lookup             |
|  [03]   | `Dialog<T>.ShowSemiModal(RhinoDoc, Control) -> T`    | extension | semi-modal typed dialog            |
|  [04]   | `Dialog.ShowSemiModal(RhinoDoc, Control)`            | extension | semi-modal untyped dialog          |
|  [05]   | `WindowsFromDocument<T>(RhinoDoc) -> IEnumerable<T>` | static    | document-scoped window roster      |
|  [06]   | `Window.LocalizeAndRestore(Type)`                    | extension | localize and restore layout        |
|  [07]   | `Window.SavePosition(Type)`                          | extension | persist screen position            |
|  [08]   | `Window.RestorePosition(Type) -> bool`               | extension | restore screen position            |
|  [09]   | `Panel.PushPickButton(EventHandler<EventArgs>)`      | extension | pick-button attachment on a panel  |
|  [10]   | `Window.PushPickButton(EventHandler<EventArgs>)`     | extension | pick-button attachment on a window |

[ENTRYPOINT_SCOPE]: GDI and Rhino value conversion

Members carry `System.Drawing` GDI and Rhino `Color4f`/`Font` carriers into `Eto.Drawing`; the reverse `ToSystemDrawing`/`ToSystemDrawingScreen` forms return to GDI at a native handoff.

| [INDEX] | [SURFACE]                                                   | [SHAPE]   | [CAPABILITY]                      |
| :-----: | :---------------------------------------------------------- | :-------- | :-------------------------------- |
|  [01]   | `Bitmap.ToEto() -> Eto.Bitmap`                              | extension | GDI bitmap into Eto               |
|  [02]   | `Bitmap.ToEto(bool) -> Eto.Image`                           | extension | GDI bitmap into a sized image     |
|  [03]   | `Icon.ToEto(bool) -> Eto.Icon`                              | extension | GDI icon into Eto                 |
|  [04]   | `Color.ToEto() -> Eto.Color`                                | extension | GDI colour into Eto               |
|  [05]   | `Eto.Color.ToSystemDrawing() -> Color`                      | extension | Eto colour back to GDI            |
|  [06]   | `Color4f.ToEto() -> Eto.Color`                              | extension | Rhino float colour into Eto       |
|  [07]   | `Eto.Color.ToColor4f() -> Color4f`                          | extension | Eto colour into Rhino float       |
|  [08]   | `Font.ToEto(float, FontDecoration) -> Eto.Font`             | extension | Rhino font into a sized Eto font  |
|  [09]   | `Font.ToEto() -> FontTypeface`                              | extension | Rhino font into an Eto typeface   |
|  [10]   | `ToEto(string, string, float) -> Eto.Font`                  | static    | family/face/size into an Eto font |
|  [11]   | `System.Drawing.Font.ToEto() -> Eto.Font`                   | extension | GDI font into Eto                 |
|  [12]   | `Point.ToEtoScreen(Screen) -> Eto.PointF`                   | extension | GDI screen point into Eto         |
|  [13]   | `Rectangle.ToEtoScreen(Screen) -> Eto.RectangleF`           | extension | GDI screen rect into Eto          |
|  [14]   | `Eto.PointF.ToSystemDrawingScreen(Screen) -> Point`         | extension | Eto point back to GDI screen      |
|  [15]   | `Eto.Point.ToSystemDrawingScreen(Screen) -> Point`          | extension | Eto pixel point back to GDI       |
|  [16]   | `Eto.RectangleF.ToSystemDrawingScreen(Screen) -> Rectangle` | extension | Eto rect back to GDI screen       |
|  [17]   | `Eto.Rectangle.ToSystemDrawingScreen(Screen) -> Rectangle`  | extension | Eto pixel rect back to GDI        |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `UseRhinoStyle` and folder-owned `StyleRow` cosmetics are two non-overlapping seams: a Rhino-styled surface routes through `SessionOp.StyleCase`, a `StyleRow` scopes folder cosmetics by tag, and a window wears both when a Rhino-styled surface also joins a folder style scope
- window binding pairs a form with a `RhinoDoc` at `Show` and resolves it back through `GetRhinoDoc`; position persistence keys on a caller `Type`, so a window restores its own screen slot across sessions
- conversion members are the one bridge for GDI and Rhino colour, font, and image carriers into `Eto.Drawing`, and `Bitmap.ToEto` is the exact path the GH1 interop icon boundary composes

[STACKING]:
- `api-languageext`(`libs/csharp/.api/api-languageext.md`): `UseRhinoStyle` and `Show` lower onto `Op.Side`/`Eff` as side-effecting host calls; `GetRhinoDoc` null-gates through `Optional(...)` into `Option<RhinoDoc>`; a `WindowsFromDocument<T>` roster carries as `Seq<T>`; `RestorePosition`'s `bool` folds to `Fin<Unit>`
- `api-thinktecture-runtime-extensions`(`libs/csharp/.api/api-thinktecture-runtime-extensions.md`): a folder style scope is a `[SmartEnum]` tag vocabulary the `StyleRow` reads, and the `FontDecoration` host enum projects onto a `[SmartEnum<TKey>]` where a case attaches decoration behaviour

[LOCAL_ADMISSION]:
- native styling enters through `UseRhinoStyle` at the `SessionOp.StyleCase` seam
- GDI and Rhino carriers cross into `Eto.Drawing` only through the `ToEto`/`ToSystemDrawing` conversion family
- form-to-document binding is `Show`/`GetRhinoDoc`; a folder owner composes these rather than re-deriving the association

[RAIL_LAW]:
- Package: `Rhino.UI`
- Owns: `UseRhinoStyle` native styling, `RhinoDoc` window show and lookup, window position persistence, and the `System.Drawing`/Rhino `Color4f`/`Font` conversions into `Eto.Drawing`
- Accept: Rhino chrome onto an Eto control, a form bound to a `RhinoDoc`, GDI and Rhino carriers converted into `Eto.Drawing`
- Reject: a hand-rolled Rhino-chrome stamp, Eto handler resolution and native embedding (`api-eto-platform`), the AppKit native seam (`api-macos-native`), GH2 chrome hosts (`api-gh2-editor`)

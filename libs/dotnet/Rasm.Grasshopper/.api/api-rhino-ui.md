# [RASM_GRASSHOPPER_API_RHINO_UI]

`Rhino.UI` carries GDI, Rhino `Color4f`, and Rhino `Font` values into `Eto.Drawing` through the static `EtoExtensions` conversion family, and attaches the Rhino pick button to a panel or window. Native styling, document window binding, position persistence, and the native value prompts are the branch host-bridge surface this partition registers.

## [01]-[PUBLIC_TYPES]

- Registers the `Rhino.UI` host-bridge seams (`libs/dotnet/.api/api-rhino-ui.md`): `EtoExtensions.UseRhinoStyle`, `Show`/`GetRhinoDoc` document binding, `ShowSemiModal`, `SavePosition`/`RestorePosition`/`LocalizeAndRestore`, `WindowsFromDocument<T>`, and the `Dialogs` edit and number prompts carry their algebra there; the rows below are the conversion and pick members this boundary adds beyond it.

[PUBLIC_TYPE_SCOPE]: the static extension surface

| [INDEX] | [SYMBOL]        | [TYPE_FAMILY] | [CAPABILITY]                                    |
| :-----: | :-------------- | :------------ | :---------------------------------------------- |
|  [01]   | `EtoExtensions` | static class  | GDI and Rhino value conversion, pick attachment |

## [02]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: GDI and Rhino value conversion

Members carry `System.Drawing` GDI and Rhino `Color4f`/`Font` carriers into `Eto.Drawing`; the reverse `ToSystemDrawing`/`ToSystemDrawingScreen` forms return to GDI at a native handoff.

| [INDEX] | [SURFACE]                                                   | [SHAPE]   | [CAPABILITY]                       |
| :-----: | :---------------------------------------------------------- | :-------- | :--------------------------------- |
|  [01]   | `Bitmap.ToEto() -> Eto.Bitmap`                              | extension | GDI bitmap into Eto                |
|  [02]   | `Bitmap.ToEto(bool) -> Eto.Image`                           | extension | GDI bitmap into a sized image      |
|  [03]   | `Icon.ToEto(bool) -> Eto.Icon`                              | extension | GDI icon into Eto                  |
|  [04]   | `Color.ToEto() -> Eto.Color`                                | extension | GDI colour into Eto                |
|  [05]   | `Eto.Color.ToSystemDrawing() -> Color`                      | extension | Eto colour back to GDI             |
|  [06]   | `Color4f.ToEto() -> Eto.Color`                              | extension | Rhino float colour into Eto        |
|  [07]   | `Eto.Color.ToColor4f() -> Color4f`                          | extension | Eto colour into Rhino float        |
|  [08]   | `Font.ToEto(float, FontDecoration) -> Eto.Font`             | extension | Rhino font into a sized Eto font   |
|  [09]   | `Font.ToEto() -> FontTypeface`                              | extension | Rhino font into an Eto typeface    |
|  [10]   | `ToEto(string, string, float) -> Eto.Font`                  | static    | family, face, and size into a font |
|  [11]   | `System.Drawing.Font.ToEto() -> Eto.Font`                   | extension | GDI font into Eto                  |
|  [12]   | `Point.ToEtoScreen(Screen) -> Eto.PointF`                   | extension | GDI screen point into Eto          |
|  [13]   | `Rectangle.ToEtoScreen(Screen) -> Eto.RectangleF`           | extension | GDI screen rect into Eto           |
|  [14]   | `Eto.PointF.ToSystemDrawingScreen(Screen) -> Point`         | extension | Eto point back to GDI screen       |
|  [15]   | `Eto.Point.ToSystemDrawingScreen(Screen) -> Point`          | extension | Eto pixel point back to GDI        |
|  [16]   | `Eto.RectangleF.ToSystemDrawingScreen(Screen) -> Rectangle` | extension | Eto rect back to GDI screen        |
|  [17]   | `Eto.Rectangle.ToSystemDrawingScreen(Screen) -> Rectangle`  | extension | Eto pixel rect back to GDI         |

[ENTRYPOINT_SCOPE]: pick-button attachment

| [INDEX] | [SURFACE]                                        | [SHAPE]   | [CAPABILITY]                       |
| :-----: | :----------------------------------------------- | :-------- | :--------------------------------- |
|  [01]   | `Panel.PushPickButton(EventHandler<EventArgs>)`  | extension | pick-button attachment on a panel  |
|  [02]   | `Window.PushPickButton(EventHandler<EventArgs>)` | extension | pick-button attachment on a window |

## [03]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Conversion is the one bridge for GDI and Rhino colour, font, and image carriers into `Eto.Drawing`, and `Bitmap.ToEto` is the exact path the GH1 interop icon boundary composes; a hand-rolled channel or point remap beside these members is the deleted form.
- Screen-space conversions take the `Screen` argument, so a point or rectangle crossing between GDI and Eto resolves against the display it belongs to rather than the primary.
- Pick attachment is per host surface: a panel and a window each take the attachment for their own chrome and the handler fires from the host's own pick lifecycle.
- Native styling from the registered bridge and folder-owned cosmetics are two non-overlapping seams: a Rhino-styled surface routes through the folder style case, a folder style row scopes cosmetics by tag, and a window wears both when a Rhino-styled surface also joins a folder style scope.

[STACKING]:
- `api-rhino-ui`(`libs/dotnet/.api/api-rhino-ui.md`): the registered host bridge — native styling, document window binding, semi-modal presentation, position persistence, and the native value prompts.
- `api-eto-drawing`(`libs/dotnet/Rasm.Grasshopper/.api/api-eto-drawing.md`): the target vocabulary every conversion lands in and the icon carrier `Bitmap.ToEto(bool)` and `Icon.ToEto(bool)` produce.
- `api-system-drawing-common`(`libs/dotnet/.api/api-system-drawing-common.md`): the GDI carriers Rhino host members declare, converted once here and never mirrored as a second drawing vocabulary.
- `api-languageext`(`libs/dotnet/.api/api-languageext.md`): a failed decode or an unresolved font face lowers onto `Fin<T>` at the folder boundary, and a pick-button handler attachment carries as a disposable subscription.
- `api-thinktecture-runtime-extensions`(`libs/dotnet/.api/api-thinktecture-runtime-extensions.md`): a folder style scope is a `[SmartEnum]` tag vocabulary the folder style row reads, and the host font-decoration enum projects onto a `[SmartEnum<TKey>]` where a case attaches decoration behaviour.

[LOCAL_ADMISSION]:
- GDI and Rhino carriers cross into `Eto.Drawing` only through the conversion family here; a per-channel colour rebuild or a manual DPI point scale is the deleted form.
- Native styling, document binding, and value prompts take the registered branch bridge; this partition never re-derives them.

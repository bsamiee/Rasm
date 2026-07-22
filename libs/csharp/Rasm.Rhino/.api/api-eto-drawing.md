# [RASM_RHINO_API_ETO_DRAWING]

`Eto.Drawing` is the immediate-mode 2D painting surface behind every `Drawable` host and owner-drawn cell: one `Graphics` command stream strokes, fills, blits, and lays out text against a save/restore transform and clip stack, with no retained scene — a `Drawable` paint event or `CreateGraphics` hands the live handle and the host re-issues the whole paint on invalidation. Canonical geometry and colour project to native-canvas primitives at the render edge, feeding the `paint` rail.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Eto.Drawing`
- package: `Eto.Drawing` (host-provided; bound in-place from the Rhino-loaded `Eto.dll`, never a second NuGet admission)
- license: BSD-3-Clause
- assembly: `Eto.dll` (Rhino `RhCore` framework)
- namespace: `Eto.Drawing`
- asset: the same `Eto.dll` the `Eto.Forms` surface binds; `Graphics` handles render against the host's native canvas
- rail: paint

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: paint host and graphics command surface
- namespace: `Eto.Forms`, `Eto.Drawing`
- rail: paint

`Drawable` hosts custom paint; `Graphics` is the immediate stream it hands to a `Paint` event or returns from `CreateGraphics`.

| [INDEX] | [SYMBOL]                            | [KIND]             | [CAPABILITY]                                                |
| :-----: | :---------------------------------- | :----------------- | :---------------------------------------------------------- |
|  [01]   | `Drawable`                          | host (`Eto.Forms`) | custom-paint control raising `Paint` with a live `Graphics` |
|  [02]   | `Drawable.Paint`                    | event              | supplies the live paint-event `Graphics`                    |
|  [03]   | `Drawable.SupportsCreateGraphics`   | property           | advertises off-event graphics acquisition                   |
|  [04]   | `Drawable.CreateGraphics()`         | member             | acquires an off-event `Graphics` handle                     |
|  [05]   | `Drawable.Update(Rectangle region)` | member             | forces an immediate bounded repaint                         |
|  [06]   | `Drawable.CancelTextComposition()`  | member             | cancels an in-progress IME composition                      |
|  [07]   | `Drawable.CommitTextComposition()`  | member             | commits an in-progress IME composition                      |
|  [08]   | `Graphics`                          | command stream     | immediate 2D draw/fill/text/clip/transform surface          |
|  [09]   | `Graphics.AntiAlias`                | property           | anti-alias toggle for the stream                            |
|  [10]   | `Graphics.ImageInterpolation`       | property           | image resampling quality (`ImageInterpolation` enum)        |
|  [11]   | `Graphics.PixelOffsetMode`          | property           | half-pixel offset policy for crisp lines                    |

[PUBLIC_TYPE_SCOPE]: geometry, pens, and paths
- rail: paint

| [INDEX] | [SYMBOL]                   | [KIND]   | [CAPABILITY]                                                                |
| :-----: | :------------------------- | :------- | :-------------------------------------------------------------------------- |
|  [01]   | `Point` / `PointF`         | value    | integer / float 2D position                                                 |
|  [02]   | `Size` / `SizeF`           | value    | integer / float extent                                                      |
|  [03]   | `Rectangle` / `RectangleF` | value    | axis-aligned box with from-center/inflate/union/intersect/contains families |
|  [04]   | `Pen`                      | stroke   | stroke definition carrying `LineCap`, `LineJoin`, `MiterLimit`, `DashStyle` |
|  [05]   | `PenLineCap`               | enum     | butt/round/square cap selector                                              |
|  [06]   | `PenLineJoin`              | enum     | miter/round/bevel join selector                                             |
|  [07]   | `DashStyle`                | value    | dash pattern over a stroke                                                  |
|  [08]   | `DashStyles`               | statics  | named dash presets (`Solid`, `Dash`, `Dot`, `DashDot`, `DashDotDot`)        |
|  [09]   | `GraphicsPath`             | geometry | retained path with construction, transform, and hit-testing                 |
|  [10]   | `IGraphicsPath`            | contract | path contract consumed by `Graphics.DrawPath`/`FillPath`                    |

[PUBLIC_TYPE_SCOPE]: colour and brushes
- rail: paint

| [INDEX] | [SYMBOL]              | [KIND]  | [CAPABILITY]                                                   |
| :-----: | :-------------------- | :------ | :------------------------------------------------------------- |
|  [01]   | `Color`               | value   | RGBA colour with blend, distance, and space-conversion members |
|  [02]   | `Colors`              | statics | named colour constants                                         |
|  [03]   | `SystemColors`        | statics | host theme colour roster                                       |
|  [04]   | `SolidBrush`          | brush   | flat fill                                                      |
|  [05]   | `LinearGradientBrush` | brush   | two-stop linear gradient fill                                  |
|  [06]   | `RadialGradientBrush` | brush   | radial gradient fill                                           |
|  [07]   | `TextureBrush`        | brush   | image-tiled fill with opacity                                  |
|  [08]   | `GradientWrapMode`    | enum    | gradient edge wrap policy                                      |

[PUBLIC_TYPE_SCOPE]: text, images, and matrices
- rail: paint

| [INDEX] | [SYMBOL]                 | [KIND]      | [CAPABILITY]                                                   |
| :-----: | :----------------------- | :---------- | :------------------------------------------------------------- |
|  [01]   | `Font`                   | text        | typeface + size + style                                        |
|  [02]   | `FontStyle`              | enum        | bold/italic style flags                                        |
|  [03]   | `FontDecoration`         | enum        | underline/strikethrough flags                                  |
|  [04]   | `SystemFonts`            | statics     | host UI font roster                                            |
|  [05]   | `FormattedText`          | text layout | wrap/alignment/trimming rich text block with `Measure`         |
|  [06]   | `FormattedTextWrapMode`  | enum        | `None`/`Word`/`Character` wrap policy                          |
|  [07]   | `FormattedTextAlignment` | enum        | left/right/center/justify alignment                            |
|  [08]   | `FormattedTextTrimming`  | enum        | ellipsis trimming policy                                       |
|  [09]   | `Bitmap`                 | image       | pixel image with lock, per-pixel, byte-array, and clone access |
|  [10]   | `BitmapData`             | lock handle | raw locked-pixel accessor from `Bitmap.Lock`                   |
|  [11]   | `Image`                  | image base  | drawable image base consumed by `Graphics.DrawImage`           |
|  [12]   | `ImageFormat`            | enum        | encode format selector for `ToByteArray`                       |
|  [13]   | `IMatrix`                | transform   | affine transform contract                                      |
|  [14]   | `Matrix`                 | transform   | affine matrix with rotation/scale/inverse composition          |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: graphics draw and fill commands
- rail: paint

A `Pen` strokes, a `Brush` fills; each primitive carries a draw and a fill form selected by the caller's argument.

| [INDEX] | [SURFACE]                                                                    | [CALL_SHAPE] | [CAPABILITY]                     |
| :-----: | :--------------------------------------------------------------------------- | :----------- | :------------------------------- |
|  [01]   | `DrawLine(Pen pen, PointF start, PointF end)`                                | stroke       | one line segment                 |
|  [02]   | `DrawLines(Pen pen, IEnumerable<PointF> points)`                             | stroke       | connected polyline               |
|  [03]   | `DrawRectangle(Pen pen, RectangleF rectangle)`                               | stroke       | rectangle outline                |
|  [04]   | `FillRectangle(Brush brush, RectangleF rectangle)`                           | fill         | filled rectangle                 |
|  [05]   | `DrawEllipse(Pen pen, RectangleF rectangle)`                                 | stroke       | ellipse outline                  |
|  [06]   | `FillEllipse(Brush brush, RectangleF rectangle)`                             | fill         | filled ellipse                   |
|  [07]   | `DrawArc(Pen pen, RectangleF rectangle, float startAngle, float sweepAngle)` | stroke       | arc segment                      |
|  [08]   | `DrawPolygon(Pen pen, params PointF[] points)`                               | stroke       | closed polygon outline           |
|  [09]   | `FillPolygon(Brush brush, params PointF[] points)`                           | fill         | filled polygon                   |
|  [10]   | `DrawPath(Pen pen, IGraphicsPath path)`                                      | stroke       | stroked path                     |
|  [11]   | `FillPath(Brush brush, IGraphicsPath path)`                                  | fill         | filled path                      |
|  [12]   | `DrawImage(Image image, RectangleF source, RectangleF destination)`          | blit         | source-to-destination image draw |

[ENTRYPOINT_SCOPE]: text draw and measure
- rail: paint

`DrawText` carries a pre-laid `FormattedText` form and a frame form with wrap, alignment, and trimming policy; `MeasureString` and `FormattedText.Measure` return the laid-out `SizeF`:

```csharp signature
Graphics.DrawText(FormattedText formattedText, PointF location)
Graphics.DrawText(Font font, Brush brush, RectangleF frame, string text, FormattedTextWrapMode wrap = FormattedTextWrapMode.Word, FormattedTextAlignment alignment = FormattedTextAlignment.Left, FormattedTextTrimming trimming = FormattedTextTrimming.WordEllipsis)
Graphics.MeasureString(Font font, string text)
FormattedText.Measure()
```

[ENTRYPOINT_SCOPE]: clip and transform state
- rail: paint

Transform and clip are a save/restore stack; a `SaveTransformState` push is unwound by the matching restore.

| [INDEX] | [SURFACE]                                          | [CALL_SHAPE]  | [CAPABILITY]                                      |
| :-----: | :------------------------------------------------- | :------------ | :------------------------------------------------ |
|  [01]   | `TranslateTransform(float offsetX, float offsetY)` | transform     | shifts the origin                                 |
|  [02]   | `RotateTransform(float angle)`                     | transform     | rotates the stream                                |
|  [03]   | `ScaleTransform(float scaleX, float scaleY)`       | transform     | scales the stream                                 |
|  [04]   | `MultiplyTransform(IMatrix matrix)`                | transform     | composes an arbitrary affine transform            |
|  [05]   | `SaveTransformState()`                             | state         | pushes transform/clip state onto the stack        |
|  [06]   | `SetClip(RectangleF rectangle)`                    | clip          | rectangular clip region                           |
|  [07]   | `SetClip(IGraphicsPath path)`                      | clip          | arbitrary path clip region                        |
|  [08]   | `ResetClip()`                                      | clip          | clears the clip region                            |
|  [09]   | `IsVisible(RectangleF rectangle)`                  | cull → `bool` | tests a rectangle against the clip for early cull |
|  [10]   | `Flush()`                                          | flush         | forces queued commands to the surface             |

[ENTRYPOINT_SCOPE]: path construction and hit-testing
- rail: paint

`Add*` build the figure, `GetRoundRect` factories a rounded rectangle, `FillContains`/`StrokeContains` are the fill and stroke hit-tests, and `Transform`/`Clone` mutate and copy:

```csharp signature
GraphicsPath.AddLine(float startX, float startY, float endX, float endY)
GraphicsPath.AddBezier(PointF start, PointF control1, PointF control2, PointF end)
GraphicsPath.AddArc(float x, float y, float width, float height, float startAngle, float sweepAngle)
GraphicsPath.AddCurve(IEnumerable<PointF> points, float tension = 0.5f)
GraphicsPath.AddEllipse(float x, float y, float width, float height)
GraphicsPath.AddRectangle(float x, float y, float width, float height)
GraphicsPath.GetRoundRect(RectangleF rectangle, float radius)
GraphicsPath.GetRoundRect(RectangleF rectangle, float nwRadius, float neRadius, float seRadius, float swRadius)
GraphicsPath.FillContains(PointF point)
GraphicsPath.StrokeContains(Pen pen, PointF point)
GraphicsPath.Transform(IMatrix matrix)
GraphicsPath.Clone()
```

[ENTRYPOINT_SCOPE]: matrix composition
- rail: paint

| [INDEX] | [SURFACE]                                                                      | [CALL_SHAPE]        | [CAPABILITY]                     |
| :-----: | :----------------------------------------------------------------------------- | :------------------ | :------------------------------- |
|  [01]   | `Matrix.Create()`                                                              | factory → `IMatrix` | identity matrix                  |
|  [02]   | `Matrix.Create(float xx, float yx, float xy, float yy, float x0, float y0)`    | factory → `IMatrix` | builds an explicit affine matrix |
|  [03]   | `Matrix.FromTranslation(float distanceX, float distanceY)`                     | factory → `IMatrix` | translation matrix               |
|  [04]   | `Matrix.FromRotation(float angle)`                                             | factory → `IMatrix` | rotation matrix                  |
|  [05]   | `Matrix.FromScaleAt(float scaleX, float scaleY, float centerX, float centerY)` | factory → `IMatrix` | scale-about-point matrix         |
|  [06]   | `Matrix.Inverse(IMatrix matrix)`                                               | factory → `IMatrix` | inverse of a matrix              |
|  [07]   | `IMatrix.TransformPoint(PointF point)`                                         | instance → `PointF` | maps a point through the matrix  |

[ENTRYPOINT_SCOPE]: colour, brushes, bitmaps, and system fonts
- rail: paint

`Color`'s blend, distance, and conversion members compute in sRGB.

| [INDEX] | [SURFACE]                                                                                     | [CAPABILITY]                    |
| :-----: | :-------------------------------------------------------------------------------------------- | :------------------------------ |
|  [01]   | `Color.Blend(Color baseColor, Color blendColor)`                                              | 50/50 sRGB blend → `Color`      |
|  [02]   | `Color.Blend(Color baseColor, Color blendColor, float blendFactor)`                           | weighted sRGB blend → `Color`   |
|  [03]   | `Color.Distance(Color value1, Color value2)`                                                  | sRGB channel distance → `float` |
|  [04]   | `Color.ToHSB()`                                                                               | HSB conversion → `ColorHSB`     |
|  [05]   | `Color.ToHSL()`                                                                               | HSL conversion → `ColorHSL`     |
|  [06]   | `Color.ToCMYK()`                                                                              | CMYK conversion → `ColorCMYK`   |
|  [07]   | `Bitmap.Lock()`                                                                               | locks raw pixels → `BitmapData` |
|  [08]   | `Bitmap.GetPixel(Point position)`                                                             | reads one pixel → `Color`       |
|  [09]   | `Bitmap.SetPixel(Point position, Color color)`                                                | writes one pixel                |
|  [10]   | `Bitmap.ToByteArray(ImageFormat imageFormat)`                                                 | encodes to a byte stream        |
|  [11]   | `Bitmap.Clone(Rectangle? rectangle = null)`                                                   | copies whole or sub-region      |
|  [12]   | `SystemFonts.Default/Bold/Label/Menu/MenuBar/Message/Palette/StatusBar/TitleBar/ToolTip/User` | host UI font roster             |
|  [13]   | `GraphicsPath.Bounds { get; }`                                                                | reads retained path bounds      |
|  [14]   | `BitmapData.SetPixel(Point position, Color color)`                                            | writes inside one bitmap lock   |
|  [15]   | `FormattedText.ForegroundBrush { get; set; }`                                                 | assigns shaped-text foreground  |
|  [16]   | `FormattedText.Text/Font/MaximumSize/Wrap/Alignment/Trimming`                                 | configures shaped-text layout   |

Gradient and texture brush constructors:

```csharp signature
LinearGradientBrush(Color startColor, Color endColor, PointF startPoint, PointF endPoint)
RadialGradientBrush(Color startColor, Color endColor, PointF center, PointF gradientOrigin, SizeF radius)
TextureBrush(Image image, float opacity = 1f)
```

## [04]-[IMPLEMENTATION_LAW]

[DRAWING_TOPOLOGY]:
- `Graphics` is the single immediate command stream: a `Drawable` `Paint` event hands a live handle and `CreateGraphics` acquires one off-event. No retained scene exists — the host re-issues the whole paint on invalidation.
- Transform and clip are one save/restore stack: `SaveTransformState` pushes, the matching restore pops, and `IsVisible` early-culls a rectangle against the active clip.
- `GraphicsPath` is retained geometry carrying both fill and stroke hit-testing, so pointer hit-testing composes the same path the paint drew, never a parallel geometry copy.
- `Color` computes in sRGB only; perceptual blend, `DeltaE`, and gamut mapping leave this type for `Unicolour` at the seam.

[STACKING]:
- `Wacton.Unicolour`(`../../.api/api-unicolour.md`): owns the perceptual colour model. `Eto.Drawing.Color`'s sRGB `Blend`/`Distance`/`ToHSB`/`ToHSL`/`ToCMYK` map to `new Unicolour(ColourSpace.Rgb255, r, g, b)` and back through `.Rgb`, so perceptual blending, `DeltaE` distance, gamut-mapped fills, and `Mix`/`Palette` theme ramps route through `Unicolour`, the `Eto.Drawing.Color` surviving only at the paint edge feeding a `Brush` or `Pen`.
- `LanguageExt.Core`(`../../.api/api-languageext.md`): `Bitmap.Lock` returns a disposable `BitmapData`, so a lock rides an `Eff<A>`/`use` resource scope releasing the handle deterministically; `Fin<A>` rails an encode/decode over `ToByteArray`; `Seq<PointF>` is the vertex carrier a polyline or `AddCurve` folds over.
- `Thinktecture.Runtime.Extensions`(`../../.api/api-thinktecture-runtime-extensions.md`): a `[ValueObject]` owns a validated stroke-style, dash-preset, or gradient-stop value; a `[SmartEnum]` owns the closed brush-kind and system-font-role vocabularies a generator-shaped paint layer folds to rows.
- Kernel unification: easing, spring, and interpolation math positioning or animating a paint composes the Rasm kernel, never a second in-folder derivation; `Eto.Drawing` owns only the immediate render of the resolved geometry and colour.

[LOCAL_ADMISSION]:
- `Eto.Drawing` binds the same Rhino-loaded `Eto.dll` as the forms surface, never a second NuGet copy; a `Graphics` handle comes only from a `Drawable` paint event or `CreateGraphics`.
- Paint code holds canonical geometry and `Unicolour` colour internally and projects to `Eto.Drawing` primitives at the render edge; `Eto.Drawing.*` types never leak past the paint owner.

[RAIL_LAW]:
- Package: `Eto.Drawing`
- Owns: the immediate `Graphics` command stream, `GraphicsPath` geometry with fill/stroke hit-testing, the pen/brush vocabulary, `FormattedText`/`MeasureString` layout, `Matrix` composition, `Bitmap` lock and pixel access, and the `SystemFonts` roster.
- Accept: custom 2D painting, path construction and hit-testing, text measurement and layout, image blit and pixel access, and transform/clip state behind a `Drawable`.
- Reject: perceptual colour math (`Unicolour` owns it), widget construction and layout (`api-eto-forms.md`), platform-handler selection (`api-eto-platform.md`), host viewport drawing through the Rhino display pipeline (`api-rhinocommon-display.md`), and leaking `Eto.Drawing.*` types past the paint owner.

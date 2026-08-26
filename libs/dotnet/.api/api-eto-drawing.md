# [RASM_API_ETO_DRAWING]

`Eto.Drawing` is the host-neutral immediate-mode 2D paint algebra both Rhino host boundaries cross: one `Graphics` command stream strokes, fills, blits, and lays out text against a save/restore transform and clip stack with no retained scene, `GraphicsPath` accumulates the geometry and hit-tests it, the brush/pen/dash family separates fill from stroke by source, `Font`/`FormattedText` measure and lay out, and `Bitmap`/`Image` stage the raster. The host re-issues the whole paint on invalidation. This branch catalogue owns the algebra; each host-boundary folder registers it and tables only the carriers its own boundary adds.

## [01]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: value geometry and affine transform

| [INDEX] | [SYMBOL]     | [TYPE_FAMILY] | [CAPABILITY]                                                                 |
| :-----: | :----------- | :------------ | :--------------------------------------------------------------------------- |
|  [01]   | `Point`      | struct        | integer pixel position                                                       |
|  [02]   | `PointF`     | struct        | float canvas position                                                        |
|  [03]   | `Size`       | struct        | integer extent                                                               |
|  [04]   | `SizeF`      | struct        | float extent                                                                 |
|  [05]   | `Rectangle`  | struct        | integer pixel box                                                            |
|  [06]   | `RectangleF` | struct        | float box with from-center, inflate, union, intersect, and contains families |
|  [07]   | `Padding`    | struct        | four-edge inset                                                              |
|  [08]   | `IMatrix`    | interface     | affine transform carrier for graphics and path transforms                    |
|  [09]   | `Matrix`     | static        | affine factory over the `IMatrix` carrier                                    |

[PUBLIC_TYPE_SCOPE]: the immediate command stream and its paint surface

| [INDEX] | [SYMBOL]         | [TYPE_FAMILY]       | [CAPABILITY]                                                         |
| :-----: | :--------------- | :------------------ | :------------------------------------------------------------------- |
|  [01]   | `Graphics`       | class               | immediate paint state, transform and clip stacks, retained-mode flag |
|  [02]   | `Drawable`       | class (`Eto.Forms`) | custom-paint control raising `Paint` with a live `Graphics`          |
|  [03]   | `PaintEventArgs` | class               | the live context and the dirty rectangle for one paint pass          |

[PUBLIC_TYPE_SCOPE]: path, fill, and stroke vocabulary

| [INDEX] | [SYMBOL]              | [TYPE_FAMILY] | [CAPABILITY]                                            |
| :-----: | :-------------------- | :------------ | :------------------------------------------------------ |
|  [01]   | `IGraphicsPath`       | interface     | managed path contract carrying figures and hit tests    |
|  [02]   | `GraphicsPath`        | class         | path builder over lines, arcs, beziers, curves, figures |
|  [03]   | `FillMode`            | enum          | even-odd versus winding interior rule                   |
|  [04]   | `Brush`               | class         | fill-source base of the brush family                    |
|  [05]   | `SolidBrush`          | class         | single-colour fill                                      |
|  [06]   | `LinearGradientBrush` | class         | point-pair and rect-plus-angle gradient fill            |
|  [07]   | `RadialGradientBrush` | class         | radial gradient fill                                    |
|  [08]   | `TextureBrush`        | class         | image-tiled fill with opacity                           |
|  [09]   | `ITransformBrush`     | interface     | transformable-brush contract                            |
|  [10]   | `GradientWrapMode`    | enum          | pad, repeat, and reflect gradient extension             |
|  [11]   | `Pen`                 | class         | stroke source over cap, join, miter, and dash           |
|  [12]   | `PenLineCap`          | enum          | butt, round, and square end cap                         |
|  [13]   | `PenLineJoin`         | enum          | miter, bevel, and round segment join                    |
|  [14]   | `DashStyle`           | class         | dash pattern over a stroke                              |
|  [15]   | `DashStyles`          | static        | named dash presets                                      |
|  [16]   | `Brushes` / `Pens`    | static        | named brush and pen anchors                             |

[DASH_PRESETS]: `Solid` `Dash` `Dot` `DashDot` `DashDotDot`

[PUBLIC_TYPE_SCOPE]: colour spaces and system palettes

| [INDEX] | [SYMBOL]       | [TYPE_FAMILY] | [CAPABILITY]                                                               |
| :-----: | :------------- | :------------ | :------------------------------------------------------------------------- |
|  [01]   | `Color`        | struct        | sRGBA colour with component, blend, distance, and space-conversion members |
|  [02]   | `ColorHSL`     | struct        | hue-saturation-lightness projection of `Color`                             |
|  [03]   | `ColorHSB`     | struct        | hue-saturation-brightness projection                                       |
|  [04]   | `ColorCMYK`    | struct        | cyan-magenta-yellow-key projection                                         |
|  [05]   | `Colors`       | static        | named-colour anchors                                                       |
|  [06]   | `SystemColors` | static        | OS-resolved chrome palette                                                 |

- `SystemColors` rows — `Control`, `ControlBackground`, `ControlText`, `DisabledText`, `Highlight`, `HighlightText`, `Selection`, `SelectionText`, `WindowBackground`, `LinkText` — are handler reads that re-resolve on an appearance flip, so a captured value stales at the flip and a literal swatch beside a native panel diverges from the host on every accent or contrast change.

[PUBLIC_TYPE_SCOPE]: font, formatted text, and raster

| [INDEX] | [SYMBOL]                 | [TYPE_FAMILY] | [CAPABILITY]                                           |
| :-----: | :----------------------- | :------------ | :----------------------------------------------------- |
|  [01]   | `Font`                   | class         | typeface, size, style, and decoration carrier          |
|  [02]   | `FontStyle`              | enum          | bold and italic style flags                            |
|  [03]   | `FontDecoration`         | enum          | underline and strikeout decoration flags               |
|  [04]   | `SystemFonts`            | static        | host UI font roster                                    |
|  [05]   | `FormattedText`          | class         | wrapped, trimmed, aligned, measurable multi-line text  |
|  [06]   | `FormattedTextWrapMode`  | enum          | `None`, `Word`, and `Character` wrap policy            |
|  [07]   | `FormattedTextAlignment` | enum          | left, right, centre, and justify alignment             |
|  [08]   | `FormattedTextTrimming`  | enum          | ellipsis trimming policy                               |
|  [09]   | `Bitmap`                 | class         | RGBA raster with lock, pixel access, encode, and clone |
|  [10]   | `BitmapData`             | class         | locked pixel-buffer view                               |
|  [11]   | `Image`                  | class         | drawable image base consumed by `Graphics.DrawImage`   |
|  [12]   | `ImageFormat`            | enum          | encode-format selector                                 |
|  [13]   | `PixelFormat`            | enum          | pixel layout including `Format32bppRgba`               |
|  [14]   | `ImageInterpolation`     | enum          | resample quality on draw and scale                     |
|  [15]   | `Icon`                   | class         | multi-frame icon resolving a frame per device scale    |
|  [16]   | `IconFrame`              | class         | one scale-tagged raster inside an icon                 |

[SYSTEM_FONT_ROLES]: `Default` `Bold` `Label` `Menu` `MenuBar` `Message` `Palette` `StatusBar` `TitleBar` `ToolTip` `User`

## [02]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: `Graphics` — state, transform, and clip

| [INDEX] | [SURFACE]                          | [SHAPE]  | [CAPABILITY]                                     |
| :-----: | :--------------------------------- | :------- | :----------------------------------------------- |
|  [01]   | `AntiAlias`                        | property | antialiasing toggle                              |
|  [02]   | `ImageInterpolation`               | property | draw-time resample quality                       |
|  [03]   | `PixelOffsetMode`                  | property | half-pixel offset rule for crisp lines           |
|  [04]   | `PointsPerPixel`                   | property | device-pixel ratio                               |
|  [05]   | `PixelsPerPoint`                   | property | inverse device-pixel ratio                       |
|  [06]   | `IsRetained`                       | property | retained-mode flag                               |
|  [07]   | `ClipBounds`                       | property | current clip rectangle                           |
|  [08]   | `CurrentTransform`                 | property | active transform matrix                          |
|  [09]   | `TranslateTransform(float, float)` | instance | shift the origin                                 |
|  [10]   | `RotateTransform(float)`           | instance | rotate the stream                                |
|  [11]   | `ScaleTransform(float, float)`     | instance | scale the stream                                 |
|  [12]   | `MultiplyTransform(IMatrix)`       | instance | compose an arbitrary affine transform            |
|  [13]   | `SaveTransform()`                  | instance | push transform state                             |
|  [14]   | `RestoreTransform()`               | instance | pop transform state                              |
|  [15]   | `SaveTransformState()`             | instance | push transform and clip state together           |
|  [16]   | `SetClip(RectangleF)`              | instance | clip to a rectangle                              |
|  [17]   | `SetClip(IGraphicsPath)`           | instance | clip to a path                                   |
|  [18]   | `ResetClip()`                      | instance | clear the clip                                   |
|  [19]   | `IsVisible(RectangleF) -> bool`    | instance | test a rectangle against the clip for early cull |
|  [20]   | `Flush()`                          | instance | force queued commands to the surface             |

[ENTRYPOINT_SCOPE]: `Graphics` — draw, fill, image, and text

A `Pen` strokes and a `Brush` fills; each primitive carries both forms and the argument selects which.

| [INDEX] | [SURFACE]                                        | [SHAPE]  | [CAPABILITY]                |
| :-----: | :----------------------------------------------- | :------- | :-------------------------- |
|  [01]   | `DrawLine(Pen, PointF, PointF)`                  | instance | stroke a segment            |
|  [02]   | `DrawLines(Pen, IEnumerable<PointF>)`            | instance | stroke a polyline           |
|  [03]   | `DrawPolygon(Pen, params PointF[])`              | instance | stroke a closed polygon     |
|  [04]   | `FillPolygon(Brush, params PointF[])`            | instance | fill a closed polygon       |
|  [05]   | `DrawRectangle(Pen, RectangleF)`                 | instance | stroke a rectangle          |
|  [06]   | `FillRectangle(Brush, RectangleF)`               | instance | fill a rectangle            |
|  [07]   | `DrawEllipse(Pen, RectangleF)`                   | instance | stroke an ellipse           |
|  [08]   | `FillEllipse(Brush, RectangleF)`                 | instance | fill an ellipse             |
|  [09]   | `DrawArc(Pen, RectangleF, float, float)`         | instance | stroke an arc               |
|  [10]   | `FillPie(Brush, RectangleF, float, float)`       | instance | fill a pie wedge            |
|  [11]   | `DrawPath(Pen, IGraphicsPath)`                   | instance | stroke a path               |
|  [12]   | `FillPath(Brush, IGraphicsPath)`                 | instance | fill a path                 |
|  [13]   | `DrawImage(Image, float, float)`                 | instance | blit at a point             |
|  [14]   | `DrawImage(Image, RectangleF, RectangleF)`       | instance | blit source to destination  |
|  [15]   | `DrawText(Font, Brush, float, float, string)`    | instance | draw a text run             |
|  [16]   | `DrawText(Font, Brush, RectangleF, string, ...)` | instance | draw policy-laid frame text |
|  [17]   | `DrawText(FormattedText, PointF)`                | instance | draw pre-laid text          |
|  [18]   | `MeasureString(Font, string) -> SizeF`           | instance | single-run extent           |
|  [19]   | `Clear(SolidBrush)`                              | instance | flood to a fill             |

[ENTRYPOINT_SCOPE]: `GraphicsPath` — construction, hit testing, and copy

| [INDEX] | [SURFACE]                                                               | [SHAPE]  | [CAPABILITY]          |
| :-----: | :---------------------------------------------------------------------- | :------- | :-------------------- |
|  [01]   | `Create() -> IGraphicsPath`                                             | static   | mint an empty path    |
|  [02]   | `Bounds`                                                                | property | path bounding rect    |
|  [03]   | `FillMode`                                                              | property | interior rule         |
|  [04]   | `IsEmpty`                                                               | property | empty-path flag       |
|  [05]   | `CurrentPoint`                                                          | property | pen position          |
|  [06]   | `MoveTo(float, float)`                                                  | instance | move the pen          |
|  [07]   | `LineTo(float, float)`                                                  | instance | line to a point       |
|  [08]   | `AddLine(float, float, float, float)`                                   | instance | add a segment         |
|  [09]   | `AddLines(IEnumerable<PointF>)`                                         | instance | add a polyline        |
|  [10]   | `AddArc(float, float, float, float, float, float)`                      | instance | add an arc            |
|  [11]   | `AddBezier(PointF, PointF, PointF, PointF)`                             | instance | add a cubic bezier    |
|  [12]   | `AddCurve(IEnumerable<PointF>, float)`                                  | instance | add a tension curve   |
|  [13]   | `AddEllipse(float, float, float, float)`                                | instance | add an ellipse        |
|  [14]   | `AddRectangle(float, float, float, float)`                              | instance | add a rectangle       |
|  [15]   | `AddPath(IGraphicsPath, bool)`                                          | instance | append a path         |
|  [16]   | `StartFigure()`                                                         | instance | open a figure         |
|  [17]   | `CloseFigure()`                                                         | instance | close a figure        |
|  [18]   | `FillContains(PointF) -> bool`                                          | instance | fill-region hit test  |
|  [19]   | `StrokeContains(Pen, PointF) -> bool`                                   | instance | stroke hit test       |
|  [20]   | `Transform(IMatrix)`                                                    | instance | transform in place    |
|  [21]   | `Clone() -> IGraphicsPath`                                              | instance | copy the path         |
|  [22]   | `GetRoundRect(RectangleF, float) -> IGraphicsPath`                      | static   | uniform round rect    |
|  [23]   | `GetRoundRect(RectangleF, float, float, float, float) -> IGraphicsPath` | static   | per-corner round rect |

[ENTRYPOINT_SCOPE]: `Matrix` — affine composition

| [INDEX] | [SURFACE]                                                 | [SHAPE]              | [CAPABILITY]             |
| :-----: | :-------------------------------------------------------- | :------------------- | :----------------------- |
|  [01]   | `Matrix.Create()`                                         | factory -> `IMatrix` | identity matrix          |
|  [02]   | `Matrix.Create(float, float, float, float, float, float)` | factory -> `IMatrix` | explicit affine matrix   |
|  [03]   | `Matrix.FromTranslation(float, float)`                    | factory -> `IMatrix` | translation matrix       |
|  [04]   | `Matrix.FromRotation(float)`                              | factory -> `IMatrix` | rotation matrix          |
|  [05]   | `Matrix.FromScaleAt(float, float, float, float)`          | factory -> `IMatrix` | scale-about-point matrix |
|  [06]   | `Matrix.Inverse(IMatrix)`                                 | factory -> `IMatrix` | inverse of a matrix      |
|  [07]   | `IMatrix.TransformPoint(PointF) -> PointF`                | instance             | map a point              |

[ENTRYPOINT_SCOPE]: `FormattedText` — measured multi-line layout

| [INDEX] | [SURFACE]            | [SHAPE]  | [CAPABILITY]        |
| :-----: | :------------------- | :------- | :------------------ |
|  [01]   | `Text`               | property | source text         |
|  [02]   | `Font`               | property | typeface            |
|  [03]   | `ForegroundBrush`    | property | fill brush          |
|  [04]   | `Wrap`               | property | wrap mode           |
|  [05]   | `Trimming`           | property | overflow trimming   |
|  [06]   | `Alignment`          | property | line alignment      |
|  [07]   | `MaximumWidth`       | property | layout width bound  |
|  [08]   | `MaximumHeight`      | property | layout height bound |
|  [09]   | `MaximumSize`        | property | layout size bound   |
|  [10]   | `Measure() -> SizeF` | instance | measured extent     |

[ENTRYPOINT_SCOPE]: colour construction and projection

`Color` computes in sRGB; perceptual blend, delta-E, and gamut mapping leave this type at the `Unicolour` boundary.

| [INDEX] | [SURFACE]                                                  | [SHAPE]  | [CAPABILITY]                 |
| :-----: | :--------------------------------------------------------- | :------- | :--------------------------- |
|  [01]   | `Color.Blend(Color, Color) -> Color`                       | static   | even sRGB blend              |
|  [02]   | `Color.Blend(Color, Color, float) -> Color`                | static   | weighted sRGB blend          |
|  [03]   | `Color.Distance(Color, Color) -> float`                    | static   | sRGB channel distance        |
|  [04]   | `Color.ToHSB() -> ColorHSB`                                | instance | brightness-space projection  |
|  [05]   | `Color.ToHSL() -> ColorHSL`                                | instance | lightness-space projection   |
|  [06]   | `Color.ToCMYK() -> ColorCMYK`                              | instance | subtractive-space projection |
|  [07]   | `LinearGradientBrush(Color, Color, PointF, PointF)`        | ctor     | axial gradient brush         |
|  [08]   | `RadialGradientBrush(Color, Color, PointF, PointF, SizeF)` | ctor     | radial gradient brush        |
|  [09]   | `TextureBrush(Image, float)`                               | ctor     | opacity-scaled image brush   |

[ENTRYPOINT_SCOPE]: `Bitmap` — staging, pixels, and encode

| [INDEX] | [SURFACE]                                       | [SHAPE]  | [CAPABILITY]               |
| :-----: | :---------------------------------------------- | :------- | :------------------------- |
|  [01]   | `Bitmap(Size, PixelFormat)`                     | ctor     | sized blank raster         |
|  [02]   | `Bitmap(int, int, PixelFormat)`                 | ctor     | sized blank raster         |
|  [03]   | `Bitmap(string)`                                | ctor     | decode from a file         |
|  [04]   | `Bitmap(Stream)`                                | ctor     | decode from a stream       |
|  [05]   | `Bitmap(Image, int?, int?, ImageInterpolation)` | ctor     | resample an image          |
|  [06]   | `Lock() -> BitmapData`                          | instance | lock the pixel buffer      |
|  [07]   | `GetPixel(Point) -> Color`                      | instance | read a pixel               |
|  [08]   | `SetPixel(Point, Color)`                        | instance | write a pixel              |
|  [09]   | `BitmapData.SetPixel(Point, Color)`             | instance | write inside one lock      |
|  [10]   | `Clone(Rectangle?) -> Bitmap`                   | instance | copy whole or sub-region   |
|  [11]   | `Save(string, ImageFormat)`                     | instance | encode to a file           |
|  [12]   | `Save(Stream, ImageFormat)`                     | instance | encode to a stream         |
|  [13]   | `ToByteArray(ImageFormat) -> byte[]`            | instance | encode to bytes            |
|  [14]   | `BitmapData.PremultipliedAlpha`                 | property | per-lock coverage carriage |

- `BitmapData` publishes its own layout and the layout is per LOCK, never per backend: `PremultipliedAlpha` is set at construction from the live representation — the macOS handler passes `alpha && !BitmapFormat.HasFlag(NSBitmapFormat.AlphaNonpremultiplied)` — and `TranslateArgbToData`/`TranslateDataToArgb` multiply and divide coverage on exactly that flag while reordering channels, so `GetPixel`/`SetPixel` speak STRAIGHT `Color` on every backend and `TranslateDataToArgb` is the only channel-order-canonical read a caller has. `ScanWidth` is the representation's own row pitch rather than `Width * BytesPerPixel`, and `Flipped` names bottom-up rows (false on macOS), so a row egress repacks both. `Bitmap.Lock()` admits no format, so a wanted carriage is normalized after the lock, unlike the GDI `LockBits(Rectangle, ImageLockMode, PixelFormat)` leg which converts inside the lock.

[ENTRYPOINT_SCOPE]: `Drawable` — the paint surface

| [INDEX] | [SURFACE]                               | [SHAPE]  | [CAPABILITY]                          |
| :-----: | :-------------------------------------- | :------- | :------------------------------------ |
|  [01]   | `Drawable.Paint`                        | event    | `PaintEventArgs` paint hook           |
|  [02]   | `Drawable.CanFocus`                     | property | focusable-surface flag                |
|  [03]   | `Drawable.SupportsCreateGraphics`       | property | off-event acquisition support flag    |
|  [04]   | `Drawable.CreateGraphics() -> Graphics` | instance | acquire an off-event context          |
|  [05]   | `Drawable.Update(Rectangle)`            | instance | force an immediate bounded repaint    |
|  [06]   | `Drawable.TextComposition`              | event    | composed-text input                   |
|  [07]   | `Drawable.TextInsertionBoundsRequested` | event    | caret-bounds request                  |
|  [08]   | `Drawable.CancelTextComposition()`      | instance | cancel an in-progress IME composition |
|  [09]   | `Drawable.CommitTextComposition()`      | instance | commit an in-progress IME composition |
|  [10]   | `PaintEventArgs.Graphics`               | property | the immediate paint context           |
|  [11]   | `PaintEventArgs.ClipRectangle`          | property | the dirty region                      |

- `CreateGraphics` is gated on `SupportsCreateGraphics`, so an off-event acquisition probes the flag first and falls back to invalidation where the handler refuses.

## [03]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `Graphics` is the one immediate surface: paint state, the transform stack (`SaveTransform`/`RestoreTransform`, `SaveTransformState`), and the clip stack (`SetClip`/`ResetClip`) fold through a single context, `IsVisible` early-culls against the active clip, and `PointsPerPixel`/`PixelsPerPoint` carry the device-pixel ratio the painter scales against.
- `GraphicsPath` is the one geometry accumulator: `MoveTo`/`AddArc`/`AddBezier`/`AddCurve` build figures, `StartFigure`/`CloseFigure` bound them, `FillContains`/`StrokeContains` hit-test the built path, and `GetRoundRect` mints the capsule outline chrome draws — pointer hit-testing composes the same path the paint drew, never a parallel geometry copy.
- Fill and stroke separate by source, never by primitive: every `Fill*` takes a `Brush` and every `Draw*` takes a `Pen`, so a new fill or stroke style is a brush or pen value.
- `FormattedText` owns measured multi-line layout and `MeasureString` answers single-run extent; both return the `SizeF` the layout engine positions against before `DrawText` commits.
- `Bitmap` stages a `Format32bppRgba` raster the painter fills through `Lock`/`SetPixel`, draws through `Graphics.DrawImage`, encodes through `ToByteArray`/`Save`, and snapshots a sub-rectangle through `Clone`.
- `Color` computes in sRGB only; its `Blend`/`Distance`/`To*` members are the sRGB fast lane and the perceptual model owns everything past them.

[STACKING]:
- `Wacton.Unicolour`(`.api/api-unicolour.md`): owns the perceptual colour model — `Color` maps to `new Unicolour(ColourSpace.Rgb255, r, g, b)` and back through `.Rgb`, so perceptual blending, delta-E distance, gamut-mapped fills, and theme ramps route through `Unicolour` and `Color` survives only at the paint edge feeding a `Brush` or `Pen`.
- `LanguageExt.Core`(`.api/api-languageext.md`): `Bitmap.Lock` returns a disposable `BitmapData`, so a lock rides an `Eff<A>`/`use` resource scope releasing the handle deterministically; a fallible decode, encode, or unresolved-`Font` measure lowers onto `Fin<A>`; `Seq<PointF>` is the vertex carrier a polyline or `AddCurve` folds over rather than a mutable list accumulation.
- `Thinktecture.Runtime.Extensions`(`.api/api-thinktecture-runtime-extensions.md`): the drawing enum vocabulary — `FillMode`, `PenLineCap`, `PenLineJoin`, `GradientWrapMode`, `FontStyle`, `FontDecoration`, `FormattedTextWrapMode`, `FormattedTextAlignment`, `FormattedTextTrimming`, `ImageInterpolation`, `PixelFormat`, `ImageFormat` — maps at each boundary onto `[SmartEnum]` owners so a paint style value carries behaviour and exhaustive dispatch rather than a bare host enum switch; a validated stroke-style, dash-preset, or gradient-stop value is a `[ValueObject]`.
- `System.Drawing.Common`(`.api/api-system-drawing-common.md`): the GDI carriers Rhino host members declare in their own signatures cross into this surface at the host boundary alone, converted once and never mirrored as a parallel drawing vocabulary.
- Kernel unification: easing, spring, and interpolation math positioning or animating a paint composes the `Rasm` kernel; this surface owns only the immediate render of the resolved geometry and colour.

[LOCAL_ADMISSION]:
- A `Graphics` handle comes from a `Drawable` paint event or a boundary-owned off-event acquisition; a painter draws through the admitted `Graphics`/`GraphicsPath`/`Brush`/`Pen` surface, never a local wrapper renaming or partially re-exporting an Eto member.
- A new fill, stroke, or geometry is a brush value, a pen value, or a `GraphicsPath` figure, never a hand-rolled tessellator beside the path.
- Paint code holds canonical geometry and perceptual colour internally and projects to these primitives at the render edge; boundary faults ride the LanguageExt carrier with no exception-style control flow beside it.

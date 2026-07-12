# [RASM_GRASSHOPPER_API_ETO_DRAWING]

`Eto.Drawing` mints the host-neutral 2D surface every Grasshopper2 canvas painter, wire renderer, icon projector, and tooltip painter draws through. `Graphics` owns immediate paint state, the clip and transform stacks, retained hit-testable rendering, and text measurement; `GraphicsPath` owns managed vector-geometry construction; the brush/pen/gradient family owns fill and stroke, `Font`/`FormattedText` own type and measured layout, and `Bitmap`/`Image` own raster staging and pixel access. `Drawable.Paint` is the single seam where the `Eto.Forms` control lifecycle hands the immediate `PaintEventArgs.Graphics` context to this surface, and `Context.CreateFromContent` projects a GH icon into a detached drawing context.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Eto.Drawing`
- host: `Grasshopper2` inside the Rhino 9 WIP process
- assembly: `Eto.dll` (in-process host reference, ALC-hosted)
- namespace: `Eto.Drawing`
- platform: managed-neutral surface; the macOS branch backs it with CoreGraphics through `Eto.macOS`
- rail: drawing

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: graphics surface and geometry primitives
- rail: drawing

| [INDEX] | [SYMBOL]     | [KIND]    | [CAPABILITY]                                                    |
| :-----: | :----------- | :-------- | :-------------------------------------------------------------- |
|  [01]   | `Graphics`   | class     | immediate paint state, clip/transform stacks, retained render   |
|  [02]   | `IMatrix`    | interface | affine transform carrier for graphics and path transforms       |
|  [03]   | `Matrix`     | static    | matrix factory over the `IMatrix` carrier                       |
|  [04]   | `PointF`     | struct    | float-precision canvas point                                    |
|  [05]   | `SizeF`      | struct    | float-precision extent                                          |
|  [06]   | `RectangleF` | struct    | float-precision rectangle with union/intersect/inflate/contains |
|  [07]   | `Point`      | struct    | integer pixel point                                             |
|  [08]   | `Rectangle`  | struct    | integer pixel rectangle                                         |
|  [09]   | `Padding`    | struct    | four-edge inset                                                 |
|  [10]   | `LineF`      | struct    | float line segment                                              |
|  [11]   | `Region`     | class     | composite clip region                                           |

[PUBLIC_TYPE_SCOPE]: path, fill, and stroke vocabulary
- rail: drawing

| [INDEX] | [SYMBOL]              | [KIND]    | [CAPABILITY]                                            |
| :-----: | :-------------------- | :-------- | :------------------------------------------------------ |
|  [01]   | `IGraphicsPath`       | interface | managed path contract carrying figures and hit tests    |
|  [02]   | `GraphicsPath`        | class     | path builder over lines, arcs, beziers, curves, figures |
|  [03]   | `FillMode`            | enum      | even-odd versus winding fill rule                       |
|  [04]   | `Brush`               | class     | fill-source base of the brush family                    |
|  [05]   | `SolidBrush`          | class     | single-color fill                                       |
|  [06]   | `LinearGradientBrush` | class     | point-pair and rect-plus-angle gradient fill            |
|  [07]   | `RadialGradientBrush` | class     | radial gradient fill                                    |
|  [08]   | `TextureBrush`        | class     | image-tiled fill                                        |
|  [09]   | `ITransformBrush`     | interface | transformable-brush contract                            |
|  [10]   | `GradientWrapMode`    | enum      | pad/repeat/reflect gradient extension                   |
|  [11]   | `Pen`                 | class     | stroke source over cap/join/miter/dash                  |
|  [12]   | `PenLineCap`          | enum      | butt/round/square end cap                               |
|  [13]   | `PenLineJoin`         | enum      | miter/bevel/round segment join                          |
|  [14]   | `DashStyle`           | class     | dash pattern over the `DashStyles` presets              |
|  [15]   | `Brushes` / `Pens`    | static    | named brush and pen anchors                             |

[PUBLIC_TYPE_SCOPE]: color spaces and system palettes
- rail: drawing

| [INDEX] | [SYMBOL]       | [KIND] | [CAPABILITY]                                   |
| :-----: | :------------- | :----- | :--------------------------------------------- |
|  [01]   | `Color`        | struct | RGBA color with component and blend accessors  |
|  [02]   | `ColorHSL`     | struct | hue-saturation-lightness projection of `Color` |
|  [03]   | `ColorHSB`     | struct | hue-saturation-brightness projection           |
|  [04]   | `ColorCMYK`    | struct | cyan-magenta-yellow-key projection             |
|  [05]   | `Colors`       | static | named-color anchors                            |
|  [06]   | `ColorStyles`  | static | semantic style-color anchors                   |
|  [07]   | `SystemColors` | static | host-resolved chrome colors                    |

[PUBLIC_TYPE_SCOPE]: font, formatted text, and raster
- rail: drawing

| [INDEX] | [SYMBOL]             | [KIND] | [CAPABILITY]                                          |
| :-----: | :------------------- | :----- | :---------------------------------------------------- |
|  [01]   | `Font`               | class  | typeface, size, style, and decoration carrier         |
|  [02]   | `FontFamily`         | class  | family over its `FontTypeface` set                    |
|  [03]   | `FontTypeface`       | class  | concrete weight/slant face                            |
|  [04]   | `SystemFont`         | enum   | host semantic font role                               |
|  [05]   | `FontStyle`          | enum   | bold/italic style flags                               |
|  [06]   | `FontDecoration`     | enum   | underline/strikeout decoration flags                  |
|  [07]   | `TextAnchor`         | enum   | text-origin alignment anchor                          |
|  [08]   | `FormattedText`      | class  | wrapped, trimmed, aligned, measurable multi-line text |
|  [09]   | `Bitmap`             | class  | RGBA raster with lock, pixel access, save, and clone  |
|  [10]   | `IndexedBitmap`      | class  | palette-indexed raster                                |
|  [11]   | `BitmapData`         | class  | locked pixel-buffer view                              |
|  [12]   | `Palette`            | class  | indexed-color table                                   |
|  [13]   | `Image`              | class  | raster/icon base                                      |
|  [14]   | `Icon`               | class  | multi-frame icon over `IconFrame`                     |
|  [15]   | `PixelFormat`        | enum   | pixel layout including `Format32bppRgba`              |
|  [16]   | `ImageInterpolation` | enum   | resample quality on draw and scale                    |
|  [17]   | `Context`            | class  | detached drawing context off a `Graphics`             |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: `Graphics` — state, transform, and clip
- `bool AntiAlias { get; set; }`, `ImageInterpolation ImageInterpolation { get; set; }`, `PixelOffsetMode PixelOffsetMode { get; set; }`
- `float PointsPerPixel { get; }`, `float ScreenScale { get; }`, `bool IsRetained { get; }`, `RectangleF ClipBounds { get; }`, `IMatrix CurrentTransform { get; }`
- `void TranslateTransform(float offsetX, float offsetY)`, `void RotateTransform(float angle)`, `void ScaleTransform(float scaleX, float scaleY)`, `void MultiplyTransform(IMatrix matrix)`
- `void SaveTransform()`, `void RestoreTransform()`, `void SetClip(RectangleF rectangle)`, `void SetClip(IGraphicsPath path)`, `void ResetClip()`

[ENTRYPOINT_SCOPE]: `Graphics` — draw, fill, image, and text
- `void DrawLine(Pen pen, PointF start, PointF end)`, `void DrawLines(Pen pen, IEnumerable<PointF> points)`, `void DrawPolygon(Pen pen, IEnumerable<PointF> points)`
- `void DrawRectangle(Pen pen, RectangleF rectangle)`, `void FillRectangle(Brush brush, RectangleF rectangle)`, `void DrawEllipse(Pen pen, RectangleF rectangle)`, `void FillEllipse(Brush brush, RectangleF rectangle)`
- `void DrawArc(Pen pen, RectangleF rectangle, float startAngle, float sweepAngle)`, `void FillPie(Brush brush, RectangleF rectangle, float startAngle, float sweepAngle)`
- `void DrawPath(Pen pen, IGraphicsPath path)`, `void FillPath(Brush brush, IGraphicsPath path)`
- `void DrawImage(Image image, float x, float y)`, `void DrawImage(Image image, RectangleF source, RectangleF destination)`
- `void DrawText(Font font, Brush brush, float x, float y, string text)`, `void DrawText(FormattedText formattedText, PointF location)`, `SizeF MeasureString(Font font, string text)`
- `void Clear(SolidBrush brush)`, `void Flush()`

[ENTRYPOINT_SCOPE]: `GraphicsPath` — construction and hit testing
- `static IGraphicsPath Create()`, `RectangleF Bounds { get; }`, `FillMode FillMode { get; set; }`, `bool IsEmpty { get; }`, `PointF CurrentPoint { get; }`
- `void MoveTo(float x, float y)`, `void LineTo(float x, float y)`, `void AddLine(float startX, float startY, float endX, float endY)`, `void AddLines(IEnumerable<PointF> points)`
- `void AddArc(float x, float y, float width, float height, float startAngle, float sweepAngle)`, `void AddBezier(PointF start, PointF control1, PointF control2, PointF end)`, `void AddCurve(IEnumerable<PointF> points, float tension = 0.5f)`
- `void AddEllipse(float x, float y, float width, float height)`, `void AddRectangle(float x, float y, float width, float height)`, `void AddPath(IGraphicsPath path, bool connect = false)`, `void StartFigure()`, `void CloseFigure()`
- `bool FillContains(PointF point)`, `bool StrokeContains(Pen pen, PointF point)`, `void Transform(IMatrix matrix)`, `IGraphicsPath Clone()`
- `static IGraphicsPath GetRoundRect(RectangleF rectangle, float radius)`, `static IGraphicsPath GetRoundRect(RectangleF rectangle, float nwRadius, float neRadius, float seRadius, float swRadius)`

[ENTRYPOINT_SCOPE]: `FormattedText` — measured multi-line layout
- `FormattedTextWrapMode Wrap { get; set; }`, `FormattedTextTrimming Trimming { get; set; }`, `FormattedTextAlignment Alignment { get; set; }`
- `Font Font { get; set; }`, `string Text { get; set; }`, `Brush ForegroundBrush { get; set; }`
- `float MaximumWidth { get; set; }`, `float MaximumHeight { get; set; }`, `SizeF MaximumSize { get; set; }`, `SizeF Measure()`

[ENTRYPOINT_SCOPE]: `Bitmap` — staging, pixels, and encode
- `Bitmap(Size size, PixelFormat pixelFormat)`, `Bitmap(int width, int height, PixelFormat pixelFormat)`, `Bitmap(string fileName)`, `Bitmap(Stream stream)`
- `Bitmap(Image image, int? width = null, int? height = null, ImageInterpolation interpolation = default)`
- `BitmapData Lock()`, `Color GetPixel(Point point)`, `void SetPixel(Point point, Color color)`, `Bitmap Clone(Rectangle? rectangle = null)`
- `void Save(string fileName, ImageFormat format)`, `void Save(Stream stream, ImageFormat format)`, `byte[] ToByteArray(ImageFormat imageFormat)`

[ENTRYPOINT_SCOPE]: `Drawable` — the paint seam
- `Eto.Forms.Drawable`: `bool CanFocus { get; set; }`, `bool SupportsCreateGraphics { get; }`, `EventHandler<PaintEventArgs> Paint`, `EventHandler<TextCompositionEventArgs> TextComposition`, `EventHandler<TextInsertionBoundsEventArgs> TextInsertionBoundsRequested`
- `Eto.Forms.PaintEventArgs`: `Graphics Graphics { get; }`, `RectangleF ClipRectangle { get; }`
- `Eto.Drawing.Context.CreateFromContent(Graphics graphics): Context`

## [04]-[IMPLEMENTATION_LAW]

[ETO_DRAWING_TOPOLOGY]:
- `Graphics` is the single immediate surface: paint state (`AntiAlias`, `ImageInterpolation`, `PixelOffsetMode`), the transform stack (`SaveTransform`/`RestoreTransform` over `MultiplyTransform`), and the clip stack (`SetClip`/`ResetClip`) all fold through one context, and `PointsPerPixel`/`ScreenScale` carry the device-pixel ratio the painter scales against.
- `GraphicsPath` is the one geometry accumulator: `MoveTo`/`LineTo`/`AddArc`/`AddBezier`/`AddCurve` build figures, `StartFigure`/`CloseFigure` bound them, `FillContains`/`StrokeContains` answer hit tests against the built path, and `GetRoundRect` mints the capsule outline the GH2 chrome draws — no per-shape reimplementation beside the path.
- Fill and stroke separate by source, not by primitive: every `Fill*` takes a `Brush` (solid, linear, radial, texture) and every `Draw*` takes a `Pen` (cap, join, miter, dash), so a new fill or stroke style is a brush or pen value, never a new draw method.
- `FormattedText` owns measured multi-line layout (`Wrap`, `Trimming`, `Alignment`, `MaximumSize`, `Measure`); `MeasureString` answers single-run extent. Both return the `SizeF` the layout engine positions against before `DrawText` commits.
- `Bitmap` stages an RGBA raster (`Format32bppRgba`) the painter fills through `Lock`/`SetPixel` or composites into, then draws through `Graphics.DrawImage`; `ToByteArray`/`Save` encode it and `Clone` snapshots a sub-rectangle.

[STACKING]:
- `Eto.Forms`(`.api/api-eto-forms`): `Drawable.Paint` is the sole attachment point — the control lifecycle raises `PaintEventArgs`, and `PaintEventArgs.Graphics` is the immediate context this catalog draws through; GH icon projection routes through `Context.CreateFromContent(Graphics)`.
- `Eto.macOS`(`.api/api-eto-platform`): the managed `Graphics`, `GraphicsPath`, and `Bitmap` back onto CoreGraphics through the `Eto.Mac.Drawing` handler set; a curved-stroke or text-state operation the managed path leaves ambiguous resolves on the `.api/api-macos-native` `CGPath`/`CATextLayer` branch under platform gate.
- `Thinktecture.Runtime.Extensions`(`.api/api-thinktecture-runtime-extensions`): the drawing enum vocabulary (`FillMode`, `PenLineCap`, `PenLineJoin`, `GradientWrapMode`, `FontStyle`, `FontDecoration`, `ImageInterpolation`, `PixelFormat`) maps at the folder boundary onto `[SmartEnum]` owners so a canvas style value carries behavior and exhaustive dispatch rather than a bare host enum switch.
- `LanguageExt.Core`(`.api/api-languageext`): a fallible raster or type operation (`Bitmap(Stream stream)` decode, `Save`, `MeasureString` on an unresolved `Font`) lowers onto `Fin<T>` at the folder boundary, and a path or point sequence projects through `Seq<PointF>` rather than a mutable list accumulation.
- `Wacton.Unicolour`(`.api/api-unicolour`): perceptual color blending, gamut mapping, and delta-E distance compose the kernel-bound `Unicolour` model; `Eto.Drawing.Color` is the sRGBA payload the blended result projects back into at the paint boundary, never a second Eto-side perceptual derivation.

[RAIL_LAW]:
- Surface: `Eto.Drawing`
- Owns: the host-neutral immediate 2D paint context, managed path geometry, the brush/pen/gradient fill-stroke vocabulary, measured `Font`/`FormattedText` layout, and `Bitmap`/`Image` raster staging
- Accept: `Graphics` from `PaintEventArgs`, `IGraphicsPath` from `GraphicsPath.Create`, `Brush`/`Pen` fill-stroke sources, `Format32bppRgba` staging rasters, `SizeF` measurement results
- Reject: a hand-rolled bezier or round-rect tessellator beside `GraphicsPath`, a per-primitive fill/stroke method family, a bare host enum switch the `[SmartEnum]` boundary owns, a raster decode that throws instead of lowering onto `Fin<T>`, a second perceptual color model beside the kernel `Unicolour` owner

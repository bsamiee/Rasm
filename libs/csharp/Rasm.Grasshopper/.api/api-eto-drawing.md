# [RASM_GRASSHOPPER_API_ETO_DRAWING]

`Eto.Drawing` mints the host-neutral 2D paint surface every Grasshopper2 canvas painter, wire renderer, icon projector, and tooltip painter draws through: `Graphics` folds immediate paint state, the clip and transform stacks, retained hit-testable rendering, and measurement into one context, and `GraphicsPath`, the brush/pen/gradient family, `Font`/`FormattedText`, and `Bitmap`/`Image` own geometry, fill-stroke, layout, and raster staging beside it. `Drawable.Paint` hands the `Eto.Forms` control lifecycle's immediate `PaintEventArgs.Graphics` to this surface.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Eto`
- package: `Eto` (host-provided cross-platform Eto.Forms UI framework)
- license: BSD-3-Clause
- assembly: `Eto` (`Eto.dll`)
- namespace: `Eto.Drawing`
- asset: host-provided — RhinoWIP ships `Eto.dll` under `RhCore.framework/Versions/A/Resources`; no NuGet admission
- rail: drawing

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: graphics surface and geometry primitives

| [INDEX] | [SYMBOL]     | [TYPE_FAMILY] | [CAPABILITY]                                                  |
| :-----: | :----------- | :------------ | :------------------------------------------------------------ |
|  [01]   | `Graphics`   | class         | immediate paint state, clip/transform stacks, retained render |
|  [02]   | `IMatrix`    | interface     | affine transform carrier for graphics and path transforms     |
|  [03]   | `Matrix`     | static        | matrix factory over the `IMatrix` carrier                     |
|  [04]   | `PointF`     | struct        | float-precision canvas point                                  |
|  [05]   | `SizeF`      | struct        | float-precision extent                                        |
|  [06]   | `RectangleF` | struct        | float rectangle with union/intersect/inflate/contains         |
|  [07]   | `Point`      | struct        | integer pixel point                                           |
|  [08]   | `Rectangle`  | struct        | integer pixel rectangle                                       |
|  [09]   | `Padding`    | struct        | four-edge inset                                               |
|  [10]   | `Region`     | class         | composite clip region                                         |

[PUBLIC_TYPE_SCOPE]: path, fill, and stroke vocabulary

| [INDEX] | [SYMBOL]              | [TYPE_FAMILY] | [CAPABILITY]                                            |
| :-----: | :-------------------- | :------------ | :------------------------------------------------------ |
|  [01]   | `IGraphicsPath`       | interface     | managed path contract carrying figures and hit tests    |
|  [02]   | `GraphicsPath`        | class         | path builder over lines, arcs, beziers, curves, figures |
|  [03]   | `FillMode`            | enum          | even-odd versus winding fill rule                       |
|  [04]   | `Brush`               | class         | fill-source base of the brush family                    |
|  [05]   | `SolidBrush`          | class         | single-color fill                                       |
|  [06]   | `LinearGradientBrush` | class         | point-pair and rect-plus-angle gradient fill            |
|  [07]   | `RadialGradientBrush` | class         | radial gradient fill                                    |
|  [08]   | `TextureBrush`        | class         | image-tiled fill                                        |
|  [09]   | `ITransformBrush`     | interface     | transformable-brush contract                            |
|  [10]   | `GradientWrapMode`    | enum          | pad/repeat/reflect gradient extension                   |
|  [11]   | `Pen`                 | class         | stroke source over cap/join/miter/dash                  |
|  [12]   | `PenLineCap`          | enum          | butt/round/square end cap                               |
|  [13]   | `PenLineJoin`         | enum          | miter/bevel/round segment join                          |
|  [14]   | `DashStyle`           | class         | dash pattern over the `DashStyles` presets              |
|  [15]   | `Brushes` / `Pens`    | static        | named brush and pen anchors                             |

[PUBLIC_TYPE_SCOPE]: color spaces and system palettes

| [INDEX] | [SYMBOL]       | [TYPE_FAMILY] | [CAPABILITY]                                   |
| :-----: | :------------- | :------------ | :--------------------------------------------- |
|  [01]   | `Color`        | struct        | RGBA color with component and blend accessors  |
|  [02]   | `ColorHSL`     | struct        | hue-saturation-lightness projection of `Color` |
|  [03]   | `ColorHSB`     | struct        | hue-saturation-brightness projection           |
|  [04]   | `ColorCMYK`    | struct        | cyan-magenta-yellow-key projection             |
|  [05]   | `Colors`       | static        | named-color anchors                            |
|  [06]   | `ColorStyles`  | static        | semantic style-color anchors                   |
|  [07]   | `SystemColors` | static        | host-resolved chrome colors                    |

[PUBLIC_TYPE_SCOPE]: font, formatted text, and raster

| [INDEX] | [SYMBOL]             | [TYPE_FAMILY] | [CAPABILITY]                                          |
| :-----: | :------------------- | :------------ | :---------------------------------------------------- |
|  [01]   | `Font`               | class         | typeface, size, style, and decoration carrier         |
|  [02]   | `FontFamily`         | class         | family over its `FontTypeface` set                    |
|  [03]   | `FontTypeface`       | class         | concrete weight/slant face                            |
|  [04]   | `SystemFont`         | enum          | host semantic font role                               |
|  [05]   | `FontStyle`          | enum          | bold/italic style flags                               |
|  [06]   | `FontDecoration`     | enum          | underline/strikeout decoration flags                  |
|  [07]   | `FormattedText`      | class         | wrapped, trimmed, aligned, measurable multi-line text |
|  [08]   | `Bitmap`             | class         | RGBA raster with lock, pixel access, save, and clone  |
|  [09]   | `IndexedBitmap`      | class         | palette-indexed raster                                |
|  [10]   | `BitmapData`         | class         | locked pixel-buffer view                              |
|  [11]   | `Palette`            | class         | indexed-color table                                   |
|  [12]   | `Image`              | class         | raster/icon base                                      |
|  [13]   | `Icon`               | class         | multi-frame icon over `IconFrame`                     |
|  [14]   | `PixelFormat`        | enum          | pixel layout including `Format32bppRgba`              |
|  [15]   | `ImageInterpolation` | enum          | resample quality on draw and scale                    |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: `Graphics` — state, transform, and clip

| [INDEX] | [SURFACE]                          | [SHAPE]  | [CAPABILITY]               |
| :-----: | :--------------------------------- | :------- | :------------------------- |
|  [01]   | `AntiAlias`                        | property | antialiasing toggle        |
|  [02]   | `ImageInterpolation`               | property | draw-time resample quality |
|  [03]   | `PixelOffsetMode`                  | property | pixel-offset rule          |
|  [04]   | `PointsPerPixel`                   | property | device-pixel ratio         |
|  [05]   | `PixelsPerPoint`                   | property | inverse device-pixel ratio |
|  [06]   | `IsRetained`                       | property | retained-mode flag         |
|  [07]   | `ClipBounds`                       | property | current clip rectangle     |
|  [08]   | `CurrentTransform`                 | property | active transform matrix    |
|  [09]   | `TranslateTransform(float, float)` | instance | translate the transform    |
|  [10]   | `RotateTransform(float)`           | instance | rotate the transform       |
|  [11]   | `ScaleTransform(float, float)`     | instance | scale the transform        |
|  [12]   | `MultiplyTransform(IMatrix)`       | instance | compose a matrix           |
|  [13]   | `SaveTransform()`                  | instance | push transform state       |
|  [14]   | `RestoreTransform()`               | instance | pop transform state        |
|  [15]   | `SetClip(RectangleF)`              | instance | clip to a rectangle        |
|  [16]   | `SetClip(IGraphicsPath)`           | instance | clip to a path             |
|  [17]   | `ResetClip()`                      | instance | clear the clip             |

[ENTRYPOINT_SCOPE]: `Graphics` — draw, fill, image, and text

| [INDEX] | [SURFACE]                                     | [SHAPE]  | [CAPABILITY]             |
| :-----: | :-------------------------------------------- | :------- | :----------------------- |
|  [01]   | `DrawLine(Pen, PointF, PointF)`               | instance | stroke a segment         |
|  [02]   | `DrawLines(Pen, IEnumerable<PointF>)`         | instance | stroke a polyline        |
|  [03]   | `DrawPolygon(Pen, IEnumerable<PointF>)`       | instance | stroke a closed polygon  |
|  [04]   | `DrawRectangle(Pen, RectangleF)`              | instance | stroke a rectangle       |
|  [05]   | `FillRectangle(Brush, RectangleF)`            | instance | fill a rectangle         |
|  [06]   | `DrawEllipse(Pen, RectangleF)`                | instance | stroke an ellipse        |
|  [07]   | `FillEllipse(Brush, RectangleF)`              | instance | fill an ellipse          |
|  [08]   | `DrawArc(Pen, RectangleF, float, float)`      | instance | stroke an arc            |
|  [09]   | `FillPie(Brush, RectangleF, float, float)`    | instance | fill a pie wedge         |
|  [10]   | `DrawPath(Pen, IGraphicsPath)`                | instance | stroke a path            |
|  [11]   | `FillPath(Brush, IGraphicsPath)`              | instance | fill a path              |
|  [12]   | `DrawImage(Image, float, float)`              | instance | blit at a point          |
|  [13]   | `DrawImage(Image, RectangleF, RectangleF)`    | instance | blit source to dest rect |
|  [14]   | `DrawText(Font, Brush, float, float, string)` | instance | draw a text run          |
|  [15]   | `DrawText(FormattedText, PointF)`             | instance | draw laid-out text       |
|  [16]   | `MeasureString(Font, string) -> SizeF`        | instance | single-run extent        |
|  [17]   | `Clear(SolidBrush)`                           | instance | clear to a fill          |
|  [18]   | `Flush()`                                     | instance | commit pending paint     |

[ENTRYPOINT_SCOPE]: `GraphicsPath` — construction and hit testing

| [INDEX] | [SURFACE]                                                               | [SHAPE]  | [CAPABILITY]          |
| :-----: | :---------------------------------------------------------------------- | :------- | :-------------------- |
|  [01]   | `Create() -> IGraphicsPath`                                             | static   | mint an empty path    |
|  [02]   | `Bounds`                                                                | property | path bounding rect    |
|  [03]   | `FillMode`                                                              | property | fill rule             |
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
|  [20]   | `Transform(IMatrix)`                                                    | instance | transform the path    |
|  [21]   | `Clone() -> IGraphicsPath`                                              | instance | copy the path         |
|  [22]   | `GetRoundRect(RectangleF, float) -> IGraphicsPath`                      | static   | uniform round rect    |
|  [23]   | `GetRoundRect(RectangleF, float, float, float, float) -> IGraphicsPath` | static   | per-corner round rect |

[ENTRYPOINT_SCOPE]: `FormattedText` — measured multi-line layout

| [INDEX] | [SURFACE]            | [SHAPE]  | [CAPABILITY]        |
| :-----: | :------------------- | :------- | :------------------ |
|  [01]   | `Wrap`               | property | wrap mode           |
|  [02]   | `Trimming`           | property | overflow trimming   |
|  [03]   | `Alignment`          | property | line alignment      |
|  [04]   | `Font`               | property | typeface            |
|  [05]   | `Text`               | property | source text         |
|  [06]   | `ForegroundBrush`    | property | fill brush          |
|  [07]   | `MaximumWidth`       | property | layout width bound  |
|  [08]   | `MaximumHeight`      | property | layout height bound |
|  [09]   | `MaximumSize`        | property | layout size bound   |
|  [10]   | `Measure() -> SizeF` | instance | measured extent     |

[ENTRYPOINT_SCOPE]: `Bitmap` — staging, pixels, and encode

| [INDEX] | [SURFACE]                                       | [SHAPE]  | [CAPABILITY]          |
| :-----: | :---------------------------------------------- | :------- | :-------------------- |
|  [01]   | `Bitmap(Size, PixelFormat)`                     | ctor     | sized blank raster    |
|  [02]   | `Bitmap(int, int, PixelFormat)`                 | ctor     | sized blank raster    |
|  [03]   | `Bitmap(string)`                                | ctor     | decode from a file    |
|  [04]   | `Bitmap(Stream)`                                | ctor     | decode from a stream  |
|  [05]   | `Bitmap(Image, int?, int?, ImageInterpolation)` | ctor     | resample an image     |
|  [06]   | `Lock() -> BitmapData`                          | instance | lock the pixel buffer |
|  [07]   | `GetPixel(Point) -> Color`                      | instance | read a pixel          |
|  [08]   | `SetPixel(Point, Color)`                        | instance | write a pixel         |
|  [09]   | `Clone(Rectangle?) -> Bitmap`                   | instance | snapshot a sub-rect   |
|  [10]   | `Save(string, ImageFormat)`                     | instance | encode to a file      |
|  [11]   | `Save(Stream, ImageFormat)`                     | instance | encode to a stream    |
|  [12]   | `ToByteArray(ImageFormat) -> byte[]`            | instance | encode to bytes       |

[ENTRYPOINT_SCOPE]: `Drawable` — the paint seam

| [INDEX] | [SURFACE]                               | [SHAPE]  | [CAPABILITY]                 |
| :-----: | :-------------------------------------- | :------- | :--------------------------- |
|  [01]   | `Drawable.CanFocus`                     | property | focusable-surface flag       |
|  [02]   | `Drawable.SupportsCreateGraphics`       | property | direct-graphics support flag |
|  [03]   | `Drawable.Paint`                        | event    | `PaintEventArgs` paint hook  |
|  [04]   | `Drawable.TextComposition`              | event    | composed-text input          |
|  [05]   | `Drawable.TextInsertionBoundsRequested` | event    | caret-bounds request         |
|  [06]   | `PaintEventArgs.Graphics`               | property | the immediate paint context  |
|  [07]   | `PaintEventArgs.ClipRectangle`          | property | the dirty region             |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `Graphics` is the one immediate surface: paint state, the transform stack (`SaveTransform`/`RestoreTransform`), and the clip stack (`SetClip`/`ResetClip`) fold through a single context, and `PointsPerPixel`/`PixelsPerPoint` carry the device-pixel ratio the painter scales against.
- `GraphicsPath` is the one geometry accumulator: `MoveTo`/`AddArc`/`AddBezier`/`AddCurve` build figures, `StartFigure`/`CloseFigure` bound them, `FillContains`/`StrokeContains` hit-test the built path, and `GetRoundRect` mints the capsule outline the GH2 chrome draws.
- Fill and stroke separate by source, never by primitive: every `Fill*` takes a `Brush` and every `Draw*` takes a `Pen`, so a new fill or stroke style is a brush or pen value.
- `FormattedText` owns measured multi-line layout and `MeasureString` answers single-run extent; both return the `SizeF` the layout engine positions against before `DrawText` commits.
- `Bitmap` stages a `Format32bppRgba` raster the painter fills through `Lock`/`SetPixel`, draws through `Graphics.DrawImage`, encodes through `ToByteArray`/`Save`, and snapshots a sub-rectangle through `Clone`.

[STACKING]:
- `api-eto-forms`(`libs/csharp/Rasm.Grasshopper/.api/api-eto-forms.md`): `Drawable.Paint` is the sole attachment point — the control lifecycle raises `PaintEventArgs` and `PaintEventArgs.Graphics` is the immediate context this catalog draws through.
- `api-eto-platform`(`libs/csharp/Rasm.Grasshopper/.api/api-eto-platform.md`): the managed `Graphics`, `GraphicsPath`, and `Bitmap` back onto CoreGraphics through the `Eto.Mac.Drawing` handler set; a curved-stroke or text-state operation the managed path leaves ambiguous resolves on the `api-macos-native` `CGPath`/`CATextLayer` branch under platform gate.
- `api-thinktecture-runtime-extensions`(`libs/csharp/.api/api-thinktecture-runtime-extensions.md`): the drawing enum vocabulary (`FillMode`, `PenLineCap`, `PenLineJoin`, `GradientWrapMode`, `FontStyle`, `FontDecoration`, `ImageInterpolation`, `PixelFormat`) maps at the folder boundary onto `[SmartEnum]` owners so a canvas style value carries behavior and exhaustive dispatch rather than a bare host enum switch.
- `api-languageext`(`libs/csharp/.api/api-languageext.md`): a fallible raster or type operation (`Bitmap(Stream)` decode, `Save`, `MeasureString` on an unresolved `Font`) lowers onto `Fin<T>` at the folder boundary, and a path or point sequence projects through `Seq<PointF>` rather than a mutable list accumulation.
- `api-unicolour`(`libs/csharp/.api/api-unicolour.md`): perceptual color blending, gamut mapping, and delta-E distance compose the kernel-bound `Unicolour` model, and `Eto.Drawing.Color` is the sRGBA payload the blended result projects back into at the paint boundary.

[LOCAL_ADMISSION]:
- `Eto.Drawing` is host-provided and composed directly — a painter draws through the admitted `Graphics`/`GraphicsPath`/`Brush`/`Pen` surface, never a local wrapper renaming or partially re-exporting Eto members.
- a new fill, stroke, or geometry is a brush value, a pen value, or a `GraphicsPath` figure, never a hand-rolled tessellator beside the path.
- boundary faults ride the LanguageExt rail; the painter carries no exception-style control flow beside it.

[RAIL_LAW]:
- Package: `Eto`
- Owns: the host-neutral immediate 2D paint context, managed path geometry, the brush/pen/gradient fill-stroke vocabulary, measured `Font`/`FormattedText` layout, and `Bitmap`/`Image` raster staging
- Accept: `Graphics` from `PaintEventArgs`, `IGraphicsPath` from `GraphicsPath.Create`, `Brush`/`Pen` fill-stroke sources, `Format32bppRgba` staging rasters, `SizeF` measurement results
- Reject: a hand-rolled bezier or round-rect tessellator beside `GraphicsPath`, a per-primitive fill/stroke method family, a bare host enum switch the `[SmartEnum]` boundary owns, a raster decode that throws instead of lowering onto `Fin<T>`, a second perceptual color model beside the `Unicolour` owner

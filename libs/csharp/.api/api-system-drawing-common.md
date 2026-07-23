# [RASM_API_SYSTEM_DRAWING_COMMON]

`System.Drawing` raster, icon, graphics-context, path, and imaging types are the vocabulary Rhino host members declare across their own boundary signatures, and a consumer binds every one of them at compile time. Running Rhino owns the drawing provider and no build output ships one, so image processing, rendering, and platform graphics stay with the owners that run them.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `System.Drawing.Common`
- package: `System.Drawing.Common` (MIT)
- assembly: `System.Drawing.Common`
- namespace: `System.Drawing`, `System.Drawing.Imaging`, `System.Drawing.Drawing2D`
- depends: `System.Drawing.Primitives` carries `Color` `Point` `PointF` `Size` `SizeF` `Rectangle` `RectangleF`
- abi: RhinoWIP host build backs these names with AppKit and CoreGraphics; package build backs them with GDI+
- rail: host-ui-compile

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: image and pixel-memory carriers

| [INDEX] | [SYMBOL]         | [TYPE_FAMILY] | [CAPABILITY]                        |
| :-----: | :--------------- | :------------ | :---------------------------------- |
|  [01]   | `Bitmap`         | class         | raster with lockable pixel memory   |
|  [02]   | `Image`          | class         | image base carrying IO and metadata |
|  [03]   | `Icon`           | class         | icon carrier projecting to raster   |
|  [04]   | `BitmapData`     | class         | pinned pixel-window descriptor      |
|  [05]   | `PropertyItem`   | class         | one image metadata tag              |
|  [06]   | `FrameDimension` | class         | multi-frame page selector           |
|  [07]   | `ImageFormat`    | class         | codec identity by GUID              |

[IMAGING_VOCABULARY]: `PixelFormat` `ImageLockMode` `ImageFlags` `RotateFlipType`

[PUBLIC_TYPE_SCOPE]: drawing context, path geometry, and style carriers

| [INDEX] | [SYMBOL]              | [TYPE_FAMILY] | [CAPABILITY]                       |
| :-----: | :-------------------- | :------------ | :--------------------------------- |
|  [01]   | `Graphics`            | class         | drawing context and command stream |
|  [02]   | `GraphicsState`       | class         | saved context-state token          |
|  [03]   | `GraphicsContainer`   | class         | nested context scope token         |
|  [04]   | `GraphicsPath`        | class         | vector path under a fill rule      |
|  [05]   | `Matrix`              | class         | affine transform                   |
|  [06]   | `Region`              | class         | combinable clip region             |
|  [07]   | `Pen`                 | class         | stroke style carrier               |
|  [08]   | `Brush`               | class         | fill style base                    |
|  [09]   | `SolidBrush`          | class         | single-colour fill                 |
|  [10]   | `TextureBrush`        | class         | image-tiled fill                   |
|  [11]   | `LinearGradientBrush` | class         | axial gradient fill                |
|  [12]   | `PathGradientBrush`   | class         | path-centred gradient fill         |
|  [13]   | `HatchBrush`          | class         | hatch-pattern fill                 |
|  [14]   | `Font`                | class         | text font carrier                  |
|  [15]   | `FontFamily`          | class         | typeface family carrier            |
|  [16]   | `StringFormat`        | class         | text layout policy                 |

[RENDER_QUALITY]: `SmoothingMode` `InterpolationMode` `PixelOffsetMode` `TextRenderingHint` `CompositingMode` `CompositingQuality`
[PATH_VOCABULARY]: `FillMode` `CombineMode` `MatrixOrder` `GraphicsUnit` `CoordinateSpace` `PathPointType`
[STROKE_VOCABULARY]: `DashStyle` `DashCap` `LineCap` `LineJoin` `WrapMode` `HatchStyle` `LinearGradientMode`
[TEXT_VOCABULARY]: `FontStyle` `StringAlignment` `StringTrimming` `StringFormatFlags` `ContentAlignment`

[PUBLIC_TYPE_SCOPE]: encoder-parameter carriers, bound by the package build alone

| [INDEX] | [SYMBOL]            | [TYPE_FAMILY] | [CAPABILITY]                  |
| :-----: | :------------------ | :------------ | :---------------------------- |
|  [01]   | `ImageCodecInfo`    | class         | installed encoder metadata    |
|  [02]   | `Encoder`           | class         | encoder parameter category    |
|  [03]   | `EncoderParameters` | class         | fixed-length parameter roster |
|  [04]   | `EncoderParameter`  | class         | one scalar encoder parameter  |

[ENCODER_VOCABULARY]: `EncoderValue` `EncoderParameterValueType` `ImageCodecFlags`

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: image construction, IO, and metadata

| [INDEX] | [SURFACE]                                        | [SHAPE]  | [CAPABILITY]                   |
| :-----: | :----------------------------------------------- | :------- | :----------------------------- |
|  [01]   | `Bitmap(int, int)`                               | ctor     | blank raster at a pixel extent |
|  [02]   | `Bitmap(int, int, PixelFormat)`                  | ctor     | blank raster at a pixel layout |
|  [03]   | `Bitmap(int, int, int, PixelFormat, nint)`       | ctor     | wrap caller-owned pixel memory |
|  [04]   | `Bitmap(Stream)`                                 | ctor     | decode a stream                |
|  [05]   | `Bitmap(Image, Size)`                            | ctor     | rescaled copy                  |
|  [06]   | `Bitmap.Clone(Rectangle, PixelFormat) -> Bitmap` | instance | crop and convert               |
|  [07]   | `Bitmap.MakeTransparent(Color)`                  | instance | key one colour to alpha        |
|  [08]   | `Bitmap.SetResolution(float, float)`             | instance | stamp DPI metadata             |
|  [09]   | `Image.FromStream(Stream) -> Image`              | static   | decode a stream                |
|  [10]   | `Image.FromFile(string) -> Image`                | static   | decode a file                  |
|  [11]   | `Image.Save(Stream, ImageFormat)`                | instance | encode to a stream             |
|  [12]   | `Image.Save(string, ImageFormat)`                | instance | encode to a file               |
|  [13]   | `Image.RotateFlip(RotateFlipType)`               | instance | reorient in place              |
|  [14]   | `Image.HorizontalResolution -> float`            | property | horizontal DPI                 |
|  [15]   | `Image.VerticalResolution -> float`              | property | vertical DPI                   |
|  [16]   | `Image.PropertyItems -> PropertyItem[]`          | property | metadata tag roster            |
|  [17]   | `Image.GetPropertyItem(int) -> PropertyItem`     | instance | read one metadata tag          |
|  [18]   | `Image.SetPropertyItem(PropertyItem)`            | instance | write one metadata tag         |
|  [19]   | `Image.GetFrameCount(FrameDimension) -> int`     | instance | count frames on a dimension    |
|  [20]   | `Icon(Stream, Size)`                             | ctor     | decode at a requested size     |
|  [21]   | `Icon.FromHandle(nint) -> Icon`                  | static   | adopt a native icon handle     |
|  [22]   | `Icon.ToBitmap() -> Bitmap`                      | instance | project to a raster            |

[IMAGE_FORMAT]: `Png` `Jpeg` `Tiff` `Bmp` `Gif` `Icon` `Emf` `Wmf` `Exif` `MemoryBmp` `Heif` `Webp`

[ENTRYPOINT_SCOPE]: pixel memory access

| [INDEX] | [SURFACE]                                                              | [SHAPE]  | [CAPABILITY]              |
| :-----: | :--------------------------------------------------------------------- | :------- | :------------------------ |
|  [01]   | `Bitmap.LockBits(Rectangle, ImageLockMode, PixelFormat) -> BitmapData` | instance | pin a pixel window        |
|  [02]   | `Bitmap.UnlockBits(BitmapData)`                                        | instance | release the pinned window |
|  [03]   | `BitmapData.Scan0 -> nint`                                             | property | first-row base pointer    |
|  [04]   | `BitmapData.Stride -> int`                                             | property | row byte pitch            |
|  [05]   | `BitmapData.Width -> int`                                              | property | pinned window width       |
|  [06]   | `BitmapData.Height -> int`                                             | property | pinned window height      |
|  [07]   | `Bitmap.GetPixel(int, int) -> Color`                                   | instance | single-pixel read         |
|  [08]   | `Bitmap.SetPixel(int, int, Color)`                                     | instance | single-pixel write        |

[ENTRYPOINT_SCOPE]: drawing context state and draw calls

| [INDEX] | [SURFACE]                                                            | [SHAPE]  | [CAPABILITY]                 |
| :-----: | :------------------------------------------------------------------- | :------- | :--------------------------- |
|  [01]   | `Graphics.FromImage(Image) -> Graphics`                              | static   | open a context over a raster |
|  [02]   | `Graphics.Save() -> GraphicsState`                                   | instance | push context state           |
|  [03]   | `Graphics.Restore(GraphicsState)`                                    | instance | pop context state            |
|  [04]   | `Graphics.BeginContainer() -> GraphicsContainer`                     | instance | open a nested scope          |
|  [05]   | `Graphics.EndContainer(GraphicsContainer)`                           | instance | close a nested scope         |
|  [06]   | `Graphics.Clear(Color)`                                              | instance | flood the surface            |
|  [07]   | `Graphics.DrawImage(Image, Rectangle)`                               | instance | blit to a destination rect   |
|  [08]   | `Graphics.DrawImage(Image, Rectangle, Rectangle, GraphicsUnit)`      | instance | blit one source window       |
|  [09]   | `Graphics.DrawPath(Pen, GraphicsPath)`                               | instance | stroke a path                |
|  [10]   | `Graphics.FillPath(Brush, GraphicsPath)`                             | instance | fill a path                  |
|  [11]   | `Graphics.MeasureString(string, Font, SizeF, StringFormat) -> SizeF` | instance | measure wrapped text         |
|  [12]   | `Graphics.SetClip(GraphicsPath, CombineMode)`                        | instance | combine a path clip          |
|  [13]   | `Graphics.ResetClip()`                                               | instance | drop the clip                |
|  [14]   | `Graphics.MultiplyTransform(Matrix, MatrixOrder)`                    | instance | compose the transform        |
|  [15]   | `Graphics.SmoothingMode`                                             | property | anti-alias knob              |
|  [16]   | `Graphics.InterpolationMode`                                         | property | resample knob                |
|  [17]   | `Graphics.PixelOffsetMode`                                           | property | half-pixel knob              |
|  [18]   | `Graphics.TextRenderingHint`                                         | property | glyph raster knob            |
|  [19]   | `Graphics.DpiX -> float`                                             | property | context horizontal DPI       |
|  [20]   | `Graphics.DpiY -> float`                                             | property | context vertical DPI         |

- `Graphics.DpiX`/`Graphics.DpiY`: RhinoWIP's host build returns a constant `96f`, so a Retina scale factor resolves from the host view rather than the context.

[ENTRYPOINT_SCOPE]: path geometry and transform

| [INDEX] | [SURFACE]                                                                          | [SHAPE]  | [CAPABILITY]                   |
| :-----: | :--------------------------------------------------------------------------------- | :------- | :----------------------------- |
|  [01]   | `GraphicsPath(FillMode)`                                                           | ctor     | empty path at an interior rule |
|  [02]   | `GraphicsPath.AddLines(PointF[])`                                                  | instance | append a polyline              |
|  [03]   | `GraphicsPath.AddBezier(PointF, PointF, PointF, PointF)`                           | instance | append a cubic segment         |
|  [04]   | `GraphicsPath.AddString(string, FontFamily, int, float, RectangleF, StringFormat)` | instance | append glyph outlines          |
|  [05]   | `GraphicsPath.AddPath(GraphicsPath, bool)`                                         | instance | append a sub-path              |
|  [06]   | `GraphicsPath.CloseFigure()`                                                       | instance | close the open figure          |
|  [07]   | `GraphicsPath.Transform(Matrix)`                                                   | instance | transform in place             |
|  [08]   | `GraphicsPath.Flatten(Matrix, float)`                                              | instance | flatten curves to a tolerance  |
|  [09]   | `GraphicsPath.Widen(Pen, Matrix, float)`                                           | instance | outline the stroke             |
|  [10]   | `GraphicsPath.GetBounds(Matrix, Pen) -> RectangleF`                                | instance | stroked bounds                 |
|  [11]   | `GraphicsPath.PathPoints -> PointF[]`                                              | property | control point roster           |
|  [12]   | `GraphicsPath.PathTypes -> byte[]`                                                 | property | per-point segment kinds        |
|  [13]   | `Matrix(float, float, float, float, float, float)`                                 | ctor     | affine from six elements       |
|  [14]   | `Matrix.Multiply(Matrix, MatrixOrder)`                                             | instance | compose transforms             |
|  [15]   | `Matrix.RotateAt(float, PointF, MatrixOrder)`                                      | instance | rotate about a pivot           |
|  [16]   | `Matrix.Invert()`                                                                  | instance | invert in place                |
|  [17]   | `Matrix.TransformPoints(PointF[])`                                                 | instance | map points in place            |
|  [18]   | `Matrix.Elements -> float[]`                                                       | property | six-element readout            |

[ENTRYPOINT_SCOPE]: encoder-parameter save, bound by the package build alone

| [INDEX] | [SURFACE]                                                  | [SHAPE]  | [CAPABILITY]                 |
| :-----: | :--------------------------------------------------------- | :------- | :--------------------------- |
|  [01]   | `ImageCodecInfo.GetImageEncoders() -> ImageCodecInfo[]`    | static   | installed encoder roster     |
|  [02]   | `ImageCodecInfo.FormatID -> Guid`                          | property | codec format identity        |
|  [03]   | `ImageCodecInfo.MimeType -> string`                        | property | codec media type             |
|  [04]   | `ImageFormat.Guid -> Guid`                                 | property | format identity to match     |
|  [05]   | `EncoderParameters(int)`                                   | ctor     | fixed-length parameter slots |
|  [06]   | `EncoderParameters.Param -> EncoderParameter[]`            | property | parameter slot array         |
|  [07]   | `EncoderParameter(Encoder, long)`                          | ctor     | one scalar parameter         |
|  [08]   | `Encoder.Quality`                                          | static   | JPEG quality category        |
|  [09]   | `Encoder.Compression`                                      | static   | TIFF compression category    |
|  [10]   | `Encoder.ColorDepth`                                       | static   | bit-depth category           |
|  [11]   | `Image.Save(Stream, ImageCodecInfo, EncoderParameters)`    | instance | encode with parameters       |
|  [12]   | `Image.SaveAdd(EncoderParameters)`                         | instance | append a multi-frame page    |
|  [13]   | `Image.GetEncoderParameterList(Guid) -> EncoderParameters` | instance | supported parameter ranges   |

[ENCODER_COMPRESSION]: `CompressionNone` `CompressionLZW` `CompressionCCITT3` `CompressionCCITT4` `CompressionRle`

[ENTRYPOINT_SCOPE]: `System.Drawing.Primitives` colour construction

| [INDEX] | [SURFACE]                                     | [SHAPE]  | [CAPABILITY]                    |
| :-----: | :-------------------------------------------- | :------- | :------------------------------ |
|  [01]   | `Color.FromArgb(int) -> Color`                | static   | unpack a 32-bit ARGB word       |
|  [02]   | `Color.FromArgb(int, int, int) -> Color`      | static   | opaque colour from RGB channels |
|  [03]   | `Color.FromArgb(int, int, int, int) -> Color` | static   | colour from alpha and RGB       |
|  [04]   | `Color.FromArgb(int, Color) -> Color`         | static   | restate alpha on a base colour  |
|  [05]   | `Color.ToArgb() -> int`                       | instance | pack to a 32-bit ARGB word      |
|  [06]   | `Color.Empty`                                 | static   | absent-colour sentinel          |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Encoder-parameter carriers and the `Heif`/`Webp` format values resolve against the package build; the RhinoWIP host build omits both.
- Native-backed carriers implement `IDisposable`, so a boundary member returning one transfers the handle and its receiver disposes it.

[STACKING]:
- `Rhino.Display`(`Rasm.Rhino/.api/api-rhinocommon-display.md`): `ViewCapture.CaptureToBitmap` hands back the `Bitmap` this surface owns and `DisplayBitmap(Bitmap)` takes one straight back as a sprite texture, so capture and GPU upload share one carrier with no intermediate encode.
- `Rhino.UI`(`Rasm.Rhino/.api/api-rhino-ui.md`): `DrawingUtilities.BitmapFromSvg`, `IconFromResource`, and `CreateMeshPreviewImage` mint `Bitmap` and `Icon` values that host chrome consumes unconverted.
- Within-library: a raster pass pins once through `Bitmap.LockBits` and walks `BitmapData.Scan0` by `Stride` for the whole window; a draw pass brackets every transform, clip, and quality mutation between `Graphics.Save` and `Graphics.Restore` so one context serves an entire overlay.

[LOCAL_ADMISSION]:
- Boundary code names a `System.Drawing` type where a host member declares it, and converts to the kernel carrier at that same boundary.

[RAIL_LAW]:
- Package: `System.Drawing.Common`
- Owns: compile-time `System.Drawing` raster, icon, context, path, transform, and imaging type names for the Rhino host boundary
- Accept: host signatures declaring these carriers, the pinned-pixel pass over them, and the encoder-parameter save
- Reject: runtime drawing execution, cross-platform image processing, and per-pixel raster walks one pinned `BitmapData` pass replaces

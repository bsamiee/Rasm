# [RASM_APPUI_API_SKIASHARP]

`SkiaSharp` is the raster/2D-vector kernel the AppUi visual rails compose: canvases, paints, paths, paints, images, animated codecs, color-managed surfaces, shaders, runtime-SkSL effects, picture recording, text/typeface shaping seams, and GPU backends. Every render, capture, drafting, evidence, and chart-custom surface draws through Skia; the package is the single owner of pixel ownership and the `SKObject` native-lifecycle discipline.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `SkiaSharp`
- package: `SkiaSharp` `3.119.4`
- assembly: `SkiaSharp` (bound asset `lib/net10.0/SkiaSharp.dll`; multi-target package also ships `net10.0-{macos26.2,ios26.2,maccatalyst26.2,tvos26.2,android36.0,tizen10.0,windows10.0.19041}` — `net10.0` is the AppUi-consumed surface)
- license: MIT
- namespace: `SkiaSharp`
- runtime: managed P/Invoke over the per-platform `libSkiaSharp` native payload supplied by `SkiaSharp.NativeAssets.*` (see `api-skia-native.md`); every type below is a managed binding, the pixels live in unmanaged memory
- rail: visuals

## [02]-[PUBLIC_TYPES]

[DRAWING_TYPES]: canvas, paint, path, and geometry value owners — rail: visuals

| [INDEX] | [SYMBOL]               | [KIND]                                               |
| :-----: | :--------------------- | :--------------------------------------------------- |
|  [01]   | `SKCanvas`             | draw target + clip + matrix stack                    |
|  [02]   | `SKPaint`              | composition state: shader/filter/effect/blend/stroke |
|  [03]   | `SKPath`               | vector path with boolean `Op`, SVG codec, transform  |
|  [04]   | `SKPathMeasure`        | arc-length sampling: position/tangent/matrix/segment |
|  [05]   | `SKRoundRect`          | per-corner rounded rect (clip + draw + difference)   |
|  [06]   | `SKRect` / `SKRectI`   | float / integer bounds value                         |
|  [07]   | `SKPoint` / `SKPointI` | point value                                          |
|  [08]   | `SKSize` / `SKSizeI`   | size value                                           |
|  [09]   | `SKMatrix`             | 3x3 affine transform value                           |
|  [10]   | `SKMatrix44`           | 4x4 transform for `Concat`/`SetMatrix` perspective   |
|  [11]   | `SKSamplingOptions`    | filter/mipmap/cubic/anisotropic resample value       |
|  [12]   | `SKCanvasSaveLayerRec` | layer-save record: bounds, paint, backdrop, flags    |
|  [13]   | `SKSurfaceProperties`  | pixel-geometry + flags for surface allocation        |

[SURFACE_AND_IMAGE_TYPES]: pixel ownership, recording, and document output — rail: visuals

| [INDEX] | [SYMBOL]                                     | [KIND]                                                   |
| :-----: | :------------------------------------------- | :------------------------------------------------------- |
|  [01]   | `SKSurface`                                  | draw target backed by raster or GPU memory               |
|  [02]   | `SKImage`                                    | immutable snapshot; raster<->texture transfer + encode   |
|  [03]   | `SKBitmap`                                   | mutable CPU pixels: decode, resize, install, peek        |
|  [04]   | `SKPixmap`                                   | typed view over raw pixel memory                         |
|  [05]   | `SKImageInfo`                                | width/height/`SKColorType`/`SKAlphaType`/`SKColorSpace`  |
|  [06]   | `SKCodec`                                    | streaming decoder: animation frames + incremental decode |
|  [07]   | `SKData`                                     | ref-counted immutable byte buffer                        |
|  [08]   | `SKPicture`                                  | recorded, replayable draw-op list                        |
|  [09]   | `SKPictureRecorder`                          | records canvas ops into an `SKPicture`                   |
|  [10]   | `SKDrawable`                                 | deferred custom draw object (recorded or replayed)       |
|  [11]   | `SKDocument`                                 | multi-page PDF/XPS sink                                  |
|  [12]   | `SKVertices`                                 | triangle/strip mesh for `DrawVertices`                   |
|  [13]   | `SKStream` / `SKWStream` / `SKManagedStream` | native I/O stream adapters                               |

[TEXT_AND_FONT_TYPES]: typeface, font, and shaped-text seam — rail: visuals

| [INDEX] | [SYMBOL]            | [KIND]                                                |
| :-----: | :------------------ | :---------------------------------------------------- |
|  [01]   | `SKFont`            | sized font: measure, glyph paths, positions, break    |
|  [02]   | `SKTypeface`        | font face (file/stream/family resolved)               |
|  [03]   | `SKFontManager`     | system font registry + `MatchCharacter` fallback      |
|  [04]   | `SKFontStyleSet`    | weight/width/slant variants of one family             |
|  [05]   | `SKFontStyle`       | weight/width/slant value (`SKFontStyleWeight` etc.)   |
|  [06]   | `SKFontMetrics`     | ascent/descent/leading/cap/x-height                   |
|  [07]   | `SKTextBlob`        | immutable positioned glyph run set                    |
|  [08]   | `SKTextBlobBuilder` | builds blobs via `AddRun`/`AllocateRun` glyph buffers |
|  [09]   | `SKTextEncoding`    | UTF8/UTF16/UTF32/GlyphId encoding selector            |

[PAINT_PIPELINE_TYPES]: color, shader, filter, effect, and runtime-SkSL surfaces — rail: visuals

| [INDEX] | [SYMBOL]                      | [KIND]                                          |
| :-----: | :---------------------------- | :---------------------------------------------- |
|  [01]   | `SKColor`                     | 8-bit ARGB; `Parse`/`FromHsl`/`FromHsv`/`With*` |
|  [02]   | `SKColorF`                    | float RGBA color (wide-gamut/HDR)               |
|  [03]   | `SKColors`                    | named color constants                           |
|  [04]   | `SKColorSpace`                | sRGB/linear/Rgb/`CreateIcc` color space         |
|  [05]   | `SKColorSpaceIccProfile`      | parsed ICC profile for `CreateIcc`              |
|  [06]   | `SKColorSpaceXyz`             | ICC XYZ primaries matrix                        |
|  [07]   | `SKColorSpaceTransferFn`      | parametric transfer curve                       |
|  [08]   | `SKShader`                    | gradient/image/picture/noise/blend paint source |
|  [09]   | `SKBlender`                   | custom blend object (paired with `SKBlendMode`) |
|  [10]   | `SKImageFilter`               | DAG image filter (compose/matrix/tile/picture)  |
|  [11]   | `SKColorFilter`               | per-pixel color transform                       |
|  [12]   | `SKPathEffect`                | geometry effect: dash/corner/discrete/trim/sum  |
|  [13]   | `SKMaskFilter`                | coverage-mask effect: blur/clip/gamma           |
|  [14]   | `SKRuntimeEffect`             | compiled SkSL shader/colorfilter/blender        |
|  [15]   | `SKRuntimeShaderBuilder`      | uniform/child binding for an SkSL shader        |
|  [16]   | `SKRuntimeColorFilterBuilder` | uniform/child binding for an SkSL color filter  |
|  [17]   | `SKRuntimeBlenderBuilder`     | uniform/child binding for an SkSL blender       |

[CODEC_AND_FORMAT_TYPES]: format, pixel layout, blend, and sampling enums — rail: visuals

| [INDEX] | [SYMBOL]                          | [KIND]                                                |
| :-----: | :-------------------------------- | :---------------------------------------------------- |
|  [01]   | `SKEncodedImageFormat`            | Png/Jpeg/Webp/Avif/Heif/Gif/Bmp/Ico/Dng/Ktx/Pkm/Astc  |
|  [02]   | `SKColorType`                     | pixel layout (`PlatformColorType` for native default) |
|  [03]   | `SKAlphaType`                     | opaque/premul/unpremul                                |
|  [04]   | `SKBlendMode`                     | Porter-Duff + separable blend modes                   |
|  [05]   | `SKShaderTileMode`                | clamp/repeat/mirror/decal gradient tiling             |
|  [06]   | `SKFilterMode`                    | nearest/linear sampling                               |
|  [07]   | `SKMipmapMode`                    | none/nearest/linear mip selection                     |
|  [08]   | `SKClipOperation`                 | intersect/difference clip combine                     |
|  [09]   | `SKPathOp`                        | difference/intersect/union/xor/reverse-difference     |
|  [10]   | `SKCodecResult`                   | decode-step status for `SKCodec`                      |
|  [11]   | `SKPaintStyle`                    | `Fill` / `Stroke` / `StrokeAndFill` (`SKPaint.Style`) |
|  [12]   | `SKPathDirection`                 | `Clockwise` / `CounterClockwise` add-shape winding    |
|  [13]   | `SKPathArcSize` / `SKPathAddMode` | arc large/small + `Append`/`Extend` path-append mode  |

[GPU_TYPES]: GPU context and backend handles — rail: visuals

| [INDEX] | [SYMBOL]                | [KIND]                                                |
| :-----: | :---------------------- | :---------------------------------------------------- |
|  [01]   | `GRContext`             | GPU-backed resource context (GL/Vulkan/Metal/D3D)     |
|  [02]   | `GRRecordingContext`    | base recording context (`SKSurface`/`SKImage` source) |
|  [03]   | `GRContextOptions`      | cache/precompile/threading tuning at context create   |
|  [04]   | `GRBackendRenderTarget` | externally-owned framebuffer target                   |
|  [05]   | `GRBackendTexture`      | externally-owned texture (import/adopt)               |
|  [06]   | `GRGlInterface`         | OpenGL/ANGLE function-pointer interface               |
|  [07]   | `GRMtlBackendContext`   | Metal device/queue backend                            |
|  [08]   | `GRVkBackendContext`    | Vulkan instance/device backend                        |
|  [09]   | `GRD3DBackendContext`   | Direct3D 12 device/queue backend                      |
|  [10]   | `GRSurfaceOrigin`       | top-left/bottom-left framebuffer origin               |

## [03]-[ENTRYPOINTS]

[CANVAS_ENTRYPOINTS]: draw, layer, clip, and transform — rail: visuals

| [INDEX] | [SURFACE]                                  | [SURFACE_ROOT] | [CALL_SHAPE_NOTE]                                                   |
| :-----: | :----------------------------------------- | :------------- | :------------------------------------------------------------------ |
|  [01]   | `DrawPath`                                 | `SKCanvas`     | `(SKPath, SKPaint)` — fill/stroke per paint `Style`                 |
|  [02]   | `DrawRoundRect`                            | `SKCanvas`     | `(SKRoundRect, SKPaint)` per-corner rounding                        |
|  [03]   | `DrawRoundRectDifference`                  | `SKCanvas`     | `(outer, inner, SKPaint)` — frame/ring fill                         |
|  [04]   | `DrawImage`                                | `SKCanvas`     | `(SKImage, SKRect src, SKRect dst, SKSamplingOptions, SKPaint)`     |
|  [05]   | `DrawImageLattice`                         | `SKCanvas`     | `(SKImage, SKLattice, SKRect dst, SKFilterMode)` 9-slice            |
|  [06]   | `DrawPicture`                              | `SKCanvas`     | `(SKPicture, in SKMatrix, SKPaint)` replay recorded ops             |
|  [07]   | `DrawDrawable`                             | `SKCanvas`     | `(SKDrawable, in SKMatrix)` deferred custom draw                    |
|  [08]   | `DrawVertices`                             | `SKCanvas`     | `(SKVertices, SKBlendMode, SKPaint)` mesh fill                      |
|  [09]   | `DrawAtlas`                                | `SKCanvas`     | sprite batch with `SKRotationScaleMatrix[]` + `SKSamplingOptions`   |
|  [10]   | `DrawTextBlob`                             | `SKCanvas`     | `(SKTextBlob, x, y, SKPaint)` shaped-glyph draw                     |
|  [11]   | `DrawArc`                                  | `SKCanvas`     | `(oval, start, sweep, useCenter, SKPaint)`                          |
|  [12]   | `DrawColor`                                | `SKCanvas`     | `(SKColor, SKBlendMode)` full-clip fill                             |
|  [13]   | `SaveLayer`                                | `SKCanvas`     | `(in SKCanvasSaveLayerRec)` — backdrop-filter/offscreen group       |
|  [14]   | `Save` / `Restore` / `RestoreToCount`      | `SKCanvas`     | matrix+clip stack; `SaveCount` reads depth                          |
|  [15]   | `ClipPath` / `ClipRoundRect` / `ClipRect`  | `SKCanvas`     | `(geom, SKClipOperation, antialias)`                                |
|  [16]   | `Concat` / `SetMatrix`                     | `SKCanvas`     | `(in SKMatrix)` or `(in SKMatrix44)` perspective                    |
|  [17]   | `Translate`/`Scale`/`RotateDegrees`/`Skew` | `SKCanvas`     | matrix mutators                                                     |
|  [18]   | `DrawAnnotation` / `DrawUrlAnnotation`     | `SKCanvas`     | PDF link/named-destination annotations                              |
|  [19]   | `DrawLine`                                 | `SKCanvas`     | `(SKPoint, SKPoint, SKPaint)` / `(x0, y0, x1, y1, SKPaint)` segment |

[PATH_CONSTRUCTION_ENTRYPOINTS]: `SKPath` contour building, shape adds, and transform — rail: visuals
- surface-root: `SKPath` (`new SKPath()` empty; `new SKPath(SKPath)` copy)

| [INDEX] | [SURFACE]                           | [CALL]                                                                      |
| :-----: | :---------------------------------- | :-------------------------------------------------------------------------- |
|  [01]   | `MoveTo` / `LineTo`                 | `(SKPoint)` or `(float x, float y)` contour/line                            |
|  [02]   | `QuadTo` / `ConicTo` / `CubicTo`    | quadratic/rational-quadratic/cubic Bézier segments                          |
|  [03]   | `ArcTo`                             | `(SKRect, startAngle, sweepAngle, forceMoveTo)`                             |
|  [04]   | `ArcTo`                             | radius and `SKPathArcSize` + `SKPathDirection` overloads                    |
|  [05]   | `Close`                             | closes the current contour                                                  |
|  [06]   | `AddRect` / `AddOval` / `AddCircle` | `SKPathDirection direction = Clockwise` shape adds                          |
|  [07]   | `AddRoundRect`                      | `(SKRoundRect, direction)`; construct via `new SKRoundRect(SKRect, rx, ry)` |
|  [08]   | `AddRoundRect`                      | `(SKRect, rx, ry, direction)`                                               |
|  [09]   | `AddPoly`                           | `(ReadOnlySpan<SKPoint>, bool close)` polyline                              |
|  [10]   | `AddPath`                           | `(SKPath, SKPathAddMode mode = Append)`                                     |
|  [11]   | `AddPath`                           | `(SKPath, dx, dy, mode)` / `(SKPath, in SKMatrix, mode)`                    |
|  [12]   | `Transform`                         | `(in SKMatrix[, SKPath dst])` affine transform                              |
|  [13]   | `Offset`                            | `(SKPoint)` or `(dx, dy)` translation                                       |
|  [14]   | `Reset` / `Rewind`                  | clears contours / clears while retaining allocation                         |

[SURFACE_IMAGE_ENTRYPOINTS]: surface allocation, snapshot, codec, and pixel transfer — rail: visuals

| [INDEX] | [SURFACE]                          | [ROOT]               | [CALL]                                                                 |
| :-----: | :--------------------------------- | :------------------- | :--------------------------------------------------------------------- |
|  [01]   | `Create`                           | `SKSurface`          | `(SKImageInfo)` raster                                                 |
|  [02]   | `Create`                           | `SKSurface`          | `(GRRecordingContext, budgeted, info, samples, origin, props)` GPU     |
|  [03]   | `Snapshot`                         | `SKSurface`          | `()` -> immutable `SKImage`; zero-copy where possible                  |
|  [04]   | `BeginRecording`                   | `SKPictureRecorder`  | `(SKRect cull)` -> `SKCanvas`                                          |
|  [05]   | `EndRecording`                     | `SKPictureRecorder`  | commits an `SKPicture`                                                 |
|  [06]   | `FromEncodedData`                  | `SKImage`            | `(ReadOnlySpan<byte>)` / `(SKData)` / `(Stream)` decode                |
|  [07]   | `FromBitmap`                       | `SKImage`            | `(SKBitmap)` snapshot                                                  |
|  [08]   | `FromPixelCopy`                    | `SKImage`            | `(SKImageInfo, ReadOnlySpan<byte>)` copy                               |
|  [09]   | `FromPixels`                       | `SKImage`            | `(SKPixmap, releaseProc)` adopt                                        |
|  [10]   | `ToTextureImage` / `ToRasterImage` | `SKImage`            | `(GRContext, mipmapped, budgeted)` upload/download                     |
|  [11]   | `ApplyImageFilter`                 | `SKImage`            | `(GRContext?, SKImageFilter, subset, clip, out outSubset, out offset)` |
|  [12]   | `Encode`                           | `SKImage`            | `()` PNG or `(SKEncodedImageFormat, quality)` -> `SKData`              |
|  [13]   | `ReadPixels` / `ScalePixels`       | `SKImage`/`SKPixmap` | `(SKPixmap, SKSamplingOptions)` GPU/CPU readback                       |
|  [14]   | `Decode`                           | `SKBitmap`           | `(SKCodec)` / `(SKData)` / `(byte[], SKImageInfo)`                     |
|  [15]   | `Resize`                           | `SKBitmap`           | `(SKImageInfo, SKSamplingOptions)`                                     |
|  [16]   | `InstallPixels` / `PeekPixels`     | `SKBitmap`           | adopts pixels / exposes a view                                         |
|  [17]   | `Create`                           | `SKCodec`            | `(SKStream)` / `(Stream, out SKCodecResult)`                           |
|  [18]   | `FrameCount` / `RepetitionCount`   | `SKCodec`            | animated-image frame metadata                                          |
|  [19]   | `GetFrameInfo`                     | `SKCodec`            | returns `SKCodecFrameInfo`                                             |
|  [20]   | `StartIncrementalDecode`           | `SKCodec`            | starts progressive decode                                              |
|  [21]   | `IncrementalDecode`                | `SKCodec`            | continues decode with `out int rowsDecoded`                            |
|  [22]   | `Create`                           | `SKVertices`         | `CreateCopy(SKVertexMode, SKPoint[], SKPoint[] texs, SKColor[])`       |

[DOCUMENT_AND_COLOR_ENTRYPOINTS]: paged export and color-managed reproject — rail: visuals

| [INDEX] | [SURFACE]                         | [ROOT]                   | [CALL]                                                |
| :-----: | :-------------------------------- | :----------------------- | :---------------------------------------------------- |
|  [01]   | `CreatePdf`                       | `SKDocument`             | `(Stream, SKDocumentPdfMetadata)` with PDF/A metadata |
|  [02]   | `CreateXps`                       | `SKDocument`             | `(Stream, dpi)` Windows-only backend                  |
|  [03]   | `BeginPage` / `EndPage`           | `SKDocument`             | `(width, height, SKRect content)` -> `SKCanvas`       |
|  [04]   | `Close` / `Abort`                 | `SKDocument`             | finalizes/discards                                    |
|  [05]   | `CreateSrgb` / `CreateSrgbLinear` | `SKColorSpace`           | standard render spaces                                |
|  [06]   | `CreateRgb`                       | `SKColorSpace`           | `(SKColorSpaceTransferFn, SKColorSpaceXyz)`           |
|  [07]   | `CreateIcc`                       | `SKColorSpace`           | `(SKColorSpaceIccProfile)` / `(ReadOnlySpan<byte>)`   |
|  [08]   | `WithColorSpace` / `WithSize`     | `SKImageInfo`            | retags space/resizes the info value                   |
|  [09]   | `Parse` / `TryParse`              | `SKColor`                | `(string hex)` -> `SKColor`                           |
|  [10]   | `FromHsl` / `FromHsv`             | `SKColor`                | constructs HSL/HSV colors                             |
|  [11]   | `ToHsl` / `ToHsv`                 | `SKColor`                | projects HSL/HSV values                               |
|  [12]   | `Equal`                           | `SKColorSpace`           | static space-identity test                            |
|  [13]   | `Srgb` / `Linear` / `TwoDotTwo`   | `SKColorSpaceTransferFn` | named transfer curves for `CreateRgb`                 |
|  [14]   | `Rec2020` / `Pq` / `Hlg`          | `SKColorSpaceTransferFn` | named transfer curves for `CreateRgb`                 |
|  [15]   | `Srgb` / `DisplayP3`              | `SKColorSpaceXyz`        | named primary matrices for `CreateRgb`                |
|  [16]   | `Rec2020` / `AdobeRgb`            | `SKColorSpaceXyz`        | named primary matrices for `CreateRgb`                |

[TEXT_AND_FONT_ENTRYPOINTS]: typeface resolution, measurement, and glyph geometry — rail: visuals

| [INDEX] | [SURFACE]                               | [ROOT]              | [CALL]                                                        |
| :-----: | :-------------------------------------- | :------------------ | :------------------------------------------------------------ |
|  [01]   | `MatchCharacter`                        | `SKFontManager`     | `(family, weight, width, slant, bcp47[], codepoint)` fallback |
|  [02]   | `Default` / `MatchFamily`               | `SKFontManager`     | process registry/family lookup                                |
|  [03]   | `CreateTypeface`                        | `SKFontManager`     | `(Stream, index)` embedded face                               |
|  [04]   | `MeasureText`                           | `SKFont`            | `(string, out SKRect bounds, SKPaint)` advance/ink box        |
|  [05]   | `BreakText`                             | `SKFont`            | `(string, maxWidth, out measuredWidth)` fit                   |
|  [06]   | `GetGlyphs`                             | `SKFont`            | returns glyph IDs                                             |
|  [07]   | `GetGlyphPositions` / `GetGlyphWidths`  | `SKFont`            | returns layout arrays                                         |
|  [08]   | `GetGlyphPath`                          | `SKFont`            | `(ushort glyph)` -> outline `SKPath`                          |
|  [09]   | `AllocateRun` / `AllocatePositionedRun` | `SKTextBlobBuilder` | allocates glyph buffers                                       |
|  [10]   | `AddRun` / `Build`                      | `SKTextBlobBuilder` | fills runs/builds an `SKTextBlob`                             |

[PAINT_PIPELINE_ENTRYPOINTS]: shader, filter, effect, and runtime-SkSL construction — rail: visuals

| [INDEX] | [SURFACE]                                        | [ROOT]            | [CALL]                                            |
| :-----: | :----------------------------------------------- | :---------------- | :------------------------------------------------ |
|  [01]   | `Shader` / `ColorFilter` / `ImageFilter`         | `SKPaint`         | settable pipeline slots composed per draw         |
|  [02]   | `MaskFilter` / `PathEffect`                      | `SKPaint`         | settable pipeline slots composed per draw         |
|  [03]   | `BlendMode` / `Style` / `StrokeWidth`            | `SKPaint`         | composition/stroke state                          |
|  [04]   | `StrokeCap` / `IsAntialias`                      | `SKPaint`         | stroke/rasterization state                        |
|  [05]   | `GetFillPath`                                    | `SKPaint`         | `(SKPath src, SKPath dst)` resolves the outline   |
|  [06]   | `CreateLinearGradient`                           | `SKShader`        | `(start, end, SKColor[], SKShaderTileMode)`       |
|  [07]   | `CreateLinearGradient`                           | `SKShader`        | `SKColorF[]` + `SKColorSpace` wide-gamut overload |
|  [08]   | `CreateRadialGradient` / `CreateSweepGradient`   | `SKShader`        | radial/sweep gradients                            |
|  [09]   | `CreateTwoPointConicalGradient`                  | `SKShader`        | two-point conical gradient                        |
|  [10]   | `CreatePerlinNoiseFractalNoise`                  | `SKShader`        | procedural fractal noise                          |
|  [11]   | `CreatePerlinNoiseTurbulence`                    | `SKShader`        | procedural turbulence                             |
|  [12]   | `CreateImage` / `CreateBitmap` / `CreatePicture` | `SKShader`        | sampled sources with `SKSamplingOptions`          |
|  [13]   | `CreateBlend` / `CreateCompose`                  | `SKShader`        | shader algebra                                    |
|  [14]   | `CreateColorFilter` / `CreateLocalMatrix`        | `SKShader`        | filter/local-matrix composition                   |
|  [15]   | `CreateDash` / `CreateTrim` / `CreateCorner`     | `SKPathEffect`    | stroke geometry effects                           |
|  [16]   | `CreateDiscrete` / `CreateSum` / `CreateCompose` | `SKPathEffect`    | stroke geometry effects                           |
|  [17]   | `CreateBlur` / `CreateClip` / `CreateGamma`      | `SKMaskFilter`    | coverage-mask effects, including soft glow        |
|  [18]   | `CreateColorMatrix` / `CreateBlendMode`          | `SKColorFilter`   | color transforms                                  |
|  [19]   | `CreateLighting` / `CreateHighContrast`          | `SKColorFilter`   | color transforms                                  |
|  [20]   | `CreateLumaColor` / `CreateTable` / `CreateLerp` | `SKColorFilter`   | color transforms                                  |
|  [21]   | `CreateCompose` / `CreateMatrix`                 | `SKImageFilter`   | image-filter DAG nodes                            |
|  [22]   | `CreateImage` / `CreatePicture` / `CreateTile`   | `SKImageFilter`   | image-filter DAG nodes                            |
|  [23]   | `CreateShader` / `CreateColorFilter`             | `SKRuntimeEffect` | `(string sksl, out string errors)` compile        |
|  [24]   | `CreateBlender`                                  | `SKRuntimeEffect` | `(string sksl, out string errors)` compile        |
|  [25]   | `BuildShader` / `BuildColorFilter`               | `SKRuntimeEffect` | returns builders for uniform/child binding        |
|  [26]   | `BuildBlender`                                   | `SKRuntimeEffect` | returns a builder for uniform/child binding       |
|  [27]   | `Uniforms` / `Children`                          | `SKRuntimeEffect` | declared names for binding                        |

[GPU_ENTRYPOINTS]: backend context creation and frame submission — rail: visuals

| [INDEX] | [SURFACE]                         | [ROOT]          | [CALL]                                                         |
| :-----: | :-------------------------------- | :-------------- | :------------------------------------------------------------- |
|  [01]   | `CreateGl`                        | `GRContext`     | `()` / `(GRGlInterface)` / `(GRGlInterface, GRContextOptions)` |
|  [02]   | `CreateVulkan`                    | `GRContext`     | `(GRVkBackendContext, GRContextOptions?)`                      |
|  [03]   | `CreateMetal`                     | `GRContext`     | `(GRMtlBackendContext, GRContextOptions?)`                     |
|  [04]   | `CreateDirect3D`                  | `GRContext`     | `(GRD3DBackendContext, GRContextOptions?)`                     |
|  [05]   | `Create` / `CreateAngle`          | `GRGlInterface` | native GL/ANGLE function-pointer interface                     |
|  [06]   | `Flush` / `Submit`                | `GRContext`     | `(submit, synchronous)` / `(synchronous)` fence                |
|  [07]   | `SetResourceCacheLimit`           | `GRContext`     | configures the GPU cache                                       |
|  [08]   | `PurgeResources` / `ResetContext` | `GRContext`     | purges cache/recovers a lost context                           |
|  [09]   | `Dispose`                         | `SKObject`      | releases surface/image/codec/context/stream state              |

## [04]-[INTEGRATION_STACKING]

[CUSTOM_VISUAL_RAIL]: the canonical AppUi draw rail composes Skia onto the Avalonia backend lease — never a parallel renderer.
- `Avalonia.Skia` `ISkiaSharpApiLeaseFeature.Lease()` yields an `ISkiaSharpApiLease` exposing the live `SkCanvas` (`SKCanvas`), `GrContext` (`GRContext`), and `SkSurface` (`SKSurface`); custom controls draw through that leased canvas so they share Avalonia's GPU context and present in-airspace (`api-avalonia-skia.md`).
- `Avalonia.Skia.SkiaSharpExtensions` bridges `Avalonia`->`SkiaSharp` value types (`ToSKRect`/`ToSKMatrix`/`ToSKColor`/`ToSKSamplingOptions`); AppUi geometry crosses the boundary through those converters, then all interior math is `SKMatrix`/`SKPath`/`SKRect`.
- Text in custom visuals shapes through `SkiaSharp.HarfBuzz` `SKShaper.Shape` (`api-skia-harfbuzz.md`) into an `SKTextBlob`, then draws via `SKCanvas.DrawTextBlob`; `SKFontManager.MatchCharacter` supplies font fallback before shaping. Direct `SKCanvas.DrawText` is reserved for diagnostics where shaping is not required.

[OFFSCREEN_AND_CAPTURE_RAIL]: deterministic raster evidence stacks raster `SKSurface` + `SKImage.Encode` + `SKData`.
- The capture/evidence rails (`Render/capture.md`, `Diagnostics/proof.md`) allocate a raster `SKSurface.Create(SKImageInfo)` (or a GPU surface from `GRRecordingContext` for compositor capture), draw, `Snapshot()` an `SKImage`, and `Encode(SKEncodedImageFormat.Png, …)` to an `SKData` payload — the byte buffer is the diffable visual receipt the bridge/Verify lane asserts on.
- `SKColorSpace.CreateSrgb`/`CreateIcc` + `SKImageInfo.WithColorSpace` make capture color-managed so cross-host evidence is reproducible regardless of platform default `PlatformColorType`.
- Animated/sequence evidence decodes through `SKCodec.FrameCount`/`GetFrameInfo` (one decode per frame) rather than per-format branching.

[DRAFTING_AND_DOC_RAIL]: paged export stacks `SKDocument` + `SKCanvas` + `SKPath` SVG codec.
- Sheet/drafting export (`Render/drafting.md`) drives `SKDocument.CreatePdf(stream, SKDocumentPdfMetadata)` -> per-sheet `BeginPage` -> draw -> `EndPage` -> `Close`, sharing the same paint/path stack as the live visual rail so on-screen and exported geometry are byte-identical.
- `SKPath.ToSvgPathData`/`ParseSvgPathData` and `SKPath.Op(SKPathOp)` (boolean union/difference/intersect) own vector interchange and clip-region math; the DWG/DXF codecs in `api-drafting-export.md` consume the resolved `SKPath` outline, never their own geometry kernel.

[RUNTIME_EFFECT_RAIL]: SkSL effects bind once and re-bind uniforms per frame.
- Shading/theme surfaces (`Render/shading.md`, `Theme/motion.md`) compile an `SKRuntimeEffect.CreateShader(sksl, out errors)` once, then `BuildShader()` an `SKRuntimeShaderBuilder`, set named `Uniforms`/`Children`, and produce an `SKShader` assigned to `SKPaint.Shader` each frame — animation drives uniform values, not recompilation.
- `SKPicture`/`SKPictureRecorder` memoize a static draw-op list once and replay via `SKCanvas.DrawPicture` so repeated overlays (grids, guides, watermarks) cost one record and N cheap replays; `SKDrawable` defers controls that must re-render lazily.

[PAINT_AS_COMPOSITION_POINT]: one `SKPaint` is the single stacking surface for the whole effect pipeline.
- A label-with-soft-shadow-under-a-color-managed-gradient is one paint: `Shader` = `SKShader.CreateLinearGradient(... SKColorF[], SKColorSpace ...)`, `MaskFilter` = `SKMaskFilter.CreateBlur(...)` for the glow, `ColorFilter` for tone, `BlendMode` for the layer, drawn via `SaveLayer(in SKCanvasSaveLayerRec)` for the offscreen group. AppUi composes effects onto the paint rather than fanning per-effect draw passes.

## [05]-[IMPLEMENTATION_LAW]

[VISUALS_LAW]:
- Package: `SkiaSharp`
- Owns: raster + 2D-vector drawing, offscreen/GPU surfaces, animated codecs, picture recording, paged document export, color-managed spaces, the full shader/filter/effect/runtime-SkSL pipeline, and GPU backend contexts
- Accept: custom visuals draw through a leased `SKCanvas` and emit deterministic `SKImage`/`SKData` evidence; effects compose onto one `SKPaint`; text shapes through HarfBuzz before it draws
- Reject: GDI public vocabulary; parallel render backends bypassing the Avalonia lease; per-effect draw fan-out where one paint composes the pipeline; `SKFilterQuality` (deprecated — use `SKSamplingOptions`); phantom `SKImageFilter.CreateBlur` (mask blur is `SKMaskFilter.CreateBlur`)

[ASSET_LAW]:
- Package: `SkiaSharp`
- Owns: managed bindings over native-backed disposable objects and explicit pixel ownership
- Accept: every `SKSurface`/`SKImage`/`SKBitmap`/`SKCodec`/`SKData`/`GRContext`/`SKStream` is lifecycle-scoped (`using`/explicit `Dispose`); the native `libSkiaSharp` payload arrives from `SkiaSharp.NativeAssets.*`
- Reject: ambient unmanaged ownership; documenting native payload identity here (it belongs to `api-skia-native.md`)

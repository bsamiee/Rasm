# [RASM_APPUI_API_SKIASHARP]

`SkiaSharp` owns the AppUi raster and 2D-vector drawing kernel: every render, capture, drafting, and evidence surface draws through a Skia canvas onto raster or GPU memory, and `SkiaSharp` holds pixel ownership and the `SKObject` native-lifecycle discipline where each managed binding is a P/Invoke shim over unmanaged pixels the `libSkiaSharp` payload backs. One paint and one canvas compose the whole shader, filter, runtime-SkSL, picture-recording, and paged-document pipeline, feeding the visuals rail.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `SkiaSharp`
- package: `SkiaSharp` (MIT)
- assembly: `SkiaSharp` (bound `lib/net10.0/SkiaSharp.dll`; managed P/Invoke binding, multi-targeted package)
- namespace: `SkiaSharp`
- runtime: managed shim over the per-platform `libSkiaSharp` native payload from `SkiaSharp.NativeAssets.*` (`api-skia-native.md`); pixels live in unmanaged memory
- rail: visuals

## [02]-[PUBLIC_TYPES]

[DRAWING_TYPES]: canvas, paint, path, and geometry value owners

| [INDEX] | [SYMBOL]               | [CAPABILITY]                                         |
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

[SURFACE_AND_IMAGE_TYPES]: pixel ownership, recording, and document output

| [INDEX] | [SYMBOL]                                     | [CAPABILITY]                                             |
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

[TEXT_AND_FONT_TYPES]: typeface, font, and shaped-text seam

| [INDEX] | [SYMBOL]            | [CAPABILITY]                                          |
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

[PAINT_PIPELINE_TYPES]: color, shader, filter, effect, and runtime-SkSL surfaces

| [INDEX] | [SYMBOL]                      | [CAPABILITY]                                    |
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

[CODEC_AND_FORMAT_TYPES]: format, pixel layout, blend, and sampling enums

| [INDEX] | [SYMBOL]                          | [CAPABILITY]                                          |
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

[GPU_TYPES]: GPU context and backend handles

| [INDEX] | [SYMBOL]                | [CAPABILITY]                                          |
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

[CANVAS_ENTRYPOINTS]: draw, layer, clip, and transform
- root: `SKCanvas`

| [INDEX] | [SURFACE]                                  | [CALL]                                                              |
| :-----: | :----------------------------------------- | :------------------------------------------------------------------ |
|  [01]   | `DrawPath`                                 | `(SKPath, SKPaint)` — fill/stroke per paint `Style`                 |
|  [02]   | `DrawRoundRect`                            | `(SKRoundRect, SKPaint)` per-corner rounding                        |
|  [03]   | `DrawRoundRectDifference`                  | `(outer, inner, SKPaint)` — frame/ring fill                         |
|  [04]   | `DrawImage`                                | `(SKImage, SKRect src, SKRect dst, SKSamplingOptions, SKPaint)`     |
|  [05]   | `DrawImageLattice`                         | `(SKImage, SKLattice, SKRect dst, SKFilterMode)` 9-slice            |
|  [06]   | `DrawPicture`                              | `(SKPicture, in SKMatrix, SKPaint)` replay recorded ops             |
|  [07]   | `DrawDrawable`                             | `(SKDrawable, in SKMatrix)` deferred custom draw                    |
|  [08]   | `DrawVertices`                             | `(SKVertices, SKBlendMode, SKPaint)` mesh fill                      |
|  [09]   | `DrawAtlas`                                | sprite batch with `SKRotationScaleMatrix[]` + `SKSamplingOptions`   |
|  [10]   | `DrawTextBlob`                             | `(SKTextBlob, x, y, SKPaint)` shaped-glyph draw                     |
|  [11]   | `DrawArc`                                  | `(oval, start, sweep, useCenter, SKPaint)`                          |
|  [12]   | `DrawColor`                                | `(SKColor, SKBlendMode)` full-clip fill                             |
|  [13]   | `SaveLayer`                                | `(in SKCanvasSaveLayerRec)` — backdrop-filter/offscreen group       |
|  [14]   | `Save` / `Restore` / `RestoreToCount`      | matrix+clip stack; `SaveCount` reads depth                          |
|  [15]   | `ClipPath` / `ClipRoundRect` / `ClipRect`  | `(geom, SKClipOperation, antialias)`                                |
|  [16]   | `Concat` / `SetMatrix`                     | `(in SKMatrix)` or `(in SKMatrix44)` perspective                    |
|  [17]   | `Translate`/`Scale`/`RotateDegrees`/`Skew` | matrix mutators                                                     |
|  [18]   | `DrawAnnotation` / `DrawUrlAnnotation`     | PDF link/named-destination annotations                              |
|  [19]   | `DrawLine`                                 | `(SKPoint, SKPoint, SKPaint)` / `(x0, y0, x1, y1, SKPaint)` segment |

[PATH_CONSTRUCTION_ENTRYPOINTS]: `SKPath` contour building, shape adds, and transform
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

[SURFACE_IMAGE_ENTRYPOINTS]: surface allocation, snapshot, codec, and pixel transfer

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
|  [22]   | `CreateCopy`                       | `SKVertices`         | `(SKVertexMode, SKPoint[], SKPoint[] texs, SKColor[])`                 |

[DOCUMENT_AND_COLOR_ENTRYPOINTS]: paged export and color-managed reproject

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

[TEXT_AND_FONT_ENTRYPOINTS]: typeface resolution, measurement, and glyph geometry

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

[PAINT_PIPELINE_ENTRYPOINTS]: shader, filter, effect, and runtime-SkSL construction

| [INDEX] | [SURFACE]                                        | [ROOT]            | [CALL]                                                           |
| :-----: | :----------------------------------------------- | :---------------- | :--------------------------------------------------------------- |
|  [01]   | `Shader` / `ColorFilter` / `ImageFilter`         | `SKPaint`         | settable pipeline slots composed per draw                        |
|  [02]   | `MaskFilter` / `PathEffect`                      | `SKPaint`         | settable pipeline slots composed per draw                        |
|  [03]   | `BlendMode` / `Style` / `StrokeWidth`            | `SKPaint`         | composition/stroke state                                         |
|  [04]   | `StrokeCap` / `IsAntialias`                      | `SKPaint`         | stroke/rasterization state                                       |
|  [05]   | `GetFillPath`                                    | `SKPaint`         | `(SKPath src, SKPath dst)` resolves the outline                  |
|  [06]   | `GetFastBounds`                                  | `SKPaint`         | `(SKRect, out SKRect) -> bool` quick-reject cull                 |
|  [07]   | `CreateLinearGradient`                           | `SKShader`        | `(start, end, SKColor[], SKShaderTileMode)`                      |
|  [08]   | `CreateLinearGradient`                           | `SKShader`        | `SKColorF[]` + `SKColorSpace` wide-gamut overload                |
|  [09]   | `CreateRadialGradient` / `CreateSweepGradient`   | `SKShader`        | radial/sweep gradients                                           |
|  [10]   | `CreateTwoPointConicalGradient`                  | `SKShader`        | two-point conical gradient                                       |
|  [11]   | `CreatePerlinNoiseFractalNoise`                  | `SKShader`        | procedural fractal noise                                         |
|  [12]   | `CreatePerlinNoiseTurbulence`                    | `SKShader`        | procedural turbulence                                            |
|  [13]   | `CreateImage` / `CreateBitmap` / `CreatePicture` | `SKShader`        | sampled sources with `SKSamplingOptions`                         |
|  [14]   | `CreateBlend` / `CreateCompose`                  | `SKShader`        | shader algebra                                                   |
|  [15]   | `CreateColorFilter` / `CreateLocalMatrix`        | `SKShader`        | filter/local-matrix composition                                  |
|  [16]   | `CreateDash` / `CreateTrim` / `CreateCorner`     | `SKPathEffect`    | stroke geometry effects                                          |
|  [17]   | `CreateDiscrete` / `CreateSum` / `CreateCompose` | `SKPathEffect`    | stroke geometry effects                                          |
|  [18]   | `CreateBlur` / `CreateClip` / `CreateGamma`      | `SKMaskFilter`    | coverage-mask effects, including soft glow                       |
|  [19]   | `CreateColorMatrix` / `CreateBlendMode`          | `SKColorFilter`   | color transforms                                                 |
|  [20]   | `CreateLighting` / `CreateHighContrast`          | `SKColorFilter`   | color transforms                                                 |
|  [21]   | `CreateLumaColor` / `CreateTable` / `CreateLerp` | `SKColorFilter`   | color transforms                                                 |
|  [22]   | `CreateOverdraw`                                 | `SKColorFilter`   | `(ReadOnlySpan<SKColor>)` overdraw-count heatmap                 |
|  [23]   | `CreateCompose` / `CreateMatrix`                 | `SKImageFilter`   | image-filter DAG nodes                                           |
|  [24]   | `CreateImage` / `CreatePicture` / `CreateTile`   | `SKImageFilter`   | image-filter DAG nodes                                           |
|  [25]   | `CreateCrop` / `CreateEmpty`                     | `SKImageFilter`   | `(SKRect, SKShaderTileMode, SKImageFilter?)` / `()` crop / empty |
|  [26]   | `CreateShader` / `CreateColorFilter`             | `SKRuntimeEffect` | `(string sksl, out string errors)` compile                       |
|  [27]   | `CreateBlender`                                  | `SKRuntimeEffect` | `(string sksl, out string errors)` compile                       |
|  [28]   | `BuildShader` / `BuildColorFilter`               | `SKRuntimeEffect` | returns builders for uniform/child binding                       |
|  [29]   | `BuildBlender`                                   | `SKRuntimeEffect` | returns a builder for uniform/child binding                      |
|  [30]   | `Uniforms` / `Children`                          | `SKRuntimeEffect` | declared names for binding                                       |

[GPU_ENTRYPOINTS]: backend context creation and frame submission

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

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Every render, capture, drafting, and evidence op draws through an `SKCanvas` — leased from the Avalonia backend on the live path, allocated from a raster or GPU `SKSurface` off it.
- Every `SKObject` (`SKSurface`/`SKImage`/`SKBitmap`/`SKCodec`/`SKData`/`GRContext`/`SKStream`) is one lifecycle-scoped disposable joining a managed binding to its unmanaged `libSkiaSharp` handle, freed by `using` or explicit `Dispose`; `SKSurface.Canvas` yields a surface-owned cached `SKCanvas` — stable across reads, never disposed by the caller.
- One `SKPaint` composes the whole effect pipeline: a shadowed, tone-mapped, gradient-filled draw sets `Shader`/`MaskFilter`/`ColorFilter`/`BlendMode` on one paint and draws once through `SaveLayer(in SKCanvasSaveLayerRec)`.

[STACKING]:
- `Avalonia.Skia`(`api-avalonia-skia.md`): `ISkiaSharpApiLeaseFeature.Lease()` yields the live `SKCanvas`/`GRContext`/`SKSurface` a custom control draws through, sharing Avalonia's GPU context and presenting in-airspace; `SkiaSharpExtensions.ToSKRect`/`ToSKMatrix`/`ToSKColor`/`ToSKSamplingOptions` bridge Avalonia value types at the boundary, interior math staying `SKMatrix`/`SKPath`/`SKRect`.
- `SkiaSharp.HarfBuzz`(`api-skia-harfbuzz.md`): custom-visual text shapes through `SKShaper.Shape` into an `SKTextBlob` drawn via `SKCanvas.DrawTextBlob`, with `SKFontManager.MatchCharacter` supplying fallback before shaping; `SKCanvas.DrawText` serves only shaping-free diagnostics.
- `SkiaSharp.NativeAssets.*`(`api-skia-native.md`): `libSkiaSharp` backs every `SKObject` from a per-platform payload, faulting at first draw on a missing or wrong-RID asset rather than at compile.
- `api-drafting-export.md`: `DWG`/`DXF` codecs consume the resolved `SKPath` outline from `SKPath.Op(SKPathOp)` and `ToSvgPathData`/`ParseSvgPathData`, never a private geometry kernel.
- Capture rail: `SKSurface.Create(SKImageInfo)` (or a GPU surface from `GRRecordingContext`) draws, `Snapshot()` an `SKImage`, and `Encode(SKEncodedImageFormat.Png, ...)` to an `SKData` byte buffer as the diffable receipt; `SKColorSpace.CreateSrgb`/`CreateIcc` + `SKImageInfo.WithColorSpace` make it color-managed, and animated evidence decodes through `SKCodec.FrameCount`/`GetFrameInfo` per frame.
- Paged export: `SKDocument.CreatePdf(stream, SKDocumentPdfMetadata)` -> per-sheet `BeginPage`/draw/`EndPage` -> `Close`, sharing the live rail's paint/path stack so on-screen and exported geometry are byte-identical.
- Runtime effects: `SKRuntimeEffect.CreateShader(sksl, out errors)` compiles once, `BuildShader()` yields an `SKRuntimeShaderBuilder`, and animation re-binds named `Uniforms`/`Children` per frame; `SKPictureRecorder`/`SKPicture` memoize a static draw-op list `SKCanvas.DrawPicture` replays N times, and `SKDrawable` defers a lazily re-rendering control.

[LOCAL_ADMISSION]:
- A custom visual draws through the leased `SKCanvas`, composes every effect onto one `SKPaint`, and emits deterministic `SKImage`/`SKData` bytes as its visual evidence; color-managed capture retags through `SKImageInfo.WithColorSpace` so evidence reproduces across host color defaults.

[RAIL_LAW]:
- Package: `SkiaSharp`
- Owns: raster and 2D-vector drawing, offscreen and GPU surfaces, animated codecs, picture recording, paged-document export, color-managed spaces, the shader/filter/effect/runtime-SkSL pipeline, and the GPU backend contexts
- Accept: custom visuals draw through a leased `SKCanvas`; effects compose onto one `SKPaint`; text shapes through HarfBuzz; capture emits deterministic `SKImage`/`SKData` evidence
- Reject: GDI public vocabulary; a parallel render backend bypassing the Avalonia lease; per-effect draw fan-out where one paint composes the pipeline; a per-draw resample knob where `SKSamplingOptions` owns filter/mipmap/cubic selection

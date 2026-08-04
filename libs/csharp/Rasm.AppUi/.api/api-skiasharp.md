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

| [INDEX] | [SYMBOL]                    | [CAPABILITY]                                          |
| :-----: | :-------------------------- | :---------------------------------------------------- |
|  [01]   | `SKCanvas`                  | draw target + clip + matrix stack                     |
|  [02]   | `SKPaint`                   | composition state: shader/filter/effect/blend/stroke  |
|  [03]   | `SKPath`                    | vector path with boolean `Op`, SVG codec, transform   |
|  [04]   | `SKPathBuilder`             | mutable contour sink; `Detach`/`Snapshot` seal paths  |
|  [05]   | `SKPathMeasure`             | arc-length sampling: position/tangent/matrix/segment  |
|  [06]   | `SKRoundRect`               | per-corner rounded rect (clip + draw + difference)    |
|  [07]   | `SKRect` / `SKRectI`        | float / integer bounds value                          |
|  [08]   | `SKPoint` / `SKPointI`      | point value                                           |
|  [09]   | `SKPoint3`                  | 3D point: light direction/location, `Vector3` convert |
|  [10]   | `SKSize` / `SKSizeI`        | size value                                            |
|  [11]   | `SKMatrix`                  | 3x3 affine transform value                            |
|  [12]   | `SKMatrix44`                | 4x4 transform for `Concat`/`SetMatrix` perspective    |
|  [13]   | `SKRotationScaleMatrix`     | similarity transform value: `SCos`/`SSin`/`TX`/`TY`   |
|  [14]   | `SKSamplingOptions`         | filter/mipmap/cubic/anisotropic resample value        |
|  [15]   | `SKCanvasSaveLayerRec`      | layer-save record: bounds, paint, backdrop, flags     |
|  [16]   | `SKCanvasSaveLayerRecFlags` | layer init/LCD-text/F16 flag set                      |
|  [17]   | `SKSurfaceProperties`       | pixel-geometry + flags for surface allocation         |

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
|  [07]   | `SKColorSpacePrimaries`       | chromaticity octuple -> primaries matrix        |
|  [08]   | `SKColorSpaceTransferFn`      | parametric transfer curve                       |
|  [09]   | `SKShader`                    | gradient/image/picture/noise/blend paint source |
|  [10]   | `SKBlender`                   | custom blend object (paired with `SKBlendMode`) |
|  [11]   | `SKImageFilter`               | DAG image filter (compose/matrix/tile/picture)  |
|  [12]   | `SKColorFilter`               | per-pixel color transform                       |
|  [13]   | `SKPathEffect`                | geometry effect: dash/corner/discrete/trim/sum  |
|  [14]   | `SKMaskFilter`                | coverage-mask effect: blur/clip/gamma           |
|  [15]   | `SKRuntimeEffect`             | compiled SkSL shader/colorfilter/blender        |
|  [16]   | `SKRuntimeShaderBuilder`      | uniform/child binding for an SkSL shader        |
|  [17]   | `SKRuntimeColorFilterBuilder` | uniform/child binding for an SkSL color filter  |
|  [18]   | `SKRuntimeBlenderBuilder`     | uniform/child binding for an SkSL blender       |
|  [19]   | `SKRuntimeEffectUniforms`     | named uniform block bound to one effect         |
|  [20]   | `SKRuntimeEffectChildren`     | named child-effect slots bound to one effect    |
|  [21]   | `SKRuntimeEffectUniform`      | one uniform value, implicit from scalar/vector  |
|  [22]   | `SKRuntimeEffectChild`        | one child slot, implicit from shader/filter     |

[CODEC_AND_FORMAT_TYPES]: format, pixel layout, blend, and sampling enums

| [INDEX] | [SYMBOL]                          | [CAPABILITY]                                                                              |
| :-----: | :-------------------------------- | :---------------------------------------------------------------------------------------- |
|  [01]   | `SKEncodedImageFormat`            | Png/Jpeg/Webp/Avif/Heif/Gif/Bmp/Ico/Dng/Ktx/Pkm/Astc                                      |
|  [02]   | `SKColorType`                     | pixel layout — `PlatformColorType` default, `RgbaF16`/`RgbaF16Clamped`/`RgbaF32` HDR rows |
|  [03]   | `SKAlphaType`                     | opaque/premul/unpremul                                                                    |
|  [04]   | `SKBlendMode`                     | Porter-Duff + separable blend modes                                                       |
|  [05]   | `SKShaderTileMode`                | clamp/repeat/mirror/decal gradient tiling                                                 |
|  [06]   | `SKFilterMode`                    | nearest/linear sampling                                                                   |
|  [07]   | `SKMipmapMode`                    | none/nearest/linear mip selection                                                         |
|  [08]   | `SKClipOperation`                 | intersect/difference clip combine                                                         |
|  [09]   | `SKPathOp`                        | difference/intersect/union/xor/reverse-difference                                         |
|  [10]   | `SKCodecResult`                   | decode-step status for `SKCodec`                                                          |
|  [11]   | `SKPaintStyle`                    | `Fill` / `Stroke` / `StrokeAndFill` (`SKPaint.Style`)                                     |
|  [12]   | `SKPathDirection`                 | `Clockwise` / `CounterClockwise` add-shape winding                                        |
|  [13]   | `SKPathArcSize` / `SKPathAddMode` | arc large/small + `Append`/`Extend` path-append mode                                      |
|  [14]   | `SKBlurStyle`                     | coverage an `SKMaskFilter` blur keeps                                                     |
|  [15]   | `SKColorChannel`                  | `R`/`G`/`B`/`A` selector for displacement mapping                                         |

`[SKBlendMode]`: `Clear=0` `Src` `Dst` `SrcOver` `DstOver` `SrcIn` `DstIn` `SrcOut` `DstOut` `SrcATop` `DstATop` `Xor` `Plus` `Modulate` `Screen` `Overlay` `Darken` `Lighten` `ColorDodge` `ColorBurn` `HardLight` `SoftLight` `Difference` `Exclusion` `Multiply` `Hue` `Saturation` `Color` `Luminosity=28`

`[SKBlurStyle]`: `Normal` blurs both sides of the edge, `Solid` holds the source opaque and blurs outward, `Outer` keeps only the blur outside the source — the glow halo — and `Inner` only the blur inside it.

- `SKBlendMode.Plus` sums both sources and `SKBlendMode.Screen` inverse-multiplies them: an emissive glow composites through one of the two, never through an alpha-over stack.

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
|  [13]   | `SaveLayer`                                | `()` / `(SKPaint)` / `(SKRect limit, SKPaint)` -> `int` save depth  |
|  [14]   | `SaveLayer`                                | `(in SKCanvasSaveLayerRec)` -> `int` — the backdrop-filter arm      |
|  [15]   | `Save` / `Restore` / `RestoreToCount`      | matrix+clip stack; `SaveCount` reads depth                          |
|  [16]   | `ClipPath` / `ClipRoundRect` / `ClipRect`  | `(geom, SKClipOperation, antialias)`                                |
|  [17]   | `Concat` / `SetMatrix`                     | `(in SKMatrix)` or `(in SKMatrix44)` perspective                    |
|  [18]   | `Translate`/`Scale`/`RotateDegrees`/`Skew` | matrix mutators                                                     |
|  [19]   | `DrawAnnotation` / `DrawUrlAnnotation`     | PDF link/named-destination annotations                              |
|  [20]   | `DrawLine`                                 | `(SKPoint, SKPoint, SKPaint)` / `(x0, y0, x1, y1, SKPaint)` segment |

- `SKCanvasSaveLayerRec`: `Bounds` hints the layer extent, `Paint` composites the layer back on restore, `Backdrop` filters the destination pixels already on the canvas into the new layer instead of transparent black, and `Flags` carries `SKCanvasSaveLayerRecFlags` — `None=0`, `PreserveLcdText=2`, `InitializeWithPrevious=4` copying those pixels unfiltered, `F16ColorType=0x10` forcing a half-float layer.

[PATH_CONSTRUCTION_ENTRYPOINTS]: `SKPath` contour building, shape adds, and transform
- surface-root: `SKPath` (`new SKPath()` empty; `new SKPath(SKPath)` copy)

| [INDEX] | [SURFACE]                                 | [CALL]                                                                      |
| :-----: | :---------------------------------------- | :-------------------------------------------------------------------------- |
|  [01]   | `MoveTo` / `LineTo`                       | `(SKPoint)` or `(float x, float y)` contour/line                            |
|  [02]   | `QuadTo` / `ConicTo` / `CubicTo`          | quadratic/rational-quadratic/cubic Bézier segments                          |
|  [03]   | `ArcTo`                                   | `(SKRect, startAngle, sweepAngle, forceMoveTo)`                             |
|  [04]   | `ArcTo`                                   | radius and `SKPathArcSize` + `SKPathDirection` overloads                    |
|  [05]   | `Close`                                   | closes the current contour                                                  |
|  [06]   | `AddRect` / `AddOval` / `AddCircle`       | `SKPathDirection direction = Clockwise` shape adds                          |
|  [07]   | `AddRoundRect`                            | `(SKRoundRect, direction)`; construct via `new SKRoundRect(SKRect, rx, ry)` |
|  [08]   | `AddRoundRect`                            | `(SKRect, rx, ry, direction)`                                               |
|  [09]   | `AddPoly`                                 | `(ReadOnlySpan<SKPoint>, bool close)` polyline                              |
|  [10]   | `AddPath`                                 | `(SKPath, SKPathAddMode mode = Append)`                                     |
|  [11]   | `AddPath`                                 | `(SKPath, dx, dy, mode)` / `(SKPath, in SKMatrix, mode)`                    |
|  [12]   | `Transform`                               | `(in SKMatrix[, SKPath dst])` affine transform                              |
|  [13]   | `Offset`                                  | `(SKPoint)` or `(dx, dy)` translation                                       |
|  [14]   | `Reset` / `Rewind`                        | clears contours / clears while retaining allocation                         |
|  [15]   | `Op`                                      | `(SKPath other, SKPathOp)` -> `SKPath` boolean path combine                 |
|  [16]   | `ToSvgPathData`                           | `()` -> `string`; static `ParseSvgPathData(string)` is the inverse          |
|  [17]   | `GetPosition` (`SKPathMeasure`)           | `(float)` -> `SKPoint` / `(float, out SKPoint)` -> `bool`                   |
|  [18]   | `GetPositionAndTangent` (`SKPathMeasure`) | `(float, out SKPoint pos, out SKPoint tan)` -> `bool`                       |

[SURFACE_IMAGE_ENTRYPOINTS]: surface allocation, picture record/replay, snapshot, codec, and pixel transfer

| [INDEX] | [SURFACE]                          | [ROOT]               | [CALL]                                                                 |
| :-----: | :--------------------------------- | :------------------- | :--------------------------------------------------------------------- |
|  [01]   | `Create`                           | `SKSurface`          | `(SKImageInfo)` raster                                                 |
|  [02]   | `Create`                           | `SKSurface`          | `(GRRecordingContext, budgeted, info, samples, origin, props)` GPU     |
|  [03]   | `Snapshot`                         | `SKSurface`          | `()` / `(SKRectI bounds)` -> immutable `SKImage`; zero-copy where able |
|  [04]   | `BeginRecording`                   | `SKPictureRecorder`  | `(SKRect cull)` / `(SKRect cull, bool useRTree)` -> `SKCanvas`         |
|  [05]   | `RecordingCanvas`                  | `SKPictureRecorder`  | the in-flight `SKCanvas` between begin and end                         |
|  [06]   | `EndRecording`                     | `SKPictureRecorder`  | `()` -> `SKPicture`; seals the op list                                 |
|  [07]   | `EndRecordingAsDrawable`           | `SKPictureRecorder`  | `()` -> `SKDrawable`; re-renders lazily per replay                     |
|  [08]   | `Playback`                         | `SKPicture`          | `(SKCanvas)` replays the ops into a canvas                             |
|  [09]   | `CullRect`                         | `SKPicture`          | `SKRect` the record was bounded to                                     |
|  [10]   | `ApproximateBytesUsed`             | `SKPicture`          | `int` retained-op byte cost — the cache-ceiling measure                |
|  [11]   | `ApproximateOperationCount`        | `SKPicture`          | `int`; `GetApproximateOperationCount(bool includeNested)` widens it    |
|  [12]   | `Serialize`                        | `SKPicture`          | `()` -> `SKData` / `(Stream)` / `(SKWStream)` op-list bytes            |
|  [13]   | `Deserialize`                      | `SKPicture`          | `(SKData)` / `(ReadOnlySpan<byte>)` / `(Stream)` / `(SKStream)`        |
|  [14]   | `ToShader`                         | `SKPicture`          | `(SKShaderTileMode tmx, tmy[, SKFilterMode][, SKMatrix][, SKRect])`    |
|  [15]   | `FromPicture`                      | `SKImage`            | `(SKPicture, SKSizeI[, SKMatrix][, SKPaint])` rasterizes a record      |
|  [16]   | `FromEncodedData`                  | `SKImage`            | `(ReadOnlySpan<byte>)` / `(SKData)` / `(Stream)` decode                |
|  [17]   | `FromBitmap`                       | `SKImage`            | `(SKBitmap)` snapshot                                                  |
|  [18]   | `FromPixelCopy`                    | `SKImage`            | `(SKImageInfo, ReadOnlySpan<byte>)` copy                               |
|  [19]   | `FromPixels`                       | `SKImage`            | `(SKPixmap, releaseProc)` adopt                                        |
|  [20]   | `ToTextureImage` / `ToRasterImage` | `SKImage`            | `(GRContext, mipmapped, budgeted)` upload/download                     |
|  [21]   | `ApplyImageFilter`                 | `SKImage`            | `(SKImageFilter, subset, clip, out SKRectI, out SKPoint)`              |
|  [22]   | `ApplyImageFilter`                 | `SKImage`            | twin `out SKPointI`; `GRContext`/`GRRecordingContext` prefixed twins   |
|  [23]   | `Encode`                           | `SKImage`            | `()` PNG or `(SKEncodedImageFormat, quality)` -> `SKData`              |
|  [24]   | `ReadPixels` / `ScalePixels`       | `SKImage`/`SKPixmap` | `(SKPixmap, SKSamplingOptions)` GPU/CPU readback                       |
|  [25]   | `Decode`                           | `SKBitmap`           | `(SKCodec)` / `(SKData)` / `(byte[], SKImageInfo)`                     |
|  [26]   | `Resize`                           | `SKBitmap`           | `(SKImageInfo, SKSamplingOptions)`                                     |
|  [27]   | `InstallPixels` / `PeekPixels`     | `SKBitmap`           | adopts pixels / exposes a view                                         |
|  [28]   | `Create`                           | `SKCodec`            | `(SKStream)` / `(Stream, out SKCodecResult)`                           |
|  [29]   | `FrameCount` / `RepetitionCount`   | `SKCodec`            | animated-image frame metadata                                          |
|  [30]   | `GetFrameInfo`                     | `SKCodec`            | returns `SKCodecFrameInfo`                                             |
|  [31]   | `StartIncrementalDecode`           | `SKCodec`            | starts progressive decode                                              |
|  [32]   | `IncrementalDecode`                | `SKCodec`            | continues decode with `out int rowsDecoded`                            |
|  [33]   | `CreateCopy`                       | `SKVertices`         | `(SKVertexMode, SKPoint[] positions, SKColor[] colors)`                |
|  [34]   | `CreateCopy`                       | `SKVertices`         | `(…, SKPoint[] texs, SKColor[])` / `(…, texs, colors, ushort[] idx)`   |
|  [35]   | `Span` / `Size` / `ToArray`        | `SKData`             | `Span<byte>` zero-copy view / `long` length / `byte[]` copy            |

[TRANSFORM_VALUE_ENTRYPOINTS]: affine and similarity transform construction

| [INDEX] | [SURFACE]                                  | [ROOT]                  | [CALL]                                                     |
| :-----: | :----------------------------------------- | :---------------------- | :--------------------------------------------------------- |
|  [01]   | `CreateIdentity` / `CreateTranslation`     | `SKMatrix`              | `()` / `(float x, float y)`                                |
|  [02]   | `CreateScale`                              | `SKMatrix`              | `(x, y)` / `(x, y, pivotX, pivotY)`                        |
|  [03]   | `CreateRotation` / `CreateRotationDegrees` | `SKMatrix`              | `(angle)` / `(angle, pivotX, pivotY)`                      |
|  [04]   | `CreateSkew` / `CreateScaleTranslation`    | `SKMatrix`              | `(x, y)` / `(sx, sy, tx, ty)`                              |
|  [05]   | `Concat`                                   | `SKMatrix`              | `(SKMatrix first, SKMatrix second)` static compose         |
|  [06]   | `Create` / `CreateDegrees`                 | `SKRotationScaleMatrix` | `(scale, radians\|degrees, tx, ty, anchorX, anchorY)`      |
|  [07]   | `CreateRotation` / `CreateRotationDegrees` | `SKRotationScaleMatrix` | `(angle, anchorX, anchorY)`                                |
|  [08]   | `CreateScale` / `CreateTranslation`        | `SKRotationScaleMatrix` | `(float s)` / `(float x, float y)`                         |
|  [09]   | `ToMatrix`                                 | `SKRotationScaleMatrix` | `()` -> `SKMatrix` widening for a non-`DrawAtlas` consumer |

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
|  [17]   | `ToColorSpaceXyz`                 | `SKColorSpacePrimaries`  | `()` -> D50-adapted matrix; `(out)` refusal overload  |

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

[PAINT_STATE_ENTRYPOINTS]: pigment, stroke, and resolved-outline state on one paint
- root: `SKPaint`
- `SetColor(SKColorF, SKColorSpace)` is the only colour-managed pigment entry: the `ColorF` and `Color` setters carry no space, so the byte path assumes sRGB and quantizes before any conversion and a wide-gamut paint writes float through `SetColor` or its gamut is fiction.

| [INDEX] | [SURFACE]                                | [CALL]                                                                          |
| :-----: | :--------------------------------------- | :------------------------------------------------------------------------------ |
|  [01]   | `Shader` / `ColorFilter` / `ImageFilter` | settable effect slots one paint composes per draw                               |
|  [02]   | `MaskFilter` / `PathEffect`              | settable effect slots one paint composes per draw                               |
|  [03]   | `BlendMode` / `Blender`                  | an `SKBlendMode` ordinal or an `SKBlender` object compositor                    |
|  [04]   | `Style` / `StrokeWidth` / `StrokeCap`    | fill/stroke geometry state                                                      |
|  [05]   | `StrokeJoin` / `StrokeMiter`             | fill/stroke geometry state                                                      |
|  [06]   | `IsAntialias` / `IsDither`               | rasterization state                                                             |
|  [07]   | `SetColor`                               | `(SKColorF, SKColorSpace)` colour-managed pigment write                         |
|  [08]   | `ColorF` / `Color`                       | settable float / 8-bit sRGB pigment slots                                       |
|  [09]   | `GetFillPath`                            | `(SKPath src[, SKRect cull][, float resScale \| SKMatrix])` -> `SKPath` outline |
|  [10]   | `GetFillPath`                            | `(SKPath src, SKPathBuilder dst, …)` -> `bool` builder-sink twin                |
|  [11]   | `GetFastBounds`                          | `(SKRect, out SKRect)` -> `bool` quick-reject cull                              |
|  [12]   | `Clone` / `Reset`                        | copies the paint / clears it to defaults                                        |

[SHADER_ENTRYPOINTS]: gradient, sampled-source, procedural-noise, and shader algebra
- root: `SKShader`
- gradient carry: every gradient factory carries an `SKColorF[] colors, SKColorSpace colorspace` wide-gamut twin of its `SKColor[]` form, an optional `float[] colorPos` stop array, and an optional trailing `SKMatrix localMatrix`.

| [INDEX] | [SURFACE]                                 | [CALL]                                                                               |
| :-----: | :---------------------------------------- | :----------------------------------------------------------------------------------- |
|  [01]   | `CreateLinearGradient`                    | `(SKPoint start, SKPoint end, SKColor[], SKShaderTileMode)`                          |
|  [02]   | `CreateRadialGradient`                    | `(SKPoint center, float radius, SKColor[], SKShaderTileMode)`                        |
|  [03]   | `CreateSweepGradient`                     | `(SKPoint center, SKColor[][, float[] colorPos][, SKMatrix])` full sweep             |
|  [04]   | `CreateSweepGradient`                     | `(center, colors[, colorPos], SKShaderTileMode, startAngle, endAngle)` arc           |
|  [05]   | `CreateTwoPointConicalGradient`           | `(start, startRadius, end, endRadius, SKColor[], SKShaderTileMode)`                  |
|  [06]   | `CreatePerlinNoiseFractalNoise`           | `(baseFreqX, baseFreqY, int numOctaves, float seed[, SKPointI \| SKSizeI tileSize])` |
|  [07]   | `CreatePerlinNoiseTurbulence`             | same arity; turbulence rather than summed fractal octaves                            |
|  [08]   | `CreateImage`                             | `(SKImage src, SKShaderTileMode tmx, tmy[, SKSamplingOptions][, SKMatrix])`          |
|  [09]   | `CreateBitmap`                            | `(SKBitmap src, SKShaderTileMode tmx, tmy[, SKMatrix])`                              |
|  [10]   | `CreatePicture`                           | `(SKPicture, tmx, tmy[, SKFilterMode][, SKMatrix][, SKRect tile])`                   |
|  [11]   | `CreateColor` / `CreateEmpty`             | `(SKColor)` / `(SKColorF, SKColorSpace)` / `()`                                      |
|  [12]   | `CreateBlend`                             | `(SKBlendMode \| SKBlender, SKShader shaderA, SKShader shaderB)`                     |
|  [13]   | `CreateCompose`                           | `(SKShader shaderA, SKShader shaderB[, SKBlendMode])`                                |
|  [14]   | `CreateColorFilter` / `CreateLocalMatrix` | `(SKShader, SKColorFilter)` / `(SKShader, SKMatrix)`                                 |
|  [15]   | `WithColorFilter` / `WithLocalMatrix`     | instance rebind on an existing shader                                                |

[PATH_AND_MASK_EFFECT_ENTRYPOINTS]: stroke-geometry effects and coverage-mask effects

| [INDEX] | [SURFACE]                                       | [ROOT]         | [CALL]                                                          |
| :-----: | :---------------------------------------------- | :------------- | :-------------------------------------------------------------- |
|  [01]   | `CreateDash`                                    | `SKPathEffect` | `(float[] intervals, float phase)` dash pattern                 |
|  [02]   | `CreateTrim`                                    | `SKPathEffect` | `(float start, float stop[, SKTrimPathEffectMode])`             |
|  [03]   | `CreateCorner` / `CreateDiscrete`               | `SKPathEffect` | `(float radius)` / `(segLength, deviation, uint seedAssist)`    |
|  [04]   | `CreateSum` / `CreateCompose`                   | `SKPathEffect` | `(first, second)` / `(outer, inner)`                            |
|  [05]   | `Create1DPath`                                  | `SKPathEffect` | `(SKPath, float advance, float phase, SKPath1DPathEffectStyle)` |
|  [06]   | `Create2DPath` / `Create2DLine`                 | `SKPathEffect` | `(SKMatrix, SKPath)` / `(float width, SKMatrix)`                |
|  [07]   | `CreateBlur`                                    | `SKMaskFilter` | `(SKBlurStyle, float sigma[, bool respectCTM])` coverage blur   |
|  [08]   | `CreateShader` / `CreateTable`                  | `SKMaskFilter` | `(SKShader)` / `(byte[] table)` 256-entry coverage LUT          |
|  [09]   | `CreateGamma` / `CreateClip`                    | `SKMaskFilter` | `(float gamma)` / `(byte min, byte max)`                        |
|  [10]   | `ConvertRadiusToSigma` / `ConvertSigmaToRadius` | `SKMaskFilter` | the blur radius-sigma conversion pair                           |

[COLOR_FILTER_ENTRYPOINTS]: per-pixel colour transforms
- root: `SKColorFilter`
- `CreateOverdraw` admits EXACTLY six colours — one band per overdraw count — and throws `ArgumentException` on any other length, so the arity is structural at the caller and never a runtime-sized array; the `SKColor[]` overload null-checks then delegates to the span form.
- `[SKBlender]`: `CreateBlendMode(SKBlendMode)` `CreateArithmetic(k1, k2, k3, k4, bool enforcePMColor)`

| [INDEX] | [SURFACE]                                             | [CALL]                                                              |
| :-----: | :---------------------------------------------------- | :------------------------------------------------------------------ |
|  [01]   | `CreateColorMatrix` / `CreateHslaColorMatrix`         | `(ReadOnlySpan<float>)` 20-entry row-major matrix                   |
|  [02]   | `CreateBlendMode`                                     | `(SKColor, SKBlendMode)` constant-colour composite                  |
|  [03]   | `CreateLighting` / `CreateLerp`                       | `(SKColor mul, SKColor add)` / `(float weight, filter0, filter1)`   |
|  [04]   | `CreateTable`                                         | `(byte[] table)` / `(byte[] a, r, g, b)` 256-entry per-channel LUTs |
|  [05]   | `CreateLumaColor` / `CreateCompose`                   | `()` luminance-to-alpha / `(outer, inner)`                          |
|  [06]   | `CreateSrgbToLinearGamma` / `CreateLinearToSrgbGamma` | the gamma reprojection pair                                         |
|  [07]   | `CreateHighContrast`                                  | `(SKHighContrastConfig)` grayscale/invert/contrast config           |
|  [08]   | `CreateHighContrast`                                  | `(bool grayscale, SKHighContrastConfigInvertStyle, float contrast)` |
|  [09]   | `CreateOverdraw`                                      | `(ReadOnlySpan<SKColor>)` overdraw-count heatmap                    |

[IMAGE_FILTER_ENTRYPOINTS]: DAG filter nodes a paint or a save-layer backdrop consumes
- root: `SKImageFilter`
- node tail: each factory chains onto a parent through a trailing `SKImageFilter? input` arm and bounds itself through a trailing `SKRect cropRect` arm; `CreateMerge`/`CreatePicture` take the crop alone, `CreateMatrix`/`CreateCrop`/`CreateTile` the input alone, and `CreateCompose`/`CreateEmpty` neither.

| [INDEX] | [SURFACE]                      | [CALL]                                                                            |
| :-----: | :----------------------------- | :-------------------------------------------------------------------------------- |
|  [01]   | `CreateBlur`                   | `(float sigmaX, float sigmaY[, SKShaderTileMode])` gaussian node                  |
|  [02]   | `CreateDropShadow`             | `(dx, dy, sigmaX, sigmaY, SKColor)` shadow behind the source                      |
|  [03]   | `CreateDropShadowOnly`         | same arity; emits the shadow alone and discards the source                        |
|  [04]   | `CreateBlendMode`              | `(SKBlendMode \| SKBlender, SKImageFilter? background[, foreground])`             |
|  [05]   | `CreateArithmetic`             | `(k1, k2, k3, k4, bool enforcePMColor, background[, foreground])`                 |
|  [06]   | `CreateMerge`                  | `(ReadOnlySpan<SKImageFilter>)` / `(SKImageFilter? first, second)`                |
|  [07]   | `CreateDilate` / `CreateErode` | `(float radiusX, float radiusY)` morphological grow / shrink                      |
|  [08]   | `CreateOffset`                 | `(float dx, float dy)` translation node                                           |
|  [09]   | `CreateMatrixConvolution`      | `(SKSizeI kernelSize, ReadOnlySpan<float> kernel, float gain, float bias)`        |
|  [10]   | `CreateMatrixConvolution`      | `(…, SKPointI kernelOffset, SKShaderTileMode, bool convolveAlpha)` tail           |
|  [11]   | `CreateDisplacementMapEffect`  | `(SKColorChannel x, SKColorChannel y, float scale, SKImageFilter displacement)`   |
|  [12]   | `CreateMagnifier`              | `(SKRect lensBounds, float zoomAmount, float inset, SKSamplingOptions)`           |
|  [13]   | `CreateMatrix`                 | `(in SKMatrix[, SKSamplingOptions])` transform node                               |
|  [14]   | `CreateShader`                 | `(SKShader?[, bool dither][, SKRect cropRect])` paint-source node                 |
|  [15]   | `CreateColorFilter`            | `(SKColorFilter)` per-pixel colour node                                           |
|  [16]   | `CreateImage`                  | `(SKImage[, SKRect src, SKRect dst][, SKSamplingOptions])`                        |
|  [17]   | `CreatePicture` / `CreateTile` | `(SKPicture)` / `(SKRect src, SKRect dst)`                                        |
|  [18]   | `CreateCompose` / `CreateCrop` | `(outer, inner)` / `(SKRect[, SKShaderTileMode])`                                 |
|  [19]   | `CreateEmpty`                  | `()` transparent-black source node                                                |
|  [20]   | `CreateDistantLitDiffuse`      | `(SKPoint3 direction, SKColor lightColor, float surfaceScale, float kd)`          |
|  [21]   | `CreateDistantLitSpecular`     | `(SKPoint3 direction, SKColor lightColor, surfaceScale, ks, shininess)`           |
|  [22]   | `CreatePointLitDiffuse`        | `(SKPoint3 location, SKColor lightColor, float surfaceScale, float kd)`           |
|  [23]   | `CreatePointLitSpecular`       | `(SKPoint3 location, SKColor lightColor, surfaceScale, ks, shininess)`            |
|  [24]   | `CreateSpotLitDiffuse`         | `(location, target, specularExponent, cutoffAngle, lightColor, surfaceScale, kd)` |
|  [25]   | `CreateSpotLitSpecular`        | `(…, specularExponent, cutoffAngle, lightColor, surfaceScale, ks, shininess)`     |

[RUNTIME_EFFECT_ENTRYPOINTS]: SkSL compilation, uniform and child binding, and effect projection
- uniform carry: `SKRuntimeEffectUniform` converts implicitly from `float` and `int` with their arrays and spans, `SKPoint`/`SKPointI`, `SKSize`/`SKSizeI`, `SKPoint3`, `SKColor`, `SKColorF`, `SKMatrix`, and `float[][]`; `SKRuntimeEffectChild` converts from `SKShader`, `SKColorFilter`, and `SKBlender`; a projection's `uniforms` argument is an `SKRuntimeEffectUniforms` and its `children` argument an `SKRuntimeEffectChildren`.

| [INDEX] | [SURFACE]                               | [ROOT]                    | [CALL]                                                        |
| :-----: | :-------------------------------------- | :------------------------ | :------------------------------------------------------------ |
|  [01]   | `CreateShader` / `CreateColorFilter`    | `SKRuntimeEffect`         | `(string sksl, out string errors)` compile                    |
|  [02]   | `CreateBlender`                         | `SKRuntimeEffect`         | `(string sksl, out string errors)` compile                    |
|  [03]   | `BuildShader` / `BuildColorFilter`      | `SKRuntimeEffect`         | `(string sksl)` -> a builder over a fresh effect              |
|  [04]   | `BuildBlender`                          | `SKRuntimeEffect`         | `(string sksl)` -> a builder over a fresh effect              |
|  [05]   | `ToShader`                              | `SKRuntimeEffect`         | `([uniforms][, children][, SKMatrix localMatrix])`            |
|  [06]   | `ToColorFilter` / `ToBlender`           | `SKRuntimeEffect`         | `([uniforms][, children])`; no local-matrix arm               |
|  [07]   | `Uniforms` / `Children` / `UniformSize` | `SKRuntimeEffect`         | declared uniform and child names; uniform-block byte size     |
|  [08]   | `Build`                                 | `SKRuntimeShaderBuilder`  | `([SKMatrix localMatrix])` -> `SKShader`                      |
|  [09]   | `Add` / `Contains` / `Reset`            | `SKRuntimeEffectUniforms` | `(string name, SKRuntimeEffectUniform)` named binding         |
|  [10]   | `Add` / `ToArray`                       | `SKRuntimeEffectChildren` | `(string name, SKRuntimeEffectChild?)` / `()` -> `SKObject[]` |

- `SKRuntimeColorFilterBuilder.Build`/`SKRuntimeBlenderBuilder.Build`: `()` -> `SKColorFilter` / `SKBlender`; only the shader builder takes a local matrix.

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
- `SKCanvas.SaveLayer(in SKCanvasSaveLayerRec)` is the in-tree backdrop mechanism: a non-null `Backdrop` samples the destination pixels already on the canvas into the new layer and filters them through that `SKImageFilter`, so a frosted panel sets `Backdrop = SKImageFilter.CreateBlur(sigma, sigma)`, clips to the panel, draws, and restores; a manual read-back through `SKSurface.Snapshot` feeding a second draw is the deleted form.
- `SKImage.ApplyImageFilter` overloads split on receiver context AND on the out-offset type — the context-free form carries an `out SKPoint` twin and an `out SKPointI` twin differing in nothing else, so `out var` is ambiguous and the offset type spells explicitly; the `GRContext` and `GRRecordingContext` forms both fix `out SKPointI`, and every overload returns `out SKRectI outSubset` beside it.

[STACKING]:
- `Avalonia.Skia`(`api-avalonia-skia.md`): `ISkiaSharpApiLeaseFeature.Lease()` yields the live `SKCanvas`/`GRContext`/`SKSurface` a custom control draws through, sharing Avalonia's GPU context and presenting in-airspace; `SkiaSharpExtensions.ToSKRect`/`ToSKMatrix`/`ToSKColor`/`ToSKSamplingOptions` bridge Avalonia value types at the boundary, interior math staying `SKMatrix`/`SKPath`/`SKRect`.
- `SkiaSharp.HarfBuzz`(`api-skia-harfbuzz.md`): custom-visual text shapes through `SKShaper.Shape` into an `SKTextBlob` drawn via `SKCanvas.DrawTextBlob`, with `SKFontManager.MatchCharacter` supplying fallback before shaping; `SKCanvas.DrawText` serves only shaping-free diagnostics.
- `SkiaSharp.NativeAssets.*`(`api-skia-native.md`): `libSkiaSharp` backs every `SKObject` from a per-platform payload, faulting at first draw on a missing or wrong-RID asset rather than at compile.
- `api-drafting-export.md`: `DWG`/`DXF` codecs consume the resolved `SKPath` outline from `SKPath.Op(SKPathOp)` and `ToSvgPathData`/`ParseSvgPathData`, never a private geometry kernel.
- Kernel colour vocabulary: `SKColorSpace.CreateRgb(SKColorSpaceTransferFn, SKColorSpaceXyz)` takes both arguments from the kernel `RgbProfile` row the calling policy names — the transfer from the named `SKColorSpaceTransferFn` curve, the primaries from the named `SKColorSpaceXyz` constant where Skia publishes the gamut and otherwise from `new SKColorSpacePrimaries(rx, ry, gx, gy, bx, by, wx, wy).ToColorSpaceXyz()` fed by that row's published chromaticities, so a hand-typed matrix or transcribed whitepoint literal is the deleted form and `SKColorSpace.Equal` compares two spaces derived from one vocabulary; `ToColorSpaceXyz()` returns the ICC D50-adapted matrix the named constants already carry, and the `out`-parameter overload is the refusal channel for a degenerate chromaticity set.
- Capture rail: `SKSurface.Create(SKImageInfo)` (or a GPU surface from `GRRecordingContext`) draws, `Snapshot()` an `SKImage`, and `Encode(SKEncodedImageFormat.Png, ...)` to an `SKData` byte buffer as the diffable receipt; `SKColorSpace.CreateSrgb`/`CreateIcc` + `SKImageInfo.WithColorSpace` make it color-managed, and animated evidence decodes through `SKCodec.FrameCount`/`GetFrameInfo` per frame.
- Paged export: `SKDocument.CreatePdf(stream, SKDocumentPdfMetadata)` -> per-sheet `BeginPage`/draw/`EndPage` -> `Close`, sharing the live rail's paint/path stack so on-screen and exported geometry are byte-identical.
- Runtime effects: `SKRuntimeEffect.BuildShader(sksl)` compiles once into an `SKRuntimeShaderBuilder`, animation re-binds through `Uniforms.Add(name, value)`/`Children.Add(name, child)` on the implicit-conversion set, and `Build([SKMatrix])` mints the frame's `SKShader`; `CreateShader(sksl, out errors)` + `ToShader(uniforms, children, localMatrix)` is the builder-free arm and `errors` carries the SkSL diagnostic.
- Picture recording: `BeginRecording(cull)` -> draw -> `EndRecording()` seals one device-independent op list that `SKCanvas.DrawPicture`/`SKPicture.Playback` replay N times and `SKImage.FromPicture` rasterizes without a second surface or a second layout run; `ApproximateBytesUsed` is the retained-cost measure a picture cache admits against, and `Serialize()` yields resolution- and device-independent bytes that hash as draw-op evidence beside a pixel hash. `EndRecordingAsDrawable()` swaps the sealed list for a `SKDrawable` that re-renders lazily per replay.

[LOCAL_ADMISSION]:
- Every custom visual draws through the leased `SKCanvas`, composes every effect onto one `SKPaint`, and emits deterministic `SKImage`/`SKData` bytes as its visual evidence; color-managed capture retags through `SKImageInfo.WithColorSpace` so evidence reproduces across host color defaults.

[RAIL_LAW]:
- Package: `SkiaSharp`
- Owns: raster and 2D-vector drawing, offscreen and GPU surfaces, animated codecs, picture recording, paged-document export, color-managed spaces, the shader/filter/effect/runtime-SkSL pipeline, and the GPU backend contexts
- Accept: custom visuals draw through a leased `SKCanvas`; effects compose onto one `SKPaint`; text shapes through HarfBuzz; capture emits deterministic `SKImage`/`SKData` evidence
- Reject: GDI public vocabulary; a parallel render backend bypassing the Avalonia lease; per-effect draw fan-out where one paint composes the pipeline; a per-draw resample knob where `SKSamplingOptions` owns filter/mipmap/cubic selection

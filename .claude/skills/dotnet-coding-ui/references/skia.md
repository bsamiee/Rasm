# [SKIA]

Deep use of SkiaSharp beneath a drawn scene: how color flows from a tagged source to the display, what a layer, clip, or filter costs, how pictures and images cache, how one scene paints raster and GPU targets, and the members each decision reaches for.

## [01]-[OWNERSHIP]

Every `SK*` object wraps a native handle, and its ownership family decides who disposes:

| [INDEX] | [FAMILY]                                                                      | [OWNERSHIP]          | [REUSE]                          |
| :-----: | :---------------------------------------------------------------------------- | :------------------- | :------------------------------- |
|  [01]   | `SKImage`, `SKPicture`, `SKShader`, `SKColorFilter`, `SKImageFilter`          | Reference counted    | Shared across paints and frames  |
|  [02]   | `SKMaskFilter`, `SKPathEffect`, `SKTypeface`, `SKTextBlob`, `SKDrawable`      | Reference counted    | Shared across paints and frames  |
|  [03]   | `SKColorSpace`, `SKData`, `SKSurface`, `SKDocument`, `SKRuntimeEffect`        | Reference counted    | One static per fixed value       |
|  [04]   | `SKPaint`, `SKPath`, `SKPathBuilder`, `SKRoundRect`, `SKRegion`, `SKFont`     | Single owner         | Built per style, reused per draw |
|  [05]   | `SKPixmap`, `SKBitmap`, `SKCodec`, `SKPictureRecorder`, `SKTextBlobBuilder`   | Single owner         | One per buffer or recording      |
|  [06]   | `SKAutoCanvasRestore`, `SKRuntimeShaderBuilder`, `SKRuntimeEffectUniforms`    | Single owner, scoped | One per scope or build           |
|  [07]   | `SKImageInfo`, `SKMatrix`, `SKColorSpaceXyz`, `SKColorF`, `SKSamplingOptions` | Struct               | Copied by value, no disposal     |

- Storing a reference-counted handle in a paint, an `SKImageInfo`, or a recording adds Skia's own reference, the creating scope disposes its handle
- `SKColorSpace.CreateSrgb()`, `CreateSrgbLinear()`, `SKFontManager.Default`, `SKTypeface.Default`, and `SKData.Empty` return dispose-protected process instances
- `SKSurface.Canvas` belongs to the surface and dies with it, `new SKCanvas(bitmap)` is the one canvas a scope owns
- Paints hold no state stack, `Save` and `Restore` track matrix and clip alone, so a scene keeps prebuilt paints in place of mutating one between draws
- `SKPaint.TextSize`, `Typeface`, and `MeasureText` and every `SKFilterQuality` overload are error-level obsolete, `SKFont` and `SKSamplingOptions` replace them
- `SKGraphics.SetResourceCacheTotalByteLimit`, `SetFontCacheLimit`, `PurgeFontCache`, and `PurgeAllCaches` govern the CPU caches, `GRContext` governs the GPU cache

## [02]-[COLOR_MANAGEMENT]

Skia converts a source to its destination on draw in six steps (unpremultiply, linearize, to XYZ D50, to destination gamut, encode, premultiply), the transfer steps carry the cost, and tags decide whether any step runs:
- Untagged sources read as sRGB, an untagged destination takes its source's space
- Untagged surfaces draw unmanaged, every tagged source lands on them unconverted
- `new SKImageInfo(width, height, colorType, alphaType, colorspace)` tags a surface, image, or pixmap, `WithColorSpace` derives a sibling contract
- `SKColorSpace.CreateRgb(SKColorSpaceTransferFn, SKColorSpaceXyz)` builds any matrix-TRC space from a transfer and a gamut matrix to XYZ D50
- `SKColorSpaceTransferFn.Srgb`, `TwoDotTwo`, `Linear`, `Rec2020`, `Pq`, and `Hlg` are the parametric transfers, `SKColorSpaceXyz.Srgb`, `AdobeRgb`, `DisplayP3`, `Rec2020`, and `Xyz` the D50-adapted gamuts
- `SKColorSpaceXyz.Concat(a, b)` and `Invert()` compose gamut to gamut, `SKColorSpacePrimaries.ToColorSpaceXyz(out toXyzD50)` adapts measured primaries and white to D50
- `CreateIcc(bytes)` reads a matrix-TRC profile, `ToProfile()` writes one, `CreateCicp(primaries, transfer)` reads H.273 codes for video
- `GetNumericalTransferFunction(out fn)` and `ToColorSpaceXyz(out matrix)` read a space back, PQ and HLG answer no numerical transfer
- `ToLinearGamma()` and `ToSrgbGamma()` keep the gamut and swap the transfer, `IsSrgb`, `GammaIsLinear`, and `GammaIsCloseToSrgb` classify without parameters
- `SKColorSpace.Equal(left, right)` compares content, handles of one space from separate factories are equal
- `SKColorType.RgbaF16` is extended range, `RgbaF16Clamped` assumes 0..1, `RgbaF32` doubles the bytes, a linear F16 premultiplied surface is the scene-referred working target
- `SKAlphaType.Premul` is what Skia draws into, `Unpremul` is interchange, an 8-bit round trip loses low-alpha precision and F16 keeps it
- `SKPaint.SetColor(SKColorF, SKColorSpace)` and `SKShader.CreateColor(SKColorF, SKColorSpace)` state a solid color in a named space, `Color` and `ColorF` without a space are sRGB
- Gradient overloads taking `SKColorF[]` with an `SKColorSpace` tag the stops, a `null` space is sRGB
- Gradient interpolation runs in the destination space, a linear blend needs a linear-tagged target
- `SKColorFilter.CreateSrgbToLinearGamma()` and `CreateLinearToSrgbGamma()` bracket a color matrix that must run in linear light on an sRGB target
- Color matrices, filter constants, and plain SkSL uniforms apply in the destination's working space
- Constants encoded in sRGB misfire on a linear target, a bake converts them once per target space
- `SKPixmap.GetPixelColorF(x, y)` unpremultiplies `Premul` content, a zero-alpha pixel loses its color
- `SKImage.ReadPixels(dstInfo, …)` converts across color and alpha types on the copy out, `SKPixmap.ReadPixels` does the same between buffers

## [03]-[LAYERS_CLIPS_AND_FILTERS]

One draw assembles one program (geometry coverage, clip coverage, shader tree, color filter tree, blend, color conversion), so nesting shaders, color filters, and blenders adds no pass, while an image filter or a layer adds an offscreen:
- `SKPaint` stages run in order: `PathEffect` on geometry, `MaskFilter` on the coverage mask, `Shader` for color, `ColorFilter` on source color, `BlendMode` or `Blender` last, `ImageFilter` on a layer
- `SaveLayer(limit, paint)` allocates an offscreen, `limit` hints the size and a `ClipRect` bounds it, the paint applies alpha, color filter, image filter, and blend on `Restore`
- Path effects and mask filters on a layer paint are ignored at restore, they belong on the draws inside
- `SKCanvasSaveLayerRec` adds `Backdrop` (a filter over what the canvas already holds) and `Flags`: `InitializeWithPrevious` copies prior pixels, `PreserveLcdText` keeps LCD glyphs, `F16ColorType` makes the layer F16
- Layers drop LCD text unless `PreserveLcdText` is set, a filtered layer is sized by the filter's reach and a user bound clips it
- `ClipRoundRect` and `ClipPath` take `antialias`, `ClipRegion` is device-space, aliased, and ignores the matrix, so damage clips apply before `Scale`
- `LocalClipBounds` is outset by one pixel under an antialiased clip, `DeviceClipBounds` is exact, `QuickReject(rect)` tests transformed geometry against the clip
- Image filters take `input` (`null` reads the layer, or the image under `ApplyImageFilter`) and `cropRect` (equal to a `CreateCrop(rect, SKShaderTileMode.Decal, input)` node)
- Chains without crop rects read the whole layer per node, `CreateCrop` with another tile mode extends edges by repeat or clamp
- `CreateDropShadow` keeps the source, `CreateDropShadowOnly` omits it for composition under content, `sigmaX` and `sigmaY` are sigmas
- `CreateBlur(sigmaX, sigmaY, tileMode, input, cropRect)` defaults to decal edges, `SKMaskFilter.ConvertRadiusToSigma` maps a design radius, a radius under one pixel maps to sigma 0
- `CreateMerge(filters, cropRect)` composites results source-over in order, `CreateCompose(outer, inner)` is outer over inner's output
- `CreateDilate` and `CreateErode` are rectangular morphology, `CreateOffset` shifts, `CreateMatrixConvolution(kernelSize, kernel, gain, bias, kernelOffset, tileMode, convolveAlpha)` runs a kernel
- `CreateDisplacementMapEffect`, `CreateDistantLitDiffuse`, `CreatePointLitSpecular`, and `CreateMagnifier` are the remaining nodes, `CreateShader`, `CreateImage`, and `CreatePicture` inject sources
- `SKImage.ApplyImageFilter(filter, subset, clipBounds, out outSubset, out outOffset)` runs a chain offline on the CPU, `outOffset` places the grown result and keeps frames aligned
- `SKMaskFilter.CreateBlur(style, sigma, respectCTM)` blurs the A8 coverage of one draw with no layer, `Normal`, `Solid`, `Outer`, and `Inner` select which side keeps ink
- `SKMaskFilter.CreateShader(shader)`, `CreateTable(table)`, `CreateGamma(gamma)`, and `CreateClip(min, max)` reshape coverage before color
- `SKPathEffect.CreateDash(intervals, phase)`, `CreateTrim(start, stop, mode)`, `CreateCorner(radius)`, `CreateDiscrete`, `Create1DPath`, `Create2DPath`, and `Create2DLine` change stroke geometry
- `SKColorFilter.CreateColorMatrix(ReadOnlySpan<float>)` is a 4×5 row-major matrix over unpremultiplied RGBA, `CreateHslaColorMatrix` runs over HSLA
- `CreateTable(byte[256])` is a per-channel lookup, `CreateLumaColor` moves luminance into alpha, `CreateBlendMode(color, mode)` tints, `CreateLighting(mul, add)` multiplies and adds
- `CreateHighContrast(grayscale, invertStyle, contrast)` uses its own weights, CSS `grayscale()`, `sepia()`, `invert()`, and `contrast()` stay hand-built matrices
- `CreateCompose(outer, inner)` chains color filters, `CreateLerp(weight, a, b)` fades between them

## [04]-[RUNTIME_EFFECTS]

SkSL compiles once per source into an `SKRuntimeEffect`, and a builder binds uniforms and children per use:
- `SKRuntimeEffect.CreateShader(sksl, out errors)` expects `half4 main(float2 p)` returning premultiplied color, `CreateColorFilter` expects `half4 main(half4 c)`, `CreateBlender` expects `half4 main(half4 src, half4 dst)`
- `errors` holds the compiler message on a `null` result, a failed compile at a static initializer fails the process at launch
- `SKRuntimeShaderBuilder(effect)`, `SKRuntimeColorFilterBuilder`, and `SKRuntimeBlenderBuilder` hold zero-initialized uniform storage, `Uniforms[name]` copies bytes with no type conversion
- `SKRuntimeEffectUniform` converts from `float`, `float[]`, `int`, `SKPoint`, `SKPoint3`, `SKSize`, `SKColor`, `SKColorF`, and `SKMatrix`, `Children[name]` takes `SKShader`, `SKColorFilter`, or `SKBlender`
- `Uniforms`, `Children`, and `UniformSize` on the effect reflect the declared names and layout, a mismatched size skips the write
- `Build()` and `Build(localMatrix)` return the shader, `SKRuntimeEffect.ToShader(uniforms, children, localMatrix)` builds without a builder
- Coordinates passed to `main` are shader-local after the local matrix, image children sample in pixel coordinates, `child.eval(coord)` samples a child
- Destination space is the working space, child inputs convert into it and plain uniforms stay as written
- `layout(color)` uniforms convert from unpremultiplied extended sRGB into the working space
- `toLinearSrgb` and `fromLinearSrgb` give a known space for lighting math on any target
- `float` and `int` are high precision, `half` and `short` medium, loops need a constant bound the compiler can unroll
- Image shaders return premultiplied color, `SKImage.ToRawShader` skips conversion and premultiplication and refuses cubic sampling

## [05]-[PICTURES_AND_RETAINED_SCENES]

Pictures record draws once and replay them per frame, and the scene decides per subtree between replay, rasterization, and immediate drawing:
- `SKPictureRecorder.BeginRecording(cullRect, useRTree: true)` returns the recording canvas, drawing outside `cullRect` is undefined, `EndRecording()` returns the immutable picture
- R-tree replay under a clip visits the ops intersecting it alone, a damage-clipped replay of a large scene pays for visible ops
- `EndRecordingAsDrawable()` keeps live `SKDrawable` references, `SKDrawable.NotifyDrawingChanged()` bumps `GenerationId` when it would draw differently and `Bounds` covers every state
- `SKCanvas.DrawPicture(picture, in matrix, paint)` with a paint draws through a layer and changes non-associative blends inside, replay with `null` paint when no group effect applies
- `SKPicture.Playback(canvas)` re-executes every op, `ApproximateOperationCount` and `ApproximateBytesUsed` probe cost, images inside are held by reference and excluded from the bytes
- Pictures hold their own references to shaders, images, and typefaces, the recording scope disposes its handles after `EndRecording`
- `SKImage.FromPicture(picture, dimensions, matrix, paint)` is a lazy image rasterized on first use and cached, the form for static content at one scale, replay serves changing or rescaled content
- `SKPicture.ToShader(tmx, tmy, filterMode, localMatrix, tile)` tiles a picture as a pattern, `SKShader.CreatePicture` is the same shader from the shader side
- `Serialize()` and `Deserialize(data)` round-trip within one Skia build, embedded images and typefaces need the serialization procs the binding does not expose
- Retained scenes record each stable subtree once per content change and replay it under the damage clip
- Live subtrees (animating, edited, timed) draw immediately
- Ancestors of a live node replay their pictures around it, an ancestor that draws immediately redraws the whole frame per tick
- `SKNoDrawCanvas` and `SKSurface.CreateNull(width, height)` measure and warm up without pixels
- `SKRegion` is a run-length rectangle set: `Op(rect, SKRegionOperation.Union)` accumulates damage, `IsComplex` and `Bounds` tell when to collapse, `CreateRectIterator()` walks rects, `QuickReject(rect)` culls
- Damage rects round outward to device pixels (`SKRectI.Ceiling(rect, outwards: true)`) before `Op`, a fractional rect leaves a seam

## [06]-[SURFACES_IMAGES_AND_GPU]

One scene paints raster and GPU targets through `SKCanvas`, the backends differ in snapshot cost, thread affinity, and read-back:
- `SKSurface.Create(info)` owns raster pixels, `Create(info, pixels, rowBytes, releaseProc, context)` wraps caller memory, `Create(pixmap)` wraps a view, each with an `SKSurfaceProperties` overload
- `SKSurface.Create(GRContext, GRBackendRenderTarget, origin, colorType, colorspace, props)` wraps a host framebuffer, `Create(context, budgeted, info, sampleCount, origin, props)` allocates an offscreen
- `Snapshot()` shares pixels copy-on-write, the next draw after a live snapshot copies the surface, so a frame snapshots once after its last draw
- `SKSurface.Draw(canvas, x, y, sampling, paint)` blits a retained panel surface onto another canvas without a snapshot
- `PeekPixels()` exposes raster memory until the next canvas call, `ReadPixels(dstInfo, dstPixels, rowBytes, srcX, srcY)` copies and converts, on GPU it blocks, `RequestReadPixels(info, srcRect, callback)` reads back asynchronously
- `Flush(submit: true)` ends GPU work for presentation, `GRContext.Flush(submit, synchronous)` and `Submit(synchronous)` are the context forms
- `SKImage.FromPixels(pixmap, releaseProc)` wraps memory the delegate keeps alive, `FromPixelCopy` copies, a pinned array freed before the image dies corrupts later draws
- `SKImage.FromEncodedData` defers decoding until the first draw on the drawing thread, `IsLazyGenerated` reports it, `ToRasterImage(ensurePixelData: true)` decodes ahead
- `SKImage.Subset(rect)` shares pixels of a window, `Encode(format, quality)` returns `SKData`, `SKPixmap.Encode(stream, SKPngEncoderOptions)` and the JPEG and WebP options encode with codec settings
- `SKCodec.Create(stream)` and `GetPixels(info, pixels, rowBytes, options)` decode with scanline and incremental modes and animation frames
- `SKBitmap` is the mutable pixel owner, `InstallPixels(info, pixels, rowBytes, releaseProc)` wraps memory, `SetImmutable()` promises no writes, `NotifyPixelsChanged()` resets caches after a write
- `SKImage.ToTextureImage(context, mipmapped, budgeted)` uploads with mipmaps for downscaling, `SKSamplingOptions(SKFilterMode.Linear, SKMipmapMode.Linear)` samples them
- `SKCubicResampler.Mitchell` is the historic high-quality upscale, `CatmullRom` is sharper, `SKFilterMode.Nearest` keeps pixel art
- `GRContext` instances pair one-to-one with a GL context or device, calls assume it current on the calling thread, texture images die with it
- `GRContext.SetResourceCacheLimit(bytes)` purges least-recently-used resources to fit, `PurgeUnusedResources(milliseconds)` evicts after idle, `PurgeResources()` frees everything
- `ResetContext(state)` runs after the host changed GL state behind Skia, `AbandonContext(releaseResources)` handles device loss and `IsAbandoned` reports it
- `SKImage.IsValid(context)` goes false after abandonment, `IsTextureBacked` tells a texture image from a raster one

## [07]-[TEXT_PRIMITIVES]

Skia rasterizes glyphs and draws blobs, a shaping engine supplies glyph ids and positions:
- `SKTextBlob` is the immutable run container and the GPU glyph-atlas cache key by `UniqueId`
- Blobs kept across frames reuse atlas data, a blob rebuilt per frame defeats the atlas
- `SKTextBlobBuilder.AllocatePositionedRun(font, count, bounds)` takes one `SKFont` per run, `AllocateHorizontalRun(font, count, y)` takes x alone on one baseline, `AddPositionedRun(glyphs, font, positions)` copies spans
- Optional `bounds` on a run lets a draw skip when the blob misses the surface, `SKTextBlob.Bounds` gives conservative ink bounds for invalidation
- `SKFont(typeface, size, scaleX, skewX)` is the glyph source, `Edging` (`Alias`, `Antialias`, `SubpixelAntialias`), `Hinting` (`None`, `Slight`, `Normal`, `Full`), `Subpixel`, `LinearMetrics`, `BaselineSnap`, `Embolden`, `ForceAutoHinting`, and `EmbeddedBitmaps` control rasterization
- `SubpixelAntialias` needs a surface with a known `SKPixelGeometry`, under `Unknown` the scaler renders grayscale, and `BaselineSnap` applies under an axis-aligned matrix alone
- `LinearMetrics = true` ignores hinting and rounding so layout at scale 1 and 2 agree, `Subpixel` positions origins at fractions
- `GetFontMetrics(out metrics)` gives `Ascent` (negative), `Descent`, `Leading`, `CapHeight`, `XHeight`, and nullable `UnderlinePosition` and `UnderlineThickness`
- `GetGlyphs(text, glyphs)` maps code points, `ContainsGlyphs(text)` tests coverage, `GetGlyphWidths(glyphs, out bounds)` and `GetGlyphPositions` measure without shaping
- `GetGlyphPath(glyph)` and `GetTextPath(text, positions)` return outlines for clipping, hit testing, and effects
- `SKCanvas.DrawText(blob, x, y, paint)` draws a blob with the paint's color, shader, stroke, and blend, `SKTextBlob.GetIntercepts(upper, lower, intervals, paint)` gives the gaps an underline skips
- `SKFontManager.Default.MatchCharacter(familyName, weight, width, slant, bcp47, character)` finds a fallback face holding one code point, `MatchFamily(name, style)` resolves a family
- `SKTypeface.FromFamilyName`, `FromFile`, `FromStream`, and `FromData` construct typefaces, `OpenStream(out ttcIndex)` hands the bytes to a shaper

## [08]-[DOCUMENTS]

`SKDocument` writes vector output through the same canvas API:
- `SKDocument.CreatePdf(stream, metadata)` opens a document, `CreatePdf(stream, dpi)` sets the raster resolution for filters and shaders the format lacks
- `BeginPage(width, height)` returns a canvas in points (`DefaultRasterDpi` is 72), `EndPage()` closes it, `Close()` writes the file, `Abort()` discards
- Gradients, paths, text, and images stay vector, image filters and runtime shaders rasterize at the document dpi
- `SKCanvas.DrawUrlAnnotation(rect, url)` adds a link, `CreateXps(stream, dpi)` writes XPS on Windows

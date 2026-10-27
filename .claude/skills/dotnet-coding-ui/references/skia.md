# [SKIA]

SkiaSharp beneath a drawn scene: handle ownership, color management, layer and filter cost, runtime effects, pictures, surfaces and GPU contexts, text primitives, and documents.

## [01]-[OWNERSHIP]

Every `SK*` object wraps a native handle, and its ownership family decides who disposes:

| [INDEX] | [FAMILY]                                                                      | [OWNERSHIP]          | [REUSE]                          |
| :-----: | :---------------------------------------------------------------------------- | :------------------- | :------------------------------- |
|  [01]   | `SKImage`, `SKPicture`, `SKShader`, `SKColorFilter`, `SKImageFilter`          | Reference counted    | Shared across paints and frames  |
|  [02]   | `SKMaskFilter`, `SKPathEffect`, `SKTypeface`, `SKTextBlob`, `SKDrawable`      | Reference counted    | Shared across paints and frames  |
|  [03]   | `SKColorSpace`, `SKData`, `SKSurface`, `SKDocument`, `SKRuntimeEffect`        | Reference counted    | One static per fixed value       |
|  [04]   | `SKPaint`, `SKPath`, `SKPathBuilder`, `SKRoundRect`, `SKRegion`, `SKFont`     | Single owner         | Built per style, reused per draw |
|  [05]   | `SKPixmap`, `SKBitmap`, `SKCodec`, `SKPictureRecorder`, `SKTextBlobBuilder`   | Single owner         | One per buffer or recording      |
|  [06]   | `SKAutoCanvasRestore`, `SKRuntimeEffectUniforms`, `SKRuntimeEffectChildren`   | Single owner, scoped | One per scope or build           |
|  [07]   | `SKImageInfo`, `SKMatrix`, `SKColorSpaceXyz`, `SKColorF`, `SKSamplingOptions` | Struct               | Copied by value, no disposal     |

- Storing a reference-counted handle in a paint, an `SKImageInfo`, or a recording adds Skia's own reference, the creating scope disposes its handle
- `SKColorSpace.CreateSrgb()`, `CreateSrgbLinear()`, `SKFontManager.Default`, `SKTypeface.Default`, `SKData.Empty` are dispose-protected singletons
- `SKColorSpace.CreateRgb`, `CreateIcc`, and `CreateCicp` return owned handles, one static per fixed space holds them for the process
- `SKSurface.Canvas` belongs to the surface and dies with it, `new SKCanvas(bitmap)` is the one canvas a scope owns
- Paints hold no state stack, `Save` and `Restore` track matrix and clip alone, a scene keeps prebuilt paints in place of mutating one between draws
- `SKPaint.TextSize`, `Typeface`, `MeasureText`, and `SKFilterQuality` overloads are obsolete errors, `SKFont` and `SKSamplingOptions` replace them
- `SKRunBuffer.GetGlyphSpan()` and `GetPositionSpan()` are error-level obsolete, `Glyphs` and `Positions` are the spans
- `SKGraphics.SetResourceCacheTotalByteLimit`, `SetFontCacheLimit`, `PurgeFontCache`, and `PurgeAllCaches` govern CPU caches, `GRContext` the GPU

```csharp
internal static class Spaces {
    public static SKColorSpace LinearP3 { get; } = SKColorSpace.CreateRgb(SKColorSpaceTransferFn.Linear, SKColorSpaceXyz.DisplayP3);
}

internal sealed class Fill : IDisposable {
    private readonly SKPaint paint = new() { IsAntialias = true };

    public Fill(SKColorF color, SKColorSpace space) {
        using SKShader shader = SKShader.CreateColor(color, space);
        paint.Shader = shader;
    }

    public void Draw(SKCanvas canvas, SKPath outline) => canvas.DrawPath(outline, paint);

    public void Dispose() => paint.Dispose();
}
```

## [02]-[COLOR_MANAGEMENT]

Skia converts a source to its destination on draw (unpremultiply, linearize, one gamut matrix, encode, premultiply). Transfer steps hold the cost, tags decide whether any step runs:
- `new SKImageInfo(width, height, colorType, alphaType, colorspace)` tags a surface, image, or pixmap
- `WithColorSpace`, `WithColorType`, and `WithAlphaType` derive a sibling contract
- `SKColorSpace.CreateRgb(SKColorSpaceTransferFn, SKColorSpaceXyz)` builds any matrix-TRC space from a transfer and a gamut matrix to XYZ D50
- `SKColorSpaceTransferFn.Srgb`, `TwoDotTwo`, `Linear`, `Rec2020`, `Pq`, and `Hlg` are parametric, `Invert()` and `Transform(x)` evaluate one
- `SKColorSpaceXyz.Srgb`, `AdobeRgb`, `DisplayP3`, `Rec2020`, and `Xyz` are the D50-adapted gamuts
- `SKColorSpaceXyz.Concat(a, b)` and `Invert()` compose gamut to gamut, the gamut step of a conversion is one such product
- `CreateIcc(bytes)` reads a matrix-TRC profile, `ToProfile()` writes one, `CreateCicp(primaries, transfer)` reads H.273 codes for video
- `GetNumericalTransferFunction(out fn)` and `ToColorSpaceXyz(out matrix)` read a space back, `IsNumericalTransferFunction` is false for PQ and HLG
- `ToLinearGamma()` and `ToSrgbGamma()` keep the gamut and swap the transfer
- `IsSrgb`, `GammaIsLinear`, and `GammaIsCloseToSrgb` classify without parameters
- `SKColorSpace.Equal(left, right)` compares content, handles of one space from separate factories are equal
- `SKColorType.RgbaF16` is extended range, `RgbaF16Clamped` assumes 0..1, `RgbaF32` doubles the bytes, `AlphaF16` holds one float channel
- `SKAlphaType.Premul` is what Skia draws into, `Unpremul` is interchange, an 8-bit round trip loses low-alpha precision and F16 keeps it
- `SKPaint.SetColor(SKColorF, SKColorSpace)` and `SKShader.CreateColor(SKColorF, SKColorSpace)` state a solid color in a named space
- `Color` and `ColorF` without a space are sRGB
- Gradient overloads taking `SKColorF[]` with an `SKColorSpace` tag the stops, a `null` space is sRGB
- Gradient interpolation runs in the destination space, a linear blend needs a linear-tagged target
- `SKColorFilter.CreateSrgbToLinearGamma()` and `CreateLinearToSrgbGamma()` bracket a color matrix that must run in linear light on an sRGB target
- Color matrices and filter constants apply in the destination's working space
- Constants encoded in sRGB misfire on a linear target, a bake converts them once per target space
- `SKPixmap.GetPixelColorF(x, y)` unpremultiplies `Premul` content, a zero-alpha pixel loses its color
- `SKImage.ReadPixels(pixmap)` converts across color type, alpha type, and color space on the copy out

```csharp
internal sealed class SceneSpace(SKColorSpaceXyz gamut) : IDisposable {
    private readonly SKColorSpace space = SKColorSpace.CreateRgb(SKColorSpaceTransferFn.Linear, gamut);

    public SKImageInfo Frame(SKSizeI pixels) => new(pixels.Width, pixels.Height, SKColorType.RgbaF16, SKAlphaType.Premul, space);

    public Fin<float[]> Samples(SKImage frame) {
        SKImageInfo linear = frame.Info.WithColorType(SKColorType.RgbaF32).WithAlphaType(SKAlphaType.Unpremul).WithColorSpace(space);
        using SKBitmap bitmap = new(linear);
        using SKPixmap pixels = bitmap.PeekPixels();
        return Fault.Unless(frame.ReadPixels(pixels), Reason.Refused, (frame.Width, frame.Height)).Map(_ => MemoryMarshal.Cast<byte, float>(bitmap.GetPixelSpan()).ToArray());
    }

    public void Dispose() => space.Dispose();
}
```

## [03]-[LAYERS_CLIPS_AND_FILTERS]

One draw assembles one program (geometry coverage, clip coverage, shader tree, color filter tree, blend, color conversion). Nested shaders, color filters, and blenders add no pass, an image filter or a layer adds an offscreen:
- `SKPaint` stages run in order: `PathEffect`, `MaskFilter`, `Shader`, `ColorFilter`, `BlendMode` or `Blender`, `ImageFilter`
- `PathEffect` acts on geometry, `MaskFilter` on the coverage mask, `Shader` and `ColorFilter` on color, `ImageFilter` on a layer
- `SaveLayer(limit, paint)` allocates an offscreen, `limit` hints the size and a `ClipRect` bounds it
- Layer paints apply alpha, color filter, image filter, and blend on `Restore`
- Path effects and mask filters on a layer paint are ignored at restore, they belong on the draws inside
- `SKCanvasSaveLayerRec.Flags`: `InitializeWithPrevious` copies prior pixels, `PreserveLcdText` keeps LCD glyphs, `F16ColorType` makes the layer F16
- Filtered layers are sized by the filter's reach, a user bound clips them
- `ClipRoundRect` and `ClipPath` take `antialias`, `ClipRegion` is aliased
- `LocalClipBounds` is outset by one pixel under an antialiased clip, `DeviceClipBounds` is exact
- Image filters take `input`, `null` reads the layer or the image under `ApplyImageFilter`
- `cropRect` on a filter equals a `CreateCrop(rect, SKShaderTileMode.Decal, input)` node
- Chains without crop rects read the whole layer per node, `CreateCrop` with another tile mode extends edges by repeat or clamp
- `CreateDropShadow` keeps the source, `CreateDropShadowOnly` omits it for composition under content, `sigmaX` and `sigmaY` are sigmas
- `CreateBlur(sigmaX, sigmaY, tileMode, input, cropRect)` defaults to decal edges
- `SKMaskFilter.ConvertRadiusToSigma` maps a radius under one pixel to sigma 0
- `CreateMerge(filters, cropRect)` composites results source-over in order, `CreateCompose(outer, inner)` is outer over inner's output
- `CreateBlendMode(mode, background, foreground)` composites both filter outputs, `SrcIn` keeps the foreground where the background has alpha
- `CreateDilate` and `CreateErode` are rectangular morphology, `CreateOffset` shifts
- `CreateMatrixConvolution(kernelSize, kernel, gain, bias, kernelOffset, tileMode, convolveAlpha)` runs a kernel
- `CreateDisplacementMapEffect`, `CreateDistantLitDiffuse`, `CreatePointLitSpecular`, `CreateMagnifier`, and `CreateArithmetic` complete the nodes
- `CreateShader`, `CreateImage`, `CreatePicture`, and `CreatePaint` inject sources
- `SKImage.ApplyImageFilter(filter, subset, clipBounds, out outSubset, out outOffset)` runs a chain offline on the CPU
- `outOffset` places the grown result and keeps frames aligned
- `SKMaskFilter.CreateBlur(style, sigma, respectCTM)` blurs the A8 coverage of one draw with no layer
- `SKBlurStyle.Normal`, `Solid`, `Outer`, and `Inner` select which side keeps coverage
- `SKMaskFilter.CreateShader(shader)`, `CreateTable(table)`, `CreateGamma(gamma)`, and `CreateClip(min, max)` reshape coverage before color
- `SKPathEffect.CreateDash(intervals, phase)`, `CreateTrim(start, stop, mode)`, and `CreateCorner(radius)` change stroke geometry
- `CreateDiscrete`, `Create1DPath`, `Create2DPath`, and `Create2DLine` complete the path effects
- `SKColorFilter.CreateColorMatrix(ReadOnlySpan<float>)` is a 4×5 row-major matrix over unpremultiplied RGBA, `CreateHslaColorMatrix` runs over HSLA
- `CreateTable(byte[256])` is a per-channel lookup, `CreateLumaColor` moves luminance into alpha
- `CreateBlendMode(color, mode)` tints, `CreateLighting(mul, add)` multiplies and adds
- `CreateHighContrast(grayscale, invertStyle, contrast)` has its own weights, CSS `grayscale()`, `sepia()`, `invert()`, `contrast()` are hand matrices
- `CreateCompose(outer, inner)` chains color filters, `CreateLerp(weight, a, b)` fades between them

```csharp
internal sealed record Shadow(float X, float Y, float Sigma, float Spread, SKColorF Color, bool Inset) {
    public SKImageFilter Filter() {
        using SKImageFilter? grown = Spread switch {
            > 0f => SKImageFilter.CreateDilate(Spread, Spread),
            < 0f => SKImageFilter.CreateErode(-Spread, -Spread),
            _ => null,
        };
        using SKImageFilter moved = SKImageFilter.CreateOffset(X, Y, grown);
        using SKImageFilter blurred = SKImageFilter.CreateBlur(Sigma, Sigma, moved);
        using SKColorFilter inked = SKColorFilter.CreateColorMatrix([0f, 0f, 0f, 0f, Color.Red, 0f, 0f, 0f, 0f, Color.Green, 0f, 0f, 0f, 0f, Color.Blue, 0f, 0f, 0f, Inset ? -Color.Alpha : Color.Alpha, Inset ? Color.Alpha : 0f]);
        SKImageFilter colored = SKImageFilter.CreateColorFilter(inked, blurred);
        if (!Inset) return colored;
        using (colored) return SKImageFilter.CreateBlendMode(SKBlendMode.SrcIn, background: null, foreground: colored);
    }
}

internal static class Layers {
    public static int Backdrop(SKCanvas canvas, SKRect bounds, float sigma) {
        using SKImageFilter blur = SKImageFilter.CreateBlur(sigma, sigma, SKShaderTileMode.Clamp);
        return canvas.SaveLayer(new SKCanvasSaveLayerRec { Bounds = bounds, Backdrop = blur });
    }
}
```

## [04]-[RUNTIME_EFFECTS]

Each SkSL effect kind has its own entry signature, builder, and color handling:
- `SKRuntimeEffect.BuildShader(sksl)` compiles `half4 main(float2 p)` returning premultiplied color, `CreateShader` answers `null` with `out errors`
- `BuildColorFilter` expects `half4 main(half4 c)`, `BuildBlender` expects `half4 main(half4 src, half4 dst)`
- `Build*` throws `SKRuntimeEffectBuilderException` with the compiler text, a static builder that fails to compile throws at its type's first use
- `SKRuntimeShaderBuilder`, `SKRuntimeColorFilterBuilder`, and `SKRuntimeBlenderBuilder` hold zero-initialized uniforms and dispose their `Effect`
- `Uniforms[name]` copies bytes with no type conversion
- `SKRuntimeEffectUniform` converts from `float` and `int` scalars, arrays, and spans
- `SKRuntimeEffectUniform` converts from `SKPoint`, `SKPointI`, `SKPoint3`, `SKSize`, `SKSizeI`, `SKColor`, `SKColorF`, and `SKMatrix`
- `Children[name]` takes an `SKShader`, `SKColorFilter`, or `SKBlender` through `SKRuntimeEffectChild`
- `Uniforms`, `Children`, and `UniformSize` on an effect reflect its declared names and layout, a mismatched size skips the write
- `Build()` and `Build(localMatrix)` read the builder's storage, `Effect.ToShader(uniforms, children, localMatrix)` binds per-use storage
- Coordinates passed to `main` are shader-local after the local matrix, image children sample in pixel coordinates through `child.eval(coord)`
- Destination space is the working space, child inputs convert into it and plain uniforms stay as written
- `layout(color)` uniforms are read as unpremultiplied extended sRGB and convert into the working space
- `toLinearSrgb` and `fromLinearSrgb` move between the working space and linear sRGB, on an untagged destination both return their input
- `float` and `int` are high precision, `half` and `short` medium, loops need a constant bound the compiler can unroll
- Image shaders return premultiplied color, `SKImage.ToRawShader` skips conversion and premultiplication for data images

```csharp
internal static class Grades {
    private const string Source = "uniform half strength; uniform half3 luma; uniform shader source; half4 main(float2 p) { half4 c = source.eval(p); half l = dot(c.rgb, luma); return half4(mix(c.rgb, half3(l), strength), c.a); }";
    private static readonly SKRuntimeShaderBuilder Desaturate = SKRuntimeEffect.BuildShader(Source);

    public static SKShader Build(SKShader source, float strength, SKPoint3 luma) {
        using SKRuntimeEffectUniforms uniforms = new(Desaturate.Effect) { ["strength"] = strength, ["luma"] = luma };
        using SKRuntimeEffectChildren children = new(Desaturate.Effect) { ["source"] = source };
        return Desaturate.Effect.ToShader(uniforms, children);
    }
}
```

## [05]-[PICTURES_AND_RETAINED_SCENES]

Scenes decide per subtree between picture replay, rasterization, and immediate drawing:
- `SKPictureRecorder.BeginRecording(cullRect, useRTree: true)` returns the recording canvas, drawing outside `cullRect` is undefined
- `EndRecording()` returns the immutable picture
- R-tree replay under a clip visits the ops intersecting it alone, a damage-clipped replay of a large scene pays for visible ops
- `EndRecordingAsDrawable()` keeps live `SKDrawable` references
- `SKDrawable.NotifyDrawingChanged()` bumps `GenerationId` when the drawable would draw differently, `Bounds` covers every state
- `DrawPicture(picture, in matrix, paint)` with a paint draws through a layer and changes non-associative blends inside, a `null` paint replays direct
- `SKPicture.Playback(canvas)` re-executes every op
- `ApproximateOperationCount` and `ApproximateBytesUsed` probe cost, images inside are held by reference and excluded from the bytes
- Pictures hold their own references to shaders, images, and typefaces, the recording scope disposes its handles after `EndRecording`
- `SKImage.FromPicture(picture, dimensions, matrix, paint)` is a lazy image rasterized on first use and cached
- Static content at one scale takes `FromPicture`, changing or rescaled content replays
- `SKPicture.ToShader(tmx, tmy, filterMode, localMatrix, tile)` tiles a picture as a pattern, `SKShader.CreatePicture` builds the same shader
- `Serialize()` and `Deserialize(data)` round-trip within one Skia build, the binding exposes no serialization procs for embedded images and typefaces
- Retained scenes replay each stable subtree's picture under the damage clip
- Live subtrees (animating, edited, timed) draw immediately
- Ancestors of a live node replay their pictures around it, an ancestor that draws immediately redraws the whole frame per tick
- `SKNoDrawCanvas` and `SKSurface.CreateNull(width, height)` measure and warm up without pixels
- `SKRegion` is a run-length rectangle set, `IsComplex` and `Bounds` tell when to collapse
- `SKRegion.CreateRectIterator()` walks rects, `QuickReject(rect)` culls
- Damage rects round outward before `Op`, a fractional rect leaves a seam

```csharp
internal sealed record Recording(int Generation, SKPicture Picture);

internal sealed class Retained : IDisposable {
    private Option<Recording> recorded;

    public SKPicture Picture(int generation, SKRect cull, Action<SKCanvas> record) =>
        recorded.Filter(held => held.Generation == generation).Map(static held => held.Picture).IfNone(() => {
            _ = recorded.Iter(static held => held.Picture.Dispose());
            using SKPictureRecorder recorder = new();
            record(recorder.BeginRecording(cull, useRTree: true));
            SKPicture made = recorder.EndRecording();
            recorded = new Recording(generation, made);
            return made;
        });

    public static SKImage Rasterized(SKPicture picture, float scale) {
        SKRect cull = picture.CullRect;
        SKSizeI pixels = new((int)MathF.Ceiling(cull.Width * scale), (int)MathF.Ceiling(cull.Height * scale));
        return SKImage.FromPicture(picture, pixels, SKMatrix.CreateScaleTranslation(scale, scale, -cull.Left * scale, -cull.Top * scale));
    }

    public void Dispose() => _ = recorded.Iter(static held => held.Picture.Dispose());
}
```

## [06]-[SURFACES_IMAGES_AND_GPU]

Raster and GPU backends differ in snapshot cost, thread affinity, and read-back:
- `SKSurface.Create(info)` owns raster pixels, each `Create` overload has an `SKSurfaceProperties` form
- `SKSurface.Create(GRContext, GRBackendRenderTarget, origin, colorType, colorspace, props)` wraps a host framebuffer
- `SKSurface.Create(context, budgeted, info, sampleCount, props)` allocates a GPU offscreen, the `origin` overload takes `shouldCreateWithMips`
- `Snapshot()` shares pixels copy-on-write, `Snapshot(bounds)` copies a window
- `SKSurface.Draw(canvas, point, sampling, paint)` blits a retained panel surface onto another canvas without a snapshot
- `PeekPixels()` exposes raster memory until the next canvas call
- `ReadPixels(dstInfo, dstPixels, rowBytes, srcX, srcY)` copies and converts, `RequestReadPixels(info, srcRect, callback)` reads back asynchronously
- `GRContext.Flush(submit, synchronous)` and `Submit(synchronous)` are the context forms of `Flush(submit: true)`
- `SKImage.FromPixels(pixmap, releaseProc)` wraps memory the delegate keeps alive, `FromPixelCopy` copies
- Pinned arrays freed before their image dies corrupt later draws
- `SKImage.FromEncodedData` defers decoding to the first draw on the drawing thread, `IsLazyGenerated` reports it
- `ToRasterImage(ensurePixelData: true)` decodes ahead
- `SKImage.Subset(rect)` shares pixels of a window
- `SKImage.Encode(format, quality)` returns `SKData`, `SKPixmap.Encode(stream, SKPngEncoderOptions)` and its JPEG and WebP forms take codec settings
- `SKCodec.Create(stream)` and `GetPixels(info, pixels, rowBytes, options)` decode with scanline and incremental modes and animation frames
- `SKBitmap` is the mutable pixel owner, `InstallPixels(info, pixels, rowBytes, releaseProc)` wraps memory, `GetPixelSpan()` reads it
- `SKBitmap.SetImmutable()` promises no writes, `NotifyPixelsChanged()` resets caches after a write
- `SKImage.ToTextureImage(context, mipmapped, budgeted)` uploads with mipmaps for downscaling
- `SKSamplingOptions(SKFilterMode.Linear, SKMipmapMode.Linear)` samples mipmaps
- `SKCubicResampler.Mitchell` is the smooth upscale, `CatmullRom` is sharper, `SKFilterMode.Nearest` keeps pixel art
- `GRContext.CreateMetal(GRMtlBackendContext)` wraps a device and queue handle, `CreateGl`, `CreateVulkan`, and `CreateDirect3D` the other backends
- `GRContext` instances pair one-to-one with a GL context or device, calls assume it current on the calling thread, texture images die with it
- `GRContext.SetResourceCacheLimit(bytes)` purges least-recently-used resources to fit
- `PurgeUnusedResources(milliseconds)` evicts after idle, `PurgeResources()` frees everything
- `PurgeUnlockedResources(scratchResourcesOnly)` answers memory pressure, `ResetContext(state)` runs after the host changed GL state behind Skia
- `AbandonContext(releaseResources)` handles device loss and `IsAbandoned` reports it, `SKImage.IsValid(context)` goes false after abandonment
- `IsTextureBacked` tells a texture image from a raster one

```csharp
internal sealed class Gpu(nint device, nint queue) : IDisposable {
    private readonly GRContext context = GRContext.CreateMetal(new GRMtlBackendContext { DeviceHandle = device, QueueHandle = queue });

    public SKSurface Offscreen(SKImageInfo info, SKSurfaceProperties properties) => SKSurface.Create(context, budgeted: true, info, sampleCount: 1, properties);

    public SKImage Uploaded(SKImage raster) => raster.ToTextureImage(context, mipmapped: true, budgeted: true);

    public static void Present(SKSurface surface) => surface.Flush(submit: true);

    public void Pressured(bool critical) {
        if (critical) context.PurgeResources();
        else context.PurgeUnlockedResources(scratchResourcesOnly: false);
    }

    public void Dispose() => context.Dispose();
}
```

## [07]-[TEXT_PRIMITIVES]

Skia rasterizes glyphs and draws blobs, a shaping engine supplies glyph ids and positions:
- `SKTextBlob` is the immutable run container and the GPU glyph-atlas cache key by `UniqueId`
- Blobs kept across frames reuse atlas data, a blob rebuilt per frame defeats the atlas
- `SKTextBlobBuilder.AllocatePositionedRun(font, count, bounds)` takes one `SKFont` per run (one typeface, size, and paint style)
- `Glyphs` and `Positions` on the run buffer are the spans to fill, `SetGlyphs` and `SetPositions` copy spans in
- `AllocateHorizontalRun(font, count, y)` takes x positions on one baseline, `AddPositionedRun(glyphs, font, positions)` copies spans in one call
- Optional `bounds` on a run lets a draw skip when the blob misses the surface, `SKTextBlob.Bounds` gives conservative glyph bounds for invalidation
- `SKFont(typeface, size, scaleX, skewX)` is the glyph source
- `Edging` (`Alias`, `Antialias`, `SubpixelAntialias`) and `Hinting` (`None`, `Slight`, `Normal`, `Full`) select edge and grid fitting
- `Subpixel`, `LinearMetrics`, `BaselineSnap`, `Embolden`, `ForceAutoHinting`, and `EmbeddedBitmaps` complete the rasterization flags
- `SubpixelAntialias` under `Unknown` pixel geometry renders grayscale
- `BaselineSnap` applies under an axis-aligned matrix alone
- `LinearMetrics = true` ignores hinting and rounding, layout at scale 1 and 2 agree, `Subpixel` positions origins at fractions
- `GetFontMetrics(out metrics)` gives `Ascent` (negative), `Descent` (positive), `Leading` (line gap), `CapHeight`, and `XHeight`
- `SKFontMetrics.UnderlinePosition`, `UnderlineThickness`, `StrikeoutPosition`, and `StrikeoutThickness` are nullable
- `GetGlyphs(text, glyphs)` maps code points, `ContainsGlyph(codepoint)` and `ContainsGlyphs(text)` test coverage
- `GetGlyphWidths(glyphs, widths, bounds)` and `GetGlyphPositions` measure without shaping
- `SKTypeface.GetGlyphs` and `ContainsGlyphs` are obsolete, the `SKFont` members replace them
- `GetGlyphPath(glyph)` and `GetTextPath(text, positions)` return outlines for clipping, hit testing, and effects
- `SKCanvas.DrawText(blob, x, y, paint)` draws a blob with the paint's color, shader, stroke, and blend
- `SKTextBlob.GetIntercepts(upper, lower, paint)` gives the x ranges an underline skips
- `SKFontManager.Default.MatchCharacter(familyName, style, bcp47, character)` finds a fallback face holding one code point in one language
- `SKFontManager.MatchFamily(name)` resolves a family
- `SKTypeface.FromFamilyName`, `FromFile`, `FromStream`, and `FromData` construct typefaces, `OpenStream(out ttcIndex)` hands the bytes to a shaper
- `SKTypeface.VariationDesignPosition` lists the axis values a variable typeface paints with

```csharp
internal static class Glyphs {
    public static Option<SKTextBlob> Blob(SKFont font, ReadOnlySpan<ushort> ids, ReadOnlySpan<SKPoint> positions) {
        using SKTextBlobBuilder builder = new();
        SKPositionedRunBuffer run = builder.AllocatePositionedRun(font, ids.Length);
        run.SetGlyphs(ids);
        run.SetPositions(positions);
        return Optional(builder.Build());
    }

    public static SKTypeface Covering(SKFont font, int codepoint, string language) =>
        font.ContainsGlyph(codepoint) ? font.Typeface : SKFontManager.Default.MatchCharacter(font.Typeface.FamilyName, font.Typeface.FontStyle, [language], codepoint);

    public static float LineHeight(SKFont font) {
        _ = font.GetFontMetrics(out SKFontMetrics metrics);
        return metrics.Descent - metrics.Ascent + metrics.Leading;
    }
}
```

## [08]-[DOCUMENTS]

`SKDocument` writes vector output through the same canvas API:
- `SKDocument.CreatePdf(stream, metadata)` opens a document, `SKDocumentPdfMetadata.RasterDpi` sets the resolution of rasterized filters and shaders
- `BeginPage(width, height)` returns a canvas in points (`DefaultRasterDpi` is 72), `EndPage()` closes it, `Close()` writes, `Abort()` discards
- Gradients, paths, text, and images stay vector, image filters and runtime shaders rasterize at the document dpi
- `SKCanvas.DrawUrlAnnotation(rect, url)` adds a link
- `SKDocument.CreateXps(stream, dpi)` writes XPS on Windows

```csharp
internal static class Pages {
    public static void Pdf(Stream output, SKSize points, float rasterDpi, Action<SKCanvas> draw) {
        using SKDocument document = SKDocument.CreatePdf(output, new SKDocumentPdfMetadata { RasterDpi = rasterDpi });
        draw(document.BeginPage(points.Width, points.Height));
        document.EndPage();
        document.Close();
    }
}
```

# [RASM_APPUI_API_AVALONIA_SKIA]

`Avalonia.Skia` binds the Skia render backend Avalonia draws through: `UseSkia` selects the rendering subsystem, `SkiaOptions` sets the Ganesh GPU-resource policy, and `ISkiaSharpApiLeaseFeature` hands the live `SKCanvas`/`GRContext`/`SKSurface` to a custom control under a `using`-scoped lease. `SkiaSharpExtensions` bridges every Avalonia primitive to its SkiaSharp value, and `DrawingContextHelper`/`ImageSavingHelper` rasterize a visual and encode an `SKImage`, feeding the visuals rail. Every render impl below the lease is internal, reached only through Avalonia composition.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Avalonia.Skia`
- package: `Avalonia.Skia` (MIT)
- assembly: `Avalonia.Skia` (bound `lib/net10.0/Avalonia.Skia.dll`)
- namespace: `Avalonia`, `Avalonia.Skia`, `Avalonia.Skia.Helpers`
- rail: visuals

## [02]-[PUBLIC_TYPES]

[BACKEND_TYPES]: backend boot and options (`Avalonia` namespace)

| [INDEX] | [SYMBOL]                    | [TYPE_FAMILY] | [CAPABILITY]                       |
| :-----: | :-------------------------- | :------------ | :--------------------------------- |
|  [01]   | `SkiaApplicationExtensions` | static class  | `UseSkia` subsystem registrar      |
|  [02]   | `SkiaOptions`               | class         | GPU-resource + opacity policy      |
|  [03]   | `SkiaPlatform`              | static class  | manual `Initialize` + `DefaultDpi` |

[LEASE_TYPES]: SkiaSharp lease contracts and the value-conversion surface (`Avalonia.Skia` namespace)

| [INDEX] | [SYMBOL]                             | [TYPE_FAMILY] | [CAPABILITY]                                        |
| :-----: | :----------------------------------- | :------------ | :-------------------------------------------------- |
|  [01]   | `ISkiaSharpApiLeaseFeature`          | interface     | `Lease()` over a draw context                       |
|  [02]   | `ISkiaSharpApiLease`                 | interface     | `SkCanvas`/`GrContext`/`SkSurface`/`CurrentOpacity` |
|  [03]   | `ISkiaSharpPlatformGraphicsApiLease` | interface     | host GPU `Context` handle                           |
|  [04]   | `ISkiaSurface`                       | interface     | `Surface`/`CanBlit`/`Blit(SKCanvas)`                |
|  [05]   | `SkiaSharpExtensions`                | static class  | Avalonia<->SkiaSharp value conversions              |

[HELPER_TYPES]: render and encode helpers (`Avalonia.Skia.Helpers` namespace)

| [INDEX] | [SYMBOL]               | [TYPE_FAMILY] | [CAPABILITY]                          |
| :-----: | :--------------------- | :------------ | :------------------------------------ |
|  [01]   | `DrawingContextHelper` | static class  | `RenderAsync` + `TryCreateDashEffect` |
|  [02]   | `ImageSavingHelper`    | static class  | `SaveImage` `SKImage` encode          |
|  [03]   | `PixelFormatHelper`    | static class  | `ResolveColorType` -> `SKColorType`   |

## [03]-[ENTRYPOINTS]

[BACKEND_ENTRYPOINTS]: backend boot and tuning

| [INDEX] | [SURFACE]                                               | [SHAPE]  | [CAPABILITY]                          |
| :-----: | :------------------------------------------------------ | :------- | :------------------------------------ |
|  [01]   | `SkiaApplicationExtensions.UseSkia() -> AppBuilder`     | static   | select the Skia rendering subsystem   |
|  [02]   | `SkiaOptions.MaxGpuResourceSizeBytes` (`long?`)         | property | Ganesh GPU cache byte cap             |
|  [03]   | `SkiaOptions.UseOpacitySaveLayer` (`bool`)              | property | opacity-group `SaveLayer` toggle      |
|  [04]   | `SkiaOptions.UseStencilBuffers` (`bool?`)               | property | render-target stencil-buffer policy   |
|  [05]   | `SkiaPlatform.Initialize()` / `Initialize(SkiaOptions)` | static   | manual subsystem boot (headless/test) |
|  [06]   | `SkiaPlatform.DefaultDpi` (`Vector`)                    | property | DPI anchor for render helpers         |

- `SkiaOptions.UseStencilBuffers`: `null` (default) allocates stencil buffers on render targets; `false` opts out.

[LEASE_ENTRYPOINTS]: raw SkiaSharp access through render-interface leases

| [INDEX] | [SURFACE]                                          | [SHAPE]  | [CAPABILITY]                                           |
| :-----: | :------------------------------------------------- | :------- | :----------------------------------------------------- |
|  [01]   | `ISkiaSharpApiLeaseFeature.Lease()`                | instance | open a `using` canvas lease -> `ISkiaSharpApiLease`    |
|  [02]   | `ISkiaSharpApiLease.SkCanvas` (`SKCanvas`)         | property | raw immediate canvas                                   |
|  [03]   | `ISkiaSharpApiLease.GrContext` (`GRContext?`)      | property | Ganesh GPU context, null on CPU                        |
|  [04]   | `ISkiaSharpApiLease.SkSurface` (`SKSurface?`)      | property | raw backing surface, null when none                    |
|  [05]   | `ISkiaSharpApiLease.CurrentOpacity` (`double`)     | property | composited opacity for leased paints                   |
|  [06]   | `ISkiaSharpApiLease.TryLeasePlatformGraphicsApi()` | instance | GPU sub-lease -> `ISkiaSharpPlatformGraphicsApiLease?` |
|  [07]   | `ISkiaSharpPlatformGraphicsApiLease.Context`       | property | host GPU handle -> `IPlatformGraphicsContext`          |

[CONVERSION_ENTRYPOINTS]: static `SkiaSharpExtensions` value bridges — Avalonia primitive <-> SkiaSharp

| [INDEX] | [SURFACE]                                                                            | [CAPABILITY]               |
| :-----: | :----------------------------------------------------------------------------------- | :------------------------- |
|  [01]   | `ToSKPoint(Point)` / `ToSKPoint(Vector)`                                             | point/vector bridge        |
|  [02]   | `ToSKRect(Rect)` / `ToSKRectI(PixelRect)` / `ToSKRoundRect(RoundedRect)`             | rect family bridge         |
|  [03]   | `ToAvaloniaRect(SKRect)` / `ToAvaloniaPixelRect(SKRectI)`                            | reverse rect bridge        |
|  [04]   | `ToSKMatrix(Matrix)` / `ToSKMatrix44(Matrix)`                                        | 2D/4x4 matrix bridge       |
|  [05]   | `ToSKColor(Color)`                                                                   | color bridge               |
|  [06]   | `ToSkColorType(PixelFormat)` / `ToAvalonia(SKColorType)` -> `PixelFormat?`           | pixel-format round-trip    |
|  [07]   | `ToSkAlphaType(AlphaFormat)` / `ToAlphaFormat(SKAlphaType)`                          | alpha-mode round-trip      |
|  [08]   | `ToSKSamplingOptions(BitmapInterpolationMode)` / `ToSKBlendMode(BitmapBlendingMode)` | interpolation/blend bridge |
|  [09]   | `ToSKShaderTileMode(GradientSpreadMethod)`                                           | gradient-spread bridge     |
|  [10]   | `ToSKStrokeCap(PenLineCap)` / `ToSKStrokeJoin(PenLineJoin)`                          | pen-geometry bridge        |
|  [11]   | `ToSKTextAlign(TextAlignment)` / `ToAvalonia(SKTextAlign)`                           | text-align round-trip      |
|  [12]   | `ToSkia(FontStyle)` -> `SKFontStyleSlant` / `ToAvalonia(SKFontStyleSlant)`           | font-slant round-trip      |
|  [13]   | `Clone(SKPath?)` -> `SKPath?`                                                        | null-tolerant path copy    |

[RENDER_ENTRYPOINTS]: static render and encode helpers (`Avalonia.Skia.Helpers`)

| [INDEX] | [SURFACE]                                                              | [CAPABILITY]                                     |
| :-----: | :--------------------------------------------------------------------- | :----------------------------------------------- |
|  [01]   | `DrawingContextHelper.RenderAsync(SKCanvas, Visual, Rect?, Vector?)`   | render a visual onto a canvas, clip/DPI optional |
|  [02]   | `DrawingContextHelper.TryCreateDashEffect(IPen?, out SKPathEffect?)`   | pen dash-style -> `SKPathEffect`                 |
|  [03]   | `ImageSavingHelper.SaveImage(SKImage, string \| Stream, int? quality)` | encode to file or stream, quality-aware          |
|  [04]   | `PixelFormatHelper.ResolveColorType(PixelFormat?)`                     | format -> `SKColorType`, null = default          |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Raw `SKCanvas`/`GRContext`/`SKSurface` access flows only through the `using`-scoped `ISkiaSharpApiLease`, and a draw multiplies `CurrentOpacity` into its leased paints.
- `UseSkia` selects the one Skia backend; `SkiaOptions.MaxGpuResourceSizeBytes` caps the Ganesh GPU resource cache, `UseOpacitySaveLayer` routes opacity through `SaveLayer`, and `UseStencilBuffers` governs render-target stencil allocation.
- Every render impl below the lease is internal, reached only through Avalonia composition; the public surface is `UseSkia` + `SkiaOptions` + `SkiaPlatform` + the lease/conversion/helper trio.

[STACKING]:
- `SkiaSharp`(`api-skiasharp.md`): `ISkiaSharpApiLease.Lease()` yields the live `SKCanvas`/`GRContext`/`SKSurface` a custom control draws through, sharing Avalonia's GPU context; `TryLeasePlatformGraphicsApi()` borrows the host `GRContext` rather than constructing `GRContext.CreateMetal`/`CreateVulkan`, and interior geometry math stays `SKMatrix`/`SKPath`/`SKRect`.
- `Avalonia.Headless`(`api-headless.md`): the headless backend selects Skia so render-hash proof lanes hash real Skia pixels rather than a stub surface.
- within-lib: a leased-canvas draw composes `SkiaSharpExtensions.ToSKRect`/`ToSKMatrix`/`ToSKColor`/`ToSKSamplingOptions` to translate Avalonia geometry and paint into Skia calls at the boundary; `ToSkColorType`/`ToAvalonia(SKColorType)` and `PixelFormatHelper.ResolveColorType` own the pixel-format round-trip the offscreen color-managed encode keys on, and capture rides `DrawingContextHelper.RenderAsync` into `ImageSavingHelper.SaveImage`.

[LOCAL_ADMISSION]:
- A custom visual draws through the leased `SKCanvas`, crosses Avalonia values through `SkiaSharpExtensions` at the boundary, and emits deterministic bytes through `DrawingContextHelper.RenderAsync` and `ImageSavingHelper.SaveImage`.

[RAIL_LAW]:
- Package: `Avalonia.Skia`
- Owns: the Skia render-backend selection (`UseSkia`), the Ganesh GPU-resource policy (`SkiaOptions`), the raw SkiaSharp lease surface, every Avalonia<->SkiaSharp value conversion, and the visual-render/encode helpers
- Accept: raw `SKCanvas`/`GRContext`/`SKSurface` flows through the `using`-scoped `ISkiaSharpApiLease` with `CurrentOpacity` multiplied into leased paints; value crossing rides `SkiaSharpExtensions`; capture rides `DrawingContextHelper.RenderAsync` and `ImageSavingHelper.SaveImage`
- Reject: a parallel render backend beside Skia; out-of-lease canvas mutation; a `GRContext` factory named at a draw call site; a hand-rolled Avalonia<->Skia value converter `SkiaSharpExtensions` already owns

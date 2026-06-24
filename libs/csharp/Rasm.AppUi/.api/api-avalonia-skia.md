# [RASM_APPUI_API_AVALONIA_SKIA]

`Avalonia.Skia` is the Skia render backend for Avalonia 12: the `UseSkia` builder extension and `SkiaOptions` boot the SkiaSharp 3 subsystem, `ISkiaSharpApiLeaseFeature`/`ISkiaSharpApiLease` hand out the raw `SKCanvas`/`GRContext`/`SKSurface` under a disposable lease, `SkiaSharpExtensions` is the full Avalonia-to-SkiaSharp value-conversion surface, and `DrawingContextHelper`/`ImageSavingHelper`/`PixelFormatHelper` render visuals onto and encode raw canvases. The public boot/option types (`UseSkia`, `SkiaOptions`, `SkiaPlatform`) live in the `Avalonia` namespace; the lease, conversion, and helper types live in `Avalonia.Skia`/`Avalonia.Skia.Helpers`. Every render impl below the lease (`PlatformRenderInterface`, `DrawingContextImpl`, GPU render targets, bitmap impls, `ISkiaGpu`) is `internal` and reached only through Avalonia composition.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Avalonia.Skia`
- package: `Avalonia.Skia` (12.0.4, MIT)
- assembly: `Avalonia.Skia` (consumer-bound `lib/net8.0`; net10 binds this asset)
- namespace: `Avalonia` (`UseSkia`, `SkiaOptions`, `SkiaPlatform`)
- namespace: `Avalonia.Skia` (lease contracts, `SkiaSharpExtensions`, `ISkiaSurface`)
- namespace: `Avalonia.Skia.Helpers` (`DrawingContextHelper`, `ImageSavingHelper`, `PixelFormatHelper`)
- asset: runtime library (SkiaSharp 3.119.4 + HarfBuzz natives are the centrally-pinned runtime family)
- rail: visuals

## [02]-[PUBLIC_TYPES]

[BACKEND_TYPES]: backend boot and options (`Avalonia` namespace)
- rail: visuals

| [INDEX] | [SYMBOL]                    | [TYPE_FAMILY] | [RAIL]                                  |
| :-----: | :-------------------------- | :------------ | :-------------------------------------- |
|  [01]   | `SkiaApplicationExtensions` | static class  | `UseSkia` rendering-subsystem registrar |
|  [02]   | `SkiaOptions`               | options class | `MaxGpuResourceSizeBytes`/`UseOpacitySaveLayer` |
|  [03]   | `SkiaPlatform`              | static class  | manual `Initialize` + `DefaultDpi` anchor |

[LEASE_TYPES]: SkiaSharp API lease contracts (`Avalonia.Skia` namespace)
- rail: visuals

| [INDEX] | [SYMBOL]                             | [TYPE_FAMILY]      | [RAIL]                            |
| :-----: | :----------------------------------- | :----------------- | :-------------------------------- |
|  [01]   | `ISkiaSharpApiLeaseFeature`          | render-interface feature | `Lease()` open over a draw context |
|  [02]   | `ISkiaSharpApiLease`                 | disposable canvas lease | `SkCanvas`/`GrContext`/`SkSurface`/`CurrentOpacity` |
|  [03]   | `ISkiaSharpPlatformGraphicsApiLease` | disposable GPU lease | `Context` (`IPlatformGraphicsContext`) |
|  [04]   | `ISkiaSurface`                       | disposable surface contract | platform-render owned surface handle |

[HELPER_TYPES]: conversion and render helpers (`Avalonia.Skia.Helpers` namespace)
- rail: visuals

| [INDEX] | [SYMBOL]               | [TYPE_FAMILY] | [RAIL]                                  |
| :-----: | :--------------------- | :------------ | :-------------------------------------- |
|  [01]   | `SkiaSharpExtensions`  | static class  | Avalonia<->SkiaSharp value conversions  |
|  [02]   | `DrawingContextHelper` | static class  | `RenderAsync` visual-to-canvas + `TryCreateDashEffect` |
|  [03]   | `ImageSavingHelper`    | static class  | `SaveImage` `SKImage` encode (file/stream + quality) |
|  [04]   | `PixelFormatHelper`    | static class  | `ResolveColorType(PixelFormat?)` -> `SKColorType` |

## [03]-[ENTRYPOINTS]

[BACKEND_ENTRYPOINTS]: backend boot and tuning
- rail: visuals

| [INDEX] | [SURFACE]                                | [SURFACE_ROOT]              | [RAIL]                         |
| :-----: | :--------------------------------------- | :-------------------------- | :----------------------------- |
|  [01]   | `UseSkia()`                              | `SkiaApplicationExtensions` | `AppBuilder.UseRenderingSubsystem("Skia")` |
|  [02]   | `MaxGpuResourceSizeBytes` (`long?`, default `29491200`) | `SkiaOptions`  | Ganesh GPU cache byte cap (~28 MiB) |
|  [03]   | `UseOpacitySaveLayer` (`bool`)           | `SkiaOptions`               | opacity-group `SaveLayer` toggle |
|  [04]   | `Initialize()` / `Initialize(SkiaOptions)` | `SkiaPlatform`            | manual subsystem boot (headless/test) |
|  [05]   | `DefaultDpi` (`Vector` 96×96)            | `SkiaPlatform`              | DPI anchor for render helpers  |

[LEASE_ENTRYPOINTS]: raw SkiaSharp access through render-interface leases
- rail: visuals

| [INDEX] | [SURFACE]                       | [SURFACE_ROOT]                       | [RAIL]                         |
| :-----: | :------------------------------ | :----------------------------------- | :----------------------------- |
|  [01]   | `Lease()`                       | `ISkiaSharpApiLeaseFeature`          | open a `using` canvas lease    |
|  [02]   | `SkCanvas` (`SKCanvas`)         | `ISkiaSharpApiLease`                 | raw immediate canvas           |
|  [03]   | `GrContext` (`GRContext?`)      | `ISkiaSharpApiLease`                 | Ganesh GPU context (null on CPU) |
|  [04]   | `SkSurface` (`SKSurface?`)      | `ISkiaSharpApiLease`                 | raw backing surface (null when none) |
|  [05]   | `CurrentOpacity` (`double`)     | `ISkiaSharpApiLease`                 | composited opacity to multiply into paints |
|  [06]   | `TryLeasePlatformGraphicsApi()` | `ISkiaSharpApiLease`                 | -> `ISkiaSharpPlatformGraphicsApiLease?` (GPU sub-lease) |
|  [07]   | `Context` (`IPlatformGraphicsContext`) | `ISkiaSharpPlatformGraphicsApiLease` | host-shared GPU context handle |

[CONVERSION_ENTRYPOINTS]: Avalonia-to-SkiaSharp value bridges (`SkiaSharpExtensions`)
- rail: visuals

| [INDEX] | [SURFACE]                                              | [RAIL]                          |
| :-----: | :----------------------------------------------------- | :------------------------------ |
|  [01]   | `ToSKPoint(Point)` / `ToSKPoint(Vector)`               | point/vector bridge             |
|  [02]   | `ToSKRect(Rect)` / `ToSKRectI(PixelRect)` / `ToSKRoundRect(RoundedRect)` | rect family bridge |
|  [03]   | `ToAvaloniaRect(SKRect)` / `ToAvaloniaPixelRect(SKRectI)` | reverse rect bridge          |
|  [04]   | `ToSKMatrix(Matrix)` / `ToSKMatrix44(Matrix)`          | 2D/4×4 matrix bridge            |
|  [05]   | `ToSKColor(Color)`                                     | color bridge                    |
|  [06]   | `ToSkColorType(PixelFormat)` / `ToAvalonia(SKColorType)` -> `PixelFormat?` | pixel-format round-trip |
|  [07]   | `ToSkAlphaType(AlphaFormat)` / `ToAlphaFormat(SKAlphaType)` | alpha-mode round-trip      |
|  [08]   | `ToSKSamplingOptions(BitmapInterpolationMode)` / `ToSKBlendMode(BitmapBlendingMode)` | interpolation/blend bridge |
|  [09]   | `ToSKShaderTileMode(GradientSpreadMethod)`             | gradient-spread bridge          |
|  [10]   | `ToSKStrokeCap(PenLineCap)` / `ToSKStrokeJoin(PenLineJoin)` | pen-geometry bridge        |
|  [11]   | `ToSKTextAlign(TextAlignment)` / `ToAvalonia(SKTextAlign)` | text-align round-trip       |
|  [12]   | `ToSkia(FontStyle)` -> `SKFontStyleSlant` / `ToAvalonia(SKFontStyleSlant)` | font-slant round-trip |
|  [13]   | `Clone(SKPath?)` -> `SKPath?`                          | null-tolerant path copy         |

[RENDER_ENTRYPOINTS]: visual rendering onto raw canvases (`Avalonia.Skia.Helpers`)
- rail: visuals

| [INDEX] | [SURFACE]                                            | [SURFACE_ROOT]         | [RAIL]                         |
| :-----: | :--------------------------------------------------- | :--------------------- | :----------------------------- |
|  [01]   | `RenderAsync(SKCanvas, Visual)`                      | `DrawingContextHelper` | immediate visual render (default DPI/bounds) |
|  [02]   | `RenderAsync(SKCanvas, Visual, Rect clipRect, Vector dpi)` | `DrawingContextHelper` | clipped/DPI-pinned visual render |
|  [03]   | `TryCreateDashEffect(IPen?, out SKPathEffect?)`      | `DrawingContextHelper` | pen dash-style -> `SKPathEffect` |
|  [04]   | `SaveImage(SKImage, string fileName, int? quality)`  | `ImageSavingHelper`    | encode to file (quality-aware) |
|  [05]   | `SaveImage(SKImage, Stream, int? quality)`           | `ImageSavingHelper`    | encode to stream (quality-aware) |
|  [06]   | `ResolveColorType(PixelFormat?)`                     | `PixelFormatHelper`    | format -> `SKColorType` (platform default when null) |

## [04]-[IMPLEMENTATION_LAW]

[BACKEND_LAW]:
- Package: `Avalonia.Skia`
- Owns: the Skia render-backend selection (`UseSkia`), the Ganesh GPU-resource policy (`SkiaOptions`), and the raw SkiaSharp lease surface
- Stacks: `SkiaSharp` 3 is the drawing substrate (`.api/api-skiasharp.md`); the GPU pipeline leases the host-shared `GRContext` through `ISkiaSharpApiLease.TryLeasePlatformGraphicsApi()` (`Render/pipeline#RENDER_GRAPH`) rather than constructing `GRContext.CreateMetal`/`CreateVulkan`, and `Render/capture#DRAW_CAPSULE` leases the in-flight canvas through `ISkiaSharpApiLeaseFeature.Lease()` for borrowed draws; `Avalonia.Headless` selects this backend with `UseHeadlessDrawing=false` so render-hash proof lanes hash real Skia pixels (`Render/capture#EVIDENCE`)
- Accept: raw `SKCanvas`/`GRContext`/`SKSurface` access flows only through the `using`-scoped `ISkiaSharpApiLease`, multiplying `CurrentOpacity` into leased paints
- Reject: a parallel render backend beside Skia; out-of-lease canvas mutation; a pass body naming a backend `GRContext` factory at a call site (`Render/pipeline` PROHIBITION host-API-in-arm)

[CONVERSION_LAW]:
- Package: `Avalonia.Skia`
- Owns: every Avalonia-primitive-to-SkiaSharp value conversion (`SkiaSharpExtensions`) and the visual-to-canvas/encode helpers
- Stacks: a leased-canvas draw composes `ToSKRect`/`ToSKMatrix`/`ToSKColor`/`ToSKSamplingOptions` to translate Avalonia geometry/paint into Skia calls without a hand-rolled converter; `ToSkColorType`/`ToAvalonia(SKColorType)` and `PixelFormatHelper.ResolveColorType` own the pixel-format round-trip the offscreen color-managed encode rail keys on (`Render/capture#ENCODE`)
- Accept: visual capture rides `DrawingContextHelper.RenderAsync` and image encode rides `ImageSavingHelper.SaveImage` with the quality and stream overloads
- Reject: re-deriving an Avalonia<->Skia value converter that `SkiaSharpExtensions` already owns; documenting `PenHelper`/`SKPathHelper` (both `internal`) as consumer surface

[INTERNAL_SURFACE_LAW]:
- Package: `Avalonia.Skia`
- Owns: `PlatformRenderInterface`, `DrawingContextImpl`, the GPU render targets (`SurfaceRenderTarget`, `FramebufferRenderTarget`, `GlRenderTarget`, `SkiaGpuRenderTarget`), the bitmap impls (`ImmutableBitmap`, `WriteableBitmapImpl`, `RenderTargetBitmapImpl`), `SkiaContext`, `FontManagerImpl`, and `ISkiaGpu`/`IDrawableBitmapImpl` as internal types
- Accept: render-interface behavior is reached through Avalonia composition and the public lease; the public surface is `UseSkia` + `SkiaOptions` + `SkiaPlatform` + the lease/conversion/helper trio
- Reject: documenting any internal render impl, GPU target, or bitmap impl as a public managed type

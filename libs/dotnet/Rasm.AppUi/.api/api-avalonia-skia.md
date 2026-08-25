# [RASM_APPUI_API_AVALONIA_SKIA]

`Avalonia.Skia` binds the Skia render backend: `UseSkia` selects the subsystem, `SkiaOptions` sets Ganesh resource and opacity policy, and `ISkiaSharpApiLeaseFeature` hands the live `SKCanvas`/`GRContext`/`SKSurface` to an `ICustomDrawOperation` under a `using`-scoped lease. `SkiaSharpExtensions` crosses every Avalonia primitive to its SkiaSharp value, `DrawingContextHelper` and `ImageSavingHelper` rasterize and encode, and every render impl below the lease stays internal.

## [01]-[PUBLIC_TYPES]

[BACKEND_TYPES]: backend boot and options (`Avalonia` namespace)

| [INDEX] | [SYMBOL]                    | [TYPE_FAMILY] | [CAPABILITY]                       |
| :-----: | :-------------------------- | :------------ | :--------------------------------- |
|  [01]   | `SkiaApplicationExtensions` | static class  | `UseSkia` subsystem registrar      |
|  [02]   | `SkiaOptions`               | class         | GPU-resource + opacity policy      |
|  [03]   | `SkiaPlatform`              | static class  | manual `Initialize` + `DefaultDpi` |

[CUSTOM_DRAW_TYPES]: the draw host a custom operation renders through (`Avalonia.Base`)

| [INDEX] | [SYMBOL]                            | [TYPE_FAMILY] | [CAPABILITY]                                        |
| :-----: | :---------------------------------- | :------------ | :-------------------------------------------------- |
|  [01]   | `ICustomDrawOperation`              | interface     | `IEquatable<>` + `IDisposable` scene-graph node     |
|  [02]   | `ImmediateDrawingContext`           | sealed class  | the render context, also `IOptionalFeatureProvider` |
|  [03]   | `DrawingContext`                    | abstract      | retained context minting the custom node            |
|  [04]   | `IOptionalFeatureProvider`          | interface     | `TryGetFeature(Type)` backend probe                 |
|  [05]   | `OptionalFeatureProviderExtensions` | static class  | typed `TryGetFeature<T>` over the probe             |
|  [06]   | `IBlurEffect` / `IDropShadowEffect` | interface     | the two effect shapes Skia renders                  |

[LEASE_TYPES]: SkiaSharp lease contracts and the value-conversion surface (`Avalonia.Skia` namespace)

| [INDEX] | [SYMBOL]                             | [TYPE_FAMILY] | [CAPABILITY]                                        |
| :-----: | :----------------------------------- | :------------ | :-------------------------------------------------- |
|  [01]   | `ISkiaSharpApiLeaseFeature`          | interface     | `Lease()` over a draw context                       |
|  [02]   | `ISkiaSharpApiLease`                 | interface     | `SkCanvas`/`GrContext`/`SkSurface`/`CurrentOpacity` |
|  [03]   | `ISkiaSharpPlatformGraphicsApiLease` | interface     | platform GPU `Context` handle                       |
|  [04]   | `ISkiaSurface`                       | interface     | `Surface`/`CanBlit`/`Blit(SKCanvas)`                |
|  [05]   | `SkiaSharpExtensions`                | static class  | Avalonia<->SkiaSharp value conversions              |

[HELPER_TYPES]: render and encode helpers (`Avalonia.Skia.Helpers` namespace)

| [INDEX] | [SYMBOL]               | [TYPE_FAMILY] | [CAPABILITY]                          |
| :-----: | :--------------------- | :------------ | :------------------------------------ |
|  [01]   | `DrawingContextHelper` | static class  | `RenderAsync` + `TryCreateDashEffect` |
|  [02]   | `ImageSavingHelper`    | static class  | `SaveImage` `SKImage` encode          |
|  [03]   | `PixelFormatHelper`    | static class  | `ResolveColorType` -> `SKColorType`   |

## [02]-[ENTRYPOINTS]

[BACKEND_ENTRYPOINTS]: backend boot and tuning

| [INDEX] | [SURFACE]                                               | [SHAPE]  | [CAPABILITY]                          |
| :-----: | :------------------------------------------------------ | :------- | :------------------------------------ |
|  [01]   | `SkiaApplicationExtensions.UseSkia() -> AppBuilder`     | static   | select the Skia rendering subsystem   |
|  [02]   | `SkiaOptions.MaxGpuResourceSizeBytes` (`long?`)         | property | Ganesh GPU cache byte cap             |
|  [03]   | `SkiaOptions.UseOpacitySaveLayer` (`bool`)              | property | opacity-group `SaveLayer` toggle      |
|  [04]   | `SkiaOptions.UseStencilBuffers` (`bool?`)               | property | render-target stencil-buffer policy   |
|  [05]   | `SkiaPlatform.Initialize()` / `Initialize(SkiaOptions)` | static   | manual subsystem boot (headless/test) |
|  [06]   | `SkiaPlatform.DefaultDpi` (`Vector`)                    | property | 96x96 DPI anchor for render helpers   |

- `SkiaOptions.MaxGpuResourceSizeBytes`: every non-null value reaches `GRContext.SetResourceCacheLimit`; `null` leaves Skia's own limit standing.
- `SkiaOptions.UseStencilBuffers`: `null` (default) allocates stencil buffers on render targets; `false` opts out.

[CUSTOM_DRAW_ENTRYPOINTS]: the custom-draw rail carrying a draw call down to the Skia lease

| [INDEX] | [SURFACE]                                              | [SHAPE]  | [CAPABILITY]                                        |
| :-----: | :----------------------------------------------------- | :------- | :-------------------------------------------------- |
|  [01]   | `DrawingContext.Custom(ICustomDrawOperation)`          | instance | enqueue the operation into the retained render data |
|  [02]   | `ICustomDrawOperation.Bounds` (`Rect`)                 | property | global-coordinate content bounds                    |
|  [03]   | `ICustomDrawOperation.HitTest(Point)`                  | instance | global-coordinate hit test, never recursing         |
|  [04]   | `ICustomDrawOperation.Render(ImmediateDrawingContext)` | instance | the one draw callback the lease opens inside        |
|  [05]   | `ImmediateDrawingContext.TryGetFeature(Type)`          | instance | `object?` backend-feature probe                     |
|  [06]   | `OptionalFeatureProviderExtensions.TryGetFeature<T>()` | static   | typed probe, also a `(out T) -> bool` overload      |
|  [07]   | `ImmediateDrawingContext.PlatformImpl`                 | property | `IDrawingContextImpl` the probe resolves against    |
|  [08]   | `ImmediateDrawingContext.CurrentTransform`             | property | `Matrix` already applied to the leased canvas       |
|  [09]   | `DrawingContext.PushEffect(IEffect, Rect)`             | instance | offscreen effect scope over the bounds              |

- `ImmediateDrawingContext.TryGetFeature`: forwards to `PlatformImpl.GetFeature`, and the Skia impl answers `ISkiaSharpApiLeaseFeature` alone — every other type reads null, which is the non-Skia-backend signal.
- `ICustomDrawOperation.Render`: `ImmediateDrawingContext` carries no `PushEffect`, so push the effect scope on the retained `DrawingContext` around `Custom`.

[LEASE_ENTRYPOINTS]: raw SkiaSharp access through render-interface leases

| [INDEX] | [SURFACE]                                          | [SHAPE]  | [CAPABILITY]                                           |
| :-----: | :------------------------------------------------- | :------- | :----------------------------------------------------- |
|  [01]   | `ISkiaSharpApiLeaseFeature.Lease()`                | instance | open a `using` canvas lease -> `ISkiaSharpApiLease`    |
|  [02]   | `ISkiaSharpApiLease.SkCanvas` (`SKCanvas`)         | property | raw immediate canvas                                   |
|  [03]   | `ISkiaSharpApiLease.GrContext` (`GRContext?`)      | property | Ganesh GPU context, null on CPU                        |
|  [04]   | `ISkiaSharpApiLease.SkSurface` (`SKSurface?`)      | property | raw backing surface, null when none                    |
|  [05]   | `ISkiaSharpApiLease.CurrentOpacity` (`double`)     | property | composited opacity for leased paints                   |
|  [06]   | `ISkiaSharpApiLease.TryLeasePlatformGraphicsApi()` | instance | GPU sub-lease -> `ISkiaSharpPlatformGraphicsApiLease?` |
|  [07]   | `ISkiaSharpPlatformGraphicsApiLease.Context`       | property | platform GPU context -> `IPlatformGraphicsContext`     |

- `ISkiaSharpApiLease.Dispose`: restores the canvas matrix captured at lease open, so a leased `SKCanvas` transform never escapes the scope.
- `TryLeasePlatformGraphicsApi`: flushes the `GRContext` on open and calls `ResetContext(All)` on dispose; while the sub-lease is open, `SkCanvas`/`SkSurface`/`CurrentOpacity` each throw `InvalidOperationException`.
- `ISkiaSharpApiLease.GrContext`: reads through without the lease check, so it answers even inside an open platform sub-lease.

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

| [INDEX] | [SURFACE]                                                                      | [CAPABILITY]                                     |
| :-----: | :----------------------------------------------------------------------------- | :----------------------------------------------- |
|  [01]   | `DrawingContextHelper.RenderAsync(SKCanvas, Visual)`                           | render a visual onto a canvas at default DPI     |
|  [02]   | `DrawingContextHelper.RenderAsync(SKCanvas, Visual, Rect, Vector)`             | same render, explicit clip rect and DPI          |
|  [03]   | `DrawingContextHelper.TryCreateDashEffect(IPen?, out SKPathEffect?)`           | pen dash-style -> `SKPathEffect`                 |
|  [04]   | `ImageSavingHelper.SaveImage(SKImage, string \| Stream, int?)`                 | encode to file or stream, quality-aware          |
|  [05]   | `ImageSavingHelper.SaveImage(SKImage, string \| Stream, BitmapEncoderOptions)` | encode under an explicit codec option set        |
|  [06]   | `PixelFormatHelper.ResolveColorType(PixelFormat?)`                             | format -> `SKColorType`, null = platform default |

## [03]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Raw `SKCanvas`/`GRContext`/`SKSurface` access flows only through the `using`-scoped `ISkiaSharpApiLease`, and a draw multiplies `CurrentOpacity` into its leased paints.
- `UseSkia` selects the one Skia backend; `SkiaOptions.MaxGpuResourceSizeBytes` caps the Ganesh GPU resource cache, `UseOpacitySaveLayer` routes opacity through `SaveLayer`, and `UseStencilBuffers` governs render-target stencil allocation.
- Every render impl below the lease is internal, reached only through Avalonia composition; the public surface is `UseSkia` + `SkiaOptions` + `SkiaPlatform` + the lease/custom-draw/conversion/helper set.
- `DrawingContext.Custom(op)` opens the custom-draw rail, lands in `op.Render(ImmediateDrawingContext)`, and reaches Skia by probing `TryGetFeature<ISkiaSharpApiLeaseFeature>()` on that context; `op.Bounds` in global coordinates drives the dirty rect and `op.HitTest` answers picking without recursing to children.
- `PushEffect(effect, bounds)` renders offscreen: it inflates `bounds` by the effect's own output padding — one `ceil(radius)+1` ring for a blur, the offset-translated blur box clamped per edge for a drop shadow — then opens exactly one `SaveLayer` at that rect carrying the paint's `SKImageFilter`, and `PopEffect` restores it.
- Skia builds that filter for two shapes only: `IBlurEffect` -> `SKImageFilter.CreateBlur` and `IDropShadowEffect` -> `CreateDropShadow` with alpha folded from the shadow color, its opacity, and the current composited opacity.
- `PushEffect` admits the built-in blur and drop shadow alone: its padding computation runs first and throws `ArgumentException` on every other `IEffect` shape, so a custom shape never reaches the Skia impl.
- `IBlurEffect` at non-positive radius yields a null filter, so the `SaveLayer` still opens and the scope buys an offscreen for no visual change — gate the scope on a positive radius rather than pushing it unconditionally.
- Acrylic paints as one shader stack, never a backdrop read: material color and tint color compose as two color shaders, then compose again with a process-wide 256x256 noise bitmap shader tiled in both axes under a `0.0225` alpha filter, and `AcrylicBackgroundSource.Digger` sets `SKBlendMode.Src` on top, punching the composited stack straight through the render layers beneath it.
- `DrawRectangle` draws each box shadow with no `SaveLayer`: every pass builds one antialiased paint carrying `CreateBlur` at the shadow's radius and the shadow color scaled by the current opacity, then draws under `Canvas.Save`/restore and a clip. Outer shadows draw first under a `Difference` clip against the rect, the fill brush draws next, inset shadows follow under an `Intersect` clip drawing `AreaCastingShadowInHole` as a round-rect difference, and the pen stroke closes.
- Blur radius crosses to Skia sigma as `0.288675 * radius + 0.5`, the one conversion every blur, drop shadow, and box shadow shares.

[STACKING]:
- `SkiaSharp`(`api-skiasharp.md`): `ISkiaSharpApiLease.Lease()` yields the live `SKCanvas`/`GRContext`/`SKSurface` a custom control draws through, sharing Avalonia's GPU context; `TryLeasePlatformGraphicsApi()` borrows the compositor's own `GRContext` rather than constructing `GRContext.CreateMetal`/`CreateVulkan` — on macOS its `Context` downcasts to the `IMetalDevice` device-plus-command-queue pair, Avalonia's own Metal state and never an embedding host's — and interior geometry math stays `SKMatrix`/`SKPath`/`SKRect`.
- `Avalonia`(`api-avalonia-gpu-interop.md`): `CompositionCustomVisualHandler.OnRender(ImmediateDrawingContext)` reaches this same lease through `TryGetFeature<ISkiaSharpApiLeaseFeature>()`, so a composition-thread custom visual and a control-thread `ICustomDrawOperation` share one Skia draw rail.
- `Avalonia.Headless`(`api-headless.md`): the headless backend selects Skia so render-hash proof lanes hash real Skia pixels rather than a stub surface.
- within-lib: a leased-canvas draw composes `SkiaSharpExtensions.ToSKRect`/`ToSKMatrix`/`ToSKColor`/`ToSKSamplingOptions` to translate Avalonia geometry and paint into Skia calls at the boundary; `ToSkColorType`/`ToAvalonia(SKColorType)` and `PixelFormatHelper.ResolveColorType` own the pixel-format round-trip the offscreen color-managed encode keys on, and capture rides `DrawingContextHelper.RenderAsync` into `ImageSavingHelper.SaveImage`.

[LOCAL_ADMISSION]:
- Every custom visual draws through the leased `SKCanvas`, crosses Avalonia values through `SkiaSharpExtensions` at the boundary, and emits deterministic bytes through `DrawingContextHelper.RenderAsync` and `ImageSavingHelper.SaveImage`.
- `ICustomDrawOperation` carries global-coordinate `Bounds`, answers `HitTest` from its own geometry, and holds every Skia handle inside the `Render` lease scope.
- Blur and drop shadow ride the `IEffect` scope; every other visual effect composes from primitives inside the leased draw.

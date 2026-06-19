# [RASM_APPUI_API_AVALONIA_SKIA]

`Avalonia.Skia` supplies the Skia render backend: the `UseSkia` builder extension, `SkiaOptions`, SkiaSharp API leases, type converters, and canvas render helpers.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Avalonia.Skia`
- package: `Avalonia.Skia`
- assembly: `Avalonia.Skia`
- namespace: `Avalonia`
- namespace: `Avalonia.Skia`
- namespace: `Avalonia.Skia.Helpers`
- asset: runtime library
- rail: visuals

## [02]-[PUBLIC_TYPES]

[BACKEND_TYPES]: backend boot and options
- rail: visuals

| [INDEX] | [SYMBOL]                    | [RAIL]            |
| :-----: | :-------------------------- | :---------------- |
|  [01]   | `SkiaApplicationExtensions` | builder extension |
|  [02]   | `SkiaOptions`               | backend options   |
|  [03]   | `SkiaPlatform`              | platform boot     |

[LEASE_TYPES]: SkiaSharp API lease contracts
- rail: visuals

| [INDEX] | [SYMBOL]                             | [RAIL]             |
| :-----: | :----------------------------------- | :----------------- |
|  [01]   | `ISkiaSharpApiLeaseFeature`          | lease feature      |
|  [02]   | `ISkiaSharpApiLease`                 | canvas lease       |
|  [03]   | `ISkiaSharpPlatformGraphicsApiLease` | platform GPU lease |

[HELPER_TYPES]: conversion and render helpers
- rail: visuals

| [INDEX] | [SYMBOL]               | [RAIL]             |
| :-----: | :--------------------- | :----------------- |
|  [01]   | `SkiaSharpExtensions`  | type conversion    |
|  [02]   | `DrawingContextHelper` | visual-to-canvas   |
|  [03]   | `ImageSavingHelper`    | `SKImage` encoding |
|  [04]   | `PixelFormatHelper`    | format resolution  |

## [03]-[ENTRYPOINTS]

[BACKEND_ENTRYPOINTS]: backend boot and tuning
- rail: visuals

| [INDEX] | [SURFACE]                 | [SURFACE_ROOT]              | [RAIL]           |
| :-----: | :------------------------ | :-------------------------- | :--------------- |
|  [01]   | `UseSkia`                 | `SkiaApplicationExtensions` | backend select   |
|  [02]   | `MaxGpuResourceSizeBytes` | `SkiaOptions`               | GPU cache cap    |
|  [03]   | `UseOpacitySaveLayer`     | `SkiaOptions`               | opacity layering |
|  [04]   | `Initialize`              | `SkiaPlatform`              | manual boot      |
|  [05]   | `DefaultDpi`              | `SkiaPlatform`              | DPI anchor       |

[LEASE_ENTRYPOINTS]: raw SkiaSharp access through render interface leases
- rail: visuals

| [INDEX] | [SURFACE]                     | [SURFACE_ROOT]              | [RAIL]         |
| :-----: | :---------------------------- | :-------------------------- | :------------- |
|  [01]   | `Lease`                       | `ISkiaSharpApiLeaseFeature` | lease open     |
|  [02]   | `SkCanvas`                    | `ISkiaSharpApiLease`        | raw canvas     |
|  [03]   | `GrContext`                   | `ISkiaSharpApiLease`        | GPU context    |
|  [04]   | `SkSurface`                   | `ISkiaSharpApiLease`        | raw surface    |
|  [05]   | `CurrentOpacity`              | `ISkiaSharpApiLease`        | opacity state  |
|  [06]   | `TryLeasePlatformGraphicsApi` | `ISkiaSharpApiLease`        | platform lease |

[CONVERSION_ENTRYPOINTS]: Avalonia-to-SkiaSharp type bridges
- rail: visuals

| [INDEX] | [SURFACE]             | [SURFACE_ROOT]        | [RAIL]         |
| :-----: | :-------------------- | :-------------------- | :------------- |
|  [01]   | `ToSKPoint`           | `SkiaSharpExtensions` | point bridge   |
|  [02]   | `ToSKRect`            | `SkiaSharpExtensions` | rect bridge    |
|  [03]   | `ToSKMatrix`          | `SkiaSharpExtensions` | matrix bridge  |
|  [04]   | `ToSKColor`           | `SkiaSharpExtensions` | color bridge   |
|  [05]   | `ToSkColorType`       | `SkiaSharpExtensions` | pixel format   |
|  [06]   | `ToAvaloniaRect`      | `SkiaSharpExtensions` | reverse bridge |
|  [07]   | `ToSKSamplingOptions` | `SkiaSharpExtensions` | interpolation  |

[RENDER_ENTRYPOINTS]: visual rendering onto raw canvases
- rail: visuals

| [INDEX] | [SURFACE]             | [SURFACE_ROOT]         | [RAIL]           |
| :-----: | :-------------------- | :--------------------- | :--------------- |
|  [01]   | `RenderAsync`         | `DrawingContextHelper` | visual-to-canvas |
|  [02]   | `TryCreateDashEffect` | `DrawingContextHelper` | dash effect      |
|  [03]   | `SaveImage`           | `ImageSavingHelper`    | image encode     |

## [04]-[IMPLEMENTATION_LAW]

[BACKEND_LAW]:
- Package: `Avalonia.Skia`
- Owns: the Skia render backend selection, GPU resource policy, and raw SkiaSharp leases
- Accept: raw `SKCanvas` access flows through `ISkiaSharpApiLeaseFeature` leases
- Reject: parallel render backends or out-of-lease canvas mutation

[INTERNAL_SURFACE_LAW]:
- Package: `Avalonia.Skia`
- Owns: `PlatformRenderInterface`, `DrawingContextImpl`, GPU targets, and bitmap impls as internal types
- Accept: render-interface behavior is reached through Avalonia composition and leases
- Reject: documenting internal render impls as public managed types

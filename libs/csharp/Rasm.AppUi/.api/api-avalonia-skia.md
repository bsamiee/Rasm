# [RASM_APPUI_API_AVALONIA_SKIA]

`Avalonia.Skia` supplies the Skia render backend: the `UseSkia` builder extension, `SkiaOptions`, SkiaSharp API leases, type converters, and canvas render helpers.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Avalonia.Skia`
- package: `Avalonia.Skia`
- assembly: `Avalonia.Skia`
- namespace: `Avalonia`
- namespace: `Avalonia.Skia`
- namespace: `Avalonia.Skia.Helpers`
- asset: runtime library
- rail: visuals

## [2]-[PUBLIC_TYPES]

[BACKEND_TYPES]: backend boot and options
- rail: visuals

| [INDEX] | [SYMBOL]                    | [RAIL]            |
| :-----: | :-------------------------- | :---------------- |
|   [1]   | `SkiaApplicationExtensions` | builder extension |
|   [2]   | `SkiaOptions`               | backend options   |
|   [3]   | `SkiaPlatform`              | platform boot     |

[LEASE_TYPES]: SkiaSharp API lease contracts
- rail: visuals

| [INDEX] | [SYMBOL]                             | [RAIL]             |
| :-----: | :----------------------------------- | :----------------- |
|   [1]   | `ISkiaSharpApiLeaseFeature`          | lease feature      |
|   [2]   | `ISkiaSharpApiLease`                 | canvas lease       |
|   [3]   | `ISkiaSharpPlatformGraphicsApiLease` | platform GPU lease |

[HELPER_TYPES]: conversion and render helpers
- rail: visuals

| [INDEX] | [SYMBOL]               | [RAIL]             |
| :-----: | :--------------------- | :----------------- |
|   [1]   | `SkiaSharpExtensions`  | type conversion    |
|   [2]   | `DrawingContextHelper` | visual-to-canvas   |
|   [3]   | `ImageSavingHelper`    | `SKImage` encoding |
|   [4]   | `PixelFormatHelper`    | format resolution  |

## [3]-[ENTRYPOINTS]

[BACKEND_ENTRYPOINTS]: backend boot and tuning
- rail: visuals

| [INDEX] | [SURFACE]                 | [SURFACE_ROOT]              | [RAIL]           |
| :-----: | :------------------------ | :-------------------------- | :--------------- |
|   [1]   | `UseSkia`                 | `SkiaApplicationExtensions` | backend select   |
|   [2]   | `MaxGpuResourceSizeBytes` | `SkiaOptions`               | GPU cache cap    |
|   [3]   | `UseOpacitySaveLayer`     | `SkiaOptions`               | opacity layering |
|   [4]   | `Initialize`              | `SkiaPlatform`              | manual boot      |
|   [5]   | `DefaultDpi`              | `SkiaPlatform`              | DPI anchor       |

[LEASE_ENTRYPOINTS]: raw SkiaSharp access through render interface leases
- rail: visuals

| [INDEX] | [SURFACE]                     | [SURFACE_ROOT]              | [RAIL]         |
| :-----: | :---------------------------- | :-------------------------- | :------------- |
|   [1]   | `Lease`                       | `ISkiaSharpApiLeaseFeature` | lease open     |
|   [2]   | `SkCanvas`                    | `ISkiaSharpApiLease`        | raw canvas     |
|   [3]   | `GrContext`                   | `ISkiaSharpApiLease`        | GPU context    |
|   [4]   | `SkSurface`                   | `ISkiaSharpApiLease`        | raw surface    |
|   [5]   | `CurrentOpacity`              | `ISkiaSharpApiLease`        | opacity state  |
|   [6]   | `TryLeasePlatformGraphicsApi` | `ISkiaSharpApiLease`        | platform lease |

[CONVERSION_ENTRYPOINTS]: Avalonia-to-SkiaSharp type bridges
- rail: visuals

| [INDEX] | [SURFACE]             | [SURFACE_ROOT]        | [RAIL]         |
| :-----: | :-------------------- | :-------------------- | :------------- |
|   [1]   | `ToSKPoint`           | `SkiaSharpExtensions` | point bridge   |
|   [2]   | `ToSKRect`            | `SkiaSharpExtensions` | rect bridge    |
|   [3]   | `ToSKMatrix`          | `SkiaSharpExtensions` | matrix bridge  |
|   [4]   | `ToSKColor`           | `SkiaSharpExtensions` | color bridge   |
|   [5]   | `ToSkColorType`       | `SkiaSharpExtensions` | pixel format   |
|   [6]   | `ToAvaloniaRect`      | `SkiaSharpExtensions` | reverse bridge |
|   [7]   | `ToSKSamplingOptions` | `SkiaSharpExtensions` | interpolation  |

[RENDER_ENTRYPOINTS]: visual rendering onto raw canvases
- rail: visuals

| [INDEX] | [SURFACE]             | [SURFACE_ROOT]         | [RAIL]           |
| :-----: | :-------------------- | :--------------------- | :--------------- |
|   [1]   | `RenderAsync`         | `DrawingContextHelper` | visual-to-canvas |
|   [2]   | `TryCreateDashEffect` | `DrawingContextHelper` | dash effect      |
|   [3]   | `SaveImage`           | `ImageSavingHelper`    | image encode     |

## [4]-[IMPLEMENTATION_LAW]

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

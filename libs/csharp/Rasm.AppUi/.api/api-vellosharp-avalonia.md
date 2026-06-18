# [RASM_APPUI_API_VELLOSHARP_AVALONIA]

`VelloSharp.Avalonia.Vello` wires the VelloSharp GPU-accelerated 2D renderer into the Avalonia platform: `VelloApplicationExtensions.UseVello` registers `VelloPlatformRenderInterface`, font management, text shaping, and the WGPU compositor through `AppBuilder`; `VelloPlatformOptions` controls FPS, clear color, antialiasing mode, and renderer options.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `VelloSharp.Avalonia.Vello`
- package: `VelloSharp.Avalonia.Vello`
- assembly: `VelloSharp.Avalonia.Vello`
- namespace: `VelloSharp.Avalonia.Vello`
- namespace: `VelloSharp.Avalonia.Vello.Rendering`
- namespace: `VelloSharp.Avalonia.Vello.Geometry`
- asset: runtime library
- rail: visuals

## [2]-[PUBLIC_TYPES]

[INTEGRATION_TYPES]: platform registration and options
- rail: visuals

| [INDEX] | [SYMBOL]                     | [KIND]             | [RAIL]                    |
| :-----: | :--------------------------- | :----------------- | :------------------------ |
|   [1]   | `VelloApplicationExtensions` | static extension   | `AppBuilder` registration |
|   [2]   | `VelloRenderer`              | static initializer | direct platform init      |
|   [3]   | `VelloPlatformOptions`       | options record     | renderer configuration    |

[GEOMETRY_TYPES]: geometry implementation adapters
- rail: visuals
- note: these adapt Avalonia geometry contracts to Vello; not composed directly by application code

| [INDEX] | [SYMBOL]                            | [KIND]               |
| :-----: | :---------------------------------- | :------------------- |
|   [1]   | `VelloGeometryImplBase`             | geometry base        |
|   [2]   | `VelloPathGeometryImpl`             | path geometry        |
|   [3]   | `VelloLineGeometryImpl`             | line geometry        |
|   [4]   | `VelloEllipseGeometryImpl`          | ellipse geometry     |
|   [5]   | `VelloRectangleGeometryImpl`        | rect geometry        |
|   [6]   | `VelloRoundedRectangleGeometryImpl` | rounded rect         |
|   [7]   | `VelloStreamGeometryImpl`           | stream geometry      |
|   [8]   | `VelloGeometryGroupImpl`            | geometry group       |
|   [9]   | `VelloCombinedGeometryImpl`         | combined geometry    |
|  [10]   | `VelloTransformedGeometryImpl`      | transformed geometry |

[RENDERING_TYPES]: internal render pipeline types — rail: visuals

| [INDEX] | [SYMBOL]                       | [KIND]            |
| :-----: | :----------------------------- | :---------------- |
|   [1]   | `VelloFontManager`             | font manager      |
|   [2]   | `VelloFontManagerImpl`         | font manager impl |
|   [3]   | `VelloTextShaper`              | text shaper       |
|   [4]   | `VelloGlyphRunImpl`            | glyph run         |
|   [5]   | `VelloGlyphTypeface`           | glyph typeface    |
|   [6]   | `VelloTextServices`            | text services     |
|   [7]   | `VelloDrawingContextImpl`      | drawing context   |
|   [8]   | `VelloBitmapImpl`              | bitmap impl       |
|   [9]   | `VelloRegionImpl`              | region impl       |
|  [10]   | `VelloOffscreenRenderTarget`   | offscreen target  |
|  [11]   | `VelloSwapchainRenderTarget`   | swapchain target  |
|  [12]   | `VelloPlatformRenderInterface` | render interface  |

## [3]-[ENTRYPOINTS]

[REGISTRATION_ENTRYPOINTS]: `AppBuilder` fluent setup
- rail: visuals

| [INDEX] | [SURFACE]                                     | [SURFACE_ROOT]               | [RAIL]                |
| :-----: | :-------------------------------------------- | :--------------------------- | :-------------------- |
|   [1]   | `UseVello(AppBuilder, VelloPlatformOptions?)` | `VelloApplicationExtensions` | platform registration |
|   [2]   | `Initialize(VelloPlatformOptions)`            | `VelloRenderer`              | direct init           |

[OPTIONS_ENTRYPOINTS]: `VelloPlatformOptions` configuration properties
- rail: visuals

| [INDEX] | [SURFACE]         | [TYPE]             | [DEFAULT]            | [RAIL]            |
| :-----: | :---------------- | :----------------- | :------------------- | :---------------- |
|   [1]   | `FramesPerSecond` | `int`              | `60`                 | render timer rate |
|   [2]   | `ClearColor`      | `RgbaColor`        | transparent black    | per-frame clear   |
|   [3]   | `Antialiasing`    | `AntialiasingMode` | default (area AA)    | AA mode           |
|   [4]   | `RendererOptions` | `RendererOptions`  | area AA on, MSAA off | renderer config   |
|   [5]   | `PresentMode`     | `PresentMode`      | platform default     | swapchain present |

## [4]-[IMPLEMENTATION_LAW]

[INTEGRATION_TOPOLOGY]:
- namespace: `VelloSharp.Avalonia.Vello` (integration root) + `Rendering` + `Geometry` sub-namespaces; 57 types total
- `UseVello` registers `VelloPlatformRenderInterface`, `VelloFontManagerImpl`, `VelloTextShaper`, and a `Compositor` into `AvaloniaLocator.CurrentMutable`
- geometry impl types implement Avalonia's `IGeometryImpl` family; platform wires them through `VelloPlatformRenderInterface`, not direct instantiation
- `VelloPlatformOptions` is the single configuration surface; pass an instance to `UseVello` or omit for defaults
- `VelloPlatform` (internal) owns singleton state; `VelloRenderer.Initialize` and `UseVello` are the two public entry paths; calling both or calling either twice is safe (guarded by `s_initialized`)

[LOCAL_ADMISSION]:
- Configure via `UseVello(options)` in `AppBuilder` setup; do not call `VelloRenderer.Initialize` separately when using `AppBuilder`.
- `AntialiasingMode` is resolved against actual `RendererOptions` capability flags; requested mode may fall back silently.
- This package is the Avalonia host binding only; `VelloSharp` owns the scene/renderer/path/brush primitives.

[RAIL_LAW]:
- Package: `VelloSharp.Avalonia.Vello`
- Owns: Avalonia platform registration, render interface, font/text services, geometry adapters, and swapchain/offscreen targets for the Vello GPU backend
- Accept: `AppBuilder.UseVello(options?)` as the sole registration path
- Reject: direct Avalonia locator binding of Vello rendering types; do not substitute SkiaSharp and Vello render paths in the same process

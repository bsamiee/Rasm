# [RASM_APPUI_API_VELLOSHARP]

`VelloSharp` supplies the Vello GPU-accelerated 2D rendering backend: `Scene` accumulates fill, stroke, layer, and image draw commands; `Renderer` rasterizes the scene into a `Span<byte>` pixel buffer; `VelloSurface` manages a GPU-backed surface target; `KurboPath` and `PenikoBrush` carry the vector path and brush primitives; `Font` and `VelloSvg` supply text and SVG input surfaces.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `VelloSharp`
- package: `VelloSharp`
- assembly: `VelloSharp`
- namespace: `VelloSharp`
- asset: runtime library
- rail: visuals

## [2]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: rendering pipeline owners
- rail: visuals

| [INDEX] | [SYMBOL]          | [TYPE_FAMILY]    | [RAIL]                 |
| :-----: | :---------------- | :--------------- | :--------------------- |
|   [1]   | `Renderer`        | rasterizer       | CPU-fallback render    |
|   [2]   | `VelatoRenderer`  | GPU renderer     | Velato GPU render path |
|   [3]   | `Scene`           | draw accumulator | draw command buffer    |
|   [4]   | `VelloSurface`    | GPU surface      | GPU-backed target      |
|   [5]   | `RendererOptions` | renderer config  | render options         |

[PUBLIC_TYPE_SCOPE]: path and brush primitives
- rail: visuals

| [INDEX] | [SYMBOL]           | [TYPE_FAMILY] | [RAIL]                 |
| :-----: | :----------------- | :------------ | :--------------------- |
|   [1]   | `KurboPath`        | vector path   | Kurbo path object      |
|   [2]   | `KurboAffine`      | affine value  | 2D affine transform    |
|   [3]   | `KurboRect`        | rect value    | axis-aligned rectangle |
|   [4]   | `KurboPoint`       | point value   | 2D point               |
|   [5]   | `KurboVec2`        | vector value  | 2D vector              |
|   [6]   | `KurboStrokeStyle` | stroke style  | stroke parameters      |
|   [7]   | `KurboStrokeCap`   | cap enum      | stroke end cap         |
|   [8]   | `KurboStrokeJoin`  | join enum     | stroke corner join     |
|   [9]   | `KurboPathElement` | path element  | path verb              |
|  [10]   | `KurboPathVerb`    | verb enum     | path command           |
|  [11]   | `PenikoBrush`      | brush object  | solid/gradient brush   |
|  [12]   | `BrushFactory`     | brush factory | brush creation         |
|  [13]   | `ImageBrush`       | image brush   | image-filled brush     |

[PUBLIC_TYPE_SCOPE]: image and text surfaces
- rail: visuals

| [INDEX] | [SYMBOL]          | [TYPE_FAMILY]  | [RAIL]              |
| :-----: | :---------------- | :------------- | :------------------ |
|   [1]   | `Image`           | image object   | pixel image upload  |
|   [2]   | `ImageInfo`       | image metadata | image dimensions    |
|   [3]   | `Font`            | font object    | font data handle    |
|   [4]   | `FontMetricsInfo` | font metrics   | metrics record      |
|   [5]   | `VelloSvg`        | SVG document   | SVG load and render |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: renderer and surface lifecycle
- rail: visuals

| [INDEX] | [SURFACE]                                      | [SURFACE_ROOT] | [RAIL]            |
| :-----: | :--------------------------------------------- | :------------- | :---------------- |
|   [1]   | `Renderer(uint, uint, options?)`               | `Renderer`     | renderer create   |
|   [2]   | `Resize(uint, uint)`                           | `Renderer`     | resize target     |
|   [3]   | `Render(Scene, RenderParams, Span<byte>, int)` | `Renderer`     | rasterize scene   |
|   [4]   | `Dispose()`                                    | `Renderer`     | renderer teardown |

[ENTRYPOINT_SCOPE]: scene draw command accumulation
- rail: visuals

| [INDEX] | [SURFACE]                                                     | [SURFACE_ROOT] | [RAIL]           |
| :-----: | :------------------------------------------------------------ | :------------- | :--------------- |
|   [1]   | `Scene()`                                                     | `Scene`        | scene create     |
|   [2]   | `Reset()`                                                     | `Scene`        | clear commands   |
|   [3]   | `FillPath(PathBuilder, FillRule, Matrix3x2, RgbaColor)`       | `Scene`        | solid fill path  |
|   [4]   | `FillPath(PathBuilder, FillRule, Matrix3x2, Brush, ...)`      | `Scene`        | brush fill path  |
|   [5]   | `FillPath(KurboPath, FillRule, Matrix3x2, Brush, ...)`        | `Scene`        | kurbo fill path  |
|   [6]   | `StrokePath(PathBuilder, StrokeStyle, Matrix3x2, RgbaColor)`  | `Scene`        | solid stroke     |
|   [7]   | `StrokePath(PathBuilder, StrokeStyle, Matrix3x2, Brush, ...)` | `Scene`        | brush stroke     |
|   [8]   | `StrokePath(KurboPath, StrokeStyle, Matrix3x2, Brush, ...)`   | `Scene`        | kurbo stroke     |
|   [9]   | `DrawImage(Image, Matrix3x2)`                                 | `Scene`        | image blit       |
|  [10]   | `DrawImage(ImageBrush, Matrix3x2)`                            | `Scene`        | brush image blit |
|  [11]   | `DrawBlurredRoundedRect(Vector2, Vector2, ...)`               | `Scene`        | blur rect        |
|  [12]   | `PushLayer(PathBuilder, LayerBlend, Matrix3x2, float)`        | `Scene`        | layer push       |
|  [13]   | `PushLuminanceMaskLayer(PathBuilder, Matrix3x2, float)`       | `Scene`        | luminance mask   |
|  [14]   | `PopLayer()`                                                  | `Scene`        | layer pop        |
|  [15]   | `Append(Scene)`                                               | `Scene`        | scene merge      |

[ENTRYPOINT_SCOPE]: image, font, and SVG loading
- rail: visuals

| [INDEX] | [SURFACE]                                                  | [SURFACE_ROOT] | [RAIL]          |
| :-----: | :--------------------------------------------------------- | :------------- | :-------------- |
|   [1]   | `Image.FromPixels(ReadOnlySpan<byte>, int, int, ...)`      | `Image`        | pixel upload    |
|   [2]   | `Image.GetInfo()`                                          | `Image`        | metadata query  |
|   [3]   | `Font.Load(ReadOnlySpan<byte>, uint)`                      | `Font`         | font data load  |
|   [4]   | `Font.TryGetGlyphIndex(uint, out ushort)`                  | `Font`         | glyph lookup    |
|   [5]   | `Font.TryGetGlyphMetrics(ushort, float, out GlyphMetrics)` | `Font`         | glyph metrics   |
|   [6]   | `Font.GetMetrics()`                                        | `Font`         | font metrics    |
|   [7]   | `VelloSvg.LoadFromFile(string, float)`                     | `VelloSvg`     | SVG file load   |
|   [8]   | `VelloSvg.LoadFromString(string, float)`                   | `VelloSvg`     | SVG string load |
|   [9]   | `VelloSvg.LoadFromUtf8(ReadOnlySpan<byte>, float)`         | `VelloSvg`     | UTF-8 SVG load  |
|  [10]   | `VelloSvg.Render(Scene, Matrix3x2?)`                       | `VelloSvg`     | SVG scene emit  |

## [4]-[IMPLEMENTATION_LAW]

[VELLO_TOPOLOGY]:
- namespace: `VelloSharp` only; 55 types across 1 namespace
- `Renderer` is native-backed (P/Invoke into `vello_renderer_create`); `VelloSurface` is the GPU-backed variant; both are disposable
- `Scene` accumulates draw commands into a native buffer; `Reset` clears the buffer for reuse
- `KurboPath` wraps native Kurbo Bézier paths through `KurboPathHandle`; path elements enumerate as `KurboPathElement` with `KurboPathVerb` discriminant
- `PenikoBrush` wraps the Peniko brush model: solid color, linear/radial/sweep gradient, and image brush kinds

[LOCAL_ADMISSION]:
- Every native-backed type (`Renderer`, `Scene`, `VelloSurface`, `Image`, `Font`, `KurboPath`, `PenikoBrush`, `VelloSvg`) implements `IDisposable`; all instances are lifecycle-scoped.
- GPU path is `VelatoRenderer` / `VelloSurface`; CPU fallback path is `Renderer` with `Span<byte>` output.
- SVG rendering via `VelloSvg.Render` emits into a `Scene`; the scene then rasterizes in a single `Renderer.Render` call.

[RAIL_LAW]:
- Package: `VelloSharp`
- Owns: Vello GPU-accelerated 2D render backend, scene accumulation, path/brush primitives, pixel image upload, font handle, and SVG scene emission
- Accept: all render commands accumulate into `Scene` before rasterization
- Reject: mixing SkiaSharp canvas calls into the Vello render pipeline; Vello and Skia are parallel render backends

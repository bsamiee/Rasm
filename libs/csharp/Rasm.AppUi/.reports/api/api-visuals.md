# [RASM_APPUI_API_VISUALS]

Visual APIs supply AppUi retained charts, gauges, SVG/path assets, Skia drawing, text shaping, native asset identity, thumbnails, and offscreen render surfaces.

## [1]-[SURFACES]

This table is a lookup by visual package.

| [INDEX] | [PACKAGE]                               | [ASSEMBLY]                               | [LOCAL_RAIL] |
| :-----: | :-------------------------------------- | :--------------------------------------- | :----------- |
|   [1]   | `LiveChartsCore.SkiaSharpView.Avalonia` | `LiveChartsCore.SkiaSharpView.Avalonia`  | chart        |
|   [2]   | `SkiaSharp`                             | `SkiaSharp`                              | drawing      |
|   [3]   | `SkiaSharp.HarfBuzz`                    | `SkiaSharp.HarfBuzz`                     | typography   |
|   [4]   | `SkiaSharp.NativeAssets.macOS`          | native asset package                     | native       |
|   [5]   | `HarfBuzzSharp.NativeAssets.macOS`      | native asset package                     | native       |
|   [6]   | `Svg.Controls.Skia.Avalonia`            | `Svg.Controls.Skia.Avalonia`             | assets       |

## [2]-[API_LOCATORS]

This table is a lookup by assembly.

| [INDEX] | [ASSEMBLY]                              | [NAMESPACE]                           | [USING]                               | [API_LOCATOR] |
| :-----: | :-------------------------------------- | :------------------------------------ | :------------------------------------ | :------------ |
|   [1]   | `LiveChartsCore`                        | `LiveChartsCore`                      | `LiveChartsCore`                      | `.cache/nuget/packages/livechartscore.skiasharpview.avalonia/` |
|   [2]   | `LiveChartsCore.SkiaSharpView.Avalonia` | `LiveChartsCore.SkiaSharpView.Avalonia` | `LiveChartsCore.SkiaSharpView.Avalonia` | `.cache/nuget/packages/livechartscore.skiasharpview.avalonia/` |
|   [3]   | `SkiaSharp`                             | `SkiaSharp`                           | `SkiaSharp`                           | `.cache/nuget/packages/skiasharp/` |
|   [4]   | `SkiaSharp.HarfBuzz`                    | `SkiaSharp.HarfBuzz`                  | `SkiaSharp.HarfBuzz`                  | `.cache/nuget/packages/skiasharp.harfbuzz/` |
|   [5]   | `Svg.Controls.Skia.Avalonia`            | `Svg.Controls.Skia.Avalonia`          | `Svg.Controls.Skia.Avalonia`          | `.cache/nuget/packages/svg.controls.skia.avalonia/` |

## [3]-[CAPABILITIES]

This table is a lookup by type family.

| [INDEX] | [TYPE_FAMILY]        | [ENTRY_SURFACE]              | [LOCAL_RAIL] |
| :-----: | :------------------- | :--------------------------- | :----------- |
|   [1]   | chart controls       | retained charts and gauges   | chart        |
|   [2]   | axis and series APIs | dashboard data visualization | chart        |
|   [3]   | `SKCanvas`           | offscreen and retained draw   | drawing      |
|   [4]   | `SKBitmap`           | thumbnails and captures       | drawing      |
|   [5]   | HarfBuzz shaper APIs | shaped text over Skia         | typography   |
|   [6]   | SVG control APIs     | vector asset rendering        | assets       |
|   [7]   | native asset files   | runtime identity evidence     | native       |

## [4]-[REJECTED]

This table is a lookup by rejected package.

| [INDEX] | [REJECT]                | [LOCAL_RAIL] | [REASON]              |
| :-----: | :---------------------- | :----------- | :-------------------- |
|   [1]   | `ScottPlot.Avalonia`    | chart        | second chart stack    |
|   [2]   | viewport Skia ownership | drawing      | host display conduit owns viewport |
|   [3]   | Material icon providers | assets       | path/SVG catalogue owns identity |

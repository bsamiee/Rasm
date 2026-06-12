# visuals — admission question

Question: is a direct `Svg.Skia` pin needed for the non-Avalonia surface, or does the admitted Avalonia-coupled package cover the doctrine?

Answer: a direct `Svg.Skia` pin is needed for any non-shell surface; the admitted Avalonia-coupled package covers only the UI-control profile.

Evidence:

- The admitted package ships exactly one assembly — the Avalonia control library. The SVG document engine (`Svg.Skia.SKSvg`, settings, scene graph, animation controller) lives in the separate `Svg.Skia` package, which arrives only as a transitive dependency.
- The admitted package's dependency rows are `Svg.Skia` plus `Avalonia.Skia` — referencing it from any project drags `Avalonia.Skia` and therefore the full Avalonia closure into that process. A headless document/export/evidence process cannot consume the SVG doctrine through this package without becoming an Avalonia process.
- `Svg.Skia`'s own dependency closure is Avalonia-free: `Svg.Animation`/`Svg.Custom`/`Svg.Model`/`Svg.SceneGraph` plus `HarfBuzzSharp` (with its native-asset rows) plus `SkiaSharp` — every member already admitted or already flowing in the visuals/typography closure. A direct pin adds no new third-party surface and no new native payload class.
- The headless doctrine is fully served by `SKSvg` alone: admission (`CreateFrom*`/`Load`/`ReLoad` with `SvgParameters`), retained `SKPicture` output for any render target row, `Save` rasterization, settings (color rows, typeface providers), retained scene/hit-test/animation — none of it requires the Avalonia control assembly.
- Transitive compile-asset flow does not rescue the non-shell case: a project that does not reference the Avalonia-coupled package has no path to `Svg.Skia` types at all, so the pin is the only route for document/export/evidence processes to compose the SVG pipeline.

Consequence for residency: visuals gains `Svg.Skia` as a direct domain-home row beside the existing Avalonia-coupled row; the Avalonia-coupled package remains the UI-shell projection of the same engine. No manifest edit performed here — this is the evidence record.

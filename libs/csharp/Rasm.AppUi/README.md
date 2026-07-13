# [APPUI]

`Rasm.AppUi` owns the APP-PLATFORM product UI — one Avalonia shell mounting onto any admitted substrate through the abstract `SurfaceHost` axis, folding AppHost ports, Persistence queries, and Compute receipts into settled product vocabulary. It references no host toolkit directly, so every Rhino and GH2 surface reaches the shell through a seam contract.

## [01]-[ROUTER]

[SHELL]:
- [01]-[NAVIGATION](.planning/Shell/navigation.md): Routing spine with a typed deep-link grammar over dockable layouts.
- [02]-[SCREENS](.planning/Shell/screens.md): Screen catalog with ref-counted activation and OAPH-paced state.
- [03]-[HOSTS](.planning/Shell/hosts.md): Host-neutral surface mounting through seam delegate columns.
- [04]-[COMMANDS](.planning/Shell/commands.md): Command vocabulary with availability algebra and total receipts.
- [05]-[CONTROLS](.planning/Shell/controls.md): `ControlIntent` union materialized through one control factory.
- [06]-[SOLVER](.planning/Shell/solver.md): Layout-constraint Kiwi algebra solved by one custom panel.
- [07]-[VIRTUALIZATION](.planning/Shell/virtualization.md): One virtual-window owner over change-sets and an extent ledger.
- [08]-[DIALOGS](.planning/Shell/dialogs.md): Typed-Fin dialog intents with dismissal-as-value over agnostic pickers.
- [09]-[INPUT](.planning/Shell/input.md): Command-derived hotkeys, behavior rows, and the pan-zoom device fabric.
- [10]-[ACCESSIBILITY](.planning/Shell/accessibility.md): Automation identity, tab-order and trap law, one WCAG luminance gate.

[RENDER]:
- [11]-[PIPELINE](.planning/Render/pipeline.md): Render-graph pass-DAG with per-backend targets and a resolve ladder.
- [12]-[MESHLETS](.planning/Render/meshlets.md): Compute residency-cluster consumption with hysteresis LOD and a cull cut.
- [13]-[PATHTRACE](.planning/Render/pathtrace.md): BVH, ReSTIR, and denoise oracle over the one light rig.
- [14]-[SHADING](.planning/Render/shading.md): Per-backend GPU shader cache feeding the layered-BSDF shade pass.
- [15]-[IMMERSIVE](.planning/Render/immersive.md): OpenXR stereo design-review and passthrough over the shared device.
- [16]-[REALITY](.planning/Render/reality.md): Gaussian-splat and point-cloud capture over the one residency carrier.
- [17]-[CAPTURE](.planning/Render/capture.md): Raster capsule, color-policy owner, and vector-print encode rows.
- [18]-[DRAFTING](.planning/Render/drafting.md): Sheet drafting consuming the hidden-line receipt with one DWG/DXF write leg.
- [19]-[ANIMATION](.planning/Render/animation.md): Timeline keyframe-track union with track-owned interpolation.

[CHARTS]:
- [20]-[DASHBOARDS](.planning/Charts/dashboards.md): Chart series and axis rows with downsampled stream binding and brushing.
- [21]-[CUSTOM](.planning/Charts/custom.md): Custom-visual Skia layout algebra with a keyed color-policy projection.
- [22]-[BASEMAP](.planning/Charts/basemap.md): Tiled basemap with Bim-owned overlays beside the viewport.

[EDITING]:
- [23]-[INSPECTOR](.planning/Editing/inspector.md): Typed property inspection with ranked editor rows and diff3 conflict hunks.
- [24]-[TABLES](.planning/Editing/tables.md): Tabular and hierarchical projection routed through the virtual window.
- [25]-[FORMS](.planning/Editing/forms.md): Form-schema wizard through the control factory, batch-edit folding one receipt.
- [26]-[HISTORY](.planning/Editing/history.md): Revertible-op inverse algebra over the recorder and a durable-ledger arm.
- [27]-[LIVEDATA](.planning/Editing/livedata.md): Reactive data spine over closed data-source cases and change-set operators.
- [28]-[GRAPH](.planning/Editing/graph.md): Node-editor parametric canvas with an admission gate and co-edit merge.

[DOCUMENT]:
- [29]-[NOTEBOOK](.planning/Document/notebook.md): Capability-pinned cells composing the recompute graph with co-edited replay.
- [30]-[MEDIA](.planning/Document/media.md): Markdown inlines and codec rows on the one surface seam.
- [31]-[EXPORT](.planning/Document/export.md): Paginated flow reports with PDF security, an Office arm, and a print arm.

[COLLAB]:
- [32]-[SYNC](.planning/Collab/sync.md): Live-merge authority and the typed edit-intent stream onto the durable ledger.
- [33]-[ISSUES](.planning/Collab/issues.md): openBIM issue board projection over the Bim BCF contract.
- [34]-[TOUR](.planning/Collab/tour.md): Review tour as a camera-track projection with presenter-follow presence.

[DIAGNOSTICS]:
- [35]-[EVIDENCE](.planning/Diagnostics/evidence.md): Evidence-receipt union, correlation join, and the 6xxx fault registry.
- [36]-[PROOF](.planning/Diagnostics/proof.md): Capture lanes, the headless proof matrix, goldens, and a typed proof fault.
- [37]-[DEVLOOP](.planning/Diagnostics/devloop.md): Hot-reload knobs, inspector HUD, flamegraph, solve scrub, and a REPL.
- [38]-[GOVERNOR](.planning/Diagnostics/governor.md): Perf-budget quality governor with timestamp attribution.

[THEME]:
- [39]-[TOKENS](.planning/Theme/tokens.md): Design-token engine with an OKLab ramp mix and atomic theme swap.
- [40]-[TYPOGRAPHY](.planning/Theme/typography.md): Type roles, embedded-font admission, and one live-front-matter shaping rail.
- [41]-[MOTION](.planning/Theme/motion.md): Motion tokens with spring algebra and a progress-to-token map.
- [42]-[ASSETS](.planning/Theme/assets.md): Nameof-derived asset-key vocabulary with rank-fallback sourcing.
- [43]-[LOCALE](.planning/Theme/locale.md): Locale rows over Resx, ICU, and time with a typed locale fault and live captioning.

## [02]-[DOMAIN_PACKAGES]

Product-UI libraries — UI toolkit, GPU rendering, media, drafting exchange, and collaboration — admitted by this folder; versions centralize in the C# manifest.

[UI_FRAMEWORK]:
- `Avalonia`
- `Avalonia.Desktop`
- `Avalonia.Headless`
- `Avalonia.Headless.XUnit` — the AppUi test-shell csproj carries this per-test headless session reference.
- `Avalonia.Themes.Fluent`
- `Avalonia.Fonts.Inter`
- `ReactiveUI`
- `ReactiveUI.Avalonia`
- `ReactiveUI.Validation`
- `Xaml.Behaviors.Avalonia`
- `System.Reactive`
- `DynamicData`
- `Dock.Avalonia`
- `Dock.Model.ReactiveUI`
- `Dock.Serializer.SystemTextJson`
- `DialogHost.Avalonia`
- `Kiwi`

[CONTROLS_THEME]:
- `Avalonia.Controls.DataGrid`
- `Avalonia.Controls.ColorPicker`
- `Avalonia.AvaloniaEdit`
- `AvaloniaEdit.TextMate`
- `bodong.Avalonia.PropertyGrid`
- `bodong.PropertyModels`
- `LiveChartsCore.SkiaSharpView.Avalonia`
- `AsyncImageLoader.Avalonia`
- `FluentIcons.Avalonia`
- `FluentIcons.Common`
- `Markdig`
- `PanAndZoom`
- `Semi.Avalonia`
- `Semi.Avalonia.DataGrid`
- `Semi.Avalonia.ColorPicker`
- `Semi.Avalonia.Dock`
- `Semi.Avalonia.AvaloniaEdit`
- `Irihi.Ursa`
- `Irihi.Ursa.Themes.Semi`
- `Irihi.Ursa.ReactiveUIExtension`

[RENDER_GPU]:
- `Avalonia.Skia`
- `SkiaSharp`
- `SkiaSharp.HarfBuzz`
- `SkiaSharp.NativeAssets.macOS`
- `SkiaSharp.NativeAssets.Linux` — transitive distribution-closure floor, central pin only.
- `SkiaSharp.NativeAssets.Linux.NoDependencies` — glibc-only Linux natives for the headless proof lane.
- `HarfBuzzSharp.NativeAssets.macOS`
- `HarfBuzzSharp.NativeAssets.Linux` — transitive distribution-closure floor, central pin only.
- `Svg.Controls.Skia.Avalonia`
- `Svg.Skia`
- `Silk.NET.WebGPU`
- `Silk.NET.WebGPU.Native.WGPU`
- `Silk.NET.WebGPU.Extensions.WGPU`
- `Silk.NET.OpenXR`
- `Silk.NET.OpenXR.Extensions.KHR`
- `Silk.NET.OpenXR.Extensions.EXT`
- `Silk.NET.OpenXR.Extensions.FB`

[MEDIA_INPUT]:
- `FFmpeg.AutoGen`
- `HanumanInstitute.LibMpv`
- `HanumanInstitute.LibMpv.Avalonia`
- `HidSharp`
- `Silk.NET.Input`
- `Silk.NET.SDL`
- `Melanchall.DryWetMidi`

[EXCHANGE_COLLAB]:
- `ACadSharp`
- `DocumentFormat.OpenXml`
- `lcmsNET`
- `PDFsharp`
- `PDFsharp-MigraDoc`
- `NodeEditorAvalonia`
- `Mapsui.Avalonia12`
- `LoroCs`
- `MessageFormat`
- `Whisper.net`

[DEV_LOOP]:
- `ProDiagnostics` — Debug-only tree, property, style, event, and layout inspection.
- `HotAvalonia` — Debug-only XAML reload and runtime inflation.
- `Avalonia.Markup.Xaml.Loader` — Debug-only markup loader inside the HotAvalonia closure.

## [03]-[SUBSTRATE_PACKAGES]

Substrate libraries from the C# registry that this folder consumes directly. Full registry and substrate contracts live in [`libs/csharp/.planning/README.md`](../.planning/README.md), with shared API evidence in `libs/csharp/.api/`.

[FUNCTIONAL_CORE]:
- `LanguageExt.Core`
- `Thinktecture.Runtime.Extensions`
- `Thinktecture.Runtime.Extensions.Json`
- `JetBrains.Annotations`

[TIME_IDENTITY]:
- `NodaTime`
- `System.IO.Hashing`

[NUMERIC_SUBSTRATE]:
- `UnitsNet`

[GRAPH_ALGORITHM]:
- `QuikGraph` — the parametric graph-canvas topology walks.

[COLOR_SCIENCE]:
- `Wacton.Unicolour` — perceptual color math for theme tokens and contrast proofs.

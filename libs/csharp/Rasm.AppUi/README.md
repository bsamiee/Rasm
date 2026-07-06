# [APPUI]

`Rasm.AppUi` is the APP-PLATFORM product-UI engine: one Avalonia shell that mounts onto any admitted substrate — Rhino panel and modal, GH2 companion window, standalone desktop, sidecar shell, headless proof surface — through one abstract `SurfaceHost` axis. It consumes AppHost ports, Persistence queries, and Compute receipts as settled vocabulary and never references Rhino or GH directly. The domain folder-map lives in `ARCHITECTURE.md`, the forward concept pool in `IDEAS.md`, and the open work in `TASKLOG.md`.

## [01]-[ROUTER]

- [01]-[NAVIGATION](.planning/Shell/navigation.md)
- [02]-[SCREENS](.planning/Shell/screens.md)
- [03]-[HOSTS](.planning/Shell/hosts.md)
- [04]-[COMMANDS](.planning/Shell/commands.md)
- [05]-[CONTROLS](.planning/Shell/controls.md)
- [06]-[SOLVER](.planning/Shell/solver.md)
- [07]-[VIRTUALIZATION](.planning/Shell/virtualization.md)
- [08]-[DIALOGS](.planning/Shell/dialogs.md)
- [09]-[INPUT](.planning/Shell/input.md)
- [10]-[ACCESSIBILITY](.planning/Shell/accessibility.md)
- [11]-[PIPELINE](.planning/Render/pipeline.md)
- [12]-[MESHLETS](.planning/Render/meshlets.md)
- [13]-[PATHTRACE](.planning/Render/pathtrace.md)
- [14]-[SHADING](.planning/Render/shading.md)
- [15]-[IMMERSIVE](.planning/Render/immersive.md)
- [16]-[REALITY](.planning/Render/reality.md)
- [17]-[CAPTURE](.planning/Render/capture.md)
- [18]-[DRAFTING](.planning/Render/drafting.md)
- [19]-[ANIMATION](.planning/Render/animation.md)
- [20]-[DASHBOARDS](.planning/Charts/dashboards.md)
- [21]-[CUSTOM](.planning/Charts/custom.md)
- [22]-[BASEMAP](.planning/Charts/basemap.md)
- [23]-[INSPECTOR](.planning/Editing/inspector.md)
- [24]-[TABLES](.planning/Editing/tables.md)
- [25]-[FORMS](.planning/Editing/forms.md)
- [26]-[HISTORY](.planning/Editing/history.md)
- [27]-[LIVEDATA](.planning/Editing/livedata.md)
- [28]-[GRAPH](.planning/Editing/graph.md)
- [29]-[NOTEBOOK](.planning/Document/notebook.md)
- [30]-[MEDIA](.planning/Document/media.md)
- [31]-[EXPORT](.planning/Document/export.md)
- [32]-[SYNC](.planning/Collab/sync.md)
- [33]-[ISSUES](.planning/Collab/issues.md)
- [34]-[TOUR](.planning/Collab/tour.md)
- [35]-[EVIDENCE](.planning/Diagnostics/evidence.md)
- [36]-[PROOF](.planning/Diagnostics/proof.md)
- [37]-[DEVLOOP](.planning/Diagnostics/devloop.md)
- [38]-[GOVERNOR](.planning/Diagnostics/governor.md)
- [39]-[TOKENS](.planning/Theme/tokens.md)
- [40]-[TYPOGRAPHY](.planning/Theme/typography.md)
- [41]-[MOTION](.planning/Theme/motion.md)
- [42]-[ASSETS](.planning/Theme/assets.md)
- [43]-[LOCALE](.planning/Theme/locale.md)

## [02]-[DOMAIN_PACKAGES]

Domain libraries that implement UI framework, rendering, GPU backends, charts, content, and CAD exchange — entirely distinct from the C# substrate registry.

[UI_FRAMEWORK]:
- `Avalonia`
- `Avalonia.Desktop`
- `Avalonia.Headless`
- `Avalonia.Themes.Fluent`
- `Avalonia.Fonts.Inter`
- `Avalonia.Controls.DataGrid`
- `Avalonia.Controls.ColorPicker`
- `Avalonia.AvaloniaEdit`
- `AvaloniaEdit.TextMate`

[REACTIVE]:
- `ReactiveUI`
- `ReactiveUI.Avalonia`
- `ReactiveUI.Validation`
- `Xaml.Behaviors.Avalonia`
- `System.Reactive`
- `DynamicData`

[DOCKING_DIALOGS]:
- `Dock.Avalonia`
- `Dock.Model.ReactiveUI`
- `Dock.Serializer.SystemTextJson`
- `DialogHost.Avalonia`

[INSPECTION_CHARTING]:
- `bodong.Avalonia.PropertyGrid`
- `bodong.PropertyModels`
- `LiveChartsCore.SkiaSharpView.Avalonia`

[RENDER_TEXT]:
- `Avalonia.Skia`
- `SkiaSharp`
- `SkiaSharp.HarfBuzz`
- `SkiaSharp.NativeAssets.macOS`
- `SkiaSharp.NativeAssets.Linux`
- `SkiaSharp.NativeAssets.Linux.NoDependencies`
- `HarfBuzzSharp.NativeAssets.macOS`
- `HarfBuzzSharp.NativeAssets.Linux`
- `Svg.Controls.Skia.Avalonia`
- `Svg.Skia`

[GPU_BACKEND]:
- `Silk.NET.WebGPU`
- `Silk.NET.WebGPU.Native.WGPU`
- `Silk.NET.WebGPU.Extensions.WGPU`
- `Silk.NET.OpenXR`
- `Silk.NET.OpenXR.Extensions.KHR`
- `Silk.NET.OpenXR.Extensions.EXT`
- `Silk.NET.OpenXR.Extensions.FB`

> [!NOTE]
> The `Wgpu` GPU family is owned by the .NET Foundation `Silk.NET.WebGPU` binding (stable, MIT) over the bundled `Silk.NET.WebGPU.Native.WGPU` wgpu/Dawn runtime, presenting into the Avalonia 12 compositor through `ICompositionGpuInterop` texture import (`.api/api-avalonia-gpu-interop.md`). `Silk.NET.WebGPU.Extensions.WGPU` adds the wgpu-native vendor surface the standard `webgpu.h` binding omits — non-blocking `DevicePoll`, native log routing, full-adapter enumeration for the compositor-LUID match, and indirect multi-draw for GPU-driven meshlet rendering (`.api/api-silk-webgpu-wgpu.md`). `Silk.NET.OpenXR` owns the immersive design-review surface — the stereo swapchain, the predicted-display-time frame loop, and the action-set controller model share the one `Wgpu` device through the OpenXR graphics binding, folding to the flat viewport where the host OpenXR loader is absent (`.api/api-silk-openxr.md`); `Silk.NET.OpenXR.Extensions.FB` adds the `XR_FB_passthrough` environment-blend layer the on-site mixed-reality review composites under the rendered scene against the one session (`.api/api-silk-openxr-fb.md`), riding the same `2.23.0` line as the core families since Silk.NET publishes its whole core-plus-extension set from one monorepo release. `Avalonia.Skia` (Ganesh) is the shippable `Software`/`Metal`/`Vulkan`/`OpenGl` floor behind the `SurfaceHost` render seam. The archived `VelloSharp`/`VelloSharp.Avalonia.Vello` rows are fully retired — no Vello identity survives in the manifest, this registry, or `.api/`; SkiaSharp Graphite carries no pinnable identity yet (targeted `4.150.0-preview.2`, unshipped), so no Graphite row is admitted until it ships.

[ASSETS_CONTENT]:
- `AsyncImageLoader.Avalonia`
- `FluentIcons.Avalonia`
- `FluentIcons.Common`
- `Markdig`
- `PanAndZoom`
- `Wacton.Unicolour`

[LAYOUT]:
- `Kiwi`

[MEDIA]:
- `FFmpeg.AutoGen`
- `HanumanInstitute.LibMpv`
- `HanumanInstitute.LibMpv.Avalonia`

[INPUT_FABRIC]:
- `HidSharp`
- `Silk.NET.Input`
- `Silk.NET.SDL`
- `Melanchall.DryWetMidi`

[DRAFTING_EXPORT]:
- `ACadSharp`
- `DocumentFormat.OpenXml`
- `lcmsNET`
- `PDFsharp`
- `PDFsharp-MigraDoc`

[THEME_SUITE]:
- `Semi.Avalonia`
- `Semi.Avalonia.DataGrid`
- `Semi.Avalonia.ColorPicker`
- `Semi.Avalonia.Dock`
- `Semi.Avalonia.AvaloniaEdit`

[EXTENDED_CONTROLS]:
- `Irihi.Ursa`
- `Irihi.Ursa.Themes.Semi`
- `Irihi.Ursa.ReactiveUIExtension`

[GRAPH_MAP_SURFACES]:
- `NodeEditorAvalonia`
- `Mapsui.Avalonia12`
- `QuikGraph`

[COLLABORATION_LOCALE]:
- `LoroCs`
- `MessageFormat`
- `Whisper.net`

[DEV_LOOP]:
- `ProDiagnostics`
- `HotAvalonia`
- `Avalonia.Markup.Xaml.Loader`

> [!NOTE]
> The Dev Loop family binds `Debug`-only with `PrivateAssets="all"` — none flows transitively to a downstream consumer. `ProDiagnostics` (MIT, wieslawsoltes) is the maintained Avalonia-12 fork of `Avalonia.Diagnostics` (the first-party `Avalonia.Diagnostics` line is feed-dead at 11.3.x with no Avalonia-12 asset): `DevToolsExtensions.AttachDevTools` mounts the runtime visual/logical-tree, live property/style, routed-event, and layout/renderer-overlay inspector, Debug-only behind the `HotAvalonia` closure (`.api/api-prodiagnostics.md`). `HotAvalonia` is the XAML hot-reload agent and `Avalonia.Markup.Xaml.Loader` its runtime-inflation substrate, both dev-loop-scoped.

> [!NOTE]
> The `Semi.Avalonia` design-token theme is the active layer over the retained `Avalonia.Themes.Fluent` floor — its base `SemiTheme` plus the `DataGrid`/`ColorPicker`/`Dock`/`AvaloniaEdit` skins restyle the admitted control roster to one token system the `Wacton.Unicolour` OKLCH pipeline materializes into the `ControlIntent` + `Theme/tokens` vocabulary, never displacing the Fluent-templated `bodong.PropertyGrid`/`DialogHost`. `Irihi.Ursa` adds the extended-control families the curated set lacks — `NavMenu`, `Timeline`, `Toast`/`Notification`, `Loading`/`Skeleton`, `Banner`, `Form`, `Drawer` — themed by `Irihi.Ursa.Themes.Semi` and bridged to the admitted ReactiveUI MVVM rail through `Irihi.Ursa.ReactiveUIExtension`. `NodeEditorAvalonia` owns the node/pin/connector graph-editing canvas inside its OWN `NodeZoomBorder` viewport (a distinct asset — the separately admitted `PanAndZoom` keeps its five page consumers, no dup) for the `Shell/Editing` parametric and dependency-graph surfaces (ReactiveUI view-models over the framework-agnostic `INode`/`IConnector` model), with `QuikGraph` owning the connection-admission cycle gate and graph algebra behind the canvas. `Mapsui.Avalonia12` is the interactive slippy-map / basemap viewport rendering through the admitted `SkiaSharp` + `Avalonia.Skia` and binding the Bim-owned `NetTopologySuite` so GDAL/OGR features draw as overlays beside the `Wgpu` 3D viewport. `LoroCs` is the Eg-walker/Fugue sequence+map+text+movable-list+tree CRDT engine backing the notebook/annotation/table collaboration op-log and presence (`runtimes/osx-arm64/native/loro.dylib`), retiring the bespoke `NotebookCrdt` LWW algebra; `MessageFormat` is the managed ICU MessageFormat engine (CLDR cardinal+ordinal `plural`/`selectordinal`/`select`) materializing `ResolvedLocale.Plural` over the resx pattern vocabulary. `PDFsharp` + `PDFsharp-MigraDoc` add the structured vector-PDF page model and the auto-paginated flow-report DOM the OOXML/DXF/raster export set lacked.

> [!NOTE]
> The Media / Drafting Export / Collaboration And Locale families gain three native-backed engine owners, each provisioned at the app-host distribution layer, never bundled. `FFmpeg.AutoGen` (MIT, Ruslan-B) is the in-process video-encode owner — the CppSharp-generated FFmpeg 8.x binding whose `ffmpeg` hub muxes the compositor/path-trace RGBA stream into an MP4/H.264 flythrough (`sws_scale` RGBA→YUV420P, the `avcodec_send_frame`/`avcodec_receive_packet` loop, libavformat write), the encode peer to the `HanumanInstitute.LibMpv` decode/playback owner; ship an LGPL-configured dynamically-linked FFmpeg build pointed at through `ffmpeg.RootPath` (`.api/api-ffmpeg-autogen.md`). `lcmsNET` (MIT) is the ICC / device-CMYK print-fidelity owner — the Little CMS 2 binding whose one polymorphic `Transform.Create` fold and K-preservation `Intent` vocabulary color-manage the export raster to device CMYK beside `PDFsharp`'s vector page, leaving the screen-perceptual `Wacton.Unicolour` OKLCH pipeline as the UI-token authority; the native `lcms2` library binds through P/Invoke (`.api/api-lcmsnet.md`). `Whisper.net` (MIT, sandrohanea) is the offline speech-to-text owner for LiveCaption — the `whisper.cpp` binding whose one `WhisperProcessorBuilder` `With*` fold and streaming `ProcessAsync` emit translated caption segments (built-in translate-to-English, Silero VAD); the native runtime ships as a separate `Whisper.net.Runtime*` package (CoreML on Apple silicon) and the ggml weights download through `WhisperGgmlDownloader` (`.api/api-whisper-net.md`).

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

[TEST_SUBSTRATE]:
- `Verify.XunitV3`

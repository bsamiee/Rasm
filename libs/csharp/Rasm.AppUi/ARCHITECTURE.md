# [RASM_APPUI_ARCHITECTURE]

The domain map of `Rasm.AppUi` — the APP-PLATFORM Avalonia product UI engine. The `Shell`, `Render`, `Charts`, `Editing`, `Document`, `Collab`, `Diagnostics`, and `Theme` domain folders, each page a UI capability unit over the settled receipt spine and the GPU render surface — 8 folders, 43 pages.

Each codemap node is the eventual source file its `.planning/` design page becomes, named in the language's own folder and file casing — PascalCase `.cs`, lowercase `.py`, lowercase `.ts`. Treat every node as realized code; the `.planning/` scaffold is the authoring substrate, never part of the map.

## [01]-[DOMAIN_MAP]

```text codemap
Rasm.AppUi/
├── Shell/                # Host-mount axis and application shell spine
│   ├── Navigation.cs     # Routing spine with typed NavFault deep-link grammar over Dock-driven dockable layouts
│   ├── Screens.cs        # Screen catalog, ref-counted activation, OAPH-paced state, control-intent stream body
│   ├── Hosts.cs          # Host-neutral SurfaceHost mounting with seam delegate columns
│   ├── Commands.cs       # CommandIntent vocabulary with availability algebra and total receipts
│   ├── Controls.cs       # ControlIntent [Union] materialized through ControlFactory over BehaviorRail.Intent
│   ├── Solver.cs         # LayoutConstraint Kiwi algebra solved by one LayoutSolver custom Avalonia panel
│   ├── Virtualization.cs # VirtualWindow owner over DynamicData change-sets; Fenwick extent ledger
│   ├── Dialogs.cs        # Typed-Fin dialog intents with dismissal-as-value and host-agnostic pickers
│   ├── Input.cs          # Command-derived hotkeys, behavior rows, pan-zoom canvas, InputFabric device drivers
│   └── Accessibility.cs  # Automation identity, tab-order/trap law, one Unicolour WCAG luminance gate
├── Render/               # Pure GPU-viewport + temporal tier
│   ├── Pipeline.cs       # RenderGraph pass-DAG, per-backend RenderTargetFactory, resolve ladder, SimVisual, Viewpoint
│   ├── Meshlets.cs       # Compute ResidencyPayload cluster consumption: hysteresis LOD cut, cone/HZB cull ladder, bindless residency
│   ├── PathTrace.cs      # Recursive SAH BVH/ReSTIR/denoise oracle over the one LightSource rig, shading FROM Materials LayeredBsdf
│   ├── Shading.cs        # GPU ShaderAsset cache per GpuBackend feeding the LayeredBsdf SurfaceShade pass off the same rig
│   ├── Immersive.cs      # OpenXR stereo design-review plus FB passthrough over the shared Wgpu device
│   ├── Reality.cs        # Gaussian-splat/point-cloud reality capture over the one Compute ResidencyPayload carrier
│   ├── Capture.cs        # Raster capsule + VisualCodec/ColorPolicy owner + vector-print arm + FFmpeg encode rows
│   ├── Drafting.cs       # Sheet drafting with per-region bases, hidden-line consumption, one-CadDocument DWG/DXF write leg
│   └── Animation.cs      # Timeline keyframe-track union, track-owned interpolation rows (one pose-slerp site), walkthrough fold
├── Charts/               # Chart, dashboard, and geo-basemap projection
│   ├── Dashboards.cs     # LiveCharts series/axis rows, LTTB-downsampled stream binding, cross-filter brushing
│   ├── Custom.cs         # CustomVisual Skia layout-algebra rail; ColorSpaceAxis as a keyed ColorPolicy projection
│   └── Basemap.cs        # Mapsui MapControl tiled basemap with Bim-owned NTS overlays beside the Wgpu viewport
├── Editing/              # Typed-edit surfaces over model
│   ├── Inspector.cs      # Typed PropertyGrid inspection with ranked editor-factory rows and real diff3 conflict hunks
│   ├── Tables.cs         # Tabular/hierarchical projection with column-metadata family routed through VirtualWindow
│   ├── Forms.cs          # FormSchema+wizard through ControlFactory, selection batch-edit folding one CommandReceipt
│   ├── History.cs        # RevertibleOp inverse algebra over CancelableCommandRecorder + Version/ledger durable arm
│   ├── LiveData.cs       # Reactive data spine over closed DataSource cases and DynamicData operators
│   └── Graph.cs          # NodeEditor parametric graph canvas: QuikGraph admission gate, echo-suppressed LoroTree co-edit
├── Document/             # Reproducible document plane
│   ├── Notebook.cs       # CapabilityPin cells composing the AppHost RecomputeGraph; CollabDoc co-editing; replay bundle
│   ├── Media.cs          # Markdig inlines + MediaSurface [Union] codec rows on the one SurfaceSeam (LibMpv, wired mount)
│   └── Export.cs         # MigraDoc flow reports, PDFsharp security/signatures/AcroForms/UA, OOXML arm, lcmsNET print arm
├── Collab/               # Live-collaboration plane over the Persistence-owned durable spine
│   ├── Sync.cs           # LoroDoc live merge authority; typed EditIntent stream onto Version/ledger; session-epoch law
│   ├── Issues.cs         # openBIM issue board PROJECTION over Rasm.Bim BCF contract; comments as CollabDoc map container
│   └── Tour.cs           # Review tour as a camera-Track projection onto the animation engine; presenter-follow presence arm
├── Diagnostics/          # Evidence, proof, dev loop, quality governance
│   ├── Evidence.cs       # One EvidenceReceipt union + correlation join + the AppUi 6xxx [FAULT_TABLES] registry
│   ├── Proof.cs          # Capture lanes, catalog-derived headless matrix, ProofLaw goldens, typed ProofFault
│   ├── DevLoop.cs        # Hot-reload knobs, ProDiagnostics inspector, HUD, flamegraph, solve scrub, REPL, remote ingest
│   └── Governor.cs       # PerfBudget quality governor + GpuTimeline timestamp/statistics attribution
└── Theme/                # Pure vocabulary tier: tokens, typography, motion, assets, locale
    ├── Tokens.cs         # Design-token engine with OKLab ramp mix and atomic theme swap
    ├── Typography.cs     # Type roles, embedded-Inter admission, one HarfBuzz shaping rail, live front-matter arms
    ├── Motion.cs         # Motion tokens with spring algebra and ProgressPhase-to-MotionToken map
    ├── Assets.cs         # Nameof-derived AssetKey vocabulary with rank-fallback icon sourcing
    └── Locale.cs         # Resx/ICU/NodaTime locale rows, typed LocaleFault, Whisper.net LiveCaption
```

`Shell` owns the host-mount axis and the application shell: the host-mount axis precedes the shell, the shell precedes the screens it routes, and the command, control, layout, virtualization, dialog, input, and accessibility pages ride the same spine — `controls` materializes the one `ControlIntent` family through `ControlFactory`, `solver` solves the `LayoutConstraint` algebra in one `LayoutSolver` panel, and `virtualization` owns the one `VirtualWindow` every windowed surface consumes. `Theme` is the pure vocabulary tier every literal traces to — the animation runtime engine lives in `Render`. `Render` is the GPU-viewport and temporal tier — `pipeline` drives the frame pass-DAG and draws the `meshlets` Compute-consumed cluster cut and the `pathtrace` oracle over the one `LightSource` rig, `capture` owns the raster capsule, the `ColorPolicy` gamut singleton, the vector-print arm, and the FFmpeg encode rows, `drafting` consumes the Fabrication hidden-line receipt and writes DWG/DXF through one `CadDocument` fold, and `animation` owns the timeline engine the tour projects onto. `Charts` projects the chart, dashboard, and Mapsui basemap planes; `Editing` is the typed-edit tier with the NodeEditor graph canvas. `Document` is the reproducible document plane — the notebook composes the AppHost `RecomputeGraph`, media mounts codec rows on the one `SurfaceSeam`, and `export` owns every paginated Office/PDF/print output. `Collab` is the live-collaboration plane: `sync` holds the one `CollabDoc` `LoroDoc` LIVE merge authority every co-edited surface composes AND the single typed `EditIntent` union that is the DURABLE truth on the Persistence `Version/ledger` — no Loro byte crosses durable truth, the snapshot blob survives only as a content-keyed derivable accelerator. `Diagnostics` carries the evidence union with the `[FAULT_TABLES]` 6xxx registry, the headless proof matrix, the dev loop, and the quality governor.

## [02]-[SEAMS]

```text seams
Shell/commands        →  typescript:core/interchange/codec          # [WIRE]: CommandPayloadWire + CommandGateWire palette gate
Shell/controls        →  typescript:core/interchange/codec          # [WIRE]: ControlIntentWire kind-discriminated control vocabulary
Shell/controls        →  typescript:ui/viewer                       # [WIRE]: ControlIntent closed union materialized at the panel plane
Shell/solver          →  typescript:core/interchange/codec          # [WIRE]: LayoutConstraintWire ordered Kiwi constraint program
Shell/solver          →  typescript:ui/viewer                       # [WIRE]: ordered LayoutProgram re-solved at the panel plane
Render/capture        →  typescript:core/interchange/codec          # [WIRE]: RenderReceiptWire frame-hash proof
Render/capture        →  typescript:ui/viewer # [RECEIPT]: RenderReceipt claims paired with local render evidence at probe plane
Render/pipeline       →  typescript:core/interchange/codec # [WIRE]: GeometryResidencyWire carries ResidencyManifest content-key residency
Diagnostics/evidence  →  typescript:core/state/feed                 # [WIRE]: EvidenceFeed / EvidenceTimeline
Render/meshlets       ←  csharp:Rasm.Compute/Runtime/payload # [SHAPE]: ResidencyPayload meshlet-cluster rows
Render/reality        ←  csharp:Rasm.Compute/Runtime/payload # [SHAPE]: the ONE ResidencyPayload carrier gaussian-splat/point-cloud kinds
Render/pipeline       ←  csharp:Rasm.Compute/Runtime                # [PROJECTION]: ResidencyManifest.Mint web geometry residency
Render/pathtrace      ←  csharp:Rasm.Compute/Analysis/daylight # [PORT]: SolarPosition.At(SolarSite, Instant) feeds LightSource.Sun
Render/shading        ⇄  csharp:Rasm.Compute                        # [SHAPE]: shared ONE_WGPU_DEVICE (Silk.NET.WebGPU)
Render                ←  csharp:Rasm.Fabrication/Documentation/projection # [RECEIPT]: HiddenLineResult Viewport2D edge sets
Render/pathtrace      ←  csharp:Rasm.Materials/Appearance           # [BOUNDARY]: LayeredBsdf / SlabStack / SurfaceShade at PATH_TRACE seam
Charts/basemap        ←  csharp:Rasm.Bim/Semantics/geospatial # [SHAPE]: Bim-owned NTS features carrying GeoReference GeoFeature.Reproject geodesy
Collab/sync           →  csharp:Rasm.Persistence/Version/ledger # [PROJECTION]: typed EditIntent ops onto Persistence-owned OpLogEntry/SyncOpKind rows
Collab/sync           ←  csharp:Rasm.Persistence/Version/ledger # [PORT]: per-document replay-window read decoded into fresh LoroDoc cold-start epoch
Collab/sync           →  csharp:Rasm.Persistence/Store # [CONTENT_KEY]: snapshot ACCELERATOR blob
Collab/sync           →  csharp:Rasm.AppHost/Wire/topics # [WIRE]: live-delta broadcast + presence topics
Editing/history       →  csharp:Rasm.Persistence/Version/ledger # [PROJECTION]: RevertibleOp inverse deltas feed Collab/sync EditIntent
Editing/graph         ←  csharp:Rasm.AppHost/Runtime/determinism # [PORT]: RecomputeGraph read projection for dependency visualization decode-only
Document/notebook     ←  csharp:Rasm.AppHost/Runtime # [PORT]: DeterminismContext / CapabilityPin environment identity + RecomputeGraph per-cell
Document/export       ←  csharp:Rasm.AppHost/Runtime/secrets # [PORT]: signing-credential lease ingress for IDigitalSigner material
Collab/issues         ←  csharp:Rasm.Bim/coordination               # [PORT]: BCF issue-board domain
Collab                ←  csharp:Rasm.Bim/coordination               # [SHAPE]: BcfTopic/BcfComment/BcfViewpoint annotation domain
AppUi/*               →  csharp:Rasm.AppHost/Runtime # [PORT]: the port-spine row family
AppUi/*               →  csharp:Rasm/Domain/identity # [CONTENT_KEY]: every AppUi content-identity mint composes kernel ContentHash.Of capture runtime
AppUi/faults          ⇄  csharp:Rasm.AppHost/Runtime/lifecycle # [FAULT]: AppUi 6xxx FaultBand neighborhood
```

Seam rows carry only live owners. AppUi reaches hidden-line geometry through the Fabrication `HiddenLineResult` receipt, texture UV through the Fabrication `ChartAtlas` carrier, collaboration through Persistence `Version/ledger`, and schedule dashboards through Bim planning receipts.

## [03]-[BOUNDARIES]

- `ChartAtlas` texture UV enters through the Fabrication nesting receipt.
- Bim `ElementSet` queries enter through Bim-owned receipt rows.
- `ScheduleNetwork` dashboards consume Bim planning receipts.
- Whisper.net owns translate-to-English captioning; broader translation binds through a locale service row.
- Kernel `Analyze` receipt projection enters inspector and dashboard surfaces through the receipt spine.
- `SurfaceHost.RhinoPanel` mounts only when a Rhino lease provider supplies `EmbedCapsule` and `RenderGraph.Lease`.

## [04]-[PROHIBITIONS]

The closed NEVER list — the deleted patterns the owner regions foreclose. Every UI seed cites the concrete owner that forecloses it.

- NEVER runtime XAML for production view materialization — every view enters the tree through its `Configure<TApp>` compiled-XAML class, `Surfaces.RejectRuntimeXaml` is the never-callable surface folding an `AvaloniaXamlLoader.Load(this)`/`Load(uri)` attempt into `SurfaceFault.MountRejected`, and the `Avalonia.Markup.Xaml.Loader` markup-loader resolves only inside the Debug HotAvalonia closure.
- NEVER per-host `GpuBackend`/`GRContext.CreateMetal`/`CreateVulkan`/`CreateGl` construction in a dispatch arm — Avalonia owns backend selection through `EmbedOptions.RenderingMode` (`AvaloniaNativePlatformOptions.RenderingMode`), and a shared-context requirement against the host pipeline rides one `SurfaceSeam` delegate column bound at composition.
- NEVER a per-surface `AsyncImageLoader`, telemetry sink, or receipt sink — every owner contributes through the one `AppUiTelemetry.Contribute` spine and the `ReceiptSinkPort`, and the `RamCachedWebImageLoader`-backed `Loader` is the single image cache.
- NEVER an `SKSurface` outside the `Offscreen` capsule — the capture capsule owns the one Skia draw boundary and every offscreen render confines to it.
- NEVER ReactiveUI code-behind view binding — `PropertyBinderImplementation.Bind`/`OneWayBind`/`BindTo`, `CommandBinder.BindCommand`, and `IViewFor<TViewModel>` property-expression wiring are rejected wholesale; `BehaviorRail.Intent(ICommand)` is the single C# binding bridge and `BehaviorRail.RejectViewBinding` is the never-callable structural rejection folding the rejected binder symbols into the typed faulting rail.
- NEVER a second command, hotkey, palette, or conflict registry beside the one `CommandIntent` row table and `CommandDeck.Freeze` — menus, toolbars, access keys, hotkeys, tray items, palette entries, deep links, and remote verbs are derivation folds over the one table, and the freeze-time `GestureConflicts` fold is the only conflict evidence.
- NEVER a parallel control-generation or layout framework — a control-materialization or constraint-layout system is ONE polymorphic owner feeding the existing rails (the `ControlIntent` `[Union]` materialized through `ControlFactory`, the `LayoutConstraint` algebra solved by one `LayoutSolver` `Panel`), never a parallel framework with a second binding, token, or automation path.
- NEVER a per-surface virtualizer — viewport-range windowing, control recycling, variable-extent measurement, sticky headers, and hierarchical flatten ride the one `VirtualWindow` owner over `DynamicData` change-sets, and every windowed surface (tables, notebook, dashboard, canvas) consumes it.
- NEVER a generic `IReceipt`, ledger, or reported-value abstraction — every receipt stays its typed record (`CommandReceipt`, `FrameReceipt`, `ShaderReceipt`, `MediaReceipt`, `FormReceipt`, `EditReceipt`) sealed through `ReceiptSinkPort`.
- NEVER a fault code outside the `Diagnostics/evidence#FAULT_TABLES` registry — every AppUi fault union's `Code` derives through its 6xxx `AppUiFaultBand` row, a `base(detail, NNNN)` literal, a hex band, and a bare `Error.New` on a rail are the three deleted forms.
- NEVER a Loro byte as durable truth — the durable collaboration stream is the ONE `Collab/sync` `EditIntent` union projected onto Persistence-owned `OpLogEntry`/`SyncOpKind` rows; the Loro snapshot blob survives only as a content-keyed derivable cold-start accelerator, and a second CRDT or per-page op union is the deleted form.
- NEVER a second revert vocabulary beside the one inverse algebra — `RevertibleOp` forward/inverse deltas fold across the client `CancelableCommandRecorder` window and the durable Persistence `Version/ledger` `OpLogEntry` inverse stream (via the `Collab/sync` intent rail) as two arms of one `RevertScope`, the `EditOutcome.Reverted` outcome the only revert receipt.
- NEVER a second BCF or coordination-domain owner inside AppUi — `Rasm.Bim/coordination` owns the openBIM topic/component/comment exchange semantics and AppUi retains only the `Viewpoint` board projection over the consumed contract.
- NEVER an AppUi-local content-identity mint beside the kernel `ContentHash.Of` — every AppUi-side content hash composes the one federation seed-zero entry; Compute-minted payload/splat keys are decode-only.
- NEVER a local geodesy, solar-position, clustering, or recompute engine — Bim owns geodesy (`GeoFeature.Reproject`), Compute owns solar position (`SolarPosition.At`) and meshlet clustering (`ResidencyPayload`), and the AppHost `RecomputeGraph` owns incremental recompute; AppUi consumes each at its declared seam.
- CSP analyzer diagnostics are architecture pressure: fix the shape, refine the rule on a false positive, never suppress.

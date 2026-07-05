# [RASM_APPUI_ARCHITECTURE]

The domain map of `Rasm.AppUi` — the APP-PLATFORM Avalonia product UI engine. The `Shell`, `Render`, `Charts`, `Editing`, and `Theme` domain folders, each page a UI capability unit over the settled receipt spine and the GPU render surface.

Each codemap node is the eventual source file its `.planning/` design page becomes, named in the language's own folder and file casing — PascalCase `.cs`, lowercase `.py`, lowercase `.ts`. Treat every node as realized code; the `.planning/` scaffold is the authoring substrate, never part of the map.

## [01]-[DOMAIN_MAP]

```text codemap
Rasm.AppUi/
├── Shell/                # Host-mount axis and application shell spine
│   ├── Navigation.cs     # Routing spine with deep-link grammar over Dock-driven dockable layouts
│   ├── Screens.cs        # Screen catalog, ref-counted activation, OAPH-paced state, control-intent stream body
│   ├── Hosts.cs          # Host-neutral SurfaceHost mounting with seam delegate columns
│   ├── Commands.cs       # CommandIntent vocabulary with availability algebra and total receipts
│   ├── Controls.cs       # ControlIntent [Union] materialized through ControlFactory over BehaviorRail.Intent
│   ├── Solver.cs         # LayoutConstraint Kiwi algebra solved by one LayoutSolver custom Avalonia panel
│   ├── Virtualization.cs # VirtualWindow owner over DynamicData change-sets every windowed surface consumes
│   ├── Dialogs.cs        # Typed-Fin dialog intents with dismissal-as-value and host-agnostic pickers
│   ├── Input.cs          # Command-derived hotkeys, behavior rows, pan-zoom canvas, InputFabric device drivers
│   └── Accessibility.cs  # Automation identity, tab-order/trap law, one WCAG luminance gate
├── Render/               # GPU render surface and its viewport projections
│   ├── Pipeline.cs       # RenderGraph pass-DAG, per-backend RenderTargetFactory, resolve ladder, SimVisual, Viewpoint
│   ├── Meshlets.cs       # MeshletCluster GPU-driven cluster-LOD with bindless residency and VRAM-budget streaming
│   ├── PathTrace.cs      # BVH/ReSTIR/denoise integrator shading FROM Materials LayeredBsdf/SurfaceShade
│   ├── Capture.cs        # Offscreen render and document export over a Skia draw capsule
│   ├── Drafting.cs       # Sheet drafting with title-blocks, hidden-line projection, ASME Y14.5 dimensioning
│   ├── Reality.cs        # Gaussian-splat/point-cloud reality-capture viewport render-pass cases
│   ├── Evidence.cs       # One EvidenceReceipt union sealed through HLC sink envelope
│   ├── Shading.cs        # GPU ShaderAsset cache per GpuBackend feeding the LayeredBsdf SurfaceShade pass
│   └── Immersive.cs      # OpenXR stereo design-review plus FB passthrough over the shared Wgpu device
├── Charts/               # Chart and dashboard projection over receipt spine
│   ├── Dashboards.cs     # LiveCharts series/axis rows, LTTB-downsampled stream binding, cross-filter brushing
│   └── Custom.cs         # CustomVisual Skia layout-algebra rail for bespoke chart geometry
├── Editing/              # Typed-edit surfaces over model
│   ├── Inspector.cs      # Typed PropertyGrid inspection with ranked editor-factory rows and conflict resolution
│   ├── Tables.cs         # Tabular/hierarchical projection with column-metadata family routed through VirtualWindow
│   ├── Notebook.cs       # Reproducible computational document with CapabilityPin cells and CRDT co-editing
│   ├── Collab.cs         # LoroDoc-backed CollabDoc merge authority every co-edited surface composes; op-log sync, presence, Frontiers time-travel
│   ├── LiveData.cs       # Reactive data spine over closed DataSource cases and DynamicData operators
│   ├── Forms.cs          # FormSchema+wizard through ControlFactory, selection batch-edit folding one CommandReceipt
│   ├── History.cs        # RevertibleOp inverse algebra over CancelableCommandRecorder + durable op-log
│   ├── Media.cs          # Markdig inlines + MediaSurface [Union] codec rows on the one SurfaceSeam (LibMpv)
│   ├── Issues.cs         # openBIM issue board PROJECTION over Rasm.Bim-owned BCF topic/component contract
│   └── Tour.cs           # Saved-viewpoint review tour over animation playhead
└── Theme/                # Vocabulary tier: tokens, typography, motion, assets, locale
    ├── Tokens.cs         # Design-token engine with OKLab ramp mix and atomic theme swap
    ├── Typography.cs     # Type roles, embedded-Inter admission, one HarfBuzz shaping rail
    ├── Motion.cs         # Motion tokens with spring algebra and ProgressPhase-to-MotionToken map
    ├── Animation.cs      # Timeline keyframe-track union with frame-indexed playhead and re-entrant scrub
    ├── Assets.cs         # Nameof-derived AssetKey vocabulary with rank-fallback icon sourcing
    └── Locale.cs         # Resx/ICU/NodaTime locale rows with pseudo-locale conformance and RTL mirroring
```

`Shell` owns the host-mount axis and the application shell: the host-mount axis precedes the shell, the shell precedes the screens it routes, and the command, control, layout, virtualization, dialog, input, and accessibility pages ride the same spine — `controls` materializes the one `ControlIntent` family through `ControlFactory`, `solver` solves the `LayoutConstraint` algebra in one `LayoutSolver` panel, and `virtualization` owns the one `VirtualWindow` every windowed surface consumes. `Theme` is the vocabulary tier every literal traces to. `Render` is the GPU render surface — `pipeline` drives the frame pass-DAG and draws the `meshlets` cluster-LOD and the `pathtrace` integrator, `drafting`, `reality`, and `evidence` resolve over the receipt spine, the `capture` codec, and the GPU render-target factory, and `shading` and `immersive` deepen it (the GPU shader-asset cache feeding the `LayeredBsdf` shade, the OpenXR stereo surface) without minting a parallel scene model or a second GPU device. `Charts` and `Editing` project over the same receipt spine; `forms` and `history` deliver declarative forms, batch editing, and the one revertible-op inverse algebra over `PropertyModels`, `media` renders markdown inlines and mounts codec rows on the one `SurfaceSeam`, `Editing/issues` composes the viewport, notebook, and chart owners as a pure board PROJECTION over the `Rasm.Bim`-owned BCF topic contract, and `Editing/tour` rides the animation playhead. `Editing/collab` is the one `CollabDoc` `LoroDoc`-backed CRDT merge authority the notebook, issue-board, table, and live-data annotation surfaces compose — no per-surface last-writer-wins register or fractional-index algebra survives beside it, its op-log delta projects onto the `Rasm.Persistence` op-log and its content-keyed snapshot crosses the blob lane.

## [02]-[SEAMS]

```text seams
Shell/commands    →  typescript:core/interchange/codec           # [WIRE]: CommandPayloadWire + AvailabilityStore gate
Render/capture    →  typescript:core/interchange/codec           # [WIRE]: RenderReceiptWire frame-hash proof
Render/capture    →  typescript:ui/viewer                        # [RECEIPT]: RenderReceipt claims paired with local render evidence at the probe plane
Render/evidence   →  typescript:core/state/feed                  # [WIRE]: EvidenceFeed / EvidenceTimeline
Render/pipeline   →  typescript:core/interchange/codec           # [WIRE]: GeometryResidencyWire ResidencyManifest content-key
Render/glb        →  typescript:ui/viewer                        # [RECEIPT]: ResidencyManifest content-key-keyed mesh residency
Render            ←  python:geometry/mesh                        # [SHAPE]: SharpGLTF GLB import per-element tessellation
Editing/notebook  ←  csharp:Rasm.AppHost/Runtime                 # [PORT]: DeterminismContext / CapabilityPin environment identity
Render/query      ←  csharp:Rasm.Bim/Model                       # [PORT]: ElementSet query algebra via capability descriptor
Charts            ←  csharp:Rasm.Bim/Semantics/geospatial        # [SHAPE]: Basemap draws Bim-owned NetTopologySuite as map overlays beside the Wgpu viewport
Charts            ←  csharp:Rasm.Bim/Planning                    # [RECEIPT]: ScheduleNetwork CPM/4D/CostSchedule/etc Render as Charts/dashboards projections
Editing/collab    →  csharp:Rasm.Persistence/Store               # [CONTENT_KEY]: CollabSnapshot XxHash128 op-log-snapshot accelerator blob
Editing/collab    →  csharp:Rasm.Persistence/Sync                # [PROJECTION]: CollabDoc op-log delta durably projected as OpLogEntry
Render/pipeline   ←  csharp:Rasm.Compute/Runtime                 # [PROJECTION]: ResidencyManifest.Mint web geometry residency
Render            ←  csharp:Rasm/Drawing/view                    # [PROJECTION]: DrawingProjection / drafting-sheet layout
Render            ←  csharp:Rasm/Processing/flatten              # [PROJECTION]: ChartAtlas / texture UV channel
Render/reality    ←  csharp:Rasm.Compute/Runtime                 # [PROJECTION]: SplatPayload / PointPayload decode
Render/drafting   ←  csharp:Rasm.Fabrication/Posting             # [BOUNDARY]: HiddenLineSeam over the kernel DrawingProjection analytic HLR
Render            ←  csharp:Rasm.Fabrication/Posting/projection  # [RECEIPT]: HiddenLineResult Viewport2D edge sets
Render/pathtrace  ←  csharp:Rasm.Materials/Appearance            # [BOUNDARY]: LayeredBsdf / SlabStack / SurfaceShade at PATH_TRACE seam
Editing/issues    ←  csharp:Rasm.Bim/coordination                # [PORT]: BCF issue-board domain
Editing           ←  csharp:Rasm.Bim/coordination                # [SHAPE]: BcfTopic/BcfComment/BcfViewpoint annotation domain
Editing/history   →  csharp:Rasm.Persistence/Sync                # [PROJECTION]: Forward/inverse delta replays as SyncOpKind durable inverse stream
Render/shading    ⇄  csharp:Rasm.Compute                         # [SHAPE]: shared ONE_WGPU_DEVICE (Silk.NET.WebGPU)
Shell/controls    →  typescript:core/interchange/codec           # [WIRE]: ControlIntentWire kind-discriminated control vocabulary
Shell/controls    →  typescript:ui/viewer                        # [WIRE]: ControlIntent six-kind union materialized at the panel plane
Shell/solver      →  typescript:core/interchange/codec           # [WIRE]: LayoutConstraintWire ordered Kiwi constraint program
Shell/solver      →  typescript:ui/viewer                        # [WIRE]: ordered LayoutProgram re-solved at the panel plane
```

## [05]-[PROHIBITIONS]

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
- NEVER a second revert vocabulary beside the one inverse algebra — `RevertibleOp` forward/inverse deltas fold across the client `CancelableCommandRecorder` window and the durable `Rasm.Persistence/Sync` `OpLogEntry` inverse stream as two arms of one `RevertScope`, the `EditOutcome.Reverted` outcome the only revert receipt.
- NEVER a second BCF or coordination-domain owner inside AppUi — `Rasm.Bim/coordination` owns the openBIM topic/component/comment exchange semantics and AppUi retains only the `Viewpoint` board projection over the consumed contract.
- CSP analyzer diagnostics are architecture pressure: fix the shape, refine the rule on a false positive, never suppress.

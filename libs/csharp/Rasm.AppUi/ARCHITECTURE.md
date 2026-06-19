# [RASM_APPUI_ARCHITECTURE]

The domain map of `Rasm.AppUi` — the APP-PLATFORM Avalonia product UI engine. The `Shell`, `Render`, `Charts`, `Editing`, and `Theme` domain folders, each page a UI capability unit over the settled receipt spine and the GPU render surface.

Each codemap node is the eventual source file its `.planning/` design page becomes, named in the language's own folder and file casing — PascalCase `.cs`, lowercase `.py`, lowercase `.ts`. Treat every node as realized code; the `.planning/` scaffold is the authoring substrate, never part of the map.

## [01]-[DOMAIN_MAP]

```text codemap
Rasm.AppUi/
├── Shell/                # Host-mount axis and application shell spine
│   ├── Navigation.cs     # Routing spine with deep-link grammar over Dock-driven dockable layouts
│   ├── Screens.cs        # Screen catalog and ref-counted activation lifecycle with OAPH-paced state
│   ├── Hosts.cs          # Host-neutral SurfaceHost mounting with seam delegate columns
│   ├── Commands.cs       # One CommandIntent vocabulary with availability algebra and total receipts
│   ├── Dialogs.cs        # Typed-Fin dialog intents with dismissal-as-value and host-agnostic pickers
│   ├── Input.cs          # Command-derived hotkeys, behavior trigger/action rows, pan-zoom canvas
│   └── Accessibility.cs  # Automation identity, tab-order/trap law, one WCAG luminance gate
├── Render/               # GPU render surface and its viewport projections
│   ├── Viewport.cs       # GPU RenderGraph pass-DAG with meshlet cluster-LOD and ReSTIR path tracing
│   ├── Capture.cs        # Offscreen render and document export over a Skia draw capsule
│   ├── Drafting.cs       # Sheet drafting with title-blocks, hidden-line projection, ASME Y14.5 dimensioning
│   ├── Reality.cs        # Gaussian-splat/point-cloud reality-capture viewport render-pass cases
│   ├── Evidence.cs       # One EvidenceReceipt union sealed through HLC sink envelope
│   ├── Shading.cs        # PLANNED — GPU shader-asset owner with per-backend pipeline-state cache
│   └── Immersive.cs      # PLANNED — OpenXR stereo design-review over shared Wgpu device
├── Charts/               # Chart and dashboard projection over receipt spine
│   ├── Dashboards.cs     # LiveCharts series/axis rows, LTTB-downsampled stream binding, cross-filter brushing
│   └── Custom.cs         # CustomVisual Skia layout-algebra rail for bespoke chart geometry
├── Editing/              # Typed-edit surfaces over model
│   ├── Inspector.cs      # Typed PropertyGrid inspection with ranked editor-factory rows and conflict resolution
│   ├── Tables.cs         # Tabular/hierarchical projection with column-metadata family and tree-flatten fold
│   ├── Notebook.cs       # Reproducible computational document with CapabilityPin cells and CRDT co-editing
│   ├── LiveData.cs       # Reactive data spine over closed DataSource cases and DynamicData operators
│   ├── Issues.cs         # openBIM issue board over Rasm.Bim BCF topic/component contract
│   └── Tour.cs           # Saved-viewpoint review tour over animation playhead
└── Theme/                # Vocabulary tier: tokens, typography, motion, assets, locale
    ├── Tokens.cs         # Design-token engine with OKLab ramp mix and atomic theme swap
    ├── Typography.cs     # Type roles, embedded-Inter admission, one HarfBuzz shaping rail
    ├── Motion.cs         # Motion tokens with spring algebra and ProgressPhase-to-MotionToken map
    ├── Animation.cs      # Timeline keyframe-track union with frame-indexed playhead and re-entrant scrub
    ├── Assets.cs         # Nameof-derived AssetKey vocabulary with rank-fallback icon sourcing
    └── Locale.cs         # Resx/ICU/NodaTime locale rows with pseudo-locale conformance and RTL mirroring
```

`Shell` owns the host-mount axis and the application shell: the host-mount axis precedes the shell, the shell precedes the screens it routes, and the command, dialog, input, and accessibility pages ride the same spine. `Theme` is the vocabulary tier every literal traces to. `Render` is the GPU render surface — `viewport`, `drafting`, `reality`, and `evidence` resolve over the receipt spine, the `capture` codec, and the GPU render-target factory, and the planned `shading` and `immersive` pages deepen it without minting a parallel scene model. `Charts` and `Editing` project over the same receipt spine; `Editing/issues` composes the viewport, notebook, and chart owners over the `Rasm.Bim` BCF topic contract, and `Editing/tour` rides the animation playhead.

## [02]-[SEAMS]

```text seams
Shell/commands    →  typescript:interchange/transport            # [WIRE]: CommandPayloadWire + AvailabilityStore gate
Render/capture    →  typescript:interchange/codec                # [PROJECTION]: RenderReceiptWire frame-hash proof
Render/evidence   →  typescript:projection/evidence              # [PROJECTION]: EvidenceFeed / EvidenceTimeline
Render/viewport   →  typescript:platform/transport               # [PROJECTION]: GeometryResidencyWire ResidencyManifest content-key
Render/glb        →  typescript:ui/render                        # [RECEIPT]: ResidencyManifest content-key-keyed mesh residency
Render            ←  python:geometry/mesh                        # [SHAPE]: SharpGLTF GLB import per-element tessellation
Editing/notebook  ←  csharp:Rasm.AppHost/Runtime                 # [PORT]: DeterminismContext / CapabilityPin environment identity
Render/query      ←  csharp:Rasm.Bim/Model                       # [PORT]: ElementSet query algebra via capability descriptor
Editing           ←  csharp:Rasm.Persistence/Sync                # [PROJECTION]: annotation collaboration op-log
Editing/notebook  ←  csharp:Rasm.Persistence/Sync                # [PROJECTION]: NotebookOp op-log
Render            ←  csharp:Rasm.Compute/Runtime                 # [PROJECTION]: ResidencyManifest.Mint web geometry residency
Render            ←  csharp:Rasm/Geometry/Drawing                # [PROJECTION]: DrawingProjection / drafting-sheet layout
Render            ←  csharp:Rasm/Geometry/Processing             # [PROJECTION]: ChartAtlas / texture UV channel
Render/reality    ←  csharp:Rasm.Compute/Runtime                 # [PROJECTION]: SplatPayload / PointPayload decode
Render/drafting   ←  csharp:Rasm.Fabrication/Posting             # [BOUNDARY]: HiddenLineSeam BSP visibility solver
Render            ←  csharp:Rasm.Fabrication/Posting/projection  # [RECEIPT]: HiddenLineResult Viewport2D edge sets
Render            ←  csharp:Rasm.Materials/Appearance            # [BOUNDARY]: LayeredBsdf / SurfaceShade at path tracer
```

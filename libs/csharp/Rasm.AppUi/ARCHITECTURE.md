# [RASM_APPUI_ARCHITECTURE]

The professional domain map of `Rasm.AppUi` — the APP-PLATFORM Avalonia product UI engine. Five domain folders (`Shell`, `Render`, `Charts`, `Editing`, `Theme`), each page a UI capability unit over the settled receipt spine and the GPU render frontier.

Each codemap node is the eventual source file its `.planning/` design page becomes, named in the language's own folder and file casing — PascalCase `.cs`, lowercase `.py`, lowercase `.ts`. Treat every node as realized code; the `.planning/` scaffold is the authoring substrate, never part of the map.

## [1]-[DOMAIN_MAP]

```text codemap
Rasm.AppUi/
├── Shell/                # the host-mount axis and the application shell spine
│   ├── Navigation.cs     # routing spine with deep-link grammar over Dock-driven dockable layouts
│   ├── Screens.cs        # screen catalog and ref-counted activation lifecycle with OAPH-paced state
│   ├── Hosts.cs          # host-neutral SurfaceHost mounting with seam delegate columns
│   ├── Commands.cs       # the one CommandIntent vocabulary with availability algebra and total receipts
│   ├── Dialogs.cs        # typed-Fin dialog intents with dismissal-as-value and host-agnostic pickers
│   ├── Input.cs          # command-derived hotkeys, behavior trigger/action rows, pan-zoom canvas
│   └── Accessibility.cs  # automation identity, tab-order/trap law, the one WCAG luminance gate
├── Render/               # the GPU frontier and its viewport projections
│   ├── Viewport.cs       # GPU RenderGraph pass-DAG with meshlet cluster-LOD and ReSTIR path tracing
│   ├── Capture.cs        # offscreen render and document export over a Skia draw capsule
│   ├── Drafting.cs       # sheet drafting with title-blocks, hidden-line projection, ASME Y14.5 dimensioning
│   ├── Reality.cs        # Gaussian-splat/point-cloud reality-capture viewport render-pass cases
│   ├── Evidence.cs       # the one EvidenceReceipt union sealed through the HLC sink envelope
│   ├── Shading.cs        # PLANNED — GPU shader-asset owner with per-backend pipeline-state cache
│   └── Immersive.cs      # PLANNED — OpenXR stereo design-review over the shared Wgpu device
├── Charts/               # chart and dashboard projection over the receipt spine
│   ├── Dashboards.cs     # LiveCharts series/axis rows, LTTB-downsampled stream binding, cross-filter brushing
│   └── Custom.cs         # the CustomVisual Skia layout-algebra rail for bespoke chart geometry
├── Editing/              # the typed-edit surfaces over the model
│   ├── Inspector.cs      # typed PropertyGrid inspection with ranked editor-factory rows and conflict resolution
│   ├── Tables.cs         # tabular/hierarchical projection with column-metadata family and tree-flatten fold
│   ├── Notebook.cs       # reproducible computational document with CapabilityPin cells and CRDT co-editing
│   ├── LiveData.cs       # reactive data spine over closed DataSource cases and DynamicData operators
│   ├── Issues.cs         # openBIM issue board over the Rasm.Bim BCF topic/component contract
│   └── Tour.cs           # saved-viewpoint review tour over the animation playhead
└── Theme/                # the vocabulary tier: tokens, typography, motion, assets, locale
    ├── Tokens.cs         # design-token engine with OKLab ramp mix and atomic theme swap
    ├── Typography.cs     # type roles, embedded-Inter admission, the one HarfBuzz shaping rail
    ├── Motion.cs         # motion tokens with spring algebra and the ProgressPhase-to-MotionToken map
    ├── Animation.cs      # timeline keyframe-track union with frame-indexed playhead and re-entrant scrub
    ├── Assets.cs         # nameof-derived AssetKey vocabulary with rank-fallback icon sourcing
    └── Locale.cs         # resx/ICU/NodaTime locale rows with pseudo-locale conformance and RTL mirroring
```

`Shell` owns the host-mount axis and the application shell: the host-mount axis precedes the shell, the shell precedes the screens it routes, and the command, dialog, input, and accessibility pages ride the same spine. `Theme` is the vocabulary tier every literal traces to. `Render` is the GPU frontier — `viewport`, `drafting`, `reality`, and `evidence` resolve over the receipt spine, the `capture` codec, and the GPU render-target factory, and the planned `shading` and `immersive` pages deepen it without minting a parallel scene model. `Charts` and `Editing` project over the same receipt spine; `Editing/issues` composes the viewport, notebook, and chart owners over the `Rasm.Bim` BCF topic contract, and `Editing/tour` rides the animation playhead.

## [2]-[SEAMS]

```text seams
Shell/commands    →  typescript:interchange/transport  # CommandPayloadWire + AvailabilityStore gate (wire)
Render/capture    →  typescript:interchange/codec      # RenderReceiptWire frame-hash proof (projection)
Render/evidence   →  typescript:projection/evidence    # EvidenceFeed / EvidenceTimeline (projection)
Render/viewport   →  typescript:platform/transport     # GeometryResidencyWire ResidencyManifest content-key (projection)
Render/glb        →  typescript:ui/render              # ResidencyManifest content-key-keyed mesh residency (receipt)
Render            ←  python:geometry/mesh              # SharpGLTF GLB import per-element tessellation (shape)
Editing/notebook  ←  csharp:Rasm.AppHost/Runtime       # DeterminismContext / CapabilityPin environment identity (port)
Render/query      ←  csharp:Rasm.Bim/Model             # ElementSet query algebra via capability descriptor (port)
Editing           ←  csharp:Rasm.Persistence/Sync      # annotation collaboration op-log (projection)
Editing/notebook  ←  csharp:Rasm.Persistence/Sync      # NotebookOp op-log (projection)
Render            ←  csharp:Rasm.Compute/Runtime       # ResidencyManifest.Mint web geometry residency (projection)
Render            ←  csharp:Rasm/Geometry/Drawing      # DrawingProjection / drafting-sheet layout (projection)
Render            ←  csharp:Rasm/Geometry/Processing   # ChartAtlas / texture UV channel (projection)
Render/reality    ←  csharp:Rasm.Compute/Runtime       # SplatPayload / PointPayload decode (projection)
```

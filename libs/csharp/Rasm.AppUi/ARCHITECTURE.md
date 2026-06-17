# [RASM_APPUI_ARCHITECTURE]

The professional domain map for the product UI engine. Each sub-domain is a genuine UI capability folder mirroring the eventual source tree under `.planning/`; the codemap names every sub-domain including the planned-but-empty ones, each with a one-line charter. Dependency direction across the C# strata is stated once in the branch `ARCHITECTURE.md`; boundaries and wires live on the task cards that build them.

## [1]-[DOMAIN_MAP]

The host-mount axis precedes the shell, the shell precedes the screens it routes, and the vocabulary owners (theme, typography, assets, motion) precede every consumer that traces a literal to them. The viewport, drafting, notebook, and animation folders are projections over the settled receipt spine, the visuals codec, and the Compute geometry/field receipts. Two planned-but-empty sub-domains mark visible gaps: `realitycapture/` (a Viewport reality-capture sibling) and `coordination/` (an openBIM issue board composing the viewport, notebook, and chart owners over the `Rasm.Bim` BCF topic contract).

```text codemap
Rasm.AppUi/
├── hosts/              # Host-neutral surface mounting: SurfaceHost axis, seam delegate columns, embed capsule, scheduler boundary, native-asset identity, host fact stream
├── shell/              # Application shell and navigation: routing spine with deep-link grammar, Dock-driven dockable layouts, intent-keyed chrome slots, adaptive breakpoint table
├── screens/            # Screen catalog and activation lifecycle: ref-counted activation scopes, paced derived state through OAPH, typed-rail validation lift, per-surface snapshots
├── commands/           # The one CommandIntent vocabulary: availability algebra over CanExecute, total execution receipts, palette/deep-link/remote/journal-replay projections
├── livedata/           # Reactive data spine: closed DataSource cases, DynamicData change-set operator rows, the single UI-thread BindingCapsule marshal, aggregation/audit fold
├── tables/             # Tabular and hierarchical projection: one column-metadata family, serializable view-state snapshots, the tree-flatten fold onto the free DataGrid, grid-commit to store ops
├── inspector/          # Typed property inspection and editing: PropertyGrid admission capsule, ranked editor-factory rows, preview-versus-commit validation, three-way conflict resolution, code panes
├── charts/             # Charting and dashboards: LiveCharts series/axis rows, LTTB-downsampled stream binding, dashboard tiles with cross-filter bitset brushing, the Skia CustomVisual layout-algebra rail
├── visuals/            # Offscreen render and document export: borrowed/owned Skia draw capsule, blob-backed thumbnails, content-hashed encode identity, paged SKDocument/Office export flow algebra
├── theme/              # Design-token engine: frozen token catalog feeding five consumers, OKLab ramp mix, orthogonal variant/density resolve, perceptual data colormaps, atomic theme swap with diff receipts
├── typography/         # Type and text shaping: typography roles, deterministic embedded-Inter admission with fallback chains, the one HarfBuzz shaping rail before Skia glyphs, Markdig role projection, baseline metrics
├── assets/             # Icons and asset loading: nameof-derived AssetKey vocabulary, icon sourcing with rank-fallback, retained SVG scene-graph pipeline, async raster rows with disk/RAM cache and DPI variants
├── dialogs/            # Dialogs and notifications: typed-Fin dialog intents with dismissal-as-value, per-surface session topology, toast suppression fold over phase/degradation, host-agnostic pickers
├── input/              # Input and interaction: command-derived hotkeys with chord transform, admitted behavior trigger/action rows, pan-zoom canvas family, typed drag/clipboard codecs, alternative-input fabric
├── motion/             # Motion vocabulary: motion tokens with reduced pairs and spring algebra, plan rows binding transitions/charts/zoom/clocks, the frozen ProgressPhase-to-MotionToken map, reduced-motion degrade
├── access/             # Accessibility: catalog-sourced automation identity and live regions, tab-order/trap/refocus law, the single WCAG luminance gate, variant-density compliance audit, 3D-scene accessibility tree
├── localization/       # Locale and culture: resx/ICU/NodaTime locale rows with pseudo-locale conformance, nameof string tables, atomic culture composition, RTL mirroring, complex-script 3D-annotation shaping
├── evidence/           # Diagnostics and evidence: the one EvidenceReceipt union sealed through the HLC sink envelope, correlation join with skew bands, render-hash capture lanes, headless proof matrix, dev-loop HUD
├── viewport/           # GPU viewport pipeline: RenderGraph pass-DAG over a host-shared GRContext lease, meshlet cluster-LOD geometry, VRAM-budget residency, ReSTIR path tracing, sim-field passes, BCF Viewpoint codec
├── drafting/           # Sheet drafting: locale-aware sheet sets with ISO/ANSI/JIS title-blocks, 3D-to-2D hidden-line projection sharing the viewport camera basis, ASME Y14.5 dimensioning/GD&T, multi-format emit
├── notebook/           # Reproducible computational document: closed cell-kind union with a CapabilityPin per code/chart cell, dependency-DAG dirty-recompute, CRDT op-log co-editing, export-to-replay bundles
├── animation/          # Timeline animation: keyframe-track union over params/cameras/visibility/fields/color, frame-indexed playhead with loop/ping-pong, re-entrant scrub with no drift, offline walkthrough render
├── realitycapture/     # [PLANNED] Gaussian-splat and point-cloud reality-capture visualization projected as a viewport render-pass case, with a LiDAR-anchored measurable overlay and time-based capture playback
└── coordination/       # [PLANNED] openBIM issue board composing the Viewpoint view-state, the Rasm.Bim BCF topic/component contract, a CRDT comment thread, and a snapshot tile — the board projection, never a second BCF semantic model
```

Each page is one transcription unit per eventual source file. A new capability deepens the owning sub-domain through rows, cases, and policy values rather than a new public surface beside it. The TS_PROJECTION clusters carried inside pages transcribe into the TypeScript workspace at web app-root creation, never as a C# source file.

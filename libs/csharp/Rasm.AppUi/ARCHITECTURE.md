# [RASM_APPUI_ARCHITECTURE]

`Rasm.AppUi` maps the APP-PLATFORM Avalonia product-UI engine over the settled receipt spine and the GPU render surface: each sub-domain page is a UI capability unit lowering onto the one 6xxx `AppUiFaultBand`. S4 seats it as the consuming leaf: it references `{Rasm, Rasm.AppHost, Rasm.Compute, Rasm.Persistence}` downward, re-owns none of their capability, and never becomes the composition root.

## [01]-[DOMAIN_MAP]

```text codemap
Rasm.AppUi/
├── Shell/                # Host-mount axis and application shell spine
│   ├── Navigation.cs     # Routing spine with typed deep-link grammar over dockable layouts
│   ├── Screens.cs        # Screen catalog with ref-counted activation and OAPH-paced state
│   ├── Hosts.cs          # Host-neutral surface mounting with seam delegate columns
│   ├── Commands.cs       # Command vocabulary with availability algebra and total receipts
│   ├── Controls.cs       # ControlIntent union materialized through one control factory
│   ├── Solver.cs         # Layout-constraint Kiwi algebra solved by one custom panel
│   ├── Virtualization.cs # One virtual-window owner over change-sets and an extent ledger
│   ├── Dialogs.cs        # Typed-Fin dialog intents with dismissal-as-value and agnostic pickers
│   ├── Input.cs          # Command-derived hotkeys, behavior rows, pan-zoom canvas, device drivers
│   └── Accessibility.cs  # Automation identity, tab-order and trap law, one WCAG luminance gate
├── Render/               # Pure GPU-viewport and temporal plane
│   ├── Pipeline.cs       # Render-graph pass-DAG over per-backend GPU targets and the resolve ladder
│   ├── Meshlets.cs       # Compute residency cluster consumption with hysteresis LOD and cull cut
│   ├── PathTrace.cs      # BVH, ReSTIR, denoise oracle, and sun study over the one light rig
│   ├── Shading.cs        # GPU shader cache per backend feeding the layered-BSDF shade pass
│   ├── Immersive.cs      # OpenXR stereo design-review and passthrough over the shared device
│   ├── Reality.cs        # Gaussian-splat and point-cloud capture over the one residency carrier
│   ├── Capture.cs        # Raster capsule, color-policy owner, vector-print arm, and encode rows
│   ├── Drafting.cs       # Sheet drafting with hidden-line consumption and one DWG/DXF write leg
│   └── Animation.cs      # Timeline keyframe-track union with 4D schedule playback
├── Charts/               # Chart, dashboard, and geo-basemap projection
│   ├── Dashboards.cs     # Chart series and axis rows with downsampled stream binding and brushing
│   ├── Custom.cs         # Custom-visual Skia layout algebra with a keyed color-policy projection
│   ├── Basemap.cs        # Tiled basemap with Bim-owned overlays and EditManager redlining beside the viewport
│   └── Telemetry.cs      # Telemetry board over instrument, SLO burn-rate, store-profile, and evidence-track tiles
├── Editing/              # Typed-edit surfaces over the model
│   ├── Inspector.cs      # Typed property inspection with ranked editor rows and diff3 conflict hunks
│   ├── Tables.cs         # Tabular and hierarchical projection routed through the virtual window
│   ├── Forms.cs          # Form-schema wizard through the control factory, batch-edit folding one receipt
│   ├── History.cs        # Revertible-op inverse algebra over the recorder and a durable-ledger arm
│   ├── LiveData.cs       # Reactive data spine over closed data-source cases and change-set operators
│   └── Graph.cs          # Node-editor parametric canvas with an admission gate and co-edit merge
├── Document/             # Reproducible document plane
│   ├── Notebook.cs       # Capability-pinned cells composing the recompute graph; co-editing; replay
│   ├── Media.cs          # Markdown inlines and codec rows materialized for the one Surfaces.Mount crossing
│   └── Export.cs         # Paginated flow reports, PDF security and forms, Office and print arms, support-bundle rows
├── Collab/               # Live-collaboration plane over the durable Persistence spine
│   ├── Sync.cs           # Live-merge authority and the typed edit-intent stream onto the durable ledger
│   ├── Issues.cs         # openBIM issue board projection over the Bim BCF contract
│   └── Tour.cs           # Review tour as a camera-track projection with presenter-follow presence
├── Diagnostics/          # Evidence, proof, dev loop, and quality governance
│   ├── Evidence.cs       # Evidence-receipt union, telemetry spine and fan, correlation join, 6xxx fault registry
│   ├── Proof.cs          # Capture lanes, headless proof matrix, frame-bench lanes, goldens, and a typed proof fault
│   ├── DevLoop.cs        # Hot-reload knobs, inspector, HUD, flamegraph, solve scrub, and a REPL
│   └── Governor.cs       # Perf-budget quality governor with timestamp attribution
└── Theme/                # Pure vocabulary: tokens, typography, motion, assets, locale
    ├── Tokens.cs         # Design-token engine with an OKLab ramp mix and atomic theme swap
    ├── Typography.cs     # Type roles, embedded-font admission, one shaping rail, live front-matter
    ├── Motion.cs         # Motion tokens with spring algebra and a progress-to-token map
    ├── Assets.cs         # Nameof-derived asset-key vocabulary with rank-fallback sourcing
    └── Locale.cs         # Locale rows over Resx, ICU, and time, a typed locale fault, live captioning
```

`Shell` owns the host-mount axis and application spine — the mount precedes the shell, the shell precedes the screens it routes — and `Theme` is the pure vocabulary every literal traces to. `Render` owns the GPU viewport and the temporal plane, `Document` the recompute graph and every paginated output, and `Diagnostics` the 6xxx fault registry, the proof matrix, the telemetry spine, and the quality governor. `Collab/sync` holds the one live-merge authority and the typed `EditIntent` union that is durable truth on the Persistence ledger — no Loro byte crosses durable truth.

## [02]-[STRATA]

Four member-resolved strata order the interior; `Diagnostics/Evidence` is the reciprocal hub — every owner derives its fault codes through `AppUiFaultBand` while `EvidenceReceipt` nests every producer's receipt record — so the hub seats S0 and the nesting reads as co-ownership, never an upward import; every consumption edge points down.

- S0 substrate — `AppUiFaultBand` 6xxx codes and the `AppUiTelemetry` dimension slots (`Diagnostics/Evidence`) over the kernel signal capsule.
- S0 vocabulary — pure `Theme` (`TokenRow`, `MotionToken`, `AssetKeys`); every visual literal traces here.
- S1 spines — one owner per fabric: the `CommandIntent` verb table with its `CommandDeck`, and the `VirtualWindowSpec` windowing fabric.
- S1 spines — the `LayoutSolver` constraint panel, the `EditReceipt` inspection rail, and the `RenderReceipt`/`RenderGraph` render spine.
- S2 streams — `BehaviorRail` binding, the `EditIntent`/`IntentLedger` live-merge authority (`Collab/Sync`), and the `RevertibleOp` inverse algebra.
- S3 surfaces — `ControlFactory` materializes every control over the spines and streams; `IssueBoard` projects over the intent ledger.
- S3 planes — notebook, export, and screen planes compose the same rungs.

```mermaid
---
config:
  layout: elk
  flowchart:
    curve: linear
    padding: 25
---
flowchart TB
    accTitle: Rasm.AppUi interior strata
    accDescr: Four member-resolved strata from the control and issue surfaces through the binding, intent, and revert streams and the one-owner spines onto the fault-band and theme substrate, every consumption edge downward and naming one sourced type.
    subgraph S3["S3 SURFACES"]
        Factory[ControlFactory]
        Board[IssueBoard]
    end
    subgraph S2["S2 STREAMS"]
        Rail[BehaviorRail]
        Intent[EditIntent]
        Revert[RevertibleOp]
    end
    subgraph S1["S1 SPINES"]
        Command[CommandIntent]
        Virtual[VirtualWindowSpec]
        Solver[LayoutSolver]
        Inspect[EditReceipt]
        Graph[RenderGraph]
    end
    subgraph S0["S0 SUBSTRATE"]
        Fault[AppUiFaultBand]
        Token[TokenRow]
    end
    Factory -->|"[IMPORT]: VirtualWindowSpec"| Virtual
    Factory -->|"[IMPORT]: LayoutSolver"| Solver
    Factory -->|"[IMPORT]: BehaviorRail"| Rail
    Factory -->|"[IMPORT]: TokenRow"| Token
    Board -->|"[IMPORT]: EditIntent"| Intent
    Rail -->|"[IMPORT]: CommandIntent"| Command
    Revert -->|"[IMPORT]: CommandIntent"| Command
    Revert -->|"[IMPORT]: EditReceipt"| Inspect
    Intent -->|"[IMPORT]: CommandIntent"| Command
    Command -->|"[IMPORT]: AppUiFaultBand"| Fault
    Graph -->|"[IMPORT]: AppUiFaultBand"| Fault
```

## [03]-[SEAMS]

Two fences split the seam map by counterpart role: the first binds the same-branch AEC peers, the kernel, and the durable store; the second binds the platform host and the TypeScript peers. Each collapsed edge stands for every contract between that owner and that partner at the load-bearing kind; the owning pages enumerate the rest.

```mermaid
---
config:
  layout: elk
  flowchart:
    curve: linear
    padding: 25
---
flowchart LR
    accTitle: AppUi AEC-domain, render-source, and storage seams
    accDescr: AppUi render, chart, collaboration, and diagnostics owners exchanging residency projections, receipts, boundaries, content keys, and shared-device shapes with the AEC peers Compute, Fabrication, Materials, Bim, the kernel, and the Persistence store.
    subgraph appui[RASM.APPUI]
        Render[Render plane]
        Charts[Chart planes]
        Collab[Collab plane]
        Diagnostics[Diagnostics]
    end
    Compute{{Rasm.Compute}}
    Fabrication([Rasm.Fabrication])
    Materials([Rasm.Materials])
    Bim([Rasm.Bim])
    Rasm([Rasm])
    Persistence[(Rasm.Persistence)]
    Compute -->|"[PROJECTION]: ResidencyPayload"| Render
    Render <-->|"[SHAPE]: WgpuDevice"| Compute
    Compute -->|"[SHAPE]: SolarPosition"| Render
    Fabrication -->|"[RECEIPT]: HiddenLineResult"| Render
    Materials -->|"[BOUNDARY]: LayeredBsdf + SurfaceShade"| Render
    Rasm -->|"[CONTENT_KEY]: ContentHash"| Render
    Bim -->|"[SHAPE]: GeoTiles"| Charts
    Bim -->|"[RECEIPT]: CostSchedule"| Charts
    Persistence -->|"[PROJECTION]: telemetry measure series"| Charts
    Persistence -->|"[RECEIPT]: resident ReceiptEnvelope"| Diagnostics
    Bim -->|"[PORT]: IssueBoard"| Collab
    Collab -->|"[PROJECTION]: ReplayWindow"| Persistence
    Collab -->|"[CONTENT_KEY]: SnapshotAccelerator"| Persistence
    Bim -->|"[RECEIPT]: ConstructionState"| Render
    Bim -->|"[BOUNDARY]: BcfViewpoint"| Render
    Bim -->|"[SHAPE]: GeoReference"| Render
```

```mermaid
---
config:
  layout: elk
  flowchart:
    curve: linear
    padding: 25
---
flowchart LR
    accTitle: AppUi platform-host and cross-runtime wire seams
    accDescr: AppUi owners exchanging wires, receipts, ports, transport, and the fault-band adjacency with the app host and the TypeScript peers.
    subgraph appui[RASM.APPUI]
        Shell[Shell spine]
        Render[Render plane]
        Editing[Edit surfaces]
        Document[Document plane]
        Collab[Collab plane]
        Diagnostics[Diagnostics]
    end
    AppHost{{Rasm.AppHost}}
    Rasm([Rasm])
    Core([typescript:core])
    Ui([typescript:ui])
    Shell -->|"[WIRE]: CommandPayloadWire"| Core
    Render -->|"[WIRE]: GeometryResidencyWire"| Core
    Diagnostics -->|"[WIRE]: EvidenceTimelineWire"| Core
    Shell -->|"[WIRE]: ControlIntentWire + CommandGateWire + LayoutConstraintWire"| Ui
    Render -->|"[RECEIPT]: RenderReceipt"| Ui
    AppHost -->|"[PORT]: DeterminismContext"| Document
    Diagnostics <-->|"[FAULT]: FaultBand"| AppHost
    Rasm -->|"[PORT]: ReceiptSinkPort + InstrumentSpec + Slo"| Diagnostics
    AppHost -->|"[PORT]: HookRail"| Diagnostics
    AppHost -->|"[PORT]: ProfileSampleSource"| Diagnostics
    Collab <-->|"[TRANSPORT]: CollabWireContext"| AppHost
```

- `[PORT]: DeterminismContext` into `Document` — the AppHost runtime port spine composed at app composition; `CapabilityPin` anchors it.
- `[PORT]` into `Diagnostics` — observability spine: owners seal evidence through the kernel `ReceiptSinkPort`, declare instruments as kernel `InstrumentSpec` rows, and bind objectives through the kernel SLO algebra; telemetry projects facts, never produces them, and the AppHost half of the edge is the `HookRail` receipt point the fan taps.
- Instrument rows contribute through `TelemetryContributorPort` on the `TelemetrySource.AppUi` meter.
- Evidence fan subscribes to the `HookRail` receipt point as one observe row.
- `[CONTENT_KEY]` edges are one idiom — every content-identity mint composes the kernel `ContentHash.Of` seed-zero entry.
- Compute-minted residency and splat keys stay decode-only.
- `[PROJECTION]: ReplayWindow` also serves the Render version-compare lane — values only, AppUi runs no ledger read.
- Persistence `ReplayWindow`/commit-DAG fold derives the `(ElementId, DiffClass)` classification `VersionGhost` renders as `VisibilityOverride` rows.
- `[RECEIPT]: ConstructionState` — `SchedulePlayback.FromSchedule` reads `ConstructionState.At`/`TaskKind` as Bim-owned 4D schedule values.
- `[PROJECTION]: telemetry measure series` into `Charts` — store tiles name a facet coordinate beside its rollup column on the Persistence telemetry series and reach it through one injected read arrow.
- `[RECEIPT]: resident ReceiptEnvelope` into `Diagnostics` — the `EvidenceSource.Resident` arrow hands back envelopes, so the correlation join and the billing accrual stay one fold over two sources.
- Profiling custody, the pg_stat slots, and the `store.<domain>.<verb>` grammar stay Persistence-side.
- `[TRANSPORT]: CollabWireContext` — `Collab/sync` frames each delta as a `CollabFrame`, W3C carrier and Loro bytes.
- Merge extracts the originating correlation; AppUi holds only the composition-bound `Inject`/`Extract` delegates of AppHost `TraceContext`.
- `Rasm.AppHost [COLLAB_WIRE_CONTEXT]` owns the reciprocal — the `TraceContext` collab-frame adapter and the `COLLAB_DELTA_FEED` frame schema.
- `[PORT]: ProfileSampleSource` lands at `Diagnostics/devloop`, each sample keyed by correlation.
- Capture stays AppHost-side — Pyroscope span profiles, EventPipe CPU stacks; `FlameNode.Of` folds the samples into the frame tree.
- Feed rides an existing AppHost port row, never a new `PortCardinality` port.
- Samples carry the producer's symbolization posture, so AppUi renders the frames it received and resolves no address itself.

`Diagnostics ⇄ Rasm.AppHost` `[FAULT]` edge is the 6xxx `AppUiFaultBand` neighborhood: AppUi lowers every fault union onto its band and the AppHost lifecycle registry pins the reciprocal range, so fault codes never collide across the platform seam.

## [04]-[BOUNDARIES]

- Bim `ElementSet` queries enter through Bim-owned receipt rows.
- Cost and schedule dashboards consume the Bim `CostSchedule` and `ScheduleNetwork` planning receipts as `Charts/dashboards` feed values.
- Whisper.net owns translate-to-English captioning; broader translation binds through a locale service row.
- Kernel `Analyze` receipt projection enters inspector and dashboard surfaces through the receipt spine.
- `SurfaceMount.Panel` mounts on an embedded host surface only when a Rhino lease supplies `EmbedCapsule` and the `Render/pipeline` render-graph GPU lease.
- `Surfaces` mount gate admits a production view only as its compiled-XAML class, so a runtime XAML load has no mount path.
- Avalonia owns GPU backend selection through `EmbedOptions.RenderingMode`; no dispatch arm constructs a per-host `GpuBackend` or `GRContext`.
- `Offscreen` capsule owns the one Skia draw boundary and every `SKSurface` inside it.
- `BehaviorRail` intent bridge is the single C# view-binding seam and rejects binder symbols, so ReactiveUI code-behind binding has no seam to enter.
- `AppUiTelemetry.Contribute` is the one spine every owner routes image-load, telemetry, and receipt facts through; every receipt stays a typed record sealed at `ReceiptSinkPort`.
- `CommandIntent` table is the one verb registry — hotkey, palette, and conflict views are derivation folds over it.
- `ControlIntent` union through `ControlFactory` materializes every control, and the `LayoutConstraint` algebra solved by one `LayoutSolver` panel owns every layout.
- `VirtualWindow` over `DynamicData` change-sets owns every windowed surface.
- Every AppUi fault union derives its `Code` through its `AppUiFaultBand` row in the `Diagnostics/evidence` registry.
- Durable truth is the one `EditIntent` union projected onto Persistence-owned `OpLogEntry` rows; a Loro snapshot survives only as a content-keyed cold-start accelerator.
- `RevertibleOp` folds forward and inverse deltas across the recorder and the durable inverse stream.
- `Rasm.Bim` owns openBIM and coordination semantics; AppUi keeps the `Viewpoint` board projection alone.
- Every AppUi content hash composes the one kernel `ContentHash.Of` seed-zero entry, the branch's frozen content-key entry.
- Bim, Compute, and the AppHost `RecomputeGraph` own geodesy, solar position, clustering, and recompute.

# [RASM_APPUI_ARCHITECTURE]

`Rasm.AppUi` maps the APP-PLATFORM Avalonia product-UI engine over the settled receipt spine and the GPU render surface: each sub-domain page is a UI capability unit lowering onto the one 6xxx `AppUiFaultBand` and aligning with peers by contract, never by reference.

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
├── Render/               # Pure GPU-viewport and temporal tier
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
│   └── Export.cs         # Paginated flow reports, PDF security and forms, an Office arm, a print arm
├── Collab/               # Live-collaboration plane over the durable Persistence spine
│   ├── Sync.cs           # Live-merge authority and the typed edit-intent stream onto the durable ledger
│   ├── Issues.cs         # openBIM issue board projection over the Bim BCF contract
│   └── Tour.cs           # Review tour as a camera-track projection with presenter-follow presence
├── Diagnostics/          # Evidence, proof, dev loop, and quality governance
│   ├── Evidence.cs       # Evidence-receipt union, telemetry spine and fan, correlation join, 6xxx fault registry
│   ├── Proof.cs          # Capture lanes, headless proof matrix, goldens, and a typed proof fault
│   ├── DevLoop.cs        # Hot-reload knobs, inspector, HUD, flamegraph, solve scrub, and a REPL
│   └── Governor.cs       # Perf-budget quality governor with timestamp attribution
└── Theme/                # Pure vocabulary tier: tokens, typography, motion, assets, locale
    ├── Tokens.cs         # Design-token engine with an OKLab ramp mix and atomic theme swap
    ├── Typography.cs     # Type roles, embedded-font admission, one shaping rail, live front-matter
    ├── Motion.cs         # Motion tokens with spring algebra and a progress-to-token map
    ├── Assets.cs         # Nameof-derived asset-key vocabulary with rank-fallback sourcing
    └── Locale.cs         # Locale rows over Resx, ICU, and time, a typed locale fault, live captioning
```

`Shell` owns the host-mount axis and application spine — the mount precedes the shell, the shell precedes the screens it routes — and `Theme` is the pure vocabulary tier every literal traces to. `Render` owns the GPU-viewport and temporal tier, `Document` the recompute graph and every paginated output, and `Diagnostics` the 6xxx fault registry, the proof matrix, the telemetry spine, and the quality governor. `Collab/sync` holds the one live-merge authority and the typed `EditIntent` union that is durable truth on the Persistence ledger — no Loro byte crosses durable truth.

## [02]-[STRATA]

Four member-resolved strata order the interior; `Diagnostics/Evidence` is the reciprocal hub — every owner derives its fault codes through `AppUiFaultBand` while `EvidenceReceipt` nests every producer's receipt record — so the hub seats S0 and the nesting reads as co-ownership, never an upward import; every consumption edge points down.

- S0 substrate — the `AppUiFaultBand` 6xxx registry and `AppUiTelemetry` spine (`Diagnostics/Evidence`) beside the pure `Theme` vocabulary (`TokenRow`, `MotionToken`, `AssetKeys`); every literal and every fault code traces here.
- S1 spines — one owner per fabric: the `CommandIntent` verb table with its `CommandDeck`, the `VirtualWindowSpec` windowing fabric, the `LayoutSolver` constraint panel, the `EditReceipt` inspection rail, and the `RenderReceipt`/`RenderGraph` render tier.
- S2 streams — `BehaviorRail` binding, the `EditIntent`/`IntentLedger` live-merge authority (`Collab/Sync`), and the `RevertibleOp` inverse algebra, each folding the S1 spines.
- S3 surfaces — `ControlFactory` materializing every control over the spines and streams, and the `IssueBoard` projection over the intent ledger; the notebook, export, and screen planes compose these same rungs.

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
    subgraph L3["S3 SURFACES"]
        Factory[ControlFactory]
        Board[IssueBoard]
    end
    subgraph L2["S2 STREAMS"]
        Rail[BehaviorRail]
        Intent[EditIntent]
        Revert[RevertibleOp]
    end
    subgraph L1["S1 SPINES"]
        Command[CommandIntent]
        Virtual[VirtualWindowSpec]
        Solver[LayoutSolver]
        Inspect[EditReceipt]
        Graph[RenderGraph]
    end
    subgraph L0["S0 SUBSTRATE"]
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
    accDescr: AppUi render, chart, and collaboration owners exchanging residency projections, receipts, boundaries, content keys, and shared-device shapes with the AEC peers Compute, Fabrication, Materials, Bim, the kernel, and the Persistence store.
    subgraph appui[RASM.APPUI]
        Render[Render tier]
        Charts[Chart planes]
        Collab[Collab plane]
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
    Persistence -->|"[RECEIPT]: DuckProfileReceipt"| Charts
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
    accDescr: AppUi shell, render, editing, document, and diagnostics owners exchanging command, residency, and evidence wires, render receipts, determinism and receipt-hook ports, and the fault-band adjacency with the app host and the TypeScript core and viewer peers.
    subgraph appui[RASM.APPUI]
        Shell[Shell spine]
        Render[Render tier]
        Editing[Edit surfaces]
        Document[Document plane]
        Diagnostics[Diagnostics]
    end
    AppHost{{Rasm.AppHost}}
    Core([typescript:core])
    Ui([typescript:ui])
    Shell -->|"[WIRE]: CommandPayloadWire"| Core
    Render -->|"[WIRE]: GeometryResidencyWire"| Core
    Diagnostics -->|"[WIRE]: EvidenceTimelineWire"| Core
    Shell -->|"[WIRE]: ControlIntentWire"| Ui
    Render -->|"[RECEIPT]: RenderReceipt"| Ui
    AppHost -->|"[PORT]: DeterminismContext"| Document
    Diagnostics <-->|"[FAULT]: FaultBand"| AppHost
    AppHost -->|"[PORT]: ReceiptSinkPort + HookRail"| Diagnostics
```

- `[PORT]: DeterminismContext` into `Document` is the one AppHost runtime port spine every surface composes at app composition, resolving through the one `Rasm.AppHost/Runtime` boundary; `Document/notebook` `CapabilityPin` is its consumer anchor.
- `[PORT]` into `Diagnostics` is the observability spine: owners seal evidence through `ReceiptSinkPort`, contribute instrument rows through `TelemetryContributorPort` mounted on the `TelemetrySource.AppUi` meter, and the evidence fan subscribes to the `HookRail` receipt point as one observe row — telemetry projects facts, never produces them.
- `[CONTENT_KEY]` edges are one idiom: every AppUi content-identity mint composes the kernel `ContentHash.Of` seed-zero entry, and Compute-minted residency and splat keys stay decode-only.
- `[PROJECTION]: ReplayWindow` also serves the Render version-compare lane: the Persistence `ReplayWindow`/commit-DAG fold derives the `(ElementId, DiffClass)` classification `VersionGhost` renders as diff-classed `VisibilityOverride` rows — values only, AppUi runs no ledger read.
- `[RECEIPT]: ConstructionState` carries the 4D schedule-phase consumption: `Render/animation.md` `SchedulePlayback.FromSchedule` reads as values off `Rasm.Bim/Planning/schedule.md` `ConstructionState.At`/`TaskKind`.
- `[RECEIPT]: DuckProfileReceipt` into `Charts` is the store-profile board feed: `Charts/telemetry.md` tiles consume the Persistence `Store/observability` profile receipts as values over the analytical query lane — profiling custody, the pg_stat slots, and the `store.<domain>.<verb>` grammar stay Persistence-side.
- `[CARRIER]: CollabWireContext` rides the AppHost collab-delta feed: `Collab/sync` frames each broadcast delta as a `CollabFrame` (W3C carrier + Loro bytes) and extracts the originating correlation on merge, but the W3C injection/extraction adapter pair is AppHost `TraceContext`'s — AppUi holds only the composition-bound `Inject`/`Extract` delegates; the reciprocal `Rasm.AppHost [COLLAB-WIRE-CONTEXT]` (the `TraceContext` collab-frame adapter and the `COLLAB_DELTA_FEED` frame schema) is the deferred counterpart.
- `[FEED]: ProfileSample` into `Diagnostics/devloop` is the host profile-sample flame join: AppHost owns capture (Pyroscope span profiles, EventPipe CPU stacks) and delivers correlation-keyed samples through the composition-bound `ProfileSampleSource`, which `FlameNode.Of` folds into the frame tree — the feed rides an existing AppHost port (a registration row, never an eighth `PortCardinality` port), and the reciprocal `Rasm.AppHost [PROFILE-FLAME-JOIN]` is the deferred counterpart.

`Diagnostics ⇄ Rasm.AppHost` `[FAULT]` edge is the 6xxx `AppUiFaultBand` neighborhood: AppUi lowers every fault union onto its band and the AppHost lifecycle registry pins the reciprocal range, so fault codes never collide across the platform seam.

## [04]-[BOUNDARIES]

- Bim `ElementSet` queries enter through Bim-owned receipt rows.
- Cost and schedule dashboards consume the Bim `CostSchedule` and `ScheduleNetwork` planning receipts as `Charts/dashboards` feed values.
- Whisper.net owns translate-to-English captioning; broader translation binds through a locale service row.
- Kernel `Analyze` receipt projection enters inspector and dashboard surfaces through the receipt spine.
- `SurfaceHost.RhinoPanel` mounts only when a Rhino lease supplies `EmbedCapsule` and the `Render/pipeline` render-graph GPU lease.

## [05]-[PROHIBITIONS]

Deleted patterns the owner regions foreclose:
- NEVER runtime XAML for production views — the `Surfaces` mount gate rejects runtime loads, so views enter only through the compiled-XAML class.
- NEVER per-host `GpuBackend`/`GRContext` construction in a dispatch arm — Avalonia owns backend selection through `EmbedOptions.RenderingMode`.
- NEVER a per-surface image loader, telemetry sink, or receipt sink — every owner contributes through the one `AppUiTelemetry.Contribute` spine.
- NEVER an `SKSurface` outside the `Offscreen` capsule — the capture capsule owns the one Skia draw boundary.
- NEVER ReactiveUI code-behind view binding — the `BehaviorRail` intent bridge is the single C# binding seam and rejects binder symbols.
- NEVER a second command, hotkey, palette, or conflict registry beside the one `CommandIntent` table — every verb is a derivation fold over it.
- NEVER a parallel control-generation framework — the one `ControlIntent` union through `ControlFactory` materializes every control.
- NEVER a parallel layout framework — constraint layout is the one `LayoutConstraint` algebra solved by one `LayoutSolver` panel.
- NEVER a per-surface virtualizer — the one `VirtualWindow` owner over `DynamicData` change-sets owns every windowed surface.
- NEVER a generic `IReceipt` or ledger abstraction — every receipt stays its typed record sealed through `ReceiptSinkPort`.
- NEVER a fault code outside the `Diagnostics/evidence` registry — every AppUi fault union's `Code` derives through its `AppUiFaultBand` row.
- NEVER a Loro byte as durable truth — the durable stream is the one `EditIntent` union projected onto Persistence-owned `OpLogEntry` rows.
- A Loro snapshot survives only as a content-keyed cold-start accelerator.
- NEVER a second revert vocabulary — `RevertibleOp` folds forward and inverse deltas across the recorder and the durable inverse stream.
- NEVER a second BCF or coordination owner in AppUi — `Rasm.Bim` owns the openBIM semantics; AppUi keeps only the `Viewpoint` board projection.
- NEVER an AppUi-local content-identity mint — every AppUi content hash composes the one kernel `ContentHash.Of` seed-zero entry.
- NEVER a local geodesy, solar-position, clustering, or recompute engine — Bim, Compute, and the AppHost `RecomputeGraph` own those.
- CSP analyzer diagnostics are architecture pressure: fix the shape, refine the rule on a false positive, never suppress.

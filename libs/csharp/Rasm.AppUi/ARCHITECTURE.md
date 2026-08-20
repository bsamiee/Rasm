# [RASM_APPUI_ARCHITECTURE]

`Rasm.AppUi` maps the APP-PLATFORM Avalonia product-UI engine over the settled receipt spine and the GPU render surface: each sub-domain page is a UI capability unit lowering onto the one 6xxx `AppUiFaultBand`. S4 seats it as the consuming leaf: it references the platform peers downward, re-owns none of their capability, and never becomes the composition root.

## [01]-[DOMAIN_MAP]

```text codemap
Rasm.AppUi/
├── Shell/                # Host-mount axis and application shell spine
│   ├── Navigation.cs     # NavRequest union over the ShellRoot router; ShellDockFactory folds DockableRow rows through RegionProgram
│   ├── Screens.cs        # Frozen ScreenCatalog derivation table and the ScreenBase activation scopes with one screen fault fold
│   ├── Hosts.cs          # SurfaceMount union beside the profile-stated host class; one seam record carries the mount
│   ├── Commands.cs       # CommandIntent rows with payload union, the availability algebra seat, CommandDeck derivations
│   ├── Controls.cs       # Closed ControlIntent [Union] where arity, provider, modality, and emphasis ride case columns
│   ├── Solver.cs         # LayoutConstraint equality, inequality, and priority algebra; the Cassowary-backed panel seat
│   ├── Virtualization.cs # VirtualWindow viewport-range realization at constant cost; control-recycling columns
│   ├── Dialogs.cs        # DialogIntent union over per-root Interaction seams; StackOwner binding and Fin-railed results
│   ├── Input.cs          # GesturePolicy chord derivation, behavior trigger and action rows, the frozen PanZoomRow family
│   └── Accessibility.cs  # ScreenCatalogRow automation columns, KeyboardNavigation reachability, the luminance gate fold
├── Render/               # Pure GPU-viewport and temporal plane
│   ├── Pipeline.cs       # Pass-DAG over the leased GRContext; GpuBackend rows carry Target delegates over the GpuBinding union
│   ├── Meshlets.cs       # ResidencyMeshlet descriptor reads; selection owner for bindless residency, prefetch, and instancing
│   ├── PathTrace.cs      # BVH build-and-refit, ReSTIR reservoirs, progressive denoise; LayeredBsdf shading off the SlabStack product
│   ├── Shading.cs        # ShaderAssetCache keyed per GpuBackend beside the byte-budgeted native texture-plane cache
│   ├── Immersive.cs      # OpenXR session machine on the leased Wgpu device; ImmersiveMode carries immersive-versus-flat as a value
│   ├── Reality.cs        # SplatSource and PointCloudSource decodes off the residency carrier; the CapturePass projection
│   ├── Capture.cs        # DrawSource capsule with Fin-railed Use; SKImage materialization and content-hashed encode rows
│   ├── Drafting.cs       # SheetSet templating and Viewport2D framing over the Fabrication hidden-line owner
│   └── Animation.cs      # Track union, Keyframe value-plus-motion-token carrier, Timeline deterministic playhead
├── Charts/               # Chart, dashboard, and geo-basemap projection
│   ├── Dashboards.cs     # Chart series and axis rows with downsampled stream binding and brushing
│   ├── Custom.cs         # CustomVisual frozen layout catalog binding one VisualPayload case per row
│   ├── Basemap.cs        # Mapsui MapControl with BasemapSource tile rows and NTS overlay rows; EditManager Apply verbs
│   ├── Telemetry.cs      # TelemetryBoard row pinning EvidenceFan, frame objectives, store-profile receipts, EvidenceJoin
│   └── Climate.cs        # Declared diagram family lowering onto the chart plane; the polar-split verdict columns
├── Analysis/             # Sealed study results as a scene, a comparison, and one environmental coordinate
│   ├── Layers.cs         # ResultLayer stack with study, input-digest, and run-history columns; ResultDomain legend resolution
│   ├── Compare.cs        # CompareAxis vocabulary; per-cell coordinate-triple binding under the shared channel set
│   └── Context.cs        # AnalysisContext value: site, civil moment, grain, climate scenario; one scrub re-derives the scene
├── Editing/              # Typed-edit surfaces over the model
│   ├── Inspector.cs      # InspectorPolicy admission capsule, ranked EditorFactory rows, the EditFault/EditReceipt commit rail
│   ├── Tables.cs         # TableColumnRow metadata family and the TableProjection fold across flat, tree, and grouped reads
│   ├── Forms.cs          # FormSchema typed sections over PropertyModels; the multi-selection batch-edit seat
│   ├── History.cs        # RevertDelta payloads with structural inverses over CancelableCommandRecorder and the ledger stream
│   ├── LiveData.cs       # DataSource axis with pacing policy, the filter and view-state pair, operator rows, OverlayRank merge
│   └── Graph.cs          # IDrawingNode/DrawingNodeEditor canvas, the QuikGraph cycle gate, LoroTree co-edit structure
├── Document/             # Reproducible document plane
│   ├── Notebook.cs       # NotebookCell kind union with pinned capability fingerprints; code cells carry data only
│   ├── Media.cs          # MarkdownRenderer over typography rows, MediaSurface codec rows, the one Surfaces.Mount crossing
│   ├── Export.cs         # MigraDoc flow composition, PDFsharp policy hardening, OOXML writers, lcmsNET print-fidelity rows
│   ├── Search.cs         # SearchQuery closed request shape and SearchSource coverage rows projecting the landed owners
│   └── Board.cs          # BoardItem placement family: live frames with board-owned crop, stat cards, sheet frames, annotation
├── Collab/               # Live-collaboration plane over the durable Persistence spine
│   ├── Sync.cs           # CollabDoc LoroDoc container forest as live authority; the typed intent stream stays durable truth
│   ├── Issues.cs         # Issue composition of Viewpoint with the BCF topic; CommentLens, IssueRegister columns, IssueTile
│   ├── Tour.cs           # ReviewTour ordered TourStop sequence binding saved Viewpoints with dwell and motion tokens
│   └── Session.cs        # SessionRole rank rows, MembershipState lifecycle axis, MembershipOp verdict transitions
├── Diagnostics/          # Evidence, proof, dev loop, and quality governance
│   ├── Evidence.cs       # EvidenceReceipt case fold into the HLC-stamped sink; scope identity, dimension vocabulary, meter mount
│   ├── Proof.cs          # Capture, check, variant-density, benchmark, and replay cells off live catalogs; CsCheck and Verify seal
│   ├── DevLoop.cs        # Reload knob rows, the attach-config inspector row, HUD sample feed, FlameNode fold, solve scrub
│   └── Governor.cs       # PerfBudget fold over the GovernorState cell; hysteresis steps passes, residency, motion, XR together
├── Vfx/                  # Owned effects plane over the one draw capsule
│   ├── Material.cs       # Layer capsule bracketing draws over a compositor-painted ground; filter-term rows, the sample contract
│   ├── Shader.cs         # Compiled-once SkSL roster, the per-frame uniform frame, path-effect family, byte-ceiling tile cache
│   └── Compose.cs        # Slot vocabulary keyed by compositor property names; keyframe mint under the duration floor, trigger maps
└── Theme/                # Pure vocabulary: tokens, typography, motion, assets, locale
    ├── Tokens.cs         # Appearance seed rows expanded by one pure fold into the TokenKey ladder; variant and density axes
    ├── Typography.cs     # TypographyRole rhythm rungs with the orthogonal TypeAxis; the itemizing shaping rail seat
    ├── Motion.cs         # MotionToken rows carrying one MotionTiming modality and a reduced-motion delegate each
    ├── Assets.cs         # IconRow registry of kernel AssetOrigins; IconSurface composes IconRender with pose and filter chain
    └── Locale.cs         # LocaleRow culture axis and the ResolvedLocale binding; per-script shaping, calendar-bound patterns
```

## [02]-[STRATA]

Member-resolved strata order the interior, and every consumption edge points down.

- S0 hub — `Diagnostics/Evidence` seats lowest; `EvidenceReceipt` NESTS producer receipts as co-ownership, so the fan adds no upward import.
- S0 substrate — every fault union in the package derives through its `AppUiFaultBand` row, so code ranges never collide across sub-domains.
- S0 vocabulary — pure `Theme`: every visual literal traces to a generated `TokenRow`, and the swap re-seeds atomically.
- S1 spines — one owner per fabric; a second command table, window fabric, constraint panel, or render spine is the forked-spine defect.
- S1 law — spines consume S0 alone and never each other, so the rank holds no lateral edge.
- S2 streams — streams fold spine facts into live authorities; `Collab/Sync` is the one concurrent-edit truth every surface reads.
- S3 surfaces — materialization is total: no screen body spells a control outside the factory, so S3 adds consumers and never vocabulary.
- S3 planes — notebook, export, screen, and analysis planes share the rungs; `ResultLayer` seats highest, reading spine, legend, and queue at once.

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
    accDescr: Which lower vocabulary each spine, stream, and surface owner consumes, and the EvidenceReceipt counter-edge the hub nests.
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
    Factory e1@-->|"[IMPORT]: VirtualWindowSpec"| Virtual
    Factory e2@-->|"[IMPORT]: LayoutSolver"| Solver
    Factory e3@-->|"[IMPORT]: BehaviorRail"| Rail
    Factory e4@-->|"[IMPORT]: TokenRow"| Token
    Board e5@-->|"[IMPORT]: EditIntent"| Intent
    Rail e6@-->|"[IMPORT]: CommandIntent"| Command
    Revert e7@-->|"[IMPORT]: CommandIntent"| Command
    Revert e8@-->|"[IMPORT]: EditReceipt"| Inspect
    Intent e9@-->|"[IMPORT]: CommandIntent"| Command
    Command e10@-->|"[IMPORT]: AppUiFaultBand"| Fault
    Graph e11@-->|"[IMPORT]: AppUiFaultBand"| Fault
    Fault e12@-.->|"[COUNTER]: EvidenceReceipt"| Factory
    Fault f1@-->|"forbidden: hub upward"| S3
```

## [03]-[SEAMS]

Seam fences split by counterpart role: the first binds the same-branch AEC peers, the kernel, and the durable store; the second binds the platform host and the TypeScript peers. Each collapsed edge stands for every contract between that owner and that partner at the load-bearing kind; the owning pages enumerate the rest.

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
    accDescr: Which projections, receipts, boundaries, and content keys cross between the AppUi owners and their C# peers.
    subgraph appui[RASM.APPUI]
        Render[Render plane]
        Charts[Chart planes]
        Collab[Collab plane]
        Document[Document plane]
        Theme[Theme vocabulary]
        Diagnostics[Diagnostics]
    end
    Compute{{Rasm.Compute}}
    Fabrication([Rasm.Fabrication])
    Materials([Rasm.Materials])
    Bim([Rasm.Bim])
    Rasm([Rasm])
    Persistence[(Rasm.Persistence)]
    Compute e1@-->|"[PROJECTION]: ResidencyPayload"| Render
    Render e2@<-->|"[SHAPE]: WgpuDevice"| Compute
    Fabrication e3@-->|"[RECEIPT]: HiddenLineResult"| Render
    Materials e4@-->|"[BOUNDARY]: LayeredBsdf + SurfaceShade + EnvironmentLight + TextureSet"| Render
    Rasm e5@-->|"[CONTENT_KEY]: ContentHash"| Render
    Rasm e6@-->|"[SHAPE]: SunPosition"| Render
    Rasm e7@-->|"[PROJECTION]: DrawingProjection + HatchResult"| Render
    Rasm e8@-->|"[WIRE]: SpatialIndex"| Render
    Rasm e9@-->|"[BOUNDARY]: SpringShape"| Theme
    Bim e10@-->|"[SHAPE]: GeoTiles"| Charts
    Bim e11@-->|"[RECEIPT]: CostSchedule"| Charts
    Bim e12@-->|"[RECEIPT]: ScheduleNetwork"| Charts
    Bim e13@-->|"[RECEIPT]: EnergyResults"| Charts
    Persistence e14@-->|"[PROJECTION]: StoreProfileRow"| Charts
    Persistence e15@-->|"[RECEIPT]: ReceiptEnvelope"| Diagnostics
    Bim e16@-->|"[PORT]: IssueBoard"| Collab
    Collab e17@-->|"[PROJECTION]: ReplayWindow"| Persistence
    Collab e18@-->|"[CONTENT_KEY]: SnapshotAccelerator"| Persistence
    Persistence e19@-->|"[WIRE]: DocumentQuery + DocumentHit"| Document
    Bim e20@-->|"[RECEIPT]: ConstructionState"| Render
    Bim e21@-->|"[BOUNDARY]: BcfViewpoint"| Render
    Bim e22@-->|"[SHAPE]: GeoReference"| Render
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
        Document[Document plane]
        Collab[Collab plane]
        Diagnostics[Diagnostics]
    end
    AppHost{{Rasm.AppHost}}
    Rasm([Rasm])
    Core([typescript:core])
    Ui([typescript:ui])
    Shell e1@-->|"[WIRE]: CommandPayloadWire"| Core
    Render e2@-->|"[WIRE]: GeometryResidencyWire"| Core
    Diagnostics e3@-->|"[WIRE]: EvidenceTimelineWire"| Core
    Shell e4@-->|"[WIRE]: ControlIntentWire + CommandGateWire + LayoutConstraintWire"| Ui
    AppHost e5@-->|"[PORT]: DeterminismContext"| Document
    Diagnostics e6@<-->|"[FAULT]: FaultBand"| AppHost
    Rasm e7@-->|"[PORT]: ReceiptSinkPort + InstrumentSpec + Slo"| Diagnostics
    AppHost e8@-->|"[PORT]: HookRail"| Diagnostics
    AppHost e9@-->|"[PORT]: ProfileSampleSource"| Diagnostics
    Collab e10@<-->|"[TRANSPORT]: CollabWireContext"| AppHost
```

- `[BOUNDARY]: LayeredBsdf + SurfaceShade + EnvironmentLight + TextureSet` into `Render` — the appearance edge, VALUES only.
- Materials lowers the layered BSDF and channel closure, presses or ingests the texture-plane set, prefilters the dome, resolves the solar disc.
- `Render/pathtrace` shades and draws the dome and its solar disc; `Render/shading` uploads planes and binds the prefiltered products.
- Appearance semantics stay Materials-side; Render supplies the point, the UV, the mip level, and the device.
- `[PORT]: DeterminismContext` into `Document` — the AppHost runtime port spine composed at app composition; `CapabilityPin` anchors it.
- `[PORT]` into `Diagnostics` — the observability spine; telemetry projects facts, never produces them.
- Owners seal evidence through kernel `ReceiptSinkPort`, declare instruments as `InstrumentSpec` rows, and bind objectives through the SLO algebra.
- AppHost's half of the edge is the `HookRail` receipt point the fan taps.
- Instrument rows contribute through `TelemetryContributorPort` on the `TelemetrySource.AppUi` meter.
- Evidence fan subscribes to the `HookRail` receipt point as one observe row.
- `[CONTENT_KEY]` edges are one idiom — every content-identity mint composes the kernel `ContentHash.Of` seed-zero entry.
- Compute-minted residency and splat keys stay decode-only.
- `[PROJECTION]: ReplayWindow` also serves the Render version-compare lane — values only, AppUi runs no ledger read.
- Persistence `ReplayWindow`/commit-DAG fold derives the `(ElementId, DiffClass)` classification `VersionGhost` renders as `VisibilityOverride` rows.
- `[RECEIPT]: ConstructionState` — `SchedulePlayback.FromSchedule` reads `ConstructionState.At`/`TaskKind` as Bim-owned 4D schedule values.
- `[PROJECTION]: telemetry measure series` into `Charts` — store tiles name a facet coordinate beside its rollup column on the Persistence series.
- Tiles reach the series through one injected read arrow.
- `[RECEIPT]: resident ReceiptEnvelope` into `Diagnostics` — the `EvidenceSource.Resident` arrow hands back message envelopes.
- Correlation join and billing accrual stay one fold over two sources.
- Profiling custody, the pg_stat slots, and the `store.<domain>.<verb>` grammar stay Persistence-side.
- `[TRANSPORT]: CollabWireContext` — `Collab/sync` frames each delta as a `CollabFrame`, W3C carrier and Loro bytes.
- Merge extracts the originating correlation; `CollabCarrier` binds the frame's getter/setter pair onto the AppHost `TraceContext` spine.
- AppHost's reciprocal is landed data: durable deltas ride `Wire/topics` `Topic.Collab` on the outbox leg, awareness frames `Topic.Presence`.
- `[PORT]: ProfileSampleSource` lands at `Diagnostics/devloop`, each sample keyed by correlation.
- Capture stays AppHost-side — Pyroscope span profiles, EventPipe CPU stacks; `FlameNode.Of` folds the samples into the frame tree.
- Feed rides an existing AppHost port row, never a new `PortCardinality` port.
- Samples carry the producer's symbolization posture, so AppUi renders the frames it received and resolves no address itself.

`Diagnostics ⇄ Rasm.AppHost` `[FAULT]` edge is the 6xxx `AppUiFaultBand` neighborhood: AppUi lowers every fault union onto its band and the AppHost lifecycle registry pins the reciprocal range, so fault codes never collide across the platform seam.

## [04]-[INTERNAL]

One mount-then-route spine orders the interior: the mount precedes the shell, and the shell precedes the screens it routes.

```mermaid
---
config:
  layout: elk
  flowchart:
    curve: linear
    padding: 25
---
flowchart LR
    accTitle: Rasm.AppUi mount-to-receipt spine
    accDescr: How a mounted surface routes through shell, screens, and controls onto the draw executor while faults converge on the band.
    Mount(["SurfaceMount"]) e1@--> Route["ShellRoot routing"]
    Route e2@--> Screen["ScreenCatalog activation"]
    Screen e3@--> Control["ControlFactory materialization"]
    Control e4@--> Draw["Vfx draw executor"]
    Draw e5@--> Spine["AppUiTelemetry.Contribute"]
    Spine e6@--> Sink(["ReceiptSinkPort"])
    Route f1@-.->|"route fault"| Band[/AppUiFaultBand/]
    Screen f2@-.->|"screen fault"| Band
    Control f3@-.->|"edit fault"| Band
    Band f4@--> Spine
```

- `Shell` owns the host-mount axis and application spine.
- `Theme` is the pure vocabulary every visual literal traces to, and `Vfx` its executor — drawing what the catalogue declares, owning no constant.
- `Render` owns the GPU viewport and the temporal plane; `Document` the recompute graph and every paginated output.
- `Diagnostics` owns the fault registry, the proof matrix, the telemetry spine, and the quality governor.
- `Analysis` owns what a SEALED study becomes and computes no value of its own — `Rasm.Compute` stays the solver and this plane its reader.
- `Collab/sync` holds the one live-merge authority; typed `EditIntent` is durable truth on the Persistence ledger, and no Loro byte crosses it.

## [05]-[BOUNDARIES]

- Element selection enters as receipts scope-qualified at `Rasm.Bim` `Model/query` or `Rasm.Persistence` `Query/lane`; AppUi runs no query engine.
- Cost and schedule dashboards consume the Bim `CostSchedule` and `ScheduleNetwork` planning receipts as `Charts/dashboards` feed values.
- Caption capture and band rendering belong to `Document/media`; `Theme/locale` owns the caption language and translation policy the capture reads.
- Kernel `Analyze` receipt projection enters inspector and dashboard surfaces through the receipt spine.
- `SurfaceMount.Panel` mounts on an embedded host surface only when a Rhino lease supplies `EmbedCapsule` and the `Render/pipeline` GPU lease.
- `Surfaces` mount gate admits a production view only as its compiled-XAML class, so a runtime XAML load has no mount path.
- Avalonia owns GPU backend selection through `EmbedOptions.RenderingMode`; no dispatch arm constructs a per-host `GpuBackend` or `GRContext`.
- `ONE_WGPU_DEVICE` fixes the wgpu arity this folder mints — one core, one `Wgpu` view, one compositor-matched `Adapter`, `Device`, and `Queue`.
- Every binder — immersive session, shading arm, query-set owner, Compute dispatch — binds that leased pair and holds `nint` handles alone.
- `Offscreen` capsule owns the one Skia draw boundary and every `SKSurface` inside it.
- `DrawSource.Layered` is the one `SaveLayer` site; `Vfx/material` selects its ground arm and supplies its `LayerSpec`.
- `Vfx` EXECUTES the `Theme` material, wash, and motion rows; a value authored on an effects page is a second token source the swap never re-seeds.
- Runtime-SkSL compilation partitions by type domain — 3D appearance at `Render/shading`, 2D chrome at `Vfx/shader`, one cache each.
- `BehaviorRail` intent bridge is the single C# view-binding seam and rejects binder symbols, so ReactiveUI code-behind binding has no seam to enter.
- `AppUiTelemetry.Contribute` is the one spine every owner routes image-load, telemetry, and receipt facts through.
- Every receipt stays a typed record sealed at `ReceiptSinkPort`.
- `CommandIntent` table is the one verb registry — hotkey, palette, and conflict views are derivation folds over it.
- `ControlIntent` union through `ControlFactory` materializes every control.
- `LayoutConstraint` algebra solved by one `LayoutSolver` panel owns every layout.
- `VirtualWindow` over `DynamicData` change-sets owns every windowed surface.
- Every AppUi fault union derives its `Code` through its `AppUiFaultBand` row in the `Diagnostics/evidence` registry.
- Durable truth is the one `EditIntent` union projected onto Persistence-owned `OpLogEntry` rows.
- Loro snapshots survive only as content-keyed cold-start accelerators.
- `RevertibleOp` folds forward and inverse deltas across the recorder and the durable inverse stream.
- `Rasm.Bim` owns openBIM and coordination semantics; AppUi keeps the `Viewpoint` board projection alone.
- Every AppUi content hash composes the one kernel `ContentHash.Of` seed-zero entry, the branch's frozen content-key entry.
- Bim, the `Rasm` kernel, and the AppHost `RecomputeGraph` own geodesy, solar position, clustering, and recompute.
- `Rasm.Materials` owns appearance whole — channel roster, texture-plane storage, decode ladder, set admission, prefiltered dome.
- Render-side appearance machinery has no seam to enter; Render binds the values and holds the device.
- Texture-plane VRAM budgets at `Render/shading` under the byte-ceiling law `Render/meshlets` `ResidencyBudget` holds for geometry VRAM.
- Every analysis value projects a sealed receipt — `Analysis` mounts, probes, compares, and bakes what a study produced, solving nothing itself.
- `Analysis/context` `AnalysisContext` is the ONE environmental coordinate every layer, diagram, sun-position consumer, and bound series reads.
- `Shell/screens` `RunQueueSurface.AdoptIntent` reaches `Analysis/layers` `AnalysisLayers.Adopt`, the one construction site for a sealed study.
- Expressiveness partitions angular rendering at the polar split; `Charts/climate` states each family's verdict beside its structural reason.

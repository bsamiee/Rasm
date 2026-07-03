# [DOSSIER_CENSUS] Rasm.AppUi

READ-ONLY census of `libs/csharp/Rasm.AppUi/.planning/` (37 pages, 5 folders, 8696 LOC) against `docs/stacks/csharp`, `libs/.planning/architecture.md`, the four upstream briefs (Geometry/Persistence/Compute/AppHost), and the sibling seam counterparts. Corpus is genuinely high-interior-quality (dense polymorphic collapse, ROP rails, single-owner spines consistently consumed: `AppUiTelemetry.Contribute`, `ReceiptSinkPort`, `SurfaceSeam`, `VirtualWindow`, `CommandIntent`, `ControlIntent`, `Viewpoint`, `TokenRow`). Defects concentrate at the PERIMETER — fault-band coherence, the Loro/Persistence durable seam, folder architecture, seam-ledger rot — not the interiors.

Anchors: `ARCH` = `libs/csharp/Rasm.AppUi/ARCHITECTURE.md`; `RDR` = `README.md`; briefs by short name. Page paths relative to `.planning/`.

## [00]-[HEADLINE]

The corpus is award-grade INSIDE each owner and structurally defective at the seams. The five campaign-defining rulings: (1) no fault-band registry + massive band collisions (internal, vs AppHost federation registry, vs Persistence); (2) the Loro→Persistence durable seam is designed as the exact form the Persistence brief RECORDED-REJECTS (opaque Loro blob as durable CRDT); (3) `collab.md` is a fully-built ORPHAN (absent codemap/seams/router/cards) and the merge authority for 4 pages; (4) `evidence.md` is a 456-LOC six-concern god-page mis-filed in Render; (5) the seam ledger is stale in both directions (phantom nodes, unwired Geometry/Bim rows, dropped-vocabulary rows, undeclared collab seams). `issues.md` still builds a hand-rolled CRDT `collab.md` explicitly dropped.

## [01]-[PER_PAGE]

Verdict scale: 10 = award, no defect; 8-9 = strong owner, local/perimeter defects; 6-7 = structural defect (orphan/god-page/contradiction); every fault-band code below is a defect subsumed by VC1.

### Shell/ (10 pages)

- `Shell/navigation.md` — 9. Routing spine (`NavRequest` 5-verb union, `ShellRoot`, `ShellDockFactory` over Dock.Avalonia, `LayoutLedger` checkpoint/drain/support/restore). Defect: bare `Error.New` on the `NavRequest.Parse`/`Resolve` rails (`navigation.md:46,81`) — federation SEAM_AND_RAIL_LAW forbids bare `Error.New`; a typed `NavFault` registry case belongs. Charter as-is: correct.
- `Shell/screens.md` — 9. `ScreenCatalog` frozen table + `ScreenBase` activation + validation lift + control-stream body. `ScreenValidation` cross-field `AdmitCross` is excellent. Clean.
- `Shell/hosts.md` — 9. `SurfaceHost` 7-case axis, `SurfaceSeam` delegate columns, one mount transaction, `EmbedCapsule`, `SurfaceScheduler`, `NativeAssets`. Exemplary host-agnostic sourcing. Defect: `SurfaceFault` codes 4100-4104 (`hosts.md:43-47`) collide with AppHost `ConfigError` 4100-4199 (VC1).
- `Shell/commands.md` — 9.5. The one command rail: `CommandIntent` row table, `CommandPayload`/`CommandOutcome` unions, `CommandDeck.Freeze` with `GestureConflicts`, palette `Search`, `Invoke` remote route, `Combine` batch. Award-grade polymorphic collapse (menus/toolbars/hotkeys/palette/deep-link/remote all derivation folds). Clean.
- `Shell/controls.md` — 9. `ControlIntent` 18-case `[Union]` + `ControlFactory` total fold + `BehaviorRail.Intent` single binding bridge + `MaterializePool` recycling. Award-grade. Defect: `ControlFault` 4200-4204 (`controls.md:24-33`) — registry (VC1).
- `Shell/solver.md` — 9. `LayoutConstraint` Kiwi algebra + `LayoutPreset` flex/grid/auto rows + one `LayoutSolver` `Panel` + ordered `LayoutProgramWire`. Excellent. Defect: `LayoutFault` 4400-4403 (`solver.md:57-66`) — registry (VC1).
- `Shell/virtualization.md` — 9. `VirtualWindow<TItem,TKey>` fabric + `ExtentLedger` O(log n) + `StickyProjection` + `HierarchyFlatten` — the one windowing owner every surface consumes. Defect: `VirtualFault` 4300-4303 (`virtualization.md:42-51`) collides with `TourFault` 4300-4302 (VC1 internal).
- `Shell/dialogs.md` — 8.5. `DialogIntent` 6-case union + `DialogTopology` per-surface rows + `ToastGate` suppression + `PickKind`. Clean. Defect: `DialogFault` 4130-4132 (`dialogs.md:33-42`) in AppHost `ConfigError` 41xx (VC1).
- `Shell/input.md` — 9. Hotkey derivation + `BehaviorRail` + pointer/`PanZoomRow` + `DragPayload`/`ClipboardRow` + `InputFabric` device fold + `DeviceDriver` 4-SDK capsules. Very dense, all folding to the one `CommandIntent` table. Defect: `InputDriverFault` 4150-4153 (`input.md:284-293`) in AppHost `ConfigError` 41xx (VC1).
- `Shell/accessibility.md` — 8.5. Automation peers + `SceneAccessNode`/`SpatialCue` 3D-scene a11y + focus law + `ContrastGate` WCAG + `AccessProof` sweep. `SpatialCue` spatial-audio is a real depth. Clean.

### Render/ (9 pages)

- `Render/pipeline.md` — 9. `RenderGraph` pass-DAG + `GpuBackend`/`RenderTargetFactory` + `ResolvePass` ladder + `SimVisual` + `Viewpoint`/`ViewpointCodec` (BCF) + `ResidencyManifest.Mint` (the single WEB_GEOMETRY_RESIDENCY_WIRE mint). Massive (507 LOC) but every concern is a real render owner. Compute-confirmed consumer (COMPUTE-BRIEF:215). Defect: `ViewportFault` 4500-4504 (`pipeline.md:27-36`) collides with AppHost `HopFault` 4500 (VC1).
- `Render/meshlets.md` — 8.5. `MeshletCluster` cluster-LOD + `ResidencyBudget` VRAM/prefetch + `InstanceBuffer`. Clean `MeshSource`-off-`GeometryPayload` boundary. No fault union.
- `Render/pathtrace.md` — 9. `Bvh`/`Reservoir`/`PathTracePass`/`Denoiser` + `BsdfShading` consuming Materials `LayeredBsdf`/`SlabStack`/`SurfaceShade` (no re-derived lobe math). Clean consumer-half seam. Uses `ViewportFault` (shared with pipeline — good).
- `Render/capture.md` — 9. `DrawSource`/`Offscreen` (one Skia-surface owner) + `Thumbnails` + `PreviewRow` + `VisualCodec` (float color-management, `ColorPolicy`/`ToneMap`/ICC) + `VisualExport`/`OfficeExport` (SKDocument PDF/SVG + OOXML). Very dense; the color-management + Office export is award-grade. Clean.
- `Render/drafting.md` — 8.5. `SheetSet`/`SheetSize` + `Viewport2D`/`HiddenLineSeam` (composes Fabrication `PROJECTION_HIDDEN_LINE`) + `Dimension`/`Annotation` GD&T + `DraftEmit` DWG/DXF/PDF/SVG. Defects: `DraftFault` 4600-4603 (`drafting.md:266-274`) collides AppHost `CommandFault` 4600 (VC1); the `Render ← Geometry/Drawing: DrawingProjection` seam it should wire is unwired — it consumes Fabrication, not Geometry (VC7a).
- `Render/reality.md` — 8.5. `SplatSource`/`PointCloudSource` off Compute payloads + `CapturePass` + `MeasureOverlay` + `CaptureClip`. Correctly `[UPSTREAM-BLOCKED]` on the Python SOG/PLY/LAZ two-hop (COMPUTE-BRIEF:217 confirms). Defect: `CaptureFault` 4900-4904 (`reality.md:25-34`) collides with `FormFault` 4900-4903 (VC1 internal).
- `Render/evidence.md` — 7. GOD-PAGE + MIS-PLACED. 456 LOC packing SIX concerns: RECEIPT_UNION, CORRELATION_JOIN, CAPTURE_LANES, HEADLESS_DERIVATION (proof matrix + `ProofLaw`), DEV_LOOP (hot-reload + HUD + flamegraph + solve-scrub + replay-verify + REPL), PERF_BUDGET (adaptive quality governor). Content is high-quality; structure is overloaded and mis-filed (Render = "GPU render surface + viewport projections"; none of these are viewport projections). Split/move pressure: extract to a Diagnostics/ folder (VC5).
- `Render/shading.md` — 8.5. `ShaderAssetCache` per-`GpuBackend` + `ShaderShade` consuming `LayeredBsdf`. Shares ONE_WGPU_DEVICE. Defect: `ShaderFault` 0x4C00-0x4C03 (`shading.md:23-31`) — hex base; registry (VC1).
- `Render/immersive.md` — 8.5. `ImmersiveSession`/`XrFrame`/`XrInput`/`Passthrough` over Silk.NET.OpenXR+FB, shared Wgpu device, flat-fold fallback. Honest macOS-loader-absent degrade. Defect: `ImmersiveFault` 0x4D00-0x4D04 (`immersive.md:24-33`) — hex; registry (VC1).

### Charts/ (2 pages)

- `Charts/dashboards.md` — 9. `ChartSeriesSpec` 15-row + `ChartCanvas` + `ChartAxisKind` + `ChartPolicy` + `ChartStream`/LTTB + `DashboardTile` + `CrossFilter`/`DimensionIndex` (word-aligned bitset) / `PolygonBrush` (ray-cast). The bitmap cross-filter engine is award-grade. `GeoOverlay` incremental land-swap over Persistence `SpatialDiff`. Defect: bare `Error.New` for tile/layout validation (`dashboards.md:296,304`) — typed fault belongs.
- `Charts/custom.md` — 9. `CustomVisual` 14-case union, each a pure layout fold to `SKPath`, materialized through the one `Offscreen` capsule + `VisualCodec`; `ColorSpaceAxis` wide-gamut. Award-grade layout-fold collapse (Bruls squarify, sunburst, hexbin, deck.gl-class geo). Uses `Error.New` for empty-data (acceptable at leaf, but VC1 pressure).

### Editing/ (10 pages)

- `Editing/inspector.md` — 9.5. `InspectorPolicy`/`InspectorSurface` PropertyGrid + 11 ranked `EditorFactory` rows (metadata-lookup, no reflection scan) + `EditFault`/`EditOutcome`/`EditGate` + options-inspector + 3-way `ConflictPane` + `CodePane`. Award-grade. Defect: `EditFault` claims the WHOLE 4700-4799 decade (`inspector.md:147-183`) — swallows NINE AppHost bands (`FeatureFault`/`SolverFault`/`WireFault`/`BusFault`/`LaneFault`/`ReplayFault`/`OrchestrationFault`/`SecretFault`/`PemFault`) (VC1, worst single collision).
- `Editing/tables.md` — 9. `TableColumnRow` metadata family + `TableViewState` + `TableProjection` 5-case + `TableCommit`/`ExportDestination`. Correctly delegates windowing to `Shell/virtualization` (`HierarchyFlatten`/`VirtualWindow`) — zero capability lost. Clean.
- `Editing/notebook.md` — 8.5. `NotebookCell` union + `CapabilityPin` (composes AppHost `DeterminismContext`/`EnvFingerprint`/`RecomputeGraph`) + `DependencyGraph` dirty-recompute + `NotebookCoedit` over `CollabDoc` + `ReplayBundle`. CRDT_COEDIT correctly reconciled to `CollabDoc` (unlike issues). Defects: durable seam via opaque Loro blob (VC2); `NotebookFault` 4800-4803 (`notebook.md:91-100`) collides AppHost `FederationFault` 4800 (VC1).
- `Editing/livedata.md` — 9. `DataSource` 7-case + `PipelineInputs` dynamic streams + `BindingCapsule` (one `ObserveOn`) + `LiveDataOps` aggregation. Award-grade DynamicData collapse; one `Connect` delta fans into charts/tables/tiles. Clean.
- `Editing/forms.md` — 9. `FormSchema`/`FormField` over `ControlFactory` + `WizardFold` + `Selection` over `ICheckedList` + `BatchEdit` via `CommandExecution.Combine`. Clean composition, no minted framework. Defect: `FormFault` 4900-4903 (`forms.md:24-32`) collides `CaptureFault` 4900 (VC1 internal).
- `Editing/history.md` — 9. `RevertibleOp` forward/inverse + `RevertScope` two-arm (client `CancelableCommandRecorder` + durable Persistence `OpLogEntry`/`SyncOpKind`) + `EditHistory`. Durable arm is the ONE_REVERT_VOCABULARY (revertible ops, NOT the Loro CRDT — a distinct legitimate seam). Defect: `HistoryFault` 0x4A00-0x4A03 (`history.md:31-41`) hex base; registry (VC1). Watch: `[DURABLE_INVERSE_STREAM]` must consume Persistence-owned `SyncOpKind` per APPHOST V2 decode-only.
- `Editing/media.md` — 8.5. `MarkdownInlineRenderer` (consumes typography `MarkdownProjection`) + `MediaSurface` codec union (AsyncImageLoader + LibMpv) + `PlaybackTransport`. Clean. Defect: `ContentFault` 0x4B00-0x4B03 (`media.md:23-31`, "4B00 code band") collides `CollabFault` 0x4B00 — BOTH pages literally claim the "4B00 code band" (VC1 internal).
- `Editing/issues.md` — 7. STALE/CONTRADICTORY. `Issue`/`ViewpointCodec.FromBcf`/`IssueBoard` BCF projection is good — but COMMENT_THREAD (`issues.md:117-173`) builds a hand-rolled `CommentOp` `[Union]` + `CommentThread` Apply/Merge CRDT that `collab.md:20` EXPLICITLY DROPS ("the `CommentThread`/`CommentOp` register are DROPPED root-up"; "an issue comment thread is a `map` container keyed by comment GUID"). issues.md references the OLD `notebook.md#CRDT_COEDIT` law, not `collab.md`'s `CollabDoc` — the collab refactor reconciled notebook but not issues (VC4). Defect: `IssueFault` 5000-5003 (`issues.md:24-32`) collides Persistence 5xxx (VC1).
- `Editing/tour.md` — 8. `ReviewTour`/`TourStop`/`TourPlayhead`/`NarrationTrack`/`TourSource`/`WalkthroughTour`. High-quality content, but DUPLICATES `Theme/animation.md`: admits "a tour is, structurally, one camera Track" (`tour.md:74`) yet reimplements its own `Bracket`/`Walk` sampler (`tour.md:106-129`) and `WalkthroughTour.Render` (`tour.md:231-261`) as near-clones of animation's `Track.Sample`/`Walkthrough.Render` (VC6). Defect: `TourFault` 4300-4302 (`tour.md:24-33`) collides `VirtualFault` 4300 (VC1 internal).
- `Editing/collab.md` — 6. ORPHAN + SEAM-VIOLATION + MIS-PLACED. Fully-realized 212-LOC `CollabDoc`/`SyncRail`/`Presence`/`TimeTravel` over LoroCs — the merge authority for notebook/issues/tables/livedata — yet ABSENT from `ARCH` codemap (36 nodes, no collab), `ARCH` seams, `RDR` router (01-36, no collab), IDEAS.md, TASKLOG.md. SYNC_RAIL designs the durable spine as Persistence replaying "the opaque delta blob without re-modeling the CRDT" (`collab.md:105`) — the exact form PERSISTENCE-BRIEF:48,203 RECORDED-REJECTS. Also mis-placed in Editing/ (it is collaboration infrastructure). Content quality high; structure the most defective in the corpus (VC2/VC3). Defect: `CollabFault` 0x4B00-0x4B05 collides `ContentFault` 0x4B00 (VC1).

### Theme/ (6 pages)

- `Theme/tokens.md` — 9.5. `TokenRow` 5-case + `ThemeVariantRow` (8 rows incl. Semi brands) + `DensityRow` + `ThemeCatalog.Resolve` OKLab ramp via Unicolour + `ThemeCell`/`ThemeRail` atomic swap + `Colormap` perceptual. Award-grade; the Unicolour OKLab `mix` delegate law is exemplary. Clean.
- `Theme/typography.md` — 9.5. `TypographyRole` 10-row + `FontChain` per-platform + `ShapingSurface` HarfBuzz + `MarkdownProjection` (7-arm Markdig fold) + `TextMetricsPolicy`. Award-grade; the one shaping rail + role vocabulary is exemplary. Clean.
- `Theme/motion.md` — 9. `MotionToken` 6-row + `MotionApplication` + `PhaseMotion` frozen map + `ReducedMotion` one switch. Clean; conformance sweep asserts map = `ProgressPhase.Items`.
- `Theme/animation.md` — 8. `Track`/`Keyframe`/`Timeline`/`Playhead`/`Scrub`/`Walkthrough` — the full keyframe/timeline/playback RUNTIME ENGINE. High-quality (total `Sample` over construction-guaranteed non-empty tracks, frame-indexed determinism). MIS-PLACED in the Theme vocabulary tier (tokens/typography/motion-tokens/assets/locale are vocabulary; this is a temporal runtime owner consumed by tour/reality/pipeline/simulation) (VC6).
- `Theme/assets.md` — 9. `IconSource` 5-case + rank walk + `SvgPipeline` (retained scene, hit-test, animation) + `RasterAssets` + `AssetCatalog`/`AssetKey`. Clean. Defect: `AssetFault` 4120-4123 (`assets.md:24-32`, "4120 code band") in AppHost `ConfigError` 41xx (VC1).
- `Theme/locale.md` — 9. `LocaleRow` + `LocaleStrings` + `ResolvedLocale` (NodaTime patterns + ICU `MessageFormatter`) + `MirrorPolicy`/`LiveCaption`. CLDR-data-driven plural (no suffix branches). Defects: bare `Error.New` on the resolve rail (`locale.md:172-173`); only English ships (COVERAGE observation — engine is data-driven so add-a-language is a row+resx, ARCHITECTURE sound).

## [02]-[CROSS_CUTTING]

### [FAULT_BAND_INCOHERENCE] (the dominant defect — VC1)

AppUi has NO fault-band registry; every fault union hardcodes `base(detail, NNNN)` literals — directly violating the federation GENERATOR_LAW (APPHOST-BRIEF V1: "every fault union's `Code` derives through the registry row, never a `base(detail, NNNN)` literal"). Full census:

Decimal-literal bands (collide with the AppHost federation registry occupied decades):
- `SurfaceFault` 4100-4104, `AssetFault` 4120-4123, `DialogFault` 4130-4132, `InputDriverFault` 4150-4153 → ALL inside AppHost `ConfigError` 4100-4199.
- `ControlFault` 4200-4204; `VirtualFault` 4300-4303 ≡ `TourFault` 4300-4302 (INTERNAL); `LayoutFault` 4400-4403.
- `ViewportFault` 4500-4504 → AppHost `HopFault` 4500.
- `DraftFault` 4600-4603 → AppHost `CommandFault` 4600.
- `EditFault` 4700-4799 → swallows NINE AppHost bands (`FeatureFault`/`SolverFault`/`WireFault`/`BusFault`/`LaneFault`/`ReplayFault`/`OrchestrationFault`/`SecretFault`/`PemFault`).
- `NotebookFault` 4800-4803 → AppHost `FederationFault` 4800.
- `FormFault` 4900-4903 ≡ `CaptureFault` 4900-4904 (INTERNAL).
- `IssueFault` 5000-5003 → Persistence 5xxx.

Hex-literal bands (a separate high block, base-inconsistent with the decimal ones):
- `HistoryFault` 0x4A00; `CollabFault` 0x4B00 ≡ `ContentFault` 0x4B00 (INTERNAL); `ShaderFault` 0x4C00; `ImmersiveFault` 0x4D00.

Three internal collisions + five AppHost-decade collisions (one swallowing nine bands) + one Persistence collision + decimal/hex base mixing. Every AppUi fault also surfaces in the shared `ReceiptEnvelope`/`EvidenceTimeline` (evidence.md correlates cross-package envelopes) so disjointness is load-bearing.

### [LORO_PERSISTENCE_DURABLE_SEAM] (the scope-mandate collision — VC2)

PERSISTENCE-BRIEF:48 (ROSTER_RECONCILIATION) + :203 (OUT_OF_SCOPE): "Recorded rejection, never re-proposed: `LoroCs` as the Persistence CRDT owner — the cross-runtime bit-parity wire law (`CrdtOpWire` `[Key]` sequence reproduced in Python/TS) forecloses a Rust-binding CRDT whose wire is its own; LoroCs stays App-UI's live-collaboration concern." The AppUi corpus designs the DURABLE spine as Persistence replaying opaque Loro-native deltas — the rejected form — across four pages: `collab.md:105` (SYNC_RAIL: "the local delta projects onto the Persistence `OpLogEntry`... Persistence replays the opaque delta blob without re-modeling the CRDT"), `notebook.md:178` (CRDT_COEDIT over the "Persistence op-log changefeed"), `issues.md:123` (`CommentOp` "projects onto the Persistence `OpLogEntry`"), `collab.md:263` RESEARCH `[TRANSPORT_TOPIC]` (AppHost transport). APPHOST-BRIEF V2 reinforces: Persistence PORT results are decode-only, Persistence-owned types. Correct disposition: LoroCs KEEPS as the LIVE session merge authority (integration-first: 4 consumers), but the durable seam re-charters — CollabSnapshot content-keyed blob persists via the Persistence object/blob lane (opaque blobs are legitimate), while the durable op-log/changefeed/`SyncOpKind` is Persistence-owned `CrdtOpWire` (cross-runtime bit-parity); AppUi transcodes Loro local-update deltas ↔ `CrdtOpWire` ops at the SyncRail boundary and re-hydrates the LoroDoc from `CrdtOpWire` on cold load. Open question for the DECISION: transcription feasibility (Loro eg-walker/Fugue op ↔ `CrdtOpWire` op vocabulary); fallback is snapshot-blob-only durability + `CrdtOpWire`-native op-log with Loro as a live accelerator.

### [SEAM_LEDGER_ROT] (both directions — VC7)

Declared-unwired (in `ARCH` [02]-[SEAMS], no consuming fence):
- `Render ← Rasm/Geometry/Drawing [PROJECTION]: DrawingProjection` (`ARCH:72`) — no fence consumes Geometry DrawingProjection; drafting.md composes the FABRICATION `HiddenLineSeam` (`drafting.md:91-96`), and GEOMETRY-BRIEF:166 confirms Fabrication consumes DrawingProjection — AppUi's producer is Fabrication, not Geometry.
- `Render ← Rasm/Geometry/Processing [PROJECTION]: ChartAtlas / texture UV channel` (`ARCH:73`) — no fence consumes ChartAtlas.
- `Render/query ← Rasm.Bim/Model [PORT]: ElementSet query algebra` (`ARCH:66`) — no page named `query`, no fence consumes Bim ElementSet query.
- `Render/glb → typescript:ui/viewer` (`ARCH:63`) — phantom node name; the residency wire lives in `pipeline.md#TS_PROJECTION`, no `glb` page exists.

Wired-undeclared / stale:
- `collab.md` → Persistence (`CrdtOpWire` durable) and `collab.md` → AppHost (transport Broadcast/presence topic) — entirely undeclared.
- `Editing/notebook ← Rasm.Persistence/Sync: NotebookOp op-log` (`ARCH:70`) — references the DROPPED `NotebookOp` (dropped by `collab.md:20`).
- collab.md is absent from the codemap so all its seams are unrepresented.

Page-level cycles: none detected — the dependency graph is a clean DAG (Theme ← everything; Shell spine ← screens/hosts; Render owners compose pipeline; Editing projects over the receipt/collab/virtualization owners). No intra-AppUi import cycle.

### [DUPLICATION / DEAD CARRIERS / HARDCODING]

- Duplication: `tour.md` `Bracket`/`Walk`/`WalkthroughTour.Render` vs `animation.md` `Track.Sample`/`Walkthrough.Render` (VC6). `issues.md` `CommentThread`/`CommentOp` vs `collab.md` `CollabDoc` map container (VC4).
- Bare `Error.New` on domain rails (federation SEAM_AND_RAIL_LAW breach; pairs with VC1): `navigation.md:46,81`, `locale.md:172-173`, `dashboards.md:296,304`, `custom.md` (per-fold empty-data), `animation.md:62`. These should be typed registry fault cases.
- No dead typed carriers or hardcoded-instead-of-generator rosters detected — rosters are consistently seed DATA (`ChartSeriesSpec` 15 rows, `CustomVisual` 14 cases, `EditorFactory` 11 rows, `TypographyRole` 10 rows, `SheetSize`/`GeometricCharacteristic` tables). This axis is award-grade.
- Concern mixing: `evidence.md` (VC5) and the `Editing/` catch-all (VC9) are the two real cases.

### [UNMINED / UNDERUTILIZED CAPABILITY]

Interiors are well-mined (Kiwi, DynamicData, LiveCharts, Silk.NET WebGPU/OpenXR, LibMpv, PropertyModels, Markdig, Unicolour, SkiaSharp color-management, OpenXML, ACadSharp/netDxf all driven to real depth). No hand-rolled reimplementation of an admitted package found. Upstream capabilities AppUi should compose instead of hand-rolling (VC8): (a) the AppHost federation FaultBand registry (APPHOST V1) — AppUi must consume it, not mint literals; (b) Persistence `CrdtOpWire` as the durable collaboration spine (VC2); (c) the AppHost decode-only PORT contracts for `history.md` durable-inverse and `collab.md` durable (APPHOST V2). Package roster: LoroCs is INTEGRATION-realized (4 consumers) — never a removal candidate; its charter re-scopes per VC2. No REMOVE candidate identified; no missing categorical-best owner found (the roster is complete for the realized surface).

### [FOLDER_ARCHITECTURE VERDICTS]

- Shell/ (10) — coherent host-mount + shell spine; largest folder but a real growth axis. KEEP.
- Render/ (9) — coherent GPU surface EXCEPT `evidence.md` (diagnostics/proof/governor/devloop, not a viewport projection — VC5). Extract evidence to Diagnostics/.
- Charts/ (2) — small but coherent; the `CrossFilter`/`DimensionIndex` bitmap engine (~200 LOC in dashboards) could seed a future Charts/crossfilter split but not yet warranted. KEEP.
- Editing/ (10) — CATCH-ALL: typed-editing (inspector/tables/forms/history) + reactive data (livedata) + document (notebook/media) + coordination (issues) + review (tour) + collab infra (collab). Re-partition (VC9): Editing/ = typed-edit; Document/ = notebook+media; Collab/ (or Review/) = collab+issues+tour+presence.
- Theme/ (6) — coherent vocabulary tier EXCEPT `animation.md` (the timeline/playback runtime engine, not a theme token — VC6). Relocate to a temporal owner.

## [03]-[VERDICT_CANDIDATES]

1. FAULT_BAND_REGISTRY — AppUi mints no fault registry; `base(detail,NNNN)` literals violate federation GENERATOR_LAW; 3 internal collisions (Tour≡Virtual 4300, Form≡Capture 4900, Content≡Collab 0x4B00), 5 AppHost-decade collisions (SurfaceFault/AssetFault/DialogFault/InputDriverFault vs ConfigError 41xx; ViewportFault 4500 vs HopFault; DraftFault 4600 vs CommandFault; EditFault 4700-4799 swallows 9 AppHost bands; NotebookFault 4800 vs FederationFault), 1 Persistence collision (IssueFault 5000), decimal/hex mixing. VERDICT: mint one AppUi `[SmartEnum<int>]` FaultBand registry (duplicate integer fails at type-init, mirroring RASM-COMPONENT-PARADIGM-DECISION.md:141-149 + APPHOST V1), re-partition into a disjoint neighborhood, pin foreign-mirror rows, derive every `Code` through the registry, convert bare `Error.New` rails to typed cases. WATERFALL: surgically edit APPHOST-BRIEF V1 to add a PINNED MIRROR row for the AppUi neighborhood (AppUi named as demanding consumer). Evidence: hosts.md:43, controls.md:24, tour.md:24, virtualization.md:42, reality.md:25, forms.md:24, media.md:23, collab.md:38, issues.md:24, inspector.md:147, pipeline.md:27, drafting.md:266, notebook.md:91, assets.md:24, shading.md:23, immersive.md:24.

2. LORO_DURABLE_SEAM_RECHARTER — the durable spine is designed as Persistence replaying opaque Loro deltas (`collab.md:105`, `notebook.md:178`, `issues.md:123`), the exact form PERSISTENCE-BRIEF:48,203 RECORDED-REJECTS. VERDICT: KEEP LoroCs as the LIVE merge authority (4 consumers, integration-first); re-charter the durable seam — CollabSnapshot blob → Persistence object lane (opaque OK); durable op-log/changefeed/`SyncOpKind` → Persistence-owned `CrdtOpWire`; AppUi transcodes Loro delta ↔ `CrdtOpWire` at the SyncRail boundary, re-hydrates from `CrdtOpWire` on cold load; the opaque-Loro-blob-as-durable-CRDT is the rejected form. Decision must resolve Loro-op ↔ `CrdtOpWire`-op transcription feasibility (fallback: snapshot-blob durability + `CrdtOpWire`-native log, Loro as live accelerator). Evidence: PERSISTENCE-BRIEF:48,203; APPHOST-BRIEF V2; collab.md:96-152, notebook.md:171-210, issues.md:117-173.

3. COLLAB_ORPHAN_REGISTER — `collab.md` (212 LOC, merge authority for 4 pages) is absent from `ARCH` codemap/seams, `RDR` router, IDEAS.md, TASKLOG.md. VERDICT: register collab in codemap ([01]), seam ledger ([02] — with the VC2 seams), README router, and IDEAS/TASKLOG cards; relocate the collaboration cluster to a coherent Collab/ folder (pairs with VC9). Evidence: ARCH:9-52 (36 nodes), RDR:7-42 (01-36), collab.md whole page.

4. ISSUES_CRDT_MIGRATION — `issues.md` COMMENT_THREAD builds a hand-rolled `CommentOp`/`CommentThread` CRDT that `collab.md:20` DROPS root-up; issues references the old notebook law, not `CollabDoc`. VERDICT: rebuild `issues.md` COMMENT_THREAD as a `CollabDoc` `map` container per collab's charter; delete `CommentOp`/`CommentThread`; project `BcfComment` from the Loro map. Evidence: collab.md:20 vs issues.md:117-173 vs notebook.md:171-199 (notebook was reconciled, issues was not).

5. EVIDENCE_GODPAGE_SPLIT — `evidence.md` (456 LOC) packs 6 concerns (RECEIPT_UNION, CORRELATION_JOIN, CAPTURE_LANES, HEADLESS_DERIVATION, DEV_LOOP, PERF_BUDGET) and is mis-filed in Render (none are viewport projections); the Render/viewport 750-LOC god-page precedent (IDEAS `[VIEWPORT_GODPAGE_SPLIT]`) was split. VERDICT: extract to a Diagnostics/ (or Observability/) folder — evidence (receipt+correlation), proof (capture+headless+ProofLaw), governor (PERF_BUDGET), devloop (DEV_LOOP). Evidence: evidence.md (456 LOC, sections [02]-[07]), IDEAS.md:31.

6. ANIMATION_RELOCATE_TOUR_COLLAPSE — `Theme/animation.md` is the timeline/playback runtime engine mis-placed in the Theme vocabulary tier; `Editing/tour.md` duplicates its sampler and walkthrough render despite admitting "a tour is one camera Track". VERDICT: relocate animation.md to a temporal owner (Motion/ or Render/-adjacent); collapse tour into a projection over animation (tour → camera Track; WalkthroughTour → animation.Walkthrough), deleting the duplicated `Bracket`/`Walk`/`WalkthroughTour.Render`. Evidence: animation.md whole page, tour.md:74,106-129,231-261.

7. SEAM_LEDGER_RECONCILE — `ARCH` [02]-[SEAMS] is stale both directions: unwired `Render ← Geometry/Drawing:DrawingProjection` (producer is Fabrication), `Render ← Geometry/Processing:ChartAtlas`, `Render/query ← Bim/Model:ElementSet` (phantom page); phantom `Render/glb` node; stale `Editing/notebook ← Persistence/Sync:NotebookOp` (dropped vocabulary); undeclared collab→Persistence/AppHost seams. VERDICT: retire phantom/stale rows, re-home DrawingProjection to Fabrication, add the collab seams, register collab in the codemap. Evidence: ARCH:59-84, drafting.md:91-96, GEOMETRY-BRIEF:166, notebook.md:171-199, collab.md:96-263.

8. UPSTREAM_CONSUMPTION_THREAD — capabilities AppUi should consume vs assumptions upstream invalidates. Consume: AppHost federation FaultBand registry (VC1); Persistence `CrdtOpWire` durable spine (VC2); AppHost decode-only PORT contracts for history.md/collab.md durable seams (APPHOST V2). Preserve (already correct): notebook.md composes AppHost `DeterminismContext`/`EnvFingerprint`/`RecomputeGraph` as settled vocabulary (notebook.md:206-210); pipeline.md `ResidencyManifest.Mint` off Compute `ResidencyPayload` (COMPUTE-BRIEF:215 confirms); reality.md gaussian-splat `[UPSTREAM-BLOCKED]` (COMPUTE-BRIEF:217 confirms). Migration pressure: retire the Geometry/Drawing seam (GEOMETRY-BRIEF:13,166 rebuilds Drawing/view HLR ground-up; Fabrication is AppUi's producer). Evidence: the four briefs' cited lines.

9. EDITING_REPARTITION — Editing/ (10 pages) is a grab-bag mixing typed-editing, data, document, coordination, review, and collab infra. VERDICT: Editing/ = inspector+tables+forms+history (typed-edit); Document/ = notebook+media; Collab/ (or Review/) = collab+issues+tour+presence. Pairs with VC3/VC6. Evidence: `ARCH:35-52` codemap Editing folder, page charters.

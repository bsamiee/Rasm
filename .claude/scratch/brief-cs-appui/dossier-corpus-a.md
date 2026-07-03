# DOSSIER — Rasm.AppUi corpus-a (Charts, Editing, Render + package governance)

Scope read fully: `.planning/Charts/{dashboards,custom}.md`; `.planning/Editing/{collab,notebook,issues,inspector,livedata,tables,history,forms,media,tour}.md`; `.planning/Render/{pipeline,drafting,capture,evidence,meshlets,pathtrace,shading,reality,immersive}.md`; `ARCHITECTURE.md`, `README.md`, `IDEAS.md`, `TASKLOG.md`; `libs/.planning/architecture.md`; brief seams (`RASM-CS-PERSISTENCE-BRIEF.md:41,203`, `RASM-CS-APPHOST-BRIEF.md:24,57,153`); `.api/{api-livecharts,api-loro}.md`. No root-level `.planning/` pages exist (all pages live under the 5 subfolders). Corpus-a = first half of the alphabetical subfolder set (Charts, Editing, Render); Shell + Theme are corpus-b.

Package posture: IDEAS.md OPEN is EMPTY; TASKLOG.md carries one BLOCKED probe (EMBED-PROBE) and everything else COMPLETE. The corpus presents as "sealed/done" — a primary illusory-depth signal this survey attacks. Overall the fences are genuinely strong (dense ADT, LanguageExt rails, real capability); the defects are structural (seam-correctness, fault-registry, folder placement, a few naive algorithm stubs behind confident prose), not surface naivety.

---

## PER-PAGE VERDICTS

### Charts/

**dashboards.md — 7/10.** LiveCharts rail: `ChartSeriesSpec` 15-row SmartEnum → 4 `ChartCanvas` rows, `ChartAxisKind` 5 scales, `ChartPolicy`, `ChartStream` + LTTB fold, `DashboardTile` union, `CrossFilter` with `DimensionIndex` bitmap index + `PolygonBrush` ray-cast, `BoardState` dock-serializer persistence. Real density (DimensionIndex/CrossFilter/LTTB are award-grade).
- DEFECT (page-craft, severe): `[02]-[SERIES_TABLE]` Boundary is ONE run-on paragraph (`dashboards.md:20`, ~60 lines, 15+ semicolon clauses). Violates comment-discipline; the prose describes behavior (GeoOverlay swap, heat-series mutation) the fence doesn't encode — inverted prose-vs-fence ratio.
- DEFECT (unverified surface): TWO research items (`:473-474` GEO_PAYLOAD, GEO_OVERLAY_DELTAS) leave the entire geo heat-series binding (`DrawnMap`, land-record shape, heat-ramp mutation, layer-load) unverified. `.api/api-livecharts.md:55-67` confirms `Xaml*Series` names are REAL (not phantom), so this is honest gating, not fabrication — but the geo path is the page's thinnest leg.
- Owner charter (as-is, sound): one LiveCharts projection surface over DynamicData change-sets; paints/motion/labels as token keys; capture/export consumed. KEEP.

**custom.md — 8/10.** `CustomVisual` 14-case Skia layout-algebra union (sankey…terrain), each a pure `Func<CustomVisualData,SKImageInfo,Fin<SKPath>>` fold materialized through `DrawSource.Owned`; `ColorSpaceAxis` 4-row wide-gamut vocabulary. The 14 layout folds (squarify/Bruls, sunburst arcs, hexbin, equirect Project) are real, dense, correct-shaped.
- DEFECT (dual owner → see VC7): `ColorSpaceAxis` (4 rows, `custom.md:449`, declared "single suite-wide gamut vocabulary") is paralleled by capture.md `ColorPolicy` (6 rows). Claimed projection, divergent row sets.
- DEFECT (page-craft): `[02]` Boundary run-on (`custom.md:19`, ~40 lines).
- Owner charter (sound): the bespoke-Skia rail LiveCharts structurally cannot supply; render-twin per cell. KEEP.

### Editing/

**collab.md — 6/10 (structurally pivotal; see VC1).** Central LoroCs owner: `CollabDoc`/`LoroDoc` merge authority, `CollabContainer` 6-kind axis, `SyncRail`, `Presence`/`EphemeralStore`/`Awareness`, `TimeTravel`/`UndoManager`. `.api/api-loro.md` confirms every LoroCs member (SubscribeLocalUpdate, EphemeralStore, ForkAt, RevertTo, ExportMode) — NOT phantom. Charters LoroCs as live merge authority and DROPS bespoke NotebookCrdt/CommentThread (`collab.md:20`).
- DEFECT (top, mandate-1): the DURABLE seam persists Loro's OWN opaque binary as the durable payload — `collab.md:105` "Persistence replays the opaque delta blob without re-modeling the CRDT", `:117-120` `CollabSnapshot` content-addresses the raw Loro blob, `:105` "crosses the Persistence blob lane as a versioned opaque payload". This makes LoroCs a SECOND durable CRDT owner, foreclosed by `RASM-CS-PERSISTENCE-BRIEF.md:41` (CrdtOpWire frozen MessagePack wire) + `:203` ("LoroCs is rejected for this package (wire bit-parity law)"). history.md proves the correct discipline (typed `RevertibleOp` JsonElement deltas → `SyncOpKind`), which collab.md violates.
- DEFECT (page-craft): `[02]` Boundary run-on (`collab.md:20`).
- Owner charter (SHOULD be): LoroCs = LIVE in-session convergence + presence + character-granular text/movable-list/time-travel ONLY. LIVE peer broadcast via Loro delta stays (fine). DURABLE seam must project EDIT-INTENTS → Persistence CrdtOpWire (never opaque Loro binary); load = CrdtOpWire replay → fresh Loro doc. Removal (option c) only if the edit-intent mapping proves lossy.

**notebook.md — 8/10.** `NotebookCell` 6-case union with `CapabilityPin` (composes AppHost `DeterminismContext`/`EnvFingerprint`/`DeterminismKernel.Reproduces`), `DependencyGraph` DAG (topo+dirty+recompute), `NotebookCoedit` over CollabDoc, `ReplayBundle`/`NotebookReplay` bit-identity. AppHost determinism seam is real (`RASM-CS-APPHOST-BRIEF.md:24` reproducibility kernel).
- DEFECT (concern duplication → VC9): `DependencyGraph` (`notebook.md:115-168`) implements a FULL local Topo/Reachable/Recompute incremental engine while `:210` claims it "aligns to the `#RECOMPUTE_GRAPH` `RecomputeGraph`… a second incremental-recompute owner is the deleted form." It IS a second owner. Either compose AppHost `RecomputeGraph` or the one-owner claim is false.
- DEFECT (inherits collab durable seam): `:176,178` co-edit "sync transports through the CollabDoc SyncRail over the Persistence op-log" — inherits the opaque-Loro-blob defect (VC1).
- NIT: fully-qualified `Rasm.AppHost.Determinism.*` repeated ~15× in fences (`:221-266`) — seam-clarity vs noise.
- Owner charter (sound minus the RecomputeGraph collision): reproducible computational document composing Compute + AppHost determinism + CollabDoc.

**issues.md — 5/10 (stale rail; see VC2).** openBIM board: `Issue`/`IssueBinding`/`IssueBoard` composing `Rasm.Bim.Coordination.BcfTopic` via `ViewpointCodec.FromBcf`, lossless `ToTopic` round-trip. The BCF projection + carried-source-row preservation (`:54-95`) is excellent.
- DEFECT (top, direct contradiction): `[03]-[COMMENT_THREAD]` (`issues.md:116-173`) is a FULL bespoke HLC last-writer-wins CRDT — `CommentOp` [Union] Add/Edit/Resolve, `Apply`/`Merge`, `HlcStamp` register, its own `Log`. collab.md:20 explicitly declares "the `CommentThread`/`CommentOp` register are DROPPED root-up" and collab.md:17 says "an issue comment thread is a `map` container keyed by comment GUID." issues.md was never re-pointed onto CollabDoc after collab.md absorbed it. TASKLOG marks ISSUES_RELOCATE_TO_BIM COMPLETE but the CRDT-collapse never propagated.
- Owner charter (SHOULD be): board = pure PROJECTION over Bim BCF contract + a `CollabDoc` `map` container for comments. Delete CommentThread/CommentOp/HlcStamp register.

**inspector.md — 8/10.** `EditorFactory` 11-row ranked SmartEnum with total shape match, `EditGate` applicative `Validation<EditFault,T>` rail, `EditFault`/`EditOutcome`/`EditReceipt`, `OptionsInspector`, `ConflictPane` three-way, `CodePane` TextMate capsule. Dense, real PropertyGrid mining (Metadata.Keyed split, AbstractCellEditFactory).
- DEFECT (naive algorithm → VC8): `ThreeWay.Diff` (`inspector.md:360-372`) `Zip(Lines(base),Lines(local),Lines(remote))` compares lines by ORDINAL INDEX — a positional zip, not LCS/diff3. Any insert/delete shifts all subsequent lines and flags them all conflicted, directly contradicting `:299` "a two-way diff that flags every divergence is the deleted form." Naive merge on the conflict-resolution critical path.
- DEFECT (unverified): 4 research items (`:425-428` NAME_BLOCK_ARGS, RECORD_DRAFT, CODE_FOLDING, CODE_ASSIST) — heavy PropertyGrid/AvaloniaEdit host surface deferred.
- Owner charter (sound): typed inspection/edit rail; conflict projection over Persistence conflict receipts (no re-modeling).

**livedata.md — 8/10.** `DataSource` 7-case union over one keyed `SourceCache`, operator-row table (14 rows), `BindingCapsule` single-`ObserveOn` edge, `LiveDataOps` aggregation via C#14 `extension`. Clean DynamicData mining; classified-exclusion consumes AppHost `DataClassification`.
- NIT: `Admit` snapshot path returns `Disposable.Empty` (`:78-84`) — one-shot load not cancellable (acceptable).
- Owner charter (sound): the one change-set spine every screen/chart/table/aggregation folds off. KEEP.

**tables.md — 8/10.** `TableColumnRow` metadata family (drives columns/filter/group/edit/export), `TableProjection` 5-case union DELEGATING to Shell/virtualization `HierarchyFlatten`+`VirtualWindow` (correct per-surface-virtualizer prohibition compliance), `TableViewState` snapshot, `TableCommit`/`ExportDestination`. C#14 extensions throughout.
- DEFECT (naive export): `Delimited` (`tables.md:234-237`) raw `string.Join(delimiter,...)` with no quoting/escaping — a cell containing the delimiter or newline breaks CSV/TSV. Minor correctness gap.
- Owner charter (sound): one grid rail delegating windowing to the shared fabric; exemplary consumption of Shell/virtualization.

**history.md — 8/10.** `RevertibleOp` forward/inverse (JsonElement deltas), `RevertScope` two-arm (client `CancelableCommandRecorder` + durable Persistence `OpLogEntry`), `EditHistory` sealing `EditOutcome.Reverted`. Pairs with AppHost `[ONE_REVERT_VOCABULARY]`. This page MODELS the correct Persistence seam (typed deltas → `SyncOpKind`), the exact discipline collab.md violates.
- NIT: `:66` claims RevertibleOp "projects onto Persistence OpLogEntry exactly as the notebook/comment ops do" — conflates linear-revert ops with CRDT-convergence ops; harmless here since RevertibleOp IS transcribable.
- Owner charter (sound): one inverse algebra spanning client + durable arms. KEEP.

**forms.md — 8/10.** `FormSchema`/`FormField` over `ControlFactory`, `WizardFold`, `Selection` over admitted `ICheckedList`, `BatchEdit` via `CommandExecution.Combine`. Zero-new-package declarative forms.
- DEFECT (fault collision → VC3): `FormFault` band 4900 (`forms.md:29`) COLLIDES with reality.md `CaptureFault` band 4900 (`reality.md:30`).
- NIT: `(fun(()=>write)(),this).Item2` void-sequencing idiom (`:115-118`) ugly but functional.
- Owner charter (sound): schema-driven forms over the one control vocabulary.

**media.md — 7/10.** `MarkdownInlineRenderer` consuming Theme/typography rows, `MediaSurface` union (Image via AsyncImageLoader single-cache / Video-Audio via LibMpv OpenGL), `PlaybackTransport` over `MpvContext`.
- DEFECT (fault collision → VC3): `ContentFault` band 0x4B00 (`media.md:28`) COLLIDES with collab.md `CollabFault` 0x4B00 (`collab.md:43`).
- DEFECT (stub mount): `Mount` Video+Audio arms (`:100-101`) both return identical `new MpvView{Renderer=OpenGl}` ignoring `seam`, Source, AutoPlay, Loop — mount doesn't wire the surface (partly gated by MPV_SURFACE_MOUNT).
- Owner charter (sound): one content vocabulary; no second image cache / markdown model.

**tour.md — 8/10.** `ReviewTour`/`TourStop` [ComplexValueObject], `TourPlayhead` bracketing sampler over animation `Playhead` (frame-indexed, no drift), `NarrationShaper` over typography, `TourSource` union (SavedSequence/TopicTour), `WalkthroughTour` offline render. Model composition of Theme/animation+motion+typography, Render/capture+pipeline Viewpoint, Bim BCF. Reuses `ViewpointCodec.FromBcf` consistently with issues.md.
- MOVE pressure (→ VC5): this is a REVIEW/presentation deliverable (walkthrough render, viewpoint tours), not an "editing" surface — misfiled in Editing/.
- Owner charter (sound): projection over settled owners; no second camera/stopwatch/raster/BCF.

### Render/

**pipeline.md — 8.5/10.** `RenderPass` union pass-DAG, `GpuBackend` 6-row `RenderTargetFactory` column, `ResolvePass` TAA/FSR/SMAA/MSAA ladder + `ResolvePolicy` tier table + Halton jitter, `SimVisual` 7-case, full `Viewpoint`↔`BcfViewpoint` codec (`:332-399`), TS_PROJECTION residency wire (`ResidencyManifest.Mint` single-mint). Award-grade. 3 honestly SPIKE-gated research items (VIEWPORT_GPU, WGPU_BACKEND, WEB_RESIDENCY).
- DEFECT (fault-convention → VC3): `ViewportFault` band 4500 (decimal) — part of the package-wide decimal/hex band incoherence.
- OWNERSHIP note (→ VC6): TS_PROJECTION (`:404,547-556`) consumes Compute-built `ResidencyMeshlet` clusters ("meshopt-built"), but meshlets.md re-builds meshlets locally from raw MeshSource — meshlet-build owner ambiguous.
- Owner charter (sound): frame pass-DAG + backend factory + resolve ladder + viewpoint receipt; composes meshlets/pathtrace.

**drafting.md — 8.5/10 (Fabrication seam; mandate-2).** `SheetSize`/`TitleBlock` locale-aware, `Viewport2D`/`HiddenLineSeam` composing Fabrication `PROJECTION_HIDDEN_LINE` BSP solver, `Dimension`/`Annotation`/`GdtFrame` GD&T, `DraftEmit` PDF/SVG/DWG/DXF. Realizes mandate-2 correctly: painter depth-sort DROPPED root-up (`:96,418`), consumes `(Visible,Hidden,Silhouette)` Edge3 sets, silhouette forward-ready (CSG_SILHOUETTE ripple).
- SEAM tension (→ VC4-adjacent): DWG write uses ACadSharp `DwgWriter` (`:342-360`) while mandate frames "Rasm.Bim owns READ via ACadSharp, Fabrication consumes the read seam only" and scopes AppUi write "over netDxf." netDxf is DXF-only (cannot write DWG), so ACadSharp-for-DWG-write is technically necessary and IS in AppUi's admitted roster (README [DRAFTING_EXPORT]). Disjoint concerns (Bim reads DWG, AppUi writes DWG via same package) — architecturally fine but the seam ledger must NAME it; the mandate phrasing "WRITE leg over netDxf" is internally imprecise (netDxf can't write DWG).
- DEFECT (fault-convention): `DraftFault` band 4600 (decimal). Verified detail: DRAFT_ENTITY (`:417`) correctly flags ACadSharp has NO `LineType.Dashed` static (only Continuous/ByLayer/ByBlock) and constructs a named `LineType("DASHED")` — careful, not phantom.
- Owner charter (sound): 2D documentation composing the single Fabrication HLR owner + one CAD writer surface.

**capture.md — 8.5/10 (over-scoped; see VC5).** `DrawSource` capsule (Borrowed/Owned), FX row table, `VisualRuntime`, `VisualCodec` with `ColorPolicy` (sRGB/P3/Rec2020/scRGB/HDR-PQ) + `ToneMap` SmartEnum (ACES/Reinhard/Hable) + ICC, `FlowBlock` pagination/`FlowFold`, `OfficeExport` (XLSX/DOCX/PPTX via OpenXML). Enormous real capability (float-end-to-end color management is award-grade).
- DEFECT (5-concern conflation → VC5): one "visuals offscreen" page owns offscreen-raster + codec + color-management/ICC/HDR + document-pagination + Office-OOXML export. OfficeExport (3 formats, part-graph writers, `:367-462`) is a substantial distinct concern bolted on.
- DEFECT (dual color vocabulary → VC7): `ColorPolicy` (6 rows, `:190-196`) vs custom.md `ColorSpaceAxis` (4 rows). Claimed projection (`:165`), divergent.
- DEFECT (page-craft): ENCODE_IDENTITY Boundary run-on (`capture.md:165`, ~40 lines).
- Owner charter (SHOULD split): draw-capsule+codec+color stays; DOCUMENT_EXPORT+OfficeExport → own page (`Render/export` or a Documents/ owner).

**evidence.md — 8.5/10 (mis-filed + 4-concern; see VC5).** `EvidenceReceipt` 7-case union + HLC envelope, `EvidenceJoin`/`SkewBand` correlation, `CaptureRow`/`ProofEngine`/`ProofLaw` headless proof matrix (CsCheck+Verify), `DevLoop` (hot-reload/HUD/flamegraph/solve-scrub/REPL), `PerfBudget` quality governor + GPU timestamp profiling (`GpuTimeline.Migrate` deepens pipeline's FrameReceipt). Exceptional density.
- DEFECT (mis-filed + over-scoped → VC5): this is a package-wide DIAGNOSTICS rail, not a Render concern. 7 sections span evidence-union + correlation + visual-regression-proof + headless-test-derivation + dev-tooling(REPL/hot-reload) + adaptive-quality-governor. Belongs in its own Diagnostics/ folder; DEV_LOOP + PERF_BUDGET are split candidates.
- DEFECT (page-craft): DEV_LOOP Boundary run-on (`evidence.md:278`, ~50 lines).
- Owner charter: correct that evidence folds ALL sibling receipts into one union (`AppUiTelemetry.Contribute` single spine) — but the folder/page boundary is wrong.

**meshlets.md — 6.5/10 (naive build stub; see VC6).** `MeshletCluster`/`ClusterCull`/`BindlessTable`, `ResidencyBudget` LRU + velocity-prefetch + `InstanceBuffer`. Residency manager is real and correct.
- DEFECT (top, prose-vs-fence + naive): `Partition` (`meshlets.md:71-81`) assigns EVERY meshlet the SAME `mesh.Bounds` (`:78`), a never-cull `NormalCone(0,0,1,-1)` (`:79`), and whole-mesh `ScreenSpaceError=mesh.Bounds.Radius` (`:80`) — so `Visible` cull (`:67-69`) is NON-FUNCTIONAL (every meshlet passes). Prose (`:14`) claims "computes each meshlet's bounding sphere and normal cone… edge-collapse simplification bottom-up LOD tree" — the fence does NONE of it. The page's entire value proposition (GPU-driven cull) can't work as fenced.
- DEFECT (ownership → VC6): builds meshlets locally from raw MeshSource while pipeline TS_PROJECTION consumes Compute-built `ResidencyMeshlet`. No `meshoptimizer` admitted. Decide owner: Compute (meshopt, AppUi consumes clusters) OR AppUi (admit meshoptimizer).
- Owner charter (SHOULD be): consume Compute-built meshlet clusters + own the VRAM residency/prefetch/instancing (which it does well); stop re-building meshlets naively.

**pathtrace.md — 7.5/10 (stub BVH behind "settled" claim; see VC6).** `Bvh` (SAH), `Reservoir` (ReSTIR), `PathTracePass`, `Denoiser`, `BsdfShading` consuming Materials `LayeredBsdf`/`SlabStack`/`SurfaceShade`. The Materials seam consumption (`:98-108`, no Render-side lobe math) is exactly right.
- DEFECT (stub vs "settled"): `Bvh.Split` (`:33-36`) NEVER recurses (adds one node, references children indices never built); `Enclose` (`:38-40`) sums centers (not an enclosing sphere); `Accumulate` (`:64-67`) returns `Accumulated+sampleBudget` with zero ray tracing. Research (`:125`) calls the CPU reference tracer "settled as the correctness oracle" — the fence can't build a tree. Prose-vs-fence gap (less severe than meshlets: honestly a reference stub, but claimed settled).
- Owner charter (sound): consumer end of Materials appearance seam; own BVH/ReSTIR/accumulate/denoise — needs a real recursive BVH build.

**shading.md — 7.5/10.** `ShaderAssetCache` per-`GpuBackend` (SKRuntimeEffect / wgpu pipeline-state), `ShaderShade` consuming `LayeredBsdf`→`ShadeUniforms`, shares ONE_WGPU_DEVICE. Clean, well-gated (SHADER_COMPILE, BSDF_SHADE_SEAM).
- DEFECT (fault-convention): `ShaderFault` band 0x4C00 (hex).
- NIT: `BindShade` (`:106-109`) only checks uniform count > 0 — a placeholder for the real uniform bind (gated under SHADER_COMPILE).
- Owner charter (sound): one shader-asset cache; one BSDF, two evaluators (CPU pathtrace + GPU shade).

**reality.md — 7.5/10.** `SplatSource`/`PointCloudSource` decoded from Compute `SplatPayload`/`PointPayload`, `CapturePass` union → viewport RenderPass, `MeasureOverlay` bound to Viewpoint, `CaptureClip` on animation playhead. Good composition; honest CAPTURE_DECODE [UPSTREAM-BLOCKED] on the Python scan-decode two-hop.
- DEFECT (fault collision → VC3): `CaptureFault` band 4900 (`reality.md:30`) COLLIDES with forms.md `FormFault` 4900.
- DEFECT (dead carrier + prose-vs-fence): `SplatSort` SmartEnum (RadixDepth/RadixTile, `:54-58`) declared but `Sorted` (`:72-78`) ignores it and does `OrderByDescending` — a comparison sort, though prose (`:18`) claims "radix-sorts." `MemoryMarshal.Cast<byte,SplatEllipsoid>` (`:81,131`) assumes exact Compute-payload layout (gated CAPTURE_PAYLOAD).
- Owner charter (sound): consume-at-wire scan geometry; residency shared with meshlets; measurement on Viewpoint.

**immersive.md — 8/10.** OpenXR `ImmersiveSession`/`XrFrame`/`XrInput`/`Passthrough` over Silk.NET.OpenXR+FB, shares ONE_WGPU_DEVICE, predicted-display-time loop, flat-fold fallback (macOS has no loader — honest). Cleanly scoped, well SPIKE-gated (XR_SESSION_GRAPHICS, FB_PASSTHROUGH).
- DEFECT (fault-convention): `ImmersiveFault` band 0x4D00 (hex).
- Owner charter (sound): optional immersive surface degrading to flat; no second GPU device/scene.

---

## CROSS-CUTTING

### Fault-band registry (systemic — VC3)
NO single fault-band allocation owner exists. Bands are minted ad-hoc per page with a DECIMAL/HEX convention split and ≥2 literal collisions:
- Decimal band: ControlFault 4200, TourFault 4300, ViewportFault 4500, DraftFault 4600, EditFault 4700, NotebookFault 4800, **FormFault 4900**, **CaptureFault 4900 (COLLISION)**, IssueFault 5000.
- Hex band: HistoryFault 0x4A00, **CollabFault 0x4B00**, **ContentFault 0x4B00 (COLLISION)**, ShaderFault 0x4C00, ImmersiveFault 0x4D00.
- If a contiguous 4200→4D00 hex ladder was intended, every decimal band (4500/4700/4900…) is mis-encoded; regardless, `0x4A00`=18944 ≠ decimal-adjacent, so the two schemes are semantically incoherent. `RASM-CS-APPHOST-BRIEF.md:57,153` mandates a type-enforced `FaultBand` `[SmartEnum<int>]` registry with pinned mirror rows + disjointness. AppUi must own a single fault-band allocation table mirroring into AppHost's registry; re-partition the 2 collisions.

### Duplication / concern-mixing
- **LoroCs opaque-blob durable persistence** = second durable CRDT owner vs CrdtOpWire (VC1).
- **issues.md CommentThread/CommentOp** = bespoke CRDT collab.md declared DROPPED (VC2).
- **notebook.md DependencyGraph** = local incremental-recompute engine vs claimed-one AppHost RecomputeGraph (VC9).
- **Color-space vocabulary** split: custom.md ColorSpaceAxis (4) vs capture.md ColorPolicy (6) (VC7).
- **Meshlet build**: local naive Partition vs Compute-built ResidencyMeshlet (VC6).

### Hardcoding-vs-generator
Mostly parameterized well (sheet sizes/page sizes/feed rows/dashboard rows are DATA rows). Minor: netDxf `AutoCad2018` hardcoded (`drafting.md:363`); ProjectionBasis presets hardcoded (acceptable — standard views are data).

### Dead / underused typed carriers
- `SplatSort` SmartEnum declared, ignored by `Sorted` (`reality.md:54,72`).
- `BindlessTable`/`NormalCone`/`ScreenSpaceError` on Meshlet are populated with degenerate constants (`meshlets.md:79-80`) — carriers exist, values are stubs.

### Unwired seams / unmined capability (integration mandates — VC4)
Three ADMITTED packages have ZERO page consumers across the ENTIRE `.planning/` (grep-verified):
- **PDFsharp + PDFsharp-MigraDoc** — PDF is done via SkiaSharp `SKDocument.CreatePdf` (tour/capture/drafting). README [DRAFTING_EXPORT] admits them for "the structured vector-PDF page model and auto-paginated flow-report DOM the OOXML/DXF/raster export set lacked." Integration mandate (MigraDoc flow-report DOM) OR proven-redundant-and-remove (capture.md FlowBlock/FlowFold appears to subsume MigraDoc pagination — must be argued, not assumed).
- **Mapsui.Avalonia12** — geo is done via LiveCharts `GeoMap`/`DrawnMap` (dashboards.md). `ARCHITECTURE.md:67` DECLARES the Basemap seam ("Basemap draws Bim-owned NetTopologySuite as map overlays beside the Wgpu viewport") but NO page realizes it. Unwired seam + unconsumed package → integration mandate (a Basemap page/row) OR redundancy vs LiveCharts GeoMap.
- **NodeEditorAvalonia** — README promises "the node/pin/connector graph-editing canvas for the Shell/Editing parametric and dependency-graph surfaces." No page consumes INode/IConnector (notebook DependencyGraph is a DATA DAG, not a visual editor). Integration mandate: the promised parametric/dependency-graph editing surface has no page — a genuine COVERAGE gap.
- **meshoptimizer** NOT admitted despite EXT_meshopt_compression / meshlet-LOD being central (pipeline TS_PROJECTION); the meshlet-build owner decision (VC6) determines whether AppUi needs it.

### Folder-architecture verdicts (VC5)
- **evidence.md misfiled under Render/**: it is a package-wide DIAGNOSTICS rail (evidence-union + correlation + proof-harness + dev-loop + perf-governor) that folds ALL sibling receipts. Should be its own `Diagnostics/` folder (peer to Render); DEV_LOOP + PERF_BUDGET are internal split candidates.
- **Editing/ conflates edit surfaces with review/coordination**: tour.md (design-review walkthrough deliverable) and issues.md (BCF coordination board) are review/presentation, not editing. Candidate: a `Review/` (or `Coordinate/`) folder for {issues, tour}, leaving Editing = {inspector, forms, tables, history, notebook, media, livedata, collab}.
- **capture.md over-scoped** (5 concerns): DOCUMENT_EXPORT + OfficeExport should split to a Documents/export owner.
- **Charts/ thin (2 pages) but cohesive** — acceptable (LiveCharts rail vs bespoke-Skia rail); CustomVisual is a shared primitive (consumed by dashboards DashboardTile.Custom + Render SimVisual.ParallelCoords).

### Page-craft (systemic prose discipline)
Recurring giant single-paragraph Boundary sections violating comment-discipline and inverting prose-vs-fence: `dashboards.md:20` (~60 lines), `custom.md:19`, `collab.md:20`, `capture.md:165` (~40 lines), `evidence.md:278` (~50 lines). These describe behavior (GeoOverlay swap, meshlet cull, Office writers) the fences don't fully encode.

### AppHost seam compliance (mandate-3)
AppUi consumes AppHost ports as settled vocabulary correctly throughout: `TelemetryContributorPort`/`AppUiTelemetry.Contribute` (single spine), `ReceiptSinkPort`/`ReceiptEnvelope`, `ClockPolicy`, `CorrelationId`, `TenantContext`, `DeterminismContext`/`DeterminismKernel`/`RecomputeGraph`, `DataClassification`, `UiSchedulerPort`/`SurfaceScheduler`. No host-plumbing re-mint observed. The ONE friction is the fault-band mirror obligation (VC3) and the notebook RecomputeGraph duplication (VC9).

### Waterfall-ripple candidates (record in return)
- **Persistence CrdtOpWire ← AppUi (VC1)**: if CrdtOpWire lacks an ingest for AppUi's edit-intent op stream, the Persistence brief gains a consumer-pressure capability row (AppUi named as the demanding live-collab consumer); AppUi collab.md gains the edit-intent→CrdtOpWire projection owner. Both sides declare.
- **Compute residency ↔ AppUi meshlets (VC6)**: if meshlet clusters are Compute-owned (meshopt), pipeline TS_PROJECTION's `ResidencyMeshlet` is the seam and meshlets.md consumes `ResidencyPayload.Clusters` — Compute brief may need the CPU meshlet-build capability named; else AppUi admits meshoptimizer.

---

## VERDICT CANDIDATES (ranked, campaign-defining)

1. **LoroCs durable seam violates CrdtOpWire bit-parity law.** collab.md:105,117-120 persists Loro's opaque binary as the durable Persistence payload — a second durable CRDT owner foreclosed by RASM-CS-PERSISTENCE-BRIEF.md:41,203. Ruling: keep LoroCs as the LIVE convergence+presence engine; the durable seam must project EDIT-INTENTS → CrdtOpWire (never opaque Loro bytes); durable snapshot = CrdtOpWire projection; removal only if edit-intent mapping proves lossy. history.md is the correct-discipline exemplar.

2. **issues.md CommentThread/CommentOp is a bespoke CRDT collab.md already DROPPED.** issues.md:116-173 (full HLC-LWW register + Apply/Merge/Log) directly contradicts collab.md:20 ("CommentThread/CommentOp register DROPPED root-up") and collab.md:17 ("issue comment thread is a `map` container"). Ruling: comment thread = CollabDoc `map` container; delete the bespoke CRDT.

3. **No fault-band registry; ad-hoc bands, decimal/hex split, ≥2 literal collisions.** FormFault 4900 = CaptureFault 4900; CollabFault 0x4B00 = ContentFault 0x4B00. Ruling: one AppUi fault-band allocation owner (`[SmartEnum<int>]`) mirroring into the AppHost FaultBand registry (RASM-CS-APPHOST-BRIEF.md:57,153); re-partition collisions; pick one encoding.

4. **Three admitted packages have zero page consumers → integration mandates.** PDFsharp/MigraDoc (PDF via SKDocument; MigraDoc flow-report unrealized), Mapsui.Avalonia12 (Basemap seam declared ARCHITECTURE.md:67 but no page; geo via LiveCharts GeoMap), NodeEditorAvalonia (README-promised parametric/dependency-graph surfaces have no page). Per mandate, absence-of-consumers is NEVER a removal reason — each is an integration mandate on a named home OR a proven-redundancy removal argued from a stronger admitted owner.

5. **Folder architecture: evidence.md is mis-filed diagnostics; Editing/ mixes edit with review.** evidence.md (Render/) is a package-wide DIAGNOSTICS rail (5 concerns) → own Diagnostics/ folder + internal split (dev-loop, perf-governor). Editing/ conflates true edit surfaces with review/coordination (tour.md, issues.md) → candidate Review/ folder. capture.md over-scoped (offscreen+codec+color+pagination+Office) → split Office/document export.

6. **Meshlet & BVH build fences are naive stubs behind confident "settled" prose.** meshlets.md:71-81 gives every meshlet identical mesh.Bounds + never-cull NormalCone(-1) + whole-mesh ScreenSpaceError (cull non-functional; prose:14 claims per-meshlet bounds/cone + edge-collapse LOD). pathtrace.md:33-40 Bvh.Split never recurses; Accumulate does no ray tracing. Plus ownership ambiguity: pipeline TS_PROJECTION consumes Compute-built ResidencyMeshlet while meshlets.md re-builds locally. Ruling: decide meshlet-build owner (Compute meshopt vs AppUi+meshoptimizer); make the CPU reference builds real or explicitly signature-locked as spike-only.

7. **Color-space vocabulary split across two owners.** custom.md ColorSpaceAxis (4 rows, "single suite-wide gamut vocabulary") vs capture.md ColorPolicy (6 rows: +WideGamut,+HdrPq). Claimed projection (capture.md:165) but divergent row sets. Ruling: one owner — the encode-side ColorPolicy rows ARE the ColorSpaceAxis projection (or fold ColorSpaceAxis into ColorPolicy); no parallel gamut enum.

8. **inspector.md ThreeWay.Diff is a positional line-zip, not a real three-way merge.** inspector.md:360-372 zips lines by ordinal index, so any insert/delete flags all trailing lines conflicted — directly contradicting its own :299 claim. Ruling: real LCS/diff3 alignment on the conflict-resolution path (or consume a Persistence-owned diff if one exists).

9. **notebook.md DependencyGraph duplicates the AppHost RecomputeGraph it claims is the one owner.** notebook.md:115-168 is a full local topo+dirty+recompute engine; :210 asserts "a second incremental-recompute owner is the deleted form" and alignment to AppHost RecomputeGraph. Ruling: compose AppHost RecomputeGraph node-identity, or retract the one-owner claim and justify a document-local engine.

10. **Systemic page-craft: giant run-on Boundary paragraphs invert the prose-vs-fence ratio.** dashboards.md:20, custom.md:19, collab.md:20, capture.md:165, evidence.md:278 are single semicolon-chained paragraphs (40-60 lines) describing behavior the fences don't encode. Ruling: refactor Boundary prose to concise declarative clauses; push described-but-unfenced behavior (GeoOverlay swap, meshlet cull, Office writers) into fences or honest research gates.

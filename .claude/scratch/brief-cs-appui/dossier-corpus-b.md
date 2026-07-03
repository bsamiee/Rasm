# DOSSIER — corpus-b — Rasm.AppUi/.planning (second half)

Scope: alphabetically-sorted subfolders `[Charts, Editing, Render, Shell, Theme]`; second half = **Render (9), Shell (10), Theme (6)** deep-read fully (25 pages, 5823 LOC). First half (`Charts`, `Editing`) skimmed only at seams — `Editing/collab.md` read in full (mandate item 1), `Charts/dashboards.md` at the `BoardState`/`DockSerializer` seam. Governing docs read: `ARCHITECTURE.md`, `README.md`, `Rasm.AppUi.csproj`, `libs/.planning/architecture.md`, sibling briefs (Persistence/AppHost/Fabrication/Component-Paradigm) at the mandate seams.

Overall: this is a **world-class corpus** — the polymorphic-owner discipline, ROP rails (`Fin`/`IO`/`Validation`), `[Union]`/`[SmartEnum]`/`[ValueObject]` ADT collapse, and AppHost-port composition are exemplary. Defects are concentrated in (a) a **collapsed fault-code space** (top ruling), (b) **fence-realism gaps** where "settled" code fences don't do what their prose claims, (c) **two cross-page/cross-package seam tensions** the mandate names (LoroCs↔CrdtOpWire, meshlet-cluster↔Compute), and (d) folder-placement of the diagnostics spine.

---

## PER-PAGE VERDICTS

### RENDER (deep)

**pipeline.md — 9/10** (507 LOC, the folder spine). `RenderGraph` pass-DAG, `GpuBackend` `RenderTargetFactory` column, `ResolvePass`/`ResolvePolicy` tier ladder, `SimVisual` union, `Viewpoint`↔BCF codec, `ResidencyManifest` single-mint of the web residency wire. Exemplary backend-agnostic factory column; honest SPIKE-gating.
- Defect (prose/fence): `[02]` header entry (`pipeline.md:16`) signs `Frame(RenderGraph graph, ViewportClock, FrameBudget)` but the fence method (`:155`) is instance `Frame(ViewportClock, FrameBudget)` — stale `graph` param in prose.
- Defect (dead ordinal): `Render` fold seals `FrameReceipt(0L, …)` (`:168`) then `Frame` overwrites ordinal via `Interlocked` (`:157`) — correct, but `Empty(…)` fault path (`:194`) hardcodes `0L` ordinal, so a faulted frame always sinks ordinal 0 (collides on the correlation/render-hash lane).
- Charter: correct as the pure GPU-frame owner; keep.

**drafting.md — 9/10** (353 LOC). Cleanly **disposes mandate item (2)**: painter depth-sort DROPPED root-up (`:96`, `:418`), `HiddenLineSeam` composes `Rasm.Fabrication/Posting/projection#PROJECTION_HIDDEN_LINE` BSP solver; silhouette CONSUMPTION contract forward-ready. `SheetSize`/`TitleBlock`/`Dimension`/`Annotation`/GD&T rich.
- Defect (hardcode): `DraftEmit.Project` pins `ProjectionBasis.Top` for **every** region (`drafting.md:310`) — a sheet with a front elevation + iso detail drafts both as Top. `SheetRegion` (`:77`) carries no basis/view, so the region cannot specify its own projection. Move the basis onto `SheetRegion` (or a `Viewport2D` spec per region); the `meshOf(region.Key)` conflation (region key doubles as mesh-lookup key, `:311`) is the same thin-carrier symptom.
- Roster/seam: `WriteDwg` uses **ACadSharp** `CadDocument`/`DwgWriter` (`:342-360`) — but the mandate/Fabrication-seam (`Fabrication/ARCHITECTURE.md:51`) assigns ACadSharp to **Bim READ** and says AppUi's write leg is "over netDxf." netDxf cannot write DWG (DXF-only); ACadSharp is the only DWG writer. Brief must refine the mandate framing: ACadSharp is a shared central pin (Bim = read authority, AppUi = DWG-write leg), netDxf owns only the DXF-write leg.

**evidence.md — 9/10** (456 LOC). 7-case `EvidenceReceipt` union + HLC sink envelope, correlation join with `SkewBand` algebra, capture lanes, headless `ProofEngine`+`ProofLaw` matrix, `DevLoop`, `PerfBudget` governor (folds telemetry→one `QualityVerdict` degrading passes+watermark+motion), `GpuTimeline.Migrate` deepening `FrameReceipt`. Composes AppHost ports throughout.
- Concern-mixing/folder: this page owns the **package-wide diagnostics/observability spine** (Surface/Focus/Disposal/Edit/Command facts — most NOT render) plus a sprawling `DEV_LOOP` (`:271-376`: hot-reload MSBuild gating + runtime evidence ingest + user-facing HUD/REPL/flamegraph/solve-scrub). It sits in `Render/` only because it also has capture lanes + consumes `FrameReceipt`. See folder-architecture verdict.
- Charter: the evidence UNION + join + proof engine is correct and dense; the misfit is its FOLDER, not its shape.

**capture.md — 9/10** (439 LOC). `DrawSource` capsule (one Skia-surface owner), thumbnails, previews, `VisualCodec` with **float-end-to-end color management** (ICC, `ColorPolicy` gamut rows, `ToneMap` LUT), `FlowBlock` PDF pagination, `OfficeExport` (XLSX/DOCX/PPTX OOXML). Excellent color depth.
- Split pressure: page owns raster-capsule + thumbnail + preview + codec/color + PDF-flow + **Office OOXML export** — the document/Office export (XLSX/DOCX via `DocumentFormat.OpenXml`) is a distinct "Export" concern from the offscreen-raster capsule. Cohesion via `RenderReceipt`/`VisualDestination`/`FlowBlock` is real, but this is the widest single page. Candidate to split an `Export` tier (Office+PDF+drafting-CAD emit).
- Note: `[05]` Boundary prose is a single ~2500-char paragraph (`:165`) — extreme density; acceptable but the folder's densest prose block.

**meshlets.md — 7/10** (131 LOC). `MeshletCluster`/`ClusterCull`/`BindlessTable`/`ResidencyBudget` — the residency owner is strong.
- **Illusory fence (strong)**: `MeshletCluster.Partition` (`meshlets.md:71-80`) emits **degenerate** per-meshlet data — `Bounds: mesh.Bounds` (every meshlet shares the whole-mesh sphere), `Cone: NormalCone(0,0,1,-1)` (cutoff −1 = never culled), `ScreenSpaceError: mesh.Bounds.Radius`. No edge-collapse, no LOD tree, no per-meshlet bounds/cone. The prose (`:14`) claims "computes each meshlet's bounding sphere and normal cone… folds the cluster-LOD tree bottom-up by edge-collapse." The fence does none of it — and the pipeline TS wire (`pipeline.md:463` `MeshletWire` cone apex/axis/cutoff, `:542` `MeshletWireOf`) EXPECTS real cones. See "meshlet-cluster ownership" verdict.
- `ResidencyBudget.Plan` (`:107-125`) LRU admit is real and correct.

**pathtrace.md — 8/10** (102 LOC). Consumes Materials `LayeredBsdf`/`SlabStack`/`SurfaceShade` — exemplary "one BSDF, integrator never re-derives lobe math" waterfall seam (`:82`).
- Fence-realism: `Bvh.Split` (`pathtrace.md:33-36`) **never recurses** — it appends ONE node claiming child indices `nodes.Count+1/+2` that are never emitted, so `Bvh.Build` produces a single root asserting two non-existent children. `PathTracePass.Accumulate` (`:64-67`) is a stub returning `Accumulated + sampleBudget` (no trace). RESEARCH honestly gates the GPU as SPIKE, but the CPU tracer is claimed "the correctness oracle" (`:3`) while the fence oracle doesn't trace.

**reality.md — 7/10** (184 LOC). `SplatSource`/`PointCloudSource`/`CapturePass` reality-capture; SPIKE-gated; measure overlay binds `Viewpoint`.
- **Content-identity re-mint (strong)**: `SplatEllipsoid.ContentKey` (`reality.md:46-49`) re-hashes mean/scale/rotation through `XxHash128` **locally**. This directly violates the single-mint law `pipeline.md:404,496` asserts ("AppUi mints no second content-identity value object… never re-folds raw positions") and the `[05]-[PROHIBITIONS]` intent. Residency keys TILES by the Compute payload's own `ContentKey`, not per-ellipsoid re-hashes.
- Carrier divergence: `SplatPayload`/`PointPayload` (`:52,:112`) are AppUi-local records with `{bytes, harmonics, bounds}` shape — but `pipeline.md` consumes Compute's `ResidencyPayload` (`.Kind/.ContentKey/.Clusters/.Layout`) directly for the same splat concept. Two divergent AppUi-side projections of one Compute concept; brief must pick the canonical boundary record.
- Dead carrier: `SplatSort` enum (`:54`, RadixDepth/RadixTile) is never read — `Sorted()` (`:72`) always `OrderByDescending` (a comparison sort, not the radix the type/prose name).

**shading.md — 8/10** (102 LOC). Per-`GpuBackend` `ShaderAssetCache` (SKRuntimeEffect vs wgpu pipeline-state), consumes Materials `LayeredBsdf`. Shares `ONE_WGPU_DEVICE`.
- Fence-realism: `ShaderShade.BindShade` (`shading.md:106-109`) only checks `Uniforms.Count==0` and returns `Succ(unit)` — never binds uniforms; the `RenderPass.Geometry` body ignores target/cluster and returns `visible` unchanged (`:102`). Honestly SPIKE-gated, acceptable for a design page.
- Mild duplication: pathtrace (CPU integrator) + shading (GPU shader) both "shade from `LayeredBsdf`" — legitimate "one BSDF, two evaluators" (`:76`), but the split reads thin (shading = 102 LOC, mostly cache + stub). Could fold into pathtrace as a GPU arm.

**immersive.md — 8/10** (107 LOC). OpenXR stereo + `XR_FB_passthrough`, shares `ONE_WGPU_DEVICE`, honest flat-fold fallback (macOS ships no loader). Strong.
- Prose/fence: `[02]` Entry prose (`immersive.md:66`) signs `Frame(ImmersiveSession, RenderGraph)` but the fence extension (`:80`) is `Frame(RenderGraph, ViewportClock, FrameBudget)` — stale/incomplete prose signature.

### SHELL (deep) — uniformly world-class (verdicts 8-10)

**hosts.md — 10/10** (271 LOC). **Disposes mandate item (3)** on the AppHost seam: 7-case `SurfaceHost` `[Union]`, `SurfaceSeam` delegate columns (no dispatch arm names a host API), `EmbedCapsule`, `SurfaceScheduler` completing `UiSchedulerPort`, `NativeAssets` per-RID identity. Composes `TelemetryContributorPort`/`ReceiptSinkPort`/`ClockPolicy`/`CorrelationId`. The exemplar host-neutral mount axis. No material defect.

**navigation.md — 9/10** (247 LOC). `NavRequest` 5-verb union, `ShellRoot` ReactiveUI router, `ShellDockFactory` (dockables = screens), `LayoutLedger` with AppHost port registrations (`ScheduleEntry`/`DrainParticipantPort`/`SupportContributorPort`/`TelemetryContributorPort`), concrete `DockSerializer` seam. `BoardState` (dashboards) + dock graph = two blobs over one serializer — clean. Minor: `NavRequest.Parse` (`:39`) treats `rasm:` as a path segment (fragile scheme handling).

**commands.md — 9/10** (259 LOC). One `CommandIntent` row table; menus/hotkeys/palette/deep-links/remote all derivation folds. `CommandGate` availability algebra, `CommandExecution` total-outcome rail + batch `Combine`, `CommandProjections` palette search. Composes `ReceiptSinkPort`/`TenantContext`. Deletes every parallel registry. No material defect.

**controls.md — 9/10** (204 LOC). 18-case `ControlIntent` `[Union]` materialized through one `ControlFactory` fold; `BehaviorRail.Intent` single binding bridge; token+automation derived from the one `IntentBinding`. `RejectViewBinding` structural rejection.
- Fence-realism: `MaterializePool.Rebind` (`controls.md:181-184`) only type-checks and returns the parked control — the prose (`:158`) claims "a parked control resets its bindings to the new row's ValuePath and re-applies its token role"; the fence resets nothing (the recycling stale-state core).
- Shallow: `Apply` (`:136`) sets only `TemplatedControl.ForegroundProperty` for the token brush (`:141`) — one property, though prose claims "every brush and metric."

**solver.md — 9/10** (193 LOC). Kiwi/Cassowary constraint-layout: `LayoutConstraint` algebra, flex/grid/auto-layout as preset rows, one `LayoutSolver` Panel folding the solve into measure/arrange, ordered-program TS wire for cross-surface parity. Excellent.
- Design-completeness: `Suggest` (`solver.md:182`) `TrySuggestValue` on the panel's own Width/Height vars, but those must be added as edit-variables in `Load` (via `ConstraintProgram.Edits`) for the suggest to take — an implicit contract not enforced; `SolvedRect` (`:190`) keys child geometry by `Control.Name` (a nameless child → zero rect).

**virtualization.md — 9/10** (178 LOC). One `VirtualWindow` fabric (list/tree/grid/canvas), `ExtentLedger`, `StickyProjection`, `HierarchyFlatten` (avoids the O(n)-per-toggle re-subscribe). Every windowed surface routes here.
- Complexity/prose gap: `ExtentLedger.Reaccumulate` (`virtualization.md:127-131`) is **O(n)** linear prefix rebuild `from..end`, but the prose (`:78`) claims a "Fenwick/prefix-sum tree… O(log n)." For a million measured rows, each measure is O(n) → O(n²) to fill. The page's whole value proposition ("constant cost… O(log n)") is undercut by the fence.

**dialogs.md — 8/10** (184 LOC). `DialogIntent` union (dismissal-as-value `Option`), `DialogTopology` per-surface rows, `ToastGate` suppression fold over phase/level, host-agnostic pickers. Minor: `ToastPipe` returns `Unit` synchronously (`:68`) though toasts are presentation effects (should be `IO<Unit>`).

**input.md — 9/10** (276 LOC, largest Shell page). `GesturePolicy` chord derivation over the one command table, `BehaviorRail` admitted triggers, `PanZoomRow`, `DragPayload`/`ClipboardRow` validation rail, `InputFabric` (SpaceMouse/Gamepad/Haptic/MIDI over HidSharp/Silk.NET.Input/Silk.NET.SDL/DryWetMidi), `DeviceDrivers` SDK capsules. Rich, well-mined `.api` surfaces.
- Breadth (mild): 6 sections spanning keyboard→behavior→pointer→drag/clipboard→alternative-device fabric→SDK drivers. Cohesive under "interaction" but `INPUT_FABRIC`+`DEVICE_DRIVERS` (alt-devices) could split from core hotkey/pointer/clipboard. Defensible as-is.

**accessibility.md — 8/10** (174 LOC). Catalog-sourced automation identity, `SceneAccessNode` 3D-scene a11y tree, `SpatialCue`, `ContrastGate` WCAG, `AccessProof` sweep.
- **Two color-science owners**: `ContrastGate.Ratio` (`accessibility.md:141`) uses Avalonia `ColorHelper.GetRelativeLuminance` for WCAG luminance, but `tokens.md:20` mandates `Wacton.Unicolour` as "the one suite colour owner" and forbids "a second color-interpolation implementation." WCAG relative-luminance and OKLab mix are different ops, but Unicolour computes both — so this is a second color-science entry point. See cross-cutting.

### THEME (deep)

**tokens.md — 9/10** (319 LOC). One frozen token catalogue (`TokenRow` union), orthogonal `ThemeVariantRow`×`DensityRow` resolve, Unicolour OKLab `mix`, `Colormap` perceptual catalog (viridis/magma/cividis/turbo, shared by Render `SimVisual`/`reality`), atomic `ThemeCell.Swap`, `Semi.Avalonia` chain. Exemplary single color-owner law. No material defect (ramp keys `region+3` etc. all within `RampSteps=3`).

**typography.md — 9/10** (208 LOC). 10-role `TypographyRole`, `FontChain` per-platform, one HarfBuzz `ShapingSurface`, `MarkdownProjection` (Markdig→role rows), `TextMetricsPolicy`.
- Dead carriers (honestly gated): `MarkdownProjection.Project` (`typography.md:172-178`) hardcodes `FrontMatter: None`/`Footnotes: empty` despite the pipeline enabling `UseYamlFrontMatter().UseFootnotes()` — the fields are declared but always empty pending research row FRONT_MATTER_AST.

**animation.md — 9/10** (203 LOC). `Track` union with `Of*` smart constructors (non-empty+sorted → total `Sample`), `Timeline`/`Playhead`/`Walkthrough`. Strong ADT discipline.
- **Phantom mode**: `Playhead.Advance` PingPong (`animation.md:121`) sets `Position=Total` and stops — identical to Once; it **never reverses direction** (no direction/sign field exists on `Playhead`). Declared enum case with non-functional distinct behavior.
- Dead param: `Scrub.To` (`:176`) takes `SurfaceScheduler scheduler` but the fence never uses it (no `ObserveOn`/marshal), though prose (`:161,:165`) claims "emits on the UI thread… marshals through the surface scheduler."

**motion.md — 9/10** (117 LOC). 6-row `MotionToken`, `SpringValue` (2 tunings → stiffness/damping), `PhaseMotion` frozen `ProgressPhase`→token map, `ReducedMotion` one global degrade switch. Clean.

**assets.md — 8/10** (210 LOC). 5-case `IconSource` rank-walk, `SvgPipeline` (retained scene graph, hit-test, SVG-font-via-typography-chain), `RasterAssets`, `AssetCatalog` (avares + `XxHash128` content-address). Strong. `AssetKey` `[ValueObject]`.

**locale.md — 9/10** (178 LOC). `LocaleRow` culture axis, `ResolvedLocale` (CultureInfo/NodaTime patterns/`CompositeFormat`/one ICU `MessageFormatter`), `LocaleStrings` inbox-resx, `LocaleRuntime` atomic swap, RTL `MirrorPolicy`. Suffix-branch plural DROPPED for real ICU CLDR. `Jeffijoe.MessageFormat` well-mined.
- Scope-stretch (mild): `[05]-[RTL_MIRRORING]` folds `LiveCaption`/`CaptionSource` (speech-recognition + machine-translation → shaped annotations, `:212-226`) into the mirroring section — an unbacked capability (NO admitted speech/MT package; research-gated LIVE_TRANSLATE) parked in locale.md.

### SEAM-SKIMMED (first half)

**Editing/collab.md — 8/10** (read fully; mandate item 1). `CollabDoc` over one `LoroDoc`, `CollabContainer` 6-kind axis, `SyncRail`, `Presence` (ephemeral), `TimeTravel`/`CollabUndo`. Drops bespoke `NotebookCrdt`/`CommentThread` LWW root-up. Companion-only firebreak (`loro.dylib` out of in-Rhino ALC). **See LoroCs↔CrdtOpWire verdict** — the durability seam is the open question.

**Charts/dashboards.md** (seam only). `BoardState` (`:219`) persists tile arrangement+brush over the concrete `DockSerializer` — the seam `navigation.md:99` names; two blobs, one serializer. Clean.

---

## CROSS-CUTTING

**Duplication / duplicate mechanisms**
- **Meshlet-cluster build** duplicated across the runtime boundary: Compute owns meshopt-built `ResidencyMeshlet` clusters (pipeline TS wire consumes them, `pipeline.md:407,542`), yet `meshlets.md` `MeshletCluster.Build`/`Partition` re-clusters (naively). Either AppUi consumes Compute's clusters (no re-build) or admits `meshoptimizer` — it cannot claim to build clusters the wire says come from Compute.
- **Splat/point carrier**: `reality.md` `SplatPayload`/`PointPayload` vs `pipeline.md` Compute `ResidencyPayload` — two AppUi-side projections of one Compute geometry concept.
- **Color science**: Unicolour (tokens, mandated sole owner) vs Avalonia `ColorHelper.GetRelativeLuminance` (accessibility WCAG) — two entry points.
- **Shade-from-BSDF**: pathtrace (CPU) + shading (GPU) — justified "two evaluators," but shading is thin.

**Concern-mixing**
- `evidence.md` = package-wide diagnostics/observability spine (7-case union folding Surface/Focus/Disposal/Edit/Command; correlation join; proof engine; dev-loop; perf-governor) filed under `Render/`.
- `capture.md` mixes offscreen-raster capsule + codec/color + PDF-flow + Office OOXML export.
- `evidence.md` `DEV_LOOP` (`:271-376`) mixes build-time MSBuild hot-reload gating + runtime evidence ingest + user-facing HUD/REPL/flamegraph/solve-scrub.

**Hardcoding-vs-generator**
- `drafting.md:310` `ProjectionBasis.Top` pinned for every region (should be `SheetRegion` data).
- Everything else is table/row-driven (sheet sizes, breakpoints, page rows, colormaps, tokens are correctly seed DATA on polymorphic axes).

**Dead / thin typed carriers**
- `reality.md` `SplatSort` enum (never read; `Sorted()` ignores it).
- `animation.md` `PlaybackMode.PingPong` (behaves as Once; no direction field).
- `animation.md` `Scrub.To` `scheduler` param (unused).
- `typography.md` `MarkdownDocumentRows.{FrontMatter,Footnotes}` (always empty; research-gated).
- `pipeline.md` `FrameReceipt` ordinal `0L` on the `Empty` fault path (collides on lane key).

**Unwired / under-declared seams**
- **Fault bands are NOT registered** in the AppHost/federation `FaultBand` registry (top verdict) — every AppUi fault hardcodes `base(detail, NNNN)`, the exact form the AppHost band law forbids.
- **LoroCs durability seam** underspecified (verdict): `collab.md:105` "projects onto the Persistence `OpLogEntry`… replays the opaque delta blob" — does not say whether that is the typed `CrdtOpWire` op-log (incompatible with loro binary) or a distinct opaque-payload lane.
- Fabrication HLR seam: cleanly two-sided (`drafting.md` ↔ `Fabrication/ARCHITECTURE.md:46`). ✓

**Unmined capability (with catalog anchors)**
- `.api` catalogs are RICH and largely well-mined: `api-loro.md` (fully mined by collab — `SubscribeLocalUpdate`/`ExportMode`/`EphemeralStore`/`Awareness`/`UndoManager`/`RevertTo`/`ForkAt`), `api-silk-webgpu.md`+`api-silk-webgpu-wgpu.md` (mined by pipeline + `evidence.md:384` `GpuTimingPass` timestamp queries), `api-kiwi.md` (solver `Try*` family), `api-dynamicdata.md` (`Virtualise`/`TransformToTree`). **No integration-mandate orphans found in the deep half.**
- The one genuine capability GAP is meshlet clustering: no `meshoptimizer` binding admitted, and the fence `Partition` cannot produce the cone/LOD data the wire consumes — so either consume Compute's meshopt clusters (preferred; Compute owns `EXT_meshopt_compression`) or admit a meshopt binding.

**Folder-architecture verdicts** (graded axis)
- Structure is fundamentally SOUND: 5 folders, 37 pages, no one-file folders, no flat sprawl, source-mirroring. Not rot.
- Refinement 1 (moderate): `Render/evidence.md` is the package-wide diagnostics spine misfiled under the GPU-render folder; extract a `Diagnostics`/`Observability` tier (evidence + proof/dev-loop/perf-governor) so `Render` is the pure GPU-viewport tier (pipeline/meshlets/pathtrace/shading/immersive/reality).
- Refinement 2 (low): `Charts` (2 pages) is the thinnest folder — coherent but a fold candidate into a broader `Visuals` tier with `capture`.
- Refinement 3 (low): a document/CAD/Office `Export` concern is spread across `capture.md` (PDF/Office) + `drafting.md` (DWG/DXF) — candidate to unify.

---

## VERDICT CANDIDATES (campaign-defining, strongest first)

**V1 — FAULT-CODE SPACE HAS COLLAPSED; register bands in the federation FaultBand registry.** The AppUi fault-code space fails on four axes simultaneously: (a) **intra-AppUi hard collisions** — `VirtualFault`≡`TourFault` (4300; `virtualization.md:48`/`tour.md:30`), `CaptureFault`≡`FormFault` (4900; `reality.md:30`/`forms.md:29`), `CollabFault`≡`ContentFault` (0x4B00; `collab.md:43`/`media.md:28`); (b) **radix split that does not nest** — pages 41xx-49xx are DECIMAL (`hosts.md:43`=4100 … `reality.md`=4900) while 4Axx-4Dxx are HEX (`history.md:37`=0x4A00 … `immersive.md:29`=0x4D00); decimal 4900 → hex 0x4A00 is 4900→19456, not contiguous; (c) **cross-package collision** with the AppHost/federation `FaultBand` registry (`RASM-CS-APPHOST-BRIEF.md:57`, mirroring `RASM-COMPONENT-PARADIGM-DECISION.md:141-149`): `SurfaceFault`/`AssetFault`/`DialogFault`/`InputDriverFault` (4100-4153) ⊂ AppHost `ConfigError` 4100-4199; `ViewportFault` 4500 ≡ AppHost `HopFault` 4500; `DraftFault` 4600 ≡ AppHost `CommandFault` 4600; `EditFault` 4700-4799 ⊃ AppHost `WireFault` 4720; (d) **forbidden literals** — every fault hardcodes `base(detail, NNNN)`, the exact form the AppHost law bans ("every fault union's Code derives through the registry row, never a `base(detail, NNNN)` literal"). Disposition: AppUi owns a contiguous, single-radix band allocation registered as pinned mirror rows in the federation `FaultBand` `[SmartEnum<int>]`; every `Code` derives through the row; disjointness type-enforced.

**V2 — LoroCs is CHARTERED App-UI-side, but its DURABILITY seam must decode/replay onto CrdtOpWire, not persist loro-native blobs.** `collab.md` richly charters `LoroCs` as the live-collab merge authority (companion-only; drops bespoke CRDTs) — correct per mandate item (1). But `SyncRail` persists **loro-native opaque binary** (`collab.md:105` "Persistence replays the opaque delta blob without re-modeling the CRDT"; `:117` `CollabSnapshot` content-addresses the loro blob). The Persistence brief (`RASM-CS-PERSISTENCE-BRIEF.md:203`) rejects LoroCs precisely because `CrdtOpWire`'s `[MessagePack.Union]` wire must be bit-parity across runtimes (`:41`). The mandate law is "decode/replay at the boundary, **never a second durable CRDT owner**" — persisting loro binary IS a second durable CRDT representation on disk. Ruling required: either (A) loro deltas TRANSCRIBE to `CrdtOpWire` ops at the `SyncRail` boundary (CrdtOpWire = sole durable owner; loro = ephemeral live engine; `collab.md`'s opaque-blob persistence rebuilt), OR (B) explicitly charter a Persistence **opaque-payload lane** distinct from the `CrdtOpWire` ElementGraph CRDT, justified by disjoint domains (ElementGraph federation vs UI-document co-editing) — and re-word `collab.md:105`'s "projects onto the Persistence `OpLogEntry`" to name that opaque lane, not the typed CrdtOpWire op-log. As written the seam is ambiguous and reads as the forbidden second durable CRDT. (Page is in `Editing`/corpus-a's half; the ruling is brief-level.)

**V3 — Meshlet clustering is DUPLICATED with Compute and the AppUi fence is illusory.** `pipeline.md` TS wire projects Compute's meshopt-built `ResidencyMeshlet` clusters (`pipeline.md:407,547`), yet `meshlets.md` `MeshletCluster.Build`/`Partition` (`meshlets.md:57-80`) re-clusters — and does so with **degenerate output**: every meshlet gets the whole-mesh bounding sphere, a never-cull cone `NormalCone(0,0,1,-1)`, and no LOD tree, contradicting its own prose (`:14`) and the cone-carrying wire (`MeshletWire`, `pipeline.md:463`). Ruling: AppUi CONSUMES Compute's `ResidencyPayload.Clusters` (Compute owns `EXT_meshopt_compression`) rather than re-partitioning; the AppUi meshlet page keeps the residency/cull/bindless owner and drops the naive build. If a genuine AppUi-side build is required, admit a `meshoptimizer` binding — the current fence cannot produce the data downstream consumers require.

**V4 — A cluster of "settled" Render fences do not do what their prose claims (fence-realism).** `pathtrace.md:33-36` `Bvh.Split` never recurses (root claims non-existent children); `pathtrace.md:64` `PathTracePass.Accumulate` is a stub; `shading.md:106` `BindShade` binds no uniforms; `controls.md:181` `MaterializePool.Rebind` resets no bindings (recycling stale-state core); `virtualization.md:127` `ExtentLedger.Reaccumulate` is O(n) while prose claims a Fenwick O(log n) tree; `animation.md:121` `PlaybackMode.PingPong` never reverses. Each is prose-vs-fence divergence on a surface marked settled. Ruling: either implement the fence to the prose or downgrade the prose to an honest RESEARCH/SPIKE gate — the corpus law is "code fences are the product," so a fence that lies about being settled is a defect, not a deferral.

**V5 — Content-identity single-mint law is violated in reality.md.** `SplatEllipsoid.ContentKey` (`reality.md:46-49`) re-hashes raw mean/scale/rotation through `XxHash128`, contradicting `pipeline.md:404,496` ("AppUi mints no second content-identity value object… never re-folds raw positions") and the `ARCHITECTURE.md` PROHIBITION. Ruling: residency keys tiles by the Compute payload's own `ContentKey`; the per-ellipsoid re-hash is deleted; `SplatPayload`/`PointPayload` reconcile to the one Compute `ResidencyPayload` boundary record (removing the divergent carrier).

**V6 — Two color-science owners contradict the mandated single owner.** `tokens.md:20` mandates `Wacton.Unicolour` as "the one suite colour owner" and forbids "a second color-interpolation implementation," but `accessibility.md:141` `ContrastGate.Ratio` computes WCAG luminance via Avalonia `ColorHelper.GetRelativeLuminance`. Ruling: route the WCAG relative-luminance gate through Unicolour (which owns the sRGB→luminance transform) so contrast, OKLab mixing, and colormap sampling share one color kernel — or explicitly charter ColorHelper as the WCAG-only luminance owner distinct from the OKLab mix owner (two owners, disjoint transforms, stated).

**V7 — ACadSharp DWG-write coexists with Bim's ACadSharp-READ ownership; refine the "netDxf write leg" framing.** `drafting.md:342` writes DWG via ACadSharp `DwgWriter` (the only DWG writer; netDxf is DXF-only), while `Fabrication/ARCHITECTURE.md:51` and the mandate assign ACadSharp to Bim READ and say AppUi's write leg is "over netDxf." Ruling: ACadSharp is a shared central pin — Bim = CAD-interchange READ authority, AppUi = DWG-WRITE leg; netDxf owns the DXF-WRITE leg. Both packages legitimately reference the one ACadSharp pin; the roster note and the mandate's "over netDxf" phrasing get refined, not the code.

**V8 — Extract the diagnostics spine from Render (folder architecture).** `Render/evidence.md` owns the package-wide observability spine (7-case `EvidenceReceipt` folding Surface/Focus/Disposal/Edit/Command — mostly non-render — plus correlation join, headless `ProofEngine`, `DevLoop`, `PerfBudget` governor). It is in `Render/` only because it also holds capture lanes + consumes `FrameReceipt`. Ruling: extract a `Diagnostics`/`Observability` folder (evidence + proof + dev-loop + perf-governor), leaving `Render` the pure GPU-viewport tier; optionally unify a document/CAD/Office `Export` concern (capture PDF/Office + drafting DWG/DXF) and re-evaluate the 2-page `Charts` folder. The structure is otherwise sound — this is refinement, not rot.

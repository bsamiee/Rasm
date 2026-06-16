# [RASM_APPUI_ROADMAP]

The `.planning/` corpus is finalized; implementation transcribes pages in the charter BUILD_ORDER ([planning charter](.planning/README.md)). Every task exits against named page clusters and proves through the charter PROOF_GATES. Owner realization state lives on the charter DENSITY_BAR `[STATE]` column; this roadmap routes to it rather than mirroring it.

## [1]-[CURRENT_POSITION]

| [INDEX] | [SURFACE]         | [STATE]                                       |
| :-----: | :---------------- | :-------------------------------------------- |
|   [1]   | planning corpus   | 23 pages finalized; charter complete (viewport/drafting/notebook/animation authored, GPU+writer SPIKE-gated) |
|   [2]   | package graph     | runtime closure admitted and lock-tracked     |
|   [3]   | production source | absent                                        |
|   [4]   | test project      | `Rasm.AppUi.Tests` node present, empty         |
|   [5]   | API catalogues    | 37 pages current; `Verify.XunitV3` admission pending |

## [2]-[START_GATES]

Implementation-start gates: bridge-proofed spikes and research-resolution probes that need a live host or scratch process. Decompile-grade research items (`assay api resolve`) resolve inline inside the owning task and are not listed. An unresolved gate blocks its named cluster; the charter FILE_PROCESS makes the block mechanical and the charter PROOF_GATES carry the executable rails.

| [INDEX] | [GATE]                                              | [PROBE]                                                                                                  | [UNBLOCKS]                                              |
| :-----: | :-------------------------------------------------- | :------------------------------------------------------------------------------------------------------- | :----------------------------------------------------- |
|   [1]   | Avalonia-in-Rhino NSView embedding spike            | `uv run python -m tools.assay bridge verify --pattern avalonia_embed_pump` (+ `avalonia_embed_resize`, `avalonia_embed_render`) | surface-hosts#EMBED_CAPSULE, #SCHEDULER_BOUNDARY, #HOST_AXIS |
|   [2]   | `Rasm.AppUi.Tests` row on the assay test rail       | `uv run python -m tools.assay test run --target Rasm.AppUi.Tests`                                        | every spec and render-hash gate below                  |
|   [3]   | HotAvalonia Release closure strip + markup-loader floor | `dotnet build libs/csharp/Rasm.AppUi/Rasm.AppUi.csproj -c Release`                                       | diagnostics-evidence#DEV_LOOP                          |
|   [4]   | Embedded TopLevel service resolution                | `tests/csharp/libs/Rasm.AppUi/scenarios/appui-embedded-toplevel.verify.csx`                              | dialogs-notifications#NOTIFICATIONS, #PICKERS_HOST_MODALITY |
|   [5]   | Host-object drag across the NSView boundary         | `libs/csharp/Rasm.AppUi/scenarios/embedded-drag.verify.csx` under live RhinoWIP                          | input-interaction#DRAG_CLIPBOARD                      |
|   [6]   | macOS reduce-motion preference probe                | `libs/csharp/Rasm.AppUi/scenarios/reduced-motion-probe.verify.csx`                                       | motion-tokens#REDUCED_MOTION                          |
|   [7]   | VoiceOver reach across the embedded root            | `tests/csharp/libs/Rasm.Rhino/UI/scenarios/avalonia-embed-a11y.verify.csx`                               | accessibility#AUTOMATION_PEERS                        |
|   [8]   | input-interaction member re-grounding               | `uv run python -m tools.assay api resolve --pattern behaviors` then specs against the re-grounded trigger and clipboard rows | input-interaction#POINTER_GESTURES, #DRAG_CLIPBOARD   |
|   [9]   | host-shared `GRContext` GPU viewport spike          | `uv run python -m tools.assay bridge verify --pattern viewport_gpu_lease` (+ `viewport_gpu_meshlet`, `viewport_gpu_pathtrace`) under live RhinoWIP; standalone windowed `GRContext` first | viewport-pipeline#RENDER_GRAPH, #GEOMETRY_VIRTUAL, #RESIDENCY_BUDGET, #PATH_TRACE, #SIM_VISUAL |
|  [10]   | OpenXML Office writer surface                       | `uv run python -m tools.assay api resolve --pattern openxml` then writer round-trip against the admitted OpenXML package | visuals-offscreen#DOCUMENT_EXPORT (OfficeExport) |
|  [11]   | DWG/DXF entity-writer surface                       | `uv run python -m tools.assay api resolve --pattern dwg` then entity-table round-trip against the admitted entity-writer package | drafting-sheets#DRAFT_EMIT |
|  [12]   | alternative-input + device-output SDK surfaces      | `uv run python -m tools.assay api resolve` against the admitted SpaceMouse/controller/gaze/voice/MIDI/CNC packages bound through `SurfaceSeam` delegates | input-interaction#INPUT_FABRIC |

## [3]-[IMPLEMENTATION_TASKS]

Ordered by the charter BUILD_ORDER; each task transcribes its clusters verbatim, resolves the RESEARCH items its pages carry, runs the collapse scan, and exits on the named proof.

| [INDEX] | [FILE]                         | [EXITS_AGAINST]                                                                                                   | [PROOF]                                  |
| :-----: | :----------------------------- | :--------------------------------------------------------------------------------------------------------------- | :--------------------------------------- |
|   [1]   | `Hosts/SurfaceVocabulary.cs`   | surface-hosts#HOST_AXIS, #SCALE_FOCUS                                                                            | G3 + G4 specs                            |
|   [2]   | `Motion/MotionRail.cs`         | motion-tokens#MOTION_AXIS, #MOTION_APPLICATION, #PHASE_MAPPING, #REDUCED_MOTION                                  | G4 phase-map conformance sweep (keys equal `ProgressPhase.Items`) |
|   [3]   | `Typography/TypographyRail.cs` | typography-shaping#ROLE_AXIS, #FONT_ADMISSION, #SHAPING_RAIL, #MARKDOWN_PROJECTION, #TEXT_METRICS                | G4 shaping specs                         |
|   [4]   | `Assets/AssetCatalog.cs`       | icons-assets#ASSET_CATALOG, #RASTER_ASSETS                                                                       | G4 specs                                 |
|   [5]   | `Assets/IconRail.cs`           | icons-assets#ICON_AXIS, #SVG_PIPELINE                                                                            | G6 render-hash                           |
|   [6]   | `Theme/ThemeTokens.cs`         | theme-tokens#TOKEN_CATALOG, #VARIANT_AXIS, #DENSITY_AXIS                                                         | G4 specs                                 |
|   [7]   | `Localization/LocaleRail.cs`   | localization-culture#LOCALE_AXIS, #STRING_TABLES, #CULTURE_COMPOSITION, #RTL_MIRRORING                          | G4 specs                                 |
|   [8]   | `Hosts/SurfaceRail.cs`         | surface-hosts#HOST_AXIS (dispatch), #EMBED_CAPSULE, #SCHEDULER_BOUNDARY, #NATIVE_ASSETS; theme-tokens#CONTROL_THEMES | G5 bridge (embed pump/resize/render) + G6 variant sweep; `Surfaces.Mount` carries `ClockPolicy` |
|   [9]   | `Theme/ThemeRail.cs`           | theme-tokens#CONTROL_THEMES                                                                                      | G6 render-hash                           |
|  [10]   | `Commands/CommandRail.cs`      | commands-availability#INTENT_TABLE, #AVAILABILITY_ALGEBRA, #EXECUTION_RECEIPTS, #PALETTE_AND_REMOTE, #TS_PROJECTION | G4 specs (deck freeze, conflict fold, receipt totality, palette ranking, wire round-trip) |
|  [11]   | `Input/InteractionRail.cs`     | input-interaction#HOTKEY_DERIVATION, #BEHAVIOR_RAIL, #POINTER_GESTURES, #DRAG_CLIPBOARD                          | G5 bridge (embedded-drag) + G4 specs     |
|  [12]   | `Screens/ScreenRail.cs`        | screens-activation#SCREEN_CATALOG, #ACTIVATION_SCOPES, #DERIVED_STATE, #VALIDATION_UX, #SCREEN_STATE            | G4 specs (activation/suspend law, snapshot merge) |
|  [13]   | `LiveData/LiveDataRail.cs`     | live-data#DATA_SOURCES, #CHANGE_PIPELINES, #BINDING_CAPSULE, #AGGREGATION_SPINE                                  | G4 specs under `VirtualTimeScheduler`; single-`ObserveOn` law |
|  [14]   | `Tables/TableRail.cs`          | tables-hierarchy#GRID_SUBSTRATE, #VIEW_STATE, #TREE_FLATTEN, #GRID_COMMIT                                        | G4 specs (tree-flatten fold, DeferRefresh batching) |
|  [15]   | `Inspector/InspectorRail.cs`   | inspector-editing#INSPECTOR_SURFACE, #EDITOR_FACTORIES, #COMMIT_VALIDATION, #OPTIONS_INSPECTOR, #CONFLICT_RESOLUTION, #CODE_EDITING | G4 specs (editor rank walk, preview-versus-commit law) |
|  [16]   | `Dialogs/DialogRail.cs`        | dialogs-notifications#DIALOG_INTENTS, #SESSION_ALGEBRA, #NOTIFICATIONS, #PICKERS_HOST_MODALITY                  | G5 bridge (embedded TopLevel) + G4 specs (toast gate totality) |
|  [17]   | `Shell/ShellRail.cs`           | shell-navigation#ROUTING_SPINE, #DOCK_LAYOUTS, #SHELL_CHROME, #ADAPTIVE_LAYOUT                                  | G4 specs (dock serialize-restore round-trip, layout admit) |
|  [18]   | `Charts/ChartRail.cs`          | charts-dashboards#SERIES_TABLE, #AXES_SECTIONS, #CHART_INTERACTION, #STREAM_BINDING, #DASHBOARD_TILES          | G6 render-hash per `ChartSeriesSpec` row and per named dashboard; Lttb fold spec |
|  [19]   | `Visuals/VisualRail.cs`        | visuals-offscreen#DRAW_CAPSULE, #THUMBNAIL_PIPELINE, #PREVIEW_SURFACES, #ENCODE_IDENTITY, #DOCUMENT_EXPORT      | G6 render-hash                           |
|  [20]   | `Access/AccessRail.cs`         | accessibility#AUTOMATION_PEERS, #KEYBOARD_NAV, #CONTRAST_GATE, #COMPLIANCE_PROOF                                | G4 contrast-floor rows + audit sweep over the typed (variant, density) grid |
|  [21]   | `Evidence/EvidenceRail.cs`     | diagnostics-evidence#RECEIPT_UNION, #CORRELATION_JOIN, #CAPTURE_LANES, #HEADLESS_DERIVATION, #DEV_LOOP, #TS_PROJECTION | G6 render-hash + G5 bridge; full `ProofEngine.Derive` matrix; journal replay under `FakeTimeProvider` |
|  [22]   | `Viewport/ViewportPipeline.cs` | viewport-pipeline#RENDER_GRAPH, #GEOMETRY_VIRTUAL, #RESIDENCY_BUDGET, #PATH_TRACE, #SIM_VISUAL, #VIEWPOINT_CODEC, #TS_PROJECTION | G5 bridge (GPU lease/meshlet/path-trace spikes) + G6 render-hash (CPU/2D-Skia fallback frame); frame-budget invariant spec |
|  [23]   | `Drafting/DraftingRail.cs`     | drafting-sheets#SHEET_SET, #PROJECTION, #DIMENSIONING, #DRAFT_EMIT | G6 render-hash (PDF/SVG) + G5 bridge (DWG/DXF entity-writer spike); sheet-bounds and projection-basis specs |
|  [24]   | `Notebook/NotebookRail.cs`     | notebook-document#CELL_MODEL, #DEPENDENCY_GRAPH, #CRDT_COEDIT, #REPLAY_BUNDLE | G4 specs (recompute closure O(downstream), CRDT convergence, replay bit-identity under `FakeTimeProvider`) |
|  [25]   | `Animation/AnimationRail.cs`   | animation-timeline#TRACK_MODEL, #TIMELINE, #SCRUB, #WALKTHROUGH | G4 specs (deterministic frame-indexed sample) + G6 render-hash (walkthrough frame sequence) |

G1 and G2 run once before task [1] and again on any manifest or catalogue change; G7 runs on any page-diagram edit. The BUILD_ORDER seam notes are binding at transcription: `AssetKeys` nameof spellings are the only cross-file asset references; the `PhaseMotion` map keys mirror the Compute `ProgressPhase` nine-case set; `AccessProof.Sweep` and `ProofEngine.Derive` share the typed `(ThemeVariantRow, DensityRow)` grid.

## [4]-[TESTING_APPROACH]

Universal rails carry one owner and resolved member identical across the four packages; versions live in `Directory.Packages.props`.

Universal-rail concept differentiator:

| [RAIL]                                          | [CONCEPT PROVEN]                                                                                                                              |
| :---------------------------------------------- | :------------------------------------------------------------------------------------------------------------------------------------------- |
| xUnit v3 managed law (UI-thread, headless rail) | surface-mount fold, command-intent gesture-conflict table, availability seeded-total pipeline, token resolve / freeze missing-cell rejection, nav / restore total fold; dispatches on the headless UI thread |
| CsCheck PBT                                     | OkLab perceptual-tween monotone-lightness law, token-diff changed-key set, nav-verb FSM                                                       |
| coverlet.MTP coverage                           | managed reachability of fold / projection / resolve surfaces; GPU / native draw path classified out                                           |
| dotnet-stryker mutation                         | killing oracle over command-availability gates, gesture-conflict fold, variant-fold idempotency, token changed-key diff; mutation pairs the managed UI-logic project, never the headless scenario harness |
| Verify.XunitV3 snapshot                         | render-hash chart baselines projecting the pinned-pixel content-addressed render-hash receipt text under `[AvaloniaFact]` dispatch + scale-pinned capture; snapshot the receipt, never floating pixels |
| ArchUnitNET architecture                        | one shaping owner per grid (view layer never shapes), no captured-brush staleness type leak, automation-id derivation reachability            |

Package-specific rails:

| [RAIL]                                       | [OWNER]                  | [CONCEPT PROVEN]                                                                                  | [RESOLVED MEMBER / TOKEN]                                                                              |
| :------------------------------------------- | :----------------------- | :----------------------------------------------------------------------------------------------- | :---------------------------------------------------------------------------------------------------- |
| xUnit v3 headless types                      | `api-headless.md`        | UI-thread fact / theory dispatch                                                                 | `[AvaloniaFact]`, `[AvaloniaTheory]`                                                                   |
| host/runtime scenarios (headless production) | `interaction.md` MOUNT_LAW | headless is a production row — frames advance only on `ForceRenderTimerTick` / `CaptureRenderedFrame`; synthetic input drives the real pipeline; the render-hash baseline pins scale before grab | `HeadlessUnitTestSession`, `Dispatch`, `GetOrStartForAssembly`, `DisposeAsync`, `ForceRenderTimerTick`, `CaptureRenderedFrame`, `SetRenderScaling` |

N/A rails: BenchmarkDotNet — `specialized-rails.md [2]` reject bars UI surfaces / viewport / UI threads; durable OkLab ramp math routes to the Compute benchmark rail. SharpFuzz — `specialized-rails.md [3]` reject names UI surfaces; markdown / SVG admission goes through frozen fail-loud pipelines.

## [5]-[EXIT]

The package exits implementation when every BUILD_ORDER file is transcribed `Hosts/SurfaceVocabulary.cs` through `Animation/AnimationRail.cs`, every PROOF_GATES row is green (G1 restore, G2 `api doctor`/`resolve`, G3 `static build`, G4 `test run`, G5 `bridge verify`, G6 headless render-hash lanes, G7 `mmdc` render), the charter GAP_LEDGER stays fully CLOSED, and the charter `spec` gate passes on the full suite.

Residual host-bridge work is the charter DENSITY_BAR `SPIKE` set discharging against its page RESEARCH clusters; each exits when its bridge or native probe lands as a settled fence row rather than a re-opened gate:
- Embedding seam (`SurfaceHost`, `SurfaceFact`) — surface-hosts#RESEARCH [EMBED_SPIKE]: gated on the live Rhino-owned AppKit run-loop and NSView panel host, de-risked standalone with only the in-host confirmation remaining.
- Viewport GPU surface (`RenderPass`, `RenderGraph`, `MeshletCluster`, `ResidencyBudget`, `PathTracePass`, `SimVisual`, `ViewportFault`) — viewport-pipeline#RESEARCH [VIEWPORT_GPU]: the host-shared `GRContext` lease, per-backend bindless/acceleration-structure spellings, and `SKRuntimeEffect` mesh/ray-gen emit gated on the live host GPU context; every owner ships its CPU/2D-Skia fallback today, de-risked against a windowed `GRContext`.
- Drafting entity writer (`DraftEmit`, `DraftFault`) — drafting-sheets#RESEARCH [DRAFT_ENTITY]: the DWG/DXF model-space entity-writer member set; PDF (`SKDocument`) and SVG (`SKSvgCanvas`) emit ship today.
- Office export (`OfficeExport`, `OfficeSpec`, `OfficeSheet`) — visuals-offscreen#RESEARCH [OFFICE_OPENXML]: the OOXML part-graph and font-embedding member set gated on the admitted OpenXML writer; the `FlowFold`-shared deterministic pagination is settled.
- Input fabric SDKs (`InputDevice`, `DeviceOutput`) — input-interaction#RESEARCH [INPUT_DEVICE_SDK]: the SpaceMouse/controller/gaze/switch/voice/MIDI and CNC/robot/haptic driver member spellings bound through `SurfaceSeam` delegate columns; the device-to-intent and intent-to-device folds are settled.

Implementation-bound refinements ride FINALIZED owners and carry no `SPIKE` state; each is a settled fence whose remaining detail binds against the live closure at transcription:
- Embedded notification, picker, and host-object-drag capsule rides the embedding seam — dialogs-notifications#RESEARCH [EMBEDDED_TOPLEVEL], input-interaction#RESEARCH [EMBEDDED_DRAG]: the embedded-root `TopLevel` resolution resolves with the embed capsule.
- Behaviors routed-event spellings — input-interaction#RESEARCH [GESTURE_TRIGGERS]/[POINTER_CAPTURE]/[ROTATE_GESTURE]: the per-gesture trigger family binds at transcription against the installed `Xaml.Behaviors.Avalonia` surface.
- Geo land-record projection — charts-dashboards#RESEARCH [GEO_PAYLOAD]: the `GeometryPayload`-to-`GeoMap` land-record projection is the cross-package wire boundary, with `SourceGenMapChart` `ActiveMap`/`MapProjection`/`Series`/`Stroke`/`Fill` confirmed.
- Flow page-resume — visuals-offscreen#RESEARCH [PARAGRAPH_BREAK]: the within-`FlowBlock.Text` cluster split rides the confirmed `SKShaper.Result` `Clusters`/`Width` surface.
- Dev-loop Release strip — diagnostics-evidence#RESEARCH [DEV_LOOP_STRIP]: the HotAvalonia strip and markup-loader floor verify against the built MSBuild closure.

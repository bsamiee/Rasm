# [RASM_APPUI_ROADMAP]

The `.planning/` corpus is finalized; implementation transcribes pages in the charter BUILD_ORDER ([planning charter](.planning/README.md)). Every task exits against named page clusters and proves through the charter PROOF_GATES. Owner realization state lives on the charter DENSITY_BAR `[STATE]` column; this roadmap routes to it rather than mirroring it.

## [1]-[CURRENT_POSITION]

| [INDEX] | [SURFACE]         | [STATE]                                       |
| :-----: | :---------------- | :-------------------------------------------- |
|   [1]   | planning corpus   | 18 pages finalized; charter complete          |
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

The package exits implementation when every BUILD_ORDER file is transcribed `Hosts/SurfaceVocabulary.cs` through `Evidence/EvidenceRail.cs`, every PROOF_GATES row is green (G1 restore, G2 `api doctor`/`resolve`, G3 `static build`, G4 `test run`, G5 `bridge verify`, G6 headless render-hash lanes, G7 `mmdc` render), the charter GAP_LEDGER stays fully CLOSED, and the charter `spec` gate passes on the full suite.

Residual host-bridge work is the charter DENSITY_BAR `SPIKE` set discharging against its page RESEARCH clusters; each exits when its bridge or native probe lands as a settled fence row rather than a re-opened gate:
- Embedding seam (`SurfaceHost`, `SurfaceFact`) — surface-hosts#RESEARCH [EMBED_SPIKE]/[WIN32_ROUTE].
- Embedded notification and picker capsule (`ToastRow` · `ToastOutcome`) — dialogs-notifications#RESEARCH [EMBEDDED_TOPLEVEL].
- Cross-boundary transfer (`DragPayload`) — input-interaction#RESEARCH [EMBEDDED_DRAG]/[GESTURE_TRIGGERS]/[POINTER_CAPTURE]/[ROTATE_GESTURE]/[CLIPBOARD_WRITE]; the Avalonia 12 data-transfer reshape moves the structured clipboard write onto `IClipboard.SetDataAsync(IDataTransfer)` over `DataFormat` application formats.
- Series render fidelity (`ChartSeriesSpec`) — charts-dashboards#RESEARCH [SERIES_RENDER]/[GEO_PAYLOAD].
- Offscreen render-byte identity and break (`VisualDestination` · `ExportDestination` · `VisualCodec`) — visuals-offscreen#RESEARCH [PARAGRAPH_BREAK]/[SERIES_RENDER]; the color-reproject member surface is confirmed against SkiaSharp.
- Headless evidence derivation (`EvidenceReceipt`, `ProofCheck`) — diagnostics-evidence#RESEARCH [HEADLESS_RUNNER]/[DEV_LOOP_STRIP].

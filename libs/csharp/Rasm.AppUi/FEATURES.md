# [APPUI_FEATURES]

Isolation-concept atlas for the product UI rail. Every concept rides one budgeted owner, and a new concept is a row, case, or policy value on that owner, never a new surface.

## [1]-[SURFACE_AND_SHELL]

| [INDEX] | [FEATURE] | [OWNER] | [PAGE#CLUSTER] |
| :-----: | --------- | ------- | -------------- |
| [1] | One shell mounts on any host through one mount transaction; no dispatch arm names a host API | `SurfaceHost` | surface-hosts#HOST_AXIS |
| [2] | Foreign-view embedding — retained NSView handoff, embedded platform policy, ordered lifecycle, defensive teardown | `EmbedCapsule` | surface-hosts#EMBED_CAPSULE |
| [3] | One UI scheduler boundary completes `UiSchedulerPort.Marshal`; `ObserveOn` rides `Ui` once inside binding capsules | `SurfaceScheduler` | surface-hosts#SCHEDULER_BOUNDARY |
| [4] | Per-RID native asset load-identity receipts at mount; wrong-RID load surfaces as a receipt, never a draw fault | `NativeAssets` | surface-hosts#NATIVE_ASSETS |
| [5] | Closed host fact union for scale, visibility, focus, appearance feeding activation, theme re-probe, DPI selection | `SurfaceFact` | surface-hosts#SCALE_FOCUS |
| [6] | Routing spine with deep-link grammar; two view-resolution hosts; viewport history rides the same grammar | `NavRequest` | shell-navigation#ROUTING_SPINE |
| [7] | Dockables fold from route rows; checkpoint cadence, drain flush, crash restore, external-dock surface, drop selector | `ShellDockFactory` | shell-navigation#DOCK_LAYOUTS |
| [8] | Chrome rows derive from intent keys per surface; macOS global menu and tray export; visibility matrix per host | `ShellChrome` | shell-navigation#SHELL_CHROME |
| [9] | One breakpoint table; behavior-attached responsive policy values; orthogonal to density tokens | `AdaptiveLayout` | shell-navigation#ADAPTIVE_LAYOUT |

## [2]-[SCREENS_AND_COMMANDS]

| [INDEX] | [FEATURE] | [OWNER] | [PAGE#CLUSTER] |
| :-----: | --------- | ------- | -------------- |
| [1] | One screen catalog row drives dockables, titles, automation names, route keys, headless proof lanes | `ScreenCatalog` | screens-activation#SCREEN_CATALOG |
| [2] | One activatable base — ref-counted activation scopes, suspend/resume, scoped disposal, drain row rank 10 | `ScreenBase` | screens-activation#ACTIVATION_SCOPES |
| [3] | Paced derived state through OAPH rows; one screen fault fold; distinct-before-throttle pacing | `DerivedOps` | screens-activation#DERIVED_STATE |
| [4] | Typed-rail validation lift into ReactiveUI.Validation; context validity gates commands, property text feeds adorners | `ScreenValidation` | screens-activation#VALIDATION_UX |
| [5] | Per-surface state snapshots; restore-on-activate merge; partition by id and surface; support-bundle contribution | `ScreenState` | screens-activation#SCREEN_STATE |
| [6] | One `CommandIntent` table is the only command vocabulary; menus, hotkeys, palette, deep links, remote verbs derive | `CommandIntent` | commands-availability#INTENT_TABLE |
| [7] | Availability algebra over degradation, validation, selection, busy folds into one `CanExecute` stream; macro verbs combine | `CommandGate` | commands-availability#AVAILABILITY_ALGEBRA |
| [8] | Total execution receipts; `Catch` rail makes outcome total; XxHash128 payload digest; sealed through the sink envelope | `CommandExecution` | commands-availability#EXECUTION_RECEIPTS |
| [9] | Palette, deep links, remote, and journal replay over one intent table; span-ranked fuzzy search; ControlService verbs as rows | `CommandProjections` | commands-availability#PALETTE_AND_REMOTE |

## [3]-[LIVE_DATA_AND_TABLES]

| [INDEX] | [FEATURE] | [OWNER] | [PAGE#CLUSTER] |
| :-----: | --------- | ------- | -------------- |
| [1] | Live-data spine — host fact to projection write to tag transition to delta fetch to `IChangeSet`; closed sourcing cases | `DataSource<TRow,TKey>` | live-data#DATA_SOURCES |
| [2] | Operator rows — dynamic predicate, comparer, page, window streams; classified-exclusion subtracts the deny projection | `PipelineInputs<TRow>` | live-data#CHANGE_PIPELINES |
| [3] | One UI-thread binding capsule; single `ObserveOn`; async-disposal drain; the one fault rail | `BindingCapsule` | live-data#BINDING_CAPSULE |
| [4] | Stat folds, change-audit evidence, suspend-resume; metrics derive from the same audit as the receipt stream | `LiveDataOps` | live-data#AGGREGATION_SPINE |
| [5] | One column metadata family drives columns, filter, masking, edit, export; classified column never enters filter or export | `TableColumnRow<TRow>` | tables-hierarchy#GRID_SUBSTRATE |
| [6] | Serializable collection-view snapshot applied in one `DeferRefresh`; paged and virtualized window restore | `TableViewState` | tables-hierarchy#VIEW_STATE |
| [7] | Projection cases fold to one flat virtualized `TreeRow` stream on the free DataGrid; TreeDataGrid rejected | `TableProjection<TRow,TKey>` | tables-hierarchy#TREE_FLATTEN |
| [8] | Grid-edit commit to `StoreOp.Upsert` and `DocumentTransaction`; delimited exports through Persistence Sep lane | `TableCommit<TRow>` | tables-hierarchy#GRID_COMMIT |

## [4]-[INSPECTOR_AND_EDITING]

| [INDEX] | [FEATURE] | [OWNER] | [PAGE#CLUSTER] |
| :-----: | --------- | ------- | -------------- |
| [1] | PropertyGrid admission capsule; descriptor filters, focus receipts; operation controls as command-table intents | `InspectorSurface` | inspector-editing#INSPECTOR_SURFACE |
| [2] | Ranked editor rows incl. value-object and quantity; metadata-driven generated-owner detection; first-accepting rank walk | `EditorFactory` | inspector-editing#EDITOR_FACTORIES |
| [3] | Typed admission rail with preview-versus-commit law; accumulating `Validation`; executing-vs-executed debounce | `EditGate` | inspector-editing#COMMIT_VALIDATION |
| [4] | Options-inspector to user-settings write and `ReloadReceipt` banner; immutable-record-draft route | `OptionsInspector<T>` | inspector-editing#OPTIONS_INSPECTOR |
| [5] | Side-by-side conflict projection over Persistence conflict receipts; seven resolution intent keys | `ConflictPane<TReceipt>` | inspector-editing#CONFLICT_RESOLUTION |
| [5a] | Three-way base-local-remote hunk differ (auto-merge non-conflicting hunks), live merge-preview re-solve over per-hunk choices, geometry-diff viewport (added/removed/modified ids + dual `Viewpoint` cameras) | `ThreeWay` · `GeometryDiff` | inspector-editing#CONFLICT_RESOLUTION |
| [6] | Grammar-scoped code panes — folding, completion, search, TextMate theme sync to tokens; fenced code only | `CodePane` | inspector-editing#CODE_EDITING |

## [5]-[CHARTS_AND_VISUALS]

| [INDEX] | [FEATURE] | [OWNER] | [PAGE#CLUSTER] |
| :-----: | --------- | ------- | -------------- |
| [1] | Chart series rows incl. geo-map dispatching onto canvas rows; each row is its own render-hash twin | `ChartSeriesSpec` | charts-dashboards#SERIES_TABLE |
| [2] | Axis scales; `CompositeFormat` labels; shared-scale pairing; threshold sections | `ChartAxisKind` | charts-dashboards#AXES_SECTIONS |
| [3] | One interaction policy — zoom, anchors, intent routing, `ZoomBorder` dashboard canvas with view history | `ChartPolicy` | charts-dashboards#CHART_INTERACTION |
| [4] | Windowed and LTTB-downsampled stream binding; `SyncContext`-locked swap; feed rows | `ChartFolds` | charts-dashboards#STREAM_BINDING |
| [5] | Dashboard tile union; placement fold; benchmark and activity-timeline named rows; versioned `DashboardLayout` blobs | `DashboardTile` | charts-dashboards#DASHBOARD_TILES |
| [6] | Borrowed/owned Skia draw capsule on one `Fin` rail; FX table; in-tree `ICustomDrawOperation` vehicle | `DrawSource` | visuals-offscreen#DRAW_CAPSULE |
| [7] | Host-agnostic thumbnails with blob-backed cache, async display, DPI variants; host bitmap converts once at the seam | `Thumbnails` | visuals-offscreen#THUMBNAIL_PIPELINE |
| [8] | Receipt-keyed zoomable previews; geometry families from Compute receipt streams as rows; HUD stays host-side | `PreviewRow<TReceipt>` | visuals-offscreen#PREVIEW_SURFACES |
| [9] | Content-hashed encode identity; codec axis; color-managed float reproject with color-space receipt tag; XxHash128 frame hash | `VisualCodec` | visuals-offscreen#ENCODE_IDENTITY |
| [10] | Paged `SKDocument` export — `FlowFold` over `FlowBlock` under `BreakRule` with `HeaderFooterBand`; three destinations | `FlowBlock` | visuals-offscreen#DOCUMENT_EXPORT |
| [10a] | Office XLSX/PPTX/DOCX export — typed `OfficeSheet` content (table/chart/image/rich-text), embedded font faces in the OOXML part, deterministic pagination reusing the `FlowFold` break algebra | `OfficeExport` | visuals-offscreen#DOCUMENT_EXPORT |
| [11] | Skia diagram + deck.gl-class geo kinds LiveCharts cannot supply — sankey/treemap/waterfall/funnel/parallel-coordinates/radar/network/gantt/sunburst diagrams plus hexbin/arc/trip/3D-extrusion/terrain geo layers as `[Union]` cases, squarified worst-aspect treemap packing, equirectangular geo projection, per-cell render-hash twin per kind | `CustomVisual` | custom-visuals#SKIA_KINDS |
| [12] | Suite-wide wide-gamut color-space vocabulary (srgb/p3/rec2020/scrgb-float) with ICC-primary working space + half-float encode surface | `ColorSpaceAxis` | custom-visuals#COLOR_SPACE |
| [13] | Custom-tile placement + analytical-flow named dashboard over the residence-selected feed | `DashboardTile.Custom` | charts-dashboards#DASHBOARD_TILES |
| [14] | Cross-tile linked brushing — one `FilterState` subject re-filters every non-source tile through the dynamic-predicate `Filter` on the shared `Connect()` spine | `CrossFilter` | charts-dashboards#DASHBOARD_TILES |
| [14a] | Multi-dimensional categorical cross-filter as word-aligned `ulong[]` bitsets per (dimension,value) cell — O(changed-words) AND-of-unions, never an O(rows) re-scan; spatial-polygon brush as an even-odd ray-cast `Contains` ring | `DimensionIndex<TRow,TKey>` · `PolygonBrush` | charts-dashboards#DASHBOARD_TILES |
| [15] | Dashboard board layout plus brush-state persistence over the concrete `DockSerializer` (`Dock.Serializer.SystemTextJson`), distinct from shell pane docking; restore re-applies the brush | `BoardState` | charts-dashboards#STREAM_BINDING |
| [16] | Live geometry overlay — Persistence `SpatialDiff` change-set folds onto `HeatLandSeries.Lands` in place, an incremental land swap never a re-load | `GeoOverlay` | charts-dashboards#SERIES_TABLE |

## [6]-[THEME_AND_TYPOGRAPHY]

| [INDEX] | [FEATURE] | [OWNER] | [PAGE#CLUSTER] |
| :-----: | --------- | ------- | -------------- |
| [1] | One frozen token catalogue feeding five consumers; OKLab ramp mix; orthogonal variant and density resolve | `TokenRow` | theme-tokens#TOKEN_CATALOG |
| [2] | Variant rows with host-agnostic appearance probe folding; per-surface override delegate | `ThemeVariantRow` | theme-tokens#VARIANT_AXIS |
| [3] | Density rows selecting metric columns orthogonally; Fluent compact resource swap on the one rail | `DensityRow` | theme-tokens#DENSITY_AXIS |
| [4] | Atomic theme swap re-resolving the full catalogue; token-diff receipts sink through the evidence envelope | `ThemeCell` | theme-tokens#CONTROL_THEMES |
| [4a] | Perceptually-uniform data-colormap catalog (viridis/magma/cividis perceptual + turbo) sampled over the one OKLab mix; `HeatMap` feeds LiveCharts heat/geo series | `Colormap` | theme-tokens#TOKEN_CATALOG |
| [4b] | HDR tone-mapping (ACES/Reinhard/Hable LUT operators), ICC-profile working spaces via `SKColorSpace.CreateIcc`, OCIO config as profile-byte source | `VisualCodec.ColorPolicy` · `ToneMap` | visuals-offscreen#ENCODE_IDENTITY |
| [5] | Typography roles; every size, weight, and feature literal traces here; one resolved `TextStyleRow` for five consumers | `TypographyRole` | typography-shaping#ROLE_AXIS |
| [6] | Deterministic embedded-Inter admission; ranked per-platform fallback chains; one boot-time font registration | `FontChain` | typography-shaping#FONT_ADMISSION |
| [7] | One HarfBuzz shaping rail before every Skia glyph; per-role feature tags; libHarfBuzzSharp load-identity proof | `ShapingSurface` | typography-shaping#SHAPING_RAIL |
| [8] | Markdig AST to role-keyed rows; one typed-traversal fold shared with inspector and SVG walks; math and diagrams excluded | `MarkdownProjection` | typography-shaping#MARKDOWN_PROJECTION |
| [9] | Baseline-grid metrics; `MeasureText` and the shaped rail only; tabular-numeral advance proof | `TextMetricsPolicy` | typography-shaping#TEXT_METRICS |

## [7]-[ICONS_AND_ASSETS]

| [INDEX] | [FEATURE] | [OWNER] | [PAGE#CLUSTER] |
| :-----: | --------- | ------- | -------------- |
| [1] | Icon sourcing cases with rank-column fallback walk; off-axis size aborts; per-row tint through token key | `IconSource` | icons-assets#ICON_AXIS |
| [2] | Retained SVG pipeline — scene graph, animation invalidation, hit testing; process-static cache; motion-pumped frames | `SvgPipeline` | icons-assets#SVG_PIPELINE |
| [3] | Async raster rows with disk/RAM cache, DPI variants, placeholder/error fallback keys; one global loader; storage-scoped fetch lane | `RasterAssets` | icons-assets#RASTER_ASSETS |
| [4] | nameof-derived `AssetKey` vocabulary shared across pages; avares admission with preload receipts; geo asset routing | `AssetCatalog` | icons-assets#ASSET_CATALOG |

## [8]-[DIALOGS_AND_INPUT]

| [INDEX] | [FEATURE] | [OWNER] | [PAGE#CLUSTER] |
| :-----: | --------- | ------- | -------------- |
| [1] | Dialog intents with typed `Fin` results, dismissal as a value; boot crash-restore offer; deadline-typed progress | `DialogIntent` | dialogs-notifications#DIALOG_INTENTS |
| [2] | Per-surface session topology rows — stacking, blur, positioner, `TopLevelResolver`; one `Interaction` seam per root | `DialogTopology` | dialogs-notifications#SESSION_ALGEBRA |
| [3] | Toast suppression fold over phase and degradation with queued/dropped receipts; no notification-history store | `ToastGate` | dialogs-notifications#NOTIFICATIONS |
| [4] | Host-agnostic pickers with format-derived filters; anchored/centered positioner rows; headless folds to typed fault | `PickOps` | dialogs-notifications#PICKERS_HOST_MODALITY |
| [5] | Hotkey derivation from the command table with platform chord transform; first-claimant conflict drop; no second key table | `GesturePolicy` | input-interaction#HOTKEY_DERIVATION |
| [6] | Admitted behavior trigger and action rows; rejected file/network/http triggers; intervals from motion vocabulary | `BehaviorRail` | input-interaction#BEHAVIOR_RAIL |
| [7] | Pan-zoom canvas family — dashboard and preview rows; one zoom owner per canvas; view-state round-trip | `PanZoomRow` | input-interaction#POINTER_GESTURES |
| [8] | Typed drag payloads and clipboard codec rows on the validation rail; format-probe before paste; text copy-only | `DragPayload` | input-interaction#DRAG_CLIPBOARD |
| [9] | Alternative-input fabric folding SpaceMouse/controller/gaze/switch/voice/MIDI samples onto the one `CommandIntent` table, plus symmetric device output to motion-controller/CNC/robot/haptic over normalized `DeviceAxis`; SDKs are `SurfaceSeam` delegate columns | `InputFabric` · `InputDevice` · `DeviceOutput` | input-interaction#INPUT_FABRIC |

## [9]-[MOTION_AND_ACCESSIBILITY]

| [INDEX] | [FEATURE] | [OWNER] | [PAGE#CLUSTER] |
| :-----: | --------- | ------- | -------------- |
| [1] | Motion tokens with reduced pairs and spring algebra; every duration and easing literal traces here; host parity | `MotionToken` | motion-tokens#MOTION_AXIS |
| [2] | Plan rows binding transitions, charts, zoom, clocks; reduction applies once; deterministic motion clocks | `MotionApplication` | motion-tokens#MOTION_APPLICATION |
| [3] | Frozen nine-row `ProgressPhase`-to-`MotionToken` map; conformance sweep fails on Compute phase drift | `PhaseMotion` | motion-tokens#PHASE_MAPPING |
| [4] | One global reduced-motion degrade switch; host-agnostic probe rows; opacity-only reduced render | `ReducedMotion` | motion-tokens#REDUCED_MOTION |
| [5] | Catalog-sourced automation identity and live-region announcements; `Synthesized` peer-presence column | `AccessOps` | accessibility#AUTOMATION_PEERS |
| [5a] | 3D-scene accessibility tree mirroring the scene hierarchy with `Nearest`/`Step` spatial focus navigation and `FocusGeometry` peer projection; distance-attenuated stereo-panned spatial-audio cues | `SceneAccessNode` · `SpatialCue` | accessibility#AUTOMATION_PEERS |
| [6] | Tab-order, trap, refocus law over attached navigation; access keys fold over the command table; navigation modes | `FocusOps` | accessibility#KEYBOARD_NAV |
| [7] | The suite's single WCAG luminance gate with floor rows; theme tokens emit pairs, consume receipts, never ratios | `ContrastGate` | accessibility#CONTRAST_GATE |
| [8] | Per-row compliance audit across the variant-density grid; executed by the evidence derivation engine | `AccessProof` | accessibility#COMPLIANCE_PROOF |

## [10]-[LOCALIZATION_AND_EVIDENCE]

| [INDEX] | [FEATURE] | [OWNER] | [PAGE#CLUSTER] |
| :-----: | --------- | ------- | -------------- |
| [1] | Locale rows on inbox resx/ICU/NodaTime with a pseudo-locale conformance row; authoritative flow column | `LocaleRow` | localization-culture#LOCALE_AXIS |
| [2] | nameof-derived string tables with PropertyGrid `ILocalizationService` bridge; rejected MS.Extensions.Localization | `LocaleStrings` | localization-culture#STRING_TABLES |
| [3] | Atomic culture composition under the options reload class; `CompositeFormat` and NodaTime patterns; ambient culture absent | `LocaleRuntime` | localization-culture#CULTURE_COMPOSITION |
| [4] | RTL mirroring at surface root by inheritance with icon exemption; directional rows re-join root flow | `MirrorPolicy` | localization-culture#RTL_MIRRORING |
| [4a] | 3D-annotation complex-script shaping (bidi flow + RTL ligature/contextual-alternate feature tags) through the one HarfBuzz shaping rail; locale-aware ISO/ANSI/JIS title-block field text; live caption with translate-to-target-locale stream | `ShapedAnnotation` · `LiveCaption` | localization-culture#RTL_MIRRORING |
| [5] | Evidence union sealed through the HLC sink envelope; every sibling receipt stream folds in, zero new emitters | `EvidenceReceipt` | diagnostics-evidence#RECEIPT_UNION |
| [6] | Correlation join with typed skew bands and model-result provenance projection; `Overlaps` band algebra | `EvidenceJoin` | diagnostics-evidence#CORRELATION_JOIN |
| [7] | Host-agnostic render-hash capture lanes; `ForceRenderTimerTick` deterministic frames; scale-attributable hash | `Captures` | diagnostics-evidence#CAPTURE_LANES |
| [8] | Catalog-derived headless proof matrix with command-journal replay under virtual time; one session per assembly | `ProofEngine` | diagnostics-evidence#HEADLESS_DERIVATION |
| [9] | Debug hot-reload loop with dispatcher-lag probe; manual-reload intent verbs; Release closure strip; markup-loader Debug pin | `DevLoop` | diagnostics-evidence#DEV_LOOP |
| [9a] | User-facing performance HUD (frame/GPU/VRAM/triangle + per-node solve from the receipt envelopes), flamegraph fold over receipt durations, solve time-travel scrub with per-node diff, cross-machine replay-verify by payload digest, in-app REPL over the one `CommandProjections.Invoke` route | `HudSample` · `FlameNode` · `SolveScrub` · `Repl` | diagnostics-evidence#DEV_LOOP |

## [11]-[VIEWPORT_AND_DOCUMENTS]

| [INDEX] | [FEATURE] | [OWNER] | [PAGE#CLUSTER] |
| :-----: | --------- | ------- | -------------- |
| [1] | Infinite-viewport render-graph pass-DAG over a host-shared `GRContext` lease with frame-budget invariant and a 2D-Skia composite fallback shipping today | `RenderGraph` · `RenderPass` | viewport-pipeline#RENDER_GRAPH |
| [2] | GPU-virtualized geometry — meshlet cluster-LOD, screen-space-error pop-free cull, bindless residency, mesh-shader draw; `MeshSource` projects off the Compute `GeometryPayload`, never re-tessellated | `MeshletCluster` | viewport-pipeline#GEOMETRY_VIRTUAL |
| [3] | Out-of-core streaming geometry with VRAM-budget LRU residency, velocity-extrapolation predictive prefetch, massive instancing as one draw call | `ResidencyBudget` | viewport-pipeline#RESIDENCY_BUDGET |
| [4] | Hardware path tracing — SAH BVH build/refit, ReSTIR reservoirs, sample-count progressive convergence, edge-aware denoise; CPU reference oracle ships today | `PathTracePass` | viewport-pipeline#PATH_TRACE |
| [5] | Simulation visualization — isosurface/volume/streamline/glyph/deformation/mesh-quality/parallel-coords off Compute field receipts, transient playback by frame index under the deterministic motion clock | `SimVisual` | viewport-pipeline#SIM_VISUAL |
| [6] | Portable BCF-compatible viewpoint receipt — camera, section-box, visibility, color-override, selection round-tripping to and from an open BCF topic | `Viewpoint` | viewport-pipeline#VIEWPOINT_CODEC |
| [7] | Locale-aware sheet set with ISO/ANSI/JIS title-block templating; sheet-bounds fault at compose; composes into the `FlowBlock` PDF page run | `SheetSet` · `SheetSize` · `TitleBlock` | drafting-sheets#SHEET_SET |
| [8] | 3D-to-2D hidden-line viewport frame sharing the `Viewpoint` camera basis; standard-view presets; millimeter-to-millimeter scale rows | `Viewport2D` · `ProjectionBasis` | drafting-sheets#PROJECTION |
| [9] | Dimensioning and GD&T annotation vocabulary as typed records; ASME Y14.5 feature-control frame; values format through the locale quantity edge | `Dimension` · `Annotation` | drafting-sheets#DIMENSIONING |
| [10] | Multi-format drawing emit — PDF (`SKDocument`) and SVG (`SKSvgCanvas`) ship today, DWG/DXF entity-writer SPIKE-gated; delivers through the one `VisualDestination` | `DraftEmit` | drafting-sheets#DRAFT_EMIT |
| [11] | Reproducible notebook cell union with a `CapabilityPin` per code/chart cell; an unpinned cell faults at evaluate; markdown/chart/render route to existing owners | `NotebookCell` · `CapabilityPin` | notebook-document#CELL_MODEL |
| [12] | Dependency-aware recompute — DAG from declared cell inputs, dirty-closure propagation O(downstream), cycle fault at build | `DependencyGraph` | notebook-document#DEPENDENCY_GRAPH |
| [13] | CRDT co-editing — HLC-ordered op-log, fractional-index insertion, tombstone deletion, last-writer-wins content register; commutative-associative-idempotent merge over the Persistence op-log | `NotebookCrdt` | notebook-document#CRDT_COEDIT |
| [14] | Export-to-replay bundle — self-contained pinned capabilities and input blobs, bit-identity verify by XxHash128 over the diagnostics deterministic-replay law | `ReplayBundle` | notebook-document#REPLAY_BUNDLE |
| [15] | Keyframe-track union over params/cameras/visibility/transient-field-index/color with motion-token easing; logarithmic bracket sample | `Track` · `Keyframe<T>` | animation-timeline#TRACK_MODEL |
| [16] | Track composition under a deterministic frame-indexed playhead; loop/ping-pong as playhead policy; one composed `TimelineSample` drives viewport/inspector/simulation | `Timeline` · `Playhead` | animation-timeline#TIMELINE |
| [17] | Re-entrant frame-indexed scrub with no delta drift; kinematic playback and transient-field scrub share one playhead through the scheduler boundary | `Scrub` | animation-timeline#SCRUB |
| [18] | Offline walkthrough — deterministic frame-sequence render through the supplied frame delegate and the visuals codec; per-frame content hash; video-container muxing is the research row | `Walkthrough` | animation-timeline#WALKTHROUGH |

## [12]-[ROUTING]

- Mechanics route to the owning page cluster cited in each `[PAGE#CLUSTER]` cell; the page card legislates, the table enumerates.
- Owner budget and realization state route to the charter DENSITY_BAR through the `[OWNER]` cell — the DENSITY_BAR `[STATE]` column is the sole state authority.
- Wire shapes route to the TS_PROJECTION clusters on commands-availability#TS_PROJECTION, diagnostics-evidence#TS_PROJECTION, and viewport-pipeline#TS_PROJECTION; command intent, availability, invocation, payload, outcome, and receipt plus the evidence and viewpoint/frame-receipt wire shapes transcribe the camelCase Strict emission, and receipts bind as envelope payload type parameters. The portable `Viewpoint` (BCF-compatible) and `FrameReceipt` cross for a web viewer and the live performance HUD; the GPU pass internals never cross the wire.
- Hosts: avalonia-desktop, rhino-panel, rhino-modal, gh2-companion, sidecar-shell, headless; web-browser stays designed-only — TS owns web UI while `CommandIntent` keys and `EvidenceReceipt` cases serialize for it through commands-availability#TS_PROJECTION and diagnostics-evidence#TS_PROJECTION.
- Sourcing-as-rows: every source axis is one extensible row — `DataSource` feed cases, `ChartStream` feed rows, `IconSource` cases, `AssetKind` origins, `ThemeVariantRow` probe column per surface, `FontChain` per-RID rows, `LocaleRow` culture rows, `ClipboardRow` format codecs, `NativeAssetRow` per-RID rows, `RenderPass`/`GpuBackend` rows, `SheetSize`/`DraftFormat` rows, `NotebookCell`/`CellOutput` cases, `Track`/`PlaybackMode` rows, `InputDevice`/`DeviceOutput` cases, `ColorSpaceAxis`/`Colormap`/`ToneMap` rows; a new source is one row, never an ad-hoc branch.
- Host-agnostic sourcing law: `SurfaceSeam` is the host-delegate column record; every probe, marshal, mount, fact, capture, toast-pipe, pick-pipe, theme-probe, and reduced-motion-probe is a delegate column bound at composition. No dispatch arm names a host API — a host crossing is a `SurfaceSeam` column, a port delegate, or a `Func<...>` field (surface-hosts#HOST_AXIS, dialogs-notifications#SESSION_ALGEBRA, theme-tokens#VARIANT_AXIS, motion-tokens#REDUCED_MOTION).
- Unified observability: every page-owner contributes traces and metrics inward through one `AppUiTelemetry.Contribute(version, instruments)` to `TelemetryContributorPort`; instrument names stay owned by the contributing page as constants. The OTLP backend is AppHost-owned and env-parameterized; AppUi never names a backend. Every receipt sinks through the one `ReceiptSinkPort` HLC envelope; the `EvidenceReceipt` union and the correlation join are the only evidence vocabulary, and a generic receipt or ledger abstraction is rejected.
- Unified hardening: `DataClassification` deny projection subtracts classified rows from live-data, table filter and export, and thumbnail bundles; `DegradationLevel`/`Capability.HostDocument` fold host-targeting commands unavailable structurally on `LocalOnly`/`Suspended`; `ToastGate` suppresses notices under terminal and paused phases; every dialog, edit, command, and asset fault is a typed `[Union]` band on the doctrine `Expected` shape; cancellation rides `EnvIO`/`CancellationToken` on command run and table commit; credential material is barred from the emission grammar. The rhino-gh2, web/ux, and remote/local matrix resolves through `SurfaceHost` cases, `DegradationLevel`, and `SurfaceSeam` delegates.
- Packaging capture (under-util to row): DynamicData `ExpireAfter`/`LimitSizeTo`/`Page`/`Virtualise`/`MergeMany`/`GroupWithImmutableState`/`TransformToTree`/`AsyncDisposeMany` route to live-data and tables; LiveCharts `GeoMap`/gauge/box/candlestick plus `ZoomBorder` history route to charts; Dock `RegisterExternalDockSurface`/`GlobalDockTarget`/`HostWindowFactory` route to shell-navigation#DOCK_LAYOUTS; Behaviors `ListReorderDragBehavior`/`Throttle`/`Debounce`/`ObservableStreamBehavior` route to input; DialogHost `IsMultipleDialogsEnabled`/`BlurBackground`/`IDialogPopupPositioner` route to dialogs; Headless `Dispatch`/`SetRenderScaling`/`ForceRenderTimerTick`/`CaptureRenderedFrame` route to evidence; AvaloniaEdit `FoldingManager`/`CompletionWindow`/`SearchPanel`/`TextMate.SetTheme` route to inspector-editing#CODE_EDITING; PropertyGrid `AbstractCellEditFactory`/`MetadataLookup`/`ILocalizationService` route to inspector and localization; Markdig `TrackTrivia`/`UseFootnotes`/`UseYamlFrontMatter`/`Descendants<T>` route to typography; SkiaSharp `SKDocument`/`SKRuntimeEffect`/`SKShaper`/`SKSvgCanvas`/`SKColorSpace.CreateIcc`/`SKColorFilter.CreateTable` route to visuals, typography, and drafting; SkiaSharp GPU `GRContext`/`GRMtlBackendContext`/`GRVkBackendContext`/`ISkiaSharpApiLease.TryLeasePlatformGraphicsApi` route to viewport-pipeline; OpenXML `SpreadsheetDocument`/`PresentationDocument`/`WordprocessingDocument` route to visuals-offscreen#DOCUMENT_EXPORT; AsyncImageLoader `DiskCachedWebImageLoader`/`AdvancedImage.FallbackImage` route to icons; HotAvalonia `ExcludeReferences` routes to dev-loop.

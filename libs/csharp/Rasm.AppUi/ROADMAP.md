# [RASM_APPUI_ROADMAP]

Implementation transcribes the eighteen finalized planning pages in the charter [BUILD_ORDER](.planning/README.md). Every task exits against named page clusters; every exit is proven by a charter proof gate. Production source is absent; this roadmap is the complete path from manifest-backed project to proven rail.

## [1]-[START_GATES]

Implementation-start gates owned by this package — each unblocks its named clusters before transcription begins.

| [INDEX] | [GATE] | [PROOF_ROUTE] | [UNBLOCKS] |
| :-----: | ------ | ------------- | ---------- |
| [1] | Avalonia-12-in-Rhino NSView embedding spike | `uv run python -m tools.assay bridge verify --pattern avalonia_embed_pump` (+ `avalonia_embed_resize`, `avalonia_embed_render`) | surface-hosts#EMBED_CAPSULE, surface-hosts#SCHEDULER_BOUNDARY, surface-hosts#HOST_AXIS |
| [2] | `Rasm.AppUi.Tests` target row on the assay test rail | `uv run python -m tools.assay test run --target Rasm.AppUi.Tests` | every specs and render-hash gate below |
| [3] | HotAvalonia Release closure strip | `dotnet build libs/csharp/Rasm.AppUi/Rasm.AppUi.csproj -c Release` | diagnostics-evidence#DEV_LOOP |
| [4] | Embedded TopLevel service resolution | `tests/csharp/libs/Rasm.AppUi/scenarios/appui-embedded-toplevel.verify.csx` | dialogs-notifications#NOTIFICATIONS, dialogs-notifications#PICKERS_HOST_MODALITY |
| [5] | Host-object drag across the NSView boundary | `libs/csharp/Rasm.AppUi/scenarios/embedded-drag.verify.csx` under live RhinoWIP | input-interaction#DRAG_CLIPBOARD |
| [6] | macOS reduce-motion preference probe | `libs/csharp/Rasm.AppUi/scenarios/reduced-motion-probe.verify.csx` | motion-tokens#REDUCED_MOTION |
| [7] | VoiceOver reach across the embedded root | `tests/csharp/libs/Rasm.Rhino/UI/scenarios/avalonia-embed-a11y.verify.csx` | accessibility#AUTOMATION_PEERS |

Every remaining research row resolves through its page's RESEARCH table — `uv run python -m tools.assay api query` decompile probes and named scratch probes — before its gated cluster is transcribed. An unresolved row blocks the cluster; the charter [FILE_PROCESS](.planning/README.md) makes the block mechanical.

## [2]-[IMPLEMENTATION_TASKS]

Tasks in charter BUILD_ORDER. A task exits when its clusters are transcribed verbatim, the collapse scan passes, and the named gates are green.

[VOCABULARY_SPINE]:
- Exits: surface-hosts#HOST_AXIS (vocabulary fences), surface-hosts#SCALE_FOCUS; motion-tokens#MOTION_AXIS, motion-tokens#MOTION_APPLICATION, motion-tokens#PHASE_MAPPING, motion-tokens#REDUCED_MOTION; typography-shaping#ROLE_AXIS, typography-shaping#FONT_ADMISSION, typography-shaping#SHAPING_RAIL, typography-shaping#MARKDOWN_PROJECTION, typography-shaping#TEXT_METRICS; icons-assets#ASSET_CATALOG, icons-assets#RASTER_ASSETS, icons-assets#ICON_AXIS, icons-assets#SVG_PIPELINE; theme-tokens#TOKEN_CATALOG, theme-tokens#VARIANT_AXIS, theme-tokens#DENSITY_AXIS; localization-culture#LOCALE_AXIS, localization-culture#STRING_TABLES, localization-culture#CULTURE_COMPOSITION, localization-culture#RTL_MIRRORING.
- Proof: static + specs; icon materialize rides the render-hash lane; PhaseMotion conformance sweep asserts map keys equal `ProgressPhase.Items`.

[HOST_MOUNT]:
- Exits: surface-hosts#HOST_AXIS (dispatch), surface-hosts#EMBED_CAPSULE, surface-hosts#SCHEDULER_BOUNDARY, surface-hosts#NATIVE_ASSETS; theme-tokens#CONTROL_THEMES.
- Proof: bridge (embed pump/resize/render scenarios) + specs + render-hash variant sweep; `Surfaces.Mount` lands with the `ClockPolicy` retype per the ledger conformance ruling.

[COMMAND_AND_INPUT]:
- Exits: commands-availability#INTENT_TABLE, commands-availability#AVAILABILITY_ALGEBRA, commands-availability#EXECUTION_RECEIPTS, commands-availability#PALETTE_AND_REMOTE, commands-availability#TS_PROJECTION; input-interaction#HOTKEY_DERIVATION, input-interaction#BEHAVIOR_RAIL, input-interaction#POINTER_GESTURES, input-interaction#DRAG_CLIPBOARD.
- Proof: specs (deck freeze, conflict fold, receipt totality, palette ranking, wire round-trip) + bridge (embedded-drag).

[SCREENS_AND_LIVE_DATA]:
- Exits: screens-activation#SCREEN_CATALOG, screens-activation#ACTIVATION_SCOPES, screens-activation#DERIVED_STATE, screens-activation#VALIDATION_UX, screens-activation#SCREEN_STATE; live-data#DATA_SOURCES, live-data#CHANGE_PIPELINES, live-data#BINDING_CAPSULE, live-data#AGGREGATION_SPINE.
- Proof: specs — activation/suspend law, snapshot merge, fake-deterministic rows under `VirtualTimeScheduler`, single-`ObserveOn` law.

[CONTENT_SURFACES]:
- Exits: tables-hierarchy#GRID_SUBSTRATE, tables-hierarchy#VIEW_STATE, tables-hierarchy#TREE_FLATTEN, tables-hierarchy#GRID_COMMIT; inspector-editing#INSPECTOR_SURFACE, inspector-editing#EDITOR_FACTORIES, inspector-editing#COMMIT_VALIDATION, inspector-editing#OPTIONS_INSPECTOR, inspector-editing#CONFLICT_RESOLUTION, inspector-editing#CODE_EDITING; dialogs-notifications#DIALOG_INTENTS, dialogs-notifications#SESSION_ALGEBRA, dialogs-notifications#NOTIFICATIONS, dialogs-notifications#PICKERS_HOST_MODALITY.
- Proof: specs (tree-flatten fold, DeferRefresh batching, editor rank walk, preview-versus-commit law, toast gate totality) + bridge (embedded TopLevel).

[SHELL_AND_VISUALS]:
- Exits: shell-navigation#ROUTING_SPINE, shell-navigation#DOCK_LAYOUTS, shell-navigation#SHELL_CHROME, shell-navigation#ADAPTIVE_LAYOUT; charts-dashboards#SERIES_TABLE, charts-dashboards#AXES_SECTIONS, charts-dashboards#CHART_INTERACTION, charts-dashboards#STREAM_BINDING, charts-dashboards#DASHBOARD_TILES; visuals-offscreen#DRAW_CAPSULE, visuals-offscreen#THUMBNAIL_PIPELINE, visuals-offscreen#PREVIEW_SURFACES, visuals-offscreen#ENCODE_IDENTITY, visuals-offscreen#DOCUMENT_EXPORT.
- Proof: specs (dock serialize-restore round-trip, Lttb fold, layout admit) + render-hash sweeps per `ChartSeriesSpec` row and per named dashboard.

[ACCESSIBILITY_RAIL]:
- Exits: accessibility#AUTOMATION_PEERS, accessibility#KEYBOARD_NAV, accessibility#CONTRAST_GATE, accessibility#COMPLIANCE_PROOF.
- Proof: specs — contrast floor rows over the candidate pairs, audit sweep across the typed (variant, density) grid.

[EVIDENCE_RAIL]:
- Exits: diagnostics-evidence#RECEIPT_UNION, diagnostics-evidence#CORRELATION_JOIN, diagnostics-evidence#CAPTURE_LANES, diagnostics-evidence#HEADLESS_DERIVATION, diagnostics-evidence#DEV_LOOP, diagnostics-evidence#TS_PROJECTION.
- Proof: full derived proof matrix green (`ProofEngine.Derive` across catalog × checks × grid), render-hash regression against blob-lane baselines, command-journal replay deterministic under `FakeTimeProvider`, bridge scenarios for host capture lanes.

## [3]-[COMPLETION_SIGNALS]

- Every charter BUILD_ORDER row closed with its gates green; `uv run python -m tools.assay static build` clean on the package closure.
- `uv run python -m tools.assay test run --target Rasm.AppUi.Tests` green, including the derived proof matrix, contrast sweep, motion conformance, and render-hash lanes.
- Bridge scenarios for the four host seams pass under live RhinoWIP.
- The charter [GAP_LEDGER](.planning/README.md) stays fully CLOSED; no implementation re-opens a routed gap.

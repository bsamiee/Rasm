# [APPUI_PLANNING]

Rasm.AppUi has zero consumers; the implementation is full-capability with no holding back. These pages are decision-complete blueprints an implementation agent transcribes — never re-designed downstream. The package owns the product UI rail: surface hosts, shell and navigation, screens, commands, live data, tables, inspectors, charts, offscreen visuals, theme and typography, icons, dialogs, input, motion, accessibility, localization, and evidence — consuming AppHost ports, Persistence queries, and Compute receipts as settled vocabulary.

## [1]-[PAGE_INDEX]

| [INDEX] | [PAGE] | [OWNS] | [STATE] |
| :-----: | ------ | ------ | :-----: |
| [1] | [surface-hosts](surface-hosts.md) | SurfaceHost axis; one shell mounted on any host; marshal rows | finalized |
| [2] | [shell-navigation](shell-navigation.md) | Shell composition, routing, dock layouts with checkpoint/restore | finalized |
| [3] | [screens-activation](screens-activation.md) | Screen family, activation scopes, lifecycle | finalized |
| [4] | [commands-availability](commands-availability.md) | One CommandIntent table; availability; hotkeys; palette; deep links | finalized |
| [5] | [live-data](live-data.md) | DataSource axis; change-set engine; the live-data spine | finalized |
| [6] | [tables-hierarchy](tables-hierarchy.md) | TableProjection rows; virtualization; tree-flatten fold | finalized |
| [7] | [inspector-editing](inspector-editing.md) | EditorFactory rows; validation-to-save gate; options inspector | finalized |
| [8] | [charts-dashboards](charts-dashboards.md) | ChartSeriesSpec rows incl. geo; dashboard composition | finalized |
| [9] | [visuals-offscreen](visuals-offscreen.md) | Offscreen rendering; render-hash; document export | finalized |
| [10] | [theme-tokens](theme-tokens.md) | ThemeVariant and Density row families; token resolution | finalized |
| [11] | [typography-shaping](typography-shaping.md) | TypographyRole rows; shaping; markdown projection | finalized |
| [12] | [icons-assets](icons-assets.md) | IconSource rows with fallback ranks; asset loading | finalized |
| [13] | [dialogs-notifications](dialogs-notifications.md) | DialogIntent rows; session algebra; notification policy | finalized |
| [14] | [input-interaction](input-interaction.md) | Pointer/keyboard/gesture rows; pan-zoom canvases | finalized |
| [15] | [motion-tokens](motion-tokens.md) | MotionToken rows with reduced pairs; phase mapping | finalized |
| [16] | [accessibility](accessibility.md) | Automation peers; contrast governance; reduced-motion law | finalized |
| [17] | [localization-culture](localization-culture.md) | LocaleRow; resx/ICU/NodaTime culture composition | finalized |
| [18] | [diagnostics-evidence](diagnostics-evidence.md) | EvidenceReceipt union; correlation join; headless proof | finalized |

## [2]-[WIRE_PAGES]

commands-availability · diagnostics-evidence (each carries exactly one TS_PROJECTION cluster).

## [3]-[CATALOGUE_PENDING]

TextMateSharp grammar registry (transitive, unpinned) gets a page only if admitted; FluentIcons.Common vocabulary rides the api-fluenticons.md surface card.

## [4]-[GAP_LEDGER]

| [INDEX] | [GAP] | [CLOSED_BY] | [STATE] |
| :-----: | ----- | ----------- | :-----: |
| [1] | web-browser SurfaceHost stays designed-only; CommandIntent keys + EvidenceReceipt cases serialize for the TS web layer today | surface-hosts + commands-availability + diagnostics-evidence | CLOSED |
| [2] | CommandIntent collapses to ONE table row (intent, availability delegate incl. DegradationLevel input, Option<KeyGesture>, surface predicate); menus/toolbars/palette/deep-links/remote derive | commands-availability | CLOSED |
| [3] | ThemeVariant and Density are orthogonal row families composed at resolve time | theme-tokens | CLOSED |
| [4] | IconSource fallback order is a rank column | icons-assets | CLOSED |
| [5] | Dock layouts gain checkpoint-cadence + crash-restore columns | shell-navigation | CLOSED |
| [6] | ProgressPhase-to-MotionToken frozen mapping | motion-tokens | CLOSED |
| [7] | Options inspector composite (options to PropertyGrid to user-settings write to ReloadReceipt) | inspector-editing | CLOSED |
| [8] | Command-journal replay (headless surface + snapshot store + virtual time) | diagnostics-evidence + surface-hosts | CLOSED |
| [9] | Correlation drill-down: the evidence-view fold owns the three-stream join keyed correlation + HLC | diagnostics-evidence | CLOSED |
| [10] | MarkdownProjection rows (Markdig AST to TypographyRole fold to inlines; AvaloniaEdit owns code fences) | typography-shaping | CLOSED |
| [11] | DocumentExport cluster (SKDocument PDF/XPS through admitted Skia/HarfBuzz; zero new render packages) | visuals-offscreen | CLOSED |
| [12] | Model-result provenance projection (fold over receipts keyed by correlation) | diagnostics-evidence | CLOSED |
| [13] | Tree-flatten fold over the free DataGrid (TreeDataGrid rejected); grid-edit commit path to StoreOp.Upsert | tables-hierarchy | CLOSED |
| [14] | Live-data spine composite (WatchPhase fact to projection to tag transition to delta fetch to IChangeSet); SourceCache keys ride IdentityPolicy | live-data | CLOSED |
| [15] | Palette fuzzy-search index row; explicit MessageBus rejection row | commands-availability | CLOSED |
| [16] | Contrast governance algorithm row (WCAG luminance gate) | accessibility | CLOSED |
| [17] | Notification suppression during drain/suspended phases | dialogs-notifications | CLOSED |
| [18] | GeoMap GeoJSON asset dependency routed through icons-assets | charts-dashboards + icons-assets | CLOSED |
| [19] | Conflict-resolution inspector row consuming Persistence conflict receipts | inspector-editing | CLOSED |
| [20] | ValueObject editor wiring over Thinktecture factories; UnitsNet quantity editors | inspector-editing | CLOSED |
| [21] | LocaleRow rides inbox resx/ICU/NodaTime; ICU MessageFormat plural route research row | localization-culture | CLOSED |
| [22] | Avalonia-12-in-Rhino NSView embedding is the bridge-proofed implementation-start gate; rhino-panel/modal rows carry the embedding shape | surface-hosts | CLOSED |
| [23] | Headless + Skia render-hash proof lanes; EvidenceReceipt serialization for dashboards | diagnostics-evidence + visuals-offscreen | CLOSED |
| [24] | Pan-zoom canvas rows (PanAndZoom package surface) with input-interaction gesture law | input-interaction | CLOSED |

## [5]-[DENSITY_BAR]

Implementation lands at 25-35% of the naive LOC for this capability set. One owner per axis, one entrypoint family per rail; a new feature is a row or a case inside a budgeted owner, never a new surface. The budget below is the closed owner set derived from the ledger AU-01 through AU-18 signature regions; an implementation file that mints an owner absent from this table fails review.

| [INDEX] | [AXIS/CONCERN] | [OWNER] | [KIND] | [CASES] |
| :-----: | -------------- | ------- | ------ | ------- |
| [1] | host substrate | SurfaceHost | [Union] | 7 |
| [2] | host facts | SurfaceFact | [Union] | 4 |
| [3] | navigation verbs | NavRequest | [Union] | 5 |
| [4] | chrome slots | ChromeSlot | [SmartEnum<string>] | 4 |
| [5] | screen rows | ScreenCatalog | frozen row table | one row per screen |
| [6] | command rows | CommandIntent + CommandDeck | row record + frozen deck | one row per verb |
| [7] | command payload/outcome | CommandPayload · CommandOutcome | [Union] pair | 4 · 4 |
| [8] | data sourcing | DataSource<TRow,TKey> | [Union] | 6 |
| [9] | table projection | TableProjection<TRow,TKey> | [Union] | 5 |
| [10] | editor rows | EditorFactory | [SmartEnum<string>] | 11 |
| [11] | chart series | ChartSeriesSpec | [SmartEnum<string>] | 15 |
| [12] | chart scales | ChartAxisKind | [SmartEnum<string>] | 5 |
| [13] | dashboard tiles | DashboardTile | [Union] | 4 |
| [14] | draw sources | DrawSource | [Union] | 2 |
| [15] | visual destinations | VisualDestination · ExportDestination | [Union] pair | 3 · 3 |
| [16] | theme tokens | TokenRow | [Union] | 5 |
| [17] | theme variant × density | ThemeVariantRow · DensityRow | [SmartEnum<string>] pair | 4 × 2 |
| [18] | typography roles | TypographyRole | [SmartEnum<string>] | 10 |
| [19] | markdown rows | MarkdownRow | [Union] | 7 |
| [20] | icon sourcing | IconSource | [Union] | 5 |
| [21] | asset identity | AssetKey · AssetKind | [ValueObject<string>] + [SmartEnum<string>] | open · 3 |
| [22] | dialog intents | DialogIntent | [Union] | 6 |
| [23] | toast rows/outcomes | ToastRow · ToastOutcome | [SmartEnum<string>] pair | 4 · 3 |
| [24] | transfer payloads | DragPayload | [Union] | 5 |
| [25] | motion grades | MotionToken | [SmartEnum<string>] | 6 |
| [26] | locale rows | LocaleRow | [SmartEnum<string>] | 2 |
| [27] | evidence union | EvidenceReceipt | [Union] | 7 |
| [28] | proof checks | ProofCheck | [SmartEnum<string>] | 6 |
| [29] | fault bands | SurfaceFault 4100 · AssetFault 4120 · DialogFault 4130 · EditFault 4700 | [Union fault] | 5 · 4 · 3 · 7 |

## [6]-[BUILD_ORDER]

Vocabulary owners land first, then shapes, rails, dispatch surfaces, boundaries, and composition. Ledger seam notes are binding at transcription: `Surfaces.Mount`, `InspectorSurface.Mount`, and `InspectorSurface.Attach` carry `ClockPolicy` per the AppHost one-clock-seam law and the page fences already spell it; the input file consumes the frozen deck's gesture-conflict fold and mints no second conflict shape; `AssetKeys` nameof spellings are the only cross-file asset references; the `PhaseMotion` map keys mirror the Compute `ProgressPhase` nine-case set and the conformance sweep fails on drift; `AccessProof.Sweep` and `ProofEngine.Derive` share the typed `(ThemeVariantRow, DensityRow)` grid.

| [INDEX] | [FILE] | [TRANSCRIBES] | [GATE] |
| :-----: | ------ | ------------- | ------ |
| [1] | `Hosts/SurfaceVocabulary.cs` | surface-hosts#HOST_AXIS (vocabulary fences) + surface-hosts#SCALE_FOCUS | static · specs |
| [2] | `Motion/MotionRail.cs` | motion-tokens#MOTION_AXIS + #MOTION_APPLICATION + #PHASE_MAPPING + #REDUCED_MOTION | static · specs |
| [3] | `Typography/TypographyRail.cs` | typography-shaping#ROLE_AXIS + #FONT_ADMISSION + #SHAPING_RAIL + #MARKDOWN_PROJECTION + #TEXT_METRICS | static · specs |
| [4] | `Assets/AssetCatalog.cs` | icons-assets#ASSET_CATALOG + #RASTER_ASSETS | static · specs |
| [5] | `Assets/IconRail.cs` | icons-assets#ICON_AXIS + #SVG_PIPELINE | static · specs · render-hash |
| [6] | `Theme/ThemeTokens.cs` | theme-tokens#TOKEN_CATALOG + #VARIANT_AXIS + #DENSITY_AXIS | static · specs |
| [7] | `Localization/LocaleRail.cs` | localization-culture#LOCALE_AXIS + #STRING_TABLES + #CULTURE_COMPOSITION + #RTL_MIRRORING | static · specs |
| [8] | `Hosts/SurfaceRail.cs` | surface-hosts#HOST_AXIS (dispatch) + #EMBED_CAPSULE + #SCHEDULER_BOUNDARY + #NATIVE_ASSETS | static · specs · bridge |
| [9] | `Theme/ThemeRail.cs` | theme-tokens#CONTROL_THEMES | static · specs · render-hash |
| [10] | `Commands/CommandRail.cs` | commands-availability#INTENT_TABLE + #AVAILABILITY_ALGEBRA + #EXECUTION_RECEIPTS + #PALETTE_AND_REMOTE | static · specs |
| [11] | `Input/InteractionRail.cs` | input-interaction#HOTKEY_DERIVATION + #BEHAVIOR_RAIL + #POINTER_GESTURES + #DRAG_CLIPBOARD | static · specs · bridge |
| [12] | `Screens/ScreenRail.cs` | screens-activation#SCREEN_CATALOG + #ACTIVATION_SCOPES + #DERIVED_STATE + #VALIDATION_UX + #SCREEN_STATE | static · specs |
| [13] | `LiveData/LiveDataRail.cs` | live-data#DATA_SOURCES + #CHANGE_PIPELINES + #BINDING_CAPSULE + #AGGREGATION_SPINE | static · specs |
| [14] | `Tables/TableRail.cs` | tables-hierarchy#GRID_SUBSTRATE + #VIEW_STATE + #TREE_FLATTEN + #GRID_COMMIT | static · specs |
| [15] | `Inspector/InspectorRail.cs` | inspector-editing#INSPECTOR_SURFACE + #EDITOR_FACTORIES + #COMMIT_VALIDATION + #OPTIONS_INSPECTOR + #CONFLICT_RESOLUTION + #CODE_EDITING | static · specs |
| [16] | `Dialogs/DialogRail.cs` | dialogs-notifications#DIALOG_INTENTS + #SESSION_ALGEBRA + #NOTIFICATIONS + #PICKERS_HOST_MODALITY | static · specs · bridge |
| [17] | `Shell/ShellRail.cs` | shell-navigation#ROUTING_SPINE + #DOCK_LAYOUTS + #SHELL_CHROME + #ADAPTIVE_LAYOUT | static · specs |
| [18] | `Charts/ChartRail.cs` | charts-dashboards#SERIES_TABLE + #AXES_SECTIONS + #CHART_INTERACTION + #STREAM_BINDING + #DASHBOARD_TILES | static · specs · render-hash |
| [19] | `Visuals/VisualRail.cs` | visuals-offscreen#DRAW_CAPSULE + #THUMBNAIL_PIPELINE + #PREVIEW_SURFACES + #ENCODE_IDENTITY + #DOCUMENT_EXPORT | static · specs · render-hash |
| [20] | `Access/AccessRail.cs` | accessibility#AUTOMATION_PEERS + #KEYBOARD_NAV + #CONTRAST_GATE + #COMPLIANCE_PROOF | static · specs |
| [21] | `Evidence/EvidenceRail.cs` | diagnostics-evidence#RECEIPT_UNION + #CORRELATION_JOIN + #CAPTURE_LANES + #HEADLESS_DERIVATION + #DEV_LOOP + #TS_PROJECTION | static · specs · render-hash · bridge |

## [7]-[FILE_PROCESS]

1. Read this charter end-to-end, then read every page in the file's TRANSCRIBES cell end-to-end before the first edit.
2. Transcribe the signature fences verbatim; add nothing but file-organization scaffolding (section separators, usings, namespace). Apply the BUILD_ORDER seam notes during transcription, never after.
3. A RESEARCH-gated member resolves its proof route before its gated cluster is transcribed; an unresolved row blocks the cluster — an improvised spelling is the named defect.
4. Run the collapse scan on every edit: three or more parallel types, sibling factories, repeated switch arms, or single-call helpers triggers in-place collapse.
5. Run `uv run python -m tools.assay static fix`, then `uv run python -m tools.assay static build` on the touched closure; the build is authoritative — grep `': error '` broadly.
6. Author specs per the `testing-cs` skill against the file's law; run `uv run python -m tools.assay test run --target Rasm.AppUi.Tests` to green.
7. Host-seam files (rows 8, 11, 16, 21) add `.verify.csx` scenarios and pass `uv run python -m tools.assay bridge verify` under live RhinoWIP before the row closes.

## [8]-[PROOF_GATES]

| [GATE] | [COMMAND] | [EVIDENCE] |
| ------ | --------- | ---------- |
| restore | `dotnet restore --force-evaluate` | regenerated `packages.lock.json` committed; zero NU1004 |
| catalogue | `uv run python -m tools.assay api doctor` + `api resolve <key>` | every fence member greps in its `.reports/api` page |
| static | `uv run python -m tools.assay static plan` / `static fix` / `static build` | zero `': error '` lines on the Rasm.AppUi closure |
| specs | `uv run python -m tools.assay test run --target Rasm.AppUi.Tests` | law specs plus the derived ProofEngine matrix green |
| bridge | `uv run python -m tools.assay bridge verify --pattern <scenario>` | per-scenario facts green under live RhinoWIP |
| render-hash | `uv run python -m tools.assay test run --target Rasm.AppUi.Tests` (headless Skia lanes) | `FrameHash` equality against blob-lane baselines per `ChartSeriesSpec` row and named dashboard |
| mermaid | `npx -p @mermaid-js/mermaid-cli mmdc -i <page>` | every planning diagram renders locally (the MCP renderer is permission-blocked) |

## [9]-[PROHIBITIONS]

- [NEVER] mint a public surface beside the budgeted owners in [5]; a new capability is a row, a case, or a policy value.
- [NEVER] add wrappers, rename adapters, helper/util files, or single-call indirections over package APIs.
- [NEVER] add a generic receipt, ledger, or reported-value abstraction; the typed `EvidenceReceipt` union with slot metadata is the absorbing owner.
- [NEVER] propagate sentinels inward; absence is `Option<T>` at the boundary.
- [NEVER] call `DateTime.UtcNow`, wall clocks, or stopwatches; `ClockPolicy` and injected `TimeProvider` are the only time sources.
- [NEVER] add a second cache, retry, or correlation owner; AppHost owns the cache port, the hop registry, and the correlation spine.
- [NEVER] suppress CSP analyzer diagnostics; findings are architecture pressure — fix the shape.
- [NEVER] reference TreeDataGrid; hierarchy is the tree-flatten fold on the free DataGrid.
- [NEVER] use ReactiveUI MessageBus; decoupled invocation is an intent key through the one command table.
- [NEVER] apply `ObserveOn` outside `BindingCapsule`; the UI marshal is the one scheduler boundary.
- [NEVER] add a key table, hotkey registry, or conflict fold beside the `CommandIntent` gesture column and the deck freeze.
- [NEVER] write a literal paint, font value, duration, or easing at a call site; every visual constant traces to a token, role, or motion row.
- [NEVER] add a second color interpolation beside the OKLab mix delegate, or a second luminance computation beside `ContrastGate`.
- [NEVER] render Markdown outside `MarkdownProjection`, or export documents outside `SKDocument` and the codec rows.
- [NEVER] write `CultureInfo.CurrentCulture`/`CurrentUICulture`; culture travels explicitly through `ResolvedLocale`.
- [NEVER] author a per-screen smoke spec beside the `ProofEngine` derivation, or a second image cache beside the loader hierarchy and the blob lane.
- [NEVER] boot a second `AppBuilder` or lifetime; `Surfaces.Boot` behind the `Interlocked` edge guard is the one admission.
- [NEVER] name a host API inside a dispatch arm; every host crossing is a `SurfaceSeam` column or a port delegate bound at composition.

## [10]-[ADMISSIONS_RECORD]

The executed admissions ledger — the only planning location where versions are written. Versions transcribe `Directory.Packages.props`; catalogue cells name the `.reports/api` page proving the surface.

| [PACKAGE] | [VERSION] | [PAGE] | [CATALOGUE] |
| --------- | --------- | ------ | ----------- |
| Avalonia | 12.0.4 | surface-hosts | api-avalonia.md |
| Avalonia.Desktop | 12.0.4 | surface-hosts | api-avalonia-desktop.md |
| Avalonia.Skia | 12.0.4 (central pin, lease seam) | visuals-offscreen | api-avalonia-skia.md |
| Avalonia.Controls.ColorPicker | 12.0.4 | inspector-editing · accessibility | api-avalonia-color.md |
| Avalonia.Controls.DataGrid | 12.0.0 | tables-hierarchy | api-avalonia-grid.md |
| Avalonia.Fonts.Inter | 12.0.4 | typography-shaping | api-avalonia-fonts.md |
| Avalonia.Themes.Fluent | 12.0.4 | theme-tokens | api-avalonia-fluent.md |
| ReactiveUI | 23.2.28 | shell-navigation · screens-activation · commands-availability | api-reactiveui.md |
| ReactiveUI.Avalonia | 12.0.3 | surface-hosts · shell-navigation | api-reactiveui-avalonia.md |
| ReactiveUI.Validation | 7.1.0 | screens-activation · inspector-editing | api-reactiveui-validation.md |
| Xaml.Behaviors.Avalonia | 12.0.0.1 | input-interaction · shell-navigation | api-behaviors.md |
| System.Reactive | 6.1.0 | live-data · screens-activation | api-reactive.md |
| DynamicData | 9.4.31 | live-data · tables-hierarchy | api-dynamicdata.md |
| LiveChartsCore.SkiaSharpView.Avalonia | 2.0.4 | charts-dashboards | api-livecharts.md |
| SkiaSharp | 3.119.4 | visuals-offscreen · icons-assets | api-skiasharp.md |
| SkiaSharp.HarfBuzz | 3.119.4 | typography-shaping | api-skia-harfbuzz.md |
| Svg.Controls.Skia.Avalonia | 12.0.0.11 | icons-assets | api-svg-skia.md |
| SkiaSharp.NativeAssets.macOS | 3.119.4 | surface-hosts | api-skia-native.md |
| SkiaSharp.NativeAssets.Win32 | 3.119.4 | surface-hosts | api-skia-native.md |
| SkiaSharp.NativeAssets.Linux.NoDependencies | 3.119.4 | surface-hosts | api-skia-native.md |
| SkiaSharp.NativeAssets.Linux | 3.119.4 (pinned + excluded) | surface-hosts | api-skia-native.md |
| HarfBuzzSharp.NativeAssets.macOS | 8.3.1.5 | surface-hosts | api-harfbuzz-native.md |
| HarfBuzzSharp.NativeAssets.Win32 | 8.3.1.5 | surface-hosts | api-harfbuzz-native.md |
| HarfBuzzSharp.NativeAssets.Linux | 8.3.1.5 | surface-hosts | api-harfbuzz-native.md |
| AsyncImageLoader.Avalonia | 3.8.0 | icons-assets · visuals-offscreen | api-asyncimageloader.md |
| Avalonia.AvaloniaEdit | 12.0.0 | inspector-editing | api-avaloniaedit.md |
| AvaloniaEdit.TextMate | 12.0.0 | inspector-editing | api-avaloniaedit.md |
| bodong.Avalonia.PropertyGrid | 12.0.4.1 | inspector-editing | api-propertygrid.md |
| bodong.PropertyModels | 12.0.0 (transitive floor) | inspector-editing | api-propertygrid.md |
| DialogHost.Avalonia | 0.12.2 | dialogs-notifications | api-dialoghost.md |
| Dock.Avalonia | 12.0.0.2 | shell-navigation | api-dock.md |
| Dock.Model.ReactiveUI | 12.0.0.2 | shell-navigation | api-dock.md |
| PanAndZoom | 12.0.0.1 | input-interaction · charts-dashboards | api-panandzoom.md |
| FluentIcons.Avalonia | 2.1.328 | icons-assets | api-fluenticons.md |
| Markdig | 1.2.0 | typography-shaping | api-markdig.md |
| Thinktecture.Runtime.Extensions.Json | 10.2.0 | commands-availability · diagnostics-evidence | doctrine (docs/stacks/csharp) |
| HotAvalonia | 3.1.1 (Debug self-gating, PrivateAssets=all) | diagnostics-evidence | api-hotavalonia.md |
| Avalonia.Headless | 12.0.4 (tests) | diagnostics-evidence · accessibility | api-headless.md |
| Avalonia.Headless.XUnit | 12.0.4 (tests) | diagnostics-evidence | api-headless.md |

## [11]-[REFINEMENT_HORIZON]

Entry for the next deepening session: `libs/csharp/.planning/campaign-method.md` then `TASKLOG.md` then this charter. Folder-specific deepening targets: the embedding capsule proven by the NSView bridge spike and extended with the Win32 route; viewport-in-panel and host-data-in-shell compositions rehearsed as concept rows (rhino panels, viewports, and host documents inside the same shell that runs standalone); every element family re-swept after the embedding spike answers its research rows. The bar already set here is the suite bar: any app UI — panel, modal, standalone, companion console — composes from rows with dynamic sourcing and zero host-coupled code.

# [APPUI_PLANNING]

Rasm.AppUi has zero consumers; the implementation is full-capability with no holding back. These pages are decision-complete blueprints an implementation agent transcribes — never re-designed downstream. The package owns the product UI rail: surface hosts, shell and navigation, screens, commands, live data, tables, inspectors, charts, offscreen visuals, theme and typography, icons, dialogs, input, motion, accessibility, localization, and evidence — consuming AppHost ports, Persistence queries, and Compute receipts as settled vocabulary.

## [1]-[PAGE_INDEX]

| [INDEX] | [PAGE]                                            | [OWNS]                                      | [STATE]   |
| :-----: | ------------------------------------------------- | :------------------------------------------ | :-------- |
|   [1]   | [surface-hosts](surface-hosts.md)                 | host axis, mount, marshal rows              | finalized |
|   [2]   | [shell-navigation](shell-navigation.md)           | shell, routing, dock layouts                | finalized |
|   [3]   | [screens-activation](screens-activation.md)       | screen family, activation, lifecycle        | finalized |
|   [4]   | [commands-availability](commands-availability.md) | intent table, availability, invocation      | finalized |
|   [5]   | [live-data](live-data.md)                         | data sources and change-set spine           | finalized |
|   [6]   | [tables-hierarchy](tables-hierarchy.md)           | table projections and tree flattening       | finalized |
|   [7]   | [inspector-editing](inspector-editing.md)         | editors, validation, options, conflicts     | finalized |
|   [8]   | [charts-dashboards](charts-dashboards.md)         | chart rows and dashboard composition        | finalized |
|   [9]   | [visuals-offscreen](visuals-offscreen.md)         | offscreen render, hash, export              | finalized |
|  [10]   | [theme-tokens](theme-tokens.md)                   | tokens, variants, density                   | finalized |
|  [11]   | [typography-shaping](typography-shaping.md)       | typography, shaping, markdown               | finalized |
|  [12]   | [icons-assets](icons-assets.md)                   | icon sources and asset loading              | finalized |
|  [13]   | [dialogs-notifications](dialogs-notifications.md) | dialogs, sessions, notifications            | finalized |
|  [14]   | [input-interaction](input-interaction.md)         | input rows, gestures, pan-zoom              | finalized |
|  [15]   | [motion-tokens](motion-tokens.md)                 | motion rows and phase mapping               | finalized |
|  [16]   | [accessibility](accessibility.md)                 | automation, contrast, motion law            | finalized |
|  [17]   | [localization-culture](localization-culture.md)   | locale rows and culture composition         | finalized |
|  [18]   | [diagnostics-evidence](diagnostics-evidence.md)   | evidence union, correlation, headless proof | finalized |

## [2]-[WIRE_PAGES]

commands-availability · diagnostics-evidence (each carries exactly one TS_PROJECTION cluster).

## [3]-[CATALOGUE_PENDING]

TextMateSharp grammar registry (transitive, unpinned) gets a page only if admitted; FluentIcons.Common vocabulary rides the api-fluenticons.md surface card.

## [4]-[GAP_LEDGER]

Every row is CLOSED: a gap is present only when absorbed by the named page#cluster.

| [INDEX] | [GAP]                                    | [CLOSED_BY (page#cluster)]                                   |
| :-----: | :--------------------------------------- | :----------------------------------------------------------- |
|   [1]   | designed-only web host and TS wire reuse | surface-hosts + commands-availability + diagnostics-evidence |
|   [2]   | one `CommandIntent` table                | commands-availability                                        |
|   [3]   | orthogonal variant and density rows      | theme-tokens                                                 |
|   [4]   | ranked icon fallback                     | icons-assets                                                 |
|   [5]   | dock checkpoint and crash restore        | shell-navigation                                             |
|   [6]   | `ProgressPhase` to `MotionToken` map     | motion-tokens                                                |
|   [7]   | options inspector reload composite       | inspector-editing                                            |
|   [8]   | command-journal replay                   | diagnostics-evidence + surface-hosts                         |
|   [9]   | correlation drill-down join              | diagnostics-evidence                                         |
|  [10]   | Markdig to typography projection         | typography-shaping                                           |
|  [11]   | `SKDocument` document export             | visuals-offscreen                                            |
|  [12]   | model-result provenance projection       | diagnostics-evidence                                         |
|  [13]   | DataGrid tree flatten and edit commit    | tables-hierarchy                                             |
|  [14]   | live-data spine and `SourceCache` keys   | live-data                                                    |
|  [15]   | palette search and MessageBus rejection  | commands-availability                                        |
|  [16]   | WCAG luminance gate                      | accessibility                                                |
|  [17]   | drain/suspend notification suppression   | dialogs-notifications                                        |
|  [18]   | GeoMap GeoJSON asset route               | charts-dashboards + icons-assets                             |
|  [19]   | conflict inspector projection            | inspector-editing                                            |
|  [20]   | value-object and quantity editors        | inspector-editing                                            |
|  [21]   | locale row and ICU research route        | localization-culture                                         |
|  [22]   | Avalonia-in-Rhino embedding gate         | surface-hosts                                                |
|  [23]   | headless render-hash and dashboard wire  | diagnostics-evidence + visuals-offscreen                     |
|  [24]   | pan-zoom canvas rows                     | input-interaction                                            |

## [5]-[DENSITY_BAR]

Implementation collapses to one owner per axis and one entrypoint family per rail; density means no parallel rails, no near-duplicate shapes, no re-derived logic — a file is as large as its owner's concern requires, never trimmed to a line count. A new feature is a row or case, never a new surface. The budget below is the closed owner set; an implementation file that mints an owner absent from this table fails review. `[STATE]` carries `FINALIZED` where the owner is a transcription-complete fence with no open gate and `SPIKE` where the owner is fence-complete but its proof carries a residual native, bridge, or live-server probe named in the page's RESEARCH cluster — a SPIKE owner is fully shaped now, never a deferred surface.

| [INDEX] | [AXIS]                  | [OWNER]                                                     | [KIND]       | [CASES]       |  [STATE]  |
| :-----: | :---------------------- | :---------------------------------------------------------- | :----------- | :------------ | :-------: |
|   [1]   | host substrate          | `SurfaceHost`                                               | [Union]      | 7             |   SPIKE   |
|   [2]   | host facts              | `SurfaceFact`                                               | [Union]      | 4             |   SPIKE   |
|   [3]   | navigation verbs        | `NavRequest`                                                | [Union]      | 5             | FINALIZED |
|   [4]   | chrome slots            | `ChromeSlot`                                                | SmartEnum    | 4             | FINALIZED |
|   [5]   | screen rows             | `ScreenCatalog`                                             | row table    | per screen    | FINALIZED |
|   [6]   | command rows            | `CommandIntent` + `CommandDeck`                             | row + deck   | per verb      | FINALIZED |
|   [7]   | command payload/outcome | `CommandPayload` · `CommandOutcome`                         | union pair   | 4 · 4         | FINALIZED |
|   [8]   | data sourcing           | `DataSource<TRow,TKey>`                                     | [Union]      | 7             | FINALIZED |
|   [9]   | table projection        | `TableProjection<TRow,TKey>`                                | [Union]      | 5             | FINALIZED |
|  [10]   | editor rows             | `EditorFactory`                                             | SmartEnum    | 11            | FINALIZED |
|  [11]   | chart series            | `ChartSeriesSpec`                                           | SmartEnum    | 15            |   SPIKE   |
|  [12]   | chart scales            | `ChartAxisKind`                                             | SmartEnum    | 5             | FINALIZED |
|  [13]   | dashboard tiles         | `DashboardTile`                                             | [Union]      | 4             | FINALIZED |
|  [14]   | draw sources            | `DrawSource`                                                | [Union]      | 2             | FINALIZED |
|  [15]   | visual destinations     | `VisualDestination` · `ExportDestination` (+ `VisualCodec` encode) | union pair   | 3 · 3         |   SPIKE   |
|  [16]   | theme tokens            | `TokenRow`                                                  | [Union]      | 5             | FINALIZED |
|  [17]   | variant and density     | `ThemeVariantRow` · `DensityRow`                            | enum pair    | 4 × 2         | FINALIZED |
|  [18]   | typography roles        | `TypographyRole`                                            | SmartEnum    | 10            | FINALIZED |
|  [19]   | markdown rows           | `MarkdownRow`                                               | [Union]      | 7             | FINALIZED |
|  [20]   | icon sourcing           | `IconSource`                                                | [Union]      | 5             | FINALIZED |
|  [21]   | asset identity          | `AssetKey` · `AssetKind` (+ `SvgPipeline` · `RasterAssets`) | value + enum | open · 3      | FINALIZED |
|  [22]   | dialog intents          | `DialogIntent`                                              | [Union]      | 6             | FINALIZED |
|  [23]   | toast rows/outcomes     | `ToastRow` · `ToastOutcome`                                 | enum pair    | 4 · 3         |   SPIKE   |
|  [24]   | transfer payloads       | `DragPayload`                                               | [Union]      | 5             |   SPIKE   |
|  [25]   | motion grades           | `MotionToken`                                               | SmartEnum    | 6             | FINALIZED |
|  [26]   | locale rows             | `LocaleRow`                                                 | SmartEnum    | 2             | FINALIZED |
|  [27]   | evidence union          | `EvidenceReceipt`                                           | [Union]      | 7             |   SPIKE   |
|  [28]   | proof checks            | `ProofCheck`                                                | SmartEnum    | 8             |   SPIKE   |
|  [29]   | fault bands             | surface, asset, dialog, edit faults          | fault unions | 5 · 4 · 3 · 7 | FINALIZED |
|  [30]   | export flow blocks      | `FlowBlock` (+ `HeaderFooterBand` band, `BreakRule` policy on `VisualExportSpec`) | [Union]      | 3             | FINALIZED |

## [6]-[BUILD_ORDER]

Vocabulary owners land first, then shapes, rails, dispatch surfaces, boundaries, and composition. Ledger seam notes are binding at transcription: `Surfaces.Mount`, `InspectorSurface.Mount`, and `InspectorSurface.Attach` carry `ClockPolicy` per the AppHost one-clock-seam law and the page fences already spell it; the input file consumes the frozen deck's gesture-conflict fold and mints no second conflict shape; `AssetKeys` nameof spellings are the only cross-file asset references; the `PhaseMotion` map keys mirror the Compute `ProgressPhase` nine-case set and the conformance sweep fails on drift; `AccessProof.Sweep` and `ProofEngine.Derive` share the typed `(ThemeVariantRow, DensityRow)` grid.

Cluster cells use page-local anchor names; proof cells name evidence beyond the standard static/spec gate.

| [INDEX] | [FILE]                         | [CLUSTERS]                                     | [PROOF]              |
| :-----: | :----------------------------- | :--------------------------------------------- | :------------------- |
|   [1]   | `Hosts/SurfaceVocabulary.cs`   | host axis, scale focus                         | specs                |
|   [2]   | `Motion/MotionRail.cs`         | motion axis, application, phase map, reduction | phase-map specs      |
|   [3]   | `Typography/TypographyRail.cs` | roles, fonts, shaping, markdown, metrics       | shaping specs        |
|   [4]   | `Assets/AssetCatalog.cs`       | asset catalog, raster assets                   | specs                |
|   [5]   | `Assets/IconRail.cs`           | icon axis, SVG pipeline                        | render-hash          |
|   [6]   | `Theme/ThemeTokens.cs`         | token catalog, variant, density                | specs                |
|   [7]   | `Localization/LocaleRail.cs`   | locale, strings, culture, RTL                  | specs                |
|   [8]   | `Hosts/SurfaceRail.cs`         | host dispatch, embed, scheduler, native assets | bridge               |
|   [9]   | `Theme/ThemeRail.cs`           | control themes                                 | render-hash          |
|  [10]   | `Commands/CommandRail.cs`      | intent, availability, receipts, palette        | specs                |
|  [11]   | `Input/InteractionRail.cs`     | hotkeys, behavior, gestures, drag              | bridge               |
|  [12]   | `Screens/ScreenRail.cs`        | catalog, activation, state, validation         | specs                |
|  [13]   | `LiveData/LiveDataRail.cs`     | sources, pipelines, binding, aggregation       | specs                |
|  [14]   | `Tables/TableRail.cs`          | grid, view state, tree flatten, commit         | specs                |
|  [15]   | `Inspector/InspectorRail.cs`   | surface, editors, commit, options, conflicts   | specs                |
|  [16]   | `Dialogs/DialogRail.cs`        | dialogs, sessions, notices, pickers            | bridge               |
|  [17]   | `Shell/ShellRail.cs`           | routing, docks, chrome, adaptive layout        | specs                |
|  [18]   | `Charts/ChartRail.cs`          | series, axes, interaction, streams, tiles      | render-hash          |
|  [19]   | `Visuals/VisualRail.cs`        | draw, thumbnails, previews, encode, export     | render-hash          |
|  [20]   | `Access/AccessRail.cs`         | peers, keyboard, contrast, proof               | specs                |
|  [21]   | `Evidence/EvidenceRail.cs`     | receipts, correlation, capture, headless, TS   | render-hash + bridge |

## [7]-[FILE_PROCESS]

1. Read this charter end-to-end, then read every page in the file's TRANSCRIBES cell end-to-end before the first edit.
2. Transcribe the signature fences verbatim; add nothing but file-organization scaffolding (section separators, usings, namespace). Apply the BUILD_ORDER seam notes during transcription, never after.
3. A RESEARCH-gated member resolves before its gated cluster is transcribed; an unresolved item blocks the cluster — an improvised spelling is the named defect.
4. Run the collapse scan on every edit: three or more parallel types, sibling factories, repeated switch arms, or single-call helpers triggers in-place collapse.
5. Run `uv run python -m tools.assay static fix`, then `uv run python -m tools.assay static build` on the touched closure; the build is authoritative — grep `': error '` broadly.
6. Author specs per the `testing-cs` skill against the file's law; run `uv run python -m tools.assay test run --target Rasm.AppUi.Tests` to green.
7. Host-seam files (rows 8, 11, 16, 21) add `.verify.csx` scenarios and pass `uv run python -m tools.assay bridge verify` under live RhinoWIP before the row closes.

## [8]-[PROOF_GATES]

Assay rows use `uv run python -m tools.assay`; proof runs at the planned phase gate, not after each edit.

| [GATE] | [RAIL]                            | [EVIDENCE]                                   |
| :----: | :-------------------------------- | :------------------------------------------- |
|  [G1]  | `dotnet restore --force-evaluate` | lockfile regenerated; zero NU1004            |
|  [G2]  | `api doctor` + `api resolve`      | fence members resolve in `.api`              |
|  [G3]  | `static plan/fix/build`           | zero `': error '` lines on the AppUi closure |
|  [G4]  | `test run` AppUi target           | law specs and ProofEngine matrix pass        |
|  [G5]  | `bridge verify` scenario          | live RhinoWIP scenario facts pass            |
|  [G6]  | `test run` headless lanes         | `FrameHash` equality for named visual rows   |
|  [G7]  | `mmdc` page render                | planning diagrams render locally             |

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

The executed admissions ledger maps each package to its consuming page, `.api` catalogue, and admission status. Versions live in `Directory.Packages.props`; this table never carries a pin. Catalogue keys omit the `api-` prefix and `.md` suffix.

| [INDEX] | [PACKAGE]                                   | [PAGE]                      | [CATALOGUE]           | [STATUS]     |
| :-----: | :------------------------------------------ | :-------------------------- | :-------------------- | :----------- |
|   [1]   | Avalonia                                    | surface-hosts               | avalonia              | admitted     |
|   [2]   | Avalonia.Desktop                            | surface-hosts               | avalonia-desktop      | admitted     |
|   [3]   | Avalonia.Skia                               | visuals-offscreen           | avalonia-skia         | admitted     |
|   [4]   | Avalonia.Controls.ColorPicker               | inspector + accessibility   | avalonia-color        | admitted     |
|   [5]   | Avalonia.Controls.DataGrid                  | tables-hierarchy            | avalonia-grid         | admitted     |
|   [6]   | Avalonia.Fonts.Inter                        | typography-shaping          | avalonia-fonts        | admitted     |
|   [7]   | Avalonia.Themes.Fluent                      | theme-tokens                | avalonia-fluent       | admitted     |
|   [8]   | ReactiveUI                                  | shell + screens + commands  | reactiveui            | admitted     |
|   [9]   | ReactiveUI.Avalonia                         | surface-hosts + shell       | reactiveui-avalonia   | admitted     |
|  [10]   | ReactiveUI.Validation                       | screens + inspector         | reactiveui-validation | admitted     |
|  [11]   | Xaml.Behaviors.Avalonia                     | input + shell               | behaviors             | admitted     |
|  [12]   | System.Reactive                             | live-data + screens         | reactive              | admitted     |
|  [13]   | DynamicData                                 | live-data + tables          | dynamicdata           | admitted     |
|  [14]   | LiveChartsCore.SkiaSharpView.Avalonia       | charts-dashboards           | livecharts            | admitted     |
|  [15]   | SkiaSharp                                   | visuals + icons             | skiasharp             | admitted     |
|  [16]   | SkiaSharp.HarfBuzz                          | typography-shaping          | skia-harfbuzz         | admitted     |
|  [17]   | Svg.Controls.Skia.Avalonia                  | icons-assets                | svg-skia              | admitted     |
|  [18]   | SkiaSharp.NativeAssets.macOS                | surface-hosts               | skia-native           | admitted     |
|  [19]   | SkiaSharp.NativeAssets.Win32                | surface-hosts               | skia-native           | admitted     |
|  [20]   | SkiaSharp.NativeAssets.Linux.NoDependencies | surface-hosts               | skia-native           | admitted     |
|  [21]   | SkiaSharp.NativeAssets.Linux                | surface-hosts               | skia-native           | admitted     |
|  [22]   | HarfBuzzSharp.NativeAssets.macOS            | surface-hosts               | harfbuzz-native       | admitted     |
|  [23]   | HarfBuzzSharp.NativeAssets.Win32            | surface-hosts               | harfbuzz-native       | admitted     |
|  [24]   | HarfBuzzSharp.NativeAssets.Linux            | surface-hosts               | harfbuzz-native       | admitted     |
|  [25]   | AsyncImageLoader.Avalonia                   | icons + visuals             | asyncimageloader      | admitted     |
|  [26]   | Avalonia.AvaloniaEdit                       | inspector-editing           | avaloniaedit          | admitted     |
|  [27]   | AvaloniaEdit.TextMate                       | inspector-editing           | avaloniaedit          | admitted     |
|  [28]   | bodong.Avalonia.PropertyGrid                | inspector-editing           | propertygrid          | admitted     |
|  [29]   | bodong.PropertyModels                       | inspector-editing           | propertygrid          | admitted     |
|  [30]   | DialogHost.Avalonia                         | dialogs-notifications       | dialoghost            | admitted     |
|  [31]   | Dock.Avalonia                               | shell-navigation            | dock                  | admitted     |
|  [32]   | Dock.Model.ReactiveUI                       | shell-navigation            | dock                  | admitted     |
|  [33]   | PanAndZoom                                  | input + charts              | panandzoom            | admitted     |
|  [34]   | FluentIcons.Avalonia                        | icons-assets                | fluenticons           | admitted     |
|  [35]   | Markdig                                     | typography-shaping          | markdig               | admitted     |
|  [36]   | Thinktecture.Runtime.Extensions.Json        | commands + diagnostics      | docs/stacks/csharp    | admitted     |
|  [37]   | HotAvalonia                                 | diagnostics-evidence        | hotavalonia           | admitted     |
|  [38]   | Avalonia.Headless                           | diagnostics + accessibility | headless              | admitted     |
|  [39]   | Avalonia.Headless.XUnit                     | diagnostics-evidence        | headless              | admitted     |
|  [40]   | Verify.XunitV3                              | diagnostics-evidence        | headless              | catalogue-pending |

## [11]-[REFINEMENT_HORIZON]

Folder-specific deepening targets: the embedding capsule beyond the NSView bridge spike — the `surface-hosts` [EMBED_SPIKE] seam resolving the UI-thread predicate, the CADisplayLink-paced pump fallback, `RenderingMode` ordering against the host pipeline, and the shared-`GRContext` composite into the host-owned context, then the [WIN32_ROUTE] embed route activating the designed-only host-axis case against the Rhino Windows panel host once the WinForms interoperability host admission lands; viewport-in-panel and host-data-in-shell compositions rehearsed as concept rows (rhino panels, viewports, and host documents inside the one shell that runs standalone), gated by that same spike; every element family re-swept after the embedding spike answers its research items. The bar already set here is the suite bar: any app UI — panel, modal, standalone, companion console — composes from rows with dynamic sourcing and zero host-coupled code.

Testing-infrastructure horizon: the headless render-hash baselines drive `[AvaloniaFact]` UI-thread dispatch through `SetRenderScaling`/`CaptureRenderedFrame`, snapshotting the content-addressed render-hash receipt text under `Verify.XunitV3`, never floating pixels.

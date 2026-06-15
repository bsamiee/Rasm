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

`[OWNER]` names the page that absorbed the gap; the red-team pass requires every row CLOSED.

| [INDEX] | [GAP]                                    | [OWNER]                                                      | [STATE] |
| :-----: | :--------------------------------------- | :----------------------------------------------------------- | :------ |
|   [1]   | designed-only web host and TS wire reuse | surface-hosts + commands-availability + diagnostics-evidence | CLOSED  |
|   [2]   | one `CommandIntent` table                | commands-availability                                        | CLOSED  |
|   [3]   | orthogonal variant and density rows      | theme-tokens                                                 | CLOSED  |
|   [4]   | ranked icon fallback                     | icons-assets                                                 | CLOSED  |
|   [5]   | dock checkpoint and crash restore        | shell-navigation                                             | CLOSED  |
|   [6]   | `ProgressPhase` to `MotionToken` map     | motion-tokens                                                | CLOSED  |
|   [7]   | options inspector reload composite       | inspector-editing                                            | CLOSED  |
|   [8]   | command-journal replay                   | diagnostics-evidence + surface-hosts                         | CLOSED  |
|   [9]   | correlation drill-down join              | diagnostics-evidence                                         | CLOSED  |
|  [10]   | Markdig to typography projection         | typography-shaping                                           | CLOSED  |
|  [11]   | `SKDocument` document export             | visuals-offscreen                                            | CLOSED  |
|  [12]   | model-result provenance projection       | diagnostics-evidence                                         | CLOSED  |
|  [13]   | DataGrid tree flatten and edit commit    | tables-hierarchy                                             | CLOSED  |
|  [14]   | live-data spine and `SourceCache` keys   | live-data                                                    | CLOSED  |
|  [15]   | palette search and MessageBus rejection  | commands-availability                                        | CLOSED  |
|  [16]   | WCAG luminance gate                      | accessibility                                                | CLOSED  |
|  [17]   | drain/suspend notification suppression   | dialogs-notifications                                        | CLOSED  |
|  [18]   | GeoMap GeoJSON asset route               | charts-dashboards + icons-assets                             | CLOSED  |
|  [19]   | conflict inspector projection            | inspector-editing                                            | CLOSED  |
|  [20]   | value-object and quantity editors        | inspector-editing                                            | CLOSED  |
|  [21]   | locale row and ICU research route        | localization-culture                                         | CLOSED  |
|  [22]   | Avalonia-in-Rhino embedding gate         | surface-hosts                                                | CLOSED  |
|  [23]   | headless render-hash and dashboard wire  | diagnostics-evidence + visuals-offscreen                     | CLOSED  |
|  [24]   | pan-zoom canvas rows                     | input-interaction                                            | CLOSED  |

## [5]-[DENSITY_BAR]

Implementation lands at 25-35% of the naive LOC for this capability set. One owner per axis, one entrypoint family per rail; a new feature is a row or a case inside a budgeted owner, never a new surface. The budget below is the closed owner set derived from the ledger AU-01 through AU-18 signature regions; an implementation file that mints an owner absent from this table fails review. `[STATE]` carries `FINALIZED` where the owner is a transcription-complete fence with no open gate and `SPIKE` where the owner is fence-complete but its proof carries a residual native, bridge, or live-server probe named in the page's RESEARCH cluster — a SPIKE owner is fully shaped now, never a deferred surface.

| [INDEX] | [AXIS]                  | [OWNER]                                                     | [KIND]       | [CASES]       |  [STATE]  |
| :-----: | :---------------------- | :---------------------------------------------------------- | :----------- | :------------ | :-------: |
|   [1]   | host substrate          | `SurfaceHost`                                               | [Union]      | 7             |   SPIKE   |
|   [2]   | host facts              | `SurfaceFact`                                               | [Union]      | 4             |   SPIKE   |
|   [3]   | navigation verbs        | `NavRequest`                                                | [Union]      | 5             | FINALIZED |
|   [4]   | chrome slots            | `ChromeSlot`                                                | SmartEnum    | 4             | FINALIZED |
|   [5]   | screen rows             | `ScreenCatalog`                                             | row table    | per screen    | FINALIZED |
|   [6]   | command rows            | `CommandIntent` + `CommandDeck`                             | row + deck   | per verb      | FINALIZED |
|   [7]   | command payload/outcome | `CommandPayload` · `CommandOutcome`                         | union pair   | 4 · 4         | FINALIZED |
|   [8]   | data sourcing           | `DataSource<TRow,TKey>`                                     | [Union]      | 7             |   SPIKE   |
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
|  [21]   | asset identity          | `AssetKey` · `AssetKind` (+ `SvgPipeline` · `RasterAssets`) | value + enum | open · 3      |   SPIKE   |
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

The executed admissions ledger binds each package to its version pin, owner page, and API evidence. Versions transcribe `Directory.Packages.props`; API keys omit the `api-` prefix and `.md` suffix. This is the only planning location where versions are written.

| [INDEX] | [PACKAGE]                                   | [VERSION]    | [OWNER]                     | [API]                 |
| :-----: | :------------------------------------------ | :----------- | :-------------------------- | :-------------------- |
|   [1]   | Avalonia                                    | 12.0.4       | surface-hosts               | avalonia              |
|   [2]   | Avalonia.Desktop                            | 12.0.4       | surface-hosts               | avalonia-desktop      |
|   [3]   | Avalonia.Skia                               | 12.0.4       | visuals-offscreen           | avalonia-skia         |
|   [4]   | Avalonia.Controls.ColorPicker               | 12.0.4       | inspector + accessibility   | avalonia-color        |
|   [5]   | Avalonia.Controls.DataGrid                  | 12.0.0       | tables-hierarchy            | avalonia-grid         |
|   [6]   | Avalonia.Fonts.Inter                        | 12.0.4       | typography-shaping          | avalonia-fonts        |
|   [7]   | Avalonia.Themes.Fluent                      | 12.0.4       | theme-tokens                | avalonia-fluent       |
|   [8]   | ReactiveUI                                  | 23.2.28      | shell + screens + commands  | reactiveui            |
|   [9]   | ReactiveUI.Avalonia                         | 12.0.3       | surface-hosts + shell       | reactiveui-avalonia   |
|  [10]   | ReactiveUI.Validation                       | 7.1.0        | screens + inspector         | reactiveui-validation |
|  [11]   | Xaml.Behaviors.Avalonia                     | 12.0.0.1     | input + shell               | behaviors             |
|  [12]   | System.Reactive                             | 6.1.0        | live-data + screens         | reactive              |
|  [13]   | DynamicData                                 | 9.4.31       | live-data + tables          | dynamicdata           |
|  [14]   | LiveChartsCore.SkiaSharpView.Avalonia       | 2.0.4        | charts-dashboards           | livecharts            |
|  [15]   | SkiaSharp                                   | 3.119.4      | visuals + icons             | skiasharp             |
|  [16]   | SkiaSharp.HarfBuzz                          | 3.119.4      | typography-shaping          | skia-harfbuzz         |
|  [17]   | Svg.Controls.Skia.Avalonia                  | 12.0.0.11    | icons-assets                | svg-skia              |
|  [18]   | SkiaSharp.NativeAssets.macOS                | 3.119.4      | surface-hosts               | skia-native           |
|  [19]   | SkiaSharp.NativeAssets.Win32                | 3.119.4      | surface-hosts               | skia-native           |
|  [20]   | SkiaSharp.NativeAssets.Linux.NoDependencies | 3.119.4      | surface-hosts               | skia-native           |
|  [21]   | SkiaSharp.NativeAssets.Linux                | 3.119.4      | surface-hosts               | skia-native           |
|  [22]   | HarfBuzzSharp.NativeAssets.macOS            | 8.3.1.5      | surface-hosts               | harfbuzz-native       |
|  [23]   | HarfBuzzSharp.NativeAssets.Win32            | 8.3.1.5      | surface-hosts               | harfbuzz-native       |
|  [24]   | HarfBuzzSharp.NativeAssets.Linux            | 8.3.1.5      | surface-hosts               | harfbuzz-native       |
|  [25]   | AsyncImageLoader.Avalonia                   | 3.8.0        | icons + visuals             | asyncimageloader      |
|  [26]   | Avalonia.AvaloniaEdit                       | 12.0.0       | inspector-editing           | avaloniaedit          |
|  [27]   | AvaloniaEdit.TextMate                       | 12.0.0       | inspector-editing           | avaloniaedit          |
|  [28]   | bodong.Avalonia.PropertyGrid                | 12.0.4.1     | inspector-editing           | propertygrid          |
|  [29]   | bodong.PropertyModels                       | 12.0.0       | inspector-editing           | propertygrid          |
|  [30]   | DialogHost.Avalonia                         | 0.12.2       | dialogs-notifications       | dialoghost            |
|  [31]   | Dock.Avalonia                               | 12.0.0.2     | shell-navigation            | dock                  |
|  [32]   | Dock.Model.ReactiveUI                       | 12.0.0.2     | shell-navigation            | dock                  |
|  [33]   | PanAndZoom                                  | 12.0.0.1     | input + charts              | panandzoom            |
|  [34]   | FluentIcons.Avalonia                        | 2.1.328      | icons-assets                | fluenticons           |
|  [35]   | Markdig                                     | 1.2.0        | typography-shaping          | markdig               |
|  [36]   | Thinktecture.Runtime.Extensions.Json        | 10.2.0       | commands + diagnostics      | docs/stacks/csharp    |
|  [37]   | HotAvalonia                                 | 3.1.1        | diagnostics-evidence        | hotavalonia           |
|  [38]   | Avalonia.Headless                           | 12.0.4       | diagnostics + accessibility | headless              |
|  [39]   | Avalonia.Headless.XUnit                     | 12.0.4       | diagnostics-evidence        | headless              |

Substrate, pending, and test-only admissions:

| [PACKAGE]               | [VERSION] | [PAGE]               | [CATALOGUE]               |
| :---------------------- | :-------- | :------------------- | :------------------------ |
| Avalonia.Headless.XUnit | 12.0.4    | surface-hosts        | api-headless.md (tests-only) |
| Avalonia.Skia           | 12.0.4    | surface-hosts        | api-headless.md (tests-only) |
| Verify.XunitV3          | 31.19.1   | diagnostics-evidence | catalogue pending         |

## [11]-[REFINEMENT_HORIZON]

Folder-specific deepening targets: the embedding capsule beyond the NSView bridge spike — the `surface-hosts` [EMBED_SPIKE] seam resolving the UI-thread predicate, the CADisplayLink-paced pump fallback, `RenderingMode` ordering against the host pipeline, and the shared-`GRContext` composite into the host-owned context, then the [WIN32_ROUTE] embed route activating the designed-only host-axis case against the Rhino Windows panel host once the WinForms interoperability host admission lands; viewport-in-panel and host-data-in-shell compositions rehearsed as concept rows (rhino panels, viewports, and host documents inside the one shell that runs standalone), gated by that same spike; every element family re-swept after the embedding spike answers its research items. The bar already set here is the suite bar: any app UI — panel, modal, standalone, companion console — composes from rows with dynamic sourcing and zero host-coupled code.

Testing-infrastructure horizon: the headless render-hash baselines drive `[AvaloniaFact]` UI-thread dispatch through `SetRenderScaling`/`CaptureRenderedFrame`, snapshotting the content-addressed render-hash receipt text under `Verify.XunitV3`, never floating pixels.

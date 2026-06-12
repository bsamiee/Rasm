# [APPUI_FEATURES]

Feature atlas for the product UI rail. Every concept rides SurfaceHost, DataSource, ThemeVariant×Density, TypographyRole, IconSource, ChartSeriesSpec, EditorFactory, DialogIntent, CommandIntent, MotionToken, TableProjection, LocaleRow, and EvidenceReceipt rows — a new concept is a row, never a surface. Mechanics live in `.planning/` pages.

## [1]-[CONCEPTS]

| [INDEX] | [CONCEPT] | [MODALITIES] | [PAGES] |
| :-----: | --------- | ------------ | ------- |
| [1] | One shell mounted on any host (panel, modal, desktop, headless) | all UI | surface-hosts, shell-navigation |
| [2] | Command palette + deep links + remote invocation | all UI | commands-availability |
| [3] | Live-data dashboards over store/host/compute streams | all UI | live-data, charts-dashboards |
| [4] | Benchmark + activity-timeline dashboards | standalone, service-rendered | charts-dashboards, diagnostics-evidence |
| [5] | Document intelligence panels (Markdown + code editing) | panel hosts | typography-shaping, inspector-editing |
| [6] | Document export to PDF/XPS (Skia, zero new render packages) | all | visuals-offscreen |
| [7] | Theme/design-token system with host-matched probing | all UI | theme-tokens |
| [8] | Property inspector with typed value-object editors | all UI | inspector-editing |
| [9] | Virtualized tables + tree hierarchy (flatten-fold) | all UI | tables-hierarchy |
| [10] | Dialog/notification/progress surfaces with motion mapping | all UI | dialogs-notifications, motion-tokens |
| [11] | Accessibility + localization rail (reduced-motion pairs, ICU) | all UI | accessibility, localization-culture |
| [12] | Evidence views + headless render-hash proof derivation | all + test-host | diagnostics-evidence |

## [2]-[CAPABILITY_ROWS]

- Hosts: avalonia-desktop, rhino-panel, rhino-modal, gh2-companion, sidecar-shell, headless; web-browser stays designed-only (TS owns web UI; CommandIntent keys and EvidenceReceipt cases serialize for it today).
- Live data: DynamicData change-set engine (set algebra, aggregation, virtualization, paging, expiry, TransformToTree); SourceCache keys ride the IdentityPolicy vocabulary; the live-data spine composes WatchPhase facts → projections → tag transitions → delta fetch → IChangeSet.
- Controls: free DataGrid + tree-flatten fold (TreeDataGrid rejected), PropertyGrid with Thinktecture value-object editor wiring, Dock layouts with checkpoint cadence + crash restore, PanAndZoom canvases, AvaloniaEdit + TextMate fences, Markdig AST → TypographyRole folds, FluentIcons + SVG icon rows with fallback ranks, AsyncImageLoader rows.
- Motion: MotionToken rows with reduced pairs; ProgressPhase→MotionToken frozen mapping; OKLab perceptual color interpolation.
- Evidence: EvidenceReceipt cases serialize for dashboards; correlation-join (evidence-view fold over three per-package receipt streams) is owned here; headless + Skia render-hash lanes prove visual states in CI.

## [3]-[GAPS_TRACKED]

- Palette fuzzy-search index row, contrast-governance algorithm, grid-edit commit path to StoreOp.Upsert, notification suppression during drain, GeoMap asset dependency, completion-data projection — each closes on its named page; Avalonia-in-Rhino NSView embedding is the bridge-proofed implementation-start gate.

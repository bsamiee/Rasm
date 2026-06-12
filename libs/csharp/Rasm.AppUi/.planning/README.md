# [APPUI_PLANNING]

Rasm.AppUi has zero consumers; the implementation is full-capability with no holding back. These pages are decision-complete blueprints an implementation agent transcribes — never re-designed downstream. The package owns the product UI rail: surface hosts, shell and navigation, screens, commands, live data, tables, inspectors, charts, offscreen visuals, theme and typography, icons, dialogs, input, motion, accessibility, localization, and evidence — consuming AppHost ports, Persistence queries, and Compute receipts as settled vocabulary.

## [1]-[PAGE_INDEX]

| [INDEX] | [PAGE] | [OWNS] | [STATE] |
| :-----: | ------ | ------ | :-----: |
| [1] | [surface-hosts](surface-hosts.md) | SurfaceHost axis; one shell mounted on any host; marshal rows | planned |
| [2] | [shell-navigation](shell-navigation.md) | Shell composition, routing, dock layouts with checkpoint/restore | planned |
| [3] | [screens-activation](screens-activation.md) | Screen family, activation scopes, lifecycle | planned |
| [4] | [commands-availability](commands-availability.md) | One CommandIntent table; availability; hotkeys; palette; deep links | planned |
| [5] | [live-data](live-data.md) | DataSource axis; change-set engine; the live-data spine | planned |
| [6] | [tables-hierarchy](tables-hierarchy.md) | TableProjection rows; virtualization; tree-flatten fold | planned |
| [7] | [inspector-editing](inspector-editing.md) | EditorFactory rows; validation-to-save gate; options inspector | planned |
| [8] | [charts-dashboards](charts-dashboards.md) | ChartSeriesSpec rows incl. geo; dashboard composition | planned |
| [9] | [visuals-offscreen](visuals-offscreen.md) | Offscreen rendering; render-hash; document export | planned |
| [10] | [theme-tokens](theme-tokens.md) | ThemeVariant and Density row families; token resolution | planned |
| [11] | [typography-shaping](typography-shaping.md) | TypographyRole rows; shaping; markdown projection | planned |
| [12] | [icons-assets](icons-assets.md) | IconSource rows with fallback ranks; asset loading | planned |
| [13] | [dialogs-notifications](dialogs-notifications.md) | DialogIntent rows; session algebra; notification policy | planned |
| [14] | [input-interaction](input-interaction.md) | Pointer/keyboard/gesture rows; pan-zoom canvases | planned |
| [15] | [motion-tokens](motion-tokens.md) | MotionToken rows with reduced pairs; phase mapping | planned |
| [16] | [accessibility](accessibility.md) | Automation peers; contrast governance; reduced-motion law | planned |
| [17] | [localization-culture](localization-culture.md) | LocaleRow; resx/ICU/NodaTime culture composition | planned |
| [18] | [diagnostics-evidence](diagnostics-evidence.md) | EvidenceReceipt union; correlation join; headless proof | planned |

## [2]-[WIRE_PAGES]

commands-availability · diagnostics-evidence (each carries exactly one TS_PROJECTION cluster).

## [3]-[CATALOGUE_PENDING]

TextMateSharp grammar registry (transitive, unpinned) gets a page only if admitted; FluentIcons.Common vocabulary rides the api-fluenticons.md surface card.

## [4]-[GAP_LEDGER]

| [INDEX] | [GAP] | [CLOSED_BY] | [STATE] |
| :-----: | ----- | ----------- | :-----: |
| [1] | web-browser SurfaceHost stays designed-only; CommandIntent keys + EvidenceReceipt cases serialize for the TS web layer today | surface-hosts + commands-availability + diagnostics-evidence | OPEN |
| [2] | CommandIntent collapses to ONE table row (intent, availability delegate incl. DegradationLevel input, Option<KeyGesture>, surface predicate); menus/toolbars/palette/deep-links/remote derive | commands-availability | OPEN |
| [3] | ThemeVariant and Density are orthogonal row families composed at resolve time | theme-tokens | OPEN |
| [4] | IconSource fallback order is a rank column | icons-assets | OPEN |
| [5] | Dock layouts gain checkpoint-cadence + crash-restore columns | shell-navigation | OPEN |
| [6] | ProgressPhase-to-MotionToken frozen mapping | motion-tokens | OPEN |
| [7] | Options inspector composite (options to PropertyGrid to user-settings write to ReloadReceipt) | inspector-editing | OPEN |
| [8] | Command-journal replay (headless surface + snapshot store + virtual time) | diagnostics-evidence + surface-hosts | OPEN |
| [9] | Correlation drill-down: the evidence-view fold owns the three-stream join keyed correlation + HLC | diagnostics-evidence | OPEN |
| [10] | MarkdownProjection rows (Markdig AST to TypographyRole fold to inlines; AvaloniaEdit owns code fences) | typography-shaping | OPEN |
| [11] | DocumentExport cluster (SKDocument PDF/XPS through admitted Skia/HarfBuzz; zero new render packages) | visuals-offscreen | OPEN |
| [12] | Model-result provenance projection (fold over receipts keyed by correlation) | diagnostics-evidence | OPEN |
| [13] | Tree-flatten fold over the free DataGrid (TreeDataGrid rejected); grid-edit commit path to StoreOp.Upsert | tables-hierarchy | OPEN |
| [14] | Live-data spine composite (WatchPhase fact to projection to tag transition to delta fetch to IChangeSet); SourceCache keys ride IdentityPolicy | live-data | OPEN |
| [15] | Palette fuzzy-search index row; explicit MessageBus rejection row | commands-availability | OPEN |
| [16] | Contrast governance algorithm row (WCAG luminance gate) | accessibility | OPEN |
| [17] | Notification suppression during drain/suspended phases | dialogs-notifications | OPEN |
| [18] | GeoMap GeoJSON asset dependency routed through icons-assets | charts-dashboards + icons-assets | OPEN |
| [19] | Conflict-resolution inspector row consuming Persistence conflict receipts | inspector-editing | OPEN |
| [20] | ValueObject editor wiring over Thinktecture factories; UnitsNet quantity editors | inspector-editing | OPEN |
| [21] | LocaleRow rides inbox resx/ICU/NodaTime; ICU MessageFormat plural route research row | localization-culture | OPEN |
| [22] | Avalonia-12-in-Rhino NSView embedding is the bridge-proofed implementation-start gate; rhino-panel/modal rows carry the embedding shape | surface-hosts | OPEN |
| [23] | Headless + Skia render-hash proof lanes; EvidenceReceipt serialization for dashboards | diagnostics-evidence + visuals-offscreen | OPEN |
| [24] | Pan-zoom canvas rows (PanAndZoom package surface) with input-interaction gesture law | input-interaction | OPEN |

Sections [DENSITY_BAR], [BUILD_ORDER], [FILE_PROCESS], [PROOF_GATES], [PROHIBITIONS], [ADMISSIONS_RECORD] complete after page finalization.

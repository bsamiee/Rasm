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

Sections [DENSITY_BAR], [BUILD_ORDER], [FILE_PROCESS], [PROOF_GATES], [PROHIBITIONS], [ADMISSIONS_RECORD] complete after page finalization.

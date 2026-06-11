# [RASM_APPUI_ROADMAP]

`Rasm.AppUi` implementation starts from a manifest-backed product UI package and proceeds through one shell/screen/command/live/theme/asset rail.

## [1]-[CURRENT_POSITION]

| [INDEX] | [SURFACE]         | [STATE]                         |
| :-----: | :---------------- | :------------------------------ |
|   [1]   | Project graph     | solution node present           |
|   [2]   | Package graph     | retained UI packages admitted   |
|   [3]   | Host references   | Rhino/GH/Eto awareness active   |
|   [4]   | Production source | absent                          |
|   [5]   | API catalogues    | package lookup pages maintained |

## [2]-[IMPLEMENTATION_TASKS]

[APPUI_FOLDER_ARCHITECTURE]:
- Status: QUEUED
- Exit: owner folders, rail entrypoints, host adapters, generated shapes, receipts, scheduler boundaries, assets, tokens, and diagnostics are planned before production source.
- Proof: architecture plan consumes every AppUi package and host assembly API catalogue and names the UI rail owners.

[APPUI_SCHEDULER]:
- Status: QUEUED
- Exit: Avalonia dispatcher, ReactiveUI scheduler, and host-thread affinity enter through one boundary.
- Proof: managed scheduler specs and Rhino/GH host scenario receipts.

[APPUI_SHELL_SCREEN]:
- Status: QUEUED
- Exit: shell route, nav stack, screen activation, validation, command availability, and live binding enter one rail.
- Proof: managed UI rail specs and headless Avalonia specs.

[APPUI_ASSETS_THEME]:
- Status: QUEUED
- Exit: Fluent theme, token catalogues, typography roles, embedded fonts, SVG/path assets, provider-generated assets, and custom assets enter one asset rail.
- Proof: asset catalogue generation, native identity receipts, and theme token specs.

[APPUI_HOST_ADAPTERS]:
- Status: QUEUED
- Exit: Rhino panels, GH2 canvas/popup/component/overlay surfaces, companion windows, sidecar shells, and downstream shells differ by host adapter only.
- Proof: host load, focus, scale, screenshot, accessibility, and disposal receipts.

## [3]-[CATALOGUE_USE]

[RETAINED_CATALOGUES]:
- Status: REQUIRED
- Action: retained UI design consumes Avalonia shell, desktop, fluent, font, grid, and color catalogues.
- Exit: UI owners name shell root, retained state, screen ownership, theme policy, and asset admission.

[REACTIVE_CATALOGUES]:
- Status: REQUIRED
- Action: reactive design consumes ReactiveUI, validation, DynamicData, and System.Reactive catalogues.
- Exit: UI owners name activation, command availability, validation state, stream ownership, and disposal receipts.

[VISUAL_CATALOGUES]:
- Status: REQUIRED
- Action: visual design consumes LiveCharts, SkiaSharp, HarfBuzz, SVG, and native asset catalogues.
- Exit: UI owners name chart state, drawing paths, text shaping, SVG asset policy, and native identity receipts.

[CONTROL_CATALOGUES]:
- Status: REQUIRED
- Action: control design consumes property-grid, dialog-host, and behavior catalogues.
- Exit: UI owners name editor surfaces, modal ownership, behavior admission, and command routing.

[HOST_CATALOGUES]:
- Status: REQUIRED
- Action: host design consumes Rhino, Rhino UI, GH2, GrasshopperIO, Eto, System.Drawing, and macOS catalogues.
- Exit: adapters name host classification, focus, scaling, screenshot, disposal, and boundary receipts.

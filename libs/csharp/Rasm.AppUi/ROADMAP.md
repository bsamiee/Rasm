# [RASM_APPUI_ROADMAP]

`Rasm.AppUi` implementation starts from a manifest-backed product UI package and proceeds through one shell/screen/command/live/theme/asset rail.

## [1]-[CURRENT_POSITION]

This table is a lookup by implementation surface.

| [INDEX] | [SURFACE]         | [STATE]                         |
| :-----: | :---------------- | :------------------------------ |
|   [1]   | Project graph     | solution node present           |
|   [2]   | Package graph     | retained UI packages admitted   |
|   [3]   | Host references   | Rhino/GH/Eto awareness active   |
|   [4]   | Production source | absent                          |
|   [5]   | API catalogues    | package lookup pages maintained |

## [2]-[IMPLEMENTATION_TASKS]

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

## [3]-[PACKAGE_PROOF]

This table is a lookup by package rail.

| [INDEX] | [RAIL]       | [REQUIRED_STATE]                          |
| :-----: | :----------- | :---------------------------------------- |
|   [1]   | Retained     | Avalonia shell and control packages       |
|   [2]   | Reactive     | activation, commands, validation, streams |
|   [3]   | Visuals      | charts, Skia, SVG, native assets          |
|   [4]   | Controls     | property grid, dialogs, behaviors         |
|   [5]   | Host         | Rhino/GH/Eto/System.Drawing boundaries    |

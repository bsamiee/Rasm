# [RASM_APPUI_API_HOST]

Host APIs supply AppUi project contracts, Rhino/GH/Eto compile surfaces, System.Drawing compile support, native host handles, repaint, focus, scale, screenshot, and disposal evidence.

## [1]-[SURFACES]

This table is a lookup by host surface.

| [INDEX] | [PACKAGE]             | [ASSEMBLY]          | [LOCAL_RAIL] |
| :-----: | :-------------------- | :------------------ | :----------- |
|   [1]   | project reference     | `Rasm`              | kernel       |
|   [2]   | project reference     | `Rasm.AppHost`      | runtime      |
|   [3]   | project reference     | `Rasm.Compute`      | progress     |
|   [4]   | project reference     | `Rasm.Persistence`  | state        |
|   [5]   | project reference     | `Rasm.Rhino`        | rhino        |
|   [6]   | project reference     | `Rasm.Grasshopper`  | gh2          |
|   [7]   | host reference        | `Rhino.UI`          | rhino        |
|   [8]   | host reference        | `Eto`               | rhino        |
|   [9]   | host reference        | `Grasshopper2`      | gh2          |
|  [10]   | compile package       | `System.Drawing.Common` | host compile |

## [2]-[API_LOCATORS]

This table is a lookup by assembly.

| [INDEX] | [ASSEMBLY]          | [NAMESPACE]             | [USING]                  | [API_LOCATOR] |
| :-----: | :------------------ | :---------------------- | :----------------------- | :------------ |
|   [1]   | `Rasm.Rhino`        | `Rasm.Rhino.UI`         | `Rasm.Rhino.UI`          | `libs/csharp/Rasm.Rhino/` |
|   [2]   | `Rasm.Grasshopper`  | `Rasm.Grasshopper.UI`   | `Rasm.Grasshopper.UI`    | `libs/csharp/Rasm.Grasshopper/` |
|   [3]   | `Rhino.UI`          | `Rhino.UI`              | `Rhino.UI`               | RhinoWIP resources |
|   [4]   | `Eto`               | `Eto.Forms`             | `Eto.Forms`              | RhinoWIP resources |
|   [5]   | `Grasshopper2`      | `Grasshopper2.UI`       | `Grasshopper2.UI`        | RhinoWIP resources |
|   [6]   | `System.Drawing.Common` | `System.Drawing`    | `System.Drawing`         | `.cache/nuget/packages/system.drawing.common/` |

## [3]-[CAPABILITIES]

This table is a lookup by type family.

| [INDEX] | [TYPE_FAMILY]        | [ENTRY_SURFACE]            | [LOCAL_RAIL] |
| :-----: | :------------------- | :------------------------- | :----------- |
|   [1]   | Rhino UI rail types  | panel, window, HUD intent  | rhino        |
|   [2]   | GH2 UI rail types    | canvas, popup, toolbar     | gh2          |
|   [3]   | Eto forms            | host panel parent          | rhino        |
|   [4]   | native handle APIs   | NSView and panel handle    | host         |
|   [5]   | repaint APIs         | viewport and canvas redraw | host         |
|   [6]   | focus APIs           | first responder evidence   | diagnostics  |
|   [7]   | bitmap APIs          | screenshot compile support | diagnostics  |

## [4]-[REJECTED]

This table is a lookup by rejected host pattern.

| [INDEX] | [REJECT]                  | [LOCAL_RAIL] | [REASON]                 |
| :-----: | :------------------------ | :----------- | :----------------------- |
|   [1]   | direct viewport Avalonia  | host         | display conduit owns overlay |
|   [2]   | independent `NSWindow` UI | shell        | shell rail owns surfaces |
|   [3]   | AppUi undo stack          | command      | host mutation rail owns undo |

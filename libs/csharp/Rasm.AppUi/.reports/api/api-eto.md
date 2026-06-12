# [RASM_APPUI_API_ETO]

`Eto` supplies host-native forms, dialogs, controls, layouts, commands, drawing, images, colors, and application dispatch used by Rhino and Grasshopper boundary adapters.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: host assembly `Eto`
- package: `Eto`
- assembly: `Eto`
- namespace: `Eto`
- namespace: `Eto.Forms`
- namespace: `Eto.Drawing`
- asset: host assembly
- rail: host-eto

## [2]-[PUBLIC_TYPES]

[FORM_TYPES]: host-native forms and controls
- rail: host-eto

| [INDEX] | [SYMBOL]                  | [RAIL]         |
| :-----: | :------------------------ | :------------- |
|   [1]   | `Eto.Forms.Application`   | app dispatch   |
|   [2]   | `Eto.Forms.Form`          | host form      |
|   [3]   | `Eto.Forms.Dialog`        | modal dialog   |
|   [4]   | `Eto.Forms.Control`       | control base   |
|   [5]   | `Eto.Forms.Panel`         | panel surface  |
|   [6]   | `Eto.Forms.DynamicLayout` | layout surface |
|   [7]   | `Eto.Forms.Button`        | command button |
|   [8]   | `Eto.Forms.TextBox`       | text input     |
|   [9]   | `Eto.Forms.GridView`      | grid control   |
|  [10]   | `Eto.Forms.TreeGridView`  | tree grid      |

[DRAWING_TYPES]: host drawing and image surface
- rail: host-eto

| [INDEX] | [SYMBOL]               | [RAIL]          |
| :-----: | :--------------------- | :-------------- |
|   [1]   | `Eto.Drawing.Bitmap`   | bitmap image    |
|   [2]   | `Eto.Drawing.Image`    | image base      |
|   [3]   | `Eto.Drawing.Icon`     | icon image      |
|   [4]   | `Eto.Drawing.Graphics` | draw context    |
|   [5]   | `Eto.Drawing.Color`    | color value     |
|   [6]   | `Eto.Drawing.Brush`    | fill resource   |
|   [7]   | `Eto.Drawing.Pen`      | stroke resource |
|   [8]   | `Eto.Drawing.Font`     | font resource   |
|   [9]   | `Eto.Drawing.Point`    | point value     |
|  [10]   | `Eto.Drawing.Size`     | size value      |

## [3]-[ENTRYPOINTS]

[FORM_ENTRYPOINTS]: form, dialog, and control operations
- rail: host-eto

| [INDEX] | [SURFACE]    | [SURFACE_ROOT]  | [RAIL]         |
| :-----: | :----------- | :-------------- | :------------- |
|   [1]   | `Show`       | `Form`          | form open      |
|   [2]   | `ShowModal`  | `Dialog`        | modal open     |
|   [3]   | `Close`      | `Form`          | form close     |
|   [4]   | `Invalidate` | `Control`       | redraw request |
|   [5]   | `Content`    | `Panel`         | child content  |
|   [6]   | `Padding`    | `DynamicLayout` | layout padding |
|   [7]   | `Spacing`    | `DynamicLayout` | layout spacing |
|   [8]   | `Invoke`     | `Application`   | UI dispatch    |
|   [9]   | `Command`    | `Button`        | command source |

[DRAWING_ENTRYPOINTS]: bitmap, color, and graphics operations
- rail: host-eto

| [INDEX] | [SURFACE]      | [SURFACE_ROOT] | [RAIL]          |
| :-----: | :------------- | :------------- | :-------------- |
|   [1]   | `FromResource` | `Bitmap`       | bitmap resource |
|   [2]   | `Lock`         | `Bitmap`       | pixel lock      |
|   [3]   | `Save`         | `Bitmap`       | bitmap write    |
|   [4]   | `ToByteArray`  | `Bitmap`       | bitmap encode   |
|   [5]   | `GetPixel`     | `Bitmap`       | pixel read      |
|   [6]   | `SetPixel`     | `Bitmap`       | pixel write     |
|   [7]   | `FromArgb`     | `Color`        | color create    |
|   [8]   | `TryParse`     | `Color`        | color parse     |
|   [9]   | `ToHex`        | `Color`        | color format    |

## [4]-[IMPLEMENTATION_LAW]

[HOST_ETO_LAW]:
- Package: `Eto`
- Owns: host-native forms, dialogs, controls, layouts, commands, and drawing interop
- Accept: Eto stays at Rhino and GH2 host boundaries
- Reject: Eto replacement for product UI

[BOUNDARY_LAW]:
- Package: `Eto`
- Owns: native host handoff where Rhino/GH2 require Eto surfaces
- Accept: AppUi product screens remain Avalonia-first and project into Eto only through adapters
- Reject: parallel Eto product UI rail

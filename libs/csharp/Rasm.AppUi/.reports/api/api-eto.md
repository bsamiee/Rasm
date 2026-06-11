# [RASM_APPUI_API_ETO]

`Eto` supplies host-native controls and forms used by Rhino and Grasshopper boundary adapters.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: host assembly `Eto`
- host_assembly: `Eto`
- assembly: `Eto`
- namespace: `Eto`
- asset: host assembly
- rail: host-eto

## [2]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: Eto family
- rail: host-eto

| [INDEX] | [SYMBOL]                  | [PACKAGE_ROLE]  | [CAPABILITY]              |
| :-----: | :------------------------ | :-------------- | :------------------------ |
|   [1]   | `Eto.Forms.Form`          | host form       | anchors host-eto contract |
|   [2]   | `Eto.Forms.Dialog`        | UI surface      | renders product surface   |
|   [3]   | `Eto.Forms.Control`       | UI surface      | renders product surface   |
|   [4]   | `Eto.Forms.Panel`         | UI surface      | renders product surface   |
|   [5]   | `Eto.Forms.DynamicLayout` | host layout     | anchors host-eto contract |
|   [6]   | `Eto.Drawing.Bitmap`      | drawing surface | draws visual evidence     |
|   [7]   | `Eto.Drawing.Color`       | drawing surface | draws visual evidence     |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: Eto operations
- rail: host-eto

| [INDEX] | [SURFACE]    | [CALL_SHAPE]     | [CAPABILITY]             |
| :-----: | :----------- | :--------------- | :----------------------- |
|   [1]   | `Show`       | display method   | opens surface            |
|   [2]   | `ShowModal`  | modal display    | opens modal surface      |
|   [3]   | `Close`      | close method     | closes surface           |
|   [4]   | `Invalidate` | member surface   | drives host-eto behavior |
|   [5]   | `Content`    | property surface | binds surface state      |
|   [6]   | `Padding`    | layout property  | sets host spacing        |
|   [7]   | `Spacing`    | layout property  | sets layout gap          |

## [4]-[IMPLEMENTATION_LAW]

[RAIL_LAW]:
- Package: `Eto`
- Owns: host-native UI interop
- Accept: Eto stays at host boundary
- Reject: Eto replacement for product UI

